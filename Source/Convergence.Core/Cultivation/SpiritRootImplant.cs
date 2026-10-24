using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Implante que hace de raíz espiritual (la raíz artificial de Qi-Tech). El grado sale de su DefModExtension_SpiritRoot; el
    /// elemento se sortea al instalarlo y se guarda. La raíz se recalcula al ponerlo y al quitarlo.
    /// </summary>
    public class Hediff_SpiritRootImplant : Hediff_Implant
    {
        public QiElementDef element;

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            if (element == null && def.GetModExtension<DefModExtension_SpiritRoot>()?.element == null)
                element = CultivationGenerator.RandomElements()[0];
            CompCultivator.For(pawn)?.Notify_RootSourcesChanged();
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            CompCultivator.For(pawn)?.Notify_RootSourcesChanged();
        }

        public override string LabelInBrackets => element?.label ?? base.LabelInBrackets;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref element, "element");
        }
    }

    /// <summary>Instalar una raíz artificial: solo en quien no tiene raíz.</summary>
    public class Recipe_InstallSpiritRootImplant : Recipe_InstallImplant
    {
        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null) =>
            base.AvailableOnNow(thing, part) && thing is Pawn p && !(CompCultivator.For(p)?.HasRoot ?? false);
    }
}
