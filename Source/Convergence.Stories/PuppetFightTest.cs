using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Convergence.Stories
{
    /// <summary>
    /// Prueba de balance de las marionetas de guerra (con <c>-cvgpuppetfighttest</c> y <c>-quicktest</c>): combates reales, varias
    /// rondas por caso, contando quién gana. Dos referencias (2 y 1 marioneta contra un cultivador de su nivel: la regla acordada es
    /// que dos ganan y una no) e incursiones de grupo como en una partida (titiriteros que también pelean, con sus marionetas, contra
    /// varios cultivadores con armas mezcladas). Los casos de un reino que aún no existe se saltan. Sin el argumento no hace nada.
    /// </summary>
    public class GameComponent_PuppetFightTest : GameComponent
    {
        private static readonly bool Enabled = GenCommandLine.CommandLineArgPassed("cvgpuppetfighttest");
        private const int Rounds = 3, MaxTicks = 3 * 2500;

        private class Case
        {
            public string label, kind, realm, enemyRealm;
            public int stage, enemyStage, masters = 1, perMaster, enemies = 1;
            public bool mastersFight, equipped;
            public int wins, losses, draws;
            public bool skipped;
        }

        private readonly List<Case> cases = new List<Case>
        {
            new Case { label = "2 jade vs 1", kind = "CVG_Puppet_Jade", realm = "CVG_Realm_GoldenCore", stage = 4, perMaster = 2 },
            new Case { label = "1 jade vs 1", kind = "CVG_Puppet_Jade", realm = "CVG_Realm_GoldenCore", stage = 4, perMaster = 1 },
            new Case { label = "raid: 2 masters + 6 jade vs 3", kind = "CVG_Puppet_Jade", realm = "CVG_Realm_GoldenCore", stage = 4, masters = 2, perMaster = 3, mastersFight = true, enemies = 3 },
            new Case { label = "raid: 2 masters + 6 jade vs 4", kind = "CVG_Puppet_Jade", realm = "CVG_Realm_GoldenCore", stage = 4, masters = 2, perMaster = 3, mastersFight = true, enemies = 4 },
            new Case { label = "raid: 2 masters + 6 bronze vs 3", kind = "CVG_Puppet_Bronze", realm = "CVG_Realm_Foundation", stage = 2, masters = 2, perMaster = 3, mastersFight = true, enemies = 3 },
            new Case { label = "raid: 2 masters + 6 jade vs 3 equipped", kind = "CVG_Puppet_Jade", realm = "CVG_Realm_GoldenCore", stage = 4, masters = 2, perMaster = 3, mastersFight = true, enemies = 3, equipped = true },
            // el reino que sigue a Núcleo Dorado: se salta mientras no exista
            new Case { label = "3 masters + 6 jade vs 1 Nascent Soul", kind = "CVG_Puppet_Jade", realm = "CVG_Realm_GoldenCore", stage = 4, masters = 3, perMaster = 2, mastersFight = true, enemyRealm = "CVG_Realm_NascentSoul", enemyStage = 1 },
        };

        private int caseIdx, round, startTick = -1, state;
        private bool stuckChecked;
        private float startInjury;
        private List<Pawn> masters = new List<Pawn>(), puppets = new List<Pawn>(), enemies = new List<Pawn>();
        private IntVec3 arena;

        public GameComponent_PuppetFightTest(Game game) { }

        /// <summary>Si algo pausa el juego (una carta, un diálogo), la prueba no avanzaría nunca: se quita la pausa.</summary>
        public override void GameComponentUpdate()
        {
            if (!Enabled || state == 2 || Current.ProgramState != ProgramState.Playing) return;
            if (Find.TickManager.Paused || Find.TickManager.CurTimeSpeed != TimeSpeed.Ultrafast) Find.TickManager.CurTimeSpeed = TimeSpeed.Ultrafast;
            foreach (var w in Find.WindowStack.Windows.Where(w => w.forcePause).ToList()) w.Close(false);
        }

        private Case C => cases[caseIdx];
        private IEnumerable<Pawn> Team => C.mastersFight ? puppets.Concat(masters) : puppets;
        private static bool Up(Pawn p) => p != null && p.Spawned && !p.Dead && !p.Downed;

        private float InjurySum() => enemies.Concat(puppets).Concat(masters).Where(p => p != null && !p.Dead)
            .Sum(p => p.health.hediffSet.hediffs.OfType<Hediff_Injury>().Sum(h => h.Severity));

        public override void GameComponentTick()
        {
            if (!Enabled || state == 2 || Find.CurrentMap == null) return;
            var map = Find.CurrentMap;
            int now = Find.TickManager.TicksGame;
            if (state == 0)
            {
                if (now < 300) return;
                // sin Convergencia el cultivo está latente (sin vida ni defensa de reino): el cultivador enemigo sería un mortal
                var gc = GameComponent_Convergence.Instance;
                if (gc != null && !gc.Occurred) gc.MarkOccurred();
                foreach (var d in new[] { "CVG_Research_QiBreathing", "CVG_Research_QiNetwork", "CVG_Research_Puppetry" })
                {
                    var pr = DefDatabase<ResearchProjectDef>.GetNamedSilentFail(d);
                    if (pr != null && !pr.IsFinished) Find.ResearchManager.FinishProject(pr, doCompletionDialog: false);
                }
                PickArena(map);
                Find.TickManager.CurTimeSpeed = TimeSpeed.Ultrafast;
                state = 1;
                NextValidCase(map);
                return;
            }
            if (now % 30 != 0) return;
            bool enemyDown = enemies.All(e => !Up(e));
            bool teamDown = Team.All(p => !Up(p));
            // ronda trabada (nadie llegó al otro en una hora: terreno, agua, paredes): no cuenta, se repite en otro lugar
            if (!enemyDown && !teamDown && now - startTick > 2500 && !stuckChecked)
            {
                stuckChecked = true;
                if (Math.Abs(InjurySum() - startInjury) < 0.01f)
                {
                    Log.Message($"[Convergence] fight stuck ({C.label}): retrying elsewhere");
                    Cleanup();
                    PickArena(map);
                    StartRound(map);
                    return;
                }
            }
            if (enemyDown || teamDown || now - startTick > MaxTicks)
            {
                var c = C;
                if (enemyDown && !teamDown) c.wins++; else if (teamDown && !enemyDown) c.losses++; else c.draws++;
                Log.Message($"[Convergence] fight round: {c.label} -> {(enemyDown && !teamDown ? "puppets win" : teamDown && !enemyDown ? "cultivators win" : "draw")} in {(now - startTick) / 2500f:0.0} h; standing: team {Team.Count(Up)}, enemies {enemies.Count(Up)}");
                Cleanup();
                round++;
                if (round >= Rounds) { round = 0; caseIdx++; }
                NextValidCase(map);
                return;
            }
            // sin un señor (lord) que los mande, los pawns solo responden de cerca: cada uno ataca al rival vivo más cercano
            var team = Team.Where(Up).ToList();
            var foes = enemies.Where(Up).ToList();
            foreach (var p in team) Engage(p, foes);
            foreach (var e in foes) Engage(e, team);
        }

        private static void Engage(Pawn p, List<Pawn> targets)
        {
            if (p.CurJob != null && p.CurJob.targetA.Thing is Pawn cur && Up(cur) && targets.Contains(cur)) return;
            var near = targets.OrderBy(t => t.Position.DistanceToSquared(p.Position)).FirstOrDefault();
            if (near == null) return;
            var job = p.equipment?.Primary != null && p.equipment.Primary.def.IsRangedWeapon
                ? JobMaker.MakeJob(JobDefOf.AttackStatic, near) : JobMaker.MakeJob(JobDefOf.AttackMelee, near);
            job.expiryInterval = 600; job.checkOverrideOnExpire = true;
            p.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        }

        private void PickArena(Map map) =>
            CellFinder.TryFindRandomCellNear(map.Center, map, 60, x => GenRadial.RadialCellsAround(x, 10f, true)
                .All(y => y.InBounds(map) && y.Standable(map) && y.GetFirstBuilding(map) == null && !y.GetTerrain(map).IsWater), out arena);

        private void NextValidCase(Map map)
        {
            while (caseIdx < cases.Count)
            {
                var c = C;
                bool ok = DefDatabase<CultivationRealmDef>.GetNamedSilentFail(c.realm) != null
                    && DefDatabase<CultivationRealmDef>.GetNamedSilentFail(c.enemyRealm ?? c.realm) != null
                    && map.mapPawns.FreeColonistsSpawned.Count >= c.masters;
                if (ok) break;
                c.skipped = true;
                Log.Message($"[Convergence] fight case skipped: {c.label} (realm not in the game yet, or not enough colonists)");
                caseIdx++;
            }
            if (caseIdx >= cases.Count) { Report(); state = 2; return; }
            StartRound(map);
        }

        private void StartRound(Map map)
        {
            var c = C;
            var realmDef = DefDatabase<CultivationRealmDef>.GetNamed(c.realm);
            var enemyRealm = DefDatabase<CultivationRealmDef>.GetNamed(c.enemyRealm ?? c.realm);
            var link = DefDatabase<HediffDef>.GetNamed("CVG_PuppetHeartThread");
            var kind = DefDatabase<PawnKindDef>.GetNamed(c.kind);
            var mechlink = DefDatabase<HediffDef>.GetNamedSilentFail("MechlinkImplant");

            masters = map.mapPawns.FreeColonistsSpawned.Take(c.masters).ToList();
            puppets = new List<Pawn>();
            int i = 0;
            foreach (var m in masters)
            {
                m.health.Reset();
                SetRealm(m, realmDef, c.stage);
                if (c.mastersFight) TechniqueUtility.GiveNpcTechniques(m);
                // con mecaenlace + hilo del corazón: los hilos alcanzan para 3 de jade (como el titiritero completo)
                if (c.perMaster > 2 && mechlink != null && !m.health.hediffSet.HasHediff(mechlink)) m.health.AddHediff(mechlink, m.health.hediffSet.GetBrain());
                if (m.equipment?.Primary == null)
                    m.equipment?.AddEquipment((ThingWithComps)ThingMaker.MakeThing(ThingDef.Named("MeleeWeapon_LongSword"), ThingDefOf.Steel));
                m.drafter.Drafted = false;
                m.DeSpawn();
                GenSpawn.Spawn(m, CellFinder.RandomClosewalkCellNear(arena + new IntVec3(-7, 0, -2 + 3 * i++), map, 1), map);
                foreach (var p in ScenPart_StartingPuppets.Equip(m, link, kind, c.perMaster))
                {
                    p.DeSpawn();
                    GenSpawn.Spawn(p, CellFinder.RandomClosewalkCellNear(arena + new IntVec3(-5, 0, 0), map, 3), map);
                    p.GetComp<CompPuppet>()?.Refresh();
                    if (p.needs?.energy != null) p.needs.energy.CurLevelPercentage = 1f;
                    puppets.Add(p);
                }
            }
            if (!c.mastersFight)
                foreach (var m in masters) { m.DeSpawn(); GenSpawn.Spawn(m, CellFinder.RandomClosewalkCellNear(arena + new IntVec3(-20, 0, 0), map, 3), map); }

            var hostile = Find.FactionManager.AllFactions.Where(f => !f.IsPlayer && f.HostileTo(Faction.OfPlayer) && f.def.humanlikeFaction && !f.Hidden).RandomElementWithFallback();
            enemies = new List<Pawn>();
            for (int k = 0; k < c.enemies; k++)
            {
                var e = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Colonist, hostile, forceGenerateNewPawn: true, mustBeCapableOfViolence: true, tile: map.Tile));
                var comp = CompCultivator.For(e);
                if (comp != null && !comp.HasRoot) { CultivationGenerator.GiveRoot(e, CultivationGenerator.RandomGrade(), CultivationGenerator.RandomElements()); comp.Sync(); }
                SetRealm(e, enemyRealm, c.enemyRealm != null ? c.enemyStage : c.stage);
                TechniqueUtility.GiveNpcTechniques(e);
                e.equipment?.DestroyAllEquipment();
                // armas mezcladas como en una incursión: uno de cada dos a distancia
                bool ranged = c.enemies > 1 && k % 2 == 1;
                var weaponDef = ThingDef.Named(ranged ? "Gun_AssaultRifle" : "MeleeWeapon_LongSword");
                var weapon = ThingMaker.MakeThing(weaponDef, weaponDef.MadeFromStuff ? (c.equipped ? ThingDefOf.Plasteel : ThingDefOf.Steel) : null);
                if (c.equipped) weapon.TryGetComp<CompQuality>()?.SetQuality(QualityCategory.Excellent, ArtGenerationContext.Outsider);
                e.equipment?.AddEquipment((ThingWithComps)weapon);
                if (c.equipped)
                    foreach (var a in new[] { "Apparel_PowerArmor", "Apparel_PowerArmorHelmet" })
                    {
                        var ad = DefDatabase<ThingDef>.GetNamedSilentFail(a);
                        if (ad != null) e.apparel?.Wear((Apparel)ThingMaker.MakeThing(ad, GenStuff.DefaultStuffFor(ad)), false);
                    }
                GenSpawn.Spawn(e, CellFinder.RandomClosewalkCellNear(arena + new IntVec3(7, 0, 0), map, 3), map);
                enemies.Add(e);
            }
            startTick = Find.TickManager.TicksGame;
            stuckChecked = false;
            startInjury = InjurySum();
            if (round == 0)
            {
                var e0 = enemies.First();
                var pp = puppets.FirstOrDefault();
                Log.Message($"[Convergence] fight setup {c.label}: {masters.Count} master(s) bandwidth {masters.First().GetStatValue(StatDefOf.MechBandwidth):0}, {puppets.Count} puppet(s) (health x{pp?.HealthScale:0.##}, melee x{pp?.GetStatValue(StatDefOf.MeleeDamageFactor):0.##}, incoming x{pp?.GetStatValue(StatDefOf.IncomingDamageFactor):0.##}, controlled {puppets.Count(p => p.GetOverseer() != null)}); {enemies.Count} enemy(ies) {CompCultivator.For(e0)?.Realm?.Realm?.defName} (health x{e0.HealthScale:0.##}, melee x{e0.GetStatValue(StatDefOf.MeleeDamageFactor):0.##}, incoming x{e0.GetStatValue(StatDefOf.IncomingDamageFactor):0.##})");
            }
        }

        private static void SetRealm(Pawn p, CultivationRealmDef realm, int stage)
        {
            var comp = CompCultivator.For(p);
            if (comp == null) return;
            comp.Sync();
            if (!comp.HasRoot) { CultivationGenerator.GiveRoot(p, CultivationGenerator.RandomGrade(), CultivationGenerator.RandomElements()); comp.Sync(); }
            var t = comp.Realm;
            if (!t.Started) t.EnsureStarted();
            if (t.Realm.order < realm.order) t.AdvanceTo(realm, 1f);
            t.SetStage(stage);
            comp.InvalidateHealthScale();
        }

        private void Cleanup()
        {
            foreach (var p in puppets) { if (p.Spawned) p.DeSpawn(); if (!p.Destroyed) p.Destroy(); }
            foreach (var e in enemies) { if (e.Spawned) e.DeSpawn(); if (!e.Destroyed) e.Destroy(); }
            // los titiriteros se reusan: si murieron, se los resucita antes de borrar los cadáveres
            foreach (var m in masters.Where(m => m.Dead)) ResurrectionUtility.TryResurrect(m);
            foreach (var corpse in Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.Corpse).ToList()) corpse.Destroy();
            foreach (var m in masters) m.health.hediffSet.hediffs.RemoveAll(h => h.def.defName == "CVG_PuppetBacklash");
        }

        private void Report()
        {
            var sb = new StringBuilder("[Convergence] Puppet fight test (puppets and their masters vs hostile cultivators of the same realm)");
            foreach (var c in cases)
                sb.Append(c.skipped ? $"\n    {c.label}: skipped (realm not in the game yet)"
                    : $"\n    {c.label} ({c.realm.Replace("CVG_Realm_", "")} {c.stage}): puppets win {c.wins}/{Rounds}, lose {c.losses}, draw {c.draws}");
            sb.Append("\n  ALL OK");
            Log.Message(sb.ToString());
        }
    }
}
