# Convergence: Core — Diseño técnico (v0)

El módulo Core es nuestro "framework": todo el sistema de cultivo. Los demás módulos solo agregan datos.
Este documento cubre la **primera versión jugable (MVP)** y deja preparado lo que viene después.

Principios (lecciones de los mods de referencia):
- **Datos en XML, lógica en C#.** Reinos, técnicas, raíces e implantes se definen en XML. Agregar uno nuevo no
  requiere tocar código (el error de Isekai: enums cerrados).
- **Usar lo que el juego ya trae** antes de inventar: hediffs con etapas para los reinos, stats propios, habilidades
  vanilla, `renderNodeProperties` para auras.
- **Sin dependencias** salvo Harmony. Sin AssetBundles ni `GL` (por la 1.7).
- **Usa todas las DLC** (decisión 2026-10-15), cada función de DLC en su carpeta condicional (`Mods/Biotech`, `Mods/Royalty`…) o con `MayRequire` / `ModsConfig.BiotechActive`: sin la DLC, el mod funciona sin esa parte.

---

## 1. Estructura del repositorio

```
C:\MODS_Rimmworld\
├── Mods\
│   └── Convergence.Core\           ← la carpeta que ve RimWorld
│       ├── About\  (About.xml, Preview.png)
│       ├── LoadFolders.xml
│       ├── 1.6\Assemblies\         ← salida del compilador
│       ├── Common\Defs\  Common\Patches\  Common\Textures\  Common\Sounds\
│       ├── Common\Languages\English\  Spanish\  SpanishLatin\
│       └── Mods\Biotech\  Mods\Royalty\   ← contenido condicional por DLC
├── Source\
│   └── Convergence.Core\Convergence.Core.csproj
├── tools\  (script para enlazar el mod a la carpeta Mods del juego)
└── docs\
```

Desarrollo: un **enlace de directorio** (junction) desde `RimWorld\Mods\Convergence.Core` hacia `Mods\Convergence.Core`.
Así compilamos y el juego ve los cambios sin copiar nada.

- **packageId:** `prospero1088.convergence.core` (autor: Prospero1088).
- **Dos modos** (ver DISENO.md §2.1): el Core no usa electricidad ni investigación industrial; lo tecnológico va en
  módulos aparte o marcado con `DefModExtension_TechOnly`.
- **Prefijo de defs:** `CVG_`. **Namespace:** `Convergence`.

## 2. Piezas del sistema

### 2.1 Estado global: `GameComponent_Convergence`
- Guarda si ocurrió la Convergencia, cuándo y el modo elegido.
- Programa el evento según los ajustes: narrador (entre el día X y el día Y), investigación o inmediato (escenario).
- Acceso rápido: `ConvergenceState.Active`.
- Más adelante: el calendario celestial (Marea de Qi, Luna del Demonio Interno…).

### 2.2 El cultivador: `CompCultivator` (ThingComp)
- Se agrega por código al arrancar a **todo ThingDef humanoide** (incluye razas de otros mods), recorriendo
  `DefDatabase<ThingDef>`. Es el método robusto de Isekai: un parche XML no alcanza a las razas heredadas.
- **No hace nada si el colono no tiene raíz espiritual**, así que su costo es casi nulo.
- Caché estático `thingID → comp` para buscarlo rápido (patrón de Isekai).
- Contiene sub-rastreadores (`IExposable`), cada uno en su archivo:

| Rastreador | Guarda |
|---|---|
| `SpiritRootTracker` | Elemento(s), calidad y estado (`None` / `Latent` / `Awakened`) |
| `QiTracker` | Qi actual (el máximo sale del stat) |
| `RealmTracker` | Reino, etapa (1-4), progreso, "cuello de botella" |
| `DaoHeartTracker` | Estabilidad 0-100 y riesgo de desviación |
| `MeridianTracker` | (después del MVP) nodos desbloqueados del árbol |

- **Tick:** `CompTickRare` (cada 250 ticks) regenera qi y suma progreso. Nada pesado por tick.

### 2.3 Raíz espiritual
- **Con Biotech:** `GeneDef` por elemento (`CVG_Root_Fire`…) y un gen de calidad.
- **Sin Biotech:** `TraitDef CVG_SpiritRoot` con grados de calidad (el elemento lo elige el comp al generar el colono).
- Ambos llevan un `DefModExtension_SpiritRoot { element, quality }`. El comp lee de genes o rasgos sin saber cuál es.
- **Latente** antes de la Convergencia, **despierta** después.
- Generación: probabilidad configurable de que un colono nazca con raíz (por defecto ~25%).

### 2.4 Reinos: `CultivationRealmDef` + hediffs
Cada reino es un **Def propio** con un **HediffDef de 4 etapas**:
- El hediff aplica los stats (vida, salud, `statOffsets`) de forma nativa: se ve en la pestaña de salud y otros mods lo entienden.
- El hediff lleva el **aura** con `renderNodeProperties` (sin Harmony). Color por elemento con un `PawnRenderNode` propio (`ColorFor`).
- El Def define: orden, qi base, progreso por etapa, requisito de avance (objeto, tribulación sí/no) y técnicas desbloqueadas.

**Caminos** (`CultivationPathDef`, los Tres Tesoros de DISENO.md §4.1b): cada reino tiene `path`; dentro de su camino
los reinos se recorren por `order` (sin tope en el código: los 6 del diseño se agregan con XML). El comp guarda un
`RealmTracker` **por camino** (reino, etapa, progreso, recurso, calidad del fundamento), así el doble cultivo cabe
sin reescribir. El camino define:
- `workerClass` (`CultivationPathWorker`): quién puede seguirlo (`requiresSpiritRoot` para el qi; el cuerpo de M4b
  tendrá su propia regla).
- `resourceClass` (`CultivationResource`): el recurso del camino. `StatResource` usa `resourceMaxStat` /
  `resourceRegenStat` (el qi: `CVG_QiMax` / `CVG_QiRegen`); la sangre-qi de M4b podrá ser otra subclase (se llena
  comiendo/entrenando) sin tocar el resto.
- `usesQiConcentration`, `foundationQualityCurve`, `foundationAidPenalty`.

Solo existe `CVG_Path_Qi`; `CVG_Path_Body` llega en M4b y Shen después.

Los campos `breakthrough*` y `tribulation*` describen el avance **para entrar** a ese reino:

```xml
<Convergence.CultivationRealmDef>
  <defName>CVG_Realm_Foundation</defName>
  <label>Foundation Establishment</label>
  <order>2</order>
  <hediff>CVG_Hediff_Foundation</hediff>
  <progressPerStage>2500</progressPerStage>
  <breakthroughItem>CVG_FoundationPill</breakthroughItem>   <!-- la toma antes del retiro si hay una -->
  <breakthroughChanceFactor>1</breakthroughChanceFactor>
  <breakthroughHours>4</breakthroughHours>
  <breakthroughCooldownDays>2</breakthroughCooldownDays>
  <breakthroughFailureHediff>CVG_QiDeviation</breakthroughFailureHediff>
  <tribulation>false</tribulation>                         <!-- Núcleo Dorado: true + tribulationStrikes/Damage... -->
  <breakthroughRequirements>                               <!-- opcional, ampliable con Class= -->
    <li Class="Convergence.BreakthroughRequirement_Stat"><stat>CVG_QiPurity</stat><min>4</min></li>
  </breakthroughRequirements>
</Convergence.CultivationRealmDef>
```

