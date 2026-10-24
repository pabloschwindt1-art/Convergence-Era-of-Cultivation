using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using RimWorld.Planet;
using Verse;

namespace Convergence
{
    /// <summary>
    /// El Maestro: a los pocos días de empezar llega la misión "un viejo sabio busca refugio" (<see cref="QuestPart_MentorRefuge"/>);
    /// aceptarla desata la Convergencia y el Maestro cae herido. Se une a la colonia como un residente muy poderoso y sin
    /// defectos, da sus lecciones una a una (objetivos simples con recompensa), protege de la primera incursión de cultivadores,
    /// quienes lo persiguen pueden venir a buscarlo y, al acabar su estancia, se va caminando y deja el fragmento de jade.
    /// Estado de la partida en este GameComponent; los datos del Maestro, en <see cref="MentorDef"/>.
    /// </summary>
    public class GameComponent_Mentor : GameComponent
    {
        private const int CheckInterval = 250;
        /// <summary>Días desde el inicio de la partida hasta que llega la misión.</summary>
        private static readonly IntRange OfferDays = new IntRange(2, 4);
        /// <summary>Probabilidad de que quienes lo persiguen vengan a buscarlo durante su estancia.</summary>
        private const float HuntersChance = 0.6f;

        private MentorDef def;
        private Pawn mentor;
        private int offerTick = -1;
        private bool offered;
        private int huntTick = -1;
        private bool wasDowned;
        private int leaveTick = -1;
        private int lessonIndex;
        private bool lessonStarted;
        private int lessonBaseline;
        private Dictionary<int, int> lessonBaselines = new Dictionary<int, int>();
        private int lessonStartTick = -1;
        private int nextLessonTick = -1;
        /// <summary>Mínimo de días entre que empieza una lección y se da por cumplida, y de espera antes de la siguiente.</summary>
        private const float MinLessonDays = 2f, PauseBetweenLessonsDays = 1f;
        private bool protectionUsed;
        private bool finished;

        public static GameComponent_Mentor Instance => Current.Game?.GetComponent<GameComponent_Mentor>();

        public GameComponent_Mentor(Game game)
        {
        }

        public MentorDef Def => def;
        public Pawn Mentor => mentor;
        public bool Present => mentor != null && !finished;
        public int LeaveTicksLeft => leaveTick < 0 ? 0 : Mathf.Max(0, leaveTick - Find.TickManager.TicksGame);
        public MentorLesson CurrentLesson => def != null && lessonIndex < def.lessons.Count ? def.lessons[lessonIndex] : null;
        public int LessonNumber => lessonIndex + 1;

        /// <summary>Progreso de la lección actual medido desde que empezó (técnicas nuevas, etapas avanzadas).</summary>
        public string LessonProgress
        {
            get
            {
                var lesson = CurrentLesson;
                if (lesson == null || mentor == null || !lessonStarted) return null;
                return lesson.Progress(mentor, mentor.MapHeld, lessonBaseline, lessonBaselines);
            }
        }

        /// <summary>"Próxima lección en N días" o "el Anciano observa: N días" (vacío si no hay espera).</summary>
        public string LessonWaitText
        {
            get
            {
                if (CurrentLesson == null || mentor == null) return null;
                int now = Find.TickManager.TicksGame;
                if (!lessonStarted && nextLessonTick > now)
                    return "CVG_Mentor_NextLessonIn".Translate((nextLessonTick - now).ToStringTicksToPeriod());
                int watch = lessonStartTick < 0 ? 0 : lessonStartTick + (int)(MinLessonDays * GenDate.TicksPerDay) - now;
                if (lessonStarted && watch > 0)
                    return "CVG_Mentor_Watching".Translate(mentor.Named("PAWN"), watch.ToStringTicksToPeriod());
                return null;
            }
        }

        public bool IsMentor(Pawn pawn) => pawn != null && pawn == mentor && !finished;

        public override void StartedNewGame() => EnsureOffer();

        public override void LoadedGame() => EnsureOffer();

        /// <summary>Programa la misión del sabio (una sola vez por partida).</summary>
        private void EnsureOffer()
        {
            if (offered || offerTick >= 0 || mentor != null || finished) return;
            offerTick = Find.TickManager.TicksGame + OfferDays.RandomInRange * GenDate.TicksPerDay;
        }

