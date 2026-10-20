# Linaje de la Tortuga Negra — Black Tortoise (玄武, Xuánwǔ)

> Ficha de diseño y lore. Base aprobada: [RAZAS.md](../RAZAS.md). Convenciones comunes, escala de qi y
> purificación general: [README.md](README.md).
> **Papel:** defensa y longevidad. **Afinidad:** agua y tierra. **Cuerpo:** defensa enorme, longevidad extrema,
> resistencia a toxinas y enfermedades.

---

## 1. Lore

### Origen mítico
**Xuanwu** (玄武, "Guerrero Oscuro"), la Tortuga Negra del Norte, es una tortuga con una **serpiente enroscada**
(龟蛇合体): el agua, el invierno, la tierra firme y la longevidad. En el taoísmo se convirtió en el dios guerrero
**Zhenwu** (真武大帝). Es el cuarto de los Cuatro Seres Benévolos (四灵, *Liji*: la tortuga, símbolo de vida larga).
La mitología añade una imagen central: cuando **Nüwa** reparó el cielo roto, usó las patas de una tortuga gigante
(鳌, *áo*) como columnas para sostenerlo. La tortuga es lo que impide que el cielo caiga.

### Historia en Jiuxiao
Los xuanwu de Jiuxiao eran **los pilares del cielo**: vivían en el Norte Profundo (北冥, Beiming) y sostenían con
sus caparazones las islas flotantes donde se asentaban las sectas. Eran lentos, silenciosos y casi eternos:
algunos recordaban la fundación de Jiuxiao.
- **En la Calamidad de Hierro**, **Beiming Laozu** (北冥老祖) se mantuvo mil años bajo el cielo agrietado.
  Cuando la Gran Ruptura rompió el reino, los pilares de las otras bestias cayeron, pero él sostuvo un fragmento
  entero, llamado después el **Pilar**. Esa isla sigue flotando en algún lugar del Borde: es el hogar secreto
  del linaje.
- Por eso consideran que **su deber es proteger**, no atacar. No le enseñan a nadie a pelear; le enseñan a
  aguantar.

### Cómo llegó su sangre al Borde
1. **Los Pilares Caídos.** Cuando un fragmento rozó el Borde, un caparazón colosal cayó en un desierto y se
   convirtió en una **montaña**. Los pueblos que vivieron a su sombra heredaron **Rastros**: los llaman
   "descendientes de la montaña".
2. **Los Guardianes de Puertas.** Algunos xuanwu ofrecieron servir como guardas de fortalezas y ciudades; su sangre
   pasó a familias de capitanes, y son los **Diluida**.
3. **Imperio / Royalty.** Los guardias de élite de ciertas casas imperiales tienen Diluida o Pura y no saben
   por qué su dinastía "aguanta tanto".
4. **Ascension Dynamics.** Quiere la longevidad: su eslogan es "vivir 500 años".

### Relación con las sectas y los otros linajes
| Con | Relación |
|---|---|
| **Azure Peak Sword Sect** | Aliados de convicción: los guardias del Pico son a menudo Rastro/Diluida |
| **Silent Lotus Temple** | Protegen sus templos y hospitales |
| **Ancient Dragon Clan** | Amigos de milenios: "el dragón ataca, la tortuga aguanta" |
| **Heavenly Cauldron Pavilion** | Los consideran "buenos clientes": compran píldoras de longevidad |
| **Blood Moon Cult** | Los codicia: la sangre de tortuga es la más barata de extraer y la más larga de rendir |
| **Qilin** | Aliados: el Qilin cura, la Tortuga protege |
| **Fénix** | Respeto: calor eterno y frío eterno se respetan |
| **Zorro** | Paciencia fría: la ilusión rara vez funciona con alguien que no cambia de opinión |
| **Facción propia** ❓ | **Northern Abyss Citadel** (北冥城): ciudad-fortaleza de bajo perfil; neutral; refugio de linajes |

### Cultura y costumbres
- **La paciencia como virtud (忍).** Una frase común: "Si no puedes ganar, resiste hasta que el mundo cambie".
- **Duermen mucho y profundo.** Una siesta de un día no es pereza, es "restitución del Pilar".
- **Terquedad ritual:** cada decisión se toma en tres noches. Por eso cambiar de planes les cuesta.
- **Peso y carga:** los niños llevan una piedra del tamaño de su puño colgada del cuello; cada año se cambia por
  otra más grande.
