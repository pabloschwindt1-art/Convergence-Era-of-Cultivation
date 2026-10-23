# Nuevo Sects: investigación de Royalty y plan de la base común

Estado: **propuesta** (2026-10-22). Todavía no hay código. Decisiones del usuario: mod paralelo nuevo (el Sects actual no se toca y no
entra en las pruebas), **requiere Royalty**, cinco especialidades (espada, cuerpo, marionetas, alquimia, demoníaca), el rango no baja al
usar favores y las constituciones son un sistema común para todas las sectas. Inventario del Sects actual: `SECTS_INVENTARIO.md`.

---

## 1. Royalty con facciones propias: lo verificado en el código del juego

Fuente: `Assembly-CSharp` de RimWorld 1.6.4871 decompilado (`Pawn_RoyaltyTracker`, `FactionDef`, `RoyalTitleDef`, `RoyalTitlePermitDef`,
los workers de permisos, `RewardsGenerator`). Busqué cada uso de `Faction.OfEmpire`: los 20 archivos que lo nombran son misiones y
generación de mapa propias del Imperio; **ninguno afecta a títulos, honor ni permisos de otra facción**.

| Pregunta | Respuesta | Evidencia |
|---|---|---|
| ¿Una facción propia puede dar títulos? | **Sí.** Basta con `royalTitleTags` en el `FactionDef` y `RoyalTitleDef` con esa etiqueta. | `FactionDef.RoyalTitlesAwardableInSeniorityOrderForReading` filtra por etiquetas, sin mirar qué facción es |
| ¿Varias facciones a la vez? | **Sí.** El honor y los títulos se guardan por facción en cada personaje. | `Pawn_RoyaltyTracker`: `Dictionary<Faction,int> favor`, `List<RoyalTitle> titles` (cada uno con su facción) |
| ¿Cómo se gana honor? | Las **misiones vanilla** de una facción con títulos lo dan como recompensa, igual que el Imperio. También se puede dar desde código o eventos (`GainFavor`). | `RewardsGenerator`: `allowRoyalFavor && giverFaction.def.HasRoyalTitles` |
| ¿Hace falta la ceremonia de investidura del Imperio? | **No.** Cada título define cómo se otorga. Vanilla trae `RoyalTitleAwardWorker_Instant`, que lo da al instante con una carta. Más adelante se puede hacer una ceremonia wuxia propia. | `RoyalTitleDef.awardWorkerClass` |
| ¿Gastar honor baja el título? | **No.** El título queda; solo se pierde con `ReduceTitle` (eventos puntuales). Es justo lo que pediste. | `TryRemoveFavor` / `SetFavor` no tocan el título |
| ¿Los permisos sirven para otra facción? | **Sí.** Cada permiso tiene su `faction`, su título mínimo, su enfriamiento en días y su costo en honor. Los workers reciben la facción por parámetro. | `RoyalTitlePermitDef`, `RoyalTitlePermitWorker_*` |
| ¿Se usan sin consola de comunicaciones? | **Sí.** Se piden desde el personaje que tiene el título, con un menú propio. No hace falta electricidad, así que resuelve el problema de la consola. | `GetRoyalAidOptions(map, pawn, faction)` |
| ¿Se ven en la interfaz? | **Sí.** La ficha de permisos lista todas las facciones visibles que tienen títulos. | `PermitsCardUtility` recorre `AllFactionsVisible` |
| ¿Obligaciones de los títulos? | Son **todas opcionales por título**: sala del trono, dormitorio, ropa, comida, trabajos prohibidos, nivel de psylink, decretos. Nuestros títulos no traen ninguna. | campos de `RoyalTitleDef` |

**Permisos que vanilla ya trae y sirven tal cual:** pedir tropas (`CallAid`, con el tipo de personaje y la cantidad de la facción),
pedir recursos (`DropResources`, con lista de objetos), bombardeo (`OrbitalStrike`), peones (`CallLaborers`) y lanzadera (`CallShuttle`).

