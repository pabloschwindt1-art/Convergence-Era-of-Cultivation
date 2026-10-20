# Espacio y naves voladoras (Odyssey)

Principio n.º 1: se usa el sistema de gravnaves y vacío de Odyssey; nada paralelo. Todo en `Mods/Odyssey` (LoadFolders `IfModActive`).

## Hecho
- **Resistencia al vacío por reino** (`Mods/Convergence.Core/Mods/Odyssey/Patches/Vacuum.xml`): un cultivador respira qi, no aire.
  Reino 1 (Refinamiento de Qi / Templado de Piel) 4-10 %; reino 2 (Fundación / Refinamiento de Sangre) 15-30 %; reino 3 (Núcleo Dorado /
  Cuerpo Dorado) 35-50 %, con tope de 50 % en Núcleo Dorado (decisión del usuario, 2026-10-19). Se suma a la ropa de vanilla (trajes de
  vacío). La inmunidad total queda para los reinos superiores (Alma Naciente).

## Ideas aprobadas para hacer (criterio: pocas piezas, cada una con identidad)
| Vanilla (Odyssey) | Wuxia | Nota |
|---|---|---|
| Gravnave | **Arca Voladora** (云舟) | La misma gravnave; el "tecno" usa la vanilla, el wuxia la suya |
| Gravnúcleo / motor grav | **Núcleo de Formación de Vuelo** | Formación grabada en jade; se carga con qi |
| Propulsores (quimiocombustible) | **Propulsores de qi** | Queman piedras espirituales en vez de quimiocombustible |
| Generador de escudo de nave | **Formación protectora del arca** | Enlaza con Convergence: Formations |

### Secta voladora (pedido del usuario: "creás tu secta móvil voladora", isla flotante)
La base del jugador puede ser una **isla flotante** que viaja con el sistema de gravnaves. Cómo lo arma Odyssey (estudiado en
`Data/Odyssey/Defs/ThingDefs_Buildings/Buildings_Gravship.xml`): el motor grav tiene `SubstructureSupport` 500 (cuánta subestructura
aguanta), `CompProperties_SubstructureFootprint` (radio 18,9) y `CompProperties_AffectedByFacilities` (propulsores y tanques de
quimiocombustible se enlazan como instalaciones y dan `GravshipRange`).
| Pieza Odyssey | Pieza nuestra | Diferencia |
|---|---|---|
| Motor grav | **Corazón de la Isla** (formación de vuelo en un núcleo de bestia/jade) | Más `SubstructureSupport` (1000-2000, sube con mejoras de formación): islas grandes como pidió el usuario |
| Subestructura | **Roca de nube** (suelo de isla flotante) | Misma función, aspecto de roca y tierra con hierba |
| Propulsores | **Propulsores de qi** / **Velas de viento** | Sin quimiocombustible |
| Tanque de quimiocombustible | **Vasija de esencia** | Combustible: piedras espirituales molidas |
| Consola de piloto | **Altar del timonel** | Lo maneja un cultivador; su reino suma alcance |
**Progresión por núcleos (decisión del usuario):** la isla arranca algo más grande que vanilla (~800 de `SubstructureSupport`),
y para seguir creciendo hay que **juntar más núcleos de gravedad**: cada núcleo engarzado en el Corazón (instalación enlazada,
como propulsores y tanques) suma superficie. Fuentes: gravnúcleos vanilla de las misiones de Odyssey, núcleos de bestias celestiales,
recompensas de reinos secretos y fabricación cara en el endgame. Con un tope de núcleos (se ajusta jugando).

Primero investigar con ilspycmd qué partes del código buscan el motor grav por `ThingDefOf.GravEngine` o por clase
(`Building_GravEngine`) para saber si basta XML (misma clase) o hace falta un parche mínimo. Requiere Odyssey.

- **Qi celestial en órbita:** en mapas de espacio el qi es más puro (bonus a la velocidad de cultivo; vía `CVG_QiConcentration`).
- **Fragmento flotante de Jiuxiao** (ESCENARIOS.md) como sitio orbital: flora y fauna celestial.

