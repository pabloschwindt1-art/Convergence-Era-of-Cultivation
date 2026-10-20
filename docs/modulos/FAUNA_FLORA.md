# Módulo: Fauna, flora y comida (Convergence.Nature)

Depende de: Core + Recursos. Usa todas las DLC.
Regla: **todo aparece de forma natural en el mundo con baja probabilidad**, y además se puede cultivar o criar.

## 1. Fauna: bestias espirituales

### Despertar de los animales
Tras la Convergencia, los animales pueden **despertar como bestias espirituales**. No se duplica cada animal:
se le agrega un **estado** (hediff) de "bestia espiritual" con niveles. Funciona con animales de cualquier mod.

- **Probabilidad de despertar** según su nivel de amenaza (`combatPower`) y si es salvaje: una ardilla casi nunca,
  un oso a veces, un thrumbo casi siempre.
- **Niveles de bestia** (equivalentes a los reinos): más vida, daño y velocidad, y aura de color por elemento.
- Las bestias de nivel alto pueden **cultivar solas** con el tiempo y tener una técnica innata (escupir fuego, rugido).
- Los animales domesticados también pueden despertar: el Domador de Bestias lo favorece.

### Lo que dan
| Producto | Depende de |
|---|---|
| **Carne espiritual** | La da cualquier bestia despierta, además de su carne normal. La calidad depende del nivel |
| **Núcleo de bestia** (兽核) | Solo bestias de nivel medio o alto. Se absorbe (cultivo) o se usa en alquimia y formaciones |
| **Sangre, huesos, escamas** | Alquimia, tierra espiritual, artefactos, purificación de linajes |

### Bestiario propio (inspirado en el *Shan Hai Jing* 山海经, mitología de dominio público)

Hay que ir a fondo con lo wuxia: bestias propias además del despertar de las vanilla.

| Nivel | Bestia | Idea |
|---|---|---|
| Común | **Lobo de Viento**, **Serpiente de Jade**, **Grulla Celestial** | Fauna salvaje tras la Convergencia |
| Élite | **Araña de los Nueve Infiernos** (九幽蛛) | Telarañas que atrapan; veneno que corroe el qi |
| Élite | **Suanni, León Celestial del Caos** (狻猊) | Rugido que aturde; ama el humo y el incienso (se lo puede atraer o domar) |
| Élite | **Bifang** (毕方) | Pájaro de fuego de una pata: incendios donde pasa |
| Élite | **Gudiao** (蛊雕) | Águila cornuda que caza humanos; llora como un bebé |
| Élite | **Jiuying** (九婴) | Bestia de nueve cabezas de agua y fuego |
| Élite | **Bestia de la Tribulación** | Aparece donde una tribulación falló |
| **Jefe** | **Los Cuatro Malvados** (四凶): **Taotie** (饕餮, el devorador), **Qiongqi** (穷奇, tigre alado), **Hundun** (混沌, el caos sin rostro), **Taowu** (梼杌) | Amenazas de final de juego; cada uno con mecánica propia |

### Cómo aparecen
- **Misiones de caza** en el mapa del mundo (estilo gremio): ir a un sitio, matar la bestia y volver con núcleo y materiales.
- **Marea de bestias** (兽潮): oleadas que atacan la colonia; escalan con el reino de los cultivadores.
- **Asaltos de jefe:** un Malvado llega como asalto o como misión de raid en un reino secreto.
- **Guaridas:** sitios del mundo con bestias élite y tesoros.

### Arte (cuello de botella)
Cada bestia propia necesita gráficos de 3 vistas (norte, este, sur). Plan: empezar con **animales vanilla recoloreados**
como provisionales para probar mecánicas y luego reemplazarlos con arte propio.

### Animales de granja espirituales (domesticables)

| Animal | Produce | Notas |
|---|---|---|
| **Araña Tejedora de Jade** | **Hilo de seda espiritual** (como la lana: se "esquila" el capullo) | Tranquila si está bien alimentada; con hambre se vuelve agresiva. Base de la seda espiritual (ROPA.md) |
| **Oveja de Siete Colores** (七彩羊) | **Lana de siete colores**: cada esquila sale de un color/elemento al azar | La lana de un elemento da ropa con afinidad a ese elemento (feng shui y cultivo) |
| **Gallina de Sangre de Fénix** | Huevos espirituales | Ingrediente de píldoras y comida espiritual; resiste el calor |
| **Buey de las Nubes** | Leche espiritual; animal de carga | Muy fuerte, lento; leche para sopas y té espiritual |
| **Abeja de Jade** | Miel espiritual (colmena) | Con flores espirituales cerca, mejor miel |
| **Ciervo Lunar** | Astas que vuelven a crecer (alquimia) | Se le cortan las astas cada temporada sin matarlo |

