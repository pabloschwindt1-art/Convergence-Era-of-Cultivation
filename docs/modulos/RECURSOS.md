# Módulo: Recursos (Convergence.Resources)

Depende de: Core. Usa todas las DLC (Odyssey para el espacio). Funciona en modo clásico (nada requiere electricidad).

## 1. Minerales en el mapa

Se generan con el sistema del propio juego: cada veta tiene `mineableScatterCommonality` y tamaño de grupo.
Referencia vanilla: **plastiacero 0.05**, **jade 0.065**, ambos en grupos de 2 a 8 celdas.
Las vetas reutilizan la textura común de roca moteada (`RockFlecked_Atlas`) con dos colores: **sin arte nuevo**.

| Veta | Rareza (idea) | Qué da | Cuándo aparece |
|---|---|---|---|
| **Piedra espiritual** (灵石) | 0.05 (como plastiacero) | Piedra espiritual: moneda de cultivadores, combustible de formaciones, cultivo | Tras la Convergencia |
| **Jade espiritual** | Parte de las vetas de jade se transforma | Jade espiritual: tablillas de técnicas, talismanes, artefactos | Tras la Convergencia |
| **Cristal espiritual** | 0.02 (más raro) | Cristal: núcleos de formaciones, píldoras altas | Tras la Convergencia |
| **Hierro frío de mil años** | 0.01, vetas profundas | Metal raro para armas | Tras la Convergencia |
| **Acero galáctico / meteórico** | Solo en asteroides y escombros orbitales (Odyssey) | Metal de élite para artefactos | Siempre (espacio) |

**El jade vanilla gana uso:** tallar tablillas de jade (registran técnicas y mensajes), talismanes simples y
refinarlo en jade espiritual con piedras espirituales.

**Al ocurrir la Convergencia:** un porcentaje de las vetas de jade del mapa se convierte en jade espiritual y
aparecen algunas vetas de piedra espiritual nuevas (el evento las coloca; los mapas nuevos ya las generan solas).

## 1b. Rocas wuxia (decisión 2026-10-15)
El mundo está mezclado con el Reino Celestial: hay **tipos de roca nuevos** completos (roca natural, trozos, bloques):
Roca de las Diez Tribulaciones, Jade Negro de Montaña, Piedra Nube, Mármol Celestial…
- **Ajuste "Mundo wuxia":** al generar el mundo, solo rocas nuestras (reemplazo de caliza, mármol, etc.); apagado, se mezclan.
- **Feng shui por material:** paredes, columnas y pisos de piedra/jade espiritual o rocas wuxia suben la concentración de qi.
- **Hierro frío de mil años:** veta solo en biomas fríos.

## 1c. Materiales por bioma (con los biomas de Odyssey)
| Bioma | Material exclusivo |
|---|---|
| Campo de lava (volcánico) | Cristal de fuego espiritual, Obsidiana de Dragón (roca wuxia), loto de llama |
| Glaciar / tundra / hielo | Hierro frío de mil años, loto de nieve |
| Bosque luminoso | Jade lunar, hongo espiritual luminoso |
| Tierras cicatrizadas | Jade de sangre (camino demoníaco) |
| Desierto | Arena de oro solar (Yang) |
| Jungla | Fruta roja de sangre, madera espiritual |
| Pantano | Hierba venenosa |
| Costa y mar | Perlas espirituales (pesca) |
| Espacio | Acero galáctico |

## 2. Metales espirituales (refinados)

Materiales nuevos (*stuff*), como el plastiacero: sirven para armas, armaduras, muebles y edificios, cada uno con
sus propios multiplicadores.

| Material | Receta (idea) | Fortaleza |
|---|---|---|
| **Acero espiritual** | Acero + piedra espiritual | Mejor que el acero; conduce qi (técnicas de arma más baratas) |
| **Plata lunar** | Plata + jade espiritual | Ligera; buena contra lo demoníaco y lo del vacío (Anomaly) |
| **Oro del caos** | Oro + cristal espiritual | Muy caro; artefactos de alto nivel |
| **Hierro frío de mil años** | Se extrae, no se refina | Filo extremo; frío |
| **Acero galáctico** | Se extrae en el espacio | El mejor metal; combina con Qi-Tech |

Se refinan en un **horno de refinado** (combustible: carbón, madera o piedras espirituales) para no requerir electricidad.

- **Cristal primordial** (`CVG_PrimordialCrystal`, Core, 2026-10-21): 10 piedras espirituales prensadas en la mesa de cantero
  (`CVG_MakePrimordialCrystal`). Qi sólido y estable, material común para los módulos; primer uso: el resonador espiritual de Qi-Tech.
  Arte provisional (jade vanilla), falta el propio: `Things/Item/Resource/CVG_PrimordialCrystal`.

