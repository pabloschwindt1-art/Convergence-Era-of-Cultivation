using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Convergence.Formations
{
    /// <summary>
    /// Autotest de las formaciones. Solo corre con el argumento <c>-cvgformationstest</c> (y un mapa, p. ej. -quicktest): coloca banderas y
    /// cada núcleo, y comprueba área, qi, cúpula, espadas y atadura contra un enemigo. Inerte sin el argumento.
    /// </summary>
    public class GameComponent_FormationsTest : GameComponent
    {
        private static readonly bool Enabled = GenCommandLine.CommandLineArgPassed("cvgformationstest");
        private bool started;
        private int phase = -1;
        private int waitUntil;
        private int deadline;
        private readonly StringBuilder sb = new StringBuilder("[Convergence] Formations self-test");
        private int fails;
        private Map map;
        private IntVec3 center;
        private readonly List<Thing> spawned = new List<Thing>();
        private Pawn enemy;
        private Pawn friend;
        private Thing core;
        private CompFormationCore comp;
        private Projectile bullet;
        private float fuelBefore;
        private float qiBefore;
        private int friendInjuries;
        private int enemyInjuries;

        public GameComponent_FormationsTest(Game game) { }

        private void Check(bool ok, string what)
        {
            sb.Append("\n    " + (ok ? "ok   " : "FAIL ") + what);
            if (!ok) fails++;
        }

        private void Note(string what) => sb.Append("\n- " + what);

        public override void GameComponentTick()
        {
            if (!Enabled) return;
            try
            {
                if (!started)
                {
                    if (Find.CurrentMap == null || Find.TickManager.TicksGame < 200) return;
                    started = true;
                    map = Find.CurrentMap;
                    Find.TickManager.CurTimeSpeed = TimeSpeed.Ultrafast;
                    phase = 0;
                    waitUntil = 0;
                }
                if (phase < 0 || Find.TickManager.TicksGame < waitUntil) return;
                Step();
            }
            catch (Exception e)
            {
                Check(false, "exception: " + e);
                Finish();
            }
        }

        private static ThingDef D(string n) => DefDatabase<ThingDef>.GetNamed(n);

        private IntVec3 FreeNear(IntVec3 c)
        {
            if (CellFinder.TryFindRandomCellNear(c, map, 2, x => x.Standable(map) && x.GetFirstBuilding(map) == null && x.GetFirstPawn(map) == null, out var r)) return r;
            return c;
        }

        private void Cleanup()
        {
            foreach (var t in spawned.ToList()) if (t != null && !t.Destroyed) t.Destroy();
            spawned.Clear();
            core = null; comp = null;
        }

        private void Setup(string coreDef)
        {
            Cleanup();
            center = map.Center;
            TestArena.Clear(map, CellRect.CenteredOn(center, 24), removeStrangers: true);   // superficie limpia: sin roca ni agua donde van banderas y enemigos
            foreach (var dx in new[] { -6, 6 })
                foreach (var dz in new[] { -6, 6 })
                {
                    var flag = ThingMaker.MakeThing(D("CVG_FormationFlag"));
                    flag.SetFactionDirect(Faction.OfPlayer);
                    GenSpawn.Spawn(flag, FreeNear(center + new IntVec3(dx, 0, dz)), map);
                    spawned.Add(flag);
                }
            core = ThingMaker.MakeThing(D(coreDef));
            core.SetFactionDirect(Faction.OfPlayer);
            GenSpawn.Spawn(core, FreeNear(center), map);
            spawned.Add(core);
            comp = core.TryGetComp<CompFormationCore>();
            comp.Refuelable.Refuel(20f);
            comp.Recalculate();
        }

        private Pawn SpawnHostile(IntVec3 at)
        {
            var fac = Find.FactionManager.AllFactionsListForReading.First(f => !f.IsPlayer && f.HostileTo(Faction.OfPlayer) && f.def.humanlikeFaction && !f.Hidden);
            var kind = fac.def.pawnGroupMakers?.SelectMany(g => g.options ?? new List<PawnGenOption>()).Select(o => o.kind).FirstOrDefault(k => k != null && k.RaceProps.Humanlike)
                       ?? PawnKindDefOf.Villager;
            var p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, fac, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: true));
            p.equipment?.DestroyAllEquipment();
            GenSpawn.Spawn(p, FreeNear(at), map);
            spawned.Add(p);
            return p;
        }

        // El enemigo se queda quieto (trabajo de espera) para que siga dentro del área durante la prueba.
        private static void Freeze(Pawn p)
        {
            var job = JobMaker.MakeJob(JobDefOf.Wait, p.Position);
            job.expiryInterval = 5000;
            job.checkOverrideOnExpire = false;
            p.jobs.StartJob(job, JobCondition.InterruptForced);
        }

        private void Step()
        {
            int now = Find.TickManager.TicksGame;
            switch (phase)
            {
                case 0: // qi
                    Note("qi gathering formation");
                    qiBefore = QiConcentrationUtility.ConcentrationAt(map.Center, map);
                    Setup("CVG_QiGatheringCore");
                    Check(comp.CellList.Count > 50, "area has " + comp.CellList.Count + " cells, level " + comp.Level + ", flags " + comp.FlagCount);
                    Check(comp.Active, "core is active with fuel");
                    float qiNow = QiConcentrationUtility.ConcentrationAt(map.Center, map);
                    Check(qiNow > qiBefore + 0.5f, $"qi concentration {qiBefore:0.0} -> {qiNow:0.0}");
                    Check(!comp.Contains(map.Center + new IntVec3(40, 0, 40)), "far cell is outside");
                    phase = 1; waitUntil = now + 30; break;

                case 1: // cúpula: enemigo dentro dispara a un amigo
                    Note("grand protective formation");
                    Setup("CVG_ProtectiveFormationCore");
                    fuelBefore = comp.Refuelable.Fuel;
                    enemy = SpawnHostile(center + new IntVec3(2, 0, 0));
                    friend = SpawnHostile(center + new IntVec3(-3, 0, 0));
                    friend.SetFaction(Faction.OfPlayer);
                    friendInjuries = friend.health.hediffSet.hediffs.Count(h => h is Hediff_Injury);
                    var def = D("Bullet_Revolver");
                    bullet = (Projectile)GenSpawn.Spawn(def, enemy.Position, map);
                    bullet.Launch(enemy, friend, friend, ProjectileHitFlags.IntendedTarget);
                    phase = 2; waitUntil = now + 30; break;

                case 2:
                    Check(bullet.Destroyed, "enemy projectile was absorbed by the dome");
                    Check(friend.health.hediffSet.hediffs.Count(h => h is Hediff_Injury) == friendInjuries, "friend took no damage");
                    Check(comp.Refuelable.Fuel < fuelBefore, $"fuel dropped {fuelBefore:0.00} -> {comp.Refuelable.Fuel:0.00}");
                    // sin combustible la cúpula cae
                    comp.Refuelable.ConsumeFuel(comp.Refuelable.Fuel);
                    Check(!comp.Active, "dome is down without fuel");
                    phase = 3; waitUntil = now + 10; break;

                case 3: // espadas
                    Note("killing swords formation");
                    Setup("CVG_SwordFormationCore");
                    fuelBefore = comp.Refuelable.Fuel;
                    enemy = SpawnHostile(center + new IntVec3(1, 0, 1));
                    enemyInjuries = enemy.health.hediffSet.hediffs.Count(h => h is Hediff_Injury);
                    Freeze(enemy);
                    phase = 4; waitUntil = now + 400; break;

                case 4:
                    Check(enemy.Dead || enemy.Downed || enemy.health.hediffSet.hediffs.Count(h => h is Hediff_Injury) > enemyInjuries, "swords hurt the enemy inside the area");
                    Check(comp.Refuelable.Fuel < fuelBefore, "swords used spirit stone");
                    phase = 5; waitUntil = now + 10; break;

                case 5: // atadura
                    Note("binding formation");
                    Setup("CVG_BindingFormationCore");
                    enemy = SpawnHostile(center + new IntVec3(1, 0, -1));
                    Freeze(enemy);
                    phase = 6; waitUntil = now + 60; deadline = now + 1500; break;

                case 6:
                    // La atadura aturde unos segundos y vuelve a atar en su intervalo: se mira seguido hasta que se vea atado o venza el plazo
                    // (mirar una sola vez a los 700 ticks caía a veces entre dos ataduras).
                    if (!enemy.stances.stunner.Stunned && !enemy.Dead && now < deadline) { waitUntil = now + 20; break; }
                    Check(enemy.stances.stunner.Stunned || enemy.Dead, "strongest enemy was bound (stunned)");
                    phase = 7; waitUntil = now + 10; break;

                case 7: // torretas
                    Note("fixed defenses");
                    Cleanup();
                    foreach (var n in new[] { "CVG_RuneCrossbowTurret" })
                    {
                        var t = (Building_TurretGun)ThingMaker.MakeThing(D(n));
                        t.SetFactionDirect(Faction.OfPlayer);
                        GenSpawn.Spawn(t, FreeNear(map.Center), map);
                        spawned.Add(t);
                        Check(t.Spawned && t.CurrentEffectiveVerb != null, n + " spawned with a gun verb");
                    }
                    phase = 8; waitUntil = now + 200; break;

                case 8:
                    Finish();
                    break;
            }
        }

        private void Finish()
        {
            phase = -1;
            Cleanup();
            sb.Append(fails == 0 ? "\n  ALL OK" : "\n  FAILURES: " + fails);
            if (fails == 0) Log.Message(sb.ToString()); else Log.Error(sb.ToString());
        }
    }
}
