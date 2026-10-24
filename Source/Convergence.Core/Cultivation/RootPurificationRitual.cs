using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Ritual de purificación de raíz (docs/modulos/RAICES.md §6), con el patrón del cambio de rol de Ideology: un oficiante
    /// cultivador, un objetivo y un resultado por calidad. Bueno: 35 % de subir un grado; excelente: 70 % (o despierta una raíz
    /// inferior a quien no tiene); terrible: desviación de qi. Tope: superior. Una vez por año por colono.
    /// </summary>
    public static class RootPurification
    {
        public const int CooldownTicks = GenDate.TicksPerYear;

        public static SpiritRootGradeDef Cap => DefDatabase<SpiritRootGradeDef>.GetNamedSilentFail("CVG_RootGrade_Superior");

        public static bool CanTarget(Pawn p, out string reason)
        {
            reason = null;
            var comp = CompCultivator.For(p);
            if (comp == null) { reason = "CVG_RootRitual_NotCultivator".Translate(); return false; }
            if (SpiritRootUtility.IsArtificial(p)) { reason = "CVG_RootRitual_Artificial".Translate(); return false; }
            if (comp.Root != null && Cap != null && comp.Root.grade.order >= Cap.order) { reason = "CVG_RootRitual_AtCap".Translate(); return false; }
            int wait = comp.LastRootRitualTick + CooldownTicks - Find.TickManager.TicksGame;
            if (wait > 0) { reason = "CVG_RootRitual_Cooldown".Translate(wait.ToStringTicksToPeriod()); return false; }
            return true;
        }
    }

    public class RitualRole_RootPurificationTarget : RitualRoleColonist
    {
        public override bool AppliesToPawn(Pawn p, out string reason, TargetInfo selectedTarget, LordJob_Ritual ritual = null,
            RitualRoleAssignments assignments = null, Precept_Ritual precept = null, bool skipReason = false)
        {
            if (!base.AppliesToPawn(p, out reason, selectedTarget, ritual, assignments, precept, skipReason)) return false;
            if (RootPurification.CanTarget(p, out string why)) return true;
            reason = skipReason ? null : why;
            return false;
        }
    }

    /// <summary>El oficiante tiene que cultivar qi y estar al menos en Fundación.</summary>
    public class RitualRole_RootPurificationOfficiant : RitualRoleColonist
    {
        public override bool AppliesToPawn(Pawn p, out string reason, TargetInfo selectedTarget, LordJob_Ritual ritual = null,
            RitualRoleAssignments assignments = null, Precept_Ritual precept = null, bool skipReason = false)
        {
            if (!base.AppliesToPawn(p, out reason, selectedTarget, ritual, assignments, precept, skipReason)) return false;
            var realm = CompCultivator.For(p)?.TrackerFor(CVG_DefOf.CVG_Path_Qi)?.Realm;
            if (realm != null && realm.order >= 2) return true;
            reason = skipReason ? null : (string)"CVG_RootRitual_OfficiantRealm".Translate();
            return false;
        }
    }

    public class RitualOutcomeEffectWorker_RootPurification : RitualOutcomeEffectWorker_FromQuality
    {
        public RitualOutcomeEffectWorker_RootPurification() { }
        public RitualOutcomeEffectWorker_RootPurification(RitualOutcomeEffectDef def) : base(def) { }

        protected override void ApplyExtraOutcome(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, RitualOutcomePossibility outcome,
            out string extraOutcomeDesc, ref LookTargets letterLookTargets)
        {
            extraOutcomeDesc = null;
            var pawn = jobRitual.PawnWithRole("target");
            var comp = CompCultivator.For(pawn);
            if (comp == null) return;
            comp.LastRootRitualTick = Find.TickManager.TicksGame;
            letterLookTargets = pawn;
            var before = comp.Root?.grade;
            switch (outcome.positivityIndex)
            {
                case -2:
                    var deviation = DefDatabase<HediffDef>.GetNamedSilentFail("CVG_QiDeviation");
                    if (deviation != null) pawn.health.AddHediff(deviation);
                    extraOutcomeDesc = "CVG_RootRitual_Deviation".Translate(pawn.Named("PAWN"));
                    return;
                case 1:
                case 2:
                    float chance = outcome.positivityIndex == 2 ? 0.7f : 0.35f;
                    if (before == null)
                    {
                        if (outcome.positivityIndex == 2 && SpiritRootUtility.Awaken(pawn, SpiritRootUtility.Grades[0]))
                        {
                            extraOutcomeDesc = "CVG_RootRitual_Awakened".Translate(pawn.Named("PAWN"), comp.Root.grade.label);
                            return;
                        }
                    }
                    else if (Rand.Chance(chance) && SpiritRootUtility.TryRaise(pawn, RootPurification.Cap))
                    {
                        extraOutcomeDesc = "CVG_RootRitual_Raised".Translate(pawn.Named("PAWN"), before.label, comp.Root.grade.label);
                        return;
                    }
                    extraOutcomeDesc = "CVG_RootRitual_Unchanged".Translate(pawn.Named("PAWN"));
                    return;
                default:
                    extraOutcomeDesc = "CVG_RootRitual_Unchanged".Translate(pawn.Named("PAWN"));
                    return;
            }
        }
    }
}
