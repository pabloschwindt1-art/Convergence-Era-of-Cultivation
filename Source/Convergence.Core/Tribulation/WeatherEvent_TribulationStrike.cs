using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Convergence
{
    /// <summary>
    /// Rayo de tribulación: el destello, el rayo y el trueno vanilla, pero sin la explosión de fuego del rayo normal
    /// (el daño lo aplica <see cref="MapComponent_Tribulation"/> directamente al colono).
    /// </summary>
    [StaticConstructorOnStartup]
    public class WeatherEvent_TribulationStrike : WeatherEvent_LightningFlash
    {
        private static readonly Material LightningMat = MatLoader.LoadMat("Weather/LightningBolt");

        private readonly IntVec3 strikeLoc;
        private Mesh boltMesh;

        public WeatherEvent_TribulationStrike(Map map, IntVec3 strikeLoc) : base(map)
        {
            this.strikeLoc = strikeLoc;
        }

        public override void FireEvent()
        {
            SoundDefOf.Thunder_OffMap.PlayOneShotOnCamera(map);
            boltMesh = LightningBoltMeshPool.RandomBoltMesh;
            var loc = strikeLoc.ToVector3Shifted();
            for (int i = 0; i < 4; i++)
            {
                FleckMaker.ThrowSmoke(loc, map, 1.5f);
                FleckMaker.ThrowMicroSparks(loc, map);
                FleckMaker.ThrowLightningGlow(loc, map, 1.5f);
            }
            SoundDefOf.Thunder_OnMap.PlayOneShot(SoundInfo.InMap(new TargetInfo(strikeLoc, map)));
            TribulationSproutUtility.OnStrike(map, strikeLoc);
            CompQiLightningRod.OnTribulationStrike(map, strikeLoc);
        }

        public override void WeatherEventDraw()
        {
            if (boltMesh == null) return;
            Graphics.DrawMesh(boltMesh, strikeLoc.ToVector3ShiftedWithAltitude(AltitudeLayer.Weather), Quaternion.identity,
                FadedMaterialPool.FadedVersionOf(LightningMat, LightningBrightness), 0);
        }
    }
}
