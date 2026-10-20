using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>Linaje y pureza de un colono (derivado de sus genes o rasgos; no se guarda aparte).</summary>
    public class BloodlineInfo
    {
        public BloodlineDef bloodline;
        public int index;
        /// <summary>El gen de identidad es un xenogen (los genes nuevos al purificar se agregan igual).</summary>
        public bool xenogene;

        public BloodlinePurity Purity => bloodline.PurityAt(index);
        public bool CanPurify => index + 1 < bloodline.purities.Count;
    }

    /// <summary>
    /// Lógica genérica de linajes: detectar, dar, purificar, generar al azar y aplicar sus efectos (hediff y capacidades)
    /// solo después de la Convergencia. Sirve para cualquier <see cref="BloodlineDef"/> definido en XML.
    /// </summary>
    public static class BloodlineUtility
    {
        private static HashSet<HediffDef> allHediffs;
        private static HashSet<AbilityDef> allAbilities;
        private static HashSet<GeneDef> allAspectGenes;

        public static bool AnyBloodlines => DefDatabase<BloodlineDef>.DefCount > 0;

        private static HashSet<HediffDef> AllHediffs =>
            allHediffs ??= new HashSet<HediffDef>(DefDatabase<BloodlineDef>.AllDefsListForReading
                .SelectMany(b => b.purities).Select(p => p.hediff).Where(h => h != null));

        private static HashSet<AbilityDef> AllAbilities =>
            allAbilities ??= new HashSet<AbilityDef>(DefDatabase<BloodlineDef>.AllDefsListForReading
                .SelectMany(b => b.purities).SelectMany(p => p.abilities)
                .Concat(TierGenes.SelectMany(g => g.GetModExtension<DefModExtension_BloodlineGeneTier>().awakenedAbilities ?? new List<AbilityDef>())));

        private static List<GeneDef> tierGenes;

        /// <summary>Todos los genes de sangre por niveles (comunes y firma).</summary>
        public static List<GeneDef> TierGenes =>
            tierGenes ??= DefDatabase<GeneDef>.AllDefsListForReading.Where(g => g.GetModExtension<DefModExtension_BloodlineGeneTier>()?.group != null).ToList();

        /// <summary>
        /// Genes por niveles que corresponden a la pureza <paramref name="index"/>: de cada grupo común o firma del linaje, el de
        /// mayor nivel que no supere la pureza (nivel = índice + 1).
        /// </summary>
        public static List<GeneDef> TierGenesFor(BloodlineDef bloodline, int index)
        {
            var result = new List<GeneDef>();
            foreach (var group in TierGenes.Select(g => g.GetModExtension<DefModExtension_BloodlineGeneTier>().group).Distinct())
            {
                if (group.bloodline != null && group.bloodline != bloodline) continue;
                var best = TierGenes.Where(g =>
                    {
                        var e = g.GetModExtension<DefModExtension_BloodlineGeneTier>();
                        return e.group == group && e.tier <= index + 1;
                    })
                    .OrderByDescending(g => g.GetModExtension<DefModExtension_BloodlineGeneTier>().tier).FirstOrDefault();
                if (best != null) result.Add(best);
            }
            return result;
        }

        /// <summary>Deja exactamente los genes por niveles de la pureza (sube el nivel al purificar; quita los de otros linajes).</summary>
        public static void SyncTierGenes(Pawn pawn, BloodlineDef bloodline, int index, bool xenogene)
        {
            if (!ModsConfig.BiotechActive || pawn?.genes == null) return;
            var target = TierGenesFor(bloodline, index);
            var genes = pawn.genes.GenesListForReading;
            if (genes.Count(g => g.def.HasModExtension<DefModExtension_BloodlineGeneTier>()) == target.Count && target.All(t => genes.Any(g => g.def == t)))
                return;
            foreach (var gene in genes.ToList())
                if (gene.def.HasModExtension<DefModExtension_BloodlineGeneTier>() && !target.Contains(gene.def)) pawn.genes.RemoveGene(gene);
            foreach (var def in target)
                if (!pawn.genes.GenesListForReading.Any(g => g.def == def)) pawn.genes.AddGene(def, xenogene);
        }

        private static HashSet<GeneDef> AllAspectGenes =>
            allAspectGenes ??= new HashSet<GeneDef>(DefDatabase<BloodlineDef>.AllDefsListForReading
                .SelectMany(b => b.purities).SelectMany(p => p.aspectGenes));

        /// <summary>Linaje activo del colono, o null. Con dos (no debería pasar), gana la mayor pureza.</summary>
        public static BloodlineInfo Get(Pawn pawn)
        {
            if (pawn == null || !AnyBloodlines) return null;
            BloodlineInfo best = null;

            if (pawn.genes != null)
            {
                var genes = pawn.genes.GenesListForReading;
                for (int i = 0; i < genes.Count; i++)
                {
                    var gene = genes[i];
                    if (!gene.Active) continue;
                    var ext = gene.def.GetModExtension<DefModExtension_Bloodline>();
                    if (ext?.bloodline == null) continue;
                    int index = ext.bloodline.IndexOf(ext.purity);
                    if (index < 0 || (best != null && best.index >= index)) continue;
                    best = new BloodlineInfo { bloodline = ext.bloodline, index = index, xenogene = pawn.genes.IsXenogene(gene) };
                }
            }

            if (best == null && pawn.story?.traits != null)
            {
                foreach (var bloodline in DefDatabase<BloodlineDef>.AllDefsListForReading)
                {
                    for (int i = bloodline.purities.Count - 1; i >= 0; i--)
                    {
                        var p = bloodline.purities[i];
                        if (p.trait != null && pawn.story.traits.HasTrait(p.trait, p.traitDegree))
                        {
                            if (best == null || i > best.index) best = new BloodlineInfo { bloodline = bloodline, index = i };
                            break;
                        }
                    }
                }
            }
            return best;
        }

        /// <summary>
        /// Pone el linaje <paramref name="bloodline"/> con la pureza <paramref name="index"/>: cambia el gen de identidad (o
        /// el rasgo), ajusta los genes de aspecto y sincroniza los efectos. Reemplaza cualquier otro linaje.
        /// </summary>
        public static bool SetBloodline(Pawn pawn, BloodlineDef bloodline, int index)
        {
            var purity = bloodline?.PurityAt(index);
            if (pawn == null || purity == null) return false;
            var current = Get(pawn);
            bool xenogene = current?.xenogene ?? false;

            if (ModsConfig.BiotechActive && pawn.genes != null && purity.gene != null)
            {
                // Un solo linaje: fuera los genes de identidad de cualquier linaje.
                foreach (var gene in pawn.genes.GenesListForReading.ToList())
                    if (gene.def.HasModExtension<DefModExtension_Bloodline>()) pawn.genes.RemoveGene(gene);
                RemoveBloodlineTraits(pawn);

                pawn.genes.AddGene(purity.gene, xenogene);
                SyncAspectGenes(pawn, bloodline, index, xenogene);
                SyncTierGenes(pawn, bloodline, index, xenogene);
                SyncXenotype(pawn, bloodline, index, xenogene);
            }
            else if (pawn.story?.traits != null && purity.trait != null)
            {
                RemoveBloodlineTraits(pawn);
                pawn.story.traits.GainTrait(new Trait(purity.trait, purity.traitDegree, forced: true));
            }
            else
            {
                return false;
            }

            SyncEffects(pawn);
            return true;
        }

        /// <summary>Sube a la pasión de la pureza las habilidades con aptitud positiva de su gen de identidad (nunca la baja).</summary>
        public static void EnsurePassions(Pawn pawn, BloodlinePurity purity)
        {
            if (pawn?.skills == null || purity == null || purity.passion == Passion.None || purity.gene?.aptitudes == null) return;
            foreach (var aptitude in purity.gene.aptitudes)
            {
                if (aptitude.level <= 0) continue; // la habilidad penalizada del linaje no recibe pasión
                var record = pawn.skills.GetSkill(aptitude.skill);
                if (record != null && record.passion < purity.passion) record.passion = purity.passion;
            }
        }

        /// <summary>
        /// Sube un nivel de pureza (purificación). La API la usarán el ritual y las píldoras. <paramref name="notify"/> = false
        /// para pruebas o generación: sin mensajes.
        /// </summary>
        public static bool TryPurify(Pawn pawn, bool notify = true)
        {
            var info = Get(pawn);
            if (info == null || !info.CanPurify) return false;
            if (!SetBloodline(pawn, info.bloodline, info.index + 1)) return false;
            notify = notify && pawn.Faction == Faction.OfPlayer && PawnUtility.ShouldSendNotificationAbout(pawn);
            if (notify)
                SoundCues.At("CVG_Cue_BloodlinePurified", pawn);
            if (notify)
                Messages.Message("CVG_Bloodline_Purified".Translate(pawn.Named("PAWN"), info.bloodline.LabelCap,
                    info.bloodline.purities[info.index + 1].purity.LabelCap), pawn, MessageTypeDefOf.PositiveEvent);
            TryAwakenRootElement(pawn, info.bloodline.purities[info.index + 1], notify);
            // La sangre ancestral limpia la raíz: sube un grado (o despierta una raíz inferior), una sola vez.
            if (info.index + 2 == info.bloodline.purities.Count && TryRefineRoot(pawn, info.bloodline) && notify)
                Messages.Message("CVG_Bloodline_RootRefined".Translate(pawn.Named("PAWN"), CultivationText.RootLabel(CompCultivator.For(pawn).Root)),
                    pawn, MessageTypeDefOf.PositiveEvent);
            return true;
        }

        /// <summary>Pureza que despierta un elemento de raíz (Qilin del Trueno): una tirada al alcanzarla.</summary>
        public static bool TryAwakenRootElement(Pawn pawn, BloodlinePurity purity, bool notify)
        {
            if (purity?.rootElement == null || !Rand.Chance(purity.rootElementChance)) return false;
            if (!CultivationGenerator.AddRootElement(pawn, purity.rootElement)) return false;
            if (notify && pawn.Faction == Faction.OfPlayer)
                Messages.Message("CVG_Bloodline_RootElementAwakened".Translate(pawn.Named("PAWN"), purity.rootElement.label), pawn, MessageTypeDefOf.PositiveEvent);
            return true;
        }

        /// <summary>Afinidad de sangre: multiplicador de cultivo de la pureza actual si la raíz tiene algún elemento del linaje; si no, 1.</summary>
        public static float AffinityCultivationFactor(Pawn pawn)
        {
            var info = Get(pawn);
            if (info == null || !HasAffinity(pawn, info)) return 1f;
            return info.Purity.affinityCultivationFactor;
        }

        /// <summary>La raíz espiritual del colono tiene alguno de los elementos del linaje.</summary>
        public static bool HasAffinity(Pawn pawn, BloodlineInfo info)
        {
            var root = CompCultivator.For(pawn)?.Root;
            return root != null && info != null && root.elements.Any(e => info.bloodline.affinities.Contains(e));
        }

        /// <summary>
        /// Sube un grado la raíz espiritual (gen de calidad con Biotech, grado del rasgo sin él); sin raíz, da una raíz inferior
        /// del elemento del linaje. Solo una vez por colono (flag en <see cref="CompCultivator"/>). Devuelve si cambió algo.
        /// </summary>
        public static bool TryRefineRoot(Pawn pawn, BloodlineDef bloodline)
        {
            var comp = CompCultivator.For(pawn);
            if (comp == null || comp.BloodRootRefined) return false;
            comp.BloodRootRefined = true;
            comp.Sync();
            var grades = DefDatabase<SpiritRootGradeDef>.AllDefsListForReading.OrderBy(g => g.order).ToList();
            var root = comp.Root;

            if (root == null)
            {
                var element = bloodline.affinities.FirstOrDefault() ?? CultivationGenerator.RandomElements().First();
                CultivationGenerator.GiveRoot(pawn, grades.First(), new List<QiElementDef> { element });
                comp.SetTraitRootElements(new List<QiElementDef> { element });
            }
            else
            {
                var next = grades.FirstOrDefault(g => g.order > root.grade.order);
                if (next == null) return false;
                var gradeGene = pawn.genes?.GenesListForReading.FirstOrDefault(g => g.def.GetModExtension<DefModExtension_SpiritRoot>()?.grade != null);
                var nextGene = DefDatabase<GeneDef>.AllDefsListForReading.FirstOrDefault(g =>
                {
                    var ext = g.GetModExtension<DefModExtension_SpiritRoot>();
                    return ext != null && ext.grade == next && ext.element == null;
                });
                if (gradeGene != null && nextGene != null)
                {
                    bool xeno = pawn.genes.IsXenogene(gradeGene);
                    pawn.genes.RemoveGene(gradeGene);
                    pawn.genes.AddGene(nextGene, xeno);
                }
                else
                {
                    var trait = pawn.story?.traits?.GetTrait(CVG_DefOf.CVG_SpiritRoot);
                    if (trait == null) return false;
                    pawn.story.traits.RemoveTrait(trait);
                    pawn.story.traits.GainTrait(new Trait(CVG_DefOf.CVG_SpiritRoot, next.traitDegree, forced: true));
                }
            }
            comp.Sync();
            comp.SyncRealm();
            return true;
        }

        private static void RemoveBloodlineTraits(Pawn pawn)
        {
            if (pawn.story?.traits == null) return;
            foreach (var bloodline in DefDatabase<BloodlineDef>.AllDefsListForReading)
                foreach (var p in bloodline.purities)
                {
                    if (p.trait == null) continue;
                    var trait = pawn.story.traits.GetTrait(p.trait, p.traitDegree);
                    if (trait != null) pawn.story.traits.RemoveTrait(trait);
                }
        }

        /// <summary>
        /// Genes de aspecto acumulados hasta la pureza <paramref name="index"/>; los de purezas mayores reemplazan a los que chocan
        /// (exclusionTag, o el mismo color de pelo/piel).
        /// </summary>
        public static List<GeneDef> AspectGenesFor(BloodlineDef bloodline, int index)
        {
            var result = new List<GeneDef>();
            for (int i = 0; i <= index && i < bloodline.purities.Count; i++)
            {
                foreach (var gene in bloodline.purities[i].aspectGenes)
                {
                    result.RemoveAll(g => CosmeticConflict(g, gene));
                    result.Add(gene);
                }
            }
            return result;
        }

        /// <summary>Deja exactamente los genes de aspecto de la pureza: quita los de otros linajes/purezas y los vanilla que chocan.</summary>
        private static void SyncAspectGenes(Pawn pawn, BloodlineDef bloodline, int index, bool xenogene)
        {
            var target = AspectGenesFor(bloodline, index);
            foreach (var gene in pawn.genes.GenesListForReading.ToList())
                if (AllAspectGenes.Contains(gene.def) && !target.Contains(gene.def)) pawn.genes.RemoveGene(gene);

            foreach (var def in target)
            {
                if (pawn.genes.GenesListForReading.Any(g => g.def == def)) continue;
                // Los genes cosméticos vanilla que chocan (cuernos, orejas, color de piel o pelo) se eligen al azar entre
                // los presentes: se quitan para que se vea el del linaje.
                foreach (var other in pawn.genes.GenesListForReading.ToList())
                {
                    if (other.def == def || AllAspectGenes.Contains(other.def) || other.def.HasModExtension<DefModExtension_Bloodline>()) continue;
                    if (pawn.genes.IsXenogene(other) && !xenogene) continue;
                    if (CosmeticConflict(other.def, def)) pawn.genes.RemoveGene(other);
                }
                pawn.genes.AddGene(def, xenogene);
            }
        }

        /// <summary>
        /// Muestra la raza en la ficha: con <see cref="BloodlinePurity.xenotype"/> el colono pasa a ese xenotipo (se agregan los
        /// genes que le falten; no se borra ninguno, así se conservan raíz espiritual, cuerpo especial y demás); sin él, un
        /// baseliner (o alguien con el xenotipo o el nombre de otro linaje) recibe un nombre de xenotipo propio.
        /// </summary>
        private static void SyncXenotype(Pawn pawn, BloodlineDef bloodline, int index, bool xenogene)
        {
            var genes = pawn.genes;
            var purity = bloodline.purities[index];
            if (purity.xenotype != null)
            {
                var aspects = AspectGenesFor(bloodline, index);
                foreach (var def in purity.xenotype.genes)
                {
                    if (def.HasModExtension<DefModExtension_Bloodline>() || def.HasModExtension<DefModExtension_BloodlineGeneTier>() || AllAspectGenes.Contains(def)) continue;
                    if (genes.GenesListForReading.Any(g => g.def == def)) continue;
                    if (aspects.Any(a => CosmeticConflict(a, def))) continue;
                    genes.AddGene(def, xenogene || !purity.xenotype.inheritable);
                }
                genes.SetXenotypeDirect(purity.xenotype);
                genes.iconDef = null;
                return;
            }

            bool ours = genes.Xenotype == XenotypeDefOf.Baseliner || IsBloodlineXenotype(genes.Xenotype);
            if (!ours || (genes.UniqueXenotype && !IsBloodlineXenotypeName(genes.xenotypeName))) return;
            genes.SetXenotypeDirect(XenotypeDefOf.Baseliner);
            genes.xenotypeName = XenotypeName(bloodline, index);
            genes.iconDef = bloodline.xenotypeIcon;
        }

        public static string XenotypeName(BloodlineDef bloodline, int index) =>
            "CVG_Bloodline_XenotypeName".Translate(bloodline.label, bloodline.purities[index].purity.label).Resolve().CapitalizeFirst();

        private static bool IsBloodlineXenotype(XenotypeDef xenotype) =>
            xenotype != null && DefDatabase<BloodlineDef>.AllDefsListForReading.Any(b => b.purities.Any(p => p.xenotype == xenotype));

        private static bool IsBloodlineXenotypeName(string name) =>
            DefDatabase<BloodlineDef>.AllDefsListForReading.Any(b => Enumerable.Range(0, b.purities.Count).Any(i => XenotypeName(b, i) == name));

        private static bool CosmeticConflict(GeneDef a, GeneDef b) =>
            a.ConflictsWith(b)
            || (a.hairColorOverride.HasValue && b.hairColorOverride.HasValue)
            || (a.skinColorOverride.HasValue && b.skinColorOverride.HasValue);

        /// <summary>
        /// Aplica o quita el hediff de efectos y las capacidades según el linaje y si ya ocurrió la Convergencia.
        /// Barato: se llama desde el tick de sincronización de <see cref="CompCultivator"/>.
        /// </summary>
        public static void SyncEffects(Pawn pawn)
        {
            if (pawn?.health == null || pawn.Dead || !AnyBloodlines) return;
            var info = Get(pawn);
            // Pasión y genes por niveles: talento natural, también antes de la Convergencia (p. ej. colonos con el xenotipo).
            if (info != null)
            {
                EnsurePassions(pawn, info.Purity);
                SyncTierGenes(pawn, info.bloodline, info.index, info.xenogene);
            }
            bool awake = info != null && GameComponent_Convergence.Active;
            var purity = awake ? info.Purity : null;

            // Hediff de efectos
            var hediffs = pawn.health.hediffSet.hediffs;
            for (int i = hediffs.Count - 1; i >= 0; i--)
            {
                var h = hediffs[i];
                if (AllHediffs.Contains(h.def) && h.def != purity?.hediff) pawn.health.RemoveHediff(h);
            }
            if (purity?.hediff != null)
            {
                float severity = purity.hediffSeverity >= 0f ? purity.hediffSeverity : info.index + 1;
                var h = pawn.health.hediffSet.GetFirstHediffOfDef(purity.hediff);
                if (h == null)
                {
                    h = HediffMaker.MakeHediff(purity.hediff, pawn);
                    h.Severity = severity;
                    pawn.health.AddHediff(h);
                }
                else if (!UnityEngine.Mathf.Approximately(h.Severity, severity))
                {
                    h.Severity = severity;
                }
            }

            // Capacidades: las de esta pureza y las menores.
            if (pawn.abilities == null) return;
            var wanted = new HashSet<AbilityDef>();
            if (awake)
            {
                for (int i = 0; i <= info.index; i++) wanted.AddRange(info.bloodline.purities[i].abilities);
                if (pawn.genes != null)
                    foreach (var gene in pawn.genes.GenesListForReading)
                    {
                        var tierExt = gene.Active ? gene.def.GetModExtension<DefModExtension_BloodlineGeneTier>() : null;
                        if (tierExt?.awakenedAbilities != null) wanted.AddRange(tierExt.awakenedAbilities);
                    }
            }
            foreach (var def in AllAbilities)
            {
                bool has = pawn.abilities.GetAbility(def) != null;
                if (wanted.Contains(def) && !has) pawn.abilities.GainAbility(def);
                else if (!wanted.Contains(def) && has) pawn.abilities.RemoveAbility(def);
            }
        }

        /// <summary>Tirada al generar humanos: pureza de mayor a menor, luego linaje por peso.</summary>
        public static void TryGiveRandomBloodline(Pawn pawn)
        {
            if (!AnyBloodlines || pawn?.RaceProps == null || !pawn.RaceProps.Humanlike) return;
            var existing = Get(pawn);
            if (existing != null)
            {
                SyncTierGenes(pawn, existing.bloodline, existing.index, existing.xenogene);
                EnsurePassions(pawn, existing.Purity);
                return;
            }
            float factor = ConvergenceMod.Settings.bloodlineChanceFactor;
            if (factor <= 0f) return;

            foreach (var purity in DefDatabase<BloodlinePurityDef>.AllDefsListForReading.OrderByDescending(p => p.order))
            {
                if (!Rand.Chance(purity.chance * factor)) continue;
                var candidates = DefDatabase<BloodlineDef>.AllDefsListForReading
                    .Select(b => (bloodline: b, index: b.IndexOf(purity)))
                    .Where(x => x.index >= 0 && CanGive(pawn, x.bloodline.purities[x.index]))
                    .ToList();
                if (candidates.TryRandomElementByWeight(x => x.bloodline.commonality * x.bloodline.purities[x.index].chanceFactor, out var pick)
                    && SetBloodline(pawn, pick.bloodline, pick.index))
                    TryAwakenRootElement(pawn, pick.bloodline.purities[pick.index], notify: false);
                return;
            }
        }

        private static bool CanGive(Pawn pawn, BloodlinePurity p) =>
            (ModsConfig.BiotechActive && pawn.genes != null && p.gene != null) || (pawn.story?.traits != null && p.trait != null);

        public static string Label(BloodlineInfo info) =>
            info == null
                ? "CVG_Bloodline_None".Translate().Resolve()
                : "CVG_Bloodline_Label".Translate(info.bloodline.LabelCap, info.Purity.purity.LabelCap).Resolve();
    }
}
