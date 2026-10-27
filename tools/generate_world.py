"""
World art for Legend Quest: the tilesets, the props, the gate behind the title, the torch flame, and
the two soft textures light and embers are drawn with.

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
from pixels import BAYER, OUT, clamp_byte, hash01, lerp_color, new_image, value_noise, write_png  # noqa: E402


# --------------------------------------------------------------------------- #
# glow.png and mote.png -- the soft white shapes light and embers are drawn with
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
    """A small soft dot for one ember."""
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
# title-scene.png -- the gate at night, lit by two torches
# --------------------------------------------------------------------------- #
# 400x225 art pixels, drawn at 4x behind the title. A crenellated wall against the night, an
# arched gate with a barred door, flagstones in front, and a torch in an iron sconce either side.
# The light is worked out in 3D (the wall stands up, the floor lies down, the flames stand out
# from the wall), then cut into dithered bands so it reads as pixel art and not a gradient. The
# flames themselves aren't in the picture. The game draws them over it and makes them flicker.

TITLE_W, TITLE_H = 400, 225

#: The bottom middle of each flame, in art pixels. Has to match TorchesAt in Screens/TitleBackgroundScreen.cs.
TITLE_TORCHES = ((118, 99), (282, 99))

GROUND = 172            # where the wall meets the flagstones
MERLON_TOP = 12         # top of the battlements
PARAPET_TOP = 26        # top of the coping the battlements stand on
WALL_TOP = 32           # where the coping ends and the wall below starts
MERLON_W, CRENEL_W = 16, 10

GATE_X = 200            # middle of the gate
SPRING = 112            # where the arch starts curving
ARCH_IN, ARCH_OUT = 44, 56
VOUSSOIRS = 11          # odd, so there's a keystone in the middle

#: The flagstones are laid out flat and then put in perspective: HORIZON is the height the
#: joints run back toward, and a stone is FLAG_W across and FLAG_D deep in that flat layout.
HORIZON = 40
FLAG_W = 26.0
FLAG_D = 0.055
STEP_X0, STEP_X1, STEP_H = 136, 264, 5

# Albedo, which is the colour each thing would be under a full torch.
STONE = (120, 113, 106)
MORTAR = (50, 46, 44)
FLOOR_STONE = (106, 99, 92)
WOOD = (96, 64, 40)
WOOD_SEAM = (34, 22, 16)
IRON = (78, 74, 76)
IRON_LIT = (150, 142, 138)
IRON_DARK = (30, 28, 30)
MOSS = (62, 78, 46)
SKY_TOP = (6, 7, 13)
SKY_LOW = (14, 14, 24)
STAR = (84, 88, 112)

# The light.
AMBIENT = 0.07                      # how much of the night gets everywhere
AMBIENT_TINT = (0.55, 0.62, 0.88)   # cold
TORCH_TINT = (1.0, 0.66, 0.38)      # warm
TORCH_POWER = 1.7
TORCH_REACH = 38.0                  # how far, in art pixels, before the light is down to half
TORCH_OUT = 8.0                     # how far the flames stand off the wall
TORCH_LIFT = 5.0                    # how far above the flame's base the light comes from
FLOOR_DEPTH = 1.8                   # how far back one row of floor pixels is, compared to one pixel across
LIGHT_STEP = 0.07                   # size of one band of light
VIGNETTE = 0.5

#: Something lit on the other side of the gate: a thin line of it gets out under the door and
#: across the step. Kept faint, it's a hint and nothing more.
UNDER_DOOR = (GATE_X + 0.5, 0.5, -3.0)
UNDER_DOOR_POWER = 0.55
UNDER_DOOR_REACH = 16.0
UNDER_DOOR_TINT = (1.0, 0.55, 0.26)
UNDER_DOOR_GAP = (196, 116, 52)


def ashlar(rng, top, bottom, h_lo, h_hi, w_lo, w_hi):
    """Courses of stone blocks: [(y0, y1, [(x0, x1, tint), ...]), ...], staggered so no joints line up."""
    courses = []
    y = top
    while y < bottom:
        h = rng.randint(h_lo, h_hi)
        if bottom - (y + h) < h_lo:
            h = bottom - y
        blocks = []
        x = -rng.randint(0, w_hi)
        while x < TITLE_W:
            w = rng.randint(w_lo, w_hi)
            blocks.append((x, x + w, rng.uniform(-1.0, 1.0)))
            x += w
        courses.append((y, y + h, blocks))
        y += h
    return courses


def find_block(courses, x, y):
    for y0, y1, blocks in courses:
        if y0 <= y < y1:
            for x0, x1, tint in blocks:
                if x0 <= x < x1:
                    return y0, y1, x0, x1, tint
    return None


def cracks(rng, courses, count):
    """Hairline cracks wandering down a few blocks, as a set of pixels."""
    out = set()
    for _ in range(count):
        y0, y1, blocks = courses[rng.randrange(len(courses))]
        x0, x1, _ = blocks[rng.randrange(len(blocks))]
        x, y = rng.randint(x0 + 2, max(x0 + 2, x1 - 3)), y0 + 2
        for _ in range(rng.randint(3, 8)):
            if y >= y1 - 1:
                break
            out.add((x, y))
            x += rng.choice((-1, 0, 0, 1))
            y += 1
    return out


def block_shade(x, y, y0, y1, x0, x1):
    """Bevel on a block: the top and left edges catch light, the bottom and right fall away."""
    if y == y0 + 1 or x == x0 + 1:
        return 1.12
    if y == y1 - 1 or x == x1 - 1:
        return 0.84
    return 1.0


def grain(x, y, seed, fine=0.08, broad=0.12, cell=5):
    """Speckle plus a broad blotch, as a multiplier near 1."""
    return 1.0 + (hash01(x, y, seed) - 0.5) * fine + (value_noise(x, y, cell, seed + 1) - 0.5) * broad


def mix_rgb(c, k):
    return (c[0] * k, c[1] * k, c[2] * k)


def arch_r(x, y):
    return math.hypot(x + 0.5 - GATE_X, y + 0.5 - SPRING)


def in_opening(x, y):
    if y >= GROUND:
        return False
    if y >= SPRING:
        return GATE_X - ARCH_IN <= x < GATE_X + ARCH_IN
    return arch_r(x, y) < ARCH_IN


def in_arch(x, y):
    """The voussoir ring, keystone included (it stands proud of the ring by a few pixels)."""
    if y >= SPRING:
        return False
    r = arch_r(x, y)
    if ARCH_IN <= r < ARCH_OUT:
        return True
    return ARCH_OUT <= r < ARCH_OUT + 3 and abs(x + 0.5 - GATE_X) < 6


def in_jamb(x, y):
    if not SPRING <= y < GROUND:
        return False
    return GATE_X - ARCH_OUT <= x < GATE_X - ARCH_IN or GATE_X + ARCH_IN <= x < GATE_X + ARCH_OUT


def opening_edge(x, y):
    """How far a door pixel is from the stone round it, for the shadow in the recess."""
    side = min(x - (GATE_X - ARCH_IN), GATE_X + ARCH_IN - 1 - x)
    if y < SPRING:
        side = min(side, ARCH_IN - arch_r(x, y))
    return side


def scene_pixel(x, y, ctx):
    """What one pixel is: (albedo, surface, how much light reaches into it). Surface is 'wall', 'door', 'floor' or 'sky'."""
    # The night, with the battlements against it.
    if y < PARAPET_TOP:
        merlon = (x + 6) % (MERLON_W + CRENEL_W) < MERLON_W and y >= MERLON_TOP
        if not merlon:
            if hash01(x, y, 91) > 0.9965:
                return STAR, "sky", 1.0
            return lerp_color(SKY_TOP, SKY_LOW, y / PARAPET_TOP), "sky", 1.0
        left = x - (x + 6) % (MERLON_W + CRENEL_W)
        if x == left or y == MERLON_TOP + 7:
            return MORTAR, "wall", 1.0
        k = 1.15 if y == MERLON_TOP else (1.08 if x == left + 1 else (0.85 if x == left + MERLON_W - 1 else 1.0))
        return mix_rgb(STONE, 0.9 * k * grain(x, y, 11)), "wall", 1.0

    # The coping: a course that sticks out, lit on top and throwing a shadow under its lip.
    if y < WALL_TOP:
        if y == WALL_TOP - 1:
            return mix_rgb(MORTAR, 0.6), "wall", 0.5
        if (x + 3) % 22 == 0:
            return MORTAR, "wall", 1.0
        k = 1.18 if y == PARAPET_TOP else 1.0
        return mix_rgb(STONE, 1.05 * k * grain(x, y, 12)), "wall", 1.0

    # The step in front of the gate: its top, then its face.
    if GROUND <= y < GROUND + STEP_H and STEP_X0 <= x < STEP_X1:
        if (x - STEP_X0) % 32 == 0:
            return MORTAR, "floor", 1.0
        if y < GROUND + 2:
            return mix_rgb(FLOOR_STONE, 1.12 * grain(x, y, 13)), "floor", 1.0
        return mix_rgb(FLOOR_STONE, 0.66 * grain(x, y, 13)), "floor", 0.8

    # The flagstones, laid flat and then put in perspective, so the joints run back toward the wall.
    if y >= GROUND:
        column, row = flagstone(x, y)
        if flagstone(x - 1, y)[0] != column or flagstone(x, y - 1)[1] != row:
            return MORTAR, "floor", 1.0
        k = 1.06 if flagstone(x, y - 2)[1] != row else (0.9 if flagstone(x, y + 1)[1] != row else 1.0)
        k *= 1.04 if flagstone(x - 2, y)[0] != column else 1.0
        k *= 1.0 + 0.16 * (hash01(column, row, 22) - 0.5)
        if hash01(x, y, 23) > 0.994:
            k *= 0.6
        return mix_rgb(FLOOR_STONE, k * grain(x, y, 14)), "floor", 1.0

    # The door, set back in the opening, so its edges are in shadow.
    if in_opening(x, y):
        # Light from the other side, getting out under the door and up the middle seam a little.
        if y == GROUND - 1:
            return UNDER_DOOR_GAP, "glow", 1.0
        if x in (GATE_X - 1, GATE_X) and y >= GROUND - 7:
            return lerp_color(WOOD_SEAM, UNDER_DOOR_GAP, 0.5 * (1.0 - (GROUND - 1 - y) / 7.0)), "glow", 1.0
        edge = opening_edge(x, y)
        ao = 0.45 + 0.55 * min(1.0, edge / 4.0)
        left = GATE_X - ARCH_IN
        # The gap between the two leaves cuts through everything.
        if x in (GATE_X - 1, GATE_X):
            return WOOD_SEAM, "door", ao
        # A ring pull either side of the gap, hanging off a boss. Iron goes over the plank seams.
        for rx in (GATE_X - 7, GATE_X + 6):
            if rx - 1 <= x < rx + 1 and 136 <= y < 138:
                return IRON_LIT, "door", ao
            if abs(math.hypot(x + 0.5 - rx, y + 0.5 - 141.5) - 3.0) < 0.7:
                return IRON_LIT, "door", ao
        # Two iron straps across each leaf, studded.
        for strap in (124, 152):
            if strap <= y < strap + 3:
                if (x - left) % 6 == 3 and y == strap + 1:
                    return IRON_LIT, "door", ao
                return (IRON_LIT if y == strap else (IRON if y == strap + 1 else IRON_DARK)), "door", ao
        if (x - left) % 8 == 0:
            return WOOD_SEAM, "door", ao
        plank = (x - left) // 8
        k = 1.0 + 0.1 * (hash01(plank, 0, 15) - 0.5) * 2.0
        streak = 0.82 + 0.18 * value_noise(x * 2, y // 5, 2, 16 + plank)
        return mix_rgb(WOOD, k * streak), "door", ao

    # The arch: wedge stones round the top, with the keystone in the middle a bit lighter.
    if in_arch(x, y):
        r = arch_r(x, y)
        a = math.atan2(SPRING - (y + 0.5), x + 0.5 - GATE_X)
        width = math.pi / VOUSSOIRS
        seg = min(VOUSSOIRS - 1, max(0, int(a / width)))
        to_joint = min(a - seg * width, (seg + 1) * width - a) * r
        keystone = seg == VOUSSOIRS // 2
        if to_joint < 0.75 or (r >= ARCH_OUT - 1 and not keystone) or r >= ARCH_OUT + 2:
            return MORTAR, "wall", 1.0
        k = 0.8 if r < ARCH_IN + 1.5 else (1.1 if r > ARCH_OUT - 2.5 else 1.0)
        k *= 1.1 if keystone else 1.0 + 0.06 * math.sin(seg * 2.3)
        return mix_rgb(STONE, 1.04 * k * grain(x, y, 17)), "wall", 1.0

    # The jambs: big blocks down either side of the door, their inner edge in shadow.
    if in_jamb(x, y):
        course = (y - SPRING) // 15
        inner = x == GATE_X - ARCH_IN - 1 or x == GATE_X + ARCH_IN
        if (y - SPRING) % 15 == 0 or x in (GATE_X - ARCH_OUT, GATE_X + ARCH_OUT - 1):
            return MORTAR, "wall", 1.0
        k = 0.78 if inner else 1.0
        k *= 1.0 + 0.05 * math.cos(course * 1.7)
        return mix_rgb(STONE, 1.02 * k * grain(x, y, 18)), "wall", 1.0

    # Everything else is the wall: coursed blocks, a few cracks, and moss creeping up from the ground.
    y0, y1, x0, x1, tint = find_block(ctx["wall"], x, y)
    if y == y0 or x == x0:
        return MORTAR, "wall", 1.0
    k = block_shade(x, y, y0, y1, x0, x1) * (1.0 + 0.07 * tint)
    if (x, y) in ctx["wall_cracks"]:
        k *= 0.5
    colour = mix_rgb(STONE, k * grain(x, y, 19))
    # Moss along the bottom of the lowest blocks, where the wet sits.
    creep = (y - (GROUND - 24)) / 24.0
    if creep > 0.0 and y >= y1 - 3 and value_noise(x, y, 3, 20) * (0.7 + 0.5 * creep) > 0.78:
        colour = lerp_color(colour, mix_rgb(MOSS, k), 0.75)
    return colour, "wall", 1.0


def flagstone(x, y):
    """Which flagstone a floor pixel is on: (column, row) in the flat layout.

    The floor is laid out flat with every stone the same size, then squeezed toward HORIZON, so
    the near rows come out bigger and the joints lean in toward the gate."""
    s = (y + 0.5 - HORIZON) / (GROUND - HORIZON)       # 1 at the foot of the wall, bigger nearer
    back = 1.0 - 1.0 / s                               # 0 at the wall
    across = GATE_X + (x + 0.5 - GATE_X) / s
    row = int(math.floor(back / FLAG_D))
    stagger = (row % 2) * 0.5 * FLAG_W + hash01(row, 0, 24) * 6.0
    return int(math.floor((across + stagger) / FLAG_W)), row


def torch_light(x, y, surface):
    """How much torchlight reaches a pixel, before it's cut into bands."""
    if surface == "floor":
        p = (x + 0.5, 0.0, (y - GROUND + 0.5) * FLOOR_DEPTH)
        n = (0.0, 1.0, 0.0)
    else:
        p = (x + 0.5, GROUND - y - 0.5, -4.0 if surface == "door" else 0.0)
        n = (0.0, 0.0, 1.0)
    lights = [((tx, GROUND - ty + TORCH_LIFT, TORCH_OUT), TORCH_POWER, TORCH_REACH) for tx, ty in TITLE_TORCHES]
    lights.append((UNDER_DOOR, UNDER_DOOR_POWER, UNDER_DOOR_REACH))
    total = 0.0
    for (lx, ly, lz), power, reach in lights:
        v = (lx - p[0], ly - p[1], lz - p[2])
        d = math.sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2])
        ndl = max(0.0, (v[0] * n[0] + v[1] * n[1] + v[2] * n[2]) / d)
        total += power * (0.3 + 0.7 * ndl) / (1.0 + (d / reach) ** 2)
    return total