## Implementación: la secta voladora (mod `Convergence: Flying Sect`, 2026-10-15)
**Conclusión de la investigación (ilspy):** la gravnave de Odyssey es casi toda por clase y componente (`Building_GravEngine`, `CompGravshipFacility`, `CompGravshipThruster`, `CompSubstructureFootprint`, `CompAffectedByFacilities`, terreno con la etiqueta `Substructure`), así que **basta XML** para casi todo. Solo unos pocos sitios buscan el motor por `ThingDefOf.GravEngine` o la consola por `ThingDefOf.PilotConsole`; se cubren con **dos parches mínimos en el Core** (`Source/Convergence.Core/Space/IslandPatches.cs`):
- `ListerThings.ThingsOfDef`: pedir `GravEngine` devuelve también los defs con `DefModExtension_GravEngineAlias` (cubre la búsqueda del motor del jugador, el embarque, el rango de colocación de instalaciones y el dibujo de la subestructura).
- `Gravship.AddThing`: reconoce como consola del gravbarco cualquier edificio con `CompPilotConsole`.
- `StatPart_HelmsmanRealm` (sobre `GravshipRange`): +3 de alcance por nivel de reino del mejor cultivador de la colonia (solo en el Corazón de la Isla).
- La combustión usa `CompRefuelable` con cualquier filtro, así que la vasija quema piedras espirituales; solo el texto del coste de la consola dice "quimiocombustible" (cosmético).

**Piezas** (todas en `Mods/Convergence.Space/Common/Defs/Island.xml`, requieren Odyssey; texturas provisionales vanilla teñidas, ruta final en el comentario de cada def):
| Pieza | Equivale a | Datos |
|---|---|---|
| Corazón de la Isla `CVG_IslandHeart` | motor grav | se construye (3 núcleos de bestia, 40 jade, 300 piedras espirituales); `SubstructureSupport` 800; radio 22,9 |
| Roca de nube `CVG_CloudRock` | subestructura | terreno-cimiento con etiqueta `Substructure`; 2 madera espiritual + 1 piedra espiritual por casilla |
| Núcleo de vuelo `CVG_FlightCore` | gravnúcleo | objeto; dos recetas: con un gravnúcleo vanilla (+2 núcleos de bestia, 20 jade) o solo con 6 núcleos de bestia, 80 jade y 200 piedras espirituales |
| Montura de núcleo de gravedad `CVG_GravityCoreMount` | extensor de campo | consume 1 núcleo de vuelo; +250 de soporte; **máximo 6** (el Corazón llega a 2300) |
| Vasija de esencia `CVG_EssenceVessel` | tanque de quimiocombustible | combustible: piedras espirituales (250); no explota |
| Propulsor de qi `CVG_QiThruster` / Vela de viento `CVG_WindSail` | propulsores | +10 / +5 de alcance; máx. 4 / 6; exigen zona libre detrás |
| Altar del timonel `CVG_HelmsmanAltar` | consola de piloto | `CompPilotConsole`; el reino del mejor cultivador suma alcance |
Investigación `CVG_Research_FlightArray` (tras la formación de reunión de qi).
**Autotest** `-cvgislandtest` (con Odyssey): el juego encuentra el Corazón como motor, la roca de nube es subestructura válida, soporte 800 → 2300 con tope de 6 monturas, combustible, alcance, consola enlazada, "listo para despegar" y la captura del gravbarco reconoce el altar.
**Pendiente:** recompensas de núcleos de vuelo en reinos secretos y misiones de Odyssey; fragmento flotante de Jiuxiao; qi celestial en órbita; formación protectora del arca; arte propio.

## Revisión de la isla jugando (2026-10-16): por qué no despegaba y qué se cambió
El usuario jugó y la isla "no despega ni con modo desarrollador". Se reprodujo con una **prueba de vuelo real** (`-cvgflighttest`, `Source/Convergence.Core/Space/IslandFlightTest.cs`: isla mínima, colono en la consola, ritual de lanzamiento, destino válido, combustible, generación del gravbarco, viaje por el mundo y aterrizaje; con `-cvgflightvanilla` hace lo mismo con una gravnave vanilla para comparar; `-cvgflightrender` usa también la cinemática con captura de pantalla, que en la ventana diminuta de pruebas puede agotar la GPU).

