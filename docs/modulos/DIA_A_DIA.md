# El día a día: versión wuxia de lo que más se usa

Pedido del usuario (2026-10-15): el núcleo (cultivo, reinos, genes, aptitudes) está bien; falta **lo que se usa todos los días**.
Regla: de cada cosa vanilla de uso diario, su **versión wuxia** (mismo sistema, otra estética, material y lore). Se inventa
solo lo que no tiene equivalente. Se hace **por partes**, en el orden de cuánto se usa.
✅ existe · 🔄 parcial · ❌ falta

## Parte 1 — Energía, luz y temperatura (lo primero que se construye)
| Vanilla | Wuxia | Estado |
|---|---|---|
| Generadores, conductos, baterías | **Red de qi** (pozo de vena, formación de reunión, pilar, cristal acumulador) — RED_QI.md | ❌ (en cola) |
| Antorcha, lámpara, lámpara solar | Farol de papel, farol de piedra espiritual, cristal luminoso; **lámpara de crecimiento → Farol del Sol** (para cultivos de interior) | 🔄 (falta el de cultivo) |
| Calefactor / enfriador / ventilación | **Brasero de fuego espiritual** (calienta) y **Formación de escarcha** (enfría), sin electricidad; ventana-celosía | ❌ |
| Hoguera / estufa de leña | Brasero ritual ✅; **fogón de leña** (cocina) | 🔄 |

## Parte 2 — Mesas de trabajo de cada día
| Vanilla | Wuxia | Estado |
|---|---|---|
| Cocina (estufa) / carnicería | **Cocina de la secta** (fogón con wok) y **tajo de carnicero** | ❌ |
| Mesa de investigación | Escritorio de contemplación | ✅ |
| Mesa de sastre | Telar espiritual | ✅ |
| Herrería / mesa de maquinaria | Forja espiritual / horno de refinado | ✅ |
| Cantero (bloques) | **Taller del cantero** | ❌ (hoy se usa el vanilla) |
| Mesa de arte (esculturas) | **Mesa del tallador de jade** | ❌ |
| Destilería / cervecería | **Bodega de vino espiritual** (tinaja ✅ como decoración) | 🔄 |
| Laboratorio de drogas | Horno de alquimia (píldoras) | ✅ |
| Mesa de mecanoides / gestador | Taller de marionetas | ✅ |

## Parte 3 — Muebles de vida
| Vanilla | Wuxia | Estado |
|---|---|---|
| Camas (normal, doble, hospital) | Cama de secta ✅, cama doble de secta ✅, cama del sanador ✅ (cama médica: mejora la atención y la cirugía) | ✅ |
| Mesas y sillas de comedor | Mesa baja + taburete ✅; mesa redonda de banquete ✅ (3×3, punto de reunión) y silla tallada ✅ | ✅ |
| Estantes / almacenamiento | Estante de escrituras ✅, armario ✅; estante de almacén ✅ (3 de ancho), tinaja de grano ✅ (solo comida, sin deterioro), cofre de la secta ✅ | ✅ |
| Puertas | Puerta de luna ✅ y puerta lacada ✅ (más resistente), sobre `DoorBase` | ✅ |
| Muros / suelos | Rocas wuxia ✅ (bloques, losas); muro de bambú ✅ (solo con Resources; cuesta bambú, sin material) y suelo de tejas ✅ | ✅ |
| Recreo (TV, ajedrez, billar, herradura) | Go ✅, caligrafía ✅; mesa de mahjong ✅ (4 jugadores), pabellón de té ✅ (social), guqin ✅ (tipo de recreo propio `CVG_Music`) | ✅ |
| Bancos de jardín, macetas | Bonsái ✅, jardín de rocas ✅; banco de piedra ✅, macetero de loto ✅ | ✅ |

### Parte 3b — Arte (pedido del usuario)
| Vanilla | Wuxia | Estado |
|---|---|---|
| Escultura pequeña / grande / gran escultura (CompArt, calidad, historias) | **Esculturas de jade** pequeña / grande / gran escultura (Furniture, `Art_Jade.xml`), en la mesa del tallador de jade; siempre de jade (belleza 90 / 180 / 600); nombres y descripciones wuxia (`CVG_NamerArtWuxia`, `CVG_ArtDescription_JadeSculpture`: inmortales, bestias sagradas, fundadores, tribulaciones) con las historias de la colonia | ✅ |
| Arte de pared (Ideology/escultura en relieve) | **Pergamino de caligrafía** y **pintura de tinta** (Core, `Art_Wall.xml`): arte con calidad de pie sobre un soporte de madera, como la escultura chica vanilla (colgados del muro se veían mal), hechos en la mesa de caligrafía, que ahora también es mesa de trabajo | ✅ |

Arreglo de paso: las mesas wuxia que copian recetas (`DefModExtension_CopyRecipes`) también se suman a los `WorkGiverDef` de su mesa vanilla; antes nadie trabajaba en ellas (la cocina de la secta, la mesa del tallador…).

### Parte 3c — La vida del cultivador (revisión mod por mod)
| Vanilla | Wuxia | Estado |
|---|---|---|
| Lugar de fiesta / puesto de meditación (Royalty) | **Lugar de cultivo** (gratis, qi ×3) y **plataforma de meditación** (piedra o metal, con calidad, qi ×7); el cojín y los lugares se asignan a un colono como una cama | ✅ |
| Pensamientos del día | Avance logrado/fallido, tribulación superada o presenciada, desviación de qi; retiro (amante del retiro) y rango (joven maestro arrogante) | ✅ |
| Ficha: registros | Tiempo cultivando, avances, tribulaciones superadas, técnicas lanzadas | ✅ |
| Alertas | Listo para el avance, desviación de qi, sin lugar de cultivo, cultivador sin el trabajo activado | ✅ |
| Inspiraciones | Iluminación súbita, intención de la espada, fuego de la forja (solo cultivadores) | ✅ |
| Relatos para el arte | Avance, tribulación superada, muerte en la tribulación | ✅ |

