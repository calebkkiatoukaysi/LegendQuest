"""
The meadows round Questeria: the old road out of the city gate and the fields of Thornbridge outside
its east gate. Their tileset, and their props: the farm (the farmhouse, the windmill, the hay, the
wheat, the scarecrow and the fences), the stone bridge over the stream, the great oak, the wild
flowers, and the refugees' camp at Thornbridge, with the city's east gate behind it.

    python tools/generate_meadow.py

The road used to be a wood at dusk, and beside Questeria it looked like another game. Outside the
walls is meant to be the cheerful part: noon, lush green, flowers everywhere, a stream with a bridge
over it, the sort of fields a starting town has round it. So the grass here is the lawns' grass
from generate_harbour.py turned up, greener and brighter, its shadows leaning teal and its light
leaning yellow the same way the leaves on my trees sheet do, and everything else is drawn in the
colours of my style sheet: warm earth for the paths, azure water, cream stone, warm wood.

Everything a tileset needs is laid out the way generate_harbour.py lays Questeria's out: each
variant of a tile has the same edges as the others, so a field of them has no seams (the grass and
the earth have their own light and dark in the middle of each, so they don't repeat either), and a
tile with edges has four more frames for where it meets other ground. The stream ripples, four frames a
variant, and so does the ford across it.

The props are three kinds. The trees and the wild flowers are cut off my trees sheet with
cut_supplied.py's cutter, the great oak put together out of its trunks and crowns. The farmhouse is
put together out of the parts on my houses sheet, the way cut_supplied.py builds Questeria's houses,
under a roof turned the red of a farm's tiles, and the east gate is my city gate, with a stretch of
the white wall cut to stand either side of it. The rest (the windmill, the hay, the fences, the
bridge, the scarecrow, the wheat and everything in the camp) is drawn here to match: lit from the
top left, dark outlines, small detail.

Only needs CPython. Every bit of noise is hash01 on where it is or a seeded Random, and the cutter
has nothing random in it, so a rerun writes the same bytes.
"""

import colorsys
import math
import os
import random
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from pixels import OUT, hash01, lerp_color, write_png  # noqa: E402
from generate_harbour import (GOLD, IRON, MORTAR, NAVY, OUTLINE, STONE, WOOD, T, Canvas, blobs, build_sheet,  # noqa: E402
                              dither, put, slabs, tile, turned, water_frames)
from generate_world import clump, pebble, put_inside, shade  # noqa: E402
from cut_supplied import (HUE_KEPT, KITBASH_PITCH, OUTSIDE, ROOF, ROOF_COLOURS, cut_out, parts_of,  # noqa: E402
                          put_together, roof_colour_of)


# --------------------------------------------------------------------------- #
# The palette: the meadow's greens, its earth and water, from my style sheet and my trees sheet
# --------------------------------------------------------------------------- #
# Every ramp goes dark to light.

#: The grass: greener and brighter than Questeria's lawns, the shadows teal and the light yellow.
GRASS = ((34, 86, 66), (56, 124, 60), (88, 158, 56), (124, 186, 58), (164, 210, 72), (206, 232, 120))

#: The long grass: a step deeper and richer.
LONG = ((22, 64, 58), (34, 96, 58), (54, 128, 54), (84, 158, 54), (124, 188, 62), (170, 214, 92))

#: A footpath's warm, trodden earth.
PATH = ((108, 66, 50), (146, 94, 62), (184, 128, 80), (212, 160, 104), (232, 188, 132), (246, 214, 166))

#: The road's paler, sandier earth, packed hard by the carts.
ROAD = ((124, 90, 72), (166, 126, 90), (200, 160, 112), (222, 188, 138), (238, 208, 162), (250, 230, 194))

#: A field turned over for planting: dark, rich and damp.
SOIL = ((64, 38, 38), (94, 58, 46), (126, 82, 58), (156, 108, 74), (186, 138, 96))

#: The stream: a brighter, greener blue than the harbour, it being shallow and moving.
STREAM = ((14, 66, 132), (16, 98, 170), (24, 132, 200), (40, 164, 222), (86, 202, 238), (170, 236, 250), (240, 253, 255))

#: The ford: the stream gone shallow over sand and gravel, so it's lighter and greener still.
SHALLOW = ((48, 128, 158), (70, 156, 184), (98, 184, 204), (132, 208, 222), (180, 234, 240), (236, 252, 252))

#: Stones and gravel, lit grey going lavender underneath.
GRAVEL = ((112, 104, 120), (156, 148, 156), (198, 190, 190), (232, 226, 218))

#: A hedge's leaves, the same greens as my trees sheet's oaks.
HEDGE = ((18, 54, 54), (30, 86, 60), (54, 122, 58), (92, 158, 58), (140, 192, 68), (188, 220, 102))

#: Flowers: white, buttercup yellow, pink, cornflower blue, lilac, poppy red, and the yellow of a
#: daisy's middle.
WHITE = (248, 244, 236)
YELLOW = (252, 214, 84)
PINK = (244, 148, 186)
BLUE = (124, 164, 240)
LILAC = (192, 160, 238)
RED = (230, 84, 80)
HEART = (250, 196, 64)

#: The outline round anything lying in the grass: its own darkest green, never black.
GRASS_INK = GRASS[0]


def ramp_at(ramp, k):
    """A shade off a ramp, held to its ends."""
    return ramp[max(0, min(len(ramp) - 1, k))]


# --------------------------------------------------------------------------- #
# Tiles/meadow.png -- the road out of Questeria and the fields of Thornbridge
# --------------------------------------------------------------------------- #
# The ground is grass, long grass, the footpaths and the road, tilled fields, the stream and the
# ford, hedges and the top of the city wall. Grass is the ground everything else sits in, so it's
# the one without edges: the long grass has a ragged edge of short grass round it, the paths and
# the road a verge with a lip of shadow or light under it, the fields grass growing over their edge,
# the stream a bank and a line of foam, the ford wet gravel, the hedges a lit top and a shadowed
# face, and the wall its parapet. Everything lying about (flowers, clover, long tufts, stones, the
# ruts, reeds, lilies, the crops, stepping stones and petals) is detail, drawn over the ground and
# kept off the edges of its tile, apart from the ruts and the crops, which run on into the next one.

def tuft_at(img, x, y, ramp=GRASS):
    """A tuft of grass: a few blades lit at their tips, dark at their root."""
    put(img, x, y - 1, ramp[5])
    put(img, x - 1, y, ramp[4])
    put(img, x + 1, y, ramp[4])
    put(img, x, y, ramp[3])
    put(img, x - 2, y + 1, ramp[3])
    put(img, x - 1, y + 1, ramp[2])
    put(img, x, y + 1, ramp[3])
    put(img, x + 1, y + 1, ramp[2])
    put(img, x + 2, y + 1, ramp[1])
    put(img, x - 1, y + 2, ramp[0])
    put(img, x, y + 2, ramp[1])
    put(img, x + 1, y + 2, ramp[0])


def bloom_at(img, x, y, petal, middle=HEART):
    """A little flower lying open in the grass: four petals round its middle, lit on the top left,
    and a pixel of shadow under it."""
    for dx, dy in ((0, -1), (-1, 0), (1, 0), (0, 1)):
        put(img, x + dx, y + dy, petal if dx + dy < 0 else shade(petal, 0.86))
    put(img, x, y, middle)
    put(img, x + 1, y + 1, GRASS[0])


def mottled(seed, variant):
    """Soft light and dark patches across a tile, -1 to 1: the same as every other variant's round
    its edges, so neighbours always meet, and its own in the middle, so the tiles don't repeat."""
    shared, own = blobs(seed, 6, 5.0), blobs(seed + 1 + variant * 7, 5, 4.5)
    field = [[0.0] * T for _ in range(T)]
    for y in range(T):
        for x in range(T):
            middle = min(x + 0.5, T - x - 0.5, y + 0.5, T - y - 0.5) / (T / 2)
            k = min(1.0, middle * 1.6)
            field[y][x] = shared[y][x] * (1 - k) + own[y][x] * k
    return field


def lawn(ramp, seed, variant, blades=0.08):
    """Grass with soft light and dark across it and a blade lit at its tip here and there. The blades
    are every pixel's own hash, different in every variant, since a blade can't make a seam."""
    img = tile()
    light = mottled(seed, variant)
    own = seed + 11 + variant * 13
    for y in range(T):
        for x in range(T):
            m = light[y][x]
            c = 2
            if m > 0.2 and dither(x, y, (m - 0.2) * 1.5):
                c = 3
            elif m < -0.45 and dither(x, y, (-0.45 - m) * 1.3):
                c = 1
            n = hash01(x, y, own)
            if n < 0.05:
                c = 1
            elif n > 0.95:
                c = 3
            put(img, x, y, ramp[c])
    for y in range(T):
        for x in range(T):
            if hash01(x, y, own + 1) < blades:
                put(img, x, y, ramp[4] if hash01(y, x, own + 2) < 0.22 else ramp[3])
                put(img, x, y + 1, ramp[1])
    return img


def grass(variant):
    """The meadow's grass: lush and bright, with a tuft or two, and on one variant in six a daisy
    and on another a buttercup. The tufts and flowers are each variant's own, off its edges."""
    img = lawn(GRASS, 501, variant)
    for i in range(1 + variant % 2):
        tuft_at(img, 3 + int(hash01(i, variant, 505) * 10), 3 + int(hash01(variant, i, 506) * 9))
    if variant in (2, 5):
        x, y = 4 + int(hash01(1, variant, 507) * 8), 4 + int(hash01(variant, 1, 508) * 8)
        bloom_at(img, x, y, WHITE if variant == 2 else YELLOW)
    return img


def long_grass(variant):
    """Long grass, deeper and thicker than the lawn, with more blades standing up in it and tufts
    of it taller still. Under the trees and along the hedges and the stream."""
    img = lawn(LONG, 511, variant, blades=0.18)
    for i in range(3):
        tuft_at(img, 3 + int(hash01(i, variant, 515) * 10), 3 + int(hash01(variant, i, 516) * 9), LONG)
    return img


