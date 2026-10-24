# Raíces espirituales que cambian: diseño (borrador para revisar)

Pedido del usuario (2026-10-24): "cultivar es crecer, avanzar, progresar". Hoy la raíz con la que nace un colono lo encasilla
para siempre: un colono excelente con raíz inferior (o sin raíz) no tiene salida. Este documento propone **cinco vías** para
mejorar, extirpar, trasplantar y fusionar raíces. Nada se implementa hasta que el usuario lo apruebe.

Estado actual (lo que ya funciona):
- La raíz es un **gen** (calidad + elementos) con Biotech o un **rasgo** sin Biotech. Desde el 2026-10-24 se recalcula al
  momento cuando cambian rasgos o genes, y rasgo y gen se combinan tomando el grado más alto.
- **Xenogerm de Biotech:** los genes de raíz se extraen y se implantan con el extractor y el ensamblador vanilla. Ya es un
  "trasplante" tecnológico, pero copia la raíz sin quitársela al donante y no tiene lore propio.
- **Sangre ancestral** (linajes): sube la raíz un grado, una sola vez por colono.

Grados: inferior (×0,6) → común (×1) → superior (×1,5) → celestial (×2,2). Probabilidad al nacer: 50/30/15/5.

---

## 1. Principios

- **Nada pasa solo.** Cada mejora pide algo: una cirugía, un avance de reino, una píldora, un ritual o una fusión. Todas tienen
  riesgo.
- **Cada vía, sobre un sistema vanilla** (principio n.º 2): cirugía de órganos, avance (ya nuestro), ingerible con tolerancia,
  ritual de Ideology con calidad y receta de mesa. Nada de ventanas nuevas.
- **La celestial es rara.** Se puede llegar a ella por avance, trasplante o fusión, pero no por píldora ni por ritual.
- **Historias primero:** extirpar una raíz es un crimen en casi todo Jiuxiao. Tiene que generar odio, venganza, comercio y
  dilemas por canales vanilla (pensamientos, relaciones, preceptos, reputación de facciones, relatos).

## 2. Dónde vive cada cosa

| Pieza | Mod | Por qué |
|---|---|---|
| `SpiritRootUtility`: subir o bajar grado, quitar la raíz y dársela a otro (gen o rasgo, sin duplicar lógica) | **Core** | La usan todas las vías y los módulos; `BloodlineUtility.TryRefineRoot` pasa a usarla |
| Objeto **raíz sellada**: **un ThingDef por grado** (`CVG_SealedRoot_Inferior/Common/Superior/Heavenly`) con `CompSealedRoot` (elementos y nombre del donante) | **Core** | Es un bien comerciable y lo usan Grafts, Factions y Stories. Un def por grado deja la fusión como receta XML pura y hace que el precio, el ícono y los filtros de almacén salgan solos |
| Mejora en el **avance de reino** | **Core** | `BreakthroughUtility.Ascend` |
| **Ritual** de purificación de raíz | **Core** (carpeta Ideology; el Core ya exige las cinco DLC) | Es parte del cultivo, no de un módulo |
| Cirugías de **extirpar** y **trasplantar** en el altar | **Grafts** | Ya tiene el altar del Dao y el patrón de instalar y quitar injertos |
| **Fusión** de raíces | **Grafts** | El horno de refinado de huesos ya "refina partes de bestias en injertos" |
| **Píldora** de lavado de raíz | **Alchemy** | Ahí están todas las píldoras y la toxicidad |
| `IngestionOutcomeDoer_RefineRoot` (efecto genérico de "subir raíz") | **Core** | Para que cualquier módulo lo use solo con XML |

## 3. Vía 1: extirpar y trasplantar en el altar (Grafts)

**Equivalente vanilla:** la extracción de órganos (`Recipe_RemoveBodyPart`) y la instalación de partes. Se usan sus mismos eventos
de historia: `HarvestedOrgan`, `HarvestedOrganFromGuest/Colonist` e `InstalledOrgan`, así que **los preceptos de uso de órganos
de Ideology lo juzgan solos** (aborrecible, horrible, aceptable). Lo mismo pasa con los pensamientos vanilla de "supe que le
sacaron un órgano a un colono o a un invitado" y con la pérdida de reputación con la facción del donante.

