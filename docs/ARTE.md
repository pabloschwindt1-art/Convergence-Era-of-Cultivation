# Guía de arte (para generar con Gemini u otra IA)

Flujo: **Claude escribe el prompt → Gemini genera → Claude procesa** (recorta vistas, quita el fondo, redimensiona,
nombra e integra en el mod).

## Plantillas oficiales: RimWorld Art Source
Texturas originales de vanilla y de todas las DLC, publicadas por Ludeon para hacer mods, en
una carpeta local `ArtSource/` (por DLC) que no forma parte de este repositorio. Base de formato e img2img para ropa (5 tipos de
cuerpo × 3 vistas), aspecto de razas (genes de Biotech) y criaturas. Se transforman en diseño propio; no se copian tal cual.

## Requisitos de RimWorld

| Tipo | Vistas | Tamaño final | Notas |
|---|---|---|---|
| Animal / bestia | Norte, este, sur (el oeste es el este reflejado) | 256×256 px por vista (bestias grandes: 512) | PNG con transparencia |
| Objeto / recurso | 1 | 128×128 px | Puede tener variantes de pila (1, pocas, muchas) |
| Edificio | 1 a 4 según rotación | 128 px por celda | Ej.: forja 2×2 → 256×256 |
| Icono (gen, rasgo, habilidad) | 1 | 64×64 px | Fondo transparente |

Nombres de archivo de una bestia: `Nombre_north.png`, `Nombre_east.png`, `Nombre_south.png`.

## Estilo (igual para todo el mod)
- Vista **desde arriba con ligera perspectiva** (como los animales vanilla de RimWorld).
- **Colores planos** con sombreado simple, **contorno oscuro grueso**, sin texturas finas ni brillos realistas.
- Paleta un poco más saturada que la vanilla para lo espiritual; brillos de elemento como manchas de color simples.
- Sin sombra proyectada en el suelo (el juego pone la suya).

## Truco para que las 3 vistas coincidan
Pedir **una sola imagen con la hoja de las 3 vistas** (turnaround) en fila, sobre **fondo de un color plano**
(magenta `#FF00FF`) para recortarlo fácil. Si una vista sale mal, se vuelve a pedir la hoja entera.

## Plantilla de prompt (inglés: los generadores responden mejor)

```
Game sprite turnaround sheet for the top-down colony sim RimWorld. Three views of the SAME creature in one row,
evenly spaced, same scale: facing down (south view), facing right (east/side view), facing up (north view, seen from behind).
Creature: <DESCRIPCIÓN>.
Art style: RimWorld vanilla animal sprites — slightly angled top-down view, flat colors, simple cel shading,
thick dark outline, clean readable silhouette, no fine texture, no realistic lighting, no cast shadow.
Solid flat magenta background (#FF00FF), no text, no labels, no grid, no border.
```

## Prompts listos

**Araña de los Nueve Infiernos (九幽蛛)**
```
Game sprite turnaround sheet for the top-down colony sim RimWorld. Three views of the SAME creature in one row,
evenly spaced, same scale: facing down (south view), facing right (east/side view), facing up (north view, seen from behind).
Creature: a giant spirit spider from Chinese xianxia mythology, the size of a horse. Black chitin body with nine glowing
violet eye-like markings arranged on its back like a face, long jointed legs tipped with pale jade-green claws,
faint purple spirit mist around the abdomen.
Art style: RimWorld vanilla animal sprites — slightly angled top-down view, flat colors, simple cel shading,
thick dark outline, clean readable silhouette, no fine texture, no realistic lighting, no cast shadow.
Solid flat magenta background (#FF00FF), no text, no labels, no grid, no border.
```

**Suanni, León Celestial del Caos (狻猊)**
```
(misma plantilla) Creature: a celestial lion from Chinese mythology (suanni), larger than a bear. Muscular golden-bronze
body, a huge curly mane made of swirling grey-white smoke and incense wisps, dragon-like scales on its legs and back,
small curved horns, fierce but regal face, tail ending in a smoky plume.
```

