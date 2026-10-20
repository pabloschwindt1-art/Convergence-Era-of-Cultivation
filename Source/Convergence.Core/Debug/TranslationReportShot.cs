using System.Linq;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Herramienta de revisión: con <c>-cvgtransreport</c> escribe en el log los errores de traducción del idioma activo (los que
    /// cuenta el aviso vanilla "Translation data … has N errors": errores de carga y de cada paquete de DefInjected). El informe
    /// vanilla se guarda en el escritorio del usuario, así que no se usa. Sin el argumento no hace nada.
    /// </summary>
    public static class TranslationReportShot
    {
        private static bool done;

        public static void Update()
        {
            if (done || Current.ProgramState != ProgramState.Playing) return;
            done = true;
            if (!GenCommandLine.CommandLineArgPassed("cvgtransreport")) return;
            var lang = LanguageDatabase.activeLanguage;
            var errors = lang.loadErrors.Concat(lang.defInjections.SelectMany(p => p.loadErrors)).ToList();
            foreach (var e in errors) Log.Warning("[Convergence] Translation: " + e);
            Log.Message($"[Convergence] TranslationReport: {errors.Count} error(s) in {lang.FriendlyNameEnglish}\n  ALL OK");
        }
    }
}
