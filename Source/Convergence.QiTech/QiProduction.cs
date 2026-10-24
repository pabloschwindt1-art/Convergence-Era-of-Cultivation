using HarmonyLib;
using RimWorld;
using Verse;

namespace Convergence.QiTech
{
    /// <summary>
    /// Acumulador de qi (la batería vanilla para la red de qi): almacén del nodo de qi que con electricidad casi no pierde
    /// (<see cref="poweredLeakPerDay"/>) y sin ella pierde como un cristal (<c>leakPerDay</c>).
    /// </summary>
    public class CompProperties_QiAccumulator : CompProperties_QiNode
    {
        public float poweredLeakPerDay = 0.01f;

        public CompProperties_QiAccumulator() => compClass = typeof(CompQiAccumulator);
    }

    public class CompQiAccumulator : CompQiNode
    {
        private CompPowerTrader power;

        public override float LeakPerDay =>
            (power ??= parent.GetComp<CompPowerTrader>())?.PowerOn ?? false
                ? ((CompProperties_QiAccumulator)props).poweredLeakPerDay
                : Props.leakPerDay;
    }

    /// <summary>
    /// Bandeja hidropónica espiritual: las plantas espirituales (las de los mods de Convergence) crecen
    /// <see cref="spiritGrowthFactor"/> veces más rápido mientras la bandeja tiene piedras espirituales.
    /// </summary>
    public class DefModExtension_SpiritGrower : DefModExtension
    {
        public float spiritGrowthFactor = 1.5f;
        public string sowTag = "CVG_SpiritHydroponic";
    }

    [StaticConstructorOnStartup]
    public static class SpiritHydroponics
    {
        /// <summary>Planta espiritual: una planta sembrable de los mods de Convergence (CVG_ en el defName o en lo que da).</summary>
        public static bool IsSpiritPlant(ThingDef plant) =>
            plant?.plant != null && (plant.defName.StartsWith("CVG_") || (plant.plant.harvestedThingDef?.defName.StartsWith("CVG_") ?? false));

        /// <summary>
        /// La bandeja acepta lo de la hidropónica vanilla y las plantas espirituales sembrables: se les suma su etiqueta de
        /// siembra (como hace vanilla con <c>Hydroponic</c>).
        /// </summary>
        static SpiritHydroponics()
        {
            foreach (var grower in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                var ext = grower.GetModExtension<DefModExtension_SpiritGrower>();
                if (ext == null) continue;
                foreach (var plant in DefDatabase<ThingDef>.AllDefsListForReading)
                {
                    var tags = plant.plant?.sowTags;
                    if (tags == null || tags.Count == 0 || tags.Contains(ext.sowTag)) continue;
                    if (tags.Contains("Hydroponic") || (IsSpiritPlant(plant) && tags.Contains("Ground"))) tags.Add(ext.sowTag);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Plant), nameof(Plant.GrowthRate), MethodType.Getter)]
    public static class Patch_Plant_GrowthRate_SpiritHydroponics
    {
        public static void Postfix(Plant __instance, ref float __result)
        {
            if (__result <= 0f || !__instance.Spawned || !SpiritHydroponics.IsSpiritPlant(__instance.def)) return;
            if (!(__instance.Map.edificeGrid[__instance.Position] is Building_PlantGrower grower)) return;
            var ext = grower.def.GetModExtension<DefModExtension_SpiritGrower>();
            if (ext == null) return;
            if (grower.GetComp<CompRefuelable>()?.HasFuel ?? true) __result *= ext.spiritGrowthFactor;
        }
    }
}
