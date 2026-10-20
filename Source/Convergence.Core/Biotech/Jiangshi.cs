using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace Convergence
{
    // Jiangshi (docs/modulos/BIOTECH_WUXIA.md §3): cadáveres que absorben el yang de los vivos, sobre los mutantes de
    // Anomaly (MutantDef, facción de entidades, plataforma de contención). Aquí solo lo genérico: el daño que roba yang,
    // la quemadura del sol, el espejo bagua y el evento nocturno. Las debilidades a materiales y armas (madera de
    // melocotonero) usan DefModExtension_StuffWeakness; el arroz glutinoso es una operación vanilla (Recipe_RemoveHediff).

    /// <summary>
    /// Daño que roba el yang (extraMeleeDamages de las garras del jiangshi): suma severidad del hediff
    /// <see cref="DamageDef.hediff"/> (qi cadavérico) a la víctima y cura al atacante. No hiere por sí mismo.
    /// </summary>
    public class DamageWorker_YangDrain : DamageWorker
    {
        public const float SeverityPerDamage = 0.02f;

        public override DamageResult Apply(DamageInfo dinfo, Thing victim)
        {
            var result = new DamageResult();
            if (!(victim is Pawn pawn) || pawn.Dead || def.hediff == null) return result;
            if (!pawn.RaceProps.IsFlesh || pawn.IsMutant) return result;
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(def.hediff);
            float severity = dinfo.Amount * SeverityPerDamage;
            if (hediff == null)
            {
                hediff = HediffMaker.MakeHediff(def.hediff, pawn);
                hediff.Severity = severity;
                pawn.health.AddHediff(hediff);
            }
            else hediff.Severity += severity;
            if (dinfo.Instigator is Pawn drinker && !drinker.Dead)
                CompAbilityEffect_BloodDrain.HealInjuries(drinker, dinfo.Amount);
            return result;
        }
    }

    /// <summary>Quemadura del sol: al aire libre y con luz de día el pawn recibe daño (jiangshi, criaturas yin).</summary>
    public class HediffCompProperties_SunBurn : HediffCompProperties
    {
        public DamageDef damageDef;
        public float damage = 2f;
        public int intervalTicks = 250;
        public float minSkyGlow = 0.5f;
        public FleckDef fleck;

        public HediffCompProperties_SunBurn()
        {
            compClass = typeof(HediffComp_SunBurn);
        }
    }

    public class HediffComp_SunBurn : HediffComp
    {
        public HediffCompProperties_SunBurn Props => (HediffCompProperties_SunBurn)props;

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);
            var pawn = Pawn;
            if (!pawn.Spawned || !pawn.IsHashIntervalTick(Props.intervalTicks, delta)) return;
            if (!InSunlight(pawn, Props.minSkyGlow)) return;
            pawn.TakeDamage(new DamageInfo(Props.damageDef ?? DamageDefOf.Burn, Props.damage));
            if (Props.fleck != null && pawn.Spawned) FleckMaker.Static(pawn.DrawPos, pawn.Map, Props.fleck);
        }

        public static bool InSunlight(Pawn pawn, float minSkyGlow)
        {
            var map = pawn.Map;
            return map != null && !pawn.Position.Roofed(map) && map.skyManager.CurSkyGlow >= minSkyGlow;
        }
    }

    /// <summary>
    /// Espejo bagua: los pawns con alguno de <see cref="affectedHediffs"/> que estén cerca y a la vista quedan
    /// deslumbrados (<see cref="hediff"/>) y reciben un poco de daño.
    /// </summary>
    public class CompProperties_BaguaMirror : CompProperties
    {
        public float radius = 7.9f;
        public List<HediffDef> affectedHediffs = new List<HediffDef>();
        public HediffDef hediff;
        public DamageDef damageDef;
        public float damage = 3f;
        public FleckDef fleck;

        public CompProperties_BaguaMirror()
        {
            compClass = typeof(CompBaguaMirror);
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (var e in base.ConfigErrors(parentDef)) yield return e;
            if (hediff == null) yield return "CompProperties_BaguaMirror: hediff is null";
            if (parentDef.tickerType != TickerType.Rare) yield return "CompProperties_BaguaMirror needs tickerType Rare";
        }
    }

    public class CompBaguaMirror : ThingComp
    {
        public CompProperties_BaguaMirror Props => (CompProperties_BaguaMirror)props;

        public override void CompTickRare()
        {
            base.CompTickRare();
            Pulse();
        }

        /// <summary>Deslumbra a los afectados en el radio. Devuelve cuántos.</summary>
        public int Pulse()
        {
            var map = parent.Map;
            if (map == null) return 0;
            int count = 0;
            foreach (var pawn in map.mapPawns.AllPawnsSpawned.ToList())
            {
                if (pawn.Dead || !pawn.Position.InHorDistOf(parent.Position, Props.radius)) continue;
                if (!Props.affectedHediffs.Any(h => pawn.health.hediffSet.HasHediff(h))) continue;
                if (!GenSight.LineOfSight(parent.Position, pawn.Position, map)) continue;
                var dazed = pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediff);
                if (dazed == null) pawn.health.AddHediff(Props.hediff);
                else
                {
                    var disappears = dazed.TryGetComp<HediffComp_Disappears>();
                    if (disappears != null) disappears.ticksToDisappear = disappears.disappearsAfterTicks;
                }
                if (Props.damage > 0f) pawn.TakeDamage(new DamageInfo(Props.damageDef ?? DamageDefOf.Burn, Props.damage, 0f, -1f, parent));
                if (Props.fleck != null && pawn.Spawned) FleckMaker.ConnectingLine(parent.DrawPos, pawn.DrawPos, Props.fleck, map);
                count++;
            }
            return count;
        }

        public override void PostDrawExtraSelectionOverlays()
        {
            base.PostDrawExtraSelectionOverlays();
            GenDraw.DrawRadiusRing(parent.Position, Props.radius);
        }
    }

    /// <summary>Hace visible el radio del espejo al colocarlo.</summary>
    public class PlaceWorker_BaguaMirror : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            var props = def.GetCompProperties<CompProperties_BaguaMirror>();
            if (props != null) GenDraw.DrawRadiusRing(center, props.radius);
        }
    }

    /// <summary>Jiangshi del evento: tipo de pawn, puntos y si solo cae de noche.</summary>
    public class DefModExtension_JiangshiIncident : DefModExtension
    {
        public PawnKindDef pawnKind;
        public IntRange count = new IntRange(2, 12);
        /// <summary>Hora local desde la que puede ocurrir (incluida) y hasta la que puede ocurrir (excluida).</summary>
        public int fromHour = 20;
        public int toHour = 4;
        public bool requiresConvergence = true;
    }

    /// <summary>
    /// Noche de los fantasmas: una horda de jiangshi (mutantes de Anomaly, facción de entidades) entra por el borde y
    /// asalta la colonia. Solo de noche; los puntos de amenaza deciden cuántos.
    /// </summary>
    public class IncidentWorker_JiangshiNight : IncidentWorker
    {
        private DefModExtension_JiangshiIncident Ext => def.GetModExtension<DefModExtension_JiangshiIncident>();

        public static bool IsNight(Map map, int fromHour, int toHour)
        {
            int hour = GenLocalDate.HourInteger(map);
            return fromHour <= toHour ? hour >= fromHour && hour < toHour : hour >= fromHour || hour < toHour;
        }

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            var ext = Ext;
            if (!ModsConfig.AnomalyActive || ext?.pawnKind == null || Faction.OfEntities == null) return false;
            if (ext.requiresConvergence && !GameComponent_Convergence.Active) return false;
            if (!(parms.target is Map map) || !IsNight(map, ext.fromHour, ext.toHour)) return false;
            return base.CanFireNowSub(parms);
        }

        public int CountFor(float points)
        {
            var ext = Ext;
            int n = Mathf.RoundToInt(points / Mathf.Max(1f, ext.pawnKind.combatPower));
            return Mathf.Clamp(n, ext.count.min, ext.count.max);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var ext = Ext;
            var map = (Map)parms.target;
            if (ext?.pawnKind == null || !RCellFinder.TryFindRandomPawnEntryCell(out var entry, map, CellFinder.EdgeRoadChance_Hostile))
                return false;
            var pawns = Generate(ext.pawnKind, CountFor(parms.points + CultivationWealth.ThreatPoints(map)), map, entry);
            if (pawns.Count == 0) return false;
            LordMaker.MakeNewLord(Faction.OfEntities, new LordJob_AssaultColony(Faction.OfEntities, canKidnap: false,
                canTimeoutOrFlee: false, sappers: false, useAvoidGridSmart: false, canSteal: false), map, pawns);
            SendStandardLetter(def.letterLabel, def.letterText.Formatted(pawns.Count.Named("COUNT")), def.letterDef, parms, pawns);
            Find.TickManager.slower.SignalForceNormalSpeedShort();
            return true;
        }

        public static List<Pawn> Generate(PawnKindDef kind, int count, Map map, IntVec3 near)
        {
            var list = new List<Pawn>();
            for (int i = 0; i < count; i++)
            {
                var pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, Faction.OfEntities, PawnGenerationContext.NonPlayer,
                    map.Tile, forceGenerateNewPawn: true, canGeneratePawnRelations: false, mustBeCapableOfViolence: false));
                GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(near, map, 8), map);
                list.Add(pawn);
            }
            return list;
        }
    }
}
