using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Progreso de un colono en UN camino de cultivo: reino, etapa (1..N), progreso, recurso del camino y calidad del
    /// fundamento. Al llenar la última etapa queda en "cuello de botella": deja de progresar hasta superar el avance
    /// (<see cref="BreakthroughUtility"/>). Los reinos del camino se recorren por <see cref="CultivationRealmDef.order"/>.
    /// </summary>
    public class RealmTracker : IExposable
    {
        private CultivationPathDef path;
        private CultivationRealmDef realm;
        private int stage = 1;
        private float progress;
        private float foundationQuality = 1f;
        private int breakthroughCooldownUntil = -1;
        private CultivationResource resource;
        // Nació con este cultivo (cultivo previo, DISENO §4.1d): solo para la interfaz.
        private bool preCultivated;
        // Ritmo de progreso (media móvil por día) para estimar cuánto falta para la próxima etapa.
        private float gainToday, ratePerDay;
        private int rateDay = -1;

        public Pawn pawn;

        public RealmTracker()
        {
        }

        public RealmTracker(Pawn pawn, CultivationPathDef path)
        {
            this.pawn = pawn;
            this.path = path;
        }

        public CultivationPathDef Path => path;
        public CultivationRealmDef Realm => realm;
        public int Stage => stage;
        public float Progress => progress;
        /// <summary>Progreso medio por día de los últimos días (0 si todavía no hay datos).</summary>
        public float RatePerDay { get { RollRate(); return ratePerDay; } }
        /// <summary>Días estimados hasta llenar la etapa al ritmo actual (-1 si no se puede estimar).</summary>
        public float DaysToNextStage => realm == null || Bottleneck || RatePerDay <= 0.01f ? -1f : (realm.progressPerStage - progress) / ratePerDay;

        private void RollRate()
        {
            int day = Find.TickManager?.TicksGame / GenDate.TicksPerDay ?? 0;
            if (rateDay < 0) { rateDay = day; return; }
            while (rateDay < day)
            {
                ratePerDay = ratePerDay <= 0f ? gainToday : ratePerDay * 0.6f + gainToday * 0.4f;
                gainToday = 0f;
                rateDay++;
                if (day - rateDay > 10) { ratePerDay *= 0.2f; rateDay = day; }   // mucho tiempo sin datos
            }
        }
        public bool Started => realm != null;
        public bool PreCultivated => preCultivated;
        public float StagePercent => realm == null ? 0f : Mathf.Clamp01(progress / realm.progressPerStage);
        public bool Bottleneck => realm != null && stage >= realm.stages && progress >= realm.progressPerStage;

        /// <summary>Calidad del fundamento del reino actual (1 = normal; menos = inestable). Escala los stats del hediff del reino.</summary>
        public float FoundationQuality => foundationQuality;

        /// <summary>Multiplicador de vida del reino y etapa actuales, escalado por la calidad del fundamento.</summary>
        public float HealthScaleFactor => realm == null ? 1f : Mathf.Max(0.1f, 1f + (realm.HealthScaleAtStage(stage) - 1f) * foundationQuality);

        /// <summary>Recurso del camino (qi, sangre-qi…).</summary>
        public CultivationResource Resource
        {
            get
            {
                if (resource == null) resource = path.MakeResource(pawn);
                resource.pawn = pawn;
                resource.path = path;
                return resource;
            }
        }

        /// <summary>Reino siguiente del mismo camino, o null si ya está en el último.</summary>
        public CultivationRealmDef NextRealm => realm == null ? null : NextAfter(realm);

        /// <summary>Ticks que faltan para poder reintentar el avance tras un fallo.</summary>
        public int BreakthroughCooldownTicksLeft =>
            breakthroughCooldownUntil < 0 ? 0 : Mathf.Max(0, breakthroughCooldownUntil - Find.TickManager.TicksGame);

        public static CultivationRealmDef FirstRealmOf(CultivationPathDef path) =>
            DefDatabase<CultivationRealmDef>.AllDefsListForReading.Where(r => r.path == path).OrderBy(r => r.order).FirstOrDefault();

        public static CultivationRealmDef NextAfter(CultivationRealmDef current) =>
            DefDatabase<CultivationRealmDef>.AllDefsListForReading
                .Where(r => r.path == current.path && r.order > current.order)
                .OrderBy(r => r.order)
                .FirstOrDefault();

        /// <summary>Entra al primer reino del camino si todavía no cultivaba. Devuelve false si el camino no tiene reinos.</summary>
        public bool EnsureStarted()
        {
            if (realm != null) return true;
            realm = FirstRealmOf(path);
            stage = 1;
            progress = 0f;
            foundationQuality = 1f;
            SyncHediff();
            return realm != null;
        }

        /// <summary>Pasa a <paramref name="newRealm"/> en la etapa 1 con progreso 0 (avance de reino).</summary>
        public void AdvanceTo(CultivationRealmDef newRealm, float quality)
        {
            if (newRealm == null) return;
            RemoveHediff();
            realm = newRealm;
            stage = 1;
            progress = 0f;
            foundationQuality = quality;
            breakthroughCooldownUntil = -1;
            SyncHediff();
            CompCultivator.For(pawn)?.Techniques.Notify_Progressed(newRealm: true);
        }

        /// <summary>Fija la etapa dentro del reino actual (progreso 0). Para pawns generados con un reino ya hecho.</summary>
        public void SetStage(int newStage)
        {
            if (realm == null) return;
            stage = Mathf.Clamp(newStage, 1, realm.stages);
            progress = 0f;
            SyncHediff();
        }

        /// <summary>
        /// Cultivo previo al generar el pawn: reino, etapa y fundamento. Si todavía no puede seguir el camino (Convergencia
        /// pendiente) queda latente: sin hediff hasta que SyncRealm lo active.
        /// </summary>
        public void SetPreCultivated(CultivationRealmDef newRealm, int newStage, float quality, bool activate)
        {
            if (newRealm == null) return;
            if (activate) RemoveHediff();
            realm = newRealm;
            stage = Mathf.Clamp(newStage, 1, newRealm.stages);
            progress = Rand.Range(0f, newRealm.progressPerStage * 0.8f);
            foundationQuality = quality;
            preCultivated = true;
            if (activate) SyncHediff();
            else NotifyPowerChanged();
        }

        /// <summary>Tras un avance fallido: no se puede reintentar durante <paramref name="ticks"/> ticks.</summary>
        public void StartBreakthroughCooldown(int ticks)
        {
            breakthroughCooldownUntil = Find.TickManager.TicksGame + Mathf.Max(0, ticks);
        }

        /// <summary>Suma progreso; sube de etapa al llenarla. Devuelve el progreso realmente sumado.</summary>
        public float AddProgress(float amount)
        {
            if (!EnsureStarted() || amount <= 0f || Bottleneck) return 0f;

            RollRate();
            float added = 0f;
            while (amount > 0f && !Bottleneck)
            {
                float room = realm.progressPerStage - progress;
                float step = Mathf.Min(room, amount);
                progress += step;
                added += step;
                amount -= step;

                if (progress >= realm.progressPerStage)
                {
                    if (stage < realm.stages)
                    {
                        stage++;
                        progress = 0f;
                        SyncHediff();
                        CompCultivator.For(pawn)?.Techniques.Notify_Progressed(newRealm: false);
                        SoundCues.ForPlayerPawn("CVG_Cue_StageReached", pawn);
                        Notify("CVG_StageReached".Translate(pawn.LabelShortCap, realm.LabelCap, stage, pawn.Named("PAWN")),
                            MessageTypeDefOf.PositiveEvent);
                    }
                    else
                    {
                        progress = realm.progressPerStage;
                        Notify("CVG_BottleneckReached".Translate(pawn.LabelShortCap, realm.LabelCap, pawn.Named("PAWN")),
                            MessageTypeDefOf.NeutralEvent);
                    }
                }
            }
            gainToday += added;
            return added;
        }

        /// <summary>Pone el hediff del reino con la severidad de la etapa actual (lo agrega si falta).</summary>
        public void SyncHediff()
        {
            NotifyPowerChanged();
            if (realm?.hediff == null || pawn?.health == null) return;
            RemoveOtherRealmHediffs();
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(realm.hediff);
            if (hediff == null)
            {
                hediff = HediffMaker.MakeHediff(realm.hediff, pawn);
                hediff.Severity = stage;
                pawn.health.AddHediff(hediff);
            }
            else if (!Mathf.Approximately(hediff.Severity, stage))
            {
                hediff.Severity = stage;
            }
        }

        /// <summary>Quita el hediff del reino (p. ej. si el colono perdió la raíz).</summary>
        public void RemoveHediff()
        {
            NotifyPowerChanged();
            if (realm?.hediff == null || pawn?.health == null) return;
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(realm.hediff);
            if (hediff != null) pawn.health.RemoveHediff(hediff);
        }

        /// <summary>Quita los hediffs de otros reinos del mismo camino (partidas viejas, acciones de debug, avances).</summary>
        private void RemoveOtherRealmHediffs()
        {
            var realms = DefDatabase<CultivationRealmDef>.AllDefsListForReading;
            for (int i = 0; i < realms.Count; i++)
            {
                var other = realms[i];
                if (other == realm || other.path != path || other.hediff == null || other.hediff == realm.hediff) continue;
                var h = pawn.health.hediffSet.GetFirstHediffOfDef(other.hediff);
                if (h != null) pawn.health.RemoveHediff(h);
            }
        }

        /// <summary>Recalcula la vida máxima cacheada (al subir de etapa/reino o perder el reino). No cura ni hiere.</summary>
        private void NotifyPowerChanged()
        {
            if (pawn == null) return;
            CompCultivator.For(pawn)?.InvalidateHealthScale();
        }

        private void Notify(TaggedString text, MessageTypeDef type)
        {
            if (pawn.Faction == Faction.OfPlayer && PawnUtility.ShouldSendNotificationAbout(pawn))
                Messages.Message(text, pawn, type);
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref path, "path");
            Scribe_Defs.Look(ref realm, "realm");
            Scribe_Values.Look(ref stage, "stage", 1);
            Scribe_Values.Look(ref progress, "progress");
            Scribe_Values.Look(ref foundationQuality, "foundationQuality", 1f);
            Scribe_Values.Look(ref breakthroughCooldownUntil, "breakthroughCooldownUntil", -1);
            Scribe_Values.Look(ref preCultivated, "preCultivated");
            Scribe_Values.Look(ref gainToday, "gainToday");
            Scribe_Values.Look(ref ratePerDay, "ratePerDay");
            Scribe_Values.Look(ref rateDay, "rateDay", -1);
            Scribe_Deep.Look(ref resource, "resource");
            if (Scribe.mode == LoadSaveMode.PostLoadInit) FixAfterLoad();
        }

        /// <summary>Corrige datos tras cargar (las partidas de M3 no tenían camino).</summary>
        public void FixAfterLoad()
        {
            path ??= realm?.path ?? CVG_DefOf.CVG_Path_Qi;
            if (realm != null)
            {
                stage = Mathf.Clamp(stage, 1, realm.stages);
                progress = Mathf.Clamp(progress, 0f, realm.progressPerStage);
            }
        }

        /// <summary>Partidas de M3: el qi se guardaba aparte en el comp.</summary>
        public void SetLegacyResource(CultivationResource legacy)
        {
            if (legacy == null || resource != null) return;
            legacy.path = path;
            resource = legacy;
        }
    }
}
