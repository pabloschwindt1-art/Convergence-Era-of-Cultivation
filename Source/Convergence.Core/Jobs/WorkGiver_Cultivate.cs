using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace Convergence
{
    /// <summary>Busca un lugar de cultivo libre (cualquier edificio con <see cref="DefModExtension_CultivationSpot"/>).</summary>
    public class WorkGiver_Cultivate : WorkGiver_Scanner
    {
        private static List<ThingDef> spotDefs;

        private static List<ThingDef> SpotDefs => spotDefs ??=
            DefDatabase<ThingDef>.AllDefsListForReading.Where(d => d.HasModExtension<DefModExtension_CultivationSpot>()).ToList();

        public override PathEndMode PathEndMode => PathEndMode.OnCell;

        public override Danger MaxPathDanger(Pawn pawn) => Danger.Some;

        public override bool ShouldSkip(Pawn pawn, bool forced = false) =>
            !(CompCultivator.For(pawn)?.CanCultivateNow ?? false);

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn) => AllSpots(pawn.Map);

        /// <summary>Todos los lugares de cultivo del mapa.</summary>
        public static IEnumerable<Thing> AllSpots(Map map)
        {
            var defs = SpotDefs;
            for (int i = 0; i < defs.Count; i++)
                foreach (var t in map.listerThings.ThingsOfDef(defs[i]))
                    yield return t;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!t.def.HasModExtension<DefModExtension_CultivationSpot>()) return false;
            if (t.Faction != pawn.Faction || t.IsForbidden(pawn) || t.IsBurning()) return false;
            if (!(CompCultivator.For(pawn)?.CanCultivateNow ?? false)) return false;
            if (!forced && CultivatorAI.InDanger(pawn, 20f)) return false;
            // Puestos asignados a un colono (como el lugar de meditación): solo su dueño.
            var owners = t.TryGetComp<CompAssignableToPawn>();
            if (owners != null && owners.AssignedPawnsForReading.Count > 0 && !owners.AssignedPawnsForReading.Contains(pawn)) return false;
            return pawn.CanReserveSittableOrSpot(t.Position, t, forced)
                && pawn.CanReach(t, PathEndMode.OnCell, Danger.Some);
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false) =>
            JobMaker.MakeJob(CVG_DefOf.CVG_Cultivate, t);
    }
}
