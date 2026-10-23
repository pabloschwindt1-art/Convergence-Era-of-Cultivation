using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Convergence.Factions
{
    /// <summary>
    /// Recorrido de los favores de todas las sectas, como los usaría el jugador. Solo con <c>-cvgfavorstest</c> (y un mapa, p. ej.
    /// -quicktest): la Luna Sangrienta hostil hasta que la ideología del jugador sigue el Camino Demoníaco; después, por secta, el título
    /// de anciano invitado, los cinco favores usados de verdad (recursos, refuerzos, obra maestra, regalo a elección con todas sus
    /// opciones y golpe), que llegue lo prometido, que los refuerzos peleen contra un enemigo y que no sean hostiles. Deja en el log el
    /// texto de cada favor para revisarlo. Inerte sin el argumento.
    /// </summary>
    public class GameComponent_FavorsTest : GameComponent
    {
        private static readonly bool Enabled = GenCommandLine.CommandLineArgPassed("cvgfavorstest");
        private readonly StringBuilder sb = new StringBuilder("[Convergence] Favors test");
        private int fails;
        private bool started;
        private IEnumerator<int> script;
        private int waitUntil;
        private Map map;
        private Pawn caller;

        public GameComponent_FavorsTest(Game game) { }

        private void Check(bool ok, string what)
        {
            sb.Append("\n    " + (ok ? "ok   " : "FAIL ") + what);
            if (!ok) fails++;
        }

        private void Note(string what) => sb.Append("\n- " + what);
        private void Text(string what) => sb.Append("\n      " + what);

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
                    script = Script();
                }
                if (script == null || Find.TickManager.TicksGame < waitUntil) return;
                if (script.MoveNext()) waitUntil = Find.TickManager.TicksGame + Math.Max(1, script.Current);
                else script = null;
            }
            catch (Exception e)
            {
                script = null;
                Check(false, "exception: " + e);
                Finish();
            }
        }

        private static readonly string[] Sects = { "FormlessSword", "NineTemperings", "HeavenlyCauldron", "BloodMoon", "InvisibleThread" };

        private static Faction FactionOf(string sect) =>
            Find.FactionManager.FirstFactionOfDef(DefDatabase<FactionDef>.GetNamedSilentFail("CVG_Faction_" + sect));

        private IEnumerator<int> Script()
        {
            Find.TickManager.CurTimeSpeed = TimeSpeed.Superfast;
            if (!GameComponent_Convergence.Active) GameComponent_Convergence.Instance?.MarkOccurred();
            ConvergenceFactionUtility.EnsureFactions();
            caller = map.mapPawns.FreeColonistsSpawned.FirstOrDefault(p => p.royalty != null && !p.Downed);
            Check(caller != null, "a colonist to ask for favors: " + caller?.LabelShort);
            if (caller == null) { Finish(); yield break; }
            var initial = new HashSet<int>(map.listerThings.AllThings.Select(t => t.thingIDNumber));

            // 0) Ideologías de secta: nuestras, sin nada vanilla al azar.
            GameComponent_SectIdeos.ApplyAll();
            foreach (var sect in Sects.Concat(new[] { "RogueCultivators" }))
            {
                var f = FactionOf(sect);
                var sid = f?.ideos?.PrimaryIdeo;
                if (sid == null) continue;
                var ext = f.def.GetModExtension<DefModExtension_SectIdeo>();
                Note("ideoligion of " + f.Name + ": " + sid.name);
                Check(sid.name == f.def.ideoName && f.LeaderTitle == f.def.leaderTitle && sid.iconDef == ext?.icon,
                    "name '" + sid.name + "', leader title '" + f.LeaderTitle + "', icon " + sid.iconDef?.defName);
                Text("memes: " + string.Join(", ", sid.memes.Select(m => m.LabelCap.ToString())));
                Text("deities: " + string.Join(", ", (sid.foundation as IdeoFoundation_Deity)?.DeitiesListForReading.Select(d => d.name + " (" + d.type + ")") ?? Enumerable.Empty<string>()));
                Text("precepts: " + string.Join(" | ", sid.PreceptsListForReading.Select(p => p.LabelCap + " [" + p.def.defName + "]")));
                var vanilla = sid.PreceptsListForReading.Where(p => !p.def.defName.StartsWith("CVG_") && !GameComponent_SectIdeos.Kept.Contains(p.def.defName)).ToList();
                var elder = sid.PreceptsListForReading.OfType<Precept_Role>().FirstOrDefault(r => r.def == PreceptDefOf.IdeoRole_Moralist);
                Check(elder == null || elder.LabelCap == "CVG_Factions_GrandElder".Translate().CapitalizeFirst(), "second role: " + elder?.LabelCap);
                Check(vanilla.Count == 0, "only our precepts and the basic ones (funeral, festival, birth, speech, role change)" + (vanilla.Count > 0 ? ": " + string.Join(", ", vanilla.Select(p => p.def.defName)) : ""));
            }

            // 1) La Luna Sangrienta: enemiga natural, hostil con la Espada y los Nueve Templados; neutral con el Camino Demoníaco.
            Note("Blood Moon diplomacy");
            var moon = FactionOf("BloodMoon");
            var demonic = DefDatabase<MemeDef>.GetNamed("CVG_Meme_DemonicWay");
            var ideo = Faction.OfPlayer.ideos?.PrimaryIdeo;
            Check(moon != null && ideo != null, "Blood Moon in the world and a player ideoligion");
            if (moon == null || ideo == null) { Finish(); yield break; }
            bool wasDemonic = ideo.memes.Contains(demonic);
            if (!wasDemonic)
            {
                Check(moon.HostileTo(Faction.OfPlayer), "hostile to a colony that does not walk the demonic way (goodwill " + moon.PlayerGoodwill + ")");
                moon.TryAffectGoodwillWith(Faction.OfPlayer, 0, canSendMessage: false, canSendHostilityLetter: false);
                caller.royalty.GainFavor(moon, 2);
                var permit = DefDatabase<RoyalTitlePermitDef>.AllDefsListForReading.First(p => p.faction == moon.def && p.minTitle == moon.def.RoyalTitlesAwardableInSeniorityOrderForReading[0]);
                caller.royalty.AddPermit(permit, moon);
                var blocked = permit.Worker.GetRoyalAidOptions(map, caller, moon).FirstOrDefault();
                Check(blocked == null || blocked.Disabled || blocked.action == null, "while hostile its favors are blocked: " + blocked?.Label);
            }
            foreach (var other in new[] { "FormlessSword", "NineTemperings" })
            {
                var f = FactionOf(other);
                Check(f != null && f.HostileTo(moon), "hostile with " + f?.Name);
            }
            if (!wasDemonic) ideo.memes.Add(demonic);
            ConvergenceFactionUtility.ApplyRelationsToExisting();
            Check(!moon.HostileTo(Faction.OfPlayer), "with the demonic way in the colony's ideoligion it becomes neutral (goodwill " + moon.PlayerGoodwill + ")");
            if (!wasDemonic) ideo.memes.Remove(demonic);

            // 2) Los cinco favores de cada secta.
            foreach (var sect in Sects)
            {
                var faction = FactionOf(sect);
                if (faction == null)
                {
                    Note(sect + ": not in this world" + (sect == "InvisibleThread" ? " (needs Biotech Wuxia)" : ""));
                    continue;
                }
                Note(sect + " — " + faction.Name);
                if (faction.HostileTo(Faction.OfPlayer) || faction.PlayerGoodwill < 50)
                    faction.TryAffectGoodwillWith(Faction.OfPlayer, 60 - faction.PlayerGoodwill, canSendMessage: false, canSendHostilityLetter: false);
                Check(!faction.HostileTo(Faction.OfPlayer), "friendly (goodwill " + faction.PlayerGoodwill + ")");
                var titles = faction.def.RoyalTitlesAwardableInSeniorityOrderForReading;
                caller.royalty.GainFavor(faction, 34 - caller.royalty.GetFavor(faction));
                Check(caller.royalty.GetCurrentTitle(faction) == titles.Last(), "title: " + caller.royalty.GetCurrentTitle(faction)?.GetLabelCapFor(caller));
                foreach (var t in titles) Text("title  " + t.GetLabelCapFor(caller) + ": " + t.description);
                var permits = DefDatabase<RoyalTitlePermitDef>.AllDefsListForReading.Where(p => p.faction == faction.def).ToList();
                foreach (var p in permits) if (!caller.royalty.HasPermit(p, faction)) caller.royalty.AddPermit(p, faction);
                Check(permits.Count == 5 && permits.All(p => caller.royalty.HasPermit(p, faction)), "the five favors chosen, " + caller.royalty.GetPermitPoints(faction) + " points left");
                caller.royalty.GainFavor(faction, 60);

                var aidPawns = new List<Pawn>();
                foreach (var permit in permits)
                {
                    Text("favor  " + permit.LabelCap + " [" + permit.minTitle?.GetLabelCapFor(caller) + ", " + permit.cooldownDays + " days, " + permit.royalAid?.favorCost + " contribution]: " + permit.description);
                    var option = permit.Worker.GetRoyalAidOptions(map, caller, faction).FirstOrDefault();
                    Text("menu   " + option?.Label);
                    Check(option != null && !option.Disabled && option.action != null, permit.LabelCap + ": available in the royal aid menu");
                    if (option == null || option.Disabled || option.action == null) continue;

                    if (permit.Worker is RoyalTitlePermitWorker_SectGift gift)
                    {
                        var ext = permit.GetModExtension<DefModExtension_SectGift>();
                        var before = new HashSet<int>(map.listerThings.AllThings.Select(t => t.thingIDNumber));
                        if (ext.playerChooses)
                        {
                            option.action();
                            var menu = Find.WindowStack.Windows.OfType<FloatMenu>().LastOrDefault();
                            var labels = menu == null ? new List<string>() : MenuLabels(menu);
                            Check(labels.Count == ext.options.Count, permit.LabelCap + ": the colonist chooses among " + labels.Count + " (" + string.Join(", ", labels) + ")");
                            menu?.Close(false);
                            Find.Targeter.StopTargeting();
                            foreach (var o in ext.options) DropGift(gift, faction, o);
                        }
                        else
                        {
                            Text("random among: " + string.Join(", ", ext.options.Select(o => o.Label + (o.count > 1 ? " x" + o.count : "") + " (" + o.quality + ")")));
                            DropGift(gift, faction, ext.options.RandomElement());
                        }
                        yield return 600;
                        var landed = map.listerThings.AllThings.Where(t => !before.Contains(t.thingIDNumber) && t.def.category == ThingCategory.Item && t.Spawned).ToList();
                        foreach (var o in ext.playerChooses ? ext.options : ext.options.Take(0).ToList())
                        {
                            bool got = landed.Any(t => (o.thing != null ? t.def == o.thing : t.def.weaponTags?.Contains(o.weaponTag) == true)
                                                      && (t.TryGetComp<CompQuality>() == null || t.TryGetComp<CompQuality>().Quality == o.quality)
                                                      && t.stackCount >= Mathf.Min(o.count, t.def.stackLimit));
                            Check(got, permit.LabelCap + " -> " + o.Label + (o.count > 1 ? " x" + o.count : "") + (o.thing?.HasComp(typeof(CompQuality)) != false ? " " + o.quality : "") + " delivered");
                        }
                        if (!ext.playerChooses)
                            Check(landed.Any(t => ext.options.Any(o => (o.thing != null ? t.def == o.thing : t.def.weaponTags?.Contains(o.weaponTag) == true)
                                                                       && (t.TryGetComp<CompQuality>() == null || t.TryGetComp<CompQuality>().Quality == o.quality))),
                                permit.LabelCap + " -> " + string.Join(", ", landed.Select(t => t.LabelCap)) + " delivered");
                        continue;
                    }

                    int skyBefore = map.listerThings.AllThings.Count(t => t is Skyfaller);
                    var factionBefore = new HashSet<Pawn>(map.mapPawns.SpawnedPawnsInFaction(faction));
                    var itemsBefore = permit.royalAid?.itemsToDrop?.ToDictionary(i => i.thingDef, i => Count(i.thingDef));
                    if (!UsePermit(permit, faction, option, out var reason))
                    {
                        if (reason != null && reason.Contains("BadTemperature".Translate())) { Note(permit.LabelCap + " skipped: bad temperature on this map"); continue; }
                        Check(false, permit.LabelCap + ": could not be used: " + reason);
                        continue;
                    }
                    Check(map.listerThings.AllThings.Count(t => t is Skyfaller) > skyBefore || map.mapPawns.SpawnedPawnsInFaction(faction).Count > factionBefore.Count,
                        permit.LabelCap + ": on its way");
                    yield return 600;
                    if (itemsBefore != null)
                    {
                        foreach (var i in permit.royalAid.itemsToDrop)
                            Check(Count(i.thingDef) >= itemsBefore[i.thingDef] + i.count, permit.LabelCap + " -> " + i.thingDef.LabelCap + " x" + i.count + " (" + (Count(i.thingDef) - itemsBefore[i.thingDef]) + " arrived)");
                    }
                    if (permit.royalAid?.pawnKindDef != null)
                    {
                        var arrived = map.mapPawns.SpawnedPawnsInFaction(faction).Where(p => !factionBefore.Contains(p)).ToList();
                        int expected = permit.royalAid.pawnCount;
                        Check(arrived.Count >= expected && arrived.All(p => p.kindDef == permit.royalAid.pawnKindDef),
                            permit.LabelCap + " -> " + arrived.Count + "/" + expected + " " + permit.royalAid.pawnKindDef.label + (arrived.Count > 0 ? " (" + string.Join(", ", arrived.Select(p => p.LabelShort)) + ")" : ""));
                        Check(arrived.All(p => !p.HostileTo(Faction.OfPlayer)), permit.LabelCap + ": not hostile to the colony");
                        var lord = arrived.FirstOrDefault()?.GetLord();
                        Check(lord?.LordJob is LordJob_AssistColony, permit.LabelCap + ": they come to help (" + lord?.LordJob?.GetType().Name + ")");
                        aidPawns.AddRange(arrived);
                    }
                }

                // 3) Los refuerzos pelean: dos enemigos armados en medio de ellos.
                if (aidPawns.Count > 0)
                {
                    var enemies = SpawnEnemies(aidPawns);
                    Check(enemies.Count > 0, "enemies to fight: " + enemies.Count);
                    int damaged = 0;
                    for (int i = 0; i < 24 && enemies.Any(e => !e.Dead && !e.Downed); i++) yield return 250;
                    damaged = enemies.Count(e => e.Dead || e.Downed || e.health.hediffSet.hediffs.OfType<Hediff_Injury>().Any());
                    var fighters = aidPawns.Where(p => p.records.GetValue(RecordDefOf.ShotsFired) > 0 || p.records.GetValue(RecordDefOf.Kills) > 0
                                                       || (p.mindState?.lastAttackTargetTick ?? -1) > 0).ToList();
                    Check(damaged == enemies.Count && fighters.Count > 0, "the aid fights: " + damaged + "/" + enemies.Count + " enemies hurt or down, "
                        + fighters.Count + "/" + aidPawns.Count + " allies attacked");
                    foreach (var e in enemies) if (!e.Destroyed) e.Destroy();
                }

                // Limpieza: fuera la ayuda, las cápsulas y lo que llegó, para la secta siguiente.
                foreach (var t in map.listerThings.AllThings.Where(t => !initial.Contains(t.thingIDNumber) && !(t is Pawn p && p.IsColonist)
                                                                     && (t is Pawn || t is Corpse || t.def.category == ThingCategory.Item || t is Skyfaller
                                                                         || t.def.thingClass == typeof(Building) && t.def.defName.Contains("DropPod"))).ToList())
                    if (!t.Destroyed) t.Destroy();
                yield return 30;
            }
            Finish();
        }

        private static List<string> MenuLabels(FloatMenu menu)
        {
            var field = HarmonyLib.AccessTools.Field(typeof(FloatMenu), "options");
            var opts = field?.GetValue(menu) as List<FloatMenuOption>;
            return opts?.Select(o => o.Label).ToList() ?? new List<string>();
        }

        private int Count(ThingDef def) => map.listerThings.ThingsOfDef(def).Where(t => t.Spawned).Sum(t => t.stackCount);

        private bool DropCell(out IntVec3 cell) => CellFinder.TryFindRandomCellNear(caller.Position, map, 12, c => !c.Fogged(map) && c.GetEdifice(map) == null
            && DropCellFinder.CanPhysicallyDropInto(c, map, canRoofPunch: true) && c.Standable(map) && c.DistanceTo(caller.Position) > 3, out cell);

        private void DropGift(RoyalTitlePermitWorker_SectGift worker, Faction faction, SectGiftOption option)
        {
            if (!DropCell(out var cell)) { Check(false, "no drop cell"); return; }
            worker.Begin(caller, faction, map, free: true, option);
            worker.OrderForceTarget(new LocalTargetInfo(cell));
            Find.Targeter.StopTargeting();
        }

        /// <summary>Como el jugador: la opción del menú de ayuda y después el objetivo.</summary>
        private bool UsePermit(RoyalTitlePermitDef def, Faction faction, FloatMenuOption opt, out string reason)
        {
            reason = null;
            opt.action();
            if (!DropCell(out var cell)) { reason = "no drop cell"; Find.Targeter.StopTargeting(); return false; }
            if (def.Worker is RoyalTitlePermitWorker_Targeted targeted) targeted.OrderForceTarget(new LocalTargetInfo(cell));
            else { reason = "worker is not targeted (" + def.Worker.GetType().Name + ")"; Find.Targeter.StopTargeting(); return false; }
            Find.Targeter.StopTargeting();
            return true;
        }

        private List<Pawn> SpawnEnemies(List<Pawn> allies)
        {
            var result = new List<Pawn>();
            var enemyFaction = Find.FactionManager.AllFactionsListForReading.FirstOrDefault(f => f.def == FactionDefOf.Pirate && f.HostileTo(Faction.OfPlayer))
                               ?? Find.FactionManager.RandomEnemyFaction(allowNonHumanlike: false);
            var center = allies.Where(p => p.Spawned).Select(p => p.Position).FirstOrDefault();
            if (enemyFaction == null || !center.IsValid) return result;
            for (int i = 0; i < 2; i++)
            {
                var enemy = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Drifter, enemyFaction, mustBeCapableOfViolence: true));
                var cell = CellFinder.RandomClosewalkCellNear(center, map, 4);
                GenSpawn.Spawn(enemy, cell, map);
                result.Add(enemy);
            }
            LordMaker.MakeNewLord(enemyFaction, new LordJob_AssaultColony(enemyFaction, canKidnap: false, canTimeoutOrFlee: false), map, result);
            return result;
        }

        private void Finish()
        {
            sb.Append(fails == 0 ? "\n  ALL OK" : "\n  FAILURES: " + fails);
            if (fails == 0) Log.Message(sb.ToString()); else Log.Error(sb.ToString());
            script = null;
        }
    }
}
