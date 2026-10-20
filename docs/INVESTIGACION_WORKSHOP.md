# Investigación de Workshop y GitHub (2026-10-15)

Objetivo: saber qué existe antes de construir, qué copiar *legalmente* y qué estudiar.

**Método.** Búsquedas en Steam Workshop (appid 294100) por la API pública `GetPublishedFileDetails` y el listado de búsqueda
(`cultivation`, `xianxia`, `修仙`, `immortal`, `wuxia`, `qi`, `dungeon`, `pocket dimension`, `shield generator`, etc.).
Licencias leídas de la API de GitHub y del archivo `LICENSE` del repositorio.
Los suscriptores son el valor "actual" del Workshop el 2026-10-15; la fecha es la de última actualización. "Versiones" = etiquetas del Workshop.

**Regla de licencias.** Un mod sin `LICENSE` en GitHub es "todos los derechos reservados": solo se estudia, no se copia.
Solo se reutilizan MIT, Apache, Unlicense y similares, con atribución. Las licencias NC/ND/SA **no** nos sirven.
"No confirmado" = no hay repositorio público o no se encontró archivo de licencia.

---

## ⚠️ Hallazgo principal: RimImmortal (边缘仙路) ya existe y es enorme

Es una **serie de ~15 mods chinos** (con traducción al inglés ~99 %) del equipo 堂丸 (TangW) y otros.
Es de lejos lo más grande que hay en el género. Llegó a 1.7 el mismo día de esta investigación.
Cubre **buena parte de lo que planeamos**: raíces espirituales, reinos, pastillas y alquimia, técnicas, facciones y sectas con rangos,
bestias y biomas, forja, muebles, cinco elementos. Es de código cerrado (no se encontró repositorio público).

**Implicaciones.**
1. No competimos en cantidad de contenido. Nos diferenciamos por lo que ellos no tienen: tecno-cultivo, Maestro y cadena de jades,
   disonancia celestial de implantes, tribulación con riesgo real, linajes/cuerpos especiales latentes, diseño con todas las DLC.
2. **Hay que instalarlo y jugarlo** como referencia de diseño y de balance. Es gratis y legal estudiar el juego.
3. Es una ventaja: la comunidad ya demostró que hay demanda (70 000 suscripciones históricas solo en su Core).

---

## Tabla A — Mods de cultivo, xianxia y wuxia

