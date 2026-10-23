using System;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Convergence.Sects2
{
    /// <summary>
    /// Autotest de la prueba de concepto S0 (sectas sobre Royalty). Solo corre con <c>-cvgsects2test</c> (y un mapa, p. ej. -quicktest):
    /// títulos honorarios instantáneos al ganar contribución, sin misión de investidura del Imperio, puntos de permiso, los dos favores
    /// (recursos y discípulos), el enfriamiento, el pago con contribución durante el enfriamiento y que gastar no baje el título.
    /// Inerte sin el argumento.
    /// </summary>
    public class GameComponent_Sects2Test : GameComponent
    {
        private static readonly bool Enabled = GenCommandLine.CommandLineArgPassed("cvgsects2test");
        private bool started;
        private int phase = -1;
        private int waitUntil;
        private readonly StringBuilder sb = new StringBuilder("[Convergence] Sects2 S0 self-test");
        private int fails;
        private Map map;
        private Pawn pawn;
        private Faction faction;
        private int skyfallersBefore;

        private static RoyalTitleDef Title(string n) => DefDatabase<RoyalTitleDef>.GetNamed("CVG_Title_FormlessSword_" + n);
        private static RoyalTitlePermitDef Permit(string n) => DefDatabase<RoyalTitlePermitDef>.GetNamed("CVG_Permit_FormlessSword_" + n);

        public GameComponent_Sects2Test(Game game) { }

        private void Check(bool ok, string what)
        {
            sb.Append("\n    " + (ok ? "ok   " : "FAIL ") + what);
            if (!ok) fails++;
        }

        private void Note(string what) => sb.Append("\n- " + what);

        public override void GameComponentTick()
        {
            if (!Enabled) return;
            try
            {
                if (!started)
                {
                    if (Find.CurrentMap == null || Find.TickManager.TicksGame < 200) return;
                    started = true;
                    map = Find.CurrentMap;
                    phase = 0;
                }
                if (phase < 0 || Find.TickManager.TicksGame < waitUntil) return;
                Step();
            }
            catch (Exception e)
            {
                Check(false, "exception: " + e);
                Finish();
            }
        }

        private int Skyfallers() => map.listerThings.AllThings.Count(t => t is Skyfaller);

        private bool BestowingQuest() => Find.QuestManager.QuestsListForReading.Any(q => q.root == QuestScriptDefOf.BestowingCeremony
            && (q.State == QuestState.NotYetAccepted || q.State == QuestState.Ongoing));

        /// <summary>Usa un permiso como lo haría el jugador: la opción del menú y después el objetivo elegido.</summary>
        private bool UsePermit(RoyalTitlePermitDef def, out string reason)
        {
            reason = null;
            var opt = def.Worker.GetRoyalAidOptions(map, pawn, faction).FirstOrDefault();
            if (opt == null) { reason = "no option"; return false; }
            if (opt.Disabled || opt.action == null) { reason = opt.Label; return false; }
            opt.action();
            if (!CellFinder.TryFindRandomCellNear(pawn.Position, map, 10, c => !c.Fogged(map) && c.GetEdifice(map) == null
                && DropCellFinder.CanPhysicallyDropInto(c, map, canRoofPunch: true) && c.Standable(map), out var cell))
            {
                reason = "no drop cell";
                Find.Targeter.StopTargeting();
                return false;
            }
            if (def.Worker is RoyalTitlePermitWorker_Targeted targeted) targeted.OrderForceTarget(new LocalTargetInfo(cell));
            else { reason = "worker is not targeted"; Find.Targeter.StopTargeting(); return false; }
            Find.Targeter.StopTargeting();
            return true;
        }

        private void Step()
        {
            switch (phase)
            {
                case 0:
                {
                    var fdef = DefDatabase<FactionDef>.GetNamed("CVG_Faction_FormlessSword");
                    faction = Find.FactionManager.FirstFactionOfDef(fdef);
                    if (faction == null)
                    {
                        // El quicktest arma el mundo sin las facciones opcionales: la secta llega con la Convergencia, como en una partida vieja.
                        Note("faction not in the quicktest world: firing the Convergence");
                        if (!GameComponent_Convergence.Active) GameComponent_Convergence.Instance?.MarkOccurred();
                        ConvergenceFactionUtility.EnsureFactions();
                        faction = Find.FactionManager.FirstFactionOfDef(fdef);
                    }
                    Check(faction != null, "faction exists");
                    if (faction == null) { Finish(); return; }
                    Check(ModsConfig.RoyaltyActive, "Royalty active");
                    Check(fdef.HasRoyalTitles && fdef.RoyalTitlesAwardableInSeniorityOrderForReading.Count == 4, "4 honorary titles: "
                        + string.Join(", ", fdef.RoyalTitlesAwardableInSeniorityOrderForReading.Select(t => t.defName)));
                    Check(faction.def.royalFavorLabel == "sect contribution" || !faction.def.royalFavorLabel.NullOrEmpty(), "contribution label: " + faction.def.royalFavorLabel);
                    if (faction.HostileTo(Faction.OfPlayer) || faction.PlayerGoodwill < 0)
                        faction.TryAffectGoodwillWith(Faction.OfPlayer, 30 - faction.PlayerGoodwill, canSendMessage: false, canSendHostilityLetter: false);
                    Check(!faction.HostileTo(Faction.OfPlayer), "faction friendly (goodwill " + faction.PlayerGoodwill + ")");
                    pawn = map.mapPawns.FreeColonistsSpawned.FirstOrDefault();
                    Check(pawn?.royalty != null, "colonist with royalty tracker: " + pawn?.LabelShort);
                    if (pawn?.royalty == null) { Finish(); return; }

                    // Contribución → títulos al instante
                    pawn.royalty.GainFavor(faction, 2);
                    Check(pawn.royalty.GetCurrentTitle(faction) == Title("Friend"), "2 contribution -> Friend (" + pawn.royalty.GetCurrentTitle(faction)?.defName + ", left " + pawn.royalty.GetFavor(faction) + ")");
                    pawn.royalty.GainFavor(faction, 6);
                    Check(pawn.royalty.GetCurrentTitle(faction) == Title("Guest"), "+6 -> Honored Guest (" + pawn.royalty.GetCurrentTitle(faction)?.defName + ", left " + pawn.royalty.GetFavor(faction) + ")");
                    Check(!BestowingQuest(), "no Empire bestowing ceremony quest");
                    Check(pawn.royalty.GetPermitPoints(faction) == 2, "permit points " + pawn.royalty.GetPermitPoints(faction));

                    // Elegir los dos favores (como en la ventana de permisos)
                    pawn.royalty.AddPermit(Permit("SpiritResources"), faction);
                    pawn.royalty.AddPermit(Permit("Disciples"), faction);
                    Check(pawn.royalty.HasPermit(Permit("SpiritResources"), faction) && pawn.royalty.HasPermit(Permit("Disciples"), faction), "both favors chosen");

                    // Favor 1: recursos espirituales, gratis
                    skyfallersBefore = Skyfallers();
                    Check(UsePermit(Permit("SpiritResources"), out var r1), "use spirit resources (free)" + (r1 != null ? ": " + r1 : ""));
                    Check(Skyfallers() > skyfallersBefore, "a drop pod is falling");
                    Check(pawn.royalty.GetPermit(Permit("SpiritResources"), faction).OnCooldown, "spirit resources on cooldown");

                    // Favor 2: discípulos, gratis
                    int before = Skyfallers();
                    if (UsePermit(Permit("Disciples"), out var r2)) Check(Skyfallers() > before, "sword disciples dropping in");
                    else if (r2 != null && r2.Contains("BadTemperature".Translate())) Note("call disciples skipped: bad temperature on this map");
                    else Check(false, "use call disciples: " + r2);

                    // En enfriamiento: se paga con contribución y el título no baja
                    pawn.royalty.GainFavor(faction, 5);
                    int favor = pawn.royalty.GetFavor(faction);
                    Check(pawn.royalty.GetCurrentTitle(faction) == Title("Guest"), "5 contribution is not enough for Protector");
                    Check(UsePermit(Permit("SpiritResources"), out var r3), "use spirit resources again (paid)" + (r3 != null ? ": " + r3 : ""));
                    Check(pawn.royalty.GetFavor(faction) == favor - Permit("SpiritResources").royalAid.favorCost, "paid " + Permit("SpiritResources").royalAid.favorCost + " contribution (" + favor + " -> " + pawn.royalty.GetFavor(faction) + ")");
                    Check(pawn.royalty.GetCurrentTitle(faction) == Title("Guest"), "spending contribution keeps the title");

                    // Hasta Anciano Invitado
                    pawn.royalty.GainFavor(faction, 7);
                    Check(pawn.royalty.GetCurrentTitle(faction) == Title("Protector"), "-> Protector (" + pawn.royalty.GetCurrentTitle(faction)?.defName + ")");
                    pawn.royalty.GainFavor(faction, 16);
                    Check(pawn.royalty.GetCurrentTitle(faction) == Title("GuestElder"), "-> Guest Elder (" + pawn.royalty.GetCurrentTitle(faction)?.defName + ")");
                    Check(!BestowingQuest(), "still no bestowing ceremony quest");
                    Check(pawn.royalty.GetPermitPoints(faction) == 2, "2 more permit points: " + pawn.royalty.GetPermitPoints(faction));

                    phase = 1;
                    waitUntil = Find.TickManager.TicksGame + 600;
                    break;
                }
                case 1:
                {
                    // Llegaron los discípulos y el cofre
                    var disciples = map.mapPawns.AllPawnsSpawned.Count(p => p.Faction == faction);
                    Note("sect pawns on the map after the drops: " + disciples);
                    Check(map.listerThings.ThingsOfDef(DefDatabase<ThingDef>.GetNamed("CVG_SpiritStone")).Sum(t => t.stackCount) >= 40, "spirit stones delivered");
                    Finish();
                    break;
                }
            }
        }

        private void Finish()
        {
            phase = -1;
            sb.Append(fails == 0 ? "\n  ALL OK" : "\n  FAILURES: " + fails);
            if (fails == 0) Log.Message(sb.ToString()); else Log.Error(sb.ToString());
        }
    }
}
