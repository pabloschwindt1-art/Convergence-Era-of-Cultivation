using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Rango del jugador en una facción (discípulo externo → interno → núcleo → anciano…). Se apoya en el sistema vanilla de
    /// buena voluntad: el rango es el más alto cuyo <see cref="minGoodwill"/> se cumple y cuyo reino mínimo alcanza algún colono.
    /// Al subir de rango llega una carta y, opcionalmente, un regalo.
    /// </summary>
    public class FactionRankDef : Def
    {
        public FactionDef faction;
        public int order;
        public int minGoodwill;
        public CultivationRealmDef minRealm;
        /// <summary>Regalo que llega en cápsula al subir a este rango.</summary>
        public List<ThingDefCountClass> rewards;
    }

    public static class FactionRankUtility
    {
        public static FactionRankDef CurrentRank(Faction faction)
        {
            if (faction == null || faction.IsPlayer || faction.def == null) return null;
            int realm = CultivatorPawnKindUtility.ColonyMaxRealm();
            FactionRankDef best = null;
            foreach (var r in DefDatabase<FactionRankDef>.AllDefsListForReading)
            {
                if (r.faction != faction.def || faction.PlayerGoodwill < r.minGoodwill) continue;
                if (r.minRealm != null && realm < r.minRealm.order) continue;
                if (best == null || r.order > best.order) best = r;
            }
            return best;
        }
    }

    public class GameComponent_FactionRanks : GameComponent
    {
        private const int CheckInterval = 2500;
        private Dictionary<string, int> announced = new Dictionary<string, int>();
        private List<string> keysTmp;
        private List<int> valsTmp;

        public GameComponent_FactionRanks(Game game) { }

        public override void GameComponentTick()
        {
            if (Find.TickManager.TicksGame % CheckInterval != 0 || !GameComponent_Convergence.Active) return;
            foreach (var faction in Find.FactionManager.AllFactionsVisible.ToList())
            {
                var rank = FactionRankUtility.CurrentRank(faction);
                int now = rank?.order ?? 0;
                announced.TryGetValue(faction.def.defName, out int before);
                announced[faction.def.defName] = now;
                if (now > before) Promote(faction, rank);
            }
        }

        private static void Promote(Faction faction, FactionRankDef rank)
        {
            var map = Find.AnyPlayerHomeMap;
            string text = "CVG_Rank_Text".Translate(faction.Name, rank.LabelCap, faction.Named("FACTION"));
            if (!rank.rewards.NullOrEmpty() && map != null)
            {
                var items = new List<Thing>();
                foreach (var r in rank.rewards)
                {
                    var t = ThingMaker.MakeThing(r.thingDef);
                    t.stackCount = r.count;
                    items.Add(t);
                }
                DropPodUtility.DropThingsNear(DropCellFinder.TradeDropSpot(map), map, items);
                text += "\n\n" + "CVG_Rank_Reward".Translate(items.Select(i => i.LabelCap.ToString()).ToCommaList());
            }
            SoundCues.OnCamera("CVG_Cue_RankUp");
            Find.LetterStack.ReceiveLetter("CVG_Rank_Label".Translate(rank.LabelCap, faction.Name), text, LetterDefOf.PositiveEvent);
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref announced, "announced", LookMode.Value, LookMode.Value, ref keysTmp, ref valsTmp);
            if (announced == null) announced = new Dictionary<string, int>();
        }
    }
}
