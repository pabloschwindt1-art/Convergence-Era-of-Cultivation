using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Veta que solo existe tras la Convergencia (piedra espiritual, jade espiritual, cristal…). En mapas generados antes
    /// de la Convergencia no aparece; al ocurrir el evento, <see cref="convertFrom"/> se transforma en parte y se añaden
    /// <see cref="eventLumps"/> vetas nuevas en la roca del mapa.
    /// </summary>
    public class DefModExtension_ConvergenceMineable : DefModExtension
    {
        /// <summary>Veta vanilla que la Convergencia transforma en esta (p. ej. MineableJade → jade espiritual).</summary>
        public ThingDef convertFrom;
        /// <summary>Fracción de los grupos de <see cref="convertFrom"/> que se transforman.</summary>
        public float convertFraction = 0.5f;
        /// <summary>Vetas nuevas que el evento coloca en la roca natural del mapa.</summary>
        public IntRange eventLumps = IntRange.Zero;
        public IntRange eventLumpSize = new IntRange(3, 8);
        /// <summary>Solo en casillas con temperatura media anual ≤ este valor (p. ej. hierro frío: mapas fríos).</summary>
        public float maxTileTemperature = 999f;
        /// <summary>
        /// Biomas donde aparece. Solo con esta lista, la veta es exclusiva de esos biomas; con <see cref="maxTileTemperature"/>
        /// también, aparece en esos biomas o en casillas lo bastante frías.
        /// </summary>
        public List<BiomeDef> allowedBiomes;
    }

    public static class ConvergenceMineables
    {
        public static bool IsGated(ThingDef def) => def?.HasModExtension<DefModExtension_ConvergenceMineable>() ?? false;

        /// <summary>La veta puede aparecer en este mapa (clima/bioma). Sin mapa, sí.</summary>
        public static bool AllowedOn(ThingDef def, Map map)
        {
            var ext = def?.GetModExtension<DefModExtension_ConvergenceMineable>();
            if (ext == null || map == null || !map.Tile.Valid) return true;
            var tile = map.Tile.Tile;
            bool byTemperature = ext.maxTileTemperature < 999f;
            if (ext.allowedBiomes.NullOrEmpty()) return !byTemperature || tile.temperature <= ext.maxTileTemperature;
            if (ext.allowedBiomes.Contains(tile.PrimaryBiome)) return true;
            return byTemperature && tile.temperature <= ext.maxTileTemperature;
        }

        /// <summary>Lo llama la Convergencia: transforma vetas y añade vetas nuevas en el mapa.</summary>
        public static void OnConvergence(Map map)
        {
            if (map == null) return;
            foreach (var def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                var ext = def.GetModExtension<DefModExtension_ConvergenceMineable>();
                if (ext == null || !AllowedOn(def, map)) continue;
                if (ext.convertFrom != null) Convert(map, ext.convertFrom, def, ext.convertFraction);
                int lumps = ext.eventLumps.RandomInRange;
                for (int i = 0; i < lumps; i++) SpawnLump(map, def, ext.eventLumpSize.RandomInRange);
            }
        }

        /// <summary>Transforma grupos enteros (celdas conectadas) de <paramref name="from"/> en <paramref name="to"/>.</summary>
        private static void Convert(Map map, ThingDef from, ThingDef to, float fraction)
        {
            var visited = new HashSet<IntVec3>();
            foreach (var thing in map.listerThings.ThingsOfDef(from).ToList())
            {
                if (thing.Destroyed || visited.Contains(thing.Position)) continue;
                var group = new List<IntVec3>();
                map.floodFiller.FloodFill(thing.Position, c => c.GetFirstThing(map, from) != null, c => group.Add(c));
                visited.AddRange(group);
                if (!Rand.Chance(fraction)) continue;
                foreach (var c in group) Replace(map, c, to);
            }
        }

        private static void SpawnLump(Map map, ThingDef def, int size)
        {
            if (!CellFinder.TryFindRandomCell(map, c => IsPlainRock(map, c), out var start)) return;
            foreach (var c in GridShapeMaker.IrregularLump(start, map, size))
                if (IsPlainRock(map, c)) Replace(map, c, def);
        }

        private static bool IsPlainRock(Map map, IntVec3 c)
        {
            var edifice = c.GetEdifice(map);
            return edifice != null && edifice.def.building != null && edifice.def.building.isNaturalRock && !edifice.def.building.isResourceRock;
        }

        private static void Replace(Map map, IntVec3 c, ThingDef def)
        {
            var old = c.GetEdifice(map);
            if (old == null) return;
            old.Destroy();
            GenSpawn.Spawn(ThingMaker.MakeThing(def), c, map);
        }
    }

    /// <summary>Las vetas de la Convergencia no aparecen al generar mapas antes de ella, ni fuera de su clima/bioma.</summary>
    [HarmonyPatch(typeof(GenStep_ScatterLumpsMineable), "ChooseThingDef")]
    public static class Patch_GenStep_ScatterLumpsMineable_ChooseThingDef
    {
        public static void Postfix(GenStep_ScatterLumpsMineable __instance, ref ThingDef __result)
        {
            if (!ConvergenceMineables.IsGated(__result) || __instance.forcedDefToScatter != null) return;
            var map = MapGenerator.mapBeingGenerated;
            if (GameComponent_Convergence.Active && ConvergenceMineables.AllowedOn(__result, map)) return;
            float maxValue = __instance.maxValue;
            bool active = GameComponent_Convergence.Active;
            __result = DefDatabase<ThingDef>.AllDefs.RandomElementByWeightWithFallback(d =>
            {
                if (d.building == null) return 0f;
                if (ConvergenceMineables.IsGated(d) && (!active || !ConvergenceMineables.AllowedOn(d, map))) return 0f;
                return d.building.mineableThing != null && d.building.mineableThing.BaseMarketValue > maxValue ? 0f : d.building.mineableScatterCommonality;
            });
        }
    }
}
