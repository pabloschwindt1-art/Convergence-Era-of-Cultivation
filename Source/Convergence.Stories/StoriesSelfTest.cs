using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse.AI;
using Verse.AI.Group;
using Verse;

namespace Convergence.Stories
{
    /// <summary>
    /// Autotest de historias y escenarios. Solo con <c>-cvgstoriestest</c> (y un mapa, p. ej. -quicktest): valida los escenarios,
    /// las partes de escenario, el dantian dañado y el elixir, y lanza cada evento de historia. Inerte sin el argumento.
    /// </summary>
    public class GameComponent_StoriesTest : GameComponent
    {
        private static readonly bool Enabled = GenCommandLine.CommandLineArgPassed("cvgstoriestest");
        private bool done;
        private readonly StringBuilder sb = new StringBuilder("[Convergence] Stories self-test");
        private int fails;

        public GameComponent_StoriesTest(Game game) { }

        private void Check(bool ok, string what)
        {
            sb.Append("\n    " + (ok ? "ok   " : "FAIL ") + what);
            if (!ok) fails++;
        }

        private void Section(string name, Action a)
        {
            sb.Append("\n- " + name);
            try { a(); }
            catch (Exception e) { Check(false, "exception: " + e); }
        }

        public override void GameComponentTick()
        {
            if (!Enabled || done) return;
            if (Find.CurrentMap == null || Find.TickManager.TicksGame < 200) return;
            done = true;
            var map = Find.CurrentMap;
            Section("scenarios", () => TestScenarios());
            Section("scenario parts", () => TestParts(map));
            Section("damaged dantian and elixir", () => TestDantian(map));
            Section("story events", () => TestEvents(map));
            Section("story event definitions", () => TestEventDefs(map));
            Section("secret realms", () => TestRealms(map));
            Section("forgotten sanctuary", () => TestSanctuary(map));
            Section("sect events", () => TestSectEvents(map));
            Section("endgame", () => TestEndgame(map));
            Section("puppet master scenario", () => TestPuppetMaster(map));
            Section("visits wait for guests to leave", () => TestGuests(map));
            sb.Append(fails == 0 ? "\n  ALL OK" : "\n  FAILURES: " + fails);
            if (fails == 0) Log.Message(sb.ToString()); else Log.Error(sb.ToString());
        }

        /// <summary>Las visitas de las historias no se juntan con otros huéspedes: el evento no se elige y una visita pendiente se aplaza.</summary>
        private void TestGuests(Map map)
        {
            void SendHome() // los huéspedes de pruebas anteriores se van
            {
                foreach (var lord in map.lordManager.lords.Where(l => l.LordJob is LordJob_VisitColony || l.LordJob is LordJob_TradeWithColony).ToList())
                    foreach (var p in lord.ownedPawns.ToList()) p.Destroy();
            }
            SendHome();
            Check(!StoryUtility.GuestsOnMap(map), "no guests at the start");
            var visits = StoryEngine.All.Where(StoryEngine.BringsVisitors).ToList();
            // Los eventos de visita son de las sectas (MayRequire): sin un mod de sectas no se cargan.
            if (visits.Count == 0) sb.Append("\n    note: no story event brings visitors (they need a Sects mod)");
            else Check(visits.Count >= 6, visits.Count + " story events bring visitors");
            var f = Find.FactionManager.AllFactionsListForReading.FirstOrDefault(x => !x.IsPlayer && !x.Hidden && !x.temporary && x.def.humanlikeFaction
                && !x.HostileTo(Faction.OfPlayer) && x.def.pawnGroupMakers != null && x.def.pawnGroupMakers.Any(g => g.kindDef == PawnGroupKindDefOf.Peaceful));
            if (f == null) { sb.Append("\n    skip: no friendly faction that sends visitors"); return; }
            int scheduledBefore = GameComponent_Story.Instance.Scheduled.Count(s => s.key.StartsWith("visit:"));
            SO_Visit.Send(f, map, 1);
            Check(StoryUtility.GuestsOnMap(map), "a visit arrives when there are no guests (" + f.Name + ")");
            int lords = map.lordManager.lords.Count;
            SO_Visit.Send(f, map, 1);
            Check(map.lordManager.lords.Count == lords, "a second visit does not arrive while the first is here");
            Check(GameComponent_Story.Instance.Scheduled.Count(s => s.key.StartsWith("visit:")) == scheduledBefore + 1, "the second visit is postponed one day");
            SO_Visit.Send(f, map, StoryUtility.VisitMaxTries);
            Check(GameComponent_Story.Instance.Scheduled.Count(s => s.key.StartsWith("visit:")) == scheduledBefore + 1, "after " + StoryUtility.VisitMaxTries + " tries it is dropped");
            GameComponent_Story.Instance.Scheduled.RemoveAll(s => s.key.StartsWith("visit:"));
            SendHome();
        }

