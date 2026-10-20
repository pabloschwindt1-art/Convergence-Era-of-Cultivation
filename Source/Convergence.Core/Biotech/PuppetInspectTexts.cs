using HarmonyLib;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// El panel de una marioneta usa las líneas de los mecanoides de vanilla ("Supervisor", "Descontrolado", "Energía del meca"). Para las
    /// marionetas se muestran con la jerga de los titiriteros: maestro, hilos sueltos, qi. Los mecanoides de vanilla no cambian.
    /// </summary>
    public static class PuppetInspectTexts
    {
        public static bool IsPuppet(Pawn pawn) => pawn?.def?.GetModExtension<DefModExtension_PuppetTier>() != null;

        /// <summary>Cambia en un texto ya traducido las frases de vanilla por las de las marionetas (claves CVG_Puppet_*).</summary>
        public static string Wuxia(string text)
        {
            if (text.NullOrEmpty()) return text;
            text = text.Replace("Overseer".Translate().Resolve(), "CVG_Puppet_Master".Translate().Resolve());
            text = text.Replace("OverseerNone".Translate().Resolve(), "CVG_Puppet_MasterNone".Translate().Resolve());
            text = text.Replace("InsufficientBandwidth".Translate().Resolve(), "CVG_Puppet_NoBandwidth".Translate().Resolve());
            text = text.Replace("Uncontrolled".Translate().Resolve(), "CVG_Puppet_Uncontrolled".Translate().Resolve());
            text = text.Replace("MayGoFeral".Translate().Resolve(), "CVG_Puppet_MayGoFeral".Translate().Resolve());
            text = text.Replace("MechEnergy".Translate().Resolve(), "CVG_Puppet_Energy".Translate().Resolve());
            return text;
        }
    }

    [HarmonyPatch(typeof(CompOverseerSubject), nameof(CompOverseerSubject.CompInspectStringExtra))]
    public static class Patch_CompOverseerSubject_InspectString
    {
        public static void Postfix(CompOverseerSubject __instance, ref string __result)
        {
            if (__result != null && PuppetInspectTexts.IsPuppet(__instance.parent as Pawn)) __result = PuppetInspectTexts.Wuxia(__result);
        }
    }

    [HarmonyPatch(typeof(Pawn), nameof(Pawn.GetInspectString))]
    public static class Patch_Pawn_InspectString_Puppet
    {
        public static void Postfix(Pawn __instance, ref string __result)
        {
            if (__result != null && PuppetInspectTexts.IsPuppet(__instance)) __result = PuppetInspectTexts.Wuxia(__result);
        }
    }
}
