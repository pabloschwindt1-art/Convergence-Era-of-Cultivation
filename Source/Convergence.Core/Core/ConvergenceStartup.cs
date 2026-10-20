using System.Linq;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Se ejecuta una vez con todos los Defs cargados. Agrega el componente de cultivo y la pestaña
    /// a todo ThingDef humanoide, incluidas razas de otros mods (un parche XML no alcanza a las heredadas).
    /// </summary>
    [StaticConstructorOnStartup]
    public static class ConvergenceStartup
    {
        static ConvergenceStartup()
        {
            int count = 0;
            var tab = InspectTabManager.GetSharedInstance(typeof(ITab_Cultivation));

            foreach (var def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                // Solo pawns vivos: las defs de cadáver copian `race` pero su clase es Corpse.
                if (def.race == null || !def.race.Humanlike || !typeof(Pawn).IsAssignableFrom(def.thingClass)) continue;

                def.comps ??= new System.Collections.Generic.List<CompProperties>();
                if (!def.comps.Any(c => c is CompProperties_Cultivator))
                    def.comps.Add(new CompProperties_Cultivator());

                def.inspectorTabs ??= new System.Collections.Generic.List<System.Type>();
                if (!def.inspectorTabs.Contains(typeof(ITab_Cultivation)))
                {
                    def.inspectorTabs.Add(typeof(ITab_Cultivation));
                    def.inspectorTabsResolved ??= new System.Collections.Generic.List<InspectTabBase>();
                    def.inspectorTabsResolved.Add(tab);
                }
                count++;
            }

            Log.Message($"[Convergence] Cultivation added to {count} humanlike race(s).");
        }
    }
}
