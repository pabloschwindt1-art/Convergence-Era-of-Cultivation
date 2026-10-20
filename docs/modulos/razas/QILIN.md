# Linaje del Qilin — Qilin (麒麟, Qílín)

> Ficha de diseño y lore. Base aprobada: [RAZAS.md](../RAZAS.md). Convenciones comunes, escala de qi y
> purificación general: [README.md](README.md).
> **Papel:** apoyo y fortuna. **Afinidad:** tierra. **Cuerpo:** calma, Corazón del Dao estable, detecta el mal.

---

## 1. Lore

### Origen mítico
El *qilin* (麒麟; *qi* el macho, *lin* la hembra) es el primero de los **Cuatro Seres Benévolos** en el *Liji* (礼记).
Tiene cuerpo de ciervo, cola de buey, un cuerno carnoso y escamas de dragón. **No pisa la hierba viva ni
daña un insecto**, y aparece solo para anunciar el nacimiento o la muerte de un sabio: se dice que apareció
ante la madre de Confucio y, al final, lloró cuando el sabio murió. Es el símbolo del buen gobierno, la
benevolencia y la prosperidad (los niños-qilin 麒麟送子 traen hijos y buenos augurios).

### Historia en Jiuxiao
Los qilin de Jiuxiao eran **los arbitros de la tierra**: los jueces de los pactos entre sectas y los
pacificadores de las guerras de sucesión. Vivían en los **Valles Medios** (中谷), la región entre los cuatro
puntos, donde se concentraba el qi de la tierra.
- **En la Calamidad de Hierro**, los qilin no combatieron: **sanaron**. Se calcula que curaron a un millón de
  cultivadores heridos y absorbieron en sus cuerpos la corrupción de las máquinas. El precio: casi todos murieron
  de "veneno de hierro".
- Tras la Gran Ruptura quedó **una sola familia** de qilin en el mundo, y esa familia se dispersó para que la
  sangre no se acabara de golpe. Son la sangre **más escasa** de los cinco linajes.

### Cómo llegó su sangre al Borde
1. **La Marcha de los Cuatro Pies.** Un grupo de qilin que huía de las máquinas cruzó una grieta y se hizo pasar
   por ciervos: durante siglos vivieron como *"ciervos sagrados"* en bosques del Borde. Mestizos con humanos
   curanderos y chamanes; de ahí vienen muchos **Rastros** entre tribus (Ideology).
2. **Las Parteras.** Algunas mujeres qilin aceptaron servir como parteras de los pueblos del Borde. Los niños
   que ayudaron a nacer heredaron un hilo de su sangre: son los **Diluida**.
3. **Imperio.** El Imperio los llama "bendecidos" y los usa como testigos de ceremonias ❓.
4. **Ascension Dynamics.** Intenta replicar la "calma qilin" para fabricar cultivadores estables: falla.

### Relación con las sectas y los otros linajes
| Con | Relación |
|---|---|
| **Silent Lotus Temple** | Su hogar. Usan el xenotipo `CVG_Xeno_Qilin` como "Voz del Loto". Acoge a todos |
| **Heavenly Cauldron Pavilion** | Intentan comprar cuernos y escamas de qilin; la Voz del Loto lo prohíbe: tensión |
| **Ancient Dragon Clan** | Respeto: el Qilin es de los pocos que pueden reprender a un dragón |
| **Blood Moon Cult** | Enemigos naturales: el Qilin detecta la sangre corrompida y se niega a callar |
| **Azure Peak Sword Sect** | Neutrales; el Qilin arbitra sus duelos si piden juez |
| **Fénix** | "Qilin y fénix juntos": el mejor augurio; veneración mutua |
| **Tortuga Negra** | Aliados: el Qilin cura, la Tortuga protege |
| **Zorro** | Tolerancia desconfiada: el Qilin ve a través de sus ilusiones, el Zorro le pide que no las revele |

### Cultura y costumbres
- **No matan salvo lo estrictamente necesario.** Cada muerte es un peso en el *Corazón del Dao*: de ahí el contra
  "Pacifista".
- **Caminan despacio.** Se dice que "el Qilin no pisa la hierba viva": prefieren caminos de tierra y se enfadan
  cuando alguien pisotea cultivos.