**Lo que estaba roto**
| Hallazgo | Causa | Arreglo |
|---|---|---|
| La consola no se podía usar: ni opción de pilotar, ni ritual, ni lanzamiento | `CompPilotConsole.CanUseNow` da por hecho que la consola tiene `CompBreakdownable`; el altar del timonel no lo tenía y lanzaba una `NullReferenceException` en cada uso | Parche `Patch_CompPilotConsole_CanUseNow_NoBreakdown` (sin averías: repararlas pediría componentes industriales en una colonia medieval) |
| El altar no salía como objetivo del ritual de lanzamiento | `RitualObligationTargetWorker_GravshipLaunch` busca solo `ThingDefOf.PilotConsole` | Parche que añade las consolas con `CompPilotConsole` |
| El Corazón no evitaba borrar el mapa | `Map.AnyBuildingBlockingMapRemoval` usa `AnyThingWithDef(GravEngine)` (solo `ThingsOfDef` estaba aliasado) | Alias también para `AnyThingWithDef` |
| El Corazón pedía "enviar a un colono a inspeccionar el motor misterioso" | `gravEngineInspected` solo se activa inspeccionando un motor encontrado (y eso lanza la misión de los mecanoides) | Al aparecer el Corazón cuenta como inspeccionado (sin misión ni carta); además ya se puede bautizar la isla |
| El ancla de gravedad vanilla (plastiacero, gravlita…) era obligatoria para no perder el mapa | `ThingDefOf.GravAnchor` | **Estaca del cielo** (`CVG_SkyStake`): cuenta como ancla de gravedad (`DefModExtension_GravAnchorAlias`) |

**Todo en lenguaje de isla (100% wuxia, sin quimiocombustible)**: mientras el mapa actual tenga un Corazón de la Isla (o ningún motor grav vanilla), `IslandText.cs` cambia al vuelo las claves de Odyssey por las `CVG_IslandText_*` (60 textos EN/ES/ES-Latino: coste de lanzamiento "N piedras espirituales" en vez de "N quimiocombustible", "piedras espirituales guardadas", "Gobernar la isla", "Zarpar", "roca de nube" en vez de subestructura, "Corazón de la Isla" en vez de motor grav, aviso de lanzamiento, nombre de la isla, atraque en vez de aterrizaje, estadísticas). Los percances de aterrizaje (`LandingOutcomeDef`) también hablan de la isla (parche + traducciones). La combustión, las recetas y los edificios ya usaban piedras espirituales o la red de qi.

## Supervivencia en el espacio, versión wuxia (Odyssey: vacío)
Odyssey mide el vacío por habitación (`VacuumUtility.GetVacuum`), los muros herméticos (`building.isAirtight` sobre subestructura) y el oxígeno con bombas (`CompOxygenPusher`). La versión wuxia cambia las piezas, no el sistema:
| Vanilla | Wuxia | Cómo engancha |
|---|---|---|
| Muro del casco (hermético) | **Muro de roca de nube** y **muro de jade** | `isAirtight`, solo sobre roca de nube (`Substructure`), mismo atlas enlazado que el casco |
| Bomba de oxígeno | **Formación del aliento** | `CompOxygenPusher` con `requiresPower`; el "poder" lo decide la red de qi (15 qi/día) |
| Barrera de vacío / sellado de salas | **Formación de cúpula celestial** | Barrera de qi por área: `GetVacuum` = 0 y temperatura fija dentro de su radio (22,9 casillas), 90 qi/día de la red; sin qi o apagada, no protege (`IslandDome.cs`) |
| Ancla de gravedad | **Estaca del cielo** | alias de `GravAnchor` |
| Traje espacial / resistencia al vacío | **Resistencia por reino** (ya existía) | `Mods/Convergence.Core/Mods/Odyssey/Patches/Vacuum.xml`: Fundación 10-30 %, Núcleo Dorado 50-85 %. La cúpula cubre al resto de la colonia; los cultivadores avanzados salen a trabajar fuera |
Todo con lore EN/ES, texturas provisionales (`Final art:` en cada def) y probado en `-cvgflighttest` (muros herméticos, formación del aliento, cúpula con y sin qi, estaca).

