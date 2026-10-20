using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Autotest de recursos (con <c>-cvgselftest</c>): vetas de la Convergencia en el mapa antes y después de
    /// <see cref="ConvergenceMineables.OnConvergence"/>, y materiales de los módulos cargados.
    /// </summary>
    public static class ResourceSelfTest
    {
        public static void Run()
        {
            var map = Find.CurrentMap;
            var sb = new StringBuilder("[Convergence] Resource self-test");
            int fails = 0;
            var gated = DefDatabase<ThingDef>.AllDefsListForReading.Where(ConvergenceMineables.IsGated).ToList();
            sb.Append($"\n  gated veins: {gated.Select(d => d.defName).ToCommaList()}");
            if (map != null)
            {
                var jade = ThingDef.Named("MineableJade");
                int jadeBefore = map.listerThings.ThingsOfDef(jade).Count;
                var before = gated.ToDictionary(d => d, d => map.listerThings.ThingsOfDef(d).Count);
                ConvergenceMineables.OnConvergence(map);
                int jadeAfter = map.listerThings.ThingsOfDef(jade).Count;
                sb.Append($"\n  vanilla jade cells: {jadeBefore} -> {jadeAfter}");
                foreach (var d in gated)
                {
                    int after = map.listerThings.ThingsOfDef(d).Count;
                    sb.Append($"\n  {d.defName}: {before[d]} -> {after}");
                    var ext = d.GetModExtension<DefModExtension_ConvergenceMineable>();
                    if (ext.eventLumps.max > 0 && ConvergenceMineables.AllowedOn(d, map) && after <= before[d]) { sb.Append("  FAIL no new veins"); fails++; }
                }
            }
            foreach (var stuff in DefDatabase<ThingDef>.AllDefsListForReading.Where(d => d.IsStuff && d.defName.StartsWith("CVG_")))
                sb.Append($"\n  stuff {stuff.defName}: {stuff.stuffProps.categories.Select(c => c.defName).ToCommaList()}, value {stuff.BaseMarketValue}");
            foreach (var terrain in DefDatabase<TerrainDef>.AllDefsListForReading.Where(t => t.HasModExtension<DefModExtension_FengShui>()))
                sb.Append($"\n  floor {terrain.defName}: qi {terrain.GetModExtension<DefModExtension_FengShui>().qi} per cell");
            fails += TestRocks(sb, map);
            fails += TestAlchemy(sb, map);
            fails += TestTrees(sb, map);
            sb.Append(fails == 0 ? "\n  ALL OK" : "\n  FAILURES: " + fails);
            if (fails == 0) Log.Message(sb.ToString());
            else Log.Error(sb.ToString());
        }

        /// <summary>Árboles: melocotonero frutal (fruta al cosechar y rebrota; madera al cortar) y pino milenario.</summary>
        private static int TestTrees(StringBuilder sb, Map map)
        {
            int fails = 0;
            var peachTree = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_Plant_PeachTree");
            if (peachTree != null && map != null)
            {
                var p = peachTree.plant;
                bool fruit = p.harvestedThingDef?.defName == "CVG_Peach" && !p.HarvestDestroys && peachTree.thingClass == typeof(Plant_FruitTree);
                sb.Append("\n  " + $"{(fruit ? "ok  " : "FAIL")} peach tree gives {p.harvestedThingDef?.defName} and grows back ({p.harvestAfterGrowth})");
                if (!fruit) fails++;
                var cell = CellFinder.RandomClosewalkCellNear(map.Center, map, 10, c => c.Standable(map) && c.GetFirstItem(map) == null && c.GetPlant(map) == null);
                var tree = (Plant)GenSpawn.Spawn(ThingMaker.MakeThing(peachTree), cell, map);
                tree.Growth = 1f;
                var cutter = map.mapPawns.FreeColonistsSpawned.FirstOrDefault();
                int timberBefore = map.listerThings.ThingsOfDef(ThingDef.Named("CVG_PeachTimber")).Sum(t => t.stackCount);
                if (cutter != null) tree.PlantCollected(cutter, PlantDestructionMode.Cut);
                int timber = map.listerThings.ThingsOfDef(ThingDef.Named("CVG_PeachTimber")).Sum(t => t.stackCount) - timberBefore;
                bool woodOk = cutter == null || timber > 0;
                sb.Append("\n  " + $"{(woodOk ? "ok  " : "FAIL")} cutting a grown peach tree gives {timber} peach timber");
                if (!woodOk) fails++;
                if (!tree.Destroyed) tree.Destroy();
            }
            var pine = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_Plant_MillennialPine");
            if (pine != null)
            {
                bool pineOk = pine.plant.harvestedThingDef?.defName == "CVG_SpiritWood";
                sb.Append("\n  " + $"{(pineOk ? "ok  " : "FAIL")} millennial pine gives {pine.plant.harvestedThingDef?.defName} x{pine.plant.harvestYield}");
                if (!pineOk) fails++;
            }
            return fails;
        }

        /// <summary>Rocas wuxia, ajuste "Mundo wuxia", qi por material y clima de las vetas.</summary>
        private static int TestRocks(StringBuilder sb, Map map)
        {
            int fails = 0;
            var rocks = DefDatabase<ThingDef>.AllDefsListForReading.Where(d => d.HasModExtension<DefModExtension_WuxiaRock>()).ToList();
            if (rocks.Count == 0 || map == null) return 0;
            var vanilla = Find.World.NaturalRockTypesIn(map.Tile).Select(d => d.defName).ToList();
            bool old = ConvergenceMod.Settings.wuxiaRocksOnly;
            ConvergenceMod.Settings.wuxiaRocksOnly = true;
            var wuxia = Find.World.NaturalRockTypesIn(map.Tile).ToList();
            ConvergenceMod.Settings.wuxiaRocksOnly = old;
            sb.Append($"\n  wuxia rocks: {rocks.Count} (terrain {rocks[0].building.naturalTerrain?.defName}); tile rocks off [{vanilla.ToCommaList()}], on [{wuxia.Select(d => d.defName).ToCommaList()}]");
            if (wuxia.Count < 2 || wuxia.Any(d => !rocks.Contains(d))) { sb.Append("  FAIL wuxia world"); fails++; }

            var blocks = DefDatabase<ThingDef>.AllDefsListForReading.FirstOrDefault(d => d.IsStuff && d.stuffProps.categories.Contains(StuffCategoryDefOf.Stony)
                                                                                      && d.HasModExtension<DefModExtension_FengShui>());
            if (blocks != null)
            {
                var wall = ThingMaker.MakeThing(ThingDefOf.Wall, blocks);
                float qi = QiConcentrationUtility.FengShuiQi(wall);
                sb.Append($"\n  wall of {blocks.defName}: qi {qi:0.###}");
                if (qi <= 0f) { sb.Append("  FAIL material qi"); fails++; }
            }
            var allowed = DefDatabase<ThingDef>.AllDefsListForReading.Where(d => ConvergenceMineables.IsGated(d) && ConvergenceMineables.AllowedOn(d, map)).Select(d => d.defName);
            sb.Append($"\n  veins allowed on this map: {allowed.ToCommaList()}");
            if (ConvergenceMineables.AllowedOn(DefDatabase<ThingDef>.GetNamedSilentFail("CVG_MineableSolarGold"), map) &&
                !new[] { "Desert", "ExtremeDesert", "AridShrubland" }.Contains(map.Tile.Tile.PrimaryBiome.defName)) { sb.Append("  FAIL biome-only vein"); fails++; }
            var coldIron = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_MineableColdIron");
            if (coldIron != null)
                sb.Append($"\n  cold iron allowed here ({map.Tile.Tile.PrimaryBiome.defName}, {map.Tile.Tile.temperature:0}C): {ConvergenceMineables.AllowedOn(coldIron, map)}");
            return fails;
        }

        /// <summary>Mesas con niveles, recetas por nivel, runas y calidad de píldoras (si el módulo de alquimia está cargado).</summary>
        private static int TestAlchemy(StringBuilder sb, Map map)
        {
            int fails = 0;
            var forgeDef = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_SpiritForge");
            if (forgeDef == null || map == null) return 0;
            var forge = (ThingWithComps)ThingMaker.MakeThing(forgeDef, GenStuff.DefaultStuffFor(forgeDef));
            GenSpawn.Spawn(forge, CellFinder.RandomClosewalkCellNear(map.Center, map, 8), map);
            forge.SetFaction(Faction.OfPlayer);
            var level = forge.GetComp<CompWorkbenchLevel>();
            var flying = DefDatabase<RecipeDef>.AllDefsListForReading.FirstOrDefault(r => r.ProducedThingDef?.defName == "CVG_FlyingSword");
            bool atOne = flying != null && flying.AvailableOnNow(forge);
            level.SetLevel(3);
            bool atThree = flying != null && flying.AvailableOnNow(forge);
            sb.Append($"\n  spirit forge: levels {level.Props.levels.Count}, flying sword recipe at level 1 {atOne}, at level 3 {atThree}, " +
                      $"graphic {forge.Graphic?.path}, speed x{forge.GetStatValue(StatDefOf.WorkTableWorkSpeedFactor):0.##}");
            if (flying == null || atOne || !atThree) { sb.Append("  FAIL level-gated recipe"); fails++; }
            forge.Destroy();

            var weapon = (ThingWithComps)ThingMaker.MakeThing(ThingDef.Named("MeleeWeapon_Knife"), ThingDefOf.Steel);
            int before = SpiritGradeUtility.GradeOf(weapon);
            new RecipeWorker_EngraveRunes().ConsumeIngredient(weapon, null, null);
            int after = SpiritGradeUtility.GradeOf(weapon);
            sb.Append($"\n  runes on a knife: grade {before} -> {after}");
            if (after != 1) { sb.Append("  FAIL runes"); fails++; }

            var pillDef = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_QiRecoveryPill");
            if (pillDef != null)
            {
                var pill = ThingMaker.MakeThing(pillDef);
                pill.TryGetComp<CompQuality>()?.SetQuality(QualityCategory.Masterwork, ArtGenerationContext.Colony);
                sb.Append($"\n  masterwork pill factor x{PillQuality.FactorOf(pill):0.##}");
                if (PillQuality.FactorOf(pill) <= 1f) { sb.Append("  FAIL pill quality"); fails++; }
            }
            return fails;
        }
    }
}