def sconces(img):
    """The torches themselves: an iron cup on a wooden handle, held off the wall by a plate.
    Painted after the lighting with fixed colours, since they're right under the flame."""
    for tx, ty in TITLE_TORCHES:
        def px(x, y, c):
            img[y][x] = [c[0], c[1], c[2], 255]

        for y in range(ty + 6, ty + 15):                    # the plate on the wall
            for x in range(tx - 2, tx + 2):
                px(x, y, (118, 84, 56) if y == ty + 6 else ((58, 48, 44) if x == tx - 2 else (34, 29, 28)))
        for y in range(ty + 1, ty + 13):                    # the handle
            px(tx - 1, y, (156, 102, 58))
            px(tx, y, (92, 58, 34))
        for x in range(tx - 2, tx + 2):                     # the band holding it to the plate
            px(x, ty + 8, (96, 82, 74) if x < tx else (52, 44, 42))
        for x in range(tx - 3, tx + 3):                     # the cup the flame sits in
            px(x, ty - 1, (214, 160, 100))
            px(x, ty, (98, 82, 72) if x < tx else (64, 54, 50))
            px(x, ty + 1, (40, 34, 32))


def build_title_scene(seed=2601):
    rng = random.Random(seed)
    wall = ashlar(rng, WALL_TOP, GROUND, 10, 13, 16, 30)
    ctx = {"wall": wall, "wall_cracks": cracks(rng, wall, 26)}

    img = new_image(TITLE_W, TITLE_H)
    for y in range(TITLE_H):
        for x in range(TITLE_W):
            albedo, surface, ao = scene_pixel(x, y, ctx)
            # The night and the light under the door aren't lit by anything, they just are.
            if surface in ("sky", "glow"):
                img[y][x] = [albedo[0], albedo[1], albedo[2], 255]
                continue
            # Cut the torchlight into dithered bands.
            lit = torch_light(x, y, surface)
            lit = math.floor(lit / LIGHT_STEP + BAYER[y % 4][x % 4] / 16.0) * LIGHT_STEP
            dx, dy = (x - TITLE_W / 2) / (TITLE_W / 2), (y - TITLE_H / 2) / (TITLE_H / 2)
            vignette = 1.0 - VIGNETTE * (0.6 * dx * dx + 0.4 * dy * dy)
            img[y][x] = [clamp_byte(albedo[i] * (AMBIENT * AMBIENT_TINT[i] + lit * TORCH_TINT[i]) * ao * vignette)
                         for i in range(3)] + [255]
    sconces(img)
    write_png(os.path.join(OUT, "title-scene.png"), img)


