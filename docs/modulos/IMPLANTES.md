# Módulo: Implantes del Dao y genética de bestias (Convergence: Grafts)

Mod aparte. Depende del Core (+ Biotech para genes; + Resources y Fauna & Flora para materiales). Principio n.º 1:
se engancha a los sistemas vanilla de **cirugía/bionicos** y de **genes de Biotech** (extractor, ensamblador,
genepacks). Aquí se cruzan tecnología y mitología.

## 1. Implantes míticos (injertos)

**Desbloqueo:** investigación vanilla de bionicos/prótesis (según el implante) **+** un proyecto propio de la rama
"Injertos del Dao" (Comprensión del Dao). Se instalan como operaciones vanilla (RecipeDef de cirugía), pero **solo en
mobiliario médico propio** (ver §3). Cada implante tiene **disonancia celestial** (stat configurable): los injertos de
bestia tienen poca; los híbridos con tecnología, más.

| Injerto | Parte | Efecto (idea) | Material clave |
|---|---|---|---|
| **Huesos del Dao** (道骨) | Esqueleto (hediff de cuerpo) | +velocidad de cultivo, +calidad de fundamento al avanzar | Médula de bestia de nivel alto + jade espiritual |
| **Piel del Qilin** | Torso (piel) | Armadura, resistencia a fuego y toxinas | Escamas doradas de Qilin (evento/linaje) |
| **Venas de Dragón** (龙脉) | Sistema circulatorio | +regeneración de qi y de sangre-qi | Sangre de dragón / jiao |
| **Brazo del Simio Divino** (通臂猿猴) | Brazo | Fuerza c. a c. enorme, alcance | Hueso de bestia simia (Fauna) |
| **Piernas del Corcel de Mil Li** (千里马) | Piernas | Velocidad de movimiento muy alta | Tendones de bestia veloz |
| **Columna del Xiniu** (犀牛, rinoceronte mítico) | Columna | Capacidad de carga, resistencia al aturdimiento | Cuerno de Xiniu |
| **Ojos de Mil Li** (千里眼) | Ojo | Visión/puntería a gran distancia | Ojo de Gudiao (águila) |
| **Tercer Ojo Celestial** (como Erlang Shen) | Frente (nuevo) | Ve auras y reinos, detecta lo invisible, ataque de luz | Cristal espiritual + ojo de bestia élite |
| **Orejas que oyen con el viento** (顺风耳) | Oreja | Detecta asaltos antes; +percepción | Oreja de bestia nocturna |
| **Corazón de Fénix** | Corazón | Regeneración; una resurrección | Pluma/corazón de Bifang o Fénix |
| **Pulmones de Dragón** | Pulmones | Inmune a gases; aliento | Pulmón de bestia de fuego |
| **Riñones de Jade** | Riñones | +qi Yin, cultivo nocturno | Jade lunar |
| **Colmillos del Taotie** | Mandíbula | Devorar: cura al comer carne; ataque de mordida | Colmillo de jefe Taotie |
| **Piel de la Campana Dorada** | Piel | Absorbe daño como el escudo de qi, pasivo | Bronce espiritual + técnica |

### 1b. Injertos de oficio (brazos y manos para el trabajo)
Además de los de combate, injertos que mejoran el trabajo, como los bionicos vanilla pero con temática wuxia:

| Injerto | Mejora |
|---|---|
| **Brazo del Agricultor Divino** (神农, como Shennong) | Siembra, cosecha y corte de plantas más rápidos; más rendimiento; +Plantas |
| **Brazo del Minero del Dao** | Velocidad y rendimiento de minería; +Minería |
| **Mano del Forjador Celestial** | Velocidad y calidad en forja y fabricación; +Fabricación |
| **Mano del Alquimista** | Calidad de píldoras y medicinas; menos toxicidad; +Intelectual/Medicina |
| **Mano del Sanador** | Calidad de tratamiento y cirugía; +Medicina |
| **Brazo del Constructor de Montañas** | Velocidad de construcción, capacidad de carga; +Construcción |
| **Dedos del Artista de Jade** | Calidad de arte y tallado de jade; +Arte |
| **Brazo del Pastor de Bestias** | Doma, entrenamiento y producción animal; +Animales |

## 2. Genética de bestias (Biotech)

- **Investigación "Sangre de bestia"** (rama Injertos/Linajes): permite **extraer genes de bestia** de núcleos y sangre
  de bestias espirituales en una **cuba de sangre** (equivalente al extractor de genes): salen **genepacks de bestia**
  (afinidad elemental, piel dura, visión nocturna, colmillos…).
- **Los genes de linaje** (Dragón, Fénix…) se pueden **extraer** de un portador con el extractor vanilla y ensamblar en
  un xenogermen (más caro y con riesgo de rechazo por el Dao: hediff temporal).