- Saludo: se llevan el puño a la frente y se inclinan lentamente.

### Figuras legendarias
- **Beiming Laozu** (北冥老祖, "Ancestro del Abismo Norte"). Sostuvo el Pilar durante la Ruptura. Dicen que sigue
  vivo, dormido, y que despertará cuando el cielo vuelva a agrietarse.
- **Shen Yuan** (沈渊, "Abismo Silencioso"). Guardián de Puertas de la Ciudad del Norte. Se le atribuye haber
  resistido solo un asedio de tres meses sin dormir; murió sentado, aún en guardia.

---

## 2. Rasgos por pureza

| | **Rastro** (piel dura) | **Diluida** | **Pura** | **Ancestral** |
|---|---|---|---|---|
| Armadura afilada / contundente | +5% / +4% | +12% / +10% | +25% / +20% | **+40% / +35%** |
| Daño recibido (IncomingDamageFactor) | ×0,97 | ×0,90 | ×0,80 | **×0,60** |
| Resistencia al dolor (PainShockThreshold) | +5% | +15% | +30% | **+50%** |
| Toxinas / veneno (ToxicResistance) | +0,1 | +0,3 | +0,6 | **+1,0 (inmune)** |
| Entorno tóxico (ToxicEnvironmentResistance) | — | +0,2 | +0,5 | **+0,8** |
| Inmunidad (ImmunityGainSpeed) | ×1,1 | ×1,3 | ×1,6 | **×2,0** |
| Vida / envejecimiento (LifespanFactor) | ×1,5 | ×2,0 | ×3,0 | **×5,0** |
| Resistencia a aturdir (StaggerDurationFactor) | — | ×0,8 | ×0,5 | **×0 (inmune a aturdimiento, empujes y derribos)** |
| Qi máx. / regen | +5% / — | +10% / +5% | +20% / +10% | **+40% / +20%** |
| Capacidad innata | — (latente) | **Caparazón de Xuanwu** | + **Serpiente Guardiana** | **Montaña Inamovible** |
| **Rasgo de identidad: Paso lento** (MoveSpeed; presente en TODAS las purezas, compensado con defensa y aguante) | ×0,75 | ×0,70 | ×0,85 | **×0,90** *(leve)* |
| **Contra: Terquedad** (aprendizaje GlobalLearningFactor; humor al cambiarle el trabajo) | ×0,95 / −2 | ×0,85 / −4 | ×0,9 / −2 | — |
| **Contra: Sueño pesado** (RestFallRateFactor y tarda en despertar) | — | ×1,5 | ×1,3 | — |

Regla del usuario (ajuste 2026-10-15): la Tortuga **camina lento en todas las purezas** (−25% / −30% / −15% / −10%); en **Ancestral** no se la puede empujar, aturdir ni derribar. Es el único contra que conserva Ancestral (leve). Terquedad y sueño pesado
desaparecen.

---

### Rol en la colonia: minero y constructor

- **Aptitud** (gen de pureza, siempre activa, también antes de la Convergencia): **Minería y Construcción** +1 / +2 / +3 / +4 (Rastro → Ancestral).
- **Penalización** (mismo gen, sabor): **Artesanía** -2 / -2 / -1 / -1 (manos pesadas: "una tortuga, ¿cómo fabrica cosas?"). Sin pasión.
- **Pasión** en esas dos habilidades: menor en Pura, mayor en Ancestral (solo sube, nunca baja).
- **Stats de rol** (hediff de sangre, tras la Convergencia; Rastro / Diluida / Pura / Ancestral):
  - Velocidad de minería ×1,15 / ×1,3 / ×1,5 / ×1,8
  - Rendimiento de minería ×1,1 / ×1,2 / ×1,35 / ×1,5
  - Capacidad de carga ×1,2 / ×1,4 / ×1,6 / ×2
  - Armadura (ya en §2); sigue lenta al caminar en todas las purezas

---

## 3. Genes de Biotech

