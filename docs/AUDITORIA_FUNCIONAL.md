# Auditoría funcional

Generado por `tools/audit_funcional.py` (solo lectura). Convergence.Sects no se audita.

| Gravedad | Sección | Hallazgos |
|---|---|---|
| ALTA | Dice que sube la concentración de qi y no tiene con qué | 1 |
| MEDIA | Código que ningún XML usa (la función nunca se activa) | 15 |
| MEDIA | Edificios rectangulares que se giran sin rotar el dibujo | 1 |
| MEDIA | Investigaciones que no desbloquean nada | 2 |
| MEDIA | Objetos que no se consiguen de ninguna forma | 2 |
| MEDIA | Recetas que no están en ninguna mesa | 9 |
| BAJA | Descripciones de menos de 2 frases (la regla pide lore de 2-3) | 51 |
| BAJA | Objetos con valor 0 | 4 |
| BAJA | Sin descripción | 6 |
| BAJA | Sube el qi pero la descripción no lo dice | 32 |

## [ALTA] Dice que sube la concentración de qi y no tiene con qué

- **Convergence.Formations** `CVG_QiGatheringCore` — sin DefModExtension_FengShui ni comp de qi

## [MEDIA] Código que ningún XML usa (la función nunca se activa)

- **Convergence.Core** `Alert_CVG_QiDeviation` — clase Alert_CVG_QiDeviation : Alert_Critical en Source\Convergence.Core\Core\CultivationLife.cs
- **Convergence.Core** `CompAbilityEffect_ChainLightning` — clase CompAbilityEffect_ChainLightning : CompAbilityEffect en Source\Convergence.Core\Techniques\ChainLightning.cs
- **Convergence.Core** `CompAbilityEffect_Cone` — clase CompAbilityEffect_Cone : CompAbilityEffect en Source\Convergence.Core\Abilities\AbilityComps.cs
- **Convergence.Core** `CompAbilityEffect_GiveHediffInRadius` — clase CompAbilityEffect_GiveHediffInRadius : CompAbilityEffect en Source\Convergence.Core\Abilities\AbilityComps.cs
- **Convergence.Core** `CompAbilityEffect_TechniqueHediff` — clase CompAbilityEffect_TechniqueHediff : CompAbilityEffect en Source\Convergence.Core\Techniques\TechniqueAbilityComps.cs
- **Convergence.Core** `CompAbilityEffect_TechniqueProjectile` — clase CompAbilityEffect_TechniqueProjectile : CompAbilityEffect en Source\Convergence.Core\Techniques\TechniqueAbilityComps.cs
- **Convergence.Core** `CompPlantSpread` — clase CompPlantSpread : ThingComp en Source\Convergence.Core\Cultivation\CompPlantSpread.cs
- **Convergence.Core** `CompUseEffect_SpiritBurst` — clase CompUseEffect_SpiritBurst : CompUseEffect en Source\Convergence.Core\Crafting\PillsAndRunes.cs
- **Convergence.Core** `HediffComp_ColdBrittle` — clase HediffComp_ColdBrittle : HediffComp en Source\Convergence.Core\Biotech\PuppetTiers.cs
- **Convergence.Core** `RitualObligationTrigger_AdmissionProperties` — clase RitualObligationTrigger_AdmissionProperties : RitualObligationTriggerProperties en Source\Convergence.Core\Ideology\SectRituals.cs
- **Convergence.Core** `RitualObligationTrigger_AscensionProperties` — clase RitualObligationTrigger_AscensionProperties : RitualObligationTriggerProperties en Source\Convergence.Core\Ideology\SectRituals.cs
- **Convergence.Core** `StatPart_DualCultivation` — clase StatPart_DualCultivation : StatPart en Source\Convergence.Core\Traits\TraitMechanics.cs
- **Convergence.Core** `StatPart_RealmCurveTraits` — clase StatPart_RealmCurveTraits : StatPart en Source\Convergence.Core\Traits\TraitMechanics.cs
- **Convergence.Core** `StatPart_WorkbenchLevel` — clase StatPart_WorkbenchLevel : StatPart en Source\Convergence.Core\Crafting\WorkbenchLevels.cs
- **Convergence.Stories** `DefModExtension_RealmGate` — clase DefModExtension_RealmGate : DefModExtension en Source\Convergence.Stories\SecretRealms.cs

## [MEDIA] Edificios rectangulares que se giran sin rotar el dibujo

- **Convergence.BiotechWuxia** `CVG_BloodDrumOfReturn` — tamaño (1,2), Graphic_Single con drawRotated false: al girarlo el dibujo no acompaña la huella

