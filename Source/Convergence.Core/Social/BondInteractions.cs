using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Vínculo que nace jugando (plan de integración, 2026-10-23): una interacción social vanilla que, si se dan las condiciones,
    /// crea una relación (hermano jurado, rival de por vida, compañero del Dao), con su recuerdo y su carta, como el
    /// noviazgo o la propuesta de matrimonio de vanilla. Todo en el XML de la <c>InteractionDef</c>.
    /// </summary>
    public class DefModExtension_BondInteraction : DefModExtension
    {
        public PawnRelationDef relation;
        /// <summary>Relación del otro hacia el que inicia, para vínculos no recíprocos (p. ej. el discípulo tiene un "maestro").</summary>
        public PawnRelationDef reverseRelation;
        /// <summary>El otro solo puede tener una relación <see cref="reverseRelation"/> (un solo maestro).</summary>
        public bool reverseExclusive;
        /// <summary>El que inicia tiene que estar al menos estos reinos por encima del otro (maestro y discípulo).</summary>
        public int minRealmLead = -99;
        /// <summary>Recuerdo del otro, si es distinto del de quien inicia.</summary>
        public ThoughtDef recipientThought;
        /// <summary>Peso base de la interacción cuando se cumplen las condiciones (vanilla: charla 1, cumplido ~0.1).</summary>
        public float weight = 0.02f;
        public bool bothCultivators = true;
        /// <summary>Solo tras la Convergencia.</summary>
        public bool afterConvergence = true;
        /// <summary>Opinión mínima / máxima de cada uno sobre el otro.</summary>
        public float minOpinion = -100f;
        public float maxOpinion = 100f;
        /// <summary>Si no está vacía, hace falta una de estas relaciones previas (p. ej. pareja para el voto del Dao).</summary>
        public List<PawnRelationDef> requiresRelations;
        /// <summary>Diferencia máxima de reino entre los dos (rivales: parejos). -1 = sin límite.</summary>
        public int maxRealmGap = -1;
        /// <summary>Probabilidad de que el otro acepte (si no, solo queda la charla).</summary>
        public float acceptChance = 1f;
        public ThoughtDef thought;
        [MustTranslate] public string letterLabel;
        [MustTranslate] public string letterText;
    }

    public class InteractionWorker_FormBond : InteractionWorker
    {
        private DefModExtension_BondInteraction Ext => interaction.GetModExtension<DefModExtension_BondInteraction>();

        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            var ext = Ext;
            return ext != null && BondUtility.CanForm(initiator, recipient, ext) ? ext.weight : 0f;
        }

        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText,
            out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
        {
            letterText = letterLabel = null;
            letterDef = null;
            lookTargets = null;
            var ext = Ext;
            if (ext == null || !Rand.Chance(ext.acceptChance)) return;
            if (!BondUtility.Form(initiator, recipient, ext.relation, ext.thought, ext.reverseRelation, ext.recipientThought)) return;
            if (!PawnUtility.ShouldSendNotificationAbout(initiator) && !PawnUtility.ShouldSendNotificationAbout(recipient)) return;
            if (ext.letterLabel.NullOrEmpty()) return;
            letterLabel = ext.letterLabel.Formatted(initiator.Named("INITIATOR"), recipient.Named("RECIPIENT"));
            letterText = ext.letterText.Formatted(initiator.Named("INITIATOR"), recipient.Named("RECIPIENT"));
            letterDef = LetterDefOf.PositiveEvent;
            lookTargets = new LookTargets(initiator, recipient);
        }
    }

    /// <summary>Charla entre cultivadores (como la charla profunda vanilla): solo pide que los dos cultiven.</summary>
    public class InteractionWorker_Cultivators : InteractionWorker
    {
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient) =>
            GameComponent_Convergence.Active && BondUtility.IsCultivator(initiator) && BondUtility.IsCultivator(recipient)
                ? interaction.GetModExtension<DefModExtension_BondInteraction>()?.weight ?? 0.5f : 0f;
    }

    public static class BondUtility
    {
        public static bool IsCultivator(Pawn p) => CompCultivator.For(p)?.IsCultivator ?? false;

        public static bool CanForm(Pawn a, Pawn b, DefModExtension_BondInteraction ext)
        {
            if (ext.relation == null || a.relations == null || b.relations == null) return false;
            if (ext.afterConvergence && !GameComponent_Convergence.Active) return false;
            if (a.relations.DirectRelationExists(ext.relation, b)) return false;
            if (ext.bothCultivators && (!IsCultivator(a) || !IsCultivator(b))) return false;
            float ab = a.relations.OpinionOf(b), ba = b.relations.OpinionOf(a);
            if (ab < ext.minOpinion || ba < ext.minOpinion || ab > ext.maxOpinion || ba > ext.maxOpinion) return false;
            if (!ext.requiresRelations.NullOrEmpty())
            {
                bool any = false;
                foreach (var r in ext.requiresRelations)
                    if (a.relations.DirectRelationExists(r, b)) { any = true; break; }
                if (!any) return false;
            }
            if (ext.maxRealmGap >= 0 && System.Math.Abs(SpiritGradeUtility.RealmLevel(a) - SpiritGradeUtility.RealmLevel(b)) > ext.maxRealmGap)
                return false;
            if (ext.minRealmLead > -99 && SpiritGradeUtility.RealmLevel(a) - SpiritGradeUtility.RealmLevel(b) < ext.minRealmLead) return false;
            if (ext.reverseRelation != null && ext.reverseExclusive && b.relations.GetFirstDirectRelationPawn(ext.reverseRelation) != null) return false;
            return true;
        }

        /// <summary>Crea la relación (si no existe) y deja el recuerdo en los dos.</summary>
        public static bool Form(Pawn a, Pawn b, PawnRelationDef relation, ThoughtDef thought) => Form(a, b, relation, thought, null, null);

        /// <summary>Crea la relación (si no existe), la inversa si la hay, y deja el recuerdo en los dos.</summary>
        public static bool Form(Pawn a, Pawn b, PawnRelationDef relation, ThoughtDef thought, PawnRelationDef reverse, ThoughtDef recipientThought)
        {
            if (relation == null || a.relations.DirectRelationExists(relation, b)) return false;
            a.relations.AddDirectRelation(relation, b);
            if (reverse != null && !b.relations.DirectRelationExists(reverse, a)) b.relations.AddDirectRelation(reverse, a);
            if (thought != null) a.needs?.mood?.thoughts.memories.TryGainMemory(thought, b);
            var other = recipientThought ?? thought;
            if (other != null) b.needs?.mood?.thoughts.memories.TryGainMemory(other, a);
            return true;
        }
    }

    /// <summary>
    /// Una pelea social entre dos cultivadores puede volverlos rivales de por vida (la relación con
    /// <see cref="DefModExtension_RivalryFromFights"/>).
    /// </summary>
    public class DefModExtension_RivalryFromFights : DefModExtension
    {
        public float chance = 0.35f;
        public ThoughtDef thought;
        [MustTranslate] public string letterLabel;
        [MustTranslate] public string letterText;
    }

    [HarmonyPatch(typeof(Pawn_InteractionsTracker), nameof(Pawn_InteractionsTracker.StartSocialFight))]
    public static class Patch_SocialFight_Rivalry
    {
        public static void Postfix(Pawn ___pawn, Pawn otherPawn)
        {
            var pawn = ___pawn;
            if (pawn == null || otherPawn == null || !GameComponent_Convergence.Active) return;
            if (!BondUtility.IsCultivator(pawn) || !BondUtility.IsCultivator(otherPawn)) return;
            foreach (var def in DefDatabase<PawnRelationDef>.AllDefsListForReading)
            {
                var ext = def.GetModExtension<DefModExtension_RivalryFromFights>();
                if (ext == null || !Rand.Chance(ext.chance)) continue;
                if (!BondUtility.Form(pawn, otherPawn, def, ext.thought)) continue;
                if (!ext.letterLabel.NullOrEmpty() && (PawnUtility.ShouldSendNotificationAbout(pawn) || PawnUtility.ShouldSendNotificationAbout(otherPawn)))
                    Find.LetterStack.ReceiveLetter(ext.letterLabel.Formatted(pawn.Named("INITIATOR"), otherPawn.Named("RECIPIENT")),
                        ext.letterText.Formatted(pawn.Named("INITIATOR"), otherPawn.Named("RECIPIENT")), LetterDefOf.NeutralEvent,
                        new LookTargets(pawn, otherPawn));
                return;
            }
        }
    }
}
