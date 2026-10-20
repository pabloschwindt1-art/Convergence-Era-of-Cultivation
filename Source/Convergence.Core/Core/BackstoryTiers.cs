using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Modificadores de stats en una historia de fondo (vanilla no los tiene): +10 % de velocidad de minado, etc. Se aplican con
    /// <see cref="StatPart_Backstory"/>, que se agrega solo a los stats que alguna historia usa.
    /// </summary>
    public class DefModExtension_BackstoryStats : DefModExtension
    {
        public List<StatModifier> statOffsets;
        public List<StatModifier> statFactors;
    }

    public class StatPart_Backstory : StatPart
    {
        private static IEnumerable<DefModExtension_BackstoryStats> Exts(Pawn p)
        {
            if (p?.story == null) yield break;
            var c = p.story.Childhood?.GetModExtension<DefModExtension_BackstoryStats>();
            if (c != null) yield return c;
            var a = p.story.Adulthood?.GetModExtension<DefModExtension_BackstoryStats>();
            if (a != null) yield return a;
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!(req.Thing is Pawn p)) return;
            foreach (var e in Exts(p))
            {
                if (e.statOffsets != null) val += e.statOffsets.GetStatOffsetFromList(parentStat);
                if (e.statFactors != null) val *= e.statFactors.GetStatFactorFromList(parentStat);
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!(req.Thing is Pawn p)) return null;
            var sb = new StringBuilder();
            foreach (var bs in new[] { p.story?.Childhood, p.story?.Adulthood })
            {
                var e = bs?.GetModExtension<DefModExtension_BackstoryStats>();
                if (e == null) continue;
                float off = e.statOffsets?.GetStatOffsetFromList(parentStat) ?? 0f;
                float fac = e.statFactors?.GetStatFactorFromList(parentStat) ?? 1f;
                if (off != 0f) sb.AppendLine(bs.TitleCapFor(p.gender) + ": " + parentStat.ValueToString(off, ToStringNumberSense.Offset));
                if (fac != 1f) sb.AppendLine(bs.TitleCapFor(p.gender) + ": " + fac.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Factor));
            }
            return sb.Length == 0 ? null : sb.ToString().TrimEnd();
        }
    }

    /// <summary>
    /// Tiers de historias de fondo (1 común … 4 legendaria) y su color en la ficha del colono, para reconocer de un vistazo las
    /// buenas al aleatorizar. Las nuestras dicen su tier por categoría (CVG_Tier2..4; el resto, 1); las vanilla y de otros mods se
    /// clasifican por un puntaje: habilidades sumadas, trabajos que no puede hacer, rasgos forzados y modificadores.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class BackstoryTiers
    {
        public static readonly Color[] TierColors =
        {
            new Color(0.55f, 0.85f, 0.45f), // 1 verde
            new Color(0.40f, 0.65f, 1.00f), // 2 azul
            new Color(0.75f, 0.45f, 1.00f), // 3 violeta
            new Color(1.00f, 0.62f, 0.20f), // 4 naranja
        };

        private static readonly Dictionary<BackstoryDef, int> cache = new Dictionary<BackstoryDef, int>();

        static BackstoryTiers()
        {
            // StatPart en cada stat que use alguna historia.
            var stats = new HashSet<StatDef>();
            foreach (var b in DefDatabase<BackstoryDef>.AllDefsListForReading)
            {
                var e = b.GetModExtension<DefModExtension_BackstoryStats>();
                if (e == null) continue;
                foreach (var m in (e.statOffsets ?? new List<StatModifier>()).Concat(e.statFactors ?? new List<StatModifier>())) if (m.stat != null) stats.Add(m.stat);
            }
            foreach (var s in stats)
            {
                s.parts ??= new List<StatPart>();
                if (!s.parts.Any(p => p is StatPart_Backstory)) s.parts.Add(new StatPart_Backstory { parentStat = s });
            }
        }

        public static int Tier(BackstoryDef b)
        {
            if (b == null) return 1;
            if (cache.TryGetValue(b, out int t)) return t;
            var cats = b.spawnCategories ?? new List<string>();
            if (cats.Contains("CVG_Tier4")) t = 4;
            else if (cats.Contains("CVG_Tier3")) t = 3;
            else if (cats.Contains("CVG_Tier2")) t = 2;
            else if (cats.Contains("CVG_Cultivation")) t = 1;
            else t = ForeignTier(b);
            cache[b] = t;
            return t;
        }

        private static Dictionary<BackstoryDef, int> foreignTiers;

        /// <summary>
        /// Historias ajenas (vanilla y otros mods): se puntúan todas las de su etapa y se reparten por percentil — 5 % legendarias,
        /// 10 % fuertes, 30 % buenas, el resto comunes —, así la proporción no depende de umbrales ni de cuántas agregue otro mod.
        /// </summary>
        private static int ForeignTier(BackstoryDef b)
        {
            if (foreignTiers == null)
            {
                foreignTiers = new Dictionary<BackstoryDef, int>();
                foreach (var slot in new[] { BackstorySlot.Childhood, BackstorySlot.Adulthood })
                {
                    var list = DefDatabase<BackstoryDef>.AllDefsListForReading.Where(x => x.slot == slot && !(x.spawnCategories?.Any(c => c.StartsWith("CVG_")) ?? false))
                        .OrderByDescending(Score).ToList();
                    for (int i = 0; i < list.Count; i++)
                    {
                        float pct = (float)i / list.Count;
                        foreignTiers[list[i]] = pct < 0.05f ? 4 : pct < 0.15f ? 3 : pct < 0.45f ? 2 : 1;
                    }
                }
            }
            return foreignTiers.TryGetValue(b, out int t) ? t : 1;
        }

        /// <summary>Puntaje de una historia: habilidades sumadas, menos trabajos que no puede hacer, más rasgos forzados.</summary>
        private static float Score(BackstoryDef b)
        {
            float s = b.skillGains?.Sum(g => g.amount) ?? 0;
            int disabled = 0;
            foreach (WorkTags tag in System.Enum.GetValues(typeof(WorkTags)))
                if (tag != WorkTags.None && (b.workDisables & tag) != 0 && Mathf.IsPowerOfTwo((int)tag)) disabled++;
            s -= disabled * 1.5f;
            // Rasgos forzados: suman si son buenos (grado positivo o sin penalización de valor) y restan si son malos (Pirómano…).
            if (b.forcedTraits != null)
                foreach (var ft in b.forcedTraits)
                {
                    var deg = ft.def?.DataAtDegree(ft.degree);
                    bool bad = ft.degree < 0 || (deg != null && deg.marketValueFactorOffset < 0f);
                    s += bad ? -1.5f : 1.5f;
                }
            return s;
        }

        public static string Colored(string text, BackstoryDef b) => text.Colorize(TierColors[Mathf.Clamp(Tier(b), 1, 4) - 1]);
    }

    /// <summary>Solo mientras se dibuja la ficha del colono, los títulos de historias salen con el color de su tier.</summary>
    [HarmonyPatch(typeof(CharacterCardUtility), nameof(CharacterCardUtility.DrawCharacterCard))]
    public static class Patch_CharacterCard_TierColors
    {
        public static bool Drawing;
        public static void Prefix() => Drawing = true;
        public static void Finalizer() => Drawing = false;
    }

    [HarmonyPatch(typeof(BackstoryDef), nameof(BackstoryDef.TitleCapFor))]
    public static class Patch_BackstoryTitle_TierColor
    {
        public static void Postfix(BackstoryDef __instance, ref string __result)
        {
            if (Patch_CharacterCard_TierColors.Drawing && !__result.NullOrEmpty()) __result = BackstoryTiers.Colored(__result, __instance);
        }
    }

    /// <summary>La descripción de la historia lista sus modificadores de stats.</summary>
    [HarmonyPatch(typeof(BackstoryDef), nameof(BackstoryDef.FullDescriptionFor))]
    public static class Patch_BackstoryDescription_Stats
    {
        public static void Postfix(BackstoryDef __instance, ref TaggedString __result)
        {
            // Leyenda del color: "Historia buena (tier 2 de 4)".
            int tier = BackstoryTiers.Tier(__instance);
            __result = ("CVG_Backstory_Tier" + tier).Translate().ToString().Colorize(BackstoryTiers.TierColors[tier - 1]) + "\n\n" + __result;
            var e = __instance.GetModExtension<DefModExtension_BackstoryStats>();
            if (e == null) return;
            var sb = new StringBuilder();
            foreach (var m in e.statOffsets ?? new List<StatModifier>()) sb.AppendLine("  - " + m.stat.LabelCap + ": " + m.stat.ValueToString(m.value, ToStringNumberSense.Offset));
            foreach (var m in e.statFactors ?? new List<StatModifier>()) sb.AppendLine("  - " + m.stat.LabelCap + ": " + m.value.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Factor));
            if (sb.Length > 0) __result += "\n\n" + "CVG_Backstory_StatsHeader".Translate() + "\n" + sb.ToString().TrimEnd();
        }
    }
}
