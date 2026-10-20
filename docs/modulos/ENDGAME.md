# La cadena final de partida (Endgame)

Pedido del usuario (DISENO.md, ronda 3): "endgame como 4-8 misiones de exploración". Es la versión wuxia del final vanilla (nave de Royalty/Odyssey, arcotecno, monolito de Anomaly): una cadena de pasos de exploración que termina en una elección con tres finales. Código en `Source/Convergence.Stories/Endgame.cs`; los textos de los eventos son secretos (ver docs/dev/EVENTOS_SECRETOS.md): al usuario solo se le cuenta en términos generales.

## Equivalencia con lo vanilla
| Vanilla | Aquí |
|---|---|
| Piezas de la nave (reactor, motor, ordenador) | 4 llaves del final, una por paso, que cargan los jugadores desde cada reino |
| Rutas de misiones del arcotecno / sitios de Anomaly | Reinos secretos fijos (`SecretRealmDef`) con puerta, guardianes y jefe |
| Despegar la nave / escapar | Final de **Ascensión** |
| Terminar el monolito de Anomaly (cerrar el vacío) | Final de **Sellado** |
| Salvar el planeta pero quedarse | Final de **Gobierno** |
| Créditos de victoria | `GameVictoryUtility.ShowCredits` (se puede seguir jugando) |

## Los pasos (6, encadenados)
Un `GameComponent_Endgame` guarda el paso actual. Cada paso es un evento de historia (`StoryEventDef`, chainOnly) que abre una puerta de reino y se completa cuando **la colonia tiene la llave** del paso (en el suelo de un mapa propio o en el inventario de un colono). Solo entonces dispara el siguiente.

| # | Paso | Dónde | Llave | Usa |
|---|---|---|---|---|
| 0 | Inicio | tras la Convergencia ≥ 45 días y un cultivador de reino ≥ 2 | — | el **jade del Maestro** que deja pistas (si ya lo dejó, mejor; si no, el jade "despierta" solo) |
| 1 | La tumba del fundador | reino `CVG_EndRealm_FounderTomb` | Sello del fundador | reinos secretos |
| 2 | El fragmento de Jiuxiao | reino `CVG_EndRealm_JiuxiaoFragment`; la puerta se abre **sobre la isla voladora** (Corazón de la Isla) si existe; sin isla, tras 25 días aparece una "escalera de nubes" en tierra | Fragmento de Jiuxiao | isla voladora (Odyssey) |
| 3 | El campo de la Gran Ruptura | reino `CVG_EndRealm_RuptureField`, con restos de la Calamidad de Hierro (mecanoides) | Núcleo de la Ruptura | mecanoides de Biotech si están (si no, guardianes de la Calamidad propios) |
| 4 | El reino más profundo | reino `CVG_EndRealm_DeepestRealm`, jefe final | Llave del Cielo | reinos secretos |
| 5 | La elección | carta con tres opciones | consume las cuatro llaves | tribulación / sectas vivas |

Los pasos no caducan; si el jugador no recoge una llave, el paso espera. Cada paso sólo abre su puerta una vez por partida (si la puerta se pierde porque el reino colapsa sin la llave, a los 6 días se reabre).

## Los tres finales
| Final | Condición | Qué pasa |
|---|---|---|
| **Ascender** | un cultivador del reino más alto disponible | Asciende (sale de la partida) y salen los créditos de victoria con su nombre. Sin cultivador en el reino máximo la elección no se cumple: avisa y la carta vuelve al día siguiente |
| **Sellar** | siempre | La grieta de la Convergencia se cierra: las tormentas de qi cesan para siempre (flag `rift_sealed`, el planificador deja de lanzar eventos de amenaza de Jiuxiao) y las sectas aliadas lo agradecen (+25). Créditos de paz |
| **Gobernar** | siempre | La colonia toma el trono del Tribunal Celestial: todas las sectas pasan a ser aliadas (+100) y rinden tributo (piedras espirituales), pero la Calamidad de Hierro despierta: una incursión enorme de mecanoides llega a los 10 días. Créditos del reinado |

Tras los créditos el juego sigue (no sale al menú). Si la carta de elección caduca (30 días) sin decidir, vuelve a los 35. Los finales consumen las cuatro llaves y solo puede haber uno por partida.

## Reglas de compatibilidad
- Sin Anomaly/Biotech/Odyssey funciona: los guardianes mecánicos se sustituyen por los de la Calamidad (`MayRequire`), la isla es opcional (escalera de nubes).
- Los reinos finales tienen `commonality=0`: no aparecen al azar ni por puertas sueltas.
- Las llaves no se pueden vender (`tradeability None`) ni quemar.

## Prueba
`-cvgstoriestest` → sección "endgame": defs de reinos/llaves/eventos, avance de paso al recibir la llave, apertura de puertas, elección y flags. Los créditos se omiten en el test.
