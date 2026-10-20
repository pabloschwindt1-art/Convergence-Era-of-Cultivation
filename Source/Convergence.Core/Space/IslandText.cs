using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    // Textos de isla (docs/modulos/ESPACIO.md): la isla es 100% wuxia, así que mientras el mapa actual tenga un Corazón de la Isla (o ningún motor
    // grav vanilla) los textos de gravnave de Odyssey se muestran con la versión de isla: piedras espirituales en vez de quimiocombustible, roca de nube
    // en vez de subestructura, Corazón de la Isla en vez de motor grav, etc. Las versiones están en Keyed con el prefijo CVG_IslandText_<claveVanilla>.
    // Mecanismo (sin Harmony): al entrar en contexto de isla se cambian esas entradas en el diccionario de textos del idioma activo
    // (keyedReplacements) por su versión de isla, y al salir se devuelven las originales. Antes era un prefijo a todas las sobrecargas
    // de Translate: funcionaba, pero corría en cada texto de la interfaz y era frágil con otros mods.

    public static class IslandText
    {
        public const string Prefix = "CVG_IslandText_";
        private static HashSet<string> keys;
        private static LoadedLanguage keysLanguage;
        private static int cachedFrame = -1;
        private static bool cachedContext;

        public static bool Has(string key)
        {
            var lang = LanguageDatabase.activeLanguage;
            if (keys == null || keysLanguage != lang)
            {
                keysLanguage = lang;
                keys = new HashSet<string>();
                if (lang != null)
                    foreach (var k in lang.keyedReplacements.Keys)
                        if (k.StartsWith(Prefix)) keys.Add(k.Substring(Prefix.Length));
            }
            return keys.Contains(key);
        }

        /// <summary>El mapa actual tiene un Corazón de la Isla, o no tiene ningún motor grav vanilla (lo único que se pueda construir es la isla).</summary>
        public static bool Context
        {
            get
            {
                int frame = Time.frameCount;
                if (frame == cachedFrame) return cachedContext;
                cachedFrame = frame;
                cachedContext = Compute();
                return cachedContext;
            }
        }

        public static void ResetCache() => cachedFrame = -1;

        private static bool Compute()
        {
            if (!ModsConfig.OdysseyActive || IslandUtility.Aliases.Count == 0) return false;
            var map = Current.ProgramState == ProgramState.Playing ? Find.CurrentMap : null;
            if (map == null) return false;
            foreach (var alias in IslandUtility.Aliases)
                if (map.listerThings.AnyThingWithDef(alias)) return true;
            foreach (var t in map.listerThings.ThingsOfDef(ThingDefOf.GravEngine))
                if (t.def == ThingDefOf.GravEngine) return false;
            return true;
        }
    }

    /// <summary>Cambia las entradas de texto de gravnave por las de isla en el idioma activo, y las devuelve.</summary>
    public static class IslandTextSwap
    {
        private static bool applied;
        private static LoadedLanguage appliedLanguage;
        private static readonly Dictionary<string, LoadedLanguage.KeyedReplacement> originals = new Dictionary<string, LoadedLanguage.KeyedReplacement>();

        public static void Set(bool on)
        {
            var lang = LanguageDatabase.activeLanguage;
            if (lang == null) return;
            if (applied && appliedLanguage != lang) Restore();   // cambió el idioma: se vuelve a aplicar sobre el nuevo
            if (on == applied) return;
            if (on) Apply(lang); else Restore();
        }

        private static void Apply(LoadedLanguage lang)
        {
            originals.Clear();
            foreach (var kv in lang.keyedReplacements.ToList())
            {
                if (!kv.Key.StartsWith(IslandText.Prefix)) continue;
                string key = kv.Key.Substring(IslandText.Prefix.Length);
                lang.keyedReplacements.TryGetValue(key, out var orig);
                originals[key] = orig;   // null = el idioma no la tenía (cae al inglés)
                lang.keyedReplacements[key] = new LoadedLanguage.KeyedReplacement
                {
                    key = key, value = kv.Value.value, fileSource = kv.Value.fileSource,
                    fileSourceLine = kv.Value.fileSourceLine, fileSourceFullPath = kv.Value.fileSourceFullPath
                };
            }
            applied = true;
            appliedLanguage = lang;
        }

        public static void Restore()
        {
            if (!applied || appliedLanguage == null) return;
            foreach (var kv in originals)
            {
                if (kv.Value != null) appliedLanguage.keyedReplacements[kv.Key] = kv.Value;
                else appliedLanguage.keyedReplacements.Remove(kv.Key);
            }
            originals.Clear();
            applied = false;
            appliedLanguage = null;
        }
    }
}

namespace Convergence
{
    /// <summary>Mira el contexto de isla un par de veces por segundo y cambia los textos cuando cambia (mapa actual, Corazón construido).</summary>
    public class GameComponent_IslandTextInstaller : GameComponent
    {
        public GameComponent_IslandTextInstaller(Game game) { }

        public override void FinalizeInit()
        {
            IslandText.ResetCache();
            IslandTextSwap.Set(IslandText.Context);
        }

        public override void GameComponentUpdate()
        {
            if (Time.frameCount % 30 != 0) return;
            IslandTextSwap.Set(IslandText.Context);
        }
    }
}
