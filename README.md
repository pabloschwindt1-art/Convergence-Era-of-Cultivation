# Convergence: Era of Cultivation

*A fragment of the Nine Heavens collides with your rimworld. Qi begins to flow, and the Era of Cultivation begins.*

A xianxia / wuxia cultivation mod for **RimWorld 1.6**, inspired by *Tale of Immortal*: spirit roots, cultivation realms, heavenly tribulations,
techniques, sects, bloodlines, puppets, formations and a floating sect island. Play it **with technology** (techno-cultivation) or **without it**
(classic cultivation): the Core never requires electricity or industrial research. It is split into **15 mods** (a Core plus 14 modules) that you can
install separately.

Version **0.1.0** (early release). Nascent Soul and higher realms are not in yet.

## The 15 mods

| Mod | Folder | Also requires (besides Harmony and the Core) |
|---|---|---|
| Convergence: Era of Cultivation | `Mods/Convergence.Core` | - |
| Convergence: Alchemy & Artifacts | `Mods/Convergence.Alchemy` | Convergence: Resources |
| Convergence: Apparel | `Mods/Convergence.Apparel` | Convergence: Resources |
| Convergence: Biotech Wuxia | `Mods/Convergence.BiotechWuxia` | Biotech, Convergence: Resources, Convergence: Formations |
| Convergence: Fauna & Flora | `Mods/Convergence.FaunaFlora` | - |
| Convergence: Flying Sect | `Mods/Convergence.Space` | Odyssey, Convergence: Resources, Convergence: Formations |
| Convergence: Formations | `Mods/Convergence.Formations` | - |
| Convergence: Furniture | `Mods/Convergence.Furniture` | - |
| Convergence: Grafts | `Mods/Convergence.Grafts` | - |
| Convergence: Qi-Tech | `Mods/Convergence.QiTech` | Convergence: Formations |
| Convergence: Races | `Mods/Convergence.Races` | Biotech |
| Convergence: Resources | `Mods/Convergence.Resources` | - |
| Convergence: Sects | `Mods/Convergence.Sects` | - |
| Convergence: Stories & Scenarios | `Mods/Convergence.Stories` | - |
| Convergence: Traits & Destinies | `Mods/Convergence.TraitsDestinies` | - |

Harmony is the only external requirement. The DLCs are optional unless listed above: every DLC feature lives in a conditional folder
(`LoadFolders.xml`, `IfModActive`) so the mods work without it. Load order: Harmony, the DLCs, the Core, then the modules (let RimWorld auto-sort).

## Install

- **Steam Workshop:** coming soon (links will be added here).
- **From a release:** download the zips of the [Releases](https://github.com/pabloschwindt1-art/Convergence-Era-of-Cultivation/releases) page and extract the `Convergence.*` folders into `<RimWorld>/Mods/`.
  Subscribe to [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077) first.

## Build from source

Requirements: .NET SDK 8 and RimWorld 1.6 (the NuGet packages `Krafs.Rimworld.Ref` and `Lib.Harmony` are restored automatically; Harmony is not copied into the mods).

```
dotnet build Source/Convergence.Core/Convergence.Core.csproj -c Release
dotnet build Source/Convergence.Formations/Convergence.Formations.csproj -c Release
dotnet build Source/Convergence.Grafts/Convergence.Grafts.csproj -c Release
dotnet build Source/Convergence.QiTech/Convergence.QiTech.csproj -c Release
dotnet build Source/Convergence.Stories/Convergence.Stories.csproj -c Release
```

Build the Core first (the other projects reference it). The DLLs go to `Mods/<Mod>/1.6/Assemblies/`. To play from the repository, link the mods into the
game with `tools/link-mods.ps1` (junctions) or copy the folders into `<RimWorld>/Mods/`.

## Tools

| Tool | Use |
|---|---|
| `tools/run-test.ps1` | Launches an isolated RimWorld test instance (`-quicktest`, autotests such as `-cvgselftest`); see `docs/HERRAMIENTAS_Y_ENTORNO.md` |
| `tools/check-workshop.py` | Pre-publication check of the 15 mods (About, LoadFolders, previews, licenses) |
| `tools/make-licenses.py` | Generates the license files (`--check` verifies them) |
| `tools/package-release.py` | Builds the release zips (one per mod and one with everything) |
| `tools/workshop-link-deps.py` | Fills in the Workshop URLs of the dependencies after each upload |
| `tools/DefValidator/` | Validates the Defs against the real RimWorld types |

## Documentation

Spanish documentation in [`docs/`](docs/): [lore](docs/LORE.md), [design](docs/DISENO.md), [Core technical design](docs/DISENO_CORE.md),
[vanilla/DLC integration](docs/INTEGRACION.md) and the [technical documentation and publication guide](docs/WORKSHOP.md). Contributors: see [CLAUDE.md](CLAUDE.md).

## License

Each kind of material has its own license (summary in [LICENSES.md](LICENSES.md)):

- **Code, scripts and technical content: MIT** ([LICENSE](LICENSE)).
- **Original art, lore and narrative text: CC BY-NC-SA 4.0** ([LICENSE-CC-BY-NC-SA-4.0.txt](LICENSE-CC-BY-NC-SA-4.0.txt)): credit Prospero1088, non-commercial, share-alike.
- **Material derived from RimWorld (Ludeon) and third-party material is not licensed by the author**: see [LICENSE-THIRD-PARTY.md](LICENSE-THIRD-PARTY.md).
- **The art was created with the assistance of AI** tools, then selected, edited and placed by the author.

Each folder under `Mods/` carries its own `LICENSE.txt` and a copy of the CC BY-NC-SA text. Mods must stay free and ad-free (RimWorld EULA).

*Portions of the materials used to create this content/mod are trademarks and/or copyrighted works of Ludeon Studios Inc. All rights reserved by Ludeon. This content/mod is not official and is not endorsed by Ludeon.*

---

## Español

Mod de cultivo xianxia / wuxia para **RimWorld 1.6**, inspirado en *Tale of Immortal*: raíces espirituales, reinos de cultivo, tribulaciones celestiales, técnicas, sectas,
linajes de bestia, marionetas, formaciones y una isla voladora. Se juega **con tecnología** (tecno-cultivo) o **sin ella** (cultivo clásico): el Core nunca exige
electricidad ni investigación industrial. Son **15 mods** (un Core y 14 módulos) que se instalan por separado. Versión **0.1.0** (acceso anticipado).

- **Instalar:** Steam Workshop (próximamente) o los zips de [Releases](https://github.com/pabloschwindt1-art/Convergence-Era-of-Cultivation/releases) dentro de `<RimWorld>/Mods/`. Harmony es el único requisito externo; Races y Biotech Wuxia
  piden Biotech, y Flying Sect pide Odyssey.
- **Compilar:** los 5 comandos `dotnet build` de arriba (el Core primero). Los DLL salen en `Mods/<Mod>/1.6/Assemblies/`.
- **Probar:** `tools/run-test.ps1` (instancia aislada del juego) y `tools/check-workshop.py` (revisión previa a publicar).
- **Documentación:** carpeta [`docs/`](docs/) (lore, diseño, integración con el juego base y las DLC, guía de publicación). Para colaborar, leé [CLAUDE.md](CLAUDE.md).
- **Licencias:** código, scripts y contenido técnico **MIT**; arte original, lore y textos narrativos **CC BY-NC-SA 4.0**; lo derivado de RimWorld y de terceros no lo licencia el autor
  ([LICENSES.md](LICENSES.md), [LICENSE-THIRD-PARTY.md](LICENSE-THIRD-PARTY.md)). El arte se hizo con asistencia de IA.
