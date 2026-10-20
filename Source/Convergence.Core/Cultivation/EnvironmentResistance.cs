using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Resistencia al entorno (pedido del usuario, 2026-10-17): el stat <c>CVG_EnvironmentResistance</c> (lo dan las etapas de los
    /// reinos: Núcleo Dorado 50 %) se traduce en stats vanilla: rango de temperatura cómoda más ancho, menos inflamable y más
    /// resistencia tóxica. Encima, la afinidad de los elementos de la raíz (<see cref="QiElementDef.affinityOffsets"/>), con
    /// su valor completo al 50 % y el doble al 100 %. Se engancha en los stats al iniciar (como <see cref="StatPart_Backstory"/>).
    /// </summary>
    [StaticConstructorOnStartup]
    public class StatPart_EnvironmentResistance : StatPart
    {
        private const float TemperatureAtFull = 40f;   // ±°C de confort con resistencia 100 %
        private static StatDef envStat;

        static StatPart_EnvironmentResistance()
        {
            envStat = DefDatabase<StatDef>.GetNamedSilentFail("CVG_EnvironmentResistance");
            if (envStat == null) return;
            var stats = new HashSet<StatDef> { StatDefOf.ComfyTemperatureMin, StatDefOf.ComfyTemperatureMax, StatDefOf.Flammability, StatDefOf.ToxicResistance, StatDefOf.ToxicEnvironmentResistance };
            foreach (var el in DefDatabase<QiElementDef>.AllDefsListForReading)
                foreach (var m in (el.affinityOffsets ?? new List<StatModifier>()).Concat(el.affinityFactors ?? new List<StatModifier>()))
                    if (m.stat != null) stats.Add(m.stat);
            foreach (var s in stats)
            {
                if (s == null || s == envStat) continue;
                s.parts ??= new List<StatPart>();
                if (!s.parts.Any(p => p is StatPart_EnvironmentResistance)) s.parts.Add(new StatPart_EnvironmentResistance { parentStat = s });
            }
        }

        private static float Env(Pawn p) => envStat == null ? 0f : Mathf.Clamp01(p.GetStatValue(envStat));

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!(req.Thing is Pawn pawn) || !pawn.RaceProps.Humanlike) return;
            float e = Env(pawn);
            if (e <= 0f) return;
            val = Apply(parentStat, val, e);
            foreach (var el in Elements(pawn)) val = ApplyAffinity(el, parentStat, val, e);
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!(req.Thing is Pawn pawn) || !pawn.RaceProps.Humanlike) return null;
            float e = Env(pawn);
            if (e <= 0f) return null;
            var sb = new StringBuilder();
            float before = 0f, after = Apply(parentStat, 0f, e);
            if (parentStat == StatDefOf.Flammability || parentStat == StatDefOf.ToxicResistance || parentStat == StatDefOf.ToxicEnvironmentResistance)
                sb.AppendLine("CVG_EnvRes_Explain".Translate(e.ToStringPercent()));
            else if (Mathf.Abs(after - before) > 0.01f)
                sb.AppendLine("CVG_EnvRes_Explain".Translate(e.ToStringPercent()) + ": " + after.ToStringByStyle(parentStat.toStringStyle, ToStringNumberSense.Offset));
            foreach (var el in Elements(pawn))
                if (el.affinityOffsets?.Any(m => m.stat == parentStat) == true || el.affinityFactors?.Any(m => m.stat == parentStat) == true)
                    sb.AppendLine("CVG_EnvRes_Affinity".Translate(el.LabelCap));
            return sb.Length > 0 ? sb.ToString().TrimEndNewlines() : null;
        }

        private static IEnumerable<QiElementDef> Elements(Pawn pawn) =>
            CompCultivator.For(pawn)?.Root?.elements ?? Enumerable.Empty<QiElementDef>();

        private static float Apply(StatDef stat, float val, float e)
        {
            if (stat == StatDefOf.ComfyTemperatureMin) return val - TemperatureAtFull * e;
            if (stat == StatDefOf.ComfyTemperatureMax) return val + TemperatureAtFull * e;
            if (stat == StatDefOf.Flammability) return val * (1f - e);
            if (stat == StatDefOf.ToxicResistance || stat == StatDefOf.ToxicEnvironmentResistance) return 1f - (1f - val) * (1f - e);
            return val;
        }

        private static float ApplyAffinity(QiElementDef el, StatDef stat, float val, float e)
        {
            float k = Mathf.Clamp(e / 0.5f, 0f, 2f);   // valor completo en el Núcleo Dorado, doble al 100 %
            if (el.affinityOffsets != null)
                foreach (var m in el.affinityOffsets) if (m.stat == stat) val += m.value * k;
            if (el.affinityFactors != null)
                foreach (var m in el.affinityFactors) if (m.stat == stat) val *= Mathf.Lerp(1f, m.value, Mathf.Clamp01(k));
            return val;
        }
    }
}
