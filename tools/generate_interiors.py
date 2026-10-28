"""
Inside Questeria's buildings: the interior tileset, and the furniture for the Gilded Flagon, the
cottages, the shops and the warehouses on the docks.

    python tools/generate_interiors.py

The same camera as outside: you see the floor from above and the back wall from the front, two
tiles tall (plaster with a beam over wainscoting), and the side and front walls as their tops
only, so the room's open towards you. Warm wood and cream plaster, lit from the top left and
shadowed lavender, in the colours of my style sheet. The rug has edges the tilemap draws wherever it
meets the floor, like the lawns outside. The furniture stands on its feet like every other prop.

Only needs CPython. Every bit of noise is hash01 on where it is, so a rerun writes the same bytes.
"""

import math
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from pixels import OUT, hash01, new_image, write_png  # noqa: E402
from generate_harbour import OUTLINE, T, Canvas, dither, put, tile, turned  # noqa: E402

FLOOR = ((106, 66, 50), (146, 96, 64), (184, 130, 84), (214, 166, 108), (236, 198, 140))
STONE = ((112, 104, 116), (150, 140, 146), (184, 174, 172), (212, 204, 196))
PLASTER = ((178, 164, 170), (214, 202, 196), (236, 226, 214), (250, 244, 232))
BEAM = ((60, 38, 36), (96, 62, 48), (132, 90, 62), (168, 120, 80))
RUG = ((92, 24, 40), (138, 36, 52), (176, 58, 64), (208, 92, 84))
GOLD = ((150, 104, 40), (214, 166, 74), (246, 214, 130))
GLASS = ((84, 130, 176), (140, 188, 226), (210, 236, 250))
FIRE = ((196, 70, 30), (244, 140, 50), (255, 210, 110), (255, 246, 200))
IRON = ((40, 40, 52), (78, 80, 96), (130, 134, 150))
CLOTH = ((60, 70, 120), (86, 104, 160), (140, 156, 206), (200, 210, 236))
PAGE = ((150, 120, 90), (206, 180, 140))


# --------------------------------------------------------------------------- #
# Tiles
# --------------------------------------------------------------------------- #

