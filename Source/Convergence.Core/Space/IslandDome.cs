using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    // Supervivencia en el espacio, versión wuxia (docs/modulos/ESPACIO.md): la Formación de Cúpula Celestial es una barrera de qi por área.
    // Odyssey mide el vacío por habitación (VacuumUtility.GetVacuum) y la temperatura por habitación (GenTemperature.TryGetDirectAirTemperatureForCell);
    // dentro del radio de una cúpula abastecida de qi las dos se corrigen, así que el aire y el calor "se quedan" aunque no haya muros.

    public class CompProperties_CelestialDome : CompProperties
    {
        public float radius = 22.9f;
        /// <summary>Temperatura (°C) que mantiene dentro de su radio.</summary>
        public float temperature = 20f;

        public CompProperties_CelestialDome() => compClass = typeof(CompCelestialDome);
    }

    public class CompCelestialDome : ThingComp
    {
        public CompProperties_CelestialDome Props => (CompProperties_CelestialDome)props;

        /// <summary>Encendida y con qi: la cúpula funciona.</summary>
        public bool Working
        {
            get
            {
                if (!parent.Spawned || parent.Faction != Faction.OfPlayer) return false;
                var flick = parent.GetComp<CompFlickable>();
                if (flick != null && !flick.SwitchIsOn) return false;
                var qi = parent.GetComp<CompQiNode>();
                return qi == null || qi.Powered;
            }
        }

        public override string CompInspectStringExtra() =>
            parent.Spawned ? (Working ? "CVG_Dome_Working" : "CVG_Dome_Off").Translate(Props.radius.ToString("F0"), Props.temperature.ToString("F0")).ToString() : null;

        public override void PostDrawExtraSelectionOverlays()
        {
            if (parent.Spawned) GenDraw.DrawRadiusRing(parent.Position, Props.radius);
        }
    }

    public static class DomeUtility
    {
        private static Map cachedMap;
        private static int cachedTick = -999;
        private static readonly List<CompCelestialDome> cachedDomes = new List<CompCelestialDome>();

        /// <summary>Cúpulas de la facción del jugador en el mapa que funcionan ahora (se recalcula cada medio segundo).</summary>
        public static List<CompCelestialDome> ActiveDomes(Map map)
        {
            int now = Find.TickManager?.TicksGame ?? 0;
            if (map == cachedMap && now - cachedTick < 30 && now >= cachedTick) return cachedDomes;
            cachedMap = map;
            cachedTick = now;
            cachedDomes.Clear();
            if (map == null) return cachedDomes;
            var def = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_CelestialDome");
            if (def != null)
                foreach (var t in map.listerThings.ThingsOfDef(def))
                {
                    var c = t.TryGetComp<CompCelestialDome>();
                    if (c != null && c.Working) cachedDomes.Add(c);
                }
            return cachedDomes;
        }

        public static void ResetCache() => cachedTick = -999;

        public static CompCelestialDome DomeAt(IntVec3 cell, Map map)
        {
            var list = ActiveDomes(map);
            for (int i = 0; i < list.Count; i++)
            {
                var d = list[i];
                if ((cell - d.parent.Position).LengthHorizontalSquared <= d.Props.radius * d.Props.radius) return d;
            }
            return null;
        }
    }

    /// <summary>Dentro de una cúpula abastecida no hay vacío.</summary>
    [HarmonyPatch(typeof(VacuumUtility), nameof(VacuumUtility.GetVacuum))]
    public static class Patch_VacuumUtility_GetVacuum_Dome
    {
        public static void Postfix(IntVec3 cell, Map map, ref float __result)
        {
            if (__result <= 0f || map == null || DomeUtility.DomeAt(cell, map) == null) return;
            __result = 0f;
        }
    }

    /// <summary>Dentro de una cúpula abastecida la temperatura es la de la cúpula.</summary>
    [HarmonyPatch(typeof(GenTemperature), nameof(GenTemperature.TryGetDirectAirTemperatureForCell))]
    public static class Patch_GenTemperature_Dome
    {
        public static void Postfix(IntVec3 c, Map map, ref bool __result, ref float temperature)
        {
            if (!__result || map == null || !map.Biome.inVacuum) return;
            var dome = DomeUtility.DomeAt(c, map);
            if (dome != null) temperature = dome.Props.temperature;
        }
    }
}
