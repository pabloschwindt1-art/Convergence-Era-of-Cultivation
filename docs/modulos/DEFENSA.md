# Módulo: Defensa — armas espirituales y formaciones

Consecuencia de la **supresión de reino** (DISENO.md §4.1c): contra un cultivador de reino alto, las armas mortales
casi no hacen daño. El jugador necesita armas espirituales y formaciones. Todo se investiga y se mejora
(INVESTIGACION.md: ramas Forja, Formaciones y feng shui, Talismanes, Qi-Tech).

## 1. Grado espiritual de las armas

Cada arma tiene un **grado** (stat/comp `CVG_SpiritGrade`). El daño contra un cultivador se compara así:
grado del arma (o reino del atacante en técnicas y puños) **≥** reino del objetivo → daño completo; menor →
suprimido según la diferencia (curva en XML, fuerza ajustable en los ajustes).

| Grado | Qué es | Cómo se obtiene |
|---|---|---|
| 0 — Mortal | Armas vanilla y de otros mods | Ya existen |
| 1 — **Grabada con runas** | Arma existente mejorada con runas que conducen qi (también munición rúnica en modo tecno) | Mesa de runas: mejora armas que ya tienes (comp en todas las armas) |
| 2 — **Arma espiritual** (法器) | Espadas, sables, lanzas, arcos de acero espiritual / plata lunar | Forja Espiritual nivel 1-2 |
| 3+ — **Tesoro espiritual** | Artefactos de alto grado, **espadas voladoras** | Forja nivel 3 |
| 2-4 — **Energía Qi-Tech** | Rifles de qi, cañones de cristal, torretas | Rama Qi-Tech (solo modo tecno, con electricidad) |

- **Talismanes de combate:** consumibles baratos de daño espiritual (salida de emergencia).
- **Técnicas:** el daño de técnicas usa el reino del lanzador como grado.

## 2. Formaciones (阵法)

Estructura: un **núcleo** (combustible: piedras espirituales, `CompRefuelable`) + **banderas/placas** colocadas
alrededor que definen el área. Un `MapComponent` calcula las celdas cubiertas y aplica el efecto. Se mejoran por
niveles (como la forja) y por investigación.

| Formación | Efecto | Notas |
|---|---|---|
| **Gran Formación Protectora** | Cúpula que absorbe proyectiles y ataques; se debilita con los golpes | Consume piedras al recibir daño |
| **Formación de Espadas Asesinas** | Espadas de qi que disparan a enemigos dentro del área | Torreta de área; grado según el nivel |
| **Formación de Ilusión / Laberinto** | Enemigos ralentizados, desorientados o que se atacan entre sí | Control de masas |
| **Formación de Atadura** | Inmoviliza a un enemigo poderoso unos segundos | Contra jefes y cultivadores |
| **Formación de Reunión de Qi** | Sube la concentración de qi | No defensiva; cultivo |

- Con **feng shui**: el elemento de las banderas y la armonía de los cinco elementos potencian la formación.
- En **modo tecno**: formaciones alimentadas por la red eléctrica (Qi-Tech).

## 3. Torretas y defensas fijas
- **Torreta de ballesta rúnica** (modo clásico), **torreta de cristal de qi** (modo tecno).
- **Estatuas guardianas** de piedra espiritual que despiertan cuando entran enemigos.

## 4. Dificultad justa
- Los cultivadores enemigos **escalan con tu colonia**: tu reino más alto, la riqueza y la investigación de
  Formaciones/Forja. Sin cultivadores fuertes en tu colonia, no llegan Núcleos Dorados enemigos.
- Aviso previo: los asaltos con cultivadores de reino alto llegan con carta especial ("Una presión de aura
  aplastante se acerca…") para que el jugador active formaciones.
- Ajustes del mod: fuerza de la supresión, escala de cultivadores enemigos, desactivar la supresión.

## 5. Prioridad
1. Grado espiritual + supresión de reino (C#) — junto con los reinos altos.
2. Mesa de runas (mejorar armas existentes) y talismanes de combate.
3. Armas espirituales de la forja.
4. Formaciones: protectora y de espadas primero.
5. Qi-Tech y el resto.

## 6. Formaciones: implementación v0.1 (mod `Mods/Convergence.Formations`, 2026-10-16)
Mod aparte con DLL (`Source/Convergence.Formations`, depende del Core). Decisión: no se copió código de `referencias/github` (ED-Shields es MIT, RimworldShieldGenerator
Unlicense); solo se estudió su enfoque (interceptar el `Projectile` en su `Tick` con Harmony). No hay código de terceros → no hace falta `LICENSE-THIRD-PARTY.md`.

- **Núcleo** (`CompFormationCore` + `CompRefuelable` con `CVG_SpiritStone`) y **bandera** (`CVG_FormationFlag`): el área es el **polígono convexo** de las banderas a ≤16 casillas del núcleo
  (mín. 3, máx. 8; tope de casillas). Cada pareja de banderas sobre el mínimo sube un **nivel** (1–3) que multiplica el efecto. Al seleccionar el núcleo se dibuja el área y las líneas a las banderas.
- Los efectos son clases `FormationWorker` elegidas por XML (`workerClass`): **Reunión de Qi** (+1 qi/nivel por casilla, vía el hook `QiConcentrationUtility.AreaProviders` del Core, en interior y exterior),
  **Gran Formación Protectora** (cúpula: parche Harmony a `Projectile.Tick`; absorbe proyectiles de lanzadores hostiles dentro del área; cada golpe gasta daño/(120×nivel) piedras; si no hay combustible, cae),
  **Espadas Asesinas** (lanza `CVG_Projectile_MetalNeedle` del Core, 1 por nivel, a enemigos del área), **Atadura** (aturde 5 s×nivel al enemigo más fuerte: mayor reino y luego `combatPower`).
- **Estatua guardiana** y **torreta de ballesta rúnica**: `Building_TurretGun` vanilla con `CompRefuelable` de piedra espiritual (sin electricidad).
- Investigación: rama de Formaciones del árbol de Comprensión del Dao: Cinco elementos → Reunión de Qi → Formaciones defensivas → Gran Formación Protectora (`Research_Formations.xml`).
- Efectos visuales: solo flecks vanilla (destello, chispas) y `GenDraw`; sonidos vanilla.
- Prueba: `-cvgformationstest` (con -quicktest): área, qi, cúpula, espadas, atadura y torretas, ALL OK.

**Pendiente:** Formación de Ilusión/Laberinto; subir de nivel el núcleo con mejoras (hoy el nivel sale de las banderas); armonía de elementos de las banderas (el elemento de los materiales); cúpula contra melé y aire; la cúpula
siempre visible (hoy solo al seleccionar); estatua que "despierta" visualmente; arte propio (hoy TorchLamp/Column vanilla teñidos; rutas `Things/Building/CVG/Formations/<Nombre>`); formaciones eléctricas (Qi-Tech).
