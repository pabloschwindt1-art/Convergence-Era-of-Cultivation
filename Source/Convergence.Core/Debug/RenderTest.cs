using System.IO;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Herramienta de arte: con <c>-cvgrendertest</c> guarda retratos (sur, este, norte) de colonos con cada gen propio que
    /// dibuja partes del cuerpo y con cada prenda propia en los 5 tipos de cuerpo, en <c>&lt;datos&gt;/cvg_renders/</c>.
    /// Sirve para revisar el arte y los offsets sin abrir el juego a mano. Sin el argumento no hace nada.
    /// </summary>
    public static class RenderTest
    {
        private static bool done;
        private const int Size = 192;
        private static readonly Rot4[] Rots = { Rot4.South, Rot4.East, Rot4.North };

        public static void CheckCommandLine()
        {
            if (done || Current.ProgramState != ProgramState.Playing || Find.TickManager.TicksGame < 60) return;
            done = true;
            if (!GenCommandLine.CommandLineArgPassed("cvgrendertest")) return;
            string dir = Path.Combine(GenFilePaths.SaveDataFolderPath, "cvg_renders");
            int count = 0, fails = 0;
            try
            {
                foreach (var gene in DefDatabase<GeneDef>.AllDefs.Where(g => g.defName.StartsWith("CVG_") && !g.renderNodeProperties.NullOrEmpty()))
                {
                    var pawn = Generate(Gender.Male, BodyTypeDefOf.Male);
                    pawn.genes.AddGene(gene, xenogene: true);
                    count += Save(pawn, Path.Combine(dir, "genes"), gene.defName);
                }
                var bodies = new[] { BodyTypeDefOf.Male, BodyTypeDefOf.Female, BodyTypeDefOf.Thin, BodyTypeDefOf.Hulk, BodyTypeDefOf.Fat };
                foreach (var def in DefDatabase<ThingDef>.AllDefs.Where(d => d.IsApparel && d.defName.StartsWith("CVG_")))
                {
                    foreach (var body in bodies)
                    {
                        var pawn = Generate(body == BodyTypeDefOf.Female ? Gender.Female : Gender.Male, body);
                        var apparel = (Apparel)ThingMaker.MakeThing(def, GenStuff.DefaultStuffFor(def));
                        if (!ApparelUtility.HasPartsToWear(pawn, def)) continue;
                        pawn.apparel.Wear(apparel, dropReplacedApparel: false);
                        count += Save(pawn, Path.Combine(dir, "apparel"), $"{def.defName}_{body.defName}");
                    }
                }
            }
            catch (System.Exception e)
            {
                fails++;
                Log.Error("[Convergence] Render test: " + e);
            }
            Log.Message($"[Convergence] Render test: {count} image(s) in {dir}\n  " + (fails == 0 ? "ALL OK" : $"FAILURES: {fails}"));
        }

        private static Pawn Generate(Gender gender, BodyTypeDef body)
        {
            var request = new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayerSilentFail, forceGenerateNewPawn: true,
                canGeneratePawnRelations: false, fixedGender: gender, forcedXenotype: XenotypeDefOf.Baseliner, forceNoGear: true);
            var pawn = PawnGenerator.GeneratePawn(request);
            pawn.apparel?.DestroyAll();
            pawn.story.bodyType = body;
            return pawn;
        }

        private static int Save(Pawn pawn, string dir, string name)
        {
            Directory.CreateDirectory(dir);
            int n = 0;
            foreach (var rot in Rots)
            {
                pawn.Drawer.renderer.SetAllGraphicsDirty();
                PortraitsCache.SetDirty(pawn);
                var rt = PortraitsCache.Get(pawn, new Vector2(Size, Size), rot, new Vector3(0f, 0f, 0.1f), 1f, supersample: false, compensateForUIScale: false);
                var prev = RenderTexture.active;
                RenderTexture.active = rt;
                var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
                tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                tex.Apply();
                RenderTexture.active = prev;
                File.WriteAllBytes(Path.Combine(dir, $"{name}_{rot.ToStringHuman()}.png"), tex.EncodeToPNG());
                Object.Destroy(tex);
                n++;
            }
            return n;
        }
    }
}
