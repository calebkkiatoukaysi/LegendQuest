"""
Inside Questeria's buildings: the interior tileset, the bed for the cottages, and the tavern
furniture my props sheet hasn't got (a table with the tankards on it, and a high-backed booth).

    python tools/generate_interiors.py

The same camera as outside: you see the floor from above and the back wall from the front, two
tiles tall (plaster with a beam over wainscoting), and the side and front walls as their tops
only, so the room's open towards you. Warm wood and cream plaster, lit from the top left and
shadowed lavender, in the colours of my style sheet. The rug has edges the tilemap draws wherever it
meets the floor, like the lawns outside.

The rest of the furniture (the bar, the tables, the hearth and the shelves) used to be drawn here
too, but my props sheet has better of all of it, so tools/cut_supplied.py cuts it from there now
under the same names. There's no bed on the sheet, so the bed's still drawn here, standing on its
foot like every other prop, and so are the taverns' tankard table and booth, in the colours of the
cut furniture beside them.

Only needs CPython. Every bit of noise is hash01 on where it is, so a rerun writes the same bytes.
"""

import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from pixels import OUT, hash01, new_image, write_png  # noqa: E402
from generate_harbour import T, Canvas, dither, put, tile, turned  # noqa: E402

FLOOR = ((106, 66, 50), (146, 96, 64), (184, 130, 84), (214, 166, 108), (236, 198, 140))
STONE = ((112, 104, 116), (150, 140, 146), (184, 174, 172), (212, 204, 196))
PLASTER = ((178, 164, 170), (214, 202, 196), (236, 226, 214), (250, 244, 232))
BEAM = ((60, 38, 36), (96, 62, 48), (132, 90, 62), (168, 120, 80))
RUG = ((92, 24, 40), (138, 36, 52), (176, 58, 64), (208, 92, 84))
GOLD = ((150, 104, 40), (214, 166, 74), (246, 214, 130))
GLASS = ((84, 130, 176), (140, 188, 226), (210, 236, 250))
CLOTH = ((60, 70, 120), (86, 104, 160), (140, 156, 206), (200, 210, 236))
PAGE = ((150, 120, 90), (206, 180, 140))
OAK = ((70, 40, 34), (104, 64, 46), (140, 90, 58), (178, 120, 74), (212, 156, 100), (236, 192, 134))
PEWTER = ((70, 72, 90), (116, 120, 140), (168, 172, 190), (222, 226, 236))
ALE = ((150, 90, 30), (214, 150, 52), (250, 244, 226))
CUSHION = ((70, 22, 40), (110, 34, 56), (150, 54, 70), (186, 86, 92))
CANDLE = ((214, 200, 170), (246, 238, 214), (255, 214, 120), (255, 248, 214))


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


TANKARD_W, TANKARD_H = 32, 28


def tankard(c, x, y, tipped=False):
    """A pewter tankard with its head of foam, standing with its base at x, y, the handle on its
    right. A tipped one lies on its side with the ale run out of it."""
    if tipped:
        c.box(x - 3, y - 3, x + 2, y - 1, PEWTER[2])
        c.box(x - 3, y - 1, x + 2, y - 1, PEWTER[1])
        c.put(x - 3, y - 3, PEWTER[3])
        c.box(x + 3, y - 3, x + 3, y - 1, ALE[2])
        for dx in range(4, 9):
            c.put(x + dx, y - 1, ALE[1] if dx < 7 else ALE[0])
        return
    c.box(x - 2, y - 6, x + 1, y, PEWTER[2])
    c.box(x - 2, y - 6, x - 2, y, PEWTER[3])
    c.box(x + 1, y - 5, x + 1, y, PEWTER[1])
    c.box(x - 2, y, x + 1, y, PEWTER[0])
    c.box(x - 2, y - 8, x + 1, y - 7, ALE[2])
    c.put(x - 1, y - 9, ALE[2])
    c.put(x + 2, y - 5, PEWTER[1])
    c.put(x + 3, y - 4, PEWTER[1])
    c.put(x + 3, y - 3, PEWTER[1])
    c.put(x + 2, y - 2, PEWTER[1])


