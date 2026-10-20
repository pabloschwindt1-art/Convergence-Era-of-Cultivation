using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Efectos de una condición del calendario celestial (docs/EVENTOS.md §1: Marea de Qi, Reflujo del Qi, Luna del
    /// Demonio Interno…), en el <c>GameConditionDef</c>. Genérico: factores y sumas a cualquier stat con
    /// <see cref="StatPart_CelestialCondition"/> en su lista de partes, multiplicador de la comprensión del Dao y riesgo
    /// de desviación al cultivar.
    /// </summary>
    public class DefModExtension_CelestialCondition : DefModExtension
    {
        public List<StatModifier> statFactors;
        public List<StatModifier> statOffsets;
        public float daoComprehensionFactor = 1f;
        /// <summary>Probabilidad por hora de cultivo de sufrir <see cref="deviationHediff"/>.</summary>
        public float deviationChancePerHour;
        public HediffDef deviationHediff;
        /// <summary>Multiplicador de la probabilidad de la crisis "demonio interno" (MentalBreakWorker_InnerDemon).</summary>
        public float innerDemonBreakFactor = 1f;
    }

    /// <summary>Solo se dispara después de la Convergencia (el cielo de Jiuxiao todavía no se ve antes).</summary>
    public class IncidentWorker_CelestialCondition : IncidentWorker_MakeGameCondition
    {
        protected override bool CanFireNowSub(IncidentParms parms) => GameComponent_Convergence.Active && base.CanFireNowSub(parms);
    }

    /// <summary>Aplica a un stat de pawn los efectos de las condiciones celestiales activas en su mapa.</summary>
    public class StatPart_CelestialCondition : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            var map = (req.Thing as Pawn)?.MapHeld;
            if (map == null) return;
            var conditions = map.gameConditionManager.ActiveConditions;
            for (int i = 0; i < conditions.Count; i++)
            {
                var ext = conditions[i].def.GetModExtension<DefModExtension_CelestialCondition>();
                if (ext == null) continue;
                if (ext.statOffsets != null) val += ext.statOffsets.GetStatOffsetFromList(parentStat);
                if (ext.statFactors != null) val *= ext.statFactors.GetStatFactorFromList(parentStat);
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            var map = (req.Thing as Pawn)?.MapHeld;
            if (map == null) return null;
            StringBuilder sb = null;
            foreach (var condition in map.gameConditionManager.ActiveConditions)
            {
                var ext = condition.def.GetModExtension<DefModExtension_CelestialCondition>();
                if (ext == null) continue;
                float offset = ext.statOffsets?.GetStatOffsetFromList(parentStat) ?? 0f;
                float factor = ext.statFactors?.GetStatFactorFromList(parentStat) ?? 1f;
                if (offset == 0f && factor == 1f) continue;
                sb ??= new StringBuilder();
                sb.Append(condition.LabelCap + ": ");
                if (offset != 0f) sb.Append(parentStat.ValueToString(offset, ToStringNumberSense.Offset) + " ");
                if (factor != 1f) sb.Append("x" + factor.ToStringPercent());
                sb.AppendLine();
            }
            return sb?.ToString().TrimEndNewlines();
        }
    }

    public static class CelestialCalendar
    {
        /// <summary>Multiplicador de la comprensión del Dao en el mapa del colono.</summary>
        public static float DaoFactor(Pawn pawn)
        {
            var map = pawn?.MapHeld;
            if (map == null) return 1f;
            float f = 1f;
            foreach (var condition in map.gameConditionManager.ActiveConditions)
                f *= condition.def.GetModExtension<DefModExtension_CelestialCondition>()?.daoComprehensionFactor ?? 1f;
            return f;
        }

        /// <summary>Riesgo de desviación mientras cultiva (Luna del Demonio Interno).</summary>
        public static void TickPracticing(Pawn pawn, int ticks)
        {
            var map = pawn?.MapHeld;
            if (map == null) return;
            float hours = (float)ticks / GenDate.TicksPerHour;
            foreach (var condition in map.gameConditionManager.ActiveConditions)
            {
                var ext = condition.def.GetModExtension<DefModExtension_CelestialCondition>();
                if (ext?.deviationHediff == null || ext.deviationChancePerHour <= 0f) continue;
                if (pawn.health.hediffSet.HasHediff(ext.deviationHediff) || !Rand.Chance(ext.deviationChancePerHour * hours)) continue;
                pawn.health.AddHediff(ext.deviationHediff);
                if (pawn.Faction?.IsPlayer ?? false)
                    Messages.Message("CVG_Calendar_Deviation".Translate(pawn.Named("PAWN"), condition.LabelCap), pawn, MessageTypeDefOf.NegativeHealthEvent);
                pawn.jobs?.EndCurrentJob(Verse.AI.JobCondition.InterruptForced);
                return;
            }
        }
    }
}