| Mod | Workshop | Autor | Versiones | Subs. (actual / histórico) | Qué hace bien | Qué aprender / solapamiento con nosotros |
|---|---|---|---|---|---|---|
| **RimImmortal-Core** (边缘仙路-羽化登仙) | [3296476341](https://steamcommunity.com/sharedfiles/filedetails/?id=3296476341) | 堂丸 (TangW) y equipo | 1.5, 1.6, 1.7 | 33 822 / 70 682 | Raíz espiritual, reinos, meditación, pastillas con hierbas, libros de técnicas, tutorial dentro del juego | **Solapa mucho** con nuestro Core. Estudiar UX del tutorial y curva de reinos. Código cerrado |
| RimImmortal-Faction (百家争鸣) | [3414794353](https://steamcommunity.com/sharedfiles/filedetails/?id=3414794353) | 堂丸 | 1.5–1.7 | 22 316 / 44 777 | 2 sectas interestelares rivales, **rango de secta con puntos de contribución**, culturas y memes | Modelo directo para nuestro módulo de sectas. Nuestro twist: la postura ante la tecnología la decide la Ideología |
| RimImmortal-Forge (神机天纵) | [3308873789](https://steamcommunity.com/sharedfiles/filedetails/?id=3308873789) | 堂丸 | 1.5–1.7 | 27 321 / 55 935 | Forja, armas y artefactos | Referencia para Forja Espiritual por niveles |
| RimImmortal-Living (俗世仙工) | [3326145525](https://steamcommunity.com/sharedfiles/filedetails/?id=3326145525) | 堂丸 | 1.5–1.7 | 25 254 / 51 489 | Recetas, bancos de trabajo, mobiliario, elementos de fantasía xianxia. Independiente del Core | Referencia de calidad de vida |
| RimImmortal-FiveElements (五行流转) | [3439473215](https://steamcommunity.com/sharedfiles/filedetails/?id=3439473215) | 堂丸 | 1.5–1.7 | 18 137 / 34 128 | Caminos de cultivo de los Cinco Elementos (incluye agua) | Compararlo con nuestras raíces/elementos |
| RimImmortal-Biome (山海灵域) | [3624919432](https://steamcommunity.com/sharedfiles/filedetails/?id=3624919432) | 堂丸 | 1.6, 1.7 | 12 053 | Biomas y fauna del Shan Hai Jing | Idea de bioma de qi para Fauna & Flora |
| RimImmortal-Bizarre (道诡异仙) | [3325655760](https://steamcommunity.com/sharedfiles/filedetails/?id=3325655760) | otro equipo (estilo "Dao of the Bizarre Immortal") | 1.5, 1.6 | 9 281 | Cultivo "extraño/horror" | Tono distinto, solo curiosidad |
| RimImmortal-SeasonsOfRule (四象之争), Exorcism (镇邪诛鬼), Farmcraft, Furniture, Hairstyle, 归墟前传 | ver búsqueda `修仙` | varios | 1.5–1.7 | 7 000–18 000 cada uno | Contenido satélite | Confirma que el ecosistema es modular (igual que el nuestro) |
| RimImmortal-Sword (剑心通明), Dual Cultivation (双修) | [3796450332](https://steamcommunity.com/sharedfiles/filedetails/?id=3796450332), [3808968532](https://steamcommunity.com/sharedfiles/filedetails/?id=3808968532) | terceros (addons no oficiales) | 1.6 | 689 / 244 | Espada y doble cultivo | Muestran que la comunidad extiende el Core: **conviene que el nuestro también sea extensible por XML** |
| **Ascension – Immortal Cultivation** | [2652340519](https://steamcommunity.com/sharedfiles/filedetails/?id=2652340519) | ano0bis | 1.3–1.7 | 3 703 / 8 720 | Cultivo taoísta con wiki propia, técnicas corporales, leyes y elementos combinables, "artes de técnica" | **Muy cercano** a nuestro diseño. Peso: 213 MB. Estudiar el sistema de leyes+elementos para nuestras técnicas (M5). Tiene Patreon: sin licencia pública |
| **Cultivator of the Rim** | [3221270722](https://steamcommunity.com/sharedfiles/filedetails/?id=3221270722) | Aranmaho / aranmaso | 1.5, 1.6, 1.7 | 3 717 / 8 401 | 4 etapas (Mortal → Dominio), 9 reinos menores × 9 niveles, pastilla de condensación, tribulación al romper muy seguido, espadas voladoras (Vehicle Framework), manuales vía It's Sorcery | Ya lo tenemos en `referencias/cultivo`. **Ya actualizado a 1.6/1.7**: conviene refrescar la copia. Repositorio [aranmaso/Cultivator-of-the-Rim](https://github.com/aranmaso/Cultivator-of-the-Rim) **sin licencia** |
| Murim Cultivation | [3820282209](https://steamcommunity.com/sharedfiles/filedetails/?id=3820282209) | no confirmado | 1.6 | 8 | 10 reinos × 10 pasos, nodos de meridianos, secta justa/demoníaca, mentoría, escenario "único sobreviviente de una secta caída" | Muy nuevo y pequeño. Interesante el escenario y los **meridianos**. Poco maduro |
| 修仙修炼 | [3740040604](https://steamcommunity.com/sharedfiles/filedetails/?id=3740040604) | no confirmado | 1.6 | 211 | Mod "AI-asistido", reinos hasta Núcleo/Alma Naciente, piedras espirituales con aura | Solo curiosidad; autor dice "uso personal" |
| 武道 (Wudao) | [3729701464](https://steamcommunity.com/sharedfiles/filedetails/?id=3729701464) | no confirmado | 1.6 | 1 436 | Artes marciales por experiencia de combate, 7 reinos, técnicas pasivas/activas | Camino de cultivo corporal; idea para "cultivo por combate" |
| **MoeLotl: Painted Jianghu** (绘卷江湖) | [3795055620](https://steamcommunity.com/sharedfiles/filedetails/?id=3795055620) | no confirmado | 1.6, 1.7 | 4 723 / 5 500 | Expansión wuxia para la raza MoeLotl: 5 tipos de artes con ranura propia | Ejemplo de **artes por ranuras** (golpe, cuerpo, ligereza, interna) |
| Xianxia Cultivation | (no localizado en Workshop actual; solo espejos) | KilThrall | no confirmado | no confirmado | Reescritura difícil de un mod antiguo | Probablemente abandonado |
| Immortals / Immortal Regenerant Gene / Immortals Gene | [1984905966](https://steamcommunity.com/sharedfiles/filedetails/?id=1984905966) y otros | varios | 1.6 (el primero) | 29 791 (Immortals) | Inmortalidad *regenerativa*, no cultivo | **No** es del género. Ignorar |

Notas:
- Búsquedas con `wuxia` y `xianxia` casi no devuelven resultados: **el género se llama "Immortal/仙" en el Workshop**, no xianxia.
- No se encontró ningún mod de cultivo con código abierto y licencia permisiva.

---

## Tabla B — Frameworks y piezas que vamos a necesitar

| Necesidad | Mod | Workshop | Versiones | Licencia (verificada en GitHub) | Uso para nosotros |
|---|---|---|---|---|---|
| Harmony | Harmony (pardeike) | [2009463077](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077) | 1.2–1.7 | **MIT** ([repo](https://github.com/pardeike/HarmonyRimWorld)) | Dependencia obligatoria, ya la usamos |
| Gráficos de pawn por genes / razas | **Humanoid Alien Races** (erdelf) | [839005762](https://steamcommunity.com/sharedfiles/filedetails/?id=839005762) | 1.0–1.7 | **MIT** ([AlienRaces](https://github.com/erdelf/AlienRaces)) | Módulo Races, si hacen falta razas con cuerpos propios. Nuestro diseño dice "no obligatorio": integración opcional |
| Capas de render múltiples en pawn | NareisLib | [2999614450](https://steamcommunity.com/sharedfiles/filedetails/?id=2999614450) | 1.4–1.7 | **MIT** ([repo](https://github.com/Kamijouko/NareisLib)) | Aura/ropa por capas. Solo si `renderNodeProperties` no alcanza |
| Magia con árboles/"sistemas" de habilidades | **It's Sorcery!** (Zomuro) | [3013487115](https://steamcommunity.com/sharedfiles/filedetails/?id=3013487115) | 1.4, 1.5, 1.6 | **MIT** ([framework](https://github.com/Zomuro/ItsSorceryFramework), [ejemplo](https://github.com/Zomuro/ItsSorceryExample)) | **Estudio y reutilización de partes** para M5 (técnicas y progresión). Cultivator of the Rim ya lo usa |
| Sistema de psicasts con árboles | Vanilla Psycasts Expanded | [2842502659](https://steamcommunity.com/sharedfiles/filedetails/?id=2842502659) | 1.5–1.7 | **No confirmado** (el repo [VanillaPsycastsExpanded](https://github.com/Vanilla-Expanded/VanillaPsycastsExpanded) no declara licencia) | Solo estudio (ya en `referencias/`) |
| Framework VE (genes, mapas, ajustes…) | Vanilla Expanded Framework | [2023507013](https://steamcommunity.com/sharedfiles/filedetails/?id=2023507013) | 1.0–1.7 | **CC BY-NC-ND 4.0** ([LICENSE](https://github.com/Vanilla-Expanded/VanillaExpandedFramework/blob/main/LICENSE)): **no se puede reutilizar código** | Solo estudio. Como dependencia opcional (`MayRequire`) sí es legal |
| Edificios que contienen un mapa / inventario extra (anillo espacial) | Pocket Dimensions (CM) / **Dimensions RePocketed** | [3554632020](https://steamcommunity.com/sharedfiles/filedetails/?id=3554632020) (original [2307817213](https://steamcommunity.com/sharedfiles/filedetails/?id=2307817213)) | 1.2–1.6 | Original [CM_Pocket_Dimension](https://github.com/CaptainMuscles/CM_Pocket_Dimension): **sin licencia**; RePocketed: no confirmado | Estudio. Para el "anillo espacial" probablemente basta el mapa de bolsillo vanilla de Odyssey/Anomaly |
| Mazmorras / reinos secretos | **Dungeon Core** | [3064597982](https://steamcommunity.com/sharedfiles/filedetails/?id=3064597982) | 1.4–1.7 | [ying636/DungeonCore](https://github.com/ying636/DungeonCore): **sin licencia** | Estudio: misión de mazmorra, mapas personalizados, monstruos subterráneos |
| Mazmorras | Isekai Dungeons Add-on | [3813394602](https://steamcommunity.com/sharedfiles/filedetails/?id=3813394602) | 1.6, 1.7 | [Poupun/IsekaiDungeons](https://github.com/Poupun/IsekaiDungeons): **sin licencia** | Estudio (el mismo autor que Isekai RPG Leveling, que ya tenemos) |
| Escudos de área / cúpulas | Shield Generators (Outpost-21) | [2540563802](https://steamcommunity.com/sharedfiles/filedetails/?id=2540563802) | 1.3–1.6 | [Outpost-21/Shield-Generators](https://github.com/Outpost-21/Shield-Generators): **sin licencia** | Estudio |
| Escudos de área | **Shield Generator** (JohnCannon87) | [3591079938](https://steamcommunity.com/sharedfiles/filedetails/?id=3591079938) | 1.6 | **Unlicense (dominio público)** ([repo](https://github.com/JohnCannon87/RimworldShieldGenerator)) | **Reutilizable** para formaciones de escudo (módulo Defensa). Revisar calidad antes de copiar |
| Escudos de área (otros) | Enhanced Defence (Jaxxa) | no confirmado | no confirmado | [jaxxa/ED-Shields](https://github.com/jaxxa/ED-Shields) **MIT**, último push 2023 | Posible fuente de ideas; probablemente obsoleto |
| Vehículos (espadas voladoras) | **Vehicle Framework** (SmashPhil) | [3014915404](https://steamcommunity.com/sharedfiles/filedetails/?id=3014915404) | 1.4–1.6 | **MIT** ([repo](https://github.com/SmashPhil/Vehicle-Framework)) | Espadas voladoras como dependencia **opcional** (como hace Cultivator of the Rim). 780 000 subs |
| Vehículos en mapas | Vehicle Map Framework | [3426502333](https://steamcommunity.com/sharedfiles/filedetails/?id=3426502333) | 1.5, 1.6 | **CC BY-NC-SA 4.0**: no reutilizable | Ignorar |
| Inventario extra (anillo espacial, almacenamiento) | **Adaptive Storage Framework** | [3033901359](https://steamcommunity.com/sharedfiles/filedetails/?id=3033901359) | 1.4–1.6 | **MIT** ([repo](https://github.com/bbradson/Adaptive-Storage-Framework)) | Almacenes espirituales. Más para edificios que para inventario personal |
| Inventario RPG (huecos para colgantes, anillos, talismanes) | **RPG Style Inventory Revamped** (avilmask) | [2478833213](https://steamcommunity.com/sharedfiles/filedetails/?id=2478833213) (962 mil históricas, 575 mil actuales) | 1.2–1.6 (etiqueta 1.7 ausente en Workshop, no confirmado) | [catgirlfighter/-1.0-RPG-Style-Inventory-V3.0-](https://github.com/catgirlfighter/-1.0-RPG-Style-Inventory-V3.0-): **sin licencia** (el original de Sandy tampoco) | Solo interfaz (cosmético). **Los huecos se definen por XML**: ver sección "Inventario RPG y capas CVG_" |
| Jefes / barras de vida | Legendary Boss | [3538894636](https://steamcommunity.com/sharedfiles/filedetails/?id=3538894636) | 1.6 | No confirmado (solo marca un pawn con barra de vida) | Referencia de UI de jefe; trivial |
| Pathfinding personalizado | Pathfinding Framework (joseasoler) | no confirmado | no confirmado | **MIT** ([repo](https://github.com/joseasoler/Pathfinding-Framework)) | Solo si hay terrenos especiales (agua espiritual); poco probable |
| Misiones encadenadas (el Maestro) | — | — | — | — | No encontré un framework dedicado. Usar el `QuestScriptDef`/`QuestPart` de vanilla; estudiar Vanilla Quests Expanded (código no reutilizable) |
| Sitios del mundo | — | — | — | — | Igual: sitio de mundo vanilla (`WorldObject`, `SitePartDef`); estudiar VE Landmarks |
| Ajustes del mod | HugsLib / ModSettings vanilla | [818773962](https://steamcommunity.com/sharedfiles/filedetails/?id=818773962) | 1.0–1.6 | no verificada | Usar `ModSettings` vanilla; **no** depender de HugsLib |
| Mods de ajustes XML | XML Extensions (15adhami) | — | — | **GPL-3.0** | **No usar**: GPL obligaría a licenciar nuestro código igual |

---

## Inventario RPG y capas de ropa nuevas (`CVG_Neck`, `CVG_Finger`, `CVG_Waist`)

**Mods encontrados** (todos solo cosméticos: reemplazan la pestaña Equipo):

| Mod | Workshop | Versiones | Notas |
|---|---|---|---|
| **RPG Style Inventory Revamped** (avilmask) | [2478833213](https://steamcommunity.com/sharedfiles/filedetails/?id=2478833213) | 1.2–1.6 | El más usado (575 mil). Huecos por XML. `packageId`: `Sandy.RPGStyleInventory.avilmask.Revamped` |
| RPG Style Inventory (Sandy, original) | [1561221991](https://steamcommunity.com/sharedfiles/filedetails/?id=1561221991) | 1.0–1.6 | Original, 333 mil. Mismo árbol de repositorio ([SandyTheGreat](https://github.com/SandyTheGreat/RPG-Style-Inventory)), sin licencia |
| **True RPG Inventory** | [3744201621](https://steamcommunity.com/sharedfiles/filedetails/?id=3744201621) | 1.6 | Estilo Diablo: muñeco con *amuleto, dos anillos, cinturón…* y cuadrícula tetris. Parte del código fue generado con IA (lo declara el autor). Repositorio y licencia: no confirmado. Si cubre "anillo/amuleto" de fábrica es el candidato natural para nuestras capas, pero **no se verificó cómo asigna cada prenda a un hueco** |
| Nice Inventory Tab | [3609897594](https://steamcommunity.com/sharedfiles/filedetails/?id=3609897594) | 1.6 | Pestaña tipo tabla; 177 mil. Parche de convivencia con Revamped: [3677781354](https://steamcommunity.com/sharedfiles/filedetails/?id=3677781354) |
| Awesome Inventory 1.6 | [3552214807](https://steamcommunity.com/sharedfiles/filedetails/?id=3552214807) | 1.6 | Autor dice que ya no lo mantiene |

**¿Revamped soporta capas nuevas de otros mods?** Sí, **sin código**, pero hay que añadir los huecos por XML:

- Cada hueco es un def `Sandy_Detailed_RPG_Inventory.ItemSlotDef` con `xPos`/`yPos` (rejilla de 7 columnas × 6 filas aprox.), `apparelLayers` (lista de `ApparelLayerDef`), `bodyPartGroups`, `validationOrder`, y opcionalmente `anchor` y `Default`.
  Ejemplo real del repo: `ItemSlotNeckAccessories` (layer `Accessories`, posición 0,1).
- Según la descripción del autor: no muestra huecos *indefinidos*, y la ropa que no cabe en ningún hueco **aparece en una lista aparte**. O sea, sin parche no rompe: las prendas `CVG_*` se verían en esa lista, no en el muñeco.
- El propio mod trae huecos de otros mods (Vanilla Factions Expanded, Combat Extended, Rimworld of Magic…) con parches, y huecos "PH" (placeholder) para artefactos, así que es el patrón esperado.

**Cómo se haría el parche.** Un archivo en nuestro mod que solo se carga si Revamped está activo, por ejemplo
`Mods/Convergence.Core/LoadFolders` con `IfModActive="Sandy.RPGStyleInventory.avilmask.Revamped"`, que contenga tres `ItemSlotDef`:

```xml
<Sandy_Detailed_RPG_Inventory.ItemSlotDef>
  <defName>CVG_ItemSlotNeck</defName>
  <label>Pendant</label>
  <xPos>..</xPos><yPos>..</yPos>        <!-- elegir una celda libre de la rejilla -->
  <validationOrder>..</validationOrder> <!-- número alto/único para no pisar otros -->
  <apparelLayers><li>CVG_Neck</li></apparelLayers>
  <bodyPartGroups><li>Neck</li></bodyPartGroups>
</Sandy_Detailed_RPG_Inventory.ItemSlotDef>
```
(y lo equivalente para `CVG_Finger` y `CVG_Waist`). Esto **no** es dependencia dura: sin Revamped el archivo no se carga (carpeta condicional), igual que con las DLC.

**Pendiente de verificar antes de implementarlo** (no lo leí en el código C#):
1. Si `bodyPartGroups` es obligatorio o si basta `apparelLayers`.
2. Qué celdas quedan libres: el mod ya usa casi toda la rejilla (hay huecos en `Neck Accessory`, `Belt`, `Right/Left Hand Accessory`). Podemos **reutilizar** `Accessories` en vez de crear capas nuevas si no queremos tocar la rejilla.
3. Si Revamped ya funciona en 1.7 (el Workshop solo lista 1.6).

**Licencia:** sin licencia en GitHub → no se copia nada de sus archivos. Escribir nuestros propios `ItemSlotDef` es **uso por API pública (XML)**, no copia de código, y es práctica habitual (los mods de Combat Extended y VFE lo hacen). Es la parte menos arriesgada, pero conviene indicarlo en `docs/`.

---

## Recomendación

### 1. Qué instalar como referencia (para jugar y estudiar)

Prioridad alta:

| Mod | ID de Workshop | Por qué |
|---|---|---|
| **RimImmortal-Core** | 3296476341 | El competidor principal. Jugarlo unas horas |
| **RimImmortal-Faction** | 3414794353 | Sectas y rangos de secta |
| RimImmortal-Forge | 3308873789 | Forja y artefactos |
| **Ascension – Immortal Cultivation** | 2652340519 | El otro modelo de cultivo profundo; leer su [wiki](https://rimworldascension.wiki.gg/) |
| **Cultivator of the Rim** (refrescar copia actual) | 3221270722 | Ya actualizado a 1.6/1.7 |
| It's Sorcery! | 3013487115 | Lo usa Cultivator y es MIT |
| Dungeon Core | 3064597982 | Mazmorras con misión |

Prioridad media: RimImmortal-FiveElements (3439473215), Murim Cultivation (3820282209: escenario "secta caída" y meridianos),
MoeLotl Painted Jianghu (3795055620: artes por ranuras), Vehicle Framework (3014915404), Dimensions RePocketed (3554632020).

Recomendación práctica: instalar solo Core + Faction + Ascension + Cultivator para empezar, en una instalación de **pruebas aparte** (no en la de desarrollo).

### 2. Qué podemos reutilizar por licencia, y para qué módulo

| Código / mod | Licencia | Módulo | Condiciones |
|---|---|---|---|
| Harmony | MIT | Core (ya) | Conservar aviso de copyright si se redistribuye |
| It's Sorcery! (Zomuro) | MIT | Core M5 (técnicas), Races | Aviso MIT con el nombre del autor en el README del mod. Mejor adaptar ideas que acoplar |
| Humanoid Alien Races (erdelf) | MIT | Races (opcional) | Preferible dependencia opcional, no copiar |
| NareisLib | MIT | Aura/capas de render | Solo si `renderNodeProperties` no alcanza; dependencia opcional |
| Vehicle Framework (SmashPhil) | MIT | Espadas voladoras (Qi-Tech o módulo propio) | Dependencia opcional con `MayRequire` |
| Adaptive Storage Framework | MIT | Edificios, almacenes espirituales | Dependencia opcional |
| **JohnCannon87/RimworldShieldGenerator** | **Unlicense** | Defensa: formaciones/escudos | Dominio público. Verificar que el código esté bien y sea de 1.6 |
| Pathfinding Framework | MIT | Solo terrenos especiales | Poco probable |

Para todo lo MIT: si copiamos código, incluir en `LICENSE-THIRD-PARTY.md` el texto de la licencia y la atribución.
Si solo lo usamos como dependencia opcional, no hay obligación de copiar nada.

### 3. Qué ignorar (o solo estudiar)

- **Sin licencia → solo estudio:** Cultivator of the Rim, Ascension, toda la serie RimImmortal, Dungeon Core, Isekai Dungeons, Pocket Dimensions, Shield Generators (Outpost-21), Vanilla Psycasts Expanded.
- **Licencia que impide reutilizar:** Vanilla Expanded Framework (CC BY-NC-ND), Vehicle Map Framework (CC BY-NC-SA), XML Extensions (GPL-3.0: contagia).
- **No relevantes:** Immortals / Immortal Regenerant Gene (inmortalidad regenerativa), Wild Cultivation y mods de cultivo agrícola, los mods "MoeLotl" (raza), Rice Cultivating, Psilocap, Milira, mods de cultivo de plantas.
- **Poco fiables:** 修仙修炼 (autor declara "AI-assisted, uso personal"), Xianxia Cultivation (no localizado vigente), Murim Cultivation (8 suscriptores, tiene 1 día de vida).

### Inventario RPG (añadido)
- **Instalar:** RPG Style Inventory Revamped (2478833213) para ver el hueco de colgante/anillo/talismán, y True RPG Inventory (3744201621) para comparar cómo trata amuletos y anillos.
- **Decisión de diseño:** dar soporte opcional a Revamped con un parche XML condicional (3 `ItemSlotDef`). No depender de ningún mod de inventario.

### Conclusiones para el diseño

1. **Diferenciación:** RimImmortal es chino y enorme; no podemos ganarle en volumen. Nuestro valor es la identidad (tecno-cultivo + Convergencia + Maestro) y un Core extensible por XML.
2. **No reinventar:** pastillas, sectas con rangos y forja ya están bien resueltos por ellos; podemos aprender de su UX.
3. **Piezas reutilizables reales son pocas** (todas MIT/Unlicense): Harmony, It's Sorcery, HAR, Vehicle Framework y un shield generator. El resto del ecosistema de cultivo está cerrado: hay que programarlo nosotros.
4. **Mazmorras y reinos secretos:** ningún mod ofrece una pieza con licencia. Hay que construirlo con el `MapGenerator`/`PocketMap` vanilla (Odyssey/Anomaly).

### Pendiente / no confirmado

- Licencia de Dimensions RePocketed, Legendary Boss, Toggleable Shields, Spacer Shields, Vanilla Psycasts Expanded.
- Autor y versión exacta de Murim Cultivation, Wudao, Painted Jianghu.
- Si RimImmortal publica código en algún sitio (no se encontró en GitHub ni en Gitee).
