# Sectas y facciones (borrador v0)

Desarrolla las facciones de [../LORE.md](../LORE.md) §6 (base aprobada; no se contradice). Reinos de cultivo, glosario y
Calamidad de Hierro: ver LORE.md. Linajes de sangre: [../modulos/razas/README.md](../modulos/razas/README.md). Rasgos
y destinos: [../modulos/RASGOS.md](../modulos/RASGOS.md). Maestros que pertenecen a estas sectas: ver MAESTROS.md (no publicado: spoilers).

Marcas: **❓ Decisión** = lo aprueba el usuario. Los números son propuestas a calibrar con M3–M5.

---

## 0. Reglas comunes (decididas por el usuario, 2026-10-15)

1. **Ninguna secta es buena per se.** Las rectas y las neutrales empiezan **neutrales y justas** (buena voluntad 0; comercian,
   piden favores, se ofenden si se les falta al respeto). Tienen defectos propios, listados en cada ficha.
2. **Culto de la Luna Sangrienta** (demoníaco): hostil por defecto. Solo es amistoso si el inicio es de secta demoníaca
   **o** la mayoría de la colonia sigue una ideología con el meme/precepto **Camino Demoníaco** (ver §2.5).
   ❓ Decisión: umbral intermedio (≥25% de la colonia = neutral en vez de hostil).
3. **Ninguna es anti-tecnología.** Jiuxiao tenía su propia "tecnología" (formaciones 阵法, refinado de artefactos 炼器) y
   ve la del Borde como otra rama del mismo arte. Lo que temen es a **lo que se parece a la Calamidad de Hierro**
   (mecanoides, inteligencias que devoran qi). La postura de **tu colonia** ante implantes y tecnología la decide **tu
   Ideología**, no el mod. Por eso cada ficha propone una Ideología *representativa* de la secta, no un dogma obligatorio.
4. **Cada secta tiene reinos típicos** (rango de reino de sus miembros), **nivel tecnológico** (para `FactionDef.techLevel`
   y su equipo), **xenotipos/linajes comunes** (genes de [razas](../modulos/razas/README.md)) y **una Ideología**
   (memes y preceptos de Ideology; el mod añade memes propios `CVG_Meme_*` con XML, así que no dependemos solo de los vanilla).
5. Las sectas son facciones normales de RimWorld (`FactionDef`, grietas como "naves" de llegada). Sin Realeza/Ideología
   siguen funcionando (comercio, misiones, rangos propios); con ellas ganan títulos opcionales y ritual propio.

### Diferenciación respecto a RimImmortal
RimImmortal usa 2 facciones (justa / malvada) con títulos de Realeza obligatorios (requiere Realeza + Ideología). Convergence
usa **6 facciones de Jiuxiao con carácter propio**, **rangos propios que no exigen Realeza**, y reacciones del Borde. Las
sectas **se oponen o se ayudan entre sí** (§4), no solo contra el jugador. Cuentan una historia (la Calamidad de Hierro).

### Rangos de secta (esquema común)
Cuatro rangos más la cabeza. El jugador **entra a una secta** (o pide su apoyo) acumulando **Contribución** (el "favor" de la
facción: misiones, comercio, tributo de piedras espirituales, ayudar en su guerra).

| Rango | Contribución ❓ | Reino mínimo ❓ | Lo que abre (patrón común) |
|---|---|---|---|
| **Discípulo externo** 外门弟子 | 5 | 1 | Comerciante de la secta (precio −10%); pedir hierbas y piedras |
| **Discípulo interno** 内门弟子 | 20 | 2 (Fundación) | Tablilla de técnica básica; pedir ayuda de discípulos (2 combatientes) |
| **Discípulo núcleo** 核心弟子 | 60 | 3 (Núcleo Dorado) | Técnica firma de la secta; pedir un anciano de apoyo (1 día); pasaje a su reino secreto |
| **Anciano** 长老 | 150 | 4 (Alma Naciente) | Técnica suprema de la secta; asiento en el consejo (decide guerras); pedir "bombardeo" de la secta *(solo ancianos; caro)* |
| **Maestro de secta** 宗主 | — (no se obtiene normalmente) | 5 | Heredar la secta (final alternativo) |

- Una colonia puede ser **externa de una secta y aliada de otra**, pero rangos de **sectas enemigas son incompatibles**
  (§4). ❓ Decisión: máximo una secta de rango ≥ interno al mismo tiempo.
- Con **Royalty** activo, cada rango tiene un `RoyalTitleDef` equivalente opcional (integración, no requisito).
- Cada secta cambia **los nombres** de los rangos (en cada ficha) y el *sabor* de lo que ofrece.

### Técnicas firma por elemento
Los elementos del Core son metal, madera, agua, fuego y tierra. Cada secta tiene una **técnica firma por elemento** (pocas,
con nombre) para dar identidad a sus PNJ y a las **tablillas de técnicas** que ofrece. Las de elementos que no son su especialidad
son "técnicas prestadas" (más débiles, o simplemente "—"). Nombres propuestos; **M5 define los números** (coste de qi, escalado
con R del reino). Tablillas: *Jade Slip* 玉简.

### Tabla resumen

