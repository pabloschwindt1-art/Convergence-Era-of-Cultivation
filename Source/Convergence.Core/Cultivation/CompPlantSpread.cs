using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>Planta que se propaga sola por los alrededores cuando está madura (bambú). Sin techo, en suelo fértil y fuera de zonas y del área de hogar.</summary>
    public class CompProperties_PlantSpread : CompProperties
    {
        public float mtbDays = 4f;
        public float radius = 3f;
        public float minGrowth = 0.8f;
        public CompProperties_PlantSpread() => compClass = typeof(CompPlantSpread);
    }

    public class CompPlantSpread : ThingComp
    {
        public CompProperties_PlantSpread Props => (CompProperties_PlantSpread)props;

        public override void CompTickLong()
        {
            if (!(parent is Plant plant) || !plant.Spawned || plant.Growth < Props.minGrowth) return;
            if (!Rand.MTBEventOccurs(Props.mtbDays, GenDate.TicksPerDay, GenTicks.TickLongInterval)) return;
            var map = plant.Map;
            if (!CellFinder.TryFindRandomCellNear(plant.Position, map, (int)Props.radius, c =>
                    c != plant.Position && plant.def.CanEverPlantAt(c, map) && c.GetPlant(map) == null
                    && c.GetEdifice(map) == null && !map.roofGrid.Roofed(c)
                    && c.GetZone(map) == null && !map.areaManager.Home[c], out var cell)) return;   // ni huertos ni el área de hogar
            var sprout = (Plant)ThingMaker.MakeThing(plant.def);
            sprout.Growth = 0.05f;
            GenSpawn.Spawn(sprout, cell, map);
        }
    }
}