- **Requisitos de avance** (`BreakthroughRequirement`, lista en XML con `Class=`): ya existen `_Stat` (mínimo de un
  stat: los atributos de fundamento serán stats) y `_Realm` (haber alcanzado un reino, de cualquier camino). Ningún
  reino los usa todavía.
- **Calidad del fundamento** (根基): al subir se guarda en el tracker. Hoy = curva(probabilidad sin píldoras) − 0.15
  si usó píldoras, entre 0.5 y 1.3. `Hediff_CultivationRealm` escala los stats de la etapa: offsets × calidad,
  factores 1 + (f − 1) × calidad. Se ve en la pestaña Cultivo y en el tooltip del hediff. Pendiente: sumar el exceso
  sobre los mínimos de atributos y el lugar; más riesgo en el próximo avance si es inestable.
- **Curva de poder:** cada reino **multiplica** velocidad, daño cuerpo a cuerpo, daño recibido, vida y armadura
  (tabla en §5.3). La supresión de reino queda para después.

### 2.5 Stats propios (`StatDef`)
| Stat | Uso |
|---|---|
| `CVG_QiMax` / `CVG_QiRegen` | Recurso |
| `CVG_CultivationSpeed` | Progreso al cultivar |
| `CVG_BreakthroughChance` | Éxito al avanzar de reino |
| `CVG_DaoHeartResilience` | Resistencia del Corazón del Dao |
| `CVG_CelestialDissonance` | **Disonancia celestial**: los implantes la suben o bajan con `statOffsets` en su hediff. Endurece la tribulación |
| `CVG_TechniqueCost` | Multiplicador del coste de las técnicas |

Así genes, implantes, equipo, píldoras y formaciones modifican el cultivo **solo con XML** (patrón de The Force).

### 2.6 Cultivar (meditación propia)
La meditación del juego depende de Royalty, así que hacemos la nuestra:
- **Tipo de trabajo `CVG_Cultivation`** en la pestaña de trabajo (el jugador decide quién cultiva y con qué prioridad).
- **Edificio "cojín de cultivo"** + `WorkGiver_Cultivate` + `JobDriver_Cultivate` (pattern Job/WorkGiver/JobDriver).
- La ganancia depende de `CVG_CultivationSpeed`, la calidad de la raíz y la densidad de qi del lugar (vetas de qi, más adelante formaciones).
- Con Royalty: integración opcional con la meditación vanilla.

### 2.7 Avance de reino y tribulación
1. Al llenar la etapa 4 → **cuello de botella** (deja de progresar).
2. Botón (gizmo) **"Intentar avance"**: consume el objeto requerido (píldora) si lo hay.
3. Tirada con `CVG_BreakthroughChance` y Corazón del Dao.
4. Si el reino requiere **tribulación**: un `MapComponent` ejecuta la secuencia (aviso, rayos sobre el colono durante
   unos segundos, intensidad según el reino y la disonancia). Si sobrevive → siguiente reino.
5. Fallo → herida grave, **desviación de qi** (hediff) o muerte (si los ajustes lo permiten).

### 2.8 Técnicas (M5, inspiradas en Vanilla Psycasts Expanded)
- **Manual** (`TechniqueManualDef`): un árbol de técnicas de un camino (`path`) y un elemento opcional (`element`,
  `requiresElement`, `minRealm`). No lista sus técnicas: cada `AbilityDef` apunta a su manual (referencia inversa, como
  los caminos de VPE), así cualquier mod agrega técnicas solo con XML.
- **Técnica** = `AbilityDef` **vanilla** + `Convergence.TechniqueExtension` (modExtension): `manual`, `tier` (fila),
  `order`, `minRealm`, `prerequisites` (basta **cualquiera**), `requiresElement`, `unlockCost`, `masteryXpPerUse`,
  curvas opcionales `realmPowerCurve` / `masteryPowerCurve`, `npcCommonality`.
- **Comps genéricos** (nada de una clase por técnica):
  - `CompProperties_AbilityResourceCost { path, cost }`: cobra el recurso del camino (abstracción `CultivationResource`
    de M4) × stat `CVG_TechniqueCost` × maestría; en técnicas exige los requisitos (motivo en el gizmo gris), suma
    maestría, acorta el enfriamiento y esconde el gizmo si no está fijada. `CompProperties_AbilityQiCost { qiCost }`
    (capacidades innatas de Races) es la misma clase con el camino del qi.
  - `CompProperties_AbilityTechniqueProjectile`: proyectil (`Projectile_TechniqueBullet` / `_TechniqueExplosive`) con
    el daño × poder.
  - `CompProperties_AbilityTechniqueHediff`: hediff al objetivo o al lanzador; duración × poder^`durationPowerExponent`
    y los comps `ITechniquePowered` reciben el poder (escudo de qi: `HediffComp_QiShield`, capacidad × poder).
  - `CompProperties_AbilityCone` / `GiveHediffInRadius` (de Races) escalan con el mismo poder.
- **Poder** = reino (curva general de `AbilityPower` o la de la técnica) × maestría × afinidad (+15% si la raíz tiene el
  elemento del manual; la maestría sube ×1.5 más rápido).
- **Progreso** (`TechniqueTracker` dentro de `CompCultivator`): manuales, técnicas desbloqueadas, XP de maestría por
  técnica, **puntos de comprensión** (1 al empezar, +1 por etapa, +2 por reino; partidas viejas reciben los de su
  progreso) y técnicas fijadas en la barra. Niveles de maestría = `TechniqueMasteryDef` (XML: principiante → competente
  → experto → maestro → incomparable: poder ×1–1.6, coste ×1–0.7, enfriamiento ×1–0.6).
- **Aprender**: tablilla de jade (`CVG_JadeSlip`, `CompUseEffect_LearnTechniqueManual`; manual fijo o al azar, no se
  gasta; la venden los comerciantes exóticos) → el manual y su primera técnica gratis; el resto con puntos en la
  pestaña **Técnicas**.
- **IA**: `aiCanUse` vanilla (proyectiles y conos ofensivos; escudos/buffs con `ai_IsOffensive=false` solo en combate).
  Los cultivadores generados de otras facciones nacen con un manual afín a su raíz y técnicas según su reino (hook en
  `PawnGenerator.GeneratePawn`).
- **Supresión de reino** (DISENO.md §4.1c, DEFENSA.md §1): `CompCultivator.PostPreApplyDamage` (sin Harmony, O(1)):
  grado del ataque = `CompSpiritGrade` del arma (vanilla 0; stat `CVG_SpiritGrade` en la ficha) o, en técnicas, puños y
  armas naturales, el reino del atacante. Si es menor que el reino del objetivo, el daño × curva del XML
  (`DefModExtension_RealmSuppression` en el StatDef) con la fuerza del ajuste (0 = desactivada). Daño sin instigador
  (fuego, caídas, tribulación) no se suprime. Después actúan los escudos de qi (antes que la armadura).

### 2.9 Interfaz
- **Barra de qi** (gizmo) al seleccionar un cultivador.
- **Pestaña "Cultivo"** (`ITab`) en todos los humanoides: raíz, reino, etapa, progreso, Corazón del Dao y técnicas.
  Solo con `Widgets`/`Listing_Standard` (portable a 1.7).