| Facción | Alineamiento inicial | Tecnología | Reinos típicos | Linajes comunes | Ideología representativa |
|---|---|---|---|---|---|
| Azure Peak Sword Sect 青峰剑宗 | Recta, neutral | Medieval (forja de espadas, formaciones) | 1–5 | Dragón (raro), humanos con *Corazón de Espada* | **Camino de la Espada** |
| Silent Lotus Temple 静莲寺 | Recta, neutral | Neolítico–Medieval | 1–4 | **Qilin**, Tortuga | **Loto y Vacío** |
| Heavenly Cauldron Pavilion 天鼎阁 | Neutral, comercial | Industrial (compra todo) | 1–4 (pocos combatientes) | **Fénix**, Zorro | **Gremio del Caldero** |
| Ancient Dragon Clan 古龙族 | Neutral, cerrada | Medieval | 3–6 (mínimo Núcleo) | **Dragón** | **Sangre y Tesoro** |
| Blood Moon Cult 血月教 | Demoníaca, hostil | Medieval (cirugías de sangre) | 1–5 | humanos sin linaje, *Corazón Demoníaco* | **Camino Demoníaco** |
| Rogue Cultivators 散修 | Neutral, sin bandera | Variable | 1–4 (alguno raro de 5) | Todos, azar | **Libre Albedrío** |
| El Imperio | Neutral–tenso | Imperial (vanilla) | psylinks, "no-cultivadores" | — | **Orden Psíquico** |
| Ascension Dynamics | Hostil–neutral | Spacer / Ultra | 1–3 artificiales | Sangre extraída (xenogérmenes) | **Ascensión como Producto** |

---

## 1. Facciones de Jiuxiao

### 1.1 Azure Peak Sword Sect — Secta de la Espada del Pico Azur (青峰剑宗, *Qīngfēng Jiànzōng*)

> *"La espada no es buena ni mala. Lo es la mano."* — lema inscrito en la puerta del Pico.

**Historia.** Fundada hace cuatro mil años por el Santo de la Espada **Qing Feng**, que partió una montaña de un solo tajo y
se quedó en la cima a cultivar. Fue la primera secta en formar el **Cementerio de Espadas** (剑冢), donde duermen las espadas de
todos sus ancianos. Durante la Calamidad de Hierro perdió a nueve de sus diez Ancianos; los supervivientes forjaron las
**Espadas del Cielo Roto** (破天剑) a partir de los cascarones de máquina caídos, la primera vez que "metal enemigo" entró en
la forja del Pico. Desde entonces el Pico mezcla: forjan con metal, qi y, si hace falta, con lo que traiga el Borde. Les
fascinan los **filos monomoleculares** ("una hoja que corta el vacío") y las **armas de plastiacero**.

**Doctrina / Dao.** *Camino de la Espada* (剑道): el espadachín *es* su espada; el Dao se aprende cortando y templando la
**Voluntad de Espada** (剑意). Disciplina estricta, rangos duros, honor. **Defectos:** orgullo, jerarquía rígida, desprecian
a quien no sabe empuñar un arma, y cualquier desafío al honor del Pico se responde con un duelo.

**Líderes y figuras**
| Nombre | Cargo | Nota |
|---|---|---|
| **Lin Wuchen 林无尘** ("Sin Polvo") | Maestro de secta, Alma Naciente (4) | Sereno, calculador; dice que toda espada se dobla al fin. Sabe de la Calamidad |
| **Shen Ruoxue 沈若雪** | Anciana de Espada, Núcleo Dorado cima (3) | Espada de hielo; cuida a los discípulos jóvenes con severidad |
| **Gu Hengyue 顾衡岳** | Anciano de Disciplina, Núcleo Dorado (3) | Inflexible; ejecuta los castigos de la secta. Futuro enemigo si se humilla al Pico |
| **Yan Zhao 燕昭** | Discípulo núcleo, joven prodigio (2→3) | Arrogante (*Joven Maestro Arrogante*); aparece en torneos y como rival de por vida |

**Técnicas firma**
| Elemento | Técnica | Efecto (idea) |
|---|---|---|
| Metal (especialidad) | **Sword Qi Rain** 剑雨 | Lluvia de qi cortante en cono; escala con espadas equipadas |
| Metal | **Sword Heart Clarity** 剑心通明 | Buff: precisión y crítico cuerpo a cuerpo con espada |
| Agua | **Frost Blade Dance** 寒刃舞 | Corte con ralentización; viene de Shen Ruoxue |
| Fuego | **Furnace Edge** 炉锋 | Templa el arma: daño ardiente temporal |
| Tierra | **Mountain-Splitting Strike** 开山斩 | Golpe pesado, rompe armadura y estructuras |
| Madera | — | (prestada del Templo) |

**Rangos:** Discípulo externo *(Aprendiz de Espada)* → Discípulo interno *(Portador de Espada)* → Discípulo núcleo *(Hoja del
Pico)* → Anciano *(Anciano de Espada)* → Maestro de secta. Para ascender: **vencer en duelo** a un discípulo del rango
siguiente (Torneo de sectas) y entregar una espada forjada de calidad ≥ buena.

