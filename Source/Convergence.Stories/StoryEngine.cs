using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Convergence.Stories
{
    // ====================================================================== definiciones

    /// <summary>
    /// Un evento de historia definido por XML: texto, condiciones, opciones del jugador y resultados (<see cref="StoryOutcome"/>).
    /// El planificador de <see cref="GameComponent_Story"/> los lanza de uno en uno con una frecuencia moderada, solo tras la Convergencia
    /// (salvo <see cref="pre"/>). Los pasos de una cadena tienen <see cref="chainOnly"/> y los activa otro evento.
    /// Textos (Keyed): <c>CVG_Ev_&lt;defName&gt;_Label/_Text</c>, <c>_Opt_&lt;opción&gt;</c> y <c>_Res_&lt;opción&gt;</c>.
    /// </summary>
    public class StoryEventDef : Def
    {
        /// <summary>encounter, dilemma, disaster, opportunity, visit, mission, chain, pre (solo documentación y estadísticas).</summary>
        public string category = "encounter";
        public float weight = 1f;
        public int minDays = 3;
        public float minRefireDays = 40f;
        public bool pre;
        public bool chainOnly;
        public bool isThreat;
        public bool needsCultivator;
        public int minColonists = 1;
        public List<string> requiresFlags;
        public List<string> forbidsFlags;
        /// <summary>Facción principal del texto ({FACTION_name}); ver <see cref="StoryContext.Resolve"/>.</summary>
        public string factionToken;
        /// <summary>Segunda facción del texto ({OTHER_name}); p. ej. la otra secta de una guerra.</summary>
        public string factionToken2;
        /// <summary>Si es true, el evento solo ocurre cuando <see cref="factionToken"/> existe.</summary>
        public bool requiresFaction;
        public string tone = "neutral";
        public int timeoutDays = 2;
        public List<StoryOption> options;
        public List<StoryOutcome> outcomes;

        public LetterDef LetterDef => tone switch
        {
            "positive" => LetterDefOf.PositiveEvent,
            "negative" => LetterDefOf.NegativeEvent,
            "threat" => LetterDefOf.ThreatSmall,
            _ => LetterDefOf.NeutralEvent
        };
    }

    public class StoryOption
    {
        public string key;
        public List<StoryOutcome> outcomes;
        /// <summary>Coste en objetos del almacén de la colonia (opcional).</summary>
        public ThingDef costThing;
        public IntRange costCount = new IntRange(1, 1);
        public string requiresFlag;
    }

    public class StoryTask : IExposable
    {
        public List<int> pawnIds = new List<int>();
        public List<int> deadIds = new List<int>();
        public string success;
        public string fail;
        public int expireTick;

        public void ExposeData()
        {
            Scribe_Collections.Look(ref pawnIds, "pawnIds", LookMode.Value);
            Scribe_Collections.Look(ref deadIds, "deadIds", LookMode.Value);
            Scribe_Values.Look(ref success, "success");
            Scribe_Values.Look(ref fail, "fail");
            Scribe_Values.Look(ref expireTick, "expireTick");
            pawnIds ??= new List<int>();
            deadIds ??= new List<int>();
        }
    }

    // ====================================================================== contexto

    public class StoryContext
    {
        public Map map;
        public StoryEventDef def;
        public readonly Dictionary<string, Faction> factions = new Dictionary<string, Faction>();
        public LookTargets look = LookTargets.Invalid;

        public StoryContext(Map map, StoryEventDef def) { this.map = map; this.def = def; }

        public Faction Primary => def?.factionToken == null ? null : Resolve(def.factionToken);

        /// <summary>
        /// Tokens: bloodmoon / sword / temperings / cauldron / thread / rogue (azurepeak y lotus: alias viejos); sect:any, sect:friendly, sect:hostile, sect:rival
        /// (otra secta distinta de la amistosa); hostile (cualquier humanoide hostil), neutral (no hostil), empire.
        /// </summary>
        public Faction Resolve(string token)
        {
            if (token.NullOrEmpty()) return null;
            if (factions.TryGetValue(token, out var f)) return f;
            f = ResolveNew(token);
            if (f != null) factions[token] = f;
            return f;
        }

        private Faction ResolveNew(string token)
        {
            switch (token)
            {
                case "bloodmoon": return StoryUtility.FactionByDef("CVG_Faction_BloodMoon");
                // Convergence: Factions. "azurepeak" y "lotus" quedan como alias de partidas viejas: el Pico Azur pasó a la Espada Sin
                // Forma y el Loto Silencioso al Caldero Celestial.
                case "sword":
                case "azurepeak": return StoryUtility.FactionByDef("CVG_Faction_FormlessSword");
                case "temperings": return StoryUtility.FactionByDef("CVG_Faction_NineTemperings");
                case "thread": return StoryUtility.FactionByDef("CVG_Faction_InvisibleThread");
                case "lotus":
                case "cauldron": return StoryUtility.FactionByDef("CVG_Faction_HeavenlyCauldron");
                case "rogue": return StoryUtility.FactionByDef("CVG_Faction_RogueCultivators");
                case "sect:any": return StoryUtility.Sects().RandomElementWithFallback();
                case "sect:friendly": return StoryUtility.Sects().Where(s => !s.HostileTo(Faction.OfPlayer) && s.def.defName != "CVG_Faction_BloodMoon").RandomElementWithFallback();
                case "sect:hostile": return StoryUtility.Sects().Where(s => s.HostileTo(Faction.OfPlayer)).RandomElementWithFallback();
                case "sect:rival":
                    var friendly = Resolve("sect:friendly");
                    return StoryUtility.Sects().Where(s => s != friendly && s.def.defName != "CVG_Faction_BloodMoon").RandomElementWithFallback();
                case "hostile": return StoryUtility.AnyHostileHumanlike();
                case "neutral":
                    return Find.FactionManager.AllFactionsListForReading.Where(x => !x.IsPlayer && !x.Hidden && x.def.humanlikeFaction && !x.HostileTo(Faction.OfPlayer)).RandomElementWithFallback();
                case "empire": return Find.FactionManager.OfEmpire;
            }
            return StoryUtility.FactionByDef(token);
        }

        public string Serialize()
        {
            string s = string.Join("|", factions.Select(kv => kv.Key + "=" + kv.Value.def.defName + ":" + kv.Value.loadID));
            var p = RandomColonist;
            return p == null ? s : s + "|@pawn=" + p.ThingID;
        }

        public static StoryContext Deserialize(Map map, StoryEventDef def, string payload)
        {
            var ctx = new StoryContext(map, def);
            if (payload.NullOrEmpty()) return ctx;
            foreach (var part in payload.Split('|'))
            {
                if (part.StartsWith("@pawn="))
                {
                    string thingId = part.Substring(6);
                    ctx.protagonist = map.mapPawns.FreeColonistsSpawned.FirstOrDefault(x => x.ThingID == thingId);
                    continue;
                }
                var kv = part.Split('=');
                if (kv.Length != 2) continue;
                var idPart = kv[1].Split(':');
                if (idPart.Length != 2 || !int.TryParse(idPart[1], out int id)) continue;
                var f = Find.FactionManager.AllFactionsListForReading.FirstOrDefault(x => x.loadID == id);
                if (f != null) ctx.factions[kv[0]] = f;
            }
            return ctx;
        }

        public List<Pawn> Colonists => map.mapPawns.FreeColonistsSpawned;
        private Pawn protagonist;

        /// <summary>El colono que vive el evento: el mismo en el texto, en la decisión y en sus resultados (antes cada
        /// resultado elegía otro al azar y la carta nombraba a uno mientras la herida le tocaba a otro).</summary>
        public Pawn RandomColonist => protagonist != null && !protagonist.Dead && protagonist.Spawned ? protagonist : protagonist = Colonists.RandomElementWithFallback();
        public Pawn RandomCultivator => Colonists.Where(p => CompCultivator.For(p)?.IsCultivator ?? false).RandomElementWithFallback();

        public NamedArgument[] Args()
        {
            var list = new List<NamedArgument>();
            var f = Primary;
            if (f != null) list.Add(f.Named("FACTION"));
            var other = def?.factionToken2 == null ? null : Resolve(def.factionToken2);
            if (other != null) list.Add(other.Named("OTHER"));
            var p = RandomColonist;
            if (p != null) list.Add(p.Named("PAWN"));
            return list.ToArray();
        }
    }

    // ====================================================================== resultados

    public abstract class StoryOutcome
    {
        public float chance = 1f;
        /// <summary>Clave Keyed del mensaje que se muestra si el resultado ocurre (opcional).</summary>
        public string message;
        public string messageType = "neutral";

        public void TryApply(StoryContext ctx)
        {
            if (!Rand.Chance(chance)) return;
            var targets = Apply(ctx);
            if (!message.NullOrEmpty() && message.TryTranslate(out var text))
            {
                var type = messageType == "positive" ? MessageTypeDefOf.PositiveEvent : messageType == "negative" ? MessageTypeDefOf.NegativeEvent : MessageTypeDefOf.NeutralEvent;
                Messages.Message(string.Format(text, ctx.Primary?.Name ?? ""), targets ?? LookTargets.Invalid, type);
            }
        }

        protected abstract LookTargets Apply(StoryContext ctx);
    }

    public class SO_Raid : StoryOutcome
    {
        public string faction = "hostile";
        public float points = 0.8f;
        protected override LookTargets Apply(StoryContext ctx)
        {
            StoryUtility.Raid(ctx.Resolve(faction), points, null);
            return null;
        }
    }

    public class SO_Goodwill : StoryOutcome
    {
        public string faction;
        public int amount;
        protected override LookTargets Apply(StoryContext ctx)
        {
            ctx.Resolve(faction)?.TryAffectGoodwillWith(Faction.OfPlayer, amount, canSendMessage: true, canSendHostilityLetter: amount < 0);
            return null;
        }
    }

    public class SO_Items : StoryOutcome
    {
        public ThingDef thing;
        public ThingDef stuff;
        public IntRange count = new IntRange(1, 1);
        protected override LookTargets Apply(StoryContext ctx)
        {
            if (thing == null) return null; // objeto de un módulo que no está cargado
            var t = ThingMaker.MakeThing(thing, thing.MadeFromStuff ? (stuff ?? GenStuff.DefaultStuffFor(thing)) : null);
            t.stackCount = Mathf.Max(1, count.RandomInRange);
            DropPodUtility.DropThingsNear(DropCellFinder.TradeDropSpot(ctx.map), ctx.map, new List<Thing> { t });
            return t;
        }
    }

    public class SO_TakeItems : StoryOutcome
    {
        public ThingDef thing;
        public IntRange count = new IntRange(1, 1);
        protected override LookTargets Apply(StoryContext ctx)
        {
            if (thing != null) StoryUtility.TakeFromColony(ctx.map, thing, count.RandomInRange);
            return null;
        }
    }

    /// <summary>Queda como relato (arte, esculturas): el protagonista del evento vivió esto.</summary>
    public class SO_Tale : StoryOutcome
    {
        public TaleDef tale;
        protected override LookTargets Apply(StoryContext ctx)
        {
            var p = ctx.RandomColonist;
            if (tale != null && p != null) TaleRecorder.RecordTale(tale, p);
            return null;
        }
    }

    /// <summary>Favor de secta (Royalty): la contribución que suben los rangos de la secta del protagonista.</summary>
    public class SO_Favor : StoryOutcome
    {
        public string faction = "sect:friendly";
        public int amount = 2;
        protected override LookTargets Apply(StoryContext ctx)
        {
            var f = ctx.Resolve(faction);
            var p = ctx.RandomColonist;
            if (f == null || p?.royalty == null || !ModsConfig.RoyaltyActive || f.def.RoyalTitlesAwardableInSeniorityOrderForReading.NullOrEmpty()) return null;
            p.royalty.GainFavor(f, amount);
            return p;
        }
    }

    /// <summary>Gente que se une a la colonia.</summary>
    public class SO_Join : StoryOutcome
    {
        public PawnKindDef kind;
        public IntRange count = new IntRange(1, 1);
        protected override LookTargets Apply(StoryContext ctx)
        {
            var list = new List<Pawn>();
            int n = count.RandomInRange;
            for (int i = 0; i < n; i++)
            {
                // Con la facción del jugador desde el principio (como el vanilla "se une un viajero"): ideología y nombre correctos.
                var p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind ?? PawnKindDefOf.Villager, Faction.OfPlayer, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: true));
                if (RCellFinder.TryFindRandomPawnEntryCell(out var cell, ctx.map, CellFinder.EdgeRoadChance_Friendly)) GenSpawn.Spawn(p, cell, ctx.map);
                else GenSpawn.Spawn(p, CellFinder.RandomClosewalkCellNear(ctx.map.Center, ctx.map, 8), ctx.map);
                p.SetFaction(Faction.OfPlayer);
                list.Add(p);
            }
            return list.Count > 0 ? new LookTargets(list) : null;
        }
    }

    /// <summary>Un grupo de visitantes de la facción.</summary>
    public class SO_Visit : StoryOutcome
    {
        public string faction = "sect:friendly";
        protected override LookTargets Apply(StoryContext ctx)
        {
            var f = ctx.Resolve(faction);
            if (f == null) return null;
            if (f.HostileTo(Faction.OfPlayer)) { StoryUtility.Raid(f, 0.6f, null); return null; }
            Send(f, ctx.map, 1);
            return null;
        }

        /// <summary>Manda el grupo, o lo aplaza un día si ya hay huéspedes en el mapa (como mucho <see cref="StoryUtility.VisitMaxTries"/> veces).</summary>
        public static void Send(Faction f, Map map, int attempt)
        {
            if (f == null || map == null || f.HostileTo(Faction.OfPlayer)) return;
            if (StoryUtility.GuestsOnMap(map))
            {
                if (attempt < StoryUtility.VisitMaxTries)
                    GameComponent_Story.Instance?.Schedule("visit:" + f.loadID + ":" + (attempt + 1), StoryUtility.VisitRetryDays * GenDate.TicksPerDay);
                return;
            }
            var parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.Misc, map);
            parms.faction = f;
            parms.forced = true;
            IncidentDefOf.VisitorGroup.Worker.TryExecute(parms);
        }
    }

    public class SO_Hediff : StoryOutcome
    {
        public HediffDef hediff;
        /// <summary>random, all, cultivator</summary>
        public string target = "random";
        public float severity = 1f;
        protected override LookTargets Apply(StoryContext ctx)
        {
            var targets = target switch
            {
                "all" => ctx.Colonists.ToList(),
                "cultivator" => new List<Pawn> { ctx.RandomCultivator }.Where(p => p != null).ToList(),
                _ => new List<Pawn> { ctx.RandomColonist }.Where(p => p != null).ToList()
            };
            foreach (var p in targets)
            {
                var h = HediffMaker.MakeHediff(hediff, p);
                h.Severity = severity;
                p.health.AddHediff(h);
            }
            return targets.Count > 0 ? new LookTargets(targets) : null;
        }
    }

    public class SO_Thought : StoryOutcome
    {
        public ThoughtDef thought;
        public string target = "all";
        protected override LookTargets Apply(StoryContext ctx)
        {
            var targets = target == "all" ? ctx.Colonists.ToList() : new List<Pawn> { ctx.RandomColonist }.Where(p => p != null).ToList();
            foreach (var p in targets) p.needs?.mood?.thoughts?.memories?.TryGainMemory(thought);
            return null;
        }
    }

    public class SO_Mental : StoryOutcome
    {
        public MentalStateDef state;
        public int count = 1;
        protected override LookTargets Apply(StoryContext ctx)
        {
            var victims = ctx.Colonists.InRandomOrder().Take(count).ToList();
            foreach (var p in victims) p.mindState.mentalStateHandler.TryStartMentalState(state, null, forceWake: true);
            return victims.Count > 0 ? new LookTargets(victims) : null;
        }
    }

    public class SO_Condition : StoryOutcome
    {
        public GameConditionDef condition;
        public IntRange days = new IntRange(2, 4);
        protected override LookTargets Apply(StoryContext ctx)
        {
            if (condition == null || ctx.map.gameConditionManager.ConditionIsActive(condition)) return null;
            var c = GameConditionMaker.MakeCondition(condition, days.RandomInRange * GenDate.TicksPerDay);
            ctx.map.gameConditionManager.RegisterCondition(c);
            return null;
        }
    }

    /// <summary>Animales: hostiles (manhunter) o de paso.</summary>
    public class SO_Beasts : StoryOutcome
    {
        public List<PawnKindDef> kinds;
        public IntRange count = new IntRange(2, 4);
        public bool hostile;
        protected override LookTargets Apply(StoryContext ctx)
        {
            var kind = kinds.Where(k => k != null).RandomElementWithFallback();
            if (kind == null) return null;
            if (!RCellFinder.TryFindRandomPawnEntryCell(out var entry, ctx.map, CellFinder.EdgeRoadChance_Animal)) return null;
            var animals = new List<Pawn>();
            int n = count.RandomInRange;
            for (int i = 0; i < n; i++)
            {
                var a = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, null, PawnGenerationContext.NonPlayer, ctx.map.Tile));
                GenSpawn.Spawn(a, CellFinder.RandomClosewalkCellNear(entry, ctx.map, 8), ctx.map, Rot4.Random);
                SpiritBeastUtility.TryAwaken(a);
                if (hostile) a.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
                animals.Add(a);
            }
            if (!hostile) LordMaker.MakeNewLord(null, new LordJob_ExitMapBest(LocomotionUrgency.Walk, canDig: false, canDefendSelf: true), ctx.map, animals);
            return new LookTargets(animals);
        }
    }

    public class SO_Flag : StoryOutcome
    {
        public string set;
        public string clear;
        protected override LookTargets Apply(StoryContext ctx)
        {
            var gc = GameComponent_Story.Instance;
            if (!set.NullOrEmpty()) gc.Flags.Add(set);
            if (!clear.NullOrEmpty()) gc.Flags.Remove(clear);
            return null;
        }
    }

    /// <summary>Programa otro evento (siguiente paso de una cadena).</summary>
    public class SO_Chain : StoryOutcome
    {
        public StoryEventDef next;
        public IntRange days = new IntRange(3, 6);
        protected override LookTargets Apply(StoryContext ctx)
        {
            if (next != null) GameComponent_Story.Instance?.Schedule("ev:" + next.defName, days.RandomInRange * GenDate.TicksPerDay);
            return null;
        }
    }

    /// <summary>Comprensión súbita: progreso de cultivo a un cultivador.</summary>
    public class SO_Progress : StoryOutcome
    {
        public float amount = 300f;
        protected override LookTargets Apply(StoryContext ctx)
        {
            var p = ctx.RandomCultivator;
            var comp = p == null ? null : CompCultivator.For(p);
            if (comp == null || !comp.IsQiCultivator) return null;
            comp.Realm.AddProgress(amount);
            return p;
        }
    }

    public class SO_Inspire : StoryOutcome
    {
        public InspirationDef inspiration;
        protected override LookTargets Apply(StoryContext ctx)
        {
            var p = ctx.RandomColonist;
            p?.mindState.inspirationHandler.TryStartInspiration(inspiration ?? DefDatabase<InspirationDef>.AllDefsListForReading.RandomElement());
            return p;
        }
    }

    public class SO_Damage : StoryOutcome
    {
        public DamageDef damage;
        public float amount = 8f;
        public int hits = 2;
        protected override LookTargets Apply(StoryContext ctx)
        {
            var p = ctx.RandomColonist;
            if (p == null) return null;
            for (int i = 0; i < hits; i++) p.TakeDamage(new DamageInfo(damage ?? DamageDefOf.Cut, amount));
            return p;
        }
    }

    public class SO_Skill : StoryOutcome
    {
        public SkillDef skill;
        public float xp = 4000f;
        protected override LookTargets Apply(StoryContext ctx)
        {
            var p = ctx.RandomColonist;
            p?.skills?.GetSkill(skill ?? SkillDefOf.Melee)?.Learn(xp, direct: true);
            return p;
        }
    }

    /// <summary>Misión de caza: aparece una bestia; al matarla (a tiempo) ocurre el evento de éxito, si no el de fracaso.</summary>
    public class SO_Hunt : StoryOutcome
    {
        public PawnKindDef kind;
        public IntRange count = new IntRange(1, 1);
        public StoryEventDef success;
        public StoryEventDef fail;
        public int days = 8;
        protected override LookTargets Apply(StoryContext ctx)
        {
            if (kind == null) return null;
            var cell = CellFinderLoose.TryGetRandomCellWith(c => c.Standable(ctx.map) && !c.Roofed(ctx.map) && c.DistanceToEdge(ctx.map) > 10 && c.Walkable(ctx.map), ctx.map, 200, out var found) ? found : ctx.map.Center;
            var task = new StoryTask { success = success?.defName, fail = fail?.defName, expireTick = Find.TickManager.TicksGame + days * GenDate.TicksPerDay };
            var list = new List<Pawn>();
            for (int i = 0; i < count.RandomInRange; i++)
            {
                var a = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, null, PawnGenerationContext.NonPlayer, ctx.map.Tile));
                GenSpawn.Spawn(a, CellFinder.RandomClosewalkCellNear(cell, ctx.map, 6), ctx.map);
                SpiritBeastUtility.TryAwaken(a);
                task.pawnIds.Add(a.thingIDNumber);
                list.Add(a);
            }
            GameComponent_Story.Instance.Tasks.Add(task);
            return new LookTargets(list);
        }
    }

    // ====================================================================== motor

    public static class StoryEngine
    {
        /// <summary>Multiplicador de la frecuencia de eventos (lo fija la dificultad del Core; 0 = sin eventos de historia).</summary>
        public static Func<float> FrequencyFactor = () => 1f;

        public static IEnumerable<StoryEventDef> All => DefDatabase<StoryEventDef>.AllDefsListForReading;

        private static readonly Dictionary<StoryEventDef, bool> bringsVisitors = new Dictionary<StoryEventDef, bool>();

        /// <summary>El evento trae un grupo de visitantes (<see cref="SO_Visit"/>, directo o en alguna opción).</summary>
        public static bool BringsVisitors(StoryEventDef def)
        {
            if (!bringsVisitors.TryGetValue(def, out bool v))
            {
                v = (def.outcomes?.Any(o => o is SO_Visit) ?? false) || (def.options?.Any(o => o.outcomes?.Any(x => x is SO_Visit) ?? false) ?? false);
                bringsVisitors[def] = v;
            }
            return v;
        }

        public static bool Eligible(StoryEventDef def, Map map, bool ignoreTiming = false)
        {
            var gc = GameComponent_Story.Instance;
            var conv = GameComponent_Convergence.Instance;
            if (gc == null || conv == null) return false;
            if (!ignoreTiming)
            {
                if (def.chainOnly) return false;
                if (def.pre ? conv.Occurred : !conv.Occurred) return false;
                if (!def.pre && Find.TickManager.TicksGame - conv.OccurredTick < def.minDays * GenDate.TicksPerDay) return false;
                if (def.pre && Find.TickManager.TicksGame < def.minDays * GenDate.TicksPerDay) return false;
                if (gc.LastFired.TryGetValue(def.defName, out int last) && Find.TickManager.TicksGame - last < def.minRefireDays * GenDate.TicksPerDay) return false;
                if (BringsVisitors(def) && StoryUtility.GuestsOnMap(map)) return false;
            }
            if (map.mapPawns.FreeColonistsSpawnedCount < def.minColonists) return false;
            if (def.needsCultivator && !map.mapPawns.FreeColonistsSpawned.Any(p => CompCultivator.For(p)?.IsCultivator ?? false)) return false;
            if (!def.requiresFlags.NullOrEmpty() && def.requiresFlags.Any(f => !gc.Flags.Contains(f))) return false;
            if (!def.forbidsFlags.NullOrEmpty() && def.forbidsFlags.Any(f => gc.Flags.Contains(f))) return false;
            if (def.isThreat && (GenHostility.AnyHostileActiveThreatToPlayer(map) || gc.Flags.Contains("rift_sealed"))) return false;
            if (def.requiresFaction && def.factionToken != null && new StoryContext(map, def).Resolve(def.factionToken) == null) return false;
            return true;
        }

        public static bool Fire(StoryEventDef def, Map map)
        {
            var ctx = new StoryContext(map, def);
            if (def.requiresFaction && ctx.Primary == null) return false;
            var gc = GameComponent_Story.Instance;
            gc.LastFired[def.defName] = Find.TickManager.TicksGame;
            string key = "CVG_Ev_" + def.defName;
            var args = ctx.Args();
            // resolver las facciones del texto antes de serializar
            if (def.factionToken != null) ctx.Resolve(def.factionToken);
            if (def.factionToken2 != null) ctx.Resolve(def.factionToken2);
            if (!def.options.NullOrEmpty())
            {
                var letter = (ChoiceLetter_Story)LetterMaker.MakeLetter(DefDatabase<LetterDef>.GetNamed("CVG_StoryLetter"));
                letter.Label = (key + "_Label").Translate(args);
                letter.title = letter.Label;
                letter.Text = (key + "_Text").Translate(args);
                letter.storyKey = def.defName;
                letter.options = def.options.Select(o => o.key).ToList();
                letter.payload = ctx.Serialize();
                letter.lookTargets = LookTargets.Invalid;
                letter.StartTimeout(GenDate.TicksPerDay * Mathf.Max(1, def.timeoutDays));
                Find.LetterStack.ReceiveLetter(letter);
            }
            else
            {
                Find.LetterStack.ReceiveLetter((key + "_Label").Translate(args), (key + "_Text").Translate(args), def.LetterDef);
            }
            if (!def.outcomes.NullOrEmpty()) foreach (var o in def.outcomes) o.TryApply(ctx);
            return true;
        }

        public static void Decide(StoryEventDef def, string option, string payload)
        {
            var map = StoryUtility.HomeMap;
            if (map == null) return;
            var ctx = StoryContext.Deserialize(map, def, payload);
            var opt = def.options?.FirstOrDefault(o => o.key == option);
            if (opt == null) return;
            if (opt.costThing != null) StoryUtility.TakeFromColony(map, opt.costThing, opt.costCount.RandomInRange);
            if (!opt.outcomes.NullOrEmpty()) foreach (var o in opt.outcomes) o.TryApply(ctx);
            string res = "CVG_Ev_" + def.defName + "_Res_" + option;
            if (res.TryTranslate(out var text)) Messages.Message(text.Formatted(ctx.Args()), MessageTypeDefOf.NeutralEvent, historical: false);
        }

        private const int ScheduledRetries = 10;

        /// <summary>Un evento programado ("ev:&lt;defName&gt;[#intento]") o de cadena. Si justo no puede ocurrir (hay una amenaza
        /// en el mapa, huéspedes, falta un cultivador) se aplaza un día en vez de perderse: así las cadenas y las cacerías no se
        /// cortan en silencio.</summary>
        public static void RunScheduled(string arg)
        {
            int hash = arg.IndexOf('#');
            string defName = hash < 0 ? arg : arg.Substring(0, hash);
            int attempt = hash < 0 ? 0 : int.TryParse(arg.Substring(hash + 1), out int n) ? n : 0;
            var def = DefDatabase<StoryEventDef>.GetNamedSilentFail(defName);
            var map = StoryUtility.HomeMap;
            if (def == null || map == null) return;
            if (Eligible(def, map, ignoreTiming: true))
            {
                Fire(def, map);
                return;
            }
            if (attempt < ScheduledRetries)
                GameComponent_Story.Instance?.Schedule("ev:" + defName + "#" + (attempt + 1), GenDate.TicksPerDay);
            else
                Log.Warning("[Convergence] Story event " + defName + " could not happen after " + ScheduledRetries + " days; dropped.");
        }

        public static bool CanAffordOption(StoryOption o, Map map, out string reason)
        {
            reason = null;
            if (o.costThing == null) return true;
            int need = o.costCount.max;
            if (map.resourceCounter.GetCount(o.costThing) >= need) return true;
            reason = "CVG_Story_NeedItems".Translate(need, o.costThing.label);
            return false;
        }

        public static bool PickAndFire(Map map) => PickAndFire(map, threat: null);

        /// <summary>Elige y dispara un evento elegible (<paramref name="threat"/>: solo amenazas / solo no amenazas / cualquiera).</summary>
        public static bool PickAndFire(Map map, bool? threat)
        {
            var pool = All.Where(d => (threat == null || d.isThreat == threat.Value) && Eligible(d, map)).ToList();
            if (pool.Count == 0) return false;
            return Fire(pool.RandomElementByWeight(d => d.weight), map);
        }

        public static bool AnyEligible(Map map, bool threat) => All.Any(d => d.isThreat == threat && Eligible(d, map));
    }
}

namespace Convergence.Stories
{
    /// <summary>
    /// Los eventos de historia pasan por el narrador vanilla (plan de integración, 2026-10-23): un incidente común y uno de
    /// amenaza que eligen un <see cref="StoryEventDef"/> elegible. Así Cassandra, Phoebe o Randy deciden cuándo, la dificultad y
    /// "desactivar incidentes" funcionan, y no hay un reloj aparte.
    /// </summary>
    public class IncidentWorker_StoryEvent : IncidentWorker
    {
        protected bool Threat => def.category == IncidentCategoryDefOf.ThreatSmall || def.category == IncidentCategoryDefOf.ThreatBig;

        protected override bool CanFireNowSub(IncidentParms parms) =>
            parms.target is Map map && map.IsPlayerHome && StoryEngine.FrequencyFactor() > 0f && StoryEngine.AnyEligible(map, Threat);

        protected override bool TryExecuteWorker(IncidentParms parms) => StoryEngine.PickAndFire((Map)parms.target, Threat);
    }
}
