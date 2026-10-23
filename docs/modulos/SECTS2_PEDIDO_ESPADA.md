# Pedido de diseño: Secta de la Espada Sin Forma completa (etapa S2)

Para diseñar con otra IA y devolvérmelo todo junto. Base: `SECTS2_DISENO.md` §1 (identidad acordada) y `docs/lore/SECTAS.md` §1.1 (lore del
Pico Azur, que hereda). La mecánica ya está programada (etapa S1): solo hace falta **contenido**. En cada punto pongo lo que ya existe y una
propuesta mía por defecto; si te sirve, alcanza con confirmarla.

**Formato de cada texto:** nombre en inglés, español y chino (con pinyin) + descripción con lore de 2-3 frases en inglés y en español.

---

## 1. Constitución: Cuerpo de Espada (3 grados) + Cuerpo de Espada Celestial (aparte)
Hoy el Core tiene dos cuerpos especiales sueltos:
- `CVG_Body_Sword` "sword body": solo nace en colonos **sin raíz espiritual** (camino del cuerpo); +30% velocidad de templado del cuerpo,
  Melee +3. Probabilidad al nacer 0,8%.
- `CVG_Body_CelestialSword` "celestial sword body": colonos con raíz; Melee +3. Probabilidad 0,4%.

Necesito:
- **Nombres y lore** de Común, Superior, Superior+ (el 3.er grado) y Celestial.
- **Decidir:** ¿el Cuerpo de Espada de la secta sirve para cultivadores con raíz **y** sin raíz, o sigue siendo solo del camino del cuerpo?
  (Propuesta: para ambos; el actual "sword body" pasa a ser el grado Común.)
- **Efectos por grado.** Propuesta, chica y que crece:

| Grado | Melee | Desvío de proyectiles (con espada) | Velocidad de maestría de técnicas de espada | Otro |
|---|---|---|---|---|
| Común | +2 | 8% | +10% | — |
| Superior | +3 | 14% | +20% | — |
| Superior+ | +4 | 20% | +30% | — |
| Celestial | +5 | 25% | +50% | La tribulación lo "reconoce": −10% de daño de tribulación |

- **Probabilidad en los discípulos** de la secta. Propuesta: Común 35%, Superior 8%, Superior+ 2%, Celestial 0,3%.

## 2. Ideología de la secta (requiere Ideology)
Del Sects viejo ya existe el meme **Sword Way** ("La espada no es buena ni mala; lo es la mano. La maestría se gana cortando y templando la
Voluntad de Espada"). Necesito:
- **Meme:** confirmar el nombre y el texto, o reemplazarlos. Ícono: ver §8.
- **Preset** de ideoligión (el que se elige al empezar): nombre, descripción y qué preceptos lleva. Preceptos que ya existen para elegir:
  cultivo (obligatorio/venerado/libre/mal visto), alquimia, extracción de esencia, artes demoníacas, duelos (alentados/permitidos/
  prohibidos), jerarquía (mandan los ancianos/todos iguales), retiro de avance. Más los de vanilla que quieras.
- **3 roles** (nombre, descripción y habilidad de vanilla de cada uno). Propuesta:
  - **Maestro de secta** (líder): habilidades de líder de vanilla.
  - **Gran Anciano** (guía moral): habilidades de guía moral de vanilla.
  - **Maestro de Espadas** (especialista; requiere Cuerpo de Espada de cualquier grado; se desbloquea primero): ¿qué habilidad? Por ejemplo,
    "inspirar combate" de vanilla, o una propia.

## 3. Ritual: Templado del Cuerpo de Espada
Lo dirige el Maestro de Espadas; el sujeto es un colono. Cada vez sube **un** paso: sin cuerpo → Común → Superior → Superior+ → Celestial.
Necesito:
- **Nombre, descripción y discurso** del ritual (2-3 frases de lore de qué pasa en la ceremonia).
- **Costo por paso.** Propuesta: 40 piedras espirituales (→ Común), 80 + 5 jade espiritual (→ Superior), 150 + 15 jade (→ Superior+),
  300 + 40 jade + 1 núcleo de bestia (→ Celestial).
- **Probabilidad de éxito según la calidad del ritual** (de 0% a 100%). Propuesta:

| Paso | Calidad 0% | 50% | 100% |
|---|---|---|---|
| → Común | 30% | 70% | 95% |
| → Superior | 10% | 40% | 70% |
| → Superior+ | 3% | 20% | 45% |
| → Celestial | 0% | 5% | 15% |

- **Textos de los 4 resultados** de vanilla (terrible, aburrido, satisfactorio, espectacular): una frase cada uno, sobre la ceremonia.
- ¿Tiempo mínimo entre rituales? Propuesta: el mismo que cualquier ritual de vanilla (sin agregar nada).

## 4. Facción y discípulos
- **Color** de la secta (vestimenta). Propuesta: blanco con azul acero, como el Pico.
- **Ropa** de Apparel que encaja (nombres de prendas del módulo Apparel o de vanilla).
- **Tipos de cuerpo excluidos:** el diseño dice "Hulk y Tiny"; en RimWorld no existe "Tiny" en adultos. ¿Es **Thin** (delgado)?
- **Rangos de discípulos** (4 + comerciante): nombres y reinos. Hoy: externo (Refinamiento de Qi), interno (Fundación), anciano (Núcleo
  Dorado), forjador (comerciante). ¿Sumamos "Hoja del Pico" (núcleo, Fundación 3-4) como en el viejo?
- **Personajes con nombre** del lore viejo (Lin Wuchen, Shen Ruoxue, Gu Hengyue, Yan Zhao): ¿se quedan?

## 5. Comercio
- Además de las espadas de calidad (Excelente 75%, Obra maestra 20%, Legendaria 5%, ya programado), ¿qué más vende y compra?
  Propuesta: vende metales espirituales, manuales de espada y piedras espirituales; compra espadas, metales y jade.

## 6. Los 5 favores (permisos de Royalty)
Para cada uno: nombre y lore EN/ES, título mínimo, costo en contribución y enfriamiento en días. Propuesta:

| Favor | Título mínimo | Contenido | Enfriamiento | Costo fuera de enfriamiento |
|---|---|---|---|---|
| Recursos espirituales (ya hecho) | Amigo | 40 piedras + 3 jade | 20 días | 2 |
| Discípulos espadachines (ya hecho) | Invitado de Honor | 3 discípulos | 30 días | 4 |
| Espada Obra maestra | Protector | 1 espada de la secta | 45 días | 8 |
| Espada Legendaria | Anciano Invitado | 1 espada | 60 días | 16 |
| Ataque de espadachines | Anciano Invitado | ¿cómo es? (propuesta: "lluvia de espadas", un bombardeo de qi de espada sobre una zona) | 60 días | 12 |

## 7. Títulos honorarios
Ya están escritos (Amigo, Invitado de Honor, Protector, Anciano Invitado de la Espada Sin Forma), con su lore. Confirmalos o reemplazalos.

## 8. Arte necesario
- Ícono del **meme** (64×64, sobre fondo transparente).
- Íconos de los **3 roles** (64×64).
- Ícono del **ritual** (64×64).
- (Opcional) ícono propio de la **facción**: hoy uso una copia del ícono del Pico Azur.
- (Opcional) ícono de la **contribución de secta**: hoy uso el del Camino de la Espada del Core.
