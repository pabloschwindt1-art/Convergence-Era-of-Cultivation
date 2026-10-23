# Lista de arte pendiente (backlog)

Se genera con ComfyUI (parámetros en ARTE.md) → candidatos en `art/generated/comfy/<nombre>/` → el usuario elige →
se integran en el mod. Prioridad: **P1** lo que el código ya usa o usará pronto; **P2** módulos de contenido; **P3** después.

| Categoría | Elementos | Tamaño | Prioridad |
|---|---|---|---|
| **Iconos de interfaz** | Pestaña de construcción "Cultivo", pestaña de investigación, botón Intentar avance, gizmo de qi, iconos de los 3 tesoros (cuerpo/qi/alma), icono de cuello de botella, icono de tribulación | 64 | P1 |
| **Iconos de genes** | 5 elementos, 4 calidades de raíz, 11 cuerpos especiales, genes de linaje (5 linajes × 4 purezas) | 64 | P1 (raíz) / P2 (linajes) |
| **Poderes / técnicas** | Proyectil elemental (×5 elementos), escudo de qi, paso veloz, presión de aura, Aliento de Dragón, Nirvana del Fénix, Bendición del Qilin, Caparazón de Xuanwu, Encanto del Zorro, Forma de Nueve Colas, Cuerpo de Hierro, Furia de Sangre | 64 | P1 (las 3 del MVP) / P2 |
| **Objetos y materiales** | Piedra/jade/cristal espiritual, Píldora de Fundación y otras píldoras (grados), acero espiritual, plata lunar, oro del caos, hierro frío, acero galáctico, núcleo de bestia (por nivel), esencias de sangre de linaje, tablilla de jade, talismanes, madera espiritual, seda espiritual, carne espiritual | 128 | P1 (piedra, píldora) / P2 |
| **Edificios** | Cojín de cultivo, plataforma de jade, Forja Espiritual (niveles 1-3), horno de alquimia, mesa de runas, mesa de talismanes, escritorio de contemplación, muñeco de madera, pesas, pilar de hierro, estanque frío, altar de incienso, mesa de caligrafía, tablero de go, núcleo y banderas de formación, muebles de feng shui (biombo, estatua, jarrón, incensario) | 128 por celda | P1 (cojín) / P2 |
| **Flora** | Hierba espiritual, ginseng de mil años, loto de nieve, fruta roja de sangre, árbol espiritual, hongos subterráneos, Champiñón de la Tribulación, terreno de tierra espiritual | 128 / textura de terreno | P2 |
| **Fauna (3 vistas)** | Lobo de Viento, Serpiente de Jade, Grulla Celestial, Araña de los Nueve Infiernos (✅ hecha con Gemini), Suanni, Bifang, Gudiao, Jiuying, Bestia de la Tribulación | 256 | P2 |
| **Jefes (3 vistas)** | Taotie, Qiongqi, Hundun, Taowu | 512 | P3 |
| **Razas (aspecto)** | Cuernos de dragón, cola escamosa, plumas de fénix, cuernos de qilin, patrón de caparazón de tortuga, orejas y colas de zorro (1/3/6/9) | Según el nodo de render (pelo/cabeza/cuerpo) | P2 |
| **Implantes de cultivo** | Corazón del Dao abierto, raíces espirituales expuestas, sangre de fénix, circuitos de meridianos, reactor de núcleo dorado, ojo celestial | 64 (icono) | P2 |
| **Armas y equipo** | Espadas y sables espirituales, espada voladora, lanza, arco, colgantes y anillos de jade, túnicas de secta | 128 | P2 |
| **Ropa y telas** | Algodón de las Nubes (planta + fardo), seda espiritual, cuero de bestia; túnicas de discípulo externo/interno/anciano, hanfu, sombrero de bambú, corona de jade, velo; armaduras de escamas, jade, plata lunar, escama de dragón, túnica rúnica (vistas de ropa sobre pawn) | Ropa (render) / 128 | P2 |
| **Enemigos humanoides** | Túnicas y emblemas de las 6 sectas, armaduras de Ascension Dynamics | Ropa (render) | P3 |

## Rutas de arte de las técnicas (M5)

El código busca estos PNG al arrancar y, si existen, reemplazan al icono provisional (el del elemento): no hace falta tocar XML.

