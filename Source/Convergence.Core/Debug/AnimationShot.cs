using System.Collections.Generic;
using System.IO;
using System.Linq;
using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Herramienta de revisión visual: con <c>-cvganimshot</c> (y <c>tools/run-test.ps1 -Visible</c>) coloca cada criatura del mod
    /// cuya raza tiene <c>startingAnimation</c> (marionetas, etc.), comprueba que la animación esté corriendo y, con el juego en pausa,
    /// adelanta el reloj para capturar 3 momentos del ciclo en <c>&lt;datos&gt;/cvg_animshot/</c>. Sin el argumento no hace nada.
    /// </summary>
    public static class AnimationShot
    {
        private static int state;      // 0 sin empezar, 1 colocado, 2 capturando, 3 terminado
        private static int frameWait, shot;
        private static bool captured;
        private static readonly List<Pawn> pawns = new List<Pawn>();
        private static string dir;
        private static IntVec3 center;

        public static void Update()
        {
            if (state == 3 || Current.ProgramState != ProgramState.Playing || Find.CurrentMap == null) return;
            if (!GenCommandLine.CommandLineArgPassed("cvganimshot")) { state = 3; return; }
            var map = Find.CurrentMap;
            if (state == 0)
            {
                if (Find.TickManager.TicksGame < 120) return;
                Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
                int hour = GenLocalDate.HourOfDay(map);
                if (hour < 12) Find.TickManager.DebugSetTicksGame(Find.TickManager.TicksGame + (12 - hour) * GenDate.TicksPerHour);
                dir = Path.Combine(GenFilePaths.SaveDataFolderPath, "cvg_animshot");
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
                Directory.CreateDirectory(dir);
                Place(map);
                state = 1; frameWait = 30;
                return;
            }
            if (--frameWait > 0) return;
            if (state == 1)
            {
                Find.WindowStack.TryRemove(typeof(EditWindow_Log), doCloseSound: false);
                Find.CameraDriver.SetRootPosAndSize(center.ToVector3Shifted(), 6f);
                state = 2; shot = 0; frameWait = 20;
                return;
            }
            if (!captured)
            {
                foreach (var w in Find.WindowStack.Windows.OfType<EditWindow_Log>().ToList()) w.Close(false);
                ScreenCapture.CaptureScreenshot(Path.Combine(dir, $"frame_{shot}.png"));
                captured = true; frameWait = 5;
                return;
            }
            captured = false;
            if (++shot >= 3)
            {
                Report();
                state = 3;
                return;
            }
            // un tercio de cada ciclo: el reloj avanza en pausa y la animación se dibuja en otro momento
            Find.TickManager.DebugSetTicksGame(Find.TickManager.TicksGame + 120);
            frameWait = 10;
        }

        private static void Place(Map map)
        {
            var kinds = DefDatabase<PawnKindDef>.AllDefs
                .Where(k => k.race?.race?.startingAnimation != null && k.race.defName.StartsWith("CVG_"))
                .OrderBy(k => k.defName).ToList();
            center = map.Center + new IntVec3(0, 0, 10);   // lejos de los colonos y la carga del inicio
            int x = center.x - kinds.Count, i = 0;
            foreach (var kind in kinds)
            {
                var cell = new IntVec3(x + i * 2, 0, center.z);
                foreach (var t in cell.GetThingList(map).ToList())
                    if (t.def.destroyable && !(t is Pawn)) t.Destroy();
                if (map.roofGrid.Roofed(cell)) map.roofGrid.SetRoof(cell, null);
                var p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, null));
                GenSpawn.Spawn(p, cell, map);
                p.Rotation = Rot4.South;
                pawns.Add(p);
                i++;
            }
        }

        private static void Report()
        {
            var bad = pawns.Where(p => p.Drawer.renderer.CurAnimation != p.RaceProps.startingAnimation).ToList();
            foreach (var p in bad)
                Log.Error($"[Convergence] AnimShot: {p.def.defName} should play {p.RaceProps.startingAnimation.defName}, plays {p.Drawer.renderer.CurAnimation?.defName ?? "nothing"}");
            Log.Message($"[Convergence] AnimShot: {pawns.Count} pawn(s), 3 frame(s) in {dir}\n  " + (bad.Count == 0 && pawns.Count > 0 ? "ALL OK" : "FAILED"));
        }
    }
}
