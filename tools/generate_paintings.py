"""
The paintings behind the story's big moments, each in layers like the title, and the little
pictures the game moves over them (the rain, the lightning, the Soul-Bond's mark, glints on water,
the pier's lantern).

    python tools/generate_paintings.py            # all three, and the effects
    python tools/generate_paintings.py dawn       # just Questeria at dawn (or moon, pier, effects)

Every layer is the title's canvas, 416x277 art pixels, drawn at 4x, and the game sways them against
each other the way it sways the title (Story/Paintings.cs has the layers and how much each moves).
Only the middle 400x225 is ever on screen, and the conversation's panel sits over the bottom of
that, so what matters in each one is above row 176.

They're built out of my own ChatGPT pictures, not drawn from nothing: the title's sky, clouds,
mountains and city are sampled down the way cut_supplied.py samples them and then recoloured with
gradient maps (each pixel's brightness looked up on a new ramp of colours, with a share of its own
colour kept), so all of the picture's detail survives in the new light. Sunrise for Questeria, a
crimson night for the oath, a slate storm for the pier. What isn't in those pictures is drawn here
in their style, lit from the top left with lavender shadows: the ship's rail, the moon's halo, the
waves, the pier and the boat. The lanterns are the one off the lantern post on my props sheet.

Only needs CPython. There's nothing random in here: every bit of noise is hash01 on where it is, so
a rerun writes the same bytes. It reads one of my pictures at a time and lets it go before the next.
"""

import math
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import cut_supplied  # noqa: E402
from cut_supplied import TITLE_H, TITLE_W, keyed, sample  # noqa: E402
from pixels import OUT, hash01, new_image, write_png  # noqa: E402

W, H = TITLE_W, TITLE_H

BAYER = ((0, 8, 2, 10), (12, 4, 14, 6), (3, 11, 1, 9), (15, 7, 13, 5))


# --------------------------------------------------------------------------- #
# Helpers
# --------------------------------------------------------------------------- #

def lit(x, y, t):
    """Whether an ordered-dither cell is on at a level 0..1."""
    return t * 16 > BAYER[y % 4][x % 4]


def clamp(v):
    return max(0, min(255, int(round(v))))


def mix(a, b, t):
    """Part of the way from one colour to another, as an opaque pixel."""
    t = max(0.0, min(1.0, t))
    return [clamp(a[0] + (b[0] - a[0]) * t), clamp(a[1] + (b[1] - a[1]) * t), clamp(a[2] + (b[2] - a[2]) * t), 255]


def luma(p):
    return (p[0] * 0.299 + p[1] * 0.587 + p[2] * 0.114) / 255


def put(img, x, y, c, alpha=255):
    """Sets a pixel, or blends over it if alpha is under 255; off the canvas is ignored."""
    x, y = int(x), int(y)
    if not (0 <= x < len(img[0]) and 0 <= y < len(img)):
        return
    if alpha >= 255 or not img[y][x][3]:
        img[y][x] = [c[0], c[1], c[2], alpha]
    else:
        img[y][x] = mix(img[y][x], c, alpha / 255)


def ramp(stops, t):
    """A colour off a ramp of (where, colour) stops, 0..1, blending between the two either side."""
    t = max(0.0, min(1.0, t))
    for (a, ca), (b, cb) in zip(stops, stops[1:]):
        if t <= b:
            f = (t - a) / (b - a) if b > a else 0.0
            return [ca[k] + (cb[k] - ca[k]) * f for k in range(3)]
    return list(stops[-1][1])


def grade(img, stops, keep=0.0, lo=0.0, hi=1.0, gamma=1.0):
    """The gradient map: every drawn pixel's brightness, stretched from lo..hi and curved by gamma,
    looked up on the ramp, then `keep` of how far its own colour is from grey added back, so a pink
    cloud and a blue sky of the same brightness still come out different."""
    for line in img:
        for x, p in enumerate(line):
            if not p[3]:
                continue
            l = luma(p)
            c = ramp(stops, max(0.0, min(1.0, (l - lo) / (hi - lo))) ** gamma)
            g = l * 255
            line[x] = [clamp(c[k] + keep * (p[k] - g)) for k in range(3)] + [p[3]]
    return img


def glow(img, cx, cy, radius, colour, strength):
    """Pulls every drawn pixel toward a colour, most at a point and fading out round it."""
    for y, line in enumerate(img):
        for x, p in enumerate(line):
            if not p[3]:
                continue
            d = math.hypot(x - cx, y - cy) / radius
            if d < 2.5:
                img[y][x] = mix(p, colour, strength * math.exp(-d * d))[:3] + [p[3]]


