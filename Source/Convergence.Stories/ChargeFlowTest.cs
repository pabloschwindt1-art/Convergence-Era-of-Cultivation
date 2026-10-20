using System.Linq;
using RimWorld;
using Verse;

namespace Convergence.Stories
{
    /// <summary>
    /// Prueba del flujo real de recarga (con <c>-cvgchargetest</c> y <c>-quicktest</c>): un titiritero con una marioneta de madera
    /// casi descargada, una formación de respiración del cielo y un cargador espiritual al lado. El juego corre ~1 día y la
    /// marioneta tiene que ir sola al cargador y recargarse. Sin el argumento no hace nada.
    /// </summary>
    public class GameComponent_ChargeFlowTest : GameComponent
    {
        private static readonly bool Enabled = GenCommandLine.CommandLineArgPassed("cvgchargetest");
        private int state, startTick;
        private Pawn puppet;
        private float startEnergy;
        private bool seenCharging;

        public GameComponent_ChargeFlowTest(Game game) { }

        public override void GameComponentTick()
        {
            if (!Enabled || state == 2 || Find.CurrentMap == null) return;
            var map = Find.CurrentMap;
            int now = Find.TickManager.TicksGame;
            if (state == 0)
            {
                if (now < 300) return;
                var master = map.mapPawns.FreeColonistsSpawned.FirstOrDefault();
                var link = DefDatabase<HediffDef>.GetNamedSilentFail("CVG_PuppetHeartThread");
                var kind = DefDatabase<PawnKindDef>.GetNamedSilentFail("CVG_Puppet_Wood");
                if (master == null || link == null || kind == null) { Log.Error("[Convergence] Charge flow test: missing pieces"); state = 2; return; }
                foreach (var d in new[] { "CVG_Research_QiBreathing", "CVG_Research_QiNetwork", "CVG_Research_Puppetry" })
                {
                    var p = DefDatabase<ResearchProjectDef>.GetNamedSilentFail(d);
                    if (p != null && !p.IsFinished) Find.ResearchManager.FinishProject(p, doCompletionDialog: false);
                }
                puppet = ScenPart_StartingPuppets.Equip(master, link, kind, 1).First();
                CellFinder.TryFindRandomCellNear(puppet.Position, map, 15, c => GenAdj.OccupiedRect(c, Rot4.North, new IntVec2(6, 6)).Cells.All(x => x.InBounds(map) && x.Standable(map) && !x.Roofed(map) && x.GetFirstBuilding(map) == null), out var c0);
                var arrayDef = ThingDef.Named("CVG_SkyBreathArray");
                var chargerDef = ThingDef.Named("CVG_SpiritCharger");
                GenSpawn.Spawn(ThingMaker.MakeThing(arrayDef, GenStuff.DefaultStuffFor(arrayDef)), c0, map).SetFaction(Faction.OfPlayer);
                GenSpawn.Spawn(ThingMaker.MakeThing(chargerDef, GenStuff.DefaultStuffFor(chargerDef)), c0 + new IntVec3(1, 0, 3), map).SetFaction(Faction.OfPlayer);
                puppet.needs.energy.CurLevelPercentage = 0.03f;   // vanilla manda a cargar por debajo del umbral del grupo (5 % por defecto)
                startEnergy = puppet.needs.energy.CurLevelPercentage;
                startTick = now;
                Find.TickManager.CurTimeSpeed = TimeSpeed.Ultrafast;
                state = 1;
                return;
            }
            if (puppet.CurJobDef == JobDefOf.MechCharge) seenCharging = true;
            if ((now - startTick) % 2500 == 0 && now - startTick <= 7500)
            {
                var charger = map.listerBuildings.allBuildingsColonist.OfType<Building_MechCharger>().FirstOrDefault();
                var closest = JobGiver_GetEnergy_Charger.GetClosestCharger(puppet, puppet, false);
                Log.Message($"[Convergence] charge diag t+{now - startTick}: job {puppet.CurJobDef?.defName}, energy {puppet.needs.energy.CurLevel:0.##}/{puppet.needs.energy.MaxLevel:0.##}, threshold {JobGiver_GetEnergy.GetMinAutorechargeThreshold(puppet)}, charger {(charger != null)}, powered {charger?.IsPowered}, canCharge {charger?.CanPawnChargeCurrently(puppet)}, closest {(closest != null)}, mode {puppet.GetMechWorkMode()}, drafted {puppet.Drafted}, dormant {puppet.TryGetComp<CompCanBeDormant>()?.Awake}");
            }
            if (now - startTick < GenDate.TicksPerDay) return;
            float end = puppet.needs.energy.CurLevelPercentage;
            bool ok = seenCharging && end > startEnergy + 0.2f;
            string msg = $"[Convergence] Charge flow test\n    {(seenCharging ? "ok  " : "FAIL")} the puppet went to the spirit charger by itself\n    {(end > startEnergy + 0.2f ? "ok  " : "FAIL")} energy {startEnergy:P0} -> {end:P0} in a day";
            if (ok) Log.Message(msg + "\n  ALL OK"); else Log.Error(msg + "\n  FAILURES: 1");
            state = 2;
        }
    }
}
