using System;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Convergence.QiTech
{
    /// <summary>
    /// Autotest de Qi-Tech. Solo con <c>-cvgqitechtest</c> (y un mapa, p. ej. -quicktest): convertidor en los dos modos con la red de qi,
    /// cámara de estabilización y exoesqueleto. Inerte sin el argumento.
    /// </summary>
    public class GameComponent_QiTechTest : GameComponent
    {
        private static readonly bool Enabled = GenCommandLine.CommandLineArgPassed("cvgqitechtest");
        private static bool done; // estático: no repetir tras cargar la partida del autotest militar
        private readonly StringBuilder sb = new StringBuilder("[Convergence] Qi-Tech self-test");
        private int fails;

        public GameComponent_QiTechTest(Game game) { }

        private void Check(bool ok, string what)
        {
            sb.Append("\n    " + (ok ? "ok   " : "FAIL ") + what);
            if (!ok) fails++;
        }

        public override void GameComponentTick()
        {
            if (!Enabled || done) return;
            if (Find.CurrentMap == null || Find.TickManager.TicksGame < 200) return;
            done = true;
            try { Run(Find.CurrentMap); }
            catch (Exception e) { Check(false, "exception: " + e); }
            sb.Append(fails == 0 ? "\n  ALL OK" : "\n  FAILURES: " + fails);
            if (fails == 0) Log.Message(sb.ToString()); else Log.Error(sb.ToString());
        }

        private static Thing Spawn(string def, Map map, IntVec3 cell, string stuff = null)
        {
            var d = DefDatabase<ThingDef>.GetNamed(def);
            var t = ThingMaker.MakeThing(d, stuff != null ? DefDatabase<ThingDef>.GetNamed(stuff) : null);
            t.SetFaction(Faction.OfPlayer);
            GenSpawn.Spawn(t, cell, map);
            return t;
        }

        private void Run(Map map)
        {
            foreach (var r in new[] { "CVG_Research_QiTech", "CVG_Research_QiStabilization", "CVG_Research_QiExosuit" })
                Check(DefDatabase<ResearchProjectDef>.GetNamedSilentFail(r) != null, "research " + r);
            Check(DefDatabase<ResearchProjectDef>.GetNamed("CVG_Research_QiTech").prerequisites.Any(p => p.defName == "Electricity"), "qi-tech needs industrial research (Electricity)");

            var c = map.Center + new IntVec3(-30, 0, 0);
            foreach (var cell in GenRadial.RadialCellsAround(c, 8f, true).Where(x => x.InBounds(map)))
                foreach (var t in cell.GetThingList(map).ToList()) if (t.def.destroyable && t.def.category != ThingCategory.Pawn) t.Destroy();
            var pillar = Spawn("CVG_FormationPillar", map, c);
            var crystal = Spawn("CVG_QiCrystal", map, c + new IntVec3(2, 0, 0));
            var conv = Spawn("CVG_QiConverter", map, c + new IntVec3(0, 0, 3));
            var node = conv.TryGetComp<CompQiConverterNode>();
            var power = conv.TryGetComp<CompQiConverterPower>();
            Check(node != null && power != null, "converter has both comps");
            if (node == null || power == null) return;
            var mgr = map.GetComponent<QiNetworkManager>();
            mgr.EnsureBuilt();
            crystal.TryGetComp<CompQiNode>().Stored = 500f;

            // qi -> electricidad
            mgr.Step(0.01f);
            power.UpdateDesiredPowerOutput();
            Check(node.Mode == ConverterMode.QiToPower && node.ConsumeDay > 0f && node.ProduceDay == 0f, "default mode consumes qi");
            Check(node.Powered && power.PowerOutput > 900f, "with qi stored it delivers about 1000 W (" + power.PowerOutput.ToString("F0") + ")");
            crystal.TryGetComp<CompQiNode>().Stored = 0f;
            mgr.Step(0.01f);
            power.UpdateDesiredPowerOutput();
            Check(!node.Powered && power.PowerOutput == 0f, "without qi it delivers nothing");

            // electricidad -> qi
            node.SetMode(ConverterMode.PowerToQi);
            power.UpdateDesiredPowerOutput();
            Check(node.ConsumeDay == 0f && !node.IsConsumer, "reverse mode does not consume qi");
            Check(power.PowerOutput <= 0f, "reverse mode draws power instead of giving it");
            power.PowerOn = false;
            Check(node.ProduceDay == 0f, "reverse mode produces no qi without current");
            power.PowerOn = true;
            Check(node.ProduceDay > 0f, "reverse mode produces qi with current (" + node.ProduceDay.ToString("F0") + "/day)");
            mgr.Step(0.01f);
            Check(crystal.TryGetComp<CompQiNode>().Stored > 0f, "the crystal collects the converted qi");
            node.SetMode(ConverterMode.QiToPower);

            // cámara de estabilización y exoesqueleto
            var chamber = Spawn("CVG_StabilizationChamber", map, c + new IntVec3(4, 0, 4), null);
            Check(chamber.TryGetComp<CompStabilizationChamber>() != null, "chamber has its comp");
            Check(chamber.def.tickerType != TickerType.Never, "chamber ticks (otherwise it never stabilizes anyone in play)");
            Check(chamber.TryGetComp<CompStabilizationChamber>().Apply() == 0, "an empty chamber stabilizes nobody");
            var pawn = map.mapPawns.FreeColonistsSpawned.FirstOrDefault();
            if (pawn == null) { sb.Append("\n    (no colonist)"); return; }
            var suit = (Apparel)ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("CVG_Apparel_QiTechExosuit"));
            float baseDiss = pawn.GetStatValue(CVG_DefOf.CVG_CelestialDissonance);
            pawn.apparel.Wear(suit, dropReplacedApparel: true, locked: false);
            float withSuit = pawn.GetStatValue(CVG_DefOf.CVG_CelestialDissonance);
            Check(withSuit - baseDiss > 0.8f, "the exoskeleton adds high celestial dissonance (+" + (withSuit - baseDiss).ToString("F2") + ")");
            Check(CompStabilizationChamber.NeedsStabilizing(pawn), "a wearer of the exoskeleton needs stabilizing");
            pawn.health.AddHediff(DefDatabase<HediffDef>.GetNamed("CVG_Stabilized"));
            float stabilized = pawn.GetStatValue(CVG_DefOf.CVG_CelestialDissonance);
            Check(withSuit - stabilized > 0.3f, "stabilization lowers the dissonance (" + withSuit.ToString("F2") + " -> " + stabilized.ToString("F2") + ")");
            pawn.health.RemoveHediff(pawn.health.hediffSet.GetFirstHediffOfDef(DefDatabase<HediffDef>.GetNamed("CVG_Stabilized")));
            pawn.apparel.Remove(suit);
            suit.Destroy();
            foreach (var t in new[] { pillar, crystal, conv, chamber }) t.Destroy();
            RunSpiritEngineering(map, c + new IntVec3(0, 0, -6));
        }

        /// <summary>Ingeniería Espiritual: pestaña, bloqueo real por la Convergencia, investigación de un mortal, resonador y cristal.</summary>
        private void RunSpiritEngineering(Map map, IntVec3 at)
        {
            var tab = DefDatabase<ResearchTabDef>.GetNamedSilentFail("CVG_SpiritualEngineering");
            var inTab = DefDatabase<ResearchProjectDef>.AllDefs.Where(p => p.tab == tab).ToList();
            Check(tab != null && inTab.Count == 10, "spiritual engineering tab with its 10 projects (" + inTab.Count + ")");
            Check(inTab.All(p => p.HasModExtension<DefModExtension_RequiresConvergence>()), "every project of the tab needs the Convergence");
            var fund = DefDatabase<ResearchProjectDef>.GetNamed("CVG_Research_QiFundamentals");
            var reso = DefDatabase<ResearchProjectDef>.GetNamed("CVG_Research_SpiritResonance");
            var instr = DefDatabase<ResearchProjectDef>.GetNamed("CVG_Research_AdvancedSpiritInstrumentation");
            Check(DefDatabase<ResearchProjectDef>.GetNamed("CVG_Research_QiTech").prerequisites.Contains(fund), "qi-tech (same defName) now hangs from qi fundamentals");
            Check(fund.requiredResearchBuilding?.defName == "HiTechResearchBench" && instr.requiredResearchFacilities.Any(f => f.defName == "CVG_SpiritResonator"),
                "hi-tech bench for the branch; the resonator for advanced instrumentation");

            // Banco avanzado y prerrequisitos vanilla terminados; el bloqueo se prueba con la Convergencia apagada y encendida.
            foreach (var cell in GenRadial.RadialCellsAround(at, 5f, true).Where(x => x.InBounds(map)))
                foreach (var t in cell.GetThingList(map).ToList()) if (t.def.destroyable && t.def.category != ThingCategory.Pawn) t.Destroy();
            var bench = (Building_ResearchBench)Spawn("HiTechResearchBench", map, at, "Steel");
            var rm = Find.ResearchManager;
            void FinishWithPrereqs(ResearchProjectDef p)
            {
                if (p.IsFinished) return;
                foreach (var q in (p.prerequisites ?? new System.Collections.Generic.List<ResearchProjectDef>()).Concat(p.hiddenPrerequisites ?? new System.Collections.Generic.List<ResearchProjectDef>()))
                    FinishWithPrereqs(q);
                rm.FinishProject(p, doCompletionDialog: false, null, doCompletionLetter: false);
            }
            FinishWithPrereqs(DefDatabase<ResearchProjectDef>.GetNamed("MicroelectronicsBasics"));
            var conv = Traverse.Create(GameComponent_Convergence.Instance).Field("occurred");
            bool wasActive = conv.GetValue<bool>();
            conv.SetValue(false);
            Check(!fund.CanStartNow, "before the Convergence qi fundamentals cannot be started");
            conv.SetValue(true);
            Check(fund.CanStartNow, "after the Convergence it can");

            // Un colono sin raíz espiritual investiga de forma convencional.
            var mortal = map.mapPawns.FreeColonistsSpawned.FirstOrDefault(p => CompCultivator.For(p) == null || !CompCultivator.For(p).HasRoot);
            if (mortal != null)
            {
                rm.SetCurrentProject(fund);
                float before = rm.GetProgress(fund);
                rm.ResearchPerformed(100f, mortal);
                Check(rm.GetProgress(fund) > before, "a colonist without a spirit root advances qi fundamentals (" + before.ToString("0") + " -> " + rm.GetProgress(fund).ToString("0") + ")");
                rm.StopProject(fund);
            }
            else sb.Append("\n    (no colonist without a spirit root to research with)");
            rm.FinishProject(fund, false, null, false);
            Check(reso.CanStartNow, "spirit resonance opens after qi fundamentals");
            rm.FinishProject(reso, false, null, false);

            // Resonador: instalación del banco avanzado; sin él, o sin energía, no hay instrumentación avanzada.
            Check(!instr.CanBeResearchedAt(bench, true), "without a resonator advanced instrumentation cannot be researched");
            var res = Spawn("CVG_SpiritResonator", map, at + new IntVec3(3, 0, 0));
            Check(res.def.costList.Any(cc => cc.thingDef.defName == "CVG_PrimordialCrystal") && res.def.HasComp(typeof(CompPowerTrader)), "resonator needs power and primordial crystals");
            var power = res.TryGetComp<CompPowerTrader>();
            power.PowerOn = true;
            bool linked = bench.TryGetComp<CompAffectedByFacilities>().LinkedFacilitiesListForReading.Contains(res);
            Check(linked, "the resonator links to the hi-tech bench as a facility");
            Check(instr.CanBeResearchedAt(bench, true), "with a powered resonator advanced instrumentation can be researched");
            power.PowerOn = false;
            Check(!instr.CanBeResearchedAt(bench, true), "with the resonator unpowered it cannot");
            power.PowerOn = true;
            Check(!res.def.HasComp(typeof(CompQiNode)) && res.def.GetCompProperties<CompProperties_Refuelable>() == null, "the resonator neither makes, stores nor burns qi");

            // Cristal primordial (Core): 10 piedras espirituales en la mesa de cantero.
            var recipe = DefDatabase<RecipeDef>.GetNamedSilentFail("CVG_MakePrimordialCrystal");
            Check(recipe != null && recipe.ProducedThingDef?.defName == "CVG_PrimordialCrystal" && recipe.AllRecipeUsers.Any(u => u.defName == "TableStonecutter")
                && recipe.ingredients[0].GetBaseCount() == 10, "primordial crystal: 10 spirit stones at the stonecutter's table");

            conv.SetValue(wasActive);
            res.Destroy();
            bench.Destroy();
        }
    }
}
