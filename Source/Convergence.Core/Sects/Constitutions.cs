using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Constituciones de secta (Cuerpo de Espada, cuerpo Vajra…): rasgos con grados de vanilla. El nuevo Sects las usa en sus discípulos
    /// (<see cref="DefModExtension_CultivatorPawnKind.traitChances"/>), en el especialista (<see cref="RoleRequirement_HasTrait"/>), en el
    /// inicio (<see cref="DefModExtension_StartingTrait"/>) y en el ritual de templado (<see cref="RitualOutcomeEffectWorker_TemperConstitution"/>).
    /// </summary>
    public static class ConstitutionUtility
    {
        /// <summary>Grado del rasgo en el pawn, o null si no lo tiene.</summary>
        public static int? Degree(Pawn pawn, TraitDef trait)
        {
            var t = pawn?.story?.traits?.GetTrait(trait);
            return t?.Degree;
        }

        /// <summary>Pone el rasgo con ese grado: reemplaza el grado anterior y quita los rasgos que chocan con él.</summary>
        public static void SetTrait(Pawn pawn, TraitDef trait, int degree)
        {
            var traits = pawn?.story?.traits;
            if (traits == null || trait == null) return;
            var old = traits.GetTrait(trait);
            if (old != null)
            {
                if (old.Degree == degree) return;
                traits.RemoveTrait(old);
            }
            foreach (var other in traits.allTraits.Where(t => trait.ConflictsWith(t) || t.def.ConflictsWith(trait)).ToList())
                traits.RemoveTrait(other);
            traits.GainTrait(new Trait(trait, degree, forced: true));
        }

        /// <summary>Saca el rasgo (al pasar a otra constitución, p. ej. del Cuerpo de Espada al Celestial).</summary>
        public static void RemoveTrait(Pawn pawn, TraitDef trait)
        {
            var t = pawn?.story?.traits?.GetTrait(trait);
            if (t != null) pawn.story.traits.RemoveTrait(t);
        }
    }

    /// <summary>Requisito de un rol de Ideology: tener alguno de estos rasgos (cualquier grado ≥ <see cref="minDegree"/>).</summary>
    public class RoleRequirement_HasTrait : RoleRequirement
    {
        public List<TraitDef> traits;
        public int minDegree = int.MinValue;

        public override string GetLabel(Precept_Role role) =>
            "CVG_RoleRequirement_Trait".Translate(traits.Select(t => t.degreeDatas.FirstOrDefault()?.LabelCap ?? t.LabelCap).ToCommaList(useAnd: false));

        public override bool Met(Pawn p, Precept_Role role) =>
            traits != null && traits.Any(t => ConstitutionUtility.Degree(p, t) is int d && d >= minDegree);
    }

    /// <summary>
    /// En un <c>MemeDef</c>: si la ideología del jugador empieza con este meme, al menos un colono inicial tiene el rasgo (no todos, y no
    /// por convertirse después).
    /// </summary>
    public class DefModExtension_StartingTrait : DefModExtension
    {
        public TraitDef trait;
        public int degree;
        public int count = 1;
    }

    public class GameComponent_StartingTraits : GameComponent
    {
        public GameComponent_StartingTraits(Game game) { }

        public override void StartedNewGame()
        {
            var ideo = Faction.OfPlayer?.ideos?.PrimaryIdeo;
            var map = Find.AnyPlayerHomeMap;
            if (ideo == null || map == null) return;
            foreach (var meme in ideo.memes)
            {
                var ext = meme.GetModExtension<DefModExtension_StartingTrait>();
                if (ext?.trait != null) Apply(map.mapPawns.FreeColonists, ext);
            }
        }

        /// <summary>Da el rasgo a los colonos que falten hasta llegar a <c>count</c>; prefiere a los mejores en cuerpo a cuerpo.</summary>
        public static void Apply(IEnumerable<Pawn> colonists, DefModExtension_StartingTrait ext)
        {
            var list = colonists.Where(p => p.story?.traits != null && p.DevelopmentalStage.Adult()).ToList();
            int missing = ext.count - list.Count(p => ConstitutionUtility.Degree(p, ext.trait) != null);
            foreach (var p in list.Where(p => ConstitutionUtility.Degree(p, ext.trait) == null)
                         .OrderByDescending(p => p.skills?.GetSkill(SkillDefOf.Melee)?.Level ?? 0).Take(missing))
                ConstitutionUtility.SetTrait(p, ext.trait, ext.degree);
        }
    }
}
