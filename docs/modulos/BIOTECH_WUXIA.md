# Biotech reinterpretado en clave wuxia

Principio n.º 1: reutilizar los sistemas de Biotech (mecanitor, sanguófago) con estética y mecánicas de cultivo,
en vez de crear sistemas paralelos desde cero. Todo **opcional** (ajuste: vanilla / wuxia / ambos).

## 1. Maestro de Marionetas (傀儡师) — reinterpreta al mecanitor

| Vanilla (Biotech) | Wuxia |
|---|---|
| Mecaenlace | **Hilo del Corazón de Marioneta** (implante/técnica que da el control) |
| Formas de obtenerlo | Las mismas: escenario inicial, **cripta de un antiguo maestro titiritero** (equivalente a la nave antigua: hay que romperla y aguantar a sus guardianes), derrotar a una **marioneta guardiana** (equivalente al ataque de meca) |
| Mecas | **Marionetas** de madera (trabajo), bronce (combate) y jade (élite), con su propia estética |
| Gestador de mecas | **Taller de marionetas** (madera, metales espirituales, núcleos de bestia como "alma") |
| Ancho de banda | Sale de la **fuerza del alma (神)** y del reino: más reino → más marionetas |
| Grupos de control | Igual que vanilla |

Técnica: reutilizar el sistema de mecanitor (Pawn_MechanitorTracker, MechBandwidth, gestación) con un implante
alternativo y razas de marioneta basadas en el framework de mecas; el ancho de banda se calcula con un StatPart que
suma la fuerza del alma. Investigar con ilspycmd qué partes del mecanitor están ligadas al mecaenlace.

### Ajustes del usuario (2026-10-15)
- **Materiales propios, no componentes:** las marionetas se construyen con **planos de marioneta** (se obtienen en criptas,
  comercio o investigación), **hilo espiritual** (seda de la Araña Tejedora de Jade) y **madera espiritual** (o metales
  espirituales para las de combate). Nada de componentes industriales ni acero vanilla.
- **Funciones vanilla intactas, estética y toque propios.**
- **Crecen con el maestro:** los stats de cada marioneta escalan con el **reino de cultivo del Maestro de Marionetas**
  (vida, daño, velocidad). Si el maestro sube de reino, todas sus marionetas mejoran.
- **Vínculo:** si una marioneta se rompe, el maestro recibe una penalización (contragolpe del hilo: herida leve al alma,
  mal humor o una baja temporal de qi).
- **Recarga:** en vez de electricidad, las marionetas se recargan con **qi** del maestro o con **esencia espiritual**
  (piedras espirituales molidas / núcleos de bestia) en una estación de recarga propia.

## 2. Cultivador del Demonio de Sangre (血魔) — reinterpreta al sanguófago (jugable)

| Vanilla | Wuxia |
|---|---|
| Hemogén | **Esencia de sangre** (sube bebiendo sangre o con píldoras de sangre) |
| Sueño de muerte | **Retiro en el estanque de sangre** |
| Habilidades de sanguófago | Técnicas de sangre (camino demoníaco): drenar vida, coraza de sangre |
| Juventud / no envejecer | Igual, por el cultivo demoníaco |
| Debilidades | Luz del sol (como vanilla) y daño extra de plata lunar y técnicas del Qilin |

Conecta con el **Culto de la Luna Sangrienta**, la **Luna del Demonio Interno** y el camino del Corazón del Dao.

**No es malo por definición** (decisión del usuario): es un camino de cultivo distinto, no un villano. Puede ser un
héroe, un neutral o un tirano según el colono y su ideología; lo demoníaco es el método (la sangre), no la moral.

## 3. Jiangshi (僵尸) — enemigo (con Anomaly)
Cadáveres saltarines que absorben el yang de los vivos: duermen en ataúdes, saltan, débiles a la **madera de
melocotonero**, los **espejos** y el **arroz glutinoso** (objetos/armas propios). Evento: cripta de jiangshi o
noche de los fantasmas (Festival de los Fantasmas Hambrientos). Integración con las entidades de Anomaly.

## 4. Otros sistemas de Biotech a revisar
- **Niños cultivadores:** el crecimiento y el aprendizaje de Biotech suman raíz y comprensión; una "escuela de secta".
- **Xenogermen:** "trasplante de raíz espiritual" o "infusión de sangre de bestia" como xenogermen wuxia.
- **Contaminación (polución):** el qi turbio de la tecnología (modo tecno) como "polución espiritual".

## Implementación (2026-10-15): mod `Convergence: Biotech Wuxia`

