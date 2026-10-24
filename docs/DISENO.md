# Convergence: Era of Cultivation — Documento de diseño (v0)

Referencia: **Tale of Immortal** (鬼谷八荒) y el género **xianxia / wuxia**, fusionado con la tecnología de RimWorld.
Nombre del mod: **Convergence: Era of Cultivation**. Prefijo de defName: `CVG_`, namespace C#: `Convergence`.

---

## 1. ¿Necesitamos un framework propio?

**Sí, pero es nuestro módulo Core, no un mod aparte.** Los mods de referencia demuestran los dos extremos:
KotOR depende de ~15 frameworks ajenos y se rompe por todos lados; Isekai lo hace todo en su propia DLL.

| Dependencia | Decisión | Por qué |
|---|---|---|
| **Harmony** | Obligatoria | Estándar; sin ella no hay C# serio |
| **Vanilla Expanded Framework** | **No obligatoria** | Grande y cambia seguido; si algo suyo nos sirve, integración opcional (`MayRequire`) |
| XML Extensions, HAR, EBSG, Big and Small… | No | Lo que necesitemos lo escribimos en Core |
| **Todas las DLC** (Royalty, Ideology, Biotech, Anomaly, Odyssey) | **Se usan todas** (decisión del usuario 2026-10-15) | Diseñamos sin limitarnos. Cada función de una DLC va en su carpeta condicional (`Mods/<DLC>`, `MayRequire`, `ModsConfig.XActive`): sin la DLC el mod funciona, sin esa parte. Si degradar algo da demasiado trabajo, ese módulo requiere la DLC |
| Dubs Mint Menus | Nada que ver | Es solo de interfaz; no afecta |

Nuestro Core será el "framework": componente del cultivador, stats de qi, sistema de reinos, árbol de meridianos,
comps genéricos de técnicas, efectos visuales. Los demás módulos solo ponen **datos (XML)** sobre él.

## 2. Módulos y orden de trabajo

Tu orden (razas → cultivo → items → lore) lo ajustaría así, porque **todo depende del sistema de cultivo**
y el lore conviene esbozarlo primero (da nombres y reglas a todo lo demás):

| # | Módulo (mod) | Contenido | Depende de |
|---|---|---|---|
| 0 | **Lore** (documento, no mod) | Mundo, reinos, elementos, facciones, tono | — |
| 1 | **Core: Cultivo** | Qi, raíces espirituales, reinos, cultivo, tribulación, Corazón del Dao, meridianos, técnicas base, UI | Harmony |
| 2 | **Razas, genes y rasgos** ([detalle](modulos/RAZAS.md)) | Linajes (dragón, fénix, qilin…), genes de sangre, rasgos de talento/oficio/destino, capacidades innatas | Core |
| 3 | **Recursos** ([detalle](modulos/RECURSOS.md)) | Piedra/jade/cristal espiritual, metales espirituales, generación en el mapa y en el espacio | Core |
| 4 | **Fauna, flora y comida** ([detalle](modulos/FAUNA_FLORA.md)) | Despertar de bestias, núcleos, plantas espirituales, tierra espiritual, comida espiritual | Core + Recursos |
| 4d | **Defensa: armas espirituales y formaciones** ([detalle](modulos/DEFENSA.md)) | Grado espiritual de armas vs. supresión de reino, mesa de runas, talismanes, formaciones, torretas, dificultad que escala | Core + Recursos + Edificios |
| 4c | **Investigación: Comprensión del Dao** ([detalle](modulos/INVESTIGACION.md)) | Categoría de conocimiento propia estilo Anomaly, iluminaciones, árbol por ramas | Core |
| 4b | **Edificios, forja y feng shui** ([detalle](modulos/EDIFICIOS.md)) | Forja Espiritual por niveles, lugares de cultivo, muebles y joyas de jade, concentración de qi por habitación | Core + Recursos |
| 4e | **Ropa, armaduras y telas** ([detalle](modulos/ROPA.md)) | Algodón de las Nubes, seda espiritual, cuero de bestia, túnicas de secta, armaduras con grado espiritual, telar | Core + Resources (+ Fauna & Flora) |
| 4f | **Implantes del Dao y genética de bestias** ([detalle](modulos/IMPLANTES.md)) | Injertos míticos (huesos del Dao, venas de dragón, tercer ojo…), genepacks de bestia, altar de injerto y cuba de sangre | Core + Biotech + Fauna & Flora |
| 4h | **Raíces que cambian** ([detalle](modulos/RAICES.md)) | Extirpar, trasplantar y fusionar raíces en el altar y el horno de huesos, mejora en el avance de reino, píldora de lavado de raíz, ritual de purificación y la vía de ingeniería de Qi-Tech | Core + Grafts + Qi-Tech + Alchemy |
| 4g | **Escenarios e historias** (detalle en un documento reservado: contiene spoilers) | Escenarios con historia encadenada (marionetista, superviviente, huida, familia destruida…), eventos de historia, sitios y landmarks | Core + Sects (+ todos) |
| 5 | **Artefactos y alquimia** | Píldoras, talismanes, armas y tesoros | Core + Recursos + Flora + Edificios |
| 6 | **Sectas y facciones** | Facciones, rangos de secta, misiones, maestro/discípulo | Core (+ Razas) |
| 7 | **Qi-Tech** | Generadores de piedra espiritual, formaciones con electricidad, implantes de cultivo mecánicos | Core + Artefactos |
| 8 | **Mundo y eventos** | Reinos secretos, bestias legendarias, ruinas de sectas antiguas, eventos | Todos |

