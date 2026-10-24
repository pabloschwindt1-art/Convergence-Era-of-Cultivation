using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Convergence
{
    /// <summary>
    /// Da recurso de cultivo al comerse (carne espiritual, sangre y núcleo de bestia, sopa espiritual). Con
    /// <see cref="startsPath"/> empieza el camino si el colono puede seguirlo: así el camino del cuerpo arranca con
    /// los recursos de bestias espirituales (decisión de diseño: sin ellos no se progresa).
    /// Sin <see cref="path"/>: el recurso va al camino del cuerpo (empezándolo) y a todos los caminos que ya sigue.
    /// </summary>
    public class IngestionOutcomeDoer_GainCultivationResource : IngestionOutcomeDoer
    {
        public CultivationPathDef path;
        public float amount = 10f;
        public bool startsPath = true;
        /// <summary>Comprensión del Dao al absorberlo (núcleos de bestia).</summary>
        public float daoComprehension;

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
        {
            var comp = CompCultivator.For(pawn);
            if (comp == null) return;
            if (daoComprehension > 0f) DaoComprehension.Gain(pawn, daoComprehension * Mathf.Max(1, ingestedCount), DaoSource.Absorbing);
            float gain = amount * Mathf.Max(1, ingestedCount);
            if (path != null)
            {
                var tracker = startsPath ? comp.StartPath(path) : comp.Follows(path) ? comp.TrackerFor(path) : null;
                if (tracker != null && tracker.Started) tracker.Resource.Gain(gain);
                return;
            }
            if (startsPath && CVG_DefOf.CVG_Path_Body != null) comp.StartPath(CVG_DefOf.CVG_Path_Body);
            foreach (var tracker in comp.ActiveTrackers.ToList())
                if (tracker.Started) tracker.Resource.Gain(gain);
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef parentDef)
        {
            var p = path ?? CVG_DefOf.CVG_Path_Body;
            string label = path == null ? "CVG_Stat_CultivationResource".Translate().Resolve() : p.resourceLabel.CapitalizeFirst();
            yield return new StatDrawEntry(StatCategoryDefOf.BasicsNonPawn, label, "+" + amount.ToString("0"),
                "CVG_Stat_CultivationResourceDesc".Translate(), 2000);
        }
    }

    /// <summary>
    /// Koi de seda (Odyssey, plan de fusión 2026-10-23): al comerlo, chance de subir una etapa dentro del reino actual de cada
    /// camino que sigue. Nunca pasa de reino (eso sigue siendo un avance con tribulación).
    /// </summary>
    public class IngestionOutcomeDoer_AdvanceStage : IngestionOutcomeDoer
    {
        public float chance = 0.25f;

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
        {
            var comp = CompCultivator.For(pawn);
            if (comp == null) return;
            foreach (var tracker in comp.ActiveTrackers.ToList())
            {
                if (!tracker.Started || tracker.Stage >= tracker.Realm.stages || !Rand.Chance(chance)) continue;
                tracker.SetStage(tracker.Stage + 1);
                if (PawnUtility.ShouldSendNotificationAbout(pawn))
                    Messages.Message("CVG_Koi_StageUp".Translate(pawn.Named("PAWN"), tracker.Realm.label, tracker.Stage), pawn, MessageTypeDefOf.PositiveEvent);
            }
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef parentDef)
        {
            yield return new StatDrawEntry(StatCategoryDefOf.BasicsNonPawn, "CVG_Stat_AdvanceStage".Translate(), chance.ToStringPercent(),
                "CVG_Stat_AdvanceStageDesc".Translate(), 1990);
        }
    }

    /// <summary>Marca un edificio como aparato de templado corporal (muñeco de madera, piedras de pesas…).</summary>
    public class DefModExtension_BodyTrainingSpot : DefModExtension
    {
        /// <summary>Multiplicador del progreso por hora del reino del cuerpo.</summary>
        public float progressFactor = 1f;
        /// <summary>Sangre-qi que se gasta por hora de entrenamiento (el combustible del templado).</summary>
        public float bloodQiPerHour = 10f;
        /// <summary>Habilidad que se entrena a la vez (opcional).</summary>
        public SkillDef skill;
        public float skillXpPerHour = 150f;
        /// <summary>Descanso que se gasta de más por hora (cansa).</summary>
        public float extraRestFallPerHour = 0.05f;
        public int sessionTicks = 2500;
        public FleckDef fleck;
    }

    /// <summary>Busca un aparato de templado libre para quien puede templar el cuerpo ahora (tiene sangre-qi).</summary>
    public class WorkGiver_TemperBody : WorkGiver_Scanner
    {
        private static List<ThingDef> spotDefs;

        private static List<ThingDef> SpotDefs => spotDefs ??=
            DefDatabase<ThingDef>.AllDefsListForReading.Where(d => d.HasModExtension<DefModExtension_BodyTrainingSpot>()).ToList();

        public override PathEndMode PathEndMode => PathEndMode.InteractionCell;

        public override Danger MaxPathDanger(Pawn pawn) => Danger.Some;

        public override bool ShouldSkip(Pawn pawn, bool forced = false) => !(CompCultivator.For(pawn)?.CanTemperNow ?? false);

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            var defs = SpotDefs;
            for (int i = 0; i < defs.Count; i++)
                foreach (var t in pawn.Map.listerThings.ThingsOfDef(defs[i]))
                    yield return t;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!t.def.HasModExtension<DefModExtension_BodyTrainingSpot>()) return false;
            if (t.Faction != pawn.Faction || t.IsForbidden(pawn) || t.IsBurning()) return false;
            if (!(CompCultivator.For(pawn)?.CanTemperNow ?? false)) return false;
            if (!forced && CultivatorAI.InDanger(pawn, 20f)) return false;
            return pawn.CanReserve(t, 1, -1, null, forced) && pawn.CanReach(t, PathEndMode.InteractionCell, Danger.Some);
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false) => JobMaker.MakeJob(CVG_DefOf.CVG_TemperBody, t);
    }

    /// <summary>
    /// Templar el cuerpo en un aparato: gasta sangre-qi y la convierte en progreso del reino del cuerpo.
    /// Progreso/hora = progressPerHour del reino × <c>CVG_BodyTemperingSpeed</c> × factor del aparato. Sin sangre-qi
    /// (carne/sangre de bestia espiritual) no se avanza.
    /// </summary>
    public class JobDriver_TemperBody : JobDriver
    {
        private const int FleckInterval = 300;

        private Thing Spot => job.GetTarget(TargetIndex.A).Thing;
        private CompCultivator Comp => CompCultivator.For(pawn);
        private RealmTracker Tracker => Comp?.TrackerFor(CVG_DefOf.CVG_Path_Body);

        public override bool TryMakePreToilReservations(bool errorOnFailed) => pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            this.FailOn(() => !(Comp?.IsBodyCultivator ?? false));

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

            var ext = Spot?.def.GetModExtension<DefModExtension_BodyTrainingSpot>() ?? new DefModExtension_BodyTrainingSpot();
            var train = ToilMaker.MakeToil("TemperBody");
            train.socialMode = RandomSocialMode.Off;
            train.handlingFacing = true;
            train.defaultCompleteMode = ToilCompleteMode.Delay;
            train.defaultDuration = ext.sessionTicks;
            train.tickIntervalAction = delta =>
            {
                var tracker = Tracker;
                if (tracker == null || !tracker.Started) return;
                pawn.rotationTracker.FaceTarget(Spot);

                float hours = (float)delta / GenDate.TicksPerHour;
                var res = tracker.Resource;
                float cost = ext.bloodQiPerHour * hours;
                if (res.Current < cost)
                {
                    EndJobWith(JobCondition.Succeeded);
                    return;
                }
                res.TryConsume(cost);
                float perHour = tracker.Realm.progressPerHour * pawn.GetStatValue(CVG_DefOf.CVG_BodyTemperingSpeed) * ext.progressFactor;
                tracker.AddProgress(perHour * hours);
                DaoComprehension.GainWhilePracticing(pawn, delta, 0.15f, DaoSource.Tempering);
                TraitProgression.Add(pawn, "BodyTempering", hours);
                if (ext.skill != null) pawn.skills?.Learn(ext.skill, ext.skillXpPerHour * hours);
                if (pawn.needs?.rest != null) pawn.needs.rest.CurLevel -= ext.extraRestFallPerHour * hours;

                if (pawn.IsHashIntervalTick(FleckInterval, delta) && pawn.Map != null)
                {
                    if (ext.fleck != null) FleckMaker.Static(Spot.TrueCenter(), pawn.Map, ext.fleck);
                    else FleckMaker.ThrowDustPuff(Spot.TrueCenter(), pawn.Map, 0.6f);
                }
            };
            train.AddEndCondition(() => Comp.CanTemperNow ? JobCondition.Ongoing : JobCondition.Succeeded);
            train.WithProgressBar(TargetIndex.A, () => Tracker?.StagePercent ?? 0f, alwaysShow: true);
            yield return train;
        }
    }
}
