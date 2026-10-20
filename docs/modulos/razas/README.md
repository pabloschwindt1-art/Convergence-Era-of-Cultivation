# Linajes de sangre — índice y reglas comunes

Fichas de diseño y lore de los 5 linajes de la primera versión del módulo **Razas** (ver [../RAZAS.md](../RAZAS.md),
que es la base aprobada y no se contradice). Mundo y glosario: [../../LORE.md](../../LORE.md). Bestias para
la purificación: [../FAUNA_FLORA.md](../FAUNA_FLORA.md).

| Linaje | Ficha | Papel | Afinidad |
|---|---|---|---|
| Dragón 龙 | [DRAGON.md](DRAGON.md) | Combate y presencia | Agua, rayo |
| Fénix 凤凰 | [FENIX.md](FENIX.md) | Supervivencia y fuego | Fuego |
| Qilin 麒麟 | [QILIN.md](QILIN.md) | Apoyo y fortuna | Tierra |
| Tortuga Negra 玄武 | [TORTUGA.md](TORTUGA.md) | Defensa y longevidad | Agua, tierra |
| Zorro 狐 | [ZORRO.md](ZORRO.md) | Mente y encanto | Fuego, mente |

Dragón, Fénix, Qilin y Tortuga son los **Cuatro Seres Benévolos** (四灵, *Liji*). El Zorro es "el invitado": ni
sagrado ni demonio.

---

## 1. Tabla comparativa

Valores del nivel **Ancestral** (y Diluida entre paréntesis) para comparar. Todos son propuestas ❓ a calibrar.

| | Dragón | Fénix | Qilin | Tortuga | Zorro |
|---|---|---|---|---|---|
| Armadura (afilada) | **+30%** (+10%) | — | — | **+40%** (+12%) | — |
| Daño recibido | ×0,75 | — | — | **×0,60** | ×1 (frágil en bajas) |
| Daño melee | ×1,35 | — | ×1 | — | — |
| Curación | — | **×3,0** | tendido +50% | inmunidad ×2 | — |
| Vida (LifespanFactor) | ×2,5 | ×2,0 | ×2,0 | **×5,0** | ×4,0 |
| Qi máx. / regen | +35% / +30% | +20% / +35% | +25% / +30% | +40% / +20% | +30% / +30% |
| Rasgo único | Presencia, Forma de Dragón | Nirvana (revivir) | Fortuna, anti-mal | Inmunidad a toxinas y aturdimiento | Belleza +4, 9 colas |
| 1ª capacidad (Diluida) | Aliento de Dragón | Llamas del Fénix | Bendición del Qilin | Caparazón de Xuanwu | Encanto |
| 2ª capacidad (Pura) | Presión del Dragón | Nirvana (1 vez) | Pezuña Purificadora | Serpiente Guardiana | Ilusión |
| Suprema (Ancestral) | Forma de Dragón | Nirvana Eterna | Dominio Auspicioso | Montaña Inamovible | Forma de Nueve Colas |
| Contras en bajas | Orgullo, Tesoro, Apetito, Cazado | Frío, Explosivo, Crisis incendiaria | Pacifista, Cadáveres | Paso lento (−25/−30%), Terquedad, Sueño | Frágil, Desconfianza, Travieso |
| **Único contra Ancestral** | Tesoro (sabor) | Frío leve | Ejecuciones | Paso lento leve (−10%) | Desconfianza leve |
| Facción | Ancient Dragon Clan | Heavenly Cauldron Pavilion | Silent Lotus Temple | Northern Abyss Citadel ❓ | Qingqiu Court ❓ |
| Malvado (Pura→Ancestral) | Qiongqi | Taotie | Taowu (purificado) | Hundun | Perla del Zorro ❓ |
| Peso de aparición ❓ | 0,8 | 0,8 | 0,5 | 1,0 | 1,2 |

---

## 2. Convenciones técnicas comunes

- **Nombres:** gen de identidad `CVG_Blood_<Linaje>_<Trace|Diluted|Pure|Ancestral>`; aspecto `CVG_<Linaje>_<Parte>`;
  hediff `CVG_Bloodline_<Linaje>` (4 etapas); habilidad `CVG_Ability_<Nombre>`; xenotipo `CVG_Xeno_<Linaje>`;
  pensamiento `CVG_Thought_<Nombre>`.
- **Un solo linaje:** todos los genes de identidad llevan `exclusionTags: CVG_Bloodline`. Los de aspecto, el tag de
  su zona (cuernos, cola, ojos, orejas…) para evitar choques con genes vanilla.
