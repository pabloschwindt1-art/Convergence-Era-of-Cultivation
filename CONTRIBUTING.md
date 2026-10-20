# Contributing to Convergence: Era of Cultivation

Thank you for your interest. This project is a xianxia / wuxia cultivation mod for RimWorld 1.6, split into 15 mods (a Core plus 14 modules).
Bug reports, translations, art, code and new addons are all welcome. The maintainer is **Prospero1088**.

Most of the project documentation is in Spanish ([`docs/`](docs/)). You do not need to read Spanish to contribute: this guide has what you need, and
you can write issues and pull requests in English (or Spanish).

## Ways to help

| I want to... | Start here |
|---|---|
| Report a bug | [Open a bug report](https://github.com/pabloschwindt1-art/Convergence-Era-of-Cultivation/issues/new/choose) |
| Report a problem with another mod | [Open a compatibility report](https://github.com/pabloschwindt1-art/Convergence-Era-of-Cultivation/issues/new/choose) |
| Suggest something | [Open a suggestion](https://github.com/pabloschwindt1-art/Convergence-Era-of-Cultivation/issues/new/choose) |
| Translate the mods | [Translations](#translations) |
| Draw or improve art | [Art](#art) |
| Fix a bug or add a feature | [Code and content](#code-and-content) |
| Make an addon on top of the Core | [New addons](#new-addons) |

Before opening an issue, search the existing ones. For a bug, first check that it still happens with **only Harmony, your DLCs and the Convergence mods**
(no other mods): it makes it much faster to fix. Attach the relevant lines of `Player.log` (the file is in
`%USERPROFILE%/AppData/LocalLow/Ludeon Studios/RimWorld by Ludeon Studios/`). Remove anything private from it first (user name in paths, etc.).

## Project principles

These are design decisions of the project. Please follow them, and talk to the maintainer in an issue before proposing to change one.

- **Everything has lore.** Every item, building, graft, technique, trait, gene, beast and event has a 2-3 sentence description with its origin or legend,
  written in English in the def and translated through `DefInjected`. See [`docs/LORE.md`](docs/LORE.md) and [`docs/lore/`](docs/lore/) for the tone.
- **The wuxia version of what already works.** Before inventing a system, find its vanilla or DLC equivalent and make its wuxia version with the same
  structure (electricity becomes the qi network, the mechanitor becomes the puppet master, the gravship becomes the flying island, psycasts become techniques).
  Integrate with the existing game systems and DLCs; do not build parallel ones ([`docs/INTEGRACION.md`](docs/INTEGRACION.md)).
- **With and without technology.** The Core never requires electricity or industrial research; anything technological goes in Qi-Tech or is marked `TechOnly`.
- **Only Harmony.** No other mod is a hard dependency. Every DLC feature lives in a conditional folder (`Mods/<DLC>` with `IfModActive` in `LoadFolders.xml`) or uses
  `MayRequire`, so each mod still works without that DLC.
- **Data in XML, logic in C#.** New content must be addable with XML. No closed enums for content. Prefer XML over code in the modules.
- **No AssetBundles, no `GL`, no `Shader.Find`.** Use vanilla flecks/effecters and `renderNodeProperties`.

## Code and content

Requirements: .NET SDK 8, RimWorld 1.6 and Git. The projects target `net472` and restore `Krafs.Rimworld.Ref` and `Lib.Harmony` from NuGet.

```
git clone https://github.com/pabloschwindt1-art/Convergence-Era-of-Cultivation.git
cd Convergence-Era-of-Cultivation
dotnet build Source/Convergence.Core/Convergence.Core.csproj -c Release       # the Core first
dotnet build Source/Convergence.Formations/Convergence.Formations.csproj -c Release
dotnet build Source/Convergence.Grafts/Convergence.Grafts.csproj -c Release
dotnet build Source/Convergence.QiTech/Convergence.QiTech.csproj -c Release
dotnet build Source/Convergence.Stories/Convergence.Stories.csproj -c Release
powershell -ExecutionPolicy Bypass -File tools/link-mods.ps1                  # junctions into <RimWorld>/Mods
```

Conventions:

- Def names use the prefix `CVG_`; the C# namespace is `Convergence`. Each module lives in `Mods/Convergence.<Name>/` (plus `Source/Convergence.<Name>/` only if it needs code),
  depends on the Core and never the other way around.
- Texts are never hard-coded: use `Keyed` (code strings) and `DefInjected` (labels and descriptions), English plus Spanish and Spanish (Latin America).
  A def that only exists with another mod or DLC (`MayRequire`) must have its translations in the matching conditional folder, not in `Common`.
- Keep Harmony patches small and scoped to our own defs or components. Do nothing heavy per tick (use `CompTickRare`, `IsHashIntervalTick`).
- Do not touch balance or mechanics in a translation, art or documentation pull request.
- A new automatic test must be inert unless its command-line argument is passed (`-cvg...test`) and must not leave messages or changes in normal games.

Check your work before opening a pull request:

```
python -I tools/check-workshop.py            # About, LoadFolders, previews, dependencies, licenses (needs the built DLLs)
python -I tools/make-licenses.py --check     # license files are up to date
powershell -ExecutionPolicy Bypass -File tools/run-test.ps1 -GameArgs "-quicktest -cvgselftest"   # isolated RimWorld test instance
```

`tools/run-test.ps1` starts a separate RimWorld instance with its own data folder (it never touches your `Prefs.xml` or `ModsConfig.xml`), loads the 15 mods with all
DLCs and closes itself. Run one `-cvg...test` per launch, only one game at a time, and read the log at the end. See
[`docs/HERRAMIENTAS_Y_ENTORNO.md`](docs/HERRAMIENTAS_Y_ENTORNO.md) and [`docs/DISENO_CORE.md`](docs/DISENO_CORE.md) for the architecture and the available tests.
Optional: `tools/DefValidator` checks the Defs against the real RimWorld types.

## Translations

English is the source language. The defs carry their English text inline, and each mod has its translations under
`Mods/Convergence.<Name>/Common/Languages/<Language>/`:

- `Keyed/` for strings used by the code (the keys start with `CVG_`). English lives here too.
- `DefInjected/<DefType>/` for the labels and descriptions of defs (`<CVG_DefName.label>`, `<CVG_DefName.description>`).

To add a language, create the folder with the name RimWorld uses for it (for example `French`, `German`, `Russian`, `ChineseSimplified`) in each mod, copy the structure of
`Spanish/` and translate the values (never the keys or the tags). Please keep the lore tone. Translate the Core first; every module is a separate mod, so a partial
translation is fine. Mention in the pull request which mods you covered. Translations are text content and fall under CC BY-NC-SA 4.0 (see [Licensing](#licensing)).

## Art

The art style is clean vector cartoon: a thick, even dark outline, flat cel shading, light from the top left, silhouettes readable at about 64 px, and no cast shadow on
the ground. Sizes and file naming (`_south`, `_east`, `_north` views, 128 px items, 64 px icons, grayscale for tinted textures) are in [`docs/ARTE.md`](docs/ARTE.md).
Mod previews are `About/Preview.png`, 640x360 and under 1 MB.

Rules for art contributions:

- **Say how it was made.** The project's art was created with the assistance of AI tools. AI-assisted art is accepted, but say so in the pull request, with the tool.
  Do not submit art you cannot license.
- **Original art only.** Do not copy art from other mods or from RimWorld. Art made over RimWorld's own artwork (for example its Art Source) is a derivative of
  Ludeon's work and is **not** covered by our licenses; it needs maintainer approval and goes in [`LICENSE-THIRD-PARTY.md`](LICENSE-THIRD-PARTY.md).
- Contributed art is licensed under CC BY-NC-SA 4.0 like the rest of the project's art.

## New addons

You can extend Convergence with your own mod. The Core is meant to be extended with XML: new realms (`CultivationRealmDef`), cultivation paths (`CultivationPathDef`),
techniques (a vanilla `AbilityDef` plus `Convergence.TechniqueExtension`), technique manuals, bloodlines (`BloodlineDef`), story events (`StoryEventDef`), sect factions and ranks.
The extension points are listed in [`docs/WORKSHOP.md`](docs/WORKSHOP.md) (section 4.2) and [`docs/DISENO_CORE.md`](docs/DISENO_CORE.md).

- **A separate mod (recommended for third parties).** Make your own mod with your own `packageId` and your own def prefix (not `CVG_`), list the Core as a dependency in
  `About.xml` (`prospero1088.convergence.core`) and `loadAfter`, and publish it where you like. Open an issue if you want it linked from this repository.
- **A module in this repository.** Open a suggestion first. A module is a folder `Mods/Convergence.<Name>/` with `About/About.xml`, `LoadFolders.xml`, `Common/Defs`, `Common/Languages`
  (English, Spanish, Spanish Latin) and a `Preview.png`, depending on the Core (plus Resources or Formations only when needed). Run `tools/check-workshop.py` and add the module to the
  lists in the README and in `tools/make-licenses.py` if it needs a special notice.

## Pull requests

1. Open (or comment on) an issue for anything bigger than a small fix, so we can agree on the approach.
2. Fork, create a branch, and keep the pull request about one thing. Small commits with short imperative English messages (`Fix ...`, `Add ...`).
3. Fill in the pull request template: what changed, how you tested it, and the licensing and AI-assistance notes.
4. Do not commit build output (`obj/`, `bin/`, DLLs), working art files, screenshots of the game, or other authors' mods; `.gitignore` already excludes most of it.
5. The maintainer reviews pull requests when there is time; please be patient.

## Licensing

By contributing you agree that your contribution is released under the license of the part of the project it belongs to (summary in [`LICENSES.md`](LICENSES.md)):

- **Code, scripts and technical content** (C#, the technical data in XML, tools, technical documentation): **MIT** ([`LICENSE`](LICENSE)).
- **Original art, lore and narrative text** (textures, descriptions, story and event texts, translations, creative documentation): **CC BY-NC-SA 4.0**
  ([`LICENSE-CC-BY-NC-SA-4.0.txt`](LICENSE-CC-BY-NC-SA-4.0.txt)): credit "Prospero1088, Convergence: Era of Cultivation", non-commercial, adaptations under the same license.
- Anything derived from RimWorld or from third parties is listed in [`LICENSE-THIRD-PARTY.md`](LICENSE-THIRD-PARTY.md) and is not licensed by us.

You must have the right to submit what you contribute. Because of the [RimWorld EULA](https://rimworldgame.com/eula/), the mods have to stay **free** and must not contain
advertising or calls to donate. Do not add other authors' code or art. If you used an AI tool, say so in the pull request.

## Be kind

Be respectful and assume good faith. Harassment, personal attacks and discrimination are not tolerated and can lead to being blocked from the project.
