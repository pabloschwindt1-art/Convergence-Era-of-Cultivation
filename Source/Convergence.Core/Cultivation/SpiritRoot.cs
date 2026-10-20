using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Raíz espiritual de un colono: calidad + uno o más elementos.
    /// En el xianxia, menos elementos = raíz más pura (más rápida).
    /// </summary>
    public class SpiritRoot : IExposable
    {
        public SpiritRootGradeDef grade;
        public List<QiElementDef> elements = new List<QiElementDef>();

        public bool IsValid => grade != null && elements.Count > 0;

        public string ElementsLabel => elements.Select(e => e.LabelCap.ToString()).ToCommaList();

        public void ExposeData()
        {
            Scribe_Defs.Look(ref grade, "grade");
            Scribe_Collections.Look(ref elements, "elements", LookMode.Def);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                elements ??= new List<QiElementDef>();
                elements.RemoveAll(e => e == null);
            }
        }
    }
}
