"""
Under the ground, painted (12-rooms §6): the barrow off the Old Road (its steps, its hall and the
side room) and the Old Crown Vault under the Sunspire (the antechamber, the long hall, the relic
gallery and the treasury). One recipe a room, each a function of a paint_room.Room, registered in
RECIPES; tools/rooms/paint_all.py runs them.

    python tools/rooms/paint_all.py under --no-shared --preview shots/art/rooms
    python tools/rooms/paint_room.py barrow/hall --preview shots/art/rooms

The look is the Lantern Market's (paint.py's), indoors and underground: clean dark outlines, flat
fills cel-shaded in two or three tones lit from the top left. Every room here is dark, and the game
darkens the mid layer, the floor and the foreground with its light map (World/LightMap.cs) and
lights them again round the zone file's lights and its sconces, braziers and the like; so the
walls are painted in readable middle values, not dark, and only the sky and far layer, which keep
their own light, are painted as dark as they'll look: the black under the hill, and far halls
seen through the arches with torches burning in them.

A hall is a row of bays between piers (hall()): each bay a wall of dressed stone with a round arch
springing from the piers' capitals and a recess under it, which is a door, a row of burial niches,
a relief, shelves of relics, a heap of treasure, or open on the dark beyond; a cornice runs along
the top, a plinth along the floor. The barrow is old grey-violet stone with moss, roots through the
cracks and cobwebs; the vault is cream stone dressed with gold, navy banners and the Aurelian sun.

The parts, built on paint.py's Piece and tones so they match its own: ashlar(), facets() and
strata() (stone, rock and quarried faces, worked out over a piece's samples from absolute
coordinates, so neighbouring pieces join without seams), dark_sky(), far_hall(), hall(), pier(),
stone_door(), vault_door(), stair_up(), niches(), sarcophagus(), relief(), relic_shelf(),
treasure_heap(), torch(), roots(), cobweb(), crystal_cluster(), fungus(), rock_wall(). The quarry's
rooms (paint_rooms_wild.py) use the rock, crystal, fungus and root parts too.

Floor props (sconces, braziers, urns, banners on poles, gold piles, relic cases ...) are the game's,
drawn from the zone file's prop lines, so nothing here paints them; the walls round a sconce get a
warm wash so its pool of light reads as firelight.

Deterministic: all noise is hash01 seeded from the room's name. Only needs CPython and numpy.
"""

import math
import os
import sys

import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
import paint as P  # noqa: E402
import roomkit  # noqa: E402
from paint import Path, Piece, hash01, haze, tones  # noqa: E402
from roomkit import CANVAS_H  # noqa: E402
from vector import Mask  # noqa: E402

# --------------------------------------------------------------------------- #
# Laying long things in, chunk by chunk (paint_rooms_wild.py uses these too)
# --------------------------------------------------------------------------- #

CHUNK = 640
OVERLAP = 64


def chunks(x0, x1, step=CHUNK):
    """The stretches (xa, xb) a long thing from x0 to x1 is laid in by."""
    out = []
    xa = x0
    while xa < x1:
        xb = min(x1, xa + step)
        out.append((xa, xb))
        xa = xb
    return out


def place_span(layer, p, xa=None, xb=None, ink=P.INK, reach=P.OUTER, ink_alpha=1.0, fade=None):
    """Lays a piece into a layer as Layer.place does (its contact shadows, it with its outline, its
    glows), but only its columns from layer x xa to xb, so a long thing drawn in overlapping
    chunks shows no outline where they meet. `fade` (y0, y1) fades it out from y0 down to y1."""
    xa = p.ox if xa is None else xa
    xb = p.ox + p.w if xb is None else xb
    for (x0, x1, y, depth, strength) in p.shadows:
        cx = (x0 + x1) / 2.0
        if xa <= cx < xb:
            layer.soft_ellipse(cx, y, (x1 - x0) / 2.0, depth, P.SHADOW_COLOUR, strength)
    img = p.rgba_premultiplied(ink, reach, ink_alpha)
    a = max(0, int(math.floor(xa)) - p.ox)
    b = min(p.w, int(math.floor(xb)) - p.ox)
    if b <= a:
        return
    sub = img[:, a:b]
    if fade is not None:
        y0, y1 = fade
        ys = p.oy + np.arange(sub.shape[0]) + 0.5
        k = np.clip((y1 - ys) / max(1.0, y1 - y0), 0.0, 1.0)
        sub = sub * k[:, None, None]
    layer.put(sub, p.ox + a, p.oy)
    for (x, y, r, c, s) in p.glows:
        if xa <= x < xb:
            layer.glow(x, y, r, c, s)


def pick(seq, i, k, seed):
    return seq[int(hash01(i, k, seed) * len(seq)) % len(seq)]


# --------------------------------------------------------------------------- #
# Surfaces worked out over a piece's samples: dressed stone, rock, quarried faces
# --------------------------------------------------------------------------- #

def vhash(x, y, seed):
    """hash01 over arrays of integers (the same numbers it gives one at a time)."""
    x = np.asarray(x, np.int64)
    y = np.asarray(y, np.int64)
    h = (x * 374761393 + y * 668265263 + int(seed) * 2147483647) & 0xFFFFFFFF
    h = ((h ^ (h >> 13)) * 1274126177) & 0xFFFFFFFF
    return ((h ^ (h >> 16)) & 0xFFFF) / 65535.0


def _samples(p):
    """The layer coordinates of every sample of a piece's canvas: (X, Y), each H x W."""
    ss = p.c.ss
    H, W = p.c.cov.shape
    xs = (np.arange(W) + 0.5) / ss + p.ox
    ys = (np.arange(H) + 0.5) / ss + p.oy
    return np.meshgrid(xs, ys)


def _band(v, w):
    """Coverage of a line w wide at the zero of v (v in texels), softened over a sample."""
    return np.clip(w / 2.0 - np.abs(v) + 0.5, 0.0, 1.0)


def ashlar(p, region, colour, seed=0, course=30, block=62, tint=0.2, joint=1.3, lips=True, worn=0.0):
    """Dressed stone over a region (a Mask) of a piece: courses `course` high, blocks about `block`
    long breaking joint course by course, a few blocks a shade lighter or darker, the joints in the
    shadow tone with a lit lip under each, and (worn) chips and cracks."""
    t = tones(colour)
    p.tone(region, t, "base")
    X, Y = _samples(p)
    r = np.floor(Y / course).astype(np.int64)
    off = (r % 2) * block * 0.5
    c = np.floor((X - off) / block).astype(np.int64)
    # each block's own length: its right joint moved by up to a third of a block
    jit = (vhash(c, r, seed) - 0.5) * block * 0.36
    jit_r = (vhash(c + 1, r, seed) - 0.5) * block * 0.36
    left = off + c * block + jit
    right = off + (c + 1) * block + jit_r
    inb = (X >= left) & (X < right)
    cc = np.where(inb, c, np.where(X < left, c - 1, c + 1))
    left = np.where(inb, left, np.where(X < left, off + (c - 1) * block + (vhash(c - 1, r, seed) - 0.5) * block * 0.36, right))
    tint_v = vhash(cc, r, seed + 3)
    if tint:
        p.tone(region & Mask(p.c, (tint_v < tint * 0.6).astype(np.float64)), t, "terminator", alpha=0.7)
        p.tone(region & Mask(p.c, (tint_v > 1 - tint * 0.5).astype(np.float64)), t, "base-light", alpha=0.75)
    yin = Y - r * course
    if lips:
        p.tone(region & Mask(p.c, ((yin > joint) & (yin < joint + 1.8)).astype(np.float64)), t, "light", alpha=0.6)
    lines = np.maximum(_band(yin, joint * 2), _band(Y - (r + 1) * course, joint * 2))
    lines = np.maximum(lines, _band(X - left, joint * 1.6))
    p.tone(region & Mask(p.c, lines), t, "shadow", alpha=0.95)
    if worn:
        k = (vhash(cc, r, seed + 5) < worn * 0.35)
        cx_ = left + (vhash(cc, r, seed + 6)) * block * 0.6 + 6
        cy_ = r * course + course * (0.3 + 0.4 * vhash(cc, r, seed + 7))
        chip = k & ((X - cx_) ** 2 / 36.0 + (Y - cy_) ** 2 / 9.0 < 1.0)
        p.tone(region & Mask(p.c, chip.astype(np.float64)), t, "shadow")
    return region


def facets(p, region, colour, seed=0, cell=64, squash=0.8, crack=1.4, tilt=0.0, light=1.0):
    """Rock over a region: broken into angular faces (a jittered Voronoi of cells about `cell`
    across), each face lit on the half toward the light and shadowed on the other, some faces a
    tone lighter or darker, the cracks between them in the deep tone."""
    t = tones(colour)
    X, Y = _samples(p)
    ch = cell * squash
    gx = np.floor(X / cell).astype(np.int64)
    gy = np.floor((Y + X * tilt) / ch).astype(np.int64)
    d1 = np.full(X.shape, 1e9)
    d2 = np.full(X.shape, 1e9)
    bx = np.zeros(X.shape, np.int64)
    by = np.zeros(X.shape, np.int64)
    px_ = np.zeros(X.shape)
    py_ = np.zeros(X.shape)
    for dj in (-1, 0, 1):
        for di in (-1, 0, 1):
            ix, iy = gx + di, gy + dj
            qx = (ix + 0.15 + 0.7 * vhash(ix, iy, seed)) * cell
            qy = (iy + 0.15 + 0.7 * vhash(ix, iy, seed + 1)) * ch - X * tilt
            d = np.hypot(X - qx, (Y - qy) / squash)
            closer = d < d1
            d2 = np.where(closer, d1, np.minimum(d2, d))
            bx = np.where(closer, ix, bx)
            by = np.where(closer, iy, by)
            px_ = np.where(closer, qx, px_)
            py_ = np.where(closer, qy, py_)
            d1 = np.where(closer, d, d1)
    p.tone(region, t, "base")
    kind = vhash(bx, by, seed + 2)
    lit_side = ((X - px_) * -0.55 + (Y - py_) * -0.85) > cell * 0.06 * (0.5 - vhash(bx, by, seed + 4))
    dark_side = ((X - px_) * 0.6 + (Y - py_) * 0.8) > cell * 0.12
    M = lambda a: region & Mask(p.c, a.astype(np.float64))  # noqa: E731
    p.tone(M((kind < 0.22)), t, "terminator", alpha=0.8)
    p.tone(M((kind > 0.82)), t, "base-light", alpha=0.8)
    if light:
        p.tone(M(lit_side & (kind > 0.3)), t, "light", alpha=0.55 * light)
    p.tone(M(dark_side), t, "shadow", alpha=0.85)
    p.tone(M(dark_side & (kind < 0.35)), t, "deep-shadow", alpha=0.6)
    gap = d2 - d1
    p.tone(region & Mask(p.c, _band(gap, crack * 2.2)), t, "deep")
    return region


