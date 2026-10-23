using System.Linq;
using HarmonyLib;
using RimWorld;
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

        public override void FinalizeInit() => ApplyAll();

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
