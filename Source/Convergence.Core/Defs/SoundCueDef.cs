using System.Linq;
using Verse;
using Verse.Sound;

namespace Convergence
{
    /// <summary>
    /// Un "aviso sonoro" del mod (Convergencia, avance, tribulación…). Apunta a un <c>SoundDef</c>: hoy uno vanilla, mañana uno propio
    /// cambiando solo el XML. Si el SoundDef falta, no suena nada.
    /// </summary>
    public class SoundCueDef : Def
    {
        public SoundDef sound;
    }

    public static class SoundCues
    {
        private static SoundDef Sound(string cue) =>
            DefDatabase<SoundCueDef>.GetNamedSilentFail(cue)?.sound;

        /// <summary>Suena en la cámara (avisos globales).</summary>
        public static void OnCamera(string cue)
        {
            var s = Sound(cue);
            if (s == null) return;
            // Algunos sonidos vanilla (p. ej. Psycast_Skip_Entry) solo tienen subsonidos "en el mapa": tocarlos en la cámara da error.
            if (s.subSounds.Any(sub => sub.onCamera)) s.PlayOneShotOnCamera();
            else if (Find.CurrentMap != null) s.PlayOneShot(new TargetInfo(Find.CameraDriver.MapPosition, Find.CurrentMap));
        }

        /// <summary>Suena en la posición del pawn si está en el mapa; si no, no suena.</summary>
        public static void At(string cue, Thing thing)
        {
            var s = Sound(cue);
            if (s != null && thing != null && thing.Spawned) s.PlayOneShot(new TargetInfo(thing.Position, thing.Map));
        }

        /// <summary>Solo para pawns del jugador (los demás no molestan).</summary>
        public static void ForPlayerPawn(string cue, Pawn pawn)
        {
            if (pawn?.Faction == RimWorld.Faction.OfPlayer) At(cue, pawn);
        }
    }
}
