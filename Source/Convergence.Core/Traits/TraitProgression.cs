using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Rasgo que sube de grado con el uso (docs/modulos/RASGOS.md §9.2): <see cref="counter"/> es el nombre de un contador
    /// por colono (abierto: "Alchemy", "Forging", "Talismans", "SpiritFarming", "BeastTaming", "Formations",
    /// "BodyTempering", "Enlightenment", "TechniquesCast"…) y <see cref="steps"/> los grados a los que llega al cruzar
    /// cada umbral (solo sube, nunca baja). Al subir: carta "X ahora es …".
    /// </summary>
    public class DefModExtension_TraitProgression : DefModExtension
    {
        [NoTranslate] public string counter;
        public List<TraitProgressionStep> steps = new List<TraitProgressionStep>();
    }

    public class TraitProgressionStep
    {
        public int degree;
        public float threshold;
    }

    /// <summary>
    /// Fuente de un contador de progresión en un ThingDef: mesas (cada producto hecho allí), plantas (cada cosecha),
    /// edificios (cada construcción terminada). Ampliable con XML.
    /// </summary>
    public class DefModExtension_ProgressCounter : DefModExtension
    {
        [NoTranslate] public string counter;
        public float amount = 1f;
    }

    /// <summary>Contadores de uso de un colono (en <see cref="CompCultivator"/>).</summary>
    public class TraitProgressTracker : IExposable
    {
        private Dictionary<string, float> counters = new Dictionary<string, float>();
        private List<string> tmpKeys;
        private List<float> tmpValues;

        public float Get(string counter) => counter != null && counters.TryGetValue(counter, out float v) ? v : 0f;

        public void Add(string counter, float amount) => counters[counter] = Get(counter) + amount;

        public void ExposeData()
        {
            Scribe_Collections.Look(ref counters, "counters", LookMode.Value, LookMode.Value, ref tmpKeys, ref tmpValues);
            if (Scribe.mode == LoadSaveMode.PostLoadInit) counters ??= new Dictionary<string, float>();
        }
    }

    public static class TraitProgression
    {
        private static Dictionary<string, List<TraitDef>> byCounter;

        private static Dictionary<string, List<TraitDef>> ByCounter
        {
            get
            {
                if (byCounter == null)
                {
                    byCounter = new Dictionary<string, List<TraitDef>>();
                    foreach (var t in DefDatabase<TraitDef>.AllDefsListForReading)
                    {
                        var ext = t.GetModExtension<DefModExtension_TraitProgression>();
                        if (ext?.counter == null) continue;
                        if (!byCounter.TryGetValue(ext.counter, out var list)) byCounter[ext.counter] = list = new List<TraitDef>();
                        list.Add(t);
                    }
                }
                return byCounter;
            }
        }

        /// <summary>Suma al contador del colono y sube de grado los rasgos que lo usan si cruzan un umbral.</summary>
        public static void Add(Pawn pawn, string counter, float amount = 1f)
        {
            if (pawn == null || counter.NullOrEmpty() || amount <= 0f || pawn.story?.traits == null) return;
            var comp = CompCultivator.For(pawn);
            if (comp == null) return;
            comp.Progress.Add(counter, amount);
            if (!ByCounter.TryGetValue(counter, out var traitDefs)) return;
            float value = comp.Progress.Get(counter);
            foreach (var def in traitDefs)
            {
                var trait = pawn.story.traits.GetTrait(def);
                if (trait == null) continue;
                var ext = def.GetModExtension<DefModExtension_TraitProgression>();
                var next = ext.steps.Where(s => s.degree > trait.Degree && value >= s.threshold && def.degreeDatas.Any(d => d.degree == s.degree))
                    .OrderByDescending(s => s.degree).FirstOrDefault();
                if (next != null) Upgrade(pawn, trait, next.degree);
            }
        }

        private static void Upgrade(Pawn pawn, Trait trait, int degree)
        {
            var def = trait.def;
            string before = trait.LabelCap;
            // Respeta los conflictos (p. ej. "prodigio" no convive con "aprende lento"): si choca, se queda como estaba. Se mira
            // antes de quitar el rasgo, para no sacarlo y volver a ponerlo en cada suma.
            if (!TraitConflictUtility.CanGain(pawn, def, degree, ignore: trait)) return;
            pawn.story.traits.RemoveTrait(trait);
            var upgraded = new Trait(def, degree, forced: true);
            pawn.story.traits.GainTrait(upgraded);
            if (pawn.Faction?.IsPlayer ?? false)
                Find.LetterStack.ReceiveLetter("CVG_TraitUp_Label".Translate(pawn.LabelShortCap, upgraded.LabelCap),
                    "CVG_TraitUp_Text".Translate(pawn.Named("PAWN"), before, upgraded.LabelCap), LetterDefOf.PositiveEvent, pawn);
        }

        /// <summary>Contador del ThingDef (o, para edificios de feng shui sin extensión, "Formations").</summary>
        public static DefModExtension_ProgressCounter CounterOf(ThingDef def) => def?.GetModExtension<DefModExtension_ProgressCounter>();
    }

    /// <summary>Cada producto hecho en una mesa con <see cref="DefModExtension_ProgressCounter"/> suma al contador del artesano.</summary>
    [HarmonyPatch(typeof(GenRecipe), nameof(GenRecipe.MakeRecipeProducts))]
    public static class Patch_GenRecipe_MakeRecipeProducts
    {
        public static void Postfix(Pawn worker, IBillGiver billGiver)
        {
            if (billGiver is Thing bench && TraitProgression.CounterOf(bench.def) is DefModExtension_ProgressCounter c)
                TraitProgression.Add(worker, c.counter, c.amount);
        }
    }

    /// <summary>Cosechar una planta con <see cref="DefModExtension_ProgressCounter"/> (hierbas espirituales).</summary>
    [HarmonyPatch(typeof(Plant), nameof(Plant.PlantCollected))]
    public static class Patch_Plant_PlantCollected
    {
        public static void Prefix(Plant __instance, Pawn by)
        {
            if (TraitProgression.CounterOf(__instance.def) is DefModExtension_ProgressCounter c) TraitProgression.Add(by, c.counter, c.amount);
        }
    }

    /// <summary>Terminar un edificio con contador o con feng shui (formaciones) suma al constructor.</summary>
    [HarmonyPatch(typeof(Frame), nameof(Frame.CompleteConstruction))]
    public static class Patch_Frame_CompleteConstruction
    {
        public static void Prefix(Frame __instance, Pawn worker)
        {
            var built = __instance.def.entityDefToBuild as ThingDef;
            if (built == null) return;
            if (TraitProgression.CounterOf(built) is DefModExtension_ProgressCounter c) TraitProgression.Add(worker, c.counter, c.amount);
            // Muebles y paredes con feng shui cuentan muy poco: antes cada tramo de pared de bambú valía lo mismo que un núcleo de formación
            // (los núcleos suman 3 con su propio contador, en Convergence.Formations).
            else if (built.GetModExtension<DefModExtension_FengShui>()?.qi > 0f) TraitProgression.Add(worker, "Formations", 0.1f);
        }
    }

    /// <summary>Domar un animal suma "BeastTaming" (×3 si es bestia espiritual).</summary>
    [HarmonyPatch(typeof(InteractionWorker_RecruitAttempt), nameof(InteractionWorker_RecruitAttempt.DoRecruit),
        new[] { typeof(Pawn), typeof(Pawn), typeof(string), typeof(string), typeof(bool), typeof(bool) },
        new[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out, ArgumentType.Normal, ArgumentType.Normal })]
    public static class Patch_RecruitAttempt_DoRecruit
    {
        public static void Postfix(Pawn recruiter, Pawn recruitee)
        {
            if (recruitee?.RaceProps == null || !recruitee.RaceProps.Animal) return;
            TraitProgression.Add(recruiter, "BeastTaming", Hediff_SpiritBeast.LevelOf(recruitee) > 0 ? 3f : 1f);
        }
    }
}
