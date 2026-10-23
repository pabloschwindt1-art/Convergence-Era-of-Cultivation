# Convergence: Alchemy & Artifacts — inventario completo (2026-10-22)

Documento para diseñar la próxima versión del addon con otra IA. Describe **lo que hay hoy**, con los números reales sacados de
los XML y del código. Al final hay una lista de huecos y preguntas de diseño. Paquete: `prospero1088.convergence.alchemy`.
Requiere el Core y Resources. Opcional: Fauna & Flora (tres recetas alternativas).

Convenciones: "trabajo" en ticks de RimWorld (60 ticks = 1 segundo a velocidad normal; 1200 = 20 s). "Qi" = recurso del dantian
(lo que se gasta en técnicas y se llena meditando), **no** el progreso de reino. Los nombres en español son los del juego.

---

## 1. Mecánicas generales (código del Core)

### 1.1 Calidad de las píldoras
Las píldoras con calidad (templado, recuperación de qi, purificación, nueve vueltas) multiplican su efecto según la calidad:

| Calidad | Horrible | Mediocre | Normal | Buena | Excelente | Obra maestra | Legendaria |
|---|---|---|---|---|---|---|---|
| Factor | ×0,5 | ×0,75 | ×1 | ×1,2 | ×1,45 | ×1,75 | ×2,2 |

- Afecta al recurso que da la píldora (qi o sangre-qi) y a la probabilidad de purificar el linaje.
- **No** afecta a la toxicidad: una píldora legendaria intoxica igual que una horrible.

### 1.2 Niveles de los talleres (forja espiritual y horno de alquimia)
Se mejoran en el lugar, con investigación del Dao y materiales:

| Nivel | Investigación | Costo de mejora (horno) | Costo de mejora (forja) | Velocidad de trabajo | Bonus de calidad |
|---|---|---|---|---|---|
| 1 | — | — | — | ×1 | 0 |
| 2 | Fuego controlado (horno) / Plegado con qi (forja), 2000 + 6 de comprensión | 40 piedra espiritual, 10 jade espiritual | 50 acero espiritual, 30 piedra espiritual | ×1,25 | 0,25 |
| 3 | Tribulación de la píldora (horno) / Tesoros espirituales (forja), 4500 + 14 de comprensión, piden también Fundación | 8 cristal espiritual, 20 jade espiritual | 10 cristal espiritual, 20 jade, 50 piedra | ×1,5 | 0,6 |

- **Bonus de calidad:** probabilidad de subir un escalón la calidad del resultado (0,25 = 25 %; 0,6 = 60 %). Nunca pasa de Obra
  maestra por este bonus.
- Algunas recetas solo aparecen desde cierto nivel (ver tablas).
- Los dos talleres queman madera (50 de capacidad; solo gastan mientras trabajan).
- **Copian las recetas vanilla:** el horno hace todo lo del laboratorio de drogas y la forja todo lo de la herrería de combustible.

### 1.3 Toxicidad de píldoras (`CVG_PillToxicity`)
Baja **0,08 por día**. Etapas:

| Severidad | Etapa | Efecto |
|---|---|---|
| 0–0,4 | rastro (invisible) | nada |
| 0,4 | acumulación | cultivo ×0,85; umbral de crisis +3 % |
| 0,7 | envenenamiento | dolor +10 %; conciencia −15 %; cultivo ×0,6 |
| 1,0 | envenenamiento grave | dolor +25 %; conciencia −35 %; cultivo ×0,3; vomita cada ~12 h |

Máximo 1,2. Cuatro píldoras de las nueve vueltas seguidas (4 × 0,3) llevan al máximo.

### 1.4 Grado espiritual de las armas
- **Grado 0:** mortal (vanilla y otros mods).
- **Grado 1:** runas o arco de cuerno.
- **Grado 2:** arma espiritual.
- **Grado 3 o más:** tesoro.

