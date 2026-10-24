# Registro de cambios (notas de versión para Steam Workshop)

Cada cambio a un mod publicado se anota acá, por mod, en **"Sin publicar"**. Al subir: se sube `modVersion` en el `About.xml`
del mod, se copia el texto en inglés a las *change notes* de Steam y la sección pasa a **"Publicadas"** con la fecha.
Pedido del usuario (2026-10-21): versionar todo lo que se modifica en los mods y addons para subirlo después.

Estado de partida: los 15 mods se publicaron (ocultos) en la **0.1.0** (tag git `v0.1.0`; ids en `About/PublishedFileId.txt`).

---

## Sin publicar

Próxima subida (juntar todo en una sola vez; ver la nota de abajo para cada mod).

### Raíz espiritual que cambia (2026-10-24)
- **Core: la raíz espiritual se actualiza al momento.** Si un colono gana o pierde un rasgo o gen de raíz (xenogerm, ensamblador
  de genes, sangre ancestral, modo desarrollador), la pestaña Cultivo y la velocidad de cultivo cambian enseguida; antes seguía
  la raíz vieja hasta recargar la partida.
- **Core: rasgo y gen de raíz valen lo mismo:** gana el grado más alto. Antes, con Biotech, el gen de raíz común tapaba a un rasgo
  de raíz superior.
- EN: *Spirit roots now update immediately when a pawn gains or loses a root trait or gene (xenogerm, gene assembler, ancestral
  blood, dev mode). Root traits and genes now stack to the highest grade instead of the gene always winning.*

### Raíces que cambian (2026-10-24, docs/modulos/RAICES.md)
- **Core: raíces selladas** (inferior, común, superior y celestial, con arte nuevo): guardan elementos y de quién eran.
- **Core: extirpar e implantar raíces** (cirugía; necesita el altar del Dao o la mesa de cirugía de qi). El donante queda sin
  raíz, con "raíz arrancada", un mal recuerdo y odio al cirujano; si falla en Fundación o más, puede morir. Ideology lo juzga como
  la extracción de órganos. El receptor pasa 30 días con "raíz ajena" y puede rechazarla.
- **Core: el avance de reino puede aclarar la raíz** (más chance cuanto peor es; superior → celestial solo con tribulación), y
  llegar a Limpieza de Médula puede despertar una raíz en quien no tenía.
- **Core: órganos de cultivador:** el corazón, los pulmones, los riñones y el hígado de un cultivador salen "templados" (valen
  más y dan bonos chicos al implantarlos).
- **Core: ritual de purificación de raíz** (precepto de Ideology): sube la raíz o despierta una; una vez por año.
- **Grafts:** investigación "Cirugía de raíces" y fusión de 3 raíces en 1 del grado siguiente en el horno de huesos (purifica).
- **Qi-Tech:** investigación "Ingeniería de raíces", mesa de cirugía de qi (con arte nuevo), síntesis de raíces en el
  laboratorio de píldoras de qi (sin purificar) y raíz artificial (con arte nuevo).
- **Alchemy:** Píldora de Lavado de Raíz (con arte nuevo): sube la raíz hasta superior, con tolerancia.
- **Stories:** raíces selladas como recompensa en 6 eventos. **Factions:** el Pabellón del Caldero Celestial compra y vende raíces.
- EN: *Spirit roots can now change. Sealed roots (four grades); extract and implant roots on the Dao grafting altar or the new qi
  surgery table; fuse three roots into a better one; breakthroughs may clear a root; Marrow Cleansing may awaken one; tempered
  cultivator organs; a root purification ritual; the root-washing pill; an artificial root from Qi-Tech; sealed roots as event
  rewards and Heavenly Cauldron trade goods.*

### Partidas largas de prueba (2026-10-24, madrugada)
Once partidas de 30 a 60 días con una base autosuficiente y el narrador libre. Lo que salió y se corrigió:
- **Core: bestias espirituales más justas.** Con la supresión de reino, la vida y el daño extra por rango eran demasiado (una
  manada de wargos de rango 4 mataba a la colonia el día 20). Ahora los multiplicadores son suaves (x1,1 a x2,5), la supresión
  empieza en rango 2 y el mundo despierta de a poco: la chance va del 30% al 100% en 60 días desde la Convergencia y el rango
  tope empieza en 2 y sube uno cada 15 días.
