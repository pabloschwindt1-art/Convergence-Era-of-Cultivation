using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Grupo de genes de sangre por niveles (Vitalidad bestial, Escamas de dragón…). Sin <see cref="bloodline"/> es común a
    /// todos los linajes; con él, es un gen firma de ese linaje. Los genes del grupo llevan <see cref="DefModExtension_BloodlineGeneTier"/>.
    /// </summary>
    public class BloodlineGeneGroupDef : Def
    {
        /// <summary>Linaje dueño (gen firma) o null (gen común a todos).</summary>
        public BloodlineDef bloodline;
        public int order;
    }

    /// <summary>
    /// Marca un gen como el nivel <see cref="tier"/> (1 = Rastro … 4 = Ancestral) de un grupo. Al purificar, el colono pasa
    /// al gen de mayor nivel del grupo que no supere su pureza. Un grupo puede empezar en un nivel alto (p. ej. Nirvana en III).
    /// </summary>
    public class DefModExtension_BloodlineGeneTier : DefModExtension
    {
        public BloodlineGeneGroupDef group;
        public int tier = 1;
        /// <summary>Capacidades que da este nivel, solo tras la Convergencia (como las del linaje).</summary>
        public List<AbilityDef> awakenedAbilities;
    }

    /// <summary>Regenera partes perdidas y cicatrices cada cierto tiempo (tras la Convergencia). Gen con <c>Convergence.Gene_Bloodline</c>.</summary>
    public class DefModExtension_BloodRegeneration : DefModExtension
    {
        public float intervalDays = 10f;
        public bool regrowParts = true;
        public bool healScars = true;
    }

    /// <summary>
    /// Vidas extra (Nirvana, nueve colas): al morir tras la Convergencia, el colono se salva, se cura y gasta una vida.
    /// <see cref="rechargeDays"/> &gt; 0 recupera una vida cada tantos días.
    /// </summary>
    public class DefModExtension_ExtraLives : DefModExtension
    {
        public int lives = 1;
        public float rechargeDays;
        /// <summary>Estallido de fuego al revivir (radio; 0 = nada).</summary>
        public float fireBurstRadius;
        /// <summary>Clave Keyed del mensaje al revivir: {PAWN} y {1} = vidas restantes.</summary>
        [NoTranslate] public string messageKey = "CVG_ExtraLife_Used";
    }

    /// <summary>Aura: da un hediff a pawns cercanos (aliados, hostiles o solo entidades) mientras el portador está despierto.</summary>
    public class DefModExtension_GeneAura : DefModExtension
    {
        public float radius = 6f;
        public HediffDef hediff;
        public bool affectAllies = true;
        public bool affectHostiles;
        public bool onlyEntities;
        public bool affectSelf;
        /// <summary>Severidad del hediff; -1 = nivel del gen.</summary>
        public float severity = -1f;
        /// <summary>Ticks que dura el hediff tras salir del aura.</summary>
        public int lingerTicks = 600;
    }

    /// <summary>Contraataque: al recibir un golpe cuerpo a cuerpo, devuelve daño al atacante (tras la Convergencia).</summary>
    public class DefModExtension_Retaliation : DefModExtension
    {
        public float chance = 0.3f;
        public DamageDef damageDef;
        public float amount = 8f;
        public int cooldownTicks = 120;
    }
}
