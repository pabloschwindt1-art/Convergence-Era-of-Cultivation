using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using LudeonTK;

namespace Convergence.QiTech
{
    /// <summary>
    /// Diagnóstico de puntería (con <c>-cvgqitechaimshot</c> y <c>-quicktest</c>, ventana visible): las dos torretas de Qi-Tech y, de
    /// referencia, la ballesta rúnica de Formations, cada una con un enemigo quieto exactamente al ESTE y después al NORTE. Registra el
    /// ángulo del cabezal y el estado del arma, y guarda capturas en &lt;datos&gt;/cvg_qitechaim/. Sin el argumento no hace nada.
    /// </summary>
    public class GameComponent_QiTechAimShot : GameComponent
    {
        private static readonly bool Enabled = GenCommandLine.CommandLineArgPassed("cvgqitechaimshot");
        private int state, wait;
        private bool shotPending;
        private int focusIndex;
        private string dir;
        private IntVec3 center;
        private readonly List<Building_TurretGun> turrets = new List<Building_TurretGun>();
        private readonly List<Pawn> enemies = new List<Pawn>();
        private static readonly string[] Defs = { "CVG_QT_InductionTurret", "CVG_RuneCrossbowTurret" };

        public GameComponent_QiTechAimShot(Game game) { }

        public override void GameComponentTick()
        {
            if (!Enabled || state < 0) return;
            var map = Find.CurrentMap;
            if (map == null || Find.TickManager.TicksGame < 120) return;
            if (state == 0) { SetUp(map); PlaceEnemies(map, new IntVec3(8, 0, 0)); state = 1; wait = 400; return; }
            if (--wait > 0) return;
            switch (state)
            {
                case 1: Report("EAST"); state = 2; focusIndex = 0; break;
                case 2: if (Capture("east")) { PlaceEnemies(map, new IntVec3(0, 0, 8)); state = 3; wait = 400; } break;
                case 3: Report("NORTH"); state = 4; focusIndex = 0; break;
                case 4:
                    if (Capture("north"))
                    {
                        Log.Message("[Convergence] Qi-Tech aim shots saved in " + dir + "\n  ALL OK");
                        state = -1;
                    }
                    break;
            }
        }

        private void SetUp(Map map)
        {
            dir = Path.Combine(GenFilePaths.SaveDataFolderPath, "cvg_qitechaim");
            Directory.CreateDirectory(dir);
            center = map.Center;
            foreach (var cell in GenRadial.RadialCellsAround(center, 22f, true).Where(x => x.InBounds(map)))
            {
                foreach (var t in cell.GetThingList(map).ToList()) if (t.def.destroyable) t.Destroy();
                if (cell.GetTerrain(map).passability != Traversability.Standable) map.terrainGrid.SetTerrain(cell, TerrainDefOf.Soil);
                map.roofGrid.SetRoof(cell, null);
            }
            for (int i = 0; i < Defs.Length; i++)
            {
                var def = DefDatabase<ThingDef>.GetNamedSilentFail(Defs[i]);
                if (def == null) continue;
                var t = (Building_TurretGun)ThingMaker.MakeThing(def);
                t.SetFaction(Faction.OfPlayer);
                GenSpawn.Spawn(t, center + new IntVec3(-12, 0, 12 - i * 12), map);
                t.TryGetComp<CompRefuelable>()?.Refuel(12f);
                var power = t.TryGetComp<CompPowerTrader>();
                if (power != null) power.PowerOn = true;
                turrets.Add(t);
            }
            Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;
        }

        private void PlaceEnemies(Map map, IntVec3 offset)
        {
            foreach (var e in enemies) if (!e.Destroyed) e.Destroy();
            enemies.Clear();
            var fac = Find.FactionManager.RandomEnemyFaction(allowNonHumanlike: false) ?? Faction.OfAncientsHostile;
            foreach (var t in turrets)
            {
                var p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Villager, fac, forceGenerateNewPawn: true));
                p.equipment?.DestroyAllEquipment();
                p.health.forceDowned = false;
                GenSpawn.Spawn(p, t.Position + offset, map);
                var job = JobMaker.MakeJob(JobDefOf.Wait, p.Position);
                job.expiryInterval = 5000; job.checkOverrideOnExpire = false;
                p.jobs.StartJob(job, JobCondition.InterruptForced);
                enemies.Add(p);
            }
        }

        private void Report(string side)
        {
            foreach (var t in turrets)
            {
                var top = Traverse.Create(t).Field("top").GetValue<TurretTop>();
                float rot = top != null ? top.CurRotation : -1f;
                var target = t.CurrentTarget;
                float wanted = target.IsValid ? (target.Cell.ToVector3Shifted() - t.DrawPos).AngleFlat() : -1f;
                Log.Message($"[Convergence] aim {side} {t.def.defName}: target={(target.IsValid ? target.Cell.ToString() : "none")} wantedAngle={wanted:0} topRotation={rot:0}"
                    + $" active={t.Active} available={t.AttackVerb.Available()} warmup={Traverse.Create(t).Field("burstWarmupTicksLeft").GetValue<int>()}"
                    + $" cooldown={Traverse.Create(t).Field("burstCooldownTicksLeft").GetValue<int>()} fuel={t.TryGetComp<CompRefuelable>()?.Fuel:0.##}"
                    + $" verbState={t.AttackVerb.state} minRange={t.AttackVerb.verbProps.minRange} range={t.AttackVerb.verbProps.range}");
            }
        }

        /// <summary>Una captura por torreta (enfoca, espera unos ticks, captura). Devuelve true cuando terminó con todas.</summary>
        private bool Capture(string side)
        {
            if (focusIndex >= turrets.Count) return true;
            foreach (var w in Find.WindowStack.Windows.OfType<EditWindow_Log>().ToList()) w.Close(false);
            var focus = turrets[focusIndex];
            Find.CameraDriver.SetRootPosAndSize(focus.DrawPos + new Vector3(3f, 0f, 2f), 7f);
            if (shotPending)
            {
                ScreenCapture.CaptureScreenshot(Path.Combine(dir, $"{side}_{focus.def.defName}.png"));
                shotPending = false; focusIndex++; wait = 20;
            }
            else { shotPending = true; wait = 10; }
            return false;
        }
    }
}
