using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Auditoría de alcanzabilidad desde el lado del jugador: con <c>-cvgreach</c> (sin <c>-quicktest</c>) revisa al terminar de
    /// cargar que cada edificio CVG se pueda construir (categoría, investigación investigable, materiales conseguibles), que cada
    /// receta tenga una mesa construible que alguien atienda (WorkGiver de facturas), que cada investigación tenga su mesa, y que
    /// cada objeto CVG tenga alguna fuente (receta, mina, planta, animal, comerciante, veta profunda, carnicería/fundición).
    /// Escribe <c>&lt;datos&gt;/cvg_reach.txt</c> y cierra el juego. Sin el argumento no hace nada.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class ReachabilityAudit
    {
        static ReachabilityAudit()
        {
            if (!GenCommandLine.CommandLineArgPassed("cvgreach")) return;
            LongEventHandler.ExecuteWhenFinished(Run);
        }

        private static readonly Dictionary<ThingDef, string> sourceCache = new Dictionary<ThingDef, string>();
        private static readonly HashSet<ThingDef> visiting = new HashSet<ThingDef>();
        private static List<WorkGiverDef> billGivers;

        private static bool Ours(Def d) => d.defName.StartsWith("CVG_");

        public static void Run()
        {
            var errors = new List<string>();
            var warnings = new List<string>();
            billGivers = DefDatabase<WorkGiverDef>.AllDefs.Where(w => w.giverClass != null && typeof(WorkGiver_DoBill).IsAssignableFrom(w.giverClass)).ToList();

            foreach (var d in DefDatabase<ThingDef>.AllDefs.Where(Ours))
            {
                if (d.BuildableByPlayer)
                {
                    if (d.designationCategory == null) errors.Add($"{d.defName}: construible sin categoría de construcción");
                    foreach (var r in d.researchPrerequisites ?? new List<ResearchProjectDef>())
                        if (!ResearchReachable(r, out string why)) errors.Add($"{d.defName}: su investigación {r.defName} no se puede investigar ({why})");
                    foreach (var c in d.costList ?? new List<ThingDefCountClass>())
                        if (Source(c.thingDef) == null) errors.Add($"{d.defName}: material {c.thingDef.defName} sin fuente conocida");
                    if (d.MadeFromStuff && !GenStuff.AllowedStuffsFor(d).Any()) errors.Add($"{d.defName}: ningún material (stuff) permitido");
                    if (d.IsWorkTable && !d.AllRecipes.NullOrEmpty() && !Attended(d))
                        errors.Add($"{d.defName}: mesa con recetas que ningún trabajo atiende (falta WorkGiver_DoBill con esta mesa)");
                }
            }

            foreach (var rec in DefDatabase<RecipeDef>.AllDefs.Where(Ours))
            {
                var users = rec.AllRecipeUsers.ToList();
                if (users.Count == 0) errors.Add($"receta {rec.defName}: ninguna mesa la tiene");
                else if (!users.Any(u => u.BuildableByPlayer || u.race != null)) errors.Add($"receta {rec.defName}: solo en mesas no construibles ({string.Join(", ", users.Select(u => u.defName))})");
                else if (!users.Any(u => u.race != null || Attended(u))) errors.Add($"receta {rec.defName}: ninguna de sus mesas la atiende un trabajo");
                if (rec.researchPrerequisite != null && !ResearchReachable(rec.researchPrerequisite, out string why))
                    errors.Add($"receta {rec.defName}: investigación {rec.researchPrerequisite.defName} no investigable ({why})");
                foreach (var ing in rec.ingredients)
                {
                    var allowed = DefDatabase<ThingDef>.AllDefs.Where(t => ing.filter.Allows(t)).ToList();
                    if (allowed.Count == 0) errors.Add($"receta {rec.defName}: un ingrediente no admite ningún objeto");
                    else if (!allowed.Any(t => Source(t) != null)) errors.Add($"receta {rec.defName}: ningún ingrediente conseguible ({string.Join(", ", allowed.Take(3).Select(t => t.defName))})");
                }
            }

            foreach (var r in DefDatabase<ResearchProjectDef>.AllDefs.Where(Ours))
                if (!ResearchReachable(r, out string why)) errors.Add($"investigación {r.defName}: {why}");

            foreach (var d in DefDatabase<ThingDef>.AllDefs.Where(t => Ours(t) && t.category == ThingCategory.Item && !t.IsCorpse && !t.isUnfinishedThing && !t.IsBlueprint && !t.IsFrame))
            {
                string s = Source(d);
                if (s == null) warnings.Add($"objeto {d.defName} ({d.label}): sin fuente encontrada (¿solo eventos o misiones?)");
            }

            var sb = new StringBuilder();
            sb.AppendLine($"[Convergence] Reachability: {errors.Count} error(es), {warnings.Count} aviso(s)");
            foreach (var e in errors) sb.AppendLine("  ERROR " + e);
            foreach (var w in warnings) sb.AppendLine("  aviso " + w);
            string path = Path.Combine(GenFilePaths.SaveDataFolderPath, "cvg_reach.txt");
            File.WriteAllText(path, sb.ToString());
            Log.Message(sb.ToString().Split('\n')[0] + " -> " + path);
            Root.Shutdown();
        }

        private static bool Attended(ThingDef bench) =>
            billGivers.Any(w => w.fixedBillGiverDefs != null && w.fixedBillGiverDefs.Contains(bench))
            || (bench.thingClass != null && typeof(Building_WorkTable).IsAssignableFrom(bench.thingClass) && billGivers.Any(w => w.billGiversAllHumanlikes || w.billGiversAllMechanoids));

        private static bool ResearchReachable(ResearchProjectDef r, out string why)
        {
            why = null;
            var seen = new HashSet<ResearchProjectDef>();
            var stack = new Stack<ResearchProjectDef>(); stack.Push(r);
            while (stack.Count > 0)
            {
                var p = stack.Pop();
                if (!seen.Add(p)) continue;
                if (p.requiredResearchBuilding != null && !p.requiredResearchBuilding.BuildableByPlayer)
                { why = $"{p.defName} pide la mesa {p.requiredResearchBuilding.defName}, que no se construye"; return false; }
                if (p.TechprintCount > 0 && p.Techprint != null && Source(p.Techprint) == null)
                { why = $"{p.defName} pide un techprint sin fuente"; return false; }
                foreach (var q in p.prerequisites ?? new List<ResearchProjectDef>()) stack.Push(q);
                foreach (var q in p.hiddenPrerequisites ?? new List<ResearchProjectDef>()) stack.Push(q);
            }
            return true;
        }

        /// <summary>Dónde se consigue un objeto (o null). Lo vanilla sin "CVG_" se da por conseguible.</summary>
        public static string Source(ThingDef t)
        {
            if (t == null) return null;
            if (!Ours(t)) return "vanilla";
            if (sourceCache.TryGetValue(t, out var cached)) return cached;
            if (!visiting.Add(t)) return null;
            string s = null;
            foreach (var rec in DefDatabase<RecipeDef>.AllDefsListForReading)
                if (rec.products != null && rec.products.Any(p => p.thingDef == t) && rec.AllRecipeUsers.Any(u => u.BuildableByPlayer || u.race != null))
                { s = "receta " + rec.defName; break; }
            if (s == null)
                foreach (var d in DefDatabase<ThingDef>.AllDefsListForReading)
                {
                    if (d.building?.mineableThing == t) { s = "mina " + d.defName; break; }
                    if (d.plant?.harvestedThingDef == t) { s = "planta " + d.defName; break; }
                    if (d.race != null && (d.race.meatDef == t || d.race.leatherDef == t)) { s = "animal " + d.defName; break; }
                    if (d.butcherProducts != null && d.butcherProducts.Any(b => b.thingDef == t)) { s = "carnicería " + d.defName; break; }
                    if (d.smeltProducts != null && d.smeltProducts.Any(b => b.thingDef == t)) { s = "fundición " + d.defName; break; }
                    if (d.killedLeavings != null && d.killedLeavings.Any(b => b.thingDef == t)) { s = "restos " + d.defName; break; }
                    if (d.comps != null && d.comps.Any(c => c is CompProperties_Milkable m && m.milkDef == t || c is CompProperties_Shearable sh && sh.woolDef == t
                        || c is CompProperties_EggLayer e && (e.eggFertilizedDef == t || e.eggUnfertilizedDef == t) || c is CompProperties_Spawner sp && sp.thingToSpawn == t))
                    { s = "producto de " + d.defName; break; }
                }
            if (s == null && t.deepCommonality > 0) s = "veta profunda";
            if (s == null && t.tradeability != Tradeability.None)
                foreach (var tk in DefDatabase<TraderKindDef>.AllDefsListForReading)
                {
                    bool sells = false;
                    try { sells = tk.stockGenerators != null && tk.stockGenerators.Any(g => g != null && g.HandlesThingDef(t)); } catch { }
                    if (sells) { s = "comerciante " + tk.defName; break; }
                }
            visiting.Remove(t);
            sourceCache[t] = s;
            return s;
        }
    }
}
