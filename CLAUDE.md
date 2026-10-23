# Convergence: Era of Cultivation: guía para colaboradores (y asistentes de IA)

Mod de RimWorld 1.6 de **Prospero1088** (packageId `prospero1088.convergence.core`, prefijo de defName `CVG_`, namespace C# `Convergence`).
Cultivo xianxia / wuxia con tecno-cultivo, inspirado en *Tale of Immortal* y en el subgénero del renacimiento del qi. Documentación y textos del proyecto en español.

## Lee primero
- `docs/LORE.md`: lore aprobado (la Convergencia, Jiuxiao, la Calamidad de Hierro, facciones, glosario).
- `docs/DISENO.md`: diseño general y decisiones tomadas.
- `docs/DISENO_CORE.md`: diseño técnico del Core e hitos.
- `docs/INTEGRACION.md`: sistemas de RimWorld y de sus DLC con los que se integra el mod.
- `docs/EVENTOS.md`, `docs/HERRAMIENTAS_Y_ENTORNO.md`.
- `docs/WORKSHOP.md`: documentación técnica de los 15 mods y guía de publicación en Steam Workshop; `docs/WORKSHOP_TEXTOS.md` (páginas BBCode); `tools/check-workshop.py` (revisión previa a subir).

## Decisiones de diseño firmes (no contradecirlas sin consultar al autor)
- **Todo lleva lore:** cada objeto, mueble, injerto, técnica, rasgo, gen, bestia y evento tiene una descripción con lore (origen, leyenda, a qué secta o mito se vincula) de 2-3 frases, en inglés en el def y en español en `DefInjected`. Fuente y tono: `docs/LORE.md` y `docs/lore/`.
- **De casi todo lo que ya funciona, su versión wuxia:** antes de diseñar algo, buscar el equivalente vanilla/DLC y hacer su versión con el mismo sistema y estructura, con estética, materiales y lore propios (electricidad -> red de qi, mecanitor -> titiritero, gravnave -> isla voladora, psycasts -> técnicas). Nada desde cero sin necesidad.
- **Integrarse con TODOS los sistemas existentes del juego y sus DLC** (ver `docs/INTEGRACION.md`); nada de sistemas paralelos. Actualizar esa tabla al terminar cada hito.
- Tono **tecno-cultivo**. Se juega **con y sin tecnología**: el Core nunca requiere electricidad ni investigación industrial; lo tecnológico va en el módulo Qi-Tech o marcado `TechOnly`.
- **Las sectas no son anti-tecnología:** la postura ante la tecnología la decide la Ideología del jugador.
- Cada implante tiene una **disonancia celestial** configurable (stat) que afecta a la tribulación.
- Raíz espiritual = gen con Biotech / rasgo sin Biotech. Cuerpos especiales latentes hasta la Convergencia. La tribulación puede matar (con opción en los ajustes).
- El Maestro llega herido con la Convergencia, enseña, se va y deja un jade (cadena de búsqueda).

## Estructura de mods
- **Convergence: Core** = base y framework (cultivo, qi, reinos, Convergencia, tipos genéricos como `BloodlineDef`, `CultivationPathDef`).
- **Un mod aparte por módulo**, cada uno en `Mods/Convergence.<Módulo>/` (+ `Source/Convergence.<Módulo>/` solo si necesita código), dependiendo del Core. Hoy: Resources, Formations, Races, Traits & Destinies, Sects, Alchemy, Apparel, Fauna & Flora, Stories, Grafts, Qi-Tech, Furniture, Biotech Wuxia y Flying Sect (carpeta `Space`).
- Los módulos dependen del Core, nunca al revés ni entre ellos en círculo. Preferir XML sobre código en los módulos. Los módulos sin DLL propio usan las clases del DLL del Core.

## Reglas técnicas
- RimWorld **1.6** (net472, Krafs.Rimworld.Ref 1.6.4871, Lib.Harmony 2.4.3 con `ExcludeAssets=runtime`, sin `0Harmony.dll` en los mods). Un solo DLL por proyecto que también carga en 1.7 (verificado en 1.7.4985: los 15 mods cargan y los autotests pasan).
- Dependencia externa: solo Harmony. Se diseña usando **todas las DLC**, pero cada función de DLC va en `Mods/<DLC>` (LoadFolders `IfModActive`), con `MayRequire` o `ModsConfig.XActive`, para que sin la DLC el mod funcione sin esa parte (si es demasiado trabajo, ese módulo requiere la DLC). **Nada de AssetBundles, `GL` ni `Shader.Find`**: solo flecks/effecters vanilla y `renderNodeProperties`.
- Datos en XML, lógica en C#. Sin enums cerrados para contenido: cada cosa nueva debe poder agregarse con XML.
- Textos siempre en `Keyed`/`DefInjected`: inglés + Spanish + SpanishLatin. Las traducciones de un def condicional (`MayRequire`) van en la carpeta condicional correspondiente, no en `Common`.
- Compilar (el Core primero): `dotnet build Source/Convergence.Core/Convergence.Core.csproj -c Release` y los otros cuatro proyectos (ver README). Los DLL salen en `Mods/<Mod>/1.6/Assemblies/` y no se versionan.
- Jugar desde el repositorio: `tools/link-mods.ps1` enlaza cada carpeta de `Mods/` en `<RimWorld>/Mods/` con un junction. Log del juego: `%USERPROFILE%/AppData/LocalLow/Ludeon Studios/RimWorld by Ludeon Studios/Player.log`.

## Pruebas
- Instancia de prueba aislada: `powershell -ExecutionPolicy Bypass -File tools/run-test.ps1 [-Version 1.6|1.7] -GameArgs "-quicktest -cvgselftest" [-Mods ...|todos] [-TimeoutSec N]` usa una carpeta de datos propia (ModsConfig y Prefs aislados), ventana chica minimizada (o visible en el monitor secundario, si hay uno) y log propio, y cierra el juego al terminar. Listas de mods de ejemplo: `tools/pack-completo.txt`, `tools/pack-oleada1.txt`.
- Reglas: un solo RimWorld de prueba a la vez; cerrar el juego al terminar; no tocar `Prefs.xml` ni `ModsConfig.xml` de nadie; los autotests deben ser inertes sin su argumento y no dejar mensajes ni cambios en partidas normales. Una prueba `-cvg...test` por lanzamiento (comparten el mapa y se pisan). Leer `Player.log` al terminar.
- Antes de publicar: `python -I tools/check-workshop.py` (0 errores) y `python -I tools/make-licenses.py --check`.

## Licencias y material de terceros
- **Código C#, scripts y contenido técnico (datos de los XML, documentación técnica): MIT.** **Arte original, lore, historias, descripciones narrativas y documentación creativa: CC BY-NC-SA 4.0.** Resumen en `LICENSES.md`; los archivos de licencia los genera `tools/make-licenses.py`.
- **Lo derivado de RimWorld (Art Source de Ludeon) y el material de terceros se lista en `LICENSE-THIRD-PARTY.md` y no se le atribuye licencia propia.** El EULA de RimWorld permite usar el Art Source como base de un mod **gratuito**, con el aviso de Ludeon, sin contener una parte sustancial de su arte; no se pueden redistribuir los originales. Si una textura nueva parte de arte de Ludeon, agregarla a `DERIVED` en `tools/make-licenses.py` y a `LICENSE-THIRD-PARTY.md`.
- Los mods deben ser gratuitos y no llevar publicidad ni llamados a donar (EULA). Cada About lleva el aviso de Ludeon.
- No copiar código ni arte de otros mods. Si alguna vez se incorpora código ajeno con licencia libre, conservar su aviso y listarlo en `LICENSE-THIRD-PARTY.md`.
- El arte se declara como hecho con asistencia de IA.

## Commits y colaboración
- Mensajes en inglés, en imperativo y cortos; commits pequeños por tema. Si un asistente de IA participó, terminar el mensaje con su línea `Co-Authored-By`.
- Material de trabajo (capturas, candidatos de arte, intermedios, mods ajenos de referencia) no se versiona: ver `.gitignore`.
