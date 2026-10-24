using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Convergence.Formations
{
    /// <summary>
    /// Formación (阵法): un núcleo con combustible (piedras espirituales) y banderas colocadas alrededor. El área es el polígono convexo
    /// que forman las banderas cercanas; cada formación (<see cref="FormationWorker"/>) aplica su efecto dentro.
    /// </summary>
    public class CompProperties_FormationCore : CompProperties
    {
        public Type workerClass = typeof(FormationWorker);
        /// <summary>Defecto de la bandera que delimita el área.</summary>
        public ThingDef flagDef;
        public int minFlags = 3;
        public int maxFlags = 8;
        /// <summary>Distancia máxima núcleo → bandera.</summary>
        public float maxLinkRange = 15.9f;
        public int maxCells = 700;
        /// <summary>Fuerza base del efecto (se multiplica por el nivel: banderas sobre el mínimo).</summary>
        public float power = 1f;
        public int intervalTicks = 120;
        /// <summary>Combustible (piedras) que gasta cada acción del efecto.</summary>
        public float fuelPerEvent = 0.1f;
        public ThingDef projectile;
        public Color color = new Color(0.5f, 0.8f, 1f);

        public CompProperties_FormationCore()
        {
            compClass = typeof(CompFormationCore);
        }
    }

    public abstract class FormationWorker
    {
        public virtual void Tick(CompFormationCore core) { }
        /// <summary>Qi extra por celda dentro del área.</summary>
        public virtual float QiPerCell(CompFormationCore core) => 0f;
        /// <summary>Si es true, el núcleo gasta combustible mientras está activo (si no, solo al actuar).</summary>
        public virtual bool DrainsWhileActive => false;
        public virtual string Describe(CompFormationCore core) => null;

        protected static bool IsEnemy(Pawn p, CompFormationCore core) =>
            p != null && p.Spawned && !p.Dead && p.HostileTo(core.parent.Faction) && !p.IsPrisoner;
    }

    public class CompFormationCore : ThingComp
    {
        private HashSet<IntVec3> cells = new HashSet<IntVec3>();
        private List<IntVec3> cellList = new List<IntVec3>();
        private int flagCount;
        private bool tooBig;
        private FormationWorker worker;
        private CompRefuelable refuelable;

        public CompProperties_FormationCore Props => (CompProperties_FormationCore)props;
        public FormationWorker Worker => worker ??= (FormationWorker)Activator.CreateInstance(Props.workerClass);
        public CompRefuelable Refuelable => refuelable ??= parent.GetComp<CompRefuelable>();
        public IReadOnlyCollection<IntVec3> Cells => cells;
        public List<IntVec3> CellList => cellList;
        public int FlagCount => flagCount;
        public bool Contains(IntVec3 c) => cells.Contains(c);

        public bool HasFuel => Refuelable == null || Refuelable.HasFuel;
        public bool Active => parent.Spawned && parent.Faction == Faction.OfPlayer && cellList.Count > 0 && !tooBig && HasFuel;

        /// <summary>Nivel (1–3): una pareja de banderas sobre el mínimo sube un nivel.</summary>
        public int Level => Mathf.Clamp(1 + (flagCount - Props.minFlags) / 2, 1, 3);
        public float Power => Props.power * Level;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            parent.Map.GetComponent<FormationMapComponent>()?.Register(this);
            Recalculate();
        }

        public override void PostDeSpawn(Map map, DestroyMode mode)
        {
            map.GetComponent<FormationMapComponent>()?.Unregister(this);
        }

        public void ConsumeFuel(float amount) => Refuelable?.ConsumeFuel(amount);

        public override void CompTick()
        {
            if (!parent.Spawned) return;
            int t = Find.TickManager.TicksGame;
            if ((t + parent.thingIDNumber) % 250 == 0) Recalculate();
            if (!Active) return;
            if (Worker.DrainsWhileActive) Refuelable?.Notify_UsedThisTick();
            if ((t + parent.thingIDNumber) % Mathf.Max(1, Props.intervalTicks) == 0) Worker.Tick(this);
        }

        public void Recalculate()
        {
            cells.Clear();
            cellList.Clear();
            tooBig = false;
            var map = parent.Map;
            if (map == null || Props.flagDef == null) { flagCount = 0; return; }

            var flags = map.listerThings.ThingsOfDef(Props.flagDef)
                .Where(f => f.Faction == parent.Faction && f.Position.DistanceTo(parent.Position) <= Props.maxLinkRange)
                .OrderBy(f => f.Position.DistanceToSquared(parent.Position)).Take(Props.maxFlags).ToList();
            flagCount = flags.Count;
            if (flagCount < Props.minFlags) return;

            var hull = ConvexHull(flags.Select(f => f.Position).ToList());
            if (hull.Count < 3) return;

            int minX = hull.Min(p => p.x), maxX = hull.Max(p => p.x), minZ = hull.Min(p => p.z), maxZ = hull.Max(p => p.z);
            if ((maxX - minX + 1) * (maxZ - minZ + 1) > Props.maxCells * 3) { tooBig = true; return; }
            for (int x = minX; x <= maxX; x++)
                for (int z = minZ; z <= maxZ; z++)
                {
                    var c = new IntVec3(x, 0, z);
                    if (c.InBounds(map) && InsideConvex(hull, c)) { cells.Add(c); cellList.Add(c); }
                }
            if (cellList.Count > Props.maxCells) { cells.Clear(); cellList.Clear(); tooBig = true; }
        }

        // Cadena monótona de Andrew
        private static List<IntVec3> ConvexHull(List<IntVec3> pts)
        {
            pts = pts.Distinct().OrderBy(p => p.x).ThenBy(p => p.z).ToList();
            if (pts.Count < 3) return pts;
            long Cross(IntVec3 o, IntVec3 a, IntVec3 b) => (long)(a.x - o.x) * (b.z - o.z) - (long)(a.z - o.z) * (b.x - o.x);
            var h = new List<IntVec3>();
            foreach (var p in pts)
            {
                while (h.Count >= 2 && Cross(h[h.Count - 2], h[h.Count - 1], p) <= 0) h.RemoveAt(h.Count - 1);
                h.Add(p);
            }
            int lower = h.Count + 1;
            for (int i = pts.Count - 2; i >= 0; i--)
            {
                var p = pts[i];
                while (h.Count >= lower && Cross(h[h.Count - 2], h[h.Count - 1], p) <= 0) h.RemoveAt(h.Count - 1);
                h.Add(p);
            }
            h.RemoveAt(h.Count - 1);
            return h;
        }

        private static bool InsideConvex(List<IntVec3> hull, IntVec3 c)
        {
            for (int i = 0; i < hull.Count; i++)
            {
                var a = hull[i];
                var b = hull[(i + 1) % hull.Count];
                long cross = (long)(b.x - a.x) * (c.z - a.z) - (long)(b.z - a.z) * (c.x - a.x);
                if (cross < 0) return false; // casco en sentido antihorario
            }
            return true;
        }

        public override string CompInspectStringExtra()
        {
            string s = flagCount < Props.minFlags
                ? "CVG_Formation_NeedFlags".Translate(flagCount, Props.minFlags).ToString()
                : tooBig
                    ? "CVG_Formation_TooBig".Translate().ToString()
                    : "CVG_Formation_Status".Translate(Active ? "CVG_Formation_On".Translate() : "CVG_Formation_Off".Translate(), cellList.Count, Level).ToString();
            string extra = Worker.Describe(this);
            return extra.NullOrEmpty() ? s : s + "\n" + extra;
        }

        public override void PostDrawExtraSelectionOverlays()
        {
            if (cellList.Count > 0) GenDraw.DrawFieldEdges(cellList, Props.color);
            var map = parent.Map;
            if (map == null || Props.flagDef == null) return;
            foreach (var f in map.listerThings.ThingsOfDef(Props.flagDef))
                if (f.Position.DistanceTo(parent.Position) <= Props.maxLinkRange)
                    GenDraw.DrawLineBetween(parent.TrueCenter(), f.TrueCenter(), SimpleColor.White);
        }
    }

    public class FormationMapComponent : MapComponent
    {
        private readonly List<CompFormationCore> cores = new List<CompFormationCore>();
        private readonly List<CompFormationCore> protective = new List<CompFormationCore>();
        private static Map cachedMap;
        private static FormationMapComponent cachedComp;

        public FormationMapComponent(Map map) : base(map) { }
        public List<CompFormationCore> Cores => cores;
        /// <summary>Solo los núcleos protectores: el parche de proyectiles corre en cada tick de cada proyectil y no recorre el resto.</summary>
        public List<CompFormationCore> Protective => protective;

        public void Register(CompFormationCore c)
        {
            if (!cores.Contains(c)) cores.Add(c);
            if (c.Worker is FormationWorker_Protective && !protective.Contains(c)) protective.Add(c);
        }

        public void Unregister(CompFormationCore c)
        {
            cores.Remove(c);
            protective.Remove(c);
        }

        /// <summary>El componente del mapa, con caché del último mapa (GetComponent recorre la lista en cada llamada).</summary>
        public static FormationMapComponent For(Map map)
        {
            if (map == null) return null;
            if (map != cachedMap || cachedComp?.map != map)
            {
                cachedMap = map;
                cachedComp = map.GetComponent<FormationMapComponent>();
            }
            return cachedComp;
        }
    }

    // ------------------------------------------------------------------ efectos

    /// <summary>Gran Formación Protectora: cúpula que absorbe los proyectiles enemigos; cada golpe gasta piedras.</summary>
    public class FormationWorker_Protective : FormationWorker
    {
        public override bool DrainsWhileActive => true;

        public override string Describe(CompFormationCore core) => "CVG_Formation_ProtectiveDesc".Translate(core.Power.ToString("0")).ToString();

        /// <summary>Devuelve true si el proyectil queda absorbido.</summary>
        public static bool TryAbsorb(Projectile p)
        {
            var map = p.Map;
            if (map == null) return false;
            var comp = FormationMapComponent.For(map);
            if (comp == null || comp.Protective.Count == 0) return false;
            var cell = p.ExactPosition.ToIntVec3();
            foreach (var core in comp.Protective)
            {
                if (!core.Active || !core.Contains(cell)) continue;
                var launcher = p.Launcher;
                if (launcher == null || launcher.Faction == null || !launcher.Faction.HostileTo(core.parent.Faction)) continue;
                float cost = p.DamageAmount / Mathf.Max(1f, core.Power);
                var fuel = core.Refuelable;
                if (fuel != null && fuel.Fuel < cost) continue;
                core.ConsumeFuel(cost);
                FleckMaker.Static(p.ExactPosition, map, FleckDefOf.ExplosionFlash, 1.2f);
                FleckMaker.ThrowLightningGlow(p.ExactPosition, map, 0.8f);
                SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(cell, map));
                p.Destroy();
                return true;
            }
            return false;
        }
    }

    /// <summary>Formación de Espadas Asesinas: lanza espadas de qi a los enemigos dentro del área.</summary>
    public class FormationWorker_Swords : FormationWorker
    {
        public override string Describe(CompFormationCore core) => "CVG_Formation_SwordsDesc".Translate(Mathf.RoundToInt(core.Power)).ToString();

        public override void Tick(CompFormationCore core)
        {
            var map = core.parent.Map;
            var proj = core.Props.projectile;
            if (proj == null) return;
            var targets = map.mapPawns.AllPawnsSpawned.Where(p => IsEnemy(p, core) && !p.Downed && core.Contains(p.Position)).ToList();
            if (targets.Count == 0) return;
            int swords = Mathf.RoundToInt(core.Power);
            for (int i = 0; i < swords; i++)
            {
                var target = targets.RandomElement();
                var sword = (Projectile)GenSpawn.Spawn(proj, core.parent.Position, map);
                sword.Launch(core.parent, target, target, ProjectileHitFlags.IntendedTarget);
                core.ConsumeFuel(core.Props.fuelPerEvent);
                if (!core.HasFuel) break;
            }
        }
    }

    /// <summary>Formación de Atadura: inmoviliza (aturde) al enemigo más fuerte del área unos segundos.</summary>
    public class FormationWorker_Binding : FormationWorker
    {
        public override string Describe(CompFormationCore core) => "CVG_Formation_BindingDesc".Translate((core.Power * 5f).ToString("0")).ToString();

        public override void Tick(CompFormationCore core)
        {
            var map = core.parent.Map;
            var best = map.mapPawns.AllPawnsSpawned
                .Where(p => IsEnemy(p, core) && !p.Downed && core.Contains(p.Position) && !p.stances.stunner.Stunned)
                .OrderByDescending(p => (CompCultivator.For(p)?.RealmLevel ?? 0) * 1000f + p.kindDef.combatPower)
                .FirstOrDefault();
            if (best == null) return;
            best.stances.stunner.StunFor(Mathf.RoundToInt(core.Power * 300f), core.parent, addBattleLog: false, showMote: true);
            core.ConsumeFuel(core.Props.fuelPerEvent);
            FleckMaker.ThrowLightningGlow(best.DrawPos, map, 1.2f);
            SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(best.Position, map));
        }
    }

    /// <summary>Formación de Reunión de Qi: sube la concentración de qi en toda el área.</summary>
    public class FormationWorker_QiGather : FormationWorker
    {
        public override bool DrainsWhileActive => true;
        public override float QiPerCell(CompFormationCore core) => core.Power;
        public override string Describe(CompFormationCore core) => "CVG_Formation_QiDesc".Translate(core.Power.ToString("0.0")).ToString();

        public override void Tick(CompFormationCore core)
        {
            // un destello de qi ocasional dentro del área
            if (core.CellList.Count == 0) return;
            FleckMaker.ThrowMicroSparks(core.CellList.RandomElement().ToVector3Shifted(), core.parent.Map);
        }
    }

    [StaticConstructorOnStartup]
    public static class FormationsStartup
    {
        static FormationsStartup()
        {
            new Harmony("prospero1088.convergence.formations").PatchAll();
            // Qi extra de las formaciones de reunión (hook genérico del Core)
            QiConcentrationUtility.AreaProviders.Add((cell, map) =>
            {
                var comp = map.GetComponent<FormationMapComponent>();
                if (comp == null || comp.Cores.Count == 0) return 0f;
                float total = 0f;
                foreach (var core in comp.Cores)
                    if (core.Active && core.Contains(cell)) total += core.Worker.QiPerCell(core);
                return total;
            });
            Log.Message("[Convergence] Formations loaded.");
        }
    }

    [HarmonyPatch(typeof(Projectile), "Tick")]
    public static class Patch_Projectile_Tick
    {
        public static bool Prefix(Projectile __instance) => !FormationWorker_Protective.TryAbsorb(__instance);
    }
}

