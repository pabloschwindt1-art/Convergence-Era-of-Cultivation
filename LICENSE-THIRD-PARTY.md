# Third-party and RimWorld-derived material

Project: Convergence: Era of Cultivation (15 mods). Author: Prospero1088. Last review: 2026-10-20.

The project license scheme is in [LICENSES.md](LICENSES.md): code, scripts and technical content are MIT; original art, lore and narrative text are
CC BY-NC-SA 4.0. **Nothing listed in this file is licensed by the author under MIT or CC BY-NC-SA.** It is listed here so that no license
is attributed to material that is not the author's to license.

## 1. RimWorld (Ludeon Studios)

RimWorld, its DLCs, its art, sounds, texts and trademarks belong to Ludeon Studios Inc. These mods are unofficial and are not
endorsed by Ludeon. Notice carried in every mod's `About.xml` and `LICENSE.txt`:

> Portions of the materials used to create this content/mod are trademarks and/or copyrighted works of Ludeon Studios Inc.
> All rights reserved by Ludeon. This content/mod is not official and is not endorsed by Ludeon.

### 1.0 What the RimWorld EULA says (read 2026-10-20 at https://rimworldgame.com/eula/)

The EULA is the only license that governs Ludeon's art; the RimWorld Art Source folder that Ludeon publishes for modders contains no
license file of its own, and the RimWorld Wiki describes it as the place to find the vanilla textures "for modders to reference" and to
start apparel from a vanilla template. Sections *Things you create using our software* and *Mods*, in short:

- Mods and other derivative works are allowed if they follow the EULA. **The creator owns the derivative works he or she creates.**
- Ludeon's resources may be used "as a basis or reference for a Mod". They may not be ripped out and passed around independently
  (the original files cannot be redistributed).
- A Mod must be "something original" that **does not contain a substantial part of Ludeon's Software, including its artwork, lore or
  other content**. Ludeon has the final say on what counts as a Mod.
- Mods must be free, without ads or calls to action; donations may be accepted only if they are not tied to the mod. They must only work
  with a registered copy of the game and must be presented as community content, with the disclaimer quoted above.
- By creating a derivative work the creator grants Ludeon a perpetual, irrevocable license to use, modify, distribute and sublicense it.
  Nothing in this project's licenses limits that license.

### 1.1 Files in this project that are derived from Ludeon artwork

145 shipped files are derivative works made over artwork from the RimWorld Art Source (the silhouette and shading of the vanilla piece
were the base and details were painted over). **They may be distributed in free mods under the EULA, but only as derivative works:**
Ludeon's rights in the underlying artwork remain, the author's added work is not offered separately under MIT or CC BY-NC-SA, and they
may be used only inside RimWorld (not extracted and reused elsewhere).

| Mod | Files | Location (inside the mod folder) | Made over |
|---|---|---|---|
| Convergence: Apparel | 125 | `Common/Textures/Things/Pawn/Humanlike/Apparel/CVG/` | vanilla robe, flak vest and hat artwork |
| Convergence: Qi-Tech | 16 | `Common/Textures/Things/Pawn/Humanlike/Apparel/CVG/QiTechExosuit/` | vanilla recon armor artwork |
| Convergence: Flying Sect | 4 | `Common/Textures/Things/Building/CVG/Island/CloudRockWall_Atlas.png`, `CloudRockWall_MenuIcon.png`, `JadeWall_Atlas.png`, `JadeWall_MenuIcon.png` | vanilla Odyssey gravship hull atlas |

The scripts and intermediate files that produced them are part of the author's private working files (they read Ludeon's Art Source
as input) and are not published in this repository.

**How close they are to the originals** (measured 2026-10-20: silhouette overlap and correlation of the shading with the matching Art
Source file):

