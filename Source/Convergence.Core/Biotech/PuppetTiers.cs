using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Convergence
{
    /// <summary>
    /// Escalón de una marioneta (docs/modulos/MARIONETAS_PROGRESION.md). En el ThingDef de la raza:
    /// - para fabricarla, el titiritero necesita al menos <see cref="minMasterRealm"/> (orden del reino) y <see cref="minMasterStage"/>;
    /// - si es de guerra (<see cref="combat"/>), pelea con la fuerza de la tabla del reino del titiritero (vida, daño, defensa,
    ///   velocidad) a <see cref="powerFraction"/>, con techo en <see cref="capRealm"/>/<see cref="capStage"/> y nunca por encima
    ///   del titiritero: dos marionetas vencen a un cultivador de su nivel, una sola no.
    /// </summary>
    public class DefModExtension_PuppetTier : DefModExtension
    {
        public int tier = 1;
        public bool combat;
        public int minMasterRealm;
        public int minMasterStage = 1;
        public int capRealm = 99;
        public int capStage = 4;
        /// <summary>Fracción del daño del reino (golpe y disparo). El usuario pidió no inflar el daño: la diferencia la hacen
        /// la vida, la defensa y la velocidad de ataque.</summary>
        public float powerFraction = 0.7f;
        /// <summary>Fracción de la vida y la defensa del reino.</summary>
        public float defenseFraction = 1f;
        /// <summary>Velocidad de ataque (golpe, apuntado y recarga): fracción del bono de daño del reino convertida en cadencia.</summary>
        public float speedFraction = 0.6f;
        /// <summary>Debilidad del material (madera: fuego; hierro: frío...). Se pone al aparecer.</summary>
        public HediffDef materialHediff;
    }

    /// <summary>Fuerza calculada de una marioneta de guerra (se recalcula con el vínculo, cada 1000 ticks).</summary>
    public struct PuppetPower
    {
        public float health, melee, incoming, move, ranged, speed;
        public int realm, stage;
        public static readonly PuppetPower None = new PuppetPower { health = 1f, melee = 1f, incoming = 1f, move = 1f, ranged = 1f, speed = 1f };
    }

    public static class PuppetTierUtility
    {
        /// <summary>Reino (orden) y etapa más altos del titiritero, en cualquiera de sus caminos.</summary>
        public static void MasterLevel(Pawn master, out int realm, out int stage)
        {
            realm = 0; stage = 0;
            var comp = master == null ? null : CompCultivator.For(master);
            if (comp == null) return;
            foreach (var t in comp.ActiveTrackers)
            {
                if (!t.Started) continue;
                int o = t.Realm.order;
                if (o > realm || (o == realm && t.Stage > stage)) { realm = o; stage = t.Stage; }
            }
        }

        public static bool MeetsRequirement(Pawn master, DefModExtension_PuppetTier ext)
        {
            if (ext == null || ext.minMasterRealm <= 0) return true;
            MasterLevel(master, out int r, out int s);
            return r > ext.minMasterRealm || (r == ext.minMasterRealm && s >= ext.minMasterStage);
        }

        public static string RealmName(int order, int stage)
        {
            var def = DefDatabase<CultivationRealmDef>.AllDefsListForReading.Where(d => d.path == CVG_DefOf.CVG_Path_Qi && d.order == order).FirstOrDefault();
            return (def?.LabelCap ?? (string)("#" + order)) + " " + "CVG_Puppet_StageN".Translate(stage);
        }

        /// <summary>
        /// Fuerza de una marioneta de guerra: el reino y etapa efectivos son los del titiritero con techo en el escalón; las
        /// cifras salen de la tabla del reino (hediff de su etapa y vida del CultivationRealmDef), a una fracción.
        /// </summary>
        /// <summary>Reino con el que pelea una marioneta de guerra (para la supresión de reino); 0 para todo lo demás.</summary>
        public static int PowerRealm(Pawn pawn)
        {
            if (pawn == null || !pawn.RaceProps.IsMechanoid) return 0;
            var comp = pawn.GetComp<CompPuppet>();
            return comp != null && comp.IsCombat ? comp.Power.realm : 0;
        }

        /// <summary>Multiplicador de los disparos de una marioneta de guerra (1 para todo lo demás).</summary>
        public static float RangedFactor(Thing launcher)
        {
            if (!(launcher is Pawn pawn) || !pawn.RaceProps.IsMechanoid) return 1f;
            var comp = pawn.GetComp<CompPuppet>();
            return comp != null && comp.IsCombat && comp.Power.ranged > 0f ? comp.Power.ranged : 1f;
        }

        public static PuppetPower Compute(Pawn master, DefModExtension_PuppetTier ext)
        {
            if (ext == null || !ext.combat) return PuppetPower.None;
            MasterLevel(master, out int r, out int s);
            if (r <= 0) return PuppetPower.None;
            if (r > ext.capRealm || (r == ext.capRealm && s > ext.capStage)) { r = ext.capRealm; s = ext.capStage; }
            var realms = DefDatabase<CultivationRealmDef>.AllDefsListForReading.Where(d => d.path == CVG_DefOf.CVG_Path_Qi).OrderBy(d => d.order).ToList();
            var def = realms.LastOrDefault(d => d.order <= r);
            if (def == null) return PuppetPower.None;
            if (def.order < r) s = def.stages;   // reino aún no implementado: el más alto que existe, en su cima
            s = Mathf.Clamp(s, 1, def.stages);
            float f = ext.powerFraction;
            var p = new PuppetPower { realm = def.order, stage = s };
            float hs = Mathf.Lerp(def.healthScaleFactor.min, def.healthScaleFactor.max, def.stages <= 1 ? 1f : (s - 1f) / (def.stages - 1f));
            float d = ext.defenseFraction;
            p.health = 1f + (hs - 1f) * d;
            var stageDef = def.hediff?.stages?.ElementAtOrDefault(s - 1);
            float Factor(StatDef st) => stageDef?.statFactors?.FirstOrDefault(m => m.stat == st)?.value ?? 1f;
            p.melee = 1f + (Factor(StatDefOf.MeleeDamageFactor) - 1f) * f;
            p.incoming = 1f - (1f - Factor(StatDefOf.IncomingDamageFactor)) * d;
            p.move = 1f + (Factor(StatDefOf.MoveSpeed) - 1f) * f;
            p.speed = 1f + (Factor(StatDefOf.MeleeDamageFactor) - 1f) * ext.speedFraction;
            p.ranged = p.melee;   // cada disparo pega más fuerte y perfora más, como el golpe (ver Patch_Projectile_PuppetDamage)
            return p;
        }
    }

    /// <summary>Aplica la fuerza de <see cref="PuppetPower"/> a las marionetas de guerra (daño, defensa, velocidad, cadencia).</summary>
    [StaticConstructorOnStartup]
    public class StatPart_PuppetPower : StatPart
    {
        static StatPart_PuppetPower()
        {
            foreach (var s in new[] { StatDefOf.MeleeDamageFactor, StatDefOf.IncomingDamageFactor, StatDefOf.MoveSpeed,
                StatDefOf.MeleeCooldownFactor, StatDefOf.RangedCooldownFactor, StatDefOf.AimingDelayFactor })
            {
                if (s == null) continue;
                s.parts ??= new List<StatPart>();
                if (!s.parts.Any(p => p is StatPart_PuppetPower)) s.parts.Add(new StatPart_PuppetPower { parentStat = s });
            }
        }

        private float Factor(Pawn pawn)
        {
            var comp = pawn.GetComp<CompPuppet>();
            if (comp == null || !comp.IsCombat) return 1f;
            var p = comp.Power;
            if (parentStat == StatDefOf.MeleeDamageFactor) return p.melee;
            if (parentStat == StatDefOf.IncomingDamageFactor) return p.incoming;
            if (parentStat == StatDefOf.MoveSpeed) return p.move;
            if (parentStat == StatDefOf.MeleeCooldownFactor || parentStat == StatDefOf.RangedCooldownFactor || parentStat == StatDefOf.AimingDelayFactor)
                return p.speed > 0f ? 1f / p.speed : 1f;
            return 1f;
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.Thing is Pawn pawn && pawn.RaceProps.IsMechanoid) val *= Factor(pawn);
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!(req.Thing is Pawn pawn) || !pawn.RaceProps.IsMechanoid) return null;
            var comp = pawn.GetComp<CompPuppet>();
            if (comp == null || !comp.IsCombat || comp.Power.realm <= 0) return null;
            return "CVG_Puppet_PowerPart".Translate(PuppetTierUtility.RealmName(comp.Power.realm, comp.Power.stage)) + ": x" + Factor(pawn).ToStringPercent();
        }
    }

    /// <summary>
    /// Las marionetas de distancia aprovechan el reino como las de cuerpo a cuerpo: el daño y la penetración de cada disparo se
    /// multiplican (disparar más seguido no servía: la defensa de un cultivador se come casi entero un golpe chico).
    /// </summary>
    [HarmonyPatch(typeof(Projectile), nameof(Projectile.DamageAmount), MethodType.Getter)]
    public static class Patch_Projectile_PuppetDamage
    {
        public static void Postfix(Projectile __instance, ref int __result)
        {
            float f = PuppetTierUtility.RangedFactor(__instance.Launcher);
            if (f != 1f) __result = Mathf.Max(1, GenMath.RoundRandom(__result * f));
        }
    }

    [HarmonyPatch(typeof(Projectile), nameof(Projectile.ArmorPenetration), MethodType.Getter)]
    public static class Patch_Projectile_PuppetPenetration
    {
        public static void Postfix(Projectile __instance, ref float __result) => __result *= PuppetTierUtility.RangedFactor(__instance.Launcher);
    }

    /// <summary>Para fabricar una marioneta, el titiritero tiene que haber llegado al reino de su escalón (además de la investigación).</summary>
    [HarmonyPatch(typeof(Bill_Mech), nameof(Bill_Mech.PawnAllowedToStartAnew))]
    public static class Patch_Bill_Mech_PuppetRealm
    {
        public static void Postfix(Bill_Mech __instance, Pawn p, ref bool __result)
        {
            if (!__result) return;
            var ext = __instance.recipe?.ProducedThingDef?.GetModExtension<DefModExtension_PuppetTier>();
            if (ext == null || PuppetTierUtility.MeetsRequirement(p, ext)) return;
            __result = false;
            JobFailReason.Is("CVG_Puppet_NeedRealm".Translate(PuppetTierUtility.RealmName(ext.minMasterRealm, ext.minMasterStage)));
        }
    }

    /// <summary>
    /// Debilidad del material que se activa con el frío (hierro, acero): la severidad del hediff sigue la temperatura del
    /// lugar (0 templado, 1 bajo cero); las etapas del XML dicen cuánto frena.
    /// </summary>
    public class HediffCompProperties_ColdBrittle : HediffCompProperties
    {
        public float belowCelsius = 0f;
        public HediffCompProperties_ColdBrittle() { compClass = typeof(HediffComp_ColdBrittle); }
    }

    public class HediffComp_ColdBrittle : HediffComp
    {
        public HediffCompProperties_ColdBrittle Props => (HediffCompProperties_ColdBrittle)props;

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            if (!Pawn.IsHashIntervalTick(250, delta) || !Pawn.Spawned) return;
            float t = Pawn.AmbientTemperature;
            parent.Severity = t < Props.belowCelsius ? 1f : 0.1f;
        }
    }
}
