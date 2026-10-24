using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Estado global de la partida: si ya ocurrió la Convergencia, cuándo, y cuándo está programada.
    /// Más adelante: el calendario celestial.
    /// </summary>
    public class GameComponent_Convergence : GameComponent
    {
        private const int CheckInterval = 250;

        private bool occurred;
        private int occurredTick = -1;
        private int scheduledTick = -1;
        private int guideTick = -1;
        private bool guideSent;
        private CultivationDifficultyValues difficulty;

        private static Game cachedGame;
        private static GameComponent_Convergence cached;

        /// <summary>Con caché por partida: <see cref="Active"/> se consulta en rutas calientes (stats, incidentes, comps) y
        /// GetComponent recorre la lista de componentes en cada llamada.</summary>
        public static GameComponent_Convergence Instance
        {
            get
            {
                var game = Current.Game;
                // Se recalcula mientras dé null: al cargar, la partida existe un momento antes que sus componentes.
                if (game != cachedGame || cached == null)
                {
                    cachedGame = game;
                    cached = game?.GetComponent<GameComponent_Convergence>();
                }
                return cached;
            }
        }

        /// <summary>True cuando la Era del Cultivo ya comenzó en esta partida.</summary>
        public static bool Active => Instance?.occurred ?? false;

        public bool Occurred => occurred;
        public int OccurredTick => occurredTick;
        public int ScheduledTick => scheduledTick;

        /// <summary>Dificultad de cultivo de esta partida (la de los ajustes al empezarla).</summary>
        public CultivationDifficultyValues Difficulty => difficulty ??= ConvergenceMod.Settings.difficulty.Copy();

        /// <summary>true en partidas nuevas: las sectas vienen con el mundo (elegidas al crearlo) y se respeta esa elección.</summary>
        private bool factionsFromWorldGen;

        public GameComponent_Convergence(Game game)
        {
        }

        public override void StartedNewGame()
        {
            difficulty = ConvergenceMod.Settings.difficulty.Copy();
            // Las facciones de la Convergencia (sectas) ya se eligieron al crear el mundo: no se agregan después. La partida rápida
            // (-quicktest) no pasa por la pantalla del mundo (solo crea las facciones obligatorias): ahí siguen llegando con la Convergencia.
            factionsFromWorldGen = !GenCommandLine.CommandLineArgPassed("quicktest");
            ConvergenceFactionUtility.ApplyRelationsToExisting();
            EnsureScheduled();
        }

        public override void LoadedGame()
        {
            EnsureScheduled();
            // Partidas anteriores a que las sectas fueran facciones del mundo: que lleguen cuando la Convergencia ya pasó.
            if (!factionsFromWorldGen) ConvergenceFactionUtility.EnsureFactions();
        }

        /// <summary>
        /// La Convergencia la desata el jugador al aceptar la misión del sabio (<see cref="QuestPart_MentorRefuge"/>), como el
        /// monolito de Anomaly. Si nunca la acepta, llega igual: por defecto al empezar el segundo año (ajustes del mod).
        /// </summary>
        private void EnsureScheduled()
        {
            if (occurred || scheduledTick >= 0) return;
            scheduledTick = Find.TickManager.TicksGame + ConvergenceMod.Settings.convergenceForcedDay * GenDate.TicksPerDay;
        }

        public override void GameComponentTick()
        {
            if (guideTick >= 0 && Find.TickManager.TicksGame >= guideTick)
            {
                guideTick = -1;
                guideSent = true;
                CultivationGuide.SendLetter();
            }
            if (occurred || scheduledTick < 0) return;
            int now = Find.TickManager.TicksGame;
            if (now % CheckInterval != 0 || now < scheduledTick) return;

            var map = Find.AnyPlayerHomeMap;
            if (map == null) return; // reintenta cuando haya un mapa del jugador
            TryFire(map);
        }

        /// <summary>Dispara la Convergencia en el mapa dado (evento, investigación o acción de debug).</summary>
        public bool TryFire(Map map)
        {
            if (occurred) return false;
            var parms = StorytellerUtility.DefaultParmsNow(CVG_DefOf.CVG_Convergence.category, map);
            parms.forced = true;
            return CVG_DefOf.CVG_Convergence.Worker.TryExecute(parms);
        }

        public void MarkOccurred()
        {
            if (occurred) return;
            occurred = true;
            occurredTick = Find.TickManager.TicksGame;
            scheduledTick = -1;
            guideTick = occurredTick + GenDate.TicksPerHour * 3;
            if (!factionsFromWorldGen) ConvergenceFactionUtility.EnsureFactions();
            // El reino y la vida se cachean; antes de la Convergencia valían como mortal (cultivo latente): se recalculan.
            foreach (var p in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead)
                CompCultivator.For(p)?.InvalidateHealthScale();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref occurred, "occurred");
            Scribe_Values.Look(ref factionsFromWorldGen, "factionsFromWorldGen");
            Scribe_Values.Look(ref occurredTick, "occurredTick", -1);
            Scribe_Values.Look(ref scheduledTick, "scheduledTick", -1);
            Scribe_Values.Look(ref guideTick, "guideTick", -1);
            Scribe_Values.Look(ref guideSent, "guideSent");
            // Partidas de antes de la guía: si la Convergencia ya ocurrió, la carta llega enseguida.
            if (Scribe.mode == LoadSaveMode.PostLoadInit && occurred && !guideSent && guideTick < 0)
                guideTick = Find.TickManager.TicksGame + GenDate.TicksPerHour / 2;
            Scribe_Deep.Look(ref difficulty, "difficulty");
        }
    }
}
