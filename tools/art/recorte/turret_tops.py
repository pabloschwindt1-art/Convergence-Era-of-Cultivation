"""Recorta la hoja de armas giratorias (vista cenital, apuntando al este) y las centra sobre su eje de giro en un lienzo de
256 px (la torreta gira alrededor del centro de la imagen). El _Top final apunta al ESTE, como los del Art Source de Ludeon
(TurretTop dibuja con ArtworkRotation = -90); se gira al norte solo para calcular el eje y se vuelve al este al guardar.

    python -I turret_tops.py <hoja.webp> <carpeta_texturas>

PIVOT = fraccion del largo del arma (0 = culata, 1 = punta) donde queda el eje de giro.
"""
import sys

import numpy as np
from PIL import Image
from scipy import ndimage

import os
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from recorte_pro import downscale

NAMES = ['GuardianStatue', 'RuneCrossbowTurret', 'ThunderBallistaTurret']
CUTS = [(0, 530), (530, 980), (980, None)]
PIVOT = [0.30, 0.47, 0.46]
LIMIT = [116, 116, 120]


def cut(big, box):
    a = np.array(big.crop(box)).astype(int)
    near = np.abs(a - 255).sum(2) < 45
    lab, _ = ndimage.label(near)
    border = set(np.unique(np.concatenate([lab[0], lab[-1], lab[:, 0], lab[:, -1]]))) - {0}
    fg = ~np.isin(lab, list(border))
    fg = ndimage.binary_opening(fg, iterations=1)
    pure = np.abs(a - 255).sum(2) < 16
    pl, pn = ndimage.label(pure)
    pb = set(np.unique(np.concatenate([pl[0], pl[-1], pl[:, 0], pl[:, -1]]))) - {0}
    for i, sz in enumerate(ndimage.sum(pure, pl, range(1, pn + 1))):
        if sz > 150 and (i + 1) not in pb:
            fg &= pl != i + 1
    grp, ng = ndimage.label(ndimage.binary_dilation(fg, iterations=14))
    sizes = ndimage.sum(fg, grp, range(1, ng + 1))
    fg &= grp == int(np.argmax(sizes)) + 1
    ys, xs = np.where(fg)
    return Image.fromarray(np.dstack([a, fg * 255]).astype(np.uint8)[ys.min():ys.max() + 1, xs.min():xs.max() + 1], 'RGBA')


if __name__ == '__main__':
    big = Image.open(sys.argv[1]).convert('RGB')
    W, H = big.size
    for i, n in enumerate(NAMES):
        x0, x1 = CUTS[i]
        w = cut(big, (x0, 0, x1 or W, H)).rotate(90, expand=True, resample=Image.BICUBIC)  # este -> norte
        px, py = w.width / 2, w.height * (1 - PIVOT[i])
        sc = LIMIT[i] / max(py, w.height - py, w.width / 2)
        r = downscale(w, (round(w.width * sc), round(w.height * sc)))
        cv = Image.new('RGBA', (256, 256), (0, 0, 0, 0))
        cv.alpha_composite(r, (round(128 - px * sc), round(128 - py * sc)))
        cv.transpose(Image.ROTATE_270).save(sys.argv[2].rstrip('/') + '/' + n + '_Top.png')  # norte -> este
        print(n, r.size, 'escala %.2f' % sc)
