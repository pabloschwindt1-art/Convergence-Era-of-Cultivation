using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Preajuste de dificultad de cultivo (docs/DISENO.md §8, ronda 3): Fácil / Difícil / Pesadilla en XML
    /// (<c>Defs/CultivationDifficulty.xml</c>); se pueden agregar más.
    /// </summary>
    public class CultivationDifficultyDef : Def
    {
        public int order;
        public CultivationDifficultyValues values = new CultivationDifficultyValues();
    }

    /// <summary>Valores de la dificultad de cultivo. La partida guarda los suyos; los ajustes, los de las partidas nuevas.</summary>
    public class CultivationDifficultyValues : IExposable
    {
        /// <summary>Preajuste del que salen (null = personalizado).</summary>
        public CultivationDifficultyDef preset;
        /// <summary>Multiplicador de la velocidad de cultivo y de templado (StatPart).</summary>
        public float cultivationSpeed = 1f;
        /// <summary>Multiplicador del daño de los rayos de la tribulación.</summary>
        public float tribulationDamage = 1f;
        public bool tribulationCanKill = true;
        /// <summary>Fuerza de la supresión de reino (0 = sin supresión, 1 = la curva del XML).</summary>
        public float suppressionStrength = 1f;
        /// <summary>Los cultivadores enemigos llegan hasta el reino más alto de la colonia + esto.</summary>
        public int enemyRealmBonus = 1;
        public bool scaleEnemyRealms = true;
        /// <summary>Multiplicador de los puntos de amenaza por la riqueza de cultivo.</summary>
        public float cultivationThreatFactor = 1f;
        /// <summary>Multiplicador de la probabilidad de los eventos del mod.</summary>
        public float eventFrequency = 1f;
        /// <summary>Fracción de los objetos de cultivo que cuenta en la riqueza vanilla (puntos de asalto vanilla).</summary>
        public float vanillaWealthFactor = 0.2f;

        public CultivationDifficultyValues Copy()
        {
            var c = (CultivationDifficultyValues)MemberwiseClone();
            return c;
        }

        public void CopyFrom(CultivationDifficultyValues o)
        {
            preset = o.preset;
            cultivationSpeed = o.cultivationSpeed;
            tribulationDamage = o.tribulationDamage;
            tribulationCanKill = o.tribulationCanKill;
            suppressionStrength = o.suppressionStrength;
            enemyRealmBonus = o.enemyRealmBonus;
            scaleEnemyRealms = o.scaleEnemyRealms;
            cultivationThreatFactor = o.cultivationThreatFactor;
            eventFrequency = o.eventFrequency;
            vanillaWealthFactor = o.vanillaWealthFactor;
        }

        public void Apply(CultivationDifficultyDef def)
        {
            CopyFrom(def.values);
            preset = def;
        }

        public string Label => preset?.LabelCap ?? "CVG_Difficulty_Custom".Translate();

        public void ExposeData()
        {
            Scribe_Defs.Look(ref preset, "preset");
            Scribe_Values.Look(ref cultivationSpeed, "cultivationSpeed", 1f);
            Scribe_Values.Look(ref tribulationDamage, "tribulationDamage", 1f);
            Scribe_Values.Look(ref tribulationCanKill, "tribulationCanKill", true);
            Scribe_Values.Look(ref suppressionStrength, "suppressionStrength", 1f);
            Scribe_Values.Look(ref enemyRealmBonus, "enemyRealmBonus", 1);
            Scribe_Values.Look(ref scaleEnemyRealms, "scaleEnemyRealms", true);
            Scribe_Values.Look(ref cultivationThreatFactor, "cultivationThreatFactor", 1f);
            Scribe_Values.Look(ref eventFrequency, "eventFrequency", 1f);
            Scribe_Values.Look(ref vanillaWealthFactor, "vanillaWealthFactor", 0.2f);
        }

        /// <summary>Controles de edición (ajustes del mod y diálogo de inicio de partida).</summary>
        public void DoEditor(Listing_Standard listing)
        {
            var presets = DefDatabase<CultivationDifficultyDef>.AllDefsListForReading.OrderBy(d => d.order).ToList();
            var row = listing.GetRect(30f);
            float w = row.width / Mathf.Max(1, presets.Count);
            for (int i = 0; i < presets.Count; i++)
            {
                var d = presets[i];
                var r = new Rect(row.x + i * w, row.y, w - 4f, row.height);
                bool selected = preset == d;
                if (selected) Widgets.DrawHighlightSelected(r);
                if (Widgets.ButtonText(r, d.LabelCap, drawBackground: !selected)) Apply(d);
                TooltipHandler.TipRegion(r, d.description);
            }
            listing.Label("CVG_Difficulty_Current".Translate(Label));
            listing.Label("CVG_Difficulty_CultivationSpeed".Translate(cultivationSpeed.ToStringPercent()));
            cultivationSpeed = Mathf.Round(listing.Slider(cultivationSpeed, 0.1f, 3f) * 20f) / 20f;
            listing.Label("CVG_Difficulty_TribulationDamage".Translate(tribulationDamage.ToStringPercent()));
            tribulationDamage = Mathf.Round(listing.Slider(tribulationDamage, 0.25f, 3f) * 20f) / 20f;
            listing.CheckboxLabeled("CVG_Setting_TribulationCanKill".Translate(), ref tribulationCanKill);
            listing.Label(suppressionStrength <= 0f
                    ? "CVG_Setting_RealmSuppressionOff".Translate()
                    : "CVG_Setting_RealmSuppression".Translate(suppressionStrength.ToStringPercent()),
                tooltip: "CVG_Setting_RealmSuppressionTip".Translate());
            suppressionStrength = Mathf.Round(listing.Slider(suppressionStrength, 0f, 2f) * 20f) / 20f;
            listing.CheckboxLabeled("CVG_Setting_ScaleEnemyRealms".Translate(), ref scaleEnemyRealms, "CVG_Setting_ScaleEnemyRealmsTip".Translate());
            if (scaleEnemyRealms)
            {
                listing.Label("CVG_Difficulty_EnemyRealmBonus".Translate(enemyRealmBonus.ToString("+0;-0;0")));
                enemyRealmBonus = Mathf.RoundToInt(listing.Slider(enemyRealmBonus, 0f, 3f));
            }
            listing.Label("CVG_Difficulty_CultivationThreat".Translate(cultivationThreatFactor.ToStringPercent()), tooltip: "CVG_Difficulty_CultivationThreatTip".Translate());
            cultivationThreatFactor = Mathf.Round(listing.Slider(cultivationThreatFactor, 0f, 3f) * 20f) / 20f;
            listing.Label("CVG_Difficulty_EventFrequency".Translate(eventFrequency.ToStringPercent()));
            eventFrequency = Mathf.Round(listing.Slider(eventFrequency, 0f, 3f) * 20f) / 20f;
            listing.Label("CVG_Difficulty_VanillaWealth".Translate(vanillaWealthFactor.ToStringPercent()), tooltip: "CVG_Difficulty_VanillaWealthTip".Translate());
            vanillaWealthFactor = Mathf.Round(listing.Slider(vanillaWealthFactor, 0f, 1f) * 20f) / 20f;
            if (preset != null && !SameValues(this, preset.values)) preset = null;
        }

        public static bool SameValues(CultivationDifficultyValues a, CultivationDifficultyValues b) =>
            Mathf.Approximately(a.cultivationSpeed, b.cultivationSpeed) && Mathf.Approximately(a.tribulationDamage, b.tribulationDamage)
            && a.tribulationCanKill == b.tribulationCanKill && Mathf.Approximately(a.suppressionStrength, b.suppressionStrength)
            && a.enemyRealmBonus == b.enemyRealmBonus && a.scaleEnemyRealms == b.scaleEnemyRealms
            && Mathf.Approximately(a.cultivationThreatFactor, b.cultivationThreatFactor) && Mathf.Approximately(a.eventFrequency, b.eventFrequency)
            && Mathf.Approximately(a.vanillaWealthFactor, b.vanillaWealthFactor);
    }

    public static class CultivationDifficulty
    {
        /// <summary>Los valores de la partida en curso (o los de los ajustes, fuera de partida).</summary>
        public static CultivationDifficultyValues Current => GameComponent_Convergence.Instance?.Difficulty ?? ConvergenceMod.Settings.difficulty;

        /// <summary>El def pertenece a un mod de Convergence (para la frecuencia de eventos).</summary>
        public static bool IsConvergenceDef(Def def)
        {
            string package = def?.modContentPack?.PackageId;
            var main = CultivationWealthDef.Main;
            return package != null && main != null && main.packageIdPrefixes.Any(p => package.StartsWith(p, System.StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>Velocidad de cultivo de la dificultad (en CVG_CultivationSpeed y CVG_BodyTemperingSpeed).</summary>
    public class StatPart_CultivationDifficulty : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (Current.ProgramState == ProgramState.Playing) val *= CultivationDifficulty.Current.cultivationSpeed;
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (Current.ProgramState != ProgramState.Playing) return null;
            var d = CultivationDifficulty.Current;
            if (Mathf.Approximately(d.cultivationSpeed, 1f)) return null;
            return "CVG_Difficulty_StatPart".Translate(d.Label) + ": x" + d.cultivationSpeed.ToStringPercent();
        }
    }

    /// <summary>Frecuencia de los eventos del mod (probabilidad base de los IncidentDef de Convergence).</summary>
    [HarmonyPatch(typeof(IncidentWorker), nameof(IncidentWorker.BaseChanceThisGame), MethodType.Getter)]
    public static class Patch_IncidentWorker_BaseChanceThisGame
    {
        public static void Postfix(IncidentWorker __instance, ref float __result)
        {
            if (__result <= 0f || __instance.def == null || !CultivationDifficulty.IsConvergenceDef(__instance.def)) return;
            __result *= CultivationDifficulty.Current.eventFrequency;
        }
    }

    /// <summary>
    /// Elección al empezar una partida nueva: dificultad de cultivo y cómo llega la Convergencia (inmediata o vanilla
    /// primero). Parte de los ajustes del mod.
    /// </summary>
    public class Dialog_CultivationStart : Window
    {
        private readonly CultivationDifficultyValues values;
        private ConvergenceTrigger trigger;
        private int minDay, maxDay;
        private Vector2 scroll;

        public override Vector2 InitialSize => new Vector2(620f, 720f);

        public Dialog_CultivationStart()
        {
            var s = ConvergenceMod.Settings;
            values = s.difficulty.Copy();
            trigger = s.convergenceTrigger;
            minDay = s.convergenceMinDay;
            maxDay = s.convergenceMaxDay;
            forcePause = true;
            absorbInputAroundWindow = true;
            doCloseX = false;
            closeOnCancel = false;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 36f), "CVG_StartDialog_Title".Translate());
            Text.Font = GameFont.Small;
            var outRect = new Rect(inRect.x, inRect.y + 40f, inRect.width, inRect.height - 90f);
            var viewRect = new Rect(0f, 0f, outRect.width - 20f, 900f);
            Widgets.BeginScrollView(outRect, ref scroll, viewRect);
            var listing = new Listing_Standard();
            listing.Begin(viewRect);
            listing.Label("CVG_StartDialog_Intro".Translate());
            listing.GapLine();
            listing.Label("CVG_Setting_Trigger".Translate());
            StartModeEditor(listing, ref trigger, ref minDay, ref maxDay);
            listing.GapLine();
            listing.Label("CVG_Difficulty_Title".Translate());
            values.DoEditor(listing);
            listing.End();
            Widgets.EndScrollView();
            if (Widgets.ButtonText(new Rect(inRect.xMax - 200f, inRect.yMax - 40f, 200f, 38f), "OK".Translate()))
            {
                GameComponent_Convergence.Instance?.SetStart(values, trigger, minDay, maxDay);
                Close();
            }
        }

        /// <summary>Modo de inicio: Convergencia inmediata, vanilla primero (narrador, días) o por investigación.</summary>
        public static void StartModeEditor(Listing_Standard listing, ref ConvergenceTrigger trigger, ref int minDay, ref int maxDay)
        {
            if (listing.RadioButton("CVG_Trigger_Immediate".Translate(), trigger == ConvergenceTrigger.Immediate, tooltip: "CVG_Trigger_ImmediateTip".Translate()))
                trigger = ConvergenceTrigger.Immediate;
            if (listing.RadioButton("CVG_Trigger_Storyteller".Translate(), trigger == ConvergenceTrigger.Storyteller, tooltip: "CVG_Trigger_StorytellerTip".Translate()))
                trigger = ConvergenceTrigger.Storyteller;
            if (listing.RadioButton("CVG_Trigger_Research".Translate(), trigger == ConvergenceTrigger.Research))
                trigger = ConvergenceTrigger.Research;
            if (trigger == ConvergenceTrigger.Storyteller)
            {
                listing.Label("CVG_Setting_MinDay".Translate(minDay));
                minDay = (int)listing.Slider(minDay, 1, 120);
                listing.Label("CVG_Setting_MaxDay".Translate(maxDay));
                maxDay = Mathf.Max(minDay, (int)listing.Slider(maxDay, 1, 180));
            }
        }
    }
}
