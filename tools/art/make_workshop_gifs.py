"""Arma los GIF de la página de Workshop con los cuadros que graba -cvgworkshopshot.

    python -I tools/art/make_workshop_gifs.py <carpeta cvg_workshop> <carpeta de salida> [--width 800] [--step 2]

Cada subcarpeta anim_<nombre>/ (cuadros 000.png, 001.png... a 30 cuadros por segundo de juego) se convierte en <nombre>.gif:
toma uno de cada --step cuadros, recorta el centro a 16:9 (2/3 de la pantalla, donde la cámara puso la acción), lo escala
a --width de ancho y lo guarda en bucle. Las capturas fijas (*.png sueltos) se copian convertidas a JPG de calidad 92.
--delta N (recomendado 60): congela los píxeles que cambian menos de N (suma RGB) y guarda cada cuadro como diferencia
transparente. Con pasto y viento baja el GIF de ~17 MB a < 1 MB (límite del Workshop: 2 MB por archivo). --shift-x corre el
recorte (fracción del ancho; negativo = a la izquierda).
"""
import argparse
import os

import numpy as np

from PIL import Image

ap = argparse.ArgumentParser()
ap.add_argument('src')
ap.add_argument('out')
ap.add_argument('--width', type=int, default=640)
ap.add_argument('--colors', type=int, default=96)
ap.add_argument('--step', type=int, default=2)
ap.add_argument('--crop', type=float, default=2 / 3, help='fracción central de la pantalla que se conserva')
ap.add_argument('--delta', type=int, default=0)
ap.add_argument('--shift-x', type=float, default=0.0)
a = ap.parse_args()
os.makedirs(a.out, exist_ok=True)


def crop_center(im, frac):
    w, h = im.size
    cw, ch = int(w * frac), int(h * frac)
    x, y = (w - cw) // 2 + int(a.shift_x * w), (h - ch) // 2
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
        out = os.path.join(a.out, name[5:] + '.gif')
        if a.delta:
            # Congela lo que casi no cambia (pasto, viento) y guarda cada cuadro como diferencia transparente.
            arr = [np.array(fr).astype(np.int16) for fr in frames]
            cur = arr[0].copy(); frozen = [cur.copy()]
            for f in arr[1:]:
                d = np.abs(f - cur).sum(2) > a.delta
                cur[d] = f[d]; frozen.append(cur.copy())
            frames = [Image.fromarray(f.astype(np.uint8)) for f in frozen]
            colors = min(a.colors, 255)
            pal = frames[len(frames) // 2].quantize(colors=colors, method=Image.Quantize.MEDIANCUT)
            q = [fr.quantize(palette=pal, dither=Image.Dither.NONE) for fr in frames]
            T = colors
            seq = [q[0]]
            for prev, nxt in zip(q, q[1:]):
                A, B = np.array(prev), np.array(nxt).copy()
                B[A == B] = T
                im = Image.fromarray(B.astype(np.uint8), 'P')
                im.putpalette(pal.getpalette()[:colors * 3] + [255, 0, 255])
                seq.append(im)
            seq[0].save(out, save_all=True, append_images=seq[1:], duration=round(1000 * a.step / 30), loop=0,
                        transparency=T, disposal=1, optimize=False)
        else:
            # Una paleta común (la del cuadro del medio) evita el parpadeo de colores entre cuadros.
            # Sin tramado: el ruido del tramado cambia en cada cuadro y multiplica el peso del GIF.
            pal = frames[len(frames) // 2].quantize(colors=a.colors, method=Image.Quantize.MEDIANCUT)
            q = [fr.quantize(palette=pal, dither=Image.Dither.NONE) for fr in frames]
            q[0].save(out, save_all=True, append_images=q[1:], duration=round(1000 * a.step / 30), loop=0, optimize=True)
        print('gif', out, len(q), 'cuadros', round(os.path.getsize(out) / 1e6, 2), 'MB')
