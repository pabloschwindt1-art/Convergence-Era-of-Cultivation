using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Convergence
{
    /// <summary>
    /// Tribulación celestial: tras un retiro de avance exitoso hacia un reino con <c>tribulation</c>, el cielo lanza
    /// rayos sobre el colono durante unos segundos. Rayos y daño crecen con <c>CVG_CelestialDissonance</c>
    /// (× el ajuste de disonancia). Aguanta en pie → sube de reino. Cae → no sube; muere solo si el ajuste
    /// <c>tribulationCanKill</c> lo permite (si no, los rayos evitan las partes vitales y paran al derribarlo).
    /// </summary>
    public class MapComponent_Tribulation : MapComponent
    {
        // Unos segundos de nubes antes del primer rayo: el tiempo de que la colonia salga a mirar.
        private const int WarningTicks = 900;
        private const float SafeDistanceMin = 6f;
        private const float SafeDistanceMax = 13f;
        /// <summary>Cada testigo resta este porcentaje al daño de los rayos, hasta <see cref="MinWitnessFactor"/>.</summary>
        private const float PerWitness = 0.07f;
        private const float MinWitnessFactor = 0.6f;

        private List<Tribulation> active = new List<Tribulation>();

        public MapComponent_Tribulation(Map map) : base(map)
        {
        }

        public static MapComponent_Tribulation For(Map map) => map?.GetComponent<MapComponent_Tribulation>();

        public bool IsUndergoing(Pawn pawn) => active.Any(t => t.pawn == pawn);

        /// <summary>Strikes y daño por rayo para este colono y reino.</summary>
        public static void Strength(Pawn pawn, CultivationRealmDef target, out int strikes, out float damage)
        {
            float dissonance = Mathf.Max(0f, pawn.GetStatValue(CVG_DefOf.CVG_CelestialDissonance))
                * ConvergenceMod.Settings.celestialDissonanceFactor;
            strikes = Mathf.Max(1, target.tribulationStrikes + Mathf.RoundToInt(dissonance * target.tribulationStrikesPerDissonance));
            damage = target.tribulationDamage * (1f + dissonance * target.tribulationDamagePerDissonance) * CultivationDifficulty.Current.tribulationDamage;
            // Una raíz de Trueno nació del rayo: el cielo la castiga menos.
            var root = CompCultivator.For(pawn)?.Root;
            if (root != null && root.elements.Count > 0) damage *= root.elements.Min(e => e.tribulationDamageFactor);
        }

        /// <summary>Empieza una tribulación (carta de aviso y unos segundos de calma antes del primer rayo).</summary>
        public void Begin(Pawn pawn, CultivationRealmDef target, float foundationQuality = 1f)
        {
            if (pawn == null || target == null || IsUndergoing(pawn)) return;
            Strength(pawn, target, out int strikes, out float damage);
            active.Add(new Tribulation
            {
                pawn = pawn,
                target = target,
                strikesLeft = strikes,
                damage = damage,
                foundationQuality = foundationQuality,
                nextStrikeTick = Find.TickManager.TicksGame + WarningTicks
            });

            float dissonance = pawn.GetStatValue(CVG_DefOf.CVG_CelestialDissonance);
            SoundCues.OnCamera("CVG_Cue_TribulationWarning");
            Find.LetterStack.ReceiveLetter("CVG_Tribulation_LetterLabel".Translate(),
                "CVG_Tribulation_LetterText".Translate(pawn.Named("PAWN"), target.LabelCap, strikes, damage.ToString("0.#"),
                    dissonance.ToString("0.##"),
                    CultivationDifficulty.Current.tribulationCanKill ? "CVG_Tribulation_CanKill".Translate() : "CVG_Tribulation_CannotKill".Translate()),
                LetterDefOf.ThreatBig, pawn);
            Find.CameraDriver.shaker.DoShake(1f);
            GatherWitnesses(pawn);
        }

        /// <summary>
        /// La colonia sale a mirar, como en un parto: los colonos libres van a una celda a distancia segura con vista al que
        /// enfrenta el cielo. No se acercan más: el rayo no elige.
        /// </summary>
        private void GatherWitnesses(Pawn tribulant)
        {
            if (tribulant.Faction != Faction.OfPlayer) return;
            foreach (var p in map.mapPawns.FreeColonistsSpawned)
            {
                if (p == tribulant || p.Downed || p.Drafted || p.InMentalState || !p.Awake() || p.InBed() || p.IsBurning()) continue;
                if (HealthAIUtility.ShouldSeekMedicalRest(p) || p.CurJob?.def == CVG_DefOf.CVG_WatchTribulation) continue;
                if (!CellFinder.TryFindRandomCellNear(tribulant.Position, map, (int)SafeDistanceMax, c =>
                        c.Standable(map) && !c.IsForbidden(p) && c.DistanceTo(tribulant.Position) >= SafeDistanceMin
                        && GenSight.LineOfSight(c, tribulant.Position, map) && p.CanReserveAndReach(c, PathEndMode.OnCell, Danger.Some),
                        out var cell))
                    continue;
                var job = JobMaker.MakeJob(CVG_DefOf.CVG_WatchTribulation, tribulant, cell);
                job.expiryInterval = 6000;
                job.checkOverrideOnExpire = true;
                p.jobs.StartJob(job, JobCondition.InterruptForced, resumeCurJobAfterwards: true);
            }
        }

        /// <summary>Quienes la vieron: los que salieron a mirar y los que estaban cerca con vista.</summary>
        public List<Pawn> Witnesses(Pawn tribulant)
        {
            var list = new List<Pawn>();
            foreach (var p in map.mapPawns.FreeColonistsSpawned)
                if (p != tribulant && (JobDriver_WatchTribulation.IsWatching(p, tribulant)
                    || (p.Position.DistanceTo(tribulant.Position) <= 20f && p.Awake() && GenSight.LineOfSight(p.Position, tribulant.Position, map))))
                    list.Add(p);
            return list;
        }

        /// <summary>Testigos en su lugar mirando a <paramref name="tribulant"/>.</summary>
        public int WitnessCount(Pawn tribulant)
        {
            int n = 0;
            foreach (var p in map.mapPawns.FreeColonistsSpawned)
                if (JobDriver_WatchTribulation.IsWatching(p, tribulant)) n++;
            return n;
        }

        /// <summary>Multiplicador del daño del rayo según los testigos presentes.</summary>
        public float WitnessFactor(Pawn tribulant) => UnityEngine.Mathf.Max(MinWitnessFactor, 1f - PerWitness * WitnessCount(tribulant));

        public override void MapComponentTick()
        {
            if (active.Count == 0) return;
            int now = Find.TickManager.TicksGame;
            for (int i = active.Count - 1; i >= 0; i--)
            {
                var t = active[i];
                if (t.pawn == null || t.pawn.Dead || t.pawn.MapHeld != map)
                {
                    active.RemoveAt(i);
                    Finish(t);
                    continue;
                }
                if (now < t.nextStrikeTick) continue;

                bool canKill = CultivationDifficulty.Current.tribulationCanKill;
                if (t.strikesLeft <= 0 || (!canKill && t.pawn.Downed))
                {
                    active.RemoveAt(i);
                    Finish(t);
                    continue;
                }

                Strike(t, canKill);
                t.strikesLeft--;
                t.nextStrikeTick = now + t.target.tribulationStrikeInterval.RandomInRange;
            }
        }

        private void Strike(Tribulation t, bool canKill)
        {
            var pawn = t.pawn;
            var cell = pawn.PositionHeld;
            map.weatherManager.eventHandler.AddEvent(new WeatherEvent_TribulationStrike(map, cell));

            var dinfo = new DamageInfo(DamageDefOf.Burn, t.damage * WitnessFactor(pawn), armorPenetration: 1f);
            if (!canKill)
            {
                var part = NonVitalPart(pawn, t.damage);
                if (part == null) return; // nada que quemar sin arriesgar la vida
                dinfo.SetHitPart(part);
            }
            pawn.TakeDamage(dinfo);
        }

        /// <summary>Parte exterior que el rayo puede quemar sin matar (ni cabeza, cuello, torso ni órganos vitales).</summary>
        private static BodyPartRecord NonVitalPart(Pawn pawn, float damage)
        {
            var body = pawn.RaceProps.body;
            return pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Outside)
                .Where(p => p != body.corePart && !ContainsVital(p) && pawn.health.hediffSet.GetPartHealth(p) > damage + 1f)
                .RandomElementByWeightWithFallback(p => p.coverageAbsWithChildren);
        }

        private static bool ContainsVital(BodyPartRecord part)
        {
            var tags = part.def.tags;
            if (tags != null && (tags.Contains(BodyPartTagDefOf.ConsciousnessSource) || tags.Contains(BodyPartTagDefOf.BloodPumpingSource)
                || tags.Contains(BodyPartTagDefOf.BreathingSource) || tags.Contains(BodyPartTagDefOf.BreathingPathway)
                || tags.Contains(BodyPartTagDefOf.BloodFiltrationLiver) || tags.Contains(BodyPartTagDefOf.MetabolismSource)))
                return true;
            for (int i = 0; i < part.parts.Count; i++)
                if (ContainsVital(part.parts[i])) return true;
            return false;
        }

        private static void Finish(Tribulation t)
        {
            var pawn = t.pawn;
            if (pawn == null) return;
            if (pawn.Dead)
            {
                CultivationLife.Notify_DiedInTribulation(pawn);
                if (pawn.Faction == Faction.OfPlayer)
                    Find.LetterStack.ReceiveLetter("CVG_Tribulation_DeathLabel".Translate(pawn.LabelShortCap),
                        "CVG_Tribulation_DeathText".Translate(pawn.Named("PAWN"), t.target.LabelCap), LetterDefOf.Death, pawn.Corpse);
                return;
            }
            if (t.strikesLeft <= 0 && !pawn.Downed && pawn.MapHeld != null)
            {
                CultivationLife.Notify_TribulationSurvived(pawn, For(pawn.MapHeld)?.Witnesses(pawn));
                BreakthroughUtility.Ascend(pawn, t.target, t.foundationQuality);
                return;
            }
            BreakthroughUtility.Fail(pawn, t.target, "CVG_Tribulation_FailText".Translate(pawn.Named("PAWN"), t.target.LabelCap));
        }

        /// <summary>Acción de debug: tribulación hacia el reino siguiente (o el actual si ya está en el último).</summary>
        public void DebugBegin(Pawn pawn)
        {
            var comp = CompCultivator.For(pawn);
            if (comp == null || !comp.Realm.EnsureStarted()) return;
            Begin(pawn, comp.Realm.NextRealm ?? comp.Realm.Realm);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref active, "active", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                active ??= new List<Tribulation>();
                active.RemoveAll(t => t.pawn == null || t.target == null);
            }
        }

        public class Tribulation : IExposable
        {
            public Pawn pawn;
            public CultivationRealmDef target;
            public int strikesLeft;
            public float damage;
            public float foundationQuality = 1f;
            public int nextStrikeTick;

            public void ExposeData()
            {
                Scribe_References.Look(ref pawn, "pawn");
                Scribe_Defs.Look(ref target, "target");
                Scribe_Values.Look(ref strikesLeft, "strikesLeft");
                Scribe_Values.Look(ref damage, "damage");
                Scribe_Values.Look(ref foundationQuality, "foundationQuality", 1f);
                Scribe_Values.Look(ref nextStrikeTick, "nextStrikeTick");
            }
        }
    }
}
