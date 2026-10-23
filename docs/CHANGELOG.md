# Registro de cambios (notas de versión para Steam Workshop)

Cada cambio a un mod publicado se anota acá, por mod, en **"Sin publicar"**. Al subir: se sube `modVersion` en el `About.xml`
del mod, se copia el texto en inglés a las *change notes* de Steam y la sección pasa a **"Publicadas"** con la fecha.
Pedido del usuario (2026-10-21): versionar todo lo que se modifica en los mods y addons para subirlo después.

Estado de partida: los 15 mods se publicaron (ocultos) en la **0.1.0** (tag git `v0.1.0`; ids en `About/PublishedFileId.txt`).

---

## Sin publicar

Próxima subida (juntar todo en una sola vez; ver la nota de abajo para cada mod).

### Alquimia y salas de trabajo (2026-10-22)
- **Salas de trabajo wuxia** (Core): sala de alquimia (horno), herrería espiritual (forja), sala de escribas (mesa de runas, de
  talismanes, escritorio de caligrafía) y sala de tejido (telar espiritual), la versión del taller y el laboratorio vanilla. Nuevo stat
  de mesa **calidad de la sala de trabajo**: suma niveles de calidad por la limpieza (sucia resta), la impresión y la concentración de
  qi de la sala, por los **muebles vinculados** (instalaciones vanilla: armario de píldoras, incensario, estantería de sutras, brasero
  ritual, guardarropa, rollo pintado, linterna de piedra espiritual) y por el fuego del horno. Una buena sala compensa a un artesano
  flojo. La estantería de sutras también acelera el escritorio de contemplación y la jarra de grano la cocina de secta.
- **El horno necesita fuego** y el jugador elige cuál: leña (+0), piedra espiritual (+0.25) o cristal de fuego (+0.5, con Resources).
  Cada horno quema solo el combustible elegido.
- **La alquimia puede fallar**: cada píldora sale de una tabla según el poder del alquimista (habilidad, sala, nivel del horno,
  maestría alquímica e inspiración). Base: 30% falla o mala, 45% normal, 20% excelente, 5% celestial (legendaria); un gran maestro
  con todo a favor llega a 5 / 30 / 40 / 25. Si falla sale una **píldora de desecho** (arte del usuario; def en el Core, así también falla igual sin Alchemy, por ejemplo la de Fundación en el puesto de fabricación). Las otras 13 píldoras nuevas del usuario están en rt/pildoras_nuevas/, para cuando se programen.
- Nuevo stat del colono **maestría alquímica**: la dan el rasgo de alquimista innato (+0.25 / +0.5 / +1), el cuerpo de yang puro
  (+0.1 a +0.5), las historias de alquimista del Core y del Caldero, y el rol de Maestro Alquimista del Caldero (+0.5).
- Autotest `-cvgselftest`: sección "work rooms, furnace fire and pill failure" con una sala de alquimia real y 1600 píldoras en cuatro
  escenarios (habilidad 10: 55 fallos y 28 celestiales de 400; habilidad 3 con el horno frío: 101 fallos; habilidad 3 con horno nivel
  3 y cristal de fuego: 57; habilidad 20: 11 fallos y 86 celestiales). ALL OK en 1.6.4871.

- Core/Stories: la investigación **reinos secretos** ya hace algo. Al terminarla se abre la primera puerta cerca de la colonia
  (ResearchMod_FireIncident nuevo en el Core, dispara un incidente por nombre), y las puertas ya no aparecen solas antes de investigarla.
  Antes costaba 6000 y no desbloqueaba nada.
- Core: el autotest del tope de huéspedes prueba también el camino real del narrador (4 caravanas de facciones distintas: entran 2)
  y lista quién parchea `CanFireNow`. El de ideología del viejo Sects se salta si Sects no está activo. `tools/run-test.ps1` abre la
  prueba en el monitor secundario (ROG), visible y sin robar el foco (`-NoSecondMonitor` vuelve a la ventana minimizada).
  Tanda en 1.7.4985 con los 15 mods: Core, Resource, Bloodline, Trait, Stories, Formations, Grafts, Factions, Qi network, Qi-Tech,
  Crops, Furniture y Flying island ALL OK.