**Qué ofrece al jugador**
- **Comercio:** espadas y armas de filo con grado espiritual; compra espadas bien forjadas, metales espirituales, filos del Borde.
- **Misiones:** *Reto por el honor* (duelo al mejor espadachín), *Torneo de sectas*, escoltar un envío de espadas, limpiar un
  nido de bestias, recuperar una espada del Cementerio robada por piratas.
- **Enseñanzas:** técnicas de espada y combate, voluntad de espada (rasgo *Corazón de Espada* más probable en discípulos).
- **Tablillas:** *Sword Qi Rain*, *Frost Blade Dance*, *Mountain-Splitting Strike*.
- **Favores (por Contribución):** ayuda de discípulos para raids, un anciano de refuerzo.

**Xenotipos/linajes.** Sobre todo humanos de buena raíz (metal/agua). Raros descendientes de dragón entre los ancianos (alianza con
el Clan). Reinos 1–5; la mayoría de sus discípulos son 1–2; un raid del Pico trae 2–3 con alguno en 3.

**Ideología representativa: *Camino de la Espada*.** Memes: Individualist o Loyalist (ver ❓ abajo), Supremacist *(solo fuerza
marcial; ❓ muy fuerte)*, y el meme propio **Sword Way** `CVG_Meme_SwordWay`. Preceptos: arma de filo preferida (ritual de
duelo), *Weapons: Despised/Preferred*, ritual **Sword Offering** (ofrenda de espada), ritual **Sword Trial** (duelo ritual).
Rol: **Sword Elder** (líder).

### 1.2 Silent Lotus Temple — Templo del Loto Silencioso (静莲寺, *Jìng Lián Sì*)

> *"Todo sufrimiento nace de un deseo. También la compasión."* — sutra de la entrada.

**Historia.** Un monasterio fundado por la primera mujer que alcanzó el estado de **Cuerpo Vajra**. Sus monjes acogieron a los
heridos de la Calamidad y cuidaron de los **qilin** (los primeros auspiciosos que bajaron a vivir con ellos). Cuando la Gran Ruptura
rompió el reino, el Templo fue el único que no perdió a ninguna de sus doce pagodas. Lo atribuyen al **Loto de Jade** que
florece en la sala de meditación: una planta que "siente" el qi de los que entran.

**Doctrina / Dao.** *Camino del Cuerpo y la Compasión* (体道): templar el cuerpo como templo, sanar sin preguntar por qué se
hirió, **vacío** (空) para soltar los deseos y la ira. **Defectos:** dogmáticos; **no curan a quien mató por placer** (lo
saben por el qi de la víctima); piden **tributo**: una ofrenda sincera, aunque no sea cuantiosa; no comercian con armas
(no por odio a la tecnología, sino porque "un arma sin dueño es un deseo sin dueño").

**Líderes y figuras**
| Nombre | Cargo | Nota |
|---|---|---|
| **Abbess Miao Yin 妙音** | Abadesa, Alma Naciente (4) | Voz tranquila; nadie la ha visto enfadada. Guarda un registro de todos los que cura |
| **Kongming 空明** | Monje Guardián, Núcleo Dorado cima (3) | Cuerpo Vajra, golpe de un solo puño. Fiel pero estricto |
| **Yun Xin 云心** | Sanadora, Fundación (2) | Alumna de Miao Yin; ofrece curar a cualquiera. Puede unirse a la colonia como discípula |

**Técnicas firma**
| Elemento | Técnica | Efecto |
|---|---|---|
| Tierra (especialidad) | **Diamond Body** 金刚体 | Armadura de qi, resistencia al dolor |
| Madera | **Blooming Lotus Heal** 莲花愈 | Cura heridas leves de aliados en radio, escalado con R |
| Agua | **Still Water Mind** 止水心 | Buff mental: ignora ilusiones y demonio interno |
| Metal | **Vajra Palm** 金刚掌 | Golpe de palma que aturde |
| Fuego | — | (los monjes no cultivan fuego destructivo) |

**Rangos:** Aprendiz Laico *(externo)* → Monje *(interno)* → Guardián *(núcleo)* → Venerable *(anciano)* → Abadesa.

**Qué ofrece al jugador**
- **Comercio:** hierbas de curación, píldoras de sanación, ropa monástica; **no vende armas**.
- **Misiones:** escoltar peregrinos, sanar un brote de enfermedad, buscar una reliquia, **pedir refugio** para heridos de otra
  facción (decisión moral con coste), custodiar un qilin herido.
- **Enseñanzas:** cuerpo (Piel de Hierro / Cuerpo Vajra), meditación profunda (*Amante del Retiro*, Corazón de Buda), curación.
- **Tablillas:** *Diamond Body*, *Blooming Lotus Heal*, *Still Water Mind*.
- **Favores:** sanar a un colono grave (1 vez al año), purificar el demonio interno (rito del Loto).

**Linajes.** **Qilin** (los guardianes del Templo; sin ellos, el Templo no existe) y **Tortuga Negra**. Reinos 1–4;
la abadesa en 4. Pocos combatientes, pero muy resistentes.

**Ideología: *Loto y Vacío*.** Memes: Collectivist, Humanist *(o Animal Personhood por los qilin)*, y propio **Lotus Path**
`CVG_Meme_LotusPath`. Preceptos: Meditación diaria (ritual), Carne: Despreciada (dieta vegetariana), ritual **Mind
Cleansing** (reduce demonio interno), Matar: Desagradable. Rol: **Abbot** (líder). No impone postura ante la tecnología: un
colono del Loto puede tener implantes si su ideología lo permite.

