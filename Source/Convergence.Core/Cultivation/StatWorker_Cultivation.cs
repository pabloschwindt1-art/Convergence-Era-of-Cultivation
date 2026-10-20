using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>Stat de cultivo: solo se muestra en cultivadores despiertos (de cualquier camino).</summary>
    public class StatWorker_Cultivation : StatWorker
    {
        public override bool ShouldShowFor(StatRequest req)
        {
            if (!base.ShouldShowFor(req)) return false;
            return req.Thing is Pawn pawn && (CompCultivator.For(pawn)?.IsCultivator ?? false);
        }
    }
}