- **Core: el Maestro ya no cuenta como riqueza de la colonia** (un Núcleo Dorado de paso inflaba todas las amenazas: incursiones
  de 130 puntos el día 11).
- **Core: el Maestro herido sobrevive a su llegada.** Llegaba con golpes "hasta caer", y como un Núcleo Dorado casi no siente
  dolor, los golpes seguían hasta matarlo. Ahora llega con golpes en los miembros y desmayado por agotamiento de qi unos días.
- **Core: el jade del Maestro** no recibe daño ni se puede robar (daba cientos de errores).
- **Biotech Wuxia: noche de los jiangshi** con mínimo 1 (con pocos puntos mandaba el doble).
- **Qi-Tech: los mecas de qi se ven distintos:** acento verde jade en lugar del color de su facción y un aura suave.
- **Apparel:** gorros del mod aptos para chicos. **Factions:** los cascos de titiritero se pueden comprar a su secta.

### Fusión, Anomaly y Odyssey (2026-10-24, plan de fusión)
- **Qi-Tech: forja de qi** (versión de qi de la mesa de maquinado, sale temprano tras la Convergencia con "forja de qi"). Templa
  cualquier arma, de qi o mortal: +1 grado espiritual y +1 nivel de calidad por templado, costo creciente por grado (piedras, jade,
  cristal; o bioferrita), sin techo.
- **Qi-Tech: torretas de qi para todas las torretas.** Cada torreta del juego que dispara daño (vanilla, nuestras, Vanilla
  Expanded Security, cualquier mod) tiene su versión qi I, II y III en un desplegable del menú de construcción: cuesta lo mismo
  que la normal más el cañón de qi de su nivel (se hace en la forja; el I es barato y temprano). Pega entero hasta el reino de su
  grado.
- **Qi-Tech:** escopeta y pistola de qi (grado 1); chaleco, pantalón y casco de qi (más armadura y resistencia al caos; el casco
  frena lo psíquico como el casco de lámina psíquica); biónicos de qi (brazo, pierna, ojo: mejores que los vanilla, más
  disonancia); laboratorio de píldoras de qi (todas las píldoras, tope excelente salvo con un alquimista; refina jade espiritual);
  bandeja hidropónica espiritual (hierbas espirituales 50% más rápido); acumulador de qi (batería de la red de qi); proyectil de
  mortero de qi (grado 3).
- **Qi-Tech: mecas de qi.** Tres núcleos de qi (básico, medio, avanzado: uno por peso) que el mecanitor implanta en su meca; la
  estación de afinación junto al recargador sube su grado hasta 3. Tras la Convergencia los mecas enemigos también traen núcleo.
- **Core: bestias espirituales** ahora resisten las armas mortales con la misma supresión de reino que los cultivadores (rango
  1-2 = reino 1, 3-4 = reino 2, 5-6 = reino 3); las armas, torretas y proyectiles de qi les pegan entero.
- **Core: jade.** Más vetas de jade vanilla (0,15) y espiritual (0,12); piedra y jade espirituales salen con el taladro profundo y
  el escáner (solo tras la Convergencia), vienen en la estrella de qi, los venden las sectas y se muelen en la mesa de cantería
  (3 trozos de piedra + 3 piedras espirituales = 10 de jade espiritual).
- **Core: Anomaly = caos primordial.** Al despertar el monolito llega el sabio del vacío (signo de pregunta, se le habla con clic
  derecho): explica el caos, se queda hasta la primera amenaza y deja núcleos de caos y un talismán. Las entidades dejan núcleos de
  caos, piedras y sangre según su rango; estudiarlas da comprensión del Dao (con riesgo de demonio interno). Tres talismanes del
  caos en el moldeador de bioferrita (ruina, pavor, remiendo). El qi les pega +25%; los reinos altos resisten lo psíquico; nueva
  resistencia al caos. Alchemy: píldora de sangre del caos (Luna de Sangre).
