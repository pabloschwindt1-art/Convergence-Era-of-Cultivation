using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>En un <c>PawnKindDef</c> de animal salvaje: solo aparece de forma natural en los biomas una vez ocurrida la Convergencia.</summary>
    public class DefModExtension_PostConvergence : DefModExtension
    {
    }

    [HarmonyPatch(typeof(BiomeDef), nameof(BiomeDef.CommonalityOfAnimal))]
    public static class Patch_BiomeDef_CommonalityOfAnimal
    {
        public static void Postfix(PawnKindDef animalDef, ref float __result)
        {
            if (__result > 0f && animalDef != null && animalDef.GetModExtension<DefModExtension_PostConvergence>() != null
                && !GameComponent_Convergence.Active)
                __result = 0f;
        }
    }

    /// <summary>
    /// En un <c>ThingDef</c> de planta: tras un rayo de tribulación puede brotar cerca del impacto
    /// (Champiñón de la Tribulación…).
    /// </summary>
    public class DefModExtension_TribulationSprout : DefModExtension
    {
        public float chance = 0.25f;
        public int radius = 3;
        /// <summary>Crecimiento inicial (0–1).</summary>
        public float initialGrowth = 0.2f;
    }

    public static class TribulationSproutUtility
    {
        public static void OnStrike(Map map, IntVec3 loc)
        {
            foreach (var def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                var ext = def.GetModExtension<DefModExtension_TribulationSprout>();
                if (ext == null || def.plant == null || !Rand.Chance(ext.chance)) continue;
                for (int tries = 0; tries < 12; tries++)
                {
                    var c = loc + GenRadial.RadialPattern[Rand.Range(0, GenRadial.NumCellsInRadius(ext.radius))];
                    if (!c.InBounds(map) || !c.Walkable(map) || c.GetEdifice(map) != null || c.GetPlant(map) != null) continue;
                    if (!def.CanEverPlantAt(c, map, out _, canWipePlantsExceptTree: false)) continue;
                    var plant = (Plant)GenSpawn.Spawn(ThingMaker.MakeThing(def), c, map);
                    plant.Growth = ext.initialGrowth;
                    break;
                }
            }
        }
    }
}

namespace Convergence
{
    /// <summary>
    /// Peces con <see cref="DefModExtension_PostConvergence"/>: antes de la Convergencia no se pescan; si salen, se cambian por un pez
    /// común del mismo cuerpo de agua (el qi todavía no volvió a los ríos).
    /// </summary>
    [HarmonyLib.HarmonyPatch(typeof(RimWorld.FishingUtility), nameof(RimWorld.FishingUtility.GetCatchesFor))]
    public static class Patch_FishingUtility_GetCatchesFor
    {
        public static void Postfix(Verse.Pawn pawn, Verse.IntVec3 cell, System.Collections.Generic.List<Verse.Thing> __result)
        {
            if (GameComponent_Convergence.Active || __result == null || pawn?.Map == null) return;
            for (int i = 0; i < __result.Count; i++)
            {
                var t = __result[i];
                if (t?.def == null || !t.def.HasModExtension<DefModExtension_PostConvergence>()) continue;
                var water = RimWorld.FishingUtility.GetWaterBody(cell, pawn.Map);
                Verse.ThingDef common = null;
                if (water == null || !water.CommonFishIncludingExtras.Where(f => !f.HasModExtension<DefModExtension_PostConvergence>()).TryRandomElement(out common))
                    continue;
                var swap = Verse.ThingMaker.MakeThing(common);
                swap.stackCount = t.stackCount;
                __result[i] = swap;
            }
        }
    }
}
