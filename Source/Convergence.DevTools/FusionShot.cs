using System.IO;
using System.Linq;
using RimWorld;
using UnityEngine;
using LudeonTK;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Captura de revisión del plan de fusión (<c>-cvgfusionshot</c>, instancia de prueba 1.6): edificios de Qi-Tech en sus
    /// rotaciones, la bandeja hidropónica con plantas crecidas (de frente y de costado), colonos con la armadura de qi en las
    /// tres vistas, una torreta de qi junto a la vanilla, un meca de qi y los objetos nuevos en el suelo. Sin su argumento no
    /// hace nada. Guarda en <c>cvg_fusionshot/</c> y cierra el juego.
    /// </summary>
    public static class FusionShot
    {
        private static int state, wait, step;
        private static string dir;
        private static IntVec3 center;

        public static void Update()
        {
            if (state == 9 || Current.ProgramState != ProgramState.Playing || Find.CurrentMap == null) return;
            if (state == 0)
            {
                if (!GenCommandLine.CommandLineArgPassed("cvgfusionshot")) { state = 9; return; }
                state = 1;
            }
            var map = Find.CurrentMap;
            if (state == 1)
            {
                if (Find.TickManager.TicksGame < 120) return;
                Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
                dir = Path.Combine(GenFilePaths.SaveDataFolderPath, "cvg_fusionshot");
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
                Directory.CreateDirectory(dir);
                state = 2; wait = 60; step = 0;
                try { Place(map); }
                catch (System.Exception e)
                {
                    Log.Error("[Convergence] Fusion shot: " + e + "\n  FAILURES: 1");
                    state = 9;
                    Root.Shutdown();
                    return;
                }
                int cur = GenLocalDate.HourOfDay(map);
                int add = ((12 - cur) + 24) % 24 * GenDate.TicksPerHour;
                if (add > 0) Find.TickManager.DebugSetTicksGame(Find.TickManager.TicksGame + add);
                return;
            }
            if (--wait > 0) return;
            foreach (var w in Find.WindowStack.Windows.OfType<EditWindow_Log>().ToList()) w.Close(false);
            // Vistas: todo, edificios, bandeja y colonos de cerca, objetos.
            var views = new (string name, IntVec3 at, float size)[]
            {
                ("overview", center + new IntVec3(0, 0, 2), 22f),
                ("buildings", center + new IntVec3(0, 0, 10), 12f),
                ("hydroponics", center + new IntVec3(-15, 0, 3), 6f),
                ("armor", center + new IntVec3(2, 0, 0), 4f),
                ("turrets_items", center + new IntVec3(6, 0, -3), 8f),
            };
            if (step < views.Length)
            {
                var v = views[step];
                if (state == 2) { Find.CameraDriver.SetRootPosAndSize(v.at.ToVector3Shifted(), v.size); state = 3; wait = 25; return; }
                ScreenCapture.CaptureScreenshot(Path.Combine(dir, $"{step}_{v.name}.png"));
                step++; state = 2; wait = 15; return;
            }
            Log.Message("[Convergence] Fusion shots saved in " + dir + "\n  ALL OK");
            state = 9;
            Root.Shutdown();
        }

        private static ThingDef T(string name) => DefDatabase<ThingDef>.GetNamedSilentFail(name);

        private static Thing Build(Map map, string def, IntVec3 cell, Rot4 rot)
        {
            var d = T(def);
            if (d == null) { Log.Warning("[Convergence] Fusion shot: no ThingDef " + def); return null; }
            var t = ThingMaker.MakeThing(d, d.MadeFromStuff ? GenStuff.DefaultStuffFor(d) : null);
            t.SetFaction(Faction.OfPlayer);
            return GenSpawn.Spawn(t, cell, map, rot);
        }

        private static void Place(Map map)
        {
            center = map.Center;
            var rect = CellRect.CenteredOn(center, 22);
            foreach (var c in rect)
            {
                if (!c.InBounds(map)) continue;
                foreach (var t in c.GetThingList(map).ToList()) if (t.def.destroyable) t.Destroy();
                map.roofGrid.SetRoof(c, null);
                map.terrainGrid.SetTerrain(c, TerrainDefOf.Soil);
            }
            map.fogGrid.ClearAllFog();

            // Edificios en sus rotaciones (arriba a la izquierda).
            int x = -20;
            foreach (var (def, rots) in new (string, Rot4[])[]
            {
                ("CVG_QiForge", new[] { Rot4.South, Rot4.East, Rot4.North }),
                ("CVG_QT_PillLab", new[] { Rot4.South, Rot4.East, Rot4.North }),
                ("CVG_QT_QiAccumulator", new[] { Rot4.South, Rot4.East, Rot4.North }),
                ("CVG_QT_MechTuningStation", new[] { Rot4.South, Rot4.East, Rot4.North }),
            })
            {
                var d = T(def);
                if (d == null) continue;
                foreach (var rot in rots)
                {
                    Build(map, def, center + new IntVec3(x, 0, 10), rot);
                    x += Mathf.Max(d.size.x, d.size.z) + 1;
                }
            }

            // Bandejas hidropónicas: de frente (East, horizontal como vanilla) y de costado (South), con plantas crecidas.
            var basins = new[] { (center + new IntVec3(-18, 0, 3), Rot4.East), (center + new IntVec3(-18, 0, 1), Rot4.East), (center + new IntVec3(-11, 0, 2), Rot4.South) };
            for (int i = 0; i < basins.Length; i++)
            {
                var basin = Build(map, "CVG_QT_SpiritHydroponics", basins[i].Item1, basins[i].Item2);
                if (basin == null) continue;
                basin.TryGetComp<CompRefuelable>()?.Refuel(20f);
                var herb = DefDatabase<ThingDef>.AllDefsListForReading.FirstOrDefault(d => d.plant != null && d.defName == "CVG_Plant_SpiritHerb")
                           ?? DefDatabase<ThingDef>.AllDefsListForReading.FirstOrDefault(d => d.plant != null && d.defName.StartsWith("CVG_") && d.plant.sowTags.Contains("CVG_SpiritHydroponic"));
                var plantDef = i == 1 && herb != null ? herb : T("Plant_Rice");
                foreach (var c in basin.OccupiedRect())
                {
                    var plant = (Plant)ThingMaker.MakeThing(plantDef);
                    plant.Growth = 0.9f;
                    GenSpawn.Spawn(plant, c, map);
                }
            }
            // Bandeja vanilla para comparar.
            var vanilla = Build(map, "HydroponicsBasin", center + new IntVec3(-18, 0, 5), Rot4.East);
            if (vanilla != null)
                foreach (var c in vanilla.OccupiedRect())
                {
                    var plant = (Plant)ThingMaker.MakeThing(T("Plant_Rice"));
                    plant.Growth = 0.9f;
                    GenSpawn.Spawn(plant, c, map);
                }

            // Colonos con la armadura de qi en sur, este y norte; uno con escopeta de qi.
            var rots3 = new[] { Rot4.South, Rot4.East, Rot4.North };
            for (int i = 0; i < 3; i++)
            {
                var p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayer, forceGenerateNewPawn: true, tile: map.Tile));
                GenSpawn.Spawn(p, center + new IntVec3(i * 2, 0, 0), map, rots3[i]);
                foreach (var a in new[] { "CVG_Apparel_QiHelmet", "CVG_Apparel_QiVest", "CVG_Apparel_QiPants" })
                    if (T(a) is ThingDef ad) p.apparel.Wear((Apparel)ThingMaker.MakeThing(ad), dropReplacedApparel: false);
                if (i == 0 && T("CVG_QT_QiShotgun") is ThingDef gun)
                {
                    p.equipment.DestroyAllEquipment();
                    p.equipment.AddEquipment((ThingWithComps)ThingMaker.MakeThing(gun));
                    p.drafter.Drafted = true;
                }
                p.Rotation = rots3[i];
                p.Drawer?.renderer?.SetAllGraphicsDirty();
            }

            // Torreta de qi III junto a la mini-torreta vanilla; un meca de qi.
            Build(map, "Turret_MiniTurret", center + new IntVec3(8, 0, 0), Rot4.North);
            Build(map, "CVG_QiTurret3_Turret_MiniTurret", center + new IntVec3(10, 0, 0), Rot4.North);
            var militor = DefDatabase<PawnKindDef>.GetNamedSilentFail("Mech_Militor");
            var core = DefDatabase<HediffDef>.GetNamedSilentFail("CVG_MechQiCore");
            if (militor != null)
            {
                var mech = PawnGenerator.GeneratePawn(new PawnGenerationRequest(militor, Faction.OfPlayer));
                if (core != null) mech.health.AddHediff(core).Severity = 2;
                GenSpawn.Spawn(mech, center + new IntVec3(12, 0, 0), map, Rot4.South);
                var plain = PawnGenerator.GeneratePawn(new PawnGenerationRequest(militor, Faction.OfPlayer));
                GenSpawn.Spawn(plain, center + new IntVec3(14, 0, 0), map, Rot4.South);
            }

            // Objetos nuevos en el suelo.
            string[] items =
            {
                "CVG_QiCannon_I", "CVG_QiCannon_II", "CVG_QiCannon_III", "CVG_QT_MechQiCore_Basic", "CVG_QT_MechQiCore_Medium",
                "CVG_QT_MechQiCore_Advanced", "CVG_ChaosCore", "CVG_ChaosTalisman_Ruin", "CVG_ChaosTalisman_Dread",
                "CVG_ChaosTalisman_Mending", "CVG_ChaosBloodPill", "CVG_Fish_SilkKoi", "CVG_QT_Shell_Qi", "CVG_QT_QiPistol",
                "CVG_QiBionicArm", "CVG_QiBionicLeg", "CVG_QiBionicEye", "CVG_Apparel_QiVest", "CVG_Apparel_QiPants", "CVG_Apparel_QiHelmet"
            };
            int ix = 0;
            int iz = -5;
            foreach (var name in items)
            {
                var d = T(name);
                if (d == null) continue;
                var t = ThingMaker.MakeThing(d, d.MadeFromStuff ? GenStuff.DefaultStuffFor(d) : null);
                GenSpawn.Spawn(t, center + new IntVec3(ix, 0, iz), map);
                ix++;
                if (ix > 12) { ix = 0; iz -= 2; }
            }
        }
    }
}