# --------------------------------------------------------------------------- #
# Tiles and props: shared bits
# --------------------------------------------------------------------------- #

TILE = 16
SHEET_COLUMNS = 8

#: The darkest edge a prop or a detail tile gets on its shadow side.
INK = (18, 14, 13)


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


def blocks(img, ox, oy, rows, base, mortar, seed, v, bevel=(1.12, 0.84), grime=0.0, rounded=False):
    """Courses of stone blocks filling a tile. rows is [(top, bottom, [joints...])]; a joint is the
    x where one block ends and the next begins. Mortar runs along the top of each course and down
    each joint, and every block is lit on its top and left edges. Rounded knocks the corners off,
    for cobbles."""
    for top, bottom, joints in rows:
        edges = sorted(set([0] + list(joints) + [TILE]))
        for x0, x1 in zip(edges, edges[1:]):
            tint = 1.0 + (hash01(x0 + v * 7, top, seed) - 0.5) * 0.14
            for y in range(top, bottom):
                for x in range(x0, x1):
                    corner = rounded and (x in (x0 + 1, x1 - 1)) and (y in (top + 1, bottom - 1))
                    if y == top or x == x0 or corner:
                        img[oy + y][ox + x] = opaque(mortar, speck(x, y, seed, 0.1))
                        continue
                    k = tint * speck(x, y, seed + 1, 0.10) * (0.92 + 0.16 * tile_noise(x, y, seed + v, 4))
                    if y == top + 1 or x == x0 + 1:
                        k *= bevel[0]
                    elif y == bottom - 1 or x == x1 - 1:
                        k *= bevel[1]
                    k *= 1.0 - grime * (y / TILE)
                    img[oy + y][ox + x] = opaque(base, k)


