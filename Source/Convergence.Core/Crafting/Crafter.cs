using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>Quién hizo el objeto: alquimista (píldoras), forjador (armas), maestro de talismanes o artesano.</summary>
    public class CompProperties_Crafter : CompProperties
    {
        /// <summary>Alchemist, Smith, TalismanMaster o Artisan: elige el oficio del creador inventado.</summary>
        public string role = "Artisan";

        public CompProperties_Crafter()
        {
            compClass = typeof(CompCrafter);
        }
    }

    /// <summary>
    /// "Hecho por" en las píldoras, armas y talismanes de Convergence. Si lo fabricó un colono queda su nombre; si llegó de otro lado
    /// (botín, comercio, favores, eventos) se inventa un creador de lore la primera vez que se mira: "Mo Qing, un viejo alquimista".
    /// En la ficha suma también lo que hace esta píldora con su calidad (qi, toxicidad) y lo que pide la receta.
    /// </summary>
    public class CompCrafter : ThingComp
    {
        private string crafter;
        private string role;
        private bool byColonist;
        private bool mixed;

        public CompProperties_Crafter Props => (CompProperties_Crafter)props;

        private static readonly string[] Families =
        {
            "Mo", "Qian", "Hu", "Ning", "Bai", "Shen", "Lu", "Yao", "Tang", "Su", "Gu", "Lin", "Xu", "Fang", "Ye", "Jiang", "Wei", "Zhou", "Han", "Qin",
        };
        private static readonly string[] Givens =
        {
            "Qing", "Ning", "Yun", "Shou", "Lan", "Feng", "Ming", "Hua", "Jin", "Ruo", "Wen", "Xian", "Yi", "Zhen", "Lao", "Heng", "Yan", "Bo", "Shan", "Tie",
        };

        public string CrafterLabel
        {
            get
            {
                EnsureCrafter();
                if (mixed) return "CVG_Crafter_Mixed".Translate();
                return byColonist ? crafter : "CVG_Crafter_Lore".Translate(crafter, ("CVG_Crafter_" + role).Translate()).Resolve();
            }
        }

        private void EnsureCrafter()
        {
            if (!crafter.NullOrEmpty()) return;
            // Determinista por objeto: el mismo creador cada vez que se mira, sin guardar nada hasta que se guarde la partida.
            var rand = new System.Random(parent.thingIDNumber * 7919 + parent.def.shortHash);
            crafter = Families[rand.Next(Families.Length)] + " " + Givens[rand.Next(Givens.Length)];
            int variants = 4;
            role = Props.role + "_" + rand.Next(variants);
            byColonist = false;
        }

        public override void Notify_RecipeProduced(Pawn pawn)
        {
            base.Notify_RecipeProduced(pawn);
            if (pawn == null) return;
            crafter = pawn.Name?.ToStringShort ?? pawn.LabelShort;
            byColonist = true;
            mixed = false;
        }

        public override void PreAbsorbStack(Thing otherStack, int count)
        {
            base.PreAbsorbStack(otherStack, count);
            var other = otherStack.TryGetComp<CompCrafter>();
            if (other == null) return;
            EnsureCrafter();
            other.EnsureCrafter();
            if (other.mixed || other.crafter != crafter || other.byColonist != byColonist) mixed = true;
        }

        public override void PostSplitOff(Thing piece)
        {
            base.PostSplitOff(piece);
            var other = piece.TryGetComp<CompCrafter>();
            if (other == null || other == this) return;
            EnsureCrafter();
            other.crafter = crafter;
            other.role = role;
            other.byColonist = byColonist;
            other.mixed = mixed;
        }

        public override string CompInspectStringExtra() => "CVG_Crafter_MadeBy".Translate() + ": " + CrafterLabel;

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            yield return new StatDrawEntry(StatCategoryDefOf.BasicsImportant, "CVG_Crafter_MadeBy".Translate(), CrafterLabel,
                (byColonist ? "CVG_Crafter_ByColonistDesc" : mixed ? "CVG_Crafter_MixedDesc" : "CVG_Crafter_LoreDesc").Translate(), 4990);

            // Lo que hace ESTA píldora, con su calidad.
            var doers = parent.def.ingestible?.outcomeDoers;
            if (doers != null)
            {
                float factor = PillQuality.FactorOf(parent);
                foreach (var d in doers)
                {
                    if (d is IngestionOutcomeDoer_GainResourceByQuality res && res.path != null)
                        yield return new StatDrawEntry(StatCategoryDefOf.Drug, "CVG_Pill_ThisPill".Translate(res.path.resourceLabel), "+" + (res.amount * factor).ToString("0"),
                            "CVG_Pill_ThisPillDesc".Translate(res.amount.ToString("0"), factor.ToStringPercent()) + "\n\n" + PillQuality.Table(res.amount), 1990);
                    else if (d is IngestionOutcomeDoer_GiveHediff gh && gh.hediffDef != null)
                        yield return new StatDrawEntry(StatCategoryDefOf.Drug, gh.hediffDef.isBad ? "CVG_Pill_SideEffect".Translate() : "CVG_Pill_Effect".Translate(),
                            gh.hediffDef.LabelCap + (gh.hediffDef.isBad ? " +" + (gh.severity > 0f ? gh.severity : gh.hediffDef.initialSeverity).ToStringPercent() : ""),
                            gh.hediffDef.description, 1980, hyperlinks: new[] { new Dialog_InfoCard.Hyperlink(gh.hediffDef) });
                }
            }

            // Lo que pide la receta: nivel de la mesa y habilidad.
            var level = parent.def.GetModExtension<DefModExtension_RequiresWorkbenchLevel>();
            var recipe = DefDatabase<RecipeDef>.AllDefsListForReading.FirstOrDefault(r => r.ProducedThingDef == parent.def);
            level ??= recipe?.GetModExtension<DefModExtension_RequiresWorkbenchLevel>();
            if (level != null && level.level > 1)
                yield return new StatDrawEntry(StatCategoryDefOf.BasicsNonPawn, "CVG_Crafter_BenchLevel".Translate(), level.level.ToString(),
                    "CVG_Crafter_BenchLevelDesc".Translate(), 4400);
            if (recipe?.skillRequirements != null && recipe.skillRequirements.Count > 0)
                yield return new StatDrawEntry(StatCategoryDefOf.BasicsNonPawn, "CVG_Crafter_Skill".Translate(),
                    string.Join(", ", recipe.skillRequirements.Select(s => s.skill.LabelCap + " " + s.minLevel)), "CVG_Crafter_SkillDesc".Translate(), 4390);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref crafter, "crafter");
            Scribe_Values.Look(ref role, "crafterRole");
            Scribe_Values.Look(ref byColonist, "crafterIsColonist");
            Scribe_Values.Look(ref mixed, "crafterMixed");
        }
    }

    /// <summary>
    /// Las píldoras, armas y talismanes de los mods de Convergence llevan <see cref="CompCrafter"/> (sin tocar cada XML): píldoras =
    /// alquimista, armas = forjador, talismanes = maestro de talismanes.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class CrafterInjection
    {
        static CrafterInjection()
        {
            foreach (var def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (def.modContentPack?.PackageId?.StartsWith("prospero1088.convergence") != true || def.IsStuff || def.IsCorpse) continue;
                if (!typeof(ThingWithComps).IsAssignableFrom(def.thingClass) || def.category != ThingCategory.Item) continue;
                string role = def.IsWeapon ? "Smith"
                    : def.defName.Contains("Talisman") ? "TalismanMaster"
                    : def.ingestible?.drugCategory != null && def.ingestible.drugCategory != DrugCategory.None ? "Alchemist"
                    : def.defName.Contains("Pill") || def.defName.Contains("Elixir") ? "Alchemist" : null;
                if (role == null) continue;
                def.comps ??= new List<CompProperties>();
                if (!def.comps.Any(c => c is CompProperties_Crafter)) def.comps.Add(new CompProperties_Crafter { role = role });
            }
        }
    }
}