- **Ajustes del mod** (`Mod` + `ModSettings`): modo de la Convergencia, días, probabilidad de raíz, muerte en
  tribulación, multiplicador de disonancia, activar el Maestro.

### 2.10 Objetos del MVP
- **Piedra espiritual** (recurso; combustible futuro de formaciones).
- **Píldora de Fundación** (requisito para pasar a Fundación).
- **Capas de ropa nuevas:** `CVG_Neck` y `CVG_Waist` (`ApparelLayerDef`), listas para el jade del Maestro.

## 3. Evento de la Convergencia (MVP)
- `IncidentDef CVG_Convergence` + `IncidentWorker`: carta, efecto visual simple (flecks y rayos vanilla), despierta raíces.
- `ScenPart` para el escenario de inicio inmediato (útil también para probar rápido).
- Después: vetas de qi en el mapa, investigación del Dao, el Maestro.

## 4. Rendimiento y compatibilidad
- Lógica en `CompTickRare` / `IsHashIntervalTick`; nada de búsquedas por tick.
- Integraciones con otros mods por **reflexión + carpetas condicionales** (patrón de Isekai/KotOR), nunca dependencias duras.
- Textos en `Keyed` / `DefInjected` desde el día uno: inglés + español (España y Latinoamérica).

## 5. Hitos de desarrollo

| # | Hito | Se puede probar |
|---|---|---|
| M1 ✅ | Esqueleto: mod carga, Harmony, ajustes, pestaña vacía | **Probado 2026-10-15**: carga sin errores, 4 razas con cultivo |
| M2 🔧 | Raíz espiritual + cuerpos especiales + Convergencia | **Implementado, falta probar en el juego** (ver §5.1) |
| M3 🔧 | Qi + cultivo (cojín, trabajo) | **Implementado, falta probar en el juego** (ver §5.2) |
| M4 🔧 | Reinos + avance + tribulación | **Implementado, falta probar en el juego** (ver §5.3) |
| M4b 🔧 | Camino del cuerpo + bestias espirituales | **Implementado, falta probar en el juego** (ver §5.5) |
| M5 🔧 | Técnicas + supresión de reino | **Implementado, falta probar en el juego** (ver §5.4). El aura queda pendiente |
| M6 🔧 | El Maestro (versión mínima) | **Implementado, falta probar en el juego** (ver §5.6). Un arquetipo: Anciano Yun |

### 5.1 Cómo probar M2

Implementado:
- **Raíz espiritual**: 4 calidades (inferior, común, superior, celestial) y 5 elementos (metal, madera, agua, fuego, tierra).
  Con Biotech se da como **genes** (categoría "cultivo"); sin Biotech, como el rasgo **raíz espiritual**.
  Probabilidad configurable (25% por defecto) al generar cualquier humanoide.
- **11 cuerpos especiales** como rasgos raros (Yin Puro, Yang Puro, Nueve Yang, Espada Celestial, Jade Inmortal, Caos,
  Venenoso, Espiritual Innato y, solo sin raíz, Acero, Tribulación y Espada). Sin efectos todavía (llegan con M3/M4b).
- **La Convergencia**: carta, aurora de un día y rayos. Se programa según el ajuste (narrador entre los días X–Y,
  inmediata, o por la investigación **"estudio de la anomalía espiritual"** en la pestaña de investigación **Cultivo**).
- **Pestaña Cultivo**: estado de la Convergencia, raíz (latente/despierta) y cuerpo especial.

Pasos:
1. Opciones → activar **modo desarrollador**. Partida nueva (los colonos se generan con la probabilidad de raíz).
2. Seleccionar un colono → pestaña **Cultivo**: debe decir que el qi duerme y mostrar raíz/cuerpo si tiene.
3. Menú de debug (icono de bicho) → buscar **Convergence**:
   - **Give random spirit root** → clic en un colono: le da una raíz.
   - **Trigger Convergence**: carta + aurora + rayos; la pestaña pasa a "despierta".
4. Guardar y cargar la partida: la raíz y el estado de la Convergencia deben conservarse.
5. Revisar `Player.log` por errores.

### 5.2 Cómo probar M3

Implementado:
- **Stats** (categoría *Cultivo* en la ficha del colono, solo visibles en cultivadores despiertos):
  `CVG_QiMax` (100), `CVG_QiRegen` (2 qi/h) y `CVG_CultivationSpeed` (100%).
- **Qi** (`QiTracker`): se regenera cada 250 ticks si el colono tiene raíz y ya ocurrió la Convergencia.
- **Reino** (`RealmTracker` + `CultivationRealmDef CVG_Realm_QiRefining`): etapas 1-4, 1000 de progreso por etapa.
  Al llenar la etapa 4 queda en **cuello de botella** (el avance de reino es M4). El hediff `CVG_Hediff_QiRefining`
  (severidad = etapa) muestra el reino en *Salud* y suma qi máximo/regeneración por etapa.
- **Trabajo Cultivar** (`CVG_Cultivation`, pestaña de trabajo) + **cojín de cultivo** (Mobiliario, 30 de madera, sin
  electricidad; gráfico provisional: taburete vanilla teñido). Progreso/hora = 50 × velocidad de cultivo × calidad de
  la raíz (0.6–2.2) × factor de concentración de qi. Mientras cultiva, el qi se recupera ×3.
- **Concentración de qi** (`RoomStatDef CVG_QiConcentration`): 1 ambiental + qi de los edificios con
  `DefModExtension_FengShui` (el cojín aporta 2). En una habitación cerrada es un stat de habitación; al aire libre se
  suman los edificios a 2.9 casillas. Curva concentración → factor en el XML (1 → ×1, 3 → ×1.15, 5 → ×1.3…).
  **Pendiente:** armonía de los cinco elementos (el elemento ya está en el XML pero no se usa), vetas de qi, formaciones.
- **UI**: barra de qi (gizmo) al seleccionar un colono cultivador; la pestaña *Cultivo* muestra reino, etapa,
  barra de progreso, qi y el aviso de cuello de botella.

Pasos:
1. Modo desarrollador. Partida con un colono con raíz (o debug **Give random spirit root**) y **Trigger Convergence**.
2. En unos segundos: pestaña *Cultivo* → "Refinamiento de Qi, etapa 1/4" y barra de qi; en *Salud* aparece el hediff.
   La ficha del colono (i) muestra la categoría *Cultivo* con los 3 stats.
3. Construir un **cojín de cultivo** (Mobiliario). En la pestaña de trabajo activar **Cultivar** para ese colono
   (en partidas viejas la columna nueva empieza desactivada).
4. El colono se sienta en el cojín: barra de progreso sobre él, iconos de meditación, el qi sube rápido.
   Al pasar el ratón por la habitación debe verse *concentración de qi* (3 con un cojín en una habitación cerrada).
5. Debug **Add cultivation progress** (una etapa por clic): mensajes de etapa; a la 4.ª llena, mensaje de
   **cuello de botella** y el colono deja de cultivar. **Fill qi** llena la barra.
6. Guardar y cargar: qi, etapa y progreso se conservan. Revisar `Player.log`.

### 5.3 Cómo probar M4

