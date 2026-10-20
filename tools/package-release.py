"""Arma los zips de una release: un zip por mod y uno con los 15.

    python -I tools/package-release.py          # compila antes los DLL (dotnet build) y escribe dist/

Cada zip contiene la carpeta del mod (Convergence.<Mod>/) lista para descomprimir dentro de <RimWorld>/Mods/. No incluye .pdb ni archivos
de trabajo. Escribe tambien dist/SHA256SUMS.txt. La carpeta dist/ esta en .gitignore.
"""
import hashlib
import os
import subprocess
import sys
import xml.etree.ElementTree as ET
import zipfile

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
MODS = os.path.join(ROOT, 'Mods')
DIST = os.path.join(ROOT, 'dist')
SKIP_EXT = ('.pdb',)
DOTNET = 'dotnet'


def build():
    order = ['Core', 'Formations', 'Grafts', 'QiTech', 'Stories']       # el Core primero: los demas lo referencian
    for name in order:
        proj = os.path.join(ROOT, 'Source', 'Convergence.' + name, 'Convergence.' + name + '.csproj')
        r = subprocess.run([DOTNET, 'build', proj, '-c', 'Release'], capture_output=True, text=True)
        if r.returncode != 0:
            print(r.stdout[-2000:])
            sys.exit('Fallo la compilacion de ' + name)
        print('compilado:', name)


def add_mod(z, mod):
    base = os.path.join(MODS, mod)
    for dp, dn, fn in os.walk(base):
        dn[:] = [d for d in dn if d != '__pycache__']
        for f in sorted(fn):
            if f.lower().endswith(SKIP_EXT):
                continue
            full = os.path.join(dp, f)
            z.write(full, os.path.join(mod, os.path.relpath(full, base)))


def main():
    if '--no-build' not in sys.argv:
        build()
    version = ET.parse(os.path.join(MODS, 'Convergence.Core', 'About', 'About.xml')).getroot().findtext('modVersion').strip()
    os.makedirs(DIST, exist_ok=True)
    mods = sorted(d for d in os.listdir(MODS) if os.path.isfile(os.path.join(MODS, d, 'About', 'About.xml')))
    for mod in mods:
        asm = os.path.join(MODS, mod, '1.6', 'Assemblies')
        if os.path.isdir(os.path.join(ROOT, 'Source', mod)) and not os.path.isdir(asm):
            sys.exit('Falta el DLL de ' + mod + ' (compilar primero)')
    sums = []
    for mod in mods:
        path = os.path.join(DIST, f'{mod}-{version}.zip')
        with zipfile.ZipFile(path, 'w', zipfile.ZIP_DEFLATED) as z:
            add_mod(z, mod)
        sums.append(path)
    allp = os.path.join(DIST, f'Convergence-all-{version}.zip')
    with zipfile.ZipFile(allp, 'w', zipfile.ZIP_DEFLATED) as z:
        for mod in mods:
            add_mod(z, mod)
    sums.append(allp)
    with open(os.path.join(DIST, 'SHA256SUMS.txt'), 'w', encoding='utf-8') as f:
        for p in sums:
            f.write(hashlib.sha256(open(p, 'rb').read()).hexdigest() + '  ' + os.path.basename(p) + chr(10))
    for p in sums:
        print(f'{os.path.getsize(p)/1e6:6.1f} MB  {os.path.basename(p)}')


if __name__ == '__main__':
    main()
