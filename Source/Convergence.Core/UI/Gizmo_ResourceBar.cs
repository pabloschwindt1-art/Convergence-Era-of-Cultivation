using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Barra del recurso de un camino (qi, sangre-qi…). Usa el gizmo de barra vanilla (<see cref="Gizmo_Slider"/>,
    /// el mismo que el hemogén y el psyfocus): título corto, barra horizontal y "actual / máximo" encima.
    /// Reino, etapa y regeneración van en el tooltip.
    /// </summary>
    [StaticConstructorOnStartup]
    public class Gizmo_ResourceBar : Gizmo_Slider
    {
        private readonly RealmTracker tracker;

        public Gizmo_ResourceBar(RealmTracker tracker)
        {
            this.tracker = tracker;
        }

        private CultivationResource Resource => tracker.Resource;

        protected override float Width => 160f;
        protected override string Title => Resource.Label.CapitalizeFirst();
        protected override float ValuePercent => Resource.Percent;
        protected override string BarLabel => Resource.Current.ToString("F0") + " / " + Resource.Max.ToString("F0");
        protected override Color BarColor => tracker.Path.resourceColor;
        protected override Color BarHighlightColor => Color.Lerp(tracker.Path.resourceColor, Color.white, 0.25f);
        protected override bool IsDraggable => false;
        protected override float Target { get => 1f; set { } }
        protected override bool DraggingBar { get => false; set { } }

        protected override string GetTooltip()
        {
            var r = Resource;
            string tip = r.Label.CapitalizeFirst().AsTipTitle() + ": " + r.Current.ToString("F0") + " / " + r.Max.ToString("F0");
            if (tracker.Realm != null)
                tip += "\n" + "CVG_RealmStage".Translate(tracker.Realm.LabelCap, tracker.Stage, tracker.Realm.stages);
            tip += "\n" + "CVG_ResourceRegen".Translate(r.RegenPerHour.ToString("0.#"));
            if (!tracker.Path.resourceDescription.NullOrEmpty()) tip += "\n\n" + tracker.Path.resourceDescription;
            return tip;
        }
    }
}
