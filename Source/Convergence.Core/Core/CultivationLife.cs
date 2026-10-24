using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// El cultivo en los sistemas vanilla de la vida diaria (docs/revision/REVISION_Core.md, prioridad alta):
    /// pensamientos (ThoughtDef), registros de la ficha (RecordDef) y relatos para el arte (TaleDef) cuando un colono
    /// avanza, falla, supera o muere en la tribulación, o lanza técnicas. Los defs están en Defs/CultivationLife.xml;
    /// si falta uno (otro mod lo quitó), simplemente no se registra.
    /// </summary>
    public static class CultivationLife
    {
        private static ThoughtDef T(string name) => DefDatabase<ThoughtDef>.GetNamedSilentFail(name);
        private static RecordDef R(string name) => DefDatabase<RecordDef>.GetNamedSilentFail(name);
        private static TaleDef Tale(string name) => DefDatabase<TaleDef>.GetNamedSilentFail(name);

        private static void Memory(Pawn pawn, string thought)
        {
            var def = T(thought);
            if (def != null && pawn?.needs?.mood?.thoughts?.memories != null) pawn.needs.mood.thoughts.memories.TryGainMemory(def);
        }

        private static void Increment(Pawn pawn, string record)
        {
            var def = R(record);
            if (def != null) pawn?.records?.Increment(def);
        }

        private static void RecordTale(string tale, Pawn pawn)
        {
            var def = Tale(tale);
            if (def != null && pawn != null) TaleRecorder.RecordTale(def, pawn);
        }

        public static void Notify_Ascended(Pawn pawn)
        {
            Memory(pawn, "CVG_Thought_BreakthroughSuccess");
            Increment(pawn, "CVG_Breakthroughs");
            RecordTale("CVG_Tale_Breakthrough", pawn);
            NotifyCloseOnes(pawn);
        }

        /// <summary>Pareja o compañero del Dao (+10), familia o hermano jurado (+4), rival de por vida (−5; solo la relación, no cualquiera que le caiga mal).</summary>
        private static void NotifyCloseOnes(Pawn pawn)
        {
            if (pawn?.relations == null) return;
            var rivalRel = DefDatabase<PawnRelationDef>.GetNamedSilentFail("CVG_LifelongRival");
            var daoPartner = DefDatabase<PawnRelationDef>.GetNamedSilentFail("CVG_DaoPartner");
            var sworn = DefDatabase<PawnRelationDef>.GetNamedSilentFail("CVG_SwornSibling");
            foreach (var other in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists)
            {
                if (other == pawn || other.needs?.mood == null) continue;
                string thought = null;
                if (LovePartnerRelationUtility.LovePartnerRelationExists(other, pawn) || (daoPartner != null && other.relations.DirectRelationExists(daoPartner, pawn)))
                    thought = "CVG_Thought_PartnerAscended";
                else if (rivalRel != null && other.relations.DirectRelationExists(rivalRel, pawn))
                {
                    thought = "CVG_Thought_RivalAscended";
                    // Y el que avanzó lo saborea si ahora está por encima de su rival.
                    if (SpiritGradeUtility.RealmLevel(pawn) > SpiritGradeUtility.RealmLevel(other))
                        SocialMemory(pawn, "CVG_Thought_SurpassedRival", other);
                }
                else if (other.relations.FamilyByBlood.Contains(pawn) || (sworn != null && other.relations.DirectRelationExists(sworn, pawn)))
                    thought = "CVG_Thought_KinAscended";
                var def = thought == null ? null : T(thought);
                if (def != null) other.needs.mood.thoughts.memories.TryGainMemory(def, pawn);
            }
        }

        public static void Notify_BreakthroughFailed(Pawn pawn)
        {
            Memory(pawn, "CVG_Thought_BreakthroughFailed");
            // Los cercanos se preocupan (pareja, compañero del Dao, familia, hermano jurado).
            if (pawn?.relations == null) return;
            var daoPartner = DefDatabase<PawnRelationDef>.GetNamedSilentFail("CVG_DaoPartner");
            var sworn = DefDatabase<PawnRelationDef>.GetNamedSilentFail("CVG_SwornSibling");
            foreach (var other in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists)
            {
                if (other == pawn || other.relations == null) continue;
                if (LovePartnerRelationUtility.LovePartnerRelationExists(other, pawn) || other.relations.FamilyByBlood.Contains(pawn)
                    || (daoPartner != null && other.relations.DirectRelationExists(daoPartner, pawn))
                    || (sworn != null && other.relations.DirectRelationExists(sworn, pawn)))
                    SocialMemory(other, "CVG_Thought_CloseOneFailed", pawn);
            }
        }

        private static void SocialMemory(Pawn pawn, string thought, Pawn about)
        {
            var def = T(thought);
            if (def != null && pawn?.needs?.mood?.thoughts?.memories != null) pawn.needs.mood.thoughts.memories.TryGainMemory(def, about);
        }

        /// <summary>Una refinación salió mal: el alquimista se lo toma a pecho.</summary>
        public static void Notify_RuinedPill(Pawn pawn) => Memory(pawn, "CVG_Thought_RuinedPill");

        /// <summary>Superó la tribulación: él, orgulloso; quienes la vieron (no todo el mapa), admirados.</summary>
        public static void Notify_TribulationSurvived(Pawn pawn, List<Pawn> witnesses)
        {
            Memory(pawn, "CVG_Thought_SurvivedTribulation");
            Increment(pawn, "CVG_TribulationsSurvived");
            RecordTale("CVG_Tale_SurvivedTribulation", pawn);
            if (witnesses == null) return;
            foreach (var other in witnesses)
                if (other != pawn) Memory(other, "CVG_Thought_WitnessedTribulation");
        }

        public static void Notify_DiedInTribulation(Pawn pawn) => RecordTale("CVG_Tale_DiedInTribulation", pawn);

        public static void Notify_TechniqueCast(Pawn pawn) => Increment(pawn, "CVG_TechniquesCast");
    }

    /// <summary>Tiempo cultivando o templando el cuerpo (registro de la ficha del colono).</summary>
    public class RecordWorker_TimeCultivating : RecordWorker
    {
        public override bool ShouldMeasureTimeNow(Pawn pawn)
        {
            var job = pawn.CurJobDef;
            return MeditationCultivation.IsCultivating(pawn) || job?.defName == "CVG_TemperBody";
        }
    }

    // ---------------------------------------------------------------- alertas (forma vanilla de "algo pide atención")

    /// <summary>Un cultivador está en el cuello de botella y puede intentar el avance ahora.</summary>
    public class Alert_CVG_ReadyToBreakthrough : Alert
    {
        private readonly List<Pawn> culprits = new List<Pawn>();

        public Alert_CVG_ReadyToBreakthrough()
        {
            defaultLabel = "CVG_Alert_ReadyToBreakthrough".Translate();
            defaultPriority = AlertPriority.Medium;
        }

        private List<Pawn> Culprits
        {
            get
            {
                culprits.Clear();
                foreach (var p in PawnsFinder.AllMaps_FreeColonistsSpawned)
                {
                    var comp = CompCultivator.For(p);
                    if (comp == null) continue;
                    foreach (var t in comp.ActiveTrackers)
                        if (t.Started && t.Bottleneck && BreakthroughUtility.CanAttempt(comp, t, out _)) { culprits.Add(p); break; }
                }
                return culprits;
            }
        }

        public override TaggedString GetExplanation() =>
            "CVG_Alert_ReadyToBreakthroughDesc".Translate(culprits.Select(p => p.LabelShortCap).ToLineList("  - "));

        public override AlertReport GetReport() => AlertReport.CulpritsAre(Culprits);
    }

    /// <summary>Desviación de qi (o demonio interno) en curso.</summary>
    public class Alert_CVG_QiDeviation : Alert_Critical
    {
        private readonly List<Pawn> culprits = new List<Pawn>();

        public Alert_CVG_QiDeviation()
        {
            defaultLabel = "CVG_Alert_QiDeviation".Translate();
        }

        private List<Pawn> Culprits
        {
            get
            {
                culprits.Clear();
                var deviation = DefDatabase<HediffDef>.GetNamedSilentFail("CVG_QiDeviation");
                foreach (var p in PawnsFinder.AllMaps_FreeColonistsSpawned)
                    if ((deviation != null && p.health.hediffSet.HasHediff(deviation)) || p.MentalStateDef?.defName == "CVG_InnerDemon")
                        culprits.Add(p);
                return culprits;
            }
        }

        public override TaggedString GetExplanation() =>
            "CVG_Alert_QiDeviationDesc".Translate(culprits.Select(p => p.LabelShortCap).ToLineList("  - "));

        public override AlertReport GetReport() => AlertReport.CulpritsAre(Culprits);
    }
}
