using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Convergence.Formations
{
    /// <summary>
    /// Prueba de formaciones y torretas como en la partida: con <c>-cvgformflow</c> (y <c>-quicktest</c>) construye un núcleo de la
    /// Formación de Espadas con 4 banderas alrededor y piedras espirituales, comprueba que el área se forme y se active, mete un
    /// enemigo adentro y espera a que la formación lo hiera; después lo mismo con la torreta de ballesta rúnica. Cierra el juego.
    /// Sin el argumento no hace nada.
    /// </summary>
    public class GameComponent_FormationFlowTest : GameComponent
    {
        private static readonly bool Enabled = GenCommandLine.CommandLineArgPassed("cvgformflow");
        private readonly StringBuilder sb = new StringBuilder("[Convergence] Formation flow test");
        private int fails, stage, stageStart;
        private float hpBefore, fuelBefore;
        private Pawn enemy;
        private Thing core, turret;
        private IntVec3 center;

        public GameComponent_FormationFlowTest(Game game) { }

        private void Check(bool ok, string what)
        {
            sb.Append("\n    " + (ok ? "ok   " : "FAIL ") + what);
            if (!ok) fails++;
        }

        private void Finish()
        {
            sb.Append(fails == 0 ? "\n  ALL OK" : "\n  FAILURES: " + fails);
            if (fails == 0) Log.Message(sb.ToString()); else Log.Error(sb.ToString());
            stage = 99;
            Root.Shutdown();
        }

        private Thing Build(string defName, IntVec3 c, Map map)
        {
            var def = DefDatabase<ThingDef>.GetNamed(defName);
            var t = ThingMaker.MakeThing(def, def.MadeFromStuff ? GenStuff.DefaultStuffFor(def) : null);
            t.SetFaction(Faction.OfPlayer);
            GenSpawn.Spawn(t, c, map, WipeMode.Vanish);
            var fuel = t.TryGetComp<CompRefuelable>();
            fuel?.Refuel(fuel.Props.fuelCapacity);
            return t;
        }

        private Pawn SpawnEnemy(Map map, IntVec3 c, bool disarm = true)
        {
            var faction = Find.FactionManager.AllFactions.FirstOrDefault(f => f.HostileTo(Faction.OfPlayer) && f.def.humanlikeFaction && !f.def.hidden);
            var kind = faction?.RandomPawnKind() ?? PawnKindDefOf.Villager;
            var p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, faction, mustBeCapableOfViolence: true));
            if (disarm) p.equipment?.DestroyAllEquipment(); // en la formación: que no se defienda; la torreta solo apunta a amenazas (armados)
            GenSpawn.Spawn(p, c, map);
            return p;
        }

        public override void GameComponentTick()
        {
            if (!Enabled || stage == 99 || Find.CurrentMap == null) return;
            var map = Find.CurrentMap;
            int now = Find.TickManager.TicksGame;
            if (stage == 0)
            {
                if (now < 300) return;
                Find.TickManager.CurTimeSpeed = TimeSpeed.Ultrafast;
                if (!GameComponent_Convergence.Active) GameComponent_Convergence.Instance?.TryFire(map);
                // zona despejada lejos de los colonos
                CellFinderLoose.TryGetRandomCellWith(c => c.Standable(map) && c.DistanceToEdge(map) > 15 && !c.Roofed(map)
                    && GenRadial.RadialCellsAround(c, 6f, true).All(x => x.InBounds(map) && x.Standable(map) && x.GetFirstBuilding(map) == null && !x.Roofed(map))
                    && map.mapPawns.FreeColonistsSpawned.All(p => p.Position.DistanceTo(c) > 25f), map, 2000, out center);
                core = Build("CVG_SwordFormationCore", center, map);
                foreach (var off in new[] { new IntVec3(5, 0, 5), new IntVec3(-5, 0, 5), new IntVec3(-5, 0, -5), new IntVec3(5, 0, -5) })
                    Build("CVG_FormationFlag", center + off, map);
                var comp = core.TryGetComp<CompFormationCore>();
                comp.Recalculate();
                Check(comp.FlagCount == 4 && comp.CellList.Count > 30, $"4 flags enclose an area of {comp.CellList.Count} cells");
                Check(comp.Active, "the sword formation is active (fueled, inside limits)");
                enemy = SpawnEnemy(map, center + new IntVec3(2, 0, 2));
                hpBefore = enemy.health.summaryHealth.SummaryHealthPercent;
                stageStart = now; stage = 1;
                return;
            }
            if (stage == 1)
            {
                bool hurt = enemy.Dead || enemy.Downed || enemy.health.summaryHealth.SummaryHealthPercent < hpBefore - 0.01f;
                if (!hurt && now - stageStart < 2500) return;
                Check(hurt, $"the formation hurts an enemy inside its area (health {hpBefore:P0} -> {(enemy.Dead ? 0f : enemy.health.summaryHealth.SummaryHealthPercent):P0})");
                if (!enemy.Destroyed) enemy.Destroy();
                core.Destroy();
                // torreta
                turret = Build("CVG_RuneCrossbowTurret", center, map);
                Check(turret.TryGetComp<CompRefuelable>()?.HasFuel ?? false, "the rune crossbow turret is loaded with spirit stones");
                enemy = SpawnEnemy(map, center + new IntVec3(8, 0, 0), disarm: false);
                fuelBefore = turret.TryGetComp<CompRefuelable>().Fuel;
                hpBefore = enemy.health.summaryHealth.SummaryHealthPercent;
                stageStart = now; stage = 2;
                return;
            }
            if (stage == 2)
            {
                bool hurt = enemy.Dead || enemy.Downed || enemy.health.summaryHealth.SummaryHealthPercent < hpBefore - 0.01f;
                if (!hurt && now - stageStart < 2500) return;
                float fuelNow = turret.TryGetComp<CompRefuelable>().Fuel;
                Check(fuelNow < fuelBefore, $"the turret fired (spirit stones {fuelBefore:0.0} -> {fuelNow:0.0})");
                Check(hurt, $"the turret shoots and hurts an enemy in range (health {hpBefore:P0} -> {(enemy.Dead ? 0f : enemy.health.summaryHealth.SummaryHealthPercent):P0})");
                if (!enemy.Destroyed) enemy.Destroy();
                turret.Destroy();
                // Contra un cultivador del Núcleo Dorado (reino 3): la balista del trueno (grado 3) le pega como a un mortal.
                turret = Build("CVG_ThunderBallistaTurret", center, map);
                enemy = SpawnEnemy(map, center + new IntVec3(14, 0, 0), disarm: false);
                var ec = CompCultivator.For(enemy);
                var golden = DefDatabase<CultivationRealmDef>.AllDefsListForReading.FirstOrDefault(r => r.path == CVG_DefOf.CVG_Path_Qi && r.order == 3);
                if (ec != null && golden != null)
                {
                    if (!ec.HasRoot) CultivationGenerator.GiveRoot(enemy, CultivationGenerator.RandomGrade(), CultivationGenerator.RandomElements());
                    ec.Sync();
                    var tr = ec.TrackerFor(CVG_DefOf.CVG_Path_Qi, create: true);
                    tr.EnsureStarted();
                    tr.AdvanceTo(golden, 1f);
                }
                int realm = SpiritGradeUtility.RealmLevel(enemy);
                int grade = SpiritGradeUtility.GradeOf(turret is Building_TurretGun tg ? tg.gun : null);
                Check(realm == 3 && grade == 3 && SpiritGradeUtility.SuppressionFactor(realm, grade) == 1f,
                    $"thunder ballista grade {grade} vs a realm {realm} cultivator: full damage");
                Check(golden != null && (turret.def.description.Contains(golden.label) || turret.def.description.Contains("CVG_SpiritGrade_DescAll".Translate(3).ToString())), "its description says up to which realm it is strong: ..." + turret.def.description.Substring(System.Math.Max(0, turret.def.description.Length - 160)));
                hpBefore = enemy.health.summaryHealth.SummaryHealthPercent;
                fuelBefore = turret.TryGetComp<CompRefuelable>().Fuel;
                stageStart = now; stage = 3;
                return;
            }
            if (stage == 3)
            {
                bool hurt = enemy.Dead || enemy.Downed || enemy.health.summaryHealth.SummaryHealthPercent < hpBefore - 0.05f;
                if (!hurt && now - stageStart < 3500) return;
                Check(turret.TryGetComp<CompRefuelable>().Fuel < fuelBefore, "the thunder ballista fired at the cultivator");
                Check(hurt, $"the thunder ballista really hurts a Golden Core cultivator (health {hpBefore:P0} -> {(enemy.Dead ? 0f : enemy.health.summaryHealth.SummaryHealthPercent):P0})");
                Finish();
            }
        }
    }
}
