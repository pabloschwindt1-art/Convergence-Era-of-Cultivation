# Cómo agregar una secta al nuevo Sects (plantilla)

La Espada Sin Forma es la secta de referencia. Todo lo común ya está hecho y probado (S0-S2): una secta nueva **solo cambia su
especialidad, su lore y lo que vende**. Copiá los archivos de la Espada, renombrá `FormlessSword` → `<Secta>` y cambiá lo marcado.

## Lo que ya está hecho para todas (no se toca)
| Pieza | Dónde |
|---|---|
| Facción base: llega con la Convergencia, títulos de Royalty instantáneos que no se pierden con la hostilidad, contribución, herencia | `Sect_Bases.xml` → `CVG_SectFactionBase` |
| Discípulos: base de tipo de personaje | `CVG_Sect2KindBase` |
| 4 títulos honorarios: costos 2/6/10/16, 1+1+1+2 puntos de permiso (5 favores) | `CVG_SectTitle1`…`CVG_SectTitle4` |
| 5 favores: recursos, refuerzos, regalo obra maestra, regalo excepcional (a elección), golpe | `CVG_SectFavorResources` … `CVG_SectFavorStrike` |
| Ritual de templado: conducción por el especialista, sujeto, 2 creyentes más, calidad, discurso | `Ideology/Sect_Framework.xml` |
| Especialista: activo desde 1 creyente, ánimo por rol vacío desde 4, etiqueta `CVG_SectSpecialist` | `CVG_SectSpecialistBase` |
| Preceptos de secta (cultivo, alquimia, esencia, demoníaco, duelos, jerarquía, retiro) y estructuras wuxia | `Ideology/Precepts_Sect.xml`, `Memes_Structures_Wuxia.xml` |
| Código: constituciones con grados, ritual de templado, desvío y resistencia a proyectiles, calidad del stock, cuerpos de los discípulos, regalo a elección | Core `Source/Convergence.Core/Sects/` y `Source/Convergence.Factions/SectGift.cs` |

## Lo que define cada secta (checklist)
1. **Constitución** (Core, `Traits_*.xml`): un rasgo con 4 grados (0-3) y `DefModExtension_SpecialBody` (solo nace el grado 0). Efectos
   chicos y crecientes; el grado 3 raro. Lore EN en el def y ES en DefInjected.
2. **Facción** (`<Secta>.xml`): `ParentName="CVG_SectFactionBase"`; nombre, descripción con lore, ícono, `royalTitleTags`
   (`CVG_<Secta>Title`), color, `forcedMemes`, grupos de personajes y `DefModExtension_ConvergenceFaction` (alianzas y enemistades).
3. **Discípulos**: 4 rangos (externo, interno, principal, anciano) + comerciante; reino y etapa, manuales, elementos, `weaponTags`
   (la especialidad), `disallowedBodyTypes` o `forcedBodyType`, y `traitChances` de su constitución (el grado más raro primero).
   **Historias de fondo (obligatorio en toda secta):** la adultez de un miembro sale siempre de la secta; la infancia queda al azar.
   Un archivo `<Secta>_Backstories.xml` con `ParentName` de una base propia (`slot` Adulthood, `shuffleable` false, categoría
   `CVG_Sect_<Secta>`), 1-2 historias por rango con lore y habilidades de la especialidad, y en cada tipo de personaje
   `<fixedAdultBackstories>` (mecanismo de vanilla). Ver `FormlessSword_Backstories.xml`.
4. **Comercio**: un `TraderKindDef` abstracto con `StockGenerator_QualityTag` (armas por `weaponTags`) o `StockGenerator_QualityDefs`
   (lista de objetos, aunque sean solo vendibles por el jugador) para su especialidad (calidades y pesos; nunca Legendario) + materiales
   de Convergence que correspondan; dos hijos (caravana y asentamiento). Una secta que no comercia (la Luna Sangrienta) deja vacíos
   `caravanTraderKinds`, `visitorTraderKinds` y `baseTraderKinds`.
   **Armas**: `weaponTags` que el discípulo pueda usar (las historias exigen `Violent`) y, en el meme, `preferredWeaponClasses`
   (noble y despreciada de Ideology): pueden usar cualquier arma, pero la noble les sube el ánimo y la despreciada se lo baja.
