using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Textos de linaje generados desde los datos (hediff de efectos y <see cref="BloodlineDef"/>), para que la descripción
    /// de los genes y la pestaña Cultivo no se desincronicen del XML.
    /// </summary>
    public static class BloodlineText
    {
        private static readonly Dictionary<(BloodlineDef, int), List<string>> effectLines = new Dictionary<(BloodlineDef, int), List<string>>();

        /// <summary>Efectos de la pureza <paramref name="index"/> al despertar: stats y contras del hediff y capacidades.</summary>
        public static List<string> EffectLines(BloodlineDef bloodline, int index)
        {
            if (effectLines.TryGetValue((bloodline, index), out var lines)) return lines;
            lines = new List<string>();
            var purity = bloodline.PurityAt(index);
            if (purity != null)
            {
                var stage = HediffStage(purity, index);
                if (stage != null)
                    foreach (var entry in HediffStatsUtility.SpecialDisplayStats(stage, null))
                    {
                        string value = entry.ValueString;
                        if (value == "x100%" || value == "100%") continue; // dolor neutro y similares
                        lines.Add(entry.LabelCap + ": " + value);
                    }
                for (int i = 0; i <= index; i++)
                    foreach (var ability in bloodline.purities[i].abilities)
                        lines.Add("CVG_Bloodline_AbilityLine".Translate(ability.LabelCap).Resolve());
            }
            effectLines[(bloodline, index)] = lines;
            return lines;
        }

        private static HediffStage HediffStage(BloodlinePurity purity, int index)
        {
            var stages = purity.hediff?.stages;
            if (stages.NullOrEmpty()) return null;
            float severity = purity.hediffSeverity >= 0f ? purity.hediffSeverity : index + 1;
            HediffStage found = null;
            foreach (var stage in stages)
                if (stage.minSeverity <= severity) found = stage;
            return found;
        }

        /// <summary>Estado de los dones de la sangre en esta partida.</summary>
        public static string StatusLine() =>
            (GameComponent_Convergence.Active ? "CVG_Bloodline_StatusAwake" : "CVG_Bloodline_StatusLatent").Translate().Resolve();

        /// <summary>Texto que se agrega a la descripción de un gen de pureza.</summary>
        public static string IdentityGeneText(DefModExtension_Bloodline ext)
        {
            int index = ext.bloodline.IndexOf(ext.purity);
            var sb = new StringBuilder();
            sb.AppendLine("CVG_Bloodline_EffectsHeader".Translate(ext.bloodline.label, ext.purity.label).Resolve().Colorize(ColoredText.TipSectionTitleColor));
            foreach (var line in EffectLines(ext.bloodline, index)) sb.AppendLine("  - " + line);
            var purity = ext.bloodline.PurityAt(index);
            if (ext.bloodline.affinities.Count > 0 && purity.affinityCultivationFactor != 1f)
                sb.AppendLine("  - " + "CVG_Bloodline_AffinityLine".Translate(ext.bloodline.affinities.Select(e => e.label).ToCommaList(),
                    purity.affinityCultivationFactor.ToStringPercent()).Resolve());
            if (Current.Game != null) sb.Append(StatusLine());
            return sb.ToString().TrimEnd();
        }

        private static readonly string[] Roman = { "I", "II", "III", "IV", "V", "VI" };

        public static string RomanTier(int tier) => tier >= 1 && tier <= Roman.Length ? Roman[tier - 1] : tier.ToString();

        /// <summary>Texto de un gen de sangre por niveles: nivel, mecánicas propias y si están activas.</summary>
        public static string TierGeneText(GeneDef gene, DefModExtension_BloodlineGeneTier ext)
        {
            int max = BloodlineUtility.TierGenes.Where(g => g.GetModExtension<DefModExtension_BloodlineGeneTier>().group == ext.group)
                .Max(g => g.GetModExtension<DefModExtension_BloodlineGeneTier>().tier);
            var sb = new StringBuilder();
            sb.AppendLine("CVG_BloodGene_TierHeader".Translate(ext.group.LabelCap, RomanTier(ext.tier), RomanTier(max)).Resolve()
                .Colorize(ColoredText.TipSectionTitleColor));
            sb.AppendLine(ext.group.bloodline == null
                ? "CVG_BloodGene_Common".Translate().Resolve()
                : "CVG_BloodGene_Signature".Translate(ext.group.bloodline.label).Resolve());
            bool special = false;
            var regen = gene.GetModExtension<DefModExtension_BloodRegeneration>();
            if (regen != null) { sb.AppendLine("  - " + "CVG_BloodGene_Regen".Translate(regen.intervalDays.ToString("0.#")).Resolve()); special = true; }
            var lives = gene.GetModExtension<DefModExtension_ExtraLives>();
            if (lives != null)
            {
                sb.AppendLine("  - " + (lives.rechargeDays > 0f
                    ? "CVG_BloodGene_LivesRecharge".Translate(lives.lives, lives.rechargeDays.ToString("0"))
                    : "CVG_BloodGene_Lives".Translate(lives.lives)).Resolve());
                special = true;
            }
            var aura = gene.GetModExtension<DefModExtension_GeneAura>();
            if (aura?.hediff != null) { sb.AppendLine("  - " + "CVG_BloodGene_Aura".Translate(aura.radius.ToString("0"), aura.hediff.label).Resolve()); special = true; }
            var retaliation = gene.GetModExtension<DefModExtension_Retaliation>();
            if (retaliation != null) { sb.AppendLine("  - " + "CVG_BloodGene_Retaliation".Translate(retaliation.chance.ToStringPercent(), retaliation.amount.ToString("0")).Resolve()); special = true; }
            if (ext.awakenedAbilities != null)
                foreach (var ability in ext.awakenedAbilities) { sb.AppendLine("  - " + "CVG_Bloodline_AbilityLine".Translate(ability.LabelCap).Resolve()); special = true; }
            if (ext.tier < max) sb.AppendLine("CVG_BloodGene_Upgrade".Translate().Resolve());
            if (special && Current.Game != null) sb.Append(StatusLine());
            return sb.ToString().TrimEnd();
        }

        /// <summary>Linaje al que pertenece un gen de aspecto (o null).</summary>
        public static BloodlineDef AspectOwner(GeneDef gene) =>
            DefDatabase<BloodlineDef>.AllDefsListForReading.FirstOrDefault(b => b.purities.Any(p => p.aspectGenes.Contains(gene)));
    }

    /// <summary>
    /// Descripción de genes de linaje: los de pureza listan los efectos al despertar y si ya están activos; los de aspecto
    /// aclaran que son cosméticos. Se agrega al final, en cada lectura (el estado cambia con la Convergencia).
    /// </summary>
    [HarmonyPatch(typeof(GeneDef), nameof(GeneDef.DescriptionFull), MethodType.Getter)]
    public static class Patch_GeneDef_DescriptionFull
    {
        public static void Postfix(GeneDef __instance, ref string __result)
        {
            var ext = __instance.GetModExtension<DefModExtension_Bloodline>();
            if (ext?.bloodline != null && ext.purity != null)
            {
                __result = __result.TrimEnd() + "\n\n" + BloodlineText.IdentityGeneText(ext);
                return;
            }
            var tierExt = __instance.GetModExtension<DefModExtension_BloodlineGeneTier>();
            if (tierExt?.group != null)
            {
                __result = __result.TrimEnd() + "\n\n" + BloodlineText.TierGeneText(__instance, tierExt);
                return;
            }
            if (!BloodlineUtility.AnyBloodlines) return;
            var owner = BloodlineText.AspectOwner(__instance);
            if (owner != null)
                __result = __result.TrimEnd() + "\n\n" + "CVG_Bloodline_AspectNote".Translate(owner.label).Resolve();
        }
    }
}