- **Hediff aplicado tras la Convergencia** (igual que los reinos): contiene los stats, capacidades y contras de cada
  etapa. Antes de ella solo hay gen (identidad + aspecto). Sin Biotech: rasgo como identidad.
- **Biostats de identidad:** Cpx 1 / 2 / 3 / 4 (Rastro→Ancestral), Met 0, Arc 0. Los genes de aspecto: 0/0/0.
- **Stats nuevos que propone esta serie** (para el Core): `CVG_TribulationDamageFactor` (daño recibido de tribulación),
  `CVG_QiMax`, `CVG_QiRegen`. Los otros son stats vanilla verificados (ArmorRating_*, MeleeDamageFactor,
  IncomingDamageFactor, LifespanFactor, InjuryHealingFactor, ImmunityGainSpeed, ToxicResistance, ToxicEnvironmentResistance,
  PainShockThreshold, StaggerDurationFactor, MeleeDodgeChance, PawnBeauty, SocialImpact, NegotiationAbility, etc.).
- **"Vida ×N"** del enunciado ("MaxHitPoints") se implementa como `IncomingDamageFactor` (el stat `MaxHitPoints` no
  aplica a los pawns). Hambre = `hungerRateFactor` de la etapa del hediff.
- **Capacidades** = `AbilityDef` vanilla con coste de qi (comp de Core), `GiveHediff` y comps de proyectil, área, buff
  canalizado y movimiento del Core. Las pasivas (Nirvana) = comp del hediff con Harmony en `Pawn.Kill`.

### Escala de qi y reino
QiMax base de Refinamiento ≈ 100 (❓ calibrar con M3). **R (potencia por reino):**

| Reino | 1 Refinamiento | 2 Fundación | 3 Núcleo | 4 Alma | 5 Transformación | 6 Ascensión |
|---|---|---|---|---|---|---|
| R | 1,0 | 1,3 | 1,7 | 2,2 | 2,8 | 3,5 |

Enfriamiento: −8% por reino adicional. Duración de efectos: ×R.

### Aspecto: arte vanilla de Biotech que se reutiliza
Se crean GeneDefs propios cuyo `renderNodeProperties` copia el `texPath` vanilla (no se hereda de los genes vanilla):

| Pieza | Gen vanilla | texPath |
|---|---|---|
| Cuernos pequeños | `Headbone_MiniHorns` | `Things/Pawn/Humanlike/HeadAttachments/MiniHorns/MiniHorns` |
| Cuerno central | `Headbone_CenterHorn` | `HeadAttachments/CenterHorn/CenterHorn` |
| Orejas de gato/zorro | `Ears_Cat` | `HeadAttachments/CatEars/CatEars` |
| Orejas puntiagudas | `Ears_Pointed` | `HeadAttachments/PointedEars/PointedEars` |
| Cola peluda | `Tail_Furry` | `BodyAttachments/FurryTail/FurryTail` (se repite en abanico) |
| Cola lisa (reptil/serpiente) | `Tail_Smooth` | `BodyAttachments/SmoothTail/SmoothTail` |
| Piel de color | `Skin_Blue/Green/DeepRed/PaleRed/Orange/DeepYellow/PaleYellow/SlateGray/InkBlack` | override de color |
| Ojos | `Eyes_Red`, `Eyes_Gray` | (no hay dorado) |
| Pelo | `Hair_SnowWhite`, `Hair_BrightRed`, `Hair_LightOrange`, `Hair_Blonde`, `Hair_InkBlack`, `Hair_Gray`, `Hair_LightTeal` | colores de Core |
| Crestas | `FacialRidges`, `Brow_Heavy`, `Jaw_Heavy`, `Head_Gaunt` | |

**Arte propio pendiente (cuello de botella):** ojos dorados/ámbar y pupila vertical, escamas con brillo, plumas, marcas de
llama, patrón de caparazón, abanico de colas, cuernos curvos. Con el arte provisional ya se puede jugar.

---

## 3. Reglas generales

### 3.1 Herencia y mezcla
- Un hijo hereda el linaje del progenitor de **mayor pureza**; si empatan, al azar. Puede "saltar" un nivel hacia abajo (15%)
  o, si ambos progenitores son del mismo linaje, hacia arriba (3%). ❓ Decisión.
- Una sangre puede **extraerse** (xenogerm) desde Diluida en adelante; implantarla en un portador de otro linaje
  **reemplaza** su sangre tras un rechazo severo ❓. La Ascensión Dynamics lo explota.
- Hijos de linajes distintos: gen dominante único (nunca dos linajes).

### 3.2 Aparición en el mundo
Probabilidad base: Rastro ~3%, Diluida ~1%, Pura ~0,3% (más común en su facción), Ancestral ~0,03% (casi solo por
purificación). Se reparte entre linajes con el peso de la tabla. Opcional ❓: afinidad por bioma (fuego en cálidos,
tortuga en húmedos).

