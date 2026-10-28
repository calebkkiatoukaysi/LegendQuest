"""
Questeria's ground, and its harbour: every tile of the Questeria tileset, the ship you arrive on,
its gangplank, and the rowing boat moored by the quay.

    python tools/generate_harbour.py

The ground used to be cut from my ground sheet, but that was painted with its cobbles on the
diagonal, which reads as a different camera to everything standing on it. These are drawn straight
on, the way the camera sees them: the tops of things, lit from the top left, the shadows leaning
lavender. They're in the colours of my style sheet, so they sit under the cut props and people.

A tile that's "edged" has four more frames after its variants: what it looks like along its north,
east, south and west sides where it meets ground of another kind. Tilemap draws those itself, so a
lawn gets its kerb and the water its quay wall wherever the map puts them, without tiles for every
corner. Every tile wraps round left to right and top to bottom, so a field of one has no seams.

Only needs CPython. Every bit of noise is hash01 on where it is, so a rerun writes the same bytes.
"""

import math
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from pixels import OUT, hash01, new_image, write_png  # noqa: E402

T = 16

# --------------------------------------------------------------------------- #
# The palette, from the style sheet: pale warm stone, lavender shadow, spring green, sea blue
# --------------------------------------------------------------------------- #

STONE = ((150, 138, 160), (186, 174, 182), (214, 202, 196), (234, 224, 212), (248, 242, 230))
MORTAR = (156, 144, 164)
SLAB = ((160, 146, 160), (198, 184, 184), (224, 212, 200), (240, 232, 218), (252, 248, 238))
TERRACE = ((168, 156, 170), (206, 196, 198), (230, 222, 214), (244, 238, 228), (254, 252, 246))
GRASS = ((46, 96, 70), (72, 132, 70), (104, 166, 78), (140, 194, 92), (178, 216, 118))
FLOWERS = ((246, 160, 190), (252, 246, 232), (252, 220, 120), (176, 180, 246))
SAND = ((190, 160, 130), (218, 192, 152), (236, 214, 174), (248, 232, 198))
WATER = ((34, 84, 136), (44, 112, 170), (62, 146, 198), (104, 186, 222), (190, 234, 246))
WOOD = ((80, 50, 48), (118, 76, 56), (156, 106, 70), (190, 140, 92), (220, 178, 124))
OUTLINE = (52, 30, 44)


def tile():
    return new_image(T, T)


def put(img, x, y, c):
    img[y % len(img)][x % len(img[0])] = [c[0], c[1], c[2], 255]


def dither(x, y, t):
    """Whether an ordered-dither cell is lit at a level 0..1 (the 4x4 Bayer matrix)."""
    bayer = ((0, 8, 2, 10), (12, 4, 14, 6), (3, 11, 1, 9), (15, 7, 13, 5))
    return t * 16 > bayer[y % 4][x % 4]


# --------------------------------------------------------------------------- #
# Paving
# --------------------------------------------------------------------------- #

def stones(seed, rows, ramp, mortar, variant):
    """Paving laid in courses: `rows` is each course's height and the widths of the stones along
    it (they add up to the tile's width, so it wraps). Every stone's lit along its top and left and
    shaded along its bottom and right, with a speck or two, and a tone of its own."""
    img = tile()
    y = 0
    for course, (height, widths, shift) in enumerate(rows):
        x = shift
        for i, w in enumerate(widths):
            tone = hash01(course * 7 + i, variant, seed)
            base = 2 if tone < 0.6 else 3 if tone < 0.9 else 1
            for dy in range(height):
                for dx in range(w):
                    px, py = x + dx, y + dy
                    if dx == w - 1 or dy == height - 1:
                        put(img, px, py, mortar)
                        continue
                    c = base
                    if dy == 0 or dx == 0:
                        c = min(4, base + 1)
                    elif dy == height - 2 or dx == w - 2:
                        c = max(0, base - 1)
                    elif hash01(px, py, seed + variant * 31) < 0.08:
                        c = max(0, base - 1)
                    put(img, px, py, ramp[c])
            # Rounded corners: the mortar creeps into each one.
            put(img, x + w - 2, y + height - 2, ramp[max(0, base - 1)])
            put(img, x, y, ramp[base])
            x += w
        y += height
    return img