### Extirpar raíz (`CVG_ExtractSpiritRoot`)
- **Objetivo:** cualquier pawn con raíz: prisionero, colono, invitado o caído (como la extracción vanilla). En el altar del Dao.
- **Requisitos:** Medicina 8, una investigación propia en Grafts ("Arte de arrancar raíces") y 2 de jade espiritual para sellar
  la raíz (sin objeto contenedor aparte).
- **Resultado** (chance de éxito de cirugía vanilla × factor por grado):

| Grado del donante | Factor |
|---|---|
| Inferior | ×1,0 |
| Común | ×0,9 |
| Superior | ×0,75 |
| Celestial | ×0,6 |

  - **Éxito:** raíz sellada del mismo grado y elementos.
  - **Éxito parcial** (la mitad de los fallos): raíz sellada un grado más baja (una inferior que falla se pierde).
  - **Fallo:** la raíz se deshace en el aire (un estallido de qi con un fleck vanilla). La cirugía sigue el fallo vanilla:
    daño leve o grave al donante.
- **El donante**, salga como salga:
  - **Pierde la raíz** (se le quitan los genes de raíz o el rasgo).
  - Recibe el hediff **"raíz arrancada"** (`CVG_RootTorn`), permanente: −10 % de conciencia, dolor crónico leve y "el qi se le
    escapa". Si cultivaba qi, **su reino de qi se apaga** (queda inactivo, como el reino latente antes de la Convergencia) y el
    camino corporal sigue intacto. Así un cultivador mutilado puede pasarse al cuerpo, que es una historia en sí.
  - Pensamiento propio **"me arrancaron la raíz"** (−30, permanente con decaimiento lento) y opinión de −100 sobre el cirujano y
    sobre quien lo ordenó (vía `Thought_MemorySocial`, como el "me cosecharon un órgano" vanilla).
  - Los reinos altos se resisten: si el donante está en Fundación o más, el fallo tiene chance de **muerte** (desviación de qi):
    25 % en Fundación, 50 % en Núcleo Dorado o más. Decidido por el usuario (2026-10-24); no depende de ningún ajuste.

### Implantar raíz (`CVG_ImplantSpiritRoot`)
- **Objetivo:** un colono o prisionero. En el altar, con Medicina 8 y la raíz sellada como ingrediente.
- **Si ya tenía raíz:** la nueva reemplaza a la vieja, que se pierde. Para conservarla hay que extirparla antes.
- **Resultado:** con Biotech, la raíz entra como **xenogenes** (grado + elementos): no se hereda a los hijos y se ve en la pestaña
  de genes. Sin Biotech, entra como rasgo.
- **Rechazo:** hediff **"raíz ajena"** (`CVG_ForeignRoot`) que dura 30 días. Con elementos que chocan con la afinidad del
  receptor (las resonancias de `ElementalResonance`) hay chance de rechazo: la raíz baja un grado o se pierde. Mientras dura
  suma **+0,1 de disonancia celestial**: el Cielo nota la costura. Encaja con el sistema de implantes.
- **Historias:** relato vanilla (`TaleDef`) "le implantó la raíz de [donante]". El receptor guarda el nombre del donante en el
  comp, y si se conocían, pensamiento de culpa o gratitud según la relación.

### Comercio y facciones
- La raíz sellada tiene valor de mercado por grado (orientativo: 600 / 1500 / 4000 / 12000).
- **Pabellón del Caldero Celestial**: compra y vende raíces selladas (comerciantes de su secta, subastas).
- **Culto de la Luna Sangrienta**: la extracción es su arte. Con su ideología, el precepto es aceptable o venerado, y sus
  incursiones pueden intentar **secuestrar a tus colonos de raíz alta** (más adelante, en Stories).
- **Piratas:** "cazan cultivadores y tesoros para venderlos" (LORE §6). Una raíz sellada en el almacén sube el valor que atrae
  incursiones, como cualquier riqueza.

## 4. Vía 2: el avance de reino (Core)

**Equivalente vanilla:** ninguno directo. Es nuestro sistema de avances; se engancha en `BreakthroughUtility.Ascend`, que ya recibe
la calidad del avance.