El daño contra un cultivador se multiplica según cuántos reinos lo supera (supresión de reino). Un arma de grado bajo apenas hiere
a un cultivador de reino alto. Algunos materiales dan un grado mínimo: un jian mortal de acero espiritual ya hiere a un cultivador
de Refinamiento de Qi, y la madera de melocotonero, por lore, también.

### 1.5 "Hecho por" (desde 2026-10-22)
Toda píldora, arma y talismán muestra en su ficha quién lo hizo:
- **Colono:** su nombre.
- **De afuera** (comercio, sectas, botín, eventos): un creador de lore del oficio que corresponda: "Lin Jin, un alquimista
  errante", "un viejo herrero", "un tallador de sellos olvidado".

La ficha de cada píldora también muestra:
- el efecto de **esa** píldora según su calidad, con la tabla completa;
- los efectos secundarios;
- el nivel de mesa y la habilidad que pide.

### 1.6 Contadores de progreso
Los talleres suman a contadores del Core: Alchemy, Forging, Talismans. Las plantas espirituales suman a SpiritFarming. Esos
contadores alimentan rasgos y eventos.

### 1.7 Escala de los reinos (para comparar)
Progreso por etapa (4 etapas por reino):

| Camino | Reino 1 | Reino 2 | Reino 3 | Reino 4 | Reino 5 | Reino 6 |
|---|---|---|---|---|---|---|
| Qi | Refinamiento de Qi: 1000 | Fundación: 2500 | Núcleo Dorado: 6000 | — | — | — |
| Cuerpo | Templado de piel: 800 | Templado de carne: 1800 | Forja de huesos: 3500 | Limpieza de médula: 6000 | Refinamiento de sangre: 9000 | Cuerpo dorado: 14000 |

---

## 2. Talleres

| Taller | Construcción | Qué hace | Lore |
|---|---|---|---|
| **Forja espiritual** (2×2) | 80 de piedra o metal + 20 piedra espiritual; Construcción 5; trabajo 4000 | Armas espirituales + todo lo de la herrería. Niveles 1-3 (2: lanzas y arcos; 3: tesoros como la espada voladora) | Gan Jiang y Mo Ye, la pareja de herreros de la leyenda |
| **Horno de alquimia** (2×2) | 80 de piedra o metal + 15 piedra espiritual | Píldoras y medicinas + todo lo del laboratorio de drogas. Niveles 1-3 (2: purificación y elixir; 3: nueve vueltas) | El trípode de Shennong, el Divino Agricultor |
| **Mesa de runas** (2×2) | 80 de piedra + 10 jade espiritual | Grabar runas (sube un arma común a grado 1) | Durante la Calamidad de Hierro se grabaron runas en millones de hojas mortales |
| **Mesa de talismanes** (2×2) | 80 de madera + 5 piedra espiritual | Talismanes de un solo uso (trabajo de Arte) | Zhang Daoling, el primer Maestro Celestial, en Longhushan |

---

## 3. Píldoras (horno de alquimia)

| Píldora | Ingredientes | Trabajo | Habilidad | Nivel | Efecto (calidad Normal) | Toxicidad | Valor |
|---|---|---|---|---|---|---|---|
| **Píldora de fundación** (Core) | Receta del Core (hierbas y polvo de plata); alternativa: 1 arena estelar + 2 hierba espiritual + 2 piedra espiritual | 1200 | — | 1 | Durante 1 día: +30 % de probabilidad de avance al intentar la Fundación (se gasta en el intento). Sin calidad | — | 45 |
| **Píldora de templado** | 3 hierba espiritual, 1 sangre de bestia, 1 piedra espiritual | 1500 (lote de 4) | Intelectual 4 | 1 | +40 sangre-qi (camino del cuerpo) | +12 % | 30 |
| **Píldora de recuperación de qi** | 2 hierba espiritual, 1 piedra espiritual | 1500 (lote de 4) | Intelectual 4 | 1 | +50 qi (no inicia el camino) | +10 % | 25 |
| **Píldora de purificación de sangre** | 4 hierba espiritual, 1 núcleo de bestia, 2 jade espiritual, 3 piedra espiritual | 1500 (lote de 4) | Intelectual 8 | 2 | 60 % de subir un nivel de pureza del linaje (por calidad). Si falla o no hay linaje: +35 % de toxicidad extra | +20 % | 250 |
| **Píldora de las nueve vueltas** | 2 raíz de ginseng espiritual, 3 pétalo de loto de nieve, 1 cristal espiritual, 5 piedra espiritual | 6000 (sin lote) | Intelectual 12 | 3 | +300 qi (de +150 a +660 según la calidad) | +30 % | 400 |
| **Píldora de esencia de sangre** (Biotech Wuxia) | (receta en Biotech Wuxia, horno de alquimia) | — | — | — | Hemógeno del demonio de sangre | — | — |

