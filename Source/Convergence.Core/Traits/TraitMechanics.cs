using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>Velocidad de cultivo según el reino (Floración Tardía, Genio Efímero): curva orden de reino → multiplicador.</summary>
    public class DefModExtension_RealmCultivationCurve : DefModExtension
    {
        public SimpleCurve curve;
    }

    /// <summary>
    /// Cultivo dual (Pareja Dao, rival de por vida…) en un <c>PawnRelationDef</c>: si el otro está cerca y también cultiva,
    /// la velocidad de cultivo se multiplica. Con <see cref="vowFromRelations"/>, dos cultivadores despiertos con esa
    /// relación (amantes, prometidos, esposos) pueden hacer el voto que crea esta relación (botón del colono).
    /// </summary>
    public class DefModExtension_DualCultivation : DefModExtension
    {
        public float factor = 1.25f;
        public float radius = 10f;
        public bool requireBothCultivating = true;
        public List<PawnRelationDef> vowFromRelations;
    }

    /// <summary>Premonición (Regresor): un asalto contra el jugador se retrasa unas horas y llega con aviso.</summary>
    public class DefModExtension_RaidPremonition : DefModExtension
    {
        public float delayHours = 6f;
    }

    /// <summary>Etiqueta de oficio espiritual (alquimia, formaciones, talismanes…) para que la usen módulos futuros.</summary>
    public class DefModExtension_SpiritCraft : DefModExtension
    {
        [NoTranslate] public string craft;
    }

    /// <summary>Hooks de rasgos y relaciones: StatParts sobre la velocidad de cultivo, voto de Pareja Dao, vidas por rasgo.</summary>
    [StaticConstructorOnStartup]
    public static class TraitMechanics
    {
        static TraitMechanics()
        {
            var stat = CVG_DefOf.CVG_CultivationSpeed;
            stat.parts ??= new List<StatPart>();
            stat.parts.Add(new StatPart_RealmCurveTraits { parentStat = stat });
            stat.parts.Add(new StatPart_DualCultivation { parentStat = stat });
        }

        public static bool IsCultivating(Pawn p) => p?.CurJobDef == CVG_DefOf.CVG_Cultivate;

        /// <summary>Relaciones de cultivo dual del colono con su pareja cercana que cumple las condiciones.</summary>
        public static float DualFactor(Pawn pawn, out Pawn partner)
        {
            partner = null;
            float best = 1f;
            if (pawn?.relations == null || !pawn.Spawned) return best;
            foreach (var rel in pawn.relations.DirectRelations)
            {
                var ext = rel.def.GetModExtension<DefModExtension_DualCultivation>();
                var other = rel.otherPawn;
                if (ext == null || other == null || !other.Spawned || other.Map != pawn.Map || other.Dead) continue;
                if (!other.Position.InHorDistOf(pawn.Position, ext.radius)) continue;
                if (ext.requireBothCultivating && !IsCultivating(other)) continue;
                if (ext.factor > best) { best = ext.factor; partner = other; }
            }
            return best;
        }
    }

    public class StatPart_RealmCurveTraits : StatPart
    {
        private static float Factor(Pawn pawn, out TraitDef source)
        {
            source = null;
            float f = 1f;
            var traits = pawn?.story?.traits?.allTraits;
            if (traits == null) return f;
            for (int i = 0; i < traits.Count; i++)
            {
                var curve = traits[i].def.GetModExtension<DefModExtension_RealmCultivationCurve>()?.curve;
                if (curve == null) continue;
                var comp = CompCultivator.For(pawn);
                int order = comp != null && comp.IsQiCultivator && comp.Realm.Started ? comp.Realm.Realm.order : 1;
                f *= curve.Evaluate(order);
                source = traits[i].def;
            }
            return f;
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.Thing is Pawn pawn) val *= Factor(pawn, out _);
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!(req.Thing is Pawn pawn)) return null;
            float f = Factor(pawn, out var source);
            return source == null || Mathf.Approximately(f, 1f) ? null : source.LabelCap + ": x" + f.ToStringPercent();
        }
    }

    public class StatPart_DualCultivation : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.Thing is Pawn pawn) val *= TraitMechanics.DualFactor(pawn, out _);
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!(req.Thing is Pawn pawn)) return null;
            float f = TraitMechanics.DualFactor(pawn, out var partner);
            return partner == null ? null : "CVG_DualCultivation".Translate(partner.LabelShort).Resolve() + ": x" + f.ToStringPercent();
        }
    }

    /// <summary>Botón "Voto de Pareja Dao" (o el voto de cualquier relación con <c>vowFromRelations</c>).</summary>
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.GetGizmos))]
    public static class Patch_Pawn_GetGizmos_DaoVow
    {
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> gizmos, Pawn __instance)
        {
            foreach (var g in gizmos) yield return g;
            var pawn = __instance;
            if (!pawn.IsColonistPlayerControlled || pawn.relations == null || !GameComponent_Convergence.Active) yield break;
            if (CompCultivator.For(pawn)?.IsCultivator != true) yield break;
            foreach (var def in DefDatabase<PawnRelationDef>.AllDefsListForReading)
            {
                var ext = def.GetModExtension<DefModExtension_DualCultivation>();
                if (ext?.vowFromRelations == null) continue;
                foreach (var rel in pawn.relations.DirectRelations.ToList())
                {
                    var other = rel.otherPawn;
                    if (!ext.vowFromRelations.Contains(rel.def) || other == null || other.Dead) continue;
                    if (pawn.relations.DirectRelationExists(def, other) || CompCultivator.For(other)?.IsCultivator != true) continue;
                    var vowDef = def;
                    yield return new Command_Action
                    {
                        defaultLabel = "CVG_Vow_Label".Translate(def.label, other.LabelShort),
                        defaultDesc = "CVG_Vow_Desc".Translate(def.label, other.LabelShort, (ext.factor - 1f).ToStringPercent()),
                        icon = TexCommand.Attack,
                        action = () => TraitMechanics_Vow.MakeVow(pawn, other, vowDef)
                    };
                }
            }
        }
    }

    public static class TraitMechanics_Vow
    {
        public static void MakeVow(Pawn a, Pawn b, PawnRelationDef def)
        {
            if (a.relations.DirectRelationExists(def, b)) return;
            a.relations.AddDirectRelation(def, b);
            Find.LetterStack.ReceiveLetter("CVG_Vow_LetterLabel".Translate(def.label),
                "CVG_Vow_LetterText".Translate(a.Named("A"), b.Named("B"), def.label), LetterDefOf.PositiveEvent, new LookTargets(a, b));
        }
    }

    /// <summary>Regresor: un asalto contra el jugador se retrasa y avisa (una vez por asalto). La marca de "ya retrasado" se
    /// guarda con la partida (tick de disparo + facción): el asalto vuelve deserializado tras cargar y es otro objeto.</summary>
    [HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "TryExecuteWorker")]
    public static class Patch_RaidPremonition
    {
        public static bool Prefix(IncidentWorker_RaidEnemy __instance, IncidentParms parms, ref bool __result)
        {
            if (!(parms.target is Map map) || !map.IsPlayerHome || parms.quest != null) return true;
            if (parms.faction == null || !parms.faction.HostileTo(Faction.OfPlayer)) return true;
            var marks = Current.Game.GetComponent<GameComponent_RaidPremonition>();
            if (marks == null || marks.Consume(parms.faction)) return true;
            Pawn seer = null;
            DefModExtension_RaidPremonition ext = null;
            foreach (var p in map.mapPawns.FreeColonistsSpawned)
            {
                var t = p.story?.traits?.allTraits.FirstOrDefault(x => x.def.HasModExtension<DefModExtension_RaidPremonition>());
                if (t != null) { seer = p; ext = t.def.GetModExtension<DefModExtension_RaidPremonition>(); break; }
            }
            if (seer == null) return true;
            int fireTick = Find.TickManager.TicksGame + Mathf.RoundToInt(ext.delayHours * GenDate.TicksPerHour);
            marks.Mark(fireTick, parms.faction);
            Find.Storyteller.incidentQueue.Add(new QueuedIncident(new FiringIncident(__instance.def, null, parms), fireTick));
            Find.LetterStack.ReceiveLetter("CVG_Premonition_Label".Translate(), "CVG_Premonition_Text".Translate(seer.Named("PAWN"),
                parms.faction.Name, ext.delayHours.ToString("0")), LetterDefOf.ThreatBig, seer);
            __result = true;
            return false;
        }
    }

    /// <summary>Asaltos ya retrasados por una premonición: tick de disparo y facción, guardados con la partida.</summary>
    public class GameComponent_RaidPremonition : GameComponent
    {
        private List<int> ticks = new List<int>();
        private List<Faction> factions = new List<Faction>();

        public GameComponent_RaidPremonition(Game game)
        {
        }

        public void Mark(int tick, Faction f)
        {
            ticks.Add(tick);
            factions.Add(f);
        }

        /// <summary>true si este asalto de la facción ya fue retrasado (la cola lo dispara en su tick o poco después).</summary>
        public bool Consume(Faction f)
        {
            int now = Find.TickManager.TicksGame;
            for (int i = ticks.Count - 1; i >= 0; i--)
            {
                if (factions[i] == null || now - ticks[i] > GenDate.TicksPerDay) { ticks.RemoveAt(i); factions.RemoveAt(i); continue; }
                if (factions[i] == f && now >= ticks[i]) { ticks.RemoveAt(i); factions.RemoveAt(i); return true; }
            }
            return false;
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref ticks, "ticks", LookMode.Value);
            Scribe_Collections.Look(ref factions, "factions", LookMode.Reference);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                ticks ??= new List<int>();
                factions ??= new List<Faction>();
                if (ticks.Count != factions.Count) { ticks.Clear(); factions.Clear(); }
            }
        }
    }

    /// <summary>Vidas por rasgo (Hijo del Cielo): estado por colono, guardado en la partida.</summary>
    public class GameComponent_TraitLives : GameComponent
    {
        private Dictionary<int, int> lastUsed = new Dictionary<int, int>();

        public GameComponent_TraitLives(Game game)
        {
        }

        public static GameComponent_TraitLives Instance => Current.Game?.GetComponent<GameComponent_TraitLives>();

        /// <summary>Si el rasgo con vidas extra está disponible, lo gasta y salva al colono.</summary>
        public bool TrySave(Pawn pawn)
        {
            var trait = pawn.story?.traits?.allTraits.FirstOrDefault(t => t.def.HasModExtension<DefModExtension_ExtraLives>());
            if (trait == null) return false;
            var ext = trait.def.GetModExtension<DefModExtension_ExtraLives>();
            int now = Find.TickManager.TicksGame;
            if (lastUsed.TryGetValue(pawn.thingIDNumber, out int last) && (ext.rechargeDays <= 0f || now - last < ext.rechargeDays * GenDate.TicksPerDay))
                return false;
            lastUsed[pawn.thingIDNumber] = now;
            BloodlineMechanics.HealFromDeath(pawn);
            if (pawn.Faction == Faction.OfPlayer)
                Find.LetterStack.ReceiveLetter("CVG_ExtraLife_Label".Translate(pawn.Named("PAWN")),
                    ext.messageKey.Translate(pawn.Named("PAWN"), ext.rechargeDays.ToString("0")), LetterDefOf.PositiveEvent, pawn);
            return true;
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref lastUsed, "lastUsed", LookMode.Value, LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit) lastUsed ??= new Dictionary<int, int>();
        }
    }
}
