# Textos para las páginas del Workshop (BBCode de Steam, en inglés)

Cómo usarlos: tras subir un mod desde el juego, abrir su página, **Editar** y pegar el título y el bloque de descripción. Steam usa BBCode
(`[h1]`, `[b]`, `[list]`, `[url=]`, `[hr]`); cada bloque pesa < 8 000 caracteres (límite de Steam). Donde dice `{ID_X}` va el id del ítem de ese mod
(sale de su `About\PublishedFileId.txt` tras subirlo). Las etiquetas (`Mod`, `1.6`) las pone el juego; no hace falta agregar otras.
Requisitos de cada ítem (pestaña "Elementos necesarios"): siempre **Harmony** y lo que diga "Required items" de cada bloque.

## Variante de la oleada 1 (solo la base publicada)
Mientras los otros módulos no estén en el Workshop:
- **Core:** reemplazar la sección *Modules (separate items)* por
  `[h2]More modules on the way[/h2]` + `Resources, Alchemy, Apparel, Fauna & Flora, Furniture, Sects, Traits & Destinies, Grafts, Qi-Tech, Biotech Wuxia and Flying Sect are coming as separate items. Subscribe or watch this page.`
  y quitar el enlace a la colección (se agrega cuando exista, con los ids reales).
- **Stories:** agregar al final de la lista de escenarios `[*]Some scenarios and events appear only when the matching module is installed (Sects, Fauna & Flora, Apparel, Biotech Wuxia, Odyssey).`
- **Races:** en "Requires" no mencionar Sects hasta que se publique.
- En los tres, el bloque *Required items* de la página = Harmony + Convergence: Era of Cultivation (Races: además **Biotech** en el texto, que es DLC, no ítem).

Bloque repetido (se pega al final de cada descripción; incluye licencia y la declaración de IA):

**[FOOTER]**
```
[hr][/hr]
[h2]Install & load order[/h2]
Subscribe to the items listed under "Required items" (Steam does it for you) and sort mods with [b]Auto-sort[/b] if RimWorld asks. Harmony loads first, then the DLCs, then Convergence: Era of Cultivation (Core), then the modules.
[h2]Languages[/h2]
English, Spanish (Español) and Spanish Latin America (Español latino). Every item, technique, trait and event carries a lore description.
[h2]License[/h2]
[list]
[*]Code, scripts and technical data: [b]MIT License[/b].
[*]Original art, lore and narrative text: [b]CC BY-NC-SA 4.0[/b] (credit Prospero1088, non-commercial use only, adaptations under the same license).
[*][b]The art was created with the assistance of AI tools[/b], then selected, edited and placed by the author.
[*]RimWorld material, and anything derived from it, belongs to Ludeon Studios and is not covered by these licenses.
[/list]
The full texts are in the LICENSE.txt files inside the mod folder.
[i]Portions of the materials used to create this content/mod are trademarks and/or copyrighted works of Ludeon Studios Inc. All rights reserved by Ludeon. This content/mod is not official and is not endorsed by Ludeon.[/i]
```

---

## 1. Convergence: Era of Cultivation (Core) — `prospero1088.convergence.core`
Tags: Mod, 1.6, 1.7 · Required items: Harmony · DLC: optional (Royalty, Ideology, Biotech, Anomaly, Odyssey add parts)
Texto vigente de la página (0.1.2, 2026-10-22): el que escribió el usuario, con 1.7, Factions en lugar de Sects y las novedades.

