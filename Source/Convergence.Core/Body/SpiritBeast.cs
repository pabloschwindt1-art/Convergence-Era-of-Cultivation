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
        /// <summary>Probabilidad de que salga un rango por encima del de su especie (élite).</summary>
        public float eliteChance = 0.15f;
        /// <summary>Desde este combatPower la especie despierta como bestia soberana (jefe).</summary>
        public float sovereignMinCombatPower = 600f;
        public HediffDef sovereignHediff;
        /// <summary>Piedras espirituales que deja al morir, por rango (índice 0 = rango 1).</summary>
        public List<int> deathStonesByLevel = new List<int>();
        /// <summary>
        /// Reino equivalente por rango (índice 0 = rango 1) para la supresión de reino: el daño de un arma de grado menor se
        /// reduce como contra un cultivador de ese reino (Core: <see cref="BeastAndChaosDamage"/>). Solo las armas de qi pegan entero.
        /// </summary>
        public List<int> realmByLevel = new List<int> { 0, 1, 1, 2, 2, 3 };
        /// <summary>
        /// El mundo despierta de a poco (playtest 2026-10-24: una manada de wargos de rango 4 al día 20 mató a la colonia): la
        /// chance de despertar va de <see cref="awakenRampStart"/> a 1 en <see cref="awakenRampDays"/> días desde la
        /// Convergencia, y el rango tope empieza en <see cref="rankCapStart"/> y sube uno cada <see cref="rankCapDaysPerRank"/> días.
        /// </summary>
        public float awakenRampStart = 0.3f;
        public float awakenRampDays = 60f;
        public int rankCapStart = 2;
        public float rankCapDaysPerRank = 15f;
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

        /// <summary>Rango de la especie (siempre el mismo: lo decide su combatPower), a veces uno más (élite).</summary>
        public static int SpeciesLevel(PawnKindDef kind) =>
            Mathf.Clamp(Mathf.RoundToInt(Ext?.levelCurve?.Evaluate(kind?.combatPower ?? 0f) ?? 1f), 1, MaxLevel);

        public static int RollLevel(Pawn pawn)
        {
            int level = SpeciesLevel(pawn.kindDef);
            if (Rand.Chance(Ext?.eliteChance ?? 0f)) level++;
            return Mathf.Clamp(level, 1, MaxLevel);
        }

        public static bool IsSovereignSpecies(PawnKindDef kind) => kind != null && Ext?.sovereignHediff != null && kind.combatPower >= Ext.sovereignMinCombatPower;

        /// <summary>Tira el despertar (tras la Convergencia). Devuelve true si despertó.</summary>
        public static bool TryAwaken(Pawn pawn)
        {
            if (!GameComponent_Convergence.Active || Hediff_SpiritBeast.For(pawn) != null) return false;
            if (!Rand.Chance(AwakenChance(pawn) * AwakenRamp)) return false;
            Awaken(pawn, Mathf.Min(RollLevel(pawn), RankCapNow));
            return true;
        }

        private static float DaysSinceConvergence
        {
            get
            {
                var c = GameComponent_Convergence.Instance;
                return c == null || !c.Occurred ? 0f : (Find.TickManager.TicksGame - c.OccurredTick) / (float)GenDate.TicksPerDay;
            }
        }

        /// <summary>Factor de la chance de despertar según los días desde la Convergencia (el qi se filtra de a poco).</summary>
        public static float AwakenRamp
        {
            get
            {
                var e = Ext;
                if (e == null || e.awakenRampDays <= 0f) return 1f;
                return Mathf.Lerp(e.awakenRampStart, 1f, Mathf.Clamp01(DaysSinceConvergence / e.awakenRampDays));
            }
        }

        /// <summary>Rango más alto que puede despertar hoy.</summary>
        public static int RankCapNow
        {
            get
            {
                var e = Ext;
                if (e == null || e.rankCapDaysPerRank <= 0f) return MaxLevel;
                return Mathf.Clamp(e.rankCapStart + Mathf.FloorToInt(DaysSinceConvergence / e.rankCapDaysPerRank), 1, MaxLevel);
            }
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
            if (IsSovereignSpecies(pawn.kindDef) && level >= SpeciesLevel(pawn.kindDef) && !pawn.health.hediffSet.HasHediff(Ext.sovereignHediff))
                pawn.health.AddHediff(Ext.sovereignHediff);
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

    /// <summary>
    /// Presión de la bestia soberana (aura): cada pocos segundos, los enemigos humanos cerca y de reino más bajo que el rango de la
    /// bestia reciben <see cref="HediffCompProperties_BeastPressure.hediff"/>. La primera vez que aparece en un mapa del jugador, una
    /// carta lo avisa.
    /// </summary>
    public class HediffCompProperties_BeastPressure : HediffCompProperties
    {
        public float radius = 9.9f;
        public HediffDef hediff;
        public HediffCompProperties_BeastPressure() { compClass = typeof(HediffComp_BeastPressure); }
    }

    public class HediffComp_BeastPressure : HediffComp
    {
        private bool announced;
        private HediffCompProperties_BeastPressure Props => (HediffCompProperties_BeastPressure)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            var beast = Pawn;
            if (!beast.IsHashIntervalTick(250) || !beast.Spawned || beast.Downed || !GameComponent_Convergence.Active) return;
            var map = beast.Map;
            if (!announced && map.IsPlayerHome)
            {
                announced = true;
                Find.LetterStack.ReceiveLetter("CVG_BeastSovereign_Label".Translate(beast.LabelShortCap),
                    "CVG_BeastSovereign_Text".Translate(beast.Named("BEAST"), Hediff_SpiritBeast.LevelOf(beast)), LetterDefOf.ThreatSmall, beast);
            }
            if (Props.hediff == null) return;
            int level = Hediff_SpiritBeast.LevelOf(beast);
            foreach (var p in map.mapPawns.AllPawnsSpawned)
            {
                if (p == beast || !p.RaceProps.Humanlike || p.Dead || !p.HostileTo(beast)) continue;
                if (p.Position.DistanceToSquared(beast.Position) > Props.radius * Props.radius) continue;
                if (SpiritGradeUtility.RealmLevel(p) >= level) continue;
                var h = p.health.hediffSet.GetFirstHediffOfDef(Props.hediff);
                if (h == null) p.health.AddHediff(Props.hediff);
                else h.TryGetComp<HediffComp_Disappears>()?.ResetElapsedTicks();
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref announced, "announced");
        }
    }

    /// <summary>Al morir, el qi de la bestia se condensa: deja piedras espirituales según su rango (como los núcleos de maná del Isekai).</summary>
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
    public static class Patch_Pawn_Kill_SpiritStones
    {
        private static ThingDef SpiritStoneDef;

        public static void Postfix(Pawn __instance)
        {
            if (!__instance.Dead || !__instance.RaceProps.Animal) return;
            int level = Hediff_SpiritBeast.LevelOf(__instance);
            var list = SpiritBeastUtility.Ext?.deathStonesByLevel;
            var stone = SpiritStoneDef ??= DefDatabase<ThingDef>.GetNamedSilentFail("CVG_SpiritStone");
            if (level <= 0 || list == null || list.Count == 0 || stone == null) return;
            var map = __instance.MapHeld;
            if (map == null) return;
            int count = list[Mathf.Clamp(level - 1, 0, list.Count - 1)];
            if (count <= 0) return;
            var t = ThingMaker.MakeThing(stone);
            t.stackCount = count;
            GenPlace.TryPlaceThing(t, __instance.PositionHeld, map, ThingPlaceMode.Near);
        }
    }

    /// <summary>Se nota en el nombre: "alce espiritual", "trumbo soberano".</summary>
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.KindLabel), MethodType.Getter)]
    public static class Patch_Pawn_KindLabel_SpiritBeast
    {
        public static void Postfix(Pawn __instance, ref string __result)
        {
            if (Hediff_SpiritBeast.For(__instance) == null) return;
            var sov = SpiritBeastUtility.Ext?.sovereignHediff;
            bool sovereign = sov != null && __instance.health.hediffSet.HasHediff(sov);
            __result = (sovereign ? "CVG_SovereignKind" : "CVG_SpiritBeastKind").Translate(__result);
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
