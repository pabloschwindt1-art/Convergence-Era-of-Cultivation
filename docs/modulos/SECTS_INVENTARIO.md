# Convergence: Sects — resumen e inventario (para diseñar con otra IA)

Estado al **2026-10-22** (mod v0.1.1, publicado oculto en Steam Workshop, RimWorld 1.6 y 1.7). Lo generé leyendo los archivos reales del
mod. `SECTS_IMPLEMENTACION.md` está desactualizado: dice que no hay rangos ni Ideología, y ya existen los dos.

---

## 1. Qué es el proyecto (contexto para la otra IA)

- **Convergence: Era of Cultivation** es un mod de RimWorld de cultivo xianxia/wuxia, inspirado en *Tale of Immortal* y en el subgénero 灵气复苏
  (renacimiento del qi). Tono **tecno-cultivo**: el planeta Jiuxiao vive la **Convergencia**, el regreso del qi a un mundo con tecnología.
- El mod se divide en un **Core** (framework: cultivo, qi, reinos, técnicas, Convergencia) y **14 módulos**. Este documento trata el módulo
  **Sects**, que trae las sectas como facciones del mundo.
- **Reglas firmes** (cualquier propuesta tiene que respetarlas):
  1. **Todo lleva lore.** Cada objeto, facción, rango o evento tiene una descripción de 2-3 frases con origen o leyenda, en inglés y en español.
  2. **Versión wuxia de lo que ya existe.** Antes de inventar un sistema, buscar el equivalente vanilla o de una DLC (facciones, buena
     voluntad, misiones, rituales, Ideology, Royalty) y hacer su versión con estética y lore propios. Nada de sistemas paralelos.
  3. **Las sectas NO son anti-tecnología.** La postura ante la tecnología la decide la Ideología del jugador. Nada de "tribales raros".
  4. **Se juega con y sin tecnología:** el Core nunca exige electricidad.
  5. **Datos en XML, lógica en C#.** Todo contenido nuevo tiene que poder agregarse con XML (sin listas cerradas en el código).
  6. Cada función de una DLC va aparte: sin la DLC, el mod funciona sin esa parte.
- **Reinos de cultivo** (orden): 1 Refinamiento de Qi · 2 Establecimiento de Fundación · 3 Núcleo Dorado · (4 Alma Naciente, 5 y 6 **todavía no
  existen**). Hay también un camino del cuerpo (templado con sangre de bestia).
- **Elementos de la raíz espiritual:** Metal, Madera, Agua, Fuego, Tierra (+ Trueno como variante).
- **Lore de referencia:** `docs/LORE.md` y `docs/lore/SECTAS.md` (392 líneas: fichas de las 6 sectas con nombres chinos, rangos, técnicas
  firma por elemento, relaciones, qué ofrece el jugador e ideas de ideología). **Conviene pasarle también ese archivo a la otra IA.**

---

## 2. Cómo funciona hoy (lo que ya hace el código)

| Mecánica | Cómo funciona | Dónde |
|---|---|---|
| **Llegada con la Convergencia** | Las sectas **no existen** al crear el mundo. Cuando ocurre la Convergencia aparecen con 1 asentamiento más 1-2 extra, y quedan fijadas sus alianzas y enemistades. Antes no hay asaltos, comercio ni asentamientos de secta. | Core: `DefModExtension_ConvergenceFaction` |
| **Discípulos que ya cultivan** | Cada tipo de personaje (PawnKind) lleva reino, etapa, manuales de técnicas preferidos y elementos de raíz. Se generan ya cultivando. | Core: `DefModExtension_CultivatorPawnKind` |
| **Dificultad justa** | Ajuste del Core (activado por defecto): un enemigo **nunca supera en más de 1 reino** al mejor cultivador de la colonia. | Core: `scaleWithColony` + ajuste `scaleEnemyRealms` |
| **Rangos del jugador en cada secta** | Usa la buena voluntad vanilla. El rango es el más alto cuya buena voluntad mínima se cumple **y** cuyo reino mínimo alcanza algún colono. Al subir llega una carta y un regalo en cápsula. | Core: `FactionRankDef` + `GameComponent_FactionRanks` |
| **Neutralidad por meme** | Una secta puede llegar neutral si la ideología del jugador tiene cierto meme (p. ej. el Culto de la Luna Sangrienta con el Camino Demoníaco). | `neutralToPlayerMemes` |
| **Rituales de secta (Ideology)** | Admisión (un colono se une a la ideoligión de la secta), ascensión (un miembro sube de reino) y torneo. | Core: `SectRituals.cs` + XML del módulo |
| **Política entre sectas** | Guerras, pactos, riñas y torneos entre dos sectas al azar; el jugador puede ayudar a un bando o mediar. | Stories: `SectWorld.cs` |

---

## 3. Inventario