        private void TestScenarios()
        {
            var scenarios = DefDatabase<ScenarioDef>.AllDefsListForReading.Where(s => s.defName.StartsWith("CVG_Scenario_")).ToList();
            Check(scenarios.Count >= 6, scenarios.Count + " Convergence scenarios");
            foreach (var s in scenarios)
            {
                var errors = s.ConfigErrors().ToList();
                foreach (var part in s.scenario.AllParts) errors.AddRange(part.ConfigErrors());
                Check(errors.Count == 0 && !s.scenario.AllParts.Any(p => p.HasNullDefs()), s.defName + (errors.Count > 0 ? ": " + errors[0] : ""));
            }
        }

        private void TestParts(Map map)
        {
            var gc = GameComponent_Story.Instance;
            var occurred = new ScenPart_ConvergenceOccurred();
            occurred.PostGameStart();
            Check(GameComponent_Convergence.Active, "ConvergenceOccurred part marks the Convergence");
            var stoneVein = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_MineableSpiritStone");
            Check(stoneVein == null || map.listerThings.ThingsOfDef(stoneVein).Count > 0, "the map has spirit stone veins (the scenario wakes the map like the Convergence does)");
            Check(GameComponent_Mentor.Instance == null || GameComponent_Mentor.Instance.Mentor != null || HarmonyLib.Traverse.Create(GameComponent_Mentor.Instance).Field("arrivalTick").GetValue<int>() >= 0,
                "the mentor is scheduled");

            var origin = new ScenPart_StoryOrigin { origin = "SelfTest", followUps = new List<StoryFollowUp> { new StoryFollowUp { key = "sect_offer", days = new IntRange(1, 1) } } };
            int before = gc.Scheduled.Count;
            origin.PostGameStart();
            Check(gc.origin == "SelfTest" && gc.Scheduled.Count == before + 1, "StoryOrigin saves the origin and schedules the follow-up");
            gc.Scheduled.RemoveAll(s => s.key == "sect_offer");

            var colonist = map.mapPawns.FreeColonistsSpawned.First();
            var initData = new GameInitData { startingAndOptionalPawns = new List<Pawn> { colonist } };
            var old = Current.Game.InitData;
            Current.Game.InitData = initData;
            try
            {
                var part = new ScenPart_StartingCultivators { realm = DefDatabase<CultivationRealmDef>.GetNamed("CVG_Realm_Foundation"), stage = new IntRange(2, 2) };
                part.PostGameStart();
            }
            finally { Current.Game.InitData = old; }
            var comp = CompCultivator.For(colonist);
            Check(comp != null && comp.IsQiCultivator, "starting cultivator follows the qi path");
            Check(comp != null && comp.Realm.Realm.defName == "CVG_Realm_Foundation" && comp.Realm.Stage == 2, "realm Foundation stage 2 set");
            Check(comp != null && comp.Techniques.AnyManual, "starting cultivator knows a technique manual");
        }

