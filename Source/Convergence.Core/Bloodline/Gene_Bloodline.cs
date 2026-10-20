using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Clase de los genes de sangre con mecánicas propias (aura, regeneración, vidas extra, contraataque). Lee sus
    /// DefModExtension; todo funciona solo tras la Convergencia (los stats del gen, en cambio, aplican siempre).
    /// </summary>
    public class Gene_Bloodline : Gene
    {
        private const int AuraInterval = 250;
        private const int RegenCheckInterval = 2500;

        private int livesUsed;
        private int lastLifeTick = -1;
        private int lastRegenTick = -1;
        public int lastRetaliationTick = -99999;

        private static bool Awake => GameComponent_Convergence.Active;

        public int Tier => def.GetModExtension<DefModExtension_BloodlineGeneTier>()?.tier ?? 1;

        public int LivesLeft
        {
            get
            {
                var ext = def.GetModExtension<DefModExtension_ExtraLives>();
                if (ext == null) return 0;
                RechargeLives(ext);
                return Mathf.Max(0, ext.lives - livesUsed);
            }
        }

        public override void PostAdd()
        {
            base.PostAdd();
            lastRegenTick = Find.TickManager?.TicksGame ?? 0;
        }

        public override void TickInterval(int delta)
        {
            base.TickInterval(delta);
            if (!Awake || !Active || pawn.Dead) return;
            if (pawn.Spawned && pawn.IsHashIntervalTick(AuraInterval, delta)) DoAura();
            if (pawn.IsHashIntervalTick(RegenCheckInterval, delta)) DoRegeneration();
        }

        private void DoAura()
        {
            var ext = def.GetModExtension<DefModExtension_GeneAura>();
            if (ext?.hediff == null) return;
            float severity = ext.severity >= 0f ? ext.severity : Tier;
            foreach (var thing in GenRadial.RadialDistinctThingsAround(pawn.Position, pawn.Map, ext.radius, useCenter: true))
            {
                if (!(thing is Pawn other) || other.Dead) continue;
                if (other == pawn ? !ext.affectSelf : !Affects(ext, other)) continue;
                var h = other.health.hediffSet.GetFirstHediffOfDef(ext.hediff);
                if (h == null)
                {
                    h = HediffMaker.MakeHediff(ext.hediff, other);
                    h.Severity = severity;
                    other.health.AddHediff(h);
                }
                else if (h.Severity < severity)
                {
                    h.Severity = severity;
                }
                var disappears = h.TryGetComp<HediffComp_Disappears>();
                if (disappears != null) disappears.ticksToDisappear = Mathf.Max(disappears.ticksToDisappear, ext.lingerTicks);
            }
        }

        private bool Affects(DefModExtension_GeneAura ext, Pawn other)
        {
            if (ext.onlyEntities && !other.RaceProps.IsAnomalyEntity) return false;
            if (other.HostileTo(pawn)) return ext.affectHostiles;
            return ext.affectAllies && other.Faction != null && other.Faction == pawn.Faction;
        }

        private void DoRegeneration()
        {
            var ext = def.GetModExtension<DefModExtension_BloodRegeneration>();
            if (ext == null) return;
            int now = Find.TickManager.TicksGame;
            if (lastRegenTick < 0) lastRegenTick = now;
            if (now - lastRegenTick < ext.intervalDays * GenDate.TicksPerDay) return;

            var hediffs = pawn.health.hediffSet.hediffs;
            Hediff target = null;
            if (ext.regrowParts)
                target = pawn.health.hediffSet.GetMissingPartsCommonAncestors().FirstOrDefault();
            if (target == null && ext.healScars)
                target = hediffs.FirstOrDefault(h => h is Hediff_Injury injury && injury.IsPermanent());
            if (target == null) return; // nada que curar: espera sin gastar el ciclo

            lastRegenTick = now;
            if (target is Hediff_MissingPart missing) pawn.health.RestorePart(missing.Part);
            else pawn.health.RemoveHediff(target);
            if (pawn.Faction == Faction.OfPlayer && PawnUtility.ShouldSendNotificationAbout(pawn))
                Messages.Message("CVG_BloodRegeneration".Translate(pawn.Named("PAWN"), target.Part?.Label ?? target.LabelCap),
                    pawn, MessageTypeDefOf.PositiveEvent);
        }

        private void RechargeLives(DefModExtension_ExtraLives ext)
        {
            if (ext.rechargeDays <= 0f || livesUsed <= 0 || lastLifeTick < 0) return;
            int now = Find.TickManager.TicksGame;
            int ticks = Mathf.RoundToInt(ext.rechargeDays * GenDate.TicksPerDay);
            while (livesUsed > 0 && now - lastLifeTick >= ticks)
            {
                livesUsed--;
                lastLifeTick += ticks;
            }
        }

        /// <summary>Intenta salvar al portador de la muerte gastando una vida. Devuelve true si se salvó.</summary>
        public bool TryUseLife()
        {
            var ext = def.GetModExtension<DefModExtension_ExtraLives>();
            if (ext == null || !Awake || !Active || LivesLeft <= 0) return false;
            livesUsed++;
            lastLifeTick = Find.TickManager.TicksGame;
            BloodlineMechanics.HealFromDeath(pawn);
            if (ext.fireBurstRadius > 0f && pawn.Spawned)
                GenExplosion.DoExplosion(pawn.Position, pawn.Map, ext.fireBurstRadius, DamageDefOf.Flame, pawn,
                    ignoredThings: pawn.Map.mapPawns.AllPawnsSpawned.Where(p => p == pawn || (p.Faction != null && p.Faction == pawn.Faction)).Cast<Thing>().ToList());
            if (pawn.Faction == Faction.OfPlayer)
                Find.LetterStack.ReceiveLetter("CVG_ExtraLife_Label".Translate(pawn.Named("PAWN")),
                    ext.messageKey.Translate(pawn.Named("PAWN"), LivesLeft), LetterDefOf.PositiveEvent, pawn);
            return true;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref livesUsed, "livesUsed");
            Scribe_Values.Look(ref lastLifeTick, "lastLifeTick", -1);
            Scribe_Values.Look(ref lastRegenTick, "lastRegenTick", -1);
        }
    }

    /// <summary>Utilidades y parches de las mecánicas de los genes de sangre.</summary>
    public static class BloodlineMechanics
    {
        private static bool retaliating;

        /// <summary>Cura lo que lo estaba matando: heridas, partes perdidas y enfermedades o estados letales.</summary>
        public static void HealFromDeath(Pawn pawn)
        {
            var health = pawn.health;
            foreach (var part in health.hediffSet.GetMissingPartsCommonAncestors().Select(m => m.Part).ToList())
                health.RestorePart(part);
            foreach (var h in health.hediffSet.hediffs.ToList())
            {
                if (h is Hediff_Injury || (h.def.isBad && (h.def.lethalSeverity > 0f || h.def == HediffDefOf.BloodLoss)) || h.def.makesSickThought)
                    health.RemoveHediff(h);
            }
            health.hediffSet.DirtyCache();
            if (pawn.Spawned) FleckMaker.ThrowLightningGlow(pawn.DrawPos, pawn.Map, 2f);
        }

        public static Gene_Bloodline GeneWith<T>(Pawn pawn) where T : DefModExtension
        {
            var genes = pawn?.genes?.GenesListForReading;
            if (genes == null) return null;
            for (int i = 0; i < genes.Count; i++)
                if (genes[i] is Gene_Bloodline g && g.Active && g.def.HasModExtension<T>()) return g;
            return null;
        }

        [HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
        public static class Patch_Pawn_Kill
        {
            public static bool Prefix(Pawn __instance)
            {
                if (__instance.Dead || __instance.Destroyed || !GameComponent_Convergence.Active) return true;
                var gene = GeneWith<DefModExtension_ExtraLives>(__instance);
                if (gene != null && gene.TryUseLife()) return false;
                // Rasgos con vida extra (Hijo del Cielo).
                return GameComponent_TraitLives.Instance?.TrySave(__instance) != true;
            }
        }

        [HarmonyPatch(typeof(Pawn), nameof(Pawn.PostApplyDamage))]
        public static class Patch_Pawn_PostApplyDamage
        {
            public static void Postfix(Pawn __instance, DamageInfo dinfo)
            {
                if (retaliating || __instance.Dead || !GameComponent_Convergence.Active) return;
                if (!(dinfo.Instigator is Pawn attacker) || attacker == __instance || attacker.Dead) return;
                if (dinfo.Def == null || dinfo.Def.isRanged || dinfo.Def.isExplosive || !attacker.Position.AdjacentTo8WayOrInside(__instance.Position)) return;
                var gene = GeneWith<DefModExtension_Retaliation>(__instance);
                if (gene == null) return;
                var ext = gene.def.GetModExtension<DefModExtension_Retaliation>();
                int now = Find.TickManager.TicksGame;
                if (now - gene.lastRetaliationTick < ext.cooldownTicks || !Rand.Chance(ext.chance)) return;
                gene.lastRetaliationTick = now;
                retaliating = true;
                try
                {
                    attacker.TakeDamage(new DamageInfo(ext.damageDef ?? DamageDefOf.Blunt, ext.amount, 0f, -1f, __instance));
                    if (__instance.Spawned) MoteMaker.ThrowText(__instance.DrawPos, __instance.Map, "CVG_Retaliation".Translate(), 2f);
                }
                finally
                {
                    retaliating = false;
                }
            }
        }
    }
}
