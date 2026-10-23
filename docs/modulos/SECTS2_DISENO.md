# Nuevo Sects: diseño de las cinco sectas (borrador del usuario con ChatGPT, 2026-10-22)

> **Lo que sumé yo:** las secciones "Nuestro lore y lo que ya existe" al final de cada secta, y el §6 "Cómo se arma con Royalty". Los
> datos salen de `docs/lore/SECTAS.md` y de los archivos reales del repo.

Este documento es **material de diseño, no son órdenes**. Lo que manda son las decisiones del usuario (ver `SECTS2_BASE.md` §5). La única
diferencia con el borrador está en Royalty: el nuevo Sects **requiere Royalty** (decisión del usuario), aunque el borrador diga "sin
imponer Royalty". Estado (2026-10-22): las cinco sectas están implementadas con la plantilla y probadas en 1.6 (tabla en `SECTS2_PLANTILLA.md`);
Hilo Invisible, Caldero Celestial y Luna Sangrienta las completé yo a partir de este diseño y del lore, a revisar por el usuario.

## Framework común
- **Roles de Ideology:** como máximo tres. Líder de Secta, Gran Anciano (función moral) y un especialista propio que se desbloquea
  primero. Sin Ideology, las facciones se juegan igual.
- **Títulos:** Amigo de la Secta → Invitado de Honor → Protector Honorario → Anciano Invitado (客卿长老). El jugador sigue siendo
  independiente.
- **Cinco favores por facción:** refuerzos, recursos, equipo Obra maestra, equipo Legendario, ataque a un objetivo válido. El contenido
  cambia según la especialidad.
- **Identidad de cada secta:** combate, tipos de cuerpo, comercio, color, ritual y personajes propios. Respetan el límite del Core de
  reino enemigo +1.
- **Stories:** mantener y adaptar los 47 eventos y los escenarios. No inventar eventos de relleno.
- **Técnicas:** primero las del Core. Nada de daño arbitrario ni técnicas exclusivas sin justificar.
- **Estética:** por ahora un color por facción y ropa de Apparel; los atuendos propios vienen después.
- **Lore:** 2-3 frases en inglés y en español. Revisar los nombres chinos nuevos antes de publicar.

## 1. Secta de la Espada Sin Forma — Formless Sword Sect (无形剑宗, Wúxíng Jiànzōng)
Sustituye conceptualmente al Pico Azur. Filosofía: «la espada es un instrumento; la verdadera espada está en la intención» (Jian Yi, 剑意).
- **Combate:** solo cuerpo a cuerpo y con espadas; técnicas de espada a distancia permitidas (decisión del usuario). Sin Hulk ni Tiny.
- **Constitución:** Cuerpo de Espada Común, Superior y Superior+; Celestial aparte (4.º, decisión del usuario). Común frecuente, Superior
  poco frecuente, Celestial excepcional.
- **Ideology:** Líder, Gran Anciano y Maestro de Espadas (requiere Cuerpo de Espada de cualquier grado).
- **Inicio:** con su ideología, al menos un colono tiene el Cuerpo de Espada Común. No a todos, y no por convertirse.
- **Ritual:** Templado del Cuerpo de Espada, lo dirige el Maestro de Espadas. Sube un grado; el efecto es permanente.
- **Comercio:** su stock habitual más espadas Excelente o mejor; Obra maestra y Legendaria, raras.
- **Favores:** refuerzos espadachines; piedras espirituales, hierbas y materiales; espada Obra maestra; espada Legendaria; ataque de
  espadachines.
- **Lore provisional:** los maestros de la Espada Sin Forma enseñan que el acero puede quebrarse, pero la intención verdadera persiste.
  Tras la Convergencia buscan reconstruir las enseñanzas perdidas y hallar discípulos capaces de comprender un corte antes de ejecutarlo.

