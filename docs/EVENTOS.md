# Catálogo de eventos (borrador v0)

Inspiración: Vanilla Events Expanded (más variedad en cada partida) y los tópicos del xianxia.
Todos los eventos requieren que haya ocurrido la **Convergencia**, salvo los marcados como "pre".

Tipos técnicos en RimWorld:
- **Condición de mapa** (`GameConditionDef`): dura días y modifica reglas. Por ejemplo, la aurora o la luna de sangre.
- **Incidente** (`IncidentDef`): algo que pasa una vez (llegada, ataque, hallazgo).
- **Misión** (`QuestScriptDef`): propuesta con recompensa.
- **Calendario** (`GameComponent` propio): fechas fijas por cuadrimestre/año.

---

## 1. Calendario celestial (el corazón)

El cielo del Reino Celestial superpuesto al rimworld crea ciclos. Dan ritmo a la partida: el jugador planifica
avances y tribulaciones según el calendario.

| Evento | Tipo | Efecto |
|---|---|---|
| **Marea de Qi** | Condición (semana) | +qi en todo el mapa; meditar rinde el doble. Buen momento para avanzar de etapa |
| **Reflujo del Qi** | Condición | Qi escaso; meditar casi no sirve; las técnicas cuestan más |
| **Luna del Demonio Interno** | Condición (días) | El Corazón del Dao baja más rápido; riesgo alto de desviación de qi; el camino demoníaco rinde más |
| **Eclipse Celestial** | Condición | Las tribulaciones son más fuertes, pero dan más recompensa |
| **Conjunción de los Cinco Elementos** | Calendario (anual) | Cada día potencia un elemento (metal, madera, agua, fuego, tierra) |
| **Lluvia Espiritual** | Condición | Crecen hierbas espirituales; las plantas maduran rápido |
| **Festival de los Fantasmas Hambrientos** | Calendario (anual) | Espíritus vagan; ofrendas dan buena fortuna, ignorarlos trae desgracias |

## 2. Oportunidades

| Evento | Tipo | Efecto |
|---|---|---|
| **Pilar de luz** (tesoro nace) | Incidente | Un tesoro aparece en el mapa o cerca; otras facciones vienen a disputarlo |
| **Cueva de herencia** (洞府) | Misión / sitio | La morada de un cultivador antiguo, con pruebas, trampas y técnicas |
| **Reino secreto se abre** | Misión / sitio | Fragmento del Reino Celestial por tiempo limitado; solo entran ciertos reinos de cultivo |
| **Casa de subastas** | Misión | Caravana de subasta de una secta: píldoras, técnicas, tesoros raros |
| **Iluminación súbita** | Incidente | Un colono comprende el Dao: salto de progreso inmediato |
| **Bestia espiritual herida** (pre) | Incidente | Animal raro que se puede domar o matar para obtener un núcleo |

## 3. Amenazas

| Evento | Tipo | Efecto |
|---|---|---|
| **Marea de bestias** (兽潮) | Incidente grande | Oleada de bestias demoníacas; escala con los reinos de la colonia |
| **Ataque de secta demoníaca** | Asalto | Cultivadores con técnicas; pueden secuestrar discípulos |
| **Tribulación errante** | Incidente | La tribulación de un extraño cae cerca de la colonia: rayos y daño colateral |
| **Demonio interno** | Incidente individual | Un colono con Corazón del Dao bajo entra en crisis: pelea contra sí mismo (o lo controla) |
| **Desviación de qi colectiva** | Condición | Interferencia dimensional: los avances fallan más |
| **Purga del Imperio** | Asalto | El Imperio ataca para "controlar" a los cultivadores |
| **Cazadores corporativos** | Asalto / misión | Una corporación quiere capturar cultivadores para experimentos |

## 4. Sociales y de secta

| Evento | Tipo | Efecto |
|---|---|---|
| **Cultivador errante pide refugio** | Incidente | Puede unirse, enseñar algo o traicionar |
| **Discípulo busca maestro** | Incidente | Un joven con buena raíz quiere unirse a un cultivador de la colonia |
| **Torneo de sectas** | Misión | Duelo entre colonos y discípulos de sectas; da reputación y premios |
| **Alianza matrimonial** | Misión | Una secta propone unir familias |
| **Reto por el honor** | Misión | Un rival desafía a tu cultivador más fuerte (como los duelos de título de Isekai) |
| **Venganza** | Misión | Familiares de un enemigo que mataste vienen a por ti (tópico clásico) |

## 5. Eventos del Maestro

| Evento | Efecto |
|---|---|
| **La llegada** | Con la Convergencia: el Maestro herido cae por un portal |
| **Lecciones** | Misiones de enseñanza |
| **La partida** | El Maestro se va (lo llaman, lo persiguen o lo secuestran) y deja el fragmento de jade |
| **Ecos del jade** | Pistas periódicas que abren las misiones de búsqueda en el mapa del mundo |

---

## Prioridad

- **MVP:** Convergencia, Marea de Qi, Luna del Demonio Interno, Demonio interno (individual) y el Maestro (llegada, lecciones y partida).
- **Después:** Pilar de luz, Marea de bestias, Tribulación errante, Reino secreto.
- **Más adelante:** el resto, junto con el módulo **Mundo y eventos**.
- Cada evento con su opción en los ajustes del mod (activar/desactivar y frecuencia).
