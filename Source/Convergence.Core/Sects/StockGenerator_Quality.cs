using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Como <see cref="StockGenerator_Tag"/>, pero solo con armas de ciertas <c>weaponTags</c> (p. ej. <c>CVG_Sword</c>) y con calidad
    /// mínima: cada objeto sale con una calidad sorteada de <see cref="qualities"/> (p. ej. Excelente 75%, Obra maestra 20%, Legendaria 5%).
    /// Es el stock de especialidad de una secta (espadas de la Espada Sin Forma, armaduras de los Nueve Templados…); el resto del comercio
    /// sigue con los generadores normales.
    /// </summary>
    public class StockGenerator_QualityTag : StockGenerator_Tag
    {
        public List<string> weaponTags;
        public List<QualityChance> qualities;

        public override bool HandlesThingDef(ThingDef thingDef)
        {
            if (!base.HandlesThingDef(thingDef)) return false;
            return weaponTags.NullOrEmpty() || (thingDef.weaponTags != null && thingDef.weaponTags.Any(weaponTags.Contains));
        }

        public override IEnumerable<Thing> GenerateThings(PlanetTile forTile, Faction faction = null)
        {
            foreach (var thing in base.GenerateThings(forTile, faction))
            {
                var comp = thing.TryGetComp<CompQuality>();
                if (comp != null && !qualities.NullOrEmpty())
                {
                    var q = qualities.RandomElementByWeight(c => c.weight).quality;
                    if (q > comp.Quality) comp.SetQuality(q, ArtGenerationContext.Outsider);
                }
                yield return thing;
            }
        }
    }

    public class QualityChance
    {
        public QualityCategory quality;
        public float weight = 1f;
    }
}
