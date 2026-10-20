using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>Autotest de cultivos (parte 4b) y defensa diaria (parte 5) de DIA_A_DIA.md: solo con <c>-cvgcropstest</c> (y un mapa, p. ej. -quicktest).</summary>
    public class GameComponent_CropsDefenseTest : GameComponent
    {
        private static readonly bool Enabled = GenCommandLine.CommandLineArgPassed("cvgcropstest");
        private bool done;
        private readonly StringBuilder sb = new StringBuilder("[Convergence] Crops and daily defense self-test");
        private int fails;
        private readonly List<Thing> spawned = new List<Thing>();

        public GameComponent_CropsDefenseTest(Game game) { }

        private void Check(bool ok, string what)
        {
            sb.Append("\n    " + (ok ? "ok   " : "FAIL ") + what);
            if (!ok) fails++;
        }

        public override void GameComponentTick()
        {
            if (!Enabled || done || Find.CurrentMap == null || Find.TickManager.TicksGame < 200) return;
            done = true;
            try { Run(Find.CurrentMap); }
            catch (Exception e) { Check(false, "exception: " + e); }
            foreach (var t in spawned) if (!t.Destroyed) t.Destroy();
            sb.Append(fails == 0 ? "\n  ALL OK" : "\n  FAILURES: " + fails);
            if (fails == 0) Log.Message(sb.ToString()); else Log.Error(sb.ToString());
        }

        private Thing Spawn(string def, IntVec3 cell, Map map)
        {
            var d = DefDatabase<ThingDef>.GetNamed(def);
            var t = ThingMaker.MakeThing(d, d.MadeFromStuff ? GenStuff.DefaultStuffFor(d) : null);
            if (t.def.CanHaveFaction) t.SetFactionDirect(Faction.OfPlayer);
            GenSpawn.Spawn(t, cell, map);
            spawned.Add(t);
            return t;
        }

        private void Run(Map map)
        {
            if (DefDatabase<ThingDef>.GetNamedSilentFail("CVG_Plant_GlutinousRice") == null) { sb.Append("\n  (Fauna & Flora not loaded: skipped)"); return; }
            var center = map.Center;
            foreach (var c in GenRadial.RadialCellsAround(center, 10f, true).Where(x => x.InBounds(map)))
            {
                map.roofGrid.SetRoof(c, null);
                map.terrainGrid.SetTerrain(c, TerrainDefOf.Soil);
                foreach (var t in c.GetThingList(map).ToList()) if (t.def.category != ThingCategory.Pawn && !(t is Corpse) && t.def.destroyable) t.Destroy();
            }
            var crops = new[] { "CVG_Plant_GlutinousRice", "CVG_Plant_CloudTea", "CVG_Plant_Lotus", "CVG_Plant_Mulberry", "CVG_Plant_MillennialPine" };
            int i = 0;
            foreach (var name in crops)
            {
                var def = DefDatabase<ThingDef>.GetNamed(name);
                var cell = center + new IntVec3(-6 + 3 * i++, 0, 0);
                bool canPlant = def.CanEverPlantAt(cell, map, out Thing blocker);
                if (name == "CVG_Plant_Lotus") { Check(!canPlant, "the lotus does not grow on dry soil (only in shallow water)"); canPlant = true; }
                Check(canPlant, name + " can grow on soil" + (canPlant ? "" : $" (terrain {cell.GetTerrain(map)?.defName}, fertility {map.fertilityGrid.FertilityAt(cell):0.##}, roofed {cell.Roofed(map)}, things [{string.Join(",", cell.GetThingList(map).Select(t => t.def.defName))}]" + (blocker == null ? "" : ", blocked by " + blocker.def.defName) + ")"));
                var plant = (Plant)Spawn(name, cell, map);
                plant.Growth = 1f;
                Check(plant.def.plant.harvestedThingDef != null && plant.YieldNow() > 0, name + " yields " + plant.YieldNow() + " " + plant.def.plant.harvestedThingDef?.defName);
            }
            Check(DefDatabase<ThingDef>.GetNamed("CVG_Plant_GlutinousRice").plant.humanFoodPlant && DefDatabase<ThingDef>.GetNamed("CVG_GlutinousRice").ingestible != null, "glutinous rice is human food");
            Check(DefDatabase<ThingDef>.GetNamed("CVG_Plant_MillennialPine").plant.IsTree, "millennial pine is a tree");
            var wool = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_SevenColorWool");
            if (wool != null)
            {
                var colors = new HashSet<UnityEngine.Color>();
                for (int k = 0; k < 12; k++) colors.Add(ThingMaker.MakeThing(wool).DrawColor);
                Check(colors.Count > 1, "seven-color wool comes out in different colors (" + colors.Count + " seen)");
            }
            var pine = DefDatabase<ThingDef>.GetNamed("CVG_Plant_MillennialPine");
            Check(DefDatabase<BiomeDef>.GetNamed("TemperateForest").CommonalityOfPlant(pine) > 0f, "the pine grows wild in the temperate forest");
            var silk = DefDatabase<RecipeDef>.GetNamedSilentFail("CVG_RaiseSilkworms");
            if (DefDatabase<ThingDef>.GetNamedSilentFail("CVG_SpiritSilk") != null)
                Check(silk != null && silk.products.Any(p => p.thingDef.defName == "CVG_SpiritSilk") && silk.ingredients.Any(g => g.filter.Allows(DefDatabase<ThingDef>.GetNamed("CVG_MulberryLeaf"))), "mulberry leaves become spirit silk");
            else Check(silk == null, "silkworm recipe absent without Apparel");

            // comida: el pastel de arroz es de aquí; el resto de la parte 4 (comidas, té, píldora de ayuno) es del mod de alquimia
            var cakeRecipe = DefDatabase<RecipeDef>.GetNamed("CVG_CookRiceCake");
            Check(cakeRecipe.products.Any(p => p.thingDef.defName == "CVG_RiceCake") && cakeRecipe.recipeUsers.Any(u => u.defName == "FueledStove"), "the rice cake has a stove recipe");
            var cake = ThingDefOf.MealSimple != null ? DefDatabase<ThingDef>.GetNamed("CVG_RiceCake") : null;
            Check(cake.ingestible.preferability == FoodPreferability.MealSimple && cake.GetCompProperties<CompProperties_Rottable>().daysToRotStart >= 40f, "the rice cake keeps for weeks");
            Check(DefDatabase<ThingDef>.GetNamedSilentFail("CVG_CloudTea") == null || DefDatabase<ThingDef>.AllDefsListForReading.Count(d => d.defName == "CVG_CloudTea") == 1, "a single cloud tea definition");

            // defensa
            var pal = Spawn("CVG_BambooPalisade", center + new IntVec3(0, 0, 4), map);
            var wall = Spawn("CVG_LowStoneWall", center + new IntVec3(2, 0, 4), map);
            Check(pal.def.passability == Traversability.PassThroughOnly && pal.def.fillPercent >= 0.5f && pal.def.building.isFence, "bamboo palisade: low cover, a fence for animals");
            Check(wall.MaxHitPoints > pal.MaxHitPoints && wall.GetStatValue(StatDefOf.Flammability) < 0.01f, "low stone wall: tougher and fireproof (" + wall.MaxHitPoints + " vs " + pal.MaxHitPoints + " hp)");
            var vanillaSpike = Spawn("TrapSpike", center + new IntVec3(7, 0, 4), map);
            var trap = Spawn("CVG_QiSpikeTrap", center + new IntVec3(4, 0, 4), map);
            Check(trap is Building_TrapDamager && trap.def.building.isTrap && trap.GetStatValue(StatDefOf.TrapMeleeDamage) >= vanillaSpike.GetStatValue(StatDefOf.TrapMeleeDamage), "qi spike trap is a damaging trap (" + trap.GetType().Name + ", isTrap " + trap.def.building.isTrap + ", dmg " + trap.GetStatValue(StatDefOf.TrapMeleeDamage) + ")");
            // presión de aura: un hostil de reino superior al mejor de la colonia avisa con una carta
            var kind = DefDatabase<PawnKindDef>.GetNamedSilentFail("CVG_BloodMoon_Elder");
            var cult = DefDatabase<FactionDef>.GetNamedSilentFail("CVG_Faction_BloodMoon");
            if (kind != null && cult != null && Find.FactionManager.FirstFactionOfDef(cult) is Faction bm)
            {
                GameComponent_Convergence.Instance?.MarkOccurred();
                var elder = PawnGenerator.GeneratePawn(kind, bm);
                GenSpawn.Spawn(elder, center + new IntVec3(0, 0, 8), map);
                spawned.Add(elder);
                int colonyLevel = CultivatorPawnKindUtility.ColonyMaxRealm();
                int elderLevel = CompCultivator.For(elder)?.RealmLevel ?? 0;
                bool sent = AuraPressure.TryNotify(map, bm);
                Check(sent == (elderLevel > colonyLevel), "aura pressure letter " + (sent ? "sent" : "not sent") + " (elder realm " + elderLevel + " vs colony " + colonyLevel + ")");
            }
            // agua poco profunda: el loto crece y se puede crear una zona de cultivo; el agua profunda no
            var lotus = DefDatabase<ThingDef>.GetNamed("CVG_Plant_Lotus");
            var shallowCell = center + new IntVec3(-3, 0, 7); var deepCell = center + new IntVec3(-1, 0, 7); var marshCell = center + new IntVec3(1, 0, 7); var oceanCell = center + new IntVec3(3, 0, 7);
            map.terrainGrid.SetTerrain(shallowCell, DefDatabase<TerrainDef>.GetNamed("WaterShallow"));
            map.terrainGrid.SetTerrain(deepCell, DefDatabase<TerrainDef>.GetNamed("WaterDeep"));
            map.terrainGrid.SetTerrain(marshCell, DefDatabase<TerrainDef>.GetNamed("Marsh"));
            map.terrainGrid.SetTerrain(oceanCell, DefDatabase<TerrainDef>.GetNamed("WaterOceanShallow"));
            Check(lotus.CanEverPlantAt(shallowCell, map), "the lotus can be planted in shallow fresh water");
            Check(lotus.CanEverPlantAt(marshCell, map), "the lotus can be planted in a marsh");
            Check(!lotus.CanEverPlantAt(deepCell, map), "the lotus cannot be planted in deep water");
            Check(!lotus.CanEverPlantAt(oceanCell, map), "the lotus cannot be planted in the ocean");
            var designator = new Designator_ZoneAdd_Growing();
            Check(designator.CanDesignateCell(shallowCell).Accepted && designator.CanDesignateCell(marshCell).Accepted, "a growing zone can be drawn over shallow water and marsh");
            Check(!designator.CanDesignateCell(deepCell).Accepted && !designator.CanDesignateCell(oceanCell).Accepted, "a growing zone cannot be drawn over deep water or the ocean");
            var lotusPlant = (Plant)Spawn("CVG_Plant_Lotus", shallowCell, map);
            Check(PlantUtility.GrowthRateFactorFor_Fertility(lotus, map.fertilityGrid.FertilityAt(shallowCell)) > 0.99f, "the lotus grows at full rate in water (it ignores fertility)");
            // hongos: solo crecen sin luz
            foreach (var n in new[] { "CVG_Plant_SpiritFungus", "CVG_Plant_GlowMushroom" })
            {
                var md = DefDatabase<ThingDef>.GetNamedSilentFail(n);
                if (md == null) continue;
                Check(md.plant.cavePlant && md.plant.growMinGlow == 0f && md.plant.growOptimalGlow == 0f && md.plant.diesToLight, n + " is a cave plant: it only grows in the dark and dies in the light");
            }
            // muros con su arte tal cual, sin el tinte del material
            Check(pal.DrawColor == UnityEngine.Color.white && wall.DrawColor == UnityEngine.Color.white, "the low stone wall and the bamboo palisade are not tinted by their material");
            var plainWall = Spawn("Wall", center + new IntVec3(5, 0, 7), map);
            Check(plainWall.DrawColor != UnityEngine.Color.white, "a vanilla stone wall keeps its material tint");

            Check(DefDatabase<ThingDef>.GetNamed("CVG_ThunderBallistaTurret").building.turretGunDef.Verbs[0].range > DefDatabase<ThingDef>.GetNamed("CVG_Gun_RuneCrossbow").Verbs[0].range * 1.4f, "thunder ballista outranges the rune crossbow");
            Check(new[] { pal, wall, trap }.All(t => t.def.designationCategory?.defName == "Security"), "all three are in the Security menu");
        }
    }
}
