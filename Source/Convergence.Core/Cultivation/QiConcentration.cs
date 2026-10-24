using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Concentración de qi (feng shui básico): qi ambiental + qi de los edificios y de los suelos (por celda) con
    /// <see cref="DefModExtension_FengShui"/>.
    /// En una habitación cerrada es un stat de habitación (cacheado por el juego); al aire libre se suman los
    /// edificios cercanos. Pendiente: armonía de los cinco elementos, vetas de qi y formaciones.
    /// </summary>
    public static class QiConcentrationUtility
    {
        private static readonly HashSet<Thing> counted = new HashSet<Thing>();

        public static DefModExtension_QiConcentration Props =>
            CVG_DefOf.CVG_QiConcentration.GetModExtension<DefModExtension_QiConcentration>();

        public static float Ambient => Props?.ambient ?? 1f;

        /// <summary>
        /// Qi de una cosa: el de su def más el de su material (paredes, columnas o muebles de piedra espiritual, jade o rocas
        /// wuxia llevan <see cref="DefModExtension_FengShui"/> en el stuff).
        /// </summary>
        public static float FengShuiQi(Thing t) =>
            (t.def.GetModExtension<DefModExtension_FengShui>()?.qi ?? 0f) + (t.Stuff?.GetModExtension<DefModExtension_FengShui>()?.qi ?? 0f);

        /// <summary>Qi que aporta el suelo de una celda (pisos de jade, piedra espiritual…): <c>DefModExtension_FengShui.qi</c> por celda.</summary>
        public static float TerrainQi(IntVec3 c, Map map) => c.GetTerrain(map)?.GetModExtension<DefModExtension_FengShui>()?.qi ?? 0f;

        /// <summary>
        /// Fuentes de qi por área que no son edificios ni suelos (formaciones de reunión de qi…): módulos externos añaden aquí una
        /// función (celda, mapa) → qi extra. Se suma a la concentración de la habitación o del exterior.
        /// </summary>
        public static readonly List<System.Func<IntVec3, Map, float>> AreaProviders = new List<System.Func<IntVec3, Map, float>>();

        private static float AreaBonus(IntVec3 cell, Map map)
        {
            float total = 0f;
            for (int i = 0; i < AreaProviders.Count; i++) total += AreaProviders[i](cell, map);
            return total;
        }

        /// <summary>Concentración de qi en una celda.</summary>
        public static float ConcentrationAt(IntVec3 cell, Map map) => BaseConcentrationAt(cell, map) + AreaBonus(cell, map);

        private static float BaseConcentrationAt(IntVec3 cell, Map map)
        {
            var room = cell.GetRoom(map);
            if (room != null && room.ProperRoom && !room.PsychologicallyOutdoors)
            {
                float score = room.GetStat(CVG_DefOf.CVG_QiConcentration);
                // GetStat devuelve roomlessScore si el juego no calcula stats de esta habitación (muy grande).
                if (score > 0f) return score;
            }
            return OutdoorConcentration(cell, map);
        }

        private static float OutdoorConcentration(IntVec3 cell, Map map)
        {
            float total = MapQi.AmbientFor(map, Ambient);
            float radius = Props?.outdoorRadius ?? 2.9f;
            counted.Clear();
            int cells = GenRadial.NumCellsInRadius(radius);
            for (int i = 0; i < cells; i++)
            {
                var c = cell + GenRadial.RadialPattern[i];
                if (!c.InBounds(map)) continue;
                total += TerrainQi(c, map);
                var things = c.GetThingList(map);
                for (int j = 0; j < things.Count; j++)
                {
                    var t = things[j];
                    if (counted.Add(t)) total += FengShuiQi(t);
                }
            }
            counted.Clear();
            return total;
        }

        /// <summary>Multiplicador de la velocidad de cultivo según la concentración.</summary>
        public static float CultivationFactor(float concentration)
        {
            var curve = Props?.cultivationFactorCurve;
            return curve != null ? curve.Evaluate(concentration) : 1f;
        }

        /// <summary>Multiplicador de la probabilidad de avance de reino según la concentración.</summary>
        public static float BreakthroughFactor(float concentration)
        {
            var curve = Props?.breakthroughFactorCurve;
            return curve != null ? curve.Evaluate(concentration) : 1f;
        }
    }

    /// <summary>Stat de habitación <c>CVG_QiConcentration</c>: qi ambiental + edificios de feng shui de la habitación.</summary>
    public class RoomStatWorker_QiConcentration : RoomStatWorker
    {
        private static readonly HashSet<Thing> counted = new HashSet<Thing>();

        public override float GetScore(Room room)
        {
            var map = room.Map;
            float total = MapQi.AmbientFor(map, QiConcentrationUtility.Ambient);
            counted.Clear();
            foreach (var c in room.Cells) total += QiConcentrationUtility.TerrainQi(c, map);
            var things = room.ContainedAndAdjacentThings;
            for (int i = 0; i < things.Count; i++)
            {
                var t = things[i];
                if (counted.Add(t)) total += QiConcentrationUtility.FengShuiQi(t);
            }
            counted.Clear();
            return total;
        }
    }
}
