# Auditoría 2026-10-22 — resumen para decidir

Detalle completo, línea por línea: [AUDITORIA.md](AUDITORIA.md), generado por `python tools/audit.py`, que se puede repetir.
Índice del proyecto: [indice/INDICE.md](indice/INDICE.md), [indice/DEFS.tsv](indice/DEFS.tsv) y [indice/CODIGO.md](indice/CODIGO.md), generados por `python tools/make-index.py`.
Alcance: los 15 mods activos. Convergence.Sects no se audita porque está descontinuado.

## Limpio (sin hallazgos)

- XML: todos se pueden leer.
- Referencias a defs `CVG_`: no falta ninguna. Se revisaron las referencias en los XML, en los parches y en los `GetNamed` del C#.
- Textos Keyed: están las 382 claves que pide el código, en EN, ES y ES-Latin. Las claves armadas por concatenación no se pueden verificar así.
- Texturas propias: existen las 623 rutas.
- Traducciones huérfanas: no hay. Los falsos positivos se descartaron: las recetas `Make_*` que genera `recipeMaker` y los defs vanilla de Odyssey que se traducen a propósito.

## Para decidir

### A. Lore que nombra las sectas viejas (unas 128 descripciones en inglés, cada una con sus 2 traducciones)

Hay objetos, rasgos y backstories que dicen "monjes del Templo del Loto Silencioso", "espadachines del Pico Azur" o "el Clan del Dragón Antiguo".

| Mod | Descripciones EN |
|---|---|
| Core (sobre todo `Backstories_Cultivation.xml`, 24) | 33 |
| Traits & Destinies (`Traits_Bodies`, `Traits_Crafts`) | 26 |
| Furniture | 15 |
| Races | 14 |
| Alchemy | 10 |
| Apparel, FaunaFlora | 7 c/u |
| Formations, Grafts | 5 c/u |
| Resources | 3 |
| Qi-Tech | 2 |
| Space | 1 |

Propuesta:
- Pico Azur → Espada Sin Forma.
- Loto Silencioso → Caldero Celestial.
- Los lugares cambian según el lore nuevo; por ejemplo, el Cementerio de Espadas pasa a ser el Túmulo de Espadas.
- El Clan del Dragón queda como está hasta que decidamos qué hacer con él. Se puede dejar como leyenda ("un clan extinto" o "que ya nadie ve") o reemplazarlo.
- Se haría mod por mod, cuando le toque a cada uno, y no todo de golpe.

### B. Apparel: parche de túnicas para el Sects viejo

`Apparel/Patches/SectRobes.xml` aplica túnicas a los pawnkinds de Sects (AzurePeak, SilentLotus y AncientDragon). Es inerte sin Sects, porque está dentro de FindMod. Propuesta: borrarlo, porque `SectRobes_Factions.xml` ya lo reemplaza. También hay que sacar la mención a "Convergence: Sects" de la descripción en `About.xml`.

### C. Menciones al mod Sects

- `Core/Common/Defs/HistoryEvents_Sect.xml:3`: es un comentario y no lo ve el jugador. Propuesta: cambiar "Convergence: Sects" por Factions.
- `Factions/About/About.xml:22`: dice "Replaces Convergence: Sects (do not use both)". Propuesta: dejarlo, porque es información útil para el jugador.
- `Factions/RogueCultivators.xml:6`: es un comentario. Propuesta: dejarlo o borrarlo, da igual.
- `Stories/About/About.xml:13` y `Stories/Mods/Sects/` (escenario DragonHeir y eventos del dragón): dependen de que decidamos qué hacer con el Dragón.

### D. Lin Wuchen en una backstory de Factions

`FormlessSword_Backstories.xml:108` (`CVG_Backstory_FS_DisciplineElder`) nombra a Lin Wuchen, un personaje del Pico Azur viejo. Propuesta: cambiarlo por un personaje de la Espada Sin Forma o sacar el nombre.

### E. Stories: código con alias viejos

- `StoryEngine.cs:115-123` y `StoryEvents.cs:106,237` todavía buscan `CVG_Faction_AzurePeak`, `SilentLotus` y `AncientDragon` como alternativa por si no está la secta nueva.
- No rompe nada: si esa facción no existe, el código devuelve null y sigue.
- Propuesta: dejar los alias hasta decidir el Dragón y limpiarlos junto con eso.

### F. Etiquetas en español que faltan (menor)

`Factions/SectIdeoIcons.xml`: el ColorDef `CVG_IdeoColor_Natural` no tiene etiqueta en español. Propuesta: agregarla ("natural").

## Fuera de esta auditoría

- Las descripciones que no dicen qué hace el objeto, como la píldora que encontraste, quedan para cuando revisemos cada addon. Alquimia ya tiene su inventario en `docs/modulos/ALQUIMIA_INVENTARIO.md`.
- `docs/*.md`: los documentos de diseño viejos todavía hablan de Sects. Son historia y no se tocaron.
