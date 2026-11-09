"""
Sunspire Keep: the keep's tileset and its furniture, and the stair down to the Old Crown Vault on
Questeria's terrace.

    python tools/generate_keep.py

The keep is the palace on the terrace from the inside, so it's the same white-gold stone, only
polished: marble floors in big squares with a vein or two and a sheen on them, some with gold laid
in the joints, the royal blue carpet with its red-and-gold border, and the dais the throne stands on
with steps up to it. The walls are three tiles tall, which is a storey grander than a house's: a
gold cornice along the top, courses of marble, and a dado of blue marble panelled in gold along the
bottom. The windows are tiles of their own, a top and a bottom, so a map says where each one goes
(the variants of a tile are picked by where it is, and a window can't turn up just anywhere).

The furniture's drawn here to match my props sheet: lit from the top left, shadows leaning
lavender, dark outlines, and a little too much gold. The throne with the sun behind it, the
Aurelian sun on blue banners, marble pillars, a knight in marble, the war table with Fable's map on
it, the desk, the wine table, braziers, the arch between the throne room and the great hall, and
the sunlight falling from the windows across the floor. The game draws the braziers' flames.

Only needs CPython. Every bit of noise is hash01 on where it is, so a rerun writes the same bytes.
"""

import math
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from pixels import OUT, hash01, lerp_color, new_image, write_png  # noqa: E402
from generate_harbour import T, OUTLINE, Canvas, build_sheet, dither, put, tile, turned  # noqa: E402

# --------------------------------------------------------------------------- #
# The palette: the terrace's cream stone polished up, with lavender in its shadows, gold, the
# palace's royal blue, crimson for the borders, and the sky through the windows
# --------------------------------------------------------------------------- #

MARBLE = ((150, 132, 156), (190, 174, 186), (218, 204, 204), (236, 224, 212), (247, 239, 226), (255, 251, 240))
LAVENDER = ((132, 126, 166), (170, 166, 198), (196, 194, 220), (214, 212, 232), (228, 226, 242), (244, 242, 252))
VEIN = ((214, 200, 206), (232, 210, 170))
GOLD = ((112, 64, 30), (166, 106, 38), (212, 152, 52), (240, 196, 86), (254, 230, 146), (255, 248, 212))
ROYAL = ((16, 20, 58), (28, 40, 104), (42, 62, 150), (64, 94, 194), (110, 142, 222))
CRIMSON = ((70, 14, 34), (118, 26, 48), (162, 40, 58), (204, 68, 72), (232, 112, 98))
SKY = ((36, 62, 140), (60, 104, 194), (104, 162, 232), (176, 220, 252), (240, 252, 255))
WALNUT = ((52, 30, 30), (84, 50, 40), (120, 74, 50), (158, 104, 66), (196, 140, 92))
PARCHMENT = ((176, 146, 104), (214, 188, 140), (238, 218, 176), (250, 240, 214))
LAND = ((92, 132, 74), (128, 164, 86), (170, 196, 110))
SEA = ((62, 112, 164), (96, 150, 196))
IRON = ((34, 32, 46), (62, 60, 78), (98, 98, 118), (140, 142, 160))
COALS = ((96, 26, 18), (176, 52, 22), (236, 120, 40), (255, 204, 96))
WINE = ((70, 10, 30), (120, 20, 46), (170, 40, 64))
CRYSTAL = ((150, 190, 214), (206, 232, 244), (250, 254, 255))
SUNLIGHT = (255, 214, 120)

#: The dark line round every prop: the harbour's, a touch warmer, the same as the cut props'.
INK = OUTLINE


def ramp_at(ramp, k):
    """A shade off a ramp, clamped to its ends."""
    return ramp[max(0, min(len(ramp) - 1, int(k)))]


# --------------------------------------------------------------------------- #
# Floors: marble in squares, marble with gold in the joints, the carpet, the dais and its steps
# --------------------------------------------------------------------------- #
# The marble's laid in squares of eight, four to a tile, every square lit along its top and left
# and in shadow along its bottom and right, cream and lavender by turns like a chessboard. Every
# variant has the same joints, so a floor of mixed variants has no seams; what changes is the veins.

SLAB = 8


def vein(img, x, y, seed, colour, steps, avoid):
    """A hairline vein wandering down and across a slab from x, y, stopping at a joint."""
    for step in range(steps):
        if avoid(x, y):
            return
        put(img, x, y, colour)
        x += 1 if hash01(step, x, seed) < 0.62 else 0
        y += 1 if hash01(y, step, seed + 1) < 0.7 else 0


