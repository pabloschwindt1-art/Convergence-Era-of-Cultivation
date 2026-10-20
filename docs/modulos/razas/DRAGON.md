# Linaje del Dragón — Dragon (龙, Lóng)

> Ficha de diseño y lore. Base aprobada: [RAZAS.md](../RAZAS.md). Convenciones comunes, escala de qi y
> purificación general: [README.md](README.md).
> **Papel:** combate y presencia. **Afinidad:** agua y rayo. **Cuerpo:** escamas, fuerza, vida muy larga.

---

## 1. Lore

### Origen mítico
En la tradición china el dragón es el primero de los **Cuatro Seres Benévolos** (四灵, *sì líng*; el *Liji* los
nombra "qilin, fénix, tortuga y dragón"). Gobierna las aguas y las tormentas, trae la lluvia y habita
palacios bajo los ríos y los mares (los Reyes Dragón, 龙王). La tradición dice que el dragón engendra **nueve
hijos, ninguno igual a otro** (龙生九子): Suanni, Bixi, Chiwen… Es el mismo Suanni (狻猊) del bestiario del mod,
el León Celestial del Caos: *los linajes y las bestias del Borde comparten parentesco*.

### Historia en Jiuxiao
Los dragones de Jiuxiao no eran bestias: eran **señores del Mar Celestial**, el océano que sostenía los cielos
bajos. Tres milenios antes de la Calamidad de Hierro formaron el **Antiguo Clan del Dragón** (Ancient Dragon Clan,
古龙族) con los humanos que se casaban con ellos y heredaban su sangre.
- **En la Calamidad de Hierro** (铁劫) la inteligencia de máquina devoraba el qi de los océanos. **Ao Zhenhai**
  (敖镇海) abrió el Mar Celestial y lo lanzó contra ella: el agua, saturada de rayo, quemó sus circuitos.
  La victoria rompió el mar en fragmentos: parte de la **Gran Ruptura**.
- Desde entonces el Clan es **orgulloso y cerrado**: se considera el guardián del cielo roto y cree que todo
  tesoro del mundo es, en último término, parte de su hoguera de oro.

### Cómo llegó su sangre al Borde
1. **Las Lluvias de Escamas.** Cuando un fragmento del mar celestial rozó el Borde en una Convergencia menor, miles
   de escamas cayeron como granizo. Quienes las recogieron y las llevaron en amuletos conservaron su qi en la
   descendencia: de ahí vienen los **Rastros** de dragón entre humanos comunes.
2. **El exilio de Ao Lingshuang.** Una princesa del Clan cruzó una grieta huyendo de un matrimonio pactado y vivió
   en el Borde como mercenaria. Su descendencia es el origen de las casas nobles con "sangre de tormenta".
3. **Royalty / Imperio.** Algunas casas imperiales conservan, sin saberlo, Diluidas. El Imperio las considera
   un recurso y quiere "registrarlas" ❓ (ver Uso de DLC).
4. **Ascension Dynamics.** La corporación codicia sus genes: un dragón Puro vale una flota.

### Relación con las sectas y los otros linajes
| Con | Relación |
|---|---|
| **Ancient Dragon Clan** | Su hogar. Usa el xenotipo `CVG_Xeno_Dragon`. Neutral; acoge a un Puro como "pariente" y a un Ancestral como **Señor del Clan** |
| **Azure Peak Sword Sect** | Rivalidad respetuosa: la espada es "una escama mal forjada" |
| **Heavenly Cauldron Pavilion** | Les compra sangre y escamas: tensión (el Clan lo considera profanación) |
| **Blood Moon Cult** | Los caza: la sangre de dragón es el mejor ingrediente de su cultivo |
| **Fénix** | Aliados antiguos: dragón (este) y fénix (sur) en el Pacto de los Cuatro Puntos |
| **Qilin** | Respeto. Un Qilin puede reprender a un Dragón; es de los pocos que lo logran |
| **Tortuga Negra** | Amistad de milenios: el dragón ataca, la tortuga aguanta |
| **Zorro** | Desconfianza mutua: el dragón desprecia el engaño y el zorro se ríe de su orgullo |

### Cultura y costumbres
- **Tesoro (宝, *bǎo*):** cada dragón cuida un "lecho de tesoros"; se mide la dignidad de un pariente por el
  oro y el jade de su cuarto.
- **Rito del Primer Rayo:** a los 13 años el joven se para bajo una tormenta; el rayo que no lo mata lo reconoce.
- **No se inclinan ante nadie salvo el Dao celestial.** Hacer limpieza o cocinar para otros se considera una
  humillación (de ahí el contra *Orgullo*).
- Saludo: puño sobre la palma con un leve giro de muñeca "como una ola".

