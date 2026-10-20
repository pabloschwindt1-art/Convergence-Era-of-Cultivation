using HarmonyLib;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Sucesos del cultivo para los preceptos de secta (Ideología): el Core registra <see cref="HistoryEventDef"/> propios y los preceptos
    /// del mod Sects (Mods/Ideology) reaccionan con pensamientos. Sin Ideología no se registra nada.
    /// </summary>
    public static class SectEvents
    {
        public const string Cultivated = "CVG_Cultivated";
        public const string Ascended = "CVG_Ascended";
        public const string Retreat = "CVG_RetreatCompleted";
        public const string TookPill = "CVG_TookPill";
        public const string DemonicArt = "CVG_DemonicArt";
        public const string ExtractedEssence = "CVG_ExtractedEssence";
        public const string DuelFought = "CVG_DuelFought";

        /// <summary>Registra el suceso con <paramref name="doer"/> como protagonista (no hace nada sin Ideología o con un pawn sin ideología).</summary>
        public static void Record(string defName, Pawn doer)
        {
            if (!ModsConfig.IdeologyActive || doer == null || doer.Ideo == null) return;
            var def = DefDatabase<HistoryEventDef>.GetNamedSilentFail(defName);
            if (def == null) return;
            Find.HistoryEventsManager.RecordEvent(new HistoryEvent(def, doer.Named(HistoryEventArgsNames.Doer)));
            if (defName == Ascended) RitualObligationTrigger_Ascension.NotifyAscended(doer);
        }
    }

    /// <summary>En un <c>AbilityDef</c> o <c>TechniqueDef</c>: usarla cuenta como "arte demoníaca" para los preceptos de secta.</summary>
    public class DefModExtension_DemonicArt : DefModExtension
    {
    }

    [HarmonyPatch(typeof(Ability), nameof(Ability.Activate), typeof(LocalTargetInfo), typeof(LocalTargetInfo))]
    public static class Patch_Ability_Activate_SectEvents
    {
        public static void Postfix(Ability __instance)
        {
            if (__instance.def.HasModExtension<DefModExtension_DemonicArt>()) SectEvents.Record(SectEvents.DemonicArt, __instance.pawn);
        }
    }

    [HarmonyPatch(typeof(Thing), nameof(Thing.Ingested))]
    public static class Patch_Thing_Ingested_SectEvents
    {
        public static void Postfix(Thing __instance, Pawn ingester)
        {
            if (__instance.def.IsDrug && __instance.def.defName.StartsWith("CVG_")) SectEvents.Record(SectEvents.TookPill, ingester);
        }
    }
}