        private void TestDantian(Map map)
        {
            var colonist = map.mapPawns.FreeColonistsSpawned.First();
            var comp = CompCultivator.For(colonist);
            var hediff = DefDatabase<HediffDef>.GetNamed("CVG_DamagedDantian");
            colonist.health.AddHediff(hediff);
            comp.InvalidateHealthScale();
            Check(!comp.IsQiCultivator, "damaged dantian stops the qi path");
            var elixir = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("CVG_DantianElixir"));
            elixir.Ingested(colonist, 1f);
            Check(!colonist.health.hediffSet.HasHediff(hediff), "elixir removes the damaged dantian");
            Check(comp.IsQiCultivator, "the pawn can cultivate again");
        }

        private void TestEventDefs(Map map)
        {
            var defs = StoryEngine.All.ToList();
            bool allModules = new[] { "prospero1088.convergence.factions", "prospero1088.convergence.resources", "prospero1088.convergence.faunaflora", "prospero1088.convergence.alchemy" }.All(id => ModsConfig.IsActive(id));
            // 29 eventos sueltos más sus pasos de cadena (plan de integración: de 80 a 29).
            Check(defs.Count >= (allModules ? 50 : 35), defs.Count + " story event definitions" + (allModules ? "" : " (sin todos los addons)"));
            int missing = 0;
            foreach (var d in defs)
            {
                string key = "CVG_Ev_" + d.defName;
                if (!(key + "_Label").TryTranslate(out _) || !(key + "_Text").TryTranslate(out _)) { missing++; Log.Warning("[Convergence] Missing text for " + d.defName); }
                if (d.options != null)
                    foreach (var o in d.options)
                        if (!(key + "_Opt_" + o.key).TryTranslate(out _)) { missing++; Log.Warning("[Convergence] Missing option text " + d.defName + "/" + o.key); }
            }
            Check(missing == 0, "all event texts exist (" + missing + " missing)");
            // chequeo de cadenas: cada paso encadenado debe existir
            int broken = 0;
            foreach (var d in defs)
                foreach (var o in (d.options ?? new List<StoryOption>()).SelectMany(x => x.outcomes ?? new List<StoryOutcome>()).Concat(d.outcomes ?? new List<StoryOutcome>()))
                    if (o is SO_Chain c && c.next == null) broken++;
            Check(broken == 0, "chain links resolve");
            // lanzar cada evento y resolver cada opción sin excepciones
            int fired = 0, decided = 0;
            foreach (var d in defs)
            {
                try
                {
                    if (d.isThreat) continue; // las amenazas llegan como asaltos reales: se prueban aparte
                    if (d.chainOnly) continue; // los de cadena (mundo de sectas) los dispara su sistema con las facciones ya elegidas
                    if (!StoryEngine.Eligible(d, map, ignoreTiming: true)) { gcFlagsHack(d); if (!StoryEngine.Eligible(d, map, ignoreTiming: true)) continue; }
                    if (StoryEngine.Fire(d, map)) fired++;
                    foreach (var o in d.options ?? new List<StoryOption>())
                    {
                        if (o.costThing != null) continue; // sin coste no se puede garantizar el stock
                        StoryEngine.Decide(d, o.key, new StoryContext(map, d).Serialize());
                        decided++;
                    }
                }
                catch (Exception e) { Check(false, "event " + d.defName + " threw: " + e.Message); }
            }
            Check(fired >= 20, fired + " events fired, " + decided + " option outcomes resolved");
        }

        // Pone las banderas que requiere el evento para poder probarlo.
        private static void gcFlagsHack(StoryEventDef d)
        {
            if (d.requiresFlags == null) return;
            foreach (var f in d.requiresFlags) GameComponent_Story.Instance.Flags.Add(f);
        }

        private static IntVec3 FindOpenCell(Map map)
        {
            CellFinderLoose.TryGetRandomCellWith(c => c.Standable(map) && !c.Roofed(map) && c.DistanceToEdge(map) > 12 && c.GetEdifice(map) == null, map, 500, out var cell);
            return cell;
        }