### 1.3 Heavenly Cauldron Pavilion — Pabellón del Caldero Celestial (天鼎阁, *Tiāndǐng Gé*)

> *"Todo tiene precio. El Dao, también."*

**Historia.** Gremio de alquimistas y subasteros nacido como gremio comercial entre sectas en guerra: ofrecía píldoras a todos
los bandos y eso les valió neutralidad por tratado. Nunca ha sido destruido porque **nadie se atrevería a matar al
proveedor de todos**. Heredan los registros del **Fénix** que les enseñó el **fuego alquímico**. Descubrieron el Borde
mucho antes del evento, **a través de rumores y de chatarra** que cruzaba la grieta, y llevan siglos hambrientos de él: compran y
venden **cualquier tecnología** con entusiasmo (y sin escrúpulos).

**Doctrina / Dao.** *Camino del Caldero* (丹道) y del **comercio**: transformar el mundo en píldoras. "Equivalencia de valores":
cada cosa tiene un precio, incluso la vida y las promesas. **Defectos:** mercenarios; venden información; se alían con quien
pague, incluso a escondidas con el Culto o con Ascension Dynamics (§4); con trampas legales y letra pequeña.

**Líderes y figuras**
| Nombre | Cargo | Nota |
|---|---|---|
| **Qian Wanli 钱万里** | Maestro del Pabellón, Núcleo Dorado (3) | Gordo, jovial, sin enemigos oficiales. Sabe quién compró qué |
| **Mo Qinghua 墨青华** | Gran Alquimista, Alma Naciente (4) | Jamás deja su caldero; el único que sabe fabricar píldoras de Alma |
| **Hu Xiaoman 胡小满** | Subastadora, Fundación (2) | Sangre de Zorro; vende un secreto por cada pieza que subasta |
| **Ning Yuhuo 凝玉火** | Alquimista de Fuego, Núcleo Dorado (3) | Sangre de Fénix; arde cuando se enfada. Cazadora de materiales raros |

**Técnicas firma**
| Elemento | Técnica | Efecto |
|---|---|---|
| Fuego (especialidad) | **Nine-Turn Cauldron Fire** 九转丹火 | Bonus a calidad de píldoras; en combate, fuego sostenido en área |
| Madera | **Verdant Essence Draw** 青木引 | Extrae esencia de hierbas, +rendimiento de alquimia |
| Agua | **Dew Condensation** 凝露 | Cura leve + antídoto en radio |
| Metal | **Coin Rain** 钱雨 | Tira monedas de qi (ataque a distancia barato y molesto) |
| Tierra | — | — |

**Rangos:** Aprendiz de Caldero *(externo)* → Alquimista *(interno)* → Maestro de Caldero *(núcleo)* → Gran Alquimista *(anciano)* →
Maestro del Pabellón. La Contribución sube **comprando y vendiendo** (volumen de comercio), no solo con misiones.

**Qué ofrece al jugador**
- **Comercio:** el mejor del juego. Píldoras (de todos los grados), hierbas, ingredientes raros; compra tecnología, equipo
  imperial y piezas de mecanoides por buen precio. **Subastas** (misión *Casa de subastas*): carros con tablillas raras.
- **Misiones:** recolección de ingredientes raros, asegurar un envío, hallar a un cliente moroso, **espiar** a una secta rival
  (pagadas, a veces con consecuencias).
- **Enseñanzas:** alquimia (rasgo *Alquimista Innato*), identificación de píldoras, "ojo de comerciante".
- **Tablillas:** *Nine-Turn Cauldron Fire*, *Verdant Essence Draw*, *Dew Condensation*.
- **Favores:** crédito (pagar en un plazo), traer un alquimista maestro un día para una píldora difícil.

**Linajes.** **Fénix** (heredan su fuego), **Zorro** (subastadores), humanos con *Alquimista Innato*. Reinos 1–4 (la mayoría 1–2,
combatientes escoltas 2–3). Sus caravanas llevan guardias *Rogue Cultivators* contratados.

**Ideología: *Gremio del Caldero*.** Memes: Individualist, Rancher *(hierbas y bestias de laboratorio)*, y propio **Cauldron Way**
`CVG_Meme_CauldronWay`. Preceptos: Trade: Honorable, Medicina: Venerada, ritual **Auction Night** (subasta festiva, da ánimo y
reputación), Drogas: Preferidas (aquí = píldoras). Rol: **Guildmaster**.

### 1.4 Ancient Dragon Clan — Clan del Dragón Antiguo (古龙族, *Gǔlóng Zú*)

> *"Tenemos mil años. Podemos esperar mil más."*

**Historia.** El clan que desciende directamente de los dragones que **firmaron el Pacto de los Cuatro Puntos** (el de este) y
sellaron los linajes. Viven en una cordillera enroscada (**Coiled Dragon Range**, 盘龙岭) que ya era un fragmento antes de la
Ruptura. Perdieron media sangre al enfrentarse a la Calamidad, y desde entonces guardan sus huevos como un tesoro y solo
aceptan **forasteros que demuestren ser dignos**. Los más jóvenes sueñan con volver a ver el cielo abierto; los viejos temen
las grietas.

