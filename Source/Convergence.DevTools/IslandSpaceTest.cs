using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Prueba de la isla voladora hasta el espacio (requiere Odyssey y Convergence: Flying Sect): solo con <c>-cvgislandspacetest</c>.
    /// Arma una isla (corazón, vasija, propulsores, altar, sala sellada con formación del aliento, cúpula celestial con qi), le da el reino
    /// Núcleo Dorado (etapas 1-4) a 4 colonos, la lanza a una casilla de la órbita (asteroides), la posa en el mapa de vacío y mide:
    /// resistencia al vacío por etapa, cuánto daña el vacío a un cultivador fuera de la cúpula, el aire de la sala sellada
    /// (con la pared rota, con y sin formación del aliento) y los percances de aterrizaje (todos los <c>LandingOutcomeDef</c>).
    /// Escribe cada medida en el log y en <c>%TEMP%/cvg_islandspace.log</c>.
    /// </summary>
    public class GameComponent_IslandSpaceTest : GameComponent
    {
        private static readonly bool Enabled = GenCommandLine.CommandLineArgPassed("cvgislandspacetest");
        private int phase = 0, phaseTick, fails, sample;
        private readonly StringBuilder sb = new StringBuilder("[Convergence] Island space test");
        private Map planet, space;
        private Building_GravEngine engine;
        private Thing altar, breath, dome, vessel;
        private readonly List<Pawn> cultivators = new List<Pawn>();
        private Pawn control;
        private Gravship ship;
        private IntVec3 sealedCell;
        private readonly Dictionary<Pawn, List<float>> severities = new Dictionary<Pawn, List<float>>();
        private float roomVacuumBefore;
        private IntVec3 breachCell;
        private Thing breachWall;

        public GameComponent_IslandSpaceTest(Game game) { }

        private static string LogPath => Path.Combine(Path.GetTempPath(), "cvg_islandspace.log");

        private void Note(string what)
        {
            sb.Append("\n    " + what);
            Log.Message("[Convergence] space: " + what);
            try { File.AppendAllText(LogPath, what + Environment.NewLine); } catch { }
        }

        private void Step(bool ok, string what)
        {
            Note((ok ? "ok   " : "FAIL ") + what);
            if (!ok) fails++;
        }

        private void Finish()
        {
            phase = 99;
            sb.Append(fails == 0 ? "\n  ALL OK" : "\n  FAILURES: " + fails);
            if (fails == 0) Log.Message(sb.ToString()); else Log.Error(sb.ToString());
        }

        private Thing Spawn(string def, IntVec3 cell, Map map, Rot4? rot = null)
        {
            var d = DefDatabase<ThingDef>.GetNamed(def);
            var t = ThingMaker.MakeThing(d, d.MadeFromStuff ? GenStuff.DefaultStuffFor(d) : null);
            t.SetFactionDirect(Faction.OfPlayer);
            GenSpawn.Spawn(t, cell, map, rot ?? Rot4.North);
            return t;
        }

        public override void GameComponentTick()
        {
            if (!Enabled || phase >= 99 || Find.CurrentMap == null || Find.TickManager.TicksGame < 200) return;
            if (!ModsConfig.OdysseyActive) { Log.Message("[Convergence] Island space test skipped: Odyssey not active"); phase = 99; return; }
            try { Tick(); }
            catch (Exception e) { Step(false, "exception in phase " + phase + ": " + e); Finish(); }
        }

        /// <summary>Se llama en cada cuadro (corre también con el juego en pausa): Odyssey pausa al llegar a un mapa nuevo y puede abrir avisos.</summary>
        public override void GameComponentUpdate()
        {
            if (!Enabled || phase < 2 || phase >= 99) return;
            if (phase >= 11 && phase <= 13) { try { FlightUpdate(); } catch (Exception e) { Step(false, "exception in the flight phase " + phase + ": " + e); Finish(); } if (phase >= 99) return; }
            if (phase >= 11 && phase <= 13) return;      // durante la cinemática no se toca la velocidad
            foreach (var w in Find.WindowStack.Windows.ToList())
                if (w.forcePause && !(w is EditWindow_Log)) { Note("closing blocking window " + w.GetType().Name); w.Close(false); }
            if (Find.TickManager.Paused) Find.TickManager.CurTimeSpeed = TimeSpeed.Ultrafast;
        }

        private void Tick()
        {
            int now = Find.TickManager.TicksGame;
            var naming = Find.WindowStack.WindowOfType<Dialog_NamePlayerGravship>();
            if (naming != null) naming.Close();
            switch (phase)
            {
                case 0: planet = Find.CurrentMap; Build(planet); phase = 1; phaseTick = now; break;
                case 1: if (now - phaseTick < 60) return; BeforeLaunch(); StartTakeoff(); break;
                case 2: if (now - phaseTick < 120) return; InSpace(); StartExposure(); phase = 3; phaseTick = now; sample = 0; break;
                case 3: Exposure(now); break;
                case 4: if (now - phaseTick < 600) return; BreachResult(); BreathOn(); phase = 5; phaseTick = now; break;
                case 5: if (now - phaseTick < 900) return; BreathResult(); BarrierOff(); phase = 6; phaseTick = now; break;
                case 6: if (now - phaseTick < 600) return; BarrierOffResult(); Outcomes(); Finish(); break;
            }
        }

        // ---------------------------------------------------------------- isla
        private IslandEndgameBuilder.Result built;

        private void DumpResist(string when, Pawn p)
        {
            var sbr = new StringBuilder("resistance dump (" + when + ") for " + p.LabelShort + ": stat " + p.GetStatValue(StatDefOf.VacuumResistance).ToString("0.00") + "; hediffs: ");
            foreach (var h in p.health.hediffSet.hediffs)
            {
                var stage = h.CurStage;
                float off = stage?.statOffsets?.GetStatOffsetFromList(StatDefOf.VacuumResistance) ?? 0f;
                if (off != 0f || h.def.defName.StartsWith("CVG_Hediff_")) sbr.Append(h.def.defName + "[sev " + h.Severity.ToString("0.00") + ", stage " + h.CurStageIndex + ", offset " + off.ToString("0.00") + "] ");
            }
            sbr.Append("| explanation: " + StatDefOf.VacuumResistance.Worker.GetExplanationUnfinalized(StatRequest.For(p), ToStringNumberSense.Absolute).Replace(((char)10).ToString(), " / "));
            Note(sbr.ToString());
        }
        private IntVec3 oldEnginePos, delta;
        private readonly Dictionary<Thing, IntVec3> positionsBefore = new Dictionary<Thing, IntVec3>();

        private void Build(Map map)
        {
            var gc = Current.Game.GetComponent<GameComponent_Convergence>();
            if (gc != null && !gc.Occurred) { gc.MarkOccurred(); Note("the Convergence is marked as occurred (otherwise the cultivators are not awake and lose their realm)"); }
            built = IslandEndgameBuilder.Build(map);
            oldEnginePos = built.engine.Position;
            engine = built.engine; altar = built.altar; dome = built.dome; vessel = built.vessels.FirstOrDefault();
            foreach (var n in built.notes) Note("builder: " + n);
            foreach (var room in built.rooms)
                Note("room '" + room.name + "' " + room.interior.Width + "x" + room.interior.Height + ": " + room.placed + " pieces placed" + (room.missing.Count > 0 ? ", not placed: " + string.Join(", ", room.missing.Distinct()) : "") + (room.breath != null ? ", with a breath formation" : ""));
            sealedCell = built.rooms[0].probe;
            breath = built.rooms[0].breath;
            Step(engine != null && altar != null && dome != null && built.thrusters.Count > 0, "endgame island built: heart, " + built.mounts.Count + " core mounts, " + built.vessels.Count + " vessels, " + built.thrusters.Count + " vanilla thrusters, altar, dome, " + built.rooms.Count + " sealed rooms");
            var center = map.Center;
            // 4 cultivadores de Núcleo Dorado (etapas 1 a 4) y un colono sin cultivo para comparar
            var realm = DefDatabase<CultivationRealmDef>.GetNamed("CVG_Realm_GoldenCore");
            for (int i = 0; i < 5; i++)
            {
                var p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayer, forceGenerateNewPawn: true, tile: map.Tile));
                GenSpawn.Spawn(p, CellFinder.RandomClosewalkCellNear(center + new IntVec3(0, 0, 3), map, 4), map);
                if (i < 4) { SetRealm(p, realm, i + 1); cultivators.Add(p); } else control = p;
                if (p.drafter != null) p.drafter.Drafted = true;
            }
            var pilot = cultivators[0];
            pilot.Position = altar.InteractionCell;
            pilot.Notify_Teleported(true, true);
        }

        private static void SetRealm(Pawn p, CultivationRealmDef realm, int stage)
        {
            var comp = CompCultivator.For(p);
            if (comp == null) return;
            comp.Sync();
            if (!comp.HasRoot) { CultivationGenerator.GiveRoot(p, CultivationGenerator.RandomGrade(), CultivationGenerator.RandomElements()); comp.Sync(); }
            var t = comp.Realm;
            if (!t.Started) t.EnsureStarted();
            if (t.Realm.order < realm.order) t.AdvanceTo(realm, 1f);
            t.SetStage(stage);
            comp.InvalidateHealthScale();
        }

        // ---------------------------------------------------------------- despegue y aterrizaje
        private string ProducerReport(Map m)
        {
            var sbp = new StringBuilder();
            foreach (var t in built.producers)
            {
                var n = t.TryGetComp<CompQiNode>();
                if (n == null || !t.Spawned || t.Map != m) { sbp.Append(t.def.defName + ": no qi node; "); continue; }
                var rf = t.TryGetComp<CompRefuelable>();
                sbp.Append(t.def.defName.Replace("CVG_", "") + " " + n.ProduceDay.ToString("0") + "/day (roofed " + m.roofGrid.Roofed(t.Position) + ", concentration " + QiConcentrationUtility.ConcentrationAt(t.Position, m).ToString("0.0") + (rf != null ? ", fuel " + rf.Fuel.ToString("0") : "") + "); ");
            }
            var net = built.producers.Select(t => t.TryGetComp<CompQiNode>()).FirstOrDefault(n => n?.Net != null)?.Net;
            return sbp + (net == null ? "no net" : "NET supply " + net.SupplyDay.ToString("0") + " vs demand " + net.DemandDay.ToString("0") + " per day, stored " + net.Stored.ToString("0") + ", satisfied " + net.Satisfied);
        }

        private void BeforeLaunch()
        {
            planet.GetComponent<QiNetworkManager>()?.Step(0.01f);
            Note("qi sources on the planet: " + ProducerReport(planet));
            foreach (var p in cultivators)
                Note(p.LabelShort + ": realm " + CompCultivator.For(p)?.Realm?.Realm?.defName + " stage " + CompCultivator.For(p)?.Realm?.Stage + ", VacuumResistance " + p.GetStatValue(StatDefOf.VacuumResistance).ToString("0.00"));
            DumpResist("before launch", cultivators[3]);
            Note("control (no cultivation): VacuumResistance " + control.GetStatValue(StatDefOf.VacuumResistance).ToString("0.00"));
            Step(cultivators.All(p => p.GetStatValue(StatDefOf.VacuumResistance) >= 0.34f && p.GetStatValue(StatDefOf.VacuumResistance) <= 0.501f), "the 4 Golden Core cultivators resist vacuum 35% to 50% (the cap of the realm is 50%)");
            Step(cultivators[3].GetStatValue(StatDefOf.VacuumResistance) > cultivators[0].GetStatValue(StatDefOf.VacuumResistance), "a higher Golden Core stage resists more");
            engine.ForceSubstructureDirty();
            foreach (var t in planet.listerThings.AllThings.Where(t => t is Building && t.Faction == Faction.OfPlayer && (t.Position - planet.Center).LengthHorizontal < 20f)) positionsBefore[t] = t.Position;
            Note("substructure cells " + engine.ValidSubstructure.Count + ", fuel " + engine.TotalFuel.ToString("0") + "/" + engine.MaxFuel.ToString("0") + ", max launch distance " + engine.MaxLaunchDistance);
            var left = planet.listerThings.AllThings.Where(t => t is Building b && t.Faction == Faction.OfPlayer && (t.Position - planet.Center).LengthHorizontal < 20f && !engine.ValidSubstructure.Contains(t.Position)).GroupBy(t => t.def.defName).Select(g => g.Key + "x" + g.Count());
            Note("buildings NOT on valid substructure (they would stay behind): " + (left.Any() ? string.Join(", ", left) : "none"));
            foreach (var ri in built.rooms)
            {
                var rr = ri.interior.CenterCell.GetRoom(planet);
                var edges = ri.interior.ExpandedBy(1).EdgeCells.Select(c => c.GetEdifice(planet)?.def.defName ?? "NONE").GroupBy(x => x).Select(g => g.Key + "x" + g.Count());
                Note("room '" + ri.name + "' on the planet: " + (rr == null ? "no room at the probe" : rr.CellCount + " cells, open roof " + rr.OpenRoofCount) + "; ring: " + string.Join(", ", edges));
            }
            var room = sealedCell.GetRoom(planet);
            Note("dining hall on the planet: " + (room == null ? "none" : room.CellCount + " cells") + "; vacuum at the breath formation " + VacuumUtility.GetVacuum(sealedCell, planet).ToString("0.00"));
            foreach (var q in new[] { 0f, 0.25f, 0.5f, 0.8f, 1f })
                Note("launch quality " + q.ToString("0.00") + ": chance of a landing mishap " + GravshipUtility.NegativeLandingOutcomeFromQuality(q).ToString("P0") + ", cooldown " + ((int)GravshipUtility.LaunchCooldownFromQuality(q)).ToStringTicksToPeriod());
        }

        private PlanetTile dest = PlanetTile.Invalid;
        private float phaseReal;

        /// <summary>Elige un destino en la órbita, gasta el combustible y arranca el despegue REAL de Odyssey (con su cinemática).</summary>
        private void StartTakeoff()
        {
            var orbit = Find.WorldGrid.Orbit;
            Step(orbit != null, "the world has an orbit layer");
            if (orbit == null) { Finish(); return; }
            PlanetTile cur = planet.Tile;
            float cost = 0; int dist = 0;
            // un asteroide de verdad: un objeto del mundo en la órbita cuyo generador de mapa es de asteroide (si el mundo no tiene, se crea uno)
            WorldObject asteroid = Find.WorldObjects.AllWorldObjects.FirstOrDefault(o => o.Tile.Valid && o.Tile.Layer == orbit && o is MapParent && o.def.mapGenerator != null && o.def.mapGenerator.defName.Contains("Asteroid")
                && GravshipUtility.TryGetPathFuelCost(cur, o.Tile, out var c0, out var d0, 10f, engine.FuelUseageFactor) && c0 <= engine.TotalFuel);
            Note("asteroid world objects already in orbit: " + Find.WorldObjects.AllWorldObjects.Count(o => o.Tile.Valid && o.Tile.Layer == orbit && o.def.mapGenerator != null && o.def.mapGenerator.defName.Contains("Asteroid")));
            if (asteroid == null)
            {
                var adef = DefDatabase<WorldObjectDef>.GetNamedSilentFail("AsteroidBasic");
                for (int i = 0; i < orbit.TilesCount && !dest.Valid && adef != null; i++)
                {
                    var t = new PlanetTile(i, orbit);
                    if (Find.WorldObjects.AnyWorldObjectAt(t) || !TileFinder.IsValidTileForNewSettlement(t, null, forGravship: true)) continue;
                    if (!GravshipUtility.TryGetPathFuelCost(cur, t, out cost, out dist, 10f, engine.FuelUseageFactor)) continue;
                    if (cost > engine.TotalFuel || dist > engine.MaxLaunchDistance) continue;
                    dest = t;
                }
                if (dest.Valid)
                {
                    asteroid = WorldObjectMaker.MakeWorldObject(adef);
                    asteroid.Tile = dest;
                    Find.WorldObjects.Add(asteroid);
                    Note("no asteroid in this world: a basic asteroid was placed in orbit at " + dest);
                }
            }
            else { dest = asteroid.Tile; GravshipUtility.TryGetPathFuelCost(cur, dest, out cost, out dist, 10f, engine.FuelUseageFactor); }
            Step(dest.Valid, dest.Valid ? "found a reachable asteroid " + asteroid?.def.defName + " at " + dest + " (biome " + dest.Tile.PrimaryBiome?.defName + ", path cost " + cost.ToString("0") + " spirit stones, distance " + dist + ")"
                : "found a reachable orbit tile (fuel " + engine.TotalFuel.ToString("0") + ", range " + engine.MaxLaunchDistance + ")");
            if (!dest.Valid) { Finish(); return; }
            engine.launchInfo = new LaunchInfo { quality = 1f, doNegativeOutcome = false, pilot = cultivators[0] };
            float before = engine.TotalFuel;
            engine.ConsumeFuel(dest);
            Step(engine.TotalFuel < before, "takeoff consumed fuel " + before.ToString("0") + " -> " + engine.TotalFuel.ToString("0"));
            Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;
            Find.GravshipController.InitiateTakeoff(engine, dest);
            Note("InitiateTakeoff called: the real takeoff cutscene starts");
            phase = 11; phaseReal = Time.realtimeSinceStartup;
        }

        private bool TimedOut(string what)
        {
            if (Time.realtimeSinceStartup - phaseReal < 240f) return false;
            var c = Find.GravshipController;
            Step(false, what + " did not finish in 4 minutes (travelling=" + c.IsGravshipTravelling + ", cutscene=" + WorldComponent_GravshipController.CutsceneInProgress + ", landing confirmation=" + c.LandingAreaConfirmationInProgress + ", paused=" + Find.TickManager.Paused + ")");
            Finish();
            return true;
        }

        /// <summary>Se llama en cada cuadro durante el vuelo (el juego puede estar en cinemática o en pausa).</summary>
        private void FlightUpdate()
        {
            if (phase == 11)
            {
                // despegue: la cinemática termina y el gravbarco viaja por el mundo
                var traveller = Find.WorldObjects.AllWorldObjects.OfType<Gravship>().FirstOrDefault();
                if (traveller != null) { ship = traveller; Step(true, "takeoff completed after " + (Time.realtimeSinceStartup - phaseReal).ToString("0") + " s: the gravship travels the world (" + ship.Things.Count() + " things aboard)"); phase = 12; phaseReal = Time.realtimeSinceStartup; return; }
                TimedOut("takeoff");
            }
            else if (phase == 12)
            {
                // llegada: el mapa de órbita se genera solo y aparece el marcador de aterrizaje (hay que confirmar el atraque)
                space = Find.WorldObjects.MapParentAt(dest)?.Map;
                var marker = space?.listerThings.ThingsOfDef(ThingDefOf.GravshipLandingMarker).FirstOrDefault() as GravshipLandingMarker;
                if (marker != null)
                {
                    Step(true, "arrival: the orbit map exists and the landing marker waits for the docking confirmation (" + (Time.realtimeSinceStartup - phaseReal).ToString("0") + " s)");
                    Note("orbit map: biome " + space.Biome.defName + ", size " + space.Size.x + "x" + space.Size.z + ", parent " + space.Parent?.GetType().Name + ", natural rock cells " + space.AllCells.Count(c => c.GetEdifice(space)?.def.building?.isNaturalRock == true) + ", map things " + space.listerThings.AllThings.Count);
                    Step(space.Biome.inVacuum, "the destination is in vacuum");
                    Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;
                    // lo mismo que hace el botón "Confirmar atraque": empezar el aterrizaje y soltar el marcador del controlador
                    marker.BeginLanding(Find.GravshipController);
                    typeof(WorldComponent_GravshipController).GetField("landingMarker", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)?.SetValue(Find.GravshipController, null);
                    Note("landing confirmed: BeginLanding called, the real landing cutscene starts");
                    phase = 13; phaseReal = Time.realtimeSinceStartup; return;
                }
                TimedOut("arrival");
            }
            else if (phase == 13)
            {
                var landed = space == null ? null : GravshipUtility.GetPlayerGravEngine_NewTemp(space);
                if (landed != null && landed.Spawned && !WorldComponent_GravshipController.CutsceneInProgress && !Find.GravshipController.LandingAreaConfirmationInProgress)
                {
                    engine = landed;
                    Step(true, "landing completed after " + (Time.realtimeSinceStartup - phaseReal).ToString("0") + " s: the island heart is on the asteroid map");
                    AfterLanding();
                    phase = 2; phaseTick = Find.TickManager.TicksGame; return;
                }
                TimedOut("landing");
            }
        }

        private void AfterLanding()
        {
            delta = engine.Position - oldEnginePos;
            dome = space.listerThings.ThingsOfDef(DefDatabase<ThingDef>.GetNamed("CVG_CelestialDome")).FirstOrDefault();
            breath = built.rooms[0].breath != null && built.rooms[0].breath.Spawned && built.rooms[0].breath.Map == space ? built.rooms[0].breath : null;
            Step(breath != null && dome != null, "the breath formation and the dome came along");
            Step(cultivators.All(p => p.Spawned && p.Map == space), "the 4 cultivators are on the asteroid map");
            var moved = positionsBefore.Where(kv => kv.Key.Spawned && kv.Key.Map == space && kv.Key.Position != kv.Value + delta).ToList();
            Note("things whose position after landing is NOT the expected translation: " + moved.Count + " of " + positionsBefore.Count + (moved.Count > 0 ? " -> " + string.Join(", ", moved.Take(14).Select(kv => kv.Key.def.defName + " " + kv.Key.def.size.x + "x" + kv.Key.def.size.z + " rot" + kv.Key.Rotation.AsInt + " off " + (kv.Key.Position - (kv.Value + delta)))) : ""));
            DumpResist("just after landing", cultivators[3]);
            foreach (var room in built.rooms)
                Note("room '" + room.name + "' at landing: vacuum " + VacuumUtility.GetVacuum(room.probe + delta, space).ToString("0.00"));
            Note("after landing: " + space.listerThings.ThingsOfDef(DefDatabase<ThingDef>.GetNamed("CVG_CelestialDome")).Count + " domes, " + space.listerThings.ThingsOfDef(DefDatabase<ThingDef>.GetNamed("CVG_QiCrystal")).Count + " qi crystals, " + space.listerThings.ThingsOfDef(DefDatabase<ThingDef>.GetNamed("CVG_FormationPillar")).Count + " pillars, " + space.listerThings.ThingsOfDef(DefDatabase<ThingDef>.GetNamed("SmallThruster")).Count + " thrusters aboard (built: " + built.thrusters.Count + ")");
            var mgr0 = space.GetComponent<QiNetworkManager>();
            mgr0?.EnsureBuilt();
            mgr0?.Step(0.01f);
            Find.TickManager.CurTimeSpeed = TimeSpeed.Ultrafast;
        }

        // ---------------------------------------------------------------- en el espacio
        private void InSpace()
        {
            DomeUtility.ResetCache();
            space.GetComponent<QiNetworkManager>()?.Step(0.01f);
            DomeUtility.ResetCache();
            var dc = dome?.TryGetComp<CompCelestialDome>();
            Note("dome working in space: " + dc?.Working + "; qi stored " + dome?.TryGetComp<CompQiNode>()?.Stored.ToString("0"));
            var center = space.Center;
            float inRoom = breath != null ? VacuumUtility.GetVacuum(breath.Position, space) : -1f;
            IntVec3 farCell = IntVec3.Invalid;
            for (int r = 36; r < 90 && !farCell.IsValid; r += 2)
                foreach (var c in GenRadial.RadialCellsAround(center + new IntVec3(r, 0, 0), 2f, true))
                    if (c.InBounds(space) && c.Standable(space) && DomeUtility.DomeAt(c, space) == null) { farCell = c; break; }
            float outside = farCell.IsValid ? VacuumUtility.GetVacuum(farCell, space) : -1f;
            Note("vacuum in the sealed room " + inRoom.ToString("0.00") + "; far from the island (no dome) " + outside.ToString("0.00") + "; at the heart (inside the dome radius) " + VacuumUtility.GetVacuum(engine.Position, space).ToString("0.00"));
            Step(outside >= 0.5f, "outside the dome the asteroid map is in vacuum");
            Step(dc != null && dc.Working && VacuumUtility.GetVacuum(engine.Position, space) == 0f, "inside the working dome there is no vacuum");
            foreach (var room in built.rooms)
            {
                var cell = room.probe + delta;
                var rm = cell.InBounds(space) ? cell.GetRoom(space) : null;
                var doorThing = (room.door + delta).GetEdifice(space);
                var ring = room.interior.ExpandedBy(1).EdgeCells.Select(c => (c + delta).GetEdifice(space)?.def.defName ?? "NONE").GroupBy(x => x).Select(g => g.Key + "x" + g.Count());
                Note("room '" + room.name + "' wall ring in space: " + string.Join(", ", ring));
                Note("room '" + room.name + "' in space: vacuum " + VacuumUtility.GetVacuum(cell, space).ToString("0.00") + ", " + (rm == null ? "no room" : rm.CellCount + " cells, " + (rm.TouchesMapEdge ? "touches the map edge" : "enclosed") + ", roofed " + (rm.OpenRoofCount == 0)) + ", door " + (doorThing?.def.defName ?? "missing") + (room.breath != null ? ", breath formation " + (room.breath.Spawned && room.breath.Map == space && room.breath.TryGetComp<CompQiNode>()?.Powered == true ? "powered" : "unpowered") : ""));
            }
            Note("landing delta " + delta + ", ship rotation " + ship?.Rotation);
            foreach (var bf in space.listerThings.ThingsOfDef(DefDatabase<ThingDef>.GetNamed("CVG_BreathFormation")))
                Note("breath formation at " + bf.Position + ": qi powered " + bf.TryGetComp<CompQiNode>()?.Powered + ", oxygen pusher " + (bf.TryGetComp<CompOxygenPusher>() != null) + ", room vacuum " + VacuumUtility.GetVacuum(bf.Position, space).ToString("0.00") + ", room " + (bf.Position.GetRoom(space)?.CellCount ?? 0) + " cells");
            Note("qi sources on the asteroid: " + ProducerReport(space));
            Note("orbital qi bonus here: +" + OrbitalQi.BonusFor(space).ToString("0") + " concentration");
            roomVacuumBefore = inRoom;
        }

        private void StartExposure()
        {
            int n = 0;
            foreach (var p in cultivators.Concat(new[] { control }))
            {
                if (!p.Spawned) { Step(false, p.LabelShort + " is not spawned on the asteroid map (" + (p.Dead ? "dead" : "state " + p.ParentHolder) + ")"); continue; }
                IntVec3 cell = IntVec3.Invalid;
                for (int r = 36; r < 70 && !cell.IsValid; r += 2)
                    foreach (var c in GenRadial.RadialCellsAround(space.Center + new IntVec3(r, 0, 0), 2f, true))
                        if (c.InBounds(space) && c.Standable(space) && DomeUtility.DomeAt(c, space) == null && VacuumUtility.GetVacuum(c, space) >= 0.99f && c.GetFirstPawn(space) == null) { cell = c; break; }
                if (!cell.IsValid) { Step(false, "no standable vacuum cell for " + p.LabelShort); continue; }
                p.Position = cell; p.Notify_Teleported(true, true);
                if (p.drafter != null) p.drafter.Drafted = true;
                severities[p] = new List<float>();
                n++;
            }
            Note(n + " pawns standing in open vacuum outside the dome, drafted, for 60 s of game time");
        }

        private void Exposure(int now)
        {
            if (now - phaseTick < 600) return;
            phaseTick = now; sample++;
            foreach (var kv in severities)
            {
                kv.Value.Add(kv.Key.Dead ? 1f : (kv.Key.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.VacuumExposure)?.Severity ?? 0f));
                var pp = kv.Key;
                string hs = string.Join(" ", pp.health.hediffSet.hediffs.Where(h => h.def.defName.StartsWith("CVG_") || h.def.defName.Contains("Vacuum")).Select(h => h.def.defName.Replace("CVG_Hediff_", "") + "(" + h.Severity.ToString("0.0") + "/" + h.CurStageIndex + ")"));
                Note("  t+" + sample * 10 + "s " + (pp == control ? "control" : "stage " + CompCultivator.For(pp)?.Realm?.Stage) + ": stat " + (pp.Dead ? "dead" : pp.GetStatValue(StatDefOf.VacuumResistance).ToString("0.00")) + ", vacuum here " + (pp.Spawned ? VacuumUtility.GetVacuum(pp.Position, space).ToString("0.00") : "-") + ", hediffs " + hs);
            }
            if (sample < 6) return;
            foreach (var kv in severities)
            {
                var p = kv.Key;
                float s = kv.Value.Last();
                float perSec = s / (sample * 10f);
                string toDeath = perSec > 0.0001f ? (1f / perSec).ToString("0") + " s" : "never";
                int burns = p.health.hediffSet.hediffs.Count(h => h.def == DamageDefOf.VacuumBurn.hediff);
                string who = p == control ? "control (no cultivation)" : "Golden Core stage " + CompCultivator.For(p)?.Realm?.Stage;
                Note(who + " (resistance " + p.GetStatValue(StatDefOf.VacuumResistance).ToString("0.00") + "): exposure severity " + s.ToString("0.00") + " after " + sample * 10 + " s [" + string.Join(", ", kv.Value.Select(v => v.ToString("0.00"))) + "], about " + toDeath + " until it reaches 1.0, " + (p.Dead ? "DEAD" : "alive") + ", " + burns + " burn wounds");
            }
            var cs = cultivators.Select(p => severities[p].Last()).ToList();
            Step(cs[3] <= cs[0] + 0.001f, "a higher Golden Core stage takes less vacuum exposure than the first");
            Step(severities[control].Last() >= cs[0] - 0.001f, "a person without cultivation suffers at least as much as a cultivator");
            // brecha en la sala sellada
            foreach (var p in cultivators.Concat(new[] { control })) if (p.Spawned) { p.Position = engine.Position + new IntVec3(-2, 0, 2); p.Notify_Teleported(true, true); }
            var room = breath != null ? breath.Position.GetRoom(space) : null;
            if (breath == null || room == null) { Step(false, "no sealed room with a breath formation to breach"); phase = 5; phaseTick = now; return; }
            Note("before the breach: the sealed room has " + room?.CellCount + " cells, vacuum " + (breath == null ? "n/a" : VacuumUtility.GetVacuum(breath.Position, space).ToString("0.00")));
            Thing wall = null;
            if (room != null)
                foreach (var c in room.Cells)
                    foreach (var dir in GenAdj.CardinalDirections)
                    {
                        var e = (c + dir).GetEdifice(space);
                        if (e != null && e.def.defName == "CVG_CloudRockWall") { wall = e; break; }
                    }
            if (wall == null) Step(false, "could not find a wall of the sealed room to breach");
            var flick = breath?.TryGetComp<CompFlickable>();
            if (flick != null && flick.SwitchIsOn) { flick.SwitchIsOn = !flick.SwitchIsOn; Note("breath formation switched off for the breach measurement"); }
            var domeFlick = dome?.TryGetComp<CompFlickable>();
            if (domeFlick != null && domeFlick.SwitchIsOn) { domeFlick.SwitchIsOn = false; DomeUtility.ResetCache(); Note("the dome is switched off too: it hides the real vacuum, which is what is measured next"); }
            roomVacuumBefore = VacuumUtility.GetVacuum(breath.Position, space);
            Note("with the dome off and the wall intact the room reads vacuum " + roomVacuumBefore.ToString("0.00"));
            var barrierThing = (built.rooms[0].door + delta).GetEdifice(space);
            breachWall = wall;
            Step(barrierThing?.def.defName == "CVG_QiBarrier" && roomVacuumBefore < 0.05f, "the qi barrier on the dining hall door holds the air with the dome off (door " + barrierThing?.def.defName + ", powered " + barrierThing?.TryGetComp<CompPowerTrader>()?.PowerOn + ", qi " + barrierThing?.TryGetComp<CompQiNode>()?.Powered + ")");
            if (breachWall != null) { Note("breach: a cloud rock wall of the sealed room is destroyed"); breachCell = breachWall.Position; breachWall.Destroy(); }
            phase = 4; phaseTick = now;
        }

        private void BreachResult()
        {
            if (breath == null) return;
            float v = VacuumUtility.GetVacuum(breath.Position, space);
            Note("10 s after the breach with the breath formation OFF and the dome OFF: vacuum in the room " + v.ToString("0.00"));
            Step(v > roomVacuumBefore + 0.01f, "a broken wall lets the vacuum into the room (dome off): " + roomVacuumBefore.ToString("0.00") + " -> " + v.ToString("0.00"));
            if (breachCell.IsValid && breachCell.GetEdifice(space) == null) { var wd = DefDatabase<ThingDef>.GetNamed("CVG_CloudRockWall"); var nw = ThingMaker.MakeThing(wd, wd.MadeFromStuff ? GenStuff.DefaultStuffFor(wd) : null); nw.SetFactionDirect(Faction.OfPlayer); GenSpawn.Spawn(nw, breachCell, space); Note("the wall is rebuilt"); }
        }

        private void BreathOn()
        {
            var flick = breath?.TryGetComp<CompFlickable>();
            if (flick != null && !flick.SwitchIsOn) { flick.SwitchIsOn = !flick.SwitchIsOn; Note("breath formation switched back on"); }
        }

        private void BreathResult()
        {
            if (breath == null) return;
            float v = VacuumUtility.GetVacuum(breath.Position, space);
            Note("15 s after rebuilding the wall with the breath formation ON (dome still off): vacuum in the room " + v.ToString("0.00") + " (oxygen pusher " + (breath.TryGetComp<CompOxygenPusher>() == null ? "missing" : "present") + ")");
        }

        private void BarrierOff()
        {
            var b = (built.rooms[0].door + delta).GetEdifice(space);
            var fl = b?.TryGetComp<CompFlickable>();
            float v0 = VacuumUtility.GetVacuum(breath.Position, space);
            Note("barrier test: the room (wall rebuilt, breath formation on) reads vacuum " + v0.ToString("0.00") + "; now the barrier is switched off");
            if (fl != null && fl.SwitchIsOn) fl.SwitchIsOn = false;
            // forzar la sala a aire para que la fuga sea visible: se baja el vacio de la sala y se mira si vuelve
            var room = breath.Position.GetRoom(space);
            if (room != null) room.Vacuum = 0f;
            roomVacuumBefore = room?.Vacuum ?? -1f;
        }

        private void BarrierOffResult()
        {
            float v = VacuumUtility.GetVacuum(breath.Position, space);
            Step(v > roomVacuumBefore + 0.01f, "with the barrier switched off the room leaks into the void again: " + roomVacuumBefore.ToString("0.00") + " -> " + v.ToString("0.00"));
        }

        // ---------------------------------------------------------------- percances de aterrizaje
        private void Outcomes()
        {
            var things = space.listerThings.AllThings.Where(t => t is Building && GravshipUtility.InsideFootprint(t.Position, space)).ToList();
            Gravship shipForOutcomes = null;
            try
            {
                var root = engine.Position;
                shipForOutcomes = GravshipUtility.GenerateGravship(engine);
                GravshipPlacementUtility.PlaceGravshipInMap(shipForOutcomes, shipForOutcomes.originalPosition, space, out var again);
                shipForOutcomes.Engine.ForceSubstructureDirty();
                engine = shipForOutcomes.Engine;
                Step(engine != null && engine.Spawned, "the island was lifted and set down again to test the mishaps (" + engine?.GravshipComponents.Count() + " ship components)");
            }
            catch (Exception e) { Step(false, "could not rebuild a gravship for the mishap test: " + e.GetType().Name + ": " + e.Message); return; }
            foreach (var gc2 in engine.GravshipComponents)
                Note("ship component " + gc2.parent.def.defName + " spawned=" + gc2.parent.Spawned + " map=" + (gc2.parent.Map == null ? "null" : "ok") + " thruster=" + gc2.parent.HasComp<CompGravshipThruster>());
            Note("engine.launchInfo " + (engine.launchInfo == null ? "null" : "set (pilot " + engine.launchInfo.pilot?.LabelShort + ")") + "; ship.Engine " + (shipForOutcomes.Engine == null ? "null" : "set"));
            var defs = DefDatabase<LandingOutcomeDef>.AllDefsListForReading;
            Step(defs.Count > 0, "there are landing mishaps defined (" + defs.Count + ": " + string.Join(", ", defs.Select(d => d.defName)) + ")");
            foreach (var def in defs)
            {
                int hpBefore = things.Where(t => !t.Destroyed).Sum(t => t.HitPoints);
                int alive = things.Count(t => !t.Destroyed);
                int hediffsBefore = cultivators.Sum(p => p.health.hediffSet.hediffs.Count);
                try
                {
                    engine.launchInfo = new LaunchInfo { quality = 0f, doNegativeOutcome = true, pilot = cultivators[0] };
                    def.Worker.ApplyOutcome(shipForOutcomes);
                    int hpAfter = things.Where(t => !t.Destroyed).Sum(t => t.HitPoints);
                    Note("landing mishap '" + def.defName + "' (weight " + def.weight + "): building HP " + hpBefore + " -> " + hpAfter + " (" + (alive - things.Count(t => !t.Destroyed)) + " destroyed), new hediffs on the cultivators " + (cultivators.Sum(p => p.health.hediffSet.hediffs.Count) - hediffsBefore));
                    Step(true, "mishap '" + def.defName + "' ran without errors");
                }
                catch (Exception e) { Step(false, "mishap '" + def.defName + "' threw: " + e.GetType().Name + ": " + e.Message + " @ " + string.Join(" < ", (e.StackTrace ?? "").Split((char)10).Take(3).Select(x => x.Trim()))); }
            }
            Note("takeoff itself deals no damage in Odyssey: only the landing mishap (chance by launch quality) can hurt the ship");
        }
    }
}