### Estado de verificación (2026-10-16)
- `-cvgflighttest` pasa completo con el pack del usuario + Space + Qi-Tech: consola utilizable, opción "Gobernar la isla", ritual de lanzamiento, destino válido, 50 piedras espirituales consumidas, gravbarco generado con la consola a bordo, viaje por el mundo, aterrizaje (el Corazón vuelve a funcionar tras su descanso), textos de isla, muros herméticos, cúpula con y sin qi, estaca. La misma prueba con `-cvgflightvanilla` (gravnave vanilla) da el mismo resultado: paridad.
- No cubierto: la cinemática de despegue/aterrizaje con captura de pantalla (`-cvgflightrender`), porque la ventana pequeña de pruebas agota la GPU; es código vanilla.
- **Arte pendiente (no aplicar como está):** muros de roca de nube/jade (tinte del atlas del casco), formación del aliento (cristal de la red de qi), cúpula celestial (matriz de formación teñida), estaca del cielo (ancla vanilla teñida), altar del timonel y propulsores: las texturas actuales son provisionales y no muestran lo que hacen. Necesitan concepto propio (cúpula = bóveda de qi con pilares; aliento = loto/vela que respira; estaca = pilar hundido en la montaña).


## Prueba real de una isla de endgame hasta un asteroide (2026-10-19, `-cvgislandspacetest`)
**Decisión de diseño (usuario):** la isla es un *complemento wuxia de Odyssey*: se usa el núcleo de Odyssey y solo se agrega lo de cultivación. Donde nuestra pieza era peor que la vanilla (propulsores), se usa la vanilla: el Corazón enlaza `SmallThruster`/`LargeThruster`, tanques, extensor, etc. (`linkableFacilities`) y `CVG_QiThruster`/`CVG_WindSail` quedan ocultos del menú (los defs siguen por las partidas viejas).

**Qué hace la prueba** (`Source/Convergence.Core/Space/IslandSpaceTest.cs` + `IslandEndgameBuilder.cs`): isla de radio 18 (1069 casillas de roca de nube, Corazón con 6 monturas, 2 vasijas, altar, cúpula con su red de qi, 4 propulsores vanilla y 10 salas con paredes de roca de nube, puerta y techo: comedor, cocina y despensa, salón ancestral, salón de cultivo, taller, enfermería y 4 dormitorios); 4 cultivadores de Núcleo Dorado (etapas 1-4) y un colono común. Sigue el flujo REAL de Odyssey: `InitiateTakeoff` (cinemática de 12 s), viaje, llegada al mapa de asteroide, marcador de atraque, **confirmar atraque** (`BeginLanding` y soltar el marcador interno, igual que el botón) y aterrizaje (11 s). Hay que correrla con ventana (`-Visible`).

**Resultados**
- Despegue y aterrizaje: sin daño propio al despegar. Con el combustible en piedras espirituales, la vasija se gasta (500 → 0 para un asteroide a 5 casillas con 4 propulsores; alcance 49).
- Resistencia al vacío en vacío abierto (fuera de la cúpula), 60 s: etapa 1 (0,50) severidad 0,60 → ~100 s hasta 1,0; etapa 2 (0,60) 0,48 → ~125 s; etapa 3 (0,70) 0,36 → ~167 s; etapa 4 (0,85) 0,18 → ~333 s; colono sin cultivo muere en ~50 s. Los cultivadores sobreviven pero acumulan 5-7 quemaduras de vacío: la resistencia frena la asfixia, no las quemaduras (igual que en vanilla; las evita un traje).
- **Hay que activar la Convergencia**: sin ella los cultivadores no están despiertos y el mod les quita el reino en el primer `SyncRealm` (la resistencia pasa a 0).
- Cúpula celestial con qi: dentro de su radio (22,9) el vacío es 0; con la cúpula en el centro cubre toda la isla. Qi orbital: +2 de concentración (solo con la Convergencia).
- Todas las salas conservan el aire 0,00 al aterrizar. Ninguna cosa cambia de lugar al aterrizar (0 de 320).
- **Puertas:** en Odyssey las puertas comunes NO son herméticas (`ExchangeVacuum` siempre intercambia). Con la cúpula apagada, un sala cuya puerta da al vacío se vacía aunque las paredes estén enteras, y la formación del aliento (bomba de oxígeno) no puede llenarla. Vanilla lo resuelve con una cámara de dos puertas o con la barrera de vacío (usa electricidad). Pendiente de decidir: barrera de qi hermética (puerta de formación).
- **Propulsores:** solo se enlazan si las 5 casillas de detrás NO son suelo de la nave; van en el último borde de la isla.
- **Percances de aterrizaje** (4 en vanilla, solo si el lanzamiento fue malo: probabilidad 100 % con calidad 0, 63 % 0,25, 25 % 0,5, 11 % 0,8, 2 % 1,0): choque menor (daña edificios cerca de un propulsor), propulsores averiados (`CompBreakdownable`, se reparan con componentes industriales: problema para una colonia sin industria), motor sobrecalentado y náusea gravitatoria (4 hediffs). Corren sin errores con propulsores enlazados; sin propulsores enlazados el choque menor lanza una `NullReferenceException` (es de vanilla: busca un propulsor al azar).
- Un mapa de órbita vacío (no es asteroide) solo tiene el marcador de atraque; los asteroides son objetos del mundo (`AsteroidBasic`, `AsteroidMiningSite`) con su propio generador.

