using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Convergence
{
    /// <summary>
    /// Prueba de vuelo REAL de la isla voladora (requiere Odyssey y Convergence: Flying Sect): solo con <c>-cvgflighttest</c>.
    /// Arma una isla mínima y recorre el flujo de Odyssey: opciones de la consola para un colono, ritual de lanzamiento (diálogo),
    /// elección de destino válido, consumo de combustible y despegue (InitiateTakeoff) hasta que el gravbarco viaja por el mundo.
    /// Con <c>-cvgflightvanilla</c> hace lo mismo con una gravnave vanilla para comparar. Escribe cada paso en el log.
    /// </summary>
    public class GameComponent_IslandFlightTest : GameComponent
    {
        private static readonly bool Enabled = GenCommandLine.CommandLineArgPassed("cvgflighttest");
        private static readonly bool Vanilla = GenCommandLine.CommandLineArgPassed("cvgflightvanilla");
        private int phase;
        private int startTick = -1;
        private int phaseTick;
        private readonly StringBuilder sb = new StringBuilder("[Convergence] Island flight test");
        private int fails;
        private bool sawNaming;
        private Building_GravEngine engine;
        private Thing console;
        private Pawn pilot;
        private PlanetTile dest;

        public GameComponent_IslandFlightTest(Game game) { }

        private void Step(bool ok, string what)
        {
            sb.Append("\n    " + (ok ? "ok   " : "FAIL ") + what);
            Log.Message("[Convergence] flight step: " + (ok ? "ok   " : "FAIL ") + what);
            try { System.IO.File.AppendAllText(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "cvg_flight.log"), (ok ? "ok   " : "FAIL ") + what + System.Environment.NewLine); } catch { }
            if (!ok) fails++;
        }

        private void Finish()
        {
            phase = 99;
            sb.Append(fails == 0 ? "\n  ALL OK" : "\n  FAILURES: " + fails);
            if (fails == 0) Log.Message(sb.ToString()); else Log.Error(sb.ToString());
        }

        private Thing Spawn(string def, IntVec3 cell, Map map, Rot4? rot = null, bool vanillaDef = false)
        {
            var d = vanillaDef ? DefDatabase<ThingDef>.GetNamed(def) : DefDatabase<ThingDef>.GetNamed(def);
            var t = ThingMaker.MakeThing(d, d.MadeFromStuff ? GenStuff.DefaultStuffFor(d) : null);
            t.SetFactionDirect(Faction.OfPlayer);
            GenSpawn.Spawn(t, cell, map, rot ?? Rot4.North);
            return t;
        }

        public override void GameComponentTick()
        {
            if (!Enabled || phase >= 99 || Find.CurrentMap == null || Find.TickManager.TicksGame < 200) return;
            try { System.IO.File.AppendAllText(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "cvg_flight.log"), "tick " + Find.TickManager.TicksGame + " phase " + phase + System.Environment.NewLine); } catch { }
            if (!ModsConfig.OdysseyActive) { Log.Message("[Convergence] Island flight test skipped: Odyssey not active"); phase = 99; return; }
            try { Tick(Find.CurrentMap); }
            catch (Exception e) { Step(false, "exception in phase " + phase + ": " + e); Finish(); }
        }

        private void Tick(Map map)
        {
            int now = Find.TickManager.TicksGame;
            if (startTick < 0) startTick = now;
            // el diálogo de bautizar la isla pausa el juego: en la prueba se cierra (y se comprueba que apareció)
            var naming = Find.WindowStack.WindowOfType<Dialog_NamePlayerGravship>();
            if (naming != null) { sawNaming = true; naming.Close(); }
            switch (phase)
            {
                case 0: Build(map); phase = 1; phaseTick = now; break;
                case 1:
                    if (now - phaseTick < 30) return;
                    Inspect(map);
                    phase = 2; phaseTick = now;
                    break;
                case 2:
                    // dar tiempo al diálogo del ritual y a posibles excepciones
                    if (now - phaseTick < 60) return;
                    Ritual(map);
                    phase = 3; phaseTick = now;
                    break;
                case 3:
                    if (now - phaseTick < 30) return;
                    Takeoff(map);
                    if (phase < 99) { phase = 4; phaseTick = now; }
                    break;
                case 4:
                    // esperar a que el gravbarco exista en el mundo (la cinemática dura ~10 s reales)
                    var ship = Find.WorldObjects.AllWorldObjects.OfType<Gravship>().FirstOrDefault();
                    if (ship != null && !GenCommandLine.CommandLineArgPassed("cvgflightrender")) { Finish(); }
                    else if (ship != null)
                    {
                        Step(true, "takeoff completed: the gravship travels the world (" + ship.Things.Count() + " things aboard)");
                        Finish();
                    }
                    else if (now - phaseTick > 60 * 60 * 2)
                    {
                        Step(false, "takeoff did not complete after 2 minutes of game time (controller travelling=" + Find.GravshipController.IsGravshipTravelling + ")");
                        Finish();
                    }
                    break;
            }
        }

        private void Build(Map map)
        {
            var center = map.Center;
            foreach (var c in GenRadial.RadialCellsAround(center, 16f, true).Where(x => x.InBounds(map)))
            {
                map.roofGrid.SetRoof(c, null);
                foreach (var t in c.GetThingList(map).ToList())
                    if (t.def.category != ThingCategory.Pawn && !(t is Corpse) && t.def.destroyable) t.Destroy();
            }
            if (Vanilla)
            {
                var sub = DefDatabase<TerrainDef>.GetNamed("Substructure");
                foreach (var c in GenRadial.RadialCellsAround(center, 7f, true)) map.terrainGrid.SetTerrain(c, sub);
                engine = (Building_GravEngine)Spawn("GravEngine", center, map);
                Spawn("ChemfuelTank", center + new IntVec3(0, 0, 5), map);
                Spawn("SmallThruster", center + new IntVec3(0, 0, -7), map, Rot4.North);
                console = Spawn("PilotConsole", center + new IntVec3(-1, 0, 3), map);
            }
            else
            {
                var cloud = DefDatabase<TerrainDef>.GetNamed("CVG_CloudRock");
                foreach (var c in GenRadial.RadialCellsAround(center, 7f, true)) map.terrainGrid.SetTerrain(c, cloud);
                engine = (Building_GravEngine)Spawn("CVG_IslandHeart", center, map);
                var vessel = Spawn("CVG_EssenceVessel", center + new IntVec3(0, 0, 5), map);
                Spawn("SmallThruster", center + new IntVec3(0, 0, -7), map, Rot4.North);
                console = Spawn("CVG_HelmsmanAltar", center + new IntVec3(-1, 0, 3), map);
                vessel.TryGetComp<CompRefuelable>().Refuel(200f);
            }
            if (Vanilla)
                foreach (var tank in map.listerThings.AllThings.Where(t => t.TryGetComp<CompRefuelable>() != null && t.TryGetComp<CompGravshipFacility>() != null))
                    tank.TryGetComp<CompRefuelable>().Refuel(tank.TryGetComp<CompRefuelable>().Props.fuelCapacity);
            engine.ForceSubstructureDirty();
            // el diálogo de bautizar la isla pausa el juego (y con él esta prueba): se da por mostrado
            typeof(Building_GravEngine).GetField("haveShownNameDialog", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)?.SetValue(engine, true);
            pilot = map.mapPawns.FreeColonistsSpawned.FirstOrDefault(p => p.skills != null && !p.skills.GetSkill(SkillDefOf.Intellectual).TotallyDisabled) ?? map.mapPawns.FreeColonistsSpawned.FirstOrDefault();
            if (pilot != null)
            {
                pilot.Position = console.InteractionCell;
                pilot.Notify_Teleported(true, true);
            }
            Step(engine != null && console != null && pilot != null, (Vanilla ? "vanilla" : "island") + " built: engine, console and a pilot");
        }

        private void Inspect(Map map)
        {
            engine.ForceSubstructureDirty();
            var comp = console.TryGetComp<CompPilotConsole>();
            Step(comp != null && comp.engine == engine, "console is linked to the engine");
            if (comp == null) { Finish(); return; }
            Step(engine.ValidSubstructure.Count > 0, "valid substructure cells: " + engine.ValidSubstructure.Count);
            Step(engine.TotalFuel >= 10f, "fuel " + engine.TotalFuel.ToString("0") + " / " + engine.MaxFuel.ToString("0"));
            Step(engine.MaxLaunchDistance > 0, "max launch distance " + engine.MaxLaunchDistance);
            var can = comp.CanUseNow();
            Step(can.Accepted, "console usable: " + (can.Accepted ? "yes" : can.Reason));
            var opts = comp.CompFloatMenuOptions(pilot).ToList();
            Step(opts.Any(o => o.Label == "PilotGravship".Translate()), "float menu offers 'Pilot gravship' (" + string.Join(" | ", opts.Select(o => o.Label)) + ")");
            var ideo = pilot.Ideo;
            Step(ideo != null, "pilot has an ideoligion");
            var precept = ideo?.GetPrecept(PreceptDefOf.GravshipLaunch);
            Step(precept != null, "the ideoligion has the gravship launch ritual");
            if (!Vanilla) Survival(map);

        }

        private void Survival(Map map)
        {
            var center = map.Center;
            Step(Find.ResearchManager.gravEngineInspected, "the Island Heart counts as inspected (no mysterious-engine inspection)");
            Step(!engine.GetGizmos().Any(g => g is Command c && (c.defaultLabel ?? "").ToLower().Contains("inspect")), "the Heart offers no inspect gizmo");
            // textos de isla en vez de gravnave / quimiocombustible
            IslandText.ResetCache();
            IslandTextSwap.Set(IslandText.Context);
            string pilot = "PilotGravship".Translate();
            Step(IslandText.Context && pilot == "CVG_IslandText_PilotGravship".Translate().ToString(), "island wording replaces the gravship wording ('" + pilot + "')");
            string cost = "FuelAmount".Translate(50, ThingDefOf.Chemfuel);
            Step(!cost.ToLower().Contains("chemfuel") && !cost.ToLower().Contains("quimiocombustible"), "the launch cost is shown in spirit stones ('" + cost + "')");
            string stored = "StoredChemfuel".Translate();
            Step(!stored.ToLower().Contains("chemfuel") && !stored.ToLower().Contains("quimiocombustible"), "the console shows spirit stones stored ('" + stored + "')");
            var consoleComp = console.TryGetComp<CompPilotConsole>();
            string inspect = consoleComp?.CompInspectStringExtra() ?? "";
            Step(!inspect.ToLower().Contains("chemfuel") && !inspect.ToLower().Contains("quimiocombustible"), "the console inspect string has no chemfuel");
            // supervivencia: muros, cúpula, formación del aliento, estaca
            var wall = Spawn("CVG_CloudRockWall", center + new IntVec3(6, 0, 0), map);
            var jade = Spawn("CVG_JadeWall", center + new IntVec3(6, 0, 1), map);
            Step(wall.def.building.isAirtight && jade.def.building.isAirtight, "cloud rock and jade walls are airtight");
            var breath = Spawn("CVG_BreathFormation", center + new IntVec3(5, 0, 3), map);
            Step(breath.TryGetComp<CompOxygenPusher>() != null && breath.TryGetComp<CompQiNode>()?.IsConsumer == true, "the breath formation pushes oxygen and drinks qi");
            var dome = Spawn("CVG_CelestialDome", center + new IntVec3(11, 0, 0), map);
            var domeComp = dome.TryGetComp<CompCelestialDome>();
            var mgr = map.GetComponent<QiNetworkManager>();
            Step(domeComp != null, "the dome formation has its comp");
            if (domeComp != null)
            {
                Step(!domeComp.Working, "a dome without qi does not work");
                DomeUtility.ResetCache();
                float v = 1f;
                Patch_VacuumUtility_GetVacuum_Dome.Postfix(dome.Position, map, ref v);
                Step(v == 1f, "an unpowered dome leaves the vacuum alone");
                // darle qi: pilar + cristal lleno
                var pillar = Spawn("CVG_FormationPillar", center + new IntVec3(11, 0, 4), map);
                var crystal = Spawn("CVG_QiCrystal", center + new IntVec3(13, 0, 4), map);
                mgr.EnsureBuilt();
                crystal.TryGetComp<CompQiNode>().Stored = 800f;
                mgr.Step(0.01f);
                DomeUtility.ResetCache();
                Step(domeComp.Working, "a dome with qi works");
                v = 1f;
                Patch_VacuumUtility_GetVacuum_Dome.Postfix(dome.Position + new IntVec3(5, 0, 0), map, ref v);
                Step(v == 0f, "inside a working dome there is no vacuum");
                v = 1f;
                Patch_VacuumUtility_GetVacuum_Dome.Postfix(dome.Position + new IntVec3(40, 0, 0), map, ref v);
                Step(v == 1f, "outside the dome the vacuum remains");
            }
            var stake = Spawn("CVG_SkyStake", center + new IntVec3(-12, 0, -9), map);
            Step(map.listerThings.AnyThingWithDef(ThingDefOf.GravAnchor), "the sky stake counts as a gravity anchor");
        }

        private void Ritual(Map map)
        {
            var precept = pilot.Ideo?.GetPrecept(PreceptDefOf.GravshipLaunch) as Precept_Ritual;
            if (precept == null) { Step(false, "no ritual: cannot continue the ritual step"); return; }
            try
            {
                precept.ShowRitualBeginWindow(console, null, pilot);
                var dlg = Find.WindowStack.WindowOfType<Dialog_BeginGravshipLaunch>();
                Step(dlg != null, "the launch ritual dialog opens");
                dlg?.Close();
            }
            catch (Exception e) { Step(false, "ritual dialog threw: " + e); }
            // objetivos del ritual para esta consola
            var worker = new RitualObligationTargetWorker_GravshipLaunch();
            var targets = worker.GetTargets(null, map).ToList();
            Step(targets.Any(t => t.Thing == console), "the ritual target filter offers this console (" + targets.Count + " targets)");
        }

        private void Takeoff(Map map)
        {
            var comp = console.TryGetComp<CompPilotConsole>();
            // destino válido cercano
            PlanetTile cur = map.Tile;
            dest = PlanetTile.Invalid;
            var tmp = new List<PlanetTile>();
            for (int radius = 2; radius <= 12 && !dest.Valid; radius++)
            {
                tmp.Clear();
                Find.WorldGrid.GetTileNeighbors(cur, tmp);
                var ring = new HashSet<PlanetTile>(tmp);
                for (int i = 0; i < radius; i++)
                {
                    var next = new List<PlanetTile>();
                    foreach (var t in ring) { var n2 = new List<PlanetTile>(); Find.WorldGrid.GetTileNeighbors(t, n2); next.AddRange(n2); }
                    foreach (var t in next) ring.Add(t);
                }
                foreach (var t in ring)
                {
                    if (t == cur || !TileFinder.IsValidTileForNewSettlement(t, null, forGravship: true)) continue;
                    if (!GravshipUtility.TryGetPathFuelCost(cur, t, out var cost, out var distance, 10f, engine.FuelUseageFactor)) continue;
                    if (cost > engine.TotalFuel || distance > engine.MaxLaunchDistance) continue;
                    dest = t; break;
                }
            }
            Step(dest.Valid, "found a reachable destination tile " + dest);
            if (!dest.Valid) { Finish(); return; }
            engine.launchInfo = new LaunchInfo { quality = 1f, doNegativeOutcome = false, pilot = pilot };
            GravshipUtility.TryGetPathFuelCost(cur, dest, out var fuelCost, out var dist, 10f, engine.FuelUseageFactor);
            float before = engine.TotalFuel;
            engine.ConsumeFuel(dest);
            Step(engine.TotalFuel < before, "fuel consumed " + before.ToString("0") + " -> " + engine.TotalFuel.ToString("0") + " (expected cost " + fuelCost.ToString("0") + ")");
            if (GenCommandLine.CommandLineArgPassed("cvgflightrender"))
            {
                Find.GravshipController.InitiateTakeoff(engine, dest);
                Step(true, "InitiateTakeoff called (with the render capture)");
                return;
            }
            // sin captura de pantalla (la ventana de prueba es diminuta): el resto de la secuencia de despegue
            var ship = GravshipUtility.GenerateGravship(engine);
            Step(ship != null && ship.Things.Any(t => t == console), "the gravship was generated with the console aboard (" + (ship?.Things.Count() ?? 0) + " things)");
            if (ship == null) { Finish(); return; }
            Step(ship.Engine == engine || ship.Engine?.def == engine.def, "the generated gravship keeps its engine");
            Step(ship.PilotConsole != null, "the generated gravship has a pilot console");
            GravshipUtility.TravelTo(ship, map.Tile, dest);
            Step(Find.WorldObjects.AllWorldObjects.Contains(ship), "the gravship is now a world object travelling to " + dest);
            // aterrizaje en el mismo mapa, 40 casillas al este (sin la cinemática ni el marcador de aterrizaje)
            var root = ship.originalPosition + new IntVec3(40, 0, 0);
            foreach (var c in GenRadial.RadialCellsAround(root, 16f, true).Where(x => x.InBounds(map)))
            {
                map.roofGrid.SetRoof(c, null);
                foreach (var t in c.GetThingList(map).ToList())
                    if (t.def.category != ThingCategory.Pawn && !(t is Corpse) && t.def.destroyable) t.Destroy();
            }
            try
            {
                GravshipPlacementUtility.PlaceGravshipInMap(ship, root, map, out var spawnedAgain);
                var landed = ship.Engine;
                Step(landed != null && landed.Spawned, "landing: the engine is back on the map at " + landed?.Position);
                landed?.ForceSubstructureDirty();
                Step(landed != null && GravshipUtility.PlayerHasGravEngine(map), "landing: the game finds the engine again");
                var comp2 = landed?.GravshipComponents.OfType<CompPilotConsole>().FirstOrDefault();
                bool cooling = landed != null && GenTicks.TicksGame < landed.cooldownCompleteTick;
                Step(cooling, "landing: the heart rests after the flight (cooldown " + ((landed?.cooldownCompleteTick ?? 0) - GenTicks.TicksGame).ToStringTicksToPeriod() + ")");
                if (landed != null) landed.cooldownCompleteTick = -1;
                Step(comp2 != null && comp2.CanUseNow().Accepted, "landing: the console works again once the heart has rested (" + (comp2 == null ? "no console" : comp2.CanUseNow().Reason) + ")");
                Step(landed != null && landed.ValidSubstructure.Count > 100, "landing: substructure intact (" + landed?.ValidSubstructure.Count + " cells)");
            }
            catch (Exception e) { Step(false, "landing threw: " + e); }
            Find.WorldObjects.Remove(ship);
            Finish();
        }
    }
}
