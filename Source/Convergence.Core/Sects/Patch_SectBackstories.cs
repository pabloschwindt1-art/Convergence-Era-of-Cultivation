using HarmonyLib;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Los miembros de una secta tienen siempre su historia adulta de secta (<c>PawnKindDef.fixedAdultBackstories</c>). Vanilla, con 25% de
    /// probabilidad (y siempre al líder de la facción), les da en cambio una biografía fija del catálogo, con nombre y las dos historias,
    /// e ignora las de la secta. Para los tipos de pawn de Convergence con historias fijas, eso no pasa; la infancia sigue al azar.
    /// </summary>
    [HarmonyPatch(typeof(PawnBioAndNameGenerator), "TryGiveSolidBioTo")]
    public static class Patch_PawnBio_SectBackstories
    {
        public static bool Prefix(Pawn pawn, ref bool __result)
        {
            var kind = pawn?.kindDef;
            if (kind == null || kind.fixedAdultBackstories.NullOrEmpty() || !kind.HasModExtension<DefModExtension_CultivatorPawnKind>()) return true;
            __result = false;
            return false;
        }
    }
}