### Pesca (Odyssey)
Odyssey define peces por bioma (`BiomeDef.fishTypes`: agua dulce/salada, comunes/poco comunes) y una tabla de
**capturas raras** (`rareCatchesSetMaker`). Agregamos (con carpeta condicional de Odyssey):
- **Peces espirituales:** Carpa Dorada, Anguila de Jade, Pez Luna (solo de noche), Pez del Trueno (en tormentas). Comida
  espiritual e ingredientes de alquimia. Solo aparecen tras la Convergencia (filtro en C#).
- **Capturas raras:** perlas espirituales, jade, fragmentos de tablillas de técnicas, núcleos de bestia acuática.
- **Leyenda de la Puerta del Dragón** (鲤鱼跃龙门): una Carpa Dorada criada en un estanque espiritual puede, muy rara vez,
  saltar la "puerta" y transformarse en un **jiao (dragón joven)** domesticable.
- **Estanque de carpas koi** (MOBILIARIO §4): cría de peces espirituales y feng shui de agua.

## 2. Flora

| Planta | Dónde | Uso |
|---|---|---|
| **Hierba espiritual común** | Silvestre, rara, en casi todos los biomas | Píldoras básicas, sopas |
| **Ginseng de mil años** | Silvestre, muy rara | Píldoras de avance |
| **Loto de Nieve** | Biomas fríos | Píldoras de Yin, curación |
| **Fruta Roja de Sangre** | Junglas | Cultivo corporal (templado) |
| **Árbol espiritual** | Muy raro; también se planta | Concentra qi en su zona (mejor cultivo cerca); da madera y frutos espirituales. Parecido al árbol ánima de Ideology |
| **Hongos subterráneos** | Crecen en la oscuridad (cuevas, bajo techo de roca) | Ingredientes de píldoras |
| **Champiñón de la Tribulación** | **Brota donde cae un rayo** (tribulaciones y tormentas) | Píldoras de tribulación; muy valioso |

### Tierra espiritual
- Las plantas espirituales **cultivadas** solo crecen en **tierra espiritual**.
- Se fabrica (es un suelo que se construye, como el suelo fértil): tierra + fragmentos de bestia (huesos, sangre) +
  piedras espirituales. Mejor tierra = mejor calidad de cosecha.
- El **Agricultor Espiritual** cultiva con más rendimiento y calidad, y puede sembrar las plantas raras.

## 3. Comida espiritual

| Comida | Ingredientes | Efecto |
|---|---|---|
| **Sopa espiritual** | Carne espiritual o hierbas + agua | Pequeño bonus de cultivo durante un día |
| **Banquete de bestia** | Carne de bestia de nivel alto | Bonus grande; buen humor |
| **Té espiritual** | Hojas del árbol espiritual | Estabiliza el Corazón del Dao |
| **Gachas de grano espiritual** | Grano cultivado en tierra espiritual | Comida diaria con bonus mínimo |

El efecto escala con el nivel de la bestia o la calidad de la hierba. Los alimentos espirituales no reemplazan
a las píldoras: son el bonus pequeño de todos los días.

## 4. Uso de las DLC
- **Ideology:** el árbol espiritual se integra con el estilo del árbol ánima; memes como "Armonía con las Bestias".
- **Biotech:** vientres de cría para bestias espirituales; sangre de bestia para linajes.
- **Anomaly:** bestias corrompidas por el vacío (variante oscura del despertar).
- **Odyssey:** flora de otros biomas y planetas.
- **Royalty:** el Imperio compra núcleos y plantas raras.

## 5. Prioridad
1. Despertar de animales + carne espiritual + núcleos de bestia.
2. Hierba espiritual común + tierra espiritual + sopa espiritual.
3. Árbol espiritual y Champiñón de la Tribulación.
4. Bestias propias y el resto.

## 6. Implementación v0.1 (mod `Mods/Convergence.FaunaFlora`, 2026-10-16)
Casi todo XML. El despertar de bestias, carne/sangre/cuero espiritual y la sopa son del Core (no se duplican).
Carpetas condicionales: `Mods/Odyssey` (peces, parches de pesca) y `Mods/Apparel` (receta de seda).

| Hecho | Detalle |
|---|---|
| Animales de granja (domesticables) | `CVG_JadeWeaverSpider` (cuerpo/sprite propio del Core teñido; esquila → `CVG_SpiderCocoon`; en el telar de Apparel se hila a `CVG_SpiritSilk`, receta `CVG_SpinSpiritSilk`), `CVG_SevenColorSheep` (una lana `CVG_SevenColorWool` con `colorGenerator` de 7 colores: cada esquila sale de uno), `CVG_PhoenixBloodHen` (huevos fecundables), `CVG_CloudOx` (leche `CVG_SpiritMilk`, animal de carga), `CVG_MoonDeer` (astas `CVG_MoonAntler` que vuelven a crecer) |
| Aparición natural | `wildBiomes` con comunidad baja y `DefModExtension_PostConvergence` (Core): el animal no sale en los biomas hasta la Convergencia (parche Harmony a `BiomeDef.CommonalityOfAnimal`) |
| Bestias propias salvajes | `CVG_WindWolf`, `CVG_CelestialCrane`, `CVG_NineNetherSpider` (sprite propio del Core). Su `combatPower` alto hace que el despertar del Core les dé nivel de bestia espiritual con probabilidad alta |
| Tierra espiritual | `TerrainDef CVG_SpiritSoil` (se construye en Suelos: 2 piedras espirituales + 1 sangre de bestia; fertilidad 1.9; +0.04 qi/celda). Parche: las plantas de Alchemy y el Algodón de las Nubes de Apparel suben su `fertilitySensitivity` a 1.2 → crecen mucho más rápido en ella |
| Champiñón de la Tribulación | `CVG_Plant_TribulationMushroom`; brota cerca de cada rayo de tribulación (`DefModExtension_TribulationSprout` + `TribulationSproutUtility` en el Core) |
| Hongo espiritual subterráneo | `CVG_Plant_SpiritFungus` (cave plant, también sembrable) |
| Árbol espiritual | Ya existe en Resources (`CVG_Plant_SpiritTree`); el parche le da qi (3) y elemento Madera si está dentro de una habitación |
| Pesca (Odyssey) | `CVG_Fish_GoldenCarp/JadeEel/MoonFish/ThunderFish` en `fishTypes` (tablas *Uncommon* de agua dulce y salada de todos los biomas) y una captura rara extra (jade, tablilla, piedra espiritual, perla espiritual) en los `RareFishingCatches_*` |

**Pendiente / notas**
- Araña tejedora: el "agresiva si pasa hambre" no está (solo `manhunterOnDamageChance`); necesitaría un hediff/mental state de hambre.
- Lana de siete colores: el color es aleatorio por esquila, **no** hay afinidad de elemento en la ropa todavía.
- Peces: no están restringidos a "después de la Convergencia" ni el Pez Luna a la noche / el del Trueno a la tormenta (haría falta C#). Carpa Dorada → jiao: sin hacer.
- Árbol espiritual: el aporte de qi "en su zona" al aire libre necesita una mecánica de zona en el Core.
- Champiñón: solo brota con rayos de **tribulación** (los rayos normales de tormenta no).
- Abeja de Jade, bestias élite (Suanni, Bifang…), jefes, cría en Biotech, comida espiritual extra: pendientes.
- Arte (todo provisional con texturas vanilla teñidas): Araña Tejedora, Oveja de Siete Colores, Gallina, Buey, Ciervo (3 vistas o `Graphic_Multi` + muerto), Lobo de Viento, Grulla, plantas (tribulación, hongo), peces (4), terreno Tierra espiritual. Rutas propias sugeridas: `Things/Pawn/Animal/CVG/<Nombre>/<Nombre>` (+`_north/_east/_south`), `Things/Plant/CVG/<Nombre>`, `Things/Item/Fish/CVG/<Nombre>`, `Terrain/Surfaces/CVG_SpiritSoil`.
