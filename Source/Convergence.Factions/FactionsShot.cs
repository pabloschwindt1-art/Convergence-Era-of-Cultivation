using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using LudeonTK;
using RimWorld;
using UnityEngine;
using RimWorld.Planet;
using Verse;

namespace Convergence.Factions
{
    /// <summary>
    /// Capturas para la página de Steam Workshop de Factions: con <c>-cvgfactionsshot</c> (lanzar con
    /// <c>run-test.ps1 -Language English -Width 1920 -Height 1080 -Visible</c>) trae las sectas, forma a sus discípulos y marionetas en
    /// fila, y guarda en <c>&lt;datos&gt;/cvg_factions_shots/</c> la fila, la pestaña Facciones, dos ideologías, los títulos y favores de
    /// un anciano invitado y la ficha de un discípulo. Cierra el juego al terminar. Sin el argumento no hace nada.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class FactionsShotMap
    {
        static FactionsShotMap()
        {
            if (!GenCommandLine.CommandLineArgPassed("cvgfactionsshot")) return;
            // El quicktest elige el bioma al azar: para las capturas, bosque templado llano.
            new Harmony("prospero1088.convergence.factionsshot").Patch(AccessTools.Method(typeof(GameInitData), nameof(GameInitData.ChooseRandomStartingTile)),
                prefix: new HarmonyMethod(typeof(FactionsShotMap), nameof(Prefix)));
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

    public class GameComponent_FactionsShot : GameComponent
    {
        private static readonly bool Enabled = GenCommandLine.CommandLineArgPassed("cvgfactionsshot");
        private string dir;
        private Map map;
        private IEnumerator<int> script;
        private int wait;
        private bool started;

        public GameComponent_FactionsShot(Game game) { }

        public override void GameComponentUpdate()
        {
            if (!Enabled) return;
            if (!started)
            {
                if (Current.ProgramState != ProgramState.Playing || Find.CurrentMap == null || Find.TickManager.TicksGame < 120) return;
                started = true;
                dir = Path.Combine(GenFilePaths.SaveDataFolderPath, "cvg_factions_shots");
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
                Directory.CreateDirectory(dir);
                script = Script();
                wait = 30;
            }
            if (script == null || --wait > 0) return;
            try
            {
                if (script.MoveNext()) wait = Mathf.Max(1, script.Current);
                else script = null;
            }
            catch (Exception e)
            {
                script = null;
                Log.Error("[Convergence] FactionsShot: " + e + "\n  FAILURES: 1");
                Root.Shutdown();
            }
        }

        private static readonly string[] Sects = { "FormlessSword", "NineTemperings", "HeavenlyCauldron", "BloodMoon", "InvisibleThread" };

        /// <summary>Por secta: el anciano y un discípulo; el Hilo Invisible suma tres marionetas.</summary>
        private static readonly Dictionary<string, string[]> Lineup = new Dictionary<string, string[]>
        {
            { "FormlessSword", new[] { "CVG_FormlessSword_Elder", "CVG_FormlessSword_Blade", "CVG_FormlessSword_Disciple" } },
            { "NineTemperings", new[] { "CVG_NineTemperings_Elder", "CVG_NineTemperings_Core", "CVG_NineTemperings_Disciple" } },
            { "HeavenlyCauldron", new[] { "CVG_HeavenlyCauldron_Grand", "CVG_HeavenlyCauldron_Master", "CVG_HeavenlyCauldron_Apprentice" } },
            { "BloodMoon", new[] { "CVG_BloodMoon_Elder", "CVG_BloodMoon_Crimson", "CVG_BloodMoon_Servant" } },
            { "InvisibleThread", new[] { "CVG_InvisibleThread_Elder", "CVG_Puppet_Jade", "CVG_Puppet_Bronze", "CVG_Puppet_Blade" } },
        };

        private IEnumerator<int> Script()
        {
            map = Find.CurrentMap;
            var tm = Find.TickManager;
            tm.CurTimeSpeed = TimeSpeed.Paused;
            int hour = GenLocalDate.HourOfDay(map);
            tm.DebugSetTicksGame(tm.TicksGame + (11 - hour + 24) % 24 * GenDate.TicksPerHour);
            map.weatherManager.curWeather = WeatherDefOf.Clear;
            map.weatherManager.lastWeather = WeatherDefOf.Clear;
            Prefs.AdaptiveTrainingEnabled = false;
            Find.CameraDriver.config.sizeRange = new FloatRange(5f, 60f);

            // Las sectas llegan con la Convergencia.
            if (!GameComponent_Convergence.Active) GameComponent_Convergence.Instance?.MarkOccurred();
            ConvergenceFactionUtility.EnsureFactions();
            yield return 20;
            Prefs.DevMode = false;
            CleanScreen();

            // 1) Las cinco sectas en fila, por columnas.
            var spawned = SpawnLineup(out var center);
            if (spawned.Count == 0)
            {
                Log.Error("[Convergence] FactionsShot: could not spawn the lineup\n  FAILURES: 1");
                Root.Shutdown();
                yield break;
            }
            HideUI(true);
            Find.CameraDriver.SetRootPosAndSize(center.ToVector3Shifted() + new Vector3(0f, 0f, 0.5f), 6.5f);
            yield return 40;
            Capture("01_the_sects");
            yield return 10;
            // Primer plano del Hilo Invisible y sus marionetas.
            var thread = spawned.Where(p => p.Faction?.def.defName == "CVG_Faction_InvisibleThread").ToList();
            if (thread.Count > 0)
            {
                var c = new Vector3(thread.Average(p => p.DrawPos.x), 0f, thread.Average(p => p.DrawPos.z));
                Find.CameraDriver.SetRootPosAndSize(c, 5f);
                yield return 40;
                Capture("02_invisible_thread");
                yield return 10;
            }
            HideUI(false);

            // 2) Pestaña Facciones.
            var factionsTab = DefDatabase<MainButtonDef>.GetNamedSilentFail("Factions");
            if (factionsTab != null)
            {
                Find.MainTabsRoot.SetCurrentTab(factionsTab, playSound: false);
                yield return 40;
                Capture("03_factions_tab");
                yield return 10;
                Find.MainTabsRoot.EscapeCurrentTab(playSound: false);
            }

            // 3) Ideologías de dos sectas.
            var ideosTab = DefDatabase<MainButtonDef>.GetNamedSilentFail("Ideos");
            int n = 4;
            foreach (var sect in new[] { "FormlessSword", "BloodMoon" })
            {
                var ideo = FactionOf(sect)?.ideos?.PrimaryIdeo;
                if (ideosTab == null || ideo == null) continue;
                Find.MainTabsRoot.SetCurrentTab(ideosTab, playSound: false);
                yield return 5;
                var select = AccessTools.Method(typeof(IdeoUIUtility), "SetSelected");
                select?.Invoke(null, new object[] { ideo });
                yield return 40;
                Capture($"{n++:00}_ideoligion_{sect}");
                yield return 10;
                Find.MainTabsRoot.EscapeCurrentTab(playSound: false);
            }

            // 4) Un colono anciano invitado del Caldero, con sus cinco favores.
            var colonist = map.mapPawns.FreeColonistsSpawned.FirstOrDefault(p => p.royalty != null);
            var cauldron = FactionOf("HeavenlyCauldron");
            if (colonist != null && cauldron != null)
            {
                if (cauldron.HostileTo(Faction.OfPlayer) || cauldron.PlayerGoodwill < 50)
                    cauldron.TryAffectGoodwillWith(Faction.OfPlayer, 60 - cauldron.PlayerGoodwill, canSendMessage: false, canSendHostilityLetter: false);
                colonist.royalty.GainFavor(cauldron, 34);
                foreach (var permit in DefDatabase<RoyalTitlePermitDef>.AllDefsListForReading.Where(p => p.faction == cauldron.def))
                    if (!colonist.royalty.HasPermit(permit, cauldron)) colonist.royalty.AddPermit(permit, cauldron);
                colonist.royalty.GainFavor(cauldron, 9);
                CleanScreen();
                yield return 5;
                OpenCard(colonist, "Permits");
                // La ficha abre en el Imperio: se elige la secta después de abrirla (como el botón de cambiar facción).
                yield return 3;
                PermitsCardUtility.selectedFaction = cauldron;
                yield return 40;
                Capture($"{n++:00}_titles_and_favors");
                yield return 10;
                CleanScreen();
            }

            // 5) La ficha de un anciano de la Espada: historia de la secta y constitución.
            var elder = spawned.FirstOrDefault(p => p.kindDef.defName == "CVG_FormlessSword_Elder");
            if (elder != null)
            {
                OpenCard(elder, "Character");
                yield return 40;
                Capture($"{n++:00}_sword_elder");
                yield return 10;
                CleanScreen();
            }

            yield return 20;
            Log.Message($"[Convergence] FactionsShot: screenshots in {dir} ({Screen.width}x{Screen.height})\n  ALL OK");
            yield return 20;
            Root.Shutdown();
        }

        private static Faction FactionOf(string sect) =>
            Find.FactionManager.FirstFactionOfDef(DefDatabase<FactionDef>.GetNamedSilentFail("CVG_Faction_" + sect));

        private List<Pawn> SpawnLineup(out IntVec3 center)
        {
            var result = new List<Pawn>();
            var anchor = map.mapPawns.FreeColonistsSpawned.FirstOrDefault()?.Position ?? map.Center;
            bool Clear(IntVec3 c)
            {
                var r = CellRect.CenteredOn(c, 9, 3);
                return r.InBounds(map) && r.Cells.All(x => x.GetEdifice(map) == null && !x.Fogged(map) && !x.Roofed(map) && !x.GetTerrain(map).IsWater
                                                         && x.GetTerrain(map).passability == Traversability.Standable);
            }
            if (!CellFinder.TryFindRandomCellNear(anchor, map, 30, Clear, out center) && !CellFinder.TryFindRandomCellNear(map.Center, map, 60, Clear, out center))
                return result;
            var area = CellRect.CenteredOn(center, 12, 5);
            // Solo el pasto queda: árboles, arbustos y bambú tapan a los discípulos.
            foreach (var t in area.Cells.SelectMany(c => c.GetThingList(map)).Where(t => t is Plant p && !p.def.defName.Contains("Grass")
                                                                                         || t.def.category == ThingCategory.Item || t is Corpse).ToList())
                t.Destroy();
            int col = -8;
            foreach (var sect in Sects)
            {
                var faction = FactionOf(sect);
                if (faction == null || !Lineup.TryGetValue(sect, out var kinds)) { col += 4; continue; }
                for (int i = 0; i < kinds.Length; i++)
                {
                    var kind = DefDatabase<PawnKindDef>.GetNamedSilentFail(kinds[i]);
                    if (kind == null) continue;
                    var pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, faction, forceGenerateNewPawn: true, mustBeCapableOfViolence: true));
                    // El anciano adelante; los demás detrás, alternando lados.
                    // Las marionetas del Hilo Invisible, en fila detrás de su maestro (son altas y se tapan).
                    var offset = i == 0 ? new IntVec3(0, 0, -1)
                        : sect == "InvisibleThread" ? new IntVec3(i * 2 - 3, 0, 1)
                        : new IntVec3(i % 2 == 1 ? -1 : 1, 0, 1);
                    var cell = center + new IntVec3(col, 0, 0) + offset;
                    if (!cell.InBounds(map) || !cell.Standable(map)) cell = CellFinder.RandomClosewalkCellNear(cell, map, 2);
                    GenSpawn.Spawn(pawn, cell, map, Rot4.South);
                    pawn.Rotation = Rot4.South;
                    pawn.jobs?.StopAll();
                    result.Add(pawn);
                }
                col += 4;
            }
            return result;
        }

        /// <summary>La ficha del personaje (Dialog_InfoCard) en la pestaña pedida (Character, Permits…).</summary>
        private static void OpenCard(Pawn pawn, string tab)
        {
            var card = new Dialog_InfoCard(pawn);
            var field = AccessTools.Field(typeof(Dialog_InfoCard), "tab");
            if (field != null)
            {
                try { field.SetValue(card, Enum.Parse(field.FieldType, tab)); }
                catch (Exception e) { Log.Warning("[Convergence] FactionsShot: tab " + tab + ": " + e.Message); }
            }
            Find.WindowStack.Add(card);
        }

        private static void CleanScreen()
        {
            foreach (var w in Find.WindowStack.Windows.ToList())
                if (w is EditWindow_Log || w is Dialog_NodeTree || w is Dialog_InfoCard || w is MainTabWindow || w.layer == WindowLayer.Dialog)
                    w.Close(false);
            Messages.Clear();
            Find.LetterStack.LettersListForReading.ToList().ForEach(l => Find.LetterStack.RemoveLetter(l));
        }

        private static void HideUI(bool hide) => Find.UIRoot.screenshotMode.Active = hide;

        private void Capture(string name)
        {
            var path = Path.Combine(dir, name + ".png");
            ScreenCapture.CaptureScreenshot(path);
        }
    }
}