```
[h1]Convergence: Era of Cultivation[/h1]
[i]A fragment of the Nine Heavens collides with your rimworld. Qi begins to flow, and the Era of Cultivation begins.[/i]

Convergence is a modular xianxia / wuxia cultivation system for RimWorld 1.6 and 1.7, inspired by Tale of Immortal, Amazing Cultivation Simulator and cultivation fantasy.

Build your own story. Become a spiritual cultivator, master body cultivation, combine technology with the Dao, or establish a traditional sect.

Convergence expands RimWorld's existing mechanics rather than replacing them.

[h2]The Vision Behind Convergence[/h2]
Convergence isn't about turning RimWorld into Tale of Immortal or Amazing Cultivation Simulator. It's about bringing the xianxia universe into RimWorld and letting both worlds interact.

The Convergence is a world-changing event that introduces Qi, cultivation, spiritual creatures, ancient sects and new possibilities into the stories RimWorld already creates.

A traditional sect seeking immortality? A mechanitor commanding spiritual puppets? A cybernetic warrior pursuing body cultivation? An ordinary colony trying to survive in a world where cultivators have suddenly appeared?

Why not?

The traits, backstories, factions, technologies and cultivation systems are designed to open up these possibilities and create new stories through the unpredictable interactions that make RimWorld special. Some of these possibilities come from the optional modules.

The goal isn't to create a linear cultivation RPG or force every colonist down the same path. It's to introduce an entirely new dimension to the world while preserving the freedom to play however you want.

Cultivation is another dimension of RimWorld, not a replacement for it.

This is my personal interpretation of a genre I love, built around the freedom and emergent storytelling that make RimWorld special.

[h2]Core Features[/h2]
[list]
[*][b]The Convergence:[/b] Choose when spiritual energy awakens. The world changes as spirit roots, factions, beasts and resources become active.
[*][b]Spirit Roots:[/b] Six elements and four grades of spiritual potential, integrated with Biotech genes when available.
[*][b]Special Bodies:[/b] Rare constitutions such as the sword body, steel body or pure yang body, some with four grades that can be refined further.
[*][b]Cultivation Realms:[/b] Nine realms across two paths: Qi Cultivation and Body Cultivation.
[*][b]Heavenly Tribulations:[/b] Break through cultivation realms while facing lightning, spiritual trials and the risk of death.
[*][b]Martial Techniques:[/b] 44 techniques across nine manuals, featuring mastery and combat combinations.
[*][b]The Master:[/b] A mysterious wounded cultivator arrives to guide your colony and introduce the cultivation system through events.
[*][b]Qi Networks:[/b] Spiritual conduits and consumers provide an alternative to electricity.
[*][b]Celestial Calendar:[/b] Qi Tides, Qi Ebbs and the Moon of the Inner Demon.
[*][b]Celestial Dissonance:[/b] Implants affect the difficulty of heavenly tribulations.
[*][b]Dao Research:[/b] A dedicated cultivation research system.
[*][b]Primordial Crystal:[/b] Ten spirit stones pressed into a stable crystal, a shared spiritual material used by the modules.
[*][b]Stories and Characters:[/b] Cultivation traits, extensive backstories and character progression.
[/list]

[h2]Your World, Your Story[/h2]
Cultivation is not mandatory for every colonist. You can pursue immortality, explore the scientific potential of Qi, build a hybrid civilization, or simply try to survive as the world changes around you.

The original RimWorld mechanics remain available, and your colony's story remains your own.

[h2]Customization[/h2]
[list]
[*]Choose when the Convergence begins.
[*]Configure spirit-root probability.
[*]Select cultivation difficulty: Easy, Hard or Nightmare.
[*]Enable or disable lethal tribulations.
[*]Configure the Master and celestial dissonance.
[*]Limit how many visitor groups and caravans can be on your map at once.
[*]Play with or without industrial technology.
[/list]

[h2]Modular Addons[/h2]
Convergence consists of one Core framework and 14 optional modules:

Resources, Formations, Races, Traits & Destinies, Factions, Alchemy & Artifacts, Apparel, Fauna & Flora, Furniture, Grafts, Stories & Scenarios, Qi-Tech, Biotech Wuxia and Flying Sect.

Each module expands the world with new mechanics, content and storytelling possibilities.

[i]Note: Convergence: Sects is no longer part of the pack. It has been replaced by Convergence: Factions. Sects will stay available for existing saves, but it will no longer be updated. Do not use both.[/i]

[h2]Compatibility & Requirements[/h2]
Requires: RimWorld 1.6 or 1.7, and Harmony.

Royalty, Ideology, Biotech, Anomaly and Odyssey are optional for the Core. DLC-specific features activate when their expansions are installed.

No electricity or industrial research is required.

[h2]Development Status[/h2]
Version 0.1.2 — Early Release

Convergence is playable and undergoing additional testing and balancing. Nascent Soul and higher cultivation realms are not implemented yet.

What's new in 0.1.2:
[list]
[*]RimWorld 1.7 support: the same mod works on 1.6 and 1.7.
[*]New setting "Visitor groups at once" (default 2): caravans and visitors no longer pile up on your map. Set it to 0 for vanilla behaviour.
[*]The sword, steel and pure yang bodies are now four-grade constitutions, and two new special bodies appear: the blood moon body and, with Biotech, the thousand threads body. Colonists are born at the first grade; the higher grades are earned through the rituals of Convergence: Factions. Old celestial sword bodies become the highest grade of the sword body.
[/list]

[h2]Community & Contributions[/h2]
This is a free community project. Players and modders are welcome to contribute with bug reports, translations, artwork, compatibility improvements, code and new addons.

GitHub — Source Code & Contributions:
https://github.com/pabloschwindt1-art/Convergence-Era-of-Cultivation

[h2]Languages[/h2]
English, Spanish and Spanish (Latin America).

[h2]Licensing & AI Assistance[/h2]
[list]
[*]Code, scripts and technical content: MIT License.
[*]Original art, lore and narrative content: CC BY-NC-SA 4.0 (attribution, non-commercial use, share-alike).
[*]Artwork was created with AI assistance, under the author's creative direction, and subsequently selected, edited and integrated into the project.
[*]Some artwork remains provisional. RimWorld materials and derivatives belonging to Ludeon Studios are not covered by these licenses.
[/list]

Portions of the materials used to create this content/mod are trademarks and/or copyrighted works of Ludeon Studios Inc. All rights reserved by Ludeon. This content/mod is not official and is not endorsed by Ludeon.

Created by Prospero1088

[h2]A Personal Project[/h2]
I'm not a professional game developer. Convergence started as a personal hobby because I wanted to experience the world of cultivation within RimWorld.

I originally made it for myself, combining elements from cultivation stories and games that I enjoy with RimWorld's emergent storytelling. Eventually, I decided to share it in case other players would enjoy exploring this interpretation of the genre.

This project was developed in my free time with the assistance of AI tools. I'll keep fixing bugs, improving mechanics and adding new ideas whenever I feel like working on it.

There is no development schedule or commitment to regular updates. This is simply a hobby that I enjoy, and I'm happy to share the results with anyone interested.

Feedback, suggestions and contributions are always welcome.
```

