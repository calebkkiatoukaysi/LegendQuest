"""
World art for Legend Quest past Questeria's walls: the old road's forest and the barrow, their
tilesets and their props, and the torch flame and the two soft textures light and sparkles are
drawn with.

    python tools/generate_world.py

These were the first art I made, and beside the pictures I made with ChatGPT they looked it, so
they're drawn again here in those pictures' style: the colours of my style sheet, lit from the top
left, shadows leaning lavender, dark (not black) outlines, and lots of small detail. The forest
floor is drawn the way generate_harbour.py draws Questeria's lawns, so the two sit together, and
its trees, bushes, stumps, ferns and rocks are cut off my trees sheet with cut_supplied.py's cutter,
some of them turned a deeper green for the back of the wood. The barrow is the same style gone cold:
slate and lavender stone, teal lichen and old bone. The chest, the barrel, the crate and the urns
are cut off my props sheet; the rest (the doors, the lever, the sconce, the sarcophagus, the camp,
the fallen trees, the mushrooms, the waypoint stone and the sign) is drawn here to match.

Only needs CPython. Every bit of noise is hash01 on where it is or a seeded Random, and the cutter
has nothing random in it, so a rerun writes the same bytes. My pictures are read one at a time.

Everything on a map is drawn as if a torch were right on it. The game does the dark itself, the
same as The Black Box's lobby, so the road goes dusky and the barrow black away from its torches.
"""

import colorsys
import math
import os
import random
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from pixels import OUT, clamp_byte, hash01, lerp_color, new_image, write_png  # noqa: E402
from generate_harbour import T, Canvas, blobs, build_sheet, dither, put, tile, turned  # noqa: E402
from cut_supplied import cut_out, paste  # noqa: E402


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
# The palette, from my style sheet: the forest's greens and earths, and the barrow's cold stone
# --------------------------------------------------------------------------- #
# Every ramp goes dark to light. The forest's are a little deeper and bluer than Questeria's, it
# being dusk under trees; the barrow's are the style sheet's slate and lavender greys.

GRASS = ((30, 64, 60), (48, 94, 66), (74, 124, 66), (104, 150, 70), (140, 176, 82), (182, 202, 108))
EARTH = ((62, 40, 40), (92, 60, 48), (124, 86, 62), (154, 114, 80), (182, 144, 102), (208, 176, 132))
MOSS = ((28, 66, 60), (44, 96, 70), (76, 130, 72), (120, 166, 84), (170, 198, 106))
UNDERGROWTH = ((12, 28, 32), (20, 46, 46), (32, 70, 56), (50, 98, 62), (82, 130, 70), (124, 162, 86))
BARK = ((52, 32, 34), (84, 52, 44), (114, 72, 52), (146, 98, 66), (182, 132, 88), (214, 170, 120))
HEARTWOOD = ((150, 100, 66), (196, 146, 96), (226, 184, 128), (244, 214, 160))
PEBBLE = ((86, 82, 106), (126, 122, 144), (170, 166, 182), (214, 210, 218))

#: Fallen leaves, the commonest first: brown, olive, russet, ochre and the odd gold one.
FALLEN = ((150, 98, 62), (118, 118, 60), (140, 72, 52), (194, 138, 70), (220, 178, 96))

#: Little flowers: white, bluebell, a yellow middle, and pink.
BLOOM = ((246, 242, 232), (160, 184, 240), (250, 214, 110), (240, 150, 184))

SLATE = ((30, 30, 46), (46, 48, 66), (64, 68, 88), (84, 88, 108), (106, 110, 128), (132, 136, 152), (162, 164, 178))
GROUT = (22, 22, 34)
LICHEN = ((28, 58, 60), (40, 82, 74), (64, 108, 86), (96, 136, 100))
BONE = ((96, 88, 92), (150, 140, 136), (198, 188, 174), (232, 224, 208))
WEB = (214, 218, 240)

#: The outlines: round leaves, round wood and earth, and round stone. Dark, never black.
LEAF_INK = (16, 32, 36)
WOOD_INK = (46, 26, 32)
STONE_INK = (18, 18, 30)


def shade(c, k):
    """A colour made lighter (k over 1) or darker."""
    return tuple(clamp_byte(v * k) for v in c)


def lobed(dx, dy, r, lobes, phase, depth=0.2):
    """How far a point is from the middle of a round shape with a wavy edge, as a share of the
    edge's distance that way: under 1 is inside. The edge comes and goes `lobes` times round."""
    a = math.atan2(dy, dx)
    return math.hypot(dx, dy) / (r * (1.0 + depth * math.cos(lobes * a + phase)))


def clump(img, cx, cy, r, ramp, seed, lift=0, put_at=put):
    """One clump of leaves the way my trees sheet draws them: round, with a wavy edge, lit on its
    upper left with a highlight on top, and a dark rim on its lower right where it sits on the next
    one down. ramp is five shades from the rim up; lift shifts the clump a shade lighter."""
    phase = hash01(int(cx * 8), int(cy * 8), seed) * 6.28
    for y in range(int(cy - r - 2), int(cy + r + 2)):
        for x in range(int(cx - r - 2), int(cx + r + 2)):
            dx, dy = x + 0.5 - cx, y + 0.5 - cy
            d = lobed(dx, dy, r, 5, phase)
            if d > 1.0:
                continue
            light = -(dx * 0.7 + dy) / r
            if d > 0.78 and light < 0.05:
                k = 0
            elif light > 0.62:
                k = 4
            elif light > 0.22:
                k = 3
            elif light > -0.35:
                k = 2
            else:
                k = 1
            if hash01(x, y, seed + 3) > 0.9:
                k += 1 if hash01(y, x, seed + 4) > 0.5 else -1
            put_at(img, x, y, ramp[max(0, min(len(ramp) - 1, k + lift))])


def leaf(img, x, y, c, flat, under=None):
    """A fallen leaf, three pixels long, lit on its top left, lying flat or on a slant, with a
    pixel of shadow off its lower right if it's lying on something."""
    lit, dark = shade(c, 1.16), shade(c, 0.74)
    if flat:
        cells = ((0, 0, lit), (1, 0, c), (2, 0, c), (1, 1, dark), (2, 1, dark))
        cast = (3, 1)
    else:
        cells = ((0, 0, lit), (1, 0, c), (1, 1, c), (2, 1, dark), (2, 2, dark))
        cast = (3, 2)
    if under is not None:
        put(img, x + cast[0], y + cast[1], under)
    for dx, dy, col in cells:
        put(img, x + dx, y + dy, col)


def pebble(c, x, y, w, h, ramp=PEBBLE):
    """A small stone on a canvas, w by h, lit on its top left and lavender underneath."""
    for yy in range(h):
        for xx in range(w):
            if (xx in (0, w - 1)) and (yy in (0, h - 1)) and w > 2 and h > 2:
                continue
            u, v = (xx + 0.5) / w - 0.5, (yy + 0.5) / h - 0.5
            k = 3 if u + v < -0.3 else 2 if u + v < 0.2 else 1 if u + v < 0.6 else 0
            c.put(x + xx, y + yy, ramp[k])


# --------------------------------------------------------------------------- #
# Tiles/forest.png -- the old road
# --------------------------------------------------------------------------- #
# The ground is grass, fallen leaves under the trees, the road, and undergrowth too thick to get
# through. Each variant of a tile has the same edges as the others and only its middle changes, the
# same as Questeria's ground, so a field of them has no seams. The leaves, the road and the
# undergrowth are edged: where the leaves meet anything else the grass reaches over them, the road
# has a verge of grass with a lip of shadow or light under it, and the undergrowth has a lit top
# and a shadowed face. Everything else lying about (moss, tufts, ferns, flowers, pebbles, roots,
# the road's ruts and scattered leaves) is detail, drawn over the ground and kept off the edges of
# its tile, apart from the ruts, which run on into the next one. The trees are props.

def tuft_at(img, x, y):
    """A tuft of grass in the lawn: a few blades lit at their tips, dark at their root."""
    put(img, x, y - 1, GRASS[5])
    put(img, x - 1, y, GRASS[4])
    put(img, x + 1, y, GRASS[4])
    put(img, x, y, GRASS[3])
    put(img, x - 2, y + 1, GRASS[3])
    put(img, x - 1, y + 1, GRASS[2])
    put(img, x, y + 1, GRASS[3])
    put(img, x + 1, y + 1, GRASS[2])
    put(img, x + 2, y + 1, GRASS[1])
    put(img, x - 1, y + 2, GRASS[0])
    put(img, x, y + 2, GRASS[1])
    put(img, x + 1, y + 2, GRASS[0])


def grass(variant):
    """The forest's grass: soft light and dark across it like light through leaves, a blade here
    and there, a couple of tufts, and on some a fallen leaf or a flower. The light and the blades
    are the same in every variant; the tufts, leaves and flowers are each one's own, off its edges."""
    img = tile()
    light = blobs(171, 6, 5.0)
    for y in range(T):
        for x in range(T):
            m = light[y][x]
            c = 2
            if m > 0.25 and dither(x, y, (m - 0.25) * 1.5):
                c = 3
            elif m < -0.45 and dither(x, y, (-0.45 - m) * 1.3):
                c = 1
            n = hash01(x, y, 172)
            if n < 0.05:
                c = 1
            elif n > 0.96:
                c = 3
            put(img, x, y, GRASS[c])
    for y in range(T):
        for x in range(T):
            if hash01(x, y, 173) < 0.07:
                put(img, x, y, GRASS[4] if hash01(y, x, 174) < 0.4 else GRASS[3])
                put(img, x, y + 1, GRASS[1])
    for i in range(2):
        tuft_at(img, 3 + int(hash01(i, variant, 175) * 10), 3 + int(hash01(variant, i, 176) * 9))
    if variant in (1, 3):
        x, y = 4 + int(hash01(variant, 7, 177) * 7), 4 + int(hash01(7, variant, 178) * 7)
        leaf(img, x, y, FALLEN[variant], variant == 1, GRASS[0])
    if variant == 2:
        fx, fy = 5 + int(hash01(2, 9, 179) * 6), 5 + int(hash01(9, 2, 179) * 6)
        for dx, dy in ((0, -1), (-1, 0), (1, 0), (0, 1)):
            put(img, fx + dx, fy + dy, BLOOM[0])
        put(img, fx, fy, BLOOM[2])
        put(img, fx + 1, fy + 1, GRASS[0])
    return img


def fallen(n):
    """Which of the fallen leaves a 0..1 number picks: mostly brown and olive, now and then gold."""
    return FALLEN[min(len(FALLEN) - 1, int(n * n * len(FALLEN)))]


