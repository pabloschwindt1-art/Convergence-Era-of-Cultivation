using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence.QiTech
{
    /// <summary>
    /// Boca del cañón de una torreta (en el ThingDef de la torreta). Vanilla lanza el proyectil desde el centro del edificio
    /// (<c>caster.DrawPos</c>) y pone el destello en la casilla central; con un cabezal largo se ve que dispara la base. Con esta extensión
    /// el proyectil y el destello salen a <see cref="muzzleDistance"/> casillas del eje, hacia el blanco (a donde mira el cabezal).
    /// Poner <c>muzzleFlashScale</c> 0 en el verbo del arma montada para no duplicar el destello vanilla.
    /// </summary>
    public class DefModExtension_TurretMuzzle : DefModExtension
    {
        public float muzzleDistance = 1f;
        public float flashScale = 9f;
    }

    [HarmonyPatch(typeof(Projectile), nameof(Projectile.Launch), typeof(Thing), typeof(Vector3), typeof(LocalTargetInfo), typeof(LocalTargetInfo),
        typeof(ProjectileHitFlags), typeof(bool), typeof(Thing), typeof(ThingDef))]
    public static class Patch_Projectile_Launch_Muzzle
    {
        public static void Prefix(Thing launcher, ref Vector3 origin, LocalTargetInfo usedTarget)
        {
            if (!(launcher is Building_TurretGun turret) || !usedTarget.IsValid) return;
            var ext = turret.def.GetModExtension<DefModExtension_TurretMuzzle>();
            if (ext == null) return;
            var dir = usedTarget.CenterVector3 - origin;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f) return;
            origin += dir.normalized * ext.muzzleDistance;
            if (ext.flashScale > 0f && turret.Map != null)
                FleckMaker.Static(origin, turret.Map, FleckDefOf.ShotFlash, ext.flashScale);
        }
    }
}
