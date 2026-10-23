using System.IO;
using System.Linq;
using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Capturas de revisión del arte (instancia de prueba, ventana visible). Sin su argumento no hacen nada.
    /// <c>-cvgnightshot</c>: las marionetas y un colono para la escala, de día, al atardecer y de noche (<c>cvg_nightshot/</c>).
    /// <c>-cvgislandshot</c>: una isla armada con sus piezas (corazón, núcleos, vasija, altar, propulsores, velas, muros) rotadas (<c>cvg_islandshot/</c>).
    /// </summary>
    public static class ArtShots
    {
        private static int state, wait, step;
        private static string dir;
        private static bool night, island, bench;

        public static void Update()
        {
            if (state == 9 || Current.ProgramState != ProgramState.Playing || Find.CurrentMap == null) return;
            if (state == 0)
            {
                night = GenCommandLine.CommandLineArgPassed("cvgnightshot");
                island = GenCommandLine.CommandLineArgPassed("cvgislandshot");
                bench = GenCommandLine.CommandLineArgPassed("cvgbenchshot") || GenCommandLine.TryGetCommandLineArg("cvgbenchshot", out _);
                if (!night && !island && !bench) { state = 9; return; }
                state = 1;
            }
            var map = Find.CurrentMap;
            if (state == 1)
            {
                if (Find.TickManager.TicksGame < 120) return;
                Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
                dir = Path.Combine(GenFilePaths.SaveDataFolderPath, bench ? "cvg_benchshot" : night ? "cvg_nightshot" : "cvg_islandshot");
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
                Directory.CreateDirectory(dir);
                // El estado avanza ANTES de colocar: si algo falla, la escena no se vuelve a armar en cada cuadro
                // (antes un def faltante repetía la colocación y llenaba el mapa de colonos).
                state = 2; wait = 40; step = 0;
                try
                {
                    if (bench) PlaceBenches(map); else if (night) PlacePuppets(map); else PlaceIsland(map);
                }
                catch (System.Exception e)
                {
                    Log.Error("[Convergence] Art shots: " + e + "\n  FAILURES: 1");
                    state = 9;
                    return;
                }
                SetHour(map, 12);
                return;
            }
            if (--wait > 0) return;
            foreach (var w in Find.WindowStack.Windows.OfType<EditWindow_Log>().ToList()) w.Close(false);
            if (night)
            {
                int[] hours = { 12, 19, 23, 3 };
                if (step < hours.Length)
                {
                    if (state == 2) { SetHour(map, hours[step]); state = 3; wait = 25; return; }
                    ScreenCapture.CaptureScreenshot(Path.Combine(dir, $"puppets_{hours[step]:D2}h.png"));
                    step++; state = 2; wait = 10; return;
                }
            }
            else if (step < 1)
            {
                ScreenCapture.CaptureScreenshot(Path.Combine(dir, bench ? "benches.png" : "island.png"));
                step++; wait = 20; return;
            }
            else if (bench && step < 2)
            {
                Find.CameraDriver.SetRootPosAndSize((map.Center + new IntVec3(0, 0, WallRowZ)).ToVector3Shifted(), 7f);
                if (state == 2) { state = 3; wait = 20; return; }
                ScreenCapture.CaptureScreenshot(Path.Combine(dir, "walls.png"));
                step++; wait = 20; return;
            }
            Log.Message("[Convergence] Art shots saved in " + dir + "\n  ALL OK");
            state = 9;
            Root.Shutdown();
        }

        private static void SetHour(Map map, int hour)
        {
            int cur = GenLocalDate.HourOfDay(map);
            int add = ((hour - cur) + 24) % 24 * GenDate.TicksPerHour;
            if (add > 0) Find.TickManager.DebugSetTicksGame(Find.TickManager.TicksGame + add);
        }

        private static void Clear(Map map, CellRect rect, TerrainDef terrain)
        {
            foreach (var c in rect)
            {
                if (!c.InBounds(map)) continue;
                foreach (var t in c.GetThingList(map).ToList()) if (!(t is Pawn) && t.def.destroyable) t.Destroy();
                map.roofGrid.SetRoof(c, null);
                if (terrain != null) map.terrainGrid.SetTerrain(c, terrain);
            }
        }

        private static void PlacePuppets(Map map)
        {
            var center = map.Center;
            Clear(map, CellRect.CenteredOn(center, 14), TerrainDefOf.Soil);
            map.fogGrid.ClearAllFog();
            string[] kinds = { "CVG_Puppet_Wood", "CVG_Puppet_Mason", "CVG_Puppet_Miner", "CVG_Puppet_Bronze", "CVG_Puppet_Blade", "CVG_Puppet_Jade" };
            int x = -7;
            Spawn(map, PawnKindDefOf.Colonist, center + new IntVec3(x - 2, 0, 0), Rot4.South);
            foreach (var k in kinds)
            {
                var kind = DefDatabase<PawnKindDef>.GetNamedSilentFail(k);
                if (kind == null) { Log.Warning("[Convergence] Art shots: no PawnKindDef " + k); continue; }
                Spawn(map, kind, center + new IntVec3(x, 0, 0), Rot4.South);
                Spawn(map, kind, center + new IntVec3(x, 0, -3), Rot4.East);
                Spawn(map, kind, center + new IntVec3(x, 0, 3), Rot4.North);
                x += 3;
            }
            Find.CameraDriver.SetRootPosAndSize(center.ToVector3Shifted() + new Vector3(1.5f, 0, 0), 11f);
        }

        private static void Spawn(Map map, PawnKindDef kind, IntVec3 cell, Rot4 rot)
        {
            var p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, Faction.OfPlayer, forceGenerateNewPawn: true, tile: map.Tile));
            GenSpawn.Spawn(p, cell, map, rot);
            p.Rotation = rot;
            if (rot == Rot4.South && kind.defName == "CVG_Puppet_Mason") Log.Message("[Convergence] Puppet inspect string: " + p.GetInspectString().Replace(((char)10).ToString(), " | "));
        }

        /// <summary>Cada mesa de trabajo del mod (en sus rotaciones posibles) con un colono parado en su celda de trabajo, para ver de qué lado se trabaja.</summary>
        private static void PlaceBenches(Map map)
        {
            var center = map.Center;
            Clear(map, CellRect.CenteredOn(center, 20), TerrainDefOf.Soil);
            map.fogGrid.ClearAllFog();
            var defs = DefDatabase<ThingDef>.AllDefs.Where(d => d.defName.StartsWith("CVG_") && d.category == ThingCategory.Building && typeof(Building_WorkTable).IsAssignableFrom(d.thingClass)).OrderBy(d => d.defName).ToList();
            // -cvgbenchshot=CVG_A,CVG_B: solo esas mesas (y la cámara sobre ellas).
            bool filtered = GenCommandLine.TryGetCommandLineArg("cvgbenchshot", out string only) && !only.NullOrEmpty();
            if (filtered)
            {
                var names = only.Split(',').Select(s => s.Trim()).ToList();
                defs = defs.Where(d => names.Contains(d.defName)).ToList();
            }
            int x = -18, z = 8;
            foreach (var d in defs)
            {
                foreach (var rot in d.rotatable ? new[] { Rot4.South, Rot4.East, Rot4.North, Rot4.West } : new[] { Rot4.North })
                {
                    var cell = center + new IntVec3(x, 0, z);
                    var t = ThingMaker.MakeThing(d, d.MadeFromStuff ? GenStuff.DefaultStuffFor(d) : null);
                    t.SetFaction(Faction.OfPlayer);
                    GenSpawn.Spawn(t, cell, map, rot);
                    var ic = t.InteractionCell;
                    var p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayer, forceGenerateNewPawn: true, tile: map.Tile));
                    GenSpawn.Spawn(p, ic, map, Rot4.North);
                    if (p.drafter != null) p.drafter.Drafted = true;
                    // -cvgequip=CVG_X: cada colono lleva esa arma (para ver cómo se sostiene en las cuatro direcciones).
                    if (GenCommandLine.TryGetCommandLineArg("cvgequip", out string weapon) && DefDatabase<ThingDef>.GetNamedSilentFail(weapon) is ThingDef wd)
                    {
                        p.equipment?.DestroyAllEquipment();
                        p.equipment?.AddEquipment((ThingWithComps)ThingMaker.MakeThing(wd, wd.MadeFromStuff ? GenStuff.DefaultStuffFor(wd) : null));
                        p.Drawer?.renderer?.SetAllGraphicsDirty();
                        Log.Message("[Convergence] Art shots: " + p.LabelShort + " equipped " + p.equipment?.Primary?.def.defName + ", drafted " + p.Drafted);
                        p.Rotation = rot;
                    }
                    Log.Message("[Convergence] bench " + d.defName + " rot " + rot + " size " + d.size + " pos " + cell + " interaction cell offset " + (ic - cell) + " (" + (d.rotatable ? "rotatable" : "fixed") + ")");
                    x += Mathf.Max(d.size.x, d.size.z) + 2;
                    if (x > 18) { x = -18; z -= 6; }
                }
            }
            if (filtered)
            {
                if (GenCommandLine.TryGetCommandLineArg("cvgequip", out string w2) && DefDatabase<ThingDef>.GetNamedSilentFail(w2) is ThingDef wd2)
                    GenSpawn.Spawn(ThingMaker.MakeThing(wd2, wd2.MadeFromStuff ? GenStuff.DefaultStuffFor(wd2) : null), center + new IntVec3(-18, 0, 4), map);
                Find.CameraDriver.SetRootPosAndSize((center + new IntVec3(-18 + defs.Sum(d => (d.rotatable ? 4 : 1) * (Mathf.Max(d.size.x, d.size.z) + 2)) / 2, 0, 5)).ToVector3Shifted(), 12f);
                return;
            }
            // fila de muros: la empalizada y el muro bajo con distintos materiales, junto a un muro común de vanilla, para ver que no se tiñen
            Clear(map, new CellRect(center.x - 12, center.z + WallRowZ - 3, 25, 7), TerrainDefOf.Soil);
            void Wall(string def, string stuff, int x0, int count, int dz = 0)
            {
                var d = DefDatabase<ThingDef>.GetNamedSilentFail(def);
                if (d == null) return;
                for (int i = 0; i < count; i++)
                {
                    var t = ThingMaker.MakeThing(d, stuff != null ? DefDatabase<ThingDef>.GetNamed(stuff) : null);
                    t.SetFaction(Faction.OfPlayer);
                    GenSpawn.Spawn(t, center + new IntVec3(x0 + i, 0, WallRowZ + dz + (stuff == "BlocksMarble" ? -2 : 0)), map);
                }
            }
            Wall("CVG_BambooPalisade", "WoodLog", -10, 5);
            Wall("CVG_LowStoneWall", "BlocksGranite", -3, 5);
            Wall("CVG_LowStoneWall", "BlocksMarble", -3, 5);
            Wall("Wall", "BlocksGranite", 4, 5);
            Wall("Barricade", "WoodLog", -10, 5, -2);
            Find.CameraDriver.SetRootPosAndSize(center.ToVector3Shifted(), 14f);
        }

        private const int WallRowZ = 22;

        private static void PlaceIsland(Map map)
        {
            var center = map.Center;
            var cloud = DefDatabase<TerrainDef>.GetNamedSilentFail("CVG_CloudRock");
            Clear(map, CellRect.CenteredOn(center, 16), TerrainDefOf.Soil);
            map.fogGrid.ClearAllFog();
            Clear(map, CellRect.CenteredOn(center, 11), cloud);
            void Put(string def, IntVec3 off, Rot4 rot)
            {
                var d = DefDatabase<ThingDef>.GetNamedSilentFail(def);
                if (d == null) { Log.Warning("[Convergence] Art shots: no def " + def); return; }
                var t = ThingMaker.MakeThing(d, d.MadeFromStuff ? GenStuff.DefaultStuffFor(d) : null);
                t.SetFaction(Faction.OfPlayer);
                GenSpawn.Spawn(t, center + off, map, rot, WipeMode.Vanish);
            }
            Put("CVG_IslandHeart", new IntVec3(0, 0, 0), Rot4.North);
            foreach (var o in new[] { (-3, 3), (3, 3), (-3, -3), (3, -3) }) Put("CVG_GravityCoreMount", new IntVec3(o.Item1, 0, o.Item2), Rot4.North);
            Put("CVG_EssenceVessel", new IntVec3(-8, 0, 0), Rot4.North);
            Put("CVG_HelmsmanAltar", new IntVec3(0, 0, 6), Rot4.North);
            Put("CVG_HelmsmanAltar", new IntVec3(8, 0, 6), Rot4.East);
            Put("SmallThruster", new IntVec3(-4, 0, -8), Rot4.South);
            Put("SmallThruster", new IntVec3(-1, 0, -8), Rot4.South);
            Put("SmallThruster", new IntVec3(8, 0, -2), Rot4.East);
            Put("LargeThruster", new IntVec3(3, 0, -8), Rot4.South);
            Put("LargeThruster", new IntVec3(6, 0, -8), Rot4.South);
            Put("LargeThruster", new IntVec3(8, 0, 1), Rot4.East);
            for (int i = -2; i <= 2; i++) { Put("CVG_CloudRockWall", new IntVec3(i, 0, 9), Rot4.North); Put("CVG_JadeWall", new IntVec3(i, 0, -10), Rot4.North); }
            Put("CVG_BreathFormation", new IntVec3(-6, 0, 6), Rot4.North);
            Put("CVG_ArkProtectiveFormation", new IntVec3(-8, 0, 8), Rot4.North);
            Find.CameraDriver.SetRootPosAndSize(center.ToVector3Shifted(), 15f);
        }
    }
}
