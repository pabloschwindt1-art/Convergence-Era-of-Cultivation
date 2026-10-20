using System.Linq;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Tablilla de jade (玉简) que enseña un manual de técnicas al leerla. Con <see cref="manual"/> fijo enseña ese; sin
    /// él, cada tablilla elige uno al azar al crearse (se guarda). No se gasta: sirve para varios discípulos.
    /// </summary>
    public class CompProperties_UseEffectLearnManual : CompProperties_UseEffect
    {
        public TechniqueManualDef manual;

        public CompProperties_UseEffectLearnManual()
        {
            compClass = typeof(CompUseEffect_LearnTechniqueManual);
        }
    }

    public class CompUseEffect_LearnTechniqueManual : CompUseEffect
    {
        private TechniqueManualDef manual;

        private CompProperties_UseEffectLearnManual Props => (CompProperties_UseEffectLearnManual)props;

        public TechniqueManualDef Manual => Props.manual ?? manual;

        /// <summary>Fija el manual de una tablilla genérica (debug, recompensas).</summary>
        public void SetManual(TechniqueManualDef value) => manual = value;

        public override void PostPostMake()
        {
            base.PostPostMake();
            if (Props.manual == null && manual == null)
                manual = DefDatabase<TechniqueManualDef>.AllDefsListForReading.Where(m => m.npcCommonality > 0f && m.fromJadeSlips)
                    .RandomElementByWeightWithFallback(m => m.npcCommonality);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref manual, "manual");
        }

        public override string TransformLabel(string label) => Manual == null ? label : label + " (" + Manual.label + ")";

        public override bool AllowStackWith(Thing other) =>
            other.TryGetComp<CompUseEffect_LearnTechniqueManual>()?.Manual == Manual;

        public override string GetDescriptionPart()
        {
            var m = Manual;
            if (m == null) return null;
            string text = m.LabelCap.AsTipTitle() + "\n" + m.description;
            if (m.Techniques.Count > 0)
                text += "\n\n" + "CVG_Manual_Contains".Translate() + "\n" +
                        m.Techniques.Select(t => "  - " + t.LabelCap.Resolve()).ToList().ToLineList();
            return text;
        }

        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            if (Manual == null) return false;
            return Manual.CanLearn(p);
        }

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            CompCultivator.For(usedBy)?.Techniques.LearnManual(Manual);
            DaoComprehension.Gain(usedBy, 1.5f, DaoSource.Studying);
        }
    }
}
