using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Convergence
{
    /// <summary>
    /// Autotest de rasgos y relaciones (con <c>-cvgselftest</c>): rasgos CVG cargados, cultivo dual de una relación con
    /// <see cref="DefModExtension_DualCultivation"/> y vida extra por rasgo.
    /// </summary>
    public static class TraitSelfTest
    {
        public static void Run()
        {
            var map = Find.CurrentMap;
            var sb = new StringBuilder("[Convergence] Trait self-test");
            int fails = 0;
            var traits = DefDatabase<TraitDef>.AllDefsListForReading.Where(t => t.defName.StartsWith("CVG_")).ToList();
            sb.Append($"\n  CVG traits: {traits.Count}, destinies: {traits.Count(t => t.exclusionTags.Contains("CVG_Destiny"))}");
            var dual = DefDatabase<PawnRelationDef>.GetNamedSilentFail("CVG_DaoPartner");
            if (map != null && dual != null)
            {
                var a = Generate(map);
                var b = Generate(map);
                a.relations.AddDirectRelation(dual, b);
                float alone = a.GetStatValue(CVG_DefOf.CVG_CultivationSpeed, cacheStaleAfterTicks: 0);
                b.jobs.StartJob(JobMaker.MakeJob(JobDefOf.Meditate, b.Position), JobCondition.InterruptForced);
                float together = TraitMechanics.DualFactor(a, out var partner);
                sb.Append($"\n  {dual.defName}: relation {a.relations.DirectRelationExists(dual, b)}, dual factor x{together:0.##} (partner {partner?.LabelShort ?? "none"}), speed {alone:0.##}");
                if (!b.relations.DirectRelationExists(dual, a)) { sb.Append("  FAIL relation not reflexive"); fails++; }
                a.Destroy();
                b.Destroy();
            }
            var near = DefDatabase<PawnRelationDef>.AllDefsListForReading.FirstOrDefault(r => r.GetModExtension<DefModExtension_DualCultivation>() is DefModExtension_DualCultivation e && !e.requireBothCultivating);
            if (map != null && near != null)
            {
                var a = Generate(map);
                var b = Generate(map);
                a.relations.AddDirectRelation(near, b);
                float f = TraitMechanics.DualFactor(a, out _);
                sb.Append($"\n  {near.defName}: dual factor x{f:0.##} with the other nearby");
                if (f <= 1f) { sb.Append("  FAIL no dual factor"); fails++; }
                a.Destroy();
                b.Destroy();
            }
            var lifeTrait = traits.FirstOrDefault(t => t.HasModExtension<DefModExtension_ExtraLives>());
            if (map != null && lifeTrait != null && GameComponent_Convergence.Active)
            {
                var p = Generate(map);
                // Sin genes de linaje con vidas extra (nueve vidas del zorro): si no, el gen salva la segunda muerte y el test falla al azar
                if (p.genes != null)
                    foreach (var g in p.genes.GenesListForReading.Where(g => g.def.HasModExtension<DefModExtension_ExtraLives>()).ToList())
                        p.genes.RemoveGene(g);
                p.story.traits.GainTrait(new Trait(lifeTrait, 0, forced: true));
                p.Kill(null);
                sb.Append($"\n  {lifeTrait.defName}: first death -> dead {p.Dead}");
                if (p.Dead) { sb.Append("  FAIL trait extra life"); fails++; }
                else
                {
                    p.Kill(null);
                    sb.Append($", second death -> dead {p.Dead}");
                    if (!p.Dead) { sb.Append("  FAIL recharge ignored"); fails++; }
                }
                if (!p.Destroyed) p.Destroy();
                else p.Corpse?.Destroy();
            }
            sb.Append(fails == 0 ? "\n  ALL OK" : "\n  FAILURES: " + fails);
            if (fails == 0) Log.Message(sb.ToString());
            else Log.Error(sb.ToString());
        }

        private static Pawn Generate(Map map)
        {
            var pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayer, forceGenerateNewPawn: true,
                developmentalStages: DevelopmentalStage.Adult));
            GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(map.Center, map, 3), map);
            return pawn;
        }
    }
}