def strata(p, region, colour, seed=0, row=(46, 78), block=(90, 170), marks=True):
    """A quarried face over a region: the rock cut in steps of blocks, each course a different
    height, each block's top a lit ledge and its right end a shadowed return, with the chisel's
    diagonal marks across its face."""
    t = tones(colour)
    X, Y = _samples(p)
    p.tone(region, t, "base")
    # rows of varying height: find each sample's row from a fixed list of row tops
    y0 = int(math.floor((p.oy - 200) / row[1])) * row[1]
    tops = [float(y0)]
    k = 0
    while tops[-1] < p.oy + p.h + 200:
        tops.append(tops[-1] + row[0] + (row[1] - row[0]) * hash01(int(tops[-1]), 1, seed))
        k += 1
    tops = np.array(tops)
    ri = np.clip(np.searchsorted(tops, Y, side="right") - 1, 0, len(tops) - 2)
    rt = tops[ri]
    rb = tops[ri + 1]
    off = vhash(ri, 0, seed + 1) * block[1]
    bw = block[0] + (block[1] - block[0]) * vhash(ri, 1, seed + 2)
    c = np.floor((X - off) / bw)
    left = off + c * bw
    right = left + bw
    shade = vhash(c.astype(np.int64), ri + 7, seed + 3)
    M = lambda a: region & Mask(p.c, np.asarray(a, np.float64))  # noqa: E731
    p.tone(M(shade < 0.25), t, "terminator", alpha=0.8)
    p.tone(M(shade > 0.8), t, "base-light", alpha=0.8)
    p.tone(M((Y - rt) < 5.0), t, "light")
    p.tone(M((right - X) < 7.0), t, "shadow")
    p.tone(M((rb - Y) < 4.0), t, "deep-shadow", alpha=0.8)
    if marks:
        u = ((X - left) + (Y - rt) * 0.8) % 11.0
        hatch = (u < 1.1) & ((X - left) > 10) & ((right - X) > 12) & ((Y - rt) > 8) & ((rb - Y) > 8) & (shade > 0.35)
        p.tone(M(hatch), t, "terminator", alpha=0.7)
    p.tone(M(np.maximum(_band(Y - rt, 2.0), _band(X - left, 2.0))), t, "deep")
    return region


# --------------------------------------------------------------------------- #
# Styles
# --------------------------------------------------------------------------- #

BARROW = {
    "wall": "#a49cb2", "front": "#b2aabf", "recess": "#7e7690", "pier": "#bab2c6", "trim": "#958ca6",
    "dark": "#241e30", "far": "#4a4260", "moss": "#7a9a62", "accent": "#8fb0c8", "glow": "#ffb35a",
    "door": "#8f879e", "course": 32, "block": 64,
}
VAULT = {
    "wall": "#dccaa8", "front": "#e8d9bd", "recess": "#a8946f", "pier": "#efe2c8", "trim": "gold",
    "dark": "#2a2218", "far": "#5c4e38", "moss": None, "accent": "cloth.navy", "glow": "#ffc46a",
    "door": "#c9973f", "course": 30, "block": 70,
}


# --------------------------------------------------------------------------- #
# The dark beyond: the sky and the far layer underground
# --------------------------------------------------------------------------- #

def dark_sky(layer, top="#0c0a14", bottom="#1c1828", seed=0, motes=40, mote="#8a80b0"):
    """The black under the hill: a sky layer of near-black going a little lighter toward the floor,
    with motes of dust hanging in it. It keeps its own light, so it's as dark as it'll look."""
    h = layer.h
    v = np.clip(np.arange(h, dtype=np.float64) / h, 0, 1)
    col = P.lerp_colour([(0.0, top), (1.0, bottom)], v)
    layer.px[..., :3] = col[:, None, :] / 255.0
    layer.px[..., 3] = 1.0
    for i in range(motes):
        x = hash01(i, 1, seed) * layer.w
        y = hash01(i, 2, seed) * h * 0.9
        layer.soft_ellipse(x, y, 1.6, 1.6, mote, 0.3 + 0.3 * hash01(i, 3, seed), hard=0.3)


def torch(p, x, y, s=1.0, lit=True, glow=True):
    """A torch in an iron bracket on a wall, its top at about (x, y): the bracket's ring and stem,
    the wrapped head, and the flame in three tones with its glow."""
    iron = tones("iron")
    p.stroke(Path.polyline([(x - 10 * s, y + 46 * s), (x - 2 * s, y + 30 * s)]), 2.4 * s, iron, "base")
    p.fill(Path.rect(x - 14 * s, y + 42 * s, 7 * s, 14 * s, r=1.5), iron, "shadow")
    shaft = Path.polygon([(x - 3.5 * s, y + 8 * s), (x + 3.5 * s, y + 8 * s), (x + 2.2 * s, y + 52 * s), (x - 2.2 * s, y + 52 * s)])
    p.fill(shaft, tones("wood.dark"), "base")
    p.edge(shaft, tones("wood.dark"), 1.0)
    ring = Path.rect(x - 6 * s, y + 26 * s, 12 * s, 5 * s, r=1.5)
    p.fill(ring, iron, "base")
    head = Path.rect(x - 6 * s, y + 4 * s, 12 * s, 12 * s, r=3)
    hm = p.fill(head, tones("#8a6a4e"), "base")
    for k in range(3):
        p.stroke(Path.polyline([(x - 6 * s, y + 6 * s + k * 4 * s), (x + 6 * s, y + 8 * s + k * 4 * s)]), 0.9, tones("#8a6a4e"), "deep", clip=hm)
    p.edge(head, tones("#8a6a4e"), 1.0)
    if lit:
        fl, core = tones("#ff9a3c"), tones("#ffe08a")
        flame = Path().move(x - 8 * s, y + 6 * s).quad(x - 10 * s, y - 14 * s, x + 1 * s, y - 30 * s).quad(x + 2 * s, y - 12 * s, x + 8 * s, y + 6 * s).close()
        p.fill(flame, fl, "base")
        inner = Path().move(x - 4 * s, y + 6 * s).quad(x - 5 * s, y - 6 * s, x + 0.5 * s, y - 16 * s).quad(x + 1 * s, y - 4 * s, x + 4 * s, y + 6 * s).close()
        p.fill(inner, core, "base")
        if glow:
            p.glow(x, y - 4 * s, 110 * s, "#ff9a4a", 0.32)
            p.glow(x, y - 6 * s, 34 * s, "#ffe0a0", 0.45)


def brazier(p, x, ground, s=1.0, lit=True):
    """A bronze brazier on three legs, a bowl of coals and a fire in it (for the far halls)."""
    br = tones("brass")
    for a in (-1, 0, 1):
        p.stroke(Path.polyline([(x + a * 4 * s, ground - 40 * s), (x + a * 18 * s, ground)]), 3 * s, br, "shadow")
    bowl = Path().move(x - 26 * s, ground - 44 * s).line(x + 26 * s, ground - 44 * s).quad(x + 22 * s, ground - 30 * s, x, ground - 28 * s) \
        .quad(x - 22 * s, ground - 30 * s, x - 26 * s, ground - 44 * s).close()
    p.fill(bowl, br, "base")
    p.edge(bowl, br, 1.0)
    if lit:
        for k, (fx, fh) in enumerate(((-10, 30), (0, 44), (10, 32))):
            cx_ = x + fx * s
            flame = Path().move(cx_ - 8 * s, ground - 44 * s).quad(cx_ - 9 * s, ground - 44 * s - fh * s * 0.6, cx_, ground - 44 * s - fh * s) \
                .quad(cx_ + 2 * s, ground - 44 * s - fh * s * 0.45, cx_ + 8 * s, ground - 44 * s).close()
            p.fill(flame, tones("#ff9a3c"), "base")
        p.glow(x, ground - 60 * s, 120 * s, "#ff9a4a", 0.35)


def far_hall(layer, ground, style, seed=0, k=0.55, span=300, top=40, lights="torch", openings=None):
    """The halls beyond, seen through the arches: a dark arcade of piers and round arches going on
    under a vault, the stone hazed most of the way to the dark, torches (or braziers) burning on
    the piers. It keeps its own light, so it's painted as dark as it'll look. `openings` ((x0, x1,
    painter), ...) leaves stretches for something else (the daylight outside)."""
    dark = style["dark"]
    st = haze(tones(style["wall"]), dark, k)
    rec = haze(tones(style["recess"]), dark, min(0.92, k + 0.2))
    W = layer.w
    for xa, xb in chunks(0, W):
        p = Piece(xa - OVERLAP, top - 10, xb + OVERLAP, ground + 40, ss=2, pad=4)
        whole = p.shape(Path.rect(xa - OVERLAP, top, xb - xa + 2 * OVERLAP, ground + 40 - top))
        ashlar(p, whole, P.mixc(style["wall"], dark, k), seed, course=style["course"] * 0.7, block=style["block"] * 0.7, tint=0.1, lips=False)
        i0 = int(math.floor((xa - OVERLAP) / span)) - 1
        for i in range(i0, int((xb + OVERLAP) / span) + 2):
            cx = i * span + span / 2
            aw = span * 0.62
            spring = ground - (ground - top) * 0.42
            arch = Path().move(cx - aw / 2, ground + 40).line(cx - aw / 2, spring).arc(cx, spring, aw / 2, 180, 360).line(cx + aw / 2, ground + 40).close()
            am = p.shape(arch)
            p.tone(am, rec, "base")
            p.tone(am - am.shift(0, 18), rec, "deep")
            p.tone(p.where(am, lambda xx, yy, a=cx: xx > a + aw * 0.2), rec, "shadow")
            # a further arch inside, darker still
            inner = Path().move(cx - aw * 0.3, ground + 40).line(cx - aw * 0.3, spring + aw * 0.2).arc(cx, spring + aw * 0.2, aw * 0.3, 180, 360) \
                .line(cx + aw * 0.3, ground + 40).close()
            p.tone(p.shape(inner), rec, "deep")
            ring = am - am.inset(7)
            p.tone(ring, st, "base-light")
            px_ = i * span
            pier_m = p.shape(Path.rect(px_ - 20, top + 30, 40, ground - top))
            p.tone(pier_m, st, "base-light")
            p.tone(p.where(pier_m, lambda xx, yy, a=px_: xx > a + 6), st, "shadow")
            p.tone(p.shape(Path.rect(px_ - 26, spring - 16, 52, 14)), st, "light")
            if lights == "torch":
                torch(p, px_, spring - 70, 0.8, glow=True)
            elif lights == "brazier":
                brazier(p, px_ + span * 0.5, ground + 10, 0.8)
        p.tone(p.shape(Path.rect(xa - OVERLAP, top, xb - xa + 2 * OVERLAP, 26)), st, "shadow")
        place_span(layer, p, xa, xb, ink=P.mixc(P.INK, dark, 0.3), reach=0.8)
    if openings:
        for x0, x1, fn in openings:
            fn(layer, x0, x1)


# --------------------------------------------------------------------------- #
# Halls: bays between piers
# --------------------------------------------------------------------------- #

