# Herramientas y entorno de desarrollo

Fecha: 2026-10-15. Incluye la evaluación del PDF de Gemini
("Manual de Arquitectura e Ingeniería de Mods para RimWorld", 5 páginas).

## Datos verificados en esta PC

| Elemento | Valor |
|---|---|
| RimWorld | 1.6.4871 rev590, todas las DLC |
| Unity del juego | 2022.3.35 (de `UnityPlayer.dll`) — necesario para el debug con build de desarrollo |
| Harmony (mod de Workshop 3.0.1) | trae **0Harmony 2.4.3 para 1.6** y **3.0.0 para 1.7** |
| .NET | solo runtime 9.0.3 — **no hay SDK instalado** (hace falta para compilar) |
| Código oficial | `RimWorld/Source/` trae 43 `.cs` de ejemplo de Ludeon; `ModUpdating.txt` = changelog de API por versión |

## Evaluación del PDF de Gemini

**Útil y correcto (lo adoptamos):**
- Proyecto `.csproj` estilo SDK con NuGet `Krafs.Rimworld.Ref` (sin copiar DLLs del juego) y `Lib.Harmony` con `ExcludeAssets="runtime"` (no incluir 0Harmony.dll en el mod).
- `Krafs.Publicizer` para acceder a miembros privados sin reflexión.
- Decompilar `Assembly-CSharp.dll` con ILSpy/dnSpyEx para leer el código del juego.
- Debug con breakpoints: `UnityPlayer.dll` de desarrollo de la misma versión de Unity + `player-connection-debug=1` + attach desde Rider/VS.
- Rendimiento: nada pesado en `Tick()`; usar `TickRare` (250), `TickLong` (2000), `IsHashIntervalTick`. Dubs Performance Analyzer para medir.
- RimSort como gestor de mods; lista de frameworks con sus GitHub.

**Desactualizado o incorrecto:**
- `Lib.Harmony 2.3.*`: eso es de 1.5. Para 1.6 es **2.4.x**, y para 1.7 Harmony **3.0** (posibles cambios de API).
- **No menciona 1.7 ni el motor Waylith** (anunciado por Ludeon): ni .NET distinto, ni riesgos de Unity (`GL`, `Shader.Find`, AssetBundles). Todo el capítulo asume Unity.
- Recomienda AssetBundles para shaders: con el cambio de motor es justo lo más frágil.
- El ejemplo de Harmony (postfix a `StatPart_WorkTableUnpowered.TransformValue` para acelerar investigación) es un mal ejemplo: esa StatPart solo actúa en mesas sin energía. Para eso se usa una StatPart propia o un `statOffset`.
- "About.xml generado automáticamente al subir a Steam": falso, lo escribe uno; el que se genera es `PublishedFileId.txt`.
- El árbol de carpetas y la tabla de "5 pilares" están desalineados (descripciones cruzadas entre GameComponent/MapComponent/ThingComp).

**Incompleto:** el PDF quedó cortado. El **capítulo 5 (IA, renderizado, shaders, UI) está vacío**, que era justo la parte de "diseños y características especiales"; la sección de parches XPath y el ejemplo de `Scribe` se cortan a mitad.

**Faltan temas importantes:** traducciones (`Keyed`/`DefInjected`), texturas (tamaños, `Graphic_Multi`, máscaras), modo desarrollador del juego (acciones de debug, inspector de Defs, recarga), errores típicos del log, publicación en Workshop desde el juego, compatibilidad con Multiplayer y Combat Extended, comunidad (wiki de modding de RimWorld, Discord oficial de mod dev).

### Versión completa (8 páginas) — capítulo 5

Llegó una segunda versión con el capítulo 5. Sigue con defectos: cada página se corta a mitad de código
(el transpiler, el JobDriver, el `PostDraw` y el ITab quedan incompletos) y las páginas 7-8 repiten los
capítulos 4, 6 y 7 de la versión anterior.

- **5-A IA y trabajos** (correcto): ThinkTree → `WorkGiver_Scanner` → reservas → `JobDriver` con Toils; JobDef/WorkGiverDef en XML. Es el patrón para meditar, refinar píldoras o trabajar en formaciones.
- **5-B Gráficos** — **lo más valioso para nosotros**: desde 1.5 un `HediffDef`/`TraitDef`/ropa puede inyectar nodos en el `PawnRenderTree` con `renderNodeProperties`, **sin Harmony**. Sirve para auras, halos o marcas de reino: un hediff por reino con su nodo gráfico. Es más limpio y seguro para 1.7 que el parche a `Pawn.DrawAt` de Isekai.
  - Matiz que el PDF omite: `PostDraw()` de un comp solo se llama si el edificio se dibuja en tiempo real (`drawerType` `RealtimeOnly` o `MapMeshAndRealTime`). Y animar con `Time.realtimeSinceStartup` sigue animando con el juego en pausa: usar ticks del juego.
- **5-C Shaders/UI**: insiste en AssetBundles (lo evitamos por 1.7). El ejemplo de `ITab` con `Listing_Standard`/`Widgets` es correcto y es el camino para la pestaña de cultivo.

### Versión final (9 páginas) — descartada