def reaches(seed):
    """How far a ragged edge reaches into a tile down each column: a walk that wanders between one
    and four pixels and comes back to two at either end, so two edges side by side meet."""
    rng = random.Random(seed)
    walk, r = [], 2.0
    for x in range(T):
        r = max(1.0, min(4.0, r + rng.choice((-1, 0, 0, 1))))
        walk.append(r)
    out = []
    for x, r in enumerate(walk):
        end = min(x, T - 1 - x) / 3.0
        out.append(int(round(2 + (r - 2) * min(1.0, end))))
    return out


def fringe(img, seed, ramp=GRASS):
    """Short grass reaching over the edge of a tile from its north side, ragged, with the odd blade
    further in. Returns how far it reaches down each column."""
    reach = []
    for x, r in enumerate(reaches(521 + seed)):
        for y in range(r):
            put(img, x, y, ramp[3] if y == 0 and hash01(x, 1, 522 + seed) < 0.4 else ramp[2])
        put(img, x, r, ramp[1])
        if hash01(x, 2, 523 + seed) < 0.22:
            put(img, x, r, ramp[3])
            put(img, x, r + 1, ramp[1])
            r += 1
        reach.append(r)
    return reach


def long_edges():
    """Where long grass meets anything else: the short grass round it, ragged, and the tips of the
    long grass catching the light along its edge."""
    edges = []
    for side in range(4):
        img = tile()
        reach = fringe(img, side)
        for x, r in enumerate(reach):
            if hash01(x, side, 531) < 0.55:
                put(img, x, r, LONG[4])
                put(img, x, r + 1, LONG[2])
        edges.append(turned(img, side))
    return edges


def earth(ramp, seed, variant, grit, stones):
    """Trodden earth, worn paler in patches, with grit worked up through it, and a stone or two in
    the middle that's each variant's own."""
    img = tile()
    light = mottled(seed, variant)
    own = seed + 11 + variant * 13
    for y in range(T):
        for x in range(T):
            m = light[y][x]
            c = 3
            if m > 0.3 and dither(x, y, (m - 0.3) * 1.4):
                c = 4
            elif m < -0.4 and dither(x, y, (-0.4 - m) * 1.4):
                c = 2
            n = hash01(x, y, own)
            if n < 0.06:
                c = 2
            elif n > 0.97:
                c = 5
            put(img, x, y, ramp[c])
    for y in range(T):
        for x in range(T):
            if hash01(x, y, own + 1) < grit:
                put(img, x, y, GRAVEL[2] if hash01(y, x, own + 2) < 0.6 else GRAVEL[3])
                put(img, x + 1, y + 1, ramp[1])
    c = Canvas(T, T)
    for i in range(stones[variant]):
        x, y = 3 + int(hash01(i, variant, seed + 4) * 9), 3 + int(hash01(variant, i, seed + 5) * 9)
        pebble(c, x, y, 3 if i else 4, 2 if i else 3, GRAVEL)
    c.outline(ramp[1])
    for y in range(T):
        for x in range(T):
            if c.img[y][x][3]:
                img[y][x] = list(c.img[y][x])
    return img


def path(variant):
    """A footpath: warm earth trodden smooth, with a little grit and a stone now and then."""
    return earth(PATH, 541, variant, 0.012, (0, 1, 0))


def road(variant):
    """The road: paler, sandier earth packed hard by the carts, with more gravel in it."""
    return earth(ROAD, 551, variant, 0.035, (1, 0, 2))


def verge(seed, ramp, stones=False):
    """A path's or a road's verge: grass reaching over its edge, and under that a lip of shadow on
    the north and west sides, where the verge stands between the earth and the light, and of light
    on the south and east, where the earth rises to meet it. The road's has stones kicked up along it."""
    edges = []
    for side in range(4):
        img = tile()
        reach = fringe(img, seed + side)
        under = ramp[1] if side in (0, 3) else ramp[5]
        for x, r in enumerate(reach):
            put(img, x, r + 1, under)
            if side in (0, 3) and hash01(x, side, seed + 7) < 0.5:
                put(img, x, r + 2, ramp[2])
            if stones and hash01(x, side, seed + 8) < 0.16:
                put(img, x, r + 2, GRAVEL[3])
                put(img, x + 1, r + 2, GRAVEL[1])
        edges.append(turned(img, side))
    return edges


#: A furrow, from the top of its ridge down into the trough: which shade of SOIL each row is.
FURROW = (3, 2, 1, 0)


def soil(variant):
    """A field ploughed and harrowed: furrows running across it four pixels apart, the top of each
    ridge lit and the trough behind it dark, clods here and there. The furrows are the same in every
    variant so a field runs on; a stone or a sprouting weed is each one's own."""
    img = tile()
    for y in range(T):
        for x in range(T):
            c = FURROW[y % len(FURROW)]
            n = hash01(x, y, 561)
            if n < 0.1:
                c = max(0, c - 1)
            elif n > 0.92:
                c = min(4, c + 1)
            put(img, x, y, SOIL[c])
    for y in range(0, T, len(FURROW)):
        for x in range(T):
            if hash01(x, y, 562) < 0.12:
                put(img, x, y, SOIL[4])
                put(img, x + 1, y + 1, SOIL[0])
    if variant == 1:
        x, y = 4 + int(hash01(1, 5, 563) * 7), 5 + int(hash01(5, 1, 563) * 6)
        put(img, x, y, GRAVEL[3])
        put(img, x + 1, y, GRAVEL[2])
        put(img, x + 1, y + 1, SOIL[0])
    return img


def soil_edges():
    """Where a field meets the grass, the grass grows over its edge."""
    edges = []
    for side in range(4):
        img = tile()
        fringe(img, side + 11)
        edges.append(turned(img, side))
    return edges


def stream(variant):
    """The stream, every frame of a variant: the harbour's water drawn in the stream's colours."""
    return water_frames(variant, ramp=STREAM, seed=571, ripples=5, glints=1)


def stream_edges():
    """The stream's banks. Along the north it's the far bank seen from the front: the grass at its
    top, ragged, then the earth of its face going dark and wet, a line of shadow on the water under
    it and foam along it. The other sides are a strip of wet sand and gravel at the water's edge,
    wider and narrower as it goes, with the water breaking white along it. Where the stream runs
    into the ford it's the same: the deep water stopping at the gravel."""
    north = tile()
    for x, r in enumerate(reaches(581)):
        top = max(1, r - 1)
        for y in range(top):
            put(north, x, y, GRASS[3] if y == 0 and hash01(x, 0, 582) < 0.4 else GRASS[2])
        put(north, x, top, GRASS[1])
        put(north, x, top + 1, PATH[3])
        put(north, x, top + 2, PATH[2] if hash01(x, 2, 583) < 0.8 else GRAVEL[2])
        put(north, x, top + 3, PATH[1])
        put(north, x, top + 4, STREAM[0])
        put(north, x, top + 5, STREAM[5] if hash01(x, 5, 584) < 0.6 else STREAM[4])
        if hash01(x, 6, 584) < 0.3:
            put(north, x, top + 6, STREAM[4])
    edges = [north]
    for side in (1, 2, 3):
        img = tile()
        for x, r in enumerate(reaches(585 + side)):
            d = 1 if r < 3 else 2
            for y in range(d):
                n = hash01(x, y + side * 5, 586)
                put(img, x, y, PATH[4] if n < 0.5 else PATH[3] if n < 0.8 else GRAVEL[3])
            put(img, x, d, STREAM[6] if hash01(x, side, 587) < 0.5 else STREAM[5])
            if hash01(x, side, 588) < 0.35:
                put(img, x, d + 1, STREAM[4])
        edges.append(turned(img, side))
    return edges


def ford(variant):
    """The ford, every frame of a variant: shallow water over sand, with gravel and stones showing
    through it, and the ripples coming and going over them like the stream's."""
    frames = water_frames(variant, ramp=SHALLOW + (SHALLOW[-1],), seed=591, ripples=4, glints=1)
    stones = [(int(hash01(i, variant, 592) * (T - 3)) + 1, int(hash01(variant, i, 593) * (T - 3)) + 1,
               2 + (i % 2), 2) for i in range(4)]
    out = []
    for f, img in enumerate(frames):
        for x, y, w, h in stones:
            for dy in range(h):
                for dx in range(w):
                    k = 3 if dx + dy == 0 else 2 if dy == 0 else 1
                    lit = tuple(img[y + dy][x + dx][:3]) in (SHALLOW[4], SHALLOW[5])
                    if not lit:
                        put(img, x + dx, y + dy, lerp_color(GRAVEL[k], SHALLOW[2], 0.45))
            put(img, x + w, y + h, SHALLOW[1])
        out.append(img)
    return out


def ford_edges():
    """Where the ford meets the bank or the deep water: a strip of wet gravel, lit on top, wider and
    narrower as it goes, and the water breaking white along it."""
    edges = []
    for side in range(4):
        img = tile()
        for x, r in enumerate(reaches(601 + side)):
            d = 1 if r < 3 else 2
            for y in range(d):
                n = hash01(x, y + side * 5, 602)
                stone = GRAVEL[3] if n < 0.45 else GRAVEL[2] if n < 0.8 else PATH[4]
                put(img, x, y, lerp_color(stone, SHALLOW[2], 0.3))
            if hash01(x, side, 603) < 0.5:
                put(img, x, d, SHALLOW[4])
        edges.append(turned(img, side))
    return edges


#: Flowers dotted through a hedge: where on the tile, and which colour, for each variant.
HEDGE_FLOWERS = (((4, 5, WHITE), (11, 10, PINK), (7, 13, WHITE)), ((10, 4, WHITE), (3, 11, YELLOW)))


