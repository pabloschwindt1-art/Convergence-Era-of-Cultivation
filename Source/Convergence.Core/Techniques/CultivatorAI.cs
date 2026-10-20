using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Convergence
{
    // IA de los cultivadores NPC (docs/DISENO.md §8, ronda 3) con las herramientas del juego: las técnicas son
    // habilidades vanilla (aiCanUse, ai_IsOffensive) que JobGiver_AIFightEnemy ya lanza; aquí se suman condiciones de uso
    // por distancia, salud y número de enemigos, y las defensivas (escudo, coraza de sangre) se lanzan antes de atacar.

    /// <summary>
    /// Cuándo la IA puede usar la técnica (no afecta al jugador). Distancias al objetivo, salud del lanzador, enemigos
    /// cerca del objetivo (técnicas de área) o del lanzador (escudos) y que no tenga ya el hediff que da.
    /// </summary>
    public class CompProperties_AbilityAIConditions : CompProperties_AbilityEffect
    {
        public float minDistance = -1f;
        public float maxDistance = -1f;
        /// <summary>Solo si la salud del lanzador es menor que esto (escudos, curas). 1 = siempre.</summary>
        public float maxCasterHealth = 1f;
        /// <summary>Enemigos (hostiles, en pie) a menos de <see cref="enemyRadius"/> del objetivo (o del lanzador si apunta a sí mismo).</summary>
        public int minEnemies;
        public float enemyRadius = 4f;
        /// <summary>Con la salud por debajo de esto ignora <see cref="minEnemies"/> (desesperación).</summary>
        public float desperateHealth = 0.4f;
        /// <summary>No la usa si ya tiene este hediff (escudo activo).</summary>
        public HediffDef skipIfHediff;

        public CompProperties_AbilityAIConditions()
        {
            compClass = typeof(CompAbilityEffect_AIConditions);
        }
    }

    public class CompAbilityEffect_AIConditions : CompAbilityEffect
    {
        public new CompProperties_AbilityAIConditions Props => (CompProperties_AbilityAIConditions)props;

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            var caster = parent.pawn;
            if (caster?.Map == null) return false;
            float health = caster.health.summaryHealth.SummaryHealthPercent;
            if (health > Props.maxCasterHealth) return false;
            if (Props.skipIfHediff != null && caster.health.hediffSet.HasHediff(Props.skipIfHediff)) return false;
            var center = target.IsValid && target.Thing != caster ? target.Cell : caster.Position;
            if (target.IsValid && target.Thing != caster)
            {
                float dist = caster.Position.DistanceTo(target.Cell);
                if (Props.minDistance >= 0f && dist < Props.minDistance) return false;
                if (Props.maxDistance >= 0f && dist > Props.maxDistance) return false;
            }
            if (Props.minEnemies > 0 && health > Props.desperateHealth && CultivatorAI.EnemiesNear(caster, center, Props.enemyRadius) < Props.minEnemies)
                return false;
            return true;
        }
    }

    public static class CultivatorAI
    {
        private static readonly List<Pawn> tmp = new List<Pawn>();

        /// <summary>Pawns hostiles al lanzador, en pie, a menos de <paramref name="radius"/> de <paramref name="center"/>.</summary>
        public static int EnemiesNear(Pawn caster, IntVec3 center, float radius)
        {
            int n = 0;
            tmp.Clear();
            tmp.AddRange(caster.Map.mapPawns.AllPawnsSpawned);
            for (int i = 0; i < tmp.Count; i++)
            {
                var p = tmp[i];
                if (p == caster || p.Dead || p.Downed || !p.Position.InHorDistOf(center, radius)) continue;
                if (p.HostileTo(caster)) n++;
            }
            tmp.Clear();
            return n;
        }

        /// <summary>Hay enemigos activos cerca: nada de avances ni de sesiones de cultivo largas.</summary>
        public static bool InDanger(Pawn pawn, float radius = 30f) =>
            pawn.Spawned && (pawn.Drafted || GenAI.EnemyIsNear(pawn, radius) || (pawn.mindState?.enemyTarget != null));

        /// <summary>Primera técnica defensiva sobre sí mismo (escudo, coraza) que la IA puede lanzar ahora.</summary>
        public static Job DefensiveJob(Pawn pawn)
        {
            if (pawn.abilities == null) return null;
            var list = pawn.abilities.AllAbilitiesForReading;
            for (int i = 0; i < list.Count; i++)
            {
                var a = list[i];
                if (a.def.ai_IsOffensive || !a.def.aiCanUse || !a.verb.targetParams.canTargetSelf) continue;
                if (!TechniqueUtility.IsTechnique(a.def) && a.CompOfType<CompAbilityEffect_AIConditions>() == null) continue;
                if (a.AICanTargetNow(pawn)) return a.GetJob(pawn, pawn);
            }
            return null;
        }
    }

    /// <summary>Los cultivadores NPC se cubren (escudo de qi, coraza de sangre) antes de atacar si sus condiciones lo piden.</summary>
    [HarmonyPatch(typeof(JobGiver_AIFightEnemy), nameof(JobGiver_AIFightEnemy.GetAbilityJob))]
    public static class Patch_JobGiver_AIFightEnemy_GetAbilityJob
    {
        public static bool Prefix(Pawn pawn, ref Job __result)
        {
            if (pawn?.abilities == null || pawn.IsColonist || CompCultivator.For(pawn) == null) return true;
            var job = CultivatorAI.DefensiveJob(pawn);
            if (job == null) return true;
            __result = job;
            return false;
        }
    }
}