## Lista de arte pendiente (por prioridad)
1. Iconos de genes de raíz y elementos (hoy son círculos provisionales).
2. Piedra espiritual, jade espiritual, Píldora de Fundación (objetos).
3. Cojín de cultivo y Forja Espiritual nivel 1 (edificios).
4. Bestias élite (araña, Suanni…) y después los Cuatro Malvados.

## Parámetros ComfyUI que funcionan (SDXL base 1.0, RTX 5070 Ti)

Hallazgos de la prueba de estilo (tanda 1, objetos/iconos/cojín):
- **SDXL no sabe hacer fondo magenta liso**: lo ensucia (gris/rosa) y tiñe el objeto de rosa. Funciona mucho mejor pidiendo
  **fondo blanco liso** y quitándolo después con `process_sheet.py --single --flood 40` (rellena desde los bordes, no toca el interior).
- Sin bloque de estilo del script: usar `--style none` y escribir el estilo en el prompt.
- Parámetros: `--cfg 7 --steps 30 --size 1024x1024 --count 4` (sampler dpmpp_2m / karras, el del script). **~31 s por tanda de 4 imágenes (~7 s/imagen)**.
- Objetos de una sola pieza (gemas, jade, cristal, lingote) salen bien. Objetos con "detalle" (tablilla, talismán, píldora) tienden a
  salir como hojas de sprites o con ornamento fino: hay que repetir muchas veces o retocar a mano. Iconos de genes: salen detallados/ornamentados,
  no planos; sirven como candidatos, no como arte final.

**Prompt (objetos y edificios)**
```
<sujeto, "a single ...">, simple 2D game sprite, RimWorld style, flat colors, simple cel shading, thick dark outline,
one single large object centered filling the frame, slightly top-down view, isolated on a plain pure white background, minimal
```
**Prompt (iconos de genes)**
```
<sujeto>, bold simple flat vector game icon, only 4 flat colors, very simple shapes, thick black outline,
one centered round emblem, no ornament, isolated on a plain pure white background, minimal
```
**Negativo**
```
photo, photorealistic, realistic, 3d render, blurry, gradient, scenery, landscape, ground, pedestal, stand, shadow,
sprite sheet, collection, set, many items, pattern, grid, hand, fingers, ornate, filigree, intricate detail, text, letters,
watermark, frame, border, multiple objects, stars, particles, sparkles, aura, noisy, box, container
```
**Procesado**: `python -I tools/art/process_sheet.py <raw.png> <salida> <Nombre> --single --size 128 --flood 40`
(`--single` = una sola figura, guarda `<Nombre>.png`; objetos/cojín 128, iconos de gen 64).

### Actualización: lo que funcionó y lo que no (tanda 1 y razas)
- **img2img** (`comfy_generate.py --init sprite.png --denoise 0.75`): parte de una imagen (se recorta, se compone sobre blanco y se sube a ComfyUI).
  Con el sprite procedural de `item_gen.py` como base y denoise 0.75, píldora, lingote, tablilla y talismán salen **un solo objeto, limpios y legibles**;
  es el método recomendado para objetos. Denoise 0.6 es casi idéntico a la base; 0.8 deja a SDXL inventar más.
- Iconos de genes: añadir "one centered symbol only, no circle, no disc, no frame" y negativo "white circle, white disc" reduce los discos blancos
  pero no los elimina (el relleno por borde no toca el disco interior). Calidades de raíz (brote) salen aceptables; elementos, irregulares.
- **Razas / accesorios de render** (formato de referencia: 128×128 por vista `_north/_east/_south`, la pieza en una caja fija de la cabeza:
  cuernos S(42,29,86,62) N(42,19,86,52) E(64,30,87,59); orejas S(26,38,102,64) E(48,34,69,66)). Script: `art/generated/comfy/races.py` (no versionado).
  - Cuernos: SDXL los dibuja aceptablemente (dragón A 3-4, B 2-3); la vista de lado se genera aparte y **no coincide** con la frontal.
  - Colas, orejas, cresta de plumas: SDXL dibuja la **criatura entera** aunque se pida solo la pieza. Con img2img desde una silueta (cola/orejas) sale una
    silueta plana; sirve de base para el abanico de 1/3/6/9 colas (rotando la misma cola), pero el detalle hay que pintarlo o usar el arte vanilla.
  - Superposiciones de piel (escamas, llamas) y ojos: no se intentaron, no se pueden alinear con la máscara del cuerpo con SDXL. Recomendación: patrón repetible
    (textura) aplicado por código, o arte manual.
  - Caparazón de tortuga: los patrones hexagonales (c1, c3) sirven como textura de espalda.