def bay(layer, xa, xb, ground, style, seed=0, spring=None, top=0, kind="wall", pier_w=64, content=None, open_=False, roots_=0.0, cobwebs=True):
    """One bay of a hall between the piers at xa and xb: the front wall of dressed stone from the top
    down to the floor, a round arch springing at `spring` from the piers' sides with its voussoirs,
    the recess under it (a step back, darker, in the soffit's shadow at its top) or, if open_, a
    hole on whatever's beyond; a cornice along the top and a plinth along the floor. `content`
    draws in the recess (a door, niches ...) on a piece of its own after."""
    s = style
    spring = spring if spring is not None else ground - (ground - top) * 0.6
    ax0, ax1 = xa + pier_w / 2, xb - pier_w / 2
    aw = ax1 - ax0
    ar = aw / 2
    cx = (ax0 + ax1) / 2
    rise = min(ar, (spring - top) * 0.82)
    p = Piece(xa - 8, top, xb + 8, ground + 8, ss=2, pad=4)
    arch = Path().move(ax0, ground + 8).line(ax0, spring).arc(cx, spring, ar, 180, 360, rise).line(ax1, ground + 8).close()
    am = p.shape(arch)
    front = p.shape(Path.rect(xa - 8, top, xb - xa + 16, ground + 8 - top)) - am
    ashlar(p, front, s["front"], seed, course=s["course"], block=s["block"], worn=0.6)
    if not open_:
        ashlar(p, am, s["recess"], seed + 1, course=s["course"], block=s["block"], tint=0.15)
        rt = tones(s["recess"])
        p.tone(am - am.shift(0, 26), rt, "deep-shadow", alpha=0.9)
        p.tone(p.where(am, lambda xx, yy: xx < ax0 + 14), rt, "shadow", alpha=0.8)
    # voussoirs
    vw = 20
    vou = Path().move(ax0 - vw, spring).arc(cx, spring, ar + vw, 180, 360, rise + vw).line(ax1, spring).arc(cx, spring, ar, 360, 180, rise).close()
    ft = tones(s["front"])
    vm = p.fill(vou, ft, "base-light")
    n = max(9, int(ar / 14))
    lines = Path()
    for k in range(1, n):
        a = math.radians(180 + 180 * k / n)
        lines.add(Path.polyline([(cx + math.cos(a) * ar, spring + math.sin(a) * rise), (cx + math.cos(a) * (ar + vw), spring + math.sin(a) * (rise + vw))]))
    p.stroke(lines, 1.1, ft, "shadow", clip=vm)
    key = Path.polygon([(cx - 10, spring - rise - vw - 4), (cx + 10, spring - rise - vw - 4), (cx + 7, spring - rise + 4), (cx - 7, spring - rise + 4)])
    if s["trim"] == "gold":
        kt = tones("gold")
        p.fill(key, kt, "base")
        p.tone(p.shape(key) - p.shape(key).shift(-1.5, 1.5), kt, "light")
        p.stroke(Path().arc(cx, spring, ar + vw + 2, 180, 360, rise + vw + 2), 2.2, kt, "base")
    else:
        p.fill(key, ft, "light")
    p.edge(key, ft, 1.0, "shadow")
    p.edge(vou, ft, 1.2, "terminator")
    # cornice and plinth
    cor = Path.rect(xa - 8, top, xb - xa + 16, 34)
    cm = p.fill(cor, ft, "base")
    p.tone(p.below(cm, top + 24), ft, "light")
    p.tone(p.above(cm, top + 12), ft, "shadow")
    for k in range(int((xb - xa) / 40) + 1):
        dx = xa + 10 + k * 40
        dent = Path.rect(dx, top + 34, 18, 12)
        p.fill(dent, ft, "base")
        p.tone(p.shape(dent) - p.shape(dent).shift(0, 3), ft, "light")
        p.edge(dent, ft, 0.9, "shadow")
    if s["trim"] == "gold":
        p.stroke(Path.polyline([(xa - 8, top + 24), (xb + 8, top + 24)]), 2.4, tones("gold"), "base")
    p.stroke(Path.polyline([(xa - 8, top + 34), (xb + 8, top + 34)]), 1.4, ft, "deep")
    pl = Path.rect(xa - 8, ground - 30, xb - xa + 16, 38)
    plm = p.shape(pl) - am if not open_ else p.shape(pl)
    p.tone(plm, tones(s["trim"] if s["trim"] != "gold" else s["front"]), "terminator")
    p.tone(plm - plm.shift(0, 3), ft, "light")
    p.stroke(Path.polyline([(xa - 8, ground - 30), (xb + 8, ground - 30)]), 1.2, ft, "deep")
    if s.get("moss") and roots_ >= 0:
        mt = tones(s["moss"])
        moss = p.nothing()
        for k in range(4):
            mx = xa + (xb - xa) * hash01(k, 1, seed + 9)
            moss = moss | p.shape(Path.ellipse(mx, ground - 4, 30 + 30 * hash01(k, 2, seed + 9), 12 + 8 * hash01(k, 3, seed + 9)))
        moss = moss & p.covered()
        p.tone(moss, mt, "base")
        p.tone(moss - moss.shift(-1.5, -2), mt, "shadow", alpha=0.6)
    p.ground_shadow(xa, xb, ground + 2, 8, 0.25)
    layer.place(p, reach=1.6)
    geo = {"x0": ax0, "x1": ax1, "cx": cx, "spring": spring, "rise": rise, "ar": ar, "arch": arch, "top": top, "ground": ground}
    if cobwebs and s.get("moss"):
        cobweb(layer, ax0 + 2, spring - rise * 0.25 + 4, 46, "left", seed + 11)
    if content is not None:
        content(layer, geo)
    if roots_:
        roots(layer, xa + 20, xb - 20, top + 34, seed + 13, depth=120 * roots_, n=int(2 + 4 * roots_))
    return geo


def pier(layer, x, ground, style, w=64, top=0, seed=0, capital_y=None, warm=None):
    """A square pier from the floor to the cornice: a moulded base, the shaft in dressed stone lit on
    its left and shadowed on its right, a band of gold (in the vault), and a capital the arches
    spring from; `warm` (y) washes it with a torch's light round there (a sconce on it)."""
    s = style
    p = Piece(x - w / 2 - 14, top, x + w / 2 + 14, ground + 8, ss=3, pad=4)
    pt = tones(s["pier"])
    shaft = Path.rect(x - w / 2, top + 40, w, ground - top - 40)
    m = p.shape(shaft)
    ashlar(p, m, s["pier"], seed, course=s["course"] * 1.4, block=w * 1.2, tint=0.12)
    p.tone(p.where(m, lambda xx, yy: xx > x + w * 0.22), pt, "shadow")
    p.tone(p.where(m, lambda xx, yy: xx < x - w * 0.36), pt, "light")
    p.stroke(Path.polyline([(x - w * 0.12, top + 60), (x - w * 0.12, ground - 40)]), 1.2, pt, "terminator", alpha=0.7)
    base = Path.polygon([(x - w / 2 - 12, ground + 4), (x - w / 2 - 12, ground - 22), (x - w / 2 - 4, ground - 32), (x + w / 2 + 4, ground - 32),
                         (x + w / 2 + 12, ground - 22), (x + w / 2 + 12, ground + 4)])
    bm = p.fill(base, pt, "base")
    p.tone(p.where(bm, lambda xx, yy: xx > x + w * 0.3), pt, "shadow")
    p.tone(bm - bm.shift(0, 3), pt, "light")
    p.edge(base, pt, 1.1)
    cy = capital_y if capital_y is not None else ground - (ground - top) * 0.6
    cap = Path.polygon([(x - w / 2 - 14, cy - 18), (x + w / 2 + 14, cy - 18), (x + w / 2 + 14, cy - 6), (x + w / 2 + 4, cy + 4), (x - w / 2 - 4, cy + 4), (x - w / 2 - 14, cy - 6)])
    capm = p.fill(cap, pt, "base-light")
    p.tone(p.below(capm, cy - 6), pt, "shadow")
    p.tone(p.where(capm, lambda xx, yy: xx > x + w * 0.35), pt, "shadow")
    p.edge(cap, pt, 1.1)
    if s["trim"] == "gold":
        g = tones("gold")
        for yy in (cy + 14, ground - 44):
            band = Path.rect(x - w / 2, yy, w, 6)
            bm2 = p.fill(band, g, "base")
            p.tone(p.where(bm2, lambda xx, y_: xx > x + w * 0.22), g, "shadow")
        P._icon(p, "sun", x, (cy + ground) / 2 - 20, 26)
    p.edge(shaft, pt, 1.2)
    if warm is not None:
        p.glow(x, warm, 120, s["glow"], 0.16)
    layer.place(p)
    return p


def hall(r, piers, style, seed, top=40, spring=None, pier_w=64, kinds=None, contents=None, opens=(), roots_=None, warm=()):
    """A whole hall wall: a bay between each pair of pier xs, then the piers over their joins.
    `contents` {bay index: painter(layer, geo)}, `opens` the bays left open on the far layer,
    `roots_` {bay index: how many roots hang from its cornice}, `warm` the xs of sconces whose light
    should warm the wall."""
    mid = r["mid"]
    g = r.ground
    spring = spring if spring is not None else g - 250
    geos = []
    for i, (xa, xb) in enumerate(zip(piers, piers[1:])):
        geos.append(bay(mid, xa, xb, g, style, seed + 17 * i, spring=spring, top=top, pier_w=pier_w,
                        content=(contents or {}).get(i), open_=i in opens, roots_=(roots_ or {}).get(i, 0.0)))
    for i, x in enumerate(piers):
        w_ = None
        for sx in warm:
            if abs(sx - x) < pier_w:
                w_ = 520
        pier(mid, x, g, style, pier_w, top, seed + 31 * i, capital_y=spring, warm=w_)
    for sx in warm:
        mid.glow(sx, 500, 220, style["glow"], 0.12)
    return geos


# --------------------------------------------------------------------------- #
# What goes in a bay
# --------------------------------------------------------------------------- #

def stone_door(layer, cx, ground, w=130, h=200, style=BARROW, seed=0, carving="spiral", frame=24):
    """A great stone door in an arched frame: a single slab carved with a sun or with spirals, set
    back in its frame, with an iron ring; the frame of big blocks. Returns the doorway (x, y, w, h)."""
    s = style
    p = Piece(cx - w / 2 - frame - 20, ground - h - frame - 30, cx + w / 2 + frame + 20, ground + 8)
    ft = tones(s["front"])
    dt = tones(s["door"])
    fr = P._opening("arch", cx - w / 2 - frame, ground - h - frame, w + 2 * frame, h + frame + 4)
    fm = p.fill(fr, ft, "base-light")
    n = 9
    r0 = w / 2
    cyy = ground - h + r0
    for k in range(1, n):
        a = math.radians(180 + 180 * k / n)
        p.stroke(Path.polyline([(cx + math.cos(a) * r0, cyy + math.sin(a) * r0), (cx + math.cos(a) * (r0 + frame), cyy + math.sin(a) * (r0 + frame))]), 1.1, ft, "shadow", clip=fm)
    for j in range(int((h - r0) / 34)):
        yy = ground - (j + 1) * 34
        p.stroke(Path.polyline([(cx - w / 2 - frame, yy), (cx - w / 2, yy)]), 1.1, ft, "shadow", clip=fm)
        p.stroke(Path.polyline([(cx + w / 2, yy), (cx + w / 2 + frame, yy)]), 1.1, ft, "shadow", clip=fm)
    p.edge(fr, ft, 1.3, "terminator")
    op = P._opening("arch", cx - w / 2, ground - h, w, h + 4)
    om = p.fill(op, dt, "base")
    p.tone(om - om.shift(6, 7), dt, "deep-shadow")
    p.tone(p.where(om, lambda xx, yy: xx > cx + w * 0.3), dt, "shadow", alpha=0.6)
    if carving == "sun":
        cyc = ground - h * 0.6
        g = tones("gold") if s["trim"] == "gold" else dt
        for k in range(12):
            a = math.radians(k * 30)
            ray = Path.polygon([(cx + math.cos(a) * 40, cyc + math.sin(a) * 40), (cx + math.cos(a + 0.14) * 20, cyc + math.sin(a + 0.14) * 20),
                                (cx + math.cos(a - 0.14) * 20, cyc + math.sin(a - 0.14) * 20)])
            p.fill(ray, g, "light" if k % 2 else "base", clip=om)
        sm = p.fill(Path.circle(cx, cyc, 18), g, "base-light", clip=om)
        p.edge(Path.circle(cx, cyc, 18), g, 1.0, "shadow")
        del sm
    else:
        for k, (sx, sy) in enumerate(((cx - 22, ground - h * 0.62), (cx + 22, ground - h * 0.62), (cx, ground - h * 0.42))):
            sp = Path()
            for j in range(24):
                a = j * 0.55
                rr = 1.5 + j * 0.75
                pt = (sx + math.cos(a) * rr, sy + math.sin(a) * rr)
                if j == 0:
                    sp.move(*pt)
                else:
                    sp.line(*pt)
            p.stroke(sp, 2.0, dt, "deep", clip=om)
            p.stroke(sp.translate(-1, -1), 0.8, dt, "light", clip=om, alpha=0.7)
    p.stroke(Path.polyline([(cx - w / 2 + 10, ground - 20), (cx + w / 2 - 10, ground - 20)]), 1.4, dt, "deep", clip=om)
    p.stroke(Path.circle(cx + w * 0.3, ground - h * 0.4, 7), 2.4, tones("iron"), "base")
    p.edge(op, dt, 1.6)
    p.ground_shadow(cx - w / 2 - frame, cx + w / 2 + frame, ground + 2, 8, 0.3)
    layer.place(p)
    return (cx - w / 2, ground - h, w, h)


