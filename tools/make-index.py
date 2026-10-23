"""Índice del proyecto Convergence (docs/indice/): qué hay en cada mod y dónde, para ir directo a lo que hay que tocar.

Genera:
  docs/indice/INDICE.md   mapa por mod: about, dependencias, carpetas condicionales, archivos de defs (tipos y defNames),
                          parches, idiomas, texturas, código C# (clases y su resumen) y autotests.
  docs/indice/DEFS.tsv    un renglón por def: defName, tipo, mod, archivo, etiqueta en inglés y en español (buscar con Ctrl+F).
  docs/indice/CODIGO.md   cada clase C# con su archivo y la primera línea de su <summary>.

Uso: python tools/make-index.py   (rehacerlo cada vez que se agregan o mueven defs o clases; no toca los mods).
Convergence.Sects está descontinuado (lo reemplaza Factions) y se excluye.
"""
import glob
import os
import re
import xml.etree.ElementTree as ET
from collections import Counter, defaultdict

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
MODS = os.path.join(ROOT, 'Mods')
SRC = os.path.join(ROOT, 'Source')
OUT = os.path.join(ROOT, 'docs', 'indice')
SKIP = {'Convergence.Sects'}

# Módulos sin DLL propio (o con parte de su lógica en el Core): dónde está su código en Source/Convergence.Core.
CORE_CODE = {
    'Convergence.Alchemy': 'Crafting/ (Crafter, ForgeUtility, PillsAndRunes, WorkbenchLevels), Techniques/SpiritGrade.cs',
    'Convergence.BiotechWuxia': 'Biotech/ (marionetas, demonio de sangre, jiangshi, tótem)',
    'Convergence.Space': 'Space/ (isla voladora, cúpula, vuelo)',
    'Convergence.Resources': 'Resources/ (vetas, rocas wuxia)',
    'Convergence.Races': 'Bloodline/ y Defs/Bloodline*.cs (linajes y genes)',
    'Convergence.TraitsDestinies': 'Traits/ (mecánicas, progresión, conflictos de rasgos)',
    'Convergence.Factions': 'Sects/ (constituciones, templado, desvío de proyectiles, stock con calidad, historias de secta); el resto en Source/Convergence.Factions',
    'Convergence.FaunaFlora': 'Body/SpiritBeast.cs, Cultivation/FruitTree.cs, CompPlantSpread.cs, WaterGrowing.cs',
    'Convergence.Furniture': 'QiNetwork/ (red de qi), Debug/FurnitureSelfTest.cs',
}


def parse(path):
    try:
        return ET.parse(path).getroot()
    except ET.ParseError:
        return None


def rel(p, base=ROOT):
    return os.path.relpath(p, base).replace('\\', '/')


def spanish_labels(mod_dir):
    """defName.label -> texto, de todos los DefInjected en español del mod."""
    labels = {}
    for p in glob.glob(os.path.join(mod_dir, '**', 'Languages', 'Spanish', 'DefInjected', '**', '*.xml'), recursive=True):
        r = parse(p)
        if r is None:
            continue
        for el in r:
            if el.tag.endswith('.label'):
                labels[el.tag[:-len('.label')]] = (el.text or '').strip()
    return labels


def about(mod_dir):
    r = parse(os.path.join(mod_dir, 'About', 'About.xml'))
    if r is None:
        return {}
    deps = [d.findtext('packageId') for d in r.findall('./modDependencies/li')]
    return {
        'name': r.findtext('name') or '',
        'packageId': r.findtext('packageId') or '',
        'version': r.findtext('modVersion') or '',
        'deps': [d for d in deps if d],
        'loadAfter': [li.text for li in r.findall('./loadAfter/li') if li.text],
        'incompatible': [li.text for li in r.findall('./incompatibleWith/li') if li.text],
    }


def load_folders(mod_dir):
    r = parse(os.path.join(mod_dir, 'LoadFolders.xml'))
    if r is None:
        return []
    v = r.find('v1.6') or r.find('v1.7')
    out = []
    for li in (v if v is not None else []):
        cond = li.attrib.get('IfModActive') or li.attrib.get('IfModActiveAll') or ''
        out.append((li.text or '').strip() + (f'  (si está {cond})' if cond else ''))
    return out


def source_for(mod):
    d = os.path.join(SRC, mod)
    return d if os.path.isdir(d) else None


CLASS_RE = re.compile(r'(?:public|internal)\s+(?:static\s+|abstract\s+|sealed\s+|partial\s+)*(?:class|struct|enum)\s+(\w+)')
SUMMARY_RE = re.compile(r'///\s*<summary>\s*(?:\r?\n\s*///\s*)?(.*?)(?:</summary>|\r?\n)', re.S)


def classes_of(cs_path):
    text = open(cs_path, encoding='utf-8-sig', errors='replace').read()
    out = []
    for m in CLASS_RE.finditer(text):
        before = text[max(0, m.start() - 900):m.start()]
        idx = before.rfind('<summary>')
        summ = ''
        if idx >= 0 and '</summary>' in before[idx:] or idx >= 0:
            chunk = before[idx + len('<summary>'):]
            chunk = re.sub(r'\s*///\s*', ' ', chunk)
            chunk = chunk.split('</summary>')[0]
            chunk = re.sub(r'<see cref="([^"]+)"\s*/>', r'\1', chunk)
            chunk = re.sub(r'<[^>]+>', '', chunk).strip()
            # solo si el summary queda pegado a la clase (sin otra declaración en medio)
            tail = before[idx:]
            if not re.search(r'\b(class|struct|enum)\s+\w+', tail.split('</summary>')[-1] if '</summary>' in tail else ''):
                summ = chunk.split('. ')[0][:220]
        out.append((m.group(1), summ))
    return out


