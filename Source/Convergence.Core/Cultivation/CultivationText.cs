using Verse;

namespace Convergence
{
    /// <summary>Textos cortos de cultivo reutilizados en cartas y en la pestaña.</summary>
    public static class CultivationText
    {
        public static string RootLabel(SpiritRoot root) =>
            root == null
                ? "CVG_NoRoot".Translate().Resolve()
                : "CVG_RootLabel".Translate(root.grade.LabelCap, root.ElementsLabel).Resolve();

        public static string Summary(Pawn pawn, CompCultivator comp)
        {
            string s = pawn.LabelShortCap + ": " + RootLabel(comp.Root);
            if (comp.SpecialBody != null) s += ", " + comp.SpecialBody.LabelCap;
            var bloodline = BloodlineUtility.Get(pawn);
            if (bloodline != null) s += ", " + BloodlineUtility.Label(bloodline);
            return s;
        }
    }
}
