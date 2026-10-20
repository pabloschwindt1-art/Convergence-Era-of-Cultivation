# Investigación: Comprensión del Dao

Pestaña propia **"Cultivo"** en investigación (ya existe desde M2, con el proyecto que dispara la Convergencia).

## 1. Mecánica

Modelo: la investigación de **Anomaly** (proyectos con categoría de conocimiento propia que avanzan estudiando, no en
la mesa). Nuestra categoría: **Comprensión del Dao** (`KnowledgeCategoryDef CVG_DaoComprehension`).

| Fuente de comprensión | Notas |
|---|---|
| Cultivar | La fuente principal: los colonos comprenden mientras cultivan |
| Entrenar (cuerpo / qi / alma) | Cada tesoro aporta a su rama |
| Estudiar **tablillas de jade**, ruinas de sectas, tesoros, núcleos de bestia | Objetos estudiables (como las entidades de Anomaly) |
| Feng shui y Marea de Qi | Multiplicadores |
| **Iluminación súbita** (顿悟) | Evento aleatorio en colonos cultivando: gran salto de comprensión. Más probable con Comprensión alta |

- **Al completar un proyecto:** carta de iluminación, p. ej. *"Leonid se iluminó mientras cultivaba y comprendió el Arte de
  la Forja Espiritual."* (se nombra al colono que más aportó).
- **Requisitos de reino:** algunos proyectos solo avanzan si en la colonia hay un cultivador de cierto reino
  (`DefModExtension_RequiresRealm` + comprobación en C#).
- **Sin Anomaly:** todo pasa a investigación normal en un **Escritorio de Contemplación** propio (degradación).
- **Qi-Tech:** investigación normal en la mesa vanilla, con prerrequisito de electricidad; oculta en modo clásico.

## 2. Árbol (borrador)

| Rama | Proyectos (en orden) | Desbloquea |
|---|---|---|
| **Fundamentos** | Estudio de la anomalía espiritual (dispara la Convergencia) → Respiración del Qi → Meridianos | Cojín de cultivo, pestaña de cultivo completa |
| **Reinos** | Fundación → Núcleo Dorado → Alma Naciente → Transformación Divina → Ascensión | Requisitos de comprensión para cada avance; reino mínimo en la colonia para los altos |
| **Cuerpo** | Templado de Piel → Huesos → Sangre → Cuerpo Dorado | Muñeco de madera, pesas, pilar de hierro, estanque frío; reinos del Camino del Cuerpo |
| **Alma** | Sentido Divino → Fortaleza del Alma → Alma Naciente | Altar de incienso, mesa de caligrafía, tablero de go, cámara de ilusiones |
| **Alquimia** | Hierbas espirituales → Horno de alquimia → Píldoras de grado medio → Píldoras de grado alto | Horno (niveles), recetas de píldoras, tierra espiritual (con Flora) |
| **Forja** | Acero espiritual → Forja nivel 2 → Forja nivel 3 → Espada voladora | Forja Espiritual y sus niveles, metales espirituales, artefactos |
| **Formaciones y feng shui** | Cinco elementos → Formación de reunión de qi → Formaciones defensivas → Gran formación protectora | Muebles de feng shui, formaciones (combustible: piedras espirituales) |
| **Talismanes** | Papel de talismán → Talismanes de combate → Talismanes de teletransporte | Mesa de talismanes, talismanes |
| **Bestias y flora** | Domar bestias espirituales → Cría → Árbol espiritual | Corrales, árbol espiritual, comida espiritual |
| **Qi-Tech** (modo tecno) | Medición del qi → Baterías de piedra espiritual → Formaciones eléctricas → Implantes de cultivo | Contenido Qi-Tech (mesa vanilla + electricidad) |

## 2b. Descubrimientos que abren contenido (iluminaciones especiales)

Algunos proyectos no desbloquean muebles: **descubren partes del mundo**. Ejemplo:
*"Juan se iluminó y descubrió que en este mundo existen los reinos secretos. Explorarlos es difícil, pero muy satisfactorio."*

| Descubrimiento | Abre |
|---|---|
| **Reinos secretos** (秘境) | Misiones de mazmorra: un **mapa de bolsillo** (la tecnología de mapas internos de Anomaly, como su laberinto y la cueva bajo la compuerta del pozo) al que se entra por una grieta en el suelo o en el aire. Con Odyssey, algunos están en el espacio o en otros biomas |
| **Cuevas de herencia** (洞府) | Sitios en el mapa del mundo con pruebas, trampas y técnicas |
| **Vetas de dragón** (龙脉) | Detectar vetas de qi en el mapa para colocar formaciones y cojines |
| **Mercados de cultivadores** | Caravanas y subastas de sectas |

## 3. Prioridad
1. Categoría de conocimiento + cultivar genera comprensión + carta de iluminación (con lo que ya existe).
2. Mover el cojín y lo de M4 detrás de proyectos de Fundamentos y Reinos.
3. Ramas nuevas a medida que se hacen sus módulos.
