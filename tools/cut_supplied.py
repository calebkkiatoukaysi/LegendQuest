"""
Cuts the pictures I made with ChatGPT (everything in tools/source/) into the game's layouts: the
title's layers and loops, Questeria's props and buildings, and the townsfolk's walking sheets.

    python tools/cut_supplied.py                       # everything
    python tools/cut_supplied.py title                 # just the title
    python tools/cut_supplied.py questeria             # just Questeria's props and buildings
    python tools/cut_supplied.py buildings             # just the buildings put together from parts
    python tools/cut_supplied.py people                # just the townsfolk
    python tools/cut_supplied.py fish-stall townhouse  # just these props, by name

The same idea as The Black Box's serenity-walker. Each thing gets found on its sheet, the flat dark
backdrop gets keyed out by flooding in from the edge of its box (so a dark pixel inside it, like an
outline, stays), and it's sampled down to art pixels by taking the middle colour of each cell, which
keeps the pixel art's own colours instead of blurring them together. The pictures aren't on a real
pixel grid (I measured, and there's no pitch to find), so every cut says how big it should come out.

Only needs CPython. There's nothing random in here, so a rerun writes the same bytes.
"""

import colorsys
import math
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from pixels import OUT, ROOT, hash01, new_image, read_png, write_png  # noqa: E402

SOURCE = os.path.join(ROOT, "tools", "source")

#: The backdrop: a dark, neutral grey, never quite the same twice. Anything brighter than BG_HI in
#: a channel, or with its channels further apart than BG_TINT, is part of the picture.
BG_HI = 28
BG_TINT = 8

#: How much of a cell has to be picture, not backdrop, for the cell to be drawn at all.
KEEP = 0.5

_pictures = {}


def picture(name):
    """A picture out of tools/source, read once however many cuts in a row come out of it. Only one
    is kept at a time (they're a million and a half pixels each), so the cuts go a sheet at a time."""
    if name not in _pictures:
        _pictures.clear()
        w, h, ch, rows = read_png(os.path.join(SOURCE, name))
        _pictures[name] = (w, h, ch, rows)
    return _pictures[name]


def backdrop(r, g, b, dark=0):
    """Whether a pixel is backdrop: dark and grey, or darker than `dark` whatever its colour (for
    the glow round something lit, which comes out as a dark tinted haze on the backdrop)."""
    top = max(r, g, b)
    return top <= dark or top <= BG_HI and top - min(r, g, b) <= BG_TINT


def keyed(name, box, dark=0, holes=False):
    """Which pixels of a box are picture: everything the backdrop can't reach from the box's edge.
    With holes, a patch of backdrop the edge can't reach goes too, if it's big enough to be a gap in
    the picture rather than a dark pixel of it (the sky between a balustrade's balusters). Returns
    one bytearray a row, 1 for picture."""
    w, h, ch, rows = picture(name)
    x0, y0, bw, bh = box
    outside = [bytearray(bw) for _ in range(bh)]
    stack = [(x, 0) for x in range(bw)] + [(x, bh - 1) for x in range(bw)]
    stack += [(0, y) for y in range(bh)] + [(bw - 1, y) for y in range(bh)]
    while stack:
        x, y = stack.pop()
        if outside[y][x]:
            continue
        row, i = rows[y0 + y], (x0 + x) * ch
        if not backdrop(row[i], row[i + 1], row[i + 2], dark):
            continue
        outside[y][x] = 1
        if x > 0 and not outside[y][x - 1]:
            stack.append((x - 1, y))
        if x < bw - 1 and not outside[y][x + 1]:
            stack.append((x + 1, y))
        if y > 0 and not outside[y - 1][x]:
            stack.append((x, y - 1))
        if y < bh - 1 and not outside[y + 1][x]:
            stack.append((x, y + 1))
    if holes:
        clear_holes(name, box, outside, dark)
    return [bytearray(1 - v for v in line) for line in outside]


#: How many sheet pixels a patch of backdrop inside a picture has to be to count as a hole in it.
HOLE = 150


def clear_holes(name, box, outside, dark):
    """Marks every patch of backdrop the flood from the edge didn't reach as outside too, if it's
    at least HOLE pixels."""
    w, h, ch, rows = picture(name)
    x0, y0, bw, bh = box
    seen = [bytearray(bw) for _ in range(bh)]
    for sy in range(bh):
        for sx in range(bw):
            if outside[sy][sx] or seen[sy][sx]:
                continue
            row, i = rows[y0 + sy], (x0 + sx) * ch
            if not backdrop(row[i], row[i + 1], row[i + 2], dark):
                continue
            patch, stack = [], [(sx, sy)]
            seen[sy][sx] = 1
            while stack:
                x, y = stack.pop()
                patch.append((x, y))
                for nx, ny in ((x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1)):
                    if 0 <= nx < bw and 0 <= ny < bh and not seen[ny][nx] and not outside[ny][nx]:
                        r2, j = rows[y0 + ny], (x0 + nx) * ch
                        if backdrop(r2[j], r2[j + 1], r2[j + 2], dark):
                            seen[ny][nx] = 1
                            stack.append((nx, ny))
            if len(patch) >= HOLE:
                for x, y in patch:
                    outside[y][x] = 1