- **Ensamblador del Dao** (variante del ensamblador vanilla) para genes de cultivo: raíz espiritual artificial
  ("Raíces espirituales expuestas"), sangre purificada.
- **Infusión de sangre de bestia** como xenogermen wuxia (BIOTECH_WUXIA §4).

### 2b. Esencias: extraer cuesta una vida (decisión 2026-10-15)
- Los genes de linaje y de bestia se manejan como **esencias** (objetos contenedores: "Esencia de sangre de dragón",
  "Esencia de bestia de fuego"…), no como genepacks normales.
- **Para extraer una esencia hace falta una persona o bestia viva, y el donante muere** en la extracción (en la Cuba de
  Sangre / Altar). Queda la esencia.
- La esencia se **infunde** a otro colono (Altar de Injerto o Cuba de Sangre) y le otorga ese gen/poder.
- **Moral con Ideology:** pensamientos de humor como la extracción de órganos (verlo/hacerlo) y un **precepto propio**
  (prohibida / aborrecible / aceptada); el camino demoníaco y la Luna Sangrienta la aprueban.
- Da un uso (y un dilema) a los prisioneros y a las bestias espirituales capturadas.

## 3. Mobiliario médico propio (tecno-cultivo)

| Mueble | Para | Notas |
|---|---|---|
| **Altar de Injerto del Dao** | Instalar injertos míticos | Cama médica + formación: requiere piedras espirituales; da bonus de éxito con un cirujano cultivador |
| **Cuba de Sangre** | Extraer genes de bestia | Equivalente al extractor de genes; con Biotech |
| **Ensamblador del Dao** | Ensamblar genes de cultivo | Variante del ensamblador vanilla |
| **Horno de refinado de huesos** | Preparar huesos, médulas, escamas | Procesa partes de bestia en materiales de injerto |
| **Cámara de estabilización** [Tecno] | Bajar la disonancia celestial de un implante mecánico | Qi-Tech: electricidad + cristal |

## 4. Prioridad
1. Altar de Injerto + 4 injertos (Huesos del Dao, Venas de Dragón, Ojos de Mil Li, Piernas del Corcel).
2. Cuba de sangre + genepacks de bestia (Biotech).
3. Resto de injertos (dependen de partes de bestias de Fauna & Flora y jefes).
4. Ensamblador del Dao y cámara de estabilización (Qi-Tech).

## Implementación v0.1 (mod `Mods/Convergence.Grafts`, 2026-10-16)
Mod aparte con DLL mínima (`Source/Convergence.Grafts`: solo los workers de receta). Depende del Core; materiales de Resources/Fauna con `MayRequire`. Sin investigación industrial ni electricidad.
- **Estructura vanilla:** cada injerto = `HediffDef` (`AddedBodyPartBase` si sustituye una parte, `ImplantHediffBase` si se añade), `ThingDef` hijo de `BodyPartBase` (con `recipeMaker`) y `RecipeDef` de instalación
  (`SurgeryInstallBodyPartArtificialBase` / `SurgeryInstallImplantBase`). Los stats van en `stages[0].statOffsets` del hediff, con `CVG_CelestialDissonance` pequeña (0,1-0,2).
- **Injertos (12):** de oficio — Brazo del Agricultor Divino, Brazo del Minero del Dao, Mano del Forjador Celestial, Mano del Alquimista, Mano del Sanador, Brazo del Constructor de Montañas, Dedos del Artista de Jade, Brazo del Pastor de Bestias;
  de bestia — Huesos del Dao (columna; cultivo y avance), Venas de Dragón (corazón; qi y sangre-qi), Ojos de Mil Li (ojo; puntería, eficiencia 1,4), Piernas del Corcel de Mil Li (pierna; eficiencia 1,5). Lore tal cual de `docs/lore/INJERTOS.md`.
- **Mobiliario:** `CVG_BoneRefiningFurnace` (mesa donde se fabrican) y `CVG_GraftingAltar` (cama médica con `SurgerySuccessChanceFactor` 1,6). Las recetas de instalación usan `Recipe_InstallGraft`/`Recipe_InstallGraftImplant`:
  **solo están disponibles si hay un altar en el mapa**; además `surgerySuccessChanceFactor` base 0,6 (el altar compensa).
- **Investigación** (rama del Dao): `CVG_Research_DaoGrafting` (tras Templado del cuerpo) → de oficio, horno y altar; `CVG_Research_BeastGrafting` → de bestia.
- **Pendiente:** bonus de éxito con cirujano cultivador; +skill real (hoy solo velocidad/calidad por stats); resto de injertos (§1), Cuba de sangre, esencias, Ensamblador del Dao, cámara de estabilización; arte propio (rutas `Things/Item/Health/CVG/<Nombre>`, `Things/Building/CVG/Grafts/<Nombre>`).
- Prueba: `-cvggraftstest` fabrica/instala cada injerto en un colono y comprueba que cada stat sube, y que sin altar no hay instalación.
