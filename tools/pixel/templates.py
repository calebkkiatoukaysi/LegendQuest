"""Makes the blank templates ChatGPT paints the pixel art on (docs/pixel-art-brief.md §4, §4.7, §7.4).

    python tools/pixel/templates.py            # writes every template
    python tools/pixel/templates.py --list     # says what it would write, and where

Templates, all in tools/source/pixel/templates/:

- portrait-template-male.png, portrait-template-female.png (1024 x 1024): the base mannequin's head and
  shoulders in grey on flat #00FF00 green, with dashed darker-green guide lines for the eye line, the chin,
  the collar line (where the neck meets the shoulders) and the safe area the top of the head stays inside.
  The portrait is converted to 128 x 128 art pixels, 8 picture pixels to one.
- emote-sheet.png and life-sheet.png (1536 x 1024): a 4 x 3 grid of cells on flat #FF00FF magenta, with
  light lines between the cells and a dashed margin inside each.

The mannequins are tools/source/characters/base-male.png and base-female.png. Their landmarks (the crown,
the chin and where the neck meets the shoulders) are measured off the pictures, with Story/13 §2.3's table
as the fallback; the eye line is 0.55 of the way down the head (Story/03 §2.2).
"""
import os
import sys

import numpy as np
from PIL import Image, ImageDraw

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
BASES = os.path.join(ROOT, "tools", "source", "characters")
OUT = os.path.join(ROOT, "tools", "source", "pixel", "templates")

GREEN = (0, 255, 0)
GUIDE = (0, 150, 0)
MAGENTA = (255, 0, 255)
CELL_LINE = (255, 170, 255)
MARGIN_LINE = (255, 120, 255)

PORTRAIT = 1024
#: How much of the base's height the portrait shows, from above the crown to mid-chest.
CROP_HEIGHT = 500
#: Room above the crown for hair, hats and helms, in base pixels.
HEADROOM = 70
#: The safe area's inset from the portrait's top and sides, in portrait pixels.
SAFE = 40
#: Story/13 §2.3: the jaw's lowest point and where the neck meets the shoulders, per base.
LANDMARKS = {"base-male": {"chin": 304, "collar": 335}, "base-female": {"chin": 330, "collar": 370}}
EYE_LINE = 0.55

SHEET = (1536, 1024)
GRID = (4, 3)
CELL_MARGIN = 24


def dashed(draw, start, end, colour, width=3, dash=18, gap=12):
    """A dashed straight line from start to end."""
    (x0, y0), (x1, y1) = start, end
    length = max(abs(x1 - x0), abs(y1 - y0))
    step = dash + gap
    for s in range(0, length, step):
        a = s / length
        b = min(s + dash, length) / length
        draw.line([(x0 + (x1 - x0) * a, y0 + (y1 - y0) * a), (x0 + (x1 - x0) * b, y0 + (y1 - y0) * b)],
                  fill=colour, width=width)


def portrait(base):
    """The head-and-shoulders template for one base."""
    picture = Image.open(os.path.join(BASES, base + ".png")).convert("RGBA")
    pixels = np.asarray(picture).astype(float)
    alpha = pixels[..., 3] / 255.0
    shown = (alpha > 0.5) & (pixels[..., :3].sum(2) > 40)
    rows = np.nonzero(shown.any(1))[0]
    crown = int(rows.min())
    chin = LANDMARKS[base]["chin"]
    collar = LANDMARKS[base]["collar"]

    # The head's middle, from the widest run of the head between the crown and the chin.
    head = shown[crown:chin]
    xs = np.nonzero(head.any(0))[0]
    middle = (int(xs.min()) + int(xs.max())) // 2

    top = crown - HEADROOM
    left = middle - CROP_HEIGHT // 2
    crop = picture.crop((left, top, left + CROP_HEIGHT, top + CROP_HEIGHT))
    scale = PORTRAIT / CROP_HEIGHT
    crop = crop.resize((PORTRAIT, PORTRAIT), Image.LANCZOS)

    # The mannequin in light grey, keeping its shading, on flat green.
    rgba = np.asarray(crop).astype(float)
    grey = rgba[..., :3] @ np.array([0.299, 0.587, 0.114])
    grey = 110 + grey * 0.55
    a = (rgba[..., 3:4] / 255.0) * (rgba[..., :3].sum(2, keepdims=True) > 40)
    out = np.array(GREEN, float) * (1 - a) + grey[..., None] * a
    image = Image.fromarray(out.clip(0, 255).astype(np.uint8), "RGB")

    draw = ImageDraw.Draw(image)
    y = lambda base_y: round((base_y - top) * scale)
    eye = crown + (chin - crown) * EYE_LINE
    dashed(draw, (0, y(eye)), (PORTRAIT, y(eye)), GUIDE)
    dashed(draw, (0, y(chin)), (PORTRAIT, y(chin)), GUIDE)
    dashed(draw, (0, y(collar)), (PORTRAIT, y(collar)), GUIDE, dash=6, gap=10)
    dashed(draw, (SAFE, SAFE), (PORTRAIT - SAFE, SAFE), GUIDE)
    dashed(draw, (SAFE, SAFE), (SAFE, PORTRAIT), GUIDE)
    dashed(draw, (PORTRAIT - SAFE, SAFE), (PORTRAIT - SAFE, PORTRAIT), GUIDE)
    return image


def grid_sheet():
    """A 4 x 3 grid of empty cells on magenta, for the emote marks and a room's life sheet."""
    image = Image.new("RGB", SHEET, MAGENTA)
    draw = ImageDraw.Draw(image)
    cols, rows = GRID
    w, h = SHEET[0] // cols, SHEET[1] // rows
    for c in range(1, cols):
        draw.line([(c * w, 0), (c * w, SHEET[1])], fill=CELL_LINE, width=3)
    for r in range(1, rows):
        draw.line([(0, r * h), (SHEET[0], r * h)], fill=CELL_LINE, width=3)
    for r in range(rows):
        for c in range(cols):
            x0, y0 = c * w + CELL_MARGIN, r * h + CELL_MARGIN
            x1, y1 = (c + 1) * w - CELL_MARGIN, (r + 1) * h - CELL_MARGIN
            for a, b in (((x0, y0), (x1, y0)), ((x1, y0), (x1, y1)), ((x0, y1), (x1, y1)), ((x0, y0), (x0, y1))):
                dashed(draw, a, b, MARGIN_LINE, width=2, dash=10, gap=10)
    return image


def templates():
    """Every template, by file name."""
    return {
        "portrait-template-male.png": lambda: portrait("base-male"),
        "portrait-template-female.png": lambda: portrait("base-female"),
        "emote-sheet.png": grid_sheet,
        "life-sheet.png": grid_sheet,
    }


def main(argv):
    made = templates()
    if "--list" in argv:
        for name in made:
            print(os.path.relpath(os.path.join(OUT, name), ROOT).replace(os.sep, "/"))
        return 0
    os.makedirs(OUT, exist_ok=True)
    for name, make in made.items():
        path = os.path.join(OUT, name)
        make().save(path)
        print("templates: wrote", os.path.relpath(path, ROOT).replace(os.sep, "/"))
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