Implementado:
- **Arreglos de M3**: la barra de qi usa el gizmo de barra vanilla (`Gizmo_Slider`, como hemogén/psyfocus): título
  "Qi", barra horizontal de 160 px con "actual / máximo"; reino, etapa y regeneración en el tooltip.
  **Equilibrio del qi**: regeneración base 4/h y +1/h por etapa (en reposo se llena en ~1 día en todos los reinos);
  cultivar en el cojín multiplica la regeneración ×5 × factor de concentración (~4-5 h para llenarse). Cultivar no
  gasta qi; el qi se gasta en el **avance** (y en técnicas, M5). En cuello de botella el colono sigue cultivando
  solo para llenar el qi.
- **Caminos**: `CultivationPathDef CVG_Path_Qi`, un `RealmTracker` por camino con su recurso (ver §2.4). Las
  partidas de M3 se migran solas (el reino y el qi viejos pasan al tracker del qi).
- **Reinos 2 y 3** con curva de poder **multiplicativa** (etapa 1 → 4; × calidad del fundamento):

  | Reino | Qi máx | Salud máx. | Velocidad | Daño c. a c. | Daño recibido | Armadura | Longevidad | Otros |
  |---|---|---|---|---|---|---|---|---|
  | Refinamiento de Qi | 100-160 | ×1.2-1.35 | ×1.05-1.15 | ×1.1-1.3 | ×0.95-0.85 | — | ×1.2 | |
  | Establecimiento de Fundación | 300-420 | ×2-2.3 | ×1.3-1.4 | ×1.7-2.0 | ×0.65-0.6 | +15-20% | ×2 | curación ×1.5, inmunidad ×1.3, dolor +15% |
  | Núcleo Dorado | 600-900 | ×4-4.6 | ×1.6-1.75 | ×3-3.5 | ×0.4-0.35 | +35-45% | ×4 | curación ×2.2, inmunidad ×1.6, dolor +30% |

  **Salud máxima**: `healthScaleFactor` del reino (etapa 1 ~ última) × calidad del fundamento, aplicado con un
  postfix a `Pawn.HealthScale` (vida de todas las partes; también escala sangrado y dolor por herida como un
  cuerpo más grande). El factor se cachea en el comp y se recalcula al cambiar etapa/reino: las heridas guardan daño
  absoluto, así que subir de reino ni cura ni mata de golpe. Los stats de la tabla los aplica vanilla desde la etapa
  del hediff (`HediffStatsUtility`, que lee `CurStage`, sobrescrito por `Hediff_CultivationRealm` con la calidad).
- **Calidad del fundamento** al subir (sólido / estable / inestable): ver §2.4. La píldora sube la probabilidad
  pero resta calidad.
- **Pestaña "Cultivo" del Arquitecto** (`DesignationCategoryDef CVG_Cultivation`): el cojín se mudó ahí; todo lo
  construible del mod irá en ella.
- **Botón "Intentar avance"** (solo en cuello de botella; desactivado con motivo si falta qi, hay enfriamiento,
  está derribado o ya en retiro). Su tooltip muestra el desglose de la probabilidad y si tomará la píldora.
  Trabajo `CVG_Breakthrough`: (toma la píldora si hay) → va al cojín libre más cercano (o se queda donde está) →
  **retiro** de 4 h (Fundación) / 6 h (Núcleo Dorado) con barra de progreso; el qi se vacía durante el retiro.
- **Probabilidad** = `CVG_BreakthroughChance` (base 50%, la píldora suma +30%) × dificultad del reino
  (Fundación ×1, Núcleo ×0.7) × raíz (0.8 / 1 / 1.15 / 1.3) × concentración de qi (curva en `CVG_QiConcentration`),
  entre 2% y 95%.
- **Resultado**: éxito → siguiente reino (etapa 1), carta y destello. Fallo → `CVG_QiDeviation` (≈2 días:
  dolor, -15% conciencia, cultivo y regeneración ×0.5) y enfriamiento (2-3 días) antes de reintentar.
- **Tribulación** (`MapComponent_Tribulation`, solo al entrar a Núcleo Dorado): carta de aviso y, tras 3 s, rayos
  vanilla (`WeatherEvent_TribulationStrike`: destello, rayo y trueno sin la explosión de fuego) cada 1-2 s.
  6 rayos de 10 de quemadura (antes del multiplicador de daño recibido del reino); cada punto de `CVG_CelestialDissonance` (stat nuevo, base 0; los implantes lo subirán
  con `statOffsets`) × el ajuste de disonancia suma +2 rayos y +50% de daño. En pie al final → sube. Derribado → no
  sube y queda con desviación de qi. Con "las tribulaciones pueden matar" desactivado, los rayos solo queman partes
  exteriores no vitales y paran al derribarlo.
- **Píldora de Fundación** (provisional hasta el módulo de alquimia): en el **lugar de fabricación** (crafting
  spot), 2 medicinas herbales + 15 de plata, sin investigación. Al tomarla: hediff "píldora de fundación absorbida"
  (1 día, +30% de probabilidad), que se gasta en el intento.

Pasos:
1. Modo desarrollador. Colono con raíz despierta (debug **Give random spirit root** + **Trigger Convergence**).
   Menú de debug (icono del bicho): el **buscador filtra por nombre de acción**; escribir `Convergence` muestra todas.
   Las herramientas de pawn ("tool") se usan haciendo **clic en el colono** después de elegirlas (clic derecho o
   Esc para soltarlas).
2. Seleccionar el colono: barra **Qi** ancha, con el tooltip de reino/etapa/regeneración.
3. **Add cultivation progress** ×4 → cuello de botella. Aparece **Intentar avance** (desactivado: falta qi).
   **Fill qi** → se activa; pasar el ratón muestra la probabilidad.
4. (Opcional) Construir un cojín (pestaña **Cultivo** del Arquitecto) y fabricar una **píldora de fundación** en el lugar de fabricación: el tooltip
   pasa a decir "Antes tomará: píldora de fundación (+30%)".
5. Clic en **Intentar avance**: toma la píldora, va al cojín, barra de retiro de 4 h. Al terminar: carta de éxito
   con la calidad del fundamento y 2-4 líneas "antes → después" (salud máxima, velocidad, daño c. a c., daño
   recibido, qi). Pestaña *Cultivo* → "Fundamento: …" y la sección **Bonus del reino** (lista legible de lo que da
   la etapa actual). *Salud* → Establecimiento de Fundación, etapa 1; las partes del cuerpo muestran el doble de
   vida (p. ej. torso 80 en vez de 40). Debug **Log cultivation stats** (clic en el colono) escribe en el log
   HealthScale y los stats principales para comparar antes/después. o de fallo (desviación de qi en *Salud*,
   enfriamiento en la pestaña). **Reset breakthrough cooldown** permite reintentar enseguida.
6. Núcleo Dorado: otra vez **Add cultivation progress** ×4 + **Fill qi** + **Intentar avance** (6 h). Si la tirada
   sale bien llega la carta de **tribulación** y caen los rayos. Probar con el ajuste de muerte activado y
   desactivado.
7. Atajos: **Force breakthrough** (sube al siguiente reino sin tirada ni tribulación) y **Start tribulation**
   (tribulación directa hacia el reino siguiente).
