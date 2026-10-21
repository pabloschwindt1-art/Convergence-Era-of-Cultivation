using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Convergence.QiTech
{
    /// <summary>
    /// Vitrina de Ingeniería Espiritual (con <c>-cvgqitechlab</c> y <c>-quicktest</c>, ventana visible): Convergencia activa,
    /// Microelectrónica, Fundamentos del Qi y Resonancia investigados, un banco avanzado con un resonador encendido al lado, una mesa
    /// de cantero con piedras espirituales, Instrumentación Espiritual Avanzada como proyecto actual y la ventana de investigación
    /// abierta en la pestaña. Sin el argumento no hace nada; no escribe "ALL OK", así que el juego queda abierto.
    /// </summary>
    public class GameComponent_QiTechLab : GameComponent
    {
        private static readonly bool Enabled = GenCommandLine.CommandLineArgPassed("cvgqitechlab");
        private int state;

        public GameComponent_QiTechLab(Game game) { }

        public override void GameComponentTick()
        {
            if (!Enabled || state > 1) return;
            var map = Find.CurrentMap;
            if (map == null || Find.TickManager.TicksGame < 120) return;
            if (state == 0) { SetUp(map); state = 1; return; }
            if (Find.TickManager.TicksGame < 240) return;
            OpenResearchTab();
            state = 2;
        }

        private static void SetUp(Map map)
        {
            if (!GameComponent_Convergence.Active) GameComponent_Convergence.Instance.MarkOccurred();
            var rm = Find.ResearchManager;
            void Finish(ResearchProjectDef p)
            {
                if (p == null || p.IsFinished) return;
                foreach (var q in (p.prerequisites ?? new System.Collections.Generic.List<ResearchProjectDef>())
                    .Concat(p.hiddenPrerequisites ?? new System.Collections.Generic.List<ResearchProjectDef>())) Finish(q);
                rm.FinishProject(p, doCompletionDialog: false, null, doCompletionLetter: false);
            }
            Finish(DefDatabase<ResearchProjectDef>.GetNamed("CVG_Research_SpiritResonance"));
            Finish(DefDatabase<ResearchProjectDef>.GetNamedSilentFail("Stonecutting"));

            var c = map.Center + new IntVec3(0, 0, 6);
            foreach (var cell in GenRadial.RadialCellsAround(c, 9f, true).Where(x => x.InBounds(map)))
            {
                foreach (var t in cell.GetThingList(map).ToList()) if (t.def.destroyable && !(t is Pawn)) t.Destroy();
                if (cell.GetTerrain(map).passability != Traversability.Standable) map.terrainGrid.SetTerrain(cell, TerrainDefOf.Soil);
            }
            Thing Spawn(string def, IntVec3 cell, string stuff = null)
            {
                var t = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed(def), stuff != null ? DefDatabase<ThingDef>.GetNamed(stuff) : null);
                t.SetFaction(Faction.OfPlayer);
                return GenSpawn.Spawn(t, cell, map);
            }
            var bench = Spawn("HiTechResearchBench", c, "Steel");
            var res = Spawn("CVG_SpiritResonator", c + new IntVec3(3, 0, 0));
            // Sin red eléctrica en la vitrina: se simula la corriente.
            bench.TryGetComp<CompPowerTrader>().PowerOn = true;
            res.TryGetComp<CompPowerTrader>().PowerOn = true;
            Spawn("TableStonecutter", c + new IntVec3(-5, 0, 0), "Steel");
            var stones = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("CVG_SpiritStone"));
            stones.stackCount = 60;
            GenPlace.TryPlaceThing(stones, c + new IntVec3(-5, 0, -3), map, ThingPlaceMode.Near);
            var crystals = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("CVG_PrimordialCrystal"));
            crystals.stackCount = 4;
            GenPlace.TryPlaceThing(crystals, c + new IntVec3(3, 0, -3), map, ThingPlaceMode.Near);

            rm.SetCurrentProject(DefDatabase<ResearchProjectDef>.GetNamed("CVG_Research_AdvancedSpiritInstrumentation"));
            Find.CameraDriver.SetRootPosAndSize(c.ToVector3Shifted(), 14f);
        }

        private static void OpenResearchTab()
        {
            Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Research, playSound: false);
            var tab = DefDatabase<ResearchTabDef>.GetNamed("CVG_SpiritualEngineering");
            if (MainButtonDefOf.Research.TabWindow is MainTabWindow_Research w)
            {
                Traverse.Create(w).Property("CurTab").SetValue(tab);
                w.Select(DefDatabase<ResearchProjectDef>.GetNamed("CVG_Research_AdvancedSpiritInstrumentation"));
            }
        }
    }
}
