# Módulo: Razas, genes y rasgos (Convergence.Races)

Depende de: Core + **Biotech** (xenotipos y genes). Usa todas las DLC (decisión 2026-10-15).

## 1. Linajes: decisiones (2026-10-15)

- **Primera versión: 5 linajes** — las Cuatro Bestias Benévolas (四灵: **Dragón** 龙, **Fénix** 凤, **Qilin** 麒麟,
  **Tortuga Negra** 玄武) más el **Zorro** (狐). Variantes y más linajes después (Tigre Blanco, bestia demoníaca,
  constructo de qi…). Todo definido por datos para
  poder **agregar linajes con XML**.
- **Se consiguen naciendo y despertando:** xenotipos en facciones y escenarios, y además humanos comunes con un
  **rastro de sangre** dormido que se purifica jugando.
- **Un solo linaje por colono.** Las sangres se repelen (todas las sangres comparten `exclusionTag`).
- **Aparecen al azar en el mundo**, con probabilidad baja que baja mucho con la pureza.
- **Pureza = recompensa:** las purezas bajas tienen pros y contras marcados; **Ancestral casi no tiene contras**,
  para que 5 años de partida purificando valgan la pena.

## 2. Pureza de sangre (4 niveles)

| Pureza | Probabilidad en humanos del mundo (idea) | Pros | Contras |
|---|---|---|---|
| **Rastro** | ~3% | Mínimos (dormidos hasta la Convergencia) | Uno leve |
| **Diluida** | ~1% | Notables + 1ª capacidad innata | Marcados |
| **Pura** | ~0,3% (más común en su facción) | Fuertes + 2ª capacidad | Leves |
| **Ancestral** | ~0,03% (casi solo por purificación) | Enormes + capacidad suprema | Casi ninguno (solo de sabor) |

