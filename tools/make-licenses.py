"""Genera los archivos de licencia del proyecto a partir de una sola fuente (este script).

    python -I tools/make-licenses.py            # escribe todo
    python -I tools/make-licenses.py --check    # solo comprueba que lo escrito coincide (sale con 1 si no)

Esquema (decisión del autor, 2026-10-20):
  * Código, scripts y contenido técnico (datos de los XML, documentación técnica) ... MIT
  * Arte original, lore, textos narrativos y documentación creativa ................ CC BY-NC-SA 4.0
  * Material derivado de RimWorld (Ludeon) ......................................... NO se cubre con ninguna de las dos (EULA de RimWorld)
  * Material de terceros ........................................................... se lista aparte, sin atribuirle una licencia que no corresponda
  * El arte se creó con asistencia de IA (se declara)

Escribe:
  LICENSE                                    texto MIT (raíz; GitHub lo detecta como la licencia del repositorio)
  LICENSES.md                                resumen del esquema completo (raíz)
  Mods/<Mod>/LICENSE.txt                     aviso de licencia de ese mod + texto MIT (viaja con el mod al Workshop)
  Mods/<Mod>/LICENSE-CC-BY-NC-SA-4.0.txt     texto legal oficial de CC BY-NC-SA 4.0 (copia de LICENSE-CC-BY-NC-SA-4.0.txt de la raíz)
Lo que lista el material de terceros y lo derivado de Ludeon con más detalle está a mano en LICENSE-THIRD-PARTY.md.
"""
import os
import sys
import textwrap
import xml.etree.ElementTree as ET

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
MODS = os.path.join(ROOT, 'Mods')
HOLDER = 'Copyright (c) 2026 Prospero1088'
NL = chr(10)

MIT = f"""MIT License

{HOLDER}

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
"""

LUDEON = ('Portions of the materials used to create this content/mod are trademarks and/or copyrighted works of '
          'Ludeon Studios Inc. All rights reserved by Ludeon. This content/mod is not official and is not endorsed by Ludeon.')

# Archivos de arte derivados del arte oficial de Ludeon (RimWorld Art Source): silueta y sombreado de la pieza vanilla como base.
# Rutas relativas a la carpeta del mod. Detalle y análisis del EULA: LICENSE-THIRD-PARTY.md §1.
DERIVED = {
    'Convergence.Apparel': {
        'what': 'the worn clothing textures (robes, hanfu, hats, helmets, crown; 5 body types x 3 views)',
        'source': 'the silhouettes and shading of the RimWorld robe, flak vest and hat artwork',
        'paths': ['Common/Textures/Things/Pawn/Humanlike/Apparel/CVG/'],
    },
    'Convergence.QiTech': {
        'what': 'the worn Qi-Tech exoskeleton textures (5 body types x 3 views; the armour design is original, fitted to the body outlines)',
        'source': 'the outlines of the RimWorld naked body artwork',
        'paths': ['Common/Textures/Things/Pawn/Humanlike/Apparel/CVG/QiTechExosuit/'],
    },
    'Convergence.Space': {
        'what': 'the island wall atlases and their menu icons (cloud rock wall, jade wall)',
        'source': 'the geometry of the RimWorld (Odyssey) gravship hull atlas',
        'paths': ['Common/Textures/Things/Building/CVG/Island/CloudRockWall_Atlas.png',
                  'Common/Textures/Things/Building/CVG/Island/CloudRockWall_MenuIcon.png',
                  'Common/Textures/Things/Building/CVG/Island/JadeWall_Atlas.png',
                  'Common/Textures/Things/Building/CVG/Island/JadeWall_MenuIcon.png'],
    },
}

AI_NOTE = ('The art of this project was created with the assistance of generative AI tools (image generation and editing), '
           'then selected, edited, cut and placed by the author. These licenses grant the rights the author holds; AI-generated '
           'material may not be protected by copyright in some jurisdictions.')

NARRATIVE = ('lore descriptions, story and event texts, letters, backstories, scenario and rule-pack texts, and all their '
             'translations, wherever they are written (inline in the Defs or in the Languages/ folders)')

EULA_NOTE = ('Separately, the RimWorld End User License Agreement gives Ludeon Studios a perpetual, irrevocable license to use and '
             'sublicense Derivative Works such as these mods. Nothing in these licenses limits it.')


