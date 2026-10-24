using HarmonyLib;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Convergence: DevTools (plan de integración, 2026-10-23): los autotests y las capturas del Core viven en este mod de
    /// desarrollo, que solo carga tools/run-test.ps1. El mod que se instala para jugar no los lleva.
    /// Todo es inerte sin su argumento de línea de comandos (-cvg...).
    /// </summary>
    public class GameComponent_DevTools : GameComponent
    {
        public GameComponent_DevTools(Game game) { }

        public override void GameComponentUpdate()
        {
            RenderTest.CheckCommandLine();
            Showcase.Update();
            AnimationShot.Update();
            TranslationReportShot.Update();
            FloorShot.Update();
            ArtShots.Update();
            WorkshopShot.Update();
            ModShots.Update();
        }

        public override void GameComponentTick()
        {
            BloodlineSelfTest.CheckCommandLine();
            DevelopmentShot.CheckCommandLine();
            TabShot.CheckCommandLine();
            WorkshopShot.CheckCommandLine();
        }
    }

    public static class DevToolsDebugActions
    {
        [LudeonTK.DebugAction("Convergence", "Convergence: Bloodline self-test", allowedGameStates = LudeonTK.AllowedGameStates.PlayingOnMap)]
        private static void BloodlineSelfTestAction()
        {
            BloodlineSelfTest.Run();
            Messages.Message("Logged to the debug log.", RimWorld.MessageTypeDefOf.NeutralEvent, false);
        }
    }

    [StaticConstructorOnStartup]
    public static class DevToolsStartup
    {
        static DevToolsStartup()
        {
            new Harmony("prospero1088.convergence.devtools").PatchAll();
            Log.Message("[Convergence] DevTools loaded (autotests and captures).");
        }
    }
}
