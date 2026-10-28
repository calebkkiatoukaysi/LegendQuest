"""
World art for Legend Quest: the tilesets, the props (the ones you can open have a picture for each
way they can be), the torch flame, and the two soft textures light and sparkles are drawn with.
Questeria and the title are cut from the pictures I made instead, by cut_supplied.py.

    python tools/generate_world.py

Only needs CPython (the PNG writer is in pixels.py), and every random number is seeded, so a rerun
writes the same bytes.

Everything on a map is drawn as if a torch were right on it, lit from the top left. The game does
the dark itself, the same as The Black Box's lobby, so anything that walks out of the light goes
dark along with the floor.
"""

import math
import os
import random
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from pixels import OUT, clamp_byte, hash01, lerp_color, new_image, write_png  # noqa: E402


# --------------------------------------------------------------------------- #
# glow.png and mote.png -- the soft white shapes light and sparkles are drawn with
# --------------------------------------------------------------------------- #
# White, with the shape in the alpha. The pipeline premultiplies them and the game colours them,
# so one texture does every colour of light.

def build_glow(size=64):
    """A round light that falls off to nothing at the edge (squared, so the middle is a soft hot spot)."""
    img = new_image(size, size)
    half = size / 2.0
    for y in range(size):
        for x in range(size):
            d = math.hypot(x + 0.5 - half, y + 0.5 - half) / half
            img[y][x] = [255, 255, 255, clamp_byte(255 * max(0.0, 1.0 - d) ** 2)]
    write_png(os.path.join(OUT, "glow.png"), img)


def build_mote(size=8):
    """A small soft dot: a spark of light, a star."""
    img = new_image(size, size)
    half = size / 2.0
    for y in range(size):
        for x in range(size):
            d = math.hypot(x + 0.5 - half, y + 0.5 - half) / half
            img[y][x] = [255, 255, 255, clamp_byte(255 * max(0.0, 1.0 - d) ** 1.4)]
    write_png(os.path.join(OUT, "mote.png"), img)


# --------------------------------------------------------------------------- #
# torch-flame.png -- six frames of a torch flame, base at the bottom middle
# --------------------------------------------------------------------------- #
# A teardrop that sways more the higher up it goes, hottest at the bottom middle. Every frame is
# its own seeded shape, so played at random offsets two torches never look the same.

FLAME_W = 12
FLAME_H = 20
FLAME_FRAMES = 6

#: Coolest to hottest.
FLAME_RAMP = ((150, 44, 20), (204, 78, 26), (246, 140, 46), (255, 204, 108), (255, 244, 212))

#: Where the heat crosses from one shade of FLAME_RAMP to the next.
FLAME_STEPS = (0.22, 0.42, 0.66, 0.92)


def build_flame(seed=7070):
    img = new_image(FLAME_W * FLAME_FRAMES, FLAME_H)
    for f in range(FLAME_FRAMES):
        rng = random.Random(seed + f)
        height = rng.uniform(13.5, 17.5)
        width = rng.uniform(3.3, 3.9)
        phase = f * (2.0 * math.pi / FLAME_FRAMES) + rng.uniform(-0.4, 0.4)
        sway = rng.uniform(1.0, 1.9)

        def centre(t):
            return FLAME_W / 2.0 + sway * math.sin(phase + t * 5.0) * t ** 1.4

        for y in range(FLAME_H):
            t = (FLAME_H - 0.5 - y) / height          # 0 at the bottom row, 1 at the tip
            if t > 1.0:
                continue
            base = min(1.0, (t + 0.12) / 0.32)        # rounds off the bottom
            half = width * math.sqrt(base) * (1.0 - t) ** 0.85
            c = centre(t)
            for x in range(FLAME_W):
                u = abs(x + 0.5 - c) / max(half, 0.01)
                if u > 1.0:
                    continue
                heat = (1.0 - u) * 0.7 + (1.0 - t) * 0.6 - 0.15
                k = sum(1 for step in FLAME_STEPS if heat >= step)
                r, g, b = FLAME_RAMP[k]
                img[y][f * FLAME_W + x] = [r, g, b, 255]

        # A loose spark over the tip, on most frames.
        if rng.random() < 0.6:
            sx = int(centre(1.0) + rng.uniform(-1.5, 1.5))
            sy = int(FLAME_H - height - rng.uniform(1.5, 3.5))
            if 0 <= sx < FLAME_W and 0 <= sy < FLAME_H:
                img[sy][f * FLAME_W + sx] = [255, 196, 110, 255]

    write_png(os.path.join(OUT, "torch-flame.png"), img)


# --------------------------------------------------------------------------- #
# Tiles and props: shared bits
# --------------------------------------------------------------------------- #

TILE = 16
SHEET_COLUMNS = 8

#: The darkest edge a prop or a detail tile gets on its shadow side.
INK = (18, 14, 13)

#: Iron, for hoops, straps, plates and pins: its middle, the edge that catches the light, and the
#: dark of a hole through it.
IRON = (78, 74, 76)
IRON_LIT = (150, 142, 138)
IRON_DARK = (30, 28, 30)

#: Old wood, for the barrel, the crate, the chest and the sign.
PLANK = (98, 72, 48)
PLANK_D = (62, 44, 30)


def opaque(c, k=1.0):
    return [clamp_byte(c[0] * k), clamp_byte(c[1] * k), clamp_byte(c[2] * k), 255]


def speck(x, y, seed, amount):
    """A multiplier near 1 that's different for every pixel, the same every run."""
    return 1.0 + (hash01(x, y, seed) - 0.5) * amount


def tile_noise(x, y, seed, cell=4):
    """Smooth noise that wraps every 16 pixels, so a tile's left edge matches its right and its top its bottom."""
    n = TILE // cell
    gx, gy = x / cell, y / cell
    x0, y0 = int(math.floor(gx)), int(math.floor(gy))
    fx, fy = gx - x0, gy - y0
    sx, sy = fx * fx * (3 - 2 * fx), fy * fy * (3 - 2 * fy)

    def h(i, j):
        return hash01(i % n, j % n, seed)
    top = h(x0, y0) + (h(x0 + 1, y0) - h(x0, y0)) * sx
    bottom = h(x0, y0 + 1) + (h(x0 + 1, y0 + 1) - h(x0, y0 + 1)) * sx
    return top + (bottom - top) * sy


def outline(img, ox, oy, w, h):
    """A dark edge round anything drawn on its own (props, detail tiles): darkest on the shadow side
    (right and bottom), a shade down on the lit side. The same selective outline as The Black Box's people."""
    solid = [[img[oy + y][ox + x][3] > 0 for x in range(w)] for y in range(h)]

    def empty(x, y):
        return x < 0 or y < 0 or x >= w or y >= h or not solid[y][x]
    for y in range(h):
        for x in range(w):
            if not solid[y][x]:
                continue
            p = img[oy + y][ox + x]
            if empty(x + 1, y) or empty(x, y + 1):
                img[oy + y][ox + x] = [int(p[0] * 0.25 + INK[0] * 0.75), int(p[1] * 0.25 + INK[1] * 0.75),
                                       int(p[2] * 0.25 + INK[2] * 0.75), 255]
            elif empty(x - 1, y) or empty(x, y - 1):
                img[oy + y][ox + x] = [int(p[0] * 0.78), int(p[1] * 0.78), int(p[2] * 0.78), 255]


