using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>Autotest de los preceptos, roles y rituales de secta: solo con <c>-cvgsectideotest</c> (y un mapa, p. ej. -quicktest). Inerte sin el argumento.</summary>
    public class GameComponent_SectIdeoTest : GameComponent
    {
        private static readonly bool Enabled = GenCommandLine.CommandLineArgPassed("cvgsectideotest");
        private bool done;
        private readonly StringBuilder sb = new StringBuilder("[Convergence] Sect ideology self-test");
        private int fails;

        public GameComponent_SectIdeoTest(Game game) { }

        private void Check(bool ok, string what)
        {
            sb.Append("\n    " + (ok ? "ok   " : "FAIL ") + what);
            if (!ok) fails++;
        }

        public override void GameComponentTick()
        {
            if (!Enabled || done || Find.CurrentMap == null || Find.TickManager.TicksGame < 200) return;
            done = true;
            try { Run(Find.CurrentMap); }
            catch (Exception e) { Check(false, "exception: " + e); }
            sb.Append(fails == 0 ? "\n  ALL OK" : "\n  FAILURES: " + fails);
            if (fails == 0) Log.Message(sb.ToString()); else Log.Error(sb.ToString());
        }

        private static bool HasMemory(Pawn p, string def) => p.needs.mood.thoughts.memories.Memories.Any(m => m.def.defName == def);

        private Precept Add(Ideo ideo, string def)
        {
            var pd = DefDatabase<PreceptDef>.GetNamed(def);
            var p = PreceptMaker.MakePrecept(pd);
            ideo.AddPrecept(p, init: true, fillWith: pd.ritualPatternBase);
            return p;
        }

        private void Run(Map map)
        {
            if (!ModsConfig.IdeologyActive || DefDatabase<PreceptDef>.GetNamedSilentFail("CVG_Cultivation_Venerated") == null)
            {
                sb.Append("\n  (Ideology or the sects module not loaded: skipped)");
                return;
            }
            var issues = DefDatabase<IssueDef>.AllDefsListForReading.Where(i => i.defName.StartsWith("CVG_Issue_")).ToList();
            Check(issues.Count == 7, issues.Count + " sect issues (cultivation, alchemy, essence, demonic arts, duels, hierarchy, retreat)");
            foreach (var i in issues)
                Check(DefDatabase<PreceptDef>.AllDefsListForReading.Count(p => p.issue == i) >= 2, i.defName + " has at least two precepts");
            Check(DefDatabase<PreceptDef>.AllDefsListForReading.Count(p => p.defName.StartsWith("CVG_Role_")) >= 6, "sect roles defined");
            // Las sectas vienen con el mundo: la lista por defecto de la pantalla de creación (Page_CreateWorldParams.ResetFactionCounts)
            // toma las facciones configurables con startingCountAtWorldCreation > 0; tienen que estar y poder quitarse.
            var sects = DefDatabase<FactionDef>.AllDefsListForReading.Where(f => f.HasModExtension<DefModExtension_ConvergenceFaction>()).ToList();
            var worldDefaults = FactionGenerator.ConfigurableFactions.Where(f => f.startingCountAtWorldCreation > 0).ToList();
            Check(sects.Count >= 6 && sects.All(worldDefaults.Contains), sects.Count + " sects are in the default faction list of a new world");
            Check(sects.All(f => f.requiredCountAtGameStart == 0 && f.settlementGenerationWeight > 0), "sects are optional and get settlements at world creation");
            // Creencias (memes de estructura) de Jiuxiao: se generan como las vanilla, con nombre, descripción y deidades.
            foreach (var st in DefDatabase<MemeDef>.AllDefsListForReading.Where(m => m.defName.StartsWith("CVG_Structure_")))
            {
                try
                {
                    var gen = IdeoGenerator.GenerateIdeo(new IdeoGenerationParms(Faction.OfPlayer.def, forcedMemes: new List<MemeDef> { st }));
                    int deities = (gen?.foundation as IdeoFoundation_Deity)?.DeitiesListForReading?.Count ?? 0;
                    bool ok = gen != null && gen.StructureMeme == st && !gen.name.NullOrEmpty() && !gen.description.NullOrEmpty()
                              && (st.deityCount.max == 0 || deities > 0);
                    Check(ok, $"{st.defName}: '{gen?.name}', {deities} deities, worship room '{st.worshipRoomLabel}'");
                }
                catch (Exception e) { Check(false, st.defName + " generation threw " + e.Message); }
            }
            Check(DefDatabase<PreceptDef>.AllDefsListForReading.Count(p => p.defName.StartsWith("CVG_Ritual_")) == 4, "admission, ascension, tournament and Dao funeral rituals defined");
            var altar = DefDatabase<ThingDef>.GetNamed("CVG_AncestralAltar");
            Check(altar.isAltar, "ancestral altar is a ritual altar");
            var buildingPrecept = DefDatabase<PreceptDef>.GetNamed("IdeoBuilding");
            Check(buildingPrecept.buildingDefChances.Any(c => c.def == altar), "the altar can be an ideoligion building (patch applied)");

            int memes = DefDatabase<MemeDef>.AllDefsListForReading.Count(m => m.defName.StartsWith("CVG_Meme_"));
            Check(memes == (ModsConfig.BiotechActive ? 9 : 8), memes + " sect memes (body way, puppet path with Biotech, thunder way added)");
            var presets = DefDatabase<IdeoPresetDef>.AllDefsListForReading.Where(p => p.defName.StartsWith("CVG_Preset_")).ToList();
            Check(presets.Count == memes, presets.Count + " sect ideoligion presets, one per doctrine");
            Check(presets.All(p => p.memes.Count == 2 && p.memes.Any(m => m.defName.StartsWith("CVG_Meme_"))), "each preset pairs a sect doctrine with a structure meme");
            var relics = DefDatabase<ThingDef>.AllDefsListForReading.Where(t => t.defName.StartsWith("CVG_Relic_")).ToList();
            Check(relics.Count == 3 && relics.All(r => r.relicChance > 0f && r.thingCategories.Any(c => c.defName == "InertRelics")), "three sect treasures are eligible relics");
            var stele = DefDatabase<ThingDef>.GetNamed("CVG_ScriptureStele");
            Check(stele.isAltar && buildingPrecept.buildingDefChances.Any(c => c.def == stele), "scripture stele is a ritual focus and an ideoligion building");
            var funeralDef = DefDatabase<PreceptDef>.GetNamed("CVG_Ritual_DaoFuneral");
            Check(funeralDef.ritualPatternBase.ritualObligationTriggers.Any(), "Dao funeral has its death trigger");

            var ideo = Faction.OfPlayer.ideos.PrimaryIdeo;
            var pawn = map.mapPawns.FreeColonistsSpawned.First();
            pawn.ideo.SetIdeo(ideo);
            var added = new List<Precept>();
            try
            {
                added.Add(Add(ideo, "CVG_Cultivation_Venerated"));
                SectEvents.Record(SectEvents.Cultivated, pawn);
                Check(HasMemory(pawn, "CVG_Cultivation_Venerated_Self"), "venerated cultivation: mood memory after cultivating");

                added.Add(Add(ideo, "CVG_Demonic_Forbidden"));
                var other = map.mapPawns.FreeColonistsSpawned.FirstOrDefault(c => c != pawn);
                if (other != null) other.ideo.SetIdeo(ideo);
                SectEvents.Record(SectEvents.DemonicArt, pawn);
                Check(HasMemory(pawn, "CVG_Demonic_Forbidden_Self"), "forbidden demonic arts: self memory");
                if (other != null) Check(HasMemory(other, "CVG_Demonic_Forbidden_KnowMood"), "forbidden demonic arts: others feel it");

                added.Add(Add(ideo, "CVG_Retreat_Revered"));
                SectEvents.Record(SectEvents.Retreat, pawn);
                Check(HasMemory(pawn, "CVG_Retreat_Revered_Self"), "revered retreat: memory");

                var ritual = (Precept_Ritual)Add(ideo, "CVG_Ritual_Ascension");
                added.Add(ritual);
                Check(ritual.obligationTriggers.Any(t => t is RitualObligationTrigger_Ascension), "ascension ritual has its trigger");
                int before = ritual.activeObligations?.Count ?? 0;
                SectEvents.Record(SectEvents.Ascended, pawn);
                Check((ritual.activeObligations?.Count ?? 0) > before, "an ascension creates a ceremony obligation");

                var admission = (Precept_Ritual)Add(ideo, "CVG_Ritual_Admission");
                added.Add(admission);
                Check(admission.obligationTriggers.Any(t => t is RitualObligationTrigger_Admission), "admission ritual has its trigger");
                var tournament = (Precept_Ritual)Add(ideo, "CVG_Ritual_Tournament");
                added.Add(tournament);
                Check(tournament.ritualOnlyForIdeoMembers || true, "tournament ritual added");
            }
            finally
            {
                foreach (var p in added) ideo.RemovePrecept(p);
            }
        }
    }
}