8. Guardar y cargar durante el retiro y durante la tribulación. Cargar también una partida de M3 con un
   cultivador: debe conservar reino, etapa y qi. Revisar `Player.log`.

### 5.4 Cómo probar M5

Implementado (ver §2.8):
- **5 manuales** (uno por elemento) con 3 técnicas cada uno (filas 1 y 2: Refinamiento de Qi; fila 3: Fundación y
  raíz del elemento, 2 puntos):

  | Manual | Fila 1 (proyectil) | Fila 2 | Fila 3 (cono) |
  |---|---|---|---|
  | Canon del Filo Dorado (metal) | Aguja dorada (perforante, 45% PA) | **Escudo de la campana dorada** (escudo de qi 40 × poder, 3 h) | Barrido de qi de espada |
  | Sutra Siempreverde (madera) | Espina venenosa (+ toxina) | Rejuvenecimiento primaveral (aliado: curación ×4, sangrado ×0.5) | Tormenta de espinas |
  | Manual de la Nube Fluyente (agua) | Esquirla de escarcha (congelación) | **Paso de la nube veloz** (salto vanilla de 12 casillas) | Aliento helado (aturde, apaga fuego) |
  | Arte del Sol Abrasador (fuego) | Bola de fuego (explosión 1.5, incendia) | Meridianos ardientes (c. a c. ×1.3, velocidad ×1.2) | Mar de llamas |
  | Método del Corazón de la Montaña (tierra) | Bala de piedra (50% aturdir) | Cuerpo de montaña (armadura +30%, velocidad ×0.85) | Temblor de tierra |

- **Pestaña Técnicas**: manuales en columnas, filas por tier, líneas a los prerequisitos (blanca si ya se tiene), nodo
  resaltado = se puede aprender (clic gasta puntos), velado = bloqueado (el tooltip dice por qué), maestría debajo,
  casilla = poner la técnica activa en el conjunto activo; las pasivas llevan borde dorado. Botón de desarrollo "+1".
- **Modelo de Vanilla Psycasts Expanded (ronda 3, decisión del usuario)**. Ya no hay pestaña Técnicas: la pestaña
  Cultivo tiene un botón **Desarrollo** que abre `Dialog_CultivationDevelopment`, con la **estructura de la pestaña de
  VPE** (decisión del usuario: copiar lo que funciona):
  - Izquierda: nombre, "Reino: X, etapa N" con barra de progreso y cómo se gana, puntos de comprensión disponibles y en
    qué se gastan, stats de cultivo con icono por fila (desglose vanilla en el tooltip), la raíz como fila de los cinco
    elementos (los de la raíz, brillantes), los **conjuntos de combate** (arrastrar técnicas aprendidas desde el árbol,
    clic derecho quita, "Crear conjunto", el activo se edita y se resalta) y un buscador que oscurece lo que no coincide.
  - Derecha: pestaña "Técnicas" con los caminos como **cartas verticales con arte** (`cardTexPath`,
    `UI/Paths/CVG_Path_<Nombre>`; mientras falte, degradado del color del camino y su glifo), hasta 5 por fila con scroll
    vertical. Cerrada: arte oscurecido y botón **Desbloquear (1 punto)** si cumple el requisito (raíz del elemento,
    seguir el camino, gen), o "Bloqueado" con el motivo en el tooltip; las de tablilla se abren al leerla. Abierta: el
    árbol dibujado encima (nivel 1 abajo), nodos con brillo que late si se pueden aprender, velo si no, borde dorado si
    son pasivas; clic = aprender.
  Contenido:
  - **Caminos** en columnas (`TechniqueManualDef` = camino), **5 niveles** cada uno como VPE (nivel 1-2 Refinamiento de
    Qi, 3 Fundación, 4 Núcleo Dorado, 5 reservado al Alma Naciente), líneas a los prerequisitos, **1 punto por técnica**
    (o 2 en los niveles altos). Los puntos salen de las etapas y reinos de todos los caminos de cultivo (un solo fondo,
    como el nivel de VPE); la maestría mejora las técnicas al usarlas (las pasivas, con el tiempo).
  - **La raíz abre los caminos elementales** (`openedByRoot`): cada elemento de la raíz abre el suyo (más elementos =
    más caminos, pero la raíz impura cultiva más lento, como siempre): desbloquearlo cuesta 1 punto, como en VPE. Las
    **tablillas de jade** abren los caminos extra directamente y regalan su primera técnica (`fromJadeSlips`: Canon del
    Corazón de la Espada, Sutra del Sentido Divino); el **Método del Cuerpo Vajra** se desbloquea al seguir el camino del
    cuerpo (`openedByFollowing`); un gen también puede habilitar uno (`openedByGenes`). Los cultivadores que no son del
    jugador abren solos los caminos de su naturaleza.
  - **Pasivas y activas**: 24 pasivas (`TechniqueExtension.passiveHediff`: hediff permanente cuya severidad es el poder,
    reino × maestría × afinidad) y 13 activas. Por camino: Metal (aguja, piel de campana dorada / escudo / tajo de qi /
    núcleo dorado del metal), Madera (espina, aliento perenne / rejuvenecimiento / tormenta de espinas, corazón del
    árbol / pino de diez mil años), Agua (esquirla, pasos de nube / paso veloz / aliento helado, leer la marea / mar sin
    orillas), Fuego (bola, horno interior / meridianos / mar de llamas, corazón del sol / brasa del fénix), Tierra (bala,
    médula de piedra / cuerpo de montaña / temblor, montaña enraizada / montaña sin fin), Espada (filo del corazón /
    espada voladora / intención de la espada), Sentido Divino (mente de agua quieta / sentido divino), Cuerpo Vajra
    (camisa de hierro / fuerza del dragón y el elefante / huesos irrompibles), **Trueno de los Nueve Cielos** (paso de
    relámpago, reflejos de relámpago / rayo en cadena, corazón de la tormenta / rugido del trueno, cuerpo de relámpago /
    nacido de la tribulación; balance del Staticlord de VPE).
  - **Raíz de Trueno (雷)**, variante celestial como en Tale of Immortal (`QiElementDef.variant`): nunca sale en el sorteo
    normal; con un 15% reemplaza a Madera+Fuego cuando una raíz tiene los dos (~2% de las raíces), y la sangre del
    **Qilin del Trueno** la puede despertar al alcanzar la pureza Pura (25%) o Ancestral (50%)
    (`BloodlinePurity.rootElement`). Los rayos de la tribulación le hacen ×0,6 (`tribulationDamageFactor`).
  - **Conjuntos de combate** (psysets de VPE, `TechniqueSet`): uno automático por camino abierto con activas, más los
    que haga el jugador (hasta 8), de 3 técnicas (4 desde la Fundación, `CultivationPathDef.activeTechniqueSlots`).
    Solo las del conjunto activo tienen gizmo y **solo reclutado**; un botón (también solo reclutado) cambia de conjunto.
  - **Balance de VPE** (psyfoco → qi con 1 = 400 qi; calor neural → enfriamiento a 0,54/s): proyectiles 10 qi y 22 s
    (pico de hielo, bola de llamas), conos 32 qi, 57 s y alcance 9,9 (aliento de fuego/hielo), escudo 8 qi, 46 s y 30 s
    de duración (escudo de hielo/fuego), paso veloz 8 qi y 15 s (salto potente). El reino escala el poder.
  - **Partidas viejas**: las técnicas aprendidas abren su camino (aunque hayan cambiado de camino), las fijadas pasan a
    un conjunto "Combate", y las que ahora son pasivas pierden el botón y ganan su hediff (`SyncAbilities`).
  - El botón de avance se esconde reclutado. Revisión visual: `-cvgdevshot` guarda una captura de la ventana.
