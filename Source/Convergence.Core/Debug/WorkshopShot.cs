using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using LudeonTK;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Convergence
{
    /// <summary>
    /// Capturas para la página de Steam Workshop del Core: con <c>-cvgworkshopshot</c> (lanzar con
    /// <c>run-test.ps1 -Language English -Width 1920 -Height 1080</c>) dispara la Convergencia, arma una escena de tres cultivadores,
    /// y guarda en <c>&lt;datos&gt;/cvg_workshop/</c> la carta de la Convergencia, la escena, el mapa de qi, la pestaña Cultivo y la
    /// ventana Desarrollo; después graba cuadro a cuadro (<c>Time.captureFramerate</c>: el tiempo del juego no depende de lo que tarda
    /// cada captura) una técnica de fuego y una tribulación, para armar GIF. Cierra el juego al terminar. Sin el argumento no hace nada.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class WorkshopShotMap
    {
        static WorkshopShotMap()
        {
            if (!GenCommandLine.CommandLineArgPassed("cvgworkshopshot")) return;
            // El quicktest elige el bioma al azar: para las capturas, bosque templado llano.
            new Harmony("prospero1088.convergence.workshopshot").Patch(AccessTools.Method(typeof(GameInitData), nameof(GameInitData.ChooseRandomStartingTile)),
                prefix: new HarmonyMethod(typeof(WorkshopShotMap), nameof(Prefix)));
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

    public static class WorkshopShot
    {
        private const int CaptureFps = 30;   // a velocidad normal = 2 ticks por cuadro
        private static bool done;
        private static string dir;
        private static Map map;
        private static List<Pawn> pawns;
        private static IntVec3 center;
        private static IEnumerator<int> script;
        private static int wait;

        public static void CheckCommandLine()
        {
            if (done || Current.ProgramState != ProgramState.Playing || Find.TickManager.TicksGame < 120) return;
            done = true;
            if (!GenCommandLine.CommandLineArgPassed("cvgworkshopshot")) return;
            dir = Path.Combine(GenFilePaths.SaveDataFolderPath, "cvg_workshop");
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
            Directory.CreateDirectory(dir);
            script = Script();
            wait = 30;
        }

        /// <summary>Avanza el guion un paso por cuadro (desde GameComponentUpdate: sigue corriendo con la interfaz oculta).</summary>
        public static void Update()
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
                Log.Error("[Convergence] WorkshopShot: " + e + "\n  FAILURES: 1");
                Root.Shutdown();
            }
        }

        /// <summary>Guion: cada <c>yield return n</c> espera n cuadros dibujados.</summary>
        private static IEnumerator<int> Script()
        {
            map = Find.CurrentMap;
            var tm = Find.TickManager;
            tm.CurTimeSpeed = TimeSpeed.Paused;
            // Mediodía y cielo despejado: colores reales.
            int hour = GenLocalDate.HourOfDay(map);
            tm.DebugSetTicksGame(tm.TicksGame + (11 - hour + 24) % 24 * GenDate.TicksPerHour);
            map.weatherManager.curWeather = WeatherDefOf.Clear;
            map.weatherManager.lastWeather = WeatherDefOf.Clear;
            Prefs.AdaptiveTrainingEnabled = false;
            // Más cerca que el zoom mínimo del jugador, para que se vean los colonos y los efectos.
            Find.CameraDriver.config.sizeRange = new FloatRange(5f, 60f);

            // 1) La Convergencia y su carta.
            if (!GameComponent_Convergence.Active) GameComponent_Convergence.Instance?.TryFire(map);
            yield return 20;
            Prefs.DevMode = false;
            var letter = Find.LetterStack.LettersListForReading.LastOrDefault();
            CleanScreen(closeLetters: false);
            letter?.OpenLetter();
            yield return 40;
            Capture("05_convergence_letter");
            yield return 10;
            CleanScreen(closeLetters: true);

            // 2) Escena: tres cultivadores sentados.
            if (!SetupScene())
            {
                Log.Error("[Convergence] WorkshopShot: could not set up the scene\n  FAILURES: 1");
                Root.Shutdown();
                yield break;
            }
            tm.CurTimeSpeed = TimeSpeed.Normal;
            yield return 180;
            CleanScreen(closeLetters: true);
            HideUI(true);
            Find.CameraDriver.SetRootPosAndSize(center.ToVector3Shifted(), 6f);
            // El icono de meditación que el trabajo de cultivo tira cada 400 ticks, para los tres a la vez.
            foreach (var p in pawns) FleckMaker.ThrowMetaIcon(p.Position, map, FleckDefOf.Meditating);
            yield return 25;
            tm.CurTimeSpeed = TimeSpeed.Paused;
            yield return 10;
            Capture("01_cultivation");
            yield return 10;

            // 3) Mapa de qi.
            MapComponent_QiOverlay.Show = true;
            Find.CameraDriver.SetRootPosAndSize(center.ToVector3Shifted(), 28f);
            yield return 40;
            Capture("02_qi_map");
            yield return 10;
            MapComponent_QiOverlay.Show = false;
            HideUI(false);

            // 4) Pestaña Cultivo del Núcleo Dorado.
            var hero = pawns[0];
            Find.Selector.ClearSelection();
            Find.Selector.Select(hero);
            Find.CameraDriver.SetRootPosAndSize(hero.DrawPos, 8f);
            ((MainTabWindow_Inspect)MainButtonDefOf.Inspect.TabWindow).OpenTabType = typeof(ITab_Cultivation);
            yield return 40;
            CleanScreen(closeLetters: true, keepInspect: true);
            yield return 5;
            Capture("03_cultivation_tab");
            yield return 10;

            // 5) Ventana Desarrollo.
            CleanScreen(closeLetters: true);
            Find.WindowStack.Add(new Dialog_CultivationDevelopment(hero));
            yield return 40;
            Capture("04_development");
            yield return 10;
            CleanScreen(closeLetters: true);

            // 5b) El Maestro: llega herido (cae del cielo) y su carta.
            Find.Selector.ClearSelection();
            var mentorDef = DefDatabase<MentorDef>.AllDefsListForReading.FirstOrDefault();
            var mentor = mentorDef == null || GameComponent_Mentor.Instance?.Mentor != null ? null : GameComponent_Mentor.Instance?.Arrive(map, mentorDef, wounded: true);
            if (mentor != null)
            {
                var mentorLetter = Find.LetterStack.LettersListForReading.LastOrDefault();
                // El diálogo ocupa el centro: la cámara se corre para que el Maestro quede a la izquierda.
                Find.CameraDriver.SetRootPosAndSize(mentor.DrawPos + new Vector3(7f, 0f, 0f), 6f);
                // Un segundo de juego: se va el destello rojo del daño (queda congelado si se pausa en el mismo tick).
                Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;
                yield return 60;
                Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
                CleanScreen(closeLetters: false);
                mentorLetter?.OpenLetter();
                yield return 40;
                Capture("06_master");
                yield return 10;
                CleanScreen(closeLetters: true);
            }
            else Log.Warning("[Convergence] WorkshopShot: no mentor");

            // 5c) El árbol de investigación del Dao (pestaña propia).
            Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Research, playSound: false);
            yield return 5;
            ((MainTabWindow_Research)MainButtonDefOf.Research.TabWindow).Select(DefDatabase<ResearchProjectDef>.GetNamed("CVG_Research_Foundation"));
            yield return 40;
            Capture("07_dao_research");
            yield return 10;
            Find.MainTabsRoot.EscapeCurrentTab(playSound: false);
            CleanScreen(closeLetters: true);

            // 5d) Ajustes del mod.
            Find.WindowStack.Add(new Dialog_ModSettings(LoadedModManager.GetMod<ConvergenceMod>()));
            yield return 40;
            Capture("08_settings");
            yield return 10;
            CleanScreen(closeLetters: true);

            // 6) Técnica: bola de fuego y mar de llamas contra un enemigo aturdido.
            var faction = Find.FactionManager.RandomEnemyFaction() ?? Faction.OfAncientsHostile;
            var enemy = PawnGenerator.GeneratePawn(PawnKindDefOf.Villager, faction);
            enemy.equipment?.DestroyAllEquipment();
            var enemyCell = CellFinder.RandomClosewalkCellNear(hero.Position + IntVec3.East * 5, map, 1);
            GenSpawn.Spawn(enemy, enemyCell, map);
            enemy.stances?.stunner?.StunFor(2000, null, addBattleLog: false, showMote: false);
            hero.jobs.StopAll();
            // Los tres reclutados: el enemigo no hace huir a los otros dos.
            foreach (var p in pawns) if (p.drafter != null) p.drafter.Drafted = true;
            hero.rotationTracker.FaceCell(enemyCell);
            Find.Selector.ClearSelection();
            Find.Selector.Select(hero);
            Find.CameraDriver.SetRootPosAndSize(((hero.Position + enemyCell).ToVector3() / 2f) + new Vector3(0.5f, 0f, 0.5f), 5f);
            yield return 30;
            CleanScreen(closeLetters: true);
            HideUI(true);
            var fireball = hero.abilities.GetAbility(DefDatabase<AbilityDef>.GetNamed("CVG_Tech_Fireball"));
            var sea = hero.abilities.GetAbility(DefDatabase<AbilityDef>.GetNamed("CVG_Tech_SeaOfFlames"));
            UnityCompat.CaptureFramerate = CaptureFps;
            tm.CurTimeSpeed = TimeSpeed.Normal;
            for (int f = 0; f < 100; f++)
            {
                if (f == 6) Cast(hero, fireball, enemy);
                if (f == 45) Cast(hero, sea, enemy);
                Capture($"anim_technique/{f:000}");
                yield return 1;
            }
            UnityCompat.CaptureFramerate = 0;
            tm.CurTimeSpeed = TimeSpeed.Paused;
            if (!enemy.Destroyed) enemy.Destroy();
            foreach (var fire in map.listerThings.ThingsOfDef(ThingDefOf.Fire).ToList()) fire.Destroy();
            yield return 10;

            // 7) Tribulación: rayos sobre el Núcleo Dorado (no muere: se cura entre rayos).
            foreach (var p in pawns) if (p.drafter != null) p.drafter.Drafted = false;
            Find.Selector.ClearSelection();
            CleanScreen(closeLetters: true);
            // De vuelta en la plataforma, cultivando.
            hero.jobs.StopAll();
            hero.pather.StopDead();
            hero.Position = center;
            hero.Notify_Teleported();
            var platform = center.GetFirstBuilding(map);
            if (platform != null) hero.jobs.StartJob(JobMaker.MakeJob(CVG_DefOf.CVG_Cultivate, platform), JobCondition.InterruptForced);
            Find.CameraDriver.SetRootPosAndSize(center.ToVector3Shifted(), 6f);
            yield return 10;
            MapComponent_Tribulation.For(map).DebugBegin(hero);
            CleanScreen(closeLetters: true);
            UnityCompat.CaptureFramerate = CaptureFps;
            tm.CurTimeSpeed = TimeSpeed.Normal;
            for (int f = 0; f < 360; f++)
            {
                if (f % 30 == 0) HealAll(hero);
                if (f % 20 == 0) Find.LetterStack.LettersListForReading.ToList().ForEach(l => Find.LetterStack.RemoveLetter(l));
                Capture($"anim_tribulation/{f:000}");
                yield return 1;
            }
            UnityCompat.CaptureFramerate = 0;
            tm.CurTimeSpeed = TimeSpeed.Paused;
            HideUI(false);
            yield return 20;
            Log.Message($"[Convergence] WorkshopShot: screenshots in {dir} ({Screen.width}x{Screen.height})\n  ALL OK");
            yield return 20;
            Root.Shutdown();
        }

        private static bool SetupScene()
        {
            pawns = map.mapPawns.FreeColonistsSpawned.Where(p => p.ageTracker.Adult).Take(3).ToList();
            if (pawns.Count < 3) return false;
            // Sobre una veta de qi si hay una (el suelo jade la delata y concentra el qi); si no, cerca de los colonos.
            var vein = QiNetworkUtility.VeinTerrain;
            var veinCell = vein == null ? IntVec3.Invalid
                : map.AllCells.Where(c => c.GetTerrain(map) == vein).OrderBy(c => c.DistanceToSquared(pawns[0].Position)).FirstOrDefault();
            var start = veinCell.IsValid && veinCell != default ? veinCell : pawns[0].Position;
            if (start == veinCell)
            {
                // El centro de la mancha de veta (las celdas de veta a 4 o menos).
                var patch = GenRadial.RadialCellsAround(veinCell, 4f, true).Where(c => c.InBounds(map) && c.GetTerrain(map) == vein).ToList();
                start = new IntVec3(Mathf.RoundToInt((float)patch.Average(c => c.x)), 0, Mathf.RoundToInt((float)patch.Average(c => c.z)));
            }
            bool Clear(IntVec3 c)
            {
                var r = CellRect.CenteredOn(c, 3);
                return r.InBounds(map) && r.Cells.All(x => x.GetEdifice(map) == null && !x.Fogged(map) && !x.Roofed(map) && !x.GetTerrain(map).IsWater
                                                         && x.GetTerrain(map).passability == Traversability.Standable
                                                         && x.GetThingList(map).All(t => t is Plant || t is Filth || t.def.category == ThingCategory.Item));
            }
            center = start;
            if (!Clear(center) && !CellFinder.TryFindRandomCellNear(start, map, 3, Clear, out center)
                && !CellFinder.TryFindRandomCellNear(pawns[0].Position, map, 25, Clear, out center)) return false;
            // Árboles y arbustos fuera (tapan a los colonos); el pasto queda.
            foreach (var plant in CellRect.CenteredOn(center, 4).Cells.SelectMany(c => c.GetThingList(map)).OfType<Plant>()
                         .Where(p => p.def.plant.IsTree || p.def.passability != Traversability.Standable || p.def.plant.visualSizeRange.max > 1f).ToList())
                plant.Destroy();
            // Ni trozos de roca ni objetos sueltos en la escena.
            foreach (var item in CellRect.CenteredOn(center, 3).Cells.SelectMany(c => c.GetThingList(map)).Where(t => t.def.category == ThingCategory.Item).ToList())
                item.Destroy();
            // Sin cadáveres ni esqueletos a la vista.
            foreach (var corpse in map.listerThings.ThingsInGroup(ThingRequestGroup.Corpse).Where(c => c.Position.InHorDistOf(center, 25f)).ToList())
                corpse.Destroy();

            var spots = new[]
            {
                Place("CVG_MeditationPlatform", center),
                Place("CVG_CultivationCushion", center + new IntVec3(-2, 0, -1)),
                Place("CVG_CultivationCushion", center + new IntVec3(2, 0, -1)),
            };
            var elements = new[] { "CVG_Element_Fire", "CVG_Element_Water", "CVG_Element_Wood" };
            var realms = new[] { "CVG_Realm_GoldenCore", "CVG_Realm_Foundation", "CVG_Realm_QiRefining" };
            for (int i = 0; i < 3; i++)
            {
                var p = pawns[i];
                var comp = CompCultivator.For(p);
                var element = DefDatabase<QiElementDef>.GetNamed(elements[i]);
                if (!comp.HasRoot) CultivationGenerator.GiveRoot(p, CultivationGenerator.RandomGrade(), new List<QiElementDef> { element });
                else CultivationGenerator.AddRootElement(p, element);
                comp.Sync();
                comp.SyncRealm();
                var tracker = comp.Realm;
                tracker.EnsureStarted();
                while (tracker.Realm != null && tracker.Realm.defName != realms[i] && tracker.NextRealm != null)
                    BreakthroughUtility.Ascend(p, tracker.NextRealm, 1f);
                tracker.AddProgress(tracker.Realm.progressPerStage);
                tracker.Resource.Fill();
                if (spots[i] == null) continue;
                p.jobs.StopAll();
                p.pather.StopDead();
                p.Position = spots[i].Position;
                p.Notify_Teleported();
                p.jobs.StartJob(JobMaker.MakeJob(CVG_DefOf.CVG_Cultivate, spots[i]), JobCondition.InterruptForced);
            }

            // Técnicas del Núcleo Dorado: el camino del fuego abierto y varias técnicas, para la ventana Desarrollo y los botones.
            var hero = pawns[0];
            var t = CompCultivator.For(hero).Techniques;
            t.InitializePoints(CompCultivator.For(hero));
            var sun = DefDatabase<TechniqueManualDef>.GetNamed("CVG_Manual_BlazingSun");
            if (t.CanOpenPath(sun, out _, ignorePoints: true)) t.OpenPath(sun, free: true);
            t.LearnManual(sun, sendMessage: false);
            t.AddPoints(3);
            foreach (var name in new[] { "CVG_Tech_Fireball", "CVG_Tech_SeaOfFlames" })
                t.Unlock(DefDatabase<AbilityDef>.GetNamed(name), free: true);
            foreach (var def in sun.Techniques.Where(d => TechniqueUtility.Ext(d).tier <= 2).Take(4).ToList())
                t.Unlock(def, free: true);
            return true;
        }

        private static Thing Place(string defName, IntVec3 cell)
        {
            var def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
            if (def == null || !cell.InBounds(map)) return null;
            var thing = ThingMaker.MakeThing(def, def.MadeFromStuff ? GenStuff.DefaultStuffFor(def) : null);
            thing.SetFaction(Faction.OfPlayer);
            return GenSpawn.Spawn(thing, cell, map, Rot4.South);
        }

        private static void Cast(Pawn caster, Ability ability, Pawn target)
        {
            if (ability == null) { Log.Warning("[Convergence] WorkshopShot: technique missing"); return; }
            CompCultivator.For(caster).Realm.Resource.Fill();
            ability.ResetCooldown();
            caster.rotationTracker.FaceCell(target.Position);
            ability.Activate(target, target);
        }

        private static void HealAll(Pawn p)
        {
            foreach (var h in p.health.hediffSet.hediffs.OfType<Hediff_Injury>().ToList()) p.health.RemoveHediff(h);
        }

        /// <summary>Saca el registro, las cartas, los mensajes y las ventanas abiertas (menos el inspector, si se pide).</summary>
        private static void CleanScreen(bool closeLetters, bool keepInspect = false)
        {
            foreach (var w in Find.WindowStack.Windows.ToList())
            {
                if (keepInspect && w is MainTabWindow_Inspect) continue;
                if (w is EditWindow_Log || w is Dialog_NodeTree || w is Dialog_CultivationDevelopment || w is MainTabWindow || w.layer == WindowLayer.Dialog)
                    w.Close(false);
            }
            Messages.Clear();
            if (closeLetters) Find.LetterStack.LettersListForReading.ToList().ForEach(l => Find.LetterStack.RemoveLetter(l));
        }

        /// <summary>El modo captura del juego (F11): sin interfaz.</summary>
        private static void HideUI(bool hide) => Find.UIRoot.screenshotMode.Active = hide;

        private static void Capture(string name)
        {
            var path = Path.Combine(dir, name + ".png");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            ScreenCapture.CaptureScreenshot(path);
        }
    }
}
