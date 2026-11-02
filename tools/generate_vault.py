"""
The Old Crown Vault, under the Sunspire: its tileset and its props.

    python tools/generate_vault.py

The vault is the palace's own stone gone underground: the same pale cream ashlar as Questeria's
terrace, only dressed finer and polished, with gold let into it and lapis round the gold, the way my
style sheet does the plaza's inlays. The halls have runners of marble lozenges between bands of
gold, the walls a gilded cornice and the sun of the old kings carved here and there, and the
treasure is heaped where the torchlight catches it. Against all that, the Hollowed sappers' work:
raw black earth and broken stone where they've dug up into it from underneath.

It's drawn the way generate_world.py draws the barrow, so the two sit together: floors in blocks
two tiles each way so the slabs don't line up, walls as their tops and then two rows of face, and
detail drawn over the floor and outlined in the dark. The pit and the earth have edges the tilemap
draws wherever they meet other ground, like Questeria's lawns: the pit its far wall and its lips,
the earth the broken ends of the slabs it's torn through. The props are drawn to match my props
sheet: dark outlines, lit from the top left, gold that goes from brown to nearly white. The relic
shelves, the cases and the urns fill the gallery; the doors are blued steel bound in gold.

Everything is drawn as if a torch were right on it. The map is dark, and its lamps and braziers
make the pools of light, so away from them it's black with the gold just showing.

Only needs CPython. Every bit of noise is hash01 on where it is or a seeded Random, so a rerun
writes the same bytes.
"""

import math
import os
import random
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from pixels import OUT, hash01, lerp_color, new_image, write_png  # noqa: E402
from generate_harbour import T, Canvas, build_sheet, dither, put, side_by_side, tile, turned  # noqa: E402
from generate_world import BONE, IRON, bone, cells_of, cobweb, detail, lobed, skull  # noqa: E402

# --------------------------------------------------------------------------- #
# The palette: the terrace's cream stone with lavender shadows, the plaza's gold and lapis, and
# the earth the Hollowed dig through
# --------------------------------------------------------------------------- #
# Every ramp goes dark to light.

IVORY = ((48, 40, 58), (80, 70, 88), (116, 104, 116), (154, 142, 146), (190, 178, 170), (218, 206, 190),
         (240, 230, 210))
GOLD = ((98, 56, 28), (158, 100, 38), (210, 152, 54), (242, 198, 90), (255, 238, 168))
BRONZE = ((54, 32, 30), (88, 56, 38), (124, 82, 46), (160, 112, 54))
STEEL = ((16, 20, 46), (26, 36, 76), (40, 56, 112), (62, 86, 150), (98, 128, 186))
LAPIS = ((20, 28, 72), (32, 52, 120), (50, 84, 164), (88, 128, 206), (156, 188, 238))
EARTH = ((22, 15, 20), (38, 27, 30), (58, 40, 38), (84, 60, 48), (112, 84, 62), (142, 110, 80))
VELVET = ((68, 18, 36), (110, 30, 50), (154, 46, 62), (196, 78, 84))
WOOD = ((38, 22, 28), (62, 36, 38), (90, 54, 48), (120, 76, 60), (150, 102, 76))
GEMS = ((150, 30, 52), (226, 70, 84), (40, 120, 92), (90, 200, 150), (60, 90, 200), (140, 180, 250))
EMBER = ((120, 30, 22), (196, 70, 32), (250, 150, 60), (255, 222, 140))
PARCHMENT = ((150, 120, 92), (200, 176, 136), (236, 220, 184))

#: The mortar in the walls, dark like the barrow's.
GROUT = (40, 32, 48)

#: The joints between the floor's slabs: darker than the stone, but nowhere near as dark as the
#: walls', since this stone is pale and laid close.
FLOOR_JOINT = (106, 92, 106)

#: The outline round the props: a warm near-black, like my props sheet's.
INK = (32, 20, 30)

#: The dark in a pit, a hole or a doorway: not quite black, so it still reads as a place.
DARK = (10, 7, 14)

#: A few slabs are tinted: warm, as if the gold had rubbed off on them, or lavender.
SLAB_TINTS = ((226, 192, 140), (164, 150, 200))


def shade_of(ramp, k):
    """A step of a ramp, kept on the ramp."""
    return ramp[max(0, min(len(ramp) - 1, k))]


# --------------------------------------------------------------------------- #
# The floor: polished flagstones in courses, in their shadow, and cracked
# --------------------------------------------------------------------------- #
# Laid in blocks two tiles each way like the barrow's, but in courses, the way the terrace above is
# laid: three courses a block, each as deep as it likes and with its own joints, so the joints
# never line up down the rows. Every block has a joint along its top and its left, so any two
# meet. The slabs are polished, so some have a pale vein running across them, and a few are tinted.

FLOOR = 2 * T

#: Each layout's courses: how deep each is, and where its joints are besides the one at the left
#: every course has.
FLOOR_COURSES = (
    ((11, (13,)), (11, (6, 22)), (10, (17,))),
    ((10, (9, 24)), (12, (16,)), (10, (4, 21))),
    ((12, (20,)), (10, (8, 26)), (10, (13,))),
    ((11, (5, 19)), (10, (11, 27)), (11, (22,))),
)


def slab_block(layout, seed):
    """One block of the floor: every slab lit along its top and in shadow along its bottom and
    right, a shade and maybe a tint of its own, pitted here and there, and some veined."""
    img = new_image(FLOOR, FLOOR)
    y1 = 0
    for course, (depth, joints) in enumerate(FLOOR_COURSES[layout]):
        y0, y1 = y1, y1 + depth
        edges = [0] + list(joints) + [FLOOR]
        for i, (x0, x1) in enumerate(zip(edges, edges[1:])):
            key = layout * 8 + course * 3 + i
            pick = hash01(key, 1, seed)
            base = 4 if pick < 0.55 else 3 if pick < 0.85 else 5
            n_tint = hash01(key, 2, seed)
            tint = SLAB_TINTS[0] if n_tint < 0.16 else SLAB_TINTS[1] if n_tint > 0.86 else None
            veined = hash01(key, 3, seed) < 0.4
            vein_at = hash01(key, 4, seed) * (x1 - x0)
            for y in range(y0, y1):
                for x in range(x0, x1):
                    if y == y0 or x == x0:
                        colour = FLOOR_JOINT
                    else:
                        if y == y0 + 1:
                            k = base + 1
                        elif y == y1 - 1 or x == x1 - 1:
                            k = base - 1
                        else:
                            k = base
                            n = hash01(x, y, seed + 5)
                            if n < 0.05:
                                k -= 1
                            elif veined and abs((x - x0) - vein_at - (y - y0) * 0.7) < 0.6 and dither(x, y, 0.7):
                                k += 1
                        colour = shade_of(IVORY, k)
                        if tint:
                            colour = lerp_color(colour, tint, 0.2)
                    img[y][x] = [colour[0], colour[1], colour[2], 255]
    return img


def floor_blocks(change=None):
    """Every layout of the floor, each as its four tiles, with change (a colour for a pixel, from
    where it is in its block) made to every pixel if it's given."""
    cells = []
    for layout in range(len(FLOOR_COURSES)):
        block = slab_block(layout, 701)
        if change:
            block = change(block, layout)
        cells += cells_of(block)
    return cells


def in_shadow(block, layout):
    """The floor right under a wall's face: darker the nearer it is to the wall, which is the top
    of each tile."""
    for y in range(FLOOR):
        for x in range(FLOOR):
            colour = lerp_color(block[y][x][:3], IVORY[0], 0.6 * (1.0 - (y % T) / T))
            block[y][x] = [colour[0], colour[1], colour[2], 255]
    return block


