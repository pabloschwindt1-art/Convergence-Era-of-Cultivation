"""Hoja de torreta con fondo transparente: base (izquierda) y cabezal (derecha, vista cenital, dibujado apuntando al NORTE).
Genera <Nombre>.png (base), <Nombre>_Top.png (cabezal centrado sobre su eje de giro en 256 px y girado para apuntar al ESTE,
como los _Top del Art Source de Ludeon: TurretTop dibuja con ArtworkRotation = -90) y <Nombre>_MenuIcon.png (base + cabezal
hacia arriba, a la escala del juego).

    python -I turret_sheet.py <hoja.png|webp> <carpeta_texturas> <Nombre> [--pivot-y FRACCION] [--base-size 1.96] [--top-size 2.7]

Eje de giro: por defecto, el centroide del núcleo de qi (el blob cian más grande del 40-90 % inferior del cabezal); --pivot-y lo
fija a mano como fracción de la altura del cabezal (0 = arriba/boca, 1 = abajo/culata).
"""
import argparse
import os
import sys

import numpy as np
from PIL import Image
from scipy import ndimage

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from recorte_pro import downscale

LIMIT = 118  # radio máximo (px) del cabezal alrededor del eje en el lienzo de 256


def split(sheet):
    a = np.array(sheet)
    alpha = a[:, :, 3] > 12
    a[~alpha] = 0
    lab, n = ndimage.label(ndimage.binary_dilation(alpha, iterations=6))
    sizes = ndimage.sum(alpha, lab, range(1, n + 1))
    big = sorted(np.argsort(sizes)[-2:] + 1, key=lambda i: np.where(lab == i)[1].mean())
    parts = []
    for i in big:
        m = (lab == i) & alpha
        ys, xs = np.where(m)
        crop = a[ys.min():ys.max() + 1, xs.min():xs.max() + 1].copy()
        crop[~m[ys.min():ys.max() + 1, xs.min():xs.max() + 1]] = 0
        parts.append(Image.fromarray(crop, 'RGBA'))
    return parts  # [base, cabezal]


def core_pivot(top):
    a = np.array(top).astype(int)
    r, g, b, al = a[..., 0], a[..., 1], a[..., 2], a[..., 3]
    cyan = (al > 200) & (g > 170) & (b > 170) & (r < 140)
    h = top.height
    cyan[: int(h * 0.4)] = False
    cyan[int(h * 0.9):] = False
    lab, n = ndimage.label(cyan)
    if n == 0:
        return top.width / 2, h * 0.5
    sizes = ndimage.sum(cyan, lab, range(1, n + 1))
    cy, cx = ndimage.center_of_mass(lab == int(np.argmax(sizes)) + 1)
    return top.width / 2, cy  # x: el eje del cañón es el centro horizontal (dibujo simétrico)


def ring_center(base):
    """Centro del anillo de la base (donde se apoya el cabezal): centroide del cian del 30-70 % central."""
    a = np.array(base).astype(int)
    r, g, b, al = a[..., 0], a[..., 1], a[..., 2], a[..., 3]
    cyan = (al > 200) & (g > 150) & (b > 150) & (r < 140)
    h, w = cyan.shape
    box = np.zeros_like(cyan); box[int(h * .3):int(h * .7), int(w * .3):int(w * .7)] = True
    cyan &= box
    if cyan.sum() < 20:
        return w / 2, h / 2
    cy, cx = ndimage.center_of_mass(cyan)
    return cx, cy


def square(img, center, size=256):
    cx, cy = center
    half = max(cx, img.width - cx, cy, img.height - cy) * 1.02
    side = int(half * 2)
    cv = Image.new('RGBA', (side, side), (0, 0, 0, 0))
    cv.alpha_composite(img, (round(side / 2 - cx), round(side / 2 - cy)))
    return downscale(cv, (size, size))


if __name__ == '__main__':
    ap = argparse.ArgumentParser()
    ap.add_argument('sheet'); ap.add_argument('out'); ap.add_argument('name')
    ap.add_argument('--pivot-y', type=float); ap.add_argument('--base-size', type=float, default=1.96)
    ap.add_argument('--top-size', type=float, default=2.7)
    o = ap.parse_args()
    base, top = split(Image.open(o.sheet).convert('RGBA'))
    px, py = core_pivot(top) if o.pivot_y is None else (top.width / 2, top.height * o.pivot_y)
    sc = LIMIT / max(py, top.height - py, top.width / 2)
    r = downscale(top, (round(top.width * sc), round(top.height * sc)))
    tcv = Image.new('RGBA', (256, 256), (0, 0, 0, 0))
    tcv.alpha_composite(r, (round(128 - px * sc), round(128 - py * sc)))
    bc = ring_center(base)
    b = square(base, bc)
    os.makedirs(o.out, exist_ok=True)
    b.save(f'{o.out}/{o.name}.png')
    tcv.rotate(-90, resample=Image.BICUBIC).save(f'{o.out}/{o.name}_Top.png')  # norte -> este, sobre el eje (centro)
    # Ícono: lienzo = tamaño del cabezal en el juego; la base a su escala relativa.
    icon = Image.new('RGBA', (256, 256), (0, 0, 0, 0))
    bs = round(256 * o.base_size / o.top_size)
    icon.alpha_composite(downscale(b, (bs, bs)), ((256 - bs) // 2, (256 - bs) // 2))
    icon.alpha_composite(tcv)
    icon.save(f'{o.out}/{o.name}_MenuIcon.png')
    print(o.name, 'base', base.size, 'cabezal', top.size, 'eje (%.0f, %.0f) = %.2f de la altura' % (px, py, py / top.height), 'anillo base (%.0f, %.0f)' % bc, 'escala %.3f' % sc,
          '-> muzzleDistance %.2f casillas (punta del cañón; con turretTopDrawSize %.1f)' % (py * sc / 256 * o.top_size, o.top_size))
