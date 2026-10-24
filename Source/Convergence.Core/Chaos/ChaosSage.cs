using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Convergence
{
    /// <summary>
    /// El sabio del caos (plan de fusión, 2026-10-23): cuando despierta el monolito de Anomaly llega un viejo cultivador con
    /// el signo de pregunta encima, como los que esperan que se les hable en vanilla. Se manda a alguien a hablar con él
    /// (clic derecho, como al creepjoiner): explica que el vacío es el caos primordial (Hundun) que se filtró con la Convergencia
    /// y pide cuidado. Se queda con la colonia hasta que pasa la primera amenaza de Anomaly, pelea a su lado y al irse deja
    /// una muestra de lo que da el caos (núcleo de caos y un talismán). Es un <see cref="MentorDef"/> con commonality 0 (no
    /// sale como maestro); la llegada y la despedida reusan las del maestro.
    /// </summary>
    public class GameComponent_ChaosSage : GameComponent
    {
        public const int ArrivalDelayTicks = GenDate.TicksPerHour * 8;
        public const int CalmTicksToLeave = GenDate.TicksPerHour * 6;
        public const int MaxStayTicks = GenDate.TicksPerDay * 40;

        private Pawn sage;
        private int arriveTick = -1;
        private int arrivedTick = -1;
        private bool spokenTo;
        private bool threatSeen;
        private int calmSinceTick = -1;
        private bool done;

        public static GameComponent_ChaosSage Instance => Current.Game?.GetComponent<GameComponent_ChaosSage>();

        public Pawn Sage => sage;
        public bool IsSage(Pawn p) => p != null && p == sage && !done;
        public bool WaitingToBeSpokenTo => IsSage(sage) && !spokenTo && sage.Spawned;

        public GameComponent_ChaosSage(Game game)
        {
        }

        private static MentorDef SageDef => DefDatabase<MentorDef>.GetNamedSilentFail("CVG_Mentor_ChaosSage");

        /// <summary>Lo llama el parche del nivel del monolito: el monolito despertó.</summary>
        public void Notify_MonolithAwoken()
        {
            if (done || arriveTick >= 0 || sage != null || SageDef == null) return;
            arriveTick = Find.TickManager.TicksGame + ArrivalDelayTicks;
        }

        public override void GameComponentTick()
        {
            if (done || Find.TickManager.TicksGame % 250 != 0) return;
            int now = Find.TickManager.TicksGame;
            if (sage == null)
            {
                if (arriveTick >= 0 && now >= arriveTick) Arrive();
                return;
            }
            if (sage.Dead || sage.Destroyed)
            {
                done = true;
                return;
            }
            var map = sage.MapHeld;
            if (map == null) return;
            bool threat = map.mapPawns.AllPawnsSpawned.Any(p => p.RaceProps.IsAnomalyEntity && !p.Dead && !p.Downed && p.HostileTo(Faction.OfPlayer));
            if (threat)
            {
                threatSeen = true;
                calmSinceTick = -1;
            }
            else if (threatSeen)
            {
                if (calmSinceTick < 0) calmSinceTick = now;
                else if (now - calmSinceTick >= CalmTicksToLeave) Leave();
            }
            if (!done && now - arrivedTick >= MaxStayTicks) Leave();
        }

        private void Arrive()
        {
            var map = Find.AnyPlayerHomeMap;
            if (map == null) return;
            var def = SageDef;
            var pawn = MentorUtility.Generate(def);
            if (pawn == null)
            {
                done = true;
                return;
            }
            if (!CellFinder.TryFindRandomEdgeCellWith(c => c.Standable(map) && map.reachability.CanReachColony(c), map,
                    CellFinder.EdgeRoadChance_Neutral, out var cell))
                cell = DropCellFinder.RandomDropSpot(map);
            GenSpawn.Spawn(pawn, cell, map);
            pawn.SetFaction(Faction.OfPlayer);
            sage = pawn;
            arrivedTick = Find.TickManager.TicksGame;
            SoundCues.OnCamera("CVG_Cue_MentorArrival");
            Find.LetterStack.ReceiveLetter("CVG_ChaosSage_ArrivalLabel".Translate(pawn.LabelShortCap),
                def.arrivalWalkerText.Formatted(pawn.Named("PAWN")).Resolve(), LetterDefOf.NeutralEvent, pawn);
        }

        /// <summary>Un colono habló con él: la charla del caos primordial.</summary>
        public void SpokenTo(Pawn speaker)
        {
            if (spokenTo || !IsSage(sage)) return;
            spokenTo = true;
            var def = SageDef;
            var node = new DiaNode("CVG_ChaosSage_Talk".Translate(sage.Named("PAWN"), speaker.Named("SPEAKER")).Resolve());
            node.options.Add(new DiaOption("CVG_ChaosSage_TalkClose".Translate()) { resolveTree = true });
            Find.WindowStack.Add(new Dialog_NodeTree(node, delayInteractivity: true, radioMode: false, def?.LabelCap ?? (TaggedString)""));
            DaoComprehension.Gain(speaker, 20f, DaoSource.Studying);
        }

        private void Leave()
        {
            if (done || sage == null) return;
            done = true;
            var def = SageDef;
            var map = sage.MapHeld ?? Find.AnyPlayerHomeMap;
            Thing first = null;
            if (map != null)
            {
                var near = sage.Spawned ? sage : null;
                var pos = sage.PositionHeld.IsValid && sage.MapHeld != null ? sage.PositionHeld : DropCellFinder.TradeDropSpot(map);
                foreach (var entry in def.farewellItems)
                {
                    var t = ThingMaker.MakeThing(entry.thingDef);
                    t.stackCount = Mathf.Max(1, entry.count);
                    GenPlace.TryPlaceThing(t, pos, map, ThingPlaceMode.Near);
                    first ??= t;
                }
                var talisman = def.farewellChoice.RandomElementWithFallback();
                if (talisman != null) GenPlace.TryPlaceThing(ThingMaker.MakeThing(talisman), pos, map, ThingPlaceMode.Near);
            }
            if (sage.Spawned)
            {
                FleckMaker.ThrowLightningGlow(sage.DrawPos, sage.Map, 3f);
                sage.jobs?.StopAll();
                var leaveFaction = Find.FactionManager.RandomNonHostileFaction(allowHidden: false, allowDefeated: false, allowNonHumanlike: false);
                if (leaveFaction != null)
                {
                    sage.SetFaction(leaveFaction);
                    Verse.AI.Group.LordMaker.MakeNewLord(leaveFaction, new Verse.AI.Group.LordJob_ExitMapBest(LocomotionUrgency.Walk, canDig: false),
                        sage.Map, new List<Pawn> { sage });
                }
            }
            SoundCues.OnCamera("CVG_Cue_MentorFarewell");
            Find.LetterStack.ReceiveLetter("CVG_ChaosSage_FarewellLabel".Translate(sage.LabelShortCap),
                def.farewellText.Formatted(sage.Named("PAWN")).Resolve(), LetterDefOf.PositiveEvent,
                first != null ? new LookTargets(first) : LookTargets.Invalid);
        }

        public override void ExposeData()
        {
            Scribe_References.Look(ref sage, "sage", saveDestroyedThings: true);
            Scribe_Values.Look(ref arriveTick, "arriveTick", -1);
            Scribe_Values.Look(ref arrivedTick, "arrivedTick", -1);
            Scribe_Values.Look(ref spokenTo, "spokenTo");
            Scribe_Values.Look(ref threatSeen, "threatSeen");
            Scribe_Values.Look(ref calmSinceTick, "calmSinceTick", -1);
            Scribe_Values.Look(ref done, "done");
        }
    }

    /// <summary>El monolito despertó (nivel 1 o más): llega el sabio.</summary>
    [HarmonyPatch(typeof(GameComponent_Anomaly), "Notify_LevelChanged")]
    public static class Patch_Anomaly_LevelChanged_ChaosSage
    {
        public static void Postfix(GameComponent_Anomaly __instance)
        {
            if (__instance.Level >= 1) GameComponent_ChaosSage.Instance?.Notify_MonolithAwoken();
        }
    }

    /// <summary>El signo de pregunta sobre el sabio mientras nadie le habló (como los huéspedes que esperan en vanilla).</summary>
    public class MapComponent_ChaosSageOverlay : MapComponent
    {
        public MapComponent_ChaosSageOverlay(Map map) : base(map)
        {
        }

        public override void MapComponentUpdate()
        {
            var sage = GameComponent_ChaosSage.Instance;
            if (sage != null && sage.WaitingToBeSpokenTo && sage.Sage.Map == map)
                map.overlayDrawer.DrawOverlay(sage.Sage, OverlayTypes.QuestionMark);
        }
    }

    /// <summary>"Hablar con el sabio": clic derecho con un colono elegido, como con el creepjoiner.</summary>
    public class FloatMenuOptionProvider_TalkChaosSage : FloatMenuOptionProvider
    {
        protected override bool Drafted => true;
        protected override bool Undrafted => true;
        protected override bool Multiselect => false;

        protected override FloatMenuOption GetSingleOptionFor(Pawn clickedPawn, FloatMenuContext context)
        {
            var comp = GameComponent_ChaosSage.Instance;
            var pawn = context.FirstSelectedPawn;
            if (comp == null || !comp.WaitingToBeSpokenTo || clickedPawn != comp.Sage || pawn == null || pawn == clickedPawn) return null;
            if (!pawn.CanReach(clickedPawn, PathEndMode.Touch, Danger.Deadly))
                return new FloatMenuOption("CannotTalkTo".Translate(clickedPawn) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
            if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking))
                return new FloatMenuOption("CannotTalkTo".Translate(clickedPawn) + ": " + "Incapable".Translate().CapitalizeFirst(), null);
            return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("TalkTo".Translate(clickedPawn), () =>
            {
                var job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("CVG_TalkChaosSage"), clickedPawn);
                job.playerForced = true;
                pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            }), pawn, clickedPawn);
        }
    }

    public class JobDriver_TalkChaosSage : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed) => pawn.Reserve(TargetA, job, 1, -1, null, errorOnFailed);

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.Wait(120).WithProgressBarToilDelay(TargetIndex.A);
            yield return Toils_General.Do(() => GameComponent_ChaosSage.Instance?.SpokenTo(pawn));
        }
    }
}