**Sangre y raíz** (decisión 2026-10-15): cada pureza da **afinidad** con los elementos del linaje (cultivo más rápido de
esos elementos). Al llegar a **Ancestral**, la raíz espiritual **sube un grado una sola vez** ("la sangre ancestral limpia
la raíz"); si el colono no tenía raíz, despierta una raíz inferior del elemento del linaje.

**Purificar:** se sube un nivel consumiendo esencias de sangre de bestias (módulo Fauna), píldoras de purificación
(Alquimia) y, para Ancestral, un tesoro legendario (núcleo de uno de los Cuatro Malvados o equivalente). Cada paso
puede requerir una prueba (soportar el dolor de la purificación: estado temporal con riesgo).

## 2c. Identidad por linaje (decisión 2026-10-15)
Cada linaje tiene un **rol en la colonia**, no solo poder de combate. Aptitudes de habilidad en el gen de pureza
(Rastro +1 → Ancestral +4; pasión desde Pura) y stats de rol en el hediff de sangre (despiertan con la Convergencia).

| Linaje | Identidad | Habilidades (+) | Penalización (−) | Stats de rol |
|---|---|---|---|---|
| Dragón | Sabio y líder | Intelectual, Social | Animales (le temen) | Investigación, aprendizaje, impacto social |
| Qilin | Veloz y bendito | Plantas, Medicina | Cuerpo a cuerpo (pacifista) | Velocidad de movimiento, calidad médica, rendimiento de cosecha |
| Tortuga Negra | Muralla y minero | Minería, Construcción | Fabricación | Armadura, capacidad de carga, rendimiento minero (lenta al caminar) |
| Fénix | Artesano del fuego | Fabricación, Cocina | Plantas (su fuego marchita) | Velocidad de forja y cocina, inmune a quemaduras |
| Zorro | Encanto y astucia | Social, Arte | Minería (delicado) | Negociación, comercio, reclutamiento, esquiva |

Penalización: −2 en Rastro y Diluida, −1 en Pura y Ancestral (sabor).

## 2d. Genes de sangre por niveles (decisión 2026-10-15)
Todos los linajes **comparten** un set de genes de sangre, y cada gen tiene **4 niveles (uno por pureza)**. Al purificar,
los genes **suben de nivel**: más efecto y menos defecto (en Ancestral el defecto casi desaparece).

| Gen común | Efecto (sube) | Defecto (baja) |
|---|---|---|
| Vitalidad bestial | Regeneración de heridas, resistencia al dolor | Hambre extra |
| Sentidos agudos | Puntería, detección, visión nocturna | Sensibilidad al ruido/luz (humor) |
| Longevidad de sangre | Envejecimiento más lento | Fertilidad baja |
| Instinto de la bestia | Daño cuerpo a cuerpo, esquiva | Agresividad (peleas sociales) |
| Resonancia de qi | Velocidad de cultivo, qi máximo | Inestabilidad de qi (riesgo de desviación) |

**Genes firma por linaje** (también por niveles), 2-3 por linaje, p. ej.: Dragón — Escamas de dragón, Pulmones de dragón,
Vitalidad del dragón; Fénix — Plumas ígneas, Nacido de las cenizas, Nirvana; Qilin — Presencia auspiciosa, Pezuñas puras,
Repelente del mal; Tortuga — Caparazón, Sueño de piedra, Serpiente guardiana; Zorro — Fuego de zorro, Velo de ilusión,
**Nueve colas, nueve vidas** (1/3/6/9 vidas extra).

## 3. Los 5 linajes

### Dragón (龙) — combate y presencia
- **Afinidad:** agua y rayo. **Cuerpo:** escamas (armadura natural), fuerza, vida muy larga.
- **Capacidades:** Aliento de Dragón (Diluida) → Presión del Dragón, aura que aturde a los débiles (Pura) → Forma de Dragón temporal (Ancestral).
- **Contras por pureza:** Orgullo (mal humor al hacer trabajos humildes), Tesoro de Dragón (quiere riqueza en su
  cuarto), los clanes rivales lo cazan. En Ancestral solo queda el Tesoro de Dragón (sabor).
- **Aspecto:** cuernos, cola escamosa, ojos de serpiente.

### Fénix (凤) — supervivencia y fuego
- **Afinidad:** fuego. **Cuerpo:** regeneración, inmune al calor.
- **Capacidades:** Llamas del Fénix (Diluida) → Nirvana, resurrección **una vez** (Pura) → Nirvana cada pocos años (Ancestral).
- **Contras por pureza:** sufre mucho el frío, temperamento explosivo, sus crisis mentales pueden prender fuego. En
  Ancestral solo queda la aversión al frío (leve).
- **Aspecto:** plumas en el pelo, ojos dorados, marcas de llama.

### Qilin (麒麟) — apoyo y fortuna
- **Afinidad:** tierra. **Cuerpo:** calma, Corazón del Dao estable, detecta el mal.
- **Capacidades:** Bendición del Qilin, aura de ánimo y curación (Diluida) → Pezuña Purificadora, daño extra a lo demoníaco y al vacío de Anomaly (Pura) → Dominio Auspicioso, fortuna para toda la colonia (Ancestral).
- **Contras por pureza:** pacifista (mal humor al matar), sufre cerca de cadáveres y ejecuciones. En Ancestral
  puede luchar sin penalización; solo rechaza las ejecuciones.
- **Aspecto:** pequeños cuernos, escamas doradas en la piel, ojos serenos.

### Zorro (狐) — mente y encanto
- **Afinidad:** fuego y lo mental. **Cuerpo:** belleza, impacto social, técnicas mentales; cuerpo frágil en purezas bajas.
- **Las colas marcan la pureza:** 1 cola (Rastro), 3 (Diluida), 6 (Pura), **9 colas** (Ancestral).
- **Capacidades:** Encanto (Diluida) → Ilusión (Pura) → Forma de Nueve Colas (Ancestral).
- **Contras por pureza:** desconfianza social, travieso (peleas y bromas), cuerpo frágil. En Ancestral solo queda una leve desconfianza.
- **Aspecto:** orejas y colas de zorro (de Biotech mientras no haya arte propio).

### Tortuga Negra (玄武) — defensa y longevidad
- **Afinidad:** agua y tierra. **Cuerpo:** defensa enorme, longevidad extrema, resistencia a toxinas y enfermedades.
- **Capacidades:** Caparazón de Xuanwu, escudo temporal (Diluida) → Serpiente Guardiana, contraataque automático (Pura) → Montaña Inamovible, inmune a empujes y aturdimientos y protege a los aliados cercanos (Ancestral).
- **Rasgo de identidad: camina lento en todas las purezas** (es la tortuga). Se compensa con defensa y aguante: en
  Ancestral sigue siendo lenta al caminar, pero no la pueden empujar, aturdir ni derribar.
- **Contras por pureza:** lentitud marcada al caminar (más fuerte en purezas bajas), terquedad (le cuesta cambiar de
  trabajo y aprender), sueño pesado. En Ancestral solo queda la lentitud al caminar, como rasgo de sabor.
- **Aspecto:** piel con patrón de caparazón, ojos oscuros profundos, marcas de serpiente.

## 4. Arquitectura (para poder agregar linajes con XML)

- **`BloodlineDef`** (XML): nombre, elementos de afinidad, lista de **4 niveles de pureza**; cada nivel con su gen,
  su hediff de efectos, capacidades y contras.
- **Gen = identidad** (qué linaje y qué pureza; se hereda con Biotech). **Hediff de linaje = efectos** (stats,
  capacidades, contras), aplicado **solo después de la Convergencia**, igual que los reinos. Sin Biotech: rasgo como identidad.
- **Purificación genérica en C#:** sube de nivel cambiando el gen y el hediff; sirve para cualquier linaje nuevo.
- **Generación:** al crear humanos, tirada por linaje y pureza (configurable en ajustes); facciones con xenotipos propios.
- **Xenotipos:** uno por linaje para facciones y escenarios (el Clan del Dragón Antiguo usa el de Dragón, etc.).

## 5. Genes de cultivo (ya en M2)
- Calidad de raíz (inferior → celestial) y elementos (metal, madera, agua, fuego, tierra).
- Cuerpos especiales como genes también (hoy son rasgos), para que se hereden.

## 6. Rasgos

| Tipo | Ejemplos |
|---|---|
| Raíz y cuerpos | Hecho en M2 (rasgos sin Biotech; con todas las DLC se priorizan genes) |
| **Catálogo completo** | Ver [RASGOS.md](RASGOS.md): cuerpos, corazones, comprensión, oficios, destino y personalidad wuxia |
| **Talento de cultivo** | Mediocre / Bueno / Prodigio (velocidad de cultivo y de comprensión) |
| **Oficios espirituales** | Agricultor Espiritual, Alquimista Innato, Forjador de Artefactos, Maestro de Formaciones, Domador de Bestias |
| **Corazón del Dao** | Corazón Firme (resiste el demonio interno), Corazón Inquieto, Obsesión por el Dao |
| **Destino** (气运) | Afortunado (encuentra tesoros), Destino Turbulento (más eventos, buenos y malos), Elegido del Cielo |

## 7. Capacidades innatas
Habilidades (`AbilityDef`) que vienen con un gen de linaje o con un cuerpo especial, **sin aprender técnicas**.
Despiertan con la Convergencia. Escalan con el reino de cultivo.

## 8. Uso de las DLC
- **Biotech:** xenotipos, genes, herencia (los hijos heredan sangre y raíz), vientre de crecimiento.
- **Royalty:** los linajes nobles pueden tener títulos; la presión de aura recuerda a los psycasts.
- **Ideology:** memes como "Linaje Sagrado" o "Supremacía de Sangre"; precepto sobre bestias demoníacas.
- **Anomaly:** el linaje de bestia demoníaca puede tener una variante corrompida por el vacío.

## 9. Prioridad
1. Arquitectura de linajes (`BloodlineDef`, gen + hediff, purificación) con el **Dragón** como primer caso.
2. Fénix, Qilin, Tortuga Negra y Zorro sobre la misma plantilla.
3. Rasgos de talento y oficio (el Agricultor Espiritual hace falta para Flora).
4. Variantes y linajes nuevos.