def cobble(variant):
    layouts = (
        ((4, (5, 6, 5), 0), (4, (4, 5, 7), 3), (4, (6, 5, 5), 1), (4, (5, 4, 7), 5)),
        ((4, (6, 5, 5), 2), (4, (5, 6, 5), 0), (4, (4, 7, 5), 4), (4, (6, 4, 6), 1)),
        ((4, (5, 5, 6), 4), (4, (7, 4, 5), 1), (4, (5, 6, 5), 3), (4, (4, 6, 6), 0)),
    )
    return stones(11, layouts[variant], STONE, MORTAR, variant)


def plaza(variant):
    """Big square slabs, two to a tile each way, every other course shifted half a slab."""
    layouts = (
        ((8, (8, 8), 0), (8, (8, 8), 4)),
        ((8, (8, 8), 4), (8, (8, 8), 0)),
        ((8, (8, 8), 0), (8, (8, 8), 4)),
    )
    return stones(23, layouts[variant], SLAB, MORTAR, variant)


def terrace(variant):
    """The palace terrace: long slabs, a tile wide and half a tile deep, shifted half a tile a course."""
    layouts = (((8, (16,), 0), (8, (16,), 8)), ((8, (16,), 8), (8, (16,), 0)))
    return stones(37, layouts[variant], TERRACE, MORTAR, variant)


# --------------------------------------------------------------------------- #
# Lawns, sand and water
# --------------------------------------------------------------------------- #

def grass(variant):
    """Short spring grass: dithered light and dark, little tufts of blades, and flowers on two of them."""
    img = tile()
    for y in range(T):
        for x in range(T):
            n = hash01(x, y, 41 + variant)
            m = (math.sin((x + variant * 5) * 0.8) + math.cos((y + variant * 3) * 0.9)) * 0.15 + 0.5
            c = 2
            if dither(x, y, m * 0.55) and n < 0.7:
                c = 3
            if n > 0.93:
                c = 1
            put(img, x, y, GRASS[c])
    for i in range(4):
        tx, ty = int(hash01(i, variant, 43) * T), int(hash01(variant, i, 44) * T)
        put(img, tx, ty, GRASS[4])
        put(img, tx + 1, ty + 1, GRASS[1])
        put(img, tx - 1, ty + 1, GRASS[1])
        put(img, tx, ty + 1, GRASS[3])
    if variant >= 2:
        for i in range(2 + variant):
            fx, fy = int(hash01(i, variant, 45) * T), int(hash01(variant, i, 46) * T)
            petal = FLOWERS[(i + variant) % len(FLOWERS)]
            for dx, dy in ((0, -1), (-1, 0), (1, 0), (0, 1)):
                put(img, fx + dx, fy + dy, petal)
            put(img, fx, fy, FLOWERS[2] if petal != FLOWERS[2] else FLOWERS[1])
            put(img, fx, fy + 2, GRASS[1])
    return img


def sand(variant):
    img = tile()
    for y in range(T):
        for x in range(T):
            n = hash01(x, y, 51 + variant)
            c = 2 if n < 0.75 else 1 if n < 0.9 else 3
            if dither(x, y, 0.3) and (x + y + variant) % 7 == 0:
                c = 1
            put(img, x, y, SAND[c])
    return img


def water(variant):
    """Open water: deep blue going lighter in long soft bands, with the glint of a ripple here and there."""
    img = tile()
    for y in range(T):
        for x in range(T):
            band = 0.5 + 0.5 * math.sin((y * 2 + variant * 3) * math.pi / T + math.sin(x * math.pi / 8) * 0.6)
            c = 1 if band < 0.35 else 2
            if band > 0.7 and dither(x, y, (band - 0.7) * 2):
                c = 3
            put(img, x, y, WATER[c])
    for i in range(2):
        rx, ry = int(hash01(i, variant, 61) * T), int(hash01(variant, i, 62) * T)
        for dx in range(3 + i):
            put(img, rx + dx, ry, WATER[4] if dx in (1, 2) else WATER[3])
        put(img, rx + 1, ry + 1, WATER[1])
    return img