Recetas alternativas con Fauna & Flora:
- **Recuperación de qi:** 1 asta lunar + 1 piedra espiritual → 2 píldoras (trabajo 2500).
- **Purificación:** 6 hongo espiritual en lugar de las hierbas (nivel 2).
- **Nueve vueltas:** 1 champiñón de la tribulación en lugar del cristal espiritual (nivel 3).

## 4. Medicinas y consumibles

| Objeto | Dónde | Ingredientes | Trabajo / habilidad | Efecto | Valor |
|---|---|---|---|---|---|
| **Ungüento de hierbas** | Lugar de fabricación u horno | 2 hierba espiritual | 500; Medicina 3 | Medicina: potencia 0,65, calidad máx. 0,75 (entre herbal e industrial) | 13 |
| **Píldora curativa** | Horno | 1 ungüento, 1 hierba espiritual, 1 piedra espiritual | 700; Intelectual 4 | Medicina: potencia 1,0 (como la industrial) | 20 |
| **Elixir de jade** | Horno nivel 2 | 1 píldora curativa, 1 jade espiritual, 2 hierba espiritual | 2500; Intelectual 8 | Medicina: potencia 1,35, calidad máx. 1,15 (como la de mundo brillante) | 55 |
| **Píldora de ayuno (bigu)** | Horno | 10 arroz, 1 hierba espiritual → 20 píldoras | 700; Cocina | Comida simple (nutrición 0,05); hambre ×0,75 durante ~6 h. No se pudre en 120 días | 1,6 |
| **Té de las nubes** | Fogón o cocina eléctrica | 2 hoja de té de las nubes o hierba espiritual → 6 tés | 400; Cocina | Droga social, diversión social; regeneración de qi ×1,1 y conciencia +3 % durante ~12 h. Sin adicción | 4 |
| **Incienso** | Lugar de fabricación u horno | 2 hierba espiritual, 4 madera → 8 varitas | 450; Intelectual | Diversión meditativa; cultivo ×1,1; umbral de crisis −3 %; vista y movimiento −5 % durante ~12 h | 3 |
| **Bollos al vapor** | Fogón o cocina | Comida simple (0,5 ingredientes; lote de 4) | 300 | Comida simple (nutrición 0,9) | 15 |
| **Arroz con carne espiritual** | Fogón o cocina | 0,25 carne espiritual + 0,25 arroz | fina | Comida fina: +10 qi y +10 sangre-qi | 35 |
| **Banquete de la secta** | Fogón o cocina | 0,5 proteína + 0,5 vegetal + 1 hierba espiritual | 800; Cocina 8 | Comida lujosa + "nutrido de espíritu" (1 día) | 60 |

## 5. Hierbas (plantas y cosechas)

| Planta | Crece en | Siembra | Cosecha | Valor de la cosecha | Uso |
|---|---|---|---|---|---|
| **Hierba espiritual** | 8 días | Plantas 6 | 4 hierbas | 4 | Base de casi todas las píldoras, medicinas, incienso y té |
| **Ginseng espiritual** | 18 días | Plantas 9 | 3 raíces | 30 | Nueve vueltas |
| **Loto de nieve** | 12 días (necesita luz ≥ 30 %) | Plantas 8 | 4 pétalos | 18 | Nueve vueltas |

Las tres aparecen silvestres, raras. El ginseng es muy raro.