def blocks(img, ox, oy, rows, base, mortar, seed, v, bevel=(1.12, 0.84), grime=0.0):
    """Courses of stone blocks filling a tile. rows is [(top, bottom, [joints...])]; a joint is the
    x where one block ends and the next begins. Mortar runs along the top of each course and down
    each joint, and every block is lit on its top and left edges."""
    for top, bottom, joints in rows:
        edges = sorted(set([0] + list(joints) + [TILE]))
        for x0, x1 in zip(edges, edges[1:]):
            tint = 1.0 + (hash01(x0 + v * 7, top, seed) - 0.5) * 0.14
            for y in range(top, bottom):
                for x in range(x0, x1):
                    if y == top or x == x0:
                        img[oy + y][ox + x] = opaque(mortar, speck(x, y, seed, 0.1))
                        continue
                    k = tint * speck(x, y, seed + 1, 0.10) * (0.92 + 0.16 * tile_noise(x, y, seed + v, 4))
                    if y == top + 1 or x == x0 + 1:
                        k *= bevel[0]
                    elif y == bottom - 1 or x == x1 - 1:
                        k *= bevel[1]
                    k *= 1.0 - grime * (y / TILE)
                    img[oy + y][ox + x] = opaque(base, k)


# --------------------------------------------------------------------------- #
# Tiles/crypt.png
# --------------------------------------------------------------------------- #
# The barrow: big worn slabs underfoot, walls seen from above (their tops) and from the front (two
# rows of face), and the bones and rubble lying about.

SLAB = (80, 72, 64)
SLAB_GAP = (36, 32, 30)
CRYPT_TOP = (54, 50, 52)
WALL_FACE = (106, 98, 90)
BONE = (196, 186, 164)


def paint_floor(img, ox, oy, v, seed):
    # Every tile has a joint down its left and along its top, and one more inside that depends on the variant.
    inner = ((8, None), (None, 9), (None, None), (6, 5))[v % 4]
    for y in range(TILE):
        for x in range(TILE):
            joint = x == 0 or y == 0 or y == inner[0] or (x == inner[1] and (inner[0] is None or y < inner[0]))
            if v == 3 and x == 5 and y < 6:
                joint = True
            if joint:
                img[oy + y][ox + x] = opaque(SLAB_GAP, speck(x, y, seed, 0.1))
                continue
            # Each slab its own shade, worn and pitted, with only a little catch of light on its edge.
            slab = (y > (inner[0] or 99)) * 2 + (x > (inner[1] or 99))
            k = speck(x, y, seed, 0.16) * (0.86 + 0.26 * tile_noise(x, y, seed, 8)) * (1.0 + 0.1 * (hash01(slab, v, seed) - 0.5))
            lit = x == 1 or y == 1 or (inner[0] is not None and y == inner[0] + 1) or (inner[1] is not None and x == inner[1] + 1)
            k *= 1.06 if lit else 1.0
            if hash01(x, y, seed + 9) > 0.97:
                k *= 0.7
            img[oy + y][ox + x] = opaque(SLAB, k)


def paint_floor_worn(img, ox, oy, v, seed):
    paint_floor(img, ox, oy, 2, seed)
    for y in range(2, TILE - 1):
        for x in range(2, TILE - 1):
            d = math.hypot(x - 8.5 - v, y - 8.5) / 7.0
            if d < 1.0:
                p = img[oy + y][ox + x]
                img[oy + y][ox + x] = opaque(p, 1.0 + 0.12 * (1.0 - d))


def paint_floor_shadow(img, ox, oy, v, seed):
    """Floor right under a wall's face, darker the closer it is to the wall."""
    paint_floor(img, ox, oy, v, seed)
    for y in range(TILE):
        for x in range(TILE):
            img[oy + y][ox + x] = opaque(img[oy + y][ox + x], 0.55 + 0.4 * (y / TILE))


def paint_steps(img, ox, oy, v, seed):
    for y in range(TILE):
        step = y % 5
        for x in range(TILE):
            k = (1.15 if step < 2 else 0.62 if step < 4 else 0.45) * speck(x, y, seed, 0.1)
            img[oy + y][ox + x] = opaque(SLAB, k)


def paint_wall_top(img, ox, oy, v, seed):
    rows = ((0, 8, (7 + 2 * v,)), (8, TILE, (3, 12)))
    blocks(img, ox, oy, rows, CRYPT_TOP, (24, 22, 24), seed, v, bevel=(1.15, 0.85))


