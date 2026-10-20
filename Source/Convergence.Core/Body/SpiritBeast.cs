using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Datos del despertar de bestias espirituales (va en el HediffDef <c>CVG_SpiritBeast</c>): probabilidad y nivel
    /// según el <c>combatPower</c> del animal (cualquier animal de cualquier mod) y lo que da al carnearlo.
    /// </summary>
    public class DefModExtension_SpiritBeast : DefModExtension
    {
        /// <summary>combatPower → probabilidad de despertar (salvaje).</summary>
        public SimpleCurve awakenChanceCurve;
        /// <summary>Multiplicador de la probabilidad para animales domesticados.</summary>
        public float tameChanceFactor = 0.5f;
        /// <summary>combatPower → nivel medio (se suma un poco de azar).</summary>
        public SimpleCurve levelCurve;
        /// <summary>Multiplicador de vida (Pawn.HealthScale) por nivel (índice 0 = nivel 1).</summary>
        public List<float> healthScaleByLevel = new List<float> { 1.3f, 1.8f, 2.5f, 3.5f };
        /// <summary>Carne espiritual al carnear: × tamaño corporal × nivel.</summary>
        public float meatPerBodySize = 8f;
        /// <summary>Sangre de bestia al carnear: × tamaño corporal × nivel.</summary>
        public float bloodPerBodySize = 2f;
        /// <summary>Nivel desde el que deja un núcleo de bestia (y cuántos por nivel por encima).</summary>
        public int coreMinLevel = 3;
        /// <summary>Cuero que deja la bestia al carnear (opcional): × tamaño corporal × nivel.</summary>
        public ThingDef leatherDef;
        public float leatherPerBodySize = 3f;
    }

    /// <summary>
    /// Bestia espiritual: un estado (no un animal nuevo) con niveles 1-4 = severidad. Los stats van por etapa en el XML
    /// (velocidad, daño, daño recibido); la vida, en <see cref="DefModExtension_SpiritBeast.healthScaleByLevel"/>. El nivel
    /// se cachea por pawn para la supresión de reino y la vida (consultas O(1)).
    /// </summary>
    public class Hediff_SpiritBeast : HediffWithComps
    {
        private static readonly Dictionary<int, Hediff_SpiritBeast> byPawn = new Dictionary<int, Hediff_SpiritBeast>();

        public int Level => Mathf.Clamp(Mathf.RoundToInt(Severity), 1, Mathf.Max(1, def.stages?.Count ?? 1));

        public static Hediff_SpiritBeast For(Pawn pawn)
        {
            if (pawn == null) return null;
            return byPawn.TryGetValue(pawn.thingIDNumber, out var h) && h.pawn == pawn && !h.ShouldRemove ? h : null;
        }

        /// <summary>Nivel de bestia (0 = no despertó).</summary>
        public static int LevelOf(Pawn pawn) => For(pawn)?.Level ?? 0;

        public static float HealthFactorOf(Pawn pawn)
        {
            var h = For(pawn);
            if (h == null) return 1f;
            var list = SpiritBeastUtility.Ext?.healthScaleByLevel;
            return list == null || list.Count == 0 ? 1f : list[Mathf.Clamp(h.Level - 1, 0, list.Count - 1)];
        }

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            byPawn[pawn.thingIDNumber] = this;
            pawn.health.hediffSet.DirtyCache();
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            if (byPawn.TryGetValue(pawn.thingIDNumber, out var h) && h == this) byPawn.Remove(pawn.thingIDNumber);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.PostLoadInit && pawn != null) byPawn[pawn.thingIDNumber] = this;
        }
    }

    /// <summary>Despertar de animales tras la Convergencia y productos al carnear.</summary>
    public static class SpiritBeastUtility
    {
        private static DefModExtension_SpiritBeast ext;

        public static DefModExtension_SpiritBeast Ext => ext ??= CVG_DefOf.CVG_SpiritBeast?.GetModExtension<DefModExtension_SpiritBeast>();

        public static int MaxLevel => Mathf.Max(1, CVG_DefOf.CVG_SpiritBeast?.stages?.Count ?? 1);

        public static float AwakenChance(Pawn pawn)
        {
            if (pawn?.RaceProps == null || !pawn.RaceProps.Animal || pawn.Dead || Ext?.awakenChanceCurve == null) return 0f;
            float chance = Ext.awakenChanceCurve.Evaluate(pawn.kindDef.combatPower) * ConvergenceMod.Settings.spiritBeastChanceFactor;
            if (pawn.Faction != null) chance *= Ext.tameChanceFactor;
            return Mathf.Clamp01(chance);
        }

        public static int RollLevel(Pawn pawn)
        {
            float mean = Ext?.levelCurve?.Evaluate(pawn.kindDef.combatPower) ?? 1f;
            return Mathf.Clamp(Mathf.RoundToInt(mean + Rand.Range(-0.6f, 0.9f)), 1, MaxLevel);
        }

        /// <summary>Tira el despertar (tras la Convergencia). Devuelve true si despertó.</summary>
        public static bool TryAwaken(Pawn pawn)
        {
            if (!GameComponent_Convergence.Active || Hediff_SpiritBeast.For(pawn) != null) return false;
            if (!Rand.Chance(AwakenChance(pawn))) return false;
            Awaken(pawn, RollLevel(pawn));
            return true;
        }

        public static void Awaken(Pawn pawn, int level)
        {
            if (pawn?.health == null || CVG_DefOf.CVG_SpiritBeast == null) return;
            var existing = pawn.health.hediffSet.GetFirstHediffOfDef(CVG_DefOf.CVG_SpiritBeast);
            if (existing != null)
            {
                existing.Severity = Mathf.Clamp(level, 1, MaxLevel);
                pawn.health.hediffSet.DirtyCache();
                return;
            }
            var hediff = HediffMaker.MakeHediff(CVG_DefOf.CVG_SpiritBeast, pawn);
            hediff.Severity = Mathf.Clamp(level, 1, MaxLevel);
            pawn.health.AddHediff(hediff);
            if (pawn.Spawned)
            {
                FleckMaker.ThrowLightningGlow(pawn.DrawPos, pawn.Map, 1.5f);
                FleckMaker.ThrowMicroSparks(pawn.DrawPos, pawn.Map);
            }
        }

        /// <summary>Al ocurrir la Convergencia: los animales del mapa pueden despertar. Devuelve cuántos.</summary>
        public static int AwakenAnimalsOnMap(Map map)
        {
            int count = 0;
            foreach (var p in map.mapPawns.AllPawnsSpawned.ToList())
                if (p.RaceProps.Animal && TryAwaken(p)) count++;
            return count;
        }

        /// <summary>Productos extra de una bestia: carne espiritual, sangre de bestia y (nivel alto) núcleo.</summary>
        public static IEnumerable<Thing> ExtraButcherProducts(Pawn beast, int level, float efficiency)
        {
            var e = Ext;
            if (e == null) yield break;
            float size = beast.BodySize;
            int meat = Mathf.Max(1, GenMath.RoundRandom(size * e.meatPerBodySize * level * efficiency));
            int blood = Mathf.Max(1, GenMath.RoundRandom(size * e.bloodPerBodySize * level * efficiency));
            foreach (var t in MakeStacks(CVG_DefOf.CVG_SpiritMeat, meat)) yield return t;
            foreach (var t in MakeStacks(CVG_DefOf.CVG_BeastBlood, blood)) yield return t;
            if (e.leatherDef != null)
                foreach (var t in MakeStacks(e.leatherDef, Mathf.Max(1, GenMath.RoundRandom(size * e.leatherPerBodySize * level * efficiency)))) yield return t;
            if (level >= e.coreMinLevel)
                foreach (var t in MakeStacks(CVG_DefOf.CVG_BeastCore, 1 + level - e.coreMinLevel)) yield return t;
        }

        private static IEnumerable<Thing> MakeStacks(ThingDef def, int count)
        {
            if (def == null) yield break;
            while (count > 0)
            {
                var t = ThingMaker.MakeThing(def);
                t.stackCount = Mathf.Min(count, def.stackLimit);
                count -= t.stackCount;
                yield return t;
            }
        }
    }

    /// <summary>Carnear una bestia espiritual da sus productos además de los normales.</summary>
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.ButcherProducts))]
    public static class Patch_Pawn_ButcherProducts
    {
        public static void Postfix(Pawn __instance, Pawn butcher, float efficiency, ref IEnumerable<Thing> __result)
        {
            if (!__instance.RaceProps.Animal || CVG_DefOf.CVG_SpiritBeast == null) return;
            var hediff = __instance.health?.hediffSet?.GetFirstHediffOfDef(CVG_DefOf.CVG_SpiritBeast) as Hediff_SpiritBeast;
            if (hediff == null) return;
            __result = __result.Concat(SpiritBeastUtility.ExtraButcherProducts(__instance, hediff.Level, efficiency)).ToList();
            if (butcher != null) SectEvents.Record(SectEvents.ExtractedEssence, butcher);   // preceptos de extracción de esencia
        }
    }
}
