using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Convergence
{
    /// <summary>
    /// Avance de reino de un camino: requisitos, probabilidad, gizmo "Intentar avance" y resultado (éxito, fallo o
    /// tribulación). Probabilidad = CVG_BreakthroughChance × dificultad del reino × calidad de la raíz (camino del qi)
    /// × concentración de qi del lugar (si el camino la usa). Al subir se guarda la calidad del fundamento.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class BreakthroughUtility
    {
        private const float MinChance = 0.02f;
        private const float MaxChance = 0.95f;

        private static readonly Texture2D Icon = ContentFinder<Texture2D>.Get("UI/Commands/CVG_Breakthrough");

        /// <summary>Si el colono puede empezar el avance de <paramref name="tracker"/> ahora; si no, el motivo.</summary>
        public static bool CanAttempt(CompCultivator comp, RealmTracker tracker, out string reason)
        {
            reason = null;
            var pawn = comp.Pawn;
            if (!comp.CanFollow(tracker.Path) || !tracker.Bottleneck) { reason = "CVG_Breakthrough_NotBottleneck".Translate(); return false; }
            var target = tracker.NextRealm;
            if (target == null) { reason = "CVG_Breakthrough_PeakRealm".Translate(); return false; }
            if (pawn.Downed || pawn.InMentalState) { reason = "CVG_Breakthrough_Incapable".Translate(pawn.Named("PAWN")); return false; }
            if (pawn.Map == null) { reason = "CVG_Breakthrough_NoMap".Translate(); return false; }
            if (pawn.CurJobDef == CVG_DefOf.CVG_Breakthrough) { reason = "CVG_Breakthrough_InProgress".Translate(pawn.Named("PAWN")); return false; }
            if (MapComponent_Tribulation.For(pawn.Map)?.IsUndergoing(pawn) ?? false) { reason = "CVG_Breakthrough_InTribulation".Translate(); return false; }
            // Nada de avances ni tribulaciones en combate.
            if (CultivatorAI.InDanger(pawn)) { reason = "CVG_Breakthrough_InCombat".Translate(pawn.Named("PAWN")); return false; }
            int cooldown = tracker.BreakthroughCooldownTicksLeft;
            if (cooldown > 0) { reason = "CVG_Breakthrough_Cooldown".Translate(cooldown.ToStringTicksToPeriod()); return false; }
            var res = tracker.Resource;
            if (!res.IsFull)
            {
                reason = "CVG_Breakthrough_ResourceNotFull".Translate(res.Current.ToString("F0"), res.Max.ToString("F0"), res.Label);
                return false;
            }
            if (target.breakthroughRequirements != null)
                foreach (var req in target.breakthroughRequirements)
                    if (!req.Met(pawn, tracker, out reason)) return false;
            if (target.breakthroughItemRequired && target.breakthroughItem != null && FindItem(pawn, target) == null)
            {
                reason = "CVG_Breakthrough_ItemRequired".Translate(target.breakthroughItem.label);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Probabilidad de éxito en <paramref name="cell"/>, sumando <paramref name="extraOffset"/> al stat
        /// (positivo: la píldora que va a tomar; negativo: descontar ayudas ya absorbidas).
        /// </summary>
        public static float Chance(Pawn pawn, RealmTracker tracker, CultivationRealmDef target, IntVec3 cell, Map map,
            float extraOffset = 0f, StringBuilder explanation = null)
        {
            var comp = CompCultivator.For(pawn);
            var path = tracker.Path;
            float stat = Mathf.Max(0f, pawn.GetStatValue(CVG_DefOf.CVG_BreakthroughChance) + extraOffset);
            float realmFactor = target?.breakthroughChanceFactor ?? 1f;
            float rootFactor = path.requiresSpiritRoot ? comp?.Root?.grade?.breakthroughFactor ?? 1f : 1f;
            float concentration = -1f, placeFactor = 1f;
            if (path.usesQiConcentration)
            {
                concentration = map != null && cell.IsValid ? QiConcentrationUtility.ConcentrationAt(cell, map) : QiConcentrationUtility.Ambient;
                placeFactor = QiConcentrationUtility.BreakthroughFactor(concentration);
            }
            float chance = Mathf.Clamp(stat * realmFactor * rootFactor * placeFactor, MinChance, MaxChance);

            if (explanation != null)
            {
                explanation.AppendLine("CVG_Breakthrough_ChanceBase".Translate(stat.ToStringPercent()));
                if (extraOffset > 0f) explanation.AppendLine("  " + "CVG_Breakthrough_ChanceItem".Translate(extraOffset.ToStringPercent()));
                explanation.AppendLine("CVG_Breakthrough_ChanceRealm".Translate(target?.LabelCap ?? "", realmFactor.ToStringPercent()));
                if (path.requiresSpiritRoot) explanation.AppendLine("CVG_Breakthrough_ChanceRoot".Translate(rootFactor.ToStringPercent()));
                if (path.usesQiConcentration)
                    explanation.AppendLine("CVG_Breakthrough_ChancePlace".Translate(concentration.ToString("0.#"), placeFactor.ToStringPercent()));
                explanation.Append("CVG_Breakthrough_ChanceTotal".Translate(chance.ToStringPercent()).Resolve().AsTipTitle());
            }
            return chance;
        }

        /// <summary>Suma de <c>CVG_BreakthroughChance</c> que da el hediff de un objeto ingerible (p. ej. la píldora).</summary>
        public static float ItemChanceOffset(ThingDef item)
        {
            var doers = item?.ingestible?.outcomeDoers;
            if (doers == null) return 0f;
            float total = 0f;
            foreach (var doer in doers.OfType<IngestionOutcomeDoer_GiveHediff>())
            {
                var stage = doer.hediffDef?.stages?.FirstOrDefault();
                if (stage?.statOffsets != null) total += stage.statOffsets.GetStatOffsetFromList(CVG_DefOf.CVG_BreakthroughChance);
            }
            return total;
        }

        /// <summary>Si ya tiene absorbida una ayuda (no hace falta tomar otra píldora).</summary>
        public static bool HasAid(Pawn pawn) =>
            pawn.health.hediffSet.hediffs.Any(h => h.def.HasModExtension<DefModExtension_BreakthroughAid>());

        /// <summary>Suma de <c>CVG_BreakthroughChance</c> de las ayudas absorbidas.</summary>
        public static float AidChanceOffset(Pawn pawn)
        {
            float total = 0f;
            foreach (var h in pawn.health.hediffSet.hediffs)
                if (h.def.HasModExtension<DefModExtension_BreakthroughAid>() && h.CurStage?.statOffsets != null)
                    total += h.CurStage.statOffsets.GetStatOffsetFromList(CVG_DefOf.CVG_BreakthroughChance);
            return total;
        }

        /// <summary>Cojín de cultivo libre más cercano; null si no hay (entonces el retiro es donde está).</summary>
        public static Thing FindSpot(Pawn pawn)
        {
            if (pawn.Map == null) return null;
            return GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, MeditationCultivation.AllSpots(pawn.Map),
                PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Some), 9999f,
                t => t.Faction == pawn.Faction && !t.IsForbidden(pawn) && !t.IsBurning()
                     && pawn.CanReserveSittableOrSpot(t.Position, t));
        }

        /// <summary>Objeto de avance (píldora) más cercano que el colono puede tomar, si no tiene ya una absorbida.</summary>
        public static Thing FindItem(Pawn pawn, CultivationRealmDef target)
        {
            var def = target?.breakthroughItem;
            if (def == null || pawn.Map == null || HasAid(pawn)) return null;
            return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(def), PathEndMode.ClosestTouch,
                TraverseParms.For(pawn, Danger.Some), 9999f, t => !t.IsForbidden(pawn) && pawn.CanReserve(t, 1, 1));
        }

        public static Command MakeGizmo(CompCultivator comp, RealmTracker tracker)
        {
            var pawn = comp.Pawn;
            var target = tracker.NextRealm;
            var cmd = new Command_Action
            {
                defaultLabel = "CVG_Breakthrough_Label".Translate(),
                icon = Icon,
                Order = -99f,
                action = () => StartBreakthrough(pawn, tracker.Path)
            };

            var desc = new StringBuilder();
            desc.AppendLine(target != null
                ? "CVG_Breakthrough_Desc".Translate(pawn.Named("PAWN"), target.LabelCap, target.breakthroughHours.ToString("0.#"), tracker.Resource.Label).Resolve()
                : "CVG_Breakthrough_PeakRealm".Translate().Resolve());

            if (target != null && pawn.Map != null)
            {
                var spot = FindSpot(pawn);
                var item = FindItem(pawn, target);
                float itemOffset = item != null ? ItemChanceOffset(item.def) : 0f;
                desc.AppendLine();
                desc.AppendLine(spot != null ? "CVG_Breakthrough_WillUseSpot".Translate(spot.LabelShort).Resolve() : "CVG_Breakthrough_NoSpot".Translate().Resolve());
                if (item != null) desc.AppendLine("CVG_Breakthrough_WillTakeItem".Translate(item.def.label, itemOffset.ToStringPercent()).Resolve());
                else if (target.breakthroughItem != null && !HasAid(pawn) && !target.breakthroughItemRequired)
                    desc.AppendLine("CVG_Breakthrough_ItemMissing".Translate(target.breakthroughItem.label).Resolve());
                if (itemOffset > 0f || HasAid(pawn)) desc.AppendLine("CVG_Breakthrough_AidFoundationWarning".Translate().Resolve());
                if (target.tribulation) desc.AppendLine("CVG_Breakthrough_TribulationWarning".Translate().Resolve());
                desc.AppendLine();
                Chance(pawn, tracker, target, spot?.Position ?? pawn.Position, pawn.Map, itemOffset, desc);
            }
            cmd.defaultDesc = desc.ToString().TrimEnd();

            if (!CanAttempt(comp, tracker, out string reason)) cmd.Disable(reason);
            return cmd;
        }

        /// <summary>Ordena el trabajo de avance del camino: (tomar la píldora) → ir al cojín → retiro.</summary>
        public static void StartBreakthrough(Pawn pawn, CultivationPathDef path)
        {
            var comp = CompCultivator.For(pawn);
            var tracker = comp?.TrackerFor(path);
            if (tracker == null || !CanAttempt(comp, tracker, out _)) return;
            var target = tracker.NextRealm;
            var spot = FindSpot(pawn);
            var item = FindItem(pawn, target);
            comp.BreakthroughPath = path;
            var job = JobMaker.MakeJob(CVG_DefOf.CVG_Breakthrough, spot != null ? (LocalTargetInfo)spot : pawn.Position, item);
            if (item != null) job.count = 1;
            pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        }

        /// <summary>
        /// Fin del retiro: tirada. Éxito → sube (o tribulación primero) con la calidad de fundamento calculada.
        /// Fallo → desviación de qi y enfriamiento.
        /// </summary>
        public static void ResolveRetreat(Pawn pawn, RealmTracker tracker)
        {
            var target = tracker?.NextRealm;
            if (target == null) return;
            float aid = AidChanceOffset(pawn);
            float chance = Chance(pawn, tracker, target, pawn.Position, pawn.Map);
            float chanceWithoutAids = Chance(pawn, tracker, target, pawn.Position, pawn.Map, -aid);
            float quality = tracker.Path.FoundationQuality(chanceWithoutAids, aid > 0f);
            ConsumeAids(pawn);
            tracker.Resource.Drain();
            SectEvents.Record(SectEvents.Retreat, pawn);

            if (!Rand.Chance(chance))
            {
                Fail(pawn, target, "CVG_Breakthrough_FailText".Translate(pawn.Named("PAWN"), target.LabelCap, chance.ToStringPercent()));
                return;
            }

            if (target.tribulation && pawn.MapHeld != null)
                MapComponent_Tribulation.For(pawn.MapHeld).Begin(pawn, target, quality);
            else
                Ascend(pawn, target, quality);
        }

        /// <summary>Sube al reino <paramref name="target"/> (de su camino) con carta y efecto visual.</summary>
        public static void Ascend(Pawn pawn, CultivationRealmDef target, float quality)
        {
            var tracker = CompCultivator.For(pawn)?.TrackerFor(target?.path, create: true);
            if (tracker == null) return;
            var before = RealmBonusUtility.Snapshot(pawn, out float healthBefore);
            if (tracker.Realm != target) tracker.AdvanceTo(target, quality);
            tracker.Resource.Fill();
            ThrowAscendEffect(pawn);
            SectEvents.Record(SectEvents.Ascended, pawn);
            CultivationLife.Notify_Ascended(pawn);

            if (pawn.Faction == Faction.OfPlayer)
            {
                var text = "CVG_Breakthrough_SuccessText".Translate(pawn.Named("PAWN"), target.LabelCap, target.description,
                    Hediff_CultivationRealm.FoundationQualityLabel(quality), quality.ToStringPercent());
                string gains = RealmBonusUtility.CompareLines(pawn, before, healthBefore);
                if (!gains.NullOrEmpty()) text += "\n\n" + "CVG_Bonus_LetterHeader".Translate() + "\n" + gains;
                SoundCues.OnCamera("CVG_Cue_BreakthroughSuccess");
                Find.LetterStack.ReceiveLetter("CVG_Breakthrough_SuccessLabel".Translate(pawn.LabelShortCap), text,
                    LetterDefOf.PositiveEvent, pawn);
            }
        }

        /// <summary>Avance fallido: hediff de desviación de qi y enfriamiento del reintento.</summary>
        public static void Fail(Pawn pawn, CultivationRealmDef target, TaggedString text)
        {
            var tracker = CompCultivator.For(pawn)?.TrackerFor(target?.path);
            if (tracker == null) return;
            if (target.breakthroughFailureHediff != null && !pawn.Dead)
                pawn.health.AddHediff(target.breakthroughFailureHediff);
            tracker.StartBreakthroughCooldown(Mathf.RoundToInt(target.breakthroughCooldownDays * GenDate.TicksPerDay));
            if (!pawn.Dead) CultivationLife.Notify_BreakthroughFailed(pawn);

            if (pawn.Faction == Faction.OfPlayer)
                SoundCues.OnCamera("CVG_Cue_BreakthroughFail");
            if (pawn.Faction == Faction.OfPlayer)
                Find.LetterStack.ReceiveLetter("CVG_Breakthrough_FailLabel".Translate(pawn.LabelShortCap), text,
                    LetterDefOf.NegativeEvent, pawn);
        }

        /// <summary>Las ayudas absorbidas (píldoras) se gastan en el intento.</summary>
        public static void ConsumeAids(Pawn pawn)
        {
            var aids = pawn.health.hediffSet.hediffs.Where(h => h.def.HasModExtension<DefModExtension_BreakthroughAid>()).ToList();
            foreach (var h in aids) pawn.health.RemoveHediff(h);
        }

        public static void ThrowAscendEffect(Pawn pawn)
        {
            var map = pawn.MapHeld;
            if (map == null) return;
            var loc = pawn.DrawPos;
            FleckMaker.Static(loc, map, FleckDefOf.PsycastAreaEffect, 6f);
            FleckMaker.ThrowLightningGlow(loc, map, 2.5f);
            for (int i = 0; i < 6; i++) FleckMaker.ThrowMicroSparks(loc, map);
        }
    }
}
