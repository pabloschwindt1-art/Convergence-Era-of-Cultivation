using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Convergence
{
    // El caos primordial (Hundun): lo que la gente llama vacío en Anomaly (plan de fusión, 2026-10-23). Todo pasa por los
    // sistemas de la DLC: el estudio de entidades da también comprensión del Dao, las entidades dejan núcleos de caos al morir,
    // los reinos altos resisten lo psíquico (sensibilidad psíquica vanilla) y el qi le pega más al caos (BeastAndChaosDamage).

    public static class Chaos
    {
        private static ThingDef chaosCore, spiritStone, beastBlood;
        public static ThingDef ChaosCore => chaosCore ??= DefDatabase<ThingDef>.GetNamedSilentFail("CVG_ChaosCore");
        public static ThingDef SpiritStone => spiritStone ??= DefDatabase<ThingDef>.GetNamedSilentFail("CVG_SpiritStone");
        public static ThingDef BeastBlood => beastBlood ??= DefDatabase<ThingDef>.GetNamedSilentFail("CVG_BeastBlood");

        /// <summary>Rango de una entidad: como el de una bestia, por su poder de combate (1-6).</summary>
        public static int EntityRank(Pawn pawn) => pawn?.kindDef == null ? 1 : SpiritBeastUtility.SpeciesLevel(pawn.kindDef);

        public static void Place(ThingDef def, int count, Pawn near)
        {
            if (def == null || count <= 0 || near?.MapHeld == null) return;
            while (count > 0)
            {
                var t = ThingMaker.MakeThing(def);
                t.stackCount = Mathf.Min(count, def.stackLimit);
                count -= t.stackCount;
                GenPlace.TryPlaceThing(t, near.PositionHeld, near.MapHeld, ThingPlaceMode.Near);
            }
        }
    }

    /// <summary>
    /// Las entidades del vacío son ásperas, así que dejan buen botín (usuario): núcleos de caos según su rango, piedras
    /// espirituales tras la Convergencia (como las bestias) y sangre si son de carne. Solo las que no son de la colonia.
    /// </summary>
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
    public static class Patch_Pawn_Kill_ChaosDrops
    {
        public static void Postfix(Pawn __instance)
        {
            if (!__instance.Dead || !__instance.RaceProps.IsAnomalyEntity || __instance.Faction == Faction.OfPlayer || __instance.MapHeld == null) return;
            int rank = Chaos.EntityRank(__instance);
            int cores = rank >= 3 ? 1 + (rank - 3) / 2 : (Rand.Chance(0.3f * rank) ? 1 : 0);
            Chaos.Place(Chaos.ChaosCore, cores, __instance);
            if (GameComponent_Convergence.Active)
            {
                var list = SpiritBeastUtility.Ext?.deathStonesByLevel;
                if (!list.NullOrEmpty()) Chaos.Place(Chaos.SpiritStone, list[Mathf.Clamp(rank - 1, 0, list.Count - 1)], __instance);
            }
            if (__instance.RaceProps.IsFlesh) Chaos.Place(Chaos.BeastBlood, rank, __instance);
        }
    }

    /// <summary>
    /// Estudiar una entidad del vacío también da comprensión del Dao (el caos es lo que había antes del orden), a cambio de un
    /// pequeño riesgo de demonio interno en los cultivadores. Va por el estudio vanilla de Anomaly (CompStudiable.Study).
    /// </summary>
    [HarmonyPatch(typeof(CompStudiable), nameof(CompStudiable.Study))]
    public static class Patch_CompStudiable_Study_Dao
    {
        public const float DaoPerKnowledge = 0.5f;
        public const float InnerDemonChance = 0.01f;
        private static MentalStateDef innerDemon;

        public static void Postfix(CompStudiable __instance, Pawn studier, float anomalyKnowledgeAmount)
        {
            if (studier == null || anomalyKnowledgeAmount <= 0f) return;
            var thing = __instance.parent;
            bool entity = thing is Pawn p && p.RaceProps.IsAnomalyEntity
                          || thing is Building_HoldingPlatform platform && (platform.HeldPawn?.RaceProps.IsAnomalyEntity ?? false);
            if (!entity) return;
            DaoComprehension.Gain(studier, anomalyKnowledgeAmount * DaoPerKnowledge, DaoSource.Studying);
            if (SpiritGradeUtility.RealmLevel(studier) <= 0 || !Rand.Chance(InnerDemonChance)) return;
            innerDemon ??= DefDatabase<MentalStateDef>.GetNamedSilentFail("CVG_InnerDemon");
            if (innerDemon != null)
                studier.mindState?.mentalStateHandler?.TryStartMentalState(innerDemon, "CVG_Chaos_StudyDemon".Translate(), forceWake: true);
        }
    }

    /// <summary>Los reinos altos tienen la mente asentada: menos sensibilidad psíquica (el vacío les llega menos).</summary>
    public class StatPart_RealmPsychic : StatPart
    {
        public float perRealm = 0.15f;
        public float minFactor = 0.4f;

        private float Factor(StatRequest req)
        {
            if (!(req.Thing is Pawn pawn) || !pawn.RaceProps.Humanlike) return 1f;
            int realm = SpiritGradeUtility.RealmLevel(pawn);
            return realm <= 1 ? 1f : Mathf.Max(minFactor, 1f - perRealm * (realm - 1));
        }

        public override void TransformValue(StatRequest req, ref float val) => val *= Factor(req);

        public override string ExplanationPart(StatRequest req)
        {
            float f = Factor(req);
            return f >= 1f ? null : "CVG_Chaos_RealmPsychic".Translate() + ": x" + f.ToStringPercent();
        }
    }

    /// <summary>Resistencia al caos de un cultivador: un poco por cada reino.</summary>
    public class StatPart_RealmChaosResistance : StatPart
    {
        public float perRealm = 0.08f;

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.Thing is Pawn pawn && pawn.RaceProps.Humanlike) val += perRealm * SpiritGradeUtility.RealmLevel(pawn);
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!(req.Thing is Pawn pawn) || !pawn.RaceProps.Humanlike) return null;
            int realm = SpiritGradeUtility.RealmLevel(pawn);
            return realm <= 0 ? null : "CVG_Chaos_RealmResistance".Translate() + ": +" + (perRealm * realm).ToStringPercent();
        }
    }

    // ------------------------------------------------------------------ talismanes del caos (objetos de un uso)

    /// <summary>Talismán del pavor: los hostiles cercanos huyen aterrados (humanos) o quedan aturdidos (bestias y entidades).</summary>
    public class CompProperties_UseEffectChaosDread : CompProperties_UseEffect
    {
        public float radius = 7.9f;
        public int stunTicks = 300;

        public CompProperties_UseEffectChaosDread() => compClass = typeof(CompUseEffect_ChaosDread);
    }

    public class CompUseEffect_ChaosDread : CompUseEffect
    {
        public CompProperties_UseEffectChaosDread Props => (CompProperties_UseEffectChaosDread)props;

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            var map = usedBy.MapHeld;
            if (map == null) return;
            FleckMaker.Static(usedBy.Position, map, FleckDefOf.PsycastAreaEffect, Props.radius);
            foreach (var p in GenRadial.RadialDistinctThingsAround(usedBy.Position, map, Props.radius, true).OfType<Pawn>().ToList())
            {
                if (p.Dead || !p.HostileTo(usedBy)) continue;
                if (p.RaceProps.Humanlike && !p.InMentalState)
                    p.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.PanicFlee, "CVG_Chaos_DreadReason".Translate(), forceWake: true);
                else
                    p.stances?.stunner?.StunFor(Props.stunTicks, usedBy, addBattleLog: false);
            }
        }
    }

    /// <summary>Talismán del remiendo: cura las heridas de los aliados cercanos, a costa de su cordura (pensamiento malo).</summary>
    public class CompProperties_UseEffectChaosMending : CompProperties_UseEffect
    {
        public float radius = 4.9f;
        public float healAmount = 40f;
        public ThoughtDef thought;

        public CompProperties_UseEffectChaosMending() => compClass = typeof(CompUseEffect_ChaosMending);
    }

    public class CompUseEffect_ChaosMending : CompUseEffect
    {
        public CompProperties_UseEffectChaosMending Props => (CompProperties_UseEffectChaosMending)props;

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            var map = usedBy.MapHeld;
            if (map == null) return;
            FleckMaker.Static(usedBy.Position, map, FleckDefOf.PsycastAreaEffect, Props.radius);
            foreach (var p in GenRadial.RadialDistinctThingsAround(usedBy.Position, map, Props.radius, true).OfType<Pawn>().ToList())
            {
                if (p.Dead || p.HostileTo(usedBy) || !p.RaceProps.IsFlesh) continue;
                float left = Props.healAmount;
                foreach (var injury in p.health.hediffSet.hediffs.OfType<Hediff_Injury>().OrderByDescending(h => h.Severity).ToList())
                {
                    if (left <= 0f) break;
                    float heal = Mathf.Min(left, injury.Severity);
                    injury.Heal(heal);
                    left -= heal;
                }
                if (left < Props.healAmount && Props.thought != null) p.needs?.mood?.thoughts?.memories?.TryGainMemory(Props.thought);
                FleckMaker.ThrowMetaIcon(p.Position, map, FleckDefOf.HealingCross);
            }
        }
    }
}
