using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>Elemento del qi (metal, madera, agua, fuego, tierra…). Definido en XML para poder ampliarlo.</summary>
    public class QiElementDef : Def
    {
        public Color color = Color.white;
        public int order;
        /// <summary>
        /// Raíz variante (como el Trueno en Tale of Immortal): nunca sale en el sorteo normal de elementos. Aparece con
        /// <see cref="variantChance"/> cuando la raíz sorteada tiene todos los <see cref="variantOf"/> (que reemplaza), o por
        /// un linaje (<see cref="BloodlinePurity.rootElement"/>).
        /// </summary>
        public bool variant;
        public List<QiElementDef> variantOf;
        public float variantChance;
        /// <summary>Multiplicador del daño de los rayos de la tribulación para una raíz con este elemento (Trueno: menos).</summary>
        public float tribulationDamageFactor = 1f;
        /// <summary>Afinidad con el entorno a la altura del Núcleo Dorado (resistencia al entorno 50 %); escala con ella
        /// (<see cref="StatPart_EnvironmentResistance"/>). Offsets se suman; factores van de 1 a su valor.</summary>
        public List<StatModifier> affinityOffsets;
        public List<StatModifier> affinityFactors;
    }

    /// <summary>Calidad de la raíz espiritual (inferior → celestial).</summary>
    public class SpiritRootGradeDef : Def
    {
        public int order;
        /// <summary>Peso relativo al generar colonos.</summary>
        public float commonality = 1f;
        /// <summary>Multiplicador de velocidad de cultivo (se usa desde M3).</summary>
        public float cultivationSpeedFactor = 1f;
        /// <summary>Grado del rasgo <c>CVG_SpiritRoot</c> que representa esta calidad sin Biotech.</summary>
        public int traitDegree;
        /// <summary>Multiplicador de la probabilidad de avance de reino.</summary>
        public float breakthroughFactor = 1f;
    }

    /// <summary>
    /// Marca un gen o un grado de rasgo como parte de la raíz espiritual.
    /// Un gen puede aportar la calidad, un elemento, o ambos.
    /// </summary>
    public class DefModExtension_SpiritRoot : DefModExtension
    {
        public SpiritRootGradeDef grade;
        public QiElementDef element;
    }

    /// <summary>Marca un rasgo o gen como cuerpo especial (体质).</summary>
    public class DefModExtension_SpecialBody : DefModExtension
    {
        /// <summary>Probabilidad de que un colono nazca con este cuerpo.</summary>
        public float chance = 0.01f;
        /// <summary>Solo aparece en colonos sin raíz espiritual (cuerpos de templado: acero, espada…).</summary>
        public bool rootlessOnly;
        /// <summary>Elementos con afinidad (opcional).</summary>
        public List<QiElementDef> affinities;
    }

    /// <summary>
    /// Reino de cultivo (Refinamiento de Qi, Fundación…). Cada reino tiene <see cref="stages"/> etapas y un
    /// hediff con una etapa por cada una, que aplica los stats y muestra el reino en la pestaña de salud.
    /// Cada reino pertenece a un <see cref="CultivationPathDef"/>; dentro del camino se recorren por <see cref="order"/>:
    /// para agregar uno basta otro Def en XML.
    /// Los campos "breakthrough*" y "tribulation*" describen el avance para ENTRAR a este reino.
    /// </summary>
    public class CultivationRealmDef : Def
    {
        public CultivationPathDef path;
        public int order;
        public HediffDef hediff;
        public int stages = 4;
        /// <summary>Progreso necesario para llenar cada etapa.</summary>
        public float progressPerStage = 1000f;
        /// <summary>Progreso por hora de cultivo con todos los factores en 1.</summary>
        public float progressPerHour = 50f;
        /// <summary>
        /// Multiplicador de la vida (salud máxima de todas las partes, <c>Pawn.HealthScale</c>) en la etapa 1 (min) y
        /// en la última (max). Se escala con la calidad del fundamento.
        /// </summary>
        public FloatRange healthScaleFactor = new FloatRange(1f, 1f);

        /// <summary>Objeto que el colono toma antes del retiro si hay uno en el mapa (p. ej. la Píldora de Fundación).</summary>
        public ThingDef breakthroughItem;
        /// <summary>
        /// El avance exige tener <see cref="breakthroughItem"/> a mano y lo consume (camino del cuerpo: sangre o núcleo de
        /// bestia espiritual). Si es false, el objeto es una ayuda opcional (píldora).
        /// </summary>
        public bool breakthroughItemRequired;
        /// <summary>Multiplicador de la probabilidad de avance (los reinos altos son más difíciles).</summary>
        public float breakthroughChanceFactor = 1f;
        /// <summary>Horas de juego del retiro de avance.</summary>
        public float breakthroughHours = 4f;
        /// <summary>Tras un fallo, días hasta poder reintentar.</summary>
        public float breakthroughCooldownDays = 2f;
        /// <summary>Hediff que deja un avance fallido (desviación de qi).</summary>
        public HediffDef breakthroughFailureHediff;
        /// <summary>Requisitos para intentar el avance (atributos de fundamento, reinos de otros caminos…).</summary>
        public List<BreakthroughRequirement> breakthroughRequirements;

        /// <summary>Entrar a este reino requiere superar una tribulación celestial.</summary>
        public bool tribulation;
        /// <summary>Rayos de la tribulación sin disonancia.</summary>
        public int tribulationStrikes = 5;
        /// <summary>Rayos extra por cada punto de disonancia celestial.</summary>
        public float tribulationStrikesPerDissonance = 2f;
        /// <summary>Daño de quemadura de cada rayo sin disonancia.</summary>
        public float tribulationDamage = 6f;
        /// <summary>Daño extra por rayo (fracción) por cada punto de disonancia celestial.</summary>
        public float tribulationDamagePerDissonance = 0.5f;
        /// <summary>Ticks entre rayos.</summary>
        public IntRange tribulationStrikeInterval = new IntRange(60, 120);

        /// <summary>Multiplicador de vida de la etapa <paramref name="stage"/> (1..stages), sin calidad de fundamento.</summary>
        /// <summary>
        /// Velocidad del envejecimiento biológico de los adultos en la etapa 1 (min) y en la última (max): los reinos lo
        /// frenan y los altos casi lo paran (como el gen Ageless). Se aplica sobre <c>Pawn_AgeTracker.BiologicalTicksPerTick</c>.
        /// </summary>
        public FloatRange biologicalAgingFactor = new FloatRange(1f, 1f);

        public float AgingFactorAtStage(int stage) =>
            stages <= 1 ? biologicalAgingFactor.min : Mathf.Lerp(biologicalAgingFactor.min, biologicalAgingFactor.max, (stage - 1f) / (stages - 1f));

        public float HealthScaleAtStage(int stage) =>
            stages <= 1 ? healthScaleFactor.min : Mathf.Lerp(healthScaleFactor.min, healthScaleFactor.max, (stage - 1f) / (stages - 1f));

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var e in base.ConfigErrors()) yield return e;
            if (path == null) yield return "path is null";
            if (hediff == null) yield return "hediff is null";
            if (stages < 1) yield return "stages must be >= 1";
            if (progressPerStage <= 0f) yield return "progressPerStage must be > 0";
            if (breakthroughHours <= 0f) yield return "breakthroughHours must be > 0";
        }
    }

    /// <summary>
    /// Feng shui de un edificio: cuánto qi aporta a la concentración del lugar y de qué elemento.
    /// El elemento todavía no se usa (la armonía de los cinco elementos llega más adelante).
    /// </summary>
    public class DefModExtension_FengShui : DefModExtension
    {
        public float qi;
        public QiElementDef element;
    }

    /// <summary>En un <c>HediffDef</c>: mientras el pawn lo tenga no puede seguir los caminos indicados (todos si <c>paths</c> está vacío).</summary>
    public class DefModExtension_BlocksCultivation : DefModExtension
    {
        public List<CultivationPathDef> paths;
    }

    /// <summary>Marca un edificio como lugar de cultivo (cojín, plataforma de jade…).</summary>
    public class DefModExtension_CultivationSpot : DefModExtension
    {
        /// <summary>Multiplicador de la regeneración de qi mientras se cultiva aquí.</summary>
        public float qiRegenFactor = 3f;
        /// <summary>Duración de cada sesión de cultivo, en ticks.</summary>
        public int sessionTicks = 2500;
    }

    /// <summary>
    /// Marca un hediff como ayuda al avance (p. ej. "píldora absorbida"): se consume al terminar el retiro de avance,
    /// salga bien o mal. Su efecto va en el XML del hediff (statOffsets de <c>CVG_BreakthroughChance</c>).
    /// </summary>
    public class DefModExtension_BreakthroughAid : DefModExtension
    {
    }

    /// <summary>Datos de la concentración de qi (va en el <c>RoomStatDef CVG_QiConcentration</c>).</summary>
    public class DefModExtension_QiConcentration : DefModExtension
    {
        /// <summary>Qi ambiental de cualquier lugar tras la Convergencia.</summary>
        public float ambient = 1f;
        /// <summary>Al aire libre (o en habitaciones enormes) se suman los edificios a esta distancia.</summary>
        public float outdoorRadius = 2.9f;
        /// <summary>Concentración → multiplicador de la velocidad de cultivo.</summary>
        public SimpleCurve cultivationFactorCurve;
        /// <summary>Concentración → multiplicador de la probabilidad de avance de reino.</summary>
        public SimpleCurve breakthroughFactorCurve;
    }

    [DefOf]
    public static class CVG_DefOf
    {
        public static TraitDef CVG_SpiritRoot;
        public static IncidentDef CVG_Convergence;

        public static CultivationPathDef CVG_Path_Qi;
        public static CultivationPathDef CVG_Path_Body;

        public static StatDef CVG_QiMax;
        public static StatDef CVG_QiRegen;
        public static StatDef CVG_CultivationSpeed;
        public static RoomStatDef CVG_QiConcentration;
        public static StatDef CVG_BreakthroughChance;
        public static StatDef CVG_CelestialDissonance;
        public static StatDef CVG_TechniqueCost;
        public static StatDef CVG_SpiritGrade;
        public static StatDef CVG_PurificationChance;
        public static JobDef CVG_Cultivate;
        public static JobDef CVG_Breakthrough;
        public static ThingDef CVG_JadeSlip;
        public static JobDef CVG_TemperBody;
        public static StatDef CVG_BodyTemperingSpeed;
        public static HediffDef CVG_SpiritBeast;
        public static ThingDef CVG_SpiritMeat;
        public static ThingDef CVG_BeastBlood;
        public static ThingDef CVG_BeastCore;

        static CVG_DefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(CVG_DefOf));
        }
    }
}
