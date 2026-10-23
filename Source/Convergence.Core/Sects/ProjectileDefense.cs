using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Defensa contra proyectiles (balas, flechas, técnicas a distancia; no explosiones). Dos stats que cualquier rasgo, gen, hediff o
    /// técnica puede dar por XML:
    /// <list type="bullet">
    /// <item><c>CVG_ProjectileDeflectChance</c>: probabilidad de desviar el proyectil entero (espadachines: la hoja corta la bala en el aire).</item>
    /// <item><c>CVG_ProjectileResistance</c>: fracción del daño de proyectil que se ignora (cultivadores del cuerpo: la piel aguanta).</item>
    /// </list>
    /// La aplica <see cref="CompCultivator.PostPreApplyDamage"/>, antes de la supresión de reino y los escudos.
    /// </summary>
    public static class ProjectileDefense
    {
        public const float MaxDeflect = 0.5f;
        public const float MaxResistance = 0.5f;

        private static StatDef deflectStat, resistStat;
        public static StatDef DeflectStat => deflectStat ??= DefDatabase<StatDef>.GetNamedSilentFail("CVG_ProjectileDeflectChance");
        public static StatDef ResistStat => resistStat ??= DefDatabase<StatDef>.GetNamedSilentFail("CVG_ProjectileResistance");

        public static bool IsProjectileDamage(DamageInfo dinfo) =>
            dinfo.Def != null && !dinfo.Def.isExplosive && (dinfo.Def.isRanged || (dinfo.Weapon != null && dinfo.Weapon.IsRangedWeapon));

        /// <summary>Devuelve true si el proyectil se desvió (sin daño); si no, reduce el daño por la resistencia.</summary>
        public static bool Apply(Pawn pawn, ref DamageInfo dinfo)
        {
            if (pawn == null || !IsProjectileDamage(dinfo) || pawn.Downed) return false;
            return Apply(pawn, ref dinfo, DeflectStat == null ? 0f : pawn.GetStatValue(DeflectStat), ResistStat == null ? 0f : pawn.GetStatValue(ResistStat));
        }

        /// <summary>Lo mismo con los valores ya calculados (también lo usa el autotest).</summary>
        public static bool Apply(Pawn pawn, ref DamageInfo dinfo, float deflect, float resist)
        {
            if (!IsProjectileDamage(dinfo)) return false;
            deflect = Mathf.Clamp(deflect, 0f, MaxDeflect);
            resist = Mathf.Clamp(resist, 0f, MaxResistance);
            if (deflect > 0f && Rand.Chance(deflect))
            {
                if (pawn != null && pawn.Spawned) MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "CVG_ProjectileDeflected".Translate(), Color.white);
                return true;
            }
            if (resist > 0f) dinfo.SetAmount(dinfo.Amount * (1f - resist));
            return false;
        }
    }

    /// <summary>
    /// El stat vale 0 si el pawn no empuña un arma cuerpo a cuerpo con alguna de estas etiquetas (<c>weaponTags</c>) o categorías: el desvío
    /// de los espadachines solo funciona con la espada en la mano.
    /// </summary>
    public class StatPart_RequiresMeleeWeapon : StatPart
    {
        public List<string> weaponTags;
        public List<ThingCategoryDef> categories;

        private bool Holds(StatRequest req, out Thing weapon)
        {
            weapon = (req.Thing as Pawn)?.equipment?.Primary;
            if (weapon == null || !weapon.def.IsMeleeWeapon) return false;
            if (weaponTags.NullOrEmpty() && categories.NullOrEmpty()) return true;
            var def = weapon.def;
            return (weaponTags != null && def.weaponTags != null && def.weaponTags.Any(weaponTags.Contains))
                || (categories != null && def.thingCategories != null && def.thingCategories.Any(categories.Contains));
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.Thing is Pawn && !Holds(req, out _)) val = 0f;
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!(req.Thing is Pawn)) return null;
            return Holds(req, out var w) ? "CVG_StatPart_WieldingSword".Translate(w.LabelShort) : "CVG_StatPart_NoSword".Translate();
        }
    }
}