def wrap(text, indent='', width=78):
    return textwrap.fill(text, width=width, initial_indent=indent, subsequent_indent=indent, break_on_hyphens=False)


def count_files(mod, paths):
    n = 0
    for rel in paths:
        full = os.path.join(MODS, mod, rel.replace('/', os.sep))
        if os.path.isdir(full):
            n += sum(len(fs) for _, _, fs in os.walk(full))
        elif os.path.isfile(full):
            n += 1
    return n


def mod_notice(mod):
    t = ET.parse(os.path.join(MODS, mod, 'About', 'About.xml')).getroot()
    title, pid = t.findtext('name').strip(), t.findtext('packageId').strip()
    out = []
    out.append(f'{title.upper()}')
    out.append(f'{HOLDER} (packageId {pid})')
    out.append('')
    out.append(wrap('This mod folder contains different kinds of material, each under its own terms. '
                    'There is no single license for everything in it.'))
    out.append('')
    out.append('1. CODE AND TECHNICAL DATA: MIT License')
    out.append(wrap('Applies to the compiled assemblies (Assemblies/*.dll, built from the project source code), any '
                    'script, and the technical data in the XML files: structure, numbers, names and labels of '
                    'definitions, patches, references, About and LoadFolders. The full text is at the end of this '
                    'file.', '   '))
    out.append('')
    out.append('2. ORIGINAL ART, LORE AND NARRATIVE TEXT: Creative Commons')
    out.append('   Attribution-NonCommercial-ShareAlike 4.0 International (CC BY-NC-SA 4.0)')
    out.append(wrap('Applies to (a) the author\'s original art: the PNG textures under Textures/ and About/Preview.png, '
                    'except the files listed in section 3; and (b) the narrative text: ' + NARRATIVE + '. Credit: '
                    '"Prospero1088, Convergence: Era of Cultivation". Non-commercial use only. Adaptations must be '
                    'shared under the same license. Full legal text: LICENSE-CC-BY-NC-SA-4.0.txt (this folder). '
                    'Summary: https://creativecommons.org/licenses/by-nc-sa/4.0/', '   '))
    out.append('')
    out.append('3. MATERIAL DERIVED FROM RIMWORLD: not covered by the MIT License or by CC BY-NC-SA')
    d = DERIVED.get(mod)
    if d:
        n = count_files(mod, d['paths'])
        out.append(wrap(f'{n} files of this mod are derivative works made over artwork owned by Ludeon Studios: '
                        f'{d["what"]}, made over {d["source"]} (the RimWorld Art Source, which the RimWorld EULA lets '
                        f'modders use as a basis for a free mod). They are distributed here under the terms of that '
                        f'EULA. Ludeon\'s rights in the underlying artwork remain and the author does not license '
                        f'these files under MIT or CC BY-NC-SA. Use them only inside RimWorld; do not extract or '
                        f'redistribute them separately. Location:', '   '))
        for p in d['paths']:
            out.append('      ' + p)
    else:
        out.append(wrap('The textures of this mod are original; none is derived from RimWorld artwork.', '   '))
    out.append(wrap('The mod also refers to RimWorld\'s own assets and definitions by name (for example vanilla '
                    'textures, defs and sounds) without including them. Those belong to Ludeon Studios.', '   '))
    out.append('')
    out.append('4. THIRD-PARTY MATERIAL')
    out.append(wrap('This mod includes no third-party code or art. It requires the Harmony library (MIT License, '
                    'Andreas Pardeike), which is installed separately and is not part of this folder. Details: '
                    'LICENSE-THIRD-PARTY.md in the project repository.', '   '))
    out.append('')
    out.append('5. AI ASSISTANCE')
    out.append(wrap(AI_NOTE, '   '))
    out.append('')
    out.append(wrap(EULA_NOTE))
    out.append('')
    out.append(wrap(LUDEON))
    out.append('')
    out.append('=' * 78)
    out.append('')
    out.append(MIT.rstrip('\n'))
    return '\n'.join(out) + '\n'


_S1 = wrap('C# source code (Source/) and the assemblies built from it; scripts (tools/); the technical data in the XML '
           'files (structure, numbers, names and labels of definitions, patches, About, LoadFolders); and the technical '
           'documentation (docs/ except the creative documents listed in section 2, README and the other *.md files).', '   ')