| Qué | Ruta (en `Mods/Convergence.Core/Common/Textures/`) | Tamaño |
|---|---|---|
| Icono de cada técnica | `UI/Techniques/<defName>.png`: `CVG_Tech_MetalNeedle`, `CVG_Tech_QiShield`, `CVG_Tech_SwordQiSweep`, `CVG_Tech_ThornBolt`, `CVG_Tech_SpringRejuvenation`, `CVG_Tech_ThornStorm`, `CVG_Tech_FrostShard`, `CVG_Tech_SwiftStep`, `CVG_Tech_FrostBreath`, `CVG_Tech_Fireball`, `CVG_Tech_BlazingMeridians`, `CVG_Tech_SeaOfFlames`, `CVG_Tech_StoneBullet`, `CVG_Tech_MountainBody`, `CVG_Tech_EarthTremor` | 128×128 |
| Técnicas pasivas (ronda 3) | `UI/Techniques/<defName>.png`: `CVG_Tech_GoldenBellSkin`, `CVG_Tech_EdgeOfTheHeart`, `CVG_Tech_SwordIntent`, `CVG_Tech_EvergreenBreath`, `CVG_Tech_TreeHeart`, `CVG_Tech_CloudSteps`, `CVG_Tech_StillWaterMind`, `CVG_Tech_TideReading`, `CVG_Tech_InnerFurnace`, `CVG_Tech_SunHeart`, `CVG_Tech_StoneMarrow`, `CVG_Tech_RootedMountain` | 128×128 |
| Cartas de los caminos (ventana Desarrollo) | `UI/Paths/CVG_Path_Metal`, `_Wood`, `_Water`, `_Fire`, `_Earth`, `_Sword`, `_Soul`, `_Body` (verticales, 512×768) | 512×768 |
| Iconos de stats (ventana Desarrollo) | `UI/Stats/<defName>`: `CVG_QiMax`, `CVG_QiRegen`, `CVG_CultivationSpeed`, `CVG_BreakthroughChance`, `CVG_CelestialDissonance`, `CVG_TechniqueCost`, `CVG_BloodQiMax`, `CVG_BodyTemperingSpeed` (hoy, iconos de técnicas) | 64×64 |
| Raíz de Trueno | `UI/Icons/Genes/CVG_RootElement_Thunder` (hoy, el de metal teñido de violeta; también es el icono provisional de sus técnicas) y `UI/Techniques/CVG_Tech_LightningStep`, `_ChainLightning`, `_ThunderRoar`, `_LightningReflexes`, `_StormHeart`, `_LightningBody`, `_TribulationBorn` | 64×64 / 128×128 |
| Caminos estilo VPE | `UI/Techniques/CVG_Tech_FlyingSword`, `CVG_Tech_UnbreakableEdge`, `CVG_Tech_TenThousandYearPine`, `CVG_Tech_BoundlessSea`, `CVG_Tech_PhoenixEmber`, `CVG_Tech_EndlessMountain`, `CVG_Tech_DivineSense`, `CVG_Tech_IronShirt`, `CVG_Tech_DragonElephantMight`, `CVG_Tech_UnbreakableBones`; iconos de camino (`iconPath` de Espada, Sentido Divino y Cuerpo Vajra; hoy, iconos de técnicas) y, como VPE, un fondo ilustrado por camino (opcional) | 128×128 |
| Jiangshi (Biotech Wuxia + Anomaly) | Espejo bagua `Things/Building/CVG/BaguaMirror`, espada de melocotonero `Things/Item/Equipment/CVG/PeachWoodSword`, madera de melocotonero (stack), cataplasma de arroz (stack); hoy texturas vanilla teñidas | 64×64 / 128×128 |
| Icono de manual (opcional) | `iconPath` del `TechniqueManualDef` (hoy, los iconos de elemento) | 64×64 |
| Proyectiles | `Things/Projectile/CVG/<Nombre>` (cambiar `texPath` en `Techniques_Things.xml`) | 64×64 |
| Tablilla de jade | `Things/Item/CVG_JadeSlip` (hoy, el jade vanilla teñido) | 64×64 |

## Sonidos (propios deseables)

Hoy todo usa sonidos vanilla a través de `SoundCueDef` (`Mods/Convergence.Core/Common/Defs/SoundCues.xml`) y `soundCast` en las técnicas.
Para poner un sonido propio: crear un `SoundDef` con sus clips en `Sounds/` y cambiar `sound` del cue (o el `soundCast` de la técnica). Sin código.

| Cue / uso | Hoy (vanilla) | Sonido propio deseado | Prioridad |
|---|---|---|---|
| `CVG_Cue_Convergence` | `PsychicPulseGlobal` | Gong grave + zumbido que sube, el cielo "respira" (3–5 s) | P1 |
| `CVG_Cue_BreakthroughStart` | `PsychicSootheGlobal` | Cuenco tibetano + inhalación larga | P2 |
| `CVG_Cue_BreakthroughSuccess` | `Quest_Succeded` | Campana de bronce + "ding" cristalino ascendente | P1 |
| `CVG_Cue_BreakthroughFail` | `EnergyShield_Broken` | Cristal que se quiebra + tos de sangre | P2 |
| `CVG_Cue_StageReached` | `TinyBell` | Campanilla corta de jade | P2 |
| `CVG_Cue_TribulationWarning` | `Thunder_OffMap` | Trueno lejano con retumbo creciente; el golpe del rayo (`Thunder_OnMap`) con crepitar eléctrico | P1 |
| `CVG_Cue_BloodlinePurified` | `Psycast_Skip_Entry` | Rugido/graznido suave del linaje (uno por bestia: dragón, fénix, qilin, tortuga, zorro) | P2 |
| `CVG_Cue_MentorArrival` | `Psycast_Skip_Entry` | Grieta que se abre + pasos cansados | P2 |
| `CVG_Cue_MentorFarewell` / `Death` | `RitualConclusion_Positive` / `_Negative` | Gong de despedida / gong ahogado | P3 |
| `CVG_Cue_ManualLearned` | `LetterArrive_Good` | Tablilla de jade al tocar la frente (tintineo) | P3 |
| Técnicas (`soundCast`) | Disparos/explosiones vanilla | Espada de qi (silbido metálico), espina de madera, escarcha, bola de fuego (whoosh+crepitar), piedra, temblor; escudo de qi (campana) | P2 |
| Genes de linaje | Voces de animal vanilla (`Pawn_Furskin_Call`…) | Voces propias por linaje | P3 |
| Combate de sectas | — | Gritos de batalla y entrada de grieta al llegar un asalto | P3 |

