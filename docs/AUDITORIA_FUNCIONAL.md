# Auditoría funcional

Generado por `tools/audit_funcional.py` (solo lectura). Convergence.Sects no se audita.

| Gravedad | Sección | Hallazgos |
|---|---|---|
| ALTA | Dice que sube la concentración de qi y no tiene con qué | 1 |
| MEDIA | Código que ningún XML usa (la función nunca se activa) | 3 |
| MEDIA | Objetos que no se consiguen de ninguna forma | 2 |
| MEDIA | Recetas que no están en ninguna mesa | 9 |
| BAJA | Objetos con valor 0 | 4 |

## [ALTA] Dice que sube la concentración de qi y no tiene con qué

- **Convergence.Formations** `CVG_QiGatheringCore` — sin DefModExtension_FengShui ni comp de qi

## [MEDIA] Código que ningún XML usa (la función nunca se activa)

- **Convergence.Core** `RitualObligationTrigger_Admission` — clase RitualObligationTrigger_Admission : RitualObligationTrigger en Source\Convergence.Core\Ideology\SectRituals.cs
- **Convergence.Core** `RitualObligationTrigger_AdmissionProperties` — clase RitualObligationTrigger_AdmissionProperties : RitualObligationTriggerProperties en Source\Convergence.Core\Ideology\SectRituals.cs
- **Convergence.Core** `RitualObligationTrigger_AscensionProperties` — clase RitualObligationTrigger_AscensionProperties : RitualObligationTriggerProperties en Source\Convergence.Core\Ideology\SectRituals.cs

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

## [BAJA] Objetos con valor 0

- **Convergence.Stories** `CVG_FounderSeal` — MarketValue 0
- **Convergence.Stories** `CVG_HeavenKey` — MarketValue 0
- **Convergence.Stories** `CVG_JiuxiaoShard` — MarketValue 0
- **Convergence.Stories** `CVG_RuptureCore` — MarketValue 0
