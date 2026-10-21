# Qi-Tech v0.2 (mod `Convergence.QiTech`)

Rama tecnológica del mod, **solo para quien juega con tecnología** (DISENO.md §2: el Core nunca depende de la electricidad). Depende del Core y de Formations (red de qi). Todo exige investigación industrial y **científica**: `CVG_Research_QiTech` requiere `Electricity` y `MicroelectronicsBasics` (v0.2: antes pedía `CVG_Research_QiNetwork`, que es un proyecto del Dao con mesa de contemplación y, con Anomaly, conocimiento; un investigador sin raíz espiritual no podía llegar). Se conserva el defName por las partidas guardadas.

## Equivalencia (Principio n.º 2)
| Vanilla | Qi-Tech |
|---|---|
| Generador / batería ↔ red eléctrica | **Convertidor de qi**: une la red de qi con la red eléctrica vanilla |
| Armadura de reconocimiento / marine | **Exoesqueleto Qi-Tech**: armadura con núcleo de qi |
| Cama de hospital + monitor | **Cámara de estabilización**: baja la disonancia celestial |

## Convertidor de qi (2×2, categoría Energía)
- Dos modos que se alternan con un botón: **qi → electricidad** (consume 100 qi/día de la red y entrega 1000 W, como un generador a leña) y **electricidad → qi** (toma 1000 W y entrega 55 qi/día). Se pierde en ambos sentidos (≈ 10 W por qi hacia la electricidad, ≈ 18 W por qi de vuelta; la referencia de pulido es 14 W/qi).
- Código: `CompQiConverterNode` (hereda `CompQiNode`: ConsumeDay/ProduceDay/IsConsumer son virtuales en el Core) + `CompQiConverterPower` (hereda `CompPowerPlant`). Sin la red de qi no produce nada; sin red eléctrica en modo inverso no produce qi.
- Costo: acero 120, componentes 6, piedras espirituales 20. Investigación `CVG_Research_QiTech`.

## Cámara de estabilización
- Cama médica con corriente (400 W). Quien descansa dentro y lleva algo que sume disonancia celestial (implante o injerto con ese stat positivo, o un aparato como el exoesqueleto) recibe `CVG_Stabilized` durante 6-8 días: **−0,4** de `CVG_CelestialDissonance`.
- Investigación `CVG_Research_QiStabilization`. Costo: metal 30, componentes 8, piedras espirituales 30, jade 6.

## Exoesqueleto Qi-Tech
- Armadura (hereda la de reconocimiento): agudo 0,85, contundente 0,55, calor 0,4; +40 qi máximo, +0,6 regeneración de qi, velocidad +0,15 y **disonancia celestial +0,9** (alta: la tribulación se endurece; estabilizar compensa 0,4).
- Se fabrica en el banco de fabricación; investigación `CVG_Research_QiExosuit` (requiere el multianalizador). Costo: plastiacero 40, componentes 8, piedras espirituales 12.

## Armas y torretas (v0.2)
Borrador externo (`QiTech_v02_integracion.zip`) revisado e integrado. El **grado espiritual va en el arma** (`CompProperties_SpiritGrade`); el Core lo lee de `DamageInfo.Weapon` (arma equipada o `turretGunDef`), verificado con impactos reales en `-cvgqitechwartest`. Grado 0 = vanilla; ningún colono necesita raíz espiritual para investigar, fabricar o disparar.

| Investigación (pestaña de cultivo, x8-10) | Requiere | Desbloquea |
|---|---|---|
| `CVG_Research_QiInduction` (3000) | QiTech + Fabrication | — |
| `CVG_Research_QiWeapons` (4000) | QiInduction + PrecisionRifling | rifles de inducción y enfocado |
| `CVG_Research_QiDefense` (4400) | QiInduction + HeavyTurrets | torreta de inducción |
| `CVG_Research_QiAdvancedMilitary` (6500) | QiWeapons + QiDefense | cañón de compresión, cañón de contención |