## 6. Armas espirituales (forja espiritual, material: metal espiritual)

| Arma | Grado | Nivel | Material + extra | Trabajo / habilidad | Ataques |
|---|---|---|---|---|---|
| **Espada espiritual** | 2 | 1 | 80 metal espiritual + 5 piedra espiritual | 20000; Artesanía 6 | Punta y filo 26 (2,5 s), mango 10 |
| **Sable espiritual** | 2 | 1 | 80 + 5 piedra | 20000; Artesanía 6 | Filo 29 (2,4 s) |
| **Lanza espiritual** | 2 | 2 | 80 + 5 piedra | 22000; Artesanía 7 | Punta 31 (2,6 s) |
| **Arco espiritual** | 2 | 2 | 60 + 4 piedra + 20 madera | 18000; Artesanía 7 | Flecha 18, penetración 30 %, alcance 32,9 |
| **Espada voladora** | 3 | 3 | 80 + 5 piedra | 40000; Artesanía 10 | Punta y filo 34 (2,2 s). Hoy se usa en la mano: el arte de montarla "se perdió en la Gran Ruptura" |

## 7. Armas wuxia (mortales, grado 0 salvo el arco de cuerno)

| Arma | Dónde | Material | Habilidad / investigación | Ataques / notas |
|---|---|---|---|---|
| **Jian** | Herrería | Metal o madera (80) | Artesanía 5; Espadas largas | Punta y filo 21 |
| **Dao** | Herrería | Metal (90) | Artesanía 5; Espadas largas | Filo 24 |
| **Daga bishou** | Lugar de fabricación o herrería | Metal (35) | — | Filo 12, punta 13; rápida |
| **Zhanmadao** | Herrería | Metal (150) | Artesanía 7; Espadas largas | Filo 34 (3,6 s); pesada |
| **Guandao** | Herrería | Metal (120) | Artesanía 6; Espadas largas | Filo 29, punta 22 |
| **Bastón gun** | Lugar de fabricación o herrería | Madera o metal (60) | — | Contundente 14 |
| **Abanico de hierro** | Herrería | Metal (50) | Artesanía 6; Herrería | Filo 13; esquiva +4 % |
| **Arco recto de bambú** | Lugar de fabricación o forja | Madera (45) | Artesanía 3 | Flecha 12, alcance 24,9 |
| **Arco de cuerno de bestia** (grado 1) | Lugar de fabricación o forja | 50 madera + 1 núcleo de bestia | Artesanía 7; Arco grande | Flecha 18, penetración 30 %, alcance 33,9 |
| **Ballesta de repetición zhuge** | Herrería | 40 madera + 25 acero | Artesanía 6; Herrería | Ráfaga de 2, virote 9, alcance 20,9 |
| **Agujas voladoras** | Herrería | Metal (20) | Artesanía 5; Herrería | Ráfaga de 2, aguja 6, penetración 25 %, alcance 14,9 |

Todas tienen calidad y pueden recibir runas.

## 8. Runas y talismanes

| Objeto o receta | Dónde | Costo | Trabajo / habilidad | Efecto | Valor |
|---|---|---|---|---|---|
| **Grabar runas** | Mesa de runas | El arma (no se consume) + 3 piedra espiritual | 3000; Artesanía 5 | Arma de grado 0 → grado 1 (conserva calidad y material) | — |
| **Talismán del trueno** | Mesa de talismanes | 2 tela, 2 piedra, 1 jade | 1200; Arte 4 | Un uso: rayo a todos los hostiles en 5 casillas (20 de quemadura) | 60 |
| **Talismán de curación** | Mesa de talismanes | 2 tela, 1 núcleo de bestia, 5 jade, 5 piedra | 6000; Arte 10 | Un uso: cura la peor condición de salud | 600 |
| **Talismán del viento veloz** | Mesa de talismanes | 2 tela, 1 piedra | 1200; Arte 4 | Un uso: movimiento ×1,4 durante 6 h | 40 |

## 9. Investigación