- Al **avanzar de reino mayor** (no por etapa), una chance de que la raíz suba un grado. Es la vía del que no se rinde.
- La chance es **mayor cuanto más baja la raíz** (el Cielo premia al que rompe el muro con una raíz pobre) y sube con la calidad
  del avance:

| Raíz actual | Base | Con avance perfecto |
|---|---|---|
| Inferior | 20 % | 35 % |
| Común | 10 % | 20 % |
| Superior | 4 % (solo si el avance pasó por tribulación) | 8 % |

- **Camino corporal sin raíz:** al llegar a **Médula** (el templado que "limpia" el cuerpo), chance del 15 % de que **despierte una
  raíz inferior**. Le da salida al colono excelente que nació sin raíz.
- Carta vanilla de logro, con lore: "la raíz de X se aclaró en el avance".

## 5. Vía 3: la píldora de lavado de raíz (Alchemy)

**Nombre:** **Píldora de Lavado de Raíz** (洗根丹). No "lavado de médula": "Limpieza de médula" ya es un reino del camino
corporal y confundiría.

**Equivalente vanilla:** un ingerible con **tolerancia** (como la tolerancia a las drogas) y nuestra toxicidad de píldoras
(`CVG_PillToxicity`).

- **Receta:** horno de alquimia; ingredientes caros (hierba espiritual de alto grado + núcleo de bestia + jade). La calidad de la
  píldora (tabla de fallas ya existente) suma a la chance.
- **Efecto:** chance de subir un grado. **Tope: superior**: con raíz superior no hace nada (el texto lo avisa al intentar dársela).
- **Chance:** inferior 40 %, común 25 %, con ±10 % por calidad de la píldora.
- **Tolerancia** (`CVG_RootWashTolerance`): cada píldora tomada resta 10 puntos a la siguiente y se va en 60 días. Evita la
  "metralleta de píldoras".
- **Fallo:** toxicidad de píldora fuerte y chance de desviación de qi leve (pérdida de progreso del reino, como un avance fallido).
- Lore: la receta es del Pabellón del Caldero Celestial; dicen que la primera la hizo un alquimista para su hijo sin raíz.

## 6. Vía 4: el ritual de purificación de raíz (Core, Ideology)

**Equivalente vanilla:** la **vinculación con el árbol del ánima** de Royalty (un ritual que mejora a un pawn según su calidad y
la del lugar) y la **ceremonia de escarificación** de Ideology (ritual con objetivo, precepto propio, resultado por calidad).

- **Precepto** `CVG_RootPurificationCeremony` (como `ScarificationCeremony`): da el ritual a la ideología. Se agrega por defecto a
  las ideologías con memes de cultivo, si existen; si no, el jugador lo elige al armarla.
- **Foco:** un **punto de cultivo** o una **formación de reunión de qi** (lo que haya en el Core). La concentración de qi del lugar
  (`QiConcentration`) sube la calidad.
- **Roles:** el **oficiante** es un cultivador de reino más alto que el objetivo (o el Maestro, si sigue en la colonia), y el
  **objetivo** es el colono a purificar. El público suma calidad como en cualquier ritual.
- **Resultados** (`RitualOutcomeEffectWorker` con calidad):
  - **Terrible:** desviación de qi del objetivo (dolor, pérdida de progreso).
  - **Pobre:** nada; el objetivo queda frustrado (pensamiento leve).
  - **Bueno:** 35 % de subir un grado.
  - **Excelente:** 70 % de subir un grado. **Sin raíz: despierta una raíz inferior.**
  - Tope: **superior**, igual que la píldora.
- **Una vez por año por colono** (enfriamiento por pawn guardado en el comp).
- Lore: los monjes del Templo del Loto Silencioso la practican con niños sin raíz; casi nunca funciona, pero nadie se va
  sin intentarlo.

## 7. Vía 5: fusión de raíces (Grafts, horno de refinado de huesos)

Pedido del usuario: "5 comunes hacen una buena, 5 buenas hacen una excelente…"; después, 3 a 1.

