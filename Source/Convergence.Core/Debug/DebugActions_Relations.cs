using System.Collections.Generic;
using LudeonTK;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>Acciones de debug de relaciones de cultivo (Pareja Dao, hermanos jurados, rival…).</summary>
    public static class DebugActions_Relations
    {
        private static Pawn firstPawn;

        [DebugAction("Convergence", "Convergence: Add cultivation relation", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static List<DebugActionNode> AddCultivationRelation()
        {
            var nodes = new List<DebugActionNode>();
            foreach (var def in DefDatabase<PawnRelationDef>.AllDefsListForReading)
            {
                if (!def.defName.StartsWith("CVG_")) continue;
                var relation = def;
                nodes.Add(new DebugActionNode(def.defName, DebugActionType.ToolMapForPawns)
                {
                    pawnAction = p =>
                    {
                        if (firstPawn == null || firstPawn == p || firstPawn.Dead)
                        {
                            firstPawn = p;
                            Messages.Message("First pawn: " + p.LabelShort + ". Now click the second one.", MessageTypeDefOf.NeutralEvent, false);
                            return;
                        }
                        if (!firstPawn.relations.DirectRelationExists(relation, p)) firstPawn.relations.AddDirectRelation(relation, p);
                        Messages.Message(firstPawn.LabelShort + " + " + p.LabelShort + ": " + relation.label, MessageTypeDefOf.PositiveEvent, false);
                        firstPawn = null;
                    }
                });
            }
            return nodes;
        }
    }
}
