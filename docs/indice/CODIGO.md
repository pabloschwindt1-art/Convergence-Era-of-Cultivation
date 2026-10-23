# Clases C# de Convergence

Generado por `tools/make-index.py`. Primera frase del `<summary>` de cada clase.

| Clase | Mod | Archivo | Para qué |
|---|---|---|---|
| `AbilityPower` | Convergence.Core | `Abilities/AbilityComps.cs` | Potencia R de las capacidades según el reino del camino del qi (docs/modulos/razas/README.md §2). |
| `Alert_CVG_IdleCultivator` | Convergence.Core | `Core/CultivationLife.cs` | Un cultivador tiene desactivado el trabajo de cultivar (el jugador puede no haberlo notado). |
| `Alert_CVG_NoCultivationSpot` | Convergence.Core | `Core/CultivationLife.cs` | Tras la Convergencia hay cultivadores y ningún lugar de cultivo en el mapa. |
| `Alert_CVG_QiDeviation` | Convergence.Core | `Core/CultivationLife.cs` | Desviación de qi (o demonio interno) en curso. |
| `Alert_CVG_ReadyToBreakthrough` | Convergence.Core | `Core/CultivationLife.cs` | Un cultivador está en el cuello de botella y puede intentar el avance ahora. |
| `AnimationShot` | Convergence.Core | `Debug/AnimationShot.cs` | Herramienta de revisión visual: con -cvganimshot (y tools/run-test.ps1 -Visible) coloca cada criatura del mod cuya raza tiene startingAnimation (marionetas, etc.), comprueba que la animación esté corriendo y, con el jueg |
| `ArtShots` | Convergence.Core | `Debug/ArtShots.cs` | Capturas de revisión del arte (instancia de prueba, ventana visible) |
| `AuraPressure` | Convergence.Core | `Events/AuraPressure.cs` | Presión de aura (DEFENSA §4): cuando un asalto trae un cultivador de un reino superior al de todos los colonos, el jugador recibe una carta especial que lo avisa y dice cuánto más alto es |
| `BackstoryTiers` | Convergence.Core | `Core/BackstoryTiers.cs` | Tiers de historias de fondo (1 común … 4 legendaria) y su color en la ficha del colono, para reconocer de un vistazo las buenas al aleatorizar |
| `BloodlineDef` | Convergence.Core | `Defs/BloodlineDefs.cs` | Linaje de sangre (Dragón, Fénix…): afinidades y lista ordenada de purezas |
| `BloodlineGeneGroupDef` | Convergence.Core | `Defs/BloodlineGeneDefs.cs` | Grupo de genes de sangre por niveles (Vitalidad bestial, Escamas de dragón…) |
| `BloodlineInfo` | Convergence.Core | `Bloodline/BloodlineUtility.cs` | Linaje y pureza de un colono (derivado de sus genes o rasgos; no se guarda aparte). |
| `BloodlineMechanics` | Convergence.Core | `Bloodline/Gene_Bloodline.cs` | Utilidades y parches de las mecánicas de los genes de sangre. |
| `BloodlinePurity` | Convergence.Core | `Defs/BloodlineDefs.cs` | Un nivel de pureza dentro de un BloodlineDef. |
| `BloodlinePurityDef` | Convergence.Core | `Defs/BloodlineDefs.cs` | Nivel de pureza de sangre (Rastro, Diluida, Pura, Ancestral…) |
| `BloodlineSelfTest` | Convergence.Core | `Bloodline/BloodlineSelfTest.cs` | Autotest de linajes: para cada linaje genera un colono, le da la pureza más baja y lo purifica hasta la máxima, comprobando gen de identidad, genes de aspecto y xenotipo en cada paso; luego prueba un colono generado con  |
| `BloodlineText` | Convergence.Core | `Bloodline/BloodlineText.cs` | Textos de linaje generados desde los datos (hediff de efectos y BloodlineDef), para que la descripción de los genes y la pestaña Cultivo no se desincronicen del XML. |
| `BloodlineUtility` | Convergence.Core | `Bloodline/BloodlineUtility.cs` | Lógica genérica de linajes: detectar, dar, purificar, generar al azar y aplicar sus efectos (hediff y capacidades) solo después de la Convergencia |
| `BreakthroughRequirement_Realm` | Convergence.Core | `Defs/CultivationPathDefs.cs` | Requiere haber llegado a un reino (de cualquier camino; útil para doble cultivo). |
| `BreakthroughRequirement_Research` | Convergence.Core | `Research/DaoComprehension.cs` | Requisito de avance: haber completado un proyecto de investigación (p |
| `BreakthroughRequirement_Stat` | Convergence.Core | `Defs/CultivationPathDefs.cs` | Requiere un valor mínimo de un stat (los atributos de fundamento serán stats). |
| `BreakthroughRequirement` | Convergence.Core | `Defs/CultivationPathDefs.cs` | Requisito para intentar el avance a un reino (lista breakthroughRequirements del reino, en XML con Class=) |
| `BreakthroughUtility` | Convergence.Core | `Cultivation/BreakthroughUtility.cs` | Avance de reino de un camino: requisitos, probabilidad, gizmo "Intentar avance" y resultado (éxito, fallo o tribulación) |
| `Building_FrostFormation` | Convergence.Core | `QiNetwork/QiSources.cs` | Formación de escarcha: enfría la habitación donde está, sin salida de calor (es una formación, no una máquina) |
| `Building_LeveledWorkTable` | Convergence.Core | `Crafting/WorkbenchLevels.cs` | Mesa de trabajo cuyo arte cambia con el nivel (WorkbenchLevel.texPath). |
| `Building_RealmGate` | Convergence.Stories | `SecretRealms.cs` | Crea la puerta en la celda (despeja lo que haya) con el reino dado o uno al azar. |
| `CelestialCalendar` | Convergence.Core | `Events/CelestialCalendar.cs` |  |
| `ChoiceLetter_Story` | Convergence.Stories | `StoryEvents.cs` | Carta con decisión: las opciones se resuelven en StoryEvents.Decide. |
| `Command_Technique` | Convergence.Core | `Techniques/TechniqueAbilityComps.cs` | Gizmo de técnica: muestra el coste arriba a la derecha (como VPE). |
| `CompAbilityEffect_AIConditions` | Convergence.Core | `Techniques/CultivatorAI.cs` | No la usa si ya tiene este hediff (escudo activo). |
| `CompAbilityEffect_BloodDrain` | Convergence.Core | `Biotech/BloodDemon.cs` | Hemogén ganado (0-1) por punto de daño. |
| `CompAbilityEffect_ChainLightning` | Convergence.Core | `Techniques/ChainLightning.cs` | Daño de cada salto respecto del anterior. |
| `CompAbilityEffect_Cone` | Convergence.Core | `Abilities/AbilityComps.cs` |  |
| `CompAbilityEffect_GiveHediffInRadius` | Convergence.Core | `Abilities/AbilityComps.cs` |  |
| `CompAbilityEffect_ResourceCost` | Convergence.Core | `Techniques/TechniqueAbilityComps.cs` | Camino cuyo recurso se gasta |
| `CompAbilityEffect_TechniqueHediff` | Convergence.Core | `Techniques/TechniqueAbilityComps.cs` | La IA solo la usa con un enemigo cerca o en combate reciente (buffs y escudos). |
| `CompAbilityEffect_TechniqueProjectile` | Convergence.Core | `Techniques/TechniqueAbilityComps.cs` | Fleck en el lanzador al disparar (opcional). |
| `CompBaguaMirror` | Convergence.Core | `Biotech/Jiangshi.cs` |  |
| `CompCelestialDome` | Convergence.Core | `Space/IslandDome.cs` | Temperatura (°C) que mantiene dentro de su radio. |
| `CompCrafter` | Convergence.Core | `Crafting/Crafter.cs` | "Hecho por" en las píldoras, armas y talismanes de Convergence |
| `CompCultivator` | Convergence.Core | `Cultivation/CompCultivator.cs` | Componente de cultivo de cada humanoide |
| `CompDaoStudy` | Convergence.Core | `Events/InnerDemon.cs` |  |
| `CompFormationCore` | Convergence.Formations | `Formations.cs` | Si es true, el núcleo gasta combustible mientras está activo (si no, solo al actuar). |
| `CompPlantSpread` | Convergence.Core | `Cultivation/CompPlantSpread.cs` |  |
| `CompProperties_AbilityAIConditions` | Convergence.Core | `Techniques/CultivatorAI.cs` | Cuándo la IA puede usar la técnica (no afecta al jugador) |
| `CompProperties_AbilityBloodDrain` | Convergence.Core | `Biotech/BloodDemon.cs` | Técnica de sangre "drenar": daña al objetivo y la sangre robada cura al lanzador y le da hemogén (esencia de sangre) |
| `CompProperties_AbilityChainLightning` | Convergence.Core | `Techniques/ChainLightning.cs` | Rayo en cadena (camino del Trueno; idea del Chain Bolt de VPE): golpea al objetivo y salta a los enemigos más cercanos (hasta jumps, a menos de jumpRadius del último) |
| `CompProperties_AbilityCone` | Convergence.Core | `Abilities/AbilityComps.cs` | Cono de daño desde el lanzador (aliento): daño de cualquier DamageDef, aturdimiento con probabilidad y, opcionalmente, apaga el fuego |
| `CompProperties_AbilityGiveHediffInRadius` | Convergence.Core | `Abilities/AbilityComps.cs` | Aura: da un hediff a los pawns en un radio alrededor del objetivo (o del lanzador si se lanza sobre sí mismo) |
| `CompProperties_AbilityQiCost` | Convergence.Core | `Abilities/AbilityComps.cs` | Coste de qi de una capacidad innata (módulo de razas) |
| `CompProperties_AbilityResourceCost` | Convergence.Core | `Techniques/TechniqueAbilityComps.cs` | Coste en el recurso de un camino (qi, sangre-qi…) de una técnica o capacidad, × stat CVG_TechniqueCost × maestría |
| `CompProperties_AbilityTechniqueHediff` | Convergence.Core | `Techniques/TechniqueAbilityComps.cs` | Da un hediff al objetivo o al lanzador (escudo de qi, buffs, curas, debuffs) |
| `CompProperties_AbilityTechniqueProjectile` | Convergence.Core | `Techniques/TechniqueAbilityComps.cs` | Lanza un proyectil al objetivo |
| `CompProperties_BaguaMirror` | Convergence.Core | `Biotech/Jiangshi.cs` | Espejo bagua: los pawns con alguno de affectedHediffs que estén cerca y a la vista quedan deslumbrados (hediff) y reciben un poco de daño. |
| `CompProperties_CelestialDome` | Convergence.Core | `Space/IslandDome.cs` |  |
| `CompProperties_Crafter` | Convergence.Core | `Crafting/Crafter.cs` | Quién hizo el objeto: alquimista (píldoras), forjador (armas), maestro de talismanes o artesano. |
| `CompProperties_Cultivator` | Convergence.Core | `Cultivation/CompCultivator.cs` |  |
| `CompProperties_DaoStudy` | Convergence.Core | `Events/InnerDemon.cs` | Edificio de estudio del alma (tablero de go, mesa de caligrafía…): quien lo usa como recreación gana un poco de comprensión del Dao |
| `CompProperties_FormationCore` | Convergence.Formations | `Formations.cs` | Formación (阵法): un núcleo con combustible (piedras espirituales) y banderas colocadas alrededor |
| `CompProperties_PlantSpread` | Convergence.Core | `Cultivation/CompPlantSpread.cs` | Planta que se propaga sola por los alrededores cuando está madura (bambú) |
| `CompProperties_Puppet` | Convergence.Core | `Biotech/Puppetry.cs` | Marioneta: escala con el reino del maestro, recarga con su qi y contragolpe al romperse. |
| `CompProperties_QiConverter` | Convergence.QiTech | `QiTech.cs` |  |
| `CompProperties_QiLightningRod` | Convergence.Core | `QiNetwork/QiSources.cs` | ×1,5 con la Marea de Qi, ×0,5 con el Reflujo de Qi, ×1 si no. |
| `CompProperties_QiNode` | Convergence.Core | `QiNetwork/QiNetwork.cs` |  |
| `CompProperties_SpiritGrade` | Convergence.Core | `Techniques/SpiritGrade.cs` | Grado espiritual de un arma (docs/modulos/DEFENSA.md §1): 0 mortal (vanilla y otros mods), 1 runas, 2 arma espiritual, 3+ tesoro |
| `CompProperties_StabilizationChamber` | Convergence.QiTech | `QiTech.cs` |  |
| `CompProperties_UseEffectLearnManual` | Convergence.Core | `Techniques/CompUseEffect_LearnTechniqueManual.cs` | Tablilla de jade (玉简) que enseña un manual de técnicas al leerla |
| `CompProperties_UseEffectSpiritBurst` | Convergence.Core | `Crafting/PillsAndRunes.cs` | Talismán de ataque: al usarlo, daña a los hostiles en un radio alrededor del usuario. |
| `CompProperties_WorkbenchLevel` | Convergence.Core | `Crafting/WorkbenchLevels.cs` | Mesa con niveles (Forja Espiritual, Horno de alquimia…): se mejora en el sitio con un botón que consume los materiales del mapa |
| `CompPuppet` | Convergence.Core | `Biotech/Puppetry.cs` | Debilidad del material (madera: fuego; hierro: frío...) |
| `CompQiConsumer` | Convergence.Core | `QiNetwork/QiNetwork.cs` | Marca de rol para consumidores (el comportamiento es el de CompQiNode). |
| `CompQiConverterNode` | Convergence.QiTech | `QiTech.cs` | Lado "red de qi" del convertidor |
| `CompQiConverterPower` | Convergence.QiTech | `QiTech.cs` | Lado "red eléctrica" del convertidor |
| `CompQiLightningRod` | Convergence.Core | `QiNetwork/QiSources.cs` | Pararrayos del trueno celestial |
| `CompQiNode` | Convergence.Core | `QiNetwork/QiNetwork.cs` |  |
| `CompSpiritGrade` | Convergence.Core | `Techniques/SpiritGrade.cs` |  |
| `CompStabilizationChamber` | Convergence.QiTech | `QiTech.cs` | Cama con corriente: quien descansa dentro y lleva algo que suma disonancia celestial (implantes, injertos) recibe el hediff que la baja. |
| `CompUseEffect_LearnTechniqueManual` | Convergence.Core | `Techniques/CompUseEffect_LearnTechniqueManual.cs` |  |
| `CompUseEffect_SpiritBurst` | Convergence.Core | `Crafting/PillsAndRunes.cs` |  |
| `CompWorkbenchLevel` | Convergence.Core | `Crafting/WorkbenchLevels.cs` |  |
| `ConstitutionUtility` | Convergence.Core | `Sects/Constitutions.cs` | Constituciones de secta (Cuerpo de Espada, cuerpo Vajra…): rasgos con grados de vanilla |
| `ConvergenceFactionUtility` | Convergence.Core | `Cultivation/CultivatorPawnKind.cs` | Facciones marcadas con DefModExtension_ConvergenceFaction (sectas): en partidas nuevas vienen con el mundo y aquí solo se aplican sus relaciones; en partidas viejas se crean al llegar la Convergencia. |
| `ConvergenceMineables` | Convergence.Core | `Resources/ConvergenceMineables.cs` | Biomas donde aparece |
| `ConvergenceMod` | Convergence.Core | `Core/ConvergenceMod.cs` |  |
| `ConvergenceSettings` | Convergence.Core | `Core/ConvergenceSettings.cs` |  |
| `ConvergenceStartup` | Convergence.Core | `Core/ConvergenceStartup.cs` | Se ejecuta una vez con todos los Defs cargados |
| `ConvergenceTrigger` | Convergence.Core | `Core/ConvergenceSettings.cs` | Cómo se dispara la Convergencia. |
| `ConvergenceUtility` | Convergence.Core | `Events/IncidentWorker_Convergence.cs` | Lo que la Convergencia le hace a un mapa: linajes, vetas de piedra y jade espiritual, bestias, vetas de qi y el Maestro |
| `ConverterMode` | Convergence.QiTech | `QiTech.cs` |  |
| `CopyRecipesStartup` | Convergence.Core | `Defs/CopyRecipes.cs` |  |
| `CoreSelfTest` | Convergence.Core | `Debug/CoreSelfTest.cs` | Autotest de integración del Core (M2–M6): Convergencia, raíz y cultivo, avance y tribulación, técnicas y supresión de reino, camino del cuerpo con bestias y sopa, el Maestro y un humo de todos los ThingDef de los mods Co |
| `CrafterInjection` | Convergence.Core | `Crafting/Crafter.cs` | Las píldoras, armas y talismanes de los mods de Convergence llevan CompCrafter (sin tocar cada XML): píldoras = alquimista, armas = forjador, talismanes = maestro de talismanes. |
| `CultivationDevelopmentUtility` | Convergence.Core | `UI/CultivationDevelopmentUtility.cs` | Cuándo y cómo abrir la ventana de desarrollo (técnicas y conjuntos de combate). |
| `CultivationDifficulty` | Convergence.Core | `Core/CultivationDifficulty.cs` |  |
| `CultivationDifficultyDef` | Convergence.Core | `Core/CultivationDifficulty.cs` | Preajuste de dificultad de cultivo (docs/DISENO.md §8, ronda 3): Fácil / Difícil / Pesadilla en XML (Defs/CultivationDifficulty.xml); se pueden agregar más. |
| `CultivationDifficultyValues` | Convergence.Core | `Core/CultivationDifficulty.cs` | Valores de la dificultad de cultivo |
| `CultivationGenerator` | Convergence.Core | `Cultivation/CultivationGenerator.cs` | Reparte raíces espirituales y cuerpos especiales al generar humanoides |
| `CultivationGuide` | Convergence.Core | `UI/CultivationGuide.cs` | Guía dentro del juego: carta "Guía del cultivador" unas horas después de la Convergencia (dónde está cada cosa) y conceptos del Ayudante de aprendizaje vanilla que se enseñan cuando aparece la ocasión. |
| `CultivationLife` | Convergence.Core | `Core/CultivationLife.cs` | El cultivo en los sistemas vanilla de la vida diaria (docs/revision/REVISION_Core.md, prioridad alta): pensamientos (ThoughtDef), registros de la ficha (RecordDef) y relatos para el arte (TaleDef) cuando un colono avanza |
| `CultivationPathDef` | Convergence.Core | `Defs/CultivationPathDefs.cs` | Camino de cultivo (los Tres Tesoros 精气神): Qi, Cuerpo (M4b), Shen (más adelante) |
| `CultivationPathWorker_Body` | Convergence.Core | `Defs/CultivationPathDefs.cs` | Camino del cuerpo (M4b): lo siguen los humanos adultos SIN raíz espiritual tras la Convergencia (incluidos los cuerpos especiales sin raíz: Acero, Tribulación, Espada, Espiritual Innato) |
| `CultivationPathWorker` | Convergence.Core | `Defs/CultivationPathDefs.cs` | Quién puede seguir un camino |
| `CultivationRealmDef` | Convergence.Core | `Defs/CultivationDefs.cs` | Reino de cultivo (Refinamiento de Qi, Fundación…) |
| `CultivationResource` | Convergence.Core | `Defs/CultivationPathDefs.cs` | Recurso de un camino (qi, sangre-qi…) |
| `CultivationText` | Convergence.Core | `Cultivation/CultivationText.cs` | Textos cortos de cultivo reutilizados en cartas y en la pestaña. |
| `CultivationWealth` | Convergence.Core | `Economy/CultivationWealth.cs` | Lo que se quitó de la riqueza vanilla. |
| `CultivationWealthDef` | Convergence.Core | `Economy/CultivationWealth.cs` | Economía paralela (docs/DISENO.md §8, ronda 3): los objetos del mod (píldoras, esencias, tesoros, piedras y jade espiritual, metales, armas espirituales, marionetas, muebles de cultivo…) cuentan solo una fracción en la r |
| `CultivationWealthRecord` | Convergence.Core | `Economy/CultivationWealth.cs` |  |
| `CultivatorAI` | Convergence.Core | `Techniques/CultivatorAI.cs` |  |
| `CultivatorPawnKindUtility` | Convergence.Core | `Cultivation/CultivatorPawnKind.cs` | Si la ideología principal del jugador tiene alguno de estos memes, la facción llega neutral con él (p |
| `CVG_DefOf` | Convergence.Core | `Defs/CultivationDefs.cs` | Concentración → multiplicador de la probabilidad de avance de reino. |
| `DamageWorker_YangDrain` | Convergence.Core | `Biotech/Jiangshi.cs` | Daño que roba el yang (extraMeleeDamages de las garras del jiangshi): suma severidad del hediff DamageDef.hediff (qi cadavérico) a la víctima y cura al atacante |
| `DaoComprehension` | Convergence.Core | `Research/DaoComprehension.cs` |  |
| `DaoContribution` | Convergence.Core | `Research/DaoComprehension.cs` |  |
| `DaoSource` | Convergence.Core | `Research/DaoComprehension.cs` | Fuente de comprensión (para la carta: "se iluminó mientras cultivaba"). |
| `DebugActions_Convergence` | Convergence.Core | `Debug/DebugActions_Convergence.cs` | Acciones del modo desarrollador para probar rápido (menú de debug → "Convergence"). |
| `DebugActions_Relations` | Convergence.Core | `Debug/DebugActions_Relations.cs` | Acciones de debug de relaciones de cultivo (Pareja Dao, hermanos jurados, rival…). |
| `DefModExtension_BackstoryStats` | Convergence.Core | `Core/BackstoryTiers.cs` | Modificadores de stats en una historia de fondo (vanilla no los tiene): +10 % de velocidad de minado, etc |
| `DefModExtension_BandNodeSubstitute` | Convergence.Core | `Biotech/BandTotem.cs` | En un edificio con CompBandNode propio (tótem de hilos): cuenta como nodo de banda para el mecanitor al que está sintonizado mientras tenga combustible (el nodo vanilla solo cuenta ThingDefOf.BandNode y la red eléctrica) |
| `DefModExtension_BlocksCultivation` | Convergence.Core | `Defs/CultivationDefs.cs` | En un HediffDef: mientras el pawn lo tenga no puede seguir los caminos indicados (todos si paths está vacío). |
| `DefModExtension_Bloodline` | Convergence.Core | `Defs/BloodlineDefs.cs` | Marca un gen como identidad de un linaje y una pureza. |
| `DefModExtension_BloodlineGeneTier` | Convergence.Core | `Defs/BloodlineGeneDefs.cs` | Marca un gen como el nivel tier (1 = Rastro … 4 = Ancestral) de un grupo |
| `DefModExtension_BloodRegeneration` | Convergence.Core | `Defs/BloodlineGeneDefs.cs` | Regenera partes perdidas y cicatrices cada cierto tiempo (tras la Convergencia) |
| `DefModExtension_BodyTrainingSpot` | Convergence.Core | `Body/BodyTempering.cs` | Marca un edificio como aparato de templado corporal (muñeco de madera, piedras de pesas…). |
| `DefModExtension_BreakthroughAid` | Convergence.Core | `Defs/CultivationDefs.cs` | Marca un hediff como ayuda al avance (p |
| `DefModExtension_CelestialCondition` | Convergence.Core | `Events/CelestialCalendar.cs` | Efectos de una condición del calendario celestial (docs/EVENTOS.md §1: Marea de Qi, Reflujo del Qi, Luna del Demonio Interno…), en el GameConditionDef |
| `DefModExtension_ConvergenceFaction` | Convergence.Core | `Cultivation/CultivatorPawnKind.cs` | En un FactionDef: la facción no existe hasta la Convergencia; entonces llega al mundo. |
| `DefModExtension_ConvergenceMineable` | Convergence.Core | `Resources/ConvergenceMineables.cs` | Veta que solo existe tras la Convergencia (piedra espiritual, jade espiritual, cristal…) |
| `DefModExtension_CopyRecipes` | Convergence.Core | `Defs/CopyRecipes.cs` | Versión wuxia de una mesa de trabajo vanilla (principio n.º 2): la mesa hace todas las recetas de las mesas de from, incluidas las que otros mods agreguen a esas mesas (se copian al arrancar, después de cargar todo) |
| `DefModExtension_CultivationSpot` | Convergence.Core | `Defs/CultivationDefs.cs` | Marca un edificio como lugar de cultivo (cojín, plataforma de jade…). |
| `DefModExtension_CultivationThreat` | Convergence.Core | `Economy/CultivationWealth.cs` | En un FactionDef: sus asaltos suman los puntos de la riqueza de cultivo (las sectas ya lo hacen por ser de la Convergencia). |
| `DefModExtension_CultivationWealth` | Convergence.Core | `Economy/CultivationWealth.cs` | En un ThingDef: fuerza si cuenta como objeto de cultivo (aunque no sea de un mod de Convergence, o para excluir uno que sí lo es) y, opcionalmente, su fracción en la riqueza vanilla. |
| `DefModExtension_CultivatorPawnKind` | Convergence.Core | `Cultivation/CultivatorPawnKind.cs` | En un PawnKindDef: los humanoides de este tipo salen ya cultivando |
| `DefModExtension_DaoResearch` | Convergence.Core | `Research/DaoComprehension.cs` | Marca un proyecto de la pestaña Cultivo como "Comprensión del Dao" (docs/modulos/INVESTIGACION.md) |
| `DefModExtension_DemonicArt` | Convergence.Core | `Ideology/SectEvents.cs` | En un AbilityDef o TechniqueDef: usarla cuenta como "arte demoníaca" para los preceptos de secta. |
| `DefModExtension_DualCultivation` | Convergence.Core | `Traits/TraitMechanics.cs` | Cultivo dual (Pareja Dao, rival de por vida…) en un PawnRelationDef: si el otro está cerca y también cultiva, la velocidad de cultivo se multiplica |
| `DefModExtension_ElementalResonance` | Convergence.Core | `Cultivation/ElementalResonance.cs` | En una prenda o accesorio (aros de los cinco elementos, docs/modulos/CONTENIDO_TOI_ACS.md): si la raíz espiritual de quien lo lleva tiene element, el stat al que se añade StatPart_ElementalResonance se multiplica por fac |
| `DefModExtension_ExtraLives` | Convergence.Core | `Defs/BloodlineGeneDefs.cs` | Vidas extra (Nirvana, nueve colas): al morir tras la Convergencia, el colono se salva, se cura y gasta una vida |
| `DefModExtension_FengShui` | Convergence.Core | `Defs/CultivationDefs.cs` | Feng shui de un edificio: cuánto qi aporta a la concentración del lugar y de qué elemento |
| `DefModExtension_Forge` | Convergence.Core | `Crafting/ForgeUtility.cs` | Marca una mesa de trabajo como forja (herrería, banco de fabricación, la futura Forja Espiritual…) |
| `DefModExtension_GeneAura` | Convergence.Core | `Defs/BloodlineGeneDefs.cs` | Aura: da un hediff a pawns cercanos (aliados, hostiles o solo entidades) mientras el portador está despierto. |
| `DefModExtension_GravAnchorAlias` | Convergence.Core | `Space/IslandPatches.cs` | En el ThingDef de un ancla alternativa: para el juego cuenta como ThingDefOf.GravAnchor (evita que el mapa se destruya al zarpar). |
| `DefModExtension_GravEngineAlias` | Convergence.Core | `Space/IslandPatches.cs` | En el ThingDef de un motor grav alternativo: para el juego cuenta como ThingDefOf.GravEngine. |
| `DefModExtension_InnerDemon` | Convergence.Core | `Events/InnerDemon.cs` | Hediffs que deja el demonio interno al acabar (en el MentalStateDef). |
| `DefModExtension_JiangshiIncident` | Convergence.Core | `Biotech/Jiangshi.cs` | Jiangshi del evento: tipo de pawn, puntos y si solo cae de noche. |
| `DefModExtension_MechanitorLink` | Convergence.Core | `Biotech/Puppetry.cs` | Marca un hediff como enlace de mecanitor alternativo (Hilo del Corazón de Marioneta). |
| `DefModExtension_NoStuffTint` | Convergence.Core | `Core/NoStuffTint.cs` | Edificio hecho de material (MadeFromStuff) cuyo arte ya trae sus colores: el juego lo multiplica por el color del material (granito gris, madera marrón…) y lo ensucia |
| `DefModExtension_PostConvergence` | Convergence.Core | `Core/PostConvergence.cs` | En un PawnKindDef de animal salvaje: solo aparece de forma natural en los biomas una vez ocurrida la Convergencia. |
| `DefModExtension_ProgressCounter` | Convergence.Core | `Traits/TraitProgression.cs` | Fuente de un contador de progresión en un ThingDef: mesas (cada producto hecho allí), plantas (cada cosecha), edificios (cada construcción terminada) |
| `DefModExtension_PuppetTier` | Convergence.Core | `Biotech/PuppetTiers.cs` | Escalón de una marioneta (docs/modulos/MARIONETAS_PROGRESION.md) |
| `DefModExtension_QiConcentration` | Convergence.Core | `Defs/CultivationDefs.cs` | Datos de la concentración de qi (va en el RoomStatDef CVG_QiConcentration). |
| `DefModExtension_RaidPremonition` | Convergence.Core | `Traits/TraitMechanics.cs` | Premonición (Regresor): un asalto contra el jugador se retrasa unas horas y llega con aviso. |
| `DefModExtension_RealmCultivationCurve` | Convergence.Core | `Traits/TraitMechanics.cs` | Velocidad de cultivo según el reino (Floración Tardía, Genio Efímero): curva orden de reino → multiplicador. |
| `DefModExtension_RealmGate` | Convergence.Stories | `SecretRealms.cs` | En el ThingDef de la puerta: opcionalmente fija el reino (si no, se elige al azar). |
| `DefModExtension_RealmSuppression` | Convergence.Core | `Techniques/SpiritGrade.cs` | Curva de la supresión de reino (va en el StatDef CVG_SpiritGrade). |
| `DefModExtension_RequiresConvergence` | Convergence.QiTech | `SpiritEngineering.cs` | En un ResearchProjectDef: no se puede empezar (y por eso tampoco completar con el banco) antes de la Convergencia (GameComponent_Convergence.Active) |
| `DefModExtension_RequiresWorkbenchLevel` | Convergence.Core | `Crafting/WorkbenchLevels.cs` | Receta (o producto de una receta) que solo se ofrece en mesas de cierto nivel. |
| `DefModExtension_Retaliation` | Convergence.Core | `Defs/BloodlineGeneDefs.cs` | Contraataque: al recibir un golpe cuerpo a cuerpo, devuelve daño al atacante (tras la Convergencia). |
| `DefModExtension_RoleEmptyMinBelievers` | Convergence.Core | `Sects/SectIdeology.cs` | En un PreceptDef de rol: el ánimo por "rol vacío" solo aparece con al menos minBelievers colonos de esa ideología (el rol se puede asignar antes) |
| `DefModExtension_SectGift` | Convergence.Factions | `SectGift.cs` | En un permiso de secta: qué regala (favores "obra maestra" y "excepcional") |
| `DefModExtension_SectIdeo` | Convergence.Factions | `SectIdeos.cs` | En un FactionDef de secta: el ícono y el color de su ideología |
| `DefModExtension_SpecialBody` | Convergence.Core | `Defs/CultivationDefs.cs` | Marca un rasgo o gen como cuerpo especial (体质). |
| `DefModExtension_SpiritBeast` | Convergence.Core | `Body/SpiritBeast.cs` | Datos del despertar de bestias espirituales (va en el HediffDef CVG_SpiritBeast): probabilidad y nivel según el combatPower del animal (cualquier animal de cualquier mod) y lo que da al carnearlo. |
| `DefModExtension_SpiritCraft` | Convergence.Core | `Traits/TraitMechanics.cs` | Etiqueta de oficio espiritual (alquimia, formaciones, talismanes…) para que la usen módulos futuros. |
| `DefModExtension_SpiritGradeMaterial` | Convergence.Core | `Techniques/SpiritGrade.cs` | En un material (stuff): las armas hechas de él tienen al menos minGrade de grado espiritual |
| `DefModExtension_SpiritRoot` | Convergence.Core | `Defs/CultivationDefs.cs` | Marca un gen o un grado de rasgo como parte de la raíz espiritual |
| `DefModExtension_StartingTrait` | Convergence.Core | `Sects/Constitutions.cs` | En un MemeDef: si la ideología del jugador empieza con este meme, al menos un colono inicial tiene el rasgo (no todos, y no por convertirse después). |
| `DefModExtension_StuffWeakness` | Convergence.Core | `Biotech/BloodDemon.cs` | Debilidad a un material o a un arma: el daño de armas hechas de stuff (o de weapons) se multiplica por factor |
| `DefModExtension_TemperConstitution` | Convergence.Core | `Sects/TemperConstitution.cs` | Configuración del ritual de templado de una constitución, en su RitualOutcomeEffectDef |
| `DefModExtension_TraitConflicts` | Convergence.Core | `Traits/TraitConflicts.cs` | Conflictos de rasgos que el vanilla no puede expresar: con un grado concreto de otro rasgo (p |
| `DefModExtension_TraitGrantsHediff` | Convergence.Core | `Biotech/BloodDemon.cs` | Rasgo que da un hediff al generarse el pawn (p |
| `DefModExtension_TraitProgression` | Convergence.Core | `Traits/TraitProgression.cs` | Rasgo que sube de grado con el uso (docs/modulos/RASGOS.md §9.2): counter es el nombre de un contador por colono (abierto: "Alchemy", "Forging", "Talismans", "SpiritFarming", "BeastTaming", "Formations", "BodyTempering", |
| `DefModExtension_TribulationSprout` | Convergence.Core | `Core/PostConvergence.cs` | En un ThingDef de planta: tras un rayo de tribulación puede brotar cerca del impacto (Champiñón de la Tribulación…). |
| `DefModExtension_TurretMuzzle` | Convergence.QiTech | `TurretMuzzle.cs` | Boca del cañón de una torreta (en el ThingDef de la torreta) |
| `DefModExtension_WoodOnCut` | Convergence.Core | `Cultivation/FruitTree.cs` | Árbol frutal: "Cosechar" da la fruta (harvestedThingDef) y rebrota; "Cortar" además da madera. |
| `DefModExtension_WuxiaRock` | Convergence.Core | `Resources/WuxiaRocks.cs` | Marca una roca natural (no de recurso) como roca wuxia: el ajuste "Mundo wuxia" la prefiere. |
| `DevelopmentShot` | Convergence.Core | `Debug/DevelopmentShot.cs` | Herramienta de revisión de interfaz: con -cvgdevshot (lanzar con run-test.ps1 -Width 1600 -Height 900) abre la ventana Desarrollo de un cultivador de prueba (raíz de fuego, su camino abierto, la tablilla de la Espada, al |
| `Dialog_CultivationDevelopment` | Convergence.Core | `UI/Dialog_CultivationDevelopment.cs` | Ventana "Desarrollo" (botón de la pestaña Cultivo) con la estructura de la pestaña de Vanilla Psycasts Expanded (decisión del usuario: copiar lo que funciona, escrito por nosotros): - Izquierda: nombre, reino y etapa con |
| `Dialog_CultivationStart` | Convergence.Core | `Core/CultivationDifficulty.cs` | Elección al empezar una partida nueva: dificultad de cultivo y cómo llega la Convergencia (inmediata o vanilla primero) |
| `DomeUtility` | Convergence.Core | `Space/IslandDome.cs` | Encendida y con qi: la cúpula funciona. |
| `ErrorEntry` | Convergence.Core | `Debug/PlayTest.cs` |  |
| `FactionRankDef` | Convergence.Core | `Cultivation/FactionRanks.cs` | Rango del jugador en una facción (discípulo externo → interno → núcleo → anciano…) |
| `FactionRankUtility` | Convergence.Core | `Cultivation/FactionRanks.cs` | Regalo que llega en cápsula al subir a este rango. |
| `FactionsHarmony` | Convergence.Factions | `SectIdeos.cs` |  |
| `FactionsShotMap` | Convergence.Factions | `FactionsShot.cs` | Capturas para la página de Steam Workshop de Factions: con -cvgfactionsshot (lanzar con run-test.ps1 -Language English -Width 1920 -Height 1080 -Visible) trae las sectas, forma a sus discípulos y marionetas en fila, y gu |
| `FloorShot` | Convergence.Core | `Debug/FloorShot.cs` | Herramienta de revisión visual: con -cvgfloorshot (y tools/run-test.ps1 -Visible) pinta un parche de 4x4 de cada suelo construible del mod, con un colono para la escala, y guarda una captura en &lt;datos&gt;/cvg_floorsho |
| `ForgeUtility` | Convergence.Core | `Crafting/ForgeUtility.cs` | Hook genérico de forja: velocidad (StatPart sobre el stat de velocidad de sus recetas) y calidad (Harmony). |
| `FormationMapComponent` | Convergence.Formations | `Formations.cs` |  |
| `FormationsStartup` | Convergence.Formations | `Formations.cs` |  |
| `FormationWorker_Binding` | Convergence.Formations | `Formations.cs` | Formación de Atadura: inmoviliza (aturde) al enemigo más fuerte del área unos segundos. |
| `FormationWorker_Protective` | Convergence.Formations | `Formations.cs` | Gran Formación Protectora: cúpula que absorbe los proyectiles enemigos; cada golpe gasta piedras. |
| `FormationWorker_QiGather` | Convergence.Formations | `Formations.cs` | Formación de Reunión de Qi: sube la concentración de qi en toda el área. |
| `FormationWorker_Swords` | Convergence.Formations | `Formations.cs` | Formación de Espadas Asesinas: lanza espadas de qi a los enemigos dentro del área. |
| `FormationWorker` | Convergence.Formations | `Formations.cs` | Combustible (piedras) que gasta cada acción del efecto. |
| `GameComponent_BandTotemTest` | Convergence.Core | `Biotech/BandTotem.cs` | Autotest del tótem de hilos: solo con -cvgbandtotemtest (y un mapa, p |
| `GameComponent_ChargeFlowTest` | Convergence.Stories | `ChargeFlowTest.cs` | Prueba del flujo real de recarga (con -cvgchargetest y -quicktest): un titiritero con una marioneta de madera casi descargada, una formación de respiración del cielo y un cargador espiritual al lado |
| `GameComponent_Convergence` | Convergence.Core | `Core/GameComponent_Convergence.cs` | Estado global de la partida: si ya ocurrió la Convergencia, cuándo, y cuándo está programada |
| `GameComponent_CropsDefenseTest` | Convergence.Core | `Debug/CropsDefenseSelfTest.cs` | Autotest de cultivos (parte 4b) y defensa diaria (parte 5) de DIA_A_DIA.md: solo con -cvgcropstest (y un mapa, p |
| `GameComponent_Dao` | Convergence.Core | `Research/DaoComprehension.cs` | Quién aporta a cada proyecto del Dao (para la carta) y qué cartas ya se enviaron. |
| `GameComponent_Endgame` | Convergence.Stories | `Endgame.cs` |  |
| `GameComponent_FactionRanks` | Convergence.Core | `Cultivation/FactionRanks.cs` |  |
| `GameComponent_FactionsShot` | Convergence.Factions | `FactionsShot.cs` |  |
| `GameComponent_FactionsTest` | Convergence.Factions | `FactionsSelfTest.cs` | Autotest de la prueba de concepto S0 (sectas sobre Royalty) |
| `GameComponent_FavorsTest` | Convergence.Factions | `FavorsTest.cs` | Recorrido de los favores de todas las sectas, como los usaría el jugador |
| `GameComponent_FlowTest` | Convergence.Core | `Debug/FlowTest.cs` | Prueba del avance a Fundación como lo hace el jugador: con -cvgflowtest (y -quicktest) un colono llega al cuello de botella de Refinamiento de Qi; sin la investigación el botón de avance está deshabilitado; se termina la |
| `GameComponent_FormationFlowTest` | Convergence.Formations | `FormationFlowTest.cs` | Prueba de formaciones y torretas como en la partida: con -cvgformflow (y -quicktest) construye un núcleo de la Formación de Espadas con 4 banderas alrededor y piedras espirituales, comprueba que el área se forme y se act |
| `GameComponent_FormationsTest` | Convergence.Formations | `FormationsSelfTest.cs` | Autotest de las formaciones |
| `GameComponent_FurnitureTest` | Convergence.Core | `Debug/FurnitureSelfTest.cs` | Autotest de los muebles de vida (Convergence: Furniture, parte 3): solo con -cvgfurnituretest (y un mapa, p |
| `GameComponent_GraftsTest` | Convergence.Grafts | `GraftsSelfTest.cs` | Autotest de los injertos |
| `GameComponent_IslandFlightTest` | Convergence.Core | `Space/IslandFlightTest.cs` | Prueba de vuelo REAL de la isla voladora (requiere Odyssey y Convergence: Flying Sect): solo con -cvgflighttest |
| `GameComponent_IslandSpaceTest` | Convergence.Core | `Space/IslandSpaceTest.cs` | Prueba de la isla voladora hasta el espacio (requiere Odyssey y Convergence: Flying Sect): solo con -cvgislandspacetest |
| `GameComponent_IslandTest` | Convergence.Core | `Space/IslandSelfTest.cs` | Autotest de la isla voladora (mod Convergence: Flying Sect, requiere Odyssey): solo con -cvgislandtest (y un mapa, p |
| `GameComponent_IslandTextInstaller` | Convergence.Core | `Space/IslandText.cs` | Mira el contexto de isla un par de veces por segundo y cambia los textos cuando cambia (mapa actual, Corazón construido). |
| `GameComponent_LinkedWallShot` | Convergence.Formations | `LinkedWallShot.cs` | Captura de las piezas de conexión (con -cvgwallshot y -quicktest, ventana visible): construye una fila, una L, una T y una cruz del muro de piedra y de la empalizada de bambú y saca una captura en &lt;datos&gt;/cvg_walls |
| `GameComponent_MechTabTest` | Convergence.Stories | `MechTabTest.cs` | Prueba del botón "Mecas" de la barra (con -cvgmechtabtest y -quicktest): replica el caso de un colono que se instala el mecaenlace |
| `GameComponent_Mentor` | Convergence.Core | `Mentor/GameComponent_Mentor.cs` | El Maestro (M6, versión mínima): llega con la Convergencia (herido o caminante), se une a la colonia como un residente muy poderoso y sin defectos, da sus lecciones una a una (objetivos simples con recompensa), protege d |
| `GameComponent_PlayTest` | Convergence.Core | `Debug/PlayTest.cs` | Arnés de pulido jugando (docs/revision/PULIDO.md): con -cvgplaytest (y -quicktest) fuerza la Convergencia, da raíces y reinos a los colonos, termina la investigación, suelta materiales, construye una muestra de nuestros  |
| `GameComponent_PuppetFightTest` | Convergence.Stories | `PuppetFightTest.cs` | Prueba de balance de las marionetas de guerra (con -cvgpuppetfighttest y -quicktest): combates reales, varias rondas por caso, contando quién gana |
| `GameComponent_QiNetTest` | Convergence.Core | `QiNetwork/QiNetworkSelfTest.cs` | Autotest de la red de qi: solo con -cvgqinettest (y un mapa, p |
| `GameComponent_QiTechAimShot` | Convergence.QiTech | `QiTechAimShot.cs` | Diagnóstico de puntería (con -cvgqitechaimshot y -quicktest, ventana visible): las dos torretas de Qi-Tech y, de referencia, la ballesta rúnica de Formations, cada una con un enemigo quieto exactamente al ESTE y después  |
| `GameComponent_QiTechLab` | Convergence.QiTech | `QiTechLabShowcase.cs` | Vitrina de Ingeniería Espiritual (con -cvgqitechlab y -quicktest, ventana visible): Convergencia activa, Microelectrónica, Fundamentos del Qi y Resonancia investigados, un banco avanzado con un resonador encendido al lad |
| `GameComponent_QiTechMilitaryTest` | Convergence.QiTech | `QiTechMilitaryTest.cs` | Autotest de las armas y torretas de Qi-Tech v0.2 |
| `GameComponent_QiTechShowcase` | Convergence.QiTech | `QiTechShowcase.cs` | Vitrina para mirar Qi-Tech en combate (con -cvgqitechshowcase y -quicktest, ventana visible): pone las dos torretas con corriente y piedras, tres colonos reclutados con el rifle de inducción, el rifle concentrado y el ca |
| `GameComponent_QiTechTest` | Convergence.QiTech | `QiTechSelfTest.cs` | Autotest de Qi-Tech |
| `GameComponent_QiTechWorkshopShot` | Convergence.QiTech | `QiTechWorkshopShot.cs` | Capturas para la página de Steam Workshop de Qi-Tech, como las del Core (WorkshopShot): con -cvgqitechworkshopshot (lanzar con run-test.ps1 -Language English -Width 1920 -Height 1080 -Visible) arma un laboratorio y una b |
| `GameComponent_RaidPremonition` | Convergence.Core | `Traits/TraitMechanics.cs` | Asaltos ya retrasados por una premonición: tick de disparo y facción, guardados con la partida. |
| `GameComponent_SecretRealms` | Convergence.Stories | `SecretRealms.cs` |  |
| `GameComponent_SectIdeos` | Convergence.Factions | `SectIdeos.cs` | La ideología de cada secta es la suya, sin nada vanilla al azar: título del líder = el de la facción (no "primer ministro"), nuestros roles con su nombre (no el que inventa el generador), ícono y color propios, y el nomb |
| `GameComponent_SectIdeoTest` | Convergence.Core | `Ideology/SectIdeoSelfTest.cs` | Autotest de los preceptos, roles y rituales de secta: solo con -cvgsectideotest (y un mapa, p |
| `GameComponent_SectWorld` | Convergence.Stories | `SectWorld.cs` |  |
| `GameComponent_StartingTraits` | Convergence.Core | `Sects/Constitutions.cs` |  |
| `GameComponent_StoriesTest` | Convergence.Stories | `StoriesSelfTest.cs` | Autotest de historias y escenarios |
| `GameComponent_Story` | Convergence.Stories | `Story.cs` | Estado de las historias: el "origen" del escenario (para que sus eventos de seguimiento lo llamen después), los eventos programados y las venganzas pendientes. |
| `GameComponent_SwordBodyMigration` | Convergence.Core | `Sects/SectIdeology.cs` | El viejo "cuerpo de espada celestial" (rasgo aparte) pasó a ser el grado 3 (jade celestial) del Cuerpo de Espada: al cargar una partida se convierte |
| `GameComponent_TraitLives` | Convergence.Core | `Traits/TraitMechanics.cs` | Vidas por rasgo (Hijo del Cielo): estado por colono, guardado en la partida. |
| `GameComponent_TurretShot` | Convergence.Formations | `TurretShot.cs` | Captura de las torretas contra enemigos (con -cvgturretshot y -quicktest, ventana visible): pone la estatua guardiana, la ballesta rúnica y la ballesta del trueno cargadas, enemigos quietos a distintos lados y saca tres  |
| `GameComponent_WorkFlowTest` | Convergence.Stories | `WorkFlowTest.cs` | Prueba del trabajo de las marionetas de trabajo (con -cvgworktest y -quicktest): las tres del titiritero (madera, albañil, minera) con trabajo a mano (cosas tiradas para acarrear, suciedad, un cultivo para cosechar) dura |
| `Gene_Bloodline` | Convergence.Core | `Bloodline/Gene_Bloodline.cs` | Clase de los genes de sangre con mecánicas propias (aura, regeneración, vidas extra, contraataque) |
| `GenStep_RealmGateSite` | Convergence.Stories | `SecretRealms.cs` | Sitio del mundo: la puerta de un reino secreto, con unos guardianes fuera. |
| `GenStep_SecretRealm` | Convergence.Stories | `SecretRealms.cs` |  |
| `Gizmo_ResourceBar` | Convergence.Core | `UI/Gizmo_ResourceBar.cs` | Barra del recurso de un camino (qi, sangre-qi…) |
| `GraftsStartup` | Convergence.Grafts | `Grafts.cs` |  |
| `GraftUtility` | Convergence.Grafts | `Grafts.cs` | Los injertos solo se pueden instalar si la colonia tiene un Altar de Injerto del Dao en el mapa. |
| `Grudge` | Convergence.Stories | `Story.cs` | Un enemigo notable muerto por el jugador: la facción volverá a por venganza. |
| `GuestLimit` | Convergence.Core | `Patches/Patch_GuestLimit.cs` | Tope de grupos de huéspedes a la vez (ajuste ConvergenceSettings.maxGuestGroups, 2 por defecto, 0 = sin tope) |
| `Hediff_CultivationRealm` | Convergence.Core | `Cultivation/Hediff_CultivationRealm.cs` | Hediff de un reino |
| `Hediff_PuppetLink` | Convergence.Core | `Biotech/Puppetry.cs` | Hediff del hilo: al ponerlo o quitarlo recalcula los componentes dinámicos (crea o borra el tracker de mecanitor), como hace el mecaenlace. |
| `Hediff_SpiritBeast` | Convergence.Core | `Body/SpiritBeast.cs` | Bestia espiritual: un estado (no un animal nuevo) con niveles 1-4 = severidad |
| `HediffComp_ColdBrittle` | Convergence.Core | `Biotech/PuppetTiers.cs` |  |
| `HediffComp_PassiveTechnique` | Convergence.Core | `Techniques/PassiveTechniques.cs` |  |
| `HediffComp_QiShield` | Convergence.Core | `Techniques/HediffComp_QiShield.cs` | El daño lo intercepta CompCultivator.PostPreApplyDamage (después de la supresión de reino y antes de la armadura y los cinturones escudo) |
| `HediffComp_SunBurn` | Convergence.Core | `Biotech/Jiangshi.cs` |  |
| `HediffCompProperties_ColdBrittle` | Convergence.Core | `Biotech/PuppetTiers.cs` | Debilidad del material que se activa con el frío (hierro, acero): la severidad del hediff sigue la temperatura del lugar (0 templado, 1 bajo cero); las etapas del XML dicen cuánto frena. |
| `HediffCompProperties_PassiveTechnique` | Convergence.Core | `Techniques/PassiveTechniques.cs` | Hediff de una técnica pasiva: su severidad es el poder de la técnica (reino × maestría × afinidad), así las etapas del XML suben solas; gana maestría con el tiempo (la práctica diaria) |
| `HediffCompProperties_QiShield` | Convergence.Core | `Techniques/HediffComp_QiShield.cs` | Escudo de qi: absorbe daño hasta agotar su capacidad (capacidad base × poder de la técnica). |
| `HediffCompProperties_SunBurn` | Convergence.Core | `Biotech/Jiangshi.cs` | Quemadura del sol: al aire libre y con luz de día el pawn recibe daño (jiangshi, criaturas yin). |
| `IncidentWorker_AllyPlea` | Convergence.Stories | `StoryEvents.cs` |  |
| `IncidentWorker_BeastMigration` | Convergence.Stories | `StoryEvents.cs` |  |
| `IncidentWorker_CelestialCondition` | Convergence.Core | `Events/CelestialCalendar.cs` | Solo se dispara después de la Convergencia (el cielo de Jiuxiao todavía no se ve antes). |
| `IncidentWorker_ClanVengeance` | Convergence.Stories | `StoryEvents.cs` |  |
| `IncidentWorker_Convergence` | Convergence.Core | `Events/IncidentWorker_Convergence.cs` | La Convergencia: un fragmento de Jiuxiao cruza el cielo, el qi empieza a fluir y despiertan las raíces espirituales y los cuerpos especiales |
| `IncidentWorker_CultistsFlee` | Convergence.Stories | `StoryEvents.cs` |  |
| `IncidentWorker_JiangshiNight` | Convergence.Core | `Biotech/Jiangshi.cs` | Noche de los fantasmas: una horda de jiangshi (mutantes de Anomaly, facción de entidades) entra por el borde y asalta la colonia |
| `IncidentWorker_RealmGate` | Convergence.Stories | `SecretRealms.cs` | Incidente: una puerta de reino secreto aparece en el mapa (en la roca junto a un paso, o en campo abierto). |
| `IncidentWorker_Story` | Convergence.Stories | `StoryEvents.cs` |  |
| `IncidentWorker_TreasureBirth` | Convergence.Stories | `StoryEvents.cs` |  |
| `IngestionOutcomeDoer_GainCultivationResource` | Convergence.Core | `Body/BodyTempering.cs` | Da recurso de cultivo al comerse (carne espiritual, sangre y núcleo de bestia, sopa espiritual) |
| `IngestionOutcomeDoer_GainResourceByQuality` | Convergence.Core | `Crafting/PillsAndRunes.cs` | Recurso de cultivo (qi, sangre-qi) escalado por la calidad de la píldora. |
| `IngestionOutcomeDoer_GiveHediffByQuality` | Convergence.Core | `Crafting/PillsAndRunes.cs` | Como IngestionOutcomeDoer_GiveHediff, pero la severidad se multiplica por la calidad de la píldora. |
| `IngestionOutcomeDoer_PurifyBloodline` | Convergence.Core | `Crafting/PillsAndRunes.cs` | Píldora de purificación de sangre: sube un nivel de pureza del linaje (purificación del Core) con una probabilidad que crece con la calidad |
| `IngestionOutcomeDoer_RemoveHediffs` | Convergence.Stories | `Story.cs` |  |
| `InspirationWorker_Cultivator` | Convergence.Core | `Traits/TraitThoughts.cs` | Inspiración de cultivo (iluminación súbita, intención de la espada, fuego de la forja): solo para cultivadores despiertos y después de la Convergencia, para que una partida sin ella no las vea. |
| `IslandEndgameBuilder` | Convergence.Core | `Space/IslandEndgameBuilder.cs` | Isla de endgame para las pruebas: radio 18, Corazón con 6 monturas de núcleo, vasijas, 4 propulsores de Odyssey, altar del timonel, cúpula celestial con su red de qi y 10 salas con paredes de roca de nube, puerta y techo |
| `IslandText` | Convergence.Core | `Space/IslandText.cs` |  |
| `IslandTextSwap` | Convergence.Core | `Space/IslandText.cs` | Cambia las entradas de texto de gravnave por las de isla en el idioma activo, y las devuelve. |
| `IslandUtility` | Convergence.Core | `Space/IslandPatches.cs` |  |
| `ITab_Cultivation` | Convergence.Core | `UI/ITab_Cultivation.cs` | Pestaña "Cultivo" del colono: Convergencia, raíz espiritual y cuerpo especial. |
| `JobDriver_Breakthrough` | Convergence.Core | `Jobs/JobDriver_Breakthrough.cs` | Retiro de avance |
| `JobDriver_Cultivate` | Convergence.Core | `Jobs/JobDriver_Cultivate.cs` | El colono se sienta en el lugar de cultivo, reúne qi y avanza en su etapa |
| `JobDriver_InfuseQi` | Convergence.Core | `QiNetwork/QiNetwork.cs` |  |
| `JobDriver_TemperBody` | Convergence.Core | `Body/BodyTempering.cs` | Templar el cuerpo en un aparato: gasta sangre-qi y la convierte en progreso del reino del cuerpo |
| `LegendaryBackstories` | Convergence.Core | `Core/LegendaryBackstories.cs` | Historias de fondo por tiers (1 comunes … 4 legendarias) |
| `MapComponent_QiOverlay` | Convergence.Core | `QiNetwork/QiSources.cs` | Capa del mapa con la concentración de qi (botón junto a las demás capas, como la de fertilidad). |
| `MapComponent_SecretRealm` | Convergence.Stories | `SecretRealms.cs` |  |
| `MapComponent_Tribulation` | Convergence.Core | `Tribulation/MapComponent_Tribulation.cs` | Tribulación celestial: tras un retiro de avance exitoso hacia un reino con tribulation, el cielo lanza rayos sobre el colono durante unos segundos |
| `MentalBreakWorker_InnerDemon` | Convergence.Core | `Events/InnerDemon.cs` | Demonio interno (心魔) como crisis mental vanilla: solo cultivadores, más probable bajo la Luna del Demonio Interno (factor DefModExtension_CelestialCondition.innerDemonBreakFactor de las condiciones activas) |
| `MentalState_InnerDemon` | Convergence.Core | `Events/InnerDemon.cs` |  |
| `MentorDef` | Convergence.Core | `Mentor/MentorDefs.cs` | Un Maestro (docs/LORE.md "El Maestro", docs/lore/MAESTROS.md): quién es, cómo se genera (raíz, reino, técnicas, rasgos solo positivos), cuánto se queda, qué lecciones da (MentorLesson, ampliables con XML) y qué deja al i |
| `MentorLesson_BuildThing` | Convergence.Core | `Mentor/MentorDefs.cs` | Tener construidos count edificios thingDef (cojín de cultivo, aparato de templado…). |
| `MentorLesson_LearnTechnique` | Convergence.Core | `Mentor/MentorDefs.cs` | Que algún discípulo conozca al menos count técnicas. |
| `MentorLesson_ReachStage` | Convergence.Core | `Mentor/MentorDefs.cs` | Que algún discípulo llegue a la etapa stage de cualquier reino (o de path); si ya venía más arriba, que avance al menos una etapa desde que empezó la lección. |
| `MentorLesson` | Convergence.Core | `Mentor/MentorDefs.cs` | Lección del Maestro: un objetivo simple que se comprueba cada poco |
| `MentorUtility` | Convergence.Core | `Mentor/GameComponent_Mentor.cs` |  |
| `NoEdgeShadowsStartup` | Convergence.Core | `Core/NoBuildingShadows.cs` |  |
| `OrbitalQi` | Convergence.Core | `Space/IslandPatches.cs` | Qi celestial en órbita (ESPACIO.md): en mapas de vacío (espacio de Odyssey) el qi ambiental es más puro y la concentración sube. |
| `PassiveTechniqueUtility` | Convergence.Core | `Techniques/PassiveTechniques.cs` |  |
| `Patch_Ability_Activate_SectEvents` | Convergence.Core | `Ideology/SectEvents.cs` |  |
| `Patch_BackstoryDescription_Stats` | Convergence.Core | `Core/BackstoryTiers.cs` | La descripción de la historia lista sus modificadores de stats. |
| `Patch_BackstoryTitle_TierColor` | Convergence.Core | `Core/BackstoryTiers.cs` |  |
| `Patch_Bill_Mech_PuppetRealm` | Convergence.Core | `Biotech/PuppetTiers.cs` | Para fabricar una marioneta, el titiritero tiene que haber llegado al reino de su escalón (además de la investigación). |
| `Patch_BiomeDef_CommonalityOfAnimal` | Convergence.Core | `Core/PostConvergence.cs` |  |
| `Patch_Building_DrawColor_NoStuffTint` | Convergence.Core | `Core/NoStuffTint.cs` |  |
| `Patch_Building_MechCharger_CanPawnChargeCurrently` | Convergence.Core | `Biotech/Puppetry.cs` | El cargador vanilla exige una red eléctrica (Power.PowerNet != null) además de estar encendido; un cargador de qi (CompQiNode: el cargador espiritual de las marionetas) no tiene red eléctrica y nunca dejaba cargar |
| `Patch_Building_MechCharger_IsPowered` | Convergence.Core | `Biotech/Puppetry.cs` | Estación de recarga con esencia espiritual: un cargador de mecas vanilla (sin consumo eléctrico, red propia) que solo funciona con combustible (piedras espirituales) y lo gasta mientras carga. |
| `Patch_Building_MechCharger_Tick` | Convergence.Core | `Biotech/Puppetry.cs` | Gasta la esencia espiritual una vez por tick mientras carga. |
| `Patch_CharacterCard_TierColors` | Convergence.Core | `Core/BackstoryTiers.cs` | Solo mientras se dibuja la ficha del colono, los títulos de historias salen con el color de su tier. |
| `Patch_CompOverseerSubject_InspectString` | Convergence.Core | `Biotech/PuppetInspectTexts.cs` |  |
| `Patch_CompPilotConsole_CanUseNow_NoBreakdown` | Convergence.Core | `Space/IslandPatches.cs` | CompPilotConsole.CanUseNow da por hecho que la consola tiene CompBreakdownable (la de vanilla lo tiene) |
| `Patch_CompPower_Inspect_Qi` | Convergence.Core | `QiNetwork/QiNetwork.cs` |  |
| `Patch_CompPowerTrader_Inspect_Qi` | Convergence.Core | `QiNetwork/QiNetwork.cs` | CompPowerTrader tiene su propio texto ("Energía generada: 0 W"); en edificios de la red de qi no se muestra. |
| `Patch_CompPowerTrader_PowerOn_Qi` | Convergence.Core | `QiNetwork/QiNetwork.cs` |  |
| `Patch_CompPowerTrader_PowerOn` | Convergence.Core | `Biotech/BandTotem.cs` | Un tótem no está en ninguna red eléctrica: su "energía" es tener combustible (piedras espirituales). |
| `Patch_EntityCodex_Hidden` | Convergence.Core | `Research/DaoComprehension.cs` | Con Anomaly, el juego oculta los proyectos de conocimiento hasta activar el monolito |
| `Patch_FactionGenerator_NewGeneratedFaction` | Convergence.Factions | `SectIdeos.cs` | Una secta recién creada (al generar el mundo o al llegar con la Convergencia) ya nace con su ideología propia. |
| `Patch_FishingUtility_GetCatchesFor` | Convergence.Core | `Core/PostConvergence.cs` | Peces con DefModExtension_PostConvergence: antes de la Convergencia no se pescan; si salen, se cambian por un pez común del mismo cuerpo de agua (el qi todavía no volvió a los ríos). |
| `Patch_Frame_CompleteConstruction` | Convergence.Core | `Traits/TraitProgression.cs` | Terminar un edificio con contador o con feng shui (formaciones) suma al constructor. |
| `Patch_GeneDef_DescriptionFull` | Convergence.Core | `Bloodline/BloodlineText.cs` | Descripción de genes de linaje: los de pureza listan los efectos al despertar y si ya están activos; los de aspecto aclaran que son cosméticos |
| `Patch_GenRecipe_MakeRecipeProducts` | Convergence.Core | `Traits/TraitProgression.cs` | Cada producto hecho en una mesa con DefModExtension_ProgressCounter suma al contador del artesano. |
| `Patch_GenStep_ScatterLumpsMineable_ChooseThingDef` | Convergence.Core | `Resources/ConvergenceMineables.cs` | Las vetas de la Convergencia no aparecen al generar mapas antes de ella, ni fuera de su clima/bioma. |
| `Patch_GenTemperature_Dome` | Convergence.Core | `Space/IslandDome.cs` | Dentro de una cúpula abastecida la temperatura es la de la cúpula. |
| `Patch_GraphicShadow_DrawWorker` | Convergence.Core | `Core/NoBuildingShadows.cs` |  |
| `Patch_GraphicShadow_Print` | Convergence.Core | `Core/NoBuildingShadows.cs` | Los edificios del mod (CVG_*) no dibujan las cajas oscuras del juego: ni la sombra rectangular (shadowData) ni el halo de borde (castEdgeShadows) |
| `Patch_GravEngine_SpawnSetup_Inspected` | Convergence.Core | `Space/IslandPatches.cs` | El motor grav vanilla hay que "inspeccionarlo" antes de usarlo (un colono lo estudia y eso lanza la misión de los mecanoides) |
| `Patch_Gravship_AddThing_Console` | Convergence.Core | `Space/IslandPatches.cs` | La consola del gravbarco: el juego la reconoce por su def; las nuestras la reconocen por tener CompPilotConsole. |
| `Patch_HediffBandNode_RecacheBandNodes` | Convergence.Core | `Biotech/BandTotem.cs` |  |
| `Patch_IncidentWorker_AggressiveAnimals` | Convergence.Core | `Economy/CultivationWealth.cs` | Las bestias huelen el qi: las manadas enloquecidas suman parte de los puntos de la riqueza de cultivo. |
| `Patch_IncidentWorker_BaseChanceThisGame` | Convergence.Core | `Core/CultivationDifficulty.cs` | Frecuencia de los eventos del mod (probabilidad base de los IncidentDef de Convergence). |
| `Patch_IncidentWorker_CanFireNow_GuestLimit` | Convergence.Core | `Patches/Patch_GuestLimit.cs` | Caravanas, visitantes, viajeros y recaudadores (todos los IncidentWorker_NeutralGroup) no llegan si el mapa ya está lleno de huéspedes; el narrador lo vuelve a intentar más adelante |
| `Patch_IncidentWorker_Raid_AdjustedRaidPoints` | Convergence.Core | `Economy/CultivationWealth.cs` | Los asaltos de facciones de cultivo suman los puntos de la riqueza de cultivo del mapa. |
| `Patch_IncidentWorker_Raid_AuraPressure` | Convergence.Core | `Events/AuraPressure.cs` |  |
| `Patch_JobGiver_AIFightEnemy_GetAbilityJob` | Convergence.Core | `Techniques/CultivatorAI.cs` | Los cultivadores NPC se cubren (escudo de qi, coraza de sangre) antes de atacar si sus condiciones lo piden. |
| `Patch_LightningStrike_QiRod` | Convergence.Core | `QiNetwork/QiSources.cs` | Un rayo de tormenta que cae cerca de un pararrayos del jugador cae en él: no hay explosión ni fuego, solo qi. |
| `Patch_ListerThings_AnyThingWithDef_GravEngine` | Convergence.Core | `Space/IslandPatches.cs` | Lo mismo que ThingsOfDef para AnyThingWithDef: el Corazón de la Isla bloquea el borrado del mapa y el ancla del cielo cuenta como ancla de gravedad (avisos de lanzamiento, abandono del mapa, aterrizaje). |
| `Patch_ListerThings_ThingsOfDef_GravEngine` | Convergence.Core | `Space/IslandPatches.cs` |  |
| `Patch_MainButtonWorker_ToggleMechTab_Disabled` | Convergence.Core | `Biotech/MechTabButton.cs` | El botón "Mecas" de la barra: vanilla lo oculta mientras no haya ningún meca en la colonia, aunque alguien ya sea mecanitor (recién instalado el mecaenlace o el Hilo del Corazón de Marioneta no se ve y parece que falta e |
| `Patch_MechanitorUtility_ShouldBeMechanitor` | Convergence.Core | `Biotech/Puppetry.cs` | El hilo de marioneta también hace mecanitor (el mecaenlace vanilla sigue funcionando igual). |
| `Patch_Mineable_DestroyMined` | Convergence.Stories | `SecretRealms.cs` | Al minar la montaña puede aparecer la puerta de un reino secreto (raro, solo tras la Convergencia). |
| `Patch_Pawn_AgeTracker_BiologicalTicksPerTick` | Convergence.Core | `Cultivation/CompCultivator.cs` | Longevidad: los reinos frenan el envejecimiento biológico de los adultos (los niños crecen normal) multiplicando el mismo valor que usan los genes (BiologicalTicksPerTick) |
| `Patch_Pawn_ButcherProducts` | Convergence.Core | `Body/SpiritBeast.cs` | Carnear una bestia espiritual da sus productos además de los normales. |
| `Patch_Pawn_GetGizmos_DaoVow` | Convergence.Core | `Traits/TraitMechanics.cs` | Botón "Voto de Pareja Dao" (o el voto de cualquier relación con vowFromRelations). |
| `Patch_Pawn_HealthScale` | Convergence.Core | `Cultivation/CompCultivator.cs` | Vida por reino: multiplica Pawn.HealthScale (salud máxima de las partes, sangrado, dolor por herida…). |
| `Patch_Pawn_InspectString_Puppet` | Convergence.Core | `Biotech/PuppetInspectTexts.cs` |  |
| `Patch_Pawn_Kill_Grudge` | Convergence.Stories | `Story.cs` |  |
| `Patch_Pawn_Kill` | Convergence.Core | `Bloodline/Gene_Bloodline.cs` |  |
| `Patch_Pawn_PostApplyDamage` | Convergence.Core | `Bloodline/Gene_Bloodline.cs` |  |
| `Patch_PawnBio_SectBackstories` | Convergence.Core | `Sects/Patch_SectBackstories.cs` | Los miembros de una secta tienen siempre su historia adulta de secta (PawnKindDef.fixedAdultBackstories) |
| `Patch_PawnGenerator_GeneratePawn` | Convergence.Core | `Cultivation/CultivationGenerator.cs` |  |
| `Patch_Plant_PlantCollected` | Convergence.Core | `Traits/TraitProgression.cs` | Cosechar una planta con DefModExtension_ProgressCounter (hierbas espirituales). |
| `Patch_PlaySettings` | Convergence.Core | `QiNetwork/QiSources.cs` |  |
| `Patch_Precept_Icon_SectRoles` | Convergence.Factions | `SectIdeos.cs` | El líder y el Gran Anciano de una secta muestran su propio ícono (vanilla usa el mismo para todas las ideologías) |
| `Patch_Projectile_Launch_Muzzle` | Convergence.QiTech | `TurretMuzzle.cs` |  |
| `Patch_Projectile_PuppetDamage` | Convergence.Core | `Biotech/PuppetTiers.cs` | Las marionetas de distancia aprovechan el reino como las de cuerpo a cuerpo: el daño y la penetración de cada disparo se multiplican (disparar más seguido no servía: la defensa de un cultivador se come casi entero un gol |
| `Patch_Projectile_PuppetPenetration` | Convergence.Core | `Biotech/PuppetTiers.cs` |  |
| `Patch_Projectile_Tick` | Convergence.Formations | `Formations.cs` |  |
| `Patch_QualityUtility_Forge` | Convergence.Core | `Crafting/ForgeUtility.cs` | Calidad de forja: lo creado en una forja sube tantos niveles como CVG_ForgeQuality (la parte decimal es una probabilidad) |
| `Patch_QualityUtility_WorkbenchLevel` | Convergence.Core | `Crafting/WorkbenchLevels.cs` | Calidad por nivel de mesa (se suma al hook de forja). |
| `Patch_RaidPremonition` | Convergence.Core | `Traits/TraitMechanics.cs` | Regresor: un asalto contra el jugador se retrasa y avisa (una vez por asalto) |
| `Patch_RecipeDef_AvailableOnNow_Level` | Convergence.Core | `Crafting/WorkbenchLevels.cs` |  |
| `Patch_RecruitAttempt_DoRecruit` | Convergence.Core | `Traits/TraitProgression.cs` | Domar un animal suma "BeastTaming" (×3 si es bestia espiritual). |
| `Patch_ResearchProjectDef_CanStartNow` | Convergence.QiTech | `SpiritEngineering.cs` |  |
| `Patch_RitualOutcome_Tournament` | Convergence.Core | `Ideology/SectRituals.cs` |  |
| `Patch_RitualTargetGravshipLaunch_Consoles` | Convergence.Core | `Space/IslandPatches.cs` | El ritual de lanzamiento busca sus consolas con ThingDefOf.PilotConsole: sin esto el altar del timonel no aparecía como objetivo del ritual. |
| `Patch_Thing_Ingested_SectEvents` | Convergence.Core | `Ideology/SectEvents.cs` |  |
| `Patch_Thought_IdeoRoleEmpty_MinBelievers` | Convergence.Core | `Sects/SectIdeology.cs` |  |
| `Patch_VacuumUtility_GetVacuum_Dome` | Convergence.Core | `Space/IslandDome.cs` | Dentro de una cúpula abastecida no hay vacío. |
| `Patch_WealthWatcher_ForceRecount` | Convergence.Core | `Economy/CultivationWealth.cs` |  |
| `Patch_World_NaturalRockTypesIn` | Convergence.Core | `Resources/WuxiaRocks.cs` | "Mundo wuxia" (ajuste del Core): las casillas eligen sus 2-3 tipos de roca solo entre las rocas wuxia, con la misma semilla por casilla que vanilla |
| `Patch_ZoneAddGrowing_ShallowWater` | Convergence.Core | `Cultivation/WaterGrowing.cs` | Zonas de cultivo sobre agua poco profunda (el loto): vanilla solo deja crear una zona de cultivo si la casilla tiene fertilidad (0,5 con Biotech) y el agua tiene 0 |
| `PawnRenderNode_Single` | Convergence.Core | `Techniques/HediffComp_QiShield.cs` | Nodo de render con una textura simple (Graphic_Single), para efectos alrededor del cuerpo (burbuja del escudo de qi) con texturas vanilla de una sola vista |
| `PillQuality` | Convergence.Core | `Crafting/PillsAndRunes.cs` | Factor de potencia de una píldora (o cualquier cosa) según su calidad |
| `PlaceWorker_BaguaMirror` | Convergence.Core | `Biotech/Jiangshi.cs` | Hace visible el radio del espejo al colocarlo. |
| `PlaceWorker_FormationRange` | Convergence.Formations | `Formations.cs` | Al colocar un núcleo o una bandera: dibuja el alcance núcleo-bandera y líneas a lo que quedaría enlazado, para que se entienda sin leer la descripción que el área es el polígono que encierran las banderas. |
| `PlaceWorker_OnQiVein` | Convergence.Core | `QiNetwork/QiNetwork.cs` | El edificio solo se puede construir con todas sus casillas sobre una veta de qi. |
| `PlaceWorker_QiNodeRadius` | Convergence.Core | `QiNetwork/QiNetwork.cs` | Dibuja el radio de conexión (pilares) y líneas a los nodos de la red que quedarían unidos. |
| `Plant_FruitTree` | Convergence.Core | `Cultivation/FruitTree.cs` | Vanilla da una sola cosecha por planta (fruta o madera) |
| `PlayTestProbe` | Convergence.Core | `Debug/PlayTest.cs` | Sondas del arnés: log, incidentes, cartas y tiempos |
| `PreCultivation` | Convergence.Core | `Cultivation/PreCultivation.cs` |  |
| `PreCultivationDef` | Convergence.Core | `Cultivation/PreCultivation.cs` | Cultivo ya iniciado al generar pawns (docs/DISENO.md §4.1d): cultivadores caídos por grietas de Jiuxiao antes de la Convergencia o que recuerdan una vida anterior |
| `PreCultivationOption` | Convergence.Core | `Cultivation/PreCultivation.cs` | Solo para pawns con cuerpo especial (camino del cuerpo). |
| `Projectile_TechniqueBullet` | Convergence.Core | `Techniques/TechniqueProjectiles.cs` | Proyectil de un impacto (aguja de metal, espina, saeta de escarcha…). |
| `Projectile_TechniqueExplosive` | Convergence.Core | `Techniques/TechniqueProjectiles.cs` | Proyectil que explota (bola de fuego…): la explosión usa el daño escalado. |
| `ProjectileDefense` | Convergence.Core | `Sects/ProjectileDefense.cs` | Defensa contra proyectiles (balas, flechas, técnicas a distancia; no explosiones) |
| `PuppetInspectTexts` | Convergence.Core | `Biotech/PuppetInspectTexts.cs` | El panel de una marioneta usa las líneas de los mecanoides de vanilla ("Supervisor", "Descontrolado", "Energía del meca") |
| `PuppetPower` | Convergence.Core | `Biotech/PuppetTiers.cs` | Fuerza calculada de una marioneta de guerra (se recalcula con el vínculo, cada 1000 ticks). |
| `PuppetTierUtility` | Convergence.Core | `Biotech/PuppetTiers.cs` |  |
| `PuppetUtility` | Convergence.Core | `Biotech/Puppetry.cs` |  |
| `QiConcentrationUtility` | Convergence.Core | `Cultivation/QiConcentration.cs` | Concentración de qi (feng shui básico): qi ambiental + qi de los edificios y de los suelos (por celda) con DefModExtension_FengShui |
| `QiElementDef` | Convergence.Core | `Defs/CultivationDefs.cs` | Elemento del qi (metal, madera, agua, fuego, tierra…) |
| `QiNet` | Convergence.Core | `QiNetwork/QiNetwork.cs` | ...y cada tanto qi condensado deja un condenseProduct al lado. |
| `QiNetworkManager` | Convergence.Core | `QiNetwork/QiNetwork.cs` |  |
| `QiNetworkUtility` | Convergence.Core | `QiNetwork/QiNetwork.cs` |  |
| `QiSourceUtility` | Convergence.Core | `QiNetwork/QiSources.cs` | Fuentes de qi que no son de flujo constante y el mapa de qi (docs/modulos/RED_QI.md): la marea celeste para las formaciones del aire, el pararrayos del trueno celestial (ráfagas de qi con tormentas y tribulaciones) y la  |
| `QiTechHarmony` | Convergence.QiTech | `TurretMuzzle.cs` |  |
| `QiTechWorkshopShotMap` | Convergence.QiTech | `QiTechWorkshopShot.cs` | Con -cvgqitechworkshopshot: el quicktest arranca en bosque templado llano (como las capturas del Core). |
| `QualityChance` | Convergence.Core | `Sects/StockGenerator_Quality.cs` |  |
| `QuestNode_IfConvergence` | Convergence.Stories | `SecretRealms.cs` | Nodo de misión: ejecuta su hijo solo si la Convergencia ya ocurrió. |
| `ReachabilityAudit` | Convergence.Core | `Debug/ReachabilityAudit.cs` | Auditoría de alcanzabilidad desde el lado del jugador: con -cvgreach (sin -quicktest) revisa al terminar de cargar que cada edificio CVG se pueda construir (categoría, investigación investigable, materiales conseguibles) |
| `RealmBonusUtility` | Convergence.Core | `Cultivation/RealmBonusUtility.cs` | Textos legibles de lo que da un reino: sección "Bonus del reino" de la pestaña y "antes → después" de la carta. |
| `RealmLoot` | Convergence.Stories | `SecretRealms.cs` |  |
| `RealmSpawn` | Convergence.Stories | `SecretRealms.cs` |  |
| `RealmTracker` | Convergence.Core | `Cultivation/RealmTracker.cs` | Progreso de un colono en UN camino de cultivo: reino, etapa (1..N), progreso, recurso del camino y calidad del fundamento |
| `Recipe_InstallGraft` | Convergence.Grafts | `Grafts.cs` | Instalación de un injerto que sustituye una parte (brazo, mano, ojo, pierna). |
| `Recipe_InstallGraftImplant` | Convergence.Grafts | `Grafts.cs` | Instalación de un injerto que se añade a la parte natural (huesos, venas). |
| `RecipeWorker_EngraveRunes` | Convergence.Core | `Crafting/PillsAndRunes.cs` | Grabar runas: la receta toma un arma de grado 0 y no la destruye, le suma un grado espiritual (CompSpiritGrade.bonus) |
| `RecordWorker_TimeCultivating` | Convergence.Core | `Core/CultivationLife.cs` | Tiempo cultivando o templando el cuerpo (registro de la ficha del colono). |
| `RenderTest` | Convergence.Core | `Debug/RenderTest.cs` | Herramienta de arte: con -cvgrendertest guarda retratos (sur, este, norte) de colonos con cada gen propio que dibuja partes del cuerpo y con cada prenda propia en los 5 tipos de cuerpo, en &lt;datos&gt;/cvg_renders/ |
| `ResearchMod_TriggerConvergence` | Convergence.Core | `Events/IncidentWorker_Convergence.cs` | Al completar el proyecto de investigación, dispara la Convergencia (modo Investigación). |
| `ResourceSelfTest` | Convergence.Core | `Resources/ResourceSelfTest.cs` | Autotest de recursos (con -cvgselftest): vetas de la Convergencia en el mapa antes y después de ConvergenceMineables.OnConvergence, y materiales de los módulos cargados. |
| `Result` | Convergence.Core | `Space/IslandEndgameBuilder.cs` |  |
| `RitualObligationTrigger_Admission` | Convergence.Core | `Ideology/SectRituals.cs` | Un colono se une a la ideoligión de la secta → se puede celebrar su admisión. |
| `RitualObligationTrigger_AdmissionProperties` | Convergence.Core | `Ideology/SectRituals.cs` |  |
| `RitualObligationTrigger_Ascension` | Convergence.Core | `Ideology/SectRituals.cs` | Un miembro sube de reino → se puede celebrar su ascensión. |
| `RitualObligationTrigger_AscensionProperties` | Convergence.Core | `Ideology/SectRituals.cs` |  |
| `RitualOutcomeComp_MinBelievers` | Convergence.Core | `Sects/SectIdeology.cs` | Componente de ritual: no se puede empezar sin al menos count creyentes de la ideología presentes además de los roles (p |
| `RitualOutcomeEffectWorker_TemperConstitution` | Convergence.Core | `Sects/TemperConstitution.cs` | Ritual de templado: además del ánimo según la calidad (como cualquier ritual), sube un paso la constitución del sujeto con la probabilidad del paso |
| `RitualRoleConstitutionSubject` | Convergence.Core | `Sects/TemperConstitution.cs` | Sujeto del templado: un colono libre al que le queda un paso de la constitución y con el costo en el almacén. |
| `RoleRequirement_HasTrait` | Convergence.Core | `Sects/Constitutions.cs` | Requisito de un rol de Ideology: tener alguno de estos rasgos (cualquier grado ≥ minDegree). |
| `RoomInfo` | Convergence.Core | `Space/IslandEndgameBuilder.cs` |  |
| `RoomStatWorker_QiConcentration` | Convergence.Core | `Cultivation/QiConcentration.cs` | Stat de habitación CVG_QiConcentration: qi ambiental + edificios de feng shui de la habitación. |
| `RoyalTitlePermitWorker_SectGift` | Convergence.Factions | `SectGift.cs` | Favor de regalo de una secta: como el envío de recursos de Royalty (cápsula donde se apunta, gratis fuera del enfriamiento o pagando contribución), pero con los objetos de DefModExtension_SectGift y, si corresponde, a el |
| `ScenarioShot` | Convergence.Core | `Debug/ScenarioShot.cs` | Revisión de la lista de escenarios: con -cvgscenarioshot (sin -quicktest) abre "Elegir escenario" desde el menú principal, selecciona el primer escenario de Convergence, guarda &lt;datos&gt;/cvg_renders/scenarios.png y c |
| `ScenPart_ConvergenceOccurred` | Convergence.Stories | `Story.cs` | La Convergencia ya ocurrió cuando empieza la partida (la Calamidad pasó antes). |
| `ScenPart_StartingBloodline` | Convergence.Stories | `Story.cs` | Los colonos iniciales nacen con un linaje de sangre (si el mod de razas lo trae). |
| `ScenPart_StartingCultivators` | Convergence.Stories | `Story.cs` | Los colonos iniciales ya cultivan: raíz, reino, etapa y técnicas. |
| `ScenPart_StartingPuppets` | Convergence.Stories | `ScenPuppets.cs` | Escenario Maestro de Marionetas ("como el inicio del mecanitor"): el primer colono lleva el hilo del corazón de marioneta y empieza con marionetas ya enlazadas a él (relación de supervisor vanilla, la misma que usa ScenP |
| `ScenPart_StoryOrigin` | Convergence.Stories | `Story.cs` | Guarda el origen del escenario y programa sus eventos de seguimiento. |
| `ScheduledStory` | Convergence.Stories | `Story.cs` |  |
| `SecretRealmDef` | Convergence.Stories | `SecretRealms.cs` | Contenido de un reino secreto, todo por XML. |
| `SecretRealmUtility` | Convergence.Stories | `SecretRealms.cs` |  |
| `SectEvents` | Convergence.Core | `Ideology/SectEvents.cs` | Sucesos del cultivo para los preceptos de secta (Ideología): el Core registra HistoryEventDef propios y los preceptos del mod Sects (Mods/Ideology) reaccionan con pensamientos |
| `SectGiftOption` | Convergence.Factions | `SectGift.cs` |  |
| `SectPair` | Convergence.Stories | `SectWorld.cs` | 0 = nadie, 1 = A, 2 = B (a quién ayudó el jugador). |
| `SectSpec` | Convergence.Factions | `FactionsSelfTest.cs` | Lo que distingue a cada secta en la prueba genérica de contenido (todo lo demás sale de la plantilla). |
| `SectWar` | Convergence.Stories | `SectWorld.cs` |  |
| `Showcase` | Convergence.Core | `Debug/Showcase.cs` | Herramienta de revisión visual: con -cvgshowcase (y tools/run-test.ps1 -Visible) coloca en el mapa de prueba cada edificio y planta del mod (los rotables en sur y este), con colonos al lado para la escala, recorre la gri |
| `SO_Beasts` | Convergence.Stories | `StoryEngine.cs` | Animales: hostiles (manhunter) o de paso. |
| `SO_Chain` | Convergence.Stories | `StoryEngine.cs` | Programa otro evento (siguiente paso de una cadena). |
| `SO_Condition` | Convergence.Stories | `StoryEngine.cs` |  |
| `SO_Damage` | Convergence.Stories | `StoryEngine.cs` |  |
| `SO_Ending` | Convergence.Stories | `Endgame.cs` | Resultado de la carta de elección final: ascend, seal o rule |
| `SO_Flag` | Convergence.Stories | `StoryEngine.cs` |  |
| `SO_Goodwill` | Convergence.Stories | `StoryEngine.cs` |  |
| `SO_Hediff` | Convergence.Stories | `StoryEngine.cs` | Manda el grupo, o lo aplaza un día si ya hay huéspedes en el mapa (como mucho StoryUtility.VisitMaxTries veces). |
| `SO_Hunt` | Convergence.Stories | `StoryEngine.cs` | Misión de caza: aparece una bestia; al matarla (a tiempo) ocurre el evento de éxito, si no el de fracaso. |
| `SO_Inspire` | Convergence.Stories | `StoryEngine.cs` |  |
| `SO_Items` | Convergence.Stories | `StoryEngine.cs` |  |
| `SO_Join` | Convergence.Stories | `StoryEngine.cs` | Gente que se une a la colonia. |
| `SO_Mental` | Convergence.Stories | `StoryEngine.cs` |  |
| `SO_Progress` | Convergence.Stories | `StoryEngine.cs` | Comprensión súbita: progreso de cultivo a un cultivador. |
| `SO_Raid` | Convergence.Stories | `StoryEngine.cs` | Clave Keyed del mensaje que se muestra si el resultado ocurre (opcional). |
| `SO_SectWar` | Convergence.Stories | `SectWorld.cs` | Resultado de un evento sobre la guerra actual entre dos sectas: favorA/favorB (el jugador ayudó), peace (la mediación acaba la guerra). |
| `SO_Skill` | Convergence.Stories | `StoryEngine.cs` |  |
| `SO_TakeItems` | Convergence.Stories | `StoryEngine.cs` |  |
| `SO_Thought` | Convergence.Stories | `StoryEngine.cs` | random, all, cultivator |
| `SO_Visit` | Convergence.Stories | `StoryEngine.cs` | Un grupo de visitantes de la facción. |
| `SoundCueDef` | Convergence.Core | `Defs/SoundCueDef.cs` | Un "aviso sonoro" del mod (Convergencia, avance, tribulación…) |
| `SoundCues` | Convergence.Core | `Defs/SoundCueDef.cs` |  |
| `SpecialThingFilterWorker_SpiritGraded` | Convergence.Core | `Crafting/PillsAndRunes.cs` | Armas que grabar runas no puede mejorar (ya tienen grado espiritual 1+ o no llevan grado): la receta las excluye. |
| `SpiritBeastUtility` | Convergence.Core | `Body/SpiritBeast.cs` | Despertar de animales tras la Convergencia y productos al carnear. |
| `SpiritEngineeringStartup` | Convergence.QiTech | `SpiritEngineering.cs` |  |
| `SpiritGradeInjection` | Convergence.Core | `Crafting/PillsAndRunes.cs` | Todas las armas pueden recibir runas: se les añade CompSpiritGrade (grado 0) si no lo tienen. |
| `SpiritGradeUtility` | Convergence.Core | `Techniques/SpiritGrade.cs` | Supresión de reino (DISENO.md §4.1c): el daño contra un cultivador se reduce si el grado espiritual del ataque es menor que su reino |
| `SpiritRoot` | Convergence.Core | `Cultivation/SpiritRoot.cs` | Raíz espiritual de un colono: calidad + uno o más elementos |
| `SpiritRootGradeDef` | Convergence.Core | `Defs/CultivationDefs.cs` | Calidad de la raíz espiritual (inferior → celestial). |
| `StatPart_Backstory` | Convergence.Core | `Core/BackstoryTiers.cs` |  |
| `StatPart_CelestialCondition` | Convergence.Core | `Events/CelestialCalendar.cs` | Aplica a un stat de pawn los efectos de las condiciones celestiales activas en su mapa. |
| `StatPart_CultivationDifficulty` | Convergence.Core | `Core/CultivationDifficulty.cs` | Velocidad de cultivo de la dificultad (en CVG_CultivationSpeed y CVG_BodyTemperingSpeed). |
| `StatPart_DualCultivation` | Convergence.Core | `Traits/TraitMechanics.cs` |  |
| `StatPart_ElementalResonance` | Convergence.Core | `Cultivation/ElementalResonance.cs` | Multiplica un stat de cultivo por la resonancia de los accesorios puestos con la raíz del portador. |
| `StatPart_EnvironmentResistance` | Convergence.Core | `Cultivation/EnvironmentResistance.cs` | Resistencia al entorno (pedido del usuario, 2026-10-17): el stat CVG_EnvironmentResistance (lo dan las etapas de los reinos: Núcleo Dorado 50 %) se traduce en stats vanilla: rango de temperatura cómoda más ancho, menos i |
| `StatPart_Forge` | Convergence.Core | `Crafting/ForgeUtility.cs` | Multiplica la velocidad de trabajo por CVG_ForgeSpeed mientras el colono trabaja en una forja. |
| `StatPart_HelmsmanRealm` | Convergence.Core | `Space/IslandPatches.cs` | Alcance del gravbarco por el reino del cultivador más fuerte de la colonia (altar del timonel): perRealmLevel por nivel. |
| `StatPart_ImplantDissonance` | Convergence.Core | `Tribulation/ImplantDissonance.cs` | Disonancia celestial de los implantes que no la definen (biónicos vanilla o de otros mods): según el nivel tecnológico del implante, por el factor de los ajustes |
| `StatPart_PuppetBandwidth` | Convergence.Core | `Biotech/Puppetry.cs` | Ancho de banda extra de un maestro de marionetas: +N por nivel de reino (fuerza del alma). |
| `StatPart_PuppetPower` | Convergence.Core | `Biotech/PuppetTiers.cs` | Aplica la fuerza de PuppetPower a las marionetas de guerra (daño, defensa, velocidad, cadencia). |
| `StatPart_RealmCurveTraits` | Convergence.Core | `Traits/TraitMechanics.cs` |  |
| `StatPart_RequiresMeleeWeapon` | Convergence.Core | `Sects/ProjectileDefense.cs` | El stat vale 0 si el pawn no empuña un arma cuerpo a cuerpo con alguna de estas etiquetas (weaponTags) o categorías: el desvío de los espadachines solo funciona con la espada en la mano. |
| `StatPart_WorkbenchLevel` | Convergence.Core | `Crafting/WorkbenchLevels.cs` |  |
| `StatResource` | Convergence.Core | `Defs/CultivationPathDefs.cs` | Recurso con máximo y regeneración por stats del camino (el qi usa CVG_QiMax / CVG_QiRegen). |
| `StatWorker_Cultivation` | Convergence.Core | `Cultivation/StatWorker_Cultivation.cs` | Stat de cultivo: solo se muestra en cultivadores despiertos (de cualquier camino). |
| `StatWorker_SpiritGrade` | Convergence.Core | `Techniques/SpiritGrade.cs` | Stat de solo lectura: grado espiritual del arma (del comp) |
| `StockGenerator_QualityDefs` | Convergence.Core | `Sects/StockGenerator_Quality.cs` | Stock de especialidad por lista de objetos (p |
| `StockGenerator_QualityTag` | Convergence.Core | `Sects/StockGenerator_Quality.cs` | Como StockGenerator_Tag, pero solo con armas de ciertas weaponTags (p |
| `StoriesStartup` | Convergence.Stories | `Story.cs` |  |
| `StoryContext` | Convergence.Stories | `StoryEngine.cs` |  |
| `StoryEngine` | Convergence.Stories | `StoryEngine.cs` |  |
| `StoryEventDef` | Convergence.Stories | `StoryEngine.cs` | Un evento de historia definido por XML: texto, condiciones, opciones del jugador y resultados (StoryOutcome) |
| `StoryEvents` | Convergence.Stories | `StoryEvents.cs` |  |
| `StoryFollowUp` | Convergence.Stories | `Story.cs` | Un evento de seguimiento programado por un escenario (clave del evento + rango de días). |
| `StoryOption` | Convergence.Stories | `StoryEngine.cs` | Si es true, el evento solo ocurre cuando factionToken existe. |
| `StoryOutcome` | Convergence.Stories | `StoryEngine.cs` |  |
| `StoryTask` | Convergence.Stories | `StoryEngine.cs` | Coste en objetos del almacén de la colonia (opcional). |
| `StoryUtility` | Convergence.Stories | `Story.cs` |  |
| `StuffWeaknessUtility` | Convergence.Core | `Biotech/BloodDemon.cs` |  |
| `SymbolResolver_Interior_ForgottenSanctuary` | Convergence.Stories | `SecretRealms.cs` | Interior del templo antiguo: sarcófagos con discípulos de una secta olvidada, dormidos en cultivo, y su tesoro. |
| `TabShot` | Convergence.Core | `Debug/TabShot.cs` | Revisión de la pestaña Cultivo: con -cvgtabshot (lanzar con run-test.ps1 -Width 1600 -Height 900) sube un colono a Núcleo Dorado etapa 2 con las mismas acciones de debug que usa el jugador (avance forzado, progreso, llen |
| `TechniqueExtension` | Convergence.Core | `Techniques/TechniqueDefs.cs` | Convierte un AbilityDef vanilla en técnica de cultivo: manual, fila del árbol, requisitos y escalado |
| `TechniqueManualDef` | Convergence.Core | `Techniques/TechniqueDefs.cs` | Manual de técnicas (功法): un árbol de técnicas de un camino de cultivo y, opcionalmente, de un elemento |
| `TechniqueMasteryDef` | Convergence.Core | `Techniques/TechniqueDefs.cs` | Nivel de maestría de una técnica (principiante → incomparable) |
| `TechniqueSet` | Convergence.Core | `Techniques/PassiveTechniques.cs` | Conjunto de combate (psyset de VPE): hasta 3-4 técnicas activas; solo las del activo tienen gizmo. |
| `TechniqueSetUtility` | Convergence.Core | `Techniques/PassiveTechniques.cs` | Camino del que salió el conjunto automático (null = hecho por el jugador). |
| `TechniqueTracker` | Convergence.Core | `Techniques/TechniqueTracker.cs` | Técnicas de un colono (va dentro de CompCultivator): manuales conocidos, técnicas desbloqueadas, experiencia de maestría por técnica, puntos de comprensión para desbloquear y qué técnicas se ven en la barra |
| `TechniqueUtility` | Convergence.Core | `Techniques/TechniqueUtility.cs` | Consultas de técnicas: requisitos, poder (reino × maestría × afinidad), coste y reparto a PNJ. |
| `TemperStep` | Convergence.Core | `Sects/TemperConstitution.cs` |  |
| `ThoughtWorker_CVG_RealmRank` | Convergence.Core | `Traits/TraitThoughts.cs` | Joven maestro arrogante: orgulloso si su reino es el más alto de la colonia (etapa 0), herido si alguien lo supera (etapa 1) |
| `ThoughtWorker_CVG_Seclusion` | Convergence.Core | `Traits/TraitThoughts.cs` | Amante del retiro: contento solo en su propia habitación cerrada (+) y molesto con mucha gente alrededor (−) |
| `ThoughtWorker_Precept_MissedCultivation` | Convergence.Core | `Traits/TraitThoughts.cs` | Precepto "cultivo obligatorio": pensamiento negativo mientras un cultivador despierto lleva un día sin cultivar (no tiene el recuerdo de "cultivé hoy", que dura un día) |
| `TraitChance` | Convergence.Core | `Cultivation/CultivatorPawnKind.cs` | Un rasgo (con su grado) que un tipo de pawn recibe con cierta probabilidad. |
| `TraitConflictEntry` | Convergence.Core | `Traits/TraitConflicts.cs` |  |
| `TraitConflictUtility` | Convergence.Core | `Traits/TraitConflicts.cs` | Grado del otro rasgo (vacío = cualquiera). |
| `TraitGrantsHediffUtility` | Convergence.Core | `Biotech/BloodDemon.cs` |  |
| `TraitMechanics_Vow` | Convergence.Core | `Traits/TraitMechanics.cs` |  |
| `TraitMechanics` | Convergence.Core | `Traits/TraitMechanics.cs` | Hooks de rasgos y relaciones: StatParts sobre la velocidad de cultivo, voto de Pareja Dao, vidas por rasgo. |
| `TraitProgression` | Convergence.Core | `Traits/TraitProgression.cs` |  |
| `TraitProgressionStep` | Convergence.Core | `Traits/TraitProgression.cs` |  |
| `TraitProgressTracker` | Convergence.Core | `Traits/TraitProgression.cs` | Contadores de uso de un colono (en CompCultivator). |
| `TraitSelfTest` | Convergence.Core | `Traits/TraitSelfTest.cs` | Autotest de rasgos y relaciones (con -cvgselftest): rasgos CVG cargados, cultivo dual de una relación con DefModExtension_DualCultivation y vida extra por rasgo. |
| `TranslationReportShot` | Convergence.Core | `Debug/TranslationReportShot.cs` | Herramienta de revisión: con -cvgtransreport escribe en el log los errores de traducción del idioma activo (los que cuenta el aviso vanilla "Translation data … has N errors": errores de carga y de cada paquete de DefInje |
| `Tribulation` | Convergence.Core | `Tribulation/MapComponent_Tribulation.cs` | Acción de debug: tribulación hacia el reino siguiente (o el actual si ya está en el último). |
| `TribulationSproutUtility` | Convergence.Core | `Core/PostConvergence.cs` | Crecimiento inicial (0–1). |
| `UnityCompat` | Convergence.Core | `Core/UnityCompat.cs` | Miembros de Unity que la 1.7 (motor Waylith, capa UnityStubs) ya no tiene |
| `WeatherEvent_TribulationStrike` | Convergence.Core | `Tribulation/WeatherEvent_TribulationStrike.cs` | Rayo de tribulación: el destello, el rayo y el trueno vanilla, pero sin la explosión de fuego del rayo normal (el daño lo aplica MapComponent_Tribulation directamente al colono). |
| `WorkbenchLevel` | Convergence.Core | `Crafting/WorkbenchLevels.cs` | Un nivel de una mesa mejorable: arte, coste para llegar a él, velocidad y calidad. |
| `WorkbenchLevelStartup` | Convergence.Core | `Crafting/WorkbenchLevels.cs` | Velocidad por nivel: StatPart sobre WorkTableWorkSpeedFactor (stat de la mesa). |
| `WorkbenchLevelUtility` | Convergence.Core | `Crafting/WorkbenchLevels.cs` |  |
| `WorkGiver_Cultivate` | Convergence.Core | `Jobs/WorkGiver_Cultivate.cs` | Busca un lugar de cultivo libre (cualquier edificio con DefModExtension_CultivationSpot). |
| `WorkGiver_TemperBody` | Convergence.Core | `Body/BodyTempering.cs` | Busca un aparato de templado libre para quien puede templar el cuerpo ahora (tiene sangre-qi). |
| `WorkshopShot` | Convergence.Core | `Debug/WorkshopShot.cs` |  |
| `WorkshopShotMap` | Convergence.Core | `Debug/WorkshopShot.cs` | Capturas para la página de Steam Workshop del Core: con -cvgworkshopshot (lanzar con run-test.ps1 -Language English -Width 1920 -Height 1080) dispara la Convergencia, arma una escena de tres cultivadores, y guarda en &lt |
| `WorldMode` | Convergence.Core | `Core/ConvergenceSettings.cs` | Tecno-cultivo o cultivo clásico (sin tecnología) |
