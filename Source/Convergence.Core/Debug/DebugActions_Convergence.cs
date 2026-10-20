using System.Collections.Generic;
using LudeonTK;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>Acciones del modo desarrollador para probar rápido (menú de debug → "Convergence").</summary>
    public static class DebugActions_Convergence
    {
        [DebugAction("Convergence", "Convergence: Trigger Convergence", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void TriggerConvergence()
        {
            if (!GameComponent_Convergence.Instance.TryFire(Find.CurrentMap))
                Messages.Message("Convergence already happened (or could not fire).", MessageTypeDefOf.RejectInput, false);
        }

        [DebugAction("Convergence", "Convergence: Give random spirit root", actionType = DebugActionType.ToolMapForPawns,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void GiveRandomRoot(Pawn p)
        {
            var comp = CompCultivator.For(p);
            if (comp == null || comp.HasRoot) return;
            CultivationGenerator.GiveRoot(p, CultivationGenerator.RandomGrade(), CultivationGenerator.RandomElements());
            comp.Sync();
            DebugActionsUtility.DustPuffFrom(p);
        }

        [DebugAction("Convergence", "Convergence: Add cultivation progress", actionType = DebugActionType.ToolMapForPawns,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void AddCultivationProgress(Pawn p)
        {
            var tracker = MainTracker(p);
            if (tracker == null) return;
            // Una etapa completa por clic.
            tracker.AddProgress(tracker.Realm.progressPerStage);
            DebugActionsUtility.DustPuffFrom(p);
        }

        /// <summary>Tracker principal para las acciones de debug: el del qi si cultiva qi; si no, el del cuerpo (si lo empezó).</summary>
        private static RealmTracker MainTracker(Pawn p)
        {
            var comp = CompCultivator.For(p);
            if (comp == null) return null;
            RealmTracker tracker = null;
            if (comp.IsQiCultivator) tracker = comp.Realm;
            else if (comp.IsBodyCultivator) tracker = comp.TrackerFor(CVG_DefOf.CVG_Path_Body);
            return tracker != null && tracker.EnsureStarted() ? tracker : null;
        }

        [DebugAction("Convergence", "Convergence: Fill qi", actionType = DebugActionType.ToolMapForPawns,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void FillQi(Pawn p)
        {
            var comp = CompCultivator.For(p);
            if (comp == null) return;
            // Llena el recurso de todos los caminos que sigue (qi, sangre-qi…).
            foreach (var tracker in comp.ActiveTrackers)
                if (tracker.Started) tracker.Resource.Fill();
            DebugActionsUtility.DustPuffFrom(p);
        }

        [DebugAction("Convergence", "Convergence: Force breakthrough", actionType = DebugActionType.ToolMapForPawns,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ForceBreakthrough(Pawn p)
        {
            var tracker = MainTracker(p);
            if (tracker == null) return;
            var next = tracker.NextRealm;
            if (next == null)
            {
                Messages.Message("Already at the highest realm.", MessageTypeDefOf.RejectInput, false);
                return;
            }
            // Sin tirada ni tribulación: sube directamente.
            BreakthroughUtility.Ascend(p, next, 1f);
        }

        [DebugAction("Convergence", "Convergence: Start tribulation", actionType = DebugActionType.ToolMapForPawns,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void StartTribulation(Pawn p)
        {
            var comp = CompCultivator.For(p);
            if (comp == null || !comp.IsQiCultivator || p.Map == null) return;
            MapComponent_Tribulation.For(p.Map).DebugBegin(p);
        }

        [DebugAction("Convergence", "Convergence: Reset breakthrough cooldown", actionType = DebugActionType.ToolMapForPawns,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ResetBreakthroughCooldown(Pawn p)
        {
            var tracker = MainTracker(p);
            if (tracker == null) return;
            tracker.StartBreakthroughCooldown(0);
            DebugActionsUtility.DustPuffFrom(p);
        }

        [DebugAction("Convergence", "Convergence: Start body tempering", actionType = DebugActionType.ToolMapForPawns,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void StartBodyTempering(Pawn p)
        {
            var comp = CompCultivator.For(p);
            if (comp == null) return;
            // Como si hubiera comido carne de bestia: empieza el camino del cuerpo (solo sin raíz) y llena la sangre-qi.
            var tracker = comp.StartPath(CVG_DefOf.CVG_Path_Body);
            if (tracker == null)
            {
                Messages.Message("Cannot follow the body path (needs the Convergence, no spirit root, adult).", MessageTypeDefOf.RejectInput, false);
                return;
            }
            tracker.Resource.Fill();
            DebugActionsUtility.DustPuffFrom(p);
        }

        [DebugAction("Convergence", "Convergence: Awaken spirit beast", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static List<DebugActionNode> AwakenSpiritBeast()
        {
            var nodes = new List<DebugActionNode>();
            for (int level = 1; level <= SpiritBeastUtility.MaxLevel; level++)
            {
                int l = level;
                nodes.Add(new DebugActionNode("level " + l, DebugActionType.ToolMapForPawns)
                {
                    pawnAction = p =>
                    {
                        if (!p.RaceProps.Animal) return;
                        SpiritBeastUtility.Awaken(p, l);
                        DebugActionsUtility.DustPuffFrom(p);
                    }
                });
            }
            return nodes;
        }

        [DebugAction("Convergence", "Convergence: Give bloodline", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static List<DebugActionNode> GiveBloodline()
        {
            var nodes = new List<DebugActionNode>();
            foreach (var bloodline in DefDatabase<BloodlineDef>.AllDefsListForReading)
            {
                var node = new DebugActionNode(bloodline.defName);
                for (int i = 0; i < bloodline.purities.Count; i++)
                {
                    int index = i;
                    node.AddChild(new DebugActionNode(bloodline.purities[i].purity.defName, DebugActionType.ToolMapForPawns)
                    {
                        pawnAction = p =>
                        {
                            if (BloodlineUtility.SetBloodline(p, bloodline, index)) DebugActionsUtility.DustPuffFrom(p);
                        }
                    });
                }
                nodes.Add(node);
            }
            return nodes;
        }

        [DebugAction("Convergence", "Convergence: Spawn bloodline pawn", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static List<DebugActionNode> SpawnBloodlinePawn()
        {
            var nodes = new List<DebugActionNode>();
            foreach (var bloodline in DefDatabase<BloodlineDef>.AllDefsListForReading)
            {
                var node = new DebugActionNode(bloodline.defName);
                for (int i = 0; i < bloodline.purities.Count; i++)
                {
                    int index = i;
                    node.AddChild(new DebugActionNode(bloodline.purities[i].purity.defName, DebugActionType.ToolMap)
                    {
                        action = () => SpawnWithBloodline(bloodline, index)
                    });
                }
                nodes.Add(node);
            }
            return nodes;
        }

        /// <summary>Colono del jugador con el linaje y la pureza dados (xenotipo del linaje si la pureza lo tiene).</summary>
        private static void SpawnWithBloodline(BloodlineDef bloodline, int index)
        {
            var map = Find.CurrentMap;
            var cell = UI.MouseCell();
            if (!cell.InBounds(map)) return;
            var xenotype = ModsConfig.BiotechActive ? bloodline.purities[index].xenotype ?? XenotypeDefOf.Baseliner : null;
            var request = new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayer, forceGenerateNewPawn: true,
                developmentalStages: DevelopmentalStage.Adult, forcedXenotype: xenotype);
            var pawn = PawnGenerator.GeneratePawn(request);
            BloodlineUtility.SetBloodline(pawn, bloodline, index);
            GenSpawn.Spawn(pawn, cell, map);
            DebugActionsUtility.DustPuffFrom(pawn);
        }

        [DebugAction("Convergence", "Convergence: Purify bloodline", actionType = DebugActionType.ToolMapForPawns,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void PurifyBloodline(Pawn p)
        {
            if (BloodlineUtility.TryPurify(p)) DebugActionsUtility.DustPuffFrom(p);
            else Messages.Message("No bloodline, or already at the highest purity.", MessageTypeDefOf.RejectInput, false);
        }

        [DebugAction("Convergence", "Convergence: Bloodline self-test", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void BloodlineSelfTestAction()
        {
            BloodlineSelfTest.Run();
            Messages.Message("Logged to the debug log.", MessageTypeDefOf.NeutralEvent, false);
        }

        [DebugAction("Convergence", "Convergence: Learn manual", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static List<DebugActionNode> LearnManual()
        {
            var nodes = new List<DebugActionNode>();
            foreach (var manual in DefDatabase<TechniqueManualDef>.AllDefsListForReading)
            {
                nodes.Add(new DebugActionNode(manual.defName, DebugActionType.ToolMapForPawns)
                {
                    pawnAction = p =>
                    {
                        var comp = CompCultivator.For(p);
                        if (comp == null) return;
                        var report = manual.CanLearn(p);
                        if (!report.Accepted)
                        {
                            // Debug: avisa pero lo enseña igual si sigue el camino (p. ej. sin el elemento).
                            Messages.Message(report.Reason, MessageTypeDefOf.RejectInput, false);
                            if (!comp.CanFollow(manual.Path) || comp.Techniques.KnowsManual(manual)) return;
                        }
                        comp.Techniques.InitializePoints(comp);
                        comp.Techniques.LearnManual(manual);
                        DebugActionsUtility.DustPuffFrom(p);
                    }
                });
            }
            return nodes;
        }

        [DebugAction("Convergence", "Convergence: Unlock all techniques", actionType = DebugActionType.ToolMapForPawns,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void UnlockAllTechniques(Pawn p)
        {
            var comp = CompCultivator.For(p);
            if (comp == null || !comp.IsQiCultivator) return;
            comp.Techniques.InitializePoints(comp);
            // Todos los manuales y todas sus técnicas, sin requisitos ni puntos.
            foreach (var manual in DefDatabase<TechniqueManualDef>.AllDefsListForReading)
            {
                comp.Techniques.LearnManual(manual, sendMessage: false);
                foreach (var t in manual.Techniques) comp.Techniques.Unlock(t, free: true);
            }
            DebugActionsUtility.DustPuffFrom(p);
        }

        [DebugAction("Convergence", "Convergence: Add mastery", actionType = DebugActionType.ToolMapForPawns,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void AddMastery(Pawn p)
        {
            var comp = CompCultivator.For(p);
            if (comp == null) return;
            // Un nivel de maestría en todas las técnicas aprendidas.
            foreach (var t in comp.Techniques.Unlocked) comp.Techniques.AddMasteryLevel(t);
            DebugActionsUtility.DustPuffFrom(p);
        }

        [DebugAction("Convergence", "Convergence: Add technique point", actionType = DebugActionType.ToolMapForPawns,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void AddTechniquePoint(Pawn p)
        {
            var comp = CompCultivator.For(p);
            if (comp == null) return;
            comp.Techniques.AddPoints(1);
            DebugActionsUtility.DustPuffFrom(p);
        }

        [DebugAction("Convergence", "Convergence: Spawn jade slip", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static List<DebugActionNode> SpawnJadeSlip()
        {
            var nodes = new List<DebugActionNode>();
            foreach (var manual in DefDatabase<TechniqueManualDef>.AllDefsListForReading)
            {
                nodes.Add(new DebugActionNode(manual.defName, DebugActionType.ToolMap)
                {
                    action = () =>
                    {
                        var cell = UI.MouseCell();
                        var map = Find.CurrentMap;
                        if (!cell.InBounds(map)) return;
                        var thing = ThingMaker.MakeThing(CVG_DefOf.CVG_JadeSlip);
                        thing.TryGetComp<CompUseEffect_LearnTechniqueManual>()?.SetManual(manual);
                        GenPlace.TryPlaceThing(thing, cell, map, ThingPlaceMode.Near);
                    }
                });
            }
            return nodes;
        }

        [DebugAction("Convergence", "Convergence: Spawn mentor", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static List<DebugActionNode> SpawnMentor()
        {
            var nodes = new List<DebugActionNode>();
            foreach (var mentor in DefDatabase<MentorDef>.AllDefsListForReading)
            {
                var node = new DebugActionNode(mentor.defName);
                node.AddChild(new DebugActionNode("walker", DebugActionType.Action, () => SpawnMentor(mentor, false)));
                node.AddChild(new DebugActionNode("wounded", DebugActionType.Action, () => SpawnMentor(mentor, true)));
                nodes.Add(node);
            }
            return nodes;
        }

        private static void SpawnMentor(MentorDef def, bool wounded)
        {
            var state = GameComponent_Mentor.Instance;
            if (state == null) return;
            if (state.Mentor != null)
            {
                Messages.Message("A mentor already came in this game.", MessageTypeDefOf.RejectInput, false);
                return;
            }
            state.Arrive(Find.CurrentMap, def, wounded);
        }

        [DebugAction("Convergence", "Convergence: Mentor complete lesson", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void MentorCompleteLesson()
        {
            var state = GameComponent_Mentor.Instance;
            if (state != null && state.Present) state.CompleteLesson();
        }

        [DebugAction("Convergence", "Convergence: Mentor leave now", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void MentorLeaveNow()
        {
            var state = GameComponent_Mentor.Instance;
            if (state != null && state.Present) state.End(died: state.Mentor.Dead);
        }

        [DebugAction("Convergence", "Convergence: Log cultivation stats", actionType = DebugActionType.ToolMapForPawns,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void LogCultivationStats(Pawn p)
        {
            var comp = CompCultivator.For(p);
            if (comp == null) return;
            var sb = new System.Text.StringBuilder("[Convergence] " + p.LabelShort + ": HealthScale=" + p.HealthScale.ToString("0.###")
                + " (cultivation x" + comp.HealthScaleFactor.ToString("0.###") + ")");
            foreach (var t in comp.ActiveTrackers)
                sb.Append("\n  " + t.Path.defName + ": " + t.Realm?.defName + " stage " + t.Stage + ", foundation " + t.FoundationQuality.ToString("0.##")
                    + ", " + t.Resource.Label + " " + t.Resource.Current.ToString("F0") + "/" + t.Resource.Max.ToString("F0"));
            foreach (var stat in new[] { StatDefOf.MoveSpeed, StatDefOf.MeleeDamageFactor, StatDefOf.IncomingDamageFactor, StatDefOf.ArmorRating_Sharp,
                         StatDefOf.InjuryHealingFactor, StatDefOf.PainShockThreshold, CVG_DefOf.CVG_QiMax, CVG_DefOf.CVG_QiRegen })
                sb.Append("\n  " + stat.defName + " = " + p.GetStatValue(stat).ToString("0.###"));
            Log.Message(sb.ToString());
            Messages.Message("Logged to the debug log.", MessageTypeDefOf.NeutralEvent, false);
        }
    }
}