def planks(variant):
    """Boards running across, three to a tile, with their joins staggered and a nail at each end."""
    img = tile()
    for row in range(3):
        y0 = row * 5 + (1 if row == 2 else 0)
        h = 5 if row < 2 else 6
        join = (4 + row * 6 + variant * 5) % T
        for dy in range(h):
            for x in range(T):
                y = y0 + dy
                if dy == h - 1:
                    put(img, x, y, WOOD[0])
                elif x == join:
                    put(img, x, y, WOOD[1])
                else:
                    c = 3 if dy == 0 else 2
                    grain = hash01(x // 3, y + row * 5, 71 + variant)
                    if grain < 0.15 and dy not in (0,):
                        c = 1
                    elif (x + row * 2) % 9 == 0 and dy == 2:
                        c = 1
                    put(img, x, y, WOOD[c])
        put(img, join + 1, y0 + 1, WOOD[0])
        put(img, join - 1, y0 + 1, WOOD[0])
    return img


# --------------------------------------------------------------------------- #
# Edges: drawn over a tile along whichever sides meet ground of another kind
# --------------------------------------------------------------------------- #

def turned(img, side):
    """A north edge turned to face another side: 0 north, 1 east, 2 south, 3 west."""
    out = tile()
    for y in range(T):
        for x in range(T):
            if side == 0:
                sx, sy = x, y
            elif side == 1:
                sx, sy = y, T - 1 - x
            elif side == 2:
                sx, sy = x, T - 1 - y
            else:
                sx, sy = y, x
            out[y][x] = list(img[sy][sx])
    return out


def kerb_edges():
    """A lawn's edges: a kerb of pale stone round it, the grass darker where it meets it. Lit on
    the north and west, so those kerbs are lighter than the south and east ones."""
    edges = []
    for side in range(4):
        lit = side in (0, 3)
        img = tile()
        for x in range(T):
            put(img, x, 0, STONE[4] if lit else STONE[2])
            put(img, x, 1, STONE[2] if lit else STONE[0])
            put(img, x, 2, GRASS[0] if x % 5 else GRASS[1])
            if hash01(x, side, 81) < 0.45:
                put(img, x, 3, GRASS[1])
        edges.append(turned(img, side))
    return edges


def water_edges():
    """Water's edges. Along the north it's the quay wall, seen from the front: coping, three courses
    of wet stone going darker, then the white of the water lapping at it. The other sides are a
    lip of coping with a line of foam inside it."""
    edges = []
    north = tile()
    for x in range(T):
        put(north, x, 0, STONE[4])
        put(north, x, 1, STONE[3] if x % 8 else STONE[1])
        for row, tone in ((2, 2), (3, 2), (4, 1), (5, 1), (6, 0), (7, 0)):
            joint = (x + (4 if row in (4, 5) else 0)) % 8 == 7
            put(north, x, row, MORTAR if joint or row in (3, 5) and x % 2 else STONE[tone])
        put(north, x, 7, (64, 80, 110))
        put(north, x, 8, WATER[4] if (x // 2) % 3 else WATER[3])
        if x % 4 == 1:
            put(north, x, 9, WATER[3])
    edges.append(north)
    for side in (1, 2, 3):
        img = tile()
        for x in range(T):
            put(img, x, 0, STONE[3] if side == 3 else STONE[2])
            put(img, x, 1, STONE[1] if side == 3 else STONE[0])
            put(img, x, 2, WATER[4] if (x + side) % 5 else WATER[3])
            if (x + side) % 3 == 0:
                put(img, x, 3, WATER[3])
        edges.append(turned(img, side))
    return edges


def plank_edges():
    """A boardwalk's edges: along the south, the beam its boards are nailed to and the tops of the
    posts under it; along the sides, the end of the boards. Nothing along the north, where it meets
    the quay, except a seam."""
    north = tile()
    for x in range(T):
        put(north, x, 0, WOOD[0])
    south = tile()
    for x in range(T):
        put(south, x, 12, WOOD[3])
        put(south, x, 13, WOOD[1])
        put(south, x, 14, WOOD[1] if x % 8 not in (3, 4) else WOOD[2])
        put(south, x, 15, OUTLINE)
    east, west = tile(), tile()
    for y in range(T):
        put(east, 15, y, WOOD[0])
        put(east, 14, y, WOOD[1])
        put(west, 0, y, WOOD[3])
        put(west, 1, y, WOOD[2])
    return [north, east, south, west]


# --------------------------------------------------------------------------- #
# The tileset, in TilesetCatalog's order
# --------------------------------------------------------------------------- #

#: Each tile: its name, its variants, and its edges if it has them. TilesetCatalog has the same
#: list in the same order, with Edged on the ones that have edges here.
TILES = (
    ("plaza", [plaza(v) for v in range(3)], None),
    ("terrace", [terrace(v) for v in range(2)], None),
    ("cobble", [cobble(v) for v in range(3)], None),
    ("grass", [grass(v) for v in range(4)], kerb_edges()),
    ("sand", [sand(v) for v in range(2)], None),
    ("water", [water(v) for v in range(3)], water_edges()),
    ("planks", [planks(v) for v in range(3)], plank_edges()),
)


def build_tiles():
    frames = []
    for _, variants, edges in TILES:
        frames += variants
        frames += edges or []
    columns = 8
    sheet = new_image(columns * T, (len(frames) + columns - 1) // columns * T)
    for i, f in enumerate(frames):
        for y in range(T):
            for x in range(T):
                if f[y][x][3]:
                    sheet[(i // columns) * T + y][(i % columns) * T + x] = list(f[y][x])
    write_png(os.path.join(OUT, "Tiles", "questeria.png"), sheet)


# --------------------------------------------------------------------------- #
# Boats
# --------------------------------------------------------------------------- #

class Canvas:
    """A picture being drawn: put a pixel, fill a box, draw a line, all clipped."""

    def __init__(self, w, h):
        self.w, self.h = w, h
        self.img = new_image(w, h)

    def put(self, x, y, c):
        if 0 <= x < self.w and 0 <= y < self.h:
            self.img[y][x] = [c[0], c[1], c[2], 255]

    def box(self, x0, y0, x1, y1, c):
        for y in range(y0, y1 + 1):
            for x in range(x0, x1 + 1):
                self.put(x, y, c)

    def line(self, x0, y0, x1, y1, c):
        steps = max(abs(x1 - x0), abs(y1 - y0), 1)
        for i in range(steps + 1):
            self.put(round(x0 + (x1 - x0) * i / steps), round(y0 + (y1 - y0) * i / steps), c)

    def filled(self, x, y):
        return 0 <= x < self.w and 0 <= y < self.h and self.img[y][x][3]

    def outline(self, c=OUTLINE):
        """A dark line round everything drawn, on the outside."""
        edge = [(x, y) for y in range(self.h) for x in range(self.w)
                if not self.filled(x, y) and any(self.filled(x + dx, y + dy) for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)))]
        for x, y in edge:
            self.put(x, y, c)


SAILCLOTH = ((150, 136, 146), (206, 194, 188), (236, 228, 216), (252, 248, 238))
WOAD = ((30, 44, 96), (46, 72, 146), (84, 120, 196))
GOLD = ((150, 98, 36), (214, 160, 66), (248, 214, 120))

SHIP_W, SHIP_H = 208, 136


def hull_top(x, length=SHIP_W):
    """How far down the frame the near side of the hull starts at a point along it: the deck line,
    which rises towards the bow."""
    t = x / length
    return 92 - int(10 * max(0, t - 0.62) / 0.38) ** 1 - (6 if x < 46 else 0)


def ship(furled):
    """The ship, side on, sailing to the left: its hull, its deck, the cabin at the stern, the mast
    and its sail, full when it's sailing in and furled when it's tied up. The gangplank's a prop of
    its own, so it can lie under whoever's walking up it."""
    c = Canvas(SHIP_W, SHIP_H)
    water_line = 124
    bow, stern = 12, 192

    # The deck, seen from above: from the far rail down to the near gunwale.
    for x in range(bow + 10, stern - 4):
        top = 66 if x < stern - 44 else 54
        for y in range(top, hull_top(SHIP_W - x) + 1):
            board = (y - top) % 4
            tone = 3 if board == 0 else 2 if board in (1, 2) else 1
            if (x + (y // 4) * 7) % 23 == 0:
                tone = 1
            c.put(x, y, WOOD[tone])
    # The near side of the hull: from the gunwale down to the water, rounding in at the bow.
    for x in range(bow, stern + 2):
        top = hull_top(SHIP_W - x) if x >= bow + 10 else hull_top(SHIP_W - bow - 10) + (bow + 10 - x)
        bottom = water_line - (max(0, bow + 22 - x) // 2)
        for y in range(top, bottom + 1):
            d = y - top
            if d == 0:
                col = WOOD[4]
            elif d in (3, 4):
                col = WOAD[2] if d == 3 else WOAD[1]
            elif d == 5:
                col = GOLD[1]
            else:
                col = WOOD[2] if (d // 4) % 2 else WOOD[1]
                if (d % 4) == 3:
                    col = WOOD[0]
                if y > bottom - 5:
                    col = WOOD[0] if d % 4 == 3 else WOOD[1]
            c.put(x, y, col)
    # The stern castle: a cabin with a door and a window, its roof a deck of its own with a rail.
    cx0, cx1 = stern - 44, stern - 2
    for y in range(40, 70):
        for x in range(cx0, cx1):
            if y < 46:
                col = WOOD[3] if (y - 40) % 3 else WOOD[2]
            else:
                col = WOOD[2] if (x - cx0) % 6 else WOOD[1]
            c.put(x, y, col)
    c.box(cx0, 46, cx1 - 1, 46, WOOD[0])
    c.box(cx0 + 8, 52, cx0 + 15, 69, WOOD[0])
    c.box(cx0 + 9, 53, cx0 + 14, 69, WOOD[1])
    c.put(cx0 + 13, 61, GOLD[2])
    c.box(cx0 + 24, 52, cx0 + 33, 59, OUTLINE)
    c.box(cx0 + 25, 53, cx0 + 32, 58, (110, 190, 220))
    c.box(cx0 + 25, 53, cx0 + 28, 55, (200, 240, 250))
    for x in range(cx0, cx1, 5):
        c.box(x, 34, x, 40, WOOD[1])
    c.box(cx0, 33, cx1 - 1, 34, WOOD[3])
    # The far rail along the deck.
    for x in range(bow + 12, cx0, 6):
        c.box(x, 60, x, 66, WOOD[1])
    c.box(bow + 10, 59, cx0, 60, WOOD[3])
    # The lantern on the stern.
    c.box(cx1 - 3, 24, cx1 - 3, 33, WOOD[0])
    c.box(cx1 - 5, 25, cx1 - 1, 29, GOLD[2])
    c.box(cx1 - 4, 26, cx1 - 2, 28, (255, 244, 200))
    # The bowsprit, out over the water.
    c.line(bow + 12, 78, 0, 64, WOOD[1])
    c.line(bow + 12, 79, 0, 65, WOOD[0])

    mast = 92
    # The mast and the yard.
    c.box(mast, 6, mast + 2, 80, WOOD[2])
    c.box(mast + 2, 6, mast + 2, 80, WOOD[1])
    c.box(mast, 6, mast, 80, WOOD[3])
    c.box(mast - 2, 2, mast + 4, 5, WOOD[1])
    yard_y = 14
    c.box(mast - 40, yard_y, mast + 42, yard_y + 1, WOOD[1])
    # The rigging, from the masthead to the bow and the stern.
    c.line(mast + 1, 4, 2, 64, WOOD[0])
    c.line(mast + 1, 4, cx1 - 3, 33, WOOD[0])
    # The pennant at the masthead.
    for i in range(10):
        c.put(mast + 3 + i, 2 + i // 4, WOAD[2] if i % 3 else WOAD[1])
        c.put(mast + 3 + i, 3 + i // 4, WOAD[1])
    if furled:
        for x in range(mast - 38, mast + 41):
            for y in range(yard_y + 2, yard_y + 6):
                bulge = 1 if (x // 6) % 2 else 0
                if y > yard_y + 4 + bulge:
                    continue
                c.put(x, y, SAILCLOTH[2] if y == yard_y + 2 else SAILCLOTH[1])
            if x % 9 == 0:
                c.box(x, yard_y + 1, x, yard_y + 6, WOOD[0])
    else:
        top, bottom = yard_y + 2, 62
        for y in range(top, bottom + 1):
            t = (y - top) / (bottom - top)
            belly = int(6 * math.sin(t * math.pi))
            x0, x1 = mast - 38 - belly // 2, mast + 40 + belly // 2
            for x in range(x0, x1 + 1):
                u = (x - x0) / max(1, x1 - x0)
                shade = 2
                if u < 0.12 or t > 0.9:
                    shade = 1
                elif u > 0.55 and dither(x, y, (u - 0.55) * 1.6):
                    shade = 3
                if y in (top + 14, top + 15):
                    col = WOAD[1] if y == top + 15 else WOAD[2]
                else:
                    col = SAILCLOTH[shade]
                c.put(x, y, col)
        # The crest on the sail: a star in gold on the blue band.
        sx, sy = mast + 1, top + 26
        for dx, dy in ((0, 0), (0, -1), (0, 1), (-1, 0), (1, 0), (0, -2), (0, 2), (-2, 0), (2, 0), (-1, -1), (1, 1), (1, -1), (-1, 1)):
            c.put(sx + dx, sy + dy, GOLD[2] if dx == 0 or dy == 0 else GOLD[1])
        for x in range(mast - 38, mast + 41, 3):
            c.put(x, bottom + 1, WOOD[0])
    c.outline()
    # The foam where the hull meets the water.
    for x in range(bow + 6, stern + 4):
        c.put(x, water_line + 1, WATER[4] if (x // 3) % 2 else WATER[3])
        if x % 5 == 0:
            c.put(x, water_line + 2, WATER[3])
    return c.img


def gangplank():
    """The gangplank, from the ship's deck up to the boardwalk: boards with a slat across every few
    pixels to stop feet slipping, and a rope along each side."""
    c = Canvas(12, 36)
    for y in range(36):
        for x in range(1, 11):
            if x in (1, 10):
                c.put(x, y, WOOD[0] if y % 6 else WOOD[1])
            elif y % 5 == 0:
                c.put(x, y, WOOD[1])
            else:
                c.put(x, y, WOOD[3] if x < 6 else WOOD[2])
    c.outline()
    return c.img


def rowboat():
    """A rowing boat moored by the quay, side on: hull, thwarts, a pair of oars shipped in it."""
    c = Canvas(44, 22)
    for x in range(3, 41):
        t = abs(x - 22) / 19
        top, bottom = 6 + int(4 * t * t), 16 - int(3 * t)
        for y in range(top, bottom + 1):
            d = y - top
            if d == 0:
                col = WOOD[4]
            elif y < 11 and 6 < x < 38:
                col = WOOD[1] if (x % 9 == 0) else WOOD[2]
            elif d in (5, 6):
                col = WOAD[2] if d == 5 else WOAD[1]
            else:
                col = WOOD[2] if d % 3 else WOOD[1]
            c.put(x, y, col)
    c.line(9, 8, 36, 5, WOOD[3])
    c.line(10, 9, 37, 6, WOOD[1])
    c.outline()
    for x in range(4, 41):
        c.put(x, 18, WATER[4] if x % 3 else WATER[3])
    return c.img


def side_by_side(frames):
    w, h = len(frames[0][0]), len(frames[0])
    sheet = new_image(w * len(frames), h)
    for i, f in enumerate(frames):
        for y in range(h):
            sheet[y][i * w:(i + 1) * w] = f[y]
    return sheet


if __name__ == "__main__":
    print("Generating Questeria's ground and its harbour for Legend Quest...")
    build_tiles()
    write_png(os.path.join(OUT, "Props", "ship.png"), side_by_side([ship(False), ship(True)]))
    write_png(os.path.join(OUT, "Props", "gangplank.png"), gangplank())
    write_png(os.path.join(OUT, "Props", "rowboat.png"), rowboat())
    print("Done.")