def blades(img, ox, oy, rng, count, dark, light, tall=(2, 4)):
    """Grass blades: short upright strokes, dark at the root and catching the light at the tip. Kept
    inside the tile so the tiles still meet cleanly."""
    for _ in range(count):
        x, y = rng.randint(1, TILE - 2), rng.randint(tall[1], TILE - 1)
        h = rng.randint(*tall)
        lean = rng.choice((-1, 0, 0, 1))
        for i in range(h):
            yy, xx = y - i, x + (lean if i == h - 1 else 0)
            if 0 <= yy < TILE and 0 <= xx < TILE:
                img[oy + yy][ox + xx] = opaque(light if i == h - 1 else dark, speck(xx, yy, h, 0.1))


# --------------------------------------------------------------------------- #
# Tiles/village.png
# --------------------------------------------------------------------------- #
# An overcast village: grass, a dirt track, cobbles round the well, timber-framed houses with
# stone and plaster fronts two tiles tall, and thatch that hangs over the ground behind each house.

GRASS = (76, 90, 56)
GRASS_D = (58, 70, 44)
GRASS_L = (102, 116, 72)
GRASS_LONG = (70, 83, 51)
DIRT = (104, 86, 66)
DIRT_L = (124, 106, 82)
COBBLE = (112, 106, 98)
COBBLE_GAP = (52, 48, 44)
HOUSE_STONE = (122, 114, 104)
PLASTER = (178, 168, 148)
TIMBER = (64, 48, 36)
TIMBER_L = (92, 70, 52)
PLANK = (98, 72, 48)
PLANK_D = (62, 44, 30)
THATCH = (126, 106, 72)
THATCH_D = (88, 72, 48)
THATCH_L = (154, 134, 94)
GLASS = (34, 36, 40)


