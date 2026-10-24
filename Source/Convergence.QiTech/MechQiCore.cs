using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence.QiTech
{
    // Mecas de qi (plan de fusión, 2026-10-23). Un núcleo de qi (básico, medio, avanzado: uno por peso de meca) se implanta en
    // un meca propio como la lanza psíquica vanilla: con el mecanitor elegido, clic derecho en el núcleo y clic en el meca.
    // El meca queda con la condición CVG_MechQiCore, cuya severidad es su grado espiritual (Core: DefModExtension_GradeHediff,
    // que entra en la supresión de reino). Nace en grado 1; la estación de afinación, junto a su recargador, lo sube hasta su
    // nivel mientras carga. Tras la Convergencia, los mecas enemigos también pueden traer núcleo (QiMechSpawns).

    /// <summary>Núcleo de qi: qué pesos de meca acepta.</summary>
    public class CompProperties_TargetableMechQiCore : CompProperties_Targetable
    {
        public List<MechWeightClassDef> weightClasses = new List<MechWeightClassDef>();

        public CompProperties_TargetableMechQiCore() => compClass = typeof(CompTargetable_MechQiCore);
    }

    public class CompTargetable_MechQiCore : CompTargetable
    {
        public new CompProperties_TargetableMechQiCore Props => (CompProperties_TargetableMechQiCore)props;

        protected override bool PlayerChoosesTarget => true;

        protected override TargetingParameters GetTargetingParameters() => new TargetingParameters
        {
            canTargetPawns = true,
            canTargetBuildings = false,
            canTargetItems = false,
            validator = t => t.Thing is Pawn p && Accepts(p)
        };

        public bool Accepts(Pawn mech) =>
            mech.RaceProps.IsMechanoid && mech.Faction == Faction.OfPlayer && !mech.Dead
            && Props.weightClasses.Contains(mech.RaceProps.mechWeightClass)
            && !mech.health.hediffSet.HasHediff(MechQiCore.HediffDef);

        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            if (!(target.Thing is Pawn p) || !Accepts(p))
            {
                if (showMessages && target.Thing is Pawn other && other.RaceProps.IsMechanoid)
                    Messages.Message("CVG_MechQiCore_WrongMech".Translate(parent.LabelNoCount, other.LabelShort), MessageTypeDefOf.RejectInput, false);
                return false;
            }
            return base.ValidateTarget(target, showMessages);
        }

        public override IEnumerable<Thing> GetTargets(Thing targetChosenByPlayer = null)
        {
            yield return targetChosenByPlayer;
        }

        /// <summary>Lo implanta un mecanitor (es su meca y su banda).</summary>
        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            if (!MechanitorUtility.IsMechanitor(p)) return "CVG_MechQiCore_NeedsMechanitor".Translate();
            return base.CanBeUsedBy(p);
        }
    }

    public class CompProperties_TargetEffectMechQiCore : CompProperties
    {
        public int grade = 1;

        public CompProperties_TargetEffectMechQiCore() => compClass = typeof(CompTargetEffect_MechQiCore);
    }

    public class CompTargetEffect_MechQiCore : CompTargetEffect
    {
        public override void DoEffectOn(Pawn user, Thing target)
        {
            if (!(target is Pawn mech)) return;
            MechQiCore.Install(mech, ((CompProperties_TargetEffectMechQiCore)props).grade);
            Messages.Message("CVG_MechQiCore_Installed".Translate(mech.LabelShort, user.LabelShort), mech, MessageTypeDefOf.PositiveEvent);
        }
    }

    [StaticConstructorOnStartup]
    public static class MechQiCore
    {
        public static readonly HediffDef HediffDef = DefDatabase<HediffDef>.GetNamedSilentFail("CVG_MechQiCore");

        public static Hediff CoreOf(Pawn mech) => HediffDef == null ? null : mech?.health?.hediffSet?.GetFirstHediffOfDef(HediffDef);

        public static void Install(Pawn mech, int grade)
        {
            if (HediffDef == null || mech == null) return;
            var h = CoreOf(mech) ?? mech.health.AddHediff(HediffDef);
            h.Severity = Mathf.Max(h.Severity, grade);
            if (mech.Spawned) FleckMaker.ThrowLightningGlow(mech.DrawPos, mech.Map, 1.5f);
        }
    }

    /// <summary>
    /// Estación de afinación (instalación del recargador de mecas): cada meca con núcleo de qi que carga en un recargador
    /// vinculado sube su grado hasta el nivel de la estación (nivel 1-3, se sube con piedras en el sitio:
    /// <see cref="CompWorkbenchLevel"/>). Necesita energía.
    /// </summary>
    public class CompProperties_MechTuning : CompProperties
    {
        public CompProperties_MechTuning() => compClass = typeof(CompMechTuning);
    }

    public class CompMechTuning : ThingComp
    {
        public int Level => parent.GetComp<CompWorkbenchLevel>()?.Level ?? 1;

        private bool Powered => parent.GetComp<CompPowerTrader>()?.PowerOn ?? true;

        public override void CompTickRare()
        {
            base.CompTickRare();
            if (!Powered) return;
            var facility = parent.GetComp<CompFacility>();
            if (facility == null) return;
            foreach (var building in facility.LinkedBuildings)
            {
                if (!(building is Building_MechCharger charger)) continue;
                var mech = charger.CurrentlyChargingMech;
                var core = MechQiCore.CoreOf(mech);
                if (core == null || core.Severity >= Level) continue;
                core.Severity = Level;
                FleckMaker.ThrowLightningGlow(mech.DrawPos, mech.Map, 1.2f);
                Messages.Message("CVG_MechQiCore_Tuned".Translate(mech.LabelShort, Level), mech, MessageTypeDefOf.PositiveEvent);
            }
        }

        public override string CompInspectStringExtra() => "CVG_MechTuning_Inspect".Translate(Level);
    }

    /// <summary>El aura del meca de qi escala con su tamaño (un mecanoide pesado tiene un aura más grande).</summary>
    public class PawnRenderNodeWorker_MechQiAura : PawnRenderNodeWorker
    {
        public override Vector3 ScaleFor(PawnRenderNode node, PawnDrawParms parms) =>
            base.ScaleFor(node, parms) * Mathf.Max(0.8f, Mathf.Sqrt(parms.pawn.BodySize) * 1.3f);
    }

    /// <summary>"Mecanoide de qi": el nombre del tipo dice que tiene núcleo (como "bestia espiritual" en el Core).</summary>
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.KindLabel), MethodType.Getter)]
    public static class Patch_Pawn_KindLabel_QiMech
    {
        public static void Postfix(Pawn __instance, ref string __result)
        {
            if (__instance.RaceProps.IsMechanoid && MechQiCore.CoreOf(__instance) != null)
                __result = "CVG_MechQiCore_KindLabel".Translate(__result);
        }
    }

    /// <summary>
    /// Mecas enemigos de qi (usuario: "los mecas siguen siendo un problema para los cultivadores"): tras la Convergencia, cada
    /// meca no aliado que se genera puede traer un núcleo, con una chance que crece con los días desde la Convergencia. El
    /// grado depende de su peso.
    /// </summary>
    [HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), typeof(PawnGenerationRequest))]
    public static class QiMechSpawns
    {
        public static float ChanceAt(float daysSinceConvergence) => Mathf.Clamp(0.15f + daysSinceConvergence * 0.006f, 0f, 0.6f);

        public static int GradeFor(MechWeightClassDef weight)
        {
            if (weight == MechWeightClassDefOf.Light) return 1;
            if (weight == MechWeightClassDefOf.Medium) return Rand.Chance(0.5f) ? 2 : 1;
            if (weight == MechWeightClassDefOf.Heavy) return 2;
            return 3;
        }

        public static void Postfix(Pawn __result)
        {
            if (__result == null || !__result.RaceProps.IsMechanoid || MechQiCore.HediffDef == null) return;
            if (__result.Faction == null || __result.Faction == Faction.OfPlayer || !__result.Faction.HostileTo(Faction.OfPlayer)) return;
            var convergence = GameComponent_Convergence.Instance;
            if (convergence == null || !convergence.Occurred) return;
            float days = (Find.TickManager.TicksGame - convergence.OccurredTick) / (float)GenDate.TicksPerDay;
            if (!Rand.Chance(ChanceAt(days))) return;
            MechQiCore.Install(__result, GradeFor(__result.RaceProps.mechWeightClass));
        }
    }
}
