using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Convergence
{
    /// <summary>
    /// Relaciones que nacen de lo que pasa en la partida (plan de integración, 2026-10-23), no de un botón:
    /// deuda de vida (rescatar a alguien caído), enemistad de sangre (matar a un ser querido de un colono) y condiscípulos
    /// (discípulos del mismo maestro, implícita como los hermanos vanilla). Los defs están en Relations.xml (TraitsDestinies);
    /// sin ese módulo, no pasa nada.
    /// </summary>
    public static class LifeBonds
    {
        private static PawnRelationDef R(string name) => DefDatabase<PawnRelationDef>.GetNamedSilentFail(name);

        public static PawnRelationDef Benefactor => R("CVG_Benefactor");
        public static PawnRelationDef BloodFeud => R("CVG_BloodFeud");
        public static PawnRelationDef Master => R("CVG_Master");

        /// <summary>Probabilidad de que un rescate deje una deuda de vida.</summary>
        public const float LifeDebtChance = 0.5f;

        /// <summary>Quienes sienten como propia la muerte de <paramref name="victim"/>.</summary>
        public static bool IsLovedOne(Pawn colonist, Pawn victim)
        {
            if (colonist.relations == null) return false;
            if (colonist.relations.FamilyByBlood.Contains(victim) || LovePartnerRelationUtility.LovePartnerRelationExists(colonist, victim)) return true;
            foreach (var name in new[] { "CVG_DaoPartner", "CVG_SwornSibling", "CVG_Master", "CVG_Disciple" })
            {
                var def = R(name);
                if (def != null && (colonist.relations.DirectRelationExists(def, victim) || victim.relations?.DirectRelationExists(def, colonist) == true))
                    return true;
            }
            return false;
        }

        public static void Notify_Rescued(Pawn rescuer, Pawn rescued)
        {
            var def = Benefactor;
            if (def == null || rescuer == null || rescued == null || rescuer == rescued || !rescued.RaceProps.Humanlike) return;
            if (rescued.relations == null || rescued.relations.DirectRelationExists(def, rescuer) || !Rand.Chance(LifeDebtChance)) return;
            rescued.relations.AddDirectRelation(def, rescuer);
            var thought = DefDatabase<ThoughtDef>.GetNamedSilentFail("CVG_Thought_LifeDebt");
            if (thought != null) rescued.needs?.mood?.thoughts.memories.TryGainMemory(thought, rescuer);
            if (PawnUtility.ShouldSendNotificationAbout(rescued))
                Messages.Message("CVG_LifeDebt_Message".Translate(rescued.Named("PAWN"), rescuer.Named("OTHER")), new LookTargets(rescued, rescuer),
                    MessageTypeDefOf.PositiveEvent);
        }

        public static void Notify_Killed(Pawn victim, Pawn killer)
        {
            var def = BloodFeud;
            if (def == null || killer == null || victim == null || killer == victim || !killer.RaceProps.Humanlike || !victim.RaceProps.Humanlike) return;
            if (!GameComponent_Convergence.Active) return;
            var avengers = new List<Pawn>();
            foreach (var c in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists)
            {
                if (c == killer || c.relations == null || c.relations.DirectRelationExists(def, killer) || !IsLovedOne(c, victim)) continue;
                c.relations.AddDirectRelation(def, killer);
                avengers.Add(c);
            }
            if (avengers.Count == 0) return;
            Find.LetterStack.ReceiveLetter("CVG_BloodFeud_Label".Translate(killer.LabelShortCap),
                "CVG_BloodFeud_Text".Translate(killer.Named("KILLER"), victim.Named("VICTIM"), avengers.Select(p => p.LabelShortCap).ToCommaList(useAnd: true)),
                LetterDefOf.NegativeEvent, new LookTargets(avengers.Cast<Thing>().Append(killer)));
        }
    }

    /// <summary>Condiscípulos: dos discípulos del mismo maestro (relación implícita, como los hermanos).</summary>
    public class PawnRelationWorker_FellowDisciple : PawnRelationWorker
    {
        public override bool InRelation(Pawn me, Pawn other)
        {
            var master = LifeBonds.Master;
            if (me == other || master == null || me.relations == null || other.relations == null) return false;
            foreach (var rel in me.relations.DirectRelations)
                if (rel.def == master && rel.otherPawn != other && other.relations.DirectRelationExists(master, rel.otherPawn))
                    return true;
            return false;
        }
    }

    /// <summary>Rescatar a alguien caído y llevarlo a una cama puede dejar una deuda de vida.</summary>
    [HarmonyPatch(typeof(JobDriver_TakeToBed), "MakeNewToils")]
    public static class Patch_TakeToBed_LifeDebt
    {
        public static IEnumerable<Toil> Postfix(IEnumerable<Toil> toils, JobDriver_TakeToBed __instance)
        {
            Toil last = null;
            foreach (var t in toils)
            {
                if (last != null) yield return last;
                last = t;
            }
            if (last == null) yield break;
            var driver = __instance;
            last.AddFinishAction(() =>
            {
                var takee = driver.job.GetTarget(TargetIndex.A).Pawn;
                if (driver.job.def == JobDefOf.Rescue && takee != null && takee.InBed() && !takee.Dead)
                    LifeBonds.Notify_Rescued(driver.pawn, takee);
            });
            yield return last;
        }
    }

    /// <summary>Matar a un ser querido de un colono deja una enemistad de sangre con el asesino.</summary>
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
    public static class Patch_Pawn_Kill_BloodFeud
    {
        public static void Postfix(Pawn __instance, DamageInfo? dinfo)
        {
            if (__instance.Dead && dinfo?.Instigator is Pawn killer) LifeBonds.Notify_Killed(__instance, killer);
        }
    }
}
