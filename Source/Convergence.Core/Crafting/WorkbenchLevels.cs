using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>Un nivel de una mesa mejorable: arte, coste para llegar a él, velocidad y calidad.</summary>
    public class WorkbenchLevel
    {
        [NoTranslate] public string texPath;
        public List<ThingDefCountClass> upgradeCost = new List<ThingDefCountClass>();
        public float workSpeedFactor = 1f;
        /// <summary>Niveles de calidad extra de lo que se fabrica aquí (decimal = probabilidad).</summary>
        public float qualityBonus;
        /// <summary>Investigación necesaria para subir a este nivel (Comprensión del Dao), opcional.</summary>
        public ResearchProjectDef requiredResearch;
    }

    /// <summary>
    /// Mesa con niveles (Forja Espiritual, Horno de alquimia…): se mejora en el sitio con un botón que consume los
    /// materiales del mapa. Las recetas con <see cref="DefModExtension_RequiresWorkbenchLevel"/> piden un nivel mínimo.
    /// Usar con <see cref="Building_LeveledWorkTable"/> para cambiar el arte por nivel.
    /// </summary>
    public class CompProperties_WorkbenchLevel : CompProperties
    {
        public List<WorkbenchLevel> levels = new List<WorkbenchLevel>();

        public CompProperties_WorkbenchLevel()
        {
            compClass = typeof(CompWorkbenchLevel);
        }
    }

    public class CompWorkbenchLevel : ThingComp
    {
        private int level = 1;

        public CompProperties_WorkbenchLevel Props => (CompProperties_WorkbenchLevel)props;
        public int Level => level;
        public WorkbenchLevel Current => Props.levels.Count == 0 ? null : Props.levels[Mathf.Clamp(level - 1, 0, Props.levels.Count - 1)];
        public WorkbenchLevel Next => level < Props.levels.Count ? Props.levels[level] : null;

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var g in base.CompGetGizmosExtra()) yield return g;
            var next = Next;
            if (next == null || parent.Faction != Faction.OfPlayer) yield break;
            string cost = next.upgradeCost.Select(c => c.count + " " + c.thingDef.label).ToCommaList();
            var cmd = new Command_Action
            {
                defaultLabel = "CVG_Workbench_Upgrade".Translate(level + 1),
                defaultDesc = "CVG_Workbench_UpgradeDesc".Translate(parent.LabelShort, level + 1, cost),
                icon = parent.def.uiIcon,
                action = TryUpgrade
            };
            if (next.requiredResearch != null && !next.requiredResearch.IsFinished)
                cmd.Disable("CVG_Workbench_RequiresResearch".Translate(next.requiredResearch.LabelCap));
            else if (!HasMaterials(next)) cmd.Disable("CVG_Workbench_Missing".Translate(cost));
            yield return cmd;
        }

        /// <summary>Los materiales de la mejora tienen que estar cerca de la mesa (no desaparecen de la otra punta del mapa).</summary>
        public const float MaterialRadius = 8.9f;

        private IEnumerable<Thing> Nearby(ThingDef def) =>
            parent.Map.listerThings.ThingsOfDef(def).Where(t => !t.IsForbidden(Faction.OfPlayer) && !t.Position.Fogged(t.Map)
                && t.Position.InHorDistOf(parent.Position, MaterialRadius));

        private bool HasMaterials(WorkbenchLevel next) => next.upgradeCost.All(c => Nearby(c.thingDef).Sum(t => t.stackCount) >= c.count);

        private void TryUpgrade()
        {
            var next = Next;
            if (next == null || !HasMaterials(next) || (next.requiredResearch != null && !next.requiredResearch.IsFinished)) return;
            foreach (var c in next.upgradeCost)
            {
                int left = c.count;
                foreach (var t in Nearby(c.thingDef).OrderBy(t => t.Position.DistanceToSquared(parent.Position)).ToList())
                {
                    if (left <= 0) break;
                    int take = Mathf.Min(left, t.stackCount);
                    t.SplitOff(take).Destroy();
                    left -= take;
                }
            }
            SetLevel(level + 1);
            FleckMaker.ThrowLightningGlow(parent.DrawPos, parent.Map, 2f);
            Messages.Message("CVG_Workbench_Upgraded".Translate(parent.LabelShort, level), parent, MessageTypeDefOf.PositiveEvent);
        }

        public void SetLevel(int newLevel)
        {
            level = Mathf.Clamp(newLevel, 1, Mathf.Max(1, Props.levels.Count));
            (parent as Building_LeveledWorkTable)?.Notify_LevelChanged();
        }

        public override string CompInspectStringExtra() => "CVG_Workbench_Level".Translate(level, Props.levels.Count);

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref level, "workbenchLevel", 1);
        }
    }

    /// <summary>Mesa de trabajo cuyo arte cambia con el nivel (<see cref="WorkbenchLevel.texPath"/>).</summary>
    public class Building_LeveledWorkTable : Building_WorkTable
    {
        private Graphic cachedGraphic;
        private int cachedLevel = -1;

        public override Graphic Graphic
        {
            get
            {
                var comp = GetComp<CompWorkbenchLevel>();
                string tex = comp?.Current?.texPath;
                if (tex.NullOrEmpty()) return base.Graphic;
                if (cachedGraphic == null || cachedLevel != comp.Level)
                {
                    cachedLevel = comp.Level;
                    // Se copia el graphicData del def (con drawRotated, sombra, etc.) y solo cambia la textura: crear la imagen
                    // "a mano" perdía esos datos y el juego giraba el dibujo según la rotación del edificio (horno dado vuelta).
                    var gd = new GraphicData();
                    gd.CopyFrom(def.graphicData);
                    gd.texPath = tex;
                    cachedGraphic = gd.GraphicColoredFor(this);
                }
                return cachedGraphic;
            }
        }

        public void Notify_LevelChanged()
        {
            cachedGraphic = null;
            if (Spawned) Map.mapDrawer.MapMeshDirty(Position, MapMeshFlagDefOf.Things);
        }
    }

    /// <summary>Receta (o producto de una receta) que solo se ofrece en mesas de cierto nivel.</summary>
    public class DefModExtension_RequiresWorkbenchLevel : DefModExtension
    {
        public int level = 1;
    }

    public static class WorkbenchLevelUtility
    {
        public static CompWorkbenchLevel LevelCompOf(Pawn pawn) =>
            (pawn?.CurJob?.bill?.billStack?.billGiver as ThingWithComps)?.GetComp<CompWorkbenchLevel>();
    }

    [HarmonyPatch(typeof(RecipeDef), nameof(RecipeDef.AvailableOnNow))]
    public static class Patch_RecipeDef_AvailableOnNow_Level
    {
        public static void Postfix(RecipeDef __instance, Thing thing, ref bool __result)
        {
            if (!__result) return;
            // En la receta, o en el producto (las recetas de recipeMaker se generan solas).
            var ext = __instance.GetModExtension<DefModExtension_RequiresWorkbenchLevel>()
                      ?? __instance.ProducedThingDef?.GetModExtension<DefModExtension_RequiresWorkbenchLevel>();
            if (ext == null) return;
            var comp = (thing as ThingWithComps)?.GetComp<CompWorkbenchLevel>();
            if (comp != null && comp.Level < ext.level) __result = false;
        }
    }

    /// <summary>Velocidad por nivel: StatPart sobre <c>WorkTableWorkSpeedFactor</c> (stat de la mesa).</summary>
    [StaticConstructorOnStartup]
    public static class WorkbenchLevelStartup
    {
        static WorkbenchLevelStartup()
        {
            var stat = StatDefOf.WorkTableWorkSpeedFactor;
            stat.parts ??= new List<StatPart>();
            stat.parts.Add(new StatPart_WorkbenchLevel { parentStat = stat });
        }
    }

    public class StatPart_WorkbenchLevel : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            var lvl = (req.Thing as ThingWithComps)?.GetComp<CompWorkbenchLevel>()?.Current;
            if (lvl != null) val *= lvl.workSpeedFactor;
        }

        public override string ExplanationPart(StatRequest req)
        {
            var comp = (req.Thing as ThingWithComps)?.GetComp<CompWorkbenchLevel>();
            return comp?.Current == null || Mathf.Approximately(comp.Current.workSpeedFactor, 1f)
                ? null
                : "CVG_Workbench_Level".Translate(comp.Level, comp.Props.levels.Count) + ": x" + comp.Current.workSpeedFactor.ToStringPercent();
        }
    }

    /// <summary>Calidad por nivel de mesa (se suma al hook de forja).</summary>
    [HarmonyPatch(typeof(QualityUtility), nameof(QualityUtility.GenerateQualityCreatedByPawn), typeof(Pawn), typeof(SkillDef), typeof(bool))]
    public static class Patch_QualityUtility_WorkbenchLevel
    {
        public static void Postfix(Pawn pawn, ref QualityCategory __result)
        {
            float bonus = WorkbenchLevelUtility.LevelCompOf(pawn)?.Current?.qualityBonus ?? 0f;
            if (bonus <= 0f || __result >= QualityCategory.Masterwork) return;
            int levels = Mathf.FloorToInt(bonus) + (Rand.Chance(bonus - Mathf.Floor(bonus)) ? 1 : 0);
            __result = (QualityCategory)Mathf.Min((int)__result + levels, (int)QualityCategory.Masterwork);
        }
    }
}
