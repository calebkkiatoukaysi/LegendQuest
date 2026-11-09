"""
The floor tilesets in the painted style: every room's floor is a tile strip (the course's tilemap),
and these are its tiles drawn as vectors with paint.py's palette and shading, at 64 x 64 texels a
tile, so a tile is one canvas pixel to a texel on screen (Tilemap draws a tile 16 world pixels
across, and a room is 4 screen pixels to a world pixel) and nothing is blown up.

    python tools/rooms/paint_tiles.py                       # every tileset it can paint, into shots/art/rooms/tiles
    python tools/rooms/paint_tiles.py questeria --sheet     # one, and a sheet of it laid out for review
    python tools/rooms/paint_tiles.py --content             # into Content/Tiles/<id>.png (once Tilemap reads 64-texel tiles)

Each sheet keeps the tileset's name, its tiles' names and the order World/TilesetCatalog.cs lists
them in (read off the catalog through roomkit.tileset, as the placeholders do): every variant of
a tile, each a frame (four for water, which ripples), then its four edges (north, east, south,
west) if it's Edged, eight frames to a row. So the sheet is the old one's layout, four times as
big: questeria.png is 512 x 384 where it was 128 x 96.

The tiles are made to sit side by side in any order the room file and Tilemap's variant hash put
them in: anything that crosses a tile's edge (a cobble across the seam, a board's end) is drawn
the same in every variant, and only what's inside a tile changes from one variant to the next.
Everything is drawn on a canvas three tiles each way and the middle one cut out, so what goes off
one side comes in at the other.

The floor is calmer than the mid layer (12-rooms §3.1): a narrow range of tones, thin joints in a
darker shade of the ground rather than ink, and the light from the top left as everywhere.

Tilesets painted so far: questeria (plaza, terrace, cobble, grass, sand, water, planks). The others
are listed in PAINTERS as they're done; a tileset with a tile nobody paints yet is refused by name.

Deterministic, as paint.py is. Only needs CPython and numpy.
"""

import argparse
import math
import os
import sys

import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
import paint  # noqa: E402
import roomkit  # noqa: E402
from paint import Path, Piece, hash01, ramp, tones  # noqa: E402
from vector import write_png  # noqa: E402

T = 64
"""A tile, in texels on the sheet (16 world pixels, 64 screen pixels: one to one)."""
COLUMNS = 8
SS = 4
OUT = os.path.join(roomkit.ROOT, "shots", "art", "rooms", "tiles")

# The ground's own ramps: soft (paint.ramp's soft < 1) so the floor stays calm.
COBBLE = "#d6c9b8"
SLAB = "#e6dccb"
MORTAR = "#b9ab9c"
TERRACE = "#d9a48e"
GRASS = "#6fb25a"
SAND = "#efd9ae"
WATER = "#3f8fd2"
PLANK = "#b98458"
KERB = "#e3d7c4"


def soft(c, k=0.6):
    return ramp(c, soft=k)


class Tile:
    """One tile being painted: a Piece three tiles each way, (-64, -64) to (128, 128), whose middle
    tile is cut out. `wrap(fn)` calls fn(dx, dy) for the nine copies, so a shape drawn at its
    place and its copies a tile away comes in at the far side of the middle."""

    def __init__(self):
        self.p = Piece(-T, -T, 2 * T, 2 * T, ss=SS, pad=0)

    def wrap(self, fn):
        for dy in (-T, 0, T):
            for dx in (-T, 0, T):
                fn(dx, dy)

    def ground(self, colour):
        self.p.colour(self.p.everything(), colour)

    def image(self):
        img = self.p.c.rgba()
        return img[T:2 * T, T:2 * T].copy()


def _stone(p, x, y, w, h, t, r=5.0, tint=0.0, seed=0, chip=False, calm=False):
    """One cobble or slab: a rounded block with a lit rim on its top left and a shadowed rim on its
    bottom right, and now and then a chip or a crack. A `calm` stone (a cobble, which there are
    many of) has no edge line and softer rims, so the street reads as one surface."""
    jit = lambda i: (hash01(i, 1, seed) - 0.5) * 2.0  # noqa: E731
    pts = [(x + 1 + jit(1), y + 1 + jit(2)), (x + w - 1 + jit(3), y + 1 + jit(4)),
           (x + w - 1 + jit(5), y + h - 1 + jit(6)), (x + 1 + jit(7), y + h - 1 + jit(8))]
    path = Path()
    n = len(pts)
    for i in range(n):
        a, b, c = pts[i - 1], pts[i], pts[(i + 1) % n]
        rr = min(r, abs(b[0] - a[0]) / 2 + abs(b[1] - a[1]) / 2, abs(c[0] - b[0]) / 2 + abs(c[1] - b[1]) / 2)
        da = math.hypot(b[0] - a[0], b[1] - a[1]) or 1
        dc = math.hypot(c[0] - b[0], c[1] - b[1]) or 1
        p0 = (b[0] + (a[0] - b[0]) * rr / da, b[1] + (a[1] - b[1]) * rr / da)
        p1 = (b[0] + (c[0] - b[0]) * rr / dc, b[1] + (c[1] - b[1]) * rr / dc)
        if i == 0:
            path.move(*p0)
        else:
            path.line(*p0)
        path.quad(b[0], b[1], *p1)
    path.close()
    m = p.fill(path, t, "base")
    if abs(tint) >= 0.3:
        p.tone(m, t, "base-light" if tint > 0 else "terminator", alpha=min(0.55 if calm else 0.7, abs(tint)))
    if calm:
        p.tone(m - m.shift(-1.2, -1.6), t, "terminator", alpha=0.9)
        p.tone(m - m.shift(1.0, 1.2), t, "base-light", alpha=0.8)
    else:
        p.tone(m - m.shift(-1.4, -1.8), t, "shadow", alpha=0.8)
        p.tone(m - m.shift(1.2, 1.4), t, "light", alpha=0.55)
        p.edge(path, t, 0.7, "shadow")
    if chip:
        cx, cy = x + w * (0.3 + 0.4 * hash01(1, 9, seed)), y + h * (0.3 + 0.4 * hash01(2, 9, seed))
        p.stroke(Path.polyline([(cx, cy), (cx + 3, cy + 2), (cx + 5, cy + 1.5)]), (0.9, 0.0), t, "shadow", clip=m)
    return m


# --------------------------------------------------------------------------- #
# The painters, by kind
# --------------------------------------------------------------------------- #

def cobble(variant, frame=0, seed=11):
    """Cobbles in four courses of 16, laid like brick: the even courses break at the tile's edge,
    the odd ones have one stone across it (the same in every variant); between, each variant
    picks its own joints and tints."""
    tl = Tile()
    t = soft(COBBLE, 0.5)
    tl.ground(t["terminator"])
    p = tl.p
    for r in range(4):
        y = r * 16
        if r % 2 == 0:
            cuts = [0.0]
            inner = 1 + int(hash01(variant, r, seed + 1) * 2)
            for k in range(inner):
                cuts.append(64.0 * (k + 1) / (inner + 1) + (hash01(variant, r * 7 + k, seed + 2) - 0.5) * 10)
            cuts.append(64.0)
            shared = []
        else:
            cuts = [12.0]
            inner = int(hash01(variant, r, seed + 3) * 2)
            for k in range(inner):
                cuts.append(12 + 40.0 * (k + 1) / (inner + 1) + (hash01(variant, r * 7 + k, seed + 4) - 0.5) * 6)
            cuts.append(52.0)
            shared = [(-12.0, 12.0)]
        for a, b in shared:
            tint = (hash01(r, 0, seed + 5) - 0.5) * 1.2

            def draw(dx, dy, a=a, b=b, tint=tint, r=r):
                _stone(p, a + dx, y + dy, b - a, 16, t, r=6.0, tint=tint, seed=seed + r * 13, calm=True)
            tl.wrap(draw)
        for i in range(len(cuts) - 1):
            a, b = cuts[i], cuts[i + 1]
            tint = (hash01(variant * 31 + i, r, seed + 6) - 0.5) * 1.3
            chip = hash01(variant * 17 + i, r, seed + 7) < 0.15

            def draw(dx, dy, a=a, b=b, tint=tint, chip=chip, i=i, r=r):
                _stone(p, a + dx, y + dy, b - a, 16, t, r=6.0, tint=tint, seed=seed + variant * 97 + r * 11 + i, chip=chip, calm=True)
            tl.wrap(draw)
    return tl.image()


def slabs(variant, frame=0, seed=21, colour=SLAB, inlay=True):
    """Big pale slabs, two courses of 32: one slab across the tile on the upper course, and the lower
    course broken at the middle, so the seams run on; a crack, a worn hollow, a tuft of moss in a
    joint, or (one variant in six) a little diamond of blue and gold let into the stone."""
    tl = Tile()
    t = soft(colour, 0.5)
    tl.ground(MORTAR)
    p = tl.p

    def draw(dx, dy):
        _stone(p, 0 + dx, 0 + dy, 64, 32, t, r=3.5, seed=seed + 1)
        _stone(p, -32 + dx, 32 + dy, 64, 32, t, r=3.5, seed=seed + 2)
        _stone(p, 32 + dx, 32 + dy, 64, 32, t, r=3.5, seed=seed + 3)
    tl.wrap(draw)
    kind = variant % 6
    if kind == 1:
        p.stroke(Path.polyline([(14, 6), (20, 12), (18, 18), (26, 26)]), (1.0, 0.0), t, "deep-shadow")
    elif kind == 2:
        m = p.fill(Path.ellipse(42, 16, 12, 5), t, "terminator", alpha=0.7)
        p.tone(m - m.shift(1, 1.5), t, "shadow", alpha=0.6)
    elif kind == 3:
        g = soft("#7fae5c", 0.7)
        for k in range(5):
            x = 26 + k * 3
            p.stroke(Path.polyline([(x, 33), (x + (k - 2) * 1.2, 27 - 2 * hash01(k, 1, seed))]), (1.6, 0.0), g, "base")
    elif kind == 4:
        p.stroke(Path.polyline([(50, 40), (46, 48), (52, 56)]), (1.0, 0.0), t, "deep-shadow")
    elif kind == 5 and inlay:
        gold = soft("#e9b54c", 0.6)
        blue = soft("#5f8fd8", 0.6)
        d = Path.polygon([(32, 6), (46, 16), (32, 26), (18, 16)])
        p.fill(d, gold, "base")
        p.fill(Path.polygon([(32, 10), (41, 16), (32, 22), (23, 16)]), blue, "base")
        p.fill(Path.polygon([(32, 13), (36, 16), (32, 19), (28, 16)]), gold, "light")
        p.edge(d, gold, 0.9, "shadow")
    return tl.image()


def terrace(variant, frame=0, seed=31):
    """A terrace of small terracotta squares, alternate ones a shade apart, each lit on its top left."""
    tl = Tile()
    t = soft(TERRACE, 0.5)
    tl.ground("#a87868")
    p = tl.p
    for r in range(4):
        for c in range(4):
            tint = 0.5 if (r + c) % 2 else -0.4
            if hash01(variant * 7 + c, r, seed) < 0.12:
                tint = 0.0
            _stone(p, c * 16, r * 16, 16, 16, t, r=2.0, tint=tint, seed=seed + variant * 41 + r * 5 + c)
    return tl.image()


def grass(variant, frame=0, seed=41, colour=GRASS, flowers=True):
    """Grass: a lawn green with tufts of blades, dark at their roots and lit at their tips, and a few
    small flowers; the tufts that cross an edge are shared, the rest are the variant's."""
    tl = Tile()
    t = soft(colour, 0.6)
    tl.ground(t["base"])
    p = tl.p

    def tuft(cx, cy, s, sd):
        blades = Path()
        for k in range(5):
            a = math.radians(-90 + (k - 2) * 22 + (hash01(k, 1, sd) - 0.5) * 14)
            ln = s * (0.7 + 0.5 * hash01(k, 2, sd))
            blades.add(Path.polyline([(cx + (k - 2) * 1.6, cy), (cx + (k - 2) * 1.6 + math.cos(a) * ln, cy + math.sin(a) * ln)]))
        p.stroke(blades, (2.4, 0.0), t, "shadow")
        p.stroke(blades.translate(-0.6, -0.8), (1.6, 0.0), t, "light")

    shared = [(0.0, 10.0), (64.0 * 0.62, 0.0), (0.0, 50.0)]
    for i, (sx, sy) in enumerate(shared):
        tl.wrap(lambda dx, dy, sx=sx, sy=sy, i=i: tuft(sx + dx, sy + dy, 9, seed + i))
    for i in range(7):
        x = 6 + 52 * hash01(variant, i, seed + 1)
        y = 8 + 50 * hash01(variant, i, seed + 2)
        s = 6 + 5 * hash01(variant, i, seed + 3)
        # a soft patch of darker grass under some
        if hash01(variant, i, seed + 4) < 0.4:
            p.fill(Path.ellipse(x, y + 1, 9, 3.5), t, "terminator", alpha=0.6)
        tuft(x, y, s, seed + variant * 19 + i)
    if flowers and variant % 2 == 1:
        for i in range(2):
            x = 12 + 40 * hash01(variant, i, seed + 5)
            y = 14 + 36 * hash01(variant, i, seed + 6)
            c = tones(("flower.white", "flower.yellow", "flower.pink")[int(hash01(i, variant, seed + 7) * 3)])
            for k in range(4):
                a = k * math.pi / 2
                p.fill(Path.circle(x + math.cos(a) * 1.6, y + math.sin(a) * 1.6, 1.4), c, "base")
            p.fill(Path.circle(x, y, 0.9), tones("flower.yellow"), "shadow")
    return tl.image()


