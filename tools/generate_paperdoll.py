"""
The paperdoll: the youth off my townsperson sheet, taken apart so anyone can be made out of him.

    python tools/generate_paperdoll.py [sheet ...]

With sheet names (hat-crown, say), only those are written, for every body, and everything else on
disk is left as it is.

He's the best-drawn walker I have (the biggest on his sheet, so the cleanest when he's sampled
down), so he's the body everyone in the party is made from. Every pixel of him gets sorted into what
it's part of: skin, hair, eyes, shirt, the vest over it, the breeches, or something that never
changes colour (his outline, his belt, his boots, the shine in his eyes). Each recolourable part gets
cut down to a few shades, dark to light, and those shades are the keys the palette swap looks for,
the same as The Black Box's CharacterPalettes. Then the hair comes off him onto a layer of its own,
and the other hair, the faces, the beards, the skirt and the cape get drawn onto his frames from
where his head, eyes, chin, waist and legs turn out to be in each one. So does what the party
wears: nine body armours, three helms, gloves and gauntlets, and two kinds of boots, in keys of
their own for metal and leather so Paperdoll can tint them. What they hold (thirteen weapons and
two shields) is drawn on each body's own frames instead, from wherever the hand holding it is.

Every sheet is his layout: 32x48 frames, a row for each way he faces (down, left, right, up), and
ten across: standing, four steps, and the five a fight uses (winding up, striking, casting, hurt
and lying downed). He was only drawn standing and walking, so the fights' frames are his standing
frame cut up and posed, by one map that moves every layer the same way. character-palettes.png has
every preset a part can be swapped for, a row each, dark to light; the first row of each block is
the keys themselves, which is him as drawn. gear-palettes.png is the same for metal and leather.

Only needs CPython. There's nothing random in here, so a rerun writes the same bytes.
"""

import colorsys
import math
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from pixels import OUT, new_image, write_png  # noqa: E402
from cut_supplied import FRAME_H, FRAME_W, PEOPLE_COLUMNS, SOLES, frame_of, grid  # noqa: E402

DOWN, LEFT, RIGHT, UP = range(4)

#: The parts that get recoloured, and how many shades each is cut down to. The order is the order
#: of the blocks in character-palettes.png, and CharacterPalettes reads them in the same order.
SHADES = (
    ("skin", 5),
    ("hair", 5),
    ("eyes", 2),
    ("shirt", 4),
    ("top", 5),
    ("bottoms", 4),
    ("cloak", 5),
)

#: The presets after the first, as a shadow, a middle and a light colour; every key shade lands
#: between them by how light it is. The names are for me: CharacterLook has the ones the screen shows.
PRESETS = {
    "skin": (
        ((178, 128, 142), (242, 206, 192), (255, 236, 226)),    # porcelain
        ((170, 102, 86), (224, 160, 116), (248, 204, 154)),     # golden
        ((136, 80, 70), (192, 128, 92), (226, 170, 124)),       # tan
        ((94, 54, 60), (150, 94, 72), (196, 138, 100)),         # brown
        ((58, 34, 46), (104, 64, 60), (150, 102, 82)),          # deep
    ),
    "hair": (
        ((26, 22, 40), (52, 48, 72), (100, 94, 128)),           # black
        ((150, 100, 48), (220, 174, 92), (252, 234, 164)),      # gold
        ((108, 38, 38), (178, 78, 50), (230, 134, 80)),         # copper
        ((116, 112, 140), (190, 188, 208), (244, 244, 252)),    # silver
        ((30, 36, 76), (62, 78, 140), (116, 140, 200)),         # midnight
        ((136, 58, 100), (218, 118, 160), (250, 194, 218)),     # rose
        ((86, 68, 128), (160, 130, 202), (224, 204, 246)),      # lavender
    ),
    "eyes": (
        ((60, 34, 26), (120, 74, 46), (120, 74, 46)),           # brown
        ((24, 64, 40), (54, 130, 78), (54, 130, 78)),           # green
        ((96, 56, 14), (196, 130, 40), (196, 130, 40)),         # amber
        ((52, 58, 70), (124, 134, 150), (124, 134, 150)),       # grey
        ((62, 30, 92), (136, 82, 186), (136, 82, 186)),         # violet
    ),
    "shirt": (
        ((150, 150, 170), (226, 228, 236), (252, 252, 255)),    # white
        ((110, 130, 170), (174, 200, 232), (222, 236, 252)),    # sky
        ((164, 112, 130), (232, 182, 192), (252, 222, 228)),    # blush
        ((96, 84, 70), (150, 132, 108), (196, 180, 150)),       # oat
    ),
    "top": (
        ((24, 70, 76), (46, 128, 128), (104, 190, 180)),        # teal
        ((30, 62, 40), (58, 112, 66), (110, 164, 104)),         # forest
        ((80, 22, 40), (150, 40, 58), (210, 86, 92)),           # crimson
        ((54, 32, 76), (106, 64, 136), (164, 118, 196)),        # plum
        ((110, 70, 30), (186, 130, 56), (236, 192, 106)),       # ochre
        ((30, 30, 44), (58, 58, 78), (104, 104, 128)),          # charcoal
        ((136, 54, 92), (214, 112, 148), (246, 176, 196)),      # rose
    ),
    "bottoms": (
        ((32, 32, 44), (60, 60, 76), (96, 96, 114)),            # charcoal
        ((26, 32, 64), (46, 60, 110), (82, 100, 156)),          # navy
        ((50, 58, 34), (90, 100, 58), (134, 144, 92)),          # olive
        ((116, 92, 64), (178, 148, 106), (220, 196, 150)),      # sand
        ((80, 26, 44), (136, 46, 66), (186, 84, 98)),           # wine
    ),
    "cloak": (
        ((70, 22, 40), (140, 36, 56), (204, 80, 86)),           # crimson
        ((24, 62, 58), (40, 112, 104), (96, 172, 158)),         # teal
        ((30, 30, 44), (56, 56, 76), (100, 100, 126)),          # charcoal
        ((86, 64, 30), (152, 112, 52), (214, 172, 98)),         # ochre
        ((70, 50, 104), (128, 96, 172), (192, 166, 226)),       # lavender
    ),
}

#: The cloak's own keys. Nothing on the youth is a cloak, so the cape is painted in these, and
#: they're the first cloak preset: a deep woad blue with a little purple in its shadow.
CLOAK_KEYS = ((30, 30, 70), (44, 50, 104), (60, 74, 138), (86, 104, 170), (124, 146, 204))

#: What stays the colour it's drawn: the outline, which the new hair and beards are edged in too.
OUTLINE = (52, 30, 44)

#: Gold, for a clasp.
GOLD = ((150, 98, 36), (232, 186, 82))


# --------------------------------------------------------------------------- #
# Sorting him into parts
# --------------------------------------------------------------------------- #

def hsv(px):
    h, s, v = colorsys.rgb_to_hsv(px[0] / 255, px[1] / 255, px[2] / 255)
    return h * 360, s, v


def warm(hue):
    return hue < 50 or hue > 330


def bluish(hue):
    return 180 <= hue <= 270


class Frame:
    """One frame of him: its pixels, what each one is part of, and where his head, neck, waist,
    face and eyes are in it."""

    def __init__(self, img, facing):
        self.img = img
        self.facing = facing
        self.drawn = {(x, y) for y in range(FRAME_H) for x in range(FRAME_W) if img[y][x][3] > 127}
        self.top = min(y for _, y in self.drawn)
        self.bottom = max(y for _, y in self.drawn)
        colour = {p: hsv(img[p[1]][p[0]]) for p in self.drawn}

        # The neck is the first row down from the hair with collar in it: vest blue or shirt cream.
        def collar(p):
            h, s, v = colour[p]
            return bluish(h) and s > 0.15 and v > 0.3 or s < 0.3 and v > 0.7
        self.neck = next(y for y in range(self.top + 8, self.bottom)
                         if sum(1 for x in range(FRAME_W) if (x, y) in colour and collar((x, y))) >= 3)

        # The legs go up from the boots until a row has shirt or vest in it, which is the hem.
        self.waist = self.bottom - 7
        while self.waist > self.neck + 4 and sum(
                1 for x in range(FRAME_W) if (x, self.waist) in colour and collar((x, self.waist))) < 2:
            self.waist -= 1

        part = {}
        for (x, y), (h, s, v) in colour.items():
            if y < self.neck:
                if bluish(h) and s > 0.2 and v < 0.6:
                    part[(x, y)] = "eyes?"
                elif v < 0.3:
                    part[(x, y)] = "outline"
                elif s < 0.15 and v > 0.85:
                    part[(x, y)] = "shine"
                elif warm(h) and s >= 0.33 and v >= 0.66:
                    part[(x, y)] = "skin"
                else:
                    part[(x, y)] = "hair"
            elif v < 0.3:
                part[(x, y)] = "outline"
            elif bluish(h) and s > 0.12:
                part[(x, y)] = "top"
            elif warm(h) and s >= 0.42 and v >= 0.62 and y <= self.waist + 1:
                part[(x, y)] = "skin"
            elif s < 0.42 and v >= 0.55:
                part[(x, y)] = "shirt"
            elif self.bottom - y <= 3:
                part[(x, y)] = "boots"
            elif y <= self.waist + 1:
                part[(x, y)] = "leather"
            else:
                part[(x, y)] = "bottoms"

        # What looked like an eye is one if it's in the face: skin beside it and nothing clear round
        # it. The rest is the bluish dark of the outline where the hair meets the face.
        for (x, y), p in list(part.items()):
            if p != "eyes?":
                continue
            around = [(x + dx, y + dy) for dx in (-1, 0, 1) for dy in (-1, 0, 1) if dx or dy]
            beside = any(part.get(n) == "skin" for n in ((x - 1, y), (x + 1, y)))
            part[(x, y)] = "eyes" if beside and all(n in part for n in around) else "outline"
        self.part = part

        face = [p for p, k in part.items() if k == "skin" and p[1] < self.neck]
        self.face = face
        self.chin = max((y for _, y in face), default=self.neck - 1)
        # Eyes are well above the chin, and there aren't any on the back of his head. Anything
        # else that came out eye-coloured is the shadow under his chin or by his ear.
        found = clusters([p for p, k in part.items() if k in ("eyes", "shine")])
        self.eyes = [] if facing == UP else [e for e in found if min(y for _, y in e) <= self.chin - 3]
        for e in found:
            if e not in self.eyes:
                for p in e:
                    part[p] = "outline" if part[p] == "eyes" else "skin"

    def row_span(self, y, parts=None):
        """The leftmost and rightmost drawn pixels on a row (of some parts only, if it's told which)."""
        xs = [x for x in range(FRAME_W) if (x, y) in self.part and (parts is None or self.part[(x, y)] in parts)]
        return (min(xs), max(xs)) if xs else None


def clusters(points):
    """Points grouped into the patches they touch each other in, left to right."""
    left, found = set(points), []
    while left:
        stack, patch = [left.pop()], []
        while stack:
            x, y = stack.pop()
            patch.append((x, y))
            for n in ((x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1)):
                if n in left:
                    left.remove(n)
                    stack.append(n)
        found.append(sorted(patch))
    return sorted(found, key=lambda c: min(x for x, _ in c))


def youth():
    """His twenty frames, a row of five for each way he faces."""
    cells = grid("townsperson")
    return [[Frame(frame_of(cells[r][c]), r) for c in range(PEOPLE_COLUMNS)] for r in range(4)]


# --------------------------------------------------------------------------- #
# The keys
# --------------------------------------------------------------------------- #

def luma(px):
    return 0.299 * px[0] + 0.587 * px[1] + 0.114 * px[2]


def shade_levels(values, count):
    """Splits a lot of brightnesses into `count` groups (one-dimensional k-means, started on the
    quantiles) and returns the middle of each group, darkest first."""
    values = sorted(values)
    centres = [values[min(len(values) - 1, int((i + 0.5) * len(values) / count))] for i in range(count)]
    for _ in range(20):
        groups = [[] for _ in centres]
        for v in values:
            groups[min(range(len(centres)), key=lambda i: abs(v - centres[i]))].append(v)
        centres = [sum(g) / len(g) if g else c for g, c in zip(groups, centres)]
    return sorted(centres)


def make_keys(frames):
    """The key shades for every recolourable part on him: the average colour of each group of his
    pixels, darkest first. Also returns which shade each of his pixels is."""
    keys, shade_of = {}, {}
    fixed = {tuple(f.img[y][x][:3]) for row in frames for f in row for (x, y), k in f.part.items()
             if k not in dict(SHADES)}
    for name, count in SHADES:
        pixels = [(f, p) for row in frames for f in row for p, k in f.part.items() if k == name]
        if not pixels:
            continue
        levels = shade_levels([luma(f.img[p[1]][p[0]]) for f, p in pixels], count)
        sums = [[0, 0, 0, 0] for _ in levels]
        for f, p in pixels:
            px = f.img[p[1]][p[0]]
            i = min(range(count), key=lambda i: abs(luma(px) - levels[i]))
            shade_of[(id(f), p)] = i
            for c in range(3):
                sums[i][c] += px[c]
            sums[i][3] += 1
        ramp = []
        for s in sums:
            colour = tuple(round(s[c] / max(1, s[3])) for c in range(3))
            # A key has to be a colour nothing else on the sheet is, or the swap would catch it too.
            while colour in fixed or colour in ramp or any(colour in r for r in keys.values()):
                colour = (colour[0], colour[1], min(255, colour[2] + 1))
            ramp.append(colour)
        keys[name] = tuple(ramp)
    keys["cloak"] = CLOAK_KEYS
    return keys, shade_of


def preset_ramp(keys, anchors):
    """A preset's shades, one for each key: each key's place between the darkest and lightest key
    picks a colour along shadow, middle and light."""
    ls = [luma(k) for k in keys]
    lo, hi = min(ls), max(ls)
    out = []
    for l in ls:
        t = 0.5 if hi == lo else (l - lo) / (hi - lo)
        a, b, u = (anchors[0], anchors[1], t * 2) if t < 0.5 else (anchors[1], anchors[2], t * 2 - 1)
        out.append(tuple(round(a[c] + (b[c] - a[c]) * u) for c in range(3)))
    return tuple(out)


# --------------------------------------------------------------------------- #
# Layers
# --------------------------------------------------------------------------- #

class Layer:
    """A sheet being built: a frame's worth of pixels for each of his standing and walking frames.
    The fights' frames are posed from the standing one when it's saved."""

    def __init__(self):
        self.frames = [[new_image(FRAME_W, FRAME_H) for _ in range(PEOPLE_COLUMNS)] for _ in range(4)]

    def put(self, r, c, x, y, rgb):
        if 0 <= x < FRAME_W and 0 <= y < FRAME_H:
            self.frames[r][c][y][x] = [rgb[0], rgb[1], rgb[2], 255]

    def get(self, r, c, x, y):
        if 0 <= x < FRAME_W and 0 <= y < FRAME_H and self.frames[r][c][y][x][3]:
            return tuple(self.frames[r][c][y][x][:3])
        return None

    def clear(self, r, c, x, y):
        if 0 <= x < FRAME_W and 0 <= y < FRAME_H:
            self.frames[r][c][y][x] = [0, 0, 0, 0]



def hair_of(f):
    """Which of a frame's pixels are his hair: the hair itself, and the outline round it that
    doesn't touch his face."""
    mine = set()
    for (x, y), k in f.part.items():
        if y >= f.neck:
            continue
        if k == "hair":
            mine.add((x, y))
        elif k == "outline":
            near = [f.part.get(n) for n in ((x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1))]
            if not any(n in ("skin", "eyes", "shine") for n in near):
                mine.add((x, y))
    return mine


def keyed(f, p, keys, shade_of):
    """A pixel of him in its key colour, or as drawn if it's part of something that never changes."""
    k = f.part[p]
    if k in keys and (id(f), p) in shade_of:
        return keys[k][shade_of[(id(f), p)]]
    return tuple(f.img[p[1]][p[0]][:3])


def body_and_hair(frames, keys, shade_of):
    """Him without his hair, and his hair on its own."""
    body, hair = Layer(), Layer()
    for r, row in enumerate(frames):
        for c, f in enumerate(row):
            mine = hair_of(f)
            for p in f.part:
                (hair if p in mine else body).put(r, c, p[0], p[1], keyed(f, p, keys, shade_of))
    return body, hair


def head_span(f, hair, r, c, y):
    """The leftmost and rightmost pixels of his head (hair or face) on a row."""
    xs = [x for x in range(FRAME_W) if hair.get(r, c, x, y) is not None or f.part.get((x, y)) in ("skin", "eyes", "shine")]
    return (min(xs), max(xs)) if xs else None


def eye_row(f):
    """The row his eyes are on (their bottom), or a guess at it from his chin for the back of his head."""
    if f.eyes:
        return max(y for e in f.eyes for _, y in e)
    return f.chin - 3


def strand(layer, r, c, x, y0, y1, ramp, light):
    """One lock of hair down a column, lighter near the top, from y0 to y1."""
    for y in range(y0, y1 + 1):
        t = (y - y0) / max(1, y1 - y0)
        i = max(0, min(len(ramp) - 1, round(light - t * 1.5)))
        layer.put(r, c, x, y, ramp[i])