### 3.3 Cuándo se ven los rasgos
Aspecto físico desde que se nace (Rastro sólo ojos/pelo; de Diluida en adelante, cuernos/colas/plumas).
Stats y capacidades: **solo tras la Convergencia**. ❓ Decisión: ¿mostrar el aspecto de Rastro en la ficha?

---

## 4. Purificación común

- **Ritual:** *Rito de Purificación de Sangre* en un altar de meditación (lugar de cultivo) con el colono sentado;
  dura 1 día y consume los materiales al inicio. Cada salto requiere un **reino mínimo** ❓:
  Rastro→Diluida: Fundación; Diluida→Pura: Núcleo Dorado; Pura→Ancestral: Alma Naciente.
- **Materiales por salto:** Esencia de Sangre de bestias (módulo Fauna; ver cada ficha), **Píldoras de Purificación I /
  II / III** (módulo Alquimia), **Píldora de Estabilización** (Ancestral), y para Ancestral un **tesoro legendario**
  (núcleo de un Malvado o reliquia).
- **Estado de purificación (`CVG_BloodPurging`):** hediff temporal de 2–5 días con dolor alto y −30% de conciencia;
  la "prueba temática" lo agrava. Resultado:
  | Éxito | Falla leve | Falla grave |
  |---|---|---|
  | Sube de nivel (cambia el gen de identidad y el hediff) | Pierde materiales, queda "Sangre agitada" 5 días | Pérdida de materiales + herida grave, o muerte si el Corazón del Dao es bajo ❓ (la tribulación puede matar; usar el mismo ajuste) |
  Probabilidad base 70% + Corazón del Dao y MedicalTendQuality del que asiste.
- **Códigos de esencia:** `CVG_Item_BloodEssence_<Bestia>` (Serpiente de Jade, Grulla Celestial, Lobo de Viento, Araña de los
  Nueve Infiernos, Suanni, Bifang, Gudiao, Jiuying, Tortuga Abisal ❓). Los tesoros: núcleos de Qiongqi / Taotie / Taowu /
  Hundun y la Perla del Zorro.
- **Pruebas temáticas:** Dragón = rayo y marea; Fénix = pira y muerte; Qilin = no matar; Tortuga = peso, veneno e inmovilidad;
  Zorro = ilusiones y engaño.

---

## 5. Decisiones abiertas (❓)

1. **Nuevas facciones menores:** *Northern Abyss Citadel* (Tortuga) y *Qingqiu Court* (Zorro). Alternativa: guardarlas
   dentro de Azure Peak y Rogue Cultivators.
2. **Bestia nueva** *Tortuga Abisal* (鳌) en el bestiario Fauna, para purificar a la Tortuga.
3. **Tesoros legendarios:** el Malvado asignado a cada linaje (Qiongqi / Taotie / Taowu / Hundun) y la Perla del Zorro.
4. **Valores numéricos** (propuestas) y calibración de qi/reino.
5. **Herencia, extracción de sangre y rechazo** (3.1).
6. **Visibilidad del Rastro** y afinidad por bioma (3.3, 3.2).
7. **Nirvana:** recarga de la versión Pura con Pluma legendaria; si salva de una tribulación mortal.
8. **Cuándo es obligatoria la prueba** (¿sólo Ancestral?) y si la falla grave puede matar.

## 6. Relaciones entre linajes

| | Dragón | Fénix | Qilin | Tortuga | Zorro |
|---|---|---|---|---|---|
| **Dragón** | — | Aliados (Pacto) | Respeto | Amigos | Desconfianza |
| **Fénix** | Aliados | — | Veneración mutua | Respeto | Rivalidad (fuego falso) |
| **Qilin** | Respeto | Veneración mutua | — | Aliados | Tolerancia |
| **Tortuga** | Amigos | Respeto | Aliados | — | Paciencia fría |
| **Zorro** | Desconfianza | Rivalidad | Tolerancia | Paciencia fría | — |

**Por qué las sangres se repelen (lore):** la sangre de cada bestia lleva una *armonía del Dao* (道韵) propia,
sellada en el **Pacto de los Cuatro Puntos** (dragón-este, fénix-sur, tortuga-norte, qilin-centro). Dos armonías
en un mismo cuerpo se anulan: el cuerpo conserva solo la dominante. El Zorro, ajeno al Pacto, hereda igual pero sin
sello: "el invitado" que no puede sentarse a dos mesas a la vez.

---

## 7. Implementación: mod `Convergence: Races`

