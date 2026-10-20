using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>Autotest de la red de qi: solo con <c>-cvgqinettest</c> (y un mapa, p. ej. -quicktest). Inerte sin el argumento.</summary>
    public class GameComponent_QiNetTest : GameComponent
    {
        private static readonly bool Enabled = GenCommandLine.CommandLineArgPassed("cvgqinettest");
        private bool done;
        private readonly StringBuilder sb = new StringBuilder("[Convergence] Qi network self-test");
        private int fails;
        private readonly List<Thing> spawned = new List<Thing>();

        public GameComponent_QiNetTest(Game game) { }

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

        private Thing Spawn(string def, IntVec3 cell, Map map)
        {
            var t = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed(def), GenStuff.DefaultStuffFor(DefDatabase<ThingDef>.GetNamed(def)));
            t.SetFactionDirect(Faction.OfPlayer);
            GenSpawn.Spawn(t, cell, map);
            spawned.Add(t);
            return t;
        }

        private void Run(Map map)
        {
            var vein = QiNetworkUtility.VeinTerrain;
            Check(vein != null, "qi vein terrain defined");
            var wellDef = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_QiVeinWell");
            var workshopDef = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_PuppetWorkshop");
            if (vein == null || wellDef == null) { sb.Append("\n  (Formations not loaded: skipped)"); return; }
            GameComponent_Convergence.Instance?.MarkOccurred();
            QiNetworkUtility.EnsureVeins(map);
            var veinCells = map.AllCells.Where(c => map.terrainGrid.TerrainAt(c) == vein).ToList();
            Check(veinCells.Count >= 9, veinCells.Count + " qi vein cells generated");
            var origin = veinCells.FirstOrDefault(c => veinCells.Contains(c + IntVec3.East) && veinCells.Contains(c + IntVec3.North) && veinCells.Contains(c + IntVec3.NorthEast));
            Check(origin.IsValid, "a 2x2 vein spot exists");
            var pw = new PlaceWorker_OnQiVein();
            Check(pw.AllowsPlacing(wellDef, origin, Rot4.North, map).Accepted, "the well can be built on a vein");
            var off = CellFinderLoose.TryGetRandomCellWith(c => c.Standable(map) && map.terrainGrid.TerrainAt(c) != vein && c.DistanceTo(origin) > 30f, map, 200, out var offCell) ? offCell : IntVec3.Invalid;
            Check(off.IsValid && !pw.AllowsPlacing(wellDef, off, Rot4.North, map).Accepted, "the well cannot be built off a vein");

            var mgr = map.GetComponent<QiNetworkManager>();
            var well = Spawn("CVG_QiVeinWell", origin, map);
            var pillarCell = CellFinder.StandableCellNear(origin + new IntVec3(4, 0, 0), map, 3f);
            var pillar = Spawn("CVG_FormationPillar", pillarCell, map);
            var shopCell = CellFinder.StandableCellNear(pillarCell + new IntVec3(0, 0, 5), map, 4f, c => c.DistanceTo(pillarCell) <= 8f);
            var shop = workshopDef != null ? Spawn("CVG_PuppetWorkshop", shopCell, map) : null;
            var crystal = Spawn("CVG_QiCrystal", CellFinder.StandableCellNear(pillarCell + new IntVec3(-3, 0, 0), map, 3f), map);
            mgr.EnsureBuilt();
            var wn = well.TryGetComp<CompQiNode>();
            Check(wn.Net != null && wn.Net == pillar.TryGetComp<CompQiNode>().Net, "well and pillar share a net");
            Check(wn.ProduceDay >= 159f, "well produces " + wn.ProduceDay.ToString("0") + " qi/day on the vein");
            if (shop != null)
            {
                var sn = shop.TryGetComp<CompQiNode>();
                Check(sn.Net == wn.Net, "workshop inside the pillar radius joins the net");
                Check(sn.Powered && shop.TryGetComp<CompPowerTrader>().PowerOn, "workshop is powered by qi (CompPowerTrader.PowerOn follows the net)");
                var text = shop.GetInspectString();
                Check(!text.Contains("PowerNotConnected".Translate().ToString()) && text.Contains(wn.Net.SupplyDay.ToString("F0")), "no electricity text, qi network status shown");
            }
            // acumulador: carga con el excedente y mantiene la red cuando falta la fuente
            var cn = crystal.TryGetComp<CompQiNode>();
            mgr.Step(1f);
            Check(cn.Stored > 50f, "crystal stores the surplus (" + cn.Stored.ToString("0") + ")");
            // estanque de condensación: con el cristal lleno, el sobrante se vuelve piedras espirituales
            if (DefDatabase<ThingDef>.GetNamedSilentFail("CVG_CondensationPool") != null)
            {
                var pool = Spawn("CVG_CondensationPool", CellFinder.StandableCellNear(pillarCell + new IntVec3(0, 0, 3), map, 3f), map);
                mgr.EnsureBuilt();
                var stoneDef = DefDatabase<ThingDef>.GetNamed("CVG_SpiritStone");
                int before = map.listerThings.ThingsOfDef(stoneDef).Sum(t => t.stackCount);
                cn.Stored = cn.Props.storageCapacity;
                mgr.Step(1f);
                int after = map.listerThings.ThingsOfDef(stoneDef).Sum(t => t.stackCount);
                Check(pool.TryGetComp<CompQiNode>().Net == wn.Net && after > before, $"condensation pool turns surplus into spirit stones ({before} -> {after})");
                pool.Destroy();
                mgr.EnsureBuilt();
            }
            well.DeSpawn();
            mgr.EnsureBuilt();
            mgr.Step(0.25f);
            Check(shop == null || shop.TryGetComp<CompQiNode>().Powered, "the crystal keeps the workshop running after the well is gone");
            mgr.Step(30f);
            Check(shop == null || (!shop.TryGetComp<CompQiNode>().Powered && !shop.TryGetComp<CompPowerTrader>().PowerOn), "without source or charge the workshop switches off");
            // fuera de radio: red aparte, sin qi
            if (workshopDef != null)
            {
                var farCell = CellFinder.StandableCellNear(pillarCell + new IntVec3(30, 0, 0), map, 6f, c => c.DistanceTo(pillarCell) > 18f);
                var far = Spawn("CVG_PuppetWorkshop", farCell, map);
                mgr.EnsureBuilt();
                Check(far.TryGetComp<CompQiNode>().Net != pillar.TryGetComp<CompQiNode>().Net && !far.TryGetComp<CompQiNode>().Powered, "a workshop outside every pillar radius stays unpowered");
            }
            // formación de reunión: sin piedras no produce; con piedras sí, según la concentración
            var arrayDef = DefDatabase<ThingDef>.GetNamed("CVG_QiGatherArray");
            var array = Spawn("CVG_QiGatherArray", CellFinder.StandableCellNear(pillarCell + new IntVec3(0, 0, -3), map, 3f), map);
            var an = array.TryGetComp<CompQiNode>();
            var fuel = array.TryGetComp<CompRefuelable>();
            fuel.ConsumeFuel(fuel.Fuel); // arranca con medio tanque: se vacía para probar el caso sin piedras
            Check(an.ProduceDay == 0f, "the gathering array produces nothing without spirit stones");
            array.TryGetComp<CompRefuelable>().Refuel(10f);
            Check(an.ProduceDay > 20f, "with stones it produces " + an.ProduceDay.ToString("0") + " qi/day");
            Check(DefDatabase<JobDef>.GetNamedSilentFail("CVG_InfuseQi") != null, "infuse-qi job exists");
        }
    }
}
