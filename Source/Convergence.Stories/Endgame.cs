using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Convergence.Stories
{
    // La cadena final de partida (docs/modulos/ENDGAME.md): cuatro reinos fijos con una llave cada uno, y una elección con tres finales.
    // Los textos de los eventos son secretos (docs/dev/EVENTOS_SECRETOS.md).

    public class GameComponent_Endgame : GameComponent
    {
        public static readonly string[] Keys = { "CVG_FounderSeal", "CVG_JiuxiaoShard", "CVG_RuptureCore", "CVG_HeavenKey" };
        public static readonly string[] Realms = { "CVG_EndRealm_FounderTomb", "CVG_EndRealm_JiuxiaoFragment", "CVG_EndRealm_RuptureField", "CVG_EndRealm_DeepestRealm" };
        public const int StartDays = 45;
        public const int StartRealm = 2;
        public const int CloudLadderDays = 25;
        public const int ReopenDays = 6;

        /// <summary>Las pruebas no deben abrir la pantalla de créditos.</summary>
        public static bool SuppressCredits;

        private int step;           // 0 = sin empezar, 1-4 = reino activo, 5 = elección, 6 = terminado
        private int stepTick;
        private bool gateOpened;
        private int lastGateTick = -999999;
        private string ending;
        private int raidTick = -1;

        public static GameComponent_Endgame Instance => Current.Game?.GetComponent<GameComponent_Endgame>();

        public GameComponent_Endgame(Game game) { }

        /// <summary>Solo para pruebas: otras pruebas disparan las opciones de la elección y dejan un final aplicado.</summary>
        public void ResetForTest() { step = 0; ending = null; gateOpened = false; raidTick = -1; }

        public int Step => step;
        public string Ending => ending;
        public bool GateOpened => gateOpened;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref step, "step");
            Scribe_Values.Look(ref stepTick, "stepTick");
            Scribe_Values.Look(ref gateOpened, "gateOpened");
            Scribe_Values.Look(ref lastGateTick, "lastGateTick", -999999);
            Scribe_Values.Look(ref ending, "ending");
            Scribe_Values.Look(ref raidTick, "raidTick", -1);
        }

        public override void GameComponentTick()
        {
            if (Find.TickManager.TicksGame % 250 != 0 || !GameComponent_Convergence.Active) return;
            Check();
        }

        /// <summary>Avanza la cadena si toca. Lo llama el tick y las pruebas.</summary>
        public void Check()
        {
            int now = Find.TickManager.TicksGame;
            if (raidTick >= 0 && now >= raidTick) { raidTick = -1; CalamityRaid(); }
            var map = StoryUtility.HomeMap;
            if (map == null || step >= 6) return;
            // la carta de elección caduca a los 30 días: si nadie decidió, vuelve
            if (step == 5) { if (ending == null && now - stepTick >= 35 * GenDate.TicksPerDay) Begin(5); return; }
            if (step == 0)
            {
                var conv = GameComponent_Convergence.Instance;
                if (conv != null && conv.Occurred && now - conv.OccurredTick >= StartDays * GenDate.TicksPerDay && CultivatorPawnKindUtility.ColonyMaxRealm() >= StartRealm)
                    Begin(1);
                return;
            }
            int i = step - 1;
            if (HasKey(Keys[i])) { Begin(step + 1); return; }
            if (!gateOpened)
            {
                // paso 2: la puerta se abre sobre la isla; sin isla, una escalera de nubes tras unos días
                bool island = IslandHeart(map) != null;
                if (step != 2 || island || now - stepTick >= CloudLadderDays * GenDate.TicksPerDay) OpenGate(i);
            }
            else if (now - lastGateTick >= ReopenDays * GenDate.TicksPerDay && !GateExists(Realms[i])) OpenGate(i);
        }

        public void Begin(int n)
        {
            step = n;
            stepTick = Find.TickManager.TicksGame;
            gateOpened = false;
            var map = StoryUtility.HomeMap;
            string evName = n <= 4 ? "EndgameStep" + n : "EndgameChoice";
            var def = DefDatabase<StoryEventDef>.GetNamedSilentFail(evName);
            if (def != null && map != null) StoryEngine.Fire(def, map);
        }

        private static Thing IslandHeart(Map map)
        {
            var def = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_IslandHeart");
            return def == null ? null : map.listerThings.ThingsOfDef(def).FirstOrDefault();
        }

        public static bool HasKey(string defName)
        {
            var def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
            if (def == null) return false;
            foreach (var m in Find.Maps)
                if (m.listerThings.ThingsOfDef(def).Count > 0) return true;
            foreach (var p in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists)
                if (p.inventory?.innerContainer.Any(t => t.def == def) == true) return true;
            return false;
        }

        public static bool GateExists(string realmDef)
        {
            var gateDef = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_RealmGate");
            foreach (var m in Find.Maps)
            {
                if (m.GetComponent<MapComponent_SecretRealm>()?.realm?.defName == realmDef) return true;
                if (gateDef != null && m.listerThings.ThingsOfDef(gateDef).OfType<Building_RealmGate>().Any(g => g.realm?.defName == realmDef)) return true;
            }
            return false;
        }

        /// <summary>Abre la puerta del reino del paso (0-3). Devuelve la puerta o null.</summary>
        public Building_RealmGate OpenGate(int index)
        {
            var map = StoryUtility.HomeMap;
            var realm = DefDatabase<SecretRealmDef>.GetNamedSilentFail(Realms[index]);
            if (map == null || realm == null) return null;
            IntVec3 cell = IntVec3.Invalid;
            var heart = index == 1 ? IslandHeart(map) : null;
            if (heart != null)
                CellFinder.TryFindRandomCellNear(heart.Position, map, 9, c => c.Standable(map) && c.GetEdifice(map) == null && c.GetFirstItem(map) == null, out cell);
            if (!cell.IsValid)
                CellFinderLoose.TryGetRandomCellWith(c => c.Standable(map) && !c.Roofed(map) && c.GetEdifice(map) == null && c.DistanceToEdge(map) > 8 && c.DistanceTo(map.Center) < 40, map, 400, out cell);
            if (!cell.IsValid) cell = CellFinder.RandomClosewalkCellNear(map.Center, map, 20);
            var gate = SecretRealmUtility.SpawnGate(map, cell, realm);
            if (gate == null) return null;
            gateOpened = true;
            lastGateTick = Find.TickManager.TicksGame;
            Find.LetterStack.ReceiveLetter("CVG_Endgame_GateLabel".Translate(), ("CVG_Endgame_Gate" + (index + 1)).Translate(), LetterDefOf.PositiveEvent, gate);
            return gate;
        }

        public static void ConsumeKeys()
        {
            foreach (var k in Keys)
            {
                var def = DefDatabase<ThingDef>.GetNamedSilentFail(k);
                if (def == null) continue;
                foreach (var m in Find.Maps) foreach (var t in m.listerThings.ThingsOfDef(def).ToList()) t.Destroy();
                foreach (var p in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists)
                    foreach (var t in (p.inventory?.innerContainer.Where(x => x.def == def).ToList() ?? new List<Thing>())) t.Destroy();
            }
        }

        public static int MaxRealmOrder() => DefDatabase<CultivationRealmDef>.AllDefsListForReading.Select(r => r.order).DefaultIfEmpty(0).Max();

        public static Pawn Ascendant()
        {
            int max = MaxRealmOrder();
            return PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists
                .Where(p => (CompCultivator.For(p)?.RealmLevel ?? 0) >= max).OrderByDescending(p => p.ageTracker.AgeBiologicalYears).FirstOrDefault();
        }

        /// <summary>Aplica un final. Devuelve false si no se pudo (p. ej. nadie en el reino máximo para ascender).</summary>
        public bool Finish(string kind)
        {
            if (ending != null) return false;
            string credits;
            switch (kind)
            {
                case "ascend":
                    var pawn = Ascendant();
                    if (pawn == null) return false;
                    credits = "CVG_Endgame_Credits_Ascend".Translate(pawn.LabelShortCap);
                    Messages.Message("CVG_Endgame_Ascended".Translate(pawn.LabelShortCap), MessageTypeDefOf.PositiveEvent, historical: false);
                    if (pawn.Spawned) pawn.DeSpawn();
                    Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
                    break;
                case "seal":
                    credits = "CVG_Endgame_Credits_Seal".Translate();
                    GameComponent_Story.Instance?.Flags.Add("rift_sealed");
                    foreach (var f in StoryUtility.Sects().Where(f => !f.HostileTo(Faction.OfPlayer)))
                        f.TryAffectGoodwillWith(Faction.OfPlayer, 25, canSendMessage: false, canSendHostilityLetter: false);
                    break;
                case "rule":
                    credits = "CVG_Endgame_Credits_Rule".Translate();
                    GameComponent_Story.Instance?.Flags.Add("court_ruler");
                    foreach (var f in StoryUtility.Sects())
                        f.TryAffectGoodwillWith(Faction.OfPlayer, 100, canSendMessage: false, canSendHostilityLetter: false);
                    var stone = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_SpiritStone");
                    var map = StoryUtility.HomeMap;
                    if (stone != null && map != null)
                    {
                        var t = ThingMaker.MakeThing(stone);
                        t.stackCount = 400;
                        DropPodUtility.DropThingsNear(DropCellFinder.TradeDropSpot(map), map, new List<Thing> { t });
                    }
                    raidTick = Find.TickManager.TicksGame + 10 * GenDate.TicksPerDay;
                    break;
                default:
                    return false;
            }
            ConsumeKeys();
            ending = kind;
            step = 6;
            if (!SuppressCredits) GameVictoryUtility.ShowCredits(credits, null);
            return true;
        }

        private void CalamityRaid()
        {
            var map = StoryUtility.HomeMap;
            if (map == null) return;
            var mech = Faction.OfMechanoids;
            if (mech == null) return;
            var parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, map);
            parms.faction = mech;
            parms.forced = true;
            parms.points = Math.Max(400f, parms.points * 2f);
            if (IncidentDefOf.RaidEnemy.Worker.TryExecute(parms))
                Find.LetterStack.ReceiveLetter("CVG_Endgame_CalamityLabel".Translate(), "CVG_Endgame_CalamityText".Translate(), LetterDefOf.ThreatBig);
        }
    }

    /// <summary>Resultado de la carta de elección final: <c>ascend</c>, <c>seal</c> o <c>rule</c>. Si no se puede (nadie en el reino máximo), la carta vuelve al día siguiente.</summary>
    public class SO_Ending : StoryOutcome
    {
        public string kind;

        protected override LookTargets Apply(StoryContext ctx)
        {
            var gc = GameComponent_Endgame.Instance;
            if (gc == null || gc.Finish(kind)) return null;
            if (gc.Ending == null)
            {
                Messages.Message("CVG_Endgame_NotReady".Translate(), MessageTypeDefOf.RejectInput, historical: false);
                GameComponent_Story.Instance?.Schedule("ev:EndgameChoice", GenDate.TicksPerDay);
            }
            return null;
        }
    }
}