Regla de oro (lección de KotOR): **los módulos dependen del Core, nunca al revés, ni entre ellos en círculo.**
Todo vive en un solo repositorio, con una carpeta por mod.

## 2.1 Dos maneras de jugar: tecno-cultivo y cultivo clásico

Pedido de diseño desde el día uno: el mod tiene que poder jugarse **con tecnología** (tecno-cultivo) y **sin
tecnología** (estilo "edad media", como Medieval Overhaul o VFE Medieval, muy populares).

**Regla de arquitectura: el Core nunca depende de la tecnología.**

| Regla | Cómo se cumple |
|---|---|
| Nada del Core requiere electricidad | Los edificios de cultivo usan **piedras espirituales** como combustible (`CompRefuelable`), nunca `CompPower` |
| La investigación del Dao no pide investigación industrial | Proyectos con `techLevel` Neolítico/Medieval y sin prerrequisitos de electricidad |
| Recetas con materiales de cualquier época | Madera, piedra, hierbas, jade; nada de componentes ni plastiacero en el Core |
| Toda la tecnología vive en módulos aparte | **Qi-Tech** (formaciones eléctricas, implantes mecánicos de cultivo) y la corporación Ascension Dynamics son opcionales |
| Las sectas funcionan en ambos modos | Facciones de nivel tecnológico Medieval, con artefactos propios en vez de armas de fuego |

**Cómo elige el jugador:**
- **Sin el módulo Qi-Tech instalado** = cultivo clásico puro.
- **Ajuste "Modo de mundo"** (`Tecno-cultivo` / `Clásico`): en Clásico se desactivan los eventos y facciones
  tecnológicos (Ascension Dynamics, cazadores corporativos, purga del Imperio). El lore del Borde pasa a ser un
  mundo de baja tecnología, pero la Convergencia funciona igual.
- **Escenario "Era Clásica"**: inicio tribal/medieval con la Convergencia ya ocurrida o por llegar.
- Más adelante, carpeta de compatibilidad con **Medieval Overhaul** y **VFE Medieval** (sin depender de ellos).

En el código: cada Def que solo tenga sentido con tecnología lleva `DefModExtension_TechOnly`. Si el ajuste está
en Clásico, el Core lo oculta o lo deshabilita. Así no hace falta duplicar contenido.

## 3. Primera versión jugable (MVP)

Para no repetir lo de la traducción ("demasiado trabajo"), primero un corte vertical pequeño pero completo:

