using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Guía dentro del juego: carta "Guía del cultivador" unas horas después de la Convergencia (dónde está cada cosa) y conceptos
    /// del Ayudante de aprendizaje vanilla que se enseñan cuando aparece la ocasión.
    /// </summary>
    public static class CultivationGuide
    {
        public static ConceptDef Cultivation => DefDatabase<ConceptDef>.GetNamedSilentFail("CVG_Concept_Cultivation");
        public static ConceptDef Techniques => DefDatabase<ConceptDef>.GetNamedSilentFail("CVG_Concept_Techniques");
        public static ConceptDef Breakthrough => DefDatabase<ConceptDef>.GetNamedSilentFail("CVG_Concept_Breakthrough");
        public static ConceptDef QiNetwork => DefDatabase<ConceptDef>.GetNamedSilentFail("CVG_Concept_QiNetwork");

        public static void Teach(ConceptDef concept, OpportunityType type)
        {
            if (concept != null && GameComponent_Convergence.Active) LessonAutoActivator.TeachOpportunity(concept, type);
        }

        public static void Demonstrated(ConceptDef concept, KnowledgeAmount amount)
        {
            if (concept != null) PlayerKnowledgeDatabase.KnowledgeDemonstrated(concept, amount);
        }

        public static void SendLetter()
        {
            Find.LetterStack.ReceiveLetter("CVG_GuideLetterLabel".Translate(), "CVG_GuideLetterText".Translate(), LetterDefOf.NeutralEvent);
            Teach(Cultivation, OpportunityType.Important);
        }
    }
}