- **IA enemiga** (`Techniques/CultivatorAI.cs`): `CompProperties_AbilityAIConditions` (distancia mínima/máxima, salud
  del lanzador, enemigos cerca del objetivo, no repetir un escudo activo) en las bases de proyectil (no a quemarropa) y
  cono (solo contra 2+ enemigos o desesperado) y en los escudos; un prefijo a `JobGiver_AIFightEnemy.GetAbilityJob`
  hace que los cultivadores NPC levanten el escudo de qi o la coraza de sangre antes de atacar. Nadie empieza un avance
  (tribulación) reclutado o con enemigos a 30 casillas, ni sesiones de cultivo o templado con enemigos a 20.
- **Longevidad**: `CultivationRealmDef.biologicalAgingFactor` (etapa 1 → última) multiplica
  `Pawn_AgeTracker.BiologicalTicksPerTick` (el valor que usan los genes, como Ageless) en adultos de 20+ años: Qi
  ×0,85-0,7, Fundación ×0,5-0,35, Núcleo Dorado ×0,15-0,05 (casi sin edad); el cuerpo de ×0,9 a ×0,05. Los niños
  crecen normal y el rejuvenecimiento de Biotech sigue funcionando; `LifespanFactor` de los hediffs de reino sigue
  bajando las enfermedades de la edad. Se ve en la pestaña Cultivo.
- **Ajuste** "Fuerza de la supresión de reino" (0–200%). Curva: 1 reino de diferencia ×0.85, 2 ×0.5, 3 ×0.2, 4 ×0.08.
- **Iconos provisionales**: los de elemento de los genes de raíz. Arte propio en `Textures/UI/Techniques/<defName>.png`
  (lista en docs/ARTE_BACKLOG.md): se usa solo al arrancar, sin tocar XML.
- **Validador**: `tools/DefValidator` (reflexión sobre los tipos reales del juego): campos, `Class=`, enums, números,
  `compClass` nulo, Defs inexistentes o solo de DLC, DefInjected y Keyed (incluidas las claves `"CVG_*".Translate()` del
  código). `dotnet build tools/DefValidator/DefValidator.csproj -c Release` y luego
  `tools/DefValidator/bin/DefValidator.exe "<carpeta RimWorld>" <repo>`.

Pasos:
1. Modo desarrollador. Colono con raíz despierta (**Give random spirit root** + **Trigger Convergence**); **Fill qi**.
2. Debug **Convergence: Spawn jade slip** → elegir un manual y hacer clic en el mapa. Clic derecho del colono sobre la
   tablilla → "Leer tablilla de jade (…)" (10 s): mensaje "ha aprendido el … Primera técnica: …".
   Alternativa: **Convergence: Learn manual** (clic en el colono).
3. Pestaña **Técnicas**: la fila 1 aprendida, la fila 2 resaltada si hay puntos (1 al empezar; **Add technique point**
   o "Dev: +1 point" para más). Clic → la aprende. La fila 3 velada: el tooltip pide Fundación y la raíz del elemento.
4. Reclutar (draft) al colono: gizmos de las técnicas fijadas con el coste ("8 qi"). Lanzar la bola de fuego o la aguja
   contra un enemigo: el qi baja y empieza el enfriamiento; el tooltip muestra poder, maestría y grado espiritual.
   **Escudo de la campana dorada**: burbuja azul alrededor del colono y el hediff en *Salud* con "restante / máximo";
   los golpes la gastan antes que la armadura y al romperse sale un mensaje.
   **Paso de la nube veloz**: elegir una casilla a ≤12; el colono salta.
5. **Unlock all techniques** (todos los manuales sin requisitos) y **Add mastery** (un nivel de maestría a todas: sube
   el poder y bajan coste y enfriamiento, visible en el gizmo y la pestaña). **Force breakthrough** a Fundación →
   "+2 puntos" y la fila 3 se puede aprender si la raíz tiene el elemento.
6. Supresión: con el colono en Núcleo Dorado (Force breakthrough ×2), que le disparen con un rifle: el daño debe ser
   muy bajo (≈8% del normal junto con el multiplicador del reino). Las técnicas o los puños de un cultivador sí hacen
   daño según su reino. Con el ajuste en 0% el daño vuelve a ser normal. La tribulación y el fuego no se suprimen.
7. Enemigos: con la Convergencia ya ocurrida, los humanos con raíz de una incursión traen técnicas (en modo
   desarrollador se ve su pestaña Técnicas) y las usan en combate (proyectiles, conos, escudo al empezar).
8. Las capacidades innatas de Races (p. ej. llamas del fénix) siguen cobrando qi solo a quien cultiva.
9. Guardar y cargar: manuales, técnicas, maestría, puntos, fijadas y escudo activo (capacidad restante). Revisar
   `Player.log`.

### 5.5 Cómo probar M4b

Implementado:
- **Camino del cuerpo** (`CVG_Path_Body`, `CultivationPathWorker_Body`): lo siguen los humanos adultos **sin raíz**
  tras la Convergencia (también los cuerpos especiales sin raíz). `autoStart = false`: **no empieza solo**; empieza al
  comer carne o sangre de bestia espiritual, sopa espiritual o un núcleo (`IngestionOutcomeDoer_GainCultivationResource`,
  mensaje "ha empezado el camino del cuerpo"). Hasta entonces no tiene hediff, barra ni stats de cultivo.
- **Sangre-qi** (`CVG_BloodQiMax` 50 + reino; `CVG_BloodQiRegen` 0): **sin regeneración**, solo sale de productos de
  bestia: carne espiritual +2 por unidad, sangre +25, sopa +30 (+ "nutrido de espíritu": cultivo ×1.1 y templado ×1.15
  un día), núcleo +80 a todos los caminos.
- **6 reinos** (Templado de Piel → Carne → Forja de Huesos → Purificación de la Médula → Refinamiento de la Sangre →
  Cuerpo Dorado), 4 etapas, poder multiplicativo con más vida y armadura que el qi: vida ×1.3–1.45 … ×6–7, daño
  recibido ×0.92 … ×0.26, armadura +5% … +60%, c. a c. ×1.15 … ×4, curación/inmunidad desde la Médula, sangrado
  menor desde la Sangre. Cuerpo Dorado trae **tribulación**. Fallo → "reflujo de sangre-qi".
- **Avance**: el mismo sistema de M4 (botón "Intentar avance", retiro, probabilidad, fundamento). Nuevo
  `breakthroughItemRequired`: Carne y Huesos **exigen sangre de bestia**; Médula, Sangre y Cuerpo Dorado **exigen un
  núcleo de bestia** (se consume en el retiro; sin él el botón queda gris con el motivo).
