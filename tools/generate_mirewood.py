"""
The Mirewood and the Old Quarry under it: the wood's tileset and the quarry's, the black-veined
trees cut off my trees sheet, and everything else that stands in them, drawn to match.

    python tools/generate_mirewood.py

The Mirewood starts as the same wood as the old road at its west edge, bright grass and wild
flowers and friendly oaks, and goes bad as it goes east: the grass wilts and then greys, black veins
run through the leaves and the bark, grey-violet moss creeps over everything, and the stream comes
down from the quarry streaked with tar. Beautiful and wrong is what I was after: the colours of my
style sheet and its light from the top left, only sickening. The grass, the leaves and the
undergrowth are drawn the way generate_world.py draws the old road's, so the two woods sit together,
and the rotten ground is the same ground in sick colours, with the same light across it, so where
one meets the other only the colour changes. The trees are cut off my trees sheet with
cut_supplied.py's cutter, like the road's, and then turned: their leaves pulled toward a grey olive
with violet in the shadows, and black veins drawn up their bark and out through their crowns, each
with a violet glow along one side.

The quarry is the stone the city's built of, cut in steps and left to the roots: pale limestone
floors in blocks of every size with the chisel marks still on them, walls the barrow's way (the top
seen from above, then two rows of face), roots coming down the faces and across the floor, and the
black crystal under it all, veining the floor and the walls with its violet light. The crystal
spike, the root walls Sera parts, and the courier and his satchel are drawn here too.

Only needs CPython. Every bit of noise is hash01 on where it is or a seeded Random, and the cutter
has nothing random in it, so a rerun writes the same bytes.

Everything is drawn as if a light were right on it. Both maps are dark: the wood's twilight, its
grey-violet tint and the quarry's black are the game's.
"""

import colorsys
import math
import os
import random
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from pixels import OUT, hash01, lerp_color, new_image, write_png  # noqa: E402
from pixels import put as glaze_at  # noqa: E402
from generate_harbour import T, Canvas, blobs, build_sheet, dither, put, tile, turned  # noqa: E402
from generate_world import (  # noqa: E402
    BARK, BONE, BOARD, EARTH, HEARTWOOD, LEAF_HUES, LEAF_SATURATION, MOSS, NAIL, PEBBLE, STONE_INK, TREES_SHEET,
    UNDERGROWTH, WOOD_INK, bark_at, bone, cells_of, clump, detail, detail_litter, leaf, lobed, paint_fern,
    paint_pebbles, paint_roots, pebble, rings, shade, undergrowth, undergrowth_edges,
)
from cut_supplied import cut_out, paste  # noqa: E402


# --------------------------------------------------------------------------- #
# The palette
# --------------------------------------------------------------------------- #
# Every ramp goes dark to light. The three grasses share their light and their blades, so one goes
# into the next with only its colour changing.

#: The wood's west edge: the old road's grass, brighter and warmer, since the Mirewood's twilight
#: takes some of it back.
LUSH = ((38, 82, 58), (62, 116, 60), (96, 150, 62), (132, 180, 70), (174, 206, 92), (214, 232, 134))

#: The grass on the turn: yellowing, a little dry.
WILT = ((58, 70, 56), (94, 100, 60), (132, 128, 66), (164, 150, 78), (196, 176, 100), (224, 206, 142))

#: The grass gone: a grey olive with lavender in its shadows.
ROT = ((46, 40, 62), (70, 66, 86), (96, 98, 96), (122, 124, 106), (150, 150, 122), (184, 180, 150))

#: Wild flowers on the lush grass: white, buttercup, pink, cornflower; and their middles.
WILDFLOWERS = ((248, 244, 234), (252, 210, 72), (244, 140, 180), (120, 160, 246))
FLOWER_MIDDLE = ((250, 196, 70), (196, 120, 40), (252, 236, 150), (250, 236, 170))

#: The black in the veins, the violet glow along them, and the bright knot where they branch.
VEIN = (16, 10, 26)
VEIN_GLOW = (140, 84, 200)
VEIN_HOT = (214, 174, 252)

#: The gloom moss: grey-violet cushions, and the pale spores on them.
GLOOM = ((40, 32, 58), (64, 54, 88), (92, 80, 120), (124, 112, 152), (162, 150, 186))
SPORE = (226, 214, 252)

#: The earth under rotten leaves, and the leaves: dull maroon, brown, a grey olive and a bruise.
SOIL_ROT = ((42, 30, 48), (62, 46, 62), (84, 64, 76))
ROT_LEAVES = ((104, 62, 66), (120, 86, 72), (96, 92, 70), (88, 66, 96))

#: Tar, black to its lit rim, and the oil's colours on it: violet, teal, lilac, white and a gold.
TAR = ((6, 4, 12), (16, 12, 26), (28, 22, 42), (46, 38, 66), (74, 64, 102))
SHEEN = ((96, 64, 160), (54, 120, 146), (170, 132, 222), (232, 224, 252), (206, 184, 112))

#: The stream: murky water going dark green-teal, its ripples, and their white.
MURK = ((10, 30, 42), (16, 46, 56), (24, 64, 72), (38, 86, 88), (66, 118, 110), (138, 178, 162), (226, 244, 234))

#: Mushrooms: pale ones that glow a little, and ink caps.
PALE_CAP = ((112, 100, 152), (160, 150, 204), (204, 198, 238), (240, 236, 254))
INK_CAP = ((10, 6, 18), (30, 22, 46), (56, 46, 80), (92, 82, 124))
STALK = ((132, 124, 140), (188, 180, 190), (230, 224, 232))

#: The quarry's stone: the city's pale limestone, lavender in its shadows, darkest to lightest.
QUARRY = ((50, 44, 62), (76, 68, 86), (106, 96, 110), (140, 128, 136), (174, 162, 158), (204, 194, 182),
          (230, 222, 208))
QUARRY_GROUT = (34, 28, 42)

#: The black crystal: its facets, darkest to the lit one, and its violet, dull to blazing.
CRYSTAL = ((8, 6, 16), (20, 14, 34), (36, 26, 56), (58, 44, 84), (96, 80, 128))
CRYSTAL_GLOW = ((96, 52, 150), (150, 92, 214), (200, 150, 250), (240, 222, 255))

#: The crystal made clean: white crystal with gold in it.
PURE = ((160, 132, 92), (206, 182, 128), (238, 220, 168), (252, 244, 214), (255, 253, 240))

#: The fungus that glows in the quarry: teal caps going pale where they're brightest.
GLOW_CAP = ((30, 96, 112), (60, 168, 176), (132, 228, 226), (214, 252, 246))

#: The rot's outline: round leaves gone grey, and round tar.
ROT_INK = (24, 20, 34)
TAR_INK = (4, 2, 8)


def glow_into(img, x, y, colour, alpha):
    """Lays a see-through colour over a pixel of a tile, wrapping round it like put does."""
    glaze_at(img, x % T, y % T, tuple(colour) + (alpha,))


def vein_line(setter, points, glow=True):
    """A black vein along a run of points, with its violet glow on its upper left side, the way the
    light falls, and nothing drawn twice. setter(x, y, colour, alpha) does the drawing."""
    seen = set()
    for x, y in points:
        if glow:
            for dx, dy in ((-1, 0), (0, -1)):
                if (x + dx, y + dy) not in points:
                    setter(x + dx, y + dy, VEIN_GLOW, 150)
    for x, y in points:
        if (x, y) not in seen:
            setter(x, y, VEIN, 255)
            seen.add((x, y))


def wander(rng, x, y, heading, steps, bend=0.5):
    """The pixels a vein or a root runs through: from x, y, the way it's heading (radians), turning a
    little as it goes."""
    points = []
    fx, fy = x + 0.5, y + 0.5
    for _ in range(steps):
        points.append((int(math.floor(fx)), int(math.floor(fy))))
        heading += rng.uniform(-bend, bend)
        fx += math.cos(heading)
        fy += math.sin(heading)
    out = []
    for p in points:
        if not out or out[-1] != p:
            out.append(p)
    return out


# --------------------------------------------------------------------------- #
# Tiles/mirewood.png -- the wood
# --------------------------------------------------------------------------- #
# The ground is three grasses (lush, wilting and rotten), fallen leaves and rotten ones under the
# trees, gloom moss, the rangers' trail, the tar-streaked stream, the undergrowth, and the rock the
# quarry's cut into. Each variant of a tile has the same edges as the others and only its middle
# changes, so a field of them has no seams. The leaves, the moss, the trail, the stream, the
# undergrowth and the top of the rock are edged. Everything lying about is detail, drawn over the
# ground and kept off the edges of its tile. The trees are props.

#: The seed every grass's light comes from, so the three grasses carry on into each other.
GRASS_LIGHT = 601


def tuft_in(img, x, y, ramp):
    """A tuft of grass: a few blades lit at their tips, dark at their root."""
    for dx, dy, k in ((0, -1, 5), (-1, 0, 4), (1, 0, 4), (0, 0, 3), (-2, 1, 3), (-1, 1, 2), (0, 1, 3),
                      (1, 1, 2), (2, 1, 1), (-1, 2, 0), (0, 2, 1), (1, 2, 0)):
        put(img, x + dx, y + dy, ramp[k])


def meadow(ramp, variant):
    """Grass the way generate_world.py draws the road's: soft light and dark across it like light
    through leaves, a blade here and there, and two tufts. The light and the blades are the same in
    every variant and every grass; the tufts are each variant's own, off its edges."""
    img = tile()
    light = blobs(GRASS_LIGHT, 6, 5.0)
    for y in range(T):
        for x in range(T):
            m = light[y][x]
            c = 2
            if m > 0.25 and dither(x, y, (m - 0.25) * 1.5):
                c = 3
            elif m < -0.45 and dither(x, y, (-0.45 - m) * 1.3):
                c = 1
            n = hash01(x, y, GRASS_LIGHT + 1)
            if n < 0.05:
                c = 1
            elif n > 0.96:
                c = 3
            put(img, x, y, ramp[c])
    for y in range(T):
        for x in range(T):
            if hash01(x, y, GRASS_LIGHT + 2) < 0.07:
                put(img, x, y, ramp[4] if hash01(y, x, GRASS_LIGHT + 3) < 0.4 else ramp[3])
                put(img, x, y + 1, ramp[1])
    for i in range(2):
        tuft_in(img, 3 + int(hash01(i, variant, 611) * 10), 3 + int(hash01(variant, i, 612) * 9), ramp)
    return img


def flower_at(img, x, y, petal, middle):
    """A little five-petalled flower: four petals round a coloured middle, lit on the top left, and
    a pixel of shadow and stem under it."""
    for dx, dy in ((0, -1), (-1, 0), (1, 0), (0, 1)):
        put(img, x + dx, y + dy, petal if dx <= 0 and dy <= 0 else shade(petal, 0.84))
    put(img, x, y, middle)
    put(img, x + 1, y + 1, LUSH[0])
    put(img, x, y + 2, LUSH[1])


def grass(variant):
    """The lush grass at the wood's west edge, a flower or three on most of it: white, buttercup,
    pink and cornflower, like the meadows round the city."""
    img = meadow(LUSH, variant)
    for i in range(variant):
        x, y = 3 + int(hash01(i, variant, 613) * 10), 3 + int(hash01(variant, i, 614) * 9)
        k = (i + variant * 2) % len(WILDFLOWERS)
        flower_at(img, x, y, WILDFLOWERS[k], FLOWER_MIDDLE[k])
    return img


