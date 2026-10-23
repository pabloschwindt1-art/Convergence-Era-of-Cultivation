"""Auditoría de nombres y referencias de los mods de Convergence (solo lectura). Escribe docs/AUDITORIA.md.

Revisa, en los 15 mods activos (Convergence.Sects está descontinuado y no se audita):
  1. Menciones a cosas que ya no existen o se retiraron (sectas viejas, el mod Sects, "Sects 2.0").
  2. Referencias a defs CVG_ que no existen en ningún mod (XML y C#).
  3. Traducciones huérfanas: DefInjected de defs que no existen.
  4. Traducciones que faltan: defs con etiqueta en inglés sin etiqueta en Spanish o SpanishLatin.
  5. Claves Keyed que el código pide y no están (por idioma), y claves que están en inglés y faltan en español.
  6. Texturas propias (rutas con CVG) que no existen.
Uso: python tools/audit.py
"""
import glob
import os
import re
import xml.etree.ElementTree as ET
from collections import defaultdict

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
MODS = os.path.join(ROOT, 'Mods')
SRC = os.path.join(ROOT, 'Source')
SKIP = {'Convergence.Sects'}
LANGS = ('Spanish', 'SpanishLatin')

STALE = [
    (r'Azure Peak|Pico Azur', 'secta vieja (Pico Azur → Espada Sin Forma)'),
    (r'Silent Lotus|Loto Silencioso|Lotus Temple|Templo del Loto', 'secta vieja (Loto Silencioso → Caldero)'),
    (r'Lin Wuchen', 'personaje de la secta vieja'),
    (r'Ancient Dragon|Dragón Antiguo|Dragon Clan|Clan del Dragón', 'secta vieja (Clan del Dragón: pendiente de decidir)'),
    (r'Sword Graveyard|Cementerio de Espadas', 'lugar de la secta vieja (ahora Túmulo de Espadas)'),
    (r'Convergence: Sects\b|Sects 2\.0|\bsects2\b', 'el mod Sects (descontinuado)'),
    (r'CVG_Faction_(AzurePeak|SilentLotus|AncientDragon)|CVG_(AzurePeak|SilentLotus|AncientDragon)_\w+', 'def de la secta vieja'),
]


def parse(p):
    try:
        return ET.parse(p).getroot()
    except ET.ParseError as e:
        return e


def rel(p):
    return os.path.relpath(p, ROOT).replace('\\', '/')


def active_mods():
    return [m for m in sorted(os.listdir(MODS)) if os.path.isdir(os.path.join(MODS, m)) and m not in SKIP]