def tankard_table():
    """A heavy oak tavern table seen from above and in front: its top of three boards, scarred and
    ringed where the tankards stand, a thick front edge, and square legs. On it, two tankards with
    their heads on, one knocked over with the ale running out, and a stub of candle in a dish."""
    c = Canvas(TANKARD_W, TANKARD_H)
    top, front, foot = 9, 18, TANKARD_H - 1
    for y in range(top, front):
        board = (y - top) // 3
        for x in range(1, TANKARD_W - 1):
            k = 4 if y == top else 3 if (y - top) % 3 else 2
            if hash01(x // 3 + board * 5, y, 621) < 0.15:
                k -= 1
            c.put(x, y, OAK[k])
    for x, y in ((6, 12), (7, 13), (8, 12), (22, 15), (23, 14)):
        c.put(x, y, OAK[1])
    for y in range(front, front + 3):
        for x in range(1, TANKARD_W - 1):
            c.put(x, y, OAK[3] if y == front else OAK[1] if x > TANKARD_W // 2 else OAK[2])
    for lx in (2, TANKARD_W - 5):
        c.box(lx, front + 3, lx + 2, foot, OAK[1])
        c.box(lx, front + 3, lx, foot, OAK[2])
    c.box(TANKARD_W // 2 - 1, front + 3, TANKARD_W // 2, foot - 2, OAK[0])
    tankard(c, 7, front - 3)
    tankard(c, 14, front - 1)
    tankard(c, 22, front - 4, tipped=True)
    # The candle in its dish at the back corner, unlit: the room's dark enough to have its own light.
    c.box(24, top - 1, 28, top, PEWTER[1])
    c.box(25, top - 6, 26, top - 2, CANDLE[1])
    c.put(25, top - 6, CANDLE[0])
    c.put(26, top - 7, CANDLE[2])
    c.put(26, top - 8, CANDLE[3])
    c.outline()
    c.glow(26, top - 8, 5, CANDLE[2], 100)
    return c.img


BOOTH_W, BOOTH_H = 56, 46


def booth():
    """A high-backed booth for a quiet word: an oak settle against nothing at all, its back panelled
    and crested, a crimson cushion along its seat, and a tall end either side coming forward to the
    floor, so whoever sits in it is half hidden from the room. A brass lamp hook on its crest."""
    c = Canvas(BOOTH_W, BOOTH_H)
    left, right = 6, BOOTH_W - 7
    seat = 30
    # The back: panels between rails, an arched crest over the top.
    for y in range(4, seat):
        for x in range(left, right + 1):
            crest = y >= 8 or abs(x + 0.5 - BOOTH_W / 2.0) < 10 + (y - 4) * 2.5
            if not crest:
                continue
            rail = y in (8, seat - 1) or (x - left) % 11 in (0, 1)
            if rail:
                k = 4 if y == 8 or (x - left) % 11 == 0 else 3
            else:
                k = 2 if y < 14 else 1
                if (x - left) % 11 == 2 or y == 9:
                    k = 0
                elif hash01(x, y // 3, 631) < 0.2:
                    k += 1
            if y < 8:
                k = 5 if y == 4 or abs(x + 0.5 - BOOTH_W / 2.0) > 10 + (y - 5) * 2.5 else 4
            c.put(x, y, OAK[k])
    # The cushion, and the seat's front.
    for y in range(seat, seat + 5):
        for x in range(left, right + 1):
            k = 3 if y == seat else 2 if y < seat + 3 else 1
            if y == seat + 1 and (x - left) % 7 == 3:
                k = 0
            c.put(x, y, CUSHION[k])
    for y in range(seat + 5, BOOTH_H - 1):
        for x in range(left, right + 1):
            c.put(x, y, OAK[2] if y == seat + 5 else OAK[1] if (x - left) % 8 else OAK[0])
    # The two tall ends, their front edges toward the room.
    for x0 in (0, right + 1):
        for y in range(2, BOOTH_H - 1):
            for x in range(x0, x0 + 6):
                k = 4 if x == x0 or y == 2 else 3 if x < x0 + 3 else 2
                if y > BOOTH_H - 4:
                    k = 1
                c.put(x, y, OAK[k])
        c.put(x0 + 2, 1, OAK[4])
        c.put(x0 + 3, 1, OAK[3])
    # The lamp hook.
    c.box(BOOTH_W // 2 - 1, 0, BOOTH_W // 2, 2, GOLD[1])
    c.put(BOOTH_W // 2 - 1, 0, GOLD[2])
    c.outline()
    return c.img


#: The furniture drawn here: just what my props sheet hasn't got.
FURNITURE = {
    "bed": bed,
    "tankard-table": tankard_table,
    "booth": booth,
}


if __name__ == "__main__":
    print("Generating the insides of Questeria's buildings for Legend Quest...")
    print("  Tiles/interior.png: painted by tools/rooms/paint_tiles.py now; not written here")
    for name, draw in FURNITURE.items():
        write_png(os.path.join(OUT, "Props", name + ".png"), draw())
    print("Done.")