        private void Offer(Map map)
        {
            offered = true;
            if (QuestPart_MentorRefuge.AnyOffered() || CVG_DefOf.CVG_MentorRefuge == null) return;
            var slate = new RimWorld.QuestGen.Slate();
            slate.Set("map", map);
            slate.Set("points", StorytellerUtility.DefaultThreatPointsNow(map));
            var quest = QuestUtility.GenerateQuestAndMakeAvailable(CVG_DefOf.CVG_MentorRefuge, slate);
            if (quest != null) QuestUtility.SendLetterQuestAvailable(quest);
        }

        public override void GameComponentTick()
        {
            int now = Find.TickManager.TicksGame;
            if (now % CheckInterval != 0) return;

            if (mentor == null)
            {
                if (!offered && offerTick >= 0 && now >= offerTick)
                {
                    var map = Find.AnyPlayerHomeMap;
                    if (map != null) Offer(map);
                }
                return;
            }
            if (finished) return;
            if (mentor.Dead)
            {
                End(died: true);
                return;
            }
            CheckHurt();
            CheckLesson();
            CheckProtection();
            CheckHunters(now);
            if (now >= leaveTick && mentor.Spawned && !mentor.Downed && !mentor.InMentalState) End(died: false);
            // secuestrado, en caravana o en otro mapa: pasados 5 días de su partida igual se despide y deja el jade en casa
            else if (now >= leaveTick + 5 * GenDate.TicksPerDay && !mentor.Spawned) End(died: false);
        }

        /// <summary>"Mi maestro resultó herido": cada vez que cae, la colonia lo siente.</summary>
        private void CheckHurt()
        {
            bool downed = mentor.Downed;
            if (downed && !wasDowned)
            {
                var thought = DefDatabase<ThoughtDef>.GetNamedSilentFail("CVG_Thought_MentorHurt");
                if (thought != null)
                    foreach (var p in PawnsFinder.AllMaps_FreeColonistsSpawned)
                        if (p != mentor) p.needs?.mood?.thoughts.memories.TryGainMemory(thought, mentor);
            }
            wasDowned = downed;
        }

        /// <summary>Quienes lo perseguían lo encontraron: una incursión enemiga que viene por él (una vez).</summary>
        private void CheckHunters(int now)
        {
            if (huntTick < 0 || now < huntTick || !mentor.Spawned) return;
            huntTick = -1;
            var map = mentor.Map;
            var faction = Find.FactionManager.RandomEnemyFaction(allowHidden: false, allowDefeated: false, allowNonHumanlike: false);
            if (faction == null || !map.IsPlayerHome) return;
            var parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, map);
            parms.faction = faction;
            parms.forced = true;
            parms.customLetterLabel = "CVG_Mentor_HuntersLabel".Translate(mentor.LabelShortCap);
            parms.customLetterText = "CVG_Mentor_HuntersText".Translate(mentor.Named("PAWN"), faction.Named("FACTION"));
            IncidentDefOf.RaidEnemy.Worker.TryExecute(parms);
        }

