using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Técnicas de un colono (va dentro de <see cref="CompCultivator"/>): manuales conocidos, técnicas desbloqueadas,
    /// experiencia de maestría por técnica, puntos de comprensión para desbloquear y qué técnicas se ven en la barra.
    /// Las técnicas desbloqueadas son habilidades vanilla en <c>pawn.abilities</c>; este tracker es la fuente de verdad
    /// y las vuelve a dar si faltan (al cargar, tras editores de personajes…).
    /// </summary>
    public class TechniqueTracker : IExposable
    {

        public Pawn pawn;

        private List<TechniqueManualDef> manuals = new List<TechniqueManualDef>();
        private List<AbilityDef> unlocked = new List<AbilityDef>();
        // Partidas anteriores a los conjuntos: las técnicas fijadas pasan al primer conjunto al cargar.
        private List<AbilityDef> pinned;
        private List<TechniqueSet> sets = new List<TechniqueSet>();
        private int activeSet;
        private Dictionary<AbilityDef, float> masteryXp = new Dictionary<AbilityDef, float>();
        private int points;
        // Los puntos de las etapas ya alcanzadas se dan una vez (partidas de M4 y colonos generados).
        private bool pointsInitialized;

        // Para Scribe del diccionario.
        private List<AbilityDef> tmpKeys;
        private List<float> tmpValues;

        public TechniqueTracker()
        {
        }

        public TechniqueTracker(Pawn pawn)
        {
            this.pawn = pawn;
        }

        public List<TechniqueManualDef> Manuals => manuals;
        public List<AbilityDef> Unlocked => unlocked;
        public int Points => points;
        public bool AnyManual => manuals.Count > 0;

        public bool KnowsManual(TechniqueManualDef manual) => manuals.Contains(manual);
        public bool IsUnlocked(AbilityDef def) => unlocked.Contains(def);
        /// <summary>Técnica activa equipada en una ranura (su gizmo se ve con el colono reclutado).</summary>
        /// <summary>Técnica en el conjunto de combate activo (su gizmo se ve con el colono reclutado).</summary>
        public bool IsEquipped(AbilityDef def) => ActiveSet?.techniques.Contains(def) ?? false;
        public int EquippedCount => ActiveSet?.techniques.Count ?? 0;
        /// <summary>Técnicas activas que caben en un conjunto (3, y 4 desde la Fundación).</summary>
        public int ActiveSlots => PassiveTechniqueUtility.ActiveSlots(pawn);

        /// <summary>Conjuntos de combate (como los psysets de VPE): solo el activo tiene gizmos, y solo reclutado.</summary>
        public List<TechniqueSet> Sets => sets;
        public int ActiveSetIndex => activeSet;
        public TechniqueSet ActiveSet => sets.Count == 0 ? null : sets[Mathf.Clamp(activeSet, 0, sets.Count - 1)];

        public void SetActiveSet(int index)
        {
            if (index >= 0 && index < sets.Count) activeSet = index;
        }

        public TechniqueSet AddSet(string name, TechniqueManualDef manual = null)
        {
            var set = new TechniqueSet { name = name, manual = manual };
            sets.Add(set);
            return set;
        }

        public void RemoveSet(TechniqueSet set)
        {
            int i = sets.IndexOf(set);
            if (i < 0) return;
            sets.RemoveAt(i);
            if (activeSet >= sets.Count) activeSet = Mathf.Max(0, sets.Count - 1);
        }

        /// <summary>Pone o quita una técnica activa de un conjunto. Falla si es pasiva, no está aprendida o el conjunto está lleno.</summary>
        public bool SetInSet(TechniqueSet set, AbilityDef def, bool value)
        {
            if (set == null) return false;
            if (!value) return set.techniques.Remove(def);
            if (set.techniques.Contains(def)) return true;
            if (!IsUnlocked(def) || PassiveTechniqueUtility.IsPassive(def) || set.techniques.Count >= ActiveSlots) return false;
            set.techniques.Add(def);
            return true;
        }

        /// <summary>
        /// Como en VPE, abrir un camino cuesta 1 punto. Lo pueden abrir: la raíz con su elemento, seguir su camino de
        /// cultivo o un gen (<see cref="TechniqueManualDef.OpensNaturallyFor"/>). Los de tablilla se abren al leerla.
        /// </summary>
        public bool CanOpenPath(TechniqueManualDef m, out string reason, bool ignorePoints = false)
        {
            reason = null;
            if (manuals.Contains(m)) return false;
            if (!m.OpensNaturallyFor(CompCultivator.For(pawn)))
            {
                reason = m.HowToOpen;
                return false;
            }
            if (!ignorePoints && points < 1)
            {
                reason = "CVG_Path_NoPoints".Translate();
                return false;
            }
            return true;
        }

        public void OpenPath(TechniqueManualDef m, bool free = false)
        {
            if (manuals.Contains(m)) return;
            if (!free) points = Mathf.Max(0, points - 1);
            LearnManual(m, sendMessage: false, freeTechnique: false);
        }

        /// <summary>
        /// Los cultivadores que no son del jugador abren solos los caminos de su naturaleza (raíz, camino, gen); los del
        /// jugador los desbloquean en la ventana Desarrollo. Crea el conjunto de combate de cada camino abierto.
        /// </summary>
        public void SyncOpenPaths()
        {
            var comp = CompCultivator.For(pawn);
            if (comp == null) return;
            if (!(pawn.Faction?.IsPlayer ?? false))
                foreach (var m in DefDatabase<TechniqueManualDef>.AllDefsListForReading)
                    if (!manuals.Contains(m) && m.OpensNaturallyFor(comp)) LearnManual(m, sendMessage: false, freeTechnique: false);
            foreach (var m in manuals)
                if (m.Techniques.Any(t => !PassiveTechniqueUtility.IsPassive(t)) && !sets.Any(s => s.manual == m))
                    AddSet(m.LabelCap, m);
        }

        public float MasteryXp(AbilityDef def) => masteryXp.TryGetValue(def, out float xp) ? xp : 0f;
        public TechniqueMasteryDef Mastery(AbilityDef def) => TechniqueMasteryDef.ForXp(MasteryXp(def));

        /// <summary>
        /// Da los puntos de las etapas y reinos ya alcanzados (una sola vez): 1 al empezar, +1 por etapa, +2 por reino.
        /// </summary>
        public void InitializePoints(CompCultivator comp)
        {
            if (pointsInitialized) return;
            // Un solo fondo de puntos (como el nivel de VPE) con lo alcanzado en cada camino que sigue (qi, cuerpo…).
            var started = comp.ActiveTrackers.Where(t => t.Started).ToList();
            if (started.Count == 0) return;
            pointsInitialized = true;
            foreach (var tracker in started) points += PointsForProgress(tracker.Realm, tracker.Stage);
        }

        /// <summary>Puntos que corresponden a haber llegado a <paramref name="realm"/>, etapa <paramref name="stage"/>.</summary>
        public static int PointsForProgress(CultivationRealmDef realm, int stage)
        {
            if (realm == null) return 0;
            var first = RealmTracker.FirstRealmOf(realm.path);
            int realmsAbove = first == null ? 0 : Mathf.Max(0, realm.order - first.order);
            return 1 + realmsAbove * 2 + Mathf.Max(0, stage - 1);
        }

        public void AddPoints(int amount)
        {
            points = Mathf.Max(0, points + amount);
        }

        /// <summary>Etapa nueva (+1) o reino nuevo (+2).</summary>
        public void Notify_Progressed(bool newRealm)
        {
            if (!pointsInitialized) return;
            int gained = newRealm ? 2 : 1;
            points += gained;
            if (AnyManual && (pawn.Faction?.IsPlayer ?? false) && PawnUtility.ShouldSendNotificationAbout(pawn))
                Messages.Message("CVG_Technique_PointsGained".Translate(pawn.Named("PAWN"), gained, points), pawn, MessageTypeDefOf.PositiveEvent);
        }

        /// <summary>Aprende un manual y desbloquea gratis su primera técnica (si cumple los requisitos).</summary>
        public void LearnManual(TechniqueManualDef manual, bool sendMessage = true, bool freeTechnique = true)
        {
            if (manual == null || manuals.Contains(manual)) return;
            manuals.Add(manual);
            if (manual.Techniques.Any(t => !PassiveTechniqueUtility.IsPassive(t)) && !sets.Any(s => s.manual == manual))
                AddSet(manual.LabelCap, manual);
            var first = freeTechnique ? manual.Techniques.FirstOrDefault(t => CanUnlock(t, out _, ignorePoints: true)) : null;
            if (first != null) Unlock(first, free: true);
            if (sendMessage && (pawn.Faction?.IsPlayer ?? false))
                SoundCues.At("CVG_Cue_ManualLearned", pawn);
            if (sendMessage && (pawn.Faction?.IsPlayer ?? false))
                Messages.Message("CVG_Manual_Learned".Translate(pawn.Named("PAWN"), manual.LabelCap,
                        first != null ? first.LabelCap : (TaggedString)"-"), pawn, MessageTypeDefOf.PositiveEvent);
        }

        /// <summary>Si puede desbloquear la técnica ahora; si no, el motivo.</summary>
        public bool CanUnlock(AbilityDef def, out string reason, bool ignorePoints = false, bool ignoreManual = false)
        {
            var ext = TechniqueUtility.Ext(def);
            reason = null;
            if (ext == null) { reason = "not a technique"; return false; }
            if (IsUnlocked(def)) { reason = "CVG_Technique_AlreadyUnlocked".Translate(); return false; }
            if (!ignoreManual && !KnowsManual(ext.manual)) { reason = "CVG_Technique_ManualUnknown".Translate(ext.manual.LabelCap); return false; }
            if (!TechniqueUtility.RequirementsMet(pawn, def, out reason)) return false;
            if (!ext.prerequisites.NullOrEmpty() && !ext.prerequisites.Any(IsUnlocked))
            {
                reason = "CVG_Technique_RequiresPrevious".Translate(ext.prerequisites.Select(p => p.label).ToCommaListOr());
                return false;
            }
            if (!ignorePoints && points < ext.unlockCost)
            {
                reason = "CVG_Technique_NotEnoughPoints".Translate(ext.unlockCost, points);
                return false;
            }
            return true;
        }

        /// <summary>Desbloquea la técnica (gasta puntos salvo <paramref name="free"/>) y da la habilidad.</summary>
        public void Unlock(AbilityDef def, bool free = false)
        {
            if (def == null || IsUnlocked(def)) return;
            var ext = TechniqueUtility.Ext(def);
            if (!free && ext != null) points = Mathf.Max(0, points - ext.unlockCost);
            if (ext?.manual != null && !manuals.Contains(ext.manual)) manuals.Add(ext.manual);
            unlocked.Add(def);
            if (ext != null && ext.IsPassive) AddPassive(def);
            else
            {
                // Al conjunto de su camino (si cabe) y, si no hay conjuntos, al primero.
                var set = sets.FirstOrDefault(s => s.manual == ext?.manual) ?? (sets.Count == 0 ? AddSet(ext?.manual?.LabelCap.ToString() ?? "1", ext?.manual) : null);
                if (set != null && set.techniques.Count < ActiveSlots) set.techniques.Add(def);
                pawn.abilities?.GainAbility(def);
            }
        }

        /// <summary>Equipa o quita una técnica activa. Falla si es pasiva o no quedan ranuras.</summary>
        public bool SetEquipped(AbilityDef def, bool value) => SetInSet(ActiveSet ?? AddSet("1"), def, value);

        private void AddPassive(AbilityDef def)
        {
            var hediff = TechniqueUtility.Ext(def)?.passiveHediff;
            if (hediff == null || pawn?.health == null || pawn.health.hediffSet.HasHediff(hediff)) return;
            pawn.health.AddHediff(hediff);
        }

        /// <summary>Suma experiencia de maestría; avisa al subir de nivel.</summary>
        public void AddMasteryXp(AbilityDef def, float amount)
        {
            if (def == null || amount <= 0f) return;
            var before = Mastery(def);
            masteryXp[def] = MasteryXp(def) + amount;
            var after = Mastery(def);
            if (after != before && after != null && (pawn.Faction?.IsPlayer ?? false) && PawnUtility.ShouldSendNotificationAbout(pawn))
                Messages.Message("CVG_Technique_MasteryUp".Translate(pawn.Named("PAWN"), def.LabelCap, after.LabelCap), pawn,
                    MessageTypeDefOf.PositiveEvent);
        }

        /// <summary>Sube la técnica al siguiente nivel de maestría (debug).</summary>
        public void AddMasteryLevel(AbilityDef def)
        {
            var next = Mastery(def)?.Next;
            if (next != null) AddMasteryXp(def, next.xpRequired - MasteryXp(def));
        }

        /// <summary>
        /// Vuelve a dar las habilidades de las técnicas activas y los hediffs de las pasivas que falten; quita la
        /// habilidad de las que ahora son pasivas (partidas anteriores) y las equipadas que sobren.
        /// </summary>
        public void SyncAbilities()
        {
            if (pawn?.abilities == null) return;
            for (int i = 0; i < unlocked.Count; i++)
            {
                var def = unlocked[i];
                if (PassiveTechniqueUtility.IsPassive(def))
                {
                    if (pawn.abilities.GetAbility(def) != null) pawn.abilities.RemoveAbility(def);
                    AddPassive(def);
                }
                else if (pawn.abilities.GetAbility(def) == null) pawn.abilities.GainAbility(def);
            }
            // Técnicas de partidas viejas que cambiaron de camino: su camino queda abierto.
            foreach (var def in unlocked)
            {
                var m = TechniqueUtility.Ext(def)?.manual;
                if (m != null && !manuals.Contains(m)) manuals.Add(m);
            }
            SyncOpenPaths();
            int slots = ActiveSlots;
            foreach (var set in sets)
            {
                set.techniques.RemoveAll(d => d == null || PassiveTechniqueUtility.IsPassive(d) || !IsUnlocked(d));
                if (set.techniques.Count > slots) set.techniques.RemoveRange(slots, set.techniques.Count - slots);
            }
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref manuals, "manuals", LookMode.Def);
            Scribe_Collections.Look(ref unlocked, "unlocked", LookMode.Def);
            if (Scribe.mode == LoadSaveMode.LoadingVars) Scribe_Collections.Look(ref pinned, "pinned", LookMode.Def);
            Scribe_Collections.Look(ref sets, "sets", LookMode.Deep);
            Scribe_Values.Look(ref activeSet, "activeSet");
            Scribe_Collections.Look(ref masteryXp, "masteryXp", LookMode.Def, LookMode.Value, ref tmpKeys, ref tmpValues);
            Scribe_Values.Look(ref points, "points");
            Scribe_Values.Look(ref pointsInitialized, "pointsInitialized");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                manuals ??= new List<TechniqueManualDef>();
                unlocked ??= new List<AbilityDef>();
                sets ??= new List<TechniqueSet>();
                sets.RemoveAll(s => s == null);
                if (!pinned.NullOrEmpty() && sets.Count == 0)
                {
                    pinned.RemoveAll(a => a == null);
                    var old = AddSet("CVG_Set_Combat".Translate());
                    old.techniques.AddRange(pinned);
                }
                pinned = null;
                masteryXp ??= new Dictionary<AbilityDef, float>();
                manuals.RemoveAll(m => m == null);
                unlocked.RemoveAll(a => a == null);
                masteryXp.RemoveAll(kv => kv.Key == null);
            }
        }
    }
}
