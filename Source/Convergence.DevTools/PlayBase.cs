using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Partida natural del arnés (<c>-cvgplaytest -cvgplaynatural</c>, pedido del usuario 2026-10-24): en vez de regalar reinos e
    /// investigación, arma una base autosuficiente como la de un jugador (casa de piedra con techo, heladera con enfriador y
    /// paneles solares, camas, mesa, cocina, mesa de investigación, cultivo, comida y unas torretas) y deja que el narrador haga
    /// lo suyo. La misión del maestro se acepta cuando llega (eso desata la Convergencia). Sirve para ver eventos, incursiones,
    /// vínculos, ánimo y errores en una partida que se parece a jugar.
    /// </summary>
    public static class PlayBase
    {
        public static readonly bool Natural = GenCommandLine.CommandLineArgPassed("cvgplaynatural");

        private const int W = 19, H = 13;

        public static void Setup(Map map, StringBuilder log)
        {
            foreach (var name in new[] { "Electricity", "Batteries", "SolarPanels", "AirConditioning", "GunTurrets", "Smithing", "Stonecutting", "PassiveCooler", "ComplexFurniture" })
            {
                var proj = DefDatabase<ResearchProjectDef>.GetNamedSilentFail(name);
                if (proj != null && !proj.IsFinished) Find.ResearchManager.FinishProject(proj, doCompletionDialog: false, null, doCompletionLetter: false);
            }
            if (!TryFindSite(map, out var origin))
            {
                log.Append("\n  natural base: no flat site found");
                return;
            }
            int x0 = origin.x, z0 = origin.z;
            var rect = new CellRect(x0, z0, W, H);
            Clear(map, rect.ExpandedBy(6));
            var stone = ThingDef.Named("BlocksGranite");
            var wood = ThingDef.Named("WoodLog");

            // Paredes: perímetro y una interior que separa la sala (11x11) de la heladera (6x11).
            foreach (var c in rect.EdgeCells) Spawn(map, "Wall", c, Rot4.North, stone);
            for (int z = z0 + 1; z < z0 + H - 1; z++) Spawn(map, "Wall", new IntVec3(x0 + 12, 0, z), Rot4.North, stone);
            Replace(map, "Door", new IntVec3(x0 + 6, 0, z0), Rot4.North, wood);
            Replace(map, "Door", new IntVec3(x0 + 12, 0, z0 + 6), Rot4.North, wood);
            // Enfriador en la pared este de la heladera: el lado frío mira hacia adentro (oeste).
            Replace(map, "Cooler", new IntVec3(x0 + W - 1, 0, z0 + 6), Rot4.East, null);
            foreach (var c in rect.ContractedBy(1)) map.roofGrid.SetRoof(c, RoofDefOf.RoofConstructed);
            foreach (var c in rect) map.roofGrid.SetRoof(c, RoofDefOf.RoofConstructed);

            // Energía: paneles solares y baterías al este, unidos al enfriador con un conducto.
            Spawn(map, "PowerConduit", new IntVec3(x0 + W, 0, z0 + 6), Rot4.North, null);
            Spawn(map, "SolarGenerator", new IntVec3(x0 + W + 2, 0, z0 + 6), Rot4.North, null);
            Spawn(map, "SolarGenerator", new IntVec3(x0 + W + 2, 0, z0 + 2), Rot4.North, null);
            Spawn(map, "Battery", new IntVec3(x0 + W, 0, z0 + 9), Rot4.North, null);
            Spawn(map, "PowerConduit", new IntVec3(x0 + W, 0, z0 + 7), Rot4.North, null);
            Spawn(map, "PowerConduit", new IntVec3(x0 + W, 0, z0 + 5), Rot4.North, null);
            Spawn(map, "PowerConduit", new IntVec3(x0 + W, 0, z0 + 4), Rot4.North, null);

            // Dos colonos más (una colonia de cinco aguanta mejor una mala racha, como la de un jugador al mes de empezar).
            for (int k = 0; k < 2; k++)
            {
                var extra = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayer, forceGenerateNewPawn: true,
                    developmentalStages: DevelopmentalStage.Adult, tile: map.Tile));
                GenSpawn.Spawn(extra, new IntVec3(x0 + 6, 0, z0 + 3), map);
            }

            // Sala: una cama por colono contra la pared norte, mesa y sillas, cocina, carnicería, investigación y antorchas.
            int bx = x0 + 1;
            foreach (var p in map.mapPawns.FreeColonistsSpawned.ToList())
            {
                var bed = Spawn(map, "Bed", new IntVec3(bx, 0, z0 + H - 3), Rot4.North, wood) as Building_Bed;
                bed?.CompAssignableToPawn.TryAssignPawn(p);
                bx += 2;
            }
            Spawn(map, "Table2x2c", new IntVec3(x0 + 5, 0, z0 + 5), Rot4.North, wood);
            foreach (var c in new[] { new IntVec3(x0 + 4, 0, z0 + 5), new IntVec3(x0 + 7, 0, z0 + 5), new IntVec3(x0 + 5, 0, z0 + 7), new IntVec3(x0 + 6, 0, z0 + 4) })
                Spawn(map, "DiningChair", c, Rot4.North, wood);
            Spawn(map, "FueledStove", new IntVec3(x0 + 9, 0, z0 + 2), Rot4.North, null);
            Spawn(map, "ButcherSpot", new IntVec3(x0 + 2, 0, z0 + 2), Rot4.North, null);
            Spawn(map, "SimpleResearchBench", new IntVec3(x0 + 9, 0, z0 + 6), Rot4.East, wood);
            Spawn(map, "TorchLamp", new IntVec3(x0 + 1, 0, z0 + 5), Rot4.North, null);
            Spawn(map, "TorchLamp", new IntVec3(x0 + 11, 0, z0 + 9), Rot4.North, null);
            Spawn(map, "CVG_CultivationCushion", new IntVec3(x0 + 2, 0, z0 + 7), Rot4.North, null);
            Spawn(map, "HorseshoesPin", new IntVec3(x0 + 3, 0, z0 + 3), Rot4.North, null);
            // Calefacción: una fogata en la sala (la base caía en invierno sin calor y los colonos morían congelados).
            Spawn(map, "Campfire", new IntVec3(x0 + 6, 0, z0 + 9), Rot4.North, null);
            Spawn(map, "ChessTable", new IntVec3(x0 + 8, 0, z0 + 9), Rot4.North, wood);
            // Tumbas al oeste: un colono sin enterrar hunde el ánimo de todos.
            for (int k = 0; k < 4; k++) Spawn(map, "Grave", new IntVec3(x0 - 4 + 2 * (k % 2), 0, z0 + 10 + 3 * (k / 2)), Rot4.North, null);

            // Defensa: dos mini-torretas y una de qi junto a la puerta, con sacos de arena.
            Spawn(map, "Turret_MiniTurret", new IntVec3(x0 + 3, 0, z0 - 3), Rot4.North, null);
            Spawn(map, "Turret_MiniTurret", new IntVec3(x0 + 9, 0, z0 - 3), Rot4.North, null);
            Spawn(map, "CVG_QiTurret1_Turret_MiniTurret", new IntVec3(x0 + 6, 0, z0 - 3), Rot4.North, null);
            // Las mini-torretas gastan 80 W: un conducto desde los paneles pasa por debajo de las tres.
            for (int z = z0 - 3; z <= z0 + 3; z++) Spawn(map, "PowerConduit", new IntVec3(x0 + W, 0, z), Rot4.North, null);
            for (int x = x0 + 3; x < x0 + W; x++) Spawn(map, "PowerConduit", new IntVec3(x, 0, z0 - 3), Rot4.North, null);
            for (int x = x0 + 2; x <= x0 + 10; x++) if (x != x0 + 6) Spawn(map, "Sandbags", new IntVec3(x, 0, z0 - 2), Rot4.North, null);

            // Lo que un jugador ordena el primer día: cocinar, carnear, investigar, y un arma para quien no tiene.
            foreach (var table in map.listerBuildings.allBuildingsColonist.OfType<Building_WorkTable>())
                foreach (var recipeName in new[] { "CookMealSimple", "ButcherCorpseFlesh" })
                {
                    var recipe = DefDatabase<RecipeDef>.GetNamedSilentFail(recipeName);
                    if (recipe == null || !table.def.AllRecipes.Contains(recipe)) continue;
                    var bill = (Bill_Production)recipe.MakeNewBill();
                    bill.repeatMode = recipeName == "CookMealSimple" ? BillRepeatModeDefOf.TargetCount : BillRepeatModeDefOf.Forever;
                    bill.targetCount = 20;
                    table.BillStack.AddBill(bill);
                }
            foreach (var p in map.mapPawns.FreeColonistsSpawned)
                if (p.equipment != null && p.equipment.Primary == null && !p.WorkTagIsDisabled(WorkTags.Violent))
                    p.equipment.AddEquipment((ThingWithComps)ThingMaker.MakeThing(ThingDef.Named("Gun_Autopistol")));
            PickResearch();

            // Zonas: heladera para comida, depósito al oeste, cultivo de arroz al sur.
            var fridge = new Zone_Stockpile(StorageSettingsPreset.DefaultStockpile, map.zoneManager);
            map.zoneManager.RegisterZone(fridge);
            foreach (var c in new CellRect(x0 + 13, z0 + 1, 5, 11)) fridge.AddCell(c);
            fridge.settings.filter.SetDisallowAll();
            fridge.settings.filter.SetAllow(ThingCategoryDefOf.Foods, true);
            fridge.settings.filter.SetAllow(ThingCategoryDefOf.Medicine, true);
            fridge.settings.Priority = StoragePriority.Important;
            var depot = new Zone_Stockpile(StorageSettingsPreset.DefaultStockpile, map.zoneManager);
            map.zoneManager.RegisterZone(depot);
            foreach (var c in new CellRect(x0 - 7, z0, 6, 8)) if (c.InBounds(map) && c.Standable(map)) depot.AddCell(c);
            var rice = ThingDef.Named("Plant_Rice");
            var field = new Zone_Growing(map.zoneManager);
            map.zoneManager.RegisterZone(field);
            foreach (var c in new CellRect(x0, z0 - 15, 12, 9))
                if (c.InBounds(map) && map.fertilityGrid.FertilityAt(c) >= 0.7f && c.GetEdifice(map) == null) field.AddCell(c);
            if (field.Cells.Count == 0) field.Delete();
            else field.SetPlantDefToGrow(rice);

            // Provisiones: comida en la heladera, lo demás en el depósito.
            var fridgeCell = new IntVec3(x0 + 15, 0, z0 + 6);
            Drop(map, "MealSurvivalPack", 250, fridgeCell);
            Drop(map, "RawRice", 250, fridgeCell);
            Drop(map, "Meat_Cow", 150, fridgeCell);
            Drop(map, "MedicineIndustrial", 20, fridgeCell);
            var depotCell = new IntVec3(x0 - 4, 0, z0 + 4);
            Drop(map, "WoodLog", 700, depotCell);
            Drop(map, "Steel", 400, depotCell);
            Drop(map, "ComponentIndustrial", 15, depotCell);
            Drop(map, "Cloth", 150, depotCell);
            Drop(map, "CVG_SpiritStone", 60, depotCell);
            foreach (var t in map.listerThings.AllThings.ToList())
                t.TryGetComp<CompRefuelable>()?.Refuel(t.TryGetComp<CompRefuelable>().Props.fuelCapacity);
            map.fogGrid.Refog(map.BoundsRect());
            FloodFillerFog.FloodUnfog(origin + new IntVec3(6, 0, 6), map);
            foreach (var p in map.mapPawns.FreeColonistsSpawned) FloodFillerFog.FloodUnfog(p.Position, map);
            log.Append($"\n  natural base at {origin}: {map.mapPawns.FreeColonistsSpawned.Count} colonists, field {field.Cells.Count} cells, research basics done");
        }

        /// <summary>Una vez por hora: acepta la misión del maestro si está ofrecida y sigue investigando (como haría el jugador).</summary>
        public static void AcceptMentorQuest()
        {
            var quest = Find.QuestManager.QuestsListForReading.FirstOrDefault(q => q.State == QuestState.NotYetAccepted
                && q.PartsListForReading.Any(part => part is QuestPart_MentorRefuge));
            if (quest != null)
            {
                quest.Accept(null);
                Log.Message("[Convergence] playtest: accepted the mentor quest on day " + GenDate.DaysPassedSinceSettle);
            }
            if (Find.ResearchManager.GetProject() == null) PickResearch();
            ScheduleMeditation();
        }

        /// <summary>
        /// Cultivar es meditar: un jugador le pone a cada cultivador la franja Meditar del horario vanilla (tres horas a la mañana).
        /// </summary>
        private static void ScheduleMeditation()
        {
            foreach (var p in PawnsFinder.AllMaps_FreeColonistsSpawned)
            {
                var comp = CompCultivator.For(p);
                if (comp == null || p.timetable == null || !comp.ActiveTrackers.Any(t => t.Started)) continue;
                for (int h = 6; h <= 8; h++)
                    if (p.timetable.GetAssignment(h) != TimeAssignmentDefOf.Meditate) p.timetable.SetAssignment(h, TimeAssignmentDefOf.Meditate);
            }
        }

        /// <summary>El proyecto más barato que se pueda empezar, priorizando el del mod (como un jugador curioso).</summary>
        private static void PickResearch()
        {
            var next = DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where(r => r.CanStartNow && r.knowledgeCategory == null)
                .OrderBy(r => r.defName.StartsWith("CVG") ? 0 : 1).ThenBy(r => r.baseCost).FirstOrDefault();
            if (next != null) Find.ResearchManager.SetCurrentProject(next);
        }

        private static bool TryFindSite(Map map, out IntVec3 origin)
        {
            var center = map.Center;
            foreach (var c in GenRadial.RadialCellsAround(center, 40f, true))
            {
                var rect = new CellRect(c.x - 8, c.z - 16, W + 16, H + 22);
                if (!rect.InBounds(map)) continue;
                if (rect.Cells.All(x => x.GetTerrain(map) is TerrainDef t && !t.IsWater && t.affordances.Contains(TerrainAffordanceDefOf.Heavy)
                                        && (x.GetEdifice(map) == null || x.GetEdifice(map).def.building.isNaturalRock == false))
                    && new CellRect(c.x, c.z - 15, 12, 9).Cells.Count(x => map.fertilityGrid.FertilityAt(x) >= 0.7f) >= 90)
                {
                    origin = c;
                    return true;
                }
            }
            origin = center;
            return false;
        }

        private static void Clear(Map map, CellRect rect)
        {
            foreach (var c in rect)
            {
                if (!c.InBounds(map)) continue;
                foreach (var t in c.GetThingList(map).ToList())
                    if (!(t is Pawn) && t.def.destroyable && (t.def.category == ThingCategory.Building || t.def.category == ThingCategory.Plant || t.def.category == ThingCategory.Filth))
                        t.Destroy();
                map.roofGrid.SetRoof(c, null);
            }
        }

        private static Thing Spawn(Map map, string defName, IntVec3 cell, Rot4 rot, ThingDef stuff)
        {
            var def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
            if (def == null || !cell.InBounds(map)) return null;
            var t = ThingMaker.MakeThing(def, def.MadeFromStuff ? stuff ?? GenStuff.DefaultStuffFor(def) : null);
            t.SetFactionDirect(Faction.OfPlayer);
            return GenSpawn.Spawn(t, cell, map, rot, WipeMode.Vanish);
        }

        private static void Replace(Map map, string defName, IntVec3 cell, Rot4 rot, ThingDef stuff)
        {
            cell.GetEdifice(map)?.Destroy();
            Spawn(map, defName, cell, rot, stuff);
        }

        private static void Drop(Map map, string defName, int count, IntVec3 near)
        {
            var def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
            if (def == null) return;
            while (count > 0)
            {
                var t = ThingMaker.MakeThing(def);
                t.stackCount = System.Math.Min(count, def.stackLimit);
                count -= t.stackCount;
                GenPlace.TryPlaceThing(t, near, map, ThingPlaceMode.Near);
            }
        }

        /// <summary>Vínculos y relaciones del mod entre los colonos, y cuántas interacciones del mod hubo.</summary>
        public static string RelationsReport()
        {
            var sb = new StringBuilder("\n- bonds and relations (Convergence)");
            foreach (var p in PawnsFinder.AllMaps_FreeColonists)
            {
                var rels = p.relations?.DirectRelations.Where(r => r.def.defName.StartsWith("CVG")).Select(r => $"{r.def.label} of {r.otherPawn?.LabelShort}").ToList() ?? new List<string>();
                var thoughts = p.needs?.mood?.thoughts?.memories?.Memories.Where(m => m.def.defName.StartsWith("CVG")).Select(m => m.def.defName).Distinct().ToList() ?? new List<string>();
                sb.Append($"\n    {p.LabelShort}: relations [{string.Join(", ", rels)}], our memories [{string.Join(", ", thoughts)}]");
            }
            var counts = Find.PlayLog.AllEntries.OfType<PlayLogEntry_Interaction>()
                .Select(e => (InteractionDef)typeof(PlayLogEntry_Interaction).GetField("intDef", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(e))
                .Where(d => d != null && d.defName.StartsWith("CVG")).GroupBy(d => d.defName).Select(g => $"{g.Key} x{g.Count()}");
            sb.Append("\n    our interactions in the play log: " + string.Join(", ", counts));
            return sb.ToString();
        }
    }
}
