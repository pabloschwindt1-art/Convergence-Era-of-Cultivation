using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence.QiTech
{
    // Convergence: Qi-Tech (docs/modulos/QITECH.md). El convertidor une la red de qi del Core con la red eléctrica vanilla;
    // la cámara de estabilización baja la disonancia celestial de quien descansa en ella.

    public enum ConverterMode { QiToPower, PowerToQi }

    public class CompProperties_QiConverter : CompProperties_QiNode
    {
        /// <summary>Vatios que entrega (modo qi → electricidad) o que consume (modo electricidad → qi).</summary>
        public float watts = 1000f;

        public CompProperties_QiConverter() => compClass = typeof(CompQiConverterNode);
    }

    /// <summary>
    /// Lado "red de qi" del convertidor. En modo qi → electricidad consume <c>consumeDay</c> de qi; en modo electricidad → qi produce
    /// <c>produceDay</c> mientras la red eléctrica lo abastece. Pierde por el camino (100 qi/día → 1000 W; 1000 W → 55 qi/día).
    /// </summary>
    public class CompQiConverterNode : CompQiNode
    {
        private ConverterMode mode = ConverterMode.QiToPower;

        public CompProperties_QiConverter ConverterProps => (CompProperties_QiConverter)props;
        public ConverterMode Mode => mode;
        public CompPowerTrader PowerComp => parent.GetComp<CompPowerTrader>();

        public override bool IsConsumer => mode == ConverterMode.QiToPower;

        public override float ConsumeDay => mode == ConverterMode.QiToPower && Active && SwitchedOn ? Props.consumeDay : 0f;

        public override float ProduceDay
        {
            get
            {
                if (mode != ConverterMode.PowerToQi || !Active || !SwitchedOn) return 0f;
                var power = PowerComp;
                return power != null && power.PowerOn ? Props.produceDay : 0f;
            }
        }

        public bool SwitchedOn => parent.GetComp<CompFlickable>()?.SwitchIsOn ?? true;

        public void SetMode(ConverterMode m)
        {
            mode = m;
            var power = PowerComp;
            // el encendido lo decide la red eléctrica (y UpdateDesiredPowerOutput al generar)
            Manager?.MarkDirty();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref mode, "converterMode", ConverterMode.QiToPower);
        }

        public override string CompInspectStringExtra()
        {
            string s = ("CVG_QiConverter_Mode_" + mode).Translate();
            var power = PowerComp;
            if (power != null)   // el Core oculta el texto vanilla de energía en los nodos de qi: aquí la línea de vatios
                s += "\n" + "CVG_QiConverter_Watts".Translate((mode == ConverterMode.QiToPower ? "+" : "-") + System.Math.Abs(power.PowerOutput).ToString("0"));
            string b = base.CompInspectStringExtra();
            return b.NullOrEmpty() ? s : s + "\n" + b;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var g in base.CompGetGizmosExtra()) yield return g;
            if (!Active) yield break;
            yield return new Command_Action
            {
                defaultLabel = ("CVG_QiConverter_Toggle_" + mode).Translate(),
                defaultDesc = "CVG_QiConverter_ToggleDesc".Translate(ConverterProps.watts.ToString("F0"), Props.consumeDay.ToString("F0"), Props.produceDay.ToString("F0")),
                icon = ContentFinder<Texture2D>.Get("UI/Commands/CVG_ConverterMode", false),
                action = () => SetMode(mode == ConverterMode.QiToPower ? ConverterMode.PowerToQi : ConverterMode.QiToPower)
            };
        }
    }

    /// <summary>
    /// Lado "red eléctrica" del convertidor. En modo qi → electricidad actúa como generador (entrega <c>watts</c> si la red de qi lo abastece);
    /// en modo electricidad → qi se comporta como un consumidor corriente.
    /// </summary>
    public class CompQiConverterPower : CompPowerPlant
    {
        private CompQiConverterNode node;
        private CompQiConverterNode Node => node ??= parent.GetComp<CompQiConverterNode>();

        public override void UpdateDesiredPowerOutput()
        {
            var n = Node;
            if (n == null || n.Mode == ConverterMode.PowerToQi)
            {
                // consumidor: como el de vanilla, demanda mientras está encendido y la red lo abastece
                bool flickOn = flickableComp == null || flickableComp.SwitchIsOn;
                bool broken = breakdownableComp != null && breakdownableComp.BrokenDown;
                PowerOutput = flickOn && !broken ? -Props.PowerConsumption : 0f;
                return;
            }
            bool ok = n.SwitchedOn && n.Active && n.Powered && !(breakdownableComp != null && breakdownableComp.BrokenDown);
            if (ok && !PowerOn) PowerOn = true;   // solo cuando de verdad puede generar (forzarlo apagado avisaba en el log)
            PowerOutput = ok ? n.ConverterProps.watts : 0f;
        }
    }

    // ------------------------------------------------------------------ cámara de estabilización

    public class CompProperties_StabilizationChamber : CompProperties
    {
        public HediffDef hediff;
        public CompProperties_StabilizationChamber() => compClass = typeof(CompStabilizationChamber);
    }

    /// <summary>Cama con corriente: quien descansa dentro y lleva algo que suma disonancia celestial (implantes, injertos) recibe el hediff que la baja.</summary>
    public class CompStabilizationChamber : ThingComp
    {
        public CompProperties_StabilizationChamber Props => (CompProperties_StabilizationChamber)props;

        public override void CompTickRare()
        {
            base.CompTickRare();
            Apply();
        }

        /// <summary>Aplica el hediff a quien esté en la cámara. Devuelve cuántos lo recibieron.</summary>
        public int Apply()
        {
            var bed = parent as Building_Bed;
            var power = parent.GetComp<CompPowerTrader>();
            if (bed == null || !parent.Spawned || Props.hediff == null || power == null || !power.PowerOn) return 0;
            int n = 0;
            foreach (var p in bed.CurOccupants)
            {
                if (!NeedsStabilizing(p)) continue;
                var h = p.health.hediffSet.GetFirstHediffOfDef(Props.hediff) ?? p.health.AddHediff(Props.hediff);
                var d = h.TryGetComp<HediffComp_Disappears>();
                if (d != null) d.ticksToDisappear = System.Math.Max(d.ticksToDisappear, Props.hediff.comps.OfType<HediffCompProperties_Disappears>().First().disappearsAfterTicks.max);
                n++;
            }
            return n;
        }

        /// <summary>Lleva algo que sube la disonancia celestial (un hediff con ese stat positivo) o un aparato que lo hace.</summary>
        public static bool NeedsStabilizing(Pawn p)
        {
            var stat = CVG_DefOf.CVG_CelestialDissonance;
            if (p.GetStatValue(stat) > 0.001f) return true;   // incluye biónicos vanilla (StatPart_ImplantDissonance del Core)
            foreach (var h in p.health.hediffSet.hediffs)
            {
                var stages = h.def.stages;
                if (stages == null) continue;
                foreach (var st in stages)
                    if (st.statOffsets != null && st.statOffsets.Any(o => o.stat == stat && o.value > 0f)) return true;
            }
            if (p.apparel != null)
                foreach (var a in p.apparel.WornApparel)
                    if (a.def.equippedStatOffsets != null && a.def.equippedStatOffsets.Any(o => o.stat == stat && o.value > 0f)) return true;
            return false;
        }
    }
}
