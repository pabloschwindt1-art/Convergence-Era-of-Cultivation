# Convergence Sects S2 — Espada Sin Forma: decisiones de diseño (puntos 1–7)

**Destinatario: Claude Code.** Este documento sustituye las propuestas por defecto de `SECTS2_PEDIDO_ESPADA.md` cuando hay conflicto. Implementar/ajustar únicamente en función de las decisiones indicadas y contrastar con el Core, Sects, Stories, Royalty e Ideology reales. No generar arte: el punto 8 se diseña por separado. Mantener lore en inglés y español, priorizar XML y no alterar el balance compartido del Core sin validación.

## 1. Constitución — Cuerpo de Espada

- Una misma constitución progresiva, válida **con y sin raíz espiritual**, y utilizable por cultivo espiritual o corporal.
- Grados, en orden:
  1. **Cuerpo de Espada de Hierro** (Iron Sword Body).
  2. **Cuerpo de Espada de Acero Frío** (Cold Steel Sword Body).
  3. **Cuerpo de Espada de Hierro Estelar** (Star-Iron Sword Body).
  4. **Cuerpo de Espada de Jade Celestial** (Celestial Jade Sword Body).
- Los nombres son etapas simbólicas de refinamiento, **no materiales implantados**.
- Revisar y migrar/reutilizar sin duplicación `CVG_Body_Sword` y `CVG_Body_CelestialSword` existentes; preservar compatibilidad razonable con partidas guardadas.
- Bonificaciones exactas y distribución entre NPC **no aprobadas aún**: proponer valores prudentes comparables con el Core, sin apilar multiplicadores desequilibrados. El grado máximo debe ser raro.

## 2. Ideology

- Conservar meme **Sword Way** y preset temático de **Espada Sin Forma**.
- Preset: cultivación venerada, duelos alentados, autoridad de ancianos y artes demoníacas prohibidas; reutilizar preceptos vanilla/existentes cuando sea posible.
- **No imponer prendas obligatorias por defecto** (ni burka, hiyab ni ninguna otra). Las preferencias de vestimenta las decide quien edita la ideología. Revisar herencia de restricciones en presets.
- Tres roles: **Maestro de Secta** (líder vanilla), **Gran Anciano** (guía moral vanilla), **Maestro de Espadas** (especialista, el primero que los fieles desean cubrir).
- El rol Maestro de Espadas exige cualquier grado de Cuerpo de Espada; dar función relacionada con inspiración al combate, preferentemente reutilizando una habilidad existente.
- Penalización de ánimo por falta de ese especialista **solo desde 4 creyentes** de esa ideología, sin alterar globalmente otras ideologías ni sus requisitos.
- Ideology es requisito de estas funciones, pero conservar separación adecuada de dependencias de DLC. **Royalty sí es obligatorio para Sects**, decidido previamente.

## 3. Ritual — Templado del Cuerpo de Espada

- Ritual de Ideology dirigido por un **Maestro de Espadas que tenga Cuerpo de Espada**; requiere **al menos otros dos miembros** de la ideología y altar/punto de ritual.
- Puede implantar el primer grado o avanzar **exactamente un grado por ceremonia**.
- Si falla: no baja ni pierde grado; materiales consumidos.
- La calidad del templo/ritual eleva la probabilidad; valores aprobados:

| Ascenso | Probabilidad base | Máximo en templo excelente |
|---|---:|---:|
| Sin cuerpo → Hierro | 80 % | 100 % |
| Hierro → Acero Frío | 60 % | 100 % |
| Acero Frío → Hierro Estelar | 40 % | 90 % |
| Hierro Estelar → Jade Celestial | 20 % | **55 % (tope absoluto)** |

- Para los primeros tres ascensos, bonificación de calidad hasta **+50 puntos porcentuales**, topada al 100 %. Para Jade Celestial, bonificación máxima **+35 puntos**, tope estricto **55 %**.
- Costos de materiales y cooldown **pendientes de balance técnico**: proponer usando recursos ya existentes en Convergence; no copiar sin más costos del documento previo.

## 4. Facción y discípulos