## [MEDIA] Investigaciones que no desbloquean nada

- **Convergence.Core** `CVG_Research_SecretRealms` — ningún def ni código la pide
- **Convergence.Core** `CVG_SpiritualAnomalyStudy` — ningún def ni código la pide

## [MEDIA] Objetos que no se consiguen de ninguna forma

- **Convergence.BiotechWuxia** `CVG_PuppetGun_JadeNeedle` — no se fabrica, no lo nombra ningún otro def (comercio, botín, evento) ni el código
- **Convergence.BiotechWuxia** `CVG_PuppetGun_RepeatingCrossbow` — no se fabrica, no lo nombra ningún otro def (comercio, botín, evento) ni el código

## [MEDIA] Recetas que no están en ninguna mesa

- **Convergence.Grafts** `CVG_Remove_DaoBones` — sin recipeUsers y ninguna mesa la lista
- **Convergence.Grafts** `CVG_Remove_DragonVeins` — sin recipeUsers y ninguna mesa la lista
- **Convergence.Grafts** `CVG_Remove_QilinSkin` — sin recipeUsers y ninguna mesa la lista
- **Convergence.Resources** `CVG_Make_StoneBlocksBlackJade` — sin recipeUsers y ninguna mesa la lista
- **Convergence.Resources** `CVG_Make_StoneBlocksCelestialMarble` — sin recipeUsers y ninguna mesa la lista
- **Convergence.Resources** `CVG_Make_StoneBlocksCloudStone` — sin recipeUsers y ninguna mesa la lista
- **Convergence.Resources** `CVG_Make_StoneBlocksDragonObsidian` — sin recipeUsers y ninguna mesa la lista
- **Convergence.Resources** `CVG_Make_StoneBlocksTribulationRock` — sin recipeUsers y ninguna mesa la lista
- **Convergence.Resources** `CVG_Make_StoneBlocksVermilionStone` — sin recipeUsers y ninguna mesa la lista

## [BAJA] Descripciones de menos de 2 frases (la regla pide lore de 2-3)

