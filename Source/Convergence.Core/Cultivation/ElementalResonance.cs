using System.Text;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// En una prenda o accesorio (aros de los cinco elementos, docs/modulos/CONTENIDO_TOI_ACS.md): si la raíz espiritual de
    /// quien lo lleva tiene <see cref="element"/>, el stat al que se añade <see cref="StatPart_ElementalResonance"/> se
    /// multiplica por <see cref="factor"/>. Con otro elemento no hace nada.
    /// </summary>
    public class DefModExtension_ElementalResonance : DefModExtension
    {
        public QiElementDef element;
        public float factor = 1.15f;
    }

    /// <summary>Multiplica un stat de cultivo por la resonancia de los accesorios puestos con la raíz del portador.</summary>
    public class StatPart_ElementalResonance : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (TryGetFactor(req, out float f, null)) val *= f;
        }

        public override string ExplanationPart(StatRequest req)
        {
            var sb = new StringBuilder();
            return TryGetFactor(req, out _, sb) ? sb.ToString().TrimEndNewlines() : null;
        }

        private static bool TryGetFactor(StatRequest req, out float factor, StringBuilder sb)
        {
            factor = 1f;
            if (!(req.Thing is Pawn pawn) || pawn.apparel == null) return false;
            var root = CompCultivator.For(pawn)?.Root;
            if (root == null) return false;
            bool any = false;
            foreach (var ap in pawn.apparel.WornApparel)
            {
                var ext = ap.def.GetModExtension<DefModExtension_ElementalResonance>();
                if (ext?.element == null || !root.elements.Contains(ext.element)) continue;
                factor *= ext.factor;
                any = true;
                sb?.AppendLine("CVG_ElementalResonance".Translate(ap.LabelShortCap, ext.element.LabelCap) + ": x" + ext.factor.ToStringPercent());
            }
            return any;
        }
    }
}
