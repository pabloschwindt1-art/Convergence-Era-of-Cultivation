using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Economía paralela (docs/DISENO.md §8, ronda 3): los objetos del mod (píldoras, esencias, tesoros, piedras y jade
    /// espiritual, metales, armas espirituales, marionetas, muebles de cultivo…) cuentan solo una fracción en la riqueza
    /// vanilla que calcula los puntos de los asaltos (una píldora de 5k no trae 200 asaltantes), sin tocar su valor de
    /// mercado ni de comercio. Su valor completo, más los reinos y técnicas de los colonos, forma la "riqueza de cultivo",
    /// que da puntos extra a los asaltos de facciones de cultivo (sectas) y a los eventos del mod (bestias, jiangshi).
    /// Datos en <c>Defs/CultivationWealth.xml</c>; factores en los ajustes y en la dificultad de cultivo.
    /// </summary>
    public class CultivationWealthDef : Def
    {
        /// <summary>Los defs de mods cuyo packageId empieza así son "objetos de cultivo".</summary>
        public List<string> packageIdPrefixes = new List<string>();
        /// <summary>Riqueza de cultivo por reino (order del reino → riqueza en la etapa 1).</summary>
        public SimpleCurve realmWealth;
        /// <summary>Fracción extra de la riqueza del reino por cada etapa por encima de la primera.</summary>
        public float perStageFraction = 0.25f;
        public float perTechnique = 150f;
        /// <summary>Riqueza de cultivo → puntos de amenaza extra (antes del factor de los ajustes y la dificultad).</summary>
        public SimpleCurve pointsPerWealth;
        /// <summary>Fracción de esos puntos que se suma a las manadas de animales enloquecidos (las bestias huelen el qi).</summary>
        public float manhunterFactor = 0.5f;

        public static CultivationWealthDef Main => DefDatabase<CultivationWealthDef>.GetNamedSilentFail("CVG_CultivationWealth");
    }

    /// <summary>
    /// En un ThingDef: fuerza si cuenta como objeto de cultivo (aunque no sea de un mod de Convergence, o para excluir uno
    /// que sí lo es) y, opcionalmente, su fracción en la riqueza vanilla.
    /// </summary>
    public class DefModExtension_CultivationWealth : DefModExtension
    {
        public bool cultivation = true;
        /// <summary>Fracción en la riqueza vanilla (−1 = la de los ajustes).</summary>
        public float vanillaFactor = -1f;
    }

    /// <summary>En un FactionDef: sus asaltos suman los puntos de la riqueza de cultivo (las sectas ya lo hacen por ser de la Convergencia).</summary>
    public class DefModExtension_CultivationThreat : DefModExtension
    {
        public float factor = 1f;
    }

    public class CultivationWealthRecord
    {
        public float items, buildings, pawns, realms, techniques;
        /// <summary>Lo que se quitó de la riqueza vanilla.</summary>
        public float removedFromVanilla;
        public float Total => items + buildings + pawns + realms + techniques;
    }

    public static class CultivationWealth
    {
        private static readonly ConditionalWeakTable<Map, CultivationWealthRecord> records = new ConditionalWeakTable<Map, CultivationWealthRecord>();
        private static readonly List<Thing> tmpThings = new List<Thing>();
        // -2 = sin calcular, -1 = no es de cultivo, >= 0 = fracción vanilla propia (o la de los ajustes si es NaN).
        private static float[] factorCache;

        private static readonly AccessTools.FieldRef<WealthWatcher, Map> mapRef = AccessTools.FieldRefAccess<WealthWatcher, Map>("map");
        private static readonly AccessTools.FieldRef<WealthWatcher, float> itemsRef = AccessTools.FieldRefAccess<WealthWatcher, float>("wealthItems");
        private static readonly AccessTools.FieldRef<WealthWatcher, float> buildingsRef = AccessTools.FieldRefAccess<WealthWatcher, float>("wealthBuildings");
        private static readonly AccessTools.FieldRef<WealthWatcher, float> pawnsRef = AccessTools.FieldRefAccess<WealthWatcher, float>("wealthPawns");

        public static void ClearCache() => factorCache = null;

        /// <summary>Si el def es un objeto de cultivo y qué fracción cuenta en la riqueza vanilla; −1 si no es de cultivo.</summary>
        public static float VanillaFactor(ThingDef def)
        {
            if (def == null) return -1f;
            if (factorCache == null || factorCache.Length <= def.index)
            {
                factorCache = new float[DefDatabase<ThingDef>.DefCount + 16];
                for (int i = 0; i < factorCache.Length; i++) factorCache[i] = -2f;
            }
            float cached = factorCache[def.index];
            if (cached == -2f) factorCache[def.index] = cached = Classify(def);
            if (cached == -1f) return -1f;
            return float.IsNaN(cached) ? Mathf.Clamp01(CultivationDifficulty.Current.vanillaWealthFactor) : cached;
        }

        private static float Classify(ThingDef def)
        {
            var ext = def.GetModExtension<DefModExtension_CultivationWealth>();
            if (ext != null) return !ext.cultivation ? -1f : ext.vanillaFactor >= 0f ? ext.vanillaFactor : float.NaN;
            if (def.race != null && def.race.Humanlike) return -1f;
            if (def.category != ThingCategory.Item && def.category != ThingCategory.Building && def.category != ThingCategory.Pawn) return -1f;
            string package = def.modContentPack?.PackageId;
            var main = CultivationWealthDef.Main;
            if (package == null || main == null) return -1f;
            return main.packageIdPrefixes.Any(p => package.StartsWith(p, System.StringComparison.OrdinalIgnoreCase)) ? float.NaN : -1f;
        }

        public static bool IsCultivationThing(ThingDef def) => VanillaFactor(def) >= 0f;

        /// <summary>Riqueza de cultivo del mapa (la del último recuento de la riqueza vanilla).</summary>
        public static CultivationWealthRecord For(Map map)
        {
            if (map == null) return new CultivationWealthRecord();
            _ = map.wealthWatcher.WealthTotal; // fuerza el recuento si toca
            return records.GetValue(map, _ => new CultivationWealthRecord());
        }

        /// <summary>Puntos de amenaza extra por la riqueza de cultivo (con los factores de ajustes y dificultad).</summary>
        public static float ThreatPoints(Map map)
        {
            var main = CultivationWealthDef.Main;
            if (map == null || main?.pointsPerWealth == null || !GameComponent_Convergence.Active) return 0f;
            return main.pointsPerWealth.Evaluate(For(map).Total) * CultivationDifficulty.Current.cultivationThreatFactor;
        }

        /// <summary>Tras el recuento vanilla: separa los objetos de cultivo y calcula la riqueza de cultivo.</summary>
        public static void Recount(WealthWatcher watcher)
        {
            var map = mapRef(watcher);
            if (map == null) return;
            var rec = records.GetValue(map, _ => new CultivationWealthRecord());
            rec.items = rec.buildings = rec.pawns = rec.realms = rec.techniques = rec.removedFromVanilla = 0f;

            tmpThings.Clear();
            ThingOwnerUtility.GetAllThingsRecursively(map, ThingRequest.ForGroup(ThingRequestGroup.HaulableEver), tmpThings, false, WealthWatcher.WealthItemsFilter);
            float removedItems = 0f;
            for (int i = 0; i < tmpThings.Count; i++)
            {
                var t = tmpThings[i];
                float f = VanillaFactor(t.def);
                if (f < 0f || !t.SpawnedOrAnyParentSpawned || t.PositionHeld.Fogged(map)) continue;
                float v = t.MarketValue * t.stackCount;
                rec.items += v;
                removedItems += v * (1f - f);
            }
            tmpThings.Clear();

            float removedBuildings = 0f;
            var buildings = map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial);
            for (int i = 0; i < buildings.Count; i++)
            {
                var b = buildings[i];
                if (b.Faction != Faction.OfPlayer) continue;
                float f = VanillaFactor(b.def);
                if (f < 0f) continue;
                float v = b.GetStatValue(StatDefOf.MarketValueIgnoreHp);
                rec.buildings += v;
                removedBuildings += v * (1f - f);
            }

            float removedPawns = 0f;
            var main = CultivationWealthDef.Main;
            foreach (var p in map.mapPawns.PawnsInFaction(Faction.OfPlayer))
            {
                if (p.IsQuestLodger()) continue;
                float f = VanillaFactor(p.def);
                if (f >= 0f)
                {
                    float v = p.MarketValue;
                    rec.pawns += v;
                    removedPawns += v * (1f - f);
                }
                // El Maestro y el sabio del vacío están de paso, como un huésped de misión: no cuentan (playtest 2026-10-24: un
                // Núcleo Dorado sumaba más de 50.000 de riqueza y las incursiones del día 11 venían con 130 puntos).
                if (main != null && p.IsFreeColonist && !(GameComponent_Mentor.Instance?.IsMentor(p) ?? false)
                    && !(GameComponent_ChaosSage.Instance?.IsSage(p) ?? false))
                    AddCultivator(rec, p, main);
            }

            itemsRef(watcher) = Mathf.Max(0f, itemsRef(watcher) - removedItems);
            buildingsRef(watcher) = Mathf.Max(0f, buildingsRef(watcher) - removedBuildings);
            pawnsRef(watcher) = Mathf.Max(0f, pawnsRef(watcher) - removedPawns);
            rec.removedFromVanilla = removedItems + removedBuildings + removedPawns;
        }

        private static void AddCultivator(CultivationWealthRecord rec, Pawn pawn, CultivationWealthDef main)
        {
            var comp = CompCultivator.For(pawn);
            if (comp == null) return;
            if (main.realmWealth != null)
            {
                foreach (var t in comp.ActiveTrackers)
                {
                    if (!t.Started) continue;
                    rec.realms += main.realmWealth.Evaluate(t.Realm.order) * (1f + main.perStageFraction * (t.Stage - 1));
                }
            }
            var techniques = comp.TechniquesIfAny;
            if (techniques != null) rec.techniques += techniques.Unlocked.Count * main.perTechnique;
        }

        public static string Explain(Map map)
        {
            var rec = For(map);
            return "CVG_CultivationWealthTip".Translate(rec.Total.ToStringMoney(), rec.items.ToStringMoney(), rec.buildings.ToStringMoney(),
                rec.pawns.ToStringMoney(), rec.realms.ToStringMoney(), rec.techniques.ToStringMoney(), rec.removedFromVanilla.ToStringMoney(),
                ThreatPoints(map).ToString("F0"));
        }
    }

    [HarmonyPatch(typeof(WealthWatcher), nameof(WealthWatcher.ForceRecount))]
    public static class Patch_WealthWatcher_ForceRecount
    {
        public static void Postfix(WealthWatcher __instance) => CultivationWealth.Recount(__instance);
    }

    /// <summary>Los asaltos de facciones de cultivo suman los puntos de la riqueza de cultivo del mapa.</summary>
    [HarmonyPatch(typeof(IncidentWorker_Raid), nameof(IncidentWorker_Raid.AdjustedRaidPoints))]
    public static class Patch_IncidentWorker_Raid_AdjustedRaidPoints
    {
        public static void Postfix(ref float __result, Faction faction, IIncidentTarget target)
        {
            if (faction?.def == null || !(target is Map map)) return;
            float factor = faction.def.GetModExtension<DefModExtension_CultivationThreat>()?.factor
                ?? (faction.def.HasModExtension<DefModExtension_ConvergenceFaction>() ? 1f : 0f);
            if (factor > 0f) __result += CultivationWealth.ThreatPoints(map) * factor;
        }
    }

    /// <summary>Las bestias huelen el qi: las manadas enloquecidas suman parte de los puntos de la riqueza de cultivo.</summary>
    [HarmonyPatch(typeof(IncidentWorker_AggressiveAnimals), "TryExecuteWorker")]
    public static class Patch_IncidentWorker_AggressiveAnimals
    {
        public static void Prefix(IncidentParms parms)
        {
            var main = CultivationWealthDef.Main;
            if (main == null || main.manhunterFactor <= 0f || !(parms.target is Map map)) return;
            parms.points += CultivationWealth.ThreatPoints(map) * main.manhunterFactor;
        }
    }
}