## Parte 4 — Comida, medicina y consumo diario
| Vanilla | Wuxia | Estado |
|---|---|---|
| Comidas simples/finas/lujosas | Sopa espiritual ✅; **bollos al vapor** (simple, `MealCooked`), **arroz con carne espiritual** (fina: +10 qi y +10 sangre-qi), **banquete de la secta** (lujosa, con hierba espiritual: nutrido espiritual). Se cocinan en cualquier cocina: la de la secta copia las recetas de la cocina vanilla | ✅ |
| Pemmican / raciones | **Píldora de ayuno (bigu)**: ración de viaje como el pemmican (0,05 de nutrición, 120 días sin pudrirse) que además baja el hambre ×0,75 un rato; se prensa en el horno de alquimia | ✅ |
| Medicina herbal / industrial / glitterworld | **Ungüento de hierbas** (0,7 / 0,8), **píldora curativa** (1,0 / 1,0) y **elixir de jade** (1,6 / 1,3, horno de nivel 2), como `MedicineBase` vanilla | ✅ |
| Cerveza / humo / drogas | Vino espiritual (Furniture) ✅; **té de las nubes** (bebida social sin alcohol ni adicción: regeneración de qi ×1,1) ✅; **incienso** (droga suave de meditación: cultivo ×1,1, umbral de crisis −3%, vista y movimiento −5%) ✅ | ✅ |

Contenido en el mod Alchemy (`Defs/DailyConsumables.xml`), con lore en inglés y español. Texturas vanilla teñidas hasta que haya arte
propio. Autotest "daily consumables": ALL OK.

## Parte 4b — Flora y fauna de cada día (mod Fauna & Flora; ver FAUNA_FLORA.md)
| Vanilla | Wuxia | Estado |
|---|---|---|
| Animales de granja (gallina, vaca, oveja, alpaca) | Gallina de sangre de fénix, buey de nube, oveja de siete colores, araña tejedora de jade | ✅ (arte provisional) |
| Animales salvajes | Ciervo lunar, lobo del viento, grulla celestial, araña del inframundo; bestias despiertas | ✅ (arte provisional) |
| Cultivos (arroz, maíz, algodón, hierba curativa) | Arroz glutinoso ✅, té de las nubes ✅, loto ✅ (suelo húmedo y rico: vanilla no tiene cultivos de agua), morera ✅ (sus hojas, con la receta *criar gusanos de seda*, dan seda espiritual si hay Apparel); bambú ✅; algodón de las nubes ✅, hierbas espirituales ✅ | ✅ |
| Pesca (Odyssey) | 4 peces espirituales | ✅ |
| Árboles | Árbol espiritual ✅; Árbol espiritual ✅, pino milenario ✅ (crece 75 días, silvestre en bosques templados y boreales), melocotonero ✅ | ✅ |
| Arte | Animales en 3 vistas y plantas propias (reemplazar vanilla teñido) | ❌ |

## Parte 5 — Defensa diaria
| Vanilla | Wuxia | Estado |
|---|---|---|
| Torretas | Ballesta rúnica ✅, formaciones ✅ | ✅ |
| Sacos de arena / barricadas | Empalizada de bambú ✅ y muralla baja de piedra ✅ (cobertura baja y valla, en el menú de Seguridad) | ✅ |
| Trampas | Trampa de púas de qi ✅ (edificable, `Building_TrapDamager`; la `CVG_FormationTrap` de los reinos secretos sigue siendo solo de mazmorra) | ✅ |

## Parte 6 — Arte de todo lo anterior
Cada pieza nueva sale con su textura propia (FLUX para objetos/edificios, ver ARTE.md). Las provisionales de vanilla teñida se
reemplazan por partes, en el mismo orden.

### Implementación de la Parte 3 (2026-10-15)
`Mods/Convergence.Furniture/Common/Defs/Furniture3.xml` (generado; 14 edificios, un suelo, un tipo de recreo, 3 trabajos y 3 `JoyGiverDef`): camas sobre `BedBase`/`BedWithQualityBase`; mesa redonda sobre `TableGatherSpotBase`; estante, tinaja y cofre sobre `Building_Storage` (la tinaja con filtro fijo de comida); puertas sobre `DoorBase`; muro sobre `Wall` sin material (coste en bambú, `MayRequire` Resources); mahjong y té con `JoyGiver_InteractBuildingSitAdjacent` y `JobDriver_SitFacingBuilding`, guqin con `JoyGiver_InteractBuildingInteractionCell` y `JobDriver_WatchBuilding`. Texturas provisionales con la ruta final `Things/Building/CVG/Furniture/<Nombre>` en un comentario. Autotest `-cvgfurnituretest`.

### Implementación de las partes 4b y 5 (2026-10-15)
Cultivos en `Mods/Convergence.FaunaFlora/Common/Defs/Crops.xml` (plantas sobre `PlantBase`/`TreeBase`, alimentos sobre `PlantFoodRawBase`; parche de plantas silvestres para el pino) y defensa en `Mods/Convergence.Formations/Common/Defs/Defense3.xml` (sobre `Barricade` y `TrapSpike` vanilla). Texturas provisionales con la ruta final en un comentario `Final art:`. Autotest `-cvgcropstest`.
