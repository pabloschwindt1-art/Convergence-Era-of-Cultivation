using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence.Sects2
{
    /// <summary>
    /// Autotest de la prueba de concepto S0 (sectas sobre Royalty). Solo corre con <c>-cvgsects2test</c> (y un mapa, p. ej. -quicktest):
    /// títulos honorarios instantáneos al ganar contribución, sin misión de investidura del Imperio, puntos de permiso, los dos favores
    /// (recursos y discípulos), el enfriamiento, el pago con contribución durante el enfriamiento y que gastar no baje el título.
    /// Inerte sin el argumento.
    /// </summary>
    public class GameComponent_Sects2Test : GameComponent
    {
        private static readonly bool Enabled = GenCommandLine.CommandLineArgPassed("cvgsects2test");
        private bool started;
        private int phase = -1;
        private int waitUntil;
        private readonly StringBuilder sb = new StringBuilder("[Convergence] Sects2 S0 self-test");
        private int fails;
        private Map map;
        private Pawn pawn;
        private Faction faction;
        private int skyfallersBefore;

        private static RoyalTitleDef Title(string n) => DefDatabase<RoyalTitleDef>.GetNamed("CVG_Title_FormlessSword_" + n);
        private static RoyalTitlePermitDef Permit(string n) => DefDatabase<RoyalTitlePermitDef>.GetNamed("CVG_Permit_FormlessSword_" + n);

        public GameComponent_Sects2Test(Game game) { }

        private void Check(bool ok, string what)
        {
            sb.Append("\n    " + (ok ? "ok   " : "FAIL ") + what);
            if (!ok) fails++;
        }

        private void Note(string what) => sb.Append("\n- " + what);

        public override void GameComponentTick()
        {
            if (!Enabled) return;
            try
            {
                if (!started)
                {
                    if (Find.CurrentMap == null || Find.TickManager.TicksGame < 200) return;
                    started = true;
                    map = Find.CurrentMap;
                    phase = 0;
                }
                if (phase < 0 || Find.TickManager.TicksGame < waitUntil) return;
                Step();
            }
            catch (Exception e)
            {
                Check(false, "exception: " + e);
                Finish();
            }
        }

        private int Skyfallers() => map.listerThings.AllThings.Count(t => t is Skyfaller);

        private bool BestowingQuest() => Find.QuestManager.QuestsListForReading.Any(q => q.root == QuestScriptDefOf.BestowingCeremony
            && (q.State == QuestState.NotYetAccepted || q.State == QuestState.Ongoing));

        /// <summary>Usa un permiso como lo haría el jugador: la opción del menú y después el objetivo elegido.</summary>
        private bool UsePermit(RoyalTitlePermitDef def, out string reason)
        {
            reason = null;
            var opt = def.Worker.GetRoyalAidOptions(map, pawn, faction).FirstOrDefault();
            if (opt == null) { reason = "no option"; return false; }
            if (opt.Disabled || opt.action == null) { reason = opt.Label; return false; }
            opt.action();
            if (!CellFinder.TryFindRandomCellNear(pawn.Position, map, 10, c => !c.Fogged(map) && c.GetEdifice(map) == null
                && DropCellFinder.CanPhysicallyDropInto(c, map, canRoofPunch: true) && c.Standable(map), out var cell))
            {
                reason = "no drop cell";
                Find.Targeter.StopTargeting();
                return false;
            }
            if (def.Worker is RoyalTitlePermitWorker_Targeted targeted) targeted.OrderForceTarget(new LocalTargetInfo(cell));
            else { reason = "worker is not targeted"; Find.Targeter.StopTargeting(); return false; }
            Find.Targeter.StopTargeting();
            return true;
        }

        private void Step()
        {
            switch (phase)
            {
                case 0:
                {
                    var fdef = DefDatabase<FactionDef>.GetNamed("CVG_Faction_FormlessSword");
                    faction = Find.FactionManager.FirstFactionOfDef(fdef);
                    if (faction == null)
                    {
                        // El quicktest arma el mundo sin las facciones opcionales: la secta llega con la Convergencia, como en una partida vieja.
                        Note("faction not in the quicktest world: firing the Convergence");
                        if (!GameComponent_Convergence.Active) GameComponent_Convergence.Instance?.MarkOccurred();
                        ConvergenceFactionUtility.EnsureFactions();
                        faction = Find.FactionManager.FirstFactionOfDef(fdef);
                    }
                    Check(faction != null, "faction exists");
                    if (faction == null) { Finish(); return; }
                    Check(ModsConfig.RoyaltyActive, "Royalty active");
                    Check(fdef.HasRoyalTitles && fdef.RoyalTitlesAwardableInSeniorityOrderForReading.Count == 4, "4 honorary titles: "
                        + string.Join(", ", fdef.RoyalTitlesAwardableInSeniorityOrderForReading.Select(t => t.defName)));
                    Check(faction.def.royalFavorLabel == "sect contribution" || !faction.def.royalFavorLabel.NullOrEmpty(), "contribution label: " + faction.def.royalFavorLabel);
                    if (faction.HostileTo(Faction.OfPlayer) || faction.PlayerGoodwill < 0)
                        faction.TryAffectGoodwillWith(Faction.OfPlayer, 30 - faction.PlayerGoodwill, canSendMessage: false, canSendHostilityLetter: false);
                    Check(!faction.HostileTo(Faction.OfPlayer), "faction friendly (goodwill " + faction.PlayerGoodwill + ")");
                    pawn = map.mapPawns.FreeColonistsSpawned.FirstOrDefault();
                    Check(pawn?.royalty != null, "colonist with royalty tracker: " + pawn?.LabelShort);
                    if (pawn?.royalty == null) { Finish(); return; }

                    // Contribución → títulos al instante
                    pawn.royalty.GainFavor(faction, 2);
                    Check(pawn.royalty.GetCurrentTitle(faction) == Title("Friend"), "2 contribution -> Friend (" + pawn.royalty.GetCurrentTitle(faction)?.defName + ", left " + pawn.royalty.GetFavor(faction) + ")");
                    pawn.royalty.GainFavor(faction, 6);
                    Check(pawn.royalty.GetCurrentTitle(faction) == Title("Guest"), "+6 -> Honored Guest (" + pawn.royalty.GetCurrentTitle(faction)?.defName + ", left " + pawn.royalty.GetFavor(faction) + ")");
                    Check(!BestowingQuest(), "no Empire bestowing ceremony quest");
                    Check(pawn.royalty.GetPermitPoints(faction) == 2, "permit points " + pawn.royalty.GetPermitPoints(faction));

                    // Elegir los dos favores (como en la ventana de permisos)
                    pawn.royalty.AddPermit(Permit("SpiritResources"), faction);
                    pawn.royalty.AddPermit(Permit("Disciples"), faction);
                    Check(pawn.royalty.HasPermit(Permit("SpiritResources"), faction) && pawn.royalty.HasPermit(Permit("Disciples"), faction), "both favors chosen");

                    // Favor 1: recursos espirituales, gratis
                    skyfallersBefore = Skyfallers();
                    Check(UsePermit(Permit("SpiritResources"), out var r1), "use spirit resources (free)" + (r1 != null ? ": " + r1 : ""));
                    Check(Skyfallers() > skyfallersBefore, "a drop pod is falling");
                    Check(pawn.royalty.GetPermit(Permit("SpiritResources"), faction).OnCooldown, "spirit resources on cooldown");

                    // Favor 2: discípulos, gratis
                    int before = Skyfallers();
                    if (UsePermit(Permit("Disciples"), out var r2)) Check(Skyfallers() > before, "sword disciples dropping in");
                    else if (r2 != null && r2.Contains("BadTemperature".Translate())) Note("call disciples skipped: bad temperature on this map");
                    else Check(false, "use call disciples: " + r2);

                    // En enfriamiento: se paga con contribución y el título no baja
                    pawn.royalty.GainFavor(faction, 5);
                    int favor = pawn.royalty.GetFavor(faction);
                    Check(pawn.royalty.GetCurrentTitle(faction) == Title("Guest"), "5 contribution is not enough for Protector");
                    Check(UsePermit(Permit("SpiritResources"), out var r3), "use spirit resources again (paid)" + (r3 != null ? ": " + r3 : ""));
                    Check(pawn.royalty.GetFavor(faction) == favor - Permit("SpiritResources").royalAid.favorCost, "paid " + Permit("SpiritResources").royalAid.favorCost + " contribution (" + favor + " -> " + pawn.royalty.GetFavor(faction) + ")");
                    Check(pawn.royalty.GetCurrentTitle(faction) == Title("Guest"), "spending contribution keeps the title");

                    // Hasta Anciano Invitado
                    pawn.royalty.GainFavor(faction, 7);
                    Check(pawn.royalty.GetCurrentTitle(faction) == Title("Protector"), "-> Protector (" + pawn.royalty.GetCurrentTitle(faction)?.defName + ")");
                    pawn.royalty.GainFavor(faction, 16);
                    Check(pawn.royalty.GetCurrentTitle(faction) == Title("GuestElder"), "-> Guest Elder (" + pawn.royalty.GetCurrentTitle(faction)?.defName + ")");
                    Check(!BestowingQuest(), "still no bestowing ceremony quest");
                    Check(pawn.royalty.GetPermitPoints(faction) == 3, "3 more permit points (1 + 2): " + pawn.royalty.GetPermitPoints(faction));

                    // Favores 3-5: obra maestra, legendaria a elección y golpe
                    foreach (var n in new[] { "MasterworkSword", "LegendarySword", "SwordStrike" }) pawn.royalty.AddPermit(Permit(n), faction);
                    Check(new[] { "MasterworkSword", "LegendarySword", "SwordStrike" }.All(n => pawn.royalty.HasPermit(Permit(n), faction)), "the five favors chosen");
                    var master = (RoyalTitlePermitWorker_SectGift)Permit("MasterworkSword").Worker;
                    var masterOpt = Permit("MasterworkSword").GetModExtension<DefModExtension_SectGift>().options[0];
                    Check(Gift(master, masterOpt), "masterwork sword sent");
                    var legend = Permit("LegendarySword").GetModExtension<DefModExtension_SectGift>();
                    Check(legend.playerChooses && legend.options.Count >= 3, "legendary sword: the colonist chooses among " + legend.options.Count);
                    Check(Gift((RoyalTitlePermitWorker_SectGift)Permit("LegendarySword").Worker, legend.options[0]), "legendary sword sent (" + legend.options[0].Label + ")");
                    int beforeStrike = Skyfallers();
                    if (UsePermit(Permit("SwordStrike"), out var r4)) Check(Skyfallers() > beforeStrike, "sword strike: core blades dropping in");
                    else if (r4 != null && r4.Contains("BadTemperature".Translate())) Note("sword strike skipped: bad temperature on this map");
                    else Check(false, "use sword strike: " + r4);

                    // Hostilidad: el título no se pierde, los favores se suspenden
                    faction.TryAffectGoodwillWith(Faction.OfPlayer, -200, canSendMessage: false, canSendHostilityLetter: false);
                    Check(faction.HostileTo(Faction.OfPlayer), "faction made hostile");
                    Check(pawn.royalty.GetCurrentTitle(faction) == Title("GuestElder"), "hostility keeps the title");
                    var opt = Permit("SpiritResources").Worker.GetRoyalAidOptions(map, pawn, faction).FirstOrDefault();
                    Check(opt == null || opt.Disabled || opt.action == null, "hostility suspends the favors");
                    faction.TryAffectGoodwillWith(Faction.OfPlayer, 230, canSendMessage: false, canSendHostilityLetter: false);
                    Check(!faction.HostileTo(Faction.OfPlayer), "peace again");

                    TestSwordSect();
                    foreach (var spec in Specs()) TestSect(spec);

                    phase = 1;
                    waitUntil = Find.TickManager.TicksGame + 600;
                    break;
                }
                case 1:
                {
                    // Llegaron los discípulos, el cofre y las espadas
                    var disciples = map.mapPawns.AllPawnsSpawned.Count(p => p.Faction == faction);
                    Note("sect pawns on the map after the drops: " + disciples);
                    Check(map.listerThings.ThingsOfDef(DefDatabase<ThingDef>.GetNamed("CVG_SpiritStone")).Sum(t => t.stackCount) >= 40, "spirit stones delivered");
                    var swords = map.listerThings.AllThings.Where(t => t.def.weaponTags != null && t.def.weaponTags.Contains("CVG_Sword") && !t.ParentHolder.IsEnclosingContainer()
                        && t.ParentHolder is Map).ToList();
                    Check(swords.Any(t => t.TryGetComp<CompQuality>()?.Quality == QualityCategory.Masterwork), "a masterwork sword lies on the ground");
                    Check(swords.Any(t => t.TryGetComp<CompQuality>()?.Quality == QualityCategory.Legendary), "a legendary sword lies on the ground");
                    Finish();
                    break;
                }
            }
        }

        /// <summary>Un favor de regalo como lo usaría el jugador (ya elegida la opción): cápsula cerca del colono.</summary>
        private bool Gift(RoyalTitlePermitWorker_SectGift worker, SectGiftOption option)
        {
            if (!CellFinder.TryFindRandomCellNear(pawn.Position, map, 10, c => !c.Fogged(map) && c.GetEdifice(map) == null
                && DropCellFinder.CanPhysicallyDropInto(c, map, canRoofPunch: true) && c.Standable(map), out var cell)) return false;
            int before = Skyfallers();
            worker.Begin(pawn, faction, map, free: true, option);
            worker.OrderForceTarget(new LocalTargetInfo(cell));
            Find.Targeter.StopTargeting();
            return Skyfallers() > before;
        }

        /// <summary>Contenido de la Espada Sin Forma (S2): Cuerpo de Espada, ideología, ritual, discípulos y comercio.</summary>
        private void TestSwordSect()
        {
            var body = DefDatabase<TraitDef>.GetNamed("CVG_Body_Sword");
            Check(body.degreeDatas.Count == 4 && body.degreeDatas.Select(d => d.degree).SequenceEqual(new[] { 0, 1, 2, 3 }), "Sword Body has 4 grades (0-3)");
            Check(body.GetModExtension<DefModExtension_SpecialBody>()?.rootlessOnly == false, "Sword Body works with and without a spirit root");
            var oldCelestial = DefDatabase<TraitDef>.GetNamedSilentFail("CVG_Body_CelestialSword");
            Check(oldCelestial == null || oldCelestial.GetModExtension<DefModExtension_SpecialBody>()?.chance == 0f, "the old celestial sword body is no longer born");

            // Ideología
            var meme = DefDatabase<MemeDef>.GetNamed("CVG_Meme_SwordWay");
            Check(meme.preventApparelRequirements, "the sword way imposes no clothing");
            Check(meme.requireOne != null && meme.requireOne.Count == 4, "preset precepts: cultivation, duels, elders, no demonic arts");
            var role = DefDatabase<PreceptDef>.GetNamed("CVG_Role_SwordMaster");
            Check(role.activationBelieverCount == 1 && role.GetModExtension<DefModExtension_RoleEmptyMinBelievers>()?.minBelievers == 4, "sword master: active from 1 believer, empty-role mood from 4");
            Check(role.grantedAbilities?.Any(a => a.defName == "CombatCommand") ?? false, "sword master grants combat command");
            var req = role.roleRequirements.OfType<RoleRequirement_HasTrait>().FirstOrDefault();
            var test = map.mapPawns.FreeColonistsSpawned.FirstOrDefault(p => p != pawn) ?? pawn;
            ConstitutionUtility.RemoveTrait(test, body);
            Check(req != null && !req.Met(test, null), "without a Sword Body the colonist cannot be sword master");
            ConstitutionUtility.SetTrait(test, body, 0);
            Check(req.Met(test, null), "with an iron Sword Body they can");

            // Ritual de templado: 4 pasos con las probabilidades aprobadas
            var outcome = DefDatabase<RitualOutcomeEffectDef>.GetNamed("CVG_Outcome_TemperSwordBody");
            var ext = outcome.GetModExtension<DefModExtension_TemperConstitution>();
            Check(ext?.steps?.Count == 4, "tempering ritual: 4 steps");
            var expected = new[] { (0.80f, 1.00f), (0.60f, 1.00f), (0.40f, 0.90f), (0.20f, 0.55f) };
            for (int i = 0; i < 4; i++)
                Check(Mathf.Abs(ext.steps[i].Chance(0f) - expected[i].Item1) < 0.001f && Mathf.Abs(ext.steps[i].Chance(1f) - expected[i].Item2) < 0.001f,
                    "step " + (i + 1) + ": " + ext.steps[i].Chance(0f).ToStringPercent() + " -> " + ext.steps[i].Chance(1f).ToStringPercent());
            Check(outcome.comps.OfType<RitualOutcomeComp_MinBelievers>().Any(c => c.count == 2), "the ritual needs two more believers");
            Check(outcome.comps.OfType<RitualOutcomeComp_MinBelievers>().First().BlockingIssues(null, TargetInfo.Invalid, null).Any(), "without believers it cannot start");
            Check(ext.StepFor(test) == ext.steps[1], "an iron body is tempered toward cold steel");
            RitualOutcomeEffectWorker_TemperConstitution.Resolve(new DefModExtension_TemperConstitution { steps = ext.steps }, test, 1f, map);
            Check(ConstitutionUtility.Degree(test, body) == 1, "perfect ceremony: iron -> cold steel");

            // Discípulos: solo espadas, sin Hulk, el anciano siempre con Cuerpo de Espada
            foreach (var kindName in new[] { "Disciple", "Bearer", "Blade", "Elder" })
            {
                var kind = DefDatabase<PawnKindDef>.GetNamed("CVG_FormlessSword_" + kindName);
                int hulks = 0, nonSwords = 0, bodies = 0, sectStories = 0, adults = 0;
                for (int i = 0; i < 6; i++)
                {
                    var p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, faction, forceGenerateNewPawn: true));
                    if (p.story.Adulthood != null) { adults++; if (kind.fixedAdultBackstories.Contains(p.story.Adulthood)) sectStories++; }
                    if (p.story.bodyType == BodyTypeDefOf.Hulk) hulks++;
                    var w = p.equipment?.Primary;
                    if (w != null && !(w.def.weaponTags?.Contains("CVG_Sword") ?? false)) nonSwords++;
                    if (ConstitutionUtility.Degree(p, body) != null) bodies++;
                    p.Destroy();
                }
                Check(hulks == 0 && nonSwords == 0 && (kindName != "Elder" || bodies == 6), kindName + ": no Hulk, swords only, " + bodies + "/6 with Sword Body");
                Check(kind.fixedAdultBackstories.Count > 0 && sectStories == adults, kindName + ": adult backstory from the sect (" + sectStories + "/" + adults + ")");
            }

            // Comercio: espadas Buena-Obra maestra, nunca Legendaria
            var trader = DefDatabase<TraderKindDef>.GetNamed("CVG_Trader_FormlessSword");
            var stock = new List<Thing>();
            for (int i = 0; i < 8; i++) foreach (var g in trader.stockGenerators) stock.AddRange(g.GenerateThings(map.Tile, faction));
            var sold = stock.Where(t => t.def.weaponTags?.Contains("CVG_Sword") ?? false).ToList();
            var q = sold.Select(t => t.TryGetComp<CompQuality>()?.Quality).Where(x => x != null).Select(x => x.Value).ToList();
            Check(sold.Count > 0 && q.All(x => x >= QualityCategory.Good && x <= QualityCategory.Masterwork), "trade: " + sold.Count + " swords, all Good-Masterwork");
            Check(!q.Contains(QualityCategory.Legendary), "trade: never legendary");
            Check(stock.Any(t => t.def.defName == "CVG_SpiritStone"), "trade: spirit stones in stock");
            foreach (var t in stock) t.Destroy();
        }

        private static bool IsSword(Thing t) => t?.def.weaponTags?.Contains("CVG_Sword") ?? false;

        /// <summary>Las sectas del nuevo Sects y sus reglas propias.</summary>
        private static IEnumerable<SectSpec> Specs()
        {
            yield return new SectSpec
            {
                id = "FormlessSword", bodyTrait = "CVG_Body_Sword", meme = "CVG_Meme_SwordWay", role = "CVG_Role_SwordMaster", ability = "CombatCommand",
                outcome = "CVG_Outcome_TemperSwordBody",
                kinds = new[] { "CVG_FormlessSword_Disciple", "CVG_FormlessSword_Bearer", "CVG_FormlessSword_Blade", "CVG_FormlessSword_Elder" },
                weaponOk = p => p.equipment?.Primary == null || IsSword(p.equipment.Primary), weaponRule = "swords only",
                bodyOk = p => p.story.bodyType != BodyTypeDefOf.Hulk, bodyRule = "no Hulk",
                specialty = IsSword, specialtyLabel = "swords"
            };
            yield return new SectSpec
            {
                id = "NineTemperings", bodyTrait = "CVG_Body_Steel", meme = "CVG_Meme_BodyWay", role = "CVG_Role_TemperingMaster", ability = "BerserkTrance",
                outcome = "CVG_Outcome_NineTemperings",
                kinds = new[] { "CVG_NineTemperings_Disciple", "CVG_NineTemperings_Inner", "CVG_NineTemperings_Core", "CVG_NineTemperings_Elder" },
                weaponOk = p => p.equipment?.Primary == null, weaponRule = "unarmed",
                bodyOk = p => p.story.bodyType == BodyTypeDefOf.Hulk && (CompCultivator.For(p)?.IsBodyCultivator ?? false), bodyRule = "Hulk body cultivator",
                specialty = t => t.def.IsApparel && (t.def.tradeTags?.Contains("Armor") ?? false), specialtyLabel = "armors"
            };
        }

        /// <summary>Lo que distingue a cada secta en la prueba genérica de contenido (todo lo demás sale de la plantilla).</summary>
        public class SectSpec
        {
            public string id;                       // FormlessSword, NineTemperings…
            public string bodyTrait, meme, role, ability, outcome;
            public string[] kinds;                  // los 4 rangos de combate, el último es el anciano
            public Func<Pawn, bool> weaponOk = p => true;
            public string weaponRule = "any weapon";
            public Func<Pawn, bool> bodyOk = p => true;
            public string bodyRule = "any body";
            public Func<Thing, bool> specialty;     // lo que vende con calidad controlada
            public string specialtyLabel;
        }

        /// <summary>Prueba de contenido común a toda secta del nuevo Sects (plantilla): facción, títulos, favores, constitución,
        /// ideología, ritual, discípulos (armas, cuerpo, historias) y comercio de especialidad.</summary>
        private void TestSect(SectSpec s)
        {
            Note("sect: " + s.id);
            var fdef = DefDatabase<FactionDef>.GetNamedSilentFail("CVG_Faction_" + s.id);
            if (fdef == null) { Check(false, s.id + ": faction def missing"); return; }
            var f = Find.FactionManager.FirstFactionOfDef(fdef);
            Check(f != null, "faction in the world");
            Check(fdef.RoyalTitlesAwardableInSeniorityOrderForReading.Count == 4, "4 honorary titles");
            var permits = DefDatabase<RoyalTitlePermitDef>.AllDefsListForReading.Where(p => p.faction == fdef).ToList();
            Check(permits.Count == 5, "5 favors (" + string.Join(", ", permits.Select(p => p.defName.Replace("CVG_Permit_" + s.id + "_", ""))) + ")");
            foreach (var gift in permits.Where(p => p.HasModExtension<DefModExtension_SectGift>()))
            {
                var ext = gift.GetModExtension<DefModExtension_SectGift>();
                var made = ext.options.Select(o => o.Make()).Where(t => t != null).ToList();
                Check(made.Count == ext.options.Count && made.All(t => t.TryGetComp<CompQuality>() == null
                    || t.TryGetComp<CompQuality>().Quality == ext.options[made.IndexOf(t)].quality), gift.LabelCap + ": " + made.Count + " option(s) make their item");
                foreach (var t in made) t.Destroy();
            }
            var body = DefDatabase<TraitDef>.GetNamed(s.bodyTrait);
            Check(body.degreeDatas.Count == 4 && body.degreeDatas.Select(d => d.degree).SequenceEqual(new[] { 0, 1, 2, 3 }), "constitution with 4 grades: " + body.degreeDatas[0].label + " ... " + body.degreeDatas[3].label);
            Check(body.GetModExtension<DefModExtension_SpecialBody>()?.rootlessOnly == false, "constitution with or without a spirit root");

            var meme = DefDatabase<MemeDef>.GetNamed(s.meme);
            Check(meme.preventApparelRequirements && meme.requireOne?.Count == 4 && meme.requiredRituals?.Count == 1, "meme: no clothing, 4 precepts, its ritual");
            Check(fdef.forcedMemes?.Contains(meme) ?? false, "the faction follows its meme");
            var role = DefDatabase<PreceptDef>.GetNamed(s.role);
            Check(role.activationBelieverCount == 1 && role.GetModExtension<DefModExtension_RoleEmptyMinBelievers>()?.minBelievers == 4
                && (role.grantedAbilities?.Any(a => a.defName == s.ability) ?? false), "specialist " + role.label + ": from 1 believer, mood from 4, " + s.ability);
            Check(role.roleRequirements.OfType<RoleRequirement_HasTrait>().Any(r => r.traits.Contains(body)), "specialist requires the constitution");

            var outcome = DefDatabase<RitualOutcomeEffectDef>.GetNamed(s.outcome);
            var steps = outcome.GetModExtension<DefModExtension_TemperConstitution>()?.steps;
            var expected = new[] { (0.80f, 1.00f), (0.60f, 1.00f), (0.40f, 0.90f), (0.20f, 0.55f) };
            Check(steps?.Count == 4 && Enumerable.Range(0, 4).All(i => Mathf.Abs(steps[i].Chance(0f) - expected[i].Item1) < 0.001f
                && Mathf.Abs(steps[i].Chance(1f) - expected[i].Item2) < 0.001f && steps[i].toTrait == body && steps[i].cost != null), "ritual: 4 steps, approved chances, costs");
            Check(outcome.comps.OfType<RitualOutcomeComp_MinBelievers>().Any(c => c.count == 2), "ritual needs two more believers");

            for (int k = 0; k < s.kinds.Length; k++)
            {
                var kind = DefDatabase<PawnKindDef>.GetNamed(s.kinds[k]);
                int badWeapon = 0, badBody = 0, bodies = 0, sectStories = 0, adults = 0;
                for (int i = 0; i < 6; i++)
                {
                    var p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, f, forceGenerateNewPawn: true));
                    if (p.story.Adulthood != null) { adults++; if (kind.fixedAdultBackstories.Contains(p.story.Adulthood)) sectStories++; }
                    if (!s.weaponOk(p)) badWeapon++;
                    if (!s.bodyOk(p)) badBody++;
                    if (ConstitutionUtility.Degree(p, body) != null) bodies++;
                    p.Destroy();
                }
                bool elder = k == s.kinds.Length - 1;
                Check(badWeapon == 0 && badBody == 0 && (!elder || bodies == 6) && kind.fixedAdultBackstories.Count > 0 && sectStories == adults,
                    kind.label + ": " + s.weaponRule + ", " + s.bodyRule + ", " + bodies + "/6 with the constitution, sect backstory " + sectStories + "/" + adults);
            }

            var trader = DefDatabase<TraderKindDef>.GetNamed("CVG_Trader_" + s.id);
            var stock = new List<Thing>();
            for (int i = 0; i < 8; i++) foreach (var g in trader.stockGenerators) stock.AddRange(g.GenerateThings(map.Tile, f));
            var sold = stock.Where(s.specialty).ToList();
            var q = sold.Select(t => t.TryGetComp<CompQuality>()?.Quality).Where(x => x != null).Select(x => x.Value).ToList();
            Check(sold.Count > 0 && q.All(x => x >= QualityCategory.Good && x <= QualityCategory.Masterwork), "trade: " + sold.Count + " " + s.specialtyLabel + ", Good-Masterwork, never legendary");
            foreach (var t in stock) t.Destroy();
        }

        private void Finish()
        {
            phase = -1;
            sb.Append(fails == 0 ? "\n  ALL OK" : "\n  FAILURES: " + fails);
            if (fails == 0) Log.Message(sb.ToString()); else Log.Error(sb.ToString());
        }
    }
}