| Arma | Grado | Base vanilla | Daño / PA | Costo |
|---|---|---|---|---|
| Rifle de inducción de qi | 1 | rifle de asalto | 12 / 0,18, ráfaga 3 | acero 70, comp. 7, piedras 10 |
| Rifle de qi concentrado | 2 | rifle de francotirador | 24 / 0,38 | plastiacero 45, comp. avanzados 2, piedras 22 |
| Cañón de compresión de qi | 3 | lanza de carga | 30 / 0,5, −0,25 velocidad | plastiacero 110, comp. avanzados 6, piedras 48 |
| Torreta de inducción (2×2, 450 W) | 2 | torreta de autocañón | 12 / 0,18, ráfaga 3, **0,1 piedra por disparo** | acero 200, comp. 8, piedras 20 |
| Cañón de contención (2×2, 1200 W) | 3 | torreta de uranio | 30 / 0,5, alcance 3,9-43,9, **0,4 piedra por disparo** | acero 150, plastiacero 120, comp. avanzados 5, piedras 45 |

- Recetas solo en el banco de fabricación (`recipeUsers Inherit="False"`); `weaponTags`/`tradeTags` propios (`CVG_QiTechGun`, `CVG_QiTechWeapon`, `Inherit="False"`): ningún pawnkind ni comerciante vanilla las trae, así que los raids y el inicio no cambian.
- Torretas: heredan `AutocannonTurret` (Principio n.º 2). Dejan de disparar sin corriente (`Active`), apagadas (FlickedOff → PowerOn falso) o con menos piedra que un disparo (`Verb.Available`); combustible por disparo con el patrón de Formations. Se construyen vacías.
- Balance: los grados 1-3 se quedan por debajo del 2-4 que preveía DEFENSA.md; el daño base es el de su arma vanilla equivalente (el borrador era más fuerte y más barato que el rifle de carga). El grado 3 iguala a un Núcleo Dorado; los reinos 4-6 siguen suprimiendo.
- **Arte (del usuario, 2026-10-21):** rifle de inducción con dos variantes al azar (`Graphic_Random`, carpeta `InductionRifle/` _a/_b), rifle concentrado, cañón de compresión (`Things/Item/Equipment/CVG/QiTech/`), y las dos torretas con base, cabezal e ícono (`Things/Building/CVG/QiTech/Qi<Nombre>Turret`, `_Top`, `_MenuIcon`) hechos con `tools/art/recorte/turret_sheet.py`. Proyectiles: vanilla (Charge_Small, ChargeLanceShot).
- **Orientación:** el `_Top` apunta al **este**, como el Art Source de Ludeon (`TurretTop.ArtworkRotation = -90`), centrado en su eje (el núcleo); el usuario lo dibuja hacia arriba y el script lo gira. Los proyectiles se dibujan hacia el norte. El cañón de contención usa `turretTopDrawSize` 3,2 (más grande que el 2,7 de inducción).
- **Boca del cañón:** vanilla lanza el proyectil desde el centro del edificio; `DefModExtension_TurretMuzzle` (`muzzleDistance` 1,15 y 1,4) lo corre a la punta, con el destello ahí (el del verbo, en 0).
- **Pruebas visuales:** `-cvgqitechshowcase` (vitrina con oleadas, ventana visible) y `-cvgqitechaimshot` (enemigo al este y al norte, ángulo y capturas, con la ballesta de Formations de referencia).

## Pendiente
Escudo tecnológico contra técnicas, almacenamiento, sensores, tecnología contra Anomaly y mecanoides, baterías de qi eléctricas, espadas voladoras con circuitos, constructo de qi (androide), compatibilidad con Medieval Overhaul; arte propio de armas y torretas.

## Prueba
`-cvgqitechtest`: investigaciones y prerrequisito industrial, convertidor en los dos modos con red de qi real (pilar + cristal), cámara y exoesqueleto (dissonancia +0,9 y −0,4).

`-cvgqitechwartest` (v0.2): grafo de investigación (sin ciclos ni proyectos del Dao), recetas, etiquetas, texturas, curva de supresión; en vivo, un colono mortal y las dos torretas disparan a un objetivo, se captura el `DamageInfo` real y su grado, se mide la piedra por disparo; la torreta calla sin energía, apagada y sin piedras; guardar/cargar conserva combustible y bonus de grado.