def vault_door(layer, cx, ground, w=170, h=240, seed=0, emblem="sun", frame=30, colour="#c9973f"):
    """The Old Crown's vault doors: two tall leaves of gilded bronze under a round arch, banded and
    studded, an emblem (the sun, or the crown) across the meeting of the leaves, ring pulls, and a
    frame of gold-trimmed stone. Returns the doorway (x, y, w, h)."""
    p = Piece(cx - w / 2 - frame - 20, ground - h - frame - 40, cx + w / 2 + frame + 20, ground + 8)
    ft = tones(VAULT["front"])
    gt = tones("gold")
    bt = tones(colour)
    fr = P._opening("arch", cx - w / 2 - frame, ground - h - frame, w + 2 * frame, h + frame + 4)
    fm = p.fill(fr, ft, "base-light")
    p.tone(p.where(fm, lambda xx, yy: xx > cx + w / 2), ft, "base")
    p.stroke(Path().move(cx - w / 2 - frame + 6, ground).line(cx - w / 2 - frame + 6, ground - h + w / 2)
             .arc(cx, ground - h + w / 2, w / 2 + frame - 6, 180, 360).line(cx + w / 2 + frame - 6, ground), 2.6, gt, "base")
    p.edge(fr, ft, 1.3, "terminator")
    op = P._opening("arch", cx - w / 2, ground - h, w, h + 4)
    om = p.fill(op, bt, "base")
    p.tone(p.where(om, lambda xx, yy: xx > cx), bt, "shadow")
    p.tone(p.where(om, lambda xx, yy: xx < cx - w * 0.38), bt, "light")
    for yy in (ground - h * 0.18, ground - h * 0.5, ground - h * 0.8):
        band = p.shape(Path.rect(cx - w / 2, yy, w, 9)) & om
        p.tone(band, gt, "base")
        p.tone(band - band.shift(0, 2), gt, "light")
        for k in range(int(w / 16)):
            sx = cx - w / 2 + 8 + k * 16
            if abs(sx - cx) > 6:
                p.fill(Path.circle(sx, yy + 4.5, 2.2), gt, "highlight", clip=om)
    p.stroke(Path.polyline([(cx, ground - h), (cx, ground)]), 3.0, bt, "line", clip=om)
    p.tone(om - om.shift(5, 6), bt, "deep")
    ey = ground - h * 0.62
    if emblem == "crown":
        crown = Path.polygon([(cx - 34, ey + 16), (cx - 38, ey - 16), (cx - 18, ey), (cx, ey - 26), (cx + 18, ey), (cx + 38, ey - 16), (cx + 34, ey + 16)])
        cm = p.fill(crown, gt, "base")
        p.tone(p.where(cm, lambda xx, yy: xx > cx), gt, "shadow", alpha=0.6)
        p.tone(cm - cm.shift(1.5, 1.5), gt, "light")
        for gx_, gc in ((cx - 18, "#e2534f"), (cx, "#3f8fd0"), (cx + 18, "#e2534f")):
            p.fill(Path.circle(gx_, ey + 8, 3.6), tones(gc), "base")
        p.edge(crown, gt, 1.1, "shadow")
    else:
        disc = Path.circle(cx, ey, 30)
        for k in range(12):
            a = math.radians(k * 30)
            ray = Path.polygon([(cx + math.cos(a) * 48, ey + math.sin(a) * 48), (cx + math.cos(a + 0.16) * 26, ey + math.sin(a + 0.16) * 26),
                                (cx + math.cos(a - 0.16) * 26, ey + math.sin(a - 0.16) * 26)])
            p.fill(ray, gt, "light" if k % 2 else "base")
        dm = p.fill(disc, gt, "base-light")
        p.tone(p.where(dm, lambda xx, yy: xx > cx + 6), gt, "base")
        p.edge(disc, gt, 1.2, "shadow")
    for sx in (cx - 20, cx + 20):
        p.stroke(Path.circle(sx, ground - h * 0.36, 8), 2.6, gt, "base")
        p.fill(Path.circle(sx, ground - h * 0.36 - 8, 3), gt, "light")
    p.edge(op, bt, 1.6)
    p.ground_shadow(cx - w / 2 - frame, cx + w / 2 + frame, ground + 2, 8, 0.3)
    layer.place(p)
    return (cx - w / 2, ground - h, w, h)


def stair_up(layer, cx, ground, w=170, h=300, style=VAULT, seed=0, steps=9):
    """A stair going up out of the room, seen side on through a tall arch: steps climbing from the
    floor toward the back left, the daylight falling down them from above. Returns the doorway
    (the stair's foot)."""
    s = style
    p = Piece(cx - w / 2 - 30, ground - h - 30, cx + w / 2 + 30, ground + 8)
    ft = tones(s["front"])
    fr = P._opening("arch", cx - w / 2 - 22, ground - h - 22, w + 44, h + 26)
    fm = p.fill(fr, ft, "base-light")
    if s["trim"] == "gold":
        p.stroke(Path().move(cx - w / 2 - 14, ground).line(cx - w / 2 - 14, ground - h + w / 2).arc(cx, ground - h + w / 2, w / 2 + 14, 180, 360)
                 .line(cx + w / 2 + 14, ground), 2.4, tones("gold"), "base")
    p.edge(fr, ft, 1.3, "terminator")
    op = P._opening("arch", cx - w / 2, ground - h, w, h + 4)
    sky = tones("#fff1cf")
    om = p.fill(op, sky, "base")
    st = tones(s["wall"])
    rise = (h - 60) / steps
    for k in range(steps):
        y = ground - k * rise
        x0 = cx + w / 2 - (k + 1) * (w / steps)
        tread = Path.polygon([(x0 - 40, y - rise), (cx + w / 2 + 10, y - rise), (cx + w / 2 + 10, y + 4), (x0 - 40, y + 4)])
        tm = p.fill(tread, st, "base", clip=om)
        p.tone(p.shape(Path.rect(x0 - 40, y - rise, w + 60, 5)) & om, st, "light")
        p.tone(p.where(tm, lambda xx, yy: xx > cx + w * 0.25), st, "shadow", alpha=0.6)
        del tm
    shaft = p.shape(Path.polygon([(cx - w * 0.5, ground - h), (cx - w * 0.05, ground - h), (cx + w * 0.4, ground), (cx - w * 0.1, ground)])) & om
    p.tone(shaft, sky, "highlight", alpha=0.35)
    p.tone(om - om.shift(6, 0), st, "deep-shadow", alpha=0.7)
    p.glow(cx - w * 0.2, ground - h * 0.8, w * 1.2, "#fff2d0", 0.25)
    p.edge(op, st, 1.4)
    layer.place(p)
    return (cx - w / 2 + 6, ground - 118, w - 12, 118)


def niches(layer, x0, x1, y0, rows=3, row_h=70, style=BARROW, seed=0, things=("skull", "urn", "bones", "skull", "candle")):
    """Rows of burial niches cut in a wall: small round-headed recesses in the deep tone, each with a
    skull, a pot, a heap of bones or a guttered candle in it, a ledge under each row."""
    p = Piece(x0 - 10, y0 - 10, x1 + 10, y0 + rows * row_h + 10)
    rt = tones(style["recess"])
    ft = tones(style["front"])
    n = max(1, int((x1 - x0) / 62))
    for j in range(rows):
        y = y0 + j * row_h
        ledge = Path.rect(x0, y + row_h - 14, x1 - x0, 8, r=1.5)
        for i in range(n):
            nx = x0 + (i + 0.5) * (x1 - x0) / n
            nw, nh = 44, row_h - 22
            op = P._opening("arch", nx - nw / 2, y + 6, nw, nh)
            om = p.fill(op, rt, "deep")
            p.tone(om - om.shift(0, 8), rt, "line", alpha=0.6)
            kind = pick(things, i, j, seed)
            gy = y + 6 + nh
            if kind == "skull":
                bt = tones("#e8dcc4")
                sk = Path.circle(nx, gy - 11, 9)
                p.fill(sk, bt, "base")
                p.fill(Path.rect(nx - 5, gy - 6, 10, 6, r=1.5), bt, "shadow")
                for ex in (nx - 3.5, nx + 3.5):
                    p.fill(Path.circle(ex, gy - 11, 2.3), rt, "line")
                p.edge(sk, bt, 0.9)
            elif kind == "urn":
                ut = tones(pick(("terracotta", "pottery", "#8fb0c8"), i, j + 3, seed))
                urn = Path().move(nx - 6, gy - 26).line(nx + 6, gy - 26).quad(nx + 14, gy - 16, nx + 7, gy).line(nx - 7, gy).quad(nx - 14, gy - 16, nx - 6, gy - 26).close()
                um = p.fill(urn, ut, "base")
                p.tone(p.where(um, lambda xx, yy, a=nx: xx > a + 2), ut, "shadow")
                p.edge(urn, ut, 0.9)
            elif kind == "bones":
                bt = tones("#e8dcc4")
                for k in range(3):
                    p.stroke(Path.polyline([(nx - 14 + k * 4, gy - 3 - k * 3), (nx + 12 - k * 2, gy - 6 + k)]), 3.0, bt, "base")
            elif kind == "candle":
                ct = tones("cloth.cream")
                p.fill(Path.rect(nx - 3, gy - 18, 6, 18), ct, "base")
                p.fill(Path.ellipse(nx, gy - 22, 2.2, 4.5), tones("#ffcf6a"), "light")
            p.edge(op, ft, 1.2, "shadow")
        lm = p.fill(ledge, ft, "base-light")
        p.tone(lm - lm.shift(0, 2), ft, "light")
        p.edge(ledge, ft, 0.9, "shadow")
    layer.place(p, reach=1.2)
    return p


