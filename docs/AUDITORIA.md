# Auditoría de nombres y referencias

Generado por `tools/audit.py` (solo lectura). Convergence.Sects está descontinuado y no se audita.

| Sección | Hallazgos |
|---|---|
| 1. Menciones a lo que ya no existe | 21 |
| 4. Sin etiqueta en español | 2 |

## 1. Menciones a lo que ya no existe

### Convergence.Apparel (16)
- `Mods/Convergence.Apparel/About/About.xml:13` — "Convergence: Sects" — el mod Sects (descontinuado): Cloud cotton (a crop) and cloud cloth woven on a spirit loom, spirit silk, sect robes for outer disciples, inner disciples and elders in the colors of each sect
- `Mods/Convergence.Apparel/Patches/SectRobes.xml:3` — "Convergence: Sects" — el mod Sects (descontinuado): <!-- Sect members wear the robe of their rank in the color of their sect. Only if Convergence: Sects is loaded. -->
- `Mods/Convergence.Apparel/Patches/SectRobes.xml:6` — "Convergence: Sects" — el mod Sects (descontinuado): <li>Convergence: Sects</li>
- `Mods/Convergence.Apparel/Patches/SectRobes.xml:11` — "CVG_AzurePeak_Disciple" — def de la secta vieja: <xpath>/Defs/PawnKindDef[defName="CVG_AzurePeak_Disciple"]</xpath>
- `Mods/Convergence.Apparel/Patches/SectRobes.xml:18` — "CVG_AzurePeak_Bearer" — def de la secta vieja: <xpath>/Defs/PawnKindDef[defName="CVG_AzurePeak_Bearer"]</xpath>
- `Mods/Convergence.Apparel/Patches/SectRobes.xml:25` — "CVG_AzurePeak_Blade" — def de la secta vieja: <xpath>/Defs/PawnKindDef[defName="CVG_AzurePeak_Blade"]</xpath>
- `Mods/Convergence.Apparel/Patches/SectRobes.xml:32` — "CVG_AzurePeak_Elder" — def de la secta vieja: <xpath>/Defs/PawnKindDef[defName="CVG_AzurePeak_Elder"]</xpath>
- `Mods/Convergence.Apparel/Patches/SectRobes.xml:39` — "CVG_AzurePeak_Swordsmith" — def de la secta vieja: <xpath>/Defs/PawnKindDef[defName="CVG_AzurePeak_Swordsmith"]</xpath>
- `Mods/Convergence.Apparel/Patches/SectRobes.xml:46` — "CVG_SilentLotus_Lay" — def de la secta vieja: <xpath>/Defs/PawnKindDef[defName="CVG_SilentLotus_Lay"]</xpath>
- `Mods/Convergence.Apparel/Patches/SectRobes.xml:53` — "CVG_SilentLotus_Monk" — def de la secta vieja: <xpath>/Defs/PawnKindDef[defName="CVG_SilentLotus_Monk"]</xpath>
- `Mods/Convergence.Apparel/Patches/SectRobes.xml:60` — "CVG_SilentLotus_Guardian" — def de la secta vieja: <xpath>/Defs/PawnKindDef[defName="CVG_SilentLotus_Guardian"]</xpath>
- `Mods/Convergence.Apparel/Patches/SectRobes.xml:67` — "CVG_SilentLotus_Venerable" — def de la secta vieja: <xpath>/Defs/PawnKindDef[defName="CVG_SilentLotus_Venerable"]</xpath>
- `Mods/Convergence.Apparel/Patches/SectRobes.xml:74` — "CVG_SilentLotus_Herbalist" — def de la secta vieja: <xpath>/Defs/PawnKindDef[defName="CVG_SilentLotus_Herbalist"]</xpath>
- `Mods/Convergence.Apparel/Patches/SectRobes.xml:116` — "CVG_AncientDragon_Watcher" — def de la secta vieja: <xpath>/Defs/PawnKindDef[defName="CVG_AncientDragon_Watcher"]</xpath>
- `Mods/Convergence.Apparel/Patches/SectRobes.xml:123` — "CVG_AncientDragon_ClanElder" — def de la secta vieja: <xpath>/Defs/PawnKindDef[defName="CVG_AncientDragon_ClanElder"]</xpath>
- `Mods/Convergence.Apparel/Patches/SectRobes.xml:130` — "CVG_AncientDragon_Hoarder" — def de la secta vieja: <xpath>/Defs/PawnKindDef[defName="CVG_AncientDragon_Hoarder"]</xpath>

### Convergence.Core (1)
- `Mods/Convergence.Core/Common/Defs/HistoryEvents_Sect.xml:3` — "Convergence: Sects" — el mod Sects (descontinuado): <!-- Cultivation events recorded by the Core (SectEvents); the sect precepts of Convergence: Sects (Ideology) react to them. -->

### Convergence.Factions (2)
- `Mods/Convergence.Factions/About/About.xml:22` — "Convergence: Sects" — el mod Sects (descontinuado): Requires Convergence: Era of Cultivation, Royalty and Ideology. Replaces Convergence: Sects (do not use both).
- `Mods/Convergence.Factions/Common/Defs/RogueCultivators.xml:6` — "Convergence: Sects" — el mod Sects (descontinuado): Brought over from the old Convergence: Sects.

### Convergence.Stories (2)
- `Source/Convergence.Stories/StoryEngine.cs:115` — "Pico Azur" — secta vieja (Pico Azur → Espada Sin Forma): // Convergence: Factions. "azurepeak" y "lotus" quedan como alias de partidas viejas: el Pico Azur pasó a la Espada Sin
- `Source/Convergence.Stories/StoryEngine.cs:116` — "Loto Silencioso" — secta vieja (Loto Silencioso → Caldero): // Forma y el Loto Silencioso al Caldero Celestial.


## 4. Sin etiqueta en español

### Convergence.Factions (2)
- `Mods/Convergence.Factions/Common/Defs/Ideology/SectIdeoIcons.xml` — Spanish: CVG_IdeoColor_Natural (ColorDef)
- `Mods/Convergence.Factions/Common/Defs/Ideology/SectIdeoIcons.xml` — SpanishLatin: CVG_IdeoColor_Natural (ColorDef)

