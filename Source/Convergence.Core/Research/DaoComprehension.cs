using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Marca un proyecto de la pestaña Cultivo como "Comprensión del Dao" (docs/modulos/INVESTIGACION.md).
    /// Con Anomaly se convierte al arrancar en un proyecto de conocimiento (<see cref="knowledgeCost"/>, categoría
    /// <c>CVG_DaoComprehension</c>) que avanza cultivando, entrenando y estudiando; sin Anomaly queda como investigación
    /// normal (<c>baseCost</c>) en el escritorio de contemplación, y cultivar también le suma.
    /// </summary>
    public class DefModExtension_DaoResearch : DefModExtension
    {
        public float knowledgeCost = 5f;
        /// <summary>Descubrimiento especial (carta propia, p. ej. los reinos secretos).</summary>
        [MustTranslate] public string discoveryText;
    }

    /// <summary>Requisito de avance: haber completado un proyecto de investigación (p. ej. "Fundación").</summary>
    public class BreakthroughRequirement_Research : BreakthroughRequirement
    {
        public ResearchProjectDef project;

        public override bool Met(Pawn pawn, RealmTracker tracker, out string reason)
        {
            // Solo el jugador investiga: los PNJ no tienen esta barrera.
            bool ok = project == null || project.IsFinished || !(pawn.Faction?.IsPlayer ?? false);
            reason = ok ? null : "CVG_Breakthrough_RequiresResearch".Translate(project.LabelCap).Resolve();
            return ok;
        }
    }

    /// <summary>Fuente de comprensión (para la carta: "se iluminó mientras cultivaba").</summary>
    public enum DaoSource
    {
        Cultivating,
        Tempering,
        Studying,
        Absorbing,
        Breathing
    }

    [StaticConstructorOnStartup]
    public static class DaoComprehension
    {
        private static KnowledgeCategoryDef category;

        public static KnowledgeCategoryDef Category => category ??= DefDatabase<KnowledgeCategoryDef>.GetNamedSilentFail("CVG_DaoComprehension");

        public static bool UsesKnowledge => Category != null;

        static DaoComprehension()
        {
            // Con Anomaly, los proyectos del Dao pasan a ser de conocimiento (no necesitan mesa).
            if (!UsesKnowledge) return;
            int count = 0;
            foreach (var proj in DefDatabase<ResearchProjectDef>.AllDefsListForReading)
            {
                var ext = proj.GetModExtension<DefModExtension_DaoResearch>();
                if (ext == null) continue;
                proj.knowledgeCategory = Category;
                proj.knowledgeCost = ext.knowledgeCost;
                proj.baseCost = 0f;
                proj.requiredResearchBuilding = null;
                count++;
            }
            if (count > 0) Log.Message($"[Convergence] {count} Dao research project(s) use the Dao Comprehension knowledge category (Anomaly).");
        }

        public static bool IsDaoProject(ResearchProjectDef proj) => proj != null && proj.HasModExtension<DefModExtension_DaoResearch>();

        /// <summary>
        /// Suma comprensión: conocimiento (Anomaly) al proyecto del Dao en curso; si no hay, elige el primero disponible.
        /// Solo colonos del jugador.
        /// </summary>
        public static void Gain(Pawn pawn, float amount, DaoSource source)
        {
            if (amount <= 0f || pawn == null || !(pawn.Faction?.IsPlayer ?? false) || !GameComponent_Convergence.Active) return;
            amount *= CelestialCalendar.DaoFactor(pawn);
            if (!UsesKnowledge) return;
            var rm = Find.ResearchManager;
            var proj = rm.GetProject(Category);
            if (proj == null)
            {
                proj = DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where(p => p.knowledgeCategory == Category && p.CanStartNow)
                    .OrderBy(p => p.researchViewX).FirstOrDefault();
                if (proj == null) return;
                rm.SetCurrentProject(proj);
            }
            GameComponent_Dao.Instance?.Record(proj, pawn, amount, source);
            rm.ApplyKnowledge(Category, amount);
        }

        /// <summary>Comprensión al cultivar/templar durante <paramref name="ticks"/>. La iluminación súbita es la inspiración
        /// vanilla (<see cref="DefModExtension_InspirationDao"/>), no una tirada propia.</summary>
        public static void GainWhilePracticing(Pawn pawn, int ticks, float perHour, DaoSource source)
        {
            Gain(pawn, perHour * ticks / GenDate.TicksPerHour, source);
        }

        public static string SourceLabel(DaoSource source) => ("CVG_Dao_Source_" + source).Translate();
    }

    /// <summary>Quién aporta a cada proyecto del Dao (para la carta) y qué cartas ya se enviaron.</summary>
    public class GameComponent_Dao : GameComponent
    {
        private List<DaoContribution> contributions = new List<DaoContribution>();
        private List<ResearchProjectDef> announced = new List<ResearchProjectDef>();

        public static GameComponent_Dao Instance => Current.Game?.GetComponent<GameComponent_Dao>();

        private const int CheckInterval = 250;
        private static List<ResearchProjectDef> daoProjects;

        private static List<ResearchProjectDef> DaoProjects =>
            daoProjects ??= DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where(DaoComprehension.IsDaoProject).ToList();

        public GameComponent_Dao(Game game)
        {
        }

        // Los proyectos ya completos al empezar o cargar no mandan carta (p. ej. completados por debug o el Maestro).
        public override void StartedNewGame() => MarkFinishedAsAnnounced();

        public override void LoadedGame() => MarkFinishedAsAnnounced();

        private void MarkFinishedAsAnnounced()
        {
            foreach (var p in DaoProjects)
                if (p.IsFinished && !announced.Contains(p)) announced.Add(p);
        }

        /// <summary>Carta de iluminación cuando se completa un proyecto del Dao (comprobación barata cada 250 ticks).</summary>
        public override void GameComponentTick()
        {
            if (Find.TickManager.TicksGame % CheckInterval != 0) return;
            var list = DaoProjects;
            for (int i = 0; i < list.Count; i++)
                if (list[i].IsFinished && !announced.Contains(list[i])) Notify_Finished(list[i]);
        }

        /// <summary>Proyecto completado sin carta de iluminación (lo enseñó el Maestro, debug…).</summary>
        public void MarkAnnounced(ResearchProjectDef proj)
        {
            if (proj != null && !announced.Contains(proj)) announced.Add(proj);
        }

        public void Record(ResearchProjectDef proj, Pawn pawn, float amount, DaoSource source)
        {
            var c = contributions.FirstOrDefault(x => x.project == proj && x.pawn == pawn);
            if (c == null) contributions.Add(c = new DaoContribution { project = proj, pawn = pawn });
            c.amount += amount;
            c.source = source;
        }

        public void Notify_Finished(ResearchProjectDef proj)
        {
            if (proj == null || announced.Contains(proj) || Current.ProgramState != ProgramState.Playing) return;
            announced.Add(proj);
            var top = contributions.Where(c => c.project == proj && c.pawn != null && !c.pawn.Dead).OrderByDescending(c => c.amount).FirstOrDefault();
            var ext = proj.GetModExtension<DefModExtension_DaoResearch>();
            TaggedString text;
            if (top != null) TraitProgression.Add(top.pawn, "Enlightenment");
            if (top != null)
                text = "CVG_Dao_EnlightenmentText".Translate(top.pawn.Named("PAWN"), DaoComprehension.SourceLabel(top.source), proj.LabelCap);
            else
                text = "CVG_Dao_EnlightenmentTextNoOne".Translate(proj.LabelCap);
            if (ext != null && !ext.discoveryText.NullOrEmpty())
                text += "\n\n" + (top != null ? ext.discoveryText.Formatted(top.pawn.Named("PAWN")).Resolve() : ext.discoveryText);
            Find.LetterStack.ReceiveLetter("CVG_Dao_EnlightenmentLabel".Translate(proj.LabelCap), text, LetterDefOf.PositiveEvent,
                top?.pawn != null && top.pawn.Spawned ? new LookTargets(top.pawn) : LookTargets.Invalid);
            contributions.RemoveAll(c => c.project == proj);
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref contributions, "contributions", LookMode.Deep);
            Scribe_Collections.Look(ref announced, "announced", LookMode.Def);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                contributions ??= new List<DaoContribution>();
                announced ??= new List<ResearchProjectDef>();
                contributions.RemoveAll(c => c.project == null);
                announced.RemoveAll(p => p == null);
            }
        }
    }

    public class DaoContribution : IExposable
    {
        public ResearchProjectDef project;
        public Pawn pawn;
        public float amount;
        public DaoSource source;

        public void ExposeData()
        {
            Scribe_Defs.Look(ref project, "project");
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_Values.Look(ref amount, "amount");
            Scribe_Values.Look(ref source, "source");
        }
    }
}