def args_of(cs_path):
    text = open(cs_path, encoding='utf-8-sig', errors='replace').read()
    return re.findall(r'CommandLineArgPassed\("([^"]+)"\)', text)


def main():
    os.makedirs(OUT, exist_ok=True)
    mods = [m for m in sorted(os.listdir(MODS)) if os.path.isdir(os.path.join(MODS, m)) and m not in SKIP]
    md = ['# Índice del proyecto Convergence', '',
          'Generado por `tools/make-index.py` (no editar a mano: rehacerlo después de agregar o mover defs o clases).',
          'Buscadores: [DEFS.tsv](DEFS.tsv) (cada def: mod, archivo, nombre EN/ES) y [CODIGO.md](CODIGO.md) (cada clase C#).',
          'Convergence.Sects está descontinuado (lo reemplaza Factions) y no se indexa.', '',
          '| Mod | Versión | Depende de | Defs | Clases C# | Autotests |', '|---|---|---|---|---|---|']
    rows, body, code, defs_tsv = [], [], [], ['defName\ttipo\tmod\tarchivo\tlabel_en\tlabel_es']
    for mod in mods:
        d = os.path.join(MODS, mod)
        a = about(d)
        es = spanish_labels(d)
        files = sorted(glob.glob(os.path.join(d, '**', 'Defs', '**', '*.xml'), recursive=True))
        patches = sorted(glob.glob(os.path.join(d, '**', 'Patches', '**', '*.xml'), recursive=True))
        total = 0
        sec = [f'## {mod} — {a.get("name", "")}', '',
               f'- packageId `{a.get("packageId", "")}`, versión {a.get("version", "")}',
               f'- Depende de: {", ".join(a.get("deps", [])) or "—"}']
        if a.get('incompatible'):
            sec.append(f'- Incompatible con: {", ".join(a["incompatible"])}')
        lf = load_folders(d)
        if lf:
            sec.append('- Carpetas que carga: ' + '; '.join(lf))
        sec += ['', '**Defs** (archivo → tipos y defNames):', '']
        for f in files:
            r = parse(f)
            if r is None:
                sec.append(f'- `{rel(f, d)}` — ERROR de XML')
                continue
            kinds, names = Counter(), []
            for el in r:
                name = el.findtext('defName')
                if not name:
                    continue
                kind = el.tag.split('.')[-1]
                kinds[kind] += 1
                names.append(name)
                label = (el.findtext('label') or '').strip()
                defs_tsv.append('\t'.join([name, kind, mod, rel(f), label, es.get(name, '')]))
            total += len(names)
            ktxt = ', '.join(f'{k} {n}' for k, n in kinds.most_common())
            shown = ', '.join(names[:40]) + (f' … (+{len(names) - 40})' if len(names) > 40 else '')
            sec.append(f'- `{rel(f, d)}` — {ktxt}: {shown}')
        if patches:
            sec += ['', '**Parches:** ' + ', '.join(f'`{rel(p, d)}`' for p in patches)]
        langs = sorted({p.split(os.sep + 'Languages' + os.sep)[1].split(os.sep)[0]
                        for p in glob.glob(os.path.join(d, '**', 'Languages', '*', '**', '*.xml'), recursive=True)})
        tex = len(glob.glob(os.path.join(d, '**', 'Textures', '**', '*.png'), recursive=True))
        sec += ['', f'**Idiomas:** {", ".join(langs) or "—"} · **Texturas:** {tex}']
        if mod in CORE_CODE:
            sec += ['', f'**Código en el Core:** Source/Convergence.Core/ → {CORE_CODE[mod]}']
        src = source_for(mod)
        ncls, tests = 0, []
        if src:
            sec += ['', f'**Código** (`{rel(src)}`):', '']
            for cs in sorted(glob.glob(os.path.join(src, '**', '*.cs'), recursive=True)):
                if os.sep + 'obj' + os.sep in cs or os.sep + 'bin' + os.sep in cs:
                    continue
                cls = classes_of(cs)
                ncls += len(cls)
                tests += args_of(cs)
                sec.append(f'- `{rel(cs, src)}`: ' + ', '.join(c for c, _ in cls))
                for c, s in cls:
                    code.append(f'| `{c}` | {mod} | `{rel(cs, src)}` | {s.replace("|", "/")} |')
        tests = sorted(set(tests))
        if tests:
            sec += ['', '**Argumentos de autotest y herramientas:** ' + ', '.join(f'`-{t}`' for t in tests)]
        body += sec + ['']
        rows.append(f'| [{mod}](#{mod.lower().replace(".", "")}) | {a.get("version", "")} | {", ".join(x.split(".")[-1] for x in a.get("deps", [])) or "—"} | {total} | {ncls or "—"} | {", ".join(tests) or "—"} |')
    md += rows + [''] + body
    open(os.path.join(OUT, 'INDICE.md'), 'w', encoding='utf-8').write('\n'.join(md) + '\n')
    open(os.path.join(OUT, 'DEFS.tsv'), 'w', encoding='utf-8').write('\n'.join(defs_tsv) + '\n')
    cm = ['# Clases C# de Convergence', '', 'Generado por `tools/make-index.py`. Primera frase del `<summary>` de cada clase.', '',
          '| Clase | Mod | Archivo | Para qué |', '|---|---|---|---|'] + sorted(code, key=lambda s: s.lower())
    open(os.path.join(OUT, 'CODIGO.md'), 'w', encoding='utf-8').write('\n'.join(cm) + '\n')
    print(f'{len(mods)} mods, {len(defs_tsv) - 1} defs, {len(code)} clases -> {rel(OUT)}')


if __name__ == '__main__':
    main()
