using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    // Demonio de Sangre (docs/modulos/BIOTECH_WUXIA.md §2): reinterpreta al sanguófago con sus sistemas vanilla
    // (hemogén = esencia de sangre, sueño de muerte = retiro en el estanque de sangre). Aquí solo lo genérico: la técnica
    // de drenar sangre, la debilidad a un material (plata lunar) y los rasgos que dan un hediff al nacer.

    /// <summary>
    /// Debilidad a un material o a un arma: el daño de armas hechas de <see cref="stuff"/> (o de <see cref="weapons"/>)
    /// se multiplica por <see cref="factor"/>. En genes, rasgos o hediffs (Demonio de Sangre: plata lunar; jiangshi:
    /// madera de melocotonero); lo aplica <see cref="CompCultivator.PostPreApplyDamage"/>.
    /// </summary>
    public class DefModExtension_StuffWeakness : DefModExtension
    {
        public ThingDef stuff;
        public List<ThingDef> weapons;
        public float factor = 1.5f;

        public bool Matches(ThingDef weapon, ThingDef weaponStuff) =>
            (stuff != null && stuff == weaponStuff) || (weapons != null && weapons.Contains(weapon));
    }

    public static class StuffWeaknessUtility
    {
        /// <summary>Multiplicador del daño si el arma del atacante es del material (o el arma) débil del pawn.</summary>
        public static float FactorFor(Pawn victim, DamageInfo dinfo)
        {
            if (victim == null || !(dinfo.Instigator is Pawn attacker)) return 1f;
            var primary = attacker.equipment?.Primary;
            if (primary == null || dinfo.Weapon == null || primary.def != dinfo.Weapon) return 1f;
            float f = 1f;
            if (victim.genes != null)
            {
                foreach (var gene in victim.genes.GenesListForReading)
                {
                    if (!gene.Active) continue;
                    var ext = gene.def.GetModExtension<DefModExtension_StuffWeakness>();
                    if (ext != null && ext.Matches(primary.def, primary.Stuff)) f *= ext.factor;
                }
            }
            var hediffs = victim.health?.hediffSet?.hediffs;
            if (hediffs != null)
            {
                for (int i = 0; i < hediffs.Count; i++)
                {
                    var ext = hediffs[i].def.GetModExtension<DefModExtension_StuffWeakness>();
                    if (ext != null && ext.Matches(primary.def, primary.Stuff)) f *= ext.factor;
                }
            }
            return f;
        }
    }

    /// <summary>
    /// Técnica de sangre "drenar": daña al objetivo y la sangre robada cura al lanzador y le da hemogén (esencia de
    /// sangre). Escala con el poder de la técnica (reino × maestría).
    /// </summary>
    public class CompProperties_AbilityBloodDrain : CompProperties_AbilityEffect
    {
        public DamageDef damageDef;
        public float damage = 10f;
        /// <summary>Fracción del daño hecho que cura al lanzador.</summary>
        public float healFraction = 0.5f;
        /// <summary>Hemogén ganado (0-1) por punto de daño.</summary>
        public float hemogenPerDamage = 0.01f;
        public FleckDef fleck;

        public CompProperties_AbilityBloodDrain()
        {
            compClass = typeof(CompAbilityEffect_BloodDrain);
        }
    }

    public class CompAbilityEffect_BloodDrain : CompAbilityEffect
    {
        public new CompProperties_AbilityBloodDrain Props => (CompProperties_AbilityBloodDrain)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            var caster = parent.pawn;
            var victim = target.Pawn;
            if (victim == null || victim.Dead) return;
            float amount = Props.damage * TechniqueUtility.Power(caster, parent.def);
            var result = victim.TakeDamage(new DamageInfo(Props.damageDef ?? DamageDefOf.Cut, amount, 0.2f, -1f, caster));
            float dealt = result.totalDamageDealt;
            if (Props.fleck != null && victim.Spawned) FleckMaker.ConnectingLine(caster.DrawPos, victim.DrawPos, Props.fleck, caster.Map);
            HealInjuries(caster, dealt * Props.healFraction);
            var hemogen = caster.genes?.GetFirstGeneOfType<Gene_Hemogen>();
            if (hemogen != null) GeneUtility.OffsetHemogen(caster, dealt * Props.hemogenPerDamage);
        }

        public static void HealInjuries(Pawn pawn, float amount)
        {
            if (amount <= 0f) return;
            var injuries = pawn.health.hediffSet.hediffs.OfType<Hediff_Injury>().Where(h => h.CanHealNaturally() || h.CanHealFromTending()).ToList();
            foreach (var injury in injuries)
            {
                if (amount <= 0f) break;
                float heal = Mathf.Min(amount, injury.Severity);
                injury.Heal(heal);
                amount -= heal;
            }
        }

        public override bool AICanTargetNow(LocalTargetInfo target) => target.Pawn != null && target.Pawn.HostileTo(parent.pawn);
    }

    /// <summary>Rasgo que da un hediff al generarse el pawn (p. ej. el maestro de marionetas nace con su hilo).</summary>
    public class DefModExtension_TraitGrantsHediff : DefModExtension
    {
        public HediffDef hediff;
        public BodyPartDef bodyPart;
    }

    public static class TraitGrantsHediffUtility
    {
        public static void Apply(Pawn pawn)
        {
            var traits = pawn?.story?.traits?.allTraits;
            if (traits == null) return;
            foreach (var trait in traits)
            {
                var ext = trait.def.GetModExtension<DefModExtension_TraitGrantsHediff>();
                if (ext?.hediff == null || pawn.health.hediffSet.HasHediff(ext.hediff)) continue;
                var part = ext.bodyPart == null ? null : pawn.health.hediffSet.GetNotMissingParts().FirstOrDefault(p => p.def == ext.bodyPart);
                if (ext.bodyPart != null && part == null) continue;
                pawn.health.AddHediff(ext.hediff, part);
            }
        }
    }
}
