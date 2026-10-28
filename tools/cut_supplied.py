"""
Cuts the pictures I made with ChatGPT (everything in tools/source/) into the game's layouts: the
title's layers and loops, Questeria's props, and the townsfolk's walking sheets.

    python tools/cut_supplied.py              # everything
    python tools/cut_supplied.py title        # just the title
    python tools/cut_supplied.py questeria    # just Questeria's props
    python tools/cut_supplied.py people       # just the townsfolk

The same idea as The Black Box's serenity-walker. Each thing gets found on its sheet, the flat dark
backdrop gets keyed out by flooding in from the edge of its box (so a dark pixel inside it, like an
outline, stays), and it's sampled down to art pixels by taking the middle colour of each cell, which
keeps the pixel art's own colours instead of blurring them together. The pictures aren't on a real
pixel grid (I measured, and there's no pitch to find), so every cut says how big it should come out.

Only needs CPython. There's nothing random in here, so a rerun writes the same bytes.
"""

import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from pixels import OUT, ROOT, new_image, read_png, write_png  # noqa: E402

SOURCE = os.path.join(ROOT, "tools", "source")

#: The backdrop: a dark, neutral grey, never quite the same twice. Anything brighter than BG_HI in
#: a channel, or with its channels further apart than BG_TINT, is part of the picture.
BG_HI = 28
BG_TINT = 8

#: How much of a cell has to be picture, not backdrop, for the cell to be drawn at all.
KEEP = 0.5

_pictures = {}


def picture(name):
    """A picture out of tools/source, read once however many cuts come out of it."""
    if name not in _pictures:
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

#: Questeria's props, and the harbour's: the name (Props/<name>.png, and what PropCatalog and the
#: map call it), the sheet it's on, the box round it on the sheet, and how wide it comes out in art
#: pixels. The inn is the shop's house, a size bigger. A fifth
#: True means just the biggest piece in the box: the palace and the gate have bits of the next
#: thing on the sheet loose inside their boxes.
QUESTERIA_PROPS = (
    # The big buildings.
    ("palace", "questeria-grand.png", (998, 588, 534, 420), 290, True),
    ("temple", "questeria-grand.png", (368, 615, 314, 390), 176),
    ("city-gate", "questeria-grand.png", (8, 665, 350, 330), 190, True),
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


def isolated(name, box, dark=0, alone=False):
    """The mask of one thing in a box: its biggest piece, and (unless it's to be alone) any other
    piece wholly inside the box (a sign hanging off a stall), but nothing touching the box's edge,
    which is a bit of the thing next to it on the sheet."""
    mask = keyed(name, box, dark)
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
    w, h, _, _ = picture(name)
    x0, y0 = max(0, box[0] - PROP_PAD), max(0, box[1] - PROP_PAD)
    x1, y1 = min(w, box[0] + box[2] + PROP_PAD), min(h, box[1] + box[3] + PROP_PAD)
    padded = (x0, y0, x1 - x0, y1 - y0)
    mask = isolated(name, padded, dark, alone)
    ys = [y for y, line in enumerate(mask) if any(line)]
    xs = [x for x in range(padded[2]) if any(line[x] for line in mask)]
    tight = (x0 + min(xs), y0 + min(ys), max(xs) - min(xs) + 1, max(ys) - min(ys) + 1)
    trimmed = [line[min(xs):max(xs) + 1] for line in mask[min(ys):max(ys) + 1]]
    p = pitch if pitch else tight[2] / width
    size = (max(1, round(tight[2] / p)), max(1, round(tight[3] / p)))
    return sample(name, tight, size, mask=trimmed)


def build_questeria_props():
    for out, name, box, width, *alone in QUESTERIA_PROPS:
        save(cut_thing(name, box, width=width, alone=bool(alone)), "Props/" + out + ".png")

    # The waypoint's two pictures side by side, each standing on the middle of its bottom edge.
    frames = [cut_thing(WAYPOINT_SHEET, box, pitch=WAYPOINT_PITCH, dark=WAYPOINT_DARK) for box in WAYPOINT_FRAMES]
    fw, fh = max(len(f[0]) for f in frames), max(len(f) for f in frames)
    strip = new_image(fw * len(frames), fh)
    for i, f in enumerate(frames):
        paste(strip, f, i * fw + (fw - len(f[0])) // 2, fh - len(f))
    save(strip, "Props/crystal-stone.png")


def build_questeria():
    build_questeria_props()


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


if __name__ == "__main__":
    wanted = sys.argv[1:] or ["title", "questeria", "people"]
    print("Cutting the supplied pictures for Legend Quest...")
    if "title" in wanted:
        build_title()
    if "questeria" in wanted:
        build_questeria()
    if "people" in wanted:
        build_people()
    print("Done.")