def sample(name, box, size, key=True, keep=KEEP, dark=0, mask=None, holes=False):
    """A box of a picture sampled down to size (art pixels wide and tall): each cell is the middle
    colour, by brightness, of the picture pixels in it, or clear if the cell is mostly backdrop.
    Which pixels are picture comes from keying the box, unless it's handed a mask already."""
    w, h, ch, rows = picture(name)
    x0, y0, bw, bh = box
    tw, th = size
    if mask is None and key:
        mask = keyed(name, box, dark, holes)
    sx, sy = bw / tw, bh / th
    img = new_image(tw, th)
    for ty in range(th):
        ya, yb = int(ty * sy), max(int(ty * sy) + 1, int((ty + 1) * sy))
        for tx in range(tw):
            xa, xb = int(tx * sx), max(int(tx * sx) + 1, int((tx + 1) * sx))
            mine, total = [], 0
            for y in range(ya, yb):
                row, line = rows[y0 + y], mask[y] if mask else None
                for x in range(xa, xb):
                    total += 1
                    if line is not None and not line[x]:
                        continue
                    i = (x0 + x) * ch
                    mine.append((row[i] + row[i + 1] + row[i + 2], row[i], row[i + 1], row[i + 2]))
            if mine and len(mine) >= keep * total:
                mine.sort()
                _, r, g, b = mine[len(mine) // 2]
                img[ty][tx] = [r, g, b, 255]
    return img


def paste(dst, src, ox, oy):
    """Copies the drawn pixels of one image onto another."""
    for y, line in enumerate(src):
        for x, px in enumerate(line):
            if px[3] and 0 <= oy + y < len(dst) and 0 <= ox + x < len(dst[0]):
                dst[oy + y][ox + x] = list(px)


def save(img, rel):
    write_png(os.path.join(OUT, rel), img)


# --------------------------------------------------------------------------- #
# The title: three layers back to front, the clouds and the island that drift and bob between
# them, and three loops
# --------------------------------------------------------------------------- #
# Every layer is sampled to the same canvas, TITLE_W x TITLE_H art pixels, and drawn at 4x. That's
# a little wider than the window (400 art pixels), so the layers can sway against each other
# without showing an edge. TitleBackgroundScreen has the same numbers. Everything else is cut to
# whatever size looks right on it; an art pixel is four screen pixels whatever it's cut from.

TITLE_W, TITLE_H = 416, 277
TITLE_SOURCE_BOX = (0, 0, 1536, 1024)

#: The layers, back to front: the file it's cut from, and whether it has a backdrop to key out.
#: title-far.png isn't a layer: the city covers all of it but the islands in its sky, so its
#: biggest island gets cut out on its own instead and floats where there's sky to see it in.
TITLE_LAYERS = (
    ("sky", "title-sky.png", False),
    ("city", "title-city.png", True),
    ("foreground", "title-foreground.png", True),
)

#: The clouds: how many there are on their sheet, the cell each one goes in, and the sheet pixels
#: an art pixel takes (the same as the layers, so they're the size they were painted).
CLOUDS = 6
CLOUD_W, CLOUD_H = 132, 60
CLOUD_PITCH = 1536 / TITLE_W

#: How wide the floating island comes out: the biggest one off title-far.png's sky, waterfall and
#: all. There are three more, but the trees and the castle hid them, and the sky's busy enough.
ISLAND_WIDTH = 72

#: The loops: the file, how many frames along it, the sheet pixels an art pixel takes, the frame's
#: size in art pixels, what each frame is lined up by so the thing doesn't jump about between
#: frames and how far across the frame that goes, and how dark a tinted pixel can be and still be
#: backdrop (the tower's glow leaves a dark blue haze round it). The banner hangs from the tip of
#: its pole, a bit left of middle since the cloth blows to the right; the island hangs from the top
#: of its balustrade; the tower stands on its foot.
TITLE_LOOPS = (
    ("banner", "title-banner.png", 8, 9.9, (34, 66), "tip", 0.3, 0),
    ("waterfall", "title-waterfall.png", 6, 10.0, (34, 69), "top", 0.5, 0),
    ("spire", "title-spire.png", 8, 7.6, (30, 74), "bottom", 0.5, 60),
)


#: The petal that falls off the blossom trees: one of the ones lying under the stalls on the
#: Questeria ground sheet, and its size in art pixels.
PETAL_BOX = (46, 946, 26, 18)
PETAL_SIZE = (5, 4)


def build_title_layers():
    for out, name, key in TITLE_LAYERS:
        save(sample(name, TITLE_SOURCE_BOX, (TITLE_W, TITLE_H), key=key, holes=key), "Title/" + out + ".png")


def things(name, box, smallest=400, dark=0):
    """Every separate thing in a box: the boxes round each patch of picture the backdrop doesn't
    reach, biggest first, skipping specks smaller than `smallest` pixels."""
    mask = keyed(name, box, dark)
    bw, bh = box[2], box[3]
    seen = [bytearray(bw) for _ in range(bh)]
    found = []
    for y in range(bh):
        for x in range(bw):
            if not mask[y][x] or seen[y][x]:
                continue
            stack = [(x, y)]
            seen[y][x] = 1
            x0 = x1 = x
            y0 = y1 = y
            count = 0
            while stack:
                cx, cy = stack.pop()
                count += 1
                x0, x1, y0, y1 = min(x0, cx), max(x1, cx), min(y0, cy), max(y1, cy)
                for nx, ny in ((cx + 1, cy), (cx - 1, cy), (cx, cy + 1), (cx, cy - 1)):
                    if 0 <= nx < bw and 0 <= ny < bh and mask[ny][nx] and not seen[ny][nx]:
                        seen[ny][nx] = 1
                        stack.append((nx, ny))
            if count >= smallest:
                found.append((count, (box[0] + x0, box[1] + y0, x1 - x0 + 1, y1 - y0 + 1)))
    found.sort(key=lambda f: -f[0])
    return [b for _, b in found]


def strip_of(name, boxes, cell, pitch):
    """Cuts each box to a cell of its own along one strip, sitting on the bottom middle of its cell,
    at `pitch` sheet pixels an art pixel."""
    cw, chh = cell
    strip = new_image(cw * len(boxes), chh)
    for i, (x, y, bw, bh) in enumerate(boxes):
        tw, th = min(cw, max(1, round(bw / pitch))), min(chh, max(1, round(bh / pitch)))
        paste(strip, sample(name, (x, y, bw, bh), (tw, th)), i * cw + (cw - tw) // 2, chh - th)
    return strip


def build_title_clouds():
    """The clouds, top row then bottom, left to right."""
    w, h, _, _ = picture("title-clouds.png")
    clouds = sorted(things("title-clouds.png", (0, 0, w, h))[:CLOUDS], key=lambda b: (b[1] // 200, b[0]))
    save(strip_of("title-clouds.png", clouds, (CLOUD_W, CLOUD_H), CLOUD_PITCH), "Title/clouds.png")


def build_title_island():
    """The biggest island in title-far.png's sky, with its waterfall and the cloud round it. Its sky
    stops about 540 pixels down; below that it's mountains."""
    x, y, bw, bh = [b for b in things("title-far.png", (0, 0, 1536, 560)) if b[1] + b[3] < 540][0]
    save(sample("title-far.png", (x, y, bw, bh), (ISLAND_WIDTH, round(bh * ISLAND_WIDTH / bw))), "Title/island.png")


def build_title_loop(out, name, frames, pitch, size, anchor, across, dark):
    """One loop, its frames side by side. Each frame is one of the biggest things on the sheet, left
    to right (they aren't evenly spaced, so cutting the sheet into equal columns cut some of them
    in half), and every frame is lined up on the same point of the thing so it doesn't jump: the
    middle of its top or bottom tenth, or the very top of it (a pole's tip). The specks round a
    frame (a falling drop, a spark off the crystal) get left behind."""
    w, h, _, _ = picture(name)
    fw, fh = size
    strip = new_image(fw * frames, fh)
    boxes = sorted(things(name, (0, 0, w, h), dark=dark)[:frames])
    for f, (cx, cy, cw, chh) in enumerate(boxes):
        tw, th = max(1, round(cw / pitch)), max(1, round(chh / pitch))
        art = sample(name, (cx, cy, cw, chh), (tw, th), dark=dark)
        tenth = max(1, th // 10)
        band = {"tip": range(0, 2), "top": range(0, tenth), "bottom": range(th - tenth, th)}[anchor]
        xs = [x for y in band for x in range(tw) if art[y][x][3]]
        middle = sum(xs) / len(xs) if xs else tw / 2
        frame = new_image(fw, fh)
        paste(frame, art, round(fw * across - middle), fh - th if anchor == "bottom" else 0)
        paste(strip, frame, f * fw, 0)
    save(strip, "Title/" + out + ".png")


def build_title():
    build_title_layers()
    save(sample("questeria-ground.png", PETAL_BOX, PETAL_SIZE), "Title/petal.png")
    build_title_clouds()
    build_title_island()
    for loop in TITLE_LOOPS:
        build_title_loop(*loop)


# --------------------------------------------------------------------------- #
# Questeria: everything standing on its ground, as props
# --------------------------------------------------------------------------- #
# The ground itself is drawn by tools/generate_harbour.py: my ground sheet was painted with its
# cobbles on the diagonal, which read as a different camera to everything standing on them. The
# sheets are drawn about two and a half sheet pixels to an art pixel; the buildings come out finer
# than that, since next to people their height they looked like dolls' houses, with doors you'd
# have to duck through.

#: How far round a prop's box to look, in sheet pixels, in case the box is a pixel or two tight.
PROP_PAD = 4

#: How a prop's cut can go: ALONE is just the biggest piece in the box (the palace and the gate have
#: bits of the next thing on the sheet loose inside their boxes); HOLES clears the backdrop showing
#: through a gap the edge can't reach (between a lamp's arm, its chain and its post).
ALONE, HOLES = "alone", "holes"

#: Questeria's props, and the harbour's: the name (Props/<name>.png, and what PropCatalog and the
#: map call it), the sheet it's on, the box round it on the sheet, how wide it comes out in art
#: pixels, and how it's cut if that's not the usual way. The inn is the shop's house, a size bigger.
QUESTERIA_PROPS = (
    # The big buildings.
    ("palace", "questeria-grand.png", (998, 588, 534, 420), 290, ALONE),
    ("temple", "questeria-grand.png", (368, 615, 314, 390), 176),
    ("city-gate", "questeria-grand.png", (8, 665, 350, 330), 190, ALONE),
    ("guildhall", "questeria-houses.png", (1120, 576, 410, 422), 220),
    ("inn", "questeria-houses.png", (258, 640, 314, 340), 186),
    ("house-ivy", "questeria-houses.png", (12, 640, 238, 332), 130),
    ("house-shop", "questeria-houses.png", (258, 640, 314, 340), 170),
    ("house-awning", "questeria-houses.png", (580, 644, 272, 330), 148),
    ("house-rose", "questeria-houses.png", (862, 642, 248, 324), 138),

    # The plaza and the streets.
    ("fountain", "questeria-props-1.png", (12, 14, 358, 344), 128),
    ("statue", "questeria-props-1.png", (1126, 168, 122, 234), 48),
    ("crystal-pillar", "questeria-props-1.png", (772, 18, 58, 132), 20),
    ("lamp", "questeria-props-1.png", (22, 374, 46, 178), 16),
    ("banner-pink", "questeria-props-1.png", (1268, 334, 100, 160), 36),
    ("banner-blue", "questeria-props-1.png", (1384, 320, 98, 164), 36),
    ("bench", "questeria-props-2.png", (95, 529, 148, 79), 46),
    ("table", "questeria-props-2.png", (471, 530, 140, 80), 46),
    ("planter", "questeria-props-1.png", (136, 542, 98, 102), 32),
    ("cypress-pot", "questeria-props-1.png", (404, 398, 68, 162), 22),
    ("flower-cart", "questeria-props-1.png", (482, 642, 148, 96), 50),
    ("notice-board", "questeria-props-1.png", (814, 536, 112, 112), 40),
    ("well-roofed", "questeria-props-1.png", (16, 564, 112, 182), 50),
    ("cask", "questeria-props-1.png", (1440, 570, 34, 56), 12),
    ("water-barrel", "questeria-props-1.png", (266, 642, 54, 76), 18),
    ("stall-blue", "questeria-props-1.png", (18, 744, 170, 244), 72),
    ("stall-pink", "questeria-props-1.png", (188, 740, 158, 242), 70),
    ("stall-teal", "questeria-props-1.png", (352, 736, 148, 244), 68),
    ("stall-cloth", "questeria-props-1.png", (674, 744, 162, 238), 70),
    ("garden-wall", "questeria-garden.png", (22, 756, 210, 90), 84),

    # The harbour.
    ("cargo-crate", "questeria-props-2.png", (12, 280, 62, 65), 22),
    ("cargo-barrel", "questeria-props-2.png", (18, 432, 55, 86), 18),
    ("sacks", "questeria-props-2.png", (478, 435, 104, 95), 36),
    ("rope-posts", "questeria-props-2.png", (823, 553, 117, 71), 40),
    ("bollard", "questeria-props-2.png", (953, 553, 26, 68), 9),
    ("lantern-post", "questeria-props-2.png", (1195, 343, 66, 168), 22),
    ("signpost", "questeria-props-2.png", (1275, 353, 117, 161), 40),

    # Trees, and the green between things.
    ("cherry-tree", "questeria-trees.png", (14, 12, 196, 226), 104),
    ("cherry-tree-small", "questeria-garden.png", (200, 184, 176, 160), 70),
    ("oak", "questeria-trees.png", (598, 12, 180, 228), 96),
    ("blossom-tree", "questeria-trees.png", (1106, 10, 242, 232), 112),
    ("cypress", "questeria-trees.png", (622, 678, 50, 146), 22),
    ("topiary", "questeria-trees.png", (910, 694, 98, 134), 34),
    ("flower-arch", "questeria-trees.png", (1166, 694, 118, 138), 48),
    ("bush-flowers", "questeria-trees.png", (106, 676, 104, 84), 32),
    ("bush-pink", "questeria-trees.png", (332, 672, 106, 92), 32),
    ("flowers-pink", "questeria-trees.png", (106, 924, 74, 62), 22),
    ("flowers-white", "questeria-trees.png", (16, 918, 74, 66), 22),
    ("flowers-mixed", "questeria-trees.png", (198, 924, 92, 64), 26),
    ("rocks", "questeria-trees.png", (16, 832, 112, 82), 30),

    # More for the docks: crates, barrels and sacks, the bell the harbour rings, washing hung out,
    # and water: a pump, a trough, a tub and a well.
    ("harbour-bell", "questeria-props-2.png", (1446, 363, 78, 137), 26),
    ("crate-open", "questeria-props-2.png", (93, 282, 58, 62), 20),
    ("crate-apples", "questeria-props-2.png", (188, 349, 52, 69), 18),
    ("crate-onions", "questeria-props-2.png", (255, 355, 47, 61), 16),
    ("crate-stack", "questeria-props-2.png", (523, 321, 84, 102), 28),
    ("barrel-water", "questeria-props-2.png", (229, 431, 57, 89), 19),
    ("barrel-salt", "questeria-props-2.png", (300, 439, 52, 78), 17),
    ("sack-pile", "questeria-props-2.png", (570, 404, 84, 113), 28),
    ("washing-line", "questeria-props-2.png", (701, 631, 120, 114), 40),
    ("water-pump", "questeria-props-2.png", (909, 338, 67, 134), 22),
    ("trough", "questeria-props-2.png", (1000, 419, 100, 101), 34),
    ("washtub", "questeria-props-2.png", (929, 476, 70, 64), 23),
    ("well-flowers", "questeria-props-2.png", (701, 326, 141, 210), 56),

    # More for the streets: the quest board (the roofed one), signposts, lamps, planters, benches, a
    # handcart, a railing, a sewer grate, and a café's chalkboard.
    ("quest-board", "questeria-props-2.png", (1285, 212, 130, 137), 44),
    ("signpost-arrows", "questeria-props-2.png", (1439, 228, 85, 126), 28),
    ("planter-stone", "questeria-props-2.png", (847, 479, 68, 71), 23),
    ("lamp-arm", "questeria-props-1.png", (82, 373, 108, 176), 36, ALONE, HOLES),
    ("lamp-double", "questeria-props-1.png", (309, 360, 84, 195), 28),
    ("lantern-small", "questeria-props-1.png", (130, 479, 41, 56), 14),
    ("planter-long", "questeria-props-1.png", (937, 537, 113, 98), 38),
    ("planter-pink", "questeria-props-1.png", (1339, 496, 102, 81), 34),
    ("flower-box", "questeria-props-1.png", (307, 574, 51, 63), 17),
    ("barrel-flowers", "questeria-props-1.png", (1085, 636, 58, 82), 20),
    ("bench-iron", "questeria-props-1.png", (135, 645, 122, 90), 41),
    ("bench-plain", "questeria-props-1.png", (866, 642, 123, 84), 41),
    ("handcart", "questeria-props-1.png", (384, 653, 92, 73), 31),
    ("fence-iron", "questeria-props-1.png", (1118, 509, 205, 94), 68),
    ("sewer-grate", "questeria-props-1.png", (1445, 488, 68, 57), 23),
    ("chalkboard", "questeria-props-1.png", (1172, 894, 56, 70), 19),

    # Inside: the furniture off my props sheet, in place of what generate_interiors.py drew, under
    # the same names so the rooms don't change; and more of it, for the rooms to have.
    ("hearth", "questeria-props-2.png", (189, 684, 107, 189), 44, ALONE),
    ("round-table", "questeria-props-2.png", (949, 752, 65, 82), 24),
    ("stool", "questeria-props-2.png", (778, 744, 38, 51), 13),
    ("wardrobe", "questeria-props-2.png", (433, 706, 76, 139), 29),
    ("bookshelf", "questeria-props-2.png", (1032, 616, 131, 123), 44),
    ("candle-stand", "questeria-props-2.png", (633, 629, 49, 100), 18),
    ("shop-counter", "questeria-props-2.png", (14, 790, 180, 90), 60, ALONE),
    ("chair", "questeria-props-2.png", (795, 755, 71, 109), 24),
    ("armchair", "questeria-props-2.png", (992, 917, 77, 98), 26),
    ("tapestry", "questeria-props-2.png", (906, 626, 101, 124), 34),
    ("anvil", "questeria-props-2.png", (1223, 709, 92, 116), 31),
    ("tool-rack", "questeria-props-2.png", (1307, 607, 83, 132), 28),
    ("desk", "questeria-props-2.png", (969, 823, 140, 113), 47),
    ("side-table", "questeria-props-2.png", (1108, 740, 84, 94), 28),
    ("utensil-rack", "questeria-props-2.png", (519, 729, 92, 119), 31),
    ("book-crate", "questeria-props-2.png", (885, 841, 63, 92), 21),
    ("potted-plant", "questeria-props-2.png", (813, 877, 62, 123), 21),
)

#: The waypoint stone: a crystal on a plinth, asleep (dim) and then awake (glowing), lined up on
#: the plinth. The glowing one leaves a dark blue haze on the backdrop round its crystal.
WAYPOINT_FRAMES = ((522, 288, 62, 98), (366, 258, 68, 124))
WAYPOINT_SHEET = "questeria-props-1.png"
WAYPOINT_PITCH = 3.4
WAYPOINT_DARK = 50


def pieces(mask):
    """Every separate patch of picture in a mask: its pixels, and whether it touches the edge."""
    bh, bw = len(mask), len(mask[0])
    seen = [bytearray(bw) for _ in range(bh)]
    found = []
    for y in range(bh):
        for x in range(bw):
            if not mask[y][x] or seen[y][x]:
                continue
            stack, cells, edge = [(x, y)], [], False
            seen[y][x] = 1
            while stack:
                cx, cy = stack.pop()
                cells.append((cx, cy))
                edge = edge or cx in (0, bw - 1) or cy in (0, bh - 1)
                for nx, ny in ((cx + 1, cy), (cx - 1, cy), (cx, cy + 1), (cx, cy - 1)):
                    if 0 <= nx < bw and 0 <= ny < bh and mask[ny][nx] and not seen[ny][nx]:
                        seen[ny][nx] = 1
                        stack.append((nx, ny))
            found.append((cells, edge))
    return found


def isolated(name, box, dark=0, alone=False, holes=False):
    """The mask of one thing in a box: its biggest piece, and (unless it's to be alone) any other
    piece wholly inside the box (a sign hanging off a stall), but nothing touching the box's edge,
    which is a bit of the thing next to it on the sheet."""
    mask = keyed(name, box, dark, holes)
    found = pieces(mask)
    biggest = max(found, key=lambda f: len(f[0]))
    keep = [bytearray(box[2]) for _ in range(box[3])]
    for cells, edge in found:
        if cells is biggest[0] or not edge and not alone:
            for x, y in cells:
                keep[y][x] = 1
    return keep


def cut_thing(name, box, width=None, pitch=None, dark=0, alone=False):
    """One thing off a sheet, isolated and sampled down to `width` art pixels across (or `pitch`
    sheet pixels an art pixel), trimmed to the box round what's left."""
    return cut_out(name, box, width, pitch, dark, alone)


def cut_out(name, box, width=None, pitch=None, dark=0, alone=False, holes=False):
    """cut_thing, and with holes, the backdrop showing through any gap in the thing cleared too."""
    w, h, _, _ = picture(name)
    x0, y0 = max(0, box[0] - PROP_PAD), max(0, box[1] - PROP_PAD)
    x1, y1 = min(w, box[0] + box[2] + PROP_PAD), min(h, box[1] + box[3] + PROP_PAD)
    padded = (x0, y0, x1 - x0, y1 - y0)
    mask = isolated(name, padded, dark, alone, holes)
    ys = [y for y, line in enumerate(mask) if any(line)]
    xs = [x for x in range(padded[2]) if any(line[x] for line in mask)]
    tight = (x0 + min(xs), y0 + min(ys), max(xs) - min(xs) + 1, max(ys) - min(ys) + 1)
    trimmed = [line[min(xs):max(xs) + 1] for line in mask[min(ys):max(ys) + 1]]
    p = pitch if pitch else tight[2] / width
    size = (max(1, round(tight[2] / p)), max(1, round(tight[3] / p)))
    return sample(name, tight, size, mask=trimmed)


def build_questeria_props(wanted=None):
    """The props cut whole, or just the ones wanted, by name."""
    # A sheet at a time, so each is only read once.
    for out, name, box, width, *how in sorted(QUESTERIA_PROPS, key=lambda p: p[1]):
        if wanted is None or out in wanted:
            save(cut_out(name, box, width=width, alone=ALONE in how, holes=HOLES in how), "Props/" + out + ".png")
    if wanted is not None and "crystal-stone" not in wanted:
        return

    # The waypoint's two pictures side by side, each standing on the middle of its bottom edge.
    frames = [cut_thing(WAYPOINT_SHEET, box, pitch=WAYPOINT_PITCH, dark=WAYPOINT_DARK) for box in WAYPOINT_FRAMES]
    fw, fh = max(len(f[0]) for f in frames), max(len(f) for f in frames)
    strip = new_image(fw * len(frames), fh)
    for i, f in enumerate(frames):
        paste(strip, f, i * fw + (fw - len(f[0])) // 2, fh - len(f))
    save(strip, "Props/crystal-stone.png")


# --------------------------------------------------------------------------- #
# Questeria's other buildings: put together out of the parts on my sheets
# --------------------------------------------------------------------------- #
# My houses sheet has five whole houses along its bottom, and above them the parts they're made
# of: roofs, walls with windows and flower boxes, doorways and shop fronts, stalls and hanging
# signs. The grand sheet has the same for towers and chapels. Each building here is some of those
# parts laid out the way I'd lay them out in a paint program, in sheet pixels and back to front, so
# each part covers the edge of the one behind it and there's no seam; then the whole thing is
# sampled down at once, like a house cut whole. Nothing's flipped, so the light stays top left.

#: How many sheet pixels an art pixel takes for a building put together from parts. The parts are
#: drawn a size bigger than the whole houses, so they're sampled coarser: this makes their doors
#: and windows come out the size of the whole houses' ones.
KITBASH_PITCH = 2.1

HOUSES, GRAND = "questeria-houses.png", "questeria-grand.png"

#: What a part can be marked: ROOF is roof, which is what gets recoloured for a building's other
#: roofs; SOLID fills in what the backdrop showed through between its sides (an open archway, whose
#: dark inside keys out with the backdrop, since it's open at the bottom); OUTSIDE stands outside
#: the walls (a sign, a barrel, a banner on its stand), so it doesn't count when the building's
#: foot is put under the middle of its walls.
ROOF, SOLID, OUTSIDE = "roof", "solid", "outside"

#: The buildings: the name, the colour its roof's drawn (None if it has no tiles to recolour), the
#: middle of the bottom of its door (in sheet pixels on the building, the same as the parts, or None
#: if it hasn't one), its parts back to front, and anything painted on afterwards. A part is the
#: sheet, the box round it there, where its top left goes, and what it's marked; a box can cut
#: through something on the sheet to take just a slice of it. A painting is what it is, where its
#: middle goes, and for an emblem, how big a device it paints out if that's not DEVICE.
BUILDINGS = (
    # Tall and narrow: a gable with a garret window, a floor of flower boxes, and a door beside a
    # slit window.
    ("townhouse", "blue", (110, 417), (
        (HOUSES, (1443, 362, 78, 115), (12, 302)),
        (HOUSES, (612, 368, 65, 103), (78, 314)),
        (HOUSES, (215, 204, 64, 145), (14, 173)),
        (HOUSES, (288, 206, 79, 143), (64, 175)),
        (HOUSES, (231, 7, 157, 181), (0, 0), ROOF),
        (HOUSES, (625, 580, 28, 47), (-4, 370), OUTSIDE),
    ), ()),
    # One floor under a big hipped roof: a window box each side of a gabled porch door.
    ("cottage", "blue", (105, 307), (
        (GRAND, (1397, 188, 65, 130), (73, 162)),
        (HOUSES, (215, 204, 64, 145), (10, 162)),
        (HOUSES, (1047, 206, 47, 149), (137, 158)),
        (HOUSES, (122, 360, 87, 120), (62, 187)),
        (HOUSES, (15, 11, 203, 172), (0, 0), ROOF),
        (HOUSES, (420, 590, 21, 39), (0, 268), OUTSIDE),
        (HOUSES, (625, 580, 28, 47), (172, 260), OUTSIDE),
    ), ()),
    # A gabled house with a round turret up its corner, taller than the house.
    ("turret-house", "blue", (118, 400), (
        (HOUSES, (693, 368, 96, 104), (73, 296)),
        (HOUSES, (377, 204, 86, 154), (77, 166)),
        (HOUSES, (566, 9, 113, 174), (64, 0), ROOF),
        (HOUSES, (1188, 206, 71, 141), (4, 259)),
        (HOUSES, (1445, 204, 77, 140), (1, 133)),
        (HOUSES, (1434, 20, 87, 169), (-4, -22), ROOF),
        (HOUSES, (625, 580, 28, 47), (166, 353), OUTSIDE),
    ), ()),
    # The baker's: a shop window under a striped awning, the door beside it, a floor of flower boxes
    # over, and a loaf on the sign hanging off the corner.
    ("bakery", "blue", (152, 398), (
        (HOUSES, (1104, 368, 125, 106), (4, 292)),
        (HOUSES, (612, 368, 65, 103), (120, 295)),
        (HOUSES, (119, 204, 83, 146), (4, 162)),
        (HOUSES, (288, 206, 79, 143), (73, 165)),
        (HOUSES, (1047, 206, 47, 149), (140, 159)),
        (HOUSES, (436, 489, 112, 68), (10, 300)),
        (HOUSES, (1457, 486, 63, 95), (-43, 225), OUTSIDE),
        (HOUSES, (15, 11, 203, 172), (0, 0), ROOF),
        (HOUSES, (625, 580, 28, 47), (180, 351), OUTSIDE),
    ), (("loaf", (-16, 287)),)),
    # A little chapel: a gothic door under a gable, and a stained glass window under a gable each side.
    ("chapel", "blue", (154, 341), (
        (GRAND, (13, 168, 80, 158), (10, 183)),
        (GRAND, (107, 168, 80, 158), (218, 183)),
        (HOUSES, (991, 99, 69, 90), (15, 101), ROOF),
        (HOUSES, (991, 99, 69, 90), (223, 101), ROOF),
        (GRAND, (208, 335, 138, 170), (85, 171)),
        (HOUSES, (231, 7, 157, 181), (75, 0), ROOF),
    ), ()),
    # The smithy: an open arch with the forge glowing inside, a chimney, and the anvil sign on its
    # post outside.
    ("forge", "blue", (60, 313), (
        (HOUSES, (1173, 8, 40, 78), (100, -40)),
        (GRAND, (595, 337, 91, 140), (14, 173), SOLID),
        (HOUSES, (974, 206, 56, 148), (92, 165)),
        (HOUSES, (231, 7, 157, 181), (0, 0), ROOF),
        (HOUSES, (333, 486, 73, 140), (145, 173), OUTSIDE),
    ), (("forge", (60, 300)),)),
    # The Watch's post: a guardroom with the Watch's blue door and battlements along its top, a
    # battlemented tower on its corner, and the Watch's banner on its stand outside.
    ("watch-post", None, (94, 388), (
        (HOUSES, (352, 364, 70, 114), (60, 274)),
        (HOUSES, (1443, 362, 78, 115), (122, 273)),
        (GRAND, (1194, 484, 80, 40), (58, 238)),
        (GRAND, (1194, 484, 80, 40), (124, 238)),
        (HOUSES, (1271, 204, 65, 150), (1, 238)),
        (HOUSES, (1111, 204, 64, 142), (2, 110)),
        (GRAND, (1452, 534, 67, 122), (0, 0)),
        (HOUSES, (625, 580, 28, 47), (38, 341), OUTSIDE),
        (HOUSES, (905, 364, 87, 109), (196, 279), OUTSIDE),
    ), ()),
    # A warehouse for the docks: two gables, loft windows, and wide doors to get the cargo in.
    ("warehouse", "blue", (122, 400), (
        (HOUSES, (1443, 362, 78, 115), (0, 285)),
        (HOUSES, (221, 364, 118, 111), (66, 289)),
        (HOUSES, (439, 367, 69, 107), (172, 293)),
        (HOUSES, (1355, 220, 73, 130), (0, 175)),
        (HOUSES, (16, 205, 80, 143), (61, 162)),
        (HOUSES, (1355, 220, 73, 130), (129, 175)),
        (HOUSES, (718, 206, 58, 136), (190, 169)),
        (HOUSES, (400, 7, 153, 180), (118, 3), ROOF),
        (HOUSES, (231, 7, 157, 181), (-15, 2), ROOF),
        (HOUSES, (625, 580, 28, 47), (-8, 353), OUTSIDE),
    ), ()),
    # The Gilded Flagon, the Lantern Market's inn: wide, two floors, a front gable over its door, and
    # its sign, a gold flagon, hanging off the corner.
    ("gilded-flagon", "blue", (242, 400), (
        (HOUSES, (1331, 368, 96, 106), (0, 294)),
        (HOUSES, (1104, 368, 125, 106), (84, 294)),
        (HOUSES, (693, 368, 96, 104), (197, 296)),
        (HOUSES, (377, 204, 86, 154), (0, 164)),
        (HOUSES, (865, 204, 90, 144), (72, 166)),
        (HOUSES, (119, 204, 83, 146), (148, 164)),
        (HOUSES, (479, 204, 78, 147), (217, 163)),
        (HOUSES, (15, 11, 203, 172), (-8, 2), ROOF),
        (HOUSES, (231, 7, 157, 181), (150, -7), ROOF),
        (HOUSES, (1321, 489, 64, 92), (-45, 180), OUTSIDE),
        (HOUSES, (625, 580, 28, 47), (280, 353), OUTSIDE),
    ), (("flagon", (-21, 239)),)),
    # The Rusty Anvil, the dockside tavern: two gables, a wide door with a lantern, and its sign.
    ("rusty-anvil", "blue", (128, 360), (
        (HOUSES, (1443, 362, 78, 115), (0, 245)),
        (HOUSES, (221, 364, 118, 111), (72, 249)),
        (HOUSES, (1355, 220, 73, 130), (0, 135)),
        (HOUSES, (16, 205, 80, 143), (60, 122)),
        (HOUSES, (1271, 204, 65, 150), (128, 115)),
        (HOUSES, (566, 9, 113, 174), (88, -44), ROOF),
        (HOUSES, (400, 7, 153, 180), (-22, -50), ROOF),
        (HOUSES, (420, 590, 21, 39), (-6, 321), OUTSIDE),
        (HOUSES, (568, 489, 79, 138), (186, 222), OUTSIDE),
    ), ()),
    # The fishmonger's on the docks: a timber shop front under a blue striped awning, a garret
    # over it, and a fish on the sign.
    ("fishmonger", "blue", (80, 279), (
        (HOUSES, (1104, 368, 125, 106), (16, 173)),
        (HOUSES, (231, 7, 157, 181), (0, 0), ROOF),
        (HOUSES, (797, 490, 96, 66), (30, 152)),
        (HOUSES, (1321, 489, 64, 92), (-31, 160), OUTSIDE),
    ), (("fish", (-7, 219), (12, 16)),)),
    # Not a building, but put together the same way: the Gilded Flagon's sign on its own post, for
    # the street outside, the anvil off the smith's sign painted over with a flagon. The post's its
    # foot, so the sign itself hangs outside it.
    ("gilded-flagon-sign", None, None, (
        (HOUSES, (333, 486, 17, 140), (0, 0)),
        (HOUSES, (350, 486, 56, 140), (17, 0), OUTSIDE),
    ), (("flagon", (45, 63), (16, 14)),)),
)

PROPS_1, PROPS_2 = "questeria-props-1.png", "questeria-props-2.png"

#: Props put together the same way as the buildings, from parts of my props sheets: the name, how
#: many sheet pixels an art pixel takes (the props sheets are drawn bigger than the houses' parts,
#: about three to an art pixel), the parts back to front, and anything painted on afterwards.
MADE_PROPS = (
    # The Flagon's bar: the counter off the market stall with the cloth over it, twice end to end.
    ("bar-counter", 3.0, (
        (PROPS_2, (14, 790, 180, 90), (0, 0)),
        (PROPS_2, (14, 790, 180, 90), (146, 0)),
    ), ()),
    # Kegs: one at the back and two in front of it.
    ("keg-stack", 3.0, (
        (PROPS_2, (361, 438, 51, 79), (30, 0)),
        (PROPS_2, (86, 429, 57, 90), (0, 30)),
        (PROPS_2, (158, 429, 56, 91), (56, 29)),
    ), ()),
    # The fishmonger's stall on the docks: the blue striped stall, the jars on its counter cleared
    # for the day's catch laid out on ice, and its crate of more in front.
    ("fish-stall", 2.36, (
        (PROPS_1, (984, 732, 140, 220), (0, 0)),
    ), (("catch", (32, 104, 124, 146)),)),
    # A rack for drying nets: the washing line, its washing taken in and a net hung over it.
    ("net-rack", 3.0, (
        (PROPS_2, (701, 631, 120, 114), (0, 0)),
    ), (("net", (14, 9, 99, 97)),)),
)

#: The roof colours every house comes in: the middle of the colour (hue in degrees, saturation and
#: brightness), which a roof's own shades are moved round. Blue and pink are my sheets' two; teal and
#: plum are new, made to sit between them. HUE_KEPT is how much of the spread of hues in a roof's own
#: shades it keeps in the new colour.
ROOF_COLOURS = {
    "blue": (199, 0.77, 0.75),
    "pink": (346, 0.51, 0.87),
    "teal": (172, 0.62, 0.68),
    "plum": (290, 0.40, 0.66),
}
HUE_KEPT = 0.5

#: The houses cut whole that come in the other roof colours too: the colour they're drawn (None if
#: it's two, so it comes in all four), and the boxes their roof is in, left, top, right and bottom
#: in art pixels. The walls outside them are left alone, glass, awnings, flowers and all.
WHOLE_HOUSES = (
    ("house-ivy", "blue", ((0, 0, 130, 74),)),
    ("house-awning", "blue", ((0, 0, 148, 80), (118, 40, 140, 100))),
    ("house-rose", "pink", ((13, 0, 138, 82),)),
    ("house-shop", None, ((0, 0, 170, 75),)),
)


def roof_colour_of(r, g, b):
    """Which of my two roof colours a pixel is, or None if it isn't roof: blue tiles are bright
    and sky blue, pink ones rosy, shadowed purple. Window glass is a darker, deeper blue, so it's
    left out."""
    h, s, v = colorsys.rgb_to_hsv(r / 255, g / 255, b / 255)
    h *= 360
    if 180 <= h <= 215 and s > 0.35 and v > 0.3 and not (h > 205 and v < 0.45):
        return "blue"
    if (h >= 290 or h <= 12) and s > 0.25 and v > 0.2:
        return "pink"
    return None


def recolour(img, roof, colour):
    """A copy of a picture with its roof tiles moved to another of the roof colours: each tile
    pixel keeps where it sits in its own colour's shades, round the new colour's middle."""
    out = [[list(px) for px in line] for line in img]
    th, ts, tv = ROOF_COLOURS[colour]
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
            s = min(1.0, s * ts / ss)
            v = min(1.0, v * tv / sv)
            line[x] = [round(c * 255) for c in colorsys.hsv_to_rgb(h, s, v)] + [255]
    return out


#: How dark a pixel round the edge of a building can be and still be the backdrop's haze rather
#: than the outline (the haze is near black and grey; the outline is a dark brown or blue).
HAZE = 44


def tidy(img):
    """Takes the backdrop's haze off a cut's edge: near-black, grey pixels with nothing beside them,
    which read as a black fringe on light cobbles. Twice, since the haze can be two pixels deep."""
    h, w = len(img), len(img[0])
    for _ in range(2):
        clear = []
        for y in range(h):
            for x in range(w):
                r, g, b, a = img[y][x]
                if not a or max(r, g, b) > HAZE or max(r, g, b) - min(r, g, b) > HAZE // 2:
                    continue
                if any(not (0 <= nx < w and 0 <= ny < h) or not img[ny][nx][3]
                       for nx, ny in ((x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1))):
                    clear.append((x, y))
        for x, y in clear:
            img[y][x] = [0, 0, 0, 0]
    return img


def parts_of(layouts):
    """Every part some layouts of parts use, cut off its sheet once, a sheet at a time: its pixels
    and its mask (the part alone, keyed, and filled in if it's SOLID), by sheet, box and marks."""
    cut = {}
    for name, box, *marks in sorted({(p[0], p[1]) + tuple(p[3:]) for parts in layouts for p in parts}):
        mask = isolated(name, box)
        if SOLID in marks:
            for line in mask:
                xs = [x for x, v in enumerate(line) if v]
                if xs:
                    line[min(xs):max(xs) + 1] = b"" * (max(xs) - min(xs) + 1)
        w, h, ch, rows = picture(name)
        x0, y0, bw, bh = box
        pixels = [bytes(rows[y0 + y][x0 * ch:(x0 + bw) * ch]) for y in range(bh)]
        cut[(name, box) + tuple(marks)] = (ch, pixels, mask)
    return cut


def kitbash(key, parts, cut):
    """Lays a building's parts out back to front on a canvas of its own, which goes in as a
    picture called `key` so it can be sampled like a sheet. Returns where the canvas's corner is
    in the parts' sheet pixels, its mask, and which of its pixels are roof."""
    x0 = min(at[0] for _, _, at, *_ in parts)
    y0 = min(at[1] for _, _, at, *_ in parts)
    w = max(at[0] + box[2] for _, box, at, *_ in parts) - x0
    h = max(at[1] + box[3] for _, box, at, *_ in parts) - y0
    rows = [bytearray(w * 3) for _ in range(h)]
    mask = [bytearray(w) for _ in range(h)]
    roof = [bytearray(w) for _ in range(h)]
    for name, box, (ax, ay), *marks in parts:
        ch, pixels, part = cut[(name, box) + tuple(marks)]
        for y in range(box[3]):
            line, src, cy = part[y], pixels[y], ay - y0 + y
            for x in range(box[2]):
                if line[x]:
                    cx = ax - x0 + x
                    rows[cy][cx * 3:cx * 3 + 3] = src[x * ch:x * ch + 3]
                    mask[cy][cx] = 1
                    roof[cy][cx] = 1 if ROOF in marks else 0
    _pictures.clear()
    _pictures[key] = (w, h, 3, rows)
    return (x0, y0), mask, roof


# --------------------------------------------------------------------------- #
# What gets painted on afterwards, in art pixels
# --------------------------------------------------------------------------- #

#: The little pictures painted onto signs: a row of letters a row of pixels, each letter a colour
#: (a space is clear), lit from the top left.
EMBLEMS = {
    # A gold flagon with a head of foam, its handle on the right.
    "flagon": ((
        " wwwf   ",
        "wwwwwf  ",
        "lgggdd  ",
        "lgggdgd ",
        "lgggd d ",
        "lgggd d ",
        "lgggddd ",
        "lgggd   ",
        "ddddd   ",
    ), {"w": (250, 244, 230), "f": (214, 202, 190), "l": (248, 222, 140), "g": (222, 172, 76),
        "d": (150, 98, 36)}),
    # A loaf, scored across the top.
    "loaf": ((
        "  lllll  ",
        " lldlldl ",
        "lgggggggd",
        "dggggggdd",
        " ddddddd ",
    ), {"l": (244, 204, 128), "g": (208, 142, 66), "d": (132, 76, 40)}),
    # A silver fish, nose to the left, its tail forked.
    "fish": ((
        "  oooo    o",
        " olllllo oo",
        "oellllllooo",
        "osssssssoo ",
        " osssssooo ",
        "  oooo    o",
    ), {"l": (226, 236, 244), "s": (150, 174, 204), "o": (62, 74, 108), "e": (24, 22, 34)}),
}

#: The forge's fire: its shades from the coals out, how far it glows across and up its arch in art
#: pixels, how dark a pixel has to be to be the inside of the arch it lights (by brightness, out of
#: 255), how much the edge of one shade wanders into the next, and how little heat still shows.
FIRE = ((255, 236, 170), (255, 196, 92), (240, 126, 44), (176, 62, 34))
FIRE_REACH = (16, 22)
FIRE_DARK = 60
FIRE_DITHER = 0.18
FIRE_EDGE = 0.08

#: How big the gold device my signs have on them is, in art pixels (a fleur, or the smith's anvil):
#: that much round an emblem's middle is painted out before the emblem goes on.
DEVICE = (12, 13)


def golden(r, g, b):
    """Whether a pixel is the gold a sign's device is painted in, from its shadow to its shine."""
    h, s, v = colorsys.rgb_to_hsv(r / 255, g / 255, b / 255)
    return 15 <= h * 360 <= 75 and s > 0.12 and v > 0.35


def emblem(img, x, y, which, device=DEVICE):
    """Paints an emblem with its middle at (x, y), over the gold device the sign had there (`device`
    across and down): that's painted out in the colour of the sign's field first (the commonest
    colour round it that isn't gold)."""
    rows, colours = EMBLEMS[which]
    left, top = x - len(rows[0]) // 2, y - len(rows) // 2
    dw, dh = device
    box = [(px, py) for py in range(y - dh // 2, y + dh - dh // 2) for px in range(x - dw // 2, x + dw - dw // 2)
           if 0 <= py < len(img) and 0 <= px < len(img[0]) and img[py][px][3]]
    field = [tuple(img[py][px][:3]) for px, py in box if not golden(*img[py][px][:3])]
    if field:
        fill = max(set(field), key=field.count)
        for px, py in box:
            if golden(*img[py][px][:3]):
                img[py][px] = list(fill) + [255]
    for dy, line in enumerate(rows):
        for dx, letter in enumerate(line):
            if letter != " " and 0 <= top + dy < len(img) and 0 <= left + dx < len(img[0]):
                img[top + dy][left + dx] = list(colours[letter]) + [255]


def forge_glow(img, x, y):
    """The forge's fire through its arch: the dark inside it lit from (x, y), on the ground at
    the back, brightest there and going out to the dark, dithered where one shade meets the next."""
    rx, ry = FIRE_REACH
    for py in range(max(0, y - ry), min(len(img), y + 2)):
        for px in range(max(0, x - rx), min(len(img[0]), x + rx + 1)):
            r, g, b, a = img[py][px]
            if not a or 0.299 * r + 0.587 * g + 0.114 * b > FIRE_DARK:
                continue
            d = ((px - x) / rx) ** 2 + ((py - y) / ry) ** 2
            heat = 1 - d ** 0.5 + (hash01(px, py, 5803) - 0.5) * FIRE_DITHER
            if heat > FIRE_EDGE:
                img[py][px] = list(FIRE[min(3, int((1 - heat) * 4))]) + [255]


#: Fish small enough to lay out on a stall: a row of letters a row of pixels, nose to the left; how
#: far apart the rows of them are, and the fish along a row; and the kinds of fish, light, shadow and
#: tail. The ice they lie on, dark to light, and the tray's rim.
FISH_ROW, FISH_STEP = 4, 8
SMALL_FISH = (
    " llll t",
    "elllsst",
    " ssss t",
)
FISH_KINDS = (
    ((150, 172, 202), (96, 114, 148), (68, 82, 114)),
    ((236, 154, 130), (184, 98, 88), (132, 70, 70)),
    ((122, 172, 168), (72, 112, 122), (50, 80, 92)),
)
ICE = ((176, 204, 222), (206, 226, 238), (238, 246, 250))
TRAY = (86, 54, 42)


def catch(img, x0, y0, x1, y1):
    """The day's catch on a stall, over a box: a tray of crushed ice with rows of fish laid on it,
    every other row set over by half a fish, the kinds mixed."""
    for y in range(y0, y1 + 1):
        for x in range(x0, x1 + 1):
            if x in (x0, x1) or y in (y0, y1):
                img[y][x] = list(TRAY) + [255]
            else:
                h = hash01(x, y, 7151)
                img[y][x] = list(ICE[2 if h > 0.8 else 1 if h > 0.35 else 0]) + [255]
    for row, fy in enumerate(range(y0 + 2, y1 - 3, FISH_ROW)):
        for fx in range(x0 + 2 + (row % 2) * FISH_STEP // 2, x1 - len(SMALL_FISH[0]) + 1, FISH_STEP):
            light, shade, tail = FISH_KINDS[int(hash01(fx, fy, 7152) * len(FISH_KINDS))]
            colours = {"l": light, "s": shade, "t": tail, "e": (24, 22, 34)}
            for dy, line in enumerate(SMALL_FISH):
                for dx, letter in enumerate(line):
                    if letter != " ":
                        img[fy + dy][fx + dx] = list(colours[letter]) + [255]


#: A net's rope, dark to light, how far apart its knots are, how much of its depth it sags by in
#: the middle, and the corks along its bottom edge (shadow and light), every so many pixels.
ROPE = ((118, 90, 64), (166, 130, 88), (206, 176, 128))
MESH = 4
SAG = 0.45
CORK = ((176, 82, 46), (232, 142, 78))
CORK_EVERY = 5


def net(img, x0, y0, x1, y1):
    """A fishing net hung up to dry over a box: whatever hung there taken down, then the net draped
    from the top of the box, deepest in the middle, its mesh lighter at the top, and corks along
    its bottom edge."""
    for y in range(y0, y1 + 1):
        for x in range(x0, x1 + 1):
            img[y][x] = [0, 0, 0, 0]
    depth = y1 - y0
    for x in range(x0, x1 + 1):
        t = (x - x0) / max(1, x1 - x0)
        bottom = y0 + int(depth * (1 - SAG + SAG * math.sin(math.pi * t)))
        for y in range(y0, bottom + 1):
            if y in (y0, bottom) or (x + y) % MESH == 0 or (x - y) % MESH == 0:
                shade = ROPE[2] if y < y0 + depth // 3 else ROPE[1] if y < bottom else ROPE[0]
                img[y][x] = list(shade) + [255]
        if (x - x0) % CORK_EVERY == 2:
            img[bottom][x] = list(CORK[0]) + [255]
            img[bottom - 1][x] = list(CORK[1]) + [255]


def paint(img, which, where, *device):
    """One painting, where it goes in art pixels: the forge's fire, fish laid out on ice or a net
    hung up (each over a box), or an emblem on a sign."""
    if which == "forge":
        forge_glow(img, *where)
    elif which == "catch":
        catch(img, *where)
    elif which == "net":
        net(img, *where)
    else:
        emblem(img, *where, which, *device)


def put_together(out, parts, paintings, pitch, cut):
    """One building or made prop: its parts laid out, sampled down at `pitch` sheet pixels an art
    pixel, tidied and painted. Returns the art, which of its pixels are roof, and where a point on
    it in sheet pixels lands in the art."""
    (cx, cy), mask, roof = kitbash(out, parts, cut)
    ys = [y for y, line in enumerate(mask) if any(line)]
    xs = [x for x in range(len(mask[0])) if any(line[x] for line in mask)]
    tight = (min(xs), min(ys), max(xs) - min(xs) + 1, max(ys) - min(ys) + 1)
    trimmed = [line[tight[0]:tight[0] + tight[2]] for line in mask[tight[1]:tight[1] + tight[3]]]
    size = (round(tight[2] / pitch), round(tight[3] / pitch))
    img = tidy(sample(out, tight, size, mask=trimmed))
    sx, sy = tight[2] / size[0], tight[3] / size[1]
    art_roof = [[roof[tight[1] + int((y + 0.5) * sy)][tight[0] + int((x + 0.5) * sx)] for x in range(size[0])]
                for y in range(size[1])]

    # Clear columns down one side, so the middle of the walls (whatever stands outside them left
    # out) is the middle of the picture, which is where its foot is and its footprint goes.
    walls = [(at[0], at[0] + box[2]) for _, box, at, *marks in parts if OUTSIDE not in marks]
    middle = ((min(a for a, _ in walls) + max(b for _, b in walls)) / 2 - cx - tight[0]) / sx
    pad = round(2 * middle - size[0])
    left = max(0, -pad)
    for rows in (img, art_roof):
        for i, line in enumerate(rows):
            blank = [[0, 0, 0, 0] if rows is img else 0 for _ in range(abs(pad))]
            rows[i] = blank + line if pad < 0 else line + blank

    def art(x, y):
        """Where a point on the layout in sheet pixels is in the art."""
        return round((x - cx - tight[0]) / sx) + left, round((y - cy - tight[1]) / sy)

    for which, where, *device in paintings:
        paint(img, which, art(*where[:2]) + (art(*where[2:]) if len(where) == 4 else ()), *device)
    return img, art_roof, art


def build_buildings(wanted=None):
    """The buildings put together from parts, each in every roof colour, the whole houses' other
    roofs, and the props put together from parts. Prints where each building's door is from its
    foot, for the maps' entrances."""
    buildings = [b for b in BUILDINGS if wanted is None or b[0] in wanted]
    made = [m for m in MADE_PROPS if wanted is None or m[0] in wanted]
    cut = parts_of([b[3] for b in buildings] + [m[2] for m in made])
    for out, colour, door, parts, paintings in buildings:
        img, roof, art = put_together(out, parts, paintings, KITBASH_PITCH, cut)
        save(img, "Props/" + out + ".png")
        if door:
            dx, dy = art(*door)
            walls = [(at[0], at[0] + box[2]) for _, box, at, *marks in parts if not {OUTSIDE, ROOF} & set(marks)]
            wide = art(max(b for _, b in walls), 0)[0] - art(min(a for a, _ in walls), 0)[0]
            print("    door at (%d, %d) from its foot, its walls %d wide" % (dx - len(img[0]) // 2, dy - len(img), wide))
        for other in ROOF_COLOURS if colour else ():
            if other != colour:
                save(recolour(img, roof, other), "Props/" + out + "-" + other + ".png")
    for out, pitch, parts, paintings in made:
        save(put_together(out, parts, paintings, pitch, cut)[0], "Props/" + out + ".png")
    for out, colour, boxes in WHOLE_HOUSES:
        if wanted is not None and out not in wanted:
            continue
        name, box, width = next((p[1], p[2], p[3]) for p in QUESTERIA_PROPS if p[0] == out)
        img = cut_thing(name, box, width=width)
        roof = [[any(x0 <= x < x1 and y0 <= y < y1 for x0, y0, x1, y1 in boxes) for x in range(len(line))]
                for y, line in enumerate(img)]
        for other in ROOF_COLOURS:
            if other != colour:
                save(recolour(img, roof, other), "Props/" + out + "-" + other + ".png")


def build_questeria():
    build_questeria_props()
    build_buildings()


# --------------------------------------------------------------------------- #
# The townsfolk: a walking sheet each, in WalkerSprite's layout
# --------------------------------------------------------------------------- #
# WalkerSprite cuts 32x48 frames with the feet at (16, 46), a row for each way someone faces (down,
# left, right, up). The party's sheets go on to the fighting columns, but townsfolk only stand and
# walk, so theirs stop after the first five: standing, then the four steps.

FRAME_W, FRAME_H = 32, 48

#: The row the soles of the feet stand on: the last row of the figure. WalkerSprite draws from the
#: point just under it.
SOLES = 45

#: Standing, and four steps.
PEOPLE_COLUMNS = 5

#: The figures on each sheet, as a grid of rows (the ways they face, in WalkerSprite's order) and
#: columns: the picture, the box round them, how many rows and columns, and how many sheet pixels
#: an art pixel takes. The pitches make a grown-up about 39 art pixels from heels to crown, about
#: the party's height with a bigger head, and the child on the villager sheet a head shorter, as
#: they were drawn. Each grid is the biggest things in its box, so the lines between the sets on a
#: sheet get left out.
PEOPLE_GRIDS = {
    "villagers": ("npc-villagers.png", (0, 636, 1448, 450), 4, 24, 2.25),
    "templates-ab": ("npc-customization.png", (935, 60, 594, 385), 4, 10, 2.25),
    "templates-cd": ("npc-customization.png", (935, 505, 594, 370), 4, 10, 2.25),
    "townsperson": ("npc-townsperson.png", (0, 0, 1254, 1254), 4, 5, 5.2),
}

#: The townsfolk: the sheet (People/<id>.png, and what a map's npc sheet= says), the grid it's
#: from, and its five columns there. The villager sets walk in four frames and stand in the
#: first; the townsperson stands, then walks in four; the templates only have the one picture each
#: way, so it's the same one five times. Townsfolk.cs lists the same ids. A1, B5 and D5 on the
#: template sheet aren't here: they turn into someone else when they turn round.
TOWNSFOLK = (
    ("elder", "templates-cd", (5,) * 5),
    ("trader", "templates-cd", (7,) * 5),
    ("baker", "templates-ab", (1,) * 5),
    ("smith", "templates-ab", (5,) * 5),
    ("flower-seller", "templates-cd", (1,) * 5),
    ("traveller", "templates-ab", (3,) * 5),
    ("seamstress", "templates-ab", (4,) * 5),
    ("courier", "templates-ab", (8,) * 5),
    ("pilgrim", "templates-cd", (0,) * 5),
    ("weaver", "templates-cd", (6,) * 5),
    ("farmhand", "villagers", (0, 0, 1, 2, 3)),
    ("old-man", "villagers", (4, 4, 5, 6, 7)),
    ("maid", "villagers", (8, 8, 9, 10, 11)),
    ("porter", "villagers", (12, 12, 13, 14, 15)),
    ("old-woman", "villagers", (16, 16, 17, 18, 19)),
    ("child", "villagers", (20, 20, 21, 22, 23)),
    ("youth", "townsperson", (0, 1, 2, 3, 4)),
)

_grids = {}


def grid(key):
    """A grid of figures, cut out and sampled: a list a row, each the figures left to right."""
    if key not in _grids:
        name, box, rows, columns, pitch = PEOPLE_GRIDS[key]
        found = [b for b in things(name, box) if b[2] > 4][:rows * columns]
        found.sort(key=lambda b: b[1])
        cells = []
        for r in range(rows):
            row = sorted(found[r * columns:(r + 1) * columns])
            cells.append([sample(name, b, (round(b[2] / pitch), round(b[3] / pitch))) for b in row])
        _grids[key] = cells
    return _grids[key]


def frame_of(art):
    """One figure in a frame of its own, standing on the soles' row, with the middle of it (by
    how much of it is either side) down the middle of the frame so a walk doesn't sway."""
    xs = [x for line in art for x, px in enumerate(line) if px[3]]
    middle = sum(xs) / len(xs)
    frame = new_image(FRAME_W, FRAME_H)
    paste(frame, art, round(FRAME_W / 2 - 0.5 - middle), SOLES + 1 - len(art))
    return frame


def build_people():
    for out, key, columns in TOWNSFOLK:
        cells = grid(key)
        sheet = new_image(FRAME_W * PEOPLE_COLUMNS, FRAME_H * len(cells))
        for r, row in enumerate(cells):
            for c, column in enumerate(columns):
                paste(sheet, frame_of(row[column]), c * FRAME_W, r * FRAME_H)
        save(sheet, "People/" + out + ".png")


#: What can be asked for by a word rather than a prop's name.
PARTS = ("title", "questeria", "buildings", "people")


def prop_names():
    """Every prop this cuts, by name: the ones cut whole, the buildings, and the made props."""
    names = {p[0] for p in QUESTERIA_PROPS} | {b[0] for b in BUILDINGS} | {m[0] for m in MADE_PROPS}
    return names | {"crystal-stone"}


if __name__ == "__main__":
    wanted = sys.argv[1:] or ["title", "questeria", "people"]
    names = [a for a in wanted if a not in PARTS]
    for name in names:
        if name not in prop_names():
            sys.exit("No part or prop called '%s'." % name)
    print("Cutting the supplied pictures for Legend Quest...")
    if "title" in wanted:
        build_title()
    if "questeria" in wanted:
        build_questeria()
    elif "buildings" in wanted:
        build_buildings()
    if names:
        build_questeria_props(names)
        build_buildings(names)
    if "people" in wanted:
        build_people()
    print("Done.")