## Qi-Tech e Ingeniería Espiritual (pedidos al usuario, 2026-10-21)

Fondo transparente (PNG o WebP). Se integran con `tools/art/recorte/` (`turret_sheet.py`, recorte de armas) a las rutas indicadas.

| # | Qué | Vista y formato | Ruta final | Prioridad |
|---|---|---|---|---|
| 1 | Resonador espiritual (2×2) | Cenital, como el multianalizador; una sola imagen (no rota) | `QiTech: Things/Building/CVG/QiTech/SpiritResonator.png` | ✅ hecho (2026-10-21) |
| 2 | Cristal primordial | 3 imágenes de pila: 1 cristal, unos pocos, montón (`_a`, `_b`, `_c`), 128 px | `Core: Things/Item/Resource/CVG_PrimordialCrystal/CVG_PrimordialCrystal_a/_b/_c.png` | ✅ hecho (2026-10-21) |
| 3 | Proyectiles de qi: rayo de inducción, rayo concentrado, rayo de qi comprimido | Apuntando al **norte**, alargados, 64-128 px | `QiTech: Things/Projectile/CVG/QiTech/<Nombre>.png` | ✅ hecho (2026-10-21) |
| 4 | Convertidor de qi (2×2) | Cenital, una imagen | `QiTech: Things/Building/CVG/QiTech/QiConverter.png` | ✅ hecho (2026-10-21) |
| 5 | Cámara de estabilización (1×2, cama) | Tres vistas: `_south`, `_east`, `_north` | `QiTech: Things/Building/CVG/QiTech/StabilizationChamber_*.png` | ✅ ya era del usuario (commits 861d93e, eb1986f) |
| 6 | Exoesqueleto Qi-Tech | Ropa sobre el pawn: 5 cuerpos × 3 vistas + icono | `QiTech: Things/Pawn/Humanlike/Apparel/CVG/QiTechExosuit/` | ✅ hecho (2026-10-21): diseño del usuario ajustado a las siluetas vanilla (plantilla `art/qitech/plantilla_exoesqueleto.png`) |

Hechos: rifles de inducción (2 variantes), rifle concentrado, cañón de compresión, torretas de inducción y de contención (base, cabezal, icono).
## Sects 2.0: íconos de las cinco sectas (2026-10-22)

Hojas del usuario (7 íconos por secta, hechas con IA). Recortes a tamaño completo y a 128×128 en `art/sects2/<Secta>/`
(`meme`, `leader`, `elder`, `specialist`, `ritual`, `emblem`, `contribution`, más `*_full.png`). Las facciones usan color casi blanco
para que el emblema no se tiña.

| Recurso | Integrado en (`Mods/Convergence.Factions/Common/Textures/`) | Estado |
|---|---|---|
| 1 Meme | `UI/Memes/CVG/SwordWay`, `BodyWay`, `CauldronWay`, `DemonicWay`, `PuppetPath` | ✅ |
| 4 Especialista | `UI/Roles/CVG/SwordMaster`, `TemperingMaster`, `AlchemistMaster`, `BloodMoonOfficiant`, `PuppetMaster` | ✅ |
| 5 Ritual | `UI/Icons/Rituals/CVG/TemperSwordBody`, `NineTemperings`, `CauldronBlessing`, `BloodOath`, `ThreadBinding` | ✅ |
| 6 Emblema | `World/CVG/Factions/<Secta>` (`factionIconPath`) | ✅ |
| 7 Contribución | `UI/Icons/CVG/Contribution/<Secta>` (`royalFavorIconPath`) | ✅ |
| 2 Líder de secta | `UI/Roles/CVG/<Secta>_Leader` (parche de `Precept.Icon` en las ideologías de secta) | ✅ |
| 3 Gran Anciano | `UI/Roles/CVG/<Secta>_Elder` (el rol moralista se llama Gran Anciano) | ✅ |
