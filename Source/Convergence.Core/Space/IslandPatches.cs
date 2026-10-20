using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Convergence
{
    // Isla voladora (docs/modulos/ESPACIO.md): el Corazón de la Isla es un motor grav de Odyssey con otro ThingDef, misma clase (Building_GravEngine).
    // Casi todo en Odyssey es por clase/componente (instalaciones, subestructura, propulsores, combustible), pero unos pocos sitios buscan el motor
    // por ThingDefOf.GravEngine o la consola por ThingDefOf.PilotConsole. Estos parches mínimos lo cubren; todo lo demás es XML.

    /// <summary>En el ThingDef de un motor grav alternativo: para el juego cuenta como <c>ThingDefOf.GravEngine</c>.</summary>
    public class DefModExtension_GravEngineAlias : DefModExtension
    {
    }

    /// <summary>En el ThingDef de un ancla alternativa: para el juego cuenta como <c>ThingDefOf.GravAnchor</c> (evita que el mapa se destruya al zarpar).</summary>
    public class DefModExtension_GravAnchorAlias : DefModExtension
    {
    }

    public static class IslandUtility
    {
        private static List<ThingDef> anchorAliases;

        public static List<ThingDef> AnchorAliases =>
            anchorAliases ??= DefDatabase<ThingDef>.AllDefsListForReading.Where(d => d.HasModExtension<DefModExtension_GravAnchorAlias>()).ToList();

        private static List<ThingDef> aliases;

        public static List<ThingDef> Aliases =>
            aliases ??= DefDatabase<ThingDef>.AllDefsListForReading.Where(d => d.HasModExtension<DefModExtension_GravEngineAlias>()).ToList();
    }

    [HarmonyPatch(typeof(ListerThings), nameof(ListerThings.ThingsOfDef))]
    public static class Patch_ListerThings_ThingsOfDef_GravEngine
    {
        public static void Postfix(ListerThings __instance, ThingDef def, ref List<Thing> __result)
        {
            if (def == null || def != ThingDefOf.GravEngine || IslandUtility.Aliases.Count == 0) return;
            List<Thing> merged = null;
            foreach (var alias in IslandUtility.Aliases)
            {
                var extra = __instance.ThingsOfDef(alias);
                if (extra.Count == 0) continue;
                merged ??= new List<Thing>(__result);
                merged.AddRange(extra);
            }
            if (merged != null) __result = merged;
        }
    }

    /// <summary>
    /// El motor grav vanilla hay que "inspeccionarlo" antes de usarlo (un colono lo estudia y eso lanza la misión de los mecanoides). El Corazón de la Isla se
    /// construye, no se encuentra: al aparecer ya cuenta como inspeccionado (sin misión ni carta), y así no pide "enviar a un colono a inspeccionarlo", se
    /// puede bautizar la isla y ya no hay nada de motor misterioso.
    /// </summary>
    [HarmonyPatch(typeof(Building_GravEngine), "SpawnSetup")]
    public static class Patch_GravEngine_SpawnSetup_Inspected
    {
        public static void Postfix(Building_GravEngine __instance)
        {
            if (__instance.def.HasModExtension<DefModExtension_GravEngineAlias>() && !Find.ResearchManager.gravEngineInspected)
                Find.ResearchManager.gravEngineInspected = true;
        }
    }

    /// <summary>Lo mismo que <c>ThingsOfDef</c> para <c>AnyThingWithDef</c>: el Corazón de la Isla bloquea el borrado del mapa y el ancla del cielo cuenta como ancla de gravedad (avisos de lanzamiento, abandono del mapa, aterrizaje).</summary>
    [HarmonyPatch(typeof(ListerThings), nameof(ListerThings.AnyThingWithDef))]
    public static class Patch_ListerThings_AnyThingWithDef_GravEngine
    {
        public static void Postfix(ListerThings __instance, ThingDef def, ref bool __result)
        {
            if (__result || def == null) return;
            var list = def == ThingDefOf.GravEngine ? IslandUtility.Aliases : def == ThingDefOf.GravAnchor ? IslandUtility.AnchorAliases : null;
            if (list == null) return;
            foreach (var alias in list)
                if (__instance.AnyThingWithDef(alias)) { __result = true; return; }
        }
    }

    /// <summary>El ritual de lanzamiento busca sus consolas con <c>ThingDefOf.PilotConsole</c>: sin esto el altar del timonel no aparecía como objetivo del ritual.</summary>
    [HarmonyPatch(typeof(RitualObligationTargetWorker_GravshipLaunch), nameof(RitualObligationTargetWorker_GravshipLaunch.GetTargets))]
    public static class Patch_RitualTargetGravshipLaunch_Consoles
    {
        public static void Postfix(Map map, ref IEnumerable<TargetInfo> __result)
        {
            var extra = new List<TargetInfo>();
            foreach (var t in map.listerThings.AllThings)
                if (t.def != ThingDefOf.PilotConsole && t.TryGetComp<CompPilotConsole>() is CompPilotConsole c && c.CanUseNow().Accepted) extra.Add(t);
            if (extra.Count > 0) __result = __result.Concat(extra);
        }
    }

    /// <summary>La consola del gravbarco: el juego la reconoce por su def; las nuestras la reconocen por tener <c>CompPilotConsole</c>.</summary>
    [HarmonyPatch(typeof(Gravship), "AddThing")]
    public static class Patch_Gravship_AddThing_Console
    {
        private static readonly AccessTools.FieldRef<Gravship, Building> ConsoleRef = AccessTools.FieldRefAccess<Gravship, Building>("pilotConsole");

        public static void Postfix(Gravship __instance, Thing thing)
        {
            if (thing is Building b && ConsoleRef(__instance) == null && b.def != ThingDefOf.PilotConsole && b.TryGetComp<CompPilotConsole>() != null)
                ConsoleRef(__instance) = b;
        }
    }

    /// <summary>
    /// <c>CompPilotConsole.CanUseNow</c> da por hecho que la consola tiene <c>CompBreakdownable</c> (la de vanilla lo tiene). El altar del timonel no se avería
    /// (repararlo exigiría componentes industriales en una colonia medieval), así que sin ese componente se salta la comprobación en vez de lanzar una
    /// NullReferenceException que dejaba la consola inutilizable: ni opción de pilotar, ni ritual, ni lanzamiento.
    /// </summary>
    [HarmonyPatch(typeof(CompPilotConsole), nameof(CompPilotConsole.CanUseNow))]
    public static class Patch_CompPilotConsole_CanUseNow_NoBreakdown
    {
        public static bool Prefix(CompPilotConsole __instance, ref AcceptanceReport __result)
        {
            if (__instance.parent.GetComp<CompBreakdownable>() != null) return true;
            var engine = __instance.engine;
            if (engine == null) { __result = new AcceptanceReport("CannotLaunchNoEngine".Translate().CapitalizeFirst()); return false; }
            if (!engine.ValidSubstructureAt(__instance.parent.InteractionCell)) { __result = new AcceptanceReport("PilotConsoleInaccessible".Translate().CapitalizeFirst()); return false; }
            var result = engine.CanLaunch(__instance);
            __result = result.Accepted ? AcceptanceReport.WasAccepted : result;
            return false;
        }
    }

    /// <summary>Alcance del gravbarco por el reino del cultivador más fuerte de la colonia (altar del timonel): <c>perRealmLevel</c> por nivel.</summary>
    public class StatPart_HelmsmanRealm : StatPart
    {
        public float perRealmLevel = 3f;

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!(req.Thing is Building_GravEngine engine) || !engine.def.HasModExtension<DefModExtension_GravEngineAlias>()) return;
            val += perRealmLevel * CultivatorPawnKindUtility.ColonyMaxRealm();
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!(req.Thing is Building_GravEngine engine) || !engine.def.HasModExtension<DefModExtension_GravEngineAlias>()) return null;
            int level = CultivatorPawnKindUtility.ColonyMaxRealm();
            return level <= 0 ? null : "CVG_Island_HelmsmanRealm".Translate(level, (perRealmLevel * level).ToString("+0.#")).ToString();
        }
    }
}

namespace Convergence
{
    /// <summary>Qi celestial en órbita (ESPACIO.md): en mapas de vacío (espacio de Odyssey) el qi ambiental es más puro y la concentración sube.</summary>
    [Verse.StaticConstructorOnStartup]
    public static class OrbitalQi
    {
        public const float Bonus = 2f;

        static OrbitalQi() => QiConcentrationUtility.AreaProviders.Add((cell, map) => BonusFor(map));

        public static float BonusFor(Verse.Map map) => map?.Biome != null && map.Biome.inVacuum && GameComponent_Convergence.Active ? Bonus : 0f;
    }
}
