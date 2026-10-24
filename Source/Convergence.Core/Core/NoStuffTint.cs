using HarmonyLib;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Edificio hecho de material (<c>MadeFromStuff</c>) cuyo arte ya trae sus colores: el juego lo multiplica por el color del material
    /// (granito gris, madera marrón…) y lo ensucia. Con esta extensión el edificio sigue teniendo el material para costo y resistencia,
    /// pero se dibuja con su arte tal cual. Se usa en el muro bajo de piedra y la empalizada de bambú.
    /// </summary>
    public class DefModExtension_NoStuffTint : DefModExtension
    {
    }

    [HarmonyPatch(typeof(Building), nameof(Building.DrawColor), MethodType.Getter)]
    public static class Patch_Building_DrawColor_NoStuffTint
    {
        private static readonly System.Collections.Generic.Dictionary<ThingDef, bool> noTint = new System.Collections.Generic.Dictionary<ThingDef, bool>();

        public static void Postfix(Building __instance, ref Color __result)
        {
            var def = __instance.def;
            if (!noTint.TryGetValue(def, out bool v)) noTint[def] = v = def.HasModExtension<DefModExtension_NoStuffTint>();
            if (v) __result = Color.white;
        }
    }
}