def cropped(f, hair, r, c):
    """Short: everything outside a cap round his skull taken off, then edged again, so the spikes on
    his crown and the tufts over his ears go."""
    cy = eye_row(f) - 1
    span = head_span(f, hair, r, c, cy)
    cx, rx = (span[0] + span[1]) / 2, (span[1] - span[0]) / 2 + 0.5
    ry = cy - f.top - 1.5
    for y in range(cy):
        for x in range(FRAME_W):
            if hair.get(r, c, x, y) is not None and ((x - cx) / rx) ** 2 + ((y - cy) / ry) ** 2 > 1:
                hair.clear(r, c, x, y)
    for y in range(cy):
        for x in range(FRAME_W):
            if hair.get(r, c, x, y) is not None and any(
                    hair.get(r, c, *n) is None and f.part.get(n) not in ("skin", "eyes", "shine")
                    for n in ((x - 1, y), (x + 1, y), (x, y - 1))):
                hair.put(r, c, x, y, OUTLINE)


def long_hair(f, hair, r, c, ramp):
    """Long: down past the shoulders. Framing the face from the front, down the back of the head
    from the side, and over the shoulders from behind."""
    eyes = eye_row(f)
    end = f.neck + 6
    if r == DOWN:
        span = head_span(f, hair, r, c, eyes)
        for side, x0 in ((0, span[0]), (1, span[1])):
            xs = (x0 - 1, x0, x0 + 1) if side == 0 else (x0 + 1, x0, x0 - 1)
            for i, x in enumerate(xs):
                bottom = end - (1 if i == 2 else 0) - ((c + i) % 2)
                if i == 0:
                    for y in range(eyes - 2, bottom + 1):
                        hair.put(r, c, x, y, OUTLINE)
                else:
                    strand(hair, r, c, x, eyes - 2, bottom, ramp, 3 if side == 0 else 2)
            hair.put(r, c, xs[1], end + 1 - (c % 2), OUTLINE)
            hair.put(r, c, xs[2], end - (c + 1) % 2, OUTLINE)
    elif r in (LEFT, RIGHT):
        back = 1 if r == LEFT else -1
        top_y = eyes - 3
        span = head_span(f, hair, r, c, top_y)
        edge = span[1] if r == LEFT else span[0]
        width = 5
        for y in range(top_y, end + 1):
            w = width if y < end - 1 else width - 1 - (end - 1 - y < 0)
            for i in range(w):
                x = edge + back - i * back
                if i == 0:
                    hair.put(r, c, x, y, OUTLINE)
                else:
                    light = 3 if (i % 2 and back < 0) or (not i % 2 and back > 0) else 2
                    strand(hair, r, c, x, y, y, ramp, light - (y - top_y) / (end - top_y) * 1.5)
        for i in range(width):
            hair.put(r, c, edge + back - i * back, end + 1 - (i + c) % 2, OUTLINE)
    else:
        top_y = f.neck - 3
        span = head_span(f, hair, r, c, top_y)
        for y in range(top_y, end + 1):
            inset = max(0, (y - top_y) // 4)
            x0, x1 = span[0] + inset, span[1] - inset
            for x in range(x0, x1 + 1):
                if x in (x0, x1):
                    hair.put(r, c, x, y, OUTLINE)
                else:
                    light = 3 if (x - x0) % 3 == 1 else 2 if (x - x0) % 3 == 2 else 1
                    if x > (x0 + x1) // 2 + 1:
                        light -= 1
                    hair.put(r, c, x, y, ramp[max(0, light)])
        x0, x1 = span[0] + (end - top_y) // 4, span[1] - (end - top_y) // 4
        for x in range(x0, x1 + 1):
            hair.put(r, c, x, end + 1 - (x + c) % 2, OUTLINE)


def hang(f, hair, r, c, ramp, braided):
    """A tail or a braid hanging down the back of the head: down the middle from behind, and off the
    back of the head from the side. A braid's twists are pairs of rows shaded opposite ways."""
    eyes = eye_row(f)
    sway = (0, 1, 0, -1, 0)[c]
    if r == UP:
        span = head_span(f, hair, r, c, f.neck - 2)
        cx, start, end = (span[0] + span[1]) // 2, f.neck - 2, f.neck + 8
        for y in range(start, end):
            x = cx + (sway if y > f.neck + 3 else 0)
            twist = braided and (y // 2) % 2
            hair.put(r, c, x - 2, y, OUTLINE)
            hair.put(r, c, x - 1, y, ramp[2 if twist else 3])
            hair.put(r, c, x, y, ramp[2])
            hair.put(r, c, x + 1, y, ramp[3 if twist else 1])
            hair.put(r, c, x + 2, y, OUTLINE)
        for x in range(-2, 3):
            hair.put(r, c, cx + x, f.neck - 1, OUTLINE)
            hair.put(r, c, cx + x + sway, end, OUTLINE)
        return
    back = 1 if r == LEFT else -1
    span = head_span(f, hair, r, c, eyes - 2)
    edge = span[1] if r == LEFT else span[0]
    end = f.neck + 5
    for y in range(eyes - 2, end):
        x = edge + (1 if y >= eyes else 0) * back + (sway * back if y > f.neck + 1 else 0)
        twist = braided and (y // 2) % 2
        hair.put(r, c, x - back, y, ramp[2])
        hair.put(r, c, x, y, ramp[1 if twist else 3] if back > 0 else ramp[3 if twist else 1])
        hair.put(r, c, x + back, y, OUTLINE)
        if y == eyes:
            hair.put(r, c, x - back, y, OUTLINE)
            hair.put(r, c, x, y, OUTLINE)
    x = edge + back + sway * back
    hair.put(r, c, x, end, OUTLINE)
    hair.put(r, c, x - back, end, OUTLINE)


def tail(f, hair, r, c, ramp):
    """Tied back: a tail tied at the nape. You can't see it from the front."""
    if r != DOWN:
        hang(f, hair, r, c, ramp, False)


def braid(f, hair, r, c, ramp):
    """One braid: over his shoulder from the front, down his back otherwise."""
    if r != DOWN:
        hang(f, hair, r, c, ramp, True)
        return
    eyes = eye_row(f)
    span = head_span(f, hair, r, c, eyes)
    x0, end = span[0], f.neck + 7
    for y in range(eyes - 1, end):
        twist = (y // 2) % 2
        hair.put(r, c, x0 - 1, y, OUTLINE)
        hair.put(r, c, x0, y, ramp[2 if twist else 3])
        hair.put(r, c, x0 + 1, y, ramp[3 if twist else 1])
        hair.put(r, c, x0 + 2, y, OUTLINE)
    for x in range(x0 - 1, x0 + 3):
        hair.put(r, c, x, end, OUTLINE)


HAIR_STYLES = ("tousled", "cropped", "long", "tail", "braid")


def hair_styles(frames, base, keys):
    """Every hair style's sheet, from his own hair. They're all his hair underneath, so each one
    still covers his crown wherever the body sheet hasn't got one."""
    ramp = keys["hair"]
    for style in HAIR_STYLES:
        layer = Layer()
        for r in range(4):
            for c in range(PEOPLE_COLUMNS):
                layer.frames[r][c] = [[list(px) for px in line] for line in base.frames[r][c]]
                f = frames[r][c]
                if style == "cropped":
                    cropped(f, layer, r, c)
                elif style == "long":
                    long_hair(f, layer, r, c, ramp)
                elif style == "tail":
                    tail(f, layer, r, c, ramp)
                elif style == "braid":
                    braid(f, layer, r, c, ramp)
        keep("hair-" + style, layer)


# --------------------------------------------------------------------------- #
# Faces and beards
# --------------------------------------------------------------------------- #

FACES = ("bright", "gentle", "lashed", "stern", "freckled", "scarred")


def skin_at(f, keys, shade_of, x, y, darker=0):
    """The key skin shade at a pixel of his face, or a shade darker."""
    if f.part.get((x, y)) == "skin":
        i = shade_of.get((id(f), (x, y)), 3)
    else:
        i = 3
    return keys["skin"][max(0, i - darker)]


def faces(frames, keys, shade_of):
    """The faces after his own, each a layer that only has the pixels it changes. Nothing changes
    from behind."""
    for face in FACES[1:]:
        layer = Layer()
        for r in range(3):
            for c in range(PEOPLE_COLUMNS):
                f = frames[r][c]
                for eye in f.eyes:
                    xs = [x for x, _ in eye]
                    ys = [y for _, y in eye]
                    x0, x1, y0, y1 = min(xs), max(xs), min(ys), max(ys)
                    outer = x0 - 1 if (x0 + x1) / 2 < 16 else x1 + 1
                    if r == LEFT:
                        outer = x1 + 1
                    elif r == RIGHT:
                        outer = x0 - 1
                    if face == "gentle":
                        # Softer: the top of each eye goes, so they're a pixel shorter.
                        for x in range(x0, x1 + 1):
                            layer.put(r, c, x, y0, skin_at(f, keys, shade_of, x, y0 - 1))
                    elif face == "lashed":
                        layer.put(r, c, outer, y0, OUTLINE)
                        layer.put(r, c, outer, y0 - 1 if r == DOWN else y0, OUTLINE)
                    elif face == "stern":
                        for x in range(x0, x1 + 1):
                            if f.part.get((x, y0 - 1)) == "skin":
                                layer.put(r, c, x, y0 - 1, OUTLINE)
                            layer.put(r, c, x, y0, keys["eyes"][0])
                    elif face == "freckled":
                        for x, dy in ((x0 - 1, 2), (x1, 2), (x0 + 1, 3)):
                            if f.part.get((x, y1 + dy)) == "skin":
                                layer.put(r, c, x, y1 + dy, skin_at(f, keys, shade_of, x, y1 + dy, 2))
                    elif face == "scarred":
                        if eye is not f.eyes[0]:
                            continue
                        for i in range(5):
                            x, y = x0 + 2 - i // 2, y0 - 2 + i
                            if f.part.get((x, y)) == "skin":
                                layer.put(r, c, x, y, skin_at(f, keys, shade_of, x, y, 2))
        keep("face-" + face, layer)


BEARDS = ("stubble", "moustache", "beard", "full")


def beards(frames, keys, shade_of):
    """The beards, in his hair's keys so they come out whatever colour his hair is. His face is only
    a couple of rows below his eyes at this size, so a beard is mostly the row of his chin and a
    row or two hanging under it."""
    ramp = keys["hair"]
    for beard in BEARDS:
        layer = Layer()
        for r in (DOWN, LEFT, RIGHT):
            for c in range(PEOPLE_COLUMNS):
                f = frames[r][c]
                eyes = eye_row(f)
                jaw = [(x, y) for x, y in f.face if y > eyes]
                if not jaw or not f.eyes:
                    continue
                if beard == "stubble":
                    for x, y in jaw:
                        if (x + y) % 2 == 0:
                            layer.put(r, c, x, y, skin_at(f, keys, shade_of, x, y, 2))
                    continue
                if r == DOWN:
                    mouth = eyes + 1
                    xs = [x for e in f.eyes for x, _ in e]
                    lip = range(min(xs) + 1, max(xs))
                    near = lambda x: True
                else:
                    mouth = eyes + 2
                    row = [x for x, y in f.face if y == mouth] or [x for x, _ in jaw]
                    front = min(row) if r == LEFT else max(row)
                    step = 1 if r == LEFT else -1
                    lip = range(front, front + 3 * step, step)
                    near = lambda x, front=front, step=step: (x - front) * step <= 4
                for x in lip:
                    if (x, mouth) in f.face:
                        layer.put(r, c, x, mouth, ramp[1 if beard == "moustache" else 2])
                if beard == "moustache":
                    continue
                for x, y in jaw:
                    if y > mouth and near(x):
                        layer.put(r, c, x, y, ramp[2 if (x + y) % 3 else 3])
                    elif beard == "full" and y >= mouth - 1 and near(x) and (x - 1, y) not in f.face:
                        layer.put(r, c, x, y, ramp[2])
                    elif beard == "full" and y >= mouth - 1 and near(x) and (x + 1, y) not in f.face:
                        layer.put(r, c, x, y, ramp[1])
                chin = sorted(x for x, y in f.face if y == f.chin and near(x))
                for i in range(1, 2 if beard == "beard" else 3):
                    for x in chin[i:len(chin) - i] if r == DOWN else chin[:len(chin) - i]:
                        layer.put(r, c, x, f.chin + i, ramp[1 if x > sum(chin) / len(chin) else 2])
                drawn = {(x, y) for y in range(FRAME_H) for x in range(FRAME_W) if layer.get(r, c, x, y)}
                for x, y in drawn:
                    if (x, y + 1) not in drawn and f.part.get((x, y + 1)) not in ("skin",):
                        layer.put(r, c, x, y + 1, OUTLINE)
        keep("beard-" + beard, layer)


# --------------------------------------------------------------------------- #
# The skirt and the cape
# --------------------------------------------------------------------------- #

def drape(f, top_y, hem):
    """The cells of something hanging from his waist to a hem: as wide as his waist at the top,
    flaring a pixel every few rows below that as far as his stride, and never over his hands. Each
    is a cell and the ends of its row."""
    waist = f.row_span(top_y, ("top", "shirt", "leather", "outline", "bottoms"))
    legs = [f.row_span(y, ("bottoms", "boots", "outline", "shirt")) for y in range(f.waist + 1, f.bottom - 1)]
    legs = [s for s in legs if s]
    lo, hi = min(s[0] for s in legs), max(s[1] for s in legs)
    for y in range(top_y, hem + 1):
        flare = (y - top_y) // 3
        x0 = max(lo - 1, waist[0] - flare) if y > top_y + 2 else waist[0]
        x1 = min(hi + 1, waist[1] + flare) if y > top_y + 2 else waist[1]
        x0, x1 = min(x0, waist[0]), max(x1, waist[1])
        for x in range(x0, x1 + 1):
            if f.part.get((x, y)) != "skin":
                yield x, y, x0, x1


def skirt(frames, keys):
    """A long skirt in the breeches' keys, from the waist to above the boots, flaring a pixel every
    few rows and as wide as his stride. It goes over his legs and under his hands."""
    ramp = keys["bottoms"]
    layer = Layer()
    for r in range(4):
        for c in range(PEOPLE_COLUMNS):
            f = frames[r][c]
            top_y, hem = f.waist - 1, f.bottom - 3
            for x, y, x0, x1 in drape(f, top_y, hem):
                if x in (x0, x1) or y == hem:
                    colour = OUTLINE
                elif y == top_y:
                    colour = ramp[0]
                else:
                    fold = (x - x0) % 3 == 0
                    lit = x < (x0 + x1) / 2
                    colour = ramp[(2 if lit else 1) - fold + (1 if lit and y < top_y + 3 else 0)]
                    if y == hem - 1:
                        colour = ramp[1 if lit else 0]
                layer.put(r, c, x, y, colour)
    keep("skirt", layer)


def cape(frames, keys):
    """A cape off the shoulders to the backs of the knees, in the cloak's keys. It's two sheets:
    the back, drawn behind him, and the front, drawn over him: the shoulders from the front, and
    all of it from behind."""
    ramp = CLOAK_KEYS
    back, front = Layer(), Layer()
    for r in range(4):
        for c in range(PEOPLE_COLUMNS):
            f = frames[r][c]
            shoulders = f.row_span(f.neck + 1, ("top", "shirt", "outline"))
            hem = f.bottom - 6 + (c % 2)
            if r in (DOWN, UP):
                layer = back if r == DOWN else front
                for y in range(f.neck, hem + 1):
                    spread = (y - f.neck) // 4
                    x0, x1 = shoulders[0] - spread, shoulders[1] + spread
                    for x in range(x0, x1 + 1):
                        if x in (x0, x1) or y == hem or (y == f.neck and r == UP):
                            colour = OUTLINE
                        else:
                            fold = (x - x0) % 4 == 2
                            lit = x < (x0 + x1) / 2
                            colour = ramp[(3 if lit else 2) - (1 if fold else 0) - (1 if y > hem - 2 else 0)]
                        layer.put(r, c, x, y, colour)
                if r == DOWN:
                    # The drape over each shoulder, and the clasp at the collar.
                    for x in range(shoulders[0], shoulders[0] + 3):
                        front.put(r, c, x, f.neck, ramp[3])
                        front.put(r, c, x, f.neck + 1, ramp[2])
                    for x in range(shoulders[1] - 2, shoulders[1] + 1):
                        front.put(r, c, x, f.neck, ramp[2])
                        front.put(r, c, x, f.neck + 1, ramp[1])
                    front.put(r, c, shoulders[0], f.neck + 2, OUTLINE)
                    front.put(r, c, shoulders[1], f.neck + 2, OUTLINE)
                    mid = (shoulders[0] + shoulders[1]) // 2
                    front.put(r, c, mid, f.neck, GOLD[1])
                    front.put(r, c, mid + 1, f.neck, GOLD[0])
            else:
                away = 1 if r == LEFT else -1
                edge = shoulders[1] if r == LEFT else shoulders[0]
                sway = (0, 1, 0, 1, 0)[c]
                for y in range(f.neck, hem + 1):
                    reach = 2 + (y - f.neck) // 3 + (sway if y > f.neck + 6 else 0)
                    body = f.row_span(y, None)
                    near = body[1] if r == LEFT else body[0]
                    near = edge if body is None else near
                    for i in range(reach + 4):
                        x = edge - 3 * away + i * away
                        if (x - edge) * away > reach:
                            break
                        last = (x - edge) * away == reach
                        if last or y == hem:
                            colour = OUTLINE
                        else:
                            colour = ramp[2 if (x + y) % 4 else 1]
                        back.put(r, c, x, y, colour)
                for i in range(3):
                    front.put(r, c, edge - i * away, f.neck, ramp[2])
                front.put(r, c, edge + away, f.neck, OUTLINE)
    keep("cape-back", back)
    keep("cape-front", front)


# --------------------------------------------------------------------------- #
# The peoples: ears, horns and tails, and a bald head
# --------------------------------------------------------------------------- #

#: Horn, from dark to light: old bone going ivory at the tips. Horns never change colour.
HORN = ((62, 44, 56), (110, 86, 92), (166, 140, 130), (214, 196, 176))

#: An Emberkin's tail: crimson, the tip burning from red through orange to yellow.
EMBER_TAIL = ((70, 22, 40), (128, 34, 48), (184, 58, 56))
EMBER_TIP = ((222, 96, 40), (248, 160, 56), (255, 226, 120))

#: Iron and steel for the Watch's helm, and the leather and gold of a captain's tricorn. Neither changes colour.
STEEL = ((56, 60, 78), (98, 106, 128), (150, 160, 180), (206, 214, 226))
TRICORN = ((24, 20, 30), (44, 36, 46), (70, 58, 66))
TRIM = ((176, 128, 50), (236, 196, 104))

#: Royal gold for a crown and a circlet, dark to light, and the stones set in them: the king's ruby
#: and the princess's sapphire. None of them changes colour.
REGALIA = ((116, 66, 30), (184, 124, 44), (236, 186, 78), (255, 236, 156))
RUBY = ((108, 20, 48), (192, 36, 66), (252, 128, 146))
SAPPHIRE = ((26, 48, 116), (56, 110, 206), (164, 210, 255))


def head_box(f, hair, r, c):
    """His head in a frame: the top, the eye row, the head's middle across, and how wide it is at
    the eyes, from the hair and the face together."""
    eyes = eye_row(f)
    span = head_span(f, hair, r, c, eyes - 1) or head_span(f, hair, r, c, eyes)
    return f.top, eyes, (span[0] + span[1]) / 2.0, span


def paint(layer, r, c, cells, ramp, lit_left=True):
    """Fills a set of cells with a ramp, lit from the top left: the light side gets the light end,
    the far side the dark end, and an outline goes round the outside below and to the sides."""
    if not cells:
        return
    xs = [x for x, _ in cells]
    ys = [y for _, y in cells]
    x0, x1, y0, y1 = min(xs), max(xs), min(ys), max(ys)
    top = len(ramp) - 1
    for x, y in cells:
        u = (x - x0) / max(1, x1 - x0) if lit_left else 1 - (x - x0) / max(1, x1 - x0)
        v = (y - y0) / max(1, y1 - y0)
        level = round(top - (u * 0.6 + v * 0.6) * top)
        layer.put(r, c, x, y, ramp[max(0, min(top, level))])
    for x, y in cells:
        for n in ((x - 1, y), (x + 1, y), (x, y + 1), (x, y - 1)):
            if n not in cells and layer.get(r, c, *n) is None:
                layer.put(r, c, n[0], n[1], OUTLINE)


def mirrored(points, cx):
    """Points flipped across a column, for the other side of a head."""
    return [(round(2 * cx - x), y) for x, y in points]


def bald(frames, hair, keys):
    """Bald: a smooth crown of skin where the hair was, lit on top, edged like the rest of him."""
    ramp = keys["skin"]
    layer = Layer()
    for r in range(4):
        for c in range(PEOPLE_COLUMNS):
            f = frames[r][c]
            top, eyes, cx, span = head_box(f, hair, r, c)
            cy = eyes - 1
            rx = (span[1] - span[0]) / 2.0 - 0.5
            ry = cy - top - 2.0
            cells = set()
            for y in range(top, cy + 1):
                for x in range(FRAME_W):
                    inside = ((x - cx) / rx) ** 2 + ((y - cy) / ry) ** 2 <= 1.0
                    if inside and f.part.get((x, y)) not in ("skin", "eyes", "shine"):
                        cells.add((x, y))
            paint(layer, r, c, cells, ramp[1:])
            # The brow and the ears where the hair used to hide them.
            for x, y in cells:
                if y == cy and f.part.get((x, y + 1)) == "skin":
                    layer.put(r, c, x, y, ramp[2])
    keep("hair-bald", layer)


def ears(frames, hair, keys):
    """The peoples' ears: the Sylvan's long points, and the Wildkin's wolf, hawk and bear ears on
    top of the head. Points are the colour of the skin; beast ears are the hair's."""
    skin, fur = keys["skin"], keys["hair"]
    for kind in ("pointed", "wolf", "hawk", "bear"):
        layer = Layer()
        for r in range(4):
            for c in range(PEOPLE_COLUMNS):
                f = frames[r][c]
                top, eyes, cx, span = head_box(f, hair, r, c)
                if kind == "pointed":
                    # From the side of the head at the eyes, out and up to a point.
                    shape = [(0, 0), (0, -1), (-1, -1), (-1, -2), (-2, -2), (-2, -3), (-3, -4), (0, 1)]
                    left = [(span[0] + dx, eyes - 1 + dy) for dx, dy in shape]
                    sides = {DOWN: (left, mirrored(left, cx)), UP: (left, mirrored(left, cx)),
                             LEFT: (mirrored(left, cx),), RIGHT: (left,)}[r]
                    for side in sides:
                        paint(layer, r, c, set(side), skin[1:])
                    continue

                # Beast ears sit on the crown, either side of the middle, and from the side only one shows.
                if kind == "wolf":
                    shape = [(0, 0), (1, 0), (2, 0), (3, 0), (0, -1), (1, -1), (2, -1), (1, -2), (2, -2), (1, -3)]
                    inner = [(1, 0), (2, 0), (1, -1)]
                elif kind == "hawk":
                    shape = [(0, 0), (1, 0), (2, 0), (0, -1), (1, -1), (-1, -2), (0, -2), (-1, -3), (-2, -4)]
                    inner = []
                else:
                    shape = [(0, 0), (1, 0), (2, 0), (3, 0), (0, -1), (1, -1), (2, -1), (3, -1), (1, -2), (2, -2)]
                    inner = [(1, 0), (2, 0)]
                base_y = top + 2
                left_x = round(cx) - 5
                left = [(left_x + dx, base_y + dy) for dx, dy in shape]
                right = mirrored(left, cx)
                if r == LEFT:
                    pairs = [(mirrored(left, cx), mirrored([(left_x + dx, base_y + dy) for dx, dy in inner], cx))]
                elif r == RIGHT:
                    pairs = [(left, [(left_x + dx, base_y + dy) for dx, dy in inner])]
                else:
                    pairs = [(left, [(left_x + dx, base_y + dy) for dx, dy in inner]),
                             (right, mirrored([(left_x + dx, base_y + dy) for dx, dy in inner], cx))]
                for outside, middle in pairs:
                    paint(layer, r, c, set(outside), fur[1:])
                    if r != UP:
                        for x, y in middle:
                            layer.put(r, c, x, y, skin[2])
        keep("ears-" + kind, layer)


def horns(frames, hair):
    """The Emberkin's horns, out of the hair at the temples: curled round like a ram's, or swept
    back in a long curve."""
    for kind in ("curl", "swept"):
        layer = Layer()
        for r in range(4):
            for c in range(PEOPLE_COLUMNS):
                f = frames[r][c]
                top, eyes, cx, span = head_box(f, hair, r, c)
                base_y = top + 4
                if kind == "curl":
                    shape = [(0, 0), (-1, 0), (-1, -1), (-2, -1), (-3, -1), (-3, 0), (-4, 0), (-4, 1), (-4, 2), (-3, 3), (-2, 3)]
                else:
                    shape = [(0, 0), (1, 0), (0, -1), (1, -1), (-1, -2), (0, -2), (-1, -3), (-2, -3), (-2, -4), (-3, -5)]
                left = [(span[0] + 3 + dx, base_y + dy) for dx, dy in shape]
                right = mirrored(left, cx)
                if r == LEFT:
                    sides = [mirrored(left, cx)] if kind == "curl" else [[(round(cx) - 1 - dx, base_y + dy) for dx, dy in shape]]
                elif r == RIGHT:
                    sides = [left] if kind == "curl" else [[(round(cx) + 1 + dx, base_y + dy) for dx, dy in shape]]
                else:
                    sides = [left, right]
                for side in sides:
                    paint(layer, r, c, set(side), HORN)
        keep("horns-" + kind, layer)


def tails(frames, keys):
    """Tails, out of the small of the back: the Emberkin's thin one with a burning tip, and the
    wolf line's bushy one. Each comes in two halves like the cape: what hangs behind, and what shows
    over the body from behind."""
    fur = keys["hair"]
    for kind in ("ember", "wolf"):
        back, front = Layer(), Layer()
        for r in range(4):
            for c in range(PEOPLE_COLUMNS):
                f = frames[r][c]
                waist = f.waist
                body = f.row_span(waist, None)
                cx = (body[0] + body[1]) / 2.0
                sway = (0, 1, 0, -1, 0)[c]
                if r == UP:
                    path = [(round(cx) + i // 3 + sway * (i > 4), waist - 1 + i) for i in range(9)]
                    layer, away = front, 1
                elif r == DOWN:
                    path = [(body[1] + 1 + i // 2, waist + 1 + i - (i > 5) * (i - 5)) for i in range(8)]
                    layer, away = back, 1
                else:
                    away = 1 if r == LEFT else -1
                    edge = body[1] if r == LEFT else body[0]
                    path = [(edge + away * (1 + i // 2), waist + 1 + i // 2 + (i > 6)) for i in range(10)]
                    layer = back
                cells = set()
                for i, (x, y) in enumerate(path):
                    # The ember tail's a line; the wolf's is bushy, fattest in the middle.
                    width = 1 if kind == "ember" else 2 + (1 if 1 < i < len(path) - 2 else 0)
                    for w in range(width):
                        cells.add((x + w * away if r != UP else x + w - 1, y))
                        if kind == "wolf" and 2 < i < len(path) - 1:
                            cells.add((x + w * away if r != UP else x + w - 1, y - 1))
                if kind == "ember":
                    body_cells = set(list(cells)[:])
                    tip = set(path[-3:])
                    paint(layer, r, c, body_cells - tip, EMBER_TAIL)
                    for i, (x, y) in enumerate(path[-3:]):
                        layer.put(r, c, x, y, EMBER_TIP[i])
                    x, y = path[-1]
                    layer.put(r, c, x, y + 1, EMBER_TIP[1])
                else:
                    paint(layer, r, c, cells, fur[1:])
                    x, y = path[-1]
                    layer.put(r, c, x, y, fur[4])
        keep("tail-" + kind + "-back", back)
        keep("tail-" + kind + "-front", front)


# --------------------------------------------------------------------------- #
# Hats
# --------------------------------------------------------------------------- #

#: Where a hat's brim sits, in rows above his eyes: on his hairline, so his brows and eyes show
#: under it. The first hats came down to his brows, and in a close-up the brim went across his eyes.
BRIM_ABOVE_EYES = 5

#: How many rows a tricorn's crown and a helm's dome stand up over the top of his head, so they
#: sit on it rather than being painted into his hair.
CROWN_RISE = 1


def hat_anchor(f, hair, r, c):
    """Where a hat goes in a frame: the top of his head, the brim's row, his head's middle across,
    and its span on the brim's row. Facing away there are no eyes to measure from, and the guess
    from his chin wobbles from step to step, so it's measured up from his neck instead, which is
    where his eyes are from it facing down."""
    top, eyes, cx, span = head_box(f, hair, r, c)
    if r == UP:
        eyes = f.neck - 3
    brim = eyes - BRIM_ABOVE_EYES
    return top, brim, cx, head_span(f, hair, r, c, brim) or span


#: How many rows down from the top of his head his skull is wide enough to shape a hat's dome on.
#: Above that, it's the tufts of his hair, which a hat flattens.
DOME_ROWS = 2


def crown(f, hair, r, c, top, brim, rise, narrower):
    """The cells a hat covers, from `rise` rows over his head down to its brim: as wide as his head
    on each row (less `narrower` each side), and rounded over the top, more each row up."""
    cells = set()
    base = top + DOME_ROWS
    for y in range(top - rise, brim + 1):
        span = head_span(f, hair, r, c, max(y, base))
        if span is None:
            continue
        up = max(0, base - y)
        inset = narrower + up * (up + 1) // 4
        cells.update((x, y) for x in range(span[0] + inset, span[1] - inset + 1))
    return cells


def hats(frames, hair, keys):
    """Hats over the hair, sitting on his crown with the brim on his hairline: a bandana in the
    vest's dye, knotted at the back (the Red Gulls wear crimson ones); a captain's tricorn, black
    with a gold edge; and the Watch's steel helm. Each is worked out from his head in that frame,
    so it bobs with him, and the Stout and Small bodies get it on the same head."""
    for kind in ("bandana", "tricorn", "helm"):
        layer = Layer()
        for r in range(4):
            for c in range(PEOPLE_COLUMNS):
                f = frames[r][c]
                top, brim, cx, span = hat_anchor(f, hair, r, c)
                if kind == "bandana":
                    paint(layer, r, c, crown(f, hair, r, c, top, brim, 0, 0), keys["top"][1:])
                    if r in (LEFT, RIGHT, UP):
                        knot_x = (span[1] + 1) if r == LEFT else (span[0] - 1) if r == RIGHT else round(cx)
                        for i in range(4):
                            dx = (1 if r == LEFT else -1) * (i // 2) if r != UP else (i % 2) * 2 - 1
                            layer.put(r, c, knot_x + dx, brim + i, keys["top"][2 if i % 2 else 1])
                elif kind == "tricorn":
                    paint(layer, r, c, crown(f, hair, r, c, top, brim - 1, CROWN_RISE, 1), TRICORN)
                    # The brim: gold-edged, past his head both sides, cocked up at the corners
                    # (two rows at the tips, one next to them), and its point coming down a row
                    # over the way he's facing.
                    x0, x1 = span[0] - 2, span[1] + 2
                    for x in range(x0, x1 + 1):
                        layer.put(r, c, x, brim, TRIM[1] if x % 3 else TRIM[0])
                        layer.put(r, c, x, brim + 1, OUTLINE)
                    for x, lift in ((x0, 2), (x1, 2), (x0 + 1, 1), (x1 - 1, 1)):
                        for y in range(brim - lift, brim):
                            layer.put(r, c, x, y, TRICORN[2] if x in (x0, x0 + 1) else TRICORN[1])
                        layer.put(r, c, x, brim - lift - 1, OUTLINE)
                    for x in (x0, x1):
                        layer.put(r, c, x, brim - 2, TRIM[1])
                    point = {DOWN: round(cx), LEFT: x0 + 1, RIGHT: x1 - 1}.get(r)
                    if point is not None:
                        layer.put(r, c, point, brim + 1, TRIM[0])
                        layer.put(r, c, point, brim + 2, OUTLINE)
                else:
                    paint(layer, r, c, crown(f, hair, r, c, top, brim, CROWN_RISE, 0), STEEL)
                    for x in range(span[0] - 1, span[1] + 2):
                        layer.put(r, c, x, brim, STEEL[1])
                        layer.put(r, c, x, brim + 1, OUTLINE)
                    if r == DOWN:
                        layer.put(r, c, round(cx), top - CROWN_RISE + 1, STEEL[3])
        keep("hat-" + kind, layer)
    royal_crown(frames, hair)
    hood(frames, hair, keys)
    circlet(frames, hair)


#: The king's crown, as rows of text from its points down to the bottom of its band: G is gold
#: and J is where the jewel goes facing you. Three points, the middle one tallest.
CROWN_SHAPE = (
    "G...G...G",
    "G..GGG..G",
    "GGGGGGGGG",
    "GGGGJGGGG",
    "GGGGGGGGG",
)

#: How many rows over the hat's brim the crown's band stands, so it sits up on top of his head
#: with its points over his hair, the way a small crown does.
CROWN_ABOVE_BRIM = 3


def royal_crown(frames, hair):
    """King Leon's small gold crown with a ruby in front, sat on top of the head over the hair.
    Its points catch the light on their left edges, the band's top edge is the brightest row, and
    from the side the ruby's at the front edge; from behind there isn't one."""
    layer = Layer()
    width = len(CROWN_SHAPE[0])
    for r in range(4):
        for c in range(PEOPLE_COLUMNS):
            f = frames[r][c]
            top, brim, cx, span = hat_anchor(f, hair, r, c)
            x0 = round(cx - width / 2)
            y0 = brim - CROWN_ABOVE_BRIM - len(CROWN_SHAPE) + 1
            jewel = {DOWN: width // 2, LEFT: 1, RIGHT: width - 2}.get(r)
            cells = set()
            for row, text in enumerate(CROWN_SHAPE):
                for col, ch in enumerate(text):
                    if ch == ".":
                        continue
                    x, y = x0 + col, y0 + row
                    cells.add((x, y))
                    band = row - 2
                    if band < 0:
                        # The points: light on the side the light comes from.
                        lit = (x - 1, y) not in cells and CROWN_SHAPE[row][col - 1:col] != "G"
                        colour = REGALIA[3] if row == 0 or lit else REGALIA[2]
                    elif band == 0:
                        colour = REGALIA[3] if col < width - 2 else REGALIA[2]
                    elif band == 1:
                        colour = REGALIA[2] if col < width // 2 else REGALIA[1]
                    else:
                        colour = REGALIA[1] if col < width - 2 else REGALIA[0]
                    layer.put(r, c, x, y, colour)
            if jewel is not None:
                x, y = x0 + jewel, y0 + 3
                layer.put(r, c, x, y, RUBY[1])
                layer.put(r, c, x, y - 1, RUBY[2])
                layer.put(r, c, x + 1 if r != RIGHT else x - 1, y, RUBY[0])
            for x, y in cells:
                for n in ((x - 1, y), (x + 1, y), (x, y + 1), (x, y - 1)):
                    if n not in cells and layer.get(r, c, *n) is None:
                        layer.put(r, c, n[0], n[1], OUTLINE)
    keep("hat-crown", layer)


#: How far a hood stands out from the head each side, in pixels. Over the top it comes up to
#: where the tips of the hair were, which is a pixel or two over the skull.
HOOD_LOFT = 1

#: How many rows of forehead show under a hood's edge, over the top of the eyes. One of them is in
#: its shadow.
HOOD_BROW = 2


def hood(frames, hair, keys):
    """A deep travelling hood in the cloak's keys, so it's the same dye as their cloak (woad
    without one). It hides the hair and ears, which Paperdoll knows, so it's the whole head: a
    rounded cowl a pixel bigger than the head with a little peak, open on the face from
    just over the eyes down to the chin, lined dark round the opening, and falling onto the
    shoulders. From the side the opening's the face's front, from behind there isn't one and a seam
    runs down the middle."""
    ramp = CLOAK_KEYS
    layer = Layer()
    for r in range(4):
        for c in range(PEOPLE_COLUMNS):
            f = frames[r][c]
            top, eyes, cx, span = head_box(f, hair, r, c)
            if r == UP:
                eyes = f.neck - 3
            shoulders = f.row_span(f.neck + 1, ("top", "shirt", "outline", "skin"))
            face = {}
            for x, y in f.face + [p for e in f.eyes for p in e]:
                face.setdefault(y, []).append(x)
            # The hood's outline: the head's width and a pixel each side, rounded over the top,
            # and wide enough everywhere to cover the face round the opening.
            cy = eyes - 3
            rx = (span[1] - span[0]) / 2.0 + HOOD_LOFT + 0.5
            ry = cy - top + 0.5
            outside = set()
            for y in range(top, f.neck):
                if y < cy:
                    half = rx * max(0.0, 1 - ((y - cy) / ry) ** 2) ** 0.5
                else:
                    half = rx
                if half < 1:
                    continue
                x0, x1 = round(cx - half), round(cx + half) - 1
                if y in face:
                    x0, x1 = min(x0, min(face[y]) - 1), max(x1, max(face[y]) + 1)
                outside.update((x, y) for x in range(x0, x1 + 1))
            # The peak: a pixel up at the top, off to the back from the side.
            peak = {LEFT: round(cx) + 2, RIGHT: round(cx) - 3}.get(r, round(cx) - 1)
            outside.update(((peak, top - 1), (peak + 1, top - 1)))
            # The cowl on the shoulders: a row as wide as them, under the chin.
            for y in (f.neck, f.neck + 1):
                inset = 1 if y == f.neck else 0
                outside.update((x, y) for x in range(shoulders[0] + inset, shoulders[1] - inset + 1))

            # The opening: the face from a little over the eyes to the chin, less its back edge
            # from the side and a pixel at each cheek from the front.
            top_eye = min((y for e in f.eyes for _, y in e), default=eyes - 1)
            opening = set()
            if r != UP:
                for y in range(top_eye - HOOD_BROW, f.chin + 1):
                    xs = face.get(y)
                    if not xs:
                        continue
                    lo, hi = min(xs), max(xs)
                    if r == DOWN:
                        brow = 2 if y == top_eye - HOOD_BROW else 1 if y > f.chin - 2 else 0
                        lo, hi = lo + brow, hi - brow
                    elif r == LEFT:
                        hi -= 2
                    else:
                        lo += 2
                    opening.update((x, y) for x in range(lo, hi + 1) if (x, y) in f.part)
            cloth = outside - opening

            # Shaded like a ball lit from the top left, the lining dark round the opening, and the
            # folds where it gathers under the chin and down the back.
            for x, y in cloth:
                u = (x + 0.5 - cx) / max(1.0, rx)
                v = (y + 0.5 - cy) / max(1.0, ry if y < cy else f.neck - cy)
                light = -0.55 * u - 0.65 * v + 0.35
                level = 3 if light > 0.45 else 2 if light > -0.1 else 1
                if any(n in opening for n in ((x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1))):
                    level = 0
                elif r == UP and abs(x + 0.5 - cx) < 1 and top < y < f.neck - 1:
                    level = max(1, level - 1)
                layer.put(r, c, x, y, ramp[level])
            if r == DOWN:
                # The shadow the hood's edge throws on the forehead.
                for x, y in opening:
                    if (x, y - 1) not in opening and f.part.get((x, y)) == "skin":
                        layer.put(r, c, x, y, keys["skin"][1])
                mid = round(cx) - 1
                layer.put(r, c, mid, f.neck, GOLD[1])
                layer.put(r, c, mid + 1, f.neck, GOLD[0])
            elif r in (LEFT, RIGHT):
                for x, y in opening:
                    if (x, y - 1) not in opening and f.part.get((x, y)) == "skin":
                        layer.put(r, c, x, y, keys["skin"][1])
            for x, y in cloth:
                for n in ((x - 1, y), (x + 1, y), (x, y + 1), (x, y - 1)):
                    if n not in outside and layer.get(r, c, *n) is None:
                        layer.put(r, c, n[0], n[1], OUTLINE)
    keep("hat-hood", layer)


def circlet(frames, hair, name="hat-circlet", ramp=REGALIA, stone=SAPPHIRE):
    """A slim gold circlet over the hair at the hairline, with a sapphire on the brow (or another
    metal and stone). Round the head, so its middle sits a row lower than its ends from every side;
    the stone's at the front of it facing you or side on, and from behind it's plain metal."""
    layer = Layer()
    for r in range(4):
        for c in range(PEOPLE_COLUMNS):
            f = frames[r][c]
            top, brim, cx, span = hat_anchor(f, hair, r, c)
            x0, x1 = span[0], span[1]
            for x in range(x0, x1 + 1):
                end = min(x - x0, x1 - x)
                y = brim - (1 if end < 2 else 0)
                layer.put(r, c, x, y, ramp[3] if (x + y) % 3 == 0 else ramp[2] if x < cx else ramp[1])
                layer.put(r, c, x, y + 1, OUTLINE)
                if end == 0:
                    layer.put(r, c, x, y - 1, OUTLINE)
            at = {DOWN: round(cx) - 1, LEFT: x0 + 1, RIGHT: x1 - 2}.get(r)
            if at is not None:
                layer.put(r, c, at, brim - 1, stone[2])
                layer.put(r, c, at + 1, brim - 1, stone[1])
                layer.put(r, c, at, brim, stone[1])
                layer.put(r, c, at + 1, brim, stone[0])
                layer.put(r, c, at - 1, brim - 1, ramp[3])
                layer.put(r, c, at + 2, brim - 1, ramp[1])
                for x in range(at - 1, at + 3):
                    layer.put(r, c, x, brim - 2, OUTLINE)
    keep(name, layer)


# --------------------------------------------------------------------------- #
# The bodies
# --------------------------------------------------------------------------- #

#: Every sheet, by name, before it's saved for each body.
SHEETS = {}


def keep(name, layer):
    """Holds a finished sheet till every body's been made from it."""
    SHEETS[name] = layer


#: The bodies: the folder each one's sheets go in, the rows taken out of the torso and the legs
#: (counting down from his neck and his waist), and how many columns wider the torso goes. The
#: Stoneborn are a head shorter than him and broad with it; the Hearthlings are smaller again.
BUILDS = (
    ("Paperdoll", (), (), 0),
    ("Paperdoll/Stout", (4,), (2, 3, 4, 5), 4),
    ("Paperdoll/Small", (3, 4, 5), (2, 3, 4, 5, 6), 0),
)


def built(frame_pixels, f, torso, legs, wider):
    """One frame of a sheet made for another body: the rows taken out, everything above them
    dropped to stand on the same feet, and the torso widened down its middle."""
    dropped = {f.neck + d for d in torso} | {f.waist + d for d in legs}
    rows = [frame_pixels[y] for y in range(FRAME_H) if y not in dropped]
    out = [[[0, 0, 0, 0] for _ in range(FRAME_W)] for _ in range(FRAME_H)]
    shift = FRAME_H - len(rows)
    neck = f.neck + shift
    for y, line in enumerate(rows):
        out[y + shift] = [list(px) for px in line]
    if wider:
        # The middle of his body, from his own frame, so every layer widens down the same column.
        body = [x for x, y in f.drawn if y >= f.neck]
        cx = round(sum(body) / len(body))
        for y in range(max(0, neck), FRAME_H):
            line = out[y]
            new = [[0, 0, 0, 0] for _ in range(FRAME_W)]
            for x in range(FRAME_W):
                if x < cx:
                    src = x + wider // 2
                elif x > cx + wider - 1:
                    src = x - wider // 2
                else:
                    src = cx
                if 0 <= src < FRAME_W:
                    new[x] = list(line[src])
            out[y] = new
    return out


# --------------------------------------------------------------------------- #
# The fights' frames
# --------------------------------------------------------------------------- #
# Every sheet goes on past his walk to the five frames a fight uses. They're not drawn: they're
# his standing frame cut up and moved, the way you'd pose a paper puppet, by one map for each
# frame of where every pixel comes from. The same map moves every layer, so a hat stays on his
# head and a sleeve on his arm whatever he's doing, and the layers still stack. His arms come off
# and go where the frame wants them (raised to swing, out to strike or cast), he leans from his
# feet, sinks or comes up at the knee, and his feet go apart. Lying down is the whole of him
# turned onto his back.

#: The fights' frames after the walk: winding up, striking, casting, hurt and downed.
WIND_UP, STRIKE, CAST, HURT, DOWNED = range(PEOPLE_COLUMNS, PEOPLE_COLUMNS + 5)

#: Standing, four steps, and the fights' five.
COLUMNS = DOWNED + 1

#: The colour a probe frame puts a pixel's part in (its blue), so it can be read back once a body's
#: been made from it. Red and green are where the pixel was on him.
PROBE_PARTS = ("skin", "hair", "eyes", "shine", "outline", "top", "shirt", "leather", "bottoms", "boots",
               "weapon arm", "other arm")

#: How a front view, a back view and a side view stand in each fight frame, as what changes from
#: standing. lean: how many pixels the top of the head goes over (towards the way they face from
#: the side, towards the other arm from the front and behind; back if it's less than nothing), the
#: rest of them leaning from the feet; lift: how many rows they come up (or sink, less than
#: nothing), bending at the knee; stride: how many pixels each foot moves apart (from the side, the
#: front foot forward and the back one back); shove: how many pixels the whole of them goes back;
#: arm and other: where the weapon arm and the other arm go, or None to stay down: "up" over the
#: head, "ahead" straight out from the shoulder, "push" out in front of the chest (from the side,
#: the far hand comes up beside it, so both hands push a spell out), "across" in front of the body.
FRONT_POSES = {
    WIND_UP: dict(lean=1, lift=0, stride=1, shove=0, arm="up", other=None),
    STRIKE: dict(lean=-1, lift=-1, stride=1, shove=0, arm="across", other=None),
    CAST: dict(lean=0, lift=1, stride=0, shove=0, arm="up", other="up"),
    HURT: dict(lean=-2, lift=-1, stride=1, shove=0, arm=None, other=None),
}
BACK_POSES = {**FRONT_POSES,
              # From behind, a blow is over the head: the arm stays up, and the weapon comes over.
              STRIKE: dict(lean=-1, lift=-1, stride=1, shove=0, arm="up", other=None)}
SIDE_POSES = {
    WIND_UP: dict(lean=-2, lift=0, stride=1, shove=1, arm="up", other=None),
    STRIKE: dict(lean=2, lift=-1, stride=2, shove=-1, arm="ahead", other=None),
    CAST: dict(lean=-1, lift=1, stride=1, shove=0, arm="push", other="push"),
    HURT: dict(lean=-4, lift=-1, stride=0, shove=1, arm=None, other=None),
}

#: How long someone lying down can be, head to heels: the width of a frame. Anyone longer has rows
#: taken out of their legs and body first.
DOWNED_LENGTH = FRAME_W


def facing_ahead(r):
    """Which way across a side view faces: -1 left, 1 right, 0 for the front and back."""
    return {LEFT: -1, RIGHT: 1}.get(r, 0)


def weapon_side(r):
    """Which side of the frame the weapon arm's on: the left from the front (it's his right hand),
    the right from behind; from the side it's the arm nearer you, which hangs down his back."""
    return {DOWN: -1, UP: 1, LEFT: 1, RIGHT: -1}[r]


#: How many rows under his neck his hands start: the skin above that is his neck and his collar.
HANDS_BELOW_NECK = 6

#: How many rows under his neck his arms start, below the shoulder that stays with his body.
ARMS_BELOW_NECK = 2

#: How many rows at each end of an arm are its shoulder and its hand.
ARM_ENDS = 3


def arm_strips(f, r):
    """His two arms in a frame, the weapon arm first, each as the pixels it is, from under the
    shoulder to the bottom of the hand. From the front and behind each is everything out past the
    inside edge of its hand. From the side the weapon arm's the near one, the strip down his back
    in front of his vest, and the other is the far hand peeping out in front of him."""
    skin = [p for p, k in f.part.items() if k == "skin" and p[1] > f.neck + HANDS_BELOW_NECK]
    middle = sum(x for x, _ in f.drawn) / len(f.drawn)
    groups = {-1: [], 1: []}
    for patch in clusters(skin):
        groups[-1 if sum(x for x, _ in patch) / len(patch) < middle else 1].extend(patch)
    side = weapon_side(r)
    near, far = groups[side], groups[-side]
    top = f.neck + ARMS_BELOW_NECK

    def outer(hand, s, rows, reach):
        """Everything from the edge of the frame on side s in to the hand's inside edge (and
        `reach` past it), on some rows."""
        edge = max(x for x, _ in hand) + reach if s < 0 else min(x for x, _ in hand) - reach
        return {(x, y) for (x, y) in f.part if y in rows and (x <= edge if s < 0 else x >= edge)}

    bottom = max(y for _, y in near + far)
    rows = range(top, bottom + 1)
    if r in (DOWN, UP):
        return outer(near, side, rows, 0), outer(far, -side, rows, 0)
    weapon = set()
    for y in rows:
        vest = [x for (x, yy), k in f.part.items() if yy == y and k == "top"]
        if vest:
            inside = min(vest) if side < 0 else max(vest)
            weapon |= {(x, y) for (x, yy) in f.part if yy == y and (x < inside if side < 0 else x > inside)}
        else:
            weapon |= outer(near, side, {y}, 1)
    hand_rows = range(min(y for _, y in far), bottom + 1)
    return weapon, outer(far, -side, hand_rows, 0)


class Rig:
    """One body's standing frame facing one way, worked out for posing: where his neck, waist and
    the top of his head are, the middle of his legs, and his two arms, all on that body. It comes
    from a probe: his frame with every pixel's part and place written into its colour, made into
    that body the way any sheet is, and read back."""

    def __init__(self, f, r, build):
        _, torso, legs, wider = build
        weapon, other = arm_strips(f, r)
        probe = new_image(FRAME_W, FRAME_H)
        for (x, y), k in f.part.items():
            part = "weapon arm" if (x, y) in weapon else "other arm" if (x, y) in other else k
            probe[y][x] = [x, y, PROBE_PARTS.index(part), 255]
        if torso or legs or wider:
            probe = built(probe, f, torso, legs, wider)
        self.r = r
        self.ahead = facing_ahead(r)
        self.side = weapon_side(r)
        self.part, self.was = {}, {}
        for y in range(FRAME_H):
            for x in range(FRAME_W):
                if probe[y][x][3]:
                    self.part[(x, y)] = PROBE_PARTS[probe[y][x][2]]
                    self.was[(x, y)] = (probe[y][x][0], probe[y][x][1])
        self.neck = min(y for (x, y), was in self.was.items() if was[1] >= f.neck)
        self.waist = max(y for (x, y), was in self.was.items() if was[1] <= f.waist)
        self.top = min(y for _, y in self.part)
        legs_xs = [x for (x, y) in self.part if y > self.waist]
        self.legs = (min(legs_xs) + max(legs_xs) + 1) / 2.0
        self.arms = {"weapon": {p for p, k in self.part.items() if k == "weapon arm"},
                     "other": {p for p, k in self.part.items() if k == "other arm"}}
        self.knee = self.waist + (SOLES - self.waist) // 2

    def band(self, y):
        """Which part of him a row is: the head, the body or the legs."""
        return "head" if y < self.neck else "body" if y <= self.waist else "legs"


def lifted_row(rig, y, lift):
    """The row of his standing frame that a row of a posed one shows, when he's come up `lift` rows
    (or sunk, if it's less than nothing) by bending at the knee: everything over his knee moves,
    and the knee itself stretches or folds."""
    if lift > 0:
        return y + lift if y <= rig.knee - lift else rig.knee if y <= rig.knee else y
    if lift < 0:
        return y + lift if y <= rig.knee else y
    return y


def leaned(rig, y, lean):
    """How many pixels across a row of him moves when the top of his head goes `lean` over,
    leaning from his feet."""
    return round(lean * (SOLES - y) / float(SOLES - rig.top))


#: How far back an arm held straight out from the side is drawn, which is as if it's pointing a
#: little towards you or away: at its full length the hand's so near the front of the frame that
#: nothing in it has room.
REACH_BACK = 1

#: How far an arm brought across in front of him goes, as a share of the way from his shoulder to
#: his middle.
ACROSS = 0.6

#: Pushing a spell out from the side: how many rows under the shoulder the near arm comes out, and
#: how far forward and up the far hand comes from his hip to meet it.
PUSH_DROP = 2
PUSH_REACH = 3
PUSH_RISE = 5


def arm_moved(rig, arm, how, at):
    """Where every pixel of an arm goes for one of the arm poses: a dict from where it was to
    where it goes. `at` is how far across and down his shoulder's moved with his body."""
    cells = rig.arms[arm]
    if not cells or how is None:
        return {}
    top = min(y for _, y in cells)
    s = rig.side if arm == "weapon" else -rig.side
    shoulder = [x for x, y in cells if y < top + ARM_ENDS]
    shoulder = round(sum(shoulder) / len(shoulder))
    dx, dy = at
    out = {}
    for x, y in cells:
        if how == "up":
            # Flipped over at the shoulder, so the hand's up and the light's still on the left.
            # From the front it goes out a pixel, clear of the head.
            to = (x + (s if rig.ahead == 0 else 0), 2 * top - y)
        elif how in ("ahead", "push") and arm == "weapon":
            # Swung forward to lie straight out from the shoulder, the side that was lit on top, and
            # drawn back a little so a weapon has room in front of it in the frame; pushing a spell,
            # a little lower, in front of the chest.
            along, across = y - top, (shoulder - x) * -rig.ahead * s
            to = (shoulder + rig.ahead * (along - REACH_BACK), top - across + (PUSH_DROP if how == "push" else 0))
        elif how == "push":
            # The far hand, brought up and forward from his hip to beside the near one.
            to = (x + rig.ahead * PUSH_REACH, y - PUSH_RISE)
        else:
            # Brought across in front of him, the hand over his belly and a row lower.
            to = (x - s * round(abs(rig.legs - shoulder) * ACROSS), y + 1)
        out[(x, y)] = (to[0] + dx, to[1] + dy)
    return out


def pose_map(rig, c, behind=False):
    """Where every pixel of a posed frame comes from in the standing one: a dict from the posed
    frame's pixel to the standing frame's. With `behind`, it's for a sheet that hangs behind him (a
    cape's back, a tail), which doesn't go with his arms and needn't fill in behind them."""
    if c == DOWNED:
        return downed_map(rig)
    pose = (SIDE_POSES if rig.ahead else BACK_POSES if rig.r == UP else FRONT_POSES)[c]
    across = rig.ahead if rig.ahead else -rig.side
    lean, shove = pose["lean"] * across, -pose["shove"] * (rig.ahead or 0)
    stride = pose["stride"]
    moved = {} if behind else {arm: pose[key] for arm, key in (("weapon", "arm"), ("other", "other")) if pose[key]}
    out = {}
    for y in range(FRAME_H):
        sy = lifted_row(rig, y, pose["lift"])
        if not 0 <= sy < FRAME_H:
            continue
        dx = leaned(rig, sy, lean) + shove
        for sx in range(FRAME_W):
            if any((sx, sy) in rig.arms[arm] for arm in moved):
                continue
            x = sx + dx
            if rig.band(sy) == "legs" and not behind:
                # Feet apart: the leg on the right goes right and the other left, which from the
                # side is the front foot forward and the back one back, whichever way they face.
                x += stride if sx >= rig.legs else -stride
            out[(x, y)] = (sx, sy)

    # The arms, over the rest of him, from where his shoulder's got to.
    for arm, how in moved.items():
        top = min(y for _, y in rig.arms[arm])
        row = next(y for y in range(FRAME_H) if lifted_row(rig, y, pose["lift"]) == top)
        at = (leaned(rig, top, lean) + shove, row - top)
        for src, to in arm_moved(rig, arm, how, at).items():
            out[to] = src
    if moved.get("weapon") and rig.ahead and not behind:
        fill_back(rig, out, pose["lift"], lean, shove)
    return {p: s for p, s in out.items() if 0 <= p[0] < FRAME_W and 0 <= p[1] < FRAME_H}


def fill_back(rig, out, lift, lean, shove):
    """From the side, his near arm is the back of him, so once it's gone up or out there's a gap.
    Each row of it is filled from the first pixel of his body in front of it, and the outermost
    from the arm's own edge, so his back's still edged."""
    inward = -rig.side
    arm = rig.arms["weapon"]
    for y in range(FRAME_H):
        sy = lifted_row(rig, y, lift)
        row = sorted((sx for sx, yy in arm if yy == sy), key=lambda sx: sx * inward)
        if not row:
            continue
        dx = leaned(rig, sy, lean) + shove
        for sx in row:
            x = sx + dx
            if (x, y) in out:
                continue
            if sx == row[0]:
                out[(x, y)] = (sx, sy)
                continue
            k = x + inward
            while 0 <= k < FRAME_W and ((k, y) not in out or out[(k, y)] in arm):
                k += inward
            if 0 <= k < FRAME_W:
                out[(x, y)] = out[(k, y)]


#: How many rows a hat can stand over the top of his hair, which lying down is kept in the frame.
HEADROOM = 2


def downed_map(rig):
    """Lying on the ground: the standing frame turned a quarter over onto the row his feet were
    on, the head towards his back from the side (he's gone over backwards), to the left from the
    front and to the right from behind. Anyone too long to fit across has rows taken out of their
    body and legs first, the way the shorter bodies are made."""
    head = -rig.ahead if rig.ahead else (-1 if rig.r == DOWN else 1)
    top = rig.top - HEADROOM
    spare = SOLES - top + 1 - DOWNED_LENGTH
    # Every other row of the legs and the body first, a leg row and then a body row, then the rows
    # between them, so what's left is still in proportion.
    legs = list(range(rig.waist + 2, SOLES - 2, 2)) + list(range(rig.waist + 3, SOLES - 2, 2))
    body = list(range(rig.neck + 3, rig.waist - 1, 2)) + list(range(rig.neck + 4, rig.waist - 1, 2))
    order = [row for pair in zip(legs, body) for row in pair] + legs[len(body):] + body[len(legs):]
    gone = set(order[:max(0, spare)])
    kept = [y for y in range(top, SOLES + 1) if y not in gone]
    start = (FRAME_W - len(kept)) // 2
    xs = [x for x, _ in rig.part]
    out = {}
    for i, sy in enumerate(kept):
        x = start + i if head < 0 else start + len(kept) - 1 - i
        for sx in range(FRAME_W):
            # Head left, his right side's up; head right, his left side is.
            y = SOLES - (sx - min(xs)) if head < 0 else SOLES - (max(xs) - sx)
            if 0 <= x < FRAME_W and 0 <= y < FRAME_H:
                out[(x, y)] = (sx, sy)
    return out


def posed(frame, mapping):
    """A frame moved by a pose map."""
    out = new_image(FRAME_W, FRAME_H)
    for (x, y), (sx, sy) in mapping.items():
        if frame[sy][sx][3]:
            out[y][x] = list(frame[sy][sx])
    return out


# --------------------------------------------------------------------------- #
# Gear you wear
# --------------------------------------------------------------------------- #
# Armour, helms, gloves and boots, drawn onto his frames the way the hats are, so they're made
# into the other bodies and posed for the fights the same as everything else. Metal and leather
# are painted in keys of their own, the way his clothes are, so Paperdoll can tint one sheet into
# several things: gear-palettes.png has the keys and every tint, a row each, dark to light. Cloth
# is in his own clothes' keys, so a robe or a padded jacket comes out whatever his vest is dyed.
# Brass, wood and fur never change colour.

#: Metal's keys, dark to light: a cool steel, its lightest shade the glint on an edge.
METAL = ((44, 48, 68), (84, 92, 116), (130, 142, 166), (182, 192, 212), (234, 240, 250))

#: Leather's keys, dark to light: a warm saddle brown.
LEATHER = ((58, 34, 30), (96, 56, 40), (138, 86, 54), (182, 124, 76), (222, 172, 112))

#: The tints after the keys in gear-palettes.png, as a shadow, a middle and a light colour like the
#: clothes' presets. Paperdoll has their names in the same order: iron, blackened steel, bronze
#: and gold; and dark, black, tan and red leather.
TINTS = (
    ("metal", METAL, (
        ((40, 36, 40), (96, 90, 92), (172, 166, 160)),
        ((18, 18, 28), (46, 48, 64), (116, 122, 146)),
        ((92, 50, 24), (176, 112, 52), (242, 198, 122)),
        ((120, 70, 22), (216, 162, 54), (255, 242, 164)),
    )),
    ("leather", LEATHER, (
        ((40, 22, 22), (76, 46, 38), (128, 86, 62)),
        ((20, 18, 24), (46, 42, 50), (100, 94, 104)),
        ((122, 86, 52), (194, 150, 98), (242, 216, 166)),
        ((70, 18, 26), (138, 42, 46), (206, 98, 86)),
    )),
)

#: Fur round the collar and hem of hide armour, dark to light.
FUR = ((76, 62, 56), (126, 108, 94), (176, 158, 136), (218, 206, 188))

#: The stone in a circlet of focus: a violet that's nothing like the princess's sapphire.
FOCUS = ((60, 28, 96), (134, 76, 198), (214, 172, 255))


class Cover:
    """His body in a frame cut up the way armour covers it: the torso from the collar to the hem of
    his vest, his sleeves (the shoulders, down to the elbow, and the forearms) and his hands, and
    his legs."""

    def __init__(self, f):
        weapon, other = arm_strips(f, f.facing)
        arms = weapon | other
        self.hands = {p for p in arms if f.part[p] == "skin"}
        sleeves = arms - self.hands
        # Anything of his vest is body, even where a swinging hand's strip takes it in, so armour
        # never leaves a patch of it showing.
        self.torso = {p for p, k in f.part.items()
                      if k == "top" or f.neck <= p[1] <= f.waist and p not in arms and k != "skin"}
        sleeves -= self.torso
        top = min((y for _, y in self.hands), default=f.waist)
        self.elbow = f.neck + (top - f.neck) // 2 + 1
        self.shoulders = {p for p in sleeves if p[1] < f.neck + SHOULDER_ROWS}
        self.upper = {p for p in sleeves if f.neck + SHOULDER_ROWS <= p[1] < self.elbow}
        self.forearms = {p for p in sleeves if p[1] >= self.elbow}
        self.wrists = {p for p in sleeves if top - CUFF_ROWS <= p[1] < top}
        self.legs = {p for p in f.part if p[1] > f.waist and p not in arms and p not in self.torso}
        belt = range(f.waist - BELT_SEARCH, f.waist + 1)
        self.belt = max(belt, key=lambda y: sum(1 for (x, yy), k in f.part.items() if yy == y and k == "leather"))
        xs = [x for x, _ in self.torso]
        self.middle = (min(xs) + max(xs)) // 2
        self.f = f

    def edge(self, p):
        """Whether a pixel of him is on his outside, with nothing of him next to it."""
        x, y = p
        return any(n not in self.f.part for n in ((x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1)))


#: How many rows down from his neck his shoulders are, which a pauldron or a shoulder cap covers.
SHOULDER_ROWS = 4

#: How many rows of sleeve over a hand a glove's cuff and a gauntlet's cover.
CUFF_ROWS = 2

#: How many rows up from the hem of his vest his belt can be: it's the row in there with the most
#: of it.
BELT_SEARCH = 6

#: How many rows below the hem of his vest each armour comes down that hangs past it.
HEMS = {"padded": 2, "hide": 2, "chain-shirt": 2, "scale": 3, "chain": 6, "plate": 3}


def cloth(f, cells, ramp, lo=0, hi=None):
    """Cloth over some of his pixels: each the shade of the ramp (from `lo` to `hi`) that's as light
    as he is there, against the rest of that part of him, so it keeps his folds. His outline stays
    his outline."""
    hi = len(ramp) - 1 if hi is None else hi
    out, parts = {}, {}
    for p in cells:
        if f.part[p] == "outline":
            out[p] = OUTLINE
        else:
            parts.setdefault(f.part[p], []).append(p)
    for ps in parts.values():
        ls = [luma(f.img[y][x]) for x, y in ps]
        a, b = min(ls), max(ls)
        for p, l in zip(ps, ls):
            out[p] = ramp[lo + round((l - a) / max(1.0, b - a) * (hi - lo))]
    return out


def hard(cover, cells, ramp, lo=1, hi=None):
    """Something stiff over some of his pixels, lit from the top left across the whole of it
    (from shade `lo` to `hi`), edged dark where it's his outside."""
    hi = len(ramp) - 2 if hi is None else hi
    if not cells:
        return {}
    xs = [x for x, _ in cells]
    ys = [y for _, y in cells]
    x0, x1, y0, y1 = min(xs), max(xs), min(ys), max(ys)
    out = {}
    for x, y in cells:
        if cover.f.part.get((x, y)) == "outline" and cover.edge((x, y)):
            out[(x, y)] = OUTLINE
            continue
        t = 1 - 0.6 * (x - x0) / max(1, x1 - x0) - 0.5 * (y - y0) / max(1, y1 - y0)
        out[(x, y)] = ramp[max(lo, min(hi, lo + round(t * (hi - lo))))]
    return out


def darker(ramp, colour, by=1):
    """A colour of a ramp, a shade or more darker."""
    return ramp[max(0, ramp.index(colour) - by)] if colour in ramp else colour


def lighter(ramp, colour, by=1):
    """A colour of a ramp, a shade or more lighter."""
    return ramp[min(len(ramp) - 1, ramp.index(colour) + by)] if colour in ramp else colour


def chain(cells, ramp):
    """Mail: every other pixel a shade darker, which at this size is the rings."""
    return {(x, y): darker(ramp, c) if (x + y) % 2 else c for (x, y), c in cells.items()}


def scales(cells, ramp, neck):
    """Scales: rows of them two pixels tall and two across, each row set over by one, lit on the
    top left of each scale and dark at its bottom right where the next row overlaps it."""
    out = {}
    for (x, y), c in cells.items():
        row, low = divmod(y - neck, 2)
        left = (x + row) % 2 == 0
        out[(x, y)] = lighter(ramp, c) if left and not low else darker(ramp, c) if low and not left else c
    return out


def hem(f, cover, top_y, bottom, ragged=False):
    """What hangs below the vest to a hem, as cells: the drape from the waist, its last row
    left with every other pixel gone if it's ragged (mail and fur)."""
    cells = {(x, y) for x, y, _, _ in drape(f, top_y, bottom)}
    if ragged:
        cells = {(x, y) for x, y in cells if y < bottom or x % 2 == 0}
    return cells - cover.hands


def belted(f, cover, out, buckle=True):
    """A leather belt across the row his own is on, with a brass buckle facing you."""
    row = [p for p in out if p[1] == cover.belt and out[p] != OUTLINE]
    for p in row:
        out[p] = LEATHER[1] if p[0] % 3 else LEATHER[0]
    if buckle and f.facing == DOWN and row:
        out[(cover.middle, cover.belt)] = REGALIA[3]
        out[(cover.middle + 1, cover.belt)] = REGALIA[1]


def outside(out, f):
    """Edges whatever's drawn outside his own outline in the outline colour, so a skirt of mail
    over his legs has an edge like the rest of him."""
    for (x, y) in list(out):
        if out[(x, y)] == OUTLINE:
            continue
        for n in ((x - 1, y), (x + 1, y), (x, y + 1)):
            if n not in out and n not in f.part:
                out[n] = OUTLINE


def armour(frames, keys):
    """The nine body armours, each over his torso, his sleeves as far as it comes down them, and
    his legs as far as it hangs: padded and a robe in his vest's dye, leather, studded leather and
    hide in leather's keys, and a chain shirt, scale, a mail hauberk and plate in metal's."""
    cloth_ramp = keys["top"]
    for kind in ("padded", "leather", "studded", "hide", "chain-shirt", "scale", "chain", "plate", "robe"):
        layer = Layer()
        for r in range(4):
            for c in range(PEOPLE_COLUMNS):
                f = frames[r][c]
                cover = Cover(f)
                out = dress(kind, f, cover, cloth_ramp)
                outside(out, f)
                for (x, y), colour in out.items():
                    layer.put(r, c, x, y, colour)
        keep("armour-" + kind, layer)


def dress(kind, f, cover, cloth_ramp):
    """One frame of a body armour, as the colour of every pixel it covers."""
    sleeves = cover.shoulders | cover.upper | cover.forearms
    if kind == "padded":
        # A quilted jacket: his vest's dye over his body and sleeves and two rows past the hem,
        # a darker line every third row where it's stitched, and a high collar.
        cells = cover.torso | sleeves | hem(f, cover, f.waist, f.waist + HEMS[kind])
        out = cloth(f, cells & set(f.part), cloth_ramp, 1)
        for p in cells - set(out):
            out[p] = cloth_ramp[2]
        for (x, y), colour in list(out.items()):
            if colour != OUTLINE and (y - f.neck) % 3 == 2:
                out[(x, y)] = darker(cloth_ramp, colour)
            elif colour != OUTLINE and y == f.neck:
                out[(x, y)] = cloth_ramp[3]
        return out
    if kind in ("leather", "studded", "hide"):
        out = cloth(f, cover.torso, LEATHER, 0, 3 if kind != "hide" else 2)
        out.update(hard(cover, cover.shoulders, LEATHER, 2))
        for (x, y) in cover.shoulders:
            if y == f.neck + SHOULDER_ROWS - 1 and out[(x, y)] != OUTLINE:
                out[(x, y)] = LEATHER[1]
        if kind == "studded":
            # Rivets in a diamond pattern, every fourth pixel along every third row.
            for (x, y), colour in list(out.items()):
                row = y - f.neck - 1
                if colour != OUTLINE and row % 3 == 0 and (x + 2 * (row // 3)) % 4 == 0 and not cover.edge((x, y)):
                    out[(x, y)] = METAL[3]
        if kind == "hide":
            collar = {p for p in cover.torso | cover.shoulders if p[1] < f.neck + 2 + p[0] % 2}
            out.update(hard(cover, collar, FUR, 1))
            fur = hem(f, cover, f.waist, f.waist + HEMS[kind], ragged=True)
            out.update(hard(cover, fur, FUR, 0))
        elif f.facing == DOWN:
            # Laced up the front.
            for y in range(f.neck + 1, cover.belt):
                if (cover.middle, y) in out:
                    out[(cover.middle, y)] = LEATHER[0] if y % 2 else LEATHER[3]
        belted(f, cover, out)
        return out
    if kind in ("chain-shirt", "chain", "scale"):
        # A hauberk's sleeves come to the wrist; a shirt's and scale's stop at the elbow.
        arms = sleeves if kind == "chain" else cover.shoulders | cover.upper
        cells = cover.torso | arms | hem(f, cover, f.waist, f.waist + HEMS[kind], ragged=kind != "scale")
        out = hard(cover, cells & set(f.part), METAL, 1)
        for p in cells - set(out):
            out[p] = METAL[2]
        out = scales(out, METAL, f.neck) if kind == "scale" else chain(out, METAL)
        for p in cells & set(f.part):
            if f.part[p] == "outline" and cover.edge(p):
                out[p] = OUTLINE
        belted(f, cover, out)
        return out
    if kind == "plate":
        out = hard(cover, cover.torso, METAL, 1)
        # The breastplate's ridge down the middle facing you, a glint high on the left, and the
        # shadow under it at the waist.
        for (x, y), colour in list(out.items()):
            if colour == OUTLINE:
                continue
            if f.facing in (DOWN, UP) and x == cover.middle and y < cover.belt:
                out[(x, y)] = lighter(METAL, colour)
            if y == cover.belt - 1:
                out[(x, y)] = METAL[1]
        glint = sorted((p for p in out if out[p] != OUTLINE), key=lambda p: p[0] + p[1])[:3]
        for p in glint[1:]:
            out[p] = METAL[4]
        out.update(chain(hard(cover, cover.upper, METAL, 1), METAL))
        out.update(hard(cover, cover.shoulders, METAL, 2))
        out.update(hard(cover, cover.forearms, METAL, 1))
        for (x, y) in cover.shoulders:
            if y == f.neck + SHOULDER_ROWS - 1 and out[(x, y)] != OUTLINE:
                out[(x, y)] = METAL[0]
        for (x, y) in cover.wrists:
            if y == max(yy for _, yy in cover.wrists) and out.get((x, y)) not in (None, OUTLINE):
                out[(x, y)] = REGALIA[2]
        # The tassets: bands of plate over the hips, a dark line between each.
        tassets = hem(f, cover, f.waist, f.waist + HEMS[kind])
        bands = hard(cover, tassets & set(f.part), METAL, 1)
        for p in tassets - set(bands):
            bands[p] = METAL[2]
        for (x, y), colour in bands.items():
            out[(x, y)] = METAL[1] if colour != OUTLINE and (y - f.waist) % 2 else colour
        for (x, y) in cover.torso:
            if y == f.neck and out.get((x, y)) not in (None, OUTLINE):
                out[(x, y)] = REGALIA[2] if x < cover.middle else REGALIA[1]
        belted(f, cover, out, buckle=False)
        return out
    # A robe: his vest's dye over his body and sleeves and down to his ankles, gold at the hem,
    # the cuffs and down the front, and a sash.
    out = cloth(f, cover.torso | sleeves, cloth_ramp, 1)
    for x, y, x0, x1 in drape(f, f.waist, f.bottom - 1):
        if y > f.waist and (x, y) not in cover.hands:
            # Below the waist it's all skirt, folds and all, with nothing of his legs through it.
            lit = x < (x0 + x1) / 2
            out[(x, y)] = OUTLINE if x in (x0, x1) else cloth_ramp[(3 if lit else 2) - ((x - x0) % 3 == 0)]
    for (x, y), colour in list(out.items()):
        if colour == OUTLINE:
            continue
        if y == f.bottom - 1 or (x, y) in cover.wrists and y == max(yy for _, yy in cover.wrists):
            out[(x, y)] = REGALIA[2] if x % 2 else REGALIA[1]
        elif f.facing == DOWN and x == cover.middle and y > f.neck:
            out[(x, y)] = REGALIA[2]
        elif y == cover.belt:
            out[(x, y)] = cloth_ramp[0]
    return out


def helms(frames, hair):
    """The three helms: a leather cap, a full steel helm that hides the hair and the face behind a
    visor, and a silver circlet with a violet stone."""
    cap, full = Layer(), Layer()
    for r in range(4):
        for c in range(PEOPLE_COLUMNS):
            f = frames[r][c]
            top, brim, cx, span = hat_anchor(f, hair, r, c)
            # The cap: a leather skullcap, half an egg from a row under the tips of his hair down to
            # his hairline and a pixel narrower than his hair each side, so it hugs his head
            # rather than sitting on it like a pot. A darker band round the bottom, a seam over the
            # top from the front and behind, and a button.
            rx = (span[1] - span[0] + 1) / 2.0 - CAP_SNUG
            ry = brim - top - CAP_SNUG
            cells = {(x, y) for y in range(top, brim + 1) for x in range(FRAME_W)
                     if ((x - cx) / rx) ** 2 + ((y - brim) / ry) ** 2 <= 1}
            paint(cap, r, c, cells, LEATHER[1:])
            for x in sorted(x for x, y in cells if y == brim):
                cap.put(r, c, x, brim, LEATHER[1] if x < cx else LEATHER[0])
            if r in (DOWN, UP):
                for y in range(min(y for _, y in cells) + 1, brim):
                    cap.put(r, c, round(cx), y, LEATHER[1])
            cap.put(r, c, round(cx), min(y for _, y in cells), LEATHER[4])
            full_helm(f, hair, full, r, c)
    keep("helm-cap", cap)
    keep("helm-full", full)
    circlet(frames, hair, "helm-circlet", METAL[1:], FOCUS)


#: How far inside the tips of his hair a leather cap comes, each side and over the top.
CAP_SNUG = 1.5

#: How much smaller than his hair a great helm is each side, and how far down from the tips of his
#: hair its top is: his hair stands out round his head in tufts, and a helm the size of all of it is
#: a bucket.
HELM_SNUG = 1
HELM_CROWN = 2


def full_helm(f, hair, layer, r, c):
    """A great helm: a round steel dome over the whole head and straight cheeks down to under the
    chin, so nothing of the hair or the face shows. From the front its visor is a dark slot across
    the eyes under a bright brow and a slot down the middle, with breathing holes; from the side
    the slot runs back from the front; from behind a ridge runs down it. A brass rim goes round
    the bottom."""
    top, eyes, cx, span = head_box(f, hair, r, c)
    if r == UP:
        eyes = f.neck - 3
    rx = (span[1] - span[0] + 1) / 2.0 - HELM_SNUG
    crown_y, dome, bottom = top + HELM_CROWN, eyes - 1, f.chin + 1
    cells = set()
    for y in range(crown_y, bottom + 1):
        up = max(0.0, (dome - y) / (dome - crown_y + 0.5))
        half = rx * (1 - up * up) ** 0.5 - (1 if y == bottom else 0)
        if half >= 1:
            cells.update((x, y) for x in range(round(cx - half), round(cx + half)))
    paint(layer, r, c, cells, METAL[1:])
    xs = sorted(x for x, y in cells if y == eyes - 1)
    for x in sorted(x for x, y in cells if y == bottom):
        layer.put(r, c, x, bottom, REGALIA[2] if x < cx else REGALIA[1])
    mid = round(cx) - 1
    if r == DOWN:
        eye_xs = [x for e in f.eyes for x, _ in e] or [mid - 2, mid + 3]
        for x in range(min(eye_xs) - 1, max(eye_xs) + 2):
            layer.put(r, c, x, eyes - 2, METAL[4] if x < cx else METAL[3])
            layer.put(r, c, x, eyes - 1, OUTLINE)
        for y in range(eyes, bottom - 1):
            layer.put(r, c, mid, y, OUTLINE)
        for x in (mid - 3, mid + 3):
            layer.put(r, c, x, bottom - 2, METAL[1])
    elif r in (LEFT, RIGHT):
        front, step = (xs[0], 1) if r == LEFT else (xs[-1], -1)
        for i in range(HELM_SLOT):
            layer.put(r, c, front + i * step, eyes - 2, METAL[4])
            layer.put(r, c, front + i * step, eyes - 1, OUTLINE)
        layer.put(r, c, front + 2 * step, bottom - 2, METAL[1])
        layer.put(r, c, front + 4 * step, bottom - 2, METAL[1])
    for y in range(crown_y + 1, dome if r != UP else bottom):
        if r in (DOWN, UP) and layer.get(r, c, mid, y) not in (None, OUTLINE):
            layer.put(r, c, mid, y, METAL[4] if y < crown_y + 3 else METAL[3])


#: How far back from the front of a great helm its eye slot goes, seen from the side.
HELM_SLOT = 4


def hand_gear(frames):
    """Gloves in leather and steel gauntlets: over his hands, with a cuff over his wrist."""
    for kind in ("gloves", "gauntlets"):
        layer = Layer()
        for r in range(4):
            for c in range(PEOPLE_COLUMNS):
                f = frames[r][c]
                cover = Cover(f)
                if kind == "gloves":
                    out = cloth(f, cover.hands, LEATHER, 1)
                    out.update(hard(cover, cover.wrists, LEATHER, 2))
                else:
                    out = hard(cover, cover.hands, METAL, 1)
                    out.update(hard(cover, cover.wrists, METAL, 1))
                    for hand in clusters([p for p in cover.hands if out[p] != OUTLINE]):
                        out[min(hand, key=lambda p: p[0] + p[1])] = METAL[4]
                for (x, y), colour in out.items():
                    layer.put(r, c, x, y, colour)
        keep(kind, layer)


def boots(frames):
    """Tall leather boots to the middle of the calf with a turned-down cuff, and iron greaves from
    the knee with plated feet."""
    for kind in ("leather", "iron"):
        layer = Layer()
        for r in range(4):
            for c in range(PEOPLE_COLUMNS):
                f = frames[r][c]
                cover = Cover(f)
                knee = f.waist + (f.bottom - f.waist) // 2
                if kind == "leather":
                    cells = {p for p in cover.legs if p[1] >= f.bottom - BOOT_ROWS}
                    out = cloth(f, cells, LEATHER, 0)
                    for (x, y), colour in list(out.items()):
                        if colour == OUTLINE:
                            continue
                        if y == f.bottom - BOOT_ROWS:
                            out[(x, y)] = LEATHER[4] if x % 3 else LEATHER[3]
                        elif y == f.bottom:
                            out[(x, y)] = LEATHER[0]
                else:
                    middle = sum(x for x, _ in cover.legs) / max(1, len(cover.legs))
                    out = {}
                    for side in (True, False):
                        leg = {p for p in cover.legs if p[1] >= knee - 1 and (p[0] < middle) == side}
                        out.update(hard(cover, leg, METAL, 1))
                    for (x, y), colour in list(out.items()):
                        if colour == OUTLINE:
                            continue
                        if y == knee - 1:
                            out[(x, y)] = METAL[4] if x % 2 else METAL[3]
                        elif y == f.bottom - 3:
                            out[(x, y)] = LEATHER[1]
                        elif y >= f.bottom - 1:
                            out[(x, y)] = METAL[2] if y == f.bottom - 1 else METAL[1]
                for (x, y), colour in out.items():
                    layer.put(r, c, x, y, colour)
        keep("boots-" + kind, layer)


#: How many rows up from the soles a leather boot comes.
BOOT_ROWS = 6


# --------------------------------------------------------------------------- #
# Gear you hold
# --------------------------------------------------------------------------- #
# A weapon or a shield is drawn on each body's own frames, all ten, from where the hand holding it
# has got to: in the walk, where his hand is in that frame, and in the fights' frames, where the
# pose map took it. It isn't posed from the standing frame like everything else because it has to
# swing: raised to wind up, brought through to strike, held up to cast. Only lying down is posed,
# so it's still in his hand where he fell. The weapon's in his right hand from the front and
# behind and the near hand from the side; the shield's on the other arm.

#: Wood for hafts, staves, bows and a round shield's planks, dark to light. It never changes colour.
WOOD = ((66, 40, 32), (108, 70, 46), (152, 106, 66), (196, 150, 98))

#: A bowstring and an arrow's shaft, and its fletching.
STRING = (238, 228, 206)
FLETCH = ((150, 40, 48), (232, 96, 86))


class Hold:
    """Where a held thing goes in one frame of one body: which way they face and which frame it is,
    the middle of each hand, the pixels of the weapon hand (the grip goes under them), and every
    pixel of him."""

    def __init__(self, rig, c, mapping):
        self.r, self.c = rig.r, c
        self.ahead, self.side = rig.ahead, rig.side
        moved = {}
        for to, src in mapping.items():
            moved.setdefault(src, []).append(to)

        def hand(arm):
            cells = rig.arms[arm]
            low = max(y for _, y in cells)
            tip = [to for p in cells if p[1] > low - ARM_ENDS for to in moved.get(p, ())]
            mid = (round(sum(x for x, _ in tip) / len(tip) - 0.5), round(sum(y for _, y in tip) / len(tip) - 0.5))
            skin = {to for p in cells if p[1] > low - ARM_ENDS and rig.part[p] == "skin" for to in moved.get(p, ())}
            return mid, skin

        self.hand, weapon = hand("weapon")
        self.other, other = hand("other")
        self.fists = {"weapon": weapon, "other": other}
        self.body = {to for to, src in mapping.items() if src in rig.part}

    @property
    def view(self):
        """Which of the swings' views this frame is: the front, the back or the side."""
        return "side" if self.ahead else "front" if self.r == DOWN else "back"

    @property
    def pose(self):
        """Which frame of a swing this is: the walk counts as standing."""
        return 0 if self.c < PEOPLE_COLUMNS else self.c


def holds(frames, build):
    """Where things are held in each of a body's frames, up to lying down, a list a row."""
    out = []
    for r in range(4):
        row = []
        for c in range(PEOPLE_COLUMNS):
            rig = Rig(frames[r][c], r, build)
            row.append(Hold(rig, c, {p: p for p in rig.part}))
        rig = Rig(frames[r][0], r, build)
        row.extend(Hold(rig, c, pose_map(rig, c)) for c in range(WIND_UP, DOWNED))
        out.append(row)
    return out


class Pen:
    """Draws a held thing into one frame: pixels put down in order, each over the last, then a
    dark outline round them all that keeps off the hand (the grip goes under its fingers) and off
    a bowstring. Something held loosely (a shield on the arm, a bow or a crossbow) is moved in from
    the side of the frame if it'd go off it."""

    def __init__(self, hold):
        self.hold = hold
        self.cells = {}
        self.bare = set()
        self.behind = False
        self.loose = False
        self.fist = hold.fists["weapon"]

    def put(self, p, colour, bare=False):
        """One pixel, unless it's under the hand, or it's behind him this frame and he's there. A
        `bare` one isn't outlined."""
        if p in self.fist or self.behind and p in self.hold.body:
            return
        self.cells[p] = colour
        (self.bare.add if bare else self.bare.discard)(p)

    def line(self, a, b, colour, bare=False):
        """A clean line of pixels from one point to another."""
        (x0, y0), (x1, y1) = a, b
        n = max(abs(x1 - x0), abs(y1 - y0), 1)
        for i in range(n + 1):
            self.put((x0 + math.floor((x1 - x0) * i / n + 0.5), y0 + math.floor((y1 - y0) * i / n + 0.5)), colour, bare)

    def frame(self):
        """The frame it's drawn: the pixels, moved in from the side if it's held loosely, and the
        outline round them. Anything still off the frame is left off."""
        shift = 0
        if self.loose and self.cells:
            left = min(x for x, _ in self.cells) - 1
            right = max(x for x, _ in self.cells) + 1
            shift = -left if left < 0 else FRAME_W - 1 - right if right >= FRAME_W else 0
        cells = {(x + shift, y): colour for (x, y), colour in self.cells.items()}
        bare = {(x + shift, y) for x, y in self.bare}
        fist = {(x + shift, y) for x, y in self.fist}
        img = new_image(FRAME_W, FRAME_H)
        for (x, y), colour in cells.items():
            if 0 <= x < FRAME_W and 0 <= y < FRAME_H:
                img[y][x] = [colour[0], colour[1], colour[2], 255]
        for (x, y) in cells:
            if (x, y) in bare:
                continue
            for n in ((x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1)):
                nx, ny = n
                if 0 <= nx < FRAME_W and 0 <= ny < FRAME_H and n not in cells and n not in fist:
                    img[ny][nx] = [OUTLINE[0], OUTLINE[1], OUTLINE[2], 255]
        return img


def along(h, d, t):
    """The pixel `t` steps from a point in a direction (a step across and down, like (1, 2)), on a
    clean line: every step is one pixel along the longer way."""
    dx, dy = d
    m = max(abs(dx), abs(dy))
    return h[0] + math.floor(t * dx / m + 0.5), h[1] + math.floor(t * dy / m + 0.5)


def offset(p, step, k=1):
    """A point moved `k` steps."""
    return p[0] + step[0] * k, p[1] + step[1] * k


def beside(d):
    """The step to the shaded side of something pointing along d, which makes it two pixels thick
    without a gap: to the right of anything steep, below anything shallow."""
    return (1, 0) if abs(d[1]) >= abs(d[0]) else (0, 1)


def across(d, prefer):
    """The step straight across something pointing along d (the nearest of the eight ways), turned
    to the side `prefer` points to, or down if it's square to that."""
    dx, dy = d
    if abs(dx) == abs(dy):
        a = (1, -1) if dx * dy > 0 else (1, 1)
    else:
        a = (1, 0) if abs(dy) > abs(dx) else (0, 1)
    dot = a[0] * prefer[0] + a[1] * prefer[1]
    if dot < 0 or dot == 0 and a[1] < 0:
        a = (-a[0], -a[1])
    return a


#: Which way a weapon points from the hand in each frame (a step across and down), how much of its
#: length shows (less when it points away from you or at you), and whether it goes behind his
#: head (levelled over the shoulder from the side, where it'd cut his face in half if it went over
#: it). Standing is for the walk too. "front" faces you with the weapon in the hand on your left,
#: "back" faces away with it in the hand on your right, and "side" faces right; facing left is the
#: side in a mirror.
SWINGS = {
    # Swords, axes, maces and hammers: lifted over the shoulder, and down through the foe.
    "cut": {
        "front": {0: ((0, 1), 0.8), WIND_UP: ((0, -1), 0.7), STRIKE: ((1, 1), 1.0), CAST: ((0, -1), 0.7), HURT: ((0, 1), 0.8)},
        "back": {0: ((0, 1), 0.8), WIND_UP: ((1, 2), 1.0), STRIKE: ((-2, -1), 1.0), CAST: ((0, -1), 0.7), HURT: ((0, 1), 0.8)},
        "side": {0: ((1, 2), 1.0), WIND_UP: ((-1, -2), 1.0), STRIKE: ((1, 2), 1.0), CAST: ((1, 2), 1.0), HURT: ((-1, 2), 1.0)},
    },
    # The dagger and the rapier: lifted back over the shoulder, and driven straight in.
    "thrust": {
        "front": {0: ((0, 1), 0.8), WIND_UP: ((0, -1), 0.7), STRIKE: ((0, 1), 0.6), CAST: ((0, -1), 0.7), HURT: ((0, 1), 0.8)},
        "back": {0: ((0, 1), 0.8), WIND_UP: ((1, 2), 1.0), STRIKE: ((0, -1), 0.6), CAST: ((0, -1), 0.7), HURT: ((0, 1), 0.8)},
        "side": {0: ((1, 2), 1.0), WIND_UP: ((-1, -2), 1.0), STRIKE: ((1, 0), 1.0), CAST: ((1, 2), 1.0), HURT: ((-1, 2), 1.0)},
    },
    # A spear: upright, levelled over the shoulder, and thrust. Its head is the end it points.
    "pole": {
        "front": {0: ((0, -1), 1.0), WIND_UP: ((0, -1), 0.6), STRIKE: ((0, 1), 0.6), CAST: ((0, -1), 0.8), HURT: ((-1, -2), 1.0)},
        "back": {0: ((0, -1), 1.0), WIND_UP: ((0, 1), 0.6), STRIKE: ((0, -1), 0.6), CAST: ((0, -1), 0.8), HURT: ((1, -2), 1.0)},
        "side": {0: ((0, -1), 1.0), WIND_UP: ((1, 0), 1.0, True), STRIKE: ((1, 0), 1.0), CAST: ((0, -1), 1.0), HURT: ((-1, -2), 1.0)},
    },
    # A staff: upright, swung back and brought down, and held up high to cast.
    "staff": {
        "front": {0: ((0, -1), 1.0), WIND_UP: ((-1, -2), 0.8), STRIKE: ((1, 1), 1.0), CAST: ((0, -1), 1.0), HURT: ((-1, -2), 1.0)},
        "back": {0: ((0, -1), 1.0), WIND_UP: ((1, -2), 0.8), STRIKE: ((-1, -1), 1.0), CAST: ((0, -1), 1.0), HURT: ((1, -2), 1.0)},
        "side": {0: ((0, -1), 1.0), WIND_UP: ((-1, -1), 1.0), STRIKE: ((1, 1), 1.0), CAST: ((0, -1), 1.0), HURT: ((-1, -2), 1.0)},
    },
}


def fit(h, d, n, beyond, margin):
    """How many steps of something `n` long (and `beyond` more past that, for a head or a tip)
    pointing along d from h fit in the frame, `margin` pixels clear of its sides and top and not
    through the ground. Cut short like that it looks like it's pointing a little towards you or
    away, which is better than its end going off the edge of the frame."""
    while n > 1:
        x, y = along(h, d, n + beyond)
        if margin <= x < FRAME_W - margin and margin <= y <= SOLES:
            break
        n -= 1
    return n


#: How far a blade or a staff's crystal keeps from the edges of the frame (its outline goes on the
#: pixel in between), and an axe's or a hammer's head, which sticks out sideways from its haft.
BLADE_MARGIN = 2
HEAD_MARGIN = 4

#: How far past the end of its haft or shaft each head goes: an axe's, a hammer's and a mace's
#: (with its flanges), a staff's crystal and a spear's head.
HEAD_LENGTHS = {"axe": 2, "hammer": 2, "mace": 5}
CRYSTAL_LENGTH = 3
SPEARHEAD_LENGTH = 4


def swing(style, pen):
    """Which way a weapon points in a frame and the number that goes with it, mirrored for
    facing left. If the weapon goes behind his head in this frame, the pen's told."""
    hold = pen.hold
    (dx, dy), n, *behind = SWINGS[style][hold.view][hold.pose]
    pen.behind = bool(behind)
    return (-dx if hold.ahead < 0 else dx, dy), n


def outward(hold):
    """The way that's away from him for something on the weapon: forward from the side, and out
    past his weapon arm from the front and behind."""
    return (hold.ahead, 0) if hold.ahead else (hold.side, 0)


#: The blades: how many pixels from the guard to the tip, how many thick, how far the guard goes
#: out each side, how long the grip behind the hand is, and whether it curves.
BLADES = {
    "dagger": dict(style="thrust", blade=4, width=1, guard=1, grip=1, curve=False),
    "shortsword": dict(style="cut", blade=7, width=2, guard=1, grip=1, curve=False),
    "longsword": dict(style="cut", blade=9, width=2, guard=2, grip=1, curve=False),
    "rapier": dict(style="thrust", blade=10, width=1, guard=2, grip=1, curve=False),
    "scimitar": dict(style="cut", blade=8, width=2, guard=1, grip=1, curve=True),
    "greatsword": dict(style="cut", blade=12, width=3, guard=3, grip=2, curve=False),
}


def blade(pen, hold, spec):
    """A sword or a dagger: the pommel and grip behind the hand, a brass guard across, and the
    blade, lit along its left or top edge with a glint a third of the way up, and curving back
    near the tip if it's a scimitar."""
    d, scale = swing(spec["style"], pen)
    h = hold.hand
    n = fit(h, d, round(spec["blade"] * scale), 1, BLADE_MARGIN)
    for t in range(1, spec["grip"] + 1):
        pen.put(along(h, d, -t), LEATHER[1])
    pen.put(along(h, d, -spec["grip"] - 1), REGALIA[2])
    a = across(d, outward(hold))
    guard = along(h, d, 1)
    for k in range(-spec["guard"], spec["guard"] + 1):
        pen.put(offset(guard, a, k), REGALIA[3] if k < 0 else REGALIA[2] if k == 0 else REGALIA[1])
    shade = beside(d)
    for t in range(2, n + 2):
        p = along(h, d, t)
        if spec["curve"]:
            p = offset(p, a, -(max(0, t - n + 3) // 2))
        tip = t == n + 1
        pen.put(p, METAL[4] if t == 2 + n // 3 or tip else METAL[3])
        if spec["width"] >= 2 and not tip:
            pen.put(offset(p, shade), METAL[2] if t < n else METAL[1])
        if spec["width"] == 3 and t < n:
            pen.put(offset(p, shade, 2), METAL[1])


#: The hafted weapons: how long the haft is from the hand to where the head goes on.
HAFTS = {"axe": 8, "hammer": 8, "mace": 6}


def solid(h, d, a, inside):
    """The pixels of a shape laid along a weapon, by where the middle of each falls: `inside` is
    asked how many steps along d from h it is and how many pixels out towards a, and says whether
    it's in. Being worked out backwards from the pixels, it's solid whichever way the weapon points."""
    m = max(abs(d[0]), abs(d[1]))
    sx, sy = d[0] / m, d[1] / m
    norm = math.hypot(*a)
    wx, wy = a[0] / norm, a[1] / norm
    det = sx * wy - sy * wx
    cells = set()
    for y in range(h[1] - HEAD_REACH, h[1] + HEAD_REACH + 1):
        for x in range(h[0] - HEAD_REACH, h[0] + HEAD_REACH + 1):
            px, py = x - h[0], y - h[1]
            if inside((px * wy - py * wx) / det, (sx * py - sy * px) / det):
                cells.add((x, y))
    return cells


#: How far from the end of its haft a weapon's head can reach, in pixels.
HEAD_REACH = 6


def lit(cells, ramp, lo, hi):
    """Colours for a solid shape, lit from the top left across it, from shade lo to hi."""
    if not cells:
        return {}
    xs = [x for x, _ in cells]
    ys = [y for _, y in cells]
    x0, y0, w, h = min(xs), min(ys), max(1, max(xs) - min(xs)), max(1, max(ys) - min(ys))
    return {(x, y): ramp[max(lo, min(hi, lo + round((1 - 0.55 * (x - x0) / w - 0.55 * (y - y0) / h) * (hi - lo))))]
            for x, y in cells}


def haft(pen, h, d, start, end, ramp):
    """A haft two pixels thick from `start` steps along from the hand to `end`, with a leather grip
    behind the hand."""
    shade = beside(d)
    for t in range(start, end + 1):
        p = along(h, d, t)
        pen.put(p, LEATHER[2] if t < 0 else ramp[3] if t % 4 else ramp[2])
        pen.put(offset(p, shade), LEATHER[1] if t < 0 else ramp[1])


def hafted(pen, hold, kind):
    """An axe, a warhammer or a mace: a wooden haft (iron for the mace) with a leather grip, and
    its head on the end: an axe's broad blade out to the front with a bright edge, a hammer's
    block across, a mace's flanged ball."""
    d, scale = swing("cut", pen)
    h = hold.hand
    n = fit(h, d, round(HAFTS[kind] * scale), HEAD_LENGTHS[kind], HEAD_MARGIN)
    haft(pen, h, d, -2, n, METAL if kind == "mace" else WOOD)
    a = across(d, outward(hold))
    end = along(h, d, n)
    if kind == "axe":
        def reach(t):
            return AXE_EDGE - 0.4 * (t + 0.5) ** 2
        blade = solid(end, d, a, lambda t, k: -3 <= t <= 1.5 and 0.5 <= k <= reach(t))
        poll = solid(end, d, a, lambda t, k: -1 <= t <= 0.5 and -1.6 <= k <= -0.5)
        for p, colour in lit(blade | poll, METAL, 1, 3).items():
            pen.put(p, colour)
        for p in solid(end, d, a, lambda t, k: -3 <= t <= 1.5 and reach(t) - 1.1 < k <= reach(t)):
            pen.put(p, METAL[4])
    elif kind == "hammer":
        head = solid(end, d, a, lambda t, k: -0.5 <= t <= 1.6 and -2.6 <= k <= 2.6)
        for p, colour in lit(head, METAL, 1, 3).items():
            pen.put(p, colour)
        for p in solid(end, d, a, lambda t, k: -0.5 <= t <= 1.6 and 1.6 < k <= 2.6):
            pen.put(p, METAL[4])
    else:
        middle = MACE_BALL + 0.3
        ball = solid(end, d, a, lambda t, k: (t - middle) ** 2 + k * k <= MACE_BALL ** 2
                     or abs(t - middle) <= 0.6 and abs(k) <= MACE_BALL + 1
                     or abs(k) <= 0.6 and abs(t - middle) <= MACE_BALL + 1)
        for p, colour in lit(ball, METAL, 1, 4).items():
            pen.put(p, colour)


#: How far out from its haft an axe's edge comes at its widest, and how big round a mace's ball is.
AXE_EDGE = 3.8
MACE_BALL = 1.9

#: Where a staff and a spear are held in each frame, as how many steps of it are below the hand:
#: in the middle to stand, cast and get hurt, near the foot to swing a staff or thrust a spear, so
#: all of it's out in front.
GRIPS = {
    "staff": {0: 9, WIND_UP: 3, STRIKE: 3, CAST: 6, HURT: 9},
    "pole": {0: 8, WIND_UP: 7, STRIKE: 15, CAST: 8, HURT: 8},
}

#: How long a staff is, foot to crystal, and a spear, butt to the bottom of its head.
STAFF_LENGTH = 20
SPEAR_LENGTH = 20


def held_at(style, hold, d, scale, length):
    """How far a pole goes below and above the hand in a frame: where it's gripped, the rest above,
    and never through the ground."""
    below = GRIPS[style][hold.pose]
    above = round((length - below) * scale)
    while below > 0 and along(hold.hand, d, -below)[1] > SOLES:
        below -= 1
    return below, above


def staff(pen, hold):
    """A staff: two pixels of wood through the hand, an iron shoe at the foot that never goes
    through the ground, and a violet crystal in a wooden crook at the top."""
    d, scale = swing("staff", pen)
    h = hold.hand
    below, above = held_at("staff", hold, d, scale, STAFF_LENGTH)
    above = fit(h, d, above, CRYSTAL_LENGTH, BLADE_MARGIN)
    shade = beside(d)
    for t in range(-below, above + 1):
        p = along(h, d, t)
        pen.put(p, WOOD[3] if t % 4 else WOOD[2])
        pen.put(offset(p, shade), WOOD[1])
    pen.put(along(h, d, -below), METAL[2])
    pen.put(offset(along(h, d, -below), shade), METAL[1])
    top = along(h, d, above + 1)
    for k, colour in ((0, FOCUS[2]), (1, FOCUS[1]), (2, FOCUS[0])):
        pen.put(along(top, d, k), colour)
        pen.put(offset(along(top, d, k), shade), FOCUS[0] if k else FOCUS[1])
    pen.put(offset(top, shade, -1), WOOD[3])
    pen.put(offset(along(top, d, 1), shade, 2), WOOD[2])


def spear(pen, hold):
    """A spear: a long ash shaft through the hand, a leather binding and a leaf-shaped steel head."""
    d, scale = swing("pole", pen)
    h = hold.hand
    below, above = held_at("pole", hold, d, scale, SPEAR_LENGTH)
    above = fit(h, d, above, SPEARHEAD_LENGTH, BLADE_MARGIN + 1)
    for t in range(-below, above + 1):
        pen.put(along(h, d, t), WOOD[3] if t % 3 else WOOD[2])
    pen.put(along(h, d, above), LEATHER[1])
    a = across(d, outward(hold))
    head = along(h, d, above + 1)
    pen.put(head, METAL[1])
    for t in (1, 2):
        p = along(head, d, t)
        pen.put(p, METAL[3])
        pen.put(offset(p, a, 1), METAL[2])
        pen.put(offset(p, a, -1), METAL[4])
    pen.put(along(head, d, 3), METAL[4])


#: Where a bow or a crossbow is in each frame: which way the shot goes, how far the string's drawn
#: back (-1 for a bow carried with nothing on it; for a crossbow, 1 cocked with a bolt on and 0
#: loosed), and which hand it's in. It's carried in the other hand and taken in the weapon hand
#: to shoot; winding up, the weapon hand's up behind the head taking an arrow or a bolt from the
#: quiver on the back.
SHOTS = {
    "bow": {
        "front": {0: ((1, 0), -1, "other"), WIND_UP: ((1, 0), -1, "other"), STRIKE: ((0, 1), 4, "weapon"),
                  CAST: ((1, 0), -1, "other"), HURT: ((1, 0), -1, "other")},
        "back": {0: ((-1, 0), -1, "other"), WIND_UP: ((-1, 0), -1, "other"), STRIKE: ((0, -1), 4, "weapon"),
                 CAST: ((-1, 0), -1, "other"), HURT: ((-1, 0), -1, "other")},
        "side": {0: ((1, 0), -1, "other"), WIND_UP: ((1, 0), -1, "other"), STRIKE: ((1, 0), 4, "weapon"),
                 CAST: ((1, 0), -1, "other"), HURT: ((1, 0), -1, "other")},
    },
    "crossbow": {
        "front": {0: ((0, 1), 1, "other"), WIND_UP: ((0, 1), 0, "other"), STRIKE: ((0, 1), 1, "weapon"),
                  CAST: ((0, 1), 1, "other"), HURT: ((0, 1), 1, "other")},
        "back": {0: ((0, 1), 1, "other"), WIND_UP: ((0, 1), 0, "other"), STRIKE: ((0, -1), 1, "weapon"),
                 CAST: ((0, 1), 1, "other"), HURT: ((0, 1), 1, "other")},
        "side": {0: ((1, 2), 1, "other"), WIND_UP: ((1, 2), 0, "other"), STRIKE: ((1, 0), 1, "weapon"),
                 CAST: ((1, 2), 1, "other"), HURT: ((1, 2), 1, "other")},
    },
}

#: Which way the arrow or bolt in the weapon hand points while it's taken from the quiver: up, and
#: back over the shoulder from the side.
NOCKING = {"front": (0, -1), "back": (0, -1), "side": (-1, -1)}


def shot(kind, pen):
    """Where a bow or a crossbow is in a frame: which way it shoots (mirrored facing left), the
    number that goes with it, and the hand it's in, which the pen's told to keep off. In the far
    hand from the side it's behind him. Winding up, the arrow or bolt in the weapon hand is drawn
    too."""
    hold = pen.hold
    (dx, dy), n, hand = SHOTS[kind][hold.view][hold.pose]
    if hold.pose == WIND_UP:
        quiver(pen, hold, kind)
    pen.fist = hold.fists[hand]
    pen.behind = bool(hold.ahead) and hand == "other"
    pen.loose = True
    return (-dx if hold.ahead < 0 else dx, dy), n, hold.hand if hand == "weapon" else hold.other


def quiver(pen, hold, kind):
    """The arrow or bolt in the weapon hand, just taken from the quiver: its fletching under the
    fist and its head up."""
    dx, dy = NOCKING[hold.view]
    d = (-dx if hold.ahead < 0 else dx, dy)
    length = ARROW_LENGTH if kind == "bow" else ARROW_LENGTH - 2
    for t in range(-1, length):
        pen.put(along(hold.hand, d, t), FLETCH[1] if t < 0 else STRING, bare=t >= 0)
    pen.put(along(hold.hand, d, length), METAL[3])


#: How long an arrow is in the hand, not counting its head; a bolt's two shorter.
ARROW_LENGTH = 6


def bow(pen, hold):
    """A bow: two wooden limbs bending back from a leather grip in the hand, and its string;
    drawn, the string comes back to a point with an arrow on it, red-fletched and steel-headed."""
    aim, draw, h = shot("bow", pen)
    a = across(aim, (0, 1)) if aim[0] == 0 else (0, 1)
    limbs = [offset(offset(h, a, s), aim, -round(BOW_BEND * (s / BOW_HALF) ** 2)) for s in range(-BOW_HALF, BOW_HALF + 1)]
    nock = offset(h, aim, -BOW_BEND - max(0, draw))
    if draw < 0:
        pen.line(limbs[0], limbs[-1], STRING, bare=True)
    else:
        pen.line(limbs[0], nock, STRING, bare=True)
        pen.line(nock, limbs[-1], STRING, bare=True)
    for s, p in zip(range(-BOW_HALF, BOW_HALF + 1), limbs):
        pen.put(p, LEATHER[1] if abs(s) <= 1 else WOOD[1] if abs(s) == BOW_HALF else WOOD[3] if s < 0 else WOOD[2])
    if draw >= 0:
        for t in range(0, draw + BOW_BEND + 3):
            pen.put(offset(nock, aim, t), STRING, bare=True)
        pen.put(offset(nock, aim, BOW_BEND + draw + 3), METAL[3])
        pen.put(nock, FLETCH[1])
        pen.put(offset(nock, aim, 1), FLETCH[0])


#: Half a bow's height, and how far its tips bend back from the grip.
BOW_HALF = 7
BOW_BEND = 2


def crossbow(pen, hold):
    """A light crossbow: a wooden stock along the shot, a steel bow across its front end, the
    string drawn back to the nut with a bolt on it, or slack once it's loosed."""
    aim, loaded, h = shot("crossbow", pen)
    shade = beside(aim)
    for t in range(-CROSSBOW_STOCK, CROSSBOW_STOCK):
        p = along(h, aim, t)
        pen.put(p, WOOD[3] if t > 1 - CROSSBOW_STOCK else WOOD[2])
        pen.put(offset(p, shade), WOOD[1])
    a = across(aim, (0, -1))
    front = along(h, aim, CROSSBOW_STOCK)
    ends = []
    for k in range(-CROSSBOW_PROD, CROSSBOW_PROD + 1):
        # The bow's ends sweep back a pixel.
        p = offset(along(front, aim, -1 if abs(k) >= CROSSBOW_PROD - 1 else 0), a, k)
        pen.put(p, METAL[3] if k < 0 else METAL[2])
        if abs(k) == CROSSBOW_PROD:
            ends.append(p)
    nut = along(h, aim, 0 if loaded else CROSSBOW_STOCK - 1)
    pen.line(ends[0], nut, STRING, bare=True)
    pen.line(nut, ends[1], STRING, bare=True)
    if loaded:
        for t in range(0, BOLT_LENGTH + 1):
            pen.put(along(h, aim, t), STRING if t < BOLT_LENGTH else METAL[4], bare=t < BOLT_LENGTH)


#: How far a crossbow's stock goes behind and in front of the hand, how far its bow goes out each
#: side, and how long a bolt on it is, not counting the head.
CROSSBOW_STOCK = 4
CROSSBOW_PROD = 4
BOLT_LENGTH = 5


def shield(pen, hold, kind, field):
    """A shield on the other arm: a round one of wooden planks with a steel rim and boss, or a
    heater with a steel rim and its face in his vest's dye, a brass stud at its top. From the front
    you see its face, from behind its back and the strap, and from the side its edge, out in front
    of him."""
    ox, oy = hold.other
    pen.loose = True
    if hold.ahead:
        cx, cy = ox + hold.ahead * SHIELD_AHEAD, oy - SHIELD_RISE
        # Edge on: its face out in front, the rim, and its back. A heater's a row longer and
        # comes to a point, which is only the rim.
        bottom = SHIELD_HALF + (1 if kind == "heater" else 0)
        for dy in range(-SHIELD_HALF, bottom + 1):
            narrow = kind == "heater" and dy >= bottom - 2
            for k, colour in ((0, METAL[2]), (1, field[2] if kind == "heater" else WOOD[2]), (-1, WOOD[0])):
                if narrow and k:
                    continue
                pen.put((cx + hold.ahead * k, cy + dy), METAL[3] if dy == -SHIELD_HALF and k == 0 else colour)
        if kind == "round":
            pen.put((cx + hold.ahead * 2, cy), METAL[3])
        return
    cx = max(SHIELD_CLEAR, min(FRAME_W - 1 - SHIELD_CLEAR, ox))
    cy = oy - SHIELD_RISE
    if kind == "round":
        reach = range(-SHIELD_HALF - 1, SHIELD_HALF + 2)
        cells = {(cx + dx, cy + dy) for dx in reach for dy in reach if dx * dx + dy * dy <= SHIELD_RADIUS ** 2}
        rim = {p for p in cells if (p[0] - cx) ** 2 + (p[1] - cy) ** 2 > (SHIELD_RADIUS - 1) ** 2}
    else:
        cells = {(cx + dx, cy - SHIELD_HALF + i) for i, w in enumerate(HEATER) for dx in range(-w, w)}
        rim = {p for p in cells if any(n not in cells for n in ((p[0] - 1, p[1]), (p[0] + 1, p[1]), (p[0], p[1] - 1), (p[0], p[1] + 1)))}
    xs = [x for x, _ in cells]
    ys = [y for _, y in cells]
    front = hold.r == DOWN
    for x, y in cells:
        t = 1 - 0.55 * (x - min(xs)) / (max(xs) - min(xs)) - 0.55 * (y - min(ys)) / (max(ys) - min(ys))
        if (x, y) in rim:
            colour = METAL[max(1, min(4, 1 + round(t * 3)))]
        elif kind == "round":
            plank = WOOD[max(0, min(3, round(t * 3) - (0 if front else 1)))]
            colour = WOOD[0] if (x - cx) % 3 == 1 else plank
        else:
            colour = field[max(1, min(4, 1 + round(t * 3)))] if front else METAL[max(0, min(2, round(t * 2)))]
        pen.put((x, y), colour)
    if front and kind == "round":
        pen.put((cx, cy), METAL[3])
        pen.put((cx - 1, cy - 1), METAL[4])
        pen.put((cx - 1, cy), METAL[3])
        pen.put((cx, cy - 1), METAL[3])
        pen.put((cx + 1, cy + 1), METAL[1])
    elif front:
        pen.put((cx - 1, cy - 2), REGALIA[3])
        pen.put((cx, cy - 2), REGALIA[1])
        for y in range(cy - 1, cy + 4):
            pen.put((cx - 1, y), REGALIA[2])
    else:
        for x in range(cx - 3, cx + 3):
            pen.put((x, cy), LEATHER[1] if x % 2 else LEATHER[2])


#: Where a shield sits on its arm: how many rows up from the hand its middle is, how far out in
#: front of the hand it is from the side, and how big round a round one is.
SHIELD_RISE = 3
SHIELD_AHEAD = 2
SHIELD_RADIUS = 4.6

#: How many rows a shield goes up and down from its middle.
SHIELD_HALF = 4

#: How far a heater shield goes out each side of its middle on each row, top to bottom: square at
#: the top, coming to a point.
HEATER = (4, 4, 4, 4, 4, 3, 3, 2, 1)

#: How far the middle of a shield faced on keeps from the sides of the frame, so the stout body's
#: arm, out at the side, doesn't carry half of it off the edge.
SHIELD_CLEAR = 6


def held_gear(keys):
    """Every weapon and shield, by sheet name, as what draws it into a frame."""
    gear = {"weapon-" + name: (lambda pen, hold, spec=spec: blade(pen, hold, spec)) for name, spec in BLADES.items()}
    gear.update({"weapon-" + kind: (lambda pen, hold, kind=kind: hafted(pen, hold, kind)) for kind in HAFTS})
    gear["weapon-staff"] = staff
    gear["weapon-spear"] = spear
    gear["weapon-bow"] = bow
    gear["weapon-crossbow"] = crossbow
    gear["shield-round"] = lambda pen, hold: shield(pen, hold, "round", keys["top"])
    gear["shield-heater"] = lambda pen, hold: shield(pen, hold, "heater", keys["top"])
    return gear


# --------------------------------------------------------------------------- #
# Saving
# --------------------------------------------------------------------------- #

#: The sheets that hang behind him, which his arms don't take with them when they move.
BEHIND = ("cape-back", "tail-ember-back", "tail-wolf-back")

#: What's held, by sheet name, as what draws it into a frame. held_gear fills it in.
HELD = {}


def save_builds(frames, wanted=()):
    """Every sheet, for every body, or only the ones named: the walk made into that body and the
    fights' frames posed from its standing frame, or for something held, every frame drawn from
    where the hand is but lying down, which is posed."""
    for build in BUILDS:
        folder, torso, legs, wider = build
        rigs = [Rig(frames[r][0], r, build) for r in range(4)]
        maps = {(r, c, behind): pose_map(rigs[r], c, behind)
                for r in range(4) for c in range(WIND_UP, COLUMNS) for behind in (False, True)}
        grips = holds(frames, build)
        for name in list(SHEETS) + list(HELD):
            if wanted and name not in wanted:
                continue
            sheet = new_image(FRAME_W * COLUMNS, FRAME_H * 4)
            for r in range(4):
                for c in range(COLUMNS):
                    if name in HELD and c < DOWNED:
                        pen = Pen(grips[r][c])
                        HELD[name](pen, grips[r][c])
                        frame = pen.frame()
                    elif c < PEOPLE_COLUMNS:
                        frame = SHEETS[name].frames[r][c]
                        if torso or legs or wider:
                            frame = built(frame, frames[r][c], torso, legs, wider)
                    else:
                        frame = posed(standing, maps[(r, c, name in BEHIND)])
                    if c == 0:
                        standing = frame
                    for y in range(FRAME_H):
                        sheet[r * FRAME_H + y][c * FRAME_W:(c + 1) * FRAME_W] = frame[y]
            write_png(os.path.join(OUT, folder, name + ".png"), sheet)


# --------------------------------------------------------------------------- #
# The palettes
# --------------------------------------------------------------------------- #

def palettes(keys):
    """character-palettes.png: a row per preset, the keys first in each block, dark to light. A
    ramp shorter than the widest stops at a clear pixel."""
    rows = []
    for name, _ in SHADES:
        rows.append(keys[name])
        for anchors in PRESETS[name]:
            rows.append(preset_ramp(keys[name], anchors))
    width = max(len(r) for r in rows)
    img = new_image(width, len(rows))
    for y, ramp in enumerate(rows):
        for x, c in enumerate(ramp):
            img[y][x] = [c[0], c[1], c[2], 255]
    write_png(os.path.join(OUT, "character-palettes.png"), img)


def gear_palettes(keys):
    """gear-palettes.png: metal's keys and then each of its tints, a row each, dark to light, and
    the same for leather. Paperdoll swaps a gear sheet's keys for a tint's row. Every key has to be
    a colour nothing else on any sheet is, or the swap would catch that too, so that's checked
    first."""
    others = [c for ramp in keys.values() for c in ramp]
    others += [OUTLINE, *GOLD, *HORN, *EMBER_TAIL, *EMBER_TIP, *STEEL, *TRICORN, *TRIM, *REGALIA, *RUBY,
               *SAPPHIRE, *FUR, *FOCUS, *WOOD, STRING, *FLETCH]
    gear_keys = [c for _, ramp, _ in TINTS for c in ramp]
    clash = [c for i, c in enumerate(gear_keys) if c in others or c in gear_keys[:i]]
    if clash:
        sys.exit("Gear keys %s are colours something else already uses." % clash)
    rows = []
    for _, ramp, tints in TINTS:
        rows.append(ramp)
        rows.extend(preset_ramp(ramp, anchors) for anchors in tints)
    img = new_image(len(rows[0]), len(rows))
    for y, ramp in enumerate(rows):
        for x, c in enumerate(ramp):
            img[y][x] = [c[0], c[1], c[2], 255]
    write_png(os.path.join(OUT, "gear-palettes.png"), img)


if __name__ == "__main__":
    wanted = sys.argv[1:]
    print("Generating the paperdoll for Legend Quest...")
    frames = youth()
    keys, shade_of = make_keys(frames)
    body, hair = body_and_hair(frames, keys, shade_of)
    keep("body", body)
    hair_styles(frames, hair, keys)
    bald(frames, hair, keys)
    faces(frames, keys, shade_of)
    beards(frames, keys, shade_of)
    skirt(frames, keys)
    cape(frames, keys)
    ears(frames, hair, keys)
    horns(frames, hair)
    tails(frames, keys)
    hats(frames, hair, keys)
    armour(frames, keys)
    helms(frames, hair)
    hand_gear(frames)
    boots(frames)
    HELD.update(held_gear(keys))
    for name in wanted:
        if name not in SHEETS and name not in HELD:
            sys.exit("No sheet called '%s'." % name)
    save_builds(frames, wanted)
    if not wanted:
        palettes(keys)
        gear_palettes(keys)
    print("Done.")
