using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Configuración del ritual de templado de una constitución, en su <c>RitualOutcomeEffectDef</c>. Cada paso sube de un estado al
    /// siguiente (sin rasgo → Común, Común → Superior, Superior → Celestial, que puede ser otro rasgo) con una probabilidad según la calidad
    /// del ritual. El costo se descuenta del almacén de la colonia al terminar.
    /// </summary>
    public class DefModExtension_TemperConstitution : DefModExtension
    {
        public List<TemperStep> steps;
        public List<ThingDefCountClass> cost;
        /// <summary>Id del rol del ritual que recibe el templado.</summary>
        public string subjectRoleId = "subject";

        /// <summary>El paso que le toca al pawn ahora (el de su estado actual), o null si ya no puede subir.</summary>
        public TemperStep StepFor(Pawn pawn) => steps?.FirstOrDefault(s => s.AppliesTo(pawn));

        public bool CanPay(Map map, out string missing)
        {
            missing = null;
            if (cost.NullOrEmpty() || map == null) return true;
            foreach (var c in cost)
            {
                int have = map.listerThings.ThingsOfDef(c.thingDef).Where(t => !t.IsForbidden(Faction.OfPlayer) && t.IsInAnyStorage()).Sum(t => t.stackCount);
                if (have < c.count) { missing = c.LabelCap; return false; }
            }
            return true;
        }

        public void Pay(Map map)
        {
            if (cost.NullOrEmpty() || map == null) return;
            foreach (var c in cost)
            {
                int left = c.count;
                foreach (var t in map.listerThings.ThingsOfDef(c.thingDef).Where(t => !t.IsForbidden(Faction.OfPlayer) && t.IsInAnyStorage()).ToList())
                {
                    if (left <= 0) break;
                    int take = Mathf.Min(left, t.stackCount);
                    t.SplitOff(take).Destroy();
                    left -= take;
                }
            }
        }
    }

    public class TemperStep
    {
        /// <summary>Estado de partida: sin rasgo (null) o el rasgo con su grado.</summary>
        public TraitDef fromTrait;
        public int fromDegree;
        /// <summary>Estado al que sube.</summary>
        public TraitDef toTrait;
        public int toDegree;
        /// <summary>Probabilidad de éxito según la calidad del ritual (0-1).</summary>
        public SimpleCurve chanceByQuality;
        /// <summary>Rasgos que impiden empezar desde cero (p. ej. ya tiene el Celestial).</summary>
        public List<TraitDef> blockedBy;

        public bool AppliesTo(Pawn pawn)
        {
            if (pawn?.story?.traits == null) return false;
            if (fromTrait == null)
                return ConstitutionUtility.Degree(pawn, toTrait) == null && (blockedBy == null || !blockedBy.Any(b => ConstitutionUtility.Degree(pawn, b) != null));
            return ConstitutionUtility.Degree(pawn, fromTrait) == fromDegree;
        }

        public float Chance(float quality) => Mathf.Clamp01(chanceByQuality?.Evaluate(quality) ?? quality);

        public void Apply(Pawn pawn)
        {
            if (fromTrait != null && fromTrait != toTrait) ConstitutionUtility.RemoveTrait(pawn, fromTrait);
            ConstitutionUtility.SetTrait(pawn, toTrait, toDegree);
        }

        public string ToLabel => toTrait.DataAtDegree(toDegree)?.LabelCap ?? toTrait.LabelCap;
    }

    /// <summary>
    /// Ritual de templado: además del ánimo según la calidad (como cualquier ritual), sube un paso la constitución del sujeto con la
    /// probabilidad del paso. El costo se paga siempre, salga bien o no.
    /// </summary>
    public class RitualOutcomeEffectWorker_TemperConstitution : RitualOutcomeEffectWorker_FromQuality
    {
        public RitualOutcomeEffectWorker_TemperConstitution() { }
        public RitualOutcomeEffectWorker_TemperConstitution(RitualOutcomeEffectDef def) : base(def) { }

        protected override void ApplyExtraOutcome(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, RitualOutcomePossibility outcome,
            out string extraOutcomeDesc, ref LookTargets letterLookTargets)
        {
            extraOutcomeDesc = null;
            var ext = def.GetModExtension<DefModExtension_TemperConstitution>();
            var subject = ext == null ? null : jobRitual.PawnWithRole(ext.subjectRoleId);
            if (subject == null) return;
            extraOutcomeDesc = Resolve(ext, subject, QualityOf(jobRitual), jobRitual.Map);
            letterLookTargets = subject;
        }

        private float QualityOf(LordJob_Ritual jobRitual)
        {
            try { return GetQuality(jobRitual, jobRitual.Progress); }
            catch { return 0.5f; }
        }

        /// <summary>Paga, tira el dado y aplica el paso. Devuelve el texto para la carta (público para el autotest).</summary>
        public static string Resolve(DefModExtension_TemperConstitution ext, Pawn subject, float quality, Map map)
        {
            var step = ext.StepFor(subject);
            ext.Pay(map);
            if (step == null) return "CVG_Temper_NothingToTemper".Translate(subject.Named("PAWN"));
            float chance = step.Chance(quality);
            if (Rand.Chance(chance))
            {
                step.Apply(subject);
                return "CVG_Temper_Success".Translate(subject.Named("PAWN"), step.ToLabel, chance.ToStringPercent());
            }
            return "CVG_Temper_Failure".Translate(subject.Named("PAWN"), step.ToLabel, chance.ToStringPercent());
        }
    }

    /// <summary>Sujeto del templado: un colono libre al que le queda un paso de la constitución y con el costo en el almacén.</summary>
    public class RitualRoleConstitutionSubject : RitualRoleColonist
    {
        public override bool AppliesToPawn(Pawn p, out string reason, TargetInfo selectedTarget, LordJob_Ritual ritual = null,
            RitualRoleAssignments assignments = null, Precept_Ritual precept = null, bool skipReason = false)
        {
            if (!base.AppliesToPawn(p, out reason, selectedTarget, ritual, assignments, precept, skipReason)) return false;
            var ext = (precept ?? ritual?.Ritual)?.outcomeEffect?.def?.GetModExtension<DefModExtension_TemperConstitution>();
            if (ext == null) return true;
            if (ext.StepFor(p) == null)
            {
                if (!skipReason) reason = "CVG_Temper_NoStep".Translate(p.Named("PAWN"));
                return false;
            }
            if (!ext.CanPay(p.MapHeld, out string missing))
            {
                if (!skipReason) reason = "CVG_Temper_MissingCost".Translate(missing);
                return false;
            }
            return true;
        }
    }
}