def paint_grass(img, ox, oy, v, seed, base=GRASS, count=7):
    rng = random.Random(seed)
    for y in range(TILE):
        for x in range(TILE):
            k = speck(x, y, seed, 0.12) * (0.92 + 0.16 * tile_noise(x, y, seed, 8))
            img[oy + y][ox + x] = opaque(base, k)
    blades(img, ox, oy, rng, count + v, GRASS_D, GRASS_L)


def paint_grass_long(img, ox, oy, v, seed):
    paint_grass(img, ox, oy, v, seed, GRASS_LONG, 12)


def paint_dirt(img, ox, oy, v, seed):
    rng = random.Random(seed)
    for y in range(TILE):
        for x in range(TILE):
            k = speck(x, y, seed, 0.16) * (0.9 + 0.2 * tile_noise(x, y, seed, 8))
            img[oy + y][ox + x] = opaque(DIRT, k)
    # A few small stones in it, lit on top with a shadow under.
    for _ in range(2 + v):
        x, y = rng.randint(1, TILE - 3), rng.randint(1, TILE - 3)
        img[oy + y][ox + x] = opaque(DIRT_L, 1.1)
        img[oy + y][ox + x + 1] = opaque(DIRT_L)
        img[oy + y + 1][ox + x] = opaque(DIRT, 0.7)
        img[oy + y + 1][ox + x + 1] = opaque(DIRT, 0.66)