**Estructura (decisión: Core = framework, cada módulo = mod aparte).**

| Dónde | Qué |
|---|---|
| Core `Source/Convergence.Core/Defs/BloodlineDefs.cs` | `BloodlinePurityDef` (niveles, probabilidad base), `BloodlineDef` (afinidades, `commonality`, lista ordenada de `purities`: gen, rasgo sin Biotech, hediff, `abilities`, `aspectGenes`, `chanceFactor`), `DefModExtension_Bloodline` |
| Core `Bloodline/BloodlineUtility.cs` | Detectar el linaje (genes activos o rasgo), `SetBloodline`, `TryPurify` (API de purificación), generación aleatoria en `PawnGenerator`, `SyncEffects` (hediff + capacidades **solo tras la Convergencia**) |
| Core `Abilities/AbilityComps.cs` | Comps genéricos: `CompProperties_AbilityQiCost` (cobra qi solo a quien cultiva qi), `CompProperties_AbilityCone` (cono de daño, aturdir, apagar fuego), `CompProperties_AbilityGiveHediffInRadius` (auras y buffs). Escalan con la potencia R del reino |
| Core `Common/Defs/BloodlinePurities.xml` | Rastro 3% · Diluida 1% · Pura 0,3% · Ancestral 0,03%. El ajuste "Probabilidad de nacer con sangre de bestia" las multiplica |
| `Mods/Convergence.Races/` | Solo XML. `Bloodline_Bases.xml` (bases abstractas, categoría de gen y de habilidad) y un `Bloodline_<Linaje>.xml` por linaje: su `BloodlineDef`, 4 genes de identidad, genes de aspecto, el hediff `CVG_Bloodline_<Linaje>` (4 etapas con los valores del §2 de cada ficha), la 1ª capacidad y el xenotipo `CVG_Xeno_<Linaje>` (pureza Pura). Traducciones Spanish y SpanishLatin en `DefInjected` (el inglés va en los Defs) |

**Reglas que aplica el Core.** Un linaje por colono (`exclusionTag CVG_Bloodline`; si hubiera dos, gana la mayor pureza).
Los genes de aspecto se **acumulan** por pureza y uno de pureza mayor reemplaza al que choca (1 → 3 → 6 → 9 colas;
cuernos chicos → grandes; pelo naranja → plateado). Al dar un aspecto se quitan los genes cosméticos vanilla que chocan
(cuernos, orejas, cola, color de piel o de pelo), porque el juego elige al azar entre ellos. Los genes que agrega el linaje
son xenogenes solo si el gen de identidad lo era.

### Sangre y raíz (decisión 2026-10-15, ver RAZAS.md)
- **Afinidad:** cada pureza tiene `affinityCultivationFactor` (XML; hoy ×1,05 / ×1,1 / ×1,2 / ×1,35). Multiplica la velocidad
  de cultivo si la raíz espiritual tiene alguno de los elementos del linaje (`affinities`). La pestaña Cultivo la muestra
  como activa o inactiva.
- **Ancestral limpia la raíz:** al llegar a Ancestral **por purificación** (`TryPurify`), la raíz sube un grado (gen de
  calidad con Biotech, grado del rasgo sin él); sin raíz, despierta una raíz inferior del primer elemento del linaje. Solo
  una vez por colono (flag `bloodRootRefined` guardado en `CompCultivator`). Dar Ancestral directamente con *Give bloodline*
  no lo dispara.
- Afinidad del Dragón: el rayo no es un elemento del Core; por ahora solo agua.

### Qué hace cada gen
- **Gen de pureza** (`CVG_Blood_<Linaje>_<Pureza>`): identidad. Su descripción se **genera** desde el hediff y el
  `BloodlineDef` (`BloodlineText`, parche de `GeneDef.DescriptionFull`): stats y contras de esa pureza, capacidades innatas,
  afinidad y si están activos o latentes en la partida. La pestaña Cultivo lista los mismos efectos bajo el linaje.
- **Genes de aspecto**: cosméticos por diseño (las fichas lo dicen: "sin stats, los da el hediff"); su descripción lo aclara.
  Excepciones con efecto propio: `CVG_Dragon_ThunderVoice` (voz de rugido) y `CVG_Tortoise_Build` (cuerpo Hulk).