- Herramientas de prueba (sin cambio en el juego): skill nueva `rimworld-mod-tester` (docs/skills/) con el criterio de qué probar,
  y `tools/elegir-pruebas.py`, que elige los autotests según lo que cambió y separa los lentos (títeres, playtest), que piden permiso.
  `run-test.ps1 -Mods todos` y espera tantos resultados como pruebas pedidas. `TestArena.Clear` despeja la zona de los tests (roca,
  agua, techo; opcionalmente pawns ajenos). Arreglos de tests: Hijo del Cielo sin genes de vidas extra (fallaba al azar), atadura
  mirada seguido. 1.7.4985: Core + Stories y Formations ALL OK.
- Furniture: el bonsái espiritual ya no es maceta (vanilla le sembraba lirio de día encima del árbol). Es decoración, con belleza 15 y qi 4.
- Factions: arreglado el aviso sin fin "Rol de ... activado / desactivado" con un solo creyente: el rol de especialista de las sectas
  se desactivaba y se activaba en cada recálculo (los dos mínimos valían 1). Ahora se apaga recién sin creyentes.
- Factions: los favores del Caldero entregan píldoras con calidad. El artefacto de obra maestra, cuando da píldoras de Fundación, las da
  de obra maestra; la "píldora a elección" sale siempre legendaria (celestial). Antes llegaban en calidad Normal. `-cvgfavorstest` ALL OK.

**Change notes (Steam)** (Core, Alchemy, Furniture, Apparel, Factions, Traits & Destinies):
```
- Wuxia work rooms: alchemy room, spirit smithy, scribe hall and weaving room. Cleanliness, impressiveness, qi and linked furniture (pill cabinets, censers, scripture shelves, braziers...) raise the quality of what is made there; a good room makes up for a weak artisan.
- The alchemy furnace needs fire: wood, spirit stones or fire crystals, and a better fire makes better pills.
- Alchemy can fail: about 30% bad or failed (waste pills), 45% normal, 20% excellent, 5% celestial for an average alchemist; masters, good rooms and alchemist traits push the odds up.
- The secret realms research now opens the first realm gate near your colony; gates no longer appear before it (Core + Stories).
```

### Lore: el Clan del Dragón pasa a leyenda y las sectas viejas a las 5 de Factions (2026-10-22)
Sin cambio de versión todavía (va con la próxima subida de cada mod). Solo textos (EN, ES y ES-Latin), salvo Stories.
- **Clan del Dragón Antiguo** (decisión del usuario): ya no existe; solo quedan algunas personas que heredan un poco de su sangre. Unas 20
  descripciones lo nombran en pasado ("el desaparecido Clan del Dragón Antiguo") en Alchemy, Apparel, Core, Formations, Furniture,
  Races, Resources y Traits & Destinies. El xenotipo dragón pasa a "raros herederos" del clan; el pupilo es de "una casa de tormenta".
- **Pico Azur → Espada Sin Forma** y **Templo del Loto Silencioso** repartido: lo corporal (Vajra, Kongming, piel de hierro) a los
  **Nueve Templados**, lo de sanar (enfermería, cama de sanador, injertos de mano) al **Caldero Celestial** y el ambiente de monjes (té,
  guqin, estanques, incienso) a "los templos de montaña de Jiuxiao", sin secta. Unas 105 descripciones en 13 mods. Lin Wuchen sale de
  la backstory del Anciano de la Disciplina (Factions).
