using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Los edificios del mod (CVG_*) no dibujan las cajas oscuras del juego: ni la sombra rectangular (<c>shadowData</c>) ni el halo de
    /// borde (<c>castEdgeShadows</c>). El arte ya trae su propio contorno y esas cajas, grandes y de borde duro, ensuciaban cada pieza.
    /// Los muros y vallas enlazados (muro bajo, empalizada) quedan intactos: llevan la misma iluminación y sombras que la barricada de
    /// vanilla (halo de borde y sombra de sol). Lo de vanilla y de otros mods no se toca.
    /// </summary>
    [HarmonyPatch(typeof(Graphic_Shadow), nameof(Graphic_Shadow.Print))]
    public static class Patch_GraphicShadow_Print
    {
        public static bool Prefix(Thing thing) => !IsOurs(thing?.def);

        private static readonly System.Collections.Generic.Dictionary<ThingDef, bool> ours = new System.Collections.Generic.Dictionary<ThingDef, bool>();

        /// <summary>¿Es nuestro? Con caché: DrawWorker corre en cada frame por cada sombra dibujada.</summary>
        internal static bool IsOurs(ThingDef def)
        {
            if (def == null) return false;
            if (!ours.TryGetValue(def, out bool v)) ours[def] = v = def.defName.StartsWith("CVG_");
            return v;
        }
    }

    [HarmonyPatch(typeof(Graphic_Shadow), nameof(Graphic_Shadow.DrawWorker))]
    public static class Patch_GraphicShadow_DrawWorker
    {
        public static bool Prefix(ThingDef thingDef) => !Patch_GraphicShadow_Print.IsOurs(thingDef);
    }

    [StaticConstructorOnStartup]
    public static class NoEdgeShadowsStartup
    {
        static NoEdgeShadowsStartup()
        {
            foreach (var def in DefDatabase<ThingDef>.AllDefs.Where(d => d.defName.StartsWith("CVG_") && d.category == ThingCategory.Building))
            {
                if (def.graphicData != null && def.graphicData.linkType != LinkDrawerType.None) continue;
                def.castEdgeShadows = false;
                // la sombra de sol estatica tambien dibuja una caja oscura bajo muebles y mesas
                def.staticSunShadowHeight = 0f;
            }
        }
    }
}
