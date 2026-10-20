using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Hediff de un reino. Aplica los stats de la etapa escalados por la calidad del fundamento del tracker:
    /// statOffsets × calidad y statFactors 1 + (factor − 1) × calidad. Fundamento sólido → el reino rinde más;
    /// inestable (subido con píldoras) → menos.
    /// </summary>
    public class Hediff_CultivationRealm : HediffWithComps
    {
        private static readonly MethodInfo CloneMethod = typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
        private static Dictionary<HediffDef, CultivationRealmDef> realmByHediff;

        // Caché de la etapa escalada (no se guarda).
        private HediffStage scaledStage;
        private HediffStage scaledFrom;
        private float scaledQuality = -1f;

        public CultivationRealmDef RealmDef
        {
            get
            {
                if (realmByHediff == null)
                {
                    realmByHediff = new Dictionary<HediffDef, CultivationRealmDef>();
                    foreach (var r in DefDatabase<CultivationRealmDef>.AllDefsListForReading)
                        if (r.hediff != null) realmByHediff[r.hediff] = r;
                }
                return realmByHediff.TryGetValue(def, out var realm) ? realm : null;
            }
        }

        public float FoundationQuality
        {
            get
            {
                var realm = RealmDef;
                var tracker = realm == null ? null : CompCultivator.For(pawn)?.TrackerFor(realm.path);
                return tracker != null && tracker.Realm == realm ? tracker.FoundationQuality : 1f;
            }
        }

        public override HediffStage CurStage
        {
            get
            {
                var stage = base.CurStage;
                if (stage == null) return null;
                float q = FoundationQuality;
                if (Mathf.Approximately(q, 1f)) return stage;
                if (scaledStage == null || scaledFrom != stage || !Mathf.Approximately(scaledQuality, q))
                {
                    scaledStage = Scale(stage, q);
                    scaledFrom = stage;
                    scaledQuality = q;
                }
                return scaledStage;
            }
        }

        public override string TipStringExtra
        {
            get
            {
                string s = base.TipStringExtra;
                string line = "CVG_FoundationQualityLine".Translate(FoundationQualityLabel(FoundationQuality), FoundationQuality.ToStringPercent());
                return s.NullOrEmpty() ? line : s + "\n" + line;
            }
        }

        public static string FoundationQualityLabel(float q) =>
            (q >= 1.1f ? "CVG_Foundation_Solid" : q >= 0.9f ? "CVG_Foundation_Stable" : "CVG_Foundation_Unstable").Translate();

        private static HediffStage Scale(HediffStage stage, float q)
        {
            var copy = (HediffStage)CloneMethod.Invoke(stage, null);
            copy.statOffsets = ScaleList(stage.statOffsets, v => v * q);
            copy.statFactors = ScaleList(stage.statFactors, v => 1f + (v - 1f) * q);
            return copy;
        }

        private static List<StatModifier> ScaleList(List<StatModifier> list, System.Func<float, float> f)
        {
            if (list == null) return null;
            var result = new List<StatModifier>(list.Count);
            foreach (var m in list) result.Add(new StatModifier { stat = m.stat, value = f(m.value) });
            return result;
        }
    }
}