## 3. Madera y fibras espirituales
- **Madera espiritual:** se obtiene talando árboles espirituales (ver Fauna y Flora). Muebles que suben la concentración de qi.
- **Seda de gusano espiritual:** tela para túnicas de secta.

## 4. Productos de bestias (ver Fauna)
- **Núcleos de bestia** (por nivel de amenaza), **huesos y sangre de bestia**: alquimia, tierra espiritual, artefactos.

## 5. Uso de las DLC
- **Odyssey:** acero galáctico en asteroides y escombros orbitales; reinos secretos como sitios a los que se viaja.
- **Anomaly:** la plata lunar y los talismanes funcionan contra entidades del vacío.
- **Royalty:** el Imperio compra piedras espirituales y artefactos.
- **Biotech:** la sangre de bestia sirve para purificar linajes.

## 6. Prioridad
1. Piedra espiritual (la necesitan el Core, la alquimia y las formaciones).
2. Jade espiritual y la conversión del jade vanilla.
3. Acero espiritual.
4. El resto.

## 7. Implementación (2026-10-15)

**Core** (fundamentales, `Mods/Convergence.Core/Common/Defs/Resources_Spirit.xml`):
- `CVG_SpiritStone` (piedra espiritual: moneda, combustible; qi 0,05 como feng shui) y `CVG_SpiritJade` (jade espiritual, material
  pétreo). Sprites de ComfyUI: piedra 1 y 3, jade 2 y 3 (`Textures/Things/Item/Resource/CVG_*`).
- Vetas sobre `RockFlecked_Atlas` con colores: piedra espiritual 0,05 (como plastiacero), jade espiritual 0,03.
  `DefModExtension_ConvergenceMineable`: no aparecen en mapas generados antes de la Convergencia (parche de
  `GenStep_ScatterLumpsMineable.ChooseThingDef`); al ocurrir, la mitad de los grupos de jade vanilla del mapa pasan a jade
  espiritual y aparecen 3-5 vetas de piedra espiritual en la roca. Una veta nueva = XML con la extensión.
- **Qi del suelo:** `DefModExtension_FengShui` en un `TerrainDef` suma su `qi` **por casilla** a la concentración de qi
  (habitación y aire libre).
- Hook de forja (`DefModExtension_Forge`, `CVG_ForgeSpeed`, `CVG_ForgeQuality`): ver docs/modulos/razas/README.md.

**Mod `Convergence.Resources`** (solo XML; depende del Core):
| Material | Tipo | Valor | Notas |
|---|---|---|---|
| Acero espiritual | Metálico | 6 | HP ×1,6, filo ×1,2; horno: 20 acero + 2 piedras → 20 |
| Plata lunar | Metálico | 8 | Muy ligera, belleza; horno: 50 plata + 2 jade espiritual → 10 |
| Oro del caos | Metálico | 40 | Belleza enorme; horno: 20 oro + 2 cristal → 10 (Artesanía 8) |
| Hierro frío de mil años | Metálico | 12 | Filo ×1,6, HP ×1,8; solo se extrae (veta 0,01, **solo en tierras frías**: media ≤ 0 °C o tundra, hielo, mar helado, boreal, ciénaga fría) |
| Cristal espiritual | Pétreo | 15 | Frágil y bello, qi 0,05 (también como material); veta 0,02 |
| Acero galáctico (Odyssey) | Metálico | 30 | Solo en asteroides (parche de `mineableCounts`), HP ×2,2 |

- Todos son *stuff*: paredes, puertas, muebles y armas los usan solos. Sprites propios: acero espiritual y cristal; el resto
  tiñe texturas vanilla (plata, oro, plastiacero).
- **Horno de refinado** (`CVG_RefiningFurnace`): sin electricidad, combustible madera o piedras espirituales, se construye con
  piedra o metal + 10 piedras espirituales, cuenta como forja (bonus del Fénix). Recetas de acero espiritual, plata lunar, oro
  del caos y jade espiritual (jade vanilla + 2 piedras). Arte propio pendiente (usa la herrería de combustible teñida).
- **Pisos con qi** (menú "suelos espirituales"): baldosas de jade +0,02/casilla, piso de piedra espiritual +0,05, piso de
  madera espiritual +0,03 (usará madera espiritual cuando exista Flora), mosaico de cinco elementos +0,1.
