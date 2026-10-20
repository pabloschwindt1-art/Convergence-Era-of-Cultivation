using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>Textos legibles de lo que da un reino: sección "Bonus del reino" de la pestaña y "antes → después" de la carta.</summary>
    public static class RealmBonusUtility
    {
        /// <summary>Stats que se comparan en la carta de avance (además de la vida).</summary>
        private static IEnumerable<StatDef> CompareStats
        {
            get
            {
                yield return StatDefOf.MoveSpeed;
                yield return StatDefOf.MeleeDamageFactor;
                yield return StatDefOf.IncomingDamageFactor;
                yield return CVG_DefOf.CVG_QiMax;
            }
        }

        /// <summary>Bonus del reino y etapa actuales del tracker (ya escalados por la calidad del fundamento).</summary>
        public static List<string> BonusLines(Pawn pawn, RealmTracker tracker)
        {
            var lines = new List<string>();
            if (tracker?.Realm == null) return lines;
            if (!UnityEngine.Mathf.Approximately(tracker.HealthScaleFactor, 1f))
                lines.Add("CVG_Bonus_Health".Translate(tracker.HealthScaleFactor.ToString("0.##")));

            var stage = pawn.health.hediffSet.GetFirstHediffOfDef(tracker.Realm.hediff)?.CurStage;
            if (stage == null) return lines;
            if (stage.statFactors != null)
                foreach (var m in stage.statFactors)
                    lines.Add(m.stat.LabelCap + ": x" + m.value.ToString("0.##"));
            if (stage.statOffsets != null)
                foreach (var m in stage.statOffsets)
                    lines.Add(m.stat.LabelCap + ": " + m.stat.ValueToString(m.value, ToStringNumberSense.Offset));
            if (stage.painOffset != 0f)
                lines.Add("CVG_Bonus_Pain".Translate(stage.painOffset.ToStringPercentSigned()));
            return lines;
        }

        /// <summary>Valores actuales de los stats principales y la vida, para comparar tras subir.</summary>
        public static Dictionary<StatDef, float> Snapshot(Pawn pawn, out float healthScale)
        {
            healthScale = pawn.HealthScale;
            return CompareStats.ToDictionary(s => s, s => pawn.GetStatValue(s));
        }

        /// <summary>Líneas "stat: antes → después" de las mejoras principales.</summary>
        public static string CompareLines(Pawn pawn, Dictionary<StatDef, float> before, float healthBefore)
        {
            var sb = new StringBuilder();
            float healthAfter = pawn.HealthScale;
            if (!UnityEngine.Mathf.Approximately(healthBefore, healthAfter))
                sb.AppendLine("  - " + "CVG_Bonus_HealthCompare".Translate(healthBefore.ToStringPercent(), healthAfter.ToStringPercent()));
            foreach (var kv in before)
            {
                float after = pawn.GetStatValue(kv.Key);
                if (UnityEngine.Mathf.Approximately(kv.Value, after)) continue;
                sb.AppendLine("  - " + kv.Key.LabelCap + ": " + kv.Key.ValueToString(kv.Value) + " → " + kv.Key.ValueToString(after));
            }
            return sb.ToString().TrimEnd();
        }
    }
}