Solo agrega ejemplos ya conocidos (transpiler con `CodeMatcher`, `WorkGiver_Scanner`, `ModSettings`) y un capítulo 8
sobre 1.7 que repite lo que ya verificamos arriba (dice "C# 12/13"; lo verificado es **.NET 10**). Sigue cortándose
a mitad de página. Lo único que anotamos: un `PawnRenderNode` propio puede sobrescribir `ColorFor(pawn)` para cambiar
el color en vivo, útil para el aura según el reino o el elemento. **No se vuelve a consultar.**

**Conclusión:** sirve como resumen de la cadena de herramientas (csproj + NuGet + Publicizer + debug), que es lo que más aporta. En arquitectura no añade nada respecto a los análisis internos de mods de referencia, y en versiones está atrasado.

## Instalado y probado (2026-10-15)

- **.NET SDK 8.0.426** (winget `Microsoft.DotNet.SDK.8`).
- **ilspycmd 9.1.0.7988** (`dotnet tool install -g ilspycmd --version 9.1.0.7988`; la última versión no instala con SDK 8).
  Uso: `ilspycmd -t Verse.ThingComp "<RimWorld>/RimWorldWin64_Data/Managed/Assembly-CSharp.dll"`.
- **Git 2.50** ya estaba.
- Compilación de prueba `net472` OK en 5 s, sin advertencias. NuGet resolvió **Krafs.Rimworld.Ref 1.6.4871** (igual al juego), **Lib.Harmony 2.4.3** (igual al mod Harmony) y **Krafs.Publicizer 2.3.2**. `0Harmony.dll` no se copia a la salida (correcto).
- Ojo: MSBuild falla con rutas muy largas (>260 caracteres). Mantener el proyecto en rutas cortas como `C:\MODS_Rimmworld`.

## RimWorld 1.7 (Waylith): lo verificado

Info recibida (anuncio oficial de Ludeon + resumen de Gemini) contrastada con los DLL reales de los mods copiados:

| Afirmación | Estado | Evidencia |
|---|---|---|
| 1.7 corre en .NET moderno (CoreCLR) | **Confirmado: .NET 10** | `0Harmony.dll` 1.7 y `VEF.dll` 1.7 compilan para `.NETCoreApp v10.0` |
| Harmony 3.0 para 1.7 | **Confirmado** | Harmony mod trae 0Harmony 3.0.0 en `1.7/` (2.4.3 en `1.6/`) |
| Capa de compatibilidad con stubs de Unity | **Confirmado** | VEF 1.7 referencia `UnityStubs` en lugar de `UnityEngine.CoreModule` |
| Los DLL de 1.6 siguen funcionando en 1.7 | **Confirmado en la práctica** | Isekai declara 1.7 con **un solo DLL .NET Framework 4.8**, sin carpeta `1.7/` |
| Nombres `Waylith.UnityCompat`, `Waylith.AssetBake`, `PostEffectPass`, Vulkan, debug directo con Rider | Sin verificar | Hay que mirar los archivos de la rama beta "unstable" |

**Estrategia para nuestro mod:**
1. **Ahora:** un solo DLL `net472` para 1.6 que también carga en 1.7 por la capa de compatibilidad (como Isekai). Sin AssetBundles ni `GL`.
2. **Cuando 1.7 sea estable** (o haga falta rendimiento o efectos nuevos): segundo build nativo en `1.7/Assemblies` para `net10.0` con Harmony 3, cargado con `LoadFolders.xml`. Requiere instalar el **.NET SDK 10** (el 8 actual no compila net10).
3. Para verificar el resto: instalar la rama "unstable" en la instalación de desarrollo y revisar `RimWorldWin64_Data/Managed` (o donde estén ahora los ensamblados) con ilspycmd.

## Cadena de herramientas que vamos a usar

1. **.NET SDK 8** (o 9) — para `dotnet build` (compila `net472` con paquetes de referencia).
2. **Proyecto SDK** con `Krafs.Rimworld.Ref 1.6.*`, `Lib.Harmony 2.4.*` (`ExcludeAssets=runtime`), `Krafs.Publicizer`; salida a `1.6/Assemblies/`; `.pdb` portable.
3. **ILSpy** (o `ilspycmd`) para leer el código del juego.
4. **Editor:** VS Code con C# Dev Kit, o Rider (mejor para attach al juego).
5. **En el juego:** modo desarrollador, Dubs Performance Analyzer, RimSort.
6. **Git** en `C:\MODS_Rimmworld` (sin `referencias/`).
7. **1.7:** cuando toque, una carpeta `1.7/` con build aparte (Harmony 3).


## Pruebas automáticas: una por vez
Las pruebas `-cvg...test` son componentes que corren **a la vez sobre el mismo mapa** y varias despejan o usan la zona del centro (isla, formaciones, tótem). Si se lanzan juntas se pisan y aparecen fallos intermitentes que no son del mod (p. ej. el tótem de hilos o las formaciones). Para un resultado fiable, una prueba por lanzamiento:
`powershell -ExecutionPolicy Bypass -File tools/run-test.ps1 -GameArgs "-quicktest -cvgformationstest" -Mods "..."`.
Estado 2026-10-19 (cada una sola): Core, recursos, linaje, rasgos, red de qi, isla, Qi-Tech, tótem de hilos y formaciones pasan. `-cvgislandspacetest` (vuelo real a un asteroide) necesita `-Visible` y es lenta (~4 min).
