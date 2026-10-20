# Compatibilidad con Dubs Bad Hygiene (y Dubs Performance Analyzer)

El usuario juega mucho con los mods de Dubs. Referencias copiadas en `referencias/frameworks/` (836308268_DubsBadHygiene,
2038874626_DubsPerformanceAnalyzer), solo para estudiar. packageId de DBH: `Dubwise.DubsBadHygiene`.
Todo lo de DBH va en `Mods/<Mod>/Mods/DubsBadHygiene` (LoadFolders `IfModActive="Dubwise.DubsBadHygiene"`): sin DBH no carga nada.

## Lo que ofrece DBH y cómo encaja
- Necesidades: `Bladder`, `Hygiene`, `DBHThirst`; stats `BladderRateMultiplier`, `HygieneRateMultiplier`, `ThirstRateMultiplier`.
- Salas: baño público/privado, sauna, aseo público. Edificios con redes de agua/cloacas (`CompProperties_WaterInlet`,
  `PlaceWorker_WaterGrid`), jacuzzi (`joyKind Hydrotherapy`), sauna a leña (`CompProperties_SaunaHeater`), fuentes, pozos.

## Plan (criterio: tres piezas con identidad wuxia)
1. **Bigu (辟谷)** ✅: desde Fundación el cuerpo se alimenta de qi. Vanilla: `hungerRateFactor` en las etapas de Fundación
   (0,9→0,75) y Núcleo Dorado (0,6→0,4). Con DBH (`Mods/Convergence.Core/Mods/DubsBadHygiene/Patches/Bigu.xml`): los mismos
   factores para sed, vejiga e higiene. El camino del cuerpo NO tiene bigu (el templado pide carne y sangre: contraste buscado).
2. **Expulsión de impurezas (排出杂质)** (pendiente): tras un avance de reino o el Lavado de Médula, el cuerpo suda impurezas negras:
   suciedad propia alrededor del colono (vanilla) y, con DBH, la higiene cae a 0 y aparece un pensamiento ("necesito bañarme ya").
3. **Estanque termal de qi** (pendiente, Furniture, solo con DBH): jacuzzi conectado a la red de agua de DBH que, sobre una veta
   de qi, da regeneración de qi y velocidad de cultivo mientras se usa (`Hydrotherapy`), y suma elemento Agua a la sala (feng shui).

## Dubs Performance Analyzer
Herramienta para medir el coste por tick de nuestros comps y parches (CompCultivator, MapComponents, Harmony). Usarla antes de
publicar para revisar rendimiento con colonias grandes.
