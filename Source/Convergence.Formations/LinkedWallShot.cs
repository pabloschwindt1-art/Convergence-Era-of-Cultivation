using System.IO;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using LudeonTK;

namespace Convergence.Formations
{
    /// <summary>
    /// Captura de las piezas de conexión (con <c>-cvgwallshot</c> y <c>-quicktest</c>, ventana visible): construye una fila, una L, una T y una
    /// cruz del muro de piedra y de la empalizada de bambú y saca una captura en <c>&lt;datos&gt;/cvg_wallshot/</c>.
    /// Sin el argumento no hace nada.
    /// </summary>
    public class GameComponent_LinkedWallShot : GameComponent
    {
        private static readonly bool Enabled = GenCommandLine.CommandLineArgPassed("cvgwallshot");
        private int state, wait;
        private string dir;

        public GameComponent_LinkedWallShot(Game game) { }

        public override void GameComponentTick()
        {
            if (!Enabled || state >= 3 || Find.CurrentMap == null) return;
            var map = Find.CurrentMap;
            if (state == 0)
            {
                if (Find.TickManager.TicksGame < 200) return;
                Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
                dir = Path.Combine(GenFilePaths.SaveDataFolderPath, "cvg_wallshot");
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
                Directory.CreateDirectory(dir);
                var origin = map.Center + new IntVec3(-14, 0, -4);
                int row = 0;
                foreach (var defName in new[] { "CVG_LowStoneWall", "CVG_BambooPalisade" })
                {
                    var def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
                    if (def == null) continue;
                    var o = origin + new IntVec3(0, 0, row * 9);
                    // fila de 5, L, T, cruz, un bloque suelto y una U
                    var cells = new[] { (0, 0), (1, 0), (2, 0), (3, 0), (4, 0), (6, 0), (6, 1), (6, 2), (7, 2), (9, 0), (10, 0), (11, 0), (10, 1), (10, 2), (13, 1), (14, 1), (15, 1), (14, 0), (14, 2), (17, 0), (19, 0), (19, 1), (19, 2), (18, 2), (17, 2) };
                    foreach (var (x, z) in cells)
                    {
                        var c = o + new IntVec3(x, 0, z);
                        if (!c.InBounds(map)) continue;
                        foreach (var t in c.GetThingList(map).ToList()) if (t.def.destroyable && t.def.category != ThingCategory.Pawn) t.Destroy();
                        var wall = ThingMaker.MakeThing(def, def.MadeFromStuff ? GenStuff.DefaultStuffFor(def) : null);
                        wall.SetFaction(Faction.OfPlayer);
                        GenSpawn.Spawn(wall, c, map);
                    }
                    row++;
                }
                Find.CameraDriver.SetRootPosAndSize((origin + new IntVec3(10, 0, 5)).ToVector3Shifted(), 14f);
                state = 1; wait = 40;
                return;
            }
            if (--wait > 0) return;
            if (state == 1)
            {
                foreach (var w in Find.WindowStack.Windows.OfType<EditWindow_Log>().ToList()) w.Close(false);
                ScreenCapture.CaptureScreenshot(Path.Combine(dir, "walls.png"));
                state = 2; wait = 20;
                return;
            }
            Log.Message("[Convergence] Wall shot saved in " + dir + "\n  ALL OK");
            state = 3;
        }
    }
}
