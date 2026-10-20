using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Convergence.Stories
{
    /// <summary>
    /// Prueba del trabajo de las marionetas de trabajo (con <c>-cvgworktest</c> y <c>-quicktest</c>): las tres del titiritero
    /// (madera, albañil, minera) con trabajo a mano (cosas tiradas para acarrear, suciedad, un cultivo para cosechar) durante
    /// medio día; registra qué trabajos hace cada una y, si alguna no toma ninguno, por qué. Sin el argumento no hace nada.
    /// </summary>
    public class GameComponent_WorkFlowTest : GameComponent
    {
        private static readonly bool Enabled = GenCommandLine.CommandLineArgPassed("cvgworktest");
        private int state, startTick;
        private List<Pawn> puppets = new List<Pawn>();
        private readonly Dictionary<Pawn, HashSet<string>> seen = new Dictionary<Pawn, HashSet<string>>();
        private List<Plant> trees = new List<Plant>(), crops = new List<Plant>();
        private int filthStart;
        private readonly List<Thing> steelStacks = new List<Thing>();
        private readonly Dictionary<string, int> jobCount = new Dictionary<string, int>();
        private Job lastWoodJob;

        public GameComponent_WorkFlowTest(Game game) { }

        public override void GameComponentTick()
        {
            if (!Enabled || state == 2 || Find.CurrentMap == null) return;
            var map = Find.CurrentMap;
            int now = Find.TickManager.TicksGame;
            if (state == 0)
            {
                if (now < 300) return;
                var master = map.mapPawns.FreeColonistsSpawned.FirstOrDefault();
                var link = DefDatabase<HediffDef>.GetNamed("CVG_PuppetHeartThread");
                foreach (var k in new[] { "CVG_Puppet_Wood", "CVG_Puppet_Mason", "CVG_Puppet_Miner" })
                    puppets.AddRange(ScenPart_StartingPuppets.Equip(master, link, DefDatabase<PawnKindDef>.GetNamed(k), 1));
                foreach (var p in puppets) seen[p] = new HashSet<string>();
                // trabajo a mano: madera tirada lejos de la zona de acopio, suciedad, y una zona de acopio
                var root = master.Position;
                // lo que trae el mapa de prueba puede estar prohibido (nadie lo acarrea, como en vanilla): se permite todo
                foreach (var t in map.listerThings.AllThings.Where(t => t.def.EverHaulable && t.Spawned).ToList()) t.SetForbidden(false, false);
                for (int i = 0; i < 6; i++)
                    if (CellFinder.TryFindRandomCellNear(root, map, 12, c => c.Standable(map) && c.GetFirstItem(map) == null && !GenAdj.CellsAdjacent8Way(new TargetInfo(c, map)).Any(x => x.InBounds(map) && x.GetFirstItem(map) != null), out var cell))
                    {
                        var steel = ThingMaker.MakeThing(ThingDefOf.Steel); steel.stackCount = 20;
                        GenSpawn.Spawn(steel, cell, map);
                        steelStacks.Add(steel);
                        FilthMaker.TryMakeFilth(cell + IntVec3.East, map, ThingDefOf.Filth_Dirt, 3);
                    }
                // minera: una roca marcada; albañil: una pared de madera para construir
                var rock = map.listerThings.AllThings.OfType<Mineable>().Where(m => m.def.building.isNaturalRock && GenAdj.CardinalDirections.Any(d => (m.Position + d).Standable(map)))
                    .OrderBy(m => m.Position.DistanceToSquared(root)).FirstOrDefault();
                if (rock != null) map.designationManager.AddDesignation(new Designation(rock, DesignationDefOf.Mine));
                if (CellFinder.TryFindRandomCellNear(root, map, 10, c => c.Standable(map) && c.GetFirstItem(map) == null && c.GetFirstBuilding(map) == null, out var wc))
                    GenConstruct.PlaceBlueprintForBuild(ThingDefOf.Wall, wc, map, Rot4.North, Faction.OfPlayer, ThingDefOf.WoodLog);
                if (CellFinder.TryFindRandomCellNear(root, map, 15, c => c.Standable(map) && !c.Roofed(map) && c.GetFirstItem(map) == null, out var zc))
                {
                    var zone = new Zone_Stockpile(StorageSettingsPreset.DefaultStockpile, map.zoneManager);
                    map.zoneManager.RegisterZone(zone);
                    foreach (var c in GenRadial.RadialCellsAround(zc, 2.5f, true).Where(c => c.InBounds(map) && c.Standable(map) && map.zoneManager.ZoneAt(c) == null)) zone.AddCell(c);
                }
                // los colonos reclutados no trabajan: todo el trabajo queda para las marionetas
                foreach (var c in map.mapPawns.FreeColonistsSpawned) if (c.drafter != null) c.drafter.Drafted = true;
                // árboles para talar y un cultivo maduro para cosechar (madera: cortar plantas y sembrar/cosechar)
                trees = map.listerThings.ThingsInGroup(ThingRequestGroup.Plant).OfType<Plant>()
                    .Where(t => t.def.plant.IsTree && t.HarvestableNow && t.Position.InHorDistOf(root, 25)).Take(3).ToList();
                foreach (var t in trees) map.designationManager.AddDesignation(new Designation(t, DesignationDefOf.CutPlant));
                if (CellFinder.TryFindRandomCellNear(root, map, 15, c => c.Standable(map) && !c.Roofed(map) && c.GetFirstItem(map) == null && c.GetPlant(map) == null && c.GetTerrain(map).fertility > 0.5f, out var gc))
                {
                    var grow = new Zone_Growing(map.zoneManager);
                    map.zoneManager.RegisterZone(grow);
                    foreach (var c in GenRadial.RadialCellsAround(gc, 1.5f, true).Where(c => c.InBounds(map) && c.Standable(map) && map.zoneManager.ZoneAt(c) == null && c.GetPlant(map) == null && c.GetTerrain(map).fertility > 0.5f)) grow.AddCell(c);
                    var crop = grow.GetPlantDefToGrow();
                    foreach (var c in grow.Cells.Take(4))
                    {
                        var pl = (Plant)GenSpawn.Spawn(crop, c, map);
                        pl.Growth = 1f;
                        crops.Add(pl);
                    }
                }
                filthStart = map.listerFilthInHomeArea.FilthInHomeArea.Count;
                startTick = now;
                Find.TickManager.CurTimeSpeed = TimeSpeed.Ultrafast;
                state = 1;
                Diagnose(map, "start");
                return;
            }
            foreach (var p in puppets) if (p.CurJobDef != null) seen[p].Add(p.CurJobDef.defName);
            var wp = puppets.FirstOrDefault(p => p.def.defName == "CVG_Puppet_Wood");
            if (wp?.CurJob != null && wp.CurJob != lastWoodJob)
            {
                lastWoodJob = wp.CurJob;
                jobCount.TryGetValue(wp.CurJob.def.defName, out int n); jobCount[wp.CurJob.def.defName] = n + 1;
            }
            if (now - startTick == 1250) Diagnose(map, "t+0.5h");
            if (now - startTick < GenDate.TicksPerDay / 2) return;
            var sb = new StringBuilder("[Convergence] Work flow test (work puppets, half a day)");
            int fails = 0;
            foreach (var p in puppets)
            {
                var jobs = seen[p].Where(j => j != "Wait_Wander" && j != "GotoWander" && j != "Wait" && j != "Wait_MaintainPosture" && j != "MechCharge").ToList();
                if (jobs.Count == 0) fails++;
                sb.Append($"\n    {(jobs.Count > 0 ? "ok  " : "FAIL")} {p.def.defName}: {(jobs.Count > 0 ? string.Join(", ", jobs) : "no work at all (" + string.Join(", ", seen[p]) + ")")}");
            }
            Diagnose(map, "end");
            int loose = steelStacks.Count(t => t.Spawned && !t.IsInValidStorage());
            sb.Append($"\n    info: test steel stacks still outside storage {loose}/{steelStacks.Count}, filth in home {filthStart} -> {map.listerFilthInHomeArea.FilthInHomeArea.Count}, trees cut {trees.Count(t => t.Destroyed)}/{trees.Count}, crops harvested {crops.Count(t => t.Destroyed)}/{crops.Count}");
            var woodPuppet = puppets.FirstOrDefault(p => p.def.defName == "CVG_Puppet_Wood");
            if (woodPuppet != null)
            {
                foreach (var t in trees.Where(t => !t.Destroyed)) sb.Append($"\n    tree {t.def.defName}: wood can {(new WorkGiver_PlantsCut { def = DefDatabase<WorkGiverDef>.GetNamed("PlantsCut") }.HasJobOnThing(woodPuppet, t, false))}, reserved {map.reservationManager.IsReservedByAnyoneOf(t, Faction.OfPlayer)}, reach {woodPuppet.CanReach(t, PathEndMode.Touch, Danger.Some)}");
                foreach (var t in steelStacks.Where(t => t.Spawned && !t.IsInValidStorage()))
                {
                    bool store = StoreUtility.TryFindBestBetterStoreCellFor(t, woodPuppet, map, StoreUtility.CurrentStoragePriorityOf(t), woodPuppet.Faction, out var dest);
                    sb.Append($"\n    logs x{t.stackCount} at {t.Position}: canHaul {HaulAIUtility.PawnCanAutomaticallyHaulFast(woodPuppet, t, false)}, forbidden {t.IsForbidden(woodPuppet)} (faction {t.IsForbidden(Faction.OfPlayer)}, inArea {t.Position.InAllowedArea(woodPuppet)}, area {woodPuppet.playerSettings?.EffectiveAreaRestrictionInPawnCurrentMap?.Label ?? "none"}), store {store} {dest}, reach {woodPuppet.CanReach(t, PathEndMode.ClosestTouch, Danger.Some)}, haulJob {HaulAIUtility.HaulToStorageJob(woodPuppet, t, false)?.def.defName ?? "none"}, failReason {JobFailReason.Reason}");
                }
                sb.Append($"\n    wood jobs: {string.Join(", ", jobCount.Select(kv => kv.Key + " x" + kv.Value))}; carry capacity {woodPuppet.GetStatValue(StatDefOf.CarryingCapacity):0}");
                foreach (var t in crops.Where(t => !t.Destroyed)) sb.Append($"\n    crop {t.def.defName}: growth {t.Growth:P0}, harvestable {t.HarvestableNow}");
            }
            if (fails == 0) Log.Message(sb + "\n  ALL OK"); else Log.Error(sb + $"\n  FAILURES: {fails}");
            state = 2;
        }

        private void Diagnose(Map map, string when)
        {
            foreach (var p in puppets)
            {
                var ws = p.workSettings;
                string types = string.Join(" ", p.RaceProps.mechEnabledWorkTypes.Select(w => $"{w.defName}:{(ws?.WorkIsActive(w) ?? false ? ws.GetPriority(w).ToString() : "off")}{(p.WorkTypeIsDisabled(w) ? "(disabled)" : "")}"));
                Log.Message($"[Convergence] work diag {when} {p.def.defName}: job {p.CurJobDef?.defName}, everWork {ws?.EverWork}, mode {p.GetMechWorkMode()}, overseer {p.GetOverseer()?.LabelShort}, energy {p.needs?.energy?.CurLevelPercentage:P0}, manip {p.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation):P0}, move {p.health.capacities.GetLevel(PawnCapacityDefOf.Moving):P0}, types [{types}], hediffs [{string.Join(" ", p.health.hediffSet.hediffs.Select(h => h.def.defName))}]");
            }
        }
    }
}
