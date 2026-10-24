using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Convergence
{
    /// <summary>
    /// Testigo de una tribulación (como los que se reúnen en un parto de Biotech): va a una celda a distancia segura, mira y se
    /// queda hasta que el cielo termina. A = el que enfrenta la tribulación, B = la celda.
    /// Cada testigo presente le resta daño a los rayos (<see cref="MapComponent_Tribulation.WitnessFactor"/>).
    /// </summary>
    public class JobDriver_WatchTribulation : JobDriver
    {
        private Pawn Tribulant => job.GetTarget(TargetIndex.A).Pawn;

        public override bool TryMakePreToilReservations(bool errorOnFailed) => pawn.Reserve(job.targetB, job, errorOnFailed: errorOnFailed);

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOn(() => Tribulant == null || Tribulant.Dead || !(MapComponent_Tribulation.For(pawn.Map)?.IsUndergoing(Tribulant) ?? false));
            yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
            var watch = ToilMaker.MakeToil("WatchTribulation");
            watch.socialMode = RandomSocialMode.Off;
            watch.handlingFacing = true;
            watch.defaultCompleteMode = ToilCompleteMode.Never;
            watch.tickIntervalAction = delta => pawn.rotationTracker.FaceTarget(Tribulant);
            yield return watch;
        }

        /// <summary>¿Está mirando la tribulación de <paramref name="tribulant"/> desde su lugar?</summary>
        public static bool IsWatching(Pawn watcher, Pawn tribulant) =>
            watcher.CurJob?.def == CVG_DefOf.CVG_WatchTribulation && watcher.CurJob.targetA.Pawn == tribulant
            && watcher.Position == watcher.CurJob.targetB.Cell;
    }
}
