using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    // Interfaz simple, como los psíquicos (docs/DISENO.md §8, ronda 3): la mayoría de las técnicas son pasivas
    // (desbloquearla da un hediff permanente que escala con el reino y la maestría, sin botón) y solo unas pocas activas
    // se equipan en ranuras (3-4 según el reino), cuyos gizmos aparecen solo con el colono reclutado.

    /// <summary>
    /// Hediff de una técnica pasiva: su severidad es el poder de la técnica (reino × maestría × afinidad), así las etapas
    /// del XML suben solas; gana maestría con el tiempo (la práctica diaria). Lo pone y lo quita <see cref="TechniqueTracker"/>.
    /// </summary>
    public class HediffCompProperties_PassiveTechnique : HediffCompProperties
    {
        public int refreshTicks = 2500;

        public HediffCompProperties_PassiveTechnique()
        {
            compClass = typeof(HediffComp_PassiveTechnique);
        }
    }

    public class HediffComp_PassiveTechnique : HediffComp
    {
        public HediffCompProperties_PassiveTechnique Props => (HediffCompProperties_PassiveTechnique)props;

        public AbilityDef Technique => PassiveTechniqueUtility.TechniqueFor(parent.def);

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            Refresh();
        }

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);
            if (!Pawn.IsHashIntervalTick(Props.refreshTicks, delta)) return;
            var technique = Technique;
            var comp = CompCultivator.For(Pawn);
            if (technique == null || comp?.TechniquesIfAny == null || !comp.TechniquesIfAny.IsUnlocked(technique))
            {
                Pawn.health.RemoveHediff(parent);
                return;
            }
            float xpPerDay = TechniqueUtility.Ext(technique)?.masteryXpPerDay ?? 0f;
            if (xpPerDay > 0f && !Pawn.Dead)
                comp.TechniquesIfAny.AddMasteryXp(technique, xpPerDay * Props.refreshTicks / GenDate.TicksPerDay
                    * (TechniqueUtility.HasAffinity(Pawn, technique) ? TechniqueUtility.AffinityMasteryFactor : 1f));
            Refresh();
        }

        public void Refresh()
        {
            var technique = Technique;
            if (technique != null) parent.Severity = Mathf.Max(0.01f, TechniqueUtility.Power(Pawn, technique));
        }

        public override string CompLabelInBracketsExtra
        {
            get
            {
                var technique = Technique;
                return technique == null ? null : TechniqueUtility.Mastery(Pawn, technique)?.label;
            }
        }
    }

    /// <summary>Conjunto de combate (psyset de VPE): hasta 3-4 técnicas activas; solo las del activo tienen gizmo.</summary>
    public class TechniqueSet : IExposable
    {
        public string name;
        /// <summary>Camino del que salió el conjunto automático (null = hecho por el jugador).</summary>
        public TechniqueManualDef manual;
        public List<AbilityDef> techniques = new List<AbilityDef>();

        public string Label => name.NullOrEmpty() ? manual?.LabelCap.ToString() ?? "-" : name.CapitalizeFirst();

        public void ExposeData()
        {
            Scribe_Values.Look(ref name, "name");
            Scribe_Defs.Look(ref manual, "manual");
            Scribe_Collections.Look(ref techniques, "techniques", LookMode.Def);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                techniques ??= new List<AbilityDef>();
                techniques.RemoveAll(t => t == null);
            }
        }
    }

    [StaticConstructorOnStartup]
    public static class TechniqueSetUtility
    {
        public static Command MakeSwitchGizmo(CompCultivator comp)
        {
            var techniques = comp.Techniques;
            var active = techniques.ActiveSet;
            var icon = active?.techniques.FirstOrDefault()?.uiIcon ?? active?.manual?.Icon ?? BaseContent.BadTex;
            return new Command_Action
            {
                defaultLabel = "CVG_Set_Gizmo".Translate(active?.Label ?? "-"),
                defaultDesc = "CVG_Set_GizmoDesc".Translate(),
                icon = icon,
                Order = -90f,
                action = () =>
                {
                    var options = new List<FloatMenuOption>();
                    for (int i = 0; i < techniques.Sets.Count; i++)
                    {
                        int index = i;
                        var set = techniques.Sets[i];
                        string label = set.Label + " (" + set.techniques.Count + ")" + (i == techniques.ActiveSetIndex ? " ✓" : "");
                        options.Add(new FloatMenuOption(label, () => techniques.SetActiveSet(index),
                            set.techniques.FirstOrDefault()?.uiIcon ?? set.manual?.Icon, Color.white));
                    }
                    options.Add(new FloatMenuOption("CVG_Set_OpenDevelopment".Translate(), () => Find.WindowStack.Add(new Dialog_CultivationDevelopment(comp.Pawn))));
                    Find.WindowStack.Add(new FloatMenu(options));
                }
            };
        }
    }

    public static class PassiveTechniqueUtility
    {
        private static Dictionary<HediffDef, AbilityDef> byHediff;

        public static bool IsPassive(AbilityDef def) => TechniqueUtility.Ext(def)?.passiveHediff != null;

        public static AbilityDef TechniqueFor(HediffDef hediff)
        {
            if (byHediff == null)
            {
                byHediff = new Dictionary<HediffDef, AbilityDef>();
                foreach (var a in DefDatabase<AbilityDef>.AllDefsListForReading)
                {
                    var h = TechniqueUtility.Ext(a)?.passiveHediff;
                    if (h != null) byHediff[h] = a;
                }
            }
            return hediff != null && byHediff.TryGetValue(hediff, out var t) ? t : null;
        }

        /// <summary>Ranuras de técnicas activas del colono (curva del camino del qi; 3 por defecto, 4 desde Fundación).</summary>
        public static int ActiveSlots(Pawn pawn)
        {
            var path = CVG_DefOf.CVG_Path_Qi;
            var comp = CompCultivator.For(pawn);
            int realm = Mathf.Max(1, TechniqueUtility.RealmOrder(comp, path));
            return path.activeTechniqueSlots != null ? Mathf.Max(1, Mathf.RoundToInt(path.activeTechniqueSlots.Evaluate(realm))) : (realm >= 2 ? 4 : 3);
        }

        /// <summary>Gizmos de técnicas: solo las equipadas y, en colonos del jugador, solo reclutados.</summary>
        public static bool ShouldHideGizmo(Pawn pawn, AbilityDef def)
        {
            if (!pawn.IsColonistPlayerControlled) return false;
            if (!pawn.Drafted) return true;
            var comp = CompCultivator.For(pawn);
            return comp != null && !comp.Techniques.IsEquipped(def);
        }
    }
}