def quiet_specks(img, rows, lift=0.13):
    """Takes out the stars and other specks: any pixel much brighter than the middle of the 5x5
    round it becomes that middle colour."""
    src = [[list(p) for p in line] for line in img]
    for y in rows:
        for x in range(2, W - 2):
            window = sorted((luma(src[yy][xx]), src[yy][xx]) for yy in range(max(0, y - 2), min(H, y + 3)) for xx in range(x - 2, x + 3))
            middle = window[len(window) // 2]
            if luma(src[y][x]) > middle[0] + lift:
                img[y][x] = list(middle[1])


def fade_disc(img, cx, cy, r, amount):
    """Fades a round thing (a moon) into the sky round it: each pixel inside goes most of the way
    to a blend of the sky where the row and the column through it leave the disc, the nearer the
    more, so the sky's gradient carries straight across where it was."""
    def at(x, y):
        return img[max(0, min(H - 1, int(y)))][max(0, min(W - 1, int(x)))]

    fill = {}
    for y in range(int(cy - r) - 1, int(cy + r) + 2):
        for x in range(int(cx - r) - 1, int(cx + r) + 2):
            if not (0 <= x < W and 0 <= y < H) or math.hypot(x + 0.5 - cx, y + 0.5 - cy) > r + 1:
                continue
            across = math.sqrt(max(0.0, (r + 2) ** 2 - (y + 0.5 - cy) ** 2))
            down = math.sqrt(max(0.0, (r + 2) ** 2 - (x + 0.5 - cx) ** 2))
            edges = ((cx - across, y), (cx + across, y), (x, cy - down), (x, cy + down))
            weights = [1 / (1 + abs(ex - x) + abs(ey - y)) for ex, ey in edges]
            total = sum(weights)
            fill[(x, y)] = [sum(at(ex, ey)[k] * w for (ex, ey), w in zip(edges, weights)) / total for k in range(3)]
    for (x, y), sky in fill.items():
        img[y][x] = mix(img[y][x], sky, amount)


def drop(img, rows):
    """The same picture moved down a number of rows, its bottom lost off the canvas."""
    out = new_image(W, H)
    for y in range(H - rows):
        out[y + rows] = img[y]
    return out


def over(dst, src, ox=0, oy=0):
    """Draws one picture over another, blending the pixels that are only partly there."""
    for y, line in enumerate(src):
        for x, p in enumerate(line):
            if p[3]:
                put(dst, ox + x, oy + y, p, p[3])


def edge_light(img, colour, strength):
    """Catches the light along the top edge of what's drawn: the sunrise on a skyline, the moon
    along a ridge. The dark line round a cut-out goes too, since the light's on it."""
    marks = []
    for y in range(1, H):
        for x in range(W):
            if img[y][x][3] and not img[y - 1][x][3]:
                marks.append((x, y))
    for x, y in marks:
        below = img[min(H - 1, y + 1)][x]
        base = below if below[3] and luma(below) > luma(img[y][x]) else img[y][x]
        img[y][x] = mix(base, colour, strength)
        if y + 1 < H and img[y + 1][x][3]:
            img[y + 1][x] = mix(img[y + 1][x], colour, strength * 0.4)


def save(name, layer, img):
    write_png(os.path.join(OUT, "Paintings", name + "-" + layer + ".png"), img)


def let_go():
    """Forgets the picture cut_supplied has read, so only one is ever in memory."""
    cut_supplied._pictures.clear()


#: The lantern off the lantern post on my props sheet: the box round it (under the post's arm) and
#: how much darker than the backdrop its glow can be and still be keyed out.
LANTERN_SHEET = "questeria-props-2.png"
LANTERN_BOX = (1231, 383, 32, 47)
LANTERN_DARK = 40


def lantern(width):
    """The lantern, cut to `width` art pixels across."""
    art = cut_supplied.cut_thing(LANTERN_SHEET, LANTERN_BOX, width=width, dark=LANTERN_DARK)
    let_go()
    return art


# --------------------------------------------------------------------------- #
# P-05: Questeria at dawn, from the Gull's Wager's rail
# --------------------------------------------------------------------------- #
# The sky is the title's, from just right of its tower to just left of its far tower, and low
# enough that its castle hides behind mine. The city is the title's whole city picture, its own
# lake and lamps and all, warmed and moved down so the golden spire's tip clears the top of the
# screen. The sun comes up behind the mountains on the left, where they dip.

DAWN_SKY_BOX = (170, 80, 1230, 819)
DAWN_CITY_DROP = 18
DAWN_SUN = (84, 122)
DAWN_SUN_R = 10

#: The title's two moons, where they come out on the canvas, and how far each fades into the dawn.
DAWN_MOONS = ((351.7, 27.0, 28.0, 0.9), (309.8, 54.1, 17.0, 1.0))

#: Night to sunrise: indigo where it was darkest, through rose, to gold and cream in the light.
DAWN = ((0.0, (68, 66, 140)), (0.2, (112, 98, 180)), (0.42, (186, 118, 172)), (0.6, (244, 156, 152)),
        (0.78, (255, 200, 152)), (1.0, (255, 242, 210)))
DAWN_GOLD = (255, 214, 150)
DAWN_CLOUD = ((0.0, (112, 78, 146)), (0.45, (226, 140, 162)), (0.75, (255, 196, 156)), (1.0, (255, 244, 214)))
DAWN_CITY = ((0.0, (38, 28, 68)), (0.25, (96, 64, 116)), (0.5, (190, 124, 142)), (0.75, (250, 194, 160)),
             (1.0, (255, 244, 214)))

#: The clouds out of title-clouds.png: the box round each, where its top left goes, and the sheet
#: pixels an art pixel takes (a bit more than the sky's, so they're nearer).
DAWN_CLOUDS = (((15, 13, 942, 394), (-36, 14), 5.2), ((902, 102, 619, 266), (318, 30), 5.6))


def dawn_sky():
    sky = sample("title-sky.png", DAWN_SKY_BOX, (W, H), key=False)
    let_go()
    quiet_specks(sky, range(0, 130))
    for cx, cy, r, amount in DAWN_MOONS:
        fade_disc(sky, cx, cy, r, amount)
    grade(sky, DAWN, keep=0.3, lo=0.1, hi=0.95)

    # The light coming up behind the mountains: warmest round the sun, and warmer lower down.
    sx, sy = DAWN_SUN
    for y in range(H):
        for x in range(W):
            d = math.hypot(x - sx, (y - sy) * 1.5) / 80
            warm = 0.78 * math.exp(-d * d) + 0.22 * max(0.0, min(1.0, (y - 40) / 90))
            sky[y][x] = mix(sky[y][x], DAWN_GOLD, warm * 0.75)

    # The sun: a pale disc, a gold rim, and its haze dithered out round it.
    for y in range(sy - 30, sy + 30):
        for x in range(sx - 30, sx + 30):
            d = math.hypot(x + 0.5 - sx, y + 0.5 - sy)
            if d < DAWN_SUN_R - 1.5:
                sky[y][x] = [255, 252, 232, 255]
            elif d < DAWN_SUN_R:
                sky[y][x] = [255, 236, 178, 255]
            elif d < DAWN_SUN_R + 7 and lit(x, y, (DAWN_SUN_R + 7 - d) / 8):
                sky[y][x] = mix(sky[y][x], (255, 226, 168), 0.7)
    return sky


def dawn_clouds():
    clouds = new_image(W, H)
    for box, at, pitch in DAWN_CLOUDS:
        art = sample("title-clouds.png", box, (round(box[2] / pitch), round(box[3] / pitch)))
        grade(art, DAWN_CLOUD, keep=0.4, lo=0.15, hi=0.95)
        over(clouds, art, *at)
    let_go()
    return clouds


def dawn_city():
    city = sample("title-city.png", cut_supplied.TITLE_SOURCE_BOX, (W, H), key=True, holes=True)
    let_go()
    grade(city, DAWN_CITY, keep=0.55, lo=0.04, hi=0.9)
    city = drop(city, DAWN_CITY_DROP)
    edge_light(city, (255, 214, 164), 0.55)
    return city


# The ship's rail, across the bottom and up the sides, drawn plank by plank: varnished dark wood
# lit from the top left, brass caps and fittings, a lantern hung off a bracket on the left post,
# a coil of rope on the right, and the ship's pennant flying from a staff. Only its two ends show
# beside the panel, so that's where the lantern and the rope are.

WOOD = ((38, 22, 34), (62, 36, 40), (92, 52, 42), (120, 70, 53), (160, 98, 62), (203, 140, 86), (236, 184, 124))
BRASS = ((96, 54, 40), (167, 101, 58), (207, 140, 69), (238, 183, 87), (250, 216, 139), (252, 240, 204))
ROPE = ((84, 58, 48), (134, 98, 68), (182, 142, 96), (220, 188, 134), (244, 222, 172))
NAVY = ((14, 31, 71), (21, 48, 102), (34, 79, 134), (60, 120, 180))
INK = (40, 24, 38)

RAIL_TOP = 224
RAIL_FACE = 228
RAIL_UNDER = 234
FOOT_RAIL = 248
BALUSTER_GAP = 14

#: The posts, (left, width, top): the lantern's on the left, the pennant's on the right with its
#: staff standing out of its cap, and the coil of rope beside it (its middle, and how wide).
LANTERN_POST = (12, 9, 128)
PENNANT_POST = (396, 9, 198)
STAFF_TOP = 92
COIL = (377, 13)


def grain(x, y, seed):
    """Wood grain: long streaks along a plank, a little darker or lighter, -1, 0 or 1."""
    n = hash01(x // (6 + int(hash01(y, 0, seed) * 14)), y, seed)
    return -1 if n < 0.18 else 1 if n > 0.9 else 0


def wood(level, x, y, seed=3):
    """A shade of the varnished wood, nudged by its grain."""
    return WOOD[max(0, min(len(WOOD) - 1, level + grain(x, y, seed)))]


def cylinder(img, x0, x1, y, palette, levels, seed=None):
    """One row of something round, lit from the left: its shade picked by how far across it is,
    and nudged by the grain if it's wood (a seed)."""
    for x in range(x0, x1):
        u = (x + 0.5 - x0) / max(1, x1 - x0)
        level = levels[min(len(levels) - 1, int(u * len(levels)))]
        if seed is not None:
            level += grain(x, y, seed)
        put(img, x, y, palette[max(0, min(len(palette) - 1, level))])


def baluster(img, cx):
    """A turned baluster between the rail and the foot rail: a collar, a neck, a vase, a neck, a collar."""
    top, bottom = RAIL_UNDER, FOOT_RAIL
    for y in range(top, bottom):
        t = (y - top) / (bottom - top)
        if t < 0.1 or t > 0.86:
            half = 3.5
        elif 0.22 < t < 0.66:
            half = 1.6 + 1.9 * math.sin(math.pi * (t - 0.22) / 0.44)
        else:
            half = 1.6
        cylinder(img, int(round(cx - half)), int(round(cx + half)), y, WOOD, (4, 5, 3, 2, 2, 1), 11)


def brass_cap(img, cx, y, half):
    """A brass dome on top of a post, with a little ball on it."""
    for row in range(5):
        h = half + 1 if row >= 2 else half - 1 + row
        cylinder(img, cx - h, cx + h + 1, y - 5 + row, BRASS, (2, 4, 5, 3, 2, 1, 1))
    for row, h in ((-8, 1), (-7, 2), (-6, 1)):
        cylinder(img, cx - h, cx + h + 1, y + row, BRASS, (3, 5, 2, 1))


def post(img, x, width, top, bottom=H):
    """A square post, its grain running up it: the lit left face, a bright arris down its corner,
    the shadowed right face, a brass band under its cap and another where it meets the rail."""
    for y in range(top, bottom):
        for i in range(width):
            level = 4 if i == 0 else 5 if i == 1 else 4 if i < width // 2 else 2 if i < width - 1 else 1
            put(img, x + i, y, WOOD[max(0, min(6, level + grain(y, x + i, 17)))])
    for y in list(range(top + 3, top + 5)) + list(range(RAIL_TOP - 2, RAIL_TOP + 1)):
        cylinder(img, x - 1, x + width + 1, y, BRASS, (4, 5, 3, 2, 2, 1))
    brass_cap(img, x + width // 2, top, width // 2)


def cap_rail(img):
    """The rail you'd lean on: a broad top caught by the sunrise, a rounded nose and a moulded face,
    and the foot rail along the deck under the balusters."""
    for x in range(W):
        for y in range(RAIL_TOP, RAIL_UNDER):
            if y == RAIL_TOP:
                c = wood(5, x, y)
                # The varnish shines where the sky's in it, most toward the sun.
                if hash01(x // 5, 1, 21) > 0.45 - 0.35 * math.exp(-((x - DAWN_SUN[0]) / 70) ** 2):
                    c = BRASS[4] if hash01(x, 2, 21) > 0.25 else WOOD[6]
            elif y < RAIL_FACE - 1:
                c = wood(5 if y == RAIL_TOP + 1 else 4, x, y)
            elif y == RAIL_FACE - 1:
                c = WOOD[3]
            elif y == RAIL_FACE + 2:
                c = WOOD[1]
            elif y == RAIL_FACE + 3:
                c = wood(3, x, y)
            elif y == RAIL_UNDER - 1:
                c = WOOD[1]
            else:
                c = wood(2, x, y)
            put(img, x, y, c)
        put(img, x, RAIL_UNDER, WOOD[0])
        for y in range(FOOT_RAIL, H):
            put(img, x, y, wood(4 if y == FOOT_RAIL else 2 if y < FOOT_RAIL + 4 else 1, x, y, 5))


def bracket(img, x0, x1, y):
    """The lantern's bracket: an iron arm out of the post with brass rivets, a brace under it, and a
    ring at its end."""
    for x in range(x0, x1):
        put(img, x, y, NAVY[2] if x % 5 else BRASS[3])
        put(img, x, y + 1, NAVY[1])
        put(img, x, y + 2, NAVY[0])
    for i in range(12):
        bx, by = x0 + int(i * (x1 - x0 - 6) / 12), y + 14 - i
        put(img, bx, by, NAVY[2])
        put(img, bx + 1, by, NAVY[1])
    for dx, dy in ((0, 0), (-1, 1), (1, 1), (0, 2)):
        put(img, x1 - 2 + dx, y + 3 + dy, BRASS[3] if dy < 2 else BRASS[1])


def rope_coil(img, cx, base, rx, turns=4):
    """A coil of rope lying on the rail, each turn a twisted tube a little smaller than the one under
    it: the far halves first, then the near halves from the bottom up, so each turn sits on the
    last. The middle is in shadow, and the end trails down over the rail's face."""
    rings = [(rx - i * 1.6, base - 2 - i * 3) for i in range(turns)]
    for near in (False, True):
        for r, cy in rings:
            ry = r * 0.34
            for y in range(int(cy - ry) - 3, int(cy + ry) + 3):
                for x in range(int(cx - r) - 3, int(cx + r) + 3):
                    if (y + 0.5 > cy) != near:
                        continue
                    ex, ey = (x + 0.5 - cx) / r, (y + 0.5 - cy) / ry
                    e = math.hypot(ex, ey)
                    if e == 0:
                        continue
                    slope = math.hypot(ex / r, ey / ry) / e
                    s = (e - 1) / slope / 1.7
                    if abs(s) > 1:
                        continue
                    light = 0.55 - 0.45 * s * (1 if near else -1) - 0.25 * ex
                    if (x + 2 * y) % 4 == 0:
                        light -= 0.3
                    put(img, x, y, ROPE[max(0, min(4, int(light * 5)))])
        if not near:
            r, cy = rings[-1]
            for y in range(int(cy - r * 0.34) + 2, int(cy) + 2):
                for x in range(int(cx - r) + 3, int(cx + r) - 2):
                    if ((x + 0.5 - cx) / (r - 2.5)) ** 2 + ((y + 0.5 - cy) / (r * 0.34 - 1.5)) ** 2 < 1:
                        put(img, x, y, ROPE[0] if (x + y) % 3 else WOOD[1])
    for i in range(16):
        x, y = cx + rx - 1 + int(i * 0.3), base - 2 + i
        for k in range(3):
            put(img, x + k, y, ROPE[(3, 2, 1)[k] if (y + 2 * k) % 4 else 0])


def pennant(img, x, y, length):
    """The ship's pennant off the top of its staff, a long swallowtail streaming to the left in the
    morning wind: navy, a gold edge, and a gold star near the staff."""
    for i in range(length):
        t = i / length
        bend = math.cos(t * 6.0 + 0.6)
        mid = y + 6 + math.sin(t * 6.0 + 0.6) * 2.4 * t
        half = 6.5 * (1 - t * 0.75)
        for row in range(int(mid - half), int(mid + half) + 1):
            off = abs(row + 0.5 - mid)
            if t > 0.82 and off < (t - 0.82) / 0.18 * half:
                continue
            shade = 2 if bend > 0.3 else 1 if bend > -0.4 else 0
            put(img, x - i, row, BRASS[3] if off > half - 1.2 else NAVY[shade + (1 if row < mid - half / 2 else 0)])
    for dx, dy in ((0, -2), (-1, -1), (0, -1), (1, -1), (-2, 0), (-1, 0), (0, 0), (1, 0), (2, 0), (-1, 1), (1, 1)):
        put(img, x - 8 + dx, y + 6 + dy, BRASS[4] if dy < 0 else BRASS[3])


def outline(img, colour):
    """A dark line round everything drawn, on the outside."""
    marks = []
    for y in range(H):
        for x in range(W):
            if img[y][x][3]:
                continue
            if any(0 <= x + dx < W and 0 <= y + dy < H and img[y + dy][x + dx][3] == 255
                   for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1))):
                marks.append((x, y))
    for x, y in marks:
        put(img, x, y, colour)


def dawn_rail():
    rail = new_image(W, H)
    for x in range(BALUSTER_GAP // 2, W, BALUSTER_GAP):
        baluster(rail, x)
    cap_rail(rail)
    lx, lw, ltop = LANTERN_POST
    post(rail, lx, lw, ltop)
    px, pw, ptop = PENNANT_POST
    post(rail, px, pw, ptop)

    # The pennant's staff, out of the right-hand post's cap, with a gold ball on top.
    staff = px + pw // 2
    for y in range(STAFF_TOP, ptop - 8):
        put(rail, staff, y, WOOD[5])
        put(rail, staff + 1, y, WOOD[2])
    for y in range(STAFF_TOP - 4, STAFF_TOP):
        cylinder(rail, staff - 1, staff + 3, y, BRASS, (4, 5, 3, 1))
    pennant(rail, staff - 1, STAFF_TOP, 54)

    rope_coil(rail, COIL[0], RAIL_TOP + 1, COIL[1])
    bracket(rail, lx + lw, lx + lw + 26, ltop + 10)
    outline(rail, INK)

    # The lantern hangs off the bracket's ring, and lights the post and the rail round it.
    art = lantern(14)
    aw, ah = len(art[0]), len(art)
    ax, ay = lx + lw + 24 - aw // 2, ltop + 15
    for y in range(ay - 24, ay + ah + 24):
        for x in range(ax - 24, ax + aw + 24):
            if 0 <= x < W and 0 <= y < H and rail[y][x][3] == 255:
                d = math.hypot(x - ax - aw / 2, y - ay - ah / 2) / 28
                if d < 1:
                    rail[y][x] = mix(rail[y][x], BRASS[4], 0.45 * (1 - d))
    for y in range(ay - 14, ay + ah + 14):
        for x in range(ax - 14, ax + aw + 14):
            d = math.hypot(x + 0.5 - ax - aw / 2, (y + 0.5 - ay - ah / 2) * 0.8) / 16
            if 0 <= x < W and 0 <= y < H and not rail[y][x][3] and d < 1 and lit(x, y, (1 - d) * 0.9):
                rail[y][x] = [255, 196, 112, int(110 * (1 - d) + 40)]
    over(rail, art, ax, ay)
    return rail


#: Gulls, as rows of a few letters: W white, G grey, D their dark wingtips, and how they're placed:
#: the shape, where its top left goes, and whether it's flipped.
GULL_SHAPES = (
    ("D.......D", ".G.....G.", "..GG.GG..", "...WWW...", "....W...."),
    ("..DG...GD..", ".D..GWG..D.", ".....W....."),
    ("D.....D", ".GG.GG.", "..WWW..", "...W..."),
)
GULLS = ((0, 162, 74, False), (1, 186, 64, True), (2, 220, 86, False))
GULL_COLOURS = {"W": (255, 246, 232), "G": (196, 170, 196), "D": (74, 56, 92)}


def dawn_gulls():
    gulls = new_image(W, H)
    for shape, gx, gy, flip in GULLS:
        for y, row in enumerate(GULL_SHAPES[shape]):
            for x, letter in enumerate(row[::-1] if flip else row):
                if letter in GULL_COLOURS:
                    put(gulls, gx + x, gy + y, GULL_COLOURS[letter])
    return gulls


def questeria_dawn():
    save("questeria-dawn", "sky", dawn_sky())
    save("questeria-dawn", "clouds", dawn_clouds())
    save("questeria-dawn", "gulls", dawn_gulls())
    save("questeria-dawn", "city", dawn_city())
    save("questeria-dawn", "rail", dawn_rail())


# --------------------------------------------------------------------------- #
# P-03: the oath at sixteen, under a red moon
# --------------------------------------------------------------------------- #
# The sky is the title's upper sky, nearer, turned to a crimson night. The moon is the title's big
# moon, blown up and recoloured blood red in a dozen steps, with a red haze round it. The mountains
# and the lake are title-far's, under the moon: the mountains squashed a little and slid along so
# their lowest dip is under it, mist where they meet the water, and the moon's road on the lake.
# The headland in front and its pines are drawn here as silhouettes edged with the moon. The two of
# you are the game's: it draws your figures on the headland the same way (Story/Paintings.cs has
# where), so the headland's top is flat where you stand.

MOON_SKY_BOX = (170, 0, 860, 573)
MOON_AT = (182, 88)
MOON_R = 56
MOON_SOURCE = (1131, 63, 157, 157)

#: The mountains and the lake off title-far, and where they go: the dip in the mountains is about
#: 1000 across the picture, so the cut starts that far left of the moon and runs off the picture's
#: right edge, where it carries on mirrored. The floating island's waterfall and clouds hang down
#: into the mountains' box, so anything above FALLS_CLEAR between those columns goes.
FAR_LEFT = 232
FAR_PITCH = 3.69
RIDGE_BOX = (445, 205)
RIDGE_TOP = 104
RIDGE_ROWS = 48
LAKE_BOX = (760, 264)
FALLS = (880, 1230)
FALLS_CLEAR = 515

NIGHT_RED = ((0.0, (10, 4, 18)), (0.25, (28, 8, 34)), (0.45, (66, 14, 46)), (0.65, (122, 26, 54)),
             (0.85, (190, 58, 66)), (1.0, (244, 136, 116)))
BLOOD = ((0.0, (96, 12, 30)), (0.25, (146, 24, 38)), (0.5, (192, 42, 46)), (0.7, (226, 74, 58)), (0.88, (246, 118, 84)),
         (1.0, (255, 170, 128)))
MOON_STEPS = 12
HALO = 18
RIDGE_RED = ((0.0, (14, 5, 18)), (0.35, (40, 12, 34)), (0.65, (96, 24, 48)), (1.0, (206, 74, 76)))
LAKE_RED = ((0.0, (10, 4, 16)), (0.35, (30, 8, 30)), (0.6, (84, 18, 44)), (0.85, (180, 48, 60)), (1.0, (250, 124, 98)))
HAZE = (120, 26, 46)
SHADOW = ((14, 4, 14), (24, 8, 22), (36, 12, 30))
RIM = ((120, 24, 40), (186, 48, 54), (240, 96, 80))

#: The headland you stand on: its top edge, as (x, row) points left to right. It drops into the lake
#: on the left and runs off the right edge; the flat bit is where the game stands the two of you.
HEADLAND = ((168, 214), (174, 196), (178, 178), (182, 166), (186, 158), (192, 156), (238, 156),
            (250, 154), (266, 151), (290, 152), (318, 147), (346, 149), (380, 143), (416, 140))
#: The rocks on the left, and the pines: where each one stands, how tall, how wide at the bottom.
LEFT_ROCKS = ((0, 186), (18, 180), (34, 184), (52, 196), (66, 214), (74, 240))
PINES = ((350, 148, 62, 26), (374, 144, 44, 20), (22, 184, 36, 18))


def moon_sky():
    sky = sample("title-sky.png", MOON_SKY_BOX, (W, H), key=False)
    let_go()
    grade(sky, NIGHT_RED, keep=0.15, lo=0.06, hi=0.92)
    mx, my = MOON_AT
    glow(sky, mx, my, MOON_R * 2.1, (150, 30, 50), 0.55)
    return sky


def moon_moon():
    """The moon, its own light and dark looked up on a ramp of reds, and a haze of red round it."""
    art = sample("title-sky.png", MOON_SOURCE, (MOON_R * 2 + 2, MOON_R * 2 + 2), key=False)
    let_go()
    moon = new_image(W, H)
    mx, my = MOON_AT
    for y in range(my - MOON_R - HALO, my + MOON_R + HALO + 1):
        for x in range(mx - MOON_R - HALO, mx + MOON_R + HALO + 1):
            d = math.hypot(x + 0.5 - mx, y + 0.5 - my) / MOON_R
            if d <= 1:
                p = art[y - my + MOON_R + 1][x - mx + MOON_R + 1]
                # A little brighter up and to the left, darker toward the rim, in a dozen steps.
                level = luma(p) * 1.3 - 0.2 - 0.3 * d ** 4 - 0.1 * ((x - mx) + (y - my)) / MOON_R
                level = int(max(0.0, min(1.0, level)) * MOON_STEPS) / MOON_STEPS
                put(moon, x, y, [clamp(c) for c in ramp(BLOOD, level)])
            else:
                t = (d - 1) * MOON_R / HALO
                if t < 1:
                    put(moon, x, y, RIM[1] if t < 0.12 else RIM[0], int(200 * (1 - t) ** 2))
    return moon


def ground_only(mask, box, clear):
    """Keeps the part of a key that stands on the bottom of its box (the mountains), and loses
    what floats over it, after clearing the rows above a line between two columns."""
    x0, y0 = box[0], box[1]
    for y, line in enumerate(mask):
        if y0 + y < clear[2]:
            for x in range(len(line)):
                if clear[0] <= x0 + x < clear[1]:
                    line[x] = 0
    keep = [bytearray(len(line)) for line in mask]
    stack = [(x, len(mask) - 1) for x in range(len(mask[0])) if mask[-1][x]]
    while stack:
        x, y = stack.pop()
        if keep[y][x] or not mask[y][x]:
            continue
        keep[y][x] = 1
        for nx, ny in ((x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1)):
            if 0 <= nx < len(mask[0]) and 0 <= ny < len(mask):
                stack.append((nx, ny))
    return keep


def far_band(top, rows, keyed_out):
    """A band of title-far across the whole canvas, starting FAR_LEFT along it and carrying on
    mirrored past its right edge."""
    band = new_image(W, rows)
    main_w = 1536 - FAR_LEFT
    cols = round(main_w / FAR_PITCH)
    for left, width, at, flip in ((FAR_LEFT, main_w, 0, False), (1536 - (W - cols) * FAR_PITCH, (W - cols) * FAR_PITCH, cols, True)):
        box = (int(left), top[0], int(round(width)), top[1])
        mask = ground_only(keyed("title-far.png", box), box, (FALLS[0], FALLS[1], FALLS_CLEAR)) if keyed_out else None
        art = sample("title-far.png", box, (W - cols if flip else cols, rows), key=keyed_out, mask=mask)
        for y in range(rows):
            line = art[y][::-1] if flip else art[y]
            for x, p in enumerate(line):
                if p[3]:
                    band[y][at + x] = p
    let_go()
    return band


def moon_shore():
    shore = new_image(W, H)
    ridge = grade(far_band(RIDGE_BOX, RIDGE_ROWS, True), RIDGE_RED, keep=0.12, lo=0.05, hi=0.85)
    lake_rows = round(LAKE_BOX[1] / FAR_PITCH)
    lake = grade(far_band(LAKE_BOX, lake_rows, False), LAKE_RED, keep=0.1, lo=0.06, hi=0.9)
    for y in range(RIDGE_ROWS):
        shore[RIDGE_TOP + y] = ridge[y]
    water = RIDGE_TOP + RIDGE_ROWS
    for y in range(H - water):
        shore[water + y] = [list(p) for p in lake[y if y < lake_rows else 2 * lake_rows - y - 1]]
    edge_light(shore, RIM[2], 0.5)

    # Mist where the mountains go down into the lake, red with the moon.
    for y in range(water - 8, water + 6):
        for x in range(W):
            if shore[y][x][3]:
                shore[y][x] = mix(shore[y][x], HAZE, 0.5 * (1 - abs(y - water + 1) / 8))
    # The moon's road on the water: short strokes, narrow far off and wider toward you.
    mx = MOON_AT[0]
    for y in range(water, H):
        depth = (y - water) / (H - water)
        half = 6 + depth * 44
        for x in range(int(mx - half), int(mx + half)):
            n = hash01(x // (2 + int(depth * 4)), y, 33)
            centre = 1 - abs(x + 0.5 - mx) / half
            if n < 0.25 + 0.55 * centre and y % 2 == (x // 7) % 2:
                put(shore, x, y, RIM[2] if n < 0.2 * centre else RIM[1])
    return shore


def pine(foot_x, foot_y, height, width):
    """A pine as a set of cells: a trunk, and tiers of boughs, each wider than the one above and
    ragged at the ends, with a spike on top."""
    cells = set()
    for y in range(foot_y - height // 6, foot_y + 1):
        cells |= {(foot_x - 1, y), (foot_x, y)}
    tiers, top = 5, foot_y - height
    for k in range(tiers):
        start = top + 3 + (height * 0.8 - 3) * k / tiers
        tall = height * 0.8 / tiers * 1.6
        reach = width / 2 * (0.35 + 0.65 * (k + 1) / tiers)
        for y in range(int(start), int(start + tall)):
            half = reach * (y - start) / tall + (hash01(y, k, 71) - 0.5) * 1.6
            for x in range(int(round(foot_x - half)), int(round(foot_x + half)) + 1):
                cells.add((x, y))
    for y in range(top, top + 4):
        cells.add((foot_x, y))
    return cells


def silhouette(img, cells, toward, shadow=SHADOW, rim=RIM):
    """Fills a set of cells near-black and edges them with the light behind them (the moon, the
    lantern): the edges facing it brightest."""
    tx, ty = toward
    for x, y in cells:
        put(img, x, y, shadow[0] if hash01(x // 3, y // 2, 81) > 0.3 else shadow[1])
    for x, y in cells:
        best = 0.0
        for dx, dy in ((1, 0), (-1, 0), (0, -1), (1, -1), (-1, -1)):
            if (x + dx, y + dy) not in cells:
                d = math.hypot(tx - x, ty - y) or 1
                best = max(best, (dx * (tx - x) + dy * (ty - y)) / d)
        if best > 0.7:
            put(img, x, y, rim[2] if best > 0.9 else rim[1])
        elif best > 0.35:
            put(img, x, y, rim[0])


def moon_headland():
    front = new_image(W, H)
    cells = set()

    def fill_below(points):
        for (ax, ay), (bx, by) in zip(points, points[1:]):
            for x in range(int(ax), int(bx) + 1):
                t = (x - ax) / max(1, bx - ax)
                top = ay + (by - ay) * t + (hash01(x, 0, 91) - 0.5) * 1.4
                for y in range(int(top), H):
                    cells.add((x, y))

    fill_below(HEADLAND)
    fill_below(LEFT_ROCKS)
    for foot_x, foot_y, height, width in PINES:
        cells |= pine(foot_x, foot_y, height, width)
    # Grass along the top, a blade here and there, longer where nobody's standing.
    for x in range(0, W):
        tops = [y for y in range(100, H) if (x, y) in cells]
        if not tops or 186 <= x <= 232:
            continue
        top = tops[0]
        if hash01(x, 1, 93) > 0.72:
            for k in range(1 + int(hash01(x, 2, 93) * 3)):
                cells.add((x + (k // 2 if hash01(x, 3, 93) > 0.5 else 0), top - 1 - k))
    silhouette(front, cells, MOON_AT)
    return front


def red_moon():
    save("red-moon", "sky", moon_sky())
    save("red-moon", "moon", moon_moon())
    save("red-moon", "shore", moon_shore())
    save("red-moon", "headland", moon_headland())


# --------------------------------------------------------------------------- #
# P-03: the pier in the storm where you met, at eight
# --------------------------------------------------------------------------- #
# The sky is the title's again, turned to slate, under storm clouds that are the title's cumulus
# gone dark: grey on top, near-black underneath, and thinning to a pale band along the horizon. The
# sea is a field of waves seen in perspective, a few long swells and a chop across them, each pixel
# shaded by which way the water there faces (toward the sky, light; toward you, dark), white where
# a crest breaks, with the title city's lake laid into it for its streaks of light. Home's a dark
# line on the horizon with a few windows lit. The capsized boat's out in it with you in the water
# beside it, and the pier comes in from the left with its lantern post at the end and a child
# running for the edge: one of the children off my villagers sheet, as a silhouette in the lantern's
# light. The game hangs the lantern on the post and swings it, and does the rain and the lightning.

PIER_SKY_BOX = (170, 40, 860, 573)
STORM_SKY = ((0.0, (14, 18, 28)), (0.3, (28, 34, 48)), (0.55, (48, 56, 72)), (0.8, (80, 90, 106)), (1.0, (136, 146, 160)))
STORM_CLOUD = ((0.0, (10, 12, 20)), (0.4, (22, 26, 38)), (0.65, (40, 46, 60)), (0.85, (72, 80, 96)), (1.0, (126, 136, 150)))
STORM_LIGHT = (104, 116, 130)
#: The storm clouds out of title-clouds.png, the same way as the dawn's: box, top left, pitch.
STORM_CLOUDS = (((902, 102, 619, 266), (220, 4), 2.6), ((15, 13, 942, 394), (-40, -14), 2.9),
                ((776, 423, 737, 271), (176, 36), 2.9), ((15, 643, 843, 327), (-34, 44), 3.1))

HORIZON = 118
#: The coast you sailed from, where it starts and stops along the horizon.
COAST = (0, 132)
SEA = ((6, 12, 18), (10, 22, 30), (18, 36, 44), (30, 52, 58), (48, 72, 78), (76, 102, 106), (120, 142, 146),
       (188, 202, 204), (232, 238, 238))
#: The sea's waves: (how high, across, along, where it starts), the along being toward the horizon.
#: The first two are the long swells, the rest the chop across them.
SWELLS = ((1.0, 0.06, 0.55, 0.0), (0.6, -0.25, 0.42, 1.7), (0.4, 0.38, 0.95, 4.1), (0.25, 0.9, 1.7, 2.3),
          (0.18, -1.3, 2.6, 0.7))
#: How far the sea runs to the horizon (bigger is further), and how wide it is.
SEA_DEPTH = 1500
SEA_WIDTH = 300
FOAM = 1.45
SEA_TEXTURE_BOX = (0, 770, 1536, 254)
SEA_TEXTURE = 0.3

#: The capsized boat: where the middle of its gunwale is on the water, and how long it is.
BOAT = (306, 148, 52)
HULL = ((22, 14, 18), (52, 28, 30), (86, 42, 40), (118, 62, 54), (150, 92, 76), (176, 132, 112))

#: The pier: its deck's far and near edges from the left of the canvas to its far end, the beam
#: under the deck, the posts it stands on, and the lantern post at its end (foot, top, arm length).
PIER_FAR = ((0, 204), (234, 146))
PIER_NEAR = ((0, 222), (238, 150))
PIER_FACE = (9, 3)
PIER_POSTS = 12
LAMP_POST = (212, 92, 14)
#: Where the lantern's light is, a little under its hook.
LAMP = (LAMP_POST[0] + LAMP_POST[2], LAMP_POST[1] + 18)
PLANK = ((12, 10, 16), (24, 22, 28), (40, 36, 40), (62, 56, 58), (92, 86, 86), (128, 124, 122), (170, 166, 160))
LAMPLIGHT = (255, 196, 120)

#: The child running for the end of the pier to dive in after you: one of the children off my
#: villagers sheet, mid-stride, and where their feet land. And you, in the water by the boat.
CHILD_SHEET = "npc-villagers.png"
CHILD_GRID = ((0, 636, 1448, 450), 24, 2, 22)
CHILD_PITCH = 5.0
CHILD_FEET = (231, 149)
YOU_IN_WATER = (273, 151)
STORM_SHADOW = ((6, 8, 14), (12, 14, 22), (20, 22, 32))
STORM_RIM = ((36, 40, 54), (104, 88, 78), (214, 168, 116))


def pier_sky():
    sky = sample("title-sky.png", PIER_SKY_BOX, (W, H), key=False)
    let_go()
    quiet_specks(sky, range(0, H), 0.08)
    grade(sky, STORM_SKY, keep=0.06, lo=0.08, hi=0.95)
    clouds = new_image(W, H)
    for box, at, pitch in STORM_CLOUDS:
        art = sample("title-clouds.png", box, (round(box[2] / pitch), round(box[3] / pitch)))
        grade(art, STORM_CLOUD, keep=0.04, lo=0.2, hi=0.98, gamma=1.4)
        over(clouds, art, *at)
    let_go()
    over(sky, clouds)
    # The storm's thinner along the horizon: a pale band under the clouds.
    for y in range(HORIZON - 40, HORIZON + 1):
        f = ((y - HORIZON + 40) / 40) ** 2
        for x in range(W):
            sky[y][x] = mix(sky[y][x], STORM_LIGHT, f * 0.6)
    return sky


def sea_height(across, along):
    """How high the sea is at a point on it: the swells added up, each one peaked at its crest."""
    return sum(a * ((math.sin(kx * across + kd * along + phase) + 1) / 2) ** 2 for a, kx, kd, phase in SWELLS)


def pier_sea():
    sea = new_image(W, H)
    texture = sample("title-city.png", SEA_TEXTURE_BOX, (W, H - HORIZON), key=False)
    let_go()
    for y in range(HORIZON, H):
        z = y - HORIZON + 1.5
        along, nearer = SEA_DEPTH / z, SEA_DEPTH / (z + 1)
        far = max(0.0, 1 - z / 50)
        for x in range(W):
            across = (x - W / 2) * along / SEA_WIDTH
            h = sea_height(across, along)
            facing = h - sea_height(across, nearer)
            # Facing the sky it's light, facing you it's dark; the far sea hazes toward grey.
            b = 0.36 + facing * (1.6 + z * 0.03) + (h - 1.0) * 0.24
            b += (hash01(x // (2 + int(z / 25)), y, 7) - 0.5) * 0.14
            b += (luma(texture[y - HORIZON][x]) - 0.3) * SEA_TEXTURE
            b = b * (1 - far * 0.55) + 0.25 * far
            c = SEA[max(0, min(6, int(b * 7)))]
            foam = h - (FOAM + far * 0.3 + z * 0.002)
            if foam > 0 and facing < 0.05 and hash01(x // 2, y, 9) < foam * 5 - z * 0.004:
                c = SEA[8] if foam > 0.1 else SEA[7]
            sea[y][x] = list(c) + [255]
    for x in range(W):
        put(sea, x, HORIZON, SEA[5] if hash01(x // 3, 0, 115) > 0.3 else SEA[4])
    # The lantern's light on the water under it, in broken strokes.
    lx = LAMP[0]
    for y in range(HORIZON + 30, HORIZON + 90):
        half = 3 + (y - HORIZON - 30) * 0.18
        for x in range(int(lx - half), int(lx + half) + 1):
            if hash01(x // 2, y, 125) > 0.55 + abs(x - lx) / half * 0.4 and luma(sea[y][x]) > 0.12:
                sea[y][x] = mix(sea[y][x], LAMPLIGHT, 0.55)
    coast(sea)
    capsized_boat(sea)
    swimmer(sea)
    return sea


def swimmer(img):
    """You, in the water by the boat: a head and shoulders above the waves and an arm up, and the
    water white round you."""
    hx, hy = YOU_IN_WATER
    cells = {(hx + dx, hy - 3 + dy) for dx in range(-1, 2) for dy in range(-1, 2) if abs(dx) + abs(dy) < 2}
    cells |= {(hx - 1, hy - 1), (hx, hy - 1), (hx + 1, hy - 1)}
    cells |= {(hx + 2, hy - 4), (hx + 2, hy - 5), (hx + 3, hy - 6), (hx + 3, hy - 7), (hx + 3, hy - 8)}
    silhouette(img, cells, LAMP, STORM_SHADOW, STORM_RIM)
    for x in range(hx - 4, hx + 6):
        if hash01(x, 3, 131) > 0.3:
            put(img, x, hy, SEA[7])


def coast(img):
    """Home, far off on the left: a low line of dark land on the horizon, a few windows lit in it."""
    for x in range(COAST[0], COAST[1]):
        u = (x - COAST[0]) / (COAST[1] - COAST[0])
        tall = 2 + 6 * math.sin(math.pi * min(1.0, u * 1.3)) ** 0.7 + hash01(x // 3, 0, 127) * 1.5
        for y in range(int(HORIZON - tall), HORIZON + 2):
            put(img, x, y, SEA[1] if y > HORIZON - tall + 1 else SEA[2])
        if hash01(x, 1, 129) > 0.86 and tall > 4:
            put(img, x, int(HORIZON - tall / 2), LAMPLIGHT)


def capsized_boat(img):
    """The rowing boat, over: its round hull up out of the water, red paint gone dark in the storm,
    the strakes of its planking running along it and the keel along the top, and the sea breaking
    white against its side."""
    cx, cy, length = BOAT
    half = length / 2
    for x in range(int(cx - half), int(cx + half) + 1):
        u = (x + 0.5 - cx) / half
        if abs(u) > 1:
            continue
        # Higher in the middle, the bow (right) end lifted a little, and the stern pointed.
        rise = 8.5 * (1 - u * u) ** 0.45 * (1 + 0.12 * u)
        top = cy - rise
        for y in range(int(top), cy + 1):
            v = (y + 0.5 - top) / max(1.0, rise)
            level = 5 if v < 0.12 else 4 if v < 0.3 else 3 if v < 0.55 else 2 if v < 0.8 else 1
            # The strakes: a dark line where each plank laps the one under it.
            if int(v * 4.5) != int((v + 1 / max(1.0, rise)) * 4.5):
                level -= 2
            if u > 0.6:
                level -= 1
            put(img, x, y, HULL[max(0, min(5, level))])
        put(img, x, int(top) - 1, HULL[0])
        if abs(u) < 0.9:
            # The keel along the top, dark, with the sky's light along it.
            put(img, x, int(top) - 1, HULL[1])
            put(img, x, int(top) - 2, HULL[0])
            put(img, x, int(top), HULL[5] if x % 4 else HULL[4])
    for x in range(int(cx - half) - 8, int(cx + half) + 9):
        for y in range(cy - 1, cy + 4):
            edge = abs(x - cx) > half - 4
            if hash01(x, y, 117) > (0.25 if edge else 0.45) + (y - cy) * 0.15:
                put(img, x, y, SEA[8] if y <= cy else SEA[7])
        if abs(x - cx) > half - 6 and hash01(x, 1, 119) > 0.5:
            for k in range(1 + int(hash01(x, 2, 119) * 4)):
                put(img, x, cy - 2 - k, SEA[7] if k else SEA[8])


def pier_line(points, x):
    (ax, ay), (bx, by) = points
    return ay + (by - ay) * (x - ax) / (bx - ax)


def pier_pier():
    pier = new_image(W, H)
    end = PIER_FAR[1][0]
    # The deck: planks across it, closer together toward the far end, wet and catching the sky.
    seams = [end * (1 - (1 - i / 34) ** 1.7) for i in range(35)]
    for x in range(0, PIER_NEAR[1][0] + 1):
        far, near = pier_line(PIER_FAR, min(x, end)), pier_line(PIER_NEAR, x)
        plank = next((i for i, s in enumerate(seams) if s >= x), len(seams))
        seam = any(abs(x - s) < 0.6 for s in seams)
        for y in range(int(far), int(near) + 1):
            t = (y - far) / max(1.0, near - far)
            level = 1 if seam else 4 if t < 0.2 else 3 if t < 0.6 else 2
            if not seam and 0.3 < t < 0.5 and hash01(plank, 3, 121) > 0.6:
                level = 5
            put(pier, x, y, PLANK[level])
        put(pier, x, int(far), PLANK[5] if not seam else PLANK[3])
        # The beam along the side of it.
        face = PIER_FACE[0] + (PIER_FACE[1] - PIER_FACE[0]) * min(1.0, x / end)
        for y in range(int(near) + 1, int(near + face) + 1):
            put(pier, x, y, PLANK[3] if y == int(near) + 1 else PLANK[1] if (y + x // 6) % 4 else PLANK[2])
    # The posts it stands on, down into the sea, every other one standing up through the deck,
    # with a rope slung between those along the near side.
    tops = []
    for i in range(PIER_POSTS):
        t = (i / (PIER_POSTS - 1)) ** 1.3
        x = int(2 + t * (end - 8))
        width = max(2, int(round(8 - t * 6)))
        near = pier_line(PIER_NEAR, x)
        bottom = int(near + 26 - t * 18)
        up = int(10 - t * 6) if i % 2 == 0 else -2
        for y in range(int(near) - up, bottom):
            for k in range(width):
                put(pier, x + k, y, PLANK[4] if k == 0 else PLANK[3] if k == 1 else PLANK[2] if k < width - 1 else PLANK[0])
        for k in range(width):
            put(pier, x + k, int(near) - up, PLANK[5])
        if i % 2 == 0:
            tops.append((x + width // 2, int(near) - up + 2))
    for (ax, ay), (bx, by) in zip(tops, tops[1:]):
        for x in range(ax, bx):
            f = (x - ax) / max(1, bx - ax)
            sag = 1 + (bx - ax) * 0.08
            put(pier, x, int(ay + (by - ay) * f + sag * math.sin(f * math.pi)), PLANK[5] if x % 3 else PLANK[4])

    # The lantern post at the end, and its arm out over the water with a brace under it.
    px, ptop, arm = LAMP_POST
    base = int(pier_line(PIER_FAR, px)) + 2
    for y in range(ptop, base):
        put(pier, px, y, PLANK[5])
        put(pier, px + 1, y, PLANK[3])
        put(pier, px + 2, y, PLANK[1])
    for x in range(px - 1, px + arm + 1):
        put(pier, x, ptop + 3, PLANK[5])
        put(pier, x, ptop + 4, PLANK[2])
    for i in range(7):
        put(pier, px + 3 + i, ptop + 11 - i, PLANK[3])
        put(pier, px + 3 + i, ptop + 12 - i, PLANK[1])
    for y in range(ptop - 3, ptop):
        put(pier, px, y, PLANK[4])
        put(pier, px + 1, y, PLANK[3])
    outline(pier, (6, 6, 12))
    child(pier)

    # The lantern lights the end of the pier round it.
    lx, ly = LAMP
    for y in range(ly - 30, ly + 50):
        for x in range(lx - 40, lx + 40):
            if 0 <= x < W and 0 <= y < H and pier[y][x][3]:
                d = math.hypot(x - lx, (y - ly) * 0.8) / 38
                if d < 1:
                    pier[y][x] = mix(pier[y][x], LAMPLIGHT, 0.5 * (1 - d) ** 1.5)
    return pier


def child(img):
    """The child at the end of the pier, as a silhouette edged with the lantern's light."""
    box, columns, row, column = CHILD_GRID
    found = [b for b in cut_supplied.things(CHILD_SHEET, box) if b[2] > 4][:columns * 4]
    found.sort(key=lambda b: b[1])
    x, y, bw, bh = sorted(found[row * columns:(row + 1) * columns])[column]
    art = sample(CHILD_SHEET, (x, y, bw, bh), (round(bw / CHILD_PITCH), round(bh / CHILD_PITCH)))
    let_go()
    fx, fy = CHILD_FEET
    left, top = fx - len(art[0]) // 2, fy - len(art)
    cells = {(left + x, top + y) for y, line in enumerate(art) for x, p in enumerate(line) if p[3]}
    silhouette(img, cells, LAMP, STORM_SHADOW, STORM_RIM)


def pier_memory():
    save("pier-memory", "sky", pier_sky())
    save("pier-memory", "sea", pier_sea())
    save("pier-memory", "pier", pier_pier())


# --------------------------------------------------------------------------- #
# The little pictures the game moves over the paintings
# --------------------------------------------------------------------------- #
# Each is a strip of frames the same size, and PaintingSprite has the sizes: the weather (a white
# cell for the lightning's flash, then a near raindrop and a far one), two lightning bolts, the
# Soul-Bond's mark and its glow, a glint on water growing and going, and the pier's lantern at five
# points of its swing. All of it's drawn at the paintings' 4x, on the same pixels.

WEATHER_CELL = (8, 16)
RAIN = (176, 190, 206)
BOLT_CELL = (44, 92)
BOLT = ((236, 242, 255), (164, 182, 236), (92, 108, 176))
SIGIL_CELL = 24
SIGIL_GOLD = ((255, 250, 226), (255, 226, 150), (244, 180, 88), (214, 120, 60))
GLINT_CELL = 5
LANTERN_CELL = (16, 26)
LANTERN_SWING = (-2, -1, 0, 1, 2)
LANTERN_WIDTH = 9


def weather():
    """The white cell the flash stretches over the screen, then a near drop and a far one: a slant
    of pixels, faint at the tail and bright at the head."""
    cw, ch = WEATHER_CELL
    strip = new_image(cw * 3, ch)
    for y in range(ch):
        for x in range(cw):
            strip[y][x] = [255, 255, 255, 255]
    for frame, length in ((1, 12), (2, 7)):
        for i in range(length):
            x = frame * cw + cw - 2 - i // 4
            put(strip, x, i, RAIN, int(60 + 170 * i / length))
    return strip


def bolt(seed):
    """A lightning bolt: straight strokes down from the clouds, each kinking off a few pixels the
    other way, a fork or two off it, a white core, a blue line beside it and a faint haze round
    that."""
    cw, ch = BOLT_CELL
    img = new_image(cw, ch)
    core = set()

    def walk(x, y, rows, lean, s):
        i = 0
        while rows > 0 and 1 < x < cw - 2 and y < ch - 1:
            run = 3 + int(hash01(i, s, seed) * 7)
            shift = (int(hash01(i, s + 1, seed) * 7) - 3) + lean
            for k in range(min(run, rows)):
                core.add((int(round(x + shift * k / run)), y + k))
            x, y, rows, i = x + shift, y + run, rows - run, i + 1

    walk(cw // 2, 0, ch, 0, 0)
    path = sorted(core, key=lambda c: c[1])
    for k, at in enumerate((0.3, 0.55)):
        x, y = path[int(len(path) * at)]
        walk(x, y, int(ch * (0.3 - k * 0.1)), (-2, 2)[(seed + k) % 2], 10 + k)
    for x, y in core:
        for dx in (-2, -1, 1, 2):
            if (x + dx, y) not in core:
                put(img, x + dx, y, BOLT[2], 90)
    for x, y in core:
        for dx in (-1, 1):
            if (x + dx, y) not in core and hash01(x, y, seed) > 0.3:
                put(img, x + dx, y, BOLT[1])
    for x, y in core:
        put(img, x, y, BOLT[0])
    return img


def sigil():
    """The Soul-Bond's glow, gold fading out to nothing, and its mark: two rings linked, white-gold,
    with a bright point where they cross."""
    n = SIGIL_CELL
    strip = new_image(n * 2, n)
    c = (n - 1) / 2
    for y in range(n):
        for x in range(n):
            d = math.hypot(x + 0.5 - n / 2, y + 0.5 - n / 2) / (n / 2)
            if d < 1:
                put(strip, x, y, SIGIL_GOLD[1] if d < 0.35 else SIGIL_GOLD[2], int(230 * (1 - d) ** 1.6))
    for y in range(n):
        for x in range(n):
            for ox in (-2.2, 2.2):
                r = math.hypot(x - c - ox, y - c)
                if 2.6 <= r <= 3.8:
                    put(strip, n + x, y, SIGIL_GOLD[1] if y < c else SIGIL_GOLD[2])
    for dx, dy in ((0, -1), (-1, 0), (0, 0), (1, 0), (0, 1)):
        put(strip, n + int(c) + dx, int(c) + dy, SIGIL_GOLD[0])
    return strip


def glint():
    """A glint on water, three frames: a speck, a plus, a little star. The game tints them."""
    n = GLINT_CELL
    strip = new_image(n * 3, n)
    m = n // 2
    shapes = (((0, 0),), ((0, 0), (1, 0), (-1, 0), (0, 1), (0, -1)),
              ((0, 0), (1, 0), (-1, 0), (0, 1), (0, -1), (2, 0), (-2, 0), (0, 2), (0, -2)))
    for f, shape in enumerate(shapes):
        for dx, dy in shape:
            put(strip, f * n + m + dx, m + dy, (255, 255, 255), 255 if abs(dx) + abs(dy) < 2 else 150)
    return strip


def swinging_lantern():
    """The lantern off my props sheet on a short chain, at five points of a swing: each row of it
    pushed sideways in proportion to how far it is under the hook, which at this size reads as the
    whole thing tilting. Its warm haze goes with it."""
    art = lantern(LANTERN_WIDTH)
    cw, ch = LANTERN_CELL
    strip = new_image(cw * len(LANTERN_SWING), ch)
    chain = 4
    for f, swing in enumerate(LANTERN_SWING):
        frame = new_image(cw, ch)
        hang = chain + len(art)
        mid = cw // 2
        for y in range(ch):
            for x in range(cw):
                shift = swing * y / hang
                d = math.hypot(x + 0.5 - mid - shift, (y - chain - len(art) * 0.55) * 0.75) / 9
                if d < 1:
                    put(frame, x, y, LAMPLIGHT, int(150 * (1 - d) ** 2))
        for y in range(chain):
            put(frame, int(round(mid + swing * y / hang)), y, PLANK[1] if y % 2 else PLANK[3])
        for y, line in enumerate(art):
            shift = int(round(swing * (chain + y) / hang))
            for x, p in enumerate(line):
                if p[3]:
                    put(frame, mid - len(art[0]) // 2 + x + shift, chain + y, p)
        over(strip, frame, f * cw, 0)
    return strip


def effects():
    write_png(os.path.join(OUT, "Paintings", "weather.png"), weather())
    bolts = new_image(BOLT_CELL[0] * 2, BOLT_CELL[1])
    for i in range(2):
        over(bolts, bolt(140 + i * 7), i * BOLT_CELL[0], 0)
    write_png(os.path.join(OUT, "Paintings", "bolts.png"), bolts)
    write_png(os.path.join(OUT, "Paintings", "sigil.png"), sigil())
    write_png(os.path.join(OUT, "Paintings", "glint.png"), glint())
    write_png(os.path.join(OUT, "Paintings", "lantern.png"), swinging_lantern())


if __name__ == "__main__":
    wanted = sys.argv[1:] or ["dawn", "moon", "pier", "effects"]
    print("Painting the story's paintings for Legend Quest...")
    if "dawn" in wanted:
        questeria_dawn()
    if "moon" in wanted:
        red_moon()
    if "pier" in wanted:
        pier_memory()
    if "effects" in wanted:
        effects()
    print("Done.")