### Figuras legendarias
- **Ao Zhenhai** (敖镇海, "Sellador del Mar"). Primer Señor del Clan en la Calamidad de Hierro. Su cuerpo sigue
  en el fondo de un fragmento de mar: se rumorea que su **Perla Primordial** (龙珠) aún está viva.
- **Ao Lingshuang** (敖凌霜, "Escarcha-Ola"). Princesa exiliada al Borde. Se la recuerda por haber derrotado
  sola a una incursión mecanoide con un solo relámpago. *Su blood-line es el de las casas de tormenta.*

---

## 2. Rasgos por pureza

Efectos aplicados por el hediff `CVG_Bloodline_Dragon` **tras la Convergencia** (ver README §2). R = potencia
del reino. Los valores son propuestas ❓ para calibrar en playtest.

| | **Rastro** (1 escama) | **Diluida** (escamas en brazos/cuello) | **Pura** (escamas completas, cuernos) | **Ancestral** (dragón humanoide) |
|---|---|---|---|---|
| Armadura afilada | +4% | +10% | +18% | **+30%** |
| Armadura contundente | +3% | +8% | +15% | **+25%** |
| Armadura de calor | — | +5% | +10% | **+20%** |
| Daño cuerpo a cuerpo | ×1,05 | ×1,10 | ×1,20 | **×1,35** |
| Daño recibido (≈ vida) | ×1,0 | ×0,95 | ×0,88 | **×0,75** |
| Vida / envejecimiento (LifespanFactor) | ×1,0 | ×1,25 | ×1,6 | **×2,5** |
| Qi máximo / regen (CVG_QiMax / QiRegen) | +5% / — | +10% / +5% | +20% / +15% | **+35% / +30%** |
| Presencia (SocialImpact) | — | +5% | +15% | **+30%** |
| Daño de tribulación de rayo | — | — | ×0,90 | **×0,70** ❓ |
| Capacidad innata | — (latente) | **Aliento de Dragón** | + **Presión del Dragón** | + **Forma de Dragón** |
| **Contra: Orgullo** (mal humor en trabajos humildes: limpiar, cocinar, acarrear) | −2 | −6 | −3 | **—** |
| **Contra: Tesoro de Dragón** (mood si el cuarto vale menos que X de riqueza) | — | −5 (< 2000) | −3 (< 3000) | *Sabor:* +3 si > 5000, −1 si no |
| **Contra: Apetito** (hambre) | — | ×1,30 | ×1,15 | — |
| **Contra: Cazado por clanes rivales** (peso de incursiones "cazadores de dragón") | — | ×2,0 | ×1,0 | — |

Regla del usuario: **Ancestral** solo conserva el *Tesoro de Dragón* (sabor): un bonus de ánimo por tener
riqueza, y un malestar mínimo si no la tiene. Todo lo demás es premio de 5 años de purificación.

---

### Rol en la colonia: erudito y líder

- **Aptitud** (gen de pureza, siempre activa, también antes de la Convergencia): **Intelectual y Social** +1 / +2 / +3 / +4 (Rastro → Ancestral).
- **Penalización** (mismo gen, sabor): **Trato con animales** -2 / -2 / -1 / -1 (los animales le temen). Sin pasión.
- **Pasión** en esas dos habilidades: menor en Pura, mayor en Ancestral (solo sube, nunca baja).
- **Stats de rol** (hediff de sangre, tras la Convergencia; Rastro / Diluida / Pura / Ancestral):
  - Velocidad de investigación ×1,15 / ×1,3 / ×1,5 / ×1,8
  - Aprendizaje global ×1,1 / ×1,2 / ×1,3 / ×1,5
  - Impacto social (ya en §2) +0 / +5% / +15% / +30%
  - Pendiente (opcional): pequeña aura de inspiración

---

## 3. Genes de Biotech

Prefijo `CVG_`. Todos los de **identidad** llevan `exclusionTags: CVG_Bloodline` (un solo linaje por colono).
Los efectos de identidad van en el hediff; los de aspecto (cosméticos) se ven desde que se nace.

### 3.1 Identidad (pureza)
| defName | Label (EN) | Efecto | Cpx / Met / Arc |
|---|---|---|---|
| `CVG_Blood_Dragon_Trace` | Trace of Dragon Blood | Identidad Rastro; otorga hediff nivel 1 tras la Convergencia | 1 / 0 / 0 |
| `CVG_Blood_Dragon_Diluted` | Diluted Dragon Blood | Nivel 2 | 2 / 0 / 0 |
| `CVG_Blood_Dragon_Pure` | Pure Dragon Blood | Nivel 3 | 3 / 0 / 0 |
| `CVG_Blood_Dragon_Ancestral` | Ancestral Dragon Blood | Nivel 4 | 4 / 0 / 0 |