- **Nuestro lore y lo que ya existe (para la Espada Sin Forma):**
  - Hereda la historia del Pico Azur (`SECTAS.md` §1.1): fundada hace 4000 años por el Santo de la Espada **Qing Feng**, que partió una
    montaña de un tajo; el **Cementerio de Espadas** (剑冢); las **Espadas del Cielo Roto** (破天剑), forjadas con cascarones de máquina
    de la Calamidad de Hierro, que explican por qué no le teme a la tecnología. Lema reutilizable: "La espada no es buena ni mala. Lo
    es la mano".
  - Personajes para eventos: **Lin Wuchen** (maestro de secta), **Shen Ruoxue** (anciana de espada de hielo), **Gu Hengyue** (anciano de
    disciplina) y **Yan Zhao** (prodigio arrogante, rival de por vida en torneos).
  - Técnicas del Core que encajan: barrido de qi de espada, filo del corazón, **intención de espada** (calza con Jian Yi), espada voladora
    y el núcleo de metal irrompible. Manuales: GoldenEdge (metal) y SwordHeart.
  - Constitución: el Core ya tiene `CVG_Body_Sword` (0,8 %) y `CVG_Body_CelestialSword` (0,4 %); el cuerpo de sable está en Traits &
    Destinies.
  - Eventos de Stories del Pico: VisitAzurePeak, MissionAzureTournament, duelos y enemistades (Feud*), torneo de sectas.
## 2. Secta de los Nueve Templados — Sect of Nine Temperings (九炼宗, Jiǔliàn Zōng)
Filosofía: refinar carne, sangre y huesos con qi hasta que el cuerpo sea el arma y la armadura.
- **Combate:** solo cuerpo a cuerpo; golpes sin armas, técnicas corporales y armas contundentes. **Todos sus combatientes tienen cuerpo
  Hulk**, sea cual sea su reino. Verificar la compatibilidad con los linajes de Races y con la ropa.
- **Progresión:** el camino corporal (sangre-qi) del Core, sin otra escala de reinos.
- **Ideology:** Líder, Gran Anciano y Maestro del Templado.
- **Inicio:** con su ideología, al menos un colono empieza como cultivador corporal básico. Pendiente: ¿también Hulk?
- **Ritual:** Refinamiento de los Nueve Templados. Ayuda a preparar el siguiente avance corporal con sangre de bestia, sin regalar reinos.
- **Comercio:** sangre de bestia, recursos de templado, preparados de recuperación y armaduras Excelente o mejor.
- **Favores:** refuerzos corporales Hulk; recursos de templado; armadura Obra maestra; armadura Legendaria; ataque de cultivadores
  corporales.
- **Relaciones:** rivalidad filosófica con la Espada Sin Forma (cuerpo contra arma), sin hostilidad automática.
- **Lore provisional:** afirman que ni una espada espiritual sustituye un cuerpo forjado con dolor y perseverancia. Buscan bestias
  poderosas y materiales raros, y respetan la fuerza venga de la tecnología o del qi.

- **Nuestro lore y lo que ya existe (para los Nueve Templados):**
  - El lore del **Templo del Loto Silencioso** (`SECTAS.md` §1.2) ya es una secta corporal: fundado por la primera mujer que alcanzó el
    **Cuerpo Vajra**; doctrina "Camino del Cuerpo" (体道); el monje guardián **Kongming** tiene Cuerpo Vajra y derriba de un puño. Sirve
    de base, y la disciplina del templo encaja con "el dolor templa". Lo que no encaja (sanar a todos, no vender armas) se puede dejar
    para otra facción o adaptar.
  - Técnicas del Core: camisa de hierro, fuerza de dragón y elefante, huesos irrompibles, piel de campana dorada, cuerpo de montaña.
    Manuales: VajraBody y MountainHeart.
  - Camino del cuerpo del Core: reinos del cuerpo (templado con sangre-qi), carne y sangre de bestia espiritual, muñeco de madera y
    equipo de entrenamiento. Cuerpos de Traits & Destinies: Vajra, piel de hierro, hueso de jade de hielo; acero en el Core.
  - Meme ya hecho: **BodyWay** (`CVG_Meme_BodyWay`), con su preset.
