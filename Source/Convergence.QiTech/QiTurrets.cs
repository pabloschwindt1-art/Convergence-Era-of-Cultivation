using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Convergence.QiTech
{
    /// <summary>Cañón de qi (objeto de la forja): la torreta de qi de este nivel lo pide para construirse.</summary>
    public class DefModExtension_QiCannon : DefModExtension
    {
        public int grade = 1;
    }

    /// <summary>Marca de una torreta de qi generada: de qué torreta sale y su grado.</summary>
    public class DefModExtension_QiTurret : DefModExtension
    {
        public ThingDef source;
        public int grade;
    }

    /// <summary>
    /// Qi-Tech aplica Harmony en su clase Mod (antes de cargar los defs): <see cref="QiTurretGenerator"/> tiene que correr
    /// antes de que el juego genere los planos y marcos de construcción.
    /// </summary>
    public class QiTechMod : Mod
    {
        public QiTechMod(ModContentPack content) : base(content)
        {
            new Harmony("prospero1088.convergence.qitech").PatchAll();
        }
    }

    /// <summary>
    /// Torretas de qi (decisión del usuario, 2026-10-23): cada torreta del juego que dispara daño (vanilla, de este mod,
    /// Vanilla Expanded Security o cualquier otro) tiene su versión de qi I, II y III. Son clones del ThingDef con el grado en
    /// un <see cref="CompProperties_SpiritGrade"/> del edificio (la supresión toma el mayor entre el arma montada y el
    /// edificio), cuestan lo mismo que la torreta normal más el cañón de su nivel y piden su investigación. En el menú de
    /// construcción quedan en un desplegable con la torreta normal, como las variantes vanilla
    /// (<see cref="DesignatorDropdownGroupDef"/>). Se generan antes que los planos y marcos (como las defs implícitas
    /// vanilla), cuando las referencias cruzadas de los XML ya están resueltas.
    /// </summary>
    [HarmonyPatch(typeof(DefGenerator), nameof(DefGenerator.GenerateImpliedDefs_PreResolve))]
    public static class QiTurretGenerator
    {
        public const string DefPrefix = "CVG_QiTurret";
        private static readonly string[] Roman = { "", "I", "II", "III", "IV", "V", "VI" };
        private static readonly string[] Research =
            { "", "CVG_Research_QiForging", "CVG_Research_QiFundamentals", "CVG_Research_QiAdvancedMilitary" };

        public static void Prefix(bool hotReload)
        {
            if (hotReload) return;
            var cannons = DefDatabase<ThingDef>.AllDefsListForReading
                .Where(d => d.HasModExtension<DefModExtension_QiCannon>())
                .ToDictionary(d => d.GetModExtension<DefModExtension_QiCannon>().grade);
            if (cannons.Count == 0) return;
            var sources = DefDatabase<ThingDef>.AllDefsListForReading.Where(Qualifies).ToList();
            foreach (var source in sources)
            {
                var group = source.designatorDropdown;
                if (group == null)
                {
                    group = new DesignatorDropdownGroupDef { defName = DefPrefix + "Group_" + source.defName, label = source.label };
                    DefDatabase<DesignatorDropdownGroupDef>.Add(group);
                    source.designatorDropdown = group;
                }
                foreach (var pair in cannons.OrderBy(p => p.Key))
                    DefDatabase<ThingDef>.Add(MakeQiTurret(source, pair.Key, pair.Value, group));
            }
            Log.Message($"[Convergence] Qi-Tech: {sources.Count} turrets with qi versions ({sources.Count * cannons.Count} defs).");
        }

        /// <summary>Torretas que se construyen, disparan algo que hace daño y no tienen ya grado espiritual.</summary>
        public static bool Qualifies(ThingDef def)
        {
            if (def.defName.StartsWith(DefPrefix) || def.designationCategory == null || def.thingClass == null) return false;
            if (!typeof(Building_TurretGun).IsAssignableFrom(def.thingClass)) return false;
            var gun = def.building?.turretGunDef;
            if (gun?.Verbs == null || (gun.GetCompProperties<CompProperties_SpiritGrade>()?.grade ?? 0) > 0) return false;
            if ((def.GetCompProperties<CompProperties_SpiritGrade>()?.grade ?? 0) > 0) return false;
            return gun.Verbs.Any(v => v.defaultProjectile?.projectile?.damageDef?.harmsHealth ?? false);
        }

        private static ThingDef MakeQiTurret(ThingDef source, int grade, ThingDef cannon, DesignatorDropdownGroupDef group)
        {
            var def = (ThingDef)AccessTools.Method(typeof(object), "MemberwiseClone").Invoke(source, null);
            string roman = grade < Roman.Length ? Roman[grade] : grade.ToString();
            def.defName = $"{DefPrefix}{grade}_{source.defName}";
            def.label = "CVG_QiTurret_Label".Translate(source.label, roman);
            def.description = source.description + "\n\n" + "CVG_QiTurret_Desc".Translate(cannon.label, grade);
            def.shortHash = 0;
            def.index = ushort.MaxValue;
            def.generated = true;
            def.designatorDropdown = group;
            def.blueprintDef = null;
            def.installBlueprintDef = null;
            def.frameDef = null;
            def.inspectorTabsResolved = null;
            def.building = (BuildingProperties)AccessTools.Method(typeof(object), "MemberwiseClone").Invoke(source.building, null);
            def.comps = new List<CompProperties>(source.comps ?? new List<CompProperties>()) { new CompProperties_SpiritGrade { grade = grade } };
            def.costList = new List<ThingDefCountClass>(source.costList ?? new List<ThingDefCountClass>()) { new ThingDefCountClass(cannon, 1) };
            def.researchPrerequisites = new List<ResearchProjectDef>(source.researchPrerequisites ?? new List<ResearchProjectDef>());
            var research = grade < Research.Length ? DefDatabase<ResearchProjectDef>.GetNamedSilentFail(Research[grade]) : null;
            if (research != null && !def.researchPrerequisites.Contains(research)) def.researchPrerequisites.Add(research);
            def.descriptionHyperlinks = new List<DefHyperlink>(source.descriptionHyperlinks ?? new List<DefHyperlink>()) { new DefHyperlink(cannon) };
            def.modExtensions = new List<DefModExtension>(source.modExtensions ?? new List<DefModExtension>())
                { new DefModExtension_QiTurret { source = source, grade = grade } };
            return def;
        }
    }
}
