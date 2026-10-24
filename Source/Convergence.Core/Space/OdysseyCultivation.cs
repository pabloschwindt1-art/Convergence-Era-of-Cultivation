using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace Convergence
{
    // Odyssey (plan de fusión, 2026-10-23): la cultivación dentro de lo que trae Odyssey. En el espacio el qi es raro; los
    // reinos olvidados (fragmentos de Jiuxiao que vagan en órbita) tienen qi altísimo, guardianes y botín; los lugares
    // notables del planeta pueden traer vetas espirituales, tesoros de secta o cultivadores caídos (mutadores de casilla).

    /// <summary>
    /// Qi ambiental de un mapa: en un BiomeDef (el espacio: poco qi) o en un SitePartDef (reino olvidado: muchísimo). Lo usa
    /// <see cref="QiConcentrationUtility.AmbientFor"/>.
    /// </summary>
    public class DefModExtension_MapQi : DefModExtension
    {
        public float ambientFactor = 1f;
        public float ambientOffset;
    }

    public static class MapQi
    {
        public static float AmbientFor(Map map, float ambient)
        {
            if (map == null) return ambient;
            var biome = map.Biome?.GetModExtension<DefModExtension_MapQi>();
            if (biome != null) ambient = ambient * biome.ambientFactor + biome.ambientOffset;
            if (map.Parent is Site site)
                foreach (var part in site.parts)
                {
                    var ext = part.def.GetModExtension<DefModExtension_MapQi>();
                    if (ext != null) ambient = ambient * ext.ambientFactor + ext.ambientOffset;
                }
            return Mathf.Max(0f, ambient);
        }
    }

    /// <summary>Solo tras la Convergencia (los reinos olvidados y lo espiritual de Odyssey no existen antes).</summary>
    public class QuestNode_ConvergenceActive : QuestNode
    {
        protected override bool TestRunInt(Slate slate) => GameComponent_Convergence.Active;

        protected override void RunInt()
        {
        }
    }

    /// <summary>Una opción de botín de un reino olvidado: cosa, cantidad y peso.</summary>
    public class ForgottenRealmLoot
    {
        public ThingDef thing;
        public IntRange count = IntRange.One;
        public float weight = 1f;
    }

    /// <summary>
    /// Reino olvidado (sobre la plataforma orbital vanilla): botín del mod de calidad superior en las salas (tecnología,
    /// jades de inspiración, materiales, píldoras) y guardianes: mecanoides hostiles con núcleo de qi, que sí sobreviven al
    /// vacío. Los puntos de amenaza del sitio deciden cuántos.
    /// </summary>
    public class GenStep_ForgottenRealm : GenStep
    {
        public List<ForgottenRealmLoot> loot = new List<ForgottenRealmLoot>();
        public IntRange lootStacks = new IntRange(6, 10);
        public QualityCategory minQuality = QualityCategory.Excellent;
        public List<PawnKindDef> guardians = new List<PawnKindDef>();
        public FloatRange guardianPointsFactor = new FloatRange(0.6f, 0.9f);
        /// <summary>Condición de núcleo de qi (de Qi-Tech, opcional): por nombre, se busca al generar.</summary>
        [NoTranslate] public string guardianCore;
        public IntRange guardianGrade = new IntRange(2, 3);

        public override int SeedPart => 913407761;

        public override void Generate(Map map, GenStepParams parms)
        {
            var inside = map.AllCells.Where(c => c.Standable(map) && c.Roofed(map)).ToList();
            if (inside.Count == 0) inside = map.AllCells.Where(c => c.Standable(map)).ToList();
            if (inside.Count == 0) return;
            PlaceLoot(map, inside);
            SpawnGuardians(map, inside, parms.sitePart?.parms?.threatPoints ?? 300f);
        }

        private void PlaceLoot(Map map, List<IntVec3> cells)
        {
            var options = loot.Where(l => l.thing != null).ToList();
            if (options.Count == 0) return;
            int stacks = lootStacks.RandomInRange;
            for (int i = 0; i < stacks; i++)
            {
                var option = options.RandomElementByWeight(l => l.weight);
                var thing = ThingMaker.MakeThing(option.thing, option.thing.MadeFromStuff ? GenStuff.RandomStuffFor(option.thing) : null);
                thing.stackCount = Mathf.Clamp(option.count.RandomInRange, 1, option.thing.stackLimit);
                var quality = thing.TryGetComp<CompQuality>();
                quality?.SetQuality((QualityCategory)Rand.RangeInclusive((int)minQuality, (int)QualityCategory.Legendary), ArtGenerationContext.Outsider);
                GenPlace.TryPlaceThing(thing, cells.RandomElement(), map, ThingPlaceMode.Near);
            }
        }

        private void SpawnGuardians(Map map, List<IntVec3> cells, float threatPoints)
        {
            var kinds = guardians.Where(k => k != null).ToList();
            var faction = Faction.OfMechanoids;
            if (kinds.Count == 0 || faction == null) return;
            float points = threatPoints * guardianPointsFactor.RandomInRange;
            var spawned = new List<Pawn>();
            int guard = 0;
            while (points > 0f && guard++ < 20)
            {
                var kind = kinds.RandomElement();
                var pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, faction));
                var core = guardianCore != null ? DefDatabase<HediffDef>.GetNamedSilentFail(guardianCore) : null;
                if (core != null && !pawn.health.hediffSet.HasHediff(core))
                {
                    var h = pawn.health.AddHediff(core);
                    h.Severity = guardianGrade.RandomInRange;
                }
                GenSpawn.Spawn(pawn, cells.RandomElement(), map);
                spawned.Add(pawn);
                points -= Mathf.Max(50f, kind.combatPower);
            }
            if (spawned.Count > 0)
                LordMaker.MakeNewLord(faction, new LordJob_DefendPoint(spawned[0].Position, addFleeToil: false), map, spawned);
        }
    }

    // ------------------------------------------------------------------ lugares notables del planeta (mutadores de casilla)

    /// <summary>Vetas espirituales extra en la roca (piedra y jade espirituales), solo tras la Convergencia.</summary>
    public class TileMutatorWorker_SpiritVeins : TileMutatorWorker
    {
        public TileMutatorWorker_SpiritVeins(TileMutatorDef def) : base(def)
        {
        }

        public override void GenerateNonCriticalStructures(Map map)
        {
            if (!GameComponent_Convergence.Active) return;
            foreach (var name in new[] { "CVG_MineableSpiritStone", "CVG_MineableSpiritJade" })
            {
                var vein = DefDatabase<ThingDef>.GetNamedSilentFail(name);
                if (vein != null) ConvergenceMineables.SpawnLumps(map, vein, Rand.RangeInclusive(4, 7), new IntRange(5, 12));
            }
        }
    }

    /// <summary>Un escondite de una secta que no sobrevivió a la Convergencia: piedras, jade, tablillas y alguna píldora.</summary>
    public class TileMutatorWorker_SectCache : TileMutatorWorker
    {
        public TileMutatorWorker_SectCache(TileMutatorDef def) : base(def)
        {
        }

        private static readonly (string def, int min, int max)[] Contents =
        {
            ("CVG_SpiritStone", 40, 120), ("CVG_SpiritJade", 10, 30), ("CVG_JadeSlip", 1, 2), ("CVG_FoundationPill", 0, 1), ("CVG_PrimordialCrystal", 1, 3)
        };

        public override void GenerateNonCriticalStructures(Map map)
        {
            if (!GameComponent_Convergence.Active) return;
            if (!CellFinder.TryFindRandomCell(map, c => c.Standable(map) && !c.InNoBuildEdgeArea(map) && c.GetFirstItem(map) == null, out var center)) return;
            foreach (var (name, min, max) in Contents)
            {
                var def = DefDatabase<ThingDef>.GetNamedSilentFail(name);
                int count = Rand.RangeInclusive(min, max);
                if (def == null || count <= 0) continue;
                var thing = ThingMaker.MakeThing(def);
                thing.stackCount = Mathf.Min(count, def.stackLimit);
                GenPlace.TryPlaceThing(thing, center, map, ThingPlaceMode.Near);
            }
        }
    }

    /// <summary>Un cultivador que murió buscando este lugar: su cuerpo, su arma y lo que llevaba encima.</summary>
    public class TileMutatorWorker_FallenCultivator : TileMutatorWorker
    {
        public TileMutatorWorker_FallenCultivator(TileMutatorDef def) : base(def)
        {
        }

        public override void GenerateNonCriticalStructures(Map map)
        {
            if (!GameComponent_Convergence.Active) return;
            if (!CellFinder.TryFindRandomCell(map, c => c.Standable(map) && !c.InNoBuildEdgeArea(map), out var cell)) return;
            var pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Villager, null, forceGenerateNewPawn: true,
                developmentalStages: DevelopmentalStage.Adult));
            GenSpawn.Spawn(pawn, cell, map);
            pawn.Kill(null);
            foreach (var (name, count) in new[] { ("CVG_JadeSlip", 1), ("CVG_SpiritStone", Rand.RangeInclusive(20, 60)) })
            {
                var def = DefDatabase<ThingDef>.GetNamedSilentFail(name);
                if (def == null) continue;
                var thing = ThingMaker.MakeThing(def);
                thing.stackCount = count;
                GenPlace.TryPlaceThing(thing, cell, map, ThingPlaceMode.Near);
            }
        }
    }
}
