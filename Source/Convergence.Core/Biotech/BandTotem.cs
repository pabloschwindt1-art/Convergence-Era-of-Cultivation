using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// En un edificio con <c>CompBandNode</c> propio (tótem de hilos): cuenta como nodo de banda para el mecanitor al que está sintonizado
    /// mientras tenga combustible (el nodo vanilla solo cuenta <c>ThingDefOf.BandNode</c> y la red eléctrica). Sin electricidad:
    /// lleva un <c>CompPowerTrader</c> de consumo 0 (red propia, como el taller de marionetas) y un <c>CompRefuelable</c> de piedras espirituales.
    /// </summary>
    public class DefModExtension_BandNodeSubstitute : DefModExtension
    {
    }

    /// <summary>Un tótem no está en ninguna red eléctrica: su "energía" es tener combustible (piedras espirituales).</summary>
    [HarmonyPatch(typeof(CompPowerTrader), nameof(CompPowerTrader.PowerOn), MethodType.Getter)]
    public static class Patch_CompPowerTrader_PowerOn
    {
        public static void Postfix(CompPowerTrader __instance, ref bool __result)
        {
            if (__result || __instance.parent?.def == null || !__instance.parent.def.HasModExtension<DefModExtension_BandNodeSubstitute>()) return;
            var fuel = __instance.parent.TryGetComp<CompRefuelable>();
            __result = fuel == null || fuel.HasFuel;
        }
    }

    [HarmonyPatch(typeof(Hediff_BandNode), nameof(Hediff_BandNode.RecacheBandNodes))]
    public static class Patch_HediffBandNode_RecacheBandNodes
    {
        private static readonly AccessTools.FieldRef<Hediff_BandNode, int> CountRef = AccessTools.FieldRefAccess<Hediff_BandNode, int>("cachedTunedBandNodesCount");
        private static readonly AccessTools.FieldRef<Hediff_BandNode, HediffStage> StageRef = AccessTools.FieldRefAccess<Hediff_BandNode, HediffStage>("curStage");
        private static List<ThingDef> defs;

        public static void Postfix(Hediff_BandNode __instance)
        {
            defs ??= DefDatabase<ThingDef>.AllDefsListForReading.Where(d => d.GetModExtension<DefModExtension_BandNodeSubstitute>() != null).ToList();
            if (defs.Count == 0) return;
            int extra = 0;
            var pawn = __instance.pawn;
            foreach (var map in Find.Maps)
                foreach (var def in defs)
                    foreach (var b in map.listerBuildings.AllBuildingsColonistOfDef(def))
                    {
                        var node = b.TryGetComp<CompBandNode>();
                        if (node == null || node.tunedTo != pawn) continue;
                        var power = b.TryGetComp<CompPowerTrader>();
                        if (power != null && !power.PowerOn) continue;
                        var fuel = b.TryGetComp<CompRefuelable>();
                        if (fuel != null && !fuel.HasFuel) continue;
                        extra++;
                    }
            if (extra == 0) return;
            CountRef(__instance) += extra;
            StageRef(__instance) = null;
        }
    }

    /// <summary>Autotest del tótem de hilos: solo con <c>-cvgbandtotemtest</c> (y un mapa, p. ej. -quicktest).</summary>
    public class GameComponent_BandTotemTest : GameComponent
    {
        private static readonly bool Enabled = GenCommandLine.CommandLineArgPassed("cvgbandtotemtest");
        private int phase;
        private int waitUntil;
        private readonly StringBuilder sb = new StringBuilder("[Convergence] Band totem self-test");
        private int fails;
        private Pawn master;
        private Thing totem;
        private float bandwidth0;

        public GameComponent_BandTotemTest(Game game) { }

        private void Check(bool ok, string what)
        {
            sb.Append("\n    " + (ok ? "ok   " : "FAIL ") + what);
            if (!ok) fails++;
        }

        public override void GameComponentTick()
        {
            if (!Enabled || phase < 0) return;
            var map = Find.CurrentMap;
            if (map == null || Find.TickManager.TicksGame < 200 || Find.TickManager.TicksGame < waitUntil) return;
            try { Step(map); }
            catch (Exception e) { Check(false, "exception: " + e); Finish(); }
        }

        private void Finish()
        {
            phase = -1;
            totem?.Destroy();
            sb.Append(fails == 0 ? "\n  ALL OK" : "\n  FAILURES: " + fails);
            if (fails == 0) Log.Message(sb.ToString()); else Log.Error(sb.ToString());
        }

        private void TestPuppetRoster(Map map, Pawn master, HediffDef thread)
        {
            var roles = new (string kind, string work)[] { ("CVG_Puppet_Wood", "Hauling"), ("CVG_Puppet_Miner", "Mining"), ("CVG_Puppet_Mason", "Construction"), ("CVG_Puppet_Blade", null), ("CVG_Puppet_Bronze", null), ("CVG_Puppet_Jade", null) };
            foreach (var (kindName, work) in roles)
            {
                var kind = DefDatabase<PawnKindDef>.GetNamedSilentFail(kindName);
                if (kind == null) { Check(false, kindName + " exists"); continue; }
                var puppet = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, Faction.OfPlayer));
                GenSpawn.Spawn(puppet, CellFinder.RandomClosewalkCellNear(master.Position, map, 4), map);
                var pc = puppet.GetComp<CompPuppet>();
                Check(pc != null, kindName + ": puppet component");
                if (work != null)
                    Check(puppet.RaceProps.mechEnabledWorkTypes.Any(w => w.defName == work), kindName + ": can do " + work);
                if (kindName == "CVG_Puppet_Miner")
                {
                    Check(puppet.GetStatValue(StatDefOf.MiningSpeed) > 0.3f, "miner mining speed " + puppet.GetStatValue(StatDefOf.MiningSpeed).ToString("0.##"));
                    var rock = map.listerThings.AllThings.OfType<Mineable>().FirstOrDefault(m => m.def.building.isNaturalRock && !m.def.building.isResourceRock && GenAdj.CardinalDirections.Any(d => (m.Position + d).Standable(map)));
                    if (rock != null)
                    {
                        map.designationManager.AddDesignation(new Designation(rock, DesignationDefOf.Mine));
                        var job = new WorkGiver_Miner().JobOnThing(puppet, rock);
                        Check(job != null && job.def == JobDefOf.Mine, "the miner puppet gets a mining job");
                        map.designationManager.RemoveAllDesignationsOn(rock);
                    }
                }
                if (kindName == "CVG_Puppet_Blade") Check(puppet.equipment.Primary == null && puppet.VerbTracker.AllVerbs.Any(v => v.IsMeleeAttack), "blade puppet fights in melee without a weapon");
                puppet.Destroy();
            }
            var workshop = DefDatabase<ThingDef>.GetNamed("CVG_PuppetWorkshop");
            foreach (var r in new[] { "Wood", "Miner", "Mason", "Blade", "Bronze", "Jade" })
                Check(workshop.recipes.Any(x => x.defName == "CVG_BuildPuppet_" + r), "workshop builds " + r);
        }

        private void Step(Map map)
        {
            int now = Find.TickManager.TicksGame;
            var hediffDef = DefDatabase<HediffDef>.GetNamedSilentFail("CVG_PuppetHeartThread");
            var totemDef = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_ThreadTotem");
            if (hediffDef == null || totemDef == null) { sb.Append("\n  (Biotech Wuxia not loaded: skipped)"); phase = -1; Log.Message(sb.ToString()); return; }
            switch (phase)
            {
                case 0:
                    master = map.mapPawns.FreeColonistsSpawned.First();
                    foreach (var (name, offset) in new[] { ("CVG_Apparel_PuppeteerMask", 6f), ("CVG_Apparel_ThousandThreadsCrown", 9f), ("CVG_Apparel_GrandPuppeteerRobe", 9f) })
                    {
                        var ad = DefDatabase<ThingDef>.GetNamedSilentFail(name);
                        if (ad == null) { sb.Append("\n    (" + name + " not loaded: skipped)"); continue; }
                        Check(ad.apparel.mechanitorApparel && ad.equippedStatOffsets.GetStatOffsetFromList(StatDefOf.MechBandwidth) == offset && ad.recipeMaker != null && ad.costList.All(c => c.thingDef.defName.StartsWith("CVG_") || c.thingDef.defName == "Jade"),
                            name + ": +" + offset + " bandwidth, craftable, no industrial components");
                        var wear = (Apparel)ThingMaker.MakeThing(ad, ad.MadeFromStuff ? GenStuff.DefaultStuffFor(ad) : null);
                        master.apparel.Wear(wear, false);
                    }
                    TestPuppetRoster(map, master, hediffDef);
                    master.health.AddHediff(hediffDef);
                    Check(master.mechanitor != null, "the heart thread makes a mechanitor");
                    master.apparel.WornApparel.ToList().ForEach(a => master.apparel.Remove(a));
                    bandwidth0 = master.GetStatValue(StatDefOf.MechBandwidth);
                    totem = ThingMaker.MakeThing(totemDef);
                    totem.SetFactionDirect(Faction.OfPlayer);
                    // lejos del centro: otras pruebas (isla, formaciones) despejan esa zona mientras corren a la vez sobre el mismo mapa
                    GenSpawn.Spawn(totem, CellFinder.RandomClosewalkCellNear(map.Center + new IntVec3(-45, 0, 45), map, 8), map);
                    var fuel = totem.TryGetComp<CompRefuelable>();
                    fuel.Refuel(5f);
                    phase = 1; waitUntil = now + 120; break;
                case 1:
                    var power = totem.TryGetComp<CompPowerTrader>();
                    Check(power != null && power.PowerOn, "the totem is 'powered' by its spirit stones (no electricity needed)");
                    var node = totem.TryGetComp<CompBandNode>();
                    node.TuneTo(master);
                    node.tuningTimeLeft = 2;
                    phase = 2; waitUntil = now + 30; break;
                case 2:
                    node = totem.TryGetComp<CompBandNode>();
                    Check(node.tunedTo == master, "totem tuned to the master");
                    var bnd = (Hediff_BandNode)master.health.hediffSet.GetFirstHediffOfDef(DefDatabase<HediffDef>.GetNamed("BandNode"));
                    Check(bnd != null, "the band node hediff was added to the master");
                    bnd?.RecacheBandNodes();
                    float bw = master.GetStatValue(StatDefOf.MechBandwidth);
                    Check(bw >= bandwidth0 + 0.99f, $"bandwidth {bandwidth0:0.#} -> {bw:0.#} (+1)");
                    totem.TryGetComp<CompRefuelable>().ConsumeFuel(100f);
                    Check(!totem.TryGetComp<CompPowerTrader>().PowerOn, "without spirit stones the totem is off");
                    bnd?.RecacheBandNodes();
                    float bw2 = master.GetStatValue(StatDefOf.MechBandwidth);
                    Check(bw2 < bw - 0.5f, $"without spirit stones the totem stops helping ({bw:0.#} -> {bw2:0.#})");
                    Finish(); break;
            }
        }
    }
}