**Doctrina / Dao.** *Camino del Dragón* (龙道): fuerza física y presencia, **tesoros** como símbolo del Dao, soberanía sobre
agua y rayo. **Defectos:** orgullo, codicia (acaparan), desconfían de todo "pequeño", y **cazan a los que vendan escamas de
dragón**. Su hospitalidad es ceremonial: se deshacen en cortesías mientras evalúan.

**Líderes y figuras**
| Nombre | Cargo | Nota |
|---|---|---|
| **Ao Zhan 敖战** | Patriarca, Alma Naciente cima (4) | Dragón de pura sangre, 800 años; justo con los dignos, cruel con los traidores |
| **Ao Lian 敖莲** | Anciana, Núcleo Dorado (3) | Guarda el Tesoro del Clan; negocia con el Pabellón |
| **Ao Ming 敖明** | Príncipe joven, Fundación (2) | Impulsivo; querría ver el Borde. Aliado potencial del jugador |

**Técnicas firma**
| Elemento | Técnica | Efecto |
|---|---|---|
| Agua (especialidad) | **Dragon Tide** 龙潮 | Ola de agua en cono, derriba y apaga fuego |
| Rayo (metal/agua) | **Thunder Dragon Roar** 龙吼雷 | Aturde a los débiles; daño eléctrico en área |
| Tierra | **Coiled Mountain Stance** 盘山势 | Armadura y resistencia a empujones |
| Metal | **Dragon Claw** 龙爪 | Garra cuerpo a cuerpo con sangrado |
| Fuego | — | — |
| Madera | — | — |

*(El rayo se implementa como elemento Metal/Agua, según lo decida M5; ❓ decisión: ¿elemento propio?)*

**Rangos:** Cría *(externo)* → Joven Dragón *(interno)* → Vigilante *(núcleo)* → Anciano del Clan *(anciano)* → Patriarca.
Para el jugador el rango **se otorga por demostrar dignidad** (tesoros, hazañas, sangre) más que por contribución mecánica.
**Reino mínimo para interno: 3** (el Clan no admite menos).

**Qué ofrece al jugador**
- **Comercio:** casi nada. Aceptan **trueques de tesoros** (reliquias, minerales raros, núcleos); a cambio dan escamas, huesos de
  dragón y huevos de alta pureza *(solo a rango interno+)*.
- **Misiones:** defender un nido, buscar un huevo robado por piratas, cazar a un cazador de dragones, *Alianza matrimonial*,
  la **prueba del Tesoro** (traer algo digno).
- **Enseñanzas:** cuerpo (Piel de Hierro Ancestral), técnicas de dragón, **purificación del linaje Dragón**.
- **Tablillas:** *Dragon Tide*, *Coiled Mountain Stance*, *Dragon Claw*.
- **Favores:** ayuda en purificar la sangre (anciano: acompañar el rito).

**Linajes.** **Dragón** (Pura/Ancestral sobre todo). Reinos 3–6 (el Patriarca en 4; hay uno dormido en 6 ❓). Muy pocos, muy poderosos.

**Ideología: *Sangre y Tesoro*.** Memes: Supremacist *(linaje)*, Loyalist, propio **Dragon Blood** `CVG_Meme_DragonBlood`. Preceptos:
Tesoros: Exigidos (sala de tesoros), Altar de Dragón (sala de tesoros), ritual **Hoard Offering** (ofrenda de tesoro), Matar
bestias: Aceptado. Rol: **Patriarch**.

### 1.5 Blood Moon Cult — Culto de la Luna Sangrienta (血月教, *Xuèyuè Jiào*)

> *"El Cielo es una jaula. Rómpela con lo que tienes en las venas."*

**Historia.** Nace de una escisión de la secta ortodoxa **Heavenly Origin** (天元宗), exterminada por orden del Dao. Su fundador
**Xue Jiuyou 血九幽** halló que la sangre y el alma *concentran* el qi más rápido que cualquier meditación, y que la tribulación
no tiene por qué ser un castigo. En la **Luna del Demonio Interno** (ver [EVENTOS.md](../EVENTOS.md)) su poder se dispara. Tras la
Convergencia, han cruzado las grietas **antes que nadie**: la nueva tierra está llena de gente sin maestro y sin protección.

**Doctrina / Dao.** *Camino de la Sangre* (血道): obtener poder tomándolo; sacrificios, **farolas de alma**, sellar el demonio
interno dentro de uno mismo y usarlo como arma. **No son locos: son pragmáticos.** Respetan la fuerza y la lealtad; castigan
la traición y los hipócritas. **Defectos:** secuestran discípulos para sus ritos, mienten, sacrifican. **Pista oculta (❓):** su
fundador encontró un pedazo de máquina caída de la Calamidad: la "Luna" del culto sería, en parte, su señal. No se confirma.

**Líderes y figuras**
| Nombre | Cargo | Nota |
|---|---|---|
| **Xue Jiuyou 血九幽** (el Venerable) | Líder, Alma Naciente cima (4) | Cuerpo remendado con sangre ajena; tranquilo; le interesa el Borde como fuente de "cuerpos nuevos" |
| **Su Lanyin 苏兰音** | Santa de Sangre, Núcleo Dorado (3) | La cara pública del Culto; seduce y recluta. Posible aliada interna si se sigue el camino demoníaco |
| **Shi Gu 尸骨** | Anciano de Almas, Núcleo Dorado (3) | Recolector de farolas de alma; ataca a quien tenga mucho Corazón del Dao |