### 3.1 Identidad (pureza)
| defName | Label (EN) | Efecto | Cpx / Met / Arc |
|---|---|---|---|
| `CVG_Blood_Tortoise_Trace` | Trace of Black Tortoise Blood | Nivel 1 | 1 / 0 / 0 |
| `CVG_Blood_Tortoise_Diluted` | Diluted Black Tortoise Blood | Nivel 2 | 2 / 0 / 0 |
| `CVG_Blood_Tortoise_Pure` | Pure Black Tortoise Blood | Nivel 3 | 3 / 0 / 0 |
| `CVG_Blood_Tortoise_Ancestral` | Ancestral Black Tortoise Blood | Nivel 4 | 4 / 0 / 0 |

### 3.2 Cuerpo / aspecto / afinidad
| defName | Label (EN) | Efecto | Cpx/Met/Arc | Aspecto vanilla a reutilizar |
|---|---|---|---|---|
| `CVG_Tortoise_Shell` | Shell skin | Piel con patrón de caparazón | 0/0/0 | `Skin_SlateGray` / `Skin_InkBlack` (override); el patrón hexagonal = **arte propio** |
| `CVG_Tortoise_Ridges` | Shell ridges | Crestas faciales (borde del caparazón) | 0/0/0 | `FacialRidges` + `Brow_Heavy` |
| `CVG_Tortoise_Eyes` | Deep dark eyes | Ojos negros profundos | 0/0/0 | `Eyes_Gray` oscurecido (negro profundo = arte propio) |
| `CVG_Tortoise_Serpent` | Serpent mark | Marcas de serpiente en brazos/cola | 0/0/0 | `Tail_Smooth` (`BodyAttachments/SmoothTail`) como **cola de serpiente** (Pura/Ancestral); **sin su bono de Manipulación** |
| `CVG_Tortoise_Build` | Stout build | Cuerpo ancho | 0/0/0 | `Body_Fat` o `Body_Hulk` + `Jaw_Heavy` |
| `CVG_Tortoise_Hair` | Ink hair | Pelo negro/gris, barba en ancianos | 0/0/0 | `Hair_InkBlack`, `Hair_Gray`, `Hair_SnowWhite`; `Beard_Always` en el Anciano |
| `CVG_Tortoise_WaterEarth` | Affinity: Water & Earth | +10% al efecto de técnicas defensivas de agua/tierra | 0/0/0 | (icono propio) |

Por pureza: Rastro = Ridges + Eyes; Diluida = + Shell (patrón en brazos) + Build; Pura = + Serpent;
Ancestral = todos + patrón completo (arte propio) + aura de piedra (fleck).

---

### Genes de sangre por niveles
Comunes (los 5 de todos los linajes) y firma: **Caparazón (I–IV), Sueño de piedra (II–IV), Serpiente guardiana (III–IV)**. Suben un nivel al purificar. Valores y reglas: [README §7](README.md).

---

## 4. Capacidades innatas

| Capacidad | Pureza | Efecto | Coste | Enfriamiento | Escalado | Comps genéricos |
|---|---|---|---|---|---|---|
| **Xuanwu's Shell** · Caparazón de Xuanwu (玄武甲) | Diluida | **Escudo** en uno mismo o un aliado (alcance 6) durante 30 s: armadura afilada/contundente +50%, daño recibido ×0,5, velocidad ×0,7 | 25 | 4 h (10 000 ticks) | Duración ×R; potencia +5% por reino | `GiveHediff` |
| **Guardian Serpent** · Serpiente Guardiana (灵蛇) | Pura | 45 s: cuando lo golpean cuerpo a cuerpo, responde con un contraataque automático (100% de su daño melee, 3 s de enfriamiento interno); los proyectiles que lo rozan se desvían 25% | 40 | 8 h (20 000 ticks) | Duración ×R; contraataque ×(1 + 0,1/reino) | buff + comp de reacción |
| **Immovable Mountain** · Montaña Inamovible (不动山) | Ancestral | 20 s: **inmóvil** (anclado). Refuerza la inmunidad a empujes, aturdimientos y derribos (que Ancestral ya tiene pasiva); aliados en radio 6 reciben daño ×0,7 y la misma inmunidad. Radio +1 por reino sobre Alma Naciente | 90 | 12 h (30 000 ticks) | Duración y radio ×R | aura + `GiveHediff` |

---

## 5. Xenotipo: `CVG_Xeno_Tortoise` — Guardián del Pilar

- **Genes:** `CVG_Blood_Tortoise_Pure` · `CVG_Tortoise_Shell` · `CVG_Tortoise_Ridges` · `CVG_Tortoise_Eyes` ·
  `CVG_Tortoise_Serpent` · `CVG_Tortoise_Build` · `Robust` · `ToxResist_Partial` · `Immunity_Strong` ·
  (sin `MoveSpeed_Slow`: la lentitud ya la da el hediff).