def sarcophagus(layer, cx, ground, w=260, h=110, style=BARROW, seed=0, effigy=True):
    """A stone sarcophagus on a low dais, its side carved in panels, its lid with a knight's effigy
    lying on it (a helm, folded hands on a sword), lit top left."""
    p = Piece(cx - w / 2 - 30, ground - h - 50, cx + w / 2 + 30, ground + 8)
    st = tones(style["pier"])
    dais = Path.rect(cx - w / 2 - 22, ground - 16, w + 44, 20, r=2)
    dm = p.fill(dais, st, "terminator")
    p.tone(dm - dm.shift(0, 3), st, "light")
    p.edge(dais, st, 1.1)
    box = Path.rect(cx - w / 2, ground - h, w, h - 14)
    bm = p.fill(box, st, "base")
    p.tone(p.where(bm, lambda xx, yy: xx > cx + w * 0.3), st, "shadow")
    for k in range(4):
        px_ = cx - w / 2 + 14 + k * (w - 28) / 4
        panel = Path.rect(px_ + 4, ground - h + 22, (w - 28) / 4 - 8, h - 54, r=3)
        pm = p.shape(panel) & bm
        p.tone(pm, st, "terminator")
        p.tone(pm - pm.shift(2, 2), st, "deep-shadow")
        p.tone(pm - pm.shift(-2, -2), st, "light")
    p.edge(box, st, 1.2)
    lid = Path.rect(cx - w / 2 - 10, ground - h - 14, w + 20, 18, r=3)
    lm = p.fill(lid, st, "base-light")
    p.tone(lm - lm.shift(0, 3), st, "light")
    p.edge(lid, st, 1.2)
    if effigy:
        et = tones(style["front"])
        body = Path.smooth([(cx - w * 0.38, ground - h - 14), (cx - w * 0.36, ground - h - 34), (cx - w * 0.1, ground - h - 40), (cx + w * 0.2, ground - h - 36),
                            (cx + w * 0.36, ground - h - 26), (cx + w * 0.4, ground - h - 14)], closed=True)
        em = p.fill(body, et, "base")
        p.tone(em - em.shift(0, 4), et, "light")
        helm = Path.circle(cx - w * 0.36, ground - h - 30, 14)
        p.fill(helm, et, "base-light")
        p.edge(helm, et, 1.0)
        p.stroke(Path.polyline([(cx - w * 0.2, ground - h - 40), (cx + w * 0.3, ground - h - 32)]), 3.0, et, "shadow")
        p.edge(body, et, 1.1)
    p.ground_shadow(cx - w * 0.6, cx + w * 0.6, ground + 2, 9, 0.32)
    layer.place(p)
    return p


def relief(layer, cx, cy, r=70, style=BARROW, seed=0, kind="sun"):
    """A carved roundel in the wall: the sun with its rays, or a ring of spirals, in low relief,
    lit on its upper left edges."""
    p = Piece(cx - r - 20, cy - r - 20, cx + r + 20, cy + r + 20)
    st = tones(style["front"])
    ring = Path.circle(cx, cy, r)
    rm = p.fill(ring, st, "base")
    p.tone(rm - p.shape(Path.circle(cx + 3, cy + 3, r - 2)), st, "light")
    p.tone(rm - p.shape(Path.circle(cx - 3, cy - 3, r - 2)), st, "shadow")
    inner = Path.circle(cx, cy, r - 12)
    im = p.fill(inner, st, "terminator")
    gold = style["trim"] == "gold"
    g = tones("gold") if gold else st
    if kind == "sun":
        for k in range(16):
            a = math.radians(k * 22.5)
            ray = Path.polygon([(cx + math.cos(a) * (r - 16), cy + math.sin(a) * (r - 16)), (cx + math.cos(a + 0.12) * r * 0.4, cy + math.sin(a + 0.12) * r * 0.4),
                                (cx + math.cos(a - 0.12) * r * 0.4, cy + math.sin(a - 0.12) * r * 0.4)])
            p.fill(ray, g, "base-light" if k % 2 else "base")
        dm = p.fill(Path.circle(cx, cy, r * 0.36), g, "base-light")
        p.tone(dm - dm.shift(2, 2), g, "light")
        p.edge(Path.circle(cx, cy, r * 0.36), g, 1.1, "shadow")
    else:
        for k in range(3):
            a = math.radians(-90 + k * 120)
            sx, sy = cx + math.cos(a) * r * 0.36, cy + math.sin(a) * r * 0.36
            sp = Path()
            for j in range(26):
                aa = j * 0.5 + k
                rr = 1.5 + j * 0.85
                pt = (sx + math.cos(aa) * rr, sy + math.sin(aa) * rr)
                if j == 0:
                    sp.move(*pt)
                else:
                    sp.line(*pt)
            p.stroke(sp, 2.6, st, "deep", clip=im)
            p.stroke(sp.translate(-1, -1), 1.0, st, "light", clip=im, alpha=0.8)
    p.edge(ring, st, 1.3)
    layer.place(p, reach=1.4)
    return p


RELICS = ("crown", "goblet", "orb", "book", "sword", "plate", "jar", "casket")


def relic_shelf(layer, x0, x1, y0, rows=3, row_h=78, seed=0, things=RELICS, back="#7a5a48"):
    """Shelves of relics built into a recess: dark wood shelves on brackets, and on them crowns,
    goblets, orbs, old books, a sword on its rack, plates and caskets, gold catching the light."""
    p = Piece(x0 - 10, y0 - 20, x1 + 10, y0 + rows * row_h + 12)
    wood = tones("wood.dark")
    bt = tones(back)
    bk = Path.rect(x0, y0, x1 - x0, rows * row_h)
    bm = p.fill(bk, bt, "shadow")
    for k in range(int((x1 - x0) / 22)):
        p.stroke(Path.polyline([(x0 + k * 22 + 11, y0), (x0 + k * 22 + 11, y0 + rows * row_h)]), 1.0, bt, "deep", clip=bm)
    p.tone(bm - bm.shift(0, 10), bt, "deep")
    g = tones("gold")
    for j in range(rows):
        sy = y0 + (j + 1) * row_h - 8
        x = x0 + 12
        i = 0
        while x < x1 - 30:
            kind = pick(things, i, j, seed)
            if kind == "crown":
                c = Path.polygon([(x, sy), (x - 1, sy - 16), (x + 7, sy - 8), (x + 13, sy - 20), (x + 19, sy - 8), (x + 27, sy - 16), (x + 26, sy)])
                cm = p.fill(c, g, "base")
                p.tone(cm - cm.shift(1, 1), g, "light")
                p.fill(Path.circle(x + 13, sy - 6, 2.4), tones("#e2534f"), "base")
                p.edge(c, g, 0.9, "shadow")
                x += 36
            elif kind == "goblet":
                gb = Path().move(x, sy - 30).line(x + 18, sy - 30).quad(x + 18, sy - 16, x + 10, sy - 14).line(x + 10, sy - 4).line(x + 16, sy).line(x + 2, sy) \
                    .line(x + 8, sy - 4).line(x + 8, sy - 14).quad(x, sy - 16, x, sy - 30).close()
                gm = p.fill(gb, g, "base")
                p.tone(p.where(gm, lambda xx, yy, a=x: xx > a + 11), g, "shadow")
                p.edge(gb, g, 0.9, "shadow")
                x += 28
            elif kind == "orb":
                om = p.fill(Path.circle(x + 12, sy - 13, 12), tones("#6fb4e0"), "base")
                p.tone(om - om.shift(-2, -2), tones("#6fb4e0"), "shadow")
                p.fill(Path.circle(x + 8, sy - 17, 3), tones("#6fb4e0"), "highlight")
                p.fill(Path.rect(x + 4, sy - 3, 16, 3), g, "base")
                p.edge(Path.circle(x + 12, sy - 13, 12), tones("#6fb4e0"), 1.0)
                p.glow(x + 12, sy - 13, 30, "#9fe0ff", 0.18)
                x += 34
            elif kind == "book":
                for k in range(3 + int(hash01(i, j, seed + 3) * 3)):
                    bh = 22 + 8 * hash01(i * 7 + k, j, seed + 4)
                    bc = tones(pick(("cloth.red", "cloth.navy", "cloth.teal", "cloth.plum", "#7a5a3a"), i * 7 + k, j, seed + 5))
                    bp = Path.rect(x, sy - bh, 8, bh)
                    p.fill(bp, bc, "base")
                    p.fill(Path.rect(x, sy - bh + 4, 8, 2), g, "base")
                    p.edge(bp, bc, 0.8)
                    x += 8.5
                x += 8
            elif kind == "sword":
                p.stroke(Path.polyline([(x, sy - 8), (x + 56, sy - 30)]), 3.4, tones("#c9cfe0"), "light")
                p.stroke(Path.polyline([(x + 44, sy - 36), (x + 52, sy - 18)]), 3.0, g, "base")
                p.stroke(Path.polyline([(x + 52, sy - 27), (x + 62, sy - 31)]), 3.0, tones("wood.dark"), "base")
                x += 70
            elif kind == "plate":
                pm = p.fill(Path.ellipse(x + 14, sy - 15, 14, 15), g, "base")
                p.tone(pm - p.shape(Path.ellipse(x + 14, sy - 15, 9, 10)), g, "shadow")
                p.edge(Path.ellipse(x + 14, sy - 15, 14, 15), g, 0.9, "shadow")
                x += 34
            elif kind == "jar":
                jt = tones(pick(("pottery", "terracotta", "#7fbf9f"), i, j + 2, seed))
                jp = Path.rect(x, sy - 24, 16, 24, r=4)
                p.fill(jp, jt, "base")
                p.fill(Path.rect(x + 2, sy - 28, 12, 5, r=1.5), tones("wood.dark"), "base")
                p.edge(jp, jt, 0.9)
                x += 24
            else:
                cp = Path.rect(x, sy - 20, 32, 20, r=2)
                ct = tones("#8a4a3a")
                cm = p.fill(cp, ct, "base")
                p.tone(p.above(cm, sy - 12), ct, "light")
                p.stroke(Path.polyline([(x, sy - 11), (x + 32, sy - 11)]), 2.0, g, "base")
                p.fill(Path.rect(x + 13, sy - 14, 6, 6), g, "light")
                p.edge(cp, ct, 0.9)
                x += 40
            x += 6
            i += 1
        shelf = Path.rect(x0, sy, x1 - x0, 8)
        sm = p.fill(shelf, wood, "base")
        p.tone(sm - sm.shift(0, 2), wood, "light")
        p.edge(shelf, wood, 1.0)
        for bx in (x0 + 14, x1 - 22):
            br = Path.polygon([(bx, sy + 8), (bx + 8, sy + 8), (bx + 8, sy + 22), (bx + 2, sy + 22)])
            p.fill(br, wood, "shadow")
    p.edge(bk, wood, 2.0, "base")
    layer.place(p, reach=1.4)
    return p


