using HarmonyLib;
using RimWorld;
using Verse;

namespace Convergence.Grafts
{
    /// <summary>Los injertos solo se pueden instalar si la colonia tiene un Altar de Injerto del Dao en el mapa.</summary>
    public static class GraftUtility
    {
        public static bool HasAltar(Thing patient)
        {
            var map = patient?.MapHeld;
            var altar = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_GraftingAltar");
            return map != null && altar != null && map.listerBuildings.ColonistsHaveBuilding(altar);
        }
    }

    /// <summary>Instalación de un injerto que sustituye una parte (brazo, mano, ojo, pierna).</summary>
    public class Recipe_InstallGraft : Recipe_InstallArtificialBodyPart
    {
        // La operación se ve siempre; sin altar aparece deshabilitada con el motivo.
        public override AcceptanceReport AvailableReport(Thing thing, BodyPartRecord part = null) =>
            GraftUtility.HasAltar(thing) ? base.AvailableReport(thing, part) : (AcceptanceReport)"CVG_NeedsGraftingAltar".Translate();
    }

    /// <summary>Instalación de un injerto que se añade a la parte natural (huesos, venas).</summary>
    public class Recipe_InstallGraftImplant : Recipe_InstallImplant
    {
        // La operación se ve siempre; sin altar aparece deshabilitada con el motivo.
        public override AcceptanceReport AvailableReport(Thing thing, BodyPartRecord part = null) =>
            GraftUtility.HasAltar(thing) ? base.AvailableReport(thing, part) : (AcceptanceReport)"CVG_NeedsGraftingAltar".Translate();
    }

    [StaticConstructorOnStartup]
    public static class GraftsStartup
    {
        static GraftsStartup()
        {
            Log.Message("[Convergence] Grafts loaded.");
        }
    }
}
