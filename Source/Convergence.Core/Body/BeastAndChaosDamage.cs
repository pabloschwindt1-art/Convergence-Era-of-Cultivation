using HarmonyLib;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Daño contra lo que no es humano (plan de fusión, 2026-10-23). Los humanos ya pasan por
    /// <see cref="CompCultivator.PostPreApplyDamage"/>; esto cubre lo demás:
    /// <list type="bullet">
    /// <item>Bestias espirituales: supresión de reino con el reino equivalente a su rango
    /// (<see cref="DefModExtension_SpiritBeast.realmByLevel"/>). Las armas de qi y las torretas de qi les pegan entero.</item>
    /// <item>Entidades del vacío (caos primordial): el qi les hace +25 % (<see cref="QiVsChaosFactor"/>) si el ataque tiene
    /// grado espiritual. No se les aplica supresión: se pueden enfrentar sin qi, como en vanilla.</item>
    /// <item>Lo que las entidades le hacen a un humano se reduce con su <c>CVG_ChaosResistance</c>.</item>
    /// </list>
    /// </summary>
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.PreApplyDamage))]
    public static class BeastAndChaosDamage
    {
        public const float QiVsChaosFactor = 1.25f;

        private static StatDef chaosResistance;
        private static StatDef ChaosResistance => chaosResistance ??= DefDatabase<StatDef>.GetNamedSilentFail("CVG_ChaosResistance");

        public static void Prefix(Pawn __instance, ref DamageInfo dinfo)
        {
            if (dinfo.Def == null || !dinfo.Def.harmsHealth || dinfo.Def.consideredHelpful || dinfo.Amount <= 0f) return;
            var race = __instance.RaceProps;
            float factor = 1f;

            if (race.Animal)
            {
                int realm = BeastRealm(__instance);
                if (realm > 0) factor *= SpiritGradeUtility.SuppressionFactor(realm, SpiritGradeUtility.AttackGrade(dinfo));
            }
            else if (race.IsAnomalyEntity)
            {
                if (SpiritGradeUtility.AttackGrade(dinfo) >= 1) factor *= QiVsChaosFactor;
            }

            if (race.Humanlike && dinfo.Instigator is Pawn attacker && attacker.RaceProps.IsAnomalyEntity && ChaosResistance != null)
                factor *= 1f - __instance.GetStatValue(ChaosResistance);

            if (factor != 1f) dinfo.SetAmount(dinfo.Amount * factor);
        }

        /// <summary>Reino equivalente de una bestia espiritual (0 si no lo es).</summary>
        public static int BeastRealm(Pawn pawn)
        {
            int level = Hediff_SpiritBeast.LevelOf(pawn);
            if (level <= 0) return 0;
            var list = SpiritBeastUtility.Ext?.realmByLevel;
            if (list.NullOrEmpty()) return level;
            return list[System.Math.Min(level, list.Count) - 1];
        }
    }
}