## 3. Secta del Hilo Invisible — Invisible Thread Sect (无形丝宗, Wúxíng Sīzōng, provisional) — HECHA (2026-10-22; ver SECTS2_PLANTILLA.md)
Marionetas espirituales; el poder está en la coordinación y el control.
- **Combate:** maestros y controladores con marionetas del contenido existente (BiotechWuxia: **solo con Biotech**).
- **Ideology:** Líder, Gran Anciano y Maestro Titiritero.
- **Ritual propuesto:** Vinculación de Hilos Espirituales, solo si el sistema actual de marionetas soporta el vínculo.
- **Comercio:** componentes, núcleos, marionetas, talismanes.
- **Favores:** refuerzos de marionetas; recursos de construcción; marioneta o equipo Obra maestra y Legendario (si hay calidad);
  incursión de marionetas.
- **Validar:** marionetas reales, control, calidad, dependencias con Formations y Races.
- **Lore provisional:** una voluntad serena puede guiar cien cuerpos sin arriesgar una sola vida.

- **Nuestro lore y lo que ya existe (para el Hilo Invisible):**
  - Las marionetas existen en **BiotechWuxia** (requiere Biotech): títeres de madera, de bronce y de jade, el hilo del corazón, el taller
    de marionetas, el cargador espiritual y la investigación (marionetería → hilos de guerra → hilos de acero → hilos de jade). El vínculo
    "hilo del maestro" (`CVG_PuppetBond`) y el rebote del hilo (`CVG_PuppetBacklash`) son la base natural del ritual de vinculación.
  - Ya existen el meme **PuppetPath** y el rol de **maestro titiritero**.
  - Lore de fondo: Yan Shi y su autómata (citado en la investigación de marionetería).
  - Las marionetas no tienen calidad (son personajes); los favores de Obra maestra y Legendario serían equipo de marioneta o del
    titiritero.
## 4. Pabellón del Caldero Celestial — Heavenly Cauldron Pavilion — HECHA (2026-10-22; ver SECTS2_PLANTILLA.md)
Alquimia, forja espiritual y comercio. El nombre chino canónico es el del lore (天鼎阁, Tiāndǐng Gé); el borrador propone 天炉阁.
- **Combate:** técnicas de fuego y madera; sin restricción a cuerpo a cuerpo.
- **Ideology:** Líder, Gran Anciano y Maestro Alquimista.
- **Ritual propuesto:** Bendición del Caldero, un bono temporal de elaboración o investigación alquímica.
- **Comercio:** el actual, reforzado: píldoras, hierbas, jade, piedra espiritual, tablillas.
- **Favores:** alquimistas de escolta; hierbas y materiales; artefacto Obra maestra y Legendario; ataque del Pabellón.
- **Relaciones:** neutralidad comercial.
- **Lore provisional:** los calderos del Pabellón arden para nobles y errantes por igual.

- **Nuestro lore y lo que ya existe (para el Caldero):**
  - Lore completo en `SECTAS.md` §1.3: gremio neutral por tratado ("nadie mataría al proveedor de todos"), fuego alquímico heredado del
    **Fénix**, compra tecnología del Borde con entusiasmo. Lema: "Todo tiene precio. El Dao, también". Figuras: **Qian Wanli** (maestro
    del Pabellón), **Mo Qinghua** (gran alquimista), **Hu Xiaoman** (subastadora, sangre de zorro) y **Ning Yuhuo** (alquimista de fuego,
    sangre de fénix).
  - Su nivel tecnológico es Industrial (el único), y tiene una nave voladora comerciante (`CVG_Trader_CauldronShip`).
  - Contenido de Alchemy & Artifacts: horno de alquimia y forja espiritual con niveles, píldoras (fundación, templado, purificación,
    recuperación de qi), hierbas espirituales, armas con grado espiritual.
  - Eventos de Stories: VisitCauldron, MissionCauldronCores, PavilionTaxmen, CauldronCreditors.
