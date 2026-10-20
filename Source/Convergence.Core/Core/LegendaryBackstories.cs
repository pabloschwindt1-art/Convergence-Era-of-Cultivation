using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Historias de fondo por tiers (1 comunes … 4 legendarias). El patch de XML agrega a las facciones filtros de historias con
    /// las categorías <c>CVG_Tier2..4</c> (uno con la infancia del tier y otro con la adultez, nunca las dos juntas) y pesos base
    /// 0,25 / 0,08 / 0,02. Acá se multiplican por el ajuste "frecuencia de historias raras": así en el mundo no todos son líderes
    /// de secta y las rotas cuestan muchos clics de aleatorizar.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class LegendaryBackstories
    {
        private static readonly Dictionary<string, float> BaseWeight = new Dictionary<string, float>
        {
            { "CVG_Tier2", 0.12f }, { "CVG_Tier3", 0.08f }, { "CVG_Tier4", 0.02f },
        };

        static LegendaryBackstories() => Apply();

        public static void Apply()
        {
            float factor = ConvergenceMod.Settings?.legendaryBackstoryChance ?? 1f;
            foreach (var f in DefDatabase<FactionDef>.AllDefsListForReading) Set(f.backstoryFilters, factor);
            foreach (var k in DefDatabase<PawnKindDef>.AllDefsListForReading) Set(k.backstoryFiltersOverride, factor);
        }

        private static void Set(List<BackstoryCategoryFilter> filters, float factor)
        {
            if (filters == null) return;
            foreach (var filter in filters)
                foreach (var kv in BaseWeight)
                    if ((filter.categoriesAdulthood?.Contains(kv.Key) ?? false) || (filter.categoriesChildhood?.Contains(kv.Key) ?? false))
                        filter.commonality = kv.Value * factor;
        }
    }
}
