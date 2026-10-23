using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>Factor de potencia de una píldora (o cualquier cosa) según su calidad. Sin calidad = 1.</summary>
    public static class PillQuality
    {
        private static readonly float[] Factors = { 0.5f, 0.75f, 1f, 1.2f, 1.45f, 1.75f, 2.2f };

        public static float FactorOf(Thing thing) =>
            thing != null && thing.TryGetQuality(out var q) ? Factors[Mathf.Clamp((int)q, 0, Factors.Length - 1)] : 1f;

        /// <summary>Tabla de un efecto por calidad, para la ficha: "Horrible: +150 / ... / Legendaria: +660".</summary>
        public static string Table(float amount) =>
            string.Join("\n", Enumerable.Range(0, Factors.Length).Select(i => ((QualityCategory)i).GetLabel().CapitalizeFirst() + ": +" + (amount * Factors[i]).ToString("0")));
    }

    /// <summary>Como <c>IngestionOutcomeDoer_GiveHediff</c>, pero la severidad se multiplica por la calidad de la píldora.</summary>
    public class IngestionOutcomeDoer_GiveHediffByQuality : IngestionOutcomeDoer
    {
        public HediffDef hediffDef;
        public float severity = 1f;

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
        {
            if (hediffDef == null) return;
            float sev = severity * PillQuality.FactorOf(ingested);
            var h = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
            if (h != null) h.Severity = Mathf.Max(h.Severity, sev);
            else
            {
                h = HediffMaker.MakeHediff(hediffDef, pawn);
                h.Severity = sev;
                pawn.health.AddHediff(h);
            }
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef parentDef)
        {
            if (hediffDef != null)
                yield return new StatDrawEntry(StatCategoryDefOf.Drug, "CVG_Pill_Effect".Translate(), hediffDef.LabelCap,
                    "CVG_Pill_QualityScales".Translate(), 2000);
        }
    }

    /// <summary>Recurso de cultivo (qi, sangre-qi) escalado por la calidad de la píldora.</summary>
    public class IngestionOutcomeDoer_GainResourceByQuality : IngestionOutcomeDoer_GainCultivationResource
    {
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
        {
            float baseAmount = amount;
            amount = baseAmount * PillQuality.FactorOf(ingested);
            try
            {
                base.DoIngestionOutcomeSpecial(pawn, ingested, ingestedCount);
            }
            finally
            {
                amount = baseAmount;
            }
        }
    }

    /// <summary>
    /// Píldora de purificación de sangre: sube un nivel de pureza del linaje (purificación del Core) con una probabilidad
    /// que crece con la calidad. Si falla, o si no hay linaje, deja <see cref="failHediff"/>.
    /// </summary>
    public class IngestionOutcomeDoer_PurifyBloodline : IngestionOutcomeDoer
    {
        public float baseChance = 0.6f;
        public HediffDef failHediff;
        public float failSeverity = 0.3f;

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
        {
            var info = BloodlineUtility.Get(pawn);
            float chance = Mathf.Clamp01(baseChance * PillQuality.FactorOf(ingested) * pawn.GetStatValue(CVG_DefOf.CVG_PurificationChance));
            if (info != null && info.CanPurify && Rand.Chance(chance) && BloodlineUtility.TryPurify(pawn)) return;
            if (failHediff != null) HealthUtility.AdjustSeverity(pawn, failHediff, failSeverity);
            if (pawn.Faction == Faction.OfPlayer)
                Messages.Message((info == null ? "CVG_Pill_NoBloodline" : "CVG_Pill_PurifyFailed").Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.NegativeEvent);
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef parentDef)
        {
            yield return new StatDrawEntry(StatCategoryDefOf.Drug, "CVG_Pill_PurifyChance".Translate(), baseChance.ToStringPercent(),
                "CVG_Pill_QualityScales".Translate(), 2000);
        }
    }

    /// <summary>
    /// Grabar runas: la receta toma un arma de grado 0 y no la destruye, le suma un grado espiritual
    /// (<see cref="CompSpiritGrade.bonus"/>). Los demás ingredientes (piedras) se consumen normal.
    /// </summary>
    public class RecipeWorker_EngraveRunes : RecipeWorker
    {
        public override void ConsumeIngredient(Thing ingredient, RecipeDef recipe, Map map)
        {
            if (ingredient.def.IsWeapon && ingredient is ThingWithComps twc && twc.GetComp<CompSpiritGrade>() is CompSpiritGrade comp)
            {
                if (comp.Grade < 1) comp.bonus += 1 - comp.Grade;
                if (map != null) FleckMaker.ThrowLightningGlow(ingredient.DrawPos, map, 1.5f);
                return;
            }
            base.ConsumeIngredient(ingredient, recipe, map);
        }
    }

    /// <summary>Armas que grabar runas no puede mejorar (ya tienen grado espiritual 1+ o no llevan grado): la receta las excluye.</summary>
    public class SpecialThingFilterWorker_SpiritGraded : SpecialThingFilterWorker
    {
        public override bool Matches(Thing t) => !(t is ThingWithComps twc && twc.GetComp<CompSpiritGrade>() is CompSpiritGrade c && c.Grade < 1);
        public override bool CanEverMatch(ThingDef def) => def.IsWeapon;
    }

    /// <summary>Talismán de ataque: al usarlo, daña a los hostiles en un radio alrededor del usuario.</summary>
    public class CompProperties_UseEffectSpiritBurst : CompProperties_UseEffect
    {
        public DamageDef damageDef;
        public int amount = 20;
        public float radius = 4.9f;

        public CompProperties_UseEffectSpiritBurst()
        {
            compClass = typeof(CompUseEffect_SpiritBurst);
        }
    }

    public class CompUseEffect_SpiritBurst : CompUseEffect
    {
        public CompProperties_UseEffectSpiritBurst Props => (CompProperties_UseEffectSpiritBurst)props;

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            var map = usedBy.MapHeld;
            if (map == null) return;
            FleckMaker.Static(usedBy.Position, map, FleckDefOf.PsycastAreaEffect, Props.radius);
            foreach (var thing in GenRadial.RadialDistinctThingsAround(usedBy.Position, map, Props.radius, true).ToList())
            {
                if (!(thing is Pawn p) || p.Dead || !p.HostileTo(usedBy)) continue;
                FleckMaker.ThrowLightningGlow(p.DrawPos, map, 1.5f);
                p.TakeDamage(new DamageInfo(Props.damageDef ?? DamageDefOf.Burn, Props.amount, 0.5f, -1f, usedBy));
            }
        }
    }

    /// <summary>Todas las armas pueden recibir runas: se les añade <see cref="CompSpiritGrade"/> (grado 0) si no lo tienen.</summary>
    [StaticConstructorOnStartup]
    public static class SpiritGradeInjection
    {
        static SpiritGradeInjection()
        {
            foreach (var def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (!def.IsWeapon || def.IsStuff || !typeof(ThingWithComps).IsAssignableFrom(def.thingClass)) continue;
                def.comps ??= new List<CompProperties>();
                if (!def.comps.Any(c => c is CompProperties_SpiritGrade)) def.comps.Add(new CompProperties_SpiritGrade());
            }
        }
    }
}
