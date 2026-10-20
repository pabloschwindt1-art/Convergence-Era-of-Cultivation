# Sectas sobre Ideología (versión wuxia del sistema de ideologías)

Pedido del usuario (2026-10-15): "no creo que haya mejor lugar donde apoyarnos que en Ideología para las sectas: doctrinas,
memes, etc.". Principio n.º 2: cada pieza de Ideology, su versión wuxia. Todo en `Mods/Convergence.Sects/Mods/Ideology`
(sin Ideology, las sectas siguen funcionando como facciones). Las sectas NO son anti-tecnología: eso lo decide el jugador
con los preceptos de tecnología vanilla.

| Ideology vanilla | Versión wuxia | Estado |
|---|---|---|
| Memes (estructura y temáticos) | **Doctrinas de secta**: Camino de la Espada, Senda del Loto, Camino del Caldero, Sangre del Dragón, Camino Demoníaco, Senda Errante ✅; faltan Camino del Cuerpo, Senda de las Marionetas (con Biotech) y Camino del Trueno ✅ | ✅ |
| Preceptos (issues) | **Preceptos de secta**: Cultivo (obligatorio / venerado / libre / mal visto), Píldoras y alquimia, Extracción de esencias (prohibida / aborrecible / aceptada; IMPLANTES §2b), Artes demoníacas, Duelos entre discípulos, Jerarquía (ancianos mandan / igualdad), Retiro de cultivo (aislamiento venerado) | ✅ 7 asuntos, 20 preceptos (ver Implementación) |
| Roles (líder, moral, especialistas) | **Maestro de la secta** (líder), **Gran Anciano** (moral), especialistas: Anciano de la Espada, Maestro Alquimista, Maestro de Formaciones, Guardián de las Escrituras, Maestro de Marionetas | ✅ |
| Rituales | **Ceremonia de admisión de discípulos**, **Torneo de la secta** (duelos), **Ceremonia de ascensión** (tras un avance de reino: celebración), **Funeral del Dao** | ✅ |
| Reliquias | **Tesoro de la secta** (espada antigua, jade del fundador): la reliquia vanilla con nombre y lore de secta | ✅ espada y jade del fundador, sello de la secta |
| Estilos (ropa, edificios, iconos) | **Estilo de secta**: túnicas de rango (Apparel ✅) como estilo; edificios con estética wuxia; iconos y colores de secta | 🔄 |
| Edificios de ideología (altar, ideograma) | **Salón ancestral / altar de la secta** (edificio de ritual) y **estela de las escrituras** | ✅ altar ancestral y estela de las escrituras |
| Presets de ideología | Una ideología lista por cada secta de LORE.md (Pabellón de la Espada, Templo del Loto Silencioso, Pabellón del Caldero Celestial, Clan del Dragón Antiguo, Culto de la Luna Sangrienta…) para facciones y para el jugador | ✅ 9 presets (los de las facciones ya fuerzan su meme) |

## Prioridad
1. Preceptos de cultivo + extracción de esencias + roles (lo que más se nota en el día a día).
2. Rituales de admisión y ascensión + altar de la secta.
3. Presets por secta + memes que faltan + reliquias.

## Implementación (prioridades 1 y 2, 2026-10-15)
- **Core** (`Source/Convergence.Core/Ideology/`): `SectEvents` registra `HistoryEventDef` propios (cultivó, ascendió, retiro, píldora, arte demoníaco, esencia extraída, duelo) desde el cultivo, el avance, las píldoras `CVG_*`, las técnicas con `DefModExtension_DemonicArt` y el torneo; `SectRituals` aporta los disparadores `RitualObligationTrigger_Admission/Ascension` (nacen al unirse un discípulo o al subir de reino). Sin Ideología no se registra nada.
- **Mods/Convergence.Sects/Mods/Ideology**: `Precepts_Sect.xml` (7 `IssueDef`, 20 preceptos con pensamientos propios que reaccionan a esos eventos; el generador está en el repo de scripts del autor), `Roles_Sect.xml` (Maestro de la secta, Gran Anciano, Anciano de la Espada, Maestro Alquimista, Maestro de Formaciones, Guardián de las Escrituras y, con Biotech, Maestro de Marionetas; solo en ideoligiones con doctrina de secta), `Rituals_Sect.xml` (ceremonia de admisión, ceremonia de ascensión y torneo, sobre el patrón de celebración vanilla; el altar ancestral da +0,25 de calidad), `Altar_Sect.xml` (altar ancestral, 3×1, +1 qi de feng shui) y `Patches/Altar_Sect.xml` (el altar entra en el precepto `IdeoBuilding`).
- **Efecto de cada asunto**: cultivo y retiro (ánimo al cultivar/terminar un retiro), píldoras, esencias y artes demoníacas (ánimo propio y opinión de quien lo sabe), duelos (ánimo tras el torneo), jerarquía (los ancianos mandan: todos celebran un ascenso; iguales: solo el que sube).
- Autotest: `-cvgsectideotest` (con Ideología): asuntos, preceptos, roles, altar, pensamientos por evento y obligaciones de ascensión.
- Pendiente (prioridad 3): presets por secta, memes que faltan, reliquias, funeral del Dao, estela de las escrituras; disparar `CVG_ExtractedEssence` desde la extracción de esencias cuando exista.

### Prioridad 3 (2026-10-15)
- **Memes**: Camino del Cuerpo, Senda de las Marionetas (`MayRequire` Biotech) y Camino del Trueno (`Memes_Sects2.xml`). Los roles y rituales de secta los aceptan (salvo el Maestro de Marionetas, que va con su propia lista).
- **Presets** (`Presets_Sect.xml`): uno por doctrina, categoría "Strong", cada uno con su meme de secta y uno estructural (Leal, Colectivista, Individualista, Supremacista, Asaltante). Las facciones de las sectas ya fuerzan su meme.
- **Reliquias** (`Relics_Sect.xml`): espada del fundador, jade del fundador y sello de la secta, sobre `RelicInertBase` (aparecen como reliquias de cualquier ideoligión). Arte provisional: sprites vanilla teñidos.
- **Funeral del Dao** (`Funeral_Sect.xml`): precepto `FuneralBase` con el comportamiento de funeral vanilla.
- **Estela de las escrituras** (`Stele_Sect.xml`): foco de ritual de 1×1 con +1 qi, entra en el precepto de edificios.
- Pendiente: presets con preceptos preseleccionados (hoy fijan solo los memes), arte propio.
