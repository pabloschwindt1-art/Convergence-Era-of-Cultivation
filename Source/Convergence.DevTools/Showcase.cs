using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Herramienta de revisión visual: con <c>-cvgshowcase</c> (y <c>tools/run-test.ps1 -Visible</c>) coloca en el mapa de prueba cada
    /// edificio y planta del mod (los rotables en sur y este), con colonos al lado para la escala, recorre la grilla con la cámara y
    /// guarda capturas de pantalla reales en <c>&lt;datos&gt;/cvg_showcase/</c>, junto con un JSON con la posición en pantalla de
    /// cada pieza. Sin el argumento no hace nada.
    /// </summary>
    public static class Showcase
    {
        private static int state;          // 0 sin empezar, 1 colocado, 2 capturando, 3 terminado
        private static int frameWait;
        private static int block;
        private static bool captured;
        private static readonly List<(ThingDef def, IntVec3 cell, Rot4 rot)> placed = new List<(ThingDef, IntVec3, Rot4)>();
        private static readonly List<CellRect> blocks = new List<CellRect>();
        private static string dir;

        public static void Update()
        {
            if (state == 3 || Current.ProgramState != ProgramState.Playing || Find.CurrentMap == null) return;
            if (!GenCommandLine.CommandLineArgPassed("cvgshowcase")) { state = 3; return; }
            var map = Find.CurrentMap;
            if (state == 0)
            {
                if (Find.TickManager.TicksGame < 120) return;
                Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
                // A pleno día para ver los colores reales (el quicktest arranca al amanecer).
                int hour = GenLocalDate.HourOfDay(map);
                if (hour < 12) Find.TickManager.DebugSetTicksGame(Find.TickManager.TicksGame + (12 - hour) * GenDate.TicksPerHour);
                dir = Path.Combine(GenFilePaths.SaveDataFolderPath, "cvg_showcase");
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
                Directory.CreateDirectory(dir);
                Place(map);
                state = 1; frameWait = 30;
                return;
            }
            if (--frameWait > 0) return;
            if (state == 1) { state = 2; block = 0; Aim(); frameWait = 20; return; }
            if (state == 2)
            {
                // ScreenCapture escribe al final del cuadro: si se mueve la cámara en el mismo cuadro, la captura sale corrida.
                if (!captured)
                {
                    // la ventana del log se reabre con cada error: se cierra justo antes de capturar
                    foreach (var w in Find.WindowStack.Windows.OfType<EditWindow_Log>().ToList()) w.Close(false);
                    Capture(map); captured = true; frameWait = 5; return;
                }
                captured = false;
                block++;
                if (block >= blocks.Count)
                {
                    Log.Message($"[Convergence] Showcase: {placed.Count} piece(s), {blocks.Count} screenshot(s) in {dir}\n  ALL OK");
                    state = 3;
                    return;
                }
                Aim(); frameWait = 20;
            }
        }

        private static void Place(Map map)
        {
            var defs = DefDatabase<ThingDef>.AllDefs
                .Where(d => d.defName.StartsWith("CVG_") && (d.category == ThingCategory.Building && (d.BuildableByPlayer || d.building?.isAttachment == true || d.graphicData != null && !d.IsFrame && !d.IsBlueprint && !(d.building?.isNaturalRock ?? false) && d.mineable == false) || d.category == ThingCategory.Plant))
                .OrderBy(d => d.category == ThingCategory.Plant).ThenBy(d => d.modContentPack?.Name).ThenBy(d => d.defName).ToList();
            var cloudRock = DefDatabase<TerrainDef>.GetNamedSilentFail("CVG_CloudRock");
            var soil = TerrainDefOf.Soil;
            int x0 = 5, z = map.Size.z - 8, x = x0, rowH = 0;
            const int blockW = 24, blockH = 14;
            // Primero la distribución, después se limpia toda la zona (roca, ruinas, techos, niebla) y recién ahí se coloca.
            var layout = new List<(ThingDef def, IntVec3 cell, Rot4 rot)>();
            foreach (var def in defs)
            {
                var size = def.size;
                bool rot = def.rotatable && def.category == ThingCategory.Building;
                int w = (rot ? 2 * size.x + size.z + 6 : size.x + 2); // sur, este y norte; +2 de aire entre copias (pegadas se borraban)
                int h = Mathf.Max(size.x, size.z) + 3;
                if (x + w > map.Size.x - 5) { x = x0; z -= rowH; rowH = 0; }
                if (z - h < 5) break;
                rowH = Mathf.Max(rowH, h);
                layout.Add((def, new IntVec3(x + size.x / 2, 0, z - size.z / 2 - 1), Rot4.South));
                if (rot)
                {
                    layout.Add((def, new IntVec3(x + size.x + 2 + size.z / 2, 0, z - size.x / 2 - 1), Rot4.East));
                    layout.Add((def, new IntVec3(x + size.x + size.z + 4 + size.x / 2, 0, z - size.z / 2 - 1), Rot4.North));
                }
                x += w;
            }
            // Pawns propios no humanos (marionetas, bestias): sur, este y norte de cada uno, en filas debajo de los edificios.
            var kinds = DefDatabase<PawnKindDef>.AllDefs
                .Where(k => k.defName.StartsWith("CVG_") && k.race?.race != null && !k.race.race.Humanlike)
                .OrderBy(k => k.defName).ToList();
            var pawnLayout = new List<(PawnKindDef kind, IntVec3 cell, Rot4 rot)>();
            if (kinds.Count > 0) { z -= rowH; x = x0; rowH = 4; }
            foreach (var k in kinds)
            {
                if (x + 8 > map.Size.x - 5) { x = x0; z -= rowH; }
                if (z - rowH < 5) break;
                // un colono al principio de cada fila, para comparar la escala
                if (x == x0) pawnLayout.Add((PawnKindDefOf.Colonist, new IntVec3(x0 - 2, 0, z - 2), Rot4.South));
                for (int i = 0; i < 3; i++) pawnLayout.Add((k, new IntVec3(x + i * 2, 0, z - 2), i == 0 ? Rot4.South : i == 1 ? Rot4.East : Rot4.North));
                x += 8;
            }
            // Objetos propios: uno suelto y una pila llena de cada uno (para ver las texturas _a y _c), en filas debajo de los pawns.
            var items = DefDatabase<ThingDef>.AllDefs
                .Where(d => d.defName.StartsWith("CVG_") && d.category == ThingCategory.Item && d.graphicData != null && !d.IsCorpse
                            && !d.IsBlueprint && !d.IsFrame && d.thingClass != typeof(MinifiedThing))
                .OrderBy(d => d.modContentPack?.Name).ThenBy(d => d.defName).ToList();
            var itemLayout = new List<(ThingDef def, IntVec3 cell, int count)>();
            if (items.Count > 0) { z -= rowH; x = x0; rowH = 3; }
            foreach (var d in items)
            {
                if (x + 3 > map.Size.x - 5) { x = x0; z -= rowH; }
                if (z - rowH < 5) break;
                itemLayout.Add((d, new IntVec3(x, 0, z - 1), 1));
                if (d.stackLimit > 1) itemLayout.Add((d, new IntVec3(x + 1, 0, z - 1), d.stackLimit));
                x += 3;
            }
            int clearMinZ = Mathf.Max(0, z - rowH - 2);
            foreach (var c in CellRect.FromLimits(0, clearMinZ, map.Size.x - 1, map.Size.z - 1))
            {
                foreach (var t in c.GetThingList(map).ToList()) if (!(t is Pawn)) t.Destroy();
                map.roofGrid.SetRoof(c, null);
                map.terrainGrid.SetTerrain(c, soil);
            }
            map.fogGrid.ClearAllFog();
            foreach (var (def, cell, r) in layout) Spawn(map, def, cell, r, cloudRock, soil);
            foreach (var (kind, cell, r) in pawnLayout)
            {
                try
                {
                    var p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, Faction.OfPlayer, forceGenerateNewPawn: true, tile: map.Tile));
                    GenSpawn.Spawn(p, cell, map, r);
                    p.Rotation = r;
                    placed.Add((kind.race, cell, r));
                }
                catch (System.Exception e) { Log.Warning($"[Convergence] Showcase: could not spawn {kind.defName}: {e.Message}"); }
            }
            foreach (var (def, cell, count) in itemLayout)
            {
                try
                {
                    var thing = ThingMaker.MakeThing(def, def.MadeFromStuff ? GenStuff.DefaultStuffFor(def) : null);
                    thing.stackCount = count;
                    GenSpawn.Spawn(thing, cell, map, WipeMode.Vanish);
                    thing.SetForbidden(true, false);
                    placed.Add((def, cell, Rot4.South));
                }
                catch (System.Exception e) { Log.Warning($"[Convergence] Showcase: could not place item {def.defName}: {e.Message}"); }
            }
            // un colono cada 10 casillas en la primera columna para la escala
            foreach (var p in map.mapPawns.FreeColonistsSpawned.ToList().Take(1))
                for (int zz = map.Size.z - 9; zz > 5; zz -= 10)
                {
                    var c = new IntVec3(2, 0, zz);
                    if (c.InBounds(map)) { var copy = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayer, forceGenerateNewPawn: true, tile: map.Tile)); GenSpawn.Spawn(copy, c, map); }
                }
            // bloques de captura sobre la zona usada
            int minZ = placed.Count > 0 ? placed.Min(t => t.cell.z) - 3 : 0;
            for (int bz = map.Size.z - 4; bz > minZ; bz -= blockH)
                for (int bx = 0; bx < map.Size.x; bx += blockW)
                {
                    var r = new CellRect(bx, bz - blockH, blockW, blockH);
                    if (placed.Any(t => r.Contains(t.cell))) blocks.Add(r);
                }
        }

        private static void Spawn(Map map, ThingDef def, IntVec3 center, Rot4 rot, TerrainDef cloudRock, TerrainDef soil)
        {
            var rect = GenAdj.OccupiedRect(center, rot, def.size).ExpandedBy(1);
            foreach (var c in rect)
            {
                if (!c.InBounds(map)) return;
                foreach (var t in c.GetThingList(map).ToList()) if (!(t is Pawn)) t.Destroy();
                map.roofGrid.SetRoof(c, null);
                bool needsSub = def.terrainAffordanceNeeded?.defName == "Substructure";
                map.terrainGrid.SetTerrain(c, needsSub && cloudRock != null ? cloudRock : soil);
            }
            try
            {
                // lo que va colgado (rollos, pinturas) necesita una pared en la casilla de enfrente
                if (def.building?.isAttachment == true)
                    GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.Wall, ThingDefOf.BlocksGranite), center + rot.FacingCell, map, WipeMode.Vanish);   // como Placeworker_AttachedToWall: el muro va hacia donde mira
                var thing = ThingMaker.MakeThing(def, def.MadeFromStuff ? GenStuff.DefaultStuffFor(def) : null);
                if (thing is Plant plant) plant.Growth = 1f;
                if (def.category == ThingCategory.Building) thing.SetFaction(Faction.OfPlayer);
                GenSpawn.Spawn(thing, center, map, rot, WipeMode.Vanish);
                placed.Add((def, center, rot));
            }
            catch (System.Exception e)
            {
                Log.Warning($"[Convergence] Showcase: could not place {def.defName}: {e.Message}");
            }
        }

        private static void Aim()
        {
            // La ventana del log de depuración se abre sola con cualquier error y tapa las capturas.
            Find.WindowStack.TryRemove(typeof(EditWindow_Log), doCloseSound: false);
            var r = blocks[block];
            Find.CameraDriver.SetRootPosAndSize(r.CenterVector3, r.Height / 2f + 1f);
        }

        private static void Capture(Map map)
        {
            var r = blocks[block];
            string file = Path.Combine(dir, $"block_{block:D2}.png");
            ScreenCapture.CaptureScreenshot(file);
            var cam = Find.Camera;
            var sb = new StringBuilder("[");
            bool first = true;
            foreach (var t in placed.Where(t => r.Contains(t.cell)))
            {
                var sp = cam.WorldToScreenPoint(t.cell.ToVector3Shifted());
                int sx = Mathf.RoundToInt(sp.x), sy = Mathf.RoundToInt(Screen.height - sp.y);
                string rot = t.rot == Rot4.South ? "South" : t.rot == Rot4.East ? "East" : t.rot == Rot4.North ? "North" : "West";
                sb.Append(first ? "" : ",").Append("{\"def\":\"").Append(t.def.defName).Append("\",\"rot\":\"").Append(rot)
                  .Append("\",\"x\":").Append(sx).Append(",\"y\":").Append(sy).Append("}");
                first = false;
            }
            sb.Append("]");
            File.WriteAllText(Path.Combine(dir, $"block_{block:D2}.json"), sb.ToString());
        }
    }
}
