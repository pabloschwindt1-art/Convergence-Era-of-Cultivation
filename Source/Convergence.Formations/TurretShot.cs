using System.Collections.Generic;
using System.IO;
using System.Linq;
using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Convergence.Formations
{
    /// <summary>
    /// Captura de las torretas contra enemigos (con <c>-cvgturretshot</c> y <c>-quicktest</c>, ventana visible): pone la estatua guardiana, la
    /// ballesta rúnica y la ballesta del trueno cargadas, enemigos quietos a distintos lados y saca tres fotos mientras apuntan y disparan
    /// en <c>&lt;datos&gt;/cvg_turretshot/</c>. Sin el argumento no hace nada.
    /// </summary>
    public class GameComponent_TurretShot : GameComponent
    {
        private static readonly bool Enabled = GenCommandLine.CommandLineArgPassed("cvgturretshot");
        private int state, wait;
        private bool shotPending;
        private string dir;
        private readonly List<Thing> turrets = new List<Thing>();

        public GameComponent_TurretShot(Game game) { }

        private static void Clear(Map map, IntVec3 c, int radius)
        {
            foreach (var cell in GenRadial.RadialCellsAround(c, radius, true))
                if (cell.InBounds(map))
                    foreach (var t in cell.GetThingList(map).ToList())
                        if (t.def.destroyable && t.def.category != ThingCategory.Pawn) t.Destroy();
        }

        public override void GameComponentTick()
        {
            if (!Enabled || state >= 5 || Find.CurrentMap == null) return;
            var map = Find.CurrentMap;
            if (state == 0)
            {
                if (Find.TickManager.TicksGame < 200) return;
                // mediodía, para ver los colores reales (de madrugada el juego oscurece todo a la mitad)
                int add = ((12 - GenLocalDate.HourOfDay(map)) + 24) % 24 * 2500;
                if (add > 0) Find.TickManager.DebugSetTicksGame(Find.TickManager.TicksGame + add);
                dir = Path.Combine(GenFilePaths.SaveDataFolderPath, "cvg_turretshot");
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
                Directory.CreateDirectory(dir);
                var center = map.Center;
                Clear(map, center, 22);
                var spots = new[] { ("CVG_RuneCrossbowTurret", 0), ("CVG_ThunderBallistaTurret", 10) };
                foreach (var (defName, dx) in spots)
                {
                    var def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
                    if (def == null) continue;
                    var t = ThingMaker.MakeThing(def);
                    t.SetFaction(Faction.OfPlayer);
                    GenSpawn.Spawn(t, center + new IntVec3(dx, 0, 0), map);
                    t.TryGetComp<CompRefuelable>()?.Refuel(20f);
                    turrets.Add(t);
                }
                var fac = Find.FactionManager.AllFactionsListForReading.First(f => !f.IsPlayer && f.HostileTo(Faction.OfPlayer) && f.def.humanlikeFaction && !f.Hidden);
                var kind = fac.def.pawnGroupMakers?.SelectMany(g => g.options ?? new List<PawnGenOption>()).Select(o => o.kind).FirstOrDefault(k => k != null && k.RaceProps.Humanlike) ?? PawnKindDefOf.Villager;
                // enemigos a distintos lados de cada torreta (para ver el arma girar hacia todos lados)
                foreach (var (dx, dz) in new[] { (-4, 8), (-15, -3), (-8, -9), (4, 9), (3, -8), (12, 8), (16, -2), (8, -9) })
                {
                    var p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, fac, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: true));
                    p.equipment?.DestroyAllEquipment();
                    GenSpawn.Spawn(p, center + new IntVec3(dx, 0, dz), map);
                    var job = JobMaker.MakeJob(JobDefOf.Wait, p.Position);
                    job.expiryInterval = 5000; job.checkOverrideOnExpire = false;
                    p.jobs.StartJob(job, JobCondition.InterruptForced);
                }
                Find.CameraDriver.SetRootPosAndSize((center + new IntVec3(2, 0, 0)).ToVector3Shifted(), 16f);
                Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;
                state = 1; wait = 60;
                return;
            }
            if (--wait > 0) return;
            if (state >= 1 && state <= 3)
            {
                foreach (var w in Find.WindowStack.Windows.OfType<EditWindow_Log>().ToList()) w.Close(false);
                var focus = turrets[Mathf.Min(state - 1, turrets.Count - 1)];
                Find.CameraDriver.SetRootPosAndSize(focus.DrawPos, 8f);
                if (shotPending) { ScreenCapture.CaptureScreenshot(Path.Combine(dir, "turret_" + focus.def.defName + ".png")); state++; wait = 45; shotPending = false; }
                else { shotPending = true; wait = 8; }
                return;
            }
            string info = string.Join(", ", turrets.Select(t => t.def.defName + " target=" + ((t as Building_TurretGun)?.CurrentTarget.Thing?.LabelShort ?? "-")));
            Log.Message("[Convergence] Turret shots saved in " + dir + "\n  " + info + "\n  ALL OK");
            state = 5;
        }
    }
}
