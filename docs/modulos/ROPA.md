# Módulo: Ropa, armaduras y telas (Convergence: Apparel)

Mod aparte. Depende del Core + Resources (metales espirituales) y, para las fibras de plantas, de Fauna & Flora
(o las define él si Fauna & Flora no está). Armas: en Alchemy & Artifacts (forja). Todo sin electricidad.

## 1. Materiales textiles (stuff de tela, categoría Fabric)

| Material | De dónde sale | Rasgo |
|---|---|---|
| **Algodón de las Nubes** (云棉, "algodón ancestral") | Planta espiritual cultivable en tierra espiritual; silvestre muy rara tras la Convergencia | Tela base de cultivador: ligera, aislante, +regeneración de qi pequeña |
| **Seda de gusano espiritual** (灵蚕丝) | Gusanos de seda espirituales (criados en morera espiritual) | Tela fina: belleza alta, resistencia cortante media, +estabilidad del Corazón del Dao |
| **Seda de fuego** | Gusanos del árbol de fuego / Fénix (evento) | Inmune al fuego, aislante del frío |
| **Lino de loto de nieve** | Loto de Nieve (biomas fríos) | Aislante del frío extremo, Yin |
| **Cuero de bestia espiritual** | Carnear bestias espirituales despiertas (nivel alto → mejor cuero) | Resistente; el nivel de la bestia sube la calidad |
| **Escama de dragón / caparazón de tortuga** | Bestias élite o mudas de linajes (evento raro) | Para armaduras de élite |

## 2. Ropa de cultivador

| Prenda | Capa | Notas |
|---|---|---|
| **Túnica de discípulo externo** | Exterior | Básica, barata; color de secta |
| **Túnica de discípulo interno** | Exterior | +velocidad de cultivo |
| **Túnica de anciano** | Exterior | +qi máximo, +impacto social |
| **Hanfu / ropa interior de seda** | Piel | Comodidad, belleza |
| **Sombrero de bambú** (斗笠) | Cabeza | Estilo de espadachín errante; protege del sol y la lluvia |
| **Corona / horquilla de jade** | Cabeza (adorno) | +Corazón del Dao |
| **Velo de cultivadora** | Cara | Estilo; leve resistencia mental |
| **Botas de nube** | Pies (si se usan capas de pies) | +velocidad de movimiento |
| **Cinturón de talismanes / calabaza** | `CVG_Waist` | Lleva talismanes/píldoras (ya previsto en la capa) |

Colores por secta: la ropa toma el color de la facción (como las túnicas imperiales) para que cada secta se reconozca.

## 3. Armaduras

| Armadura | Material | Notas |
|---|---|---|
| **Armadura de escamas** (鱼鳞甲) | Acero espiritual / cuero de bestia | Media, flexible |
| **Armadura de placas de jade** | Jade espiritual | Buena contra energía/qi, frágil contra contundente |
| **Armadura de plata lunar** | Plata lunar | Ligera; fuerte contra lo demoníaco y el vacío (Anomaly) |
| **Armadura de escama de dragón** | Escama de dragón | Élite: alta resistencia y al fuego |
| **Túnica de batalla rúnica** | Seda espiritual + runas | Para cultivadores: sin penalización de movimiento, con grado espiritual |
| **Armadura Qi-Tech** [Tecno] | Acero espiritual + componentes | Exoesqueleto con núcleo de qi (módulo Qi-Tech) |

**Grado espiritual de la armadura:** como las armas (DEFENSA.md §1), la armadura tiene grado. Contra técnicas de un
reino superior, una armadura mortal protege poco.

## 4. Joyería y accesorios (capas ya creadas)
`CVG_Neck` (colgantes de jade, el jade del Maestro), `CVG_Finger` (anillos, anillo espacial), `CVG_Waist`
(talismanes, calabaza). Bonus de cultivo, Corazón del Dao o elemento.

## 5. Mesas y producción
- **Telar espiritual** (sin electricidad): tejer telas espirituales y ropa de cultivador.
- **Morera espiritual + criadero de gusanos de seda** (con Fauna & Flora).
- Las armaduras de metal se hacen en la **Forja Espiritual** (por nivel).
- **Afinidad de linaje:** el Fénix sigue siendo el mejor en la forja; un futuro rasgo "Sastre Espiritual" para el telar.