def sand(variant, frame=0, seed=51):
    """Sand: warm and pale, with soft ripples curving across and a pebble or a shell."""
    tl = Tile()
    t = soft(SAND, 0.5)
    tl.ground(t["base"])
    p = tl.p
    for r in range(4):
        y = 8 + r * 16

        def ripple(dx, dy, y=y, r=r):
            pts = [(x + dx, y + dy + 2.5 * math.sin(x / 64.0 * 2 * math.pi + r)) for x in range(-4, 69, 4)]
            line = Path.smooth(pts)
            p.stroke(line, 1.6, t, "terminator", alpha=0.7)
            p.stroke(line.translate(0, -1.6), 1.0, t, "light", alpha=0.8)
        tl.wrap(ripple)
    for i in range(1 + variant):
        x = 10 + 44 * hash01(variant, i, seed)
        y = 10 + 44 * hash01(variant, i, seed + 1)
        st = soft("#b9a690", 0.6)
        m = p.fill(Path.ellipse(x, y, 2.6, 1.8), st, "base")
        p.tone(m - m.shift(-0.6, -0.6), st, "shadow")
    return tl.image()


def water(variant, frame=0, seed=61):
    """Water: a calm blue in soft bands, and dashes of light lying on it that drift a quarter of a
    tile each frame, so four frames loop; the variants move their dashes about."""
    tl = Tile()
    t = ramp(WATER, soft=0.7)
    tl.ground(t["base"])
    p = tl.p
    for r in range(4):
        p.fill(Path.rect(-T, r * 16 + 10, 3 * T, 4), t, "terminator", alpha=0.5)
    for i in range(5):
        x = (64 * hash01(variant, i, seed) + frame * 16) % 64
        y = 6 + 52 * hash01(variant, i, seed + 1)
        ln = 8 + 10 * hash01(variant, i, seed + 2)

        def dash(dx, dy, x=x, y=y, ln=ln):
            p.stroke(Path.polyline([(x - ln / 2 + dx, y + dy), (x + ln / 2 + dx, y + dy)]), lambda u: 2.2 * math.sin(math.pi * u), t, "light")
            p.stroke(Path.polyline([(x - ln / 4 + dx, y + dy - 0.4), (x + ln / 4 + dx, y + dy - 0.4)]), lambda u: 1.2 * math.sin(math.pi * u), t, "highlight")
        tl.wrap(dash)
    return tl.image()


def planks(variant, frame=0, seed=71, colour=PLANK):
    """A boardwalk (or a wooden floor): four boards running along, their ends staggered on the
    same marks in every variant, with nails at the ends, grain, and now and then a knot."""
    tl = Tile()
    t = soft(colour, 0.6)
    tl.ground(t["deep"])
    p = tl.p
    ends = (0.0, 40.0, 16.0, 56.0)
    for r in range(4):
        y = r * 16
        e = ends[r]
        tint = (hash01(r, variant, seed) - 0.5) * 1.2

        def board(dx, dy, e=e, y=y, tint=tint, r=r):
            x0 = e - 64 + dx
            path = Path.rect(x0 + 0.8, y + dy + 0.8, 64 - 1.6, 14.4, r=1.5)
            which = "base" if abs(tint) < 0.35 else ("base-light" if tint > 0 else "terminator")
            m = p.fill(path, t, which)
            p.tone(m - m.shift(0, -1.6), t, "shadow")
            p.tone(m - m.shift(0, 1.4), t, "light", alpha=0.8)
            p.edge(path, t, 0.8, "deep-shadow")
            for k in range(2):
                gy = y + dy + 5 + k * 5 + hash01(r, k, seed + 1) * 2
                p.stroke(Path.smooth([(x0 + 6, gy), (x0 + 30, gy + 1.0), (x0 + 58, gy - 0.5)]), 0.8, t, "terminator", clip=m, alpha=0.8)
            for nx in (x0 + 3.5, x0 + 60.5):
                for ny in (y + dy + 4.5, y + dy + 11.5):
                    p.fill(Path.circle(nx, ny, 1.1), t, "deep")
        board(0, 0)
        board(64, 0)
        board(-64, 0)
        if hash01(variant, r, seed + 2) < 0.35:
            kx = (ends[r] + 12 + 40 * hash01(variant, r, seed + 3)) % 64
            m = p.fill(Path.ellipse(kx, y + 8, 3.2, 2.2), t, "deep-shadow")
            p.stroke(Path.ellipse(kx, y + 8, 5.4, 3.4), 0.8, t, "terminator")
            del m
    return tl.image()


# --- edges: drawn over a tile's own cell, along the side where it meets other ground ------- #

def _edge_strip(side, colour, depth=10, kind="kerb", seed=0):
    """One of a tile's four edge frames (0 north, 1 east, 2 south, 3 west), clear except along
    that side: a kerb of cut stone (its lit top and its face), a quay's coping over water, or a
    beam along a boardwalk."""
    tl = Tile()
    p = tl.p
    t = soft(colour, 0.6)
    horiz = side in (0, 2)
    if horiz:
        y0 = 0 if side == 0 else 64 - depth
        for k in range(3):
            x = -16 + k * 32 + (16 if side == 2 else 0)
            path = Path.rect(x + 0.6, y0, 32 - 1.2, depth, r=1.5)
            m = p.fill(path, t, "base")
            top = p.shape(Path.rect(x, y0, 32, depth * 0.4))
            p.tone(m & top, t, "light")
            p.tone(p.below(m, y0 + depth - 2), t, "shadow")
            p.edge(path, t, 0.9, "deep-shadow")
        if kind == "quay":
            p.fill(Path.rect(-T, y0 + depth if side == 0 else y0 - 3, 3 * T, 3), soft(WATER, 0.7), "deep", alpha=0.7)
        if kind == "beam":
            p.c.cov[:] = 0
            bt = soft(PLANK, 0.6)
            path = Path.rect(-T, y0, 3 * T, depth)
            m = p.fill(path, bt, "terminator")
            p.tone(p.above(m, y0 + 2), bt, "light")
            p.edge(path, bt, 0.9, "deep")
            for nx in range(4, 64, 16):
                p.fill(Path.circle(nx, y0 + depth / 2, 1.2), bt, "deep")
    else:
        x0 = 0 if side == 3 else 64 - depth
        for k in range(3):
            y = -16 + k * 32
            path = Path.rect(x0, y + 0.6, depth, 32 - 1.2, r=1.5)
            m = p.fill(path, t, "base")
            p.tone(m - m.shift(1.4, 0), t, "light" if side == 3 else "shadow")
            p.edge(path, t, 0.9, "deep-shadow")
        if kind == "beam":
            p.c.cov[:] = 0
            bt = soft(PLANK, 0.6)
            path = Path.rect(x0, -T, depth, 3 * T)
            m = p.fill(path, bt, "terminator")
            p.edge(path, bt, 0.9, "deep")
            del m
    return tl.image()


def kerb_edge(side, variant=0, seed=0):
    return _edge_strip(side, KERB, 10, "kerb", seed)


def quay_edge(side, variant=0, seed=0):
    return _edge_strip(side, KERB, 9, "quay", seed)


def beam_edge(side, variant=0, seed=0):
    return _edge_strip(side, PLANK, 8, "beam", seed)


# --------------------------------------------------------------------------- #
# The other sets: generic painters, each parameterised by colour and kind
# --------------------------------------------------------------------------- #

class Big(Tile):
    """A block of n x n tiles being painted as one picture, wrapped on its own period (a block
    repeats every n tiles), or not wrapped (wrap=False) for a block too big to paint three times
    over; cut into its n x n frames, row by row, by cells()."""

    def __init__(self, n, wrap=True, ss=SS):
        self.n, self.size, self.wrapped = n, n * T, wrap
        s = self.size
        self.p = Piece(-s, -s, 2 * s, 2 * s, ss=ss, pad=0) if wrap else Piece(0, 0, s, s, ss=ss, pad=0)

    def wrap(self, fn):
        offs = (-self.size, 0, self.size) if self.wrapped else (0,)
        for dy in offs:
            for dx in offs:
                fn(dx, dy)

    def image(self):
        img = self.p.c.rgba()
        s = self.size
        return img[s:2 * s, s:2 * s].copy() if self.wrapped else img.copy()

    def cells(self):
        img = self.image()
        return [img[y * T:(y + 1) * T, x * T:(x + 1) * T].copy() for y in range(self.n) for x in range(self.n)]


def _blot(p, x, y, rx, ry, t, which, alpha, rot=0.0):
    p.fill(Path.ellipse(x, y, rx, ry, rot=rot), t, which, alpha=alpha)


def _patches(tl, t, seed, variant, n_shared=3, n_own=4, size=(8, 16), alpha=0.4):
    """Soft blotches a shade darker or lighter: some shared (wrapped, crossing the edges the same in
    every variant), the rest the variant's own, kept inside."""
    p = tl.p
    for i in range(n_shared):
        x, y = 64 * hash01(i, 1, seed + 901), 64 * hash01(i, 2, seed + 901)
        rx = size[0] + (size[1] - size[0]) * hash01(i, 3, seed + 901)
        which = "terminator" if i % 2 == 0 else "base-light"
        tl.wrap(lambda dx, dy, x=x, y=y, rx=rx, which=which: _blot(p, x + dx, y + dy, rx, rx * 0.45, t, which, alpha))
    for i in range(n_own):
        rx = size[0] + (size[1] - size[0]) * hash01(variant, i, seed + 903) * 0.6
        x = rx + 2 + (60 - 2 * rx) * hash01(variant, i, seed + 904)
        y = 8 + 48 * hash01(variant, i, seed + 905)
        _blot(p, x, y, rx, rx * 0.42, t, "terminator" if (i + variant) % 2 else "base-light", alpha)


def _pebble(p, x, y, r, t, seed=0):
    m = p.fill(Path.ellipse(x, y, r, r * 0.68, rot=(hash01(1, 1, seed) - 0.5) * 40), t, "base")
    p.tone(m - m.shift(-0.7, -0.9), t, "shadow")
    p.tone(m - m.shift(0.6, 0.7), t, "light", alpha=0.8)
    return m


