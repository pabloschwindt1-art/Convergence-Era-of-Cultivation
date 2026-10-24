using System.Linq;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace Convergence
{
    public class ConvergenceMod : Mod
    {
        public const string HarmonyId = "prospero1088.convergence.core";

        public static ConvergenceSettings Settings { get; private set; }

        public ConvergenceMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<ConvergenceSettings>();
            new Harmony(HarmonyId).PatchAll();
            Log.Message("[Convergence] Core loaded.");
            // Instancia de prueba (-cvg...test): no hace falta dibujar rápido, solo leer el log; así no le roba GPU al usuario.
            if (System.Environment.GetCommandLineArgs().Any(a => a.StartsWith("-cvg") && a.EndsWith("test") && !System.Environment.GetCommandLineArgs().Contains("-cvgshowcase")))
            {
                UnityEngine.QualitySettings.vSyncCount = 0;
                UnityEngine.Application.targetFrameRate = 20;
            }
        }

        public override string SettingsCategory() => "CVG_ModName".Translate();

        private Vector2 settingsScroll;
        private float settingsHeight = 1600f;

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var s = Settings;
            var viewRect = new Rect(0f, 0f, inRect.width - 20f, settingsHeight);
            Widgets.BeginScrollView(inRect, ref settingsScroll, viewRect);
            var listing = new Listing_Standard();
            listing.Begin(viewRect);

            listing.Label("CVG_Setting_WorldMode".Translate());
            if (listing.RadioButton("CVG_WorldMode_TechCultivation".Translate(), s.worldMode == WorldMode.TechCultivation))
                s.worldMode = WorldMode.TechCultivation;
            if (listing.RadioButton("CVG_WorldMode_Classic".Translate(), s.worldMode == WorldMode.Classic))
                s.worldMode = WorldMode.Classic;
            listing.GapLine();

            listing.Label("CVG_Setting_Trigger".Translate());
            listing.Label("CVG_Setting_ForcedDay".Translate(s.convergenceForcedDay), tooltip: "CVG_Setting_ForcedDayTip".Translate());
            s.convergenceForcedDay = Mathf.RoundToInt(listing.Slider(s.convergenceForcedDay, 5f, 240f));
            listing.GapLine();

            listing.Label("CVG_Difficulty_NewGames".Translate());
            s.difficulty.DoEditor(listing);
            var game = Current.ProgramState == ProgramState.Playing ? GameComponent_Convergence.Instance : null;
            if (game != null)
            {
                listing.GapLine();
                listing.Label("CVG_Difficulty_ThisGame".Translate());
                game.Difficulty.DoEditor(listing);
                CultivationWealth.ClearCache();
            }
            listing.GapLine();

            listing.Label("CVG_Setting_RootChance".Translate(s.spiritRootChance.ToStringPercent()));
            s.spiritRootChance = listing.Slider(s.spiritRootChance, 0f, 1f);
            listing.Label("CVG_Setting_Dissonance".Translate(s.celestialDissonanceFactor.ToStringPercent()));
            s.celestialDissonanceFactor = listing.Slider(s.celestialDissonanceFactor, 0f, 3f);
            listing.Label("CVG_Setting_SpiritBeastChance".Translate(s.spiritBeastChanceFactor.ToStringPercent()));
            s.spiritBeastChanceFactor = Mathf.Round(listing.Slider(s.spiritBeastChanceFactor, 0f, 3f) * 20f) / 20f;
            listing.Label("CVG_Setting_LegendaryBackstories".Translate(s.legendaryBackstoryChance.ToStringPercent()), tooltip: "CVG_Setting_LegendaryBackstoriesTip".Translate());
            float legendary = Mathf.Round(listing.Slider(s.legendaryBackstoryChance, 0f, 3f) * 20f) / 20f;
            if (!Mathf.Approximately(legendary, s.legendaryBackstoryChance)) { s.legendaryBackstoryChance = legendary; LegendaryBackstories.Apply(); }
            listing.Label("CVG_Setting_ImplantDissonance".Translate(s.implantDissonanceFactor.ToStringPercent()), tooltip: "CVG_Setting_ImplantDissonanceTip".Translate());
            s.implantDissonanceFactor = Mathf.Round(listing.Slider(s.implantDissonanceFactor, 0f, 3f) * 20f) / 20f;
            listing.Label("CVG_Setting_MaxGuestGroups".Translate(s.maxGuestGroups == 0 ? "CVG_Setting_NoLimit".Translate().ToString() : s.maxGuestGroups.ToString()),
                tooltip: "CVG_Setting_MaxGuestGroupsTip".Translate());
            s.maxGuestGroups = Mathf.RoundToInt(listing.Slider(s.maxGuestGroups, 0f, 5f));
            listing.CheckboxLabeled("CVG_Setting_PreCultivation".Translate(), ref s.preCultivationEnabled, "CVG_Setting_PreCultivationTip".Translate());
            if (s.preCultivationEnabled)
            {
                listing.Label("CVG_Setting_PreCultivationChance".Translate(s.preCultivationChanceFactor.ToStringPercent()));
                s.preCultivationChanceFactor = Mathf.Round(listing.Slider(s.preCultivationChanceFactor, 0f, 5f) * 20f) / 20f;
            }
            if (BloodlineUtility.AnyBloodlines)
            {
                listing.Label("CVG_Setting_BloodlineChance".Translate(s.bloodlineChanceFactor.ToStringPercent()));
                s.bloodlineChanceFactor = listing.Slider(s.bloodlineChanceFactor, 0f, 10f);
            }
            if (DefDatabase<ThingDef>.AllDefsListForReading.Any(d => d.HasModExtension<DefModExtension_WuxiaRock>()))
                listing.CheckboxLabeled("CVG_Setting_WuxiaRocks".Translate(), ref s.wuxiaRocksOnly, "CVG_Setting_WuxiaRocksTip".Translate());

            settingsHeight = listing.CurHeight + 40f;
            listing.End();
            Widgets.EndScrollView();
        }
    }
}
