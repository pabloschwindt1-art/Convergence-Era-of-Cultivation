# Linaje del Fénix — Phoenix (凤凰, Fènghuáng)

> Ficha de diseño y lore. Base aprobada: [RAZAS.md](../RAZAS.md). Convenciones comunes, escala de qi y
> purificación general: [README.md](README.md).
> **Papel:** supervivencia y fuego. **Afinidad:** fuego. **Cuerpo:** regeneración, inmune al calor, sufre el frío.

---

## 1. Lore

### Origen mítico
El *fenghuang* (凤凰: *feng* el macho, *huang* la hembra) es el segundo de los **Cuatro Seres Benévolos** (四灵).
No nace de huevos comunes, no se posa en cualquier árbol ni bebe de cualquier fuente: solo aparece cuando el
mundo está en paz y gobierna un rey justo. El *Shan Hai Jing* (山海经) lo coloca en la **Montaña Danxue**
(丹穴山), y su cuerpo lleva escritas las cinco virtudes (德义礼仁信) en cabeza, alas, espalda, pecho y vientre.
Su pariente cercano es el **Zhuque** (朱雀, Pájaro Bermellón del Sur) y las aves de fuego del bestiario
(**Bifang** 毕方).

> **Nota de autenticidad.** En China el fénix no muere ni renace entre llamas; esa imagen viene de Occidente y del
> concepto budista de *niepan* (涅槃, nirvana). **En el mod es un préstamo deliberado:** "Nirvana" es la técnica
> secreta que Jiuxiao desarrolló al mezclar la sangre fénix con la doctrina del renacimiento.

### Historia en Jiuxiao
Los fénix de Jiuxiao eran **los jueces del Cielo Meridional**. Se posaban en la Corte de Cinabrio (丹庭) y
solo bajaban cuando una era terminaba para anunciar la siguiente. Cuando la Calamidad de Hierro empezó, ningún
fénix descendió: la corte esperó señales de paz que nunca llegaron.
- **Feng Mingluan** (凤鸣鸾) decidió que la paz no llegaría y se arrojó al corazón de la inteligencia de
  máquina. Ardió nueve veces y nueve veces renació dentro de ella hasta quemarle el núcleo. Esa técnica,
  **Nirvana** (涅槃), se convirtió en el secreto de la sangre fénix.
- Tras la Gran Ruptura, la Corte se dispersó. Los fénix perdieron el hogar y se volvieron **vagabundos**:
  consejeros, médicos, jueces itinerantes. Respetados, pero sin territorio.

### Cómo llegó su sangre al Borde
1. **Los Fuegos del Cielo.** Las llamas fénix cayeron como estrellas fugaces en una Convergencia menor y se
   enterraron como cenizas ricas en qi. Los humanos que las respiraron o las bebieron en té quedaron con **Rastros**.
2. **La Compañía del Nirvana.** Un grupo de fénix errantes cruzó una grieta persiguiendo una bestia de fuego y se
   quedó como sanadores de pueblo: sus hijos son los **Diluida**. El Borde los llama "los que no se enferman".
3. **Imperio / Royalty.** El Imperio colecciona "ceniza viva" para sus curadores psiónicos ❓.
4. **Ascension Dynamics.** Estudia la regeneración: si descifra el Nirvana, puede fabricar soldados que no mueren.

### Relación con las sectas y los otros linajes
| Con | Relación |
|---|---|
| **Heavenly Cauldron Pavilion** | Su hogar adoptivo: los fénix Diluida y Pura son los "Guardianes de la Llama" de los calderos. Usan el xenotipo `CVG_Xeno_Phoenix` |
| **Silent Lotus Temple** | Colaboración: sanación y regeneración en las mismas manos |
| **Ancient Dragon Clan** | Aliados históricos (Pacto de los Cuatro Puntos); pero el Clan lo trata como "pájaro delicado" |
| **Azure Peak Sword Sect** | Los fénix juzgan sus duelos; no aceptan armas manchadas de ira |
| **Blood Moon Cult** | Enemigos mortales: ven en el Nirvana la inmortalidad robable |
| **Qilin** | "Qilin y fénix juntos" (麟凤) es el signo mayor de buen augurio; mutua veneración |
| **Tortuga Negra** | Respeto mutuo: la tortuga es "frío eterno", el fénix "calor eterno" |
| **Zorro** | Rivalidad: el **fuego del zorro** (狐火) es una imitación; el fénix lo considera un fuego falso |

### Cultura y costumbres
- **Rito de las Cinco Virtudes:** cada fénix recita al amanecer las cinco virtudes de su cuerpo; si olvida una,
  va a un retiro de silencio.