## 5. Culto de la Luna Sangrienta — Blood Moon Cult (血月教, Xuèyuè Jiào) — HECHA (2026-10-22; ver SECTS2_PLANTILLA.md)
Cultivo heterodoxo y técnicas demoníacas; tiene reglas internas, no es solo una facción hostil genérica.
- **Ideology:** Líder, Gran Anciano y Oficiante de la Luna Sangrienta (nombre provisional).
- **Ritual propuesto:** Juramento de Sangre, un pacto con costos y consecuencias narrativas.
- **Comercio:** excepcional; hoy no comercia, y eso sigue así hasta que el usuario decida otra cosa.
- **Favores:** refuerzos demoníacos; recursos de sangre; artefactos Obra maestra y Legendario; ataque de adeptos. Dependen de la
  diplomacia (`naturalEnemy`, `neutralToPlayerMemes`).
- **Stories:** preservar los pactos demoníacos, las deudas de sangre y las rivalidades.
- **Lore provisional:** buscaron respuestas donde nadie más se atrevía, y la Convergencia les ofrece un mundo nuevo donde negociar su lugar.
- **Nuestro lore y lo que ya existe (para la Luna Sangrienta):**
  - Lore en `SECTAS.md` §1.5: escisión de la secta **Heavenly Origin** (天元宗); su fundador **Xue Jiuyou** (el Venerable) descubrió que
    la sangre y el alma concentran el qi; "No son locos: son pragmáticos". Figuras: **Su Lanyin** (santa de sangre, la cara amable) y
    **Shi Gu** (anciano de almas). Pista oculta: la "Luna" sería en parte la señal de una máquina de la Calamidad. Lema: "El Cielo es
    una jaula. Rómpela con lo que tienes en las venas".
  - Contenido existente: en BiotechWuxia (requiere Biotech), el gen y xenotipo **demonio de sangre**, las técnicas drenaje de sangre y
    armadura de sangre, el estanque de sangre y la píldora de esencia de sangre. Meme **DemonicWay** y su preset.
  - El Core ya la hace neutral si tu ideología tiene el Camino Demoníaco (`neutralToPlayerMemes`).
  - Eventos de Stories: DemonBargain, BloodPactDue, WoundedEnemyBegs.

## 6. Cómo se arma con Royalty (lo que sumé yo; detalle en `SECTS2_BASE.md`)
- Los **4 títulos** de cada secta son `RoyalTitleDef` con la etiqueta de la secta y otorgamiento instantáneo. El honor se llama
  **contribución de secta** (贡献) y se gana con las misiones vanilla de esa facción, con los eventos de Stories y con favores a la secta.
- Los **5 favores** son permisos de Royalty: refuerzos (`CallAid` con los discípulos de la secta), recursos (`DropResources` con su lista),
  Obra maestra y Legendario (permiso propio con calidad fija y enfriamiento largo) y ataque (permiso propio con efecto wuxia; por ejemplo,
  lluvia de espadas en la Espada Sin Forma).
- El título es **del colono** que lo recibe; gastar contribución no lo baja.
- Las facciones que dependen de una DLC (el Hilo Invisible, de Biotech) solo existen si está la DLC.
- **Sectas que quedan afuera de las cinco:** el Clan del Dragón Antiguo (atado al linaje dragón de Races, a un escenario y a 3 eventos) y
  los Cultivadores Errantes (los "forasteros" del mundo, que sostienen muchos eventos). En el mod nuevo propongo mantener a los Errantes
  como facción sin títulos y decidir más adelante qué pasa con el Dragón.