**Conclusión:** **sí, se puede.** Los 4 rangos honorarios pasan a ser **títulos de Royalty de cada secta**, los favores pasan a ser
**permisos**, y el honor de cada secta se puede llamar "contribución" (贡献, la moneda clásica de las sectas xianxia). Es la versión wuxia
de Royalty, sin inventar un sistema paralelo.

**Cambio de idea respecto al diseño de ChatGPT:** en Royalty el título es de **un colono**, no de la colonia. Tu colono pasa a ser
"Anciano Invitado del Pico", y él pide los favores. Me parece más xianxia (el jugador sigue siendo independiente y cada discípulo tiene su
reputación), pero conviene que lo confirmes.

---

## 2. El mod nuevo

- Carpeta `Mods/Convergence.Sects2/` (nombre y `packageId` provisorios: decidilos vos) + `Source/Convergence.Sects2/` para su código.
- Depende de **Harmony, Core y Royalty**. Opcionales: Ideology (roles y rituales, en `Mods/Ideology`), Biotech (la secta de marionetas
  solo existe con Biotech; en `Mods/Biotech`), Races (linajes), Apparel (ropa con el color de la secta), Alchemy (píldoras y hierbas en el
  comercio).
- Marcado **incompatible** con el Sects actual (`incompatibleWith`), así nadie activa los dos.
- Nombres internos nuevos y limpios (`CVG_Faction_FormlessSword`, `CVG_Title_FormlessSword_1`…). Los mods publicados están ocultos, así
  que no hay partidas de jugadores que cuidar.

---

## 3. La base común (una vez, configurable por XML para cada secta)

| # | Pieza | Sistema que reutiliza | Dónde | Código |
|---|---|---|---|---|
| A | **Títulos honorarios** (Amigo, Invitado de Honor, Protector, Anciano Invitado) por secta | `RoyalTitleDef` + `royalTitleTags`, otorgamiento instantáneo | Sects2 (XML) | No |
| B | **Contribución** (honor) | Honor de Royalty (`royalFavorLabel`, ícono) | Sects2 (XML) | No |
| C | **Favores**: refuerzos y recursos | Permisos vanilla `CallAid` y `DropResources` | Sects2 (XML) | No |
| D | **Favores**: equipo Obra maestra o Legendario | Worker propio que hereda de `DropResources` y fija la calidad | Sects2 | Chico |
| E | **Favor de ataque** (p. ej. "lluvia de espadas") | Worker propio basado en `OrbitalStrike`/`Targeted`, con efecto wuxia | Sects2 | Medio (último) |
| F | **Constituciones** con grados (Cuerpo de Espada 1-3, cuerpo Vajra…) | Rasgos con grados de vanilla + el `DefModExtension_SpecialBody` que ya existe | Core (XML) | No |
| G | **Ritual de templado genérico**: sube un grado según la calidad del ritual; costo y probabilidades en XML | Rituales de Ideology (igual que admisión y ascensión, que ya existen) | Core (código) + Sects2 (XML) | Chico |
| H | **Rol de especialista con requisito de constitución** | `RoleRequirement` de Ideology + uno propio "tiene el rasgo X" | Core | Chico |
| I | **Constitución inicial**: con la ideología de la secta, al menos un colono empieza con el grado 1 | Extensión en el `MemeDef` + componente al iniciar partida | Core | Chico |
| J | **Discípulos con constitución** (grado 1 frecuente, 2 raro, celestial rarísimo) | `DefModExtension_CultivatorPawnKind`, al que se suma `traitChances` | Core | Chico |
| K | **Solo armas cuerpo a cuerpo** | `weaponTags` del tipo de personaje | Sects2 (XML) | No |
| L | **Sin cuerpos Hulk/Tiny** en una secta | Campo nuevo en la misma extensión del tipo de personaje | Core | Chico |
| M | **Desvío de proyectiles** (espadachines) y aguante (cuerpo) | `StatDef` nuevo + un parche donde el Core ya intercepta el daño | Core | Chico |
| N | **Comercio con calidad mínima** (espadas Excelente o mejor) | `StockGenerator` propio | Core | Chico |
| O | **3 roles**: líder, gran anciano y especialista | Líder y guía moral de vanilla renombrados por meme + 1 especialista | Sects2/Mods/Ideology (XML) | No |
| P | **Facción solo con su DLC** (marionetas → Biotech) | `LoadFolders` con `IfModActive` | Sects2 | No |
| Q | **Stories**: los eventos de sectas apuntan también a las sectas nuevas | `MayRequire` en los eventos | Stories (XML) | No |