**Decisión:** el contenido va en un mod aparte (`Mods/Convergence.BiotechWuxia`, packageId
`prospero1088.convergence.biotechwuxia`, depende de Harmony, Biotech, Core y Resources; se carga después de Traits &
Destinies, Apparel y Alchemy si están). El **código** genérico va en el Core (`Source/Convergence.Core/Biotech/`), porque
son ganchos sobre sistemas vanilla que otros módulos pueden reutilizar y el Core ya es el único DLL.

**1. Maestro de Marionetas** (sistemas de mecanitor intactos):
- `DefModExtension_MechanitorLink` en un hediff = cuenta como mecaenlace (postfix a `MechanitorUtility.ShouldBeMechanitor`;
  `Hediff_PuppetLink` recalcula los componentes dinámicos al ponerse/quitarse, como `Hediff_Mechlink`). Lo único que el
  juego ata al mecaenlace por nombre es extraerlo de un cadáver y una misión de la nave; el resto (control, grupos,
  gestación, ancho de banda, supervisión) funciona igual.
- **Hilo del Corazón de Marioneta** (`CVG_PuppetHeartThread`): implante en el corazón, +4 ancho de banda y 2 grupos;
  +1 de ancho de banda por nivel de reino (`StatPart_PuppetBandwidth`, parche a `MechBandwidth`). Se obtiene: comercio
  exótico, recompensas, y los colonos con el rasgo **Maestro de Marionetas** nacen con él (`DefModExtension_TraitGrantsHediff`).
- **Marionetas** (mecas ligeros vanilla con `CompPuppet`): madera (carga y limpieza), bronce con ballesta (combate), jade
  (élite, con núcleo de bestia). `CVG_PuppetBond` = reino del maestro (velocidad, daño, daño recibido, trabajo,
  puntería) y vida ×(1 + 0,3 × reino). **Recarga con el qi del maestro** (8 qi por 10% de energía por debajo del 60%) o en
  el **recargador espiritual** (cargador vanilla sin electricidad que quema piedras espirituales). Si una se rompe, el
  maestro recibe **contragolpe del hilo** y pierde 30% de qi.
- **Taller de marionetas**: gestador vanilla con red propia de consumo 0 (sin cables). Recetas: plano de marioneta + hilo
  espiritual + madera espiritual / acero espiritual / jade espiritual (+ núcleo). Proyecto del Dao **Arte de las
  marionetas**. Los planos se dibujan en el lugar de fabricación (Intelectual 6) o se compran; el hilo espiritual se hila
  de tela + piedra espiritual (con Apparel, también de seda espiritual: `Mods/Apparel`).
- Los cargadores y el taller conservan los paquetes de desechos del vanilla (el código del juego los necesita).
- Pendiente: cripta del maestro titiritero y marioneta guardiana (misiones), arte propio.

**2. Demonio de Sangre** (sistemas de sanguófago intactos): xenotipo `CVG_Xeno_BloodDemon` con genes vanilla
(hemogénico, alimentarse de sangre, coagular, sin edad, sueño de muerte, fuerza, curación rápida, visión nocturna,
sensibilidad UV leve) + gen **Artes del demonio de sangre** (cultivo y templado ×1,2; técnicas **Drenar sangre** —daño,
cura al lanzador y da hemogén— y **Coraza de sangre** —escudo con coste de hemogén—; **plata lunar ×1,5**, genérico
`DefModExtension_StuffWeakness`). **Estanque de sangre**: ataúd de sueño de muerte sin electricidad. No es malvado: ni
ideología ni facción forzadas.

**3. Jiangshi** (solo con Anomaly: carpeta `Mods/Anomaly` de este mod; código genérico en `Source/Convergence.Core/Biotech/Jiangshi.cs`):
- `MutantDef CVG_Jiangshi` sobre la base de mutantes de Anomaly, con el árbol de pensamiento del shambler: facción de
  entidades, se puede contener en la plataforma y da conocimiento. Garras con `extraMeleeDamages` de
  `CVG_YangDrain` (`DamageWorker_YangDrain`): dejan **qi cadavérico** en la víctima (sueño, entumecimiento; baja solo
  con el tiempo) y curan al jiangshi. El **sol** los quema al aire libre (`HediffComp_SunBurn`).
- **Madera de melocotonero** (stuff leñoso: tablas curadas con ceniza de talismán y una piedra espiritual) y **espada de
  melocotonero**: ×3 de daño a los jiangshi y ×1,5 a los shamblers (`DefModExtension_StuffWeakness`, ahora también en
  hediffs y con lista de armas).