### 3.1 Las 6 facciones (`FactionDef`)
| defName | Nombre | Nivel tecnológico | Hostil con | Aliada con | Notas |
|---|---|---|---|---|---|
| `CVG_Faction_AzurePeak` | Secta de la Espada del Pico Azur | Medieval | Luna Sangrienta | Loto, Dragón | Espadas, Metal/Agua |
| `CVG_Faction_SilentLotus` | Templo del Loto Silencioso | Medieval | Luna Sangrienta | Pico Azur, Errantes | Monjes, hierbas, Tierra/Madera |
| `CVG_Faction_HeavenlyCauldron` | Pabellón del Caldero Celestial | **Industrial** | — | — | Alquimia y comercio; neutral con todos |
| `CVG_Faction_AncientDragon` | Clan del Dragón Antiguo | Medieval | Luna Sangrienta | Pico Azur | Todos desde Núcleo Dorado |
| `CVG_Faction_BloodMoon` | Culto de la Luna Sangrienta | Medieval | todas menos el Caldero | — | `naturalEnemy`; neutral si el jugador sigue el Camino Demoníaco |
| `CVG_Faction_RogueCultivators` | Cultivadores errantes | Medieval | Luna Sangrienta | Loto | Sin manuales fijos |

Cada una tiene un generador de nombres de personas (apellido + nombre, estilo chino) y uno de asentamientos (`RulePackDef CVG_Namer*`), más
un ícono de facción en `Textures/World/CVG/Factions` (6 imágenes).

### 3.2 Tipos de personaje (`PawnKindDef`, 27)
"cp" = poder de combate (lo que cuesta en un asalto). Etapa = rango dentro del reino.

| Secta | Rango 1 | Rango 2 | Rango 3 | Rango 4 | Comerciante | Manuales | Elementos |
|---|---|---|---|---|---|---|---|
| Pico Azur | outer sword disciple (QiRef 1-3, cp 55) | inner sword bearer (Fund 1-2, 105) | core blade of the peak (Fund 3-4, 140) | sword elder (Núcleo 1-3, 250) | peak swordsmith | GoldenEdge, FlowingCloud | Metal, Agua |
| Loto Silencioso | lay apprentice (QiRef, 50) | temple monk (Fund, 100) | temple guardian (Fund, 135) | venerable (Núcleo, 240) | temple herbalist | MountainHeart, EvergreenSutra | Tierra, Madera |
| Caldero Celestial | cauldron apprentice (QiRef, 50) | alchemist (Fund, 100) | cauldron master (Fund, 135) | grand alchemist (Núcleo, 240) | pavilion auctioneer | BlazingSun, EvergreenSutra | Fuego, Madera |
| Dragón Antiguo | — | dragon watcher (Núcleo 1-2, 230) | clan elder (Núcleo 3-4, 300) | — | clan hoarder (Núcleo) | FlowingCloud, MountainHeart | Agua, Tierra |
| Luna Sangrienta | blood servant (QiRef, 55) | blood disciple (Fund, 110) | crimson core (Fund, 145) | blood elder (Núcleo, 250) | — (no comercia) | FlowingCloud, BlazingSun, EvergreenSutra | Agua, Fuego, Madera |
| Errantes | wandering cultivator (QiRef, 55) | cultivator sellsword (Fund, 105) | veteran wanderer (Fund, 140) | hidden hermit (Núcleo, 245) | night-market peddler | (reparto normal) | (al azar) |

Linajes (con el módulo Races): Loto → Qilin 30 %; Pabellón → Fénix o Zorro 12 % cada uno; Dragón → Dragón 100 %.

**Manuales de técnicas que existen hoy (Core):** GoldenEdge (Metal), EvergreenSutra (Madera), FlowingCloud (Agua), BlazingSun (Fuego),
MountainHeart (Tierra), SwordHeart, SpiritSense, VajraBody, NineHeavensThunder. **Ninguna secta tiene técnica firma propia**: usan los
genéricos por elemento.

### 3.3 Rangos del jugador (`Convergence.FactionRankDef`, 24 = 4 por secta)
Mismo esquema para las 6 sectas. Cambian los nombres; el Dragón exige Núcleo Dorado desde el rango 2 y los Errantes no exigen reino.

| Orden | Buena voluntad mínima | Reino mínimo de algún colono | Regalo al subir |
|---|---|---|---|
| 1 | 20 | — | 20 piedras espirituales |
| 2 | 40 | Fundación | 1 tablilla de jade + 50 piedras |
| 3 | 60 | Núcleo Dorado | 1 tablilla + 5 jade espiritual + 100 piedras |
| 4 | 85 | Núcleo Dorado | 15 jade espiritual + 250 piedras |

Nombres: Pico Azur (sword apprentice → sword bearer → blade of the peak → sword elder) · Loto (lay apprentice → monk → guardian → venerable) ·
Caldero (cauldron apprentice → alchemist → cauldron master → grand alchemist) · Dragón (hatchling → young dragon → watcher → clan elder) ·
Luna Sangrienta (blood servant → blood disciple → crimson core → blood elder) · Errantes (wanderer → known → renowned → legend).

**El rango hoy solo da una carta y un regalo:** no desbloquea nada, no tiene deberes ni misiones propias.

