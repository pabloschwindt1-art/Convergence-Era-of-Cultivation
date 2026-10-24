using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>Raíz sellada en jade (docs/modulos/RAICES.md): un ThingDef por grado; el comp guarda elementos y donantes.</summary>
    public class CompProperties_SealedRoot : CompProperties
    {
        public SpiritRootGradeDef grade;

        public CompProperties_SealedRoot() => compClass = typeof(CompSealedRoot);
    }

    public class CompSealedRoot : ThingComp
    {
        public List<QiElementDef> elements = new List<QiElementDef>();
        /// <summary>A quién perteneció (o "las raíces de tres cultivadores" si salió de una fusión).</summary>
        public string donor;

        public CompProperties_SealedRoot Props => (CompProperties_SealedRoot)props;
        public SpiritRootGradeDef Grade => Props.grade;

        public override void PostPostMake()
        {
            base.PostPostMake();
            // Las que dan eventos, comerciantes o el modo desarrollador nacen con elementos al azar.
            if (elements.Count == 0) elements = CultivationGenerator.RandomElements();
        }

        public override string CompInspectStringExtra()
        {
            var sb = new StringBuilder();
            if (elements.Count > 0) sb.Append("CVG_SealedRoot_Elements".Translate(elements.Select(e => e.LabelCap.ToString()).ToCommaList()));
            if (!donor.NullOrEmpty())
            {
                if (sb.Length > 0) sb.AppendLine();
                sb.Append("CVG_SealedRoot_Donor".Translate(donor));
            }
            return sb.ToString();
        }

        public override bool AllowStackWith(Thing other) => false;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref elements, "elements", LookMode.Def);
            Scribe_Values.Look(ref donor, "donor");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                elements ??= new List<QiElementDef>();
                elements.RemoveAll(e => e == null);
            }
        }
    }

    public static class SealedRootUtility
    {
        public static ThingDef DefFor(SpiritRootGradeDef grade) =>
            DefDatabase<ThingDef>.AllDefsListForReading.FirstOrDefault(d => d.GetCompProperties<CompProperties_SealedRoot>()?.grade == grade);

        public static Thing Make(SpiritRootGradeDef grade, List<QiElementDef> elements, string donor)
        {
            var def = DefFor(grade);
            if (def == null) return null;
            var thing = ThingMaker.MakeThing(def);
            var comp = thing.TryGetComp<CompSealedRoot>();
            if (comp != null)
            {
                if (elements != null && elements.Count > 0) comp.elements = new List<QiElementDef>(elements);
                comp.donor = donor;
            }
            return thing;
        }

        /// <summary>Elementos de una fusión: el más repetido primero; si purifica, uno menos que el promedio (mínimo uno).</summary>
        public static List<QiElementDef> FusedElements(List<CompSealedRoot> roots, bool purify)
        {
            var all = roots.SelectMany(r => r.elements).ToList();
            if (all.Count == 0) return CultivationGenerator.RandomElements();
            var ranked = all.GroupBy(e => e).OrderByDescending(g => g.Count()).ThenBy(_ => Rand.Value).Select(g => g.Key).ToList();
            int count = Mathf.RoundToInt((float)all.Count / roots.Count);
            if (purify) count--;
            return ranked.Take(Mathf.Clamp(count, 1, ranked.Count)).ToList();
        }
    }

    /// <summary>Fusión de raíces selladas (3 → 1 del grado siguiente). Va en el RecipeDef; el producto sale del XML.</summary>
    public class DefModExtension_RootFusion : DefModExtension
    {
        public float chance = 0.75f;
        public SkillDef skill;
        public int skillMin;
        public float chancePerSkillLevel = 0.02f;
        /// <summary>La fusión del Dao purifica (un elemento menos); la de la máquina no.</summary>
        public bool purifies;
    }

    /// <summary>
    /// Fusión: si sale bien, el producto hereda elementos y donantes; si falla, vuelve una sola raíz del grado de entrada
    /// (se pierden dos). GenRecipe.MakeRecipeProducts recibe los ingredientes, que ya están destruidos pero conservan sus comps.
    /// </summary>
    [HarmonyLib.HarmonyPatch(typeof(GenRecipe), nameof(GenRecipe.MakeRecipeProducts))]
    public static class Patch_GenRecipe_RootFusion
    {
        public static IEnumerable<Thing> Postfix(IEnumerable<Thing> __result, RecipeDef recipeDef, Pawn worker, List<Thing> ingredients)
        {
            var ext = recipeDef?.GetModExtension<DefModExtension_RootFusion>();
            if (ext == null)
            {
                foreach (var t in __result) yield return t;
                yield break;
            }
            var roots = ingredients?.Select(t => t.TryGetComp<CompSealedRoot>()).Where(c => c != null).ToList() ?? new List<CompSealedRoot>();
            float chance = ext.chance;
            if (ext.skill != null && worker?.skills != null)
                chance += (worker.skills.GetSkill(ext.skill).Level - ext.skillMin) * ext.chancePerSkillLevel;
            if (roots.Count > 0 && !Rand.Chance(Mathf.Clamp01(chance)))
            {
                foreach (var t in __result) t.Destroy();
                var back = roots.RandomElement();
                var thing = SealedRootUtility.Make(back.Grade, back.elements, back.donor);
                if (worker?.Faction == Faction.OfPlayer)
                    Messages.Message("CVG_RootFusion_Failed".Translate(worker.Named("PAWN")), thing != null ? new LookTargets(worker) : null,
                        MessageTypeDefOf.NegativeEvent);
                if (thing != null) yield return thing;
                yield break;
            }
            foreach (var t in __result)
            {
                var comp = t.TryGetComp<CompSealedRoot>();
                if (comp != null && roots.Count > 0)
                {
                    comp.elements = SealedRootUtility.FusedElements(roots, ext.purifies);
                    comp.donor = "CVG_SealedRoot_FusedDonor".Translate(roots.Count, roots.Select(r => r.donor).Where(d => !d.NullOrEmpty()).ToCommaList());
                }
                yield return t;
            }
        }
    }
}