- **Stories:** se quitan los 3 eventos del clan (VisitDragon, MissionDragonTribute, DragonHeirTrial), que solo cargaban con el Sects
  viejo. El escenario **Heredero del dragón** pasa a cargar con Factions: lo persigue la Luna Sangrienta (incursión nueva "Los cazadores
  de sangre" a los 10-18 días). Sin `Mods/Sects` en LoadFolders; código sin las facciones viejas como alternativa.
- Qi-Tech y Core: "the Edge" → "the Rim" en 7 textos en inglés, como el resto del proyecto.
- Apparel: se borra `Patches/SectRobes.xml` (túnicas para los discípulos del Sects viejo; las de Factions están en
  `SectRobes_Factions.xml`) y se quita Sects de la descripción y de `loadAfter`. Comentarios internos de Core y Factions sin "Convergence:
  Sects". Factions: el color "natural" de las ideologías de secta tiene nombre en español.
- `docs/LORE.md`: el Clan del Dragón figura como desaparecido. Autotests `-cvgselftest -cvgstoriestest` ALL OK en 1.6.4871.

**Change notes (Steam)** (cada mod que la lleve):
```
- Lore pass: descriptions now name the five sects of Convergence: Factions; the Ancient Dragon Clan is a vanished legend.
```
Stories, además:
```
- The Dragon heir scenario now works with Convergence: Factions: the Blood Moon Cult hunts your dragon blood. The old Dragon Clan events were removed.
```

### Convergence: Core → 0.1.3 (2026-10-22)
- **"Hecho por"** en píldoras, armas y talismanes de todos los módulos: el colono que lo fabricó o, si vino de afuera (comercio, sectas,
  botín, eventos), un creador de lore ("Lin Jin, un alquimista errante", "un viejo herrero"…). Las pilas mezcladas dicen "varios artesanos".
- La ficha de cada píldora muestra lo que hace **esa** píldora con su calidad (qi con la tabla de todas las calidades), sus efectos
  secundarios (toxicidad), el nivel de mesa y la habilidad que pide la receta.
- Autotest `-cvgselftest`: sección "made by and pill effects" (ALL OK en 1.6.4871).

**Change notes (Steam):**
```
- Pills, weapons and talismans show who made them: your colonist, or a lore maker for items from outside ("an old alchemist", "a wandering bladesmith"...). Pills show their real effect for their quality, side effects and what the recipe needs.
```

### Convergence: Stories & Scenarios: eventos mudados a Convergence: Factions (2026-10-22)
- Los eventos de las sectas ahora salen con **Convergence: Factions**: los del Pico Azur pasan a la Espada Sin Forma y los del Templo del
  Loto Silencioso al Pabellón del Caldero Celestial (textos reescritos: enfermería de caridad, Mo Qinghua, Qian Wanli). Los del Clan del
  Dragón se quitaron después (ver "Lore" arriba).
- Escenarios: Discípulos perdidos, Alquimista exiliado y **Sanadores de la enfermería** (antes Refugiados del Loto) pasan a Factions; el
  Heredero del dragón también (ver "Lore" arriba).

**Change notes (Steam):**
```
- Sect events and scenarios now use the new Convergence: Factions (the Azure Peak events move to the Formless Sword, the Silent Lotus ones to the Heavenly Cauldron Pavilion).
```

### Convergence: Apparel: túnicas para Convergence: Factions (2026-10-22)
- Los discípulos de las sectas de Factions (y de los Errantes) visten la túnica de su rango; los comerciantes del Caldero, los Errantes
  y el Hilo Invisible venden seda espiritual y tela de nube.

**Change notes (Steam):**
```
- Sect robes for the disciples of Convergence: Factions.
```
### Convergence: Stories & Scenarios (2026-10-22)
- Las visitas de las historias (VisitAzurePeak, VisitLotus, VisitCauldron, VisitDragon, VisitRogueMarket, LotusPilgrims, SectTradePact y
  los emisarios de la Convergencia) **ya no se juntan con otros huéspedes**: el evento no se elige si hay visitantes o una caravana de otra
  facción en el mapa, y una visita ya decidida se aplaza un día (hasta 6 veces). Antes podían coincidir con las caravanas y visitas de
  vanilla: hasta 3 grupos a la vez.
- Autotest `-cvgstoriestest`: nueva sección "visits wait for guests to leave" (ALL OK en 1.6.4871).
- Versión: va junto con la 0.1.1 (compatibilidad con 1.7), que todavía no se subió.

**Change notes (Steam):**
```
- Story visits no longer pile up with other visitors or trade caravans: they wait until your guests have left.
```

### Los 15 mods → compatibles con RimWorld 1.7 (2026-10-22)
Probado en la beta 1.7.4985 (motor Waylith, Harmony 3.0) con las 5 DLC: autotests de Core, recursos, linajes y rasgos ALL OK,
sin errores rojos. Sigue siendo **un solo DLL net472** que carga en 1.6 y en 1.7: para quien juega 1.6 no cambia nada.
- `About.xml`: `<li>1.7</li>` en `supportedVersions` (los 15). `LoadFolders.xml`: bloque `<v1.7>` igual al de `<v1.6>` (los 15).
- Core y Qi-Tech: las herramientas de capturas del Workshop usaban `Time.captureFramerate`, que no existe en 1.7 y lanzaba un error
  en cada cuadro (llenaba el log). Ahora pasan por `UnityCompat.CaptureFramerate` (reflexión: igual en 1.6, no hace nada en 1.7).
- Core: el autotest de ingeribles ya no intenta comer cadáveres vacíos ni plantas sin madurar, el de "spawnear todo" salta las salidas
  de pocket map y el del muñeco de madera busca la casilla con el mismo peligro que el work giver (solo afectaba a `-cvgselftest`).
- Nota de pruebas en 1.7: el ejecutable es `RimWorld.exe`, `-logFile` se ignora (log en `...\Logs\Player.log`) y arrancar la ventana
  minimizada crashea el motor (swapchain 0×0). `tools/run-test.ps1 -Version 1.6|1.7` ya lo contempla (1.6 en `C:\Games\RimWorld_1.6`).

Versiones: Core 0.1.1 → **0.1.2** (ya subido el 2026-10-22, ver Publicadas), Formations 0.1.1 → **0.1.2**, Qi-Tech **0.2.2** (junto con el arreglo de abajo, aún sin subir),
los otros 12 0.1.0 → **0.1.1**.

**Change notes (Steam)** (Qi-Tech: ver su bloque):
```
<versión>
- Now compatible with RimWorld 1.7 (Waylith engine). Still works on 1.6: same files for both versions.
```

### Convergence: Qi-Tech → 0.2.2
- Compatibilidad con Character Editor: el rifle de inducción (dos variantes, `Graphic_Random`) hacía fallar su ventana de objetos
  (`TextureTool.GetGraphicForRandom`, miles de "GUI Error"). Ahora no se inclina al azar en el suelo, que era lo que envolvía su gráfico.
- Compatible con RimWorld 1.7 (ver arriba).

**Change notes (Steam):**
```
0.2.2
- Fixed: compatibility with Character Editor (the qi induction rifle broke its item window).
- Now compatible with RimWorld 1.7 (Waylith engine). Still works on 1.6: same files for both versions.
```

---

## Publicadas

### Convergence: Factions 0.1.1 — subido 2026-10-22 15:08
- Los miembros de las sectas y de los Errantes ya no nacen con xenotipos vanilla al azar (hussars, genies…): la base de facción
  Outlander traía esa mezcla. Ahora son baseliners (los linajes de Races siguen funcionando aparte).
- **Ideologías propias:** cada secta (y los Errantes) tiene una ideología fija con su nombre, su fundador como dios (Qing Feng, el
  Patriarca de la Montaña de Hierro, el Fénix del Primer Fuego, Xue Jiuyou, Yan Shi), el título de líder de la secta, el **Gran
  Anciano** como segundo rol, su ícono y solo nuestros preceptos más los básicos (funeral, fiesta, nacimiento, discurso, cambio de
  rol). Ya no salen títulos como "primer ministro", dioses al azar, reliquias, juicios ni ejecuciones. También en partidas viejas.
- El **Líder** y el **Gran Anciano** de cada secta usan su ícono propio (arte de las hojas del usuario), y la ideología queda bien desde que la secta se crea (antes recién se corregía al rato).
- Títulos con mayúsculas en los nombres propios ("Amigo de la Espada Sin Forma").
- Los regalos con material (armas y armaduras obra maestra o legendarias) llegan de metal, no de madera.
- La descripción del artefacto obra maestra del Caldero avisa que a veces son tres píldoras de fundación.
- Prueba nueva `-cvgfavorstest`: los cinco favores de las cinco sectas usados en el juego (llega lo prometido, los refuerzos y el
  golpe pelean y no son hostiles), la Luna Sangrienta neutral con el Camino Demoníaco y las ideologías. ALL OK en 1.6.
- Capturas de Workshop: `-cvgfactionstest`/`-cvgfactionsshot` (guion `FactionsShot.cs`, inerte sin el argumento); capturas en
  `art/capturas/workshop_factions/`.

**Change notes (Steam):**
```
- Sect members are no longer born as random vanilla xenotypes (hussars, genies...): they are human cultivators.
- Each sect now has its own fixed ideoligion: its name, founder, leader title, a Grand Elder role and only sect precepts (no more random "prime ministers", relics or executions).
- Masterwork and legendary gifts arrive made of metal, never wood. Proper names capitalized in sect titles.
- New icons for the leader and the Grand Elder of each sect.
```

### Convergence: Core 0.1.2 — subido 2026-10-22 13:57 (con la compatibilidad con 1.7)
  botín, eventos), un creador de lore ("Lin Jin, un alquimista errante", "un viejo herrero"…). Las pilas mezcladas dicen "varios artesanos".
