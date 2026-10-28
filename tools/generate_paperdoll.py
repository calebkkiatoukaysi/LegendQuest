"""
The paperdoll: the youth off my townsperson sheet, taken apart so anyone can be made out of him.

    python tools/generate_paperdoll.py

He's the best-drawn walker I have (the biggest on his sheet, so the cleanest when he's sampled
down), so he's the body everyone in the party is made from. Every pixel of him gets sorted into what
it's part of: skin, hair, eyes, shirt, the vest over it, the breeches, or something that never
changes colour (his outline, his belt, his boots, the shine in his eyes). Each recolourable part gets
cut down to a few shades, dark to light, and those shades are the keys the palette swap looks for,
the same as The Black Box's CharacterPalettes. Then the hair comes off him onto a layer of its own,
and the other hair, the faces, the beards, the skirt and the cape get drawn onto his frames from
where his head, eyes, chin, waist and legs turn out to be in each one.

Every sheet is his layout: 32x48 frames, standing and then four steps across, a row for each way he
faces (down, left, right, up). character-palettes.png has every preset a part can be swapped for, a
row each, dark to light; the first row of each block is the keys themselves, which is him as drawn.

Only needs CPython. There's nothing random in here, so a rerun writes the same bytes.
"""

import colorsys
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from pixels import OUT, new_image, write_png  # noqa: E402
from cut_supplied import FRAME_H, FRAME_W, PEOPLE_COLUMNS, frame_of, grid  # noqa: E402

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
    """A sheet being built: a frame's worth of pixels for each of his frames."""

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

    def save(self, rel):
        sheet = new_image(FRAME_W * PEOPLE_COLUMNS, FRAME_H * 4)
        for r in range(4):
            for c in range(PEOPLE_COLUMNS):
                for y in range(FRAME_H):
                    sheet[r * FRAME_H + y][c * FRAME_W:(c + 1) * FRAME_W] = self.frames[r][c][y]
        write_png(os.path.join(OUT, rel), sheet)
        return sheet


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
        layer.save("Paperdoll/hair-" + style + ".png")


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
        layer.save("Paperdoll/face-" + face + ".png")


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
        layer.save("Paperdoll/beard-" + beard + ".png")


# --------------------------------------------------------------------------- #
# The skirt and the cape
# --------------------------------------------------------------------------- #

def skirt(frames, keys):
    """A long skirt in the breeches' keys, from the waist to above the boots, flaring a pixel every
    few rows and as wide as his stride. It goes over his legs and under his hands."""
    ramp = keys["bottoms"]
    layer = Layer()
    for r in range(4):
        for c in range(PEOPLE_COLUMNS):
            f = frames[r][c]
            top_y, hem = f.waist - 1, f.bottom - 3
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
                    if f.part.get((x, y)) == "skin":
                        continue
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
    layer.save("Paperdoll/skirt.png")


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
    back.save("Paperdoll/cape-back.png")
    front.save("Paperdoll/cape-front.png")


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


if __name__ == "__main__":
    print("Generating the paperdoll for Legend Quest...")
    frames = youth()
    keys, shade_of = make_keys(frames)
    body, hair = body_and_hair(frames, keys, shade_of)
    body.save("Paperdoll/body.png")
    hair_styles(frames, hair, keys)
    faces(frames, keys, shade_of)
    beards(frames, keys, shade_of)
    skirt(frames, keys)
    cape(frames, keys)
    palettes(keys)
    print("Done.")
