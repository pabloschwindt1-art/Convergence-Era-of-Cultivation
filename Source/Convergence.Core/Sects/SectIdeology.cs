using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Componente de ritual: no se puede empezar sin al menos <see cref="count"/> creyentes de la ideología presentes además de los roles
    /// (p. ej. el templado del Cuerpo de Espada pide dos miembros más que el maestro y el sujeto). No cambia la calidad.
    /// </summary>
    public class RitualOutcomeComp_MinBelievers : RitualOutcomeComp
    {
        public int count = 2;

        public override bool DataRequired => false;

        public override bool Applies(LordJob_Ritual ritual) => true;

        public static int Believers(Precept_Ritual ritual, RitualRoleAssignments assignments) =>
            assignments?.Participants?.Count(p => p.Ideo == ritual?.ideo && assignments.RoleForPawn(p) == null) ?? 0;

        public override IEnumerable<string> BlockingIssues(Precept_Ritual ritual, TargetInfo target, RitualRoleAssignments assignments)
        {
            int n = Believers(ritual, assignments);
            if (n < count) yield return "CVG_Ritual_NeedBelievers".Translate(count, Find.ActiveLanguageWorker.Pluralize(ritual?.ideo?.memberName ?? ""), n);
        }

        public override string GetDesc(LordJob_Ritual ritual = null, RitualOutcomeComp_Data data = null) =>
            "CVG_Ritual_NeedBelieversDesc".Translate(count);

        public override QualityFactor GetQualityFactor(Precept_Ritual ritual, TargetInfo ritualTarget, RitualObligation obligation,
            RitualRoleAssignments assignments, RitualOutcomeComp_Data data) => null;
    }

    /// <summary>
    /// En un <c>PreceptDef</c> de rol: el ánimo por "rol vacío" solo aparece con al menos <see cref="minBelievers"/> colonos de esa ideología
    /// (el rol se puede asignar antes). Solo afecta a los roles que lo llevan, no a los de vanilla ni a otras ideologías.
    /// </summary>
    public class DefModExtension_RoleEmptyMinBelievers : DefModExtension
    {
        public int minBelievers = 4;
    }

    [HarmonyPatch(typeof(Thought_IdeoRoleEmpty), "CurrentStateInternal")]
    public static class Patch_Thought_IdeoRoleEmpty_MinBelievers
    {
        public static void Postfix(Thought_IdeoRoleEmpty __instance, ref ThoughtState __result)
        {
            if (!__result.Active) return;
            var role = __instance.Role;
            var ext = role?.def?.GetModExtension<DefModExtension_RoleEmptyMinBelievers>();
            if (ext == null) return;
            int believers = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists.Count(p => p.Ideo == role.ideo);
            if (believers < ext.minBelievers) __result = ThoughtState.Inactive;
        }
    }

    /// <summary>
    /// El viejo "cuerpo de espada celestial" (rasgo aparte) pasó a ser el grado 3 (jade celestial) del Cuerpo de Espada: al cargar una partida
    /// se convierte. El rasgo viejo ya no nace (probabilidad 0).
    /// </summary>
    public class GameComponent_SwordBodyMigration : GameComponent
    {
        public GameComponent_SwordBodyMigration(Game game) { }

        public override void LoadedGame()
        {
            var old = DefDatabase<TraitDef>.GetNamedSilentFail("CVG_Body_CelestialSword");
            var sword = DefDatabase<TraitDef>.GetNamedSilentFail("CVG_Body_Sword");
            if (old == null || sword == null) return;
            int n = 0;
            foreach (var p in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead)
            {
                if (p?.story?.traits?.GetTrait(old) == null) continue;
                ConstitutionUtility.RemoveTrait(p, old);
                ConstitutionUtility.SetTrait(p, sword, 3);
                n++;
            }
            if (n > 0) Log.Message("[Convergence] " + n + " celestial sword bodies converted to the celestial jade grade of the Sword Body.");
        }
    }
}