- **Espejo bagua** (plata + melocotonero): deslumbra (movimiento ×0,5, precisión −6) y quema un poco a los jiangshi y
  shamblers a la vista en 8 casillas.
- **Cataplasma de arroz glutinoso** (arroz vanilla en el lugar de fabricación o la hoguera): operación vanilla
  *Sacar el qi cadavérico* (`Recipe_RemoveHediff`).
- Evento **Noche de los fantasmas hambrientos** (amenaza grande, solo de 20 a 4 h, tras la Convergencia): 2-14 jiangshi
  según los puntos (+ riqueza de cultivo) asaltan la colonia (`LordJob_AssaultColony`).
- Pendiente: que un muerto con mucho qi cadavérico se levante como jiangshi; cripta de jiangshi; arte propio.

**Cómo probar:** `-cvgselftest` (sección "biotech wuxia": hilo → mecanitor, ancho de banda, vínculo con el reino, recarga
con qi, contragolpe, técnicas de sangre y plata lunar). A mano: modo desarrollador, spawnear `CVG_PuppetHeartThread` y
usarlo; construir el taller (pestaña Cultivo) tras el proyecto, una marioneta; generar un colono con xenotipo
*demonio de sangre*.


### Atavío del maestro titiritero (sin electricidad ni componentes)
Investigación `CVG_Research_PuppetMasters` (tras *Arte de las marionetas*, conocimiento 8). Equivalentes wuxia del equipo de mecanitor; todo con madera/hilo/seda espiritual y jade:

| Pieza | Ancho de banda | Notas |
|---|---|---|
| Máscara del titiritero (`CVG_Apparel_PuppeteerMask`) | +6 | casco ligero (armadura 0,2), `UpperHead` |
| Corona de mil hilos (`CVG_Apparel_ThousandThreadsCrown`) | +12 | sin armadura, jade + hilo espiritual |
| Túnica del gran titiritero (`CVG_Apparel_GrandPuppeteerRobe`, solo con Apparel) | +12 | armadura media, seda + hilo espiritual, capa Shell |
| Tótem de hilos (`CVG_ThreadTotem`) | +1 al maestro sintonizado | edificio 2×2; funciona con piedras espirituales (`CompRefuelable`), sin red eléctrica |

- Casco y corona se fabrican en el punto de artesanía o el sastre manual; la túnica en el telar espiritual. **Desviación:** el taller de marionetas es un gestador (solo admite recetas de mecas), así que no hospeda facturas de objetos.
- **Código del Core** (`Biotech/BandTotem.cs`): `DefModExtension_BandNodeSubstitute` marca edificios con `CompBandNode` propio. Parches: `Hediff_BandNode.RecacheBandNodes` (el nodo vanilla solo cuenta `ThingDefOf.BandNode`) suma los tótems sintonizados con combustible y encendidos; `CompPowerTrader.PowerOn` (getter) de esos edificios = tiene combustible, porque no están en ninguna red.
- Texturas provisionales (vanilla teñidas); rutas finales en comentarios del XML: `Apparel/CVG/PuppeteerMask`, `ThousandThreadsCrown`, `GrandPuppeteerRobe`, `Building/Misc/CVG_ThreadTotem`.
- Autotest: `-cvgbandtotemtest` (con `-quicktest`): ALL OK (+6/+12/+12, tótem +1, se apaga sin piedras).

### Plantel de marionetas (6, un papel cada una)
| Marioneta | Papel | Notas |
|---|---|---|
| Madera (`CVG_Puppet_Wood`) | carga y limpieza | madera + hilo espiritual |
| Picapedrera (`CVG_Puppet_Miner`) | **minería** | `mechEnabledWorkTypes` Mining (como el tunelador, pero ligera: `MiningSpeed` 0,9 base, crece con el reino del maestro); acero + hilo |
| Albañil (`CVG_Puppet_Mason`) | construcción + siembra y siega | Construction/PlantCutting/Growing; madera + acero + hilo |
| Doble hoja (`CVG_Puppet_Blade`) | combate cuerpo a cuerpo | hojas de corte/punzada, sin arma; acero + hilo |
| Bronce con ballesta (`CVG_Puppet_Bronze`) | combate a distancia | |
| Guardiana de jade (`CVG_Puppet_Jade`) | élite | núcleo de bestia |
Todas se atan en el taller de marionetas (gestación vanilla), llevan `CompPuppet` y escalan con el reino del maestro (`CVG_PuppetBond`: velocidad, daño, trabajo). Autotest `-cvgbandtotemtest` comprueba papeles, trabajo de minería real (`WorkGiver_Miner`) y recetas.
