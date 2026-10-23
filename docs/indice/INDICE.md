# Índice del proyecto Convergence

Generado por `tools/make-index.py` (no editar a mano: rehacerlo después de agregar o mover defs o clases).
Buscadores: [DEFS.tsv](DEFS.tsv) (cada def: mod, archivo, nombre EN/ES) y [CODIGO.md](CODIGO.md) (cada clase C#).
Convergence.Sects está descontinuado (lo reemplaza Factions) y no se indexa.

| Mod | Versión | Depende de | Defs | Clases C# | Autotests |
|---|---|---|---|---|---|
| [Convergence.Alchemy](#convergencealchemy) | 0.1.1 | harmony, core, resources | 72 | — | — |
| [Convergence.Apparel](#convergenceapparel) | 0.1.1 | harmony, core, resources | 33 | — | — |
| [Convergence.BiotechWuxia](#convergencebiotechwuxia) | 0.1.1 | harmony, Biotech, core, resources, formations | 69 | — | — |
| [Convergence.Core](#convergencecore) | 0.1.3 | harmony | 401 | 408 | cvganimshot, cvgbandtotemtest, cvgbenchshot, cvgcardshot, cvgcropstest, cvgdevshot, cvgflightrender, cvgflighttest, cvgflightvanilla, cvgfloorshot, cvgflowtest, cvgfurnituretest, cvgislandshot, cvgislandspacetest, cvgislandtest, cvgnightshot, cvgplaytest, cvgqinettest, cvgreach, cvgrendertest, cvgscenarioshot, cvgsectideotest, cvgselftest, cvgshowcase, cvgtabshot, cvgtransreport, cvgworkshopshot, quicktest |
| [Convergence.Factions](#convergencefactions) | 0.1.1 | harmony, core, Royalty, Ideology | 238 | 13 | cvgfactionsshot, cvgfactionstest, cvgfavorstest |
| [Convergence.FaunaFlora](#convergencefaunaflora) | 0.1.1 | harmony, core | 52 | — | — |
| [Convergence.Formations](#convergenceformations) | 0.1.2 | harmony, core | 31 | 15 | cvgformationstest, cvgformflow, cvgturretshot, cvgwallshot |
| [Convergence.Furniture](#convergencefurniture) | 0.1.1 | core | 61 | — | — |
| [Convergence.Grafts](#convergencegrafts) | 0.1.1 | harmony, core | 70 | 5 | cvggraftstest |
| [Convergence.QiTech](#convergenceqitech) | 0.2.2 | harmony, core, formations | 26 | 19 | cvgqitechaimshot, cvgqitechlab, cvgqitechshowcase, cvgqitechtest, cvgqitechwartest, cvgqitechworkshopshot |
| [Convergence.Races](#convergenceraces) | 0.1.1 | harmony, core, Biotech | 198 | — | — |
| [Convergence.Resources](#convergenceresources) | 0.1.1 | harmony, core | 93 | — | — |
| [Convergence.Space](#convergencespace) | 0.1.1 | harmony, Odyssey, core, resources, formations | 18 | — | — |
| [Convergence.Stories](#convergencestories) | 0.1.1 | harmony, core | 178 | 70 | cvgchargetest, cvgmechtabtest, cvgpuppetfighttest, cvgstoriestest, cvgworktest |
| [Convergence.TraitsDestinies](#convergencetraitsdestinies) | 0.1.1 | harmony, core | 61 | — | — |

## Convergence.Alchemy — Convergence: Alchemy & Artifacts

- packageId `prospero1088.convergence.alchemy`, versión 0.1.1
- Depende de: brrainz.harmony, prospero1088.convergence.core, prospero1088.convergence.resources
- Carpetas que carga: /; Common; Mods/Odyssey  (si está Ludeon.RimWorld.Odyssey); Mods/FaunaFlora  (si está prospero1088.convergence.faunaflora)

**Defs** (archivo → tipos y defNames):

- `Common/Defs/DailyConsumables.xml` — ThingDef 9, RecipeDef 7, HediffDef 3: CVG_MealSteamedBuns, CVG_CookSteamedBuns, CVG_CookSteamedBunsBulk, CVG_MealSpiritRice, CVG_CookSpiritRice, CVG_MealSectBanquet, CVG_CookSectBanquet, CVG_BiguPill, CVG_BiguCalm, CVG_MakeBiguPills, CVG_HerbalOintment, CVG_HealingPill, CVG_JadeElixir, CVG_CloudTea, CVG_CloudTeaClarity, CVG_BrewCloudTea, CVG_Incense, CVG_IncenseCalm, CVG_MakeIncense
- `Common/Defs/Herbs.xml` — ThingDef 6: CVG_SpiritHerb, CVG_SpiritGinsengRoot, CVG_SnowLotusPetal, CVG_Plant_SpiritHerb, CVG_Plant_SpiritGinseng, CVG_Plant_SnowLotus
- `Common/Defs/Pills.xml` — ThingDef 4, HediffDef 1: CVG_TemperingPill, CVG_QiRecoveryPill, CVG_PurificationPill, CVG_NineTurnsPill, CVG_PillToxicity
- `Common/Defs/Recipes_StarSand.xml` — RecipeDef 1: CVG_MakeFoundationPill_StarSand
- `Common/Defs/Research_Alchemy.xml` — ResearchProjectDef 4: CVG_Research_SpiritForge2, CVG_Research_SpiritForge3, CVG_Research_AlchemyFurnace2, CVG_Research_AlchemyFurnace3
- `Common/Defs/Talismans.xml` — ThingDef 3, SpecialThingFilterDef 1, RecipeDef 1, HediffDef 1: CVG_SpiritGradedWeapons, CVG_EngraveRunes, CVG_ThunderTalisman, CVG_HealingTalisman, CVG_SwiftTalisman, CVG_SwiftWind
- `Common/Defs/Weapons.xml` — ThingDef 6, StuffCategoryDef 1: CVG_SpiritMetal, CVG_SpiritSword, CVG_SpiritSabre, CVG_SpiritSpear, CVG_FlyingSword, CVG_SpiritBow, CVG_Arrow_Spirit
- `Common/Defs/Weapons_Wuxia.xml` — ThingDef 15: CVG_Jian, CVG_Dao, CVG_Bishou, CVG_Zhanmadao, CVG_Guandao, CVG_Staff, CVG_IronFan, CVG_StraightBow, CVG_Arrow_Straight, CVG_HornBow, CVG_Arrow_Horn, CVG_RepeatingCrossbow, CVG_Bolt_Repeating, CVG_FlyingNeedles, CVG_Needle_Flying
- `Common/Defs/WorkGivers_Alchemy.xml` — WorkGiverDef 2: CVG_DoBillsRuneTable, CVG_DoBillsTalismanTable
- `Common/Defs/Workbenches.xml` — ThingDef 4: CVG_SpiritForge, CVG_AlchemyFurnace, CVG_RuneTable, CVG_TalismanTable
- `Mods/FaunaFlora/Defs/Recipes_FaunaFlora.xml` — RecipeDef 3: CVG_MakeQiRecoveryPill_MoonAntler, CVG_MakePurificationPill_SpiritFungus, CVG_MakeNineTurnsPill_TribulationMushroom

**Parches:** `Common/Patches/Alchemy_Patches.xml`, `Common/Patches/Herbs_Wild.xml`, `Mods/Odyssey/Patches/Herbs_Odyssey.xml`

**Idiomas:** Spanish, SpanishLatin · **Texturas:** 62

**Código en el Core:** Source/Convergence.Core/ → Crafting/ (Crafter, ForgeUtility, PillsAndRunes, WorkbenchLevels), Techniques/SpiritGrade.cs

## Convergence.Apparel — Convergence: Apparel

- packageId `prospero1088.convergence.apparel`, versión 0.1.1
- Depende de: brrainz.harmony, prospero1088.convergence.core, prospero1088.convergence.resources
- Carpetas que carga: /; Common

**Defs** (archivo → tipos y defNames):

- `Common/Defs/Accessories.xml` — ThingDef 11: CVG_JadePendant, CVG_HeartGuardAmulet, CVG_SpaceRing, CVG_QiGatheringRing, CVG_TalismanBelt, CVG_MedicineGourd, CVG_Earrings_Metal, CVG_Earrings_Wood, CVG_Earrings_Water, CVG_Earrings_Fire, CVG_Earrings_Earth
- `Common/Defs/Apparel.xml` — ThingDef 9: CVG_Apparel_RobeOuter, CVG_Apparel_RobeInner, CVG_Apparel_RobeElder, CVG_Apparel_Hanfu, CVG_Apparel_BambooHat, CVG_Apparel_JadeCrown, CVG_Apparel_ScaleArmor, CVG_Apparel_ScaleHelmet, CVG_Apparel_RuneBattleRobe
- `Common/Defs/ApparelLayers.xml` — ApparelLayerDef 4: CVG_Neck, CVG_Finger, CVG_Ear, CVG_Waist
- `Common/Defs/Apparel_Daily.xml` — ThingDef 3: CVG_Apparel_Trousers, CVG_Apparel_PaddedRobe, CVG_Apparel_FurHood
- `Common/Defs/Textiles.xml` — ThingDef 5, RecipeDef 1: CVG_CloudCotton, CVG_Plant_CloudCotton, CVG_CloudCloth, CVG_SpiritSilk, CVG_SpiritLoom, CVG_WeaveCloudCloth

**Parches:** `Patches/SectRobes.xml`, `Patches/SectRobes_Factions.xml`

**Idiomas:** Spanish, SpanishLatin · **Texturas:** 150

## Convergence.BiotechWuxia — Convergence: Biotech Wuxia

- packageId `prospero1088.convergence.biotechwuxia`, versión 0.1.1
- Depende de: brrainz.harmony, Ludeon.RimWorld.Biotech, prospero1088.convergence.core, prospero1088.convergence.resources, prospero1088.convergence.formations
- Carpetas que carga: /; Common; Mods/Apparel  (si está prospero1088.convergence.apparel); Mods/Anomaly  (si está Ludeon.RimWorld.Anomaly)

**Defs** (archivo → tipos y defNames):

- `Common/Defs/BloodDemon.xml` — AbilityDef 2, ThingDef 2, GeneDef 1, XenotypeDef 1, HediffDef 1, RecipeDef 1: CVG_BloodDemonArts, CVG_Xeno_BloodDemon, CVG_Tech_BloodDrain, CVG_Tech_BloodArmor, CVG_BloodArmor, CVG_BloodPool, CVG_BloodEssencePill, CVG_Make_BloodEssencePill
- `Common/Defs/BloodDemonDevices.xml` — ThingDef 3: CVG_BloodLotusBasin, CVG_BloodMoonCenser, CVG_BloodDrumOfReturn
- `Common/Defs/PuppetAnimation.xml` — AnimationDef 1: CVG_PuppetSway
- `Common/Defs/PuppetGear.xml` — ThingDef 3, ResearchProjectDef 1: CVG_Research_PuppetMasters, CVG_Apparel_PuppeteerMask, CVG_Apparel_ThousandThreadsCrown, CVG_ThreadTotem
- `Common/Defs/PuppetWeapons.xml` — ThingDef 4: CVG_PuppetGun_RepeatingCrossbow, CVG_Bolt_PuppetCrossbow, CVG_PuppetGun_JadeNeedle, CVG_Needle_PuppetJade
- `Common/Defs/Puppets.xml` — ThingDef 8, HediffDef 6, ResearchProjectDef 4, PawnKindDef 3, RecipeDef 3: CVG_PuppetHeartThread, CVG_PuppetHeartThread, CVG_SpiritThread, CVG_PuppetBlueprint, CVG_Research_Puppetry, CVG_Research_WarThreads, CVG_Research_SteelThreads, CVG_Research_JadeThreads, CVG_PuppetMaterial_Wood, CVG_PuppetMaterial_Iron, CVG_PuppetMaterial_Steel, CVG_PuppetBond, CVG_PuppetBacklash, CVG_Puppet_Wood, CVG_Puppet_Wood, CVG_Puppet_Bronze, CVG_Puppet_Bronze, CVG_Puppet_Jade, CVG_Puppet_Jade, CVG_BuildPuppet_Wood, CVG_BuildPuppet_Bronze, CVG_BuildPuppet_Jade, CVG_PuppetWorkshop, CVG_SpiritCharger
- `Common/Defs/PuppetsWork.xml` — ThingDef 3, PawnKindDef 3, RecipeDef 3: CVG_Puppet_Miner, CVG_Puppet_Miner, CVG_Puppet_Mason, CVG_Puppet_Mason, CVG_Puppet_Blade, CVG_Puppet_Blade, CVG_BuildPuppet_Miner, CVG_BuildPuppet_Mason, CVG_BuildPuppet_Blade
- `Mods/Anomaly/Defs/Jiangshi.xml` — ThingDef 4, HediffDef 3, RecipeDef 3, DamageDef 1, MutantDef 1, PawnKindDef 1, IncidentDef 1: CVG_YangDrain, CVG_CorpseQi, CVG_Jiangshi, CVG_Jiangshi, CVG_Jiangshi, CVG_BaguaDazed, CVG_PeachWood, CVG_MakePeachWood, CVG_PeachWoodSword, CVG_BaguaMirror, CVG_GlutinousRicePoultice, CVG_MakeGlutinousRicePoultice, CVG_DrawCorpseQi, CVG_JiangshiNight
- `Mods/Apparel/Defs/PuppetRobe.xml` — ThingDef 1: CVG_Apparel_GrandPuppeteerRobe
- `Mods/Apparel/Defs/SpiritThread_Silk.xml` — RecipeDef 1: CVG_SpinSpiritThreadFromSilk

**Parches:** `Common/Patches/Biotech_Wuxia.xml`, `Mods/Anomaly/Patches/Jiangshi_Shambler.xml`

**Idiomas:** Spanish, SpanishLatin · **Texturas:** 60

**Código en el Core:** Source/Convergence.Core/ → Biotech/ (marionetas, demonio de sangre, jiangshi, tótem)

## Convergence.Core — Convergence: Era of Cultivation

- packageId `prospero1088.convergence.core`, versión 0.1.3
- Depende de: brrainz.harmony
- Carpetas que carga: /; Common; 1.6; Mods/Biotech  (si está Ludeon.RimWorld.Biotech); Mods/Odyssey  (si está Ludeon.RimWorld.Odyssey); Mods/DubsBadHygiene  (si está Dubwise.DubsBadHygiene)

**Defs** (archivo → tipos y defNames):

- `Common/Defs/Art_Wall.xml` — ThingDef 2, WorkGiverDef 1: CVG_CalligraphyScroll, CVG_InkPainting, CVG_DoBillsCalligraphy
- `Common/Defs/Backstories_Cultivation.xml` — BackstoryDef 112: CVG_Backstory_LakeFisherChild, CVG_Backstory_PaddyChild, CVG_Backstory_MuleDriverChild, CVG_Backstory_TeaHouseChild, CVG_Backstory_LateRootVillager, CVG_Backstory_SectServantChild, CVG_Backstory_StoneVeinChild, CVG_Backstory_PillHallRunner, CVG_Backstory_InnChild, CVG_Backstory_RangeGoatherd, CVG_Backstory_BellowsChild, CVG_Backstory_PeddlersChild, CVG_Backstory_NightMarketChild, CVG_Backstory_HerbTerraceChild, CVG_Backstory_DreamingFarmChild, CVG_Backstory_TalismanFolder, CVG_Backstory_WreckFieldScavenger, CVG_Backstory_SutraCopyist, CVG_Backstory_HermitFoundling, CVG_Backstory_AlchemistChild, CVG_Backstory_IronDayChild, CVG_Backstory_SwordCemeterySweeper, CVG_Backstory_RiftCrossedChild, CVG_Backstory_RiftScrapScholar, CVG_Backstory_ImperialPage, CVG_Backstory_TempleGateChild, CVG_Backstory_ClinicChild, CVG_Backstory_SkyIslandDeckhand, CVG_Backstory_WanderersPorter, CVG_Backstory_SpiritMarkedChild, CVG_Backstory_FallenHouseHeir, CVG_Backstory_BeastRaisedChild, CVG_Backstory_CultCaptiveChild, CVG_Backstory_BloodServantChild, CVG_Backstory_LabRaisedSubject, CVG_Backstory_YoungMaster, CVG_Backstory_SilentVowChild, CVG_Backstory_ExpelledProdigy, CVG_Backstory_CaveBornChild, CVG_Backstory_BeastTideOrphan … (+72)
- `Common/Defs/BloodlinePurities.xml` — BloodlinePurityDef 4: CVG_Purity_Trace, CVG_Purity_Diluted, CVG_Purity_Pure, CVG_Purity_Ancestral
- `Common/Defs/Body_Path.xml` — ThingDef 2, CultivationPathDef 1, HediffDef 1, WorkTypeDef 1, WorkGiverDef 1, JobDef 1: CVG_Path_Body, CVG_BloodBackflow, CVG_BodyTempering, CVG_TemperBody, CVG_TemperBody, CVG_WoodenDummy, CVG_StoneWeights
- `Common/Defs/Buildings_Cultivation.xml` — ThingDef 3: CVG_CultivationCushion, CVG_CultivationSpot, CVG_MeditationPlatform
- `Common/Defs/CelestialCalendar.xml` — GameConditionDef 3, IncidentDef 3: CVG_QiTide, CVG_QiEbb, CVG_InnerDemonMoon, CVG_QiTide, CVG_QiEbb, CVG_InnerDemonMoon
- `Common/Defs/Concepts_Cultivation.xml` — ConceptDef 4: CVG_Concept_Cultivation, CVG_Concept_Techniques, CVG_Concept_Breakthrough, CVG_Concept_QiNetwork
- `Common/Defs/CultivationDifficulty.xml` — CultivationDifficultyDef 3: CVG_Difficulty_Easy, CVG_Difficulty_Hard, CVG_Difficulty_Nightmare
- `Common/Defs/CultivationLife.xml` — ThoughtDef 8, RecordDef 4, TaleDef 3: CVG_Thought_BreakthroughSuccess, CVG_Thought_PartnerAscended, CVG_Thought_KinAscended, CVG_Thought_RivalAscended, CVG_Thought_BreakthroughFailed, CVG_Thought_SurvivedTribulation, CVG_Thought_WitnessedTribulation, CVG_Thought_QiDeviation, CVG_TimeCultivating, CVG_Breakthroughs, CVG_TribulationsSurvived, CVG_TechniquesCast, CVG_Tale_Breakthrough, CVG_Tale_SurvivedTribulation, CVG_Tale_DiedInTribulation
- `Common/Defs/CultivationWealth.xml` — CultivationWealthDef 1: CVG_CultivationWealth
- `Common/Defs/DesignationCategories_Cultivation.xml` — DesignationCategoryDef 1: CVG_Cultivation
- `Common/Defs/Hediffs_Realms.xml` — HediffDef 3: CVG_Hediff_QiRefining, CVG_Hediff_Foundation, CVG_Hediff_GoldenCore
- `Common/Defs/HistoryEvents_Sect.xml` — HistoryEventDef 7: CVG_Cultivated, CVG_Ascended, CVG_RetreatCompleted, CVG_TookPill, CVG_DemonicArt, CVG_ExtractedEssence, CVG_DuelFought
- `Common/Defs/Incidents_Convergence.xml` — IncidentDef 1: CVG_Convergence
- `Common/Defs/Items_Breakthrough.xml` — HediffDef 2, ThingDef 1: CVG_FoundationPill, CVG_FoundationPillAbsorbed, CVG_QiDeviation
- `Common/Defs/Leather_Spirit.xml` — ThingDef 1: CVG_SpiritLeather
- `Common/Defs/MentalStates_InnerDemon.xml` — MentalBreakDef 1, MentalStateDef 1: CVG_InnerDemon, CVG_InnerDemon
- `Common/Defs/Mentors.xml` — HediffDef 2, MentorDef 1, ThingDef 1: CVG_Mentor_YunShouzhi, CVG_MentorPresence, CVG_MentorProtection, CVG_JadeFragment_Leaf
- `Common/Defs/Paths_Cultivation.xml` — CultivationPathDef 1: CVG_Path_Qi
- `Common/Defs/PreCultivation.xml` — PreCultivationDef 2: CVG_PreCultivation_Qi, CVG_PreCultivation_Body
- `Common/Defs/QiElements.xml` — QiElementDef 6: CVG_Element_Metal, CVG_Element_Wood, CVG_Element_Water, CVG_Element_Fire, CVG_Element_Earth, CVG_Element_Thunder
- `Common/Defs/QiNetwork.xml` — JobDef 1, TerrainDef 1: CVG_InfuseQi, CVG_QiVein
- `Common/Defs/Realms.xml` — CultivationRealmDef 3: CVG_Realm_QiRefining, CVG_Realm_Foundation, CVG_Realm_GoldenCore
- `Common/Defs/Realms_Body.xml` — CultivationRealmDef 6, HediffDef 6: CVG_Realm_SkinTempering, CVG_Realm_FleshTempering, CVG_Realm_BoneForging, CVG_Realm_MarrowCleansing, CVG_Realm_BloodRefining, CVG_Realm_GoldenBody, CVG_Hediff_SkinTempering, CVG_Hediff_FleshTempering, CVG_Hediff_BoneForging, CVG_Hediff_MarrowCleansing, CVG_Hediff_BloodRefining, CVG_Hediff_GoldenBody
- `Common/Defs/Recreation.xml` — ThingDef 2, JobDef 2, JoyGiverDef 2: CVG_GoBoard, CVG_CalligraphyDesk, CVG_Play_Go, CVG_PracticeCalligraphy, CVG_Play_Go, CVG_PracticeCalligraphy
- `Common/Defs/Research_Convergence.xml` — ResearchTabDef 1, ResearchProjectDef 1: CVG_Cultivation, CVG_SpiritualAnomalyStudy
- `Common/Defs/Research_Dao.xml` — ResearchProjectDef 6, KnowledgeCategoryDef 1, ThingDef 1: CVG_DaoComprehension, CVG_Research_QiBreathing, CVG_Research_Foundation, CVG_Research_GoldenCore, CVG_Research_SecretRealms, CVG_Research_TechniqueManuals, CVG_Research_BodyTempering, CVG_ContemplationDesk
- `Common/Defs/Resources_Spirit.xml` — ThingDef 5, RecipeDef 1: CVG_SpiritStone, CVG_SpiritJade, CVG_MineableSpiritStone, CVG_MineableSpiritJade, CVG_PrimordialCrystal, CVG_MakePrimordialCrystal
- `Common/Defs/RoomStats_Cultivation.xml` — RoomStatDef 1: CVG_QiConcentration
- `Common/Defs/RulePacks_WuxiaArt.xml` — RulePackDef 3: CVG_NamerArtWuxia, CVG_ArtDescription_JadeSculpture, CVG_ArtDescription_Scroll
- `Common/Defs/SoundCues.xml` — SoundCueDef 12: CVG_Cue_Convergence, CVG_Cue_BreakthroughStart, CVG_Cue_BreakthroughSuccess, CVG_Cue_BreakthroughFail, CVG_Cue_StageReached, CVG_Cue_TribulationWarning, CVG_Cue_BloodlinePurified, CVG_Cue_MentorArrival, CVG_Cue_MentorFarewell, CVG_Cue_MentorDeath, CVG_Cue_ManualLearned, CVG_Cue_RankUp
- `Common/Defs/SpiritBeasts.xml` — ThingDef 4, HediffDef 2, ThingCategoryDef 1, RecipeDef 1: CVG_SpiritBeast, CVG_SpiritBeastProducts, CVG_SpiritMeat, CVG_BeastBlood, CVG_BeastCore, CVG_SpiritSoup, CVG_CookSpiritSoup, CVG_SpiritNourished
- `Common/Defs/SpiritRootGrades.xml` — SpiritRootGradeDef 4: CVG_RootGrade_Inferior, CVG_RootGrade_Common, CVG_RootGrade_Superior, CVG_RootGrade_Heavenly
- `Common/Defs/Stats_Cultivation.xml` — StatDef 12, StatCategoryDef 1: CVG_Cultivation, CVG_QiMax, CVG_QiRegen, CVG_CultivationSpeed, CVG_BreakthroughChance, CVG_CelestialDissonance, CVG_TechniqueCost, CVG_SpiritGrade, CVG_BloodQiMax, CVG_BloodQiRegen, CVG_BodyTemperingSpeed, CVG_PurificationChance, CVG_EnvironmentResistance
- `Common/Defs/Stats_Forge.xml` — StatDef 2: CVG_ForgeSpeed, CVG_ForgeQuality
- `Common/Defs/Stats_Sects.xml` — StatDef 2: CVG_ProjectileDeflectChance, CVG_ProjectileResistance
- `Common/Defs/Techniques.xml` — AbilityDef 15, TechniqueMasteryDef 5, TechniqueManualDef 5, AbilityCategoryDef 1: CVG_Technique, CVG_Mastery_Novice, CVG_Mastery_Adept, CVG_Mastery_Expert, CVG_Mastery_Master, CVG_Mastery_Peerless, CVG_Manual_GoldenEdge, CVG_Manual_EvergreenSutra, CVG_Manual_FlowingCloud, CVG_Manual_BlazingSun, CVG_Manual_MountainHeart, CVG_Tech_MetalNeedle, CVG_Tech_QiShield, CVG_Tech_SwordQiSweep, CVG_Tech_ThornBolt, CVG_Tech_SpringRejuvenation, CVG_Tech_ThornStorm, CVG_Tech_FrostShard, CVG_Tech_SwiftStep, CVG_Tech_FrostBreath, CVG_Tech_Fireball, CVG_Tech_BlazingMeridians, CVG_Tech_SeaOfFlames, CVG_Tech_StoneBullet, CVG_Tech_MountainBody, CVG_Tech_EarthTremor
- `Common/Defs/Techniques_Passive.xml` — HediffDef 15, AbilityDef 12: CVG_Tech_GoldenBellSkin, CVG_Passive_GoldenBellSkin, CVG_Tech_EdgeOfTheHeart, CVG_Passive_EdgeOfTheHeart, CVG_Tech_SwordIntent, CVG_Passive_SwordIntent, CVG_Tech_EvergreenBreath, CVG_Passive_EvergreenBreath, CVG_Tech_TreeHeart, CVG_Passive_TreeHeart, CVG_Tech_CloudSteps, CVG_Passive_CloudSteps, CVG_Tech_StillWaterMind, CVG_Passive_StillWaterMind, CVG_Tech_TideReading, CVG_Passive_TideReading, CVG_Tech_InnerFurnace, CVG_Passive_InnerFurnace, CVG_Tech_SunHeart, CVG_Passive_SunHeart, CVG_Tech_StoneMarrow, CVG_Passive_StoneMarrow, CVG_Tech_RootedMountain, CVG_Passive_RootedMountain, CVG_Passive_SpringRejuvenation, CVG_Passive_BlazingMeridians, CVG_Passive_MountainBody
- `Common/Defs/Techniques_Paths.xml` — AbilityDef 10, HediffDef 9, TechniqueManualDef 3, ThingDef 1: CVG_Manual_SwordHeart, CVG_Manual_SpiritSense, CVG_Manual_VajraBody, CVG_Projectile_FlyingSword, CVG_Tech_FlyingSword, CVG_Tech_UnbreakableEdge, CVG_Passive_UnbreakableEdge, CVG_Tech_TenThousandYearPine, CVG_Passive_TenThousandYearPine, CVG_Tech_BoundlessSea, CVG_Passive_BoundlessSea, CVG_Tech_PhoenixEmber, CVG_Passive_PhoenixEmber, CVG_Tech_EndlessMountain, CVG_Passive_EndlessMountain, CVG_Tech_DivineSense, CVG_Passive_DivineSense, CVG_Tech_IronShirt, CVG_Passive_IronShirt, CVG_Tech_DragonElephantMight, CVG_Passive_DragonElephantMight, CVG_Tech_UnbreakableBones, CVG_Passive_UnbreakableBones
- `Common/Defs/Techniques_Things.xml` — ThingDef 6, HediffDef 4: CVG_Projectile_MetalNeedle, CVG_Projectile_Thorn, CVG_Projectile_FrostShard, CVG_Projectile_Fireball, CVG_Projectile_StoneBullet, CVG_QiShield, CVG_SpringRejuvenation, CVG_BlazingMeridians, CVG_MountainBody, CVG_JadeSlip
- `Common/Defs/Techniques_Thunder.xml` — AbilityDef 7, HediffDef 4, TechniqueManualDef 1: CVG_Manual_NineHeavensThunder, CVG_Tech_LightningStep, CVG_Tech_ChainLightning, CVG_Tech_ThunderRoar, CVG_Tech_LightningReflexes, CVG_Passive_LightningReflexes, CVG_Tech_StormHeart, CVG_Passive_StormHeart, CVG_Tech_LightningBody, CVG_Passive_LightningBody, CVG_Tech_TribulationBorn, CVG_Passive_TribulationBorn
- `Common/Defs/Traits_Cultivation.xml` — TraitDef 13: CVG_SpiritRoot, CVG_Body_PureYin, CVG_Body_PureYang, CVG_Body_NineYang, CVG_Body_CelestialSword, CVG_Body_Immortal, CVG_Body_Chaos, CVG_Body_Venom, CVG_Body_InnateSpirit, CVG_Body_Steel, CVG_Body_Tribulation, CVG_Body_Sword, CVG_Body_BloodMoon
- `Common/Defs/WeaponClasses_Sects.xml` — WeaponClassDef 1: CVG_Sword
- `Common/Defs/Work_Cultivation.xml` — JobDef 2, WorkTypeDef 1, WorkGiverDef 1: CVG_Cultivation, CVG_Cultivate, CVG_Cultivate, CVG_Breakthrough
- `Mods/Biotech/Defs/Genes_SpiritRoot.xml` — GeneDef 10, GeneCategoryDef 1: CVG_Cultivation, CVG_Gene_RootGrade_Inferior, CVG_Gene_RootGrade_Common, CVG_Gene_RootGrade_Superior, CVG_Gene_RootGrade_Heavenly, CVG_Gene_RootElement_Metal, CVG_Gene_RootElement_Wood, CVG_Gene_RootElement_Water, CVG_Gene_RootElement_Fire, CVG_Gene_RootElement_Earth, CVG_Gene_RootElement_Thunder
- `Mods/Biotech/Defs/Traits_Puppeteer.xml` — TraitDef 1: CVG_Body_ThousandThreads

**Parches:** `Common/Patches/Backstories_Tiers.xml`, `Common/Patches/Forges.xml`, `Common/Patches/SwordTag.xml`, `Common/Patches/ThinkTree_InnerDemon.xml`, `Mods/DubsBadHygiene/Patches/Bigu.xml`, `Mods/Odyssey/Patches/Vacuum.xml`

**Idiomas:** English, Spanish, SpanishLatin · **Texturas:** 111

**Código** (`Source/Convergence.Core`):

- `Abilities/AbilityComps.cs`: AbilityPower, CompProperties_AbilityQiCost, CompProperties_AbilityCone, CompAbilityEffect_Cone, CompProperties_AbilityGiveHediffInRadius, CompAbilityEffect_GiveHediffInRadius
- `Biotech/BandTotem.cs`: DefModExtension_BandNodeSubstitute, Patch_CompPowerTrader_PowerOn, Patch_HediffBandNode_RecacheBandNodes, GameComponent_BandTotemTest
- `Biotech/BloodDemon.cs`: DefModExtension_StuffWeakness, StuffWeaknessUtility, CompProperties_AbilityBloodDrain, CompAbilityEffect_BloodDrain, DefModExtension_TraitGrantsHediff, TraitGrantsHediffUtility
- `Biotech/Jiangshi.cs`: DamageWorker_YangDrain, HediffCompProperties_SunBurn, HediffComp_SunBurn, CompProperties_BaguaMirror, CompBaguaMirror, PlaceWorker_BaguaMirror, DefModExtension_JiangshiIncident, IncidentWorker_JiangshiNight
- `Biotech/MechTabButton.cs`: Patch_MainButtonWorker_ToggleMechTab_Disabled
- `Biotech/PuppetInspectTexts.cs`: PuppetInspectTexts, Patch_CompOverseerSubject_InspectString, Patch_Pawn_InspectString_Puppet
- `Biotech/PuppetTiers.cs`: DefModExtension_PuppetTier, PuppetPower, PuppetTierUtility, StatPart_PuppetPower, Patch_Projectile_PuppetDamage, Patch_Projectile_PuppetPenetration, Patch_Bill_Mech_PuppetRealm, HediffCompProperties_ColdBrittle, HediffComp_ColdBrittle
- `Biotech/Puppetry.cs`: DefModExtension_MechanitorLink, PuppetUtility, Patch_MechanitorUtility_ShouldBeMechanitor, Hediff_PuppetLink, StatPart_PuppetBandwidth, CompProperties_Puppet, CompPuppet, Patch_Building_MechCharger_IsPowered, Patch_Building_MechCharger_Tick, Patch_Building_MechCharger_CanPawnChargeCurrently
- `Bloodline/BloodlineSelfTest.cs`: BloodlineSelfTest
- `Bloodline/BloodlineText.cs`: BloodlineText, Patch_GeneDef_DescriptionFull
- `Bloodline/BloodlineUtility.cs`: BloodlineInfo, BloodlineUtility
- `Bloodline/Gene_Bloodline.cs`: Gene_Bloodline, BloodlineMechanics, Patch_Pawn_Kill, Patch_Pawn_PostApplyDamage
- `Body/BodyTempering.cs`: IngestionOutcomeDoer_GainCultivationResource, DefModExtension_BodyTrainingSpot, WorkGiver_TemperBody, JobDriver_TemperBody
- `Body/SpiritBeast.cs`: DefModExtension_SpiritBeast, Hediff_SpiritBeast, SpiritBeastUtility, Patch_Pawn_ButcherProducts
- `Core/BackstoryTiers.cs`: DefModExtension_BackstoryStats, StatPart_Backstory, BackstoryTiers, Patch_CharacterCard_TierColors, Patch_BackstoryTitle_TierColor, Patch_BackstoryDescription_Stats
- `Core/ConvergenceMod.cs`: ConvergenceMod
- `Core/ConvergenceSettings.cs`: WorldMode, ConvergenceTrigger, ConvergenceSettings
- `Core/ConvergenceStartup.cs`: ConvergenceStartup
- `Core/CultivationDifficulty.cs`: CultivationDifficultyDef, CultivationDifficultyValues, CultivationDifficulty, StatPart_CultivationDifficulty, Patch_IncidentWorker_BaseChanceThisGame, Dialog_CultivationStart
- `Core/CultivationLife.cs`: CultivationLife, RecordWorker_TimeCultivating, Alert_CVG_ReadyToBreakthrough, Alert_CVG_QiDeviation, Alert_CVG_NoCultivationSpot, Alert_CVG_IdleCultivator
- `Core/GameComponent_Convergence.cs`: GameComponent_Convergence
- `Core/LegendaryBackstories.cs`: LegendaryBackstories
- `Core/NoBuildingShadows.cs`: Patch_GraphicShadow_Print, Patch_GraphicShadow_DrawWorker, NoEdgeShadowsStartup
- `Core/NoStuffTint.cs`: DefModExtension_NoStuffTint, Patch_Building_DrawColor_NoStuffTint
- `Core/PostConvergence.cs`: DefModExtension_PostConvergence, Patch_BiomeDef_CommonalityOfAnimal, DefModExtension_TribulationSprout, TribulationSproutUtility, Patch_FishingUtility_GetCatchesFor
- `Core/UnityCompat.cs`: UnityCompat
- `Crafting/Crafter.cs`: CompProperties_Crafter, CompCrafter, CrafterInjection
- `Crafting/ForgeUtility.cs`: DefModExtension_Forge, ForgeUtility, StatPart_Forge, Patch_QualityUtility_Forge
- `Crafting/PillsAndRunes.cs`: PillQuality, IngestionOutcomeDoer_GiveHediffByQuality, IngestionOutcomeDoer_GainResourceByQuality, IngestionOutcomeDoer_PurifyBloodline, RecipeWorker_EngraveRunes, SpecialThingFilterWorker_SpiritGraded, CompProperties_UseEffectSpiritBurst, CompUseEffect_SpiritBurst, SpiritGradeInjection
- `Crafting/WorkbenchLevels.cs`: WorkbenchLevel, CompProperties_WorkbenchLevel, CompWorkbenchLevel, Building_LeveledWorkTable, DefModExtension_RequiresWorkbenchLevel, WorkbenchLevelUtility, Patch_RecipeDef_AvailableOnNow_Level, WorkbenchLevelStartup, StatPart_WorkbenchLevel, Patch_QualityUtility_WorkbenchLevel
- `Cultivation/BreakthroughUtility.cs`: BreakthroughUtility
- `Cultivation/CompCultivator.cs`: CompCultivator, Patch_Pawn_AgeTracker_BiologicalTicksPerTick, Patch_Pawn_HealthScale, CompProperties_Cultivator
- `Cultivation/CompPlantSpread.cs`: CompProperties_PlantSpread, CompPlantSpread
- `Cultivation/CultivationGenerator.cs`: CultivationGenerator, Patch_PawnGenerator_GeneratePawn
- `Cultivation/CultivationText.cs`: CultivationText
- `Cultivation/CultivatorPawnKind.cs`: DefModExtension_CultivatorPawnKind, TraitChance, DefModExtension_ConvergenceFaction, CultivatorPawnKindUtility, ConvergenceFactionUtility
- `Cultivation/ElementalResonance.cs`: DefModExtension_ElementalResonance, StatPart_ElementalResonance
- `Cultivation/EnvironmentResistance.cs`: StatPart_EnvironmentResistance
- `Cultivation/FactionRanks.cs`: FactionRankDef, FactionRankUtility, GameComponent_FactionRanks
- `Cultivation/FruitTree.cs`: DefModExtension_WoodOnCut, Plant_FruitTree
- `Cultivation/Hediff_CultivationRealm.cs`: Hediff_CultivationRealm
- `Cultivation/PreCultivation.cs`: PreCultivationDef, PreCultivationOption, PreCultivation
- `Cultivation/QiConcentration.cs`: QiConcentrationUtility, RoomStatWorker_QiConcentration
- `Cultivation/RealmBonusUtility.cs`: RealmBonusUtility
- `Cultivation/RealmTracker.cs`: RealmTracker
- `Cultivation/SpiritRoot.cs`: SpiritRoot
- `Cultivation/StatWorker_Cultivation.cs`: StatWorker_Cultivation
- `Cultivation/WaterGrowing.cs`: Patch_ZoneAddGrowing_ShallowWater
- `Debug/AnimationShot.cs`: AnimationShot
- `Debug/ArtShots.cs`: ArtShots
- `Debug/CoreSelfTest.cs`: CoreSelfTest
- `Debug/CropsDefenseSelfTest.cs`: GameComponent_CropsDefenseTest
- `Debug/DebugActions_Convergence.cs`: DebugActions_Convergence
- `Debug/DebugActions_Relations.cs`: DebugActions_Relations
- `Debug/DevelopmentShot.cs`: DevelopmentShot
- `Debug/FloorShot.cs`: FloorShot
- `Debug/FlowTest.cs`: GameComponent_FlowTest
- `Debug/FurnitureSelfTest.cs`: GameComponent_FurnitureTest
- `Debug/PlayTest.cs`: GameComponent_PlayTest, PlayTestProbe, ErrorEntry
- `Debug/ReachabilityAudit.cs`: ReachabilityAudit
- `Debug/RenderTest.cs`: RenderTest
- `Debug/ScenarioShot.cs`: ScenarioShot
- `Debug/Showcase.cs`: Showcase
- `Debug/TabShot.cs`: TabShot
- `Debug/TranslationReportShot.cs`: TranslationReportShot
- `Debug/WorkshopShot.cs`: WorkshopShotMap, WorkshopShot
- `Defs/BloodlineDefs.cs`: BloodlinePurityDef, BloodlineDef, BloodlinePurity, DefModExtension_Bloodline
- `Defs/BloodlineGeneDefs.cs`: BloodlineGeneGroupDef, DefModExtension_BloodlineGeneTier, DefModExtension_BloodRegeneration, DefModExtension_ExtraLives, DefModExtension_GeneAura, DefModExtension_Retaliation
- `Defs/CopyRecipes.cs`: DefModExtension_CopyRecipes, CopyRecipesStartup
- `Defs/CultivationDefs.cs`: QiElementDef, SpiritRootGradeDef, DefModExtension_SpiritRoot, DefModExtension_SpecialBody, CultivationRealmDef, DefModExtension_FengShui, DefModExtension_BlocksCultivation, DefModExtension_CultivationSpot, DefModExtension_BreakthroughAid, DefModExtension_QiConcentration, CVG_DefOf
- `Defs/CultivationPathDefs.cs`: CultivationPathDef, CultivationPathWorker, CultivationPathWorker_Body, CultivationResource, StatResource, BreakthroughRequirement, BreakthroughRequirement_Stat, BreakthroughRequirement_Realm
- `Defs/SoundCueDef.cs`: SoundCueDef, SoundCues
- `Economy/CultivationWealth.cs`: CultivationWealthDef, DefModExtension_CultivationWealth, DefModExtension_CultivationThreat, CultivationWealthRecord, CultivationWealth, Patch_WealthWatcher_ForceRecount, Patch_IncidentWorker_Raid_AdjustedRaidPoints, Patch_IncidentWorker_AggressiveAnimals
- `Events/AuraPressure.cs`: AuraPressure, Patch_IncidentWorker_Raid_AuraPressure
- `Events/CelestialCalendar.cs`: DefModExtension_CelestialCondition, IncidentWorker_CelestialCondition, StatPart_CelestialCondition, CelestialCalendar
- `Events/IncidentWorker_Convergence.cs`: IncidentWorker_Convergence, ConvergenceUtility, ResearchMod_TriggerConvergence
- `Events/InnerDemon.cs`: MentalBreakWorker_InnerDemon, DefModExtension_InnerDemon, MentalState_InnerDemon, CompProperties_DaoStudy, CompDaoStudy
- `Ideology/SectEvents.cs`: SectEvents, DefModExtension_DemonicArt, Patch_Ability_Activate_SectEvents, Patch_Thing_Ingested_SectEvents
- `Ideology/SectIdeoSelfTest.cs`: GameComponent_SectIdeoTest
- `Ideology/SectRituals.cs`: RitualObligationTrigger_AdmissionProperties, RitualObligationTrigger_AscensionProperties, RitualObligationTrigger_Admission, RitualObligationTrigger_Ascension, Patch_RitualOutcome_Tournament
- `Jobs/JobDriver_Breakthrough.cs`: JobDriver_Breakthrough
- `Jobs/JobDriver_Cultivate.cs`: JobDriver_Cultivate
- `Jobs/WorkGiver_Cultivate.cs`: WorkGiver_Cultivate
- `Mentor/GameComponent_Mentor.cs`: GameComponent_Mentor, MentorUtility
- `Mentor/MentorDefs.cs`: MentorDef, MentorLesson, MentorLesson_BuildThing, MentorLesson_ReachStage, MentorLesson_LearnTechnique
- `Patches/Patch_GuestLimit.cs`: GuestLimit, Patch_IncidentWorker_CanFireNow_GuestLimit
- `QiNetwork/QiNetwork.cs`: CompProperties_QiNode, QiNet, CompQiNode, CompQiConsumer, QiNetworkManager, QiNetworkUtility, PlaceWorker_OnQiVein, PlaceWorker_QiNodeRadius, JobDriver_InfuseQi, Patch_CompPowerTrader_PowerOn_Qi, Patch_CompPower_Inspect_Qi, Patch_CompPowerTrader_Inspect_Qi
- `QiNetwork/QiNetworkSelfTest.cs`: GameComponent_QiNetTest
- `QiNetwork/QiSources.cs`: QiSourceUtility, CompProperties_QiLightningRod, CompQiLightningRod, Patch_LightningStrike_QiRod, MapComponent_QiOverlay, Patch_PlaySettings, Building_FrostFormation
- `Research/DaoComprehension.cs`: DefModExtension_DaoResearch, BreakthroughRequirement_Research, DaoSource, DaoComprehension, GameComponent_Dao, DaoContribution, Patch_EntityCodex_Hidden
- `Resources/ConvergenceMineables.cs`: DefModExtension_ConvergenceMineable, ConvergenceMineables, Patch_GenStep_ScatterLumpsMineable_ChooseThingDef
- `Resources/ResourceSelfTest.cs`: ResourceSelfTest
- `Resources/WuxiaRocks.cs`: DefModExtension_WuxiaRock, Patch_World_NaturalRockTypesIn
- `Sects/Constitutions.cs`: ConstitutionUtility, RoleRequirement_HasTrait, DefModExtension_StartingTrait, GameComponent_StartingTraits
- `Sects/Patch_SectBackstories.cs`: Patch_PawnBio_SectBackstories
- `Sects/ProjectileDefense.cs`: ProjectileDefense, StatPart_RequiresMeleeWeapon
- `Sects/SectIdeology.cs`: RitualOutcomeComp_MinBelievers, DefModExtension_RoleEmptyMinBelievers, Patch_Thought_IdeoRoleEmpty_MinBelievers, GameComponent_SwordBodyMigration
- `Sects/StockGenerator_Quality.cs`: StockGenerator_QualityTag, StockGenerator_QualityDefs, QualityChance
- `Sects/TemperConstitution.cs`: DefModExtension_TemperConstitution, TemperStep, RitualOutcomeEffectWorker_TemperConstitution, RitualRoleConstitutionSubject
- `Space/IslandDome.cs`: CompProperties_CelestialDome, CompCelestialDome, DomeUtility, Patch_VacuumUtility_GetVacuum_Dome, Patch_GenTemperature_Dome
- `Space/IslandEndgameBuilder.cs`: IslandEndgameBuilder, RoomInfo, Result
- `Space/IslandFlightTest.cs`: GameComponent_IslandFlightTest
- `Space/IslandPatches.cs`: DefModExtension_GravEngineAlias, DefModExtension_GravAnchorAlias, IslandUtility, Patch_ListerThings_ThingsOfDef_GravEngine, Patch_GravEngine_SpawnSetup_Inspected, Patch_ListerThings_AnyThingWithDef_GravEngine, Patch_RitualTargetGravshipLaunch_Consoles, Patch_Gravship_AddThing_Console, Patch_CompPilotConsole_CanUseNow_NoBreakdown, StatPart_HelmsmanRealm, OrbitalQi
- `Space/IslandSelfTest.cs`: GameComponent_IslandTest
- `Space/IslandSpaceTest.cs`: GameComponent_IslandSpaceTest
- `Space/IslandText.cs`: IslandText, IslandTextSwap, GameComponent_IslandTextInstaller
- `Techniques/ChainLightning.cs`: CompProperties_AbilityChainLightning, CompAbilityEffect_ChainLightning
- `Techniques/CompUseEffect_LearnTechniqueManual.cs`: CompProperties_UseEffectLearnManual, CompUseEffect_LearnTechniqueManual
- `Techniques/CultivatorAI.cs`: CompProperties_AbilityAIConditions, CompAbilityEffect_AIConditions, CultivatorAI, Patch_JobGiver_AIFightEnemy_GetAbilityJob
- `Techniques/HediffComp_QiShield.cs`: HediffCompProperties_QiShield, HediffComp_QiShield, PawnRenderNode_Single
- `Techniques/PassiveTechniques.cs`: HediffCompProperties_PassiveTechnique, HediffComp_PassiveTechnique, TechniqueSet, TechniqueSetUtility, PassiveTechniqueUtility
- `Techniques/SpiritGrade.cs`: CompProperties_SpiritGrade, CompSpiritGrade, DefModExtension_SpiritGradeMaterial, StatWorker_SpiritGrade, DefModExtension_RealmSuppression, SpiritGradeUtility
- `Techniques/TechniqueAbilityComps.cs`: CompProperties_AbilityResourceCost, CompAbilityEffect_ResourceCost, CompProperties_AbilityTechniqueProjectile, CompAbilityEffect_TechniqueProjectile, CompProperties_AbilityTechniqueHediff, CompAbilityEffect_TechniqueHediff, Command_Technique
- `Techniques/TechniqueDefs.cs`: TechniqueManualDef, TechniqueMasteryDef, TechniqueExtension
- `Techniques/TechniqueProjectiles.cs`: Projectile_TechniqueBullet, Projectile_TechniqueExplosive
- `Techniques/TechniqueTracker.cs`: TechniqueTracker
- `Techniques/TechniqueUtility.cs`: TechniqueUtility
- `Traits/TraitConflicts.cs`: DefModExtension_TraitConflicts, TraitConflictEntry, TraitConflictUtility
- `Traits/TraitMechanics.cs`: DefModExtension_RealmCultivationCurve, DefModExtension_DualCultivation, DefModExtension_RaidPremonition, DefModExtension_SpiritCraft, TraitMechanics, StatPart_RealmCurveTraits, StatPart_DualCultivation, Patch_Pawn_GetGizmos_DaoVow, TraitMechanics_Vow, Patch_RaidPremonition, GameComponent_RaidPremonition, GameComponent_TraitLives
- `Traits/TraitProgression.cs`: DefModExtension_TraitProgression, TraitProgressionStep, DefModExtension_ProgressCounter, TraitProgressTracker, TraitProgression, Patch_GenRecipe_MakeRecipeProducts, Patch_Plant_PlantCollected, Patch_Frame_CompleteConstruction, Patch_RecruitAttempt_DoRecruit
- `Traits/TraitSelfTest.cs`: TraitSelfTest
- `Traits/TraitThoughts.cs`: InspirationWorker_Cultivator, ThoughtWorker_CVG_Seclusion, ThoughtWorker_CVG_RealmRank, ThoughtWorker_Precept_MissedCultivation
- `Tribulation/ImplantDissonance.cs`: StatPart_ImplantDissonance
- `Tribulation/MapComponent_Tribulation.cs`: MapComponent_Tribulation, Tribulation
- `Tribulation/WeatherEvent_TribulationStrike.cs`: WeatherEvent_TribulationStrike
- `UI/CultivationDevelopmentUtility.cs`: CultivationDevelopmentUtility
- `UI/CultivationGuide.cs`: CultivationGuide
- `UI/Dialog_CultivationDevelopment.cs`: Dialog_CultivationDevelopment
- `UI/Gizmo_ResourceBar.cs`: Gizmo_ResourceBar
- `UI/ITab_Cultivation.cs`: ITab_Cultivation

**Argumentos de autotest y herramientas:** `-cvganimshot`, `-cvgbandtotemtest`, `-cvgbenchshot`, `-cvgcardshot`, `-cvgcropstest`, `-cvgdevshot`, `-cvgflightrender`, `-cvgflighttest`, `-cvgflightvanilla`, `-cvgfloorshot`, `-cvgflowtest`, `-cvgfurnituretest`, `-cvgislandshot`, `-cvgislandspacetest`, `-cvgislandtest`, `-cvgnightshot`, `-cvgplaytest`, `-cvgqinettest`, `-cvgreach`, `-cvgrendertest`, `-cvgscenarioshot`, `-cvgsectideotest`, `-cvgselftest`, `-cvgshowcase`, `-cvgtabshot`, `-cvgtransreport`, `-cvgworkshopshot`, `-quicktest`

## Convergence.Factions — Convergence: Factions

- packageId `prospero1088.convergence.factions`, versión 0.1.1
- Depende de: brrainz.harmony, prospero1088.convergence.core, Ludeon.RimWorld.Royalty, Ludeon.RimWorld.Ideology
- Incompatible con: prospero1088.convergence.sects
- Carpetas que carga: /; Common; 1.6; Mods/BiotechWuxia  (si está prospero1088.convergence.biotechwuxia)

**Defs** (archivo → tipos y defNames):

- `Common/Defs/BloodMoon.xml` — RoyalTitlePermitDef 5, PawnKindDef 4, RoyalTitleDef 4, FactionDef 1: CVG_Faction_BloodMoon, CVG_BloodMoon_Servant, CVG_BloodMoon_Disciple, CVG_BloodMoon_Crimson, CVG_BloodMoon_Elder, CVG_Title_BloodMoon_Guest, CVG_Title_BloodMoon_Sworn, CVG_Title_BloodMoon_Creditor, CVG_Title_BloodMoon_GuestElder, CVG_Permit_BloodMoon_BloodOffering, CVG_Permit_BloodMoon_Disciples, CVG_Permit_BloodMoon_BloodBlade, CVG_Permit_BloodMoon_VenerableGift, CVG_Permit_BloodMoon_CrimsonStrike
- `Common/Defs/BloodMoon_Backstories.xml` — BackstoryDef 8: CVG_Backstory_BM_BloodServant, CVG_Backstory_BM_RiftRunner, CVG_Backstory_BM_BloodDisciple, CVG_Backstory_BM_LanternKeeper, CVG_Backstory_BM_CrimsonEnvoy, CVG_Backstory_BM_DebtCollector, CVG_Backstory_BM_BloodElder, CVG_Backstory_BM_SoulElder
- `Common/Defs/FormlessSword.xml` — PawnKindDef 5, RoyalTitlePermitDef 5, RoyalTitleDef 4, TraderKindDef 2, FactionDef 1: CVG_Faction_FormlessSword, CVG_FormlessSword_Disciple, CVG_FormlessSword_Bearer, CVG_FormlessSword_Blade, CVG_FormlessSword_Elder, CVG_FormlessSword_Swordsmith, CVG_Trader_FormlessSword, CVG_Trader_FormlessSwordSettlement, CVG_Title_FormlessSword_Friend, CVG_Title_FormlessSword_Guest, CVG_Title_FormlessSword_Protector, CVG_Title_FormlessSword_GuestElder, CVG_Permit_FormlessSword_SpiritResources, CVG_Permit_FormlessSword_Disciples, CVG_Permit_FormlessSword_MasterworkSword, CVG_Permit_FormlessSword_LegendarySword, CVG_Permit_FormlessSword_SwordStrike
- `Common/Defs/FormlessSword_Backstories.xml` — BackstoryDef 9: CVG_Backstory_FS_OuterDisciple, CVG_Backstory_FS_MoundKeeper, CVG_Backstory_FS_InnerBearer, CVG_Backstory_FS_WanderingBlade, CVG_Backstory_FS_CoreBlade, CVG_Backstory_FS_TournamentRival, CVG_Backstory_FS_SwordElder, CVG_Backstory_FS_DisciplineElder, CVG_Backstory_FS_Swordsmith
- `Common/Defs/HeavenlyCauldron.xml` — PawnKindDef 5, RoyalTitlePermitDef 5, RoyalTitleDef 4, TraderKindDef 2, FactionDef 1: CVG_Faction_HeavenlyCauldron, CVG_HeavenlyCauldron_Apprentice, CVG_HeavenlyCauldron_Alchemist, CVG_HeavenlyCauldron_Master, CVG_HeavenlyCauldron_Grand, CVG_HeavenlyCauldron_Auctioneer, CVG_Trader_HeavenlyCauldron, CVG_Trader_HeavenlyCauldronSettlement, CVG_Title_HeavenlyCauldron_Friend, CVG_Title_HeavenlyCauldron_Guest, CVG_Title_HeavenlyCauldron_Protector, CVG_Title_HeavenlyCauldron_GuestElder, CVG_Permit_HeavenlyCauldron_AlchemyMaterials, CVG_Permit_HeavenlyCauldron_Alchemists, CVG_Permit_HeavenlyCauldron_MasterworkArtifact, CVG_Permit_HeavenlyCauldron_PillOfChoice, CVG_Permit_HeavenlyCauldron_FireStrike
- `Common/Defs/HeavenlyCauldron_Backstories.xml` — BackstoryDef 9: CVG_Backstory_HC_FurnaceStoker, CVG_Backstory_HC_HerbRunner, CVG_Backstory_HC_Alchemist, CVG_Backstory_HC_PillTaster, CVG_Backstory_HC_CauldronMaster, CVG_Backstory_HC_MaterialHunter, CVG_Backstory_HC_GrandAlchemist, CVG_Backstory_HC_LedgerElder, CVG_Backstory_HC_Auctioneer
- `Common/Defs/Ideology/BloodMoon_Ideology.xml` — PreceptDef 2, MemeDef 1, IdeoPresetDef 1, RitualOutcomeEffectDef 1, RitualPatternDef 1: CVG_Meme_DemonicWay, CVG_Preset_BloodMoon, CVG_Role_BloodMoonOfficiant, CVG_Outcome_BloodOath, CVG_Pattern_BloodOath, CVG_Ritual_BloodOath
- `Common/Defs/Ideology/FormlessSword_Ideology.xml` — PreceptDef 2, MemeDef 1, IdeoPresetDef 1, RitualOutcomeEffectDef 1, RitualPatternDef 1: CVG_Meme_SwordWay, CVG_Preset_FormlessSword, CVG_Role_SwordMaster, CVG_Outcome_TemperSwordBody, CVG_Pattern_TemperSwordBody, CVG_Ritual_TemperSwordBody
- `Common/Defs/Ideology/HeavenlyCauldron_Ideology.xml` — PreceptDef 2, MemeDef 1, IdeoPresetDef 1, RitualOutcomeEffectDef 1, RitualPatternDef 1: CVG_Meme_CauldronWay, CVG_Preset_HeavenlyCauldron, CVG_Role_AlchemistMaster, CVG_Outcome_CauldronBlessing, CVG_Pattern_CauldronBlessing, CVG_Ritual_CauldronBlessing
- `Common/Defs/Ideology/Memes_Structures_Wuxia.xml` — MemeDef 3: CVG_Structure_DaoOrigin, CVG_Structure_HeavenMandate, CVG_Structure_Ancestors
- `Common/Defs/Ideology/NineTemperings_Ideology.xml` — PreceptDef 2, MemeDef 1, IdeoPresetDef 1, RitualOutcomeEffectDef 1, RitualPatternDef 1: CVG_Meme_BodyWay, CVG_Preset_NineTemperings, CVG_Role_TemperingMaster, CVG_Outcome_NineTemperings, CVG_Pattern_NineTemperings, CVG_Ritual_NineTemperings
- `Common/Defs/Ideology/Precepts_Sect.xml` — ThoughtDef 21, PreceptDef 20, IssueDef 7: CVG_Issue_Cultivation, CVG_Issue_Alchemy, CVG_Issue_Essence, CVG_Issue_Demonic, CVG_Issue_Duels, CVG_Issue_Hierarchy, CVG_Issue_Retreat, CVG_Cultivation_Obligatory, CVG_Cultivation_Venerated, CVG_Cultivation_Free, CVG_Cultivation_Frowned, CVG_Alchemy_Venerated, CVG_Alchemy_Accepted, CVG_Alchemy_Frowned, CVG_Essence_Abhorrent, CVG_Essence_Accepted, CVG_Essence_Honored, CVG_Demonic_Forbidden, CVG_Demonic_Tolerated, CVG_Demonic_Embraced, CVG_Duels_Encouraged, CVG_Duels_Allowed, CVG_Duels_Forbidden, CVG_Hierarchy_Elders, CVG_Hierarchy_Equals, CVG_Retreat_Revered, CVG_Retreat_Ordinary, CVG_Cultivation_Obligatory_Self, CVG_Cultivation_Venerated_Self, CVG_Cultivation_Frowned_Self, CVG_Cultivation_Frowned_Know, CVG_Alchemy_Venerated_Self, CVG_Alchemy_Frowned_Self, CVG_Alchemy_Frowned_Know, CVG_Essence_Abhorrent_Self, CVG_Essence_Abhorrent_Know, CVG_Essence_Abhorrent_KnowMood, CVG_Essence_Honored_Self, CVG_Demonic_Forbidden_Self, CVG_Demonic_Forbidden_Know … (+8)
- `Common/Defs/Ideology/RogueCultivators_Ideology.xml` — MemeDef 1, IdeoPresetDef 1: CVG_Meme_WanderingPath, CVG_Preset_WanderingPath
- `Common/Defs/Ideology/SectIdeoIcons.xml` — IdeoIconDef 5, ColorDef 1: CVG_IdeoColor_Natural, CVG_IdeoIcon_SwordWay, CVG_IdeoIcon_BodyWay, CVG_IdeoIcon_CauldronWay, CVG_IdeoIcon_DemonicWay, CVG_IdeoIcon_WanderingPath
- `Common/Defs/Ideology/Sect_Framework.xml` — InteractionDef 1, RitualBehaviorDef 1: CVG_Speech_Tempering, CVG_SectTemperBehavior
- `Common/Defs/Names_BloodMoon.xml` — RulePackDef 2: CVG_NamerPerson_BloodMoon, CVG_NamerSettlement_BloodMoon
- `Common/Defs/Names_FormlessSword.xml` — RulePackDef 2: CVG_NamerPerson_FormlessSword, CVG_NamerSettlement_FormlessSword
- `Common/Defs/Names_HeavenlyCauldron.xml` — RulePackDef 2: CVG_NamerPerson_HeavenlyCauldron, CVG_NamerSettlement_HeavenlyCauldron
- `Common/Defs/Names_NineTemperings.xml` — RulePackDef 2: CVG_NamerPerson_NineTemperings, CVG_NamerSettlement_NineTemperings
- `Common/Defs/Names_RogueCultivators.xml` — RulePackDef 2: CVG_NamerPerson_RogueCultivators, CVG_NamerSettlement_RogueCultivators
- `Common/Defs/NineTemperings.xml` — PawnKindDef 5, RoyalTitlePermitDef 5, RoyalTitleDef 4, TraderKindDef 2, FactionDef 1: CVG_Faction_NineTemperings, CVG_NineTemperings_Disciple, CVG_NineTemperings_Inner, CVG_NineTemperings_Core, CVG_NineTemperings_Elder, CVG_NineTemperings_Trader, CVG_Trader_NineTemperings, CVG_Trader_NineTemperingsSettlement, CVG_Title_NineTemperings_Friend, CVG_Title_NineTemperings_Guest, CVG_Title_NineTemperings_Protector, CVG_Title_NineTemperings_GuestElder, CVG_Permit_NineTemperings_TemperingResources, CVG_Permit_NineTemperings_Disciples, CVG_Permit_NineTemperings_MasterworkArmor, CVG_Permit_NineTemperings_LegendaryArmor, CVG_Permit_NineTemperings_BodyStrike
- `Common/Defs/NineTemperings_Backstories.xml` — BackstoryDef 9: CVG_Backstory_NT_OuterDisciple, CVG_Backstory_NT_StoneCarrier, CVG_Backstory_NT_InnerDisciple, CVG_Backstory_NT_BeastHunter, CVG_Backstory_NT_CoreDisciple, CVG_Backstory_NT_PitChampion, CVG_Backstory_NT_TemperingElder, CVG_Backstory_NT_VajraGuardian, CVG_Backstory_NT_BloodTrader
- `Common/Defs/RogueCultivators.xml` — PawnKindDef 5, TraderKindDef 2, FactionDef 1: CVG_Faction_RogueCultivators, CVG_RogueCultivators_Wanderer, CVG_RogueCultivators_Sellsword, CVG_RogueCultivators_Veteran, CVG_RogueCultivators_Hermit, CVG_RogueCultivators_Peddler, CVG_Trader_RogueCultivators, CVG_Trader_RogueWanderer
- `Common/Defs/Sect_Bases.xml` — : 
- `Mods/BiotechWuxia/Defs/InvisibleThread.xml` — PawnKindDef 5, RoyalTitlePermitDef 5, RoyalTitleDef 4, TraderKindDef 2, FactionDef 1: CVG_Faction_InvisibleThread, CVG_InvisibleThread_Apprentice, CVG_InvisibleThread_Puppeteer, CVG_InvisibleThread_Master, CVG_InvisibleThread_Elder, CVG_InvisibleThread_Merchant, CVG_Trader_InvisibleThread, CVG_Trader_InvisibleThreadSettlement, CVG_Title_InvisibleThread_Friend, CVG_Title_InvisibleThread_Guest, CVG_Title_InvisibleThread_Keeper, CVG_Title_InvisibleThread_GuestElder, CVG_Permit_InvisibleThread_PuppetMaterials, CVG_Permit_InvisibleThread_BladePuppets, CVG_Permit_InvisibleThread_MasterworkRegalia, CVG_Permit_InvisibleThread_GrandMasterGift, CVG_Permit_InvisibleThread_JadeStrike
- `Mods/BiotechWuxia/Defs/InvisibleThread_Backstories.xml` — BackstoryDef 9: CVG_Backstory_IT_ThreadSpinner, CVG_Backstory_IT_JointCarver, CVG_Backstory_IT_Puppeteer, CVG_Backstory_IT_ShadowPlayer, CVG_Backstory_IT_PuppetMaster, CVG_Backstory_IT_MachineBreaker, CVG_Backstory_IT_GrandPuppeteer, CVG_Backstory_IT_BlueprintKeeper, CVG_Backstory_IT_ThreadMerchant
- `Mods/BiotechWuxia/Defs/InvisibleThread_IdeoIcon.xml` — IdeoIconDef 1: CVG_IdeoIcon_PuppetPath
- `Mods/BiotechWuxia/Defs/InvisibleThread_Ideology.xml` — PreceptDef 2, MemeDef 1, IdeoPresetDef 1, RitualOutcomeEffectDef 1, RitualPatternDef 1: CVG_Meme_PuppetPath, CVG_Preset_InvisibleThread, CVG_Role_PuppetMaster, CVG_Outcome_ThreadBinding, CVG_Pattern_ThreadBinding, CVG_Ritual_ThreadBinding
- `Mods/BiotechWuxia/Defs/Names_InvisibleThread.xml` — RulePackDef 2: CVG_NamerPerson_InvisibleThread, CVG_NamerSettlement_InvisibleThread

**Idiomas:** English, Spanish, SpanishLatin · **Texturas:** 40

**Código en el Core:** Source/Convergence.Core/ → Sects/ (constituciones, templado, desvío de proyectiles, stock con calidad, historias de secta); el resto en Source/Convergence.Factions

**Código** (`Source/Convergence.Factions`):

- `FactionsSelfTest.cs`: GameComponent_FactionsTest, SectSpec
- `FactionsShot.cs`: FactionsShotMap, GameComponent_FactionsShot
- `FavorsTest.cs`: GameComponent_FavorsTest
- `SectGift.cs`: DefModExtension_SectGift, SectGiftOption, RoyalTitlePermitWorker_SectGift
- `SectIdeos.cs`: DefModExtension_SectIdeo, Patch_Precept_Icon_SectRoles, Patch_FactionGenerator_NewGeneratedFaction, FactionsHarmony, GameComponent_SectIdeos

**Argumentos de autotest y herramientas:** `-cvgfactionsshot`, `-cvgfactionstest`, `-cvgfavorstest`

## Convergence.FaunaFlora — Convergence: Fauna & Flora

- packageId `prospero1088.convergence.faunaflora`, versión 0.1.1
- Depende de: brrainz.harmony, prospero1088.convergence.core
- Carpetas que carga: /; Common; Mods/Odyssey  (si está Ludeon.RimWorld.Odyssey); Mods/Apparel  (si está prospero1088.convergence.apparel); Mods/Resources  (si está prospero1088.convergence.resources)

**Defs** (archivo → tipos y defNames):

- `Common/Defs/Animals.xml` — ThingDef 16, PawnKindDef 10: CVG_SpiderCocoon, CVG_SevenColorWool, CVG_EggPhoenixHenUnfertilized, CVG_EggPhoenixHenFertilized, CVG_SpiritMilk, CVG_MoonAntler, CVG_JadeWeaverSpider, CVG_JadeWeaverSpider, CVG_SevenColorSheep, CVG_SevenColorSheep, CVG_PhoenixBloodHen, CVG_PhoenixBloodHen, CVG_CloudOx, CVG_CloudOx, CVG_MoonDeer, CVG_MoonDeer, CVG_WindWolf, CVG_WindWolf, CVG_CelestialCrane, CVG_CelestialCrane, CVG_NineNetherSpider, CVG_NineNetherSpider, CVG_Xuanwu, CVG_Xuanwu, CVG_LightningTiger, CVG_LightningTiger
- `Common/Defs/BeastParts.xml` — ThingDef 4: CVG_CraneFeather, CVG_WolfFang, CVG_NetherChitin, CVG_XuanwuShell
- `Common/Defs/Crops.xml` — ThingDef 9: CVG_GlutinousRice, CVG_Plant_GlutinousRice, CVG_CloudTeaLeaf, CVG_Plant_CloudTea, CVG_LotusRoot, CVG_Plant_Lotus, CVG_MulberryLeaf, CVG_Plant_Mulberry, CVG_Plant_MillennialPine
- `Common/Defs/Flora.xml` — ThingDef 4, TerrainDef 1: CVG_SpiritSoil, CVG_TribulationMushroom, CVG_Plant_TribulationMushroom, CVG_SpiritFungus, CVG_Plant_SpiritFungus
- `Common/Defs/Foods.xml` — ThingDef 1, RecipeDef 1: CVG_RiceCake, CVG_CookRiceCake
- `Mods/Apparel/Defs/Recipes_Silk.xml` — RecipeDef 2: CVG_SpinSpiritSilk, CVG_RaiseSilkworms
- `Mods/Odyssey/Defs/Fish_Spirit.xml` — ThingDef 4: CVG_Fish_GoldenCarp, CVG_Fish_JadeEel, CVG_Fish_MoonFish, CVG_Fish_ThunderFish

**Parches:** `Common/Patches/BeastParts.xml`, `Common/Patches/Crops_Wild.xml`, `Mods/Odyssey/Patches/SpiritFishing.xml`, `Mods/Resources/Patches/MillennialPine.xml`, `Patches/SpiritPlants.xml`

**Idiomas:** Spanish, SpanishLatin · **Texturas:** 74

**Código en el Core:** Source/Convergence.Core/ → Body/SpiritBeast.cs, Cultivation/FruitTree.cs, CompPlantSpread.cs, WaterGrowing.cs

## Convergence.Formations — Convergence: Formations

- packageId `prospero1088.convergence.formations`, versión 0.1.2
- Depende de: brrainz.harmony, prospero1088.convergence.core
- Carpetas que carga: /; Common; 1.6

**Defs** (archivo → tipos y defNames):

- `Common/Defs/Defense3.xml` — ThingDef 3: CVG_BambooPalisade, CVG_LowStoneWall, CVG_QiSpikeTrap
- `Common/Defs/Formations.xml` — ThingDef 10: CVG_FormationFlag, CVG_QiGatheringCore, CVG_SwordFormationCore, CVG_BindingFormationCore, CVG_ProtectiveFormationCore, CVG_Projectile_GuardianQiSword, CVG_Gun_GuardianStatue, CVG_Gun_RuneCrossbow, CVG_GuardianStatue, CVG_RuneCrossbowTurret
- `Common/Defs/QiComfort.xml` — ThingDef 5: CVG_SkyBreathArray, CVG_ThunderRod, CVG_SunLantern, CVG_SpiritBrazier, CVG_FrostFormation
- `Common/Defs/QiNetwork.xml` — ThingDef 5, ResearchProjectDef 1: CVG_Research_QiNetwork, CVG_QiVeinWell, CVG_QiGatherArray, CVG_FormationPillar, CVG_QiCrystal, CVG_CondensationPool
- `Common/Defs/Research_Formations.xml` — ResearchProjectDef 4: CVG_Research_FiveElements, CVG_Research_QiGathering, CVG_Research_DefensiveFormations, CVG_Research_GreatProtective
- `Common/Defs/Turret2.xml` — ThingDef 3: CVG_Projectile_QiBolt, CVG_Gun_ThunderBallista, CVG_ThunderBallistaTurret

**Idiomas:** English, Spanish, SpanishLatin · **Texturas:** 27

**Código** (`Source/Convergence.Formations`):

- `FormationFlowTest.cs`: GameComponent_FormationFlowTest
- `Formations.cs`: CompProperties_FormationCore, FormationWorker, CompFormationCore, FormationMapComponent, FormationWorker_Protective, FormationWorker_Swords, FormationWorker_Binding, FormationWorker_QiGather, FormationsStartup, Patch_Projectile_Tick, PlaceWorker_FormationRange
- `FormationsSelfTest.cs`: GameComponent_FormationsTest
- `LinkedWallShot.cs`: GameComponent_LinkedWallShot
- `TurretShot.cs`: GameComponent_TurretShot

**Argumentos de autotest y herramientas:** `-cvgformationstest`, `-cvgformflow`, `-cvgturretshot`, `-cvgwallshot`

## Convergence.Furniture — Convergence: Furniture

- packageId `prospero1088.convergence.furniture`, versión 0.1.1
- Depende de: prospero1088.convergence.core
- Carpetas que carga: /; Common; Mods/Resources  (si está prospero1088.convergence.resources)

**Defs** (archivo → tipos y defNames):

- `Common/Defs/Art_Jade.xml` — ThingDef 3: CVG_JadeSculptureSmall, CVG_JadeSculptureLarge, CVG_JadeSculptureGrand
- `Common/Defs/Furniture.xml` — ThingDef 24, RecipeDef 1: CVG_CarvedScreen, CVG_PorcelainVase, CVG_BronzeCenser, CVG_KoiPond, CVG_RitualBrazier, CVG_SpiritBonsai, CVG_RockGarden, CVG_StatueDragon, CVG_StatuePhoenix, CVG_StatueQilin, CVG_StatueTortoise, CVG_PaintingScroll, CVG_SectBanner, CVG_SectBed, CVG_LowTable, CVG_MeditationStool, CVG_RobeWardrobe, CVG_ScriptureShelf, CVG_PillCabinet, CVG_SpiritWineJar, CVG_PaperLantern, CVG_SpiritStoneLantern, CVG_GlowCrystal, CVG_SpiritWine, CVG_BrewSpiritWine
- `Common/Defs/Furniture3.xml` — ThingDef 15, JobDef 3, JoyGiverDef 3, TerrainDef 1, JoyKindDef 1: CVG_SectDoubleBed, CVG_HealerBed, CVG_BanquetTable, CVG_CarvedChair, CVG_WarehouseShelf, CVG_GrainJar, CVG_SectChest, CVG_MoonDoor, CVG_LacqueredDoor, CVG_BambooWall, CVG_RoofTileFloor, CVG_Music, CVG_MahjongTable, CVG_TeaPavilion, CVG_Guqin, CVG_Play_Mahjong, CVG_Take_Tea, CVG_Play_Guqin, CVG_Play_Mahjong, CVG_Take_Tea, CVG_Play_Guqin, CVG_StoneBench, CVG_LotusPlanter
- `Common/Defs/Furniture4.xml` — ThingDef 5: CVG_JadePlatform, CVG_SpiritJadeDais, CVG_BedsideTray, CVG_SleepingMat, CVG_BambooRecliner
- `Common/Defs/Workbenches.xml` — ThingDef 5: CVG_SectKitchen, CVG_ButcherBlock, CVG_StonecutterWorkshop, CVG_JadeCarvingTable, CVG_SpiritWinery

**Idiomas:** Spanish, SpanishLatin · **Texturas:** 115

**Código en el Core:** Source/Convergence.Core/ → QiNetwork/ (red de qi), Debug/FurnitureSelfTest.cs

## Convergence.Grafts — Convergence: Grafts

- packageId `prospero1088.convergence.grafts`, versión 0.1.1
- Depende de: brrainz.harmony, prospero1088.convergence.core
- Carpetas que carga: /; Common; 1.6

**Defs** (archivo → tipos y defNames):

- `Common/Defs/Grafts.xml` — ThingDef 14, RecipeDef 14, HediffDef 12, ResearchProjectDef 2: CVG_Graft_FarmerArm, CVG_Graft_FarmerArm, CVG_Install_FarmerArm, CVG_Graft_MinerArm, CVG_Graft_MinerArm, CVG_Install_MinerArm, CVG_Graft_SmithHand, CVG_Graft_SmithHand, CVG_Install_SmithHand, CVG_Graft_AlchemistHand, CVG_Graft_AlchemistHand, CVG_Install_AlchemistHand, CVG_Graft_HealerHand, CVG_Graft_HealerHand, CVG_Install_HealerHand, CVG_Graft_BuilderArm, CVG_Graft_BuilderArm, CVG_Install_BuilderArm, CVG_Graft_JadeArtistFingers, CVG_Graft_JadeArtistFingers, CVG_Install_JadeArtistFingers, CVG_Graft_ShepherdArm, CVG_Graft_ShepherdArm, CVG_Install_ShepherdArm, CVG_Graft_DaoBones, CVG_Graft_DaoBones, CVG_Install_DaoBones, CVG_Graft_DragonVeins, CVG_Graft_DragonVeins, CVG_Install_DragonVeins, CVG_Remove_DaoBones, CVG_Remove_DragonVeins, CVG_Graft_ThousandLiEyes, CVG_Graft_ThousandLiEyes, CVG_Install_ThousandLiEyes, CVG_Graft_SteedLegs, CVG_Graft_SteedLegs, CVG_Install_SteedLegs, CVG_BoneRefiningFurnace, CVG_GraftingAltar … (+2)
- `Common/Defs/GraftsBeastOrgans.xml` — RecipeDef 5, HediffDef 4, ThingDef 4: CVG_Graft_DragonLungs, CVG_Graft_DragonLungs, CVG_Install_DragonLungs, CVG_Graft_JadeKidneys, CVG_Graft_JadeKidneys, CVG_Install_JadeKidneys, CVG_Graft_WindHearingEars, CVG_Graft_WindHearingEars, CVG_Install_WindHearingEars, CVG_Graft_QilinSkin, CVG_Graft_QilinSkin, CVG_Install_QilinSkin, CVG_Remove_QilinSkin
- `Common/Defs/GraftsSpiritWood.xml` — HediffDef 5, ThingDef 5, RecipeDef 5: CVG_Graft_SpiritWoodLeg, CVG_Graft_SpiritWoodLeg, CVG_Install_SpiritWoodLeg, CVG_Graft_SpiritWoodArm, CVG_Graft_SpiritWoodArm, CVG_Install_SpiritWoodArm, CVG_Graft_SpiritWoodHand, CVG_Graft_SpiritWoodHand, CVG_Install_SpiritWoodHand, CVG_Graft_SpiritWoodFoot, CVG_Graft_SpiritWoodFoot, CVG_Install_SpiritWoodFoot, CVG_Graft_JadeDenture, CVG_Graft_JadeDenture, CVG_Install_JadeDenture

**Idiomas:** English, Spanish, SpanishLatin · **Texturas:** 27

**Código** (`Source/Convergence.Grafts`):

- `Grafts.cs`: GraftUtility, Recipe_InstallGraft, Recipe_InstallGraftImplant, GraftsStartup
- `GraftsSelfTest.cs`: GameComponent_GraftsTest

**Argumentos de autotest y herramientas:** `-cvggraftstest`

## Convergence.QiTech — Convergence: Qi-Tech

- packageId `prospero1088.convergence.qitech`, versión 0.2.2
- Depende de: brrainz.harmony, prospero1088.convergence.core, prospero1088.convergence.formations
- Carpetas que carga: /; Common; 1.6

**Defs** (archivo → tipos y defNames):

- `Common/Defs/QiTech.xml` — ResearchProjectDef 3, ThingDef 3, HediffDef 1: CVG_Research_QiTech, CVG_Research_QiStabilization, CVG_Research_QiExosuit, CVG_QiConverter, CVG_Stabilized, CVG_StabilizationChamber, CVG_Apparel_QiTechExosuit
- `Common/Defs/QiTech_ResearchMilitary.xml` — ResearchProjectDef 4: CVG_Research_QiInduction, CVG_Research_QiWeapons, CVG_Research_QiDefense, CVG_Research_QiAdvancedMilitary
- `Common/Defs/QiTech_SpiritEngineering.xml` — ResearchProjectDef 3, ResearchTabDef 1, ThingDef 1: CVG_SpiritualEngineering, CVG_Research_QiFundamentals, CVG_Research_SpiritResonance, CVG_Research_AdvancedSpiritInstrumentation, CVG_SpiritResonator
- `Common/Defs/QiTech_Turrets.xml` — ThingDef 4: CVG_QT_Gun_InductionTurret, CVG_QT_InductionTurret, CVG_QT_Gun_ContainmentTurret, CVG_QT_ContainmentTurret
- `Common/Defs/QiTech_Weapons.xml` — ThingDef 6: CVG_QT_InductionBolt, CVG_QT_FocusedBolt, CVG_QT_CompressedBolt, CVG_QT_InductionRifle, CVG_QT_FocusedRifle, CVG_QT_CompressionCannon

**Parches:** `Common/Patches/QiTech_Patches.xml`

**Idiomas:** English, Spanish, SpanishLatin · **Texturas:** 35

**Código** (`Source/Convergence.QiTech`):

- `QiTech.cs`: ConverterMode, CompProperties_QiConverter, CompQiConverterNode, CompQiConverterPower, CompProperties_StabilizationChamber, CompStabilizationChamber
- `QiTechAimShot.cs`: GameComponent_QiTechAimShot
- `QiTechLabShowcase.cs`: GameComponent_QiTechLab
- `QiTechMilitaryTest.cs`: GameComponent_QiTechMilitaryTest
- `QiTechSelfTest.cs`: GameComponent_QiTechTest
- `QiTechShowcase.cs`: GameComponent_QiTechShowcase
- `QiTechWorkshopShot.cs`: GameComponent_QiTechWorkshopShot, QiTechWorkshopShotMap
- `SpiritEngineering.cs`: DefModExtension_RequiresConvergence, Patch_ResearchProjectDef_CanStartNow, SpiritEngineeringStartup
- `TurretMuzzle.cs`: DefModExtension_TurretMuzzle, QiTechHarmony, Patch_Projectile_Launch_Muzzle

**Argumentos de autotest y herramientas:** `-cvgqitechaimshot`, `-cvgqitechlab`, `-cvgqitechshowcase`, `-cvgqitechtest`, `-cvgqitechwartest`, `-cvgqitechworkshopshot`

## Convergence.Races — Convergence: Races

- packageId `prospero1088.convergence.races`, versión 0.1.1
- Depende de: brrainz.harmony, prospero1088.convergence.core, Ludeon.RimWorld.Biotech
- Carpetas que carga: /; Common

**Defs** (archivo → tipos y defNames):

- `Common/Defs/Backstories_Bloodlines.xml` — BackstoryDef 10: CVG_Backstory_DragonClanWard, CVG_Backstory_StormArchivist, CVG_Backstory_AshOrphan, CVG_Backstory_CauldronFlamekeeper, CVG_Backstory_LotusNovice, CVG_Backstory_ValleyHealer, CVG_Backstory_MountainShadowChild, CVG_Backstory_ShellKeeper, CVG_Backstory_FoxWeddingChild, CVG_Backstory_QingqiuCourtier
- `Common/Defs/BloodGenes_Common.xml` — GeneDef 20, BloodlineGeneGroupDef 5: CVG_GeneGroup_BeastVitality, CVG_BloodGene_BeastVitality_I, CVG_BloodGene_BeastVitality_II, CVG_BloodGene_BeastVitality_III, CVG_BloodGene_BeastVitality_IV, CVG_GeneGroup_KeenSenses, CVG_BloodGene_KeenSenses_I, CVG_BloodGene_KeenSenses_II, CVG_BloodGene_KeenSenses_III, CVG_BloodGene_KeenSenses_IV, CVG_GeneGroup_BloodLongevity, CVG_BloodGene_BloodLongevity_I, CVG_BloodGene_BloodLongevity_II, CVG_BloodGene_BloodLongevity_III, CVG_BloodGene_BloodLongevity_IV, CVG_GeneGroup_BeastInstinct, CVG_BloodGene_BeastInstinct_I, CVG_BloodGene_BeastInstinct_II, CVG_BloodGene_BeastInstinct_III, CVG_BloodGene_BeastInstinct_IV, CVG_GeneGroup_QiResonance, CVG_BloodGene_QiResonance_I, CVG_BloodGene_QiResonance_II, CVG_BloodGene_QiResonance_III, CVG_BloodGene_QiResonance_IV
- `Common/Defs/BloodGenes_Dragon.xml` — GeneDef 9, BloodlineGeneGroupDef 3: CVG_GeneGroup_DragonScales, CVG_BloodGene_DragonScales_I, CVG_BloodGene_DragonScales_II, CVG_BloodGene_DragonScales_III, CVG_BloodGene_DragonScales_IV, CVG_GeneGroup_DragonLungs, CVG_BloodGene_DragonLungs_II, CVG_BloodGene_DragonLungs_III, CVG_BloodGene_DragonLungs_IV, CVG_GeneGroup_DragonVitality, CVG_BloodGene_DragonVitality_III, CVG_BloodGene_DragonVitality_IV
- `Common/Defs/BloodGenes_Fox.xml` — GeneDef 11, BloodlineGeneGroupDef 3, AbilityDef 1, ThingDef 1: CVG_GeneGroup_FoxFire, CVG_BloodGene_FoxFire_II, CVG_BloodGene_FoxFire_III, CVG_BloodGene_FoxFire_IV, CVG_GeneGroup_IllusionVeil, CVG_BloodGene_IllusionVeil_I, CVG_BloodGene_IllusionVeil_II, CVG_BloodGene_IllusionVeil_III, CVG_BloodGene_IllusionVeil_IV, CVG_GeneGroup_NineLives, CVG_BloodGene_NineLives_I, CVG_BloodGene_NineLives_II, CVG_BloodGene_NineLives_III, CVG_BloodGene_NineLives_IV, CVG_Ability_FoxFire, CVG_Projectile_FoxFire
- `Common/Defs/BloodGenes_Phoenix.xml` — GeneDef 9, BloodlineGeneGroupDef 3: CVG_GeneGroup_FieryPlumes, CVG_BloodGene_FieryPlumes_I, CVG_BloodGene_FieryPlumes_II, CVG_BloodGene_FieryPlumes_III, CVG_BloodGene_FieryPlumes_IV, CVG_GeneGroup_AshBorn, CVG_BloodGene_AshBorn_II, CVG_BloodGene_AshBorn_III, CVG_BloodGene_AshBorn_IV, CVG_GeneGroup_Nirvana, CVG_BloodGene_Nirvana_III, CVG_BloodGene_Nirvana_IV
- `Common/Defs/BloodGenes_Qilin.xml` — GeneDef 9, BloodlineGeneGroupDef 3, HediffDef 2, ThoughtDef 1: CVG_GeneGroup_AuspiciousPresence, CVG_BloodGene_AuspiciousPresence_I, CVG_BloodGene_AuspiciousPresence_II, CVG_BloodGene_AuspiciousPresence_III, CVG_BloodGene_AuspiciousPresence_IV, CVG_GeneGroup_PureHooves, CVG_BloodGene_PureHooves_II, CVG_BloodGene_PureHooves_III, CVG_BloodGene_PureHooves_IV, CVG_GeneGroup_EvilRepellent, CVG_BloodGene_EvilRepellent_III, CVG_BloodGene_EvilRepellent_IV, CVG_AuspiciousPresence, CVG_Thought_AuspiciousPresence, CVG_EvilRepelled
- `Common/Defs/BloodGenes_Tortoise.xml` — GeneDef 9, BloodlineGeneGroupDef 3: CVG_GeneGroup_Shell, CVG_BloodGene_Shell_I, CVG_BloodGene_Shell_II, CVG_BloodGene_Shell_III, CVG_BloodGene_Shell_IV, CVG_GeneGroup_StoneSleep, CVG_BloodGene_StoneSleep_II, CVG_BloodGene_StoneSleep_III, CVG_BloodGene_StoneSleep_IV, CVG_GeneGroup_GuardianSerpent, CVG_BloodGene_GuardianSerpent_III, CVG_BloodGene_GuardianSerpent_IV
- `Common/Defs/Bloodline_Bases.xml` — GeneCategoryDef 1, AbilityCategoryDef 1: CVG_Bloodline, CVG_InnateBloodline
- `Common/Defs/Bloodline_Dragon.xml` — GeneDef 11, BloodlineDef 1, HediffDef 1, AbilityDef 1, XenotypeDef 1, PawnKindDef 1: CVG_Bloodline_Dragon, CVG_Blood_Dragon_Trace, CVG_Blood_Dragon_Diluted, CVG_Blood_Dragon_Pure, CVG_Blood_Dragon_Ancestral, CVG_Dragon_Scales, CVG_Dragon_Eyes, CVG_Dragon_Mane, CVG_Dragon_ThunderVoice, CVG_Dragon_Horns, CVG_Dragon_HornsGreat, CVG_Dragon_Tail, CVG_Bloodline_Dragon, CVG_Ability_DragonBreath, CVG_Xeno_Dragon, CVG_Kind_Dragon
- `Common/Defs/Bloodline_Fox.xml` — GeneDef 12, HediffDef 2, BloodlineDef 1, AbilityDef 1, XenotypeDef 1, PawnKindDef 1: CVG_Bloodline_Fox, CVG_Blood_Fox_Trace, CVG_Blood_Fox_Diluted, CVG_Blood_Fox_Pure, CVG_Blood_Fox_Ancestral, CVG_Fox_Ears, CVG_Fox_Eyes, CVG_Fox_Mane, CVG_Fox_ManeAncestral, CVG_Fox_Tail1, CVG_Fox_Tail3, CVG_Fox_Tail6, CVG_Fox_Tail9, CVG_Bloodline_Fox, CVG_Ability_FoxCharm, CVG_FoxCharmed, CVG_Xeno_Fox, CVG_Kind_Fox
- `Common/Defs/Bloodline_Phoenix.xml` — GeneDef 11, BloodlineDef 1, HediffDef 1, AbilityDef 1, ThingDef 1, XenotypeDef 1, PawnKindDef 1: CVG_Bloodline_Phoenix, CVG_Blood_Phoenix_Trace, CVG_Blood_Phoenix_Diluted, CVG_Blood_Phoenix_Pure, CVG_Blood_Phoenix_Ancestral, CVG_Phoenix_Plumage, CVG_Phoenix_Eyes, CVG_Phoenix_Skin, CVG_Phoenix_SkinAncestral, CVG_Phoenix_Crest, CVG_Phoenix_Tail, CVG_Phoenix_TailGreat, CVG_Bloodline_Phoenix, CVG_Ability_PhoenixFlames, CVG_Projectile_PhoenixFlame, CVG_Xeno_Phoenix, CVG_Kind_Phoenix
- `Common/Defs/Bloodline_Qilin.xml` — GeneDef 10, HediffDef 2, BloodlineDef 1, AbilityDef 1, ThoughtDef 1, XenotypeDef 1, PawnKindDef 1: CVG_Bloodline_Qilin, CVG_Blood_Qilin_Trace, CVG_Blood_Qilin_Diluted, CVG_Blood_Qilin_Pure, CVG_Blood_Qilin_Ancestral, CVG_Qilin_Mane, CVG_Qilin_Eyes, CVG_Qilin_Scales, CVG_Qilin_HornSmall, CVG_Qilin_Horn, CVG_Qilin_Ears, CVG_Bloodline_Qilin, CVG_Ability_QilinBlessing, CVG_QilinBlessed, CVG_Thought_QilinBlessed, CVG_Xeno_Qilin, CVG_Kind_Qilin
- `Common/Defs/Bloodline_Tortoise.xml` — GeneDef 10, HediffDef 2, BloodlineDef 1, AbilityDef 1, XenotypeDef 1, PawnKindDef 1: CVG_Bloodline_Tortoise, CVG_Blood_Tortoise_Trace, CVG_Blood_Tortoise_Diluted, CVG_Blood_Tortoise_Pure, CVG_Blood_Tortoise_Ancestral, CVG_Tortoise_Ridges, CVG_Tortoise_Eyes, CVG_Tortoise_Shell, CVG_Tortoise_ShellAncestral, CVG_Tortoise_Hair, CVG_Tortoise_Build, CVG_Bloodline_Tortoise, CVG_Ability_XuanwuShell, CVG_XuanwuShell, CVG_Xeno_Tortoise, CVG_Kind_Tortoise
- `Common/Defs/Names_Bloodlines.xml` — RulePackDef 10: CVG_NamerPerson_Dragon, CVG_NamerPerson_Dragon_Female, CVG_NamerPerson_Phoenix, CVG_NamerPerson_Phoenix_Female, CVG_NamerPerson_Qilin, CVG_NamerPerson_Qilin_Female, CVG_NamerPerson_Tortoise, CVG_NamerPerson_Tortoise_Female, CVG_NamerPerson_Fox, CVG_NamerPerson_Fox_Female

**Idiomas:** Spanish, SpanishLatin · **Texturas:** 87

**Código en el Core:** Source/Convergence.Core/ → Bloodline/ y Defs/Bloodline*.cs (linajes y genes)

## Convergence.Resources — Convergence: Resources

- packageId `prospero1088.convergence.resources`, versión 0.1.1
- Depende de: brrainz.harmony, prospero1088.convergence.core
- Carpetas que carga: /; Common; Mods/Odyssey  (si está Ludeon.RimWorld.Odyssey)

**Defs** (archivo → tipos y defNames):

- `Common/Defs/Bamboo.xml` — ThingDef 9, HediffDef 1: CVG_Bamboo, CVG_JadeBamboo, CVG_Plant_Bamboo, CVG_Plant_JadeBamboo, CVG_PeachTimber, CVG_Peach, CVG_Plant_PeachTree, CVG_Plant_CelestialPeach, CVG_ImmortalPeach, CVG_PeachBlessing
- `Common/Defs/Biomes.xml` — ThingDef 9, HediffDef 2, RecipeDef 1: CVG_SolarGoldSand, CVG_MineableSolarGold, CVG_SmeltSolarGold, CVG_BloodFruitWarmth, CVG_BloodFruit, CVG_Plant_BloodFruitBush, CVG_SpiritWood, CVG_Plant_SpiritTree, CVG_PoisonTempered, CVG_PoisonHerb, CVG_Plant_PoisonHerb, CVG_SpiritPearl
- `Common/Defs/Celestial.xml` — ThingDef 2, RecipeDef 1: CVG_MeteoricIron, CVG_StarSand, CVG_RefineMeteoricIron
- `Common/Defs/Floors.xml` — TerrainDef 5, DesignatorDropdownGroupDef 1: CVG_SpiritFloors, CVG_JadeTiles, CVG_SpiritStoneFloor, CVG_BambooFloor, CVG_SpiritWoodFloor, CVG_FiveElementMosaic
- `Common/Defs/Materials.xml` — ThingDef 7: CVG_SpiritSteel, CVG_MoonSilver, CVG_ChaosGold, CVG_ColdIron, CVG_SpiritCrystal, CVG_MineableSpiritCrystal, CVG_MineableColdIron
- `Common/Defs/Refining.xml` — RecipeDef 4, ThingDef 1: CVG_RefiningFurnace, CVG_RefineSpiritSteel, CVG_RefineMoonSilver, CVG_RefineChaosGold, CVG_RefineSpiritJade
- `Common/Defs/Rocks.xml` — ThingDef 20, RecipeDef 5, TerrainDef 5: CVG_TribulationRock, CVG_SmoothedTribulationRock, CVG_ChunkTribulationRock, CVG_BlocksTribulationRock, CVG_Make_StoneBlocksTribulationRock, CVG_TileTribulationRock, CVG_BlackJade, CVG_SmoothedBlackJade, CVG_ChunkBlackJade, CVG_BlocksBlackJade, CVG_Make_StoneBlocksBlackJade, CVG_TileBlackJade, CVG_CloudStone, CVG_SmoothedCloudStone, CVG_ChunkCloudStone, CVG_BlocksCloudStone, CVG_Make_StoneBlocksCloudStone, CVG_TileCloudStone, CVG_CelestialMarble, CVG_SmoothedCelestialMarble, CVG_ChunkCelestialMarble, CVG_BlocksCelestialMarble, CVG_Make_StoneBlocksCelestialMarble, CVG_TileCelestialMarble, CVG_VermilionStone, CVG_SmoothedVermilionStone, CVG_ChunkVermilionStone, CVG_BlocksVermilionStone, CVG_Make_StoneBlocksVermilionStone, CVG_TileVermilionStone
- `Mods/Odyssey/Defs/Biomes_Odyssey.xml` — ThingDef 14, HediffDef 2, RecipeDef 1, TerrainDef 1: CVG_FireCrystal, CVG_MineableFireCrystal, CVG_MoonJade, CVG_MineableMoonJade, CVG_BloodJade, CVG_MineableBloodJade, CVG_DragonObsidian, CVG_SmoothedDragonObsidian, CVG_ChunkDragonObsidian, CVG_BlocksDragonObsidian, CVG_Make_StoneBlocksDragonObsidian, CVG_TileDragonObsidian, CVG_FlameWarmth, CVG_FlameLotusPetal, CVG_Plant_FlameLotus, CVG_GlowSight, CVG_GlowCap, CVG_Plant_GlowMushroom
- `Mods/Odyssey/Defs/GalacticSteel.xml` — ThingDef 2: CVG_GalacticSteel, CVG_MineableGalacticSteel

**Parches:** `Common/Patches/Biomes.xml`, `Mods/Odyssey/Patches/Asteroids.xml`, `Mods/Odyssey/Patches/Biomes_Odyssey.xml`

**Idiomas:** Spanish, SpanishLatin · **Texturas:** 62

**Código en el Core:** Source/Convergence.Core/ → Resources/ (vetas, rocas wuxia)

## Convergence.Space — Convergence: Flying Sect

- packageId `prospero1088.convergence.space`, versión 0.1.1
- Depende de: brrainz.harmony, Ludeon.RimWorld.Odyssey, prospero1088.convergence.core, prospero1088.convergence.resources, prospero1088.convergence.formations
- Carpetas que carga: /; Common

**Defs** (archivo → tipos y defNames):

- `Common/Defs/Island.xml` — ThingDef 8, RecipeDef 2, ResearchProjectDef 1, TerrainDef 1: CVG_Research_FlightArray, CVG_CloudRock, CVG_FlightCore, CVG_Make_FlightCore_Gravcore, CVG_Make_FlightCore_Beasts, CVG_IslandHeart, CVG_GravityCoreMount, CVG_EssenceVessel, CVG_QiThruster, CVG_WindSail, CVG_HelmsmanAltar, CVG_ArkProtectiveFormation
- `Common/Defs/IslandSurvival.xml` — ThingDef 6: CVG_CloudRockWall, CVG_JadeWall, CVG_BreathFormation, CVG_QiBarrier, CVG_CelestialDome, CVG_SkyStake

**Parches:** `Common/Patches/LandingOutcomes.xml`, `Common/Patches/Stats.xml`

**Idiomas:** English, Spanish, SpanishLatin · **Texturas:** 22

**Código en el Core:** Source/Convergence.Core/ → Space/ (isla voladora, cúpula, vuelo)

## Convergence.Stories — Convergence: Stories & Scenarios

- packageId `prospero1088.convergence.stories`, versión 0.1.1
- Depende de: brrainz.harmony, prospero1088.convergence.core
- Carpetas que carga: /; Common; 1.6; Mods/Odyssey  (si está Ludeon.RimWorld.Odyssey); Mods/BiotechWuxia  (si está prospero1088.convergence.biotechwuxia); Mods/Factions  (si está prospero1088.convergence.factions); Mods/Sects  (si está prospero1088.convergence.sects)

**Defs** (archivo → tipos y defNames):

- `Common/Defs/Endgame.xml` — ThingDef 4, SecretRealmDef 4: CVG_FounderSeal, CVG_JiuxiaoShard, CVG_RuptureCore, CVG_HeavenKey, CVG_EndRealm_FounderTomb, CVG_EndRealm_JiuxiaoFragment, CVG_EndRealm_RuptureField, CVG_EndRealm_DeepestRealm
- `Common/Defs/Events.xml` — StoryEventDef 111, ThoughtDef 9: WanderingSwordsman, HermitTeaches, HungryBeggar, LostDiscipleChild, PeddlerInTheMist, LotusPilgrims, StrandedAlchemist, TravelingStorytellers, RunawayDisciple, RunawayEnforcers, RunawayThanks, BeastTamerWanders, StolenManual, CursedBlade, BladeWhispers, BladeExorcism, DemonBargain, BloodPactDue, WoundedEnemyBegs, PavilionTaxmen, AllianceProposal, RivalChallenge, JadeSlipTemptation, PoorVillageFamine, SacrificeOffer, QiDeviationPlague, QiMeteor, DemonicFog, BeastTideSmall, ThievingSpirits, QiLightning, PoisonedWell, CorruptedQiLeak, CollapsingCave, RiftTremor, NightOfTheHungryGhosts, SuddenEnlightenment, SpringOfJade, AncientCacheMap, CacheGuardian … (+80)
- `Common/Defs/SecretRealms.xml` — ThingDef 5, SecretRealmDef 4, GenStepDef 2, PawnKindDef 2, HediffDef 1, BiomeDef 1, MapGeneratorDef 1, IncidentDef 1, SitePartDef 1, QuestScriptDef 1: CVG_RealmGate, CVG_RealmExit, CVG_RealmLantern, CVG_FormationTrap, CVG_ForgottenSarcophagus, CVG_RealmSealPressure, CVG_SecretRealmBiome, CVG_SecretRealmMap, CVG_SecretRealmGen, CVG_Sanctuary_Disciple, CVG_Sanctuary_Elder, CVG_SecretRealm_JadeHollow, CVG_SecretRealm_ThunderTomb, CVG_SecretRealm_NetherLabyrinth, CVG_Sanctuary, CVG_Story_RealmGate, CVG_SecretRealmEntrance, CVG_SecretRealmEntrance, CVG_OpportunitySite_SecretRealm
- `Common/Defs/Stories.xml` — ScenarioDef 6, IncidentDef 5, ScenPartDef 5, ThingDef 2, LetterDef 1, HediffDef 1, ThoughtDef 1: CVG_StoryLetter, CVG_DamagedDantian, CVG_DantianElixir, CVG_Thought_Cursed, CVG_LightPillar, CVG_Story_CultistsFlee, CVG_Story_AllyPlea, CVG_Story_BeastMigration, CVG_Story_TreasureBirth, CVG_Story_ClanVengeance, CVG_StartingBloodline, CVG_StoryOrigin, CVG_ConvergenceOccurred, CVG_StartingCultivators, CVG_Scenario_LoneSurvivor, CVG_Scenario_SectFlight, CVG_Scenario_DestroyedFamily, CVG_Scenario_AscensionProject, CVG_Scenario_ClassicalEra, CVG_Scenario_BeastHunters, CVG_StartingPuppets
- `Mods/BiotechWuxia/Defs/Scenario_PuppetMaster.xml` — ScenarioDef 1: CVG_Scenario_PuppetMaster
- `Mods/Factions/Defs/Scenarios_Factions.xml` — ScenarioDef 3: CVG_Scenario_LostDisciples, CVG_Scenario_ExiledAlchemist, CVG_Scenario_LotusRefugees
- `Mods/Odyssey/Defs/Landmarks_Sects.xml` — RulePackDef 2, LandmarkDef 2, TileMutatorDef 1: CVG_DragonVein, CVG_NamerLandmark_DragonVein, CVG_NamerLandmark_SectRuins, CVG_DragonVeinLandmark, CVG_SectRuins
- `Mods/Sects/Defs/Scenarios_Sects.xml` — ScenarioDef 1: CVG_Scenario_DragonHeir

**Parches:** `Common/Patches/Sanctuary.xml`, `Mods/Odyssey/Patches/FlightCoreLoot.xml`

**Idiomas:** English, Spanish, SpanishLatin · **Texturas:** 15

**Código** (`Source/Convergence.Stories`):

- `ChargeFlowTest.cs`: GameComponent_ChargeFlowTest
- `Endgame.cs`: GameComponent_Endgame, SO_Ending
- `MechTabTest.cs`: GameComponent_MechTabTest
- `PuppetFightTest.cs`: GameComponent_PuppetFightTest
- `ScenPuppets.cs`: ScenPart_StartingPuppets
- `SecretRealms.cs`: RealmSpawn, RealmLoot, SecretRealmDef, DefModExtension_RealmGate, SecretRealmUtility, Building_RealmGate, MapComponent_SecretRealm, GameComponent_SecretRealms, GenStep_SecretRealm, Patch_Mineable_DestroyMined, IncidentWorker_RealmGate, GenStep_RealmGateSite, QuestNode_IfConvergence, SymbolResolver_Interior_ForgottenSanctuary
- `SectWorld.cs`: SectWar, SectPair, GameComponent_SectWorld, SO_SectWar
- `StoriesSelfTest.cs`: GameComponent_StoriesTest
- `Story.cs`: StoryFollowUp, Grudge, ScheduledStory, GameComponent_Story, ScenPart_StoryOrigin, ScenPart_ConvergenceOccurred, ScenPart_StartingCultivators, ScenPart_StartingBloodline, IngestionOutcomeDoer_RemoveHediffs, StoryUtility, Patch_Pawn_Kill_Grudge, StoriesStartup
- `StoryEngine.cs`: StoryEventDef, StoryOption, StoryTask, StoryContext, StoryOutcome, SO_Raid, SO_Goodwill, SO_Items, SO_TakeItems, SO_Join, SO_Visit, SO_Hediff, SO_Thought, SO_Mental, SO_Condition, SO_Beasts, SO_Flag, SO_Chain, SO_Progress, SO_Inspire, SO_Damage, SO_Skill, SO_Hunt, StoryEngine
- `StoryEvents.cs`: ChoiceLetter_Story, StoryEvents, IncidentWorker_Story, IncidentWorker_CultistsFlee, IncidentWorker_AllyPlea, IncidentWorker_ClanVengeance, IncidentWorker_BeastMigration, IncidentWorker_TreasureBirth
- `WorkFlowTest.cs`: GameComponent_WorkFlowTest

**Argumentos de autotest y herramientas:** `-cvgchargetest`, `-cvgmechtabtest`, `-cvgpuppetfighttest`, `-cvgstoriestest`, `-cvgworktest`

## Convergence.TraitsDestinies — Convergence: Traits & Destinies

- packageId `prospero1088.convergence.traitsdestinies`, versión 0.1.1
- Depende de: brrainz.harmony, prospero1088.convergence.core
- Carpetas que carga: /; Common; Mods/Biotech  (si está Ludeon.RimWorld.Biotech); Mods/Odyssey  (si está Ludeon.RimWorld.Odyssey)

**Defs** (archivo → tipos y defNames):

- `Common/Defs/Inspirations.xml` — InspirationDef 3: CVG_Inspired_Enlightenment, CVG_Inspired_SwordIntent, CVG_Inspired_ForgeFire
- `Common/Defs/Relations.xml` — PawnRelationDef 3, ThoughtDef 2: CVG_DaoPartner, CVG_SwornSibling, CVG_LifelongRival, CVG_Thought_DaoPartnerDied, CVG_Thought_SwornSiblingDied
- `Common/Defs/Thoughts_Personality.xml` — ThoughtDef 2: CVG_Thought_Seclusion, CVG_Thought_ArrogantRank
- `Common/Defs/Traits_Bodies.xml` — TraitDef 7: CVG_Body_IronSkin, CVG_Body_Saber, CVG_Body_Vajra, CVG_Body_IceBoneJade, CVG_Body_Thunder, CVG_Body_TenThousandPoisons, CVG_Body_BeastMarrow
- `Common/Defs/Traits_Comprehension.xml` — TraitDef 3: CVG_Comprehension, CVG_LateBloomer, CVG_EphemeralGenius
- `Common/Defs/Traits_Crafts.xml` — TraitDef 6: CVG_Craft_SpiritFarmer, CVG_Craft_InnateAlchemist, CVG_Craft_ArtifactForger, CVG_Craft_FormationMaster, CVG_Craft_BeastTamer, CVG_Craft_TalismanMaster
- `Common/Defs/Traits_Destinies.xml` — TraitDef 10: CVG_Destiny_Reincarnated, CVG_Destiny_Regressor, CVG_Destiny_Transmigrated, CVG_Destiny_SonOfHeaven, CVG_Destiny_Antagonist, CVG_Destiny_HeavenDefier, CVG_Destiny_LegendaryBeastBlood, CVG_Destiny_FortuneStar, CVG_Destiny_LoneStar, CVG_Destiny_TreasureMagnet
- `Common/Defs/Traits_Hearts.xml` — TraitDef 7: CVG_Heart_Sword, CVG_Heart_SaberIntent, CVG_Heart_Buddha, CVG_Heart_Demonic, CVG_Heart_Child, CVG_Heart_Stone, CVG_Heart_Restless
- `Common/Defs/Traits_Personality.xml` — TraitDef 6: CVG_RighteousHero, CVG_ArrogantYoungMaster, CVG_HiddenExpert, CVG_ThirstForVengeance, CVG_LoyalDisciple, CVG_SecludedCultivator
- `Common/Defs/Traits_Roles.xml` — TraitDef 4: CVG_Role_SectDisciple, CVG_Role_LineageHeir, CVG_Role_SpiritTailor, CVG_Role_FengShuiMaster
- `Common/Defs/Traits_Souls.xml` — TraitDef 5: CVG_Soul_ReincarnatedCultivator, CVG_Soul_Ancient, CVG_Soul_Strong, CVG_Soul_Divided, CVG_Soul_Sealed
- `Mods/Biotech/Defs/Traits_Roles_Biotech.xml` — TraitDef 2: CVG_Role_PuppetMaster, CVG_Role_BloodChild
- `Mods/Odyssey/Defs/Traits_Roles_Odyssey.xml` — TraitDef 1: CVG_Role_SpiritAngler

**Parches:** `Common/Patches/SpiritCrafts_VanillaBenches.xml`, `Common/Patches/StoneHeart_Grief.xml`

**Idiomas:** Spanish, SpanishLatin · **Texturas:** 0

**Código en el Core:** Source/Convergence.Core/ → Traits/ (mecánicas, progresión, conflictos de rasgos)