def marble_floor(variant, seed, warm=0.0, checker=True):
    """The marble squares: four to a tile, cream and lavender by turns (or all cream), their edges
    lit and shaded, a few specks, a sheen across the top left of each, and the odd vein (which
    variant has which)."""
    img = tile()

    def at_joint(x, y):
        return x % SLAB == SLAB - 1 or y % SLAB == SLAB - 1 or x >= T or y >= T

    for y in range(T):
        for x in range(T):
            sx, sy = x % SLAB, y % SLAB
            ramp = LAVENDER if checker and (x // SLAB + y // SLAB) % 2 else MARBLE
            if sx == SLAB - 1 or sy == SLAB - 1:
                c = MARBLE[1]
            elif sy == 0 or sx == 0:
                c = ramp[5]
            elif sy == SLAB - 2 or sx == SLAB - 2:
                c = ramp[2]
            else:
                k = 4 if sx + sy < 7 else 3
                if sx + sy < 4:
                    k = 5 if dither(x, y, 0.4) else 4
                n = hash01(x, y, seed)
                if n < 0.05:
                    k -= 1
                c = ramp[k]
            if warm:
                c = lerp_color(c, GOLD[4], warm)
            put(img, x, y, c)
    for i in range(4):
        if hash01(i, variant, seed + 3) < 0.3:
            ox, oy = (i % 2) * SLAB, (i // 2) * SLAB
            vx = ox + 1 + int(hash01(variant, i, seed + 4) * 4)
            colour = VEIN[1] if hash01(i, i + variant, seed + 5) < 0.4 else VEIN[0]
            vein(img, vx, oy + 1, seed + 6 + i * 3 + variant * 17, colour, 4, at_joint)
    return img


def marble(variant):
    """The keep's floor."""
    return marble_floor(variant, 501)


def gold_joints(img):
    """Lays gold into every joint of a marble tile: lit gold along the top and left of each run,
    darker along the bottom and right, and a bright point where four squares meet."""
    for y in range(T):
        for x in range(T):
            sx, sy = x % SLAB, y % SLAB
            if sx == SLAB - 1 and sy == SLAB - 1:
                put(img, x, y, GOLD[5])
            elif sx == SLAB - 1 or sy == SLAB - 1:
                put(img, x, y, GOLD[3] if (x + y) % 3 else GOLD[4])
            elif sy == SLAB - 2 or sx == SLAB - 2:
                put(img, x, y, MARBLE[1])
    return img


def marble_gold(variant):
    """Marble with gold laid in its joints, for round the carpet and before the dais. The second
    variant has a little gold star where the four squares in the middle of the tile meet."""
    img = gold_joints(marble_floor(variant + 3, 511, checker=False))
    if variant == 1:
        c = SLAB - 1
        for dx, dy, k in ((0, -2, 3), (0, 2, 3), (-2, 0, 3), (2, 0, 3), (0, -1, 4), (0, 1, 4), (-1, 0, 4), (1, 0, 4),
                          (-1, -1, 2), (1, 1, 2), (-1, 1, 2), (1, -1, 2), (0, -3, 2), (0, 3, 2), (-3, 0, 2), (3, 0, 2)):
            put(img, c + dx, c + dy, GOLD[k])
        put(img, c, c, GOLD[5])
    return img


def carpet(variant):
    """The royal carpet: deep blue wool woven in a twill, with a trellis of lighter blue across it
    and a gold knot where its lines cross, so a runner sparkles a little down its length."""
    img = tile()
    for y in range(T):
        for x in range(T):
            c = ROYAL[2]
            if (x + 2 * y) % 5 == 0:
                c = ROYAL[1]
            u, v = (x + y) % 8, (x - y) % 8
            if u == 0 or v == 0:
                c = ROYAL[3]
            if (x % 8, y % 8) in ((4, 4), (0, 0)):
                c = GOLD[4]
            elif (x % 8, y % 8) in ((3, 4), (5, 4), (4, 3), (4, 5), (7, 0), (1, 0), (0, 7), (0, 1)):
                c = GOLD[2]
            if variant == 1 and hash01(x, y, 521) < 0.04 and c == ROYAL[2]:
                c = ROYAL[3]
            put(img, x, y, c)
    return img


def carpet_edges():
    """The carpet's border, along whichever side meets the floor: a dark hem, a gold cord, a band
    of crimson with a gold stitch along it, and a gold line inside. The north and west borders
    catch the light."""
    edges = []
    for side in range(4):
        lit = side in (0, 3)
        img = tile()
        for x in range(T):
            put(img, x, 0, CRIMSON[0])
            put(img, x, 1, GOLD[4] if lit else GOLD[3])
            put(img, x, 2, CRIMSON[3] if lit else CRIMSON[2])
            put(img, x, 3, GOLD[3] if x % 4 == 1 else CRIMSON[2])
            put(img, x, 4, CRIMSON[1])
            put(img, x, 5, GOLD[2] if x % 2 else GOLD[3])
            put(img, x, 6, ROYAL[0])
        edges.append(turned(img, side))
    return edges


def dais(variant):
    """The dais the throne stands on: the same squares as the floor, warmed toward gold, with gold
    in the joints."""
    return gold_joints(marble_floor(variant + 6, 531, warm=0.4, checker=False))


def dais_edges():
    """The dais's rim: a gold moulding along its back and sides, and along its front, where it
    drops to the floor, the moulding over a riser of marble in shadow."""
    edges = []
    for side in range(4):
        img = tile()
        if side == 2:
            for x in range(T):
                put(img, x, 10, GOLD[5])
                put(img, x, 11, GOLD[3])
                put(img, x, 12, GOLD[1])
                put(img, x, 13, MARBLE[2] if x % 8 else MARBLE[1])
                put(img, x, 14, MARBLE[1])
                put(img, x, 15, MARBLE[0])
        else:
            for x in range(T):
                put(img, x, 0, GOLD[1])
                put(img, x, 1, GOLD[4] if side != 1 else GOLD[3])
                put(img, x, 2, GOLD[2])
            img = turned(img, side)
        edges.append(img)
    return edges


def steps():
    """Steps up to the dais, two to a tile: each tread lit, a gold nosing along its edge, and its
    riser in shadow under it, the joints between the stones staggered."""
    img = tile()
    for y in range(T):
        row = y % 8
        for x in range(T):
            if row == 0:
                c = MARBLE[5]
            elif row < 3:
                c = MARBLE[4] if dither(x, y, 0.8) else MARBLE[3]
            elif row == 3:
                c = GOLD[4] if x % 4 else GOLD[3]
            elif row == 4:
                c = GOLD[1]
            elif row < 7:
                c = MARBLE[2] if row == 5 else MARBLE[1]
            else:
                c = MARBLE[0]
            put(img, x, y, c)
        if y % 8 in (1, 2, 5, 6):
            put(img, (7 if y < 8 else 15), y, MARBLE[1] if y % 8 < 3 else MARBLE[0])
    return img


# --------------------------------------------------------------------------- #
# Walls: the top from above, then three rows of face: the cornice, the courses, the dado
# --------------------------------------------------------------------------- #

def wall_top(variant):
    """The top of a wall, seen from above: polished coping with a gold line inlaid down it, and a
    shadow along each edge."""
    img = tile()
    for y in range(T):
        for x in range(T):
            if y in (0, 15):
                c = MARBLE[1]
            elif y == 1:
                c = MARBLE[5]
            elif y == 14:
                c = MARBLE[2]
            elif y == 7:
                c = GOLD[4]
            elif y == 8:
                c = GOLD[2]
            elif x % 8 == 7 and y not in (6, 9):
                c = MARBLE[2]
            else:
                c = MARBLE[4] if (y < 7) else MARBLE[3]
                if hash01(x, y, 541 + variant) < 0.05:
                    c = MARBLE[2]
            put(img, x, y, c)
    return img


#: The courses of marble in a wall's face, top to bottom: the row each starts on and how tall it is,
#: and where its joints are. They're staggered, and the same in every variant of a row.
HIGH_COURSES = ((6, 5, (7, 15)), (11, 5, (3, 11)))
FACE_COURSES = ((0, 5, (7, 15)), (5, 5, (3, 11)), (10, 6, (7, 15)))


def courses(img, rows, seed, variant):
    """Courses of ashlar in a wall's face: each block lit along its top and left, its bottom in
    shadow, a joint between, a speck or two, and in the third variant a gold vein across one."""
    for top, height, joints in rows:
        for dy in range(height):
            y = top + dy
            for x in range(T):
                if dy == height - 1 or x in joints:
                    c = MARBLE[1]
                elif dy == 0 or x - 1 in joints or x == 0 and 15 in joints:
                    c = MARBLE[5]
                elif dy == height - 2:
                    c = MARBLE[3]
                else:
                    c = MARBLE[4]
                    if hash01(x, y, seed + variant) < 0.05:
                        c = MARBLE[3]
                put(img, x, y, c)
    if variant == 2:
        top, height, joints = rows[-2]
        x0 = joints[0] + 2
        for i in range(5):
            put(img, x0 + i, top + 1 + (i * 3) // 5, VEIN[1])


def cornice(img):
    """The gold cornice along the top of a wall's face, and the dentils under it."""
    for x in range(T):
        put(img, x, 0, GOLD[5])
        put(img, x, 1, GOLD[3])
        put(img, x, 2, GOLD[1])
        put(img, x, 3, MARBLE[5])
        put(img, x, 4, MARBLE[3] if x % 3 else MARBLE[0])
        put(img, x, 5, MARBLE[1] if x % 3 else MARBLE[0])


def wall_high(variant):
    """The top row of a wall's face: the cornice, then two courses of marble."""
    img = tile()
    cornice(img)
    courses(img, HIGH_COURSES, 551, variant)
    return img


def wall_face(variant):
    """The middle row of a wall's face: three more courses. One variant in three has a vein of
    gold through a block."""
    img = tile()
    courses(img, FACE_COURSES, 561, variant)
    return img


def wall_base(variant):
    """The bottom row of a wall's face: a dado of royal blue marble along the floor, a gold rail
    over it and a gold skirting under it, panelled in gold. The second variant has a sun in the
    middle of its panel."""
    img = tile()
    for y in range(T):
        for x in range(T):
            if y == 0:
                c = GOLD[5]
            elif y == 1:
                c = GOLD[3]
            elif y == 2:
                c = GOLD[1]
            elif y == 13:
                c = GOLD[3]
            elif y == 14:
                c = GOLD[1]
            elif y == 15:
                c = ROYAL[0]
            else:
                frame = (x in (2, 13) and 4 <= y <= 11) or (y in (4, 11) and 2 <= x <= 13)
                if frame:
                    c = GOLD[4] if x == 2 or y == 4 else GOLD[2]
                else:
                    c = ROYAL[3] if y == 3 else ROYAL[2] if (x + y) % 7 else ROYAL[1]
                    if 3 <= x <= 12 and 5 <= y <= 10 and (x - y) % 6 == 0:
                        c = ROYAL[3]
            put(img, x, y, c)
    if variant == 1:
        for dx, dy, k in ((0, 0, 5), (1, 0, 4), (0, 1, 3), (1, 1, 2), (-1, 0, 3), (2, 0, 3), (0, -1, 4), (1, -1, 4),
                          (0, 2, 2), (1, 2, 2), (-1, -1, 3), (2, 2, 2), (2, -1, 3), (-1, 2, 2)):
            put(img, 7 + dx, 7 + dy, GOLD[k])
    return img


#: The window, in art pixels: its middle across the tile, how far the glass goes either side of
#: that, and where the middle of its round arch is down the top tile. The whole window is the two
#: tiles one over the other, counted down from the top of the top one.
WINDOW_MIDDLE = 8.0
WINDOW_HALF = 6.0
ARCH_CENTRE = 12.0

#: Where the glass stops in the bottom tile, and the sill under it.
WINDOW_SILL = 11


def glass(x, y):
    """The stained glass at x, y down the whole window: deep blue going pale toward the top where
    the sun's coming through, a gold mullion down the middle, a gold leading across it every few
    panes, and a glint of sun across the glass here and there."""
    dx = x + 0.5 - WINDOW_MIDDLE
    if -1 <= dx < 0:
        return GOLD[4]
    if 0 <= dx < 1:
        return GOLD[2]
    if y in (17, 23, 29):
        return GOLD[2] if dx > 0 else GOLD[3]
    if (x - y) % 9 in (0, 1) and y < 30:
        return SKY[4] if y < 20 else SKY[3]
    k = 3 if y < 15 else 2 if y < 24 else 1
    if abs(dx) > WINDOW_HALF - 1.5:
        k -= 1
    return SKY[max(0, k)]


def window_part(top):
    """One half of a tall arched window in a wall's face, the light round it bright on the marble:
    the top (the cornice, the arch with its gold frame and a ring of tracery in it) or the bottom
    (the glass going on down to its sill)."""
    img = tile()
    if top:
        cornice(img)
        courses(img, HIGH_COURSES, 551, 0)
    else:
        courses(img, FACE_COURSES, 561, 0)
    for ty in range(T):
        y = ty + (0 if top else T)
        for x in range(T):
            dx = x + 0.5 - WINDOW_MIDDLE
            if top and ty < 6 or not top and ty >= WINDOW_SILL:
                continue
            if top and ty + 0.5 < ARCH_CENTRE:
                d = math.hypot(dx, ty + 0.5 - ARCH_CENTRE)
            else:
                d = abs(dx)
            if d < WINDOW_HALF:
                put(img, x, ty, glass(x, y))
            elif d < WINDOW_HALF + 1:
                put(img, x, ty, GOLD[4] if dx < 0 or ty + 0.5 < ARCH_CENTRE - 3 and top else GOLD[1])
            elif d < WINDOW_HALF + 2:
                put(img, x, ty, MARBLE[5] if dx < 0 else MARBLE[2])
            elif d < WINDOW_HALF + 3.5:
                under = img[ty][x][:3]
                put(img, x, ty, lerp_color(under, SUNLIGHT, 0.45))
    if top:
        for a in range(20):
            angle = a / 20.0 * 2 * math.pi
            put(img, round(WINDOW_MIDDLE - 0.5 + math.cos(angle) * 2.5), round(ARCH_CENTRE - 1.5 + math.sin(angle) * 2.5),
                GOLD[4] if math.sin(angle) < 0 else GOLD[2])
        put(img, round(WINDOW_MIDDLE - 0.5), round(ARCH_CENTRE - 1.5), SKY[4])
    else:
        for x in range(T):
            if abs(x + 0.5 - WINDOW_MIDDLE) < WINDOW_HALF + 2.5:
                put(img, x, WINDOW_SILL, MARBLE[5])
                put(img, x, WINDOW_SILL + 1, GOLD[3])
                put(img, x, WINDOW_SILL + 2, MARBLE[1])
    return img


#: The keep's tileset, in TilesetCatalog's order: name, variants (every frame of every variant),
#: and edges if it has them.
KEEP_TILES = (
    ("marble", [marble(v) for v in range(3)], None),
    ("marble-gold", [marble_gold(v) for v in range(2)], None),
    ("carpet", [carpet(v) for v in range(2)], carpet_edges()),
    ("dais", [dais(v) for v in range(2)], dais_edges()),
    ("steps", [steps()], None),
    ("wall-top", [wall_top(v) for v in range(2)], None),
    ("wall-high", [wall_high(v) for v in range(2)], None),
    ("wall-face", [wall_face(v) for v in range(3)], None),
    ("wall-base", [wall_base(v) for v in range(2)], None),
    ("window-high", [window_part(True)], None),
    ("window", [window_part(False)], None),
)


# --------------------------------------------------------------------------- #
# Drawing helpers for the furniture
# --------------------------------------------------------------------------- #

def sun(c, cx, cy, r, rays, reach, ramp=GOLD):
    """The Aurelian sun: a gold disc lit from the top left with a ring of rays round it, every
    other ray longer."""
    for y in range(int(cy - r - reach - 1), int(cy + r + reach + 2)):
        for x in range(int(cx - r - reach - 1), int(cx + r + reach + 2)):
            dx, dy = x + 0.5 - cx, y + 0.5 - cy
            d = math.hypot(dx, dy)
            light = -(dx + dy) / (d * 1.41 + 0.001)
            if d <= r:
                k = 3 + light * 1.4 + (0.6 if d < r * 0.45 else 0)
                c.put(x, y, ramp_at(ramp, round(k)) if d < r - 0.8 else ramp[1])
                continue
            a = math.atan2(dy, dx)
            ray = (math.cos(a * rays) + 1) / 2
            long_ray = math.cos(a * rays / 2) > 0
            if ray > 0.55 and d <= r + reach * (1.0 if long_ray else 0.6) * ray:
                c.put(x, y, ramp_at(ramp, round(2.5 + light)))


def gold_bar(c, x0, x1, y):
    """A gold rod across, lit along its top."""
    for x in range(x0, x1 + 1):
        c.put(x, y, GOLD[4])
        c.put(x, y + 1, GOLD[2])


# --------------------------------------------------------------------------- #
# The throne room
# --------------------------------------------------------------------------- #

THRONE_W, THRONE_H = 60, 76

#: The throne's sun: where its middle is, how big its disc is, and how far its rays reach.
THRONE_SUN = (30.0, 26.0)
THRONE_SUN_R, THRONE_SUN_REACH = 12, 15


def sun_throne():
    """King Leon's throne: a tall back of royal blue velvet in a gold frame with a little sun on
    its crest, a great sun blazing behind it, gold arms scrolled at the front with blue pads, a
    blue cushion with a crimson piping, a gold apron with the sun on it, and gold feet on a marble
    step of its own. A haze of gold round the sun, because it's that kind of room."""
    c = Canvas(THRONE_W, THRONE_H)
    cx = THRONE_W / 2.0
    sx, sy = THRONE_SUN
    sun(c, sx, sy, THRONE_SUN_R, 16, THRONE_SUN_REACH)

    def back(x, y):
        if not 17 <= x <= 42 or y > 56:
            return False
        return y >= 22 or math.hypot(x + 0.5 - cx, (y + 0.5 - 22) * 1.2) <= 13.5

    def velvet(x, y):
        if not 20 <= x <= 39 or y > 54:
            return False
        return y >= 22 or math.hypot(x + 0.5 - cx, (y + 0.5 - 22) * 1.2) <= 10.2

    for y in range(6, 57):
        for x in range(16, 44):
            if not back(x, y):
                continue
            if velvet(x, y):
                k = 3 if x < cx - 3 else 2
                if not velvet(x, y - 2) or not velvet(x - 2, y):
                    k = 4
                if (x - 20) % 5 == 2 and (y - 24) % 6 == 3:
                    c.put(x, y, GOLD[4])
                    continue
                c.put(x, y, ROYAL[k] if (x + y) % 9 else ROYAL[k - 1])
            else:
                k = 4 if x < cx else 2
                if not back(x, y - 1) or not back(x - 1, y):
                    k = 5
                elif not back(x + 1, y):
                    k = 1
                c.put(x, y, GOLD[k])
    sun(c, cx, 14, 3, 8, 2.5)
    # The arms: gold, a blue pad along each, scrolled at the front.
    for side in (-1, 1):
        ax0 = 10 if side < 0 else 43
        for y in range(42, 65):
            for x in range(ax0, ax0 + 7):
                k = 4 if x == ax0 or y == 42 else 3 if x < ax0 + 4 else 2
                if x == ax0 + 6:
                    k = 1
                c.put(x, y, GOLD[k])
        for x in range(ax0, ax0 + 7):
            c.put(x, 43, ROYAL[4])
            c.put(x, 44, ROYAL[2])
        scroll_x = ax0 + 3
        for a in range(14):
            angle = a / 14.0 * 2 * math.pi
            c.put(round(scroll_x + math.cos(angle) * 2.6), round(60 + math.sin(angle) * 2.6), GOLD[1] if a < 7 else GOLD[5])
        c.put(scroll_x, 60, GOLD[5])
    # The seat: a deep blue cushion, lit along its front edge, piped crimson, on a gold apron.
    for y in range(50, 58):
        for x in range(17, 43):
            k = 4 if y == 50 else 3 if y < 54 else 2
            c.put(x, y, ROYAL[k] if x > 17 else ROYAL[k - 1])
    for x in range(17, 43):
        c.put(x, 56, CRIMSON[3])
        c.put(x, 57, GOLD[1])
    for y in range(58, 66):
        for x in range(17, 43):
            k = 5 if y == 58 else 4 if x < cx - 6 else 3 if (x - 17) % 7 else 1
            c.put(x, y, GOLD[k] if y < 64 else GOLD[1])
    sun(c, cx, 61, 2, 8, 1.5)
    # Its feet, and the step it stands on.
    for lx in (18, 39):
        c.box(lx, 66, lx + 2, 69, GOLD[2])
        c.put(lx, 66, GOLD[4])
    for y in range(69, THRONE_H - 1):
        for x in range(4, THRONE_W - 4):
            k = 5 if y == 70 else 4 if y < 72 else 2 if y < THRONE_H - 2 else 1
            c.put(x, y, GOLD[4] if y == 69 else MARBLE[k])
    for x in range(4, THRONE_W - 4):
        c.put(x, 72, GOLD[3] if x % 2 else GOLD[2])
    c.outline(INK)
    c.glow(sx, sy, THRONE_SUN_R + THRONE_SUN_REACH + 6, GOLD[4], 110)
    return c.img


BANNER_W, BANNER_H = 20, 46


def sun_banner():
    """A banner of the Aurelian sun on the wall: a gold rod with finials, royal blue cloth lit down
    its left side with a crimson-and-gold border, the sun in the middle of it, a gold chevron under
    the sun, and a swallowtail with a tassel at each point."""
    c = Canvas(BANNER_W, BANNER_H)
    left, right, bottom = 2, BANNER_W - 3, BANNER_H - 3
    notch = 6
    mid = (left + right) / 2.0

    def cloth(x, y):
        if x < left or x > right or y < 3 or y > bottom:
            return False
        return y < bottom - notch + abs(x + 0.5 - mid - 0.5) * notch / ((right - left) / 2.0)

    for y in range(3, bottom + 1):
        for x in range(left, right + 1):
            if not cloth(x, y):
                continue
            edge = not cloth(x - 2, y) or not cloth(x + 2, y) or not cloth(x, y + 2)
            rim = not cloth(x - 1, y) or not cloth(x + 1, y) or not cloth(x, y + 1)
            if rim:
                c.put(x, y, GOLD[3] if x < mid else GOLD[2])
            elif edge:
                c.put(x, y, CRIMSON[3] if x < mid else CRIMSON[2])
            else:
                k = 3 if x < left + 4 else 2 if x < right - 3 else 1
                if (y - 3) % 9 == 0:
                    k = max(1, k - 1)
                c.put(x, y, ROYAL[k])
    sun(c, mid + 0.5, 15, 3.2, 12, 3.5)
    for i in range(5):
        c.put(round(mid) - i, 26 + i, GOLD[4])
        c.put(round(mid) + 1 + i, 26 + i, GOLD[2])
    gold_bar(c, 0, BANNER_W - 1, 1)
    for fx in (0, BANNER_W - 1):
        c.put(fx, 0, GOLD[5])
    for tx in (left, right):
        tip = bottom
        c.put(tx, tip + 1, GOLD[3])
        c.put(tx, tip + 2, GOLD[2])
    c.outline(INK)
    return c.img


PILLAR_W, PILLAR_H = 22, 76


def pillar_marble():
    """A marble column: a square plinth with a gold band, a fluted shaft lit down its left side, a
    gold ring a third of the way up, and a gilded capital under a marble slab."""
    c = Canvas(PILLAR_W, PILLAR_H)
    cx = PILLAR_W / 2.0
    shaft_l, shaft_r = 5, PILLAR_W - 6
    for y in range(12, PILLAR_H - 10):
        for x in range(shaft_l, shaft_r + 1):
            u = (x - shaft_l) / float(shaft_r - shaft_l)
            k = 5 if u < 0.2 else 4 if u < 0.45 else 3 if u < 0.7 else 2 if u < 0.9 else 1
            if (x - shaft_l) % 3 == 2 and 0.1 < u < 0.95:
                k = max(0, k - 1)
            c.put(x, y, MARBLE[k])
    ring = PILLAR_H - 30
    for x in range(shaft_l - 1, shaft_r + 2):
        c.put(x, ring, GOLD[4] if x < cx else GOLD[3])
        c.put(x, ring + 1, GOLD[2])
        c.put(x, ring + 2, GOLD[1])
    # The capital: a gold band of leaves flaring out from the shaft, a scroll curling down at
    # each end, and the marble slab over it.
    for y in range(5, 13):
        spread = (12 - y) * 0.5
        for x in range(int(shaft_l - spread), int(shaft_r + spread) + 1):
            k = 4 if x < cx - 2 else 3 if x < cx + 3 else 2
            if y in (8, 11) or (x + (y // 3)) % 3 == 0 and y < 11:
                k -= 1
            c.put(x, y, GOLD[k])
    for scroll_x, k in ((2, 4), (PILLAR_W - 3, 2)):
        for a in range(12):
            angle = a / 12.0 * 2 * math.pi
            c.put(round(scroll_x + math.cos(angle) * 1.8), round(7 + math.sin(angle) * 1.8), GOLD[k if a > 5 else 1])
        c.put(scroll_x, 7, GOLD[5])
    for x in range(1, PILLAR_W - 1):
        c.put(x, 4, GOLD[4] if x < cx else GOLD[3])
    for y in range(0, 4):
        for x in range(0, PILLAR_W):
            c.put(x, y, MARBLE[5] if y == 0 else MARBLE[4] if y < 3 else MARBLE[1])
    # The base: a gold torus, then the plinth.
    for x in range(shaft_l - 2, shaft_r + 3):
        c.put(x, PILLAR_H - 10, GOLD[4])
        c.put(x, PILLAR_H - 9, GOLD[2])
    for y in range(PILLAR_H - 8, PILLAR_H):
        for x in range(1, PILLAR_W - 1):
            k = 5 if y == PILLAR_H - 8 else 4 if x < 4 else 3 if x < PILLAR_W - 4 else 2
            if y == PILLAR_H - 1:
                k = 1
            c.put(x, y, MARBLE[k])
    for x in range(1, PILLAR_W - 1):
        c.put(x, PILLAR_H - 4, GOLD[3] if x % 2 else GOLD[2])
    c.outline(INK)
    return c.img


STATUE_W, STATUE_H = 30, 68

#: The statue's stone: whiter and cooler than the walls, so it stands out against them.
STATUE = ((112, 106, 138), (150, 146, 176), (186, 184, 208), (214, 214, 232), (234, 234, 246), (252, 252, 255))


def knight_part(x, y):
    """Which part of the marble knight a pixel is, front to back, or None: the sword, his
    gauntlets on its pommel, his arms, his pauldrons, his helm and its crest, his tabard, his legs,
    and his cloak behind it all. x is from the middle of the figure; y from the top of it."""
    ax = abs(x)
    if -1 <= x < 1 and 30 <= y < 50:
        return "blade"
    if -4 <= x < 4 and y == 30:
        return "guard"
    if -1 <= x < 1 and 23 <= y < 30:
        return "grip"
    if -3 <= x < 3 and 24 <= y < 29 and not (-1 <= x < 1):
        return "hands"
    if 23 <= y < 29:
        reach = 8.5 - (y - 23) * 0.9
        if reach - 3.5 <= ax < reach:
            return "arm"
    if 15 <= y < 23 and 6 <= ax < 10.5 - (1 if y > 20 else 0) - (1 if y < 16 else 0):
        return "pauldron"
    if 15 <= y < 23 and ax < 6.5:
        return "tabard"
    if 1 <= y < 5 and ax < 1.5 + (y - 1) * 0.3:
        return "crest"
    if 5 <= y < 15 and math.hypot(x + 0.5, (y - 10) * 0.95) < 5.2:
        return "helm"
    if 23 <= y < 41 and ax < 5.5 + (y - 23) * 0.18:
        return "tabard"
    if 41 <= y < 50 and 1.5 <= ax < 5:
        return "leg"
    if 49 <= y < 51 and 1 <= ax < 6:
        return "leg"
    if 15 <= y < 50 and ax < 9.5 + (y - 15) * 0.08:
        return "cloak"
    return None


#: Each part's stone (or gold) and the shade its middle is.
KNIGHT_SHADES = {
    "blade": (STATUE, 4), "guard": (GOLD, 3), "grip": (GOLD, 2), "hands": (STATUE, 3), "arm": (STATUE, 3),
    "pauldron": (STATUE, 4), "crest": (GOLD, 3), "helm": (STATUE, 4), "tabard": (STATUE, 3),
    "leg": (STATUE, 3), "cloak": (STATUE, 1),
}


def guard_statue():
    """A knight of the old war in white marble on a plinth: a great helm with a gold crest, broad
    pauldrons, a tabard with the sun on it in gold, a cloak behind, and both gauntlets on the
    pommel of a sword stood point down before him. Every part's lit from the left and darker where
    it meets the part in front of it, so it reads at a glance."""
    c = Canvas(STATUE_W, STATUE_H)
    cx = STATUE_W // 2
    figure = 2
    parts = {}
    for y in range(STATUE_H):
        for x in range(STATUE_W):
            part = knight_part(x - cx, y - figure)
            if part:
                parts[(x, y)] = part
    for (x, y), part in parts.items():
        ramp, mid = KNIGHT_SHADES[part]
        k = mid
        if parts.get((x - 1, y)) != part or parts.get((x, y - 1)) != part:
            k = mid + 1
        if x - cx > 2 and part not in ("blade", "grip", "guard"):
            k -= 1
        if parts.get((x + 1, y)) not in (part, None) or parts.get((x, y + 1)) not in (part, None):
            k -= 2
        if part == "blade":
            k = 5 if x < cx else 2
        c.put(x, y, ramp_at(ramp, k))
    # The helm's slit and breaths, the tabard's sun and hem, the belt.
    for x in range(cx - 4, cx + 4):
        c.put(x, figure + 9, STATUE[0])
    for x in (cx - 2, cx, cx + 2):
        c.put(x, figure + 12, STATUE[1])
    sun(c, cx, figure + 19, 2.0, 8, 1.4)
    for x in range(cx - 6, cx + 6):
        if parts.get((x, figure + 31)) == "tabard":
            c.put(x, figure + 31, GOLD[3] if x < cx else GOLD[2])
    for x in range(cx - 7, cx + 8):
        if parts.get((x, figure + 39)) == "tabard":
            c.put(x, figure + 39, GOLD[2])
    # The plinth: marble, a gold band, and its panel.
    top = STATUE_H - 15
    for y in range(top, STATUE_H):
        for x in range(1, STATUE_W - 1):
            k = 5 if y == top else 4 if x < 4 else 3 if x < STATUE_W - 4 else 2
            if y == STATUE_H - 1:
                k = 1
            c.put(x, y, MARBLE[k])
    for x in range(1, STATUE_W - 1):
        c.put(x, top + 2, GOLD[4] if x < cx else GOLD[3])
        c.put(x, top + 3, GOLD[1])
    for y in range(top + 6, STATUE_H - 2):
        c.put(5, y, MARBLE[1])
        c.put(STATUE_W - 6, y, MARBLE[5])
    for x in range(5, STATUE_W - 5):
        c.put(x, top + 6, MARBLE[1])
        c.put(x, STATUE_H - 3, MARBLE[5])
    c.outline(INK)
    return c.img


BRAZIER_W, BRAZIER_H = 20, 30

#: Where the brazier's coals are, up from its foot: where the game stands its flame.
BRAZIER_COALS = 20


def brazier():
    """A gold brazier on three legs, its bowl heaped with glowing coals. The game draws the flame."""
    c = Canvas(BRAZIER_W, BRAZIER_H)
    cx = BRAZIER_W / 2.0
    top = BRAZIER_H - BRAZIER_COALS - 1
    for y in range(top - 2, top + 2):
        for x in range(3, BRAZIER_W - 3):
            if math.hypot((x + 0.5 - cx) / 6.5, (y + 0.5 - top) / 2.2) < 1:
                heat = 1 - math.hypot((x + 0.5 - cx) / 6.5, (y + 0.5 - top) / 2.2)
                c.put(x, y, COALS[min(3, int(heat * 3.5 + hash01(x, y, 571) * 0.8))])
    for y in range(top + 1, top + 8):
        half = 7.5 - (y - top - 1) * 0.6
        for x in range(int(cx - half), int(cx + half) + 1):
            k = 4 if x < cx - half + 3 else 3 if x < cx + 1 else 2
            if y == top + 1:
                k = 5
            c.put(x, y, GOLD[k])
    for x in range(1, BRAZIER_W - 1):
        if abs(x + 0.5 - cx) < 8.5:
            c.put(x, top + 1, GOLD[5] if x < cx else GOLD[4])
    for y in range(top + 8, BRAZIER_H - 1):
        t = (y - top - 8) / float(BRAZIER_H - top - 9)
        for lx, k in ((cx - 2 - t * 5, 4), (cx + 1 + t * 5, 2), (cx - 0.5, 3)):
            c.put(round(lx), y, GOLD[k])
    for lx in (round(cx - 7), round(cx + 6)):
        c.put(lx, BRAZIER_H - 2, GOLD[1])
    c.outline(INK)
    c.glow(cx, top - 1, 9, COALS[3], 90)
    return c.img


ARCH_W, ARCH_H = 72, 60

#: How wide the way through the arch is, in art pixels: PropCatalog's gap.
ARCH_GAP = 40


def keep_arch():
    """The arch from the great hall into the throne room: marble jambs with gold capitals, a round
    arch over the way through with a gold keystone bearing the sun, and the gilded doors standing
    open against the jambs."""
    c = Canvas(ARCH_W, ARCH_H)
    cx = ARCH_W / 2.0
    half = ARCH_GAP / 2.0
    spring = 26
    for y in range(4, ARCH_H):
        for x in range(0, ARCH_W):
            dx = x + 0.5 - cx
            opening = abs(dx) < half and (y >= spring or math.hypot(dx, (y + 0.5 - spring) * 1.15) < half)
            if opening:
                continue
            outer = abs(dx) < cx - 1 and (y >= spring - 2 or math.hypot(dx, (y + 0.5 - spring) * 1.15) < cx - 1)
            if not outer:
                continue
            jamb = abs(dx) >= half
            ring = not jamb or y < spring
            if ring and math.hypot(dx, (y + 0.5 - spring) * 1.15) < half + 4 and y < spring + 1:
                k = 4 if dx < 0 else 2
                c.put(x, y, GOLD[k] if math.hypot(dx, (y + 0.5 - spring) * 1.15) < half + 1.5 else MARBLE[k + 1])
                continue
            u = (abs(dx) - half) / (cx - half)
            k = 5 if (dx < 0 and u < 0.15) else 4 if dx < 0 else 3 if u < 0.5 else 2
            if (y - 4) % 7 == 6:
                k = 1
            c.put(x, y, MARBLE[k])
    # Capitals where the arch springs, and bases.
    for side in (-1, 1):
        x0 = 0 if side < 0 else int(cx + half)
        x1 = int(cx - half) - 1 if side < 0 else ARCH_W - 1
        for x in range(x0 + 1, x1):
            c.put(x, spring - 1, GOLD[4])
            c.put(x, spring, GOLD[2])
            c.put(x, ARCH_H - 5, GOLD[4])
            c.put(x, ARCH_H - 4, GOLD[2])
            c.put(x, ARCH_H - 1, MARBLE[1])
        # The door leaf folded back against the jamb: its gilt edge and a sliver of its face.
        edge = int(cx - half) if side < 0 else int(cx + half) - 1
        for y in range(spring + 2, ARCH_H - 1):
            c.put(edge, y, GOLD[4] if side < 0 else GOLD[2])
            c.put(edge - side, y, ROYAL[2] if (y % 6) else GOLD[3])
    # The keystone with the sun on it.
    top = spring - int(half / 1.15) - 4
    for y in range(top, top + 9):
        for x in range(int(cx) - 4, int(cx) + 4):
            c.put(x, y, GOLD[4] if x < cx else GOLD[2])
    sun(c, cx, top + 4, 2, 8, 2)
    c.outline(INK)
    return c.img


INLAY_SIZE = 72

#: The sun inlaid in the floor before the dais: its disc, the ring round it, and how far its rays go.
INLAY_DISC, INLAY_RING, INLAY_RAYS = 9, 18, 34


def sun_inlay():
    """The Aurelian sun laid into the floor in gold before the dais: a gold disc, a ring round it,
    and sixteen rays out to a point, every other one longer, each a gold line with a lit edge. It
    lies on the ground, under everyone."""
    img = new_image(INLAY_SIZE, INLAY_SIZE)
    mid = INLAY_SIZE / 2.0
    for y in range(INLAY_SIZE):
        for x in range(INLAY_SIZE):
            dx, dy = x + 0.5 - mid, y + 0.5 - mid
            d = math.hypot(dx, dy)
            a = math.atan2(dy, dx)
            colour = None
            if d < INLAY_DISC:
                colour = GOLD[4] if dx + dy < -4 else GOLD[3] if d < INLAY_DISC - 1 else GOLD[1]
            elif INLAY_RING - 1 <= d < INLAY_RING + 1:
                colour = GOLD[4] if d < INLAY_RING else GOLD[2]
            elif INLAY_DISC + 1 <= d < INLAY_RAYS:
                ray = round(a / (2 * math.pi) * 16)
                long_ray = ray % 2 == 0
                reach = INLAY_RAYS if long_ray else INLAY_RING + 8
                t = (d - INLAY_DISC) / float(reach - INLAY_DISC)
                across = d * math.sin(a - ray * 2 * math.pi / 16)
                half = (2.4 if long_ray else 1.8) * (1 - t) + 0.45
                if d < reach and abs(across) < half:
                    colour = GOLD[4] if across < 0 else GOLD[2]
                    if INLAY_RING - 1 <= d < INLAY_RING + 1:
                        colour = GOLD[3]
            if colour:
                img[y][x] = [colour[0], colour[1], colour[2], 255]
    return img


#: The sun through a pair of windows: how big its picture is, and where along its top the
#: windows are, from its left edge. The map lines that up under the pair.
SHAFT_W, SHAFT_H = 50, 84
SHAFT_TOP = 14


def light_shaft():
    """The sun through a pair of tall windows, lying across the floor: gold light that's strongest
    by the wall and leans away to the right as it goes, fading out, with motes of dust in it. It
    lies on the ground, under everyone."""
    img = new_image(SHAFT_W, SHAFT_H)
    for y in range(SHAFT_H):
        t = y / float(SHAFT_H - 1)
        centre = SHAFT_TOP + t * 18
        half = 11 + t * 6
        fade = (1 - t) ** 1.1
        for x in range(SHAFT_W):
            d = abs(x + 0.5 - centre) / half
            if d >= 1:
                continue
            a = 150 * fade * (1 - d ** 4)
            if abs(x + 0.5 - centre) < 1.5:
                a *= 0.75
            if hash01(x, y, 601) > 0.985:
                a = min(255, a * 2.2)
            img[y][x] = [SUNLIGHT[0], SUNLIGHT[1], SUNLIGHT[2], max(0, min(255, int(a)))]
    return img


# --------------------------------------------------------------------------- #
# The war room, the study, and the wine
# --------------------------------------------------------------------------- #

WAR_W, WAR_H = 66, 40


def war_table():
    """The war table: a long walnut table with gold corners, Fable spread across it on parchment
    (its green valleys, the river, the sea to the west, the dark of Doomward to the east), red and
    blue markers on it, and a pair of candlesticks."""
    c = Canvas(WAR_W, WAR_H)
    top, front, legs = 8, 26, 32
    for y in range(top, front):
        for x in range(2, WAR_W - 2):
            k = 3 if y == top else 2
            c.put(x, y, WALNUT[k])
    for y in range(front, legs):
        for x in range(2, WAR_W - 2):
            k = 3 if y == front else 2 if x < WAR_W // 2 else 1
            if (x - 2) % 16 == 0:
                k = 1
            c.put(x, y, WALNUT[k])
    for x in range(2, WAR_W - 2):
        c.put(x, front + 1, GOLD[3] if x % 2 else GOLD[2])
    for lx in (4, WAR_W - 7, 20, WAR_W - 23):
        c.box(lx, legs, lx + 2, WAR_H - 2, WALNUT[1])
        c.put(lx, legs, WALNUT[3])
        c.put(lx + 1, WAR_H - 2, GOLD[2])
    for gx in (2, WAR_W - 4):
        c.box(gx, top, gx + 1, top + 1, GOLD[4])
        c.box(gx, front - 1, gx + 1, front, GOLD[3])
    # The map: parchment, the land, the sea to the west, the river, and Doomward's shadow.
    mx0, my0, mx1, my1 = 7, top + 2, WAR_W - 8, front - 2
    for y in range(my0, my1 + 1):
        for x in range(mx0, mx1 + 1):
            u, v = (x - mx0) / float(mx1 - mx0), (y - my0) / float(my1 - my0)
            coast = 0.16 + 0.06 * math.sin(v * 7)
            c.put(x, y, PARCHMENT[2] if (x + y) % 5 else PARCHMENT[1])
            if u < coast:
                c.put(x, y, SEA[1] if (x + y) % 3 else SEA[0])
            elif u > 0.8 + 0.05 * math.sin(v * 9):
                c.put(x, y, (96, 74, 96) if (x + y) % 2 else (76, 56, 78))
            elif hash01(x // 3, y // 2, 581) < 0.42:
                c.put(x, y, LAND[1] if hash01(x, y, 582) < 0.6 else LAND[2])
    for i in range(40):
        t = i / 39.0
        x = mx0 + (mx1 - mx0) * (0.18 + 0.55 * t)
        y = my0 + (my1 - my0) * (0.5 + 0.25 * math.sin(t * 6))
        c.put(round(x), round(y), SEA[0])
    for y in range(my0, my1 + 1):
        c.put(mx0 - 1, y, PARCHMENT[0])
        c.put(mx1 + 1, y, PARCHMENT[0])
    for x in range(mx0, mx1 + 1):
        c.put(x, my0 - 1, PARCHMENT[3])
        c.put(x, my1 + 1, PARCHMENT[0])
    # Markers: the crown's blue, the enemy's red, and a gold one on Questeria.
    for px, py, col in ((0.3, 0.3, ROYAL), (0.42, 0.62, ROYAL), (0.55, 0.4, ROYAL), (0.74, 0.45, CRIMSON),
                        (0.77, 0.7, CRIMSON), (0.7, 0.2, CRIMSON), (0.22, 0.55, GOLD)):
        x, y = round(mx0 + (mx1 - mx0) * px), round(my0 + (my1 - my0) * py)
        c.put(x, y - 2, col[3])
        c.put(x, y - 1, col[2])
        c.put(x, y, col[1])
        c.put(x + 1, y - 1, col[1])
    # Candlesticks at the two far corners.
    for sx in (5, WAR_W - 7):
        c.box(sx, 2, sx + 1, top + 1, (244, 236, 220))
        c.put(sx, 1, (255, 214, 120))
        c.put(sx, 0, (255, 246, 206))
        c.box(sx - 1, top, sx + 2, top + 1, GOLD[3])
    c.outline(INK)
    for sx in (5, WAR_W - 7):
        c.glow(sx + 0.5, 1, 4, (255, 214, 120), 110)
    return c.img


DESK_W, DESK_H = 50, 38


def royal_desk():
    """A writing desk fit for a chancellor: dark walnut inlaid with gold, a green leather top with
    letters on it and a sealed one, a book, an inkwell with its quill, and drawers with gold pulls."""
    c = Canvas(DESK_W, DESK_H)
    top, front = 10, 22
    for y in range(top, front):
        for x in range(1, DESK_W - 1):
            leather = 4 <= x <= DESK_W - 5 and top + 2 <= y <= front - 3
            c.put(x, y, ((46, 92, 72) if (x + y) % 6 else (38, 78, 62)) if leather else WALNUT[3] if y == top else WALNUT[2])
    for x in range(1, DESK_W - 1):
        c.put(x, front, GOLD[3])
    for y in range(front + 1, DESK_H - 1):
        for x in range(1, DESK_W - 1):
            k = 2 if x < DESK_W // 2 else 1
            panel = x in (3, 15, 34, 46) or y in (front + 2, DESK_H - 5)
            c.put(x, y, WALNUT[k + 1] if panel and x < DESK_W // 2 else WALNUT[k] if not panel else WALNUT[0])
    for kx in (9, 40):
        c.put(kx, front + 6, GOLD[4])
        c.put(kx + 1, front + 6, GOLD[2])
    c.box(16, front + 1, 33, DESK_H - 2, WALNUT[1])
    for lx in (1, DESK_W - 3):
        c.box(lx, DESK_H - 4, lx + 1, DESK_H - 1, WALNUT[1])
    # Letters, one with a red seal, an open book, the inkwell and quill.
    for (x0, y0, x1, y1) in ((8, top + 3, 17, top + 8), (12, top + 5, 21, top + 10)):
        c.box(x0, y0, x1, y1, PARCHMENT[3])
        for ly in range(y0 + 2, y1, 2):
            for lx in range(x0 + 1, x1 - 1):
                if hash01(lx, ly, 591) < 0.7:
                    c.put(lx, ly, PARCHMENT[0])
    c.box(17, top + 8, 18, top + 9, CRIMSON[2])
    c.put(17, top + 8, CRIMSON[4])
    c.box(26, top + 3, 37, top + 9, PARCHMENT[2])
    for y in range(top + 3, top + 10):
        c.put(31, y, PARCHMENT[0])
        c.put(32, y, WALNUT[1])
    c.box(26, top + 9, 37, top + 9, CRIMSON[1])
    c.box(40, top + 4, 43, top + 8, IRON[1])
    c.box(41, top + 4, 42, top + 4, IRON[3])
    for i in range(9):
        c.put(44 - i // 3, top + 3 - i, (250, 246, 236) if i % 2 else (220, 214, 206))
    c.outline(INK)
    return c.img


WINE_W, WINE_H = 26, 30


def wine_table():
    """A little round table of white marble on a gold stem, a crystal decanter of red wine on it and
    two goblets."""
    c = Canvas(WINE_W, WINE_H)
    cx = WINE_W / 2.0
    ty = 15
    for y in range(ty - 4, ty + 4):
        for x in range(1, WINE_W - 1):
            d = math.hypot((x + 0.5 - cx) / 11.5, (y + 0.5 - ty) / 3.6)
            if d < 1:
                c.put(x, y, MARBLE[5] if d < 0.4 and x < cx else MARBLE[4] if y < ty + 1 else MARBLE[3])
    for x in range(1, WINE_W - 1):
        if abs(x + 0.5 - cx) < 11:
            c.put(x, ty + 3, GOLD[3])
            c.put(x, ty + 4, GOLD[1])
    for y in range(ty + 5, WINE_H - 3):
        c.put(int(cx) - 1, y, GOLD[4])
        c.put(int(cx), y, GOLD[2])
    for x in range(int(cx) - 5, int(cx) + 5):
        c.put(x, WINE_H - 3, GOLD[3] if x < cx else GOLD[2])
        c.put(x, WINE_H - 2, GOLD[1])
    # The decanter: a round crystal belly of wine, a neck and a stopper.
    dx = int(cx) - 1
    for y in range(ty - 9, ty - 1):
        for x in range(dx - 3, dx + 4):
            if math.hypot(x + 0.5 - dx - 0.5, (y + 0.5 - (ty - 4)) * 1.1) < 3.6:
                c.put(x, y, WINE[2] if x < dx and y < ty - 4 else WINE[1])
    c.put(dx - 1, ty - 7, CRYSTAL[2])
    for y in range(ty - 13, ty - 8):
        c.put(dx, y, CRYSTAL[1])
        c.put(dx + 1, y, CRYSTAL[0])
    c.box(dx, ty - 14, dx + 1, ty - 14, GOLD[4])
    # Two goblets.
    for gx in (dx - 7, dx + 7):
        c.box(gx - 1, ty - 6, gx + 1, ty - 4, CRYSTAL[1])
        c.put(gx - 1, ty - 6, CRYSTAL[2])
        c.box(gx - 1, ty - 5, gx + 1, ty - 4, WINE[2])
        c.put(gx, ty - 3, GOLD[3])
        c.box(gx - 1, ty - 2, gx + 1, ty - 2, GOLD[2])
    c.outline(INK)
    return c.img


# --------------------------------------------------------------------------- #
# The stair down to the Old Crown Vault, on Questeria's terrace
# --------------------------------------------------------------------------- #

STAIR_W, STAIR_H = 52, 46

#: The well the stair goes down, inside the balustrade: left, right, its back wall, and the front
#: edge at the terrace's level.
WELL = (8, 43, 9, 43)


def vault_stair(shut):
    """A stairwell going down into the terrace beside the palace: a marble balustrade round three
    sides with gold-capped posts at its corners, steps going down into the dark toward an old
    iron-bound door under the palace, and across the top step an iron gate with gold spear points,
    shut or swung back against the sides."""
    c = Canvas(STAIR_W, STAIR_H)
    left, right, back, front = WELL
    mid = (left + right) / 2.0
    # The well: the far wall's face with the door in it at the top, then the steps up toward you,
    # lighter the nearer the terrace they are.
    wall_bottom = back + 12
    for y in range(back, front):
        for x in range(left, right):
            depth = (front - y) / float(front - back)
            if y < wall_bottom:
                door = abs(x + 0.5 - mid) < 6 and (y > back + 5 or math.hypot(x + 0.5 - mid, y - back - 5) < 6)
                if door:
                    k = 2 if x < mid else 1
                    colour = WALNUT[k] if (x - left) % 4 else IRON[0]
                    if y in (back + 6, back + 10):
                        colour = IRON[2]
                    c.put(x, y, colour)
                    continue
                course = (y - back) % 4 == 3 or (x + ((y - back) // 4) * 3) % 7 == 0
                c.put(x, y, lerp_color(MARBLE[1] if course else MARBLE[3], IRON[0], 0.72))
                continue
            row = (y - wall_bottom) % 5
            k = 5 if row == 0 else 4 if row < 3 else 2 if row == 3 else 1
            c.put(x, y, lerp_color(MARBLE[k], IRON[0], 0.82 * depth ** 0.8))
    for y in range(back, front):
        c.put(left, y, lerp_color(MARBLE[0], IRON[0], 0.6))
        c.put(right - 1, y, lerp_color(MARBLE[2], IRON[0], 0.4))
    # The balustrade: its coping seen from above round three sides, its inner faces dropping into
    # the well.
    for y in range(2, front + 1):
        for x in range(1, STAIR_W - 1):
            if left <= x < right and y >= back:
                continue
            k = 4
            if y == 2 or x == 1:
                k = 5
            elif x == STAIR_W - 2 or y == front:
                k = 2
            elif x in (left - 1, right) or y == back - 1:
                k = 3
            c.put(x, y, MARBLE[k])
    for x in range(1, STAIR_W - 1):
        if not left <= x < right:
            c.put(x, front + 1, MARBLE[1])
            c.put(x, front + 2, MARBLE[0])
    for x in range(left + 2, right - 2, 6):
        c.put(x, 5, GOLD[3])
        c.put(x + 1, 5, GOLD[2])
    # The posts at its corners, gold-capped.
    for px, py in ((1, 0), (STAIR_W - 7, 0), (1, front - 8), (STAIR_W - 7, front - 8)):
        c.box(px, py + 3, px + 5, py + 10, MARBLE[3])
        c.box(px, py + 3, px + 1, py + 10, MARBLE[5])
        c.box(px + 4, py + 3, px + 5, py + 10, MARBLE[2])
        c.box(px, py + 1, px + 5, py + 2, GOLD[3])
        c.box(px + 1, py, px + 4, py, GOLD[4])
        c.put(px + 2, py - 1, GOLD[5])
    # The gate across the top step: iron bars with gold spear points, shut or swung back.
    gy0, gy1 = front - 14, front - 1
    if shut:
        for x in range(left + 1, right - 1):
            if (x - left) % 3 == 1:
                for y in range(gy0, gy1):
                    c.put(x, y, IRON[3] if y < gy0 + 2 else IRON[2] if x < mid else IRON[1])
                c.put(x, gy0 - 1, GOLD[4])
                c.put(x, gy0 - 2, GOLD[5])
        for y in (gy0 + 2, gy1 - 3):
            for x in range(left, right):
                c.put(x, y, IRON[3] if y == gy0 + 2 else IRON[2])
        c.box(round(mid) - 2, gy0 + 5, round(mid) + 1, gy0 + 8, GOLD[2])
        c.box(round(mid) - 2, gy0 + 5, round(mid) - 1, gy0 + 6, GOLD[4])
        c.put(round(mid) - 1, gy0 + 7, IRON[0])
    else:
        for gx, step in ((left + 1, 1), (right - 2, -1)):
            for y in range(gy0, gy1):
                c.put(gx, y, IRON[3])
                c.put(gx + step, y, IRON[2] if y % 3 else IRON[1])
            c.put(gx, gy0 - 1, GOLD[4])
    c.outline(INK)
    return c.img


def keep_props():
    """The keep's props and the vault stair, by name."""
    return {
        "sun-throne": sun_throne(),
        "sun-banner": sun_banner(),
        "pillar-marble": pillar_marble(),
        "guard-statue": guard_statue(),
        "brazier": brazier(),
        "keep-arch": keep_arch(),
        "light-shaft": light_shaft(),
        "sun-inlay": sun_inlay(),
        "war-table": war_table(),
        "royal-desk": royal_desk(),
        "wine-table": wine_table(),
        "vault-stair": vault_stair(False),
        "vault-stair-shut": vault_stair(True),
    }


if __name__ == "__main__":
    print("Generating Sunspire Keep for Legend Quest...")
    print("  Tiles/keep.png: painted by tools/rooms/paint_tiles.py now; not written here")
    for name, picture in keep_props().items():
        write_png(os.path.join(OUT, "Props", name + ".png"), picture)
    print("Done.")
