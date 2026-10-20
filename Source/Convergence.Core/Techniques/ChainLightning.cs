using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Rayo en cadena (camino del Trueno; idea del Chain Bolt de VPE): golpea al objetivo y salta a los enemigos más
    /// cercanos (hasta <see cref="jumps"/>, a menos de <see cref="jumpRadius"/> del último). Cada salto aturde un momento
    /// y hace algo menos de daño. Escala con el poder de la técnica (reino × maestría).
    /// </summary>
    public class CompProperties_AbilityChainLightning : CompProperties_AbilityEffect
    {
        public DamageDef damageDef;
        public float damage = 12f;
        public float armorPenetration = 0.2f;
        public int jumps = 3;
        public float jumpRadius = 5.9f;
        /// <summary>Daño de cada salto respecto del anterior.</summary>
        public float falloff = 0.8f;
        public int stunTicks = 60;
        public FleckDef fleck;

        public CompProperties_AbilityChainLightning()
        {
            compClass = typeof(CompAbilityEffect_ChainLightning);
        }
    }

    public class CompAbilityEffect_ChainLightning : CompAbilityEffect
    {
        public new CompProperties_AbilityChainLightning Props => (CompProperties_AbilityChainLightning)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            var caster = parent.pawn;
            var map = caster.Map;
            var current = target.Thing;
            if (map == null || current == null) return;
            float damage = Props.damage * TechniqueUtility.Power(caster, parent.def);
            var hit = new HashSet<Thing>();
            var from = caster.DrawPos;
            for (int i = 0; i <= Props.jumps && current != null; i++)
            {
                hit.Add(current);
                if (Props.fleck != null) FleckMaker.ConnectingLine(from, current.DrawPos, Props.fleck, map);
                FleckMaker.ThrowLightningGlow(current.DrawPos, map, 1.2f);
                from = current.DrawPos;
                var pos = current.Position;
                current.TakeDamage(new DamageInfo(Props.damageDef ?? DamageDefOf.Burn, damage, Props.armorPenetration, -1f, caster));
                if (current is Pawn p && !p.Dead && Props.stunTicks > 0) p.stances?.stunner?.StunFor(Props.stunTicks, caster, addBattleLog: false);
                damage *= Props.falloff;
                current = GenRadial.RadialDistinctThingsAround(pos, map, Props.jumpRadius, true)
                    .OfType<Pawn>()
                    .Where(x => !hit.Contains(x) && !x.Dead && !x.Downed && x.HostileTo(caster))
                    .OrderBy(x => x.Position.DistanceToSquared(pos))
                    .FirstOrDefault();
            }
        }

        public override bool AICanTargetNow(LocalTargetInfo target) => target.Pawn != null && target.Pawn.HostileTo(parent.pawn);
    }
}
