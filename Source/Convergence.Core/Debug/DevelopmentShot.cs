using System.IO;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Herramienta de revisión de interfaz: con <c>-cvgdevshot</c> (lanzar con <c>run-test.ps1 -Width 1600 -Height 900</c>) abre la ventana
    /// Desarrollo de un cultivador de prueba (raíz de fuego, su camino abierto, la tablilla de la Espada, algunas
    /// técnicas y un conjunto) y guarda una captura en <c>&lt;datos&gt;/cvg_renders/development.png</c>; luego cierra el
    /// juego. Sin el argumento no hace nada.
    /// </summary>
    public static class DevelopmentShot
    {
        private static bool done;

        public static void CheckCommandLine()
        {
            if (done || Current.ProgramState != ProgramState.Playing || Find.TickManager.TicksGame < 120) return;
            done = true;
            if (!GenCommandLine.CommandLineArgPassed("cvgdevshot")) return;
            Find.WindowStack.Add(new Window_Capture(Path.Combine(GenFilePaths.SaveDataFolderPath, "cvg_renders", "development.png")));
        }

        private static void OpenDialog()
        {
            var map = Find.CurrentMap;
            var pawn = map?.mapPawns.FreeColonistsSpawned.FirstOrDefault();
            if (pawn == null) return;
            // Tras la Convergencia, para ver la raíz, el reino y los caminos.
            if (!GameComponent_Convergence.Active) GameComponent_Convergence.Instance?.TryFire(map);
            var comp = CompCultivator.For(pawn);
            if (!comp.HasRoot)
                CultivationGenerator.GiveRoot(pawn, CultivationGenerator.RandomGrade(),
                    new System.Collections.Generic.List<QiElementDef> { DefDatabase<QiElementDef>.GetNamed("CVG_Element_Fire") });
            comp.Sync();
            comp.SyncRealm();
            var t = comp.Techniques;
            // Una raíz de dos elementos: un camino abierto y el otro con "Desbloquear (1 punto)" a la vista.
            CultivationGenerator.AddRootElement(pawn, DefDatabase<QiElementDef>.GetNamed("CVG_Element_Water"));
            var first = DefDatabase<TechniqueManualDef>.AllDefsListForReading.Where(m => t.CanOpenPath(m, out _, ignorePoints: true))
                .OrderBy(m => m.order).FirstOrDefault();
            if (first != null) t.OpenPath(first, free: true);
            t.LearnManual(DefDatabase<TechniqueManualDef>.GetNamed("CVG_Manual_SwordHeart"), sendMessage: false);
            t.AddPoints(4);
            foreach (var def in t.Manuals.SelectMany(m => m.Techniques).Where(d => TechniqueUtility.Ext(d).tier <= 2).Take(5).ToList())
                t.Unlock(def, free: true);
            Find.WindowStack.Add(new Dialog_CultivationDevelopment(pawn));
        }

        /// <summary>Ventana invisible: espera a la nueva resolución, abre la ventana, captura y cierra el juego.</summary>
        private class Window_Capture : Window
        {
            private readonly string path;
            private int frames;

            public Window_Capture(string path)
            {
                this.path = path;
                doWindowBackground = false;
                drawShadow = false;
                absorbInputAroundWindow = false;
                preventCameraMotion = false;
                layer = WindowLayer.Super;
            }

            public override Vector2 InitialSize => new Vector2(1f, 1f);

            public override void DoWindowContents(Rect inRect)
            {
                if (Event.current.type != EventType.Repaint) return;
                frames++;
                if (frames == 30) OpenDialog();
                if (frames == 80)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    ScreenCapture.CaptureScreenshot(path);
                    Log.Message($"[Convergence] Development window captured ({Screen.width}x{Screen.height}): {path}");
                }
                // La captura se escribe al final del cuadro: cerrar el juego de prueba unos cuadros después.
                if (frames == 120) Root.Shutdown();
            }
        }
    }
}