        /// <summary>Genera y trae al Maestro. <paramref name="wounded"/> null = según la probabilidad del Def.</summary>
        public Pawn Arrive(Map map, MentorDef mentorDef, bool? wounded)
        {
            if (mentorDef == null || map == null) return null;
            bool hurt = wounded ?? Rand.Chance(mentorDef.woundedChance);
            var pawn = MentorUtility.Generate(mentorDef);
            if (pawn == null) return null;

            IntVec3 cell;
            if (hurt) cell = DropCellFinder.RandomDropSpot(map);
            else if (!CellFinder.TryFindRandomEdgeCellWith(c => c.Standable(map) && map.reachability.CanReachColony(c), map,
                         CellFinder.EdgeRoadChance_Neutral, out cell))
                cell = DropCellFinder.RandomDropSpot(map);

            GenSpawn.Spawn(pawn, cell, map);
            pawn.SetFaction(Faction.OfPlayer);
            if (hurt)
            {
                FleckMaker.ThrowLightningGlow(pawn.DrawPos, map, 3f);
                FleckMaker.Static(pawn.Position, map, FleckDefOf.ExplosionFlash, 4f);
                // La ropa se saca antes de las heridas y se vuelve a poner después: DamageUntilDowned pega con penetración 999 y
                // destruía la túnica y el pantalón (el Maestro llegaba desnudo, visto por el usuario).
                var worn = pawn.apparel?.WornApparel.ToList() ?? new List<Apparel>();
                foreach (var a in worn) pawn.apparel.Remove(a);
                HealthUtility.DamageUntilDowned(pawn, allowBleedingWounds: false);
                foreach (var a in worn) if (!a.Destroyed) pawn.apparel.Wear(a, dropReplacedApparel: false);
            }

            // Lo primero que enseña: la respiración del qi (los proyectos que trae se completan).
            foreach (var proj in mentorDef.teachesResearch)
                if (proj != null && !proj.IsFinished)
                {
                    Find.ResearchManager.FinishProject(proj, doCompletionDialog: false, null, doCompletionLetter: false);
                    GameComponent_Dao.Instance?.MarkAnnounced(proj);
                }

            def = mentorDef;
            mentor = pawn;
            offered = true;
            int stay = Mathf.RoundToInt(mentorDef.stayDays.RandomInRange * GenDate.TicksPerDay);
            leaveTick = Find.TickManager.TicksGame + stay;
            huntTick = Rand.Chance(HuntersChance) ? Find.TickManager.TicksGame + Mathf.RoundToInt(stay * Rand.Range(0.25f, 0.7f)) : -1;
            wasDowned = pawn.Downed;
            lessonIndex = 0;
            lessonStarted = false;
            finished = false;

            string text = (hurt ? mentorDef.arrivalWoundedText : mentorDef.arrivalWalkerText).Formatted(pawn.Named("PAWN")).Resolve();
            text += "\n\n" + "CVG_Mentor_ArrivalRules".Translate(pawn.Named("PAWN"), (LeaveTicksLeft / (float)GenDate.TicksPerDay).ToString("0")).Resolve();
            SoundCues.OnCamera("CVG_Cue_MentorArrival");
            Find.LetterStack.ReceiveLetter("CVG_Mentor_ArrivalLabel".Translate(pawn.LabelShortCap), text,
                hurt ? LetterDefOf.NeutralEvent : LetterDefOf.PositiveEvent, pawn);
            return pawn;
        }

        private void CheckLesson()
        {
            var lesson = CurrentLesson;
            if (lesson == null || !mentor.Spawned) return;
            var map = mentor.Map;
            int now = Find.TickManager.TicksGame;
            if (!lessonStarted)
            {
                if (nextLessonTick >= 0 && now < nextLessonTick) return;
                lessonStarted = true;
                lessonBaseline = lesson.Baseline(mentor, map);
                lessonBaselines = lesson.BaselinePerPawn(mentor) ?? new Dictionary<int, int>();
                lessonStartTick = now;
                MentorUtility.GiveItems(lesson.startItems, mentor);
                Find.LetterStack.ReceiveLetter("CVG_Mentor_LessonLabel".Translate(LessonNumber, lesson.label),
                    "CVG_Mentor_LessonText".Translate(mentor.Named("PAWN"), lesson.label, lesson.description), LetterDefOf.NeutralEvent, mentor);
                return;
            }
            if (lessonStartTick >= 0 && now - lessonStartTick < MinLessonDays * GenDate.TicksPerDay) return;
            if (!lesson.IsComplete(mentor, map, lessonBaseline, lessonBaselines)) return;
            CompleteLesson();
        }

        /// <summary>Lección cumplida: recompensa y pasa a la siguiente.</summary>
        public void CompleteLesson()
        {
            var lesson = CurrentLesson;
            if (lesson == null) return;
            MentorUtility.GiveItems(lesson.rewards, mentor);
            string rewards = lesson.rewards.NullOrEmpty() ? "" : "\n\n" + "CVG_Mentor_Rewards".Translate(
                lesson.rewards.Select(r => r.Label).ToCommaList()).Resolve();
            Messages.Message("CVG_Mentor_LessonDone".Translate(mentor.Named("PAWN"), lesson.label) + rewards, mentor, MessageTypeDefOf.PositiveEvent);
            lessonIndex++;
            lessonStarted = false;
            nextLessonTick = Find.TickManager.TicksGame + (int)(PauseBetweenLessonsDays * GenDate.TicksPerDay);
        }

        /// <summary>Primera incursión con cultivadores en el mapa del Maestro: se pone al frente (una sola vez).</summary>
        private void CheckProtection()
        {
            if (protectionUsed || !mentor.Spawned || def.protectionHediff == null) return;
            var map = mentor.Map;
            var threat = map.mapPawns.AllPawnsSpawned.FirstOrDefault(p =>
                p.RaceProps.Humanlike && !p.Downed && p.HostileTo(Faction.OfPlayer) && SpiritGradeUtility.RealmLevel(p) > 0);
            if (threat == null) return;
            protectionUsed = true;
            mentor.health.AddHediff(def.protectionHediff);
            Find.LetterStack.ReceiveLetter("CVG_Mentor_ProtectionLabel".Translate(mentor.LabelShortCap),
                def.protectionText.Formatted(mentor.Named("PAWN")), LetterDefOf.ThreatBig, new LookTargets(mentor, threat));
        }

