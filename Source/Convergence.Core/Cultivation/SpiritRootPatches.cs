using HarmonyLib;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// La raíz se calcula una vez y se guarda (CompCultivator.Sync). Estos parches la invalidan cuando el colono gana o pierde
    /// un rasgo o gen de raíz o de cuerpo especial: xenogerm, ensamblador, modo desarrollador, sangre ancestral…
    /// </summary>
    public static class SpiritRootPatchUtility
    {
        public static bool Affects(Def def) =>
            def == CVG_DefOf.CVG_SpiritRoot || def.HasModExtension<DefModExtension_SpiritRoot>() || def.HasModExtension<DefModExtension_SpecialBody>();

        public static void Notify(Pawn pawn, Def def)
        {
            if (pawn == null || def == null || !Affects(def)) return;
            pawn.GetComp<CompCultivator>()?.Notify_RootSourcesChanged();
        }
    }

    [HarmonyPatch(typeof(TraitSet), nameof(TraitSet.GainTrait))]
    public static class Patch_TraitSet_GainTrait
    {
        public static void Postfix(Trait trait, Pawn ___pawn) => SpiritRootPatchUtility.Notify(___pawn, trait?.def);
    }

    [HarmonyPatch(typeof(TraitSet), nameof(TraitSet.RemoveTrait))]
    public static class Patch_TraitSet_RemoveTrait
    {
        public static void Postfix(Trait trait, Pawn ___pawn) => SpiritRootPatchUtility.Notify(___pawn, trait?.def);
    }

    // Lo llaman AddGene, RemoveGene y el cambio de xenotipo; cubre también los genes que quedan anulados por un xenogen.
    [HarmonyPatch(typeof(Pawn_GeneTracker), "Notify_GenesChanged")]
    public static class Patch_GeneTracker_GenesChanged
    {
        public static void Postfix(GeneDef addedOrRemovedGene, Pawn_GeneTracker __instance) =>
            SpiritRootPatchUtility.Notify(__instance.pawn, addedOrRemovedGene);
    }
}
