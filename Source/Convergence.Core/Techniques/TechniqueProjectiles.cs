using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>Proyectil de técnica: su daño se multiplica por el poder de la técnica que lo lanzó.</summary>
    public interface ITechniqueProjectile
    {
        float DamageFactor { get; set; }
    }

    /// <summary>Proyectil de un impacto (aguja de metal, espina, saeta de escarcha…).</summary>
    public class Projectile_TechniqueBullet : Bullet, ITechniqueProjectile
    {
        private float damageFactor = 1f;

        public float DamageFactor
        {
            get => damageFactor;
            set => damageFactor = Mathf.Max(0f, value);
        }

        public override int DamageAmount => Mathf.Max(1, Mathf.RoundToInt(base.DamageAmount * damageFactor));

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref damageFactor, "damageFactor", 1f);
        }
    }

    /// <summary>Proyectil que explota (bola de fuego…): la explosión usa el daño escalado.</summary>
    public class Projectile_TechniqueExplosive : Projectile_Explosive, ITechniqueProjectile
    {
        private float damageFactor = 1f;

        public float DamageFactor
        {
            get => damageFactor;
            set => damageFactor = Mathf.Max(0f, value);
        }

        public override int DamageAmount => Mathf.Max(1, Mathf.RoundToInt(base.DamageAmount * damageFactor));

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref damageFactor, "damageFactor", 1f);
        }
    }
}