def leaves(variant):
    """The forest floor under the trees: dark earth and moss with fallen leaves lying on it, each
    lit on its top left with its shadow off its lower right, and blades of grass coming up through.
    The leaves crossing the edges are the same in every variant."""
    img = tile()
    soil = blobs(181, 5, 5.0)
    for y in range(T):
        for x in range(T):
            m = soil[y][x]
            c = GRASS[1] if m > 0.15 and dither(x, y, m * 1.4) else EARTH[1]
            if hash01(x, y, 182) < 0.08:
                c = EARTH[0]
            put(img, x, y, c)
    for i in range(20):
        x, y = int(hash01(i, 1, 183) * T), int(hash01(1, i, 184) * T)
        leaf(img, x, y, fallen(hash01(i, 2, 185)), hash01(i, 3, 186) < 0.5, EARTH[0])
    for i in range(3 + variant):
        x, y = 3 + int(hash01(i, variant, 187) * 9), 3 + int(hash01(variant, i, 188) * 9)
        leaf(img, x, y, fallen(hash01(i, variant + 5, 189)), hash01(variant, i + 5, 190) < 0.5, EARTH[0])
    for y in range(T):
        for x in range(T):
            if hash01(x, y, 191) < 0.04:
                put(img, x, y, GRASS[3])
                put(img, x, y + 1, GRASS[1])
    if variant == 1:
        # A twig.
        for i in range(7):
            put(img, 4 + i, 9 - i // 3, BARK[3] if i % 3 else BARK[4])
            put(img, 4 + i, 10 - i // 3, BARK[1])
    return img


def fringe(img, side_seed):
    """Grass reaching over the edge of a tile from its north side, ragged, with a darker lip and
    the odd blade further in. Returns how far it reaches down each column."""
    reach = []
    for x in range(T):
        r = 1 + int(1.6 + 1.3 * math.sin(x * math.pi / 4 + side_seed) + hash01(x, 0, 192 + side_seed) * 1.6)
        for y in range(r):
            put(img, x, y, GRASS[3] if y == 0 and hash01(x, 1, 193) < 0.5 else GRASS[2])
        put(img, x, r, GRASS[1])
        if hash01(x, 2, 194 + side_seed) < 0.3:
            put(img, x, r, GRASS[3])
            put(img, x, r + 1, GRASS[1])
            r += 1
        reach.append(r)
    return reach


def leaf_edges():
    """Where fallen leaves meet anything else, the grass grows over their edge."""
    edges = []
    for side in range(4):
        img = tile()
        fringe(img, side)
        edges.append(turned(img, side))
    return edges


def road(variant):
    """Packed earth, worn paler in patches, with grit and the odd pebble worked up through it. The
    grit's the same in every variant; a stone or two in the middle is each one's own."""
    img = tile()
    light = blobs(201, 6, 4.5, squash=1.8)
    for y in range(T):
        for x in range(T):
            m = light[y][x]
            c = 3
            if m > 0.3 and dither(x, y, (m - 0.3) * 1.4):
                c = 4
            elif m < -0.4 and dither(x, y, (-0.4 - m) * 1.4):
                c = 2
            n = hash01(x, y, 202)
            if n < 0.06:
                c = 2
            elif n > 0.97:
                c = 5
            put(img, x, y, EARTH[c])
    for y in range(T):
        for x in range(T):
            if hash01(x, y, 203) < 0.015:
                put(img, x, y, PEBBLE[2])
                put(img, x + 1, y + 1, EARTH[1])
    c = Canvas(T, T)
    for i in range(variant):
        x, y = 3 + int(hash01(i, variant, 204) * 9), 3 + int(hash01(variant, i, 205) * 9)
        pebble(c, x, y, 3 if i else 4, 2 if i else 3)
    c.outline(EARTH[1])
    for y in range(T):
        for x in range(T):
            if c.img[y][x][3]:
                img[y][x] = list(c.img[y][x])
    return img


def road_edges():
    """The road's verge: grass reaching over its edge, and under that a lip of shadow on the north
    and west sides, where the verge stands between the road and the light, and of light on the
    south and east, where the road rises to meet it."""
    edges = []
    for side in range(4):
        img = tile()
        reach = fringe(img, side + 4)
        under = EARTH[1] if side in (0, 3) else EARTH[4]
        for x, r in enumerate(reach):
            put(img, x, r + 1, under)
            if side in (0, 3) and hash01(x, side, 206) < 0.5:
                put(img, x, r + 2, EARTH[2])
        edges.append(turned(img, side))
    return edges


def undergrowth(variant):
    """Undergrowth too thick to get through: clumps of dark leaves heaped on each other, the lower
    one in front, with the dark showing between them. The clumps are on a grid shaken well about,
    so they don't line up in rows, the same in every variant and wrapping round the tile; the ones
    in the middle are each variant's own."""
    img = tile()
    for y in range(T):
        for x in range(T):
            put(img, x, y, UNDERGROWTH[0])
    clumps = []
    for i in range(5):
        drop = hash01(i, 9, 210) * 3.2
        for j in range(4):
            own = 1 <= i <= 3 and 1 <= j <= 2
            seed = 213 + (variant * 17 if own else 0)
            cx = i * 3.2 + (hash01(i, j, seed) - 0.5) * 2.2
            cy = j * 4 + drop + (hash01(j, i, seed + 1) - 0.5) * 2.4
            r = 2.4 + hash01(i + 4, j, seed + 2) * 1.3
            lift = 1 if hash01(i, j, seed + 3) > 0.72 else 0
            clumps.append((cy, cx, r, lift, seed))
    for cy, cx, r, lift, seed in sorted(clumps):
        clump(img, cx, cy, r, UNDERGROWTH, seed, lift)
    return img


def undergrowth_edges():
    """Where the undergrowth meets open ground. Along the north it's the top of it, a row of clumps
    a shade lighter with a dark line along their tops; along the south it's the face of it, dark
    stems in the shadow under the last clumps; along the sides, a dark line, lit on the west."""
    north = tile()
    for x in range(T):
        put(north, x, 0, LEAF_INK)
    for i in range(4):
        clump(north, i * 4 + 2.0, 3.6, 3.0, UNDERGROWTH[2:], 221 + i, 0, put_inside)
    south = tile()
    for y in range(11, T):
        for x in range(T):
            stem = hash01(x, 0, 222) < 0.3
            put(south, x, y, BARK[1] if stem and y < T - 1 else UNDERGROWTH[0])
    for i in range(4):
        clump(south, i * 4 + 1.0, 10.2, 2.8, UNDERGROWTH[:5], 223 + i, 0, put_inside)
    for x in range(T):
        put(south, x, T - 1, LEAF_INK)
    east, west = tile(), tile()
    for y in range(T):
        put(east, T - 1, y, LEAF_INK)
        put(east, T - 2, y, UNDERGROWTH[0])
        put(west, 0, y, LEAF_INK)
        if hash01(0, y, 224) < 0.6:
            put(west, 1, y, UNDERGROWTH[4])
    return [north, east, south, west]


def put_inside(img, x, y, c):
    """put, but only inside the tile: for pieces that mustn't wrap round to the other side."""
    if 0 <= x < T and 0 <= y < T:
        put(img, x, y, c)


def detail(paint):
    """A detail tile: whatever paint draws on a clear canvas, outlined in the dark of the ground."""
    def build(variant):
        c = Canvas(T, T)
        ink = paint(c, variant)
        c.outline(ink)
        return c.img
    return build


def paint_moss(c, variant):
    """Cushions of moss, lumpy round the edge, lit on top, bright where it's fruiting."""
    cushions = (((6.0, 9.0, 4.4), (11.5, 5.0, 2.8)), ((5.0, 5.0, 3.0), (10.5, 9.0, 3.8), (4.5, 12.0, 2.4)))[variant]
    for i, (cx, cy, r) in enumerate(cushions):
        for y in range(T):
            for x in range(T):
                dx, dy = x + 0.5 - cx, (y + 0.5 - cy) * 1.25
                d = lobed(dx, dy, r, 4, i + variant, 0.14)
                if d > 1.0:
                    continue
                light = -(dx * 0.7 + dy) / r
                k = 0 if d > 0.8 and light < 0 else 3 if light > 0.5 else 2 if light > 0.0 else 1
                if light > 0.0 and hash01(x, y, 231 + i) > 0.9:
                    k = 4
                c.put(x, y, MOSS[k])
    return GRASS[0]


def paint_tuft(c, variant):
    """A clump of long grass: thin blades fanning out of one root and curling outward as they go,
    the ones leaning left lit at their tips."""
    rng = random.Random(241 + variant)
    root = (7.5 + variant, 14.5)
    for i in sorted(range(-4, 5), key=abs, reverse=True):
        angle = i * 0.3 + rng.uniform(-0.08, 0.08)
        length = rng.uniform(7.5, 10.0) - abs(i) * 0.7
        for j in range(int(length)):
            t = j / length
            a = angle * (1.0 + 0.9 * t)
            x = root[0] + math.sin(a) * j
            y = root[1] - math.cos(a) * j
            k = 1 if t < 0.25 else 2 if t < 0.5 else 3 if t < 0.8 or i > 0 else 4
            c.put(x, y, GRASS[k])
    return GRASS[0]


def paint_fern(c, variant):
    """A fern: fronds arching up and out of one root with leaflets down both sides of each,
    shorter toward the tip, and the tips bending back down."""
    root = (8.0 + (variant - 0.5), 14.5)
    leans = (-1.15, -0.55, 0.05, 0.6, 1.1) if variant == 0 else (-1.0, -0.4, 0.2, 0.75)
    for n, lean in sorted(enumerate(leans), key=lambda f: -abs(f[1])):
        length = 10.0 - abs(lean) * 2.4
        for j in range(1, int(length) + 1):
            t = j / length
            x = root[0] + math.sin(lean) * j * 1.05
            y = root[1] - math.cos(lean) * j * 1.05 + t * t * (2.5 + abs(lean) * 3)
            leaflet = max(0, int(round(2.4 * (1.0 - t))))
            for side in (-1, 1):
                for k in range(leaflet + 1):
                    xx = int(math.floor(x + side * k * math.cos(lean)))
                    yy = int(math.floor(y + side * k * math.sin(lean) * 0.5 + k * 0.4))
                    lit = side < 0 or lean < -0.3
                    c.put(xx, yy, MOSS[4] if k == leaflet and lit else MOSS[3] if lit else MOSS[2] if k else MOSS[3])
    return GRASS[0]


def paint_flowers(c, variant):
    """Wild flowers on a few leaves: white wood anemones, or bluebells nodding on their stems."""
    for cx, cy in ((5, 9), (10, 11), (8, 6)):
        for dx, dy, k in ((0, 0, 3), (1, 0, 2), (0, 1, 2), (1, 1, 1), (-1, 1, 3)):
            c.put(cx + dx, cy + dy + 2, GRASS[k])
    if variant == 0:
        for fx, fy in ((5, 8), (10, 10), (8, 5), (12, 6)):
            for dx, dy in ((0, -1), (-1, 0), (1, 0), (0, 1)):
                c.put(fx + dx, fy + dy, BLOOM[0] if dx <= 0 and dy <= 0 else shade(BLOOM[0], 0.82))
            c.put(fx, fy, BLOOM[2])
    else:
        for fx, fy in ((4, 6), (7, 4), (10, 7), (12, 10)):
            for i in range(4):
                c.put(fx + i // 2, fy + i, GRASS[2])
            for dx, dy in ((2, 3), (2, 4), (1, 4)):
                c.put(fx + dx, fy + dy, BLOOM[1])
            c.put(fx + 2, fy + 5, shade(BLOOM[1], 0.7))
    return GRASS[0]


def paint_pebbles(c, variant):
    """A few small stones, lit on their tops, with a little grit about them."""
    stones = (((3, 4, 4, 3), (9, 6, 3, 2), (6, 10, 3, 3), (11, 11, 2, 2)),
              ((4, 7, 3, 2), (8, 4, 4, 3), (10, 10, 3, 3), (4, 11, 2, 2), (13, 6, 2, 2)))[variant]
    for x, y, w, h in stones:
        pebble(c, x, y, w, h)
    return EARTH[1]


def paint_roots(c, variant):
    """Roots running over the ground from a tree nearby, bending as they go, lit along their tops,
    tapering away into the earth. Each is a curve from where it comes up, past a bend, to its tip."""
    runs = (((1, 7), (7, 4), (14, 6), 2.0), ((2, 12), (8, 10), (13, 13), 1.6), ((7, 5), (9, 8), (8, 11), 1.2)),         (((1, 4), (6, 7), (14, 8), 2.0), ((3, 12), (6, 10), (12, 12), 1.5), ((9, 7), (11, 4), (13, 3), 1.2))
    for (x0, y0), (bx, by), (x1, y1), thick in runs[variant]:
        steps = 40
        for i in range(steps + 1):
            t = i / steps
            x = (1 - t) ** 2 * x0 + 2 * (1 - t) * t * bx + t * t * x1
            y = (1 - t) ** 2 * y0 + 2 * (1 - t) * t * by + t * t * y1
            w = thick * (1.0 - 0.7 * t)
            c.put(x, y, BARK[4] if w > 1.2 else BARK[3])
            if w > 1.0:
                c.put(x, y + 1, BARK[2] if w > 1.5 else BARK[1])
    return WOOD_INK


def paint_ruts(c, variant):
    """Wheel ruts along the road, running on into the next tile: the north side of each groove in
    shadow, the south catching the light. One has a puddle standing in it."""
    for x in range(T):
        y = 7 + round(0.8 * math.sin(x * math.pi / 8))
        c.put(x, y - 1, EARTH[2])
        c.put(x, y, EARTH[1])
        c.put(x, y + 1, EARTH[2])
        c.put(x, y + 2, EARTH[4])
        if hash01(x, variant, 251) < 0.25:
            c.put(x, y + 3, EARTH[4])
    if variant == 1:
        for x in range(5, 11):
            y = 7 + round(0.8 * math.sin(x * math.pi / 8))
            c.put(x, y, (56, 70, 100))
            c.put(x, y + 1, (88, 108, 140) if x % 3 else (170, 190, 220))


def paint_litter(c, variant):
    """A scatter of fallen leaves lying on whatever's under them."""
    for i in range(7 + variant * 2):
        x, y = 2 + int(hash01(i, variant, 261) * 10), 2 + int(hash01(variant, i, 262) * 10)
        leaf(c.img, x, y, FALLEN[int(hash01(i, variant + 3, 263) * len(FALLEN))], hash01(i, 9, 264) < 0.5)


def detail_ruts(variant):
    """The ruts on their own: no outline, since they run off both ends of the tile."""
    c = Canvas(T, T)
    paint_ruts(c, variant)
    return c.img


def detail_litter(variant):
    """Scattered leaves, a pixel of shadow under each but no outline, since they're lying flat."""
    c = Canvas(T, T)
    paint_litter(c, variant)
    for y in range(T - 1, -1, -1):
        for x in range(T - 1, -1, -1):
            if c.img[y][x][3] and x + 1 < T and y + 1 < T and not c.img[y + 1][x + 1][3]:
                c.img[y + 1][x + 1] = [0, 0, 0, 70]
    return c.img


#: Each tile: its name, its variants, and its edges if it has them. TilesetCatalog.Forest has the
#: same list in the same order, with the same variants.
FOREST_TILES = (
    ("grass", [grass(v) for v in range(4)], None),
    ("leaves", [leaves(v) for v in range(4)], leaf_edges()),
    ("road", [road(v) for v in range(3)], road_edges()),
    ("undergrowth", [undergrowth(v) for v in range(2)], undergrowth_edges()),
    ("moss", [detail(paint_moss)(v) for v in range(2)], None),
    ("tuft", [detail(paint_tuft)(v) for v in range(2)], None),
    ("fern", [detail(paint_fern)(v) for v in range(2)], None),
    ("flowers", [detail(paint_flowers)(v) for v in range(2)], None),
    ("pebbles", [detail(paint_pebbles)(v) for v in range(2)], None),
    ("roots", [detail(paint_roots)(v) for v in range(2)], None),
    ("ruts", [detail_ruts(v) for v in range(2)], None),
    ("litter", [detail_litter(v) for v in range(2)], None),
)


# --------------------------------------------------------------------------- #
# The forest's trees and undergrowth, cut off my trees sheet
# --------------------------------------------------------------------------- #
# The same cutter as Questeria's trees (cut_supplied.py's cut_out), so they come out in the same
# style. Some are turned a deeper, bluer green for the back of the wood, so it reads as deep: each
# green pixel's hue is pulled toward a teal and it's darkened, keeping where it sits among the
# tree's own shades. The bark, the flowers and the outline aren't green, so they stay as they were.

TREES_SHEET = "questeria-trees.png"

#: The green a deepened tree's leaves are pulled toward (hue in degrees), how far, and how much of
#: their colour and their brightness they keep.
DEEP_HUE = 176
DEEP_PULL = 0.5
DEEP_SATURATION = 0.8
DEEP_KEPT = 0.76

#: What counts as a leaf to deepen: a hue between these, and at least this saturated.
LEAF_HUES = (55, 200)
LEAF_SATURATION = 0.18

#: The trees and undergrowth: the name (Props/<name>.png, and what PropCatalog and the map call
#: it), the box round it on my trees sheet, how many sheet pixels an art pixel takes, and whether
#: it's deepened. The oak is cut by cut_supplied.py for Questeria; the deep one is it again, a
#: little smaller. The cypresses make the firs.
FOREST_CUTS = (
    ("oak-deep", (598, 12, 180, 228), 2.0, True),
    ("oak-light", (767, 17, 170, 222), 2.0, False),
    ("oak-pale", (929, 10, 194, 232), 2.0, False),
    ("oak-amber", (1344, 10, 179, 230), 2.0, False),
    ("fir", (557, 670, 60, 160), 2.0, False),
    ("fir-deep", (777, 670, 49, 142), 1.8, True),
    ("stump", (840, 562, 117, 110), 3.0, False),
    ("snag", (1031, 574, 48, 98), 2.6, False),
    ("dead-tree", (1454, 575, 67, 105), 2.2, False),
    ("bush", (14, 675, 82, 78), 2.8, False),
    ("bush-deep", (221, 675, 96, 85), 2.8, True),
    ("bush-small", (107, 772, 54, 54), 2.8, False),
    ("fern", (331, 836, 59, 71), 3.0, False),
    ("grass-tall", (711, 862, 51, 56), 3.0, False),
    ("rocks-mossy", (227, 851, 95, 69), 3.0, False),
)


def deepen(img):
    """A copy of a cut with its leaves a deeper, bluer green."""
    out = [[list(px) for px in line] for line in img]
    for line in out:
        for x, px in enumerate(line):
            if not px[3]:
                continue
            h, s, v = colorsys.rgb_to_hsv(px[0] / 255, px[1] / 255, px[2] / 255)
            h *= 360
            if not (LEAF_HUES[0] <= h <= LEAF_HUES[1] and s >= LEAF_SATURATION):
                continue
            h = (h + (DEEP_HUE - h) * DEEP_PULL) / 360
            line[x] = [round(c * 255) for c in colorsys.hsv_to_rgb(h, s * DEEP_SATURATION, v * DEEP_KEPT)] + [255]
    return out


def forest_cuts():
    """Every tree and bit of undergrowth, by name."""
    cuts = {}
    for name, box, pitch, deep in FOREST_CUTS:
        img = cut_out(TREES_SHEET, box, pitch=pitch)
        cuts[name] = deepen(img) if deep else img
    return cuts


# --------------------------------------------------------------------------- #
# The forest's props drawn here: a fallen log, the tree down across the road, and mushrooms
# --------------------------------------------------------------------------- #
# My trees sheet has no logs or mushrooms on it, so these are drawn to match it: bark in its
# browns with the furrows running along the wood, moss on top, rings on a cut end, and the same
# dark outline round everything.

def bark_at(u, n):
    """The shade of bark across a round trunk or log, lit from the top left: u is 0 on the lit side
    and 1 on the dark side, and n a 0..1 number that cuts a furrow in now and then."""
    k = 4 if u < 0.2 else 3 if u < 0.45 else 2 if u < 0.72 else 1
    if n < 0.2:
        k -= 1
    elif n > 0.93:
        k += 1
    return BARK[max(0, min(len(BARK) - 1, k))]


def mossy(c, x, y, n):
    """A pixel of moss on top of something, a shade lighter where it's highest."""
    c.put(x, y, MOSS[3] if n > 0.6 else MOSS[2] if n > 0.25 else MOSS[1])


def rings(c, cx, cy, rx, ry, seed):
    """The cut end of a log: its rings round a dark heart, and its bark round the edge."""
    for y in range(int(cy - ry) - 1, int(cy + ry) + 2):
        for x in range(int(cx - rx) - 1, int(cx + rx) + 2):
            d = math.hypot((x + 0.5 - cx) / rx, (y + 0.5 - cy) / ry)
            if d >= 1.0:
                continue
            if d > 0.82:
                col = BARK[2]
            elif d < 0.18:
                col = HEARTWOOD[0]
            else:
                col = HEARTWOOD[2 if int(d * 6) % 2 else 1]
                if (x + 0.5 - cx) + (y + 0.5 - cy) < -1.5 and int(d * 6) % 2:
                    col = HEARTWOOD[3]
            if hash01(x, y, seed) < 0.06:
                col = HEARTWOOD[0]
            c.put(x, y, col)


LOG_W, LOG_H = 46, 18


def log():
    """A tree that came down long ago, lying along the ground: furrowed bark lit along its top, its
    sawn end showing its rings on the left, the broken end on the right, moss along its back, and a
    shelf of fungus on its side."""
    c = Canvas(LOG_W, LOG_H)
    top, bottom = 2, LOG_H - 2
    for y in range(top, bottom + 1):
        end = LOG_W - 3 - int(hash01(y, 0, 301) * 4)
        for x in range(5, end):
            u = (y - top) / (bottom - top)
            c.put(x, y, bark_at(u, hash01(x // 5, y, 302)))
        for x in range(end, end + 2):
            if hash01(x, y, 303) < 0.6:
                c.put(x, y, HEARTWOOD[2] if y < (top + bottom) // 2 else HEARTWOOD[1])
    for x in range(12, 34):
        for y in range(top, top + 3):
            n = hash01(x, y, 304)
            if n < 0.85 - 0.25 * (y - top) and math.sin(x * 0.45) > -0.4:
                mossy(c, x, y, n)
    rings(c, 5.5, (top + bottom) / 2 + 0.5, 3.4, (bottom - top) / 2 + 0.6, 305)
    for fx, fy, r in ((30, 11, 3.0), (34, 13, 2.2)):
        for y in range(int(fy - r), int(fy) + 1):
            for x in range(int(fx - r), int(fx + r) + 1):
                if math.hypot(x + 0.5 - fx, (y + 0.5 - fy) * 1.6) < r:
                    c.put(x, y, HEARTWOOD[3] if y < fy - r * 0.5 else HEARTWOOD[2] if x < fx else HEARTWOOD[1])
    c.outline(WOOD_INK)
    return c.img


FALLEN_W, FALLEN_H = 86, 76

#: The fallen tree's trunk: where its foot is (in the middle of the plate of roots), where its top
#: broke off, and how far it is from its middle to its bark. And the plate of roots and earth it
#: tore up when it went over: its middle and how far it reaches across and down.
FALLEN_BASE = (18.0, 56.0)
FALLEN_TOP = (66.0, 9.0)
FALLEN_RADIUS = 6.5
FALLEN_ROOTS = (17.0, 56.0, 14.5, 18.0)


def fallen_tree():
    """A tree down across the road, from the near verge to the far one: the plate of roots and
    earth it tore up when it fell stands on end at its foot, its trunk runs away up to the right,
    and its crown is a tangle of broken branches with a few dead leaves still on them."""
    c = Canvas(FALLEN_W, FALLEN_H)
    (bx, by), (tx, ty), r = FALLEN_BASE, FALLEN_TOP, FALLEN_RADIUS
    length = math.hypot(tx - bx, ty - by)
    ax, ay = (tx - bx) / length, (ty - by) / length
    # The crown: broken branches out of the top of the trunk, lying every way, thick to thin.
    for ex, ey, w in ((84, 2, 3), (83, 20, 3), (58, 0, 2), (85, 10, 2), (76, 28, 2), (50, 3, 1), (75, 0, 1)):
        steps = int(math.hypot(ex - tx, ey - ty)) * 2
        for i in range(steps + 1):
            t = i / steps
            x, y = tx + (ex - tx) * t, ty + (ey - ty) * t
            for k in range(max(1, round(w * (1.0 - 0.6 * t)))):
                c.put(x, y + k, BARK[4] if k == 0 else BARK[2])
    for lx, ly in ((82, 1), (81, 19), (58, 1), (84, 9), (75, 26), (51, 4), (70, 18), (78, 5)):
        colour = fallen(hash01(lx, ly, 311))
        for dx, dy in ((0, 0), (1, 0), (0, 1), (-1, 1), (1, 1)):
            c.put(lx + dx, ly + dy, shade(colour, 1.15) if dy == 0 else colour)
    # The trunk, lit along its upper side, furrowed along its length, mossy along its back.
    for y in range(FALLEN_H):
        for x in range(FALLEN_W):
            px, py = x + 0.5 - bx, y + 0.5 - by
            along = px * ax + py * ay
            across = -px * ay + py * ax
            if not (0 <= along <= length and abs(across) <= r):
                continue
            u = (across + r) / (2 * r)
            if along > length - 2.5 and hash01(x, y, 314) < 0.6:
                c.put(x, y, HEARTWOOD[2] if u < 0.5 else HEARTWOOD[1])
                continue
            c.put(x, y, bark_at(u, hash01(int(along / 3), int(across + r), 312)))
            n = hash01(x, y, 313)
            if 0.12 < u < 0.42 and n < 0.45 + 0.4 * math.sin(along * 0.19):
                mossy(c, x, y, n)
    # The plate of roots: earth packed round roots that fan out from the middle, stones caught in
    # it, the turf still on its top edge, and roots hanging off its bottom.
    cx, cy, rx, ry = FALLEN_ROOTS
    for y in range(int(cy - ry), int(cy + ry) + 1):
        for x in range(int(cx - rx), int(cx + rx) + 1):
            dx, dy = x + 0.5 - cx, y + 0.5 - cy
            d = lobed(dx / rx, dy / ry, 1.0, 7, 0.6, 0.08)
            if d > 1.0:
                continue
            a = math.atan2(dy, dx)
            root = abs(math.sin(a * 6 + d * 2.0)) < 0.22 and d > 0.25
            if d > 0.84 and dy < -ry * 0.3:
                col = GRASS[3] if hash01(x, y, 315) < 0.5 else GRASS[2]
            elif root:
                col = BARK[4] if dx + dy < 0 else BARK[3]
            else:
                light = -(dx / rx * 0.6 + dy / ry)
                col = EARTH[3] if light > 0.5 else EARTH[2] if light > -0.2 else EARTH[1]
                if hash01(x, y, 316) < 0.08:
                    col = EARTH[0]
            c.put(x, y, col)
    for sx, sy, w, h in ((9, 51, 3, 3), (20, 61, 4, 3), (13, 66, 3, 2), (22, 49, 2, 2)):
        pebble(c, sx, sy, w, h)
    for x in range(int(cx - rx * 0.7), int(cx + rx * 0.7), 3):
        drop = 2 + int(hash01(x, 0, 317) * 4)
        base = int(cy + ry * math.sqrt(max(0.0, 1 - ((x + 0.5 - cx) / rx) ** 2))) - 1
        for i in range(drop):
            c.put(x + (i // 2) * (1 if x < cx else -1), base + i, BARK[3] if i == 0 else BARK[2])
    c.outline(WOOD_INK)
    return c.img


MUSHROOMS_W, MUSHROOMS_H = 18, 12

#: Toadstools' caps, dark to light, the spots on the red ones, and their stalks.
RED_CAPS = ((96, 28, 40), (156, 44, 52), (206, 74, 68), (238, 128, 108))
PALE_CAPS = ((70, 76, 128), (108, 126, 186), (156, 186, 232), (212, 230, 250))
SPOTS = (246, 240, 230)
STALKS = ((150, 136, 128), (204, 192, 172), (236, 228, 210))

#: The toadstools in a cluster, back to front: the middle of each cap's rim, its half-width, and
#: how tall its stalk is.
CLUSTER = ((6.0, 4.0, 3.6, 5), (12.0, 6.0, 2.6, 4), (15.4, 8.6, 1.7, 2), (2.6, 8.4, 1.6, 2))


def mushrooms(caps, spotted):
    """A cluster of toadstools: domed caps lit on their top left over a dark band of gills, on pale
    stalks, the big one behind and the little ones in front."""
    c = Canvas(MUSHROOMS_W, MUSHROOMS_H)
    for mx, my, r, stalk in CLUSTER:
        for y in range(int(my), int(my) + stalk + 1):
            for x in range(int(mx - 0.8), int(mx + 0.9)):
                c.put(x, y, STALKS[2] if x < mx - 0.3 else STALKS[1] if y < my + stalk else STALKS[0])
        for y in range(int(my - r * 0.9), int(my) + 1):
            for x in range(int(mx - r), int(mx + r) + 1):
                dx, dy = x + 0.5 - mx, (y + 0.5 - my) / 0.9
                if math.hypot(dx, dy) > r or dy > 0.2:
                    continue
                light = -(dx * 0.7 + dy) / r
                col = caps[3] if light > 0.75 else caps[2] if light > 0.2 else caps[1] if light > -0.4 else caps[0]
                if y == int(my):
                    col = caps[0]
                elif spotted and hash01(x, y, 321) < 0.16 and abs(dx) < r - 0.8:
                    col = SPOTS
                c.put(x, y, col)
    c.outline(WOOD_INK)
    return c.img


def forest_drawn():
    """The forest's drawn props, by name."""
    return {
        "log": log(),
        "fallen-tree": fallen_tree(),
        "mushrooms": mushrooms(RED_CAPS, True),
        "mushrooms-pale": mushrooms(PALE_CAPS, False),
    }


# --------------------------------------------------------------------------- #
# The road's camp, its sign and its waypoint stone
# --------------------------------------------------------------------------- #
# The campfire and the waypoint stone keep the size they always were, since the game puts the
# fire's flame and the stone's footprint where those were.

CAMPFIRE_W, CAMPFIRE_H = 22, 12

#: The coals, dark to hot.
EMBERS = ((96, 34, 34), (168, 62, 36), (226, 112, 44), (250, 172, 72), (255, 226, 150))

#: The ring of stones round the fire: the middle of the ring, and how far it reaches each way.
FIRE_RING = (11.0, 7.4, 8.6, 3.6)


def campfire():
    """A ring of stones round two logs burning across each other, the coals glowing between and
    ash round the edge. The game draws the flame on it."""
    c = Canvas(CAMPFIRE_W, CAMPFIRE_H)
    cx, cy, rx, ry = FIRE_RING
    stones = []
    for i in range(10):
        a = i / 10.0 * 2.0 * math.pi + 0.2
        stones.append((cy + math.sin(a) * ry, cx + math.cos(a) * rx, 3 + i % 2, 2 + (i % 3 == 0)))
    behind = [s for s in stones if s[0] <= cy]
    front = [s for s in stones if s[0] > cy]
    for sy, sx, w, h in sorted(behind):
        pebble(c, round(sx - w / 2), round(sy - h / 2), w, h)
    for y in range(CAMPFIRE_H):
        for x in range(CAMPFIRE_W):
            d = math.hypot((x + 0.5 - cx) / (rx - 2.0), (y + 0.5 - cy - 0.4) / (ry - 0.8))
            if d < 1.0:
                heat = 1.0 - d + (hash01(x, y, 331) - 0.5) * 0.5
                c.put(x, y, EMBERS[max(0, min(4, int(heat * 4.5)))] if d < 0.8 else (92, 86, 92))
    for (x0, y0), (x1, y1) in (((5.5, 9.0), (15.5, 5.5)), ((6.5, 5.0), (16.5, 9.0))):
        steps = 24
        for i in range(steps + 1):
            t = i / steps
            x, y = x0 + (x1 - x0) * t, y0 + (y1 - y0) * t
            burnt = abs(t - 0.5) < 0.18
            c.put(x, y - 1, EMBERS[3] if burnt else BARK[4])
            c.put(x, y, EMBERS[2] if burnt else BARK[2])
        c.put(x1, y1 - 1, HEARTWOOD[2])
    for sy, sx, w, h in sorted(front):
        pebble(c, round(sx - w / 2), round(sy - h / 2), w, h)
    c.outline(WOOD_INK)
    return c.img


BEDROLL_W, BEDROLL_H = 24, 11

#: A woollen blanket, dark to light, its stripes, and the canvas of the pillow rolled up at its head.
BLANKET = ((84, 34, 48), (124, 48, 58), (162, 68, 70), (196, 100, 88))
STRIPE = ((150, 128, 116), (210, 190, 166), (238, 222, 196))
CANVAS = ((128, 112, 108), (184, 166, 154), (226, 208, 188), (244, 230, 210))


def bedroll():
    """A blanket laid out on the ground across the picture, seen from above at a slant, with its
    head end turned back over a rolled-up pillow, a stripe woven across it near each end, and the
    rumples in it catching the light."""
    c = Canvas(BEDROLL_W, BEDROLL_H)
    for y in range(1, BEDROLL_H):
        for x in range(BEDROLL_W):
            if x in (0, BEDROLL_W - 1) and y in (1, BEDROLL_H - 1):
                continue
            t = (y - 1) / (BEDROLL_H - 2)
            if x < 5:
                # The pillow, a roll lit along its top, with a strap round it.
                k = 3 if t < 0.2 else 2 if t < 0.55 else 1 if t < 0.85 else 0
                c.put(x, y, BARK[3] if x == 2 else CANVAS[k])
                continue
            k = 3 if t < 0.15 else 2 if t < 0.6 else 1 if t < 0.9 else 0
            if x < 8:
                k = min(3, k + (1 if x == 5 else 0))      # the turned-back end, lit along its fold
            if (x * 2 + y * 3) % 13 == 0 and 0.2 < t < 0.9:
                k = max(0, k - 1)                          # a rumple
            colour = BLANKET[k]
            if x in (10, 11, BEDROLL_W - 5, BEDROLL_W - 4):
                colour = STRIPE[min(2, k)]
            c.put(x, y, colour)
    for y in range(2, BEDROLL_H - 1):
        c.put(8, y, BLANKET[0])
    c.outline(WOOD_INK)
    return c.img


SIGN_W, SIGN_H = 20, 26

#: The boards' wood, dark to light, and the iron of their nails.
BOARD = ((96, 58, 44), (140, 90, 58), (184, 126, 78), (214, 160, 102), (236, 196, 138))
NAIL = (70, 68, 84)


def sign():
    """A fingerpost: a weathered post with a cap on it, a board pointing each way with words
    burnt into it, nailed on, and grass and moss round its foot."""
    c = Canvas(SIGN_W, SIGN_H)
    for y in range(3, SIGN_H):
        c.put(9, y, BARK[4])
        c.put(10, y, BARK[3] if y % 5 else BARK[2])
        c.put(11, y, BARK[1])
    for x in range(8, 13):
        c.put(x, 2, BARK[4] if x < 10 else BARK[3])
    c.put(9, 1, BARK[4])
    c.put(10, 1, BARK[3])
    for top, left in ((5, True), (12, False)):
        for y in range(top, top + 5):
            for x in range(1, SIGN_W - 1):
                tip = abs(y - (top + 2))
                if left and x < 1 + tip or not left and x > SIGN_W - 2 - tip:
                    continue
                k = 4 if y == top else 1 if y == top + 4 else 3 if y == top + 1 else 2
                if hash01(x // 4, y, 341) < 0.15:
                    k = max(1, k - 1)
                c.put(x, y, BOARD[k])
        for x in range(4, SIGN_W - 4):
            if hash01(x, top, 342) > 0.35 and x not in (9, 10, 11):
                c.put(x, top + 2, BOARD[0])
        c.put(9 if left else 10, top + 1, NAIL)
        c.put(9 if left else 10, top + 3, NAIL)
    for x, h in ((7, 2), (8, 3), (12, 2), (13, 1), (6, 1)):
        for i in range(h):
            c.put(x, SIGN_H - 1 - i, GRASS[3] if i == h - 1 else GRASS[2])
    c.put(10, SIGN_H - 4, MOSS[2])
    c.put(9, SIGN_H - 3, MOSS[3])
    c.outline(WOOD_INK)
    return c.img


WAYPOINT_W, WAYPOINT_H = 16, 30

#: The standing stone, dark to light: an old grey with lavender in its shadows.
STANDING = ((60, 58, 82), (88, 86, 112), (118, 116, 140), (150, 148, 170), (184, 182, 198), (214, 212, 224))

#: The mark cut in it, asleep and awake: the dark of the groove and the light on its lower lip,
#: then the glow, its bright middle, and the colour it throws on the stone round it.
RUNE_ASLEEP = ((36, 36, 56), (130, 128, 152))
RUNE_AWAKE = ((84, 196, 206), (210, 248, 246), (136, 232, 233))

#: The mark: a line down the middle with strokes off it, in stone pixels.
RUNE = ({(8, y) for y in range(9, 22)} | {(7, 11), (6, 12), (9, 14), (10, 15), (7, 17), (6, 18), (9, 19), (10, 20)})


def waypoint():
    """A standing stone with a mark cut in it: lumpy, wider at its foot, leaning a little, moss on
    its top and round its foot. The mark's dark while it's asleep and glows pale once it's found."""
    img = new_image(WAYPOINT_W * 2, WAYPOINT_H)
    for f in range(2):
        c = Canvas(WAYPOINT_W, WAYPOINT_H)
        for y in range(1, WAYPOINT_H):
            half = 5.0 + 1.6 * (y / WAYPOINT_H) - max(0.0, 4.0 - y) ** 2 * 0.28
            mid = 8 + 0.8 * (1.0 - y / WAYPOINT_H)
            for x in range(WAYPOINT_W):
                dx = x + 0.5 - mid
                if abs(dx) > half:
                    continue
                u = (dx + half) / (2 * half)
                k = 4 if u < 0.22 else 3 if u < 0.5 else 2 if u < 0.8 else 1
                if y < 4 and u < 0.7:
                    k += 1
                n = hash01(x, y, 351)
                if n < 0.1:
                    k -= 1
                elif n > 0.95:
                    k += 1
                colour = STANDING[max(0, min(5, k))]
                if (y > WAYPOINT_H - 5 and hash01(x, y, 352) > 0.45) or (y < 5 and u < 0.55 and hash01(x, y, 353) > 0.4):
                    colour = MOSS[3] if u < 0.4 else MOSS[2]
                c.put(x, y, colour)
        for x, y in ((10, 6), (11, 7), (11, 8), (12, 9)):
            c.put(x, y, STANDING[1])
        for x, y in RUNE:
            c.put(x, y, RUNE_AWAKE[1] if f else RUNE_ASLEEP[0])
            if (x + 1, y) not in RUNE and (x, y + 1) not in RUNE:
                c.put(x + 1, y + 1, RUNE_AWAKE[0] if f else RUNE_ASLEEP[1])
        c.outline(STONE_INK)
        if f:
            for y in range(WAYPOINT_H):
                for x in range(WAYPOINT_W):
                    if (x, y) in RUNE or not c.filled(x, y):
                        continue
                    near = min(math.hypot(x - rx, y - ry) for rx, ry in RUNE)
                    if near < 3.0:
                        c.glaze(x, y, RUNE_AWAKE[2], int(150 * (1.0 - near / 3.0)))
        paste(img, c.img, f * WAYPOINT_W, 0)
    return img


def road_drawn():
    """The road's camp, sign and stone, by name."""
    return {"campfire": campfire(), "bedroll": bedroll(), "sign": sign(), "waypoint": waypoint()}


# --------------------------------------------------------------------------- #
# The chest, the barrel, the crate and the urns, cut off my props sheet
# --------------------------------------------------------------------------- #

PROPS_SHEET = "questeria-props-2.png"

#: How many sheet pixels an art pixel takes for the chest's two pictures.
CHEST_PITCH = 3.4

#: The chest shut and open: a plain wooden one bound in iron, and the same chest with its lid up
#: and nothing left in it.
CHEST_BOXES = ((17, 102, 71, 74), (277, 96, 69, 89))

#: The rest: the name, the box round it, and how wide it comes out in art pixels.
PROPS_CUTS = (
    ("barrel", (158, 429, 56, 91), 17),
    ("crate", (21, 357, 67, 60), 20),
    ("urn", (680, 847, 49, 76), 15),
    ("urn-blue", (187, 884, 50, 83), 15),
)


def side_by_side(pictures):
    """Pictures in a strip, each in a cell as wide and tall as the biggest, standing on the middle
    of its bottom edge, the way a prop with more than one picture has them."""
    w, h = max(len(p[0]) for p in pictures), max(len(p) for p in pictures)
    strip = new_image(w * len(pictures), h)
    for i, p in enumerate(pictures):
        paste(strip, p, i * w + (w - len(p[0])) // 2, h - len(p))
    return strip


def props_cuts():
    """The chest, the barrel, the crate and the urns, by name."""
    cuts = {"chest": side_by_side([cut_out(PROPS_SHEET, box, pitch=CHEST_PITCH) for box in CHEST_BOXES])}
    for name, box, width in PROPS_CUTS:
        cuts[name] = cut_out(PROPS_SHEET, box, width=width)
    return cuts


# --------------------------------------------------------------------------- #
# Tiles/crypt.png -- the barrow
# --------------------------------------------------------------------------- #
# Worn flagstones underfoot, walls seen from above (their tops) and from the front (two rows of
# face), and what's lying about. The flagstones are slabs of every size, laid in blocks two tiles
# each way so they don't line up in rows; each block has its joint along its top and left, so any
# two meet. They're old: pitted, grimy, cracked, lichen in the joints. The worn floor and the floor
# in a wall's shadow are the same blocks worn pale or darkened, so they carry on into the floor
# round them. The face of a wall has a coping along its top, big blocks lit on their top and left,
# and here and there a burial niche or a carving. It's all a little darker than it would be
# outside, so even in a torch's light it's dim.

#: A few stones are tinted: lavender, or the green of damp.
STONE_TINTS = ((96, 86, 126), (70, 96, 98))


def stone_shade(base, tint, n):
    """One pixel of a stone: its shade, a pit or a glint now and then, and its tint if it has one."""
    k = base - 1 if n < 0.08 else base + 1 if n > 0.96 else base
    colour = SLATE[max(0, min(len(SLATE) - 1, k))]
    return lerp_color(colour, tint, 0.22) if tint else colour


#: The floor is laid in blocks two tiles each way, so the slabs can be all sizes and don't line up
#: in rows. Each layout is its slabs, left, top, right and bottom in art pixels. Every slab has its
#: joint along its top and left, so a block's top and left edges are joints and any two blocks meet.
FLOOR = 2 * T
FLOOR_LAYOUTS = (
    ((0, 0, 13, 11), (13, 0, 32, 9), (13, 9, 23, 20), (23, 9, 32, 20), (0, 11, 13, 24), (13, 20, 32, 32),
     (0, 24, 13, 32)),
    ((0, 0, 20, 10), (20, 0, 32, 14), (0, 10, 9, 21), (9, 10, 20, 21), (20, 14, 32, 25), (0, 21, 15, 32),
     (15, 21, 20, 25), (15, 25, 32, 32)),
    ((0, 0, 10, 16), (10, 0, 24, 7), (24, 0, 32, 12), (10, 7, 24, 16), (24, 12, 32, 22), (0, 16, 18, 25),
     (18, 16, 24, 22), (18, 22, 32, 32), (0, 25, 18, 32)),
    ((0, 0, 16, 13), (16, 0, 32, 11), (16, 11, 32, 22), (0, 13, 8, 32), (8, 13, 16, 22), (8, 22, 22, 32),
     (22, 22, 32, 32)),
)

#: The wear on each layout, if it has any: a crack (where it starts), or lichen in a joint.
FLOOR_WEAR = (
    dict(crack=(17, 11)),
    dict(lichen=(20, 14)),
    dict(lichen=(10, 16)),
    dict(crack=(3, 16)),
)


def slab_block(layout, seed):
    """One block of the floor: every slab lit along its top and left and in shadow along its bottom
    and right, a shade and a tint of its own, grimy and pitted, then its wear: a crack wandering
    across a slab, or lichen in a joint."""
    img = new_image(FLOOR, FLOOR)

    def set_(x, y, c):
        img[y % FLOOR][x % FLOOR] = [c[0], c[1], c[2], 255]

    slabs, wear = FLOOR_LAYOUTS[layout], FLOOR_WEAR[layout]
    for i, (x0, y0, x1, y1) in enumerate(slabs):
        pick = hash01(i, layout, seed)
        base = 3 if pick < 0.55 else 4 if pick < 0.8 else 2
        n_tint = hash01(layout, i, seed + 5)
        tint = STONE_TINTS[0] if n_tint < 0.22 else STONE_TINTS[1] if n_tint > 0.84 else None
        for y in range(y0, y1):
            for x in range(x0, x1):
                if y == y0 or x == x0:
                    set_(x, y, GROUT)
                    continue
                if y == y0 + 1:
                    k = base + 1
                elif y == y1 - 1 or x == x1 - 1:
                    k = base - 1
                else:
                    k = base
                    grime = math.sin(x * 0.45 + i) + math.sin(y * 0.6 + i * 2)
                    if grime < -1.2 and dither(x, y, -1.2 - grime):
                        k -= 1
                set_(x, y, stone_shade(k, tint, hash01(x, y, seed + 2)))
    if "crack" in wear:
        x, y = wear["crack"]
        for step in range(8):
            if tuple(img[y][x][:3]) == GROUT:
                break
            set_(x, y, SLATE[1])
            x, y = x + (1 if hash01(step, layout, seed + 3) < 0.6 else 0), y + (1 if step % 3 else 0)
    if "lichen" in wear:
        x, y = wear["lichen"]
        for dx, dy, k in ((0, 0, 1), (1, 0, 0), (-1, 0, 1), (0, -1, 2), (0, 1, 0), (0, 2, 1), (2, 0, 0), (-2, 1, 0)):
            set_(x + dx, y + dy, LICHEN[k])
    return img


def cells_of(block):
    """A block's four tiles in the order Tilemap wants them: top left, top right, bottom left, bottom right."""
    return [[line[cx * T:(cx + 1) * T] for line in block[cy * T:(cy + 1) * T]] for cy in range(2) for cx in range(2)]


def floor_blocks(change=None):
    """Every layout of the floor, each as its four tiles, with change (a colour for a pixel, from
    where it is in its tile) made to every pixel if it's given."""
    cells = []
    for layout in range(len(FLOOR_LAYOUTS)):
        for cell in cells_of(slab_block(layout, 401)):
            if change:
                for y in range(T):
                    for x in range(T):
                        cell[y][x] = list(change(cell[y][x][:3], x, y)) + [255]
            cells.append(cell)
    return cells


def worn(colour, x, y):
    """Where feet have worn the floor smooth: paler toward the middle of the tile."""
    d = math.hypot(x + 0.5 - 8, (y + 0.5 - 8) * 1.2) / 8.0
    return lerp_color(colour, SLATE[5], 0.3 * max(0.0, 1.0 - d)) if dither(x, y, 1.0 - d * 0.8) else colour


def in_shadow(colour, x, y):
    """The floor right under a wall's face: darker the nearer it is to the wall."""
    return lerp_color(colour, SLATE[0], 0.55 * (1.0 - y / T))


def steps():
    """Steps up and out, two to a tile: each tread lit along its nose and worn in the middle, its
    riser in shadow under it, and lichen in the corners at the ends."""
    img = tile()
    for y in range(T):
        row = y % 8
        for x in range(T):
            if row == 0:
                k = 5
            elif row < 5:
                k = 4 if abs(x + 0.5 - 8) < 4 and dither(x, y, 0.5) else 3
            elif row == 5:
                k = 2
            else:
                k = 1 if row == 6 else 0
            n = hash01(x, y, 411)
            if 0 < row < 5 and n < 0.08:
                k -= 1
            put(img, x, y, SLATE[max(0, k)])
        if y % 8 in (1, 2, 3, 4):
            put(img, 0, y, GROUT)
            put(img, 8 if y // 8 else 4, y, GROUT)
    for x, y in ((1, 4), (2, 4), (1, 3), (14, 12), (13, 12)):
        put(img, x, y, LICHEN[1])
    return img


def wall_top(variant):
    """The top of a wall, seen from above: rough stones packed in mortar, each lit on its top
    left, darker than the floor. The stones round the edge are the same in every variant."""
    seeds = []
    for j in range(3):
        for i in range(3):
            own = i == 1 and j == 1
            jitter = 411 + (variant * 7 if own else 0)
            seeds.append(((i + 0.5) * T / 3 + (hash01(i, j, jitter) - 0.5) * 3.0,
                          (j + 0.5) * T / 3 + (hash01(j, i, jitter + 1) - 0.5) * 3.0, (i, j)))
    img = tile()
    for y in range(T):
        for x in range(T):
            near = []
            for sx, sy, key in seeds:
                for ox in (-T, 0, T):
                    for oy in (-T, 0, T):
                        dx, dy = x + 0.5 - (sx + ox), y + 0.5 - (sy + oy)
                        near.append((math.hypot(dx, dy * 1.1), dx, dy, key))
            near.sort(key=lambda n: n[0])
            d1, dx, dy, key = near[0]
            d2 = next(n[0] for n in near if n[3] != key)
            base = 1 if hash01(key[0], key[1], 412 + variant * (key == (1, 1))) < 0.65 else 2
            if d2 - d1 < 0.9:
                colour = GROUT
            elif d2 - d1 < 1.8:
                colour = SLATE[base + 1] if dx + dy < -0.5 else SLATE[base - 1]
            else:
                colour = SLATE[base] if hash01(x, y, 413) > 0.07 else SLATE[base - 1]
            put(img, x, y, colour)
    return img


#: The two courses of a wall's face under its coping: where each starts and ends, and its joints
#: (the same in every variant, so a wall has no seams).
FACE_COURSES = ((3, 10, (5, 13)), (10, 16, (1, 9)))


def face_blocks(img, courses, seed, damp=False):
    """Courses of big blocks across a wall's face, each lit along its top and left and in shadow
    along its bottom and right, a shade and tint of its own, and pitted."""
    for top, bottom, joints in courses:
        edges = sorted(set([0] + list(joints) + [T]))
        for x in range(T):
            put(img, x, top, GROUT)
        for x0, x1 in zip(edges, edges[1:]):
            n_tint = hash01(x0, top, seed + 1)
            tint = STONE_TINTS[0] if n_tint < 0.25 else STONE_TINTS[1] if n_tint > 0.8 else None
            base = 4 if hash01(x0, top, seed) < 0.5 else 3
            for y in range(top + 1, bottom):
                for x in range(x0, x1):
                    if x == x0 and x0 in joints:
                        put(img, x, y, GROUT)
                        continue
                    k = base + 1 if y == top + 1 else base - 1 if y == bottom - 1 or x == x1 - 1 else base
                    if damp and 5 <= x <= 8 and hash01(x, y // 2, seed + 2) > 0.3:
                        k -= 1
                    put(img, x, y, stone_shade(k, tint, hash01(x, y, seed + 3)))


def coping(img):
    """The front edge of a wall's top, along the top of its face: a lit lip and the shadow under it."""
    for x in range(T):
        put(img, x, 0, SLATE[5] if x % 8 else SLATE[4])
        put(img, x, 1, SLATE[3])
        put(img, x, 2, SLATE[1])


def wall_face(variant):
    """The upper row of a wall's face: plain, a block with its corner broken off, and damp coming
    down it with lichen growing in the wet."""
    img = tile()
    face_blocks(img, FACE_COURSES, 421, damp=variant == 2)
    coping(img)
    if variant == 1:
        for y in range(11, 15):
            for x in range(10, 15):
                if (x - 10) + (y - 11) < 5:
                    put(img, x, y, SLATE[0] if (x - 10) + (y - 11) < 3 else SLATE[1])
        for x in range(10, 15):
            put(img, x, 15 - max(0, x - 11), SLATE[5])
    if variant == 2:
        for x, y in ((6, 3), (7, 3), (6, 4), (5, 4), (7, 5), (6, 6), (7, 8), (6, 9), (5, 12)):
            put(img, x, y, LICHEN[2] if y < 5 else LICHEN[1])
    return img


def wall_niche():
    """A burial niche cut into a wall's face, arched, with somebody's skull still in it and a bone
    beside it, and the stone round the arch catching the light."""
    img = wall_face(0)
    for y in range(4, 15):
        for x in range(3, 13):
            d = math.hypot(x + 0.5 - 8, y + 0.5 - 8)
            inside = y >= 8 or d < 5.0
            rim = not inside and (y >= 8 or d < 6.2) and 2 <= x <= 13
            if inside:
                put(img, x, y, lerp_color((10, 10, 18), SLATE[1], (y - 4) / 14.0))
            elif rim:
                put(img, x, y, SLATE[5] if x < 8 else SLATE[2])
    for x in range(3, 13):
        put(img, x, 14, SLATE[5])
        put(img, x, 15, SLATE[2])
    skull(img, 6, 9)
    for x in range(9, 12):
        put(img, x, 13, BONE[2])
    put(img, 12, 13, BONE[3])
    return img


def wall_carving():
    """A wall's face with a roundel carved in it, a mark like the waypoints' in the middle: the
    cuts dark along their top and left and lit along their bottom and right, the way something cut
    in is lit from the top left."""
    img = wall_face(0)
    cx, cy = 8.0, 9.0
    for y in range(3, T):
        for x in range(T):
            d = math.hypot(x + 0.5 - cx, y + 0.5 - cy)
            if 4.4 < d < 5.6:
                put(img, x, y, SLATE[1] if (x + 0.5 - cx) + (y + 0.5 - cy) < 0 else SLATE[6])
            elif d <= 4.4:
                put(img, x, y, SLATE[4] if dither(x, y, 0.35) else SLATE[3])
    mark = {(8, yy) for yy in range(6, 13)} | {(7, 7), (6, 8), (9, 9), (10, 10), (7, 11)}
    for x, y in mark:
        put(img, x, y, SLATE[1])
    for x, y in mark:
        if (x + 1, y + 1) not in mark:
            put(img, x + 1, y + 1, SLATE[6])
    return img


#: The course a wall's lower row has over its plinth, carrying on the face's joints.
BASE_COURSES = ((0, 10, (5, 13)),)


def wall_base(variant):
    """The lower row of a wall's face: a course of blocks, then a plinth lit along its top, and a
    dark line where the wall meets the floor, damp and lichen low down."""
    img = tile()
    face_blocks(img, BASE_COURSES, 431 + variant)
    for y in range(10, T):
        for x in range(T):
            k = 5 if y == 10 else 3 if y < 13 else 2 if y < 15 else 0
            if y == 15:
                put(img, x, y, GROUT)
                continue
            put(img, x, y, stone_shade(k, None, hash01(x, y, 432)))
    for x in range(T):
        if hash01(x, variant, 433) < 0.3:
            put(img, x, 14, LICHEN[1])
            put(img, x, 13, LICHEN[2] if hash01(x, 1, 433) < 0.5 else SLATE[2])
    return img


def skull(img, x, y):
    """A skull, looking out, lit on its top left: four pixels of crown, the dark of its eyes, and
    its teeth."""
    rows = (".3322.", "333221", "303201", ".2211.", "..21..")
    for dy, row in enumerate(rows):
        for dx, ch in enumerate(row):
            if ch != ".":
                put(img, x + dx, y + dy, (14, 12, 20) if ch == "0" else BONE[int(ch)])


def bone(c, x0, y0, x1, y1):
    """A long bone lying on the floor, its knobbly ends a pixel wider than its shaft."""
    steps = int(max(abs(x1 - x0), abs(y1 - y0)))
    for i in range(steps + 1):
        t = i / max(1, steps)
        x, y = round(x0 + (x1 - x0) * t), round(y0 + (y1 - y0) * t)
        c.put(x, y, BONE[3] if i in (0, steps) else BONE[2])
        c.put(x, y + 1, BONE[1])
    for x, y in ((x0, y0), (x1, y1)):
        c.put(x, y - 1, BONE[3])
        c.put(x + 1, y + 1, BONE[1])


def paint_rubble(c, variant):
    """A heap of broken stone: big lumps at the bottom, smaller on top, each lit on its top left,
    and grit round it."""
    rng = random.Random(441)
    lumps = [(rng.uniform(3, 13), rng.uniform(10.5, 12.5), rng.uniform(2.4, 3.1)) for _ in range(4)]
    lumps += [(rng.uniform(5, 11), rng.uniform(7.5, 9.5), rng.uniform(1.8, 2.5)) for _ in range(3)]
    lumps += [(rng.uniform(6, 10), rng.uniform(5.0, 6.5), rng.uniform(1.4, 1.9))]
    for cx, cy, r in lumps:
        base = 3 if rng.random() < 0.6 else 4
        phase = rng.uniform(0, 6.28)
        for y in range(T):
            for x in range(T):
                dx, dy = x + 0.5 - cx, (y + 0.5 - cy) * 1.25
                d = lobed(dx, dy, r, 3, phase, 0.22)
                if d > 1.0:
                    continue
                light = -(dx * 0.7 + dy) / r
                k = base + 1 if light > 0.45 else base if light > -0.2 else base - 2
                c.put(x, y, SLATE[max(0, k)])
    for x, y in ((1, 14), (14, 13), (2, 11), (13, 15), (8, 15)):
        c.put(x, y, SLATE[3])
    return STONE_INK


def paint_bones(c, variant):
    """Bones lying where they fell: two long bones and a rib, or a skull among a few of them."""
    if variant == 0:
        bone(c, 2, 9, 10, 6)
        bone(c, 5, 12, 13, 13)
        for i in range(5):
            c.put(9 + i, 3 + abs(i - 2) // 2, BONE[2])
            c.put(9 + i, 4 + abs(i - 2) // 2, BONE[1])
    else:
        bone(c, 2, 12, 8, 12)
        bone(c, 10, 9, 13, 13)
        skull(c.img, 4, 4)
    return STONE_INK


def cobweb(variant):
    """A web across a corner: threads out from the corner and arcs across them, faint. The second
    is the same web in the other corner."""
    img = tile()
    for y in range(T):
        for x in range(T):
            a = math.atan2(y + 0.5, x + 0.5)
            r = math.hypot(x + 0.5, y + 0.5)
            spoke = min(abs(a - k * math.pi / 8) for k in range(5)) * r < 0.5
            arc = abs(r % 4.0 - 2.0) < 0.35 and r < 13
            if (spoke and r < 14) or arc:
                img[y][T - 1 - x if variant else x] = list(WEB) + [100 if arc else 140]
    return img


CRYPT_TILES = (         # TilesetCatalog.Crypt has the same list in the same order
    ("floor", floor_blocks(), None),
    ("floor-worn", floor_blocks(worn), None),
    ("floor-shadow", floor_blocks(in_shadow), None),
    ("steps", [steps()], None),
    ("wall-top", [wall_top(v) for v in range(2)], None),
    ("wall-face", [wall_face(v) for v in range(3)], None),
    ("wall-niche", [wall_niche()], None),
    ("wall-carving", [wall_carving()], None),
    ("wall-base", [wall_base(v) for v in range(2)], None),
    ("rubble", [detail(paint_rubble)(0)], None),
    ("bones", [detail(paint_bones)(v) for v in range(2)], None),
    ("cobweb", [cobweb(0)], None),
    ("cobweb-ne", [cobweb(1)], None),
)


# --------------------------------------------------------------------------- #
# The barrow's props: the sconces, the doors, the lever and the sarcophagus
# --------------------------------------------------------------------------- #
# Drawn to match my props sheet, in the barrow's cold stone, with iron gone dark and brass gone
# dull. Each keeps the size it always was, since the game puts a sconce's flame and a door's cells
# from those. The doors are two tiles wide and two tall, set in the passage through a wall's face.

IRON = ((28, 26, 38), (54, 52, 68), (90, 88, 106), (138, 136, 154))
BRASS = ((106, 72, 42), (164, 116, 56), (214, 166, 84), (244, 210, 138))
DOOR_WOOD = ((44, 28, 32), (70, 46, 42), (96, 64, 50), (124, 86, 62), (152, 110, 76))
MARBLE = ((56, 58, 78), (82, 86, 106), (112, 116, 136), (144, 146, 164), (176, 178, 192), (206, 206, 218))

SCONCE_W, SCONCE_H = 10, 18


def sconce():
    """An iron plate fixed to the wall with two rivets, a ring off it holding a torch, and a cup
    round the torch's head with a brass rim. The game draws the flame coming out of the cup."""
    c = Canvas(SCONCE_W, SCONCE_H)
    for y in range(10, SCONCE_H):
        for x in range(3, 7):
            if y in (10, SCONCE_H - 1) and x in (3, 6):
                continue
            c.put(x, y, IRON[3] if x == 3 or y == 10 else IRON[1] if x == 6 or y == SCONCE_H - 1 else IRON[2])
    for y in (12, 15):
        c.put(4, y, IRON[3])
        c.put(5, y, IRON[0])
    for y in range(5, 15):
        c.put(4, y, BARK[4])
        c.put(5, y, BARK[2])
    for x in range(2, 8):
        c.put(x, 9, IRON[3] if x < 4 else IRON[2] if x < 6 else IRON[1])
    for x in range(2, 8):
        c.put(x, 3, BRASS[3] if x < 4 else BRASS[2] if x < 6 else BRASS[1])
        c.put(x, 4, IRON[2] if x < 5 else IRON[1])
    for x in range(3, 7):
        c.put(x, 5, IRON[1] if x < 5 else IRON[0])
    c.outline(STONE_INK)
    return c.img


DOOR_W, DOOR_H = 32, 32

#: The doorway: its sides, the middle and the radius of the arch over it, and how deep the ring of
#: wedge stones round the arch is.
DOOR_SIDES = (4, 27)
DOOR_ARCH = (16.0, 14.0, 11.5)
VOUSSOIR_DEPTH = 3.6


def in_doorway(x, y):
    """Whether a pixel of a door's picture is in the doorway, under its arch."""
    cx, cy, r = DOOR_ARCH
    return DOOR_SIDES[0] <= x <= DOOR_SIDES[1] and (y >= cy or math.hypot(x + 0.5 - cx, y + 0.5 - cy) < r)


def door_frame(c, seed):
    """The stone round a doorway: a ring of wedge stones round the arch with a keystone at its top,
    blocks up the jambs either side, the wall over the corners, and a worn threshold. Every stone
    is lit on its top and left."""
    cx, cy, r = DOOR_ARCH
    for y in range(DOOR_H):
        for x in range(DOOR_W):
            if in_doorway(x, y) and y < DOOR_H - 2:
                continue
            dx, dy = x + 0.5 - cx, y + 0.5 - cy
            d = math.hypot(dx, dy)
            if y >= DOOR_H - 2:
                k = 5 if y == DOOR_H - 2 else 2
            elif dy < 0.5 and d < r + VOUSSOIR_DEPTH:
                angle = math.degrees(math.atan2(-dy, dx))
                joint = abs((angle + 11.25) % 22.5 - 11.25) < 2.4 * 12.0 / d
                keystone = abs(angle - 90) < 9
                k = 1 if joint and not keystone else 6 if d < r + 1 else 5 if keystone else 4 if dx < 0 else 3
            else:
                course = y // 6
                left = x < DOOR_SIDES[0] or (x < cx and y < cy)
                joint = y % 6 == 0 or (x == (3 if left else 28) and course % 2 == 0)
                k = 1 if joint else 5 if y % 6 == 1 or x in (0, DOOR_SIDES[1] + 1) else 3 if left else 2
            c.put(x, y, stone_shade(k, None, hash01(x, y, seed)))


def door():
    """A heavy double door under a round arch: planks of old dark wood, two iron straps studded
    with rivets, a brass ring on each leaf. Open, the leaves stand back against the jambs and
    there's only the dark beyond."""
    img = new_image(DOOR_W * 2, DOOR_H)
    for f in range(2):
        c = Canvas(DOOR_W, DOOR_H)
        door_frame(c, 451)
        for y in range(DOOR_H - 2):
            for x in range(DOOR_SIDES[0], DOOR_SIDES[1] + 1):
                if not in_doorway(x, y):
                    continue
                if f:
                    leaf_side = x <= DOOR_SIDES[0] + 2 or x >= DOOR_SIDES[1] - 2
                    if leaf_side:
                        c.put(x, y, DOOR_WOOD[2] if x <= DOOR_SIDES[0] + 2 else DOOR_WOOD[1])
                    else:
                        c.put(x, y, lerp_color((6, 6, 12), SLATE[1], max(0.0, (y - 20) / 12.0)))
                    continue
                plank = (x - DOOR_SIDES[0]) % 4
                k = 0 if x in (15, 16) else 3 if plank == 0 else 1 if plank == 3 else 2
                if hash01(x, y // 3, 452) < 0.12:
                    k = max(0, k - 1)
                if y < 10:
                    k = max(0, k - 1)
                c.put(x, y, DOOR_WOOD[k])
        if not f:
            for strap in (15, 24):
                for x in range(DOOR_SIDES[0], DOOR_SIDES[1] + 1):
                    c.put(x, strap, IRON[3] if x not in (15, 16) else IRON[1])
                    c.put(x, strap + 1, IRON[1])
                for x in range(DOOR_SIDES[0] + 2, DOOR_SIDES[1], 4):
                    c.put(x, strap, BRASS[3])
            for rx in (12, 19):
                for dx, dy, k in ((0, -1, 3), (-1, 0, 3), (1, 0, 1), (0, 1, 1)):
                    c.put(rx + dx, 20 + dy, BRASS[k])
        c.outline(STONE_INK)
        paste(img, c.img, f * DOOR_W, 0)
    return img


def gate():
    """A slab of stone filling an arched doorway, a ring and a mark carved in it like the one on
    the walls. Open, it's sunk into the floor and only its top shows."""
    img = new_image(DOOR_W * 2, DOOR_H)
    for f in range(2):
        c = Canvas(DOOR_W, DOOR_H)
        door_frame(c, 461)
        top = DOOR_H - 4 if f else 0
        for y in range(DOOR_H - 2):
            for x in range(DOOR_SIDES[0], DOOR_SIDES[1] + 1):
                if not in_doorway(x, y):
                    continue
                if y < top:
                    c.put(x, y, lerp_color((6, 6, 12), SLATE[1], max(0.0, (y - 20) / 12.0)))
                    continue
                k = 4 if y == top or x == DOOR_SIDES[0] else 2 if x == DOOR_SIDES[1] else 3
                if not f:
                    d = math.hypot(x + 0.5 - 16, y + 0.5 - 18)
                    if 5.0 < d < 6.2:
                        k = 1 if (x + 0.5 - 16) + (y + 0.5 - 18) < 0 else 6
                c.put(x, y, stone_shade(k, None, hash01(x, y, 462)))
        if not f:
            mark = {(16, yy) for yy in range(14, 23)} | {(15, 15), (14, 16), (17, 17), (18, 18), (15, 20)}
            for x, y in mark:
                c.put(x, y, SLATE[1])
            for x, y in mark:
                if (x + 1, y + 1) not in mark:
                    c.put(x + 1, y + 1, SLATE[6])
            for x, y in ((6, 27), (7, 27), (6, 26), (25, 28), (26, 27)):
                c.put(x, y, LICHEN[1])
        c.outline(STONE_INK)
        paste(img, c.img, f * DOOR_W, 0)
    return img


LEVER_W, LEVER_H = 12, 20

#: The pin the lever turns on, where the end of its handle is, up and then down, and the light
#: along the handle's top edge.
LEVER_PIN = (5, 12)
LEVER_ENDS = ((3, 2), (8, 18))
LEVER_SHINE = (186, 184, 200)


def lever():
    """A carved stone plate on the wall with a slot down it, an iron lever on a pin through it and
    a brass knob on its end: up, then pulled down."""
    img = new_image(LEVER_W * 2, LEVER_H)
    for f in range(2):
        c = Canvas(LEVER_W, LEVER_H)
        for y in range(6, 19):
            for x in range(1, 11):
                if y in (6, 18) and x in (1, 10):
                    continue
                k = 4 if y == 6 or x == 1 else 1 if y == 18 or x == 10 else 3 if (x + y) % 7 else 2
                c.put(x, y, stone_shade(k, None, hash01(x, y, 471)))
        for y in range(8, 17):
            c.put(5, y, (16, 16, 26))
            c.put(6, y, (16, 16, 26))
            c.put(7, y, SLATE[5])
        (px, py), (ex, ey) = LEVER_PIN, LEVER_ENDS[f]
        steps = int(max(abs(ex - px), abs(ey - py)))
        for i in range(steps + 1):
            t = i / steps
            x, y = round(px + (ex - px) * t), round(py + (ey - py) * t)
            c.put(x, y, LEVER_SHINE)
            c.put(x + 1, y, IRON[3])
            c.put(x + 2, y, IRON[1])
        for dx, dy, k in ((0, -1, 3), (-1, 0, 3), (0, 0, 2), (1, 0, 1), (0, 1, 1), (1, 1, 0), (1, -1, 2), (2, 0, 1)):
            c.put(ex + dx, ey + dy, BRASS[k])
        for dx, dy, k in ((0, 0, 3), (1, 0, 2), (0, 1, 1), (1, 1, 0)):
            c.put(px + dx, py + dy, IRON[k])
        c.outline(STONE_INK)
        paste(img, c.img, f * LEVER_W, 0)
    return img


SARCOPHAGUS_W, SARCOPHAGUS_H = 34, 26

#: Where the lid stops and the front of the box starts.
LID_BOTTOM = 15


def sarcophagus():
    """A stone coffin. Its lid, seen from above, has a knight carved lying on it, head to the left,
    with his sword down his middle and his hands on its hilt, raised off the lid so he's lit along
    his top and left and shadowed along his bottom and right; a dull brass band runs round the lid's
    edge; the front of the box is in the lid's shadow, with a carved panel and a roundel in it,
    lichen at its foot, and a cobweb across its far corner."""
    c = Canvas(SARCOPHAGUS_W, SARCOPHAGUS_H)
    for y in range(1, SARCOPHAGUS_H):
        for x in range(1, SARCOPHAGUS_W - 1):
            if y < LID_BOTTOM:
                k = 5 if y == 1 or x == 1 else 4 if y == 2 else 1 if y == LID_BOTTOM - 2 or x == SARCOPHAGUS_W - 2 else 2
                if y == LID_BOTTOM - 1:
                    c.put(x, y, BRASS[2] if x < 17 else BRASS[1])
                    continue
            else:
                panel = 4 <= x < SARCOPHAGUS_W - 4 and 17 <= y < SARCOPHAGUS_H - 3
                k = 2
                if panel and (y == SARCOPHAGUS_H - 4 or x == SARCOPHAGUS_W - 5):
                    k = 3
                elif panel and (y == 17 or x == 4):
                    k = 0
                elif y >= SARCOPHAGUS_H - 2:
                    k = 3 if y == SARCOPHAGUS_H - 2 else 1
            c.put(x, y, MARBLE[k] if hash01(x, y, 481) > 0.06 else MARBLE[max(0, k - 1)])

    def knight(x, y):
        """Whether a pixel of the lid is the knight: his head, then his body, broad at the
        shoulders and narrowing to his feet."""
        if math.hypot(x + 0.5 - 6.5, y + 0.5 - 8) < 2.6:
            return True
        half = 3.4 if x < 15 else 3.0 if x < 21 else 2.4 if x < 28 else 1.6
        return 9 <= x <= 29 and abs(y + 0.5 - 8) < half

    for y in range(3, LID_BOTTOM - 2):
        for x in range(3, SARCOPHAGUS_W - 3):
            if not knight(x, y):
                continue
            if not knight(x, y - 1) or not knight(x - 1, y):
                k = 5
            elif not knight(x, y + 1) or not knight(x + 1, y):
                k = 0
            else:
                k = 4 if y < 8 else 3
            c.put(x, y, MARBLE[k])
    for x, y in ((6, 8), (7, 8), (8, 8)):
        c.put(x, y, MARBLE[0])
    for x in range(12, 29):
        c.put(x, 8, MARBLE[5])
        c.put(x, 9, MARBLE[2])
    for y in range(5, 12):
        c.put(12, y, BRASS[2] if y < 9 else BRASS[1])
    for x, y in ((10, 7), (11, 7), (10, 9), (11, 9)):
        c.put(x, y, MARBLE[5] if y == 7 else MARBLE[2])
    for x, y in ((16, 21), (17, 20), (18, 21), (17, 22)):
        c.put(x, y, MARBLE[4] if y < 21 or x < 17 else MARBLE[0])
    for x, y in ((2, 24), (3, 24), (2, 23), (31, 24), (30, 24)):
        c.put(x, y, LICHEN[1])
    c.outline(STONE_INK)
    for i in range(5):
        c.glaze(SARCOPHAGUS_W - 2 - i, 1 + i, WEB, 90)
        c.glaze(SARCOPHAGUS_W - 2 - i, 1, WEB, 70)
        c.glaze(SARCOPHAGUS_W - 2, 1 + i, WEB, 70)
    return c.img


def barrow_drawn():
    """The barrow's props, by name."""
    return {"sconce": sconce(), "door": door(), "gate": gate(), "lever": lever(), "sarcophagus": sarcophagus()}


if __name__ == "__main__":
    print("Generating the world art for Legend Quest...")
    build_glow()
    build_mote()
    build_flame()
    print("  Tiles/forest.png: painted by tools/rooms/paint_tiles.py now; not written here")
    print("  Tiles/crypt.png: painted by tools/rooms/paint_tiles.py now; not written here")
    for made in (forest_cuts, forest_drawn, road_drawn, props_cuts, barrow_drawn):
        for name, picture in made().items():
            write_png(os.path.join(OUT, "Props", name + ".png"), picture)
    print("Done.")
