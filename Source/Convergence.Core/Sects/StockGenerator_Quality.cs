using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Como <see cref="StockGenerator_Tag"/>, pero solo con armas de ciertas <c>weaponTags</c> (p. ej. <c>CVG_Sword</c>) y con calidad
    /// controlada: cada objeto sale con una calidad sorteada de <see cref="qualities"/> (p. ej. Buena 55%, Excelente 37%, Obra maestra 8%).
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
                    // Calidad exacta del sorteo (no solo "mínima"): así la secta nunca vende algo fuera de su tabla, p. ej. Legendario.
                    comp.SetQuality(qualities.RandomElementByWeight(c => c.weight).quality, ArtGenerationContext.Outsider);
                }
                yield return thing;
            }
        }
    }

    /// <summary>
    /// Stock de especialidad por lista de objetos (p. ej. el equipo de titiritero del Hilo Invisible), con la misma calidad controlada
    /// que <see cref="StockGenerator_QualityTag"/>. A diferencia de los generadores vanilla, vende aunque el objeto sea solo vendible
    /// por el jugador (<c>Tradeability.Sellable</c>): es la secta la que lo fabrica.
    /// </summary>
    public class StockGenerator_QualityDefs : StockGenerator
    {
        public List<ThingDef> thingDefs;
        public IntRange thingDefCountRange = IntRange.One;
        public List<QualityChance> qualities;

        public override bool HandlesThingDef(ThingDef thingDef) => thingDefs != null && thingDefs.Contains(thingDef);

        public override IEnumerable<Thing> GenerateThings(PlanetTile forTile, Faction faction = null)
        {
            if (thingDefs.NullOrEmpty()) yield break;
            var defs = thingDefs.InRandomOrder().Take(thingDefCountRange.RandomInRange).ToList();
            foreach (var def in defs)
            {
                for (int i = RandomCountOf(def); i > 0; i--)
                {
                    var thing = ThingMaker.MakeThing(def, def.MadeFromStuff ? GenStuff.RandomStuffByCommonalityFor(def) : null);
                    var comp = thing.TryGetComp<CompQuality>();
                    if (comp != null && !qualities.NullOrEmpty())
                        comp.SetQuality(qualities.RandomElementByWeight(c => c.weight).quality, ArtGenerationContext.Outsider);
                    yield return thing;
                }
            }
        }
    }

    public class QualityChance
    {
        public QualityCategory quality;
        public float weight = 1f;
    }
}