def hedge(variant):
    """A hedge too thick to get through: round clumps of leaves heaped on each other, the lower one
    in front, with the dark between them, and a flower or two in it. The clumps are on a grid shaken
    well about and wrap round the tile, the same in every variant; the middle ones are each one's own."""
    img = tile()
    for y in range(T):
        for x in range(T):
            put(img, x, y, HEDGE[0])
    clumps = []
    for i in range(5):
        drop = hash01(i, 9, 611) * 3.2
        for j in range(4):
            own = 1 <= i <= 3 and 1 <= j <= 2
            seed = 613 + (variant * 17 if own else 0)
            cx = i * 3.2 + (hash01(i, j, seed) - 0.5) * 2.2
            cy = j * 4 + drop + (hash01(j, i, seed + 1) - 0.5) * 2.4
            r = 2.5 + hash01(i + 4, j, seed + 2) * 1.3
            lift = 1 if hash01(i, j, seed + 3) > 0.6 else 0
            clumps.append((cy, cx, r, lift, seed))
    for cy, cx, r, lift, seed in sorted(clumps):
        clump(img, cx, cy, r, HEDGE[1:], seed, lift)
    for x, y, colour in HEDGE_FLOWERS[variant]:
        put(img, x, y, colour)
        put(img, x + 1, y, shade(colour, 0.85))
        put(img, x, y + 1, shade(colour, 0.85))
    return img


def hedge_edges():
    """Where a hedge meets open ground. Along the north it's the top of it, a row of clumps a shade
    lighter with a dark line along their tops; along the south it's the face of it, the leaves going
    into shadow down to the stems and a dark line where it meets the ground; along the sides, a dark
    line, lit on the west."""
    north = tile()
    for x in range(T):
        put(north, x, 0, HEDGE[0])
    for i in range(4):
        clump(north, i * 4 + 2.0, 3.6, 3.0, HEDGE[2:], 621 + i, 0, put_inside)
    south = tile()
    for y in range(10, T):
        for x in range(T):
            stem = hash01(x, 0, 622) < 0.25
            put(south, x, y, PATH[1] if stem and y >= 13 else HEDGE[1] if y < 12 else HEDGE[0])
    for i in range(4):
        clump(south, i * 4 + 1.0, 9.6, 2.8, HEDGE[1:], 623 + i, 0, put_inside)
    for x in range(T):
        put(south, x, T - 1, HEDGE[0])
    east, west = tile(), tile()
    for y in range(T):
        put(east, T - 1, y, HEDGE[0])
        put(east, T - 2, y, HEDGE[1])
        put(west, 0, y, HEDGE[0])
        if hash01(0, y, 624) < 0.6:
            put(west, 1, y, HEDGE[4])
    return [north, east, south, west]


#: The wall-walk's flagstones: the same courses as the plaza.
WALK_COURSES = (
    (7, [0], [[], [9]]),
    (9, [4], [[11], []]),
)

#: The dark line round the wall's edge: a lavender, the way my style sheet shades cream stone.
WALL_INK = (96, 82, 110)


def wall(variant):
    """The top of Questeria's wall, seen from above: the wall-walk's cream flagstones."""
    return slabs(WALK_COURSES, STONE, MORTAR, 631, variant, cracks=variant)


def merlons(img):
    """A parapet along the north of a tile, seen from above: a dark line along its outside, the
    merlons standing up from it every eight pixels with their tops lit and their faces showing under
    them, the crenels between them dark, the coping under the lot, and its shadow on the wall-walk."""
    for x in range(T):
        high = x % 8 < 5
        put(img, x, 0, WALL_INK)
        if high:
            edge = x % 8 == 4
            put(img, x, 1, STONE[3] if x % 8 == 0 else STONE[1] if edge else STONE[4])
            put(img, x, 2, STONE[3] if x % 8 == 0 else STONE[1] if edge else STONE[4])
            put(img, x, 3, STONE[1] if edge else STONE[3])
            put(img, x, 4, STONE[2] if not edge else MORTAR[0])
            put(img, x, 5, STONE[1])
        else:
            put(img, x, 1, WALL_INK)
            put(img, x, 2, WALL_INK)
            put(img, x, 3, STONE[0])
            put(img, x, 4, STONE[4])
            put(img, x, 5, STONE[2])
        put(img, x, 6, STONE[1] if x % 8 != 7 else MORTAR[0])
        put(img, x, 7, MORTAR[0])


def wall_edges():
    """Where the wall stops: a crenellated parapet along its sides, and along the south its face,
    a course of coping over the cream blocks of the wall going down into shadow."""
    north = tile()
    merlons(north)
    south = tile()
    for x in range(T):
        put(south, x, 0, STONE[4])
        put(south, x, 1, STONE[3] if x % 8 else MORTAR[1])
        for y in range(2, T - 1):
            course = (y - 2) // 5
            joint = (y - 2) % 5 == 4 or (x + course * 4) % 8 == 7
            put(south, x, y, MORTAR[0] if joint else STONE[3 if y < 7 else 2 if y < 12 else 1])
        put(south, x, T - 1, WALL_INK)
    edges = [north]
    for side in (1, 3):
        img = tile()
        merlons(img)
        edges.append(turned(img, side))
    edges.insert(2, south)
    return edges


def detail(paint, ink=GRASS_INK):
    """A detail tile: whatever paint draws on a clear canvas, outlined in the dark of the ground."""
    def build(variant):
        c = Canvas(T, T)
        paint(c, variant)
        c.outline(ink)
        return c.img
    return build


#: The wild flowers lying in the grass, one colour a variant: daisies, buttercups, campion and
#: forget-me-nots. Each is where its flowers go and the colour of their petals and middles.
MEADOW_FLOWERS = (
    (((4, 5), (10, 4), (7, 10), (12, 11), (3, 12)), WHITE, HEART),
    (((5, 4), (11, 6), (4, 11), (9, 12)), YELLOW, (255, 244, 170)),
    (((6, 4), (11, 9), (4, 10), (12, 4)), PINK, WHITE),
    (((4, 4), (8, 7), (12, 5), (5, 12), (11, 12)), BLUE, WHITE),
)


def paint_flowers(c, variant):
    """Wild flowers on their leaves, open to the sun, lit on the top left."""
    spots, petal, middle = MEADOW_FLOWERS[variant]
    for x, y in spots:
        for dx, dy, k in ((0, 2, 3), (-1, 2, 2), (1, 2, 2), (0, 3, 1), (-1, 3, 2), (1, 1, 3)):
            c.put(x + dx, y + dy, GRASS[k])
    for x, y in spots:
        for dx, dy in ((0, -1), (-1, 0), (1, 0), (0, 1)):
            c.put(x + dx, y + dy, petal if dx + dy < 0 else shade(petal, 0.84))
        c.put(x, y, middle)


#: Clover heads, white or pink, and where the patch's leaves go.
CLOVER_HEADS = ((WHITE, (236, 206, 220)), (PINK, (214, 110, 160)))
CLOVER_LEAVES = ((3, 4), (7, 3), (11, 5), (5, 8), (9, 9), (13, 10), (4, 12), (8, 13), (12, 13))


def paint_clover(c, variant):
    """A patch of clover: three round leaflets to a leaf, lit on their tops, and a few round heads."""
    for i, (x, y) in enumerate(CLOVER_LEAVES):
        x += int(hash01(i, variant, 641) * 2) - 1
        for dx, dy in ((0, -1), (-1, 0), (1, 0)):
            c.put(x + dx, y + dy, GRASS[4] if dy < 0 else GRASS[3])
        c.put(x, y, GRASS[2])
        c.put(x, y + 1, GRASS[1])
    lit, dark = CLOVER_HEADS[variant]
    for i in range(3):
        x, y = CLOVER_LEAVES[(i * 3 + variant) % len(CLOVER_LEAVES)]
        c.put(x, y - 2, lit)
        c.put(x + 1, y - 2, dark)
        c.put(x, y - 1, dark)
        c.put(x + 1, y - 1, dark)


def paint_tuft(c, variant):
    """A clump of long grass: thin blades fanning out of one root and curling outward as they go,
    the ones leaning left lit at their tips."""
    rng = random.Random(651 + variant)
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
            c.put(x, y, LONG[k + 1])


def paint_pebbles(c, variant):
    """A few small stones lying in the grass, lit on their tops."""
    stones = (((3, 4, 4, 3), (9, 6, 3, 2), (6, 10, 3, 3)),
              ((4, 7, 3, 2), (9, 4, 4, 3), (10, 10, 3, 2), (4, 11, 2, 2)))[variant]
    for x, y, w, h in stones:
        pebble(c, x, y, w, h, GRAVEL)


def paint_ruts(c, variant):
    """Wheel ruts along the road, running on into the next tile: the north side of each groove in
    shadow, the south catching the light. One has a puddle in it, with the sky in it."""
    for x in range(T):
        y = 7 + round(0.8 * math.sin(x * math.pi / 8))
        c.put(x, y - 1, ROAD[1])
        c.put(x, y, ROAD[0])
        c.put(x, y + 1, ROAD[2])
        c.put(x, y + 2, ROAD[4])
        if hash01(x, variant, 661) < 0.25:
            c.put(x, y + 3, ROAD[5])
    if variant == 1:
        for x in range(5, 11):
            y = 7 + round(0.8 * math.sin(x * math.pi / 8))
            c.put(x, y, lerp_color(STREAM[2], ROAD[1], 0.4))
            c.put(x, y + 1, lerp_color(STREAM[4], ROAD[4], 0.3) if x % 3 else STREAM[5])


def paint_reeds(c, variant):
    """Reeds by the water: tall blades in a fan, a bulrush or two standing up out of them with its
    brown head, lit on the left."""
    rng = random.Random(671 + variant)
    root = 8.0
    for i in sorted(range(-3, 4), key=abs, reverse=True):
        lean = i * 0.16 + rng.uniform(-0.05, 0.05)
        height = rng.uniform(9.0, 12.5) - abs(i) * 0.6
        x0 = root + i * 1.2
        for j in range(int(height)):
            x = x0 + math.sin(lean) * j
            c.put(x, 14.5 - j, LONG[4] if j > height - 3 and i <= 0 else LONG[3] if i <= 0 else LONG[2])
    for k, dx in enumerate((-2, 2) if variant == 0 else (0,)):
        top = 2 + k
        for j in range(4):
            c.put(root + dx, top + 4 + j, LONG[1])
        for j in range(4):
            c.put(root + dx, top + j, PATH[2] if j else PATH[3])
            c.put(root + dx + 1, top + j, PATH[1])


