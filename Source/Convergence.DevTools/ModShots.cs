using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Capturas para las páginas de Workshop de los módulos: con <c>-cvgmodshots</c> (o <c>-cvgmodshots=alchemy,races</c>) y
    /// <c>run-test.ps1 -Visible -Language English -Width 1920 -Height 1080</c>, arma para cada módulo activo hasta cinco escenas con su
    /// propio contenido (edificios en una sala, objetos, criaturas, personas, plantas, vetas, la ficha de lore de una pieza y escenas
    /// propias: sala de alquimia, conjuntos de ropa, rasgos, injertos, eventos, isla) y las guarda en
    /// <c>&lt;datos&gt;/cvg_modshots/&lt;módulo&gt;/</c>. Cierra el juego al terminar. Sin el argumento no hace nada.
    /// </summary>
    public static class ModShots
    {
        private static bool started;
        private static IEnumerator<int> script;
        private static int wait;
        private static string root;
        private static Map map;
        private static IntVec3 center;
        private static readonly List<Thing> spawned = new List<Thing>();
        private const int Half = 17;

        private static readonly string[] Modules =
        {
            "alchemy", "furniture", "apparel", "resources", "faunaflora", "formations", "races", "traitsdestinies", "grafts",
            "stories", "biotechwuxia", "space",
        };

        public static void Update()
        {
            if (!started)
            {
                if (Current.ProgramState != ProgramState.Playing || Find.CurrentMap == null) return;
                if (!GenCommandLine.CommandLineArgPassed("cvgmodshots") && !GenCommandLine.TryGetCommandLineArg("cvgmodshots", out _)) { started = true; return; }
                if (Find.TickManager.TicksGame < 120) return;
                started = true;
                root = Path.Combine(GenFilePaths.SaveDataFolderPath, "cvg_modshots");
                if (Directory.Exists(root)) Directory.Delete(root, true);
                Directory.CreateDirectory(root);
                script = Script();
                wait = 30;
                return;
            }
            if (script == null || --wait > 0) return;
            try
            {
                if (script.MoveNext()) wait = Mathf.Max(1, script.Current);
                else script = null;
            }
            catch (Exception e)
            {
                script = null;
                Log.Error("[Convergence] ModShots: " + e + "\n  FAILURES: 1");
                Root.Shutdown();
            }
        }

        private static IEnumerator<int> Script()
        {
            map = Find.CurrentMap;
            var tm = Find.TickManager;
            tm.CurTimeSpeed = TimeSpeed.Paused;
            int hour = GenLocalDate.HourOfDay(map);
            tm.DebugSetTicksGame(tm.TicksGame + (12 - hour + 24) % 24 * GenDate.TicksPerHour);
            map.weatherManager.curWeather = WeatherDefOf.Clear;
            map.weatherManager.lastWeather = WeatherDefOf.Clear;
            Prefs.AdaptiveTrainingEnabled = false;
            Prefs.DevMode = false;
            map.fogGrid.ClearAllFog();
            Find.CameraDriver.config.sizeRange = new FloatRange(2f, 80f);
            // Fichas y ventanas legibles en una captura de 1080p.
            Prefs.UIScale = 1.5f;
            if (!GameComponent_Convergence.Active) GameComponent_Convergence.Instance?.TryFire(map);
            yield return 20;
            CleanScreen();
            // Lejos de los colonos del quicktest (arrancan en el centro).
            center = new IntVec3(map.Size.x / 2, 0, Mathf.Max(Half + 2, map.Size.z / 2 - 55));

            var only = GenCommandLine.TryGetCommandLineArg("cvgmodshots", out string list) && !list.NullOrEmpty()
                ? list.Split(',').Select(s => s.Trim().ToLowerInvariant()).ToList() : null;
            int total = 0;
            foreach (var module in Modules)
            {
                if (only != null && !only.Contains(module)) continue;
                string pkg = "prospero1088.convergence." + module;
                if (ModLister.GetActiveModWithIdentifier(pkg, true) == null) continue;
                int shots = 0;
                foreach (var scene in Scenes(module, pkg))
                {
                    if (shots >= 5) break;
                    bool ok = false;
                    var steps = scene.Item2();
                    while (true)
                    {
                        int step;
                        try
                        {
                            if (!steps.MoveNext()) break;
                            step = steps.Current;
                        }
                        catch (Exception e)
                        {
                            Log.Warning("[Convergence] ModShots: " + module + "/" + scene.Item1 + " failed: " + e.Message);
                            break;
                        }
                        if (step < 0) { ok = true; break; }
                        yield return step;
                    }
                    if (ok)
                    {
                        yield return 2;
                        // la captura: el guion se detuvo con -1 justo después de preparar la escena
                        shots++;
                        total++;
                        Capture(module, shots.ToString("00") + "_" + scene.Item1);
                        yield return 12;
                    }
                    CleanUp();
                    yield return 5;
                }
                Log.Message("[Convergence] ModShots: " + module + " -> " + shots + " screenshots");
            }
            Log.Message($"[Convergence] ModShots: {total} screenshots in {root} ({Screen.width}x{Screen.height})\n  ALL OK");
            yield return 30;
            Root.Shutdown();
        }

        // ---------- Escenas ----------

        /// <summary>Escenas candidatas en orden de prioridad (nombre, guion). Un guion termina con <c>yield return -1</c> cuando está listo para la foto.</summary>
        private static IEnumerable<(string, Func<IEnumerator<int>>)> Scenes(string module, string pkg)
        {
            var defs = DefDatabase<ThingDef>.AllDefsListForReading.Where(d => d.modContentPack?.PackageId?.Equals(pkg, StringComparison.OrdinalIgnoreCase) == true).ToList();
            var buildings = defs.Where(IsShowBuilding).OrderByDescending(d => d.size.x * d.size.z).ThenBy(d => d.defName).ToList();
            var items = defs.Where(IsShowItem).OrderBy(d => d.IsWeapon ? 0 : d.IsApparel ? 1 : 2).ThenBy(d => d.defName).ToList();
            var plants = defs.Where(d => d.category == ThingCategory.Plant && d.plant != null).OrderBy(d => d.defName).ToList();
            var rocks = defs.Where(d => d.building != null && (d.building.isNaturalRock || d.building.isResourceRock)).OrderBy(d => d.defName).ToList();
            var kinds = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(k => k.modContentPack?.PackageId?.Equals(pkg, StringComparison.OrdinalIgnoreCase) == true
                                                                                 && k.race?.race != null && !k.RaceProps.Humanlike)
                .GroupBy(k => k.race).Select(g => g.First()).OrderBy(k => k.defName).ToList();
            var xenos = ModsConfig.BiotechActive
                ? DefDatabase<XenotypeDef>.AllDefsListForReading.Where(x => x.modContentPack?.PackageId?.Equals(pkg, StringComparison.OrdinalIgnoreCase) == true).OrderBy(x => x.defName).ToList()
                : new List<XenotypeDef>();

            switch (module)
            {
                case "alchemy":
                    yield return ("alchemy_room", AlchemyRoom);
                    yield return ("lore", () => Lore(PillThing("CVG_NineTurnsPill")));
                    break;
                case "apparel":
                    yield return ("outfits", () => Outfits(items.Where(d => d.IsApparel).ToList(), 0));
                    yield return ("outfits_2", () => Outfits(items.Where(d => d.IsApparel).ToList(), 3));
                    break;
                case "races":
                    yield return ("bloodlines", () => People(xenos.Take(6).ToList()));
                    if (xenos.Count > 6) yield return ("bloodlines_2", () => People(xenos.Skip(6).Take(6).ToList()));
                    yield return ("bloodline_card", () => PawnCard(xenos.FirstOrDefault()));
                    break;
                case "traitsdestinies":
                    yield return ("traits", () => Traits(pkg, 0));
                    yield return ("traits_2", () => Traits(pkg, 1));
                    yield return ("traits_3", () => Traits(pkg, 2));
                    break;
                case "grafts":
                    yield return ("grafts_health", () => Grafts(pkg));
                    break;
                case "stories":
                    yield return ("event", () => StoryEvent(0));
                    yield return ("event_2", () => StoryEvent(1));
                    yield return ("secret_realm_gate", RealmGate);
                    break;
                case "biotechwuxia":
                    yield return ("puppets", () => Creatures(kinds));
                    if (xenos.Count > 0) yield return ("bloodlines", () => People(xenos));
                    break;
                case "space":
                    yield return ("flying_island", Island);
                    break;
                case "formations":
                    yield return ("formation", Formation);
                    break;
                case "furniture":
                    yield return ("sect_hall", FurnishedHall);
                    break;
                case "faunaflora":
                    yield return ("spirit_beasts", () => Creatures(kinds.Take(14).ToList()));
                    if (kinds.Count > 14) yield return ("spirit_beasts_2", () => Creatures(kinds.Skip(14).Take(14).ToList()));
                    break;
            }
            if (rocks.Count > 0) yield return ("veins", () => Rocks(rocks));
            if (buildings.Count > 0) yield return ("buildings", () => Buildings(buildings, 0));
            if (plants.Count > 0) yield return ("plants", () => Plants(plants));
            if (kinds.Count > 0 && module != "faunaflora" && module != "biotechwuxia") yield return ("creatures", () => Creatures(kinds));
            var gear = items.Where(d => d.IsWeapon || d.IsApparel).ToList();
            var goods = items.Where(d => !d.IsWeapon && !d.IsApparel).ToList();
            if (gear.Count > 0) yield return ("gear", () => Items(gear));
            if (goods.Count > 0) yield return ("goods", () => Items(goods));
            if (buildings.Count > 24) yield return ("buildings_2", () => Buildings(buildings, 1));
            var hero = buildings.FirstOrDefault(d => (d.description?.Length ?? 0) > 120) ?? items.FirstOrDefault(d => (d.description?.Length ?? 0) > 120);
            if (hero != null) yield return ("lore", () => Lore(MakeThing(hero)));
        }

        private static bool IsShowBuilding(ThingDef d) =>
            d.category == ThingCategory.Building && d.BuildableByPlayer && !d.IsFrame && !d.IsBlueprint && !d.IsDoor
            && d.graphicData != null && !d.graphicData.Linked && d.size.x <= 6 && d.size.z <= 6 && d.building != null && !d.building.isNaturalRock;

        private static bool IsShowItem(ThingDef d) =>
            d.category == ThingCategory.Item && !d.IsCorpse && !d.isUnfinishedThing && !d.IsBlueprint && !d.IsFrame && d.graphicData != null
            && d.thingClass != typeof(MinifiedThing) && !d.destroyOnDrop && !d.label.NullOrEmpty();

        private static IEnumerator<int> Buildings(List<ThingDef> all, int page)
        {
            var defs = all.Skip(page * 24).Take(24).ToList();
            var rect = Prepare("WoodPlankFloor");
            var area = CellRect.CenteredOn(center, 12, 9);
            var placedRect = Pack(defs, area, (d, cell) => Put(d, cell, d.rotatable ? Rot4.South : Rot4.North));
            Colonist(new IntVec3(placedRect.CenterCell.x, 0, placedRect.minZ - 2));
            FrameSpawned(2);
            Walls(CellRect.FromLimits(placedRect.minX - 2, placedRect.minZ - 4, placedRect.maxX + 2, placedRect.maxZ + 2));
            yield return 30;
            HideUI(true);
            yield return -1;
        }

        private static IEnumerator<int> Items(List<ThingDef> all)
        {
            var defs = all.Take(40).ToList();
            var rect = Prepare("TileSandstone");
            int cols = Mathf.Min(7, defs.Count);
            int rows = (defs.Count + cols - 1) / cols;
            var origin = center + new IntVec3(-cols, 0, rows - 1);
            for (int i = 0; i < defs.Count; i++)
            {
                var t = MakeThing(defs[i]);
                if (t.def.stackLimit > 1) t.stackCount = Mathf.Min(t.def.stackLimit, 20);
                Spawn(t, origin + new IntVec3(i % cols * 2, 0, -(i / cols) * 2), Rot4.North);
            }
            FrameSpawned(1);
            yield return 30;
            HideUI(true);
            yield return -1;
        }

        private static IEnumerator<int> Plants(List<ThingDef> all)
        {
            var defs = all.Take(16).ToList();
            Prepare(null, TerrainDefOf.Soil);
            int cols = Mathf.Min(4, defs.Count);
            int rows = (defs.Count + cols - 1) / cols;
            var origin = center + new IntVec3(-cols * 2, 0, rows * 2);
            for (int i = 0; i < defs.Count; i++)
            {
                var basis = origin + new IntVec3(i % cols * 4, 0, -(i / cols) * 4);
                foreach (var off in new[] { IntVec3.Zero, IntVec3.East, IntVec3.North, new IntVec3(1, 0, 1) })
                {
                    var cell = basis + off;
                    if (defs[i].plant.IsTree && off != IntVec3.Zero) continue;
                    var p = (Plant)ThingMaker.MakeThing(defs[i]);
                    p.Growth = 1f;
                    Spawn(p, cell, Rot4.North);
                }
            }
            FrameSpawned(1);
            yield return 30;
            HideUI(true);
            yield return -1;
        }

        private static IEnumerator<int> Rocks(List<ThingDef> defs)
        {
            Prepare(null, TerrainDefOf.Soil);
            int cols = Mathf.Min(5, defs.Count);
            int rows = (defs.Count + cols - 1) / cols;
            var origin = center + new IntVec3(-cols * 2, 0, rows * 2);
            for (int i = 0; i < defs.Count; i++)
            {
                var basis = origin + new IntVec3(i % cols * 4, 0, -(i / cols) * 4);
                foreach (var c in CellRect.CenteredOn(basis, 1)) Spawn(ThingMaker.MakeThing(defs[i]), c, Rot4.North);
            }
            FrameSpawned(1);
            yield return 30;
            HideUI(true);
            yield return -1;
        }

        private static IEnumerator<int> Creatures(List<PawnKindDef> kinds)
        {
            Prepare(null, TerrainDefOf.Soil);
            int x = -9, z = 4, rowH = 0, count = 0;
            foreach (var k in kinds)
            {
                int step = 2 + Mathf.CeilToInt(k.RaceProps.baseBodySize);
                if (x + step > 9) { x = -9; z -= rowH + 1; rowH = 0; }
                Pawn p;
                try
                {
                    p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(k, k.RaceProps.Animal ? null : Faction.OfPlayer, forceGenerateNewPawn: true, tile: map.Tile));
                }
                catch (Exception e)
                {
                    Log.Warning("[Convergence] ModShots: could not generate " + k.defName + ": " + e.Message);
                    continue;
                }
                Spawn(p, center + new IntVec3(x, 0, z), Rot4.South);
                count++;
                x += step;
                rowH = Mathf.Max(rowH, step);
            }
            if (count == 0) yield break;
            FrameSpawned(2);
            yield return 40;
            HideUI(true);
            yield return -1;
        }

        private static IEnumerator<int> People(List<XenotypeDef> xenos)
        {
            if (xenos.Count == 0) yield break;
            Prepare("WoodPlankFloor");
            int x = -(xenos.Count - 1);
            foreach (var xeno in xenos)
            {
                var p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayer, forceGenerateNewPawn: true,
                    developmentalStages: DevelopmentalStage.Adult, forcedXenotype: xeno, tile: map.Tile));
                Spawn(p, center + new IntVec3(x, 0, 0), Rot4.South);
                x += 2;
            }
            FrameSpawned(2);
            yield return 40;
            HideUI(true);
            yield return -1;
        }

        private static IEnumerator<int> PawnCard(XenotypeDef xeno)
        {
            if (xeno == null) yield break;
            Prepare("WoodPlankFloor");
            var p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayer, forceGenerateNewPawn: true,
                developmentalStages: DevelopmentalStage.Adult, forcedXenotype: xeno, tile: map.Tile));
            Spawn(p, center, Rot4.South);
            Find.CameraDriver.SetRootPosAndSize(center.ToVector3Shifted() + new Vector3(-6f, 0, 0), 5f);
            yield return 20;
            Find.WindowStack.Add(new Dialog_InfoCard(p));
            yield return 30;
            yield return -1;
        }

        private static IEnumerator<int> Outfits(List<ThingDef> apparel, int offset)
        {
            if (apparel.Count == 0) yield break;
            Prepare("WoodPlankFloor");
            for (int i = 0; i < 5; i++)
            {
                var p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayer, forceGenerateNewPawn: true,
                    developmentalStages: DevelopmentalStage.Adult, tile: map.Tile));
                int n = apparel.Count;
                for (int j = 0; j < n; j++)
                {
                    var def = apparel[(i * 2 + offset * 5 + j) % n];
                    if (!ApparelUtility.HasPartsToWear(p, def)) continue;
                    if (p.apparel.WornApparel.Any(a => a.def.modContentPack == def.modContentPack && !ApparelUtility.CanWearTogether(a.def, def, p.RaceProps.body))) continue;
                    var a2 = (Apparel)MakeThing(def);
                    p.apparel.Wear(a2, false);
                }
                Spawn(p, center + new IntVec3(-4 + i * 2, 0, 0), Rot4.South);
            }
            FrameSpawned(1);
            yield return 40;
            HideUI(true);
            yield return -1;
        }

        private static IEnumerator<int> Traits(string pkg, int index)
        {
            var traits = DefDatabase<TraitDef>.AllDefsListForReading.Where(t => t.modContentPack?.PackageId?.Equals(pkg, StringComparison.OrdinalIgnoreCase) == true)
                .OrderBy(t => t.defName).ToList();
            if (traits.Count == 0) yield break;
            Prepare("WoodPlankFloor");
            var p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayer, forceGenerateNewPawn: true,
                developmentalStages: DevelopmentalStage.Adult, tile: map.Tile));
            int added = 0;
            for (int i = index * 7; i < traits.Count && added < 4; i += 3)
            {
                var def = traits[i % traits.Count];
                if (p.story.traits.HasTrait(def) || p.story.traits.allTraits.Any(t => t.def.ConflictsWith(def) || def.ConflictsWith(t))) continue;
                p.story.traits.GainTrait(new Trait(def, def.degreeDatas[0].degree, true));
                added++;
            }
            Spawn(p, center, Rot4.South);
            Select(p, typeof(ITab_Pawn_Character));
            yield return 40;
            yield return -1;
        }

        private static IEnumerator<int> Grafts(string pkg)
        {
            Prepare("WoodPlankFloor");
            var p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayer, forceGenerateNewPawn: true,
                developmentalStages: DevelopmentalStage.Adult, tile: map.Tile));
            var used = new HashSet<BodyPartRecord>();
            foreach (var r in DefDatabase<RecipeDef>.AllDefsListForReading.Where(r => r.modContentPack?.PackageId?.Equals(pkg, StringComparison.OrdinalIgnoreCase) == true
                                                                                     && r.addsHediff != null && !r.appliedOnFixedBodyParts.NullOrEmpty()))
            {
                var part = p.RaceProps.body.AllParts.FirstOrDefault(b => r.appliedOnFixedBodyParts.Contains(b.def) && !used.Contains(b) && !used.Any(u => u.parent == b || b.parent == u));
                if (part == null) continue;
                used.Add(part);
                p.health.AddHediff(r.addsHediff, part);
                if (used.Count >= 6) break;
            }
            Spawn(p, center, Rot4.South);
            Select(p, typeof(ITab_Pawn_Health));
            yield return 40;
            yield return -1;
        }

        private static IEnumerator<int> AlchemyRoom()
        {
            var furnace = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_AlchemyFurnace");
            if (furnace == null) yield break;
            var rect = Prepare("WoodPlankFloor");
            var room = CellRect.CenteredOn(center, 5);
            Walls(room);
            var f = Put(furnace, center + new IntVec3(0, 0, 1), Rot4.North);
            f.TryGetComp<CompWorkbenchLevel>()?.SetLevel(3);
            var fire = f.TryGetComp<CompSpiritFire>();
            var crystal = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_FireCrystal") ?? DefDatabase<ThingDef>.GetNamedSilentFail("CVG_SpiritStone");
            var refuel = f.TryGetComp<CompRefuelable>();
            if (fire != null && refuel != null && crystal != null)
            {
                fire.SetFuel(crystal);
                var c = ThingMaker.MakeThing(crystal);
                c.stackCount = 50;
                refuel.Refuel(new List<Thing> { c });
            }
            void Furn(string def, IntVec3 off) { var d = DefDatabase<ThingDef>.GetNamedSilentFail(def); if (d != null) Put(d, center + off, Rot4.South); }
            Furn("CVG_PillCabinet", new IntVec3(-3, 0, 3));
            Furn("CVG_PillCabinet", new IntVec3(3, 0, 3));
            Furn("CVG_BronzeCenser", new IntVec3(-3, 0, -1));
            Furn("CVG_ScriptureShelf", new IntVec3(3, 0, -1));
            Furn("CVG_SpiritStoneLantern", new IntVec3(-3, 0, -3));
            Furn("CVG_SpiritStoneLantern", new IntVec3(3, 0, -3));
            var cook = Colonist(f.InteractionCell);
            cook.Rotation = Rot4.North;
            foreach (var (pill, off) in new[] { ("CVG_NineTurnsPill", new IntVec3(-1, 0, -3)), ("CVG_TemperingPill", new IntVec3(0, 0, -3)), ("CVG_QiRecoveryPill", new IntVec3(1, 0, -3)) })
            {
                var t = PillThing(pill);
                if (t != null) Spawn(t, center + off, Rot4.North);
            }
            Frame(room.ExpandedBy(1));
            yield return 40;
            Find.Selector.ClearSelection();
            Find.Selector.Select(f);
            yield return 20;
            yield return -1;
        }

        /// <summary>Un salón de secta amueblado con piezas de Furniture (las que no entran se saltean).</summary>
        private static IEnumerator<int> FurnishedHall()
        {
            Prepare("WoodPlankFloor");
            var room = CellRect.CenteredOn(center, 15, 11);
            Walls(room);
            var inner = room.ContractedBy(1);
            void Try(string def, int dx, int dz, Rot4 rot)
            {
                var d = DefDatabase<ThingDef>.GetNamedSilentFail(def);
                if (d == null) { Log.Message("[Convergence] ModShots hall: no " + def); return; }
                var cell = center + new IntVec3(dx, 0, dz);
                var occ = GenAdj.OccupiedRect(cell, rot, d.size);
                if (!occ.Cells.All(c => inner.Contains(c) && c.GetEdifice(map) == null && !c.GetThingList(map).Any(t => t.def.category == ThingCategory.Building)))
                {
                    Log.Message("[Convergence] ModShots hall: " + def + " " + d.size + " does not fit at " + dx + "," + dz);
                    return;
                }
                var placed = Put(d, cell, rot);
                if (placed == null || !placed.Spawned) Log.Message("[Convergence] ModShots hall: " + def + " did not spawn");
            }
            Try("CVG_SectBed", -5, 3, Rot4.South);
            Try("CVG_BedsideTray", -4, 4, Rot4.South);
            Try("CVG_RobeWardrobe", -5, -3, Rot4.East);
            Try("CVG_LowTable", 0, 0, Rot4.South);
            Try("CVG_MeditationStool", -2, 0, Rot4.East);
            Try("CVG_MeditationStool", 2, 0, Rot4.West);
            Try("CVG_PorcelainVase", -2, 4, Rot4.South);
            Try("CVG_PaintingScroll", 0, 4, Rot4.South);
            Try("CVG_PorcelainVase", 2, 4, Rot4.South);
            Try("CVG_SpiritBonsai", 5, 4, Rot4.South);
            Try("CVG_KoiPond", 4, -2, Rot4.South);
            Try("CVG_StatueQilin", 6, 1, Rot4.South);
            Try("CVG_Guqin", 0, -3, Rot4.South);
            Try("CVG_CarvedScreen", -3, -1, Rot4.South);
            Try("CVG_PaperLantern", -3, -4, Rot4.South);
            Try("CVG_PaperLantern", 2, -4, Rot4.South);
            Try("CVG_LotusPlanter", 6, -4, Rot4.South);
            Colonist(center + new IntVec3(-2, 0, -1));
            Colonist(center + new IntVec3(1, 0, -2));
            Frame(room.ExpandedBy(1));
            yield return 40;
            HideUI(true);
            yield return -1;
        }

        private static IEnumerator<int> Formation()
        {
            var core = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_SwordFormationCore") ?? DefDatabase<ThingDef>.GetNamedSilentFail("CVG_QiGatheringCore");
            if (core == null) yield break;
            Prepare(null, TerrainDefOf.Soil);
            Put(core, center, Rot4.North);
            var flag = DefDatabase<ThingDef>.AllDefsListForReading.FirstOrDefault(d => d.defName.Contains("FormationFlag"));
            if (flag != null)
                foreach (var off in new[] { new IntVec3(-5, 0, -5), new IntVec3(5, 0, -5), new IntVec3(-5, 0, 5), new IntVec3(5, 0, 5), new IntVec3(0, 0, 6), new IntVec3(0, 0, -6), new IntVec3(-6, 0, 0), new IntVec3(6, 0, 0) })
                    Put(flag, center + off, Rot4.North);
            var statue = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_RuneCrossbowTurret");
            if (statue != null) { Put(statue, center + new IntVec3(-3, 0, 2), Rot4.South); Put(statue, center + new IntVec3(3, 0, 2), Rot4.South); }
            Colonist(center + new IntVec3(0, 0, -3));
            Frame(CellRect.CenteredOn(center, 7));
            yield return 30;
            Find.Selector.ClearSelection();
            Find.Selector.Select(center.GetFirstBuilding(map));
            yield return 20;
            yield return -1;
        }

        private static IEnumerator<int> StoryEvent(int index)
        {
            var type = AccessTools.TypeByName("Convergence.Stories.StoryEventDef");
            var fire = AccessTools.Method("Convergence.Stories.StoryEngine:Fire");
            if (type == null || fire == null) yield break;
            var defs = GenDefDatabase.GetAllDefsInDatabaseForDef(type)
                .Where(d => Traverse.Create(d).Field("options").GetValue() is System.Collections.IList l && l.Count > 0
                            && !(bool)Traverse.Create(d).Field("chainOnly").GetValue() && !(bool)Traverse.Create(d).Field("requiresFaction").GetValue())
                .OrderBy(d => d.defName).ToList();
            Prepare(null, TerrainDefOf.Soil);
            Colonist(center);
            Frame(CellRect.CenteredOn(center, 6));
            int skip = index;
            foreach (var d in defs.Skip(index * 3))
            {
                int before = Find.LetterStack.LettersListForReading.Count;
                bool ok;
                try { ok = (bool)fire.Invoke(null, new object[] { d, map }); }
                catch (Exception) { ok = false; }
                if (!ok || Find.LetterStack.LettersListForReading.Count == before) continue;
                yield return 10;
                var letter = Find.LetterStack.LettersListForReading.Last();
                Messages.Clear();
                letter.OpenLetter();
                yield return 30;
                yield return -1;
                yield break;
            }
        }

        private static IEnumerator<int> RealmGate()
        {
            var spawn = AccessTools.Method("Convergence.Stories.SecretRealmUtility:SpawnGate");
            if (spawn == null) yield break;
            Prepare(null, TerrainDefOf.Soil);
            var gate = spawn.Invoke(null, new object[] { map, center, null }) as Thing;
            if (gate == null) yield break;
            spawned.Add(gate);
            Colonist(center + new IntVec3(-3, 0, -2));
            Colonist(center + new IntVec3(3, 0, -2));
            Frame(CellRect.CenteredOn(center, 5));
            yield return 30;
            HideUI(true);
            yield return -1;
        }

        private static IEnumerator<int> Island()
        {
            if (DefDatabase<ThingDef>.GetNamedSilentFail("CVG_IslandHeart") == null) yield break;
            Prepare(null, TerrainDefOf.Soil);
            var cloud = DefDatabase<TerrainDef>.GetNamedSilentFail("CVG_CloudRock");
            if (cloud != null) foreach (var c in CellRect.CenteredOn(center, 11)) map.terrainGrid.SetTerrain(c, cloud);
            void P(string def, IntVec3 off, Rot4 rot) { var d = DefDatabase<ThingDef>.GetNamedSilentFail(def); if (d != null) Put(d, center + off, rot); }
            P("CVG_IslandHeart", IntVec3.Zero, Rot4.North);
            foreach (var o in new[] { (-3, 3), (3, 3), (-3, -3), (3, -3) }) P("CVG_GravityCoreMount", new IntVec3(o.Item1, 0, o.Item2), Rot4.North);
            P("CVG_EssenceVessel", new IntVec3(-8, 0, 0), Rot4.North);
            P("CVG_HelmsmanAltar", new IntVec3(0, 0, 6), Rot4.North);
            P("SmallThruster", new IntVec3(-4, 0, -8), Rot4.South);
            P("SmallThruster", new IntVec3(-1, 0, -8), Rot4.South);
            P("LargeThruster", new IntVec3(3, 0, -8), Rot4.South);
            P("LargeThruster", new IntVec3(6, 0, -8), Rot4.South);
            for (int i = -2; i <= 2; i++) { P("CVG_CloudRockWall", new IntVec3(i, 0, 9), Rot4.North); P("CVG_JadeWall", new IntVec3(i, 0, -10), Rot4.North); }
            P("CVG_BreathFormation", new IntVec3(-6, 0, 6), Rot4.North);
            P("CVG_ArkProtectiveFormation", new IntVec3(-8, 0, 8), Rot4.North);
            Colonist(center + new IntVec3(2, 0, 2));
            Colonist(center + new IntVec3(-2, 0, 4));
            Frame(CellRect.CenteredOn(center, 11));
            yield return 40;
            HideUI(true);
            yield return -1;
        }

        private static IEnumerator<int> Lore(Thing thing)
        {
            if (thing == null) yield break;
            Prepare("WoodPlankFloor");
            Find.CameraDriver.SetRootPosAndSize(center.ToVector3Shifted(), 8f);
            yield return 10;
            Find.WindowStack.Add(new Dialog_InfoCard(thing));
            yield return 30;
            yield return -1;
        }

        // ---------- Ayudas ----------

        private static Thing PillThing(string defName)
        {
            var def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
            if (def == null) return null;
            var t = ThingMaker.MakeThing(def);
            t.TryGetComp<CompQuality>()?.SetQuality(QualityCategory.Legendary, ArtGenerationContext.Outsider);
            return t;
        }

        private static Thing MakeThing(ThingDef def)
        {
            var t = ThingMaker.MakeThing(def, def.MadeFromStuff ? GenStuff.DefaultStuffFor(def) : null);
            t.TryGetComp<CompQuality>()?.SetQuality(QualityCategory.Excellent, ArtGenerationContext.Outsider);
            return t;
        }

        /// <summary>Limpia el área de la escena (y la deja con el piso o el terreno pedido).</summary>
        private static CellRect Prepare(string floor, TerrainDef terrain = null)
        {
            CleanUp();
            var rect = CellRect.CenteredOn(center, Half).ClipInsideMap(map);
            var t = floor != null ? DefDatabase<TerrainDef>.GetNamedSilentFail(floor) ?? TerrainDefOf.Soil : terrain ?? TerrainDefOf.Soil;
            foreach (var c in rect)
            {
                foreach (var th in c.GetThingList(map).ToList())
                    if (th.def.destroyable && !(th is Pawn p && p.IsColonist && !spawned.Contains(p))) th.Destroy();
                map.roofGrid.SetRoof(c, null);
                map.terrainGrid.SetTerrain(c, t);
                map.snowGrid?.SetDepth(c, 0f);
            }
            foreach (var f in map.listerFilthInHomeArea.FilthInHomeArea.ToList()) if (rect.Contains(f.Position)) f.Destroy();
            return rect;
        }

        private static void Walls(CellRect rect)
        {
            foreach (var c in rect.EdgeCells)
            {
                var w = ThingMaker.MakeThing(ThingDefOf.Wall, ThingDefOf.BlocksGranite);
                w.SetFaction(Faction.OfPlayer);
                Spawn(w, c, Rot4.North);
            }
        }

        /// <summary>Acomoda edificios en filas dentro de un área; devuelve el rectángulo ocupado.</summary>
        private static CellRect Pack(List<ThingDef> defs, CellRect area, Action<ThingDef, IntVec3> place)
        {
            int x = area.minX, zTop = area.maxZ, rowH = 0, maxX = area.minX, minZ = area.maxZ;
            foreach (var d in defs)
            {
                int w = d.size.x, h = d.size.z;
                if (x + w - 1 > area.maxX) { x = area.minX; zTop -= rowH + 1; rowH = 0; }
                if (zTop - h + 1 < area.minZ) break;
                // GenSpawn centra los tamaños pares: la celda base es la del centro (hacia abajo/izquierda).
                var cell = new IntVec3(x + (w - 1) / 2, 0, zTop - h / 2);
                place(d, cell);
                maxX = Mathf.Max(maxX, x + w - 1);
                minZ = Mathf.Min(minZ, zTop - h + 1);
                x += w + 1;
                rowH = Mathf.Max(rowH, h);
            }
            return CellRect.FromLimits(area.minX, minZ, maxX, area.maxZ);
        }

        private static Thing Put(ThingDef def, IntVec3 cell, Rot4 rot)
        {
            var t = MakeThing(def);
            if (t.def.CanHaveFaction) t.SetFaction(Faction.OfPlayer);
            return Spawn(t, cell, rot);
        }

        private static Thing Spawn(Thing t, IntVec3 cell, Rot4 rot)
        {
            var s = GenSpawn.Spawn(t, cell, map, rot, WipeMode.Vanish);
            spawned.Add(s);
            if (s is Pawn p) { p.Rotation = rot; p.jobs?.StopAll(); }
            return s;
        }

        private static Pawn Colonist(IntVec3 cell)
        {
            var p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayer, forceGenerateNewPawn: true,
                developmentalStages: DevelopmentalStage.Adult, tile: map.Tile));
            return (Pawn)Spawn(p, cell, Rot4.South);
        }

        private static void Select(Pawn p, Type tab)
        {
            Find.Selector.ClearSelection();
            Find.Selector.Select(p);
            ((MainTabWindow_Inspect)MainButtonDefOf.Inspect.TabWindow).OpenTabType = tab;
            Find.CameraDriver.SetRootPosAndSize(p.DrawPos + new Vector3(3f, 0, 2f), 6f);
        }

        /// <summary>La cámara encuadra el rectángulo (la altura visible es ~2,3 veces el tamaño de la cámara).</summary>
        private static void Frame(CellRect r)
        {
            float size = Mathf.Max(r.Height, r.Width / ((float)Screen.width / Screen.height)) * 0.47f;
            Find.CameraDriver.SetRootPosAndSize(r.CenterVector3, Mathf.Clamp(size, 3f, 30f));
        }

        /// <summary>Encuadra todo lo que la escena colocó (sin las paredes).</summary>
        private static void FrameSpawned(int pad = 1)
        {
            var things = spawned.Where(t => t.Spawned && t.def != ThingDefOf.Wall).ToList();
            if (things.Count == 0) return;
            var r = things.Select(t => t.OccupiedRect()).Aggregate((a, b) => CellRect.FromLimits(Mathf.Min(a.minX, b.minX), Mathf.Min(a.minZ, b.minZ),
                Mathf.Max(a.maxX, b.maxX), Mathf.Max(a.maxZ, b.maxZ)));
            Frame(r.ExpandedBy(pad));
        }

        private static void CleanUp()
        {
            HideUI(false);
            Find.Selector.ClearSelection();
            CleanScreen();
            foreach (var t in spawned) if (!t.Destroyed) t.Destroy();
            spawned.Clear();
        }

        private static void CleanScreen()
        {
            foreach (var w in Find.WindowStack.Windows.ToList())
                if (w is EditWindow_Log || w is Dialog_InfoCard || w is Dialog_NodeTree || w is MainTabWindow || w.layer == WindowLayer.Dialog) w.Close(false);
            Messages.Clear();
            Find.LetterStack.LettersListForReading.ToList().ForEach(l => Find.LetterStack.RemoveLetter(l));
        }

        private static void HideUI(bool hide) => Find.UIRoot.screenshotMode.Active = hide;

        private static void Capture(string module, string name)
        {
            foreach (var w in Find.WindowStack.Windows.OfType<EditWindow_Log>().ToList()) w.Close(false);
            var path = Path.Combine(root, module, name + ".png");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            ScreenCapture.CaptureScreenshot(path);
        }
    }
}