- 1 raíz espiritual (gen/rasgo) con 5 elementos: metal, madera, agua, fuego y tierra.
- **3 reinos**: Refinamiento de Qi → Establecimiento de Fundación → Núcleo Dorado, con etapas inicial/media/tardía/cima.
- Qi como recurso (barra), meditación que lo llena y hace avanzar.
- 1 tribulación al pasar a Núcleo Dorado.
- 3 técnicas (ataque elemental, escudo de qi, paso veloz).
- 1 píldora (Píldora de Fundación) y 1 recurso (piedra espiritual).
- Pestaña de cultivo en el colono, y aura visual por reino.
- El Maestro (1 arquetipo): llega con la Convergencia, da 2 o 3 misiones de enseñanza y se va dejando el fragmento
  de jade. La búsqueda en el mapa del mundo queda para después del MVP.

Con eso ya se juega el ciclo completo: **meditar → cuello de botella → píldora → tribulación → nuevo reino.**

## 4. Sistema de cultivo (núcleo)

### 4.1 Reinos (inspirados en Tale of Immortal)

**Decidido: 6 reinos** (condensa los 10 de Tale of Immortal para que encajen en una partida de RimWorld).

| # | Reino | Cambio en el juego (idea) |
|---|---|---|
| 1 | Refinamiento de Qi | Desbloquea el qi y técnicas básicas |
| 2 | Establecimiento de Fundación | +vida, +salud, técnicas a distancia; primera tribulación menor |
| 3 | Núcleo Dorado | Gran salto; tribulación; vuelo con espada (movimiento rápido); presión de aura |
| 4 | Alma Naciente | Detecta enemigos; sobrevive una vez a la muerte del cuerpo |
| 5 | Transformación Divina | Técnicas de área enormes; comprensión del Dao con buffs a la colonia |
| 6 | Ascensión | Final del juego: ascender = victoria alternativa |

Cada reino tiene 4 etapas. Para pasar de reino hace falta qi lleno + recurso (píldora o tesoro) y,
en reinos clave, **superar una tribulación** (incidente con rayos; si falla: herida grave, desviación de qi o muerte).

### 4.1b Los Tres Tesoros (精气神) y los caminos de cultivo

| Tesoro | Qué es | Quién | Mecánica |
|---|---|---|---|
| **精 Jing / sangre-qi (血气)** | Fuerza vital del cuerpo | Cultivadores de cuerpo (sin raíz, cuerpos especiales) | Recurso propio: se llena comiendo (carne espiritual), entrenando (trabajo de templado) y descansando; técnicas corporales |
| **气 Qi** | Energía espiritual | Cultivadores con raíz | Recurso actual (M3) |
| **神 Shen / sentido divino (神识)** | Fuerza del alma | Todos; crece con el reino | Stat al principio (detección, resistencia mental, control de espada voladora y formaciones, calidad de alquimia); recurso desde Alma Naciente |

**Caminos** (`CultivationPathDef`, cada reino pertenece a un camino):
- **Qi:** los 6 reinos del punto 4.1.
- **Cuerpo:** Templado de Piel → Carne → Huesos → Médula → Sangre → **Cuerpo Dorado** (金身). Más lento, menos técnicas,
  muy resistente, no depende de la concentración de qi. Es el camino de los colonos sin raíz (M4b).
  **Para seguir progresando necesita sí o sí recursos de bestias espirituales** (decisión 2026-10-15): sangre de bestia
  espiritual y comida hecha con bestias espirituales. Por eso M4b incluye la base del módulo de fauna: despertar de
  animales en bestias espirituales, carne y sangre espiritual, y una comida espiritual básica.
- **Doble cultivo (Qi + Cuerpo):** raro, lento y muy poderoso. La arquitectura lo permite (un rastreador por camino).

### 4.1c Avanzar de reino tiene que costar… y sentirse

**Atributos de fundamento** (se entrenan, ver EDIFICIOS.md §5): **Templado corporal** (精), **Pureza del qi** (气),
**Fuerza del alma** (神). Cada uno con niveles.

- **Requisitos mínimos por reino:** para intentar el avance hacen falta mínimos de los tres atributos (en XML, por
  reino; ej. Núcleo Dorado: cuerpo 3, qi 4, alma 2).