- **Entrenamiento**: trabajo **Templar el cuerpo** (`CVG_BodyTempering`) en el **muñeco de madera** (madera 40; +XP
  de cuerpo a cuerpo; 10 sangre-qi/h) y las **piedras de pesas** (piedra 60; ×1.3 de progreso, 13 sangre-qi/h, cansa
  más). Pestaña Cultivo del Arquitecto. Progreso/h = 40 × `CVG_BodyTemperingSpeed` × aparato; cuerpo espiritual innato
  ×1.6, acero y espada ×1.3, tribulación ×1.2. Sin sangre-qi el trabajo termina.
- **Bestias espirituales** (`CVG_SpiritBeast`, `Hediff_SpiritBeast`, niveles 1-4): al ocurrir la Convergencia los
  animales del mapa pueden despertar (la carta dice cuántos) y los generados después también. Probabilidad y nivel
  por `combatPower` (curvas en el XML; ardilla ~3%, oso ~30%, thrumbo ~85%) × ajuste "Probabilidad de bestias
  espirituales". Nivel → velocidad, daño c. a c., daño recibido y **vida** (×1.3 … ×3.5). Sus mordiscos cuentan como
  grado = nivel en la supresión de reino. Al carnear: **carne espiritual** (8 × tamaño × nivel), **sangre de bestia**
  (2 × tamaño × nivel) y, desde nivel 3, **núcleos**. Categoría propia (las comidas normales no gastan la carne).
- **Sopa espiritual**: receta en fogata/cocinas (10 de carne espiritual).
- Debug: **Start body tempering** (empieza el camino y llena la sangre-qi), **Awaken spirit beast** (nivel 1-4, clic
  en un animal); **Add cultivation progress**, **Fill qi** (llena todos los recursos), **Force breakthrough** y **Reset
  breakthrough cooldown** usan el camino del cuerpo si el colono no cultiva qi.

Pasos:
1. Modo desarrollador, **Trigger Convergence**: la carta indica cuántos animales despertaron; en *Salud* de un animal
   aparece "bestia espiritual (nivel N)".
2. **Awaken spirit beast** (nivel 3) sobre un animal grande, matarlo y carnearlo: carne espiritual, sangre y núcleo.
3. Un colono **sin raíz**: comer carne espiritual (o **Start body tempering**). Mensaje de inicio; barra roja "sangre-qi",
   hediff "Templado de Piel" y la pestaña Cultivo con el camino del cuerpo.
4. Construir un **muñeco de madera** y activar **Templar el cuerpo**: el colono entrena, la sangre-qi baja y la etapa
   sube; se detiene sin sangre-qi.
5. **Add cultivation progress** ×4 → cuello de botella; **Fill qi**. "Intentar avance" gris pidiendo sangre de bestia
   hasta que haya una en el mapa; con ella, la toma y hace el retiro.
6. Cocinar **sopa espiritual** y comerla: +30 sangre-qi y "nutrido de espíritu".
7. Un colono **con raíz** no puede seguir este camino (sigue el qi). Guardar y cargar: camino, reino y bestias se conservan.

### 5.6 Cómo probar M6

Implementado (docs/LORE.md "El Maestro", docs/lore/MAESTROS.md §0-1.1):
- **`MentorDef`** (datos del Maestro: nombre, edad, rasgos solo positivos, raíz, reino/etapa/fundamento, manuales,
  hediffs, estancia, lecciones, objeto de despedida y textos) + **`GameComponent_Mentor`** (estado: quién, lección,
  protección usada, cuándo se va). Un arquetipo: **Anciano Yun Shouzhi** (anciano recto, raíz celestial metal+madera,
  Núcleo Dorado etapa 4 con fundamento 130% + "presencia de Alma Naciente": daño recibido ×0.5, c. a c. ×1.6,
  velocidad ×1.15, aprendizaje ×1.5, hambre ×0.25, descanso ×0.5, umbral de crisis −50%; conoce todas las técnicas
  del Filo Dorado y del Sutra Siempreverde). Se le quitan rasgos, raíces/linajes al azar, achaques y heridas; xenotipo
  baseliner con Biotech.
- **Llegada**: 2 h después de la Convergencia (ajuste "El Maestro llega con la Convergencia"). 30% **cae herido** (destello
  y daño hasta quedar derribado: hay que rescatarlo) / 70% **caminante** (entra por el borde). Se une a la colonia y
  la carta dice cuánto se queda (30–60 días: medio año a un año de RimWorld).
- **Lecciones** (`MentorLesson_*`, ampliables con XML; una a la vez, carta al empezar, mensaje y regalos al cumplirla;
  progreso en la pestaña **Cultivo** del Maestro): 1) *Un lugar para respirar*: construir un cojín de cultivo (6
  medicinas herbales); 2) *Escuchar el qi*: algún discípulo llega a la etapa 2 de un reino, qi o cuerpo (píldora de
  fundación); 3) *La primera técnica*: entrega una tablilla de jade; algún discípulo aprende una técnica (núcleo de bestia).
- **Protección**: la primera vez que hay en su mapa un humano hostil con reino (cultivador de qi o cuerpo), carta y
  "determinación del guardián" un día (c. a c. ×2, daño recibido ×0.3, velocidad ×1.3, apuntar ×0.5). Solo una vez.
- **Despedida**: al acabar la estancia (si está en pie) lo llama su secta: destello, sale de la colonia (pasa al mundo)
  y deja la **Hoja del Pico** (fragmento de jade con las pistas; los ecos y la cadena de búsqueda llegan después).
  Si muere: carta y el fragmento junto al cuerpo.
- Debug: **Spawn mentor** (walker / wounded), **Mentor complete lesson**, **Mentor leave now**.

Pendiente del diseño: maltrato (se va antes o se vuelve hostil), colgante de persona si muere, frases por etiqueta,
los otros 9 personajes y la cadena de búsqueda.

Pasos:
1. **Trigger Convergence**; ~2 horas de juego después llega el Maestro (carta). O **Spawn mentor** → walker / wounded.
2. Herido: rescatarlo y dejarlo sanar. Caminante: ya es colono. Pestaña *Cultivo* del Maestro: reino, técnicas,
   "Maestro (anciano recto): se va en N días" y la lección actual con progreso; *Salud*: presencia de Alma Naciente.
3. Carta "Lección 1": construir un cojín → mensaje y 6 medicinas junto al Maestro. Lección 2 (Add cultivation progress
   en un discípulo) → píldora. Lección 3: aparece una tablilla de jade; leerla → núcleo de bestia.
4. Incursión con un cultivador (debug: *Spawn pawn* hostil con raíz + Trigger ya hecho, o una incursión normal tras la
   Convergencia): carta de protección y el hediff de un día.
5. **Mentor leave now**: carta de despedida, el Maestro desaparece y queda la Hoja del Pico.
6. Guardar y cargar en cada fase (lección en curso, protección usada, Maestro ido). Revisar `Player.log`.

### 5.6b Comprensión del Dao y calendario celestial

**Comprensión del Dao** (docs/modulos/INVESTIGACION.md §1 y §3.1–3.2): proyectos de la pestaña de investigación
**Cultivo** con `DefModExtension_DaoResearch`.
- **Con Anomaly**: al arrancar pasan a ser proyectos de conocimiento de la categoría `CVG_DaoComprehension` (no
  necesitan mesa; un parche hace que no se oculten hasta el monolito). Se elige el proyecto en la pestaña (si no hay
  ninguno elegido, la comprensión va al primero disponible).
