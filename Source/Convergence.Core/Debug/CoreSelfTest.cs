using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace Convergence
{
    /// <summary>
    /// Autotest de integración del Core (M2–M6): Convergencia, raíz y cultivo, avance y tribulación, técnicas y supresión
    /// de reino, camino del cuerpo con bestias y sopa, el Maestro y un humo de todos los ThingDef de los mods Convergence.
    /// Solo corre con <c>-cvgselftest</c> (lo llama <see cref="BloodlineSelfTest.CheckCommandLine"/>) o desde debug; está
    /// pensado para una partida de prueba (-quicktest): marca la Convergencia y trae al Maestro.
    /// </summary>
    public static class CoreSelfTest
    {
        private static StringBuilder sb;
        private static int fails;

        public static void Run()
        {
            sb = new StringBuilder("[Convergence] Core self-test");
            fails = 0;
            var map = Find.CurrentMap;
            if (map == null)
            {
                Log.Warning("[Convergence] Core self-test: no map, skipped.");
                return;
            }
            Section("convergence", () => TestConvergence(map));
            Section("dao research", () => TestDaoResearch(map));
            Pawn qi = null;
            Section("qi cultivation", () => qi = TestQiCultivation(map));
            Section("breakthrough", () => TestBreakthrough(map, qi));
            Section("environment resistance", () => TestEnvironment(map, qi));
            Section("techniques", () => TestTechniques(map, qi));
            Section("technique paths and combat sets", () => TestTechniquePaths(map));
            Section("thunder root", () => TestThunder(map));
            Section("daily consumables", () => TestDailyConsumables(map));
            Section("wuxia art and benches", () => TestWuxiaArt(map));
            Section("cultivation life (thoughts, records, tales, alerts, spots)", () => TestCultivationLife(map));
            Section("traits keep their word", () => TestTraitsKeepWord(map));
            Section("wuxia weapons and accessories", () => TestWuxiaGear(map));
            Section("realm suppression", () => TestSuppression(map, qi));
            Section("celestial calendar", () => TestCalendar(map, qi));
            Section("inner demon and recreation", () => TestInnerDemonAndJoy(map));
            Section("trait progression", () => TestTraitProgression(map));
            Section("biotech wuxia", () => TestBiotechWuxia(map));
            Section("jiangshi", () => TestJiangshi(map));
            Section("parallel economy", () => TestEconomy(map));
            Section("cultivation difficulty", () => TestDifficulty(map));
            Section("enemy AI", () => TestEnemyAI(map));
            Section("longevity", () => TestLongevity(map));
            Section("pre-cultivation", () => TestPreCultivation(map));
            Section("tribulation", () => TestTribulation(map, qi));
            Section("body path", () => TestBodyPath(map));
            Section("spirit beasts", () => TestSpiritBeasts(map));
            Section("mentor", () => TestMentor(map));
            Section("things smoke", () => TestSpawnAllThings(map));
            Section("pawn kinds smoke", () => TestPawnKinds(map));
            Section("factions (sects)", TestFactions);
            Section("recipes smoke", TestRecipes);
            Section("ingestibles smoke", () => TestIngestibles(map));
            Section("translations", TestTranslations);
            Section("guest group limit", () => TestGuestLimit(map));
            Section("sect framework (S1)", () => TestSectFramework(map));
            Section("made by and pill effects", () => TestCrafter(map));
            Section("work rooms, furnace fire and pill failure", () => TestWorkRooms(map));
            sb.Append(fails == 0 ? "\n  ALL OK" : "\n  FAILURES: " + fails);
            // En la partida de prueba, acelerar para que corran ticks (tribulación, bestias, Maestro) y salgan sus errores.
            if (GenCommandLine.CommandLineArgPassed("cvgselftest")) Find.TickManager.CurTimeSpeed = TimeSpeed.Superfast;
            if (fails == 0) Log.Message(sb.ToString());
            else Log.Error(sb.ToString());
        }

        [LudeonTK.DebugAction("Convergence", "Convergence: Run core self-test", allowedGameStates = LudeonTK.AllowedGameStates.PlayingOnMap)]
        private static void RunFromDebug() => Run();

        private static void Section(string name, Action action)
        {
            sb.Append("\n- " + name);
            try { action(); }
            catch (Exception e) { Fail("exception: " + e); }
        }

        private static void Check(bool ok, string what)
        {
            sb.Append("\n    " + (ok ? "ok   " : "FAIL ") + what);
            if (!ok) fails++;
        }

        private static void Fail(string what) => Check(false, what);

        /// <summary>"Hecho por" y la ficha de las píldoras: creador inventado, creador colono, qi por calidad, efectos y receta.</summary>
        private static void TestCrafter(Map map)
        {
            var def = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_NineTurnsPill") ?? DefDatabase<ThingDef>.GetNamed("CVG_FoundationPill");
            var pill = ThingMaker.MakeThing(def);
            pill.TryGetComp<CompQuality>()?.SetQuality(QualityCategory.Excellent, ArtGenerationContext.Outsider);
            var comp = pill.TryGetComp<CompCrafter>();
            Check(comp != null, def.defName + " has a maker");
            if (comp == null) return;
            string lore = comp.CrafterLabel;
            Check(!lore.NullOrEmpty() && lore == comp.CrafterLabel, "from outside the colony: " + lore + " (stable)");
            foreach (var row in comp.SpecialDisplayStats())
                sb.Append("\n      " + row.LabelCap + ": " + row.ValueString + " | " + row.GetExplanationText(StatRequest.For(pill)).Replace("\n", " / "));
            var colonist = map.mapPawns.FreeColonists.FirstOrDefault();
            if (colonist != null)
            {
                comp.Notify_RecipeProduced(colonist);
                Check(comp.CrafterLabel == colonist.Name.ToStringShort, "made in the colony: " + comp.CrafterLabel);
            }
            // Con solo el Core no hay armas de Convergence (las traen Alchemy y otros módulos).
            if (DefDatabase<ThingDef>.AllDefsListForReading.Any(d => d.IsWeapon && d.modContentPack?.PackageId?.StartsWith("prospero1088.convergence") == true))
            {
                var weapon = DefDatabase<ThingDef>.AllDefsListForReading.FirstOrDefault(d => d.IsWeapon && d.HasComp(typeof(CompCrafter)));
                Check(weapon != null, "weapons too: " + weapon?.defName);
            }
            pill.Destroy();
        }

        /// <summary>
        /// Salas de trabajo: tabla de las píldoras (30/45/20/5), una sala de alquimia de verdad (paredes, horno, armario de píldoras),
        /// el fuego de piedra espiritual y 400 píldoras fabricadas en ella (fallos = píldoras de desecho).
        /// </summary>
        private static void TestWorkRooms(Map map)
        {
            bool Near(float a, float b) => Math.Abs(a - b) < 0.011f;
            var w0 = Patch_QualityUtility_PillBands.Weights(0f);
            Check(Near(w0[0] + w0[1] + w0[2], 0.30f) && Near(w0[3] + w0[4], 0.45f) && Near(w0[5] + w0[6], 0.20f) && Near(w0[7], 0.05f),
                "base table: 30% fail or bad, 45% normal, 20% excellent, 5% celestial");
            var w1 = Patch_QualityUtility_PillBands.Weights(1f);
            Check(Near(w1[0] + w1[1] + w1[2], 0.05f) && Near(w1[3] + w1[4], 0.30f) && Near(w1[5] + w1[6], 0.40f) && Near(w1[7], 0.25f),
                "grandmaster with everything in favor: 5 / 30 / 40 / 25");
            Check(Near(Patch_QualityUtility_PillBands.Weights(-1f).Sum(), 1f), "novice table adds up to 100%");
            Check(WorkRoomUtility.AddLevels(QualityCategory.Normal, 1f) == QualityCategory.Good
                  && WorkRoomUtility.AddLevels(QualityCategory.Normal, -1f) == QualityCategory.Poor, "room bonus raises and lowers quality levels");

            Check(DefDatabase<ThingDef>.GetNamedSilentFail("CVG_WastePill")?.modContentPack?.PackageId == "prospero1088.convergence.core", "waste pill exists (in the Core)");
            var furnaceDef = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_AlchemyFurnace");
            if (furnaceDef == null)
            {
                sb.Append("\n    (Convergence: Alchemy not loaded: room test skipped)");
                return;
            }
            Check(furnaceDef.building?.workTableRoomRole?.defName == "CVG_AlchemyRoom", "the alchemy furnace makes an alchemy room");

            // Un cuarto de 9x9 de paredes de madera, con el horno en el medio.
            IntVec3 center = IntVec3.Invalid;
            // Cualquier lugar seco sin colonos ni edificios del jugador: la roca y las plantas se despejan (el mapa de prueba es al azar).
            foreach (var c in GenRadial.RadialCellsAround(map.Center, 56f, true))
            {
                var r = CellRect.CenteredOn(c, 4);
                if (r.InBounds(map) && r.Cells.All(x => !x.GetTerrain(map).IsWater && x.GetTerrain(map).affordances.Contains(TerrainAffordanceDefOf.Heavy)
                                                         && !x.GetThingList(map).Any(t => t is Pawn || t.Faction == Faction.OfPlayer)))
                {
                    center = c;
                    break;
                }
            }
            if (!center.IsValid)
            {
                Fail("no room for a test alchemy room");
                return;
            }
            var rect = CellRect.CenteredOn(center, 4);
            var spawned = new List<Thing>();
            foreach (var c in rect.Cells)
            {
                foreach (var t in c.GetThingList(map).ToList())
                    if (!(t is Pawn)) t.Destroy();
                map.fogGrid.Unfog(c);
                map.roofGrid.SetRoof(c, null);
            }
            foreach (var c in rect.EdgeCells)
            {
                var wall = ThingMaker.MakeThing(ThingDefOf.Wall, ThingDefOf.WoodLog);
                wall.SetFaction(Faction.OfPlayer);
                spawned.Add(GenSpawn.Spawn(wall, c, map));
            }
            var furnace = (Building_WorkTable)GenSpawn.Spawn(ThingMaker.MakeThing(furnaceDef, ThingDefOf.Steel), center, map);
            furnace.SetFaction(Faction.OfPlayer);
            spawned.Add(furnace);
            try
            {
                map.regionAndRoomUpdater.TryRebuildDirtyRegionsAndRooms();
                var room = furnace.GetRoom();
                Check(room?.Role?.defName == "CVG_AlchemyRoom", "the room is an alchemy room: " + room?.Role?.label);
                var stat = WorkRoomUtility.WorkRoomQuality;
                float s0 = furnace.GetStatValue(stat);
                sb.Append("\n      room alone: " + s0.ToString("0.00") + " | " + stat.Worker.GetExplanationFull(StatRequest.For(furnace), ToStringNumberSense.Offset, s0).Replace("\n", " / "));

                // Armario de píldoras vinculado.
                var cabinetDef = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_PillCabinet");
                if (cabinetDef != null)
                {
                    var inner = rect.ContractedBy(1);
                    foreach (var c in inner.Cells)
                    {
                        var occ = GenAdj.OccupiedRect(c, Rot4.South, cabinetDef.size);
                        if (!occ.Cells.All(x => inner.Contains(x) && x.GetEdifice(map) == null && x != furnace.InteractionCell)) continue;
                        var cabinet = GenSpawn.Spawn(ThingMaker.MakeThing(cabinetDef, cabinetDef.MadeFromStuff ? GenStuff.DefaultStuffFor(cabinetDef) : null), c, map, Rot4.South);
                        cabinet.SetFaction(Faction.OfPlayer);
                        spawned.Add(cabinet);
                        bool linked = furnace.GetComp<CompAffectedByFacilities>()?.LinkedFacilitiesListForReading.Contains(cabinet) == true;
                        float s1 = furnace.GetStatValue(stat);
                        Check(linked && Near(s1 - s0, 0.15f), "pill cabinet linked to the furnace: +" + (s1 - s0).ToString("0.00"));
                        s0 = s1;
                        break;
                    }
                }
                else sb.Append("\n    (Convergence: Furniture not loaded: no pill cabinet)");

                // Fuego de piedra espiritual.
                var fire = furnace.GetComp<CompSpiritFire>();
                var refuel = furnace.GetComp<CompRefuelable>();
                var stone = DefDatabase<ThingDef>.GetNamed("CVG_SpiritStone");
                Check(fire != null && refuel != null, "the furnace has a spirit fire");
                if (fire != null && refuel != null)
                {
                    Check(refuel.Props.fuelFilter.Allows(ThingDefOf.WoodLog) && !refuel.Props.fuelFilter.Allows(stone), "by default it burns only wood");
                    fire.SetFuel(stone);
                    Check(refuel.Props.fuelFilter.Allows(stone) && !refuel.Props.fuelFilter.Allows(ThingDefOf.WoodLog), "spirit stone chosen: it burns only spirit stones");
                    var fuel = ThingMaker.MakeThing(stone);
                    fuel.stackCount = 60;
                    refuel.Refuel(new List<Thing> { fuel });
                    float s2 = furnace.GetStatValue(stat);
                    Check(Near(fire.FireBonus, 0.25f) && Near(s2 - s0, 0.25f), "spirit stone fire: +" + fire.FireBonus.ToString("0.00") + " levels");
                    sb.Append("\n      " + fire.CompInspectStringExtra());
                }

                // 400 píldoras de templado hechas en la sala, por un colono de habilidad 10.
                var pillDef = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_TemperingPill");
                var recipe = DefDatabase<RecipeDef>.AllDefsListForReading.FirstOrDefault(r => r.ProducedThingDef == pillDef);
                var pawn = map.mapPawns.FreeColonists.FirstOrDefault();
                if (recipe == null || pawn == null)
                {
                    Fail("no tempering pill recipe or colonist");
                    return;
                }
                var bill = (Bill_Production)recipe.MakeNewBill();
                furnace.BillStack.AddBill(bill);
                var job = JobMaker.MakeJob(JobDefOf.DoBill, furnace);
                job.bill = bill;
                var oldJob = pawn.jobs.curJob;
                var skill = pawn.skills.GetSkill(recipe.workSkill ?? SkillDefOf.Intellectual);
                int oldLevel = skill.Level;
                var level = furnace.GetComp<CompWorkbenchLevel>();
                pawn.jobs.curJob = job;
                // Fabrica n píldoras como al terminar la orden (GenRecipe, el mismo camino que el trabajo real) y cuenta qué sale.
                int[] Craft(int skillLevel, int n, out float power)
                {
                    skill.Level = skillLevel;
                    power = Patch_QualityUtility_PillBands.Power(pawn, recipe.workSkill, false);
                    var counts = new int[8];
                    for (int i = 0; i < n; i++)
                        foreach (var product in GenRecipe.MakeRecipeProducts(recipe, pawn, new List<Thing>(), null, furnace).ToList())
                        {
                            if (product.def.defName == "CVG_WastePill") counts[0]++;
                            else if (product.TryGetQuality(out var q)) counts[(int)q + 1]++;
                            product.Destroy();
                        }
                    return counts;
                }
                string Show(int[] c) => "waste " + c[0] + ", bad " + (c[1] + c[2]) + ", normal " + (c[3] + c[4]) + ", excellent " + (c[5] + c[6]) + ", celestial " + c[7];
                try
                {
                    // 1. El colono de la sala recién hecha (habilidad 10, armario y fuego de piedra).
                    var mid = Craft(10, 400, out float pMid);
                    sb.Append("\n      skill 10, new room, cabinet, spirit-stone fire (power " + pMid.ToString("0.00") + "): " + Show(mid));
                    Check(mid.Sum() == 400, "400 pills came out");
                    Check(mid[0] > 10 && mid[0] < 100, "some fail and turn into waste pills: " + mid[0]);
                    Check(mid[7] > 0, "some come out celestial (legendary): " + mid[7]);

                    // 2. Novato (habilidad 3) en esa misma sala, con el fuego apagado.
                    var refuelComp = furnace.GetComp<CompRefuelable>();
                    float fuelLeft = refuelComp.Fuel;
                    refuelComp.ConsumeFuel(fuelLeft);
                    var novice = Craft(3, 1000, out float pNov);
                    sb.Append("\n      [1000] skill 3, no fire (power " + pNov.ToString("0.00") + "): " + Show(novice));
                    Check(novice[0] / 1000f > mid[0] / 400f, "a novice with a cold furnace fails more (" + (novice[0] / 10f) + "% vs " + (mid[0] / 4f) + "%)");

                    // 3. El mismo novato con horno nivel 3 y el mejor fuego: la sala compensa la habilidad.
                    refuelComp.Refuel(fuelLeft);
                    level?.SetLevel(3);
                    var crystal = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_FireCrystal");
                    if (crystal != null && fire != null)
                    {
                        refuelComp.ConsumeFuel(refuelComp.Fuel);
                        fire.SetFuel(crystal);
                        var c = ThingMaker.MakeThing(crystal);
                        c.stackCount = 60;
                        refuelComp.Refuel(new List<Thing> { c });
                    }
                    var helped = Craft(3, 1000, out float pHelp);
                    sb.Append("\n      [1000] skill 3, level 3 furnace, " + (fire?.Chosen?.label ?? "-") + " fire (power " + pHelp.ToString("0.00") + "): " + Show(helped));
                    Check(helped[0] < novice[0], "a good furnace and fire make up for low skill (" + helped[0] + " vs " + novice[0] + " failed)");

                    // 4. Gran maestro (habilidad 20) con todo a favor.
                    var master = Craft(20, 400, out float pMaster);
                    sb.Append("\n      skill 20, everything in favor (power " + pMaster.ToString("0.00") + "): " + Show(master));
                    Check(master[0] < mid[0] && master[7] > mid[7], "a grandmaster rarely fails and makes more celestial pills");

                    // 5. Inspiración creativa: suma poder y se gasta en la primera píldora.
                    var insp = InspirationDefOf.Inspired_Creativity;
                    if (pawn.mindState.inspirationHandler.TryStartInspiration(insp, null, false))
                    {
                        Craft(10, 1, out _);
                        Check(pawn.InspirationDef != insp, "creative inspiration is spent on the first pill");
                    }

                    // "Hecho por" del colono en la píldora.
                    skill.Level = 10;
                    var one = GenRecipe.MakeRecipeProducts(recipe, pawn, new List<Thing>(), null, furnace).First();
                    var maker = one.TryGetComp<CompCrafter>();
                    if (one.def == pillDef)
                        Check(maker?.CrafterLabel == pawn.Name.ToStringShort, "the pill says it was made by " + maker?.CrafterLabel);
                    one.Destroy();
                }
                finally
                {
                    pawn.jobs.curJob = oldJob;
                    skill.Level = oldLevel;
                    level?.SetLevel(1);
                    furnace.BillStack.Delete(bill);
                }
            }
            finally
            {
                foreach (var t in spawned)
                    if (!t.Destroyed) t.Destroy();
            }
        }

        private static Pawn Colonist(Map map, bool root)
        {
            var pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayer,
                forceGenerateNewPawn: true, mustBeCapableOfViolence: true, developmentalStages: DevelopmentalStage.Adult,
                forcedXenotype: ModsConfig.BiotechActive ? XenotypeDefOf.Baseliner : null));
            var comp = CompCultivator.For(pawn);
            comp.Sync();
            if (root && !comp.HasRoot)
                CultivationGenerator.GiveRoot(pawn, CultivationGenerator.RandomGrade(), new List<QiElementDef> { DefDatabase<QiElementDef>.GetNamed("CVG_Element_Fire") });
            if (!root && comp.HasRoot)
            {
                foreach (var t in pawn.story.traits.allTraits.Where(t => t.def == CVG_DefOf.CVG_SpiritRoot).ToList()) pawn.story.traits.RemoveTrait(t);
                if (pawn.genes != null)
                    foreach (var g in pawn.genes.GenesListForReading.Where(g => g.def.HasModExtension<DefModExtension_SpiritRoot>()).ToList())
                        pawn.genes.RemoveGene(g);
            }
            comp.Sync();
            GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(map.Center, map, 8), map);
            comp.SyncRealm();
            return pawn;
        }

        private static void TestConvergence(Map map)
        {
            // Cultivo previo latente: forzar uno en un colono antes de la Convergencia y ver que despierta con ella.
            Pawn latentPawn = null;
            var preDef = DefDatabase<PreCultivationDef>.GetNamedSilentFail("CVG_PreCultivation_Qi");
            if (!GameComponent_Convergence.Active && preDef != null)
            {
                latentPawn = Colonist(map, root: true);
                var lc = CompCultivator.For(latentPawn);
                PreCultivation.Apply(latentPawn, lc, preDef, new PreCultivationOption { realmOrder = 2, stage = new IntRange(3, 3) });
                var lt = lc.TrackerFor(CVG_DefOf.CVG_Path_Qi);
                Check(lt.Started && !latentPawn.health.hediffSet.HasHediff(lt.Realm.hediff) && lc.LatentTrackers.Any(),
                    $"before the Convergence: {lt.Realm.defName} stage {lt.Stage} latent (no hediff)");
            }
            if (!GameComponent_Convergence.Active) GameComponent_Convergence.Instance.TryFire(map);
            Check(GameComponent_Convergence.Active, "Convergence active");
            if (latentPawn != null)
            {
                var lc = CompCultivator.For(latentPawn);
                lc.SyncRealm();
                var lt = lc.TrackerFor(CVG_DefOf.CVG_Path_Qi);
                Check(lt.Realm.order == 2 && latentPawn.health.hediffSet.HasHediff(lt.Realm.hediff) && !lc.LatentTrackers.Any(),
                    "after the Convergence the latent realm awakens with its hediff");
                latentPawn.Destroy();
            }
        }

        /// <summary>Bloqueos por investigación y comprensión; al final completa los proyectos para el resto del test.</summary>
        private static void TestDaoResearch(Map map)
        {
            var projects = DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where(DaoComprehension.IsDaoProject).ToList();
            Check(projects.Count >= 6, projects.Count + " Dao projects; knowledge category: " + DaoComprehension.UsesKnowledge);
            var foundation = DefDatabase<ResearchProjectDef>.GetNamed("CVG_Research_Foundation");
            if (DaoComprehension.UsesKnowledge)
                Check(foundation.knowledgeCategory == DaoComprehension.Category && foundation.baseCost == 0f, "projects converted to knowledge");
            var pawn = Colonist(map, root: true);
            var comp = CompCultivator.For(pawn);
            if (!foundation.IsFinished)
            {
                var t = comp.Realm;
                while (!t.Bottleneck) t.AddProgress(t.Realm.progressPerStage);
                t.Resource.Fill();
                Check(!BreakthroughUtility.CanAttempt(comp, t, out string reason), "Foundation blocked by research: " + reason);
                var manual = DefDatabase<TechniqueManualDef>.AllDefsListForReading.First();
                Check(!manual.CanLearn(pawn).Accepted, "manual blocked by research: " + manual.CanLearn(pawn).Reason);
            }
            var first = projects.FirstOrDefault(p => p.CanStartNow);
            if (first != null)
            {
                float before = DaoComprehension.UsesKnowledge ? Find.ResearchManager.GetKnowledge(first) : Find.ResearchManager.GetProgress(first);
                if (!DaoComprehension.UsesKnowledge) Find.ResearchManager.SetCurrentProject(first);
                DaoComprehension.Gain(pawn, 0.5f, DaoSource.Studying);
                var current = DaoComprehension.UsesKnowledge ? Find.ResearchManager.GetProject(DaoComprehension.Category) : Find.ResearchManager.GetProject();
                float after = current == null ? before
                    : DaoComprehension.UsesKnowledge ? Find.ResearchManager.GetKnowledge(current) : Find.ResearchManager.GetProgress(current);
                Check(after > before || first.IsFinished, $"comprehension gain on {current?.defName}: {before:0.##} -> {after:0.##}");
            }
            else Fail("no Dao project can start (hidden?)");
            foreach (var p in projects)
                if (!p.IsFinished)
                {
                    Find.ResearchManager.FinishProject(p, doCompletionDialog: false, null, doCompletionLetter: false);
                    GameComponent_Dao.Instance?.MarkAnnounced(p);
                }
            Check(projects.All(p => p.IsFinished), "all Dao projects finished for the rest of the test");
            pawn.Destroy();
        }

        private static Pawn TestQiCultivation(Map map)
        {
            // Un colono que empieza de cero: el cultivo previo (DISENO §4.1d) puede generarlo ya avanzado.
            var pawn = Colonist(map, root: true);
            for (int i = 0; i < 20 && CompCultivator.For(pawn).Realm.PreCultivated; i++)
            {
                pawn.Destroy();
                pawn = Colonist(map, root: true);
            }
            var comp = CompCultivator.For(pawn);
            Check(comp.HasRoot && comp.IsQiCultivator, "rooted colonist cultivates qi");
            Check(comp.Realm.Started && pawn.health.hediffSet.HasHediff(comp.Realm.Realm.hediff), "realm started with hediff");
            float added = comp.Realm.AddProgress(comp.Realm.Realm.progressPerStage);
            Check(added > 0 && comp.Realm.Stage == 2, "progress fills a stage (stage " + comp.Realm.Stage + ")");
            Check(!comp.IsBodyCultivator, "rooted colonist does not follow the body path");
            return pawn;
        }

        /// <summary>Resistencia al entorno: el colono de prueba (raíz de fuego) acaba de subir a Fundación.</summary>
        private static void TestEnvironment(Map map, Pawn pawn)
        {
            var env = DefDatabase<StatDef>.GetNamed("CVG_EnvironmentResistance");
            var plain = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayer, forceGenerateNewPawn: true));
            float e = pawn.GetStatValue(env);
            Check(e >= 0.14f && e <= 0.26f, "Foundation gives environment resistance " + e.ToStringPercent());
            Check(plain.GetStatValue(env) == 0f || CompCultivator.For(plain)?.RealmLevel > 0, "a mortal has no environment resistance");
            float maxT = pawn.GetStatValue(StatDefOf.ComfyTemperatureMax), maxTPlain = plain.GetStatValue(StatDefOf.ComfyTemperatureMax);
            float minT = pawn.GetStatValue(StatDefOf.ComfyTemperatureMin), minTPlain = plain.GetStatValue(StatDefOf.ComfyTemperatureMin);
            bool fire = CompCultivator.For(pawn)?.Root?.elements?.Any(x => x.defName == "CVG_Element_Fire") == true;
            var hotPart = StatDefOf.ComfyTemperatureMax.parts.OfType<StatPart_EnvironmentResistance>().FirstOrDefault();
            float hot = 26f;
            hotPart?.TransformValue(StatRequest.For(pawn), ref hot);
            Check(hot >= 26f + 40f * e + (fire ? 20f * e * 2f - 0.5f : 0f) - 0.01f, $"{(fire ? "fire root" : "the realm")} bears more heat (26 -> {hot:0} C; with clothes {maxTPlain:0} -> {maxT:0})");
            // el mismo colono con y sin la pieza (la ropa también cambia la temperatura cómoda)
            var part = StatDefOf.ComfyTemperatureMin.parts.OfType<StatPart_EnvironmentResistance>().FirstOrDefault();
            float raw = 10f, withPart = raw;
            part?.TransformValue(StatRequest.For(pawn), ref withPart);
            Check(part != null && withPart <= raw - 40f * e + 0.01f, $"the realm bears more cold ({raw:0} -> {withPart:0} C)");
            float fl = pawn.GetStatValue(StatDefOf.Flammability), flPlain = plain.GetStatValue(StatDefOf.Flammability);
            Check(fl < flPlain * (fire ? 0.7f : 0.9f), $"{(fire ? "fire root" : "the realm")} catches fire less ({flPlain:0.##} -> {fl:0.##})");
            Check(pawn.GetStatValue(StatDefOf.ToxicResistance) > plain.GetStatValue(StatDefOf.ToxicResistance), "the realm resists toxins");
            plain.Destroy();
        }

        private static void TestBreakthrough(Map map, Pawn pawn)
        {
            var comp = CompCultivator.For(pawn);
            var t = comp.Realm;
            while (!t.Bottleneck) t.AddProgress(t.Realm.progressPerStage);
            t.Resource.Fill();
            Check(BreakthroughUtility.CanAttempt(comp, t, out string reason), "can attempt breakthrough (" + reason + ")");
            float before = pawn.HealthScale;
            BreakthroughUtility.Ascend(pawn, t.NextRealm, 1f);
            Check(t.Realm.order == 2 && pawn.HealthScale > before, $"ascended to {t.Realm.defName}, HealthScale {before:0.##} -> {pawn.HealthScale:0.##}");
            Check(comp.RealmLevel == 2, "realm level cached = 2");
            Check(comp.Techniques.Points > 0, "technique points: " + comp.Techniques.Points);
        }

        private static void TestTechniques(Map map, Pawn pawn)
        {
            var comp = CompCultivator.For(pawn);
            var manuals = DefDatabase<TechniqueManualDef>.AllDefsListForReading;
            Check(manuals.Count >= 5 && manuals.All(m => m.Techniques.Count > 0), manuals.Count + " manuals with techniques");
            foreach (var m in manuals)
            {
                comp.Techniques.LearnManual(m, sendMessage: false);
                foreach (var tech in m.Techniques) comp.Techniques.Unlock(tech, free: true);
            }
            var actives = comp.Techniques.Unlocked.Where(d => !PassiveTechniqueUtility.IsPassive(d)).ToList();
            var passives = comp.Techniques.Unlocked.Where(PassiveTechniqueUtility.IsPassive).ToList();
            int abilities = comp.Techniques.Unlocked.Count(d => pawn.abilities.GetAbility(d) != null);
            Check(abilities == actives.Count && actives.Count >= 10, abilities + " active technique abilities granted");
            Check(passives.Count > actives.Count && passives.All(d => pawn.health.hediffSet.HasHediff(TechniqueUtility.Ext(d).passiveHediff)),
                $"{passives.Count} passive techniques (more than the {actives.Count} active ones) give their hediff, no button");
            var bell = DefDatabase<AbilityDef>.GetNamedSilentFail("CVG_Tech_GoldenBellSkin");
            if (bell != null)
            {
                var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(TechniqueUtility.Ext(bell).passiveHediff);
                hediff?.TryGetComp<HediffComp_PassiveTechnique>()?.Refresh();
                Check(hediff != null && Math.Abs(hediff.Severity - TechniqueUtility.Power(pawn, bell)) < 0.01f,
                    $"golden bell skin severity = power ({hediff?.Severity:0.00}), sharp armor {pawn.GetStatValue(StatDefOf.ArmorRating_Sharp):P0}");
            }
            int slots = comp.Techniques.ActiveSlots;
            Check(comp.Techniques.EquippedCount <= slots && slots >= 3, $"{comp.Techniques.EquippedCount} equipped of {slots} slots");
            var notEquipped = actives.FirstOrDefault(d => !comp.Techniques.IsEquipped(d));
            if (notEquipped != null && comp.Techniques.EquippedCount >= slots)
                Check(!comp.Techniques.SetEquipped(notEquipped, true), "cannot equip beyond the slots");
            if (pawn.drafter != null) pawn.drafter.Drafted = false;
            var equipped = actives.First(d => comp.Techniques.IsEquipped(d));
            Check(PassiveTechniqueUtility.ShouldHideGizmo(pawn, equipped) || !pawn.IsColonistPlayerControlled, "technique buttons hidden while undrafted");
            // Partidas viejas: una técnica ahora pasiva que tenía habilidad pierde el botón y gana el hediff.
            var converted = passives.FirstOrDefault(d => d.defName == "CVG_Tech_BlazingMeridians");
            if (converted != null)
            {
                pawn.abilities.GainAbility(converted);
                comp.Techniques.SyncAbilities();
                Check(pawn.abilities.GetAbility(converted) == null, "old saves: converted technique loses its button");
            }

            var enemy = PawnGenerator.GeneratePawn(PawnKindDefOf.Villager, Find.FactionManager.RandomEnemyFaction() ?? Faction.OfAncientsHostile);
            GenSpawn.Spawn(enemy, CellFinder.RandomClosewalkCellNear(pawn.Position, map, 5), map);

            if (pawn.drafter != null) pawn.drafter.Drafted = true;
            if (pawn.IsColonistPlayerControlled)
                Check(!PassiveTechniqueUtility.ShouldHideGizmo(pawn, equipped), "equipped technique buttons shown while drafted");
            foreach (var def in actives)
            {
                var ability = pawn.abilities.GetAbility(def);
                comp.Realm.Resource.Fill();
                ability.ResetCooldown();
                if (ability.GizmoDisabled(out string reason) && !def.verbProperties.verbClass.Name.Contains("Jump")
                    && !(TechniqueUtility.Ext(def).requiresElement && !TechniqueUtility.HasAffinity(pawn, def)))
                {
                    Fail(def.defName + " disabled: " + reason);
                    continue;
                }
                if (def.verbProperties.verbClass == typeof(Verb_CastAbilityJump)) continue;
                float qiBefore = comp.Realm.Resource.Current;
                float xpBefore = comp.Techniques.MasteryXp(def);
                LocalTargetInfo target = def.targetRequired ? (def.hostile ? (LocalTargetInfo)enemy : pawn) : pawn;
                ability.Activate(target, target);
                bool paid = comp.Realm.Resource.Current < qiBefore;
                bool xp = comp.Techniques.MasteryXp(def) > xpBefore;
                Check(paid && xp, $"{def.defName}: cast, qi {qiBefore:0} -> {comp.Realm.Resource.Current:0}, mastery {xpBefore:0} -> {comp.Techniques.MasteryXp(def):0}");
            }

            // Escudo de qi: absorbe un golpe.
            var shield = pawn.health.hediffSet.hediffs.OfType<HediffWithComps>().SelectMany(h => h.comps).OfType<HediffComp_QiShield>().FirstOrDefault();
            Check(shield != null && shield.Max > 0, "qi shield active: " + (shield?.CompLabelInBracketsExtra ?? "none"));
            if (shield != null)
            {
                float left = shield.Remaining;
                var dinfo = new DamageInfo(DamageDefOf.Cut, 5f, 0f, -1f, null);
                comp.PostPreApplyDamage(ref dinfo, out bool absorbed);
                Check(absorbed && shield.Remaining < left, $"shield absorbs 5 ({left:0} -> {shield.Remaining:0})");
            }

            comp.Techniques.AddMasteryLevel(comp.Techniques.Unlocked[0]);
            Check(comp.Techniques.Mastery(comp.Techniques.Unlocked[0]).order >= 2, "mastery level up");
            enemy.Destroy();
        }

        private static void TestSuppression(Map map, Pawn pawn)
        {
            var comp = CompCultivator.For(pawn);
            var mortal = PawnGenerator.GeneratePawn(PawnKindDefOf.Villager, Faction.OfAncientsHostile);
            var rifle = DefDatabase<ThingDef>.GetNamedSilentFail("Gun_AssaultRifle");
            var dinfo = new DamageInfo(DamageDefOf.Bullet, 20f, 0f, -1f, mortal, null, rifle);
            int grade = SpiritGradeUtility.AttackGrade(dinfo);
            Check(grade == 0, "mortal rifle grade 0");
            float expected = SpiritGradeUtility.SuppressionFactor(comp.RealmLevel, grade);
            comp.PostPreApplyDamage(ref dinfo, out _);
            Check(expected < 1f && Math.Abs(dinfo.Amount - 20f * expected) < 0.01f, $"realm {comp.RealmLevel}: 20 -> {dinfo.Amount:0.##} (x{expected:0.##})");
            var fists = new DamageInfo(DamageDefOf.Blunt, 10f, 0f, -1f, pawn, null, pawn.def);
            Check(SpiritGradeUtility.AttackGrade(fists) == comp.RealmLevel, "fists use the realm of the attacker");
            var fire = new DamageInfo(DamageDefOf.Flame, 10f);
            Check(SpiritGradeUtility.AttackGrade(fire) < 0, "no instigator: not suppressed");
            mortal.Destroy();
        }

        private static void TestCalendar(Map map, Pawn pawn)
        {
            var tide = DefDatabase<GameConditionDef>.GetNamed("CVG_QiTide");
            float before = pawn.GetStatValue(CVG_DefOf.CVG_CultivationSpeed, cacheStaleAfterTicks: 0);
            var condition = GameConditionMaker.MakeCondition(tide, GenDate.TicksPerDay);
            map.gameConditionManager.RegisterCondition(condition);
            float during = pawn.GetStatValue(CVG_DefOf.CVG_CultivationSpeed, cacheStaleAfterTicks: 0);
            Check(during > before * 1.9f, $"qi tide: cultivation speed {before:0.##} -> {during:0.##}, Dao x{CelestialCalendar.DaoFactor(pawn):0.##}");
            condition.End();
            var incident = DefDatabase<IncidentDef>.GetNamed("CVG_InnerDemonMoon");
            var parms = StorytellerUtility.DefaultParmsNow(incident.category, map);
            parms.forced = true;
            bool fired = incident.Worker.TryExecute(parms);
            var moon = map.gameConditionManager.GetActiveCondition(incident.gameCondition);
            Check(fired && moon != null, "inner demon moon incident starts its condition");
            float chance = pawn.GetStatValue(CVG_DefOf.CVG_BreakthroughChance, cacheStaleAfterTicks: 0);
            moon?.End();
            float after = pawn.GetStatValue(CVG_DefOf.CVG_BreakthroughChance, cacheStaleAfterTicks: 0);
            Check(chance < after, $"inner demon moon lowers the breakthrough chance ({chance:0.##} vs {after:0.##})");
        }

        private static void TestInnerDemonAndJoy(Map map)
        {
            var pawn = Colonist(map, root: true);
            var state = DefDatabase<MentalStateDef>.GetNamed("CVG_InnerDemon");
            var breakDef = DefDatabase<MentalBreakDef>.GetNamed("CVG_InnerDemon");
            Check(breakDef.Worker.BreakCanOccur(pawn), "inner demon can happen to a cultivator");
            float baseC = breakDef.Worker.CommonalityFor(pawn);
            var moon = GameConditionMaker.MakeCondition(DefDatabase<GameConditionDef>.GetNamed("CVG_InnerDemonMoon"), GenDate.TicksPerDay);
            map.gameConditionManager.RegisterCondition(moon);
            float moonC = breakDef.Worker.CommonalityFor(pawn);
            moon.End();
            Check(moonC > baseC * 4f, $"inner demon commonality {baseC:0.##} -> {moonC:0.##} under the moon");
            bool started = pawn.mindState.mentalStateHandler.TryStartMentalState(state, "test", forced: true);
            Check(started && pawn.MentalStateDef == state, "inner demon mental state starts");
            pawn.MentalState?.RecoverFromState();
            Check(pawn.health.hediffSet.HasHediff(DefDatabase<HediffDef>.GetNamed("CVG_QiDeviation")), "qi deviation after the inner demon");
            // Sin raíz y sin cultivo del cuerpo (el cultivo previo puede templar a un cuerpo especial al nacer).
            var mortal = Colonist(map, root: false);
            for (int i = 0; i < 10 && CompCultivator.For(mortal).IsCultivator; i++)
            {
                mortal.Destroy();
                mortal = Colonist(map, root: false);
            }
            Check(!breakDef.Worker.BreakCanOccur(mortal), "no inner demon for a mortal");
            mortal.Destroy();
            pawn.Destroy();

            foreach (var name in new[] { "CVG_Play_Go", "CVG_PracticeCalligraphy" })
            {
                var giver = DefDatabase<JoyGiverDef>.GetNamed(name);
                Check(giver.thingDefs.All(d => d.building?.joyKind == giver.joyKind) && giver.jobDef.joyKind == giver.joyKind,
                    $"{name}: joy kind {giver.joyKind.defName} on job and building");
            }
            var forge = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_SpiritForge");
            var levels = forge?.GetCompProperties<CompProperties_WorkbenchLevel>()?.levels;
            if (levels != null) Check(levels.Skip(1).All(l => l.requiredResearch != null), "spirit forge levels 2-3 gated by Dao research");
        }

        /// <summary>Rasgos que suben de grado con el uso (si hay alguno con DefModExtension_TraitProgression).</summary>
        private static void TestTraitProgression(Map map)
        {
            var defs = DefDatabase<TraitDef>.AllDefsListForReading.Where(t => t.HasModExtension<DefModExtension_TraitProgression>()).ToList();
            Check(true, defs.Count + " traits evolve with use: " + string.Join(", ", defs.Select(d => d.defName)));
            foreach (var def in defs)
            {
                var ext = def.GetModExtension<DefModExtension_TraitProgression>();
                var pawn = Colonist(map, root: true);
                foreach (var t in pawn.story.traits.allTraits.ToList()) pawn.story.traits.RemoveTrait(t);
                int start = def.degreeDatas.Min(d => d.degree);
                pawn.story.traits.GainTrait(new Trait(def, start, forced: true));
                var top = ext.steps.OrderByDescending(st => st.degree).First();
                TraitProgression.Add(pawn, ext.counter, top.threshold + 1f);
                int now = pawn.story.traits.GetTrait(def)?.Degree ?? int.MinValue;
                Check(now == top.degree, $"{def.defName}: {ext.counter} {top.threshold + 1:0} -> degree {start} -> {now}");
                pawn.Destroy();
            }
        }

        /// <summary>Maestro de marionetas y demonio de sangre (mod Convergence: Biotech Wuxia), si está cargado.</summary>
        private static void TestBiotechWuxia(Map map)
        {
            var thread = DefDatabase<HediffDef>.GetNamedSilentFail("CVG_PuppetHeartThread");
            if (!ModsConfig.BiotechActive || thread == null)
            {
                sb.Append("\n    note Biotech Wuxia not loaded: skipped");
                return;
            }
            var master = Colonist(map, root: true);
            var comp = CompCultivator.For(master);
            BreakthroughUtility.Ascend(master, comp.Realm.NextRealm, 1f);
            master.health.AddHediff(thread, master.health.hediffSet.GetNotMissingParts().FirstOrDefault(p => p.def == BodyPartDefOf.Heart));
            Check(MechanitorUtility.IsMechanitor(master), "puppet heart thread makes a mechanitor");
            float bandwidth = master.GetStatValue(StatDefOf.MechBandwidth, cacheStaleAfterTicks: 0);
            Check(bandwidth >= 4f + comp.RealmLevel, $"bandwidth {bandwidth:0} (thread 4 + realm {comp.RealmLevel})");

            var kind = DefDatabase<PawnKindDef>.GetNamed("CVG_Puppet_Bronze");
            var puppet = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, Faction.OfPlayer));
            GenSpawn.Spawn(puppet, CellFinder.RandomClosewalkCellNear(master.Position, map, 3), map);
            master.relations.AddDirectRelation(PawnRelationDefOf.Overseer, puppet);
            var pc = puppet.GetComp<CompPuppet>();
            Check(pc != null && puppet.GetOverseer() == master, "puppet overseen by its master");
            puppet.needs.energy.CurLevel = puppet.needs.energy.MaxLevel * 0.3f;
            comp.Realm.Resource.Fill();
            float qiBefore = comp.Realm.Resource.Current, energyBefore = puppet.needs.energy.CurLevel;
            pc.Refresh();
            Check(pc.MasterLevel == comp.RealmLevel && puppet.health.hediffSet.HasHediff(DefDatabase<HediffDef>.GetNamed("CVG_PuppetBond")),
                $"puppet bond = master realm {pc.MasterLevel}, health x{pc.HealthFactor:0.##} (scale {puppet.HealthScale:0.##})");
            Check(puppet.needs.energy.CurLevel > energyBefore && comp.Realm.Resource.Current < qiBefore,
                $"recharge with qi: energy {energyBefore:0.##} -> {puppet.needs.energy.CurLevel:0.##}, qi {qiBefore:0} -> {comp.Realm.Resource.Current:0}");
            puppet.Kill(new DamageInfo(DamageDefOf.Cut, 999f));   // roto en combate (desarmarlo, Kill sin daño, no castiga)
            Check(master.health.hediffSet.HasHediff(DefDatabase<HediffDef>.GetNamed("CVG_PuppetBacklash")), "thread backlash when the puppet breaks");
            puppet.Corpse?.Destroy();
            master.Destroy();

            var xeno = DefDatabase<XenotypeDef>.GetNamedSilentFail("CVG_Xeno_BloodDemon");
            if (xeno == null) return;
            var demon = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayer, forceGenerateNewPawn: true,
                mustBeCapableOfViolence: true, developmentalStages: DevelopmentalStage.Adult, forcedXenotype: xeno));
            GenSpawn.Spawn(demon, map.Center, map);
            var drain = demon.abilities.GetAbility(DefDatabase<AbilityDef>.GetNamed("CVG_Tech_BloodDrain"));
            Check(drain != null && demon.abilities.GetAbility(DefDatabase<AbilityDef>.GetNamed("CVG_Tech_BloodArmor")) != null, "blood demon has blood techniques");
            var foe = PawnGenerator.GeneratePawn(PawnKindDefOf.Villager, Faction.OfAncientsHostile);
            GenSpawn.Spawn(foe, CellFinder.RandomClosewalkCellNear(demon.Position, map, 3), map);
            float foeHp = foe.health.summaryHealth.SummaryHealthPercent;
            // Hasta 3 intentos: la armadura puede desviar un golpe.
            for (int i = 0; i < 3 && drain != null && !foe.Dead && foe.health.summaryHealth.SummaryHealthPercent >= foeHp; i++)
            {
                drain.ResetCooldown();
                drain.Activate(foe, foe);
            }
            Check(foe.Dead || foe.health.summaryHealth.SummaryHealthPercent < foeHp, "blood drain hurts the foe");
            var moonSilver = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_MoonSilver");
            var sword = DefDatabase<ThingDef>.GetNamedSilentFail("MeleeWeapon_LongSword");
            if (moonSilver != null && sword != null && !foe.Dead)
            {
                var weapon = (ThingWithComps)ThingMaker.MakeThing(sword, moonSilver);
                foe.equipment.DestroyAllEquipment();
                foe.equipment.AddEquipment(weapon);
                var hit = new DamageInfo(DamageDefOf.Cut, 10f, 0f, -1f, foe, null, sword);
                Check(StuffWeaknessUtility.FactorFor(demon, hit) > 1.4f, "moon silver burns the blood demon (x1.5)");
            }
            if (!foe.Destroyed) foe.Destroy();
            demon.Destroy();
        }

        /// <summary>Economía paralela: los objetos de cultivo salen de la riqueza vanilla y entran en la de cultivo.</summary>
        private static void TestEconomy(Map map)
        {
            var stoneDef = DefDatabase<ThingDef>.GetNamed("CVG_SpiritStone");
            Check(CultivationWealth.IsCultivationThing(stoneDef) && !CultivationWealth.IsCultivationThing(ThingDefOf.Silver)
                && !CultivationWealth.IsCultivationThing(ThingDefOf.Human), "spirit stones are cultivation things; silver and humans are not");
            map.wealthWatcher.ForceRecount();
            float vanillaBefore = map.wealthWatcher.WealthItems;
            float cultBefore = CultivationWealth.For(map).items;
            var stones = ThingMaker.MakeThing(stoneDef);
            stones.stackCount = stoneDef.stackLimit;
            // Cerca de un colono (el centro del mapa puede estar bajo la niebla, que la riqueza vanilla no cuenta).
            var near = map.mapPawns.FreeColonistsSpawned.FirstOrDefault()?.Position ?? map.Center;
            GenPlace.TryPlaceThing(stones, near, map, ThingPlaceMode.Near);
            float value = stones.MarketValue * stones.stackCount;
            map.wealthWatcher.ForceRecount();
            float vanillaDelta = map.wealthWatcher.WealthItems - vanillaBefore;
            float cultDelta = CultivationWealth.For(map).items - cultBefore;
            float factor = CultivationDifficulty.Current.vanillaWealthFactor;
            Check(Math.Abs(vanillaDelta - value * factor) < value * 0.05f + 1f && Math.Abs(cultDelta - value) < value * 0.05f + 1f,
                $"{stones.stackCount} spirit stones ({value:F0}): vanilla wealth +{vanillaDelta:F0} (x{factor:0.##}), cultivation wealth +{cultDelta:F0}");
            Check(Math.Abs(stones.MarketValue * stones.stackCount - value) < 0.01f, "trade value unchanged");
            stones.Destroy();
            var sect = Find.FactionManager.AllFactionsListForReading.FirstOrDefault(f => f.def.HasModExtension<DefModExtension_ConvergenceFaction>());
            var other = Find.FactionManager.AllFactionsListForReading.FirstOrDefault(f => !f.def.HasModExtension<DefModExtension_ConvergenceFaction>()
                && f.HostileTo(Faction.OfPlayer) && !f.def.hidden);
            if (sect != null && other != null)
            {
                float threat = CultivationWealth.ThreatPoints(map);
                float a = IncidentWorker_Raid.AdjustedRaidPoints(1000f, PawnsArrivalModeDefOf.EdgeWalkIn, RaidStrategyDefOf.ImmediateAttack, sect, PawnGroupKindDefOf.Combat, map);
                float b = IncidentWorker_Raid.AdjustedRaidPoints(1000f, PawnsArrivalModeDefOf.EdgeWalkIn, RaidStrategyDefOf.ImmediateAttack, other, PawnGroupKindDefOf.Combat, map);
                Check(Math.Abs(a - b - threat) < 1f, $"sect raids get the cultivation threat (+{threat:F0} points; total {CultivationWealth.For(map).Total:F0})");
            }
        }

        /// <summary>Dificultad de cultivo: velocidad (StatPart) y frecuencia de eventos del mod.</summary>
        private static void TestDifficulty(Map map)
        {
            var d = CultivationDifficulty.Current;
            var saved = d.Copy();
            var pawn = Colonist(map, root: true);
            d.cultivationSpeed = 1f;
            float s1 = pawn.GetStatValue(CVG_DefOf.CVG_CultivationSpeed, cacheStaleAfterTicks: -1);
            d.cultivationSpeed = 2f;
            float s2 = pawn.GetStatValue(CVG_DefOf.CVG_CultivationSpeed, cacheStaleAfterTicks: -1);
            Check(s1 > 0f && Math.Abs(s2 / s1 - 2f) < 0.01f, $"cultivation speed x2 by difficulty ({s1:0.00} -> {s2:0.00})");
            var incident = DefDatabase<IncidentDef>.AllDefsListForReading.FirstOrDefault(i => CultivationDifficulty.IsConvergenceDef(i) && i.baseChance > 0f);
            if (incident != null)
            {
                d.eventFrequency = 1f;
                float c1 = incident.Worker.BaseChanceThisGame;
                d.eventFrequency = 2f;
                float c2 = incident.Worker.BaseChanceThisGame;
                Check(c1 > 0f && Math.Abs(c2 / c1 - 2f) < 0.01f, $"event frequency x2 for {incident.defName}");
            }
            var easy = DefDatabase<CultivationDifficultyDef>.GetNamedSilentFail("CVG_Difficulty_Easy");
            if (easy != null)
            {
                d.Apply(easy);
                Check(!d.tribulationCanKill && d.preset == easy, "easy preset: tribulation cannot kill");
            }
            d.CopyFrom(saved);
            pawn.Destroy();
        }

        /// <summary>IA: escudo antes de atacar si hay enemigos cerca, técnicas de área solo contra grupos, sin avances en combate.</summary>
        private static void TestEnemyAI(Map map)
        {
            var shieldDef = DefDatabase<AbilityDef>.GetNamed("CVG_Tech_QiShield");
            var coneDef = DefDatabase<AbilityDef>.GetNamed("CVG_Tech_SwordQiSweep");
            var hostile = Find.FactionManager.RandomEnemyFaction() ?? Faction.OfAncientsHostile;
            var npc = Colonist(map, root: true);
            npc.SetFaction(hostile);
            var comp = CompCultivator.For(npc);
            comp.Realm.Resource.Fill();
            comp.Techniques.Unlock(shieldDef, free: true);
            comp.Techniques.Unlock(coneDef, free: true);
            var target = Colonist(map, root: false);
            if (target.Position.DistanceTo(npc.Position) > 5f)
            {
                target.DeSpawn();
                GenSpawn.Spawn(target, CellFinder.RandomClosewalkCellNear(npc.Position, map, 3), map);
            }
            npc.mindState.enemyTarget = target; // lo fija JobGiver_AIFightEnemy antes de buscar habilidades
            var job = CultivatorAI.DefensiveJob(npc);
            Check(job != null && job.ability?.def == shieldDef, "NPC raises the qi shield with an enemy close (" + (job?.ability?.def.defName ?? "none") + ")");
            npc.health.AddHediff(DefDatabase<HediffDef>.GetNamed("CVG_QiShield"));
            Check(CultivatorAI.DefensiveJob(npc) == null, "...but not when the shield is already up");
            var cone = npc.abilities.GetAbility(coneDef);
            bool group = CultivatorAI.EnemiesNear(npc, target.Position, 3.5f) >= 2;
            Check(cone.CompOfType<CompAbilityEffect_AIConditions>().AICanTargetNow(target) == group, $"area technique only against a group ({CultivatorAI.EnemiesNear(npc, target.Position, 3.5f)} near the target)");
            var tracker = target.drafter != null ? CompCultivator.For(Colonist(map, root: true)) : null;
            if (tracker != null)
            {
                var p = tracker.Pawn;
                p.drafter.Drafted = true;
                var t = tracker.Realm;
                t.EnsureStarted();
                while (!t.Bottleneck) t.AddProgress(t.Realm.progressPerStage);
                t.Resource.Fill();
                Check(!BreakthroughUtility.CanAttempt(tracker, t, out string why) && why == "CVG_Breakthrough_InCombat".Translate(p.Named("PAWN")),
                    "no breakthrough while drafted: " + why);
                p.Destroy();
            }
            npc.Destroy();
            target.Destroy();
        }

        /// <summary>Longevidad: los reinos frenan el envejecimiento de los adultos (no el crecimiento de los niños).</summary>
        private static void TestLongevity(Map map)
        {
            var pawn = Colonist(map, root: true);
            pawn.ageTracker.AgeBiologicalTicks = 30L * GenDate.TicksPerYear;
            var comp = CompCultivator.For(pawn);
            var t = comp.Realm;
            t.EnsureStarted();
            comp.InvalidateHealthScale();
            float f1 = comp.AgingFactor, ticks1 = pawn.ageTracker.BiologicalTicksPerTick;
            var golden = DefDatabase<CultivationRealmDef>.GetNamedSilentFail("CVG_Realm_GoldenCore");
            if (golden != null)
            {
                t.AdvanceTo(golden, 1f);
                comp.InvalidateHealthScale();
                float f3 = comp.AgingFactor, ticks3 = pawn.ageTracker.BiologicalTicksPerTick;
                Check(f1 < 1f && f3 <= 0.2f && Math.Abs(ticks3 / ticks1 - f3 / f1) < 0.01f,
                    $"aging: {t.Realm.defName} x{f3:0.00} vs first realm x{f1:0.00} (biological ticks {ticks1:0.00} -> {ticks3:0.00})");
                pawn.ageTracker.AgeBiologicalTicks = 10L * GenDate.TicksPerYear;
                Check(pawn.ageTracker.BiologicalTicksPerTick > ticks3 * 2f, "children grow normally");
            }
            pawn.Destroy();
        }

        /// <summary>El cultivo en la vida diaria: pensamientos, registros, relatos, alertas y puestos asignables.</summary>
        private static void TestCultivationLife(Map map)
        {
            var pawn = Colonist(map, root: true);
            int before = pawn.records.GetAsInt(DefDatabase<RecordDef>.GetNamed("CVG_Breakthroughs"));
            CultivationLife.Notify_Ascended(pawn);
            Check(pawn.needs.mood.thoughts.memories.GetFirstMemoryOfDef(DefDatabase<ThoughtDef>.GetNamed("CVG_Thought_BreakthroughSuccess")) != null
                && pawn.records.GetAsInt(DefDatabase<RecordDef>.GetNamed("CVG_Breakthroughs")) == before + 1, "breakthrough: memory and record");
            var other = Colonist(map, root: false);
            CultivationLife.Notify_TribulationSurvived(pawn);
            Check(other.needs.mood.thoughts.memories.GetFirstMemoryOfDef(DefDatabase<ThoughtDef>.GetNamed("CVG_Thought_WitnessedTribulation")) != null,
                "colonists on the map witness the tribulation");
            // Los cercanos sienten el avance: la pareja se alegra con el nombre de quien avanzó.
            var lover = Colonist(map, root: false);
            lover.relations.AddDirectRelation(PawnRelationDefOf.Lover, pawn);
            CultivationLife.Notify_Ascended(pawn);
            var loverMem = lover.needs.mood.thoughts.memories.GetFirstMemoryOfDef(DefDatabase<ThoughtDef>.GetNamed("CVG_Thought_PartnerAscended"));
            Check(loverMem != null && loverMem.otherPawn == pawn && loverMem.MoodOffset() >= 9f, $"partner feels the breakthrough ({loverMem?.LabelCap})");
            lover.Destroy();
            var tale = Find.TaleManager.AllTalesListForReading.LastOrDefault(t => t.def.defName == "CVG_Tale_SurvivedTribulation");
            Check(tale != null && tale.def.usableForArt, "tribulation tale recorded for art");
            Check(new Alert_CVG_ReadyToBreakthrough().GetReport() is AlertReport, "alerts build their reports");
            var spot = DefDatabase<ThingDef>.GetNamed("CVG_CultivationSpot");
            Check(spot.HasModExtension<DefModExtension_CultivationSpot>() && spot.GetStatValueAbstract(StatDefOf.WorkToBuild) == 0f
                && spot.HasComp(typeof(CompAssignableToPawn)), "free cultivation spot, assignable");
            var placed = (Building)GenSpawn.Spawn(ThingMaker.MakeThing(spot), CellFinder.RandomClosewalkCellNear(pawn.Position, map, 4), map);
            placed.SetFaction(Faction.OfPlayer);
            placed.TryGetComp<CompAssignableToPawn>().TryAssignPawn(other);
            Check(!new WorkGiver_Cultivate().HasJobOnThing(pawn, placed, forced: true), "an assigned spot is only for its owner");
            placed.Destroy();
            other.Destroy();
            pawn.Destroy();
        }

        /// <summary>Armas wuxia (Alchemy) y accesorios con capas propias (Apparel): CONTENIDO_TOI_ACS.md.</summary>
        private static void TestWuxiaGear(Map map)
        {
            var jian = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_Jian");
            if (jian != null)
            {
                string[] weapons = { "CVG_Jian", "CVG_Dao", "CVG_Bishou", "CVG_Zhanmadao", "CVG_Guandao", "CVG_Staff", "CVG_IronFan",
                    "CVG_StraightBow", "CVG_HornBow", "CVG_RepeatingCrossbow", "CVG_FlyingNeedles" };
                var missing = weapons.Where(w => !DefDatabase<RecipeDef>.AllDefsListForReading.Any(r => r.ProducedThingDef?.defName == w && !r.AllRecipeUsers.EnumerableNullOrEmpty()))
                    .ToList();
                Check(missing.Count == 0, "11 wuxia weapons have a recipe at some bench" + (missing.Count > 0 ? " (missing: " + string.Join(", ", missing) + ")" : ""));
                var forge = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_SpiritForge");
                Check(forge == null || forge.AllRecipes.Any(r => r.ProducedThingDef == jian), "the spirit forge also makes the jian (copied smithy recipes)");
                var plain = (ThingWithComps)ThingMaker.MakeThing(jian, ThingDefOf.Steel);
                Check(SpiritGradeUtility.GradeOf(plain) == 0, "a steel jian is mortal (grade 0)");
                var steel = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_SpiritSteel");
                if (steel != null)
                {
                    var spiritJian = ThingMaker.MakeThing(jian, steel);
                    Check(SpiritGradeUtility.GradeOf(spiritJian) == 1, "a spirit steel jian has spirit grade 1 (material floor)");
                    var sword = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_SpiritSword");
                    if (sword != null)
                        Check(SpiritGradeUtility.GradeOf(ThingMaker.MakeThing(sword, steel)) == 2, "...a spirit sword of spirit steel stays at 2 (a floor, not a sum)");
                }
                {
                    // disonancia de implantes vanilla (StatPart_ImplantDissonance)
                    var cy = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayer, forceGenerateNewPawn: true));
                    float before = cy.GetStatValue(CVG_DefOf.CVG_CelestialDissonance, cacheStaleAfterTicks: 0);
                    var bionic = DefDatabase<HediffDef>.GetNamedSilentFail("BionicArm");
                    var shoulder = cy.RaceProps.body.GetPartsWithDef(BodyPartDefOf.Shoulder).FirstOrDefault();
                    if (bionic != null && shoulder != null)
                    {
                        cy.health.AddHediff(bionic, shoulder);
                        float after = cy.GetStatValue(CVG_DefOf.CVG_CelestialDissonance, cacheStaleAfterTicks: 0);
                        Check(after - before > 0.1f, $"a vanilla bionic arm adds celestial dissonance ({before:0.00} -> {after:0.00})");
                        float keep = ConvergenceMod.Settings.implantDissonanceFactor;
                        ConvergenceMod.Settings.implantDissonanceFactor = 0f;
                        Check(System.Math.Abs(cy.GetStatValue(CVG_DefOf.CVG_CelestialDissonance, cacheStaleAfterTicks: 0) - before) < 0.001f, "implant dissonance setting at 0% turns it off");
                        ConvergenceMod.Settings.implantDissonanceFactor = keep;
                    }
                    cy.Destroy();
                }
                var crossbow = DefDatabase<ThingDef>.GetNamed("CVG_RepeatingCrossbow");
                Check(crossbow.Verbs[0].burstShotCount == 2, "the repeating crossbow shoots bursts of 2");
            }

            var pendantDef = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_JadePendant");
            if (pendantDef == null) return;
            var pawn = Colonist(map, root: true);
            Apparel Make(string name)
            {
                var def = DefDatabase<ThingDef>.GetNamed(name);
                return (Apparel)ThingMaker.MakeThing(def, def.MadeFromStuff ? GenStuff.DefaultStuffFor(def) : null);
            }
            var robe = Make("CVG_Apparel_RobeOuter");
            var pendant = Make("CVG_JadePendant");
            var space = Make("CVG_SpaceRing");
            var qiRing = Make("CVG_QiGatheringRing");
            var belt = Make("CVG_TalismanBelt");
            Check(ApparelUtility.CanWearTogether(robe.def, pendant.def, pawn.RaceProps.body)
                && ApparelUtility.CanWearTogether(robe.def, belt.def, pawn.RaceProps.body), "accessories never take the place of clothes");
            Check(ApparelUtility.CanWearTogether(space.def, qiRing.def, pawn.RaceProps.body), "a spatial ring and a qi ring can be worn together");
            Check(!ApparelUtility.CanWearTogether(pendant.def, DefDatabase<ThingDef>.GetNamed("CVG_HeartGuardAmulet"), pawn.RaceProps.body), "...but only one pendant");
            Check(new[] { pendant, space, qiRing, belt }.All(a => a.def.apparel.wornGraphicPath.NullOrEmpty()), "accessories are not drawn over the clothes");
            float carry = pawn.GetStatValue(StatDefOf.CarryingCapacity);
            pawn.apparel.Wear(space, dropReplacedApparel: false);
            // Al menos la mitad del +60: otros mods (pack del usuario) escalan CarryingCapacity después de los offsets (75 -> 60, +60 -> +48).
            float ringOffset = space.def.equippedStatOffsets.GetStatOffsetFromList(StatDefOf.CarryingCapacity);
            Check(pawn.GetStatValue(StatDefOf.CarryingCapacity) >= carry + ringOffset * 0.5f, $"the spatial ring adds carrying capacity ({carry:0} -> {pawn.GetStatValue(StatDefOf.CarryingCapacity):0})");

            var element = CompCultivator.For(pawn)?.Root?.elements.FirstOrDefault(e => DefDatabase<ThingDef>.GetNamedSilentFail("CVG_Earrings_" + e.defName.Replace("CVG_Element_", "")) != null);
            if (element != null)
            {
                var earDef = DefDatabase<ThingDef>.GetNamed("CVG_Earrings_" + element.defName.Replace("CVG_Element_", ""));
                float before = pawn.GetStatValue(CVG_DefOf.CVG_CultivationSpeed, cacheStaleAfterTicks: 0);
                pawn.apparel.Wear((Apparel)ThingMaker.MakeThing(earDef), dropReplacedApparel: false);
                float after = pawn.GetStatValue(CVG_DefOf.CVG_CultivationSpeed, cacheStaleAfterTicks: 0);
                Check(after > before * 1.1f, $"{earDef.defName} resonate with the {element.defName} root ({before:0.00} -> {after:0.00})");
            }
            else sb.Append("\n    note no earrings for this root's elements; resonance not checked");
            pawn.Destroy();
        }

        /// <summary>Rasgos que cumplen su descripción, inspiraciones wuxia y pensamientos de personalidad.</summary>
        private static void TestTraitsKeepWord(Map map)
        {
            var child = DefDatabase<TraitDef>.GetNamedSilentFail("CVG_Heart_Child");
            if (child == null) return;
            Check(child.degreeDatas[0].disallowedMentalStates?.Any(s => s.defName == "CVG_InnerDemon") ?? false, "child's heart: no inner demon");
            var insp = DefDatabase<InspirationDef>.GetNamedSilentFail("CVG_Inspired_Enlightenment");
            var pawn = Colonist(map, root: true);
            Check(insp != null && insp.Worker.InspirationCanOccur(pawn), "a cultivator can have a sudden enlightenment");
            var mortal = Colonist(map, root: false);
            for (int i = 0; i < 10 && CompCultivator.For(mortal).IsCultivator; i++) { mortal.Destroy(); mortal = Colonist(map, root: false); }
            Check(!insp.Worker.InspirationCanOccur(mortal), "...a mortal cannot");
            Check(pawn.mindState.inspirationHandler.TryStartInspiration(insp) && pawn.GetStatValue(CVG_DefOf.CVG_CultivationSpeed) > 1.5f,
                "enlightenment speeds up cultivation");
            pawn.Destroy();
            mortal.Destroy();
        }

        /// <summary>Arte wuxia (parte 3b) y mesas wuxia: recetas y trabajadores (WorkGiver) de su mesa vanilla.</summary>
        private static void TestWuxiaArt(Map map)
        {
            var kitchen = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_SectKitchen");
            if (kitchen != null)
                Check(DefDatabase<WorkGiverDef>.GetNamed("DoBillsCook").fixedBillGiverDefs.Contains(kitchen), "the sect kitchen is worked by the cooking work giver");
            var desk = DefDatabase<ThingDef>.GetNamed("CVG_CalligraphyDesk");
            var scroll = DefDatabase<ThingDef>.GetNamed("CVG_CalligraphyScroll");
            Check(desk.AllRecipes.Any(r => r.ProducedThingDef == scroll) && DefDatabase<WorkGiverDef>.AllDefs.Any(w => w.fixedBillGiverDefs?.Contains(desk) ?? false),
                "calligraphy desk makes scroll art and has a work giver");
            var table = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_JadeCarvingTable");
            var sculpture = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_JadeSculptureSmall");
            if (table == null || sculpture == null) return;
            Check(table.AllRecipes.Any(r => r.ProducedThingDef == sculpture)
                && DefDatabase<WorkGiverDef>.GetNamed("DoBillsSculpt").fixedBillGiverDefs.Contains(table), "jade sculptures at the jade carver's table, worked by the art work giver");
            foreach (var def in new[] { sculpture, scroll })
            {
                var thing = ThingMaker.MakeThing(def);
                var art = thing.TryGetComp<CompArt>();
                art?.InitializeArt(ArtGenerationContext.Colony);
                Check(art != null && !art.Title.NullOrEmpty() && !art.GenerateImageDescription().NullOrEmpty(),
                    $"{def.defName}: \"{art?.Title}\" - {art?.GenerateImageDescription().ToString().Truncate(90)}");
            }
        }

        /// <summary>Día a día, parte 4 (mod Alchemy): comidas, ración, medicinas, té e incienso como sus equivalentes vanilla.</summary>
        private static void TestDailyConsumables(Map map)
        {
            var buns = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_MealSteamedBuns");
            if (buns == null) return;
            var rice = DefDatabase<ThingDef>.GetNamed("CVG_MealSpiritRice");
            var banquet = DefDatabase<ThingDef>.GetNamed("CVG_MealSectBanquet");
            Check(buns.ingestible.preferability == FoodPreferability.MealSimple && rice.ingestible.preferability == FoodPreferability.MealFine
                && banquet.ingestible.preferability == FoodPreferability.MealLavish, "buns / spirit rice / banquet = simple / fine / lavish meals");
            var stove = DefDatabase<ThingDef>.GetNamed("FueledStove");
            var kitchen = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_SectKitchen");
            var cook = DefDatabase<RecipeDef>.GetNamed("CVG_CookSteamedBuns");
            Check(stove.AllRecipes.Contains(cook) && (kitchen == null || kitchen.AllRecipes.Contains(cook)), "meals cook at the stove and the sect kitchen");
            float h = DefDatabase<ThingDef>.GetNamed("CVG_HerbalOintment").GetStatValueAbstract(StatDefOf.MedicalPotency);
            float p = DefDatabase<ThingDef>.GetNamed("CVG_HealingPill").GetStatValueAbstract(StatDefOf.MedicalPotency);
            float j = DefDatabase<ThingDef>.GetNamed("CVG_JadeElixir").GetStatValueAbstract(StatDefOf.MedicalPotency);
            Check(h < p && p < j && DefDatabase<ThingDef>.GetNamed("CVG_JadeElixir").IsMedicine, $"medicine tiers: ointment {h:0.00} < pill {p:0.00} < elixir {j:0.00}");

            var pawn = Colonist(map, root: true);
            foreach (var (def, hediff) in new[] { ("CVG_BiguPill", "CVG_BiguCalm"), ("CVG_CloudTea", "CVG_CloudTeaClarity"), ("CVG_Incense", "CVG_IncenseCalm") })
            {
                var thing = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed(def));
                GenSpawn.Spawn(thing, pawn.Position, map);
                thing.Ingested(pawn, 0f);
                Check(pawn.health.hediffSet.HasHediff(DefDatabase<HediffDef>.GetNamed(hediff)), $"{def} gives {hediff}");
            }
            pawn.Destroy();
        }

        /// <summary>Trueno: raíz variante rara (de Madera+Fuego o del Qilin), tribulación más suave, rayo en cadena.</summary>
        private static void TestThunder(Map map)
        {
            var thunder = DefDatabase<QiElementDef>.GetNamedSilentFail("CVG_Element_Thunder");
            if (thunder == null) return;
            int rolls = 4000, withThunder = 0, bad = 0;
            for (int i = 0; i < rolls; i++)
            {
                var els = CultivationGenerator.RandomElements();
                if (!els.Contains(thunder)) continue;
                withThunder++;
                if (thunder.variantOf.All(els.Contains)) bad++;
            }
            Check(withThunder > 0 && withThunder < rolls * 0.05f && bad == 0,
                $"thunder root is a rare variant: {withThunder} of {rolls} rolls ({withThunder * 100f / rolls:0.0}%), replacing wood+fire");

            var pawn = Colonist(map, root: true);
            var comp = CompCultivator.For(pawn);
            var target = DefDatabase<CultivationRealmDef>.GetNamed("CVG_Realm_Foundation");
            MapComponent_Tribulation.Strength(pawn, target, out _, out float before);
            var purity = new BloodlinePurity { rootElement = thunder, rootElementChance = 1f };
            Check(BloodlineUtility.TryAwakenRootElement(pawn, purity, notify: false) && comp.Root.elements.Contains(thunder),
                "a pure Qilin bloodline can awaken a thunder root");
            MapComponent_Tribulation.Strength(pawn, target, out _, out float after);
            Check(after < before * 0.65f, $"thunder root softens tribulation lightning ({before:0.0} -> {after:0.0})");
            var path = DefDatabase<TechniqueManualDef>.GetNamed("CVG_Manual_NineHeavensThunder");
            Check(path.OpensNaturallyFor(comp), "the thunder root opens the Nine Heavens Thunder path");

            var chain = DefDatabase<AbilityDef>.GetNamed("CVG_Tech_ChainLightning");
            comp.Techniques.LearnManual(path, sendMessage: false, freeTechnique: false);
            comp.Techniques.Unlock(chain, free: true);
            comp.Realm.Resource.Fill();
            var hostile = Find.FactionManager.RandomEnemyFaction() ?? Faction.OfAncientsHostile;
            var foes = new List<Pawn>();
            var center = CellFinder.RandomClosewalkCellNear(pawn.Position, map, 6);
            for (int i = 0; i < 3; i++)
            {
                var foe = PawnGenerator.GeneratePawn(PawnKindDefOf.Villager, hostile);
                GenSpawn.Spawn(foe, CellFinder.RandomClosewalkCellNear(center, map, 2), map);
                foes.Add(foe);
            }
            var hp = foes.Select(f => f.health.summaryHealth.SummaryHealthPercent).ToList();
            pawn.abilities.GetAbility(chain).Activate(foes[0], foes[0]);
            int struck = foes.Where((f, i) => f.Dead || f.health.summaryHealth.SummaryHealthPercent < hp[i]).Count();
            Check(struck >= 2, $"chain lightning leaps between enemies ({struck}/3 struck)");
            foreach (var f in foes) if (!f.Destroyed) f.Destroy();
            pawn.Destroy();
        }

        /// <summary>Caminos estilo VPE: la raíz abre su camino elemental, las tablillas los extra; conjuntos de combate.</summary>
        private static void TestTechniquePaths(Map map)
        {
            var pawn = Colonist(map, root: true); // raíz de fuego si no tenía
            var comp = CompCultivator.For(pawn);
            var t = comp.Techniques;
            t.SyncOpenPaths();
            var fire = DefDatabase<TechniqueManualDef>.GetNamed("CVG_Manual_BlazingSun");
            var sword = DefDatabase<TechniqueManualDef>.GetNamed("CVG_Manual_SwordHeart");
            bool hasFire = TechniqueUtility.HasElement(comp, fire.element);
            Check(!hasFire || (!t.KnowsManual(fire) && t.CanOpenPath(fire, out _, ignorePoints: true)), "the fire root makes the Blazing Sun path unlockable");
            if (hasFire)
            {
                t.AddPoints(1);
                int before = t.Points;
                t.OpenPath(fire);
                Check(t.KnowsManual(fire) && t.Points == before - 1, "unlocking a path costs 1 point (VPE)");
            }
            Check(!t.KnowsManual(sword) && !sword.OpensNaturallyFor(comp), "the Sword path needs its jade slip");
            int opened = t.Manuals.Count;
            Check(t.Sets.Count >= t.Manuals.Count(m => m.Techniques.Any(d => !PassiveTechniqueUtility.IsPassive(d))),
                $"one combat set per open path ({t.Sets.Count} sets, {opened} paths)");
            Check(t.Unlocked.Count == 0, "opening a path by root gives no free technique (VPE: one point each)");

            t.LearnManual(sword, sendMessage: false);
            var edge = DefDatabase<AbilityDef>.GetNamed("CVG_Tech_EdgeOfTheHeart");
            var flying = DefDatabase<AbilityDef>.GetNamed("CVG_Tech_FlyingSword");
            t.AddPoints(5);
            if (!t.IsUnlocked(edge)) t.Unlock(edge);
            Check(t.CanUnlock(flying, out string why), "flying sword unlockable after edge of the heart: " + why);
            t.Unlock(flying);
            var swordSet = t.Sets.FirstOrDefault(s => s.manual == sword);
            Check(swordSet != null && swordSet.techniques.Contains(flying), "a new active goes to the set of its path");
            var custom = t.AddSet("test");
            Check(t.SetInSet(custom, flying, true) && !t.SetInSet(custom, edge, true), "sets take actives, not passives");
            t.SetActiveSet(t.Sets.IndexOf(custom));
            if (pawn.drafter != null) pawn.drafter.Drafted = true;
            if (pawn.IsColonistPlayerControlled)
            {
                Check(!PassiveTechniqueUtility.ShouldHideGizmo(pawn, flying), "technique of the active set shown while drafted");
                t.SetActiveSet(0);
                Check(PassiveTechniqueUtility.ShouldHideGizmo(pawn, flying) == !t.ActiveSet.techniques.Contains(flying), "switching sets hides the others");
                Check(TechniqueSetUtility.MakeSwitchGizmo(comp) != null, "set switch gizmo");
            }
            // Partidas viejas: una técnica de un camino no abierto (cambió de camino) abre ese camino.
            var mind = DefDatabase<AbilityDef>.GetNamed("CVG_Tech_StillWaterMind");
            t.Unlock(mind, free: true);
            t.Manuals.Remove(TechniqueUtility.Ext(mind).manual);
            t.SyncAbilities();
            Check(t.KnowsManual(TechniqueUtility.Ext(mind).manual), "old saves: a learned technique keeps its (new) path open");
            Check(new Dialog_CultivationDevelopment(pawn) != null, "development window");
            pawn.Destroy();
        }

        /// <summary>Jiangshi (Anomaly): mutante, robo de yang, madera de melocotonero, espejo bagua y quemadura del sol.</summary>
        private static void TestJiangshi(Map map)
        {
            var kind = DefDatabase<PawnKindDef>.GetNamedSilentFail("CVG_Jiangshi");
            if (kind == null || !ModsConfig.AnomalyActive) return;
            var jiangshi = IncidentWorker_JiangshiNight.Generate(kind, 1, map, map.Center)[0];
            Check(jiangshi.IsMutant && jiangshi.mutant.Def.defName == "CVG_Jiangshi" && jiangshi.Faction == Faction.OfEntities,
                "jiangshi spawns as an Anomaly mutant of the entities faction");
            var victim = Colonist(map, root: false);
            var drain = new DamageInfo(DefDatabase<DamageDef>.GetNamed("CVG_YangDrain"), 6f, 0f, -1f, jiangshi);
            victim.TakeDamage(drain);
            victim.TakeDamage(drain);
            var corpseQi = victim.health.hediffSet.GetFirstHediffOfDef(DefDatabase<HediffDef>.GetNamed("CVG_CorpseQi"));
            Check(corpseQi != null && corpseQi.Severity > 0.2f, $"claws drain yang: corpse qi {corpseQi?.Severity:0.00}");
            var cure = DefDatabase<RecipeDef>.GetNamed("CVG_DrawCorpseQi");
            Check(cure.Worker.AvailableOnNow(victim), "glutinous rice operation available on the wounded");
            var peach = DefDatabase<ThingDef>.GetNamed("CVG_PeachWood");
            var club = DefDatabase<ThingDef>.GetNamed("MeleeWeapon_Club");
            victim.equipment.DestroyAllEquipment();
            victim.equipment.AddEquipment((ThingWithComps)ThingMaker.MakeThing(club, peach));
            Check(StuffWeaknessUtility.FactorFor(jiangshi, new DamageInfo(DamageDefOf.Blunt, 10f, 0f, -1f, victim, null, club)) > 2.9f,
                "peach wood club hurts the jiangshi x3");
            victim.equipment.DestroyAllEquipment();
            var sword = DefDatabase<ThingDef>.GetNamed("CVG_PeachWoodSword");
            victim.equipment.AddEquipment((ThingWithComps)ThingMaker.MakeThing(sword));
            Check(StuffWeaknessUtility.FactorFor(jiangshi, new DamageInfo(DamageDefOf.Blunt, 10f, 0f, -1f, victim, null, sword)) > 2.9f,
                "peach wood sword hurts the jiangshi x3");
            if (!CellFinder.TryFindRandomCellNear(jiangshi.Position, map, 3, c => c != jiangshi.Position && c.Standable(map)
                    && GenSight.LineOfSight(c, jiangshi.Position, map), out var mirrorCell))
                mirrorCell = jiangshi.Position + new IntVec3(1, 0, 0);
            var mirror = GenSpawn.Spawn(ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("CVG_BaguaMirror")), mirrorCell, map);
            int dazed = mirror.TryGetComp<CompBaguaMirror>().Pulse();
            Check(dazed >= 1 && jiangshi.health.hediffSet.HasHediff(DefDatabase<HediffDef>.GetNamed("CVG_BaguaDazed")), "bagua mirror dazes the jiangshi");
            Check(HediffComp_SunBurn.InSunlight(jiangshi, 0f) == !jiangshi.Position.Roofed(map), "sunlight check");
            var incident = DefDatabase<IncidentDef>.GetNamed("CVG_JiangshiNight");
            Check(((IncidentWorker_JiangshiNight)incident.Worker).CountFor(300f) >= 2, "night of the hungry ghosts scales with points");
            mirror.Destroy();
            victim.Destroy();
            jiangshi.Destroy();
        }

        /// <summary>Cultivo ya iniciado al generar: forzado (reino alto y caída al máximo disponible) y frecuencia real.</summary>
        private static void TestPreCultivation(Map map)
        {
            var def = DefDatabase<PreCultivationDef>.GetNamedSilentFail("CVG_PreCultivation_Qi");
            if (def == null) return;
            var pawn = Colonist(map, root: true);
            var comp = CompCultivator.For(pawn);
            PreCultivation.Apply(pawn, comp, def, new PreCultivationOption { realmOrder = 99, stage = new IntRange(2, 2) });
            var t = comp.Realm;
            int max = DefDatabase<CultivationRealmDef>.AllDefsListForReading.Where(r => r.path == def.path).Max(r => r.order);
            Check(t.PreCultivated && t.Realm.order == max && t.Stage == 2 && pawn.health.hediffSet.HasHediff(t.Realm.hediff),
                $"forced realm order 99 -> {t.Realm.defName} stage {t.Stage} (highest available), active after the Convergence");
            pawn.Destroy();
            int rooted = 0, begun = 0, high = 0;
            for (int i = 0; i < 150; i++)
            {
                var p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayer, forceGenerateNewPawn: true));
                var c = CompCultivator.For(p);
                if (c != null && c.HasRoot)
                {
                    rooted++;
                    var tr = c.TrackerFor(CVG_DefOf.CVG_Path_Qi);
                    if (tr != null && tr.PreCultivated) { begun++; if (tr.Realm.order > 1) high++; }
                }
                p.Destroy();
            }
            Check(true, $"150 colonists: {rooted} with root, {begun} with cultivation already begun ({high} above the first realm)");
        }

        private static void TestTribulation(Map map, Pawn pawn)
        {
            var mc = MapComponent_Tribulation.For(map);
            mc.DebugBegin(pawn);
            Check(mc.IsUndergoing(pawn), "tribulation started");
        }

        private static void TestBodyPath(Map map)
        {
            // El pre-cultivo puede hacer nacer colonos con el camino del cuerpo ya empezado: buscar uno que no lo empezó.
            var pawn = Colonist(map, root: false);
            for (int tries = 0; tries < 10 && CompCultivator.For(pawn).IsBodyCultivator; tries++) { pawn.Destroy(); pawn = Colonist(map, root: false); }
            var comp = CompCultivator.For(pawn);
            var body = CVG_DefOf.CVG_Path_Body;
            Check(comp.CanFollow(body) && !comp.IsBodyCultivator, "rootless colonist can follow the body path but has not started");

            var meat = ThingMaker.MakeThing(CVG_DefOf.CVG_SpiritMeat);
            meat.stackCount = 10;
            GenSpawn.Spawn(meat, pawn.Position, map);
            meat.Ingested(pawn, 0.5f);
            var tracker = comp.TrackerFor(body);
            Check(comp.IsBodyCultivator && tracker.Started, "eating spirit meat starts the body path (" + tracker?.Realm?.defName + ")");
            Check(tracker != null && tracker.Resource.Current > 0f, "blood qi " + tracker?.Resource.Current.ToString("0"));

            var soup = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("CVG_SpiritSoup"));
            GenSpawn.Spawn(soup, pawn.Position, map);
            float before = tracker.Resource.Current;
            soup.Ingested(pawn, 0.9f);
            Check(tracker.Resource.Current > before && pawn.health.hediffSet.HasHediff(DefDatabase<HediffDef>.GetNamed("CVG_SpiritNourished")),
                $"spirit soup: blood qi {before:0} -> {tracker.Resource.Current:0}, nourished");

            var dummy = (Building)ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("CVG_WoodenDummy"));
            dummy.SetFaction(Faction.OfPlayer);
            // Casilla cerca del colono, construible y alcanzable sin peligro (al azar a veces caía detrás de una pared o en el frío).
            var spot = GenRadial.RadialCellsAround(pawn.Position, 12f, false).FirstOrDefault(c => c.InBounds(map) && c.Standable(map)
                && GenConstruct.CanPlaceBlueprintAt(dummy.def, c, Rot4.North, map).Accepted
                && ThingUtility.InteractionCellWhenAt(dummy.def, c, Rot4.North, map) is var ic && ic.InBounds(map) && ic.Standable(map)
                && pawn.CanReach(ic, Verse.AI.PathEndMode.OnCell, Danger.Some));   // el mismo peligro que acepta el work giver
            GenSpawn.Spawn(dummy, spot != IntVec3.Zero ? spot : CellFinder.RandomClosewalkCellNear(pawn.Position, map, 4), map, WipeMode.Vanish);
            var giver = new WorkGiver_TemperBody();
            Check(comp.CanTemperNow, "can temper now");
            bool danger = CultivatorAI.InDanger(pawn, 20f);
            // Sin casilla segura en 12 casillas (mapa al azar del quicktest) el work giver acierta al rechazarlo: no es un fallo.
            if (spot == IntVec3.Zero) sb.Append("\n    skip work giver finds the dummy (no safe reachable cell near the colonist)");
            else Check(giver.HasJobOnThing(pawn, dummy, forced: danger), "work giver finds the dummy" + (danger ? " (forced: enemies near)" : "")
                + $" [reserve {Verse.AI.ReservationUtility.CanReserve(pawn, dummy, 1, -1, null, danger)}, forbidden {dummy.IsForbidden(pawn)}]");

            while (!tracker.Bottleneck) tracker.AddProgress(tracker.Realm.progressPerStage);
            tracker.Resource.Fill();
            bool can = BreakthroughUtility.CanAttempt(comp, tracker, out string reason);
            Check(!can, "body breakthrough blocked without beast blood: " + reason);
            var blood = ThingMaker.MakeThing(CVG_DefOf.CVG_BeastBlood);
            GenSpawn.Spawn(blood, pawn.Position, map);
            Check(BreakthroughUtility.CanAttempt(comp, tracker, out reason), "body breakthrough possible with beast blood (" + reason + ")");
            BreakthroughUtility.Ascend(pawn, tracker.NextRealm, 1f);
            Check(tracker.Realm.order == 2, "ascended to " + tracker.Realm.defName);
            dummy.Destroy();
            if (!blood.Destroyed) blood.Destroy();
        }

        private static void TestSpiritBeasts(Map map)
        {
            var kind = DefDatabase<PawnKindDef>.GetNamedSilentFail("Bear_Grizzly") ?? DefDatabase<PawnKindDef>.AllDefs.First(k => k.RaceProps.Animal);
            var beast = PawnGenerator.GeneratePawn(kind);
            GenSpawn.Spawn(beast, CellFinder.RandomClosewalkCellNear(map.Center, map, 10), map);
            // Puede haber despertado ya al generarse (Convergencia activa): empezar sin el estado.
            var already = beast.health.hediffSet.GetFirstHediffOfDef(CVG_DefOf.CVG_SpiritBeast);
            if (already != null) beast.health.RemoveHediff(already);
            float before = beast.HealthScale;
            SpiritBeastUtility.Awaken(beast, 3);
            Check(Hediff_SpiritBeast.LevelOf(beast) == 3, "awakened at level 3");
            Check(beast.HealthScale > before, $"beast health scale {before:0.##} -> {beast.HealthScale:0.##}");
            Check(SpiritGradeUtility.RealmLevel(beast) == 3, "beast bites at grade 3");
            beast.Kill(null);
            var butcher = map.mapPawns.FreeColonistsSpawned.First();
            var products = beast.Corpse.ButcherProducts(butcher, 1f).ToList();
            Check(products.Any(t => t.def == CVG_DefOf.CVG_SpiritMeat) && products.Any(t => t.def == CVG_DefOf.CVG_BeastBlood)
                  && products.Any(t => t.def == CVG_DefOf.CVG_BeastCore),
                "butcher: " + string.Join(", ", products.GroupBy(t => t.def.defName).Select(g => g.Key + " x" + g.Sum(t => t.stackCount))));
            foreach (var p in products) if (!p.Destroyed) p.Destroy();
            beast.Corpse?.Destroy();
            Check(SpiritBeastUtility.AwakenChance(PawnGenerator.GeneratePawn(kind)) > 0f, "awaken chance > 0 for " + kind.defName);
        }

        private static void TestMentor(Map map)
        {
            var state = GameComponent_Mentor.Instance;
            var def = DefDatabase<MentorDef>.AllDefsListForReading.FirstOrDefault();
            Check(state != null && def != null, "mentor component and def");
            if (state == null || def == null || state.Mentor != null) return;
            var mentor = state.Arrive(map, def, wounded: false);
            var comp = CompCultivator.For(mentor);
            Check(mentor != null && mentor.Spawned && mentor.Faction == Faction.OfPlayer, "mentor arrived and joined");
            Check(comp.RealmLevel >= 3 && comp.Techniques.Unlocked.Count >= 6, $"mentor realm {comp.RealmLevel}, {comp.Techniques.Unlocked.Count} techniques");
            Check(mentor.story.traits.allTraits.All(t => def.traits.Contains(t.def)), "only the mentor's traits");
            for (int i = 0; i < def.lessons.Count; i++) state.CompleteLesson();
            Check(state.CurrentLesson == null, "all lessons completed");
            var wounded = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction.OfPlayer);
            GenSpawn.Spawn(wounded, map.Center, map);
            HealthUtility.DamageUntilDowned(wounded, allowBleedingWounds: false);
            Check(wounded.Downed, "DamageUntilDowned works (wounded arrival)");
            wounded.Destroy();
            state.End(died: false);
            Check(!mentor.Spawned && map.listerThings.ThingsOfDef(def.farewellItem).Any(), "mentor left and the jade fragment stays");
        }

        /// <summary>Errores de carga del idioma activo (Keyed duplicados, DefInjected de Defs o campos inexistentes…).</summary>
        private static void TestTranslations()
        {
            var lang = LanguageDatabase.activeLanguage;
            if (lang == null) return;
            var errors = new List<string>(lang.loadErrors);
            foreach (var injection in lang.defInjections) errors.AddRange(injection.loadErrors);
            foreach (var e in errors) sb.Append("\n    note " + e);
            int ours = errors.Count(e => e.Contains("CVG_") || e.Contains("Convergence"));
            Check(ours == 0, lang.folderName + ": " + errors.Count + " load error(s), " + ours + " from Convergence");
        }

        private static bool IsOurs(Def def) => def.modContentPack?.PackageId?.StartsWith("prospero1088.convergence") == true;

        /// <summary>Genera (sin spawnear) un pawn de cada PawnKindDef de los mods Convergence (razas, sectas…).</summary>
        private static void TestPawnKinds(Map map)
        {
            int ok = 0;
            var kinds = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(IsOurs).ToList();
            foreach (var kind in kinds)
            {
                try
                {
                    var faction = kind.defaultFactionDef != null ? Find.FactionManager.FirstFactionOfDef(kind.defaultFactionDef) : null;
                    var pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, faction, forceGenerateNewPawn: true));
                    var comp = CompCultivator.For(pawn);
                    sb.Append($"\n    note {kind.defName}: {pawn.LabelShort}, realm {comp?.RealmLevel ?? 0}, {comp?.TechniquesIfAny?.Unlocked.Count ?? 0} techniques");
                    pawn.Destroy();
                    ok++;
                }
                catch (Exception e)
                {
                    Fail(kind.defName + ": " + e.Message);
                }
            }
            Check(ok == kinds.Count, ok + " / " + kinds.Count + " Convergence pawn kinds generated");
        }

        /// <summary>Facciones de los mods Convergence (sectas): cuáles existen en la partida tras la Convergencia.</summary>
        private static void TestFactions()
        {
            var defs = DefDatabase<FactionDef>.AllDefsListForReading.Where(IsOurs).ToList();
            foreach (var def in defs)
            {
                var faction = Find.FactionManager.FirstFactionOfDef(def);
                sb.Append($"\n    note {def.defName}: {(faction == null ? "not in this game" : faction.Name + ", hostile " + faction.HostileTo(Faction.OfPlayer))}");
            }
            Check(true, defs.Count + " Convergence faction defs listed");
        }

        /// <summary>Recetas de los mods Convergence: productos, ingredientes y mesas válidos.</summary>
        private static void TestRecipes()
        {
            var recipes = DefDatabase<RecipeDef>.AllDefsListForReading.Where(IsOurs).ToList();
            int bad = 0;
            foreach (var r in recipes)
            {
                bool users = !r.AllRecipeUsers.EnumerableNullOrEmpty() || r.IsSurgery;
                bool products = !r.products.NullOrEmpty() || r.specialProducts != null || r.IsSurgery || r.workerClass != typeof(RecipeWorker);
                bool ingredients = r.ingredients == null || r.ingredients.All(i => i.filter != null && i.filter.AllowedDefCount > 0);
                if (!users || !products || !ingredients)
                {
                    Fail($"{r.defName}: users {users}, products {products}, ingredients {ingredients}");
                    bad++;
                }
            }
            Check(bad == 0, recipes.Count + " Convergence recipes valid");
        }

        /// <summary>
        /// Base común del nuevo Sects: etiqueta de espadas, constituciones con grados, requisito de rol, constitución inicial, ritual de templado,
        /// desvío y resistencia a proyectiles, cuerpos y rasgos de los discípulos, y stock con calidad mínima. Usa rasgos vanilla con grados
        /// (laboriosidad) para no depender del contenido de ninguna secta.
        /// </summary>
        private static void TestSectFramework(Map map)
        {
            var longsword = DefDatabase<ThingDef>.GetNamed("MeleeWeapon_LongSword");
            Check(longsword.weaponTags?.Contains("CVG_Sword") ?? false, "the longsword is tagged CVG_Sword");
            var jian = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_Jian");
            if (jian != null) Check(jian.weaponTags?.Contains("CVG_Sword") ?? false, "the jian is tagged CVG_Sword");
            Check(!(DefDatabase<ThingDef>.GetNamed("MeleeWeapon_Mace").weaponTags?.Contains("CVG_Sword") ?? false), "the mace is not a sword");

            var industry = DefDatabase<TraitDef>.GetNamed("Industriousness");
            var nimble = DefDatabase<TraitDef>.GetNamed("Nimble");
            var pawn = Colonist(map, root: false);
            ConstitutionUtility.RemoveTrait(pawn, industry);
            ConstitutionUtility.RemoveTrait(pawn, nimble);

            // Rasgos con grado y requisito de rol
            var req = new RoleRequirement_HasTrait { traits = new List<TraitDef> { industry } };
            Check(!req.Met(pawn, null), "role requirement not met without the trait");
            ConstitutionUtility.SetTrait(pawn, industry, 1);
            Check(ConstitutionUtility.Degree(pawn, industry) == 1 && req.Met(pawn, null), "trait degree 1 meets the role requirement");
            ConstitutionUtility.SetTrait(pawn, industry, 2);
            Check(ConstitutionUtility.Degree(pawn, industry) == 2 && pawn.story.traits.allTraits.Count(t => t.def == industry) == 1, "degree 1 -> 2 replaces it");
            ConstitutionUtility.RemoveTrait(pawn, industry);

            // Ritual de templado: sin rasgo -> 1 -> 2 -> otro rasgo (el "celestial"), y después nada
            var ext = new DefModExtension_TemperConstitution
            {
                steps = new List<TemperStep>
                {
                    new TemperStep { toTrait = industry, toDegree = 1, blockedBy = new List<TraitDef> { nimble } },
                    new TemperStep { fromTrait = industry, fromDegree = 1, toTrait = industry, toDegree = 2 },
                    new TemperStep { fromTrait = industry, fromDegree = 2, toTrait = nimble, toDegree = 0 },
                }
            };
            foreach (var s in ext.steps) s.chanceByQuality = new SimpleCurve { new CurvePoint(0f, 0f), new CurvePoint(1f, 1f) };
            Check(ext.StepFor(pawn) == ext.steps[0], "untempered pawn: first step");
            RitualOutcomeEffectWorker_TemperConstitution.Resolve(ext, pawn, 1f, map);
            Check(ConstitutionUtility.Degree(pawn, industry) == 1, "quality 100%: none -> degree 1");
            RitualOutcomeEffectWorker_TemperConstitution.Resolve(ext, pawn, 0f, map);
            Check(ConstitutionUtility.Degree(pawn, industry) == 1, "quality 0%: the step fails, nothing changes");
            RitualOutcomeEffectWorker_TemperConstitution.Resolve(ext, pawn, 1f, map);
            Check(ConstitutionUtility.Degree(pawn, industry) == 2, "degree 1 -> 2");
            RitualOutcomeEffectWorker_TemperConstitution.Resolve(ext, pawn, 1f, map);
            Check(ConstitutionUtility.Degree(pawn, industry) == null && ConstitutionUtility.Degree(pawn, nimble) != null, "degree 2 -> the separate top trait");
            Check(ext.StepFor(pawn) == null, "nothing left to temper");
            ext.cost = new List<ThingDefCountClass> { new ThingDefCountClass(DefDatabase<ThingDef>.GetNamed("CVG_SpiritStone"), 100000) };
            Check(!ext.CanPay(map, out _), "a cost the colony cannot pay blocks the ritual");

            // Constitución inicial: solo a uno, el mejor en cuerpo a cuerpo
            var others = new List<Pawn> { Colonist(map, root: false), Colonist(map, root: false), Colonist(map, root: false) };
            foreach (var p in others) ConstitutionUtility.RemoveTrait(p, industry);
            GameComponent_StartingTraits.Apply(others, new DefModExtension_StartingTrait { trait = industry, degree = 1, count = 1 });
            Check(others.Count(p => ConstitutionUtility.Degree(p, industry) != null) == 1, "starting trait: exactly one colonist");
            var best = others.OrderByDescending(p => p.skills.GetSkill(SkillDefOf.Melee).Level).First();
            Check(ConstitutionUtility.Degree(best, industry) != null, "starting trait goes to the best melee fighter");

            // Discípulos: cuerpo y rasgo con probabilidad
            var kindExt = new DefModExtension_CultivatorPawnKind
            {
                forcedBodyType = BodyTypeDefOf.Hulk,
                traitChances = new List<TraitChance> { new TraitChance { trait = industry, degree = 2, chance = 1f } }
            };
            var d = others[0];
            CultivatorPawnKindUtility.ApplyBodyAndTraits(d, kindExt);
            Check(d.story.bodyType == BodyTypeDefOf.Hulk && ConstitutionUtility.Degree(d, industry) == 2, "forced Hulk body and trait by chance");
            kindExt = new DefModExtension_CultivatorPawnKind { disallowedBodyTypes = new List<BodyTypeDef> { BodyTypeDefOf.Hulk, BodyTypeDefOf.Thin } };
            CultivatorPawnKindUtility.ApplyBodyAndTraits(d, kindExt);
            Check(d.story.bodyType != BodyTypeDefOf.Hulk && d.story.bodyType != BodyTypeDefOf.Thin, "disallowed body types are replaced (" + d.story.bodyType.defName + ")");

            // Desvío y resistencia a proyectiles
            Check(ProjectileDefense.DeflectStat != null && ProjectileDefense.ResistStat != null, "projectile defense stats exist");
            int deflected = 0;
            for (int i = 0; i < 400; i++) { var b = new DamageInfo(DamageDefOf.Bullet, 10f); if (ProjectileDefense.Apply(null, ref b, 1f, 0f)) deflected++; }
            Check(deflected > 140 && deflected < 260, "deflection is capped at 50% (" + deflected + "/400)");
            var hit = new DamageInfo(DamageDefOf.Bullet, 10f);
            ProjectileDefense.Apply(null, ref hit, 0f, 0.3f);
            Check(Math.Abs(hit.Amount - 7f) < 0.01f, "30% resistance: 10 -> " + hit.Amount);
            var bomb = new DamageInfo(DamageDefOf.Bomb, 10f);
            Check(!ProjectileDefense.Apply(null, ref bomb, 1f, 0.5f) && Math.Abs(bomb.Amount - 10f) < 0.01f, "explosions are neither deflected nor reduced");
            var cut = new DamageInfo(DamageDefOf.Cut, 10f);
            Check(!ProjectileDefense.Apply(null, ref cut, 1f, 0.5f) && Math.Abs(cut.Amount - 10f) < 0.01f, "melee hits are not affected");
            var part = new StatPart_RequiresMeleeWeapon { weaponTags = new List<string> { "CVG_Sword" } };
            float v = 0.3f;
            part.TransformValue(StatRequest.For(pawn), ref v);
            Check(v == 0f, "no sword in hand: deflection 0");
            var sword = (ThingWithComps)ThingMaker.MakeThing(longsword, GenStuff.DefaultStuffFor(longsword));
            pawn.equipment.DestroyAllEquipment();
            pawn.equipment.AddEquipment(sword);
            v = 0.3f;
            part.TransformValue(StatRequest.For(pawn), ref v);
            Check(Math.Abs(v - 0.3f) < 0.001f, "longsword in hand: deflection kept");

            // Stock de especialidad con calidad mínima
            var gen = new StockGenerator_QualityTag
            {
                tradeTag = "WeaponMelee",
                weaponTags = new List<string> { "CVG_Sword" },
                qualities = new List<QualityChance> { new QualityChance { quality = QualityCategory.Legendary } },
                countRange = new IntRange(1, 1)
            };
            var stock = new List<Thing>();
            for (int i = 0; i < 6; i++) stock.AddRange(gen.GenerateThings(map.Tile));
            Check(stock.Count > 0 && stock.All(t => t.def.weaponTags.Contains("CVG_Sword")), "stock: only swords (" + string.Join(", ", stock.Select(t => t.def.defName).Distinct()) + ")");
            Check(stock.All(t => t.TryGetComp<CompQuality>()?.Quality == QualityCategory.Legendary), "stock: forced quality");
            foreach (var t in stock) t.Destroy();

            foreach (var p in others.Append(pawn)) if (!p.Destroyed) p.Destroy();
        }

        /// <summary>Con el mapa lleno de huéspedes no llegan más caravanas ni visitantes (salvo forzados); con tope 0, como vanilla.</summary>
        private static void TestGuestLimit(Map map)
        {
            void SendHome()
            {
                foreach (var lord in map.lordManager.lords.Where(l => l.LordJob is LordJob_VisitColony || l.LordJob is LordJob_TradeWithColony).ToList())
                    foreach (var p in lord.ownedPawns.ToList()) if (!p.Destroyed) p.Destroy();
            }
            var settings = ConvergenceMod.Settings;
            int saved = settings.maxGuestGroups;
            try
            {
                SendHome();
                Check(GuestLimit.Count(map) == 0, "no guests at the start");
                var f = Find.FactionManager.AllFactionsListForReading.FirstOrDefault(x => !x.IsPlayer && !x.Hidden && !x.temporary && x.def.humanlikeFaction
                    && !x.HostileTo(Faction.OfPlayer) && x.def.pawnGroupMakers != null && x.def.pawnGroupMakers.Any(g => g.kindDef == PawnGroupKindDefOf.Peaceful));
                if (f == null) { sb.Append("\n    skip: no friendly faction that sends visitors"); return; }
                IncidentParms Parms(bool forced)
                {
                    var p = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.Misc, map);
                    p.faction = f;
                    p.forced = forced;
                    return p;
                }
                settings.maxGuestGroups = 1;
                Check(IncidentDefOf.VisitorGroup.Worker.TryExecute(Parms(true)) && GuestLimit.Count(map) == 1, "a visitor group arrives (" + f.Name + ")");
                Check(GuestLimit.Full(map), "limit 1: the map is full");
                Check(!IncidentDefOf.VisitorGroup.Worker.CanFireNow(Parms(false)), "limit 1: no second visitor group");
                Check(!IncidentDefOf.TraderCaravanArrival.Worker.CanFireNow(Parms(false)), "limit 1: no trade caravan either");
                Check(IncidentDefOf.VisitorGroup.Worker.CanFireNow(Parms(true)), "forced visits (quests) still pass");
                settings.maxGuestGroups = 2;
                Check(!GuestLimit.Full(map), "limit 2 (default): one more group fits");
                settings.maxGuestGroups = 0;
                Check(!GuestLimit.Full(map), "limit 0: no limit, as in vanilla");
            }
            finally
            {
                settings.maxGuestGroups = saved;
                SendHome();
            }
        }

        /// <summary>Un colono come/usa una unidad de cada ingerible de los mods Convergence (píldoras, comida, sangre…).</summary>
        private static void TestIngestibles(Map map)
        {
            // Sin cadáveres (sin pawn adentro nacen "bugged") ni plantas (no se comen hasta madurar): eso lo prueba el juego.
            var defs = DefDatabase<ThingDef>.AllDefsListForReading.Where(d => IsOurs(d) && d.IsIngestible && !d.IsCorpse
                && d.category != ThingCategory.Plant).ToList();
            int ok = 0;
            foreach (var def in defs)
            {
                var pawn = Colonist(map, root: def.defName.Length % 2 == 0);
                try
                {
                    var thing = ThingMaker.MakeThing(def, def.MadeFromStuff ? GenStuff.DefaultStuffFor(def) : null);
                    GenSpawn.Spawn(thing, pawn.Position, map);
                    thing.Ingested(pawn, pawn.needs?.food?.NutritionWanted ?? 0.5f);
                    ok++;
                }
                catch (Exception e)
                {
                    Fail(def.defName + ": " + e.Message);
                }
                if (!pawn.Destroyed) pawn.Destroy();
            }
            Check(ok == defs.Count, ok + " / " + defs.Count + " Convergence ingestibles consumed");
        }

        /// <summary>Crea y spawnea un ejemplar de cada ThingDef de los mods Convergence (edificios con su material por defecto).</summary>
        private static void TestSpawnAllThings(Map map)
        {
            int ok = 0;
            var defs = DefDatabase<ThingDef>.AllDefsListForReading.Where(d =>
                IsOurs(d) && !d.IsBlueprint && !d.IsFrame
                && !d.isUnfinishedThing && d.category != ThingCategory.Pawn && d.category != ThingCategory.Ethereal
                && d.category != ThingCategory.Projectile && d.category != ThingCategory.Mote && !d.IsCorpse
                && !typeof(MapPortal).IsAssignableFrom(d.thingClass)).ToList();   // salidas de pocket map: necesitan su portal
            foreach (var def in defs)
            {
                try
                {
                    var stuff = def.MadeFromStuff ? GenStuff.DefaultStuffFor(def) : null;
                    var thing = ThingMaker.MakeThing(def, stuff);
                    if (thing.def.CanHaveFaction) thing.SetFaction(Faction.OfPlayer);
                    var cell = CellFinder.RandomClosewalkCellNear(map.Center, map, 25);
                    if (def.category == ThingCategory.Building && !GenConstruct.CanPlaceBlueprintAt(def, cell, Rot4.North, map).Accepted)
                    {
                        if (!CellFinder.TryFindRandomCellNear(map.Center, map, 40, c => GenConstruct.CanPlaceBlueprintAt(def, c, Rot4.North, map).Accepted, out cell))
                            continue;
                    }
                    GenSpawn.Spawn(thing, cell, map);
                    if (def.destroyable) thing.Destroy();
                    else thing.DeSpawn();
                    ok++;
                }
                catch (Exception e)
                {
                    Fail(def.defName + ": " + e.Message);
                }
            }
            Check(ok > 0, ok + " / " + defs.Count + " Convergence things spawned and destroyed");
        }
    }
}
