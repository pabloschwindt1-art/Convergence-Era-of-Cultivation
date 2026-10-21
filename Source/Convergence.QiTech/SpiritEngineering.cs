using HarmonyLib;
using Verse;

namespace Convergence.QiTech
{
    /// <summary>
    /// En un ResearchProjectDef: no se puede empezar (y por eso tampoco completar con el banco) antes de la Convergencia
    /// (<see cref="GameComponent_Convergence.Active"/>). Es un bloqueo real de <c>CanStartNow</c>, no un aviso; la descripción
    /// del proyecto lo dice (texto Keyed). Lo ya investigado en partidas guardadas sigue terminado.
    /// </summary>
    public class DefModExtension_RequiresConvergence : DefModExtension
    {
    }

    [HarmonyPatch(typeof(ResearchProjectDef), nameof(ResearchProjectDef.CanStartNow), MethodType.Getter)]
    public static class Patch_ResearchProjectDef_CanStartNow
    {
        public static void Postfix(ResearchProjectDef __instance, ref bool __result)
        {
            if (__result && !GameComponent_Convergence.Active && __instance.HasModExtension<DefModExtension_RequiresConvergence>())
                __result = false;
        }
    }

    [StaticConstructorOnStartup]
    public static class SpiritEngineeringStartup
    {
        static SpiritEngineeringStartup()
        {
            // Las traducciones (DefInjected y Keyed) ya están cargadas: se agrega el requisito al final de la descripción.
            foreach (var p in DefDatabase<ResearchProjectDef>.AllDefsListForReading)
                if (p.HasModExtension<DefModExtension_RequiresConvergence>())
                    p.description += "\n\n" + "CVG_QT_RequiresConvergence".Translate();
        }
    }
}
