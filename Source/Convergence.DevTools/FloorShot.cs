using System.IO;
using System.Linq;
using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Herramienta de revisión visual: con <c>-cvgfloorshot</c> (y <c>tools/run-test.ps1 -Visible</c>) pinta un parche de 4x4 de cada
    /// suelo construible del mod, con un colono para la escala, y guarda una captura en <c>&lt;datos&gt;/cvg_floorshot/</c>.
    /// Sin el argumento no hace nada.
    /// </summary>
    public static class FloorShot
    {
        private static int state, frameWait;
        private static string dir;

        public static void Update()
        {
            if (state == 3 || Current.ProgramState != ProgramState.Playing || Find.CurrentMap == null) return;
            if (!GenCommandLine.CommandLineArgPassed("cvgfloorshot")) { state = 3; return; }
            var map = Find.CurrentMap;
            if (state == 0)
            {
                if (Find.TickManager.TicksGame < 120) return;
                Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
                int hour = GenLocalDate.HourOfDay(map);
                if (hour < 12) Find.TickManager.DebugSetTicksGame(Find.TickManager.TicksGame + (12 - hour) * GenDate.TicksPerHour);
                dir = Path.Combine(GenFilePaths.SaveDataFolderPath, "cvg_floorshot");
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
                Directory.CreateDirectory(dir);
                var floors = DefDatabase<TerrainDef>.AllDefs.Where(t => t.defName.StartsWith("CVG_") && t.BuildableByPlayer && !t.isFoundation).OrderBy(t => t.defName).ToList();
                var c0 = map.Center + new IntVec3(-floors.Count * 5 / 2, 0, 12);
                for (int i = 0; i < floors.Count; i++)
                    foreach (var c in CellRect.CenteredOn(c0 + new IntVec3(i * 5, 0, 0), 2).Cells.Where(c => c.InBounds(map)))
                    {
                        foreach (var t in c.GetThingList(map).ToList()) if (!(t is Pawn)) t.Destroy();
                        map.roofGrid.SetRoof(c, null);
                        map.terrainGrid.SetTerrain(c, floors[i]);
                        if (c.x == c0.x + i * 5 && c.z == c0.z + 2) Log.Message($"[Convergence] FloorShot: {floors[i].defName} at x={c.x}");
                    }
                var p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayer, forceGenerateNewPawn: true, tile: map.Tile));
                GenSpawn.Spawn(p, c0 + new IntVec3(-4, 0, 0), map);
                Find.CameraDriver.SetRootPosAndSize((c0 + new IntVec3(floors.Count * 5 / 2 - 2, 0, 0)).ToVector3Shifted(), Mathf.Max(8f, floors.Count * 2.2f));
                state = 1; frameWait = 30;
                return;
            }
            if (--frameWait > 0) return;
            if (state == 1)
            {
                Find.WindowStack.TryRemove(typeof(EditWindow_Log), doCloseSound: false);
                ScreenCapture.CaptureScreenshot(Path.Combine(dir, "floors.png"));
                state = 2; frameWait = 5;
                return;
            }
            Log.Message("[Convergence] FloorShot: saved in " + dir + "\n  ALL OK");
            state = 3;
        }
    }
}
