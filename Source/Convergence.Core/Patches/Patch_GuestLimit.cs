using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace Convergence
{
    /// <summary>
    /// Tope de grupos de huéspedes a la vez (ajuste <see cref="ConvergenceSettings.maxGuestGroups"/>, 2 por defecto, 0 = sin tope). Con muchas
    /// facciones amistosas (sectas) las caravanas y los visitantes de vanilla, de otros mods y de las historias van por canales que no se
    /// miran entre sí: llegaban 3-4 grupos juntos, con decenas de personajes y animales, y el juego se ponía lento.
    /// </summary>
    public static class GuestLimit
    {
        /// <summary>Grupos de huéspedes en el mapa: visitantes o caravanas de comercio de otra facción no hostil.</summary>
        public static int Count(Map map) => map?.lordManager == null ? 0 : map.lordManager.lords.Count(l =>
            (l.LordJob is LordJob_VisitColony || l.LordJob is LordJob_TradeWithColony)
            && l.faction != null && !l.faction.IsPlayer && !l.faction.HostileTo(Faction.OfPlayer) && l.ownedPawns.Any(p => p.Spawned));

        public static int Max => ConvergenceMod.Settings?.maxGuestGroups ?? 2;

        public static bool Full(Map map) => Max > 0 && Count(map) >= Max;
    }

    /// <summary>
    /// Caravanas, visitantes, viajeros y recaudadores (todos los <see cref="IncidentWorker_NeutralGroup"/>) no llegan si el mapa ya está lleno
    /// de huéspedes; el narrador lo vuelve a intentar más adelante. Los forzados (misiones, eventos con su propio control) pasan igual.
    /// </summary>
    [HarmonyPatch(typeof(IncidentWorker), nameof(IncidentWorker.CanFireNow))]
    public static class Patch_IncidentWorker_CanFireNow_GuestLimit
    {
        public static void Postfix(IncidentWorker __instance, IncidentParms parms, ref bool __result)
        {
            if (!__result || parms == null || parms.forced || !(__instance is IncidentWorker_NeutralGroup) || !(parms.target is Map map)) return;
            if (GuestLimit.Full(map)) __result = false;
        }
    }
}
