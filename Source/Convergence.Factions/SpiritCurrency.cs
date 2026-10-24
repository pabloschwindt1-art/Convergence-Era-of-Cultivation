using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Convergence.Factions
{
    /// <summary>
    /// En un <c>FactionDef</c>: sus mercaderes no aceptan plata, solo piedras espirituales (plan de integración, 2026-10-23:
    /// "entre sectas y facciones de cultivo solo se negocia con piedras espirituales; una segunda economía").
    /// Los precios se siguen calculando en plata, como en vanilla; al pagar, el total se convierte a piedras según su valor.
    /// La plata pasa a ser una mercancía más, que estos mercaderes no compran.
    /// </summary>
    public class DefModExtension_SpiritStoneCurrency : DefModExtension
    {
        public ThingDef currency;
    }

    public static class SpiritCurrency
    {
        /// <summary>La moneda del trato en curso: la de la facción del mercader, o la plata.</summary>
        public static ThingDef Current
        {
            get
            {
                if (!TradeSession.Active || TradeSession.TradeCurrency != TradeCurrency.Silver) return ThingDefOf.Silver;
                var ext = TradeSession.trader?.Faction?.def?.GetModExtension<DefModExtension_SpiritStoneCurrency>();
                return ext?.currency ?? ThingDefOf.Silver;
            }
        }

        public static bool Active => Current != ThingDefOf.Silver;
    }

    [HarmonyPatch(typeof(Tradeable), nameof(Tradeable.IsCurrency), MethodType.Getter)]
    public static class Patch_Tradeable_IsCurrency
    {
        public static void Postfix(Tradeable __instance, ref bool __result)
        {
            if (!SpiritCurrency.Active || __instance.IsFavor) return;
            var def = __instance.ThingDef;
            if (def != null) __result = def == SpiritCurrency.Current;
        }
    }

    [HarmonyPatch(typeof(TradeDeal), nameof(TradeDeal.CurrencyTradeable), MethodType.Getter)]
    public static class Patch_TradeDeal_CurrencyTradeable
    {
        public static void Postfix(TradeDeal __instance, ref Tradeable __result)
        {
            if (!SpiritCurrency.Active) return;
            var cur = SpiritCurrency.Current;
            __result = __instance.AllTradeables.FirstOrDefault(t => t.ThingDef == cur);
        }
    }

    /// <summary>Si nadie tiene la moneda, vanilla agrega una fila de plata vacía; acá, de la moneda del trato.</summary>
    [HarmonyPatch(typeof(TradeDeal), "AddAllTradeables")]
    public static class Patch_TradeDeal_AddAllTradeables
    {
        private static readonly System.Reflection.FieldInfo Silver = AccessTools.Field(typeof(ThingDefOf), nameof(ThingDefOf.Silver));

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var ins in instructions)
            {
                if (ins.opcode == OpCodes.Ldsfld && Equals(ins.operand, Silver))
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(SpiritCurrency), nameof(SpiritCurrency.Current))).MoveLabelsFrom(ins);
                else
                    yield return ins;
            }
        }
    }

    /// <summary>El total del trato (en valor de plata) pasa a cantidad de piedras.</summary>
    [HarmonyPatch(typeof(Tradeable), nameof(Tradeable.CostToInt))]
    public static class Patch_Tradeable_CostToInt
    {
        public static void Postfix(Tradeable __instance, float cost, ref int __result)
        {
            if (!SpiritCurrency.Active || __instance.ThingDef != SpiritCurrency.Current) return;
            float value = UnityEngine.Mathf.Max(0.01f, SpiritCurrency.Current.BaseMarketValue);
            __result = UnityEngine.Mathf.RoundToInt(cost / value);
        }
    }
}