### Barrera de qi (2026-10-19): la barrera de vacío de Odyssey, alimentada por la red de qi
`CVG_QiBarrier` (`IslandSurvival.xml`): misma clase y estructura que la `VacBarrier` vanilla (`Building_VacBarrier`: siempre transitable, hermética mientras "tiene energía"), pero la "energía" es un `CompPowerTrader` de consumo 0 cuyo encendido decide la red de qi (`CompQiConsumer`, 20 qi/día): no usa electricidad. Cuesta 8 jade espiritual y 12 piedras espirituales, tras la investigación de la formación de vuelo. Arte provisional (la textura vanilla teñida de jade); el arte final es un par de postes de jade con un velo de qi.
Probado (`-cvgislandspacetest`, comedor y enfermería con barrera en vez de puerta): con la cúpula apagada el comedor mantiene el aire (0,00); con una pared rota pasa a 1,00; reparada la pared y con la formación del aliento encendida baja (0,52 a los 15 s); apagando la barrera con el botón la sala se vacía otra vez (0,00 → 1,00). Las puertas comunes (`CVG_LacqueredDoor`) no retienen aire, como en vanilla.
**Arreglo en el mismo cambio:** `Patch_CompPowerTrader_PowerOn_Qi` devolvía "hay qi" ignorando el interruptor, así que apagar con el botón una formación o la barrera no tenía efecto (y el juego avisaba "Tried to power on ... which did not desire it"). Ahora es `hay qi && interruptor encendido`.
**Pendiente conocido:** la prueba `-cvgbandtotemtest` (tótem de hilos) falla 4 comprobaciones; ya fallaba antes de estos cambios.

### Qi en el espacio (medido 2026-10-19, `-cvgislandspacetest`)
Los captadores de qi **funcionan en el vacío** y rinden más: en el mapa de órbita el qi orbital suma +2 de concentración (solo tras la Convergencia), así que lo que escala con la concentración se triplica (concentración 1,0 en el planeta → 3,0 en el asteroide).
- Matriz de reunión de qi (quema piedras espirituales): 35 → 105 por día.
- Matriz de aliento celeste (cielo abierto, mareas): 57 → 170 por día. Necesita estar sin techo; el espacio no tiene techos propios.
- Pozo de veta: no sirve (necesita suelo de veta, que no hay en el asteroide). Pararrayos: 0 (no hay tormentas). Farol solar y brasero: 0 (no producen red de qi).
- Red de la prueba: oferta 92/día → 275/día contra una demanda de 270 (cúpula, 4 formaciones del aliento, 2 barreras).
Además: `CompQiNode.NotifyPowerChange` avisa `PowerTurnedOn/Off` cuando cambia la energía efectiva de un consumidor de qi (se acaba el qi o se toca el interruptor), como hace vanilla; antes la barrera de vacío no recalculaba el aire.
