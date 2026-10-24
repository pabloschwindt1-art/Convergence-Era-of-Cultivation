using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Convergence
{
    /// <summary>
    /// Arnés de pulido jugando (docs/revision/PULIDO.md): con <c>-cvgplaytest</c> (y <c>-quicktest</c>) fuerza la Convergencia,
    /// da raíces y reinos a los colonos, termina la investigación, suelta materiales, construye una muestra de nuestros
    /// edificios con facturas y hace correr la partida ~30 días (<c>-cvgplaydays=N</c>) lo más rápido posible. Al final
    /// reporta errores del log agrupados por origen, el cultivo alcanzado, la red de qi, avances y tribulaciones, incidentes y
    /// cartas, y el tiempo por llamada de los ticks de nuestros componentes; luego cierra el juego. Inerte sin el argumento:
    /// no parchea nada ni engancha el log.
    /// </summary>
    public class GameComponent_PlayTest : GameComponent
    {
        public static readonly bool Enabled = GenCommandLine.CommandLineArgPassed("cvgplaytest");

        private enum Phase { Waiting, Running, Done }
        private Phase phase;
        private int startTick, endTick;
        private float realStart;
        private int nextProgressDay;
        private float lastBeat;
        private readonly StringBuilder setup = new StringBuilder();

        // Presupuesto de tiempo real (minutos) por si la partida va demasiado lenta: se reporta lo que se alcanzó.
        private static float RealTimeBudgetMinutes =>
            GenCommandLine.TryGetCommandLineArg("cvgplaybudget", out string b) && float.TryParse(b, out float m) && m > 0f ? m : 50f;

        public GameComponent_PlayTest(Game game) { }

        public override void GameComponentTick()
        {
            if (!Enabled || phase != Phase.Waiting || Find.CurrentMap == null || Find.TickManager.TicksGame < 200) return;
            try { if (PlayBase.Natural) PlayBase.Setup(Find.CurrentMap, setup); else Setup(Find.CurrentMap); }
            catch (Exception e) { Log.Error("[Convergence] playtest setup failed: " + e); }
            int days = 30;
            if (GenCommandLine.TryGetCommandLineArg("cvgplaydays", out string arg) && int.TryParse(arg, out int d) && d > 0) days = d;
            startTick = Find.TickManager.TicksGame;
            endTick = startTick + days * GenDate.TicksPerDay;
            nextProgressDay = 1;
            realStart = Time.realtimeSinceStartup;
            PlayTestProbe.Install();
            phase = Phase.Running;
            Log.Message($"[Convergence] playtest: setup done, running {days} days\n{setup}");
        }

        /// <summary>Corre ticks a mano con el juego en pausa, ~0,25 s por cuadro (más rápido que la velocidad máxima).</summary>
        public override void GameComponentUpdate()
        {
            if (!Enabled || phase != Phase.Running) return;
            var tm = Find.TickManager;
            tm.CurTimeSpeed = TimeSpeed.Paused;
            float frameStart = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - frameStart < 0.25f && tm.TicksGame < endTick)
            {
                long t0 = System.Diagnostics.Stopwatch.GetTimestamp();
                try { tm.DoSingleTick(); }
                catch (Exception e) { Log.Error("[Convergence] playtest tick exception: " + e); }
                PlayTestProbe.AddTick(System.Diagnostics.Stopwatch.GetTimestamp() - t0);
                if (tm.TicksGame % 250 == 0) AutoDefend();
                if (tm.TicksGame % 2500 == 0) AutoBreakthrough();
                if (PlayBase.Natural && tm.TicksGame % 2500 == 0) PlayBase.AcceptMentorQuest();
                if ((tm.TicksGame - startTick) % (5 * GenDate.TicksPerDay) == 0) Snapshot();
            }
            int day = (tm.TicksGame - startTick) / GenDate.TicksPerDay;
            float minutes = (Time.realtimeSinceStartup - realStart) / 60f;
            // Una línea al día y otra cada 20 s reales: run-test.ps1 cierra el juego si el log deja de crecer 45 s.
            if (day >= nextProgressDay || Time.realtimeSinceStartup - lastBeat > 20f)
            {
                nextProgressDay = Mathf.Max(nextProgressDay, day + 1);
                lastBeat = Time.realtimeSinceStartup;
                Log.Message($"[Convergence] playtest day {day} ({(tm.TicksGame - startTick) / Mathf.Max(1f, minutes * 60f):0} ticks/s)");
            }
            if (tm.TicksGame >= endTick || minutes > RealTimeBudgetMinutes)
            {
                phase = Phase.Done;
                string report;
                try { report = Report(day, minutes); }
                catch (Exception e) { report = "[Convergence] playtest report failed: " + e; }
                Log.Message(report);
                Root.Shutdown();
            }
        }

        /// <summary>
        /// Sin jugador nadie recluta a los colonos y los asaltos los matan sin pelear (artefacto del arnés): mientras haya
        /// enemigos en el mapa se reclutan los que pueden pelear; al irse, se liberan.
        /// </summary>
        private static void AutoDefend()
        {
            var map = Find.CurrentMap;
            if (map == null) return;
            // Solo con enemigos cerca de algún colono (30 casillas): un enemigo perdido en la otra punta del mapa dejaba a todos
            // reclutados días enteros, sin comer ni dormir (playtest 2026-10-24: golpes de calor en una ola de calor).
            var colonists = map.mapPawns.FreeColonistsSpawned;
            bool threat = map.mapPawns.AllPawnsSpawned.Any(p => !p.Downed && !p.Dead && p.HostileTo(Faction.OfPlayer) && !p.IsPrisoner
                && (p.RaceProps.Humanlike || p.RaceProps.IsMechanoid || p.InAggroMentalState)
                && colonists.Any(c => c.Position.InHorDistOf(p.Position, 30f)));
            foreach (var c in map.mapPawns.FreeColonistsSpawned.ToList())
            {
                if (c.drafter == null || c.Downed || c.WorkTagIsDisabled(WorkTags.Violent)) continue;
                if (threat != c.Drafted) c.drafter.Drafted = threat;
                if (threat) CastTechniques(c);
            }
        }

        public static int BreakthroughsOrdered, TechniquesOrdered;

        /// <summary>Lo que haría un jugador: ordenar el retiro de avance a quien llega al cuello de botella (tribulación incluida).</summary>
        private static void AutoBreakthrough()
        {
            var map = Find.CurrentMap;
            if (map == null) return;
            foreach (var p in map.mapPawns.FreeColonistsSpawned.ToList())
            {
                var comp = CompCultivator.For(p);
                if (comp == null || p.Drafted || p.CurJobDef == CVG_DefOf.CVG_Breakthrough) continue;
                foreach (var t in comp.ActiveTrackers.ToList())
                    if (t.Started && t.Bottleneck && BreakthroughUtility.CanAttempt(comp, t, out _))
                    {
                        BreakthroughUtility.StartBreakthrough(p, t.Path);
                        BreakthroughsOrdered++;
                        break;
                    }
            }
        }

        /// <summary>En combate, los colonos reclutados lanzan técnicas: primero defensivas sobre sí, luego ofensivas al enemigo más cercano.</summary>
        private static void CastTechniques(Pawn p)
        {
            if (p.abilities == null || p.jobs?.curJob?.ability != null) return;
            var job = CultivatorAI.DefensiveJob(p);
            if (job == null)
            {
                var enemy = p.Map.mapPawns.AllPawnsSpawned.Where(e => !e.Downed && e.HostileTo(p)).OrderBy(e => e.Position.DistanceToSquared(p.Position)).FirstOrDefault();
                if (enemy == null) return;
                foreach (var a in p.abilities.AllAbilitiesForReading)
                {
                    if (!TechniqueUtility.IsTechnique(a.def) || !a.def.ai_IsOffensive || !a.CanCast) continue;
                    if (!a.verb.CanHitTarget(enemy) || !a.CanApplyOn((LocalTargetInfo)enemy)) continue;
                    job = a.GetJob(enemy, enemy);
                    break;
                }
            }
            if (job != null && p.jobs.TryTakeOrderedJob(job, JobTag.Misc)) TechniquesOrdered++;
        }

        private void Snapshot()
        {
            var sb = new StringBuilder($"[Convergence] playtest snapshot day {(Find.TickManager.TicksGame - startTick) / GenDate.TicksPerDay}" +
                $" ({GenLocalDate.Season(Find.CurrentMap)}, outdoors {Find.CurrentMap.mapTemperature.OutdoorTemp:0}C):");
            var qiNodes = Find.CurrentMap.listerThings.AllThings.Select(t => t.TryGetComp<CompQiNode>()).Where(n => n?.Net != null).ToList();
            foreach (var net in qiNodes.Select(n => n.Net).Distinct())
                sb.Append($"\n    qi net: supply {net.SupplyDay:0}, demand {net.DemandDay:0}, stored {net.Stored:0}: " +
                          string.Join(", ", qiNodes.Where(x => x.Net == net).Select(x => x.parent.def.defName.Replace("CVG_", "") + (x.ProduceDay > 0 ? $"(+{x.ProduceDay:0})" : ""))));
            foreach (var p in PawnsFinder.AllMaps_FreeColonists)
            {
                var t = CompCultivator.For(p)?.Realm;
                sb.Append($"\n    {p.LabelShort}: {(t != null && t.Started ? t.Realm.label + " " + t.Stage : "-")}, mood {p.needs?.mood?.CurLevelPercentage ?? 0f:P0}, " +
                          $"food {p.needs?.food?.CurLevelPercentage ?? 0f:P0}, cultivated {Rec(p, "CVG_TimeCultivating") / (float)GenDate.TicksPerHour:0} h, health {p.health.summaryHealth.SummaryHealthPercent:P0}, job {p.CurJobDef?.defName ?? "-"}");
                if (p.needs?.mood?.thoughts != null)
                {
                    var thoughts = new List<Thought>();
                    p.needs.mood.thoughts.GetAllMoodThoughts(thoughts);
                    var worst = thoughts.GroupBy(x => x.def).Select(g => (def: g.Key, off: g.Sum(x => x.MoodOffset())))
                        .Where(x => x.off < 0f).OrderBy(x => x.off).Take(4);
                    sb.Append("\n        worst thoughts: " + string.Join(", ", worst.Select(x => $"{x.def.defName} {x.off:0}")));
                }
            }
            Log.Message(sb.ToString());
        }

        // ------------------------------------------------------------------ preparación

        private readonly HashSet<IntVec3> used = new HashSet<IntVec3>();

        private void Setup(Map map)
        {
            if (!GameComponent_Convergence.Active) GameComponent_Convergence.Instance.TryFire(map);
            setup.Append("  Convergence active: " + GameComponent_Convergence.Active);
            Find.ResearchManager.DebugSetAllProjectsFinished();

            // Materiales cerca del centro (lo que piden las facturas de nuestras mesas).
            var stock = new Dictionary<string, int>
            {
                { "Steel", 800 }, { "WoodLog", 800 }, { "Silver", 1500 }, { "Gold", 100 }, { "Jade", 120 }, { "ComponentIndustrial", 40 },
                { "Cloth", 300 }, { "Leather_Plain", 200 }, { "BlocksGranite", 600 }, { "MealSurvivalPack", 300 }, { "MedicineHerbal", 40 },
                { "CVG_SpiritStone", 400 }, { "CVG_SpiritHerb", 150 }, { "CVG_BeastCore", 6 }, { "CVG_SpiritSteel", 150 }, { "CVG_CloudSilk", 100 },
                { "CVG_SpiritCrystal", 60 }, { "RawRice", 300 }, { "Meat_Cow", 200 }, { "CVG_SpiritJade", 60 }, { "CVG_BeastBlood", 30 },
                { "CVG_SpiritWood", 300 }, { "CVG_SpiritThread", 120 }, { "CVG_CloudCotton", 120 },
            };
            foreach (var realm in DefDatabase<CultivationRealmDef>.AllDefsListForReading.Where(r => r.breakthroughItem != null))
                stock[realm.breakthroughItem.defName] = 3;
            foreach (var kv in stock)
            {
                var def = DefDatabase<ThingDef>.GetNamedSilentFail(kv.Key);
                if (def == null) continue;
                int left = kv.Value;
                while (left > 0)
                {
                    var t = ThingMaker.MakeThing(def, GenStuff.DefaultStuffFor(def));
                    t.stackCount = Mathf.Min(left, def.stackLimit);
                    left -= t.stackCount;
                    GenPlace.TryPlaceThing(t, map.Center + new IntVec3(-12, 0, -12), map, ThingPlaceMode.Near);
                }
            }

            // Lo mínimo de una colonia que un jugador tendría (sin esto los colonos mueren de cosas ajenas al mod):
            // un lugar para dormir por colono, mesa y sillas.
            foreach (var p in map.mapPawns.FreeColonistsSpawned.ToList())
                if (TrySpawn("Bed", map, map.Center + new IntVec3(10, 0, 10), 25, out var bed) && bed is Building_Bed b) b.CompAssignableToPawn.TryAssignPawn(p);
            TrySpawn("Table2x2c", map, map.Center + new IntVec3(-6, 0, 6), 15, out _);
            for (int k = 0; k < 4; k++) TrySpawn("DiningChair", map, map.Center + new IntVec3(-6, 0, 6), 15, out _);

            // Colonos: raíz, reinos, trabajo de cultivo.
            var cultivate = DefDatabase<WorkTypeDef>.GetNamedSilentFail("CVG_Cultivation");
            int i = 0;
            foreach (var p in map.mapPawns.FreeColonistsSpawned.ToList())
            {
                var comp = CompCultivator.For(p);
                if (comp == null) continue;
                if (!comp.HasRoot) CultivationGenerator.GiveRoot(p, CultivationGenerator.RandomGrade(), CultivationGenerator.RandomElements());
                comp.Sync();
                var tracker = comp.Realm;
                if (tracker != null && tracker.EnsureStarted())
                    for (int r = 0; r < i % 3 && tracker.NextRealm != null; r++) BreakthroughUtility.Ascend(p, tracker.NextRealm, 1f);
                // El de Fundación arranca en el cuello de botella: así el arnés ordena su retiro y pasa por la tribulación del Núcleo Dorado.
                if (tracker != null && tracker.Started && i % 3 == 1)
                    for (int st = 0; st <= tracker.Realm.stages && !tracker.Bottleneck; st++) tracker.AddProgress(tracker.Realm.progressPerStage);
                comp.SyncRealm();
                // Como un jugador: gasta los puntos de comprensión abriendo un camino y aprendiendo sus técnicas.
                var tech = comp.Techniques;
                var path = DefDatabase<TechniqueManualDef>.AllDefsListForReading.Where(m => tech.CanOpenPath(m, out _)).OrderBy(m => m.order).FirstOrDefault();
                if (path != null) tech.OpenPath(path);
                foreach (var def in tech.Manuals.SelectMany(m => m.Techniques).ToList())
                    if (tech.CanUnlock(def, out _)) tech.Unlock(def);
                setup.Append($"\n  {p.LabelShort}: path {path?.defName ?? "none"}, techniques {p.abilities?.AllAbilitiesForReading.Count(a => TechniqueUtility.IsTechnique(a.def)) ?? 0}, points left {tech.Points}");
                if (cultivate != null && !p.WorkTypeIsDisabled(cultivate)) p.workSettings?.SetPriority(cultivate, 2);
                i++;
            }
            setup.Append($"\n  colonists given roots/realms: {i}");

            // Red de qi: pozo sobre la veta, pilar y cristal; los consumidores van junto al pilar.
            IntVec3 hub = map.Center;
            var vein = QiNetworkUtility.VeinTerrain;
            if (vein != null)
            {
                QiNetworkUtility.EnsureVeins(map);
                var cells = map.AllCells.Where(c => map.terrainGrid.TerrainAt(c) == vein).ToList();
                var origin = cells.FirstOrDefault(c => cells.Contains(c + IntVec3.East) && cells.Contains(c + IntVec3.North) && cells.Contains(c + IntVec3.NorthEast));
                if (origin.IsValid && TrySpawn("CVG_QiVeinWell", map, origin, 0, out _))
                {
                    hub = origin;
                    TrySpawn("CVG_FormationPillar", map, origin + new IntVec3(4, 0, 0), 3, out var pillar);
                    if (pillar != null) hub = pillar.Position;
                    TrySpawn("CVG_QiCrystal", map, hub + new IntVec3(-2, 0, 2), 4, out _);
                }
            }
            string[] nearHub = { "CVG_PuppetWorkshop", "CVG_SunLantern", "CVG_FrostFormation", "CVG_QiGatheringCore", "CVG_SwordFormationCore", "CVG_ProtectiveFormationCore" };
            foreach (var d in nearHub) TrySpawn(d, map, hub, 12, out _);
            string[] rest =
            {
                "CVG_SpiritBrazier", "CVG_RitualBrazier", "CVG_SpiritForge", "CVG_AlchemyFurnace", "CVG_SpiritLoom", 
                "CVG_CalligraphyDesk", "CVG_TalismanTable", "CVG_RuneTable", 
                "CVG_GraftingAltar", "CVG_CultivationCushion", "CVG_CultivationCushion", "CVG_MeditationPlatform", 
                "CVG_MahjongTable", "CVG_BoneRefiningFurnace",
            };
            foreach (var d in rest) TrySpawn(d, map, map.Center, 40, out _);
        }

        private bool TrySpawn(string defName, Map map, IntVec3 near, int radius, out Thing thing)
        {
            thing = null;
            var def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
            if (def == null) { setup.Append($"\n  (no {defName}: module not loaded)"); return false; }
            foreach (var cell in GenRadial.RadialCellsAround(near, Mathf.Max(radius, 1), true))
            {
                var rect = GenAdj.OccupiedRect(cell, Rot4.North, def.size).ExpandedBy(1);
                if (!rect.InBounds(map)) continue;
                if (rect.Cells.Any(c => used.Contains(c) || !c.Standable(map) || c.GetEdifice(map) != null || c.GetFirstItem(map) != null || c.Fogged(map)))
                    continue;
                if (def.PlaceWorkers != null && def.PlaceWorkers.Any(pw => !pw.AllowsPlacing(def, cell, Rot4.North, map).Accepted)) continue;
                thing = ThingMaker.MakeThing(def, def.MadeFromStuff ? GenStuff.DefaultStuffFor(def) : null);
                thing.SetFactionDirect(Faction.OfPlayer);
                GenSpawn.Spawn(thing, cell, map, Rot4.North);
                foreach (var c in rect) used.Add(c);
                thing.TryGetComp<CompRefuelable>()?.Refuel(thing.TryGetComp<CompRefuelable>().Props.fuelCapacity);
                if (thing is Building_WorkTable table)
                    foreach (var recipe in table.def.AllRecipes.Where(r => r.AvailableNow).Take(3))
                    {
                        var bill = (Bill_Production)recipe.MakeNewBill();
                        bill.repeatMode = BillRepeatModeDefOf.RepeatCount;
                        bill.repeatCount = 2;
                        table.BillStack.AddBill(bill);
                    }
                setup.Append($"\n  built {defName} at {cell}");
                return true;
            }
            setup.Append($"\n  could not place {defName}");
            return false;
        }

        // ------------------------------------------------------------------ reporte

        private string Report(int day, float minutes)
        {
            var sb = new StringBuilder("[Convergence] Playtest report");
            var tm = Find.TickManager;
            sb.Append($"\n  {day} days in {minutes:0.0} min ({(tm.TicksGame - startTick) / Mathf.Max(1f, minutes * 60f):0} ticks/s)");

            sb.Append("\n- log errors and warnings (count, origin, first line)");
            int ours = 0, total = 0;
            foreach (var e in PlayTestProbe.Errors.OrderByDescending(e => e.Value.count).Take(25))
            {
                sb.Append($"\n    {e.Value.count,5}x [{e.Value.origin}] {e.Key}");
            }
            foreach (var e in PlayTestProbe.Errors.Values) { total += e.count; if (e.ours) ours += e.count; }
            sb.Append($"\n    total {total}, from Convergence {ours}");

            var map = Find.CurrentMap;
            sb.Append("\n- colonists");
            foreach (var p in PawnsFinder.AllMaps_FreeColonists)
            {
                var comp = CompCultivator.For(p);
                var t = comp?.Realm;
                string realm = t != null && t.Started ? $"{t.Realm.label} {t.Stage}/{t.Realm.stages} ({t.StagePercent:P0})" : "not cultivating";
                string qi = t != null && t.Started && t.Resource != null ? $"{t.Resource.Current:0}/{t.Resource.Max:0}" : "-";
                sb.Append($"\n    {p.LabelShort}: {realm}, qi {qi}, speed {p.GetStatValue(CVG_DefOf.CVG_CultivationSpeed):0.00}, " +
                          $"breakthroughs {Rec(p, "CVG_Breakthroughs")}, tribulations {Rec(p, "CVG_TribulationsSurvived")}, techniques cast {Rec(p, "CVG_TechniquesCast")}, " +
                          $"hours cultivating {Rec(p, "CVG_TimeCultivating") / (float)GenDate.TicksPerHour:0}, mood {p.needs?.mood?.CurLevelPercentage ?? 0f:P0}");
            }
            if (PlayBase.Natural) sb.Append(PlayBase.RelationsReport());
            if (map != null)
                sb.Append($"\n- colony: {map.mapPawns.FreeColonistsSpawnedCount} colonists, food {map.resourceCounter.TotalHumanEdibleNutrition:0} nutrition, " +
                          $"wealth {map.wealthWatcher.WealthTotal:0}, Convergence {GameComponent_Convergence.Active}");
            sb.Append($"\n- deaths of colonists ({PlayTestProbe.Deaths.Count})");
            foreach (var d in PlayTestProbe.Deaths) sb.Append("\n    " + d);
            sb.Append($"\n- orders: {BreakthroughsOrdered} breakthrough retreats, {TechniquesOrdered} techniques");
            sb.Append("\n- raids (points)");
            foreach (var r in PlayTestProbe.Raids) sb.Append("\n    " + r);

            if (map != null)
            {
                sb.Append("\n- qi network");
                var nodes = map.listerThings.AllThings.Select(t => t.TryGetComp<CompQiNode>()).Where(n => n?.Net != null).ToList();
                var nets = nodes.Select(n => n.Net).Distinct().ToList();
                foreach (var n in nets)
                    sb.Append($"\n    net: supply {n.SupplyDay:0}/day, demand {n.DemandDay:0}/day, stored {n.Stored:0}/{n.Capacity:0}, satisfied {n.Satisfied}: " +
                              string.Join(", ", nodes.Where(x => x.Net == n).Select(x => x.parent.def.defName + (x.parent.Faction == Faction.OfPlayer ? "" : " (not ours)"))));
                if (nets.Count == 0) sb.Append("\n    no qi net");

                sb.Append("\n- bills done on our benches");
                foreach (var b in map.listerBuildings.allBuildingsColonist.OfType<Building_WorkTable>().Where(b => b.def.defName.StartsWith("CVG_")))
                {
                    sb.Append($"\n    {b.def.defName}: " + string.Join(", ", b.BillStack.Bills.OfType<Bill_Production>().Select(x => $"{x.recipe.defName} {2 - x.repeatCount}/2")));
                    foreach (var x in b.BillStack.Bills.OfType<Bill_Production>().Where(x => x.repeatCount > 0))
                        sb.Append($"\n        {x.recipe.defName}: {WhyNot(x, b)}");
                }
            }

            sb.Append("\n- incidents");
            foreach (var kv in PlayTestProbe.Incidents.OrderByDescending(k => k.Value)) sb.Append($"\n    {kv.Value,3}x {kv.Key}");
            sb.Append("\n- letters");
            foreach (var kv in PlayTestProbe.Letters.OrderByDescending(k => k.Value).Take(40)) sb.Append($"\n    {kv.Value,3}x {kv.Key}");

            sb.Append("\n- tick cost of Convergence code (top by total; per-call in microseconds)");
            double tickMs = PlayTestProbe.TickTotalMs, ourMs = 0;
            foreach (var kv in PlayTestProbe.Timings.OrderByDescending(k => k.Value.ms).Take(25))
            {
                ourMs += kv.Value.ms;
                double perCall = kv.Value.ms * 1000.0 / Math.Max(1, kv.Value.calls);
                string flag = perCall > 50 ? "  SLOW" : "";
                sb.Append($"\n    {kv.Value.ms,9:0.0} ms {kv.Value.calls,9} calls {perCall,8:0.0} us  {kv.Key}{flag}");
            }
            sb.Append($"\n    whole game ticks: {tickMs:0} ms over {PlayTestProbe.TickCount} ticks ({tickMs / Math.Max(1, PlayTestProbe.TickCount):0.000} ms/tick); " +
                      $"Convergence top 25: {100.0 * ourMs / Math.Max(1.0, tickMs):0.0}% (nested calls count twice)");
            sb.Append(ours == 0 ? "\n  ALL OK" : "\n  FAILURES: " + ours);
            return sb.ToString();
        }

        /// <summary>Por qué una factura no avanzó: nadie con la habilidad, falta un ingrediente, o investigación.</summary>
        private static string WhyNot(Bill_Production bill, Building_WorkTable table)
        {
            var r = bill.recipe;
            var map = table.Map;
            var why = new List<string>();
            if (!r.AvailableNow) why.Add("research/unavailable");
            var colonists = map.mapPawns.FreeColonistsSpawned;
            if (colonists.Any() && !colonists.Any(p => r.PawnSatisfiesSkillRequirements(p))) why.Add("no colonist with " + r.MinSkillString.Replace("\n", " ").Trim());
            if (r.workSkill != null && colonists.All(p => p.WorkTypeIsDisabled(r.requiredGiverWorkType ?? DefDatabase<WorkTypeDef>.GetNamed("Crafting"))))
                why.Add("work type disabled");
            foreach (var ing in r.ingredients)
            {
                float have = map.listerThings.AllThings.Where(t => !t.IsForbidden(Faction.OfPlayer) && ing.filter.Allows(t) && bill.ingredientFilter.Allows(t))
                    .Sum(t => (float)t.stackCount * r.IngredientValueGetter.ValuePerUnitOf(t.def));
                if (have < ing.GetBaseCount()) why.Add($"missing {ing.filter.Summary} ({have:0}/{ing.GetBaseCount():0})");
            }
            if (!table.CurrentlyUsableForBills()) why.Add("bench not usable (power/fuel/qi?)");
            return why.Count == 0 ? "nothing obvious (time, priority or reachability)" : string.Join("; ", why);
        }

        private static int Rec(Pawn p, string def)
        {
            var r = DefDatabase<RecordDef>.GetNamedSilentFail(def);
            return r == null || p.records == null ? 0 : Mathf.RoundToInt(p.records.GetValue(r));
        }
    }

    /// <summary>Sondas del arnés: log, incidentes, cartas y tiempos. Solo se instalan con <c>-cvgplaytest</c>.</summary>
    public static class PlayTestProbe
    {
        public class ErrorEntry { public int count; public string origin; public bool ours; }

        public static readonly Dictionary<string, ErrorEntry> Errors = new Dictionary<string, ErrorEntry>();
        public static readonly Dictionary<string, int> Incidents = new Dictionary<string, int>();
        public static readonly Dictionary<string, int> Letters = new Dictionary<string, int>();
        public static readonly Dictionary<string, (double ms, long calls)> Timings = new Dictionary<string, (double, long)>();
        public static readonly List<string> Deaths = new List<string>();
        public static readonly List<string> Raids = new List<string>();
        public static double TickTotalMs;
        public static long TickCount;
        private static bool installed;
        private static readonly double MsPerTimestamp = 1000.0 / System.Diagnostics.Stopwatch.Frequency;

        private static readonly HashSet<string> TickMethods = new HashSet<string>
        {
            "MapComponentTick", "GameComponentTick", "WorldComponentTick", "CompTick", "CompTickRare", "CompTickLong", "CompTickInterval",
            "CompPostTick", "CompPostTickInterval", "Tick", "TickRare", "TickLong", "TickInterval", "PostTick", "PostTickInterval",
            "NeedInterval", "MentalStateTick", "TransformValue", "GetValueUnfinalized", "CurrentStateInternal", "GetReport",
        };

        public static void AddTick(long timestamps)
        {
            TickTotalMs += timestamps * MsPerTimestamp;
            TickCount++;
        }

        public static void Install()
        {
            if (installed || !GameComponent_PlayTest.Enabled) return;
            installed = true;
            Application.logMessageReceivedThreaded += OnLog;
            var harmony = new Harmony("prospero1088.convergence.playtest");
            harmony.Patch(AccessTools.Method(typeof(IncidentWorker), nameof(IncidentWorker.TryExecute)),
                postfix: new HarmonyMethod(typeof(PlayTestProbe), nameof(IncidentPostfix)));
            harmony.Patch(AccessTools.Method(typeof(LetterStack), nameof(LetterStack.ReceiveLetter), new[] { typeof(Letter), typeof(string), typeof(int), typeof(bool) }),
                prefix: new HarmonyMethod(typeof(PlayTestProbe), nameof(LetterPrefix)));
            harmony.Patch(AccessTools.Method(typeof(Pawn), nameof(Pawn.Kill)), prefix: new HarmonyMethod(typeof(PlayTestProbe), nameof(KillPrefix)));
            harmony.Patch(AccessTools.Method(typeof(Thing), nameof(Thing.Destroy)), prefix: new HarmonyMethod(typeof(PlayTestProbe), nameof(DestroyPrefix)));
            harmony.Patch(AccessTools.Method(typeof(FireUtility), nameof(FireUtility.TryStartFireIn)), prefix: new HarmonyMethod(typeof(PlayTestProbe), nameof(FirePrefix)));
            harmony.Patch(AccessTools.Method(typeof(IncidentWorker), nameof(IncidentWorker.TryExecute)),
                prefix: new HarmonyMethod(typeof(PlayTestProbe), nameof(RaidPrefix)));
            var pre = new HarmonyMethod(typeof(PlayTestProbe), nameof(TimePrefix));
            var post = new HarmonyMethod(typeof(PlayTestProbe), nameof(TimePostfix));
            int patched = 0;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name.StartsWith("Convergence")))
            {
                Type[] types;
                try { types = asm.GetTypes(); } catch { continue; }
                foreach (var type in types)
                {
                    if (type.IsGenericTypeDefinition || type == typeof(GameComponent_PlayTest)) continue;
                    foreach (var m in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                    {
                        if (!TickMethods.Contains(m.Name) || m.IsAbstract || m.IsGenericMethodDefinition || m.GetMethodBody() == null) continue;
                        try { harmony.Patch(m, pre, post); patched++; }
                        catch { }
                    }
                }
            }
            Log.Message($"[Convergence] playtest: timing {patched} tick methods");
        }

        private static void OnLog(string condition, string stackTrace, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception && type != LogType.Warning) return;
            if (condition.StartsWith("[Convergence] playtest") || condition.StartsWith("[Convergence] Playtest")) return;
            string first = condition.Split('\n')[0];
            if (first.Length > 160) first = first.Substring(0, 160);
            string key = (type == LogType.Warning ? "warn: " : "") + System.Text.RegularExpressions.Regex.Replace(first, @"\d+", "#");
            bool ours = condition.Contains("Convergence") || condition.Contains("CVG_") || (stackTrace?.Contains("Convergence.") ?? false);
            string origin = "?";
            var frames = (stackTrace ?? "") + "\n" + condition;
            foreach (var line in frames.Split('\n'))
            {
                var l = line.Trim().TrimStart('a', 't', ' ');
                if (l.Length == 0 || l.StartsWith("Verse.Log") || l.StartsWith("UnityEngine") || l.StartsWith("System.Environment")) continue;
                int paren = l.IndexOf('(');
                if (l.Contains("Convergence.")) { origin = (paren > 0 ? l.Substring(0, paren) : l).Trim(); break; }
                if (origin == "?" && (l.StartsWith("Verse.") || l.StartsWith("RimWorld."))) origin = (paren > 0 ? l.Substring(0, paren) : l).Trim();
            }
            lock (Errors)
            {
                if (!Errors.TryGetValue(key, out var e)) Errors[key] = e = new ErrorEntry { origin = origin, ours = ours };
                e.count++;
            }
        }

        private static void IncidentPostfix(IncidentWorker __instance, bool __result)
        {
            if (!__result) return;
            string k = __instance.def.defName;
            Incidents[k] = Incidents.TryGetValue(k, out int n) ? n + 1 : 1;
        }

        private static void LetterPrefix(Letter let)
        {
            string k = let.def.defName + ": " + let.Label.RawText;
            Letters[k] = Letters.TryGetValue(k, out int n) ? n + 1 : 1;
        }

        private static int firesLogged;

        /// <summary>De dónde salen los incendios (un fuego quemó el pozo y dos colonos el día 24).</summary>
        public static void FirePrefix(IntVec3 c, Map map, float fireSize)
        {
            if (firesLogged >= 12 || map == null || !map.IsPlayerHome || Find.TickManager.TicksGame < 3000) return;
            firesLogged++;
            var st = new System.Diagnostics.StackTrace(1, false).GetFrames()?.Take(7).Select(f => f.GetMethod()?.DeclaringType?.Name + "." + f.GetMethod()?.Name);
            Log.Warning($"[Convergence] playtest: fire at {c} day {Find.TickManager.TicksGame / GenDate.TicksPerDay} <- {string.Join(" <- ", st ?? new string[0])}");
        }

        /// <summary>Quién y cómo destruye nuestros edificios (el pozo de la veta desaparecía sin explicación).</summary>
        public static void DestroyPrefix(Thing __instance, DestroyMode mode)
        {
            if (__instance?.def == null || __instance.def.category != ThingCategory.Building || !__instance.def.defName.StartsWith("CVG_") || !__instance.Spawned) return;
            if (Find.TickManager.TicksGame < 3000) return;
            var st = new System.Diagnostics.StackTrace(1, false).GetFrames()?.Take(6).Select(f => f.GetMethod()?.DeclaringType?.Name + "." + f.GetMethod()?.Name);
            Log.Warning($"[Convergence] playtest: {__instance.def.defName} destroyed ({mode}, hp {__instance.HitPoints}/{__instance.MaxHitPoints}) at {__instance.Position} <- {string.Join(" <- ", st ?? new string[0])}");
        }

        private static void KillPrefix(Pawn __instance, DamageInfo? dinfo)
        {
            if (__instance.Dead || !__instance.IsColonist) return;
            var t = CompCultivator.For(__instance)?.Realm;
            string realm = t != null && t.Started ? t.Realm.label + " " + t.Stage : "mortal";
            var inst = dinfo?.Instigator;
            string by = inst == null ? "no instigator" : inst is Pawn ip ? $"{ip.KindLabel} ({ip.Faction?.Name ?? "no faction"})" : inst.def.defName;
            string cause = dinfo?.Def?.defName ?? __instance.health.hediffSet.hediffs.Where(h => h.def.lethalSeverity > 0 || h.def.isBad)
                .OrderByDescending(h => h.Severity).FirstOrDefault()?.def.defName ?? "?";
            Deaths.Add($"day {GenDate.DaysPassedSinceSettle}: {__instance.LabelShort} ({realm}) {cause} by {by}");
        }

        private static void RaidPrefix(IncidentWorker __instance, IncidentParms parms)
        {
            if (__instance is IncidentWorker_RaidEnemy || __instance.def.category == IncidentCategoryDefOf.ThreatBig)
                Raids.Add($"day {GenDate.DaysPassedSinceSettle}: {__instance.def.defName} {parms.points:0} pts, {parms.faction?.Name ?? "?"}");
        }

        private static void TimePrefix(out long __state) => __state = System.Diagnostics.Stopwatch.GetTimestamp();

        private static void TimePostfix(long __state, MethodBase __originalMethod)
        {
            double ms = (System.Diagnostics.Stopwatch.GetTimestamp() - __state) * MsPerTimestamp;
            string k = __originalMethod.DeclaringType.Name + "." + __originalMethod.Name;
            lock (Timings) Timings[k] = Timings.TryGetValue(k, out var v) ? (v.ms + ms, v.calls + 1) : (ms, 1);
        }
    }
}