- **Variante:** `CVG_Xeno_Tortoise_Elder` Ancestral (Beiming Laozu, único); `CVG_Xeno_Tortoise_Guard` Diluida (guardias).
- **Facciones:** **Northern Abyss Citadel** (❓ nueva, neutral); guardias de **Azure Peak Sword Sect** (Diluida);
  guardias de templo del **Silent Lotus** (Rastro/Diluida).
- **Escenario:** "Los Guardianes del Pilar": una colonia de Tortugas Pura protegiendo un fragmento caído.

---

## 6. Textos de sabor

### Genes de identidad
| Gen | EN | ES |
|---|---|---|
| Rastro | *The mountain's shadow runs in your blood. You do not hurry.* | *La sombra de la montaña corre por tu sangre. No tienes prisa.* |
| Diluida | *Your skin is thick and slow to wound. Your sleep is deeper than a lake.* | *Tu piel es gruesa y tarda en herirse. Tu sueño es más profundo que un lago.* |
| Pura | *The serpent coils within the shell. You do not strike first, and you are never struck down.* | *La serpiente se enrosca en el caparazón. No golpeas primero y nunca caes.* |
| Ancestral | *You are a pillar of the sky. The world has leaned on you for ten thousand years and has not yet broken you.* | *Eres un pilar del cielo. El mundo se ha apoyado en ti diez mil años y aún no te ha roto.* |

### Capacidades
| Capacidad | EN | ES |
|---|---|---|
| Caparazón de Xuanwu | *A wave of black stone around you or an ally. Wounds forget to open.* | *Una ola de piedra negra a tu alrededor o el de un aliado. Las heridas olvidan abrirse.* |
| Serpiente Guardiana | *Whoever strikes you meets the serpent in the shell.* | *Quien te golpea se encuentra con la serpiente del caparazón.* |
| Montaña Inamovible | *You stop being a person. You become a mountain, and your friends stand in your shade.* | *Dejas de ser una persona. Te vuelves montaña, y tus amigos se paran a tu sombra.* |

### Contras
- *Stubborn:* "I was in the middle of something." / "Estaba en medio de algo."
- *Heavy sleeper:* "It takes three alarms to wake the Pillar." / "Hacen falta tres alarmas para despertar al Pilar."

---

## 7. Purificación

> Propuesta ❓: añadir al bestiario del módulo Fauna una **Tortuga Abisal** (鳌, *áo*, élite) para esta ficha. Si no
> se añade, se sustituye por Esencia de Serpiente de Jade + Piedra del Pilar.

| Salto | Materiales | Prueba temática |
|---|---|---|
| **Rastro → Diluida** | 3× Esencia de **Serpiente de Jade**, 1× Píldora de Purificación I, 3× Piedra espiritual de tierra | **Prueba del Peso:** cargar durante 1 día un fardo del doble de su capacidad (carga fija, sin soltarlo); si lo suelta, falla y repite al día siguiente |
| **Diluida → Pura** | 3× Esencia de **Araña de los Nueve Infiernos** (九幽蛛), 2× Píldora de Purificación II, 1× Escama de Tortuga Abisal | **Prueba del Veneno:** ser picado por la Araña bajo supervisión; 3 días de fiebre y toxina: sobrevive si resiste |
| **Pura → Ancestral** | **Núcleo de Hundun** (混沌, el caos sin rostro: la tortuga debe "contener" el caos), 3× Esencia de **Suanni** o de Tortuga Abisal, 1× Píldora de Purificación III + 1× Píldora de Estabilización | **Prueba de la Montaña:** mantenerse inmóvil 24 h dentro de una tormenta de caos en un reino secreto; cualquier movimiento voluntario = falla; la dificultad escala con el reino del colono |

---

## 8. Uso de DLC

- **Royalty:** guardias de élite de casas imperiales con Diluida; la Montaña Inamovible protege una ceremonia.
- **Ideology:** meme "Longevidad Sagrada"; precepto "Piedra de Peso" (rito de la piedra).
- **Anomaly:** Serpiente Guardiana devuelve daño al vacío.
- **Odyssey:** Montaña Inamovible sirve de ancla en plataformas inestables (sabor).