def main():
    mods = active_mods()
    findings = defaultdict(list)  # sección -> [(mod, archivo, detalle)]
    defs = {}          # defName -> (tipo, mod)
    abstract_names = set()
    en_labels = {}     # defName -> (mod, archivo)
    xml_files = []
    for mod in mods + ['Convergence.Sects']:
        for f in glob.glob(os.path.join(MODS, mod, '**', '*.xml'), recursive=True):
            if os.sep + 'Languages' + os.sep in f:
                continue
            r = parse(f)
            if isinstance(r, Exception):
                if mod not in SKIP:
                    findings['0. XML que no se puede leer'].append((mod, rel(f), str(r)))
                continue
            if mod not in SKIP:
                xml_files.append((mod, f, r))
            if r.tag != 'Defs':
                continue
            for el in r:
                name = el.findtext('defName')
                if el.attrib.get('Name'):
                    abstract_names.add(el.attrib['Name'])
                if not name:
                    continue
                if mod in SKIP:
                    defs.setdefault(name, (el.tag, mod))
                    continue
                defs[name] = (el.tag, mod)
                if el.findtext('label') and el.attrib.get('Abstract') != 'True':
                    en_labels[name] = (mod, rel(f))

    # 1. Menciones a lo que ya no existe (XML, idiomas, About y código).
    texts = []
    for mod in mods:
        texts += [(mod, f) for f in glob.glob(os.path.join(MODS, mod, '**', '*.xml'), recursive=True)]
        src = os.path.join(SRC, mod)
        texts += [(mod, f) for f in glob.glob(os.path.join(src, '**', '*.cs'), recursive=True) if os.sep + 'obj' + os.sep not in f]
    for mod, f in texts:
        try:
            lines = open(f, encoding='utf-8-sig', errors='replace').read().splitlines()
        except OSError:
            continue
        for i, line in enumerate(lines, 1):
            for pat, why in STALE:
                m = re.search(pat, line)
                if m:
                    findings['1. Menciones a lo que ya no existe'].append((mod, f'{rel(f)}:{i}', f'"{m.group(0)}" — {why}: {line.strip()[:160]}'))
                    break

    # 2. Referencias a defs CVG_ que no existen (en valores y nombres de elementos de Defs/Patches; y GetNamed en C#).
    known = set(defs) | abstract_names
    keyed_all = set()
    for f in glob.glob(os.path.join(MODS, '*', '**', 'Languages', 'English', 'Keyed', '*.xml'), recursive=True):
        r = parse(f)
        if not isinstance(r, Exception):
            keyed_all |= {el.tag for el in r}
    tok = re.compile(r'\bCVG_[A-Za-z0-9_]+\b')
    for mod, f, r in xml_files:
        if r.tag not in ('Defs', 'Patch'):
            continue
        parents = {ch: p for p in r.iter() for ch in p}
        for el in r.iter():
            cands = []
            if el.tag.startswith('CVG_'):
                cands.append(el.tag)
            if el.text and el.text.strip().startswith('CVG_') and ' ' not in el.text.strip():
                cands.append(el.text.strip())
            # etiquetas (weaponTags, spawnCategories, exclusionTags, categoryTag...) no son defs.
            ctx = el.tag + ' ' + (parents[el].tag if el in parents else '')
            if el.tag == 'li' and el in parents and el.text and el.text.strip() in cands:
                ctx = parents[el].tag
            elif el.text and el.text.strip() in cands:
                ctx = el.tag
            if re.search(r'[Tt]ag|[Cc]ategor|exclusion|Filter|Commonalit', ctx):
                continue
            for c in cands:
                if c in known or c in keyed_all:
                    continue
                findings['2. Referencias a defs CVG_ que no existen'].append((mod, rel(f), f'{c} (en <{ctx}>)'))
    for mod in mods:
        src = os.path.join(SRC, mod)
        for f in glob.glob(os.path.join(src, '**', '*.cs'), recursive=True):
            if os.sep + 'obj' + os.sep in f:
                continue
            text = open(f, encoding='utf-8-sig', errors='replace').read()
            for m in re.finditer(r'GetNamed(?:SilentFail)?\("(CVG_\w+)"', text):
                if m.group(1) not in known and not m.group(1).endswith('_'):
                    findings['2. Referencias a defs CVG_ que no existen'].append((mod, rel(f), m.group(1) + ' (C#)'))

    # 3 y 4. DefInjected: huérfanas y faltantes.
    for mod in mods:
        for lang in LANGS:
            labeled = set()
            for f in glob.glob(os.path.join(MODS, mod, '**', 'Languages', lang, 'DefInjected', '**', '*.xml'), recursive=True):
                r = parse(f)
                if isinstance(r, Exception):
                    findings['0. XML que no se puede leer'].append((mod, rel(f), str(r)))
                    continue
                for el in r:
                    name = el.tag.split('.')[0]
                    if el.tag.endswith('.label'):
                        labeled.add(name)
                    # Make_X = receta generada por recipeMaker; los defs sin CVG_ son vanilla o de una DLC.
                    if name not in defs and name.startswith('CVG_') or name.startswith('Make_CVG_') and name[5:] not in defs:
                        findings['3. Traducciones de defs que no existen'].append((mod, rel(f), f'{lang}: {el.tag}'))
            for name, (m, f) in en_labels.items():
                if m == mod and name not in labeled and defs[name][0] not in ('Convergence.SoundCueDef',):
                    findings['4. Sin etiqueta en español'].append((mod, f, f'{lang}: {name} ({defs[name][0]})'))

    # 5. Keyed.
    keyed = {lang: set() for lang in ('English',) + LANGS}
    keyed_by_mod = {lang: defaultdict(set) for lang in ('English',) + LANGS}
    for mod in mods:
        for lang in keyed:
            for f in glob.glob(os.path.join(MODS, mod, '**', 'Languages', lang, 'Keyed', '*.xml'), recursive=True):
                r = parse(f)
                if isinstance(r, Exception):
                    continue
                for el in r:
                    keyed[lang].add(el.tag)
                    keyed_by_mod[lang][mod].add(el.tag)
    used = defaultdict(set)
    for mod in mods:
        src = os.path.join(SRC, mod)
        for f in glob.glob(os.path.join(src, '**', '*.cs'), recursive=True):
            if os.sep + 'obj' + os.sep in f:
                continue
            for m in re.finditer(r'"(CVG_[A-Za-z0-9_]+)"\s*\.Translate\(', open(f, encoding='utf-8-sig', errors='replace').read()):
                used[m.group(1)].add((mod, rel(f)))
    print('claves Keyed usadas por el código:', len(used), '· texturas propias:', 'ver abajo')
    for key, where in sorted(used.items()):
        for lang in keyed:
            if key not in keyed[lang]:
                mod, f = sorted(where)[0]
                findings['5. Textos que el código pide y no están'].append((mod, f, f'{lang}: {key}'))
    for mod in mods:
        for lang in LANGS:
            for key in sorted(keyed_by_mod['English'][mod] - keyed[lang]):
                findings['5. Textos que el código pide y no están'].append((mod, '(Keyed)', f'{lang}: falta {key} (está en inglés)'))

    # 6. Texturas propias que no existen.
    tex_files = set()
    for mod in mods + ['Convergence.Sects']:
        for f in glob.glob(os.path.join(MODS, mod, '**', 'Textures', '**', '*.png'), recursive=True):
            tex_files.add(f.split(os.sep + 'Textures' + os.sep, 1)[1].replace('\\', '/')[:-4])
    tex_dirs = {t.rsplit('/', 1)[0] for t in tex_files}
    path_tags = ('texPath', 'iconPath', 'factionIconPath', 'royalFavorIconPath', 'wornGraphicPath', 'uiIconPath', 'leaderIconPath', 'elderIconPath', 'expandingIconTexture')
    nchecked = 0
    for mod, f, r in xml_files:
        for el in r.iter():
            if el.tag in path_tags and el.text and 'CVG' in el.text:
                nchecked += 1
            if el.tag not in path_tags or not el.text:
                continue
            p = el.text.strip()
            if 'CVG' not in p:
                continue
            ok = p in tex_files or p in tex_dirs or any(t.startswith(p + '_') for t in tex_files) \
                or any(t.startswith(p + '/') for t in tex_files)
            if not ok:
                findings['6. Texturas que no existen'].append((mod, rel(f), f'{el.tag}: {p}'))

    print('rutas de textura revisadas:', nchecked)

    # Informe.
    out = ['# Auditoría de nombres y referencias', '',
           'Generado por `tools/audit.py` (solo lectura). Convergence.Sects está descontinuado y no se audita.', '',
           '| Sección | Hallazgos |', '|---|---|']
    order = sorted(findings)
    for s in order:
        out.append(f'| {s} | {len(findings[s])} |')
    for s in order:
        out += ['', f'## {s}', '']
        by_mod = defaultdict(list)
        for mod, f, d in findings[s]:
            by_mod[mod].append((f, d))
        for mod in sorted(by_mod):
            out.append(f'### {mod} ({len(by_mod[mod])})')
            for f, d in by_mod[mod][:400]:
                out.append(f'- `{f}` — {d}')
            if len(by_mod[mod]) > 400:
                out.append(f'- … y {len(by_mod[mod]) - 400} más')
            out.append('')
    path = os.path.join(ROOT, 'docs', 'AUDITORIA.md')
    open(path, 'w', encoding='utf-8').write('\n'.join(out) + '\n')
    for s in order:
        print(f'{len(findings[s]):5d}  {s}')
    print('->', rel(path))


if __name__ == '__main__':
    main()
