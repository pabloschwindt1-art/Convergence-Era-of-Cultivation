using HarmonyLib;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Zonas de cultivo sobre agua poco profunda (el loto): vanilla solo deja crear una zona de cultivo si la casilla tiene fertilidad
    /// (0,5 con Biotech) y el agua tiene 0. Se acepta la casilla si es agua dulce poco profunda o marisma (río, estanque, ribera): lo que
    /// puede sembrarse allí lo decide la planta (<c>completelyIgnoreFertility</c> + <c>wildTerrainTags</c>), no el agua profunda ni el océano.
    /// </summary>
    [HarmonyPatch(typeof(Designator_ZoneAdd_Growing), nameof(Designator_ZoneAdd_Growing.CanDesignateCell))]
    public static class Patch_ZoneAddGrowing_ShallowWater
    {
        public static bool IsShallowFreshWater(IntVec3 c, Map map)
        {
            var tags = c.GetTerrain(map)?.tags;
            return tags != null && (tags.Contains("WaterFreshShallow") || tags.Contains("WaterMarshy"));
        }

        public static void Postfix(IntVec3 c, ref AcceptanceReport __result)
        {
            if (__result.Accepted) return;
            var map = Find.CurrentMap;
            if (map == null || !c.InBounds(map) || !IsShallowFreshWater(c, map)) return;
            if (!Designator_ZoneAdd.IsZoneableCell(c, map).Accepted) return;
            var zone = map.zoneManager.ZoneAt(c);
            if (zone != null && !(zone is Zone_Growing)) return;
            __result = true;
        }
    }
}
