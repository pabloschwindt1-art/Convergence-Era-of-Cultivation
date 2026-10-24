using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence.QiTech
{
    /// <summary>
    /// Autotest de las armas y torretas de Qi-Tech v0.2. Solo con <c>-cvgqitechwartest</c> (y un mapa, p. ej. -quicktest); inerte sin el
    /// argumento (ni parche de Harmony ni nada en el mapa). Comprueba en defs el grafo de investigación (sin ciclos, sin proyectos del Dao),
    /// recetas, etiquetas (ni raids ni comerciantes), texturas y la curva de supresión; y en juego real: un colono mortal y una torreta
    /// disparan a un objetivo, se captura el <see cref="DamageInfo"/> de verdad (Thing.TakeDamage) y se mide el combustible por disparo;
    /// la torreta no dispara sin energía, apagada ni sin piedras; y guardar/cargar conserva el combustible y el grado por instancia.
    /// El estado es estático porque sobrevive a la carga de la partida (el GameComponent se vuelve a crear).
    /// </summary>
    public class GameComponent_QiTechMilitaryTest : GameComponent
    {
        private static readonly bool Enabled = GenCommandLine.CommandLineArgPassed("cvgqitechwartest");
        private const string SaveName = "CVG_QiTechWarTest";

        private enum Phase { Wait, Active, NoPower, SwitchedOff, NoFuel, Containment, Saving, AfterLoad, Done }

        private static Phase phase = Phase.Wait;
        private static int phaseTicks;
        private static readonly StringBuilder sb = new StringBuilder("[Convergence] Qi-Tech military self-test");
        private static int fails;

        // Capturas reales de daño (prefijo de Thing.TakeDamage), por def del arma.
        private static readonly Dictionary<ThingDef, DamageInfo> captured = new Dictionary<ThingDef, DamageInfo>();
        private static readonly HashSet<ThingDef> ourWeapons = new HashSet<ThingDef>();

        private static Map map;
        private static IntVec3 center;
        private static Building_TurretGun turret;
        private static Pawn shooter, target;
        private static readonly HashSet<Projectile> seen = new HashSet<Projectile>();
        private static int shots;
        private static float fuelAtStart;
        private static string savedTurretId, savedShooterId;
        private static float savedFuel;

        public GameComponent_QiTechMilitaryTest(Game game) { }

        private static void Check(bool ok, string what)
        {
            sb.Append("\n    " + (ok ? "ok   " : "FAIL ") + what);
            if (!ok) fails++;
        }

        public static void Prefix_TakeDamage(Thing __instance, DamageInfo dinfo)
        {
            if (dinfo.Weapon != null && ourWeapons.Contains(dinfo.Weapon) && !captured.ContainsKey(dinfo.Weapon))
                captured[dinfo.Weapon] = dinfo;
        }

        public override void GameComponentTick()
        {
            if (!Enabled || phase == Phase.Done) return;
            if (Find.CurrentMap == null) return;
            try
            {
                if (phase == Phase.Wait)
                {
                    if (Find.TickManager.TicksGame < 300) return;
                    Find.TickManager.CurTimeSpeed = TimeSpeed.Ultrafast;
                    map = Find.CurrentMap;
                    StaticChecks();
                    SetupLive();
                    Next(Phase.Active);
                    return;
                }
                if (phase == Phase.AfterLoad) { if (++phaseTicks > 120) AfterLoad(); return; }
                if (phase == Phase.Saving) return;
                phaseTicks++;
                KeepTarget();
                CountShots();
                // Progreso cada 500 ticks: diagnóstico y, además, run-test.ps1 cierra el juego si el log queda 45 s quieto.
                if (phaseTicks % 500 == 1)
                    Log.Message($"[Convergence] Qi-Tech military test: {phase} t={phaseTicks} active={turret.Active} power={turret.GetComp<CompPowerTrader>().PowerOn} fuel={Fuel:0.##} available={turret.AttackVerb.Available()} stunned={turret.GetComp<CompStunnable>()?.StunHandler?.Stunned} target={turret.CurrentTarget} enemy={target?.Position} dist={target?.Position.DistanceTo(turret.Position):0.#} shots={shots}");
                switch (phase)
                {
                    case Phase.Active:
                        if ((captured.ContainsKey(Def("CVG_QT_Gun_InductionTurret")) && captured.ContainsKey(Def("CVG_QT_InductionRifle")) && shots >= 6) || phaseTicks > 2500)
                            EndActive();
                        break;
                    case Phase.NoPower:
                    case Phase.SwitchedOff:
                    case Phase.NoFuel:
                        if (phaseTicks > 700) EndSilent();
                        break;
                    case Phase.Containment:
                        if ((captured.ContainsKey(Def("CVG_QT_Gun_ContainmentTurret")) && shots >= 2) || phaseTicks > 3000)
                            EndContainment();
                        break;
                }
            }
            catch (Exception e)
            {
                Check(false, "exception in " + phase + ": " + e);
                Finish();
            }
        }

        private static ThingDef Def(string name) => DefDatabase<ThingDef>.GetNamed(name);

        private static void Next(Phase p)
        {
            phase = p;
            phaseTicks = 0;
            Log.Message("[Convergence] Qi-Tech military test: phase " + p);
        }

        // ================= Comprobaciones de defs =================

        private static void StaticChecks()
        {
            // Investigación: el defName de la v0.1 sigue (partidas guardadas) y nada de la cadena exige el Dao.
            var ours = new[] { "CVG_Research_QiTech", "CVG_Research_QiStabilization", "CVG_Research_QiExosuit", "CVG_Research_QiInduction",
                "CVG_Research_QiWeapons", "CVG_Research_QiDefense", "CVG_Research_QiAdvancedMilitary",
                "CVG_Research_QiFundamentals", "CVG_Research_SpiritResonance", "CVG_Research_AdvancedSpiritInstrumentation" };
            foreach (var r in ours)
            {
                var proj = DefDatabase<ResearchProjectDef>.GetNamedSilentFail(r);
                Check(proj != null, "research " + r);
                if (proj == null) continue;
                var chain = new HashSet<ResearchProjectDef>();
                bool cycle = HasCycle(proj, new HashSet<ResearchProjectDef>(), chain);
                Check(!cycle, r + ": no cycles in its prerequisites");
                var dao = chain.Where(p => p.HasModExtension<DefModExtension_DaoResearch>() || p.knowledgeCategory != null
                    || (p.requiredResearchBuilding != null && p.requiredResearchBuilding.defName.StartsWith("CVG_"))).ToList();
                Check(dao.Count == 0, r + ": researchable by a mortal at a normal bench (chain of " + chain.Count + ")"
                    + (dao.Count > 0 ? " - needs " + string.Join(", ", dao.Select(p => p.defName)) : ""));
            }

            // Armas: receta solo en el banco de fabricación, solo Artesanía; etiquetas propias; grado; texturas.
            var tags = new HashSet<string>(DefDatabase<PawnKindDef>.AllDefs.Where(k => k.weaponTags != null).SelectMany(k => k.weaponTags));
            var mortal = MakeMortal();
            mortal.skills.GetSkill(SkillDefOf.Crafting).Level = 20;
            foreach (var (name, grade) in new[] { ("CVG_QT_InductionRifle", 1), ("CVG_QT_FocusedRifle", 2), ("CVG_QT_CompressionCannon", 3) })
            {
                var d = Def(name);
                ourWeapons.Add(d);
                Check(SpiritGradeUtility.GradeOf(d) == grade, name + " grade " + grade);
                var recipe = DefDatabase<RecipeDef>.AllDefs.FirstOrDefault(rd => rd.ProducedThingDef == d);
                Check(recipe != null && recipe.AllRecipeUsers.All(u => u.defName == "FabricationBench") && recipe.AllRecipeUsers.Any(),
                    name + ": made only at the fabrication bench (" + (recipe == null ? "no recipe" : string.Join(",", recipe.AllRecipeUsers.Select(u => u.defName))) + ")");
                Check(recipe != null && recipe.PawnSatisfiesSkillRequirements(mortal) && recipe.researchPrerequisite != null,
                    name + ": a mortal crafter can make it (research " + recipe?.researchPrerequisite?.defName + ")");
                var gun = ThingMaker.MakeThing(d);
                Check(EquipmentUtility.CanEquip(gun, mortal), name + ": a mortal can equip it");
                gun.Destroy();
                Check(d.weaponTags.All(t => !tags.Contains(t)), name + ": no pawnkind carries it (raids unchanged): " + string.Join(",", d.weaponTags));
                Check(!d.tradeTags.Contains("WeaponRanged"), name + ": not in vanilla trader stock");
                Tex(d.graphicData.texPath, name);
                if (d.graphicData.graphicClass == typeof(Graphic_Random))
                    Check(d.graphicData.Graphic is Graphic_Random, name + ": bare Graphic_Random (Character Editor casts it; Graphic_RandomRotated breaks its window)");
                Tex(d.Verbs[0].defaultProjectile.graphicData.texPath, d.Verbs[0].defaultProjectile.defName);
            }
            mortal.Destroy();
            foreach (var (name, gunName, grade, perShot) in new[] { ("CVG_QT_InductionTurret", "CVG_QT_Gun_InductionTurret", 2, 0.1f), ("CVG_QT_ContainmentTurret", "CVG_QT_Gun_ContainmentTurret", 3, 0.4f) })
            {
                var d = Def(name);
                var g = Def(gunName);
                ourWeapons.Add(g);
                Check(d.building.turretGunDef == g && SpiritGradeUtility.GradeOf(g) == grade, name + ": gun " + gunName + " grade " + grade);
                Check(d.HasComp(typeof(CompPowerTrader)) && d.HasComp(typeof(CompFlickable)) && d.GetCompProperties<CompProperties_Refuelable>()?.fuelFilter.Allows(Def("CVG_SpiritStone")) == true,
                    name + ": needs power, has a switch, burns spirit stones");
                Check(Mathf.Approximately(g.Verbs[0].consumeFuelPerShot, perShot), name + ": " + perShot + " stone per shot");
                Check(d.researchPrerequisites.All(r => !r.HasModExtension<DefModExtension_DaoResearch>()), name + ": scientific research");
                Tex(d.graphicData.texPath, name);
                Tex(g.graphicData.texPath, gunName);
                Tex(d.uiIconPath, name + " icon");
            }

            // Grado 0 = vanilla; 1-3 según la curva del Core (grado >= reino: daño completo; menor: suprimido y creciente con el grado).
            Check(SpiritGradeUtility.GradeOf(Def("Gun_AssaultRifle")) == 0 && SpiritGradeUtility.SuppressionFactor(0, 0) == 1f, "vanilla rifle grade 0, full damage on mortals");
            var table = new StringBuilder();
            bool curveOk = true;
            for (int level = 1; level <= 3; level++)
            {
                float prev = -1f;
                for (int g = 0; g <= 3; g++)
                {
                    float f = SpiritGradeUtility.SuppressionFactor(level, g);
                    table.Append($" L{level}/G{g}={f:0.##}");
                    if (g >= level ? f != 1f : f >= 1f) curveOk = false;
                    if (f < prev) curveOk = false;
                    prev = f;
                }
            }
            Check(curveOk, "suppression by grade:" + table);
        }

        private static bool HasCycle(ResearchProjectDef p, HashSet<ResearchProjectDef> stack, HashSet<ResearchProjectDef> all)
        {
            if (stack.Contains(p)) return true;
            if (!all.Add(p)) return false;
            stack.Add(p);
            var pre = (p.prerequisites ?? new List<ResearchProjectDef>()).Concat(p.hiddenPrerequisites ?? new List<ResearchProjectDef>());
            foreach (var q in pre) if (HasCycle(q, stack, all)) return true;
            stack.Remove(p);
            return false;
        }

        /// <summary>Textura única, o carpeta de variantes (Graphic_Random).</summary>
        private static void Tex(string path, string what)
        {
            int variants = path.NullOrEmpty() ? 0 : ContentFinder<Texture2D>.GetAllInFolder(path).Count();
            bool single = !path.NullOrEmpty() && ContentFinder<Texture2D>.Get(path, false) != null;
            Check(single || variants > 0, "texture " + what + ": " + path + (variants > 0 ? " (" + variants + " variants)" : ""));
        }

        /// <summary>Colono sin raíz espiritual ni reino (se regenera si sale con raíz).</summary>
        private static Pawn MakeMortal()
        {
            Pawn p = null;
            for (int i = 0; i < 30; i++)
            {
                p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayer, forceGenerateNewPawn: true));
                var c = CompCultivator.For(p);
                if (c == null || (!c.HasRoot && c.RealmLevel == 0)) return p;
                p.Destroy();
            }
            Check(false, "could not generate a colonist without a spirit root");
            return p;
        }

        // ================= Prueba en vivo =================

        private static void SetupLive()
        {
            var h = new Harmony("prospero1088.convergence.qitech.wartest");
            h.Patch(AccessTools.Method(typeof(Thing), nameof(Thing.TakeDamage)), prefix: new HarmonyMethod(typeof(GameComponent_QiTechMilitaryTest), nameof(Prefix_TakeDamage)));

            center = map.Center + new IntVec3(20, 0, 0);
            foreach (var cell in GenRadial.RadialCellsAround(center, 14f, true).Where(x => x.InBounds(map)))
            {
                foreach (var t in cell.GetThingList(map).ToList()) if (t.def.destroyable && !(t is Pawn)) t.Destroy();
                if (!cell.Standable(map) || cell.GetTerrain(map).passability != Traversability.Standable) map.terrainGrid.SetTerrain(cell, TerrainDefOf.Soil);
                map.roofGrid.SetRoof(cell, null);
            }
            turret = SpawnTurret("CVG_QT_InductionTurret");
            shooter = MakeMortal();
            GenSpawn.Spawn(shooter, center + new IntVec3(10, 0, 9), map);
            var rifle = (ThingWithComps)ThingMaker.MakeThing(Def("CVG_QT_InductionRifle"));
            shooter.equipment.DestroyAllEquipment();
            shooter.equipment.AddEquipment(rifle);
            Check(SpiritGradeUtility.RealmLevel(shooter) == 0, "the shooter is a mortal (realm 0)");
            shooter.drafter.Drafted = true;
            KeepTarget();
        }

        private static Building_TurretGun SpawnTurret(string def)
        {
            var t = (Building_TurretGun)ThingMaker.MakeThing(Def(def));
            t.SetFaction(Faction.OfPlayer);
            GenSpawn.Spawn(t, center, map);
            var fuel = t.GetComp<CompRefuelable>();
            Check(fuel.Fuel == 0f && !t.AttackVerb.Available(), def + ": built empty, cannot fire before it is fed");
            fuel.Refuel(12f);
            // Con capacidad 80-100, 12 piedras quedan por debajo del umbral y un colono vendría a recargarla en medio de la prueba.
            fuel.allowAutoRefuel = false;
            t.GetComp<CompPowerTrader>().PowerOn = true; // sin red eléctrica en el mapa de prueba: se simula la corriente
            fuelAtStart = fuel.Fuel;
            shots = 0;
            seen.Clear();
            return t;
        }

        private static void KeepTarget()
        {
            // Las torretas no apuntan a pawns derribados: si cae, se cambia por uno de pie (si no, las fases "silenciosas" no prueban nada).
            if (target == null || target.Dead || !target.Spawned || target.Downed)
            {
                if (target != null && !target.Destroyed) target.Destroy();
                target = PawnGenerator.GeneratePawn(PawnKindDefOf.Villager, Faction.OfAncientsHostile);
                target.equipment?.DestroyAllEquipment();
                GenSpawn.Spawn(target, center + new IntVec3(10, 0, 0), map);
            }
            hadTargetTicks++;
            if (!target.stances.stunner.Stunned) target.stances.stunner.StunFor(240, null, addBattleLog: false, showMote: false);
        }

        private static void CountShots()
        {
            if (turret == null) return;
            foreach (var t in map.listerThings.ThingsInGroup(ThingRequestGroup.Projectile))
                if (t is Projectile p && p.Launcher == turret && seen.Add(p)) shots++;
        }

        private static float Fuel => turret.GetComp<CompRefuelable>().Fuel;

        private static void CheckDinfo(string gunName, int grade, Thing expectedInstigator)
        {
            var g = Def(gunName);
            if (!captured.TryGetValue(g, out var dinfo)) { Check(false, gunName + ": no real hit captured"); return; }
            Check(dinfo.Weapon == g && dinfo.Instigator == expectedInstigator, gunName + ": DamageInfo.Weapon = " + dinfo.Weapon?.defName + ", instigator = " + dinfo.Instigator?.LabelShort);
            int ag = SpiritGradeUtility.AttackGrade(dinfo);
            Check(ag == grade, gunName + ": AttackGrade of the real hit = " + ag + " (expected " + grade + ")");
        }

        private static void EndActive()
        {
            CheckDinfo("CVG_QT_InductionRifle", 1, shooter);
            CheckDinfo("CVG_QT_Gun_InductionTurret", 2, turret);
            float used = fuelAtStart - Fuel;
            Check(shots > 0 && Mathf.Abs(used - shots * 0.1f) < 0.001f, $"induction turret: {shots} shots used {used:0.###} stones (0.1 per shot)");
            shooter.drafter.Drafted = false; // ya disparó: que no derribe al objetivo en las fases siguientes
            Next(Phase.NoPower);
            turret.GetComp<CompPowerTrader>().PowerOn = false;
            Mark();
        }

        private static int markShots;
        private static int hadTargetTicks;
        private static float markFuel;
        private static void Mark() { markShots = shots; markFuel = Fuel; hadTargetTicks = 0; }

        private static void EndSilent()
        {
            bool silent = shots == markShots && Mathf.Approximately(markFuel, Fuel);
            Check(hadTargetTicks > 600, phase + ": a valid standing enemy was in range the whole time (" + hadTargetTicks + " ticks)");
            switch (phase)
            {
                case Phase.NoPower:
                    Check(silent && !turret.Active, "without power: no shots, no fuel used (Active=" + turret.Active + ")");
                    turret.GetComp<CompPowerTrader>().PowerOn = true;
                    turret.GetComp<CompFlickable>().SwitchIsOn = false;
                    Check(!turret.GetComp<CompPowerTrader>().PowerOn, "switching off cuts the power");
                    Next(Phase.SwitchedOff);
                    break;
                case Phase.SwitchedOff:
                    Check(silent, "switched off: no shots, no fuel used");
                    turret.GetComp<CompFlickable>().SwitchIsOn = true;
                    turret.GetComp<CompPowerTrader>().PowerOn = true;
                    var fuel = turret.GetComp<CompRefuelable>();
                    fuel.ConsumeFuel(fuel.Fuel);
                    fuel.Refuel(0.05f); // menos de un disparo
                    Next(Phase.NoFuel);
                    break;
                case Phase.NoFuel:
                    Check(silent && turret.Active, "powered but with less stone than one shot: no shots (" + Fuel.ToString("0.##") + " left)");
                    turret.Destroy();
                    turret = SpawnTurret("CVG_QT_ContainmentTurret");
                    Next(Phase.Containment);
                    break;
            }
            Mark();
        }

        private static void EndContainment()
        {
            CheckDinfo("CVG_QT_Gun_ContainmentTurret", 3, turret);
            float used = fuelAtStart - Fuel;
            Check(shots > 0 && Mathf.Abs(used - shots * 0.4f) < 0.001f, $"containment cannon: {shots} shots used {used:0.###} stones (0.4 per shot)");

            // Guardar y cargar: combustible de la torreta y bonus de grado del rifle (Scribe del Core).
            shooter.equipment.Primary.GetComp<CompSpiritGrade>().bonus = 1;
            target.Destroy();
            target = null;
            savedTurretId = turret.ThingID;
            savedShooterId = shooter.ThingID;
            savedFuel = Fuel;
            Next(Phase.Saving);
            LongEventHandler.QueueLongEvent(() => GameDataSaveLoader.SaveGame(SaveName), "SavingLongEvent", false, null);
            LongEventHandler.QueueLongEvent(() =>
            {
                phase = Phase.AfterLoad;
                phaseTicks = 0;
                GameDataSaveLoader.LoadGame(SaveName);
            }, "LoadingLongEvent", false, null);
        }

        private static void AfterLoad()
        {
            map = Find.CurrentMap;
            var t = map.listerThings.ThingsOfDef(Def("CVG_QT_ContainmentTurret")).FirstOrDefault(x => x.ThingID == savedTurretId) as Building_TurretGun;
            Check(t != null, "after load: the turret is there");
            if (t != null)
            {
                Check(Mathf.Abs(t.GetComp<CompRefuelable>().Fuel - savedFuel) < 0.001f, $"after load: turret fuel kept ({savedFuel:0.##})");
                Check(SpiritGradeUtility.GradeOf(t.GunCompEq.parent) == 3, "after load: turret gun grade 3");
                t.Destroy();
            }
            var p = map.mapPawns.AllPawnsSpawned.FirstOrDefault(x => x.ThingID == savedShooterId);
            var gun = p?.equipment?.Primary;
            Check(gun != null && SpiritGradeUtility.GradeOf(gun) == 2, "after load: rifle grade 1 + saved bonus 1 = " + (gun == null ? "none" : SpiritGradeUtility.GradeOf(gun).ToString()));
            p?.Destroy();
            try { File.Delete(GenFilePaths.FilePathForSavedGame(SaveName)); } catch { }
            Finish();
        }

        private static void Finish()
        {
            phase = Phase.Done;
            foreach (var t in new Thing[] { turret, shooter, target })
                if (t != null && !t.Destroyed && t.Spawned) t.Destroy();
            sb.Append(fails == 0 ? "\n  ALL OK" : "\n  FAILURES: " + fails);
            if (fails == 0) Log.Message(sb.ToString()); else Log.Error(sb.ToString());
        }
    }
}
