using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Convergence
{
    /// <summary>
    /// Retiro de avance. A = cojín de cultivo (o la celda donde estaba), B = píldora opcional.
    /// (Toma la píldora) → va al lugar → retiro de <see cref="CultivationRealmDef.breakthroughHours"/> horas → tirada.
    /// Si el reino pide tribulación, el colono la aguanta en el sitio hasta que termine.
    /// </summary>
    public class JobDriver_Breakthrough : JobDriver
    {
        private const int FleckInterval = 300;
        private const int TakeItemTicks = 120;

        private int retreatTicks;
        private int totalTicks = -1;
        private float qiAtStart = -1f;
        private bool resolved;

        private CompCultivator Comp => CompCultivator.For(pawn);
        private RealmTracker Tracker => Comp?.TrackerFor(Comp.BreakthroughPath);
        private Thing Spot => job.GetTarget(TargetIndex.A).Thing;
        private bool HasItem => job.GetTarget(TargetIndex.B).HasThing;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref retreatTicks, "retreatTicks");
            Scribe_Values.Look(ref totalTicks, "totalTicks", -1);
            Scribe_Values.Look(ref qiAtStart, "qiAtStart", -1f);
            Scribe_Values.Look(ref resolved, "resolved");
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (HasItem && !pawn.Reserve(job.GetTarget(TargetIndex.B), job, 1, 1, null, errorOnFailed)) return false;
            if (Spot != null) return pawn.ReserveSittableOrSpot(job.targetA.Cell, job, errorOnFailed);
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOn(() => !resolved && (Tracker == null || !Comp.CanFollow(Tracker.Path) || Tracker.NextRealm == null));
            this.FailOn(() => Spot != null && (Spot.Destroyed || Spot.IsBurning()));

            // 1. Píldora (opcional): ir, tomarla y absorberla.
            var gotoSpot = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
            yield return Toils_Jump.JumpIf(gotoSpot, () => !HasItem);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch)
                .FailOnDespawnedNullOrForbidden(TargetIndex.B);
            yield return Toils_General.Wait(TakeItemTicks, TargetIndex.B).WithProgressBarToilDelay(TargetIndex.B)
                .FailOnDespawnedNullOrForbidden(TargetIndex.B);
            var take = ToilMaker.MakeToil("TakeBreakthroughItem");
            take.defaultCompleteMode = ToilCompleteMode.Instant;
            take.initAction = () =>
            {
                var item = job.GetTarget(TargetIndex.B).Thing;
                if (item != null && item.Spawned && item.IngestibleNow) item.Ingested(pawn, 0f);
            };
            yield return take;

            // 2. Ir al lugar del retiro.
            yield return gotoSpot;

            // 3. Retiro: el recurso del camino (qi) se vacía poco a poco mientras se rompe la barrera.
            var retreat = ToilMaker.MakeToil("BreakthroughRetreat");
            retreat.socialMode = RandomSocialMode.Off;
            retreat.handlingFacing = true;
            retreat.defaultCompleteMode = ToilCompleteMode.Never;
            retreat.initAction = () =>
            {
                if (qiAtStart < 0f) qiAtStart = Tracker.Resource.Current;
                retreatTicks = 0;
                totalTicks = TotalTicksFor(Tracker.NextRealm);
                SoundCues.ForPlayerPawn("CVG_Cue_BreakthroughStart", pawn);
                if (pawn.Faction == Faction.OfPlayer)
                    Messages.Message("CVG_Breakthrough_Started".Translate(pawn.Named("PAWN"), Tracker.NextRealm.LabelCap),
                        pawn, MessageTypeDefOf.NeutralEvent);
            };
            retreat.tickIntervalAction = delta =>
            {
                var tracker = Tracker;
                if (Spot != null) pawn.rotationTracker.FaceCell(pawn.Position + Spot.Rotation.FacingCell);
                retreatTicks += delta;
                if (totalTicks < 0) totalTicks = TotalTicksFor(tracker.NextRealm);
                int total = totalTicks;
                if (qiAtStart < 0f) qiAtStart = tracker.Resource.Current;
                tracker.Resource.SetCurrent(qiAtStart * (1f - Mathf.Clamp01((float)retreatTicks / total)));

                if (pawn.IsHashIntervalTick(FleckInterval, delta) && pawn.Map != null)
                {
                    FleckMaker.ThrowMetaIcon(pawn.Position, pawn.Map, FleckDefOf.Meditating);
                    FleckMaker.ThrowLightningGlow(pawn.DrawPos, pawn.Map, 0.6f);
                }

                if (retreatTicks >= total && !resolved)
                {
                    resolved = true;
                    BreakthroughUtility.ResolveRetreat(pawn, tracker);
                    ReadyForNextToil();
                }
            };
            retreat.WithProgressBar(TargetIndex.A, () => totalTicks > 0 ? Mathf.Clamp01((float)retreatTicks / totalTicks) : 0f, alwaysShow: true);
            retreat.AddFinishAction(() =>
            {
                if (!resolved && retreatTicks > 0 && pawn.Faction == Faction.OfPlayer && !pawn.Dead)
                    Messages.Message("CVG_Breakthrough_Interrupted".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.NegativeEvent);
            });
            yield return retreat;

            // 4. Tribulación (si empezó): aguantar en el sitio hasta que termine.
            var endure = ToilMaker.MakeToil("EndureTribulation");
            endure.socialMode = RandomSocialMode.Off;
            endure.defaultCompleteMode = ToilCompleteMode.Never;
            endure.tickIntervalAction = delta =>
            {
                if (!(MapComponent_Tribulation.For(pawn.Map)?.IsUndergoing(pawn) ?? false)) ReadyForNextToil();
            };
            yield return endure;
        }

        private static int TotalTicksFor(CultivationRealmDef target) =>
            Mathf.Max(1, Mathf.RoundToInt((target?.breakthroughHours ?? 4f) * GenDate.TicksPerHour));

        public override string GetReport()
        {
            var target = Tracker?.NextRealm;
            if (resolved && (MapComponent_Tribulation.For(pawn.Map)?.IsUndergoing(pawn) ?? false))
                return "CVG_Breakthrough_ReportTribulation".Translate();
            return target != null ? "CVG_Breakthrough_Report".Translate(target.LabelCap).Resolve() : base.GetReport();
        }
    }
}
