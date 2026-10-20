using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Isla de endgame para las pruebas: radio 18, Corazón con 6 monturas de núcleo, vasijas, 4 propulsores de Odyssey, altar del timonel,
    /// cúpula celestial con su red de qi y 10 salas con paredes de roca de nube, puerta y techo (comedor, cocina y despensa, salón ancestral,
    /// salón de cultivo, taller, enfermería y 4 dormitorios), cada una con su mobiliario y formación del aliento donde hace falta.
    /// </summary>
    public static class IslandEndgameBuilder
    {
        public class RoomInfo
        {
            public string name;
            public CellRect interior;
            public IntVec3 probe;
            public IntVec3 door;
            public Thing breath;
            public int placed, skipped;
            public List<string> missing = new List<string>();
        }

        public class Result
        {
            public Building_GravEngine engine;
            public Thing altar, dome;
            public readonly List<Thing> vessels = new List<Thing>();
            public readonly List<Thing> thrusters = new List<Thing>();
            public readonly List<Thing> mounts = new List<Thing>();
            public readonly List<RoomInfo> rooms = new List<RoomInfo>();
            public readonly List<string> notes = new List<string>();
            public int substructureCells;
            public readonly List<Thing> producers = new List<Thing>();
        }

        public const float Radius = 18.4f;

        private static Map map;
        private static IntVec3 center;

        private static Thing Make(string defName, IntVec3 cell, Rot4 rot)
        {
            var d = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
            if (d == null) return null;
            var t = ThingMaker.MakeThing(d, d.MadeFromStuff ? GenStuff.DefaultStuffFor(d) : null);
            t.SetFactionDirect(Faction.OfPlayer);
            GenSpawn.Spawn(t, cell, map, rot, WipeMode.Vanish);
            return t;
        }

        private static bool Free(ThingDef d, IntVec3 cell, Rot4 rot, CellRect within, HashSet<IntVec3> reserved, bool edge = false)
        {
            var rect = GenAdj.OccupiedRect(cell, rot, d.size);
            foreach (var c in rect)
            {
                if (!c.InBounds(map) || !within.Contains(c) || (reserved != null && reserved.Contains(c))) return false;
                if (!edge && (c - center).LengthHorizontal > Radius - 0.6f) return false;
                if (edge && (c - center).LengthHorizontal > Radius + 0.4f) return false;
                if (c.GetEdifice(map) != null) return false;
                foreach (var t in c.GetThingList(map)) if (t.def.category == ThingCategory.Building || t.def.category == ThingCategory.Plant && t.def.plant != null && t.def.plant.IsTree) return false;
            }
            return true;
        }

        private static Thing PlaceIn(string defName, CellRect rect, IntVec3 farFrom, HashSet<IntVec3> reserved)
        {
            var d = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
            if (d == null) return null;
            var cells = rect.Cells.OrderByDescending(c => (c - farFrom).LengthHorizontalSquared).ToList();
            foreach (var rot in d.rotatable ? new[] { Rot4.South, Rot4.East } : new[] { Rot4.South })
                foreach (var c in cells)
                    if (Free(d, c, rot, rect, reserved)) return Make(defName, c, rot);
            return null;
        }

        private static Thing Place(string defName, IntVec3 cell, Rot4 rot, Result r, string what)
        {
            var d = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
            if (d == null) { r.notes.Add("missing def " + defName); return null; }
            var rect = GenAdj.OccupiedRect(cell, rot, d.size);
            if (!Free(d, cell, rot, rect.ExpandedBy(1), null)) { r.notes.Add(what + ": no room for " + defName + " at " + cell); return null; }
            return Make(defName, cell, rot);
        }

        public static Result Build(Map m)
        {
            map = m; center = m.Center;
            var r = new Result();
            // despejar y poner roca de nube
            foreach (var c in GenRadial.RadialCellsAround(center, 30f, true).Where(x => x.InBounds(map)))
            {
                map.roofGrid.SetRoof(c, null);
                foreach (var t in c.GetThingList(map).ToList())
                    if (t.def.category != ThingCategory.Pawn && !(t is Corpse) && t.def.destroyable) t.Destroy();
            }
            var cloud = DefDatabase<TerrainDef>.GetNamed("CVG_CloudRock");
            foreach (var c in GenRadial.RadialCellsAround(center, Radius, true)) map.terrainGrid.SetTerrain(c, cloud);
            var grass = TerrainDefOf.Soil;

            // ------------------------------------------------------------ plaza central
            r.engine = (Building_GravEngine)Make("CVG_IslandHeart", center, Rot4.North);
            foreach (var o in new[] { (-6, -3), (6, -3), (-6, 3), (6, 3), (-3, -4), (3, -4) })
            {
                var mt = Place("CVG_GravityCoreMount", center + new IntVec3(o.Item1, 0, o.Item2), Rot4.North, r, "plaza");
                if (mt != null) r.mounts.Add(mt);
            }
            r.altar = Place("CVG_HelmsmanAltar", center + new IntVec3(0, 0, -6), Rot4.North, r, "plaza");
            foreach (var o in new[] { (-4, 4), (4, 4) })
            {
                var v = Place("CVG_EssenceVessel", center + new IntVec3(o.Item1, 0, o.Item2), Rot4.North, r, "plaza");
                if (v != null) { v.TryGetComp<CompRefuelable>()?.Refuel(300f); r.vessels.Add(v); }
            }
            // propulsores de Odyssey: tienen que estar en el ultimo borde de la isla, con el chorro (5 casillas) saliendo al vacio sobre
            // casillas SIN suelo de nave; si no, no se enlazan al corazon. Se prueba de a una casilla hasta que quedan enlazados.
            r.engine.ForceSubstructureDirty();
            foreach (var x in new[] { -9, -5, -1, 3 })
            {
                int zEdge = Mathf.CeilToInt(-Mathf.Sqrt(Radius * Radius - x * x));
                Thing placed = null;
                for (int dz = 0; dz <= 5 && placed == null; dz++)
                {
                    var cell = center + new IntVec3(x, 0, zEdge + dz);
                    var d = DefDatabase<ThingDef>.GetNamedSilentFail("SmallThruster");
                    if (d == null || !Free(d, cell, Rot4.North, GenAdj.OccupiedRect(cell, Rot4.North, d.size).ExpandedBy(1), null, edge: true)) continue;
                    var t = Make("SmallThruster", cell, Rot4.North);
                    r.engine.ForceSubstructureDirty();
                    var fac = t.TryGetComp<CompGravshipFacility>();
                    if (fac != null && fac.engine == r.engine) { placed = t; r.notes.Add("thruster at x " + x + ": linked at z " + (zEdge + dz) + " (edge " + zEdge + ")"); }
                    else { t.Destroy(); }
                }
                if (placed != null) r.thrusters.Add(placed); else r.notes.Add("thruster at x " + x + ": could not be linked (the 5 cells behind it must be free of ship floor)");
            }

            // ------------------------------------------------------------ red de qi y cúpula
            r.dome = Place("CVG_CelestialDome", center + new IntVec3(0, 0, -3), Rot4.North, r, "plaza");
            var crystals = new List<Thing>();
            foreach (var o in new[] { (-7, 4), (7, 4), (-7, -4), (7, -4) })
            {
                Place("CVG_FormationPillar", center + new IntVec3(o.Item1, 0, o.Item2 > 0 ? o.Item2 + 1 : o.Item2 - 1), Rot4.North, r, "qi");
                var c = Place("CVG_QiCrystal", center + new IntVec3(o.Item1, 0, o.Item2), Rot4.North, r, "qi");
                if (c != null) crystals.Add(c);
            }
            Place("CVG_FormationPillar", center + new IntVec3(0, 0, 4), Rot4.North, r, "qi");
                        Place("CVG_QiCrystal", center + new IntVec3(2, 0, 4), Rot4.North, r, "qi");

            // ------------------------------------------------------------ fuentes de qi al aire libre (borde norte), una de cada tipo
            Place("CVG_FormationPillar", center + new IntVec3(0, 0, 14), Rot4.North, r, "qi sources");
            int px = -9;
            foreach (var def in new[] { "CVG_QiVeinWell", "CVG_QiGatherArray", "CVG_SkyBreathArray", "CVG_SunLantern", "CVG_ThunderRod", "CVG_SpiritBrazier", "CVG_QiGatheringCore" })
            {
                var t = Place(def, center + new IntVec3(px, 0, 15), Rot4.North, r, "qi sources");
                if (t != null) { r.producers.Add(t); if (t.TryGetComp<CompRefuelable>() is CompRefuelable rf) rf.Refuel(20f); }
                px += 3;
            }

            // ------------------------------------------------------------ salas
            Room(r, "dining hall", -12, 7, 7, 5, 'S', new[] { ("CVG_BanquetTable", 1), ("CVG_CarvedChair", 6), ("CVG_StoneBench", 1), ("CVG_PaperLantern", 2), ("CVG_SpiritBonsai", 1) }, true, "CVG_QiBarrier");
            Room(r, "kitchen and pantry", 6, 7, 7, 5, 'S', new[] { ("CVG_SectKitchen", 2), ("CVG_SpiritWinery", 1), ("CVG_WarehouseShelf", 2), ("CVG_PillCabinet", 1), ("CVG_GrainJar", 2), ("CVG_ButcherBlock", 1), ("CVG_SpiritWineJar", 2) }, false);
            Room(r, "ancestral hall", -3, 7, 7, 4, 'S', new[] { ("CVG_AncestralAltar", 1), ("CVG_ScriptureStele", 1), ("CVG_RitualBrazier", 2), ("CVG_BronzeCenser", 1), ("CVG_ScriptureShelf", 2), ("CVG_CalligraphyDesk", 1) }, false);
            Room(r, "cultivation hall", -15, -4, 7, 8, 'E', new[] { ("CVG_CultivationCushion", 4), ("CVG_CultivationSpot", 2), ("CVG_MeditationPlatform", 1), ("CVG_BronzeCenser", 2), ("CVG_SpiritJadeDais", 1), ("CVG_SpiritStoneLantern", 1) }, true);
            Room(r, "workshop", 9, -4, 7, 8, 'W', new[] { ("CVG_AlchemyFurnace", 1), ("CVG_SpiritForge", 1), ("CVG_RuneTable", 1), ("CVG_TalismanTable", 1), ("CVG_SpiritLoom", 1), ("CVG_JadeCarvingTable", 1), ("CVG_WarehouseShelf", 1) }, false);
            Room(r, "infirmary", 6, -9, 6, 3, 'W', new[] { ("CVG_HealerBed", 3), ("CVG_PillCabinet", 1), ("CVG_BedsideTray", 2) }, true, "CVG_QiBarrier");
            int n = 1;
            foreach (var x in new[] { -12, -8, -4, 0 })
                Room(r, "bedroom " + n++, x, -10, 3, 3, 'N', new[] { ("CVG_SectBed", 1), ("CVG_BedsideTray", 1), ("CVG_SectChest", 1), ("CVG_PaperLantern", 1) }, x == -12);

            // qi de las cristales
            var mgr = map.GetComponent<QiNetworkManager>();
            mgr.EnsureBuilt();
            foreach (var c in crystals) c.TryGetComp<CompQiNode>().Stored = 900f;
            mgr.Step(0.01f);
            Place("CVG_SkyStake", center + new IntVec3(-26, 0, -20), Rot4.North, r, "anchor");
            r.engine.ForceSubstructureDirty();
            typeof(Building_GravEngine).GetField("haveShownNameDialog", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)?.SetValue(r.engine, true);
            r.substructureCells = r.engine.ValidSubstructure.Count;
            return r;
        }

        /// <summary>Sala con paredes de roca de nube, techo, una puerta y mobiliario repartido por las casillas más lejos de la puerta.</summary>
        private static void Room(Result r, string name, int x0, int z0, int w, int h, char doorSide, (string, int)[] furniture, bool breath, string doorDef = "CVG_LacqueredDoor")
        {
            var info = new RoomInfo { name = name };
            var interior = new CellRect(center.x + x0, center.z + z0, w, h);
            info.interior = interior;
            var outer = interior.ExpandedBy(1);
            IntVec3 door = doorSide == 'S' ? new IntVec3(interior.minX + w / 2, 0, interior.minZ - 1)
                         : doorSide == 'N' ? new IntVec3(interior.minX + w / 2, 0, interior.maxZ + 1)
                         : doorSide == 'W' ? new IntVec3(interior.minX - 1, 0, interior.minZ + h / 2)
                         : new IntVec3(interior.maxX + 1, 0, interior.minZ + h / 2);
            door.y = 0;
            info.door = door;
            foreach (var c in outer.EdgeCells)
            {
                if (!c.InBounds(map) || (c - center).LengthHorizontal > Radius - 0.4f) { info.missing.Add("wall outside the island at " + c); continue; }
                if (c.GetEdifice(map) != null && c != door) continue;
                Make(c == door ? doorDef : "CVG_CloudRockWall", c, doorSide == 'E' || doorSide == 'W' ? Rot4.East : Rot4.North);
            }
            foreach (var c in interior) map.roofGrid.SetRoof(c, RoofDefOf.RoofConstructed);
            info.probe = interior.CenterCell;
            // el paso desde la puerta se deja libre
            var reserved = new HashSet<IntVec3>();
            var step = doorSide == 'S' ? IntVec3.North : doorSide == 'N' ? IntVec3.South : doorSide == 'W' ? IntVec3.East : IntVec3.West;
            for (int i = 1; i <= 2; i++) reserved.Add(door + step * i);
            foreach (var (def, count) in furniture)
                for (int i = 0; i < count; i++)
                {
                    var t = PlaceIn(def, interior, door, reserved);
                    if (t != null) info.placed++; else { info.skipped++; info.missing.Add(def); }
                }
            // las paredes: cualquier hueco o pieza que haya tapado la pared se anota
            foreach (var c in outer.EdgeCells)
            {
                if (!c.InBounds(map) || (c - center).LengthHorizontal > Radius - 0.4f) continue;
                var e = c.GetEdifice(map);
                if (c == door) continue;
                if (e == null || e.def.defName != "CVG_CloudRockWall") info.missing.Add("wall cell " + c + " holds " + (e?.def.defName ?? "nothing"));
            }
            if (breath)
            {
                info.breath = PlaceIn("CVG_BreathFormation", interior, door, reserved);
                if (info.breath != null) info.placed++; else { info.skipped++; info.missing.Add("CVG_BreathFormation"); }
            }
            r.rooms.Add(info);
        }
    }
}
