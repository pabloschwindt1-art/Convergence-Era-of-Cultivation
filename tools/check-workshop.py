"""Revisión previa a subir los mods a Steam Workshop (no toca nada, no lanza el juego).

    python -I tools/check-workshop.py            # los 15 mods
    python -I tools/check-workshop.py Alchemy    # uno solo

Comprueba lo mismo que bloquea o estropea una subida desde el juego (Page_ModsConfig / Workshop.Upload del juego):
About.xml bien formado y con packageId/versión, título <= 128 y descripción <= 8000 caracteres, LoadFolders sin carpetas
inexistentes (el juego se niega a subir si hay una), Preview.png <= 1 MB, dependencias con URL de Workshop, dll presentes,
nada de 0Harmony.dll ni archivos de trabajo, claves Keyed iguales en los tres idiomas y la licencia declarada (About + LICENSE.txt).
Sale con código 1 si hay algún ERROR. Los AVISO son cosas a mirar (p. ej. falta el PublishedFileId.txt antes de la 1.ª subida).
"""
import glob
import os
import re
import sys
import xml.etree.ElementTree as ET

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
MODS = os.path.join(ROOT, 'Mods')
STRAY = ('.psd', '.kra', '.bak', '.py', '.pyc', '.md', '.zip', '.7z', '.json', '.log')
errors, warnings = [], []


def err(m, msg): errors.append(f'ERROR  {m}: {msg}')
def warn(m, msg): warnings.append(f'AVISO  {m}: {msg}')


def png_size(path):
    with open(path, 'rb') as f:
        h = f.read(24)
    return int.from_bytes(h[16:20], 'big'), int.from_bytes(h[20:24], 'big')


def check(m):
    d = os.path.join(MODS, m)
    about = os.path.join(d, 'About', 'About.xml')
    try:
        t = ET.parse(about).getroot()
    except Exception as e:
        err(m, f'About.xml no se puede leer: {e}'); return None
    name = (t.findtext('name') or '').strip()
    desc = (t.findtext('description') or '').strip()
    pid = (t.findtext('packageId') or '').strip()
    if not name: err(m, 'falta <name>')
    if len(name) > 128: err(m, f'título de {len(name)} caracteres (máximo 128)')
    if len(desc) > 8000: err(m, f'descripción de {len(desc)} caracteres (máximo 8000)')
    if not re.fullmatch(r'[a-z0-9]+(\.[a-z0-9]+)+', pid): err(m, f'packageId raro: {pid!r}')
    if pid != pid.lower(): err(m, 'packageId con mayúsculas')
    if '�' in desc: err(m, 'la descripción tiene un carácter dañado (U+FFFD)')
    vers = [x.text for x in t.findall('supportedVersions/li')]
    if '1.6' not in vers: err(m, f'supportedVersions sin 1.6: {vers}')
    if not (t.findtext('modVersion') or '').strip(): warn(m, 'sin <modVersion>')
    if 'MIT' not in desc or 'CC BY-NC-SA' not in desc: err(m, 'la descripción del About no declara la licencia (MIT para código/XML, CC BY-NC-SA 4.0 para el arte)')
    if 'Unlicense' in desc or 'public domain' in desc.lower(): err(m, 'el About todavía dice dominio público/Unlicense')
    if 'AI' not in desc: err(m, 'el About no declara que el arte se hizo con asistencia de IA')
    if 'Ludeon Studios' not in desc: err(m, 'falta el aviso de marcas de Ludeon en el About')
    for lic in ('LICENSE.txt', 'LICENSE-CC-BY-NC-SA-4.0.txt'):
        if not os.path.exists(os.path.join(d, lic)): err(m, f'falta {lic} dentro del mod (python -I tools/make-licenses.py)')
    # dependencias
    deps = t.findall('modDependencies/li')
    load_after = [x.text for x in t.findall('loadAfter/li')]
    for dep in deps:
        dpid = dep.findtext('packageId')
        if not dpid: err(m, 'una dependencia sin packageId'); continue
        if dpid.startswith('prospero1088.'):
            if not (dep.findtext('steamWorkshopUrl') or '').strip(): warn(m, f'dependencia {dpid} sin steamWorkshopUrl (se completa con el id tras subir)')
            if dpid not in load_after: warn(m, f'dependencia {dpid} no está en loadAfter')
        if not (dep.findtext('displayName') or '').strip(): warn(m, f'dependencia {dpid} sin displayName')
    # LoadFolders
    lf = os.path.join(d, 'LoadFolders.xml')
    folders = []
    if os.path.exists(lf):
        for li in ET.parse(lf).getroot().iter('li'):
            folders.append(li.text)
            full = d if li.text == '/' else os.path.join(d, li.text)
            if not os.path.isdir(full): err(m, f'LoadFolders apunta a una carpeta que no existe: {li.text} (el juego se niega a subir)')
    # dll
    dlls = glob.glob(os.path.join(d, '**', 'Assemblies', '*.dll'), recursive=True)
    for dll in dlls:
        if os.path.basename(dll).lower() == '0harmony.dll': err(m, '0Harmony.dll dentro del mod (usa el de Harmony)')
    srcs = os.path.join(ROOT, 'Source', m)
    if os.path.isdir(srcs) and not dlls: err(m, 'tiene código en Source/ pero no hay DLL compilado')
    if dlls and '1.6' not in folders: err(m, 'hay DLL en 1.6/Assemblies pero LoadFolders no incluye 1.6')
    # Preview
    pv = os.path.join(d, 'About', 'Preview.png')
    if not os.path.exists(pv): err(m, 'falta About/Preview.png')
    else:
        sz = os.path.getsize(pv)
        if sz > 1_000_000: err(m, f'Preview.png de {sz/1e6:.1f} MB (Steam acepta hasta 1 MB)')
        w, h = png_size(pv)
        if (w, h) != (640, 360): warn(m, f'Preview.png de {w}x{h} (se recomienda 640x360)')
    # archivos que no deben viajar
    stray = [os.path.relpath(p, d) for p in glob.glob(os.path.join(d, '**', '*'), recursive=True)
             if os.path.isfile(p) and p.lower().endswith(STRAY)]
    if stray: warn(m, f'archivos de trabajo dentro del mod: {stray[:4]}')
    pdbs = len(glob.glob(os.path.join(d, '**', '*.pdb'), recursive=True))
    if pdbs: warn(m, f'{pdbs} .pdb viajan con la subida (ayudan a ubicar errores; pesan poco; el juego sube la carpeta entera)')
    if not os.path.exists(os.path.join(d, 'About', 'PublishedFileId.txt')): warn(m, 'sin PublishedFileId.txt (normal antes de la 1.ª subida; el juego lo crea al subir)')
    # Keyed iguales en los tres idiomas
    keys = {}
    for lang in ('English', 'Spanish', 'SpanishLatin'):
        ks = set()
        for f in glob.glob(os.path.join(d, '**', 'Languages', lang, 'Keyed', '*.xml'), recursive=True):
            ks |= {e.tag for e in ET.parse(f).getroot()}
        keys[lang] = ks
    if keys['English']:
        for lang in ('Spanish', 'SpanishLatin'):
            miss = keys['English'] - keys[lang]
            if miss: err(m, f'{len(miss)} claves Keyed de English faltan en {lang}: {sorted(miss)[:3]}')
    size = sum(os.path.getsize(p) for p in glob.glob(os.path.join(d, '**', '*'), recursive=True) if os.path.isfile(p))
    return pid, name, size, len(desc), len(deps)


