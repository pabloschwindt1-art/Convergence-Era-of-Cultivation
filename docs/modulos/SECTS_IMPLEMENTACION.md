# Convergence: Sects — implementación (v0.1)

Mod `Mods/Convergence.Sects` (solo XML). Spec: [../lore/SECTAS.md](../lore/SECTAS.md). Depende del Core; opcional Races (linajes).

## Qué hay
- 6 `FactionDef` (`CVG_Faction_*`): Azure Peak (Medieval), Silent Lotus (Medieval), Heavenly Cauldron (Industrial), Ancient Dragon (Medieval),
  Blood Moon (Medieval, `permanentEnemy`), Rogue Cultivators. Ninguna anti-tecnología.
- `PawnKindDef` por rango (`CVG_<Secta>_<Rango>`): externo → interno → núcleo → anciano (los nombres de rango salen en la etiqueta),
  más un comerciante. Cada uno lleva `DefModExtension_CultivatorPawnKind` (Core): reino, etapa, manuales y elementos de raíz.
  Reinos actuales: Refinamiento de Qi (1), Fundación (2), Núcleo Dorado (3); el Clan del Dragón empieza en Núcleo Dorado.
- Linajes con Races: Loto → Qilin (30%), Pabellón → Fénix/Zorro (12% c/u), Dragón → Dragón (100%).
- Comerciantes (`CVG_Trader_*`): el Pabellón vende píldoras de fundación, piedra espiritual, jade espiritual y tablillas de jade y compra casi todo;
  Pico vende armas de filo; Loto, hierbas y ropa (sin armas); Dragón, tesoros; Errantes, de todo un poco.
- Nombres: `RulePackDef CVG_NamerPerson_*` (apellido + nombre, estilo chino) y `CVG_NamerSettlement_*`; nombres de facción con `fixedName`.

## Cómo llegan (Core)
- Las facciones **no existen hasta la Convergencia**: `startingCountAtWorldCreation=0`, `requiredCountAtGameStart=0`.
  `DefModExtension_ConvergenceFaction` (en el FactionDef) las crea al ocurrir la Convergencia (y al cargar una partida donde ya pasó) con
  `FactionGenerator.CreateFactionAndAddToManager` + asentamientos extra, y fija sus relaciones entre sectas (aliadas/hostiles).
  Antes de la Convergencia no hay facción ⇒ no hay asaltos, comercio ni asentamientos.
- Las "grietas" son solo sabor: aparecen como asentamientos normales del mapa mundial (sin grietas visuales todavía).

## Escalado (dificultad justa, DEFENSA §4)
`DefModExtension_CultivatorPawnKind.scaleWithColony` (por defecto sí) + ajuste del Core **"Cultivadores enemigos limitados a tu reino más alto +1"**
(`scaleEnemyRealms`, activado): un pawn hostil generado nunca pasa del reino más alto de un colono libre **+1** (sin cultivadores en la colonia ⇒ reino 1).
Además la curva `maxPawnCostPerTotalPointsCurve` (heredada del Outlander) limita qué rangos entran según los puntos del asalto.
Pendiente (DEFENSA §4): carta especial "presión de aura" para asaltos con reino alto.

## Culto de la Luna Sangrienta
Hostil permanente (`permanentEnemy`). **No** se detecta aún "inicio de secta demoníaca" ni ideología Camino Demoníaco (no hay escenario ni meme). 
Cuando existan, cambiar a `naturalEnemy` y poner la relación en la creación según el meme/escenario.

## Ideología: no implementado
Los memes `CVG_Meme_*` (SwordWay, LotusPath, CauldronWay, DragonBlood, DemonicWay, WanderingPath) requieren `MemeDef` con grupos de requisitos, iconos y
preceptos (Ideology) — es un trabajo aparte (arte de iconos incluido). Queda para un submódulo con `LoadFolders IfModActive="Ludeon.RimWorld.Ideology"`.

## Pendiente
- Rangos y Contribución del jugador en cada secta, misiones y títulos (módulo Quests & World).
- Tablillas de técnica firma de cada secta (M5 tiene 5 manuales genéricos por elemento; los manuales se reparten por afinidad).
- Reinos 4–6 cuando existan (Alma Naciente para los ancianos; hoy topan en Núcleo Dorado).
- Prueba en juego: ver "Cómo probar" abajo.

## Cómo probar
Con el juego cerrado y sin `RimWorldWin64`: activar Core + Sects en una partida de prueba, en debug: *Convergence: Fire Convergence*, comprobar en el mapa mundial las
6 facciones, y `Spawn pawn` con los kinds `CVG_*`. Leer `Player.log`.