### Resultados por categoría (tanda P1/P2, SDXL + flood blanco)
Hojas en `art/generated/comfy/contact_<categoria>.png` (iconos_ui, poderes, implantes, materiales, flora, edificios). Generador: `art/generated/comfy/batch.py` (no versionado).
- **Bien:** edificios y flora (vistas cenitales claras), materiales de una pieza, iconos de poderes por criatura/elemento, implantes.
- **Mal:** Nueve Colas y Presión de aura (hojas de iconos), seda espiritual, terreno (SpiritSoil/SpiritGrass salen como textura o parches, no como sprite), tesoros de interfaz parecidos entre sí.
- Temperatura GPU máx. observada durante la tanda: 68 °C.

### Segunda tanda (fauna, armas, jefes, P3)
Hojas: `art/generated/comfy/contact_<categoria>.png` (armas, fauna, jefes, cuerpos, genes_sangre, linajes_identidad, genes_firma, sectas_emblemas, sectas_tunicas, armaduras, nucleos_pildoras).
- **Fauna y jefes (hojas de 3 vistas): NO funciona con SDXL.** Casi todas fallan al separar las 3 vistas y las que pasan no son vistas coherentes de la misma criatura
  (salen varias criaturas o personajes). Para criaturas: Gemini u otro (como la araña), o img2img desde una silueta propia por vista.
- Armas: salen bien como objeto único solo algunos candidatos (espada 1, sable 4, lanza 4, espada voladora 3); muchos salen en pares o con fondo gris.
- Túnicas de secta: bien (Azure Peak 1-2, Silent Lotus 1/3, Heavenly Cauldron 1/3, Ancient Dragon 3, Rogue 1/4); la de Ascension sale como figura, no prenda.
- Iconos 64 px (cuerpos, linajes×purezas, genes de sangre y firma, emblemas de secta): ver hojas; calidad similar a los de P1.
- Temperatura GPU máxima observada en toda la sesión: 68 °C.
- **Pendiente de autorización del usuario:** integración en `Mods/` (22 elementos): copia preparada en `art/generated/comfy/integrar_staging/`, hoja `INTEGRADO.png`, tabla `integrado_tabla.md`, script `apply_integration.sh`.

### Aspecto de razas v2
Método: silueta dibujada con PIL en la posición exacta del lienzo (cabeza 128 px alineada con la cabeza vanilla; cola 256 px saliendo de la cadera) →
img2img (`comfy_generate.py --init sil.png --init-raw --denoise 0.75`, `--init-raw` no recorta ni reescala) → fondo fuera → **máscara de la silueta dilatada**
(evita que SDXL dibuje el animal entero) → grises + contorno oscuro procedural. Script: `art/generated/comfy/races2.py` (no versionado).
Hojas: `art/generated/comfy/razas_v2/contact_razas_v2_<linaje>.png` (cabeza de referencia VRE Saurid, solo para comparar).
- Alineadas y completas en las 3 vistas (copia lista en `art/generated/comfy/integrar_staging_razas_v2/`, tabla `TABLA.md`): cuernos de dragón A y B, cola de dragón, cresta de fénix,
  orejas de ciervo, crestas de caparazón, cola de serpiente, orejas de zorro, colas de zorro 1/3/6/9.
- **Calidad:** cuernos y orejas de ciervo salen limpios; las colas (sobre todo 3/6/9) salen como siluetas puntiagudas sin pelaje; sirven de base o para probar el render.
- **Sin candidato válido:** cuerno de qilin (SDXL lo borra de las vistas este/norte); hay que pintarlo a mano o subir el grosor de la silueta.
- Estas texturas van en **grises** para teñirlas con `colorType Skin/Hair`.

