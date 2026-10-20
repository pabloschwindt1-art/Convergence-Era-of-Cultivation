using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Coste en el recurso de un camino (qi, sangre-qi…) de una técnica o capacidad, × stat <c>CVG_TechniqueCost</c> ×
    /// maestría. En técnicas (<see cref="TechniqueExtension"/>) además: exige los requisitos (camino, reino, elemento),
    /// suma maestría al usarla, acorta el enfriamiento según la maestría y esconde el gizmo si no está fijada en la barra.
    /// Sin extensión (capacidades innatas de linaje) solo cobra a quien sigue el camino: los demás la usan gratis.
    /// </summary>
    public class CompProperties_AbilityResourceCost : CompProperties_AbilityEffect
    {
        /// <summary>Camino cuyo recurso se gasta. Si falta: el del manual de la técnica, o el del qi.</summary>
        public CultivationPathDef path;
        public float cost;

        public virtual float BaseCost => cost;

        public CompProperties_AbilityResourceCost()
        {
            compClass = typeof(CompAbilityEffect_ResourceCost);
        }

        public override IEnumerable<string> ExtraStatSummary()
        {
            yield return "CVG_Ability_ResourceCost".Translate((path ?? CVG_DefOf.CVG_Path_Qi).resourceLabel.CapitalizeFirst(), BaseCost.ToString("0"));
        }
    }

    public class CompAbilityEffect_ResourceCost : CompAbilityEffect
    {
        public new CompProperties_AbilityResourceCost Props => (CompProperties_AbilityResourceCost)props;

        private Pawn Caster => parent.pawn;
        private TechniqueExtension Ext => TechniqueUtility.Ext(parent.def);

        public CultivationPathDef Path => Props.path ?? Ext?.manual?.Path ?? CVG_DefOf.CVG_Path_Qi;

        /// <summary>Recurso del lanzador, o null si no sigue el camino (capacidad gratis).</summary>
        private CultivationResource Resource
        {
            get
            {
                var comp = CompCultivator.For(Caster);
                if (comp == null || !comp.CanFollow(Path)) return null;
                var tracker = comp.TrackerFor(Path, create: true);
                return tracker.EnsureStarted() ? tracker.Resource : null;
            }
        }

        public float Cost => Props.BaseCost * TechniqueUtility.CostFactor(Caster, parent.def);

        public override bool GizmoDisabled(out string reason)
        {
            if (Ext != null && !TechniqueUtility.RequirementsMet(Caster, parent.def, out reason)) return true;
            var res = Resource;
            float cost = Cost;
            if (res != null && res.Current < cost)
            {
                reason = "CVG_Ability_NotEnoughResource".Translate(Caster.Named("PAWN"), cost.ToString("0"), res.Current.ToString("0"), res.Label);
                return true;
            }
            reason = null;
            return false;
        }

        /// <summary>Solo las técnicas equipadas, y en colonos del jugador solo reclutados (interfaz simple, como los psíquicos).</summary>
        public override bool ShouldHideGizmo => Ext != null && PassiveTechniqueUtility.ShouldHideGizmo(Caster, parent.def);

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            if (Ext != null && !TechniqueUtility.RequirementsMet(Caster, parent.def, out _)) return false;
            var res = Resource;
            return res == null || res.Current >= Cost;
        }

        public override void PostApplied(List<LocalTargetInfo> targets, Map map)
        {
            base.PostApplied(targets, map);
            Resource?.TryConsume(Cost);
            var ext = Ext;
            if (ext == null) return;

            float cooldown = TechniqueUtility.CooldownFactor(Caster, parent.def);
            if (cooldown < 0.999f && parent.CooldownTicksRemaining > 0)
                parent.StartCooldown(Mathf.Max(1, Mathf.RoundToInt(parent.CooldownTicksTotal * cooldown)));

            TraitProgression.Add(Caster, "TechniquesCast");
            CultivationLife.Notify_TechniqueCast(Caster);
            var comp = CompCultivator.For(Caster);
            if (comp != null)
                comp.Techniques.AddMasteryXp(parent.def,
                    ext.masteryXpPerUse * (TechniqueUtility.HasAffinity(Caster, parent.def) ? TechniqueUtility.AffinityMasteryFactor : 1f));
        }

        public override string ExtraTooltipPart()
        {
            var sb = new StringBuilder();
            sb.Append("CVG_Ability_ResourceCost".Translate(Path.resourceLabel.CapitalizeFirst(), Cost.ToString("0")));
            if (Ext != null)
            {
                var comp = CompCultivator.For(Caster);
                var mastery = TechniqueUtility.Mastery(Caster, parent.def);
                if (mastery != null)
                {
                    sb.AppendLine();
                    var next = mastery.Next;
                    sb.Append("CVG_Technique_MasteryLine".Translate(mastery.LabelCap.Colorize(mastery.color),
                        next == null ? "-" : (comp?.Techniques.MasteryXp(parent.def) ?? 0f).ToString("0") + " / " + next.xpRequired.ToString("0")));
                }
                sb.AppendLine();
                sb.Append("CVG_Technique_PowerLine".Translate(TechniqueUtility.Power(Caster, parent.def).ToStringPercent()));
                if (TechniqueUtility.HasAffinity(Caster, parent.def)) sb.Append(" " + "CVG_Technique_Affinity".Translate());
                sb.AppendLine();
                sb.Append("CVG_Technique_SpiritGradeLine".Translate(SpiritGradeUtility.TechniqueGrade(Caster, parent.def)));
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// Lanza un proyectil al objetivo. Con <see cref="ITechniqueProjectile"/> (clases <c>Projectile_Technique*</c>) el
    /// daño se multiplica por el poder de la técnica (reino × maestría × afinidad). El grado espiritual del impacto es el
    /// reino del lanzador (lo calcula la supresión de reino al recibir el daño).
    /// </summary>
    public class CompProperties_AbilityTechniqueProjectile : CompProperties_AbilityEffect
    {
        public ThingDef projectileDef;
        /// <summary>Fleck en el lanzador al disparar (opcional).</summary>
        public FleckDef castFleck;
        public float castFleckScale = 1f;

        public CompProperties_AbilityTechniqueProjectile()
        {
            compClass = typeof(CompAbilityEffect_TechniqueProjectile);
        }

        public override IEnumerable<string> ConfigErrors(AbilityDef parentDef)
        {
            foreach (var e in base.ConfigErrors(parentDef)) yield return e;
            if (projectileDef?.projectile == null) yield return "projectileDef is null or not a projectile";
        }
    }

    public class CompAbilityEffect_TechniqueProjectile : CompAbilityEffect
    {
        public new CompProperties_AbilityTechniqueProjectile Props => (CompProperties_AbilityTechniqueProjectile)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            var caster = parent.pawn;
            var map = caster.Map;
            if (map == null || Props.projectileDef == null) return;
            if (Props.castFleck != null) FleckMaker.Static(caster.DrawPos, map, Props.castFleck, Props.castFleckScale);
            var projectile = (Projectile)GenSpawn.Spawn(Props.projectileDef, caster.Position, map);
            if (projectile is ITechniqueProjectile tp) tp.DamageFactor = TechniqueUtility.Power(caster, parent.def);
            projectile.Launch(caster, caster.DrawPos, target, target, ProjectileHitFlags.IntendedTarget, parent.verb.preventFriendlyFire);
        }

        public override bool AICanTargetNow(LocalTargetInfo target) => target.Pawn != null;
    }

    /// <summary>
    /// Da un hediff al objetivo o al lanzador (escudo de qi, buffs, curas, debuffs). La duración
    /// (<see cref="HediffComp_Disappears"/>) se multiplica por poder^<see cref="durationPowerExponent"/>, y los comps del
    /// hediff que implementan <see cref="ITechniquePowered"/> (p. ej. el escudo) reciben el poder.
    /// </summary>
    public class CompProperties_AbilityTechniqueHediff : CompProperties_AbilityEffect
    {
        public HediffDef hediffDef;
        public bool onCaster;
        public bool replaceExisting = true;
        public float durationPowerExponent = 0.5f;
        /// <summary>La IA solo la usa con un enemigo cerca o en combate reciente (buffs y escudos).</summary>
        public bool aiOnlyInCombat = true;
        public FleckDef fleckDef;
        public float fleckScale = 1f;

        public CompProperties_AbilityTechniqueHediff()
        {
            compClass = typeof(CompAbilityEffect_TechniqueHediff);
        }

        public override IEnumerable<string> ConfigErrors(AbilityDef parentDef)
        {
            foreach (var e in base.ConfigErrors(parentDef)) yield return e;
            if (hediffDef == null) yield return "hediffDef is null";
        }
    }

    public class CompAbilityEffect_TechniqueHediff : CompAbilityEffect
    {
        private const int RecentCombatTicks = 900;

        public new CompProperties_AbilityTechniqueHediff Props => (CompProperties_AbilityTechniqueHediff)props;

        private Pawn Caster => parent.pawn;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            var pawn = Props.onCaster ? Caster : target.Pawn;
            if (pawn == null || pawn.Dead || Props.hediffDef == null) return;

            if (Props.replaceExisting)
            {
                var existing = pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef);
                if (existing != null) pawn.health.RemoveHediff(existing);
            }

            float power = TechniqueUtility.Power(Caster, parent.def);
            var hediff = HediffMaker.MakeHediff(Props.hediffDef, pawn);
            var disappears = hediff.TryGetComp<HediffComp_Disappears>();
            if (disappears != null)
            {
                disappears.ticksToDisappear = Mathf.RoundToInt(disappears.ticksToDisappear * Mathf.Pow(power, Props.durationPowerExponent));
                disappears.disappearsAfterTicks = disappears.ticksToDisappear;
            }
            if (hediff is HediffWithComps withComps)
                foreach (var c in withComps.comps)
                    if (c is ITechniquePowered powered) powered.SetPower(power);
            pawn.health.AddHediff(hediff);

            if (Props.fleckDef != null && pawn.Spawned) FleckMaker.AttachedOverlay(pawn, Props.fleckDef, Vector3.zero, Props.fleckScale);
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            var pawn = Props.onCaster ? Caster : target.Pawn;
            if (pawn == null || pawn.health.hediffSet.HasHediff(Props.hediffDef)) return false;
            if (!Props.onCaster && pawn.HostileTo(Caster) != parent.def.hostile) return false;
            if (!Props.aiOnlyInCombat) return true;
            var mind = Caster.mindState;
            return mind != null && (mind.enemyTarget != null || Find.TickManager.TicksGame - mind.lastCombatantTick < RecentCombatTicks);
        }
    }

    /// <summary>Comp de hediff que escala con el poder de la técnica que lo dio (escudo de qi…).</summary>
    public interface ITechniquePowered
    {
        void SetPower(float power);
    }

    /// <summary>Gizmo de técnica: muestra el coste arriba a la derecha (como VPE).</summary>
    public class Command_Technique : Command_Ability
    {
        public Command_Technique(Ability ability, Pawn pawn) : base(ability, pawn)
        {
        }

        public override string TopRightLabel
        {
            get
            {
                var cost = ability.CompOfType<CompAbilityEffect_ResourceCost>();
                return cost == null ? base.TopRightLabel : TechniqueUtility.CostLabel(cost.Cost, cost.Path);
            }
        }
    }
}
