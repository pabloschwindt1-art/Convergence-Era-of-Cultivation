using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Órganos de cultivador (docs/modulos/RAICES.md §7c): el corazón, los pulmones, los riñones y el hígado vanilla llevan este
    /// comp (parche XML). Si salen de un cultivador guardan su temple; al instalarlos dan el hediff de <see cref="hediff"/>.
    /// </summary>
    public class CompProperties_TemperedOrgan : CompProperties
    {
        public HediffDef hediff;

        public CompProperties_TemperedOrgan() => compClass = typeof(CompTemperedOrgan);
    }

    public class CompTemperedOrgan : ThingComp
    {
        /// <summary>1 a 6: el qi cuenta doble por reino; el cuerpo, un punto más (ese camino templa justo los órganos).</summary>
        public int tier;
        public CultivationRealmDef realm;
        public string donor;

        public CompProperties_TemperedOrgan Props => (CompProperties_TemperedOrgan)props;

        public static int TierOf(Pawn pawn, out CultivationRealmDef best)
        {
            best = null;
            var comp = CompCultivator.For(pawn);
            if (comp == null) return 0;
            int tier = 0;
            foreach (var t in comp.ActiveTrackers)
            {
                if (t.Realm == null) continue;
                int v = t.Path == CVG_DefOf.CVG_Path_Qi ? t.Realm.order * 2 : t.Realm.order + 1;
                if (v > tier) { tier = v; best = t.Realm; }
            }
            return Mathf.Clamp(tier, 0, 6);
        }

        public override string TransformLabel(string label) =>
            tier > 0 && realm != null ? label + " (" + realm.label + ")" : label;

        public override string CompInspectStringExtra() =>
            tier > 0 && !donor.NullOrEmpty() ? "CVG_TemperedOrgan_From".Translate(donor, realm?.LabelCap ?? "?") : null;

        public override bool AllowStackWith(Thing other) =>
            other.TryGetComp<CompTemperedOrgan>() is CompTemperedOrgan o && o.tier == tier && o.realm == realm && o.donor == donor;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref tier, "tier");
            Scribe_Defs.Look(ref realm, "realm");
            Scribe_Values.Look(ref donor, "donor");
        }
    }

    /// <summary>Un órgano templado vale más: ×(1 + 0,5 por nivel de temple).</summary>
    public class StatPart_TemperedOrgan : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.Thing?.TryGetComp<CompTemperedOrgan>() is CompTemperedOrgan c && c.tier > 0) val *= 1f + 0.5f * c.tier;
        }

        public override string ExplanationPart(StatRequest req) =>
            req.Thing?.TryGetComp<CompTemperedOrgan>() is CompTemperedOrgan c && c.tier > 0
                ? "CVG_TemperedOrgan_Value".Translate() + ": x" + (1f + 0.5f * c.tier).ToString("0.0") : null;
    }

    [HarmonyPatch(typeof(MedicalRecipesUtility), nameof(MedicalRecipesUtility.SpawnNaturalPartIfClean))]
    public static class Patch_SpawnNaturalPart_Tempered
    {
        public static void Postfix(Pawn pawn, Thing __result)
        {
            var comp = __result?.TryGetComp<CompTemperedOrgan>();
            if (comp == null) return;
            comp.tier = CompTemperedOrgan.TierOf(pawn, out comp.realm);
            comp.donor = comp.tier > 0 ? pawn.LabelShort : null;
        }
    }

    /// <summary>Al instalar un órgano natural templado, la parte recibe el hediff del temple (gravedad = nivel).</summary>
    [HarmonyPatch(typeof(Recipe_InstallNaturalBodyPart), nameof(Recipe_InstallNaturalBodyPart.ApplyOnPawn))]
    public static class Patch_InstallNaturalPart_Tempered
    {
        public static void Postfix(Pawn pawn, BodyPartRecord part, List<Thing> ingredients)
        {
            if (pawn == null || part == null || pawn.Dead || pawn.health.hediffSet.PartIsMissing(part)) return;
            var comp = ingredients?.Select(t => t.TryGetComp<CompTemperedOrgan>()).FirstOrDefault(c => c != null && c.tier > 0);
            if (comp?.Props.hediff == null) return;
            if (pawn.health.hediffSet.hediffs.Any(h => h.def == comp.Props.hediff && h.Part == part)) return;
            var hediff = HediffMaker.MakeHediff(comp.Props.hediff, pawn, part);
            hediff.Severity = comp.tier;
            pawn.health.AddHediff(hediff, part);
        }
    }
}