def treasure_heap(layer, cx, ground, w=240, h=100, seed=0, chest=True):
    """A heap of gold: coins piled in a mound, lit on top, with a goblet, a crown, gems and a
    sword's hilt sticking out of it, and an open chest spilling more."""
    p = Piece(cx - w / 2 - 30, ground - h - 70, cx + w / 2 + 30, ground + 8)
    g = tones("gold")
    if chest:
        chx = cx + w * 0.22
        body = Path.rect(chx - 40, ground - 56, 80, 50, r=3)
        ct = tones("#8a4a3a")
        cm = p.fill(body, ct, "base")
        p.tone(p.where(cm, lambda xx, yy: xx > chx + 20), ct, "shadow")
        for yy in (ground - 46, ground - 22):
            p.stroke(Path.polyline([(chx - 40, yy), (chx + 40, yy)]), 3.0, g, "base")
        p.edge(body, ct, 1.1)
        lid = Path.polygon([(chx - 42, ground - 56), (chx - 30, ground - 100), (chx + 40, ground - 104), (chx + 42, ground - 58)])
        lm = p.fill(lid, ct, "terminator")
        p.tone(lm - lm.shift(2, 2), ct, "light")
        p.edge(lid, ct, 1.0)
        spill = Path.smooth([(chx - 36, ground - 54), (chx - 10, ground - 70), (chx + 20, ground - 66), (chx + 38, ground - 54)], closed=True)
        p.fill(spill, g, "light")
    mound = Path.smooth([(cx - w / 2, ground + 3), (cx - w * 0.36, ground - h * 0.45), (cx - w * 0.1, ground - h * 0.95), (cx + w * 0.08, ground - h),
                         (cx + w * 0.3, ground - h * 0.6), (cx + w / 2, ground + 3)])
    mound.close()
    m = p.fill(mound, g, "base")
    p.tone(m - p.shape(Path.ellipse(cx - w * 0.1, ground - h * 0.45, w * 0.46, h * 0.7)), g, "shadow")
    p.tone(m & p.shape(Path.ellipse(cx - w * 0.12, ground - h * 0.7, w * 0.2, h * 0.24)), g, "light")
    for i in range(int(w * h / 90)):
        a = hash01(i, 1, seed)
        x = cx - w * 0.45 + w * 0.9 * a
        y = ground - h * 0.95 * hash01(i, 2, seed) ** 0.7
        coin = Path.ellipse(x, y, 5.5, 2.6, rot=(hash01(i, 3, seed) - 0.5) * 50)
        cm = p.fill(coin, g, "base-light" if hash01(i, 4, seed) < 0.5 else "terminator", clip=m)
        p.stroke(Path.ellipse(x, y, 5.5, 2.6, rot=(hash01(i, 3, seed) - 0.5) * 50), 0.7, g, "shadow", clip=m)
        del cm
    for i, gc in enumerate(("#e2534f", "#3f8fd0", "#4fbf7a", "#a87cda")):
        gx = cx - w * 0.3 + w * 0.6 * hash01(i, 5, seed)
        gy = ground - h * 0.6 * hash01(i, 6, seed) - 6
        gem = Path.polygon([(gx, gy - 6), (gx + 5, gy), (gx, gy + 6), (gx - 5, gy)])
        p.fill(gem, tones(gc), "base")
        p.fill(Path.polygon([(gx, gy - 6), (gx + 2, gy - 1), (gx - 2, gy - 1)]), tones(gc), "highlight")
    gb = Path().move(cx - 30, ground - h - 22).line(cx - 12, ground - h - 22).quad(cx - 12, ground - h - 8, cx - 20, ground - h - 6).line(cx - 20, ground - h + 8) \
        .line(cx - 22, ground - h + 8).line(cx - 22, ground - h - 6).quad(cx - 30, ground - h - 8, cx - 30, ground - h - 22).close()
    p.fill(gb, g, "base-light")
    p.edge(gb, g, 0.9, "shadow")
    p.stroke(Path.polyline([(cx + w * 0.16, ground - h * 0.5), (cx + w * 0.26, ground - h - 24)]), 3.0, tones("#c9cfe0"), "light")
    p.stroke(Path.polyline([(cx + w * 0.22, ground - h - 14), (cx + w * 0.32, ground - h - 10)]), 3.0, g, "base")
    p.edge(mound, g, 1.3)
    p.glow(cx, ground - h * 0.5, w * 0.7, "#ffd27a", 0.16)
    p.ground_shadow(cx - w * 0.55, cx + w * 0.55, ground + 2, 9, 0.3)
    layer.place(p)
    return p


def cobweb(layer, x, y, r=46, corner="left", seed=0):
    """A cobweb strung across a corner: spokes out from it and sagging threads between them."""
    p = Piece(x - r - 6, y - 6, x + r + 6, y + r + 6, pad=2)
    t = tones("#e8e4f0")
    side = 1 if corner == "left" else -1
    spokes = [math.radians(a) for a in (0, 22, 45, 68, 90)]
    for a in spokes:
        p.stroke(Path.polyline([(x, y), (x + side * math.cos(a) * r, y + math.sin(a) * r)]), 0.8, t, "base", alpha=0.75)
    for k in range(1, 5):
        rr = r * k / 5
        pts = []
        for a in spokes:
            pts.append((x + side * math.cos(a) * rr, y + math.sin(a) * rr))
        for (ax, ay), (bx, by) in zip(pts, pts[1:]):
            mx, my = (ax + bx) / 2 - side * 2, (ay + by) / 2 + 3
            p.stroke(Path().move(ax, ay).quad(mx, my, bx, by), 0.7, t, "base", alpha=0.7)
    layer.place(p, reach=0)
    return p


def roots(layer, x0, x1, top, seed=0, depth=120, n=4, colour="#5a4440", veins=0.0, ends=True):
    """Roots coming down through cracks in the ceiling: knotted twisting strands tapering as they
    hang, a few hair-roots off them, black veins with a violet glow if the Mirewood's sickness has
    reached them."""
    p = Piece(x0 - 40, top - 10, x1 + 40, top + depth + 40)
    t = tones(colour)
    for i in range(n):
        rx = x0 + (x1 - x0) * (i + 0.5) / n + (hash01(i, 1, seed) - 0.5) * (x1 - x0) / n * 0.6
        ln = depth * (0.5 + 0.5 * hash01(i, 2, seed))
        pts = [(rx, top - 4)]
        for k in range(1, 6):
            pts.append((rx + math.sin(k * 1.3 + i) * 10 * (k / 5), top + ln * k / 5))
        path = Path.smooth(pts)
        m = p.line_shape(path, (11 - 3 * (i % 2), 1.0))
        p.tone(m, t, "base")
        p.tone(m - m.shift(2, 1), t, "light")
        p.tone(m - m.shift(-2, -1), t, "shadow")
        for k in range(2):
            (hx, hy), (dx, dy) = path.at(0.3 + 0.35 * k)
            sd = 1 if (i + k) % 2 else -1
            twig = Path.smooth([(hx, hy), (hx + sd * 14, hy + 10), (hx + sd * 20, hy + 28)])
            p.tone(p.line_shape(twig, (3.4, 0.6)), t, "base")
        if veins:
            p.tone(p.line_shape(path, (2.4, 0.4)), tones("#2a1e30"), "base")
            p.tone(p.line_shape(path.translate(0.4, 0), (0.9, 0.2)), tones("#a46cf0"), "light")
    layer.place(p, reach=1.6)
    return p


def crystal_cluster(layer, x, y, size=1.0, colour="#8f7cf0", seed=0, glow=0.35, angle=0.0, n=5, piece=None):
    """A cluster of crystals growing out of rock at (x, y), pointing up (turned `angle` degrees):
    long six-sided points, each lit on its left facet and shadowed on its right, a bright edge down
    the middle, glowing in their own colour."""
    s = size
    p = piece or Piece(x - 70 * s, y - 110 * s, x + 70 * s, y + 20 * s)
    t = tones(colour)
    for i in range(n):
        u = (i + 0.5) / n - 0.5
        a = math.radians(angle + u * 80 + (hash01(i, 1, seed) - 0.5) * 16)
        ln = (50 + 46 * hash01(i, 2, seed)) * s * (1.1 - abs(u))
        wd = (8 + 6 * hash01(i, 3, seed)) * s
        bx, by = x + u * 36 * s, y
        dx, dy = math.sin(a), -math.cos(a)
        nx, ny = -dy, dx
        tip = (bx + dx * ln, by + dy * ln)
        sh = (bx + dx * ln * 0.78, by + dy * ln * 0.78)
        poly = [(bx + nx * wd, by + ny * wd), (sh[0] + nx * wd, sh[1] + ny * wd), tip, (sh[0] - nx * wd, sh[1] - ny * wd), (bx - nx * wd, by - ny * wd)]
        m = p.fill(Path.polygon(poly), t, "base")
        left = Path.polygon([(bx, by), (sh[0], sh[1]), tip, (sh[0] + nx * wd, sh[1] + ny * wd), (bx + nx * wd, by + ny * wd)])
        p.tone(p.shape(left) & m, t, "light")
        right = Path.polygon([(bx - nx * wd * 0.4, by - ny * wd * 0.4), (sh[0] - nx * wd * 0.4, sh[1] - ny * wd * 0.4), tip, (sh[0] - nx * wd, sh[1] - ny * wd),
                              (bx - nx * wd, by - ny * wd)])
        p.tone(p.shape(right) & m, t, "shadow")
        p.stroke(Path.polyline([(bx + nx * wd * 0.2, by + ny * wd * 0.2), (sh[0] + nx * wd * 0.2, sh[1] + ny * wd * 0.2)]), 1.4 * s, t, "highlight", clip=m)
        p.edge(Path.polygon(poly), t, 1.1, "deep")
    if glow:
        p.glow(x, y - 40 * s, 120 * s, colour, glow)
        p.glow(x, y - 30 * s, 40 * s, P.mixc(colour, "#ffffff", 0.6), glow)
    if piece is None:
        layer.place(p)
    return p


def fungus(layer, x, ground, n=5, colour="#5fe0c0", seed=0, size=1.0, glow=0.3, piece=None):
    """A clump of glowing fungus at a wall's foot: pale stalks and round caps lit from inside in a
    cold teal, little spots of light on them, and their glow."""
    s = size
    p = piece or Piece(x - 60 * s, ground - 70 * s, x + 60 * s, ground + 10)
    ct = tones(colour)
    st = tones("#d9e8e0")
    for i in range(n):
        u = (i + 0.5) / n - 0.5
        mx = x + u * 70 * s + (hash01(i, 1, seed) - 0.5) * 8 * s
        mh = (10 + 26 * hash01(i, 2, seed)) * s * (1.2 - abs(u))
        cr = (7 + 7 * hash01(i, 3, seed)) * s
        stalk = Path.rect(mx - 2.4 * s, ground - mh, 4.8 * s, mh + 2)
        p.fill(stalk, st, "base")
        p.edge(stalk, st, 0.8)
        cap = Path().move(mx - cr, ground - mh + 2).quad(mx - cr, ground - mh - cr * 0.9, mx, ground - mh - cr * 0.95) \
            .quad(mx + cr, ground - mh - cr * 0.9, mx + cr, ground - mh + 2).close()
        cm = p.fill(cap, ct, "base-light")
        p.tone(p.where(cm, lambda xx, yy, a=mx: xx > a + cr * 0.35), ct, "base")
        p.fill(Path.circle(mx - cr * 0.35, ground - mh - cr * 0.4, 1.6 * s), ct, "highlight", clip=cm)
        p.fill(Path.circle(mx + cr * 0.2, ground - mh - cr * 0.55, 1.2 * s), ct, "highlight", clip=cm)
        p.edge(cap, ct, 1.0, "deep")
    if glow:
        p.glow(x, ground - 20 * s, 100 * s, colour, glow)
    if piece is None:
        layer.place(p)
    return p


def _near(pts, x0, x1, margin=300):
    """The points of a line (sorted by x) that matter between x0 and x1: those within margin of
    it, and one more each side, so a line through a few far-apart points still spans it."""
    lo = 0
    while lo + 1 < len(pts) and pts[lo + 1][0] < x0 - margin:
        lo += 1
    hi = len(pts) - 1
    while hi - 1 >= 0 and pts[hi - 1][0] > x1 + margin:
        hi -= 1
    return pts[lo:hi + 1]


def rock_wall(layer, x0, x1, top_pts, ground, colour, seed=0, style="facets", cell=70, fade=0, ss=2, ink=P.INK, reach=P.OUTER,
              holes=(), extra=None):
    """A wall of rock from x0 to x1 standing on `ground` up to a ragged top line through `top_pts`
    (sorted by x; a line under the ceiling, or the sky's edge), painted as facets() or strata(),
    laid in chunks; `holes` ((x0, x1, path), ...) are left out (openings on the far layer); `extra`
    (piece, xa, xb) paints more on each chunk before it's laid in."""
    for xa, xb in chunks(x0, x1, 480):
        cx0, cx1 = xa - OVERLAP, xb + OVERLAP
        near = _near(top_pts, cx0, cx1)
        ymin = min(q[1] for q in near)
        p = Piece(cx0, max(-20, ymin - 20), cx1, ground + 10, ss=ss, pad=4)
        body = Path.smooth(near) if len(near) > 2 else Path.polyline(near)
        body.line(near[-1][0], ground + 10).line(near[0][0], ground + 10).close()
        region = p.shape(body)
        for hx0, hx1, hp in holes:
            if hx1 >= cx0 and hx0 <= cx1:
                region = region - p.shape(hp)
        if style == "strata":
            strata(p, region, colour, seed)
        else:
            facets(p, region, colour, seed, cell=cell)
        t = tones(colour)
        p.tone(region - region.shift(0, 6), t, "light", alpha=0.8)
        if extra is not None:
            extra(p, xa, xb, region)
        place_span(layer, p, xa, xb, ink=ink, reach=reach)


