"""
Questeria's ground, and its harbour: every tile of the Questeria tileset, the ship you come in on
with its near rail and gangplank, and the rowing boat moored by the jetty; and the Gull's Wager,
the ship the Prologue's on, with its own tileset (its deck, its rails, its bow and stern, its
lanterns and the night sea all round it), its mast and its wheel.

    python tools/generate_harbour.py

The ground used to be cut from my ground sheet, but that was painted with its cobbles on the
diagonal, which reads as a different camera to everything standing on it. These are drawn straight
on, the way the camera sees them: the tops of things, lit from the top left, the shadows leaning
lavender, in the colours of my style sheet and the ground sheet, so they sit under the cut props
and the people. The flagstones come in different sizes with the odd crack, moss in the joints and
a petal blown in; the grass has tufts and little flowers; the boards have grain and nails.

Every variant of a tile has the same edges as the others (the same courses, the same stones and
boards across the border), and only its middle changes. So a field of mixed variants has no seams
and doesn't show the grid.

A tile that's "edged" has four more frames after its variants: what it looks like along its north,
east, south and west sides where it meets ground of another kind. Tilemap draws those itself, so a
lawn gets its kerb and the water its quay wall wherever the map puts them.

A tile that moves (the water, the night sea, the lanterns) has its frames one after another for
each variant. The water's ripples come and go and the odd glint flashes; Tilemap starts every cell
on a frame of its own.

Only needs CPython. Every bit of noise is hash01 on where it is, so a rerun writes the same bytes.
With --ship-ends it prints what the ground under the Gull's Wager's bow and stern has to be instead.
"""

import math
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from pixels import OUT, hash01, lerp_color, new_image, write_png  # noqa: E402

T = 16

# --------------------------------------------------------------------------- #
# The palette, from the style sheet and the ground sheet: cream stone, lavender shadow, spring
# green, warm wood, azure water, gold and navy
# --------------------------------------------------------------------------- #

STONE = ((190, 178, 194), (212, 199, 198), (228, 212, 193), (238, 224, 205), (248, 240, 225))
MORTAR = ((166, 144, 146), (194, 170, 160))
COBBLE = ((150, 140, 164), (186, 176, 190), (210, 200, 204), (228, 220, 216), (244, 238, 230))
COBBLE_MORTAR = ((160, 148, 164), (180, 168, 180))
TERRACE = ((196, 186, 200), (218, 208, 208), (234, 224, 210), (244, 236, 222), (253, 249, 239))
COOL = (210, 206, 226)
WARM = (240, 214, 196)
MOSS = ((74, 112, 62), (112, 150, 70), (150, 180, 84))
GRASS = ((50, 94, 66), (82, 130, 62), (118, 160, 64), (150, 184, 72), (188, 208, 102))
PETALS = ((244, 150, 186), (252, 246, 236), (250, 214, 110), (190, 170, 240), (228, 108, 152))
SAND = ((172, 138, 110), (204, 170, 128), (226, 196, 150), (240, 216, 176), (250, 234, 206))
PEBBLE = ((150, 140, 150), (196, 188, 190), (226, 220, 216))
WATER = ((6, 58, 120), (10, 86, 158), (18, 112, 184), (28, 140, 206), (56, 180, 228), (144, 224, 246), (238, 252, 255))
QUAY = ((44, 86, 140), (66, 110, 160), (88, 132, 176), (112, 150, 186))
WOOD = ((84, 46, 40), (126, 72, 50), (162, 100, 62), (196, 128, 74), (226, 162, 98), (244, 196, 136))
OUTLINE = (58, 32, 38)
GOLD = ((132, 80, 34), (196, 138, 50), (236, 188, 82), (254, 234, 156))
NAVY = ((20, 28, 66), (32, 50, 110), (52, 82, 154), (96, 132, 200))
IRON = ((40, 40, 56), (78, 80, 98), (130, 134, 152))
GLASS = ((255, 168, 72), (255, 214, 120), (255, 246, 206))


def tile():
    return new_image(T, T)


def put(img, x, y, c):
    img[y % len(img)][x % len(img[0])] = [c[0], c[1], c[2], 255]


def dither(x, y, t):
    """Whether an ordered-dither cell is lit at a level 0..1 (the 4x4 Bayer matrix)."""
    bayer = ((0, 8, 2, 10), (12, 4, 14, 6), (3, 11, 1, 9), (15, 7, 13, 5))
    return t * 16 > bayer[y % 4][x % 4]


def blobs(seed, count, size, squash=1.4):
    """A few soft round patches that wrap round a tile, half of them light and half dark: -1 to 1
    at every pixel. They only depend on the seed, so every variant of a tile has the same ones."""
    field = [[0.0] * T for _ in range(T)]
    for i in range(count):
        cx, cy = hash01(i, 0, seed) * T, hash01(0, i, seed) * T
        r = size * (0.7 + 0.6 * hash01(i, i, seed))
        sign = 1 if i % 2 == 0 else -1
        for y in range(T):
            for x in range(T):
                dx = min(abs(x + 0.5 - cx), T - abs(x + 0.5 - cx))
                dy = min(abs(y + 0.5 - cy), T - abs(y + 0.5 - cy))
                d = math.hypot(dx, dy * squash) / r
                if d < 1:
                    field[y][x] += sign * (1 - d * d)
    return field


def tinted(c, tint, k):
    return lerp_color(c, tint, k) if k else c


# --------------------------------------------------------------------------- #
# Paving: flagstones in courses, cobbles, and the palace terrace
# --------------------------------------------------------------------------- #

def slabs(courses, ramp, mortar, seed, variant, moss=0, cracks=0, petals=0):
    """Stones laid in courses. Each course is its height, the joints every variant has (where the
    stones cross the tile's edges, so neighbours always meet) and the joints only this variant has.
    Every stone's lit along its top and left and lavender along its bottom and right, has a tone
    and a tint of its own, and a speck or two. Then whatever's grown or blown into it."""
    img = tile()
    stones = []
    y = 0
    for course, (height, shared, own) in enumerate(courses):
        joints = sorted(shared + own[variant % len(own)])
        for i, j in enumerate(joints):
            start, end = j + 1, joints[(i + 1) % len(joints)] - 1
            if end < start:
                end += T
            edge = start == 0 or end >= T - 1
            key = (course, T) if edge else (course, start, variant)
            stones.append((start, end, y, height, edge, key))
        for x in range(T):
            put(img, x, y + height - 1, mortar[1])
        for j in joints:
            for dy in range(height):
                put(img, j, y + dy, mortar[1])
        y += height

    for start, end, top, height, edge, key in stones:
        w = end - start + 1
        pick = hash01(len(key), sum(key) * 7 + key[0], seed + (0 if edge else variant * 13))
        base = 2 if pick < 0.62 else 3
        tint = COOL if hash01(key[0], key[1], seed + 5) < 0.3 else WARM if hash01(key[1], key[0], seed + 6) < 0.3 else None
        k = 0.35 if tint else 0
        for dy in range(height - 1):
            for dx in range(w):
                px, py = start + dx, top + dy
                c = base
                if dy == 0 and dx > 0:
                    c = min(4, base + 1)
                elif dx == 0 and dy > 0:
                    c = base
                elif dy == height - 2 or dx == w - 1:
                    c = 1
                else:
                    n = hash01(px % T, py, seed)
                    if n < 0.07:
                        c = base - 1
                    elif n > 0.96:
                        c = min(4, base + 1)
                if dx == 0 and dy == 0:
                    put(img, px, py, lerp_color(ramp[base], mortar[1], 0.4))
                    continue
                put(img, px, py, tinted(ramp[c], tint, k))
        put(img, end, top + height - 2, lerp_color(ramp[1], mortar[1], 0.5))

    def joint(x, y):
        return tuple(img[y % T][x % T][:3]) == mortar[1]

    corners = [(x, y) for y in range(2, T - 2) for x in range(2, T - 2)
               if joint(x, y) and (joint(x - 1, y) or joint(x + 1, y)) and (joint(x, y - 1) or joint(x, y + 1))]
    for i in range(moss):
        if not corners:
            break
        x, y = corners[int(hash01(i, variant, seed + 21) * len(corners))]
        put(img, x, y, MOSS[0])
        put(img, x + 1, y, MOSS[1])
        put(img, x - 1, y, MOSS[1])
        put(img, x, y - 1, MOSS[2])
    for i in range(cracks):
        x, y = 4 + int(hash01(i, variant, seed + 31) * 8), 3 + int(hash01(variant, i, seed + 32) * 9)
        for step in range(4):
            if tuple(img[y][x][:3]) in (mortar[0], mortar[1]):
                break
            put(img, x, y, lerp_color(ramp[0], mortar[0], 0.4))
            x, y = x + (1 if hash01(step, i, seed + 33 + variant) < 0.6 else 0), y + 1
    for i in range(petals):
        x, y = 3 + int(hash01(i, variant, seed + 41) * 10), 3 + int(hash01(variant, i, seed + 42) * 10)
        put(img, x, y, PETALS[0])
        put(img, x + 1, y, PETALS[4])
        put(img, x, y + 1, PETALS[4] if i % 2 else PETALS[0])
        put(img, x + 1, y - 1, PETALS[1])
    return img


PLAZA_COURSES = (
    (7, [0], [[], [9], [6]]),
    (9, [4], [[11], [], [13]]),
)


def plaza(variant):
    """The plaza and the streets: cream flagstones of all sizes. One variant in six has moss in a
    joint, one a crack and one a petal blown in, so they turn up now and then and not in a pattern."""
    return slabs(PLAZA_COURSES, STONE, MORTAR, 23, variant,
                 moss=int(variant == 2), cracks=int(variant == 1), petals=int(variant == 4))


