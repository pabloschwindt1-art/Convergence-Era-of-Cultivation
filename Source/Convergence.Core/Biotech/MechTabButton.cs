using HarmonyLib;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// El botón "Mecas" de la barra: vanilla lo oculta mientras no haya ningún meca en la colonia, aunque alguien ya sea
    /// mecanitor (recién instalado el mecaenlace o el Hilo del Corazón de Marioneta no se ve y parece que falta el menú).
    /// Con un mecanitor en el mapa el botón se muestra siempre. Solo levanta el "oculto por no haber mecas": las demás
    /// razones por las que vanilla lo desactiva (sin mapa, planificando una caravana, eligiendo un tile) se respetan.
    /// </summary>
    [HarmonyPatch(typeof(MainButtonWorker_ToggleMechTab), nameof(MainButtonWorker_ToggleMechTab.Disabled), MethodType.Getter)]
    public static class Patch_MainButtonWorker_ToggleMechTab_Disabled
    {
        public static void Postfix(ref bool __result)
        {
            if (!__result) return;
            var map = Find.CurrentMap;
            if (map == null) return;
            if (Find.WorldRoutePlanner.Active || Find.TilePicker.Active) return;
            var colonists = map.mapPawns.FreeColonistsSpawned;
            for (int i = 0; i < colonists.Count; i++)
                if (colonists[i].mechanitor != null) { __result = false; return; }
        }
    }
}