def floor_wood(variant):
    """Floorboards across the tile, three to a tile, their joins staggered and worn pale down the middle."""
    img = tile()
    for row in range(3):
        y0 = row * 5 + (1 if row == 2 else 0)
        h = 5 if row < 2 else 6
        join = (5 + row * 6 + variant * 7) % T
        for dy in range(h):
            for x in range(T):
                y = y0 + dy
                if dy == h - 1:
                    put(img, x, y, FLOOR[0])
                elif x == join:
                    put(img, x, y, FLOOR[1])
                else:
                    c = 3 if dy == 0 else 2
                    if hash01(x // 3, y + row * 5, 301 + variant) < 0.14:
                        c = 1
                    elif hash01(x, y, 302 + variant) > 0.94:
                        c = 4
                    put(img, x, y, FLOOR[c])
    return img


def floor_stone(variant):
    """Big flagstones, two to a tile each way, every other course shifted."""
    img = tile()
    for row in range(2):
        shift = 0 if (row + variant) % 2 == 0 else 4
        for x in range(T):
            for dy in range(8):
                y = row * 8 + dy
                sx = (x + shift) % 8
                if dy == 7 or sx == 7:
                    put(img, x, y, STONE[0])
                else:
                    c = 3 if dy == 0 or sx == 0 else 2
                    if dy == 6 or sx == 6:
                        c = 1
                    if hash01(x, y, 311 + variant) > 0.93:
                        c = 1
                    put(img, x, y, STONE[c])
    return img


def rug():
    """Deep red wool with a small gold diamond repeating across it."""
    img = tile()
    for y in range(T):
        for x in range(T):
            c = 1 if (x + y) % 2 else 2
            d = abs(x - 7.5) + abs(y - 7.5)
            if 3.5 < d < 5:
                put(img, x, y, GOLD[1])
            elif d < 1.5:
                put(img, x, y, GOLD[2])
            else:
                put(img, x, y, RUG[c])
    return img


def rug_edges():
    """The rug's border: a gold line and a dark red hem along whichever side meets the floor."""
    edges = []
    for side in range(4):
        img = tile()
        for x in range(T):
            put(img, x, 0, RUG[0])
            put(img, x, 1, GOLD[2] if side in (0, 3) else GOLD[1])
            put(img, x, 2, RUG[0])
            if x % 4 == 0:
                put(img, x, 3, GOLD[0])
        edges.append(turned(img, side))
    return edges


def wall_top():
    """The top of a wall seen from above: a dark beam along plaster."""
    img = tile()
    for y in range(T):
        for x in range(T):
            put(img, x, y, BEAM[1] if y in (0, 15) else BEAM[2] if y in (1, 14) else PLASTER[1] if (x + y) % 9 else PLASTER[0])
    return img


def wall_face(variant):
    """The upper half of the back wall: plaster under a beam, plain, with a window, or with a shelf of crocks."""
    img = tile()
    for y in range(T):
        for x in range(T):
            if y < 3:
                c = BEAM[3] if y == 0 else BEAM[2] if y == 1 else BEAM[0]
            else:
                c = PLASTER[2] if not dither(x, y, 0.12) else PLASTER[1]
                if x == 0:
                    c = BEAM[1]
            put(img, x, y, c)
    if variant == 1:
        # A window with a wooden frame and the day outside it.
        for y in range(5, 15):
            for x in range(3, 13):
                frame = x in (3, 12) or y in (5, 14) or x == 7 or y == 9
                put(img, x, y, BEAM[1] if frame else GLASS[2] if (x + y) < 15 else GLASS[1] if y < 12 else GLASS[0])
    elif variant == 2:
        # A shelf with crocks and a jar on it.
        for x in range(1, 16):
            put(img, x, 12, BEAM[3])
            put(img, x, 13, BEAM[1])
        for cx, colour in ((4, (196, 120, 90)), (9, (110, 150, 120)), (13, (220, 210, 190))):
            for y in range(7, 12):
                for x in range(cx - 1, cx + 2):
                    put(img, x, y, colour if x < cx + 1 else tuple(max(0, v - 50) for v in colour))
    return img


def wall_base(variant):
    """The lower half of the back wall: wood panelling, a dark skirting, and its shadow on the floor."""
    img = tile()
    for y in range(T):
        for x in range(T):
            if y < 2:
                c = BEAM[3] if y == 0 else BEAM[2]
            elif y < 13:
                panel = (x % 8) in (0, 7) or y in (2, 12)
                c = BEAM[1] if panel else BEAM[2] if (x % 8) < 4 else BEAM[1]
                if variant == 1 and 4 < y < 10 and (x % 8) in (3, 4):
                    c = GOLD[1]
            else:
                c = BEAM[0]
            put(img, x, y, c)
    return img


#: The interior tileset, in TilesetCatalog's order: name, variants, and edges if it has them.
INTERIOR_TILES = (
    ("floor-wood", [floor_wood(v) for v in range(3)], None),
    ("floor-stone", [floor_stone(v) for v in range(2)], None),
    ("rug", [rug()], rug_edges()),
    ("wall-top", [wall_top()], None),
    ("wall-face", [wall_face(v) for v in range(3)], None),
    ("wall-base", [wall_base(v) for v in range(2)], None),
)


def build_tiles():
    frames = []
    for _, variants, edges in INTERIOR_TILES:
        frames += variants
        frames += edges or []
    columns = 8
    sheet = new_image(columns * T, (len(frames) + columns - 1) // columns * T)
    for i, f in enumerate(frames):
        for y in range(T):
            for x in range(T):
                if f[y][x][3]:
                    sheet[(i // columns) * T + y][(i % columns) * T + x] = list(f[y][x])
    write_png(os.path.join(OUT, "Tiles", "interior.png"), sheet)


# --------------------------------------------------------------------------- #
# Furniture
# --------------------------------------------------------------------------- #

def shade_box(c, x0, y0, x1, y1, ramp, top_rows=2):
    """A wooden block seen from above and in front: its lit top, then its front face going darker down."""
    for y in range(y0, y1 + 1):
        for x in range(x0, x1 + 1):
            if y < y0 + top_rows:
                tone = len(ramp) - 1 if y == y0 else len(ramp) - 2
            else:
                tone = 2 if x < (x0 + x1) // 2 else 1
                if y == y1:
                    tone = 0
            c.put(x, y, ramp[max(0, min(len(ramp) - 1, tone))])


def tankard(c, x, y):
    """A tankard: pewter with a head of foam."""
    c.box(x, y, x + 2, y + 3, IRON[1])
    c.box(x, y, x, y + 3, IRON[2])
    c.box(x, y - 1, x + 2, y - 1, (250, 244, 230))
    c.put(x + 3, y + 1, IRON[0])


def bar_counter():
    """The Flagon's bar: a long polished counter, its top lit, its front panelled, tankards along it and a tap."""
    c = Canvas(112, 34)
    shade_box(c, 1, 8, 110, 32, BEAM, top_rows=8)
    for x in range(1, 111, 14):
        c.box(x, 18, x, 31, BEAM[0])
    c.box(1, 15, 110, 15, GOLD[1])
    for x in (12, 30, 64, 92):
        tankard(c, x, 9)
    # The tap at one end, brass.
    c.box(100, 2, 102, 10, GOLD[1])
    c.box(97, 2, 104, 3, GOLD[2])
    c.outline()
    return c.img


def round_table():
    """A round tavern table: its top lit, two tankards and a candle on it, its one leg under."""
    c = Canvas(36, 28)
    c.box(16, 14, 19, 27, BEAM[1])
    for y in range(18):
        for x in range(36):
            d = ((x - 17.5) / 17) ** 2 + ((y - 8) / 8) ** 2
            if d <= 1.0:
                c.put(x, y, BEAM[3] if y < 7 and x < 22 else BEAM[2] if y < 13 else BEAM[1])
    tankard(c, 9, 6)
    tankard(c, 23, 8)
    c.box(17, 2, 18, 6, (240, 230, 210))
    c.put(17, 1, FIRE[2])
    c.put(17, 0, FIRE[3])
    c.outline()
    return c.img


def stool():
    """A three-legged stool."""
    c = Canvas(12, 14)
    for y in range(4):
        for x in range(12):
            if ((x - 5.5) / 5.5) ** 2 + ((y - 1.5) / 2) ** 2 <= 1.0:
                c.put(x, y, BEAM[3] if y < 2 else BEAM[2])
    for x in (2, 6, 9):
        c.box(x, 4, x, 13, BEAM[1])
    c.outline()
    return c.img


def hearth():
    """A stone hearth against the back wall: its chimney breast, the arch, the fire in it, and a pot hung over."""
    c = Canvas(52, 56)
    for y in range(56):
        for x in range(52):
            stone = ((x // 8 + (y // 6) % 2) % 2 == 0)
            if y % 6 == 5 or x % 8 == 7 and (y // 6) % 2 == 0 or (x + 4) % 8 == 7 and (y // 6) % 2 == 1:
                c.put(x, y, STONE[0])
            else:
                c.put(x, y, STONE[3] if x < 10 else STONE[2] if stone else STONE[1])
    # The firebox, dark inside the arch, and the fire.
    for y in range(24, 52):
        for x in range(10, 42):
            if ((x - 26) / 16) ** 2 + ((y - 34) / 12) ** 2 <= 1.0 or y >= 34 and 10 <= x <= 41:
                c.put(x, y, (24, 16, 20))
    for y in range(36, 52):
        for x in range(14, 38):
            heat = (52 - y) / 16 - abs(x - 26) / 14 + (hash01(x, y, 331) - 0.5) * 0.3
            if heat > 0.15:
                c.put(x, y, FIRE[min(3, int(heat * 4))])
    c.box(12, 50, 40, 51, BEAM[0])
    c.box(10, 18, 42, 20, BEAM[2])
    c.box(10, 18, 42, 18, BEAM[3])
    c.outline()
    return c.img


def keg_stack():
    """Kegs on their sides, two down and one on top, their hoops catching the light."""
    c = Canvas(40, 34)

    def keg(cx, cy):
        for y in range(cy - 7, cy + 8):
            for x in range(cx - 9, cx + 10):
                if ((x - cx) / 9) ** 2 + ((y - cy) / 7.5) ** 2 <= 1.0:
                    hoop = abs(abs(x - cx) - 5) < 1
                    c.put(x, y, IRON[1] if hoop else BEAM[3] if y < cy - 2 else BEAM[2] if y < cy + 3 else BEAM[1])
        c.box(cx - 1, cy - 1, cx + 1, cy + 1, BEAM[0])

    keg(10, 24)
    keg(29, 24)
    keg(19, 10)
    c.outline()
    return c.img


def bed():
    """A bed seen from the foot: its headboard against the wall, the blanket turned down, a pillow."""
    c = Canvas(34, 46)
    shade_box(c, 1, 0, 32, 10, BEAM, top_rows=2)
    for y in range(8, 44):
        for x in range(3, 31):
            if y < 16:
                c.put(x, y, PAGE[1] if x < 20 else (180, 160, 130))
            else:
                c.put(x, y, CLOTH[2] if y < 20 else CLOTH[1] if (x + y) % 7 else CLOTH[0])
    c.box(1, 40, 32, 45, BEAM[1])
    c.box(1, 40, 32, 40, BEAM[3])
    c.outline()
    return c.img


def wardrobe():
    """A tall wardrobe: two doors with brass handles, a cornice on top."""
    c = Canvas(28, 48)
    shade_box(c, 1, 2, 26, 47, BEAM, top_rows=3)
    c.box(1, 0, 26, 2, BEAM[3])
    c.box(13, 6, 14, 44, BEAM[0])
    c.put(11, 26, GOLD[2])
    c.put(16, 26, GOLD[2])
    c.outline()
    return c.img


def bookshelf():
    """A bookshelf against the wall: four shelves of spines in a few colours, leaning here and there."""
    c = Canvas(36, 50)
    shade_box(c, 1, 1, 34, 49, BEAM, top_rows=2)
    colours = ((150, 40, 50), (50, 80, 130), (60, 110, 70), (180, 140, 60), (110, 60, 110))
    for shelf in range(4):
        y0 = 5 + shelf * 11
        c.box(3, y0 + 9, 32, y0 + 9, BEAM[0])
        x = 3
        i = 0
        while x < 31:
            w = 2 + int(hash01(x, shelf, 341) * 2)
            h = 6 + int(hash01(shelf, x, 342) * 3)
            colour = colours[int(hash01(x, shelf, 343) * len(colours))]
            c.box(x, y0 + 9 - h, min(31, x + w - 1), y0 + 8, colour)
            c.box(x, y0 + 9 - h, x, y0 + 8, tuple(min(255, v + 40) for v in colour))
            x += w + (1 if i % 3 == 2 else 0)
            i += 1
    c.outline()
    return c.img


def shop_counter():
    """A shop's counter: a wide top with goods laid out on a cloth, and a little scale."""
    c = Canvas(64, 30)
    shade_box(c, 1, 6, 62, 29, BEAM, top_rows=7)
    c.box(8, 6, 40, 11, CLOTH[2])
    c.box(8, 11, 40, 12, CLOTH[0])
    for x in (12, 20, 28, 36):
        c.box(x, 7, x + 3, 10, (200, 120, 90) if x % 8 else (230, 200, 120))
    c.box(50, 0, 51, 8, GOLD[1])
    c.box(46, 2, 55, 2, GOLD[2])
    c.outline()
    return c.img


def candle_stand():
    """A tall iron candle stand with three candles burning."""
    c = Canvas(16, 40)
    c.box(7, 8, 8, 38, IRON[1])
    c.box(7, 8, 7, 38, IRON[2])
    c.box(3, 36, 12, 39, IRON[0])
    c.box(2, 10, 13, 11, IRON[1])
    for x in (2, 7, 12):
        c.box(x, 4, x + 1, 9, (240, 230, 210))
        c.put(x, 3, FIRE[2])
        c.put(x, 2, FIRE[3])
    c.outline()
    return c.img


FURNITURE = {
    "bar-counter": bar_counter,
    "round-table": round_table,
    "stool": stool,
    "hearth": hearth,
    "keg-stack": keg_stack,
    "bed": bed,
    "wardrobe": wardrobe,
    "bookshelf": bookshelf,
    "shop-counter": shop_counter,
    "candle-stand": candle_stand,
}


if __name__ == "__main__":
    print("Generating the insides of Questeria's buildings for Legend Quest...")
    build_tiles()
    for name, draw in FURNITURE.items():
        write_png(os.path.join(OUT, "Props", name + ".png"), draw())
    print("Done.")