---
## 2. Convergence: Resources — `prospero1088.convergence.resources`
Required items: Harmony, Convergence: Era of Cultivation

```
[h1]Convergence: Resources[/h1]
Spirit materials for [url=https://steamcommunity.com/sharedfiles/filedetails/?id={ID_CORE}]Convergence: Era of Cultivation[/url].

[list]
[*][b]Spirit metals[/b]: spirit steel, moon silver, chaos gold and thousand-year cold iron, usable as materials for walls, doors, furniture and weapons.
[*][b]Spirit crystal[/b] and [b]jade[/b], [b]spirit stone[/b] (the currency and fuel of the Dao), [b]spirit wood[/b].
[*][b]Veins[/b] of spirit crystal and cold iron that awaken with the Convergence.
[*][b]Five-element floors[/b] that raise the qi concentration of a room.
[*]A [b]refining furnace[/b] that needs no electricity.
[*]With [b]Odyssey[/b]: galactic steel in asteroids.
[/list]
Requires: Harmony, Convergence: Era of Cultivation. No electricity needed.
```
+ [FOOTER]

## 3. Convergence: Alchemy & Artifacts — `prospero1088.convergence.alchemy`
Required items: Harmony, Core, Resources · Optional: Fauna & Flora

```
[h1]Convergence: Alchemy & Artifacts[/h1]
Spirit forging and alchemy for [url=https://steamcommunity.com/sharedfiles/filedetails/?id={ID_CORE}]Convergence: Era of Cultivation[/url].

[list]
[*][b]Spirit forge[/b] and [b]alchemy furnace[/b] that are upgraded in place (levels 1 to 3).
[*][b]16 spirit weapons[/b] with spirit grade: jian, dao, sabre, spear, guandao, bows, repeating crossbow, flying sword, flying needles and more.
[*][b]Pills[/b] with quality and toxicity: foundation, tempering, blood purification, qi recovery, nine turns, bigu.
[*][b]Spirit herbs[/b], tea, incense, ointment and elixirs.
[*]A [b]rune table[/b] that engraves ordinary weapons and a [b]talisman table[/b].
[/list]
Requires: Harmony, Convergence: Era of Cultivation, Convergence: Resources. No electricity needed.
```
+ [FOOTER]

