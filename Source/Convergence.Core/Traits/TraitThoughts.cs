using System.Linq;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Inspiración de cultivo (iluminación súbita, intención de la espada, fuego de la forja): solo para cultivadores
    /// despiertos y después de la Convergencia, para que una partida sin ella no las vea.
    /// </summary>
    public class InspirationWorker_Cultivator : InspirationWorker
    {
        public override bool InspirationCanOccur(Pawn pawn)
        {
            if (!base.InspirationCanOccur(pawn) || !GameComponent_Convergence.Active) return false;
            var comp = CompCultivator.For(pawn);
            return comp != null && comp.IsCultivator;
        }
    }

    /// <summary>
    /// Amante del retiro: contento solo en su propia habitación cerrada (+) y molesto con mucha gente alrededor (−).
    /// Etapa 0 = a solas en su cuarto, etapa 1 = rodeado de gente.
    /// </summary>
    public class ThoughtWorker_CVG_Seclusion : ThoughtWorker
    {
        public const int CrowdSize = 4;

        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!p.Spawned) return ThoughtState.Inactive;
            var room = p.GetRoom();
            if (room == null || room.PsychologicallyOutdoors) return ThoughtState.Inactive;
            int others = room.ContainedAndAdjacentThings.OfType<Pawn>().Count(o => o != p && o.RaceProps.Humanlike && !o.Dead);
            if (others >= CrowdSize) return ThoughtState.ActiveAtStage(1);
            if (others == 0 && room.Owners.Contains(p)) return ThoughtState.ActiveAtStage(0);
            return ThoughtState.Inactive;
        }
    }

    /// <summary>
    /// Joven maestro arrogante: orgulloso si su reino es el más alto de la colonia (etapa 0), herido si alguien lo
    /// supera (etapa 1). Solo cuenta cuando ya cultiva.
    /// </summary>
    public class ThoughtWorker_CVG_RealmRank : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            var comp = CompCultivator.For(p);
            if (comp == null || !GameComponent_Convergence.Active || p.Faction != Faction.OfPlayer) return ThoughtState.Inactive;
            int mine = comp.RealmLevel;
            if (mine <= 0) return ThoughtState.Inactive;
            int best = 0;
            foreach (var other in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists)
                if (other != p) best = System.Math.Max(best, CompCultivator.For(other)?.RealmLevel ?? 0);
            if (best > mine) return ThoughtState.ActiveAtStage(1);
            return mine > best ? ThoughtState.ActiveAtStage(0) : ThoughtState.Inactive;
        }
    }
}

namespace Convergence
{
    /// <summary>
    /// Precepto "cultivo obligatorio": pensamiento negativo mientras un cultivador despierto lleva un día sin cultivar
    /// (no tiene el recuerdo de "cultivé hoy", que dura un día). Como los preceptos obligatorios vanilla.
    /// </summary>
    public class ThoughtWorker_Precept_MissedCultivation : RimWorld.ThoughtWorker_Precept
    {
        protected override RimWorld.ThoughtState ShouldHaveThought(Verse.Pawn p)
        {
            if (p.needs?.mood?.thoughts?.memories == null || p.Downed || !p.IsColonist) return false;
            if (!CompCultivator.For(p)?.IsAwakened ?? true) return false;
            var did = Verse.DefDatabase<RimWorld.ThoughtDef>.GetNamedSilentFail("CVG_Cultivation_Obligatory_Self");
            return did != null && p.needs.mood.thoughts.memories.GetFirstMemoryOfDef(did) == null;
        }
    }
}
