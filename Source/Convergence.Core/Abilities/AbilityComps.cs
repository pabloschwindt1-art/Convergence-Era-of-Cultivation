using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>Potencia R de las capacidades según el reino del camino del qi (docs/modulos/razas/README.md §2).</summary>
    public static class AbilityPower
    {
        /// <summary>Reino (order) → R. Sin cultivo = 1.</summary>
        private static readonly SimpleCurve RealmPowerCurve = new SimpleCurve
        {
            new CurvePoint(1f, 1f),
            new CurvePoint(2f, 1.3f),
            new CurvePoint(3f, 1.7f),
            new CurvePoint(4f, 2.2f),
            new CurvePoint(5f, 2.8f),
            new CurvePoint(6f, 3.5f)
        };

        public static float ForRealmOrder(int order) => RealmPowerCurve.Evaluate(order);

        public static float For(Pawn pawn)
        {
            var comp = CompCultivator.For(pawn);
            if (comp == null || !comp.IsQiCultivator || !comp.Realm.Started) return 1f;
            return RealmPowerCurve.Evaluate(comp.Realm.Realm.order);
        }
    }

    /// <summary>
    /// Coste de qi de una capacidad innata (módulo de razas). Igual que <see cref="CompProperties_AbilityResourceCost"/>
    /// con el camino del qi; se conserva el campo <c>qiCost</c> del XML existente.
    /// </summary>
    public class CompProperties_AbilityQiCost : CompProperties_AbilityResourceCost
    {
        public float qiCost;

        public override float BaseCost => qiCost;
    }

    /// <summary>
    /// Cono de daño desde el lanzador (aliento): daño de cualquier <see cref="DamageDef"/>, aturdimiento con probabilidad y,
    /// opcionalmente, apaga el fuego. El daño se multiplica por la potencia del reino (y la maestría en técnicas).
    /// </summary>
    public class CompProperties_AbilityCone : CompProperties_AbilityEffect
    {
        public float range = 7f;
        public float lineWidthEnd = 3f;
        public DamageDef damageDef;
        public int damAmount = 10;
        public float armorPenetration = -1f;
        public float stunChance;
        public int stunTicks = 120;
        public bool extinguishFire;
        public bool scaleWithRealm = true;
        public bool affectAllies;
        public FleckDef fleckDef;

        public CompProperties_AbilityCone()
        {
            compClass = typeof(CompAbilityEffect_Cone);
        }
    }

    public class CompAbilityEffect_Cone : CompAbilityEffect
    {
        private readonly List<IntVec3> tmpCells = new List<IntVec3>();
        private readonly List<Thing> tmpThings = new List<Thing>();

        public new CompProperties_AbilityCone Props => (CompProperties_AbilityCone)props;

        private Pawn Caster => parent.pawn;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            var map = Caster.MapHeld;
            if (map == null) return;
            float amount = Props.damAmount * (Props.scaleWithRealm ? TechniqueUtility.Power(Caster, parent.def) : 1f);

            tmpThings.Clear();
            foreach (var cell in AffectedCells(target))
            {
                if (Props.fleckDef != null) FleckMaker.Static(cell, map, Props.fleckDef);
                else FleckMaker.ThrowMicroSparks(cell.ToVector3Shifted(), map);
                tmpThings.AddRange(cell.GetThingList(map));
            }

            foreach (var thing in tmpThings)
            {
                if (thing.Destroyed || thing == Caster) continue;
                if (thing is Fire)
                {
                    if (Props.extinguishFire) thing.Destroy();
                    continue;
                }
                if (Props.extinguishFire) thing.TryGetComp<CompAttachBase>()?.GetAttachment(ThingDefOf.Fire)?.Destroy();
                if (!Props.affectAllies && thing.Faction != null && thing.Faction == Caster.Faction) continue;
                if (!(thing is Pawn) && !(thing is Building)) continue;

                if (Props.damageDef != null && amount > 0f)
                    thing.TakeDamage(new DamageInfo(Props.damageDef, amount, Props.armorPenetration, -1f, Caster));
                if (thing is Pawn pawn && !pawn.Dead && Props.stunChance > 0f && Rand.Chance(Props.stunChance))
                    pawn.stances?.stunner?.StunFor(Props.stunTicks, Caster, addBattleLog: false);
            }
            tmpThings.Clear();
        }

        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            GenDraw.DrawFieldEdges(AffectedCells(target));
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            if (Caster.Faction == null || Props.affectAllies) return true;
            foreach (var cell in AffectedCells(target))
            {
                var things = cell.GetThingList(Caster.Map);
                for (int i = 0; i < things.Count; i++)
                    if (things[i] is Pawn p && p.Faction == Caster.Faction) return false;
            }
            return true;
        }

        /// <summary>Celdas del cono: dentro del alcance y del ángulo que abre <c>lineWidthEnd</c> al final.</summary>
        private List<IntVec3> AffectedCells(LocalTargetInfo target)
        {
            tmpCells.Clear();
            var map = Caster.Map;
            if (map == null) return tmpCells;
            var origin = Caster.Position;
            var targetCell = target.Cell.ClampInsideMap(map);
            if (targetCell == origin) return tmpCells;

            Vector3 from = origin.ToVector3Shifted().Yto0();
            Vector3 dir = (targetCell.ToVector3Shifted().Yto0() - from).normalized;
            float halfAngle = Mathf.Rad2Deg * Mathf.Atan2(Props.lineWidthEnd / 2f, Props.range);
            int count = GenRadial.NumCellsInRadius(Props.range);
            for (int i = 1; i < count; i++)
            {
                var c = origin + GenRadial.RadialPattern[i];
                if (!c.InBounds(map) || c.Filled(map)) continue;
                if (Vector3.Angle(c.ToVector3Shifted().Yto0() - from, dir) > halfAngle) continue;
                if (!GenSight.LineOfSight(origin, c, map, skipFirstCell: true)) continue;
                tmpCells.Add(c);
            }
            return tmpCells;
        }
    }

    /// <summary>
    /// Aura: da un hediff a los pawns en un radio alrededor del objetivo (o del lanzador si se lanza sobre sí mismo).
    /// La duración la pone el hediff (HediffComp_Disappears); con <c>scaleWithRealm</c> se multiplica por la potencia del reino.
    /// </summary>
    public class CompProperties_AbilityGiveHediffInRadius : CompProperties_AbilityEffect
    {
        public HediffDef hediffDef;
        public float radius = 6f;
        public bool affectSelf = true;
        public bool affectAllies = true;
        public bool affectHostiles;
        public bool affectAnimals = true;
        public bool scaleWithRealm = true;
        public FleckDef fleckDef;

        public CompProperties_AbilityGiveHediffInRadius()
        {
            compClass = typeof(CompAbilityEffect_GiveHediffInRadius);
        }
    }

    public class CompAbilityEffect_GiveHediffInRadius : CompAbilityEffect
    {
        public new CompProperties_AbilityGiveHediffInRadius Props => (CompProperties_AbilityGiveHediffInRadius)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            var caster = parent.pawn;
            var map = caster.MapHeld;
            if (map == null || Props.hediffDef == null) return;
            var center = target.IsValid ? target.Cell : caster.Position;
            float power = Props.scaleWithRealm ? TechniqueUtility.Power(caster, parent.def) : 1f;
            float radius = Props.radius * (Props.scaleWithRealm ? Mathf.Sqrt(power) : 1f);

            if (Props.fleckDef != null) FleckMaker.Static(center, map, Props.fleckDef, radius);
            foreach (var thing in GenRadial.RadialDistinctThingsAround(center, map, radius, useCenter: true))
            {
                if (!(thing is Pawn pawn) || pawn.Dead || !Affects(caster, pawn)) continue;
                var existing = pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef);
                if (existing != null) pawn.health.RemoveHediff(existing);
                var hediff = HediffMaker.MakeHediff(Props.hediffDef, pawn);
                var disappears = hediff.TryGetComp<HediffComp_Disappears>();
                if (disappears != null)
                {
                    disappears.ticksToDisappear = Mathf.RoundToInt(disappears.ticksToDisappear * power);
                    disappears.disappearsAfterTicks = disappears.ticksToDisappear;
                }
                pawn.health.AddHediff(hediff);
            }
        }

        private bool Affects(Pawn caster, Pawn pawn)
        {
            if (pawn == caster) return Props.affectSelf;
            if (!Props.affectAnimals && pawn.RaceProps.Animal) return false;
            if (pawn.HostileTo(caster)) return Props.affectHostiles;
            return Props.affectAllies && (pawn.Faction == caster.Faction || (pawn.Faction != null && !pawn.Faction.HostileTo(caster.Faction)));
        }

        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            GenDraw.DrawRadiusRing(target.IsValid ? target.Cell : parent.pawn.Position, Props.radius);
        }
    }
}