- **Ceniza:** se guardan las cenizas de los muertos en jarras de cinabrio; una vez al año se esparcen al viento
  del sur "para que el fuego recuerde".
- **No mienten bajo juramento** (el fuego del fénix lo detecta) y se les pide testificar en conflictos.
- **Cuidado de los niños:** tienen la costumbre de abrigarlos mucho aunque el niño no sienta frío, por su propia
  aversión al invierno.
- Saludo: palma abierta con los dedos hacia arriba, como una llama.

### Figuras legendarias
- **Feng Mingluan** (凤鸣鸾, "Fénix que Canta en la Pira"). La que quemó el núcleo de la Calamidad. Se dice que
  aún arde en una brasa del fragmento de Danxue y que su fuego da Nirvana a quien lo merece.
- **Chi Yun** (赤昀, "Sol Carmesí"). El fénix errante que cayó al Borde con la primera Convergencia y fundó
  la compañía de sanadores. Murió y renació doce veces; sus últimas cenizas están enterradas en un lugar sin nombre.

---

## 2. Rasgos por pureza

R = potencia por reino (README §2). Valores propuestos ❓.

| | **Rastro** (plumas de ceniza) | **Diluida** | **Pura** | **Ancestral** |
|---|---|---|---|---|
| Curación de heridas (InjuryHealingFactor) | ×1,10 | ×1,5 | ×2,0 | **×3,0** |
| Regenera miembros perdidos | — | — | lentamente (1 parte / 10 días) | **1 parte / 3 días** |
| Velocidad de inmunidad (ImmunityGainSpeed) | ×1,1 | ×1,3 | ×1,6 | **×2,0** |
| Armadura de calor | +5% | +15% | +30% | **+50%** |
| Inflamabilidad (Flammability) | ×1,0 | ×0,7 | ×0,35 | **×0,0** |
| Calor cómodo máx. (ComfyTemperatureMax) | — | +10 | +25 | **+40** |
| Vida / envejecimiento (LifespanFactor) | ×1,0 | ×1,2 | ×1,5 | **×2,0** |
| Qi: regen / máx. | — | +5% / +5% | +20% / +10% | **+35% / +20%** |
| Daño de técnicas de fuego | — | +5% | +15% | **+30%** |
| Capacidad innata | — (latente) | **Llamas del Fénix** | + **Nirvana** (1 uso) | **Nirvana Eterna** (cada 3 años) |
| **Contra: Frío** (ComfyTemperatureMin; sufre antes el frío) | +5 | +15 | +10 | **+5** *(leve)* |
| **Contra: Temperamento explosivo** (MentalBreakThreshold) | — | +0,06 | +0,03 | — |
| **Contra: Crisis incendiaria** (rotura mental puede prender fuego a su alrededor) | — | sí (radio 2) | 30% de las veces | — |
| **Contra: Orgullo del fuego** (−3 humor si trabaja en el frío/de noche en interior oscuro) | — | −4 | −2 | — |

Regla del usuario: **Ancestral** solo conserva la aversión al frío, leve (+5). El Nirvana pasa de "un uso" a
renovable. El resto de contras desaparece.

### Detalle del Nirvana (涅槃)
- **Pura (una vez en la vida):** si el colono muere por cualquier causa, arde, revive tras 2 h con la salud al
  50% y sin heridas mortales. Radio 4: quema a lo que esté cerca (amigos incluidos). Consume el don: queda
  `CVG_NirvanaSpent`. ❓ Decisión: se puede recargar con una **Pluma de Fénix** legendaria.
- **Ancestral (recarga):** cada **3 años de juego (180 días)**; −30 días por cada 2 reinos desde Núcleo Dorado.
  Al revivir: salud al 80%, sin penalización. Mismo mecanismo que "Alma Naciente sobrevive a la muerte del cuerpo"
  (reutilizar el comp de Core).
- El Nirvana no cuenta como tribulación (❓ ajuste: ¿puede el Nirvana salvar de una tribulación mortal?).

---

### Rol en la colonia: artesano y cocinero

