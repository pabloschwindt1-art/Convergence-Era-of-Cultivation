using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Cultivo ya iniciado al generar pawns (docs/DISENO.md §4.1d): cultivadores caídos por grietas de Jiuxiao antes de la
    /// Convergencia o que recuerdan una vida anterior. Una por camino: probabilidad base y opciones ponderadas por
    /// <c>order</c> del reino (si no existe ese reino en el camino, se usa el más alto disponible). Ajustes: interruptor y
    /// multiplicador. Antes de la Convergencia el reino queda guardado pero latente (sin hediff) y se activa al ocurrir.
    /// </summary>
    public class PreCultivationDef : Def
    {
        public CultivationPathDef path;
        /// <summary>Probabilidad de que un pawn que puede seguir el camino nazca con cultivo.</summary>
        public float chance = 0.1f;
        /// <summary>Solo para pawns con cuerpo especial (camino del cuerpo).</summary>
        public bool requiresSpecialBody;
        public List<PreCultivationOption> options = new List<PreCultivationOption>();
        public FloatRange foundationQuality = new FloatRange(0.8f, 1.2f);

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var e in base.ConfigErrors()) yield return e;
            if (path == null) yield return "path is null";
            if (options.NullOrEmpty()) yield return "no options";
        }
    }

    public class PreCultivationOption
    {
        public int realmOrder = 1;
        public IntRange stage = new IntRange(1, 1);
        public float weight = 1f;
    }

    public static class PreCultivation
    {
        /// <summary>Tira el cultivo previo de cada camino que el pawn podría seguir (raíz → qi; cuerpo especial sin raíz → cuerpo).</summary>
        public static void TryApply(Pawn pawn, CompCultivator comp)
        {
            var settings = ConvergenceMod.Settings;
            if (!settings.preCultivationEnabled || settings.preCultivationChanceFactor <= 0f) return;
            foreach (var def in DefDatabase<PreCultivationDef>.AllDefsListForReading)
            {
                if (!Eligible(comp, def)) continue;
                if (!Rand.Chance(Mathf.Clamp01(def.chance * settings.preCultivationChanceFactor))) continue;
                var option = def.options.RandomElementByWeightWithFallback(o => o.weight);
                if (option != null) Apply(pawn, comp, def, option);
            }
        }

        /// <summary>Puede seguir el camino cuando despierte (la Convergencia todavía puede no haber ocurrido).</summary>
        public static bool Eligible(CompCultivator comp, PreCultivationDef def)
        {
            if (def.path == null) return false;
            if (def.path.requiresSpiritRoot) return comp.HasRoot;
            if (def.requiresSpecialBody && comp.SpecialBody == null) return false;
            return !comp.HasRoot && (comp.Pawn.ageTracker?.Adult ?? false);
        }

        public static void Apply(Pawn pawn, CompCultivator comp, PreCultivationDef def, PreCultivationOption option)
        {
            var realms = DefDatabase<CultivationRealmDef>.AllDefsListForReading.Where(r => r.path == def.path).OrderBy(r => r.order).ToList();
            if (realms.Count == 0) return;
            var realm = realms.LastOrDefault(r => r.order <= option.realmOrder) ?? realms[0];
            int stage = Mathf.Clamp(option.stage.RandomInRange, 1, realm.stages);
            var tracker = comp.TrackerFor(def.path, create: true);
            tracker.SetPreCultivated(realm, stage, def.foundationQuality.RandomInRange, activate: comp.CanFollow(def.path));
        }
    }
}