| Proyecto | Costo | Requisitos | Desbloquea |
|---|---|---|---|
| Plegado con qi | 2000 + 6 comprensión | Respiración de qi | Forja nivel 2 |
| Tesoros espirituales | 4500 + 14 comprensión | Plegado con qi, Fundación | Forja nivel 3 |
| Fuego controlado | 2000 + 6 comprensión | Respiración de qi | Horno nivel 2 |
| Tribulación de la píldora | 4500 + 14 comprensión | Fuego controlado, Fundación | Horno nivel 3 |

El horno, la forja y las mesas no piden investigación: están desde el principio.

## 10. Dónde aparece la alquimia en otros mods

- **Factions:**
  - El Pabellón del Caldero Celestial vende píldoras, hierbas y armas espirituales (Buena a Obra maestra). Regala píldoras a
    elección (nueve vueltas, elixir de jade, fundación, purificación, templado) y armas espirituales obra maestra.
  - Los Nueve Templados venden píldoras de templado, curativas y ungüento.
  - El cuerpo yang puro (constitución del Caldero) acelera la alquimia: velocidad de síntesis +10 a +40 %.
  - El maestro alquimista (rol) da síntesis ×1,15.
- **Stories:** eventos que dan o piden hierbas espirituales y píldoras de templado; reinos secretos con hierbas.
- **Biotech Wuxia:** la píldora de esencia de sangre se hace en el horno.
- **Apparel:** usa la forja espiritual para la ropa de escamas y metal.
- **Furniture:** gabinete de píldoras (almacenamiento).
- **Core:** píldora de fundación; historias de fondo de alquimistas (probador de píldoras, vendedor de píldoras falsas, mandadero
  de la sala de píldoras); recuerdo "tomó una píldora".

---

## 11. Huecos y preguntas para el diseño

1. **Lore viejo:** varias descripciones nombran sectas que ya no están en el pack (Templo del Loto Silencioso, Pico Azur, Clan del
   Dragón Antiguo). Hay que pasarlas a las sectas de Factions (Caldero, Espada Sin Forma, Nueve Templados…).
2. **Pocas píldoras de reino:** solo hay ayuda de avance para la Fundación (píldora del Core). No hay píldora para el Núcleo
   Dorado ni para los avances del cuerpo.
3. **Las píldoras dan qi, no progreso:** ninguna sube el progreso de reino directamente. ¿Debería haber píldoras de
   "comprensión" o de progreso, con más toxicidad?
4. **Sin fallos de alquimia:** una receta nunca sale mal. Tale of Immortal tiene píldoras fallidas, explosiones del horno y
   "píldoras de desecho". ¿Agregamos riesgo según la habilidad y el nivel del horno?
5. **Sin grados de píldora** (inferior, medio, superior; con vetas o sin ellas) más allá de la calidad vanilla.
6. **La toxicidad no depende de la calidad:** ¿una píldora mejor debería ser más limpia?
7. **Sin resistencia a la toxicidad:** ¿un cuerpo, rasgo o técnica que la reduzca (el yang puro, por lore)?
8. **Hierbas:** solo tres. ¿Más hierbas por elemento (fuego, agua…) y por reino? ¿Edad de la hierba (el ginseng de cien años)?
9. **Talleres:** el horno no tiene "fuego" propio (combustible espiritual, fuego del corazón, fuego de bestia). ¿Fuegos como
   mejora o como combustible?
10. **Armas:** las armas espirituales son pocas. Falta la "cría" del arma (subir de grado con uso o con sangre), los artefactos con
    técnica propia y la espada voladora de verdad (atacar a distancia).
11. **Talismanes:** solo tres. ¿Talismanes de defensa, de formación, de sellado, de teletransporte corto?
12. **Comercio de alquimia:** ¿subastas del Pabellón (evento), recetas como objetos (pergaminos de receta) o encargos de píldoras?
13. **"Hecho por" ya existe:** ¿le damos peso? Por ejemplo, píldoras de un maestro famoso que valen más, o un sello de secta.