def _tuft(p, cx, cy, s, t, sd, blades=5, spread=22, dead=False):
    path = Path()
    for k in range(blades):
        a = math.radians(-90 + (k - blades // 2) * spread + (hash01(k, 1, sd) - 0.5) * 14)
        ln = s * (0.7 + 0.5 * hash01(k, 2, sd))
        bx = cx + (k - blades // 2) * 1.6
        if dead:
            path.add(Path.polyline([(bx, cy), (bx + math.cos(a) * ln * 0.6, cy + math.sin(a) * ln * 0.6),
                                    (bx + math.cos(a) * ln * 0.6 + 3, cy + math.sin(a) * ln * 0.75)]))
        else:
            path.add(Path.polyline([(bx, cy), (bx + math.cos(a) * ln, cy + math.sin(a) * ln)]))
    p.stroke(path, (2.4, 0.0), t, "shadow")
    p.stroke(path.translate(-0.6, -0.8), (1.6, 0.0), t, "light")


def _leaf(p, x, y, size, angle, t, vein=None):
    a = math.radians(angle)
    path = Path.ellipse(x, y, size, size * 0.45, rot=angle)
    m = p.fill(path, t, "base")
    p.tone(m - m.shift(-0.8, -1.0), t, "shadow")
    rib = Path.polyline([(x - math.cos(a) * size * 0.9, y - math.sin(a) * size * 0.9), (x + math.cos(a) * size * 0.9, y + math.sin(a) * size * 0.9)])
    p.stroke(rib, 0.7, vein or t, "shadow" if vein is None else "base", clip=m)
    p.edge(path, t, 0.6, "shadow")
    return m


def earth(variant, frame=0, seed=101, colour="#b8946a", pebbles=3, ruts=False, furrows=False, roots=False, patches=True):
    """Bare ground (a path, a road, a trail, soil, a tunnel's floor): earth in soft blotches, a few
    pebbles, and the road's two ruts or the field's furrows running across."""
    tl = Tile()
    t = soft(colour, 0.5)
    tl.ground(t["base"])
    p = tl.p
    if patches:
        _patches(tl, t, seed, variant)
    if ruts:
        for y in (20.0, 42.0):
            tl.wrap(lambda dx, dy, y=y: p.fill(Path.rect(-T + dx, y + dy, 3 * T, 5, r=2), t, "terminator", alpha=0.8))
            tl.wrap(lambda dx, dy, y=y: p.fill(Path.rect(-T + dx, y + 4.5 + dy, 3 * T, 1.6), t, "light", alpha=0.7))
    if furrows:
        for r in range(4):
            y = r * 16

            def furrow(dx, dy, y=y):
                p.fill(Path.rect(-T + dx, y + 9 + dy, 3 * T, 6), t, "shadow", alpha=0.85)
                p.fill(Path.rect(-T + dx, y + 2 + dy, 3 * T, 3), t, "light", alpha=0.7)
            tl.wrap(furrow)
    if roots:
        rt = soft("#7a5440", 0.6)
        for i in range(1):
            y = 34 + 6 * hash01(i, 1, seed)
            tl.wrap(lambda dx, dy, y=y, i=i: p.stroke(Path.smooth([(-6 + dx, y + dy), (20 + dx, y + 5 + dy), (44 + dx, y - 3 + dy), (70 + dx, y + dy)]),
                                                      2.6, rt, "base"))
            tl.wrap(lambda dx, dy, y=y: p.stroke(Path.smooth([(-6 + dx, y - 1 + dy), (20 + dx, y + 4 + dy), (44 + dx, y - 4 + dy), (70 + dx, y - 1 + dy)]),
                                                 0.9, rt, "light"))
    st = soft("#a39684", 0.6)
    for i in range(pebbles):
        x = 8 + 48 * hash01(variant, i, seed + 7)
        y = 8 + 48 * hash01(variant, i, seed + 8)
        _pebble(p, x, y, 1.6 + 1.6 * hash01(variant, i, seed + 9), st, seed + i)
    return tl.image()


def lawn(variant, frame=0, seed=41, colour=GRASS, flowers=True, tufts=7, size=(6, 11), dead=False, long=False):
    """Grass of any kind (the forest's, the meadow's, the Mirewood's wilting and rotten ones): a
    ground green with tufts, the shared ones crossing the edges, and a few flowers."""
    tl = Tile()
    t = soft(colour, 0.6)
    tl.ground(t["base"])
    p = tl.p
    _patches(tl, t, seed, variant, 2, 3, (9, 15), 0.35)
    s0, s1 = size
    shared = [(0.0, 10.0), (64.0 * 0.62, 0.0), (0.0, 50.0)] + ([(32.0, 0.0), (0.0, 30.0)] if long else [])
    for i, (sx, sy) in enumerate(shared):
        tl.wrap(lambda dx, dy, sx=sx, sy=sy, i=i: _tuft(p, sx + dx, sy + dy, s1 - 1, t, seed + i, dead=dead))
    for i in range(tufts):
        x = 6 + 52 * hash01(variant, i, seed + 1)
        y = 8 + 50 * hash01(variant, i, seed + 2)
        s = s0 + (s1 - s0) * hash01(variant, i, seed + 3)
        _tuft(p, x, y, s, t, seed + variant * 19 + i, blades=7 if long else 5, dead=dead)
    if flowers and variant % 2 == 1:
        for i in range(2):
            x = 12 + 40 * hash01(variant, i, seed + 5)
            y = 14 + 36 * hash01(variant, i, seed + 6)
            c = tones(("flower.white", "flower.yellow", "flower.pink")[int(hash01(i, variant, seed + 7) * 3)])
            for k in range(4):
                a = k * math.pi / 2
                p.fill(Path.circle(x + math.cos(a) * 1.6, y + math.sin(a) * 1.6, 1.4), c, "base")
            p.fill(Path.circle(x, y, 0.9), tones("flower.yellow"), "shadow")
    return tl.image()


def leaves(variant, frame=0, seed=111, ground="#7a6848", colours=("#b98a52", "#c6a160", "#a0704a", "#8e9a58"), vein=None, sheen=None):
    """Fallen leaves over dark earth: the leaves across an edge the same in every variant, the rest
    the variant's; blackvein leaves with dark veins and a violet sheen."""
    tl = Tile()
    t = soft(ground, 0.5)
    tl.ground(t["base"])
    p = tl.p
    _patches(tl, t, seed, variant, 2, 2, (10, 16), 0.45)
    vt = tones(vein) if vein else None
    for i in range(6):
        x, y = 64 * hash01(i, 1, seed + 11), 64 * hash01(i, 2, seed + 11)
        if 8 < x < 56 and 8 < y < 56:
            continue
        lt = soft(colours[i % len(colours)], 0.45)
        ang = 360 * hash01(i, 3, seed + 11)
        tl.wrap(lambda dx, dy, x=x, y=y, lt=lt, ang=ang: _leaf(p, x + dx, y + dy, 6.5, ang, lt, vt))
    for i in range(5):
        x = 9 + 46 * hash01(variant, i, seed + 12)
        y = 9 + 46 * hash01(variant, i, seed + 13)
        lt = soft(colours[int(hash01(variant, i, seed + 14) * len(colours))], 0.45)
        _leaf(p, x, y, 5.0 + 2.0 * hash01(variant, i, seed + 15), 360 * hash01(variant, i, seed + 16), lt, vt)
    if sheen:
        for i in range(2):
            x = 14 + 36 * hash01(variant, i, seed + 17)
            y = 14 + 36 * hash01(variant, i, seed + 18)
            p.fill(Path.ellipse(x, y, 7, 2.2, rot=-20), tones(sheen), "light", alpha=0.45)
    return tl.image()


def moss(variant, frame=0, seed=121, colour="#5f9a5a", ground=None, speck="#3e6e4c"):
    """Moss: soft cushions a shade lighter, heaped over the ground, with dark specks."""
    tl = Tile()
    t = soft(colour, 0.6)
    tl.ground((ground and soft(ground, 0.6)["base"]) or t["terminator"])
    p = tl.p
    for i in range(8):
        x, y = 64 * hash01(i, 1, seed + 21), 64 * hash01(i, 2, seed + 21)
        r = 8 + 6 * hash01(i, 3, seed + 21)

        def cushion(dx, dy, x=x, y=y, r=r):
            m = p.fill(Path.ellipse(x + dx, y + dy, r, r * 0.62), t, "base")
            p.tone(m - m.shift(-1.2, -1.6), t, "shadow", alpha=0.8)
            p.tone(m - m.shift(1.0, 1.4), t, "light", alpha=0.7)
        tl.wrap(cushion)
    sp = tones(speck)
    for i in range(10):
        x = 6 + 52 * hash01(variant, i, seed + 22)
        y = 6 + 52 * hash01(variant, i, seed + 23)
        p.fill(Path.circle(x, y, 0.9 + 0.6 * hash01(variant, i, seed + 24)), sp, "base", alpha=0.8)
    return tl.image()


def thicket(variant, frame=0, seed=131, colour="#2f6a46", berries=None, thorns=False, dead=False):
    """A mass you can't walk through (undergrowth, a hedge, rotting brush): round clumps of leaves
    packed edge to edge, lit on their top left; a fixed layout so it runs on across tiles, with
    the variant's berries or thorns over it."""
    tl = Tile()
    t = soft(colour, 0.6)
    tl.ground(t["deep"])
    p = tl.p
    for r in range(3):
        for c in range(3):
            x = c * 21.3 + (10.6 if r % 2 else 0) + (hash01(c, r, seed + 31) - 0.5) * 6
            y = r * 21.3 + 4 + (hash01(c, r, seed + 32) - 0.5) * 5
            rr = 12 + 3 * hash01(c, r, seed + 33)

            def clump(dx, dy, x=x, y=y, rr=rr):
                m = p.fill(Path.circle(x + dx, y + dy, rr), t, "base")
                p.tone(m - p.shape(Path.circle(x + dx - rr * 0.16, y + dy - rr * 0.24, rr * 1.02)), t, "shadow")
                p.tone(m & p.shape(Path.circle(x + dx - rr * 0.32, y + dy - rr * 0.36, rr * 0.5)), t, "light")
                p.stroke(Path().arc(x + dx, y + dy, rr - 0.6, -15, 165), lambda u: 1.3 * math.sin(math.pi * u) ** 0.6, t, "line")
            tl.wrap(clump)
    if dead:
        dt = soft("#6e5a44", 0.6)
        for i in range(4):
            x = 10 + 44 * hash01(variant, i, seed + 34)
            y = 10 + 44 * hash01(variant, i, seed + 35)
            p.stroke(Path.polyline([(x - 6, y + 4), (x, y - 2), (x + 7, y - 6)]), (1.8, 0.4), dt, "base")
    if berries:
        bt = tones(berries)
        for i in range(5):
            x = 8 + 48 * hash01(variant, i, seed + 36)
            y = 8 + 48 * hash01(variant, i, seed + 37)
            m = p.fill(Path.circle(x, y, 2.0), bt, "base")
            p.fill(Path.circle(x - 0.6, y - 0.7, 0.7), bt, "highlight")
            del m
    if thorns:
        tt = soft("#5a3a3e", 0.6)
        for i in range(4):
            x = 8 + 48 * hash01(variant, i, seed + 38)
            y = 8 + 48 * hash01(variant, i, seed + 39)
            p.stroke(Path.smooth([(x - 9, y + 3), (x, y - 2), (x + 9, y + 2)]), 1.4, tt, "base")
            for k in (-5, 0, 5):
                p.stroke(Path.polyline([(x + k, y - 0.5), (x + k + 1.5, y - 3.5)]), (1.2, 0.0), tt, "base")
    return tl.image()


def liquid(variant, frame=0, seed=141, colour=WATER, bands=True, pebbles=False, sheen=None, foam=False, drift=16, dashes=5):
    """Water, or tar: a ground colour in soft bands with dashes of light lying on it drifting a
    quarter of a tile a frame (four frames loop); a ford shows pebbles under it, tar an oily
    sheen of violet and teal for its light."""
    tl = Tile()
    t = ramp(colour, soft=0.7)
    tl.ground(t["base"])
    p = tl.p
    if pebbles:
        st = soft("#9fb0b8", 0.6)
        for i in range(5):
            x = 8 + 48 * hash01(variant, i, seed + 41)
            y = 8 + 48 * hash01(variant, i, seed + 42)
            m = _pebble(p, x, y, 3 + 2 * hash01(variant, i, seed + 43), st, seed + i)
            p.tone(m, t, "base", alpha=0.45)
    if bands:
        for r in range(4):
            p.fill(Path.rect(-T, r * 16 + 10, 3 * T, 4), t, "terminator", alpha=0.45)
    lt = tones(sheen) if sheen else t
    for i in range(dashes):
        x = (64 * hash01(variant, i, seed) + frame * drift) % 64
        y = 6 + 52 * hash01(variant, i, seed + 1)
        ln = 8 + 10 * hash01(variant, i, seed + 2)

        def dash(dx, dy, x=x, y=y, ln=ln, i=i):
            p.stroke(Path.polyline([(x - ln / 2 + dx, y + dy), (x + ln / 2 + dx, y + dy)]), lambda u: 2.2 * math.sin(math.pi * u), lt, "light" if not sheen else "base", alpha=0.9 if sheen else 1.0)
            p.stroke(Path.polyline([(x - ln / 4 + dx, y + dy - 0.4), (x + ln / 4 + dx, y + dy - 0.4)]), lambda u: 1.2 * math.sin(math.pi * u), lt, "highlight", alpha=0.8 if sheen else 1.0)
        tl.wrap(dash)
    if foam:
        ft = tones("#f2f6fa")
        for i in range(3):
            x = (64 * hash01(i, variant, seed + 44) + frame * 8) % 64
            y = 10 + 44 * hash01(i, variant, seed + 45)
            tl.wrap(lambda dx, dy, x=x, y=y: p.fill(Path.ellipse(x + dx, y + dy, 4, 1.4), ft, "base", alpha=0.6))
    return tl.image()


def marble(variant, frame=0, seed=151, colour="#ece7f1", gold=False, square=32):
    """Polished marble in squares that meet on the tile's edges, so any variant sits by any other:
    each square a touch lighter or darker, soft veins wandering in it, thin joints; with gold, gold
    joints and a gold stud where four squares meet."""
    tl = Tile()
    t = soft(colour, 0.45)
    tl.ground(t["shadow"])
    p = tl.p
    n = 64 // square
    vt = soft("#b9aac8", 0.6)
    for r in range(n):
        for c in range(n):
            x, y = c * square, r * square
            tint = (hash01(variant * 7 + c, r, seed) - 0.5) * 0.9
            path = Path.rect(x + 0.7, y + 0.7, square - 1.4, square - 1.4, r=1.0)
            m = p.fill(path, t, "base" if abs(tint) < 0.3 else ("base-light" if tint > 0 else "terminator"))
            p.tone(m - m.shift(1.0, 1.0), t, "light", alpha=0.7)
            for k in range(2):
                a = hash01(variant * 11 + k, r * 3 + c, seed + 1)
                vy = y + square * (0.2 + 0.6 * a)
                vein = Path.smooth([(x + 2, vy), (x + square * 0.35, vy + (a - 0.5) * 10), (x + square * 0.7, vy - 4 + 8 * hash01(k, c, seed + 2)), (x + square - 2, vy + 3)])
                p.stroke(vein, (1.0, 0.2), vt, "base", clip=m, alpha=0.55)
    if gold:
        g = soft("#e9b54c", 0.6)
        joints = Path()
        for k in range(n + 1):
            joints.add(Path.polyline([(k * square, -2), (k * square, 66)]))
            joints.add(Path.polyline([(-2, k * square), (66, k * square)]))
        p.stroke(joints, 2.0, g, "base")
        for r in range(n + 1):
            for c in range(n + 1):
                d = Path.polygon([(c * square, r * square - 3.5), (c * square + 3.5, r * square), (c * square, r * square + 3.5), (c * square - 3.5, r * square)])
                p.fill(d, g, "light")
    return tl.image()


def _partition(seed, size=128, unit=16, minw=32, minh=16):
    """A block cut into slabs: rectangles on a 16-texel grid filling the block, as a list of
    (x, y, w, h); the same for the same seed, so the floor, its worn and its shadowed versions
    share one layout per variant."""
    rects = []
    rows = []
    y = 0
    r = 0
    while y < size:
        h = (2 if hash01(r, 1, seed) < 0.6 else 1) * 16 if size - y >= 32 else size - y
        h = max(minh, min(h * (2 if h == 16 and hash01(r, 2, seed) < 0.3 else 1), size - y))
        rows.append((y, h))
        y += h
        r += 1
    for k, (y, h) in enumerate(rows):
        x = 0
        c = 0
        while x < size:
            w = unit * (2 + int(hash01(c, k, seed + 3) * 4))
            if size - x - w < minw:
                w = size - x
            rects.append((x, y, w, h))
            x += w
            c += 1
    return rects


def flagstones(variant, frame=0, seed=161, colour="#a6a4b4", block=2, kind="plain", trim=None):
    """A floor of stone slabs of every size, laid as a block of 2 x 2 tiles (TilesetCatalog's
    floors): the block's own layout per variant, joints on the block's edge, so blocks of any
    variant sit side by side. kind: plain, worn (hollowed, lighter, smoother), shadow (in a wall's
    shadow), cracked, vein (black veins with a violet glow), gold (a gold line let in round each)."""
    b = Big(block)
    t = soft(colour, 0.5)
    b.ground(t["shadow"])
    p = b.p
    for i, (x, y, w, h) in enumerate(_partition(seed * 7 + variant, block * T)):
        tint = (hash01(i, variant, seed + 1) - 0.5) * 1.1
        m = _stone(p, x, y, w, h, t, r=3.0, tint=tint, seed=seed + variant * 53 + i, chip=hash01(i, variant, seed + 2) < 0.2)
        if kind == "gold" and trim:
            g = soft(trim, 0.6)
            p.stroke(Path.rect(x + 5, y + 5, w - 10, h - 10, r=1), 1.2, g, "base", clip=m)
        if kind == "worn" and hash01(i, variant, seed + 3) < 0.6:
            p.fill(Path.ellipse(x + w / 2, y + h / 2, w * 0.3, h * 0.22), t, "base-light", clip=m, alpha=0.35)
        if kind == "cracked" and hash01(i, variant, seed + 4) < 0.55:
            cx, cy = x + w * 0.3, y + h * 0.25
            p.stroke(Path.polyline([(cx, cy), (cx + w * 0.15, cy + h * 0.25), (cx + w * 0.08, cy + h * 0.45), (cx + w * 0.3, cy + h * 0.62)]), (1.3, 0.2), t, "deep", clip=m)
    if kind == "shadow":
        p.colour(p.everything(), "#3a3054", alpha=0.32)
    if kind == "vein":
        vt = tones("#2a1f33")
        glow = tones("#8a5ac8")
        for i in range(2):
            y0 = 20 + 80 * hash01(i, variant, seed + 5)
            pts = [(-4, y0)] + [(x, y0 + 14 * (hash01(i * 9 + k, variant, seed + 6) - 0.5)) for k, x in enumerate(range(16, 140, 22))]
            path = Path.smooth(pts)
            p.stroke(path, 5.0, glow, "base", alpha=0.25)
            p.stroke(path, (2.4, 1.0), vt, "base")
    return b.cells()


def steps(variant, frame=0, seed=171, colour="#b8b0bc"):
    """Steps: treads running across, each lit along its nosing, its riser in shadow."""
    tl = Tile()
    t = soft(colour, 0.5)
    tl.ground(t["shadow"])
    p = tl.p
    for r in range(4):
        y = r * 16

        def tread(dx, dy, y=y):
            m = p.fill(Path.rect(-T + dx, y + dy, 3 * T, 10), t, "base")
            p.fill(Path.rect(-T + dx, y + dy, 3 * T, 2.2), t, "light")
            p.fill(Path.rect(-T + dx, y + 10 + dy, 3 * T, 6), t, "terminator")
            del m
        tl.wrap(tread)
    for k in (16, 48):
        p.stroke(Path.polyline([(k, 0), (k, 64)]), 0.8, t, "shadow", alpha=0.6)
    return tl.image()


def carpet(variant, frame=0, seed=181, colour="#a8323e", pattern="#e9b54c", motif="diamond"):
    """A carpet's field: a deep colour with a small repeating motif in its pattern colour, aligned
    to the tile so it runs on, and a soft pile shading."""
    tl = Tile()
    t = soft(colour, 0.6)
    tl.ground(t["base"])
    p = tl.p
    g = soft(pattern, 0.6)
    for r in range(2):
        for c in range(2):
            x, y = 16 + c * 32, 16 + r * 32
            if motif == "diamond":
                d = Path.polygon([(x, y - 7), (x + 7, y), (x, y + 7), (x - 7, y)])
                p.fill(d, g, "base", alpha=0.85)
                p.fill(Path.polygon([(x, y - 3), (x + 3, y), (x, y + 3), (x - 3, y)]), t, "shadow")
            else:
                p.stroke(Path.circle(x, y, 6), 1.6, g, "base", alpha=0.85)
                p.fill(Path.circle(x, y, 2), g, "light")
    for k in range(4):
        p.fill(Path.rect(-T, k * 16 + 12, 3 * T, 2), t, "terminator", alpha=0.25)
    return tl.image()


def border_edge(colour, inner="#e9b54c", depth=11):
    """A carpet's (or a rug's, a dais', an inlay's) edge frames: a band of its border colour with a
    fine line each side, along the side where it meets other ground."""
    def fn(side, variant=0, seed=0):
        tl = Tile()
        p = tl.p
        t = soft(colour, 0.6)
        g = soft(inner, 0.6)
        if side in (0, 2):
            y0 = 0 if side == 0 else 64 - depth
            p.fill(Path.rect(-T, y0, 3 * T, depth), g, "base")
            p.fill(Path.rect(-T, y0 + (1.5 if side == 0 else depth - 3.5), 3 * T, 2), t, "deep")
            p.fill(Path.rect(-T, y0 + (depth - 2 if side == 0 else 0), 3 * T, 2), g, "shadow")
            for x in range(4, 64, 12):
                p.fill(Path.circle(x, y0 + depth / 2, 1.3), t, "base")
        else:
            x0 = 0 if side == 3 else 64 - depth
            p.fill(Path.rect(x0, -T, depth, 3 * T), g, "base")
            p.fill(Path.rect(x0 + (1.5 if side == 3 else depth - 3.5), -T, 2, 3 * T), t, "deep")
            for y in range(4, 64, 12):
                p.fill(Path.circle(x0 + depth / 2, y, 1.3), t, "base")
        return tl.image()
    return fn


def wall_face(variant, frame=0, seed=191, colour="#e8dcc8", kind="stone", course=32, extra=None):
    """A wall's face, a tile's worth (an interior's back wall, a crypt's, the keep's, the vault's,
    the quarry's rock): ashlar courses (stone), plaster over a hint of stones (plaster), wood boards
    (boards) or rough rock in strata (rock); courses meet on the tile's edges."""
    tl = Tile()
    t = soft(colour, 0.55)
    tl.ground(t["base"])
    p = tl.p
    if kind == "stone":
        for r in range(64 // course):
            y = r * course
            off = 32 if r % 2 else 0

            def block(dx, dy, y=y, off=off, r=r):
                for k in (-1, 0, 1):
                    x = off + k * 64
                    _stone(p, x + dx, y + dy, 64, course, t, r=2.0, tint=(hash01(r, k, seed) - 0.5) * 0.8, seed=seed + r * 5 + k)
            tl.wrap(block)
        if variant == 1:
            p.stroke(Path.polyline([(20, 6), (26, 14), (23, 22), (30, 28)]), (1.2, 0.2), t, "deep")
        if variant == 2:
            mt = soft("#6f9a5a", 0.6)
            for k in range(6):
                p.fill(Path.circle(8 + k * 4, 62 - (k % 3) * 2, 2.6), mt, "base", alpha=0.85)
    elif kind == "plaster":
        _patches(tl, t, seed, variant, 2, 2, (12, 20), 0.25)
        if variant == 1:
            p.stroke(Path.polyline([(40, 8), (36, 18), (41, 26), (38, 34)]), (1.0, 0.0), t, "deep-shadow")
        if variant == 2:
            # a patch where the plaster's come away, and the bricks under it
            bt = soft("#c98a6e", 0.6)
            patch = Path.smooth([(10, 40), (16, 30), (30, 28), (38, 34), (36, 46), (24, 52), (12, 50)], closed=True)
            pm = p.fill(patch, bt, "shadow")
            for r in range(4):
                for k in range(4):
                    x = 8 + k * 9 + (4.5 if r % 2 else 0)
                    b_ = p.fill(Path.rect(x, 28 + r * 6, 8, 5, r=0.8), bt, "base", clip=pm)
                    p.tone(b_ - b_.shift(0, 1), bt, "light", clip=pm, alpha=0.7)
            p.edge_mask(pm, t, 1.0, "shadow")
    elif kind == "boards":
        for c in range(4):
            x = c * 16

            def board(dx, dy, x=x, c=c):
                m = p.fill(Path.rect(x + 0.6 + dx, -T + dy, 14.8, 3 * T), t, "base" if c % 2 else "base-light")
                p.tone(m - m.shift(1.2, 0), t, "light", alpha=0.6)
                p.tone(m - m.shift(-1.2, 0), t, "shadow", alpha=0.7)
            tl.wrap(board)
    elif kind == "rock":
        for r in range(4):
            y = r * 16 + 4 * hash01(r, 0, seed)

            def stratum(dx, dy, y=y, r=r):
                pts = [(x + dx, y + dy + 3 * math.sin(x / 64.0 * 2 * math.pi * (1 + r % 2) + r)) for x in range(-4, 69, 8)]
                line = Path.smooth(pts)
                p.stroke(line, 2.4, t, "shadow", alpha=0.8)
                p.stroke(line.translate(0, -2), 1.2, t, "light", alpha=0.7)
            tl.wrap(stratum)
        for i in range(3):
            x = 10 + 44 * hash01(variant, i, seed + 1)
            y = 10 + 44 * hash01(variant, i, seed + 2)
            p.stroke(Path.polyline([(x, y), (x + 4, y + 7), (x + 2, y + 12)]), (1.2, 0.2), t, "deep", alpha=0.8)
    if extra:
        extra(p, variant, seed)
    return tl.image()


def wall_band(variant, frame=0, seed=201, colour="#5e4a40", kind="top", trim=None):
    """The horizontal bands of a wall seen from the front: its top (a dark cap or a ceiling beam),
    its base (a plinth or a skirting with a lit top edge) or the keep's high cornice (a moulding
    with a gold line); they run on across tiles."""
    tl = Tile()
    t = soft(colour, 0.55)
    tl.ground(t["base"])
    p = tl.p

    def band(y, h, which, dx, dy):
        p.fill(Path.rect(-T + dx, y + dy, 3 * T, h), t, which)

    if kind == "top":
        tl.wrap(lambda dx, dy: band(44, 20, "terminator", dx, dy))
        tl.wrap(lambda dx, dy: band(44, 2.2, "light", dx, dy))
        tl.wrap(lambda dx, dy: band(60, 4, "shadow", dx, dy))
        for k in range(3):
            y = 8 + k * 12
            tl.wrap(lambda dx, dy, y=y: p.stroke(Path.smooth([(-T + dx, y + dy), (0 + dx, y + 1 + dy), (64 + dx, y - 1 + dy), (2 * T + dx, y + dy)]),
                                                 0.9, t, "shadow", alpha=0.6))
    elif kind == "base":
        tl.wrap(lambda dx, dy: band(0, 6, "shadow", dx, dy))
        tl.wrap(lambda dx, dy: band(30, 34, "terminator", dx, dy))
        tl.wrap(lambda dx, dy: band(30, 3, "light", dx, dy))
        for x in (0, 32):
            p.stroke(Path.polyline([(x, 34), (x, 64)]), 1.0, t, "shadow", alpha=0.8)
    elif kind == "cornice":
        tl.wrap(lambda dx, dy: band(20, 12, "light", dx, dy))
        tl.wrap(lambda dx, dy: band(32, 8, "shadow", dx, dy))
        tl.wrap(lambda dx, dy: band(46, 18, "terminator", dx, dy))
        if trim:
            g = soft(trim, 0.6)
            tl.wrap(lambda dx, dy: p.fill(Path.rect(-T + dx, 41 + dy, 3 * T, 2.4), g, "base"))
            for x in (16, 48):
                p.fill(Path.polygon([(x, 49), (x + 5, 55), (x, 61), (x - 5, 55)]), g, "base")
    return tl.image()


def wall_feature(variant, frame=0, seed=211, colour="#9a96a8", kind="niche", accent=None, under=None):
    """One tile of wall with something in it: an arched niche with a candle, a carved relief, a sun
    crest in gold, a window (glass and a sill) or its arched head, roots hanging through, a black
    crystal growing out of the stone. Drawn over the set's own wall face (under)."""
    img = under(variant, 0) if under else None
    tl = Tile()
    p = tl.p
    t = soft(colour, 0.55)
    if img is not None:
        pass
    if kind == "niche":
        op = Path().move(18, 60).line(18, 26).arc(32, 26, 14, 180, 360).line(46, 60).close()
        sur = Path().move(13, 62).line(13, 26).arc(32, 26, 19, 180, 360).line(51, 62).close()
        p.fill(sur, t, "base-light")
        p.edge(sur, t, 1.0, "shadow")
        m = p.fill(op, t, "deep")
        p.tone(m - m.shift(3, 3), t, "line", alpha=0.6)
        ct = tones("cloth.cream")
        p.fill(Path.rect(29, 44, 6, 14, r=1), ct, "base")
        p.fill(Path.ellipse(32, 40, 2.2, 4.2), tones("#ffcf6a"), "light")
        p.glow(32, 40, 14, "#ffc070", 0.3)
    elif kind == "carving":
        sur = Path.rect(10, 10, 44, 44, r=3)
        m = p.fill(sur, t, "terminator")
        p.edge(sur, t, 1.0, "shadow")
        skull = Path.circle(32, 28, 10)
        sm = p.fill(skull, t, "base-light")
        p.tone(sm - sm.shift(-1.2, -1.2), t, "shadow")
        p.fill(Path.circle(28, 28, 2.6), t, "deep")
        p.fill(Path.circle(36, 28, 2.6), t, "deep")
        p.fill(Path.rect(27, 37, 10, 6, r=1), t, "base-light")
        p.stroke(Path.polyline([(16, 48), (48, 48)]), 2.0, t, "shadow")
        del m
    elif kind in ("crest", "sun"):
        g = soft(accent or "#e9b54c", 0.6)
        for k in range(8):
            a = math.radians(k * 45)
            p.fill(Path.polygon([(32 + math.cos(a) * 22, 32 + math.sin(a) * 22), (32 + math.cos(a + 0.4) * 10, 32 + math.sin(a + 0.4) * 10),
                                 (32 + math.cos(a - 0.4) * 10, 32 + math.sin(a - 0.4) * 10)]), g, "base")
        m = p.fill(Path.circle(32, 32, 11), g, "light")
        p.tone(m - m.shift(-1.2, -1.2), g, "base")
        p.edge(Path.circle(32, 32, 11), g, 1.0, "shadow")
    elif kind in ("window", "window-high"):
        gt = tones("#7fb4e6")
        ft = soft(accent or "#d8cbb6", 0.6)
        if kind == "window-high":
            op = Path().move(18, 66).line(18, 30).arc(32, 30, 14, 180, 360).line(46, 66).close()
            sur = Path().move(12, 66).line(12, 30).arc(32, 30, 20, 180, 360).line(52, 66).close()
        else:
            op = Path.rect(18, -2, 28, 54)
            sur = Path.rect(12, -2, 40, 60)
        p.fill(sur, ft, "base-light")
        p.edge(sur, ft, 1.0, "shadow")
        gm = p.fill(op, gt, "base")
        p.tone(gm - gm.shift(3, 3), gt, "shadow")
        p.fill(Path.polygon([(22, 50), (32, 20), (38, 20), (28, 50)]), gt, "light", clip=gm, alpha=0.7)
        p.stroke(Path.polyline([(32, -2), (32, 66)]), 2.0, ft, "base", clip=gm)
        if kind == "window":
            sill = Path.rect(9, 52, 46, 7, r=1.5)
            p.fill(sill, ft, "light")
            p.edge(sill, ft, 1.0, "shadow")
    elif kind == "roots":
        rt = soft(accent or "#6e4c3a", 0.6)
        for i in range(4):
            x = 8 + 16 * i + 4 * hash01(i, variant, seed)
            path = Path.smooth([(x, -2), (x + 3, 18), (x - 2, 34), (x + 2, 46 + 10 * hash01(i, 1, seed))])
            p.stroke(path, (3.6, 0.6), rt, "base")
            p.stroke(path.translate(-0.8, 0), (1.2, 0.0), rt, "light")
    elif kind == "crystal":
        ct = tones(accent or "#3a2a4a")
        glow = accent or "#9a6ad8"
        p.glow(32, 36, 34, "#9a6ad8", 0.35)
        shard = Path.polygon([(22, 60), (28, 14), (36, 6), (42, 26), (44, 60)])
        m = p.fill(shard, ct, "base")
        p.tone(p.where(m, lambda xx, yy: xx > 34), ct, "shadow")
        p.fill(Path.polygon([(29, 50), (31, 18), (35, 12), (33, 50)]), tones("#b48aff"), "base", alpha=0.7)
        p.edge(shard, ct, 1.2)
        del glow
    img2 = tl.image()
    if img is None:
        return img2
    a = img2[..., 3:4].astype(np.float64) / 255.0
    out = img.copy()
    out[..., :3] = (img2[..., :3] * a + img[..., :3] * (1 - a)).astype(np.uint8)
    out[..., 3] = np.maximum(img[..., 3], img2[..., 3])
    return out


def over_ground(top, under):
    """A tile painted over another (a feature on a wall face, a plate in a floor)."""
    def fn(variant, frame=0):
        base = under(variant, frame)
        img = top(variant, frame)
        a = img[..., 3:4].astype(np.float64) / 255.0
        out = base.copy()
        out[..., :3] = np.rint(img[..., :3] * a + base[..., :3] * (1 - a)).astype(np.uint8)
        out[..., 3] = np.maximum(base[..., 3], img[..., 3])
        return out
    return fn


# --- details: drawn over the ground, kept off their tile's edges (so a patch has a ragged edge) --- #

def detail(kind, colour=None, seed=301, wrap_x=False):
    """A detail painter: what's lying about on the ground (a tuft, ferns, flowers, pebbles, roots,
    litter, clover, reeds, lily pads, stepping stones, petals, black veins, dead grass, toadstools,
    tar, brambles, rubble, gravel, shards, fungus, tools, bones, cobwebs, coins, debris, darts),
    on a clear tile. Ruts and crops run on across (wrap_x)."""

    def fn(variant, frame=0):
        tl = Tile()
        p = tl.p
        sd = seed + variant * 37
        c = colour
        if kind == "tuft":
            t = soft(c or "#5f9e4c", 0.6)
            for i in range(3):
                _tuft(p, 16 + 32 * hash01(i, 1, sd), 22 + 30 * hash01(i, 2, sd), 9 + 4 * hash01(i, 3, sd), t, sd + i, blades=6)
        elif kind == "tuft-dead":
            t = soft(c or "#9a8a5e", 0.6)
            for i in range(3):
                _tuft(p, 16 + 32 * hash01(i, 1, sd), 22 + 30 * hash01(i, 2, sd), 9 + 4 * hash01(i, 3, sd), t, sd + i, blades=6, dead=True)
        elif kind == "fern":
            t = soft(c or "#4f9a5a", 0.6)
            for i in range(2):
                cx, cy = 20 + 24 * i + 4 * hash01(i, 1, sd), 46 + 6 * hash01(i, 2, sd)
                for k in range(5):
                    a = math.radians(-150 + k * 30)
                    tip = (cx + math.cos(a) * 18, cy + math.sin(a) * 18)
                    frond = Path.smooth([(cx, cy), ((cx + tip[0]) / 2, (cy + tip[1]) / 2 - 3), tip])
                    p.stroke(frond, (1.4, 0.4), t, "shadow")
                    for j in range(1, 5):
                        q, d = frond.at(j / 5.0)
                        for s_ in (-1, 1):
                            p.stroke(Path.polyline([q, (q[0] - d[1] * 4 * s_ + d[0] * 2, q[1] + d[0] * 4 * s_ + d[1] * 2)]), (2.0, 0.6), t, "base")
        elif kind in ("flowers", "petals", "clover"):
            cols = {"flowers": ("flower.pink", "flower.white", "flower.yellow", "flower.violet", "flower.blue"),
                    "petals": ("blossom", "blossom.white"), "clover": ("#5fa04e",)}[kind]
            if kind == "flowers" and c:
                cols = c
            n = {"flowers": 6, "petals": 9, "clover": 7}[kind]
            for i in range(n):
                x = 10 + 44 * hash01(i, 1, sd)
                y = 10 + 44 * hash01(i, 2, sd)
                ft = tones(cols[(i + variant) % len(cols)])
                if kind == "petals":
                    m = p.fill(Path.ellipse(x, y, 2.6, 1.4, rot=360 * hash01(i, 3, sd)), ft, "base")
                    p.tone(m - m.shift(-0.5, -0.5), ft, "shadow")
                elif kind == "clover":
                    lt = soft(ft["base"], 0.6)
                    for k in range(3):
                        a = math.radians(k * 120 - 90)
                        m = p.fill(Path.circle(x + math.cos(a) * 2.2, y + math.sin(a) * 2.2, 2.3), lt, "base")
                        p.tone(m - m.shift(-0.6, -0.6), lt, "shadow")
                    if i % 3 == 0:
                        wt = tones("flower.white")
                        p.fill(Path.circle(x + 1, y - 4, 1.8), wt, "base")
                else:
                    stem = soft("#4f8a4a", 0.6)
                    p.stroke(Path.polyline([(x, y + 6), (x, y)]), 1.0, stem, "shadow")
                    for k in range(5):
                        a = math.radians(k * 72 - 90)
                        p.fill(Path.circle(x + math.cos(a) * 1.8, y + math.sin(a) * 1.8, 1.5), ft, "base")
                    p.fill(Path.circle(x, y, 0.9), tones("flower.yellow" if cols[0] != "flower.yellow" else "wood"), "shadow")
        elif kind in ("pebbles", "gravel", "rubble", "debris", "shards"):
            n = {"pebbles": 6, "gravel": 14, "rubble": 5, "debris": 6, "shards": 5}[kind]
            for i in range(n):
                x = 10 + 44 * hash01(i, 1, sd)
                y = 12 + 42 * hash01(i, 2, sd)
                r = {"pebbles": 2.6, "gravel": 1.5, "rubble": 8.5, "debris": 3.5, "shards": 4.0}[kind] * (0.7 + 0.6 * hash01(i, 3, sd))
                if kind == "shards":
                    st = tones(c or "#3a2a4a")
                    sh = Path.polygon([(x - r * 0.5, y + r), (x, y - r * 1.4), (x + r * 0.6, y + r)])
                    m = p.fill(sh, st, "base")
                    p.tone(p.where(m, lambda xx, yy, x=x: xx > x), st, "shadow")
                    p.fill(Path.polygon([(x - r * 0.15, y + r * 0.6), (x, y - r * 0.9), (x + r * 0.1, y + r * 0.6)]), tones("#b48aff"), "base", alpha=0.7)
                    p.edge(sh, st, 0.8)
                elif kind in ("rubble", "debris"):
                    st = soft(c or "#9a94a2", 0.6)
                    pts = [(x + math.cos(a) * r * (0.7 + 0.4 * hash01(i, k, sd + 5)), y + math.sin(a) * r * 0.75) for k, a in enumerate(np.linspace(0, 2 * math.pi, 7)[:-1])]
                    m = p.fill(Path.polygon(pts), st, "base")
                    p.tone(m - m.shift(-1.0, -1.2), st, "shadow")
                    p.tone(m - m.shift(0.8, 1.0), st, "light", alpha=0.8)
                    p.edge(Path.polygon(pts), st, 0.8, "deep")
                else:
                    _pebble(p, x, y, r, soft(c or "#a39a8c", 0.6), sd + i)
        elif kind == "roots":
            rt = soft(c or "#7a5440", 0.6)
            for i in range(2):
                y = 18 + 26 * i + 6 * hash01(i, 1, sd)
                path = Path.smooth([(8, y), (22, y + 6), (38, y - 4), (56, y + 3)])
                p.stroke(path, (4.0, 1.2), rt, "base")
                p.stroke(path.translate(-0.6, -1.0), (1.4, 0.3), rt, "light")
                p.stroke(Path.smooth([(30, y), (34, y + 8), (30, y + 12)]), (2.0, 0.4), rt, "base")
        elif kind == "ruts":
            t = soft(c or "#8a6a4a", 0.6)
            for y in (20.0, 42.0):
                tl.wrap(lambda dx, dy, y=y: p.fill(Path.rect(-T + dx, y + dy, 3 * T, 5, r=2), t, "terminator", alpha=0.75))
                tl.wrap(lambda dx, dy, y=y: p.fill(Path.rect(-T + dx, y + 4.5 + dy, 3 * T, 1.6), t, "light", alpha=0.6))
        elif kind == "crops":
            t = soft(c or "#9fbf4a", 0.6)
            for r in range(4):
                y = r * 16 + 12

                def row(dx, dy, y=y, r=r):
                    for k in range(6):
                        x = k * 10.67 + (5 if r % 2 else 0)
                        _tuft(p, x + dx, y + dy, 8, t, seed + k, blades=3, spread=18)
                        p.fill(Path.ellipse(x + dx, y - 8 + dy, 1.5, 2.8), tones("#d6c26e"), "base", alpha=0.75)
                tl.wrap(row)
        elif kind == "litter":
            lt = ("#c9853f", "#a85a34", "#8c9a46")
            for i in range(5):
                x = 12 + 40 * hash01(i, 1, sd)
                y = 12 + 40 * hash01(i, 2, sd)
                _leaf(p, x, y, 5, 360 * hash01(i, 3, sd), soft(lt[i % 3], 0.6))
            tw = soft("#6e5240", 0.6)
            for i in range(2):
                x = 14 + 34 * hash01(i, 4, sd)
                y = 16 + 30 * hash01(i, 5, sd)
                p.stroke(Path.polyline([(x, y), (x + 12, y + 4), (x + 16, y + 2)]), (1.8, 0.6), tw, "base")
        elif kind == "reeds":
            t = soft(c or "#6f9a4a", 0.6)
            for i in range(6):
                x = 14 + 36 * hash01(i, 1, sd)
                y = 54
                h = 26 + 16 * hash01(i, 2, sd)
                lean = (hash01(i, 3, sd) - 0.5) * 8
                p.stroke(Path.smooth([(x, y), (x + lean * 0.5, y - h * 0.5), (x + lean, y - h)]), (2.4, 0.4), t, "base")
                if i % 2 == 0:
                    p.fill(Path.ellipse(x + lean * 0.8, y - h * 0.8, 2.0, 5.0), tones("#7a5442"), "base")
        elif kind == "lilies":
            t = soft(c or "#5aa05a", 0.6)
            for i in range(3):
                x = 14 + 36 * hash01(i, 1, sd)
                y = 16 + 32 * hash01(i, 2, sd)
                pad = Path().move(x, y).arc(x, y, 8, 20, 340, 5).close()
                m = p.fill(pad, t, "base")
                p.tone(m - m.shift(-1, -1), t, "shadow")
                p.edge(pad, t, 0.8)
                if i == 0:
                    ft = tones("blossom.white")
                    for k in range(5):
                        a = math.radians(k * 72 - 90)
                        p.fill(Path.ellipse(x + math.cos(a) * 2, y - 2 + math.sin(a) * 2, 2.2, 1.4, rot=k * 72), ft, "base")
                    p.fill(Path.circle(x, y - 2, 1.2), tones("flower.yellow"), "base")
        elif kind == "stepping":
            st = soft(c or "#b4aca0", 0.6)
            for i, (x, y) in enumerate(((18, 24), (42, 40))):
                m = p.fill(Path.ellipse(x, y, 11, 6.5), st, "base")
                p.tone(m - m.shift(-1.2, -1.6), st, "shadow")
                p.tone(m - m.shift(1.0, 1.2), st, "light")
                p.edge(Path.ellipse(x, y, 11, 6.5), st, 1.0, "deep")
        elif kind == "veins":
            vt = tones(c or "#2a1f33")
            for i in range(2):
                x0, y0 = 10 + 20 * hash01(i, 1, sd), 14 + 30 * hash01(i, 2, sd)
                path = Path.smooth([(x0, y0), (x0 + 12, y0 + 6), (x0 + 22, y0 + 2), (x0 + 34, y0 + 10)])
                p.stroke(path, 5.0, tones("#8a5ac8"), "base", alpha=0.25)
                p.stroke(path, (2.2, 0.6), vt, "base")
                p.stroke(Path.smooth([(x0 + 12, y0 + 6), (x0 + 16, y0 + 14), (x0 + 14, y0 + 20)]), (1.4, 0.3), vt, "base")
        elif kind == "toadstools":
            for i in range(3):
                x = 16 + 32 * hash01(i, 1, sd)
                y = 34 + 18 * hash01(i, 2, sd)
                s = 4 + 3 * hash01(i, 3, sd)
                st = tones("#efe4d4")
                p.fill(Path.rect(x - s * 0.3, y - s * 1.2, s * 0.6, s * 1.2, r=1), st, "base")
                cap = Path().arc(x, y - s * 1.1, s, 180, 360).close()
                ct = tones(c or "#8a5ac8")
                m = p.fill(cap, ct, "base")
                p.tone(p.where(m, lambda xx, yy, x=x: xx > x + 1), ct, "shadow")
                p.fill(Path.circle(x - s * 0.35, y - s * 1.5, s * 0.18), st, "light")
                p.edge(cap, ct, 0.8)
        elif kind in ("tar-spatter", "tar-slick"):
            tt = tones(c or "#2a2230")
            n = 4 if kind == "tar-spatter" else 1
            for i in range(n):
                x = 16 + 32 * hash01(i, 1, sd) if n > 1 else 32
                y = 18 + 28 * hash01(i, 2, sd) if n > 1 else 34
                r = 4 + 3 * hash01(i, 3, sd) if n > 1 else 20
                m = p.fill(Path.ellipse(x, y, r, r * 0.55), tt, "base")
                p.tone(m - m.shift(-1, -1), tt, "deep")
                sx = x - r * 0.3 + (frame * r * 0.15 if kind == "tar-slick" else 0)
                p.fill(Path.ellipse(sx, y - r * 0.18, r * 0.35, r * 0.1), tones("#8a6ad8"), "base", clip=m, alpha=0.6)
                p.fill(Path.ellipse(sx + r * 0.3, y + r * 0.12, r * 0.25, r * 0.07), tones("#5ac8c0"), "base", clip=m, alpha=0.4)
        elif kind == "brambles":
            tt = soft(c or "#5a3a3e", 0.6)
            for i in range(3):
                x = 12 + 40 * hash01(i, 1, sd)
                y = 16 + 32 * hash01(i, 2, sd)
                path = Path.smooth([(x - 10, y + 4), (x - 2, y - 4), (x + 6, y + 2), (x + 12, y - 3)])
                p.stroke(path, 1.8, tt, "base")
                for k in range(4):
                    q, d = path.at((k + 0.5) / 4)
                    p.stroke(Path.polyline([q, (q[0] - d[1] * 3.5 + d[0], q[1] + d[0] * 3.5 + d[1])]), (1.4, 0.0), tt, "base")
                p.fill(Path.circle(x + 3, y - 2, 1.8), tones("#8a3a5a"), "base")
        elif kind == "fungus":
            for i in range(4):
                x = 12 + 40 * hash01(i, 1, sd)
                y = 20 + 30 * hash01(i, 2, sd)
                ct = tones(c or "#c9b48a")
                cap = Path().arc(x, y, 4 + 2 * hash01(i, 3, sd), 180, 360).close()
                p.fill(cap, ct, "base")
                p.edge(cap, ct, 0.8)
                p.glow(x, y - 2, 8, "#c8f0a0", 0.15)
        elif kind == "tools":
            wt = soft("#9a6a44", 0.6)
            it = tones("#8d93a8")
            p.stroke(Path.polyline([(14, 50), (48, 18)]), 3.2, wt, "base")
            pick = Path.smooth([(38, 12), (48, 16), (56, 26)])
            p.stroke(pick, (4.0, 1.0), it, "base")
            p.stroke(Path.polyline([(20, 20), (46, 50)]), 3.0, wt, "base")
            p.fill(Path.polygon([(42, 46), (52, 50), (50, 58), (40, 54)]), it, "base")
        elif kind == "bones":
            bt = tones(c or "#ece2cc")
            for i in range(2):
                x = 16 + 30 * hash01(i, 1, sd)
                y = 20 + 26 * hash01(i, 2, sd)
                a = math.radians(360 * hash01(i, 3, sd))
                dx, dy = math.cos(a) * 9, math.sin(a) * 9
                p.stroke(Path.polyline([(x - dx, y - dy), (x + dx, y + dy)]), 3.0, bt, "base")
                for s_ in (-1, 1):
                    for k in (-1, 1):
                        p.fill(Path.circle(x + s_ * dx + k * dy * 0.25, y + s_ * dy - k * dx * 0.25, 2.2), bt, "base")
            if variant == 0:
                sk = Path.circle(40, 44, 7)
                m = p.fill(sk, bt, "base")
                p.tone(m - m.shift(-1, -1), bt, "shadow")
                p.fill(Path.circle(37.5, 44, 1.8), bt, "deep")
                p.fill(Path.circle(42.5, 44, 1.8), bt, "deep")
                p.edge(sk, bt, 0.8)
        elif kind in ("cobweb", "cobweb-ne"):
            wt = tones("#e8ecf4")
            ox = 0 if kind == "cobweb" else 64
            sx = 1 if kind == "cobweb" else -1
            for k in range(5):
                a = math.radians(5 + k * 20)
                p.stroke(Path.polyline([(ox, 0), (ox + sx * math.cos(a) * 56, math.sin(a) * 56)]), 0.8, wt, "base", alpha=0.75)
            for r in (14, 26, 38, 50):
                pts = [(ox + sx * math.cos(math.radians(5 + k * 20)) * r, math.sin(math.radians(5 + k * 20)) * r) for k in range(5)]
                p.stroke(Path.smooth(pts), 0.7, wt, "base", alpha=0.6)
        elif kind == "coins":
            g = tones("#e9b54c")
            n = 4 + 3 * variant
            for i in range(n):
                x = 14 + 36 * hash01(i, 1, sd)
                y = 18 + 32 * hash01(i, 2, sd)
                m = p.fill(Path.ellipse(x, y, 3.6, 2.2), g, "base")
                p.tone(m - m.shift(0, 1.0), g, "light")
                p.edge(Path.ellipse(x, y, 3.6, 2.2), g, 0.7, "deep-shadow")
        elif kind == "darts":
            it = tones("#6a6478")
            for i in range(3):
                x = 14 + 36 * hash01(i, 1, sd)
                y = 20 + 28 * hash01(i, 2, sd)
                p.stroke(Path.polyline([(x, y), (x + 10, y - 4)]), 1.6, it, "base")
                p.fill(Path.polygon([(x + 10, y - 6), (x + 14, y - 5), (x + 10, y - 2)]), it, "light")
                p.fill(Path.polygon([(x - 3, y - 2), (x, y), (x - 3, y + 2)]), tones("cloth.red"), "base")
        elif kind == "moss":
            t = soft(c or "#5f9a5a", 0.6)
            for i in range(2):
                x = 20 + 24 * hash01(i, 1, sd)
                y = 22 + 20 * hash01(i, 2, sd)
                cushion = p.nothing()
                for k in range(6):
                    a = 6.283 * k / 6 + hash01(i, k, sd + 3)
                    cushion = cushion | p.shape(Path.ellipse(x + math.cos(a) * 7, y + math.sin(a) * 3.5, 6 + 2 * hash01(k, i, sd), 3.6))
                p.tone(cushion, t, "base")
                p.tone(cushion - cushion.shift(-1, -1.4), t, "shadow")
                p.tone(cushion - cushion.shift(0.8, 1.0), t, "light", alpha=0.6)
                p.edge_mask(cushion, t, 0.8, "shadow")
        return tl.image()

    return fn


def plate(variant, frame=0, seed=311, colour="#c8bca8"):
    """A pressure plate: a square stone plate let into the floor, its gap dark, a worn rune on it."""
    tl = Tile()
    t = soft(colour, 0.5)
    p = tl.p
    s = 40 if variant == 0 else 34
    x0 = 32 - s / 2
    gap = Path.rect(x0 - 3, 30 - s / 2 - 3, s + 6, s + 6, r=2)
    p.fill(gap, t, "deep")
    m = _stone(p, x0, 30 - s / 2, s, s, t, r=2.0, seed=seed + variant)
    rt = soft("#e9b54c", 0.6)
    p.stroke(Path.circle(32, 30, s * 0.22), 1.2, rt, "shadow", clip=m, alpha=0.7)
    return tl.image()


def sun_crest(variant, frame=0, seed=321, colour="#e2d8c4"):
    """The vault's sun crest let into the floor: a block of 2 x 2 tiles, a gold sun in a ring of
    blue on pale stone."""
    b = Big(2)
    t = soft(colour, 0.5)
    b.ground(t["base"])
    p = b.p
    g = soft("#e9b54c", 0.6)
    bl = soft("#4f7ad0", 0.6)
    p.fill(Path.circle(64, 64, 58), bl, "base")
    p.stroke(Path.circle(64, 64, 58), 2.4, g, "base")
    for k in range(12):
        a = math.radians(k * 30)
        p.fill(Path.polygon([(64 + math.cos(a) * 52, 64 + math.sin(a) * 52), (64 + math.cos(a + 0.18) * 26, 64 + math.sin(a + 0.18) * 26),
                             (64 + math.cos(a - 0.18) * 26, 64 + math.sin(a - 0.18) * 26)]), g, "base")
    m = p.fill(Path.circle(64, 64, 24), g, "light")
    p.tone(m - m.shift(-2, -2), g, "base")
    p.edge(Path.circle(64, 64, 24), g, 1.2, "shadow")
    return b.cells()


# --- edges for the other sets --------------------------------------------------------------- #

def fringe_edge(colour, kind="grass", depth=12):
    """An edge where something soft meets other ground: grass reaching over it (grass), a verge of
    grass along a path (verge), a bank of earth with a line of foam along a stream (bank), or the
    lit lip of a rock or a pit (lip, pit)."""
    def fn(side, variant=0, seed=0):
        tl = Tile()
        p = tl.p
        t = soft(colour, 0.6)
        horiz = side in (0, 2)

        def along(u):
            """A point along the side at u (0..1), and the inward direction."""
            if side == 0:
                return (u * 64, 0.0), (0, 1)
            if side == 2:
                return (u * 64, 64.0), (0, -1)
            if side == 3:
                return (0.0, u * 64), (1, 0)
            return (64.0, u * 64), (-1, 0)

        if kind in ("grass", "verge"):
            band = []
            n = 9
            for k in range(n + 1):
                (x, y), (ix, iy) = along(k / n)
                d = depth * (0.5 + 0.5 * hash01(k, side, 401)) if 0 < k < n else depth * 0.55
                band.append((x + ix * d, y + iy * d))
            (x0, y0), _ = along(0)
            (x1, y1), _ = along(1)
            pts = [(x0, y0)] + band + [(x1, y1)]
            m = p.fill(Path.smooth(pts, closed=True), t, "base")
            p.tone(m - m.shift(-0.8, -1.2), t, "shadow", alpha=0.8)
            for k in range(1, n):
                (x, y), (ix, iy) = along(k / n)
                _tuft(p, x + ix * depth * 0.9, y + iy * depth * 0.9 + (4 if side == 0 else 0), 6, t, 401 + k + side * 13, blades=4)
        elif kind == "bank":
            et = soft("#8a6a4e", 0.6)
            if horiz:
                y0 = 0 if side == 0 else 64 - depth
                p.fill(Path.rect(-T, y0, 3 * T, depth), et, "base")
                p.fill(Path.rect(-T, y0 + (depth - 3 if side == 0 else 0), 3 * T, 3), et, "shadow")
                fy = y0 + depth + 1 if side == 0 else y0 - 3
                for k in range(8):
                    p.fill(Path.ellipse(k * 8 + 4, fy, 4.4, 1.4), tones("#f2f6fa"), "base", alpha=0.75)
            else:
                x0 = 0 if side == 3 else 64 - depth
                p.fill(Path.rect(x0, -T, depth, 3 * T), et, "base")
                fx = x0 + depth + 1 if side == 3 else x0 - 2
                for k in range(8):
                    p.fill(Path.ellipse(fx, k * 8 + 4, 1.4, 4.4), tones("#f2f6fa"), "base", alpha=0.75)
        elif kind in ("lip", "pit"):
            if horiz:
                y0 = 0 if side == 0 else 64 - depth
                m = p.fill(Path.rect(-T, y0, 3 * T, depth), t, "light" if (side == 0) == (kind == "lip") else "deep")
                p.fill(Path.rect(-T, y0 + (depth - 2.5 if side == 0 else 0), 3 * T, 2.5), t, "deep" if kind == "lip" else "base-light")
                del m
            else:
                x0 = 0 if side == 3 else 64 - depth
                p.fill(Path.rect(x0, -T, depth, 3 * T), t, "light" if side == 3 else "shadow")
                p.fill(Path.rect(x0 + (depth - 2 if side == 3 else 0), -T, 2, 3 * T), t, "deep")
        elif kind == "broken":
            # the broken ends of slabs round earth: a ragged rim of stone over it
            st = soft(colour, 0.5)
            for k in range(4):
                (x, y), (ix, iy) = along((k + 0.5) / 4)
                d = depth * (0.6 + 0.4 * hash01(k, side, 403))
                w = 15
                if horiz:
                    shard = Path.polygon([(x - w / 2, y), (x + w / 2, y), (x + w / 2 - 3, y + iy * d), (x - w / 2 + 2, y + iy * d * 0.8)])
                else:
                    shard = Path.polygon([(x, y - w / 2), (x, y + w / 2), (x + ix * d, y + w / 2 - 3), (x + ix * d * 0.8, y - w / 2 + 2)])
                m = p.fill(shard, st, "base")
                p.tone(m - m.shift(-1, -1.2), st, "shadow")
                p.edge(shard, st, 0.8, "deep")
        return tl.image()
    return fn


def rail(variant, frame=0, seed=331, colour="#8a5a3c", south=False):
    """A ship's rail along the deck's edge: a cap rail on turned balusters (the far rail) or the near
    rail's solid bulwark with a lit cap; they run on across."""
    tl = Tile()
    t = soft(colour, 0.6)
    p = tl.p
    if south:
        tl.wrap(lambda dx, dy: p.fill(Path.rect(-T + dx, 10 + dy, 3 * T, 54), t, "terminator"))
        for x in (0, 32):
            p.stroke(Path.polyline([(x, 12), (x, 64)]), 1.0, t, "shadow")
        tl.wrap(lambda dx, dy: p.fill(Path.rect(-T + dx, 4 + dy, 3 * T, 8, r=2), t, "light"))
    else:
        tl.wrap(lambda dx, dy: p.fill(Path.rect(-T + dx, 40 + dy, 3 * T, 6), t, "shadow"))
        for k in range(4):
            x = 8 + k * 16
            bal = Path.smooth([(x - 2.5, 60), (x - 3.5, 52), (x - 2, 44), (x - 3, 36), (x - 2.5, 28),
                               (x + 2.5, 28), (x + 3, 36), (x + 2, 44), (x + 3.5, 52), (x + 2.5, 60)], closed=True)
            m = p.fill(bal, t, "base")
            p.tone(p.where(m, lambda xx, yy, x=x: xx > x + 0.6), t, "shadow")
            p.edge(bal, t, 0.8)
        tl.wrap(lambda dx, dy: p.fill(Path.rect(-T + dx, 20 + dy, 3 * T, 9, r=2), t, "base"))
        tl.wrap(lambda dx, dy: p.fill(Path.rect(-T + dx, 20 + dy, 3 * T, 2.5), t, "light"))
    return tl.image()


def hull(variant, frame=0, seed=341, colour="#6a4632"):
    """The ship's side: dark strakes running along, a tarred seam between each, a wale of lighter wood."""
    tl = Tile()
    t = soft(colour, 0.6)
    tl.ground(t["base"])
    p = tl.p
    for r in range(4):
        y = r * 16
        tl.wrap(lambda dx, dy, y=y: p.fill(Path.rect(-T + dx, y + 14 + dy, 3 * T, 2), t, "deep"))
        tl.wrap(lambda dx, dy, y=y: p.fill(Path.rect(-T + dx, y + dy, 3 * T, 2), t, "light", alpha=0.6))
    tl.wrap(lambda dx, dy: p.fill(Path.rect(-T + dx, 26 + dy, 3 * T, 8), soft("#9a6a44", 0.6), "base"))
    for x in (12, 44):
        p.fill(Path.circle(x + 8 * variant, 8, 1.2), t, "deep")
    return tl.image()


def hatch(variant, frame=0, seed=351):
    """The hatch: a block of 2 x 2 tiles, a grating of crossed battens in a raised coaming."""
    b = Big(2)
    t = soft("#b08a62", 0.6)
    b.ground(soft("#b89a74", 0.6)["base"])
    p = b.p
    co = Path.rect(10, 10, 108, 108, r=3)
    p.fill(co, t, "terminator")
    p.edge(co, t, 1.2)
    p.fill(Path.rect(18, 18, 92, 92), soft("#3a2c24", 0.6), "base")
    for k in range(6):
        p.fill(Path.rect(22 + k * 15, 18, 6, 92), t, "base")
        p.fill(Path.rect(18, 22 + k * 15, 92, 6), t, "base-light")
    p.fill(Path.rect(10, 10, 108, 4), t, "light")
    return b.cells()


def ship_end(variant, frame=0, seed=361, stern=False):
    """The bow (or the stern): a block of 9 x 9 tiles, the deck's planks narrowing to the stem
    between curving rails, as the top-down maps drew it."""
    b = Big(9, wrap=False, ss=2)
    p = b.p
    s = 9 * T
    deck_t = soft("#b89a74", 0.6)
    rail_t = soft("#8a5a3c", 0.6)
    if stern:
        outline = Path().move(0, 40).line(s - 120, 40).quad(s - 10, 60, s - 20, s / 2).quad(s - 10, s - 60, s - 120, s - 40).line(0, s - 40).close()
    else:
        outline = Path().move(0, 40).quad(s * 0.6, 40, s - 20, s / 2).quad(s * 0.6, s - 40, 0, s - 40).close()
    m = p.fill(outline, deck_t, "base")
    for k in range(int(s / 16)):
        y = k * 16
        p.stroke(Path.polyline([(0, y), (s, y)]), 1.0, deck_t, "deep-shadow", clip=m)
        p.stroke(Path.polyline([(0, y + 1.6), (s, y + 1.6)]), 1.0, deck_t, "light", clip=m, alpha=0.5)
    p.stroke(outline, 12, rail_t, "base")
    p.stroke(outline.translate(-1.2, -1.6), 3, rail_t, "light")
    return b.cells()


def bowsprit(variant, frame=0, seed=371):
    """The bowsprit: a block of 3 x 3 tiles, the spar running out over the sea with its stays."""
    b = Big(3, wrap=False, ss=3)
    p = b.p
    t = soft("#9a6a44", 0.6)
    spar = Path.polygon([(0, 88), (190, 80), (190, 92), (0, 104)])
    m = p.fill(spar, t, "base")
    p.tone(p.below(m, 92), t, "shadow")
    p.edge(spar, t, 1.2)
    rope = tones("rope")
    p.stroke(Path.polyline([(0, 40), (186, 84)]), 1.2, rope, "base")
    p.stroke(Path.polyline([(0, 150), (186, 88)]), 1.2, rope, "base")
    return b.cells()


def lantern_tile(variant, frame=0, seed=381):
    """A lantern swinging on the rail: four frames of its swing, its glass lit."""
    tl = Tile()
    p = tl.p
    a = math.radians((-12, -4, 12, 4)[frame])
    px, py = 32, 4
    lx, ly = px + math.sin(a) * 26, py + math.cos(a) * 26
    p.stroke(Path.polyline([(px, py), (lx, ly)]), 1.2, tones("iron"), "base")
    from paint import lantern_body
    lantern_body(p, lx, ly, 1.0, True)
    return tl.image()


def _k(fn, **kw):
    """A painter with its parameters fixed: f(variant, frame)."""
    return lambda variant, frame=0: fn(variant, frame, **kw)


# The sets' own colours (the old sheets' looks, in the painted palette).
STONE_COOL = "#a3a2b4"       # the barrow's stone
STONE_CUT = "#b9ad9c"        # the quarry's
STONE_PALE = "#e4dac6"       # the vault's polished floor
MARBLE = "#ece7f1"           # the keep's
WOOD_FLOOR = "#a86f45"
DECK = "#b89a74"
EARTH = "#b08e64"
PATH_EARTH = "#c9aa7c"
SOIL = "#8f6a4a"
FOREST_GRASS = "#5f9e4c"
MEADOW_GRASS = "#74b45a"
MIRE_GRASS = "#5a9650"

_interior_wall = _k(wall_face, colour="#efe2cc", kind="plaster", seed=611)
_crypt_wall = _k(wall_face, colour="#9c98ac", kind="stone", seed=621)
_quarry_wall = _k(wall_face, colour="#a89c90", kind="rock", seed=631)
_keep_wall = _k(wall_face, colour="#e6dccb", kind="stone", seed=641)
_vault_wall = _k(wall_face, colour="#d9ccb4", kind="stone", seed=651)
_cliff = _k(wall_face, colour="#8f8a9e", kind="rock", seed=661)
_earth_face = _k(wall_face, colour="#8a6a4e", kind="rock", seed=671)

#: Who paints each tile: tileset -> tile name -> (the tile's painter, its edges' painter or None).
#: A painter is f(variant, frame) -> a 64 x 64 picture, or for a block tile a list of its block x
#: block frames, row by row; an edge painter is f(side) for side 0 north, 1 east, 2 south, 3 west.
PAINTERS = {
    "questeria": {
        "plaza": (slabs, None),
        "terrace": (terrace, None),
        "cobble": (cobble, None),
        "grass": (grass, kerb_edge),
        "sand": (sand, None),
        "water": (water, quay_edge),
        "planks": (planks, beam_edge),
    },
    "ship": {
        "sea": (_k(liquid, colour="#2f63a8", seed=701), None),
        "deck": (_k(planks, colour=DECK, seed=711), None),
        "rail-n": (_k(rail, south=False), None),
        "rail-s": (_k(rail, south=True), None),
        "hull": (hull, None),
        "hatch": (hatch, None),
        "bow": (_k(ship_end, stern=False), None),
        "stern": (_k(ship_end, stern=True), None),
        "bowsprit": (bowsprit, None),
        "lantern": (lantern_tile, None),
    },
    "interior": {
        "floor-wood": (_k(planks, colour=WOOD_FLOOR, seed=721), None),
        "floor-stone": (_k(slabs, colour="#d3c8b8", inlay=False, seed=731), None),
        "rug": (_k(carpet, colour="#b04a5a", pattern="#e9c46a", motif="ring", seed=741), border_edge("#2c4588", "#e9c46a")),
        "wall-top": (_k(wall_band, colour="#5e4234", kind="top", seed=751), None),
        "wall-face": (_interior_wall, None),
        "wall-base": (_k(wall_band, colour="#8a5636", kind="base", seed=761), None),
    },
    "crypt": {
        "floor": (_k(flagstones, colour=STONE_COOL, kind="plain", seed=771), None),
        "floor-worn": (_k(flagstones, colour=STONE_COOL, kind="worn", seed=771), None),
        "floor-shadow": (_k(flagstones, colour=STONE_COOL, kind="shadow", seed=771), None),
        "steps": (_k(steps, colour="#b0adbe"), None),
        "wall-top": (_k(wall_band, colour="#5e5a6e", kind="top", seed=781), None),
        "wall-face": (_crypt_wall, None),
        "wall-niche": (over_ground(_k(wall_feature, colour="#8e8aa0", kind="niche"), _crypt_wall), None),
        "wall-carving": (over_ground(_k(wall_feature, colour="#8e8aa0", kind="carving"), _crypt_wall), None),
        "wall-base": (_k(wall_band, colour="#76718a", kind="base", seed=791), None),
        "rubble": (detail("rubble", "#9a96a8", 801), None),
        "bones": (detail("bones", None, 811), None),
        "cobweb": (detail("cobweb", None, 821), None),
        "cobweb-ne": (detail("cobweb-ne", None, 831), None),
    },
    "forest": {
        "grass": (_k(lawn, colour=FOREST_GRASS, seed=841), None),
        "leaves": (_k(leaves, seed=851), fringe_edge(FOREST_GRASS, "grass")),
        "road": (_k(earth, colour=EARTH, ruts=True, seed=861), fringe_edge(FOREST_GRASS, "verge")),
        "undergrowth": (_k(thicket, colour="#2f6a46", seed=871), fringe_edge("#2f6a46", "lip")),
        "moss": (detail("moss", "#5f9a5a", 881), None),
        "tuft": (detail("tuft", "#5a9a48", 891), None),
        "fern": (detail("fern", None, 901), None),
        "flowers": (detail("flowers", None, 911), None),
        "pebbles": (detail("pebbles", None, 921), None),
        "roots": (detail("roots", None, 931), None),
        "ruts": (detail("ruts", EARTH, 941), None),
        "litter": (detail("litter", None, 951), None),
    },
    "meadow": {
        "grass": (_k(lawn, colour=MEADOW_GRASS, seed=961), None),
        "long-grass": (_k(lawn, colour="#68a84e", tufts=10, size=(9, 15), long=True, seed=971), fringe_edge(MEADOW_GRASS, "grass")),
        "path": (_k(earth, colour=PATH_EARTH, seed=981), fringe_edge(MEADOW_GRASS, "verge")),
        "road": (_k(earth, colour=EARTH, ruts=True, seed=991), fringe_edge(MEADOW_GRASS, "verge")),
        "soil": (_k(earth, colour=SOIL, furrows=True, pebbles=1, seed=1001), fringe_edge(MEADOW_GRASS, "grass")),
        "stream": (_k(liquid, colour="#4a9ac8", foam=True, seed=1011), fringe_edge(MEADOW_GRASS, "bank")),
        "ford": (_k(liquid, colour="#78bcd6", pebbles=True, seed=1021), fringe_edge("#b4aca0", "bank")),
        "hedge": (_k(thicket, colour="#3f7f4a", berries="flower.white", seed=1031), fringe_edge("#3f7f4a", "lip")),
        "wall": (_k(wall_face, colour="#d8ccb8", kind="stone", course=16, seed=1041), fringe_edge("#e3d7c4", "lip")),
        "flowers": (detail("flowers", None, 1051), None),
        "clover": (detail("clover", None, 1061), None),
        "tuft": (detail("tuft", "#62a650", 1071), None),
        "pebbles": (detail("pebbles", None, 1081), None),
        "ruts": (detail("ruts", EARTH, 1091), None),
        "reeds": (detail("reeds", None, 1101), None),
        "lilies": (detail("lilies", None, 1111), None),
        "crops": (detail("crops", "#8fae58", 1121), None),
        "stepping": (detail("stepping", None, 1131), None),
        "petals": (detail("petals", None, 1141), None),
    },
    "mirewood": {
        "grass": (_k(lawn, colour=MIRE_GRASS, seed=1151), None),
        "grass-wilt": (_k(lawn, colour="#94a050", flowers=False, seed=1161), None),
        "grass-rot": (_k(lawn, colour="#77704c", flowers=False, dead=True, seed=1171), None),
        "leaves": (_k(leaves, seed=1181), fringe_edge(MIRE_GRASS, "grass")),
        "leaves-blackvein": (_k(leaves, ground="#3e3442", colours=("#5a4a5a", "#4a3a48", "#6a5266"), vein="#1e1622", sheen="#8a5ac8", seed=1191),
                             fringe_edge("#77704c", "grass")),
        "gloom-moss": (_k(moss, colour="#4f7f72", speck="#2e4a48", seed=1201), fringe_edge("#4f7f72", "grass")),
        "trail": (_k(earth, colour="#a88a62", roots=True, seed=1211), fringe_edge(MIRE_GRASS, "verge")),
        "tar-stream": (_k(liquid, colour="#2c2434", sheen="#8a6ad8", drift=8, seed=1221), fringe_edge("#5a4a48", "bank")),
        "undergrowth": (_k(thicket, colour="#2f5e46", seed=1231), fringe_edge("#2f5e46", "lip")),
        "undergrowth-rot": (_k(thicket, colour="#4c4a3a", dead=True, thorns=True, seed=1241), fringe_edge("#4c4a3a", "lip")),
        "cliff-top": (_k(earth, colour="#8f8a9e", pebbles=2, seed=1251), fringe_edge("#8f8a9e", "lip")),
        "cliff-face": (_cliff, None),
        "cliff-base": (_k(wall_band, colour="#6e6a80", kind="base", seed=1261), None),
        "tuft": (detail("tuft", "#5a9650", 1271), None),
        "flowers": (detail("flowers", ("flower.violet", "flower.white", "flower.blue"), 1281), None),
        "fern": (detail("fern", "#4a8a5a", 1291), None),
        "pebbles": (detail("pebbles", None, 1301), None),
        "roots": (detail("roots", None, 1311), None),
        "litter": (detail("litter", None, 1321), None),
        "veins": (detail("veins", None, 1331), None),
        "tuft-dead": (detail("tuft-dead", None, 1341), None),
        "toadstools": (detail("toadstools", None, 1351), None),
        "tar-spatter": (detail("tar-spatter", None, 1361), None),
        "brambles": (detail("brambles", None, 1371), None),
        "tar-slick": (detail("tar-slick", None, 1381), None),
    },
    "quarry": {
        "floor": (_k(flagstones, colour=STONE_CUT, kind="plain", seed=1391), None),
        "floor-shadow": (_k(flagstones, colour=STONE_CUT, kind="shadow", seed=1391), None),
        "black-vein-floor": (_k(flagstones, colour=STONE_CUT, kind="vein", seed=1391), None),
        "root-floor": (_k(earth, colour="#8a6a4e", roots=True, seed=1401), fringe_edge(STONE_CUT, "broken")),
        "tar-seep": (_k(liquid, colour="#2c2434", sheen="#8a6ad8", drift=8, bands=False, seed=1411), fringe_edge("#5a4a48", "bank")),
        "ramp": (_k(steps, colour="#a89c90", seed=1421), None),
        "wall-top": (_k(wall_band, colour="#6e645c", kind="top", seed=1431), None),
        "wall-face": (_quarry_wall, None),
        "wall-roots": (over_ground(_k(wall_feature, kind="roots"), _quarry_wall), None),
        "wall-crystal": (over_ground(_k(wall_feature, kind="crystal"), _quarry_wall), None),
        "wall-base": (_k(wall_band, colour="#857a70", kind="base", seed=1441), None),
        "rubble": (detail("rubble", "#a89c90", 1451), None),
        "gravel": (detail("gravel", None, 1461), None),
        "roots": (detail("roots", None, 1471), None),
        "shards": (detail("shards", None, 1481), None),
        "fungus": (detail("fungus", None, 1491), None),
        "tools": (detail("tools", None, 1501), None),
    },
    "keep": {
        "marble": (_k(marble, colour=MARBLE, seed=1511), None),
        "marble-gold": (_k(marble, colour=MARBLE, gold=True, seed=1521), None),
        "carpet": (_k(carpet, colour="#a8323e", pattern="#e9b54c", seed=1531), border_edge("#5a1a26", "#e9b54c")),
        "dais": (_k(marble, colour="#f2ede4", square=64, seed=1541), border_edge("#b08a3a", "#e9b54c")),
        "steps": (_k(steps, colour="#e2dce8", seed=1551), None),
        "wall-top": (_k(wall_band, colour="#b8aec4", kind="top", seed=1561), None),
        "wall-high": (_k(wall_band, colour="#e6dccb", kind="cornice", trim="#e9b54c", seed=1571), None),
        "wall-face": (_keep_wall, None),
        "wall-base": (_k(wall_band, colour="#3f5a96", kind="base", seed=1581), None),
        "window-high": (over_ground(_k(wall_feature, kind="window-high"), _keep_wall), None),
        "window": (over_ground(_k(wall_feature, kind="window"), _keep_wall), None),
    },
    "vault": {
        "floor": (_k(flagstones, colour=STONE_PALE, kind="gold", trim="#d9a83e", seed=1591), None),
        "floor-shadow": (_k(flagstones, colour=STONE_PALE, kind="shadow", seed=1591), None),
        "floor-cracked": (_k(flagstones, colour=STONE_PALE, kind="cracked", seed=1591), None),
        "inlay": (_k(marble, colour="#e9c46a", square=32, seed=1601), border_edge("#8a6a2a", "#e9b54c")),
        "sun-crest": (sun_crest, None),
        "pressure-plate": (over_ground(plate, _k(slabs, colour=STONE_PALE, inlay=False, seed=1611)), None),
        "pit": (_k(liquid, colour="#141020", bands=False, dashes=0, seed=1621), fringe_edge(STONE_PALE, "pit")),
        "earth": (_k(earth, colour="#8a6a4e", pebbles=4, seed=1631), fringe_edge(STONE_PALE, "broken")),
        "tunnel": (_k(earth, colour="#7a5e46", pebbles=3, seed=1641), None),
        "steps": (_k(steps, colour="#d8ccb4", seed=1651), None),
        "wall-top": (_k(wall_band, colour="#8a7a64", kind="top", seed=1661), None),
        "wall-face": (_vault_wall, None),
        "wall-crest": (over_ground(_k(wall_feature, kind="crest"), _vault_wall), None),
        "wall-niche": (over_ground(_k(wall_feature, colour="#c9bca4", kind="niche"), _vault_wall), None),
        "wall-base": (_k(wall_band, colour="#a8936e", kind="base", seed=1671), None),
        "earth-top": (_k(earth, colour="#8a6a4e", pebbles=2, seed=1681), None),
        "earth-face": (_earth_face, None),
        "coins": (detail("coins", None, 1691), None),
        "rubble": (detail("rubble", "#c9bca4", 1701), None),
        "debris": (detail("debris", "#b4a68c", 1711), None),
        "darts": (detail("darts", None, 1721), None),
        "bones": (detail("bones", None, 1731), None),
        "cobweb": (detail("cobweb", None, 1741), None),
        "cobweb-ne": (detail("cobweb-ne", None, 1751), None),
    },
}


def frames_of(tileset_id):
    """The tileset's tiles, read off TilesetCatalog.cs: (name, variants, frames, block, edged)."""
    return roomkit.tileset(tileset_id).tiles


def sheet(tileset_id, quiet=False):
    """Paints a whole tileset's sheet: every tile's frames in the catalog's order, eight to a row; a
    block tile's block x block frames for each of its variants and frames, row by row, as
    Tilemap.FrameOf counts them; then its four edges if it's edged."""
    painters = PAINTERS.get(tileset_id)
    if painters is None:
        raise KeyError("paint_tiles.py doesn't paint the %s tileset yet" % tileset_id)
    tiles = frames_of(tileset_id)
    images = []
    for name, variants, frames, block, edged in tiles:
        if name not in painters:
            raise KeyError("paint_tiles.py doesn't paint %s's %s tile yet" % (tileset_id, name))
        fn, edge_fn = painters[name]
        for v in range(variants):
            for f in range(frames):
                got = fn(v, f)
                cells = got if isinstance(got, list) else [got]
                if len(cells) != block * block:
                    raise ValueError("%s/%s gave %d frames for a block of %d" % (tileset_id, name, len(cells), block))
                images += cells
                if not quiet:
                    print("  %s %s variant %d frame %d" % (tileset_id, name, v, f))
        if edged:
            if edge_fn is None:
                raise KeyError("%s/%s is edged but nothing paints its edges" % (tileset_id, name))
            for side in range(4):
                images.append(edge_fn(side))
    rows = -(-len(images) // COLUMNS)
    out = np.zeros((rows * T, COLUMNS * T, 4), np.uint8)
    for i, img in enumerate(images):
        r, c = divmod(i, COLUMNS)
        out[r * T:(r + 1) * T, c * T:(c + 1) * T] = img
    return out


def frame_rects(tileset_id):
    """Where each tile's first frame is on the sheet: {name: index} (for checks)."""
    out = {}
    i = 0
    for name, variants, frames, block, edged in frames_of(tileset_id):
        out[name] = i
        i += variants * frames * block * block + (4 if edged else 0)
    return out


def review_sheet(img, tileset_id, wrap=6):
    """A sheet laid out for looking at: each tile in a patch of cells picked by Tilemap's hash (3 x 3,
    or two blocks each way for a block tile, one for a big block), on a field of its set's first
    tile, with an edged tile's edges drawn where the patch meets the field, as Tilemap does; so
    seams and edges show. The patches run `wrap` to a row."""
    ts = roomkit.tileset(tileset_id)
    names = [t[0] for t in ts.tiles]
    cols = img.shape[1] // T

    def frame(f):
        r, c = divmod(f, cols)
        return img[r * T:(r + 1) * T, c * T:(c + 1) * T].astype(np.float64)

    sizes = []
    for name in names:
        block = ts.tiles[ts.ids[name] - 1][3]
        sizes.append(3 if block == 1 else (2 * block if block <= 3 else block))
    gap = 24
    cell = max(sizes) * T
    rows = -(-len(names) // wrap)
    W = min(len(names), wrap) * (cell + gap) + gap
    H = rows * (cell + gap) + gap
    out = np.zeros((H, W, 3), np.float64)
    out[...] = (40, 34, 56)
    for k, name in enumerate(names):
        tid = ts.ids[name]
        n = sizes[k]
        ox = gap + (k % wrap) * (cell + gap)
        oy = gap + (k // wrap) * (cell + gap)
        for y in range(n):
            for x in range(n):
                t = frame(ts.frame_of(tid, x + 7 * k, y))
                a = t[..., 3:4] / 255.0
                sub = out[oy + y * T:oy + (y + 1) * T, ox + x * T:ox + (x + 1) * T]
                sub[...] = t[..., :3] * a + sub * (1 - a)
                if ts.edged(tid):
                    for side, (dx, dy) in enumerate(((0, -1), (1, 0), (0, 1), (-1, 0))):
                        if not (0 <= x + dx < n and 0 <= y + dy < n):
                            e = frame(ts.edge_frame(tid, side))
                            ea = e[..., 3:4] / 255.0
                            sub[...] = e[..., :3] * ea + sub * (1 - ea)
    o = np.zeros((H, W, 4), np.uint8)
    o[..., :3] = np.rint(out).astype(np.uint8)
    o[..., 3] = 255
    return o


def paint(preview=None, only=None, quiet=True):
    """paint_all.py's hook: paints every tileset (or `only` some) into Content/Tiles/<id>.png at 64
    texels a tile, and its review sheet into `preview` (default shots/art/rooms/tiles). Returns the
    files written, relative to Content."""
    folder = preview or OUT
    written = []
    for tid in only or sorted(PAINTERS):
        img = sheet(tid, quiet=quiet)
        write_png(os.path.join(roomkit.CONTENT, "Tiles", tid + ".png"), img)
        write_png(os.path.join(folder, tid + "-review.png"), review_sheet(img, tid))
        written.append("Tiles/%s.png" % tid)
    return written


#: Whose floor each tileset is, for its ASSETS.md line.
WHOSE = {
    "questeria": "Questeria's and the harbour's",
    "ship": "the Gull's Wager's deck's",
    "interior": "the floors inside Questeria's buildings,",
    "forest": "the old road's, under the trees,",
    "meadow": "the meadows', the old road's and Thornbridge's",
    "mirewood": "the Mirewood's",
    "quarry": "the Old Quarry's",
    "crypt": "the barrow's",
    "vault": "the Old Crown Vault's",
    "keep": "Sunspire Keep's",
}


def assets_lines():
    """paint_all.py's ASSETS.md lines for the tilesets: ([the file], the line) for each one in
    Content/Tiles that PAINTERS paints."""
    lines = []
    for tid in sorted(PAINTERS):
        if not os.path.isfile(os.path.join(roomkit.CONTENT, "Tiles", tid + ".png")):
            continue
        f = "Content/Tiles/%s.png" % tid
        tiles = ", ".join(n.replace("-", " ") for n in PAINTERS[tid])
        lines.append(([f], "%s - created by Caleb Kiatoukaysi for Legend Quest, %s floor tiles (%s) for the room's tile "
                      "strip, 64 texels a tile in the illustrated style, lit from the top left, painted procedurally by "
                      "tools/rooms/paint_tiles.py with tools/rooms/paint.py, released under public domain"
                      % (f, WHOSE.get(tid, "the " + tid), tiles)))
    return lines


def main():
    ap = argparse.ArgumentParser(description="Paint the floor tilesets in the painted style, 64 texels a tile.")
    ap.add_argument("only", nargs="*", help="tilesets to paint (default: every one PAINTERS has)")
    ap.add_argument("--out", default=OUT, help="where <id>.png goes without --content (default shots/art/rooms/tiles)")
    ap.add_argument("--content", action="store_true", help="write Content/Tiles/<id>.png (what paint_all.py does)")
    ap.add_argument("--sheet", action="store_true", help="also write <id>-review.png into --out, each tile in a patch")
    args = ap.parse_args()
    for tid in args.only or sorted(PAINTERS):
        img = sheet(tid, quiet=True)
        write_png(os.path.join(os.path.join(roomkit.CONTENT, "Tiles") if args.content else args.out, tid + ".png"), img)
        if args.sheet:
            write_png(os.path.join(args.out, tid + "-review.png"), review_sheet(img, tid))


if __name__ == "__main__":
    main()