## 4. Convergence: Apparel — `prospero1088.convergence.apparel`
Required items: Harmony, Core, Resources

```
[h1]Convergence: Apparel[/h1]
Clothes, textiles and armor for [url=https://steamcommunity.com/sharedfiles/filedetails/?id={ID_CORE}]Convergence: Era of Cultivation[/url].

[list]
[*][b]Cloud cotton[/b] (a crop), [b]cloud cloth[/b] woven on a spirit loom, [b]spirit silk[/b], spirit leather from awakened beasts.
[*][b]Sect robes[/b] for outer disciples, inner disciples and elders in the colors of each sect (with Convergence: Sects), hanfu, bamboo hat, jade crown.
[*][b]Spirit scale armor[/b] and [b]runic battle robes[/b] with spirit grade.
[*][b]Accessories[/b] with their own layers: earrings of the five elements, heart-guard amulet, jade pendant, qi-gathering ring, space ring, medicine gourd and talisman belt.
[/list]
Requires: Harmony, Convergence: Era of Cultivation, Convergence: Resources. No electricity needed.
[i]Note: the worn clothing textures of this mod are derivative works of RimWorld art (Ludeon Studios), distributed under the RimWorld EULA, and are not covered by the MIT or CC licenses.[/i]
```
+ [FOOTER]

## 5. Convergence: Fauna & Flora — `prospero1088.convergence.faunaflora`
Required items: Harmony, Core · Optional: Resources, Alchemy, Apparel, Odyssey

```
[h1]Convergence: Fauna & Flora[/h1]
Spirit animals, plants and soil for [url=https://steamcommunity.com/sharedfiles/filedetails/?id={ID_CORE}]Convergence: Era of Cultivation[/url].

[list]
[*][b]Farm animals[/b]: jade weaver spider, seven-color sheep, phoenix-blood hen, cloud ox, moon deer.
[*][b]Wild spirit beasts[/b] that only appear after the Convergence: wind wolf, celestial crane, nine-nether spider, black tortoise, thunder tiger.
[*][b]Spirit soil[/b], glutinous rice, cloud tea, [b]lotus that grows in shallow water[/b], mulberry, millennial pine.
[*]The [b]tribulation mushroom[/b] that sprouts where lightning strikes, and cave fungus that grows only in the dark.
[*]With [b]Odyssey[/b]: spirit fish and rare catches.
[/list]
Requires: Harmony, Convergence: Era of Cultivation. Optional: Resources, Alchemy, Apparel (spirit silk), Odyssey.
```
+ [FOOTER]

## 6. Convergence: Furniture — `prospero1088.convergence.furniture`
Required items: Harmony, Core

```
[h1]Convergence: Furniture[/h1]
Feng shui and sect furniture for [url=https://steamcommunity.com/sharedfiles/filedetails/?id={ID_CORE}]Convergence: Era of Cultivation[/url].

Every piece adds [b]qi and an element[/b] to the room it is in. Carved screens, vases, censers, koi ponds, rock gardens, statues of the four sacred beasts, sect beds and tables, wardrobes, scripture shelves, spirit wine jars, paper and spirit-stone lanterns, glow crystals. Nothing needs electricity.
Requires: Harmony, Convergence: Era of Cultivation.
```
+ [FOOTER]

## 7. Convergence: Formations — `prospero1088.convergence.formations`
Required items: Harmony, Core

