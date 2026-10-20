# Tanda de contenido inspirado en Tale of Immortal y Amazing Cultivation Simulator

Pedido del usuario (2026-10-16), después del repaso mod por mod: "ver qué nos faltó y agregarlo: lo que hay en Tale of Immortal
y Amazing Cultivation Simulator; eventos, ítems, espadas, arcos, dagas, espadas de dos manos, sables, también colgantes, anillos,
aros; accesorios, comidas, recursos, etc.". Principio n.º 2 (versión wuxia de lo que funciona) y "todo lleva lore".

| Categoría | Qué | Dónde | Estado |
|---|---|---|---|
| Armas cuerpo a cuerpo | Espada recta (jian), sable (dao), daga, espada de dos manos (zhanmadao), lanza/alabarda (guandao), bastón, abanico de hierro; con grado espiritual y materiales wuxia (acero espiritual, plata lunar, bambú de jade, melocotonero) | Alchemy (forja) | ✅ `Weapons_Wuxia.xml`: jian, dao, daga bishou, zhanmadao, guandao, bastón gun, abanico de hierro (bases vanilla medievales/neolíticas, recetas en la herrería que la forja espiritual copia). Grado espiritual 0; **el material da un mínimo** (`DefModExtension_SpiritGradeMaterial`: acero espiritual, plata lunar, hierro frío, cristal, bambú de jade y melocotonero 1; oro del caos 2), también para armas vanilla. Las espirituales de `Weapons.xml` siguen siendo grado 2-3 |
| Armas a distancia | Arco recto, arco compuesto de cuerno de bestia, ballesta de repetición, dardos/agujas voladoras | Alchemy | ✅ Arco recto (de cualquier madera, bambú incluido), arco compuesto de cuerno de bestia (núcleo de bestia, grado 1, investigación Greatbow), ballesta de repetición zhuge (ráfaga de 3) y agujas voladoras (ráfaga de 2, metal: grado por material) |
| Accesorios | **Capas nuevas** `CVG_Neck`, `CVG_Finger`, `CVG_Ear`, `CVG_Waist` (ApparelLayerDef); colgante de jade, anillo (incl. **anillo espacial** con inventario extra), aros, cinturón de talismanes / calabaza; bonus de cultivo, Corazón del Dao o elemento | Apparel | ✅ `ApparelLayers.xml` + `Accessories.xml`: colgante de jade, amuleto del Corazón del Dao (umbral de crisis −5 %, avance +3 %), anillo espacial (mano izquierda, capacidad de carga +60) y anillo de reunión de qi (mano derecha: se llevan los dos), aros de los cinco elementos (cultivo ×1,15 si la raíz tiene ese elemento, `StatPart_ElementalResonance`), cinturón de talismanes (daño recibido −5 %) y calabaza medicinal. Sin gráfico puesto: no tapan la ropa |
| Comidas | Platos de ACS/ToI que falten (bollos rellenos, sopa de bestia, vino de flor de melocotón, pastel de luna…) | Alchemy / FaunaFlora | 🔄 |
| Recursos | Materiales de ACS/ToI que falten (hierro meteórico, arena estelar, plumas/escamas de bestia por tipo…) | Resources / FaunaFlora | ✅ Hierro meteórico (+ receta de refinado a acero espiritual con 1 piedra espiritual), arena estelar (Resources `Celestial.xml`); pluma de grulla, colmillo de lobo de viento y quitina del inframundo como productos de carnicería (FaunaFlora `BeastParts.xml` + parche). Las comidas siguen en Alchemy |
| Eventos | Más historias y encuentros (secretos: detalles en docs/dev/EVENTOS_SECRETOS.md) | Stories | ✅ +9 eventos (4 de sectas encadenados, tributo, traspaso de discípulos y 3 de recursos celestiales); detalles en docs/dev/EVENTOS_SECRETOS.md |

Fuentes: notas de análisis internas (Tale of Immortal y mods de cultivo de la comunidad), no publicadas.

## Pulido (pedido del usuario, 2026-10-16)
- **Sistemas y mecánicas ya agregados:** revisarlos jugando (con -cvgrendertest, autotests y partidas de prueba): balance, que se entiendan, que no choquen entre sí (red de qi, técnicas, injertos, marionetas, isla, reinos secretos).
- **Interacciones entre facciones:** las sectas como facciones vivas (estilo ACS/ToI y More Faction Interaction, que el usuario usa): comercio entre sectas, alianzas y rivalidades, pedidos de ayuda, torneos entre sectas, tributos, discípulos que se transfieren, guerras de sectas; sobre las relaciones y misiones vanilla. ✅ Hecho: ver [INTERACCION_SECTAS.md](INTERACCION_SECTAS.md).