### 3.2 Cuerpo / aspecto / afinidad
| defName | Label (EN) | Efecto | Cpx/Met/Arc | Aspecto vanilla a reutilizar |
|---|---|---|---|---|
| `CVG_Dragon_Horns` | Dragon horns | Cuernos. Diluida: pequeños. Pura/Ancestral: grandes | 0/0/0 | `Headbone_MiniHorns` (`Things/Pawn/Humanlike/HeadAttachments/MiniHorns`); en Ancestral `Headbone_CenterHorn` o dos nodos MiniHorns más altos (**cuernos curvos = arte propio**) |
| `CVG_Dragon_Tail` | Scaled tail | Cola escamosa. Pura+ | 0/0/0 | `Tail_Smooth` (`BodyAttachments/SmoothTail`); **sin su bono de Manipulación vanilla** |
| `CVG_Dragon_Scales` | Dragon scales | Escamas (piel con matiz azul-verde); sin stats (los da el hediff) | 0/0/0 | Color de piel: `Skin_Blue` / `Skin_Green` (override). Brillo de escamas = arte propio |
| `CVG_Dragon_Eyes` | Serpentine eyes | Ojos de dragón | 0/0/0 | `Eyes_Red` provisional (el dorado y la pupila vertical = arte propio) |
| `CVG_Dragon_ThunderVoice` | Thunder voice | Voz grave (cosmético) | 0/0/0 | `VoiceRoar` |
| `CVG_Dragon_Mane` | Dragon mane | Pelo/barba de melena (cosmético) | 0/0/0 | `Hair_LongOnly` + `Beard_BushyOnly`; pelo `Hair_LightTeal` o `Hair_InkBlack` |
| `CVG_Dragon_WaterAffinity` | Affinity: Water & Lightning | +10% al daño de técnicas de agua/rayo del reino | 0/0/0 | (icono propio) |

Asignación por pureza: Rastro = solo `CVG_Dragon_Scales` (una mancha) y Ojos; Diluida = + Horns (chicos) + ThunderVoice;
Pura = + Tail y Horns grandes; Ancestral = todos + aura (efecto visual vanilla, `renderNodeProperties`, sin shaders).

---

### Genes de sangre por niveles
Comunes (los 5 de todos los linajes) y firma: **Escamas de dragón (I–IV), Pulmones de dragón (II–IV), Vitalidad del dragón (III–IV)**. Suben un nivel al purificar. Valores y reglas: [README §7](README.md).

---

## 4. Capacidades innatas (AbilityDef)

Escala R: Refinamiento 1,0 · Fundación 1,3 · Núcleo 1,7 · Alma 2,2 · Transformación 2,8 · Ascensión 3,5.
Coste de qi con QiMax base ≈ 100 (❓ calibrar). Enfriamiento −8% por reino adicional.

| Capacidad | Pureza | Efecto | Coste | Enfriamiento | Escalado | Comps genéricos |
|---|---|---|---|---|---|---|
| **Dragon's Breath** · Aliento de Dragón (龙息) | Diluida | Cono de 7 casillas: 14 de daño de rayo (Burn) al objetivo y 25% de aturdir 2 s; apaga fuego en el cono | 25 | 6 h (15 000 ticks) | Daño ×R; alcance +1 por reino | proyectil/cono + `Stun` |
| **Dragon's Pressure** · Presión del Dragón (龙威) | Pura | Pulso de radio 8: los enemigos de **menor reino** (o sin reino) se tambalean 4 s y sufren hediff "Presionado" 1 h (−25% movimiento y manipulación); las bestias huyen | 40 | 12 h (30 000 ticks) | Radio y duración ×R; si el reino objetivo ≥ el tuyo, resiste | área + `GiveHediff` |
| **Dragon Form** · Forma de Dragón (龙化) | Ancestral | Durante 6 h: cuerpo ×1,5, daño cuerpo a cuerpo ×1,5, armadura afilada +40%, velocidad +30%, Aliento sin enfriamiento, ignora el coste de terreno. Al terminar: "Cansancio del Dragón" 1 día (−20% a todo) | 120 | 15 días | +1 h por reino | buff (`GiveHediff`) + cambio de render |

Comps que ya pide el Core: proyectil, área, buff canalizado, movimiento. Nombres de comps vanilla a confirmar al implementar.

---

## 5. Xenotipo: `CVG_Xeno_Dragon` — Descendiente del Dragón