#: Lily pads: the middle of each, how big, and whether it has a flower on it.
LILY_PADS = (((5, 5, 3.2, True), (11, 9, 2.6, False), (6, 12, 2.0, False)),
             ((10, 5, 3.0, False), (5, 10, 3.2, True), (12, 12, 1.8, False)))


def paint_lilies(c, variant):
    """Lily pads floating on the water, each with a notch cut out of it, lit on the top left; on one
    of them a water lily, pink going white in the middle."""
    for cx, cy, r, flower in LILY_PADS[variant]:
        for y in range(int(cy - r) - 1, int(cy + r) + 2):
            for x in range(int(cx - r) - 1, int(cx + r) + 2):
                dx, dy = x + 0.5 - cx, (y + 0.5 - cy) * 1.2
                if math.hypot(dx, dy) > r:
                    continue
                if dx > 0 and abs(dy) < 0.8:
                    continue
                light = -(dx * 0.7 + dy) / r
                c.put(x, y, HEDGE[4] if light > 0.5 else HEDGE[3] if light > -0.3 else HEDGE[2])
        if flower:
            x, y = int(cx), int(cy) - 1
            for dx, dy in ((0, -1), (-1, 0), (1, 0), (0, 1), (-1, -1), (1, -1)):
                c.put(x + dx, y + dy, PINK if dy < 0 or dx < 0 else (226, 120, 168))
            c.put(x, y, WHITE)


def paint_crops(c, variant):
    """Crops in rows along the furrows: young shoots, or cabbages, each sat on the top of a ridge."""
    for row in range(4):
        y = row * 4 + 2
        for col in range(4):
            x = col * 4 + 2 + (2 if row % 2 else 0) - (1 if col == 3 and row % 2 else 0)
            if variant == 0:
                c.put(x - 1, y - 1, GRASS[4])
                c.put(x + 1, y - 1, GRASS[3])
                c.put(x, y, GRASS[2])
                c.put(x, y - 1, GRASS[5])
            else:
                for dx, dy, k in ((-1, -1, 4), (0, -1, 5), (1, -1, 3), (-1, 0, 3), (0, 0, 4), (1, 0, 2)):
                    c.put(x + dx, y + dy, HEDGE[k])


def paint_stepping(c, variant):
    """Stepping stones across the ford: big flat stones, lit on top, wet and dark round the edge."""
    stones = (((2, 3, 5, 4), (9, 8, 5, 4)), ((3, 8, 5, 4), (9, 2, 5, 4)))[variant]
    for x, y, w, h in stones:
        pebble(c, x, y, w, h, GRAVEL)


def petals(variant):
    """Blossom fallen from the trees, lying in the grass or on the path: a scatter of pink petals,
    each with a pixel of shadow and no outline, since they're lying flat."""
    c = Canvas(T, T)
    for i in range(6 + variant * 3):
        x, y = 2 + int(hash01(i, variant, 681) * 12), 2 + int(hash01(variant, i, 682) * 12)
        c.put(x, y, PINK if i % 3 else WHITE)
        if i % 2:
            c.put(x + 1, y, shade(PINK, 0.86))
    for y in range(T - 1, -1, -1):
        for x in range(T - 1, -1, -1):
            if c.img[y][x][3] and x + 1 < T and y + 1 < T and not c.img[y + 1][x + 1][3]:
                c.img[y + 1][x + 1] = [0, 0, 0, 60]
    return c.img


def ruts(variant):
    """The ruts on their own: no outline, since they run off both ends of the tile."""
    c = Canvas(T, T)
    paint_ruts(c, variant)
    return c.img


def crops(variant):
    """The crops, outlined in the field's dark, which runs on into the next tile like the furrows."""
    c = Canvas(T, T)
    paint_crops(c, variant)
    c.outline(SOIL[0])
    return c.img


def flat(frames_by_variant):
    """Every frame of every variant in one list, the order they go on the sheet."""
    out = []
    for frames in frames_by_variant:
        out += frames
    return out


#: Each tile: its name, its frames (every frame of every variant, in order), and its edges if it
#: has them. TilesetCatalog.Meadow has the same list in the same order, with the same variants and
#: frames.
MEADOW_TILES = (
    ("grass", [grass(v) for v in range(6)], None),
    ("long-grass", [long_grass(v) for v in range(3)], long_edges()),
    ("path", [path(v) for v in range(3)], verge(701, PATH)),
    ("road", [road(v) for v in range(3)], verge(711, ROAD, stones=True)),
    ("soil", [soil(v) for v in range(2)], soil_edges()),
    ("stream", flat(stream(v) for v in range(3)), stream_edges()),
    ("ford", flat(ford(v) for v in range(2)), ford_edges()),
    ("hedge", [hedge(v) for v in range(2)], hedge_edges()),
    ("wall", [wall(v) for v in range(2)], wall_edges()),
    ("flowers", [detail(paint_flowers)(v) for v in range(4)], None),
    ("clover", [detail(paint_clover)(v) for v in range(2)], None),
    ("tuft", [detail(paint_tuft)(v) for v in range(2)], None),
    ("pebbles", [detail(paint_pebbles)(v) for v in range(2)], None),
    ("ruts", [ruts(v) for v in range(2)], None),
    ("reeds", [detail(paint_reeds)(v) for v in range(2)], None),
    ("lilies", [detail(paint_lilies, STREAM[0])(v) for v in range(2)], None),
    ("crops", [crops(v) for v in range(2)], None),
    ("stepping", [detail(paint_stepping, SHALLOW[0])(v) for v in range(2)], None),
    ("petals", [petals(v) for v in range(2)], None),
)


# --------------------------------------------------------------------------- #
# The props drawn here: the farm, the bridge, and the camp at Thornbridge
# --------------------------------------------------------------------------- #
# My sheets have no windmill, hay, rough fences, country bridge or tents on them, so these are drawn
# to match them: each lit from the top left, its shadows leaning lavender or teal, a dark outline
# round it, and small detail. They're drawn from the front and a little above, the way the camera
# sees everything, so each stands on the middle of its bottom edge.

#: Straw and hay, dark to light, and the twine round a bale.
STRAW = ((112, 72, 40), (160, 110, 48), (204, 154, 62), (232, 194, 92), (248, 224, 146))
TWINE = ((120, 70, 52), (170, 110, 72))

#: Tent canvas that's been out in every weather: a faded cream, and an olive-grey.
CANVAS = ((92, 82, 82), (132, 120, 112), (170, 156, 138), (204, 190, 166), (228, 216, 194))
DUN = ((76, 76, 64), (110, 110, 86), (142, 140, 106), (172, 168, 130), (200, 196, 160))

#: The patches sewn over the holes: a washed-out blue, red, green and plum.
PATCHES = ((104, 124, 156), (160, 96, 88), (122, 132, 84), (146, 110, 146))

#: A grey woollen blanket, and white linen.
WOOL = ((62, 58, 70), (94, 90, 100), (128, 124, 130), (164, 160, 160))
LINEN = ((170, 162, 168), (210, 204, 200), (236, 230, 222), (252, 250, 244))

#: Rope, and the brown glass of a bottle.
ROPE = (150, 118, 82)
BOTTLE = ((70, 34, 30), (124, 62, 40), (186, 110, 62))

#: The windmill's roof: my houses sheet's blue tiles, dark to light.
ROOF_BLUE = ((22, 60, 108), (28, 94, 152), (40, 128, 188), (62, 160, 212), (118, 198, 232))

#: The bridge's old stone, greyer and warmer than the city's, and the moss and ivy on it.
OLD_STONE = ((112, 102, 120), (150, 140, 148), (184, 174, 172), (210, 200, 190), (230, 222, 208), (246, 240, 228))
MOSS = ((44, 96, 66), (72, 132, 62), (112, 166, 66))


def polygon(pts, x, y):
    """Whether a point is inside a polygon (its corners in order), by counting edges crossed."""
    inside = False
    for i in range(len(pts)):
        (x0, y0), (x1, y1) = pts[i], pts[(i + 1) % len(pts)]
        if (y0 > y) != (y1 > y) and x < x0 + (y - y0) * (x1 - x0) / (y1 - y0):
            inside = not inside
    return inside


def fill(c, pts, colour):
    """Fills a polygon on a canvas, every pixel whose middle is inside it, with colour(x, y)."""
    xs, ys = [p[0] for p in pts], [p[1] for p in pts]
    for y in range(int(min(ys)), int(max(ys)) + 1):
        for x in range(int(min(xs)), int(max(xs)) + 1):
            if polygon(pts, x + 0.5, y + 0.5):
                c.put(x, y, colour(x, y))


HAY_W, HAY_H = 24, 17

#: How deep the top of a bale is, seen from above.
HAY_TOP = 6


