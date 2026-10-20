using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>Árbol frutal: "Cosechar" da la fruta (harvestedThingDef) y rebrota; "Cortar" además da madera.</summary>
    public class DefModExtension_WoodOnCut : DefModExtension
    {
        public ThingDef thingDef;
        public int yield = 20;
    }

    /// <summary>
    /// Vanilla da una sola cosecha por planta (fruta o madera). Al cortar (JobDriver_PlantCut: recoge la fruta y destruye
    /// el árbol) se suma la madera del <see cref="DefModExtension_WoodOnCut"/>, según cuánto creció, como la leña de un árbol.
    /// </summary>
    public class Plant_FruitTree : Plant
    {
        public override void PlantCollected(Pawn by, PlantDestructionMode plantDestructionMode)
        {
            var ext = def.GetModExtension<DefModExtension_WoodOnCut>();
            if (plantDestructionMode == PlantDestructionMode.Cut && ext?.thingDef != null && Spawned)
            {
                int n = GenMath.RoundRandom(ext.yield * Growth);
                if (n > 0)
                {
                    var wood = ThingMaker.MakeThing(ext.thingDef);
                    wood.stackCount = n;
                    GenPlace.TryPlaceThing(wood, Position, Map, ThingPlaceMode.Near);
                }
            }
            base.PlantCollected(by, plantDestructionMode);
        }
    }
}
