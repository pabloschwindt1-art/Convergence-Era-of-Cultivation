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
        private Precept_Ritual ritual;
        private Pawn prisoner;
        private Faction prisonerFaction;
        private int goodwillBefore, round;
        private bool awakened, raised;
        private List<float> savedChances;
        private float savedQuality;

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
                else if (stage == 4) StagePrisoner(map, elapsed);
                else if (stage == 5) StageForcedCeremony(map, elapsed);
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
            // Mide las cirugías y el ritual, no la convivencia: sin colapsos mentales al azar y con comida a mano.
            DebugSettings.enableRandomMentalStates = false;
            Spawn(ThingDefOf.MealSurvivalPack, 60, center + new IntVec3(2, 0, -3), map);
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

            // El médico tiene que poder hacer de médico (algunos rasgos e infancias lo impiden).
            doctor = MakeColonist(map, center);
            for (int i = 0; i < 30 && doctor.WorkTypeIsDisabled(WorkTypeDefOf.Doctor); i++)
            {
                doctor.Destroy();
                doctor = MakeColonist(map, center);
            }
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
                // El donante odia al médico y puede pelearlo: se va del mapa para no frenar el resto de la prueba.
                if (!donor.Dead) donor.DeSpawn();
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
                Note($"seeks medical rest: {HealthAIUtility.ShouldSeekMedicalRest(donor)}, bill should do now: {donor.health.surgeryBills.AnyShouldDoNow}, " +
                    $"bed found: {RestUtility.FindPatientBedFor(donor)?.def?.defName}, altar usable: {RootSurgeryUtility.IsUsable(altar)}, " +
                    $"doctor active: {doctor.workSettings.WorkIsActive(WorkTypeDefOf.Doctor)}, doctor downed: {doctor.Downed}, mental: {doctor.MentalStateDef?.defName}, " +
                    $"donor mental: {donor.MentalStateDef?.defName}, medicine on map: {map.listerThings.ThingsOfDef(ThingDefOf.MedicineIndustrial).Sum(t => t.stackCount)}");
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
                else if (recipient.health.hediffSet.hediffs.Any(h => h is Hediff_Injury))
                    Note("the implant surgery failed (vanilla failure, it left injuries): nothing to check");
                else Check(false, "the implant did nothing and the surgery did not fail (sealed roots left on the map: " +
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
            ritual = ideo.PreceptsListForReading.OfType<Precept_Ritual>().FirstOrDefault(p => p.def == def);
            if (ritual == null)
            {
                ritual = (Precept_Ritual)PreceptMaker.MakePrecept(def);
                ideo.AddPrecept(ritual, init: true, null, def.ritualPatternBase);
            }
            Check(ritual.behavior != null && ritual.outcomeEffect != null, "the ritual precept was added to " + ideo.name);

            var center = map.Center;
            ritualSpot = GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.RitualSpot), center + new IntVec3(0, 0, 5), map);
            ritualSpot.SetFaction(Faction.OfPlayer);

            MakeOfficiant(map);
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
            BeginCeremony(map);
            stage = 3;
            stageStart = Find.TickManager.TicksGame;
        }

        private void MakeOfficiant(Map map)
        {
            officiant = MakeColonist(map, ritualSpot.Position);
            var oc = CompCultivator.For(officiant);
            SpiritRootUtility.ImplantRoot(officiant, DefDatabase<SpiritRootGradeDef>.GetNamed("CVG_RootGrade_Common"), null);
            oc.SyncRealm();
            oc.TrackerFor(CVG_DefOf.CVG_Path_Qi, create: true).AdvanceTo(DefDatabase<CultivationRealmDef>.GetNamed("CVG_Realm_Foundation"), 1f);
        }

        private List<Pawn> Audience() =>
            new List<Pawn> { doctor, recipient }.Where(p => p != null && !p.Dead && !p.Downed && p.Spawned && p.CurrentBed() == null).ToList();

        private void BeginCeremony(Map map)
        {
            var ideo = ritual.ideo;
            var audience = Audience();
            foreach (var p in new[] { officiant, disciple }.Concat(audience))
                if (p.ideo != null && p.Ideo != ideo) p.ideo.SetIdeo(ideo);
            var target = new TargetInfo(ritualSpot);
            var roles = ritual.behavior.def.roles;
            var all = new List<Pawn> { officiant, disciple }.Concat(audience).Distinct().ToList();
            var assignments = new RitualRoleAssignments(ritual, target);
            // Como en la ventana del ritual: roles elegidos, no forzados (vanilla no cuenta los forzados al buscar al orador).
            assignments.Setup(all, new List<Pawn>());
            Check(assignments.TryAssign(officiant, roles.First(r => r.id == "officiant"), out _), "officiant " + officiant.LabelShort + " assigned");
            Check(assignments.TryAssign(disciple, roles.First(r => r.id == "target"), out _), "disciple " + disciple.LabelShort + " assigned");
            foreach (var p in audience) assignments.TryAssignSpectate(p);
            string cant = ritual.behavior.CanStartRitualNow(target, ritual);
            Check(cant.NullOrEmpty(), "the ritual can start" + (cant.NullOrEmpty() ? "" : ": " + cant));
            ritual.behavior.TryExecuteOn(target, officiant, ritual, null, assignments, playerForced: true);
            Check(map.lordManager.lords.Any(l => l.LordJob is LordJob_Ritual), "the ceremony started (a ritual lord is running)");
            Note($"disciple {disciple.LabelShort} root before: {CompCultivator.For(disciple).Root?.grade?.defName ?? "none"}");
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
                StartPrisoner(map);
                return;
            }
            if (elapsed > GenDate.TicksPerDay)
            {
                Note($"officiant job: {officiant.CurJob?.def?.defName}, disciple job: {disciple.CurJob?.def?.defName}");
                Check(false, "the ceremony did not end in a day");
                Finish();
            }
        }

        // ===== 4) Extirpar a un prisionero de una facción neutral, en un altar marcado para prisioneros =====
        private void StartPrisoner(Map map)
        {
            var altarDef = DefDatabase<ThingDef>.GetNamed("CVG_GraftingAltar");
            var center = map.Center;
            // Una celda cerrada, como la arma un jugador: las camas de prisioneros tienen que estar en una habitación.
            var cell = new CellRect(center.x - 11, center.z - 8, 5, 6);
            // Nadie dentro de la celda mientras se levantan las paredes (un muro sobre un pawn lo aplasta).
            foreach (var c in cell.Cells)
                foreach (var t in c.GetThingList(map).ToList())
                {
                    if (t is Pawn pw)
                    {
                        pw.Position = CellFinder.StandableCellNear(center + new IntVec3(0, 0, -6), map, 4f);
                        pw.Notify_Teleported();
                    }
                    else if (t.def.category == ThingCategory.Item || t.def.category == ThingCategory.Building) t.Destroy();
                }
            Note($"doctor before the prisoner: {(doctor.Dead ? "dead" : doctor.Downed ? "downed" : "fine")} at {doctor.Position}");
            foreach (var c in cell.EdgeCells)
            {
                bool isDoor = c.x == cell.minX + 2 && c.z == cell.maxZ;
                var def = isDoor ? ThingDefOf.Door : ThingDefOf.Wall;
                var t = ThingMaker.MakeThing(def, ThingDefOf.BlocksGranite);
                t.SetFaction(Faction.OfPlayer);
                GenSpawn.Spawn(t, c, map);
            }
            altar = (Building_Bed)GenSpawn.Spawn(ThingMaker.MakeThing(altarDef), new IntVec3(cell.minX + 2, 0, cell.minZ + 1), map);
            altar.SetFaction(Faction.OfPlayer);
            altar.ForPrisoners = true;
            altar.Medical = true;
            // Lo mismo que hace el botón de la interfaz: avisar a la habitación para que se vuelva celda de prisión.
            altar.GetDistrict()?.Notify_RoomShapeOrContainedBedsChanged();
            altar.GetRoom()?.Notify_RoomShapeChanged();
            var room = altar.GetRoom();
            Note($"prison cell: walls {cell.EdgeCells.Count(c => c.GetEdifice(map) != null)}/{cell.EdgeCells.Count()}, room cells {room?.CellCount}, " +
                $"touches edge {room?.TouchesMapEdge}, outdoors {room?.PsychologicallyOutdoors}, prison cell {room?.IsPrisonCell}, beds {room?.ContainedBeds.Count()}, " +
                $"for prisoners {altar.ForPrisoners}, edifices: {string.Join(",", cell.EdgeCells.Select(c => c.GetEdifice(map)?.def.defName ?? "-").Distinct())}");
            prisonerFaction = Find.FactionManager.AllFactionsListForReading.Where(f => !f.IsPlayer && !f.Hidden && !f.HostileTo(Faction.OfPlayer)
                && f.def.humanlikeFaction && f.def.basicMemberKind != null && f.CanChangeGoodwillFor(Faction.OfPlayer, -10)).RandomElementWithFallback();
            if (prisonerFaction == null) { Note("no neutral faction: skipping the prisoner"); StartForcedCeremonies(map); return; }
            prisoner = PawnGenerator.GeneratePawn(new PawnGenerationRequest(prisonerFaction.def.basicMemberKind, prisonerFaction, forceGenerateNewPawn: true,
                developmentalStages: DevelopmentalStage.Adult));
            GenSpawn.Spawn(prisoner, altar.Position + new IntVec3(1, 0, 0), map);
            prisoner.guest.SetGuestStatus(Faction.OfPlayer, GuestStatus.Prisoner);
            // Los prisioneros vienen con atención "herbal o peor": como haría el jugador, se sube (y hay herbal por si acaso).
            if (prisoner.playerSettings != null) prisoner.playerSettings.medCare = MedicalCareCategory.Best;
            Note("prisoner medical care: " + prisoner.playerSettings?.medCare);
            SpiritRootUtility.ImplantRoot(prisoner, DefDatabase<SpiritRootGradeDef>.GetNamed("CVG_RootGrade_Superior"),
                new List<QiElementDef> { DefDatabase<QiElementDef>.GetNamed("CVG_Element_Fire") });
            goodwillBefore = prisonerFaction.GoodwillWith(Faction.OfPlayer);
            var recipe = DefDatabase<RecipeDef>.GetNamed("CVG_ExtractSpiritRoot");
            var report = recipe.Worker.AvailableReport(prisoner);
            Check(report.Accepted, "extraction is offered for a prisoner" + (report.Accepted ? "" : ": " + report.Reason));
            HealthCardUtility.CreateSurgeryBill(prisoner, recipe, null, null, sendMessages: false);
            Spawn(ThingDefOf.MedicineIndustrial, 6, center + new IntVec3(-2, 0, -3), map);
            Spawn(ThingDefOf.MedicineHerbal, 6, center + new IntVec3(-3, 0, -3), map);
            Spawn(DefDatabase<ThingDef>.GetNamed("CVG_SpiritJade"), 6, center + new IntVec3(-1, 0, -3), map);
            Note($"prisoner {prisoner.LabelShort} of {prisonerFaction.Name}, goodwill {goodwillBefore}");
            if (doctor.Dead || doctor.Downed || doctor.InMentalState)
            {
                Note("the first doctor cannot work (" + (doctor.Dead ? "dead" : doctor.Downed ? "downed" : doctor.MentalStateDef?.defName) + "): a new one");
                do { doctor = MakeColonist(map, center); } while (doctor.WorkTypeIsDisabled(WorkTypeDefOf.Doctor));
                doctor.skills.GetSkill(SkillDefOf.Medicine).Level = 20;
                doctor.workSettings.SetPriority(WorkTypeDefOf.Doctor, 1);
            }
            stage = 4;
            stageStart = Find.TickManager.TicksGame;
        }

        private void StagePrisoner(Map map, int elapsed)
        {
            if (prisoner.Dead || !CompCultivator.For(prisoner).HasRoot)
            {
                Check(prisoner.Dead || prisoner.health.hediffSet.HasHediff(CVG_RootDefOf.CVG_RootTorn),
                    $"the prisoner lost the root on the prisoners' altar ({elapsed / 2500f:0.0} h, dead: {prisoner.Dead})");
                int after = prisonerFaction.GoodwillWith(Faction.OfPlayer);
                Check(after < goodwillBefore, $"{prisonerFaction.Name} resents it: goodwill {goodwillBefore} -> {after}");
                var sealedRoot = map.listerThings.AllThings.Where(t => t.TryGetComp<CompSealedRoot>() is CompSealedRoot c && c.donor == prisoner.LabelShort).FirstOrDefault();
                Note("prisoner's sealed root: " + (sealedRoot?.LabelNoCount ?? "none (lost)") + (sealedRoot != null ? " | " + sealedRoot.GetInspectString().Replace("\n", " / ") : ""));
                StartForcedCeremonies(map);
                return;
            }
            if (elapsed > GenDate.TicksPerDay * 2)
            {
                Note($"doctor: {(doctor.Dead ? "dead" : doctor.Downed ? "downed" : doctor.CurJob?.def?.defName)}, prisoner job: {prisoner.CurJob?.def?.defName}, prisoner bed: {prisoner.CurrentBed()?.def?.defName}");
                Note("prisoner wounds: " + string.Join(", ", prisoner.health.hediffSet.hediffs.Select(h => h.LabelCap.ToString())));
                Note($"prisoner seeks medical rest: {HealthAIUtility.ShouldSeekMedicalRest(prisoner)}, bill should do now: {prisoner.health.surgeryBills.AnyShouldDoNow}, " +
                    $"bed found: {RestUtility.FindPatientBedFor(prisoner)?.def?.defName}, altar usable: {RootSurgeryUtility.IsUsable(altar)}, " +
                    $"valid bed: {RestUtility.IsValidBedFor(altar, prisoner, prisoner, false, true, false, prisoner.GuestStatus)}, in prison cell: {altar.Position.IsInPrisonCell(map)}, " +
                    $"prisoner downed: {prisoner.Downed}, mental: {prisoner.MentalStateDef?.defName}, doctor active: {doctor.workSettings?.WorkIsActive(WorkTypeDefOf.Doctor)}");
                Check(false, "the prisoner's extraction did not happen in 2 days");
                StartForcedCeremonies(map);
            }
        }

        // ===== 5) Ceremonias con resultado espectacular forzado: despertar una raíz y subir una común =====
        private RitualOutcomeEffectDef OutcomeDef => ritual.outcomeEffect.def;

        private void StartForcedCeremonies(Map map)
        {
            savedChances = OutcomeDef.outcomeChances.Select(o => o.chance).ToList();
            // Mínimas, no cero: vanilla elige por peso y con peso total cero se rompe.
            foreach (var o in OutcomeDef.outcomeChances) o.chance = o.positivityIndex == 2 ? 1f : 0.0001f;
            // El peso de un resultado positivo es chance × calidad: con calidad inicial 1 sale el espectacular.
            savedQuality = OutcomeDef.startingQuality;
            OutcomeDef.startingQuality = 1f;
            round = 0;
            NextForcedCeremony(map);
        }

        private void NextForcedCeremony(Map map)
        {
            round++;
            if (officiant.Dead || officiant.Downed || officiant.InMentalState || !officiant.Spawned || officiant.CurrentBed() != null)
            {
                Note("the officiant cannot go on (" + (officiant.MentalStateDef?.defName ?? "downed or in bed") + "): a new one");
                MakeOfficiant(map);
            }
            disciple = MakeColonist(map, ritualSpot.Position);
            SpiritRootUtility.RemoveRoot(disciple);
            if (round > 1)
                SpiritRootUtility.ImplantRoot(disciple, DefDatabase<SpiritRootGradeDef>.GetNamed("CVG_RootGrade_Common"), null);
            BeginCeremony(map);
            stage = 5;
            stageStart = Find.TickManager.TicksGame;
        }

        private void StageForcedCeremony(Map map, int elapsed)
        {
            if (map.lordManager.lords.Any(l => l.LordJob is LordJob_Ritual))
            {
                if (elapsed > GenDate.TicksPerDay) { Check(false, "a forced ceremony did not end in a day"); EndForced(); }
                return;
            }
            var comp = CompCultivator.For(disciple);
            Note($"spectacular ceremony {round}: {disciple.LabelShort} root after: {comp.Root?.grade?.defName ?? "none"}");
            if (round == 1)
            {
                awakened = comp.Root?.grade?.defName == "CVG_RootGrade_Inferior";
                Check(awakened, "a spectacular ceremony awakens a root in someone born without one");
                NextForcedCeremony(map);
                return;
            }
            if (comp.Root?.grade?.defName == "CVG_RootGrade_Superior") raised = true;
            if (raised || round >= 4)
            {
                Check(raised, $"a spectacular ceremony raised a common root to superior (in {round - 1} tries, 70% each)");
                EndForced();
                return;
            }
            NextForcedCeremony(map);
        }

        private void EndForced()
        {
            for (int i = 0; i < savedChances.Count; i++) OutcomeDef.outcomeChances[i].chance = savedChances[i];
            OutcomeDef.startingQuality = savedQuality;
            Finish();
        }
    }
}