namespace Convergence
{
    /// <summary>
    /// Con Anomaly, el juego oculta los proyectos de conocimiento hasta activar el monolito. Los del Dao no dependen de él:
    /// se ven (y avanzan) desde el principio.
    /// </summary>
    [HarmonyLib.HarmonyPatch(typeof(RimWorld.EntityCodex), nameof(RimWorld.EntityCodex.Hidden), typeof(Verse.ResearchProjectDef))]
    public static class Patch_EntityCodex_Hidden
    {
        public static void Postfix(Verse.ResearchProjectDef def, ref bool __result)
        {
            if (__result && DaoComprehension.IsDaoProject(def)) __result = false;
        }
    }

    /// <summary>Inspiración que da comprensión del Dao al empezar (la iluminación súbita: "dunwu"). Reemplaza a la tirada
    /// propia que había al cultivar: ahora llega por el camino vanilla de las inspiraciones (buen ánimo, crisis superadas).</summary>
    public class DefModExtension_InspirationDao : DefModExtension
    {
        public float daoComprehension;
        /// <summary>Contador de rasgos que suben con el uso (<see cref="TraitProgression"/>), p. ej. "Enlightenment".</summary>
        public string traitCounter;
    }

    [HarmonyLib.HarmonyPatch(typeof(InspirationHandler), nameof(InspirationHandler.TryStartInspiration))]
    public static class Patch_InspirationHandler_Dao
    {
        public static void Postfix(bool __result, InspirationDef def, InspirationHandler __instance)
        {
            var ext = __result ? def?.GetModExtension<DefModExtension_InspirationDao>() : null;
            var pawn = __instance.pawn;
            if (ext == null || pawn == null || !GameComponent_Convergence.Active) return;
            if (ext.daoComprehension > 0f) DaoComprehension.Gain(pawn, ext.daoComprehension, DaoSource.Cultivating);
            if (!ext.traitCounter.NullOrEmpty()) TraitProgression.Add(pawn, ext.traitCounter);
            if (pawn.Spawned) FleckMaker.ThrowLightningGlow(pawn.DrawPos, pawn.Map, 2f);
        }
    }
}
