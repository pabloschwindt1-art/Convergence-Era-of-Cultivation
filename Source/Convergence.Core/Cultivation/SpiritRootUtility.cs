using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Cambiar la raíz de un colono (docs/modulos/RAICES.md): subirla o bajarla de grado, quitarla y darla. La raíz puede venir
    /// de genes (Biotech), del rasgo o de un implante (raíz artificial); todas las vías pasan por acá.
    /// </summary>
    public static class SpiritRootUtility
    {
        private static List<SpiritRootGradeDef> grades;

        public static List<SpiritRootGradeDef> Grades =>
            grades ??= DefDatabase<SpiritRootGradeDef>.AllDefsListForReading.OrderBy(g => g.order).ToList();

        public static SpiritRootGradeDef Next(SpiritRootGradeDef g) => g == null ? null : Grades.FirstOrDefault(x => x.order > g.order);
        public static SpiritRootGradeDef Previous(SpiritRootGradeDef g) => g == null ? null : Grades.LastOrDefault(x => x.order < g.order);

        /// <summary>La raíz sale de un implante: no se extirpa ni mejora (es una máquina).</summary>
        public static bool IsArtificial(Pawn pawn) =>
            pawn?.health?.hediffSet?.hediffs.Any(h => h.def.HasModExtension<DefModExtension_SpiritRoot>()) ?? false;

        /// <summary>Sube la raíz un grado, sin pasar de <paramref name="cap"/> (null = sin tope).</summary>
        public static bool TryRaise(Pawn pawn, SpiritRootGradeDef cap = null)
        {
            var root = CompCultivator.For(pawn)?.Root;
            if (root == null || IsArtificial(pawn)) return false;
            var next = Next(root.grade);
            if (next == null || (cap != null && next.order > cap.order)) return false;
            SetGrade(pawn, next);
            return true;
        }

        /// <summary>Baja la raíz un grado; si ya era la más baja, la pierde. Devuelve si le queda raíz.</summary>
        public static bool Lower(Pawn pawn)
        {
            var root = CompCultivator.For(pawn)?.Root;
            if (root == null || IsArtificial(pawn)) return root != null;
            var prev = Previous(root.grade);
            if (prev == null)
            {
                RemoveRoot(pawn);
                return false;
            }
            SetGrade(pawn, prev);
            return true;
        }

        /// <summary>Cambia el grado conservando los elementos y de dónde viene (gen endógeno, xenogen o rasgo).</summary>
        public static void SetGrade(Pawn pawn, SpiritRootGradeDef grade)
        {
            var comp = CompCultivator.For(pawn);
            if (comp == null || grade == null) return;
            var gradeGenes = pawn.genes?.GenesListForReading.Where(g => g.def.GetModExtension<DefModExtension_SpiritRoot>()?.grade != null).ToList();
            if (gradeGenes != null && gradeGenes.Count > 0)
            {
                bool xeno = gradeGenes.Any(g => g.Active && pawn.genes.IsXenogene(g));
                foreach (var g in gradeGenes) pawn.genes.RemoveGene(g);
                var trait = pawn.story?.traits?.GetTrait(CVG_DefOf.CVG_SpiritRoot);
                if (trait != null) pawn.story.traits.RemoveTrait(trait);
                var gene = GeneFor(grade, null);
                if (gene != null)
                {
                    pawn.genes.AddGene(gene, xeno);
                    return;
                }
            }
            var old = pawn.story?.traits?.GetTrait(CVG_DefOf.CVG_SpiritRoot);
            var elements = comp.Root?.elements.ToList();
            if (old != null) pawn.story.traits.RemoveTrait(old);
            pawn.story?.traits?.GainTrait(new Trait(CVG_DefOf.CVG_SpiritRoot, grade.traitDegree, forced: true));
            if (elements != null && elements.Count > 0) comp.SetTraitRootElements(elements);
        }

        /// <summary>Quita la raíz (genes, rasgo e implante) y devuelve una copia de cómo era.</summary>
        public static SpiritRoot RemoveRoot(Pawn pawn)
        {
            var comp = CompCultivator.For(pawn);
            var root = comp?.Root;
            if (root == null) return null;
            var copy = new SpiritRoot { grade = root.grade, elements = new List<QiElementDef>(root.elements) };
            CultivationGenerator.RemoveRoot(pawn);
            foreach (var h in pawn.health.hediffSet.hediffs.Where(h => h.def.HasModExtension<DefModExtension_SpiritRoot>()).ToList())
                pawn.health.RemoveHediff(h);
            comp.Notify_RootSourcesChanged();
            return copy;
        }

        /// <summary>Da una raíz ajena (trasplante): reemplaza la que tuviera. Con Biotech entra como xenogenes (no se hereda).</summary>
        public static void ImplantRoot(Pawn pawn, SpiritRootGradeDef grade, List<QiElementDef> elements)
        {
            var comp = CompCultivator.For(pawn);
            if (comp == null || grade == null) return;
            if (comp.HasRoot) RemoveRoot(pawn);
            if (elements == null || elements.Count == 0) elements = CultivationGenerator.RandomElements();
            if (ModsConfig.BiotechActive && pawn.genes != null)
            {
                var gradeGene = GeneFor(grade, null);
                var elementGenes = elements.Select(e => GeneFor(null, e)).ToList();
                if (gradeGene != null && elementGenes.All(g => g != null))
                {
                    pawn.genes.AddGene(gradeGene, xenogene: true);
                    foreach (var g in elementGenes) pawn.genes.AddGene(g, xenogene: true);
                    return;
                }
            }
            pawn.story.traits.GainTrait(new Trait(CVG_DefOf.CVG_SpiritRoot, grade.traitDegree, forced: true));
            comp.SetTraitRootElements(elements);
        }

        /// <summary>Despierta una raíz nueva en quien no tiene (avance del cuerpo, ritual).</summary>
        public static bool Awaken(Pawn pawn, SpiritRootGradeDef grade)
        {
            var comp = CompCultivator.For(pawn);
            if (comp == null || comp.HasRoot || grade == null) return false;
            var elements = CultivationGenerator.RandomElements();
            CultivationGenerator.GiveRoot(pawn, grade, elements);
            comp.SetTraitRootElements(elements);
            comp.Notify_RootSourcesChanged();
            return comp.HasRoot;
        }

        private static GeneDef GeneFor(SpiritRootGradeDef grade, QiElementDef element) =>
            DefDatabase<GeneDef>.AllDefsListForReading.FirstOrDefault(g =>
            {
                var ext = g.GetModExtension<DefModExtension_SpiritRoot>();
                return ext != null && ext.grade == grade && ext.element == element;
            });

        /// <summary>Subida de raíz al avanzar de reino; despertar de raíz en el camino del cuerpo. Devuelve el texto para la carta.</summary>
        public static string Notify_Ascended(Pawn pawn, CultivationRealmDef target, float quality)
        {
            var comp = CompCultivator.For(pawn);
            if (comp == null || target == null) return null;
            var root = comp.Root;
            if (root == null)
            {
                if (target.awakensRootChance > 0f && Rand.Chance(target.awakensRootChance) && Awaken(pawn, target.awakensRootGrade ?? Grades.First()))
                    return "CVG_Root_AwakenedByRealm".Translate(pawn.Named("PAWN"), comp.Root.grade.label, target.LabelCap);
                return null;
            }
            if (target.path != CVG_DefOf.CVG_Path_Qi || IsArtificial(pawn)) return null;
            var g = root.grade;
            if (g.ascendRaiseChance <= 0f || (g.ascendRaiseNeedsTribulation && !target.tribulation)) return null;
            float chance = UnityEngine.Mathf.Lerp(g.ascendRaiseChance, g.ascendRaiseChancePerfect, UnityEngine.Mathf.Clamp01(quality));
            if (!Rand.Chance(chance) || !TryRaise(pawn)) return null;
            return "CVG_Root_RaisedByRealm".Translate(pawn.Named("PAWN"), g.label, comp.Root.grade.label);
        }
    }
}