- **Calidad del fundamento (根基):** al subir se guarda según cuánto superaste los mínimos, el lugar y cómo subiste.
  Fundamento **sólido** → ese reino rinde más para siempre. Subir forzado con píldoras → fundamento **inestable**:
  menos poder y más riesgo en el próximo avance.
- **Curva de poder fuerte:** cada reino **multiplica** vida, daño, armadura y velocidad (no suma un 10%).
- **Supresión de reino:** el daño de un reino inferior contra uno superior se reduce mucho por cada reino de diferencia
  (un Núcleo Dorado casi no siente las armas de un mortal; un Alma Naciente aguanta balas). Configurable en los ajustes.
- **Ritmo orientativo:** Refinamiento ~1ª estación, Fundación ~año 1, Núcleo Dorado ~años 2-3, Alma Naciente ~año 5,
  Transformación Divina ~años 7-8, Ascensión final del juego.

### 4.1d Selector de personajes y generación (decisión 2026-10-15)
Todo lo del mod aparece al generar pawns, también al **aleatorizar en el selector de personajes**: raíz, cuerpos, linajes,
rasgos y destinos (ya funciona). Además:
- **Cultivo ya iniciado** (estilo Isekai "rango SSS"): probabilidad baja de nacer con reino/etapa avanzados, más rara
  cuanto más alto (Refinamiento poco común → Fundación rara → Núcleo Dorado rarísima → Alma Naciente casi imposible).
  Lore: cultivadores caídos por grietas antes o reencarnados. Si la Convergencia no ocurrió, el poder queda latente.
  **Implementado (2026-10-15):** `PreCultivationDef` por camino en el Core (`Defs/PreCultivation.xml`): probabilidad
  (qi: 12% de los que tienen raíz; cuerpo: 30% de los sin raíz con cuerpo especial) × ajuste, y opciones por orden de reino
  con pesos (Refinamiento etapas 2-4 ≈80%, Fundación ≈15%, Núcleo Dorado ≈4%, Alma Naciente ≈0,5%: si el reino no existe,
  el más alto disponible). Fundamento al azar 0,8-1,2. Antes de la Convergencia queda **latente** (sin hediff; la pestaña
  Cultivo lo muestra como "Latente") y despierta con ella. Línea "Cultivó antes de la Convergencia" en la pestaña. Ajustes:
  interruptor y multiplicador. Las sectas (`DefModExtension_CultivatorPawnKind`) usan su propio reino.
- **Implantes de serie** (con el mod de injertos): algunos pawns nacen con un injerto, como los bionicos vanilla.
- Probabilidades en Ajustes; opción para empezar sin nada de esto.

### 4.2 Piezas del sistema

- **Raíz espiritual:** gen (con Biotech) o rasgo (sin Biotech). Calidad (mortal → celestial) y elemento. Sin raíz no se cultiva… salvo con métodos "demoníacos" o tecnológicos.
- **Cultivo corporal (sin raíz):** etapas de Templado Corporal (cuerpo → médula → huesos) para colonos sin raíz
  espiritual. Es una vía wuxia "mortal", más limitada, que encaja en el modo clásico (idea tomada de Cultivator of the Rim).
- **Yin/Yang:** los cuerpos Yin cultivan mejor de noche y los Yang de día.
- **Cuerpos especiales (体质):** rasgos/genes raros, independientes de la raíz. Algunos despiertan con la Convergencia.
  | Cuerpo | Idea |
  |---|---|
  | Cuerpo Yin Puro / Yang Puro | Gran bonus de noche / de día; afinidad con agua-hielo / fuego |
  | Cuerpo de los Nueve Yang | Fuego extremo; inmune al frío; riesgo de "quemarse" si no cultiva |
  | Cuerpo de Espada Celestial | Técnicas de espada mucho más fuertes |
  | Cuerpo Inmortal (de Bronce/de Jade) | Ideal para el templado corporal: resiste daño, regenera |
  | Cuerpo del Caos | Muy raro: cultiva todos los elementos, pero avanza lento |
  | Cuerpo Venenoso | Inmune a toxinas; técnicas de veneno; los demás lo evitan |
  | Cuerpo Espiritual Innato | Sin raíz, pero puede cultivar por el cuerpo hasta reinos altos |
  Encajan con el templado corporal: un colono sin raíz pero con un buen cuerpo también tiene futuro.
