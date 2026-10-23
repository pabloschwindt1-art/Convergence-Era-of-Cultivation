using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence.Factions
{
    /// <summary>
    /// En un <c>FactionDef</c> de secta: el ícono y el color de su ideología. Con <c>fixedIdeo</c>, el nombre, la descripción, el fundador
    /// y los preceptos ya vienen del FactionDef; lo que Ideology igual genera al azar lo pisa <see cref="GameComponent_SectIdeos"/>.
    /// </summary>
    public class DefModExtension_SectIdeo : DefModExtension
    {
        public IdeoIconDef icon;
        public ColorDef color;
        /// <summary>Íconos propios del líder y del Gran Anciano (rol moralista) de la secta.</summary>
        public string leaderIconPath;
        public string elderIconPath;

        private Texture2D leaderIcon, elderIcon;
        public Texture2D LeaderIcon => leaderIconPath == null ? null : leaderIcon ??= ContentFinder<Texture2D>.Get(leaderIconPath, false);
        public Texture2D ElderIcon => elderIconPath == null ? null : elderIcon ??= ContentFinder<Texture2D>.Get(elderIconPath, false);
    }

    /// <summary>
    /// El líder y el Gran Anciano de una secta muestran su propio ícono (vanilla usa el mismo para todas las ideologías). Solo en las
    /// ideologías de las facciones de secta, que tienen color neutro: el arte se ve con sus colores.
    /// </summary>
    [HarmonyPatch(typeof(Precept), nameof(Precept.Icon), MethodType.Getter)]
    public static class Patch_Precept_Icon_SectRoles
    {
        public static void Postfix(Precept __instance, ref Texture2D __result)
        {
            if (!(__instance is Precept_Role) || __instance.ideo == null) return;
            if (!GameComponent_SectIdeos.SectOf.TryGetValue(__instance.ideo, out var ext)) return;
            var tex = __instance.def == PreceptDefOf.IdeoRole_Leader ? ext.LeaderIcon
                : __instance.def == PreceptDefOf.IdeoRole_Moralist ? ext.ElderIcon : null;
            if (tex != null) __result = tex;
        }
    }

    /// <summary>Una secta recién creada (al generar el mundo o al llegar con la Convergencia) ya nace con su ideología propia.</summary>
    [HarmonyPatch(typeof(FactionGenerator), nameof(FactionGenerator.NewGeneratedFaction), new[] { typeof(RimWorld.Planet.PlanetLayer), typeof(FactionGeneratorParms) })]
    public static class Patch_FactionGenerator_NewGeneratedFaction
    {
        public static void Postfix(Faction __result)
        {
            if (__result?.def.HasModExtension<DefModExtension_SectIdeo>() != true) return;
            try { GameComponent_SectIdeos.Apply(__result); }
            catch (System.Exception e) { Log.Error("[Convergence] Sect ideoligion of " + __result.Name + ": " + e); }
        }
    }

    [StaticConstructorOnStartup]
    public static class FactionsHarmony
    {
        static FactionsHarmony() => new Harmony("prospero1088.convergence.factions").PatchAll();
    }

    /// <summary>
    /// La ideología de cada secta es la suya, sin nada vanilla al azar: título del líder = el de la facción (no "primer ministro"),
    /// nuestros roles con su nombre (no el que inventa el generador), ícono y color propios, y el nombre y la descripción del FactionDef
    /// (también en partidas creadas antes de este cambio). Idempotente: corre al cargar y cada tanto, por si llegan sectas nuevas.
    /// </summary>
    public class GameComponent_SectIdeos : GameComponent
    {
        // Precept.name es protegido: el juego no tiene un setter para renombrar un rol.
        private static readonly System.Reflection.FieldInfo NameField = AccessTools.Field(typeof(Precept), "name");
        private static readonly System.Reflection.FieldInfo LabelCacheField = AccessTools.Field(typeof(Precept), "labelCapCache");

        /// <summary>Preceptos vanilla que una secta conserva (el resto de lo vanilla se quita).</summary>
        public static readonly System.Collections.Generic.HashSet<string> Kept = new System.Collections.Generic.HashSet<string>
        {
            "IdeoRole_Leader", "IdeoRole_Moralist", "Funeral", "Festival", "ChildBirth", "LeaderSpeech", "RoleChange", "IdeoRitualSeat",
        };

        public GameComponent_SectIdeos(Game game) { }

        /// <summary>Ideología de cada facción de secta, para el parche de íconos de rol.</summary>
        public static readonly System.Collections.Generic.Dictionary<Ideo, DefModExtension_SectIdeo> SectOf = new System.Collections.Generic.Dictionary<Ideo, DefModExtension_SectIdeo>();

        private bool pending = true;

        public override void FinalizeInit()
        {
            SectOf.Clear();
            pending = true;
        }

        // Al iniciar la partida las ideologías de las facciones todavía no están listas: se aplica en el primer cuadro (también en
        // pausa) y después cada tanto, por si llegan sectas nuevas con la Convergencia.
        public override void GameComponentUpdate()
        {
            if (!pending || Current.ProgramState != ProgramState.Playing) return;
            pending = false;
            ApplyAll();
        }

        public override void GameComponentTick()
        {
            if (Find.TickManager.TicksGame % 2500 == 0) ApplyAll();
        }

        public static void ApplyAll()
        {
            if (!ModsConfig.IdeologyActive || Find.FactionManager == null) return;
            foreach (var faction in Find.FactionManager.AllFactionsListForReading)
            {
                if (!faction.def.HasModExtension<DefModExtension_SectIdeo>()) continue;
                try { Apply(faction); }
                catch (System.Exception e) { Log.ErrorOnce("[Convergence] Sect ideoligion of " + faction.Name + ": " + e, faction.loadID ^ 0x5EC7); }
            }
        }

        public static void Apply(Faction faction)
        {
            var ideo = faction.ideos?.PrimaryIdeo;
            if (ideo == null) return;
            var def = faction.def;
            var ext = def.GetModExtension<DefModExtension_SectIdeo>();
            SectOf[ideo] = ext;
            if (def.fixedIdeo && !def.ideoName.NullOrEmpty() && ideo.name != def.ideoName) ideo.name = def.ideoName;
            if (def.fixedIdeo && !def.ideoDescription.NullOrEmpty() && ideo.description != def.ideoDescription) ideo.description = def.ideoDescription;
            ideo.leaderTitleMale = def.leaderTitle;
            ideo.leaderTitleFemale = def.leaderTitleFemale.NullOrEmpty() ? def.leaderTitle : def.leaderTitleFemale;
            // Lo vanilla que toda ideología trae y no tiene sentido en una secta (árbol del ánima, gravinave, trono, juicios,
            // reliquias y altares con nombres inventados) se va; quedan funeral, fiesta, nacimiento, discurso y cambio de rol.
            foreach (var p in ideo.PreceptsListForReading.Where(p => !p.def.defName.StartsWith("CVG_") && !Kept.Contains(p.def.defName)).ToList())
                ideo.RemovePrecept(p);
            foreach (var role in ideo.PreceptsListForReading.OfType<Precept_Role>())
            {
                string want = role.def == PreceptDefOf.IdeoRole_Leader ? def.leaderTitle
                    : role.def == PreceptDefOf.IdeoRole_Moralist ? "CVG_Factions_GrandElder".Translate().ToString()
                    : role.def.defName.StartsWith("CVG_") ? role.def.label : null;
                if (want == null || role.Label == want) continue;
                NameField.SetValue(role, want);
                LabelCacheField?.SetValue(role, null);
                role.ClearTipCache();
            }
            if (ext.icon != null && (ideo.iconDef != ext.icon || (ext.color != null && ideo.colorDef != ext.color)))
                ideo.SetIcon(ext.icon, ext.color ?? ideo.colorDef);
        }
    }
}
