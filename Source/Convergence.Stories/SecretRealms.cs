using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace Convergence.Stories
{
    // Reinos secretos (docs/dev/EVENTOS_SECRETOS.md, sección "Reinos secretos"): mazmorras de un mapa aparte (pocket map vanilla, sin DLC).
    //  - Puerta del reino (Building_RealmGate : MapPortal): la entrada. Aparece al minar, por incidente o en un sitio del mundo (misión).
    //  - SecretRealmDef (XML): contenido de cada reino (guardianes, trampas, tesoro, tope de reino, duración).
    //  - GenStep_SecretRealm: genera un laberinto de salas de piedra; MapComponent_SecretRealm: supresión por reino y derrumbe con tiempo.
    //  - Santuario de la Secta Olvidada: reemplaza el interior del templo antiguo vanilla (sarcófagos con discípulos dormidos).

    public class RealmSpawn
    {
        public PawnKindDef kind;
        public IntRange count = new IntRange(1, 1);
    }

    public class RealmLoot
    {
        public ThingDef thing;
        public IntRange count = new IntRange(1, 1);
        public float chance = 1f;
    }

    /// <summary>Contenido de un reino secreto, todo por XML.</summary>
    public class SecretRealmDef : Def
    {
        /// <summary>Peso al elegir reino para una puerta nueva.</summary>
        public float commonality = 1f;
        /// <summary>Reino mínimo del colono más fuerte para que aparezca este reino (0 = siempre).</summary>
        public int minColonyRealm;
        /// <summary>Los de un reino superior son suprimidos dentro (sello del reino). 99 = sin tope.</summary>
        public int maxRealmLevel = 99;
        /// <summary>Días hasta el derrumbe desde la primera entrada.</summary>
        public float collapseDays = 6f;
        public TerrainDef floor;
        public ThingDef wall;
        public ThingDef wallStuff;
        public ThingDef lantern;
        public ThingDef trap;
        public IntRange traps = new IntRange(6, 10);
        public IntRange guardianRooms = new IntRange(3, 5);
        public List<RealmSpawn> guardians = new List<RealmSpawn>();
        /// <summary>Puerta de cada sala (como en las ruinas antiguas: abrís y te encontrás lo que hay). Por defecto, puerta de piedra.</summary>
        public ThingDef door;
        /// <summary>Torretas de las salas de guardia (de la facción de los guardianes) y cuántas por sala.</summary>
        public List<ThingDef> guardTurrets = new List<ThingDef>();
        public IntRange guardTurretCount = new IntRange(0, 0);
        /// <summary>Botín chico en cada sala de guardia: la recompensa por abrir esa puerta.</summary>
        public List<RealmLoot> guardLoot = new List<RealmLoot>();
        public List<RealmSpawn> boss = new List<RealmSpawn>();
        public List<RealmLoot> treasure = new List<RealmLoot>();
        public List<RealmLoot> bossTreasure = new List<RealmLoot>();
        /// <summary>Guardianes que vigilan la puerta cuando está en un sitio del mundo.</summary>
        public List<RealmSpawn> siteGuards = new List<RealmSpawn>();
    }

    /// <summary>En el ThingDef de la puerta: opcionalmente fija el reino (si no, se elige al azar).</summary>
    public class DefModExtension_RealmGate : DefModExtension
    {
        public SecretRealmDef realm;
    }

    public static class SecretRealmUtility
    {
        public static readonly List<Map> PendingCollapse = new List<Map>();

        public static SecretRealmDef PickRealm()
        {
            int colony = CultivatorPawnKindUtility.ColonyMaxRealm();
            return DefDatabase<SecretRealmDef>.AllDefsListForReading
                .Where(r => r.commonality > 0f && r.minColonyRealm <= colony)
                .RandomElementByWeightWithFallback(r => r.commonality);
        }

        public static Pawn MakeGuardian(PawnKindDef kind, Faction faction)
        {
            try { return PawnGenerator.GeneratePawn(kind, faction); }
            catch (Exception e) { Log.Warning("[Convergence] Could not generate realm guardian " + kind?.defName + ": " + e.Message); return null; }
        }

        /// <summary>Genera y coloca guardianes en la sala, con un lord que los hace vigilar el punto.</summary>
        public static int SpawnGuardians(IEnumerable<RealmSpawn> spawns, CellRect room, Map map)
        {
            var faction = Faction.OfAncientsHostile;
            var pawns = new List<Pawn>();
            foreach (var s in spawns)
            {
                if (s.kind == null) continue;
                int n = s.count.RandomInRange;
                for (int i = 0; i < n; i++)
                {
                    var p = MakeGuardian(s.kind, faction);
                    if (p == null) continue;
                    var cell = room.Cells.Where(c => c.Standable(map) && c.GetFirstPawn(map) == null).RandomElementWithFallback(room.CenterCell);
                    GenSpawn.Spawn(p, cell, map);
                    pawns.Add(p);
                }
            }
            if (pawns.Count > 0 && faction != null)
                LordMaker.MakeNewLord(faction, new LordJob_DefendPoint(room.CenterCell, null, null, false, false), map, pawns);
            return pawns.Count;
        }

        public static void PlaceLoot(IEnumerable<RealmLoot> loot, IntVec3 near, Map map)
        {
            foreach (var l in loot)
            {
                if (l.thing == null || !Rand.Chance(l.chance)) continue;
                var t = ThingMaker.MakeThing(l.thing, l.thing.MadeFromStuff ? GenStuff.DefaultStuffFor(l.thing) : null);
                t.stackCount = Math.Max(1, Math.Min(l.thing.stackLimit, l.count.RandomInRange));
                GenPlace.TryPlaceThing(t, near, map, ThingPlaceMode.Near);
            }
        }

        /// <summary>Crea la puerta en la celda (despeja lo que haya) con el reino dado o uno al azar.</summary>
        public static Building_RealmGate SpawnGate(Map map, IntVec3 cell, SecretRealmDef realm = null)
        {
            var def = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_RealmGate");
            if (def == null) return null;
            foreach (var t in cell.GetThingList(map).ToList())
                if (t.def.destroyable && t.def.category != ThingCategory.Pawn) t.Destroy();
            var gate = (Building_RealmGate)ThingMaker.MakeThing(def);
            gate.realm = realm ?? PickRealm();
            GenSpawn.Spawn(gate, cell, map);
            return gate;
        }
    }

    // ====================================================================== puerta

    public class Building_RealmGate : MapPortal
    {
        public SecretRealmDef realm;
        private bool firstEntryNotified;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref realm, "realm");
            Scribe_Values.Look(ref firstEntryNotified, "firstEntryNotified");
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (realm == null) realm = def.GetModExtension<DefModExtension_RealmGate>()?.realm ?? SecretRealmUtility.PickRealm();
        }

        public override string GetInspectString()
        {
            string s = base.GetInspectString();
            if (realm != null)
            {
                string cap = realm.maxRealmLevel >= 99 ? "CVG_Realm_NoCap".Translate().ToString() : "CVG_Realm_Cap".Translate(realm.maxRealmLevel).ToString();
                s += (s.NullOrEmpty() ? "" : "\n") + realm.LabelCap + ": " + cap;
            }
            return s;
        }

        public override void OnEntered(Pawn pawn)
        {
            base.OnEntered(pawn);
            if (realm != null && !firstEntryNotified)
            {
                firstEntryNotified = true;
                Messages.Message("CVG_Realm_Entered".Translate(realm.LabelCap, realm.collapseDays.ToString("0.#")), exit, MessageTypeDefOf.NeutralEvent, historical: false);
            }
        }
    }

    // ====================================================================== mapa del reino

    public class MapComponent_SecretRealm : MapComponent
    {
        public SecretRealmDef realm;
        public int collapseTick = -1;
        private bool warned;

        public MapComponent_SecretRealm(Map map) : base(map) { }

        public void Init(SecretRealmDef def)
        {
            realm = def;
            collapseTick = Find.TickManager.TicksGame + (int)(def.collapseDays * GenDate.TicksPerDay);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref realm, "realm");
            Scribe_Values.Look(ref collapseTick, "collapseTick", -1);
            Scribe_Values.Look(ref warned, "warned");
        }

        public override void MapComponentTick()
        {
            if (realm == null || Find.TickManager.TicksGame % 250 != 0) return;
            Suppress();
            if (collapseTick < 0) return;
            int left = collapseTick - Find.TickManager.TicksGame;
            if (!warned && left < GenDate.TicksPerDay)
            {
                warned = true;
                Messages.Message("CVG_Realm_Warning".Translate(realm.LabelCap), MessageTypeDefOf.NegativeEvent, historical: false);
            }
            if (left <= 0 && !SecretRealmUtility.PendingCollapse.Contains(map)) SecretRealmUtility.PendingCollapse.Add(map);
        }

        /// <summary>Sello del reino: quien esté por encima del tope sufre la presión del sello.</summary>
        public void Suppress()
        {
            if (realm.maxRealmLevel >= 99) return;
            var hediff = DefDatabase<HediffDef>.GetNamedSilentFail("CVG_RealmSealPressure");
            if (hediff == null) return;
            foreach (var p in map.mapPawns.AllPawnsSpawned.ToList())
            {
                if (p.Faction != Faction.OfPlayer && p.HostFaction != Faction.OfPlayer) continue;
                var comp = CompCultivator.For(p);
                if (comp == null || comp.RealmLevel <= realm.maxRealmLevel) continue;
                var existing = p.health.hediffSet.GetFirstHediffOfDef(hediff);
                if (existing == null) { p.health.AddHediff(hediff); Messages.Message("CVG_Realm_Suppressed".Translate(p.LabelShort), p, MessageTypeDefOf.NegativeEvent, historical: false); }
                else existing.TryGetComp<HediffComp_Disappears>()?.CompPostMake(); // renueva la cuenta atrás
            }
        }
    }

    public class GameComponent_SecretRealms : GameComponent
    {
        public GameComponent_SecretRealms(Game game) { }

        public override void GameComponentTick()
        {
            if (SecretRealmUtility.PendingCollapse.Count == 0) return;
            var maps = SecretRealmUtility.PendingCollapse.ToList();
            SecretRealmUtility.PendingCollapse.Clear();
            foreach (var m in maps) Collapse(m);
        }

        public static void Collapse(Map realmMap)
        {
            var parent = realmMap.Parent as PocketMapParent;
            var source = parent?.sourceMap;
            if (source == null || !Find.Maps.Contains(realmMap)) return;
            var exit = realmMap.listerThings.AllThings.OfType<PocketMapExit>().FirstOrDefault();
            var gate = exit?.entrance;
            int moved = 0;
            foreach (var p in realmMap.mapPawns.AllPawnsSpawned.Where(x => x.Faction == Faction.OfPlayer || x.HostFaction == Faction.OfPlayer).ToList())
            {
                var cell = CellFinder.RandomClosewalkCellNear(gate?.Position ?? source.Center, source, 6, c => c.Standable(source));
                p.DeSpawn();
                GenSpawn.Spawn(p, cell, source);
                moved++;
            }
            // los cuerpos de los colonos caídos salen también (enterrarlos o resucitarlos); los objetos no recogidos se pierden
            foreach (var corpse in realmMap.listerThings.ThingsInGroup(ThingRequestGroup.Corpse).OfType<Corpse>()
                         .Where(c => c.InnerPawn?.Faction == Faction.OfPlayer).ToList())
            {
                var cell = CellFinder.RandomClosewalkCellNear(gate?.Position ?? source.Center, source, 6, c => c.Standable(source));
                corpse.DeSpawn();
                GenPlace.TryPlaceThing(corpse, cell, source, ThingPlaceMode.Near);
            }
            if (Find.CurrentMap == realmMap) Current.Game.CurrentMap = source;
            string label = (realmMap.GetComponent<MapComponent_SecretRealm>()?.realm?.LabelCap) ?? "";
            Find.LetterStack.ReceiveLetter("CVG_Realm_Collapsed_Label".Translate(), "CVG_Realm_Collapsed_Text".Translate(label), LetterDefOf.NeutralEvent, gate != null ? new LookTargets(gate.Position, source) : null);
            gate?.Destroy();
            PocketMapUtility.DestroyPocketMap(realmMap);
        }
    }

    public class GenStep_SecretRealm : GenStep
    {
        public override int SeedPart => 774310012;

        private struct Room { public CellRect rect; public int gx, gz; public string role; }

        public override void Generate(Map map, GenStepParams parms)
        {
            var gate = PocketMapUtility.currentlyGeneratingPortal as Building_RealmGate;
            var realm = gate?.realm ?? SecretRealmUtility.PickRealm();
            if (realm == null) { Log.Error("[Convergence] No SecretRealmDef available."); return; }
            int size = map.Size.x;
            int cell = (size - 1) / 3;
            var carved = new bool[size, size];
            var rooms = new Room[3, 3];
            for (int gx = 0; gx < 3; gx++)
                for (int gz = 0; gz < 3; gz++)
                {
                    int w = Rand.RangeInclusive(8, Math.Min(14, cell - 4)), h = Rand.RangeInclusive(8, Math.Min(14, cell - 4));
                    int x0 = 1 + gx * cell + Rand.RangeInclusive(2, cell - w - 2), z0 = 1 + gz * cell + Rand.RangeInclusive(2, cell - h - 2);
                    var rect = new CellRect(x0, z0, w, h);
                    rooms[gx, gz] = new Room { rect = rect, gx = gx, gz = gz, role = "empty" };
                    foreach (var c in rect) carved[c.x, c.z] = true;
                }
            // pasillos: árbol de expansión aleatorio + dos lazos extra
            var corridor = new List<IntVec3>();
            var visited = new bool[3, 3];
            var stack = new Stack<(int, int)>();
            var order = new List<(int, int)>();
            var start = (Rand.Chance(0.5f) ? 0 : 2, Rand.Chance(0.5f) ? 0 : 2);
            stack.Push(start); visited[start.Item1, start.Item2] = true; order.Add(start);
            var dist = new Dictionary<(int, int), int> { [start] = 0 };
            while (stack.Count > 0)
            {
                var (cx, cz) = stack.Peek();
                var next = new List<(int, int)>();
                foreach (var d in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
                {
                    int nx = cx + d.Item1, nz = cz + d.Item2;
                    if (nx < 0 || nx > 2 || nz < 0 || nz > 2 || visited[nx, nz]) continue;
                    next.Add((nx, nz));
                }
                if (next.Count == 0) { stack.Pop(); continue; }
                var n = next.RandomElement();
                visited[n.Item1, n.Item2] = true; order.Add(n); dist[n] = dist[(cx, cz)] + 1;
                Carve(rooms[cx, cz].rect.CenterCell, rooms[n.Item1, n.Item2].rect.CenterCell, carved, corridor);
                stack.Push(n);
            }
            for (int i = 0; i < 2; i++)
            {
                int gx = Rand.RangeInclusive(0, 1), gz = Rand.RangeInclusive(0, 2);
                Carve(rooms[gx, gz].rect.CenterCell, rooms[gx + 1, gz].rect.CenterCell, carved, corridor);
            }
            // salas cerradas: pared en el borde de cada sala y una puerta por cada pasillo que llega
            var doors = new List<IntVec3>();
            var roomRects = rooms.Cast<Room>().Select(r => r.rect).ToList();
            bool InAnyRoom(int x, int z) => roomRects.Any(rr => rr.Contains(new IntVec3(x, 0, z)));
            foreach (var r in rooms.Cast<Room>())
            {
                var rect = r.rect;
                var edge = rect.EdgeCells.ToList();
                bool Opening(IntVec3 c)
                {
                    if (rect.IsCorner(c)) return false;
                    var o = c.x == rect.minX ? c + IntVec3.West : c.x == rect.maxX ? c + IntVec3.East : c.z == rect.minZ ? c + IntVec3.South : c + IntVec3.North;
                    return o.x > 0 && o.z > 0 && o.x < size - 1 && o.z < size - 1 && carved[o.x, o.z] && !InAnyRoom(o.x, o.z);
                }
                var open = new HashSet<IntVec3>(edge.Where(Opening));
                // cada tramo seguido de aberturas en un mismo lado lleva una sola puerta, en el medio
                var done = new HashSet<IntVec3>();
                foreach (var c in open)
                {
                    if (done.Contains(c)) continue;
                    var run = new List<IntVec3>();
                    var q = new Queue<IntVec3>(); q.Enqueue(c); done.Add(c);
                    while (q.Count > 0)
                    {
                        var cur = q.Dequeue(); run.Add(cur);
                        foreach (var d in GenAdj.CardinalDirections)
                        {
                            var nb = cur + d;
                            if (open.Contains(nb) && !done.Contains(nb)) { done.Add(nb); q.Enqueue(nb); }
                        }
                    }
                    run = run.OrderBy(x => x.x).ThenBy(x => x.z).ToList();
                    doors.Add(run[run.Count / 2]);
                }
                foreach (var c in edge) if (!doors.Contains(c)) carved[c.x, c.z] = false;
            }

            // papeles de las salas
            var far = order.OrderByDescending(o => dist[o]).ToList();
            rooms[start.Item1, start.Item2].role = "entrance";
            rooms[far[0].Item1, far[0].Item2].role = "boss";
            rooms[far[1].Item1, far[1].Item2].role = "treasure";
            var others = order.Where(o => rooms[o.Item1, o.Item2].role == "empty").InRandomOrder().ToList();
            int guardRooms = Math.Min(others.Count, realm.guardianRooms.RandomInRange);
            for (int i = 0; i < guardRooms; i++) rooms[others[i].Item1, others[i].Item2].role = "guard";

            // piedra y suelo
            var stuff = realm.wallStuff ?? ThingDefOf.BlocksGranite;
            var wallDef = realm.wall ?? ThingDefOf.Wall;
            var floor = realm.floor ?? TerrainDefOf.PavedTile;
            for (int x = 0; x < size; x++)
                for (int z = 0; z < size; z++)
                {
                    var c = new IntVec3(x, 0, z);
                    map.terrainGrid.SetTerrain(c, floor);
                    map.roofGrid.SetRoof(c, RoofDefOf.RoofRockThick);
                    if (!carved[x, z]) GenSpawn.Spawn(ThingMaker.MakeThing(wallDef, wallDef.MadeFromStuff ? stuff : null), c, map);
                }

            var doorDef = realm.door ?? ThingDefOf.Door;
            foreach (var c in doors)
            {
                foreach (var t in c.GetThingList(map).ToList()) if (t.def.destroyable) t.Destroy();
                GenSpawn.Spawn(ThingMaker.MakeThing(doorDef, doorDef.MadeFromStuff ? stuff : null), c, map);
            }

            // contenido
            Room entrance = rooms[start.Item1, start.Item2];
            var exitDef = gate?.def.portal.exitDef ?? DefDatabase<ThingDef>.GetNamedSilentFail("CVG_RealmExit");
            if (exitDef != null)
            {
                foreach (var c in GenAdj.OccupiedRect(entrance.rect.CenterCell, Rot4.North, exitDef.size).Cells)
                    foreach (var t in c.GetThingList(map).ToList()) if (t.def.destroyable) t.Destroy();
                GenSpawn.Spawn(ThingMaker.MakeThing(exitDef), entrance.rect.CenterCell, map);
            }
            MapGenerator.PlayerStartSpot = entrance.rect.CenterCell;
            int guardians = 0;
            foreach (var r in rooms.Cast<Room>())
            {
                if (realm.lantern != null)
                    foreach (var c in new[] { new IntVec3(r.rect.minX + 1, 0, r.rect.minZ + 1), new IntVec3(r.rect.maxX - 1, 0, r.rect.minZ + 1), new IntVec3(r.rect.minX + 1, 0, r.rect.maxZ - 1), new IntVec3(r.rect.maxX - 1, 0, r.rect.maxZ - 1) })
                        GenSpawn.Spawn(ThingMaker.MakeThing(realm.lantern), c, map);
                if (r.role == "guard")
                {
                    var inner = r.rect.ContractedBy(1);
                    guardians += SecretRealmUtility.SpawnGuardians(realm.guardians, inner, map);
                    SecretRealmUtility.PlaceLoot(realm.guardLoot, inner.CenterCell, map);
                    if (realm.guardTurrets.Count > 0)
                    {
                        int n = realm.guardTurretCount.RandomInRange;
                        // en las esquinas interiores, lejos de las puertas
                        var spots = new[] { new IntVec3(inner.minX + 1, 0, inner.minZ + 1), new IntVec3(inner.maxX - 1, 0, inner.maxZ - 1), new IntVec3(inner.maxX - 1, 0, inner.minZ + 1), new IntVec3(inner.minX + 1, 0, inner.maxZ - 1) };
                        foreach (var c in spots.Take(n))
                        {
                            var def = realm.guardTurrets.RandomElement();
                            if (!GenAdj.OccupiedRect(c, Rot4.North, def.size).Cells.All(x => inner.Contains(x) && x.Standable(map) && x.GetFirstBuilding(map) == null)) continue;
                            var turret = ThingMaker.MakeThing(def, def.MadeFromStuff ? GenStuff.DefaultStuffFor(def) : null);
                            turret.SetFactionDirect(Faction.OfAncientsHostile);
                            GenSpawn.Spawn(turret, c, map);
                            var fuel = turret.TryGetComp<CompRefuelable>();
                            if (fuel != null) fuel.Refuel(fuel.Props.fuelCapacity);
                        }
                    }
                }
                else if (r.role == "boss")
                {
                    guardians += SecretRealmUtility.SpawnGuardians(realm.boss, r.rect.ContractedBy(1), map);
                    SecretRealmUtility.PlaceLoot(realm.bossTreasure, r.rect.CenterCell, map);
                }
                else if (r.role == "treasure") SecretRealmUtility.PlaceLoot(realm.treasure, r.rect.CenterCell, map);
            }
            if (realm.trap != null)
            {
                int want = realm.traps.RandomInRange;
                var used = new List<IntVec3>();
                foreach (var c in corridor.InRandomOrder())
                {
                    if (want <= 0) break;
                    if (!c.Standable(map) || used.Any(u => u.DistanceToSquared(c) < 9) || entrance.rect.ExpandedBy(2).Contains(c)) continue;
                    var trap = ThingMaker.MakeThing(realm.trap);
                    trap.SetFactionDirect(Faction.OfAncientsHostile);
                    GenSpawn.Spawn(trap, c, map);
                    used.Add(c); want--;
                }
            }
            map.GetComponent<MapComponent_SecretRealm>()?.Init(realm);
        }

        // corredor en L de 3 de ancho entre dos centros
        private static void Carve(IntVec3 a, IntVec3 b, bool[,] carved, List<IntVec3> corridor)
        {
            void Dig(int x, int z)
            {
                for (int dx = -1; dx <= 1; dx++)
                    for (int dz = -1; dz <= 1; dz++)
                    {
                        int px = x + dx, pz = z + dz;
                        if (px < 1 || pz < 1 || px >= carved.GetLength(0) - 1 || pz >= carved.GetLength(1) - 1) continue;
                        if (!carved[px, pz]) { carved[px, pz] = true; corridor.Add(new IntVec3(px, 0, pz)); }
                    }
            }
            int cx = a.x, cz = a.z;
            while (cx != b.x) { cx += Math.Sign(b.x - cx); Dig(cx, cz); }
            while (cz != b.z) { cz += Math.Sign(b.z - cz); Dig(cx, cz); }
        }
    }

    // ====================================================================== descubrimiento y sitio

    /// <summary>Al minar la montaña puede aparecer la puerta de un reino secreto (raro, solo tras la Convergencia).</summary>
    [HarmonyPatch(typeof(Mineable), nameof(Mineable.DestroyMined))]
    public static class Patch_Mineable_DestroyMined
    {
        private static int lastGateTick = -999999;

        public static void Prefix(Mineable __instance, out (Map, IntVec3, bool) __state)
        {
            __state = (__instance.Map, __instance.Position, __instance.def.building != null && __instance.def.building.isResourceRock);
        }

        public static void Postfix((Map, IntVec3, bool) __state)
        {
            var (map, cell, ore) = __state;
            if (map == null || ore || !map.IsPlayerHome || !GameComponent_Convergence.Active) return;
            if (!Rand.Chance(0.0025f * Mathf.Max(0.1f, StoryEngine.FrequencyFactor()))) return;
            int now = Find.TickManager.TicksGame;
            if (now - lastGateTick < 12 * GenDate.TicksPerDay) return;
            var gateDef = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_RealmGate");
            if (gateDef == null || map.listerThings.ThingsOfDef(gateDef).Count > 0) return;
            if (cell.GetEdifice(map) != null) return;
            lastGateTick = now;
            var gate = SecretRealmUtility.SpawnGate(map, cell);
            if (gate == null) return;
            Find.LetterStack.ReceiveLetter("CVG_RealmGate_Found_Label".Translate(), "CVG_RealmGate_Found_Text".Translate(gate.realm.LabelCap), LetterDefOf.PositiveEvent, gate);
        }
    }

    /// <summary>Incidente: una puerta de reino secreto aparece en el mapa (en la roca junto a un paso, o en campo abierto).</summary>
    public class IncidentWorker_RealmGate : IncidentWorker_Story
    {
        protected override int MinDaysSinceConvergence => 6;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms)) return false;
            var def = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_RealmGate");
            return def != null && ((Map)parms.target).listerThings.ThingsOfDef(def).Count == 0;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var map = (Map)parms.target;
            IntVec3 cell = IntVec3.Invalid;
            // preferido: roca natural con un lado abierto, lejos del borde
            var rock = new List<IntVec3>();
            foreach (var c in map.AllCells)
            {
                if (c.DistanceToEdge(map) < 6) continue;
                var e = c.GetEdifice(map);
                if (e == null || !e.def.building.isNaturalRock || e.def.building.isResourceRock) continue;
                if (GenAdj.CardinalDirections.Any(d => (c + d).InBounds(map) && (c + d).Standable(map) && !(c + d).Roofed(map))) rock.Add(c);
            }
            if (rock.Count > 0) cell = rock.RandomElement();
            else if (!CellFinderLoose.TryGetRandomCellWith(c => c.Standable(map) && !c.Roofed(map) && c.DistanceToEdge(map) > 8 && c.GetEdifice(map) == null, map, 300, out cell)) return false;
            var gate = SecretRealmUtility.SpawnGate(map, cell);
            if (gate == null) return false;
            SendStandardLetter("CVG_RealmGate_Found_Label".Translate(), "CVG_RealmGate_Found_Text".Translate(gate.realm.LabelCap), LetterDefOf.PositiveEvent, parms, gate);
            return true;
        }
    }

    /// <summary>Sitio del mundo: la puerta de un reino secreto, con unos guardianes fuera.</summary>
    public class GenStep_RealmGateSite : GenStep
    {
        public override int SeedPart => 774310013;

        public override void Generate(Map map, GenStepParams parms)
        {
            if (!CellFinderLoose.TryGetRandomCellWith(c => c.Standable(map) && !c.Roofed(map) && c.GetEdifice(map) == null && c.DistanceToEdge(map) > 12 && c.SupportsStructureType(map, TerrainAffordanceDefOf.Heavy), map, 500, out var cell))
                cell = map.Center;
            var gate = SecretRealmUtility.SpawnGate(map, cell);
            if (gate == null) return;
            var room = CellRect.CenteredOn(cell, 9, 9).ClipInsideMap(map);
            if (gate.realm.siteGuards.Count > 0) SecretRealmUtility.SpawnGuardians(gate.realm.siteGuards, room, map);
            MapGenerator.PlayerStartSpot = map.Center;
        }
    }

    /// <summary>Nodo de misión: ejecuta su hijo solo si la Convergencia ya ocurrió.</summary>
    public class QuestNode_IfConvergence : QuestNode
    {
        public QuestNode node;

        protected override bool TestRunInt(Slate slate) => GameComponent_Convergence.Active && (node == null || node.TestRun(slate));

        protected override void RunInt() => node?.Run();
    }

    // ====================================================================== santuario de la secta olvidada

    /// <summary>Interior del templo antiguo: sarcófagos con discípulos de una secta olvidada, dormidos en cultivo, y su tesoro.</summary>
    public class SymbolResolver_Interior_ForgottenSanctuary : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            var realm = DefDatabase<SecretRealmDef>.GetNamedSilentFail("CVG_Sanctuary");
            var podDef = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_ForgottenSarcophagus");
            if (!GameComponent_Convergence.Active || realm == null || podDef == null)
            {
                new SymbolResolver_Interior_AncientTemple().Resolve(rp);
                return;
            }
            var map = BaseGen.globalSettings.map;
            bool peaceful = Find.Storyteller.difficulty.peacefulTemples;
            var faction = peaceful ? Faction.OfAncients : Faction.OfAncientsHostile;
            int groupID = Find.UniqueIDsManager.GetNextAncientCryptosleepCasketGroupID();
            int want = Mathf.Clamp(rp.rect.Area / 14, 2, 5);
            var used = new List<IntVec3>();
            foreach (var c in rp.rect.Cells.InRandomOrder())
            {
                if (want <= 0) break;
                if (!c.Standable(map) || used.Any(u => u.DistanceToSquared(c) < 9)) continue;
                if (GenAdj.CardinalDirections.All(d => !(c + d).InBounds(map) || (c + d).Standable(map))) { /* mejor junto a un muro */ if (Rand.Chance(0.6f)) continue; }
                var pod = (Building_AncientCryptosleepCasket)ThingMaker.MakeThing(podDef);
                pod.groupID = groupID;
                var kind = DefDatabase<PawnKindDef>.GetNamedSilentFail(Rand.Chance(0.25f) ? "CVG_Sanctuary_Elder" : "CVG_Sanctuary_Disciple");
                var pawn = kind != null ? SecretRealmUtility.MakeGuardian(kind, faction) : null;
                if (pawn != null && !pod.TryAcceptThing(pawn, allowSpecialEffects: false)) Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
                var offering = realm.treasure.Where(l => l.thing != null && Rand.Chance(l.chance * 0.5f)).RandomElementWithFallback();
                if (offering != null) { var t = ThingMaker.MakeThing(offering.thing); t.stackCount = Math.Max(1, offering.count.RandomInRange); if (!pod.TryAcceptThing(t, allowSpecialEffects: false)) t.Destroy(); }
                GenSpawn.Spawn(pod, c, map, Rot4.Random);
                used.Add(c); want--;
            }
            SecretRealmUtility.PlaceLoot(realm.treasure, rp.rect.CenterCell, map);
            if (!peaceful && realm.guardians.Count > 0) SecretRealmUtility.SpawnGuardians(realm.guardians, rp.rect, map);
            if (realm.lantern != null)
                foreach (var c in new[] { new IntVec3(rp.rect.minX, 0, rp.rect.minZ), new IntVec3(rp.rect.maxX, 0, rp.rect.minZ), new IntVec3(rp.rect.minX, 0, rp.rect.maxZ), new IntVec3(rp.rect.maxX, 0, rp.rect.maxZ) })
                    if (c.Standable(map) && c.GetEdifice(map) == null) GenSpawn.Spawn(ThingMaker.MakeThing(realm.lantern), c, map);
        }
    }
}
