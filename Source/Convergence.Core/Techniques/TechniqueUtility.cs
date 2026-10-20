using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>Consultas de técnicas: requisitos, poder (reino × maestría × afinidad), coste y reparto a PNJ.</summary>
    [StaticConstructorOnStartup]
    public static class TechniqueUtility
    {
        /// <summary>Afinidad: el manual es de un elemento de la raíz del lanzador.</summary>
        public const float AffinityPowerFactor = 1.15f;
        public const float AffinityMasteryFactor = 1.5f;

        /// <summary>Carpeta de los iconos propios: UI/Techniques/&lt;defName&gt;.png (docs/ARTE.md).</summary>
        public const string IconFolder = "UI/Techniques/";

        private static readonly Dictionary<AbilityDef, TechniqueExtension> extCache = new Dictionary<AbilityDef, TechniqueExtension>();

        static TechniqueUtility()
        {
            // Icono propio si ya existe el PNG; si no, se queda el provisional del XML.
            int custom = 0;
            foreach (var def in DefDatabase<AbilityDef>.AllDefsListForReading)
            {
                var ext = Ext(def);
                if (ext == null) continue;
                var tex = ContentFinder<Texture2D>.Get(ext.iconPath ?? IconFolder + def.defName, reportFailure: false);
                if (tex == null) continue;
                def.uiIcon = tex;
                custom++;
            }
            if (custom > 0) Log.Message($"[Convergence] {custom} technique icon(s) loaded from {IconFolder}.");
        }

        public static TechniqueExtension Ext(AbilityDef def)
        {
            if (def == null) return null;
            if (!extCache.TryGetValue(def, out var ext))
                extCache[def] = ext = def.GetModExtension<TechniqueExtension>();
            return ext;
        }

        public static bool IsTechnique(AbilityDef def) => Ext(def) != null;

        public static bool HasElement(CompCultivator comp, QiElementDef element) =>
            element != null && comp?.Root?.elements != null && comp.Root.elements.Contains(element);

        /// <summary>Orden del reino actual en el camino (0 = no cultiva ese camino).</summary>
        public static int RealmOrder(CompCultivator comp, CultivationPathDef path)
        {
            if (comp == null || !comp.CanFollow(path)) return 0;
            var tracker = comp.TrackerFor(path);
            return tracker?.Realm?.order ?? 0;
        }

        /// <summary>Requisitos de uso y de desbloqueo (camino, reino, elemento). Los prerequisitos solo cuentan al desbloquear.</summary>
        public static bool RequirementsMet(Pawn pawn, AbilityDef def, out string reason)
        {
            reason = null;
            var ext = Ext(def);
            if (ext?.manual == null) return true;
            var comp = CompCultivator.For(pawn);
            var path = ext.manual.Path;
            if (comp == null || !comp.CanFollow(path))
            {
                reason = "CVG_Manual_WrongPath".Translate(pawn.Named("PAWN"), path.LabelCap);
                return false;
            }
            if (ext.minRealm != null && RealmOrder(comp, path) < ext.minRealm.order)
            {
                reason = "CVG_Technique_RequiresRealm".Translate(ext.minRealm.LabelCap);
                return false;
            }
            if (ext.minRealmOrder > 0 && RealmOrder(comp, path) < ext.minRealmOrder)
            {
                var realm = DefDatabase<CultivationRealmDef>.AllDefsListForReading.FirstOrDefault(r => r.path == path && r.order == ext.minRealmOrder);
                reason = "CVG_Technique_RequiresRealm".Translate(realm?.LabelCap ?? (TaggedString)("#" + ext.minRealmOrder));
                return false;
            }
            var element = ext.manual.element;
            if ((ext.requiresElement || ext.manual.requiresElement) && element != null && !HasElement(comp, element))
            {
                reason = "CVG_Manual_RequiresElement".Translate(element.label);
                return false;
            }
            return true;
        }

        public static bool HasAffinity(Pawn pawn, AbilityDef def)
        {
            var element = Ext(def)?.manual?.element;
            return element != null && HasElement(CompCultivator.For(pawn), element);
        }

        public static TechniqueMasteryDef Mastery(Pawn pawn, AbilityDef def) =>
            CompCultivator.For(pawn)?.Techniques.Mastery(def) ?? TechniqueMasteryDef.Ordered.FirstOrDefault();

        /// <summary>
        /// Poder de una técnica: reino × maestría × afinidad. Multiplica daño, escudos, duraciones… Las capacidades
        /// innatas (sin <see cref="TechniqueExtension"/>) usan solo el reino del qi (<see cref="AbilityPower"/>).
        /// </summary>
        public static float Power(Pawn pawn, AbilityDef def)
        {
            var ext = Ext(def);
            if (ext == null) return AbilityPower.For(pawn);
            var comp = CompCultivator.For(pawn);
            int realm = Mathf.Max(1, RealmOrder(comp, ext.manual?.Path ?? CVG_DefOf.CVG_Path_Qi));
            float power = ext.realmPowerCurve?.Evaluate(realm) ?? AbilityPower.ForRealmOrder(realm);
            var mastery = Mastery(pawn, def);
            if (mastery != null) power *= ext.masteryPowerCurve?.Evaluate(mastery.order) ?? mastery.powerFactor;
            if (HasAffinity(pawn, def)) power *= AffinityPowerFactor;
            return power;
        }

        /// <summary>Multiplicador del coste: stat <c>CVG_TechniqueCost</c> × maestría.</summary>
        public static float CostFactor(Pawn pawn, AbilityDef def)
        {
            float f = pawn.GetStatValue(CVG_DefOf.CVG_TechniqueCost);
            if (IsTechnique(def)) f *= Mastery(pawn, def)?.costFactor ?? 1f;
            return Mathf.Max(0f, f);
        }

        public static float CooldownFactor(Pawn pawn, AbilityDef def) =>
            IsTechnique(def) ? Mastery(pawn, def)?.cooldownFactor ?? 1f : 1f;

        /// <summary>
        /// Cultivadores generados (no del jugador): un manual afín a su raíz y técnicas según su reino, respetando
        /// prerequisitos. Reino 1 → 1 técnica, cada reino más → +1.
        /// </summary>
        public static void GiveNpcTechniques(Pawn pawn, List<TechniqueManualDef> preferred = null)
        {
            var comp = CompCultivator.For(pawn);
            if (comp == null || !comp.IsQiCultivator || pawn.abilities == null) return;
            var tracker = comp.Realm;
            if (!tracker.EnsureStarted()) return;
            var techniques = comp.Techniques;
            if (techniques.Unlocked.Count > 0) return;

            // Los caminos que abre su raíz, más (a veces) uno de tablilla; luego técnicas según el reino (VPE: 1 por punto).
            techniques.SyncOpenPaths();
            var wanted0 = preferred?.Where(m => m.CanLearn(pawn).Accepted).ToList();
            if (!wanted0.NullOrEmpty()) techniques.LearnManual(wanted0.RandomElement(), sendMessage: false, freeTechnique: false);
            else if (!techniques.AnyManual || Rand.Chance(0.3f))
            {
                var extra = DefDatabase<TechniqueManualDef>.AllDefsListForReading
                    .Where(m => m.npcCommonality > 0f && m.fromJadeSlips && m.CanLearn(pawn).Accepted).ToList();
                if (extra.Count > 0) techniques.LearnManual(extra.RandomElementByWeight(m => m.npcCommonality), sendMessage: false, freeTechnique: false);
            }
            if (!techniques.AnyManual) return;

            int wanted = TechniqueTracker.PointsForProgress(tracker.Realm, tracker.Stage);
            for (int guard = 0; guard < 40 && techniques.Unlocked.Count < wanted; guard++)
            {
                var next = techniques.Manuals.SelectMany(m => m.Techniques)
                    .Where(t => Ext(t).npcCommonality > 0f && techniques.CanUnlock(t, out _, ignorePoints: true))
                    // Las activas pesan más: un enemigo con solo pasivas no lanza nada.
                    .RandomElementByWeightWithFallback(t => Ext(t).npcCommonality * (Ext(t).IsPassive ? 1f : 2f));
                if (next == null) break;
                techniques.Unlock(next, free: true);
            }
            // Algo de maestría según el reino: los veteranos usan mejor sus técnicas.
            foreach (var t in techniques.Unlocked)
                techniques.AddMasteryXp(t, Rand.Range(0f, 150f) * wanted);
        }

        /// <summary>Etiqueta corta de coste para el gizmo ("12 qi").</summary>
        public static string CostLabel(float cost, CultivationPathDef path) => cost.ToString("0") + " " + path.resourceLabel;
    }
}
