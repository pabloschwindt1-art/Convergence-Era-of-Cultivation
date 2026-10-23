using System.Linq;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Para los autotests: deja un rectángulo del mapa como superficie limpia (sin roca, edificios, plantas ni objetos; suelo firme, sin
    /// techo ni niebla). Pedido del usuario: los tests de pelea ponían a los enemigos dentro de una montaña y quedaban trabados.
    /// No toca pawns ni cadáveres.
    /// </summary>
    public static class TestArena
    {
        public static CellRect Clear(Map map, CellRect rect)
        {
            rect = rect.ClipInsideMap(map);
            foreach (var c in rect.Cells)
            {
                foreach (var t in c.GetThingList(map).ToList())
                    if (!(t is Pawn) && !(t is Corpse) && t.def.destroyable && !t.Destroyed) t.Destroy();
                var terrain = c.GetTerrain(map);
                if (terrain == null || terrain.passability == Traversability.Impassable || terrain.IsWater || !terrain.affordances.Contains(TerrainAffordanceDefOf.Light))
                    map.terrainGrid.SetTerrain(c, TerrainDefOf.Soil);
                map.roofGrid.SetRoof(c, null);
                map.fogGrid.Unfog(c);
            }
            return rect;
        }

        /// <summary>Despeja un rectángulo de <paramref name="width"/>×<paramref name="height"/> centrado en el centro del mapa y devuelve ese centro.</summary>
        public static IntVec3 ClearAtCenter(Map map, int width, int height)
        {
            var center = map.Center;
            Clear(map, CellRect.CenteredOn(center, width, height));
            return center;
        }
    }
}
