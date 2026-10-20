using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Convergence.Stories
{
    /// <summary>Un evento de seguimiento programado por un escenario (clave del evento + rango de días).</summary>
    public class StoryFollowUp
    {
        public string key;
        public IntRange days = new IntRange(5, 10);
    }

    /// <summary>Un enemigo notable muerto por el jugador: la facción volverá a por venganza.</summary>
    public class Grudge : IExposable
    {
        public string factionDef;
        public string victimName;
        public int tick;

        public void ExposeData()
        {
            Scribe_Values.Look(ref factionDef, "factionDef");
            Scribe_Values.Look(ref victimName, "victimName");
            Scribe_Values.Look(ref tick, "tick");
        }
    }

    public class ScheduledStory : IExposable
    {
        public string key;
        public int tick;

        public void ExposeData()
        {
            Scribe_Values.Look(ref key, "key");
            Scribe_Values.Look(ref tick, "tick");
        }
    }

    /// <summary>
    /// Estado de las historias: el "origen" del escenario (para que sus eventos de seguimiento lo llamen después),
    /// los eventos programados y las venganzas pendientes.
    /// </summary>
    public class GameComponent_Story : GameComponent
    {
        public string origin;
        public int originTick = -1;
        private List<ScheduledStory> scheduled = new List<ScheduledStory>();
        private List<Grudge> grudges = new List<Grudge>();
        private int nextGrudgeCheck;
        private List<string> flags = new List<string>();
        private List<string> firedKeys = new List<string>();
        private List<int> firedTicks = new List<int>();
        private List<StoryTask> tasks = new List<StoryTask>();
        private int nextEventTick = -1;
        private HashSet<string> flagSet;
        private Dictionary<string, int> lastFired;

        public GameComponent_Story(Game game) { }

        public static GameComponent_Story Instance => Current.Game?.GetComponent<GameComponent_Story>();
        public List<ScheduledStory> Scheduled => scheduled;
        public List<Grudge> Grudges => grudges;
        public List<StoryTask> Tasks => tasks;
        public HashSet<string> Flags => flagSet ??= new HashSet<string>(flags);
        public Dictionary<string, int> LastFired => lastFired ??= firedKeys.Zip(firedTicks, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);

        public void Schedule(string key, int delayTicks)
        {
            scheduled.Add(new ScheduledStory { key = key, tick = Find.TickManager.TicksGame + delayTicks });
        }

        public override void GameComponentTick()
        {
            int now = Find.TickManager.TicksGame;
            if (now % 250 != 0) return;
            for (int i = scheduled.Count - 1; i >= 0; i--)
            {
                var s = scheduled[i];
                if (now < s.tick) continue;
                if (Find.AnyPlayerHomeMap == null) continue; // espera a que haya un mapa
                scheduled.RemoveAt(i);
                try { StoryEvents.Run(s.key); }
                catch (Exception e) { Log.Error("[Convergence] Story event " + s.key + " failed: " + e); }
            }
            UpdateTasks(now);
            var conv = GameComponent_Convergence.Instance;
            if (conv != null)
            {
                if (nextEventTick < 0) nextEventTick = now + (int)((conv.Occurred ? Rand.Range(1.5f, 4f) : Rand.Range(8f, 14f)) * GenDate.TicksPerDay);
                var map = Find.AnyPlayerHomeMap;
                if (now >= nextEventTick && map != null)
                {
                    float freq = StoryEngine.FrequencyFactor();
                    nextEventTick = now + (int)(Rand.Range(conv.Occurred ? 2.5f : 6f, conv.Occurred ? 6f : 12f) * GenDate.TicksPerDay / Mathf.Max(0.05f, freq));
                    try { if (freq > 0f) StoryEngine.PickAndFire(map); }
                    catch (Exception e) { Log.Error("[Convergence] Story scheduler failed: " + e); }
                }
            }
            if (now >= nextGrudgeCheck)
            {
                nextGrudgeCheck = now + GenDate.TicksPerHour * 6;
                CheckGrudges(now);
            }
        }

        private void UpdateTasks(int now)
        {
            for (int i = tasks.Count - 1; i >= 0; i--)
            {
                var t = tasks[i];
                if (now < t.expireTick) continue;
                tasks.RemoveAt(i);
                if (!t.fail.NullOrEmpty()) Schedule("ev:" + t.fail, 60);
            }
        }

        /// <summary>Una bestia de una misión de caza murió.</summary>
        public void Notify_PawnDied(int id)
        {
            for (int i = tasks.Count - 1; i >= 0; i--)
            {
                var t = tasks[i];
                if (!t.pawnIds.Contains(id) || t.deadIds.Contains(id)) continue;
                t.deadIds.Add(id);
                if (t.deadIds.Count >= t.pawnIds.Count)
                {
                    tasks.RemoveAt(i);
                    if (!t.success.NullOrEmpty()) Schedule("ev:" + t.success, 60);
                }
            }
        }

        private void CheckGrudges(int now)
        {
            for (int i = grudges.Count - 1; i >= 0; i--)
            {
                var g = grudges[i];
                if (now - g.tick < 3 * GenDate.TicksPerDay || Find.AnyPlayerHomeMap == null) continue;
                if (!Rand.Chance(0.25f)) continue; // llegan entre 3 y ~8 días después
                grudges.RemoveAt(i);
                StoryEvents.ClanVengeance(g);
                break;
            }
        }

        public void AddGrudge(Pawn victim)
        {
            if (victim?.Faction == null) return;
            if (grudges.Count(g => g.factionDef == victim.Faction.def.defName) >= 2) return;
            grudges.Add(new Grudge { factionDef = victim.Faction.def.defName, victimName = victim.LabelShortCap, tick = Find.TickManager.TicksGame });
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref origin, "origin");
            Scribe_Values.Look(ref originTick, "originTick", -1);
            Scribe_Collections.Look(ref scheduled, "scheduled", LookMode.Deep);
            Scribe_Collections.Look(ref grudges, "grudges", LookMode.Deep);
            Scribe_Values.Look(ref nextGrudgeCheck, "nextGrudgeCheck");
            Scribe_Values.Look(ref nextEventTick, "nextEventTick", -1);
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                flags = Flags.ToList();
                firedKeys = LastFired.Keys.ToList();
                firedTicks = LastFired.Values.ToList();
            }
            Scribe_Collections.Look(ref flags, "flags", LookMode.Value);
            Scribe_Collections.Look(ref firedKeys, "firedKeys", LookMode.Value);
            Scribe_Collections.Look(ref firedTicks, "firedTicks", LookMode.Value);
            Scribe_Collections.Look(ref tasks, "tasks", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                scheduled ??= new List<ScheduledStory>();
                grudges ??= new List<Grudge>();
                flags ??= new List<string>();
                firedKeys ??= new List<string>();
                firedTicks ??= new List<int>();
                tasks ??= new List<StoryTask>();
                flagSet = new HashSet<string>(flags);
                lastFired = firedKeys.Zip(firedTicks, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);
            }
        }
    }

    // ------------------------------------------------------------------ partes de escenario

    /// <summary>Guarda el origen del escenario y programa sus eventos de seguimiento.</summary>
    public class ScenPart_StoryOrigin : ScenPart
    {
        public string origin;
        public List<StoryFollowUp> followUps = new List<StoryFollowUp>();

        public override void PostGameStart()
        {
            var story = GameComponent_Story.Instance;
            if (story == null) return;
            story.origin = origin;
            story.originTick = Find.TickManager.TicksGame;
            foreach (var f in followUps)
                story.Schedule(f.key, f.days.RandomInRange * GenDate.TicksPerDay);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref origin, "origin");
            Scribe_Collections.Look(ref followUps, "followUps", LookMode.Deep);
        }
    }

    /// <summary>La Convergencia ya ocurrió cuando empieza la partida (la Calamidad pasó antes).</summary>
    public class ScenPart_ConvergenceOccurred : ScenPart
    {
        public override string Summary(Scenario scen) => "CVG_ScenPart_ConvergenceOccurred".Translate();

        public override void PostGameStart()
        {
            var gc = GameComponent_Convergence.Instance;
            if (gc != null && !gc.Occurred) gc.MarkOccurred();
            // Lo mismo que hace el incidente con el mapa: vetas de piedra espiritual, bestias, vetas de qi y el Maestro.
            ConvergenceUtility.WakeMap(Find.AnyPlayerHomeMap);
        }
    }

    /// <summary>Los colonos iniciales ya cultivan: raíz, reino, etapa y técnicas.</summary>
    public class ScenPart_StartingCultivators : ScenPart
    {
        public CultivationRealmDef realm;
        public IntRange stage = new IntRange(1, 1);
        public int count = 99;
        public bool techniques = true;

        /// <summary>En la descripción del escenario, como las partes vanilla ("Empezás con…").</summary>
        public override string Summary(Scenario scen)
        {
            string realmLabel = realm?.LabelCap ?? (string)"CVG_ScenPart_FirstRealm".Translate();
            string stages = stage.min == stage.max ? stage.min.ToString() : stage.min + "-" + stage.max;
            string who = count >= 99 ? "CVG_ScenPart_AllColonists".Translate() : "CVG_ScenPart_SomeColonists".Translate(count);
            string text = "CVG_ScenPart_StartingCultivators".Translate(who, realmLabel, stages);
            if (techniques) text += " " + "CVG_ScenPart_WithTechniques".Translate();
            return text;
        }

        public override void PostGameStart()
        {
            if (!GameComponent_Convergence.Active) return;
            var pawns = Find.GameInitData?.startingAndOptionalPawns?.Take(count).ToList() ?? new List<Pawn>();
            foreach (var p in pawns.Where(x => x != null && x.RaceProps.Humanlike))
            {
                var comp = CompCultivator.For(p);
                if (comp == null) continue;
                comp.Sync();
                if (!comp.HasRoot)
                {
                    CultivationGenerator.GiveRoot(p, CultivationGenerator.RandomGrade(), CultivationGenerator.RandomElements());
                    comp.Sync();
                }
                if (!comp.IsQiCultivator) continue;
                var tracker = comp.Realm;
                if (!tracker.EnsureStarted()) continue;
                var target = realm ?? tracker.Realm;
                if (target.order > tracker.Realm.order) tracker.AdvanceTo(target, 1f);
                tracker.SetStage(stage.RandomInRange);
                comp.InvalidateHealthScale();
                if (techniques)
                {
                    TechniqueUtility.GiveNpcTechniques(p);
                    if (!comp.Techniques.AnyManual)
                    {
                        // El jugador no tiene aún la investigación de manuales: el colono ya los traía de su secta.
                        var manual = DefDatabase<TechniqueManualDef>.AllDefsListForReading
                            .Where(m => m.npcCommonality > 0f && (m.element == null || TechniqueUtility.HasElement(comp, m.element)) && m.Path == CVG_DefOf.CVG_Path_Qi
                                        && (m.minRealm == null || TechniqueUtility.RealmOrder(comp, m.Path) >= m.minRealm.order))
                            .RandomElementWithFallback();
                        if (manual != null) comp.Techniques.LearnManual(manual, sendMessage: false);
                    }
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref realm, "realm");
            Scribe_Values.Look(ref stage, "stage");
            Scribe_Values.Look(ref count, "count", 99);
            Scribe_Values.Look(ref techniques, "techniques", true);
        }
    }

    /// <summary>Los colonos iniciales nacen con un linaje de sangre (si el mod de razas lo trae).</summary>
    public class ScenPart_StartingBloodline : ScenPart
    {
        public BloodlineDef bloodline;
        public int purityIndex = 1;
        public int count = 1;

        public override string Summary(Scenario scen) =>
            bloodline == null ? null : (string)"CVG_ScenPart_StartingBloodline".Translate(count, bloodline.LabelCap);

        public override void PostGameStart()
        {
            if (bloodline == null) return;
            var pawns = Find.GameInitData?.startingAndOptionalPawns?.Take(count).ToList() ?? new List<Pawn>();
            foreach (var p in pawns.Where(x => x != null && x.RaceProps.Humanlike))
                BloodlineUtility.SetBloodline(p, bloodline, purityIndex);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref bloodline, "bloodline");
            Scribe_Values.Look(ref purityIndex, "purityIndex", 1);
            Scribe_Values.Look(ref count, "count", 1);
        }
    }

    // ------------------------------------------------------------------ ingerible que cura el dantian

    public class IngestionOutcomeDoer_RemoveHediffs : IngestionOutcomeDoer
    {
        public List<HediffDef> hediffs;

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
        {
            if (hediffs == null) return;
            foreach (var def in hediffs)
            {
                var h = pawn.health.hediffSet.GetFirstHediffOfDef(def);
                if (h != null)
                {
                    pawn.health.RemoveHediff(h);
                    Messages.Message("CVG_Story_Dantian_Mended".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.PositiveEvent);
                }
            }
        }
    }

    // ------------------------------------------------------------------ utilidades

    public static class StoryUtility
    {
        public static Map HomeMap => Find.AnyPlayerHomeMap;

        public static Faction FactionByDef(string defName) =>
            Find.FactionManager.AllFactionsListForReading.FirstOrDefault(f => f.def.defName == defName);

        public static List<Faction> Sects() =>
            Find.FactionManager.AllFactionsListForReading.Where(f => f.def.GetModExtension<DefModExtension_ConvergenceFaction>() != null).ToList();

        public static Faction AnyHostileHumanlike() =>
            Find.FactionManager.AllFactionsListForReading.Where(f => !f.IsPlayer && !f.Hidden && f.def.humanlikeFaction && f.HostileTo(Faction.OfPlayer)).RandomElementWithFallback();

        /// <summary>Lanza un asalto de <paramref name="faction"/> (la vuelve hostil, recuperable, si no lo era). Devuelve false si no se pudo.</summary>
        public static bool Raid(Faction faction, float pointsFactor, string letterKey, params NamedArgument[] args)
        {
            var map = HomeMap;
            if (map == null) return false;
            faction ??= AnyHostileHumanlike();
            if (faction == null) return false;
            // Justo por debajo del umbral de hostilidad: la secta ataca, pero el jugador puede reconciliarse con regalos después.
            if (!faction.HostileTo(Faction.OfPlayer))
                faction.TryAffectGoodwillWith(Faction.OfPlayer, -(faction.PlayerGoodwill + 80), canSendMessage: false, canSendHostilityLetter: false);
            var parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, map);
            parms.faction = faction;
            parms.forced = true;
            parms.points = Mathf.Max(80f, parms.points * pointsFactor);
            if (letterKey != null)
            {
                parms.customLetterLabel = ("CVG_Story_" + letterKey + "_Label").Translate();
                parms.customLetterText = ("CVG_Story_" + letterKey + "_Text").Translate(args.Concat(new[] { faction.Named("FACTION") }).ToArray());
            }
            return IncidentDefOf.RaidEnemy.Worker.TryExecute(parms);
        }

        public static Pawn MakeRefugee(Map map, Faction forFaction)
        {
            var kind = DefDatabase<PawnKindDef>.GetNamedSilentFail("CVG_RogueCultivators_Wanderer") ?? PawnKindDefOf.Villager;
            var p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, forFaction, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: true));
            if (RCellFinder.TryFindRandomPawnEntryCell(out var cell, map, CellFinder.EdgeRoadChance_Friendly))
                GenSpawn.Spawn(p, cell, map);
            else
                GenSpawn.Spawn(p, CellFinder.RandomClosewalkCellNear(map.Center, map, 8), map);
            return p;
        }

        /// <summary>Quita objetos de los almacenes de la colonia (si no hay suficientes, los que haya).</summary>
        public static void TakeFromColony(Map map, ThingDef def, int count)
        {
            foreach (var t in map.listerThings.ThingsOfDef(def).ToList())
            {
                if (count <= 0) break;
                int n = Mathf.Min(count, t.stackCount);
                t.SplitOff(n).Destroy();
                count -= n;
            }
        }

        public static void SendLetter(string key, LetterDef def, LookTargets targets, params NamedArgument[] args)
        {
            Find.LetterStack.ReceiveLetter(("CVG_Story_" + key + "_Label").Translate(args), ("CVG_Story_" + key + "_Text").Translate(args), def, targets);
        }
    }

    // ------------------------------------------------------------------ venganzas: ¿quién murió?

    [HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
    public static class Patch_Pawn_Kill_Grudge
    {
        public static void Prefix(Pawn __instance, DamageInfo? dinfo)
        {
            try
            {
                if (__instance.Dead) return;
                GameComponent_Story.Instance?.Notify_PawnDied(__instance.thingIDNumber);
                if (!GameComponent_Convergence.Active) return;
                var instigator = dinfo?.Instigator as Pawn;
                if (instigator?.Faction != Faction.OfPlayer) return;
                var f = __instance.Faction;
                if (f == null || f.IsPlayer || f.def.GetModExtension<DefModExtension_ConvergenceFaction>() == null) return;
                var comp = CompCultivator.For(__instance);
                if (comp == null || comp.RealmLevel < 2) return; // solo cultivadores de verdad
                GameComponent_Story.Instance?.AddGrudge(__instance);
            }
            catch (Exception e) { Log.Warning("[Convergence] Grudge tracking failed: " + e.Message); }
        }
    }

    [StaticConstructorOnStartup]
    public static class StoriesStartup
    {
        static StoriesStartup()
        {
            new Harmony("prospero1088.convergence.stories").PatchAll();
            StoryEngine.FrequencyFactor = () => CultivationDifficulty.Current?.eventFrequency ?? 1f;
            Log.Message("[Convergence] Stories loaded.");
        }
    }
}