def warm_wash(layer, x, y, colour="#ffb35a", r=200, strength=0.14):
    """A soft warm light on a wall round a sconce or a brazier the game draws, so its pool of light
    reads as firelight."""
    layer.glow(x, y, r, colour, strength)


# --------------------------------------------------------------------------- #
# Recipes: the barrow
# --------------------------------------------------------------------------- #

def _barrow_back(r, seed, outside=None):
    """The barrow's sky and far layer: the black under the hill, and beyond the arches more of
    the barrow, dark, with a torch or two burning far off; `outside` (x0, x1) paints the daylight
    seen out of the barrow's mouth into the far layer there."""
    dark_sky(r["sky"], "#0b0912", "#1a1626", seed, motes=50)
    far = r["far"]
    openings = None
    if outside:
        openings = [(outside[0], outside[1], lambda layer, a, b: daylight_view(layer, a, b, seed + 3))]
    far_hall(far, r.ground - 10, BARROW, seed + 1, k=0.62, span=260, top=30, lights="torch", openings=openings)


def daylight_view(layer, x0, x1, seed=0, ground=600):
    """The day outside, seen from under the hill: a bright sky, the meadow's grass and a tree or
    two, all a little bleached by the light (the far layer keeps its own light, so it glows)."""
    p = Piece(x0, 0, x1, layer.h, ss=2, pad=0)
    sky = p.shape(Path.rect(x0, 0, x1 - x0, layer.h))
    v = np.clip((np.arange(p.c.cov.shape[0]) / p.c.ss) / 600.0, 0, 1)
    col = P.lerp_colour([(0.0, "#9cc3f2"), (0.6, "#dfe9fb"), (1.0, "#fff6e2")], v)
    p.c.rgb[:] = col[:, None, :]
    p.c.cov[:] = 1.0
    hill = Path.smooth([(x0 - 20, 470), (x0 + (x1 - x0) * 0.4, 440), (x1 + 20, 460)])
    hill.line(x1 + 20, layer.h).line(x0 - 20, layer.h).close()
    hm = p.fill(hill, tones("#9fd27a"), "base")
    p.tone(hm - hm.shift(0, 8), tones("#9fd27a"), "light")
    P._round_tree_small(p, x0 + (x1 - x0) * 0.3, 452, 40, tones("#7fbf6a"), seed)
    P._round_tree_small(p, x0 + (x1 - x0) * 0.75, 446, 30, tones("#8fc874"), seed + 1)
    layer.put(p.rgba_premultiplied(reach=0), p.ox, p.oy)
    layer.glow((x0 + x1) / 2, 480, (x1 - x0) * 1.2, "#fff2d0", 0.25)


def barrow_steps(r):
    """The Barrow Steps (12-rooms §6): the way down into the barrow; the bright day outside through
    the portal at the west end, big lintelled stones carved with spirals along the passage, roots
    through the cracks over it, burial niches, and at the east end an arch on the dark going down
    to the hall."""
    g = r.ground
    seed = roomkit.seed_of("barrow", "steps")
    _barrow_back(r, seed, outside=(40, 330))
    mid = r["mid"]
    s = BARROW
    # the portal: a hole in the rock showing the day, framed by two uprights and a lintel
    rock_top = [(-60, 0), (2000, 0)]
    portal = Path.smooth([(70, g + 12), (64, 420), (90, 330), (190, 296), (290, 330), (318, 420), (312, g + 12)], closed=True)
    rock_wall(mid, -40, 470, rock_top, g, "#857d94", seed + 1, cell=80, holes=[(60, 320, portal)])
    for ux in (62, 320):
        p = Piece(ux - 46, 300, ux + 46, g + 8)
        st = tones("#b9b2a8")
        up = Path.smooth([(ux - 34, g + 4), (ux - 36, 470), (ux - 30, 330), (ux + 28, 324), (ux + 36, 470), (ux + 32, g + 4)], closed=True)
        um = p.fill(up, st, "base")
        p.tone(p.where(um, lambda xx, yy, a=ux: xx > a + 8), st, "shadow")
        p.tone(p.where(um, lambda xx, yy, a=ux: xx < a - 18), st, "light")
        p.edge(up, st, 1.3)
        mid.place(p)
    p = Piece(10, 250, 380, 350)
    st = tones("#b9b2a8")
    lin = Path.smooth([(20, 330), (26, 282), (190, 266), (360, 280), (368, 328), (190, 340)], closed=True)
    lm = p.fill(lin, st, "base")
    p.tone(p.below(lm, 318), st, "shadow")
    p.tone(lm - lm.shift(0, 4), st, "light")
    p.edge(lin, st, 1.3)
    mid.place(p)
    # the passage: big stones under lintels, niches, a relief
    hall(r, [470, 860, 1250, 1560], s, seed + 10, top=60, spring=g - 270, pier_w=70,
         contents={0: lambda layer, geo: niches(layer, geo["x0"] + 30, geo["x1"] - 30, geo["spring"] - 40, 3, 74, s, seed + 20),
                   1: lambda layer, geo: relief(layer, geo["cx"], geo["spring"] + 20, 70, s, seed + 21, "spiral"),
                   2: lambda layer, geo: None},
         opens=(2,), roots_={0: 0.6, 1: 1.0, 2: 0.5}, warm=(1229,))
    rock_wall(mid, 1500, 1640, [(1450, 0), (1700, 0)], g, "#857d94", seed + 2, cell=80)
    ceiling_rock(mid, -40, 1640, 64, seed + 3)
    P.floor_shade(mid, g, 56, 0.34)
    fore = r["fore"]
    fore_rocks(fore, r.x("fore", 30), seed + 40)
    fore_rocks(fore, r.x("fore", 1560), seed + 41, flip=True)


