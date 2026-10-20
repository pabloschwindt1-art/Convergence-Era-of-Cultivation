using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Convergence.Stories
{
    /// <summary>
    /// Prueba del botón "Mecas" de la barra (con <c>-cvgmechtabtest</c> y <c>-quicktest</c>): replica el caso de un colono que
    /// se instala el mecaenlace. Mira si el botón se ve (1) sin mecas, (2) con el mecaenlace y sin mecas, (3) con un meca de
    /// vanilla de su facción. Vanilla lo oculta mientras no haya ningún meca en la colonia. Sin el argumento no hace nada.
    /// </summary>
    public class GameComponent_MechTabTest : GameComponent
    {
        private static readonly bool Enabled = GenCommandLine.CommandLineArgPassed("cvgmechtabtest");
        private int state, nextTick;
        private readonly StringBuilder sb = new StringBuilder("[Convergence] Mech tab test");
        private int fails;

        public GameComponent_MechTabTest(Game game) { }

        private static MainButtonDef MechsButton => DefDatabase<MainButtonDef>.GetNamedSilentFail("Mechs");

        private static string Bar()
        {
            var worker = MechsButton?.Worker;
            var all = DefDatabase<MainButtonDef>.AllDefsListForReading.Where(d => d.Worker.Visible).OrderBy(d => d.order).Select(d => d.defName);
            return $"Mechs button visible {worker?.Visible}; bar: {string.Join(" ", all)}";
        }

        private void Check(bool ok, string label)
        {
            if (!ok) fails++;
            sb.Append($"\n    {(ok ? "ok  " : "FAIL")} {label}");
        }

        public override void GameComponentTick()
        {
            if (!Enabled || state >= 4 || Find.CurrentMap == null) return;
            int now = Find.TickManager.TicksGame;
            var map = Find.CurrentMap;
            var colonist = map.mapPawns.FreeColonistsSpawned.FirstOrDefault();
            if (colonist == null) return;
            if (state == 0 && now >= 300)
            {
                var anyMech = map.mapPawns.PawnsInFaction(Faction.OfPlayer).Any(p => p.RaceProps.IsMechanoid);
                sb.Append($"\n    start: mechs in colony {anyMech}, {Bar()}");
                Check(MechsButton != null, "the Mechs button exists");
                var mechlink = DefDatabase<HediffDef>.GetNamed("MechlinkImplant");
                colonist.health.AddHediff(mechlink, colonist.health.hediffSet.GetBrain());
                PawnComponentsUtility.AddAndRemoveDynamicComponents(colonist);
                nextTick = now + 400; state = 1;
            }
            else if (state == 1 && now >= nextTick)
            {
                sb.Append($"\n    after mechlink: {colonist.LabelShort} is mechanitor {colonist.mechanitor != null}, bandwidth {colonist.GetStatValue(StatDefOf.MechBandwidth):0}, {Bar()}");
                Check(colonist.mechanitor != null, "the colonist with the mechlink is a mechanitor");
                Check(MechsButton.Worker.Visible, "the Mechs button shows for a mechanitor even without mechs");
                var kind = DefDatabase<PawnKindDef>.GetNamed("Mech_Lifter");
                var mech = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, Faction.OfPlayer, forceGenerateNewPawn: true));
                GenSpawn.Spawn(mech, CellFinder.RandomClosewalkCellNear(colonist.Position, map, 4), map);
                if (colonist.mechanitor != null && mech.OverseerSubject != null && colonist.mechanitor.CanOverseeSubject(mech))
                    colonist.relations.AddDirectRelation(PawnRelationDefOf.Overseer, mech);
                nextTick = now + 400; state = 2;
            }
            else if (state == 2 && now >= nextTick)
            {
                sb.Append($"\n    after a vanilla mech: overseen {colonist.mechanitor?.OverseenPawns.Count}, {Bar()}");
                Check(MechsButton.Worker.Visible, "the Mechs button shows once there is a mech");
                // lo mismo con una marioneta de Convergence, si está el módulo
                var puppet = DefDatabase<PawnKindDef>.GetNamedSilentFail("CVG_Puppet_Wood");
                if (puppet != null)
                {
                    foreach (var m in map.mapPawns.PawnsInFaction(Faction.OfPlayer).Where(p => p.RaceProps.IsMechanoid).ToList()) m.Destroy();
                    nextTick = now + 400; state = 3;
                    sb.Append("\n    removed the vanilla mech; spawning a puppet next");
                    var p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(puppet, Faction.OfPlayer, forceGenerateNewPawn: true));
                    GenSpawn.Spawn(p, CellFinder.RandomClosewalkCellNear(colonist.Position, map, 4), map);
                }
                else state = 4;
                if (state == 4) Finish();
            }
            else if (state == 3 && now >= nextTick)
            {
                sb.Append($"\n    with only a puppet: {Bar()}");
                Check(MechsButton.Worker.Visible, "the Mechs button shows with a puppet too");
                state = 4; Finish();
            }
        }

        private void Finish()
        {
            if (fails == 0) Log.Message(sb + "\n  ALL OK"); else Log.Error(sb + $"\n  FAILURES: {fails}");
        }
    }
}
