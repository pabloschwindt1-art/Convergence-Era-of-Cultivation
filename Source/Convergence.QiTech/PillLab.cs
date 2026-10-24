using HarmonyLib;
using RimWorld;
using Verse;

namespace Convergence.QiTech
{
    /// <summary>
    /// El laboratorio de píldoras de qi hace todas las píldoras del horno de alquimia (las que haya, sin depender de Alchemy):
    /// a cada receta de píldora que se hace en un horno de alquimia se le suma el laboratorio como mesa.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class PillLabRecipes
    {
        static PillLabRecipes()
        {
            var lab = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_QT_PillLab");
            var furnace = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_AlchemyFurnace");
            if (lab == null || furnace == null) return;
            int n = 0;
            foreach (var recipe in DefDatabase<RecipeDef>.AllDefsListForReading)
            {
                if (recipe.recipeUsers == null || !recipe.recipeUsers.Contains(furnace) || recipe.recipeUsers.Contains(lab)) continue;
                var pill = recipe.ProducedThingDef;
                // Píldora: ingerible con calidad (CrafterInjection, que marca al alquimista, puede correr después que esto).
                if (pill == null || !pill.IsIngestible || !pill.HasComp(typeof(CompQuality))) continue;
                recipe.recipeUsers.Add(lab);
                n++;
            }
            AccessTools.Field(typeof(ThingDef), "allRecipesCached").SetValue(lab, null);
            Log.Message($"[Convergence] Qi-Tech: the qi pill laboratory makes {n} pills.");
        }
    }
}