- **Revisión contra las fichas:** los genes de afinidad (`CVG_Dragon_WaterAffinity`, `CVG_Phoenix_FireAffinity`,
  `CVG_Qilin_EarthAffinity`, `CVG_Tortoise_WaterEarth`, `CVG_Fox_FireMind`) **no se crean**: la afinidad es del linaje
  (`affinities` + `affinityCultivationFactor` por pureza); el +10% al daño de técnicas por elemento queda para M5.
  `CVG_Fox_Grace` no es gen: la belleza va en el hediff (PawnBeauty +1..+4). `CVG_Phoenix_FlameMarks` está dentro de
  `CVG_Phoenix_Skin` (marcas = arte propio). `CVG_Qilin_Voice` y `CVG_Fox_Voice` (voz humana) no se crean: no harían nada.

### Rol en la colonia (identidad de cada linaje)
Hook de forja del Core: las mesas con `Convergence.DefModExtension_Forge` (parche sobre `FueledSmithy`, `ElectricSmithy`,
`FabricationBench`; la futura Forja Espiritual solo tiene que llevar la extensión) usan `CVG_ForgeSpeed` (StatPart sobre la
velocidad de sus recetas) y `CVG_ForgeQuality` (niveles extra de calidad, hasta obra maestra). Cualquier rasgo, técnica o
linaje puede subirlos.

Criterio del usuario: cada linaje es **claramente el mejor en su rol** ("si querés investigar fuerte, vas por un dragón").
Aptitudes en el gen de pureza (+1/+2/+3/+4, siempre activas); penalización temática en el mismo gen (-2/-2/-1/-1, sin pasión);
pasión menor en Pura y mayor en Ancestral en las dos habilidades buenas. Stats de rol en el hediff (tras la Convergencia),
valores Rastro / Diluida / Pura / Ancestral:

| Linaje | Rol | Aptitudes | Penalización | Stats de rol |
|---|---|---|---|---|
| Dragón | Erudito y líder | Intelectual, Social | Trato con animales | Investigación ×1,15 / ×1,3 / ×1,5 / ×1,8 · Aprendizaje ×1,1 / ×1,2 / ×1,3 / ×1,5 · Impacto social +0 / +5% / +15% / +30% |
| Fénix | Artesano y cocinero | Artesanía, Cocina | Plantas | **Forja** (herrería, banco de fabricación, forja espiritual): velocidad ×1,2 / ×1,4 / ×1,7 / ×2,2 y calidad +0,25 / +0,5 / +1 / +2 niveles (Ancestral: casi siempre excelente o más) · Velocidad de trabajo general ×1,15 / ×1,3 / ×1,5 / ×1,8 · Velocidad de cocina ×1,15 / ×1,3 / ×1,5 / ×1,8 |
| Qilin | Sanador y agricultor | Plantas, Medicina | Cuerpo a cuerpo | Calidad de tratamiento +15% / +30% / +45% / +60% · Cosecha ×1,1 / ×1,2 / ×1,3 / ×1,5 · Movimiento ×1,05 / ×1,1 / ×1,15 / ×1,25 |
| Tortuga Negra | Minero y constructor | Minería, Construcción | Artesanía | Velocidad de minería ×1,15 / ×1,3 / ×1,5 / ×1,8 · Rendimiento de minería ×1,1 / ×1,2 / ×1,35 / ×1,5 · Carga ×1,2 / ×1,4 / ×1,6 / ×2 · armadura (§2); sigue lenta |
| Zorro | Negociador y artista | Social, Arte | Minería | Negociación +15% / +30% / +50% / +80% · Precios de comercio +5% / +10% / +15% / +20% · Impacto social +10% / +25% / +50% / +100% · Esquiva |

La pasión es genérica (`BloodlinePurity.passion`): sube las habilidades con aptitud positiva del gen de identidad (la penalizada no), nunca baja una
pasión, y se aplica también a colonos generados con el xenotipo (en la sincronización periódica).

### Genes de sangre por niveles (RAZAS.md §2d)
Todos los linajes comparten 5 **genes comunes** y cada uno tiene 3 **genes firma**; cada gen tiene niveles I–IV (Rastro →
Ancestral). Al purificar, cada gen pasa al nivel siguiente: más efecto, menos defecto. Arquitectura genérica del Core:
`BloodlineGeneGroupDef` (grupo; sin `bloodline` = común) + `DefModExtension_BloodlineGeneTier { group, tier, awakenedAbilities }`
en cada gen. `SetBloodline`/purificar deja el gen de mayor nivel de cada grupo que no supere la pureza (un grupo puede
empezar en un nivel alto). **Un gen nuevo = XML.** XML en `Mods/Convergence.Races/Common/Defs/BloodGenes_*.xml`.

**Decisión de cuándo aplican:** los stats e inmunidades de estos genes son parte del cuerpo y **aplican siempre** (también antes
de la Convergencia). La Resonancia de qi solo afecta stats de cultivo, que solo cuentan para cultivadores despiertos. Las
**mecánicas sobrenaturales** (regeneración, vidas extra, auras, contraataque y capacidades) funcionan **solo tras la
Convergencia** (`Convergence.Gene_Bloodline`).