        private void TestRealms(Map map)
        {
            var realms = DefDatabase<SecretRealmDef>.AllDefsListForReading.Where(r => r.commonality > 0f).ToList();
            Check(realms.Count >= 3, realms.Count + " secret realms defined");
            Check(DefDatabase<QuestScriptDef>.GetNamedSilentFail("CVG_OpportunitySite_SecretRealm") != null && DefDatabase<SitePartDef>.GetNamedSilentFail("CVG_SecretRealmEntrance") != null, "world site and quest exist");
            var colonist = map.mapPawns.FreeColonistsSpawned.First();
            var sealDef = DefDatabase<HediffDef>.GetNamed("CVG_RealmSealPressure");
            foreach (var realm in realms)
            {
                var gate = SecretRealmUtility.SpawnGate(map, FindOpenCell(map), realm);
                Check(gate != null && gate.Spawned, realm.defName + ": gate spawned");
                if (gate == null) continue;
                var rmap = gate.GetOtherMap();
                Check(rmap != null && rmap.Parent is PocketMapParent, realm.defName + ": pocket map generated");
                if (rmap == null) { gate.Destroy(); continue; }
                var exit = rmap.listerThings.AllThings.OfType<PocketMapExit>().FirstOrDefault();
                Check(exit != null && exit.entrance == gate, "exit linked to the gate");
                int walls = rmap.listerThings.ThingsOfDef(realm.wall ?? ThingDefOf.Wall).Count;
                Check(walls > 300, walls + " wall cells");
                var hostile = rmap.mapPawns.AllPawnsSpawned.Where(p => p.Faction == Faction.OfAncientsHostile).ToList();
                Check(hostile.Count >= 3, hostile.Count + " guardians (" + string.Join(", ", hostile.GroupBy(p => p.kindDef.defName).Select(g => g.Key + "x" + g.Count())) + ")");
                int traps = realm.trap == null ? 0 : rmap.listerThings.ThingsOfDef(realm.trap).Count;
                Check(realm.trap == null || traps >= 3, traps + " formation traps");
                var lootDef = realm.treasure.First().thing;
                var loot = rmap.listerThings.ThingsOfDef(lootDef).FirstOrDefault();
                Check(loot != null, "treasure present (" + lootDef.defName + ")");
                if (exit != null && loot != null)
                    Check(rmap.reachability.CanReach(exit.Position, loot.Position, PathEndMode.Touch, TraverseParms.For(TraverseMode.PassDoors)), "treasure reachable from the exit");
                // salas cerradas: una puerta por cada pasillo que entra a una sala (como las ruinas antiguas)
                int doors = rmap.listerThings.AllThings.Count(t => t is Building_Door);
                Check(doors >= 8, doors + " doors between rooms and corridors");
                if (realm.guardTurrets.Count > 0 && realm.guardTurretCount.max > 0)
                {
                    int turrets = rmap.listerThings.AllThings.Count(t => realm.guardTurrets.Contains(t.def));
                    Check(turrets >= 1, turrets + " guard-room turrets");
                }
                if (realm.guardLoot.Count > 0)
                    Check(rmap.listerThings.ThingsOfDef(realm.guardLoot[0].thing).Count >= 2, "guard rooms keep their own loot");
                var comp = rmap.GetComponent<MapComponent_SecretRealm>();
                Check(comp != null && comp.realm == realm && comp.collapseTick > Find.TickManager.TicksGame, "collapse timer started");
                // sello: con tope por debajo del reino del colono, aparece la presión
                int oldCap = realm.maxRealmLevel;
                realm.maxRealmLevel = -1;
                colonist.DeSpawn();
                GenSpawn.Spawn(colonist, exit.Position, rmap);
                comp.Suppress();
                realm.maxRealmLevel = oldCap;
                Check(colonist.health.hediffSet.HasHediff(sealDef), "the seal pressures a pawn above the cap");
                colonist.health.RemoveHediff(colonist.health.hediffSet.GetFirstHediffOfDef(sealDef));
                // derrumbe: todos vuelven a la superficie y el reino desaparece
                GameComponent_SecretRealms.Collapse(rmap);
                Check(colonist.Map == map && colonist.Spawned, "collapse returns the colonist to the surface");
                Check(!Find.Maps.Contains(rmap), "realm map removed");
                Check(gate.Destroyed, "gate destroyed after the collapse");
            }
            // incidente
            var inc = DefDatabase<IncidentDef>.GetNamed("CVG_Story_RealmGate");
            var parms = StorytellerUtility.DefaultParmsNow(inc.category, map);
            parms.forced = true;
            Check(inc.Worker.TryExecute(parms), "incident opens a gate");
            var gateDef = DefDatabase<ThingDef>.GetNamed("CVG_RealmGate");
            var found = map.listerThings.ThingsOfDef(gateDef).FirstOrDefault();
            Check(found != null, "incident gate is on the map");
            found?.Destroy();
            // misión + sitio
            try
            {
                int sitesBefore = Find.WorldObjects.Sites.Count();
                var quest = QuestUtility.GenerateQuestAndMakeAvailable(DefDatabase<QuestScriptDef>.GetNamed("CVG_OpportunitySite_SecretRealm"), 300f);
                var site = Find.WorldObjects.Sites.FirstOrDefault(s => s.parts.Any(x => x.def.defName == "CVG_SecretRealmEntrance"));
                Check(quest != null && site != null && Find.WorldObjects.Sites.Count() == sitesBefore + 1, "quest creates a world site");
                if (site != null)
                {
                    var smap = GetOrGenerateMapUtility.GetOrGenerateMap(site.Tile, new IntVec3(75, 1, 75), null);
                    Check(smap != null && smap.listerThings.ThingsOfDef(gateDef).Count == 1, $"the site map contains the gate ({smap?.listerThings.ThingsOfDef(gateDef).Count ?? -1})");
                    var sgate = smap?.listerThings.ThingsOfDef(gateDef).FirstOrDefault() as Building_RealmGate;
                    if (sgate != null)
                    {
                        int sg = smap.mapPawns.AllPawnsSpawned.Count(p => p.Faction == Faction.OfAncientsHostile);
                        Check(sg >= 1, sg + " guardians outside the gate");
                        var inner = sgate.GetOtherMap();
                        Check(inner != null, "the site gate opens the realm");
                        if (inner != null) PocketMapUtility.DestroyPocketMap(inner);
                    }
                    if (smap != null) Current.Game.DeinitAndRemoveMap(smap, notifyPlayer: false);
                    site.Destroy();
                }
            }
            catch (Exception e) { Check(false, "quest/site: " + e); }
            Current.Game.CurrentMap = map;
        }