### Iconos de las 15 técnicas y ropa (Art Source oficial de Ludeon como plantilla)
- **Técnicas:** 15 iconos de 64 px, anillo por elemento (metal plata, madera verde, agua azul, fuego naranja, tierra ocre) sobre disco oscuro; algunos reutilizan los candidatos de `poderes`. Script `tech_build.py`/`apply_tecnicas.py`.
- **Ropa:** img2img (denoise 0.78) desde una **silueta plana** de la prenda vanilla del Art Source (Duster → túnicas y Hanfu→ShirtBasic, FlakVest → armadura de escamas, CowboyHat → sombrero de bambú), con la **máscara del contorno vanilla dilatada** para conservar la alineación
  exacta, misma semilla para los 5 cuerpos × 3 vistas, salida 128 px en grises con contorno oscuro (se tiñe con el material). Con denoise 0.55 SDXL solo redibujaba la prenda vanilla; con 0.78 sale diseño propio. No se copian texturas vanilla: son re-diseños.
  Corona de jade no hecha (la base solo existe como PSD). Las 4 túnicas comparten silueta (todas vienen de Duster): se distinguen poco entre sí.
- Pendiente de aplicar: `apply_tecnicas.py` (ya aplicado por la sesión principal), `apply_ropa.py` (cambia `texPath` y `wornGraphicPath` de las 7 prendas a `.../Apparel/CVG/<Nombre>`).

### Corona de jade, recursos textiles, planta de algodón y telar (Art Source + img2img)
Mismo método que la ropa (silueta plana del PSD vanilla, denoise 0.78, máscara del contorno, grises + contorno), 64 px los objetos y 224×96 el telar.
**Resultado flojo:** la corona de jade sale irreconocible (la vista este es una forma rara), la planta parece un trébol y el telar una losa plana: SDXL no inventa estructuras desde una silueta tan simple. Los objetos
(algodón, tela, seda, cuero) son manchas grises legibles pero muy genéricas. Recomendación: no aplicar corona, planta ni telar; los objetos solo si se acepta ese nivel. Hoja `art/generated/comfy/INTEGRADO_extras.png`, copia `integrar_staging_extras/`.

### Comparativa de modelos: SDXL base vs FLUX.1 schnell (fp8)
Hoja: `art/generated/comfy/contact_comparativa_modelos.png` (corona de jade, telar espiritual, planta de algodón, píldora; txt2img e img2img desde una silueta plana; semilla 42; script `art/generated/comfy/compare.py`).
- **Modelos:** `sd_xl_base_1.0.safetensors` (6,9 GB) y `flux1-schnell-fp8.safetensors` (Comfy-Org/flux1-schnell, Apache 2.0, 17.236.328.572 bytes, checkpoint todo-en-uno).
- **Tiempos** (RTX 5070 Ti, 1024×1024, 1 imagen): SDXL 30 pasos, 8–14 s; FLUX schnell 4 pasos, ~11 s (6 s en img2img). Temperatura máxima de la GPU: 63 °C.
- **txt2img:** FLUX gana con claridad. Dibuja el objeto pedido (corona con placas de jade y banda dorada, telar con marco de madera, planta de algodón con capullos, píldora roja y dorada) en estilo plano con contorno; SDXL tiende a mandalas/patrones o a repetir el objeto.
- **img2img (denoise 0.78):** SDXL cambia la silueta y añade detalle; FLUX con 4 pasos casi no modifica la silueta gris (solo quedan ~3 pasos efectivos): no sirve así. Para FLUX habría que subir el denoise hacia 1,0 o usar más pasos.
- **Recomendación:** objetos sueltos, plantas, edificios y recursos → **FLUX schnell en txt2img** (fondo blanco, `--model flux1-schnell-fp8.safetensors`; el script ya pone 4 pasos, cfg 1, euler/simple y sin negativo cuando el nombre contiene "flux"). Piezas que deben respetar una silueta exacta (ropa y accesorios de render sobre cuerpo/cabeza vanilla) → **SDXL img2img** con máscara de la silueta. Criaturas en 3 vistas: ninguno funciona bien.
- No se probó un LoRA de estilo ni otros modelos base (Qwen Image, Z-Image).
