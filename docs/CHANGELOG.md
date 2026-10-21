# Registro de cambios (notas de versión para Steam Workshop)

Cada cambio a un mod publicado se anota acá, por mod, en **"Sin publicar"**. Al subir: se sube `modVersion` en el `About.xml`
del mod, se copia el texto en inglés a las *change notes* de Steam y la sección pasa a **"Publicadas"** con la fecha.
Pedido del usuario (2026-10-21): versionar todo lo que se modifica en los mods y addons para subirlo después.

Estado de partida: los 15 mods se publicaron (ocultos) en la **0.1.0** (tag git `v0.1.0`; ids en `About/PublishedFileId.txt`).

---

## Sin publicar

### Convergence: Qi-Tech → 0.2.2
- Compatibilidad con Character Editor: el rifle de inducción (dos variantes, `Graphic_Random`) hacía fallar su ventana de objetos
  (`TextureTool.GetGraphicForRandom`, miles de "GUI Error"). Ahora no se inclina al azar en el suelo, que era lo que envolvía su gráfico.

**Change notes (Steam):**
```
0.2.2
- Fixed: compatibility with Character Editor (the qi induction rifle broke its item window).
```

---

## Publicadas

### Convergence: Qi-Tech 0.2.1 — subida 2026-10-21
- El exoesqueleto ya no se tiñe de gris: heredaba el `colorGenerator` de la armadura de reconocimiento vanilla (gris 0,33-0,9).
  Ahora se ve con los colores del arte (`ColorGenerator_White`). Los exoesqueletos ya fabricados conservan su color guardado.

**Change notes (Steam):**
```
0.2.1
- Fixed: the Qi-Tech exoskeleton no longer comes out tinted grey; it shows its real colours.
```

### Convergence: Core 0.1.1 — subida 2026-10-21
- Nuevo material **cristal primordial** (`CVG_PrimordialCrystal`): 10 piedras espirituales en la mesa de cantero. Material común para
  los módulos (primer uso: el resonador espiritual de Qi-Tech). Arte del usuario.

**Change notes (Steam):**
```
0.1.1
- New material: primordial crystal, made from 10 spirit stones at the stonecutter's table. Shared by the modules (first used by Qi-Tech's spirit resonator).
```

### Convergence: Formations 0.1.1 — subida 2026-10-21
- Torretas: los cabezales de la estatua guardiana, la ballesta rúnica y la ballesta del trueno estaban girados 90° respecto del disparo;
  ahora apuntan al blanco (como el arte de Ludeon, al este).
- La estatua guardiana lanza **espadas de qi** (nuevo proyectil `CVG_Projectile_GuardianQiSword`, sonido de energía) en vez de agujas.

**Change notes (Steam):**
```
0.1.1
- Fixed: the guardian statue, rune crossbow and thunder ballista now face the enemy they shoot at (their heads were turned 90 degrees).
- The guardian statue now throws qi swords instead of golden needles, as its lore says.
```

### Convergence: Qi-Tech 0.2.0 — subida 2026-10-21
- **Ingeniería Espiritual:** nueva pestaña de investigación con todas las investigaciones de Qi-Tech; investigación científica en el banco
  avanzado, sin cultivadores ni Dao. Todo bloqueado hasta la Convergencia (bloqueo real). Tres investigaciones nuevas: Fundamentos del Qi,
  Resonancia Espiritual, Instrumentación Espiritual Avanzada.
- **Resonador espiritual** (2×2, 250 W): instalación del banco avanzado, necesaria para la Instrumentación Espiritual Avanzada. Arte del usuario.
- `CVG_Research_QiTech` (mismo defName) ahora requiere Microelectrónica y Fundamentos del Qi en vez de la red de qi de Formations.
- **Armas:** rifle de inducción de qi (grado 1, dos variantes), rifle de qi concentrado (grado 2), cañón de compresión de qi (grado 3).
- **Torretas:** torreta de inducción de qi (grado 2) y cañón de contención de qi (grado 3), con energía y piedras espirituales por disparo;
  el disparo sale de la punta del cañón.
- Arte del usuario para las 3 armas, las 2 torretas, los 3 proyectiles de qi, el resonador, el convertidor de qi y el exoesqueleto (puesto, 5 cuerpos × 3 vistas, e ícono). Lore y traducción (Spanish, SpanishLatin).
- Partidas guardadas: se conserva lo ya investigado; las investigaciones de Qi-Tech pasan de la pestaña Cultivo a Ingeniería Espiritual.

**Change notes (Steam):**
```
0.2.0
- New research tab "Spiritual Engineering": the scientific road to qi, researched at the hi-tech bench with no cultivators needed. Locked until the Convergence. All Qi-Tech research moved here (saved progress is kept).
- New research: Qi Fundamentals, Spirit Resonance, Advanced Spirit Instrumentation.
- New building: spirit resonator, a research facility for the hi-tech bench (needs power and primordial crystals).
- New weapons: qi induction rifle (grade 1), concentrated qi rifle (grade 2), qi compression cannon (grade 3). Any colonist can make and use them.
- New turrets: qi induction turret (grade 2) and qi containment cannon (grade 3). They need power and burn spirit stones per shot.
- Qi-Tech no longer needs the Formations qi network research to start.
- New art for the converter, the exoskeleton, the weapons, the turrets and the qi bolts.
```
### 0.1.0 — los 15 mods (primera publicación, oculta)