**Técnicas firma**
| Elemento | Técnica | Efecto |
|---|---|---|
| Agua (sangre; especialidad) | **Blood River** 血河 | Orilla de sangre: daño continuo y drenaje de vida en área |
| Fuego | **Soul-Burning Flame** 焚魂火 | Fuego que daña el Corazón del Dao del enemigo |
| Madera | **Venomous Vines** 毒藤 | Raíces que inmovilizan y envenenan |
| Metal | **Blood Bone Blade** 血骨刃 | Arma de hueso/sangre, ataque rápido |
| Tierra | **Bone Wall** 骨墙 | Pared de huesos; defensa temporal |

**Rangos:** Siervo de Sangre *(externo)* → Discípulo de Sangre *(interno)* → Núcleo Carmesí *(núcleo)* → Anciano de Sangre *(anciano)* →
Venerable. **Ascender exige una ofrenda** (sacrificar un enemigo derrotado o un objeto de gran valor).

**Qué ofrece al jugador (solo si amistoso)**
- **Comercio:** reliquias oscuras, sangre purificada, píldoras de sangre (más potentes, con toxicidad y riesgo de demonio interno),
  **farolas de alma**, venenos.
- **Misiones:** exterminar una secta recta, recoger "ofrendas", defender un ritual en la Luna del Demonio Interno.
- **Enseñanzas:** cultivo rápido y peligroso, sellar el demonio interno, técnicas de sangre y alma.
- **Tablillas:** *Blood River*, *Soul-Burning Flame*, *Blood Bone Blade*.
- **Favores:** refugio para cultivadores perseguidos, "ritual del Vacío de Sangre" (reducir el castigo de una tribulación fallida).

**Hostil por defecto:** ataques de secta demoníaca (EVENTOS.md §3), secuestro de discípulos, venganzas. Si el jugador ayuda a una secta
recta, empeora. Si es neutral/amistoso, el Culto **ofrece contratos**: se pueden negociar cese de ataques con tributo.

**Linajes.** Humanos, algunos con *Corazón Demoníaco*; raros linajes corrompidos (Hundun, Taotie, Qiongqi ❓, los "malvados"
de [razas](../modulos/razas/README.md) §1). Reinos 1–5, avanzan **más rápido y con más riesgo**.

**Ideología: *Camino Demoníaco*.** Memes: Individualist, Raider, Pain Is Virtue, Cannibal *(extremo; ❓)*, propio **Demonic Way**
`CVG_Meme_DemonicWay`. Preceptos: Sacrificios: Aceptados, Luna Sangrienta (ritual en la Luna del Demonio Interno), Esclavitud:
Aceptada *(o prisioneros)*, Tortura: Preferida. Rol: **Venerable**. Una colonia con esta ideología (y la mayoría de colonos)
**hace amistosa** a la secta (regla 2).

### 1.6 Rogue Cultivators — Cultivadores errantes (散修, *sǎnxiū*)

> *"El Cielo no me dio secta. Yo no pedí cielo."*

**Historia.** No es una secta: es todo el que cultiva sin lazo. Huérfanos de sectas destruidas, expulsados, hijos de la Ruptura,
caminantes sin rumbo. En la Convergencia, son los **primeros en llegar**: no tienen nada que perder. Se agrupan en posadas
(*The Wandering Cloud Inn* 浮云客栈), mercados nocturnos y clanes efímeros.

**Doctrina / Dao.** *Camino Libre* (散道): cada uno su Dao; no deben fidelidad a nadie salvo a lo que hayan jurado. Valoran
la reputación personal (江湖). **Defectos:** poco fiables en masa, mercenarios, rencores personales; algunos son viejos
demoníacos que huyen.

**Figuras**
| Nombre | Cargo | Nota |
|---|---|---|
| **Old Zhao 赵老丐** | Mendigo viejo, Alma Naciente oculto (4) | Tosco; sabe más de la Calamidad que nadie. *Experto Oculto* |
| **Han Qiu 韩秋** | Mercenaria errante, Núcleo Dorado (3) | Alquila su espada por piedras espirituales; si la tratas bien, se queda como invitada |
| **Bai Mu 白慕** | Mercader nómada, Fundación (2) | Traficante de tablillas incompletas y mapas de ruinas |

**Técnicas firma.** No tienen firma: cada errante aprende "un poco de todo" (por eso son impredecibles). Aprovechan **técnicas
prestadas** de cualquier secta. Su idea de "firma": *Wandering Step* (游步, movimiento rápido) y *Borrowed Light* (借光, copia una
técnica vista).

**Rangos (reputación).** No hay rangos formales: **Caminante → Conocido → Renombrado → Leyenda** (según misiones y duelos). Sin
exigencia de reino.

