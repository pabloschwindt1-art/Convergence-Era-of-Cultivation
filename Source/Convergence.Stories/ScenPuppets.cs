using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Convergence.Stories
{
    /// <summary>
    /// Escenario Maestro de Marionetas ("como el inicio del mecanitor"): el primer colono lleva el hilo del corazón de marioneta y empieza con
    /// marionetas ya enlazadas a él (relación de supervisor vanilla, la misma que usa ScenPart_StartingMech). Solo con Biotech y Biotech Wuxia.
    /// </summary>
    public class ScenPart_StartingPuppets : ScenPart
    {
        public HediffDef link;
        public PawnKindDef puppetKind;
        public int count = 2;

        public override void PostGameStart()
        {
            if (link == null || puppetKind == null || !ModsConfig.BiotechActive) return;
            var master = Find.GameInitData?.startingAndOptionalPawns?.Take(Find.GameInitData.startingPawnCount).FirstOrDefault(p => p != null && p.RaceProps.Humanlike);
            if (master == null) return;
            Equip(master, link, puppetKind, count);
        }

        /// <summary>Pone el hilo en el corazón del maestro y crea las marionetas enlazadas. Devuelve las marionetas creadas.</summary>
        public static List<Pawn> Equip(Pawn master, HediffDef link, PawnKindDef kind, int count)
        {
            var made = new List<Pawn>();
            if (!master.health.hediffSet.HasHediff(link))
            {
                var heart = master.RaceProps.body.GetPartsWithDef(BodyPartDefOf.Heart).FirstOrDefault();
                master.health.AddHediff(link, heart);
            }
            for (int i = 0; i < count; i++)
            {
                var puppet = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, Faction.OfPlayer, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: true));
                if (master.Spawned) GenSpawn.Spawn(puppet, CellFinder.RandomClosewalkCellNear(master.Position, master.Map, 4), master.Map);
                if (master.mechanitor != null && puppet.OverseerSubject != null && master.mechanitor.CanOverseeSubject(puppet))
                    master.relations.AddDirectRelation(PawnRelationDefOf.Overseer, puppet);
                made.Add(puppet);
            }
            return made;
        }

        public override IEnumerable<string> GetSummaryListEntries(string tag)
        {
            if (tag == "PlayerStartsWith" && puppetKind != null)
                yield return puppetKind.LabelCap + " x" + count;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref link, "link");
            Scribe_Defs.Look(ref puppetKind, "puppetKind");
            Scribe_Values.Look(ref count, "count", 2);
        }
    }
}