**Equivalente vanilla:** una receta de mesa común (como cocinar varias cosas en una). Como hay un ThingDef por grado, son **tres
`RecipeDef` en XML** (3 inferiores → común, 3 comunes → superior, 3 superiores → celestial) en el horno de refinado de huesos,
que ya existe. Solo hace falta un parche chico en C# (postfix de `GenRecipe.MakeRecipeProducts`, que recibe los ingredientes)
para que el resultado herede los elementos y los donantes. La chance de fallo va en un `RecipeWorker` corto.

- **Receta** "fundir raíces" en el horno de refinado de huesos: **3 raíces selladas del mismo grado** → **1 del grado siguiente**
  (decidido por el usuario, 2026-10-24: 5 a 1 pedía 125 inferiores para una celestial).
  - 3 inferiores → 1 común; 3 comunes → 1 superior; 3 superiores → 1 celestial.
  - Una celestial cuesta 27 inferiores, 9 comunes o 3 superiores. Como cada raíz sale de un donante vivo y con riesgo, el mundo
    no se llena de raíces.
- **Habilidad:** Medicina (refinar es trabajo de cirujano espiritual), mínimo 10.
- **Chance:** 75 % → común, 55 % → superior, 30 % → celestial; +2 % por punto de Medicina sobre 10. Si falla, se pierden 2 de las
  3 raíces y la otra vuelve.
- **Elementos del resultado:** el elemento más repetido entre las tres, y **un elemento menos** que el promedio (la fusión
  purifica). Es la única forma de purificar elementos, que en el xianxia vuelve más rápida la raíz.
- **Historia:** cada raíz sellada guarda a quién perteneció. La fundida guarda "hecha con las raíces de tres cultivadores", y un
  relato lo cuenta.

## 8. Cómo se reparten (resumen)

| Vía | Quién la usa | Tope | Riesgo | Costo |
|---|---|---|---|---|
| Avance de reino | Cualquier cultivador que avanza | Celestial (con tribulación) | El del avance | Ninguno extra |
| Despertar en Médula (cuerpo) | Colonos sin raíz | Inferior | El del avance | Ninguno extra |
| Píldora | Cualquiera con raíz | Superior | Toxicidad, desviación | Ingredientes caros |
| Ritual | Ideologías con el precepto | Superior (o despierta) | Desviación | Un año de espera |
| Extirpar + implantar | Quien tenga donante | El grado del donante | Moral, reputación, rechazo | Un donante |
| Fusión | Quien junte raíces selladas | Celestial | Perder 2 de 3 | 3 raíces |

## 9. Decisiones

Tomadas por el usuario (2026-10-24):
- **Fusión 3 a 1** (no 5 a 1).
- **La extirpación puede matar** a donantes de Fundación o más, sin ajuste que lo apague.
- **Sin cofre de jade:** la raíz sellada no se marchita y no hay contenedor aparte (se sella con jade espiritual).

Abiertas (va mi propuesta si no se dice otra cosa):
1. Ritual con **precepto propio** (como la escarificación).
2. **No** se extirpa de cadáveres: el dilema es más fuerte con el donante vivo.
3. El secuestro de colonos de raíz alta por la Luna Sangrienta queda para una segunda tanda, en Stories.

## 10. Arte

128×128, como los demás objetos de Grafts y Alchemy:
- **Raíz sellada:** 4 variantes de un mismo diseño (inferior, común, superior, celestial). **Hecha** (2026-10-24, ChatGPT, prompt
  de este documento): recortes de 128×128 en `art/concepts2/items/it_sealed_root/out/SealedRoot_<Grado>.png`; van a
  `Mods/Convergence.Core/Common/Textures/Things/Item/CVG/` cuando existan los ThingDefs.
- **Píldora de Lavado de Raíz:** 1.

El resto (cirugías, hediffs, fusión, avance, ritual) no lleva arte nuevo.

## 11. Orden de implementación propuesto

1. Core: `SpiritRootUtility` (subir, bajar, quitar, dar) + raíz sellada + avance de reino + despertar en Médula.
2. Grafts: extirpar, implantar, rechazo, preceptos y pensamientos; después la fusión.
3. Alchemy: píldora con tolerancia.
4. Core: ritual.
5. Factions: comercio de raíces en el Pabellón del Caldero Celestial.

Cada paso con autotest en su mod (`-cvgselftest` en el Core, `-cvggraftstest` en Grafts) y con lore en inglés y español en cada
def, como exige "todo lleva lore".