- **Odyssey:** koi de seda (pez espiritual raro: qi y chance de subir una etapa); en órbita hay poco qi; reinos olvidados
  (fragmentos de Jiuxiao) que encuentra el escáner orbital, con qi altísimo, guardianes mecánicos de qi y botín del mod de alta
  calidad; los lugares notables pueden traer vetas espirituales, escondites de secta o cultivadores caídos.

### Bestias, comida, eventos y correcciones (2026-10-23, segunda tanda)
- **Core: bestias espirituales.** Despierta cerca de la mitad del mundo (antes 1-15% de los animales comunes), con la Convergencia
  y al aparecer después. Rango 1 a 6 según la especie, alineado con los reinos (gallina 1, oso 4, trumbo 5; las grandes bestias de
  600+ de poder, 6), con un 15% de élites un rango más arriba: pelean y aguantan como un cultivador de ese reino. Funciona con
  cualquier animal de cualquier mod (Alpha Animals, Vanilla Animals Expanded, ReGrowth). Las especies de 600+ despiertan como
  **bestias soberanas**: no sienten dolor, se curan rápido, su aura frena a los enemigos de reino más bajo y una carta avisa cuando
  llegan a la colonia. Al morir dejan piedras espirituales según su rango; al carnearlas, carne, sangre, cuero y núcleos. Se ven en
  el nombre ("alce espiritual", "gran trumbo").
- **Core: comida.** La carne y la sangre espirituales cuelgan de Comida (se guardan y se pudren como comida, sin que las comidas
  comunes las gasten); núcleos, plumas, colmillos y caparazones van a "partes de bestia espiritual".
- **Fauna & Flora + Alchemy + Apparel:** la pluma de grulla va en el talismán veloz, el colmillo de lobo en el amuleto guardacorazón
  y el quitina del Nueve Infiernos en la túnica de batalla rúnica (antes solo se vendían).
- **Core: dos eventos que arman el mundo.** Lluvia espiritual (llueve de un cielo despejado, las plantas crecen 60% más rápido, el
  qi vuelve antes, ánimo +5) y estrella de qi (el meteorito vanilla, pero cae una veta de piedra espiritual).
- **Stories:** vuelven 14 eventos: siete baratos que arman el mundo (buen augurio, hierbas que florecen, campos que brillan, puente
  de siete colores, día de los fundadores, luces extrañas, animales inquietos) y siete con algo propio (el mendigo que no era
  mendigo, el manual robado, la alianza matrimonial, el maestro del té, la estela rota, la jabalina de las colinas rojas, el peaje
  del río), más la cacería de la araña.
- **Correcciones:** vuelve la torreta de contención de Qi-Tech (era la torreta pesada de largo alcance de la colonia tecnológica) y
  "armas templadas en qi" ya no pide respiración del qi (la colonia sin cultivadores la puede investigar).
- **Formations + Qi-Tech: más carga de piedras.** Ballesta rúnica 50 (antes 10), balista del trueno 80 (20), torreta de inducción 80
  (12), torreta de contención 100 (12), formaciones de espadas y de atadura 60 (15), gran formación protectora 100 (40): una
  incursión grande ya no obliga a recargar a cada rato.
- **Core: más vetas de piedra espiritual.** La Convergencia abre 10-14 vetas de 5-10 celdas por mapa (antes 3-5 de 3-7) y en los
  mapas nuevos aparecen entre la plata y el acero (antes como el oro).
- **Formations: torretas competitivas contra cultivadores.** Ballesta rúnica grado 2 y balista del trueno grado 3 (antes 0: pegaban
  como un arma común). Toda arma y torreta con grado dice en su descripción hasta qué reino pega con toda su fuerza y cuánto pierde
  más arriba. Probado en combate real: la balista le pega con todo a un cultivador del Núcleo Dorado.

