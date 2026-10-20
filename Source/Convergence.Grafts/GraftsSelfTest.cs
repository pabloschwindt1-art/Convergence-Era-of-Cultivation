using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Convergence.Grafts
{
    /// <summary>
    /// Autotest de los injertos. Solo con <c>-cvggraftstest</c> (y un mapa, p. ej. -quicktest): comprueba las recetas de fabricación e instalación,
    /// que sin altar no se pueden instalar, que con altar sí, y que cada injerto instalado cambia los stats que dice. Inerte sin el argumento.
    /// </summary>
    public class GameComponent_GraftsTest : GameComponent
    {
        private static readonly bool Enabled = GenCommandLine.CommandLineArgPassed("cvggraftstest");
        private bool done;
        private readonly StringBuilder sb = new StringBuilder("[Convergence] Grafts self-test");
        private int fails;

        public GameComponent_GraftsTest(Game game) { }

        private void Check(bool ok, string what)
        {
            sb.Append("\n    " + (ok ? "ok   " : "FAIL ") + what);
            if (!ok) fails++;
        }

        public override void GameComponentTick()
        {
            if (!Enabled || done) return;
            if (Find.CurrentMap == null || Find.TickManager.TicksGame < 200) return;
            done = true;
            try { Run(Find.CurrentMap); }
            catch (Exception e) { Check(false, "exception: " + e); }
            sb.Append(fails == 0 ? "\n  ALL OK" : "\n  FAILURES: " + fails);
            if (fails == 0) Log.Message(sb.ToString()); else Log.Error(sb.ToString());
        }

        private static IEnumerable<RecipeDef> InstallRecipes() =>
            DefDatabase<RecipeDef>.AllDefsListForReading.Where(r => r.defName.StartsWith("CVG_Install_"));

        private void Run(Map map)
        {
            var recipes = InstallRecipes().ToList();
            Check(recipes.Count >= 12, recipes.Count + " install recipes");
            var furnace = DefDatabase<ThingDef>.GetNamed("CVG_BoneRefiningFurnace");
            var altarDef = DefDatabase<ThingDef>.GetNamed("CVG_GraftingAltar");

            sb.Append("\n- crafting recipes");
            foreach (var r in recipes)
            {
                var hediff = r.addsHediff;
                var item = DefDatabase<ThingDef>.GetNamed(r.defName.Replace("CVG_Install_", "CVG_Graft_"));
                var make = DefDatabase<RecipeDef>.GetNamedSilentFail("Make_" + item.defName);
                Check(make != null && make.recipeUsers != null && make.recipeUsers.Contains(furnace), "Make_" + item.defName + " at the furnace");
                if (!hediff.stages.NullOrEmpty())
                    Check(hediff.stages[0].statOffsets != null && hediff.stages[0].statOffsets.Any(o => o.stat == CVG_DefOf.CVG_CelestialDissonance && o.value > 0f && o.value <= 0.3f), hediff.defName + " has small celestial dissonance");
            }

            sb.Append("\n- altar requirement");
            var testPawn = NewPawn(map);
            var anyRecipe = recipes.First(r => r.defName.EndsWith("FarmerArm"));
            Check(!anyRecipe.Worker.AvailableReport(testPawn, testPawn.RaceProps.body.GetPartsWithDef(BodyPartDefOf.Shoulder).First()).Accepted, "install disabled without an altar");
            var altar = ThingMaker.MakeThing(altarDef);
            altar.SetFactionDirect(Faction.OfPlayer);
            GenSpawn.Spawn(altar, CellFinder.RandomClosewalkCellNear(map.Center, map, 12), map);
            Check(anyRecipe.Worker.AvailableReport(testPawn, testPawn.RaceProps.body.GetPartsWithDef(BodyPartDefOf.Shoulder).First()).Accepted, "install available with an altar");
            Check(altarDef.GetStatValueAbstract(StatDefOf.SurgerySuccessChanceFactor) > 1.05f, "altar boosts surgery success");
            testPawn.Destroy();

            sb.Append("\n- installing every graft");
            foreach (var r in recipes)
            {
                var pawn = NewPawn(map);
                try
                {
                    var hediff = r.addsHediff;
                    var before = new Dictionary<StatDef, float>();
                    var offsets = hediff.stages?[0].statOffsets ?? new List<StatModifier>();
                    foreach (var o in offsets) before[o.stat] = pawn.GetStatValue(o.stat);
                    var parts = r.Worker.GetPartsToApplyOn(pawn, r).ToList();
                    if (parts.Count == 0) { Check(false, r.defName + " has no valid part"); continue; }
                    var item = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed(r.defName.Replace("CVG_Install_", "CVG_Graft_")));
                    r.Worker.ApplyOnPawn(pawn, parts[0], null, new List<Thing> { item }, null);
                    bool installed = pawn.health.hediffSet.HasHediff(hediff);
                    bool statsOk = true;
                    string detail = "";
                    foreach (var o in offsets)
                    {
                        if (o.value > 0f && (before[o.stat] >= o.stat.maxValue - 0.001f || before[o.stat] <= 0.0001f)) continue;   // al tope o incapaz (p. ej. no puede minar)
                        float delta = pawn.GetStatValue(o.stat) - before[o.stat];
                        if (o.value > 0f ? delta <= 0.001f : delta >= -0.001f) { statsOk = false; detail += $" {o.stat.defName}:{delta:0.##}/{o.value}"; }
                    }
                    Check(installed && statsOk, r.defName + (installed ? "" : " not installed") + detail);
                }
                catch (Exception e) { Check(false, r.defName + " threw: " + e.Message); }
                finally { if (!pawn.Destroyed) pawn.Destroy(); }
            }
            altar.Destroy();
        }

        private static Pawn NewPawn(Map map)
        {
            var p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayer, PawnGenerationContext.PlayerStarter, -1, forceGenerateNewPawn: true));
            GenSpawn.Spawn(p, CellFinder.RandomClosewalkCellNear(map.Center, map, 10), map);
            return p;
        }
    }
}