5. **Títulos**: 4 `RoyalTitleDef` (`ParentName="CVG_SectTitle1..4"`) con su `tags` y su lore.
6. **Favores**: 5 `RoyalTitlePermitDef` con su facción, título mínimo y contenido:
   - recursos: `itemsToDrop`;
   - refuerzos y golpe: `pawnKindDef` y `pawnCount`;
   - obra maestra y excepcional: `DefModExtension_SectGift` (`options` con `thing` o `weaponTag`, `quality`, `count`; `playerChooses` para
     elegir: la espada legendaria de la Espada, la píldora a elección del Caldero…).
7. **Ideología** (`Ideology/<Secta>_Ideology.xml`):
   - meme con `preventApparelRequirements`, `requireOne` (preceptos del preset), `requiredRituals` (su templado), `agreeableTraits`
     (su constitución), reglas y `descriptionMaker`;
   - preset (`IdeoPresetDef`);
   - especialista (`ParentName="CVG_SectSpecialistBase"`, `RoleRequirement_HasTrait` con su constitución, habilidad de vanilla);
   - ritual: `RitualOutcomeEffectDef` (`ParentName="CVG_SectTemperOutcomeBase"`) con sus 4 pasos (probabilidad por calidad y costo
     por paso), `RitualPatternDef` (`ParentName="CVG_SectTemperPatternBase"`) y `PreceptDef` (`ParentName="CVG_SectRitualBase"`).
   - En `Precepts_Sect.xml`, sumar el meme nuevo a `associatedMemes`/`conflictingMemes` donde corresponda.
8. **Nombres**: `RulePackDef` de personas (muchas combinaciones: ≥ 25 apellidos × 30 nombres) y de asentamientos.
9. **Traducciones**: Spanish y SpanishLatin para todo lo anterior (DefInjected) + Keyed si hace falta.
10. **Autotest**: sumar un `SectSpec` en `Specs()` de `FactionsSelfTest.cs` (constitución, meme, rol, habilidad, ritual, los 4 rangos,
    regla de armas y de cuerpo, especialidad que vende; `requiresMod`/`specialtyMod` si dependen de otro mod, `extra` para una prueba
    propia) y correr `-cvgfactionstest` en 1.6 (y en 1.7 cuando toque), con todos los módulos y también sin los opcionales.
    **Sectas que dependen de un mod o DLC** (el Hilo Invisible, de Biotech Wuxia): todo en `Mods/<Mod>/` (Defs y Languages) con
    `IfModActive` en `LoadFolders.xml`; si su constitución usa stats de una DLC, va en `Mods/<DLC>/` del Core.
11. **Arte** (después): ícono de facción, meme, especialista y ritual. Mientras tanto, marcadores de íconos existentes.
12. `docs/CHANGELOG.md`.

## Estado de las cinco sectas (2026-10-22, todas ALL OK en 1.6.4871)
| Secta | Constitución | Especialista (habilidad) | Ritual | Armas (noble / despreciada) | Comercio |
|---|---|---|---|---|---|
| Espada Sin Forma | `CVG_Body_Sword` | maestro de espada (orden de combate) | Templado del cuerpo de espada | espadas | espadas Buena-Obra maestra |
| Nueve Templados | `CVG_Body_Steel` | maestro del templado (trance berserker) | Refinamiento de los nueve templados | sin armas (contundente / a distancia) | armaduras |
| Caldero Celestial | `CVG_Body_PureYang` | maestro alquimista (orden de producción) | Bendición del caldero | armas espirituales y armas simples | armas espirituales (con Alchemy), píldoras |
| Luna Sangrienta | `CVG_Body_BloodMoon` | oficiante de la luna sangrienta (impulso inmunitario) | Juramento de sangre | hojas (perforante / larga distancia) | no comercia |
| Hilo Invisible (Biotech Wuxia) | `CVG_Body_ThousandThreads` (Biotech) | maestro titiritero (impulso de trabajo) | Vinculación de hilos espirituales | armas de fuego (ligera / contundente) | atuendo de titiritero, materiales de marioneta |

Relaciones: la Luna Sangrienta es enemiga natural (neutral con el Camino Demoníaco) y hostil con la Espada y los Nueve Templados.
Pendiente: arte definitivo (7 íconos por secta), integración con Stories y prueba en 1.7.

## Decisiones comunes (del usuario, valen para todas)
- Títulos permanentes; la hostilidad solo suspende los favores.
- Sin prendas obligatorias en la ideología.
- Especialista con requisito de constitución; ánimo por rol vacío desde 4 creyentes.
- Ritual: un grado por ceremonia; si falla no se pierde grado, solo los materiales; requiere 2 miembros más.
- El comercio de la especialidad tiene calidad controlada; el ítem excepcional es un favor, no se vende.
