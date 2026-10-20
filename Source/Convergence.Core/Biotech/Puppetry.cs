using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    // Maestro de Marionetas (docs/modulos/BIOTECH_WUXIA.md §1): reinterpreta al mecanitor de Biotech SIN sistema paralelo.
    // - Un hediff con DefModExtension_MechanitorLink (el "Hilo del Corazón de Marioneta") da capacidad de mecanitor igual
    //   que el mecaenlace (parche a MechanitorUtility.ShouldBeMechanitor): control, grupos, gestación, ancho de banda.
    // - Las marionetas son mecas (razas vanilla) con CompPuppet: escalan con el reino del maestro, recargan energía con
    //   su qi y al romperse le dan un contragolpe. El ancho de banda suma el reino (StatPart_PuppetBandwidth).
    // El contenido (implante, marionetas, taller) está en el mod Convergence: Biotech Wuxia; aquí solo el código genérico.

    /// <summary>Marca un hediff como enlace de mecanitor alternativo (Hilo del Corazón de Marioneta).</summary>
    public class DefModExtension_MechanitorLink : DefModExtension
    {
    }

    public static class PuppetUtility
    {
        private static List<HediffDef> linkHediffs;

        public static List<HediffDef> LinkHediffs =>
            linkHediffs ??= DefDatabase<HediffDef>.AllDefsListForReading.Where(h => h.HasModExtension<DefModExtension_MechanitorLink>()).ToList();

        public static bool HasPuppetLink(Pawn pawn)
        {
            var set = pawn?.health?.hediffSet;
            if (set == null) return false;
            var defs = LinkHediffs;
            for (int i = 0; i < defs.Count; i++)
                if (set.HasHediff(defs[i])) return true;
            return false;
        }
    }

    /// <summary>El hilo de marioneta también hace mecanitor (el mecaenlace vanilla sigue funcionando igual).</summary>
    [HarmonyPatch(typeof(MechanitorUtility), nameof(MechanitorUtility.ShouldBeMechanitor))]
    public static class Patch_MechanitorUtility_ShouldBeMechanitor
    {
        public static void Postfix(Pawn pawn, ref bool __result)
        {
            if (__result || !ModsConfig.BiotechActive || !pawn.Faction.IsPlayerSafe()) return;
            if (PuppetUtility.HasPuppetLink(pawn)) __result = true;
        }
    }

    /// <summary>
    /// Hediff del hilo: al ponerlo o quitarlo recalcula los componentes dinámicos (crea o borra el tracker de mecanitor),
    /// como hace el mecaenlace.
    /// </summary>
    public class Hediff_PuppetLink : HediffWithComps
    {
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            PawnComponentsUtility.AddAndRemoveDynamicComponents(pawn);
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            PawnComponentsUtility.AddAndRemoveDynamicComponents(pawn);
        }
    }

    /// <summary>Ancho de banda extra de un maestro de marionetas: +N por nivel de reino (fuerza del alma).</summary>
    public class StatPart_PuppetBandwidth : StatPart
    {
        public float perRealmLevel = 1f;

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.Thing is Pawn pawn && PuppetUtility.HasPuppetLink(pawn)) val += perRealmLevel * SpiritGradeUtility.RealmLevel(pawn);
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!(req.Thing is Pawn pawn) || !PuppetUtility.HasPuppetLink(pawn)) return null;
            int level = SpiritGradeUtility.RealmLevel(pawn);
            return level <= 0 ? null : "CVG_Puppet_BandwidthPart".Translate(level, (perRealmLevel * level).ToString("+0;-0"));
        }
    }

    /// <summary>Marioneta: escala con el reino del maestro, recarga con su qi y contragolpe al romperse.</summary>
    public class CompProperties_Puppet : CompProperties
    {
        /// <summary>Hediff en la marioneta: severidad = reino del maestro (stats por etapa en el XML).</summary>
        public HediffDef bondHediff;
        /// <summary>Hediff en el maestro cuando una marioneta se rompe.</summary>
        public HediffDef backlashHediff;
        /// <summary>Fracción del qi del maestro que se pierde con el contragolpe.</summary>
        public float backlashQiLoss = 0.3f;
        /// <summary>Recarga con el qi del maestro: qi gastado por cada 10% de energía.</summary>
        public float qiPerTenPercent = 8f;
        /// <summary>Por debajo de esta energía la marioneta pide qi al maestro.</summary>
        public float rechargeBelow = 0.6f;
        /// <summary>Vida (HealthScale) extra por nivel de reino del maestro (marionetas de trabajo; las de guerra usan la tabla del reino).</summary>
        public float healthPerRealmLevel = 0.3f;
        /// <summary>Debilidad del material (madera: fuego; hierro: frío...). Se pone al aparecer.</summary>
        public HediffDef materialHediff;

        public CompProperties_Puppet()
        {
            compClass = typeof(CompPuppet);
        }
    }

    public class CompPuppet : ThingComp
    {
        private const int Interval = 1000;
        private int masterLevel;
        // Último maestro conocido: al morir la marioneta la relación de supervisión ya se ha borrado.
        private Pawn lastMaster;

        public CompProperties_Puppet Props => (CompProperties_Puppet)props;
        private Pawn Puppet => parent as Pawn;
        public int MasterLevel => masterLevel;
        private PuppetPower power = PuppetPower.None;
        public DefModExtension_PuppetTier Tier => parent.def.GetModExtension<DefModExtension_PuppetTier>();
        public bool IsCombat => Tier?.combat ?? false;
        /// <summary>Fuerza de una marioneta de guerra según el reino del titiritero (tabla del reino, con techo).</summary>
        public PuppetPower Power => power;
        public float HealthFactor => IsCombat ? power.health : 1f + Props.healthPerRealmLevel * masterLevel;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            var mat = Tier?.materialHediff ?? Props.materialHediff;
            if (mat != null && Puppet != null && !Puppet.health.hediffSet.HasHediff(mat))
                Puppet.health.AddHediff(mat);
            Refresh();
        }

        public override void CompTickInterval(int delta)
        {
            base.CompTickInterval(delta);
            if (!parent.IsHashIntervalTick(Interval, delta)) return;
            Refresh();
        }

        /// <summary>Sincroniza el vínculo con el reino del maestro y recarga con su qi si hace falta.</summary>
        public void Refresh()
        {
            var puppet = Puppet;
            var master = puppet?.GetOverseer();
            if (master != null) lastMaster = master;
            SyncBond(master);
            RechargeFromMaster(puppet, master);
        }

        /// <summary>El hediff del vínculo sigue el reino del maestro: si sube, todas sus marionetas mejoran.</summary>
        private void SyncBond(Pawn master)
        {
            int level = master == null ? 0 : SpiritGradeUtility.RealmLevel(master);
            var puppet = Puppet;
            var newPower = master == null ? PuppetPower.None : PuppetTierUtility.Compute(master, Tier);
            if (level != masterLevel || newPower.realm != power.realm || newPower.stage != power.stage)
            {
                masterLevel = level;
                power = newPower;
                puppet.health.hediffSet.DirtyCache();
            }
            if (Props.bondHediff == null) return;
            var h = puppet.health.hediffSet.GetFirstHediffOfDef(Props.bondHediff);
            if (level <= 0)
            {
                if (h != null) puppet.health.RemoveHediff(h);
                return;
            }
            if (h == null) h = puppet.health.AddHediff(Props.bondHediff);
            h.Severity = level;
        }

        /// <summary>Si le falta energía y el maestro tiene qi, lo transfiere (recarga sin electricidad).</summary>
        private void RechargeFromMaster(Pawn puppet, Pawn master)
        {
            var energy = puppet?.needs?.energy;
            if (energy == null || master == null || energy.CurLevelPercentage >= Props.rechargeBelow) return;
            var comp = CompCultivator.For(master);
            if (comp == null) return;
            foreach (var tracker in comp.ActiveTrackers)
            {
                if (!tracker.Started || tracker.Resource.Current < Props.qiPerTenPercent) continue;
                tracker.Resource.TryConsume(Props.qiPerTenPercent);
                energy.CurLevel = Mathf.Min(energy.MaxLevel, energy.CurLevel + energy.MaxLevel * 0.1f);
                if (puppet.Spawned) FleckMaker.ThrowMicroSparks(puppet.DrawPos, puppet.Map);
                return;
            }
        }

        /// <summary>Contragolpe del hilo: el maestro recibe una herida del alma y pierde qi.</summary>
        public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
        {
            base.Notify_Killed(prevMap, dinfo);
            // desarmarla (JobDriver_DisassembleMech) la "mata" sin daño: como en vanilla, eso no castiga a nadie
            if (dinfo == null) return;
            var master = Puppet?.GetOverseer() ?? lastMaster;
            if (master == null || master.Dead) return;
            if (Props.backlashHediff != null) master.health.AddHediff(Props.backlashHediff);
            var comp = CompCultivator.For(master);
            if (comp != null)
                foreach (var tracker in comp.ActiveTrackers)
                    if (tracker.Started) tracker.Resource.SetCurrent(tracker.Resource.Current * (1f - Props.backlashQiLoss));
            if (master.Faction?.IsPlayer ?? false)
                Messages.Message("CVG_Puppet_Backlash".Translate(master.Named("PAWN"), parent.LabelShort), master, MessageTypeDefOf.NegativeHealthEvent);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref masterLevel, "masterLevel");
            Scribe_References.Look(ref lastMaster, "lastMaster");
        }
    }

    /// <summary>
    /// Estación de recarga con esencia espiritual: un cargador de mecas vanilla (sin consumo eléctrico, red propia) que
    /// solo funciona con combustible (piedras espirituales) y lo gasta mientras carga.
    /// </summary>
    [HarmonyPatch(typeof(Building_MechCharger), nameof(Building_MechCharger.IsPowered), MethodType.Getter)]
    public static class Patch_Building_MechCharger_IsPowered
    {
        public static void Postfix(Building_MechCharger __instance, ref bool __result)
        {
            if (!__result) return;
            var fuel = __instance.GetComp<CompRefuelable>();
            if (fuel == null) return;
            if (!fuel.HasFuel) __result = false;
        }
    }

    /// <summary>Gasta la esencia espiritual una vez por tick mientras carga.</summary>
    [HarmonyPatch(typeof(Building_MechCharger), "Tick")]
    public static class Patch_Building_MechCharger_Tick
    {
        public static void Postfix(Building_MechCharger __instance)
        {
            if (__instance.CurrentlyChargingMech == null) return;
            var fuel = __instance.GetComp<CompRefuelable>();
            if (fuel != null && fuel.HasFuel) fuel.Notify_UsedThisTick();
        }
    }
}

namespace Convergence
{
    /// <summary>
    /// El cargador vanilla exige una red eléctrica (Power.PowerNet != null) además de estar encendido; un cargador de qi
    /// (CompQiNode: el cargador espiritual de las marionetas) no tiene red eléctrica y nunca dejaba cargar. Para esos, la
    /// misma regla vanilla pero con la red de qi.
    /// </summary>
    [HarmonyPatch(typeof(Building_MechCharger), nameof(Building_MechCharger.CanPawnChargeCurrently))]
    public static class Patch_Building_MechCharger_CanPawnChargeCurrently
    {
        public static bool Prefix(Building_MechCharger __instance, Pawn pawn, ref bool __result)
        {
            var node = __instance.GetComp<CompQiNode>();
            if (node == null) return true;
            __result = !__instance.IsFullOfWaste && __instance.IsCompatibleWithCharger(pawn.kindDef) && node.Powered && __instance.IsPowered
                       && (__instance.CurrentlyChargingMech == null || __instance.CurrentlyChargingMech == pawn);
            return false;
        }
    }
}