| Gen común | I | II | III | IV |
|---|---|---|---|---|
| Vitalidad bestial | Curación ×1,1, dolor +3%; **metabolismo -1** | ×1,2, +6%; **-1** | ×1,35, +10% | ×1,5, +15% |
| Sentidos agudos | Puntería +1, vista +3%; **umbral de crisis +4%** | +2, +6%; **+3%** | +3, +10%; **+1%** | +4, +15% |
| Longevidad de sangre | Vida ×1,1; **fertilidad ×0,7** | ×1,2; **×0,8** | ×1,35; **×0,9** | ×1,5 |
| Instinto de la bestia | Melee ×1,05, esquiva +2; **peleas sociales ×2** | ×1,1, +4; **×1,6** | ×1,15, +6; **×1,3** | ×1,2, +8 |
| Resonancia de qi | Cultivo ×1,05, qi máx ×1,05; **avance -5%** | ×1,1, ×1,1; **-3%** | ×1,2, ×1,15; **-1%** | ×1,3, ×1,25 |

| Linaje | Genes firma (niveles) |
|---|---|
| Dragón | Escamas (I–IV: armadura afilada +3..+12%, daño de fuego ×0,9..×0,5) · Pulmones (II–IV: entorno tóxico +30..+100%, inmune a gas tóxico desde III) · Vitalidad del dragón (III–IV: regenera una parte o cicatriz cada 15 / 6 días) |
| Fénix | Plumas ígneas (I–IV: fuego y quemaduras ×0,75..×0, inmune en IV) · Nacido de las cenizas (II–IV: regenera cada 20 / 10 / 4 días) · **Nirvana** (III: 1 vida en la vida; IV: 1 vida que vuelve cada 180 días; estallido de fuego que no daña a aliados) |
| Qilin | Presencia auspiciosa (I–IV: aura de 6..12 casillas, ánimo +1..+4 a aliados) · Pezuñas puras (II–IV: entorno tóxico +25..+80%, fuego ×0,8..×0,4) · Repelente del mal (III–IV: entidades hostiles del vacío a 8 / 12 casillas pegan menos y van más lentas) |
| Tortuga Negra | Caparazón (I–IV: armadura afilada y contundente +3..+12%) · Sueño de piedra (II–IV: descanso ×1,1..×1,3, curación ×1,1..×1,35) · Serpiente guardiana (III–IV: 35% / 60% de morder al atacante cuerpo a cuerpo, 8 / 14 de daño) |
| Zorro | Fuego de zorro (II–IV: capacidad a distancia, 15 qi; puntería +2/+4 en III/IV) · Velo de ilusión (I–IV: esquiva +3..+12) · **Nueve colas, nueve vidas** (I–IV: 1 / 3 / 6 / 9 vidas extra; cada muerte gasta una con aviso) |

Notas: las vidas extra se reinician al subir de nivel (purificar devuelve vidas). Los xenotipos listan los genes de nivel III;
al llegar a Ancestral suben a IV. "Velo de ilusión: los enemigos prefieren otros objetivos" y "Caparazón solo en el torso"
no tienen equivalente vanilla simple: hoy son esquiva y armadura global (TODO). El Fuego de zorro también es capacidad
del gen, no del linaje.

### Metabolismo (equilibrio)
Los genes del linaje no cuestan metabolismo salvo el de pureza y Vitalidad bestial I/II (-1, el "hambre extra"); juntos fijan el total del colono: **Rastro -1, Diluida -2**
(contra leve), **Pura 0, Ancestral +1** (hambre normal o menor). En Pura/Ancestral el gen de pureza compensa los genes vanilla
del xenotipo del linaje (Dragón y Zorro -3; Fénix, Qilin y Tortuga -5), que se agregan al llegar a Pura: por eso su
`biostatMet` es +3/+4 o +5/+6. El xenotipo `CVG_Xeno_<Linaje>` suma 0. Si se edita un xenotipo, recalcular el gen de pureza.

### Capacidades
Implementada la **1ª capacidad (Diluida)** de cada linaje, con coste de qi:

