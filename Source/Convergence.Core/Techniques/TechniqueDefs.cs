using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Manual de técnicas (功法): un árbol de técnicas de un camino de cultivo y, opcionalmente, de un elemento.
    /// Como los "caminos" de Vanilla Psycasts Expanded, el manual no lista sus técnicas: cada <see cref="AbilityDef"/>
    /// apunta a su manual con <see cref="TechniqueExtension.manual"/>, así otro mod agrega técnicas a un manual ajeno
    /// solo con XML. Se aprende leyendo una tablilla de jade (<see cref="CompUseEffect_LearnTechniqueManual"/>).
    /// </summary>
    public class TechniqueManualDef : Def
    {
        /// <summary>Camino que lo puede estudiar (y cuyo recurso pagan sus técnicas por defecto).</summary>
        public CultivationPathDef path;
        /// <summary>Elemento del manual (opcional). Una raíz con ese elemento rinde más y gana maestría más rápido.</summary>
        public QiElementDef element;
        /// <summary>Solo lo puede aprender una raíz con <see cref="element"/>.</summary>
        public bool requiresElement;
        /// <summary>Reino mínimo para aprender el manual (opcional).</summary>
        public CultivationRealmDef minRealm;
        public int order;
        /// <summary>Icono del manual (opcional). Si falta, el de la primera técnica.</summary>
        public string iconPath;
        /// <summary>Peso al repartir manuales a cultivadores generados (0 = nunca).</summary>
        public float npcCommonality = 1f;
        /// <summary>Investigación que necesitan los colonos del jugador para leerlo (opcional).</summary>
        public ResearchProjectDef requiredResearch;

        // Cómo se abre el camino (modelo de VPE, ver docs/DISENO_CORE.md): los elementales los abre la raíz espiritual;
        // los extra (Espada, Alma…) se aprenden con tablillas; otros, por seguir un camino de cultivo o por un gen.
        /// <summary>Lo abre una raíz con <see cref="element"/> (camino elemental).</summary>
        public bool openedByRoot;
        /// <summary>Lo abre seguir este camino de cultivo (camino del Cuerpo para los templadores).</summary>
        public CultivationPathDef openedByFollowing;
        /// <summary>Lo abre tener alguno de estos genes activos (p. ej. el Demonio de Sangre).</summary>
        public List<GeneDef> openedByGenes;
        /// <summary>Se puede enseñar con tablillas de jade al azar (los que no se abren solos).</summary>
        public bool fromJadeSlips = true;
        /// <summary>Niveles del camino (VPE usa 5).</summary>
        public int levels = 5;
        /// <summary>Arte vertical de la carta del camino (UI/Paths/...). Si falta: degradado del color del elemento y su glifo.</summary>
        public string cardTexPath;

        [Unsaved] private Texture2D cardTex;
        [Unsaved] private bool cardTexResolved;

        public Texture2D CardTex
        {
            get
            {
                if (!cardTexResolved)
                {
                    cardTexResolved = true;
                    if (!cardTexPath.NullOrEmpty()) cardTex = ContentFinder<Texture2D>.Get(cardTexPath, reportFailure: false);
                }
                return cardTex;
            }
        }

        /// <summary>Si el camino se abre solo para este colono (raíz, camino de cultivo o gen).</summary>
        public bool OpensNaturallyFor(CompCultivator comp)
        {
            if (comp == null) return false;
            if (openedByRoot && element != null && TechniqueUtility.HasElement(comp, element) && comp.CanFollow(Path)) return true;
            if (openedByFollowing != null && comp.CanFollow(openedByFollowing) && (comp.TrackerFor(openedByFollowing)?.Started ?? false))
                return true;
            var genes = comp.Pawn?.genes;
            if (!openedByGenes.NullOrEmpty() && genes != null && openedByGenes.Any(g => genes.HasActiveGene(g))) return true;
            return false;
        }

        /// <summary>Texto de cómo se abre (para los caminos cerrados de la ventana).</summary>
        public string HowToOpen
        {
            get
            {
                if (openedByRoot && element != null) return "CVG_Path_OpenByRoot".Translate(element.label);
                if (openedByFollowing != null) return "CVG_Path_OpenByFollowing".Translate(openedByFollowing.label);
                if (!openedByGenes.NullOrEmpty()) return "CVG_Path_OpenByGene".Translate(openedByGenes.Select(g => g.label).ToCommaListOr());
                return "CVG_Path_OpenByManual".Translate();
            }
        }

        [Unsaved] private List<AbilityDef> techniques;
        [Unsaved] private Texture2D icon;

        public CultivationPathDef Path => path ?? CVG_DefOf.CVG_Path_Qi;

        public Color Color => element?.color ?? Path.resourceColor;

        /// <summary>Técnicas del manual ordenadas por fila (<see cref="TechniqueExtension.tier"/>) y orden.</summary>
        public List<AbilityDef> Techniques
        {
            get
            {
                if (techniques == null)
                {
                    techniques = DefDatabase<AbilityDef>.AllDefsListForReading
                        .Where(a => a.GetModExtension<TechniqueExtension>()?.manual == this)
                        .OrderBy(a => a.GetModExtension<TechniqueExtension>().tier)
                        .ThenBy(a => a.GetModExtension<TechniqueExtension>().order)
                        .ToList();
                }
                return techniques;
            }
        }

        public int MaxTier => Mathf.Max(levels, Techniques.Count == 0 ? 0 : Techniques.Max(t => TechniqueUtility.Ext(t).tier));

        public Texture2D Icon
        {
            get
            {
                if (icon == null)
                {
                    if (!iconPath.NullOrEmpty()) icon = ContentFinder<Texture2D>.Get(iconPath, reportFailure: false);
                    if (icon == null && Techniques.Count > 0) icon = Techniques[0].uiIcon;
                    if (icon == null) icon = BaseContent.BadTex;
                }
                return icon;
            }
        }

        /// <summary>Si el colono puede aprender este manual; si no, el motivo.</summary>
        public AcceptanceReport CanLearn(Pawn pawn)
        {
            var comp = CompCultivator.For(pawn);
            if (comp == null || !comp.CanFollow(Path)) return "CVG_Manual_WrongPath".Translate(pawn.Named("PAWN"), Path.LabelCap);
            if (comp.Techniques.KnowsManual(this)) return "CVG_Manual_AlreadyKnown".Translate(pawn.Named("PAWN"), LabelCap);
            if (requiresElement && element != null && !TechniqueUtility.HasElement(comp, element))
                return "CVG_Manual_RequiresElement".Translate(element.label);
            if (minRealm != null && TechniqueUtility.RealmOrder(comp, Path) < minRealm.order)
                return "CVG_Technique_RequiresRealm".Translate(minRealm.LabelCap);
            if (requiredResearch != null && !requiredResearch.IsFinished && (pawn.Faction?.IsPlayer ?? false))
                return "CVG_Manual_RequiresResearch".Translate(requiredResearch.LabelCap);
            return true;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var e in base.ConfigErrors()) yield return e;
            if (Techniques.Count == 0) yield return "manual has no techniques (no AbilityDef with TechniqueExtension.manual = " + defName + ")";
        }
    }

    /// <summary>
    /// Nivel de maestría de una técnica (principiante → incomparable). Sube con el uso. Mejora el poder (daño, escudo,
    /// duración), baja el coste y el enfriamiento. Ampliable con XML: los niveles se ordenan por <see cref="order"/>.
    /// </summary>
    public class TechniqueMasteryDef : Def
    {
        public int order;
        /// <summary>Experiencia total de la técnica para llegar a este nivel.</summary>
        public float xpRequired;
        public float powerFactor = 1f;
        public float costFactor = 1f;
        public float cooldownFactor = 1f;
        public Color color = Color.white;

        private static List<TechniqueMasteryDef> ordered;

        public static List<TechniqueMasteryDef> Ordered =>
            ordered ??= DefDatabase<TechniqueMasteryDef>.AllDefsListForReading.OrderBy(m => m.order).ToList();

        public static TechniqueMasteryDef ForXp(float xp)
        {
            var list = Ordered;
            TechniqueMasteryDef result = list.Count > 0 ? list[0] : null;
            for (int i = 0; i < list.Count; i++)
                if (xp >= list[i].xpRequired) result = list[i];
            return result;
        }

        public TechniqueMasteryDef Next
        {
            get
            {
                var list = Ordered;
                int i = list.IndexOf(this);
                return i >= 0 && i + 1 < list.Count ? list[i + 1] : null;
            }
        }
    }

    /// <summary>
    /// Convierte un <see cref="AbilityDef"/> vanilla en técnica de cultivo: manual, fila del árbol, requisitos y escalado.
    /// El efecto lo ponen los comps genéricos (proyectil, hediff, cono…), el coste <see cref="CompProperties_AbilityResourceCost"/>.
    /// </summary>
    public class TechniqueExtension : DefModExtension
    {
        public TechniqueManualDef manual;
        /// <summary>Fila del árbol (1 = la primera).</summary>
        public int tier = 1;
        /// <summary>Posición dentro de la fila.</summary>
        public int order;
        /// <summary>Reino mínimo del camino del manual para desbloquearla (y usarla).</summary>
        public CultivationRealmDef minRealm;
        /// <summary>Alternativa a <see cref="minRealm"/> por número de reino (para niveles cuyo reino aún no existe).</summary>
        public int minRealmOrder;
        /// <summary>Técnicas previas: basta con tener CUALQUIERA de ellas (patrón de VPE).</summary>
        public List<AbilityDef> prerequisites;
        /// <summary>Requiere una raíz con el elemento del manual.</summary>
        public bool requiresElement;
        /// <summary>Puntos de comprensión para desbloquearla.</summary>
        public int unlockCost = 1;
        /// <summary>Experiencia de maestría por uso (×1.5 con afinidad elemental).</summary>
        public float masteryXpPerUse = 10f;
        /// <summary>Reino (order) → multiplicador de poder. Si falta, la curva general de <see cref="AbilityPower"/>.</summary>
        public SimpleCurve realmPowerCurve;
        /// <summary>Nivel de maestría (order) → multiplicador de poder. Si falta, <see cref="TechniqueMasteryDef.powerFactor"/>.</summary>
        public SimpleCurve masteryPowerCurve;
        /// <summary>Peso al repartir técnicas a cultivadores generados (0 = nunca).</summary>
        public float npcCommonality = 1f;
        /// <summary>Icono propio opcional. Por defecto se busca UI/Techniques/&lt;defName&gt; (ver docs/ARTE.md).</summary>
        public string iconPath;
        /// <summary>
        /// Técnica pasiva: desbloquearla pone este hediff permanente (su severidad = poder de la técnica, ver
        /// <see cref="HediffComp_PassiveTechnique"/>) en vez de dar una habilidad con botón.
        /// </summary>
        public HediffDef passiveHediff;
        /// <summary>Maestría que gana una técnica pasiva por día (práctica).</summary>
        public float masteryXpPerDay = 6f;

        public bool IsPassive => passiveHediff != null;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var e in base.ConfigErrors()) yield return e;
            if (manual == null) yield return "TechniqueExtension.manual is null";
            if (tier < 1) yield return "TechniqueExtension.tier must be >= 1";
        }
    }
}
