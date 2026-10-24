using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Componente de cultivo de cada humanoide. Se inyecta por código en <see cref="ConvergenceStartup"/>.
    /// La raíz espiritual y el cuerpo especial se leen de rasgos (sin Biotech) o genes (con Biotech):
    /// así funcionan la herencia, los editores de personajes y otros mods. El comp solo guarda los
    /// elementos de la raíz cuando viene de un rasgo (los rasgos no tienen elemento).
    /// Lleva un <see cref="RealmTracker"/> por cada camino de cultivo que sigue el colono (qi, cuerpo…).
    /// </summary>
    public class CompCultivator : ThingComp
    {
        // Búsqueda O(1) por thingIDNumber (evita GetComp en rutas calientes).
        private static readonly Dictionary<int, CompCultivator> cache = new Dictionary<int, CompCultivator>();

        private const int SyncInterval = 2000;
        private const int RegenInterval = 250;

        // Elementos elegidos para una raíz que viene de un rasgo. Se guardan.
        private List<QiElementDef> traitRootElements = new List<QiElementDef>();

        // Un tracker por camino de cultivo: la estructura admite doble cultivo.
        private List<RealmTracker> paths = new List<RealmTracker>();
        // Camino del avance en curso (lo lee JobDriver_Breakthrough).
        private CultivationPathDef breakthroughPath;
        // Partidas de M3: el qi se guardaba aparte; se pasa al tracker del qi al cargar.
        private StatResource legacyQi;

        // Multiplicador de vida de todos los caminos (lo lee el parche de Pawn.HealthScale; -1 = recalcular).
        private float healthScaleFactor = -1f;
        private float agingFactor = -1f;
        private int agingFactorTick;

        // Técnicas (M5): manuales, técnicas desbloqueadas, maestría y puntos.
        private TechniqueTracker techniques;
        // Reino más alto de los caminos que sigue (0 = mortal), para la supresión de reino (-1 = recalcular).
        private int realmLevel = -1;
        // Escudos de qi activos (los hediffs se registran solos): el daño los consulta sin recorrer los hediffs.
        private readonly List<HediffComp_QiShield> shields = new List<HediffComp_QiShield>();

        private readonly Dictionary<CultivationPathDef, Gizmo_ResourceBar> resourceGizmos = new Dictionary<CultivationPathDef, Gizmo_ResourceBar>();

        // Estado derivado (no se guarda: se recalcula desde rasgos/genes).
        private SpiritRoot root;
        private Def specialBody;
        private bool synced;

        // La sangre ancestral ya refinó la raíz (sube un grado una sola vez). Se guarda.
        private bool bloodRootRefined;

        public bool BloodRootRefined { get => bloodRootRefined; set => bloodRootRefined = value; }

        /// <summary>Elementos de una raíz que viene de un rasgo (sin Biotech), p. ej. la que despierta la sangre ancestral.</summary>
        public void SetTraitRootElements(List<QiElementDef> elements)
        {
            traitRootElements = new List<QiElementDef>(elements);
            synced = false;
        }

        public Pawn Pawn => parent as Pawn;

        public SpiritRoot Root { get { EnsureSynced(); return root; } }
        public bool HasRoot => Root != null;
        public Def SpecialBody { get { EnsureSynced(); return specialBody; } }

        /// <summary>La raíz (o el cuerpo) solo despierta cuando la Convergencia ya ocurrió.</summary>
        public bool IsAwakened => GameComponent_Convergence.Active && (HasRoot || SpecialBody != null);

        /// <summary>Sigue el camino del qi (tiene raíz y ya despertó).</summary>
        public bool IsQiCultivator => CanFollow(CVG_DefOf.CVG_Path_Qi);

        /// <summary>Sigue al menos un camino de cultivo (y lo empezó, si el camino no empieza solo).</summary>
        public bool IsCultivator => DefDatabase<CultivationPathDef>.AllDefsListForReading.Any(Follows);

        /// <summary>Templa el cuerpo: sigue el camino del cuerpo y ya lo empezó.</summary>
        public bool IsBodyCultivator => CVG_DefOf.CVG_Path_Body != null && Follows(CVG_DefOf.CVG_Path_Body);

        /// <summary>Puede templar el cuerpo ahora: tiene sangre-qi y no está en cuello de botella.</summary>
        public bool CanTemperNow
        {
            get
            {
                if (!IsBodyCultivator) return false;
                var tracker = TrackerFor(CVG_DefOf.CVG_Path_Body);
                return !tracker.Bottleneck && tracker.Resource.Current >= 1f;
            }
        }

        /// <summary>
        /// Puede sentarse a cultivar qi ahora: progresa en su etapa o, en cuello de botella, todavía le falta qi para
        /// intentar el avance (cultivar recupera el qi mucho más rápido).
        /// </summary>
        public bool CanCultivateNow => IsQiCultivator && (!Realm.Bottleneck || (Realm.NextRealm != null && !Qi.IsFull));

        /// <summary>Tracker del camino del qi (atajo para el código propio del qi: cultivar en el cojín, técnicas).</summary>
        public RealmTracker Realm => TrackerFor(CVG_DefOf.CVG_Path_Qi, create: true);

        /// <summary>Qi actual: el recurso del camino del qi.</summary>
        public CultivationResource Qi => Realm.Resource;

        /// <summary>Trackers de los caminos que el colono sigue ahora, en orden.</summary>
        private static List<CultivationPathDef> pathsByOrder;

        /// <summary>Caminos ordenados una sola vez (antes se ordenaban con LINQ en cada consulta, cada 250 ticks por humano).</summary>
        private static List<CultivationPathDef> PathsByOrder =>
            pathsByOrder ??= DefDatabase<CultivationPathDef>.AllDefsListForReading.OrderBy(p => p.order).ToList();

        public IEnumerable<RealmTracker> ActiveTrackers
        {
            get
            {
                var all = PathsByOrder;
                for (int i = 0; i < all.Count; i++)
                    if (Follows(all[i])) yield return TrackerFor(all[i], create: true);
            }
        }

        /// <summary>Caminos con cultivo guardado que todavía no puede seguir (cultivo previo antes de la Convergencia).</summary>
        public IEnumerable<RealmTracker> LatentTrackers => paths.Where(t => t.Started && !Follows(t.Path));

        /// <summary>Camino del avance en curso (por defecto, el del qi).</summary>
        public CultivationPathDef BreakthroughPath
        {
            get => breakthroughPath ?? CVG_DefOf.CVG_Path_Qi;
            set => breakthroughPath = value;
        }

        // Contadores de uso para los rasgos que evolucionan (TraitProgression).
        private TraitProgressTracker progress;

        public TraitProgressTracker Progress => progress ??= new TraitProgressTracker();

        /// <summary>Técnicas del colono si ya tiene (no crea el tracker: para la interfaz de cualquier humanoide).</summary>
        public TechniqueTracker TechniquesIfAny => techniques;

        /// <summary>Técnicas del colono (se crea al pedirla).</summary>
        public TechniqueTracker Techniques
        {
            get
            {
                if (techniques == null) techniques = new TechniqueTracker(Pawn);
                techniques.pawn = Pawn;
                return techniques;
            }
        }

        /// <summary>Nivel de reino para la supresión: el <c>order</c> del reino más alto de los caminos que sigue (0 = mortal).</summary>
        public int RealmLevel
        {
            get
            {
                if (realmLevel < 0)
                {
                    int level = 0;
                    for (int i = 0; i < paths.Count; i++)
                        if (paths[i].Started && CanFollow(paths[i].Path)) level = System.Math.Max(level, paths[i].Realm.order);
                    realmLevel = level;
                }
                return realmLevel;
            }
        }

        public void RegisterShield(HediffComp_QiShield shield)
        {
            if (!shields.Contains(shield)) shields.Add(shield);
        }

        public void UnregisterShield(HediffComp_QiShield shield) => shields.Remove(shield);

        /// <summary>
        /// Supresión de reino (el grado del ataque contra el reino del colono) y escudos de qi. Va antes de la armadura,
        /// los cinturones escudo y el multiplicador de daño recibido del reino.
        /// </summary>
        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            absorbed = false;
            if (dinfo.Def == null || !dinfo.Def.harmsHealth || dinfo.Def.consideredHelpful) return;

            // Desvío y resistencia a proyectiles (constituciones de secta).
            if (ProjectileDefense.Apply(Pawn, ref dinfo)) { absorbed = true; return; }

            // Debilidad a un material (Demonio de Sangre: plata lunar).
            float weakness = StuffWeaknessUtility.FactorFor(Pawn, dinfo);
            if (weakness != 1f) dinfo.SetAmount(dinfo.Amount * weakness);

            int level = RealmLevel;
            if (level > 0)
            {
                float factor = SpiritGradeUtility.SuppressionFactor(level, SpiritGradeUtility.AttackGrade(dinfo));
                if (factor < 1f) dinfo.SetAmount(dinfo.Amount * factor);
            }

            for (int i = shields.Count - 1; i >= 0; i--)
            {
                var shield = shields[i];
                if (shield.parent?.pawn != parent || shield.Broken) continue;
                if (shield.Absorb(ref dinfo))
                {
                    absorbed = true;
                    return;
                }
            }
        }

        /// <summary>Producto del multiplicador de vida de los caminos que sigue (cacheado: HealthScale se consulta mucho).</summary>
        /// <summary>
        /// Envejecimiento biológico (longevidad): el factor más bajo entre los caminos que sigue. Cacheado; se recalcula
        /// con la vida (etapa, reino) o cada día.
        /// </summary>
        public float AgingFactor
        {
            get
            {
                int now = Find.TickManager?.TicksGame ?? 0;
                if (agingFactor < 0f || now - agingFactorTick > GenDate.TicksPerDay)
                {
                    float f = 1f;
                    for (int i = 0; i < paths.Count; i++)
                    {
                        var t = paths[i];
                        if (t.Started && CanFollow(t.Path)) f = UnityEngine.Mathf.Min(f, t.Realm.AgingFactorAtStage(t.Stage));
                    }
                    agingFactor = f;
                    agingFactorTick = now;
                }
                return agingFactor;
            }
        }

        public float HealthScaleFactor
        {
            get
            {
                if (healthScaleFactor < 0f)
                {
                    float f = 1f;
                    for (int i = 0; i < paths.Count; i++)
                        if (paths[i].Started && CanFollow(paths[i].Path)) f *= paths[i].HealthScaleFactor;
                    healthScaleFactor = f;
                }
                return healthScaleFactor;
            }
        }

        /// <summary>
        /// La vida máxima cambió (etapa, reino, fundamento). Las heridas guardan daño absoluto, así que la salud de cada
        /// parte se recalcula sola: ni cura ni mata de golpe.
        /// </summary>
        public void InvalidateHealthScale()
        {
            healthScaleFactor = -1f;
            agingFactor = -1f;
            realmLevel = -1;
            var pawn = Pawn;
            if (pawn?.health == null) return;
            pawn.health.hediffSet?.DirtyCache();
            pawn.health.summaryHealth?.Notify_HealthChanged();
        }

        public bool CanFollow(CultivationPathDef path) => Pawn != null && !BlockedByHediff(path) && path.Worker.CanCultivate(Pawn, this);

        /// <summary>Un hediff con <see cref="DefModExtension_BlocksCultivation"/> (dantian dañado…) impide seguir el camino.</summary>
        private bool BlockedByHediff(CultivationPathDef path)
        {
            var hediffs = Pawn.health?.hediffSet?.hediffs;
            if (hediffs == null) return false;
            for (int i = 0; i < hediffs.Count; i++)
            {
                var ext = hediffs[i].def.GetModExtension<DefModExtension_BlocksCultivation>();
                if (ext != null && (ext.paths.NullOrEmpty() || ext.paths.Contains(path))) return true;
            }
            return false;
        }

        /// <summary>Puede seguir el camino y está en él (los caminos sin <c>autoStart</c> tienen que haberse empezado).</summary>
        public bool Follows(CultivationPathDef path)
        {
            if (!CanFollow(path)) return false;
            return path.autoStart || (TrackerFor(path)?.Started ?? false);
        }

        /// <summary>Empieza un camino que no empieza solo (comer carne de bestia…). Devuelve el tracker, o null si no puede.</summary>
        public RealmTracker StartPath(CultivationPathDef path)
        {
            if (path == null || !CanFollow(path)) return null;
            var tracker = TrackerFor(path, create: true);
            if (!tracker.Started)
            {
                if (!tracker.EnsureStarted()) return null;
                if (Pawn.Faction?.IsPlayer ?? false)
                    Messages.Message("CVG_PathStarted".Translate(Pawn.Named("PAWN"), path.LabelCap, tracker.Realm.LabelCap), Pawn,
                        MessageTypeDefOf.PositiveEvent);
            }
            return tracker;
        }

        public RealmTracker TrackerFor(CultivationPathDef path, bool create = false)
        {
            if (path == null) return null;
            for (int i = 0; i < paths.Count; i++)
                if (paths[i].Path == path) return paths[i];
            if (!create) return null;
            var tracker = new RealmTracker(Pawn, path);
            paths.Add(tracker);
            return tracker;
        }

        public static CompCultivator For(Pawn pawn)
        {
            if (pawn == null) return null;
            if (cache.TryGetValue(pawn.thingIDNumber, out var comp) && comp.parent == pawn) return comp;
            comp = pawn.GetComp<CompCultivator>();
            if (comp != null) cache[pawn.thingIDNumber] = comp;
            return comp;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            cache[parent.thingIDNumber] = this;
            Sync();
            BloodlineUtility.SyncEffects(Pawn);
            techniques?.SyncAbilities();
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            cache.Remove(parent.thingIDNumber);
        }

        public override void CompTickInterval(int delta)
        {
            base.CompTickInterval(delta);
            // Barato y poco frecuente: detecta rasgos/genes agregados por editores o cirugías.
            if (parent.IsHashIntervalTick(SyncInterval, delta))
            {
                Sync();
                SyncRealm();
                BloodlineUtility.SyncEffects(Pawn);
                // Caminos de técnicas que se abren solos (raíz, camino de cultivo, gen).
                if (IsQiCultivator || (TrackerFor(CVG_DefOf.CVG_Path_Body)?.Started ?? false)) Techniques.SyncOpenPaths();
                // Marionetas (Biotech): días-mecanoide supervisados suben el rasgo de maestro de marionetas.
                if (ModsConfig.BiotechActive && Pawn.mechanitor != null)
                {
                    int mechs = Pawn.mechanitor.OverseenPawns.Count;
                    if (mechs > 0) TraitProgression.Add(Pawn, "Puppetry", mechs * (float)SyncInterval / GenDate.TicksPerDay);
                }
                // Respirar el qi del mundo: una comprensión mínima para todo cultivador despierto.
                if (GameComponent_Convergence.Active && IsCultivator)
                    DaoComprehension.Gain(Pawn, 0.02f * SyncInterval / GenDate.TicksPerHour, DaoSource.Breathing);
            }
            if (parent.IsHashIntervalTick(RegenInterval, delta) && GameComponent_Convergence.Active)
            {
                foreach (var tracker in ActiveTrackers)
                    if (tracker.EnsureStarted()) tracker.Resource.TickInterval(RegenInterval);
            }
        }

        /// <summary>Entra al primer reino de cada camino que puede seguir y mantiene los hediffs de reino al día.</summary>
        public void SyncRealm()
        {
            var all = DefDatabase<CultivationPathDef>.AllDefsListForReading;
            for (int i = 0; i < all.Count; i++)
            {
                var path = all[i];
                if (Follows(path))
                {
                    var tracker = TrackerFor(path, create: true);
                    if (tracker.EnsureStarted()) tracker.SyncHediff();
                    if (tracker.Started) Techniques.InitializePoints(this);
                }
                else
                {
                    var tracker = TrackerFor(path);
                    if (tracker != null && tracker.Started) tracker.RemoveHediff();
                }
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var g in base.CompGetGizmosExtra()) yield return g;
            if (Pawn.Faction != Faction.OfPlayer) yield break;
            foreach (var tracker in ActiveTrackers)
            {
                if (!tracker.Started) continue;
                if (!resourceGizmos.TryGetValue(tracker.Path, out var bar))
                    resourceGizmos[tracker.Path] = bar = new Gizmo_ResourceBar(tracker);
                yield return bar;
                // Nada de avances en combate: el botón se esconde con el colono reclutado.
                if (tracker.Bottleneck && Pawn.IsColonistPlayerControlled && !Pawn.Drafted)
                {
                    CultivationGuide.Teach(CultivationGuide.Breakthrough, OpportunityType.Important);
                    yield return BreakthroughUtility.MakeGizmo(this, tracker);
                }
            }
            // Técnicas: abre la ventana de desarrollo (como el botón de la pestaña Cultivo).
            if (Pawn.IsColonistPlayerControlled && CultivationDevelopmentUtility.CanOpen(Pawn))
            {
                CultivationGuide.Teach(CultivationGuide.Techniques, OpportunityType.GoodToKnow);
                yield return CultivationDevelopmentUtility.MakeGizmo(Pawn);
            }
            // Conjuntos de combate (psysets de VPE): cambiar de conjunto, solo reclutado.
            if (techniques != null && Pawn.IsColonistPlayerControlled && Pawn.Drafted && techniques.Sets.Count > 1)
                yield return TechniqueSetUtility.MakeSwitchGizmo(this);
        }

        private void EnsureSynced()
        {
            if (!synced) Sync();
        }

        /// <summary>Recalcula raíz y cuerpo desde rasgos y genes.</summary>
        public void Sync()
        {
            synced = true;
            healthScaleFactor = -1f;
            realmLevel = -1;
            var pawn = Pawn;
            if (pawn == null)
            {
                root = null;
                specialBody = null;
                return;
            }

            SpiritRootGradeDef grade = null;
            var geneElements = new List<QiElementDef>();
            specialBody = null;

            if (pawn.genes != null)
            {
                foreach (var gene in pawn.genes.GenesListForReading)
                {
                    if (!gene.Active) continue;
                    var rootExt = gene.def.GetModExtension<DefModExtension_SpiritRoot>();
                    if (rootExt != null)
                    {
                        if (rootExt.grade != null && (grade == null || rootExt.grade.order > grade.order)) grade = rootExt.grade;
                        if (rootExt.element != null && !geneElements.Contains(rootExt.element)) geneElements.Add(rootExt.element);
                    }
                    if (specialBody == null && gene.def.HasModExtension<DefModExtension_SpecialBody>()) specialBody = gene.def;
                }
            }

            if (pawn.story?.traits != null)
            {
                foreach (var trait in pawn.story.traits.allTraits)
                {
                    if (trait.def == CVG_DefOf.CVG_SpiritRoot && grade == null)
                        grade = DefDatabase<SpiritRootGradeDef>.AllDefsListForReading.FirstOrDefault(g => g.traitDegree == trait.Degree);
                    if (specialBody == null && trait.def.HasModExtension<DefModExtension_SpecialBody>()) specialBody = trait.def;
                }
            }

            if (grade == null)
            {
                root = null;
                return;
            }

            var elements = geneElements;
            if (elements.Count == 0)
            {
                traitRootElements.RemoveAll(e => e == null);
                if (traitRootElements.Count == 0) traitRootElements = CultivationGenerator.RandomElements();
                elements = traitRootElements;
            }

            root = new SpiritRoot { grade = grade, elements = new List<QiElementDef>(elements) };
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref traitRootElements, "traitRootElements", LookMode.Def);
            Scribe_Values.Look(ref bloodRootRefined, "bloodRootRefined");
            Scribe_Collections.Look(ref paths, "paths", LookMode.Deep);
            Scribe_Defs.Look(ref breakthroughPath, "breakthroughPath");
            Scribe_Deep.Look(ref techniques, "techniques");
            Scribe_Deep.Look(ref progress, "traitProgress");

            // Partidas de M3: un solo reino ("realm") y el qi aparte ("qi").
            if (Scribe.mode == LoadSaveMode.LoadingVars && paths == null)
            {
                RealmTracker legacyRealm = null;
                Scribe_Deep.Look(ref legacyRealm, "realm");
                Scribe_Deep.Look(ref legacyQi, "qi");
                paths = new List<RealmTracker>();
                if (legacyRealm != null) paths.Add(legacyRealm);
            }

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                traitRootElements ??= new List<QiElementDef>();
                paths ??= new List<RealmTracker>();
                paths.RemoveAll(t => t == null);
                foreach (var t in paths)
                {
                    t.pawn = Pawn;
                    t.FixAfterLoad();
                }
                if (legacyQi != null)
                {
                    TrackerFor(CVG_DefOf.CVG_Path_Qi)?.SetLegacyResource(legacyQi);
                    legacyQi = null;
                }
                if (techniques != null) techniques.pawn = Pawn;
                synced = false;
                healthScaleFactor = -1f;
                realmLevel = -1;
            }
        }
    }

    /// <summary>
    /// Longevidad: los reinos frenan el envejecimiento biológico de los adultos (los niños crecen normal) multiplicando
    /// el mismo valor que usan los genes (<c>BiologicalTicksPerTick</c>). Compatible con el rejuvenecimiento de Biotech,
    /// que baja la edad por su cuenta, y con LifespanFactor (enfermedades de la edad), que ya dan los hediffs de reino.
    /// </summary>
    [HarmonyLib.HarmonyPatch(typeof(Pawn_AgeTracker), nameof(Pawn_AgeTracker.BiologicalTicksPerTick), HarmonyLib.MethodType.Getter)]
    public static class Patch_Pawn_AgeTracker_BiologicalTicksPerTick
    {
        public const float AdultAge = 20f;

        private static readonly HarmonyLib.AccessTools.FieldRef<Pawn_AgeTracker, Pawn> pawnRef =
            HarmonyLib.AccessTools.FieldRefAccess<Pawn_AgeTracker, Pawn>("pawn");

        public static void Postfix(Pawn_AgeTracker __instance, ref float __result)
        {
            var pawn = pawnRef(__instance);
            if (pawn?.RaceProps == null || !pawn.RaceProps.Humanlike || __instance.AgeBiologicalYearsFloat < AdultAge) return;
            var comp = CompCultivator.For(pawn);
            if (comp != null) __result *= comp.AgingFactor;
        }
    }

    /// <summary>Vida por reino: multiplica <c>Pawn.HealthScale</c> (salud máxima de las partes, sangrado, dolor por herida…).</summary>
    [HarmonyLib.HarmonyPatch(typeof(Pawn), nameof(Pawn.HealthScale), HarmonyLib.MethodType.Getter)]
    public static class Patch_Pawn_HealthScale
    {
        public static void Postfix(Pawn __instance, ref float __result)
        {
            var race = __instance.def.race;
            if (race == null) return;
            if (race.IsMechanoid)
            {
                // Marionetas (Biotech wuxia): más vida con el reino de su maestro.
                var puppet = __instance.GetComp<CompPuppet>();
                if (puppet != null) __result *= puppet.HealthFactor;
                return;
            }
            if (race.Animal)
            {
                // Bestias espirituales (M4b): vida según el nivel (caché O(1)).
                __result *= Hediff_SpiritBeast.HealthFactorOf(__instance);
                return;
            }
            if (!race.Humanlike) return;
            var comp = CompCultivator.For(__instance);
            if (comp != null) __result *= comp.HealthScaleFactor;
        }
    }

    public class CompProperties_Cultivator : CompProperties
    {
        public CompProperties_Cultivator()
        {
            compClass = typeof(CompCultivator);
        }
    }
}