- Nombre: **Secta de la Espada Sin Forma — Formless Sword Sect — 无形剑宗 (Wúxíng Jiànzōng)**. Filosofía: la espada física es un instrumento; la verdadera espada es la intención (*Jian Yi*).
- Combatientes de la secta: **solo melee, con espadas**, sin armas de distancia; todos especializados como cultivadores de espada; composición de grupos aleatoria dentro de las reglas de reino/escalado del Core.
- Siluetas permitidas: Male, Female, Thin; excluido Hulk. `Tiny` se mencionó antes por error: no hay que inventar ese body type para adultos.
- Identidad visual provisional: **blanco y azul acero**, reutilizando ropa de Apparel/vanilla. No exigir esa ropa en preceptos de Ideology.
- Conservar cuatro escalones narrativos de discípulos (externo, interno, principal, anciano) más comerciante forjador, adaptados a los reinos realmente implementados y al escalado del Core.
- Conservar **Lin Wuchen, Shen Ruoxue, Gu Hengyue y Yan Zhao**, adaptando sus biografías al nuevo lore; personajes nombrados que sean maestros, ancianos o sabios deben tener poder, maestría y constitución coherentes con su jerarquía. Hoy no superar Núcleo Dorado porque Alma Naciente no está implementado; respetar escalado para encuentros hostiles.
- Preservar y adaptar Stories y referencias existentes; no borrar facciones o defs antiguos sin plan de migración.

## 5. Comercio

- Especialidad comercial: **espadas**, tanto en caravanas como asentamientos.
- Espadas vendidas: calidad mínima **Buena**, con Excelente y una proporción **baja** de Obra maestra. **Jamás Legendaria en comercio normal**, ni visitante ni asentamiento. Determinar porcentajes de stock equilibrados (la tabla 75/20/5 del pedido anterior queda invalidada).
- Además del comercio ordinario adecuado para la facción, inventario variable **según stock** de ítems reales de Convergence: piedras espirituales, jade, hierbas, materiales de forja/cultivo, manuales y demás objetos que existan y correspondan.
- Reutilizar `ThingDef` actuales; no crear ítems nuevos para llenar comercio.

## 6. Favores de Royalty y permanencia de títulos

- **Los títulos honorarios obtenidos NO decrecen** por bajadas de buena voluntad. La relación diplomática puede condicionar temporalmente la disponibilidad de favores, sin revocar el título. No confundir el título con goodwill.
- Mantener cinco tipos de favor compartidos para las futuras sectas, con recompensa adaptada a la especialidad de cada una.
- **Propuesta inicial de costos competitivos, para contrastar y ajustar con la economía real de contribución del mod** (no valores aprobados de balance definitivo):

| Favor Espada Sin Forma | Título mínimo | Recompensa | Cooldown sugerido | Costo para reutilizar anticipadamente, si el sistema lo admite |
|---|---|---|---:|---:|
| Recursos espirituales | Amigo de la Secta | Lote moderado de piedras y jade | 20 días | 2 puntos |
| Refuerzos de espadachines | Invitado de Honor | 3 discípulos según escalado | 30 días | 4 puntos |
| Espada Obra maestra | Protector Honorario | 1 espada de ese grado | 45 días | 8 puntos |
| Espada Legendaria | Anciano Invitado | 1 espada excepcional, nunca en comercio | 60 días | 16 puntos |
| Ataque contra un objetivo válido | Anciano Invitado | Apoyo ofensivo de espadachines (evaluar mecanismo vanilla de Royalty) | 60 días | 12 puntos |

- Mantener **costos razonables y cooldowns suficientes** para que no se acumulen espadas legendarias ni ataques gratuitos. El ataque no está obligado a ser un bombardeo tipo «lluvia de espadas»: primero evaluar qué servicio de Royalty es reutilizable y apropiado.
- Priorizar permisos Royalty existentes; Claude debe validar requisitos reales de cada API y qué significa «contribución» en la implementación S1.

## 7. Títulos honorarios

Los títulos de nobleza del Imperio (conde, marqués, etc.) sirven como **modelo mecánico**, no como nombres para las sectas. Para las sectas, **usar estos cuatro títulos ya acordados**, sin alterar innecesariamente los títulos de la facción imperial vanilla:

1. **Amigo de la Secta — Sect Friend**.
2. **Invitado de Honor — Honored Guest**.
3. **Protector Honorario — Sect Protector**.
4. **Anciano Invitado — Guest Elder (客卿长老)**.

Son reconocimientos externos: el colono/la colonia conserva su independencia; no pasa a dirigir la facción ajena. Conservar/adaptar el lore ya escrito en S1. La progresión de títulos es **permanente**, pero los favores pueden suspenderse si la facción es hostil.

## Fuera de este envío: 8. Arte

**No solicitar ni generar íconos, vestimentas o sprites en esta etapa.** El arte lo estamos diseñando por separado y se entregará después.

## Entrega solicitada a Claude

Contrastar estas decisiones con la implementación S1 y `SECTS2_PEDIDO_ESPADA.md`, enumerar incompatibilidades concretas o decisiones técnicas estrictamente necesarias y continuar con la integración de los puntos 1–7 sin inventar mecánicas adicionales. Las cifras expresamente indicadas como propuestas deben validarse contra el balance real antes de fijarlas. Mantener documentación y registro de cambios.
