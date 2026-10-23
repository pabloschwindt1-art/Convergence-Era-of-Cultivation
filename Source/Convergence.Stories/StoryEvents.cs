using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Convergence.Stories
{
    /// <summary>Carta con decisión: las opciones se resuelven en <see cref="StoryEvents.Decide"/>.</summary>
    public class ChoiceLetter_Story : ChoiceLetter
    {
        public string storyKey;
        public List<string> options = new List<string>();
        public string payload;

        public override bool CanDismissWithRightClick => false;

        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                if (ArchivedOnly)
                {
                    yield return Option_Close;
                    yield break;
                }
                foreach (var o in options)
                {
                    var opt = o;
                    var evDef = DefDatabase<StoryEventDef>.GetNamedSilentFail(storyKey);
                    var optArgs = evDef != null && StoryUtility.HomeMap != null ? StoryContext.Deserialize(StoryUtility.HomeMap, evDef, payload).Args() : new NamedArgument[0];
                    var d = new DiaOption((evDef != null ? "CVG_Ev_" + storyKey + "_Opt_" + opt : "CVG_Story_" + storyKey + "_Opt_" + opt).Translate(optArgs));
                    var evOpt = evDef?.options?.FirstOrDefault(x => x.key == opt);
                    if (evOpt != null && StoryUtility.HomeMap != null && !StoryEngine.CanAffordOption(evOpt, StoryUtility.HomeMap, out var why)) d.Disable(why);
                    d.action = () =>
                    {
                        Find.LetterStack.RemoveLetter(this);
                        StoryEvents.Decide(storyKey, opt, payload);
                    };
                    d.resolveTree = true;
                    yield return d;
                }
                if (lookTargets.IsValid()) yield return Option_JumpToLocationAndPostpone;
                yield return Option_Postpone;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref storyKey, "storyKey");
            Scribe_Collections.Look(ref options, "options", LookMode.Value);
            Scribe_Values.Look(ref payload, "payload");
            options ??= new List<string>();
        }
    }

    public static class StoryEvents
    {
        public static void SendChoice(string key, IEnumerable<string> options, string payload, LookTargets targets, params NamedArgument[] args)
        {
            var letter = (ChoiceLetter_Story)LetterMaker.MakeLetter(DefDatabase<LetterDef>.GetNamed("CVG_StoryLetter"));
            letter.Label = ("CVG_Story_" + key + "_Label").Translate(args);
            letter.title = letter.Label;
            letter.Text = ("CVG_Story_" + key + "_Text").Translate(args);
            letter.storyKey = key;
            letter.options = options.ToList();
            letter.payload = payload;
            letter.lookTargets = targets;
            letter.StartTimeout(GenDate.TicksPerDay * 2);
            Find.LetterStack.ReceiveLetter(letter);
        }

        /// <summary>Evento de seguimiento programado por un escenario.</summary>
        public static void Run(string key)
        {
            var map = StoryUtility.HomeMap;
            if (map == null) return;
            if (key.StartsWith("ev:")) { StoryEngine.RunScheduled(key.Substring(3)); return; }
            if (key.StartsWith("visit:"))
            {
                // visit:<loadID de la facción>:<intento> (visita aplazada porque había huéspedes)
                var parts = key.Split(':');
                if (parts.Length == 3 && int.TryParse(parts[1], out int id) && int.TryParse(parts[2], out int attempt))
                    SO_Visit.Send(Find.FactionManager.AllFactionsListForReading.FirstOrDefault(f => f.loadID == id), map, attempt);
                return;
            }
            switch (key)
            {
                case "assassin":
                    StoryUtility.Raid(StoryUtility.FactionByDef("CVG_Faction_BloodMoon"), 0.5f, "Assassin");
                    break;
                case "refuge":
                    OfferRefuge();
                    break;
                case "pursuers":
                    StoryUtility.Raid(StoryUtility.FactionByDef("CVG_Faction_BloodMoon"), 0.9f, "Pursuers");
                    break;
                case "blood_hunters":
                    StoryUtility.Raid(StoryUtility.FactionByDef("CVG_Faction_BloodMoon"), 0.7f, "BloodHunters");
                    break;
                case "master_offer":
                    GiveElixir(map);
                    break;
                case "clan_returns":
                    var clan = StoryUtility.FactionByDef("CVG_Faction_FormlessSword");
                    StoryUtility.Raid(clan, 0.8f, "ClanReturns");
                    break;
                case "emissaries":
                    Emissaries(map);
                    break;
                case "collective_demon":
                    CollectiveDemon(map);
                    break;
                case "sect_offer":
                    SectOffer(map);
                    break;
                case "treasure_rivals":
                    if (map.listerThings.ThingsOfDef(DefDatabase<ThingDef>.GetNamed("CVG_LightPillar")).Any())
                        StoryUtility.Raid(StoryUtility.Sects().RandomElementWithFallback() ?? StoryUtility.AnyHostileHumanlike(), 0.7f, "TreasureRivals");
                    break;
                case "cultists_aftermath":
                    CultistsAftermath(map);
                    break;
                default:
                    Log.Warning("[Convergence] Unknown story event key " + key);
                    break;
            }
        }

        // ---------------------------------------------------------------- decisiones

        public static void Decide(string key, string option, string payload)
        {
            var map = StoryUtility.HomeMap;
            if (map == null) return;
            var ev = DefDatabase<StoryEventDef>.GetNamedSilentFail(key);
            if (ev != null) { StoryEngine.Decide(ev, option, payload); return; }
            switch (key + "/" + option)
            {
                case "Cultists/accept":
                    var added = new List<Pawn>();
                    int n = Rand.RangeInclusive(2, 4);
                    for (int i = 0; i < n; i++)
                    {
                        var p = StoryUtility.MakeRefugee(map, null);
                        p.SetFaction(Faction.OfPlayer);
                        added.Add(p);
                    }
                    Messages.Message("CVG_Story_Cultists_Result_accept".Translate(n), added, MessageTypeDefOf.PositiveEvent);
                    GameComponent_Story.Instance?.Schedule("cultists_aftermath", Rand.RangeInclusive(3, 6) * GenDate.TicksPerDay);
                    break;
                case "Cultists/refuse":
                    Messages.Message("CVG_Story_Cultists_Result_refuse".Translate(), MessageTypeDefOf.NeutralEvent);
                    break;
                case "AllyPlea/help":
                    var ally = StoryUtility.FactionByDef(payload);
                    if (ally == null) break;
                    ally.TryAffectGoodwillWith(Faction.OfPlayer, 12, canSendMessage: true, canSendHostilityLetter: false);
                    DefendAlly(map, ally);
                    break;
                case "AllyPlea/decline":
                    StoryUtility.FactionByDef(payload)?.TryAffectGoodwillWith(Faction.OfPlayer, -6, canSendMessage: true, canSendHostilityLetter: false);
                    break;
            }
        }

        // ---------------------------------------------------------------- eventos concretos

        public static bool OfferRefuge()
        {
            var map = StoryUtility.HomeMap;
            if (map == null) return false;
            SendChoice("Cultists", new[] { "accept", "refuse" }, null, LookTargets.Invalid);
            return true;
        }

        private static void CultistsAftermath(Map map)
        {
            if (Rand.Chance(0.65f))
            {
                if (StoryUtility.Raid(StoryUtility.FactionByDef("CVG_Faction_BloodMoon") ?? StoryUtility.AnyHostileHumanlike(), 0.9f, "CultistsPursuit")) return;
            }
            var cursed = map.mapPawns.FreeColonistsSpawned.ToList();
            var thought = DefDatabase<ThoughtDef>.GetNamed("CVG_Thought_Cursed");
            foreach (var p in cursed) p.needs?.mood?.thoughts?.memories?.TryGainMemory(thought);
            StoryUtility.SendLetter("CultistsCurse", LetterDefOf.NegativeEvent, LookTargets.Invalid);
        }

        public static void GiveElixir(Map map)
        {
            var elixir = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("CVG_DantianElixir"));
            elixir.stackCount = Mathf.Max(1, map.mapPawns.FreeColonistsSpawned.Count(p => p.health.hediffSet.HasHediff(DefDatabase<HediffDef>.GetNamed("CVG_DamagedDantian"))));
            DropPodUtility.DropThingsNear(DropCellFinder.TradeDropSpot(map), map, new List<Thing> { elixir });
            StoryUtility.SendLetter("MasterOffer", LetterDefOf.PositiveEvent, elixir);
        }

        private static void Emissaries(Map map)
        {
            var sects = StoryUtility.Sects();
            if (sects.Count == 0) return;
            var f = sects.Where(x => !x.HostileTo(Faction.OfPlayer)).RandomElementWithFallback() ?? sects.RandomElement();
            if (f.HostileTo(Faction.OfPlayer))
            {
                StoryUtility.Raid(f, 0.7f, "EmissariesHostile");
                return;
            }
            // Los emisarios llegan una sola vez: si hay huéspedes, esperan a que se vayan.
            if (StoryUtility.GuestsOnMap(map))
            {
                GameComponent_Story.Instance?.Schedule("emissaries", StoryUtility.VisitRetryDays * GenDate.TicksPerDay);
                return;
            }
            var parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.Misc, map);
            parms.faction = f;
            parms.forced = true;
            parms.customLetterLabel = "CVG_Story_Emissaries_Label".Translate();
            parms.customLetterText = "CVG_Story_Emissaries_Text".Translate(f.Named("FACTION"));
            IncidentDefOf.VisitorGroup.Worker.TryExecute(parms);
        }

        private static void CollectiveDemon(Map map)
        {
            var demon = DefDatabase<MentalStateDef>.GetNamedSilentFail("CVG_InnerDemon");
            var victims = map.mapPawns.FreeColonistsSpawned.Where(p => CompCultivator.For(p)?.IsCultivator ?? false).InRandomOrder().Take(3).ToList();
            if (demon == null || victims.Count == 0)
            {
                StoryUtility.SendLetter("CollectiveDemonNone", LetterDefOf.NeutralEvent, LookTargets.Invalid);
                return;
            }
            foreach (var p in victims) p.mindState.mentalStateHandler.TryStartMentalState(demon, "CVG_Story_CollectiveDemon_Reason".Translate(), forceWake: true);
            StoryUtility.SendLetter("CollectiveDemon", LetterDefOf.ThreatSmall, victims);
        }

        private static void SectOffer(Map map)
        {
            foreach (var d in new[] { "CVG_Faction_FormlessSword", "CVG_Faction_HeavenlyCauldron" })
                StoryUtility.FactionByDef(d)?.TryAffectGoodwillWith(Faction.OfPlayer, 35, canSendMessage: false, canSendHostilityLetter: false);
            var gift = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("CVG_SpiritStone"));
            gift.stackCount = 60;
            var slip = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("CVG_JadeSlip"));
            DropPodUtility.DropThingsNear(DropCellFinder.TradeDropSpot(map), map, new List<Thing> { gift, slip });
            StoryUtility.SendLetter("SectOffer", LetterDefOf.PositiveEvent, gift);
        }

        public static void ClanVengeance(Grudge g)
        {
            var f = StoryUtility.FactionByDef(g.factionDef);
            if (f == null) return;
            StoryUtility.Raid(f, 0.85f, "Vengeance", g.victimName.Named("VICTIM"));
        }

        private static void DefendAlly(Map map, Faction ally)
        {
            var enemy = StoryUtility.FactionByDef("CVG_Faction_BloodMoon") ?? StoryUtility.AnyHostileHumanlike();
            StoryUtility.Raid(enemy, 0.8f, "AllyRaid", ally.Named("ALLY"));
            var parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.Misc, map);
            parms.faction = ally;
            parms.points = Mathf.Max(150f, parms.points * 0.6f);
            parms.target = map;
            var pgp = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, parms, ensureCanGenerateAtLeastOnePawn: true);
            var pawns = PawnGroupMakerUtility.GeneratePawns(pgp).ToList();
            if (pawns.Count == 0 || !RCellFinder.TryFindRandomPawnEntryCell(out var cell, map, CellFinder.EdgeRoadChance_Friendly)) return;
            foreach (var p in pawns) GenSpawn.Spawn(p, CellFinder.RandomClosewalkCellNear(cell, map, 6), map);
            LordMaker.MakeNewLord(ally, new LordJob_AssistColony(ally, cell), map, pawns);
            Messages.Message("CVG_Story_AllyPlea_Reinforcements".Translate(ally.Named("ALLY")), pawns, MessageTypeDefOf.PositiveEvent);
        }
    }

    // ---------------------------------------------------------------- incidentes (storyteller)

    public abstract class IncidentWorker_Story : IncidentWorker
    {
        protected virtual int MinDaysSinceConvergence => 3;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms)) return false;
            var gc = GameComponent_Convergence.Instance;
            if (gc == null || !gc.Occurred || !(parms.target is Map map) || !map.IsPlayerHome) return false;
            return Find.TickManager.TicksGame - gc.OccurredTick >= MinDaysSinceConvergence * GenDate.TicksPerDay;
        }
    }

    public class IncidentWorker_CultistsFlee : IncidentWorker_Story
    {
        protected override bool TryExecuteWorker(IncidentParms parms) => StoryEvents.OfferRefuge();
    }

    public class IncidentWorker_AllyPlea : IncidentWorker_Story
    {
        private static Faction PickAlly() =>
            StoryUtility.Sects().Where(f => !f.HostileTo(Faction.OfPlayer) && f.def.defName != "CVG_Faction_BloodMoon").RandomElementWithFallback();

        protected override bool CanFireNowSub(IncidentParms parms) => base.CanFireNowSub(parms) && PickAlly() != null;

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var ally = PickAlly();
            if (ally == null) return false;
            StoryEvents.SendChoice("AllyPlea", new[] { "help", "decline" }, ally.def.defName, LookTargets.Invalid, ally.Named("ALLY"));
            return true;
        }
    }

    public class IncidentWorker_ClanVengeance : IncidentWorker_Story
    {
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var f = StoryUtility.Sects().Where(x => x.def.defName != "CVG_Faction_BloodMoon").RandomElementWithFallback() ?? StoryUtility.AnyHostileHumanlike();
            if (f == null) return false;
            StoryEvents.ClanVengeance(new Grudge { factionDef = f.def.defName, victimName = "CVG_Story_Vengeance_SomeoneName".Translate() });
            return true;
        }
    }

    public class IncidentWorker_BeastMigration : IncidentWorker_Story
    {
        private static readonly string[] Kinds = { "CVG_WindWolf", "CVG_WindWolf", "CVG_NineNetherSpider", "Bear_Grizzly", "Wolf_Timber", "Rhinoceros" };

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var map = (Map)parms.target;
            var kinds = Kinds.Select(k => DefDatabase<PawnKindDef>.GetNamedSilentFail(k)).Where(k => k != null).ToList();
            if (kinds.Count == 0) return false;
            var kind = kinds.RandomElement();
            int count = Mathf.Clamp(Mathf.RoundToInt(Mathf.Max(120f, parms.points) / Mathf.Max(30f, kind.combatPower)), 2, 8);
            if (!RCellFinder.TryFindRandomPawnEntryCell(out var entry, map, CellFinder.EdgeRoadChance_Animal)) return false;
            var animals = new List<Pawn>();
            for (int i = 0; i < count; i++)
            {
                var a = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, null, PawnGenerationContext.NonPlayer, map.Tile));
                GenSpawn.Spawn(a, CellFinder.RandomClosewalkCellNear(entry, map, 8), map, Rot4.Random);
                SpiritBeastUtility.TryAwaken(a);
                animals.Add(a);
            }
            LordMaker.MakeNewLord(null, new LordJob_ExitMapBest(LocomotionUrgency.Walk, canDig: false, canDefendSelf: true), map, animals);
            SendStandardLetter("CVG_Story_Beasts_Label".Translate(), "CVG_Story_Beasts_Text".Translate(kind.GetLabelPlural()), LetterDefOf.NeutralEvent, parms, animals);
            return true;
        }
    }

    public class IncidentWorker_TreasureBirth : IncidentWorker_Story
    {
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var map = (Map)parms.target;
            if (!CellFinderLoose.TryGetRandomCellWith(c => c.Standable(map) && !c.Roofed(map) && c.DistanceToEdge(map) > 8 && !c.Fogged(map), map, 200, out var cell)) return false;
            var pillar = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("CVG_LightPillar"));
            GenSpawn.Spawn(pillar, cell, map);
            var loot = new List<Thing>();
            void Add(string def, int n) { var t = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed(def)); t.stackCount = n; loot.Add(t); }
            Add("CVG_SpiritStone", Rand.RangeInclusive(60, 120));
            Add("CVG_SpiritJade", Rand.RangeInclusive(5, 12));
            Add("CVG_JadeSlip", 1);
            foreach (var t in loot) GenPlace.TryPlaceThing(t, cell, map, ThingPlaceMode.Near);
            SendStandardLetter("CVG_Story_Treasure_Label".Translate(), "CVG_Story_Treasure_Text".Translate(), LetterDefOf.PositiveEvent, parms, pillar);
            // Los rivales llegan a disputarlo en unas horas
            GameComponent_Story.Instance?.Schedule("treasure_rivals", Rand.RangeInclusive(6, 12) * GenDate.TicksPerHour);
            return true;
        }
    }
}
