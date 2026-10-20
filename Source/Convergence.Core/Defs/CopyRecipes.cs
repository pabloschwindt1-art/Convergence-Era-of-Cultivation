using System.Linq;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Versión wuxia de una mesa de trabajo vanilla (principio n.º 2): la mesa hace todas las recetas de las mesas de
    /// <see cref="from"/>, incluidas las que otros mods agreguen a esas mesas (se copian al arrancar, después de cargar todo).
    /// <code>&lt;li Class="Convergence.DefModExtension_CopyRecipes"&gt;&lt;from&gt;&lt;li&gt;FueledStove&lt;/li&gt;&lt;/from&gt;&lt;/li&gt;</code>
    /// </summary>
    public class DefModExtension_CopyRecipes : DefModExtension
    {
        public List<ThingDef> from = new List<ThingDef>();
    }

    [StaticConstructorOnStartup]
    public static class CopyRecipesStartup
    {
        static CopyRecipesStartup()
        {
            var cache = AccessTools.Field(typeof(ThingDef), "allRecipesCached");
            int benches = 0;
            foreach (var bench in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                var ext = bench.GetModExtension<DefModExtension_CopyRecipes>();
                if (ext == null || ext.from.NullOrEmpty()) continue;
                foreach (var recipe in DefDatabase<RecipeDef>.AllDefsListForReading)
                {
                    if (recipe.recipeUsers == null) continue;
                    foreach (var source in ext.from)
                        if (recipe.recipeUsers.Contains(source) && !recipe.recipeUsers.Contains(bench)) { recipe.recipeUsers.Add(bench); break; }
                }
                foreach (var source in ext.from)
                {
                    if (source.recipes.NullOrEmpty()) continue;
                    bench.recipes ??= new List<RecipeDef>();
                    foreach (var r in source.recipes) if (!bench.recipes.Contains(r)) bench.recipes.Add(r);
                }
                // Sin esto nadie trabaja en la mesa: los WorkGiver_DoBill vanilla solo miran sus fixedBillGiverDefs.
                foreach (var giver in DefDatabase<WorkGiverDef>.AllDefsListForReading)
                {
                    if (giver.fixedBillGiverDefs == null || giver.fixedBillGiverDefs.Contains(bench)) continue;
                    if (ext.from.Any(giver.fixedBillGiverDefs.Contains)) giver.fixedBillGiverDefs.Add(bench);
                }
                cache?.SetValue(bench, null);
                benches++;
            }
            if (benches > 0) Log.Message($"[Convergence] {benches} wuxia workbench(es) copy the recipes of their vanilla counterpart.");
        }
    }
}
