"""The two base mannequins from one picture of them side by side, the man on the left and the woman
on the right, on black: each figure found, scaled (both by the same amount, so the man stands
STANDARD pixels tall) and stood on its own 1024 x 1536 canvas with its soles on SOLE and its middle
on the canvas's, written as tools/source/characters/base-male.png and base-female.png, and their
blank versions (the faces, hairs and helms are painted on those; a featureless mannequin is its own).

    python tools/split_bases.py <picture.png>
"""
import os
import sys

import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
import import_character as ic  # noqa: E402
import part_sheet as ps  # noqa: E402

CHARS = os.path.join(HERE, "source", "characters")
CANVAS = (1024, 1536)
#: The man's height on the canvas (crown to sole), and the row the soles stand on.
STANDARD = 1351
SOLE = 1466
#: Brighter than this (any channel) is the figure, not the black round it.
LIT = 30
#: Columns this far apart with nothing lit between them part the two figures.
APART = 20


def figures(rgb):
    """The two figures' boxes (x0, y0, x1, y1), left to right."""
    lit = rgb.max(axis=2) > LIT
    xs = np.nonzero(lit.any(axis=0))[0]
    gaps = np.nonzero(np.diff(xs) > APART)[0]
    if len(gaps) != 1:
        sys.exit("split_bases: expected two figures side by side, found %d" % (len(gaps) + 1))
    boxes = []
    for x0, x1 in ((xs[0], xs[gaps[0]]), (xs[gaps[0] + 1], xs[-1])):
        ys = np.nonzero(lit[:, x0:x1 + 1].any(axis=1))[0]
        boxes.append((int(x0), int(ys.min()), int(x1) + 1, int(ys.max()) + 1))
    return boxes


def main(argv):
    if len(argv) != 1:
        sys.exit(__doc__)
    rgb = ps.load(argv[0])
    (m, f) = figures(rgb)
    scale = STANDARD / float(m[3] - m[1])
    for name, (x0, y0, x1, y1) in (("base-male", m), ("base-female", f)):
        w, h = CANVAS
        # The canvas's box in the picture: the figure's middle on the canvas's, its soles on SOLE.
        cx = (x0 + x1) / 2.0
        left = cx - (w / 2.0) / scale
        top = y1 - SOLE / scale
        out = ps.resample_rgb(rgb, (left, top, w / scale, h / scale), (w, h))
        out = np.where(out.max(axis=2, keepdims=True) > 6, out, 0).astype(np.uint8)
        for target in (name, name + "-blank"):
            ic.write_png(os.path.join(CHARS, target + ".png"), np.dstack([out, np.full(out.shape[:2], 255, np.uint8)]))
        print("split_bases: %s, %d x %d in the picture, scaled %.3f, into %s.png and %s-blank.png"
              % (name, x1 - x0, y1 - y0, scale, name, name))
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
