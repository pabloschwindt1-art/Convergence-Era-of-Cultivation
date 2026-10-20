# Qi-Tech v0.1 (mod `Convergence.QiTech`)

Rama tecnológica del mod, **solo para quien juega con tecnología** (DISENO.md §2: el Core nunca depende de la electricidad). Depende del Core y de Formations (red de qi). Todo exige investigación industrial: `CVG_Research_QiTech` requiere `Electricity` y `CVG_Research_QiNetwork`.

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

## Pendiente (v0.2)
Torretas con runas, baterías de qi eléctricas, espadas voladoras con circuitos, constructo de qi (androide), carpeta de compatibilidad con Medieval Overhaul.

## Prueba
`-cvgqitechtest`: investigaciones y prerrequisito industrial, convertidor en los dos modos con red de qi real (pilar + cristal), cámara y exoesqueleto (dissonancia +0,9 y −0,4).