### 3.4 Comerciantes (`TraderKindDef`, 7)
`CVG_Trader_AzurePeak` (armas de filo), `CVG_Trader_SilentLotus` (hierbas y ropa, sin armas), `CVG_Trader_HeavenlyCauldron` (píldoras de
fundación, piedra y jade espiritual, tablillas de jade; compra casi todo), `CVG_Trader_AncientDragon` (tesoros), `CVG_Trader_RogueCultivators`
(de todo un poco), `CVG_Trader_CauldronShip` (nave voladora del Caldero) y `CVG_Trader_RogueWanderer` (mercader errante).

### 3.5 Ideología (solo con la DLC Ideology: `Mods/Ideology`)
- **Memes de secta (9):** SwordWay, LotusPath, CauldronWay, DragonBlood, DemonicWay, WanderingPath, BodyWay, PuppetPath, ThunderWay.
- **Memes de estructura (3):** Origen del Dao, Mandato del Cielo, Culto ancestral.
- **Presets de ideoligión (9):** uno por meme de secta, para empezar jugando como esa secta.
- **Temas de preceptos (7) con 20 opciones:**
  - Cultivo: obligatorio / venerado / libre / mal visto.
  - Alquimia y píldoras: venerada / aceptada / mal vista.
  - Extracción de esencia: aborrecible / aceptada / honrada.
  - Artes demoníacas: prohibidas / toleradas / abrazadas.
  - Duelos entre discípulos: alentados / permitidos / prohibidos.
  - Jerarquía de secta: mandan los ancianos / todos iguales.
  - Retiro de avance: reverenciado / común.
  - Cada opción tiene sus pensamientos (moods).
- **Roles (7):** maestro de secta, gran anciano, anciano de la espada, maestro alquimista, maestro de formaciones, guardián de las escrituras,
  maestro titiritero.
- **Rituales (4):** ceremonia de admisión, ceremonia de ascensión, torneo de secta, funeral del Dao.
- **Objetos:** altar ancestral, estela de escrituras, 3 reliquias (espada del fundador, jade del fundador, sello de la secta).

### 3.6 Lo que otros módulos hacen con las sectas
- **Stories:** **47 de sus 111 eventos** usan las sectas. Por ejemplo:
  - visitas a cada secta (VisitAzurePeak, VisitLotus, VisitCauldron, VisitDragon, VisitRogueMarket);
  - misiones (arañas, hierbas del Loto, núcleos para el Caldero, escolta de caravana, torneo del Pico, tributo al Dragón, manual robado);
  - política (guerra, pacto comercial, rivalidad, torneo, tributo, traslado de discípulos);
  - eventos del Culto (pacto demoníaco, deuda de sangre);
  - recaudadores del Pabellón, enemistades con duelos, propuesta de alianza.
- **Stories, escenarios (4):** Discípulos perdidos, Alquimista exiliado, Refugiados del Loto, Heredero del Dragón.
- **Races:** linajes de bestia en los discípulos (ver 3.2).

### 3.7 Arte
Son 35 imágenes: íconos de las 6 facciones, 9 de memes, 7 de roles, 3 de estructuras y 3 de rituales, 4 de edificios y 3 de objetos.
**No hay arte propio de los discípulos:** usan ropa y aspecto genéricos.

---

## 4. Lo que falta (huecos conocidos)

1. **Técnicas firma por secta.** El lore (`SECTAS.md`) las describe, pero el juego no las tiene.
2. **El rango no da nada**, salvo el regalo: no hay beneficios, acceso a tiendas, deberes ni misiones atadas al rango.
3. **Asaltos de reino alto sin "peso":** falta la "presión de aura" (DEFENSA §4), es decir, que un Núcleo Dorado enemigo se sienta al llegar.
4. **IA de combate de cultivadores enemigos:** usar técnicas, escudo de qi y supresión con criterio.
5. **Identidad visual:** túnicas o uniformes por secta (enlaza con el módulo Apparel) y ropa de rango.
6. **Reinos 4-6:** los ancianos topan en Núcleo Dorado porque Alma Naciente no existe.
7. **Facciones del Borde** del lore (el Imperio, Ascension Dynamics) no existen como facciones.
8. **Asentamientos de secta** son asentamientos comunes del mapa mundial: no hay "monte de la secta" visitable con algo propio.
9. **Grietas** de la Convergencia: solo sabor, sin nada visual.

---

## 5. Cómo devolver la propuesta (para que la pueda implementar)

Pedile a la otra IA que entregue:
- **Por cada idea:** qué sistema vanilla o de DLC reutiliza (regla 2), qué secta afecta, el texto de lore en inglés y en español (2-3 frases)
  y si exige alguna DLC.
- **Nombres propuestos de defName** con prefijo `CVG_` y el tipo de def (FactionDef, PawnKindDef, AbilityDef, HediffDef, QuestScriptDef,
  PreceptDef…).
- **Números:** reinos, costos, buena voluntad, poder de combate. Todo como datos ajustables, no como reglas fijas.
- **Qué necesita código nuevo** y qué se resuelve solo con XML (preferir XML).
- **Lista de arte necesario:** qué imagen, tamaño aproximado y para qué.

Yo después lo valido contra los archivos reales, lo adapto a las reglas técnicas y lo programo.
