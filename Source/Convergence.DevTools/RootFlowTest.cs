using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Raíces que cambian, como las juega el jugador (docs/modulos/RAICES.md). Con <c>-cvgrootflow</c> (y <c>-quicktest</c>):
    /// 1) un médico extirpa la raíz de un colono en el altar del Dao (factura de cirugía, el paciente va solo al altar);
    /// 2) la raíz sellada que salió se implanta en otro colono en la mesa de cirugía de qi (deja la costura de la máquina);
    /// 3) una ceremonia de purificación de raíz completa: precepto en la ideología, oficiante en Fundación, discípulo y público.
    /// Registra cada paso y cierra el juego. Sin el argumento no hace nada.
    /// </summary>
    public class GameComponent_RootFlowTest : GameComponent
    {
        private static readonly bool Enabled = GenCommandLine.CommandLineArgPassed("cvgrootflow");
        private readonly StringBuilder sb = new StringBuilder("[Convergence] Root flow test: altar, qi table and purification ritual");
        private int fails, stage, stageStart;
        private Pawn doctor, donor, recipient, officiant, disciple;
        private Building_Bed altar, table;
        private Thing ritualSpot;
        private SpiritRootGradeDef donorGrade;
        private int lettersBefore;

        public GameComponent_RootFlowTest(Game game) { }

        private void Check(bool ok, string what)
        {
            sb.Append("\n    " + (ok ? "ok   " : "FAIL ") + what);
            if (!ok) fails++;
        }

        private void Note(string what) => sb.Append("\n    note " + what);

        private void Finish()
        {
            sb.Append(fails == 0 ? "\n  ALL OK" : "\n  FAILURES: " + fails);
            if (fails == 0) Log.Message(sb.ToString()); else Log.Error(sb.ToString());
            stage = 99;
            Root.Shutdown();
        }

        public override void GameComponentUpdate()
        {
            if (!Enabled || stage == 0 || stage == 99 || Find.CurrentMap == null) return;
            var tm = Find.TickManager;
            tm.CurTimeSpeed = TimeSpeed.Paused;
            float start = UnityEngine.Time.realtimeSinceStartup;
            while (UnityEngine.Time.realtimeSinceStartup - start < 0.25f && stage != 99)
            {
                try { tm.DoSingleTick(); }
                catch (System.Exception e) { Log.Error("[Convergence] root flow tick exception: " + e); }
            }
        }

        public override void GameComponentTick()
        {
            if (!Enabled || stage == 99 || Find.CurrentMap == null) return;
            var map = Find.CurrentMap;
            int now = Find.TickManager.TicksGame;
            try
            {
                if (stage == 0)
                {
                    if (now < 300) return;
                    Setup(map);
                    return;
                }
                if (now % 250 != 0) return;
                int elapsed = now - stageStart;
                if (stage == 1) StageExtract(map, elapsed);
                else if (stage == 2) StageImplant(map, elapsed);
                else if (stage == 3) StageRitual(map, elapsed);
            }
            catch (System.Exception e)
            {
                Check(false, "exception: " + e);
                Finish();
            }
        }

        private Pawn MakeColonist(Map map, IntVec3 near)
        {
            var p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayer, forceGenerateNewPawn: true,
                developmentalStages: DevelopmentalStage.Adult, forcedXenotype: XenotypeDefOf.Baseliner));
            GenSpawn.Spawn(p, CellFinder.StandableCellNear(near, map, 3f), map);
            if (p.workSettings != null)
            {
                p.workSettings.EnableAndInitialize();
                foreach (var w in DefDatabase<WorkTypeDef>.AllDefsListForReading) if (!p.WorkTypeIsDisabled(w)) p.workSettings.SetPriority(w, 0);
                // "Paciente" es lo que lleva a la cama a quien tiene una cirugía pendiente.
                foreach (var w in new[] { "Patient", "PatientBedRest" })
                    if (DefDatabase<WorkTypeDef>.GetNamedSilentFail(w) is WorkTypeDef wt && !p.WorkTypeIsDisabled(wt)) p.workSettings.SetPriority(wt, 1);
            }
            return p;
        }

        private void Spawn(ThingDef def, int count, IntVec3 near, Map map)
        {
            var t = ThingMaker.MakeThing(def);
            t.stackCount = count;
            GenPlace.TryPlaceThing(t, near, map, ThingPlaceMode.Near);
        }

        private void FinishResearch(string defName)
        {
            var r = DefDatabase<ResearchProjectDef>.GetNamedSilentFail(defName);
            if (r == null) return;
            var todo = new List<ResearchProjectDef> { r };
            for (int i = 0; i < todo.Count; i++)
                foreach (var p in todo[i].prerequisites ?? new List<ResearchProjectDef>()) if (!todo.Contains(p)) todo.Add(p);
            foreach (var p in todo) if (!p.IsFinished) Find.ResearchManager.FinishProject(p, doCompletionDialog: false);
        }

        private void Setup(Map map)
        {
            Find.Storyteller.storytellerComps.Clear();
            Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
            if (!GameComponent_Convergence.Active) GameComponent_Convergence.Instance?.TryFire(map);
            var center = TestArena.ClearAtCenter(map, 30, 20);
            // Los colonos del inicio no participan: solo los de la prueba trabajan.
            foreach (var p in map.mapPawns.FreeColonistsSpawned.ToList())
                if (p.workSettings != null) foreach (var w in DefDatabase<WorkTypeDef>.AllDefsListForReading) if (!p.WorkTypeIsDisabled(w)) p.workSettings.SetPriority(w, 0);

            FinishResearch("CVG_Research_RootSurgery");
            FinishResearch("CVG_Research_RootEngineering");

            var altarDef = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_GraftingAltar");
            if (altarDef == null) { Check(false, "Grafts is not loaded"); Finish(); return; }
            altar = (Building_Bed)GenSpawn.Spawn(ThingMaker.MakeThing(altarDef), center + new IntVec3(-6, 0, 0), map);
            altar.SetFaction(Faction.OfPlayer);
            altar.Medical = true;

            doctor = MakeColonist(map, center);
            doctor.skills.GetSkill(SkillDefOf.Medicine).Level = 20;
            doctor.workSettings.SetPriority(WorkTypeDefOf.Doctor, 1);
            donor = MakeColonist(map, center + new IntVec3(-4, 0, 3));
            SpiritRootUtility.ImplantRoot(donor, DefDatabase<SpiritRootGradeDef>.GetNamed("CVG_RootGrade_Common"),
                new List<QiElementDef> { DefDatabase<QiElementDef>.GetNamed("CVG_Element_Water") });
            donorGrade = CompCultivator.For(donor).Root?.grade;
            Spawn(ThingDefOf.MedicineIndustrial, 10, center + new IntVec3(0, 0, -3), map);
            Spawn(DefDatabase<ThingDef>.GetNamed("CVG_SpiritJade"), 10, center + new IntVec3(1, 0, -3), map);

            var recipe = DefDatabase<RecipeDef>.GetNamed("CVG_ExtractSpiritRoot");
            var report = recipe.Worker.AvailableReport(donor);
            Check(report.Accepted, "extraction is available with the altar and the research" + (report.Accepted ? "" : ": " + report.Reason));
            HealthCardUtility.CreateSurgeryBill(donor, recipe, null, null, sendMessages: false);
            Check(donor.health.surgeryBills.Count == 1, "surgery bill queued on " + donor.LabelShort);
            stage = 1;
            stageStart = Find.TickManager.TicksGame;
        }

        private void StageExtract(Map map, int elapsed)
        {
            if (donor.Dead || !CompCultivator.For(donor).HasRoot)
            {
                Check(donor.Dead || donor.health.hediffSet.HasHediff(CVG_RootDefOf.CVG_RootTorn), $"{donor.LabelShort} lost the root on the altar after {elapsed / 2500f:0.0} h (dead: {donor.Dead})");
                var sealedRoot = map.listerThings.AllThings.FirstOrDefault(t => t.TryGetComp<CompSealedRoot>() != null);
                Note("sealed root: " + (sealedRoot?.LabelNoCount ?? "none (lost in the surgery)"));
                if (!donor.Dead)
                {
                    var torn = donor.needs?.mood?.thoughts.memories.GetFirstMemoryOfDef(CVG_RootDefOf.CVG_Thought_RootTorn);
                    Check(torn != null, "the donor remembers losing the root");
                    Check(donor.relations.OpinionOf(doctor) < 0, $"the donor hates the surgeon (opinion {donor.relations.OpinionOf(doctor)})");
                }
                // Paso 2: la mesa de qi con energía, y otro colono para recibir la raíz.
                var tableDef = DefDatabase<ThingDef>.GetNamedSilentFail("CVG_QT_QiSurgeryTable");
                if (tableDef == null) { Check(false, "Qi-Tech is not loaded"); Finish(); return; }
                if (sealedRoot == null)
                {
                    sealedRoot = SealedRootUtility.Make(donorGrade, null, "test");
                    GenPlace.TryPlaceThing(sealedRoot, doctor.Position, map, ThingPlaceMode.Near);
                    Note("made a sealed root to keep testing the implant");
                }
                var center = map.Center;
                altar.Destroy();
                table = (Building_Bed)GenSpawn.Spawn(ThingMaker.MakeThing(tableDef), center + new IntVec3(6, 0, 0), map);
                table.SetFaction(Faction.OfPlayer);
                table.Medical = true;
                var power = table.TryGetComp<CompPowerTrader>();
                if (power != null) power.PowerOn = true;
                Check(RootSurgeryUtility.IsUsable(table), "the powered qi surgery table accepts root surgery");
                if (power != null)
                {
                    power.PowerOn = false;
                    Check(!RootSurgeryUtility.IsUsable(table), "without power it does not");
                    power.PowerOn = true;
                }
                recipient = MakeColonist(map, center + new IntVec3(4, 0, 3));
                SpiritRootUtility.RemoveRoot(recipient);
                var recipe = DefDatabase<RecipeDef>.GetNamed("CVG_ImplantSpiritRoot");
                HealthCardUtility.CreateSurgeryBill(recipient, recipe, null, null, sendMessages: false);
                stage = 2;
                stageStart = Find.TickManager.TicksGame;
                return;
            }
            if (elapsed > GenDate.TicksPerDay * 2)
            {
                Note($"doctor job: {doctor.CurJob?.def?.defName}, donor job: {donor.CurJob?.def?.defName}, donor bed: {donor.CurrentBed()?.def?.defName}");
                Check(false, "the extraction did not happen in 2 days");
                Finish();
            }
        }

        private void StageImplant(Map map, int elapsed)
        {
            var power = table.TryGetComp<CompPowerTrader>();
            if (power != null && !power.PowerOn) power.PowerOn = true;
            var comp = CompCultivator.For(recipient);
            if (recipient.health.surgeryBills.Count == 0)
            {
                if (comp.HasRoot)
                {
                    Check(recipient.health.hediffSet.HasHediff(DefDatabase<HediffDef>.GetNamed("CVG_QT_ForeignRootMachine"))
                          && recipient.health.hediffSet.HasHediff(DefDatabase<HediffDef>.GetNamed("CVG_QT_QiSeam")),
                        $"{recipient.LabelShort} got a {comp.Root.grade.label} on the qi table, with the machine's foreign root and qi seam ({elapsed / 2500f:0.0} h)");
                    Check(recipient.genes == null || recipient.genes.GenesListForReading.Any(g => g.def.HasModExtension<DefModExtension_SpiritRoot>() && recipient.genes.IsXenogene(g)),
                        "the implanted root is a xenogene");
                }
                else Check(false, "the implant did nothing (sealed roots left on the map: " +
                    map.listerThings.AllThings.Count(t => t.TryGetComp<CompSealedRoot>() != null) + ")");
                StartRitual(map);
                return;
            }
            if (elapsed > GenDate.TicksPerDay * 2)
            {
                Note($"doctor job: {doctor.CurJob?.def?.defName}, recipient job: {recipient.CurJob?.def?.defName}, bed: {recipient.CurrentBed()?.def?.defName}");
                Check(false, "the implant did not happen in 2 days");
                Finish();
            }
        }

        private void StartRitual(Map map)
        {
            var def = DefDatabase<PreceptDef>.GetNamed("CVG_RootPurificationCeremony");
            var ideo = Faction.OfPlayer.ideos?.PrimaryIdeo;
            if (ideo == null) { Check(false, "the player has no ideoligion"); Finish(); return; }
            var ritual = ideo.PreceptsListForReading.OfType<Precept_Ritual>().FirstOrDefault(p => p.def == def);
            if (ritual == null)
            {
                ritual = (Precept_Ritual)PreceptMaker.MakePrecept(def);
                ideo.AddPrecept(ritual, init: true, null, def.ritualPatternBase);
            }
            Check(ritual.behavior != null && ritual.outcomeEffect != null, "the ritual precept was added to " + ideo.name);

            var center = map.Center;
            ritualSpot = GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.RitualSpot), center + new IntVec3(0, 0, 5), map);
            ritualSpot.SetFaction(Faction.OfPlayer);

            officiant = MakeColonist(map, ritualSpot.Position);
            var oc = CompCultivator.For(officiant);
            SpiritRootUtility.ImplantRoot(officiant, DefDatabase<SpiritRootGradeDef>.GetNamed("CVG_RootGrade_Common"), null);
            oc.SyncRealm();
            oc.TrackerFor(CVG_DefOf.CVG_Path_Qi, create: true).AdvanceTo(DefDatabase<CultivationRealmDef>.GetNamed("CVG_Realm_Foundation"), 1f);
            // Un discípulo descansado: el donante sigue en cama tras la cirugía y vanilla no deja asignarlo.
            disciple = MakeColonist(map, ritualSpot.Position);
            SpiritRootUtility.RemoveRoot(disciple);
            var audience = new List<Pawn> { doctor, recipient }.Where(p => p != null && !p.Dead && !p.Downed).ToList();
            foreach (var p in new[] { officiant, disciple }.Concat(audience))
                if (p.ideo != null && p.Ideo != ideo) p.ideo.SetIdeo(ideo);

            var target = new TargetInfo(ritualSpot);
            var roles = ritual.behavior.def.roles;
            var oRole = roles.First(r => r.id == "officiant");
            var tRole = roles.First(r => r.id == "target");
            Check(oRole.AppliesToPawn(officiant, out string why1, target, null, null, ritual), "a Foundation cultivator can officiate" + (why1.NullOrEmpty() ? "" : ": " + why1));
            Check(!oRole.AppliesToPawn(doctor, out string why2, target, null, null, ritual) && !why2.NullOrEmpty(), "a mortal cannot: " + why2);
            Check(tRole.AppliesToPawn(disciple, out string why3, target, null, null, ritual), $"{disciple.LabelShort} can be purified" + (why3.NullOrEmpty() ? "" : ": " + why3));

            var all = new List<Pawn> { officiant, disciple }.Concat(audience).Distinct().ToList();
            var assignments = new RitualRoleAssignments(ritual, target);
            // Como en la ventana del ritual: roles elegidos, no forzados (vanilla no cuenta los forzados al buscar al orador).
            assignments.Setup(all, new List<Pawn>());
            Check(assignments.TryAssign(officiant, oRole, out _) && assignments.TryAssign(disciple, tRole, out _), "officiant and disciple assigned");
            foreach (var p in audience) assignments.TryAssignSpectate(p);
            string cant = ritual.behavior.CanStartRitualNow(target, ritual);
            Check(cant.NullOrEmpty(), "the ritual can start" + (cant.NullOrEmpty() ? "" : ": " + cant));
            lettersBefore = Find.LetterStack.LettersListForReading.Count + Find.Archive.ArchivablesListForReading.Count;
            ritual.behavior.TryExecuteOn(target, officiant, ritual, null, assignments, playerForced: true);
            Check(map.lordManager.lords.Any(l => l.LordJob is LordJob_Ritual), "the ceremony started (a ritual lord is running)");
            Note($"disciple root before: {CompCultivator.For(disciple).Root?.grade?.defName ?? "none"}");
            stage = 3;
            stageStart = Find.TickManager.TicksGame;
        }

        private void StageRitual(Map map, int elapsed)
        {
            var comp = CompCultivator.For(disciple);
            bool running = map.lordManager.lords.Any(l => l.LordJob is LordJob_Ritual);
            if (!running)
            {
                bool applied = comp.LastRootRitualTick >= stageStart;
                Check(applied, $"the ceremony ended and applied its outcome ({elapsed / 2500f:0.0} h)");
                var letter = Find.LetterStack.LettersListForReading.LastOrDefault();
                Note("last letter: " + (letter?.Label.ToString() ?? "-") + " | " + (letter is StandardLetter s ? s.Text.ToString().Split('\n').FirstOrDefault() : ""));
                Note($"disciple root after: {comp.Root?.grade?.defName ?? "none"}");
                Check(!RootPurification.CanTarget(disciple, out string why) && !why.NullOrEmpty(), "the disciple must wait a year now: " + why);
                Finish();
                return;
            }
            if (elapsed > GenDate.TicksPerDay)
            {
                Note($"officiant job: {officiant.CurJob?.def?.defName}, disciple job: {disciple.CurJob?.def?.defName}");
                Check(false, "the ceremony did not end in a day");
                Finish();
            }
        }
    }
}
