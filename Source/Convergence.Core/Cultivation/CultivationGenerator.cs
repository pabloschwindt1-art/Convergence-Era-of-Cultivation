using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Reparte raíces espirituales y cuerpos especiales al generar humanoides.
    /// Con Biotech usa genes (heredables); sin Biotech, rasgos.
    /// </summary>
    public static class CultivationGenerator
    {
        public static List<QiElementDef> RandomElements()
        {
            int count = Rand.Value switch
            {
                < 0.40f => 1,
                < 0.75f => 2,
                < 0.95f => 3,
                _ => Rand.RangeInclusive(4, 5)
            };
            var all = DefDatabase<QiElementDef>.AllDefsListForReading.Where(e => !e.variant).ToList();
            var result = all.InRandomOrder().Take(System.Math.Min(count, all.Count)).ToList();
            // Raíces variantes (Trueno = Madera + Fuego): raras, reemplazan a los elementos de los que nacen.
            foreach (var variant in DefDatabase<QiElementDef>.AllDefsListForReading)
            {
                if (!variant.variant || variant.variantOf.NullOrEmpty() || !variant.variantOf.All(result.Contains)) continue;
                if (!Rand.Chance(variant.variantChance)) continue;
                result.RemoveAll(variant.variantOf.Contains);
                result.Add(variant);
            }
            return result;
        }

        /// <summary>Agrega un elemento a la raíz del colono (gen con Biotech, elementos guardados sin él). Devuelve si cambió.</summary>
        public static bool AddRootElement(Pawn pawn, QiElementDef element)
        {
            var comp = CompCultivator.For(pawn);
            if (comp == null || element == null) return false;
            comp.Sync();
            if (comp.Root == null || comp.Root.elements.Contains(element)) return false;
            var gene = GeneFor(e => e.element == element && e.grade == null);
            bool usesGenes = pawn.genes != null && pawn.genes.GenesListForReading.Any(g => g.def.GetModExtension<DefModExtension_SpiritRoot>()?.element != null);
            if (usesGenes && gene != null) pawn.genes.AddGene(gene, xenogene: false);
            else comp.SetTraitRootElements(comp.Root.elements.Concat(new[] { element }).ToList());
            comp.Sync();
            comp.SyncRealm();
            return true;
        }

        public static void TryGiveCultivationTraits(Pawn pawn)
        {
            if (pawn?.RaceProps == null || !pawn.RaceProps.Humanlike || pawn.story?.traits == null) return;
            var comp = CompCultivator.For(pawn);
            if (comp == null) return;

            comp.Sync();
            if (!comp.HasRoot && Rand.Chance(ConvergenceMod.Settings.spiritRootChance))
                GiveRoot(pawn, RandomGrade(), RandomElements());

            comp.Sync();
            if (comp.SpecialBody == null)
                TryGiveSpecialBody(pawn, comp.HasRoot);

            BloodlineUtility.TryGiveRandomBloodline(pawn);
            comp.Sync();
            TraitGrantsHediffUtility.Apply(pawn);

            // Cultivo ya iniciado (DISENO §4.1d): raro, también en el selector de personajes; latente antes de la
            // Convergencia. Los tipos de pawn con reino fijado por XML (sectas) usan el suyo.
            if (pawn.kindDef?.GetModExtension<DefModExtension_CultivatorPawnKind>() == null)
                PreCultivation.TryApply(pawn, comp);

            // Tipos de pawn con DefModExtension_CultivatorPawnKind (sectas, etc.): raíz, reino y manuales fijados por XML.
            if (GameComponent_Convergence.Active && Current.ProgramState == ProgramState.Playing && !(pawn.Faction?.IsPlayer ?? false))
                CultivatorPawnKindUtility.Apply(pawn);

            // Cultivadores ajenos ya nacen con un manual y técnicas acordes a su reino (M5).
            if (GameComponent_Convergence.Active && Current.ProgramState == ProgramState.Playing && !(pawn.Faction?.IsPlayer ?? false))
                TechniqueUtility.GiveNpcTechniques(pawn);
        }

        public static SpiritRootGradeDef RandomGrade() =>
            DefDatabase<SpiritRootGradeDef>.AllDefsListForReading.RandomElementByWeight(g => g.commonality);

        public static void GiveRoot(Pawn pawn, SpiritRootGradeDef grade, List<QiElementDef> elements)
        {
            if (ModsConfig.BiotechActive && pawn.genes != null)
            {
                var gradeGene = GeneFor(e => e.grade == grade && e.element == null);
                var elementGenes = elements.Select(el => GeneFor(e => e.element == el && e.grade == null)).ToList();
                if (gradeGene != null && elementGenes.All(g => g != null))
                {
                    pawn.genes.AddGene(gradeGene, xenogene: false);
                    foreach (var g in elementGenes) pawn.genes.AddGene(g, xenogene: false);
                    return;
                }
            }

            pawn.story.traits.GainTrait(new Trait(CVG_DefOf.CVG_SpiritRoot, grade.traitDegree, forced: true));
        }

        /// <summary>Quita la raíz espiritual (genes de raíz o el rasgo): p. ej. a los discípulos de una secta del cuerpo, que no la usan.</summary>
        public static void RemoveRoot(Pawn pawn)
        {
            if (pawn.genes != null)
                foreach (var g in pawn.genes.GenesListForReading.Where(g => g.def.HasModExtension<DefModExtension_SpiritRoot>()).ToList())
                    pawn.genes.RemoveGene(g);
            var t = pawn.story?.traits?.GetTrait(CVG_DefOf.CVG_SpiritRoot);
            if (t != null) pawn.story.traits.RemoveTrait(t);
        }

        private static void TryGiveSpecialBody(Pawn pawn, bool hasRoot)
        {
            foreach (var def in DefDatabase<TraitDef>.AllDefsListForReading.InRandomOrder())
            {
                var ext = def.GetModExtension<DefModExtension_SpecialBody>();
                if (ext == null || (ext.rootlessOnly && hasRoot)) continue;
                if (!Rand.Chance(ext.chance)) continue;
                if (!TraitConflictUtility.CanGain(pawn, def, 0)) continue;

                pawn.story.traits.GainTrait(new Trait(def, 0, forced: true));
                return;
            }
        }

        private static GeneDef GeneFor(System.Func<DefModExtension_SpiritRoot, bool> match) =>
            DefDatabase<GeneDef>.AllDefsListForReading.FirstOrDefault(g =>
            {
                var ext = g.GetModExtension<DefModExtension_SpiritRoot>();
                return ext != null && match(ext);
            });
    }

    [HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), typeof(PawnGenerationRequest))]
    public static class Patch_PawnGenerator_GeneratePawn
    {
        public static void Postfix(Pawn __result)
        {
            // Animales generados tras la Convergencia (fauna, migraciones, manadas): pueden ser bestias espirituales.
            if (__result?.RaceProps != null && __result.RaceProps.Animal && GameComponent_Convergence.Active
                && Current.ProgramState == ProgramState.Playing)
                SpiritBeastUtility.TryAwaken(__result);
            CultivationGenerator.TryGiveCultivationTraits(__result);
            // Los rasgos tienen lógica: quita los vanilla que contradicen a un rasgo de cultivo (p. ej. Piel de Hierro + enfermizo).
            TraitConflictUtility.ResolveConflicts(__result);
        }
    }
}
