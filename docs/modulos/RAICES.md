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

## 7a. Vía de ingeniería (Qi-Tech): la mesa de cirugía de qi

Pedido del usuario (2026-10-24): "la mesa de injertos, pero para el mod de ingeniería". La regla de jugar **con y sin
tecnología** pide las dos puertas: el altar del Dao (Grafts, sin electricidad) es la vía de cultivo; Qi-Tech tiene la suya, a la
que se llega investigando, sin cultivadores. Las dos dan el mismo resultado (raíces selladas, implantadas, fundidas) por caminos
distintos, y cada una cobra a su manera: el altar purifica y la máquina suma disonancia.

**Equivalente vanilla:** la cama de hospital (cama médica con monitor de signos vitales) y la cadena de investigación de biónicos.

### Investigación: "Ingeniería de raíces" (`CVG_Research_RootEngineering`)
- Pestaña **Ingeniería espiritual**, nivel espacial, costo 3000, mesa de investigación de alta tecnología.
- Requisitos: `CVG_Research_AdvancedSpiritInstrumentation` (el resonador) y `CVG_Research_QiBionics`. Posición en el árbol: (4, 1).
- Como toda la pestaña, lleva `DefModExtension_RequiresConvergence`: no se empieza antes de la Convergencia.
- Desbloquea la mesa, las dos cirugías en ella, la fusión en el laboratorio de píldoras de qi y la raíz artificial.
- Lore: los ingenieros midieron por primera vez una raíz con el resonador y la llamaron "una antena que alguien plantó en el
  alma". Los del Caldero Celestial lo consideran robo; Ascension Dynamics, una línea de producto.

### Mesa: "Mesa de cirugía de qi" (`CVG_QT_QiSurgeryTable`)
- **Cama médica de 1×2** (como el altar y la cama de hospital), `Graphic_Multi` con vistas sur, este y norte, `drawSize (2,2)`.
- Pide **energía (250 W)**; sin energía funciona como una camilla común y no permite las cirugías de raíz.
- Stats: `SurgerySuccessChanceFactor` 1,25 (el altar 1,15), `MedicalTendQualityOffset` 0,12, `ImmunityGainSpeedFactor` 1,1.
- Se enlaza con el monitor de signos vitales vanilla y con el **resonador espiritual** (si está al lado, +5 % de éxito en las
  cirugías de raíz).
- Costo: acero 80, plastiacero 20, componentes 4, cristal primordial 2, piedra espiritual 30.
- Lore: una camilla de acero con un anillo de bobinas que lee el flujo de qi del paciente mientras se opera. Los cirujanos
  trabajan mirando una pantalla, no al paciente; dicen que así no se oye cuando la raíz grita.

### Proceso
| Paso | Altar del Dao (Grafts) | Mesa de cirugía de qi (Qi-Tech) |
|---|---|---|
| Extirpar | Medicina 8, 2 de jade espiritual | Medicina 8, 10 piedras espirituales y energía. Algo más de éxito (por la mesa); los mismos riesgos para el donante |
| Implantar | "Raíz ajena" 30 días, +0,1 de disonancia | "Raíz ajena" 30 días con **+0,25** de disonancia, y una **costura de qi** permanente de +0,05: el Cielo nota la máquina |
| Fundir 3 a 1 | Horno de refinado de huesos, Medicina 10, purifica un elemento | **Laboratorio de píldoras de qi** (`CVG_QT_PillLab`), Intelectual 8, energía y 20 piedras espirituales; 10 puntos más de éxito, **no purifica** |
| Algo propio | — | **Raíz artificial** (abajo) |

Las cirugías son las mismas recetas con un `RecipeWorker` que mira la cama: el altar o la mesa de qi encendida. Así no se
duplican defs, y el paciente busca la que haya, como el altar hoy (`Patch_RestUtility_FindPatientBedFor`).

### Raíz artificial (`CVG_QT_ArtificialRoot`)
- Implante hecho en el laboratorio de píldoras de qi: plastiacero, cristal primordial 3, componentes avanzados 2.
- Da una **raíz común de un elemento al azar** a quien no tiene raíz (con raíz, no se puede instalar). Va como hediff con
  `DefModExtension_SpiritRoot`: hay que sumar a `CompCultivator.Sync` la lectura de hediffs además de genes y rasgos.
- **+0,35 de disonancia** permanente: la tribulación de Núcleo Dorado pega fuerte. Nunca sube de grado (ni por avance, ni por
  píldora, ni por ritual): es una máquina.
- Lore: el producto estrella de Ascension Dynamics, "cultivo para todos". Los que la llevan avanzan, pero cuando llega la
  tribulación el rayo sabe dónde está el metal.
- Arte: necesita su propio sprite (128×128).

## 7c. Órganos de cultivador (idea del usuario, 2026-10-24)

"Si le quito el corazón a un cultivador, ¿es distinto del de un colono normal?" Hoy no: sale el corazón vanilla. En el xianxia
el cuerpo de un cultivador está templado por el qi, y sus órganos valen más (y se roban).

**Equivalente vanilla:** la extracción de órganos (`Recipe_RemoveBodyPart` → `MedicalRecipesUtility.SpawnNaturalPartIfClean`
suelta `spawnThingOnRemoved` del órgano) y la instalación (`Recipe_InstallNaturalBodyPart`). No se crean órganos nuevos:
- Un **comp** (`CompTemperedOrgan`) que se agrega por parche XML a los órganos naturales vanilla (corazón, pulmón, riñón,
  hígado, ojo, y los de otros mods si usan la misma base). Guarda el **reino** y el **camino** del donante.