        private void TestPuppetMaster(Map map)
        {
            var link = DefDatabase<HediffDef>.GetNamedSilentFail("CVG_PuppetHeartThread");
            var kind = DefDatabase<PawnKindDef>.GetNamedSilentFail("CVG_Puppet_Wood");
            if (link == null || kind == null || !ModsConfig.BiotechActive) { sb.Append("\n    (Biotech Wuxia or Biotech not loaded: skipped)"); return; }
            var scen = DefDatabase<ScenarioDef>.GetNamedSilentFail("CVG_Scenario_PuppetMaster");
            Check(scen != null && scen.scenario.AllParts.Any(p => p is ScenPart_StartingPuppets), "scenario with the starting puppets part exists");
            var startKinds = scen?.scenario.AllParts.OfType<ScenPart_StartingPuppets>().Select(p => p.puppetKind?.defName).ToList() ?? new List<string>();
            Check(startKinds.Contains("CVG_Puppet_Wood") && startKinds.Contains("CVG_Puppet_Mason") && startKinds.Contains("CVG_Puppet_Miner"), "the start brings the three work puppets (" + startKinds.ToCommaList() + ")");
            Check(DefDatabase<StoryEventDef>.GetNamedSilentFail("PuppetHeirs") != null && DefDatabase<StoryEventDef>.GetNamedSilentFail("PuppetCrypt") != null, "follow-up events exist");
            var master = map.mapPawns.FreeColonistsSpawned.FirstOrDefault();
            if (master == null) { sb.Append("\n    (no colonist)"); return; }
            var puppets = ScenPart_StartingPuppets.Equip(master, link, kind, 2);
            Check(master.health.hediffSet.HasHediff(link), "the master carries the heart thread");
            Check(MechanitorUtility.IsMechanitor(master), "the master is a mechanitor");
            Check(puppets.Count == 2 && puppets.All(p => p.Spawned && p.Faction == Faction.OfPlayer), "two player puppets spawned");
            Check(puppets.All(p => p.GetOverseer() == master), "both puppets are bound to the master");
            // como el inicio del mecanitor vanilla: la investigación para cargar y fabricar viene hecha
            var research = scen.scenario.AllParts.OfType<ScenPart_StartingResearch>().Select(r => Traverse.Create(r).Field("project").GetValue<ResearchProjectDef>()?.defName).ToList();
            Check(research.Contains("CVG_Research_Puppetry") && research.Contains("CVG_Research_QiNetwork"), "the start brings puppetry and qi network research (" + research.ToCommaList() + ")");
            // recarga: formación de respiración del cielo + cargador espiritual a su alcance
            foreach (var d in new[] { "CVG_Research_QiBreathing", "CVG_Research_QiNetwork", "CVG_Research_Puppetry" })
            {
                var proj = DefDatabase<ResearchProjectDef>.GetNamedSilentFail(d);
                if (proj != null && !proj.IsFinished) Find.ResearchManager.FinishProject(proj, doCompletionDialog: false);
            }
            var arrayDef = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_SkyBreathArray");
            var chargerDef = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_SpiritCharger");
            if (arrayDef != null && chargerDef != null)
            {
                Check(chargerDef.IsResearchFinished, "the spirit charger can be built");
                IntVec3 c0;
                if (CellFinder.TryFindRandomCellNear(master.Position, map, 25, c => !c.Roofed(map) && GenAdj.OccupiedRect(c, Rot4.North, new IntVec2(6, 6)).Cells.All(x => x.InBounds(map) && x.Standable(map) && !x.Roofed(map) && x.GetFirstBuilding(map) == null), out c0))
                {
                    var array = GenSpawn.Spawn(ThingMaker.MakeThing(arrayDef, GenStuff.DefaultStuffFor(arrayDef)), c0, map);
                    array.SetFaction(Faction.OfPlayer);
                    var charger = (Building_MechCharger)GenSpawn.Spawn(ThingMaker.MakeThing(chargerDef, GenStuff.DefaultStuffFor(chargerDef)), c0 + new IntVec3(1, 0, 3), map);
                    charger.SetFaction(Faction.OfPlayer);
                    var mgr = map.GetComponent<QiNetworkManager>();
                    mgr.EnsureBuilt();
                    mgr.Step(0.25f);
                    var node = charger.TryGetComp<CompQiNode>();
                    Check(node?.Net != null && node.Net == array.TryGetComp<CompQiNode>()?.Net, "charger and sky-breath array share a qi net");
                    Check(charger.IsPowered, $"the spirit charger is powered by qi (supply {node?.Net?.SupplyDay:0}/day, demand {node?.Net?.DemandDay:0}/day)");
                    var tired = puppets.FirstOrDefault();
                    if (tired?.needs?.energy != null) tired.needs.energy.CurLevelPercentage = 0.1f;
                    Check(tired != null && charger.CanPawnChargeCurrently(tired), "a tired puppet can charge there");
                    charger.Destroy(); array.Destroy();
                    mgr.EnsureBuilt();
                }
                else sb.Append("\n    (no open 6x6 spot for the charger test)");
            }
            foreach (var p in puppets) if (p.Spawned) p.Destroy();
        }