- **Sin Anomaly**: investigación normal en el **escritorio de contemplación** (pestaña Cultivo del Arquitecto); cultivar
  también suma (200 puntos por punto de comprensión) si el proyecto actual es del Dao.
- **Fuentes**: cultivar 0,25/h, templar el cuerpo 0,15/h, respirar como cultivador despierto 0,02/h, leer una tablilla
  1,5, absorber un núcleo de bestia 1. **Iluminación súbita** (0,4%/h practicando): +3 y carta.
- **Carta de iluminación** al completar un proyecto, nombrando a quien más aportó y cómo ("se iluminó mientras estaba
  cultivando y comprendió…").
- **Proyectos**: *Respiración del qi* (2; cojín de cultivo; el Maestro la enseña al llegar) → *Fundación* (6; avance a
  Fundación) → *Núcleo Dorado* (14; avance a Núcleo Dorado) → *Reinos secretos* (25; descubrimiento especial con carta;
  **TODO**: misiones de reino secreto). *Manuales de técnicas* (4; leer tablillas de jade). *Templado del cuerpo* (3;
  aparatos de entrenamiento y avance a Templado de Carne). Los bloqueos solo afectan al jugador (los PNJ no investigan).

- **Niveles 2–3 de la forja espiritual y del horno de alquimia** (Alchemy): proyectos *Plegado con qi*, *Tesoros
  espirituales*, *Fuego controlado* y *Tribulación de la píldora* (`WorkbenchLevel.requiredResearch`).

**Demonio interno** (crisis mental vanilla `CVG_InnerDemon`): solo cultivadores, mayor; ×5 bajo la Luna del Demonio
Interno. Vaga perdido como en el delirio y al acabar queda con desviación de qi (o reflujo de sangre-qi).

**Recreación del alma**: tablero de go (juego cerebral, dos jugadores, intelectual) y mesa de caligrafía (alta cultura,
artístico), en la pestaña Cultivo del Arquitecto; usarlos da 0,05 de comprensión por hora.

**Calendario celestial** (docs/EVENTOS.md §1): condiciones de mapa que solo empiezan tras la Convergencia
(`IncidentWorker_CelestialCondition`), efectos genéricos en `DefModExtension_CelestialCondition` aplicados por
`StatPart_CelestialCondition` (en velocidad de cultivo, regeneración de qi, coste de técnicas, probabilidad de avance y
velocidad de templado):
- **Marea de qi** (0.6, cada ≥20 días, 3–6 días): cultivo ×2, qi ×1.5, templado ×1.3, avance +10%, comprensión ×1.5.
- **Reflujo del qi** (0.35, ≥25 días, 2–4 días): cultivo ×0.4, qi ×0.5, técnicas ×1.3, comprensión ×0.7.
- **Luna del demonio interno** (0.25, ≥30 días, desde el día 15, 1–2.5 días): cultivo ×0.85, avance −15% y 2%/h de
  desviación de qi al cultivar.
- Pendientes: Eclipse Celestial, Conjunción de los Cinco Elementos, Lluvia Espiritual, Festival de los Fantasmas
  (y efectos sobre el Corazón del Dao cuando exista).
- Probar: debug → *Execute incident* → `CVG_QiTide` / `CVG_QiEbb` / `CVG_InnerDemonMoon`, y mirar la explicación del
  stat *velocidad de cultivo* en la ficha del colono.

### 5.7 Guía de prueba completa (todo lo que hay)

**Automática** (lo hacen los agentes; no toca tu configuración): carpeta de datos aislada con su propio
`Config/ModsConfig.xml` (Harmony, DLC, VEF y los mods Convergence en orden Core → Races → Resources → Furniture →
TraitsDestinies → Alchemy → Sects) y el argumento `-cvgselftest`:

```
RimWorldWin64.exe -quicktest -cvgselftest -savedatafolder=<carpeta temporal> -logFile <ruta>\rw.log
```

Corre `CoreSelfTest` (Convergencia, bloqueos y comprensión del Dao, calendario celestial, raíz y cultivo, avance, técnicas con lanzamiento real de las 14 no-salto, escudo,
supresión de reino, tribulación, camino del cuerpo con carne/sopa/muñeco/sangre de bestia, bestia espiritual y
carneado, el Maestro con sus lecciones y su despedida, spawn de todos los ThingDef Convergence y errores de
traducción del idioma activo, más humo de todos los mods Convergence: generar cada `PawnKindDef` (razas y discípulos
de secta con su reino y técnicas), listar las facciones de secta presentes, validar cada receta (mesas, productos,
ingredientes) y consumir cada ingerible: píldoras, hierbas, comidas, sangre, núcleos) y luego los autotests de recursos,
linajes y rasgos. Cada uno termina en `ALL OK` o
`FAILURES: n` en el log. Después acelera el juego para que corran ticks. Última pasada (2026-10-15): todo `ALL OK`
en inglés y en español con los 7 mods, sin errores ni warnings nuestros (los 3 errores de traducción de la carga en
español son de vanilla). Pendiente de Sects: 3 `Config error` de `weaponMoney` menor que el arma más barata
(`CVG_HeavenlyCauldron_Auctioneer`, `CVG_AncientDragon_Watcher`, `CVG_AncientDragon_Hoarder`). En partida normal se puede lanzar desde debug: **Convergence: Run core self-test** (marca la Convergencia y
trae al Maestro: úsalo en una partida de prueba).

**A mano** (30–40 min, modo desarrollador, menú de debug → buscar `Convergence`):
1. **Convergencia**: *Trigger Convergence* → carta (raíces, bestias despertadas) y 2 h después el **Maestro** (§5.6).
2. **Qi**: un colono con raíz (*Give random spirit root*) → cojín, trabajo Cultivar, *Add cultivation progress*,
   *Fill qi*, **Intentar avance** (§5.2–5.3).
3. **Técnicas**: *Spawn jade slip* → leer → pestaña **Técnicas**, desbloquear, lanzar reclutado; escudo y paso veloz
   (§5.4). Supresión: *Force breakthrough* ×2 y que le disparen.
4. **Cuerpo**: colono sin raíz, *Awaken spirit beast* en un animal, cazarlo y carnearlo, comer la carne / sopa,
   muñeco de madera, avance con sangre de bestia (§5.5).
5. **Alquimia y artefactos** (Convergence: Alchemy): construir la forja espiritual y el horno, subirlos de nivel (botón
   en la mesa), fabricar píldoras (calidad y toxicidad), grabar runas en un arma y usar talismanes; plantar hierbas.
6. **Sectas** (Convergence: Sects): tras la Convergencia aparecen 6 facciones de secta (pestaña Facciones); sus
   discípulos traen reino y técnicas (pestaña Técnicas); esperar o forzar (debug) una caravana, una visita y un asalto.
7. **Linajes, recursos, muebles, rasgos**: ver docs/modulos/razas/README.md y los documentos de cada módulo
   (*Spawn bloodline pawn*, *Purify bloodline*, vetas y muebles en el Arquitecto).
8. Guardar y cargar a mitad de cada cosa; mirar `Player.log`.

## 6. Pendiente
- ~~Nombre de autor~~: Prospero1088.
- ~~Ruta del juego~~: `C:\Program Files (x86)\Steam\steamapps\common\RimWorld`.
- Balance de números (se ajusta jugando desde M3).
