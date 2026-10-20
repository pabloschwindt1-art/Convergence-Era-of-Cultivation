using System.IO;
using System.Linq;
using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Revisión de la pestaña Cultivo: con <c>-cvgtabshot</c> (lanzar con <c>run-test.ps1 -Width 1600 -Height 900</c>) sube un colono a
    /// Núcleo Dorado etapa 2 con las mismas acciones de debug que usa el jugador (avance forzado, progreso, llenar qi), lo
    /// selecciona, abre la pestaña Cultivo y guarda <c>&lt;datos&gt;/cvg_renders/cultivation_tab.png</c>; registra qi actual/máximo y
    /// el hediff del reino y cierra el juego. Sin el argumento no hace nada.
    /// </summary>
    public static class TabShot
    {
        private static bool done;
        private static Pawn pawn;
        private static bool card;

        public static void CheckCommandLine()
        {
            if (done || Current.ProgramState != ProgramState.Playing || Find.TickManager.TicksGame < 120) return;
            done = true;
            card = GenCommandLine.CommandLineArgPassed("cvgcardshot");
            if (!GenCommandLine.CommandLineArgPassed("cvgtabshot") && !card) return;
            Find.WindowStack.Add(new Window_Capture(Path.Combine(GenFilePaths.SaveDataFolderPath, "cvg_renders", card ? "character_card.png" : "cultivation_tab.png")));
        }

        private static void Setup()
        {
            var map = Find.CurrentMap;
            pawn = map?.mapPawns.FreeColonistsSpawned.FirstOrDefault();
            if (pawn == null) return;
            if (!GameComponent_Convergence.Active) GameComponent_Convergence.Instance?.TryFire(map);
            var comp = CompCultivator.For(pawn);
            if (!comp.HasRoot)
                CultivationGenerator.GiveRoot(pawn, CultivationGenerator.RandomGrade(),
                    new System.Collections.Generic.List<QiElementDef> { DefDatabase<QiElementDef>.GetNamed("CVG_Element_Fire") });
            comp.Sync();
            comp.SyncRealm();
            var tracker = comp.Realm;
            tracker.EnsureStarted();
            while (tracker.Realm != null && tracker.Realm.defName != "CVG_Realm_GoldenCore" && tracker.NextRealm != null)
                BreakthroughUtility.Ascend(pawn, tracker.NextRealm, 1f);
            tracker.AddProgress(tracker.Realm.progressPerStage);
            tracker.Resource.Fill();
            Report("after setup");
            Find.Selector.ClearSelection();
            Find.Selector.Select(pawn);
            CameraJumper.TryJump(pawn);
            var inspect = (MainTabWindow_Inspect)MainButtonDefOf.Inspect.TabWindow;
            inspect.OpenTabType = card ? typeof(ITab_Pawn_Character) : typeof(ITab_Cultivation);
            if (card)
            {
                // Historia de infancia tier 2 y adultez tier 4 (con modificadores) para ver colores y la lista de modificadores.
                var child = DefDatabase<BackstoryDef>.AllDefsListForReading.FirstOrDefault(b => b.slot == BackstorySlot.Childhood && BackstoryTiers.Tier(b) == 2 && b.defName.StartsWith("CVG_"));
                var adult = DefDatabase<BackstoryDef>.AllDefsListForReading.FirstOrDefault(b => b.slot == BackstorySlot.Adulthood && BackstoryTiers.Tier(b) == 4 && b.GetModExtension<DefModExtension_BackstoryStats>() != null);
                if (child != null) pawn.story.Childhood = child;
                if (adult != null) pawn.story.Adulthood = adult;
                var counts = DefDatabase<BackstoryDef>.AllDefsListForReading.GroupBy(b => (b.defName.StartsWith("CVG_") ? "ours" : "other") + " T" + BackstoryTiers.Tier(b)).OrderBy(g => g.Key).Select(g => g.Key + "=" + g.Count());
                Log.Message("[Convergence] Card shot: " + child?.defName + " / " + adult?.defName + "; tiers: " + string.Join(", ", counts));
            }
        }

        private static void Report(string when)
        {
            var comp = CompCultivator.For(pawn);
            var t = comp.Realm;
            bool hediff = t.Realm?.hediff != null && pawn.health.hediffSet.HasHediff(t.Realm.hediff);
            Log.Message($"[Convergence] TabShot {when}: realm={t.Realm?.defName} stage={t.Stage} qi={t.Resource.Current:F0}/{t.Resource.Max:F0} " +
                        $"QiMax stat={pawn.GetStatValue(CVG_DefOf.CVG_QiMax):F0} realmHediff={hediff} dev={CultivationDevelopmentUtility.CanOpen(pawn)}");
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
                preventCameraMotion = false;
                layer = WindowLayer.Super;
            }

            public override Vector2 InitialSize => new Vector2(1f, 1f);

            public override void DoWindowContents(Rect inRect)
            {
                if (Event.current.type != EventType.Repaint) return;
                frames++;
                if (frames == 30) Setup();
                if (frames == 80) Find.WindowStack.TryRemove(typeof(EditWindow_Log), doCloseSound: false);
                if (frames == 90 && pawn != null)
                {
                    Report("before capture");
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    ScreenCapture.CaptureScreenshot(path);
                    Log.Message($"[Convergence] Cultivation tab captured ({Screen.width}x{Screen.height}): {path}");
                }
                if (frames == 130) Root.Shutdown();
            }
        }
    }
}
