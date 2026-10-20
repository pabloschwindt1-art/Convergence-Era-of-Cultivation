using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Convergence
{
    /// <summary>
    /// Fuentes de qi que no son de flujo constante y el mapa de qi (docs/modulos/RED_QI.md):
    /// la marea celeste para las formaciones del aire, el pararrayos del trueno celestial (ráfagas de qi con tormentas y
    /// tribulaciones) y la capa del mapa que pinta la concentración de qi, como la capa de fertilidad vanilla.
    /// </summary>
    public static class QiSourceUtility
    {
        private static GameConditionDef tide, ebb;

        /// <summary>×1,5 con la Marea de Qi, ×0,5 con el Reflujo de Qi, ×1 si no.</summary>
        public static float TideFactor(Map map)
        {
            tide ??= DefDatabase<GameConditionDef>.GetNamedSilentFail("CVG_QiTide");
            ebb ??= DefDatabase<GameConditionDef>.GetNamedSilentFail("CVG_QiEbb");
            var conditions = map?.gameConditionManager;
            if (conditions == null) return 1f;
            if (tide != null && conditions.ConditionIsActive(tide)) return 1.5f;
            if (ebb != null && conditions.ConditionIsActive(ebb)) return 0.5f;
            return 1f;
        }
    }

    public class CompProperties_QiLightningRod : CompProperties
    {
        /// <summary>Los rayos de tormenta que caerían a esta distancia caen en el pararrayos.</summary>
        public float attractRadius = 12f;
        /// <summary>Qi por rayo de tormenta (va al pararrayos y lo que sobra a los acumuladores de su red).</summary>
        public float qiPerStrike = 200f;
        /// <summary>Qi por rayo de tribulación caído a <see cref="attractRadius"/> (no lo desvía: la tribulación busca al colono).</summary>
        public float qiPerTribulationStrike = 120f;

        public CompProperties_QiLightningRod() => compClass = typeof(CompQiLightningRod);
    }

    /// <summary>Pararrayos del trueno celestial. Necesita un <see cref="CompQiNode"/> con almacenamiento en el mismo edificio.</summary>
    public class CompQiLightningRod : ThingComp
    {
        private static readonly List<CompQiLightningRod> rods = new List<CompQiLightningRod>();

        public CompProperties_QiLightningRod Props => (CompProperties_QiLightningRod)props;
        private CompQiNode Node => parent.GetComp<CompQiNode>();

        public override void PostSpawnSetup(bool respawningAfterLoad) { if (!rods.Contains(this)) rods.Add(this); }
        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish) => rods.Remove(this);

        /// <summary>Guarda el qi en el propio pararrayos y reparte lo que sobra entre los acumuladores de su red.</summary>
        public void Absorb(float amount)
        {
            var node = Node;
            if (node == null || amount <= 0f) return;
            float room = node.Props.storageCapacity - node.Stored;
            float take = Mathf.Min(room, amount);
            node.Stored += take;
            amount -= take;
            var net = node.Net;
            if (net == null || amount <= 0f) return;
            foreach (var other in net.nodes)
            {
                if (amount <= 0f) break;
                if (other == node || !other.IsStorage) continue;
                float r = other.Props.storageCapacity - other.Stored;
                float t = Mathf.Min(r, amount);
                other.Stored += t;
                amount -= t;
            }
        }

        public static CompQiLightningRod Nearest(Map map, IntVec3 cell, bool needsSky)
        {
            CompQiLightningRod best = null;
            float bestDist = float.MaxValue;
            foreach (var rod in rods)
            {
                if (rod.parent.Map != map || rod.parent.Faction != Faction.OfPlayer) continue;
                if (needsSky && map.roofGrid.Roofed(rod.parent.Position)) continue;
                float d = rod.parent.Position.DistanceTo(cell);
                if (d <= rod.Props.attractRadius && d < bestDist) { best = rod; bestDist = d; }
            }
            return best;
        }

        /// <summary>Rayo de tribulación: carga cada pararrayos cercano sin desviarlo.</summary>
        public static void OnTribulationStrike(Map map, IntVec3 cell)
        {
            foreach (var rod in rods)
                if (rod.parent.Map == map && rod.parent.Position.DistanceTo(cell) <= rod.Props.attractRadius)
                    rod.Absorb(rod.Props.qiPerTribulationStrike);
        }

        public override void PostDrawExtraSelectionOverlays() => GenDraw.DrawRadiusRing(parent.Position, Props.attractRadius);
    }

    /// <summary>Un rayo de tormenta que cae cerca de un pararrayos del jugador cae en él: no hay explosión ni fuego, solo qi.</summary>
    [HarmonyPatch(typeof(WeatherEvent_LightningStrike), nameof(WeatherEvent_LightningStrike.FireEvent))]
    public static class Patch_LightningStrike_QiRod
    {
        private static readonly AccessTools.FieldRef<WeatherEvent_LightningStrike, IntVec3> StrikeLoc =
            AccessTools.FieldRefAccess<WeatherEvent_LightningStrike, IntVec3>("strikeLoc");
        private static readonly AccessTools.FieldRef<WeatherEvent_LightningStrike, Mesh> BoltMesh =
            AccessTools.FieldRefAccess<WeatherEvent_LightningStrike, Mesh>("boltMesh");
        private static readonly AccessTools.FieldRef<WeatherEvent, Map> MapField = AccessTools.FieldRefAccess<WeatherEvent, Map>("map");

        public static bool Prefix(WeatherEvent_LightningStrike __instance)
        {
            var map = MapField(__instance);
            ref IntVec3 loc = ref StrikeLoc(__instance);
            if (!loc.IsValid)
                loc = CellFinderLoose.RandomCellWith(sq => sq.Standable(map) && !map.roofGrid.Roofed(sq), map);
            var rod = CompQiLightningRod.Nearest(map, loc, needsSky: true);
            if (rod == null) return true;

            loc = rod.parent.Position;
            SoundDefOf.Thunder_OffMap.PlayOneShotOnCamera(map);
            BoltMesh(__instance) = LightningBoltMeshPool.RandomBoltMesh;
            var v = loc.ToVector3Shifted();
            for (int i = 0; i < 4; i++)
            {
                FleckMaker.ThrowMicroSparks(v, map);
                FleckMaker.ThrowLightningGlow(v, map, 1.5f);
            }
            SoundDefOf.Thunder_OnMap.PlayOneShot(SoundInfo.InMap(new TargetInfo(loc, map)));
            rod.Absorb(rod.Props.qiPerStrike);
            return false;
        }
    }

    /// <summary>Capa del mapa con la concentración de qi (botón junto a las demás capas, como la de fertilidad).</summary>
    [StaticConstructorOnStartup]
    public class MapComponent_QiOverlay : MapComponent, ICellBoolGiver
    {
        public static bool Show;
        private static readonly Texture2D Icon = ContentFinder<Texture2D>.Get("UI/Overlays/CVG_QiMap", false)
            ?? ContentFinder<Texture2D>.Get("UI/Overlays/CVG_NoQi", false) ?? BaseContent.BadTex;

        private CellBoolDrawer drawer;
        private float[] values;
        private int nextRefresh;

        public MapComponent_QiOverlay(Map map) : base(map) { }

        public Color Color => Color.white;
        public bool GetCellBool(int index) => values != null && !map.fogGrid.IsFogged(index);

        public Color GetCellExtraColor(int index)
        {
            float v = values[index];
            if (v < 0f) return new Color(0.9f, 0.3f, 0.9f); // veta de qi
            // 0,5 → azul tenue; 1,5 → cian; 2,5 → verde jade; 4+ → dorado
            if (v < 1.5f) return Color.Lerp(new Color(0.1f, 0.15f, 0.45f), new Color(0.2f, 0.8f, 0.9f), Mathf.InverseLerp(0.5f, 1.5f, v));
            if (v < 2.5f) return Color.Lerp(new Color(0.2f, 0.8f, 0.9f), new Color(0.3f, 0.9f, 0.4f), v - 1.5f);
            return Color.Lerp(new Color(0.3f, 0.9f, 0.4f), new Color(1f, 0.85f, 0.25f), Mathf.InverseLerp(2.5f, 4f, v));
        }

        public override void MapComponentUpdate()
        {
            if (!Show || Find.CurrentMap != map || WorldRendererUtility.WorldRendered) return;
            drawer ??= new CellBoolDrawer(this, map.Size.x, map.Size.z, 3610, 0.45f);
            if (values == null || Find.TickManager.TicksGame >= nextRefresh)
            {
                Recompute();
                nextRefresh = Find.TickManager.TicksGame + 600;
                drawer.SetDirty();
            }
            drawer.MarkForDraw();
            drawer.CellBoolDrawerUpdate();
        }

        private void Recompute()
        {
            values ??= new float[map.cellIndices.NumGridCells];
            var vein = QiNetworkUtility.VeinTerrain;
            for (int i = 0; i < values.Length; i++)
            {
                var c = map.cellIndices.IndexToCell(i);
                values[i] = vein != null && c.GetTerrain(map) == vein ? -1f : QiConcentrationUtility.ConcentrationAt(c, map);
            }
        }

        [HarmonyPatch(typeof(PlaySettings), nameof(PlaySettings.DoPlaySettingsGlobalControls))]
        public static class Patch_PlaySettings
        {
            public static void Postfix(WidgetRow row, bool worldView)
            {
                if (worldView || row == null || !GameComponent_Convergence.Active) return;
                row.ToggleableIcon(ref Show, Icon, "CVG_QiMapToggle".Translate(), SoundDefOf.Mouseover_ButtonToggle);
            }
        }
    }

    /// <summary>
    /// Formación de escarcha: enfría la habitación donde está, sin salida de calor (es una formación, no una máquina).
    /// Misma lógica que el calefactor vanilla con energía negativa; rinde menos cuanto más frío hace.
    /// </summary>
    public class Building_FrostFormation : Building_TempControl
    {
        public override void TickRare()
        {
            if (!Spawned || !compPowerTrader.PowerOn) return;
            float ambient = AmbientTemperature;
            float eff = ambient > -10f ? 1f : (ambient < -100f ? 0f : Mathf.InverseLerp(-100f, -10f, ambient));
            float energy = compTempControl.Props.energyPerSecond * eff * 4.1666665f;
            float change = GenTemperature.ControlTemperatureTempChange(Position, Map, energy, compTempControl.TargetTemperature);
            bool working = !Mathf.Approximately(change, 0f);
            if (working) this.GetRoom().Temperature += change;
            compTempControl.operatingAtHighPower = working;
        }
    }
}