def paint_wall_face(img, ox, oy, v, seed):
    rows = ((0, 8, (5 + 3 * v, 12)), (8, TILE, (2 + v, 9)))
    rows = tuple((t, b, [j % TILE for j in joints]) for t, b, joints in rows)
    blocks(img, ox, oy, rows, WALL_FACE, SLAB_GAP, seed, v, grime=0.1)
    if v == 1:
        # A corner knocked off one block, leaving a dark hollow.
        for y in range(9, 14):
            for x in range(10, 15):
                if (x - 10) + (y - 9) < 5:
                    img[oy + y][ox + x] = opaque(SLAB_GAP, 1.2 + 0.1 * (y - 9))
    if v == 2:
        # Damp coming down from above.
        for y in range(TILE):
            for x in range(5, 9):
                if hash01(x, y // 2, seed) > 0.35:
                    img[oy + y][ox + x] = opaque(img[oy + y][ox + x], 0.78)


def paint_wall_niche(img, ox, oy, v, seed):
    paint_wall_face(img, ox, oy, 0, seed)
    # An arched hole cut into the face, with somebody's skull still in it.
    for y in range(3, 14):
        for x in range(3, 13):
            inside = y >= 7 or math.hypot(x + 0.5 - 8, y + 0.5 - 7) < 5.0
            if inside:
                img[oy + y][ox + x] = opaque((20, 17, 18), 1.0 + 0.3 * (y - 3) / 10.0)
    for y, row in enumerate(("..##..", ".####.", "#.##.#", "######", ".#..#.")):
        for x, c in enumerate(row):
            if c == "#":
                img[oy + 8 + y][ox + 5 + x] = opaque(BONE, 1.0 if x < 3 else 0.82)
    for x in range(3, 13):
        img[oy + 13][ox + x] = opaque(WALL_FACE, 1.2)


def paint_wall_base(img, ox, oy, v, seed):
    rows = ((0, 10, (4 + 4 * v, 13 - v)),)
    blocks(img, ox, oy, rows, WALL_FACE, SLAB_GAP, seed, v, grime=0.25)
    # A plinth along the bottom, and a black line where the wall meets the floor.
    for y in range(10, TILE):
        for x in range(TILE):
            k = (1.1 if y == 10 else 0.62 if y < TILE - 1 else 0.3) * speck(x, y, seed, 0.1)
            img[oy + y][ox + x] = opaque(WALL_FACE, k)


def paint_rubble(img, ox, oy, v, seed):
    """A heap of broken stone: big lumps at the bottom, smaller ones on top, each lit on its upper left."""
    rng = random.Random(seed)
    stones = [(rng.uniform(3, 13), rng.uniform(11, 13.5), rng.uniform(2.4, 3.2)) for _ in range(4)]
    stones += [(rng.uniform(5, 11), rng.uniform(7.5, 10), rng.uniform(1.8, 2.6)) for _ in range(3)]
    stones += [(rng.uniform(6, 10), rng.uniform(5, 7), rng.uniform(1.4, 2.0))]
    for cx, cy, r in stones:
        shade = rng.uniform(0.85, 1.1)
        for y in range(TILE):
            for x in range(TILE):
                # Lumpy, not round: the radius wobbles with the angle.
                a = math.atan2(y + 0.5 - cy, x + 0.5 - cx)
                wobble = 1.0 + 0.18 * math.sin(a * 3 + cx) + 0.1 * math.sin(a * 5 + cy)
                if math.hypot(x + 0.5 - cx, (y + 0.5 - cy) * 1.25) < r * wobble:
                    lit = (x + 0.5 - cx) + (y + 0.5 - cy) * 1.4 < -0.3
                    img[oy + y][ox + x] = opaque(WALL_FACE, shade * (1.15 if lit else 0.78) * speck(x, y, seed, 0.12))
    outline(img, ox, oy, TILE, TILE)


def paint_bones(img, ox, oy, v, seed):
    rng = random.Random(seed)
    for _ in range(3 + v):
        x, y = rng.randint(2, 10), rng.randint(3, 13)
        length = rng.randint(3, 5)
        down = rng.choice((0, 1))
        for i in range(length):
            xx, yy = x + i, y + (i // 2 if down else 0)
            if xx < TILE and yy < TILE:
                img[oy + yy][ox + xx] = opaque(BONE, 1.05 if i in (0, length - 1) else 0.9)
    if v == 1:
        # A skull, looking up.
        for y, row in enumerate((".###.", "#####", "#.#.#", ".###.")):
            for x, c in enumerate(row):
                if c == "#":
                    img[oy + 4 + y][ox + 9 + x] = opaque(BONE, 1.0 if y < 2 else 0.8)
    outline(img, ox, oy, TILE, TILE)


def paint_cobweb(img, ox, oy, v, seed):
    """A web in the top-left corner: threads out from the corner, and arcs across them."""
    for y in range(TILE):
        for x in range(TILE):
            a = math.atan2(y + 0.5, x + 0.5)
            r = math.hypot(x + 0.5, y + 0.5)
            spoke = min(abs(a - k * math.pi / 8) for k in range(5)) * r < 0.5
            arc = abs(r % 4.0 - 2.0) < 0.35 and r < 13
            if (spoke and r < 14) or arc:
                img[oy + y][ox + x] = [200, 196, 190, 110 if arc else 150]


CRYPT_TILES = (         # (name, variants, painter), in the same order as TilesetCatalog.Crypt
    ("floor", 4, paint_floor),
    ("floor-worn", 2, paint_floor_worn),
    ("floor-shadow", 2, paint_floor_shadow),
    ("steps", 1, paint_steps),
    ("wall-top", 2, paint_wall_top),
    ("wall-face", 3, paint_wall_face),
    ("wall-niche", 1, paint_wall_niche),
    ("wall-base", 2, paint_wall_base),
    ("rubble", 1, paint_rubble),
    ("bones", 2, paint_bones),
    ("cobweb", 1, paint_cobweb),
)


# --------------------------------------------------------------------------- #
# Tiles/forest.png
# --------------------------------------------------------------------------- #
# The old road: a forest floor of dead leaves and moss, the road worn through it, undergrowth too
# thick to get through, and the trees. A tree is a trunk on the detail layer and a crown on the
# overhead layer. The crown is a heap of round clumps of leaves, each lit on its upper left, set on
# a grid that's been shaken about and wraps round the tile, so crown tiles meet. The edge pieces leave out the outside clumps, so the crown's edge is round ones.

LITTER = (84, 68, 50)
LITTER_FLECKS = ((110, 84, 52), (74, 78, 46), (122, 98, 62), (64, 50, 38))
ROAD_DIRT = (102, 88, 70)
ROAD_STONE = (126, 116, 104)
WOOD_MOSS = (64, 76, 46)
THICKET = (34, 42, 30)
THICKET_LEAF = (56, 70, 42)
FROND = (74, 100, 54)
FROND_LIT = (104, 128, 68)
BOULDER = (106, 102, 96)
BARK = (72, 56, 42)
BARK_LIT = (104, 82, 60)
HEARTWOOD = (146, 116, 80)
GRASS = (76, 90, 56)
GRASS_D = (58, 70, 44)
GRASS_L = (102, 116, 72)

#: The crown's colours, darkest to lightest: the gaps between clumps, their shadowed side, the
#: middle, the lit side, and the highlight on the very top of each.
LEAVES = ((24, 34, 24), (40, 54, 34), (60, 78, 44), (84, 102, 56), (110, 126, 66))

#: How many clumps across (and down) one crown tile.
CLUMPS = 3

#: Where the middle of each clump is, the same in every crown tile, so any two of them meet.
CLUMP_CENTRES = {(i, j): ((i + 0.2 + 0.6 * hash01(i, j, 81)) * TILE / CLUMPS,
                          (j + 0.2 + 0.6 * hash01(i, j, 82)) * TILE / CLUMPS)
                 for i in range(CLUMPS) for j in range(CLUMPS)}


def paint_tuft(img, ox, oy, v, seed):
    """A clump of long grass: blades fanning out of one root, the outer ones bending over."""
    rng = random.Random(seed)
    root = (7.5 + v, 14.5)
    for i in range(-5, 6):
        lean = i * 0.9 + rng.uniform(-0.3, 0.3)
        h = rng.uniform(8.0, 11.0) - abs(i) * 0.7
        for j in range(int(h)):
            t = j / h
            x = int(math.floor(root[0] + lean * t * t))
            y = int(math.floor(root[1] - j))
            if 0 <= x < TILE and 0 <= y < TILE:
                shade = GRASS_L if t > 0.6 else (GRASS if t > 0.25 else GRASS_D)
                img[oy + y][ox + x] = opaque(shade, speck(x, y, seed, 0.12))
    outline(img, ox, oy, TILE, TILE)


def paint_litter(img, ox, oy, v, seed):
    """Dead leaves: a brown floor with flecks of every colour a leaf goes, a few pixels each."""
    rng = random.Random(seed)
    for y in range(TILE):
        for x in range(TILE):
            k = speck(x, y, seed, 0.14) * (0.9 + 0.2 * tile_noise(x, y, seed, 8))
            img[oy + y][ox + x] = opaque(LITTER, k)
    for _ in range(14 + v * 2):
        x, y = rng.randint(0, TILE - 2), rng.randint(0, TILE - 1)
        c = rng.choice(LITTER_FLECKS)
        img[oy + y][ox + x] = opaque(c, 1.08)
        img[oy + y][ox + x + 1] = opaque(c, 0.86)


def paint_road(img, ox, oy, v, seed):
    """Packed dirt, paler than the floor round it, with stones worked up through it."""
    rng = random.Random(seed)
    for y in range(TILE):
        for x in range(TILE):
            k = speck(x, y, seed, 0.12) * (0.92 + 0.16 * tile_noise(x, y, seed, 8))
            img[oy + y][ox + x] = opaque(ROAD_DIRT, k)
    for _ in range(2 + v):
        x, y = rng.randint(1, TILE - 4), rng.randint(1, TILE - 3)
        w = rng.randint(1, 2)
        for dx in range(w + 1):
            img[oy + y][ox + x + dx] = opaque(ROAD_STONE, 1.1)
            img[oy + y + 1][ox + x + dx] = opaque(ROAD_STONE, 0.72)


#: Where the cushions of moss go on each moss tile: (x, y, radius). Off-centre and different sizes, so
#: a patch of moss tiles doesn't line up into rows.
MOSS_CUSHIONS = (((6.0, 9.5, 5.2), (12.5, 4.0, 3.0)),
                 ((4.0, 4.5, 3.2), (11.0, 8.0, 4.2), (5.5, 12.5, 3.4)))


def paint_wood_moss(img, ox, oy, v, seed):
    """Cushions of moss lying on the leaves, lumpy round the edge and lighter on top. None of them
    reaches the side of the tile, so a patch of moss has leaves showing through it."""
    for i, (cx, cy, r) in enumerate(MOSS_CUSHIONS[v]):
        for y in range(TILE):
            for x in range(TILE):
                a = math.atan2(y + 0.5 - cy, x + 0.5 - cx)
                edge = r * (1.0 + 0.12 * math.sin(a * 3 + i + v) + 0.08 * math.sin(a * 5 + i))
                d = math.hypot(x + 0.5 - cx, (y + 0.5 - cy) * 1.15)
                if d > edge:
                    continue
                lean = (x + 0.5 - cx) + (y + 0.5 - cy) * 1.3
                k = (1.18 if lean < -edge * 0.6 else 0.76 if d > edge - 1.0 and lean > 0 else 1.0)
                img[oy + y][ox + x] = opaque(WOOD_MOSS, k * speck(x, y, seed + i, 0.1))


def paint_thicket(img, ox, oy, v, seed):
    """Undergrowth too thick to get through: dark, with leaves and stems tangled in it."""
    rng = random.Random(seed)
    for y in range(TILE):
        for x in range(TILE):
            k = speck(x, y, seed, 0.2) * (0.75 + 0.5 * tile_noise(x, y, seed + v, 4))
            img[oy + y][ox + x] = opaque(THICKET, k)
    for _ in range(9):
        x, y = rng.randint(0, TILE - 3), rng.randint(0, TILE - 3)
        img[oy + y][ox + x] = opaque(THICKET_LEAF, 1.15)
        img[oy + y][ox + x + 1] = opaque(THICKET_LEAF, 0.95)
        img[oy + y + 1][ox + x] = opaque(THICKET_LEAF, 0.8)
    for _ in range(3):
        x, y = rng.randint(0, TILE - 1), rng.randint(0, TILE - 5)
        for i in range(4):
            img[oy + y + i][ox + min(TILE - 1, x + i // 2)] = opaque(BARK, 0.8)


def paint_fern(img, ox, oy, v, seed):
    """A fern: fronds arching up and out of one root, with leaflets down both sides of each that
    get shorter toward the tip, and the tips bending back down."""
    root = (8.0 + (v - 0.5), 15.0)
    for n, lean in enumerate((-1.1, -0.55, 0.0, 0.5, 1.05) if v == 0 else (-1.0, -0.45, 0.1, 0.6)):
        length = 10.0 - abs(lean) * 2.5
        for j in range(1, int(length) + 1):
            t = j / length
            # Up and out, then over: the tip droops back toward the ground.
            x = root[0] + math.sin(lean) * j * 1.05
            y = root[1] - math.cos(lean) * j * 1.05 + t * t * (2.5 + abs(lean) * 3)
            shade = FROND_LIT if j < length * 0.7 and lean < 0.3 else FROND
            leaflet = max(0, int(round(2.4 * (1.0 - t))))
            for side in (-1, 1):
                for k in range(leaflet + 1):
                    xx = int(math.floor(x + side * k * math.cos(lean)))
                    yy = int(math.floor(y + side * k * math.sin(lean) * 0.5 + k * 0.4))
                    if 0 <= xx < TILE and 0 <= yy < TILE:
                        img[oy + yy][ox + xx] = opaque(shade if k == 0 else FROND, speck(xx, yy, seed + n, 0.1))
    outline(img, ox, oy, TILE, TILE)


def paint_rock(img, ox, oy, v, seed):
    """A boulder, lit on its upper left, with moss on its top."""
    for y in range(TILE):
        for x in range(TILE):
            a = math.atan2(y + 0.5 - 9.5, x + 0.5 - 8)
            r = 6.4 * (1.0 + 0.1 * math.sin(a * 3 + 1.0) + 0.06 * math.sin(a * 5))
            if math.hypot(x + 0.5 - 8, (y + 0.5 - 9.5) * 1.25) > r:
                continue
            lean = (x + 0.5 - 8) + (y + 0.5 - 9.5) * 1.3
            k = (1.2 if lean < -3 else 1.0 if lean < 2 else 0.72) * speck(x, y, seed, 0.12)
            c = WOOD_MOSS if y < 7 and hash01(x, y, seed) > 0.35 else BOULDER
            img[oy + y][ox + x] = opaque(c, k)
    outline(img, ox, oy, TILE, TILE)


def paint_stump(img, ox, oy, v, seed):
    """What's left of a felled tree: bark round the side, the cut on top with its rings."""
    for y in range(3, 15):
        for x in range(2, 14):
            top = math.hypot((x + 0.5 - 8) / 5.5, (y + 0.5 - 6.5) / 3.0) < 1.0
            side = abs(x + 0.5 - 8) < 5.5 and 6.5 <= y + 0.5 <= 13.5
            if top:
                d = math.hypot((x + 0.5 - 8) / 5.5, (y + 0.5 - 6.5) / 3.0)
                ring = int(d * 4) % 2
                img[oy + y][ox + x] = opaque(HEARTWOOD, (1.05 if ring else 0.88) * speck(x, y, seed, 0.06))
            elif side:
                img[oy + y][ox + x] = opaque(BARK_LIT if x < 7 else BARK, (0.9 if x % 3 == 0 else 1.0) * speck(x, y, seed, 0.1))
    outline(img, ox, oy, TILE, TILE)


def paint_log(img, ox, oy, v, seed, end):
    """Half a fallen tree lying across two tiles. end is the side with the cut end showing its rings."""
    for y in range(5, 14):
        for x in range(TILE):
            if end == "left" and x < 3 or end == "right" and x > TILE - 4:
                continue
            k = (1.2 if y == 6 else 1.05 if y < 9 else 0.8 if y < 12 else 0.62) * speck(x, y, seed, 0.1)
            if (x + y * 3) % 7 == 0:
                k *= 0.8
            img[oy + y][ox + x] = opaque(BARK_LIT if y < 9 else BARK, k)
    cx = 4.5 if end == "left" else TILE - 4.5
    for y in range(5, 14):
        for x in range(TILE):
            d = math.hypot((x + 0.5 - cx) / 2.6, (y + 0.5 - 9.5) / 4.5)
            if d < 1.0:
                img[oy + y][ox + x] = opaque(HEARTWOOD, (1.0 if int(d * 3) % 2 else 0.84) * speck(x, y, seed, 0.06))
    outline(img, ox, oy, TILE, TILE)


def paint_log_left(img, ox, oy, v, seed):
    paint_log(img, ox, oy, v, seed, "left")


def paint_log_right(img, ox, oy, v, seed):
    paint_log(img, ox, oy, v, seed, "right")


def paint_trunk(img, ox, oy, v, seed):
    """The bottom of a tree, coming down out of its crown into the ground, with its roots spreading
    and a shadow round its foot."""
    for y in range(TILE):
        flare = max(0.0, (y - 10) / 5.0) ** 2 * 3.0
        half = 3.2 + flare
        for x in range(TILE):
            dx = x + 0.5 - 8
            if abs(dx) > half:
                continue
            k = (1.15 if dx < -1.5 else 0.95 if dx < 1.0 else 0.66) * speck(x, y, seed, 0.12)
            if (x * 5 + y) % 6 == 0:
                k *= 0.78
            img[oy + y][ox + x] = opaque(BARK_LIT if dx < -1.5 else BARK, k)
    outline(img, ox, oy, TILE, TILE)
    # The shadow it stands in, put down after the outline so it doesn't get one.
    for y in range(12, TILE):
        for x in range(TILE):
            if img[oy + y][ox + x][3] == 0 and math.hypot((x + 0.5 - 9) / 7.5, (y + 0.5 - 14.5) / 2.0) < 1.0:
                img[oy + y][ox + x] = [0, 0, 0, 90]


def paint_bush(img, ox, oy, v, seed):
    """A round bush: a heap of the same clumps the crowns are made of, in a ball."""
    def keep(cx, cy):
        return math.hypot(cx - 8.0, (cy - 9.5) * 1.2) < 5.0 + v * 0.6

    for y in range(TILE):
        for x in range(TILE):
            c = leaf_colour(x, y, seed, keep)
            if c is not None and math.hypot(x + 0.5 - 8, (y + 0.5 - 9.5) * 1.2) < 7.2:
                img[oy + y][ox + x] = opaque(c)
    outline(img, ox, oy, TILE, TILE)


#: How far a clump of leaves reaches from its middle, in art pixels. A bit more than half the gap
#: between clumps, so they overlap into one heap with only small holes in it.
CLUMP_RADIUS = 4.2


def leaf_colour(x, y, seed, keep=None):
    """The crown's colour at a pixel, or None if no clump covers it. Where clumps overlap, the lower
    one is in front (it's nearer, looking down from the south), and each is lit on its upper left
    with a dark edge on its lower right. keep says which clumps are there, by their middle in tile
    pixels; the edge pieces leave out the ones past their edge."""
    size = TILE / CLUMPS
    px, py = x + 0.5, y + 0.5
    front = None
    ci, cj = int(px // size), int(py // size)
    for i in range(ci - 1, ci + 2):
        for j in range(cj - 1, cj + 2):
            sx, sy = CLUMP_CENTRES[(i % CLUMPS, j % CLUMPS)]
            sx += (i // CLUMPS) * TILE
            sy += (j // CLUMPS) * TILE
            if keep is not None and not keep(sx, sy):
                continue
            if math.hypot(px - sx, py - sy) > CLUMP_RADIUS:
                continue
            if front is None or (sy, sx) > (front[1], front[0]):
                front = (sx, sy, i % CLUMPS, j % CLUMPS)
    if front is None:
        return None
    dx, dy = px - front[0], py - front[1]
    d = math.hypot(dx, dy) / CLUMP_RADIUS
    light = -(dx * 0.8 + dy) / CLUMP_RADIUS
    if d > 0.78 and light < 0.15:
        shade = 1
    elif light > 0.62 and d < 0.72:
        shade = 4 if light > 0.8 else 3
    elif light > 0.2:
        shade = 3
    elif light > -0.5:
        shade = 2
    else:
        shade = 1
    # A few pixels a band up or down, so the bands break up the way leaves do.
    if hash01(x, y, seed) > 0.88:
        shade = max(1, min(4, shade + (1 if hash01(x, y, seed + 1) > 0.5 else -1)))
    k = 1.0 + 0.12 * (hash01(front[2], front[3], 83) - 0.5)
    return tuple(clamp_byte(c * k) for c in LEAVES[shade])


def paint_crown(img, ox, oy, v, seed, north=False, south=False, west=False, east=False):
    """One piece of a crown. The sides it's on leave out the clumps in the outside third of the tile,
    so the crown's edge is the round edges of the next clumps in, and get a dark rim and a shadow on
    the ground under the bottom and right. A corner leaves out the clump in its corner as well."""
    third = TILE / CLUMPS
    corner = ((TILE if west else 0.0) if (north or south) and (west or east) else None,
              (TILE if north else 0.0) if (north or south) and (west or east) else None)

    def keep(cx, cy):
        if (north and cy < third) or (south and cy > TILE - third) or (west and cx < third) or (east and cx > TILE - third):
            return False
        # On a corner piece, the clump nearest the corner goes too, which rounds the crown off.
        return corner[0] is None or math.hypot(cx - corner[0], cy - corner[1]) < TILE * 0.68

    def inside(x, y):
        if x < 0 or y < 0 or x >= TILE or y >= TILE:
            return True             # the next tile carries on
        return leaf_colour(x, y, seed, keep) is not None

    for y in range(TILE):
        for x in range(TILE):
            c = leaf_colour(x, y, seed, keep)
            if c is None:
                # A hole in the middle of the heap is just deep shade; past the edge, it's the
                # shadow the crown throws on the ground, just under its bottom and right.
                edge = (north and y < third) or (south and y >= TILE - third) or (west and x < third) or (east and x >= TILE - third)
                if not edge:
                    img[oy + y][ox + x] = opaque(LEAVES[0])
                elif (south and inside(x, y - 2)) or (east and inside(x - 2, y)):
                    img[oy + y][ox + x] = [0, 0, 0, 70]
                continue
            # Lighter toward the crown's top and left, darker toward its bottom and right.
            k = 1.0
            if north:
                k *= 1.0 + 0.1 * (1.0 - y / TILE)
            if south:
                k *= 1.0 - 0.14 * (y / TILE)
            if west:
                k *= 1.0 + 0.05 * (1.0 - x / TILE)
            if east:
                k *= 1.0 - 0.1 * (x / TILE)
            if not inside(x, y + 1) or not inside(x + 1, y):
                c = lerp_color(c, INK, 0.6)
            elif not inside(x, y - 1) or not inside(x - 1, y):
                c = lerp_color(c, LEAVES[1], 0.45)
            img[oy + y][ox + x] = opaque(c, k * speck(x, y, seed + v, 0.05))


def crown_painter(**sides):
    return lambda img, ox, oy, v, seed: paint_crown(img, ox, oy, v, seed, **sides)


FOREST_TILES = (        # (name, variants, painter), in the same order as TilesetCatalog.Forest
    ("leaves", 4, paint_litter),
    ("road", 3, paint_road),
    ("thicket", 2, paint_thicket),
    ("moss", 2, paint_wood_moss),
    ("tuft", 2, paint_tuft),
    ("fern", 2, paint_fern),
    ("rock", 1, paint_rock),
    ("stump", 1, paint_stump),
    ("log-left", 1, paint_log_left),
    ("log-right", 1, paint_log_right),
    ("trunk", 1, paint_trunk),
    ("bush", 2, paint_bush),
    ("canopy-nw", 1, crown_painter(north=True, west=True)),
    ("canopy-n", 2, crown_painter(north=True)),
    ("canopy-ne", 1, crown_painter(north=True, east=True)),
    ("canopy-w", 2, crown_painter(west=True)),
    ("canopy", 3, crown_painter()),
    ("canopy-e", 2, crown_painter(east=True)),
    ("canopy-sw", 1, crown_painter(south=True, west=True)),
    ("canopy-s", 2, crown_painter(south=True)),
    ("canopy-se", 1, crown_painter(south=True, east=True)),
)


def build_tileset(name, tiles, seed):
    """Every tile's variants one after another, eight to a row, in the order the catalogue lists them."""
    frames = sum(variants for _, variants, _ in tiles)
    rows = (frames + SHEET_COLUMNS - 1) // SHEET_COLUMNS
    img = new_image(SHEET_COLUMNS * TILE, rows * TILE)
    i = 0
    for _, variants, painter in tiles:
        for v in range(variants):
            painter(img, (i % SHEET_COLUMNS) * TILE, (i // SHEET_COLUMNS) * TILE, v, seed + i * 31)
            i += 1
    write_png(os.path.join(OUT, "Tiles", name + ".png"), img)


# --------------------------------------------------------------------------- #
# Props/*.png -- one picture each, standing on the middle of its bottom edge
# --------------------------------------------------------------------------- #


def prop_image(w, h):
    return new_image(w, h)


def lit_side(x, cx):
    """How much light a round thing gets across its width: bright on the left, falling off to the right."""
    return 1.18 - 0.45 * (x - cx + 4) / 8.0


def build_barrel(seed=8101):
    w, h = 14, 18
    img = prop_image(w, h)
    cx = w / 2.0
    for y in range(h):
        for x in range(w):
            bulge = 6.4 + 0.6 * math.sin(math.pi * y / (h - 1))
            if abs(x + 0.5 - cx) > bulge:
                continue
            stave = int((x + 0.5 - cx + 7) // 2.5)
            k = lit_side(x + 0.5, cx) * (0.85 if (x + 0.5 - cx + 7) % 2.5 < 0.6 else 1.0) * speck(x, y, seed + stave, 0.08)
            colour = IRON if y in (3, 4, 13, 14) else PLANK
            img[y][x] = opaque(colour, k)
    # The lid, seen from above: an ellipse of end grain with a dark rim.
    for y in range(4):
        for x in range(w):
            d = math.hypot((x + 0.5 - cx) / 6.2, (y + 0.5 - 2.0) / 2.0)
            if d < 1.0:
                img[y][x] = opaque(PLANK, 1.25 if d < 0.7 else 0.7)
    outline(img, 0, 0, w, h)
    write_png(os.path.join(OUT, "Props", "barrel.png"), img)


def build_crate(seed=8102):
    w, h = 16, 17
    img = prop_image(w, h)
    for y in range(h):
        for x in range(w):
            top = y < 5
            if top:
                k = 1.25 * speck(x, y, seed, 0.1) * (0.85 if x % 5 == 4 else 1.0)
            else:
                edge = x in (0, w - 1) or y in (5, h - 1)
                brace = abs((x - 1) - (y - 6) * (w - 3) / (h - 7)) < 1.0
                k = (1.05 if edge or brace else 0.82) * speck(x, y, seed + 1, 0.1) * (0.9 if (y - 5) % 4 == 3 else 1.0)
                k *= 1.0 - 0.25 * x / w
            img[y][x] = opaque(PLANK, k)
    outline(img, 0, 0, w, h)
    write_png(os.path.join(OUT, "Props", "crate.png"), img)


def build_urn(seed=8104):
    w, h = 10, 14
    img = prop_image(w, h)
    cx = w / 2.0
    clay = (122, 92, 70)
    for y in range(h):
        width = 2.2 if y < 2 else 4.6 * math.sin(math.pi * min(1.0, (y - 1) / (h - 1))) + 1.2
        for x in range(w):
            if abs(x + 0.5 - cx) <= width:
                k = lit_side(x + 0.5, cx) * speck(x, y, seed, 0.1) * (0.8 if y == 5 else 1.0)
                img[y][x] = opaque(clay, k)
    outline(img, 0, 0, w, h)
    write_png(os.path.join(OUT, "Props", "urn.png"), img)


def build_sarcophagus(seed=8105):
    w, h = 34, 26
    img = prop_image(w, h)
    lid_bottom = 15
    for y in range(h):
        for x in range(w):
            if y < lid_bottom:
                # The lid from above, with a figure carved lying on it.
                body = math.hypot((x + 0.5 - 17) / 11.0, (y + 0.5 - 8) / 4.0) < 1.0
                head = math.hypot(x + 0.5 - 6.5, y + 0.5 - 8) < 2.6
                k = (1.28 if y < 2 else 1.15) * speck(x, y, seed, 0.08)
                if body or head:
                    k *= 1.12 if y < 8 else 0.86
                img[y][x] = opaque(WALL_FACE, k)
            else:
                # The front of the box, in shadow under the lid's lip, with a carved panel.
                panel = 4 <= x < w - 4 and 18 <= y < h - 2
                k = (0.5 if y == lid_bottom else 0.78) * (0.85 if panel and (x in (4, w - 5) or y in (18, h - 3)) else 1.0)
                img[y][x] = opaque(WALL_FACE, k * speck(x, y, seed + 1, 0.1))
    outline(img, 0, 0, w, h)
    write_png(os.path.join(OUT, "Props", "sarcophagus.png"), img)


def build_sconce(seed=8106):
    """An iron plate on the wall, an arm, and a torch handle standing in a cup. The flame is drawn by the game."""
    w, h = 8, 18
    img = prop_image(w, h)
    for y in range(9, 18):                              # the plate on the wall
        for x in range(2, 6):
            img[y][x] = opaque(IRON, 1.15 if x == 2 else 0.75)
    for y in range(2, 14):                              # the handle
        img[y][3] = opaque((150, 100, 58))
        img[y][4] = opaque((92, 58, 34))
    for x in range(1, 7):                               # the cup, its rim catching the fire
        img[0][x] = opaque((210, 160, 100))
        img[1][x] = opaque(IRON, 1.2 if x < 4 else 0.85)
        img[2][x] = opaque(IRON, 0.6)
    for x in range(2, 6):                               # the band holding it to the plate
        img[11][x] = opaque(IRON, 1.3 if x < 4 else 0.9)
    outline(img, 0, 0, w, h)
    write_png(os.path.join(OUT, "Props", "sconce.png"), img)


def prop_strip(w, h, count):
    """A strip of `count` pictures side by side, for a prop that opens: the shut one first."""
    return new_image(w * count, h)


CHEST_W, CHEST_H = 16, 15
IRON_BAND = (70, 66, 66)
LOCK = (176, 150, 92)


def build_chest(seed=8201):
    """A wooden chest bound in iron: shut, then open with its lid thrown back and nothing left in it."""
    img = prop_strip(CHEST_W, CHEST_H, 2)
    for f in range(2):
        ox = f * CHEST_W
        body_top = 6
        for y in range(body_top, CHEST_H):
            for x in range(1, CHEST_W - 1):
                band = x in (3, CHEST_W - 4)
                k = (1.0 if y > body_top + 1 else 1.15) * (1.0 - 0.3 * (x / CHEST_W)) * speck(x, y, seed, 0.1)
                if (y - body_top) % 4 == 3:
                    k *= 0.82
                img[y][ox + x] = opaque(IRON_BAND if band else PLANK, k * (1.1 if band else 1.0))
        if f == 0:
            # The lid, shut: a curved top seen from above, its front edge over the box.
            for y in range(0, body_top + 1):
                for x in range(1, CHEST_W - 1):
                    if y == 0 and x in (1, CHEST_W - 2):
                        continue
                    band = x in (3, CHEST_W - 4)
                    k = (1.3 if y < 2 else 1.1 if y < 5 else 0.7) * (1.0 - 0.25 * (x / CHEST_W)) * speck(x, y, seed + 1, 0.08)
                    img[y][ox + x] = opaque(IRON_BAND if band else PLANK, k)
            for dy in range(3):
                for dx in range(2):
                    img[body_top + 1 + dy][ox + 7 + dx] = opaque(LOCK, 1.15 if dx == 0 else 0.85)
        else:
            # Open: the lid stands up behind, and the inside is dark and empty.
            for y in range(0, 4):
                for x in range(1, CHEST_W - 1):
                    k = (0.8 if y == 0 else 0.62) * speck(x, y, seed + 2, 0.08)
                    img[y][ox + x] = opaque(PLANK_D, k)
            for y in range(4, body_top + 2):
                for x in range(2, CHEST_W - 2):
                    img[y][ox + x] = opaque((24, 18, 14), 1.0 + 0.15 * (y - 4))
            for x in range(1, CHEST_W - 1):
                img[body_top + 1][ox + x] = opaque(PLANK, 1.25)
        outline(img, ox, 0, CHEST_W, CHEST_H)
    write_png(os.path.join(OUT, "Props", "chest.png"), img)


#: Doors are two tiles wide and two tall, set in the passage through a wall's face.
DOOR_W, DOOR_H = 32, 32

#: How thick the stone round a doorway is: the jambs either side and the lintel over it.
JAMB = 3
LINTEL = 5


def door_frame(img, ox, seed):
    """The stone round a doorway: a jamb up each side and a lintel across the top, the same blocks as the wall."""
    for y in range(DOOR_H):
        for x in range(DOOR_W):
            jamb = x < JAMB or x >= DOOR_W - JAMB
            if not (jamb or y < LINTEL):
                continue
            joint = (y % 8 == 0 and jamb) or (y == LINTEL - 1) or (y < LINTEL and x % 11 == 0)
            k = (0.55 if joint else 1.12 if x == 0 or y == 0 else 0.7 if x == DOOR_W - 1 else 1.0) * speck(x, y, seed, 0.1)
            img[y][ox + x] = opaque(WALL_FACE, k)


def build_door(seed=8202):
    """A heavy double door in a stone frame, studded with iron. Open, the leaves stand back against the jambs."""
    img = prop_strip(DOOR_W, DOOR_H, 2)
    for f in range(2):
        ox = f * DOOR_W
        if f == 0:
            for y in range(LINTEL, DOOR_H):
                for x in range(JAMB, DOOR_W - JAMB):
                    seam = (x - JAMB) % 4 == 3 or x in (15, 16)
                    k = (0.62 if seam else 1.0) * speck(x, y, seed + (x - JAMB) // 4, 0.1) * (1.0 - 0.18 * (y - LINTEL) / DOOR_H)
                    img[y][ox + x] = opaque(PLANK, k)
            for strap in (10, 24):
                for x in range(JAMB, DOOR_W - JAMB):
                    img[strap][ox + x] = opaque(IRON, 1.25)
                    img[strap + 1][ox + x] = opaque(IRON, 0.6)
                for x in range(JAMB + 2, DOOR_W - JAMB - 1, 4):
                    img[strap][ox + x] = opaque(IRON_LIT)
            # A ring on each leaf.
            for cx in (12, 19):
                for dx, dy in ((0, -1), (-1, 0), (1, 0), (0, 1)):
                    img[17 + dy][ox + cx + dx] = opaque(IRON_LIT if dx <= 0 and dy <= 0 else IRON)
        else:
            # The dark beyond, and the two leaves swung in against the sides.
            for y in range(LINTEL, DOOR_H):
                for x in range(JAMB, DOOR_W - JAMB):
                    if x < JAMB + 3 or x >= DOOR_W - JAMB - 3:
                        k = (0.85 if x in (JAMB, DOOR_W - JAMB - 1) else 0.6) * speck(x, y, seed, 0.1)
                        img[y][ox + x] = opaque(PLANK, k)
        door_frame(img, ox, seed)
        outline(img, ox, 0, DOOR_W, DOOR_H)
    write_png(os.path.join(OUT, "Props", "door.png"), img)


def build_gate(seed=8203):
    """A slab of stone filling a doorway, with a ring carved on it. Open, it's sunk into the floor and only its top shows."""
    img = prop_strip(DOOR_W, DOOR_H, 2)
    for f in range(2):
        ox = f * DOOR_W
        top = LINTEL if f == 0 else DOOR_H - 3
        for y in range(top, DOOR_H):
            for x in range(JAMB, DOOR_W - JAMB):
                k = (1.15 if y == top else 0.95) * speck(x, y, seed, 0.12) * (0.9 + 0.2 * tile_noise(x % TILE, y % TILE, seed, 8))
                if f == 0:
                    r = math.hypot(x + 0.5 - DOOR_W / 2, y + 0.5 - 18)
                    if 5.0 < r < 6.4:
                        k *= 0.6 if y > 18 else 0.75
                    elif 6.4 <= r < 7.2 and y < 18:
                        k *= 1.15
                img[y][ox + x] = opaque(WALL_FACE, k * 0.88)
        door_frame(img, ox, seed + 1)
        outline(img, ox, 0, DOOR_W, DOOR_H)
    write_png(os.path.join(OUT, "Props", "gate.png"), img)


LEVER_W, LEVER_H = 12, 20


def build_lever(seed=8204):
    """An iron plate on the wall with a lever on a pin through it, a wooden grip on the end: up, then down."""
    img = prop_strip(LEVER_W, LEVER_H, 2)
    pin = (6, 12)
    for f in range(2):
        ox = f * LEVER_W
        for y in range(7, 18):
            for x in range(2, 10):
                k = 1.25 if x == 2 or y == 7 else 0.65 if x == 9 or y == 17 else 1.0
                img[y][ox + x] = opaque(IRON, k * speck(x, y, seed, 0.1))
        for x, y in ((3, 8), (8, 8), (3, 16), (8, 16)):
            img[y][ox + x] = opaque(IRON_LIT)
        # The handle: straight up from the pin, or swung down and out to the right.
        for i in range(10):
            if f == 0:
                x, y = pin[0], pin[1] - i
            else:
                x, y = pin[0] + (i * 2) // 3, pin[1] + i // 2
            if 0 <= y < LEVER_H and 0 <= x < LEVER_W:
                grip = i >= 7
                img[y][ox + x] = opaque((168, 116, 66) if grip else IRON_LIT, 1.0)
                if not grip and x + 1 < LEVER_W:
                    img[y][ox + x + 1] = opaque(IRON, 0.8)
        img[pin[1]][ox + pin[0]] = opaque(IRON_DARK)
        outline(img, ox, 0, LEVER_W, LEVER_H)
    write_png(os.path.join(OUT, "Props", "lever.png"), img)


SIGN_W, SIGN_H = 18, 24


def build_sign(seed=8205):
    """A fingerpost: a post, one board pointing left and one pointing right."""
    img = new_image(SIGN_W, SIGN_H)
    for y in range(3, SIGN_H):
        img[y][8] = opaque(PLANK, 1.15)
        img[y][9] = opaque(PLANK, 0.75)
    for top, left in ((3, True), (10, False)):
        for y in range(top, top + 5):
            for x in range(1, SIGN_W - 1):
                tip = abs(y - (top + 2))
                if left and x < 1 + tip or not left and x > SIGN_W - 2 - tip:
                    continue
                k = (1.2 if y == top else 0.7 if y == top + 4 else 1.0) * speck(x, y, seed, 0.1)
                img[y][x] = opaque(PLANK, k)
            # Letters, too small to read, scratched along it.
            for x in range(4, SIGN_W - 4, 2):
                if hash01(x, top, seed) > 0.35:
                    img[top + 2][x] = opaque(PLANK_D, 0.8)
    outline(img, 0, 0, SIGN_W, SIGN_H)
    write_png(os.path.join(OUT, "Props", "sign.png"), img)


WAYPOINT_W, WAYPOINT_H = 16, 30
STANDING_STONE = (112, 108, 104)
RUNE_COLD = (52, 50, 52)
RUNE_AWAKE = (170, 214, 236)


def build_waypoint(seed=8206):
    """A standing stone with a mark cut in it: dark while it's asleep, lit pale once it's been found."""
    img = prop_strip(WAYPOINT_W, WAYPOINT_H, 2)
    rune = {(8, y) for y in range(9, 22)} | {(7, 11), (6, 12), (9, 14), (10, 15), (7, 17), (6, 18), (9, 19), (10, 20)}
    for f in range(2):
        ox = f * WAYPOINT_W
        for y in range(WAYPOINT_H):
            # Wider at the foot, a rounded top, leaning a little.
            half = 5.2 + 1.4 * (y / WAYPOINT_H) - max(0.0, 4.0 - y) ** 2 * 0.3
            mid = 8 + 0.6 * (1.0 - y / WAYPOINT_H)
            for x in range(WAYPOINT_W):
                dx = x + 0.5 - mid
                if abs(dx) > half:
                    continue
                k = (1.18 if dx < -2.5 else 0.98 if dx < 2.0 else 0.7) * speck(x, y, seed, 0.12)
                k *= 0.9 + 0.2 * tile_noise(x, y % TILE, seed, 4)
                c = WOOD_MOSS if y > WAYPOINT_H - 5 and hash01(x, y, seed) > 0.5 else STANDING_STONE
                img[y][ox + x] = opaque(c, k)
        for x, y in rune:
            img[y][ox + x] = opaque(RUNE_AWAKE if f == 1 else RUNE_COLD)
        outline(img, ox, 0, WAYPOINT_W, WAYPOINT_H)
        if f == 1:
            # A faint glow off the mark, on the stone round it.
            for x, y in rune:
                for dx in (-1, 1):
                    p = img[y][ox + x + dx]
                    if (x + dx, y) not in rune and p[3] == 255:
                        img[y][ox + x + dx] = list(lerp_color(p, RUNE_AWAKE + (255,), 0.3))
    write_png(os.path.join(OUT, "Props", "waypoint.png"), img)


CAMPFIRE_W, CAMPFIRE_H = 22, 12
EMBER = (232, 120, 40)


def build_campfire(seed=8207):
    """A ring of stones round two burning logs, with the coals glowing between. The game draws the flame on it."""
    img = new_image(CAMPFIRE_W, CAMPFIRE_H)
    rng = random.Random(seed)
    cx, cy = CAMPFIRE_W / 2.0, 7.0
    for y in range(CAMPFIRE_H):
        for x in range(CAMPFIRE_W):
            d = math.hypot((x + 0.5 - cx) / 9.5, (y + 0.5 - cy) / 4.6)
            if d < 0.62:
                img[y][x] = opaque(EMBER if hash01(x, y, seed) > 0.55 else (60, 36, 26), 0.8 + 0.4 * (1.0 - d))
    for i in range(10):
        a = i / 10.0 * 2.0 * math.pi + rng.uniform(-0.1, 0.1)
        sx, sy = cx + math.cos(a) * 8.5, cy + math.sin(a) * 3.8
        for y in range(CAMPFIRE_H):
            for x in range(CAMPFIRE_W):
                if math.hypot(x + 0.5 - sx, (y + 0.5 - sy) * 1.3) < 1.9:
                    img[y][x] = opaque(BOULDER, 1.2 if y + 0.5 < sy else 0.7)
    for x0, x1, y0, y1 in ((6, 15, 9, 4), (7, 16, 4, 9)):
        for i in range(12):
            t = i / 11.0
            x, y = int(x0 + (x1 - x0) * t), int(y0 + (y1 - y0) * t)
            img[y][x] = opaque(BARK, 0.7)
            img[y - 1][x] = opaque(BARK_LIT, 0.9)
    outline(img, 0, 0, CAMPFIRE_W, CAMPFIRE_H)
    write_png(os.path.join(OUT, "Props", "campfire.png"), img)


BEDROLL_W, BEDROLL_H = 24, 11
BLANKET = (118, 58, 44)
BLANKET_STRIPE = (168, 136, 82)


def build_bedroll(seed=8208):
    """A blanket laid out on the ground across the picture, the near end turned back over a rolled-up
    pillow, with a stripe woven across it near each end. Seen from above at a slant, so it's short."""
    img = new_image(BEDROLL_W, BEDROLL_H)
    for y in range(1, BEDROLL_H):
        for x in range(BEDROLL_W):
            # Rounded off at the corners, a little.
            if (x in (0, BEDROLL_W - 1)) and y in (1, BEDROLL_H - 1):
                continue
            k = speck(x, y, seed, 0.08) * (1.12 - 0.3 * y / BEDROLL_H)
            colour = BLANKET
            if x < 5:
                colour = (150, 132, 108)                # the pillow, a roll of something paler
                k *= 1.15 if x == 1 else 0.7 if x == 4 else 1.0
            elif x < 8:
                k *= 1.2 if x == 5 else 0.92            # the turned-back end, lit along its fold
            elif x in (10, BEDROLL_W - 4):
                colour = BLANKET_STRIPE
            if x >= 8 and (y * 3 + x) % 11 == 0:
                k *= 0.82                               # creases
            img[y][x] = opaque(colour, k)
    outline(img, 0, 0, BEDROLL_W, BEDROLL_H)
    write_png(os.path.join(OUT, "Props", "bedroll.png"), img)


if __name__ == "__main__":
    print("Generating world art for Legend Quest...")
    build_glow()
    build_mote()
    build_flame()
    build_tileset("crypt", CRYPT_TILES, 3200)
    build_tileset("forest", FOREST_TILES, 3300)
    build_barrel()
    build_crate()
    build_urn()
    build_sarcophagus()
    build_sconce()
    build_campfire()
    build_bedroll()
    build_chest()
    build_door()
    build_gate()
    build_lever()
    build_sign()
    build_waypoint()
    print("Done.")