### Plan de integración: el wuxia dentro de RimWorld como generador de historias (2026-10-23)
Plan cerrado con el usuario (docs/revision/PLAN_INTEGRACION.md). Nada de sistemas nuevos: reconectar lo que había con los canales
vanilla y recortar lo que sobraba. **El Core ahora exige las cinco DLC** (el mod es para el usuario).
- **Core: cultivar es meditar.** Se borró el trabajo "Cultivar": los cultivadores avanzan meditando (franja Meditar del horario,
  recreación, psyfoco de Royalty). El punto de meditación vanilla es el lugar de cultivo gratis; cojín, plataforma y estrado lo
  mejoran. Se fueron el punto de cultivo propio y la plataforma de jade (Furniture), la tirada propia de iluminación (queda la
  inspiración vanilla, que ahora da la comprensión del Dao), la desviación de qi por hora de la luna (quedan avance fallido y demonio
  interno), las alertas de cultivador inactivo y sin lugar, y el diálogo de inicio. La pestaña Cultivo solo sale en cultivadores y ya
  no repite linaje, Maestro ni riqueza.
- **Core: el Maestro es la misión de inicio.** "Un viejo sabio busca refugio" llega a la pestaña Misiones a los 2-4 días y puede
  quedar pendiente; aceptarla desata la Convergencia y el Maestro cae herido. Si nunca se acepta, la Convergencia llega sola a los
  60 días (ajustable). Quienes lo persiguen pueden venir a buscarlo, la colonia sufre cuando lo hieren y al final se va caminando.
  Sin modos de inicio ni investigación de la anomalía espiritual.
- **Core + Traits & Destinies: relaciones que nacen jugando.** Interacciones sociales vanilla: hablar del Dao, jurar hermandad,
  desafío entre rivales, voto del Dao y tomar un discípulo (maestro y discípulo; los discípulos del mismo maestro son condiscípulos).
  Una pelea entre cultivadores puede dejar rivales de por vida; rescatar a alguien caído, una deuda de vida; matar a un ser querido
  de un colono, una enemistad de sangre. Estados nuevos: dejé atrás a mi rival, falló el avance de alguien cercano, arruiné una
  píldora, hirieron a nuestro maestro, muertes de maestro, discípulo, condiscípulo y benefactor, enemigo de sangre muerto. La envidia
  ya no le toca a cualquiera que le caiga mal al que avanza, solo a su rival. Sin botón de voto.
- **Core: la tribulación se vive como un parto.** La colonia sale a mirar desde una distancia segura; cada testigo le resta un 7% al
  daño del rayo (hasta 40%) y el ánimo es solo para quien la vio.
- **Core: contramedida contra cultivadores.** Investigación "armas templadas en qi": toda arma y torreta, vanilla incluida, llega a
  grado espiritual 1. El material espiritual (jade espiritual en el Core; acero espiritual y demás con Resources) suma su grado encima.
- **Stories: 29 eventos en vez de 80**, colgados del narrador vanilla (dos incidentes: relato y amenaza de Jiuxiao). Las cadenas que
  chocaban con una amenaza ya no se pierden (se reintentan un día después). Cada evento tiene un protagonista fijo, los memorables
  dejan relatos para el arte y los de sectas dan favor (Royalty). Una secta que ataca manda la carta de ruptura. Quien se une llega
  ya con la ideología de la colonia. Sin simulación de guerras de sectas (el pedido de ayuda y el torneo quedan como eventos) ni puerta
  de reino al minar. Resources: el hierro meteórico y la arena estelar caen en los meteoritos vanilla.
- **Factions: las sectas comercian solo con piedras espirituales.** Precios en plata como siempre; el total se paga en piedras según
  su valor. Sus mercaderes traen piedras en vez de plata y los tributos de los eventos se pagan en piedras.
- **Grafts:** los injertos entran a los canales de biónicos: saqueadores de secta pueden traerlos, los mercaderes de secta los
  venden y salen como recompensa. Quien espera un injerto se acuesta en el altar.