- **Testigos del bien:** se les pide presenciar juramentos. Un Qilin que presencia una promesa la certifica.
- **Rechazan las ejecuciones** aunque el reo sea culpable; creen que la pena de muerte es el último fracaso.
- Saludo: cabeza inclinada con la palma sobre el pecho.

### Figuras legendarias
- **Lin Wenshu** (麟文述, "Pezuña sin Huellas"). El último Árbitro de los Valles Medios. Camina sin dejar huellas
  y se dice que pudo detener una guerra con un solo paso. Murió de veneno de hierro; su cuerno está en un
  templo del Loto.
- **Jin Zhi** (金芝, "Hongo Dorado"). La partera qilin que cruzó al Borde. Se le atribuye el don de saber si un
  niño será bendecido solo con tocar el vientre de la madre.

---

## 2. Rasgos por pureza

| | **Rastro** (aura serena) | **Diluida** | **Pura** | **Ancestral** |
|---|---|---|---|---|
| Estabilidad mental (MentalBreakThreshold) | −0,02 | −0,05 | −0,08 | **−0,12** |
| Corazón del Dao (estabilidad / riesgo de desviación) | +5% | +10% | +20% | **+35%** |
| Aura de ánimo (humor a aliados ≤ 8 casillas) | — | +2 | +4 | **+6** |
| Curación y buenas energías (MedicalTendQuality) | — | +15% | +30% | **+50%** |
| Velocidad de inmunidad / toxinas (ImmunityGainSpeed, ToxicResistance) | — | +10% / +0,2 | +25% / +0,4 | **+40% / +0,6** |
| Fortuna (suerte en tesoros/eventos: ver §4) | — | +5% | +10% | **+20%** |
| Detecta el mal (posesión, corrupción, demonio interno; ver §4) | — | — | detecta radio 12 | **radio 25** |
| Armadura contundente | — | +5% | +10% | **+20%** |
| Vida / envejecimiento (LifespanFactor) | ×1,0 | ×1,2 | ×1,5 | **×2,0** |
| Qi: regen / máx. | — | +5% / +5% | +15% / +15% | **+30% / +25%** |
| Capacidad innata | — (latente) | **Bendición del Qilin** | + **Pezuña Purificadora** | **Dominio Auspicioso** |
| **Contra: Pacifista** (humor por matar/herir) | −2 al matar | −6 | −3 | **Ninguno en combate** |
| **Contra: Cadáveres y ejecuciones** (humor si hay muertos/ejecuciones cerca) | — | −5 | −3 | −1 *(solo ejecuciones)* |
| **Contra: Daño al combate** (MeleeDamageFactor) | — | ×0,9 | ×0,95 | — |
| **Contra: Sensibilidad al mal** (−3 mood si un demoníaco está en su vista) | — | −3 | −2 | — |

Regla del usuario: **Ancestral** puede luchar sin penalización; solo rechaza las **ejecuciones** (−1, sabor).

---

### Rol en la colonia: sanador y agricultor

- **Aptitud** (gen de pureza, siempre activa, también antes de la Convergencia): **Plantas y Medicina** +1 / +2 / +3 / +4 (Rastro → Ancestral).
- **Penalización** (mismo gen, sabor): **Cuerpo a cuerpo** -2 / -2 / -1 / -1 (pacifista). Sin pasión.
- **Pasión** en esas dos habilidades: menor en Pura, mayor en Ancestral (solo sube, nunca baja).
- **Stats de rol** (hediff de sangre, tras la Convergencia; Rastro / Diluida / Pura / Ancestral):
  - Calidad de tratamiento +15% / +30% / +45% / +60%
  - Rendimiento de cosecha ×1,1 / ×1,2 / ×1,3 / ×1,5
  - Velocidad de movimiento ×1,05 / ×1,1 / ×1,15 / ×1,25

---

## 3. Genes de Biotech