- **Genes:** `CVG_Blood_Dragon_Pure` · `CVG_Dragon_Horns` · `CVG_Dragon_Tail` · `CVG_Dragon_Scales` · `CVG_Dragon_Eyes` ·
  `CVG_Dragon_ThunderVoice` · `Body_Hulk` ligeros (opcional) · `Robust` · `Immunity_Strong` · `Hair_Grayless`.
- **Variante de líder:** `CVG_Xeno_Dragon_Elder` con `CVG_Blood_Dragon_Ancestral` (solo líderes del Clan y jefe de incursión).
- **Facciones:** **Ancient Dragon Clan** (miembros Pura, líder Ancestral); mercenarios dragón sueltos (Diluida) en
  Cultivadores errantes; algunos nobles del Imperio (Diluida, escondida).
- **Escenario:** "Heredero del Clan Caído": colonos de pureza Pura sin Clan.

---

## 6. Textos de sabor

### Genes de identidad
| Gen | EN | ES |
|---|---|---|
| Rastro | *A single scale beneath the skin. The storm is dreaming of you.* | *Una sola escama bajo la piel. La tormenta sueña contigo.* |
| Diluida | *Dragon blood thinned by generations, yet it still answers thunder.* | *Sangre de dragón aguada por generaciones, pero aún responde al trueno.* |
| Pura | *Horns, scale and storm: the blood of the Ancient Dragon Clan runs true in this body.* | *Cuernos, escama y tormenta: la sangre del Antiguo Clan del Dragón corre sin mezcla en este cuerpo.* |
| Ancestral | *A sovereign of the Celestial Sea in human form. Even the Heavens lower their voice.* | *Un soberano del Mar Celestial en forma humana. Hasta los cielos bajan la voz.* |

### Capacidades
| Capacidad | EN | ES |
|---|---|---|
| Aliento de Dragón | *Exhale a cone of lightning-charged water. Flames die where it passes.* | *Exhala un cono de agua cargada de rayo. Las llamas mueren a su paso.* |
| Presión del Dragón | *Release your aura. The weak forget how to stand; the beasts forget how to hunt.* | *Libera tu aura. Los débiles olvidan cómo sostenerse; las bestias, cómo cazar.* |
| Forma de Dragón | *For a few hours, become the thing the ancestors were. Afterwards, sleep like the dead.* | *Durante unas horas, vuelve a ser lo que fueron los ancestros. Después, duerme como un muerto.* |

### Contras (descripción de los pensamientos)
- *Pride:* "I am the descendant of the Sea-Sealer. I should not be sweeping floors." / "Soy descendiente del Sellador del Mar. No debería estar barriendo suelos."
- *Hoard:* "My room is not worthy of my blood." / "Mi cuarto no es digno de mi sangre."

---

## 7. Purificación

Pasos y reglas generales: [README §4](README.md). Materiales **específicos del Dragón**:

| Salto | Materiales | Prueba temática |
|---|---|---|
| **Rastro → Diluida** | 3× Esencia de sangre de **Serpiente de Jade**, 1× Píldora de Purificación I, 5× Escama de dragón menor (o Escama de Lluvia, evento) | **Prueba del Primer Rayo:** soportar 1 día bajo una tormenta artificial/natural; si el colono tiene Corazón del Dao < 50% hay riesgo de quemadura grave |
| **Diluida → Pura** | 3× Esencia de **Jiuying** (九婴, agua y fuego), 2× Píldora de Purificación II, 1× Perla de agua (reino secreto de mar) | **Prueba de la Marea:** inmersión de 2 días en agua helada en cultivo; duelo con una Bestia de la Tribulación |
| **Pura → Ancestral** | **Núcleo de Qiongqi** (穷奇, tigre alado, jefe de los Cuatro Malvados), 3× Esencia de **Suanni**, 1× Píldora de Purificación III + 1× Píldora de Estabilización | **Prueba del Cielo Roto:** ascender a un reino secreto del Mar Celestial y mantener la forma de dragón 1 h sin ceder al miedo; falla = quedas Pura con "Sangre desgarrada" 10 días |

Notas: tras Ancestral el colono recibe la **Perla de Dragón** (龍珠, ítem de persona, sin hueco) como recompensa
narrativa. ❓ Decisión: ¿Qiongqi o un tesoro del propio Mar Celestial para Ancestral?

---

## 8. Uso de DLC

- **Royalty:** casas nobles con Dragón Diluida; la Presión del Dragón se siente como un psycast de pánico.
- **Ideology:** meme "Linaje Sagrado"; precepto "Tesoro de Dragón" (cuartos con riqueza = +ánimo).
- **Anomaly:** el aliento de dragón daña al vacío (×1,5).
- **Odyssey:** a Pura se le permite sobrevivir en el espacio sin traje por un breve tiempo (sabor, no mecánica).