## 6. Prioridad
1. Algodón de las Nubes (planta + tela) y túnicas de discípulo externo/interno.
2. Cuero de bestia espiritual (sale del despertar de animales de M4b).
3. Armadura de escamas de acero espiritual y túnica de batalla rúnica.
4. Seda espiritual, sombrero de bambú, joyería de jade.
5. Armaduras de élite y Qi-Tech.

## 7. Implementación v0.1 (mod `Mods/Convergence.Apparel`, 2026-10-15)
Solo XML. Sin electricidad ni investigación. Depende de Core + Resources; con **Sects** (parche `Patches/SectRobes.xml`) los miembros de
cada secta llevan la túnica de su rango en el color de su secta (`apparelRequired` + `apparelColor`); con **Alchemy** la armadura también se hace en la Forja Espiritual.

| Hecho | Detalle |
|---|---|
| Algodón de las Nubes | `CVG_Plant_CloudCotton` (cultivo normal en suelo; la tierra espiritual no existe aún, cuando exista que dé bonus) → `CVG_CloudCotton` → telar → `CVG_CloudCloth` (stuff Fabric) |
| Telar espiritual | `CVG_SpiritLoom` (madera + jade, sin energía ni investigación); receta `CVG_WeaveCloudCloth`; también confecciona las túnicas |
| Seda espiritual | `CVG_SpiritSilk` (stuff Fabric). **No existe aún la criatura que la produce** (Araña Tejedora de Jade, Fauna): hoy es bien de comercio (la vende el Pabellón, su nave y los mercaderes errantes, parche de Sects). Cuando exista: receta desde el capullo |
| Cuero espiritual | `CVG_SpiritLeather` (Core): lo deja cualquier bestia despierta al carnearla (`DefModExtension_SpiritBeast.leatherDef`, × tamaño × nivel) |
| Túnicas | `CVG_Apparel_RobeOuter/Inner/Elder` (capa Shell; qi regen, velocidad de cultivo, qi máx e impacto social por rango) |
| Hanfu, sombrero de bambú, corona de jade | `Hanfu` (OnSkin), `BambooHat` (Overhead), `JadeCrown` (+3% avance) |
| Armadura | `ScaleArmor` (escamas de metal/cuero espiritual, grado espiritual 1, Middle) y `RuneBattleRobe` (grado 1, sin penalización de movimiento) |

**Pendiente:** velo, botas de nube, joyería (`CVG_Neck/Finger/Waist` no existen aún como capas), armaduras de jade/plata lunar/escama de dragón, seda de fuego, lino de loto,
efecto de los materiales en el cultivo (hoy el bonus va en cada prenda), estilo/Ideología (túnicas por memes), producción de seda.

**Arte pendiente y ruta provisional** (el texPath actual es vanilla Core teñido; cambiar a la ruta propia cuando exista el PNG; las prendas necesitan `_north/_east/_south/_west` en `wornGraphicPath`, el icono de suelo es `<ruta>` sin sufijo):

| Ruta propia | Provisional (vanilla) |
|---|---|
| `Things/Pawn/Humanlike/Apparel/CVG/RobeOuter` | `Duster/Duster` |
| `.../CVG/RobeInner` | `Duster/Duster` |
| `.../CVG/RobeElder` | `Duster/Duster` |
| `.../CVG/Hanfu` | `ShirtBasic/ShirtBasic` |
| `.../CVG/BambooHat` | `CowboyHat/CowboyHat` |
| `.../CVG/JadeCrown` | `TribalHeaddress/TribalHeaddress` |
| `.../CVG/ScaleArmor` | `FlakVest/FlakVest` |
| `.../CVG/RuneBattleRobe` | `Duster/Duster` |
| `Things/Item/Resource/CVG_CloudCotton`, `CVG_CloudCloth`, `CVG_SpiritSilk` | `Cloth` teñido |
| `Things/Building/Production/CVG_SpiritLoom` | `TableTailorHand` teñido |
| `Things/Plant/CVG/CloudCotton` (+ `_Immature`) | `CottonPlant` teñido |
| `Things/Item/Resource/CVG_SpiritLeather` | `Leather` teñido |