### 3.1 Identidad (pureza)
| defName | Label (EN) | Efecto | Cpx / Met / Arc |
|---|---|---|---|
| `CVG_Blood_Qilin_Trace` | Trace of Qilin Blood | Nivel 1 | 1 / 0 / 0 |
| `CVG_Blood_Qilin_Diluted` | Diluted Qilin Blood | Nivel 2 | 2 / 0 / 0 |
| `CVG_Blood_Qilin_Pure` | Pure Qilin Blood | Nivel 3 | 3 / 0 / 0 |
| `CVG_Blood_Qilin_Ancestral` | Ancestral Qilin Blood | Nivel 4 | 4 / 0 / 0 |

### 3.2 Cuerpo / aspecto / afinidad
| defName | Label (EN) | Efecto | Cpx/Met/Arc | Aspecto vanilla a reutilizar |
|---|---|---|---|---|
| `CVG_Qilin_Horn` | Qilin horn | Cuerno corto (el qilin tiene uno, "carnoso") | 0/0/0 | `Headbone_CenterHorn` (`HeadAttachments/CenterHorn`); en Rastro/Diluida un nubarrón de `Headbone_MiniHorns` |
| `CVG_Qilin_Scales` | Golden scales | Escamas doradas en piel | 0/0/0 | `Skin_PaleYellow` / `Skin_DeepYellow` / `Skin_Orange` como matiz; **brillo dorado = arte propio** |
| `CVG_Qilin_Eyes` | Serene eyes | Ojos serenos, dorados | 0/0/0 | `Eyes_Gray` provisional (dorado = arte propio) |
| `CVG_Qilin_Ears` | Deer ears | Orejas de ciervo | 0/0/0 | `Ears_Cat` o `Ears_Floppy`, según cuál se vea mejor (`HeadAttachments/CatEars`) |
| `CVG_Qilin_Mane` | Pale mane | Pelo blanco o dorado | 0/0/0 | `Hair_SnowWhite`, `Hair_Blonde`, `Hair_SandyBlonde` |
| `CVG_Qilin_Voice` | Gentle voice | Voz suave | 0/0/0 | `Voice_Human` |
| `CVG_Qilin_EarthAffinity` | Affinity: Earth | +10% al daño y efecto de técnicas de tierra | 0/0/0 | (icono propio) |

Por pureza: Rastro = Mane + Eyes; Diluida = + Horn pequeño + Scales; Pura = + Ears + Scales visibles;
Ancestral = todos + halo dorado (fleck vanilla).

---

### Genes de sangre por niveles
Comunes (los 5 de todos los linajes) y firma: **Presencia auspiciosa (I–IV), Pezuñas puras (II–IV), Repelente del mal (III–IV)**. Suben un nivel al purificar. Valores y reglas: [README §7](README.md).

---

## 4. Capacidades innatas

| Capacidad | Pureza | Efecto | Coste | Enfriamiento | Escalado | Comps genéricos |
|---|---|---|---|---|---|---|
| **Qilin's Blessing** · Bendición del Qilin (麒麟赐福) | Diluida | Aura de 8 casillas durante 4 h: aliados +4 ánimo, +25% curación natural, +15% tendido de heridas; cura 1 herida leve por pulso | 30 | 8 h (20 000 ticks) | Duración y radio ×R | buff canalizado (aura) |
| **Purifying Hoof** · Pezuña Purificadora (净蹄) | Pura | Pisotón a distancia corta (radio 3): daño ×2 a lo demoníaco y al vacío de Anomaly; disipa maldiciones/posesión (limpia hediffs de corrupción) y "Demonio interno" de un aliado | 45 | 10 h (25 000 ticks) | Daño ×R; limpia un estado por pulso | área + `Heal`/`RemoveHediff` |
| **Auspicious Domain** · Dominio Auspicioso (吉兆领域) | Ancestral | 6 h: **todo el mapa** de la colonia gana: +10% fortuna (menos eventos malos), +20% cosecha, +15% velocidad de cultivo, −30% fatiga; los aliados no sufren penalización por muertos | 100 | 20 días | Duración +1 h por reino | buff global (hediff de mapa) |

---

## 5. Xenotipo: `CVG_Xeno_Qilin` — Voz del Loto

