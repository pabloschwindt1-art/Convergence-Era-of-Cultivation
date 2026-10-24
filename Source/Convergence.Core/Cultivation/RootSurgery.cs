using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Cama donde se puede extirpar e implantar una raíz (docs/modulos/RAICES.md §3 y §7a). El altar del Dao (Grafts) y la mesa de
    /// cirugía de qi (Qi-Tech) la llevan en su XML: las recetas viven en el Core y ningún módulo depende de otro.
    /// </summary>
    public class DefModExtension_RootSurgeryBed : DefModExtension
    {
        /// <summary>Investigación que habilita las cirugías de raíz en esta cama.</summary>
        public ResearchProjectDef research;
        public bool needsPower;
        /// <summary>Multiplica la chance de sacar la raíz entera.</summary>
        public float extractionFactor = 1f;
        /// <summary>Edificio que, enlazado a la cama, suma <see cref="facilityBonus"/> (el resonador junto a la mesa de qi).</summary>
        public ThingDef bonusFacility;
        public float facilityBonus;
        /// <summary>Hediff de "raíz ajena" que deja un trasplante en esta cama, y otro permanente (la costura de qi de la máquina).</summary>
        public HediffDef foreignRootHediff;
        public HediffDef permanentHediff;
    }

    public static class RootSurgeryUtility
    {
        public static DefModExtension_RootSurgeryBed Ext(Thing bed) => bed?.def.GetModExtension<DefModExtension_RootSurgeryBed>();

        public static bool IsUsable(Building_Bed bed)
        {
            var ext = Ext(bed);
            if (ext == null) return false;
            if (ext.research != null && !ext.research.IsFinished) return false;
            if (ext.needsPower && !(bed.TryGetComp<CompPowerTrader>()?.PowerOn ?? false)) return false;
            return true;
        }

        public static IEnumerable<Building_Bed> UsableBeds(Map map) =>
            map?.listerBuildings.allBuildingsColonist.OfType<Building_Bed>().Where(IsUsable) ?? Enumerable.Empty<Building_Bed>();

        public static AcceptanceReport BedReport(Thing patient)
        {
            if (UsableBeds(patient?.MapHeld).Any()) return true;
            return "CVG_NeedsRootSurgeryBed".Translate();
        }

        public static float FacilityBonus(Building_Bed bed)
        {
            var ext = Ext(bed);
            if (ext?.bonusFacility == null) return 0f;
            var facilities = bed.TryGetComp<CompAffectedByFacilities>()?.LinkedFacilitiesListForReading;
            return facilities != null && facilities.Any(f => f.def == ext.bonusFacility) ? ext.facilityBonus : 0f;
        }

        public static bool IsRootSurgery(RecipeDef r) => r?.Worker is Recipe_ExtractSpiritRoot || r?.Worker is Recipe_ImplantSpiritRoot;
    }

    /// <summary>Extirpar la raíz: sale una raíz sellada (o una peor, o nada); el donante queda sin raíz y marcado.</summary>
    public class Recipe_ExtractSpiritRoot : Recipe_Surgery
    {
        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null) =>
            base.AvailableOnNow(thing, part) && thing is Pawn p && (CompCultivator.For(p)?.HasRoot ?? false) && !SpiritRootUtility.IsArtificial(p);

        public override AcceptanceReport AvailableReport(Thing thing, BodyPartRecord part = null)
        {
            if (!AvailableOnNow(thing, part)) return false;
            return RootSurgeryUtility.BedReport(thing);
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (billDoer == null) return;
            var comp = CompCultivator.For(pawn);
            if (comp?.Root == null) return;
            bool violation = IsViolationOnPawn(pawn, part, Faction.OfPlayer);
            var bed = pawn.CurrentBed();
            var ext = RootSurgeryUtility.Ext(bed);
            var qiRealm = comp.TrackerFor(CVG_DefOf.CVG_Path_Qi)?.Realm;

            bool failed = CheckSurgeryFail(billDoer, pawn, ingredients, part, bill);
            var root = SpiritRootUtility.RemoveRoot(pawn);
            pawn.health.AddHediff(CVG_RootDefOf.CVG_RootTorn);

            Thing sealedRoot = null;
            if (!failed && root != null)
            {
                float chance = root.grade.extractionFactor * (ext?.extractionFactor ?? 1f) + RootSurgeryUtility.FacilityBonus(bed);
                if (Rand.Chance(Mathf.Clamp01(chance)))
                    sealedRoot = SealedRootUtility.Make(root.grade, root.elements, pawn.LabelShort);
                else if (Rand.Bool && SpiritRootUtility.Previous(root.grade) is SpiritRootGradeDef lower)
                    sealedRoot = SealedRootUtility.Make(lower, root.elements, pawn.LabelShort);
            }

            if (sealedRoot != null)
            {
                GenPlace.TryPlaceThing(sealedRoot, billDoer.Position, billDoer.Map, ThingPlaceMode.Near);
                Messages.Message("CVG_RootExtract_Success".Translate(billDoer.Named("DOCTOR"), pawn.Named("PAWN"), sealedRoot.LabelNoCount),
                    sealedRoot, MessageTypeDefOf.PositiveEvent);
            }
            else
            {
                FleckMaker.ThrowLightningGlow(pawn.DrawPos, pawn.Map, 1.5f);
                Messages.Message("CVG_RootExtract_Lost".Translate(billDoer.Named("DOCTOR"), pawn.Named("PAWN")), pawn, MessageTypeDefOf.NegativeEvent);
                // Los reinos altos se resisten: una raíz que se deshace puede llevarse al donante (decisión del usuario, 2026-10-24).
                if (qiRealm != null && Rand.Chance(qiRealm.rootExtractionDeathChance))
                    pawn.Kill(null, pawn.health.hediffSet.GetFirstHediffOfDef(CVG_RootDefOf.CVG_RootTorn));
            }

            TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
            if (pawn.Dead) ThoughtUtility.GiveThoughtsForPawnExecuted(pawn, billDoer, PawnExecutionKind.OrganHarvesting);
            else
            {
                // Los mismos pensamientos y eventos de Ideology que la extracción de órganos: los preceptos lo juzgan solos.
                ThoughtUtility.GiveThoughtsForPawnOrganHarvested(pawn, billDoer);
                pawn.needs?.mood?.thoughts.memories.TryGainMemory(CVG_RootDefOf.CVG_Thought_RootTorn);
                pawn.needs?.mood?.thoughts.memories.TryGainMemory(CVG_RootDefOf.CVG_Thought_RootTornBy, billDoer);
            }
            if (violation && pawn.HomeFaction != null) ReportViolation(pawn, billDoer, pawn.HomeFaction, -70);
        }
    }

    /// <summary>Implantar una raíz sellada: reemplaza la del paciente; deja "raíz ajena" (y la costura de qi si es la máquina).</summary>
    public class Recipe_ImplantSpiritRoot : Recipe_Surgery
    {
        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null) =>
            base.AvailableOnNow(thing, part) && thing is Pawn p && CompCultivator.For(p) != null && !SpiritRootUtility.IsArtificial(p);

        public override AcceptanceReport AvailableReport(Thing thing, BodyPartRecord part = null)
        {
            if (!AvailableOnNow(thing, part)) return false;
            return RootSurgeryUtility.BedReport(thing);
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (billDoer == null || CheckSurgeryFail(billDoer, pawn, ingredients, part, bill)) return;
            var sealedRoot = ingredients?.Select(t => t.TryGetComp<CompSealedRoot>()).FirstOrDefault(c => c != null);
            if (sealedRoot == null)
            {
                Log.Warning("[Convergence] implant spirit root: no sealed root among the ingredients (" +
                    (ingredients == null ? "null" : string.Join(", ", ingredients.Select(t => t.def.defName))) + ")");
                return;
            }
            var ext = RootSurgeryUtility.Ext(pawn.CurrentBed());

            SpiritRootUtility.ImplantRoot(pawn, sealedRoot.Grade, sealedRoot.elements);
            pawn.health.AddHediff(ext?.foreignRootHediff ?? CVG_RootDefOf.CVG_ForeignRoot);
            if (ext?.permanentHediff != null) pawn.health.AddHediff(ext.permanentHediff);

            TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
            if (ModsConfig.IdeologyActive)
                Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.InstalledOrgan, billDoer.Named(HistoryEventArgsNames.Doer)));
            Messages.Message("CVG_RootImplant_Success".Translate(pawn.Named("PAWN"), sealedRoot.Grade.label,
                sealedRoot.donor.NullOrEmpty() ? "?" : sealedRoot.donor), pawn, MessageTypeDefOf.PositiveEvent);
        }
    }

    /// <summary>"Raíz ajena": al terminar, chance de rechazo (la raíz baja un grado o se pierde).</summary>
    public class HediffCompProperties_RootRejection : HediffCompProperties
    {
        public float rejectChance = 0.1f;

        public HediffCompProperties_RootRejection() => compClass = typeof(HediffComp_RootRejection);
    }

    public class HediffComp_RootRejection : HediffComp
    {
        public HediffCompProperties_RootRejection Props => (HediffCompProperties_RootRejection)props;

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            var pawn = Pawn;
            if (pawn == null || pawn.Dead || parent.ShouldRemove == false || !Rand.Chance(Props.rejectChance)) return;
            bool kept = SpiritRootUtility.Lower(pawn);
            if (pawn.Faction == Faction.OfPlayer)
                Messages.Message((kept ? "CVG_RootRejected_Lowered" : "CVG_RootRejected_Lost").Translate(pawn.Named("PAWN")), pawn,
                    MessageTypeDefOf.NegativeEvent);
        }
    }

    /// <summary>Quien espera una cirugía de raíz se acuesta en una cama que la permita (altar o mesa de qi encendida).</summary>
    [HarmonyPatch(typeof(RestUtility), nameof(RestUtility.FindPatientBedFor))]
    public static class Patch_RestUtility_FindRootSurgeryBed
    {
        [HarmonyPriority(Priority.High)]
        public static bool Prefix(Pawn pawn, ref Building_Bed __result)
        {
            var bills = pawn?.health?.surgeryBills;
            if (bills == null || pawn.Map == null || !bills.Bills.Any(b => b is Bill_Medical m && RootSurgeryUtility.IsRootSurgery(m.recipe)))
                return true;
            if (pawn.CurrentBed() is Building_Bed cur && RootSurgeryUtility.IsUsable(cur)) { __result = cur; return false; }
            Building_Bed best = null;
            float bestDist = float.MaxValue;
            foreach (var bed in RootSurgeryUtility.UsableBeds(pawn.Map))
            {
                if (!RestUtility.IsValidBedFor(bed, pawn, pawn, checkSocialProperness: false, allowMedBedEvenIfSetToNoCare: true,
                        ignoreOtherReservations: false, pawn.GuestStatus)) continue;
                if (!pawn.CanReach(bed, Verse.AI.PathEndMode.OnCell, Danger.Deadly)) continue;
                float d = bed.Position.DistanceToSquared(pawn.Position);
                if (d < bestDist) { bestDist = d; best = bed; }
            }
            if (best == null) return true;
            __result = best;
            return false;
        }
    }

    [DefOf]
    public static class CVG_RootDefOf
    {
        public static HediffDef CVG_RootTorn;
        public static HediffDef CVG_ForeignRoot;
        public static ThoughtDef CVG_Thought_RootTorn;
        public static ThoughtDef CVG_Thought_RootTornBy;

        static CVG_RootDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(CVG_RootDefOf));
    }
}
