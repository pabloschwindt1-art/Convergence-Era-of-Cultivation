using System.IO;
using HarmonyLib;
using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Revisión de la lista de escenarios: con <c>-cvgscenarioshot</c> (sin <c>-quicktest</c>) abre "Elegir escenario" desde el menú
    /// principal, selecciona el primer escenario de Convergence, guarda <c>&lt;datos&gt;/cvg_renders/scenarios.png</c> y cierra el juego.
    /// Sin el argumento no hace nada.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class ScenarioShot
    {
        static ScenarioShot()
        {
            if (!GenCommandLine.CommandLineArgPassed("cvgscenarioshot")) return;
            // Se engancha al dibujo del menú principal: ahí la pila de ventanas ya existe.
            new Harmony("prospero1088.convergence.scenarioshot").Patch(AccessTools.Method(typeof(MainMenuDrawer), nameof(MainMenuDrawer.MainMenuOnGUI)),
                postfix: new HarmonyMethod(typeof(ScenarioShot), nameof(MenuPostfix)));
        }

        private static bool started;

        private static void MenuPostfix()
        {
            if (started || Find.WindowStack == null) return;
            started = true;
            Find.WindowStack.Add(new Window_Capture(Path.Combine(GenFilePaths.SaveDataFolderPath, "cvg_renders", "scenarios.png")));
        }

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
                layer = WindowLayer.Super;
            }

            public override Vector2 InitialSize => new Vector2(1f, 1f);

            public override void DoWindowContents(Rect inRect)
            {
                if (Event.current.type != EventType.Repaint) return;
                frames++;
                if (frames == 60)
                {
                    Find.WindowStack.TryRemove(typeof(EditWindow_Log), doCloseSound: false);
                    var page = new Page_SelectScenario();
                    Find.WindowStack.Add(page);
                    var def = DefDatabase<ScenarioDef>.GetNamedSilentFail("CVG_Scenario_LoneSurvivor");
                    if (def != null) Traverse.Create(page).Field("curScen").SetValue(def.scenario);
                    foreach (var s in DefDatabase<ScenarioDef>.AllDefs)
                        if (s.defName.StartsWith("CVG_")) Log.Message($"[Convergence] Scenario {s.defName}: {s.scenario.name} | {s.scenario.summary}");
                }
                if (frames == 70) Find.WindowStack.TryRemove(typeof(EditWindow_Log), doCloseSound: false);
                if (frames == 100)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    ScreenCapture.CaptureScreenshot(path);
                    Log.Message($"[Convergence] Scenario list captured: {path}");
                }
                if (frames == 140) Root.Shutdown();
            }
        }
    }
}