namespace Convergence.Formations
{
    /// <summary>
    /// Al colocar un núcleo o una bandera: dibuja el alcance núcleo-bandera y líneas a lo que quedaría enlazado, para que se
    /// entienda sin leer la descripción que el área es el polígono que encierran las banderas.
    /// </summary>
    public class PlaceWorker_FormationRange : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            var map = Find.CurrentMap;
            if (map == null) return;
            var props = def.GetCompProperties<CompProperties_FormationCore>();
            if (props != null)
            {
                GenDraw.DrawRadiusRing(center, props.maxLinkRange);
                if (props.flagDef != null)
                    foreach (var f in map.listerThings.ThingsOfDef(props.flagDef).Concat(map.listerThings.ThingsOfDef(props.flagDef.blueprintDef ?? props.flagDef)))
                        if (f.Position.DistanceTo(center) <= props.maxLinkRange)
                            GenDraw.DrawLineBetween(center.ToVector3Shifted(), f.TrueCenter(), SimpleColor.White);
                return;
            }
            // Bandera: líneas a los núcleos cuyo alcance la incluiría.
            foreach (var b in map.listerBuildings.allBuildingsColonist)
            {
                var cp = b.def.GetCompProperties<CompProperties_FormationCore>();
                if (cp == null || cp.flagDef != def) continue;
                if (b.Position.DistanceTo(center) <= cp.maxLinkRange)
                {
                    GenDraw.DrawLineBetween(center.ToVector3Shifted(), b.TrueCenter(), SimpleColor.Cyan);
                    GenDraw.DrawRadiusRing(b.Position, cp.maxLinkRange);
                }
            }
        }
    }
}
