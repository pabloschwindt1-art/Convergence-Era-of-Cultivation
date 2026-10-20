using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>Cuándo y cómo abrir la ventana de desarrollo (técnicas y conjuntos de combate).</summary>
    [StaticConstructorOnStartup]
    public static class CultivationDevelopmentUtility
    {
        private static readonly Texture2D Icon = ContentFinder<Texture2D>.Get("UI/Techniques/CVG_Tech_QiShield", false) ?? BaseContent.BadTex;

        public static bool CanOpen(Pawn pawn)
        {
            var comp = CompCultivator.For(pawn);
            if (comp == null) return false;
            return comp.IsQiCultivator || comp.IsCultivator || (comp.TrackerFor(CVG_DefOf.CVG_Path_Body)?.Started ?? false)
                || (comp.TechniquesIfAny?.AnyManual ?? false);
        }

        public static Command MakeGizmo(Pawn pawn) => new Command_Action
        {
            defaultLabel = "CVG_Dev_Gizmo".Translate(),
            defaultDesc = "CVG_Dev_ButtonTip".Translate(),
            icon = Icon,
            action = () => Find.WindowStack.Add(new Dialog_CultivationDevelopment(pawn))
        };
    }
}