- Nuevo ajuste **"Grupos de huéspedes a la vez"** (2 por defecto; 0 = sin tope, como vanilla): con el mapa lleno de visitantes o caravanas
  de comercio de otras facciones no llegan más caravanas, visitantes, viajeros ni recaudadores; el narrador lo reintenta más tarde. Los
  forzados (misiones) pasan igual y los ataques no se tocan. Con muchas facciones amistosas (sectas) llegaban 3-4 grupos juntos, con
  decenas de personas y animales, y el juego se ponía lento.
- Autotest `-cvgselftest`: nueva sección "guest group limit" (ALL OK en 1.6.4871).
- Base para el nuevo Sects (sin contenido visible todavía): etiqueta de arma `CVG_Sword` en todas las espadas de vanilla y de Convergence;
  dos stats nuevos, **desvío de proyectiles** (solo con espada en la mano) y **resistencia a proyectiles** (tope 50%, ocultos mientras
  valen 0); constituciones con grados, requisito de rol por rasgo, rasgo inicial por meme, ritual de templado genérico, cuerpos y rasgos
  de los discípulos por XML y stock con calidad mínima. Autotest: sección "sect framework (S1)" (ALL OK en 1.6.4871).
- **Cuerpo de Espada**: ahora es una constitución de 4 grados (hierro, acero frío, hierro estelar, jade celestial) con y sin raíz
  espiritual; al nacer solo sale el grado de hierro (el antiguo "cuerpo de espada"). El antiguo "cuerpo de espada celestial" ya no nace y,
  en partidas guardadas, se convierte al grado de jade celestial. Los grados suben con el ritual de la Secta de la Espada Sin Forma.