| Group | Silhouette overlap | Shading correlation | Reading |
|---|---|---|---|
| Worn clothing (Apparel) | 100 % | 0.94 median (0.88 to 0.99) | close derivative: same body fit and shading, new details and colors |
| Exoskeleton (Qi-Tech) | 100 % | 0.94 median (0.77 to 0.99) | close derivative |
| Island wall atlas, cloud rock | n/a (full atlas) | 0.76 | partly derived |
| Island wall atlas, jade | n/a (full atlas) | 0.14 | mostly original |

**Conclusion.** Using the Art Source as the base of a mod is expressly allowed, and distributing the result in a free mod with the notice is
allowed. The only open question is the "substantial part" limit, which Ludeon decides: the 145 files are about 17 % of this project's
textures but a very small fraction of RimWorld's artwork, and none is a copy. The riskiest group is the worn clothing and the exoskeleton
(identical silhouette and very similar shading, as worn clothing has to fit the same bodies). Decision: **no need to replace them all.**
The first publication wave (Core, Formations, Races, Stories) contains none of them. Before publishing Apparel, Qi-Tech and Flying Sect:
the clothing art is already planned for a redesign (so it would be replaced anyway), the island atlases are waiting for their original
design sheet, and the exoskeleton can be redrawn on its own if Ludeon ever objects. Keep the notice and keep the mods free and ad-free.

How the list was verified (2026-10-20): every script that reads the Art Source was traced to its output folder, and the silhouette of each of
the 864 shipped textures was compared with the 5,202 Art Source images that have a non-trivial silhouette (overlap >= 93 %). The matches
outside the three folders above were round icons that coincide with Ludeon's dot and disc images (pills, gene medallions, faction
emblems, a mask) and are original.

### 1.2 RimWorld assets the mods refer to by name (not included)

The XML points to vanilla textures and defs; the files themselves are not in this project. They belong to Ludeon:
ore vein atlas (`RockFlecked_Atlas`, tinted), tinted vanilla floor textures, dried corpse variants of vanilla animals, vanilla
projectile and shield-bubble art, vanilla eye textures (tinted for bloodlines), vanilla menu icons (barricade, smoothed wall) and the
base defs the mods inherit from (`ParentName`).

## 2. Libraries

| Library | License | Use |
|---|---|---|
| Harmony (Andreas Pardeike), mod `brrainz.harmony` | MIT | Required at runtime. Installed by the player from the Steam Workshop; **not** included in any mod folder (`0Harmony.dll` is excluded). |
| Lib.Harmony (NuGet) and Krafs.Rimworld.Ref (NuGet) | MIT / reference assemblies | Build time only. Not distributed. |

## 3. Studied, not copied

- The author keeps other authors' mods locally for study (`referencias/`). They are not part of this project, are not in this
  repository, are not distributed and keep the licenses of their authors. No code or art was copied from it (project rule). Check of 2026-10-20: none of the 864 shipped
  textures is byte-identical to any of the 4,474 images in `referencias/`, and no source file carries a third-party copyright or
  "adapted from" notice.
- The cost and cooldown figures of the techniques were taken as reference values from Vanilla Psycasts Expanded (numbers only; no
  code, art or text).
- Ideas (systems, structure) studied from Tale of Immortal and from other cultivation mods are not material copied from them; the
  names, lore and designs in this project were written for it.
- Optional XML patch for Dubs Bad Hygiene (`Mods/Convergence.Core/Mods/DubsBadHygiene/Patches`): only XML written for this project.

## 4. AI assistance

The art of this project was created with the assistance of generative AI tools, then selected, edited, cut and placed by the author:

- ChatGPT (OpenAI) and Gemini (Google) image generation, from the author's own briefs and designs.
- Stable Diffusion XL base 1.0 (CreativeML Open RAIL++-M license) and FLUX.1 schnell (Apache-2.0), run locally through ComfyUI, for
  candidates and drafts.
- Procedural scripts of the author's working files (not published here).

Outputs are subject to each provider's terms. AI-generated material may not be protected by copyright in some jurisdictions; the
licenses in [LICENSES.md](LICENSES.md) grant only the rights the author holds. The code was also written with the help of an AI coding
assistant under the author's direction (see the `Co-Authored-By` lines in the git history).
