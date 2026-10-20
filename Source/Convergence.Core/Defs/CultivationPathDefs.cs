using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Camino de cultivo (los Tres Tesoros 精气神): Qi, Cuerpo (M4b), Shen (más adelante). Cada
    /// <see cref="CultivationRealmDef"/> pertenece a un camino y cada colono lleva un <see cref="RealmTracker"/> por
    /// camino que puede cultivar (la estructura admite doble cultivo). El camino dice qué recurso usa
    /// (<see cref="resourceClass"/>) y quién puede seguirlo (<see cref="workerClass"/>).
    /// </summary>
    public class CultivationPathDef : Def
    {
        public int order;
        /// <summary>Decide quién puede seguir este camino.</summary>
        public Type workerClass = typeof(CultivationPathWorker);
        /// <summary>Clase del recurso del camino (qi, sangre-qi…). Por defecto, un recurso con máximo y regeneración por stats.</summary>
        public Type resourceClass = typeof(StatResource);
        /// <summary>Stat del máximo del recurso (para <see cref="StatResource"/>).</summary>
        public StatDef resourceMaxStat;
        /// <summary>Stat de la regeneración por hora (para <see cref="StatResource"/>).</summary>
        public StatDef resourceRegenStat;
        /// <summary>Nombre corto del recurso (título de la barra).</summary>
        [MustTranslate] public string resourceLabel;
        /// <summary>Texto del tooltip de la barra.</summary>
        [MustTranslate] public string resourceDescription;
        public Color resourceColor = new Color(0.25f, 0.55f, 0.75f);
        /// <summary>Requiere una raíz espiritual despierta (camino del qi).</summary>
        public bool requiresSpiritRoot;
        /// <summary>La concentración de qi del lugar afecta al avance (el cuerpo no depende de ella).</summary>
        public bool usesQiConcentration = true;
        /// <summary>
        /// Se empieza solo en cuanto el colono puede seguirlo (qi). Si es false (cuerpo), el camino empieza cuando algo
        /// lo arranca: comer carne de bestia espiritual, beber su sangre… (<see cref="RealmTracker.EnsureStarted"/>).
        /// </summary>
        public bool autoStart = true;
        /// <summary>Reino (order) → ranuras de técnicas activas equipables (null = 3, y 4 desde el segundo reino).</summary>
        public SimpleCurve activeTechniqueSlots;

        /// <summary>Probabilidad de éxito sin ayudas → calidad del fundamento al subir (1 = normal).</summary>
        public SimpleCurve foundationQualityCurve;
        /// <summary>Calidad que se resta si subió con ayudas (píldoras): fundamento inestable.</summary>
        public float foundationAidPenalty = 0.1f;
        public FloatRange foundationQualityRange = new FloatRange(0.5f, 1.3f);

        [Unsaved] private CultivationPathWorker worker;

        public CultivationPathWorker Worker
        {
            get
            {
                if (worker == null)
                {
                    worker = (CultivationPathWorker)Activator.CreateInstance(workerClass);
                    worker.def = this;
                }
                return worker;
            }
        }

        public CultivationResource MakeResource(Pawn pawn)
        {
            var r = (CultivationResource)Activator.CreateInstance(resourceClass);
            r.pawn = pawn;
            r.path = this;
            return r;
        }

        /// <summary>Calidad del fundamento según la probabilidad sin ayudas y si se usaron ayudas.</summary>
        public float FoundationQuality(float chanceWithoutAids, bool usedAids)
        {
            float q = foundationQualityCurve?.Evaluate(chanceWithoutAids) ?? 1f;
            if (usedAids) q -= foundationAidPenalty;
            return foundationQualityRange.ClampToRange(q);
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var e in base.ConfigErrors()) yield return e;
            if (workerClass == null || !typeof(CultivationPathWorker).IsAssignableFrom(workerClass)) yield return "bad workerClass";
            if (resourceClass == null || !typeof(CultivationResource).IsAssignableFrom(resourceClass)) yield return "bad resourceClass";
            if (resourceClass == typeof(StatResource) && (resourceMaxStat == null || resourceRegenStat == null))
                yield return "StatResource needs resourceMaxStat and resourceRegenStat";
        }
    }

    /// <summary>Quién puede seguir un camino. Las subclases (p. ej. el cuerpo en M4b) cambian la regla.</summary>
    public class CultivationPathWorker
    {
        public CultivationPathDef def;

        public virtual bool CanCultivate(Pawn pawn, CompCultivator comp)
        {
            if (!comp.IsAwakened) return false;
            return !def.requiresSpiritRoot || comp.HasRoot;
        }
    }

    /// <summary>
    /// Camino del cuerpo (M4b): lo siguen los humanos adultos SIN raíz espiritual tras la Convergencia (incluidos los
    /// cuerpos especiales sin raíz: Acero, Tribulación, Espada, Espiritual Innato). No empieza solo: hace falta comer
    /// carne o sangre de bestia espiritual (<see cref="CultivationPathDef.autoStart"/> = false).
    /// </summary>
    public class CultivationPathWorker_Body : CultivationPathWorker
    {
        public override bool CanCultivate(Pawn pawn, CompCultivator comp)
        {
            if (!GameComponent_Convergence.Active || comp.HasRoot) return false;
            return pawn.RaceProps.Humanlike && (pawn.ageTracker?.Adult ?? false);
        }
    }

    /// <summary>
    /// Recurso de un camino (qi, sangre-qi…). Un objeto por camino y colono, guardado en su <see cref="RealmTracker"/>.
    /// </summary>
    public abstract class CultivationResource : IExposable
    {
        protected float current;

        public Pawn pawn;
        public CultivationPathDef path;

        public float Current => current;
        public abstract float Max { get; }
        public virtual float RegenPerHour => 0f;
        public float Percent => Max > 0f ? Mathf.Clamp01(current / Max) : 0f;
        public bool IsFull => Percent >= 0.999f;
        public string Label => path?.resourceLabel ?? "?";

        /// <summary>Se llama cada <paramref name="ticks"/> ticks mientras el colono sigue el camino.</summary>
        public virtual void TickInterval(int ticks)
        {
            // Si el máximo bajó (perdió el reino, fundamento peor, editor), el sobrante se disipa.
            float max = Max;
            if (current > max) current = max;
            Regen(ticks);
        }

        /// <summary>Regenera el recurso de <paramref name="ticks"/> ticks, multiplicado por <paramref name="factor"/>.</summary>
        public void Regen(int ticks, float factor = 1f) => Gain(RegenPerHour * factor * ticks / GenDate.TicksPerHour);

        public void Gain(float amount) => current = Mathf.Clamp(current + amount, 0f, Max);

        public bool TryConsume(float amount)
        {
            if (current < amount) return false;
            current -= amount;
            return true;
        }

        public void Fill() => current = Max;
        public void Drain() => current = 0f;
        public void SetCurrent(float value) => current = Mathf.Clamp(value, 0f, Max);

        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref current, "current");
            Scribe_Defs.Look(ref path, "path");
        }
    }

    /// <summary>Recurso con máximo y regeneración por stats del camino (el qi usa <c>CVG_QiMax</c> / <c>CVG_QiRegen</c>).</summary>
    public class StatResource : CultivationResource
    {
        public override float Max => path?.resourceMaxStat == null ? 0f : Mathf.Max(0f, pawn.GetStatValue(path.resourceMaxStat));
        public override float RegenPerHour => path?.resourceRegenStat == null ? 0f : pawn.GetStatValue(path.resourceRegenStat);
    }

    /// <summary>
    /// Requisito para intentar el avance a un reino (lista <c>breakthroughRequirements</c> del reino, en XML con Class=).
    /// Previsto: atributos de fundamento (Templado corporal, Pureza del qi, Fuerza del alma), reino de otro camino…
    /// </summary>
    public abstract class BreakthroughRequirement
    {
        /// <summary>Si se cumple; si no, el motivo para el botón desactivado.</summary>
        public abstract bool Met(Pawn pawn, RealmTracker tracker, out string reason);
    }

    /// <summary>Requiere un valor mínimo de un stat (los atributos de fundamento serán stats).</summary>
    public class BreakthroughRequirement_Stat : BreakthroughRequirement
    {
        public StatDef stat;
        public float min;

        public override bool Met(Pawn pawn, RealmTracker tracker, out string reason)
        {
            float value = pawn.GetStatValue(stat);
            reason = value >= min ? null : "CVG_Breakthrough_RequiresStat".Translate(stat.LabelCap, stat.ValueToString(min), stat.ValueToString(value)).Resolve();
            return reason == null;
        }
    }

    /// <summary>Requiere haber llegado a un reino (de cualquier camino; útil para doble cultivo).</summary>
    public class BreakthroughRequirement_Realm : BreakthroughRequirement
    {
        public CultivationRealmDef realm;

        public override bool Met(Pawn pawn, RealmTracker tracker, out string reason)
        {
            var other = CompCultivator.For(pawn)?.TrackerFor(realm.path);
            bool ok = other?.Realm != null && other.Realm.order >= realm.order;
            reason = ok ? null : "CVG_Breakthrough_RequiresRealm".Translate(realm.LabelCap).Resolve();
            return ok;
        }
    }
}
