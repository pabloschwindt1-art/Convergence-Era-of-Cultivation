# Módulo: Edificios, forja y feng shui

Depende de: Core + Recursos. Inspiración: el feng shui de *Amazing Cultivation Simulator*.

## 1. Forja Espiritual (mejorable por niveles)

Banco de trabajo para metales espirituales y artefactos. **Se mejora en el sitio**: nivel 1 → 2 → 3.

| Nivel | Mejora con | Desbloquea |
|---|---|---|
| 1 — Forja de Piedra Espiritual | Construcción: piedra, acero, piedras espirituales | Acero espiritual, armas espirituales básicas |
| 2 — Forja de Fuego de Bestia | Núcleos de bestia + acero espiritual | Plata lunar, armas y armaduras de grado medio, refinado de tesoros |
| 3 — Forja de la Llama del Dao | Cristal espiritual + oro del caos (+ fuego de dragón/fénix opcional) | Oro del caos, artefactos de alto grado, espadas voladoras |

Técnica:
- `CompSpiritForge` guarda el nivel. Un botón **"Mejorar forja"** crea un trabajo de construcción que consume los materiales.
- Las recetas llevan `RecipeWorker_RequiresForgeLevel` + `minForgeLevel`: el juego oculta la receta hasta el nivel necesario.
- La forja cambia de gráfico en cada nivel y da más calidad o velocidad.
- Funciona sin electricidad (combustible: carbón, madera o piedras espirituales). Válido en el modo clásico.
- El mismo sistema de niveles sirve para el **horno de alquimia** y el **altar de formaciones**.

## 2. Jade: muebles, joyas y tablillas

El jade vanilla y el jade espiritual pasan a ser materiales centrales del cultivador:
- **Tablillas de jade:** registran técnicas (se aprenden al leerlas) y mensajes. El Maestro deja una con pistas.
- **Joyería** (huecos `CVG_Neck` / `CVG_Finger`): colgantes, anillos y brazaletes de jade con bonus de cultivo o de Corazón del Dao.
- **Lugares de cultivo:** cojín de cultivo (madera), plataforma de jade, estrado de meditación de jade espiritual. Cada uno con más concentración de qi.
- **Muebles de feng shui:** biombos, estatuas, jarrones e incensarios de jade y madera espiritual.

## 3. Feng shui (风水)

Cada habitación tiene **concentración de qi** (un stat de habitación nuevo, como la belleza o la impresión):
- Suben el qi: lugares de cultivo, muebles de jade, árboles espirituales cercanos, vetas de qi, formaciones.
- Cada mueble tiene **elemento** (madera, fuego, tierra, metal, agua). **Armonía de los cinco elementos**: si la habitación
  los tiene equilibrados, gran bonus. Si un elemento domina, bonus para cultivadores de ese elemento y penalización para su opuesto.
- El cultivo, la recuperación de qi y el Corazón del Dao dependen de la concentración de qi de la habitación.

Técnica: `RoomStatDef CVG_QiConcentration` con su `RoomStatWorker`, más un `DefModExtension_FengShui { element, qi }` en los muebles.

## 4b. Menú propio
Catálogo completo de muebles: [MOBILIARIO.md](MOBILIARIO.md).

Todo el mobiliario del mod va en una pestaña **"Cultivo"** del menú de Arquitecto (`DesignationCategoryDef CVG_Cultivation`):
lugares de cultivo, entrenamiento, forja, alquimia, feng shui.

## 5. Entrenamiento (los Tres Tesoros)
Además de cultivar, los colonos **entrenan** atributos de fundamento (requisitos para avanzar de reino, ver DISENO.md §4.1c).

| Tesoro | Edificios | Entrena |
|---|---|---|
| Cuerpo (精) | Muñeco de madera (wing chun), piedras de pesas, pilar de hierro, estanque de agua fría | Templado corporal |
| Qi (气) | Cojín de cultivo, plataforma de jade, formación de reunión de qi | Pureza del qi |
| Alma (神) | Altar de incienso, mesa de caligrafía, tablero de go (weiqi), cámara de ilusiones | Fuerza del alma |