- **Recortes:** Furniture pierde la cocina, carnicería, cantería y cervecería (copias exactas de las vanilla) y la mesa de jade (sus
  esculturas pasan a la mesa de escultura vanilla); el Core, el escritorio de contemplación. Formations pierde la estatua guardiana
  (repetía la formación de espadas) y Qi-Tech la torreta de contención (repetía a la balista). Alchemy: se van la espada, el sable,
  la lanza y el arco espirituales (un arma wuxia de material espiritual hace lo mismo); queda la espada voladora. Apparel: los cinco
  pendientes elementales pasan a ser los aros de los cinco elementos, que se sintonizan con la raíz de quien los lleva.
- **Pestañas de construcción propias:** Cultivo (asientos y todas las mesas), Mobiliario wuxia, Formaciones y Qi-Tech.
- **Interno:** los autotests y capturas del Core pasan a un mod de desarrollo (Convergence: DevTools, solo lo carga el lanzador de
  pruebas). Caché en los chequeos que corren todo el tiempo (Convergencia activa, caminos de cultivo, energía de la red de qi,
  sombras, color, escudo de formación, incidentes).

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
- Textos (EN, ES y ES-Latin): 51 descripciones cortas pasan a tener lore de 2-3 frases (técnicas pasivas, buffs, condiciones de
  alquimia, linajes y recursos, materiales de marioneta, emisores de torretas de Qi-Tech); los 6 trozos de roca de Resources tienen
  descripción; los 21 objetos que suben el qi lo dicen (muebles, puertas, bloques, jade, plantas). Biotech Wuxia: el tambor de sangre
  ocupa 1×1 (heredaba el 1×2 de la pileta y su dibujo redondo no seguía la huella). `tools/audit_funcional.py` sin dos falsos
  positivos (qi 0 de feng shui, clases usadas desde código). Carga en 1.7 ALL OK; los 3 errores de traducción son de Biotech vanilla.
- Core: el Maestro que llega herido ya no llega desnudo (las heridas de llegada le rompían la túnica y el pantalón: ahora se saca
  la ropa antes y se la vuelve a poner). Autotest nuevo: llega derribado y vestido. `-cvgselftest` ALL OK en 1.7.
- Core: el diálogo de inicio ("La Era del Cultivo") ya no pregunta cómo empieza la Convergencia cuando el escenario la trae
  ocurrida (la parte del escenario corre antes que el diálogo y elegir no cambiaba nada); muestra que ya ocurrió y deja solo la
  dificultad. Lo encontró el usuario con un escenario propio.
- Core (rituales de templado de las sectas de Factions): después de cada templado, salga bien o mal, el discípulo queda 15 días en
  "reposo tras el templado" (estado visible en su salud, con el tiempo que falta) y el ritual no lo deja elegir hasta que pase.
  Antes los cuatro grados del cuerpo de espada se podían hacer el mismo día. Textos EN/ES/ES-Latin. `-cvgselftest` ALL OK en 1.7.
- Traits & Destinies: rebalanceo de los rasgos que suben con el uso (estaban "regalados": un maestro de marionetas con las 3 del
  escenario llegaba a gran maestro en 40 días). Umbrales nuevos: marionetas 150/720 días-marioneta (antes 30/120), granjero
  espiritual 150/600 cosechas (30/120), alquimia, forja y talismanes 50/200 (25/100), domador 10/40 (6/20), cuerpo de hierro 200/800
  horas (120/480), comprensión 4/10 (2/5). Las mesas y plantas vanilla cuentan la mitad. Formaciones: ahora suman los núcleos de
  formación (3 cada uno); los muebles y paredes con feng shui solo 0.1 (antes cada tramo de pared de bambú contaba como una formación).
  1.7: Core y Formations ALL OK. El cuerpo de espada y las demás constituciones de secta suben por el ritual de templado (con costo y
  probabilidad), no por uso.
- Biotech Wuxia: se borra la ballesta de repetición de marioneta (y su virote): ningún títere la usaba desde que el guardián de
  bronce pelea a puños. La ballesta de repetición de Alchemy (la del jugador) sigue. Stories: las 4 llaves del final valen 1500 /
  2500 / 3500 / 5000 (antes 0) y siguen sin poder venderse. 1.7: Core + Stories ALL OK.
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
