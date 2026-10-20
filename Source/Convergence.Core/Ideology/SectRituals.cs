using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Convergence
{
    // Rituales de secta (Mods/Convergence.Sects/Mods/Ideology): la ceremonia de admisión nace al unirse un discípulo, la de ascensión al subir
    // de reino, y el torneo registra un duelo para sus participantes. El Core solo aporta los disparadores; el contenido está en XML.

    public class RitualObligationTrigger_AdmissionProperties : RitualObligationTriggerProperties
    {
        public RitualObligationTrigger_AdmissionProperties() => triggerClass = typeof(RitualObligationTrigger_Admission);
    }

    public class RitualObligationTrigger_AscensionProperties : RitualObligationTriggerProperties
    {
        public RitualObligationTrigger_AscensionProperties() => triggerClass = typeof(RitualObligationTrigger_Ascension);
    }

    /// <summary>Un colono se une a la ideoligión de la secta → se puede celebrar su admisión.</summary>
    public class RitualObligationTrigger_Admission : RitualObligationTrigger
    {
        public override void Notify_MemberGained(Pawn p)
        {
            if (p == null || !p.IsFreeColonist || p.Ideo != ritual.ideo) return;
            if (mustBePlayerIdeo && !Faction.OfPlayer.ideos.Has(ritual.ideo)) return;
            if (Find.TickManager.TicksGame < 60000) return; // no al empezar la partida
            ritual.AddObligation(new RitualObligation(ritual, true));
        }
    }

    /// <summary>Un miembro sube de reino → se puede celebrar su ascensión.</summary>
    public class RitualObligationTrigger_Ascension : RitualObligationTrigger
    {
        public static void NotifyAscended(Pawn p)
        {
            var ideo = p?.Ideo;
            if (ideo == null || !p.IsFreeColonist) return;
            foreach (var precept in ideo.PreceptsListForReading.OfType<Precept_Ritual>().ToList())
                foreach (var trigger in precept.obligationTriggers.OfType<RitualObligationTrigger_Ascension>())
                {
                    if (trigger.mustBePlayerIdeo && !Faction.OfPlayer.ideos.Has(ideo)) continue;
                    precept.AddObligation(new RitualObligation(precept, true));
                }
        }
    }

    [HarmonyPatch(typeof(RitualOutcomeEffectWorker_FromQuality), nameof(RitualOutcomeEffectWorker_FromQuality.Apply))]
    public static class Patch_RitualOutcome_Tournament
    {
        public static void Postfix(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual)
        {
            if (jobRitual?.Ritual?.def?.defName != "CVG_Ritual_Tournament" || totalPresence == null) return;
            foreach (var p in totalPresence.Keys.ToList()) SectEvents.Record(SectEvents.DuelFought, p);
        }
    }
}