```
[h1]Convergence: Formations[/h1]
Formations (zhenfa) and fixed defenses for [url=https://steamcommunity.com/sharedfiles/filedetails/?id={ID_CORE}]Convergence: Era of Cultivation[/url].

A formation is a [b]core that burns spirit stones[/b] plus [b]flags[/b] that enclose its area.
[list]
[*][b]Grand Protective Formation[/b]: a dome that absorbs enemy projectiles.
[*][b]Formation of Killing Swords[/b], [b]Formation of Binding[/b] (immobilizes the strongest enemy) and [b]Qi Gathering Formation[/b] (more qi in the area).
[*][b]Guardian statue[/b], [b]rune crossbow[/b] and [b]thunder ballista[/b] turrets.
[*]Low stone wall, bamboo palisade and qi spike trap.
[/list]
Researched in the Dao comprehension tree. No electricity.
Requires: Harmony, Convergence: Era of Cultivation.
```
+ [FOOTER]

## 8. Convergence: Races — `prospero1088.convergence.races`
Required items: Harmony, Core · DLC required: Biotech

```
[h1]Convergence: Races[/h1]
Beast bloodlines for [url=https://steamcommunity.com/sharedfiles/filedetails/?id={ID_CORE}]Convergence: Era of Cultivation[/url]. [b]Requires Biotech.[/b]

[list]
[*]Five bloodlines: [b]Dragon, Phoenix, Qilin, Black Tortoise and Fox[/b].
[*]Four purities each: trace, diluted, pure and ancestral.
[*]Their own look, gifts and flaws, an innate ability and a xenotype.
[*]Bloodlines appear at random in the world, awaken with the Convergence and can be purified.
[/list]
Requires: Harmony, Convergence: Era of Cultivation, Biotech.
```
+ [FOOTER]

## 9. Convergence: Traits & Destinies — `prospero1088.convergence.traitsdestinies`
Required items: Harmony, Core

```
[h1]Convergence: Traits & Destinies[/h1]
Traits for [url=https://steamcommunity.com/sharedfiles/filedetails/?id={ID_CORE}]Convergence: Era of Cultivation[/url].

[list]
[*]Special bodies, Dao hearts, comprehension, spirit crafts, wuxia personalities and souls. Crafts rank up with use (apprentice, master, grand master).
[*]Very rare destinies: reincarnated, regressor, son of Heaven, antagonist…
[*]Cultivation bonds: Dao partners with dual cultivation, sworn siblings and lifelong rivals.
[/list]
Requires: Harmony, Convergence: Era of Cultivation.
```
+ [FOOTER]

## 10. Convergence: Sects — `prospero1088.convergence.sects`
Required items: Harmony, Core · Optional: Races · DLC optional: Ideology

```
[h1]Convergence: Sects[/h1]
The six factions of Jiuxiao for [url=https://steamcommunity.com/sharedfiles/filedetails/?id={ID_CORE}]Convergence: Era of Cultivation[/url].

[list]
[*][b]Azure Peak Sword Sect, Silent Lotus Temple, Heavenly Cauldron Pavilion, Ancient Dragon Clan, Blood Moon Cult and the Rogue Cultivators.[/b]
[*]They only arrive after the Convergence. Disciples are generated already cultivating (realm, root and techniques by rank). By default, hostile ones never outclass your colony by more than one realm.
[*]Four ranks per sect that you climb with goodwill and realm, with letters and gifts. Traders, caravans and visitors per sect.
[*]With [b]Ideology[/b]: memes, precepts and rituals for each sect (sword way, lotus path, cauldron way, dragon blood, demonic way…).
[/list]
Requires: Harmony, Convergence: Era of Cultivation. Optional: Convergence: Races (dragon, qilin, phoenix and fox bloodlines among their members).
```
+ [FOOTER]

## 10b. Convergence: Factions — `prospero1088.convergence.factions` (nuevo; reemplaza a Sects)
Required items: Harmony, Core · DLC required: Royalty, Ideology · Optional: Biotech Wuxia, Alchemy, Apparel

