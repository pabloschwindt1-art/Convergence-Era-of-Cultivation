using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Nivel de pureza de sangre (Rastro, Diluida, Pura, Ancestral…). Definido en XML: se pueden agregar niveles.
    /// </summary>
    public class BloodlinePurityDef : Def
    {
        /// <summary>Orden de menor a mayor pureza.</summary>
        public int order;
        /// <summary>Probabilidad de que un humano generado tenga ALGÚN linaje de esta pureza (se reparte por peso entre linajes).</summary>
        public float chance;
        public Color color = Color.white;
    }

    /// <summary>
    /// Linaje de sangre (Dragón, Fénix…): afinidades y lista ordenada de purezas. Cada pureza trae su gen de identidad
    /// (o rasgo sin Biotech), su hediff de efectos (solo tras la Convergencia), sus capacidades y sus genes de aspecto.
    /// Un linaje nuevo es solo XML: la lógica genérica está en <see cref="BloodlineUtility"/>.
    /// </summary>
    public class BloodlineDef : Def
    {
        public List<QiElementDef> affinities = new List<QiElementDef>();
        /// <summary>Peso relativo frente a otros linajes al generar humanos.</summary>
        public float commonality = 1f;
        public Color color = Color.white;
        /// <summary>Icono del xenotipo "único" que reciben los baseliners con una pureza sin <see cref="BloodlinePurity.xenotype"/>.</summary>
        public XenotypeIconDef xenotypeIcon;
        /// <summary>Purezas de menor a mayor (el índice es el nivel de pureza del linaje).</summary>
        public List<BloodlinePurity> purities = new List<BloodlinePurity>();

        public int IndexOf(BloodlinePurityDef purity) => purities.FindIndex(p => p.purity == purity);

        public BloodlinePurity PurityAt(int index) => index >= 0 && index < purities.Count ? purities[index] : null;

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            purities.SortBy(p => p.purity?.order ?? 0);
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var e in base.ConfigErrors()) yield return e;
            if (purities.NullOrEmpty()) yield return "no purities";
            for (int i = 0; i < purities.Count; i++)
            {
                var p = purities[i];
                if (p.purity == null) yield return $"purities[{i}].purity is null";
                if (p.gene == null && p.trait == null) yield return $"purities[{i}] has neither gene nor trait";
                var ext = p.gene?.GetModExtension<DefModExtension_Bloodline>();
                if (p.gene != null && (ext == null || ext.bloodline != this || ext.purity != p.purity))
                    yield return $"gene {p.gene.defName} needs DefModExtension_Bloodline with bloodline {defName} and purity {p.purity?.defName}";
            }
        }
    }

    /// <summary>Un nivel de pureza dentro de un <see cref="BloodlineDef"/>.</summary>
    public class BloodlinePurity
    {
        public BloodlinePurityDef purity;
        /// <summary>Gen de identidad (con Biotech). Lleva <see cref="DefModExtension_Bloodline"/>.</summary>
        public GeneDef gene;
        /// <summary>Rasgo de identidad sin Biotech (opcional).</summary>
        public TraitDef trait;
        public int traitDegree;
        /// <summary>Hediff de efectos (stats, contras), aplicado solo tras la Convergencia.</summary>
        public HediffDef hediff;
        /// <summary>Severidad del hediff; -1 = índice de la pureza + 1 (una etapa por pureza).</summary>
        public float hediffSeverity = -1f;
        /// <summary>Capacidades innatas que DESBLOQUEA esta pureza (se acumulan con las de las purezas menores).</summary>
        public List<AbilityDef> abilities = new List<AbilityDef>();
        /// <summary>
        /// Genes de aspecto que se agregan en esta pureza (se acumulan con los de purezas menores; si uno choca con otro
        /// de una pureza menor, por exclusionTag, gana el de la mayor: p. ej. 1 cola → 3 colas).
        /// </summary>
        public List<GeneDef> aspectGenes = new List<GeneDef>();
        /// <summary>
        /// Xenotipo que recibe el colono con esta pureza (p. ej. el del linaje en Pura y Ancestral). Sin él, un baseliner
        /// pasa a llevar un nombre de xenotipo propio ("Sangre de dragón (diluida)").
        /// </summary>
        public XenotypeDef xenotype;
        /// <summary>
        /// Afinidad de sangre: multiplicador de la velocidad de cultivo si la raíz espiritual tiene alguno de los elementos
        /// del linaje (<see cref="BloodlineDef.affinities"/>).
        /// </summary>
        public float affinityCultivationFactor = 1f;
        /// <summary>
        /// Pasión mínima en las habilidades con aptitud positiva del gen de identidad (talento natural; se aplica también antes de
        /// la Convergencia). Solo sube la pasión, nunca la baja.
        /// </summary>
        public Passion passion = Passion.None;
        /// <summary>Multiplicador de la probabilidad de aparición de esta pureza para este linaje.</summary>
        public float chanceFactor = 1f;
        /// <summary>
        /// Elemento de raíz que esta pureza puede despertar (Qilin: el Trueno) con <see cref="rootElementChance"/>, una vez,
        /// al alcanzarla (al nacer o al purificarse). Solo si el colono ya tiene raíz.
        /// </summary>
        public QiElementDef rootElement;
        public float rootElementChance;
    }

    /// <summary>Marca un gen como identidad de un linaje y una pureza.</summary>
    public class DefModExtension_Bloodline : DefModExtension
    {
        public BloodlineDef bloodline;
        public BloodlinePurityDef purity;
    }
}