**Qué ofrece**
- **Comercio:** mercados itinerantes, mapas de sitios, tablillas incompletas (a veces con defectos), reliquias sin dueño.
- **Misiones:** guardaespaldas, rescates, encontrar la tumba de un maestro caído, recuperar una **cueva de herencia** disputada.
- **Enseñanzas:** el *Experto Oculto* enseña a quien lo merezca (sorpresa); técnicas sueltas.
- **Tablillas:** *Wandering Step*, *Borrowed Light*, y aleatorias de otras sectas (más baratas, a veces corruptas).
- **Favores:** contratar mercenarios (reino 2–3) por días.

**Linajes.** Cualquiera; es donde más se ven los **linajes sueltos** (el Zorro sin Corte, un Fénix exiliado, etc.).

**Ideología: *Libre Albedrío*.** Memes: Individualist (o Raider), propio **Wandering Path** `CVG_Meme_WanderingPath`. Preceptos:
Viaje: Preferido, Propiedad: Rechazada, ritual **Oath of the Road** (juramento de hermanos jurados), Matar sin motivo: Desagradable. Ideología sin Realeza ni sala de tronos.

---

## 2. Facciones del Borde

### 2.1 El Imperio

**Postura.** Los psylinks de la nobleza imperial **son sus cultivadores**: formalmente un título hereditario. Ve a los cultivadores
de Jiuxiao como **psycasters sin licencia**. No por odio a la tecnología (el Imperio es tecnológico), sino por **control**:
"nadie lanza qi sin título".

**Doctrina.** Orden, jerarquía, linaje. **Edicto de Conformidad del Qi** (nuevo): todo cultivador debe registrarse con la
Oficina de Orden Psíquico. Los que no, son "no-conformes".

**Figuras.** *Inquisitor Valeria Strand* (Oficina de Orden Psíquico), *Lord Cassian Voss* (noble con psylink, interesado en
"adquirir" cultivadores), *Praetor Ito Hale* (militar pragmático, abierto a la alianza).

**Qué hace.**
- Si la colonia **se registra** (misión "Registro"): comercio imperial normal, títulos y permisos, tropas en apoyo.
- Si **no se registra** y tiene cultivadores de reino ≥ 2: *Purga del Imperio* (asaltos) con prioridad creciente.
- **Alianza tensa:** el jugador puede ser noble imperial *y* cultivador, a costa de **tributo**. Los títulos del Imperio
  (Yeoman → … → Knight) **no cambian**; el mod añade el permiso *Qi Conformity Permit*.
- Con **Royalty** los psylinks pueden **convertir su psylink en raíz espiritual** (misión del Maestro o ritual).

**Tecnología:** Imperial (vanilla). **Reinos:** no usa reinos: sus psycasters se escalan con el nivel de psylink (1–6 → ≈ reino 1–4 ❓).
**Linajes:** sangre noble, Biotech *Highmate*; no tiene linajes de bestia (los considera "mutaciones").
**Ideología:** *Orden Psíquico*: Loyalist, Authoritarian *(Royalist en Ideology)*, Psychic Ritual, Honor del Linaje.

### 2.2 Ascension Dynamics (nueva)

**Historia.** Corporación de biotecnología y mecánica cuántica que **detectó el qi** antes de la Convergencia (sus sensores
registraban un "campo psíquico" anómalo). Su meta: **fabricar cultivadores** por implantes que *fuerzan* los meridianos
y sangre extraída de linajes. Es el **camino demoníaco tecnológico**: sus cultivadores son poderosos pronto, pero inestables
y con **disonancia celestial muy alta** (tribulaciones más duras; ver DISENO). Fundada por **Dr. Mireille Okonkwo-Vance**.

**Doctrina.** *"La ascensión es un producto."* Todo se mide, se patenta y se vende. Los cultivadores son **sujetos de ensayo**.
**Defectos:** crueldad corporativa; secuestran portadores de linajes (para extraer sangre y xenogérmenes); venden
"licencias" a quienes pueden pagar.

**Figuras.** *Dr. Mireille Okonkwo-Vance* (CEO), *Director Idris Kaul* (jefe de seguridad, caza cultivadores), *Subject 0 — Zhou Tian 周天*
(un discípulo de Jiuxiao capturado y modificado; el primer "ascendido" de AD, ahora su arma principal).

**Qué hace.**
- **Comercio:** implantes de cultivo (con disonancia celestial **alta por defecto**, ❓ tabla de disonancias en M4), xenogérmenes
  de linaje (Dragón, Fénix…), píldoras sintéticas (**alta toxicidad**).
- **Misiones:** muestras de qi, cazar a un errante, "ensayo clínico" (colonos voluntarios para un implante).
- **Hostilidad:** *Cazadores corporativos* (EVENTOS.md §3), compran prisioneros de cultivo, **secuestran portadores de linaje**.
- **Relación con el Pabellón del Caldero:** socios (a escondidas) y competidores.

**Tecnología:** Spacer / Ultra. **Reinos:** 1–3 artificiales (nunca naturales). **Linajes:** sangre extraída de Dragón/Fénix, no
natural.
**Ideología:** *Ascensión como Producto*: Transhumanist, Individualist, Technophile; propio **Corporate Ascension**
`CVG_Meme_CorporateAscension`. Preceptos: Implantes: Preferidos, Bioexperimentación: Aceptada.

