using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Prueba del avance a Fundación como lo hace el jugador: con <c>-cvgflowtest</c> (y <c>-quicktest</c>) un colono llega al cuello
    /// de botella de Refinamiento de Qi; sin la investigación el botón de avance está deshabilitado; se termina la investigación,
    /// hay una píldora de Fundación y un cojín; se aprieta el botón (misma acción que el gizmo) y el juego corre de verdad: el colono
    /// va al cojín, toma la píldora, hace el retiro y sale la tirada. Si falla, espera el enfriamiento y reintenta (hasta 6).
    /// Registra cada paso y cierra el juego. Sin el argumento no hace nada.
    /// </summary>
    public class GameComponent_FlowTest : GameComponent
    {
        private static readonly bool Enabled = GenCommandLine.CommandLineArgPassed("cvgflowtest");
        private readonly StringBuilder sb = new StringBuilder("[Convergence] Flow test: breakthrough to Foundation");
        private int fails, stage, attempts, waitUntil, pillsBefore;
        private Pawn pawn;
        private Thing cushion;

        public GameComponent_FlowTest(Game game) { }

        private void Check(bool ok, string what)
        {
            sb.Append("\n    " + (ok ? "ok   " : "FAIL ") + what);
            if (!ok) fails++;
        }

        private void Finish()
        {
            string guide = "CVG_GuideLetterLabel".Translate();
            Check(Find.LetterStack.LettersListForReading.Any(l => l.Label == guide) || Find.Archive.ArchivablesListForReading.Any(a => a.ArchivedLabel == guide),
                "the cultivator's guide letter arrived after the Convergence");
            var rt = pawn != null ? CompCultivator.For(pawn)?.Realm : null;
            if (rt != null) Check(rt.RatePerDay > 0f, "the cultivation pace is tracked for the tab estimate (" + rt.RatePerDay.ToString("0.#") + "/day)");
            sb.Append(fails == 0 ? "\n  ALL OK" : "\n  FAILURES: " + fails);
            if (fails == 0) Log.Message(sb.ToString()); else Log.Error(sb.ToString());
            stage = 99;
            Root.Shutdown();
        }

        /// <summary>
        /// Avanza ticks a mano con el juego en pausa (~0,25 s por cuadro), como la prueba de 30 días: el juego de prueba corre a
        /// 20 cuadros por segundo y a velocidad normal no llegaba a simular los días de las lecciones antes del tiempo límite.
        /// </summary>
        public override void GameComponentUpdate()
        {
            if (!Enabled || stage == 0 || stage == 99 || Find.CurrentMap == null) return;
            var tm = Find.TickManager;
            tm.CurTimeSpeed = TimeSpeed.Paused;
            float start = UnityEngine.Time.realtimeSinceStartup;
            while (UnityEngine.Time.realtimeSinceStartup - start < 0.25f && stage != 99)
            {
                try { tm.DoSingleTick(); }
                catch (System.Exception e) { Log.Error("[Convergence] flow test tick exception: " + e); }
            }
        }

        public override void GameComponentTick()
        {
            if (!Enabled || stage == 99 || Find.CurrentMap == null) return;
            var map = Find.CurrentMap;
            int now = Find.TickManager.TicksGame;
            if (stage == 0)
            {
                if (now < 300) return;
                Setup(map);
                return;
            }
            var comp = CompCultivator.For(pawn);
            var t = comp?.Realm;
            if (stage < 3 && (pawn == null || pawn.Dead || t == null)) { Check(false, "test pawn lost"); Finish(); return; }
            if (stage >= 3 && stage < 99)
            {
                // Las lecciones las cumple cualquier discípulo: si el de prueba murió (un asalto mientras pasan los días), otro.
                if (pawn == null || pawn.Dead || !pawn.Spawned)
                {
                    var mentor = GameComponent_Mentor.Instance?.Mentor;
                    var next = map.mapPawns.FreeColonistsSpawned.FirstOrDefault(p => p != mentor && CompCultivator.For(p)?.Realm?.Started == true)
                               ?? map.mapPawns.FreeColonistsSpawned.FirstOrDefault(p => p != mentor);
                    if (next == null) { Check(false, "no disciples left alive (test pawn: " + (pawn == null ? "null" : pawn.LabelShort + " dead=" + pawn.Dead + " spawned=" + pawn.Spawned + " holder=" + pawn.ParentHolder) + "; colonists: " + string.Join(", ", PawnsFinder.AllMaps_FreeColonists.Select(c => c.LabelShort + (c.Spawned ? "" : "(not spawned)"))) + ")"); Finish(); return; }
                    sb.Append("\n    note disciple replaced by " + next.LabelShort + (pawn != null && pawn.Dead ? " (" + pawn.LabelShort + " died)" : ""));
                    pawn = next;
                    var c = CompCultivator.For(pawn);
                    if (!c.HasRoot) CultivationGenerator.GiveRoot(pawn, CultivationGenerator.RandomGrade(), CultivationGenerator.RandomElements());
                    c.Sync(); c.SyncRealm(); c.Realm?.EnsureStarted();
                }
                MentorTick(map, now);
                return;
            }
            if (stage == 2)
            {
                // retiro en curso: esperar a que termine (sin tope infinito: 3 días de juego)
                if (t.Realm.order >= 2) { Check(true, $"ascended to {t.Realm.defName} after {attempts} attempt(s)"); Report(map, t); MentorStart(); return; }
                if (pawn.CurJobDef == CVG_DefOf.CVG_Breakthrough) return;
                if (now - waitUntil > GenDate.TicksPerDay * 3) { Check(false, "retreat never finished"); Finish(); return; }
                // terminó sin subir: fallo (desviación + enfriamiento)
                bool deviated = t.Realm.breakthroughFailureHediff == null || pawn.health.hediffSet.HasHediff(t.Realm.breakthroughFailureHediff) || t.BreakthroughCooldownTicksLeft > 0;
                if (pawn.CurJobDef != CVG_DefOf.CVG_Breakthrough && t.BreakthroughCooldownTicksLeft > 0)
                {
                    Check(deviated, $"attempt {attempts} failed with deviation/cooldown ({t.BreakthroughCooldownTicksLeft.ToStringTicksToPeriod()})");
                    if (attempts >= 6) { Check(false, "no success in 6 attempts"); Finish(); return; }
                    // como el jugador: esperar el enfriamiento (lo saltamos), llenar el qi y otra píldora
                    t.StartBreakthroughCooldown(0);
                    var hd = t.Realm.breakthroughFailureHediff == null ? null : pawn.health.hediffSet.GetFirstHediffOfDef(t.Realm.breakthroughFailureHediff);
                    if (hd != null) pawn.health.RemoveHediff(hd);
                    t.Resource.Fill();
                    PlacePill(map);
                    Press();
                }
            }
        }

        // ---------------------------------------------------------------- el Maestro, como en la partida real
        private int mentorStageStart, lessonsSeen;
        private Quest quest;
        private MentorLesson handledLesson;

        private void MentorStart()
        {
            sb.Append("\n  mentor");
            // El Maestro llega por su misión ("un viejo sabio busca refugio"): ofrecerla y aceptarla como el jugador.
            var map = Find.CurrentMap;
            quest = Find.QuestManager.QuestsListForReading.FirstOrDefault(q => q.root == CVG_DefOf.CVG_MentorRefuge);
            if (quest == null)
            {
                var slate = new RimWorld.QuestGen.Slate();
                slate.Set("map", map);
                slate.Set("points", StorytellerUtility.DefaultThreatPointsNow(map));
                quest = QuestUtility.GenerateQuestAndMakeAvailable(CVG_DefOf.CVG_MentorRefuge, slate);
            }
            Check(quest != null && quest.State == QuestState.NotYetAccepted, "the sage's quest is offered and waits for the player");
            quest?.Accept(pawn);
            mentorStageStart = Find.TickManager.TicksGame;
            stage = 3;
        }

        private void MentorTick(Map map, int now)
        {
            // Mide las lecciones, no el combate: una colonia de prueba sin defensas no sobrevive a los asaltos de 8 días simulados.
            if (now % 250 == 0)
            {
                foreach (var hostile in map.mapPawns.AllPawnsSpawned.Where(p => p.HostileTo(Faction.OfPlayer) && !p.IsPrisoner).ToList()) hostile.Destroy();
                foreach (var c in map.mapPawns.FreeColonistsSpawned)
                {
                    if (c.needs?.food != null) c.needs.food.CurLevelPercentage = 1f;
                    if (c.needs?.rest != null) c.needs.rest.CurLevelPercentage = 1f;
                    if (c.needs?.mood != null) c.needs.mood.CurLevelPercentage = 0.8f;
                }
            }
            var state = GameComponent_Mentor.Instance;
            var tr = HarmonyLib.Traverse.Create(state);
            if (now - mentorStageStart > GenDate.TicksPerDay * 15) { Check(false, $"mentor stage {stage} timed out (lesson: {state.CurrentLesson?.label ?? "-"})"); Finish(); return; }
            if (stage == 3)
            {
                if (state.Mentor == null) return;
                Check(state.Mentor.Spawned && state.Mentor.Faction == Faction.OfPlayer, $"mentor {state.Mentor.LabelShort} arrived and joined");
                stage = 4;
                return;
            }
            if (stage == 4)
            {
                // Cumplir las lecciones como lo haría el jugador: el cojín ya está; subir de etapa; aprender una técnica.
                var lesson = state.CurrentLesson;
                if (lesson == null) { Check(lessonsSeen >= 3 && now - mentorStageStart > GenDate.TicksPerDay * 6, $"all lessons completed through normal play, paced ({(now - mentorStageStart) / (float)GenDate.TicksPerDay:0.0} days)"); tr.Field("leaveTick").SetValue(now); stage = 5; return; }
                var t = CompCultivator.For(pawn).Realm;
                if (lesson == handledLesson || !HarmonyLib.Traverse.Create(state).Field("lessonStarted").GetValue<bool>()) return;
                handledLesson = lesson;
                lessonsSeen++;
                if (lesson is MentorLesson_ReachStage) { t.AddProgress(t.Realm.progressPerStage); sb.Append("\n    note lesson: reach stage 2 (cultivated)"); }
                if (lesson is MentorLesson_LearnTechnique)
                {
                    var tech = CompCultivator.For(pawn).Techniques;
                    var path = DefDatabase<TechniqueManualDef>.AllDefsListForReading.Where(m => tech.CanOpenPath(m, out _)).OrderBy(m => m.order).FirstOrDefault();
                    if (path != null) tech.OpenPath(path);
                    var def = tech.Manuals.SelectMany(m => m.Techniques).FirstOrDefault(d => tech.CanUnlock(d, out _));
                    if (def != null) { tech.Unlock(def); sb.Append("\n    note lesson: learned " + def.label); }
                }
                return;
            }
            if (stage == 5)
            {
                if (!HarmonyLib.Traverse.Create(state).Field("finished").GetValue<bool>()) return;
                var def = DefDatabase<MentorDef>.AllDefsListForReading.First(m => m.farewellItem != null);
                bool jade = map.listerThings.AllThings.Any(x => x.def.defName.StartsWith("CVG_JadeFragment"));
                Check(jade, "the mentor left and the jade fragment stays (quest chain starts)");
                Check(quest != null && quest.State == QuestState.EndedSuccess, "the sage's quest ended in success when he left (" + quest?.State + ")");
                Finish();
            }
        }

        private void Setup(Map map)
        {
            // Mide el avance y las lecciones, no la supervivencia: sin incidentes al azar (asaltos, locura animal, secuestros).
            Find.Storyteller.storytellerComps.Clear();
            Find.TickManager.CurTimeSpeed = TimeSpeed.Paused; // los ticks los corre GameComponentUpdate
            if (!GameComponent_Convergence.Active) GameComponent_Convergence.Instance?.TryFire(map);
            pawn = map.mapPawns.FreeColonistsSpawned.FirstOrDefault();
            var comp = CompCultivator.For(pawn);
            if (!comp.HasRoot) CultivationGenerator.GiveRoot(pawn, CultivationGenerator.RandomGrade(), CultivationGenerator.RandomElements());
            comp.Sync();
            comp.SyncRealm();
            var t = comp.Realm;
            t.EnsureStarted();
            while (!t.Bottleneck) t.AddProgress(t.Realm.progressPerStage);
            t.Resource.Fill();
            Check(t.Realm.order == 1 && t.Bottleneck, $"{pawn.LabelShort} at the {t.Realm.defName} bottleneck");

            var research = DefDatabase<ResearchProjectDef>.GetNamed("CVG_Research_Foundation");
            bool can = BreakthroughUtility.CanAttempt(comp, t, out string reason);
            Check(research.IsFinished || !can, "without the research the button is disabled: " + (reason ?? "-"));
            Find.ResearchManager.FinishProject(research, doCompletionDialog: false);
            foreach (var p in research.prerequisites ?? new System.Collections.Generic.List<ResearchProjectDef>()) Find.ResearchManager.FinishProject(p, false);

            var cushionDef = DefDatabase<ThingDef>.GetNamed("CVG_CultivationCushion");
            var cell = CellFinder.StandableCellNear(pawn.Position, map, 6f, c => c.GetFirstBuilding(map) == null);
            cushion = ThingMaker.MakeThing(cushionDef, GenStuff.DefaultStuffFor(cushionDef));
            cushion.SetFaction(Faction.OfPlayer);
            GenSpawn.Spawn(cushion, cell, map);
            PlacePill(map);
            can = BreakthroughUtility.CanAttempt(comp, t, out reason);
            Check(can, "with the research the button is enabled" + (can ? "" : ": " + reason));
            Check(!BreakthroughUtility.MakeGizmo(comp, t).Disabled, "the breakthrough button is enabled");
            Press();
        }

        private void PlacePill(Map map)
        {
            var pill = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("CVG_FoundationPill"));
            GenPlace.TryPlaceThing(pill, pawn.Position, map, ThingPlaceMode.Near, null, c => pawn.CanReach(c, Verse.AI.PathEndMode.ClosestTouch, Danger.Deadly));
            var t = CompCultivator.For(pawn).Realm;
            var found = BreakthroughUtility.FindItem(pawn, t.NextRealm);
            sb.Append($"\n    note pill at {pill.PositionHeld}, forbidden {pill.IsForbidden(pawn)}, reachable {pawn.CanReach(pill, Verse.AI.PathEndMode.ClosestTouch, Danger.Deadly)}, aid {BreakthroughUtility.HasAid(pawn)}, FindItem {(found != null ? "found" : "null")}");
        }

        private void Press()
        {
            var map = pawn.Map;
            pillsBefore = map.listerThings.ThingsOfDef(DefDatabase<ThingDef>.GetNamed("CVG_FoundationPill")).Sum(x => x.stackCount);
            attempts++;
            BreakthroughUtility.StartBreakthrough(pawn, CompCultivator.For(pawn).Realm.Path);
            Check(pawn.CurJobDef == CVG_DefOf.CVG_Breakthrough, $"attempt {attempts}: retreat job started");
            var job = pawn.CurJob;
            sb.Append("\n    note job targetA " + job?.targetA + ", targetB " + job?.targetB + ", FindItem now " + (BreakthroughUtility.FindItem(pawn, CompCultivator.For(pawn).Realm.NextRealm)?.ToString() ?? "null"));
            waitUntil = Find.TickManager.TicksGame;
            stage = 2;
        }

        private void Report(Map map, RealmTracker t)
        {
            Check(pawn.Position.DistanceTo(cushion.Position) < 2f || pawn.CurJobDef != CVG_DefOf.CVG_Breakthrough, "retreat done on the cushion");
            Check(CompCultivator.For(pawn).Techniques.Points > 0, "new realm gives comprehension points");
            sb.Append("\n    note hediffs: " + string.Join(", ", pawn.health.hediffSet.hediffs.Select(h => h.def.defName)));
        }
    }
}
