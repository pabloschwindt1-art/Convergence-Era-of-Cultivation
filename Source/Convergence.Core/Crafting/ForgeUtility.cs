using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Marca una mesa de trabajo como forja (herrería, banco de fabricación, la futura Forja Espiritual…). Quien trabaja en
    /// ella usa los stats <c>CVG_ForgeSpeed</c> (velocidad) y <c>CVG_ForgeQuality</c> (niveles de calidad extra).
    /// </summary>
    public class DefModExtension_Forge : DefModExtension
    {
    }

    /// <summary>Hook genérico de forja: velocidad (StatPart sobre el stat de velocidad de sus recetas) y calidad (Harmony).</summary>
    [StaticConstructorOnStartup]
    public static class ForgeUtility
    {
        public static readonly StatDef ForgeSpeed = DefDatabase<StatDef>.GetNamedSilentFail("CVG_ForgeSpeed");
        public static readonly StatDef ForgeQuality = DefDatabase<StatDef>.GetNamedSilentFail("CVG_ForgeQuality");

        static ForgeUtility()
        {
            if (ForgeSpeed == null) return;
            // Stats de velocidad de las recetas que se hacen en una forja: reciben el multiplicador de forja.
            var forges = DefDatabase<ThingDef>.AllDefsListForReading.Where(d => d.HasModExtension<DefModExtension_Forge>()).ToList();
            var stats = new HashSet<StatDef>();
            foreach (var recipe in DefDatabase<RecipeDef>.AllDefsListForReading)
                if (recipe.workSpeedStat != null && recipe.AllRecipeUsers.Any(u => forges.Contains(u))) stats.Add(recipe.workSpeedStat);
            foreach (var stat in stats)
            {
                stat.parts ??= new List<StatPart>();
                if (!stat.parts.Any(p => p is StatPart_Forge)) stat.parts.Add(new StatPart_Forge { parentStat = stat });
            }
        }

        /// <summary>La mesa del trabajo actual del colono, si es una forja.</summary>
        public static Thing ForgeOf(Pawn pawn)
        {
            if (pawn?.CurJob?.bill?.billStack?.billGiver is Thing bench && bench.def.HasModExtension<DefModExtension_Forge>()) return bench;
            return null;
        }
    }

    /// <summary>Multiplica la velocidad de trabajo por <c>CVG_ForgeSpeed</c> mientras el colono trabaja en una forja.</summary>
    public class StatPart_Forge : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.Thing is Pawn pawn && ForgeUtility.ForgeOf(pawn) != null) val *= pawn.GetStatValue(ForgeUtility.ForgeSpeed);
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!(req.Thing is Pawn pawn) || ForgeUtility.ForgeSpeed == null) return null;
            float f = pawn.GetStatValue(ForgeUtility.ForgeSpeed);
            return Mathf.Approximately(f, 1f) ? null : ForgeUtility.ForgeSpeed.LabelCap + ": x" + f.ToStringPercent();
        }
    }

    /// <summary>
    /// Calidad de forja: lo creado en una forja sube tantos niveles como <c>CVG_ForgeQuality</c> (la parte decimal es una
    /// probabilidad). Hasta obra maestra; legendario solo con inspiración, como en vanilla.
    /// </summary>
    [HarmonyPatch(typeof(QualityUtility), nameof(QualityUtility.GenerateQualityCreatedByPawn), typeof(Pawn), typeof(SkillDef), typeof(bool))]
    public static class Patch_QualityUtility_Forge
    {
        public static void Postfix(Pawn pawn, ref QualityCategory __result)
        {
            if (ForgeUtility.ForgeQuality == null || ForgeUtility.ForgeOf(pawn) == null || __result >= QualityCategory.Masterwork) return;
            float bonus = pawn.GetStatValue(ForgeUtility.ForgeQuality);
            if (bonus <= 0f) return;
            int levels = Mathf.FloorToInt(bonus) + (Rand.Chance(bonus - Mathf.Floor(bonus)) ? 1 : 0);
            __result = (QualityCategory)Mathf.Min((int)__result + levels, (int)QualityCategory.Masterwork);
        }
    }
}
