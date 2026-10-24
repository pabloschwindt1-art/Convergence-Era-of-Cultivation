using Verse;

namespace Convergence
{
    /// <summary>Tecno-cultivo o cultivo clásico (sin tecnología). Ver docs/DISENO.md §2.1.</summary>
    public enum WorldMode
    {
        TechCultivation,
        Classic
    }

    /// <summary>Cómo se dispara la Convergencia.</summary>
public class ConvergenceSettings : ModSettings
    {
        public WorldMode worldMode = WorldMode.TechCultivation;
        /// <summary>Días hasta la Convergencia si el jugador nunca acepta al sabio (60 = el comienzo del segundo año).</summary>
        public int convergenceForcedDay = 60;
        public float spiritRootChance = 0.25f;
        public float celestialDissonanceFactor = 1f;
        /// <summary>Multiplicador de la probabilidad de nacer con un linaje de sangre (módulos de razas).</summary>
        public float bloodlineChanceFactor = 1f;
        /// <summary>Mundo wuxia: las casillas solo usan rocas wuxia (módulo de recursos).</summary>
        public bool wuxiaRocksOnly;
        /// <summary>
        /// Dificultad de cultivo de las partidas nuevas (velocidad, tribulación, supresión, enemigos, eventos, riqueza).
        /// Cada partida guarda la suya en <see cref="GameComponent_Convergence"/>.
        /// </summary>
        public CultivationDifficultyValues difficulty = new CultivationDifficultyValues();
        /// <summary>Multiplicador de la probabilidad de que un animal despierte como bestia espiritual.</summary>
        public float spiritBeastChanceFactor = 1f;
        /// <summary>Cultivo ya iniciado al generar pawns (DISENO §4.1d).</summary>
        public bool preCultivationEnabled = true;
        public float preCultivationChanceFactor = 1f;
        /// <summary>Multiplicador de la frecuencia de historias de fondo de tier 2-4 (1 = pesos base 25 % / 8 % / 2 %).</summary>
        public float legendaryBackstoryChance = 1f;
        /// <summary>Multiplica la disonancia celestial de los implantes que no la definen (biónicos vanilla o de otros mods). 0 = no suman.</summary>
        public float implantDissonanceFactor = 1f;
        /// <summary>Máximo de grupos de huéspedes (visitantes, caravanas) a la vez en un mapa. 0 = sin tope. Ver <see cref="GuestLimit"/>.</summary>
        public int maxGuestGroups = 2;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref worldMode, "worldMode", WorldMode.TechCultivation);
            Scribe_Values.Look(ref convergenceForcedDay, "convergenceForcedDay", 60);
            Scribe_Values.Look(ref spiritRootChance, "spiritRootChance", 0.25f);
            Scribe_Values.Look(ref celestialDissonanceFactor, "celestialDissonanceFactor", 1f);
            Scribe_Values.Look(ref bloodlineChanceFactor, "bloodlineChanceFactor", 1f);
            Scribe_Values.Look(ref wuxiaRocksOnly, "wuxiaRocksOnly");
            Scribe_Deep.Look(ref difficulty, "difficulty");
            Scribe_Values.Look(ref spiritBeastChanceFactor, "spiritBeastChanceFactor", 1f);
            Scribe_Values.Look(ref preCultivationEnabled, "preCultivationEnabled", true);
            Scribe_Values.Look(ref preCultivationChanceFactor, "preCultivationChanceFactor", 1f);
            Scribe_Values.Look(ref legendaryBackstoryChance, "rareBackstoryFactor", 1f);
            Scribe_Values.Look(ref implantDissonanceFactor, "implantDissonanceFactor", 1f);
            Scribe_Values.Look(ref maxGuestGroups, "maxGuestGroups", 2);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && difficulty == null) difficulty = new CultivationDifficultyValues();
        }
    }
}
