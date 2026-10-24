using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Efectos de una condición del calendario celestial (docs/EVENTOS.md §1: Marea de Qi, Reflujo del Qi, Luna del
    /// Demonio Interno…), en el <c>GameConditionDef</c>. Genérico: factores y sumas a cualquier stat con
    /// <see cref="StatPart_CelestialCondition"/> en su lista de partes, multiplicador de la comprensión del Dao y de la crisis
    /// del demonio interno (que deja la desviación de qi: ya no hay tirada por hora de cultivo).
    /// </summary>
    public class DefModExtension_CelestialCondition : DefModExtension
    {
        public List<StatModifier> statFactors;
        public List<StatModifier> statOffsets;
        public float daoComprehensionFactor = 1f;
        /// <summary>Multiplicador de la probabilidad de la crisis "demonio interno" (MentalBreakWorker_InnerDemon).</summary>
        public float innerDemonBreakFactor = 1f;
        /// <summary>Multiplicador del crecimiento de las plantas del mapa (lluvia espiritual).</summary>
        public float plantGrowthFactor = 1f;
    }

    /// <summary>Las condiciones celestes con <see cref="DefModExtension_CelestialCondition.plantGrowthFactor"/> aceleran las plantas del mapa.</summary>
    [HarmonyLib.HarmonyPatch(typeof(Plant), nameof(Plant.GrowthRate), HarmonyLib.MethodType.Getter)]
    public static class Patch_Plant_GrowthRate_Celestial
    {
        public static void Postfix(Plant __instance, ref float __result)
        {
            var map = __instance.Map;
            if (map == null || __result <= 0f) return;
            var list = map.gameConditionManager.ActiveConditions;
            for (int i = 0; i < list.Count; i++)
            {
                var ext = list[i].def.GetModExtension<DefModExtension_CelestialCondition>();
                if (ext != null && ext.plantGrowthFactor != 1f) __result *= ext.plantGrowthFactor;
            }
        }
    }

    /// <summary>
    /// Estrella de qi: el meteorito vanilla, pero lo que cae es una veta de piedra espiritual (solo después de la Convergencia).
    /// El contenido sale del XML del incidente (<see cref="DefModExtension_MeteorContents"/>).
    /// </summary>
    public class DefModExtension_MeteorContents : DefModExtension
    {
        public ThingDef mineable;
        public IntRange count = new IntRange(6, 12);
        /// <summary>Otra veta que puede traer cada roca del meteorito (p. ej. jade espiritual) y su chance por roca.</summary>
        public ThingDef extraMineable;
        public float extraChance;
    }

    public class IncidentWorker_SpiritMeteor : IncidentWorker_MeteoriteImpact
    {
        protected override bool CanFireNowSub(IncidentParms parms) => GameComponent_Convergence.Active && base.CanFireNowSub(parms);

        protected override List<Thing> GenerateMeteorContents(IncidentParms parms)
        {
            var ext = def.GetModExtension<DefModExtension_MeteorContents>();
            if (ext?.mineable == null) return base.GenerateMeteorContents(parms);
            var list = new List<Thing>();
            int n = ext.count.RandomInRange;
            for (int i = 0; i < n; i++)
            {
                var b = (Building)ThingMaker.MakeThing(ext.extraMineable != null && Rand.Chance(ext.extraChance) ? ext.extraMineable : ext.mineable);
                b.canChangeTerrainOnDestroyed = false;
                list.Add(b);
            }
            return list;
        }
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

    }
}
