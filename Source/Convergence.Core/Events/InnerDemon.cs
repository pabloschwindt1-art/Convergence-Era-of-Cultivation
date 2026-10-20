using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace Convergence
{
    /// <summary>
    /// Demonio interno (心魔) como crisis mental vanilla: solo cultivadores, más probable bajo la Luna del Demonio Interno
    /// (factor <see cref="DefModExtension_CelestialCondition.innerDemonBreakFactor"/> de las condiciones activas). El colono
    /// vaga perdido (mismo comportamiento que el delirio vanilla, por el parche del árbol de pensamiento) y al acabar le
    /// queda la desviación del qi (o el reflujo de sangre-qi si cultiva el cuerpo).
    /// </summary>
    public class MentalBreakWorker_InnerDemon : MentalBreakWorker
    {
        public override bool BreakCanOccur(Pawn pawn) =>
            base.BreakCanOccur(pawn) && (CompCultivator.For(pawn)?.IsCultivator ?? false);

        public override float CommonalityFor(Pawn pawn, bool moodCaused = false)
        {
            float c = base.CommonalityFor(pawn, moodCaused);
            var map = pawn.MapHeld;
            if (map != null)
                foreach (var condition in map.gameConditionManager.ActiveConditions)
                    c *= condition.def.GetModExtension<DefModExtension_CelestialCondition>()?.innerDemonBreakFactor ?? 1f;
            return c;
        }
    }

    /// <summary>Hediffs que deja el demonio interno al acabar (en el MentalStateDef).</summary>
    public class DefModExtension_InnerDemon : DefModExtension
    {
        public HediffDef qiHediff;
        public HediffDef bodyHediff;
    }

    public class MentalState_InnerDemon : MentalState_WanderPsychotic
    {
        public override void PostEnd()
        {
            base.PostEnd();
            var ext = def.GetModExtension<DefModExtension_InnerDemon>();
            var comp = CompCultivator.For(pawn);
            if (ext == null || comp == null || pawn.Dead) return;
            var hediff = comp.IsQiCultivator ? ext.qiHediff : ext.bodyHediff ?? ext.qiHediff;
            if (hediff != null && !pawn.health.hediffSet.HasHediff(hediff)) pawn.health.AddHediff(hediff);
        }
    }

    /// <summary>
    /// Edificio de estudio del alma (tablero de go, mesa de caligrafía…): quien lo usa como recreación gana un poco de
    /// comprensión del Dao. Comprobación cada 250 ticks (el edificio necesita <c>tickerType Rare</c>).
    /// </summary>
    public class CompProperties_DaoStudy : CompProperties
    {
        public float comprehensionPerHour = 0.05f;

        public CompProperties_DaoStudy()
        {
            compClass = typeof(CompDaoStudy);
        }
    }

    public class CompDaoStudy : ThingComp
    {
        public CompProperties_DaoStudy Props => (CompProperties_DaoStudy)props;

        public override void CompTickRare()
        {
            base.CompTickRare();
            var map = parent.Map;
            if (map == null || !GameComponent_Convergence.Active) return;
            foreach (var pawn in map.mapPawns.FreeColonistsSpawned)
            {
                var job = pawn.CurJob;
                if (job == null || job.targetA.Thing != parent || job.def.joyKind == null) continue;
                DaoComprehension.Gain(pawn, Props.comprehensionPerHour * GenTicks.TickRareInterval / GenDate.TicksPerHour, DaoSource.Studying);
            }
        }
    }
}