def hay_bale():
    """A bale of hay: its top lit, its front going into shadow, the straw running along it in
    streaks, twine round it twice and a few loose straws sticking out."""
    c = Canvas(HAY_W, HAY_H)
    for y in range(1, HAY_H - 1):
        for x in range(1, HAY_W - 1):
            if x in (1, HAY_W - 2) and y in (1, HAY_H - 2):
                continue
            if y <= HAY_TOP:
                k = 4 if hash01(x // 2, y, 801) < 0.55 else 3
                if x >= HAY_W - 5:
                    k -= 1
            else:
                t = (y - HAY_TOP) / (HAY_H - 2 - HAY_TOP)
                k = 3 if t < 0.25 else 2 if t < 0.75 else 1
                if x >= HAY_W - 6:
                    k -= 1
                if hash01(x // 3, y, 802) < 0.2:
                    k -= 1
                elif hash01(x // 3, y, 803) > 0.88:
                    k += 1
            c.put(x, y, ramp_at(STRAW, k))
    for tx in (6, HAY_W - 8):
        for y in range(1, HAY_H - 1):
            c.put(tx, y, TWINE[1] if y <= HAY_TOP else TWINE[0])
    for x, y, dx, dy in ((3, 1, -1, -1), (11, 1, 0, -1), (19, 2, 1, -1), (1, 9, -1, 0), (22, 11, 1, 1)):
        c.put(x + dx, y + dy, STRAW[4])
    c.outline(OUTLINE)
    return c.img


STACK_W, STACK_H = 46, 38


def haystack():
    """A haystack: a dome of hay heaped up and raked down its sides, lit on its upper left, the
    straw hanging down it in streaks, loose hay round its foot, and a pitchfork stuck in its top."""
    c = Canvas(STACK_W, STACK_H)
    cx, cy, rx, ry = 23.0, 22.0, 20.0, 19.0
    bottom = STACK_H - 3
    for y in range(bottom + 1):
        for x in range(STACK_W):
            dx, dy = x + 0.5 - cx, y + 0.5 - cy
            half = rx * math.sqrt(max(0.0, 1.0 - (dy / ry) ** 2)) if dy < 0 else rx + dy * 0.25
            half *= 1.0 + 0.05 * math.sin(y * 1.3)
            if abs(dx) > half:
                continue
            light = -(dx / rx * 0.65 + dy / ry)
            k = 4 if light > 0.75 else 3 if light > 0.2 else 2 if light > -0.45 else 1
            if hash01(x, y // 3, 811) < 0.18:
                k -= 1
            elif hash01(x, y // 3, 812) > 0.9:
                k += 1
            if y >= bottom - 1:
                k = min(k, 1)
            c.put(x, y, ramp_at(STRAW, k))
    for x in range(2, STACK_W - 2):
        if hash01(x, 0, 813) < 0.4:
            c.put(x, bottom + 1, STRAW[2] if hash01(x, 1, 813) < 0.5 else STRAW[3])
    for i in range(14):
        hx, hy = 34 - i * 0.45, 2 + i
        c.put(hx, hy, WOOD[4] if i < 11 else STRAW[1])
        c.put(hx + 1, hy, WOOD[2] if i < 11 else STRAW[1])
    for tx in (33, 35, 37):
        c.put(tx, 1, IRON[2])
        c.put(tx, 0, IRON[1])
    c.box(33, 2, 37, 2, IRON[1])
    c.outline(OUTLINE)
    return c.img


FENCE_W, FENCE_H = 32, 18

#: Where the two rails cross a fence, from the top of the picture.
RAILS = (5, 10)


def post_at(c, x, top, bottom):
    """A fence post four pixels wide, lit down its left side, with a rounded top and the grain
    running down it."""
    for y in range(top, bottom + 1):
        for i, k in enumerate((4, 3, 2, 1)):
            if i == 2 and hash01(x, y, 821) < 0.25:
                k = 1
            c.put(x + i, y, WOOD[k])
    c.put(x + 1, top - 1, WOOD[5])
    c.put(x + 2, top - 1, WOOD[4])


def rail_at(c, x0, x1, y, seed):
    """A split rail, three pixels deep: lit along its top, dark under it, sagging a little."""
    for x in range(x0, x1 + 1):
        sag = 1 if 0.3 < (x - x0) / max(1, x1 - x0) < 0.7 and hash01(seed, 0, 822) < 0.5 else 0
        c.put(x, y + sag, WOOD[5] if hash01(x, y, seed) < 0.6 else WOOD[4])
        c.put(x, y + sag + 1, WOOD[3] if hash01(x, y + 1, seed) < 0.85 else WOOD[2])
        c.put(x, y + sag + 2, WOOD[1])


def fence_wood():
    """Two tiles of a farm fence, running across: a post at its left end and two split rails off it
    to the right, where the next length's post goes. A run of them ends in a fence-wood-post."""
    c = Canvas(FENCE_W, FENCE_H)
    for i, y in enumerate(RAILS):
        rail_at(c, 4, FENCE_W - 1, y, 823 + i)
    post_at(c, 1, 3, FENCE_H - 2)
    c.outline(OUTLINE)
    return c.img


POST_W = 6


def fence_wood_post():
    """A fence post on its own, for the end of a run of fence."""
    c = Canvas(POST_W, FENCE_H)
    post_at(c, 1, 3, FENCE_H - 2)
    c.outline(OUTLINE)
    return c.img


SIDE_H = FENCE_H + T - 4


def fence_wood_side():
    """A tile of fence running up the screen, away from you: its post at the front, and the rails
    running back from it to the next post, which from up here is a plank seen end on. One a tile
    makes a fence down the side of a field."""
    c = Canvas(POST_W, SIDE_H)
    for y in range(0, SIDE_H - FENCE_H + 8):
        c.put(2, y, WOOD[4] if hash01(2, y, 824) < 0.8 else WOOD[3])
        c.put(3, y, WOOD[2])
    post_at(c, 1, SIDE_H - FENCE_H + 3, SIDE_H - 2)
    c.outline(OUTLINE)
    return c.img


WHEAT_W, WHEAT_H = 32, 26


def wheat():
    """Two tiles of ripe wheat: stalks in three rows, the back ones higher, each with its ear of
    grain lit on its left and the awns sticking up off it."""
    c = Canvas(WHEAT_W, WHEAT_H)
    rng = random.Random(831)
    for row in range(3):
        base = WHEAT_H - 2 - (2 - row) * 3
        for i in range(8):
            x = 2 + i * 4 + (2 if row % 2 else 0) + rng.randint(-1, 1)
            if x > WHEAT_W - 3:
                continue
            tall = rng.randint(11, 14)
            lean = rng.choice((-1, 0, 0, 1))
            top = base - tall
            for y in range(top + 5, base + 1):
                c.put(x + (lean if y < top + 8 else 0), y, STRAW[2] if y < base - 3 else STRAW[1])
            ex = x + lean
            for y in range(top, top + 6):
                c.put(ex, y, STRAW[4] if y < top + 4 else STRAW[3])
                c.put(ex + 1, y, STRAW[3] if y < top + 4 else STRAW[2])
            c.put(ex, top - 1, STRAW[4])
            c.put(ex - 1, top - 2, STRAW[3])
            c.put(ex + 2, top - 1, STRAW[3])
    c.outline(STRAW[0])
    return c.img


SCARECROW_W, SCARECROW_H = 28, 44

#: The scarecrow's shirt, a faded red check; its head's sacking.
SHIRT = ((110, 44, 48), (156, 62, 60), (196, 94, 80))
SACKING = ((132, 98, 66), (182, 146, 98), (214, 184, 132))


def scarecrow():
    """A scarecrow on its post: a sacking head with a stitched face under a wide straw hat, a red
    check shirt stuffed with straw and spilling it at the cuffs and the waist, a patch on it, and a
    crow sat on its arm, not scared at all."""
    c = Canvas(SCARECROW_W, SCARECROW_H)
    cx = 14
    for y in range(16, SCARECROW_H - 1):
        c.put(cx - 1, y, WOOD[4])
        c.put(cx, y, WOOD[2])
    for x in range(3, SCARECROW_W - 3):
        for y in range(17, 22):
            check = (x % 3 == 0) or (y % 3 == 0)
            k = 2 if y < 19 and x < 16 else 1
            c.put(x, y, SHIRT[max(0, k - 1)] if check else SHIRT[k])
    for y in range(22, 32):
        for x in range(9, 20):
            check = (x % 3 == 0) or (y % 3 == 0)
            k = 2 if x < 14 else 1
            c.put(x, y, SHIRT[max(0, k - 1)] if check else SHIRT[k])
    c.box(10, 25, 13, 28, PATCHES[0])
    c.put(10, 25, shade(PATCHES[0], 1.2))
    for x, y in ((1, 18), (2, 20), (2, 17), (25, 19), (26, 21), (24, 17), (10, 32), (13, 33), (16, 32), (18, 33)):
        c.put(x, y, STRAW[4] if (x + y) % 2 else STRAW[3])
    for y in range(7, 17):
        for x in range(9, 20):
            d = math.hypot(x + 0.5 - cx, (y + 0.5 - 11.5) * 1.05)
            if d < 5.2:
                light = -((x + 0.5 - cx) * 0.7 + (y + 0.5 - 11.5)) / 5.2
                c.put(x, y, SACKING[2] if light > 0.35 else SACKING[1] if light > -0.4 else SACKING[0])
    for x, y in ((12, 11), (16, 11)):
        c.put(x, y, OUTLINE)
    for x in range(12, 17):
        c.put(x, 14, OUTLINE if x % 2 else SACKING[0])
    c.box(12, 16, 16, 16, TWINE[0])
    for x in range(5, 24):
        c.put(x, 7, STRAW[3] if x < 15 else STRAW[2])
        c.put(x, 8, STRAW[1])
    for y in range(1, 7):
        for x in range(9, 20):
            if y == 1 and x in (9, 19):
                continue
            c.put(x, y, STRAW[4] if x < 13 else STRAW[3] if x < 17 else STRAW[2])
    c.box(9, 5, 19, 5, RED)
    for x, y, col in ((22, 14, OUTLINE), (23, 14, OUTLINE), (23, 13, OUTLINE), (24, 13, IRON[1]),
                      (21, 15, OUTLINE), (22, 15, IRON[0]), (23, 15, IRON[0]), (24, 15, IRON[0]),
                      (25, 13, YELLOW), (22, 16, YELLOW), (24, 16, YELLOW)):
        c.put(x, y, col)
    c.outline(OUTLINE)
    return c.img


def tent(w, h, peak, depth, ramp, seed, blanket=False):
    """A ridge tent that's been pitched too often: its two slopes and its front, lit from the left,
    the canvas sagging between the poles, patched where it tore and torn again along its hem, the
    ridge pole showing at the front, seams running along it, and guy ropes out to its pegs. Its
    door's open with the flap tied back, or hung with a blanket."""
    c = Canvas(w, h)
    base, mid = h - 2, w / 2
    front, back = (mid, base - peak), (mid, base - peak - depth)
    left = ((2, base), front, back, (8, base - depth))
    right = ((w - 3, base), front, back, (w - 9, base - depth))
    rng = random.Random(seed)
    patches = [(rng.randint(5, w - 12), rng.randint(base - peak - depth + 6, base - 8), rng.randint(4, 7),
                rng.randint(3, 5), PATCHES[i % len(PATCHES)]) for i in range(4)]

    def cloth(k):
        def colour(x, y):
            for px, py, pw, ph, col in patches:
                if px <= x < px + pw and py <= y < py + ph:
                    edge = x in (px, px + pw - 1) or y in (py, py + ph - 1)
                    lift = 1.0 if k >= 3 else 0.8
                    return shade(col, lift * 1.15) if edge and (x + y) % 2 else shade(col, lift)
            sag = math.sin((x - 2) / (w - 5) * math.pi * 3) * 0.5 + hash01(x // 2, y, seed) * 0.6
            return ramp_at(ramp, k - (1 if sag > 0.75 else 0) + (1 if sag < 0.05 else 0))
        return colour

    fill(c, right, cloth(2))
    fill(c, left, cloth(3))
    for i in (1, 2):
        lift = depth * i / 3
        c.line(2 + 6 * i / 3, base - lift, mid, front[1] - lift, ramp[4])
        c.line(w - 3 - 6 * i / 3, base - lift, mid, front[1] - lift, ramp[1])
    fill(c, ((2, base), front, (w - 3, base)), cloth(1))
    for x in range(2, w - 2):
        if hash01(x, 0, seed + 1) < 0.25:
            c.img[base][x] = [0, 0, 0, 0]
    tear = (int(w * 0.72), base - peak // 2 - depth // 2)
    for dx, dy in ((0, 0), (1, 0), (0, 1), (1, 1), (2, 1)):
        c.put(tear[0] + dx, tear[1] + dy, ramp[0])
    door = ((mid, front[1] + 6), (mid - 7, base), (mid + 7, base))
    if blanket:
        fill(c, door, lambda x, y: WOOL[2] if (y // 3) % 3 else RED if x < mid else shade(RED, 0.8))
    else:
        fill(c, door, lambda x, y: (40, 30, 40))
        fill(c, ((mid, front[1] + 6), (mid - 7, base), (mid - 11, base - 4)), lambda x, y: ramp[4] if x < mid - 7 else ramp[3])
    for i in range(depth + 3):
        c.put(mid, back[1] - 2 + i, WOOD[3])
    c.put(mid, front[1] - 1, WOOD[4])
    c.put(mid, front[1] - 2, WOOD[4])
    c.line(5, base - depth // 2, 0, base - 2, ROPE)
    c.line(w - 6, base - depth // 2, w - 1, base - 2, ROPE)
    c.outline(OUTLINE)
    return c.img


def tent_ragged():
    """The camp's tents: patched cream canvas, its door open and the flap tied back."""
    return tent(58, 50, 23, 20, CANVAS, 841)


def tent_ragged_2():
    """A lower, wider tent of olive canvas, with a blanket hung over its door."""
    return tent(64, 44, 19, 19, DUN, 851, blanket=True)


COOK_W, COOK_H = 28, 36

#: The cooking fire's ring of stones: its middle and how far it reaches each way.
COOK_RING = (14.0, COOK_H - 6.0, 8.6, 3.4)

#: The coals, dark to hot.
EMBERS = ((96, 34, 34), (168, 62, 36), (226, 112, 44), (250, 172, 72))


def cookfire():
    """A cooking fire: a ring of stones round the coals and two logs, three sticks lashed together
    over it, and a blackened pot hanging off them on a chain with steam coming off it. The game
    draws the flame on it, under the pot."""
    c = Canvas(COOK_W, COOK_H)
    cx, cy, rx, ry = COOK_RING
    stones = [(cy + math.sin(a) * ry, cx + math.cos(a) * rx, 3 + i % 2, 2)
              for i, a in enumerate(j / 9.0 * 2.0 * math.pi + 0.3 for j in range(9))]
    for sy, sx, w, h in sorted(s for s in stones if s[0] <= cy):
        pebble(c, round(sx - w / 2), round(sy - h / 2), w, h, GRAVEL)
    for y in range(COOK_H):
        for x in range(COOK_W):
            d = math.hypot((x + 0.5 - cx) / (rx - 2.0), (y + 0.5 - cy) / (ry - 0.6))
            if d < 1.0:
                heat = 1.0 - d + (hash01(x, y, 861) - 0.5) * 0.5
                c.put(x, y, EMBERS[max(0, min(3, int(heat * 4)))] if d < 0.8 else (92, 86, 92))
    for (x0, y0), (x1, y1) in (((8.5, cy + 1), (18.5, cy - 1.5)), ((9.5, cy - 1.5), (19.5, cy + 1))):
        c.line(x0, y0, x1, y1, WOOD[2])
        c.line(x0, y0 - 1, x1, y1 - 1, WOOD[4])
    for sy, sx, w, h in sorted(s for s in stones if s[0] > cy):
        pebble(c, round(sx - w / 2), round(sy - h / 2), w, h, GRAVEL)
    apex = (cx, 2)
    for foot, k in (((3, COOK_H - 3), 3), ((COOK_W - 4, COOK_H - 3), 2), ((cx + 2, COOK_H - 1), 4)):
        c.line(apex[0], apex[1], foot[0], foot[1], WOOD[k])
    c.box(cx - 1, 1, cx + 1, 2, ROPE)
    c.line(cx, 3, cx, 5, IRON[1])
    for y in range(5, 12):
        half = 4.6 - max(0, y - 9) * 0.9
        for x in range(int(cx - half), int(cx + half) + 1):
            k = 2 if x < cx - 2 else 1 if x < cx + 2 else 0
            c.put(x, y, IRON[k] if y > 5 else IRON[2])
    c.box(cx - 4, 5, cx + 4, 5, IRON[2])
    c.outline(OUTLINE)
    for x, y in ((cx - 1, 3), (cx - 2, 1), (cx - 1, 0)):
        c.glaze(x, y, (240, 244, 250), 120)
    return c.img


COT_W, COT_H = 36, 19


def cot():
    """A camp bed: a canvas sling between two wooden rails on crossed legs, a pillow at its head
    and a grey blanket thrown over it, rumpled where someone's been lying."""
    c = Canvas(COT_W, COT_H)
    for x in (4, 6, COT_W - 7, COT_W - 5):
        c.line(x, 12, x + (2 if x in (4, COT_W - 7) else -2), COT_H - 2, WOOD[2])
    for y in range(4, 11):
        for x in range(3, COT_W - 3):
            c.put(x, y, CANVAS[3] if y < 7 else CANVAS[2])
    for x in range(10, COT_W - 3):
        for y in range(3, 12):
            fold = (x * 3 + y * 2) % 11 == 0
            k = 3 if y < 6 else 2 if y < 10 else 1
            c.put(x, y, WOOL[max(0, k - 1)] if fold else WOOL[k])
    for y in range(3, 12):
        c.put(10, y, WOOL[1])
        c.put(11, y, WOOL[3])
    for x in range(4, 10):
        for y in range(4, 9):
            if (x, y) in ((4, 4), (9, 4), (4, 8), (9, 8)):
                continue
            c.put(x, y, LINEN[3] if y < 6 else LINEN[2] if x < 8 else LINEN[1])
    for x in range(2, COT_W - 2):
        c.put(x, 2, WOOD[4])
        c.put(x, 11, WOOD[4])
        c.put(x, 12, WOOD[2])
    c.outline(OUTLINE)
    return c.img


STATION_W, STATION_H = 50, 36


def healer_station():
    """Where Mira works: a trestle table under a white cloth, on it brown bottles stoppered with
    cork, a green one, rolled bandages, a basin of water and a stone mortar, and a bunch of herbs;
    a crate of more under it and a bucket beside it."""
    c = Canvas(STATION_W, STATION_H)
    for x in (6, 9, STATION_W - 10, STATION_W - 7):
        c.line(x, 22, x + (2 if x in (6, STATION_W - 10) else -2), STATION_H - 2, WOOD[2])
    for x in range(14, 26):
        for y in range(24, STATION_H - 2):
            c.put(x, y, WOOD[3] if y in (24, 29) else WOOD[2] if x < 20 else WOOD[1])
    for y in range(12, 24):
        for x in range(3, STATION_W - 3):
            if y < 17:
                k = 3 if y < 14 else 2
            else:
                k = 2 if x % 7 else 1
                if y == 23:
                    k = 1
            c.put(x, y, LINEN[k])
    for x, top, glass in ((7, 4, BOTTLE), (11, 6, BOTTLE), (14, 5, HEDGE[1:4])):
        for y in range(top, 15):
            c.put(x, y, glass[2] if y > top + 1 else glass[1])
            c.put(x + 1, y, glass[1] if y > top + 1 else glass[0])
            c.put(x + 2, y, glass[0])
        c.put(x, top + 3, glass[2] if glass is BOTTLE else (200, 236, 180))
        c.put(x + 1, top - 1, PATH[3])
    for x in (19, 23):
        for y in range(9, 14):
            c.put(x, y, LINEN[3] if y < 11 else LINEN[2])
            c.put(x + 1, y, LINEN[2])
            c.put(x + 2, y, LINEN[1])
        c.put(x + 1, 9, LINEN[1])
    for x in range(28, 38):
        for y in range(8, 14):
            half = 5 - abs((y - 8) / 5 - 0.4) * 2
            if abs(x + 0.5 - 33) > half:
                continue
            c.put(x, y, STREAM[4] if y == 9 else STREAM[3] if y == 10 else WOOD[3] if x < 33 else WOOD[2])
    c.box(28, 8, 37, 8, WOOD[4])
    for x in range(40, 45):
        for y in range(9, 14):
            c.put(x, y, GRAVEL[3] if x < 42 else GRAVEL[2] if y < 12 else GRAVEL[1])
    c.line(43, 6, 44, 10, WOOD[3])
    for x, y in ((24, 3), (25, 2), (26, 4), (27, 3), (23, 5), (28, 5)):
        c.put(x, y, HEDGE[4])
        c.put(x, y + 1, HEDGE[3])
    c.line(25, 5, 26, 10, HEDGE[2])
    for x in range(STATION_W - 7, STATION_W - 2):
        for y in range(STATION_H - 9, STATION_H - 2):
            c.put(x, y, IRON[2] if y == STATION_H - 9 else WOOD[3] if x < STATION_W - 4 else WOOD[2])
    c.outline(OUTLINE)
    return c.img


BUNDLE_W, BUNDLE_H = 28, 19


def refugee_bundle():
    """All someone could carry: a big bundle tied up in a faded blue cloth knotted at the top, a
    blanket rolled and strapped in front of it, and a dented pot."""
    c = Canvas(BUNDLE_W, BUNDLE_H)
    for y in range(2, 16):
        for x in range(1, 16):
            d = math.hypot((x + 0.5 - 8) / 7.0, (y + 0.5 - 9.5) / 6.6)
            if d > 1.0:
                continue
            light = -((x + 0.5 - 8) * 0.7 + (y + 0.5 - 9.5)) / 7
            k = 1.15 if light > 0.4 else 1.0 if light > -0.3 else 0.78
            if (x + y) % 5 == 0:
                k *= 0.88
            c.put(x, y, shade(PATCHES[0], k))
    for x, y in ((7, 1), (8, 0), (9, 1), (6, 2), (10, 2)):
        c.put(x, y, shade(PATCHES[0], 1.2))
    for x in range(18, 26):
        for y in range(4, 11):
            if abs(x + 0.5 - 22) + abs(y + 0.5 - 7.5) * 0.4 > 5:
                continue
            c.put(x, y, IRON[2] if y == 4 else IRON[1] if x < 22 else IRON[0])
    c.box(18, 4, 25, 4, IRON[2])
    for y in range(11, 17):
        for x in range(7, 26):
            k = 3 if y < 13 else 2 if y < 15 else 1
            col = WOOL[k]
            if x in (11, 12, 20, 21):
                col = shade(RED, 0.9 if k >= 2 else 0.7)
            c.put(x, y, col)
    for y in range(11, 17):
        c.put(7, y, WOOL[0] if y > 13 else WOOL[1])
    for x in (15, 16):
        for y in range(11, 17):
            c.put(x, y, TWINE[0])
    c.outline(OUTLINE)
    return c.img


BRIDGE_W, BRIDGE_H = 80, 88

#: The bridge's parts, from the top of the picture: the north parapet, the deck you walk over
#: (three tiles, the road's width), the south parapet, then its face, with the arch over the water.
BRIDGE_DECK = (10, 58)
BRIDGE_FACE = 68

#: The arch: the middle of its springing line, and how far it reaches across and up.
BRIDGE_ARCH = (40.0, 88.0, 24.0, 17.0)

#: How wide the ends of the bridge are, standing on the banks either side of the water.
BRIDGE_ENDS = 16


def bridge_blocks(c, x0, x1, y0, y1, seed):
    """Courses of blocks across a stretch of stone, each lit on its top and dark at its foot, the
    joints staggered course by course."""
    for y in range(y0, y1 + 1):
        course, row = (y - y0) // 5, (y - y0) % 5
        for x in range(x0, x1 + 1):
            if row == 4 or (x + course * 5) % 10 == 0:
                col = OLD_STONE[1]
            elif row == 0:
                col = OLD_STONE[4]
            else:
                col = OLD_STONE[3] if hash01(x, y, seed) > 0.12 else OLD_STONE[2]
            c.put(x, y, col)


def stone_bridge():
    """The old stone bridge: the road running on over it between setts worn into its edges, a low
    parapet either side, a round arch under it over the water, the ends of it standing on the banks,
    moss on its coping and ivy hanging off its side with a few pink flowers in it."""
    c = Canvas(BRIDGE_W, BRIDGE_H)
    deck_top, deck_bottom = BRIDGE_DECK
    for y in range(deck_top, deck_bottom):
        for x in range(BRIDGE_W):
            course, row = (y - deck_top) // 4, (y - deck_top) % 4
            kerb = min(y - deck_top, deck_bottom - 1 - y) + hash01(x, 0, 870) * 3
            if kerb < 7:
                joint = row == 3 or (x + course * 3) % 6 == 0
                col = OLD_STONE[1] if joint else OLD_STONE[4] if row == 0 else OLD_STONE[3]
                if joint and hash01(x, y, 872) < 0.3:
                    col = MOSS[1]
            else:
                n = hash01(x, y, 871)
                col = ROAD[2] if n < 0.08 else ROAD[4] if n > 0.9 else ROAD[3]
                if hash01(x, y, 873) < 0.02:
                    col = GRAVEL[2]
            c.put(x, y, col)
    for y0, y1 in ((0, deck_top - 1), (deck_bottom, BRIDGE_FACE - 1)):
        for y in range(y0, y1 + 1):
            for x in range(BRIDGE_W):
                r = y - y0
                if r == 0:
                    col = OLD_STONE[3]
                elif r < 4:
                    col = OLD_STONE[5] if r == 1 else OLD_STONE[4]
                    if x % 10 == 9:
                        col = OLD_STONE[2]
                else:
                    col = OLD_STONE[2] if y < y1 else OLD_STONE[0]
                    if (x + r * 3) % 10 == 0:
                        col = OLD_STONE[1]
                if r in (1, 2) and hash01(x, y, 873) < 0.12:
                    col = MOSS[2]
                c.put(x, y, col)
    for x0 in (0, BRIDGE_W - 8):
        for y in range(0, deck_top + 1):
            for x in range(x0, x0 + 8):
                c.put(x, y, OLD_STONE[5] if y < 3 and x < x0 + 6 else OLD_STONE[3] if y < 3 else OLD_STONE[2])
        for y in range(deck_bottom - 2, BRIDGE_FACE + 1):
            for x in range(x0, x0 + 8):
                top = y < deck_bottom + 1
                c.put(x, y, OLD_STONE[5] if top and x < x0 + 6 else OLD_STONE[3] if y < deck_bottom + 3 else OLD_STONE[2])
    bridge_blocks(c, 0, BRIDGE_W - 1, BRIDGE_FACE, BRIDGE_H - 1, 874)
    ax, ay, arx, ary = BRIDGE_ARCH
    for y in range(BRIDGE_FACE, BRIDGE_H):
        for x in range(BRIDGE_W):
            d = math.hypot((x + 0.5 - ax) / arx, (y + 0.5 - ay) / ary)
            if d < 1.0:
                depth = (y - (ay - ary)) / ary
                col = lerp_color((26, 30, 56), STREAM[1], max(0.0, depth - 0.45) * 1.6)
                if y > BRIDGE_H - 5:
                    col = STREAM[2] if hash01(x, y, 875) < 0.7 else STREAM[4]
                c.put(x, y, col)
            elif d < 1.22:
                a = math.atan2(y + 0.5 - ay, x + 0.5 - ax)
                wedge = int((a + math.pi) / math.pi * 11) % 2
                c.put(x, y, OLD_STONE[4] if x < ax - 6 and d < 1.1 else OLD_STONE[3 if wedge else 2])
    for x in range(BRIDGE_W):
        if hash01(x, 0, 876) < 0.5 and (x < BRIDGE_ENDS or x >= BRIDGE_W - BRIDGE_ENDS):
            c.put(x, BRIDGE_H - 1, MOSS[1])
    for vx, length in ((12, 9), (30, 5), (55, 7), (66, 10)):
        for i in range(length):
            x = vx + (1 if (i // 3) % 2 else 0)
            c.put(x, BRIDGE_FACE - 6 + i, MOSS[2] if i % 3 else MOSS[1])
            if i % 4 == 2:
                c.put(x + 1, BRIDGE_FACE - 6 + i, MOSS[2])
        c.put(vx - 1, BRIDGE_FACE - 6 + length // 2, PINK)
    c.outline(OUTLINE)
    return c.img


MILL_W, MILL_H = 104, 178

#: Where the windmill's sails turn on the front of its cap, and how long they are.
MILL_HUB = (52, 64)
MILL_SAIL = 48

#: Its tower: the top and bottom of it, and how far it reaches either side at each.
MILL_TOWER = (72, 172, 17, 25)

#: Its cap: the top and bottom of it, and how far its eaves reach either side.
MILL_CAP = (26, 76, 26)


def windmill():
    """The windmill: a tower of cream stone getting wider to its foot, an arched door at the bottom
    and a little window halfway up, under a cap of my houses' blue tiles with a gold finial, and its
    four sails across the front of it, lattices with canvas stretched along them."""
    c = Canvas(MILL_W, MILL_H)
    cx = MILL_HUB[0]
    top, bottom, narrow, wide = MILL_TOWER
    for y in range(top, bottom + 1):
        half = narrow + (wide - narrow) * (y - top) / (bottom - top)
        for x in range(MILL_W):
            dx = x + 0.5 - cx
            if abs(dx) > half:
                continue
            u = (dx + half) / (2 * half)
            course = (y - top) // 6
            joint = (y - top) % 6 == 5 or int((dx + half) / 7 + course * 0.5) != int((dx + 1 + half) / 7 + course * 0.5)
            k = 4 if u < 0.2 else 3 if u < 0.55 else 2 if u < 0.82 else 1
            if joint:
                c.put(x, y, MORTAR[0] if u > 0.4 else MORTAR[1])
                continue
            if hash01(x, y, 881) < 0.06:
                k -= 1
            c.put(x, y, STONE[max(0, k)])
    for y in range(bottom - 4, bottom + 2):
        half = wide + 2
        for x in range(int(cx - half), int(cx + half) + 1):
            u = (x + 0.5 - cx + half) / (2 * half)
            c.put(x, y, OLD_STONE[4] if y == bottom - 4 else OLD_STONE[2] if u < 0.6 else OLD_STONE[1])
    for y in range(bottom - 25, bottom - 3):
        for x in range(cx - 7, cx + 7):
            dx, dy = x + 0.5 - cx, y - (bottom - 18)
            if dy < 0 and math.hypot(dx, dy) > 7:
                continue
            plank = int(x - cx + 7) % 4 == 3
            c.put(x, y, WOOD[1] if plank else WOOD[3] if x < cx - 2 else WOOD[2])
    c.put(cx + 4, bottom - 12, GOLD[2])
    window = (top + bottom) // 2 - 6
    for y in range(window, window + 9):
        for x in range(cx - 3, cx + 4):
            dx, dy = x + 0.5 - cx, y - (window + 3)
            if dy < 0 and math.hypot(dx, dy) > 3.6:
                continue
            frame = abs(dx) > 2.4 or y == window + 8 or (dy < 0 and math.hypot(dx, dy) > 2.6)
            c.put(x, y, WOOD[2] if frame else NAVY[3] if dx < 0 and y < window + 5 else NAVY[2])
    cap_top, cap_bottom, eaves = MILL_CAP
    for y in range(cap_top, cap_bottom + 1):
        t = (y - cap_top) / (cap_bottom - cap_top)
        half = eaves * t ** 0.8 + 1
        row = (y - cap_top) % 5
        for x in range(MILL_W):
            dx = x + 0.5 - cx
            if abs(dx) > half:
                continue
            u = (dx + half) / (2 * half)
            k = 4 if u < 0.25 else 3 if u < 0.5 else 2 if u < 0.78 else 1
            scallop = int((dx + half) / 4) % 2 == (y - cap_top) // 5 % 2
            if row == 4 or (row == 3 and scallop):
                k = max(0, k - 2)
            if y >= cap_bottom - 1:
                k = 0 if y == cap_bottom else 1
            c.put(x, y, ROOF_BLUE[k])
    for y in range(cap_top - 8, cap_top + 1):
        w = 1 if y < cap_top - 3 else 2
        for x in range(cx - w, cx + w):
            c.put(x, y, GOLD[3] if x < cx else GOLD[1])
    hx, hy = MILL_HUB
    for a in (math.pi / 4 + 0.12, 3 * math.pi / 4 + 0.12, 5 * math.pi / 4 + 0.12, 7 * math.pi / 4 + 0.12):
        d, p = (math.cos(a), math.sin(a)), (-math.sin(a), math.cos(a))
        for y in range(MILL_H):
            for x in range(MILL_W):
                vx, vy = x + 0.5 - hx, y + 0.5 - hy
                along, across = vx * d[0] + vy * d[1], vx * p[0] + vy * p[1]
                if not (3 <= along <= MILL_SAIL and -1.2 <= across <= 8.5):
                    continue
                if across < 1.0:
                    col = WOOD[4] if across < 0 else WOOD[2]
                elif along > MILL_SAIL - 1.2 or across > 7.4:
                    col = WOOD[3]
                elif along > 9 and int(along) % 6 == 0:
                    col = WOOD[2]
                elif along < 9:
                    continue
                else:
                    lit = d[0] < 0
                    col = LINEN[3] if lit and across < 4 else LINEN[2] if lit or across < 3 else LINEN[1]
                c.put(x, y, col)
    for y in range(hy - 4, hy + 5):
        for x in range(hx - 4, hx + 5):
            r = math.hypot(x - hx, y - hy)
            if r < 4.2:
                c.put(x, y, WOOD[4] if r < 1.6 else WOOD[2] if x < hx else WOOD[1])
    for fx, fy, col in ((cx - 18, bottom - 1, PINK), (cx - 14, bottom, WHITE), (cx + 16, bottom - 1, YELLOW),
                        (cx + 20, bottom, PINK), (cx - 21, bottom, YELLOW)):
        c.put(fx, fy, col)
        c.put(fx - 1, fy + 1, HEDGE[3])
        c.put(fx + 1, fy + 1, HEDGE[2])
    c.outline(OUTLINE)
    return c.img


def meadow_drawn():
    """The props drawn here, by name."""
    return {
        "windmill": windmill(),
        "hay-bale": hay_bale(),
        "haystack": haystack(),
        "fence-wood": fence_wood(),
        "fence-wood-post": fence_wood_post(),
        "fence-wood-side": fence_wood_side(),
        "stone-bridge": stone_bridge(),
        "wheat": wheat(),
        "scarecrow": scarecrow(),
        "tent-ragged": tent_ragged(),
        "tent-ragged-2": tent_ragged_2(),
        "cookfire": cookfire(),
        "cot": cot(),
        "healer-station": healer_station(),
        "refugee-bundle": refugee_bundle(),
    }


# --------------------------------------------------------------------------- #
# The props cut off my sheets: the great oak, the wild flowers, the farmhouse, the east gate and the wall
# --------------------------------------------------------------------------- #
# The same cutter as Questeria's (cut_supplied.py's), so they come out in the same style. The great
# oak and the farmhouse are put together out of parts the way cut_supplied.py builds the houses:
# laid out in sheet pixels, back to front, and sampled down at once. The east gate and the stretch
# of wall either side of it are cut whole.

TREES, HOUSES, GRAND = "questeria-trees.png", "questeria-houses.png", "questeria-grand.png"

#: The great oak: my trees sheet's oak, with two more crowns behind it to make it spread, the widest
#: trunk on the sheet in front of its own, roots and flowers round its foot, and a clump of leaves
#: over where the two trunks meet.
GREAT_OAK = (
    (TREES, (592, 242, 163, 163), (-44, 22)),
    (TREES, (965, 248, 130, 156), (128, 18)),
    (TREES, (598, 12, 180, 228), (0, 22)),
    (TREES, (414, 562, 146, 107), (17, 176)),
    (TREES, (902, 409, 91, 101), (44, 138)),
)

#: How many sheet pixels an art pixel takes for the great oak: the same as the trees on the road,
#: so it's bigger by being more tree, not by being drawn bigger.
GREAT_OAK_PITCH = 2.0

#: The wild flowers: the name, the box round it on my trees sheet, and how many sheet pixels an
#: art pixel takes (the same as the fern and the long grass).
WILDFLOWERS = (
    ("wildflowers", (641, 836, 58, 82), 3.0),
    ("wildflowers-yellow", (772, 843, 64, 76), 3.0),
    ("wildflowers-blue", (850, 845, 83, 77), 3.0),
)

#: The farmhouse: the cottage cut_supplied.py builds (its walls, porch and big hipped roof), with a
#: chimney on the roof and a barrel either side, and its roof turned the red of a farm's tiles. The
#: middle of the bottom of its door, in sheet pixels, is the same as the cottage's.
FARMHOUSE = (
    (GRAND, (1397, 188, 65, 130), (73, 162)),
    (HOUSES, (215, 204, 64, 145), (10, 162)),
    (HOUSES, (1047, 206, 47, 149), (137, 158)),
    (HOUSES, (122, 360, 87, 120), (62, 187)),
    (HOUSES, (15, 11, 203, 172), (0, 0), ROOF),
    (HOUSES, (1173, 8, 40, 78), (146, -30)),
    (HOUSES, (420, 590, 21, 39), (0, 268), OUTSIDE),
    (HOUSES, (625, 580, 28, 47), (172, 260), OUTSIDE),
)
FARMHOUSE_DOOR = (105, 307)

#: The farm's roof: a warm red, as the hue in degrees, saturation and brightness its tiles are moved
#: round, the way cut_supplied.py moves its roofs to their other colours.
TERRACOTTA = (10, 0.62, 0.84)

#: The east gate: my city gate, the same picture cut_supplied.py cuts for Questeria's south gate.
#: Its box starts under the railing over it on my sheet, which the tips of its finials touch.
EAST_GATE_BOX = (8, 679, 350, 316)

#: How many sheet pixels an art pixel takes for the east gate: the same as Questeria's south gate.
EAST_GATE_PITCH = 350 / 190

#: The way through the east gate, in art pixels: left, top, right and bottom. My picture paints the
#: road through it, but anyone walking it is behind the gate (they're further up the screen than its
#: foot), so the road would be painted over them; it's cleared, and the ground shows through instead.
EAST_GATE_WAY = (75, 92, 113, 175)

#: A stretch of the city wall, crenellated, with flowers along its top, to stand either side of the
#: gate. It's a prop of its own rather than part of the gate's picture, so it sorts from its own
#: foot, and whoever's in front of it is drawn in front of it.
CITY_WALL_BOX = (966, 482, 128, 109)


def reroof(img, roof, colour):
    """A copy of a picture with its roof tiles moved to a colour of their own: cut_supplied.py's
    recolour, to any colour rather than one of its four."""
    out = [[list(px) for px in line] for line in img]
    th, ts, tv = colour
    for y, line in enumerate(out):
        for x, px in enumerate(line):
            if not px[3] or not roof[y][x]:
                continue
            own = roof_colour_of(*px[:3])
            if own is None:
                continue
            sh, ss, sv = ROOF_COLOURS[own]
            h, s, v = colorsys.rgb_to_hsv(px[0] / 255, px[1] / 255, px[2] / 255)
            turn = ((h * 360 - sh + 180) % 360 - 180) * HUE_KEPT
            h = ((th + turn) % 360) / 360
            line[x] = [round(c * 255) for c in colorsys.hsv_to_rgb(h, min(1.0, s * ts / ss), min(1.0, v * tv / sv))] + [255]
    return out


def meadow_cuts():
    """The props cut off my sheets, by name. Prints where the farmhouse's door is from its foot,
    for the maps' entrances."""
    cuts = {}
    for name, box, pitch in WILDFLOWERS:
        cuts[name] = cut_out(TREES, box, pitch=pitch)
    built = parts_of([GREAT_OAK, FARMHOUSE])
    cuts["great-oak"] = put_together("great-oak", GREAT_OAK, (), GREAT_OAK_PITCH, built)[0]
    img, roof, art = put_together("farmhouse", FARMHOUSE, (), KITBASH_PITCH, built)
    cuts["farmhouse"] = reroof(img, roof, TERRACOTTA)
    dx, dy = art(*FARMHOUSE_DOOR)
    print("    farmhouse door at (%d, %d) from its foot" % (dx - len(img[0]) // 2, dy - len(img)))
    gate = cut_out(GRAND, EAST_GATE_BOX, pitch=EAST_GATE_PITCH, alone=True)
    left, top, right, bottom = EAST_GATE_WAY
    for y in range(top, bottom + 1):
        for x in range(left, right + 1):
            gate[y][x] = [0, 0, 0, 0]
    cuts["east-gate"] = gate
    cuts["city-wall"] = cut_out(GRAND, CITY_WALL_BOX, pitch=EAST_GATE_PITCH)
    return cuts


if __name__ == "__main__":
    print("Generating the meadow art for Legend Quest...")
    print("  Tiles/meadow.png: painted by tools/rooms/paint_tiles.py now; not written here")
    for made in (meadow_drawn, meadow_cuts):
        for name, picture in made().items():
            write_png(os.path.join(OUT, "Props", name + ".png"), picture)
    print("Done.")