- Rituales y roles de secta: mínimo de creyentes para empezar un ritual y ánimo por rol vacío solo desde N creyentes (solo en los roles
  que lo piden).
- Constituciones de 4 grados para las otras sectas (con y sin raíz espiritual; al nacer solo el grado 0; suben con el ritual de su
  secta): **cuerpo de acero** (piel de bronce, huesos de hierro, médula de oro, cuerpo adamantino: resistencia a proyectiles; el antiguo
  cuerpo de acero es el grado 0), **cuerpo yang puro** (yang puro, llama del horno, fuego de nueve vueltas, fuego del fénix: alquimia,
  cocina, calor), nuevo **cuerpo de sangre lunar** (sangre turbia, carmesí, lunar, del demonio interno: curación, daño cuerpo a cuerpo y
  dolor, a cambio de crisis mentales más fáciles) y, con Biotech, nuevo **cuerpo de mil hilos** (hilo de seda, de acero, de jade, corazón
  de mil hilos: ancho de banda de mecanoides +1 a +4 y un grupo de control más).
- Stock de especialidad por lista de objetos con calidad controlada (`StockGenerator_QualityDefs`).
- Autotest `-cvgselftest` ALL OK en 1.6.4871 después de todo lo anterior.

**Change notes (Steam):**
```
- The sword, steel and pure yang bodies are now four-grade constitutions, and two new special bodies appear: the blood moon body and, with Biotech, the thousand threads body. Colonists are born at the first grade; the higher grades come from the rituals of Convergence: Factions. Old celestial sword bodies become celestial jade.
```

**Change notes (Steam):**
```
- New setting "Visitor groups at once" (default 2): no more caravans and visitors piling up on your map at the same time. Set 0 for vanilla behaviour.
```


### Convergence: Factions 0.1.0 — subido 2026-10-22 (id 3824674368)
- Mod nuevo (`prospero1088.convergence.factions`; en desarrollo se llamó "Sects 2.0"). Reemplaza al Sects actual, que se retira más adelante, y es incompatible con él. Requiere Royalty e Ideology. Las cinco sectas siguen la misma plantilla
  (`docs/modulos/SECTS2_PLANTILLA.md`): 4 títulos honorarios permanentes, 5 favores, constitución de 4 grados con su ritual, especialista,
  meme con arma noble y despreciada, discípulos con historias de fondo propias por rango, armas que pueden usar y nombres propios.
  - **Espada Sin Forma** (espadas), **Nueve Templados** (cultivo corporal, sin armas), **Pabellón del Caldero Celestial** (alquimia y
    comercio; píldora a elección).
  - **Culto de la Luna Sangrienta** (camino demoníaco): enemigo natural, neutral con colonias del Camino Demoníaco, hostil con la Espada
    y los Nueve Templados; no comercia; hojas; ritual Juramento de Sangre.
  - **Secta del Hilo Invisible** (marionetas; solo con Biotech Wuxia): sus incursiones y asentamientos traen marionetas; vende materiales
    de marioneta y atuendo de titiritero; ritual Vinculación de Hilos Espirituales.
- Arte provisional (íconos del Sects viejo) hasta que llegue el definitivo.
- Autotest `-cvgfactionstest` ALL OK en 1.6.4871, con todos los módulos y también sin Biotech, Alchemy ni Biotech Wuxia.

