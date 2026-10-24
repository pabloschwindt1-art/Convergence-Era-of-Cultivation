using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Convergence
{
    /// <summary>
    /// Cultivar es meditar (plan de integración, 2026-10-23): no hay trabajo propio. Un cultivador que medita (franja Meditar del
    /// horario, recreación o el psyfoco de Royalty) reúne qi y avanza en su etapa. Los lugares de cultivo
    /// (<see cref="DefModExtension_CultivationSpot"/>: el punto de meditación vanilla, el cojín, la plataforma, el estrado) son
    /// lugares de meditación que el cultivador prefiere y que multiplican la regeneración de qi.
    /// Progreso por hora = progressPerHour del reino × CVG_CultivationSpeed × calidad de la raíz × concentración de qi.
    /// </summary>
    public static class MeditationCultivation
    {
        private const int RefreshInterval = 1250;
        private const float DaoCultivatePerHour = 0.25f;
        private const int SessionTicks = 2500;

        private class State
        {
            public float concentration = -1f;
            public float progressPerTick;
            public float qiExtraFactor;
            public int ticks;
        }

        private static readonly ConditionalWeakTable<JobDriver, State> states = new ConditionalWeakTable<JobDriver, State>();

        private static List<ThingDef> spotDefs;

        private static List<ThingDef> SpotDefs => spotDefs ??=
            DefDatabase<ThingDef>.AllDefsListForReading.Where(d => d.HasModExtension<DefModExtension_CultivationSpot>()).ToList();

        /// <summary>Todos los lugares de cultivo del mapa.</summary>
        public static IEnumerable<Thing> AllSpots(Map map)
        {
            var defs = SpotDefs;
            for (int i = 0; i < defs.Count; i++)
                foreach (var t in map.listerThings.ThingsOfDef(defs[i]))
                    yield return t;
        }

        /// <summary>¿Está cultivando ahora (meditando como cultivador)?</summary>
        public static bool IsCultivating(Pawn p) =>
            p?.jobs?.curDriver is JobDriver_Meditate && (CompCultivator.For(p)?.CanCultivateNow ?? false);

        /// <summary>Un tick de meditación de un cultivador.</summary>
        public static void Tick(JobDriver driver, int delta)
        {
            var pawn = driver.pawn;
            var comp = CompCultivator.For(pawn);
            if (comp == null || !comp.CanCultivateNow || !GameComponent_Convergence.Active) return;
            var state = states.GetOrCreateValue(driver);
            var spot = driver.job.GetTarget(TargetIndex.A).Thing;
            if (state.concentration < 0f || pawn.IsHashIntervalTick(RefreshInterval, delta)) Refresh(pawn, comp, spot, state);

            comp.Realm.AddProgress(state.progressPerTick * delta);
            // Comprensión del Dao: cultivar es la fuente principal.
            DaoComprehension.GainWhilePracticing(pawn, delta, DaoCultivatePerHour, DaoSource.Cultivating);
            // La regeneración pasiva sigue en el comp: aquí solo se suma el extra del lugar (× concentración).
            comp.Qi.Regen(delta, state.qiExtraFactor);

            state.ticks += delta;
            if (state.ticks >= SessionTicks)
            {
                state.ticks -= SessionTicks;
                SectEvents.Record(SectEvents.Cultivated, pawn);
            }
        }

        private static void Refresh(Pawn pawn, CompCultivator comp, Thing spot, State state)
        {
            if (!comp.Realm.EnsureStarted() || pawn.Map == null) return;
            state.concentration = QiConcentrationUtility.ConcentrationAt(pawn.Position, pawn.Map);
            float rootFactor = (comp.Root?.grade?.cultivationSpeedFactor ?? 1f) * BloodlineUtility.AffinityCultivationFactor(pawn);
            float speed = pawn.GetStatValue(CVG_DefOf.CVG_CultivationSpeed);
            float placeFactor = QiConcentrationUtility.CultivationFactor(state.concentration);
            float perHour = comp.Realm.Realm.progressPerHour * speed * rootFactor * placeFactor;
            state.progressPerTick = Mathf.Max(0f, perHour / GenDate.TicksPerHour);
            var ext = spot?.def.GetModExtension<DefModExtension_CultivationSpot>();
            state.qiExtraFactor = Mathf.Max(0f, (ext?.qiRegenFactor ?? 1f) * placeFactor - 1f);
        }

        public static string ReportSuffix(JobDriver driver) =>
            states.TryGetValue(driver, out var s) && s.concentration >= 0f
                ? " " + "CVG_CultivateReportQi".Translate(s.concentration.ToString("0.#")) : "";

        /// <summary>El mejor lugar de cultivo libre para <paramref name="pawn"/>: el suyo, y si no, el de más qi y más cerca.</summary>
        public static Thing BestSpotFor(Pawn pawn)
        {
            Thing best = null;
            float bestScore = float.MinValue;
            foreach (var t in AllSpots(pawn.Map))
            {
                if (!(t is Building b) || b.Faction != pawn.Faction || b.IsBurning()) continue;
                if (!MeditationUtility.IsValidMeditationBuildingForPawn(b, pawn)) continue;
                if (!MeditationUtility.SafeEnvironmentalConditions(pawn, b.Position, pawn.Map)) continue;
                if (CultivatorAI.InDanger(pawn, 20f)) return null;
                float score = QiConcentrationUtility.ConcentrationAt(b.Position, b.Map) * 10f
                              + (b.def.GetModExtension<DefModExtension_CultivationSpot>()?.qiRegenFactor ?? 1f) * 5f
                              - b.Position.DistanceTo(pawn.Position) * 0.2f;
                if (b.GetAssignedPawn() == pawn) score += 1000f;
                if (score > bestScore) { bestScore = score; best = b; }
            }
            return best;
        }
    }

    [HarmonyPatch(typeof(JobDriver_Meditate), "MeditationTick")]
    public static class Patch_JobDriver_Meditate_Tick
    {
        public static void Postfix(JobDriver_Meditate __instance) => MeditationCultivation.Tick(__instance, 1);
    }

    [HarmonyPatch(typeof(JobDriver_Meditate), nameof(JobDriver_Meditate.GetReport))]
    public static class Patch_JobDriver_Meditate_Report
    {
        public static void Postfix(JobDriver_Meditate __instance, ref string __result)
        {
            if (MeditationCultivation.IsCultivating(__instance.pawn)) __result += MeditationCultivation.ReportSuffix(__instance);
        }
    }

    /// <summary>El cultivador elige un lugar de cultivo para meditar si hay uno libre.</summary>
    [HarmonyPatch(typeof(MeditationUtility), nameof(MeditationUtility.FindMeditationSpot))]
    public static class Patch_MeditationUtility_FindSpot
    {
        public static void Postfix(Pawn pawn, ref MeditationSpotAndFocus __result)
        {
            if (pawn?.Map == null || !(CompCultivator.For(pawn)?.CanCultivateNow ?? false) || !GameComponent_Convergence.Active) return;
            if (__result.spot.Thing is Building_Throne) return;
            var spot = MeditationCultivation.BestSpotFor(pawn);
            if (spot == null) return;
            __result = new MeditationSpotAndFocus(spot, MeditationUtility.BestFocusAt(spot, pawn));
        }
    }
}
