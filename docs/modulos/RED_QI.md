# Red de qi (energía propia del mod)

Pedido del usuario (2026-10-15): los edificios del mod no deben depender de electricidad ni mostrar "sin energía". Hoy el taller
de marionetas, el recargador espiritual y el estanque de sangre usan un `CompPowerTrader` de consumo 0 (truco) y el juego los
marca sin energía. Se reemplaza por una **red de qi**: se genera explotando venas de qi y se reparte por formaciones.

Guía de diseño (lo que ya funciona): la red eléctrica vanilla (generador → conductos → batería → consumidor) y la cobertura por
radio de las lámparas solares/escudos. Mismo modelo mental para el jugador, con estética de formaciones.

## Piezas (pocas, con identidad)
| Pieza | Equivalente vanilla | Qué hace |
|---|---|---|
| **Pozo de vena de qi** | Generador geotérmico (va sobre un géiser) | Se construye sobre una casilla de vena de qi (alta `CVG_QiConcentration` / veta de dragón). Produce mucho qi por día, sin combustible |
| **Formación de reunión de qi** | Generador de combustible | En cualquier lugar; quema piedras espirituales y produce qi. Rinde más cuanta más concentración de qi haya |
| **Pilar de formación** | Conducto (pero por nodos) | Extiende la red: une todo lo que esté dentro de su radio (se ve el círculo al colocarlo, como las lámparas solares). Sin cables |
| **Cristal acumulador** | Batería | Guarda qi; se descarga despacio |
| **Consumidores** | Cualquier edificio con energía | `CompQiConsumer` con consumo por día; sin qi, el edificio se apaga (icono propio de "sin qi", no el rayo eléctrico) |

- **Red = conjunto de fuentes, pilares, acumuladores y consumidores conectados por radio** (como `PowerNet`, recalculada al construir o destruir).
- Superposición: en el modo de colocación y al seleccionar, se ven los radios y una línea tenue entre nodos conectados.
- Inspector: "Red de qi: +120 / −80 por día, almacenado 340/500".
- Los cultivadores pueden **cargar** un acumulador a mano con su qi (trabajo "Infundir qi"), como la recarga de las marionetas.
- **Qi-Tech** (más adelante): un convertidor qi ⇄ electricidad para quien juegue con tecnología.

## Migración
Taller de marionetas, recargador espiritual, estanque de sangre (y lo que venga: altar de injertos, horno de alquimia de alto nivel,
formaciones de defensa) pasan de `CompPowerTrader` de consumo 0 a `CompQiConsumer`. Código en el Core (`Source/Convergence.Core/QiNetwork/`),
genérico y configurable por XML; los edificios en Convergence.Formations (fuentes, pilares, acumulador).

## Implementación (2026-10-15)
- **Core** `Source/Convergence.Core/QiNetwork/`: `CompQiNode` (un solo componente configurable por XML: `produceDay`, `consumeDay`, `radius`, `storageCapacity`, `leakPerDay`, `needsVein`, `needsFuel`, `scaleWithConcentration`) y `CompQiConsumer` (rol consumidor); `QiNetworkManager` (MapComponent: agrupa nodos en redes por radio de pilar, cada 250 ticks reparte producción, carga/descarga acumuladores y decide si la red está abastecida; si no, los consumidores se apagan); `JobDriver_InfuseQi` (gizmo "Infundir qi" en el acumulador: un cultivador vierte hasta la mitad de su qi, máx. 150).
- **Vetas**: `TerrainDef CVG_QiVein` (+1 de qi de feng shui). `QiNetworkUtility.EnsureVeins` las crea en el mapa de casa tras la Convergencia (una cerca del centro y otras repartidas); solo sobre suelo natural libre.
- **Integración con los edificios vanilla**: los que exigen `CompPowerTrader` (gestador, cargador, cripta) lo conservan con consumo 0 y `showPowerNeededIfOff=false`; un parche a `CompPowerTrader.PowerOn` hace que su estado lo decida la red de qi, y otro quita el texto "no conectado a la red eléctrica". El icono "sin qi" se dibuja aparte (`UI/Overlays/CVG_NoQi`, provisional: el rayo vanilla en azul).
- **Formations** (investigación `CVG_Research_QiNetwork`): pozo de vena de qi (2×2, 200 qi/día, solo sobre veta), formación de reunión de qi (70 qi/día base, quema piedras espirituales, ×0,5–×3 según la concentración), pilar de formación (radio 9,9), cristal acumulador (1000, pierde 5 %/día).
- **Migrados**: taller de marionetas (40 qi/día), recargador espiritual (30; ya no quema piedras) y cripta de sueño de muerte (20). `Convergence: Biotech Wuxia` pasa a depender de Formations. El tótem de hilos sigue con piedras espirituales (no está en la red).
- Autotest: `-cvgqinettest` (red, pilar por radio, acumulador, apagado, vetas, formación de reunión).
- Pendiente: convertidor qi ⇄ electricidad (Qi-Tech); altar de injertos, horno de alquimia y formaciones de defensa aún sin consumo de qi.

## Números finales (pulido, 2026-10-16)
Referencia vanilla: generador a leña 1000 W, solar ~1700 W pico (≈ 50 % de media), geotérmico 3600 W; estufa eléctrica 350 W. Aquí 1 qi/día ≈ 14 W.

| Fuente | qi/día | Notas |
|---|---|---|
| Formación de reunión | 70 (×0,5–3 según concentración) | quema piedras espirituales; alimenta 2-3 edificios pequeños (cripta 20, recargador 30, tótem 10) |
| Respiración del cielo | **85** (antes 55) | gratis, a cielo abierto, sube en la Marea de Qi; queda entre la reunión y el pozo |
| Pozo de vena | **200** (antes 160) | claramente la mejor: ~2,9× una reunión; solo sobre veta |

Consumo: taller de marionetas 40, recargador 30, cripta 20, lámpara solar 60 (la lámpara vanilla pide ~3 generadores; esta, menos de uno).