- También trae a los **Cultivadores Errantes** del Sects viejo, como facción sin títulos ni favores (posadas, mercados nocturnos, mercenarios).
- Imagen de vista previa provisoria con los cinco emblemas.


### Convergence: Qi-Tech 0.2.1 — subida 2026-10-21
- El exoesqueleto ya no se tiñe de gris: heredaba el `colorGenerator` de la armadura de reconocimiento vanilla (gris 0,33-0,9).
  Ahora se ve con los colores del arte (`ColorGenerator_White`). Los exoesqueletos ya fabricados conservan su color guardado.

**Change notes (Steam):**
```
0.2.1
- Fixed: the Qi-Tech exoskeleton no longer comes out tinted grey; it shows its real colours.
```

### Convergence: Core 0.1.1 — subida 2026-10-21
- Nuevo material **cristal primordial** (`CVG_PrimordialCrystal`): 10 piedras espirituales en la mesa de cantero. Material común para
  los módulos (primer uso: el resonador espiritual de Qi-Tech). Arte del usuario.

**Change notes (Steam):**
```
0.1.1
- New material: primordial crystal, made from 10 spirit stones at the stonecutter's table. Shared by the modules (first used by Qi-Tech's spirit resonator).
```

### Convergence: Formations 0.1.1 — subida 2026-10-21
- Torretas: los cabezales de la estatua guardiana, la ballesta rúnica y la ballesta del trueno estaban girados 90° respecto del disparo;
  ahora apuntan al blanco (como el arte de Ludeon, al este).
- La estatua guardiana lanza **espadas de qi** (nuevo proyectil `CVG_Projectile_GuardianQiSword`, sonido de energía) en vez de agujas.

**Change notes (Steam):**
```
0.1.1
- Fixed: the guardian statue, rune crossbow and thunder ballista now face the enemy they shoot at (their heads were turned 90 degrees).
- The guardian statue now throws qi swords instead of golden needles, as its lore says.
```

### Convergence: Qi-Tech 0.2.0 — subida 2026-10-21
- **Ingeniería Espiritual:** nueva pestaña de investigación con todas las investigaciones de Qi-Tech; investigación científica en el banco
  avanzado, sin cultivadores ni Dao. Todo bloqueado hasta la Convergencia (bloqueo real). Tres investigaciones nuevas: Fundamentos del Qi,
  Resonancia Espiritual, Instrumentación Espiritual Avanzada.
- **Resonador espiritual** (2×2, 250 W): instalación del banco avanzado, necesaria para la Instrumentación Espiritual Avanzada. Arte del usuario.
- `CVG_Research_QiTech` (mismo defName) ahora requiere Microelectrónica y Fundamentos del Qi en vez de la red de qi de Formations.
- **Armas:** rifle de inducción de qi (grado 1, dos variantes), rifle de qi concentrado (grado 2), cañón de compresión de qi (grado 3).
- **Torretas:** torreta de inducción de qi (grado 2) y cañón de contención de qi (grado 3), con energía y piedras espirituales por disparo;
  el disparo sale de la punta del cañón.
- Arte del usuario para las 3 armas, las 2 torretas, los 3 proyectiles de qi, el resonador, el convertidor de qi y el exoesqueleto (puesto, 5 cuerpos × 3 vistas, e ícono). Lore y traducción (Spanish, SpanishLatin).
- Partidas guardadas: se conserva lo ya investigado; las investigaciones de Qi-Tech pasan de la pestaña Cultivo a Ingeniería Espiritual.

**Change notes (Steam):**
```
0.2.0
- New research tab "Spiritual Engineering": the scientific road to qi, researched at the hi-tech bench with no cultivators needed. Locked until the Convergence. All Qi-Tech research moved here (saved progress is kept).
- New research: Qi Fundamentals, Spirit Resonance, Advanced Spirit Instrumentation.
- New building: spirit resonator, a research facility for the hi-tech bench (needs power and primordial crystals).
- New weapons: qi induction rifle (grade 1), concentrated qi rifle (grade 2), qi compression cannon (grade 3). Any colonist can make and use them.
- New turrets: qi induction turret (grade 2) and qi containment cannon (grade 3). They need power and burn spirit stones per shot.
- Qi-Tech no longer needs the Formations qi network research to start.
- New art for the converter, the exoskeleton, the weapons, the turrets and the qi bolts.
```
### 0.1.0 — los 15 mods (primera publicación, oculta)