```
[h1]Convergence: Factions[/h1]
The great sects of Jiuxiao for [url=https://steamcommunity.com/sharedfiles/filedetails/?id={ID_CORE}]Convergence: Era of Cultivation[/url], built on Royalty and Ideology.

[list]
[*][b]Formless Sword Sect[/b]: sword cultivators who deflect arrows and bullets.
[*][b]Sect of Nine Temperings[/b]: body cultivators who fight bare-handed.
[*][b]Heavenly Cauldron Pavilion[/b]: alchemists and merchants; a pill of your choice for its guest elders.
[*][b]Blood Moon Cult[/b]: demonic cultivators, hostile unless you walk the demonic way yourself.
[*][b]Invisible Thread Sect[/b]: puppeteers whose raids bring spirit puppets (needs Convergence: Biotech Wuxia).
[*]And the [b]Rogue Cultivators[/b], who belong to no sect.
[/list]
[h2]Each sect[/h2]
[list]
[*]Its own ideology: meme, precepts, a specialist role and a ritual that tempers a four-grade constitution (sword body, steel body, pure yang body, blood moon body, thousand threads body).
[*]Disciples by rank with their own backstories, names and weapons they can actually use.
[*]Permanent [b]honorary titles[/b] (Royalty) for the colonists who earn its trust, and [b]sect contribution[/b] to spend on five favors: resources, reinforcements, a masterwork gift, an exceptional gift of your choice and a strike of its elite.
[*]Trade in its specialty, from Good to Masterwork.
[/list]
Requires: Harmony, Convergence: Era of Cultivation, Royalty and Ideology. Replaces Convergence: Sects (do not use both).
```
+ [FOOTER]
## 11. Convergence: Grafts — `prospero1088.convergence.grafts`
Required items: Harmony, Core

```
[h1]Convergence: Grafts[/h1]
Dao grafts for [url=https://steamcommunity.com/sharedfiles/filedetails/?id={ID_CORE}]Convergence: Era of Cultivation[/url].

Bionic-style implants made of spirit beast parts and jade, with myth-themed names and lore.
[list]
[*][b]Trade grafts[/b]: divine farmer, Dao miner, celestial smith, alchemist, healer, mountain builder, jade artist, beast shepherd.
[*][b]Beast grafts[/b]: Dao bones, dragon veins, thousand-li eyes, steed legs. [b]Beast organs[/b]: dragon lungs, jade kidneys, wind-hearing ears, qilin skin.
[*]Forged in a bone refining furnace, installed on a Dao grafting altar; removable with ordinary surgery.
[*]Early game: simple spirit-wood limbs and a jade denture, carved at a crafting spot and fitted by any healer.
[*]Each graft carries a small [b]celestial dissonance[/b] that makes tribulation harder.
[/list]
No industrial research and no electricity. Requires: Harmony, Convergence: Era of Cultivation.
```
+ [FOOTER]

## 12. Convergence: Stories & Scenarios — `prospero1088.convergence.stories`
Required items: Harmony, Core · Better with: Sects, Fauna & Flora, Apparel

```
[h1]Convergence: Stories & Scenarios[/h1]
Starting scenarios and story events for [url=https://steamcommunity.com/sharedfiles/filedetails/?id={ID_CORE}]Convergence: Era of Cultivation[/url].

[list]
[*][b]Scenarios[/b]: Lone survivor, Flight from the sect, Destroyed family, Ascension Project, Classical era, Beast hunters; with Sects: Lost disciples, Exiled alchemist, Lotus refugees, Dragon heir; with Biotech Wuxia: Puppet master. Each one has a story that calls back later.
[*][b]111 story events[/b] after the Convergence, with choices and chains: cultists fleeing the Calamity, ferocious beasts, a plea from an allied sect, the vengeance of a clan, a treasure being born…
[*][b]Secret realms[/b]: closed pocket worlds with rooms, doors, guard turrets and loot, found as world sites or by mining.
[*]With [b]Odyssey[/b]: dragon vein and ancient sect ruin landmarks.
[/list]
Requires: Harmony, Convergence: Era of Cultivation. Better with Sects, Fauna & Flora, Apparel.
```
+ [FOOTER]

## 13. Convergence: Qi-Tech — `prospero1088.convergence.qitech`
Required items: Harmony, Core, Formations