- **Aptitud** (gen de pureza, siempre activa, también antes de la Convergencia): **Artesanía y Cocina** +1 / +2 / +3 / +4 (Rastro → Ancestral).
- **Penalización** (mismo gen, sabor): **Plantas** -2 / -2 / -1 / -1 (su fuego marchita). Sin pasión.
- **Pasión** en esas dos habilidades: menor en Pura, mayor en Ancestral (solo sube, nunca baja).
- **Stats de rol** (hediff de sangre, tras la Convergencia; Rastro / Diluida / Pura / Ancestral):
  - Velocidad de trabajo general (fabricar, forjar, sastrería…) ×1,15 / ×1,3 / ×1,5 / ×1,8
  - Velocidad de cocina ×1,15 / ×1,3 / ×1,5 / ×1,8
  - **Maestro de la forja** (herrería, banco de fabricación y la futura Forja Espiritual): velocidad de forja ×1,2 / ×1,4 / ×1,7 / ×2,2 y calidad +0,25 / +0,5 / +1 / +2 niveles (Ancestral: casi siempre excelente o más). Es el "Dragón para investigar" del metal.
  - Calor: armadura de calor e inflamabilidad de §2 (Ancestral: inflamabilidad 0)

---

## 3. Genes de Biotech

### 3.1 Identidad (pureza)
| defName | Label (EN) | Efecto | Cpx / Met / Arc |
|---|---|---|---|
| `CVG_Blood_Phoenix_Trace` | Trace of Phoenix Blood | Nivel 1 del hediff | 1 / 0 / 0 |
| `CVG_Blood_Phoenix_Diluted` | Diluted Phoenix Blood | Nivel 2 | 2 / 0 / 0 |
| `CVG_Blood_Phoenix_Pure` | Pure Phoenix Blood | Nivel 3 | 3 / 0 / 0 |
| `CVG_Blood_Phoenix_Ancestral` | Ancestral Phoenix Blood | Nivel 4 | 4 / 0 / 0 |

### 3.2 Cuerpo / aspecto / afinidad
| defName | Label (EN) | Efecto | Cpx/Met/Arc | Aspecto vanilla a reutilizar |
|---|---|---|---|---|
| `CVG_Phoenix_Plumage` | Phoenix plumage | Pelo en tonos rojo/dorado como plumas | 0/0/0 | `Hair_BrightRed`, `Hair_LightOrange`, `Hair_Blonde` (se elige al azar); cresta = `Ears_Pointed` (`HeadAttachments/PointedEars`) como provisional; **plumas reales = arte propio** |
| `CVG_Phoenix_Tail` | Tail feathers | Plumas de cola (Pura/Ancestral) | 0/0/0 | `Tail_Furry` (`BodyAttachments/FurryTail`) teñida por el color del pelo, **sin su bono de frío** |
| `CVG_Phoenix_Eyes` | Golden eyes | Ojos dorados | 0/0/0 | `Eyes_Red` provisional (el dorado = arte propio) |
| `CVG_Phoenix_Skin` | Ember skin | Piel cálida, apenas rojiza | 0/0/0 | `Skin_PaleRed` (override) / `Skin_DeepRed` en Ancestral |
| `CVG_Phoenix_FlameMarks` | Flame marks | Marcas de llama en cuello/mejillas | 0/0/0 | **Arte propio** (sin equivalente vanilla; con Ideology se puede probar un tatuaje facial) |
| `CVG_Phoenix_FireAffinity` | Affinity: Fire | +10% al daño de técnicas de fuego | 0/0/0 | (icono propio) |

Por pureza: Rastro = Plumage (mechón) + Eyes; Diluida = + Skin + Tail (pequeña); Pura = + FlameMarks + Tail mayor;
Ancestral = todos + halo de calor (fleck).

---

### Genes de sangre por niveles
Comunes (los 5 de todos los linajes) y firma: **Plumas ígneas (I–IV), Nacido de las cenizas (II–IV), Nirvana (III–IV)**. Suben un nivel al purificar. Valores y reglas: [README §7](README.md).

---

## 4. Capacidades innatas

| Capacidad | Pureza | Efecto | Coste | Enfriamiento | Escalado | Comps genéricos |
|---|---|---|---|---|---|---|
| **Phoenix Flames** · Llamas del Fénix (凤焰) | Diluida | Proyectil de fuego dorado: 16 de daño de Flame en radio 2 y enciende al objetivo; **no daña ni quema a aliados** | 25 | 4 h (10 000 ticks) | Daño ×R; +1 radio en Núcleo y Transformación | proyectil + área |
| **Nirvana** · Nirvana (涅槃) | Pura | **Pasiva** (hediff comp): al morir, resurge en 2 h con 50% de salud; llamas de radio 4 (daño 30 al entorno). Una vez en la vida | 0 (pasiva) | Una vez | Salud al revivir ×(1 + 0,1·reino−1) | comp "al morir" + fuego |
| **Eternal Nirvana** · Nirvana Eterna (永涅) | Ancestral | Como Nirvana, pero **renovable cada 3 años** (−30 días cada 2 reinos desde Núcleo) y revive al 80%, y las llamas curan a los aliados cercanos | 0 (pasiva) | 180 días | Ver arriba | idem |

