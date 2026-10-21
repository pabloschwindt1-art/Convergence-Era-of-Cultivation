using System;
using System.Linq;
using System.Text;
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
        }
    }
}