- **Cuerpos exclusivos de quien NO tiene raíz** (solo pueden aparecer sin raíz espiritual): no acumulan qi, pero
  templan el cuerpo como un arma y llegan más lejos por el templado corporal.
  | Cuerpo | Idea |
  |---|---|
  | Cuerpo de Acero | Piel y huesos como metal: armadura natural alta |
  | Cuerpo de Tribulación | Los rayos celestiales lo fortalecen en vez de matarlo; puede "robar" tribulaciones |
  | Cuerpo de Espada | Su cuerpo es la espada: golpes cortantes, armas cuerpo a cuerpo como extensión |
- **Rasgos de oficio espiritual** (además de la raíz y el cuerpo): por ejemplo **Agricultor Espiritual**.
  - Cultivar plantas espirituales requiere **tierra espiritual**: se prepara mezclando tierra con fragmentos de
    bestias (huesos, sangre, núcleos) y piedras espirituales. Es un terreno que se construye, como el suelo fértil.
  - Solo en esa tierra crecen las hierbas y frutos para píldoras y otros objetos (módulo Artefactos y alquimia).
  - El Agricultor Espiritual las cultiva mejor, con más rendimiento y mejor calidad. Más oficios posibles:
    Alquimista innato, Forjador de artefactos, Maestro de formaciones, Domador de bestias.
