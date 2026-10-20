using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Convergence.Stories
{
    // Las sectas como facciones vivas (docs/modulos/CONTENIDO_TOI_ACS.md, "Interacciones entre facciones"): sobre las relaciones vanilla entre facciones
    // (buena voluntad: Faction.TryAffectGoodwillWith), cada pocos días dos sectas firman un pacto comercial, riñen, se enfrentan en una guerra o
    // celebran un torneo. El jugador lo vive con eventos de historia (SectWarAppeal, SectTradePact, SectRivalry, SectTournament) que usan los tokens
    // war:a/war:b y pact:a/pact:b. No toca ningún incidente vanilla ni de otros mods de interacción entre facciones.

    public class SectWar : IExposable
    {
        public Faction A, B;
        public int startTick, endTick;
        /// <summary>0 = nadie, 1 = A, 2 = B (a quién ayudó el jugador).</summary>
        public int favored;
        public bool over;

        public void ExposeData()
        {
            Scribe_References.Look(ref A, "a");
            Scribe_References.Look(ref B, "b");
            Scribe_Values.Look(ref startTick, "startTick");
            Scribe_Values.Look(ref endTick, "endTick");
            Scribe_Values.Look(ref favored, "favored");
            Scribe_Values.Look(ref over, "over");
        }
    }

    public class SectPair
    {
        public Faction A, B;
    }

    public class GameComponent_SectWorld : GameComponent
    {
        private List<SectWar> wars = new List<SectWar>();
        private int nextActionTick = -1;
        private Faction pairA, pairB;

        public static GameComponent_SectWorld Instance => Current.Game?.GetComponent<GameComponent_SectWorld>();

        public GameComponent_SectWorld(Game game) { }

        public IReadOnlyList<SectWar> Wars => wars;
        public SectWar CurrentWar => wars.LastOrDefault(w => !w.over) ?? wars.LastOrDefault();
        public SectPair LastPair => pairA == null ? null : new SectPair { A = pairA, B = pairB };

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref wars, "wars", LookMode.Deep);
            Scribe_Values.Look(ref nextActionTick, "nextActionTick", -1);
            Scribe_References.Look(ref pairA, "pairA");
            Scribe_References.Look(ref pairB, "pairB");
            if (Scribe.mode == LoadSaveMode.PostLoadInit) wars ??= new List<SectWar>();
            wars?.RemoveAll(w => w.A == null || w.B == null);
        }

        public override void GameComponentTick()
        {
            if (Find.TickManager.TicksGame % 2500 != 0 || !GameComponent_Convergence.Active) return;
            int now = Find.TickManager.TicksGame;
            foreach (var w in wars.Where(w => !w.over && now >= w.endTick).ToList()) EndWar(w, null);
            if (nextActionTick < 0) { nextActionTick = now + NextDelay(); return; }
            if (now < nextActionTick) return;
            nextActionTick = now + NextDelay();
            Act();
        }

        private static int NextDelay()
        {
            float f = Math.Max(0.3f, StoryEngine.FrequencyFactor == null ? 1f : StoryEngine.FrequencyFactor());
            return (int)(Rand.Range(8f, 14f) * GenDate.TicksPerDay / f);
        }

        public static List<Faction> Players() => StoryUtility.Sects().Where(f => f.def.defName != "CVG_Faction_BloodMoon" && !f.IsPlayer && !f.Hidden).ToList();

        /// <summary>Una acción entre dos sectas al azar: guerra, pacto, riña o torneo. Devuelve cuál (para las pruebas).</summary>
        public string Act(string forced = null)
        {
            var sects = Players();
            if (sects.Count < 2) return null;
            sects.Shuffle();
            var a = sects[0];
            var b = sects[1];
            pairA = a; pairB = b;
            var map = StoryUtility.HomeMap;
            string kind = forced;
            if (kind == null)
            {
                bool war = CurrentWar != null && !CurrentWar.over;
                var roll = Rand.Value;
                kind = roll < 0.30f && !war ? "war" : roll < 0.62f ? "pact" : roll < 0.84f ? "feud" : "tournament";
            }
            switch (kind)
            {
                case "war": StartWar(a, b); break;
                case "pact":
                    a.TryAffectGoodwillWith(b, 35, canSendMessage: false, canSendHostilityLetter: false);
                    Fire("SectTradePact", map);
                    break;
                case "feud":
                    a.TryAffectGoodwillWith(b, -40, canSendMessage: false, canSendHostilityLetter: false);
                    Fire("SectRivalry", map);
                    break;
                case "tournament":
                    Fire("SectTournament", map);
                    break;
            }
            return kind;
        }

        private static void Fire(string defName, Map map)
        {
            var def = DefDatabase<StoryEventDef>.GetNamedSilentFail(defName);
            if (def != null && map != null) StoryEngine.Fire(def, map);
        }

        /// <summary>Las facciones con buena voluntad no admiten SetRelationDirect: se lleva la buena voluntad al valor pedido y vanilla recalcula la relación.</summary>
        private static void SetGoodwill(Faction a, Faction b, int target)
        {
            int delta = target - a.GoodwillWith(b);
            if (delta != 0) a.TryAffectGoodwillWith(b, delta, canSendMessage: false, canSendHostilityLetter: false);
        }

        public SectWar StartWar(Faction a, Faction b)
        {
            var war = new SectWar { A = a, B = b, startTick = Find.TickManager.TicksGame, endTick = Find.TickManager.TicksGame + (int)(Rand.Range(10f, 18f) * GenDate.TicksPerDay) };
            wars.Add(war);
            SetGoodwill(a, b, -100);
            Fire("SectWarAppeal", StoryUtility.HomeMap);
            return war;
        }

        /// <summary>Termina la guerra. <paramref name="winnerIsA"/> null = al azar, salvo que el jugador haya favorecido a un bando.</summary>
        public void EndWar(SectWar war, bool? winnerIsA, bool peace = false)
        {
            if (war == null || war.over) return;
            war.over = true;
            SetGoodwill(war.A, war.B, 0);
            if (peace)
            {
                war.A.TryAffectGoodwillWith(war.B, 15, canSendMessage: false, canSendHostilityLetter: false);
                Messages.Message("CVG_SectWorld_Peace".Translate(war.A.Name, war.B.Name), MessageTypeDefOf.PositiveEvent, historical: false);
                return;
            }
            bool aWins = winnerIsA ?? (war.favored == 1 ? Rand.Chance(0.75f) : war.favored == 2 ? Rand.Chance(0.25f) : Rand.Bool);
            var winner = aWins ? war.A : war.B;
            var loser = aWins ? war.B : war.A;
            Messages.Message("CVG_SectWorld_WarEnded".Translate(winner.Name, loser.Name), MessageTypeDefOf.NeutralEvent, historical: false);
            if ((war.favored == 1 && aWins) || (war.favored == 2 && !aWins))
                winner.TryAffectGoodwillWith(Faction.OfPlayer, 12, canSendMessage: true, canSendHostilityLetter: false);
        }
    }

    /// <summary>Resultado de un evento sobre la guerra actual entre dos sectas: <c>favorA</c>/<c>favorB</c> (el jugador ayudó), <c>peace</c> (la mediación acaba la guerra).</summary>
    public class SO_SectWar : StoryOutcome
    {
        public string action = "favorA";

        protected override LookTargets Apply(StoryContext ctx)
        {
            var gc = GameComponent_SectWorld.Instance;
            var war = gc?.CurrentWar;
            if (war == null || war.over) return null;
            switch (action)
            {
                case "favorA": war.favored = 1; break;
                case "favorB": war.favored = 2; break;
                case "peace": gc.EndWar(war, null, peace: true); break;
            }
            return null;
        }
    }
}
