using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Un Maestro (docs/LORE.md "El Maestro", docs/lore/MAESTROS.md): quién es, cómo se genera (raíz, reino, técnicas,
    /// rasgos solo positivos), cuánto se queda, qué lecciones da (<see cref="MentorLesson"/>, ampliables con XML) y qué
    /// deja al irse. Llega con la Convergencia; el arquetipo se elige por <see cref="commonality"/>.
    /// </summary>
    public class MentorDef : Def
    {
        [MustTranslate] public string archetype;
        public float commonality = 1f;

        // Pawn
        public PawnKindDef pawnKind;
        public string firstName;
        public string nickName;
        public string lastName;
        public Gender gender = Gender.Male;
        public float biologicalAge = 60f;
        public float chronologicalAge = 300f;
        /// <summary>Rasgos (grado 0) que se le ponen; se quitan todos los demás (ninguno negativo).</summary>
        public List<TraitDef> traits = new List<TraitDef>();

        // Cultivo
        public SpiritRootGradeDef rootGrade;
        public List<QiElementDef> rootElements = new List<QiElementDef>();
        public CultivationRealmDef realm;
        public int stage = 4;
        public float foundationQuality = 1.3f;
        /// <summary>Manuales que conoce (con todas sus técnicas).</summary>
        public List<TechniqueManualDef> manuals = new List<TechniqueManualDef>();
        /// <summary>Hediff permanente mientras está: su poder de Alma Naciente, sin hambre ni crisis.</summary>
        public HediffDef presenceHediff;
        /// <summary>Hediff temporal al proteger a la colonia del primer ataque de cultivadores.</summary>
        public HediffDef protectionHediff;
        /// <summary>
        /// Ropa con la que llega (reemplaza la del generador que choque con ella). Sin esto podía llegar sin pantalones y
        /// cargar con "ingle descubierta" en ideologías que lo desaprueban (visto en el arnés -cvgplaytest).
        /// </summary>
        public List<ThingDef> apparel = new List<ThingDef>();

        // Estancia
        public float woundedChance = 0.3f;
        /// <summary>Días que se queda (un año de RimWorld = 60 días).</summary>
        public FloatRange stayDays = new FloatRange(30f, 60f);
        public List<MentorLesson> lessons = new List<MentorLesson>();
        /// <summary>Proyectos del Dao que enseña al llegar (se completan solos).</summary>
        public List<ResearchProjectDef> teachesResearch = new List<ResearchProjectDef>();
        /// <summary>Lo que deja al irse (fragmento de jade).</summary>
        public ThingDef farewellItem;
        /// <summary>Lo que deja además al irse (el sabio del caos: núcleos de caos, piedras).</summary>
        public List<ThingDefCountClass> farewellItems = new List<ThingDefCountClass>();
        /// <summary>Deja uno de estos al azar (el sabio del caos: un talismán del caos).</summary>
        public List<ThingDef> farewellChoice = new List<ThingDef>();

        // Textos de cartas ({PAWN_*} = el Maestro).
        [MustTranslate] public string arrivalWalkerText;
        [MustTranslate] public string arrivalWoundedText;
        [MustTranslate] public string protectionText;
        [MustTranslate] public string farewellText;
        [MustTranslate] public string deathText;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var e in base.ConfigErrors()) yield return e;
            if (pawnKind == null) yield return "pawnKind is null";
            if (realm == null) yield return "realm is null";
            if (commonality > 0f && lessons.NullOrEmpty()) yield return "no lessons";
        }
    }

    /// <summary>
    /// Lección del Maestro: un objetivo simple que se comprueba cada poco. Al empezar entrega <see cref="startItems"/>
    /// (p. ej. una tablilla de jade); al cumplirse, <see cref="rewards"/>. Subclases con Class= en el XML.
    /// </summary>
    public abstract class MentorLesson
    {
        [MustTranslate] public string label;
        [MustTranslate] public string description;
        public List<ThingDefCountClass> startItems;
        public List<ThingDefCountClass> rewards;

        public abstract bool IsComplete(Pawn mentor, Map map);

        /// <summary>
        /// Medida de progreso al empezar la lección (para las que cuentan lo que se hace DESPUÉS de empezar: si los colonos ya
        /// venían cultivando, la lección no se cumple sola). Por defecto 0.
        /// </summary>
        public virtual int Baseline(Pawn mentor, Map map) => 0;

        public virtual bool IsComplete(Pawn mentor, Map map, int baseline) => IsComplete(mentor, map);

        /// <summary>Medida por discípulo al empezar (thingIDNumber -> valor), para las lecciones que comparan pawn por pawn.</summary>
        public virtual Dictionary<int, int> BaselinePerPawn(Pawn mentor) => null;

        public virtual bool IsComplete(Pawn mentor, Map map, int baseline, Dictionary<int, int> perPawn) => IsComplete(mentor, map, baseline);

        public virtual string Progress(Pawn mentor, Map map, int baseline, Dictionary<int, int> perPawn) => Progress(mentor, map);

        /// <summary>Texto corto de progreso ("0 / 1"), opcional.</summary>
        public virtual string Progress(Pawn mentor, Map map) => null;

        /// <summary>Discípulos posibles: colonos libres del jugador, sin contar al Maestro.</summary>
        protected static IEnumerable<Pawn> Disciples(Pawn mentor) =>
            PawnsFinder.AllMaps_FreeColonists.Where(p => p != mentor);
    }

    /// <summary>Tener construidos <see cref="count"/> edificios <see cref="thingDef"/> (cojín de cultivo, aparato de templado…).</summary>
    public class MentorLesson_BuildThing : MentorLesson
    {
        public ThingDef thingDef;
        public int count = 1;

        private int Built(Map map) =>
            map == null || thingDef == null ? 0 : map.listerThings.ThingsOfDef(thingDef).Count(t => t.Faction?.IsPlayer ?? false);

        public override bool IsComplete(Pawn mentor, Map map) => Built(map) >= count;

        public override string Progress(Pawn mentor, Map map) => Built(map) + " / " + count;
    }

    /// <summary>
    /// Que algún discípulo llegue a la etapa <see cref="stage"/> de cualquier reino (o de <see cref="path"/>); si ya venía más arriba,
    /// que avance al menos una etapa desde que empezó la lección.
    /// </summary>
    public class MentorLesson_ReachStage : MentorLesson
    {
        public CultivationPathDef path;
        public int stage = 2;

        private int Level(Pawn p)
        {
            var c = CompCultivator.For(p);
            if (c == null) return 0;
            return c.ActiveTrackers.Where(t => t.Started && (path == null || t.Path == path)).Select(t => t.Realm.order * 10 + t.Stage).DefaultIfEmpty(0).Max();
        }

        /// <summary>Nivel de cada discípulo al empezar: se cumple cuando alguno avanza una etapa respecto de sí mismo (que otro
        /// empiece a cultivar, o que muera uno, no cuenta). Uno que no cultivaba cuenta si llega a la etapa <see cref="stage"/>.</summary>
        public override Dictionary<int, int> BaselinePerPawn(Pawn mentor) => Disciples(mentor).ToDictionary(p => p.thingIDNumber, Level);

        public override bool IsComplete(Pawn mentor, Map map, int baseline, Dictionary<int, int> perPawn)
        {
            foreach (var p in Disciples(mentor))
            {
                int now = Level(p);
                if (perPawn != null && perPawn.TryGetValue(p.thingIDNumber, out int start) && start > 0) { if (now > start) return true; }
                else if (now % 10 >= stage || now >= 20) return true;
            }
            return false;
        }

        public override string Progress(Pawn mentor, Map map, int baseline, Dictionary<int, int> perPawn) =>
            (IsComplete(mentor, map, baseline, perPawn) ? 1 : 0) + " / 1";

        public override bool IsComplete(Pawn mentor, Map map)
        {
            foreach (var p in Disciples(mentor))
            {
                var comp = CompCultivator.For(p);
                if (comp == null) continue;
                foreach (var t in comp.ActiveTrackers)
                    if (t.Started && (path == null || t.Path == path) && (t.Stage >= stage || t.Realm.order > 1)) return true;
            }
            return false;
        }
    }

    /// <summary>Que algún discípulo conozca al menos <see cref="count"/> técnicas.</summary>
    public class MentorLesson_LearnTechnique : MentorLesson
    {
        public int count = 1;

        private int Best(Pawn mentor) =>
            Disciples(mentor).Select(p => CompCultivator.For(p)?.TechniquesIfAny?.Unlocked.Count ?? 0).DefaultIfEmpty(0).Max();

        private static int Total(Pawn mentor) =>
            Disciples(mentor).Sum(p => CompCultivator.For(p)?.TechniquesIfAny?.Unlocked.Count ?? 0);

        public override bool IsComplete(Pawn mentor, Map map) => Best(mentor) >= count;

        public override int Baseline(Pawn mentor, Map map) => Total(mentor);

        /// <summary>Técnicas NUEVAS desde que empezó la lección (las que ya sabían no cuentan).</summary>
        public override bool IsComplete(Pawn mentor, Map map, int baseline) => Total(mentor) >= baseline + count;

        public override string Progress(Pawn mentor, Map map) => Best(mentor) + " / " + count;

        public override string Progress(Pawn mentor, Map map, int baseline, Dictionary<int, int> perPawn) =>
            Mathf.Clamp(Total(mentor) - baseline, 0, count) + " / " + count;
    }
}
