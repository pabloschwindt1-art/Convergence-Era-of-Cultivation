using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace Convergence.QiTech
{
    /// <summary>
    /// Capturas para la página de Steam Workshop de Qi-Tech, como las del Core (WorkshopShot): con <c>-cvgqitechworkshopshot</c>
    /// (lanzar con <c>run-test.ps1 -Language English -Width 1920 -Height 1080 -Visible</c>) arma un laboratorio y una batalla y guarda
    /// en <c>&lt;datos&gt;/cvg_workshop_qitech/</c>: laboratorio, pestaña Ingeniería Espiritual, ficha del cañón de compresión,
    /// exoesqueleto y torretas en combate; graba cuadro a cuadro el combate para un GIF. Cierra el juego al terminar. Inerte sin el argumento.
    /// </summary>
    public class GameComponent_QiTechWorkshopShot : GameComponent
    {
        private static readonly bool Enabled = GenCommandLine.CommandLineArgPassed("cvgqitechworkshopshot");
        private const int CaptureFps = 30;
        private static bool started;
        private static IEnumerator<int> script;
        private static int wait;
        private static string dir;
        private static Map map;
        private static IntVec3 lab, battle;
        private static Pawn model;
        private static readonly List<Building_TurretGun> turrets = new List<Building_TurretGun>();
        private static readonly List<Pawn> soldiers = new List<Pawn>();

        public GameComponent_QiTechWorkshopShot(Game game) { }

        public override void GameComponentTick()
        {
            if (!Enabled || started || Find.TickManager.TicksGame < 120 || Find.CurrentMap == null) return;
            started = true;
            dir = Path.Combine(GenFilePaths.SaveDataFolderPath, "cvg_workshop_qitech");
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
            Directory.CreateDirectory(dir);
            script = Script();
            wait = 30;
        }

        public override void GameComponentUpdate()
        {
            if (script == null || --wait > 0) return;
            try
            {
                if (script.MoveNext()) wait = Mathf.Max(1, script.Current);
                else script = null;
            }
            catch (System.Exception e)
            {
                script = null;
                UnityCompat.CaptureFramerate = 0;
                Log.Error("[Convergence] Qi-Tech WorkshopShot: " + e + "\n  FAILURES: 1");
                Root.Shutdown();
            }
        }

        private static IEnumerator<int> Script()
        {
            map = Find.CurrentMap;
            var tm = Find.TickManager;
            tm.CurTimeSpeed = TimeSpeed.Paused;
            int hour = GenLocalDate.HourOfDay(map);
            tm.DebugSetTicksGame(tm.TicksGame + (11 - hour + 24) % 24 * GenDate.TicksPerHour);
            map.weatherManager.curWeather = WeatherDefOf.Clear;
            map.weatherManager.lastWeather = WeatherDefOf.Clear;
            Prefs.AdaptiveTrainingEnabled = false;
            Find.CameraDriver.config.sizeRange = new FloatRange(4f, 60f);
            if (!GameComponent_Convergence.Active) GameComponent_Convergence.Instance.MarkOccurred();
            FinishResearch("CVG_Research_SpiritResonance");
            FinishResearch("CVG_Research_QiDefense");
            FinishResearch("CVG_Research_QiWeapons");
            Find.ResearchManager.SetCurrentProject(DefDatabase<ResearchProjectDef>.GetNamed("CVG_Research_AdvancedSpiritInstrumentation"));
            SetUp();
            CleanScreen();
            yield return 30;

            // 1) Laboratorio.
            Find.CameraDriver.SetRootPosAndSize(lab.ToVector3Shifted() + new Vector3(0.5f, 0f, 1f), 6f);
            HideUI(true);
            yield return 40;
            Capture("01_lab");
            yield return 10;
            HideUI(false);

            // 2) Pestaña Ingeniería Espiritual.
            Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Research, playSound: false);
            yield return 5;
            if (MainButtonDefOf.Research.TabWindow is MainTabWindow_Research w)
            {
                w.CurTab = DefDatabase<ResearchTabDef>.GetNamed("CVG_SpiritualEngineering");
                w.Select(DefDatabase<ResearchProjectDef>.GetNamed("CVG_Research_AdvancedSpiritInstrumentation"));
            }
            yield return 40;
            Capture("02_spiritual_engineering");
            yield return 10;
            CleanScreen();

            // 3) Ficha del cañón de compresión (grado espiritual 3).
            Find.WindowStack.Add(new Dialog_InfoCard(DefDatabase<ThingDef>.GetNamed("CVG_QT_CompressionCannon")));
            yield return 40;
            Capture("03_compression_cannon");
            yield return 10;
            CleanScreen();

            // 4) Exoesqueleto de cerca.
            Find.CameraDriver.SetRootPosAndSize(model.DrawPos, 4.5f);
            HideUI(true);
            yield return 40;
            Capture("04_exoskeleton");
            yield return 10;

            // 5) Combate: oleada desde el este contra las torretas y los colonos armados.
            SendWave(7);
            Find.CameraDriver.SetRootPosAndSize(battle.ToVector3Shifted() + new Vector3(5f, 0f, 0f), 9f);
            tm.CurTimeSpeed = TimeSpeed.Normal;
            yield return 360;
            Capture("05_turrets");
            yield return 2;
            UnityCompat.CaptureFramerate = CaptureFps;
            for (int f = 0; f < CaptureFps * 6; f++)
            {
                Capture($"anim_turrets/{f:000}");
                yield return 1;
            }
            UnityCompat.CaptureFramerate = 0;
            tm.CurTimeSpeed = TimeSpeed.Paused;
            HideUI(false);
            yield return 20;
            Log.Message($"[Convergence] Qi-Tech WorkshopShot: screenshots in {dir} ({Screen.width}x{Screen.height})\n  ALL OK");
            yield return 20;
            Root.Shutdown();
        }

        private static void FinishResearch(string defName)
        {
            void Finish(ResearchProjectDef p)
            {
                if (p == null || p.IsFinished) return;
                foreach (var q in (p.prerequisites ?? new List<ResearchProjectDef>()).Concat(p.hiddenPrerequisites ?? new List<ResearchProjectDef>())) Finish(q);
                Find.ResearchManager.FinishProject(p, doCompletionDialog: false, null, doCompletionLetter: false);
            }
            Finish(DefDatabase<ResearchProjectDef>.GetNamed(defName));
        }

        private static void SetUp()
        {
            var c = map.Center;
            lab = c + new IntVec3(-12, 0, 0);
            battle = c + new IntVec3(6, 0, 0);
            foreach (var cell in CellRect.CenteredOn(c, 26).Cells.Where(x => x.InBounds(map)))
            {
                foreach (var t in cell.GetThingList(map).ToList())
                    if (t.def.destroyable && !(t is Pawn) && (t.def.category != ThingCategory.Plant || ((Plant)t).def.plant.IsTree || cell.InHorDistOf(lab, 7f)))
                        t.Destroy();
                if (cell.GetTerrain(map).passability != Traversability.Standable || cell.GetTerrain(map).IsWater) map.terrainGrid.SetTerrain(cell, TerrainDefOf.Soil);
                map.roofGrid.SetRoof(cell, null);
            }
            foreach (var cell in CellRect.CenteredOn(lab, 5).Cells) map.terrainGrid.SetTerrain(cell, DefDatabase<TerrainDef>.GetNamed("PavedTile"));

            // Laboratorio: banco avanzado + resonador, convertidor, cámara de estabilización y cristales primordiales.
            Place("HiTechResearchBench", lab + new IntVec3(-2, 0, 3), Rot4.South, "Steel");   // 5x2: x -4..0
            Place("CVG_SpiritResonator", lab + new IntVec3(3, 0, 3), Rot4.South);
            Place("CVG_QiConverter", lab + new IntVec3(-3, 0, -2), Rot4.South);
            Place("CVG_StabilizationChamber", lab + new IntVec3(3, 0, -2), Rot4.South);
            var crystals = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("CVG_PrimordialCrystal"));
            crystals.stackCount = 12;
            GenPlace.TryPlaceThing(crystals, lab + new IntVec3(0, 0, -2), map, ThingPlaceMode.Direct);
            foreach (var b in map.listerBuildings.allBuildingsColonist.Where(b => b.Position.InHorDistOf(lab, 7f)))
                if (b.TryGetComp<CompPowerTrader>() is CompPowerTrader pt) pt.PowerOn = true;

            // Batalla: torretas y tres colonos con las armas y el exoesqueleto.
            turrets.Add(PlaceTurret("CVG_QT_InductionTurret", battle + new IntVec3(0, 0, 4)));
            var guns = new[] { "CVG_QT_InductionRifle", "CVG_QT_FocusedRifle", "CVG_QT_CompressionCannon" };
            var colonists = map.mapPawns.FreeColonistsSpawned.Where(p => p.ageTracker.Adult).Take(3).ToList();
            for (int i = 0; i < guns.Length; i++)
            {
                var p = i < colonists.Count ? colonists[i]
                    : PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayer, forceGenerateNewPawn: true));
                if (!p.Spawned) GenSpawn.Spawn(p, battle, map);
                p.jobs.StopAll();
                p.Position = battle + new IntVec3(-3, 0, 2 - i * 2);
                p.Notify_Teleported();
                p.equipment.DestroyAllEquipment();
                p.equipment.AddEquipment((ThingWithComps)ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed(guns[i])));
                p.apparel.Wear((Apparel)ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("CVG_Apparel_QiTechExosuit")), dropReplacedApparel: false);
                p.drafter.Drafted = true;
                soldiers.Add(p);
            }
            // El modelo del exoesqueleto, quieto en el laboratorio.
            model = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayer, forceGenerateNewPawn: true));
            model.apparel.Wear((Apparel)ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("CVG_Apparel_QiTechExosuit")), dropReplacedApparel: false);
            GenSpawn.Spawn(model, lab + new IntVec3(0, 0, 0), map);
            model.drafter.Drafted = true;
            model.Rotation = Rot4.South;
        }

        private static Thing Place(string defName, IntVec3 cell, Rot4 rot, string stuff = null)
        {
            var def = DefDatabase<ThingDef>.GetNamed(defName);
            var t = ThingMaker.MakeThing(def, def.MadeFromStuff ? (stuff != null ? DefDatabase<ThingDef>.GetNamed(stuff) : GenStuff.DefaultStuffFor(def)) : null);
            t.SetFaction(Faction.OfPlayer);
            return GenSpawn.Spawn(t, cell, map, rot);
        }

        private static Building_TurretGun PlaceTurret(string defName, IntVec3 cell)
        {
            var t = (Building_TurretGun)Place(defName, cell, Rot4.North);
            t.GetComp<CompRefuelable>().Refuel(12f);
            t.GetComp<CompPowerTrader>().PowerOn = true;
            return t;
        }

        private static void SendWave(int count)
        {
            var faction = Find.FactionManager.RandomEnemyFaction(allowNonHumanlike: false) ?? Faction.OfAncientsHostile;
            var wave = new List<Pawn>();
            for (int i = 0; i < count; i++)
            {
                var p = PawnGenerator.GeneratePawn(PawnKindDefOf.Villager, faction);
                GenSpawn.Spawn(p, CellFinder.StandableCellNear(battle + new IntVec3(20, 0, -9 + i * 3), map, 3f), map);
                wave.Add(p);
            }
            LordMaker.MakeNewLord(faction, new LordJob_AssaultColony(faction, canKidnap: false, canTimeoutOrFlee: false), map, wave);
        }

        private static void CleanScreen()
        {
            foreach (var w in Find.WindowStack.Windows.ToList())
                if (w is EditWindow_Log || w is MainTabWindow || w is Dialog_InfoCard || w.layer == WindowLayer.Dialog) w.Close(false);
            Messages.Clear();
            Find.LetterStack.LettersListForReading.ToList().ForEach(l => Find.LetterStack.RemoveLetter(l));
        }

        private static void HideUI(bool hide) => Find.UIRoot.screenshotMode.Active = hide;

        private static void Capture(string name)
        {
            var path = Path.Combine(dir, name + ".png");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            ScreenCapture.CaptureScreenshot(path);
        }
    }

    /// <summary>Con <c>-cvgqitechworkshopshot</c>: el quicktest arranca en bosque templado llano (como las capturas del Core).</summary>
    [StaticConstructorOnStartup]
    public static class QiTechWorkshopShotMap
    {
        static QiTechWorkshopShotMap()
        {
            if (!GenCommandLine.CommandLineArgPassed("cvgqitechworkshopshot")) return;
            new Harmony("prospero1088.convergence.qitech.workshopshot").Patch(AccessTools.Method(typeof(GameInitData), nameof(GameInitData.ChooseRandomStartingTile)),
                prefix: new HarmonyMethod(typeof(QiTechWorkshopShotMap), nameof(Prefix)));
        }

        private static bool Prefix(GameInitData __instance)
        {
            __instance.startingTile = TileFinder.RandomSettlementTileFor(Faction.OfPlayer, mustBeAutoChoosable: true, extraValidator: t =>
            {
                var tile = Find.WorldGrid[t];
                return tile.PrimaryBiome == BiomeDefOf.TemperateForest && (tile.hilliness == Hilliness.Flat || tile.hilliness == Hilliness.SmallHills);
            });
            return false;
        }
    }
}