Opcional ❓ de Ancestral: **Alas de Fénix** (vuelo corto de 20 casillas, 60 qi, 8 h), si hay presupuesto.

---

## 5. Xenotipo: `CVG_Xeno_Phoenix` — Guardián de la Llama

- **Genes:** `CVG_Blood_Phoenix_Pure` · `CVG_Phoenix_Plumage` · `CVG_Phoenix_Tail` · `CVG_Phoenix_Eyes` ·
  `CVG_Phoenix_Skin` · `FireResistant` (vanilla, cuerpo) · `Immunity_Strong` · `Superclotting` · `Beauty_Pretty`.
- **Variante errante:** `CVG_Xeno_Phoenix_Wanderer` con Diluida (médicos itinerantes, comerciantes).
- **Líder (Pabellón del Caldero Celestial):** `CVG_Xeno_Phoenix_Elder` con `CVG_Blood_Phoenix_Ancestral` (único).
- **Facciones:** **Heavenly Cauldron Pavilion** (guardianes de caldero), Cultivadores errantes (médicos), escenario
  "Ceniza Viva" (colonia con un fénix Pura sin hogar).

---

## 6. Textos de sabor

### Genes de identidad
| Gen | EN | ES |
|---|---|---|
| Rastro | *A warm ash in the veins. In winter you are always the last to shiver.* | *Una ceniza tibia en las venas. En invierno, eres el último en temblar.* |
| Diluida | *The old fire wakes in your wounds and closes them. You hate the cold.* | *El antiguo fuego despierta en tus heridas y las cierra. Odias el frío.* |
| Pura | *Feathers of the Southern Court. Death is a door you have already walked through once.* | *Plumas de la Corte del Sur. La muerte es una puerta que ya has cruzado una vez.* |
| Ancestral | *Born of Danxue's pyre. The fire remembers you, and returns you to the world every time.* | *Nacido de la pira de Danxue. El fuego te recuerda y te devuelve al mundo cada vez.* |

### Capacidades
| Capacidad | EN | ES |
|---|---|---|
| Llamas del Fénix | *Hurl a golden flame that burns only enemies and spares your friends.* | *Lanza una llama dorada que quema solo a tus enemigos y perdona a tus amigos.* |
| Nirvana | *If you fall, you burn. And from the ash, you rise once, with half your strength.* | *Si caes, ardes. Y de la ceniza te levantas una vez, con la mitad de tu fuerza.* |
| Nirvana Eterna | *The Heavens count your deaths in years, not in lives.* | *Los cielos cuentan tus muertes en años, no en vidas.* |

### Contras
- *Cold:* "The cold feels like an insult." / "El frío se siente como un insulto."
- *Hot-tempered:* "My blood is boiling, and I am about to prove it." / "Me hierve la sangre, y estoy a punto de demostrarlo."

---

## 7. Purificación

| Salto | Materiales | Prueba temática |
|---|---|---|
| **Rastro → Diluida** | 3× Esencia de **Grulla Celestial**, 1× Píldora de Purificación I, 3× Ceniza de Cinabrio | **Prueba de la Pira:** sentarse 1 h dentro de un fuego controlado (daño de calor creciente); si el colono huye, falla |
| **Diluida → Pura** | 3× Esencia de **Bifang** (毕方), 2× Píldora de Purificación II, 1× Pluma de Fénix menor (reino secreto) | **Prueba de la Ceniza:** "morir" a propósito en coma ritual 2 días (riesgo real de muerte, con 70% de éxito base +Corazón del Dao); al despertar recibe el primer Nirvana |
| **Pura → Ancestral** | **Núcleo de Taotie** (饕餮, el devorador: el fénix debe "quemar al hambre de mundo"), 3× Esencia de **Jiuying**, 1× Píldora de Purificación III + 1× Píldora de Estabilización | **Prueba de Danxue:** viaje al reino secreto de la Montaña Danxue; recitar las Cinco Virtudes sin vacilar ante cinco ilusiones (una por virtud). Falla = vuelve Pura, "Quemadura espiritual" 15 días |

❓ Decisión: ¿Taotie para Fénix, o dejar Taotie para otro uso (final de juego) y usar una Pluma Inmortal (tesoro legendario)?

---

## 8. Uso de DLC

- **Royalty:** curadores nobles; fénix Ancestral cuenta como psylink natural.
- **Ideology:** meme "Cenizas Sagradas"; precepto "Quemar a los muertos" da +ánimo.
- **Biotech:** los hijos heredan la pureza del progenitor de mayor pureza (regla común, README §3.1).
- **Anomaly:** las llamas dañan al vacío ×1,3.
