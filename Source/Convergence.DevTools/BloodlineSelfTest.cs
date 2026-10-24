using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Autotest de linajes: para cada linaje genera un colono, le da la pureza más baja y lo purifica hasta la máxima,
    /// comprobando gen de identidad, genes de aspecto y xenotipo en cada paso; luego prueba un colono generado con el
    /// xenotipo del linaje. Se lanza con la acción de debug o con el argumento de línea de comandos <c>-cvgselftest</c>.
    /// </summary>
    public static class BloodlineSelfTest
    {
        private static bool commandLineChecked;

        /// <summary>
        /// Con <c>-cvgselftest</c> en la línea de comandos, corre el autotest una vez al empezar a correr una partida. Sin ese
        /// argumento no hace nada. No guarda nada en la partida.
        /// </summary>
        public static void CheckCommandLine()
        {
            if (commandLineChecked || Find.TickManager.TicksGame < 60) return;
            commandLineChecked = true;
            if (!GenCommandLine.CommandLineArgPassed("cvgselftest")) return;
            CoreSelfTest.Run();
            ResourceSelfTest.Run();
            if (BloodlineUtility.AnyBloodlines) Run(allowConvergence: true);
            TraitSelfTest.Run();
            FusionSelfTest.Run();
        }

        /// <param name="allowConvergence">
        /// Solo desde la línea de comandos (partida de prueba): marca la Convergencia para probar las vidas extra.
        /// </param>
        public static void Run(bool allowConvergence = false)
        {
            var sb = new StringBuilder("[Convergence] Bloodline self-test");
            int fails = 0;
            foreach (var bloodline in DefDatabase<BloodlineDef>.AllDefsListForReading)
            {
                for (int i = 0; i < bloodline.purities.Count; i++)
                {
                    var lines = BloodlineText.EffectLines(bloodline, i);
                    var gene = bloodline.purities[i].gene;
                    sb.Append($"\n  {bloodline.defName}[{i}] effects: {lines.Count} ({lines.FirstOrDefault()})");
                    if (lines.Count == 0) { sb.Append("  FAIL no effect lines"); fails++; }
                    else if (gene != null && !gene.DescriptionFull.Contains(lines[0])) { sb.Append("  FAIL gene description without effects"); fails++; }
                }
                var pawn = Generate(XenotypeDefOf.Baseliner);
                BloodlineUtility.SetBloodline(pawn, bloodline, 0);
                fails += Check(sb, pawn, bloodline, 0, "give");
                var comp = CompCultivator.For(pawn);
                comp.Sync();
                int gradeBefore = comp.Root?.grade.order ?? 0;
                int maxGrade = DefDatabase<SpiritRootGradeDef>.AllDefsListForReading.Max(g => g.order);
                for (int i = 1; i < bloodline.purities.Count; i++)
                {
                    BloodlineUtility.TryPurify(pawn, notify: false);
                    fails += Check(sb, pawn, bloodline, i, "purify");
                }
                if (BloodlineUtility.TryPurify(pawn, notify: false)) { sb.Append("\n  FAIL purified past the top"); fails++; }
                var top = bloodline.purities[bloodline.purities.Count - 1];
                if (top.passion != Passion.None && top.gene?.aptitudes != null)
                    foreach (var aptitude in top.gene.aptitudes)
                    {
                        var record = pawn.skills.GetSkill(aptitude.skill);
                        sb.Append($"\n  {aptitude.skill.defName}: aptitude {record.Aptitude}, passion {record.passion}");
                        if (aptitude.level > 0 && record.passion < top.passion) { sb.Append("  FAIL passion"); fails++; }
                    }

                // Ancestral: la raíz sube un grado (o aparece una inferior), y no se repite.
                int gradeAfter = comp.Root?.grade.order ?? 0;
                int expected = gradeBefore == 0 ? 1 : System.Math.Min(gradeBefore + 1, maxGrade);
                sb.Append($"\n  root {gradeBefore} -> {gradeAfter} ({CultivationText.RootLabel(comp.Root)}), affinity x{BloodlineUtility.AffinityCultivationFactor(pawn):0.##}");
                if (gradeAfter != expected) { sb.Append("  FAIL root refine"); fails++; }
                if (gradeBefore == 0 && bloodline.affinities.Count > 0 && !BloodlineUtility.HasAffinity(pawn, BloodlineUtility.Get(pawn)))
                { sb.Append("  FAIL new root without the bloodline element"); fails++; }
                BloodlineUtility.SetBloodline(pawn, bloodline, bloodline.purities.Count - 2);
                BloodlineUtility.TryPurify(pawn, notify: false);
                if ((comp.Root?.grade.order ?? 0) != gradeAfter) { sb.Append("\n  FAIL root refined twice"); fails++; }
                pawn.Destroy();

                // Igual que la acción de debug: colono spawneado en el mapa en la 2ª pureza, purificado hasta arriba.
                var map = Find.CurrentMap;
                if (map != null && bloodline.purities.Count > 1)
                {
                    var sp = Generate(XenotypeDefOf.Baseliner);
                    BloodlineUtility.SetBloodline(sp, bloodline, 1);
                    GenSpawn.Spawn(sp, map.Center, map);
                    fails += Check(sb, sp, bloodline, 1, "spawned");
                    for (int i = 2; i < bloodline.purities.Count; i++)
                    {
                        BloodlineUtility.TryPurify(sp, notify: false);
                        fails += Check(sb, sp, bloodline, i, "spawned+purify");
                    }
                    sp.Destroy();
                }

                var xeno = bloodline.purities.Select(p => p.xenotype).FirstOrDefault(x => x != null);
                if (xeno != null && ModsConfig.BiotechActive)
                {
                    var xp = Generate(xeno);
                    var info = BloodlineUtility.Get(xp);
                    fails += Check(sb, xp, bloodline, info?.index ?? -1, "xenotype");
                    BloodlineUtility.TryPurify(xp, notify: false);
                    fails += Check(sb, xp, bloodline, (info?.index ?? -1) + 1, "xenotype+purify");
                    xp.Destroy();
                }
            }
            fails += TestExtraLives(sb, allowConvergence);
            fails += TestForge(sb);
            sb.Append(fails == 0 ? "\n  ALL OK" : "\n  FAILURES: " + fails);
            if (fails == 0) Log.Message(sb.ToString());
            else Log.Error(sb.ToString());
        }

        private static Pawn Generate(XenotypeDef xenotype) =>
            PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayer, forceGenerateNewPawn: true,
                developmentalStages: DevelopmentalStage.Adult, forcedXenotype: ModsConfig.BiotechActive ? xenotype : null));

        /// <summary>Vidas extra: un colono con el gen (máximo nivel) muere y debe seguir vivo, con una vida menos.</summary>
        private static int TestExtraLives(StringBuilder sb, bool allowConvergence)
        {
            var map = Find.CurrentMap;
            if (map == null || !ModsConfig.BiotechActive) return 0;
            if (!GameComponent_Convergence.Active)
            {
                if (!allowConvergence) { sb.Append("\n  extra lives: skipped (no Convergence yet)"); return 0; }
                GameComponent_Convergence.Instance.MarkOccurred();
            }
            int fails = 0;
            foreach (var bloodline in DefDatabase<BloodlineDef>.AllDefsListForReading)
            {
                int top = bloodline.purities.Count - 1;
                if (!BloodlineUtility.TierGenesFor(bloodline, top).Any(g => g.HasModExtension<DefModExtension_ExtraLives>())) continue;
                var pawn = Generate(XenotypeDefOf.Baseliner);
                BloodlineUtility.SetBloodline(pawn, bloodline, top);
                GenSpawn.Spawn(pawn, map.Center, map);
                var gene = BloodlineMechanics.GeneWith<DefModExtension_ExtraLives>(pawn);
                int before = gene?.LivesLeft ?? 0;
                pawn.Kill(null);
                int after = gene?.LivesLeft ?? 0;
                sb.Append($"\n  {bloodline.defName} extra lives: {before} -> {after}, dead {pawn.Dead}");
                if (pawn.Dead || after != before - 1) { sb.Append("  FAIL extra life"); fails++; }
                if (!pawn.Destroyed) pawn.Destroy();
                else pawn.Corpse?.Destroy();
            }
            return fails;
        }

        /// <summary>Forja: el StatPart está en la velocidad de las recetas de forja y la sangre sube los stats de forja.</summary>
        private static int TestForge(StringBuilder sb)
        {
            if (ForgeUtility.ForgeSpeed == null) return 0;
            int fails = 0;
            bool hooked = DefDatabase<StatDef>.GetNamedSilentFail("GeneralLaborSpeed")?.parts?.Any(x => x is StatPart_Forge) ?? false;
            sb.Append($"\n  forge hook on GeneralLaborSpeed: {hooked}");
            if (!hooked) { sb.Append("  FAIL forge hook"); fails++; }
            if (!GameComponent_Convergence.Active || Find.CurrentMap == null) return fails;
            foreach (var bloodline in DefDatabase<BloodlineDef>.AllDefsListForReading)
            {
                var pawn = Generate(XenotypeDefOf.Baseliner);
                BloodlineUtility.SetBloodline(pawn, bloodline, bloodline.purities.Count - 1);
                GenSpawn.Spawn(pawn, Find.CurrentMap.Center, Find.CurrentMap);
                float speed = pawn.GetStatValue(ForgeUtility.ForgeSpeed), quality = pawn.GetStatValue(ForgeUtility.ForgeQuality);
                if (speed != 1f || quality != 0f) sb.Append($"\n  {bloodline.defName} ancestral forge: speed x{speed:0.##}, quality +{quality:0.##}");
                pawn.Destroy();
            }
            return fails;
        }

        private static int Check(StringBuilder sb, Pawn pawn, BloodlineDef bloodline, int index, string step)
        {
            var info = BloodlineUtility.Get(pawn);
            var purity = bloodline.PurityAt(index);
            var identity = pawn.genes?.GenesListForReading.Where(g => g.def.HasModExtension<DefModExtension_Bloodline>()).Select(g => g.def.defName).ToList();
            sb.Append($"\n  {bloodline.defName} {step} -> {purity?.purity.defName}: identity [{identity.ToCommaList()}], xenotype {pawn.genes?.XenotypeLabel}");
            int fails = 0;
            if (purity == null || info == null || info.bloodline != bloodline || info.index != index) { sb.Append("  FAIL bloodline/purity"); fails++; }
            if (pawn.genes != null && purity?.gene != null && (identity.Count != 1 || identity[0] != purity.gene.defName)) { sb.Append("  FAIL identity genes"); fails++; }
            if (pawn.genes != null && purity != null)
            {
                foreach (var g in BloodlineUtility.AspectGenesFor(bloodline, index))
                    if (!pawn.genes.GenesListForReading.Any(x => x.def == g)) { sb.Append("  FAIL missing aspect " + g.defName); fails++; }
                if (purity.xenotype != null && pawn.genes.Xenotype != purity.xenotype) { sb.Append("  FAIL xenotype"); fails++; }
                if (purity.xenotype == null && pawn.genes.xenotypeName != BloodlineUtility.XenotypeName(bloodline, index)) { sb.Append("  FAIL xenotype name"); fails++; }
                var tiers = BloodlineUtility.TierGenesFor(bloodline, index);
                var have = pawn.genes.GenesListForReading.Where(g => g.def.HasModExtension<DefModExtension_BloodlineGeneTier>()).Select(g => g.def).ToList();
                sb.Append($" tiers [{have.Select(g => g.defName.Replace("CVG_BloodGene_", "")).ToCommaList()}]");
                if (have.Count != tiers.Count || tiers.Any(t => !have.Contains(t))) { sb.Append("  FAIL tier genes"); fails++; }
            }
            return fails;
        }
    }
}