| Linaje | Capacidad | Comps | Pendiente |
|---|---|---|---|
| Dragón | Aliento de Dragón: cono de 7, 14 Burn ×R, 25% de aturdir 2 s, apaga el fuego. 25 qi, 6 h | `QiCost` + `Cone` | alcance +1 por reino |
| Fénix | Llamas del Fénix: proyectil, estallido de fuego de radio 2, 16 de daño. 25 qi, 4 h | `QiCost` + `LaunchProjectile` vanilla | no dañar aliados y daño ×R (clase de proyectil propia) |
| Qilin | Bendición del Qilin: aura de 8, +4 ánimo, curación ×1,25, tratamiento +15% durante 4 h. 30 qi, 8 h | `QiCost` + `GiveHediffInRadius` + `ThoughtWorker_Hediff` | curar 1 herida leve por pulso |
| Tortuga | Caparazón de Xuanwu: uno mismo o un aliado, 30 s de armadura +50%, daño ×0,5, velocidad ×0,7. 25 qi, 4 h | `QiCost` + `GiveHediffInRadius` (radio 0,5) | potencia +5% por reino |
| Zorro | Encanto: un enemigo humano a 10 casillas queda aturdido 15 s. 30 qi, 6 h | `QiCost` + `OnlyTargetHostiles` + `Stun` + `GiveHediff` vanilla | prisionero (+reclutar), aliado (+opinión), resistencia por reino |

**TODO (M5 técnicas):** las capacidades de Pura y Ancestral (Presión y Forma de Dragón, Nirvana y Nirvana Eterna, Pezuña
Purificadora y Dominio Auspicioso, Serpiente Guardiana y Montaña Inamovible, Ilusión y Forma de Nueve Colas). Están
anotadas como comentario en cada `BloodlineDef`.

**TODO (contras que piden código):** los de humor u opinión (Orgullo y Tesoro del Dragón, Pacifista y Cadáveres del Qilin,
Terquedad de la Tortuga, Desconfianza y Travieso del Zorro), la crisis incendiaria del Fénix, la regeneración de miembros,
los cazadores de dragón, el daño de tribulación y de técnicas por elemento, la fortuna y la detección del mal. Cada hediff
los lista en un comentario. Ya funcionan los contras que son stats (hambre, frío, umbral de crisis, lentitud, aprendizaje,
sueño, fragilidad, daño cuerpo a cuerpo).

### Aspecto: ruta propia → provisional vanilla
Cada gen con dibujo lleva un comentario `Own art: <ruta>` encima de su `renderNodeProperties`. Cuando se elija el arte de
`art/generated/comfy/razas/`, copiar los PNG (`_north`/`_south`/`_east`) a `Mods/Convergence.Races/Common/Textures/<ruta propia>`
y cambiar el `texPath` del gen. Rutas propias bajo `Things/Pawn/Humanlike/CVG/`; las vanilla bajo `Things/Pawn/Humanlike/`.

| Gen | Ruta propia | Provisional vanilla (Biotech) | Ajuste provisional |
|---|---|---|---|
| `CVG_<Linaje>_Eyes` (los 5) | `CVG/<Linaje>/Eyes` | `HeadAttachments/GrayEyes/{Male,Female}/GrayEyes_*` | teñidos: dorado (Dragón), oro (Fénix), oro pálido (Qilin), oscuro (Tortuga), ámbar (Zorro) |
| `CVG_Dragon_Horns` | `CVG/Dragon/Horns` | `HeadAttachments/MiniHorns/MiniHorns` | marfil |
| `CVG_Dragon_HornsGreat` | `CVG/Dragon/HornsGreat` | `HeadAttachments/MiniHorns/MiniHorns` | ×1,35, más altos, dorados |
| `CVG_Dragon_Tail` | `CVG/Dragon/Tail` | `BodyAttachments/SmoothTail/SmoothTail` | ×1,25, color de piel (escamas) |
| `CVG_Dragon_Scales` | `CVG/Dragon/Scales` | — (solo piel `(95,150,160)`) | brillo de escamas = arte propio |
| `CVG_Phoenix_Plumage` | `CVG/Phoenix/Feathers` | — (pelo rojo llama) | plumas en el pelo = arte propio |
| `CVG_Phoenix_Crest` | `CVG/Phoenix/Crest` | `HeadAttachments/PointedEars/PointedEars` | color de pelo, ×1,1 |
| `CVG_Phoenix_Tail` / `TailGreat` | `CVG/Phoenix/TailFeathers` | `BodyAttachments/FurryTail/FurryTail` | 1 cola ×0,85 / 3 en abanico |
| `CVG_Phoenix_Skin` / `SkinAncestral` | `CVG/Phoenix/FlameMarks` | — (piel brasa / rojo profundo) | marcas de llama = arte propio |
| `CVG_Qilin_HornSmall` / `Horn` | `CVG/Qilin/Horn` | `MiniHorns` ×0,75 / `HeadAttachments/CenterHorn/CenterHorn` | dorados |
| `CVG_Qilin_Ears` | `CVG/Qilin/Ears` | `HeadAttachments/FloppyEars/FloppyEars` | color de piel |
| `CVG_Qilin_Scales` | `CVG/Qilin/Scales` | — (piel dorada) | brillo dorado = arte propio |
| `CVG_Tortoise_Ridges` | `CVG/Tortoise/ShellPattern` | `HeadAttachments/FacialRidges/FacialRidges{A,B,C}` | color de piel |
| `CVG_Tortoise_Shell` / `ShellAncestral` | `CVG/Tortoise/ShellPattern` | — (piel verde pizarra / basalto) | patrón hexagonal = arte propio |
| `CVG_Tortoise_Serpent` | `CVG/Tortoise/SerpentTail` | `BodyAttachments/SmoothTail/SmoothTail` | ×1,15, color de piel |
| `CVG_Fox_Ears` | `CVG/Fox/Ears` | `HeadAttachments/CatEars/CatEars` | color de pelo |
| `CVG_Fox_Tail1/3/6/9` | `CVG/Fox/Tails<N>` | `BodyAttachments/FurryTail/FurryTail` ×N | abanico con `rotationOffset` (±50° máx.); el abanico propio irá en una sola textura |

