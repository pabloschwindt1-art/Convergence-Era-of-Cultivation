using System.Linq;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;
using Verse.Grammar;

namespace Convergence
{
    /// <summary>
    /// "Un viejo sabio busca refugio" (plan de integración, 2026-10-23): la misión de inicio de la Convergencia, como el
    /// monolito de Anomaly, la gravnave de Odyssey o la misión del Imperio. Llega a la pestaña Misiones, puede quedar
    /// pendiente todo lo que el jugador quiera y, al aceptarla, desata la Convergencia (si no ocurrió ya) y el Maestro cae
    /// herido por la grieta. La misión sigue abierta mientras se queda y termina cuando se va (o muere).
    /// </summary>
    public class QuestNode_Root_MentorRefuge : QuestNode
    {
        protected override bool TestRunInt(Slate slate) => QuestGen_Get.GetMap() != null && DefDatabase<MentorDef>.AllDefsListForReading.Any();

        protected override void RunInt()
        {
            var quest = QuestGen.quest;
            var slate = QuestGen.slate;
            var map = QuestGen_Get.GetMap();
            var def = DefDatabase<MentorDef>.AllDefsListForReading.RandomElementByWeightWithFallback(m => m.commonality);
            bool convergenceDone = GameComponent_Convergence.Active;
            quest.AddPart(new QuestPart_MentorRefuge
            {
                inSignal = slate.Get<string>("inSignal"),
                mapParent = map.Parent,
                mentorDef = def
            });
            QuestGen.AddQuestDescriptionRules(new System.Collections.Generic.List<Rule>
            {
                new Rule_String("archetype", def.archetype),
                new Rule_String("convergenceLine", (convergenceDone ? "CVG_MentorQuest_AfterConvergence" : "CVG_MentorQuest_BeforeConvergence").Translate().Resolve())
            });
        }
    }

    public class QuestPart_MentorRefuge : QuestPart
    {
        public string inSignal;
        public MapParent mapParent;
        public MentorDef mentorDef;
        private bool arrived;

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            base.Notify_QuestSignalReceived(signal);
            if (arrived || signal.tag != inSignal) return;
            var map = mapParent?.Map ?? Find.AnyPlayerHomeMap;
            if (map == null) return;
            arrived = true;
            bool trigger = !GameComponent_Convergence.Active;
            // Aceptarlo desata la Convergencia: el sabio cae herido por la grieta que se abre en el cielo.
            if (trigger) GameComponent_Convergence.Instance?.TryFire(map);
            var mentor = GameComponent_Mentor.Instance?.Arrive(map, mentorDef, wounded: trigger ? true : (bool?)null);
            if (mentor == null) quest.End(QuestEndOutcome.Fail, sendLetter: false);
        }

        public override bool QuestPartReserves(Pawn p) => p == GameComponent_Mentor.Instance?.Mentor && arrived;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref inSignal, "inSignal");
            Scribe_References.Look(ref mapParent, "mapParent");
            Scribe_Defs.Look(ref mentorDef, "mentorDef");
            Scribe_Values.Look(ref arrived, "arrived");
        }

        /// <summary>La misión abierta del Maestro (para cerrarla cuando se va o muere).</summary>
        public static Quest OpenQuest() => Find.QuestManager.QuestsListForReading.FirstOrDefault(q =>
            q.State == QuestState.Ongoing && q.PartsListForReading.Any(p => p is QuestPart_MentorRefuge));

        /// <summary>La misión todavía sin aceptar (se ofrece una sola vez por partida).</summary>
        public static bool AnyOffered() => Find.QuestManager.QuestsListForReading.Any(q => q.PartsListForReading.Any(p => p is QuestPart_MentorRefuge));
    }
}