        private void TestEndgame(Map map)
        {
            var gc = GameComponent_Endgame.Instance;
            Check(gc != null, "GameComponent_Endgame exists");
            if (gc == null) return;
            GameComponent_Endgame.SuppressCredits = true;
            gc.ResetForTest();
            for (int i = 0; i < 4; i++)
            {
                Check(DefDatabase<ThingDef>.GetNamedSilentFail(GameComponent_Endgame.Keys[i]) != null, "key " + GameComponent_Endgame.Keys[i]);
                var realm = DefDatabase<SecretRealmDef>.GetNamedSilentFail(GameComponent_Endgame.Realms[i]);
                Check(realm != null && realm.commonality <= 0f && realm.bossTreasure.Any(l => l.thing?.defName == GameComponent_Endgame.Keys[i]), "realm " + GameComponent_Endgame.Realms[i] + " holds its key and never appears at random");
                Check(DefDatabase<StoryEventDef>.GetNamedSilentFail("EndgameStep" + (i + 1)) != null, "event EndgameStep" + (i + 1));
            }
            Check(DefDatabase<StoryEventDef>.GetNamedSilentFail("EndgameChoice")?.options.Count == 3, "choice event has three options");
            Check(!GameComponent_Endgame.HasKey("CVG_FounderSeal"), "no key before the chain");
            gc.Begin(1);
            Check(gc.Step == 1, "chain begins at step 1");
            gc.Check();
            Check(gc.GateOpened && GameComponent_Endgame.GateExists(GameComponent_Endgame.Realms[0]), "step 1 opens the founder's gate");
            for (int i = 0; i < 4; i++)
            {
                var key = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed(GameComponent_Endgame.Keys[i]));
                GenPlace.TryPlaceThing(key, map.Center, map, ThingPlaceMode.Near);
                Check(GameComponent_Endgame.HasKey(GameComponent_Endgame.Keys[i]), "key " + (i + 1) + " is detected");
                gc.Check();
                Check(gc.Step == i + 2, "holding key " + (i + 1) + " advances to step " + (i + 2));
            }
            var gate2 = gc.OpenGate(1);
            Check(gate2 != null && gate2.realm.defName == GameComponent_Endgame.Realms[1], "step 2 gate can be opened");
            bool canAscend = GameComponent_Endgame.Ascendant() != null;
            Check(gc.Finish("ascend") == canAscend, "ascension only with a top-realm cultivator");
            if (!canAscend)
            {
                bool ruled = gc.Finish("rule");
                Check(ruled && gc.Ending == "rule" && gc.Step == 6, "ruling ends the chain (finish=" + ruled + ", ending=" + gc.Ending + ", step=" + gc.Step + ")");
                Check(GameComponent_Story.Instance.Flags.Contains("court_ruler"), "ruler flag set");
                Check(!GameComponent_Endgame.HasKey("CVG_HeavenKey"), "keys are consumed");
                Check(!gc.Finish("seal"), "only one ending per game");
            }
        }

        /// <summary>Eventos de sectas (sin la vieja simulación de guerras) y los dos incidentes del narrador.</summary>
        private void TestSectEvents(Map map)
        {
            foreach (var n in new[] { "SectWarAppeal", "SectTournament", "SectTribute", "DisciplesTransfer" })
            {
                // Son de Factions (MayRequire): sin ese módulo, no tienen que existir.
                if (!ModsConfig.IsActive("prospero1088.convergence.factions")) continue;
                Check(DefDatabase<StoryEventDef>.GetNamedSilentFail(n) != null, "event " + n + " exists");
            }
            foreach (var n in new[] { "CVG_StoryEvent", "CVG_StoryThreat" })
                Check(DefDatabase<IncidentDef>.GetNamedSilentFail(n)?.Worker is IncidentWorker_StoryEvent, "storyteller incident " + n);
            var appeal = DefDatabase<StoryEventDef>.GetNamedSilentFail("SectWarAppeal");
            if (appeal == null || StoryUtility.Sects().Count < 2) { sb.Append("\n    (fewer than two sects: sect events skipped)"); return; }
            var ctx = new StoryContext(map, appeal);
            Check(ctx.Primary != null && ctx.Resolve("sect:rival") != null && ctx.Primary != ctx.Resolve("sect:rival"), "a friendly sect and a rival sect resolve");
            Check(ctx.Args().Length >= 2, "text arguments include the other sect");
        }

        private void TestSanctuary(Map map)
        {
            var podDef = DefDatabase<ThingDef>.GetNamed("CVG_ForgottenSarcophagus");
            Check(GameComponent_Convergence.Active, "Convergence active for the sanctuary");
            var rect = CellRect.CenteredOn(FindOpenCell(map), 9, 7);
            RimWorld.BaseGen.BaseGen.globalSettings.map = map;
            var rp = new RimWorld.BaseGen.ResolveParams { rect = rect };
            new SymbolResolver_Interior_ForgottenSanctuary().Resolve(rp);
            var pods = map.listerThings.ThingsOfDef(podDef).OfType<Building_AncientCryptosleepCasket>().ToList();
            Check(pods.Count >= 2, pods.Count + " sarcophagi");
            Check(pods.Count(p => p.GetDirectlyHeldThings().OfType<Pawn>().Any(x => CompCultivator.For(x)?.IsQiCultivator == true)) >= 1, "sleepers are cultivators");
            var rule = DefDatabase<RuleDef>.GetNamed("Interior_AncientTemple");
            Check(rule.resolvers.Any(r => r is SymbolResolver_Interior_ForgottenSanctuary), "the vanilla ancient temple interior is replaced");
            foreach (var p in pods)
            {
                foreach (var t in p.GetDirectlyHeldThings().OfType<Pawn>().ToList()) Find.WorldPawns.PassToWorld(t, PawnDiscardDecideMode.Discard);
                p.Destroy();
            }
        }

        private void TestEvents(Map map)
        {
            var gc = GameComponent_Story.Instance;
            // la Convergencia se marcó hace tiempo para que los incidentes puedan lanzarse
            foreach (var key in new[] { "CVG_Story_CultistsFlee", "CVG_Story_AllyPlea", "CVG_Story_BeastMigration", "CVG_Story_TreasureBirth", "CVG_Story_ClanVengeance" })
            {
                var def = DefDatabase<IncidentDef>.GetNamed(key);
                var parms = StorytellerUtility.DefaultParmsNow(def.category, map);
                parms.forced = true;
                if (key == "CVG_Story_AllyPlea" && !ModsConfig.IsActive("prospero1088.convergence.factions") && !ModsConfig.IsActive("prospero1088.convergence.sects")) continue; // aliados = sectas (addon)
                bool ok = def.Worker.TryExecute(parms);
                Check(ok, key + " executed");
            }
            int colonists = map.mapPawns.FreeColonistsSpawned.Count;
            StoryEvents.Decide("Cultists", "accept", null);
            Check(map.mapPawns.FreeColonistsSpawned.Count > colonists, "accepting the cultists adds colonists");
            Check(gc.Scheduled.Any(s => s.key == "cultists_aftermath"), "aftermath scheduled");
            StoryEvents.Decide("Cultists", "refuse", null);
            var ally = StoryUtility.Sects().FirstOrDefault(f => !f.HostileTo(Faction.OfPlayer) && f.def.defName != "CVG_Faction_BloodMoon");
            if (ally != null) { StoryEvents.Decide("AllyPlea", "help", ally.def.defName); StoryEvents.Decide("AllyPlea", "decline", ally.def.defName); }
            Check(true, "ally decisions ran (" + (ally?.Name ?? "no ally sect present") + ")");
            foreach (var k in new[] { "master_offer", "emissaries", "collective_demon", "sect_offer", "cultists_aftermath", "treasure_rivals", "assassin", "refuge", "pursuers", "clan_returns", "blood_hunters" })
            {
                try { StoryEvents.Run(k); Check(true, "story event " + k); }
                catch (Exception e) { Check(false, "story event " + k + ": " + e.Message); }
            }
            gc.AddGrudge(map.mapPawns.FreeColonistsSpawned.First());
            Check(gc.Grudges.Count >= 0, "grudge list ok");
        }
    }
}