### 2.3 Otras (sin ficha propia; de LORE.md §6)
- **Tribus:** ven a los cultivadores como espíritus; resonancia con el árbol ánima (Biotech: árbol como altar). Camino chamánico futuro.
- **Piratas:** cazan cultivadores y tesoros para venderlos.
- **Mecanoides:** atraídos por el qi; no son facción diplomática (ver LORE.md §5, la Calamidad).

---

## 3. Qué ofrece el jugador a las sectas

| Qué | Lo valoran | Lo desprecian |
|---|---|---|
| Piedras espirituales | Pabellón (mucho), Dragón (poco) | Loto |
| Armas de calidad | Pico (mucho), Culto | Loto |
| Tecnología del Borde | Pabellón (mucho), Pico | Dragón (desconfía) |
| Pieles/escamas de dragón | — | Dragón (hostiliza al vendedor) |
| Ayuda contra el Culto | Pico, Loto, Dragón | Culto |
| Cooperar con el Imperio | Imperio | Errantes, Culto |
| Armas contra mecanoides | Todas las sectas rectas | AD (los quiere vivos) |

---

## 4. Relaciones entre facciones

`+` aliada / `=` neutral / `-` enemiga / `?` negocia por conveniencia.

| | Pico | Loto | Pabellón | Dragón | Culto | Errantes | Imperio | AD |
|---|---|---|---|---|---|---|---|---|
| **Pico** | — | + | = | + (pacto antiguo) | − | = | − (desprecia el control) | − |
| **Loto** | + | — | = | = | − | + | = | − |
| **Pabellón** | = | = | — | ? | ? (vende a todos) | = | ? | ? (socio a escondidas) |
| **Dragón** | + | = | ? | — | − | = | − | − (cazan sangre) |
| **Culto** | − | − | ? | − | — | − | − | ? (AD le vende cuerpos) |
| **Errantes** | = | + | = | = | − | — | − | − |
| **Imperio** | − | = | ? | − | − | − | — | = (rivales) |
| **AD** | − | − | ? | − | ? | − | = | — |

**Conflictos que mueven el mapa**
- **Pico + Loto + Dragón** forman la "Alianza Recta" nominal contra el Culto, pero se vigilan: el Pico quiere más autoridad y
  el Dragón no se mezcla.
- **Culto vs. todos**, con ayuda secreta del **Pabellón** (le vende ingredientes) y de **AD** (le vende cuerpos).
- **Imperio vs. cultivadores**, pero el **Pabellón** se aprovecha de ambos.
- **AD vs. Dragón**: AD cazando portadores de linaje; el Clan envía cazadores propios.
- El jugador está en medio: ayudar a una facción empeora la relación con sus enemigos, como en RimWorld (cambio de
  buena voluntad por tarea).

**Ejemplos de alianzas desbloqueadas (❓ Decisión):** *Alianza Recta* (Pico+Loto+Dragón firman un tratado con la colonia, ayuda
mutua); *Camino de las Sombras* (Culto+AD, por vías ilegales).

---

## 5. Ideas de ideología (resumen)

Ideology permite **memes** y **preceptos**; los nombres vanilla (Collectivist, Individualist, Loyalist, Raider, Supremacist,
Transhumanist, Pain Is Virtue…) se usan donde existen, y se añaden memes propios `CVG_Meme_*` cuando no hay equivalente.
**Verificar contra los Defs de Ideology 1.6 antes de implementar**; los nombres vanilla aquí son aproximados.

| Secta | Ideología | Memes (vanilla + propios) | Rol |
|---|---|---|---|
| Pico | Camino de la Espada | Individualist/Loyalist + `SwordWay` | Sword Elder |
| Loto | Loto y Vacío | Collectivist + `LotusPath` | Abbot |
| Pabellón | Gremio del Caldero | Individualist + Rancher + `CauldronWay` | Guildmaster |
| Dragón | Sangre y Tesoro | Supremacist + Loyalist + `DragonBlood` | Patriarch |
| Culto | Camino Demoníaco | Individualist + Raider + Pain Is Virtue + `DemonicWay` | Venerable |
| Errantes | Libre Albedrío | Individualist + `WanderingPath` | — |
| Imperio | Orden Psíquico | Loyalist + Authoritarian | Noble |
| AD | Ascensión como Producto | Transhumanist + Individualist + `CorporateAscension` | CEO |

**Postura ante la tecnología** (la fija el jugador, no el mod): cualquiera de estas ideologías puede combinarse con memes vanilla
**Transhumanist / Flesh Purity**. El mod **no** impone "anti-implantes" a ninguna secta.

---

## ❓ Decisiones pendientes (resumen)
1. Umbral del Culto (≥ mayoría = amistosa; ≥25% = neutral).
2. Rango mínimo de reino para cada rango de secta y valores de Contribución.
3. Máximo una secta de rango ≥ interno (incompatibilidad entre enemigas).
4. Elemento "rayo" del Dragón: ¿Metal/Agua o elemento propio?
5. Si el Culto tiene la pista oculta de la Calamidad (señal de máquina).
6. Memes propios `CVG_Meme_*`: crear los 8 (o reutilizar vanilla).
7. Alianzas especiales (*Alianza Recta*, *Camino de las Sombras*) como misiones de mundo.
8. Reinos de los PNJ y escalas de poder para los raids de cada facción (alineado con M5/Quests).