- Pendiente: plata lunar contra entidades del vacío (Anomaly), seda de gusano espiritual, madera espiritual real, tablillas y
  talismanes de jade, piedras como combustible de formaciones y para cultivar.

**Cómo probar:** activar *Convergence: Resources* después del Core. Con `-quicktest -cvgselftest` el log muestra
"Resource self-test" (vetas antes y después de la Convergencia, materiales y pisos cargados).

### Rocas wuxia y feng shui por material (2026-10-15)
Cinco rocas naturales nuevas con la misma estructura que las vanilla (roca, roca pulida, trozo, bloques como material
pétreo, receta de corte y baldosa; los suelos rugoso/tallado/pulido los genera el juego):

| Roca | Rasgo | Qi del material |
|---|---|---|
| Roca de las Diez Tribulaciones | Durísima (1300 HP en roca, ×2,3 en muros), pesada | 0,03 |
| Jade negro de montaña | Dura y bella (belleza ×1,6) | 0,04 |
| Piedra nube | Ligera (0,5 kg/bloque), rápida de construir, frágil | 0,02 |
| Mármol celestial | La más bella (belleza ×2) | 0,03 |
| Piedra bermellón | Roja, de cinabrio (tinta de talismanes) | 0,03 |

- **Ajuste "Mundo wuxia"** (ajustes del Core, visible si hay rocas wuxia): las casillas del mundo eligen sus 2-3 tipos de
  roca solo entre las rocas wuxia (parche sobre `World.NaturalRockTypesIn`; los biomas con rocas forzadas no cambian).
  Desactivado: se mezclan con las vanilla.
- **Feng shui por material:** `DefModExtension_FengShui` en un *stuff* hace que cada pared, columna o mueble de ese
  material sume su qi a la habitación (bloques wuxia, jade espiritual 0,05, cristal espiritual 0,05). Las baldosas de
  rocas wuxia suman la mitad por casilla.

### Materiales por bioma (2026-10-15)
Vetas exclusivas de bioma (`DefModExtension_ConvergenceMineable.allowedBiomes`, Core: se comprueba al generar el mapa y en la
Convergencia) y plantas silvestres por bioma (parche a `wildPlants`). Lo de Odyssey va en `Mods/Odyssey` (carpeta condicional).

| Bioma | Material | Tipo | Uso |
|---|---|---|---|
| Campo de lava (Odyssey) | Cristal de fuego espiritual | Veta → material pétreo | Armadura/muros resistentes al fuego (calor ×1,6); combustible del horno de refinado |
| Campo de lava (Odyssey) | **Obsidiana de dragón** | Roca natural wuxia (`biomeSpecific`, siempre presente allí por `preferredBiomes`) | Bloques duros (HP ×2), qi 0,04 |
| Campo de lava (Odyssey) | Loto de llama | Planta → pétalo | Calor 1 día (temperatura cómoda −25 °C); yang para alquimia |
| Glaciar, tundra, hielo | Hierro frío de mil años | Veta (por temperatura y bioma) | Metal de filo extremo |
| Glaciar (Odyssey), tundra, boreal, ciénaga fría | Loto de nieve | Planta (Alchemy) | Hierbas espirituales |
| Bosque luminoso (Odyssey) | Jade lunar | Veta → material pétreo | Muy bello (×3,5), qi 0,05 |
| Bosque luminoso (Odyssey) | Hongo luminoso espiritual | Planta | Vista +15% un día |
| Tierras marcadas (Odyssey) | Jade de sangre | Veta → material pétreo | Material del camino demoníaco (etiqueta `DemonicPath`), qi 0,04 |
| Desierto, desierto extremo, matorral árido | Arena de oro solar (yang) | Veta | Combustible del horno de refinado; fundir 30 → 8 de oro |
| Selva tropical, pantano tropical | Fruta de sangre | Arbusto → fruta | Comida; +4 de sangre-qi (camino del cuerpo) y curación ×1,15 |
| Selva tropical, pantano tropical | Árbol espiritual → **madera espiritual** | Árbol → material leñoso | Muebles/muros/pisos con qi 0,03; el piso de madera espiritual ya la usa |
| Pantanos | Hierba venenosa | Planta | Resistencia a toxinas +30% dos días (con algo de toxicidad); base de técnicas de veneno |
| Costa y lagos (pesca de Odyssey) | Perla espiritual | Captura rara | Muy valiosa; qi 0,15 como objeto |
| Asteroides (Odyssey) | Acero galáctico | Veta | El mejor metal |

Pendiente: las plantas viven aquí y en Alchemy hasta que exista Fauna & Flora; sin Odyssey no hay perlas (no hay pesca).
