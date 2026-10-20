using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Convergence
{
    // Red de qi (docs/modulos/RED_QI.md): energía propia del mod, sin electricidad.
    //  - Fuentes (pozo de vena, formación de reunión), pilares (red por radio, sin cables), acumuladores (cristal) y consumidores,
    //    todos con el mismo componente CompQiNode configurable por XML (producción/consumo por día, radio, capacidad).
    //  - QiNetworkManager (MapComponent) agrupa los nodos en redes por radio de pilar, y cada 250 ticks reparte la producción,
    //    carga/descarga los acumuladores y decide si cada red está abastecida. Un consumidor sin qi se apaga.
    //  - Los edificios vanilla que exigen un CompPowerTrader (gestador de mecas, cargador, cripta) lo conservan con consumo 0 y su estado
    //    "encendido" lo decide la red de qi (parches a CompPowerTrader.PowerOn y a sus textos). Sin icono ni texto de electricidad.

    public class CompProperties_QiNode : CompProperties
    {
        /// <summary>Radio de conexión (> 0 = pilar: une todo lo que esté dentro de su radio).</summary>
        public float radius;
        /// <summary>Qi producido por día (a plena capacidad).</summary>
        public float produceDay;
        /// <summary>Qi consumido por día mientras el edificio está en la red.</summary>
        public float consumeDay;
        /// <summary>Qi que almacena (acumulador).</summary>
        public float storageCapacity;
        /// <summary>Fracción de lo almacenado que se pierde por día.</summary>
        public float leakPerDay = 0.05f;
        /// <summary>Solo produce si el edificio está sobre una casilla de vena de qi.</summary>
        public bool needsVein;
        /// <summary>Solo produce si su CompRefuelable tiene combustible.</summary>
        public bool needsFuel;
        /// <summary>La producción se multiplica por concentración de qi / <see cref="concentrationRef"/> (limitado a 0,5–3).</summary>
        public bool scaleWithConcentration;
        public float concentrationRef = 2f;
        /// <summary>Como un panel solar: bajo techo no produce.</summary>
        public bool needsOpenSky;
        /// <summary>Sigue el calendario celeste: más con la Marea de Qi, menos con el Reflujo.</summary>
        public bool celestialTides;
        /// <summary>Condensador: absorbe hasta tanto qi SOBRANTE por día (cuando los acumuladores ya están llenos)...</summary>
        public float condenseDay;
        /// <summary>...y cada tanto qi condensado deja un <see cref="condenseProduct"/> al lado.</summary>
        public float condenseCost = 60f;
        public ThingDef condenseProduct;

        public CompProperties_QiNode() => compClass = typeof(CompQiNode);
    }

    public class QiNet
    {
        public readonly List<CompQiNode> nodes = new List<CompQiNode>();
        public bool Satisfied;
        public float SupplyDay, DemandDay, Stored, Capacity;
    }

    [StaticConstructorOnStartup]
    public class CompQiNode : ThingComp
    {
        private float stored;
        private QiNet net;
        private CompRefuelable refuelable;
        private static Material noQiMat;

        public CompProperties_QiNode Props => (CompProperties_QiNode)props;
        public bool IsConnector => Props.radius > 0f;
        public virtual bool IsConsumer => Props.consumeDay > 0f;
        public bool IsStorage => Props.storageCapacity > 0f;
        public bool IsCondenser => Props.condenseDay > 0f && Props.condenseProduct != null;
        private float condensed;

        /// <summary>Recibe qi sobrante; devuelve cuánto absorbió. Cada <c>condenseCost</c> deja un producto al lado.</summary>
        public float Condense(float qi, float dt)
        {
            if (!IsCondenser || !Active) return 0f;
            float take = Mathf.Min(qi, Props.condenseDay * dt);
            condensed += take;
            while (condensed >= Props.condenseCost)
            {
                condensed -= Props.condenseCost;
                var thing = ThingMaker.MakeThing(Props.condenseProduct);
                GenPlace.TryPlaceThing(thing, parent.InteractionCell.IsValid && parent.def.hasInteractionCell ? parent.InteractionCell : parent.Position, parent.Map, ThingPlaceMode.Near);
            }
            return take;
        }
        public float Stored { get => stored; set => stored = Mathf.Clamp(value, 0f, Props.storageCapacity); }
        public CompRefuelable Refuelable => refuelable ??= parent.GetComp<CompRefuelable>();
        public QiNetworkManager Manager => parent.Map?.GetComponent<QiNetworkManager>();

        public QiNet Net
        {
            get
            {
                Manager?.EnsureBuilt();
                return net;
            }
            set => net = value;
        }

        public bool Active => parent.Spawned && parent.Faction == Faction.OfPlayer;
        public bool Powered => !IsConsumer || (Net != null && Net.Satisfied);

        private int lastEffectiveState = -1;   // -1 sin medir, 0 sin energia, 1 con energia

        /// <summary>
        /// Como la energia de un consumidor de qi no pasa por el setter de <c>CompPowerTrader.PowerOn</c>, nadie avisaba del cambio. Se avisa con
        /// las mismas señales que vanilla (PowerTurnedOn/Off), para que cosas como la barrera de vacio, el brillo o el sonido se actualicen
        /// cuando se acaba el qi o se toca el interruptor.
        /// </summary>
        public void NotifyPowerChange()
        {
            if (!IsConsumer || !parent.Spawned) return;
            bool on = Powered && (parent.TryGetComp<CompFlickable>()?.SwitchIsOn ?? true);
            int state = on ? 1 : 0;
            if (state == lastEffectiveState) return;
            bool first = lastEffectiveState < 0;
            lastEffectiveState = state;
            if (!first) parent.BroadcastCompSignal(on ? "PowerTurnedOn" : "PowerTurnedOff");
        }

        public override void ReceiveCompSignal(string signal)
        {
            base.ReceiveCompSignal(signal);
            if (signal == "FlickedOn" || signal == "FlickedOff") NotifyPowerChange();
        }

        public bool OnVein
        {
            get
            {
                var vein = QiNetworkUtility.VeinTerrain;
                if (vein == null || !parent.Spawned) return false;
                foreach (var c in parent.OccupiedRect())
                    if (parent.Map.terrainGrid.TerrainAt(c) == vein || parent.Map.terrainGrid.BaseTerrainAt(c) == vein) return true;
                return false;
            }
        }

        public virtual float ProduceDay
        {
            get
            {
                if (Props.produceDay <= 0f || !Active) return 0f;
                if (Props.needsFuel && (Refuelable == null || !Refuelable.HasFuel)) return 0f;
                if (Props.needsVein && !OnVein) return 0f;
                float v = Props.produceDay;
                if (Props.scaleWithConcentration)
                    v *= Mathf.Clamp(QiConcentrationUtility.ConcentrationAt(parent.Position, parent.Map) / Mathf.Max(0.1f, Props.concentrationRef), 0.5f, 3f);
                if (Props.needsOpenSky && parent.Map.roofGrid.Roofed(parent.Position)) return 0f;
                if (Props.celestialTides) v *= QiSourceUtility.TideFactor(parent.Map);
                return v;
            }
        }

        public virtual float ConsumeDay => IsConsumer && Active ? Props.consumeDay : 0f;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            parent.Map.GetComponent<QiNetworkManager>().Register(this);
            if (!respawningAfterLoad && parent.Faction == Faction.OfPlayer)
                CultivationGuide.Teach(CultivationGuide.QiNetwork, OpportunityType.GoodToKnow);
        }

        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            base.PostDeSpawn(map, mode);
            map.GetComponent<QiNetworkManager>()?.Deregister(this);
            net = null;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref stored, "qiStored");
            Scribe_Values.Look(ref condensed, "qiCondensed");
        }

        public override string CompInspectStringExtra()
        {
            if (!Active) return null;
            var n = Net;
            if (n == null || (n.nodes.Count <= 1 && !IsConnector))
                return (IsConsumer ? "CVG_QiNet_NotConnected" : "CVG_QiNet_Alone").Translate();
            string s = "CVG_QiNet_Status".Translate(n.SupplyDay.ToString("F0"), n.DemandDay.ToString("F0"), n.Stored.ToString("F0"), n.Capacity.ToString("F0"));
            if (IsConsumer && !n.Satisfied) s += "\n" + "CVG_QiNet_NoQi".Translate();
            if (IsCondenser) s += "\n" + "CVG_QiNet_Condensing".Translate(condensed.ToString("F0"), Props.condenseCost.ToString("F0"), Props.condenseProduct.label);
            return s;
        }

        public override void PostDraw()
        {
            if (!IsConsumer || !Active || Powered) return;
            noQiMat ??= MaterialPool.MatFrom(ContentFinder<Texture2D>.Get("UI/Overlays/CVG_NoQi", false) != null ? "UI/Overlays/CVG_NoQi" : "UI/Overlays/NeedsPower", ShaderDatabase.MetaOverlay, new Color(0.45f, 0.8f, 1f));
            var pos = parent.DrawPos;
            pos.y = AltitudeLayer.MetaOverlays.AltitudeFor();
            pos.z += 0.45f;
            Graphics.DrawMesh(MeshPool.plane08, Matrix4x4.TRS(pos, Quaternion.identity, new Vector3(0.7f, 1f, 0.7f)), noQiMat, 0);
        }

        public override void PostDrawExtraSelectionOverlays()
        {
            if (!parent.Spawned) return;
            if (IsConnector) GenDraw.DrawRadiusRing(parent.Position, Props.radius);
            var n = net;
            if (n == null) return;
            foreach (var other in n.nodes)
            {
                if (other == this || !other.parent.Spawned) continue;
                if (IsConnector ? parent.Position.DistanceTo(other.parent.Position) <= Props.radius : other.IsConnector && other.parent.Position.DistanceTo(parent.Position) <= other.Props.radius)
                    GenDraw.DrawLineBetween(parent.TrueCenter(), other.parent.TrueCenter(), SimpleColor.Cyan);
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var g in base.CompGetGizmosExtra()) yield return g;
            if (IsStorage && Active)
            {
                yield return new Command_Action
                {
                    defaultLabel = "CVG_QiNet_Infuse".Translate(),
                    defaultDesc = "CVG_QiNet_InfuseDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/Draft", false),
                    action = delegate
                    {
                        var options = new List<FloatMenuOption>();
                        foreach (var p in parent.Map.mapPawns.FreeColonistsSpawned)
                        {
                            var cult = CompCultivator.For(p);
                            if (cult == null || !cult.IsQiCultivator) continue;
                            var pawn = p;
                            if (!pawn.CanReach(parent, PathEndMode.Touch, Danger.Some)) continue;
                            options.Add(new FloatMenuOption(pawn.LabelShortCap, () =>
                            {
                                var job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("CVG_InfuseQi"), parent);
                                pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                            }));
                        }
                        if (options.Count == 0) options.Add(new FloatMenuOption("CVG_QiNet_NoInfuser".Translate(), null));
                        Find.WindowStack.Add(new FloatMenu(options));
                    }
                };
            }
        }
    }

    /// <summary>Marca de rol para consumidores (el comportamiento es el de <see cref="CompQiNode"/>).</summary>
    public class CompQiConsumer : CompQiNode
    {
    }

    public class QiNetworkManager : MapComponent
    {
        public const int StepTicks = 250;
        private readonly List<CompQiNode> nodes = new List<CompQiNode>();
        private readonly List<QiNet> nets = new List<QiNet>();
        private bool dirty = true;
        private bool veinsChecked;
        private int veinTries;

        public QiNetworkManager(Map map) : base(map) { }

        public IReadOnlyList<QiNet> Nets { get { EnsureBuilt(); return nets; } }
        public IReadOnlyList<CompQiNode> Nodes => nodes;

        public void Register(CompQiNode n) { if (!nodes.Contains(n)) nodes.Add(n); dirty = true; }
        public void Deregister(CompQiNode n) { nodes.Remove(n); dirty = true; }
        public void MarkDirty() => dirty = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref veinsChecked, "veinsChecked");
            // partidas viejas pudieron marcarlo sin haber pintado ninguna veta: se vuelve a comprobar al cargar (si hay, sale enseguida)
            if (Scribe.mode == LoadSaveMode.PostLoadInit) veinsChecked = false;
        }

        public void EnsureBuilt()
        {
            if (!dirty) return;
            Rebuild();
            Step(0f);
        }

        public override void MapComponentTick()
        {
            if (dirty) EnsureBuilt();
            else if (Find.TickManager.TicksGame % StepTicks == 0) Step(StepTicks / 60000f);
            // hasta que el mapa tenga al menos una veta: primero al toque, después una vez por día (un mapa techado o boscoso
            // puede no tener lugar hoy; con el tiempo se tala, se construye, cambia)
            if (!veinsChecked && GameComponent_Convergence.Active && map.IsPlayerHome
                && Find.TickManager.TicksGame % (veinTries == 0 ? 60 : GenDate.TicksPerDay) == 0)
            {
                veinTries++;
                veinsChecked = QiNetworkUtility.EnsureVeins(map);
            }
        }

        private void Rebuild()
        {
            dirty = false;
            nets.Clear();
            int n = nodes.Count;
            var parentIdx = new int[n];
            for (int i = 0; i < n; i++) parentIdx[i] = i;
            int Find_(int x) { while (parentIdx[x] != x) { parentIdx[x] = parentIdx[parentIdx[x]]; x = parentIdx[x]; } return x; }
            for (int i = 0; i < n; i++)
            {
                var a = nodes[i];
                if (!a.IsConnector || !a.parent.Spawned) continue;
                float r2 = a.Props.radius * a.Props.radius;
                for (int j = 0; j < n; j++)
                {
                    if (i == j || !nodes[j].parent.Spawned) continue;
                    if ((a.parent.Position - nodes[j].parent.Position).LengthHorizontalSquared <= r2)
                        parentIdx[Find_(i)] = Find_(j);
                }
            }
            var groups = new Dictionary<int, QiNet>();
            for (int i = 0; i < n; i++)
            {
                int root = Find_(i);
                if (!groups.TryGetValue(root, out var net)) { net = new QiNet(); groups[root] = net; nets.Add(net); }
                net.nodes.Add(nodes[i]);
                nodes[i].Net = net;
            }
        }

        public void Step(float dt)
        {
            foreach (var net in nets)
            {
                float supply = 0f, demand = 0f, cap = 0f, stored = 0f;
                var storages = new List<CompQiNode>();
                foreach (var nd in net.nodes)
                {
                    supply += nd.ProduceDay;
                    demand += nd.ConsumeDay;
                    if (nd.IsStorage) { storages.Add(nd); cap += nd.Props.storageCapacity; stored += nd.Stored; }
                }
                if (dt > 0f)
                    foreach (var s in storages) s.Stored -= s.Stored * s.Props.leakPerDay * dt;
                stored = storages.Sum(s => s.Stored);
                float delta = (supply - demand) * dt;
                if (dt <= 0f) net.Satisfied = supply >= demand || stored > 0f;
                else if (delta >= 0f)
                {
                    float add = delta;
                    foreach (var s in storages) { float room = s.Props.storageCapacity - s.Stored; float put = Mathf.Min(room, add); s.Stored += put; add -= put; }
                    // Lo que ya no cabe en los acumuladores se condensa (estanque de condensación) en vez de perderse.
                    if (add > 0f)
                        foreach (var nd in net.nodes)
                            if (nd.IsCondenser && add > 0f) add -= nd.Condense(add, dt);
                    net.Satisfied = true;
                }
                else
                {
                    float need = -delta;
                    if (stored >= need)
                    {
                        foreach (var s in storages) { float take = Mathf.Min(s.Stored, need); s.Stored -= take; need -= take; }
                        net.Satisfied = true;
                    }
                    else
                    {
                        foreach (var s in storages) s.Stored = 0f;
                        net.Satisfied = false;
                    }
                }
                net.SupplyDay = supply;
                net.DemandDay = demand;
                net.Capacity = cap;
                net.Stored = storages.Sum(s => s.Stored);
                foreach (var nd in net.nodes) nd.NotifyPowerChange();
            }
        }
    }

    public static class QiNetworkUtility
    {
        private static TerrainDef veinTerrain;
        public static TerrainDef VeinTerrain => veinTerrain ??= DefDatabase<TerrainDef>.GetNamedSilentFail("CVG_QiVein");

        /// <summary>Crea las vetas de qi del mapa (tras la Convergencia): una cerca del centro y otras repartidas. No toca suelos construidos ni cosas.</summary>
        /// <returns>true si el mapa ya tiene (o acaba de recibir) al menos una veta.</returns>
        public static bool EnsureVeins(Map map)
        {
            var vein = VeinTerrain;
            if (vein == null) return true;
            foreach (var c in map.AllCells) if (map.terrainGrid.TerrainAt(c) == vein) return true;
            bool painted = false;
            int count = Mathf.Clamp(map.Area / 22000, 2, 6);
            var rect = new CellRect(0, 0, map.Size.x, map.Size.z);
            for (int i = 0; i < count; i++)
            {
                IntVec3 center;
                bool ok = i == 0
                    ? CellFinderLoose.TryGetRandomCellWith(c => VeinSpotOk(c, map) && c.DistanceTo(map.Center) is > 12f and < 50f, map, 400, out center)
                    : CellFinderLoose.TryGetRandomCellWith(c => VeinSpotOk(c, map), map, 400, out center);
                if (!ok) continue;
                foreach (var c in GenRadial.RadialCellsAround(center, 1.9f, true))
                    if (c.InBounds(map) && PaintOk(c, map))
                    {
                        c.GetPlant(map)?.Destroy();   // pasto y matas silvestres: la veta los aparta
                        map.terrainGrid.SetTerrain(c, vein);
                        painted = true;
                    }
            }
            return painted;
        }

        private static bool VeinSpotOk(IntVec3 c, Map map)
        {
            if (c.DistanceToEdge(map) < 8) return false;
            foreach (var n in GenRadial.RadialCellsAround(c, 2.5f, true))
                if (!n.InBounds(map) || !PaintOk(n, map)) return false;
            return true;
        }

        // Un árbol o un cultivo bloquean la veta; el pasto y las matas silvestres no (si no, en bosques y praderas casi no habría vetas).
        private static bool SmallWildPlantOrNone(IntVec3 c, Map map)
        {
            var p = c.GetPlant(map);
            return p == null || (!p.def.plant.IsTree && c.GetZone(map) == null && p.Faction == null);
        }

        private static bool PaintOk(IntVec3 c, Map map)
        {
            var t = c.GetTerrain(map);
            return t != null && t.natural && !t.IsWater && t.affordances.Contains(TerrainAffordanceDefOf.Heavy) && !c.Roofed(map) && c.GetFirstBuilding(map) == null && SmallWildPlantOrNone(c, map) && !c.GetThingList(map).Any(x => x.def.category == ThingCategory.Building || x.def.category == ThingCategory.Item);
        }
    }

    // ---------------------------------------------------------------- colocación

    /// <summary>El edificio solo se puede construir con todas sus casillas sobre una veta de qi.</summary>
    public class PlaceWorker_OnQiVein : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            var vein = QiNetworkUtility.VeinTerrain;
            foreach (var c in GenAdj.OccupiedRect(loc, rot, checkingDef.Size))
                if (vein == null || (map.terrainGrid.TerrainAt(c) != vein && map.terrainGrid.BaseTerrainAt(c) != vein))
                    return "CVG_QiNet_NeedsVein".Translate();
            return true;
        }
    }

    /// <summary>Dibuja el radio de conexión (pilares) y líneas a los nodos de la red que quedarían unidos.</summary>
    public class PlaceWorker_QiNodeRadius : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            var map = Find.CurrentMap;
            var props = def.GetCompProperties<CompProperties_QiNode>();
            if (map == null || props == null) return;
            if (props.radius > 0f) GenDraw.DrawRadiusRing(center, props.radius);
            var mgr = map.GetComponent<QiNetworkManager>();
            if (mgr == null) return;
            foreach (var n in mgr.Nodes)
            {
                if (!n.parent.Spawned) continue;
                bool linked = props.radius > 0f ? center.DistanceTo(n.parent.Position) <= props.radius : n.IsConnector && n.parent.Position.DistanceTo(center) <= n.Props.radius;
                if (linked) GenDraw.DrawLineBetween(GenThing.TrueCenter(center, rot, def.size, def.Altitude), n.parent.TrueCenter(), SimpleColor.Cyan);
            }
        }
    }

    // ---------------------------------------------------------------- trabajo: infundir qi

    public class JobDriver_InfuseQi : JobDriver
    {
        private const int Duration = 300;
        private CompQiNode Node => TargetThingA.TryGetComp<CompQiNode>();

        public override bool TryMakePreToilReservations(bool errorOnFailed) => pawn.Reserve(TargetA, job, 1, -1, null, errorOnFailed);

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            var wait = Toils_General.Wait(Duration).WithProgressBarToilDelay(TargetIndex.A);
            wait.FailOn(() => CompCultivator.For(pawn)?.Realm.Resource.Current <= 1f);
            yield return wait;
            yield return new Toil
            {
                initAction = () =>
                {
                    var node = Node;
                    var res = CompCultivator.For(pawn)?.Realm.Resource;
                    if (node == null || res == null) return;
                    float amount = Mathf.Min(res.Current * 0.5f, node.Props.storageCapacity - node.Stored, 150f);
                    if (amount < 1f || !res.TryConsume(amount)) return;
                    node.Stored += amount;
                    Messages.Message("CVG_QiNet_Infused".Translate(pawn.LabelShort, amount.ToString("F0")), node.parent, MessageTypeDefOf.SilentInput, false);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }
    }

    // ---------------------------------------------------------------- parches: el "encendido" de los edificios lo decide la red de qi

    [HarmonyPatch(typeof(CompPowerTrader), nameof(CompPowerTrader.PowerOn), MethodType.Getter)]
    public static class Patch_CompPowerTrader_PowerOn_Qi
    {
        public static void Postfix(CompPowerTrader __instance, ref bool __result)
        {
            var parent = __instance.parent;
            if (parent?.def == null || parent.def.comps == null) return;
            var node = parent.TryGetComp<CompQiNode>();
            // el interruptor de la formacion manda: apagada con el boton, no esta encendida aunque haya qi
            if (node != null && node.IsConsumer) __result = node.Powered && (parent.TryGetComp<CompFlickable>()?.SwitchIsOn ?? true);
        }
    }

    [HarmonyPatch(typeof(CompPower), nameof(CompPower.CompInspectStringExtra))]
    public static class Patch_CompPower_Inspect_Qi
    {
        public static void Postfix(CompPower __instance, ref string __result)
        {
            if (__instance.parent.TryGetComp<CompQiNode>() != null) __result = null;
        }
    }

    /// <summary>CompPowerTrader tiene su propio texto ("Energía generada: 0 W"); en edificios de la red de qi no se muestra.</summary>
    [HarmonyPatch(typeof(CompPowerTrader), nameof(CompPowerTrader.CompInspectStringExtra))]
    public static class Patch_CompPowerTrader_Inspect_Qi
    {
        public static void Postfix(CompPowerTrader __instance, ref string __result)
        {
            if (__instance.parent.TryGetComp<CompQiNode>() != null) __result = null;
        }
    }
}