_S2 = wrap('(a) Original art created by the author: the textures under Mods/*/Common/Textures and Mods/*/Mods/*/Textures '
           'and Mods/*/About/Preview.png. (b) Narrative text: ' + NARRATIVE + '. (c) Creative documentation: '
           'docs/LORE.md, docs/lore/, docs/EVENTOS.md and docs/modulos/razas/. Credit "Prospero1088, Convergence: Era of '
           'Cultivation"; non-commercial use only; adaptations under the same license. Excludes everything in section 3. '
           'The author\'s working art files (design sheets, screenshots, intermediate files) are not part of this '
           'repository.', '   ')
_S3 = wrap("Some textures are derivative works made over artwork owned by Ludeon Studios (silhouette and shading used "
           "as a base, as the RimWorld EULA allows for a free mod). They are distributed under the terms of that EULA, "
           "Ludeon's rights in the underlying artwork remain, and the author does not license them under sections 1 or "
           "2: the worn clothing textures of Convergence.Apparel, the Qi-Tech exoskeleton textures of "
           "Convergence.QiTech and the island wall atlases of Convergence.Space. Use them only inside RimWorld. The "
           "exact list and the analysis are in LICENSE-THIRD-PARTY.md. RimWorld's own assets that the mods only refer "
           "to by name belong to Ludeon too.", '   ')
_S4 = wrap("No third-party code or art is included. Harmony (MIT, Andreas Pardeike) is a separate requirement. See "
           "LICENSE-THIRD-PARTY.md.", '   ')
_S5 = wrap(AI_NOTE, '   ')
OVERVIEW = NL.join([
    '# Convergence: Era of Cultivation - licensing',
    '',
    HOLDER,
    '',
    'This project (15 RimWorld mods: Core plus 14 modules) is made of different kinds',
    'of material, each under its own terms. There is no single license for the whole',
    'repository. The file `LICENSE` is the MIT text, which covers only the material in section 1.',
    '',
    '## 1. Code, scripts and technical content: MIT License (LICENSE)', '', _S1, '',
    '## 2. Original art, lore and narrative text: CC BY-NC-SA 4.0 (LICENSE-CC-BY-NC-SA-4.0.txt)', '', _S2, '',
    '## 3. Material derived from RimWorld (Ludeon Studios): not licensed by the author', '', _S3, '',
    '## 4. Third-party material', '', _S4, '',
    '## 5. AI assistance', '', _S5, '',
    wrap(EULA_NOTE), '',
    wrap(LUDEON), ''])


def expected_files():
    files = {os.path.join(ROOT, 'LICENSE'): MIT, os.path.join(ROOT, 'LICENSES.md'): OVERVIEW}
    cc = open(os.path.join(ROOT, 'LICENSE-CC-BY-NC-SA-4.0.txt'), encoding='utf-8', newline='').read()
    for mod in sorted(os.listdir(MODS)):
        # Convergence.DevTools: mod de desarrollo (autotests), no se publica.
        if mod == 'Convergence.DevTools' or not os.path.isfile(os.path.join(MODS, mod, 'About', 'About.xml')):
            continue
        files[os.path.join(MODS, mod, 'LICENSE.txt')] = mod_notice(mod)
        files[os.path.join(MODS, mod, 'LICENSE-CC-BY-NC-SA-4.0.txt')] = cc
    return files


def main():
    check = '--check' in sys.argv
    bad = 0
    for path, text in expected_files().items():
        cur = open(path, encoding='utf-8', newline='').read() if os.path.exists(path) else None
        if cur is not None and cur.replace(chr(13) + chr(10), chr(10)) == text.replace(chr(13) + chr(10), chr(10)):
            continue
        bad += 1
        if check:
            print('DIFERENTE:', os.path.relpath(path, ROOT))
        else:
            with open(path, 'w', encoding='utf-8', newline='') as f:
                f.write(text)
            print('escrito:', os.path.relpath(path, ROOT))
    print(('Todo coincide.' if not bad else f'{bad} archivo(s) distintos.') if check else f'{bad} archivo(s) escritos.')
    return 1 if (check and bad) else 0


if __name__ == '__main__':
    sys.exit(main())
