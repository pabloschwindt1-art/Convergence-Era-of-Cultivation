using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Convergence
{
    /// <summary>
    /// En un <c>PawnKindDef</c>: los humanoides de este tipo salen ya cultivando. Fija raíz, reino, etapa y manuales
    /// (módulo Sects, mercenarios, bandidos con qi…). Solo actúa tras la Convergencia.
    /// </summary>
    public class DefModExtension_CultivatorPawnKind : DefModExtension
    {
        /// <summary>Reino objetivo (null = el primero del camino de qi).</summary>
        public CultivationRealmDef realm;
        /// <summary>Etapa dentro del reino (se limita a las etapas que tenga).</summary>
        public IntRange stage = new IntRange(1, 1);
        /// <summary>Manuales preferidos; si ninguno es aprendible se usa el reparto normal.</summary>
        public List<TechniqueManualDef> manuals;
        /// <summary>Elementos de la raíz si hay que dársela (null = al azar). Se toma 1 al azar de esta lista y, con <c>extraElementChance</c>, un segundo.</summary>
        public List<QiElementDef> elements;
        public float extraElementChance = 0.3f;
        /// <summary>Grados de raíz permitidos al darla (null = cualquiera).</summary>
        public List<SpiritRootGradeDef> grades;
        /// <summary>Si es true y el pawn es hostil al jugador, su reino se limita al de la colonia (+1 como mucho).</summary>
        public bool scaleWithColony = true;
        /// <summary>
        /// Rasgos con probabilidad (constituciones de secta: Cuerpo de Espada común, superior…). Se prueban en orden y se queda el primero
        /// que sale: poner los más raros primero.
        /// </summary>
        public List<TraitChance> traitChances;
        /// <summary>Tipos de cuerpo que esta secta no tiene (p. ej. Hulk en la Espada Sin Forma); se cambia por uno permitido.</summary>
        public List<BodyTypeDef> disallowedBodyTypes;
        /// <summary>Tipo de cuerpo de todos sus miembros (p. ej. Hulk en los Nueve Templados).</summary>
        public BodyTypeDef forcedBodyType;
    }

    /// <summary>Un rasgo (con su grado) que un tipo de pawn recibe con cierta probabilidad.</summary>
    public class TraitChance
    {
        public TraitDef trait;
        public int degree;
        public float chance = 1f;
    }

    /// <summary>En un <c>FactionDef</c>: la facción no existe hasta la Convergencia; entonces llega al mundo.</summary>
    public class DefModExtension_ConvergenceFaction : DefModExtension
    {
        /// <summary>Asentamientos adicionales al primero (que crea el propio juego).</summary>
        public IntRange extraSettlements = new IntRange(1, 2);
        public List<FactionDef> hostileTo;
        public List<FactionDef> alliedTo;
        /// <summary>Si la ideología principal del jugador tiene alguno de estos memes, la facción llega neutral con él (p. ej. el Culto con el Camino Demoníaco).</summary>
        public List<MemeDef> neutralToPlayerMemes;
    }

    public static class CultivatorPawnKindUtility
    {
        /// <summary>Reino más alto (order) entre los cultivadores libres de la colonia del jugador.</summary>
        public static int ColonyMaxRealm()
        {
            int max = 0;
            foreach (var p in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists)
            {
                var comp = CompCultivator.For(p);
                if (comp != null) max = System.Math.Max(max, comp.RealmLevel);
            }
            return max;
        }

        /// <summary>Aplica la extensión del tipo del pawn (si la tiene). Devuelve true si lo convirtió en cultivador.</summary>
        public static bool Apply(Pawn pawn)
        {
            var ext = pawn.kindDef?.GetModExtension<DefModExtension_CultivatorPawnKind>();
            if (ext == null || pawn.RaceProps?.Humanlike != true || pawn.story?.traits == null) return false;
            ApplyBodyAndTraits(pawn, ext);
            var comp = CompCultivator.For(pawn);
            if (comp == null) return false;

            comp.Sync();
            RealmTracker tracker;
            if (ext.realm?.path != null && ext.realm.path != CVG_DefOf.CVG_Path_Qi)
            {
                // Otro camino (p. ej. el del cuerpo, Nueve Templados): no usa raíz espiritual, y el del cuerpo exige no tenerla.
                if (comp.HasRoot && ext.realm.path.requiresSpiritRoot == false)
                {
                    CultivationGenerator.RemoveRoot(pawn);
                    comp.Sync();
                }
                tracker = comp.StartPath(ext.realm.path);
                if (tracker == null || !tracker.EnsureStarted()) return false;
            }
            else
            {
                if (!comp.HasRoot)
                {
                    var grade = ext.grades.NullOrEmpty()
                        ? CultivationGenerator.RandomGrade()
                        : ext.grades.RandomElementByWeight(g => g.commonality);
                    CultivationGenerator.GiveRoot(pawn, grade, PickElements(ext));
                    comp.Sync();
                }
                if (!comp.IsQiCultivator) return false;
                tracker = comp.Realm;
                if (!tracker.EnsureStarted()) return false;
            }

            var target = ext.realm ?? tracker.Realm;
            if (ext.scaleWithColony && CultivationDifficulty.Current.scaleEnemyRealms && pawn.Faction != null
                && pawn.Faction.HostileTo(Faction.OfPlayer))
            {
                int cap = ColonyMaxRealm() + CultivationDifficulty.Current.enemyRealmBonus;
                while (target.order > cap && target.order > tracker.Realm.order)
                {
                    var lower = DefDatabase<CultivationRealmDef>.AllDefsListForReading
                        .Where(r => r.path == target.path && r.order < target.order).OrderByDescending(r => r.order).FirstOrDefault();
                    if (lower == null) break;
                    target = lower;
                }
            }

            if (target.order > tracker.Realm.order) tracker.AdvanceTo(target, 1f);
            tracker.SetStage(ext.stage.RandomInRange);
            comp.InvalidateHealthScale();

            if (!ext.manuals.NullOrEmpty())
                TechniqueUtility.GiveNpcTechniques(pawn, ext.manuals);
            return true;
        }

        /// <summary>
        /// Un miembro de secta nunca lleva un arma que no puede usar (de fuego sin poder disparar, cualquiera sin poder pelear):
        /// se la quita y queda con sus puños y sus técnicas.
        /// </summary>
        public static void DropUnusableWeapon(Pawn pawn)
        {
            var w = pawn.equipment?.Primary;
            if (w == null) return;
            bool unusable = pawn.WorkTagIsDisabled(WorkTags.Violent)
                || (w.def.IsRangedWeapon && pawn.skills?.GetSkill(SkillDefOf.Shooting)?.TotallyDisabled == true)
                || (w.def.IsMeleeWeapon && pawn.skills?.GetSkill(SkillDefOf.Melee)?.TotallyDisabled == true);
            if (unusable) pawn.equipment.DestroyEquipment(w);
        }

        /// <summary>Tipo de cuerpo de la secta (forzado o sin los prohibidos) y su constitución con probabilidad.</summary>
        public static void ApplyBodyAndTraits(Pawn pawn, DefModExtension_CultivatorPawnKind ext)
        {
            DropUnusableWeapon(pawn);
            var story = pawn.story;
            if (story != null && pawn.DevelopmentalStage.Adult())
            {
                if (ext.forcedBodyType != null) story.bodyType = ext.forcedBodyType;
                else if (!ext.disallowedBodyTypes.NullOrEmpty() && ext.disallowedBodyTypes.Contains(story.bodyType))
                {
                    var allowed = new List<BodyTypeDef> { pawn.gender == Gender.Female ? BodyTypeDefOf.Female : BodyTypeDefOf.Male, BodyTypeDefOf.Thin, BodyTypeDefOf.Fat }
                        .Where(b => !ext.disallowedBodyTypes.Contains(b)).ToList();
                    if (allowed.Count > 0) story.bodyType = allowed.First();
                }
            }
            if (ext.traitChances.NullOrEmpty() || story?.traits == null) return;
            foreach (var tc in ext.traitChances)
            {
                if (tc.trait == null || !Rand.Chance(tc.chance)) continue;
                ConstitutionUtility.SetTrait(pawn, tc.trait, tc.degree);
                break;
            }
        }

        private static List<QiElementDef> PickElements(DefModExtension_CultivatorPawnKind ext)
        {
            if (ext.elements.NullOrEmpty()) return CultivationGenerator.RandomElements();
            var list = new List<QiElementDef> { ext.elements.RandomElement() };
            if (ext.elements.Count > 1 && Rand.Chance(ext.extraElementChance))
            {
                var other = ext.elements.Where(e => e != list[0]).RandomElement();
                list.Add(other);
            }
            return list;
        }
    }

    /// <summary>Facciones marcadas con <see cref="DefModExtension_ConvergenceFaction"/> (sectas): en partidas nuevas vienen con el
    /// mundo y aquí solo se aplican sus relaciones; en partidas viejas se crean al llegar la Convergencia.</summary>
    public static class ConvergenceFactionUtility
    {
        public static void EnsureFactions()
        {
            if (!GameComponent_Convergence.Active || Find.World == null) return;
            var created = new List<Faction>();
            foreach (var def in DefDatabase<FactionDef>.AllDefsListForReading)
            {
                var ext = def.GetModExtension<DefModExtension_ConvergenceFaction>();
                if (ext == null) continue;
                if (Find.FactionManager.AllFactionsListForReading.Any(f => f.def == def)) continue;

                FactionGenerator.CreateFactionAndAddToManager(def);
                var faction = Find.FactionManager.AllFactionsListForReading.LastOrDefault(f => f.def == def);
                if (faction == null) continue;
                created.Add(faction);

                int extra = ext.extraSettlements.RandomInRange;
                for (int i = 0; i < extra; i++)
                {
                    var settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                    settlement.SetFaction(faction);
                    var tile = TileFinder.RandomSettlementTileFor(faction);
                    if (!tile.Valid) continue;   // mundo chico o lleno: sin lugar, sin asentamiento (nunca en la casilla 0)
                    settlement.Tile = tile;
                    settlement.Name = SettlementNameGenerator.GenerateSettlementName(settlement);
                    Find.WorldObjects.Add(settlement);
                }
            }
            foreach (var faction in created) ApplyRelations(faction);
            if (created.Count > 0 && Find.LetterStack != null && Current.ProgramState == ProgramState.Playing)
                Find.LetterStack.ReceiveLetter("CVG_SectsArrive_Label".Translate(),
                    "CVG_SectsArrive_Text".Translate(string.Join("\n", created.Select(f => "  - " + f.Name))), LetterDefOf.NeutralEvent);
        }

        /// <summary>Partida nueva: alianzas y enemistades de las sectas que vinieron con el mundo.</summary>
        public static void ApplyRelationsToExisting()
        {
            if (Find.FactionManager == null) return;
            foreach (var f in Find.FactionManager.AllFactionsListForReading.ToList())
                if (f.def.HasModExtension<DefModExtension_ConvergenceFaction>()) ApplyRelations(f);
        }

        private static void ApplyRelations(Faction faction)
        {
            var ext = faction.def.GetModExtension<DefModExtension_ConvergenceFaction>();
            if (ext == null) return;
            foreach (var other in Find.FactionManager.AllFactionsListForReading)
            {
                if (other == faction || other.IsPlayer) continue;
                if (!ext.hostileTo.NullOrEmpty() && ext.hostileTo.Contains(other.def))
                    SetBoth(faction, other, FactionRelationKind.Hostile);
                else if (!ext.alliedTo.NullOrEmpty() && ext.alliedTo.Contains(other.def))
                    SetBoth(faction, other, FactionRelationKind.Ally);
            }
            var memes = Faction.OfPlayer?.ideos?.PrimaryIdeo?.memes;
            if (!ext.neutralToPlayerMemes.NullOrEmpty() && memes != null && memes.Any(m => ext.neutralToPlayerMemes.Contains(m)))
            {
                faction.SetRelation(new FactionRelation { other = Faction.OfPlayer, kind = FactionRelationKind.Neutral, baseGoodwill = 0 });
                Faction.OfPlayer.SetRelation(new FactionRelation { other = faction, kind = FactionRelationKind.Neutral, baseGoodwill = 0 });
            }
        }

        private static void SetBoth(Faction a, Faction b, FactionRelationKind kind)
        {
            a.SetRelation(new FactionRelation { other = b, kind = kind, baseGoodwill = kind == FactionRelationKind.Ally ? 80 : -80 });
            b.SetRelation(new FactionRelation { other = a, kind = kind, baseGoodwill = kind == FactionRelationKind.Ally ? 80 : -80 });
        }
    }
}