def grass_wilt(variant):
    """The grass on the turn: yellowing, with a dead blade lying in it and a leaf come down early."""
    img = meadow(WILT, variant)
    if variant in (1, 3):
        x, y = 4 + int(hash01(variant, 7, 615) * 7), 4 + int(hash01(7, variant, 616) * 7)
        leaf(img, x, y, ROT_LEAVES[variant // 2], variant == 1, WILT[0])
    if variant == 2:
        for i in range(4):
            put(img, 6 + i, 9 + i // 2, WILT[5] if i < 2 else WILT[4])
            put(img, 6 + i, 10 + i // 2, WILT[1])
    return img


def grass_rot(variant):
    """The grass gone grey, with black veins coming up through it on some, and a pale spore here
    and there catching the light."""
    img = meadow(ROT, variant)
    if variant in (1, 2):
        rng = random.Random(617 + variant)
        points = wander(rng, 3 + variant * 2, 11 - variant, -0.4 - variant * 0.3, 9, 0.6)
        vein_line(lambda x, y, c, a: glow_into(img, x, y, c, a), points)
    if variant == 3:
        for x, y in ((5, 6), (10, 9), (8, 12)):
            put(img, x, y, SPORE)
            put(img, x + 1, y + 1, ROT[0])
    return img


def fringe_in(img, ramp, side_seed):
    """Grass reaching over the edge of a tile from its north side, ragged, with a darker lip and
    the odd blade further in. generate_world.py's fringe, in any grass. Returns how far it reaches
    down each column."""
    reach = []
    for x in range(T):
        r = 1 + int(1.6 + 1.3 * math.sin(x * math.pi / 4 + side_seed) + hash01(x, 0, 621 + side_seed) * 1.6)
        for y in range(r):
            put(img, x, y, ramp[3] if y == 0 and hash01(x, 1, 622) < 0.5 else ramp[2])
        put(img, x, r, ramp[1])
        if hash01(x, 2, 623 + side_seed) < 0.3:
            put(img, x, r, ramp[3])
            put(img, x, r + 1, ramp[1])
            r += 1
        reach.append(r)
    return reach


def fringe_edges(ramp, seed):
    """Where a ground meets anything else, the grass grows over its edge."""
    edges = []
    for side in range(4):
        img = tile()
        fringe_in(img, ramp, side + seed)
        edges.append(turned(img, side))
    return edges


def leaves(variant):
    """The floor under the lush trees: dark earth and moss with fallen leaves lying on it, each lit
    on its top left with its shadow off its lower right, and blades coming up through. The road's
    leaves, in the lush grass."""
    img = tile()
    soil = blobs(631, 5, 5.0)
    for y in range(T):
        for x in range(T):
            m = soil[y][x]
            c = LUSH[1] if m > 0.15 and dither(x, y, m * 1.4) else EARTH[1]
            if hash01(x, y, 632) < 0.08:
                c = EARTH[0]
            put(img, x, y, c)
    autumn = ((150, 98, 62), (118, 128, 60), (176, 92, 52), (210, 150, 70), (226, 186, 96))
    for i in range(20):
        x, y = int(hash01(i, 1, 633) * T), int(hash01(1, i, 634) * T)
        n = hash01(i, 2, 635)
        leaf(img, x, y, autumn[min(4, int(n * n * 5))], hash01(i, 3, 636) < 0.5, EARTH[0])
    for i in range(3 + variant):
        x, y = 3 + int(hash01(i, variant, 637) * 9), 3 + int(hash01(variant, i, 638) * 9)
        leaf(img, x, y, autumn[int(hash01(i, variant + 5, 639) * 5)], hash01(variant, i + 5, 640) < 0.5, EARTH[0])
    for y in range(T):
        for x in range(T):
            if hash01(x, y, 641) < 0.04:
                put(img, x, y, LUSH[3])
                put(img, x, y + 1, LUSH[1])
    return img


def leaves_blackvein(variant):
    """The floor under the rotten trees: dark soil gone violet, the leaves on it maroon and grey,
    some of them black with a violet rib down them where the veins came down with them, and a vein
    running out across the soil on some."""
    img = tile()
    soil = blobs(651, 5, 5.0)
    for y in range(T):
        for x in range(T):
            m = soil[y][x]
            c = ROT[1] if m > 0.2 and dither(x, y, m * 1.3) else SOIL_ROT[1]
            if hash01(x, y, 652) < 0.08:
                c = SOIL_ROT[0]
            put(img, x, y, c)
    for i in range(18):
        x, y = int(hash01(i, 1, 653) * T), int(hash01(1, i, 654) * T)
        leaf(img, x, y, ROT_LEAVES[int(hash01(i, 2, 655) * 4)], hash01(i, 3, 656) < 0.5, SOIL_ROT[0])
    for i in range(2 + variant):
        x, y = 3 + int(hash01(i, variant, 657) * 9), 3 + int(hash01(variant, i, 658) * 9)
        put(img, x, y, TAR[3])
        put(img, x + 1, y, VEIN)
        put(img, x + 2, y, VEIN)
        put(img, x + 1, y + 1, VEIN_GLOW)
        put(img, x + 2, y + 1, TAR[1])
        put(img, x + 3, y + 1, SOIL_ROT[0])
    if variant == 2:
        rng = random.Random(659)
        points = wander(rng, 3, 4, 0.7, 10, 0.5)
        vein_line(lambda x, y, c, a: glow_into(img, x, y, c, a), points)
    return img


def gloom_moss(variant):
    """Gloom moss over everything: soft grey-violet cushions heaped on each other with the dark
    between them, lit on their tops, and pale spores on the brightest. The cushions are on a grid
    shaken about, wrapping round the tile, the same in every variant but the ones in the middle."""
    img = tile()
    for y in range(T):
        for x in range(T):
            put(img, x, y, GLOOM[0])
    cushions = []
    for i in range(6):
        drop = hash01(i, 9, 661) * 2.6
        for j in range(5):
            own = 1 <= i <= 4 and 1 <= j <= 3
            seed = 662 + (variant * 13 if own else 0)
            cx = i * 2.67 + (hash01(i, j, seed) - 0.5) * 1.6
            cy = j * 3.2 + drop + (hash01(j, i, seed + 1) - 0.5) * 1.6
            r = 1.8 + hash01(i + 4, j, seed + 2) * 0.9
            cushions.append((cy, cx, r, seed))
    for cy, cx, r, seed in sorted(cushions):
        clump(img, cx, cy, r, GLOOM, seed)
    for y in range(T):
        for x in range(T):
            if hash01(x, y, 663 + variant) < 0.03 and tuple(img[y][x][:3]) in (GLOOM[3], GLOOM[4]):
                put(img, x, y, SPORE)
    return img


def trail(variant):
    """The rangers' trail: earth packed hard by their boots, darker than the road, with grit worked
    up through it, a fallen leaf, and on one a boot print going east."""
    img = tile()
    light = blobs(671, 6, 4.5, squash=1.6)
    for y in range(T):
        for x in range(T):
            m = light[y][x]
            c = 2
            if m > 0.3 and dither(x, y, (m - 0.3) * 1.4):
                c = 3
            elif m < -0.4 and dither(x, y, (-0.4 - m) * 1.4):
                c = 1
            n = hash01(x, y, 672)
            if n < 0.06:
                c = 1
            elif n > 0.97:
                c = 4
            put(img, x, y, EARTH[c])
    for y in range(T):
        for x in range(T):
            if hash01(x, y, 673) < 0.015:
                put(img, x, y, PEBBLE[2])
                put(img, x + 1, y + 1, EARTH[0])
    if variant == 1:
        leaf(img, 6, 7, (150, 98, 62), True, EARTH[1])
    if variant == 2:
        for bx, by in ((4, 5), (10, 9)):
            for dy in range(4):
                for dx in range(2 + (dy in (1, 2))):
                    put(img, bx + dx, by + dy, EARTH[1])
            put(img, bx, by + 5, EARTH[1])
            put(img, bx + 1, by + 5, EARTH[1])
            put(img, bx, by + 4, EARTH[3])
    return img


def trail_edges():
    """The trail's verge: a ragged lip of moss and short grass in a dull green that sits by the
    lush grass and the rotten alike, with a stone in it now and then, and under it a line of shadow
    on the north and west, where the verge stands between the trail and the light, and of light on
    the south and east."""
    edges = []
    verge = (MOSS[0], MOSS[1], (60, 96, 70), (84, 116, 74))
    for side in range(4):
        img = tile()
        for x in range(T):
            r = 1 + int(1.0 + 1.0 * math.sin(x * math.pi / 5 + side * 1.7) + hash01(x, side, 674) * 1.4)
            for y in range(r):
                put(img, x, y, verge[3] if y == 0 and hash01(x, side, 675) < 0.5 else verge[2])
            put(img, x, r, verge[1])
            under = EARTH[0] if side in (0, 3) else EARTH[4]
            put(img, x, r + 1, under)
            if hash01(x, side, 676) < 0.12:
                put(img, x, r, PEBBLE[2])
                put(img, x + 1, r, PEBBLE[1])
        edges.append(turned(img, side))
    return edges


# The stream, which moves. Its water and the patches of tar on it are the same in every frame and
# every variant, so the stream has no seams; what moves is the oil on the tar, its colours going
# round each patch, and the ripples on the water, which come and go each in its own time and stay
# a pixel off the tile's edges.

STREAM_FRAMES = 4

#: The patches of tar on the stream: across the tile, down it, and how big. The first ones cross
#: the edges, so they're the same in every variant; the last is each variant's own.
SLICKS = ((2.0, 3.0, 3.6, 1.6), (12.5, 12.0, 4.0, 1.8), (9.0, 6.5, 2.6, 1.3))


def slick_shape(cx, cy, rx, ry, x, y):
    """How far a pixel is inside a patch of tar: under 1 is in it. It wraps round the tile."""
    dx = min(abs(x + 0.5 - cx), T - abs(x + 0.5 - cx))
    dy = min(abs(y + 0.5 - cy), T - abs(y + 0.5 - cy))
    return math.hypot(dx / rx, dy / ry)


def tar_patch(img, cx, cy, rx, ry, frame, seed):
    """A patch of tar on whatever's under it: black, lit along its top left, with a rim of oil on it
    whose colours go round it from frame to frame, and one glint."""
    for y in range(T):
        for x in range(T):
            d = slick_shape(cx, cy, rx, ry, x, y)
            if d > 1.0:
                continue
            ux, uy = (x + 0.5 - cx), (y + 0.5 - cy)
            lit = -(ux / rx * 0.7 + uy / ry) > 0.55
            put(img, x, y, TAR[3] if lit and d > 0.55 else TAR[1] if d > 0.5 else TAR[0])
            if 0.62 < d <= 1.0:
                a = math.atan2(uy / ry, ux / rx)
                turn = (a / (2 * math.pi) + frame / STREAM_FRAMES + hash01(int(cx), int(cy), seed)) % 1.0
                if turn < 0.18:
                    put(img, x, y, SHEEN[0])
                elif turn < 0.3:
                    put(img, x, y, SHEEN[2])
                elif 0.55 < turn < 0.66:
                    put(img, x, y, SHEEN[1])
    gx, gy = int(cx - rx * 0.3) % T, int(cy - ry * 0.4) % T
    if frame % 2 == 0:
        put(img, gx, gy, SHEEN[3])


def tar_stream_frames(variant):
    """The stream down from the quarry, every frame of one variant: murky green water with darker
    and lighter patches, streaked with tar whose oil shimmers, and short ripples on the water."""
    patches = [(int(hash01(i, 0, 681) * T), int(hash01(0, i, 681) * T), 3 + int(hash01(i, i, 682) * 5),
                1 + (i % 3 == 0), 1 if i % 2 else 3) for i in range(7)]
    ripples = []
    for i in range(4):
        length = 2 + int(hash01(i, variant, 683) * 3)
        x = 1 + int(hash01(variant, i, 684) * (T - 2 - length))
        y = 1 + int(hash01(i, i + variant, 685) * (T - 3))
        ripples.append((x, y, length, int(hash01(i, variant, 686) * STREAM_FRAMES)))
    own = (3 + hash01(variant, 1, 687) * 9, 4 + hash01(1, variant, 688) * 8)
    frames = []
    for f in range(STREAM_FRAMES):
        img = tile()
        for y in range(T):
            for x in range(T):
                put(img, x, y, MURK[2])
        for px, py, w, h, c in patches:
            for dy in range(h):
                for dx in range(w):
                    if (dx, dy) not in ((0, h - 1), (w - 1, 0)) or h == 1:
                        put(img, px + dx, py + dy, MURK[c])
        for x, y, length, phase in ripples:
            level = (0, 1, 2, 1)[(f + phase) % STREAM_FRAMES]
            if level == 0:
                continue
            shown = length if level == 2 else max(1, length - 2)
            start = x + (length - shown) // 2
            for dx in range(shown):
                put(img, start + dx, y, MURK[5] if level == 2 and 0 < dx < shown - 1 else MURK[4])
                put(img, start + dx, y + 1, MURK[1])
        for i, (cx, cy, rx, ry) in enumerate(SLICKS[:2]):
            tar_patch(img, cx, cy, rx, ry, f, 689 + i)
        tar_patch(img, own[0], own[1], SLICKS[2][2], SLICKS[2][3], f, 691 + variant)
        frames.append(img)
    return frames


def tar_stream_edges():
    """The stream's banks. Along the north it's the bank seen from the front: a lip of moss over a
    face of earth going darker, stained black where the tar's lapped at it, then a line of oil on
    the water. The other sides are a lip of earth with a line of black along the water."""
    edges = []
    north = tile()
    for x in range(T):
        put(north, x, 0, MOSS[1] if hash01(x, 0, 692) < 0.6 else MOSS[2])
        put(north, x, 1, MOSS[0] if hash01(x, 1, 692) < 0.5 else EARTH[2])
        put(north, x, 2, EARTH[3] if x % 7 else EARTH[2])
        put(north, x, 3, EARTH[2])
        put(north, x, 4, EARTH[1] if hash01(x, 4, 693) < 0.7 else TAR[2])
        put(north, x, 5, TAR[1])
        put(north, x, 6, SHEEN[0] if hash01(x, 6, 694) < 0.4 else TAR[3] if hash01(x, 7, 694) < 0.6 else MURK[1])
        if hash01(x, 7, 695) < 0.3:
            put(north, x, 7, MURK[4])
    edges.append(north)
    for side in (1, 2, 3):
        img = tile()
        lit = side == 3
        for x in range(T):
            put(img, x, 0, EARTH[3] if lit else EARTH[2])
            put(img, x, 1, EARTH[1] if hash01(x, side, 696) < 0.7 else MOSS[1])
            put(img, x, 2, TAR[1])
            if hash01(x, side, 697) < 0.45:
                put(img, x, 3, SHEEN[0] if hash01(x, side, 698) < 0.4 else TAR[3])
        edges.append(turned(img, side))
    return edges


#: The undergrowth gone bad: grey-olive leaves over violet dark.
THICKET_ROT = ((18, 14, 30), (34, 28, 50), (54, 48, 74), (78, 74, 94), (104, 104, 108), (136, 134, 128))


def thicket_rot(variant):
    """The undergrowth gone bad: generate_world.py's undergrowth, clump for clump, in grey-olive
    leaves over a violet dark, so where it meets the green undergrowth the clumps carry on."""
    img = undergrowth(variant)
    return [[list(recolour_thicket(px)) for px in line] for line in img]


def thicket_rot_edges():
    """The undergrowth's top, face and sides, gone bad the same way."""
    return [[[list(recolour_thicket(px)) for px in line] for line in edge] for edge in undergrowth_edges()]


def recolour_thicket(px):
    """One pixel of the green undergrowth in the rotten undergrowth's colours: each of its green
    shades for the rotten one in the same place, and anything else (the stems, the outline)
    darkened toward violet."""
    if not px[3]:
        return px
    rgb = tuple(px[:3])
    if rgb in UNDERGROWTH:
        return list(THICKET_ROT[UNDERGROWTH.index(rgb)]) + [px[3]]
    return list(lerp_color(rgb, TAR[2], 0.4)) + [px[3]]


# The rock the quarry's cut into, where it comes up through the wood: its top seen from above, then
# the cut face in two rows, the quarry's own stone. The top is edged: a lit lip along its south side
# where the face drops away under it, and its sides where it ends in the wood.

def rock_lumps(img, ramp, seed, variant, moss=None):
    """Rough rock seen from above, lumps of it packed together, each lit on its top left, with moss
    or grass in the cracks between if there's any. The lumps round the edge are the same in every
    variant, and they wrap round the tile."""
    lumps = []
    for j in range(3):
        for i in range(3):
            own = i == 1 and j == 1
            jitter = seed + (variant * 7 if own else 0)
            lumps.append(((i + 0.5) * T / 3 + (hash01(i, j, jitter) - 0.5) * 3.0,
                          (j + 0.5) * T / 3 + (hash01(j, i, jitter + 1) - 0.5) * 3.0, (i, j)))
    for y in range(T):
        for x in range(T):
            near = []
            for sx, sy, key in lumps:
                for ox in (-T, 0, T):
                    for oy in (-T, 0, T):
                        dx, dy = x + 0.5 - (sx + ox), y + 0.5 - (sy + oy)
                        near.append((math.hypot(dx, dy * 1.1), dx, dy, key))
            near.sort(key=lambda n: n[0])
            d1, dx, dy, key = near[0]
            d2 = next(n[0] for n in near if n[3] != key)
            base = 2 if hash01(key[0], key[1], seed + 2 + variant * (key == (1, 1))) < 0.6 else 3
            if d2 - d1 < 0.9:
                colour = moss[1] if moss and hash01(x, y, seed + 3) < 0.55 else QUARRY_GROUT
            elif d2 - d1 < 1.8:
                colour = ramp[base + 1] if dx + dy < -0.5 else ramp[base - 1]
            else:
                colour = ramp[base] if hash01(x, y, seed + 4) > 0.07 else ramp[base - 1]
            put(img, x, y, colour)


#: The stones showing through the grass on top of the rock: where each is, and how big.
CLIFF_STONES = (((4.0, 5.0, 3.2), (11.5, 11.0, 2.6)), ((10.0, 4.5, 2.8), (4.0, 11.5, 3.0), (13.0, 13.0, 1.6)))


def cliff_top(variant):
    """The top of the rock, seen from above: the wood's rotten grass grown over it, and the pale
    stone showing through in lumps, lit on their top left."""
    img = meadow(ROT, variant)
    for i, (cx, cy, r) in enumerate(CLIFF_STONES[variant]):
        for y in range(T):
            for x in range(T):
                dx, dy = x + 0.5 - cx, (y + 0.5 - cy) * 1.2
                d = lobed(dx, dy, r, 4, i + variant, 0.2)
                if d > 1.0:
                    continue
                light = -(dx * 0.7 + dy) / r
                k = 5 if light > 0.5 else 4 if light > -0.1 else 3 if d < 0.85 else 2
                put(img, x, y, QUARRY[k])
    return img


def cliff_top_edges():
    """Where the rock's top ends. Along the south, the lip of the cut face: a line of light along
    its edge and the shadow just under it. Along the north, a dark seam where it meets the wood
    behind; along the sides, the rock's flank, lit on the west and in shadow on the east."""
    north, east, south, west = tile(), tile(), tile(), tile()
    for x in range(T):
        put(north, x, 0, ROT_INK)
        put(north, x, 1, QUARRY[1])
        for y, k in ((12, 4), (13, 6), (14, 5), (15, 3)):
            put(south, x, y, QUARRY[k] if x % 9 else QUARRY[k - 1])
    for y in range(T):
        put(east, T - 1, y, ROT_INK)
        put(east, T - 2, y, QUARRY[1])
        put(west, 0, y, ROT_INK)
        put(west, 1, y, QUARRY[5] if hash01(0, y, 702) < 0.7 else QUARRY[4])
    return [north, east, south, west]


#: Where the groove a drill left runs down a cut face, on the variant that has one.
GROOVE = 5


def strata(img, rows, seed, variant=0, ramp=QUARRY):
    """A cut face of the quarry's stone: beds of it lying one on another, split into blocks, each
    block lit along its top and left and darker toward its bottom and right, a shade of its own,
    and the odd pit. The joints between blocks are somewhere different in every variant and keep
    off the tile's edges, so a long face doesn't repeat; on one variant the half-round groove a
    drill left runs down it. rows is each bed's top and bottom."""
    for b, (top, bottom) in enumerate(rows):
        joints = [2 + int(hash01(b, variant, seed + 5) * 11)]
        if hash01(variant, b, seed + 6) < 0.5:
            joints.append((joints[0] + 5 + int(hash01(b, variant, seed + 7) * 4)) % 13 + 2)
        edges = sorted(set([0] + joints + [T]))
        for x0, x1 in zip(edges, edges[1:]):
            base = 4 if hash01(x0 + variant * 3, top, seed) < 0.55 else 3
            if hash01(top, x0 + variant, seed + 8) < 0.2:
                base = 5
            for y in range(top, bottom):
                for x in range(x0, x1):
                    if x == x0 and x0 in joints:
                        put(img, x, y, ramp[1])
                        continue
                    if y == top or (x == x0 + 1 and x0 in joints):
                        k = base + 1
                    elif y == bottom - 1 or (x == x1 - 1 and x1 in joints):
                        k = base - 1
                    else:
                        k = base
                    n = hash01(x, y, seed + 1 + variant)
                    if n < 0.06:
                        k -= 1
                    elif n > 0.97:
                        k += 1
                    put(img, x, y, ramp[max(0, min(len(ramp) - 1, k))])
        for x in range(T):
            put(img, x, bottom - 1, ramp[max(0, 2)] if hash01(x, bottom, seed + 2) < 0.8 else ramp[3])
    if variant == 0:
        for y in range(rows[0][0], rows[-1][1]):
            put(img, GROOVE, y, ramp[1])
            put(img, GROOVE + 1, y, ramp[5])


def cliff_face(variant):
    """The upper row of the rock's cut face: beds of pale stone with the drill grooves down them,
    and roots hanging over its top from the wood above, or a crack."""
    img = tile()
    strata(img, ((0, 6), (6, 12), (12, 16)), 711, variant)
    if variant == 0:
        rng = random.Random(712)
        for rx in (6, 13):
            points = wander(rng, rx, 0, math.pi / 2, 5 + rx % 4, 0.35)
            for x, y in points:
                put(img, x, y, BARK[3])
                put(img, x + 1, y, BARK[1])
    else:
        for i, (x, y) in enumerate(((8, 2), (8, 3), (9, 4), (9, 5), (8, 6), (8, 7), (7, 8), (7, 9))):
            put(img, x, y, QUARRY[0])
            put(img, x + 1, y, QUARRY[5])
    return img


def cliff_base(variant):
    """The lower row of the cut face: a bed of stone, then broken chips heaped at its foot with moss
    on them, and the dark where it meets the ground."""
    img = tile()
    strata(img, ((0, 9),), 721, variant)
    for y in range(9, T):
        for x in range(T):
            n = hash01(x, y, 722 + variant)
            if y == T - 1:
                put(img, x, y, ROT_INK)
            elif n < 0.3:
                put(img, x, y, QUARRY[2])
            elif n < 0.55:
                put(img, x, y, QUARRY[4] if y < 12 else QUARRY[3])
            elif n < 0.75:
                put(img, x, y, MOSS[1])
            else:
                put(img, x, y, QUARRY[1])
    return img


# Detail, drawn over the ground and kept off the edges of its tile.

def paint_tuft(c, variant):
    """A clump of long grass in the lush green: thin blades fanning out of one root and curling
    outward as they go, the ones leaning left lit at their tips."""
    paint_blades(c, variant, LUSH, 731)
    return LUSH[0]


def paint_tuft_dead(c, variant):
    """A clump of dead grass, grey and lilac, the blades bent over."""
    paint_blades(c, variant, ROT, 741, droop=0.9)
    return ROT[0]


def paint_blades(c, variant, ramp, seed, droop=0.0):
    """Blades of grass out of one root, fanning out and curling, lit at the tips leaning left."""
    rng = random.Random(seed + variant)
    root = (7.5 + variant, 14.5)
    for i in sorted(range(-4, 5), key=abs, reverse=True):
        angle = i * 0.3 + rng.uniform(-0.08, 0.08)
        length = rng.uniform(7.5, 10.0) - abs(i) * 0.7
        for j in range(int(length)):
            t = j / length
            a = angle * (1.0 + 0.9 * t + droop * t * t * 2.0)
            x = root[0] + math.sin(a) * j
            y = root[1] - math.cos(a) * j
            k = 1 if t < 0.25 else 2 if t < 0.5 else 3 if t < 0.8 or i > 0 else 4
            c.put(x, y, ramp[k])


def paint_flowers(c, variant):
    """Wild flowers on their leaves: a spray of white and buttercup, or pink and cornflower."""
    for cx, cy in ((5, 9), (10, 11), (8, 6), (12, 7)):
        for dx, dy, k in ((0, 0, 3), (1, 0, 2), (0, 1, 2), (1, 1, 1), (-1, 1, 3)):
            c.put(cx + dx, cy + dy + 2, LUSH[k])
    picks = ((0, 1), (2, 3))[variant]
    for i, (fx, fy) in enumerate(((5, 8), (10, 10), (8, 5), (12, 6), (3, 12))):
        k = picks[i % 2]
        for dx, dy in ((0, -1), (-1, 0), (1, 0), (0, 1)):
            c.put(fx + dx, fy + dy, WILDFLOWERS[k] if dx <= 0 and dy <= 0 else shade(WILDFLOWERS[k], 0.82))
        c.put(fx, fy, FLOWER_MIDDLE[k])
    return LUSH[0]


def paint_veins(c, variant):
    """Black veins crawling over the ground out of a knot, branching as they go, the glow along
    them, and the knot bright where they meet."""
    rng = random.Random(751 + variant)
    knot = ((6, 9), (9, 6))[variant]
    for n in range(3 + variant):
        heading = n * 2.1 + rng.uniform(-0.4, 0.4)
        points = [p for p in wander(rng, knot[0], knot[1], heading, rng.randint(5, 8), 0.5)
                  if 1 <= p[0] < T - 1 and 1 <= p[1] < T - 1]
        vein_line(lambda x, y, col, a: c.glaze(x, y, col, a) if a < 255 else c.put(x, y, col), points)
    for dx, dy in ((0, 0), (1, 0), (0, 1), (1, 1)):
        c.put(knot[0] + dx, knot[1] + dy, VEIN)
    c.put(knot[0], knot[1], VEIN_HOT)
    c.put(knot[0] + 1, knot[1], VEIN_GLOW)


def detail_veins(variant):
    """The veins on their own, with no outline: they're in the ground, not on it."""
    c = Canvas(T, T)
    paint_veins(c, variant)
    return c.img


def paint_toadstools(c, variant):
    """Little toadstools in a ring: pale ones that glow a little, or ink caps, black and
    dripping."""
    caps = PALE_CAP if variant == 0 else INK_CAP
    for mx, my, r, stalk in ((5.0, 7.0, 2.6, 4), (10.5, 8.5, 2.0, 3), (8.0, 11.5, 1.6, 2), (12.5, 12.0, 1.3, 2)):
        for y in range(int(my), int(my) + stalk + 1):
            c.put(mx - 0.5, y, STALK[2])
            c.put(mx + 0.5, y, STALK[1] if y < my + stalk else STALK[0])
        for y in range(int(my - r), int(my) + 1):
            for x in range(int(mx - r), int(mx + r) + 1):
                dx, dy = x + 0.5 - mx, y + 0.5 - my
                if math.hypot(dx, dy * (1.2 if variant else 0.9)) > r or dy > 0.4:
                    continue
                light = -(dx * 0.7 + dy) / r
                k = 3 if light > 0.7 else 2 if light > 0.15 else 1 if light > -0.4 else 0
                c.put(x, y, caps[k])
        if variant:
            c.put(mx - r + 0.5, my + 1, INK_CAP[0])
    return ROT_INK


def detail_toadstools(variant):
    """The toadstools, outlined; the pale ones with a faint glow round them."""
    c = Canvas(T, T)
    ink = paint_toadstools(c, variant)
    c.outline(ink)
    if variant == 0:
        for cx, cy in ((5, 6), (10.5, 8)):
            c.glow(cx, cy, 5, PALE_CAP[3], 70)
    return c.img


def detail_tar_spatter(variant):
    """Drops of tar on the ground, glossy: black with a lit rim and a white glint, a dark ring
    round each where it's soaked in."""
    c = Canvas(T, T)
    drops = (((5, 6, 2.2), (10, 10, 1.6), (4, 11, 1.0), (12, 4, 1.0)),
             ((8, 8, 2.8), (3, 4, 1.2), (12, 12, 1.2), (12, 5, 0.9)))[variant]
    for cx, cy, r in drops:
        for y in range(int(cy - r - 1), int(cy + r + 2)):
            for x in range(int(cx - r - 1), int(cx + r + 2)):
                d = math.hypot(x + 0.5 - cx, (y + 0.5 - cy) * 1.3)
                if d <= r:
                    lit = (x + 0.5 - cx) + (y + 0.5 - cy) < -r * 0.5
                    c.put(x, y, TAR[3] if lit else TAR[1] if d > r * 0.5 else TAR[0])
                elif d <= r + 1.0:
                    c.glaze(x, y, TAR[2], 110)
        c.put(cx - r * 0.4, cy - r * 0.4, SHEEN[3])
    return c.img


def paint_brambles(c, variant):
    """Brambles gone black over the undergrowth: stems curling across, thorns off them, and dark
    berries at their tips."""
    rng = random.Random(761 + variant)
    for n in range(3):
        sx, sy = rng.uniform(2, 13), rng.uniform(3, 13)
        heading = rng.uniform(0, 2 * math.pi)
        for x, y in wander(rng, sx, sy, heading, rng.randint(7, 11), 0.7):
            if 1 <= x < T - 1 and 1 <= y < T - 1:
                c.put(x, y, INK_CAP[1])
                if hash01(x, y, 762) < 0.3:
                    c.put(x + 1, y - 1, INK_CAP[3])
        bx, by = int(sx), int(sy)
        c.put(bx, by, (120, 40, 92))
        c.put(bx - 1, by, (74, 22, 60))
    return TAR_INK


def detail_tar_slick(variant):
    """Thick tar lying on the stream near the quarry, every frame of one variant: a patch over the
    water, its oil going round it. No edges of its own, and it stays off the tile's."""
    frames = []
    centre = ((8.0, 8.0, 5.6, 3.0), (7.0, 9.0, 4.6, 3.6))[variant]
    for f in range(STREAM_FRAMES):
        img = tile()
        cx, cy, rx, ry = centre
        for y in range(1, T - 1):
            for x in range(1, T - 1):
                d = math.hypot((x + 0.5 - cx) / rx, (y + 0.5 - cy) / ry)
                if d > 1.0:
                    continue
                ux, uy = x + 0.5 - cx, y + 0.5 - cy
                lit = -(ux / rx * 0.7 + uy / ry) > 0.55
                img[y][x] = list(TAR[3] if lit and d > 0.6 else TAR[1] if d > 0.45 else TAR[0]) + [255]
                if 0.66 < d <= 1.0:
                    turn = (math.atan2(uy / ry, ux / rx) / (2 * math.pi) + f / STREAM_FRAMES + variant * 0.3) % 1.0
                    if turn < 0.16:
                        img[y][x] = list(SHEEN[0]) + [255]
                    elif turn < 0.26:
                        img[y][x] = list(SHEEN[2]) + [255]
                    elif 0.5 < turn < 0.6:
                        img[y][x] = list(SHEEN[1]) + [255]
        gx, gy = int(cx - rx * 0.35), int(cy - ry * 0.4)
        img[gy][gx] = list(SHEEN[3] if f % 2 == 0 else SHEEN[2]) + [255]
        frames.append(img)
    return frames


def flat(frames_by_variant):
    """Every frame of every variant in one list, the order they go on the sheet."""
    out = []
    for frames in frames_by_variant:
        out += frames
    return out


#: Each tile: its name, its frames (every frame of every variant, in order), and its edges if it
#: has them. TilesetCatalog.Mirewood has the same list in the same order, with the same variants
#: and frames.
MIREWOOD_TILES = (
    ("grass", [grass(v) for v in range(4)], None),
    ("grass-wilt", [grass_wilt(v) for v in range(4)], None),
    ("grass-rot", [grass_rot(v) for v in range(4)], None),
    ("leaves", [leaves(v) for v in range(4)], fringe_edges(LUSH, 0)),
    ("leaves-blackvein", [leaves_blackvein(v) for v in range(3)], fringe_edges(ROT, 4)),
    ("gloom-moss", [gloom_moss(v) for v in range(2)], fringe_edges(ROT, 8)),
    ("trail", [trail(v) for v in range(3)], trail_edges()),
    ("tar-stream", flat(tar_stream_frames(v) for v in range(3)), tar_stream_edges()),
    ("undergrowth", [undergrowth(v) for v in range(2)], undergrowth_edges()),
    ("undergrowth-rot", [thicket_rot(v) for v in range(2)], thicket_rot_edges()),
    ("cliff-top", [cliff_top(v) for v in range(2)], cliff_top_edges()),
    ("cliff-face", [cliff_face(v) for v in range(2)], None),
    ("cliff-base", [cliff_base(v) for v in range(2)], None),
    ("tuft", [detail(paint_tuft)(v) for v in range(2)], None),
    ("flowers", [detail(paint_flowers)(v) for v in range(2)], None),
    ("fern", [detail(paint_fern)(v) for v in range(2)], None),
    ("pebbles", [detail(paint_pebbles)(v) for v in range(2)], None),
    ("roots", [detail(paint_roots)(v) for v in range(2)], None),
    ("litter", [detail_litter(v) for v in range(2)], None),
    ("veins", [detail_veins(v) for v in range(2)], None),
    ("tuft-dead", [detail(paint_tuft_dead)(v) for v in range(2)], None),
    ("toadstools", [detail_toadstools(v) for v in range(2)], None),
    ("tar-spatter", [detail_tar_spatter(v) for v in range(2)], None),
    ("brambles", [detail(paint_brambles)(v) for v in range(2)], None),
    ("tar-slick", flat(detail_tar_slick(v) for v in range(2)), None),
)


# --------------------------------------------------------------------------- #
# Tiles/quarry.png -- the Old Quarry
# --------------------------------------------------------------------------- #
# The floor is the bedrock the blocks were cut out of, laid out the way the barrow's slabs are: in
# blocks two tiles each way so the cuts can be every size, each with its joint along its top and
# left so any two meet. The floor in a wall's shadow and the floor the black's got into are the same
# blocks, so they carry on into the floor round them. The walls are the barrow's: the top seen from
# above, then two rows of face, here cut stone with the drill grooves down it, roots hanging off
# it, or the black crystal coming through it.

QFLOOR = 2 * T

#: The floor is the bedrock the blocks came out of, so its cuts are big ones: each layout is its
#: cuts, left, top, right and bottom in art pixels, every one with its joint along its top and
#: left, so a block's top and left edges are joints and any two blocks meet.
QUARRY_CUTS = (
    ((0, 0, 20, 14), (20, 0, 32, 32), (0, 14, 20, 32)),
    ((0, 0, 32, 12), (0, 12, 14, 32), (14, 12, 32, 32)),
    ((0, 0, 12, 20), (12, 0, 32, 18), (12, 18, 32, 32), (0, 20, 12, 32)),
    ((0, 0, 18, 32), (18, 0, 32, 14), (18, 14, 32, 32)),
)

#: What's been left on each of the floor's layouts: the chisel marks across a cut (where they
#: start), and a drill hole in a joint.
QUARRY_WEAR = (
    dict(chisel=(4, 4), drill=(20, 22)),
    dict(chisel=(20, 3), drill=(14, 24)),
    dict(chisel=(16, 6), drill=(12, 26)),
    dict(chisel=(21, 18), drill=(18, 8)),
)


def quarry_block(layout, seed, vein=False):
    """One block of the quarry floor: big cuts of bedrock with the joints between them cut in, dark
    along the joint and lit along its far lip, each cut a shade of its own and gritty, with chisel
    marks across one and a drill hole in a joint. With vein, the black has got into it: its joints
    are black crystal glowing violet into the stone either side, and a crack of it runs across one
    cut."""
    img = new_image(QFLOOR, QFLOOR)

    def set_(x, y, c):
        img[y % QFLOOR][x % QFLOOR] = [c[0], c[1], c[2], 255]

    cuts, wear = QUARRY_CUTS[layout], QUARRY_WEAR[layout]
    joints = set()
    for i, (x0, y0, x1, y1) in enumerate(cuts):
        pick = hash01(i, layout, seed)
        base = 4 if pick < 0.6 else 3
        for y in range(y0, y1):
            for x in range(x0, x1):
                if y == y0 or x == x0:
                    set_(x, y, QUARRY[1])
                    joints.add((x % QFLOOR, y % QFLOOR))
                    continue
                k = base + 1 if (y == y0 + 1 or x == x0 + 1) else base
                n = hash01(x, y, seed + 2)
                if n < 0.08:
                    k -= 1
                elif n > 0.975:
                    k += 1
                grain = math.sin(x * 0.5 + y * 0.9 + i * 2.0) + math.sin(y * 0.35 - i)
                if grain < -1.3 and dither(x, y, -1.3 - grain):
                    k -= 1
                set_(x, y, QUARRY[max(0, min(6, k))])
    cx, cy = wear["chisel"]
    for i in range(4):
        for j in range(3):
            set_(cx + i * 2 + j, cy + j, QUARRY[2])
    dx, dy = wear["drill"]
    for x, y, k in ((0, 0, 0), (1, 0, 1), (0, 1, 1), (1, 1, 5), (-1, 0, 1), (0, -1, 2)):
        set_(dx + x, dy + y, QUARRY[k])
    if vein:
        for x, y in joints:
            set_(x, y, VEIN)
        for x, y in joints:
            for ox, oy, a in ((1, 0, 0.5), (0, 1, 0.5), (2, 0, 0.22), (0, 2, 0.22)):
                px, py = (x + ox) % QFLOOR, (y + oy) % QFLOOR
                if (px, py) in joints:
                    continue
                under = img[py][px][:3]
                mixed = lerp_color(under, VEIN_GLOW, a)
                img[py][px] = list(mixed) + [255]
        rng = random.Random(seed + layout)
        x0, y0, x1, y1 = max(cuts, key=lambda c: (c[2] - c[0]) * (c[3] - c[1]))
        points = wander(rng, x0 + 2, y0 + 2, math.atan2(y1 - y0, x1 - x0), min(x1 - x0, y1 - y0) + 2, 0.4)
        for x, y in points:
            if x0 < x < x1 - 1 and y0 < y < y1 - 1:
                set_(x, y, VEIN)
                set_(x + 1, y, CRYSTAL_GLOW[1])
                set_(x, y + 1, lerp_color(img[(y + 1) % QFLOOR][x % QFLOOR][:3], VEIN_GLOW, 0.5))
    return img


def quarry_floor(change=None, vein=False):
    """Every layout of the quarry floor, each as its four tiles, with change made to every pixel if
    it's given."""
    cells = []
    for layout in range(len(QUARRY_CUTS)):
        for cell in cells_of(quarry_block(layout, 801, vein)):
            if change:
                for y in range(T):
                    for x in range(T):
                        cell[y][x] = list(change(cell[y][x][:3], x, y)) + [255]
            cells.append(cell)
    return cells


def quarry_shadow(colour, x, y):
    """The floor right under a wall's face: darker the nearer it is to the wall."""
    return lerp_color(colour, QUARRY[0], 0.55 * (1.0 - y / T))


def root_floor(variant):
    """Where the roots have broken the floor up: dark earth, and roots over it every way, lit along
    their tops, with the odd chip of stone. The roots cross the edges the same in every variant and
    wrap round the tile, so a patch of it has no seams."""
    img = tile()
    soil = blobs(811, 5, 4.5)
    for y in range(T):
        for x in range(T):
            m = soil[y][x]
            put(img, x, y, SOIL_ROT[1] if m > 0.1 and dither(x, y, m) else SOIL_ROT[0])
    runs = [(0.0, 3.0, 0.15, 18), (5.0, 0.0, 1.35, 18), (0.0, 11.0, -0.2, 18), (12.0, 0.0, 1.75, 18)]
    runs += [(4.0 + variant * 3, 7.0, 0.6 + variant, 7)]
    for i, (x0, y0, heading, steps) in enumerate(runs):
        rng = random.Random(812 + i + (variant * 5 if i == 4 else 0))
        thick = 2 if i < 2 else 1
        for x, y in wander(rng, x0, y0, heading, steps, 0.3):
            put(img, x, y, BARK[3] if thick == 2 else BARK[2])
            put(img, x, y + 1, BARK[1] if thick == 2 else BARK[0])
            if thick == 2:
                put(img, x, y + 2, BARK[0])
    for i in range(3):
        x, y = 2 + int(hash01(i, variant, 813) * 12), 2 + int(hash01(variant, i, 814) * 12)
        put(img, x, y, QUARRY[4])
        put(img, x + 1, y + 1, EARTH[0])
    return img


def root_floor_edges():
    """Where the roots meet the floor: the stone's broken edge over the earth, ragged, lit along
    its top, with its shadow on the earth under it."""
    edges = []
    for side in range(4):
        img = tile()
        for x in range(T):
            r = 1 + int(1.2 + 1.1 * math.sin(x * math.pi / 4 + side * 2.1) + hash01(x, side, 815) * 1.3)
            for y in range(r):
                put(img, x, y, QUARRY[5] if y == r - 1 and side in (0, 3) else QUARRY[4] if y else QUARRY[3])
            put(img, x, r, QUARRY[2] if side in (1, 2) else EARTH[0])
        edges.append(turned(img, side))
    return edges


def tar_seep_frames(variant):
    """Tar welling up out of the floor, every frame of one variant: a black pool, its oil going
    round, and a bubble rising on one frame and gone the next."""
    frames = []
    for f in range(STREAM_FRAMES):
        img = tile()
        for y in range(T):
            for x in range(T):
                put(img, x, y, TAR[1] if hash01(x, y, 821) < 0.85 else TAR[2])
        tar_patch(img, 4.0, 4.0, 4.6, 2.6, f, 822)
        tar_patch(img, 12.0, 11.0, 4.2, 2.8, f + 2, 823)
        tar_patch(img, 6.0 + variant * 4, 12.0 - variant * 4, 2.6, 1.6, f + 1, 824 + variant)
        bx, by = 9 - variant * 3, 6 + variant * 3
        if (f + variant) % STREAM_FRAMES == 1:
            for dx, dy in ((0, -1), (-1, 0), (1, 0), (0, 1)):
                put(img, bx + dx, by + dy, TAR[4])
            put(img, bx - 1, by - 1, SHEEN[3])
        elif (f + variant) % STREAM_FRAMES == 2:
            put(img, bx, by, SHEEN[2])
            put(img, bx + 1, by, TAR[3])
        frames.append(img)
    return frames


def tar_seep_edges():
    """Where the tar meets the floor: the stone's lip, stained black where it's crept up it, and a
    line of oil along it."""
    edges = []
    for side in range(4):
        img = tile()
        lit = side in (0, 3)
        for x in range(T):
            put(img, x, 0, QUARRY[5] if lit else QUARRY[3])
            put(img, x, 1, QUARRY[3] if hash01(x, side, 825) < 0.6 else TAR[3])
            put(img, x, 2, TAR[2])
            if hash01(x, side, 826) < 0.4:
                put(img, x, 3, SHEEN[0] if hash01(x, side, 827) < 0.5 else TAR[3])
        edges.append(turned(img, side))
    return edges


def ramp():
    """The ramp up out of the quarry, where the carts went: packed earth and grit with stones
    worked up through it, and the two ruts the wheels wore running up it."""
    img = tile()
    light = blobs(831, 5, 4.5, squash=1.6)
    for y in range(T):
        for x in range(T):
            m = light[y][x]
            k = 3 if m > 0.3 and dither(x, y, m) else 2
            if hash01(x, y, 832) < 0.07:
                k = 1
            put(img, x, y, EARTH[k])
    for x in (4, 11):
        for y in range(T):
            put(img, x, y, EARTH[1])
            put(img, x + 1, y, EARTH[4] if hash01(x, y, 833) < 0.4 else EARTH[2])
    for i in range(4):
        x, y = 1 + int(hash01(i, 0, 834) * 13), 1 + int(hash01(0, i, 834) * 13)
        put(img, x, y, PEBBLE[2])
        put(img, x + 1, y + 1, EARTH[0])
    return img


#: The rock the quarry's walls are left in, seen from above: darker than the floor, so the rooms
#: stand out from it.
WALL_ROCK = (QUARRY_GROUT, (42, 36, 52), (58, 50, 68), (76, 68, 86), (96, 88, 104))


def wall_top(variant):
    """The top of the quarry's wall, seen from above: the rock it's cut out of, in lumps packed
    together, darker than the floor, with a root across one from the wood overhead."""
    img = tile()
    rock_lumps(img, WALL_ROCK, 841, variant)
    if variant == 1:
        for x, y in wander(random.Random(842), 3, 7, 0.3, 9, 0.3):
            put(img, x, y, BARK[1])
            put(img, x, y + 1, QUARRY_GROUT)
    return img


#: The two beds of a wall's face under its lip, and the one over a wall's foot. The grooves are at
#: the same places on every one, so a wall has no seams.
FACE_BEDS = ((3, 9), (9, 16))
BASE_BEDS = ((0, 9),)


def lip(img):
    """The lip along the top of a cut face, where the floor above ends: a line of light, then its
    shadow, and a root or a tuft hanging over it."""
    for x in range(T):
        put(img, x, 0, QUARRY[5] if x % 9 else QUARRY[4])
        put(img, x, 1, QUARRY[3])
        put(img, x, 2, QUARRY[1])


def wall_face(variant):
    """The upper row of a wall's face: plain, cracked, or with damp coming down it and moss in the
    wet."""
    img = tile()
    strata(img, FACE_BEDS, 851, variant)
    lip(img)
    if variant == 1:
        for x, y in ((6, 3), (6, 4), (7, 5), (7, 6), (6, 7), (6, 8), (5, 9), (5, 10), (6, 11), (6, 12)):
            put(img, x, y, QUARRY[0])
            put(img, x + 1, y, QUARRY[5])
    if variant == 2:
        for y in range(3, T):
            for x in range(8, 11):
                if hash01(x, y // 2, 852) > 0.3:
                    put(img, x, y, shade(img[y][x][:3], 0.8))
        for x, y in ((9, 4), (8, 5), (9, 7), (10, 9), (9, 12), (8, 14)):
            put(img, x, y, MOSS[1])
            put(img, x, y - 1, MOSS[2])
    return img


def wall_roots():
    """A wall's face with roots coming down it out of the wood overhead: thick ones from the lip,
    thinning as they go, lit along their left, with a few going black where the rot's in them."""
    img = wall_face(0)
    rng = random.Random(861)
    for i, (rx, length) in enumerate(((2, 15), (6, 11), (9, 16), (13, 13))):
        points = wander(rng, rx, 0, math.pi / 2, length, 0.25)
        for j, (x, y) in enumerate(points):
            thick = j < len(points) * 0.6
            black = i == 2 and j > 5
            put(img, x, y, VEIN if black else BARK[4])
            if thick:
                put(img, x + 1, y, VEIN_GLOW if black else BARK[2])
                put(img, x + 2, y, QUARRY[1])
    return img


def wall_crystal():
    """A wall's face with the black crystal broken through it: a spray of black blades out of a
    split in the stone, their edges violet, and the stone round them lit violet by them."""
    img = wall_face(0)
    for y in range(3, T):
        for x in range(T):
            d = math.hypot(x + 0.5 - 8, (y + 0.5 - 10) * 1.2)
            if d < 7:
                img[y][x] = list(lerp_color(img[y][x][:3], VEIN_GLOW, 0.45 * (1 - d / 7))) + [255]
    blades = ((8, 14, 6, 3, -0.2), (5, 14, 4, 2, -0.7), (11, 14, 4, 2, 0.6), (9, 14, 3, 1, 1.2))
    for bx, by, h, w, lean in blades:
        for j in range(h * 2):
            t = j / (h * 2)
            cx = bx + math.sin(lean) * j * 0.5
            half = w * (1 - t) * 0.6 + 0.4
            yy = by - j * 0.5
            for x in range(int(cx - half), int(cx + half) + 1):
                u = (x + 0.5 - cx) / max(half, 0.5)
                put(img, x, int(yy), CRYSTAL[3] if u < -0.3 else CRYSTAL[1] if u < 0.4 else CRYSTAL[0])
        tip = (int(bx + math.sin(lean) * h), int(by - h))
        put(img, tip[0], tip[1], CRYSTAL_GLOW[2])
    for x, y in ((6, 9), (8, 7), (10, 10), (7, 12), (11, 8)):
        put(img, x, y, CRYSTAL_GLOW[1])
    put(img, 8, 6, CRYSTAL_GLOW[3])
    return img


def wall_base(variant):
    """The lower row of a wall's face: a bed of stone, then broken stone at its foot and the dark
    line where it meets the floor, roots or chips low down."""
    img = tile()
    strata(img, BASE_BEDS, 871, variant)
    for y in range(9, T):
        for x in range(T):
            if y == T - 1:
                put(img, x, y, QUARRY_GROUT)
                continue
            n = hash01(x, y, 872 + variant)
            k = 5 if y == 9 else 3 if y < 13 else 2
            if n < 0.12:
                k -= 1
            put(img, x, y, QUARRY[k])
    for x in range(T):
        if hash01(x, variant, 873) < 0.25:
            put(img, x, 14, BARK[2] if variant else MOSS[1])
            put(img, x, 13, BARK[3] if variant else MOSS[2])
    return img


def paint_rubble(c, variant):
    """A heap of broken stone off the cut faces: big lumps at the bottom, smaller on top, each lit
    on its top left, and chips round it."""
    rng = random.Random(881)
    lumps = [(rng.uniform(3, 13), rng.uniform(10.5, 12.5), rng.uniform(2.4, 3.1)) for _ in range(4)]
    lumps += [(rng.uniform(5, 11), rng.uniform(7.5, 9.5), rng.uniform(1.8, 2.5)) for _ in range(3)]
    lumps += [(rng.uniform(6, 10), rng.uniform(5.0, 6.5), rng.uniform(1.4, 1.9))]
    for cx, cy, r in lumps:
        base = 3 if rng.random() < 0.6 else 4
        phase = rng.uniform(0, 6.28)
        for y in range(T):
            for x in range(T):
                dx, dy = x + 0.5 - cx, (y + 0.5 - cy) * 1.25
                if lobed(dx, dy, r, 3, phase, 0.22) > 1.0:
                    continue
                light = -(dx * 0.7 + dy) / r
                k = base + 1 if light > 0.45 else base if light > -0.2 else base - 2
                c.put(x, y, QUARRY[max(0, min(6, k))])
    for x, y in ((1, 14), (14, 13), (2, 11), (13, 15), (8, 15)):
        c.put(x, y, QUARRY[3])
    return STONE_INK


def paint_gravel(c, variant):
    """Chips of stone lying about where the blocks were dressed."""
    for i in range(9 + variant * 3):
        x, y = 2 + int(hash01(i, variant, 891) * 12), 2 + int(hash01(variant, i, 892) * 12)
        w, h = 1 + (i % 3 == 0), 1 + (i % 4 == 1)
        for yy in range(h):
            for xx in range(w):
                c.put(x + xx, y + yy, QUARRY[5] if xx + yy == 0 else QUARRY[3])
    return QUARRY_GROUT


def paint_shards(c, variant):
    """Black crystal coming up through the floor: a few blades leaning out of a crack, their lit
    faces violet-grey and their edges glowing."""
    sets = (((6, 12, 6, 2.2, -0.3), (10, 13, 4, 1.6, 0.4), (4, 13, 3, 1.2, -0.9)),
            ((9, 12, 7, 2.4, 0.2), (5, 13, 3, 1.4, -0.6), (12, 13, 3, 1.2, 0.8)))[variant]
    for bx, by, h, w, lean in sets:
        for j in range(h * 2):
            t = j / (h * 2)
            cx = bx + math.sin(lean) * j * 0.5
            half = w * (1 - t) * 0.7 + 0.3
            for x in range(int(cx - half), int(cx + half) + 1):
                u = (x + 0.5 - cx) / max(half, 0.5)
                c.put(x, by - j * 0.5, CRYSTAL[4] if u < -0.4 else CRYSTAL[2] if u < 0.3 else CRYSTAL[0])
        c.put(bx + math.sin(lean) * h, by - h, CRYSTAL_GLOW[2])
        c.put(bx - 1, by - 1, CRYSTAL_GLOW[1])
    for x in range(3, 13):
        c.put(x, 13 + (x % 3 == 0), VEIN)
    return CRYSTAL_GLOW[0]


def detail_shards(variant):
    """The shards, outlined in a dull violet, with a glow round them."""
    c = Canvas(T, T)
    ink = paint_shards(c, variant)
    c.outline(ink)
    c.glow(8, 9, 7, CRYSTAL_GLOW[1], 60)
    return c.img


def paint_fungus(c, variant):
    """Fungus that glows: a cluster of little teal caps on a root, brightest on top."""
    caps = (((5, 9, 2.4), (9, 8, 1.8), (11, 11, 1.5), (6, 12, 1.2)),
            ((8, 7, 2.6), (4, 10, 1.6), (11, 10, 1.9), (9, 12, 1.2)))[variant]
    for y in range(11, 14):
        for x in range(2, 14):
            if abs(y - 12.5 - math.sin(x * 0.6) * 0.8) < 0.8:
                c.put(x, y, BARK[2])
    for cx, cy, r in caps:
        c.put(cx, cy + 1, STALK[1])
        c.put(cx, cy + 2, STALK[0])
        for y in range(int(cy - r), int(cy) + 1):
            for x in range(int(cx - r), int(cx + r) + 1):
                dx, dy = x + 0.5 - cx, y + 0.5 - cy
                if math.hypot(dx, dy * 1.1) > r or dy > 0.5:
                    continue
                light = -(dx * 0.7 + dy) / r
                c.put(x, y, GLOW_CAP[3] if light > 0.7 else GLOW_CAP[2] if light > 0.1 else GLOW_CAP[1])
    return GLOW_CAP[0]


def detail_fungus(variant):
    """The fungus, outlined in a deep teal, with its glow round it."""
    c = Canvas(T, T)
    ink = paint_fungus(c, variant)
    c.outline(ink)
    c.glow(8, 9, 7, GLOW_CAP[2], 80)
    return c.img


def paint_tools(c, variant):
    """What a quarryman left: a pick with its haft across the floor, and an iron wedge beside it."""
    for i in range(11):
        x, y = 2 + i, 12 - i * 0.6
        c.put(x, y, BOARD[3])
        c.put(x, y + 1, BOARD[1])
    for dx, dy, k in ((-2, -3, 2), (-1, -2, 2), (0, -1, 3), (1, 0, 3), (2, 0, 3), (3, 1, 2), (4, 2, 1),
                      (0, 0, 2), (1, 1, 1), (2, 1, 1)):
        c.put(12 + dx, 5 + dy, PEBBLE[k])
    for dx, dy, k in ((0, 0, 3), (1, 0, 2), (2, 0, 1), (0, 1, 2), (1, 1, 1), (0, 2, 1)):
        c.put(4 + dx, 4 + dy, PEBBLE[k])
    return STONE_INK


#: The quarry's tileset, in TilesetCatalog.Quarry's order.
QUARRY_TILES = (
    ("floor", quarry_floor(), None),
    ("floor-shadow", quarry_floor(quarry_shadow), None),
    ("black-vein-floor", quarry_floor(vein=True), None),
    ("root-floor", [root_floor(v) for v in range(2)], root_floor_edges()),
    ("tar-seep", flat(tar_seep_frames(v) for v in range(2)), tar_seep_edges()),
    ("ramp", [ramp()], None),
    ("wall-top", [wall_top(v) for v in range(2)], None),
    ("wall-face", [wall_face(v) for v in range(3)], None),
    ("wall-roots", [wall_roots()], None),
    ("wall-crystal", [wall_crystal()], None),
    ("wall-base", [wall_base(v) for v in range(2)], None),
    ("rubble", [detail(paint_rubble)(0)], None),
    ("gravel", [detail(paint_gravel)(v) for v in range(2)], None),
    ("roots", [detail(paint_roots)(v) for v in range(2)], None),
    ("shards", [detail_shards(v) for v in range(2)], None),
    ("fungus", [detail_fungus(v) for v in range(2)], None),
    ("tools", [detail(paint_tools)(0)], None),
)


# --------------------------------------------------------------------------- #
# The black-veined trees, cut off my trees sheet
# --------------------------------------------------------------------------- #
# The same cutter as the road's trees, then turned. Each leaf (a pixel whose hue is a green's, and
# saturated enough) has its hue pulled toward a grey olive, most of its colour taken out and some of
# its light, and the darkest go violet, so the crown keeps its shape and its light but looks sick.
# The bark goes a little greyer. Then the veins: black lines from the foot of the trunk up through
# the bark and out into the crown, wandering and branching, drawn only on the tree itself, each
# with its violet glow along its upper left and a bright knot where it branches. The one only
# turning keeps more of its green and has two short veins.

#: The trees: the name (Props/<name>.png), the box round it on my trees sheet, how many sheet
#: pixels an art pixel takes, and how far gone it is.
BLACKVEIN_CUTS = (
    ("tree-blackvein", (767, 17, 170, 222), 2.0, "rotten"),
    ("tree-blackvein-pale", (929, 10, 194, 232), 2.0, "rotten"),
    ("tree-blackvein-bare", (1454, 575, 67, 105), 1.4, "bare"),
    ("oak-turning", (598, 12, 180, 228), 2.0, "turning"),
)

#: How a tree's leaves are turned, by how far gone it is: the hue they're pulled toward (degrees),
#: how far, how much of their colour and their light they keep, and how dark a shade has to be to
#: go violet.
SICKNESS = {
    "rotten": (84, 0.8, 0.4, 0.84, 0.55),
    "bare": (84, 0.8, 0.4, 0.84, 0.55),
    "turning": (72, 0.4, 0.78, 0.94, 0.3),
}

#: The violet a rotten tree's shadows go.
SHADOW_VIOLET = (62, 46, 92)

#: How many veins each tree gets, how far up it they run as a share of its height, and how many
#: clumps of its leaves have gone black.
VEINS = {"rotten": (3, 0.8, 6), "bare": (3, 0.9, 0), "turning": (1, 0.4, 1)}

#: Where on its way up a vein branches, as shares of its length, and how far the branch runs.
BRANCHES = ((0.38, 0.38), (0.56, 0.34), (0.74, 0.28))

#: Leaves gone black: the colour they go, and the violet their edges catch.
BLACK_LEAF = (30, 22, 44)


def sicken(img, how):
    """A copy of a cut with its leaves turned and its bark greyed."""
    hue, pull, saturation, kept, dark = SICKNESS[how]
    out = [[list(px) for px in line] for line in img]
    for line in out:
        for x, px in enumerate(line):
            if not px[3]:
                continue
            h, s, v = colorsys.rgb_to_hsv(px[0] / 255, px[1] / 255, px[2] / 255)
            h *= 360
            if LEAF_HUES[0] <= h <= LEAF_HUES[1] and s >= LEAF_SATURATION:
                h = (h + (hue - h) * pull) / 360
                rgb = [round(c * 255) for c in colorsys.hsv_to_rgb(h, s * saturation, v * kept)]
                if v < dark:
                    rgb = list(lerp_color(rgb, SHADOW_VIOLET, (dark - v) / dark * 0.9))
                line[x] = rgb + [255]
            elif s > 0.15 and v > 0.2:
                line[x] = list(lerp_color(px[:3], BARK_GREY[how][0], BARK_GREY[how][1])) + [255]
    return out


#: The grey a tree's bark is pulled toward, and how far, by how far gone it is.
BARK_GREY = {"rotten": ((96, 84, 100), 0.3), "bare": ((60, 50, 74), 0.5), "turning": ((96, 84, 100), 0.1)}


def is_tree(img, x, y):
    """Whether a pixel of a cut is the tree itself and not its outline or the air round it."""
    if not (0 <= y < len(img) and 0 <= x < len(img[0])):
        return False
    px = img[y][x]
    return px[3] == 255 and sum(px[:3]) > 90


def vein_tree(img, how, seed):
    """Black veins up a tree from the foot of its trunk and out through its crown: each one runs up
    the trunk, two pixels wide there, then leans out into the crown, branching three times, and a
    few clumps of leaves near them have gone black."""
    count, reach, clumps = VEINS[how]
    h, w = len(img), len(img[0])
    rng = random.Random(seed)
    cores, knots = set(), set()

    def run(fx, fy, heading, length, wide, lean, branches):
        for step in range(int(length)):
            t = step / length
            x, y = int(fx), int(fy)
            for k in range(2 if wide and t < 0.4 else 1):
                if is_tree(img, x + k, y):
                    cores.add((x + k, y))
            for at, share in branches:
                if step == int(length * at):
                    knots.add((x, y))
                    side = 1 if rng.random() < 0.5 else -1
                    run(fx, fy, heading + side * rng.uniform(0.7, 1.0), length * share, False, side * 0.04, ())
            heading += rng.uniform(-0.18, 0.18) + (lean if t > 0.35 else 0.0)
            fx += math.cos(heading)
            fy += math.sin(heading)

    for n in range(count):
        spread = n - (count - 1) / 2
        run(w / 2 + spread * 3, h - 5, -math.pi / 2 + spread * 0.08, h * reach * rng.uniform(0.85, 1.0), True,
            spread * 0.025, BRANCHES)

    out = [[list(px) for px in line] for line in img]
    leafy = [(x, y) for y in range(int(h * 0.15), int(h * 0.65)) for x in range(w) if is_tree(img, x, y)]
    for n in range(clumps):
        cx, cy = leafy[int(hash01(n, seed, 905) * len(leafy))]
        r = 2.5 + hash01(seed, n, 906) * 2
        for y in range(int(cy - r) - 1, int(cy + r) + 2):
            for x in range(int(cx - r) - 1, int(cx + r) + 2):
                d = lobed(x - cx, y - cy, r, 4, n, 0.25)
                if d < 1 and is_tree(img, x, y):
                    edge = d > 0.65 and (x - cx) + (y - cy) < 0
                    out[y][x] = list(lerp_color(BLACK_LEAF, VEIN_GLOW, 0.5) if edge else BLACK_LEAF) + [255]
    for x, y in cores:
        if (x - 1, y) not in cores and is_tree(img, x - 1, y):
            out[y][x - 1] = list(lerp_color(out[y][x - 1][:3], VEIN_GLOW, 0.45)) + [255]
    for x, y in cores:
        out[y][x] = list(VEIN) + [255]
    for x, y in knots:
        if (x, y) in cores:
            out[y][x] = list(VEIN_HOT) + [255]
    return out


def blackvein_trees():
    """Every black-veined tree, by name."""
    trees = {}
    for i, (name, box, pitch, how) in enumerate(BLACKVEIN_CUTS):
        img = cut_out(TREES_SHEET, box, pitch=pitch)
        trees[name] = vein_tree(sicken(img, how), how, 901 + i)
    return trees


# --------------------------------------------------------------------------- #
# The Mirewood's drawn props: the tar pools, the mushrooms, the rangers' sign, the woodcutters'
# stump, the bridge, the quarry's mouth and the bones
# --------------------------------------------------------------------------- #
# Drawn to match my props sheet and the road's: lit from the top left, a dark outline round
# everything, wood in the road's browns.

TAR_POOL_W, TAR_POOL_H = 46, 24


def tar_pool():
    """A pool of tar lying in the ground, glossy and black: the earth round it stained and wet, its
    surface lit along its top left with the oil's colours swirled on it, a bubble or two, and the
    sky's pale glint."""
    c = Canvas(TAR_POOL_W, TAR_POOL_H)
    cx, cy, rx, ry = 23.0, 12.0, 21.0, 10.0
    for y in range(TAR_POOL_H):
        for x in range(TAR_POOL_W):
            dx, dy = (x + 0.5 - cx) / rx, (y + 0.5 - cy) / ry
            d = lobed(dx, dy, 1.0, 5, 0.9, 0.12)
            if d > 1.0:
                continue
            if d > 0.86:
                c.put(x, y, EARTH[1] if hash01(x, y, 1001) < 0.5 else SOIL_ROT[1])
                continue
            if d > 0.78:
                c.put(x, y, TAR[2] if dx + dy < 0 else TAR[1])
                continue
            light = -(dx * 0.6 + dy)
            k = 3 if light > 0.55 and d > 0.5 else 1 if d > 0.4 else 0
            c.put(x, y, TAR[k])
            swirl = math.sin(dx * 7.0 + dy * 3.0 + math.atan2(dy, dx) * 2.0)
            if 0.35 < d < 0.72 and abs(swirl) < 0.12:
                c.put(x, y, SHEEN[0] if dx < 0.1 else SHEEN[1])
    for bx, by, r in ((30, 9, 1.6), (15, 14, 1.2)):
        for y in range(int(by - r) - 1, int(by + r) + 2):
            for x in range(int(bx - r) - 1, int(bx + r) + 2):
                d = math.hypot(x + 0.5 - bx, y + 0.5 - by)
                if abs(d - r) < 0.6:
                    c.put(x, y, TAR[4] if y < by else TAR[3])
    for x, y, col in ((12, 7, SHEEN[3]), (13, 7, SHEEN[2]), (11, 8, SHEEN[2]), (31, 13, SHEEN[4])):
        c.put(x, y, col)
    c.outline(TAR_INK)
    return c.img


MUSHROOM_W, MUSHROOM_H = 28, 30

#: The gloom mushrooms: where each one's cap sits, its half-width, how tall its stalk is, and
#: whether it's an ink cap. Back to front.
GLOOM_CLUSTER = ((9.0, 9.0, 4.6, 14, True), (19.0, 13.0, 3.6, 11, True), (14.0, 18.0, 3.8, 7, False),
                 (5.0, 21.0, 2.8, 5, False), (22.5, 22.0, 2.4, 4, False), (11.0, 24.0, 1.8, 3, False))


def gloom_cap(c, mx, my, r, stalk, ink):
    """One mushroom of a cluster: a stalk, and a cap lit on its top left. An ink cap is a tall
    black bell with its rim dripping; a pale one is a low dome that glows a little."""
    for y in range(int(my), int(my) + stalk + 1):
        for x in range(int(mx - 1), int(mx + 1)):
            c.put(x, y, STALK[2] if x < mx - 0.5 else STALK[1] if y < my + stalk else STALK[0])
    caps = INK_CAP if ink else PALE_CAP
    tall = 1.5 if ink else 0.8
    for y in range(int(my - r * tall) - 1, int(my) + 2):
        for x in range(int(mx - r) - 1, int(mx + r) + 2):
            dx, dy = x + 0.5 - mx, (y + 0.5 - my) / tall
            if math.hypot(dx, dy) > r or dy > 0.6:
                continue
            light = -(dx * 0.7 + dy) / r
            col = caps[3] if light > 0.7 else caps[2] if light > 0.2 else caps[1] if light > -0.4 else caps[0]
            if y >= int(my):
                col = caps[0]
            c.put(x, y, col)
    if ink:
        for x in range(int(mx - r) + 1, int(mx + r), 2):
            for d in range(1 + int(hash01(x, int(my), 1011) * 3)):
                c.put(x, int(my) + 1 + d, INK_CAP[1] if d else INK_CAP[0])


def gloom_mushroom():
    """A cluster of gloom mushrooms as tall as your knee: ink caps standing over pale ones, with a
    faint pale glow round the pale ones."""
    c = Canvas(MUSHROOM_W, MUSHROOM_H)
    for mx, my, r, stalk, ink in GLOOM_CLUSTER:
        gloom_cap(c, mx, my, r, stalk, ink)
    for x in range(3, 25):
        if hash01(x, 0, 1012) < 0.6:
            c.put(x, MUSHROOM_H - 2, GLOOM[2])
            c.put(x, MUSHROOM_H - 1, GLOOM[1])
    c.outline(ROT_INK)
    for mx, my, r, stalk, ink in GLOOM_CLUSTER:
        if not ink:
            c.glow(mx, my - 1, r + 4, PALE_CAP[3], 70)
    return c.img


def gloom_mushroom_small():
    """A few gloom mushrooms: one ink cap and two pale ones."""
    c = Canvas(18, 16)
    for mx, my, r, stalk, ink in ((6.0, 5.0, 2.8, 7, True), (12.0, 9.0, 2.6, 4, False), (4.0, 12.0, 1.8, 2, False)):
        gloom_cap(c, mx, my, r, stalk, ink)
    c.outline(ROT_INK)
    c.glow(12, 8, 6, PALE_CAP[3], 70)
    return c.img


#: The rangers' ribbon, dark to light: the Lodge's green.
RIBBON = ((34, 82, 52), (52, 126, 70), (92, 170, 92), (150, 214, 130))

RANGER_SIGN_W, RANGER_SIGN_H = 26, 38


def ranger_sign():
    """The rangers' waymark: a stout post with a pair of antlers fixed to its top, a board across
    it with the Lodge's leaf burnt into it, an arrow board under that pointing east, and a green
    ribbon tied round the post. Moss and grass round its foot."""
    c = Canvas(RANGER_SIGN_W, RANGER_SIGN_H)
    mid = 12
    for y in range(6, RANGER_SIGN_H):
        c.put(mid, y, BARK[4])
        c.put(mid + 1, y, BARK[3] if y % 6 else BARK[2])
        c.put(mid + 2, y, BARK[1])
    for side in (-1, 1):
        base = mid + 1
        for i, (dx, dy) in enumerate(((1, -1), (2, -2), (3, -2), (4, -3), (5, -4), (5, -5), (6, -6))):
            c.put(base + side * dx, 6 + dy, BONE[3] if dy < -3 else BONE[2])
        for dx, dy in ((3, -3), (3, -4), (2, -5)):
            c.put(base + side * dx, 6 + dy, BONE[2])
    for y in range(9, 17):
        for x in range(2, RANGER_SIGN_W - 2):
            k = 4 if y == 9 else 1 if y == 16 else 3 if y == 10 else 2
            if hash01(x // 4, y, 1021) < 0.12:
                k = max(1, k - 1)
            c.put(x, y, BOARD[k])
    leaf_mark = ((12, 11), (13, 11), (11, 12), (12, 12), (13, 12), (14, 12), (11, 13), (12, 13), (13, 13),
                 (14, 13), (12, 14), (13, 14), (13, 15))
    for x, y in leaf_mark:
        c.put(x, y, BOARD[0])
    for x in (4, 21):
        c.put(x, 12, NAIL)
    for y in range(20, 25):
        for x in range(6, RANGER_SIGN_W - 1):
            tip = abs(y - 22)
            if x > RANGER_SIGN_W - 3 - tip:
                continue
            c.put(x, y, BOARD[4] if y == 20 else BOARD[1] if y == 24 else BOARD[3] if y == 21 else BOARD[2])
    for y in range(26, 30):
        c.put(mid - 1, y, RIBBON[2] if y < 28 else RIBBON[1])
        c.put(mid, y, RIBBON[3] if y == 26 else RIBBON[2])
        c.put(mid + 1, y, RIBBON[1])
        c.put(mid + 2, y, RIBBON[1])
        c.put(mid + 3, y, RIBBON[0])
    for x, y in ((mid - 2, 29), (mid - 3, 30), (mid - 3, 31), (mid - 2, 32)):
        c.put(x, y, RIBBON[2])
    for x, h in ((8, 2), (9, 3), (10, 1), (16, 2), (17, 3), (18, 1)):
        for i in range(h):
            c.put(x, RANGER_SIGN_H - 1 - i, LUSH[3] if i == h - 1 else LUSH[2])
    c.put(mid, RANGER_SIGN_H - 3, MOSS[2])
    c.put(mid + 1, RANGER_SIGN_H - 2, MOSS[3])
    c.outline(WOOD_INK)
    return c.img


STUMP_W, STUMP_H = 44, 32


def woodcutter_stump():
    """A big oak sawn off at the knee, its rings showing on top with the black come up into them,
    an axe buried in it where somebody left it, a wedge of wood split off lying beside it, and the
    chips all round."""
    c = Canvas(STUMP_W, STUMP_H)
    cx, top, rx, ry, bottom = 18.0, 10.0, 12.0, 5.0, 27
    for y in range(int(top), bottom + 1):
        for x in range(int(cx - rx) - 2, int(cx + rx) + 3):
            flare = 1.0 + max(0, y - 22) * 0.25
            u = (x + 0.5 - (cx - rx * flare)) / (2 * rx * flare)
            if 0 <= u <= 1:
                c.put(x, y, bark_at(u, hash01(x // 3, y, 1031)))
    for side, (x0, y0) in ((-1, (cx - rx, 25)), (1, (cx + rx, 25))):
        for i in range(5):
            c.put(x0 + side * i, y0 + i // 2, BARK[3] if i < 2 else BARK[2])
            c.put(x0 + side * i, y0 + 1 + i // 2, BARK[1])
    rings(c, cx, top, rx, ry, 1032)
    rng = random.Random(1033)
    for n in range(3):
        sx, sy = int(cx + rng.uniform(-rx * 0.6, rx * 0.6)), int(top + ry * 0.6)
        points = wander(rng, sx, sy, -math.pi / 2 + rng.uniform(-1.2, 1.2), 6, 0.5)
        for x, y in points:
            if math.hypot((x + 0.5 - cx) / rx, (y + 0.5 - top) / ry) < 0.9:
                c.put(x, y, VEIN)
                c.put(x - 1, y, VEIN_GLOW)
    for i in range(11):
        c.put(cx + 3 + i, top - 2 - i * 0.8, BOARD[4])
        c.put(cx + 4 + i, top - 2 - i * 0.8, BOARD[2])
    for dx, dy, k in ((-3, 0, 3), (-2, 0, 3), (-1, 0, 3), (0, 0, 2), (1, 0, 2), (2, 0, 2), (3, 0, 1), (-3, 1, 2),
                      (-2, 1, 2), (-1, 1, 1), (0, 1, 1), (1, 1, 1), (2, 1, 0), (-2, -1, 3), (-1, -1, 3), (0, -1, 2),
                      (1, -1, 2), (2, -1, 1), (-1, -2, 2), (0, -2, 2), (1, -2, 1)):
        c.put(cx + 2 + dx, top - 1 + dy, PEBBLE[k])
    c.line(cx - 1, top + 1, cx + 5, top + 1, (40, 30, 30))
    for y in range(21, 28):
        for x in range(32, 42):
            if (x - 32) + (27 - y) * 1.3 < 11 and (42 - x) > (27 - y) * 0.4:
                c.put(x, y, HEARTWOOD[2] if y < 23 else BARK[3] if x > 39 else HEARTWOOD[1])
    for i in range(14):
        x, y = 2 + int(hash01(i, 1, 1034) * 40), 27 + int(hash01(1, i, 1035) * 4)
        c.put(x, y, HEARTWOOD[3] if i % 2 else HEARTWOOD[2])
        c.put(x + 1, y, HEARTWOOD[1])
    c.outline(WOOD_INK)
    return c.img


BRIDGE_W, BRIDGE_H = 72, 46

#: Where the bridge's deck runs, top and bottom, in its picture, and where its rails' posts stand.
BRIDGE_DECK = (10, 42)
BRIDGE_POSTS = (5, 66)


def ranger_bridge():
    """The rangers' footbridge over the stream, seen from above: planks laid across two beams, a
    stone at each end it rests on, a post at each corner and a rope strung between them along each
    side, sagging, the far one in the near one's light."""
    c = Canvas(BRIDGE_W, BRIDGE_H)
    top, bottom = BRIDGE_DECK
    for side in (0, 1):
        x0 = 0 if side == 0 else BRIDGE_W - 9
        for sy in range(top - 2, bottom + 3, 6):
            pebble(c, x0 + (sy * 3) % 3, sy, 8, 6, PEBBLE)
    for x in range(6, BRIDGE_W - 6):
        plank = (x - 6) // 4
        edge = (x - 6) % 4
        k = 4 if edge == 0 else 1 if edge == 3 else 3 if hash01(plank, 0, 1041) < 0.5 else 2
        for y in range(top + (1 if plank % 3 == 0 else 0), bottom - (1 if plank % 4 == 1 else 0)):
            kk = k - 1 if hash01(x, y // 3, 1042) < 0.1 else k
            c.put(x, y, BOARD[max(0, kk)])
        for y in (top + 3, bottom - 4):
            c.put(x, y, NAIL if edge == 1 else BOARD[max(0, k - 1)])
    for px in BRIDGE_POSTS:
        for py in (top - 6, bottom - 4):
            for y in range(py, py + 9):
                c.put(px, y, BARK[4])
                c.put(px + 1, y, BARK[2])
            c.put(px, py - 1, BARK[5])
            c.put(px + 1, py - 1, BARK[3])
    for py, hue in ((top - 5, 0), (bottom - 3, 1)):
        for x in range(BRIDGE_POSTS[0] + 2, BRIDGE_POSTS[1]):
            t = (x - BRIDGE_POSTS[0]) / (BRIDGE_POSTS[1] - BRIDGE_POSTS[0])
            y = py + int(4 * 4 * t * (1 - t))
            c.put(x, y, (214, 180, 124) if hue else (176, 146, 100))
            c.put(x, y + 1, (120, 88, 66))
    c.outline(WOOD_INK)
    return c.img


MOUTH_W, MOUTH_H = 72, 60

#: The quarry's mouth: its two posts (left edge of each), how wide they are, where the beam over
#: them is, and where its dark starts and stops.
MOUTH_POSTS = (10, 54)
MOUTH_POST_W = 8
MOUTH_LINTEL = (8, 16)


def quarry_mouth():
    """The way into the old quarry, where the track goes into the rock: two squared posts and a
    beam over them holding the stone up, the dark beyond, roots and black veins crawling out over
    the beam, a board nailed across one post, an old lantern hanging off the beam with no light in
    it, and tar trickling out over the sill into the track."""
    c = Canvas(MOUTH_W, MOUTH_H)
    l0, l1 = MOUTH_LINTEL
    inner0, inner1 = MOUTH_POSTS[0] + MOUTH_POST_W, MOUTH_POSTS[1]
    for y in range(l1, MOUTH_H):
        for x in range(inner0, inner1):
            depth = (y - l1) / (MOUTH_H - l1)
            col = lerp_color((4, 3, 8), (34, 26, 40), max(0.0, depth - 0.45) * 1.6)
            if y >= MOUTH_H - 4 and hash01(x, y, 1051) < 0.6:
                col = TAR[2] if abs(x - 36) < 4 else EARTH[0]
            c.put(x, y, col)
    for px in MOUTH_POSTS:
        for y in range(l0, MOUTH_H):
            for x in range(px, px + MOUTH_POST_W):
                u = (x - px) / (MOUTH_POST_W - 1)
                k = 4 if u < 0.2 else 3 if u < 0.5 else 2 if u < 0.8 else 1
                if hash01(x, y // 4, 1052) < 0.12:
                    k -= 1
                c.put(x, y, BARK[max(0, k)])
    for y in range(l0, l1):
        for x in range(MOUTH_POSTS[0] - 4, MOUTH_POSTS[1] + MOUTH_POST_W + 4):
            k = 5 if y == l0 else 4 if y == l0 + 1 else 1 if y == l1 - 1 else 3 if hash01(x // 5, y, 1053) < 0.6 else 2
            c.put(x, y, BARK[k])
    for x in range(MOUTH_POSTS[1] + 1, MOUTH_POSTS[1] + MOUTH_POST_W - 1):
        for y in range(28, 32):
            c.put(x - 6, y, BOARD[3] if y == 28 else BOARD[2] if y < 31 else BOARD[1])
    rng = random.Random(1054)
    for n, (sx, heading) in enumerate(((6, 1.9), (20, 1.4), (44, 1.7), (62, 1.2), (30, 1.6))):
        points = wander(rng, sx, l0 - 2, heading, 10 + n * 2, 0.35)
        black = n in (1, 4)
        for x, y in points:
            c.put(x, y, VEIN if black else BARK[3])
            c.put(x + 1, y, VEIN_GLOW if black else BARK[1])
    hx = 28
    c.line(hx, l1, hx, l1 + 4, (70, 68, 84))
    for dy in range(6):
        for dx in range(-2, 3):
            c.put(hx + dx, l1 + 5 + dy, (54, 52, 68) if abs(dx) == 2 or dy in (0, 5) else (40, 34, 48))
    for y in range(MOUTH_H - 6, MOUTH_H):
        c.put(36, y, TAR[0])
        c.put(37, y, TAR[2])
    c.outline(WOOD_INK)
    return c.img


BONES_W, BONES_H = 26, 14


def bones():
    """What's left of something that went into the quarry: an animal's skull with its long jaw,
    two long bones and a few ribs, lying where they fell."""
    c = Canvas(BONES_W, BONES_H)
    bone(c, 2, 10, 11, 8)
    bone(c, 13, 12, 22, 11)
    for i in range(4):
        x = 4 + i * 3
        for j in range(4):
            c.put(x + j // 2, 3 + j, BONE[2] if j < 2 else BONE[1])
    rows = ("..3332..", ".333322.", "3303322.", ".3322111", "..22.11.")
    for dy, row in enumerate(rows):
        for dx, ch in enumerate(row):
            if ch != ".":
                c.put(16 + dx, 2 + dy, (14, 12, 20) if ch == "0" else BONE[int(ch)])
    c.outline(STONE_INK)
    return c.img


def mirewood_drawn():
    """The Mirewood's drawn props, by name."""
    return {
        "tar-pool": tar_pool(),
        "gloom-mushroom": gloom_mushroom(),
        "gloom-mushroom-small": gloom_mushroom_small(),
        "ranger-sign": ranger_sign(),
        "woodcutter-stump": woodcutter_stump(),
        "ranger-bridge": ranger_bridge(),
        "quarry-mouth": quarry_mouth(),
        "bones": bones(),
    }


# --------------------------------------------------------------------------- #
# The quarry's props: the root walls, the black crystal three ways, the courier and his satchel
# --------------------------------------------------------------------------- #

ROOT_WALL_W, ROOT_WALL_H = 32, 40

#: The roots of a root wall, back to front: where each starts, the point it bends toward, where it
#: ends, how thick it is at its thickest, and whether the black's in it. Down from the top first,
#: then across from the sides, woven over them.
ROOT_RUNS = (
    ((3, 0), (7, 20), (2, 39), 3, False), ((9, 0), (4, 18), (9, 39), 3, False),
    ((14, 0), (17, 22), (13, 39), 4, False), ((19, 0), (15, 20), (19, 39), 3, True),
    ((24, 0), (28, 18), (23, 39), 4, False), ((29, 0), (25, 22), (29, 39), 3, False),
    ((0, 5), (16, 16), (31, 30), 3, False), ((31, 3), (14, 14), (0, 28), 3, True),
    ((0, 20), (15, 27), (31, 38), 2, False), ((31, 17), (18, 27), (1, 37), 3, False),
    ((0, 11), (16, 7), (31, 12), 2, False), ((0, 31), (16, 28), (31, 33), 2, True),
)

#: The same roots drawn back when the wall's parted: three bundles down each side, and two arched
#: over the top of the way through.
ROOT_PARTED = (
    ((2, 0), (6, 20), (1, 39), 3, False), ((6, 0), (2, 18), (6, 39), 3, True),
    ((9, 2), (5, 22), (3, 39), 2, False), ((29, 0), (25, 20), (30, 39), 3, False),
    ((25, 0), (29, 18), (25, 39), 3, False), ((22, 2), (26, 22), (28, 39), 2, True),
    ((1, 6), (16, -6), (30, 6), 3, False), ((3, 11), (16, 1), (28, 11), 2, False),
)


def root_curve(c, start, bend, end, thick, black, seed):
    """One root from where it starts to where it ends, bending toward a point on the way, thickest
    in its middle, lit along its top and left and dark along its bottom and right, with a thorn off
    it here and there. A root with the black in it is black, glowing violet along its lit side."""
    (x0, y0), (bx, by), (x1, y1) = start, bend, end
    steps = int(math.hypot(x1 - x0, y1 - y0) * 1.6) + 4
    upright = abs(y1 - y0) > abs(x1 - x0)
    for i in range(steps + 1):
        t = i / steps
        x = (1 - t) ** 2 * x0 + 2 * (1 - t) * t * bx + t * t * x1
        y = (1 - t) ** 2 * y0 + 2 * (1 - t) * t * by + t * t * y1
        w = max(1, round(thick * (0.55 + 0.45 * math.sin(math.pi * t))))
        for k in range(w):
            u = k / max(1, w - 1)
            if black:
                col = VEIN_GLOW if k == 0 and w > 1 else VEIN
            else:
                col = BARK[4] if u < 0.3 else BARK[3] if u < 0.7 else BARK[1]
            if upright:
                c.put(x + k - w // 2, y, col)
            else:
                c.put(x, y + k - w // 2, col)
        if i % 9 == 4 and hash01(i, seed, 1064) < 0.6:
            c.put(x + (w // 2 + 1 if upright else 0), y - (0 if upright else w // 2 + 1),
                  CRYSTAL_GLOW[1] if black else BONE[1])


def root_wall():
    """Roots grown right across a way through, too thick to get past: thick ones down from the top
    and across from the sides, woven over each other, thorny, the black in some of them, dark leaves
    caught in them, and the dark showing between. Then the same roots drawn back to either side
    the way Sera parts them, arched over the way through and leaving it open."""
    img = new_image(ROOT_WALL_W * 2, ROOT_WALL_H)
    for f, runs in enumerate((ROOT_RUNS, ROOT_PARTED)):
        c = Canvas(ROOT_WALL_W, ROOT_WALL_H)
        for y in range(1, ROOT_WALL_H):
            for x in range(ROOT_WALL_W):
                if f == 1 and 8 <= x < ROOT_WALL_W - 8:
                    continue
                inside = abs(x + 0.5 - ROOT_WALL_W / 2) / (ROOT_WALL_W / 2)
                c.put(x, y, lerp_color((10, 6, 14), EARTH[0], inside * 0.7))
        for i, (start, bend, end, thick, black) in enumerate(runs):
            root_curve(c, start, bend, end, thick, black, 1061 + i)
        for i in range(12 if f == 0 else 6):
            x = int(hash01(i, f, 1062) * (ROOT_WALL_W - 4)) + 2
            y = int(hash01(f, i, 1063) * (ROOT_WALL_H - 10)) + 4
            if f == 1 and 8 < x < ROOT_WALL_W - 9:
                continue
            for dx, dy, col in ((0, 0, ROT[4]), (1, 0, ROT[3]), (0, 1, ROT[2]), (1, 1, GLOOM[1])):
                c.put(x + dx, y + dy, col)
        c.outline(WOOD_INK)
        paste(img, c.img, f * ROOT_WALL_W, 0)
    return img


CRYSTAL_W, CRYSTAL_H = 44, 68

#: The crystal's blades: where each one's foot is, where its tip is, and how wide it is at its
#: foot. The big one first, then the ones round it, back to front.
CRYSTAL_BLADES = (
    ((22.0, 60.0), (23.0, 3.0), 9.5),
    ((14.0, 60.0), (6.0, 28.0), 5.0),
    ((30.0, 60.0), (38.0, 34.0), 4.6),
    ((17.0, 62.0), (12.0, 46.0), 3.4),
    ((27.0, 62.0), (31.0, 48.0), 3.2),
)

#: The ring of broken ground round its foot: its middle, and how far it reaches across and down.
CRYSTAL_RING = (22.0, 60.0, 20.0, 6.5)


def crystal_ground(c, clean):
    """The ground the crystal's driven into: stone cracked in a ring round it, scorched dark in the
    middle with veins running out from it; or, made clean, pale stone with white flowers come up in
    the cracks."""
    cx, cy, rx, ry = CRYSTAL_RING
    for y in range(int(cy - ry), CRYSTAL_H):
        for x in range(CRYSTAL_W):
            dx, dy = (x + 0.5 - cx) / rx, (y + 0.5 - cy) / ry
            d = lobed(dx, dy, 1.0, 6, 0.4, 0.15)
            if d > 1.0:
                continue
            k = 4 if dy < -0.3 else 3 if dy < 0.4 else 2
            col = QUARRY[k] if hash01(x, y, 1071) > 0.1 else QUARRY[k - 1]
            if not clean:
                col = lerp_color(col, TAR[1], max(0.0, 0.8 - d) * 1.1)
            c.put(x, y, col)
    rng = random.Random(1072)
    for n in range(7):
        heading = n * 0.9 + 0.2 + rng.uniform(-0.2, 0.2)
        for i, (x, y) in enumerate(wander(rng, cx, cy, heading, 14, 0.4)):
            dx, dy = (x + 0.5 - cx) / rx, (y + 0.5 - cy) / ry
            if math.hypot(dx, dy) > 0.95 or y < cy - ry:
                break
            if clean:
                if i in (6, 11):
                    for ox, oy in ((0, -1), (-1, 0), (1, 0), (0, 1)):
                        c.put(x + ox, y + oy, WILDFLOWERS[0])
                    c.put(x, y, FLOWER_MIDDLE[0])
                elif i < 6:
                    c.put(x, y, QUARRY[1])
            else:
                c.put(x, y, VEIN)
                c.put(x, y - 1, CRYSTAL_GLOW[0] if i % 3 else CRYSTAL_GLOW[1])


def blade(c, foot, tip, width, ramp, glow, seed, broken=None):
    """One blade of crystal from its foot to its tip, faceted: a ridge down its middle, the face on
    the ridge's left lit and the one on its right dark, its edges glowing, and a crack or two of
    light inside it. broken stops it partway up, with a jagged top."""
    (fx, fy), (tx, ty) = foot, tip
    length = math.hypot(tx - fx, ty - fy)
    ax, ay = (tx - fx) / length, (ty - fy) / length
    stop = length * broken if broken else length
    for y in range(CRYSTAL_H):
        for x in range(CRYSTAL_W):
            px, py = x + 0.5 - fx, y + 0.5 - fy
            along = px * ax + py * ay
            across = -px * ay + py * ax
            if along < -2 or along > stop:
                continue
            t = max(0.0, along) / length
            half = width * (1.0 - t) ** 0.9 + 0.4
            if broken and along > stop - 3 and hash01(x, 0, seed) * 3 < along - (stop - 3):
                continue
            if abs(across) > half:
                continue
            u = across / half
            if abs(u) > 0.78:
                col = glow[1] if u < 0 else glow[0]
            elif u < -0.05:
                col = ramp[4] if u < -0.45 else ramp[3]
            elif u < 0.12:
                col = ramp[2]
            else:
                col = ramp[1] if u < 0.5 else ramp[0]
            c.put(x, y, col)
    rng = random.Random(seed)
    for n in range(2):
        a0 = rng.uniform(0.2, 0.6) * stop
        for i in range(int(stop * 0.25)):
            along = a0 + i
            if along > stop - 2:
                break
            off = math.sin(i * 0.9 + n) * width * 0.3
            x, y = fx + ax * along - ay * off, fy + ay * along + ax * off
            c.put(x, y, glow[2] if i % 4 else glow[3])
    if not broken:
        c.put(tx, ty + 1, glow[3])


def black_crystal():
    """The spike of black crystal driven into the quarry's floor: a great blade with smaller ones
    round its foot, black, faceted, its edges and the cracks inside it blazing violet, the stone
    cracked and scorched round it with veins running out, and its glow round it all."""
    c = Canvas(CRYSTAL_W, CRYSTAL_H)
    crystal_ground(c, clean=False)
    for i, (foot, tip, width) in enumerate(CRYSTAL_BLADES):
        blade(c, foot, tip, width, CRYSTAL, CRYSTAL_GLOW, 1081 + i)
    c.outline(CRYSTAL[0])
    c.glow(22, 30, 24, CRYSTAL_GLOW[1], 70)
    return c.img


def black_crystal_shattered():
    """The crystal smashed: the great blade broken off low with a jagged top, the small ones
    snapped, the pieces scattered over the cracked ground, black gone dull, the violet gone out of
    all but a glint or two."""
    c = Canvas(CRYSTAL_W, CRYSTAL_H)
    crystal_ground(c, clean=False)
    dull = (CRYSTAL[0], CRYSTAL[1], CRYSTAL[2], CRYSTAL[3], (70, 60, 86))
    dim = ((40, 30, 56), (64, 50, 86), (110, 80, 150), (150, 110, 190))
    for i, (foot, tip, width) in enumerate(CRYSTAL_BLADES):
        blade(c, foot, tip, width, dull, dim, 1091 + i, broken=(0.32, 0.4, 0.45, 0.5, 0.5)[i])
    for x, y, w, h in ((4, 56, 4, 2), (36, 58, 5, 2), (9, 63, 3, 2), (31, 64, 4, 2), (19, 65, 3, 1), (40, 62, 2, 2)):
        for yy in range(h):
            for xx in range(w):
                c.put(x + xx, y + yy, dull[3] if yy == 0 and xx < w - 1 else dull[1])
        c.put(x, y, dim[2])
    c.outline(CRYSTAL[0])
    return c.img


def black_crystal_purified():
    """The crystal made clean: the same spike gone white and gold, its edges and its cracks warm
    light, the ground round it pale again with white flowers come up in the cracks, and a soft gold
    glow round it all."""
    c = Canvas(CRYSTAL_W, CRYSTAL_H)
    crystal_ground(c, clean=True)
    shine = ((214, 176, 96), (244, 214, 140), (255, 246, 210), (255, 255, 246))
    for i, (foot, tip, width) in enumerate(CRYSTAL_BLADES):
        blade(c, foot, tip, width, PURE, shine, 1101 + i)
    c.outline(PURE[0])
    c.glow(22, 30, 24, (255, 236, 180), 80)
    return c.img


COURIER_W, COURIER_H = 42, 20

#: The courier's robes, Doomward black going violet, dark to light; their dull red trim; the ash
#: and the embers where they're burnt; his hands; and his leggings.
ROBE = ((40, 32, 50), (62, 52, 76), (88, 76, 104), (122, 108, 140))
TRIM = ((96, 30, 36), (140, 48, 50))
ASH = ((92, 88, 96), (140, 136, 140))
LEGGINGS = ((34, 28, 40), (50, 42, 58), (66, 58, 76), (84, 76, 94))
EMBER = (232, 120, 52)
HAND = ((150, 120, 112), (196, 168, 150))


#: How far the courier's outline is from the line down his middle at each column, head to boots:
#: his hood, his shoulders, his back narrowing to his belt, his robe spread round his hips. His legs
#: and his arms are drawn on their own.
COURIER_MIDDLE = 10.5
COURIER_HOOD = (7.5, 10.0, 4.8)
COURIER_TORSO = ((11, 5.6), (14, 6.0), (18, 5.4), (22, 4.6), (24, 4.8), (28, 5.8), (31, 6.0))

#: His legs (from his robe's hem to his ankles), and his boots, as boxes: left, top, right, bottom.
COURIER_LEGS = ((31, 6, 36, 9), (31, 12, 36, 15))
COURIER_BOOTS = ((36, 5, 40, 9), (36, 12, 40, 16))

#: His arms: the far one flung out over his head, the near one under him along his side, each from
#: his shoulder to his wrist; his hand's at the end.
COURIER_ARMS = (((13, 5), (8, 2), (3, 2)), ((14, 15), (18, 17), (23, 17)))


def courier_body():
    """A Doomward courier lying face down where he fell, head to the left in his hood, one arm
    flung out past his head and the other under him: robes of Doomward charcoal with a red trim and
    a red sash, charred black along the hem with ash on them and embers still in the cloth, his
    boots, his pale hands, and the scorch on the ground under him."""
    c = Canvas(COURIER_W, COURIER_H)
    for y in range(COURIER_H):
        for x in range(COURIER_W):
            d = math.hypot((x + 0.5 - 21) / 20, (y + 0.5 - 11) / 8)
            if d < 1.0:
                c.glaze(x, y, TAR[1], int(140 * (1 - d) ** 0.7))

    def half(x):
        """How far his robe reaches either side of his middle at a column, or 0 past it."""
        for (xa, ha), (xb, hb) in zip(COURIER_TORSO, COURIER_TORSO[1:]):
            if xa <= x < xb:
                return ha + (hb - ha) * (x - xa) / (xb - xa)
        return 0.0

    def robe(x, y):
        return abs(y + 0.5 - COURIER_MIDDLE) < half(x)

    clothed = [(leg, LEGGINGS) for leg in COURIER_LEGS] + [(boot, BARK[:4]) for boot in COURIER_BOOTS]
    for (x0, y0, x1, y1), ramp in clothed:
        for y in range(y0, y1):
            for x in range(x0, x1):
                c.put(x, y, ramp[3] if y == y0 else ramp[0] if y == y1 - 1 else ramp[2] if x < x1 - 1 else ramp[1])
    for (sx, sy), (ex, ey), (hx, hy) in COURIER_ARMS:
        c.line(sx, sy, ex, ey, ROBE[2])
        c.line(sx, sy + 1, ex, ey + 1, ROBE[1])
        c.line(ex, ey, hx + 2, hy, ROBE[3])
        c.line(ex, ey + 1, hx + 2, hy + 1, ROBE[1])
        for dx, dy, k in ((0, 0, 1), (1, 0, 1), (0, 1, 0), (1, 1, 0), (-1, 0, 1)):
            c.put(hx + dx, hy + dy, HAND[k])
    for y in range(COURIER_H):
        for x in range(COURIER_W):
            if not robe(x, y):
                continue
            if not robe(x, y - 1) or not robe(x - 1, y):
                k = 3
            elif not robe(x, y + 1) or not robe(x + 1, y):
                k = 0
            else:
                k = 2 if y + 0.5 < COURIER_MIDDLE else 1
            if x in (16, 26) and k:
                k -= 1
            c.put(x, y, ROBE[k])
    for y in range(COURIER_H):
        if robe(22, y):
            c.put(22, y, TRIM[1])
            c.put(23, y, TRIM[0])
        for x in (29, 30):
            if robe(x, y):
                c.put(x, y, TAR[1] if hash01(x, y, 1110) < 0.6 else ASH[0])
    hx, hy, hr = COURIER_HOOD
    for y in range(int(hy - hr) - 1, int(hy + hr) + 2):
        for x in range(int(hx - hr) - 1, int(hx + hr) + 2):
            dx, dy = x + 0.5 - hx, y + 0.5 - hy
            d = math.hypot(dx, dy * 1.1)
            if d > hr:
                continue
            light = -(dx * 0.7 + dy) / hr
            c.put(x, y, ROBE[3] if light > 0.45 else ROBE[2] if light > -0.2 else ROBE[1])
    for y in range(int(hy - hr) + 1, int(hy + hr)):
        c.put(int(hx + hr) - 1, y, TRIM[1] if y % 2 else TRIM[0])
    for i in range(12):
        x = 12 + int(hash01(i, 1, 1111) * 17)
        y = 5 + int(hash01(1, i, 1112) * 11)
        if robe(x, y):
            c.put(x, y, ASH[i % 2])
    for x, y in ((15, 7), (29, 13), (30, 7), (25, 14)):
        c.put(x, y, EMBER)
    c.outline(STONE_INK)
    return c.img


SATCHEL_W, SATCHEL_H = 16, 13


def satchel():
    """The courier's satchel, fallen open: worn leather with a flap and a brass buckle, its strap
    trailing, and a charred letter half out of it, sealed with a black square."""
    c = Canvas(SATCHEL_W, SATCHEL_H)
    for y in range(4, 12):
        for x in range(2, 13):
            k = 3 if y == 4 else 1 if y == 11 or x == 12 else 2
            c.put(x, y, BOARD[k])
    for y in range(4, 8):
        for x in range(3, 12):
            c.put(x, y, BOARD[4] if y == 4 else BOARD[3])
    c.put(7, 7, (214, 166, 84))
    c.put(7, 8, (164, 116, 56))
    for x, y in ((8, 1), (9, 1), (10, 1), (11, 2), (8, 2), (9, 2), (10, 2), (8, 3), (9, 3), (10, 3)):
        c.put(x, y, (226, 214, 180) if y < 3 else (168, 150, 120))
    c.put(11, 1, ASH[0])
    c.put(9, 2, CRYSTAL[0])
    for i in range(5):
        c.put(13 + i // 3, 10 - i, BOARD[1])
    c.outline(WOOD_INK)
    return c.img


def quarry_drawn():
    """The quarry's props, by name."""
    return {
        "root-wall": root_wall(),
        "black-crystal": black_crystal(),
        "black-crystal-shattered": black_crystal_shattered(),
        "black-crystal-purified": black_crystal_purified(),
        "courier-body": courier_body(),
        "satchel": satchel(),
    }


if __name__ == "__main__":
    print("Generating the Mirewood and the Old Quarry for Legend Quest...")
    build_sheet(MIREWOOD_TILES, os.path.join(OUT, "Tiles", "mirewood.png"))
    build_sheet(QUARRY_TILES, os.path.join(OUT, "Tiles", "quarry.png"))
    for made in (blackvein_trees, mirewood_drawn, quarry_drawn):
        for name, picture in made().items():
            write_png(os.path.join(OUT, "Props", name + ".png"), picture)
    print("Done.")
