using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>Escudo de qi: absorbe daño hasta agotar su capacidad (capacidad base × poder de la técnica).</summary>
    public class HediffCompProperties_QiShield : HediffCompProperties
    {
        public float capacity = 40f;
        /// <summary>Fleck al absorber un golpe (opcional).</summary>
        public FleckDef absorbFleck;
        /// <summary>Fleck al romperse (opcional).</summary>
        public FleckDef breakFleck;

        public HediffCompProperties_QiShield()
        {
            compClass = typeof(HediffComp_QiShield);
        }
    }

    /// <summary>
    /// El daño lo intercepta <see cref="CompCultivator.PostPreApplyDamage"/> (después de la supresión de reino y antes de
    /// la armadura y los cinturones escudo). El comp se registra en el cultivador al agregarse: búsqueda O(1).
    /// </summary>
    public class HediffComp_QiShield : HediffComp, ITechniquePowered
    {
        private float max = -1f;
        private float remaining = -1f;

        public HediffCompProperties_QiShield Props => (HediffCompProperties_QiShield)props;

        public float Max => max < 0f ? Props.capacity : max;
        public float Remaining => remaining < 0f ? Max : remaining;
        public bool Broken => Remaining <= 0f;

        public void SetPower(float power)
        {
            max = Props.capacity * Mathf.Max(0.1f, power);
            remaining = max;
        }

        public override string CompLabelInBracketsExtra => Remaining.ToString("0") + " / " + Max.ToString("0");

        public override bool CompShouldRemove => base.CompShouldRemove || Broken;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            CompCultivator.For(Pawn)?.RegisterShield(this);
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            CompCultivator.For(Pawn)?.UnregisterShield(this);
        }

        /// <summary>Absorbe lo que pueda. Devuelve true si se tragó todo el golpe.</summary>
        public bool Absorb(ref DamageInfo dinfo)
        {
            if (Broken) return false;
            float amount = dinfo.Amount;
            float left = Remaining;
            var map = Pawn.MapHeld;
            if (amount <= left)
            {
                remaining = left - amount;
                if (map != null && Props.absorbFleck != null) FleckMaker.Static(Pawn.DrawPos, map, Props.absorbFleck, 1.6f);
                if (Broken) Break(map);
                return true;
            }
            remaining = 0f;
            dinfo.SetAmount(amount - left);
            Break(map);
            return false;
        }

        private void Break(Map map)
        {
            if (map != null && Props.breakFleck != null) FleckMaker.Static(Pawn.DrawPos, map, Props.breakFleck, 2.5f);
            if ((Pawn.Faction?.IsPlayer ?? false) && PawnUtility.ShouldSendNotificationAbout(Pawn))
                Messages.Message("CVG_QiShield_Broken".Translate(Pawn.Named("PAWN")), Pawn, MessageTypeDefOf.NegativeEvent, historical: false);
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref max, "max", -1f);
            Scribe_Values.Look(ref remaining, "remaining", -1f);
            if (Scribe.mode == LoadSaveMode.PostLoadInit) CompCultivator.For(Pawn)?.RegisterShield(this);
        }
    }

    /// <summary>
    /// Nodo de render con una textura simple (Graphic_Single), para efectos alrededor del cuerpo (burbuja del escudo de
    /// qi) con texturas vanilla de una sola vista. Solo <c>renderNodeProperties</c>, sin shaders propios.
    /// </summary>
    public class PawnRenderNode_Single : PawnRenderNode
    {
        public PawnRenderNode_Single(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props, tree)
        {
        }

        public override Graphic GraphicFor(Pawn pawn)
        {
            string path = TexPathFor(pawn);
            if (path.NullOrEmpty()) return null;
            var shader = ShaderFor(pawn);
            return shader == null ? null : GraphicDatabase.Get<Graphic_Single>(path, shader, Vector2.one, ColorFor(pawn));
        }
    }
}