        /// <summary>Fin de la estancia: se va (o murió) y deja el fragmento de jade.</summary>
        public void End(bool died)
        {
            if (mentor == null || finished) return;
            finished = true;
            var map = mentor.MapHeld;
            var pos = mentor.PositionHeld;
            if (map == null || !map.IsPlayerHome && Find.AnyPlayerHomeMap != null)
            {
                // murió o se fue lejos de casa: el jade aparece en la colonia (es la única fuente del fragmento)
                map = Find.AnyPlayerHomeMap;
                if (map != null) pos = DropCellFinder.TradeDropSpot(map);
            }
            Thing item = null;
            if (def.farewellItem != null && map != null)
            {
                item = ThingMaker.MakeThing(def.farewellItem);
                GenPlace.TryPlaceThing(item, pos, map, ThingPlaceMode.Near);
            }

            if (!died && mentor.Spawned)
            {
                // Se va caminando, como un huésped vanilla: deja la colonia y sale por el borde del mapa.
                FleckMaker.ThrowLightningGlow(mentor.DrawPos, mentor.Map, 3f);
                mentor.jobs?.StopAll();
                var leaveFaction = Find.FactionManager.RandomNonHostileFaction(allowHidden: false, allowDefeated: false, allowNonHumanlike: false);
                if (leaveFaction != null)
                {
                    mentor.SetFaction(leaveFaction);
                    Verse.AI.Group.LordMaker.MakeNewLord(leaveFaction, new Verse.AI.Group.LordJob_ExitMapBest(Verse.AI.LocomotionUrgency.Walk, canDig: false), mentor.Map,
                        new List<Pawn> { mentor });
                }
                else
                {
                    mentor.SetFaction(null);
                    mentor.DeSpawn();
                    if (!Find.WorldPawns.Contains(mentor)) Find.WorldPawns.PassToWorld(mentor, PawnDiscardDecideMode.KeepForever);
                }
            }
            QuestPart_MentorRefuge.OpenQuest()?.End(died ? QuestEndOutcome.Fail : QuestEndOutcome.Success, sendLetter: false);

            SoundCues.OnCamera(died ? "CVG_Cue_MentorDeath" : "CVG_Cue_MentorFarewell");
            string text = (died ? def.deathText : def.farewellText).Formatted(mentor.Named("PAWN")).Resolve();
            Find.LetterStack.ReceiveLetter(
                (died ? "CVG_Mentor_DeathLabel" : "CVG_Mentor_FarewellLabel").Translate(mentor.LabelShortCap), text,
                died ? LetterDefOf.Death : LetterDefOf.NeutralEvent, item != null ? new LookTargets(item) : LookTargets.Invalid);
        }

