using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Grado espiritual de un arma (docs/modulos/DEFENSA.md §1): 0 mortal (vanilla y otros mods), 1 runas, 2 arma
    /// espiritual, 3+ tesoro. Se pone en XML con este comp; <see cref="bonus"/> (guardado) es para mejoras por
    /// instancia (mesa de runas). El stat <c>CVG_SpiritGrade</c> lo muestra en la ficha del arma.
    /// La contramedida del Core (plan de integración, 2026-10-23): la investigación "armas templadas en qi" pone un piso de 1
    /// a toda arma y torreta, vanilla incluida, y un material espiritual suma su grado encima. Alchemy (runas, armas wuxia) y
    /// Qi-Tech escalan más arriba.
    /// </summary>
    public class CompProperties_SpiritGrade : CompProperties
    {
        public int grade;

        public CompProperties_SpiritGrade()
        {
            compClass = typeof(CompSpiritGrade);
        }
    }

    public class CompSpiritGrade : ThingComp
    {
        public int bonus;

        public CompProperties_SpiritGrade Props => (CompProperties_SpiritGrade)props;
        /// <summary>Grado del def + mejoras por instancia (con el piso de la investigación) + el grado del material.</summary>
        public int Grade => System.Math.Max(Props.grade + bonus, SpiritGradeUtility.ResearchFloor) + MaterialMinGrade(parent.Stuff);

        public static int MaterialMinGrade(ThingDef stuff) => stuff?.GetModExtension<DefModExtension_SpiritGradeMaterial>()?.minGrade ?? 0;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref bonus, "spiritGradeBonus");
        }
    }

    /// <summary>
    /// En un material (stuff): las armas hechas de él suman <see cref="minGrade"/> a su grado espiritual. Un jian mortal de
    /// acero espiritual ya hiere a un cultivador de Refinamiento de Qi; templado en qi (investigación), a uno de Fundación.
    /// </summary>
    public class DefModExtension_SpiritGradeMaterial : DefModExtension
    {
        public int minGrade = 1;
    }

    /// <summary>Stat de solo lectura: grado espiritual del arma (del comp). Solo se muestra en armas.</summary>
    public class StatWorker_SpiritGrade : StatWorker
    {
        public override bool ShouldShowFor(StatRequest req) => (req.Thing?.def ?? req.Def as ThingDef)?.IsWeapon ?? false;

        public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
        {
            if (req.Thing != null) return SpiritGradeUtility.GradeOf(req.Thing);
            return req.Def is ThingDef def ? SpiritGradeUtility.GradeOf(def) + CompSpiritGrade.MaterialMinGrade(req.StuffDef) : 0f;
        }

        public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense) =>
            "CVG_SpiritGrade_Explanation".Translate();
    }

    /// <summary>Curva de la supresión de reino (va en el StatDef <c>CVG_SpiritGrade</c>).</summary>
    public class DefModExtension_RealmSuppression : DefModExtension
    {
        /// <summary>Reinos de diferencia (objetivo − grado del ataque) → multiplicador del daño.</summary>
        public SimpleCurve damageFactorCurve;
    }

    /// <summary>
    /// Supresión de reino (DISENO.md §4.1c): el daño contra un cultivador se reduce si el grado espiritual del ataque es
    /// menor que su reino. Grado del ataque: el del arma (comp <see cref="CompSpiritGrade"/>, vanilla = 0); en técnicas,
    /// puños y armas naturales, el reino del atacante. Daño sin instigador (fuego, tribulación, caídas) no se suprime.
    /// Todo O(1): grados por ThingDef en caché y el reino del objetivo cacheado en su <see cref="CompCultivator"/>.
    /// </summary>
    public static class SpiritGradeUtility
    {
        private static readonly Dictionary<ThingDef, int> defGrades = new Dictionary<ThingDef, int>();
        private static SimpleCurve curve;
        private static bool curveResolved;

        private static readonly SimpleCurve DefaultCurve = new SimpleCurve
        {
            new CurvePoint(0f, 1f),
            new CurvePoint(1f, 0.85f),
            new CurvePoint(2f, 0.5f),
            new CurvePoint(3f, 0.2f),
            new CurvePoint(4f, 0.08f),
            new CurvePoint(5f, 0.03f)
        };

        private static SimpleCurve Curve
        {
            get
            {
                if (!curveResolved)
                {
                    curveResolved = true;
                    curve = CVG_DefOf.CVG_SpiritGrade?.GetModExtension<DefModExtension_RealmSuppression>()?.damageFactorCurve;
                }
                return curve ?? DefaultCurve;
            }
        }

        private static ResearchProjectDef temperedArms;
        private static bool temperedArmsResolved;

        /// <summary>Piso de grado de toda arma y torreta: 1 con la investigación "armas templadas en qi", si no 0.</summary>
        public static int ResearchFloor
        {
            get
            {
                if (!temperedArmsResolved)
                {
                    temperedArmsResolved = true;
                    temperedArms = DefDatabase<ResearchProjectDef>.GetNamedSilentFail("CVG_Research_QiTemperedArms");
                }
                return temperedArms != null && temperedArms.IsFinished ? 1 : 0;
            }
        }

        /// <summary>Grado del def (comp en XML), con el piso de la investigación.</summary>
        public static int GradeOf(ThingDef def)
        {
            if (def == null) return 0;
            if (!defGrades.TryGetValue(def, out int grade))
                defGrades[def] = grade = def.GetCompProperties<CompProperties_SpiritGrade>()?.grade ?? 0;
            return Mathf.Max(grade, ResearchFloor);
        }

        public static int GradeOf(Thing thing)
        {
            if (thing is ThingWithComps twc && twc.GetComp<CompSpiritGrade>() is CompSpiritGrade c) return c.Grade;
            return GradeOf(thing?.def) + CompSpiritGrade.MaterialMinGrade(thing?.Stuff);
        }

        /// <summary>
        /// Nivel de reino de un pawn (0 = mortal): el reino más alto de los caminos que sigue; en animales, el nivel de
        /// bestia espiritual (sus mordiscos hieren a cultivadores de su nivel).
        /// </summary>
        public static int RealmLevel(Pawn pawn)
        {
            if (pawn == null) return 0;
            if (pawn.RaceProps.Animal) return Hediff_SpiritBeast.LevelOf(pawn);
            return CompCultivator.For(pawn)?.RealmLevel ?? 0;
        }

        /// <summary>Grado del daño de una técnica: el reino del lanzador.</summary>
        public static int TechniqueGrade(Pawn caster, AbilityDef def) => RealmLevel(caster);

        /// <summary>Grado espiritual del ataque, o -1 si no se suprime (sin instigador).</summary>
        public static int AttackGrade(DamageInfo dinfo)
        {
            var instigator = dinfo.Instigator;
            if (instigator == null) return -1;
            var weapon = dinfo.Weapon;
            if (instigator is Pawn pawn)
            {
                // Marionetas de guerra: pelean con la fuerza del reino que les presta el titiritero (con el techo de su escalón).
                int puppet = PuppetTierUtility.PowerRealm(pawn);
                // Técnicas (sin equipo), puños y armas naturales: el reino del atacante.
                if (weapon == null || weapon == pawn.def) return Mathf.Max(puppet, RealmLevel(pawn));
                var primary = pawn.equipment?.Primary;
                return Mathf.Max(puppet, primary != null && primary.def == weapon ? GradeOf(primary) : GradeOf(weapon));
            }
            // Torretas y edificios: el arma montada o el propio edificio.
            return weapon != null ? GradeOf(weapon) : GradeOf(instigator.def);
        }

        /// <summary>Multiplicador del daño para un objetivo de reino <paramref name="targetLevel"/>.</summary>
        public static float SuppressionFactor(int targetLevel, int attackGrade)
        {
            if (attackGrade < 0 || targetLevel <= attackGrade) return 1f;
            float strength = CultivationDifficulty.Current.suppressionStrength;
            if (strength <= 0f) return 1f;
            float f = Curve.Evaluate(targetLevel - attackGrade);
            return Mathf.Clamp01(1f - (1f - f) * strength);
        }
    }
}
