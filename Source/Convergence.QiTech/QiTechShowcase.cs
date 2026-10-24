using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace Convergence.QiTech
{
    /// <summary>
    /// Vitrina para mirar Qi-Tech en combate (con <c>-cvgqitechshowcase</c> y <c>-quicktest</c>, ventana visible): pone las dos torretas
    /// con corriente y piedras, tres colonos reclutados con el rifle de inducción, el rifle concentrado y el cañón de compresión, centra
    /// la cámara y manda oleadas de enemigos que asaltan desde el este. Las torretas se recargan solas en cada oleada. Sin el argumento
    /// no hace nada; no escribe "ALL OK", así que run-test.ps1 deja el juego abierto hasta que se cierra o vence el tiempo.
    /// </summary>
    public class GameComponent_QiTechShowcase : GameComponent
    {
        private static readonly bool Enabled = GenCommandLine.CommandLineArgPassed("cvgqitechshowcase");
        private bool setUp;
        private IntVec3 center;
        private readonly List<Building_TurretGun> turrets = new List<Building_TurretGun>();
        private readonly List<Pawn> wave = new List<Pawn>();
        private int nextWave;

        public GameComponent_QiTechShowcase(Game game) { }

        public override void GameComponentTick()
        {
            if (!Enabled) return;
            var map = Find.CurrentMap;
            if (map == null || Find.TickManager.TicksGame < 120) return;
            if (!setUp) { setUp = true; SetUp(map); return; }
            wave.RemoveAll(p => p.Dead || !p.Spawned || p.Downed);
            if (wave.Count > 0) return;
            // Oleada derrotada: 10 s de respiro y otra.
            if (nextWave == 0) nextWave = Find.TickManager.TicksGame + 600;
            else if (Find.TickManager.TicksGame >= nextWave) { nextWave = 0; SendWave(map); }
        }

        private void SetUp(Map map)
        {
            center = map.Center;
            foreach (var cell in GenRadial.RadialCellsAround(center, 30f, true).Where(x => x.InBounds(map)))
            {
                foreach (var t in cell.GetThingList(map).ToList()) if (t.def.destroyable && !(t is Pawn)) t.Destroy();
                if (cell.GetTerrain(map).passability != Traversability.Standable || !cell.Standable(map)) map.terrainGrid.SetTerrain(cell, TerrainDefOf.Soil);
                map.roofGrid.SetRoof(cell, null);
            }
            turrets.Add(SpawnTurret(map, "CVG_QT_InductionTurret", center + new IntVec3(0, 0, 5)));
            turrets.Add(SpawnTurret(map, "CVG_QT_ContainmentTurret", center + new IntVec3(0, 0, -5)));

            var guns = new[] { "CVG_QT_InductionRifle", "CVG_QT_FocusedRifle", "CVG_QT_CompressionCannon" };
            for (int i = 0; i < guns.Length; i++)
            {
                var p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayer, forceGenerateNewPawn: true));
                p.equipment.DestroyAllEquipment();
                p.equipment.AddEquipment((ThingWithComps)ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed(guns[i])));
                p.apparel.Wear((Apparel)ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("CVG_Apparel_QiTechExosuit")), dropReplacedApparel: false);
                GenSpawn.Spawn(p, center + new IntVec3(-4, 0, 3 - i * 3), map);
                p.drafter.Drafted = true;
            }
            Find.CameraDriver.JumpToCurrentMapLoc(center + new IntVec3(6, 0, 0));
            Messages.Message("Qi-Tech showcase: turrets and qi weapons vs. waves from the east.", MessageTypeDefOf.NeutralEvent, false);
            nextWave = Find.TickManager.TicksGame + 300;
        }

        private static Building_TurretGun SpawnTurret(Map map, string def, IntVec3 cell)
        {
            var t = (Building_TurretGun)ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed(def));
            t.SetFaction(Faction.OfPlayer);
            GenSpawn.Spawn(t, cell, map);
            t.GetComp<CompRefuelable>().Refuel(12f);
            t.GetComp<CompPowerTrader>().PowerOn = true; // sin red eléctrica en la vitrina: se simula la corriente
            return t;
        }

        private void SendWave(Map map)
        {
            var faction = Find.FactionManager.RandomEnemyFaction(allowNonHumanlike: false) ?? Faction.OfAncientsHostile;
            foreach (var t in turrets)
            {
                var fuel = t.GetComp<CompRefuelable>();
                fuel.Refuel(fuel.Props.fuelCapacity - fuel.Fuel);
            }
            for (int i = 0; i < 5; i++)
            {
                var p = PawnGenerator.GeneratePawn(PawnKindDefOf.Villager, faction);
                var cell = CellFinder.StandableCellNear(center + new IntVec3(28, 0, -8 + i * 4), map, 3f);
                GenSpawn.Spawn(p, cell, map);
                wave.Add(p);
            }
            LordMaker.MakeNewLord(faction, new LordJob_AssaultColony(faction, canKidnap: false, canTimeoutOrFlee: false), map, wave);
        }
    }
}
