"""Completa en los About.xml las URL de Workshop de las dependencias entre mods de Convergence.

Después de subir un mod, el juego deja su id en Mods/<Mod>/About/PublishedFileId.txt. Este script lee esos ids y, en cada mod que depende
de él, pone dentro de la dependencia:
    <steamWorkshopUrl>steam://url/CommunityFilePage/<ID></steamWorkshopUrl>
    <downloadUrl>https://steamcommunity.com/sharedfiles/filedetails/?id=<ID></downloadUrl>

    python -I tools/workshop-link-deps.py            # muestra qué cambiaría (no escribe)
    python -I tools/workshop-link-deps.py --apply    # escribe los cambios

Solo toca las dependencias `prospero1088.convergence.*` cuyo mod ya tiene PublishedFileId.txt. Edita el texto sin reformatear el resto del
archivo y comprueba que el XML siga siendo válido antes de guardar. Después de aplicar, hay que volver a subir (actualizar) los mods cuyo
About cambió y que ya estén publicados.
"""
import os
import re
import sys
import xml.etree.ElementTree as ET

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
MODS = os.path.join(ROOT, 'Mods')


def read_ids():
    ids = {}
    for m in sorted(os.listdir(MODS)):
        about = os.path.join(MODS, m, 'About', 'About.xml')
        pf = os.path.join(MODS, m, 'About', 'PublishedFileId.txt')
        if not (os.path.exists(about) and os.path.exists(pf)):
            continue
        pid = (ET.parse(about).getroot().findtext('packageId') or '').strip().lower()
        fid = open(pf, encoding='utf-8-sig').read().strip()
        if pid and fid.isdigit():
            ids[pid] = fid
    return ids


def patch_about(text, ids):
    """Devuelve (texto nuevo, lista de cambios)."""
    changes = []

    def fix_block(match):
        block = match.group(0)
        pid = re.search(r'<packageId>([^<]+)</packageId>', block).group(1).strip().lower()
        if pid not in ids:
            return block
        fid = ids[pid]
        steam = f'steam://url/CommunityFilePage/{fid}'
        down = f'https://steamcommunity.com/sharedfiles/filedetails/?id={fid}'
        indent = re.search(r'\n(\s*)<packageId>', block).group(1)
        new = block
        for tag, val in (('steamWorkshopUrl', steam), ('downloadUrl', down)):
            if re.search(rf'<{tag}>[^<]*</{tag}>', new):
                new = re.sub(rf'<{tag}>[^<]*</{tag}>', f'<{tag}>{val}</{tag}>', new)
            else:
                anchor = '</displayName>' if '</displayName>' in new else '</packageId>'
                if tag == 'steamWorkshopUrl':
                    new = new.replace(anchor, f'{anchor}\n{indent}<{tag}>{val}</{tag}>', 1)
                else:
                    new = new.replace('</li>', f'{indent}<{tag}>{val}</{tag}>\n' + indent[:-2] + '</li>', 1)
        if new != block:
            changes.append(f'{pid} -> {fid}')
        return new

    new_text = re.sub(r'<li>\s*<packageId>prospero1088\.convergence\.[^<]+</packageId>.*?</li>', fix_block, text, flags=re.S)
    return new_text, changes


def main():
    apply = '--apply' in sys.argv
    ids = read_ids()
    if not ids:
        print('Ningún mod tiene PublishedFileId.txt todavía: no hay nada que completar.')
        return 0
    print('Ids de Workshop conocidos:', ', '.join(f'{k.split(".")[-1]}={v}' for k, v in ids.items()))
    total = 0
    for m in sorted(os.listdir(MODS)):
        path = os.path.join(MODS, m, 'About', 'About.xml')
        text = open(path, encoding='utf-8').read()
        new, changes = patch_about(text, ids)
        if not changes:
            continue
        ET.fromstring(new.encode('utf-8'))      # debe seguir siendo XML válido
        total += len(changes)
        print(f'{m}: ' + '; '.join(changes))
        if apply:
            with open(path, 'w', encoding='utf-8', newline='') as f:
                f.write(new)
    print(('Escrito. ' if apply else 'Nada escrito (usar --apply). ') + f'{total} dependencia(s) a completar.')
    return 0


if __name__ == '__main__':
    sys.exit(main())