- **Qi:** stats propios `CVG_QiMax`, `CVG_QiRegen`… modificables por genes, equipo, píldoras y formaciones (patrón de The Force).
- **Meditación:** reutiliza la meditación del juego y suma lugares de alta energía espiritual (vetas de qi en el mapa).
- **Corazón del Dao:** estabilidad mental. Si baja, aumenta el riesgo de **desviación de qi** al avanzar (patrón de alineamiento de The Force, aplicado a recto/demoníaco).
- **Meridianos (constelaciones):** árbol de pasivas al estilo Isekai, pero **cada reino abre una rama nueva**, y los bonus son stats normales del juego (se pueden ampliar sin tocar C#).
- **Técnicas:** habilidades del juego con coste de qi y reino mínimo; pocos comps genéricos (proyectil, área, buff canalizado, movimiento).
- **Presión de aura:** los de reino muy superior aturden a los débiles (como Isekai, pero por reino).
- **NPCs:** las facciones generan cultivadores con reino y técnicas acordes.

## 4.3 Huecos para objetos especiales (equipamiento de cultivador)

RimWorld no trae huecos para colgantes, anillos ni talismanes: los creamos. Casi todo sin C#.

| Objeto | Solución | ¿C#? |
|---|---|---|
| Colgante / jade del maestro | Capa de ropa nueva `CVG_Neck` (`ApparelLayerDef` + grupo de cuerpo) | No (la personalidad sí) |
| Anillos | Capa `CVG_Finger` (1 o 2 huecos) | No |
| Talismán, calabaza, bolsa espiritual | Capa `CVG_Waist` | No |
| Espada voladora | Accesorio en la capa de utilidad con habilidad propia (como el jetpack de Royalty), compatible con un arma normal | Sí (la habilidad de vuelo) |
| Anillo espacial | Primero: gran bonus de capacidad de carga. Después: inventario propio con interfaz | Después sí |
| Núcleos, perlas del dao, semillas de fuego | Se consumen y quedan como estado permanente (dantian); no ocupan hueco | Poco |
| Núcleo de formación | Combustible = piedras espirituales (`CompRefuelable`) | No |
| Horno de alquimia, altar del jade | Edificios contenedores (como el banco de genes de Biotech) | Sí |
| Límite por reino | "Solo un cultivador de Núcleo Dorado puede usar este tesoro" | Sí (restricción de equipo) |

A revisar: que las capas nuevas convivan con mods de ropa populares, y cómo se ven en el retrato del colono.

## 5. Razas (borrador)

Todas como **xenotipos de Biotech** (genes), como hace KotOR:

| Raza | Idea |
|---|---|
| Humano mortal | Base; raíz espiritual aleatoria (la mayoría no tiene) |
| Linaje antiguo (Gu) | Humanos con sangre de cultivadores; mejor raíz |
| Bestia demoníaca humanoide (Yao) | Cuerpo fuerte, cultivo por sangre; odiada por sectas rectas |
| Espíritu/zorro (Hu Li) | Encanto, técnicas mentales, vida larga |
| Linaje de dragón | Raro; poder físico y elemental enorme |
| Constructo de qi (Qi-Tech) | Androide con núcleo espiritual: puente con la tecnología |

## 6. Fusión con la tecnología (el gancho)

- **Libertad entre carne, qi y metal:** cada implante (vanilla, de otros mods o nuestro) tiene una
  **disonancia celestial** configurable que modifica la tribulación. Por defecto, los implantes mecánicos toscos
  suman algo y los de cultivo nada (o restan). Se ajusta en los ajustes del mod. La actitud moral ante los implantes
  la pone la Ideología del jugador, no el mod.
- **Implantes de cultivo** (cirugías o hediffs, algunos también como genes de raza):
  | Implante | Efecto (idea) |
  |---|---|
  | Corazón del Dao abierto | +estabilidad del Corazón del Dao; menos riesgo de desviación |
  | Raíces espirituales expuestas | **Raíz artificial** para quien no tiene raíz; de calidad baja, pero mejorable |
  | Sangre de fénix | Regeneración; resurrección única al morir |
  | Circuitos de meridianos | +regeneración de qi; Qi-Tech |
  | Reactor de núcleo dorado | Sustituye el núcleo natural: mucho qi, pero la tribulación de ascensión es mucho más dura |
  | Ojo celestial | Ve auras y reinos de enemigos; detecta lo invisible |
- **Qi-Tech:** generadores de piedra espiritual, baterías de qi, torretas con runas, espadas voladoras con circuitos.
- **Investigación doble:** árbol del Dao y árbol Qi-Tech, que se cruzan en el nivel avanzado.

## 7. Lore y evento disparador

Detalle en [LORE.md](LORE.md). **Decidido:** choque de dimensiones al estilo 灵气复苏. Una sociedad tecnológica
(el Borde) recibe la **Convergencia** con un fragmento del Reino Celestial, y eso abre la Era del Cultivo.

Impacto en el diseño:
- **Fase latente:** raíces y rasgos de cultivo existen desde el inicio, pero inactivos (estado `Latent`).
- **Evento `CVG_Convergence`:** un `GameComponent` guarda si ya ocurrió. Se dispara por narrador, investigación o escenario (opción del mod).
- Todo el contenido (vetas de qi, investigación del Dao, sectas, reinos secretos) se activa cuando el evento ocurre.
- Modelo a estudiar: el monolito de **Anomaly** (progresión desbloqueada por evento).
- **El Maestro** (ver LORE.md): un pawn invitado que llega herido con el evento. Mecánicas a estudiar: huéspedes
  de misiones del juego (lodgers de Royalty/vanilla) y el "creepjoiner" de Anomaly. Las misiones de enseñanza se
  hacen con un `GameComponent` propio de objetivos (más simple que `QuestScriptDef`). Si muere, colgante de jade
  (ítem con comp que guarda los datos del maestro). Referencia de diseño: *Amazing Cultivation Simulator*.
- Para el MVP: el evento más simple posible (carta + despertar de raíces + vetas de qi) y un escenario de inicio inmediato para probar.

## 8. Decisiones

**Tomadas (2026-10-15):**
- **Tono:** xianxia + tecnología.
- **Reinos:** 6 en la versión completa (el MVP usa los 3 primeros).
- **Raíz espiritual:** gen con Biotech, rasgo sin Biotech.
- **Tribulación:** puede matar, pero se puede preparar (píldoras, formaciones) y hay una opción en los ajustes para desactivar la muerte.

- **Lore:** choque de dimensiones / renacimiento del qi, con un evento disparador (la Convergencia); tono tecno-cultivo.
- **DLC:** el mod se apoya en todas las DLC; sin una DLC funciona sin las partes que la necesitan (degradación por carpetas condicionales, cuando sea barato).
- **Idioma:** nombre y textos en inglés, con traducción al español desde el principio.
- **Nombre:** Convergence: Era of Cultivation (prefijo `CVG_`).

**Tomadas (2026-10-15, ronda 3):**
- **Economía paralela:** los objetos del mod no inflan la riqueza vanilla que dispara los asaltos (una píldora de 5k no
  trae 200 asaltantes); el mod lleva su propia **"riqueza de cultivo"** (reinos, tesoros, técnicas, formaciones) que
  dispara los asaltos de sectas, bestias y misiones del mod.
- **Dificultad de cultivo** como opción de partida: **Fácil / Difícil / Pesadilla** (velocidad de cultivo, letalidad de
  tribulación, supresión, escala enemiga). Y **cómo empezar**: "al palo" (Convergencia inmediata) o **vanilla primero**
  (la Convergencia llega más tarde, por defecto a mitad del primer año) para integrar el mod de a poco.
- **IA enemiga** con las herramientas del juego (AbilityDef aiCanUse, ThinkTree/JobGiver vanilla de combate).
- **Interfaz simple, como los psíquicos:** la mayoría de las técnicas son **pasivas** (desbloquear da un bonus, no un
  botón); solo **3-4 técnicas activas equipadas**, visibles **solo en modo combate** (reclutado).
- **Longevidad** con los sistemas de Biotech (edad, rejuvenecimiento): los reinos ralentizan el envejecimiento y los
  altos se acercan a "sin edad", como el gen vanilla.
- **Final del juego:** cadena de 4-8 misiones medias/difíciles de exploración hacia la Ascensión / la Calamidad de Hierro (más adelante).
- **Eventos:** el catálogo crece mucho, pero **no se le cuenta al usuario**: los descubre jugando.

**Implementado (ronda 3):**
- *Economía paralela* (`Source/Convergence.Core/Economy/CultivationWealth.cs`, `Defs/CultivationWealth.xml`): los
  objetos, edificios y marionetas/bestias de los mods de Convergence cuentan un 20% (según la dificultad) en la riqueza
  vanilla; su valor completo, más los reinos (curva por reino y etapa) y las técnicas de los colonos, es la **riqueza de
  cultivo**, que da puntos extra a los asaltos de las sectas, a las manadas enloquecidas (la mitad) y a la noche de
  jiangshi. Se ve en la pestaña Cultivo (línea con tooltip y desglose).
- *Dificultad de cultivo* (`Core/CultivationDifficulty.cs`, `Defs/CultivationDifficulty.xml`): Fácil / Difícil /
  Pesadilla (velocidad de cultivo, daño y letalidad de la tribulación, supresión, reino enemigo máximo, amenaza de
  cultivo, frecuencia de eventos, fracción en la riqueza vanilla). Cada partida guarda la suya; se elige en una ventana
  al empezar (con el modo de inicio: inmediata / vanilla primero, días 28-32 por defecto / investigación) y se cambia en
  los ajustes.
- *Interfaz de técnicas como Vanilla Psycasts Expanded* (cambio del usuario): botón Desarrollo en la pestaña Cultivo,
  ventana con caminos de 5 niveles (la raíz abre los elementales; tablillas, Espada y Sentido Divino; el templado, el
  Cuerpo Vajra), mayoría de pasivas, conjuntos de combate como los psysets (gizmos solo del conjunto activo y solo
  reclutado) y balance de VPE (ver DISENO_CORE.md, técnicas).
- *IA enemiga*: condiciones de uso por distancia, salud y grupo; escudos antes de atacar; nada de avances en combate.
- *Longevidad*: cada reino frena el envejecimiento biológico (Núcleo Dorado ×0,15-0,05, casi sin edad), sobre el mismo
  valor que usan los genes vanilla.

**Abiertas:**
1. ~~packageId~~: `prospero1088.convergence.core`.
2. **Integración con Isekai:** ¿opcional desde el principio o más adelante?
