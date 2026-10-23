using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence.Factions
{
    /// <summary>
    /// En un permiso de secta: qué regala (favores "obra maestra" y "excepcional"). Cada opción es un objeto fijo o uno al azar con una
    /// etiqueta de arma, con su calidad y cantidad. Con <see cref="playerChooses"/> el colono elige entre las opciones (una espada legendaria
    /// a elección, una píldora a elección…); si no, sale una al azar.
    /// </summary>
    public class DefModExtension_SectGift : DefModExtension
    {
        public List<SectGiftOption> options;
        public bool playerChooses;
    }

    public class SectGiftOption
    {
        public ThingDef thing;
        public string weaponTag;
        public ThingDef stuff;
        public int count = 1;
        /// <summary>Calidad fija (si el objeto tiene calidad).</summary>
        public QualityCategory quality = QualityCategory.Normal;

        public string Label => thing != null ? thing.LabelCap.ToString() : ("CVG_Factions_GiftAnyWeapon".Translate(weaponTag ?? "?")).ToString();

        /// <summary>
        /// Material de un regalo sin material fijo: un metal de los buenos (acero, plastiacero, acero espiritual...) si el objeto lo admite.
        /// Al azar por frecuencia salía madera: una "hoja de sangre obra maestra" de madera no es lo que promete el favor.
        /// </summary>
        private static ThingDef GiftStuff(ThingDef def)
        {
            var metals = GenStuff.AllowedStuffsFor(def).Where(x => x.stuffProps?.categories?.Contains(StuffCategoryDefOf.Metallic) == true).ToList();
            if (metals.Count == 0) return GenStuff.RandomStuffByCommonalityFor(def);
            var stat = def.IsApparel ? StatDefOf.StuffPower_Armor_Sharp : StatDefOf.SharpDamageMultiplier;
            return metals.OrderByDescending(x => x.GetStatValueAbstract(stat)).Take(3).RandomElement();
        }

        public Thing Make()
        {
            var def = thing ?? DefDatabase<ThingDef>.AllDefsListForReading
                .Where(d => d.weaponTags != null && d.weaponTags.Contains(weaponTag) && d.PlayerAcquirable && d.tradeability != Tradeability.None)
                .RandomElementWithFallback();
            if (def == null) return null;
            var s = def.MadeFromStuff ? (stuff ?? GiftStuff(def)) : null;
            var t = ThingMaker.MakeThing(def, s);
            t.stackCount = Mathf.Clamp(count, 1, def.stackLimit);
            t.TryGetComp<CompQuality>()?.SetQuality(quality, ArtGenerationContext.Outsider);
            return t;
        }
    }

    /// <summary>
    /// Favor de regalo de una secta: como el envío de recursos de Royalty (cápsula donde se apunta, gratis fuera del enfriamiento o pagando
    /// contribución), pero con los objetos de <see cref="DefModExtension_SectGift"/> y, si corresponde, a elección del colono.
    /// </summary>
    public class RoyalTitlePermitWorker_SectGift : RoyalTitlePermitWorker_Targeted
    {
        private Faction faction;
        private SectGiftOption chosen;

        private DefModExtension_SectGift Ext => def.GetModExtension<DefModExtension_SectGift>();

        public override IEnumerable<FloatMenuOption> GetRoyalAidOptions(Map map, Pawn pawn, Faction faction)
        {
            if (map.generatorDef.isUnderground)
            {
                yield return new FloatMenuOption(def.LabelCap + ": " + "CommandCallRoyalAidMapUnreachable".Translate(faction.Named("FACTION")), null);
                yield break;
            }
            if (faction.HostileTo(Faction.OfPlayer))
            {
                yield return new FloatMenuOption("CommandCallRoyalAidFactionHostile".Translate(faction.Named("FACTION")), null);
                yield break;
            }
            var ext = Ext;
            if (ext == null || ext.options.NullOrEmpty())
            {
                yield return new FloatMenuOption(def.LabelCap + ": no gift defined", null);
                yield break;
            }
            System.Action action = null;
            string description = def.LabelCap + ": ";
            if (FillAidOption(pawn, faction, ref description, out bool free))
            {
                action = () =>
                {
                    if (ext.playerChooses && ext.options.Count > 1)
                        Find.WindowStack.Add(new FloatMenu(ext.options.Select(o => new FloatMenuOption(o.Label, () => Begin(pawn, faction, map, free, o))).ToList()));
                    else
                        Begin(pawn, faction, map, free, ext.options.RandomElement());
                };
            }
            yield return new FloatMenuOption(description, action, faction.def.FactionIcon, faction.Color);
        }

        /// <summary>Elige dónde cae la cápsula (público para el autotest).</summary>
        public void Begin(Pawn caller, Faction faction, Map map, bool free, SectGiftOption option)
        {
            targetingParameters = new TargetingParameters { canTargetLocations = true, canTargetBuildings = false, canTargetPawns = false };
            this.caller = caller;
            this.map = map;
            this.faction = faction;
            this.free = free;
            chosen = option;
            float range = RangeClamped;
            targetingParameters.validator = t => (range <= 0f || t.Cell.DistanceTo(caller.Position) <= range) && !t.Cell.Fogged(map)
                && DropCellFinder.CanPhysicallyDropInto(t.Cell, map, canRoofPunch: true);
            Find.Targeter.BeginTargeting(this);
        }

        public override void OrderForceTarget(LocalTargetInfo target)
        {
            var thing = chosen?.Make();
            if (thing == null) return;
            var info = new ActiveTransporterInfo();
            info.innerContainer.TryAdd(thing);
            DropPodUtility.MakeDropPodAt(target.Cell, map, info);
            Messages.Message("MessagePermitTransportDrop".Translate(faction.Named("FACTION")), new LookTargets(target.Cell, map), MessageTypeDefOf.NeutralEvent);
            caller.royalty.GetPermit(def, faction).Notify_Used();
            if (!free) caller.royalty.TryRemoveFavor(faction, def.royalAid.favorCost);
        }
    }
}
