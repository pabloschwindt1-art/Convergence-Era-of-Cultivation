using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Salas de trabajo wuxia (sala de alquimia, herrería espiritual, estudio de talismanes…): la versión del taller y el laboratorio
    /// vanilla. La mesa declara su sala con <c>building.workTableRoomRole</c> y lleva el stat <c>CVG_WorkRoomQuality</c> en sus
    /// <c>statBases</c>; ese stat suma niveles de calidad por la sala (limpieza, impresión, qi, con <see cref="StatPart_RoomStatCurve"/>),
    /// por los muebles vinculados (instalaciones vanilla con <c>statOffsets</c>) y por el fuego del horno (<see cref="CompSpiritFire"/>).
    /// Así una sala buena compensa a un artesano flojo.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class WorkRoomUtility
    {
        public static readonly StatDef WorkRoomQuality = DefDatabase<StatDef>.GetNamedSilentFail("CVG_WorkRoomQuality");
        public static readonly StatDef AlchemyQuality = DefDatabase<StatDef>.GetNamedSilentFail("CVG_AlchemyQuality");

        /// <summary>La mesa del trabajo actual del colono.</summary>
        public static Thing BenchOf(Pawn pawn) => pawn?.CurJob?.bill?.billStack?.billGiver as Thing;

        /// <summary>La mesa tiene sala de trabajo wuxia (el stat está en sus statBases).</summary>
        public static bool HasWorkRoom(Thing bench) =>
            bench != null && WorkRoomQuality != null && bench.def.statBases != null && bench.def.statBases.Any(s => s.stat == WorkRoomQuality);

        /// <summary>Niveles de calidad que suma la sala (con sus muebles y su fuego).</summary>
        public static float BenchBonus(Thing bench) => HasWorkRoom(bench) ? bench.GetStatValue(WorkRoomQuality) : 0f;

        /// <summary>El producto es una píldora con calidad (alquimista según "Hecho por").</summary>
        public static bool IsPill(ThingDef def) =>
            def != null && def.HasComp(typeof(CompQuality)) && def.comps.Any(c => c is CompProperties_Crafter cr && cr.role == "Alchemist");

        public static bool IsPillJob(Pawn pawn) => IsPill(pawn?.CurJob?.bill?.recipe?.ProducedThingDef);

        /// <summary>Sube o baja niveles de calidad: la parte decimal es probabilidad. Hasta obra maestra, como vanilla.</summary>
        public static QualityCategory AddLevels(QualityCategory q, float bonus)
        {
            if (Mathf.Approximately(bonus, 0f)) return q;
            float abs = Mathf.Abs(bonus);
            int levels = Mathf.FloorToInt(abs) + (Rand.Chance(abs - Mathf.Floor(abs)) ? 1 : 0);
            if (levels == 0) return q;
            int result = bonus > 0f ? Mathf.Min((int)q + levels, Mathf.Max((int)q, (int)QualityCategory.Masterwork)) : Mathf.Max((int)q - levels, 0);
            return (QualityCategory)result;
        }
    }

    /// <summary>Suma a un stat de mesa un valor por la sala: la curva convierte un stat de sala (limpieza, impresión, qi) en niveles.</summary>
    public class StatPart_RoomStatCurve : StatPart
    {
        public RoomStatDef roomStat;
        public SimpleCurve curve;
        [NoTranslate] public string labelKey;

        private float Value(StatRequest req)
        {
            if (!req.HasThing || roomStat == null || curve == null || !req.Thing.Spawned) return 0f;
            var room = req.Thing.GetRoom();
            if (room == null || room.PsychologicallyOutdoors) return 0f;
            return curve.Evaluate(room.GetStat(roomStat));
        }

        public override void TransformValue(StatRequest req, ref float val) => val += Value(req);

        public override string ExplanationPart(StatRequest req)
        {
            float v = Value(req);
            return Mathf.Approximately(v, 0f) ? null : labelKey.Translate() + ": " + v.ToStringByStyle(ToStringStyle.FloatTwo, ToStringNumberSense.Offset);
        }
    }

    /// <summary>Rol de sala por mesa: la sala es "sala de alquimia" si tiene mesas con <c>workTableRoomRole</c> igual a este rol.</summary>
    public class RoomRoleWorker_WorkTableRole : RoomRoleWorker
    {
        private RoomRoleDef def;

        private RoomRoleDef Def => def ??= DefDatabase<RoomRoleDef>.AllDefsListForReading.FirstOrDefault(d => d.workerClass == GetType() && d.Worker == this);

        public override float GetScore(Room room)
        {
            int n = 0;
            var things = room.ContainedAndAdjacentThings;
            for (int i = 0; i < things.Count; i++)
                if (things[i].def.building?.workTableRoomRole == Def) n++;
            return 30f * n;
        }

        public override float GetScoreDeltaIfBuildingPlaced(Room room, ThingDef buildingDef) =>
            buildingDef.building?.workTableRoomRole == Def ? 30f : 0f;
    }

    /// <summary>Un combustible del fuego espiritual y los niveles de calidad que da.</summary>
    public class SpiritFuel
    {
        public ThingDef thingDef;
        public float qualityBonus;
    }

    /// <summary>
    /// Fuego espiritual de un horno con <c>CompRefuelable</c>: el jugador elige el combustible (leña, piedra espiritual, cristal de
    /// fuego…) y el fuego suma calidad según lo que arde en el tanque (promedio de lo que se cargó).
    /// </summary>
    public class CompProperties_SpiritFire : CompProperties
    {
        public List<SpiritFuel> fuels = new List<SpiritFuel>();

        public CompProperties_SpiritFire()
        {
            compClass = typeof(CompSpiritFire);
        }
    }

    public class CompSpiritFire : ThingComp
    {
        private ThingDef chosen;
        private float grade;

        public CompProperties_SpiritFire Props => (CompProperties_SpiritFire)props;
        private IEnumerable<SpiritFuel> Available => Props.fuels.Where(f => f.thingDef != null);
        public ThingDef Chosen => chosen ?? Available.FirstOrDefault()?.thingDef;

        /// <summary>Niveles de calidad del fuego que arde ahora (0 si el horno está apagado).</summary>
        public float FireBonus => parent.GetComp<CompRefuelable>()?.HasFuel == true ? grade : 0f;

        public float BonusOf(ThingDef d) => Available.FirstOrDefault(f => f.thingDef == d)?.qualityBonus ?? 0f;

        public void SetFuel(ThingDef d)
        {
            chosen = d;
            ApplyFilter();
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            ApplyFilter();
        }

        /// <summary>Cada horno tiene su propio filtro de combustible: solo el elegido (los colonos no queman piedras sin querer).</summary>
        private void ApplyFilter()
        {
            var refuel = parent.GetComp<CompRefuelable>();
            if (refuel == null || Chosen == null) return;
            var clone = (CompProperties_Refuelable)AccessTools.Method(typeof(object), "MemberwiseClone").Invoke(refuel.props, null);
            clone.fuelFilter = new ThingFilter();
            clone.fuelFilter.SetAllow(Chosen, true);
            refuel.props = clone;
        }

        public void Notify_Refueled(float before, float added, ThingDef fuel)
        {
            if (added <= 0f) return;
            grade = (before * grade + added * BonusOf(fuel)) / (before + added);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var g in base.CompGetGizmosExtra()) yield return g;
            if (parent.Faction != Faction.OfPlayer || Available.Count() < 2) yield break;
            var current = Chosen;
            yield return new Command_Action
            {
                defaultLabel = "CVG_Fire_Choose".Translate(current.label),
                defaultDesc = "CVG_Fire_ChooseDesc".Translate(string.Join("\n", Available.Select(f =>
                    "- " + f.thingDef.LabelCap + ": " + f.qualityBonus.ToStringByStyle(ToStringStyle.FloatTwo, ToStringNumberSense.Offset)))),
                icon = current.uiIcon,
                action = () => Find.WindowStack.Add(new FloatMenu(Available.Select(f => new FloatMenuOption(
                    f.thingDef.LabelCap + " (" + f.qualityBonus.ToStringByStyle(ToStringStyle.FloatTwo, ToStringNumberSense.Offset) + ")",
                    () => SetFuel(f.thingDef), f.thingDef)).ToList()))
            };
        }

        public override string CompInspectStringExtra() =>
            "CVG_Fire_Inspect".Translate(Chosen?.label ?? "-", FireBonus.ToStringByStyle(ToStringStyle.FloatTwo, ToStringNumberSense.Offset));

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref chosen, "spiritFireFuel");
            Scribe_Values.Look(ref grade, "spiritFireGrade");
        }
    }

    /// <summary>Lo que se carga en un horno con fuego espiritual cambia su grado de fuego.</summary>
    [HarmonyPatch(typeof(CompRefuelable), nameof(CompRefuelable.Refuel), typeof(List<Thing>))]
    public static class Patch_CompRefuelable_RefuelList
    {
        public static void Prefix(CompRefuelable __instance, List<Thing> fuelThings, out (float before, ThingDef def) __state)
        {
            __state = (__instance.Fuel, fuelThings != null && fuelThings.Count > 0 ? fuelThings[fuelThings.Count - 1].def : null);
        }

        public static void Postfix(CompRefuelable __instance, (float before, ThingDef def) __state)
        {
            __instance.parent.GetComp<CompSpiritFire>()?.Notify_Refueled(__state.before, __instance.Fuel - __state.before, __state.def);
        }
    }

    /// <summary>El fuego del horno suma al stat de la sala de trabajo.</summary>
    public class StatPart_SpiritFire : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.Thing is ThingWithComps t && t.GetComp<CompSpiritFire>() is CompSpiritFire fire) val += fire.FireBonus;
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!(req.Thing is ThingWithComps t) || !(t.GetComp<CompSpiritFire>() is CompSpiritFire fire) || Mathf.Approximately(fire.FireBonus, 0f)) return null;
            return "CVG_Fire_Explanation".Translate(fire.Chosen?.label ?? "-") + ": " + fire.FireBonus.ToStringByStyle(ToStringStyle.FloatTwo, ToStringNumberSense.Offset);
        }
    }

    /// <summary>
    /// Alquimia con riesgo: el resultado de una píldora sale de una tabla que mezcla tres (novato, base y gran maestro) según el
    /// "poder" del alquimista: habilidad, sala de trabajo (sala, muebles, fuego), nivel del horno, maestría alquímica del colono e
    /// inspiración. Base (habilidad 10, horno básico): 30% falla o mala, 45% normal, 20% excelente, 5% celestial (legendaria).
    /// Si falla, sale una píldora de desecho (<see cref="Patch_GenRecipe_WastePill"/>).
    /// </summary>
    [HarmonyPatch(typeof(QualityUtility), nameof(QualityUtility.GenerateQualityCreatedByPawn), typeof(Pawn), typeof(SkillDef), typeof(bool))]
    public static class Patch_QualityUtility_PillBands
    {
        // Pesos de: falla, horrible, mala, normal, buena, excelente, obra maestra, legendaria.
        public static readonly float[] Novice = { 0.35f, 0.15f, 0.15f, 0.25f, 0.07f, 0.03f, 0f, 0f };
        public static readonly float[] Base = { 0.15f, 0.07f, 0.08f, 0.30f, 0.15f, 0.13f, 0.07f, 0.05f };
        public static readonly float[] Master = { 0.02f, 0.01f, 0.02f, 0.15f, 0.15f, 0.22f, 0.18f, 0.25f };

        [System.ThreadStatic] public static bool failPending;

        /// <summary>Poder del alquimista, de -1 (novato) a 1 (gran maestro con todo a favor).</summary>
        public static float Power(Pawn pawn, SkillDef skill, bool inspired)
        {
            float p = ((pawn.skills?.GetSkill(skill ?? SkillDefOf.Intellectual)?.Level ?? 0) - 10) * 0.06f;
            var bench = WorkRoomUtility.BenchOf(pawn);
            p += WorkRoomUtility.BenchBonus(bench) * 0.25f;
            p += ((bench as ThingWithComps)?.GetComp<CompWorkbenchLevel>()?.Current?.qualityBonus ?? 0f) * 0.25f;
            if (WorkRoomUtility.AlchemyQuality != null) p += pawn.GetStatValue(WorkRoomUtility.AlchemyQuality) * 0.25f;
            if (inspired) p += 0.5f;
            return Mathf.Clamp(p, -1f, 1f);
        }

        public static float[] Weights(float p)
        {
            var from = p < 0f ? Novice : Master;
            float t = Mathf.Abs(p);
            return Base.Select((b, i) => Mathf.Lerp(b, from[i], t)).ToArray();
        }

        /// <summary>0 = falla; 1..7 = horrible..legendaria.</summary>
        public static int Roll(float[] w)
        {
            float roll = Rand.Value * w.Sum();
            int band = 0;
            for (; band < w.Length - 1; band++)
            {
                roll -= w[band];
                if (roll < 0f) break;
            }
            return band;
        }

        public static bool Prefix(Pawn pawn, SkillDef relevantSkill, bool consumeInspiration, ref QualityCategory __result)
        {
            failPending = false;
            if (pawn == null || !WorkRoomUtility.IsPillJob(pawn)) return true;
            bool inspired = pawn.InspirationDef == InspirationDefOf.Inspired_Creativity;
            if (inspired && consumeInspiration) pawn.mindState.inspirationHandler.EndInspiration(InspirationDefOf.Inspired_Creativity);
            int band = Roll(Weights(Power(pawn, relevantSkill, inspired)));
            if (band == 0)
            {
                failPending = true;
                __result = QualityCategory.Awful;
            }
            else __result = (QualityCategory)(band - 1);
            return false;
        }
    }

    /// <summary>Las demás cosas hechas en una sala de trabajo wuxia suben (o bajan, si la sala es mala) por la sala.</summary>
    [HarmonyPatch(typeof(QualityUtility), nameof(QualityUtility.GenerateQualityCreatedByPawn), typeof(Pawn), typeof(SkillDef), typeof(bool))]
    public static class Patch_QualityUtility_WorkRoom
    {
        public static void Postfix(Pawn pawn, ref QualityCategory __result)
        {
            if (pawn == null || WorkRoomUtility.IsPillJob(pawn)) return;
            __result = WorkRoomUtility.AddLevels(__result, WorkRoomUtility.BenchBonus(WorkRoomUtility.BenchOf(pawn)));
        }
    }

    /// <summary>Una píldora fallida sale como píldora de desecho (si existe; si no, queda horrible).</summary>
    [HarmonyPatch(typeof(GenRecipe), nameof(GenRecipe.MakeRecipeProducts))]
    public static class Patch_GenRecipe_WastePill
    {
        private static readonly ThingDef Waste = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_WastePill");

        public static void Postfix(ref IEnumerable<Thing> __result, Pawn worker)
        {
            __result = Wrap(__result, worker);
        }

        private static IEnumerable<Thing> Wrap(IEnumerable<Thing> products, Pawn worker)
        {
            foreach (var t in products)
            {
                if (!Patch_QualityUtility_PillBands.failPending)
                {
                    yield return t;
                    continue;
                }
                Patch_QualityUtility_PillBands.failPending = false;
                var waste = Waste ?? DefDatabase<ThingDef>.GetNamedSilentFail("CVG_WastePill");
                if (waste == null)
                {
                    yield return t;
                    continue;
                }
                var w = ThingMaker.MakeThing(waste);
                w.stackCount = Mathf.Max(1, t.stackCount);
                if (worker != null && worker.Faction == Faction.OfPlayer)
                    Messages.Message("CVG_Pill_Failed".Translate(worker.Named("PAWN"), t.def.label), worker, MessageTypeDefOf.NegativeEvent);
                t.Destroy();
                yield return w;
            }
        }
    }
}
