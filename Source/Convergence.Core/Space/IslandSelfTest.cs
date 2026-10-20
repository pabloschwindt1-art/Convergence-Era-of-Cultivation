using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Convergence
{
    /// <summary>Autotest de la isla voladora (mod Convergence: Flying Sect, requiere Odyssey): solo con <c>-cvgislandtest</c> (y un mapa, p. ej. -quicktest).</summary>
    public class GameComponent_IslandTest : GameComponent
    {
        private static readonly bool Enabled = GenCommandLine.CommandLineArgPassed("cvgislandtest");
        private bool done;
        private readonly StringBuilder sb = new StringBuilder("[Convergence] Flying island self-test");
        private int fails;
        private readonly List<Thing> spawned = new List<Thing>();

        public GameComponent_IslandTest(Game game) { }

        private void Check(bool ok, string what)
        {
            sb.Append("\n    " + (ok ? "ok   " : "FAIL ") + what);
            if (!ok) fails++;
        }

        public override void GameComponentTick()
        {
            if (!Enabled || done || Find.CurrentMap == null || Find.TickManager.TicksGame < 200) return;
            done = true;
            try { Run(Find.CurrentMap); }
            catch (Exception e) { Check(false, "exception: " + e); }
            foreach (var t in spawned) if (!t.Destroyed) t.Destroy();
            sb.Append(fails == 0 ? "\n  ALL OK" : "\n  FAILURES: " + fails);
            if (fails == 0) Log.Message(sb.ToString()); else Log.Error(sb.ToString());
        }

        private Thing Spawn(string def, IntVec3 cell, Map map, Rot4? rot = null)
        {
            var d = DefDatabase<ThingDef>.GetNamed(def);
            var t = ThingMaker.MakeThing(d, d.MadeFromStuff ? GenStuff.DefaultStuffFor(d) : null);
            t.SetFactionDirect(Faction.OfPlayer);
            GenSpawn.Spawn(t, cell, map, rot ?? Rot4.North);
            spawned.Add(t);
            return t;
        }

        private static float Stat(Thing t, string stat) => t.GetStatValue(DefDatabase<StatDef>.GetNamed(stat));

        private void Run(Map map)
        {
            if (!ModsConfig.OdysseyActive || DefDatabase<ThingDef>.GetNamedSilentFail("CVG_IslandHeart") == null)
            {
                sb.Append("\n  (Odyssey or the Flying Sect mod not loaded: skipped)");
                return;
            }
            var cloud = DefDatabase<TerrainDef>.GetNamed("CVG_CloudRock");
            Check(cloud.IsSubstructure && cloud.isFoundation, "cloud rock counts as gravship substructure");
            // un hueco despejado en el centro del mapa (se retiran plantas, rocas, edificios y techos de la zona de prueba)
            var center = map.Center;
            foreach (var c in GenRadial.RadialCellsAround(center, 14f, true).Where(x => x.InBounds(map)))
            {
                map.roofGrid.SetRoof(c, null);
                foreach (var t in c.GetThingList(map).ToList())
                    if (t.def.category != ThingCategory.Pawn && !(t is Corpse) && t.def.destroyable) t.Destroy();
            }
            Check(center.IsValid, "cleared a spot for the island");
            var oldTerrain = new Dictionary<IntVec3, TerrainDef>();
            foreach (var c in GenRadial.RadialCellsAround(center, 7f, true))
            {
                oldTerrain[c] = map.terrainGrid.TerrainAt(c);
                map.terrainGrid.SetTerrain(c, cloud);
            }
            try
            {
                var heart = (Building_GravEngine)Spawn("CVG_IslandHeart", center, map);
                Check(heart != null, "Island Heart spawns as a grav engine (same class)");
                heart.ForceSubstructureDirty();
                Check(heart.ValidSubstructureAt(center + new IntVec3(4, 0, 0)), "cloud rock around the heart is valid substructure");
                Check(GravshipUtility.GetPlayerGravEngine_NewTemp(map) == heart && GravshipUtility.PlayerHasGravEngine(map), "the game finds the Island Heart as the player's grav engine");
                Check(map.listerThings.ThingsOfDef(ThingDefOf.GravEngine).Contains(heart), "ThingsOfDef(GravEngine) includes the heart");
                float baseSupport = Stat(heart, "SubstructureSupport");
                Check(baseSupport >= 800f, "base substructure support " + baseSupport.ToString("0"));

                var mountCells = new[] { new IntVec3(-5, 0, 3), new IntVec3(-5, 0, 1), new IntVec3(-4, 0, -1), new IntVec3(-4, 0, -3), new IntVec3(4, 0, 3), new IntVec3(4, 0, 1), new IntVec3(4, 0, -1), new IntVec3(4, 0, -3) };
                Thing firstMount = null;
                int n = 0;
                foreach (var off in mountCells)
                {
                    var m = Spawn("CVG_GravityCoreMount", center + off, map);
                    firstMount ??= m;
                    n++;
                    heart.ForceSubstructureDirty();
                    float s = Stat(heart, "SubstructureSupport");
                    if (n == 2) Check(s >= baseSupport + 499f, "two core mounts raise support to " + s.ToString("0"));
                    if (n == 8) Check(s <= baseSupport + 6 * 250f + 1f && s >= baseSupport + 6 * 250f - 1f, "eight mounts: capped at six (" + s.ToString("0") + ")");
                }

                var vessel = Spawn("CVG_EssenceVessel", center + new IntVec3(0, 0, 5), map);
                vessel.TryGetComp<CompRefuelable>().Refuel(120f);
                var thruster = Spawn("SmallThruster", center + new IntVec3(0, 0, -7), map, Rot4.North);
                var sail = Spawn("SmallThruster", center + new IntVec3(3, 0, -6), map, Rot4.North);
                var altar = Spawn("CVG_HelmsmanAltar", center + new IntVec3(-1, 0, 3), map);
                heart.ForceSubstructureDirty();
                Check(heart.TotalFuel >= 119f, "spirit stones in the vessel count as gravship fuel (" + heart.TotalFuel.ToString("0") + ")");
                float range = Stat(heart, "GravshipRange");
                Check(range >= 20f, "two vanilla thrusters on the Heart give range " + range.ToString("0"));
                var shield = Spawn("CVG_ArkProtectiveFormation", center + new IntVec3(4, 0, -1), map);
                heart.ForceSubstructureDirty();
                Check(shield.TryGetComp<CompGravshipFacility>()?.engine == heart && shield.TryGetComp<CompQiNode>()?.IsConsumer == true, "the ark protective formation links to the heart and drinks qi, not electricity");
                Check(OrbitalQi.BonusFor(map) == 0f, "no orbital qi bonus on a normal planet map");
                var console = altar.TryGetComp<CompPilotConsole>();
                Check(console != null && console.engine == heart, "the helmsman's altar is linked to the heart");
                var missing = heart.GetType().GetField("missingComponents", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(heart) as List<GravshipComponentTypeDef>;
                AcceptanceReport can = console != null ? heart.CanLaunch(console) : AcceptanceReport.WasRejected;
                Check(can.Accepted, "launch check: " + (can.Accepted ? "ready to launch" : can.Reason));
                // el parche de la consola: una captura del gravbarco la reconoce
                try
                {
                    var ship = new Gravship(heart);
                    Check(ship.PilotConsole == altar, "a captured gravship recognizes the helmsman's altar as its console");
                }
                catch (Exception e) { Check(false, "capturing the gravship: " + e.Message); }
            }
            finally
            {
                foreach (var t in spawned.ToList()) if (!t.Destroyed) t.Destroy();
                foreach (var kv in oldTerrain) map.terrainGrid.SetTerrain(kv.Key, kv.Value);
            }
        }
    }
}
