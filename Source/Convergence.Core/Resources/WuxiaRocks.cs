using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Convergence
{
    /// <summary>Marca una roca natural (no de recurso) como roca wuxia: el ajuste "Mundo wuxia" la prefiere.</summary>
    public class DefModExtension_WuxiaRock : DefModExtension
    {
        /// <summary>Biomas donde esta roca está siempre entre los tipos de roca de la casilla (p. ej. obsidiana en campos de lava).</summary>
        public List<BiomeDef> preferredBiomes;
    }

    /// <summary>
    /// "Mundo wuxia" (ajuste del Core): las casillas eligen sus 2-3 tipos de roca solo entre las rocas wuxia, con la misma
    /// semilla por casilla que vanilla. Los biomas con rocas forzadas no cambian. Sin el ajuste se mezclan con las vanilla.
    /// </summary>
    [HarmonyPatch(typeof(World), nameof(World.NaturalRockTypesIn))]
    public static class Patch_World_NaturalRockTypesIn
    {
        private static List<ThingDef> wuxiaRocks;

        private static List<ThingDef> preferredRocks;

        public static void Postfix(PlanetTile tile, ref IEnumerable<ThingDef> __result)
        {
            if (tile.Valid && tile.Tile.PrimaryBiome.forceRockTypes != null) return;
            ApplyWuxiaWorld(tile, ref __result);
            // Roca preferente del bioma (siempre, con o sin "Mundo wuxia"): reemplaza el primer tipo si falta.
            preferredRocks ??= DefDatabase<ThingDef>.AllDefsListForReading
                .Where(d => d.GetModExtension<DefModExtension_WuxiaRock>()?.preferredBiomes != null).ToList();
            if (!tile.Valid || preferredRocks.Count == 0) return;
            var biome = tile.Tile.PrimaryBiome;
            foreach (var rock in preferredRocks)
            {
                if (!rock.GetModExtension<DefModExtension_WuxiaRock>().preferredBiomes.Contains(biome)) continue;
                var list = __result.ToList();
                if (list.Contains(rock)) continue;
                if (list.Count > 0) list[0] = rock;
                else list.Add(rock);
                __result = list;
            }
        }

        private static void ApplyWuxiaWorld(PlanetTile tile, ref IEnumerable<ThingDef> __result)
        {
            if (!ConvergenceMod.Settings.wuxiaRocksOnly) return;
            wuxiaRocks ??= DefDatabase<ThingDef>.AllDefsListForReading
                .Where(d => d.IsNonResourceNaturalRock && d.HasModExtension<DefModExtension_WuxiaRock>() && !d.building.biomeSpecific).ToList();
            if (wuxiaRocks.Count < 2) return;
            Rand.PushState();
            Rand.Seed = tile.GetHashCode() ^ 0x5C0A;
            int count = System.Math.Min(Rand.RangeInclusive(2, 3), wuxiaRocks.Count);
            __result = wuxiaRocks.InRandomOrder().Take(count).ToList();
            Rand.PopState();
        }
    }
}
