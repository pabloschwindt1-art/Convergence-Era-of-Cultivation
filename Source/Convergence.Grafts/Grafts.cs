using System.Linq;
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

    /// <summary>
    /// Quien espera un injerto se acuesta en el altar (plan de integración, 2026-10-23): el altar es una cama médica y la
    /// operación es un ritual en su círculo, no en cualquier camilla.
    /// </summary>
    [HarmonyPatch(typeof(RestUtility), nameof(RestUtility.FindPatientBedFor))]
    public static class Patch_RestUtility_FindPatientBedFor
    {
        public static bool Prefix(Pawn pawn, ref Building_Bed __result)
        {
            var bills = pawn?.health?.surgeryBills;
            if (bills == null || pawn.Map == null || !bills.Bills.Any(b => b is Bill_Medical m
                    && (m.recipe.Worker is Recipe_InstallGraft || m.recipe.Worker is Recipe_InstallGraftImplant)))
                return true;
            var altarDef = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_GraftingAltar");
            if (altarDef == null) return true;
            if (pawn.CurrentBed() is Building_Bed cur && cur.def == altarDef) { __result = cur; return false; }
            Building_Bed best = null;
            float bestDist = float.MaxValue;
            foreach (var b in pawn.Map.listerBuildings.AllBuildingsColonistOfDef(altarDef))
            {
                if (!(b is Building_Bed bed) || !RestUtility.IsValidBedFor(bed, pawn, pawn, checkSocialProperness: false,
                        allowMedBedEvenIfSetToNoCare: true, ignoreOtherReservations: false, pawn.GuestStatus)) continue;
                if (!pawn.CanReach(bed, Verse.AI.PathEndMode.OnCell, Danger.Deadly)) continue;
                float d = bed.Position.DistanceToSquared(pawn.Position);
                if (d < bestDist) { bestDist = d; best = bed; }
            }
            if (best == null) return true;
            __result = best;
            return false;
        }
    }

    [StaticConstructorOnStartup]
    public static class GraftsStartup
    {
        static GraftsStartup()
        {
            new Harmony("prospero1088.convergence.grafts").PatchAll();
            Log.Message("[Convergence] Grafts loaded.");
        }
    }
}
