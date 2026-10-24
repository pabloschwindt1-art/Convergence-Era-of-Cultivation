using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Convergence
{
    /// <summary>Autotest de los muebles de vida (Convergence: Furniture, parte 3): solo con <c>-cvgfurnituretest</c> (y un mapa, p. ej. -quicktest).</summary>
    public class GameComponent_FurnitureTest : GameComponent
    {
        private static readonly bool Enabled = GenCommandLine.CommandLineArgPassed("cvgfurnituretest");
        private bool done;
        private readonly StringBuilder sb = new StringBuilder("[Convergence] Furniture (life furniture) self-test");
        private int fails;
        private readonly List<Thing> spawned = new List<Thing>();

        public GameComponent_FurnitureTest(Game game) { }

        private void Check(bool ok, string what)
        {
            sb.Append("\n    " + (ok ? "ok   " : "FAIL ") + what);
            if (!ok) fails++;
        }

        public override void GameComponentTick()
        {
            if (!Enabled || done || Find.CurrentMap == null || Find.TickManager.TicksGame < 200) return;
            done = true;
            try { Run(Find.CurrentMap); }
            catch (Exception e) { Check(false, "exception: " + e); }
            foreach (var t in spawned) if (!t.Destroyed) t.Destroy();
            sb.Append(fails == 0 ? "\n  ALL OK" : "\n  FAILURES: " + fails);
            if (fails == 0) Log.Message(sb.ToString()); else Log.Error(sb.ToString());
        }

        private Thing Spawn(string def, IntVec3 cell, Map map, Rot4? rot = null)
        {
            var d = DefDatabase<ThingDef>.GetNamed(def);
            var t = ThingMaker.MakeThing(d, d.MadeFromStuff ? GenStuff.DefaultStuffFor(d) : null);
            t.SetFactionDirect(Faction.OfPlayer);
            GenSpawn.Spawn(t, cell, map, rot ?? Rot4.North);
            spawned.Add(t);
            return t;
        }

        private void Run(Map map)
        {
            var names = new[] { "CVG_SectDoubleBed", "CVG_HealerBed", "CVG_BanquetTable", "CVG_CarvedChair", "CVG_WarehouseShelf", "CVG_GrainJar", "CVG_SectChest", "CVG_MoonDoor", "CVG_LacqueredDoor",
                "CVG_MahjongTable", "CVG_TeaPavilion", "CVG_Guqin", "CVG_StoneBench", "CVG_LotusPlanter" };
            if (DefDatabase<ThingDef>.GetNamedSilentFail("CVG_HealerBed") == null) { sb.Append("\n  (Furniture not loaded: skipped)"); return; }
            foreach (var n in names)
            {
                var d = DefDatabase<ThingDef>.GetNamed(n);
                Check(!d.description.NullOrEmpty() && d.BuildableByPlayer, n + " defined and buildable");
            }
            Check(DefDatabase<TerrainDef>.GetNamedSilentFail("CVG_RoofTileFloor") != null, "roof-tile floor defined");
            bool bamboo = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_BambooWall") != null;
            Check(bamboo == (DefDatabase<ThingDef>.GetNamedSilentFail("CVG_Bamboo") != null), "bamboo wall present exactly when bamboo exists");

            var center = map.Center;
            foreach (var c in GenRadial.RadialCellsAround(center, 12f, true).Where(x => x.InBounds(map)))
            {
                map.roofGrid.SetRoof(c, null);
                foreach (var t in c.GetThingList(map).ToList()) if (t.def.category != ThingCategory.Pawn && !(t is Corpse) && t.def.destroyable) t.Destroy();
            }

            var dbl = (Building_Bed)Spawn("CVG_SectDoubleBed", center, map);
            Check(dbl.SleepingSlotsCount == 2, "double bed has two sleeping slots");
            var healer = (Building_Bed)Spawn("CVG_HealerBed", center + new IntVec3(4, 0, 0), map);
            Check(healer.Medical && healer.SleepingSlotsCount == 1, "healer's bed is a medical bed");
            Check(healer.GetStatValue(StatDefOf.MedicalTendQualityOffset) > 0.05f, "healer's bed improves tending");

            var jar = Spawn("CVG_GrainJar", center + new IntVec3(-4, 0, 0), map);
            var jarStorage = (IStoreSettingsParent)jar;
            var meal = ThingMaker.MakeThing(ThingDefOf.MealSimple);
            var steel = ThingMaker.MakeThing(ThingDefOf.Steel);
            Check(jarStorage.GetStoreSettings().AllowedToAccept(meal) && !jarStorage.GetStoreSettings().AllowedToAccept(steel), "grain jar stores food only");
            meal.Destroy(); steel.Destroy();
            var chest = Spawn("CVG_SectChest", center + new IntVec3(-6, 0, 0), map);
            Check(chest is Building_Storage, "sect chest is a storage building");
            var shelf = Spawn("CVG_WarehouseShelf", center + new IntVec3(0, 0, 5), map);
            Check(shelf is Building_Storage && shelf.def.size.x == 3, "warehouse shelf is a three-wide storage shelf");

            var door = Spawn("CVG_MoonDoor", center + new IntVec3(0, 0, -6), map);
            var door2 = Spawn("CVG_LacqueredDoor", center + new IntVec3(2, 0, -6), map);
            Check(door is Building_Door && door2 is Building_Door, "moon and lacquered doors are real doors");
            Check(door2.MaxHitPoints > door.MaxHitPoints, "lacquered door is sturdier");

            // mesa de mahjong con cuatro asientos y un jugador
            var mj = Spawn("CVG_MahjongTable", center + new IntVec3(6, 0, 4), map);
            var seats = new[] { new IntVec3(-1, 0, 0), new IntVec3(2, 0, 0), new IntVec3(0, 0, -1), new IntVec3(1, 0, 2) };
            foreach (var s in seats) Spawn("CVG_CarvedChair", mj.Position + s, map);
            var pawn = map.mapPawns.FreeColonistsSpawned.First();
            pawn.Position.ToString();
            var giver = DefDatabase<JoyGiverDef>.GetNamed("CVG_Play_Mahjong");
            pawn.Drawer.tweener.ResetTweenedPosToRoot();
            pawn.needs.joy.CurLevel = 0.1f;
            var job = giver.Worker.TryGiveJob(pawn);
            Check(job != null && job.def.defName == "CVG_Play_Mahjong", "a colonist picks the mahjong table for recreation (" + (job?.def.defName ?? "no job") + ")");
            var gu = Spawn("CVG_Guqin", center + new IntVec3(-8, 0, 4), map);
            var gj = DefDatabase<JoyGiverDef>.GetNamed("CVG_Play_Guqin").Worker.TryGiveJob(pawn);
            Check(gj != null && gj.def.defName == "CVG_Play_Guqin", "a colonist picks the guqin (" + (gj?.def.defName ?? "no job") + ")");
            Check(DefDatabase<JoyKindDef>.GetNamed("CVG_Music") != null, "music joy kind exists");

            // revisión: lugares de cultivo, bandeja de cabecera, esterilla, sillón
            foreach (var spot in new[] { "CVG_SpiritJadeDais" })
            {
                var sd = DefDatabase<ThingDef>.GetNamed(spot);
                var ext = sd.GetModExtension<DefModExtension_CultivationSpot>();
                var cushion = DefDatabase<ThingDef>.GetNamed("CVG_CultivationCushion");
                Check(ext != null && ext.qiRegenFactor > cushion.GetModExtension<DefModExtension_CultivationSpot>().qiRegenFactor
                    && sd.GetModExtension<DefModExtension_FengShui>().qi > cushion.GetModExtension<DefModExtension_FengShui>().qi, spot + " is a better cultivation spot than the cushion");
            }
            var bed2 = (Building_Bed)Spawn("CVG_SectBed", center + new IntVec3(-10, 0, -8), map);
            float comfortBefore = bed2.GetStatValue(StatDefOf.Comfort);
            Thing tray = null;
            foreach (var cell in GenAdj.CellsAdjacentCardinal(bed2).ToList())
            {
                if (!cell.Standable(map)) continue;
                tray = Spawn("CVG_BedsideTray", cell, map);
                if (bed2.GetComp<CompAffectedByFacilities>().LinkedFacilitiesListForReading.Contains(tray)) break;
                tray.Destroy(); tray = null;
            }
            Check(tray != null && bed2.GetStatValue(StatDefOf.Comfort) > comfortBefore, "a bedside tray at the head of a sect bed raises its comfort");
            Check(Spawn("CVG_SleepingMat", center + new IntVec3(-10, 0, -4), map) is Building_Bed, "sleeping mat is a bed spot");
            Check(DefDatabase<ThingDef>.GetNamed("CVG_BambooRecliner").GetStatValueAbstract(StatDefOf.Comfort) >= 0.8f, "bamboo recliner is as comfortable as an armchair");

            if (bamboo)
            {
                var wall = Spawn("CVG_BambooWall", center + new IntVec3(8, 0, -4), map);
                Check(wall.def.holdsRoof && wall.def.passability == Traversability.Impassable && wall.def.costList.Any(c => c.thingDef.defName == "CVG_Bamboo"), "bamboo wall: impassable, holds a roof, costs bamboo");
            }
        }
    }
}