def crack_line(img, x, y, dx, dy, steps, rng):
    """A crack wandering across a block from x, y, mostly the way dx, dy goes: dark along the
    crack and lit along its lower right, the way a cut is lit from the top left. Returns where it
    stopped, for a branch to start from."""
    fx, fy = float(x), float(y)
    size = len(img)
    path = []
    for _ in range(steps):
        fx += dx + rng.uniform(-0.55, 0.55)
        fy += dy + rng.uniform(-0.55, 0.55)
        px, py = int(round(fx)) % size, int(round(fy)) % size
        path.append((px, py))
    for px, py in path:
        img[py][px] = list(IVORY[1]) + [255]
    for px, py in path:
        lx, ly = (px + 1) % size, (py + 1) % size
        if (lx, ly) not in path and tuple(img[ly][lx][:3]) != FLOOR_JOINT:
            lit = lerp_color(img[ly][lx][:3], IVORY[6], 0.35)
            img[ly][lx] = [lit[0], lit[1], lit[2], 255]
    return path[len(path) // 2]


def cracked(block, layout):
    """The floor heaved and split: a crack across the block with a branch off it, and a corner
    broken off a slab, where the ground's moving underneath."""
    rng = random.Random(711 + layout)
    starts = ((2, 5, 1.0, 0.55), (4, 27, 1.0, -0.5), (6, 1, 0.45, 1.0), (1, 14, 1.0, 0.15))
    x, y, dx, dy = starts[layout]
    mx, my = crack_line(block, x, y, dx, dy, 30, rng)
    crack_line(block, mx, my, dy, dx, 9, rng)
    cx, cy = rng.randint(6, 24), rng.randint(6, 24)
    for oy in range(3):
        for ox in range(3 - oy):
            block[cy + oy][cx + ox] = list(IVORY[1] if ox + oy < 2 else IVORY[2]) + [255]
    return block


# --------------------------------------------------------------------------- #
# The inlay: a runner of marble lozenges between bands of gold, and the sun crest
# --------------------------------------------------------------------------- #
# The lozenges are set on the diagonal with a thread of gold between them and a lapis stud where
# the threads cross, the way the plaza's medallions are. The pattern repeats every tile, so a field
# of it has no seams. Where the inlay meets any other floor, the tilemap draws its edge: a band of
# gold lit along its top, and a line of lapis inside it.

def inlay(variant):
    """Polished marble lozenges, every other one a shade warmer, with gold threads between them and
    lapis studs where they cross. The second variant has a little gold sun in the middle instead of
    a stud, and the third a lozenge cracked across."""
    img = tile()
    for y in range(T):
        for x in range(T):
            a, b = (x + y) % T, (x - y) % T
            if a == 0 or b == 0:
                colour = GOLD[3] if (a == 0 and b != 0 and x < 8) or (b == 0 and a != 0 and y < 8) else GOLD[2]
            else:
                warm = ((x + y) // T + (x - y) // T) % 2
                k = 5 if a == 1 else 3 if a == T - 1 else 4
                if hash01(x, y, 721) < 0.05:
                    k -= 1
                colour = shade_of(IVORY, k)
                if warm:
                    colour = lerp_color(colour, SLAB_TINTS[0], 0.32)
            put(img, x, y, colour)
    for sx, sy in ((0, 0), (8, 8)):
        sun = variant == 1 and (sx, sy) == (8, 8)
        for dy in range(-3, 4):
            for dx in range(-3, 4):
                d = abs(dx) + abs(dy)
                if sun:
                    if d <= 1:
                        colour = GOLD[4] if dx + dy < 0 or d == 0 else GOLD[3]
                    elif d == 2:
                        colour = GOLD[2]
                    elif d == 3 and (dx == 0 or dy == 0):
                        colour = GOLD[3]
                    else:
                        continue
                elif d == 0:
                    colour = LAPIS[4]
                elif d == 1:
                    colour = LAPIS[3] if dx + dy < 0 else LAPIS[1]
                elif d == 2:
                    colour = GOLD[3] if dx + dy < 0 else GOLD[1]
                else:
                    continue
                put(img, sx + dx, sy + dy, colour)
    if variant == 2:
        for x, y in ((10, 3), (11, 4), (11, 5), (12, 6), (13, 6)):
            put(img, x, y, IVORY[2])
            put(img, x + 1, y + 1, IVORY[6])
    return img


def inlay_edges():
    """The inlay's border, wherever it meets other floor: a joint, a band of gold lit along its top
    (on the north and west, in shadow on the south and east), and a line of lapis inside it with a
    gold dot every four pixels."""
    edges = []
    for side in range(4):
        lit = side in (0, 3)
        img = tile()
        for x in range(T):
            put(img, x, 0, FLOOR_JOINT)
            put(img, x, 1, GOLD[3] if lit else GOLD[2])
            put(img, x, 2, GOLD[2] if lit else GOLD[1])
            put(img, x, 3, GOLD[0])
            put(img, x, 4, GOLD[3] if x % 4 == 2 else LAPIS[2] if lit else LAPIS[1])
            put(img, x, 5, LAPIS[0])
        edges.append(turned(img, side))
    return edges


#: The sun crest: how far out its boss, its long rays, its short rays and its ring go, in art pixels.
CREST_BOSS = 3.6
CREST_LONG = 12.5
CREST_SHORT = 9.0
CREST_RING = 14.0
CREST_RAYS = 12


def sun_at(c, cx, cy, boss, long_ray, short_ray, rays, ring=None):
    """The sun of the old kings, on a canvas: a gold boss lit on its top left, rays round it, long
    and short in turn, each lit down its upper left side, on a field of lapis inside a gold ring if
    there's a ring."""
    reach = ring + 1.6 if ring else long_ray + 1
    for y in range(int(cy - reach) - 1, int(cy + reach) + 2):
        for x in range(int(cx - reach) - 1, int(cx + reach) + 2):
            dx, dy = x + 0.5 - cx, y + 0.5 - cy
            d = math.hypot(dx, dy)
            a = math.atan2(dy, dx)
            if d < boss:
                lit = (dx + dy) / max(boss, 0.1)
                colour = GOLD[4] if lit < -0.6 else GOLD[3] if lit < 0.3 else GOLD[2]
            elif ring and ring <= d < ring + 1.6:
                colour = GOLD[3] if dx + dy < 0 else GOLD[1]
            else:
                step = 2 * math.pi / rays
                off = (a + step / 2) % step - step / 2
                index = int(round((a - off) / step)) % rays
                length = long_ray if index % 2 == 0 else short_ray
                width = 0.42 * (1.0 - (d - boss) / max(length - boss, 0.1))
                if d < length and abs(off) < width:
                    colour = GOLD[3 if off < 0 else 2] if dx + dy < 0 else GOLD[2 if off < 0 else 1]
                elif ring and d < ring:
                    colour = LAPIS[0] if d > ring - 1.2 else LAPIS[1]
                else:
                    continue
            c.put(x, y, colour)


def sun_crest():
    """A sun crest let into the floor, two tiles each way: the sun on lapis in a gold ring, the
    corners outside the ring pale marble with a gold quarter-circle in each, and a gold frame round
    the whole square."""
    c = Canvas(FLOOR, FLOOR)
    mid = FLOOR / 2
    for y in range(FLOOR):
        for x in range(FLOOR):
            if x == 0 or y == 0:
                colour = FLOOR_JOINT
            elif x == 1 or y == 1:
                colour = GOLD[3]
            elif x == FLOOR - 1 or y == FLOOR - 1:
                colour = GOLD[0]
            elif x == FLOOR - 2 or y == FLOOR - 2:
                colour = GOLD[1]
            else:
                k = 5 if hash01(x, y, 731) > 0.06 else 4
                colour = IVORY[k]
                corner = min(math.hypot(x + 0.5 - cx, y + 0.5 - cy) for cx in (2, FLOOR - 2) for cy in (2, FLOOR - 2))
                if 5.0 < corner < 6.4:
                    colour = GOLD[2]
                elif corner <= 5.0:
                    colour = LAPIS[2]
            c.put(x, y, colour)
    sun_at(c, mid, mid, CREST_BOSS, CREST_LONG, CREST_SHORT, CREST_RAYS, ring=CREST_RING)
    return cells_of(c.img)


# --------------------------------------------------------------------------- #
# Traps: the pressure plate and the pit
# --------------------------------------------------------------------------- #

def pressure_plate(variant):
    """A slab with a plate set into it: a square of stone a shade warmer than the floor round it,
    a dark seam all the way round where it sinks, and a little bronze sun in the middle. You can
    see them if you look; the people who didn't are why there are bones in the hall. The second
    has sunk a little already, its seam wider along the top and left and its face in shadow."""
    img = tile()
    sunk = variant == 1
    for y in range(T):
        for x in range(T):
            ring = 3 <= x <= 12 and 3 <= y <= 12 and (x in (3, 12) or y in (3, 12))
            inside = 4 <= x <= 11 and 4 <= y <= 11
            if x == 0 or y == 0:
                colour = FLOOR_JOINT
            elif ring or sunk and inside and (x == 4 or y == 4):
                colour = IVORY[1]
            elif inside:
                lit = not sunk and (x in (4, 5) or y in (4, 5))
                k = 5 if lit else 3 if y == 11 or x == 11 else 4
                if sunk:
                    k -= 1
                colour = lerp_color(shade_of(IVORY, k), GOLD[1], 0.22)
            else:
                k = 5 if y == 1 else 3 if y == T - 1 or x == T - 1 else 4
                if 0 < x < T - 1 and 1 < y < T - 1 and hash01(x, y, 741) < 0.06:
                    k -= 1
                colour = shade_of(IVORY, k)
            put(img, x, y, colour)
    for dx, dy, colour in ((0, 0, GOLD[3]), (1, 0, GOLD[2]), (0, 1, GOLD[2]), (1, 1, GOLD[1]),
                           (0, -2, BRONZE[2]), (1, -2, BRONZE[2]), (0, 3, BRONZE[1]), (1, 3, BRONZE[1]),
                           (-2, 0, BRONZE[2]), (-2, 1, BRONZE[2]), (3, 0, BRONZE[1]), (3, 1, BRONZE[1])):
        put(img, 7 + dx + (1 if sunk else 0), 7 + dy + (1 if sunk else 0), colour)
    return img


def pit():
    """The bottom of a pit, a long way down: nearly black, with iron spikes standing up out of
    it, the odd one catching the light."""
    img = tile()
    for y in range(T):
        for x in range(T):
            put(img, x, y, DARK if hash01(x, y, 751) > 0.12 else (18, 13, 24))
    for sx, sy in ((3, 6), (9, 3), (12, 10), (6, 12), (14, 1)):
        put(img, sx, sy, IRON[2])
        put(img, sx, sy + 1, IRON[1])
        put(img, sx + 1, sy + 1, IRON[0])
        put(img, sx, sy + 2, IRON[0])
    return img


#: How deep the far wall of a pit shows, in art pixels, from its lip down into the dark.
PIT_WALL = 9


def pit_edges():
    """A pit's edges. Along the north it's its far wall, which you look straight at: the lip of the
    floor lit along its top, then courses of stone going down into the dark. The sides are a
    narrower strip of wall seen edge on, and the south is only the near lip, since that wall faces
    away from you."""
    north = tile()
    for x in range(T):
        put(north, x, 0, IVORY[6])
        put(north, x, 1, IVORY[3])
        for y in range(2, 2 + PIT_WALL):
            depth = (y - 2) / PIT_WALL
            joint = y in (5, 8) or y > 2 and (x + (4 if y > 5 else 0)) % 8 == 0
            colour = lerp_color(IVORY[2] if not joint else IVORY[1], DARK, depth ** 0.8)
            put(north, x, y, colour)
    east = tile()
    west = tile()
    for y in range(T):
        put(west, 0, y, IVORY[5])
        put(west, 1, y, IVORY[2])
        put(west, 2, y, lerp_color(IVORY[1], DARK, 0.5))
        put(east, T - 1, y, IVORY[3])
        put(east, T - 2, y, IVORY[1])
    south = tile()
    for x in range(T):
        put(south, x, T - 1, IVORY[2])
        put(south, x, T - 2, IVORY[0])
    return [north, east, south, west]


# --------------------------------------------------------------------------- #
# The Hollowed sappers' work: raw earth, and the broken floor round it
# --------------------------------------------------------------------------- #

def earth(variant):
    """Raw black earth where the floor's been dug out from underneath: clods lit on their tops, a
    root, bits of pale stone churned up in it, and the gouges of picks. The sappers' tunnels are
    floored with the same earth, only without the edges, since where it meets the banks either side
    of a tunnel there's no floor to have broken."""
    img = tile()
    for y in range(T):
        for x in range(T):
            n = hash01(x, y, 761)
            k = 1 if n < 0.45 else 2 if n < 0.9 else 3
            put(img, x, y, EARTH[k])
    rng = random.Random(762 + variant)
    for _ in range(5):
        cx, cy = rng.randint(1, 13), rng.randint(1, 13)
        for dx, dy, k in ((0, 0, 4), (1, 0, 3), (0, 1, 3), (1, 1, 2), (2, 1, 1), (1, 2, 1)):
            put(img, cx + dx, cy + dy, EARTH[k])
    for _ in range(3):
        cx, cy = rng.randint(1, 13), rng.randint(1, 13)
        put(img, cx, cy, IVORY[4])
        put(img, cx + 1, cy, IVORY[3])
        put(img, cx + 1, cy + 1, EARTH[0])
    for _ in range(2):
        gx, gy = rng.randint(0, 12), rng.randint(0, 12)
        for i in range(4):
            put(img, gx + i, gy + i // 2, EARTH[0])
            put(img, gx + i, gy + i // 2 + 1, EARTH[3])
    if variant == 2:
        for i in range(10):
            x, y = 2 + i, 9 + int(round(math.sin(i * 0.8) * 1.5))
            put(img, x, y, EARTH[5] if i % 3 else EARTH[4])
            put(img, x, y + 1, EARTH[1])
    return img


def broken_edge(side):
    """Where the earth meets the floor it was torn out of: the slab's broken end, ragged, lit along
    the top, then its broken face (only on the north, where it faces you) and its shadow on the
    earth below it."""
    img = tile()
    for x in range(T):
        reach = 1 + int(hash01(x // 2, side, 771) * 3.2)
        for y in range(reach):
            put(img, x, y, IVORY[5] if y == 0 and side in (0, 3) else IVORY[4] if y < reach - 1 else IVORY[3])
        if side == 0:
            put(img, x, reach, IVORY[2])
            put(img, x, reach + 1, EARTH[0])
            if hash01(x, 7, 772) < 0.5:
                put(img, x, reach + 2, EARTH[0])
        else:
            put(img, x, reach, EARTH[0])
        if hash01(x, side, 773) < 0.18:
            put(img, x, reach + 3, IVORY[3])
    return img


def earth_edges():
    """The earth's edges on all four sides, each the broken end of the floor round it."""
    edges = []
    for side in range(4):
        edges.append(turned(broken_edge(side), side))
    return edges


# --------------------------------------------------------------------------- #
# The stair, and the walls: their tops, the two rows of their faces, and the earth banks
# --------------------------------------------------------------------------- #

def steps():
    """The stair up to the terrace, two steps to a tile: each tread pale and worn in the middle,
    its nose lit with a thread of gold along it, and its riser in shadow under it."""
    img = tile()
    for y in range(T):
        row = y % 8
        for x in range(T):
            if row == 0:
                colour = IVORY[6]
            elif row == 1:
                colour = GOLD[2] if x % 8 else GOLD[1]
            elif row < 5:
                k = 5 if abs(x + 0.5 - 8) < 4 and dither(x, y, 0.5) else 4
                if hash01(x, y, 781) < 0.07:
                    k -= 1
                colour = IVORY[k]
            elif row == 5:
                colour = IVORY[2]
            else:
                colour = IVORY[1] if row == 6 else IVORY[0]
            put(img, x, y, colour)
        if 2 <= row <= 4:
            put(img, 0, y, FLOOR_JOINT)
    return img


def wall_top(variant):
    """The top of a wall, seen from above: dressed blocks in courses, darker than the floor, each
    lit along its top and left. The blocks round the edge are the same in every variant."""
    img = tile()
    courses = ((0, 5, (0, 9)), (5, 11, (4, 12) if variant == 0 else (4, 10)), (11, 16, (0, 7)))
    for top, bottom, joints in courses:
        for y in range(top, bottom):
            for x in range(T):
                if y == top or x in joints:
                    colour = GROUT
                elif y == top + 1 or x - 1 in joints:
                    colour = IVORY[2]
                elif y == bottom - 1:
                    colour = IVORY[0]
                else:
                    colour = IVORY[1] if hash01(x, y, 791 + variant) > 0.08 else IVORY[0]
                put(img, x, y, colour)
    return img


#: The courses of a wall's face under its cornice: where each starts and ends, and its joints
#: (the same in every variant, so a wall has no seams).
FACE_COURSES = ((5, 11, (5, 13)), (11, 16, (1, 9)))


def face_blocks(img, courses, seed):
    """Courses of big blocks across a wall's face, each lit along its top and left and in shadow
    along its bottom and right, a shade of its own, and pitted."""
    for top, bottom, joints in courses:
        edges = sorted(set([0] + list(joints) + [T]))
        for x in range(T):
            put(img, x, top, GROUT)
        for x0, x1 in zip(edges, edges[1:]):
            base = 4 if hash01(x0, top, seed) < 0.55 else 3
            warm = hash01(x0, top, seed + 1) < 0.2
            for y in range(top + 1, bottom):
                for x in range(x0, x1):
                    if x == x0 and x0 in joints:
                        put(img, x, y, GROUT)
                        continue
                    k = base + 1 if y == top + 1 else base - 1 if y == bottom - 1 or x == x1 - 1 else base
                    if hash01(x, y, seed + 2) < 0.06:
                        k -= 1
                    colour = shade_of(IVORY, k)
                    put(img, x, y, lerp_color(colour, SLAB_TINTS[0], 0.18) if warm else colour)


def cornice(img):
    """The top of a wall's face: a pale lip lit along its top, a gilded moulding under it, and the
    shadow under that."""
    for x in range(T):
        put(img, x, 0, IVORY[6] if x % 8 else IVORY[5])
        put(img, x, 1, IVORY[4])
        put(img, x, 2, GOLD[3] if x % 4 else GOLD[4])
        put(img, x, 3, GOLD[1])
        put(img, x, 4, IVORY[0])


def wall_face(variant):
    """The upper row of a wall's face: the cornice and two courses of pale ashlar. Plain, with a
    block's corner broken off, or with a crack running down it and a trickle of black earth
    coming through, where something's been digging behind it."""
    img = tile()
    face_blocks(img, FACE_COURSES, 801)
    cornice(img)
    if variant == 1:
        for y in range(12, 16):
            for x in range(10, 15):
                if (x - 10) + (y - 12) < 4:
                    put(img, x, y, IVORY[1] if (x - 10) + (y - 12) < 2 else IVORY[2])
        for x in range(10, 14):
            put(img, x, 15 - max(0, x - 11), IVORY[5])
    if variant == 2:
        path = ((7, 5), (7, 6), (8, 7), (8, 8), (7, 9), (7, 10), (6, 11), (6, 12), (7, 13), (7, 14), (8, 15))
        for x, y in path:
            put(img, x, y, IVORY[0])
            put(img, x + 1, y, IVORY[5] if (x + 1, y) not in path else IVORY[0])
        for x, y in ((6, 13), (6, 14), (7, 15), (5, 15)):
            put(img, x, y, EARTH[2])
    return img


def wall_crest():
    """A wall's face with the sun of the old kings inlaid in it, gold on lapis in a gold ring."""
    img = wall_face(0)
    c = Canvas(T, T)
    c.img = img
    sun_at(c, 8.0, 10.0, 1.6, 4.6, 3.4, 8, ring=4.6)
    return c.img


def wall_niche():
    """An arched niche cut into a wall's face, lined in lapis, with a little gold urn standing in
    it catching the light."""
    img = wall_face(0)
    for y in range(5, 16):
        for x in range(3, 13):
            d = math.hypot(x + 0.5 - 8, y + 0.5 - 9)
            inside = y >= 9 or d < 4.2
            rim = not inside and (y >= 9 or d < 5.4) and 2 <= x <= 13
            if inside:
                put(img, x, y, lerp_color(LAPIS[0], DARK, 0.4 + 0.3 * (1 - (y - 5) / 10)))
            elif rim:
                put(img, x, y, GOLD[3] if x < 8 else GOLD[1])
    for x in range(3, 13):
        put(img, x, 15, IVORY[5])
    for y, (x0, x1) in zip(range(8, 15), ((7, 8), (6, 9), (5, 10), (5, 10), (6, 9), (7, 8), (6, 9))):
        for x in range(x0, x1 + 1):
            put(img, x, y, GOLD[3] if x <= x0 + (1 if y < 11 else 0) else GOLD[2] if x < x1 else GOLD[1])
    put(img, 6, 9, GOLD[4])
    return img


#: The course a wall's lower row has over its plinth, carrying on the face's joints.
BASE_COURSES = ((0, 9, (5, 13)),)


def wall_base(variant):
    """The lower row of a wall's face: a course of blocks, then the plinth with a gold line along
    its top, and a dark line where the wall meets the floor. The second has a lapis lozenge let
    into the plinth."""
    img = tile()
    face_blocks(img, BASE_COURSES, 811 + variant)
    for x in range(T):
        put(img, x, 9, GOLD[3] if x % 4 else GOLD[4])
        put(img, x, 10, GOLD[1])
        for y in range(11, T):
            k = 4 if y == 11 else 3 if y < 14 else 2
            if y == T - 1:
                put(img, x, y, GROUT)
            else:
                put(img, x, y, IVORY[k] if hash01(x, y, 812) > 0.06 else IVORY[k - 1])
    if variant == 1:
        for dy in range(-2, 3):
            for dx in range(-2, 3):
                d = abs(dx) + abs(dy)
                if d <= 2:
                    put(img, 8 + dx, 12 + dy, GOLD[2] if d == 2 else LAPIS[3] if dx + dy < 0 else LAPIS[1])
    return img


def earth_top(variant):
    """The top of a bank of earth where a tunnel's caved in, seen from above: black earth heaped
    up, darker than the tunnel's floor so you can tell the one from the other, with lumps of it and
    the odd broken stone lit on their tops."""
    img = tile()
    for y in range(T):
        for x in range(T):
            put(img, x, y, EARTH[0] if hash01(x, y, 821) > 0.25 else (30, 20, 26))
    rng = random.Random(822 + variant)
    for _ in range(6):
        cx, cy, r = rng.uniform(0, T), rng.uniform(0, T), rng.uniform(1.6, 2.8)
        stone = rng.random() < 0.25
        for y in range(int(cy - r) - 1, int(cy + r) + 2):
            for x in range(int(cx - r) - 1, int(cx + r) + 2):
                dx, dy = x + 0.5 - cx, y + 0.5 - cy
                if math.hypot(dx, dy) > r:
                    continue
                light = -(dx * 0.7 + dy) / r
                k = 2 if light > 0.4 else 1 if light > -0.3 else 0
                put(img, x, y, IVORY[k] if stone else EARTH[k])
    return img


def earth_face(variant):
    """The face of a bank of earth where the tunnel's roof came down: a ragged lip lit along its
    top, black earth with stones and a broken block in it, and roots hanging down out of it."""
    img = tile()
    for y in range(T):
        for x in range(T):
            ragged = int(hash01(x // 2, 1, 832 + variant) * 2.5)
            if y < ragged:
                continue
            if y == ragged:
                colour = EARTH[4]
            elif y == ragged + 1:
                colour = EARTH[3]
            else:
                depth = y / T
                k = 2 if hash01(x, y, 833) > 0.35 else 1
                if depth > 0.75:
                    k = max(0, k - 1)
                colour = EARTH[k]
            put(img, x, y, colour)
    rng = random.Random(834 + variant)
    for _ in range(3):
        sx, sy = rng.randint(1, 12), rng.randint(5, 12)
        for dx, dy, k in ((0, 0, 4), (1, 0, 3), (2, 0, 3), (0, 1, 3), (1, 1, 2), (2, 1, 1)):
            put(img, sx + dx, sy + dy, IVORY[k])
    for rx in ((4, 11) if variant == 0 else (7,)):
        length = 4 + int(hash01(rx, variant, 835) * 5)
        for i in range(length):
            put(img, rx + (1 if i > length // 2 else 0), 3 + i, EARTH[5] if i % 2 else EARTH[4])
    return img


# --------------------------------------------------------------------------- #
# Detail, drawn over the floor
# --------------------------------------------------------------------------- #

def coin(c, x, y, glint):
    """One gold coin lying flat: three pixels wide and two deep, lit on its top left, with its
    shadow under it, and now and then a glint."""
    c.put(x, y, GOLD[4] if glint else GOLD[3])
    c.put(x + 1, y, GOLD[3])
    c.put(x + 2, y, GOLD[2])
    c.put(x, y + 1, GOLD[2])
    c.put(x + 1, y + 1, GOLD[1])
    c.put(x + 2, y + 1, GOLD[0])
    c.glaze(x + 1, y + 2, (0, 0, 0), 90)
    c.glaze(x + 3, y + 1, (0, 0, 0), 70)


def coins(variant):
    """Gold coins spilled across the floor: a few, a lot, or a little stack with more round it."""
    c = Canvas(T, T)
    rng = random.Random(841 + variant)
    count = (5, 9, 6)[variant]
    for i in range(count):
        coin(c, rng.randint(1, 11), rng.randint(1, 12), i % 3 == 0)
    if variant == 2:
        for i in range(4):
            coin(c, 6, 9 - i * 2, i == 3)
    return c.img


def paint_rubble(c, variant):
    """A heap of broken stone off the walls and the floor: pale lumps with the odd one still
    gilded along an edge, and black earth in among them."""
    rng = random.Random(851 + variant)
    lumps = [(rng.uniform(3, 13), rng.uniform(10, 12.5), rng.uniform(2.4, 3.2), rng.random() < 0.3) for _ in range(4)]
    lumps += [(rng.uniform(5, 11), rng.uniform(6.5, 9), rng.uniform(1.8, 2.6), rng.random() < 0.2) for _ in range(3)]
    for cx, cy, r, dirt in lumps:
        phase = rng.uniform(0, 6.28)
        base = 4 if rng.random() < 0.6 else 3
        for y in range(T):
            for x in range(T):
                dx, dy = x + 0.5 - cx, (y + 0.5 - cy) * 1.25
                if lobed(dx, dy, r, 3, phase, 0.22) > 1.0:
                    continue
                light = -(dx * 0.7 + dy) / r
                k = base + 1 if light > 0.45 else base if light > -0.2 else base - 2
                c.put(x, y, shade_of(EARTH, k) if dirt else shade_of(IVORY, k))
    gx, gy = rng.randint(3, 9), rng.randint(6, 9)
    for i in range(4):
        c.put(gx + i, gy, GOLD[3])
        c.put(gx + i, gy + 1, GOLD[1])
    return INK


def debris(variant):
    """Grit and chips of stone scattered from a fall, with crumbs of earth: no outline, only a
    pixel of shadow under each bit, since it's lying flat."""
    c = Canvas(T, T)
    rng = random.Random(861 + variant)
    for i in range(14):
        x, y = rng.randint(0, 14), rng.randint(0, 14)
        if i % 3 == 0:
            c.put(x, y, EARTH[2])
            c.put(x + 1, y, EARTH[1])
        elif i % 3 == 1:
            c.put(x, y, IVORY[5])
            c.put(x + 1, y, IVORY[4])
            c.put(x, y + 1, IVORY[3])
        else:
            c.put(x, y, IVORY[4])
    shadowed = [(x, y) for y in range(T) for x in range(T) if c.img[y][x][3]]
    for x, y in shadowed:
        if x + 1 < T and y + 1 < T and not c.img[y + 1][x + 1][3]:
            c.img[y + 1][x + 1] = [0, 0, 0, 80]
    return c.img


def dart(c, x, y, dx, dy):
    """A spent dart lying on the floor: an iron shaft catching the light along its top, a red
    fletch at its tail, and a pixel of shadow under it."""
    for i in range(6):
        px, py = x + round(dx * i), y + round(dy * i)
        c.glaze(px + 1, py + 1, (0, 0, 0), 80)
    for i in range(6):
        px, py = x + round(dx * i), y + round(dy * i)
        c.put(px, py, IVORY[6] if i == 5 else IRON[3] if i > 1 else VELVET[3] if i == 0 else VELVET[2])


def darts(variant):
    """Darts the traps have already spat across the hall, lying where they fell."""
    c = Canvas(T, T)
    for x, y, dx, dy in (((2, 4, 1, 0.4), (8, 11, 1, -0.2)), ((3, 9, 1, -0.5), (9, 3, 0.6, 1), (5, 13, 1, 0)))[variant]:
        dart(c, x, y, dx, dy)
    return c.img


def paint_bones(c, variant):
    """What's left of a thief who trod on the wrong flagstone a long time ago: a skull, two long
    bones and a rib, and a rusted knife."""
    bone(c, 2, 12, 8, 12)
    bone(c, 10, 9, 13, 13)
    for i in range(5):
        c.put(9 + i, 3 + abs(i - 2) // 2, BONE[2])
        c.put(9 + i, 4 + abs(i - 2) // 2, BONE[1])
    skull(c.img, 3, 4)
    for i in range(4):
        c.put(5 + i, 9 - i // 2, (120, 70, 50) if i < 3 else (160, 100, 70))
    return INK


VAULT_TILES = (         # TilesetCatalog.Vault has the same list in the same order
    ("floor", floor_blocks(), None),
    ("floor-shadow", floor_blocks(in_shadow), None),
    ("floor-cracked", floor_blocks(cracked), None),
    ("inlay", [inlay(v) for v in range(3)], inlay_edges()),
    ("sun-crest", sun_crest(), None),
    ("pressure-plate", [pressure_plate(v) for v in range(2)], None),
    ("pit", [pit()], pit_edges()),
    ("earth", [earth(v) for v in range(3)], earth_edges()),
    ("tunnel", [earth(v) for v in range(3)], None),
    ("steps", [steps()], None),
    ("wall-top", [wall_top(v) for v in range(2)], None),
    ("wall-face", [wall_face(v) for v in range(3)], None),
    ("wall-crest", [wall_crest()], None),
    ("wall-niche", [wall_niche()], None),
    ("wall-base", [wall_base(v) for v in range(2)], None),
    ("earth-top", [earth_top(v) for v in range(2)], None),
    ("earth-face", [earth_face(v) for v in range(2)], None),
    ("coins", [coins(v) for v in range(3)], None),
    ("rubble", [detail(paint_rubble)(v) for v in range(2)], None),
    ("debris", [debris(v) for v in range(2)], None),
    ("darts", [darts(v) for v in range(2)], None),
    ("bones", [detail(paint_bones)(0)], None),
    ("cobweb", [cobweb(0)], None),
    ("cobweb-ne", [cobweb(1)], None),
)


# --------------------------------------------------------------------------- #
# The props: the doors, the lamps, the treasure, the traps, and the sappers' work
# --------------------------------------------------------------------------- #
# Drawn to match my props sheet: a dark outline round each, lit from the top left, the gold going
# from brown in its shadows to nearly white where it catches the light. The doors fill their
# rectangles, like the barrow's, since they're set into walls.

SCONCE_W, SCONCE_H = 10, 18


def vault_sconce():
    """A gilded lamp on the wall: a little sun for a backplate, an arm up from it, and a shallow
    bowl of oil at the top the game draws the flame coming out of, fourteen pixels up from where
    it's fixed, the same as the barrow's sconces."""
    c = Canvas(SCONCE_W, SCONCE_H)
    sun_at(c, 5.0, 13.0, 1.5, 4.4, 3.2, 8)
    for y in range(6, 11):
        c.put(4, y, GOLD[3])
        c.put(5, y, GOLD[1])
    for x in range(1, 9):
        c.put(x, 4, GOLD[3] if x < 4 else GOLD[2])
        c.put(x, 5, GOLD[1] if x < 7 else GOLD[0])
    for x in range(2, 8):
        c.put(x, 6, GOLD[0])
    for x in range(3, 7):
        c.put(x, 3, DARK)
    c.outline(INK)
    return c.img


BRAZIER_W, BRAZIER_H = 18, 24


def vault_brazier():
    """A brazier standing on three gold legs, its bowl full of coals glowing at the top. The game
    puts the flame on the coals, twenty pixels up from its foot."""
    c = Canvas(BRAZIER_W, BRAZIER_H)
    for x0, y0, x1, y1 in ((6, 9, 2, 22), (11, 9, 15, 22), (9, 9, 9, 22)):
        c.line(x0, y0, x1, y1, GOLD[2] if x1 <= x0 else GOLD[1])
        c.line(x0 + 1, y0, x1 + 1, y1, GOLD[1] if x1 <= x0 else GOLD[0])
    for x in (2, 15):
        c.put(x - 1, 22, GOLD[2])
        c.put(x + 2, 22, GOLD[1])
    c.line(4, 16, 13, 16, GOLD[1])
    for y in range(4, 10):
        half = 8 - (y - 4) * 0.9
        for x in range(int(round(8.5 - half)), int(round(8.5 + half)) + 1):
            k = 4 if y == 4 else 3 if x < 7 else 2 if x < 11 else 1
            if y == 9:
                k = 0
            c.put(x, y, GOLD[k])
    for x in range(2, 16):
        n = hash01(x, 0, 871)
        c.put(x, 3, EMBER[3] if n > 0.8 else EMBER[2] if n > 0.4 else EMBER[1])
        if 4 < x < 13:
            c.put(x, 2, EMBER[2] if n > 0.5 else EMBER[0])
    c.outline(INK)
    return c.img


DOOR_W, DOOR_H = 64, 48

#: The doorway: its sides, the middle and the radius of its arch, and how deep the ring of gilded
#: wedge stones round the arch is. The way through is the two tiles in the middle.
DOOR_SIDES = (16, 47)
DOOR_ARCH = (32.0, 30.0, 16.0)
VOUSSOIR_DEPTH = 3.4

#: The pilasters either side, and how tall the entablature over the doorway is, with the sun in it.
PILASTERS = ((1, 12), (51, 62))
ENTABLATURE = 12


def in_doorway(x, y):
    """Whether a pixel of a door's picture is in the doorway, under its arch."""
    cx, cy, r = DOOR_ARCH
    return DOOR_SIDES[0] <= x <= DOOR_SIDES[1] and y < DOOR_H - 2 and (y >= cy or math.hypot(x + 0.5 - cx, y + 0.5 - cy) < r)


def vault_portal(c):
    """The great doors' stone: a fluted pilaster either side with a gilded capital and base, an
    entablature over the top with the sun in its frieze, a ring of wedge stones round the arch with
    a gold keystone, ashlar in between, and a worn threshold."""
    cx, cy, r = DOOR_ARCH
    for y in range(DOOR_H):
        for x in range(DOOR_W):
            if in_doorway(x, y):
                continue
            dx, dy = x + 0.5 - cx, y + 0.5 - cy
            d = math.hypot(dx, dy)
            pilaster = next((p for p in PILASTERS if p[0] <= x <= p[1]), None)
            if y >= DOOR_H - 2:
                colour = IVORY[5] if y == DOOR_H - 2 else IVORY[2]
            elif y < ENTABLATURE:
                if y == 0:
                    colour = IVORY[6]
                elif y == 1:
                    colour = IVORY[4]
                elif y == 2:
                    colour = GOLD[4] if x % 6 == 0 else GOLD[3]
                elif y == 3:
                    colour = GOLD[1]
                elif y == ENTABLATURE - 1:
                    colour = IVORY[1]
                elif y == ENTABLATURE - 2:
                    colour = GOLD[2] if x % 3 else GOLD[0]
                else:
                    colour = IVORY[4] if hash01(x, y, 881) > 0.06 else IVORY[3]
            elif pilaster:
                left, right = pilaster
                if y < ENTABLATURE + 3:
                    colour = (GOLD[3], GOLD[2], GOLD[0])[y - ENTABLATURE]
                elif y >= DOOR_H - 7:
                    colour = GOLD[2] if y == DOOR_H - 7 else IVORY[4] if x == left else IVORY[2] if x == right else IVORY[3]
                elif x in (left, right):
                    colour = IVORY[5] if x == left else IVORY[1]
                else:
                    flute = (x - left) % 3
                    colour = IVORY[5] if flute == 1 else IVORY[2] if flute == 0 else IVORY[4]
            elif dy < 0.5 and d < r + VOUSSOIR_DEPTH:
                angle = math.degrees(math.atan2(-dy, dx))
                joint = abs((angle + 11.25) % 22.5 - 11.25) < 2.0 * 14.0 / d
                keystone = abs(angle - 90) < 9
                if keystone:
                    colour = GOLD[4] if d < r + 1 else GOLD[3] if dx < 0 else GOLD[2]
                elif joint:
                    colour = GROUT
                else:
                    colour = IVORY[6] if d < r + 1 else IVORY[5] if dx < 0 else IVORY[4]
            else:
                course = (y - ENTABLATURE) // 6
                joint = (y - ENTABLATURE) % 6 == 0 or x == (14 if course % 2 else 49)
                colour = GROUT if joint else IVORY[4] if (y - ENTABLATURE) % 6 == 1 else IVORY[3]
            c.put(x, y, colour)
    sun_at(c, 32.0, 7.2, 2.2, 5.6, 4.2, 12)


def steel_leaves(c):
    """The two leaves shut: blued steel in raised panels, bound across with gold bars studded
    with rivets, and the sun across the seam where they meet, half on each."""
    for y in range(DOOR_H - 2):
        for x in range(DOOR_SIDES[0], DOOR_SIDES[1] + 1):
            if not in_doorway(x, y):
                continue
            u = x - (DOOR_SIDES[0] if x < 32 else 32)
            if x in (31, 32):
                colour = INK if x == 31 else STEEL[0]
            elif u in (0, 1) or x == DOOR_SIDES[1]:
                colour = GOLD[3] if u == 0 else GOLD[1]
            else:
                panel = 3 <= u <= 12 and (19 <= y <= 30 or 34 <= y <= 43)
                edge_top = panel and (y in (19, 34) or u == 3)
                edge_bottom = panel and (y in (30, 43) or u == 12)
                colour = STEEL[3] if edge_top else STEEL[0] if edge_bottom else STEEL[2] if panel else STEEL[1]
                if y < 18:
                    colour = lerp_color(colour, STEEL[0], 0.3)
            c.put(x, y, colour)
    for bar in (25, 39):
        for x in range(DOOR_SIDES[0], DOOR_SIDES[1] + 1):
            c.put(x, bar, GOLD[3])
            c.put(x, bar + 1, GOLD[1])
        for x in range(DOOR_SIDES[0] + 2, DOOR_SIDES[1], 4):
            c.put(x, bar, GOLD[4])
    sun_at(c, 32.0, 32.0, 3.0, 9.0, 6.4, 12)


def vault_door():
    """The great doors of the vault, shut and open: two leaves of blued steel and gold under a round
    arch in a gilded portal. Open, the leaves have swung back in against the jambs and there's only
    the dark beyond."""
    frames = []
    for f in range(2):
        c = Canvas(DOOR_W, DOOR_H)
        vault_portal(c)
        if f == 0:
            steel_leaves(c)
        else:
            for y in range(DOOR_H - 2):
                for x in range(DOOR_SIDES[0], DOOR_SIDES[1] + 1):
                    if not in_doorway(x, y):
                        continue
                    if x <= DOOR_SIDES[0] + 2 or x >= DOOR_SIDES[1] - 2:
                        edge = x in (DOOR_SIDES[0] + 2, DOOR_SIDES[1] - 2)
                        colour = GOLD[3] if edge else STEEL[2] if x < 32 else STEEL[1]
                    else:
                        colour = lerp_color(DARK, IVORY[1], max(0.0, (y - 30) / 22.0))
                    c.put(x, y, colour)
        frames.append(c.img)
    return side_by_side(frames)


LEVER_W, LEVER_H = 12, 20

#: The pin the lever turns on, and where the end of its handle is, up and then pulled down.
LEVER_PIN = (5, 12)
LEVER_ENDS = ((3, 2), (8, 18))


def vault_lever():
    """A pale stone plate on the wall with a gold rim and a slot down it, and a gilded lever on a
    pin through it with a lapis knob on its end: up, then pulled down."""
    frames = []
    for f in range(2):
        c = Canvas(LEVER_W, LEVER_H)
        for y in range(6, 19):
            for x in range(1, 11):
                if y in (6, 18) and x in (1, 10):
                    continue
                rim = y in (6, 18) or x in (1, 10)
                colour = (GOLD[3] if y == 6 or x == 1 else GOLD[1]) if rim else IVORY[5] if y == 7 else IVORY[4]
                c.put(x, y, colour)
        for y in range(8, 17):
            c.put(5, y, DARK)
            c.put(6, y, DARK)
            c.put(7, y, IVORY[6])
        (px, py), (ex, ey) = LEVER_PIN, LEVER_ENDS[f]
        length = int(max(abs(ex - px), abs(ey - py)))
        for i in range(length + 1):
            t = i / length
            x, y = round(px + (ex - px) * t), round(py + (ey - py) * t)
            c.put(x, y, GOLD[4])
            c.put(x + 1, y, GOLD[2])
            c.put(x + 2, y, GOLD[0])
        for dx, dy, k in ((0, -1, 3), (-1, 0, 3), (0, 0, 2), (1, 0, 1), (0, 1, 1), (1, 1, 0), (1, -1, 2), (2, 0, 1)):
            c.put(ex + dx, ey + dy, LAPIS[k + 1])
        for dx, dy, k in ((0, 0, 3), (1, 0, 2), (0, 1, 1), (1, 1, 0)):
            c.put(px + dx, py + dy, GOLD[k])
        c.outline(INK)
        frames.append(c.img)
    return side_by_side(frames)


CHEST_W, CHEST_H = 22, 22

#: Where the lid stops and the front of the chest starts.
CHEST_LID = 10


def chest_body(c):
    """The front of a royal strongbox: dark wood between gold bands, gold corners, and a gold lock
    plate with a keyhole."""
    for y in range(CHEST_LID, CHEST_H - 1):
        for x in range(1, CHEST_W - 1):
            band = x in (2, 3, CHEST_W - 4, CHEST_W - 3) or y in (CHEST_LID, CHEST_H - 3)
            if band:
                colour = GOLD[3] if x in (2, CHEST_W - 4) or y == CHEST_LID else GOLD[1]
            else:
                colour = WOOD[2] if x < CHEST_W // 2 else WOOD[1]
                if (x + y) % 5 == 0:
                    colour = WOOD[1]
            if y == CHEST_H - 2:
                colour = WOOD[0]
            c.put(x, y, colour)
    for y in range(CHEST_LID + 2, CHEST_LID + 7):
        for x in range(8, 14):
            c.put(x, y, GOLD[3] if x == 8 or y == CHEST_LID + 2 else GOLD[1] if x == 13 or y == CHEST_LID + 6 else GOLD[2])
    c.put(10, CHEST_LID + 4, DARK)
    c.put(11, CHEST_LID + 4, DARK)
    c.put(10, CHEST_LID + 5, DARK)


def vault_chest():
    """A royal strongbox, shut and open. Shut, its rounded lid is bound in gold like its front. Open,
    the lid stands up behind it lined with red velvet, and it's full to the brim with gold and a
    couple of gems."""
    frames = []
    for f in range(2):
        c = Canvas(CHEST_W, CHEST_H)
        if f == 0:
            for y in range(2, CHEST_LID):
                inset = 2 if y == 2 else 1 if y == 3 else 0
                for x in range(1 + inset, CHEST_W - 1 - inset):
                    strap = x in (2, 3, CHEST_W - 4, CHEST_W - 3)
                    if strap:
                        colour = GOLD[3] if y < 6 else GOLD[2]
                    else:
                        k = 4 if y < 4 else 3 if y < 6 else 2 if y < CHEST_LID - 1 else 1
                        colour = WOOD[k]
                    c.put(x, y, colour)
            chest_body(c)
        else:
            for y in range(0, CHEST_LID - 1):
                for x in range(2, CHEST_W - 2):
                    rim = x in (2, CHEST_W - 3) or y == 0
                    colour = GOLD[2] if rim else VELVET[2] if y < 4 else VELVET[1]
                    c.put(x, y, colour)
            chest_body(c)
            for y in range(CHEST_LID - 2, CHEST_LID + 2):
                for x in range(2, CHEST_W - 2):
                    k = 4 if (x + y * 2) % 5 == 0 else 3 if (x + y) % 2 else 2
                    if y == CHEST_LID + 1:
                        k -= 1
                    c.put(x, y, GOLD[max(0, k)])
            for gx, gy, gem in ((6, CHEST_LID - 2, 1), (15, CHEST_LID - 1, 5)):
                c.put(gx, gy, GEMS[gem])
                c.put(gx + 1, gy, GEMS[gem - 1])
        c.outline(INK)
        frames.append(c.img)
    return side_by_side(frames)


def coin_heap(c, cx, cy, rx, ry, seed):
    """A heap of coins, round, lit on its top left: coins edge to edge in rows, each a pixel of
    light and its shadow, brighter toward the light, with glints here and there."""
    for y in range(int(cy - ry) - 1, int(cy + ry) + 2):
        for x in range(int(cx - rx) - 1, int(cx + rx) + 2):
            dx, dy = (x + 0.5 - cx) / rx, (y + 0.5 - cy) / ry
            d = dx * dx + dy * dy
            if d > 1.0:
                continue
            light = -(dx * 0.6 + dy) * 0.9
            k = 3 if light > 0.35 else 2 if light > -0.35 else 1
            ox = (x + (y // 2) * 2) % 4
            if y % 2 == 0 and ox == 0:
                k += 1
            elif y % 2 == 1 and ox in (2, 3):
                k -= 1
            if hash01(x, y, seed) > 0.96:
                k = 4
            if d > 0.86 and light < 0:
                k = min(k, 1)
            c.put(x, y, GOLD[max(0, min(4, k))])


def sparkle(c, x, y):
    """A glint on gold: a white point with four short rays."""
    c.put(x, y, (255, 255, 240))
    for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
        c.put(x + dx, y + dy, GOLD[4])


GOLD_PILE_W, GOLD_PILE_H = 42, 26


def gold_pile():
    """A heap of the crown's gold: a hill of coins with a goblet and a sword hilt sticking out of
    it, a red stone, a green and a blue, coins slid off round its foot, and the light catching it."""
    c = Canvas(GOLD_PILE_W, GOLD_PILE_H)
    coin_heap(c, 21, 18, 19, 7.5, 891)
    coin_heap(c, 18, 13, 11, 7, 892)
    coin_heap(c, 27, 15, 8, 5, 893)
    for y in range(1, 9):
        c.put(9 + y // 3, y, IRON[3] if y > 2 else GOLD[3])
        c.put(10 + y // 3, y, IRON[1] if y > 2 else GOLD[1])
    for x in range(7, 14):
        c.put(x, 3, GOLD[3] if x < 11 else GOLD[1])
    c.put(9, 0, GEMS[1])
    for y, (x0, x1) in zip(range(5, 13), ((27, 33), (28, 32), (28, 32), (29, 31), (30, 30), (30, 30), (29, 31), (28, 32))):
        for x in range(x0, x1 + 1):
            c.put(x, y, GOLD[4] if x == x0 else GOLD[3] if x < x1 else GOLD[1])
    for gx, gy, gem in ((14, 14, 1), (24, 20, 5), (33, 18, 3)):
        c.put(gx, gy, GEMS[gem])
        c.put(gx + 1, gy, GEMS[gem - 1])
        c.put(gx, gy + 1, GEMS[gem - 1])
    for x, y in ((2, 23), (6, 24), (35, 24), (39, 22), (12, 25), (30, 25)):
        coin(c, x, y, x % 2 == 0)
    c.outline(INK)
    for x, y in ((15, 8), (25, 12), (8, 17)):
        sparkle(c, x, y)
    return c.img


GOLD_SMALL_W, GOLD_SMALL_H = 26, 15


def gold_pile_small():
    """A smaller heap of coins, with a few slid off it."""
    c = Canvas(GOLD_SMALL_W, GOLD_SMALL_H)
    coin_heap(c, 13, 9, 11, 4.6, 894)
    coin_heap(c, 11, 7, 6, 4, 895)
    for x, y in ((1, 12), (21, 12)):
        coin(c, x, y, True)
    c.put(15, 9, GEMS[1])
    c.put(16, 9, GEMS[0])
    c.outline(INK)
    sparkle(c, 9, 4)
    return c.img


SHELF_W, SHELF_H = 48, 52

#: Where the tops of the three shelves are, from the top of the picture. Kael stands on the
#: lowest, twelve pixels up from the foot, so PropCatalog sorts the shelf sixteen up.
SHELF_BOARDS = (16, 28, 40)
SHELF_SIDES = (1, 46)


def relic_shelf():
    """A tall cabinet of dark wood with red velvet at its back and a gilded crest on top, its three
    shelves crowded with the old kings' things: a crown, a goblet, gold bars and an urn on the top;
    scrolls, a casket, a sceptre and a stone on a cushion in the middle; a helm, books, a lapis vase
    and a heap of coins at the bottom."""
    c = Canvas(SHELF_W, SHELF_H)
    left, right = SHELF_SIDES
    for y in range(4, SHELF_H - 1):
        for x in range(left, right + 1):
            if x in (left, left + 1, right - 1, right):
                colour = WOOD[3] if x == left else WOOD[2] if x == left + 1 else WOOD[1] if x == right - 1 else WOOD[0]
                if x == left + 1 and y % 12 == 6:
                    colour = GOLD[2]
            elif y >= SHELF_H - 7:
                colour = WOOD[3] if y == SHELF_H - 7 else GOLD[2] if y == SHELF_H - 6 else WOOD[2] if y < SHELF_H - 2 else WOOD[0]
            else:
                colour = VELVET[0] if (x + y) % 7 else lerp_color(VELVET[0], VELVET[1], 0.5)
            c.put(x, y, colour)
    for y in range(0, 5):
        half = 22 if y >= 3 else 20 if y == 2 else 8
        for x in range(24 - half, 24 + half):
            colour = WOOD[3] if y == 3 else WOOD[2] if y == 4 else GOLD[2]
            c.put(x, y, colour)
    sun_at(c, 24.0, 2.5, 1.4, 3.6, 2.8, 8)
    for board in SHELF_BOARDS:
        for x in range(left + 2, right - 1):
            c.put(x, board, WOOD[4])
            c.put(x, board + 1, WOOD[3])
            c.put(x, board + 2, WOOD[1])
            c.put(x, board - 1, lerp_color(VELVET[0], (0, 0, 0), 0.3))

    # The top shelf: a crown, a goblet, gold bars, an urn.
    top = SHELF_BOARDS[0]
    for x in range(5, 14):
        for y in range(top - 5, top):
            if y >= top - 3 or x in (5, 9, 13):
                c.put(x, y, GOLD[3] if x < 9 else GOLD[2] if y < top - 1 else GOLD[1])
    c.put(9, top - 6, GEMS[1])
    c.put(7, top - 2, GEMS[5])
    c.put(11, top - 2, GEMS[3])
    for y, (x0, x1) in zip(range(top - 8, top), ((17, 21), (17, 21), (18, 20), (19, 19), (19, 19), (19, 19), (18, 20), (17, 21))):
        for x in range(x0, x1 + 1):
            c.put(x, y, GOLD[3] if x == x0 else GOLD[2] if x < x1 else GOLD[1])
    c.put(19, top - 7, GEMS[1])
    for i, (bx, by) in enumerate(((25, top - 3), (30, top - 3), (27, top - 5))):
        for x in range(bx, bx + 5):
            c.put(x, by, GOLD[4] if i == 2 else GOLD[3])
            c.put(x, by + 1, GOLD[2])
            c.put(x, by + 2, GOLD[1])
    for y in range(top - 9, top):
        w = (1, 2, 2, 3, 3, 3, 3, 2, 2)[y - (top - 9)]
        for x in range(39 - w, 40 + w):
            c.put(x, y, GOLD[3] if y == top - 5 else LAPIS[3] if x < 39 else LAPIS[2] if x < 40 + w - 1 else LAPIS[1])

    # The middle shelf: scrolls, a casket, a sceptre, a stone on a cushion.
    mid = SHELF_BOARDS[1]
    for i, sx in enumerate((4, 7, 10)):
        for y in range(mid - 3 - (i % 2), mid):
            c.put(sx, y, PARCHMENT[2])
            c.put(sx + 1, y, PARCHMENT[1])
            c.put(sx + 2, y, PARCHMENT[0])
        c.put(sx + 1, mid - 2, VELVET[3])
    for y in range(mid - 5, mid):
        for x in range(15, 23):
            colour = GOLD[3] if y == mid - 5 or x == 15 else GOLD[1] if x == 22 else WOOD[2] if y > mid - 4 else WOOD[3]
            c.put(x, y, colour)
    c.put(18, mid - 3, GOLD[3])
    c.put(19, mid - 3, GOLD[2])
    for i in range(9):
        c.put(26 + i // 3, mid - 1 - i, GOLD[3] if i % 2 else GOLD[2])
    for dx, dy in ((0, 0), (1, 0), (0, -1), (1, -1), (-1, -1), (0, -2)):
        c.put(28 + dx, mid - 10 + dy, GEMS[5] if dy < -1 or dx < 0 else GEMS[4])
    for x in range(34, 43):
        c.put(x, mid - 1, VELVET[1])
        c.put(x, mid - 2, VELVET[3] if x < 38 else VELVET[2])
    for dx, dy, k in ((0, 0, 1), (1, 0, 0), (0, -1, 1), (1, -1, 1), (-1, -1, 1), (0, -2, 1), (2, -1, 0)):
        c.put(38 + dx, mid - 3 + dy, GEMS[k])
    c.put(37, mid - 4, (255, 210, 220))

    # The bottom shelf: a helm, books, a lapis vase, coins.
    low = SHELF_BOARDS[2]
    for y in range(low - 7, low):
        for x in range(4, 12):
            dx, dy = x + 0.5 - 8, y + 0.5 - (low - 3)
            if dx * dx / 16 + dy * dy / 16 > 1 and y < low - 3:
                continue
            c.put(x, y, IVORY[6] if dx + dy < -2 else IVORY[4] if dx + dy < 1.5 else IVORY[2])
    for x in range(5, 11):
        c.put(x, low - 3, DARK if 6 <= x <= 9 else IVORY[2])
    c.put(8, low - 8, VELVET[3])
    c.put(8, low - 9, VELVET[2])
    for i, colour in enumerate((LAPIS[2], VELVET[2], (60, 100, 70), LAPIS[1], VELVET[1])):
        for y in range(low - 6 + (i % 2), low):
            c.put(15 + i * 2, y, colour)
            c.put(16 + i * 2, y, lerp_color(colour, (0, 0, 0), 0.35))
        c.put(15 + i * 2, low - 4, GOLD[3])
    for y in range(low - 8, low):
        w = (1, 1, 2, 3, 3, 3, 2, 1)[y - (low - 8)]
        for x in range(30 - w, 31 + w):
            c.put(x, y, LAPIS[4] if x < 30 else LAPIS[2] if x < 31 + w - 1 else LAPIS[1])
    coin_heap(c, 39, low - 2, 5, 2.6, 896)
    c.outline(INK)
    sparkle(c, 6, top - 4)
    return c.img


DART_TRAP_W, DART_TRAP_H = 16, 16


def dart_trap():
    """A dart trap in the wall, dressed as the sun: a boss of pale stone carved into a face, lit on
    its top left, with short gold rays round it, eyes cut into it, and a slot of a mouth with the
    iron points of three darts just showing in it."""
    c = Canvas(DART_TRAP_W, DART_TRAP_H)
    cx, cy = 8.0, 8.0
    for y in range(DART_TRAP_H):
        for x in range(DART_TRAP_W):
            dx, dy = x + 0.5 - cx, y + 0.5 - cy
            d = math.hypot(dx, dy)
            a = math.atan2(dy, dx)
            if d < 4.8:
                light = -(dx + dy) / 4.8
                c.put(x, y, IVORY[6] if light > 0.7 else IVORY[5] if light > 0 else IVORY[4] if light > -0.6 else IVORY[3])
            elif d < 5.8:
                c.put(x, y, GOLD[3] if dx + dy < 0 else GOLD[1])
            elif d < 7.6 and abs((a / (2 * math.pi) * 8) % 1 - 0.5) > 0.3:
                c.put(x, y, GOLD[2] if dx + dy < 0 else GOLD[1])
    for x, y in ((6, 6), (10, 6)):
        c.put(x, y, IVORY[1])
        c.put(x + 1, y + 1, IVORY[6])
    for x in range(6, 11):
        c.put(x, 10, DARK)
        c.put(x, 11, IVORY[6] if x > 6 else IVORY[5])
    for x in (6, 8, 10):
        c.put(x, 10, IRON[3])
    c.outline(INK)
    return c.img


PEDESTAL_W, PEDESTAL_H = 28, 46


def sunheart_pedestal():
    """Where the Sunheart sat: a fluted column of pale stone on two gilded steps, a gold capital,
    and a gold cradle on top with five prongs curling up round an empty socket."""
    c = Canvas(PEDESTAL_W, PEDESTAL_H)
    for y in range(PEDESTAL_H - 8, PEDESTAL_H - 1):
        lower = y >= PEDESTAL_H - 4
        x0, x1 = (1, PEDESTAL_W - 2) if lower else (4, PEDESTAL_W - 5)
        for x in range(x0, x1 + 1):
            top = y in (PEDESTAL_H - 8, PEDESTAL_H - 4)
            colour = GOLD[3] if top and x < 14 else GOLD[2] if top else IVORY[5] if x < 12 else IVORY[3] if x < x1 else IVORY[2]
            c.put(x, y, colour)
    for y in range(17, PEDESTAL_H - 8):
        for x in range(8, 20):
            flute = (x - 8) % 3
            colour = IVORY[6] if flute == 1 and x < 14 else IVORY[4] if flute == 1 else IVORY[2] if flute == 0 else IVORY[5] if x < 14 else IVORY[3]
            if y in (PEDESTAL_H - 11, 20):
                colour = GOLD[3] if x < 14 else GOLD[1]
            c.put(x, y, colour)
    for y in range(12, 17):
        half = 9 if y < 15 else 7
        for x in range(14 - half, 14 + half):
            colour = GOLD[4] if y == 12 else GOLD[3] if x < 13 else GOLD[2] if y < 15 else GOLD[1]
            c.put(x, y, colour)
    for y in range(7, 12):
        half = (6, 6, 5, 5, 4)[y - 7]
        for x in range(14 - half, 14 + half):
            colour = GOLD[3] if x < 12 else GOLD[2] if x < 14 + half - 1 else GOLD[1]
            c.put(x, y, colour)
    for x in range(10, 18):
        c.put(x, 7, DARK if 11 <= x <= 16 else GOLD[1])
        c.put(x, 8, LAPIS[0] if 11 <= x <= 16 else GOLD[2])
    for x0, lean in ((8, -1), (11, -0.4), (14, 0), (17, 0.4), (20, 1)):
        for i in range(6):
            x = round(x0 + lean * i * 0.5 + lean * i * i * 0.08)
            c.put(x, 7 - i, GOLD[4] if i == 5 else GOLD[3] if lean <= 0 else GOLD[2])
    c.outline(INK)
    return c.img


MOSAIC_W, MOSAIC_H = 64, 54


def sun_mosaic():
    """The sun of the old kings, big, inlaid in the floor where the Sunheart was kept: gold rays
    on a lapis field in a gold ring, a ring of pale marble round that with lapis studs in it, and a
    joint round the edge where it's set into the floor. It lies flat, so it has no outline."""
    c = Canvas(MOSAIC_W, MOSAIC_H)
    cx, cy = MOSAIC_W / 2, MOSAIC_H / 2
    rx, ry = MOSAIC_W / 2 - 0.5, MOSAIC_H / 2 - 0.5
    squash = ry / rx
    for y in range(MOSAIC_H):
        for x in range(MOSAIC_W):
            dx, dy = (x + 0.5 - cx), (y + 0.5 - cy) / squash
            d = math.hypot(dx, dy)
            if d > rx:
                continue
            if d > rx - 1:
                colour = FLOOR_JOINT
            elif d > rx - 3:
                colour = GOLD[3] if dx + dy < 0 else GOLD[1]
            elif d > rx - 8:
                colour = IVORY[5] if hash01(x, y, 901) > 0.06 else IVORY[4]
                a = math.atan2(dy, dx)
                if abs((a / (2 * math.pi) * 16) % 1 - 0.5) < 0.09 and abs(d - (rx - 5.5)) < 1.2:
                    colour = LAPIS[3]
            elif d > rx - 10:
                colour = GOLD[2] if dx + dy < 0 else GOLD[0]
            else:
                colour = LAPIS[1] if int(d) % 4 else LAPIS[2]
            c.put(x, y, colour)
    round_sun = Canvas(MOSAIC_W, MOSAIC_W)
    sun_at(round_sun, cx, cx, 5.0, rx - 11, rx - 17, 16)
    for y in range(MOSAIC_W):
        for x in range(MOSAIC_W):
            if round_sun.img[y][x][3]:
                sy = int(round((y + 0.5 - cx) * squash + cy - 0.5))
                if 0 <= sy < MOSAIC_H:
                    c.put(x, sy, round_sun.img[y][x][:3])
    return c.img


RUBBLE_W, RUBBLE_H = 50, 30


def rubble_heap():
    """Where a tunnel's roof came down into the vault: a heap of broken blocks, pale on top and
    lit along their tops, a length of the gilded cornice still on one, and black earth spilled
    out from under it all."""
    c = Canvas(RUBBLE_W, RUBBLE_H)
    rng = random.Random(911)
    for _ in range(9):
        cx, cy, r = rng.uniform(6, 44), rng.uniform(19, 26), rng.uniform(3.0, 5.0)
        phase = rng.uniform(0, 6.28)
        for y in range(RUBBLE_H):
            for x in range(RUBBLE_W):
                dx, dy = x + 0.5 - cx, (y + 0.5 - cy) * 1.4
                if lobed(dx, dy, r, 4, phase, 0.25) > 1.0:
                    continue
                light = -(dx * 0.7 + dy) / r
                c.put(x, y, EARTH[4 if light > 0.4 else 3 if light > -0.2 else 1])
    blocks = ((5, 14, 13, 9), (17, 10, 14, 10), (31, 13, 12, 9), (11, 5, 11, 8), (24, 3, 10, 8), (36, 6, 9, 7))
    for i, (bx, by, bw, bh) in enumerate(blocks):
        slant = (i % 3) - 1
        top = max(2, bh // 3)
        for y in range(bh):
            for x in range(bw):
                sx = bx + x + (slant * y) // 4
                if y < top:
                    k = 6 if y == 0 else 5
                elif x == bw - 1:
                    k = 1
                else:
                    k = 4 if x < bw // 2 else 3
                if hash01(sx, by + y, 912) < 0.07:
                    k -= 1
                c.put(sx, by + y, IVORY[k])
        if i == 1:
            for x in range(bw):
                c.put(bx + x, by + top, GOLD[3])
                c.put(bx + x, by + top + 1, GOLD[1])
    for x, y in ((2, 27), (8, 28), (45, 27), (40, 28), (29, 28)):
        c.put(x, y, IVORY[4])
        c.put(x + 1, y, IVORY[2])
    c.outline(INK)
    return c.img


HOLE_W, HOLE_H = 46, 30


def sapper_hole():
    """Where the Hollowed came up through the floor: a ragged black hole, its far side a wall of
    raw earth going down into the dark, slabs heaved up and broken round it with their edges lit,
    churned earth between them, and a sapper's broken pick left on the rim. It lies on the floor,
    under everyone standing round it."""
    c = Canvas(HOLE_W, HOLE_H)
    cx, cy = HOLE_W / 2, 16.0
    for y in range(HOLE_H):
        for x in range(HOLE_W):
            dx, dy = (x + 0.5 - cx) / 20.0, (y + 0.5 - cy) / 12.5
            ragged = 0.08 * math.sin(math.atan2(dy, dx) * 7 + 1.3)
            if dx * dx + dy * dy > 1.0 + ragged:
                continue
            n = hash01(x, y, 921)
            c.put(x, y, EARTH[2] if n > 0.55 else EARTH[1] if n > 0.15 else EARTH[3])
    for y in range(HOLE_H):
        for x in range(HOLE_W):
            dx, dy = (x + 0.5 - cx) / 12.5, (y + 0.5 - cy - 1) / 7.5
            d = dx * dx + dy * dy
            if d > 1.0 + 0.1 * math.sin(math.atan2(dy, dx) * 5):
                continue
            if dy < 0 and d > 0.35:
                colour = lerp_color(EARTH[3], DARK, min(1.0, 0.35 + (1.0 + dy) * 0.9))
                if hash01(x, y, 922) < 0.12:
                    colour = EARTH[1]
            else:
                colour = DARK
            c.put(x, y, colour)
    slabs = ((4, 9, 9, 5, -1), (12, 2, 10, 4, 0), (26, 2, 10, 4, 1), (35, 8, 8, 5, 1), (6, 21, 10, 4, 1), (29, 22, 11, 4, -1))
    for sx, sy, sw, sh, tilt in slabs:
        for y in range(sh + 2):
            for x in range(sw):
                px, py = sx + x, sy + y + (tilt * x) // 5
                if y < sh:
                    k = 6 if y == 0 else 5 if y < 2 else 4
                    c.put(px, py, IVORY[3 if x == sw - 1 else k])
                else:
                    c.put(px, py, IVORY[2] if y == sh else EARTH[0])
    for i in range(10):
        c.put(31 + i, 19 - i // 3, WOOD[3] if i < 8 else IRON[3])
        c.put(31 + i, 20 - i // 3, WOOD[1] if i < 8 else IRON[1])
    for dy in range(-2, 3):
        c.put(40, 16 + dy, IRON[3] if dy < 0 else IRON[2])
    c.outline(INK)
    return c.img


CASE_W, CASE_H = 20, 34

#: Where the top of a relic case's plinth is, and the glass over what's on it.
CASE_TOP = 20
CASE_GLASS = (3, 2, 16, 19)
GLASS = (196, 222, 246)


def crown_on(c, cx, base):
    """A gold crown sitting on its cushion: a band with a stone set in it, and five points with a
    pearl on each, lit on its left."""
    for x in range(cx - 4, cx + 5):
        for y in range(base - 3, base):
            c.put(x, y, GOLD[3] if x < cx else GOLD[2] if x < cx + 3 else GOLD[1])
    for i, x in enumerate(range(cx - 4, cx + 5, 2)):
        height = 3 if i in (0, 4) else 4 if i in (1, 3) else 5
        for y in range(base - 3 - height + 1, base - 3):
            c.put(x, y, GOLD[3] if x <= cx else GOLD[2])
        c.put(x, base - 3 - height, (246, 240, 230))
    c.put(cx, base - 2, GEMS[1])
    c.put(cx - 2, base - 2, GEMS[5])
    c.put(cx + 2, base - 2, GEMS[3])


def orb_on(c, cx, base):
    """A gold orb sitting on its cushion: round, lit on its top left, a band of gems round it and a
    little cross on top."""
    cy = base - 4
    for y in range(cy - 4, cy + 4):
        for x in range(cx - 4, cx + 4):
            dx, dy = x + 0.5 - cx, y + 0.5 - cy
            if dx * dx + dy * dy > 15:
                continue
            light = -(dx + dy)
            c.put(x, y, GOLD[4] if light > 3.5 else GOLD[3] if light > 0 else GOLD[2] if light > -3 else GOLD[1])
    for x in range(cx - 4, cx + 4):
        c.put(x, cy, GOLD[1])
    c.put(cx - 2, cy, GEMS[1])
    c.put(cx + 1, cy, GEMS[5])
    for y in range(cy - 7, cy - 4):
        c.put(cx - 1, y, GOLD[3])
    c.put(cx - 2, cy - 6, GOLD[3])
    c.put(cx, cy - 6, GOLD[2])


def relic_case(relic):
    """One of the old kings' things on a plinth of pale stone with a gold band round its top, on a
    red cushion, under a dome of glass with the light running down its left side."""
    c = Canvas(CASE_W, CASE_H)
    for y in range(CASE_TOP, CASE_H - 1):
        foot = y >= CASE_H - 4
        x0, x1 = (1, CASE_W - 2) if foot else (3, CASE_W - 4)
        for x in range(x0, x1 + 1):
            if y in (CASE_TOP, CASE_H - 4):
                colour = GOLD[3] if x < CASE_W // 2 else GOLD[2]
            elif y == CASE_TOP + 1:
                colour = GOLD[1]
            else:
                colour = IVORY[5] if x < x0 + 3 else IVORY[2] if x == x1 else IVORY[4] if x < CASE_W // 2 else IVORY[3]
            c.put(x, y, colour)
    for x in range(4, CASE_W - 4):
        c.put(x, CASE_TOP - 1, VELVET[1])
        c.put(x, CASE_TOP - 2, VELVET[3] if x < 9 else VELVET[2])
    relic(c, CASE_W // 2, CASE_TOP - 2)
    c.outline(INK)
    x0, y0, x1, y1 = CASE_GLASS
    for y in range(y0, y1 + 1):
        for x in range(x0, x1 + 1):
            top = y0 + 3
            if y < top:
                dx = (x + 0.5 - (x0 + x1 + 1) / 2) / ((x1 - x0 + 1) / 2)
                if dx * dx + ((top - y) / 3.5) ** 2 > 1.0:
                    continue
            rim = x in (x0, x1) or y == y0 or y < top and not c.filled(x, y - 1) and c.img[y - 1][x][3] == 0
            if x == x0 + 1 and y0 + 2 < y < y1 - 2:
                c.glaze(x, y, (255, 255, 255), 200)
            elif rim:
                c.glaze(x, y, GLASS, 190)
            else:
                c.glaze(x, y, GLASS, 46)
    return c.img


def vault_drawn():
    """The vault's props, by name."""
    return {
        "vault-sconce": vault_sconce(),
        "vault-brazier": vault_brazier(),
        "vault-door": vault_door(),
        "vault-lever": vault_lever(),
        "vault-chest": vault_chest(),
        "gold-pile": gold_pile(),
        "gold-pile-small": gold_pile_small(),
        "relic-shelf": relic_shelf(),
        "dart-trap": dart_trap(),
        "sunheart-pedestal": sunheart_pedestal(),
        "sun-mosaic": sun_mosaic(),
        "rubble-heap": rubble_heap(),
        "sapper-hole": sapper_hole(),
        "relic-case": relic_case(crown_on),
        "relic-case-orb": relic_case(orb_on),
    }


if __name__ == "__main__":
    print("Generating the Old Crown Vault for Legend Quest...")
    build_sheet(VAULT_TILES, os.path.join(OUT, "Tiles", "vault.png"))
    for name, picture in vault_drawn().items():
        write_png(os.path.join(OUT, "Props", name + ".png"), picture)
    print("Done.")