        public override void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_References.Look(ref mentor, "mentor", saveDestroyedThings: true);
            Scribe_Values.Look(ref offerTick, "offerTick", -1);
            Scribe_Values.Look(ref offered, "offered");
            Scribe_Values.Look(ref huntTick, "huntTick", -1);
            Scribe_Values.Look(ref wasDowned, "wasDowned");
            Scribe_Values.Look(ref leaveTick, "leaveTick", -1);
            Scribe_Values.Look(ref lessonIndex, "lessonIndex");
            Scribe_Values.Look(ref lessonStarted, "lessonStarted");
            Scribe_Values.Look(ref lessonBaseline, "lessonBaseline");
            Scribe_Collections.Look(ref lessonBaselines, "lessonBaselines", LookMode.Value, LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && lessonBaselines == null) lessonBaselines = new Dictionary<int, int>();
            Scribe_Values.Look(ref lessonStartTick, "lessonStartTick", -1);
            Scribe_Values.Look(ref nextLessonTick, "nextLessonTick", -1);
            Scribe_Values.Look(ref protectionUsed, "protectionUsed");
            Scribe_Values.Look(ref finished, "finished");
        }
    }

    public static class MentorUtility
    {
        /// <summary>
        /// Genera al Maestro: nombre fijo, solo sus rasgos (todos positivos), sin heridas ni achaques, raíz y reino del
        /// Def, todas las técnicas de sus manuales y el hediff de presencia. Sin facción todavía (sin mensajes).
        /// </summary>
        public static Pawn Generate(MentorDef def)
        {
            Pawn pawn = null;
            for (int i = 0; i < 20; i++)
            {
                var request = new PawnGenerationRequest(def.pawnKind, faction: null, forceGenerateNewPawn: true,
                    canGeneratePawnRelations: false, mustBeCapableOfViolence: true, allowAddictions: false,
                    fixedBiologicalAge: def.biologicalAge, fixedChronologicalAge: def.chronologicalAge, fixedGender: def.gender,
                    forcedXenotype: ModsConfig.BiotechActive ? XenotypeDefOf.Baseliner : null,
                    developmentalStages: DevelopmentalStage.Adult);
                pawn = PawnGenerator.GeneratePawn(request);
                if (pawn.story == null || pawn.story.DisabledWorkTagsBackstoryAndTraits == WorkTags.None) break;
                Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
                pawn = null;
            }
            if (pawn == null) return null;

            pawn.Name = new NameTriple(def.firstName, def.nickName, def.lastName);

            // Sin defectos: solo los rasgos del Def; fuera raíces, cuerpos y linajes al azar del generador (linajes con defectos).
            var traits = pawn.story.traits;
            foreach (var t in traits.allTraits.ToList()) traits.RemoveTrait(t);
            if (pawn.genes != null)
                foreach (var g in pawn.genes.GenesListForReading.Where(g => g.def.HasModExtension<DefModExtension_SpiritRoot>()
                             || (g.def.displayCategory?.defName.StartsWith("CVG_Blood") ?? false)).ToList())
                    pawn.genes.RemoveGene(g);
            foreach (var t in def.traits) traits.GainTrait(new Trait(t, 0, forced: true));
            pawn.health.RemoveAllHediffs();

            var comp = CompCultivator.For(pawn);
            if (comp != null && def.rootGrade != null)
            {
                CultivationGenerator.GiveRoot(pawn, def.rootGrade, def.rootElements.Count > 0 ? def.rootElements : CultivationGenerator.RandomElements());
                comp.Sync();
                var tracker = comp.TrackerFor(def.realm.path, create: true);
                tracker.EnsureStarted();
                tracker.AdvanceTo(def.realm, def.foundationQuality);
                for (int s = 1; s < def.stage && s < def.realm.stages; s++) tracker.AddProgress(def.realm.progressPerStage);
                foreach (var manual in def.manuals)
                {
                    comp.Techniques.LearnManual(manual, sendMessage: false);
                    foreach (var t in manual.Techniques) comp.Techniques.Unlock(t, free: true);
                }
                tracker.Resource.Fill();
            }
            if (def.presenceHediff != null) pawn.health.AddHediff(def.presenceHediff);
            // Comparte la ideología de la colonia (la postura la decide el jugador, docs/DISENO.md): con una al azar sufría
            // "crisis de fe" e intentos de conversión mientras enseñaba (visto en el arnés -cvgplaytest).
            var colonyIdeo = Faction.OfPlayerSilentFail?.ideos?.PrimaryIdeo;
            if (ModsConfig.IdeologyActive && pawn.ideo != null && colonyIdeo != null) pawn.ideo.SetIdeo(colonyIdeo);
            if (pawn.apparel != null)
                foreach (var apDef in def.apparel)
                    if (apDef?.IsApparel ?? false)
                        pawn.apparel.Wear((Apparel)ThingMaker.MakeThing(apDef, apDef.MadeFromStuff ? GenStuff.DefaultStuffFor(apDef) : null), dropReplacedApparel: false);
            return pawn;
        }

        public static void GiveItems(List<ThingDefCountClass> items, Pawn near)
        {
            if (items.NullOrEmpty() || near?.MapHeld == null) return;
            foreach (var entry in items)
            {
                if (entry?.thingDef == null) continue;
                int left = entry.count;
                while (left > 0)
                {
                    var thing = ThingMaker.MakeThing(entry.thingDef, entry.stuff);
                    thing.stackCount = Mathf.Min(left, entry.thingDef.stackLimit);
                    left -= thing.stackCount;
                    GenPlace.TryPlaceThing(thing, near.PositionHeld, near.MapHeld, ThingPlaceMode.Near);
                }
            }
        }
    }
}
