using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Templar un arma (forja de qi de Qi-Tech, plan de fusión 2026-10-23): la receta toma un arma y no la destruye; sube un
    /// grado espiritual y, con <see cref="DefModExtension_Temper.raiseQuality"/>, un nivel de calidad (tope legendaria). Cada
    /// receta acepta solo armas de un grado (filtros <c>SpecialThingFilterWorker_NotSpiritGradeN</c>), así el costo crece
    /// con el grado sin techo fijo. Los demás ingredientes se consumen normal. Como <see cref="RecipeWorker_EngraveRunes"/>.
    /// </summary>
    public class RecipeWorker_TemperGrade : RecipeWorker
    {
        public override void ConsumeIngredient(Thing ingredient, RecipeDef recipe, Map map)
        {
            if (ingredient.def.IsWeapon && ingredient is ThingWithComps twc && twc.GetComp<CompSpiritGrade>() is CompSpiritGrade comp)
            {
                Temper(twc, comp, recipe.GetModExtension<DefModExtension_Temper>()?.raiseQuality ?? true);
                if (map != null) FleckMaker.ThrowLightningGlow(ingredient.DrawPos, map, 2f);
                return;
            }
            base.ConsumeIngredient(ingredient, recipe, map);
        }

        public static void Temper(ThingWithComps weapon, CompSpiritGrade comp, bool raiseQuality)
        {
            // El grado sin material ya tiene el piso de la investigación: se sube desde ahí.
            int baseGrade = System.Math.Max(comp.Props.grade + comp.bonus, SpiritGradeUtility.ResearchFloor);
            comp.bonus = baseGrade + 1 - comp.Props.grade;
            if (!raiseQuality) return;
            var quality = weapon.GetComp<CompQuality>();
            if (quality != null && quality.Quality < QualityCategory.Legendary)
                quality.SetQuality(quality.Quality + 1, ArtGenerationContext.Colony);
        }
    }

    public class DefModExtension_Temper : DefModExtension
    {
        public bool raiseQuality = true;
    }

    /// <summary>Base de los filtros "no es de grado N": la receta de templar a N+1 los usa para excluir el resto.</summary>
    public abstract class SpecialThingFilterWorker_NotSpiritGrade : SpecialThingFilterWorker
    {
        protected abstract int Grade { get; }
        public override bool Matches(Thing t) => SpiritGradeUtility.GradeOf(t) != Grade;
        public override bool CanEverMatch(ThingDef def) => def.IsWeapon;
    }

    public class SpecialThingFilterWorker_NotSpiritGrade0 : SpecialThingFilterWorker_NotSpiritGrade { protected override int Grade => 0; }
    public class SpecialThingFilterWorker_NotSpiritGrade1 : SpecialThingFilterWorker_NotSpiritGrade { protected override int Grade => 1; }
    public class SpecialThingFilterWorker_NotSpiritGrade2 : SpecialThingFilterWorker_NotSpiritGrade { protected override int Grade => 2; }
    public class SpecialThingFilterWorker_NotSpiritGrade3 : SpecialThingFilterWorker_NotSpiritGrade { protected override int Grade => 3; }
    public class SpecialThingFilterWorker_NotSpiritGrade4 : SpecialThingFilterWorker_NotSpiritGrade { protected override int Grade => 4; }
    public class SpecialThingFilterWorker_NotSpiritGrade5 : SpecialThingFilterWorker_NotSpiritGrade { protected override int Grade => 5; }
}