#: How many cobbles across (and down) one tile.
COBBLES = 3

#: Where the middle of each cobble is, the same in every variant, so any two cobble tiles meet.
COBBLE_CENTRES = {(i, j): ((i + 0.25 + 0.5 * hash01(i, j, 71)) * TILE / COBBLES,
                           (j + 0.25 + 0.5 * hash01(i, j, 72)) * TILE / COBBLES)
                  for i in range(COBBLES) for j in range(COBBLES)}


def paint_cobble(img, ox, oy, v, seed):
    """Rounded stones of different sizes, set in a grid that's been shaken about. Each pixel belongs
    to the nearest stone's middle; where two stones are nearly as near, that's the gap between them.
    The grid wraps round the tile, so the stones on one edge carry on over the next tile's."""
    size = TILE / COBBLES
    for y in range(TILE):
        for x in range(TILE):
            px, py = x + 0.5, y + 0.5
            nearest = second = 1e9
            centre, stone = None, None
            ci, cj = int(px // size), int(py // size)
            for i in range(ci - 1, ci + 2):
                for j in range(cj - 1, cj + 2):
                    sx, sy = COBBLE_CENTRES[(i % COBBLES, j % COBBLES)]
                    sx += (i // COBBLES) * TILE
                    sy += (j // COBBLES) * TILE
                    d = math.hypot(px - sx, py - sy)
                    if d < nearest:
                        second, nearest = nearest, d
                        centre, stone = (sx, sy), (i % COBBLES, j % COBBLES)
                    elif d < second:
                        second = d
            if second - nearest < 1.2:
                img[oy + y][ox + x] = opaque(COBBLE_GAP, speck(x, y, seed, 0.15))
                continue
            # Lit on its upper left, falling away on its lower right, each stone its own shade.
            lean = (px - centre[0]) + (py - centre[1])
            k = 1.18 if lean < -1.6 else (0.82 if lean > 1.8 else 1.0)
            k *= 1.0 + 0.18 * (hash01(stone[0], stone[1], seed + v * 13) - 0.5)
            img[oy + y][ox + x] = opaque(COBBLE, k * speck(x, y, seed + 1, 0.08))


def paint_flowers(img, ox, oy, v, seed):
    """A few small flowers: a stalk, petals round a lighter middle."""
    rng = random.Random(seed)
    colours = ((196, 176, 88), (148, 116, 158), (204, 198, 184))
    for i in range(3):
        x, y = 3 + i * 4 + rng.randint(0, 1), rng.randint(4, 9) + (i % 2) * 3
        for dy in range(1, 3):
            img[oy + y + dy][ox + x] = opaque(GRASS_D)
        c = colours[(v + i) % len(colours)]
        for dx, dy in ((0, -1), (-1, 0), (1, 0), (0, 1)):
            img[oy + y + dy][ox + x + dx] = opaque(c, 1.05 if dx <= 0 and dy <= 0 else 0.82)
        img[oy + y][ox + x] = opaque((226, 210, 140))
    outline(img, ox, oy, TILE, TILE)


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


def paint_pebbles(img, ox, oy, v, seed):
    rng = random.Random(seed)
    for _ in range(5):
        x, y = rng.randint(2, TILE - 4), rng.randint(2, TILE - 4)
        w = rng.randint(1, 2)
        for dx in range(w + 1):
            img[oy + y][ox + x + dx] = opaque(COBBLE, 1.1)
            img[oy + y + 1][ox + x + dx] = opaque(COBBLE, 0.85)
    outline(img, ox, oy, TILE, TILE)


def paint_fence(img, ox, oy, v, seed, rails=True):
    """Two rails all the way across, and a post in the middle. Rails meet the next tile's rails."""
    if rails:
        for top in (6, 10):
            for x in range(TILE):
                img[oy + top][ox + x] = opaque(PLANK, 1.15 * speck(x, top, seed, 0.1))
                img[oy + top + 1][ox + x] = opaque(PLANK, 0.8 * speck(x, top + 1, seed, 0.1))
    for y in range(3, 15):
        for x in (7, 8, 9) if not rails else (7, 8):
            k = 1.15 if x == 7 else 0.85
            img[oy + y][ox + x] = opaque(PLANK, k * speck(x, y, seed, 0.12))
    outline(img, ox, oy, TILE, TILE)


def paint_fence_post(img, ox, oy, v, seed):
    paint_fence(img, ox, oy, v, seed, rails=False)


def paint_wall_stone(img, ox, oy, v, seed):
    rows = ((0, 8, (6 + 3 * v, 13)), (8, TILE, (3, 10 - v)))
    blocks(img, ox, oy, rows, HOUSE_STONE, COBBLE_GAP, seed, v, grime=0.18)


def paint_wall_plaster(img, ox, oy, v, seed):
    for y in range(TILE):
        for x in range(TILE):
            k = speck(x, y, seed, 0.06) * (0.95 + 0.08 * tile_noise(x, y, seed, 8)) * (1.0 - 0.12 * y / TILE)
            img[oy + y][ox + x] = opaque(PLASTER, k)
    # The timber frame: a post down the left of every tile and a beam along the top, so they join up.
    for y in range(TILE):
        img[oy + y][ox] = opaque(TIMBER_L)
        img[oy + y][ox + 1] = opaque(TIMBER)
    for x in range(TILE):
        img[oy][ox + x] = opaque(TIMBER_L)
        img[oy + 1][ox + x] = opaque(TIMBER)
    if v == 1:
        # A brace, bottom left to top right.
        for i in range(2, TILE - 1):
            for t in (0, 1):
                x, y = i, TILE - 1 - i + t
                if 2 <= y < TILE:
                    img[oy + y][ox + x] = opaque(TIMBER if t else TIMBER_L)


def paint_window(img, ox, oy, v, seed):
    paint_wall_plaster(img, ox, oy, 0, seed)
    for y in range(4, 13):
        for x in range(4, 13):
            frame = x in (4, 12) or y in (4, 12) or x == 8 or y == 8
            img[oy + y][ox + x] = opaque(TIMBER_L if frame and (x == 4 or y == 4) else (TIMBER if frame else GLASS))
    # The sill, sticking out and lit on top.
    for x in range(3, 14):
        img[oy + 13][ox + x] = opaque(TIMBER_L, 1.15)
        img[oy + 14][ox + x] = opaque(TIMBER, 0.7)


def paint_door(img, ox, oy, v, seed, top=False):
    """Half a plank door in a timber frame. The top half has the lintel; the bottom the threshold and the ring."""
    paint_wall_plaster(img, ox, oy, 0, seed)
    for y in range(TILE):
        for x in range(2, 14):
            if x in (2, 13) or (top and y < 3):
                img[oy + y][ox + x] = opaque(TIMBER_L if x == 2 or y == 0 else TIMBER)
                continue
            plank = (x - 3) // 3
            seam = (x - 3) % 3 == 2
            k = (0.7 if seam else 1.0) * speck(x, y, seed + plank, 0.12) * (1.0 + 0.06 * math.sin(plank * 2.1))
            img[oy + y][ox + x] = opaque(PLANK, k)
    # An iron strap across each half.
    strap = 7 if top else 5
    for x in range(3, 13):
        img[oy + strap][ox + x] = opaque((96, 92, 92))
        img[oy + strap + 1][ox + x] = opaque((44, 40, 40))
    if not top:
        for dx, dy in ((0, -1), (1, 0), (0, 1), (-1, 0)):
            img[oy + 10 + dy][ox + 10 + dx] = opaque((120, 114, 110))
        for x in range(1, 15):
            img[oy + TILE - 1][ox + x] = opaque(COBBLE, 0.8)


def paint_door_top(img, ox, oy, v, seed):
    paint_door(img, ox, oy, v, seed, top=True)


def thatch(x, y, seed):
    """The thatch colour at a pixel: straw streaks running down, with a darker band where each layer laps over the next."""
    streak = 0.85 + 0.3 * hash01(x, y // 3, seed)
    band = 0.72 if y % 5 == 4 else (1.1 if y % 5 == 0 else 1.0)
    return opaque(THATCH, streak * band * speck(x, y, seed + 1, 0.08))


def paint_roof(img, ox, oy, v, seed):
    for y in range(TILE):
        for x in range(TILE):
            img[oy + y][ox + x] = thatch(x, y + v, seed)


def paint_roof_ridge(img, ox, oy, v, seed):
    paint_roof(img, ox, oy, v, seed)
    # The ridge: a rolled cap of straw along the top, lit on its upper edge.
    for y in range(5):
        for x in range(TILE):
            k = (1.3 if y == 0 else 1.05 if y == 1 else 0.9 if y < 4 else 0.62) * speck(x, y, seed, 0.1)
            img[oy + y][ox + x] = opaque(THATCH_L if y < 2 else THATCH_D, k)


def paint_roof_eave(img, ox, oy, v, seed):
    """The bottom edge of the thatch: ragged straw ends, and nothing below them, so the wall shows through."""
    rng = random.Random(seed)
    for x in range(TILE):
        end = 10 + rng.randint(0, 2) + (1 if (x + v) % 3 == 0 else 0)
        for y in range(TILE):
            if y <= end:
                img[oy + y][ox + x] = thatch(x, y, seed)
            if y in (end - 1, end):
                img[oy + y][ox + x] = opaque(THATCH_D, 0.75 if y == end else 0.95)


VILLAGE_TILES = (       # (name, variants, painter), in the same order as TilesetCatalog.Village
    ("grass", 4, paint_grass),
    ("grass-long", 2, paint_grass_long),
    ("dirt", 3, paint_dirt),
    ("cobble", 3, paint_cobble),
    ("flowers", 2, paint_flowers),
    ("tuft", 2, paint_tuft),
    ("pebbles", 1, paint_pebbles),
    ("fence", 1, paint_fence),
    ("fence-post", 1, paint_fence_post),
    ("wall-stone", 2, paint_wall_stone),
    ("wall-plaster", 2, paint_wall_plaster),
    ("window", 1, paint_window),
    ("door-top", 1, paint_door_top),
    ("door", 1, paint_door),
    ("roof-ridge", 1, paint_roof_ridge),
    ("roof", 3, paint_roof),
    ("roof-eave", 2, paint_roof_eave),
)


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


def build_well(seed=8103):
    w, h = 32, 34
    img = prop_image(w, h)
    cx = w / 2.0
    # The stone ring: a cylinder seen from above, front face below the rim.
    for y in range(10, h):
        for x in range(w):
            rim = math.hypot((x + 0.5 - cx) / 15.0, (y + 0.5 - 17.0) / 6.0)
            if rim < 1.0:
                inner = math.hypot((x + 0.5 - cx) / 11.5, (y + 0.5 - 17.0) / 4.0)
                if inner < 1.0:
                    img[y][x] = opaque((22, 26, 30), 1.0 + 0.4 * (1.0 - inner))      # the water, far down
                else:
                    img[y][x] = opaque(HOUSE_STONE, 1.15 * speck(x, y, seed, 0.12))
            elif 17 <= y < h - 1 and abs(x + 0.5 - cx) < 15.0 * math.sqrt(max(0.0, 1.0 - ((y - 17) / 30.0) ** 2)):
                course = (y - 17) // 4
                joint = (y - 17) % 4 == 0 or (x + course * 3) % 7 == 0
                k = lit_side(x + 0.5, cx) * (0.6 if joint else 1.0) * speck(x, y, seed + 1, 0.12)
                img[y][x] = opaque(HOUSE_STONE, k * 0.95)
    # Two posts and a beam across with a rope down to a bucket.
    for y in range(2, 18):
        for x in (3, 4, 27, 28):
            img[y][x] = opaque(PLANK, 1.15 if x in (3, 27) else 0.8)
    for x in range(2, 30):
        img[2][x] = opaque(PLANK, 1.2)
        img[3][x] = opaque(PLANK, 0.85)
    for y in range(4, 11):
        img[y][16] = opaque((150, 132, 96))
    for y in range(11, 15):
        for x in range(14, 19):
            img[y][x] = opaque(PLANK, 1.1 if x < 16 else 0.8)
    outline(img, 0, 0, w, h)
    write_png(os.path.join(OUT, "Props", "well.png"), img)


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


if __name__ == "__main__":
    print("Generating world art for Legend Quest...")
    build_glow()
    build_mote()
    build_flame()
    build_title_scene()
    build_tileset("village", VILLAGE_TILES, 3100)
    build_tileset("crypt", CRYPT_TILES, 3200)
    build_barrel()
    build_crate()
    build_well()
    build_urn()
    build_sarcophagus()
    build_sconce()
    print("Done.")