TERRACE_COURSES = (
    (8, [0], [[], [8]]),
    (8, [8], [[], []]),
)


def terrace(variant):
    """The palace terrace: long pale slabs, a tile wide and half a tile deep, shifted half a tile a course."""
    return slabs(TERRACE_COURSES, TERRACE, MORTAR, 37, variant, cracks=variant)


def cobble(variant):
    """The quay street's cobbles: rounded stones packed in mortar, each one lit on its top left. The
    stones round the tile's edge are the same in every variant, so they always meet their neighbours;
    the ones in the middle change."""
    seeds = []
    for j in range(3):
        for i in range(3):
            if i == 1 and j == 1:
                continue
            seeds.append(((i + 0.5) * T / 3 + (hash01(i, j, 61) - 0.5) * 3.4,
                          (j + 0.5) * T / 3 + (hash01(j, i, 62) - 0.5) * 3.4, (i, j)))
    middle = ((8.0, 8.0), (6.6, 8.6), (9.4, 7.0))[variant]
    seeds.append((middle[0], middle[1], (9, variant)))
    if variant == 2:
        seeds.append((7.0, 10.6, (9, 9)))
    img = tile()
    for y in range(T):
        for x in range(T):
            near = []
            for sx, sy, key in seeds:
                for ox in (-T, 0, T):
                    for oy in (-T, 0, T):
                        dx, dy = x + 0.5 - (sx + ox), y + 0.5 - (sy + oy)
                        near.append((math.hypot(dx, dy * 1.15), dx, dy, key))
            near.sort(key=lambda n: n[0])
            d1, dx, dy, key = near[0]
            d2 = next(n[0] for n in near if n[3] != key)
            base = 2 if hash01(key[0], key[1], 63) < 0.55 else 3
            tint = WARM if hash01(key[1], key[0], 66) < 0.35 else COOL if hash01(key[0], key[1], 67) < 0.25 else None
            if d2 - d1 < 0.9:
                c = COBBLE_MORTAR[1] if hash01(x, y, 64) < 0.8 else COBBLE_MORTAR[0]
            elif d2 - d1 < 1.9:
                c = COBBLE[1] if dx + dy > 0 else COBBLE[min(4, base + 1)]
            elif dx + dy < -2.2 and d1 < 3.2:
                c = COBBLE[4] if dx + dy < -2.9 else COBBLE[min(4, base + 1)]
            else:
                n = hash01(x, y, 65)
                c = COBBLE[base - 1] if n < 0.08 else COBBLE[base]
            put(img, x, y, tinted(c, tint, 0.3) if tint and d2 - d1 >= 0.9 else c)
    return img


# --------------------------------------------------------------------------- #
# Lawns and sand
# --------------------------------------------------------------------------- #

def grass(variant):
    """Spring grass: soft light and dark across it, a blade here and there, tufts, and on two of
    them little flowers. The light and the blades are the same in every variant, so the lawn has
    no seams; the tufts and flowers are each one's own, and stay off its edges."""
    img = tile()
    light = blobs(71, 6, 5.0)
    for y in range(T):
        for x in range(T):
            m = light[y][x]
            c = 2
            if m > 0.2 and dither(x, y, (m - 0.2) * 1.6):
                c = 3
            elif m < -0.5 and dither(x, y, (-0.5 - m) * 1.2):
                c = 1
            n = hash01(x, y, 72)
            if n < 0.05:
                c = 1
            elif n > 0.95:
                c = 3
            put(img, x, y, GRASS[c])
    for x in range(T):
        for y in range(T):
            if hash01(x, y, 73) < 0.06:
                put(img, x, y, GRASS[4] if hash01(y, x, 74) < 0.5 else GRASS[3])
                put(img, x, y + 1, GRASS[1])
    for i in range(3):
        tx, ty = 2 + int(hash01(i, variant, 75) * 11), 2 + int(hash01(variant, i, 76) * 11)
        put(img, tx, ty, GRASS[4])
        put(img, tx - 1, ty + 1, GRASS[3])
        put(img, tx + 1, ty + 1, GRASS[3])
        put(img, tx, ty + 1, GRASS[2])
        put(img, tx - 1, ty + 2, GRASS[0])
        put(img, tx, ty + 2, GRASS[1])
        put(img, tx + 1, ty + 2, GRASS[0])
    if variant >= 2:
        for i in range(variant):
            fx, fy = 3 + int(hash01(i, variant, 77) * 9), 3 + int(hash01(variant, i, 78) * 9)
            petal = PETALS[(i * 3 + variant) % 4]
            for dx, dy in ((0, -1), (-1, 0), (1, 0), (0, 1)):
                put(img, fx + dx, fy + dy, petal)
            put(img, fx, fy, PETALS[2] if petal != PETALS[2] else PETALS[1])
            put(img, fx + 1, fy + 1, GRASS[0])
            put(img, fx, fy + 2, GRASS[1])
    return img


def sand(variant):
    """Warm sand, rippled by the wind, with a pebble or two and the odd shell."""
    img = tile()
    light = blobs(81, 6, 5.0, squash=2.5)
    for y in range(T):
        for x in range(T):
            m = light[y][x]
            c = 2
            if m > 0.3 and dither(x, y, m - 0.3):
                c = 3
            n = hash01(x, y, 83)
            if n < 0.05:
                c = 1
            elif n > 0.95:
                c = 4
            put(img, x, y, SAND[c])
    for i in range(1 + variant):
        px, py = 2 + int(hash01(i, variant, 84) * 12), 2 + int(hash01(variant, i, 85) * 12)
        put(img, px, py, PEBBLE[2])
        put(img, px + 1, py, PEBBLE[1])
        put(img, px, py + 1, SAND[1])
        put(img, px + 1, py + 1, SAND[0])
    return img


# --------------------------------------------------------------------------- #
# Water, which moves
# --------------------------------------------------------------------------- #

WATER_FRAMES = 4


