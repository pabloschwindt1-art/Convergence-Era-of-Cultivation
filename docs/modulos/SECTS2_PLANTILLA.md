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
| Código: constituciones con grados, ritual de templado, desvío y resistencia a proyectiles, calidad del stock, cuerpos de los discípulos, regalo a elección | Core `Source/Convergence.Core/Sects/` y `Source/Convergence.Sects2/SectGift.cs` |

## Lo que define cada secta (checklist)
1. **Constitución** (Core, `Traits_*.xml`): un rasgo con 4 grados (0-3) y `DefModExtension_SpecialBody` (solo nace el grado 0). Efectos
   chicos y crecientes; el grado 3 raro. Lore EN en el def y ES en DefInjected.
2. **Facción** (`<Secta>.xml`): `ParentName="CVG_SectFactionBase"`; nombre, descripción con lore, ícono, `royalTitleTags`
   (`CVG_<Secta>Title`), color, `forcedMemes`, grupos de personajes y `DefModExtension_ConvergenceFaction` (alianzas y enemistades).
3. **Discípulos**: 4 rangos (externo, interno, principal, anciano) + comerciante; reino y etapa, manuales, elementos, `weaponTags`
   (la especialidad), `disallowedBodyTypes` o `forcedBodyType`, y `traitChances` de su constitución (el grado más raro primero).
4. **Comercio**: un `TraderKindDef` abstracto con `StockGenerator_QualityTag` para su especialidad (calidades y pesos; nunca Legendario si así
   se decide) + materiales de Convergence que correspondan; dos hijos (caravana y asentamiento).
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
10. **Autotest**: copiar `TestSwordSect` del autotest del mod y cambiar los nombres; correr `-cvgsects2test` en 1.6 (y en 1.7 cuando toque).
11. **Arte** (después): ícono de facción, meme, especialista y ritual. Mientras tanto, marcadores de íconos existentes.
12. `docs/CHANGELOG.md`.

## Decisiones comunes (del usuario, valen para todas)
- Títulos permanentes; la hostilidad solo suspende los favores.
- Sin prendas obligatorias en la ideología.
- Especialista con requisito de constitución; ánimo por rol vacío desde 4 creyentes.
- Ritual: un grado por ceremonia; si falla no se pierde grado, solo los materiales; requiere 2 miembros más.
- El comercio de la especialidad tiene calidad controlada; el ítem excepcional es un favor, no se vende.
