using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// La Convergencia: un fragmento de Jiuxiao cruza el cielo, el qi empieza a fluir y despiertan
    /// las raíces espirituales y los cuerpos especiales. Desbloquea el resto del mod.
    /// </summary>
    public class IncidentWorker_Convergence : IncidentWorker
    {
        private const int LightningStrikes = 6;

        protected override bool CanFireNowSub(IncidentParms parms) =>
            !GameComponent_Convergence.Active && parms.target is Map;

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var map = (Map)parms.target;
            var state = GameComponent_Convergence.Instance;
            if (state == null || state.Occurred) return false;

            state.MarkOccurred();
            ConvergenceUtility.WakeMap(map);

            // Espectáculo con herramientas vanilla: aurora y rayos (sin shaders propios, seguro para 1.7).
            var aurora = DefDatabase<GameConditionDef>.GetNamedSilentFail("Aurora");
            if (aurora != null)
                map.gameConditionManager.RegisterCondition(GameConditionMaker.MakeCondition(aurora, GenDate.TicksPerDay));
            for (int i = 0; i < LightningStrikes; i++)
            {
                if (CellFinderLoose.TryGetRandomCellWith(c => c.Standable(map) && !c.Roofed(map), map, 100, out var cell))
                    map.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrike(map, cell));
            }

            int beasts = ConvergenceUtility.LastBeasts;

            var awakened = map.mapPawns.FreeColonistsSpawned
                .Select(p => (pawn: p, comp: CompCultivator.For(p)))
                .Where(x => x.comp != null && (x.comp.HasRoot || x.comp.SpecialBody != null || BloodlineUtility.Get(x.pawn) != null))
                .ToList();

            var text = new StringBuilder("CVG_Convergence_LetterText".Translate().Resolve());
            text.AppendLine().AppendLine();
            if (awakened.Count == 0)
            {
                text.Append("CVG_Convergence_NoneAwakened".Translate().Resolve());
            }
            else
            {
                text.AppendLine("CVG_Convergence_Awakened".Translate().Resolve());
                foreach (var (pawn, comp) in awakened)
                    text.AppendLine("  - " + CultivationText.Summary(pawn, comp));
            }

            if (beasts > 0)
            {
                text.AppendLine();
                text.AppendLine("CVG_Convergence_BeastsAwakened".Translate(beasts).Resolve());
            }

            SoundCues.OnCamera("CVG_Cue_Convergence");
            SendStandardLetter("CVG_Convergence_LetterLabel".Translate(), text.ToString().TrimEnd(),
                LetterDefOf.PositiveEvent, parms, awakened.Select(x => x.pawn).ToList());
            return true;
        }
    }

    /// <summary>
    /// Lo que la Convergencia le hace a un mapa: linajes, vetas de piedra y jade espiritual, bestias y vetas de qi (el Maestro
    /// llega por su misión).
    /// La usan el incidente y los escenarios que empiezan con la Convergencia ya ocurrida (antes solo marcaban la bandera y el
    /// mapa quedaba sin piedras espirituales ni Maestro).
    /// </summary>
    public static class ConvergenceUtility
    {
        public static int LastBeasts;

        public static void WakeMap(Map map)
        {
            if (map == null) return;
            // Los linajes de sangre despiertan con el qi: hediff de efectos y capacidades.
            foreach (var p in map.mapPawns.AllPawnsSpawned.ToList()) BloodlineUtility.SyncEffects(p);
            // Las vetas despiertan: parte del jade se vuelve jade espiritual y brotan vetas de piedra espiritual.
            ConvergenceMineables.OnConvergence(map);
            // Los animales pueden despertar como bestias espirituales (M4b).
            LastBeasts = SpiritBeastUtility.AwakenAnimalsOnMap(map);
            QiNetworkUtility.EnsureVeins(map);
        }
    }

    /// <summary>
    /// Al completar el proyecto, dispara un incidente (forzado) en el mapa del jugador: p. ej. "reinos secretos" abre una puerta. El incidente
    /// va por nombre porque puede ser de un módulo (Stories); si ese módulo no está, no pasa nada.
    /// </summary>
    public class ResearchMod_FireIncident : ResearchMod
    {
        public string incident;

        public override void Apply()
        {
            var def = DefDatabase<IncidentDef>.GetNamedSilentFail(incident ?? "");
            var map = Find.AnyPlayerHomeMap;
            if (def == null || map == null || Current.ProgramState != ProgramState.Playing) return;
            var parms = StorytellerUtility.DefaultParmsNow(def.category, map);
            parms.forced = true;
            if (def.Worker.CanFireNow(parms)) def.Worker.TryExecute(parms);
        }
    }
}
