"""Arma los GIF de la página de Workshop con los cuadros que graba -cvgworkshopshot.

    python -I tools/art/make_workshop_gifs.py <carpeta cvg_workshop> <carpeta de salida> [--width 800] [--step 2]

Cada subcarpeta anim_<nombre>/ (cuadros 000.png, 001.png... a 30 cuadros por segundo de juego) se convierte en <nombre>.gif:
toma uno de cada --step cuadros, recorta el centro a 16:9 (2/3 de la pantalla, donde la cámara puso la acción), lo escala
a --width de ancho y lo guarda en bucle. Las capturas fijas (*.png sueltos) se copian convertidas a JPG de calidad 92.
"""
import argparse
import os

from PIL import Image

ap = argparse.ArgumentParser()
ap.add_argument('src')
ap.add_argument('out')
ap.add_argument('--width', type=int, default=640)
ap.add_argument('--colors', type=int, default=96)
ap.add_argument('--step', type=int, default=2)
ap.add_argument('--crop', type=float, default=2 / 3, help='fracción central de la pantalla que se conserva')
a = ap.parse_args()
os.makedirs(a.out, exist_ok=True)


def crop_center(im, frac):
    w, h = im.size
    cw, ch = int(w * frac), int(h * frac)
    x, y = (w - cw) // 2, (h - ch) // 2
    return im.crop((x, y, x + cw, y + ch))


for name in sorted(os.listdir(a.src)):
    path = os.path.join(a.src, name)
    if os.path.isfile(path) and name.endswith('.png'):
        Image.open(path).convert('RGB').save(os.path.join(a.out, name[:-4] + '.jpg'), quality=92)
        print('jpg', name)
    elif os.path.isdir(path) and name.startswith('anim_'):
        files = sorted(f for f in os.listdir(path) if f.endswith('.png'))[::a.step]
        frames = []
        for f in files:
            im = crop_center(Image.open(os.path.join(path, f)).convert('RGB'), a.crop)
            im = im.resize((a.width, round(a.width * im.size[1] / im.size[0])), Image.LANCZOS)
            frames.append(im)
        if not frames:
            continue
        # Una paleta común (la del cuadro del medio) evita el parpadeo de colores entre cuadros.
        # Sin tramado: el ruido del tramado cambia en cada cuadro y multiplica el peso del GIF.
        pal = frames[len(frames) // 2].quantize(colors=a.colors, method=Image.Quantize.MEDIANCUT)
        q = [fr.quantize(palette=pal, dither=Image.Dither.NONE) for fr in frames]
        out = os.path.join(a.out, name[5:] + '.gif')
        q[0].save(out, save_all=True, append_images=q[1:], duration=round(1000 * a.step / 30), loop=0, optimize=True)
        print('gif', out, len(q), 'cuadros', round(os.path.getsize(out) / 1e6, 2), 'MB')