def check_load_order():
    """Bucles de orden entre nuestros mods (loadAfter + dependencias): RimWorld no los puede ordenar y los marca en rojo."""
    graph = {}
    for m in os.listdir(MODS):
        path = os.path.join(MODS, m, 'About', 'About.xml')
        if not os.path.isfile(path): continue
        t = open(path, encoding='utf-8-sig').read()
        pid = re.search(r'<packageId>([^<]+)', t).group(1).lower()
        after = set()
        la = re.search(r'(?s)<loadAfter>(.*?)</loadAfter>', t)
        if la: after |= {x.lower() for x in re.findall(r'<li>([^<]+)</li>', la.group(1))}
        for deps in re.findall(r'(?s)<modDependencies>(.*?)</modDependencies>', t):
            after |= {x.lower() for x in re.findall(r'<packageId>([^<]+)', deps)}
        graph[pid] = {a for a in after if a.startswith('prospero1088.')}
    state = {}
    def visit(n, path):
        state[n] = 1
        for nxt in graph.get(n, ()):
            if state.get(nxt) == 1:
                err('orden de carga', 'bucle: ' + ' -> '.join(path[path.index(nxt):] + [nxt]))
            elif nxt not in state:
                visit(nxt, path + [nxt])
        state[n] = 2
    for n in sorted(graph):
        if n not in state: visit(n, [n])


def main():
    only = sys.argv[1] if len(sys.argv) > 1 else None
    # Convergence.DevTools: mod de desarrollo (autotests), no se publica.
    mods = [m for m in sorted(os.listdir(MODS)) if (not only or m.endswith(only)) and m != 'Convergence.DevTools']
    print(f'{"mod":28s} {"packageId":42s} {"MB":>5s} {"desc":>5s} deps')
    for m in mods:
        r = check(m)
        if r: print(f'{m:28s} {r[0]:42s} {r[2]/1e6:5.1f} {r[3]:5d} {r[4]}')
    check_load_order()
    for line in errors + warnings:
        print(line.encode('ascii', 'replace').decode())
    print(f'\n{len(errors)} error(es), {len(warnings)} aviso(s)')
    return 1 if errors else 0


if __name__ == '__main__':
    sys.exit(main())
