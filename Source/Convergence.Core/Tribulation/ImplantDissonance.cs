using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Disonancia celestial de los implantes que no la definen (biónicos vanilla o de otros mods): según el nivel tecnológico
    /// del implante, por el factor de los ajustes. Los que ya traen CVG_CelestialDissonance en su etapa (injertos) no suman aquí.
    /// </summary>
    public class StatPart_ImplantDissonance : StatPart
    {
        public static float ForTechLevel(TechLevel t)
        {
            switch (t)
            {
                case TechLevel.Animal:
                case TechLevel.Neolithic:
                case TechLevel.Medieval: return 0.03f;
                case TechLevel.Industrial: return 0.08f;
                case TechLevel.Spacer: return 0.15f;
                case TechLevel.Ultra:
                case TechLevel.Archotech: return 0.25f;
                default: return 0.08f;
            }
        }

        private static bool Counts(Hediff h, StatDef stat, out float value)
        {
            value = 0f;
            var d = h.def;
            if (!(d.countsAsAddedPartOrImplant || d.addedPartProps != null) || d.spawnThingOnRemoved == null) return false;
            if (d.stages != null)
                foreach (var st in d.stages)
                    if (st.statOffsets != null && st.statOffsets.Any(o => o.stat == stat)) return false;
            value = ForTechLevel(d.spawnThingOnRemoved.techLevel);
            return true;
        }

        private static float Factor => ConvergenceMod.Settings?.implantDissonanceFactor ?? 1f;

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!(req.Thing is Pawn p) || p.health?.hediffSet == null || Factor <= 0f) return;
            float sum = 0f;
            foreach (var h in p.health.hediffSet.hediffs)
                if (Counts(h, parentStat, out float v)) sum += v;
            val += sum * Factor;
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!(req.Thing is Pawn p) || p.health?.hediffSet == null || Factor <= 0f) return null;
            var sb = new StringBuilder();
            foreach (var h in p.health.hediffSet.hediffs)
                if (Counts(h, parentStat, out float v)) sb.AppendLine("  " + h.LabelCap + ": +" + (v * Factor).ToString("0.00"));
            return sb.Length == 0 ? null : "CVG_ImplantDissonance".Translate() + ":\n" + sb.ToString().TrimEnd();
        }
    }
}