Lo de Royalty (A-E) vive en el mod nuevo porque ese mod exige la DLC. Lo genérico (F-N) va al Core para que otros módulos lo usen
(p. ej. la secta corporal o un futuro módulo de cuerpos); el Core no pasa a necesitar ninguna DLC.

---

## 4. Orden de trabajo

**S0 hecha (2026-10-22):** `Mods/Convergence.Sects2` + `Source/Convergence.Sects2`, autotest `-cvgsects2test`, **ALL OK en 1.6.4871** sin
errores en el log (la prueba en 1.7 queda pendiente: Steam está en 1.6). Lo que aprendimos en el juego:
- Royalty exige `changeHeirQuestPoints` en todo título otorgable, aunque no se herede.
- Al otorgar un título, Royalty siempre busca heredero: la facción necesita `royalTitleInheritanceWorkerClass` (y relaciones), o se rompe.
- El mundo del `-quicktest` no trae las facciones opcionales; el autotest dispara la Convergencia para que llegue la secta.
- Confirmado en juego: título al instante sin ceremonia del Imperio, 1 punto de permiso por título, los dos favores (cápsula de recursos y
  3 discípulos por cápsula), enfriamiento, pago con contribución durante el enfriamiento y título que no baja al gastar.

1. **S0, prueba de concepto (lo más importante):** esqueleto del mod con **una** secta en XML: 4 títulos, contribución, permisos de
   refuerzos y recursos. Probar en el juego: dar contribución, subir de título al instante, usar los permisos, enfriamiento y que el título
   no baje al gastar. Si algo de Royalty no anda como dice el código, se detecta acá, antes de construir encima.
2. **S1, base común del Core:** piezas F-N, cada una con su autotest.
3. **S2, Espada Sin Forma completa:** facción, discípulos, constitución de 3 grados + celestial, ritual, especialista, comercio y los 5
   favores, con lore en inglés y en español.
4. **S3, validación:** 1.6 y 1.7, con y sin Ideology y Biotech, misiones vanilla que den contribución y eventos de Stories.
5. Después, una secta por vez, casi todo XML.

---

## 5. Decisiones que te tocan

1. **Nombre del mod** (y su `packageId`): ¿"Convergence: Sects" con un `packageId` nuevo, o "Sects 2.0"?
2. **El título es del colono, no de la colonia** (ver §1). ¿Te parece bien?
3. **Nombre del honor:** propongo "contribución de secta" (Sect Contribution).
4. **¿Cómo llegan los refuerzos?** Vanilla los manda en cápsulas desde el cielo. Para empezar sirve; más adelante podemos hacer que
   "bajen volando en espadas".
5. **Cuerpo de Espada en el Core:** hoy existen `CVG_Body_Sword` y `CVG_Body_CelestialSword` como cuerpos especiales. Propongo
   reconvertir `CVG_Body_Sword` en el rasgo de 3 grados y conservar el celestial como el 4.º aparte.
6. **Ideology sigue siendo opcional** en el mod nuevo (sin ella no hay roles ni rituales, y la constitución se sube por otra vía, o no se
   sube). ¿O también la hacemos obligatoria?

## 6. Riesgos

- **1.7:** Royalty no debería cambiar, pero lo confirmo compilando contra la 1.7 en la etapa S0.
- **Misiones del Imperio:** algunas usan `Faction.OfEmpire` a mano. No nos afectan, pero tampoco van a aparecer para nuestras sectas
  (por ejemplo, el recaudador de tributos). Si queremos equivalentes, son eventos propios de Stories.
- **Equilibrio del honor:** las misiones vanilla dan bastante honor. Los costos de títulos y permisos se calibran en S0 y quedan en XML.
- **Legendario:** solo con el título 4, enfriamiento largo (unos 60 días) y costo alto en contribución.