Cada edificio es un trabajo (o recreación) con su propio rendimiento; la calidad del edificio y el feng shui suman.

## 6. Prioridad
1. Cojín de cultivo y concentración de qi básica (las necesita el Core en M3).
2. Forja Espiritual nivel 1.
3. Feng shui de los cinco elementos y muebles de jade.
4. Niveles 2-3 de la forja, horno de alquimia y altar de formaciones.

## Implementación (2026-10-15): mod `Convergence: Alchemy & Artifacts`

Mod aparte (`Mods/Convergence.Alchemy`, XML + arte de ComfyUI; depende del Core y de Resources). Nada usa electricidad.

**Core (genérico):** `CompWorkbenchLevel` + `Building_LeveledWorkTable` (mesas con niveles: botón "Mejorar a nivel N" que gasta
materiales del mapa; arte, velocidad y bonus de calidad por nivel), `DefModExtension_RequiresWorkbenchLevel` (en la receta o en
el producto), efectos de píldora que escalan con la calidad (`IngestionOutcomeDoer_GiveHediffByQuality`,
`IngestionOutcomeDoer_GainResourceByQuality`), `IngestionOutcomeDoer_PurifyBloodline`, `CompUseEffect_SpiritBurst`,
`RecipeWorker_EngraveRunes` y `CompSpiritGrade` en todas las armas (grado 0).

| Mesa | Niveles | Hace |
|---|---|---|
| Forja espiritual (2×2, cuenta como forja: bonus del Fénix) | 1 → 2 (50 acero espiritual + 30 piedras) → 3 (10 cristal + 20 jade esp. + 50 piedras); velocidad ×1 / ×1,25 / ×1,5; calidad +0 / +0,25 / +0,6 niveles | Espada y sable espirituales (N1), lanza y arco (N2), espada voladora (N3, grado 3). Material: categoría nueva **metal espiritual** (acero espiritual, plata lunar, oro del caos, hierro frío, acero galáctico) |
| Horno de alquimia (2×2) | Igual que la forja, con jade y cristal | Píldora de Fundación (receta del Core movida aquí, mismo defName), de Templado (sangre-qi), de Recuperación de qi, de Purificación de sangre (N2) |
| Mesa de runas | — | Grabar runas: un arma de grado 0 sube a grado 1 (se conserva el arma; gasta 3 piedras) |
| Mesa de talismanes | — | Talismanes de un uso: trueno (20 de quemadura a hostiles a 5), curación (peor dolencia), viento veloz (+40% velocidad 6 h) |

- **Píldoras:** tienen **calidad** (habilidad Intelectual + nivel del horno) y su efecto escala ×0,5 (horrible) … ×2,2
  (legendaria). Cada píldora suma **toxicidad** (`CVG_PillToxicity`, baja 0,08/día; desde 0,4 frena el cultivo, desde 0,7
  envenena). La de Purificación sube un nivel de pureza del linaje con 60% × calidad; si falla, envenena.
- **Hierbas espirituales** (hasta que exista Fauna & Flora): hierba espiritual, ginseng espiritual y loto de nieve, raras en
  estado silvestre y sembrables; todas dan `CVG_SpiritHerb`.
- **Arte:** forja 2/3/4 y horno 2/4/3 (un sprite por nivel), mesa de runas 3, mesa de talismanes 3, armas (espada 1, sable 4,
  lanza 4, arco 1, espada voladora 3), píldoras y talismanes de los sprites de píldora/talismán, plantas de flora.
- **Pendiente:** espada voladora que vuele (técnica M5), recetas raras de los oficios espirituales, mejora de nivel con trabajo
  de construcción en vez de instantánea.