Colores de pelo y piel (sin textura): Dragón melena verde mar, Fénix rojo llama, Qilin blanco dorado, Tortuga negro tinta
y cuerpo Hulk, Zorro rojo anaranjado (Ancestral: plateado). Iconos provisionales: genes de identidad = iconos de elemento del
Core teñidos por pureza; capacidades = iconos de Biotech (AcidSpray, FireSpew, Resurrect, MechSmokepop, AnimalWarcall);
xenotipos = Impid, Highmate, Genie, Neanderthal, Highmate. El proyectil del Fénix usa `Things/Projectile/Spark` teñido.

### Cómo probar
1. Lista de mods: Harmony → Biotech → **Convergence: Era of Cultivation** → **Convergence: Races** (siempre después del Core).
2. Partida nueva en modo desarrollador. La pestaña **Cultivo** muestra la línea **Linaje** cuando hay un módulo de razas cargado.
3. **Aspecto y raza:** menú de debug → *Convergence: Give bloodline* → linaje → pureza → clic en un colono. Colores, cuernos,
   orejas y colas cambian al momento (antes de la Convergencia se ve el aspecto, no los efectos). La ficha muestra la raza:
   en Rastro y Diluida un baseliner pasa a "Sangre de dragón (diluida)" (xenotipo único con icono de Biotech); en Pura y
   Ancestral el colono pasa al xenotipo del linaje y recibe sus genes que falten, sin perder ninguno (raíz, cuerpo especial…).
   *Convergence: Purify bloodline* sube un nivel y actualiza el nombre o el xenotipo (Zorro: 1 → 3 → 6 → 9 colas).
4. **Colono de prueba:** *Convergence: Spawn bloodline pawn* → linaje → pureza → clic en el mapa: colono adulto del jugador
   con ese linaje (xenotipo del linaje en Pura/Ancestral; baseliner + linaje en Rastro/Diluida).
5. **Efectos:** *Convergence: Trigger Convergence*. El colono recibe el hediff "sangre de <linaje>" (pestaña Salud) y, desde
   Diluida, la capacidad innata en su barra. Con raíz espiritual la capacidad cuesta qi (*Convergence: Fill qi*); sin raíz es gratis.
6. **Xenotipos y Spawn pawn vanilla:** el *Spawn pawn* vanilla solo lista PawnKindDefs, así que hay uno por linaje:
   `CVG_Kind_Dragon`, `CVG_Kind_Phoenix`, `CVG_Kind_Qilin`, `CVG_Kind_Tortoise`, `CVG_Kind_Fox` (colono del jugador con el
   xenotipo del linaje, pureza Pura). También sirven el editor de xenotipos de Biotech y la pantalla de inicio.
7. **Generación al azar:** subir en los ajustes del Core *Probabilidad de nacer con sangre de bestia* (x10 ⇒ ~30% con Rastro)
   y generar visitantes o colonos.
8. **Autotest:** *Convergence: Bloodline self-test* (o abrir el juego con `-quicktest -cvgselftest`) da, purifica hasta el máximo
   y comprueba gen de identidad, aspecto y xenotipo de cada linaje (también spawneado y generado con el xenotipo); escribe
   el resultado en el log ("ALL OK" o los FAIL). Nota: la ventana de genes de Biotech no se refresca si queda abierta
   mientras se purifica; ciérrala y ábrela de nuevo.
9. Revisar `Player.log`: errores de XML, texturas que falten y las colas en abanico (las posiciones son una primera aproximación).