```
[h1]Convergence: Qi-Tech (v0.1)[/h1]
For those who play with technology. [b]Requires industrial research[/b]; the base mod never does.
[list]
[*][b]Qi converter[/b]: joins the qi network with the vanilla power grid (qi to electricity or electricity to qi).
[*][b]Qi-Tech exoskeleton[/b] with a qi core and high celestial dissonance.
[*][b]Stabilization chamber[/b]: lowers the celestial dissonance of mechanical implants for a week.
[/list]
Requires: Harmony, Convergence: Era of Cultivation, Convergence: Formations.
[i]Note: the exoskeleton textures of this mod are derivative works of RimWorld art (Ludeon Studios), distributed under the RimWorld EULA, and are not covered by the MIT or CC licenses.[/i]
```
+ [FOOTER]

## 14. Convergence: Biotech Wuxia — `prospero1088.convergence.biotechwuxia`
Required items: Harmony, Core, Resources, Formations · DLC required: Biotech · Optional: Apparel, Anomaly

```
[h1]Convergence: Biotech Wuxia[/h1]
Biotech in a wuxia key for [url=https://steamcommunity.com/sharedfiles/filedetails/?id={ID_CORE}]Convergence: Era of Cultivation[/url]. [b]Requires Biotech.[/b] It reuses Biotech's systems instead of inventing new ones.

[h2]Puppet Master (reinterprets the mechanitor)[/h2]
[list]
[*]The [b]Puppet Heart Thread[/b] gives full mechanitor control.
[*][b]Wood, bronze and jade puppets[/b] (mechs) are built in a puppet workshop from puppet blueprints, spirit thread and spirit wood or metals. No components, no electricity.
[*]Work puppets (wood, mason, miner) and war puppets (bronze guardian, blade, jade). Two puppets beat a cultivator of their realm; one alone does not.
[*]Puppets grow stronger with the realm of their master, recharge from the qi network, and a broken puppet hurts its master.
[/list]
[h2]Blood Demon cultivator (reinterprets the sanguophage)[/h2]
A xenotype that cultivates with blood essence, retreats in a blood pool and uses blood techniques. Not evil by definition: the method is blood, not the morals.
[h2]With Anomaly[/h2]
Jiangshi (hopping vampires) as mutants, peach-wood and mirror counters, glutinous-rice poultice, and the Night of Hungry Ghosts.

Requires: Harmony, Biotech, Convergence: Era of Cultivation, Convergence: Formations, Convergence: Resources.
```
+ [FOOTER]

## 15. Convergence: Flying Sect — `prospero1088.convergence.space`
Required items: Harmony, Core, Resources, Formations · DLC required: Odyssey

```
[h1]Convergence: Flying Sect[/h1]
A floating island you can move, on top of the gravship system of [b]Odyssey[/b] (same mechanic, wuxia pieces). For [url=https://steamcommunity.com/sharedfiles/filedetails/?id={ID_CORE}]Convergence: Era of Cultivation[/url]. [b]Requires Odyssey.[/b]

[list]
[*][b]Island Heart[/b]: a flight formation set in a beast core, the engine of the island.
[*][b]Cloud rock[/b] is the ground; [b]flight cores[/b] set into gravity core mounts make the island grow.
[*][b]Essence vessel[/b] stores the spirit stones that fuel it; the [b]helmsman's altar[/b] steers it; the realm of the best cultivator adds range.
[*][b]Airtight qi barrier[/b] (the wuxia vacuum barrier, powered by the qi network) and [b]vacuum resistance by realm[/b]: cultivators resist the void better as they advance, up to 50% at Golden Core.
[*]Uses Odyssey's own thrusters. No electricity, no chemfuel.
[/list]
Tested with a real flight to an asteroid with four Golden Core cultivators.
Requires: Harmony, Odyssey, Convergence: Era of Cultivation, Resources and Formations.
[i]Note: the island wall atlases of this mod are derivative works of RimWorld art (Ludeon Studios), distributed under the RimWorld EULA, and are not covered by the MIT or CC licenses.[/i]
```
+ [FOOTER]