- **Genes:** `CVG_Blood_Qilin_Pure` · `CVG_Qilin_Horn` · `CVG_Qilin_Scales` · `CVG_Qilin_Eyes` · `CVG_Qilin_Ears` ·
  `CVG_Qilin_Mane` · `Mood_Optimist` · `Pain_Reduced` · `KindInstinct` · `ToxResist_Partial`.
- **Variante:** `CVG_Xeno_Qilin_Wanderer` Diluida (curanderos sueltos); `CVG_Xeno_Qilin_Elder` Ancestral (abadesa del Templo).
- **Facciones:** **Silent Lotus Temple** (monjes y curanderos), Cultivadores errantes (curanderos sueltos), tribus
  de bosque (sabor: "ciervos sagrados" con Rastro, Ideology).
- **Escenario:** "El Último Qilin": colonia de una sola Qilin Pura protegida por refugiados.

---

## 6. Textos de sabor

### Genes de identidad
| Gen | EN | ES |
|---|---|---|
| Rastro | *You walk softly, and the grass is grateful.* | *Caminas suave y la hierba te lo agradece.* |
| Diluida | *The kindness of the Qilin is thin in you, but it still heals what it touches.* | *La bondad del Qilin es tenue en ti, pero aún cura lo que toca.* |
| Pura | *Gold scales and a small horn: the Arbiter of the Middle Valleys speaks through you.* | *Escamas doradas y un cuerno pequeño: el Árbitro de los Valles Medios habla por ti.* |
| Ancestral | *A sage was born, or a sage has died; either way, the Qilin stands where you stand.* | *O ha nacido un sabio o ha muerto otro; en cualquier caso, el Qilin está donde tú estás.* |

### Capacidades
| Capacidad | EN | ES |
|---|---|---|
| Bendición del Qilin | *A warm calm that mends wounds and softens hearts for a few hours.* | *Una calma cálida que cierra heridas y ablanda corazones durante unas horas.* |
| Pezuña Purificadora | *Strike the ground. What is demonic, corrupt or hollow is cast out or crushed.* | *Golpea el suelo. Lo demoníaco, lo corrupto y lo vacío es expulsado o aplastado.* |
| Dominio Auspicioso | *For a few hours the whole colony is under the Qilin's good omen.* | *Durante unas horas, toda la colonia está bajo el buen augurio del Qilin.* |

### Contras
- *Pacifist:* "I did what I had to. It does not make it right." / "Hice lo que tenía que hacer. Eso no lo hace correcto."
- *Corpses:* "The air here smells of what I could not save." / "El aire aquí huele a lo que no pude salvar."

---

## 7. Purificación

| Salto | Materiales | Prueba temática |
|---|---|---|
| **Rastro → Diluida** | 3× Esencia de **Grulla Celestial** (o de ciervo despierto), 1× Píldora de Purificación I, 5× Hierba espiritual común | **Prueba de la Hierba:** cruzar un campo de hierba espiritual sin pisarla (stat de movimiento especial, 1 día de viaje); si pisa una planta, repite |
| **Diluida → Pura** | 3× Esencia de **Suanni** (狻猊, que ama el incienso: el Qilin calma al león, no lo mata), 2× Píldora II, 1× Loto de Nieve | **Prueba del Duelo del Corazón:** no matar durante 3 días de incursiones (solo defender); cada muerte provocada por el colono rompe la prueba |
| **Pura → Ancestral** | **Núcleo purificado de Taowu** (梼杌): el Qilin debe **purificar** el núcleo (no matarlo) en un ritual de 5 días; 3× Esencia de **Jiuying**; 1× Píldora III + 1× Píldora de Estabilización | **Prueba del Sabio:** presenciar un juramento entre dos facciones enemigas (misión diplomática) y guiarlo a la paz sin violencia |

---

## 8. Uso de DLC

- **Anomaly:** Pezuña Purificadora ×2 contra criaturas del vacío; puede cerrar pequeñas anomalías.
- **Ideology:** meme "Armonía con las Bestias"; precepto "Pacifismo" (+ánimo). Tribus con ciervos sagrados.
- **Royalty:** testigos de juramento: Ancestral puede "bendecir" una ceremonia de coronación (+Reputación).
- **Biotech:** vientres de crecimiento: nacer bajo un Qilin da +5% de pureza ❓.