def water_frames(variant, ramp=WATER, seed=91, ripples=5, glints=1):
    """Open water, every frame of one variant: deep blue with soft darker and lighter patches (the
    same in every variant and frame, so the water has no seams), short irregular ripples that
    brighten and fade each in its own time, a dark line under each one, and a glint that flashes
    on one frame. Nothing but the patches comes within a pixel of the tile's edge."""
    found = []
    for i in range(ripples):
        length = 2 + int(hash01(i, variant, seed + 1) * 4)
        x = 1 + int(hash01(variant, i, seed + 2) * (T - 2 - length))
        y = 1 + int(hash01(i, i + variant, seed + 3) * (T - 3))
        found.append((x, y, length, int(hash01(i, variant, seed + 4) * WATER_FRAMES)))
    sparks = [(2 + int(hash01(i, variant, seed + 5) * 12), 2 + int(hash01(variant, i, seed + 6) * 12),
               int(hash01(variant + 3, i, seed + 7) * WATER_FRAMES)) for i in range(glints)]

    patches = [(int(hash01(i, 0, seed + 8) * T), int(hash01(0, i, seed + 8) * T),
                3 + int(hash01(i, i, seed + 9) * 5), 1 + (i % 3 == 0), 1 if i % 2 else 3) for i in range(7)]

    frames = []
    for f in range(WATER_FRAMES):
        img = tile()
        for y in range(T):
            for x in range(T):
                put(img, x, y, ramp[2])
        for px, py, w, h, c in patches:
            for dy in range(h):
                for dx in range(w):
                    if (dx, dy) not in ((0, h - 1), (w - 1, 0)) or h == 1:
                        put(img, px + dx, py + dy, ramp[c])
        for x, y, length, phase in found:
            level = (0, 1, 2, 1)[(f + phase) % WATER_FRAMES]
            if level == 0:
                put(img, x + length // 2, y, ramp[3])
                continue
            shown = length if level == 2 else max(1, length - 2)
            start = x + (length - shown) // 2
            for dx in range(shown):
                put(img, start + dx, y, ramp[5] if level == 2 and 0 < dx < shown - 1 else ramp[4])
                put(img, start + dx, y + 1, ramp[1])
        for x, y, phase in sparks:
            if f == phase:
                put(img, x, y, ramp[6])
                for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                    put(img, x + dx, y + dy, ramp[5])
            elif f == (phase + 1) % WATER_FRAMES:
                put(img, x, y, ramp[5])
        frames.append(img)
    return frames


# --------------------------------------------------------------------------- #
# Boards
# --------------------------------------------------------------------------- #

def boards(ramp, seed, variant, rows=((0, 5), (5, 5), (10, 6)), joins=0.75):
    """Boards running across, a few to a tile, each row a board of its own shade with grain running
    along it, a lit top and a dark gap under it. Where one board ends and the next starts there's a
    join with a nail either side, somewhere different in every variant, so a boardwalk doesn't show
    the tiles; nothing else about a row changes from variant to variant, so the boards carry on
    across the tiles without a seam. The odd knot, too."""
    img = tile()
    for row, (top, height) in enumerate(rows):
        tone = 0.94 + 0.12 * hash01(row, 0, seed + 1)
        for x in range(T):
            for dy in range(height):
                y = top + dy
                if dy == height - 1:
                    c = ramp[0]
                elif dy == 0:
                    c = ramp[4] if hash01(x, y, seed + 2) < 0.85 else ramp[3]
                else:
                    grain = hash01(x // 3 + row * 7, y, seed + 3)
                    c = ramp[2] if grain < 0.18 else ramp[3]
                    if hash01(x, y, seed + 4) < 0.015:
                        c = ramp[1]
                put(img, x, y, tuple(min(255, int(v * tone)) for v in c))
        if hash01(row, variant, seed + 7) < joins:
            j = 2 + int(hash01(variant, row, seed + 8) * 12)
            for dy in range(height - 1):
                put(img, j, top + dy, ramp[1])
            put(img, j - 1, top + 1, IRON[0])
            put(img, j + 1, top + height - 2, IRON[0])
        if hash01(row, variant, seed + 5) < 0.3:
            kx = 3 + int(hash01(variant, row, seed + 6) * 9)
            put(img, kx, top + 2, ramp[1])
            put(img, kx + 1, top + 2, ramp[1])
            put(img, kx - 1, top + 2, ramp[2])
            put(img, kx + 2, top + 2, ramp[2])
    return img


def planks(variant):
    """The boardwalk and the jetties."""
    return boards(WOOD, 101, variant)


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
    """A lawn's edges: a kerb of pale stone round it, lit on its top, and the grass darker and
    tufted where it meets it. The north and west kerbs are lit and the south and east in shadow."""
    edges = []
    for side in range(4):
        lit = side in (0, 3)
        img = tile()
        for x in range(T):
            joint = x % 8 == 7
            put(img, x, 0, MORTAR[1] if joint else STONE[4] if lit else STONE[3])
            put(img, x, 1, MORTAR[1] if joint else STONE[2] if lit else STONE[1])
            put(img, x, 2, STONE[0] if lit else MORTAR[0])
            put(img, x, 3, GRASS[0] if hash01(x, side, 111) < 0.6 else GRASS[1])
            if hash01(x, side, 112) < 0.35:
                put(img, x, 4, GRASS[1])
        edges.append(turned(img, side))
    return edges


def water_edges():
    """Water's edges. Along the north it's the quay wall, seen from the front: cream coping lit on
    top, a course of blue-grey wet stone going darker, then the water lapping white against it. The
    other sides are a lip of coping with a line of light in the water beside it."""
    edges = []
    north = tile()
    for x in range(T):
        put(north, x, 0, STONE[4])
        put(north, x, 1, STONE[3] if x % 8 else MORTAR[1])
        put(north, x, 2, STONE[0])
        for row, tone in ((3, 3), (4, 2), (5, 1), (6, 0)):
            joint = (x + (4 if row >= 5 else 0)) % 8 == 7 or row == 4 and hash01(x, row, 121) < 0.15
            put(north, x, row, QUAY[0] if joint else QUAY[tone])
        put(north, x, 7, WATER[0])
        put(north, x, 8, WATER[5] if hash01(x, 0, 122) < 0.7 else WATER[4])
        if hash01(x, 1, 122) < 0.4:
            put(north, x, 9, WATER[4])
    edges.append(north)
    for side in (1, 2, 3):
        img = tile()
        lit = side == 3
        for x in range(T):
            put(img, x, 0, STONE[4] if lit else STONE[3])
            put(img, x, 1, STONE[2] if lit else STONE[1])
            put(img, x, 2, QUAY[1] if side != 2 else STONE[0])
            if hash01(x, side, 123) < 0.55:
                put(img, x, 3, WATER[4])
        edges.append(turned(img, side))
    return edges


def plank_edges():
    """A boardwalk's edges: along the south, the beam its boards are nailed to and the tops of the
    piles under it; along the sides, the ends of the boards; along the north, where it meets the
    quay or the water behind it, a dark seam."""
    north = tile()
    for x in range(T):
        put(north, x, 0, OUTLINE)
        put(north, x, 1, WOOD[1])
    south = tile()
    for x in range(T):
        put(south, x, 11, WOOD[4])
        put(south, x, 12, WOOD[3])
        put(south, x, 13, WOOD[2] if x % 8 not in (3, 4) else WOOD[3])
        put(south, x, 14, WOOD[1] if x % 8 not in (3, 4) else WOOD[2])
        put(south, x, 15, OUTLINE)
        if x % 8 == 5:
            put(south, x, 12, IRON[1])
    east, west = tile(), tile()
    for y in range(T):
        put(east, 15, y, OUTLINE)
        put(east, 14, y, WOOD[1])
        put(west, 0, y, OUTLINE)
        put(west, 1, y, WOOD[4])
    return [north, east, south, west]


# --------------------------------------------------------------------------- #
# The tileset, in TilesetCatalog's order
# --------------------------------------------------------------------------- #

def flat(frames_by_variant):
    """Every frame of every variant in one list, the order they go on the sheet."""
    out = []
    for frames in frames_by_variant:
        out += frames
    return out


#: Each tile: its name, its frames (every frame of every variant, in order), and its edges if it
#: has them. TilesetCatalog has the same list in the same order, with the same variants and frames.
TILES = (
    ("plaza", [plaza(v) for v in range(6)], None),
    ("terrace", [terrace(v) for v in range(2)], None),
    ("cobble", [cobble(v) for v in range(3)], None),
    ("grass", [grass(v) for v in range(4)], kerb_edges()),
    ("sand", [sand(v) for v in range(2)], None),
    ("water", flat(water_frames(v) for v in range(3)), water_edges()),
    ("planks", [planks(v) for v in range(4)], plank_edges()),
)


def build_sheet(tiles, path):
    """Lays every frame of a tileset out on a sheet eight tiles wide, in order, and writes it."""
    frames = []
    for _, cells, edges in tiles:
        frames += cells
        frames += edges or []
    columns = 8
    sheet = new_image(columns * T, (len(frames) + columns - 1) // columns * T)
    for i, f in enumerate(frames):
        for y in range(T):
            for x in range(T):
                if f[y][x][3]:
                    sheet[(i // columns) * T + y][(i % columns) * T + x] = list(f[y][x])
    write_png(path, sheet)


# --------------------------------------------------------------------------- #
# Drawing things that aren't tiles
# --------------------------------------------------------------------------- #

class Canvas:
    """A picture being drawn: put a pixel (or lay a see-through one over what's there), fill a
    box, draw a line, all clipped; and a dark line round everything solid at the end."""

    def __init__(self, w, h):
        self.w, self.h = w, h
        self.img = new_image(w, h)

    def put(self, x, y, c):
        x, y = int(x), int(y)
        if 0 <= x < self.w and 0 <= y < self.h:
            self.img[y][x] = [c[0], c[1], c[2], 255]

    def glaze(self, x, y, c, alpha):
        """Lays a see-through colour over the pixel: foam on the water, a lantern's glow."""
        x, y = int(x), int(y)
        if not (0 <= x < self.w and 0 <= y < self.h) or alpha <= 0:
            return
        under = self.img[y][x]
        if under[3] == 255:
            mixed = lerp_color(under[:3], c, alpha / 255)
            self.img[y][x] = [mixed[0], mixed[1], mixed[2], 255]
            return
        a = alpha / 255
        total = a + under[3] / 255 * (1 - a)
        mixed = [int(round((c[i] * a + under[i] * under[3] / 255 * (1 - a)) / total)) for i in range(3)]
        self.img[y][x] = mixed + [int(round(total * 255))]

    def box(self, x0, y0, x1, y1, c):
        for y in range(int(y0), int(y1) + 1):
            for x in range(int(x0), int(x1) + 1):
                self.put(x, y, c)

    def line(self, x0, y0, x1, y1, c, alpha=255):
        steps = int(max(abs(x1 - x0), abs(y1 - y0), 1))
        for i in range(steps + 1):
            x, y = round(x0 + (x1 - x0) * i / steps), round(y0 + (y1 - y0) * i / steps)
            if alpha == 255:
                self.put(x, y, c)
            else:
                self.glaze(x, y, c, alpha)

    def rope(self, x0, y0, x1, y1, sag, c):
        """A rope hanging between two points, sagging in the middle."""
        steps = int(max(abs(x1 - x0), abs(y1 - y0), 1)) * 2
        for i in range(steps + 1):
            t = i / steps
            self.put(round(x0 + (x1 - x0) * t), round(y0 + (y1 - y0) * t + sag * 4 * t * (1 - t)), c)

    def filled(self, x, y):
        return 0 <= x < self.w and 0 <= y < self.h and self.img[y][x][3] == 255

    def outline(self, c=OUTLINE):
        """A dark line round everything solid, on the outside."""
        edge = [(x, y) for y in range(self.h) for x in range(self.w)
                if not self.filled(x, y) and any(self.filled(x + dx, y + dy) for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)))]
        for x, y in edge:
            self.put(x, y, c)

    def glow(self, cx, cy, radius, c, strength):
        """A soft round glow, strongest in the middle: a lantern lit."""
        for y in range(int(cy - radius), int(cy + radius) + 1):
            for x in range(int(cx - radius), int(cx + radius) + 1):
                d = math.hypot(x - cx, y - cy) / radius
                if d < 1 and not self.filled(x, y):
                    self.glaze(x, y, c, int(strength * (1 - d) ** 1.5))


def lantern_at(c, x, y, lit=True):
    """A small ship's lantern hanging with its top at x, y: an iron cap, warm glass in a gold
    frame, and its glow round it."""
    c.put(x, y, IRON[1])
    c.box(x - 1, y + 1, x + 1, y + 1, GOLD[1])
    c.box(x - 2, y + 2, x + 2, y + 6, GOLD[0])
    c.box(x - 1, y + 2, x + 1, y + 5, GLASS[1] if lit else IRON[1])
    c.put(x - 1, y + 2, GLASS[2] if lit else IRON[2])
    c.put(x, y + 2, GLASS[2] if lit else IRON[2])
    c.put(x + 1, y + 5, GLASS[0] if lit else IRON[0])
    c.box(x - 1, y + 7, x + 1, y + 7, GOLD[1])
    if lit:
        c.glow(x, y + 4, 7, GLASS[1], 120)


# --------------------------------------------------------------------------- #
# The ship you come in on, seen side on from the harbour, bow to the left
# --------------------------------------------------------------------------- #
# The camera looks down at the harbour from the south, so the ship shows its deck from above, the
# near side of its hull from the front, and its mast and sail standing up off the deck. Each column
# of it works out where the deck's far edge, near edge and waterline are from the shape of the hull
# (its half-beam, narrowing to a point at the bow) and how high its deck is above the water there
# (higher at the bow and stern), so it comes out curved, the way a hull is.
#
# It's two props. The ship is everything behind you when you're on its deck. The near rail, with
# the gangplank that slides out of it, is a prop of its own that sorts at the deck's near edge, so
# it's drawn over your feet when you're on deck and under you on the plank and the jetty.

SHIP_W, SHIP_H = 272, 128
RAIL_H = 144
BOW, LENGTH = 36, 196
STERN = BOW + LENGTH
KEEL = 103
BEAM = 18
FREEBOARD = 24
CASTLE = BOW + int(LENGTH * 0.76)
RAISE = 14
MAST = BOW + LENGTH // 2
MAST_TOP = 6
YARD = 25
GANGWAY = 92
PLANK_FOOT = 141

SAIL = ((150, 132, 152), (198, 184, 188), (228, 216, 206), (243, 235, 221), (253, 249, 239))
DECK = ((132, 82, 58), (172, 110, 70), (210, 150, 96), (228, 174, 116), (242, 200, 144))
SHADE = (112, 88, 120)
ROPE = (82, 58, 56)
HEMP = (214, 180, 124)
FOAM = (240, 250, 255)
SHALLOW = (110, 200, 236)
DEEP = (6, 40, 92)


def along(x):
    """How far along the ship a column is, 0 at the bow to 1 at the stern."""
    return (x - BOW) / LENGTH


def half_beam(x):
    """How far the deck reaches either side of her middle at a column: to a point at the bow,
    full amidships, and drawing in a little at the stern."""
    u = along(x)
    if u <= 0:
        return 0.0
    if u < 0.34:
        return BEAM * (u / 0.34) ** 0.6
    if u > 0.86:
        return BEAM * (1 - 0.18 * ((min(u, 1) - 0.86) / 0.14) ** 2)
    return float(BEAM)


def deck_height(x):
    """How high her deck is over the water at a column: higher at the bow, and a little at the stern."""
    u = min(1.0, max(0.0, along(x)))
    return FREEBOARD + 8 * (max(0.0, 0.22 - u) / 0.22) ** 1.6 + 4 * max(0.0, u - 0.82) / 0.18


def near_edge(x):
    """Where the deck's near edge is on the picture at a column: out from her middle, and up by her deck's height."""
    return KEEL + half_beam(x) - deck_height(x)


def far_edge(x):
    """Where the deck's far edge is on the picture at a column."""
    return KEEL - half_beam(x) - deck_height(x)


def waterline(x):
    """Where the near side of the hull meets the water: rounding up into the stem at the bow, and
    up under the counter at the stern."""
    u = along(x)
    tip = near_edge(BOW)
    if u < 0.2:
        end = KEEL + 0.84 * half_beam(BOW + 0.2 * LENGTH)
        return tip + (end - tip) * (max(0.0, u) / 0.2) ** 1.1
    line = KEEL + 0.84 * half_beam(x)
    if u > 0.955:
        line -= (u - 0.955) / 0.045 * 9
    return line


def hull_top(x):
    """The top of what you see of the near side: the castle's rail aft, the deck's edge forward."""
    return near_edge(x) - (RAISE if x >= CASTLE else 0)


def deck_boards(c, x, y, centre, seed, shade=0.0):
    """One pixel of planking along the ship, the boards following the curve of the hull."""
    s = y - centre + 40
    board = int(s // 3)
    row = s - board * 3
    if row < 1:
        col = DECK[1]
    elif (x + board * 11) % 29 == 0:
        col = DECK[1]
    elif row < 2 and hash01(x // 4, board, seed) < 0.5:
        col = DECK[4]
    else:
        col = DECK[3] if hash01(x // 5, board, seed + 1) < 0.8 else DECK[2]
    if shade:
        col = lerp_color(col, SHADE, shade)
    c.put(x, y, col)


def draw_deck(c):
    """The main deck between the rails, and the inside of the far rail, lit along its cap."""
    for x in range(BOW + 1, CASTLE):
        far, near = far_edge(x), near_edge(x)
        centre = KEEL - deck_height(x)
        for y in range(int(math.ceil(far)), int(near) + 1):
            under_rail = y - far
            deck_boards(c, x, y, centre, 131, 0.5 if under_rail < 2 else 0.28 if under_rail < 4 else 0.0)
        top = int(math.ceil(far)) - 9
        for y in range(top, int(math.ceil(far))):
            d = y - top
            if d == 0:
                col = WOOD[5]
            elif d == 1:
                col = WOOD[3]
            elif d == 2:
                col = GOLD[2]
            elif d == 8:
                col = WOOD[0]
            else:
                col = WOOD[1] if (x - BOW) % 9 == 0 else WOOD[2] if (d - 3) % 3 else lerp_color(WOOD[2], WOOD[1], 0.6)
                col = lerp_color(col, SHADE, 0.15)
            c.put(x, y, col)


def draw_castle(c):
    """The stern castle: its deck a step up from the main deck, its rail round it, and the face of
    it towards the bow, lit from the left."""
    for x in range(CASTLE, STERN + 1):
        far, near = far_edge(x) - RAISE, near_edge(x) - RAISE
        centre = KEEL - deck_height(x) - RAISE
        for y in range(int(math.ceil(far)), int(near) + 1):
            deck_boards(c, x, y, centre, 137, 0.45 if y - far < 2 else 0.0)
        top = int(math.ceil(far)) - 6
        for y in range(top, int(math.ceil(far))):
            d = y - top
            if d < 3:
                col = (WOOD[5], WOOD[3], GOLD[2])[d]
            elif d == 5:
                col = WOOD[0]
            else:
                col = lerp_color(WOOD[2] if (x - CASTLE) % 6 else WOOD[1], SHADE, 0.15)
            c.put(x, y, col)
    # The front of the castle, towards the bow, lit from the left, with its door.
    front = CASTLE - 5
    for x in range(front, CASTLE):
        for y in range(int(far_edge(CASTLE)) - RAISE - 6, int(near_edge(x)) + 1):
            col = WOOD[4] if x == front else WOOD[3] if (y - int(far_edge(CASTLE))) % 4 != 3 else WOOD[2]
            c.put(x, y, col)
    door = int(KEEL - deck_height(CASTLE)) - 9
    c.box(front + 1, door, front + 3, door + 9, WOOD[1])
    c.box(front + 1, door, front + 3, door, GOLD[1])
    c.put(front + 3, door + 5, GOLD[3])
    # The wheel, on the castle's deck.
    wx, wy = CASTLE + 10, int(KEEL - deck_height(CASTLE + 10)) - RAISE - 4
    c.box(wx, wy, wx, wy + 6, WOOD[1])
    for y in range(-5, 6):
        for x in range(-5, 6):
            d = math.hypot(x, y)
            angle = math.atan2(y, x)
            if 3.5 <= d <= 4.6 or d < 5.6 and abs(math.sin(angle * 4)) < 0.2 or d < 1.5:
                c.put(wx + x, wy - 4 + y, GOLD[2] if d < 1.5 else WOOD[4] if y < 0 else WOOD[2])


def draw_hull(c):
    """The near side of the hull, from its top to the water: strakes of planking lit along their
    tops, a navy wale with gold either side of it, the castle's wall with its warm windows aft, and
    the wood darker and wetter towards the water."""
    windows = [CASTLE + 9, CASTLE + 21, CASTLE + 33]
    for x in range(BOW, STERN + 1):
        top, bottom = hull_top(x), waterline(x)
        deck = near_edge(x)
        u = along(x)
        light = 0.18 * max(0.0, 0.16 - u) / 0.16
        for y in range(int(math.ceil(top)), int(bottom) + 1):
            if x >= CASTLE and y < deck:
                d = y - top
                if d < 1:
                    col = WOOD[5]
                elif d < 2:
                    col = GOLD[2]
                elif d < 3:
                    col = WOOD[1]
                else:
                    col = WOOD[3] if (x - CASTLE) % 12 not in (0, 1) else WOOD[2]
                    if d > RAISE - 3:
                        col = GOLD[1] if d < RAISE - 1 else WOOD[1]
                c.put(x, y, col)
                continue
            d = y - deck
            if d < 0:
                c.put(x, y, WOOD[2])
                continue
            if 8 <= d < 12:
                col = (GOLD[2], NAVY[2], NAVY[1], GOLD[1])[int(d) - 8]
            else:
                strake = int(d) % 4
                col = (WOOD[4], WOOD[3], WOOD[2] if hash01(x // 6, int(d) // 4, 141) < 0.6 else WOOD[3], WOOD[1])[strake]
            if bottom - y < 3:
                col = lerp_color(col, (70, 52, 66), 0.45)
            elif bottom - y < 6:
                col = lerp_color(col, (70, 52, 66), 0.2)
            if light:
                col = lerp_color(col, (255, 236, 200), light)
            if u > 0.93:
                col = lerp_color(col, (60, 40, 54), (u - 0.93) / 0.07 * 0.35)
            c.put(x, y, col)
    for wx in windows:
        wy = int(near_edge(wx)) - RAISE + 4
        c.box(wx - 1, wy - 1, wx + 4, wy + 6, GOLD[1])
        c.box(wx, wy, wx + 3, wy + 5, GLASS[1])
        c.box(wx, wy, wx + 1, wy + 1, GLASS[2])
        c.box(wx + 2, wy + 4, wx + 3, wy + 5, GLASS[0])
        c.box(wx, wy + 2, wx + 3, wy + 2, GOLD[0])
        c.put(wx + 1, wy + 5, GOLD[0])
    for px in (BOW + 70, BOW + 96, BOW + 122):
        py = int(near_edge(px)) + 4
        c.box(px - 1, py - 1, px + 1, py + 1, GOLD[1])
        c.put(px, py, (40, 30, 44))
        c.put(px - 1, py - 1, GOLD[3])
    # The quarter: a gold scroll where the castle's wall turns round the stern.
    for y in range(int(hull_top(STERN)), int(near_edge(STERN)) + 6):
        c.put(STERN, y, GOLD[1] if y % 3 else GOLD[2])
        c.put(STERN - 1, y, GOLD[2] if y % 3 == 1 else WOOD[2])


def draw_balustrade(c):
    """The rail along the near side of the castle's deck: gilt-capped posts with the deck showing
    between them, under a cap rail."""
    for x in range(CASTLE - 5, STERN + 1):
        top = int(hull_top(x))
        c.put(x, top - 7, WOOD[5])
        c.put(x, top - 6, GOLD[2])
        if (x - CASTLE) % 4 == 0:
            c.box(x, top - 5, x, top - 1, WOOD[2])
            c.put(x, top - 5, GOLD[1])


def draw_fittings(c):
    """What's on the deck aft of the mast: the hatch, two barrels and a coil of rope."""
    hx, hy = MAST + 18, int(KEEL - deck_height(MAST + 18)) - 6
    c.box(hx - 1, hy - 1, hx + 16, hy + 11, WOOD[1])
    c.box(hx, hy, hx + 15, hy + 2, WOOD[4])
    c.box(hx, hy + 3, hx + 15, hy + 10, WOOD[2])
    for gy in range(hy + 4, hy + 10, 2):
        for gx in range(hx + 1, hx + 15, 2):
            c.put(gx, gy, (40, 28, 34))
    c.box(hx, hy + 10, hx + 15, hy + 10, WOOD[1])
    for bx, by in ((CASTLE - 14, int(far_edge(CASTLE - 14)) + 6), (CASTLE - 8, int(far_edge(CASTLE - 8)) + 9)):
        c.box(bx - 3, by - 7, bx + 3, by, WOOD[3])
        c.box(bx - 3, by - 7, bx - 2, by, WOOD[4])
        c.box(bx + 2, by - 7, bx + 3, by, WOOD[2])
        c.box(bx - 3, by - 6, bx + 3, by - 6, IRON[1])
        c.box(bx - 3, by - 2, bx + 3, by - 2, IRON[1])
        c.box(bx - 2, by - 8, bx + 2, by - 8, WOOD[2])
    rx, ry = MAST - 12, int(KEEL - deck_height(MAST)) + 4
    for y in range(-3, 4):
        for x in range(-5, 6):
            d = math.hypot(x / 5.5, y / 3.5)
            if d <= 1:
                c.put(rx + x, ry + y, (214, 186, 132) if int(d * 4) % 2 == 0 else (150, 120, 84))


def draw_mast(c, sail, phase):
    """The mast, its crow's nest and yard, and the sail: full (0), being hauled up (1), nearly up
    (2) or furled on the yard (3). The pennant at the top streams out behind while she's moving
    and hangs when she isn't."""
    foot = int(KEEL - deck_height(MAST))
    for y in range(MAST_TOP, foot + 1):
        for dx, col in ((-2, WOOD[4]), (-1, WOOD[3]), (0, WOOD[2]), (1, WOOD[1])):
            c.put(MAST + dx, y, col)
    for band in (foot - 14, YARD + 12, MAST_TOP + 4):
        c.box(MAST - 2, band, MAST + 1, band + 1, GOLD[2])
        c.put(MAST + 1, band + 1, GOLD[0])
    c.box(MAST - 1, MAST_TOP - 3, MAST, MAST_TOP - 1, GOLD[2])
    c.put(MAST - 1, MAST_TOP - 3, GOLD[3])
    # The crow's nest: a little barrel round the mast, lit on its left.
    for y in range(13, 20):
        for x in range(MAST - 6, MAST + 6):
            u = (x - (MAST - 6)) / 11
            col = WOOD[4] if u < 0.25 else WOOD[3] if u < 0.6 else WOOD[2]
            if y in (13, 19):
                col = GOLD[2] if y == 13 else WOOD[1]
            elif y == 16:
                col = IRON[1]
            c.put(x, y, col)
    # The yard, tapering at its ends.
    for x in range(MAST - 41, MAST + 42):
        reach = abs(x - MAST) / 41
        c.put(x, YARD, WOOD[4])
        c.put(x, YARD + 1, WOOD[2])
        if reach < 0.85:
            c.put(x, YARD + 2, WOOD[1])
    c.line(MAST, MAST_TOP + 2, MAST - 40, YARD, ROPE)
    c.line(MAST, MAST_TOP + 2, MAST + 40, YARD, ROPE)

    left, right = MAST - 38, MAST + 38
    if sail == 3:
        for x in range(left, right + 1):
            for y in range(YARD + 2, YARD + 7):
                d = y - (YARD + 2)
                bulge = 1 if (x - left) % 10 in (3, 4, 5, 6) else 0
                if d > 3 + bulge:
                    continue
                c.put(x, y, SAIL[4] if d == 0 else SAIL[3] if d == 1 else SAIL[2] if d < 3 + bulge else SAIL[1])
            if (x - left) % 10 == 0:
                c.box(x, YARD + 1, x, YARD + 6, ROPE)
    else:
        drop = (36, 20, 9)[sail]
        swag = (0, 5, 3)[sail]
        for x in range(left - 6, right + 5):
            for y in range(YARD + 2, YARD + 2 + drop + 4):
                t = (y - YARD - 2) / drop
                # Full, the sail's wider at its foot than at the yard, bellies out forward, and its
                # foot curves down in the middle; hauled up, its foot hangs in swags between the lines.
                spread = 4 * t if sail == 0 else 0
                lead = left - spread - (4 * math.sin(min(1.0, t) * math.pi) if sail == 0 else 0)
                tail = right + spread
                if not lead <= x <= tail:
                    continue
                v = (x - lead) / (tail - lead)
                foot = drop + (3 * math.sin(v * math.pi) if sail == 0 else -swag * abs(math.sin(v * math.pi * 3)))
                if y > YARD + 2 + foot:
                    continue
                fold = math.sin(v * math.pi * (6 if sail == 0 else 9) + t * 0.8)
                shade = 2.9 - 1.5 * v + 1.1 * fold
                if t < 0.1:
                    shade -= 1.0
                if sail == 0 and t > 0.9:
                    shade -= 0.5
                col = SAIL[max(0, min(4, int(round(shade))))]
                if y >= YARD + 1 + foot or x <= lead + 0.5 or x >= tail - 0.5:
                    col = SAIL[1]
                if sail == 0 and abs(t - 0.16) < 0.02 and (x - left) % 4 == 0:
                    col = SAIL[0]
                c.put(x, y, col)
        if sail == 0:
            gx, gy = MAST - 1, YARD + 22
            for dy in range(-6, 7):
                for dx in range(-7, 8):
                    d = math.hypot(dx, dy * 1.15)
                    if d <= 6.5:
                        c.put(gx + dx, gy + dy, GOLD[1] if d > 5.6 else NAVY[1] if d > 4.6 or dy > 2 else NAVY[2])
            for dx, dy in ((-5, -1), (-4, -2), (-3, -2), (-2, -1), (-1, 0), (0, 1), (1, 0), (2, -1), (3, -2), (4, -2),
                           (5, -1), (0, 2), (-1, 1), (1, 1)):
                c.put(gx + dx, gy + dy, GOLD[3] if dy < 0 else GOLD[2])
        else:
            for x in range(left, right + 1, 13):
                c.line(x, YARD + 1, x, YARD + 2 + drop - swag, ROPE)

    # The pennant, from the masthead.
    length = 22 if phase is not None else 12
    for i in range(length):
        t = i / length
        if phase is None:
            px, py = MAST + 1 + i * 0.7, MAST_TOP + 1 + i * 0.8
        else:
            px, py = MAST + 1 + i, MAST_TOP + 1 + 1.4 * math.sin(i * 0.45 + phase * math.pi)
        width = max(1, int(round(3 * (1 - t))))
        for w in range(width):
            c.put(px, py + w, GOLD[2] if i < 3 else NAVY[2] if w == 0 else NAVY[1])


def draw_shrouds(c):
    """The shrouds, from under the crow's nest down to the far rail, behind the sail."""
    for dx in (-24, 22):
        c.line(MAST + (1 if dx > 0 else -2), 20, MAST + dx, int(far_edge(MAST + dx)) - 9, ROPE)


def draw_rigging(c):
    """The stays: thin and not much of them. The forestay out to the bowsprit and the backstay
    down to the stern."""
    tip = (BOW - 26, int(near_edge(BOW)) - 15)
    c.line(MAST, MAST_TOP + 1, tip[0] + 1, tip[1], ROPE)
    c.line(MAST, MAST_TOP + 1, STERN - 4, int(far_edge(STERN - 4)) - RAISE - 6, ROPE)


def draw_bow(c):
    """The bowsprit out over the water with its stay down to the stem, and the figurehead under
    it: a gilded gull, wings back."""
    root = (BOW + 8, int(near_edge(BOW + 8)) - 3)
    tip = (BOW - 26, int(near_edge(BOW)) - 15)
    c.line(root[0], root[1], tip[0], tip[1], WOOD[3])
    c.line(root[0], root[1] + 1, tip[0], tip[1] + 1, WOOD[2])
    c.line(root[0], root[1] + 2, tip[0] + 2, tip[1] + 2, WOOD[1])
    c.box(tip[0] - 1, tip[1] - 1, tip[0], tip[1] + 1, GOLD[2])
    c.line(tip[0] + 1, tip[1] + 2, BOW + 3, int(waterline(BOW + 3)) - 3, ROPE)
    fx, fy = BOW + 1, int(near_edge(BOW)) + 5
    gull = ("..33....",
            ".3322...",
            "33222.3.",
            "..22223.",
            "..122332",
            "...1222.",
            "...0112.",
            "....01..")
    for dy, row in enumerate(gull):
        for dx, ch in enumerate(row):
            if ch != ".":
                c.put(fx - 4 + dx, fy - 3 + dy, GOLD[int(ch)])


def draw_lanterns(c):
    """The castle's lanterns: one on a post at each aft corner, and the big one on the stern."""
    for x, base in ((STERN - 4, int(near_edge(STERN - 4)) - RAISE - 1), (STERN - 6, int(far_edge(STERN - 6)) - RAISE - 6)):
        c.box(x, base - 7, x, base, WOOD[1])
        lantern_at(c, x, base - 15)
    c.box(STERN, int(hull_top(STERN)) - 3, STERN + 4, int(hull_top(STERN)) - 3, IRON[1])
    lantern_at(c, STERN + 4, int(hull_top(STERN)) - 2)


def draw_water(c, wake, phase, moored):
    """What the ship does to the water: its shadow, the white along its waterline, the bow wave
    and the wake spreading behind it while it's moving (wake 2 is under way, 1 slowing, 0 still)."""
    for x in range(BOW + 2, STERN):
        y = int(waterline(x)) + 1
        c.glaze(x, y, DEEP, 120)
        c.glaze(x, y + 1, DEEP, 70)
        if (x // 3 + phase) % 2 or wake:
            c.glaze(x, y, FOAM, 170 if wake else 110)
        if wake and hash01(x, phase, 151) < 0.25 * wake:
            c.glaze(x, y + 2, FOAM, 140)
    if wake:
        bx = BOW + 10
        for i in range(10 + 8 * wake):
            t = i / (10 + 8 * wake)
            x = bx + 3 - t * 6 + i * 1.2
            for k in range(1 + wake):
                y = int(waterline(int(x))) - 2 + k + int(t * 3)
                c.glaze(x - 4 + k, y, FOAM, 230 - i * 7)
        for i in range(6 * wake):
            sx = bx - 4 + int(hash01(i, phase, 152) * 10)
            sy = int(waterline(bx)) - 6 + int(hash01(phase, i, 153) * 6)
            c.glaze(sx, sy, FOAM, 200)
        reach = 18 + 20 * wake
        for i in range(reach):
            t = i / reach
            x = STERN - 6 + i
            fade = int((1 - t) * (150 + 50 * wake))
            near = int(waterline(STERN - 6)) + 1 + t * (4 + 2 * wake) + math.sin(i * 0.6 + phase * math.pi) * 0.8
            far = KEEL - 9 - t * (5 + 2 * wake) + math.sin(i * 0.5 + phase * math.pi + 1) * 0.8
            c.glaze(x, near, FOAM, fade)
            c.glaze(x, far, FOAM, fade * 2 // 3)
            for y in range(int(far) + 1, int(near)):
                if hash01(x, y, 154 + phase) < 0.3 * (1 - t):
                    c.glaze(x, y, FOAM, fade)
                else:
                    c.glaze(x, y, SHALLOW, int(70 * (1 - t)))
    elif moored:
        for i in range(0, LENGTH, 7):
            x = BOW + 6 + i
            c.glaze(x, int(waterline(x)) + 3, SHALLOW, 110)
            c.glaze(x + 1, int(waterline(x)) + 3, SHALLOW, 110)


def draw_moorings(c):
    """The lines ashore, from the bow and the stern down to the bollards on the jetty."""
    c.rope(BOW + 14, int(near_edge(BOW + 14)) + 1, BOW + 4, 121, 5, ROPE)
    c.rope(STERN - 10, int(near_edge(STERN - 10)) + 1, STERN + 8, 121, 4, ROPE)


#: The ship's frames, in the order they're on its sheet: (sail, wake, phase, moored). The first is
#: her tied up, which is how she's left whenever you come back.
SHIP_FRAMES = (
    (3, 0, 0, True),
    (0, 2, 0, False),
    (0, 2, 1, False),
    (1, 1, 0, False),
    (1, 1, 1, False),
    (2, 1, 0, False),
    (3, 0, 1, False),
)


def ship(sail, wake, phase, moored):
    """One picture of the ship, back to front: her deck and castle, her side, the mast and sail,
    the stays and the bow, a dark line round it all, then her lanterns and what she does to the water."""
    c = Canvas(SHIP_W, SHIP_H)
    draw_deck(c)
    draw_castle(c)
    draw_fittings(c)
    draw_hull(c)
    draw_balustrade(c)
    draw_shrouds(c)
    draw_mast(c, sail, None if moored or (sail == 3 and not wake) else phase)
    draw_rigging(c)
    draw_bow(c)
    c.outline()
    draw_lanterns(c)
    draw_water(c, wake, phase, moored)
    if moored:
        draw_moorings(c)
    return c.img


def near_rail(gate, plank):
    """The ship's near rail with the gangplank: the rail's cap, gold, a navy band and gold again,
    with the gangway in it, shut by its gate (gate) or open; and the plank, not out (0), sliding out
    level (a length in pixels), or down on the pier (None), with its posts and pale hand-ropes."""
    c = Canvas(SHIP_W, RAIL_H)
    for x in range(BOW + 1, CASTLE):
        if not gate and abs(x - GANGWAY) <= 6:
            continue
        edge = int(near_edge(x))
        for d in range(8):
            y = edge - 8 + d
            col = (WOOD[5], WOOD[3], GOLD[2], NAVY[3], NAVY[2], NAVY[1], GOLD[1], WOOD[2])[d]
            if gate and abs(x - GANGWAY) <= 6:
                col = (WOOD[4], WOOD[3], WOOD[3], WOOD[2], WOOD[3], WOOD[2], WOOD[1], WOOD[1])[d]
                if (x - GANGWAY) % 4 == 0:
                    col = WOOD[1]
            elif (x - BOW) % 14 == 7 and 3 <= d <= 5:
                col = GOLD[2] if d == 4 else GOLD[1]
            c.put(x, y, col)
    if plank == 0:
        c.outline()
        return c.img

    top = int(near_edge(GANGWAY)) + 1
    bottom = PLANK_FOOT if plank is None else top + plank
    for y in range(top, bottom + 1):
        for x in range(GANGWAY - 5, GANGWAY + 5):
            if x in (GANGWAY - 5, GANGWAY + 4):
                col = WOOD[1]
            elif (y - top) % 4 == 3:
                col = WOOD[2]
            else:
                col = WOOD[4] if x < GANGWAY - 1 else WOOD[3]
            c.put(x, y, col)
    for x in (GANGWAY - 6, GANGWAY + 5):
        for py in (top + 1, bottom - 2):
            c.box(x, py - 6, x, py, WOOD[1])
            c.put(x, py - 7, GOLD[2])
    c.outline()
    for x in (GANGWAY - 7, GANGWAY + 6):
        c.line(x, top - 4, x, bottom - 7, HEMP)
    if plank is None:
        for x in range(GANGWAY - 6, GANGWAY + 6):
            c.glaze(x, PLANK_FOOT + 2, (40, 24, 30), 90)
    return c.img


#: The near rail's frames, in the order they're on its sheet. The first is the plank down on the
#: jetty, how it's left; then the gate shut while she sails in, the gate open, and the plank
#: sliding out, before it comes down.
RAIL_FRAMES = ((False, None), (True, 0), (False, 0), (False, 12), (False, 24), (False, 36))


def rowboat():
    """A rowing boat moored by the jetty, the same way on as the ship: the inside of it seen from
    above, ribbed, with two thwarts across it and an oar lying along it, a gilt-capped rim all round,
    the near side of its hull with a navy stripe under the rim, and the water lapping round it."""
    w, h = 50, 26
    c = Canvas(w, h)
    bow, length, keel = 3, 44, 16

    def beam(x):
        u = min(1.0, max(0.0, (x - bow) / length))
        return 7.5 * math.sin(math.pi * u ** 0.85) ** 0.55

    def rise(x):
        u = (x - bow) / length
        return 6 + 2.5 * max(0.0, 0.2 - u) / 0.2 + 1.5 * max(0.0, u - 0.85) / 0.15

    for x in range(bow, bow + length + 1):
        half = beam(x)
        if half < 0.5:
            continue
        far, near = keel - half - rise(x), keel + half - rise(x)
        for y in range(int(math.ceil(far)), int(near) + 1):
            d, depth = y - far, near - far
            if d < 1:
                col = WOOD[5]
            elif d < 3:
                col = lerp_color(WOOD[3], SHADE, 0.2)
            elif depth - d < 1:
                col = WOOD[4]
            else:
                col = WOOD[2] if (x - bow) % 4 == 0 else WOOD[1]
            c.put(x, y, col)
        bottom = keel + 0.75 * half
        for y in range(int(near) + 1, int(bottom) + 1):
            d = y - int(near) - 1
            col = (NAVY[2], NAVY[1], WOOD[3], WOOD[2], WOOD[3], WOOD[2], WOOD[1])[min(6, d)]
            if bottom - y < 1.5:
                col = lerp_color(col, (70, 52, 66), 0.4)
            c.put(x, y, col)
    for tx in (bow + 16, bow + 29):
        top, bot = int(keel - beam(tx) - rise(tx)) + 2, int(keel + beam(tx) - rise(tx)) - 1
        c.box(tx, top, tx + 1, bot, WOOD[3])
        c.box(tx, top, tx, bot, WOOD[4])
    y0 = int(keel - rise(bow + 22)) - 1
    c.line(bow + 9, y0 + 1, bow + 38, y0 - 1, WOOD[4])
    c.box(bow + 36, y0 - 2, bow + 40, y0 - 1, WOOD[3])
    c.put(bow + 40, y0 - 2, WOOD[4])
    c.outline()
    for x in range(bow + 2, bow + length):
        if beam(x) < 1:
            continue
        y = int(keel + 0.75 * beam(x)) + 2
        c.glaze(x, y, DEEP, 110)
        c.glaze(x, y, FOAM, 130 if (x // 3) % 2 else 60)
        c.glaze(x, y + 1, DEEP, 50)
    return c.img

def side_by_side(frames):
    w, h = len(frames[0][0]), len(frames[0])
    sheet = new_image(w * len(frames), h)
    for i, f in enumerate(frames):
        for y in range(h):
            sheet[y][i * w:(i + 1) * w] = f[y]
    return sheet


# --------------------------------------------------------------------------- #
# The Gull's Wager: the deck you sail in on, at night
# --------------------------------------------------------------------------- #
# Drawn in daylight colours like everything else; the map's dark and its lanterns make it night.
# The ship lies along the map with her bow to the west, nine tiles from the top of her north rail to
# her waterline: the north rail (its cap, and the inside of it facing you), six rows of deck, the
# south rail's cap with the outside of her under it, and her side down to the water. Her bow and
# stern are blocks of nine tiles each way on the detail layer, painted whole out of the same tiles,
# so her rails curve round to a point at the bow, with the figurehead under the bowsprit, and round
# at the stern. The ground under them is deck where you can walk, and rail or sea where you can't.

NIGHT = ((8, 36, 80), (14, 58, 112), (22, 82, 142), (34, 110, 170), (78, 160, 208), (170, 222, 242), (250, 253, 255))
SHIP_BLOCK = 9
SHIP_MIDDLE = T * 4
SHIP_HALF = T * 3
STEM = (10, 54)
BOWSPRIT_TIP = (-44, 12)


def night_sea(variant):
    """The open sea at night: the harbour's water a shade darker and bluer, with more glints, the
    moon catching the ripples."""
    return water_frames(variant, NIGHT, seed=161, ripples=4, glints=2)


def ship_deck(variant):
    """The deck: boards running along the ship, four to a tile."""
    return boards(DECK, 181, variant, rows=((0, 4), (4, 4), (8, 4), (12, 4)), joins=0.45)


def rail_north(variant):
    """The north rail: its cap, lit, with a gold line under it, then the inside of it facing you,
    planks between the stanchions, going into shadow at its foot."""
    img = tile()
    for y in range(T):
        for x in range(T):
            if y < 4:
                col = (WOOD[5], WOOD[4], WOOD[3], GOLD[2])[y]
            elif y == 15:
                col = WOOD[0]
            elif (x + variant * 4) % 8 == 0:
                col = GOLD[1] if y == 4 else WOOD[1]
            else:
                col = lerp_color(WOOD[2] if (y - 4) % 4 else WOOD[1], SHADE, 0.12 + 0.025 * y)
            put(img, x, y, col)
    return img


def rail_south(variant):
    """The south rail: its cap, then the outside of the ship under it, a navy band between gold
    lines and the planking going down."""
    img = tile()
    for y in range(T):
        for x in range(T):
            if y == 0:
                col = OUTLINE
            elif y < 4:
                col = (WOOD[5], WOOD[4], WOOD[3])[y - 1]
            elif y == 4:
                col = WOOD[1]
            elif y in (5, 9):
                col = GOLD[2] if y == 5 else GOLD[1]
            elif y < 9:
                col = (NAVY[3], NAVY[2], NAVY[1])[y - 6]
            elif (x + variant * 7) % 23 == 0:
                col = WOOD[1]
            else:
                col = (WOOD[4], WOOD[3], WOOD[2], WOOD[1])[(y - 10) % 4]
            put(img, x, y, col)
    return img


def hull_side(variant):
    """The ship's side down to the water: strakes, darker and wetter as they go down, and the sea
    breaking white along the bottom of it."""
    img = tile()
    for y in range(T):
        for x in range(T):
            if y < 13:
                col = (WOOD[4], WOOD[3], WOOD[2] if hash01(x // 6, y // 4, 191 + variant) < 0.6 else WOOD[3], WOOD[1])[(y + 2) % 4]
                if y > 9:
                    col = lerp_color(col, (60, 46, 62), 0.2 * (y - 9))
            elif y == 13:
                col = NIGHT[0]
            else:
                col = NIGHT[5] if hash01(x, y + variant, 192) < (0.65 if y == 14 else 0.3) else NIGHT[1]
            put(img, x, y, col)
    return img


def hatch():
    """The hatch over the hold, two tiles each way: a raised coaming lit along its top and left,
    and an iron-banded grating with the dark below showing through."""
    size = T * 2
    img = new_image(size, size)
    for y in range(size):
        for x in range(size):
            if x < 2 or y < 2 or x > size - 3 or y > size - 5:
                col = WOOD[4] if y < 2 or x < 2 else WOOD[3] if size - 4 <= y < size - 2 else WOOD[2] if y < size - 2 else WOOD[1]
            elif x in (2, size - 3) or y in (2, size - 5):
                col = WOOD[1]
            elif (x - 3) % 4 == 3 or (y - 3) % 4 == 3:
                col = (24, 16, 26)
            else:
                col = DECK[3] if (x + y) % 5 else DECK[4]
            if (4 <= y <= 5 or size - 8 <= y <= size - 7) and 3 < x < size - 4:
                col = IRON[1] if x % 4 else IRON[2]
            img[y][x] = [col[0], col[1], col[2], 255]
    return cells_of(img, 2)


def cells_of(img, block):
    """A picture a block of tiles big, cut into its tiles, a row at a time, the way a block's on the sheet."""
    return [[img[cy * T + y][cx * T:(cx + 1) * T] for y in range(T)] for cy in range(block) for cx in range(block)]


def end_profile(x, bow):
    """How far the deck reaches either side of the ship's middle, at a column of the bow's or the
    stern's block: to a point at the bow, and round at the stern. Less than nothing past her end."""
    if bow:
        if x < STEM[0]:
            return -1.0
        return SHIP_HALF * min(1.0, (x - STEM[0]) / (0.8 * (T * SHIP_BLOCK - STEM[0]))) ** 0.55
    end = T * SHIP_BLOCK - 14
    if x > end:
        return -1.0
    round_from = end - 40
    if x < round_from:
        return float(SHIP_HALF)
    return SHIP_HALF * math.sqrt(max(0.0, 1 - ((x - round_from) / 40) ** 2)) ** 0.6


def spar(c, ox):
    """The bowsprit, from the stem out to its tip, in whichever block it's being drawn into (ox is
    where that block starts, from the bow's): the spar lit along its top, and the jib furled on it."""
    (x0, y0), (x1, y1) = STEM, BOWSPRIT_TIP
    x0, x1 = x0 - ox, x1 - ox
    c.line(x0, y0, x1, y1, WOOD[4])
    c.line(x0, y0 + 1, x1, y1 + 1, WOOD[2])
    c.line(x0, y0 + 2, x1 + 1, y1 + 2, WOOD[1])
    for i in range(18, 46):
        t = i / 54
        x, y = round(x0 + (x1 - x0) * t), round(y0 + (y1 - y0) * t)
        c.put(x, y - 1, SAIL[3])
        c.put(x, y - 2, SAIL[2] if i % 6 else ROPE)
    c.box(x1 - 1, y1 - 1, x1 + 1, y1 + 1, GOLD[2])


def ship_end(bow):
    """The bow or the stern, painted whole out of the same tiles as the rest of her: the north rail
    curving round to meet the south one, the deck between them, and her side below; at the bow, the
    stem, the root of the bowsprit and the figurehead, and at the stern, the lanterns on her quarters."""
    size = T * SHIP_BLOCK
    c = Canvas(size, size)
    north_rail, south_rail, side, deck = rail_north(0), rail_south(0), hull_side(0), ship_deck(0)
    for x in range(size):
        half = end_profile(x, bow)
        if half < 0:
            continue
        north, south = int(SHIP_MIDDLE - half), int(SHIP_MIDDLE + half)
        for y in range(north - T, north):
            c.put(x, y, north_rail[y - north + T][x % T][:3])
        for y in range(north, south):
            c.put(x, y, deck[y % T][x % T][:3])
        for y in range(south, south + T):
            c.put(x, y, south_rail[y - south][x % T][:3])
        for y in range(south + T, south + T * 2):
            c.put(x, y, side[y - south - T][x % T][:3])
    if bow:
        for y in range(STEM[1] - 6, SHIP_MIDDLE + T * 2):
            c.put(STEM[0] - 1, y, WOOD[3])
            c.put(STEM[0], y, WOOD[2])
        spar(c, 0)
        gull = ("..33....",
                ".3322...",
                "33222.3.",
                "..22223.",
                "..122332",
                "...1222.",
                "...0112.",
                "....01..")
        for dy, row in enumerate(gull):
            for dx, ch in enumerate(row):
                if ch != ".":
                    c.put(STEM[0] - 6 + dx, STEM[1] + 6 + dy, GOLD[int(ch)])
    c.outline()
    if not bow:
        # The stern lantern, over her middle, and one on the post at her south quarter.
        for lx, ly in ((size - 15, SHIP_MIDDLE - 16), (size - 24, int(SHIP_MIDDLE + end_profile(size - 24, False)) - 12)):
            c.box(lx, ly + 7, lx, ly + 12, WOOD[1])
            lantern_at(c, lx, ly)
    return c.img


def bowsprit():
    """The bowsprit past the stem, out over the sea, three tiles each way: the rest of the spar the
    bow's block starts."""
    c = Canvas(T * 3, T * 3)
    spar(c, -T * 3)
    c.outline()
    return cells_of(c.img, 3)


def end_cells(bow):
    """The bow or the stern, cut into its tiles."""
    return cells_of(ship_end(bow), SHIP_BLOCK)


def ship_end_ground(bow):
    """What the ground under the bow or the stern has to be, a row of map characters per row of
    the block: deck (-) where every pixel of a cell is deck you could stand on, rail (#) where the
    picture covers it all, and sea (~) where any of the sea shows. The script prints them with
    --ship-ends, for copying into tools/rooms/legacy/gulls-wager.map when the bow or the stern changes shape."""
    img = ship_end(bow)
    rows = []
    for cy in range(SHIP_BLOCK):
        row = ""
        for cx in range(SHIP_BLOCK):
            xs, ys = range(cx * T, cx * T + T), range(cy * T, cy * T + T)
            covered = all(img[y][x][3] == 255 for y in ys for x in xs)
            standing = all(end_profile(x, bow) > 0 and SHIP_MIDDLE - end_profile(x, bow) <= cy * T
                           and cy * T + T <= SHIP_MIDDLE + end_profile(x, bow) for x in xs)
            row += "-" if covered and standing else "#" if covered else "~"
        rows.append(row)
    return rows


LANTERN_FRAMES = 4


def rail_lantern():
    """A lantern hung from a bracket on the inside of the north rail, swinging a little with the
    ship: four frames, out to one side, back, out to the other, back."""
    frames = []
    for f in range(LANTERN_FRAMES):
        lean = (0, 1, 0, -1)[f]
        c = Canvas(T, T)
        c.box(6, 1, 9, 1, IRON[1])
        c.put(8, 2, IRON[1])
        lantern_at(c, 8 + lean, 3)
        frames.append(c.img)
    return frames


SHIP_TILES = (
    ("sea", flat(night_sea(v) for v in range(3)), None),
    ("deck", [ship_deck(v) for v in range(3)], None),
    ("rail-n", [rail_north(v) for v in range(2)], None),
    ("rail-s", [rail_south(v) for v in range(2)], None),
    ("hull", [hull_side(v) for v in range(2)], None),
    ("hatch", hatch(), None),
    ("bow", end_cells(True), None),
    ("stern", end_cells(False), None),
    ("bowsprit", bowsprit(), None),
    ("lantern", rail_lantern(), None),
)


def mast():
    """The mainmast from the deck: its foot in a collar of rope, the pole going up past the yard
    with the sail furled on it for the night (high enough to be over the rail and the sea, and not
    over anyone's head), the topsail furled above that, and the crow's nest near the top, with the
    bell the lookout rings under it. No shrouds: they'd go down to the north rail, behind anyone
    standing there, and the mast's drawn in front of them."""
    w, h = 112, 200
    c = Canvas(w, h)
    mx, foot = w // 2, h - 4
    for y in range(18, foot + 1):
        for dx, col in ((-3, WOOD[4]), (-2, WOOD[3]), (-1, WOOD[3]), (0, WOOD[2]), (1, WOOD[2]), (2, WOOD[1])):
            c.put(mx + dx, y, col)
    for band in (foot - 30, foot - 70, 64, 40):
        c.box(mx - 3, band, mx + 2, band + 1, GOLD[2])
        c.put(mx + 2, band + 1, GOLD[0])
    for y in range(foot - 6, foot + 1):
        for x in range(mx - 8, mx + 8):
            if ((x - mx + 0.5) / 8) ** 2 + ((y - foot + 3) / 3.5) ** 2 <= 1:
                c.put(x, y, (214, 186, 132) if (x + y) % 3 else (150, 120, 84))
    yard = 84
    for x in range(mx - 50, mx + 50):
        c.put(x, yard, WOOD[4])
        c.put(x, yard + 1, WOOD[2])
        if abs(x - mx) < 44:
            for y in range(yard + 2, yard + 8):
                d = y - yard - 2
                bulge = 1 if (x - mx) % 10 in (2, 3, 4, 5, 6) else 0
                if d <= 4 + bulge:
                    c.put(x, y, SAIL[4] if d == 0 else SAIL[3] if d < 2 else SAIL[2] if d < 4 + bulge else SAIL[1])
            if (x - mx) % 10 == 0:
                c.box(x, yard + 1, x, yard + 7, ROPE)
    c.line(mx, 22, mx - 48, yard, ROPE)
    c.line(mx, 22, mx + 48, yard, ROPE)
    top = 50
    for x in range(mx - 30, mx + 30):
        c.put(x, top, WOOD[4])
        c.put(x, top + 1, WOOD[2])
        if abs(x - mx) < 26:
            c.put(x, top + 2, SAIL[3])
            c.put(x, top + 3, SAIL[2] if (x - mx) % 8 else ROPE)
            c.put(x, top + 4, SAIL[1])
    for y in range(18, 32):
        for x in range(mx - 10, mx + 10):
            u = (x - (mx - 10)) / 19
            col = WOOD[4] if u < 0.2 else WOOD[3] if u < 0.55 else WOOD[2] if u < 0.85 else WOOD[1]
            if y == 18:
                col = GOLD[2]
            elif y == 19:
                col = WOOD[5]
            elif y in (23, 28):
                col = IRON[1]
            elif y == 31:
                col = WOOD[1]
            c.put(x, y, col)
    c.box(mx - 1, 32, mx, 33, IRON[1])
    for y in range(34, 39):
        half = 1 + (y - 34) // 2
        c.box(mx - half, y, mx + half - 1, y, GOLD[2] if y < 37 else GOLD[1])
    c.put(mx - 1, 34, GOLD[3])
    c.box(mx - 2, 10, mx + 1, 17, WOOD[2])
    c.box(mx - 1, 5, mx, 9, GOLD[2])
    c.put(mx - 1, 5, GOLD[3])
    for i in range(16):
        for k in range(max(1, 3 - i // 6)):
            c.put(mx + 2 + i, 11 + int(math.sin(i * 0.5) * 1.2) + k, GOLD[2] if i < 2 else NAVY[2] if k == 0 else NAVY[1])
    c.outline()
    return c.img


def lantern_post():
    """A ship's lantern on a post at the rail, hung from an iron crook. Its glass is left open,
    because the flame the map draws for its light goes behind it and shows through."""
    c = Canvas(18, 46)
    c.box(3, 2, 4, 45, WOOD[2])
    c.box(3, 2, 3, 45, WOOD[3])
    c.box(2, 42, 5, 45, WOOD[1])
    c.box(3, 1, 13, 2, IRON[1])
    c.put(12, 3, IRON[1])
    c.box(9, 4, 15, 5, GOLD[2])
    c.box(8, 6, 16, 7, GOLD[1])
    for y in range(8, 22):
        c.put(8, y, GOLD[1])
        c.put(16, y, GOLD[0])
    c.box(8, 22, 16, 23, GOLD[1])
    c.box(9, 24, 15, 24, GOLD[0])
    c.put(9, 6, GOLD[3])
    c.outline()
    return c.img


def wheel():
    """The ship's wheel on its post: a ring of turned spokes with handles past the rim, gilt at the hub."""
    c = Canvas(30, 36)
    c.box(13, 18, 16, 35, WOOD[2])
    c.box(13, 18, 13, 35, WOOD[3])
    c.box(16, 18, 16, 35, WOOD[1])
    c.box(11, 33, 18, 35, WOOD[1])
    cx, cy = 14.5, 14
    for y in range(30):
        for x in range(30):
            d = math.hypot(x - cx, y - cy)
            angle = math.atan2(y - cy, x - cx)
            if d < 2.6:
                c.put(x, y, GOLD[3] if x < cx and y < cy else GOLD[2])
            elif 8.6 < d < 11.2:
                c.put(x, y, WOOD[4] if y < cy - 3 else WOOD[3] if y < cy + 4 else WOOD[2])
            elif d < 14 and abs(math.sin(angle * 4)) < 0.16:
                c.put(x, y, WOOD[3] if d < 12 else WOOD[2])
    c.outline()
    return c.img


def rope_coil():
    """A coil of rope on the deck, flat, in rings, lit on its top left."""
    c = Canvas(20, 12)
    for y in range(12):
        for x in range(20):
            d = math.hypot((x - 9.5) / 9.5, (y - 5.5) / 5.5)
            if 0.18 < d <= 1.0:
                ring = int(d * 5)
                lit = (x - 9.5) + (y - 5.5) * 1.6 < 0
                c.put(x, y, (226, 198, 142) if ring % 2 == 0 and lit else (196, 164, 112) if ring % 2 == 0 else (142, 112, 78))
    c.outline()
    return c.img


if __name__ == "__main__":
    if "--ship-ends" in sys.argv:
        for name, bow in (("bow", True), ("stern", False)):
            print("The ground under the " + name + ":")
            print("\n".join(ship_end_ground(bow)))
        sys.exit()
    print("Generating Questeria's ground and its harbour for Legend Quest...")
    build_sheet(TILES, os.path.join(OUT, "Tiles", "questeria.png"))
    write_png(os.path.join(OUT, "Props", "ship.png"), side_by_side([ship(*frame) for frame in SHIP_FRAMES]))
    write_png(os.path.join(OUT, "Props", "gangplank.png"), side_by_side([near_rail(*frame) for frame in RAIL_FRAMES]))
    write_png(os.path.join(OUT, "Props", "rowboat.png"), rowboat())
    build_sheet(SHIP_TILES, os.path.join(OUT, "Tiles", "ship.png"))
    write_png(os.path.join(OUT, "Props", "mast.png"), mast())
    write_png(os.path.join(OUT, "Props", "lantern-hanging.png"), lantern_post())
    write_png(os.path.join(OUT, "Props", "ship-wheel.png"), wheel())
    write_png(os.path.join(OUT, "Props", "rope-coil.png"), rope_coil())
    print("Done.")
