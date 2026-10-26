"""
World art for Legend Quest. For now that's the gate behind the title, the torch flame, and the two
soft textures light and embers are drawn with. The tilesets and props join it with the maps.

    python tools/generate_world.py

Only needs CPython (the PNG writer is in pixels.py), and every random number is seeded, so a rerun
writes the same bytes.
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


if __name__ == "__main__":
    print("Generating world art for Legend Quest...")
    build_glow()
    build_mote()
    build_flame()
    build_title_scene()
    print("Done.")