- **Convergence.Alchemy** `CVG_BiguCalm` — HediffDef: "The fasting pill keeps the stomach quiet: hunger comes more slowly for a while."
- **Convergence.Alchemy** `CVG_CloudTeaClarity` — HediffDef: "A cup of cloud tea: the mind is clear and qi gathers a little faster for a few hours."
- **Convergence.Alchemy** `CVG_IncenseCalm` — HediffDef: "Incense smoke: a quiet heart and freer qi, but slower senses."
- **Convergence.BiotechWuxia** `CVG_BloodArmor` — HediffDef: "Red as a temple door and hard as lacquer, an armor of hardened blood swallows blows until it breaks."
- **Convergence.BiotechWuxia** `CVG_PuppetMaterial_Iron` — HediffDef: "Iron and bronze joints are strong, but below freezing they stiffen: the puppet moves and works slower in the c"
- **Convergence.BiotechWuxia** `CVG_PuppetMaterial_Steel` — HediffDef: "Spirit steel keeps a little qi in the metal, so the cold only stiffens it half as much as iron."
- **Convergence.BiotechWuxia** `CVG_PuppetMaterial_Wood` — HediffDef: "Spirit wood moves as lightly as a branch in the wind, and burns like one: fire does double damage."
- **Convergence.Core** `CVG_BlazingMeridians` — HediffDef: "Fire qi rages through the meridians: the body moves and strikes beyond its limits, and it burns."
- **Convergence.Core** `CVG_Hediff_QiRefining` — HediffDef: "This cultivator is in the Qi Refining realm: qi flows through the opened meridians and slowly strengthens the "
- **Convergence.Core** `CVG_MentorProtection` — HediffDef: "The master fights to protect the colony with all the strength of a Nascent Soul."
- **Convergence.Core** `CVG_Passive_BlazingMeridians` — HediffDef: "Fire qi runs hot through the meridians."
- **Convergence.Core** `CVG_Passive_BoundlessSea` — HediffDef: "The mind is as deep and calm as the Eastern Sea."
- **Convergence.Core** `CVG_Passive_CloudSteps` — HediffDef: "Every step slides like a cloud over a lake."
- **Convergence.Core** `CVG_Passive_DivineSense` — HediffDef: "The spirit sense spreads like ripples on a pond."
- **Convergence.Core** `CVG_Passive_DragonElephantMight` — HediffDef: "The strength of the dragon and the elephant fills the muscles."
- **Convergence.Core** `CVG_Passive_EdgeOfTheHeart` — HediffDef: "The heart cuts before the hand moves."
- **Convergence.Core** `CVG_Passive_EndlessMountain` — HediffDef: "The body weighs like Mount Tai."
- **Convergence.Core** `CVG_Passive_EvergreenBreath` — HediffDef: "Wood qi enters with every breath and cleans the body."
- **Convergence.Core** `CVG_Passive_InnerFurnace` — HediffDef: "Coals of fire qi glow in the dantian."
- **Convergence.Core** `CVG_Passive_IronShirt` — HediffDef: "Tempered skin takes blows like an iron shirt."
- **Convergence.Core** `CVG_Passive_LightningBody` — HediffDef: "The body remembers the lightning."
- **Convergence.Core** `CVG_Passive_LightningReflexes` — HediffDef: "Thunder qi runs along the nerves faster than thought."
- **Convergence.Core** `CVG_Passive_MountainBody` — HediffDef: "Earth qi hardens skin and bones like rock."
- **Convergence.Core** `CVG_Passive_PhoenixEmber` — HediffDef: "An ember of the phoenix glows in the Golden Core."
- **Convergence.Core** `CVG_Passive_RootedMountain` — HediffDef: "Roots of earth qi reach deep below the feet."
- **Convergence.Core** `CVG_Passive_SpringRejuvenation` — HediffDef: "Living wood qi knits the flesh without pause."
- **Convergence.Core** `CVG_Passive_StillWaterMind` — HediffDef: "Storms of the heart pass without a ripple."
- **Convergence.Core** `CVG_Passive_StoneMarrow` — HediffDef: "The marrow is dense as river stone."
- **Convergence.Core** `CVG_Passive_StormHeart` — HediffDef: "A still eye of the storm beats in the chest."
- **Convergence.Core** `CVG_Passive_SunHeart` — HediffDef: "A steady sun burns at the center of the chest."
- **Convergence.Core** `CVG_Passive_SwordIntent` — HediffDef: "An invisible blade presses on everyone nearby."
- **Convergence.Core** `CVG_Passive_TenThousandYearPine` — HediffDef: "The body renews itself like an ancient pine."
- **Convergence.Core** `CVG_Passive_TideReading` — HediffDef: "The flow of the fight is as clear as a tide."
- **Convergence.Core** `CVG_Passive_TreeHeart` — HediffDef: "The slow pulse of every living thing beats in the hands."
- **Convergence.Core** `CVG_Passive_TribulationBorn` — HediffDef: "Lightning recognizes its own."
- **Convergence.Core** `CVG_Passive_UnbreakableBones` — HediffDef: "Forged bones bend and come back."
- **Convergence.Core** `CVG_Passive_UnbreakableEdge` — HediffDef: "Metal qi has condensed into the flesh itself."
- **Convergence.Core** `CVG_QiShield` — HediffDef: "A bell of qi wraps the body and swallows blows until it breaks."
- **Convergence.Core** `CVG_SpiritNourished` — HediffDef: "A spirit soup warms the meridians: cultivation and body tempering go a little faster for a day."
- **Convergence.Core** `CVG_SpringRejuvenation` — HediffDef: "Living wood qi flows through the wounds, knitting flesh and slowing the bleeding."
- **Convergence.QiTech** `CVG_QT_Gun_ContainmentTurret` — ThingDef: "The emitter of a qi containment cannon: a magnetic bottle that holds a compressed qi bolt until it is let go."
- **Convergence.QiTech** `CVG_QT_Gun_InductionTurret` — ThingDef: "The emitter of a qi induction turret: three coils that fold the qi of a spirit stone around each bolt."
- **Convergence.Races** `CVG_EvilRepelled` — HediffDef: "The auspicious blood of a qilin weakens this creature of the void."
- **Convergence.Races** `CVG_FoxCharmed` — HediffDef: "Entranced by a fox's glance."
- **Convergence.Races** `CVG_QilinBlessed` — HediffDef: "The calm of the qilin: wounds close faster and the heart is lighter."
- **Convergence.Races** `CVG_XuanwuShell` — HediffDef: "A shell of black stone covers the body: blows slide off, but movement is heavy."
- **Convergence.Resources** `CVG_BloodFruitWarmth` — HediffDef: "The blood fruit fills the body with warm blood qi: wounds close a little faster."
- **Convergence.Resources** `CVG_FlameWarmth` — HediffDef: "The fire of the flame lotus burns inside: the cold barely touches the body."
- **Convergence.Resources** `CVG_GlowSight` — HediffDef: "The glowing mushroom sharpens the eyes in the dark."
- **Convergence.Resources** `CVG_PeachBlessing` — HediffDef: "The essence of a peach of immortality runs through the body: wounds close faster, the breath is long and the q"
- **Convergence.Resources** `CVG_PoisonTempered` — HediffDef: "A small dose of swamp poison trains the body against toxins."