- Un postfix a `SpawnNaturalPartIfClean` lo llena cuando el donante cultiva. El objeto se llama "corazón (Fundación)", vale
  ×1,5 por reino y la ventana de información dice de quién era.
- Al **instalarlo**, el receptor recibe el hediff **"órgano templado"** en esa parte, con bonos chicos según el reino y el
  órgano: corazón → regeneración de qi y de vida; pulmones → respiración y resistencia al ambiente; riñones y hígado →
  filtrado de sangre y resistencia a la toxicidad de píldoras; ojos → vista. Los del **camino corporal** valen el doble,
  porque ese camino templa justamente los órganos.
- **Disonancia +0,05** por órgano ajeno templado (el qi de otro dentro del cuerpo) y rechazo leve si el reino del donante es
  mucho más alto que el del receptor.
- Ideology y la moral, gratis: se usan los mismos eventos de extracción de órganos de vanilla.
- Historias: el Culto de la Luna Sangrienta los compra, los piratas cazan cultivadores para venderlos por partes (LORE §6) y
  un colono puede vivir con el corazón de su maestro.
- Sin arte nuevo: usa los sprites vanilla de cada órgano.

## 7b. Raíces selladas como recompensa (Stories)

Pedido del usuario (2026-10-24): usar las raíces para mejorar las recompensas de los eventos que se sumaron el 2026-10-23.
Los eventos de Stories ya dan objetos con `SO_Items` (que acepta `<chance>`), así que es **solo XML** una vez que existan los
ThingDefs. Siempre con probabilidad y casi siempre inferior o común, para no llenar el mundo de raíces:

| Evento | Opción | Raíz | Historia |
|---|---|---|---|
| `OldGraveOpened` (tumba vieja) | saquear | 30 % común | El cultivador enterrado selló su raíz para que no se perdiera; los sueños inquietos ya están en el evento |
| `CacheVault` (bóveda antigua) | — | 50 % común, 10 % superior | Tesoro de una secta caída en la Calamidad de Hierro |
| `OrphanLegacy` (herencia del huérfano) | — | 1 raíz del grado del padre | Lo único que dejó su padre: implantarla en el huérfano es la historia obvia |
| `DemonBargain` (pacto demoníaco) | aceptar | 1 superior | La Luna Sangrienta paga con raíces arrancadas; aceptarla debería pesar en la conciencia |
| `SectTournament` (torneo de sectas) | ganar | 1 común (o superior al primer puesto) | Premio clásico de torneo xianxia |
| `CauldronCreditors` (deudores del Caldero) | pagar | aceptan raíces selladas como pago | El Pabellón comercia raíces |
| `PuppetCrypt` (cripta de títeres) | — | 25 % inferior | Raíces de los titiriteros, selladas en sus muñecos |

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
- **Píldora de Lavado de Raíz:** 1. **Hecha** (2026-10-24, ChatGPT): `art/concepts2/items/it_pill_rootwash/out/RootWashPill.png`,
  escalada a 72 px dentro de 128 como las otras píldoras; va a `Mods/Convergence.Alchemy/Common/Textures/Things/Item/CVG/`.

- **Mesa de cirugía de qi** (Qi-Tech): 3 vistas (sur, este, norte), 256×256 cada una, como el altar. Prompt en la conversación
  del 2026-10-24; pendiente.
- **Raíz artificial** (Qi-Tech): 1 objeto de 128×128. Pendiente.

El resto (cirugías, hediffs, fusión, avance, ritual) no lleva arte nuevo.

## 11. Orden de implementación (rearmado el 2026-10-24)

1. **Core, base:** `SpiritRootUtility` (subir, bajar, quitar, dar), `Sync` que también lee hediffs con `DefModExtension_SpiritRoot`,
   4 ThingDefs de raíz sellada con `CompSealedRoot` y su arte. La sangre ancestral pasa a usar la utilidad.
2. **Core, avance:** subida de raíz al pasar de reino mayor y despertar de raíz en Limpieza de Médula.
3. **Core, órganos templados:** `CompTemperedOrgan` por parche a los órganos naturales, postfix de `SpawnNaturalPartIfClean` y
   hediff "órgano templado" al instalar.
4. **Grafts:** extirpar e implantar (un `RecipeWorker` que acepta el altar o la mesa de qi), "raíz arrancada", "raíz ajena",
   pensamientos y eventos de órganos; fusión 3 a 1 en el horno de huesos.
5. **Qi-Tech:** investigación "Ingeniería de raíces", mesa de cirugía de qi, fusión en el laboratorio de píldoras de qi y raíz
   artificial (espera los dos sprites; mientras tanto, la mesa usa la cama de hospital teñida).
6. **Alchemy:** Píldora de Lavado de Raíz con tolerancia.
7. **Stories:** raíces selladas como recompensa en 7 eventos.
8. **Core, ritual** de purificación (Ideology). Es lo más pesado: va último y puede pasar a otra tanda.
9. **Factions:** el Pabellón del Caldero Celestial compra y vende raíces (stock de sus comerciantes).
10. **Prueba única** en 1.7 con `-cvgselftest`, `-cvggraftstest` y `-cvgqitechtest` (~5 min), una partida por prueba. Después,
    changelog, commit y aviso al celular.

Cada paso lleva lore en inglés, español y español latino en cada def, como exige "todo lleva lore", y su caso en el autotest
de su mod.
