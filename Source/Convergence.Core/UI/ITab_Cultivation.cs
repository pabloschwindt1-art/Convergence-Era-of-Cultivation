using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>Pestaña "Cultivo" del colono: raíz espiritual, reino por camino y cuerpo especial. Lo que ya muestran otras
    /// pestañas (genes del linaje, misiones del Maestro, riqueza) no se repite acá.</summary>
    [StaticConstructorOnStartup]
    public class ITab_Cultivation : ITab
    {
        private static readonly Texture2D ProgressBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.55f, 0.75f, 0.35f));
        private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.1f, 0.1f, 0.1f));

        private static readonly System.Collections.Generic.Dictionary<Color, Texture2D> barTexCache = new System.Collections.Generic.Dictionary<Color, Texture2D>();

        private static Texture2D BarTex(Color color)
        {
            if (!barTexCache.TryGetValue(color, out var tex)) barTexCache[color] = tex = SolidColorMaterials.NewSolidColorTexture(color);
            return tex;
        }

        public ITab_Cultivation()
        {
            size = new Vector2(460f, 560f);
            labelKey = "CVG_Tab_Cultivation";
        }

        private Vector2 scrollPos;
        private float viewHeight = 1000f;

        private Pawn SelPawnForCultivation => SelPawn ?? (SelThing as Corpse)?.InnerPawn;

        /// <summary>Solo cultivadores (raíz, cultivo empezado o latente, cuerpo especial o linaje): no en cualquier humano.</summary>
        public override bool IsVisible
        {
            get
            {
                var pawn = SelPawnForCultivation;
                var comp = CompCultivator.For(pawn);
                return comp != null && (comp.HasRoot || comp.SpecialBody != null || comp.ActiveTrackers.Any(t => t.Started)
                                        || comp.LatentTrackers.Any() || BloodlineUtility.Get(pawn) != null);
            }
        }

        protected override void FillTab()
        {
            var pawn = SelPawnForCultivation;
            var comp = CompCultivator.For(pawn);
            if (comp == null) return;

            CultivationGuide.Demonstrated(CultivationGuide.Cultivation, KnowledgeAmount.FrameDisplayed);
            // Con desplazamiento: la pestaña entera no entra en pantallas de 900 px y el botón de arriba quedaba cortado.
            var outRect = new Rect(0f, 0f, size.x, size.y).ContractedBy(14f);
            var rect = new Rect(0f, 0f, outRect.width - 16f, viewHeight);
            Widgets.BeginScrollView(outRect, ref scrollPos, rect);
            var listing = new Listing_Standard();
            listing.Begin(rect);

            Text.Font = GameFont.Medium;
            listing.Label(pawn.LabelShortCap);
            Text.Font = GameFont.Small;
            // Desarrollo: caminos de técnicas y conjuntos de combate en una ventana aparte (modelo de VPE). Botón ancho y arriba
            // de todo para que se encuentre; también hay un gizmo "Técnicas" en el colono.
            if (CultivationDevelopmentUtility.CanOpen(pawn))
            {
                var devRect = listing.GetRect(34f);
                if (Widgets.ButtonText(devRect, "CVG_Dev_ButtonWide".Translate()))
                    Find.WindowStack.Add(new Dialog_CultivationDevelopment(pawn));
                TooltipHandler.TipRegion(devRect, "CVG_Dev_ButtonTip".Translate());
            }
            listing.GapLine();

            listing.Label(GameComponent_Convergence.Active
                ? "CVG_Tab_ConvergenceActive".Translate()
                : "CVG_Tab_ConvergenceDormant".Translate());
            listing.Gap();

            // Raíz espiritual
            listing.Label("CVG_Tab_SpiritRoot".Translate() + ": " + CultivationText.RootLabel(comp.Root));
            if (comp.HasRoot)
            {
                GUI.color = Color.gray;
                listing.Label(comp.IsAwakened ? "CVG_Tab_Awakened".Translate() : "CVG_Tab_Latent".Translate());
                GUI.color = Color.white;
            }

            // Linaje de sangre (módulos de razas)
            if (BloodlineUtility.AnyBloodlines)
            {
                var bloodline = BloodlineUtility.Get(pawn);
                var bloodRect = listing.Label("CVG_Tab_Bloodline".Translate() + ": " + BloodlineUtility.Label(bloodline));
                if (bloodline != null)
                    TooltipHandler.TipRegion(bloodRect, bloodline.bloodline.description + "\n\n" + (bloodline.Purity.gene ?? (Def)bloodline.Purity.trait)?.description
                        + "\n\n" + BloodlineText.EffectLines(bloodline.bloodline, bloodline.index).ToLineList("- "));
            }

            // Un bloque por camino: reino, etapa, progreso, fundamento y recurso.
            foreach (var tracker in comp.ActiveTrackers)
            {
                if (!tracker.Started) continue;
                var realm = tracker.Realm;
                listing.GapLine();
                var realmRect = listing.Label(tracker.Path.LabelCap + " · " + "CVG_RealmStage".Translate(realm.LabelCap, tracker.Stage, realm.stages));
                TooltipHandler.TipRegion(realmRect, realm.description);
                if (tracker.PreCultivated)
                {
                    GUI.color = Color.gray;
                    Text.Font = GameFont.Tiny;
                    listing.Label("CVG_Tab_PreCultivated".Translate());
                    Text.Font = GameFont.Small;
                    GUI.color = Color.white;
                }

                listing.Label("CVG_Tab_StageProgress".Translate() + ": " + tracker.Progress.ToString("F0") + " / " + realm.progressPerStage.ToString("F0"));
                Widgets.FillableBar(listing.GetRect(18f), tracker.StagePercent, ProgressBarTex, EmptyBarTex, doBorder: true);
                float eta = tracker.DaysToNextStage;
                if (eta >= 0f)
                {
                    GUI.color = Color.gray;
                    listing.Label("CVG_Tab_StageEta".Translate(eta < 1f ? "<1" : eta.ToString("0")));
                    GUI.color = Color.white;
                }
                var foundationRect = listing.Label("CVG_FoundationQualityLine".Translate(
                    Hediff_CultivationRealm.FoundationQualityLabel(tracker.FoundationQuality), tracker.FoundationQuality.ToStringPercent()));
                TooltipHandler.TipRegion(foundationRect, "CVG_FoundationQualityTip".Translate());
                var bonus = RealmBonusUtility.BonusLines(pawn, tracker);
                if (bonus.Count > 0)
                {
                    listing.Label("CVG_Tab_RealmBonus".Translate() + ":");
                    GUI.color = new Color(0.75f, 0.9f, 1f);
                    Text.Font = GameFont.Tiny;
                    foreach (var line in bonus) listing.Label("   " + line);
                    Text.Font = GameFont.Small;
                    GUI.color = Color.white;
                }
                if (tracker.Bottleneck)
                {
                    GUI.color = new Color(1f, 0.8f, 0.4f);
                    listing.Label("CVG_Tab_Bottleneck".Translate());
                    int cooldown = tracker.BreakthroughCooldownTicksLeft;
                    if (cooldown > 0) listing.Label("CVG_Breakthrough_Cooldown".Translate(cooldown.ToStringTicksToPeriod()));
                    GUI.color = Color.white;
                }

                listing.Gap(6f);
                var res = tracker.Resource;
                listing.Label(res.Label.CapitalizeFirst() + ": " + res.Current.ToString("F0") + " / " + res.Max.ToString("F0")
                    + "  (" + "CVG_QiRegenShort".Translate(res.RegenPerHour.ToString("0.#")) + ")");
                Widgets.FillableBar(listing.GetRect(18f), res.Percent, BarTex(tracker.Path.resourceColor), EmptyBarTex, doBorder: true);
            }

            // Cultivo previo que espera a la Convergencia (o a poder seguir el camino).
            foreach (var latent in comp.LatentTrackers)
            {
                listing.GapLine();
                GUI.color = new Color(0.75f, 0.75f, 0.9f);
                var latentRect = listing.Label("CVG_Tab_LatentCultivation".Translate(latent.Path.LabelCap,
                    "CVG_RealmStage".Translate(latent.Realm.LabelCap, latent.Stage, latent.Realm.stages)));
                TooltipHandler.TipRegion(latentRect, "CVG_Tab_LatentCultivationTip".Translate());
                GUI.color = Color.white;
            }

            // Cuerpo especial
            // Longevidad: cuánto frenan el envejecimiento los reinos.
            float aging = comp.AgingFactor;
            if (aging < 0.999f)
            {
                var agingRect = listing.Label("CVG_Tab_Aging".Translate(aging.ToStringPercent()));
                TooltipHandler.TipRegion(agingRect, "CVG_Tab_AgingTip".Translate());
            }

            if (comp.SpecialBody != null)
            {
                listing.Gap();
                var bodyRect = listing.Label("CVG_Tab_SpecialBody".Translate() + ": " + comp.SpecialBody.LabelCap);
                TooltipHandler.TipRegion(bodyRect, comp.SpecialBody.description);
            }

            viewHeight = Mathf.Max(listing.CurHeight + 12f, outRect.height);
            listing.End();
            Widgets.EndScrollView();
        }
    }
}
