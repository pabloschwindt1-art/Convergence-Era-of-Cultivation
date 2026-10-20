# Interacciones entre facciones (sectas vivas)

Implementación: `Source/Convergence.Stories/SectWorld.cs` (`GameComponent_SectWorld`) + eventos en `Mods/Convergence.Stories` (ver docs/dev/EVENTOS_SECRETOS.md para el texto de los eventos).

## Cómo funciona
- Cada 5-10 días (escalado por la frecuencia de historias) dos sectas no jugadoras al azar hacen una de cuatro cosas: **guerra** (30 %, solo una a la vez), **pacto comercial** (+35 de buena voluntad entre ellas), **riña** (−40) o **torneo**. Cada una dispara un evento de historia con las dos facciones como `{FACTION_name}` y `{OTHER_name}`.
- Las relaciones son las **vanilla** (`Faction.TryAffectGoodwillWith`, buena voluntad entre facciones; la relación Hostile/Neutral la recalcula el juego). No se usa `SetRelationDirect` (vanilla lo rechaza para facciones con buena voluntad).
- La guerra dura 10-18 días. El jugador puede ayudar a un bando (provisiones: +75 % de que gane), mediar (50 % de paz) o no hacer nada. Al terminar, la relación vuelve a Neutral; si ganó el bando que ayudó el jugador, sube su buena voluntad con él.
- Tributos (`SectTribute`) y traspaso de discípulos (`DisciplesTransfer`) son eventos normales con la secta del jugador por medio.
- **Compatibilidad con More Faction Interaction y similares:** no se tocan incidentes, misiones ni defs vanilla ni de otros mods; solo se lee y modifica la buena voluntad vía API estándar, y solo entre facciones de Convergence (`Players()`, excluye la Luna de Sangre).

## Tokens nuevos del motor de historias
`war:a`, `war:b` (la guerra actual), `pact:a`, `pact:b` (el último par), `factionToken2` (segunda facción para el texto) y el resultado `SO_SectWar` (`favorA`, `favorB`, `peace`). Las etiquetas de las opciones también reciben los argumentos de facción.

## Prueba
`-cvgstoriestest` → sección "sect world": eventos, textos, tokens, guerra → hostilidad, favor, mediación y desenlace.
