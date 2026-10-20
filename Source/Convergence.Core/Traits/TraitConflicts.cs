using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Conflictos de rasgos que el vanilla no puede expresar: con un grado concreto de otro rasgo
    /// (p. ej. Piel de Hierro no convive con Inmunidad -1 "enfermizo") o desde un grado propio
    /// (p. ej. Comprensión "prodigio" no convive con "aprende lento"). Para conflictos con un rasgo
    /// entero se sigue usando <c>conflictingTraits</c> / <c>exclusionTags</c> del vanilla.
    /// </summary>
    public class DefModExtension_TraitConflicts : DefModExtension
    {
        public List<TraitConflictEntry> conflicts = new List<TraitConflictEntry>();
    }

    public class TraitConflictEntry
    {
        /// <summary>Grado de este rasgo al que aplica (vacío = cualquiera).</summary>
        public int? ownDegree;
        public TraitDef trait;
        /// <summary>Grado del otro rasgo (vacío = cualquiera).</summary>
        public int? degree;
    }

    public static class TraitConflictUtility
    {
        private static bool Matches(TraitDef owner, int ownerDegree, TraitDef other, int otherDegree)
        {
            var ext = owner.GetModExtension<DefModExtension_TraitConflicts>();
            if (ext == null) return false;
            foreach (var c in ext.conflicts)
            {
                if (c.trait != other) continue;
                if (c.ownDegree.HasValue && c.ownDegree.Value != ownerDegree) continue;
                if (c.degree.HasValue && c.degree.Value != otherDegree) continue;
                return true;
            }
            return false;
        }

        public static bool Conflicts(TraitDef a, int aDegree, TraitDef b, int bDegree)
        {
            if (a == b) return false;
            return a.ConflictsWith(b) || b.ConflictsWith(a) || Matches(a, aDegree, b, bDegree) || Matches(b, bDegree, a, aDegree);
        }

        /// <summary>¿Puede el pawn recibir este rasgo sin contradecir los que ya tiene?</summary>
        public static bool CanGain(Pawn pawn, TraitDef def, int degree)
        {
            var traits = pawn?.story?.traits;
            if (traits == null) return false;
            foreach (var t in traits.allTraits)
                if (Conflicts(def, degree, t.def, t.Degree)) return false;
            return true;
        }

        /// <summary>
        /// Tras generar un pawn: si un rasgo de cultivo (con extensión de conflictos) contradice a otro,
        /// se quita el otro, salvo que sea forzado por un gen. Los rasgos de cultivo son raros y valen más.
        /// </summary>
        public static void ResolveConflicts(Pawn pawn)
        {
            var traits = pawn?.story?.traits;
            if (traits == null) return;
            var all = traits.allTraits;
            for (int i = 0; i < all.Count; i++)
            {
                var owner = all[i];
                if (owner.Suppressed || !owner.def.HasModExtension<DefModExtension_TraitConflicts>()) continue;
                for (int j = all.Count - 1; j >= 0; j--)
                {
                    var other = all[j];
                    // Suprimido (p. ej. Delicado bajo un gen): vanilla no lo quita ("doesn't have it") y no cuenta.
                    if (other == owner || other.sourceGene != null || other.Suppressed) continue;
                    if (other.def.HasModExtension<DefModExtension_TraitConflicts>()) continue;
                    if (!Matches(owner.def, owner.Degree, other.def, other.Degree)) continue;
                    traits.RemoveTrait(other);
                    // Si no se pudo quitar, no retroceder: antes esto repetía el mismo par para siempre y colgaba el juego.
                    if (all.Contains(other)) continue;
                    if (j < i) i--;
                }
            }
        }
    }
}
