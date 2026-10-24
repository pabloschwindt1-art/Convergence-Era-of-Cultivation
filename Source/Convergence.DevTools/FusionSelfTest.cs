using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Autotest del plan de fusión (con <c>-cvgselftest</c>): Qi-Tech (torretas de qi generadas, templado, núcleos de meca,
    /// laboratorio de píldoras, hidroponía), supresión de reino en bestias, caos de Anomaly (daño, resistencia, botín, sabio)
    /// y Odyssey (qi en órbita, koi, reinos olvidados). Lo que pertenece a un módulo no cargado se saltea.
    /// </summary>
    public static class FusionSelfTest
    {
        private static int fails;
        private static StringBuilder sb;

        private static void Check(bool ok, string what)
        {
            sb.Append("\n  " + (ok ? "ok   " : "FAIL ") + what);
            if (!ok) fails++;
        }

        private static ThingDef T(string name) => DefDatabase<ThingDef>.GetNamedSilentFail(name);

        public static void Run()
        {
            fails = 0;
            sb = new StringBuilder("[Convergence] Fusion self-test");
            var map = Find.CurrentMap;
            try
            {
                TestDefs();
                TestQiTurrets(map);
                TestTemper();
                TestBeasts(map);
                TestChaos(map);
                TestMechCore(map);
                TestProduction();
                TestOdyssey(map);
            }
            catch (System.Exception e)
            {
                sb.Append("\n  FAIL exception: " + e);
                fails++;
            }
            sb.Append(fails == 0 ? "\n  ALL OK" : "\n  FAILURES: " + fails);
            if (fails == 0) Log.Message(sb.ToString());
            else Log.Error(sb.ToString());
        }

        private static void TestDefs()
        {
            foreach (var name in new[] { "CVG_ChaosCore", "CVG_ChaosTalisman_Ruin", "CVG_ChaosTalisman_Dread", "CVG_ChaosTalisman_Mending" })
                Check(T(name) != null, "def " + name);
            if (T("CVG_QiForge") != null)
                foreach (var name in new[] { "CVG_QiCannon_I", "CVG_QiCannon_II", "CVG_QiCannon_III", "CVG_QT_QiShotgun", "CVG_QT_QiPistol",
                             "CVG_Apparel_QiVest", "CVG_Apparel_QiPants", "CVG_Apparel_QiHelmet", "CVG_QiBionicArm", "CVG_QT_PillLab",
                             "CVG_QT_SpiritHydroponics", "CVG_QT_QiAccumulator", "CVG_QT_Shell_Qi", "CVG_QT_MechQiCore_Basic",
                             "CVG_QT_MechQiCore_Medium", "CVG_QT_MechQiCore_Advanced", "CVG_QT_MechTuningStation" })
                    Check(T(name) != null, "def " + name);
            var sage = DefDatabase<MentorDef>.GetNamedSilentFail("CVG_Mentor_ChaosSage");
            Check(sage != null && sage.commonality == 0f && GameComponent_ChaosSage.Instance != null, "chaos sage def (never a random mentor) and component");
            var jade = T("MineableJade");
            Check(jade?.building?.mineableScatterCommonality >= 0.15f, $"vanilla jade veins x{jade?.building?.mineableScatterCommonality}");
            var stone = T("CVG_SpiritStone");
            Check(stone?.deepCommonality > 0f, "spirit stone can be deep drilled");
            Check(Patch_CompDeepScanner_ChooseLumpThingDef.Blocked(stone, Find.CurrentMap) == !GameComponent_Convergence.Active,
                "deep scanner finds spirit stone only after the Convergence");
        }

        private static void TestQiTurrets(Map map)
        {
            var source = T("Turret_MiniTurret");
            var qi = T("CVG_QiTurret3_Turret_MiniTurret");
            if (T("CVG_QiCannon_III") == null) return;
            int count = DefDatabase<ThingDef>.AllDefsListForReading.Count(d => d.defName.StartsWith("CVG_QiTurret"));
            Check(count >= 3, $"qi turrets generated: {count}");
            Check(qi != null && qi.designatorDropdown != null && qi.designatorDropdown == source?.designatorDropdown,
                "qi mini-turret III shares the build dropdown with the mini-turret");
            Check(qi?.costList?.Any(c => c.thingDef?.defName == "CVG_QiCannon_III") ?? false, "qi mini-turret III costs a qi cannon III");
            Check(qi?.blueprintDef != null && qi.frameDef != null, "qi mini-turret III has blueprint and frame");
            Check(T("CVG_QiTurret1_CVG_QT_InductionTurret") == null, "turrets that already have a grade get no qi version");
            if (qi == null || map == null) return;
            var cell = CellFinder.RandomClosewalkCellNear(map.Center, map, 8, c => c.Standable(map) && c.GetEdifice(map) == null);
            var turret = GenSpawn.Spawn(ThingMaker.MakeThing(qi, qi.MadeFromStuff ? GenStuff.DefaultStuffFor(qi) : null), cell, map);
            int grade = SpiritGradeUtility.GradeOf(turret);
            var gun = qi.building.turretGunDef;
            int attack = SpiritGradeUtility.AttackGrade(new DamageInfo(DamageDefOf.Bullet, 10f, 0f, -1f, turret, null, gun));
            Check(grade >= 3 && attack >= 3, $"qi mini-turret III has grade {grade} and attacks with grade {attack}");
            turret.Destroy();
        }

        private static void TestTemper()
        {
            var def = T("Gun_Autopistol");
            if (def == null || T("CVG_QiForge") == null) return;
            var gun = (ThingWithComps)ThingMaker.MakeThing(def);
            gun.TryGetComp<CompQuality>()?.SetQuality(QualityCategory.Normal, ArtGenerationContext.Colony);
            int before = SpiritGradeUtility.GradeOf(gun);
            RecipeWorker_TemperGrade.Temper(gun, gun.GetComp<CompSpiritGrade>(), raiseQuality: true);
            int after = SpiritGradeUtility.GradeOf(gun);
            var q = gun.TryGetComp<CompQuality>()?.Quality;
            Check(after == before + 1 && q == QualityCategory.Good, $"tempering an autopistol: grade {before} -> {after}, quality Normal -> {q}");
            gun.Destroy();
        }

        private static void TestBeasts(Map map)
        {
            var kind = DefDatabase<PawnKindDef>.GetNamedSilentFail("Bear_Grizzly");
            if (kind == null || map == null) return;
            var bear = PawnGenerator.GeneratePawn(kind);
            SpiritBeastUtility.Awaken(bear, 6);
            int realm = BeastAndChaosDamage.BeastRealm(bear);
            Check(realm == 3, $"rank 6 beast counts as realm {realm}");
            var shooter = map.mapPawns.FreeColonistsSpawned.FirstOrDefault();
            var dinfo = new DamageInfo(DamageDefOf.Bullet, 100f, 0f, -1f, shooter, null, T("Gun_Autopistol"));
            BeastAndChaosDamage.Prefix(bear, ref dinfo);
            float expected = 100f * SpiritGradeUtility.SuppressionFactor(3, SpiritGradeUtility.AttackGrade(new DamageInfo(DamageDefOf.Bullet, 1f, 0f, -1f, shooter, null, T("Gun_Autopistol"))));
            Check(shooter == null || UnityEngine.Mathf.Abs(dinfo.Amount - expected) < 0.5f, $"a mortal pistol does {dinfo.Amount:0} of 100 to a rank 6 beast (expected {expected:0})");
            var qiDinfo = new DamageInfo(DamageDefOf.Bullet, 100f, 0f, -1f, shooter, null, T("Gun_Autopistol"));
            var dmg = DefDatabase<DamageDef>.GetNamedSilentFail("CVG_QiBomb");
            if (dmg != null)
            {
                qiDinfo = new DamageInfo(dmg, 100f, 0f, -1f, shooter);
                BeastAndChaosDamage.Prefix(bear, ref qiDinfo);
                Check(qiDinfo.Amount >= 99f, $"a qi mortal shell (grade 3) does {qiDinfo.Amount:0} of 100 to the same beast");
            }
            bear.Destroy();
        }

        private static void TestChaos(Map map)
        {
            if (map == null) return;
            var kind = DefDatabase<PawnKindDef>.GetNamedSilentFail("Noctol");
            var colonist = map.mapPawns.FreeColonistsSpawned.FirstOrDefault();
            if (kind == null || colonist == null) return;
            var entity = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, Faction.OfEntities));
            var cell = CellFinder.RandomClosewalkCellNear(map.Center, map, 10, c => c.Standable(map));
            GenSpawn.Spawn(entity, cell, map);
            int rank = Chaos.EntityRank(entity);
            var qiDmg = DefDatabase<DamageDef>.GetNamedSilentFail("CVG_QiBomb");
            if (qiDmg != null)
            {
                var d = new DamageInfo(qiDmg, 40f, 0f, -1f, colonist);
                BeastAndChaosDamage.Prefix(entity, ref d);
                Check(UnityEngine.Mathf.Abs(d.Amount - 50f) < 0.5f, $"qi damage on an entity: 40 -> {d.Amount:0} (+25%)");
            }
            var mortal = new DamageInfo(DamageDefOf.Bullet, 40f, 0f, -1f, colonist, null, T("Gun_Autopistol"));
            BeastAndChaosDamage.Prefix(entity, ref mortal);
            Check(mortal.Amount >= 39f || SpiritGradeUtility.ResearchFloor > 0, $"mortal damage on an entity is not suppressed: {mortal.Amount:0}");
            var vest = T("CVG_Apparel_QiVest");
            var resistance = DefDatabase<StatDef>.GetNamedSilentFail("CVG_ChaosResistance");
            if (vest != null && resistance != null)
            {
                var worn = (Apparel)ThingMaker.MakeThing(vest);
                colonist.apparel.Wear(worn, dropReplacedApparel: false);
                float r = colonist.GetStatValue(resistance);
                var hit = new DamageInfo(DamageDefOf.Bite, 100f, 0f, -1f, entity);
                BeastAndChaosDamage.Prefix(colonist, ref hit);
                Check(r >= 0.15f && hit.Amount <= 100f * (1f - r) + 0.5f, $"qi vest: chaos resistance {r:P0}, an entity bite of 100 does {hit.Amount:0}");
                colonist.apparel.Remove(worn);
                worn.Destroy();
            }
            int coresBefore = map.listerThings.ThingsOfDef(Chaos.ChaosCore).Sum(t => t.stackCount);
            entity.Kill(null);
            int cores = map.listerThings.ThingsOfDef(Chaos.ChaosCore).Sum(t => t.stackCount) - coresBefore;
            Check(rank < 3 || cores >= 1, $"a rank {rank} entity left {cores} chaos cores");
            foreach (var t in map.listerThings.ThingsOfDef(Chaos.ChaosCore).ToList()) t.Destroy();
            entity.Corpse?.Destroy();
        }

        private static void TestMechCore(Map map)
        {
            var core = DefDatabase<HediffDef>.GetNamedSilentFail("CVG_MechQiCore");
            var kind = DefDatabase<PawnKindDef>.GetNamedSilentFail("Mech_Militor");
            if (core == null || kind == null) return;
            var mech = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, Faction.OfMechanoids));
            var h = mech.health.AddHediff(core);
            h.Severity = 2;
            int grade = SpiritGradeUtility.HediffGrade(mech);
            var gun = mech.equipment?.Primary;
            int attack = SpiritGradeUtility.AttackGrade(new DamageInfo(DamageDefOf.Bullet, 10f, 0f, -1f, mech, null, gun?.def));
            Check(grade == 2 && attack >= 2, $"mech qi core grade {grade}: its gun attacks with grade {attack}");
            Check(mech.KindLabel.Length > kind.label.Length, $"qi mech kind label: {mech.KindLabel}");
            mech.Destroy();
        }

        private static void TestProduction()
        {
            var lab = T("CVG_QT_PillLab");
            if (lab != null && T("CVG_AlchemyFurnace") != null)
            {
                int pills = lab.AllRecipes.Count(r => r.ProducedThingDef?.IsIngestible ?? false);
                Check(pills >= 3, $"qi pill laboratory makes {pills} pills");
            }
            var basin = T("CVG_QT_SpiritHydroponics");
            if (basin != null)
            {
                Check(T("Plant_Rice")?.plant.sowTags.Contains(basin.building.sowTag) ?? false, "rice can be sown in the spirit hydroponics basin");
                var herb = DefDatabase<ThingDef>.AllDefsListForReading.FirstOrDefault(d => d.plant != null && d.defName.StartsWith("CVG_") && d.plant.sowTags.Contains("Ground"));
                Check(herb == null || herb.plant.sowTags.Contains(basin.building.sowTag), $"spirit plant {herb?.defName} can be sown in the basin");
            }
        }

        private static void TestOdyssey(Map map)
        {
            var space = DefDatabase<BiomeDef>.GetNamedSilentFail("Space");
            if (space != null) Check(space.GetModExtension<DefModExtension_MapQi>()?.ambientFactor < 1f, "space has little qi");
            var realm = DefDatabase<SitePartDef>.GetNamedSilentFail("CVG_ForgottenRealm");
            Check(space == null || realm?.GetModExtension<DefModExtension_MapQi>()?.ambientOffset > 0f, "forgotten realms are full of qi");
            Check(space == null || DefDatabase<QuestScriptDef>.GetNamedSilentFail("CVG_OpportunitySite_ForgottenRealm") != null, "forgotten realm quest for the orbital scanner");
            if (map != null) Check(MapQi.AmbientFor(map, 1f) >= 1f, $"home map ambient qi x{MapQi.AmbientFor(map, 1f)}");
            var koi = T("CVG_Fish_SilkKoi");
            if (koi != null) Check(koi.ingestible.outcomeDoers.Any(o => o is IngestionOutcomeDoer_AdvanceStage), "silk koi can raise a stage");
        }
    }
}