def ceiling_rock(layer, x0, x1, y, seed=0, colour="#6e667e"):
    """The rock over a passage: a ragged underside along y with roots and drips in it, filled up to
    the top of the picture."""
    pts = []
    i = int(x0 // 60) - 1
    while i * 60 < x1 + 120:
        pts.append((i * 60, y + (hash01(i, 1, seed) - 0.3) * 30))
        i += 1
    for xa, xb in chunks(x0, x1, 480):
        cx0, cx1 = xa - OVERLAP, xb + OVERLAP
        p = Piece(cx0, -10, cx1, y + 40, ss=2, pad=4)
        near = [q for q in pts if cx0 - 120 <= q[0] <= cx1 + 120]
        body = Path.smooth(near)
        body.line(near[-1][0], -20).line(near[0][0], -20).close()
        reg = p.shape(body)
        facets(p, reg, colour, seed, cell=60, squash=0.6)
        t = tones(colour)
        p.tone(reg - reg.shift(0, -8), t, "deep-shadow", alpha=0.8)
        place_span(layer, p, xa, xb)


def fore_rocks(layer, x, seed=0, flip=False, h=170, dark=0.6, colour="#6e667e"):
    """A dark heap of fallen rock at the bottom corner of the picture, in front of everyone."""
    p = Piece(x - 160, CANVAS_H - h - 20, x + 160, CANVAS_H + 30, ss=2)
    t = haze(tones(colour), "#0e0a16", dark)
    sd = -1 if flip else 1
    pts = [(x - 150 * sd, CANVAS_H + 30), (x - 140 * sd, CANVAS_H - h * 0.4), (x - 90 * sd, CANVAS_H - h * 0.9), (x - 30 * sd, CANVAS_H - h),
           (x + 30 * sd, CANVAS_H - h * 0.6), (x + 90 * sd, CANVAS_H - h * 0.3), (x + 150 * sd, CANVAS_H + 30)]
    body = Path.polygon(pts if not flip else pts[::-1])
    m = p.fill(body, t, "base")
    facets(p, m, P.mixc(colour, "#0e0a16", dark), seed, cell=60)
    layer.place(p, reach=2.6)


def barrow_hall(r):
    """The Barrow Hall (12-rooms §6): a long hall of grey-violet stone, bays between square piers
    under round arches, burial niches with skulls and urns, carved roundels, roots hanging through
    the vault, cobwebs in the arches; the stone door to the side room on its door, the tomb door
    carved with the sun on its own, and bays open on the dark of the barrow going on beyond."""
    g = r.ground
    seed = roomkit.seed_of("barrow", "hall")
    _barrow_back(r, seed)
    mid = r["mid"]
    s = BARROW
    side_x = r.door_x("side")
    tomb_x = r.door_x("tomb")
    doors = {}

    def side_door(layer, geo):
        doors["side"] = stone_door(layer, side_x, g, 150, 220, s, seed + 30, carving="spiral")

    def tomb_door(layer, geo):
        doors["tomb"] = stone_door(layer, tomb_x, g, 112, 190, s, seed + 31, carving="sun", frame=22)
        relief(layer, tomb_x - 130, geo["spring"] + 30, 46, s, seed + 32, "sun")
        relief(layer, tomb_x + 130, geo["spring"] + 30, 46, s, seed + 33, "sun")

    def niche_bay(k):
        return lambda layer, geo: niches(layer, geo["x0"] + 26, geo["x1"] - 26, geo["spring"] - 60, 4, 72, s, seed + 40 + k)

    def roundel(k, kind):
        return lambda layer, geo: (relief(layer, geo["cx"], geo["spring"] + 40, 80, s, seed + 50 + k, kind),
                                   sarcophagus(layer, geo["cx"], g, 220, 100, s, seed + 60 + k))

    piers = [-60, 220, 620, 1180, 1560, 2040, 2520, 3000, 3270]
    contents = {0: niche_bay(0), 1: side_door, 2: niche_bay(2), 3: roundel(3, "spiral"), 4: tomb_door, 5: niche_bay(5), 7: niche_bay(7)}
    hall(r, piers, s, seed + 10, top=40, spring=g - 300, pier_w=72, contents=contents, opens=(6,),
         roots_={0: 0.5, 2: 0.8, 3: 1.0, 5: 0.6, 6: 0.9, 7: 0.4}, warm=(730, 1651, 2163, 2778))
    r.doorway("side", doors["side"])
    r.doorway("tomb", doors["tomb"])
    P.floor_shade(mid, g, 56, 0.34)
    fore = r["fore"]
    fore_rocks(fore, r.x("fore", 60), seed + 70)
    fore_rocks(fore, r.x("fore", 3150), seed + 71, flip=True)


def barrow_side(r):
    """The barrow's side room (12-rooms §6): a small burial chamber, its stone door back to the hall
    on the zone file's door, a sarcophagus with a knight's effigy on its dais under a carved
    roundel, walls of burial niches, and roots through the vault."""
    g = r.ground
    seed = roomkit.seed_of("barrow", "side-room")
    _barrow_back(r, seed)
    mid = r["mid"]
    s = BARROW
    out_x = r.door_x("side-out")
    doors = {}

    def door(layer, geo):
        doors["side-out"] = stone_door(layer, out_x, g, 150, 220, s, seed + 30, carving="spiral")

    def tomb(layer, geo):
        relief(layer, geo["cx"], geo["spring"] + 10, 90, s, seed + 31, "sun")
        sarcophagus(layer, geo["cx"], g, 300, 112, s, seed + 32)

    piers = [-40, 520, 1100, 1640]
    hall(r, piers, s, seed + 10, top=80, spring=g - 290, pier_w=72,
         contents={0: door, 1: tomb, 2: lambda layer, geo: niches(layer, geo["x0"] + 26, geo["x1"] - 26, geo["spring"] - 40, 4, 72, s, seed + 40)},
         roots_={0: 0.4, 1: 1.0, 2: 0.7}, warm=(1168,))
    ceiling_rock(mid, -40, 1640, 60, seed + 3)
    r.doorway("side-out", doors["side-out"])
    P.floor_shade(mid, g, 56, 0.34)
    fore = r["fore"]
    fore_rocks(fore, r.x("fore", 1540), seed + 70, flip=True)


# --------------------------------------------------------------------------- #
# Recipes: the Old Crown Vault
# --------------------------------------------------------------------------- #

def _vault_back(r, seed):
    """The vault's sky and far layer: the dark under the Sunspire, and beyond the arches the vault's
    other halls, their gold dim, braziers burning in them."""
    dark_sky(r["sky"], "#0e0a08", "#1e1810", seed, motes=40, mote="#c8a870")
    far_hall(r["far"], r.ground - 10, VAULT, seed + 1, k=0.62, span=300, top=30, lights="brazier")


def banner_on(layer, x, y, colour="cloth.navy", emblem="sun", w=46, h=150):
    """A long banner hung on the wall from a gilded rod (not a floor prop: one fixed to the stone)."""
    p = Piece(x - w / 2 - 16, y - 12, x + w / 2 + 16, y + h + 16)
    P.banner(x, y, w, h, colour, emblem)(p)
    layer.place(p)


def vault_entrance(r):
    """The Old Crown Vault's antechamber (12-rooms §6): cream stone dressed with gold under the
    Sunspire; at the west end the wall broken through where the sappers dug in, then the stair up
    to the terrace with daylight falling down it, banners of navy and gold, the Aurelian sun in
    the roundels, and the great vault doors to the long hall and to the treasury."""
    g = r.ground
    seed = roomkit.seed_of("old-crown-vault", "entrance")
    _vault_back(r, seed)
    mid = r["mid"]
    s = VAULT
    doors = {}
    sx, ox, tx = r.door_x("stair-up"), r.door_x("outer"), r.door_x("treasury")

    def breach(layer, geo):
        rock_wall(layer, geo["x0"] - 10, geo["x1"] + 10, [(geo["x0"] - 200, geo["spring"] - 60), (geo["cx"] - 60, geo["spring"] - 140),
                                                          (geo["cx"] + 80, geo["spring"] - 110), (geo["x1"] + 200, geo["spring"] - 40)],
                  g, "#8a7462", seed + 5, cell=50, holes=[(geo["cx"] - 90, geo["cx"] + 70, Path.smooth(
                      [(geo["cx"] - 96, g + 10), (geo["cx"] - 80, g - 110), (geo["cx"] - 20, g - 160), (geo["cx"] + 50, g - 120), (geo["cx"] + 70, g + 10)], closed=True))])

    def stair(layer, geo):
        doors["stair-up"] = stair_up(layer, sx, g, 160, 300, s, seed + 30)

    def outer(layer, geo):
        doors["outer"] = vault_door(layer, ox, g, 170, 250, seed + 31, "sun")

    def treasury(layer, geo):
        doors["treasury"] = vault_door(layer, tx, g, 170, 250, seed + 32, "crown")

    def roundel(layer, geo):
        relief(layer, geo["cx"], geo["spring"] + 10, 80, s, seed + 40, "sun")
        banner_on(layer, geo["x0"] + 70, geo["spring"] - 20)
        banner_on(layer, geo["x1"] - 70, geo["spring"] - 20, "cloth.red", "diamond")

    def shelves(layer, geo):
        relic_shelf(layer, geo["x0"] + 30, geo["x1"] - 30, geo["spring"] - 50, 3, 80, seed + 41)

    piers = [-60, 360, 960, 1300, 1700, 1960, 2360, 2700, 3100, 3270]
    contents = {0: breach, 1: shelves, 2: roundel, 3: stair, 5: outer, 6: roundel, 7: treasury}
    hall(r, piers, s, seed + 10, top=40, spring=g - 320, pier_w=72, contents=contents, opens=(0, 4, 8), warm=(413,))
    r.doorway("stair-up", doors["stair-up"])
    r.doorway("outer", doors["outer"])
    r.doorway("treasury", doors["treasury"])
    P.floor_shade(mid, g, 56, 0.32)
    fore = r["fore"]
    fore_rubble(fore, r.x("fore", 120), seed + 70)


def fore_rubble(layer, x, seed=0, dark=0.6):
    """Broken dressed stone fallen at the bottom of the picture."""
    p = Piece(x - 170, CANVAS_H - 150, x + 170, CANVAS_H + 30)
    t = haze(tones(VAULT["wall"]), "#120e0a", dark)
    for i in range(6):
        bx = x - 130 + 52 * i + (hash01(i, 1, seed) - 0.5) * 20
        bw = 50 + 30 * hash01(i, 2, seed)
        bh = 40 + 60 * hash01(i, 3, seed) * (1 - abs(i - 2.5) / 3)
        blk = Path.polygon([(bx - bw / 2, CANVAS_H + 30), (bx - bw / 2 + 6, CANVAS_H - bh), (bx + bw / 2, CANVAS_H - bh + 8), (bx + bw / 2 - 4, CANVAS_H + 30)])
        m = p.fill(blk, t, "base")
        p.tone(p.where(m, lambda xx, yy, a=bx: xx > a + bw * 0.2), t, "shadow")
        p.tone(m - m.shift(0, 4), t, "light")
        p.edge(blk, t, 1.1)
    layer.place(p, reach=2.6)


def vault_halls(r):
    """The Old Crown Vault's long hall (12-rooms §6): an arcade of gilded bays going on and on, the
    traps' slots in the walls, relics on shelves, banners and roundels of the sun, the vault doors
    back to the antechamber, and the Crown Door, gold, with the Old Crown across its leaves."""
    g = r.ground
    seed = roomkit.seed_of("old-crown-vault", "halls")
    _vault_back(r, seed)
    mid = r["mid"]
    s = VAULT
    doors = {}
    cx, bx = r.door_x("crown"), r.door_x("halls-out")

    def crown(layer, geo):
        doors["crown"] = vault_door(layer, cx, g, 190, 270, seed + 30, "crown", frame=34, colour="#d9a84a")
        banner_on(layer, cx - 190, geo["spring"] - 10)
        banner_on(layer, cx + 190, geo["spring"] - 10)

    def back(layer, geo):
        doors["halls-out"] = vault_door(layer, bx, g, 170, 250, seed + 31, "sun")

    def shelves(k):
        return lambda layer, geo: relic_shelf(layer, geo["x0"] + 30, geo["x1"] - 30, geo["spring"] - 40, 3, 80, seed + 40 + k)

    def roundel(k):
        return lambda layer, geo: (relief(layer, geo["cx"], geo["spring"] + 30, 80, s, seed + 50 + k, "sun"),
                                   banner_on(layer, geo["x0"] + 60, geo["spring"] - 20, "cloth.navy"),
                                   banner_on(layer, geo["x1"] - 60, geo["spring"] - 20, "cloth.navy"))

    piers = [-60, 360, 760, 1340, 1940, 2280, 2680, 3060, 3270]
    contents = {1: roundel(1), 2: shelves(2), 3: crown, 5: back, 6: shelves(6)}
    hall(r, piers, s, seed + 10, top=40, spring=g - 320, pier_w=72, contents=contents, opens=(0, 4, 7), warm=(936, 2347))
    r.doorway("crown", doors["crown"])
    r.doorway("halls-out", doors["halls-out"])
    P.floor_shade(mid, g, 56, 0.32)
    fore = r["fore"]
    fore_rubble(fore, r.x("fore", 200), seed + 70)


def vault_strongroom(r):
    """The Relic Gallery (12-rooms §6): the vault's long gallery of relics, walls of shelves and
    alcoves full of crowns, goblets, orbs and old books, and in the middle a great apse with the
    Aurelian sun in gold over the Sunheart's pedestal (the pedestal and the cases are the floor's),
    the Crown Door back to the hall at the west end."""
    g = r.ground
    seed = roomkit.seed_of("old-crown-vault", "strongroom")
    _vault_back(r, seed)
    mid = r["mid"]
    s = VAULT
    doors = {}
    dx = r.door_x("strongroom-out")

    def door(layer, geo):
        doors["strongroom-out"] = vault_door(layer, dx, g, 190, 270, seed + 30, "crown", frame=34, colour="#d9a84a")

    def shelves(k):
        return lambda layer, geo: relic_shelf(layer, geo["x0"] + 26, geo["x1"] - 26, geo["spring"] - 60, 4, 78, seed + 40 + k)

    def apse(layer, geo):
        relief(layer, geo["cx"], geo["spring"] - 10, 120, s, seed + 50, "sun")
        layer.glow(geo["cx"], geo["spring"] - 10, 260, "#ffd27a", 0.2)
        banner_on(layer, geo["x0"] + 70, geo["spring"] - 40, "cloth.navy", "sun", 52, 170)
        banner_on(layer, geo["x1"] - 70, geo["spring"] - 40, "cloth.navy", "sun", 52, 170)

    piers = [-60, 360, 820, 1260, 1940, 2380, 2840, 3270]
    contents = {0: shelves(0), 1: door, 2: shelves(2), 3: apse, 4: shelves(4), 5: shelves(5), 6: shelves(6)}
    hall(r, piers, s, seed + 10, top=40, spring=g - 330, pier_w=72, contents=contents, warm=(496, 2704))
    r.doorway("strongroom-out", doors["strongroom-out"])
    P.floor_shade(mid, g, 56, 0.32)
    fore = r["fore"]
    fore_rubble(fore, r.x("fore", 3100), seed + 70)


def vault_treasury(r):
    """The Old Crown's treasury (12-rooms §6): a gilded chamber whose alcoves are heaped with gold,
    chests spilling coins, crowns and goblets, shelves of plate, the treasury doors back to the
    antechamber on the zone file's door."""
    g = r.ground
    seed = roomkit.seed_of("old-crown-vault", "treasury")
    _vault_back(r, seed)
    mid = r["mid"]
    s = VAULT
    doors = {}
    dx = r.door_x("treasury-out")

    def door(layer, geo):
        doors["treasury-out"] = vault_door(layer, dx, g, 170, 250, seed + 30, "crown")

    def heap(k):
        def f(layer, geo):
            relic_shelf(layer, geo["x0"] + 30, geo["x1"] - 30, geo["spring"] - 40, 2, 80, seed + 40 + k, things=("crown", "goblet", "plate", "casket", "goblet", "orb"))
            treasure_heap(layer, geo["cx"], g - 2, (geo["x1"] - geo["x0"]) * 0.8, 110, seed + 50 + k)
        return f

    def sun(layer, geo):
        relief(layer, geo["cx"], geo["spring"] + 20, 90, s, seed + 60, "sun")
        treasure_heap(layer, geo["cx"], g - 2, (geo["x1"] - geo["x0"]) * 0.7, 90, seed + 61, chest=False)

    piers = [-60, 500, 960, 1420, 1660]
    hall(r, piers, s, seed + 10, top=40, spring=g - 320, pier_w=72, contents={0: door, 1: sun, 2: heap(2)}, warm=(1335,))
    r.doorway("treasury-out", doors["treasury-out"])
    P.floor_shade(mid, g, 56, 0.32)
    fore = r["fore"]
    fore_rubble(fore, r.x("fore", 1500), seed + 70)


RECIPES = {
    "barrow/steps": barrow_steps,
    "barrow/hall": barrow_hall,
    "barrow/side-room": barrow_side,
    "old-crown-vault/entrance": vault_entrance,
    "old-crown-vault/halls": vault_halls,
    "old-crown-vault/strongroom": vault_strongroom,
    "old-crown-vault/treasury": vault_treasury,
}
