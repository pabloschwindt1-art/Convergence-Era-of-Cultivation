using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Convergence
{
    /// <summary>
    /// El colono se sienta en el lugar de cultivo, reúne qi y avanza en su etapa.
    /// Progreso por hora = progressPerHour del reino × CVG_CultivationSpeed × calidad de la raíz × concentración de qi.
    /// </summary>
    public class JobDriver_Cultivate : JobDriver
    {
        private const int RefreshInterval = 1250;
        private const int FleckInterval = 400;
        private const float DaoCultivatePerHour = 0.25f;

        // Se recalculan al empezar y cada RefreshInterval (no se guardan).
        private float concentration = -1f;
        private float progressPerTick;
        private float qiExtraFactor;

        private Thing Spot => job.GetTarget(TargetIndex.A).Thing;
        private CompCultivator Comp => CompCultivator.For(pawn);

        public override bool TryMakePreToilReservations(bool errorOnFailed) =>
            pawn.ReserveSittableOrSpot(job.targetA.Cell, job, errorOnFailed);

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            this.FailOn(() => !(Comp?.IsQiCultivator ?? false));

            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);

            var spot = Spot?.def.GetModExtension<DefModExtension_CultivationSpot>() ?? new DefModExtension_CultivationSpot();
            var cultivate = ToilMaker.MakeToil("Cultivate");
            cultivate.socialMode = RandomSocialMode.Off;
            cultivate.handlingFacing = true;
            cultivate.defaultCompleteMode = ToilCompleteMode.Delay;
            cultivate.defaultDuration = spot.sessionTicks;
            cultivate.initAction = Refresh;
            cultivate.tickIntervalAction = delta =>
            {
                var comp = Comp;
                pawn.rotationTracker.FaceCell(pawn.Position + Spot.Rotation.FacingCell);
                // concentration < 0: recién cargada la partida (initAction no se repite al cargar).
                if (concentration < 0f || pawn.IsHashIntervalTick(RefreshInterval, delta)) Refresh();

                comp.Realm.AddProgress(progressPerTick * delta);
                // Comprensión del Dao: cultivar es la fuente principal (y puede traer una iluminación súbita).
                DaoComprehension.GainWhilePracticing(pawn, delta, DaoCultivatePerHour, DaoSource.Cultivating);
                // Calendario celestial: la Luna del Demonio Interno puede desviar el qi.
                CelestialCalendar.TickPracticing(pawn, delta);
                // La regeneración pasiva sigue en el comp: aquí solo se suma el extra del lugar (× concentración).
                comp.Qi.Regen(delta, qiExtraFactor);

                if (pawn.IsHashIntervalTick(FleckInterval, delta) && pawn.Map != null)
                    FleckMaker.ThrowMetaIcon(pawn.Position, pawn.Map, FleckDefOf.Meditating);
            };
            cultivate.AddEndCondition(() => Comp.CanCultivateNow ? JobCondition.Ongoing : JobCondition.Succeeded);
            cultivate.WithProgressBar(TargetIndex.A, () => Comp.Realm.StagePercent, alwaysShow: true);
            cultivate.AddFinishAction(() => SectEvents.Record(SectEvents.Cultivated, pawn));
            yield return cultivate;
        }

        private void Refresh()
        {
            var comp = Comp;
            if (comp == null || !comp.Realm.EnsureStarted()) return;

            concentration = QiConcentrationUtility.ConcentrationAt(pawn.Position, pawn.Map);
            float rootFactor = (comp.Root?.grade?.cultivationSpeedFactor ?? 1f) * BloodlineUtility.AffinityCultivationFactor(pawn);
            float speed = pawn.GetStatValue(CVG_DefOf.CVG_CultivationSpeed);
            float placeFactor = QiConcentrationUtility.CultivationFactor(concentration);
            float perHour = comp.Realm.Realm.progressPerHour * speed * rootFactor * placeFactor;
            progressPerTick = Mathf.Max(0f, perHour / GenDate.TicksPerHour);
            var spot = Spot?.def.GetModExtension<DefModExtension_CultivationSpot>();
            qiExtraFactor = Mathf.Max(0f, (spot?.qiRegenFactor ?? 1f) * placeFactor - 1f);
        }

        public override string GetReport()
        {
            string report = base.GetReport();
            if (concentration >= 0f)
                report += " " + "CVG_CultivateReportQi".Translate(concentration.ToString("0.#"));
            return report;
        }
    }
}
