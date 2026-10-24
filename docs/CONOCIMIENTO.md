# Dónde buscar antes de implementar

Pedido del usuario (2026-10-23): "siempre ves cómo lo hace vanilla primero" y "todo lo que tenés local, usalo sin que te lo
pida". **Regla:** antes de diseñar o programar algo, buscar en este orden y citar lo que se usó. Si la respuesta está acá,
no se abre el juego para averiguarla ni se adivina.

## 1. Cómo lo hace vanilla (datos exactos de tu versión)

`referencias/vanilla/`: generado por `python -I tools/vanilla-index.py` desde los Defs de la 1.7 de Steam (Core y las cinco DLC).
No está en git; se regenera en segundos cuando cambia la versión. Para la 1.6:
`python -I tools/vanilla-index.py --data C:/Games/RimWorld_1.6/Data --out referencias/vanilla-1.6`.

| Archivo | Para qué |
|---|---|
| `INDEX.md` | Cómo buscar y cuántos defs hay de cada tipo |
| `labels.tsv` | defName, tipo, DLC, nombre en inglés y en español: del nombre que usa el jugador al defName (`grep -i "recargador"`) |
| `defs/<Tipo>.xml` | Cada def **con lo heredado ya resuelto** (`_parents` = cadena de padres, `_dlc`, `_file`) |
| `mechs.md` | Mecas: peso, banda, recargador que necesitan, trabajos |
| `research.md` | Investigaciones: nivel, costo, requisitos, mesa |
| `scenparts.md` | Partes de escenario (clase y def) |
| `buildings.md` | Edificios: tamaño, minificable, costo, investigación |
| `apparel-stats.md` | Ropa: capas, armadura, bonos (`MechBandwidth`…) |

**Código vanilla:** `ilspycmd -t <Namespace.Clase> "C:/Program Files (x86)/Steam/steamapps/common/RimWorld/Assembly-CSharp.dll"`
(la lógica exacta; p. ej. `RimWorld.ResearchManager` muestra que terminar una investigación completa sus requisitos).
**Los DLC son mods de Ludeon:** `Data/<DLC>/Defs` muestra cómo Ludeon estructura contenido opcional (Royalty, Ideology, Biotech,
Anomaly, Odyssey): copiar ese patrón antes de inventar uno.

## 2. Cómo funciona (la wiki, resumida)

`referencias/wiki/` (no está en git): resumen de la wiki de RimWorld por tema, con la URL de origen y la fecha. Empezar por
`referencias/wiki/INDEX.md` (palabras clave por archivo). Incluye mecánicas (mecanitor y banda, investigación, escenarios,
salas, calidad, narrador e incidentes, comercio, Ideology, Royalty, Biotech, Anomaly, Odyssey, habilidades, ánimo, salud,
combate, cultivos, animales, energía y temperatura, trabajo y mesas) y las guías de modding (defs, parches, Harmony, idiomas,
gráficos). Si un número de la wiki choca con `referencias/vanilla/`, manda el de vanilla (es tu versión exacta).

## 3. Cómo lo resolvieron otros mods (y cuáles podemos aprovechar)

**Primero `referencias/mods/INDEX.md`** (no está en git): catálogo de los mods que juega el usuario y de las copias de estudio.
Por cada uno dice qué sistema ofrece, cómo se enchufan otros mods, su licencia y para qué sistema de Convergence sirve, más
un ranking de las mejores oportunidades de integración. Recordar la regla: **nada se vuelve obligatorio** salvo Harmony;
integrar = compatibilidad opcional (`MayRequire`, `IfModActive`), copiar el patrón, o reutilizar código solo con licencia libre.

| Documento | Qué tiene |
|---|---|
| `docs/ANALISIS_VE.md` | Vanilla Expanded: cómo integran contenido en el mundo (incidentes, landmarks, mesas, cultivo) |
| `docs/ANALISIS_VPE.md` | Vanilla Psycasts Expanded: estructura de habilidades (base de las técnicas) |
| `docs/ANALISIS_REFERENCIAS.md` | Isekai RPG Leveling, KotOR, The Force, frameworks: niveles, rangos, auras |
| `docs/ANALISIS_RIMIMMORTAL.md` | RimImmortal y Cultivator of the Rim: mods de cultivo |
| `docs/ANALISIS_ACS.md` | Amazing Cultivation Simulator: linajes, bestias, feng shui |
| `docs/ANALISIS_CRIATURAS.md` | Mods de razas, dragones y criaturas |
| `docs/INVESTIGACION_WORKSHOP.md` | Qué existe en Workshop y GitHub y qué se puede reutilizar por licencia |
| `docs/revision/INVESTIGACION_ARTE.md` | Cómo se hace el arte de RimWorld |
| `docs/revision/HERRAMIENTAS_GITHUB.md` | Herramientas para navegar código y validar |
| `referencias/LEEME.md` | Copias de mods ajenos (`referencias/`): qué hay y licencias (MIT/Unlicense reutilizables; el resto solo estudio) |

## 4. Nuestro propio diseño y lo ya resuelto

- `docs/INTEGRACION.md`: con qué sistema vanilla se conecta cada cosa del mod (principio n.º 1).
- `docs/DISENO.md` §8 (decisiones), `docs/DISENO_CORE.md`, `docs/modulos/*.md` (un documento por módulo).
- `docs/indice/` (`INDICE.md`, `DEFS.tsv`, `CODIGO.md`): todo lo nuestro, generado por `tools/make-index.py`.
- `docs/PUNTOS_CIEGOS.md`, `docs/AUDITORIA_FUNCIONAL.md`: lo que falta o falla.
- Skills: `rimworld-mod-lecciones` (errores ya cometidos), `rimworld-mod-programador`, `rimworld-mod-tester`.

## 5. Recién después: la red

Wiki completa, foros de Ludeon y Steam, GitHub. Lo que se encuentre y sirva, resumirlo en `referencias/wiki/` para la próxima.