## [BAJA] Objetos con valor 0

- **Convergence.Stories** `CVG_FounderSeal` — MarketValue 0
- **Convergence.Stories** `CVG_HeavenKey` — MarketValue 0
- **Convergence.Stories** `CVG_JiuxiaoShard` — MarketValue 0
- **Convergence.Stories** `CVG_RuptureCore` — MarketValue 0

## [BAJA] Sin descripción

- **Convergence.Resources** `CVG_ChunkBlackJade` — ThingDef
- **Convergence.Resources** `CVG_ChunkCelestialMarble` — ThingDef
- **Convergence.Resources** `CVG_ChunkCloudStone` — ThingDef
- **Convergence.Resources** `CVG_ChunkDragonObsidian` — ThingDef
- **Convergence.Resources** `CVG_ChunkTribulationRock` — ThingDef
- **Convergence.Resources** `CVG_ChunkVermilionStone` — ThingDef

## [BAJA] Sube el qi pero la descripción no lo dice

- **Convergence.FaunaFlora** `CVG_Plant_Lotus` — tiene DefModExtension_FengShui
- **Convergence.Furniture** `CVG_BambooRecliner` — tiene DefModExtension_FengShui
- **Convergence.Furniture** `CVG_BambooWall` — tiene DefModExtension_FengShui
- **Convergence.Furniture** `CVG_BanquetTable` — tiene DefModExtension_FengShui
- **Convergence.Furniture** `CVG_CarvedChair` — tiene DefModExtension_FengShui
- **Convergence.Furniture** `CVG_GrainJar` — tiene DefModExtension_FengShui
- **Convergence.Furniture** `CVG_HealerBed` — tiene DefModExtension_FengShui
- **Convergence.Furniture** `CVG_LacqueredDoor` — tiene DefModExtension_FengShui
- **Convergence.Furniture** `CVG_LotusPlanter` — tiene DefModExtension_FengShui
- **Convergence.Furniture** `CVG_LowTable` — tiene DefModExtension_FengShui
- **Convergence.Furniture** `CVG_MahjongTable` — tiene DefModExtension_FengShui
- **Convergence.Furniture** `CVG_MeditationStool` — tiene DefModExtension_FengShui
- **Convergence.Furniture** `CVG_MoonDoor` — tiene DefModExtension_FengShui
- **Convergence.Furniture** `CVG_PaperLantern` — tiene DefModExtension_FengShui
- **Convergence.Furniture** `CVG_PillCabinet` — tiene DefModExtension_FengShui
- **Convergence.Furniture** `CVG_RobeWardrobe` — tiene DefModExtension_FengShui
- **Convergence.Furniture** `CVG_SectBanner` — tiene DefModExtension_FengShui
- **Convergence.Furniture** `CVG_SectDoubleBed` — tiene DefModExtension_FengShui
- **Convergence.Furniture** `CVG_StoneBench` — tiene DefModExtension_FengShui
- **Convergence.Furniture** `CVG_TeaPavilion` — tiene DefModExtension_FengShui
- **Convergence.Furniture** `CVG_WarehouseShelf` — tiene DefModExtension_FengShui
- **Convergence.Resources** `CVG_BlocksCelestialMarble` — tiene DefModExtension_FengShui
- **Convergence.Resources** `CVG_BlocksCloudStone` — tiene DefModExtension_FengShui
- **Convergence.Resources** `CVG_BlocksDragonObsidian` — tiene DefModExtension_FengShui
- **Convergence.Resources** `CVG_BlocksTribulationRock` — tiene DefModExtension_FengShui
- **Convergence.Resources** `CVG_BlocksVermilionStone` — tiene DefModExtension_FengShui
- **Convergence.Resources** `CVG_BloodJade` — tiene DefModExtension_FengShui
- **Convergence.Resources** `CVG_FireCrystal` — tiene DefModExtension_FengShui
- **Convergence.Resources** `CVG_MoonJade` — tiene DefModExtension_FengShui
- **Convergence.Resources** `CVG_Plant_CelestialPeach` — tiene DefModExtension_FengShui
- **Convergence.Resources** `CVG_Plant_JadeBamboo` — tiene DefModExtension_FengShui
- **Convergence.Resources** `CVG_SolarGoldSand` — tiene DefModExtension_FengShui
