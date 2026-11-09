"""
The floor props in the painted style: every prop a room stands on its floor (the zone files' prop
lines, and the looks of their chests, signs, levers and waypoints), redrawn as vectors with
paint.py, into Content/Props/Painted/<id>.png. The game draws a painted prop instead of the old
pixel one (Content/Props/<id>.png, which stays as it was for the placeholder painter).

    python tools/rooms/paint_props.py                     # every prop a room uses
    python tools/rooms/paint_props.py barrel oak chest    # a few
    python tools/rooms/paint_props.py --sheet shots/art/rooms   # and props-sheet.png there

A painted prop is 4 texels to a world pixel (an art pixel of the old one), and each of its frames
is exactly four times the old frame's size, in the same number and order (a chest shut then open,
the ship's seven, the gangplank's six), so PropSprite cuts it the same way and its foot, its
footprint and its hover box stay where they were: the foot is the middle of each frame's bottom
edge. A prop's drawing is fitted to the old picture's opaque box (fit()), so its silhouette is the
old one's size. A prop that gives off light (PropCatalog's PropLight) leaves its flame to the
game's TorchSprite, which burns at the light's Flame point; the painting has the fuel, the wick,
the lit glass under it.

The look is the rooms': clean dark outlines (Layer.place puts them round each piece), cel shading
lit from the top left, paint.py's palette. Things on the floor stand a little seen from above, so
boxes and tables show their tops; ground decals (the mosaic, the tar, a hole) lie flat, seen
side-on from slightly above.

Every prop is a painter in PAINTERS, f(c) where c is a Frame: the layer to draw on (c.L, the new
frame's size, clear), the foot (c.cx, c.g), the old picture's opaque box fitted to the new size
(c.x0, c.y0, c.x1, c.y1, c.bw, c.bh), the frame being drawn (c.frame) and a seed. Painters are
grouped by kind (trees, bushes and plants, boxes and barrels, furniture, lights, the docks, stone
and the vaults, the camp) and most are one parametric painter called with different colours.

Deterministic (hash01 noise, fixed seeds per prop). Only needs CPython and numpy.
"""

import argparse
import math
import os
import sys
import zlib

import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
import paint as P  # noqa: E402
import roomkit  # noqa: E402
from paint import Layer, Path, Piece, hash01, haze, ramp, tones  # noqa: E402
import vector  # noqa: E402
from vector import read_png, write_png  # noqa: E402

K = 4
"""Texels a world pixel: a painted prop is four times the old one's size."""

OUT = os.path.join(roomkit.CONTENT, "Props", "Painted")
SHOTS = os.path.join(roomkit.ROOT, "shots", "art", "rooms")


# --------------------------------------------------------------------------- #
# Which props, and their frames
# --------------------------------------------------------------------------- #

def used_props():
    """Every prop a room stands on its floor: the zone files' prop lines, and the look of every
    chest, sign, lever and waypoint (its kind when it has no look)."""
    found = set()
    for f in roomkit.zone_files():
        z = roomkit.load_zone(f)
        for r in z.rooms:
            for p in r.props:
                found.add(p.prop)
            for t in r.things:
                found.add(t.look or t.kind)
    return sorted(found)


class Frame:
    """One frame of a prop being painted: a clear Layer the new frame's size, the foot at the
    middle of its bottom edge, the old picture's opaque box fitted to it, the frame's index."""

    def __init__(self, name, frame, w, h, box, seed):
        self.name, self.frame, self.seed = name, frame, seed
        self.W, self.H = w, h
        self.L = Layer(w, h)
        self.cx, self.g = w / 2.0, h - 3.0
        self.x0, self.y0, self.x1, self.y1 = box
        self.bw, self.bh = self.x1 - self.x0, self.y1 - self.y0
        self.mx = (self.x0 + self.x1) / 2.0

    def piece(self, pad=4):
        return Piece(0, 0, self.W, self.H, pad=pad)


def old_frames(name):
    """The old picture's frames, as H x W x 4 arrays, and how many there are."""
    img = read_png(os.path.join(roomkit.CONTENT, "Props", name + ".png"))
    n = roomkit.prop_frames().get(name, 1)
    w = img.shape[1] // n
    return [img[:, i * w:(i + 1) * w] for i in range(n)]


def fit(old):
    """The old frame's opaque box, four times over: x0, y0, x1, y1 in new texels."""
    a = old[..., 3] > 24
    ys, xs = np.nonzero(a)
    if len(xs) == 0:
        return 0, 0, old.shape[1] * K, old.shape[0] * K
    return xs.min() * K, ys.min() * K, (xs.max() + 1) * K, (ys.max() + 1) * K


def seed_of(name):
    return zlib.crc32(("prop/" + name).encode("utf-8"))


# --------------------------------------------------------------------------- #
# Small shared shapes
# --------------------------------------------------------------------------- #

def T(name, **kw):
    return tones(name, **kw)


def shade_lr(p, m, t, cx, w, light=0.32, shadow=0.18):
    """Planar cel shading on an upright thing: its left a step lit, its right in shadow."""
    p.tone(p.where(m, lambda xx, yy: xx > cx + w * shadow), t, "shadow")
    p.tone(p.where(m, lambda xx, yy: xx < cx - w * light), t, "light")


def board(p, x, y, w, h, t, r=1.5, top=True, grain=0, seed=0):
    """A plank or a box face: fill, a lit top edge, grain lines, its edge."""
    path = Path.rect(x, y, w, h, r=r)
    m = p.fill(path, t, "base")
    if top:
        p.tone(m - m.shift(0, 2.0), t, "light")
    for i in range(grain):
        gy = y + h * (i + 1) / (grain + 1) + (hash01(i, 1, seed) - 0.5) * 2
        p.stroke(Path.polyline([(x + w * 0.1, gy), (x + w * 0.6, gy + 0.6), (x + w * 0.9, gy - 0.4)]), 0.9, t, "terminator", clip=m)
    p.edge(path, t, 1.2)
    return m


def leg(p, x, y, w, h, t):
    path = Path.rect(x, y, w, h, r=1)
    p.fill(path, t, "shadow")
    p.edge(path, t, 1.0)


def trunk(p, x, ground, top, w, t, flare=1.6, lean=0.0):
    """A tree's trunk from the ground to `top`, flaring at the roots, lit left, shadowed right."""
    tp = Path().move(x - w * flare, ground).quad(x - w * 0.55, ground - (ground - top) * 0.3, x - w * 0.5 + lean, top) \
        .line(x + w * 0.5 + lean, top).quad(x + w * 0.6, ground - (ground - top) * 0.3, x + w * flare * 1.05, ground).close()
    m = p.fill(tp, t, "base")
    shade_lr(p, m, t, x, w, 0.3, 0.12)
    for k in range(3):
        bx = x - w * 0.3 + k * w * 0.3
        p.stroke(Path.polyline([(bx, top + (ground - top) * 0.2), (bx + 1 + lean * 0.3, ground - (ground - top) * 0.15)]), (1.2, 0.0), t, "deep", clip=m)
    p.edge(tp, t, 1.3)
    return m


def branch(p, a, b, w0, w1, t):
    p.stroke(Path.smooth([a, ((a[0] + b[0]) / 2 + (b[1] - a[1]) * 0.1, (a[1] + b[1]) / 2), b]), (w0, w1), t, "base")
    p.stroke(Path.smooth([a, ((a[0] + b[0]) / 2 + (b[1] - a[1]) * 0.1, (a[1] + b[1]) / 2), b]).translate(-w0 * 0.2, -w0 * 0.15), (w0 * 0.35, 0.0), t, "light")


def flame_bed(p, cx, y, w, seed, hot=True):
    """Embers and coals glowing in a fire's bed (the flame is the game's TorchSprite)."""
    coal = T("#5a3a34")
    for i in range(int(w / 7)):
        x = cx - w / 2 + 4 + i * 7 + (hash01(i, 1, seed) - 0.5) * 3
        m = p.fill(Path.ellipse(x, y - 2 - 2 * hash01(i, 2, seed), 4.2, 3.0), coal, "base")
        if hot:
            p.tone(m & p.shape(Path.ellipse(x - 0.8, y - 3, 2.4, 1.6)), T("#ff9a48"), "light")
    if hot:
        p.glow(cx, y - 6, w * 1.1, "#ff9848", 0.45)


def stone_block(p, path, t, which="base", lit=True):
    m = p.fill(path, t, which)
    if lit:
        p.tone(m - m.shift(1.6, 1.6), t, "light")
        p.tone(m - m.shift(-1.6, -1.6), t, "shadow")
    p.edge(path, t, 1.2)
    return m


# --------------------------------------------------------------------------- #
# Trees
# --------------------------------------------------------------------------- #

TREE_COLOURS = {
    "oak": ("leaf", "leaf.dark", None),
    "oak-deep": ("#3f8a58", "#285e4c", None),
    "oak-light": ("#7cbc56", "#4c9a52", None),
    "oak-pale": ("#8ab878", "#5f9a6c", None),
    "oak-turning": ("#d9a446", "#b8703a", ("#e8c45a", "#d8603a")),
    "tree-blackvein": ("#5f8a6c", "#3e5c50", None),
    "tree-blackvein-pale": ("#87a68e", "#5f7c6c", None),
    "blossom-tree": ("blossom", "#e07fae", ("blossom.white", "flower.pink")),
    "cherry-tree-small": ("blossom", "#e07fae", ("blossom.white",)),
}


def p_bare_tree(c):
    """A dead tree (or the Mirewood's leafless blackvein): a trunk splitting into crooked branches
    that fork and thin to points."""
    p = c.piece()
    vein = "blackvein" in c.name
    bark = T("#4a3a46" if vein else "#8a6a56")
    tw = max(5.0, c.bw * 0.11)
    top = c.y0 + c.bh * 0.42
    trunk(p, c.mx, c.g, top, tw, bark, flare=1.8)

    def grow(x, y, ang, ln, w, depth, i):
        if depth == 0 or w < 1.2:
            return
        x2 = x + math.cos(math.radians(ang)) * ln
        y2 = y + math.sin(math.radians(ang)) * ln
        branch(p, (x, y), (x2, y2), w, w * 0.55, bark)
        for k, turn in enumerate((-28, 24)):
            j = i * 3 + k + 1
            grow(x2, y2, ang + turn + (hash01(j, depth, c.seed) - 0.5) * 22, ln * (0.62 + 0.15 * hash01(j, 7, c.seed)), w * 0.6, depth - 1, j)

    grow(c.mx, top + 4, -90 - 18, c.bh * 0.22, tw * 1.0, 4, 1)
    grow(c.mx, top + 4, -90 + 20, c.bh * 0.2, tw * 0.9, 4, 2)
    if vein:
        p.glow(c.mx, top + c.bh * 0.15, c.bw * 0.4, "#8a4ad0", 0.12)
    p.ground_shadow(c.mx - c.bw * 0.25, c.mx + c.bw * 0.25, c.g + 1, 6, 0.3)
    c.L.place(p)


def p_conifer(c):
    """A fir: tiers of drooping boughs, narrowing to a point, the left of each lit."""
    p = c.piece()
    deep = c.name.endswith("deep")
    t = T("#2f6e5a" if deep else "#3c8a5e")
    back = T("#21503f" if deep else "#2c6a50")
    bark = T("bark")
    trunk(p, c.mx, c.g, c.g - c.bh * 0.2, max(4.0, c.bw * 0.07), bark)
    tiers = 6
    top = c.y0 + 4
    base = c.g - c.bh * 0.1
    whole = p.nothing()
    for i in range(tiers):
        u0 = i / tiers
        u1 = (i + 1.35) / tiers
        y0_ = top + (base - top) * u0
        y1_ = top + (base - top) * min(1.0, u1)
        half = c.bw * 0.5 * (0.22 + 0.78 * min(1.0, u1))
        tier = Path().move(c.mx, y0_).quad(c.mx - half * 0.4, y0_ + (y1_ - y0_) * 0.5, c.mx - half, y1_) \
            .quad(c.mx - half * 0.3, y1_ - 6, c.mx, y1_ - 2).quad(c.mx + half * 0.3, y1_ - 6, c.mx + half, y1_) \
            .quad(c.mx + half * 0.4, y0_ + (y1_ - y0_) * 0.5, c.mx, y0_).close()
        m = p.fill(tier, t, "base")
        p.tone(p.where(m, lambda xx, yy: xx > c.mx + half * 0.15), t, "shadow")
        p.tone(m - p.shape(tier.translate(2.5, 3)), t, "light")
        p.tone(p.below(m, y1_ - 7), back, "base", alpha=0.6)
        p.edge(tier, t, 1.2)
        whole = whole | m
    p.ground_shadow(c.mx - c.bw * 0.3, c.mx + c.bw * 0.3, c.g + 1, 6, 0.3)
    c.L.place(p)


def p_topiary(c):
    p = c.piece()
    P.topiary(None, c.mx, c.g, c.bh, c.seed, piece=p)
    c.L.place(p)


def p_stump(c):
    """A cut stump with its rings showing on top, roots spreading; the woodcutter's has an axe in it."""
    p = c.piece()
    bark = T("bark")
    wood = T("wood.light")
    w = c.bw * (0.62 if c.name == "woodcutter-stump" else 0.7)
    top = c.y0 + c.bh * 0.32
    body = Path().move(c.mx - w * 0.62, c.g).quad(c.mx - w * 0.48, c.g - 6, c.mx - w * 0.44, top + 4).line(c.mx + w * 0.44, top + 4) \
        .quad(c.mx + w * 0.5, c.g - 6, c.mx + w * 0.66, c.g).close()
    m = p.fill(body, bark, "base")
    shade_lr(p, m, bark, c.mx, w, 0.3, 0.15)
    for k in range(4):
        bx = c.mx - w * 0.35 + k * w * 0.23
        p.stroke(Path.polyline([(bx, top + 10), (bx + 1, c.g - 4)]), (1.2, 0.0), bark, "deep", clip=m)
    p.edge(body, bark, 1.3)
    tp = Path.ellipse(c.mx, top + 4, w * 0.44, w * 0.13)
    tm = p.fill(tp, wood, "base")
    for k in (0.7, 0.45, 0.22):
        p.stroke(Path.ellipse(c.mx + 1, top + 4, w * 0.44 * k, w * 0.13 * k), 0.9, wood, "terminator", clip=tm)
    p.edge(tp, wood, 1.1)
    if c.name == "woodcutter-stump":
        p.stroke(Path.polyline([(c.mx + w * 0.1, top + 2), (c.mx + w * 0.55, top - c.bh * 0.3)]), 4.0, T("wood"), "base")
        head = Path.polygon([(c.mx + w * 0.02, top + 4), (c.mx + w * 0.2, top - 8), (c.mx + w * 0.26, top + 4)])
        p.fill(head, T("#9aa0b4"), "light")
        p.edge(head, T("#9aa0b4"), 1.0)
        logs = Path.rect(c.mx + w * 0.55, c.g - 14, w * 0.5, 12, r=6)
        p.fill(logs, T("wood.light"), "base")
        p.edge(logs, T("wood.light"), 1.0)
    p.ground_shadow(c.mx - w * 0.7, c.mx + w * 0.7, c.g + 1, 6, 0.3)
    c.L.place(p)


def p_snag(c):
    """A broken dead trunk standing alone, jagged at the top."""
    p = c.piece()
    bark = T("#8a6a56")
    w = c.bw * 0.42
    top = c.y0 + 6
    path = Path.polygon([(c.mx - w * 0.9, c.g), (c.mx - w * 0.45, c.g - c.bh * 0.3), (c.mx - w * 0.5, top + 14), (c.mx - w * 0.2, top + 2),
                         (c.mx, top + 12), (c.mx + w * 0.2, top), (c.mx + w * 0.45, top + 16), (c.mx + w * 0.5, c.g - c.bh * 0.3), (c.mx + w * 0.95, c.g)])
    m = p.fill(path, bark, "base")
    shade_lr(p, m, bark, c.mx, w, 0.3, 0.12)
    for k in range(3):
        bx = c.mx - w * 0.25 + k * w * 0.25
        p.stroke(Path.polyline([(bx, top + 20), (bx + 1, c.g - 6)]), (1.2, 0.0), bark, "deep", clip=m)
    p.edge(path, bark, 1.3)
    branch(p, (c.mx + w * 0.4, c.g - c.bh * 0.55), (c.mx + w * 1.5, c.g - c.bh * 0.72), 4, 1.5, bark)
    p.ground_shadow(c.mx - w, c.mx + w, c.g + 1, 5, 0.3)
    c.L.place(p)


def p_log(c):
    """A log lying along the ground, its cut end showing its rings, moss on top."""
    p = c.piece()
    bark = T("bark")
    wood = T("wood.light")
    h = c.bh * 0.78
    y = c.g - h
    x0, x1 = c.x0 + 4, c.x1 - 4
    body = Path.rect(x0, y, x1 - x0 - h * 0.3, h, r=h * 0.45)
    m = p.fill(body, bark, "base")
    p.tone(p.below(m, y + h * 0.62), bark, "shadow")
    p.tone(p.above(m, y + h * 0.22), bark, "light")
    for k in range(4):
        gx = x0 + (x1 - x0) * (0.15 + 0.2 * k)
        p.stroke(Path.polyline([(gx, y + h * 0.3), (gx + 14, y + h * 0.36)]), (1.1, 0.0), bark, "deep", clip=m)
    moss = T("leaf")
    p.fill(Path.ellipse(x0 + (x1 - x0) * 0.35, y + 3, (x1 - x0) * 0.2, 4), moss, "base", clip=m)
    p.edge(body, bark, 1.3)
    end = Path.ellipse(x1 - h * 0.45, y + h / 2, h * 0.36, h * 0.5)
    em = p.fill(end, wood, "base")
    p.stroke(Path.ellipse(x1 - h * 0.45, y + h / 2, h * 0.22, h * 0.32), 0.9, wood, "terminator", clip=em)
    p.edge(end, wood, 1.1)
    p.ground_shadow(x0, x1, c.g + 1, 5, 0.3)
    c.L.place(p)


def p_fallen_tree(c):
    """A tree down across the road: the trunk lying from the far verge to the near one, its roots
    torn up at the near end, a few dead boughs."""
    p = c.piece()
    bark = T("bark")
    ax, ay = c.x1 - c.bw * 0.08, c.y0 + c.bh * 0.12
    bx, by = c.x0 + c.bw * 0.2, c.y1 - c.bh * 0.18
    w = c.bh * 0.17
    body = Path.capsule((ax, ay), (bx, by), w * 0.45, w * 0.6)
    m = p.fill(body, bark, "base")
    p.tone(m - m.shift(-3, -4), bark, "shadow")
    p.tone(m - m.shift(3, 3), bark, "light")
    for k in range(5):
        u = 0.15 + k * 0.16
        x, y = ax + (bx - ax) * u, ay + (by - ay) * u
        p.stroke(Path.polyline([(x - 6, y - 3), (x + 8, y + 2)]), (1.2, 0.0), bark, "deep", clip=m)
    p.edge(body, bark, 1.4)
    for k in range(3):
        u = 0.2 + k * 0.25
        x, y = ax + (bx - ax) * u, ay + (by - ay) * u
        branch(p, (x, y - w * 0.3), (x + 20 + k * 6, y - 40 - k * 8), 4, 1.2, bark)
    roots = T("#6a4a3a")
    rp = Path.circle(bx - w * 0.4, by + w * 0.1, w * 1.05)
    rm = p.fill(rp, T("earth"), "base")
    p.tone(rm - rm.shift(-3, -3), T("earth"), "shadow")
    for k in range(7):
        a = math.radians(100 + k * 30)
        p.stroke(Path.smooth([(bx - w * 0.4, by), (bx - w * 0.4 + math.cos(a) * w * 0.9, by + math.sin(a) * w * 0.9),
                              (bx - w * 0.4 + math.cos(a + 0.3) * w * 1.4, by + math.sin(a + 0.3) * w * 1.3)]), (3.0, 0.6), roots, "base")
    p.edge(rp, T("earth"), 1.2)
    p.ground_shadow(c.x0, c.x1, c.g + 1, 8, 0.3)
    c.L.place(p)


# --------------------------------------------------------------------------- #
# Bushes, flowers and plants
# --------------------------------------------------------------------------- #

BUSHES = {
    "bush": ("leaf", None), "bush-deep": ("leaf.dark", None), "bush-flowers": ("leaf", ("flower.yellow", "flower.white")),
    "bush-pink": ("leaf", ("flower.pink", "blossom")), "bush-small": ("leaf", None),
}


def p_wildflowers(c):
    """Tall wild flowers: leafy stems with heads of colour (pink and white, blue, or yellow)."""
    colours = {"wildflowers": ("flower.pink", "flower.white", "flower.violet"), "wildflowers-blue": ("flower.blue", "flower.violet"),
               "wildflowers-yellow": ("flower.yellow", "flower.white")}[c.name]
    p = c.piece()
    leaf = T("leaf")
    n = max(4, int(c.bw / 9))
    for i in range(n):
        x = c.x0 + 6 + (c.bw - 12) * (i + 0.5) / n + (hash01(i, 1, c.seed) - 0.5) * 6
        h = c.bh * (0.55 + 0.42 * hash01(i, 2, c.seed))
        top = c.g - h
        lean = (hash01(i, 3, c.seed) - 0.5) * 12
        stem = Path.smooth([(x, c.g), (x + lean * 0.4, c.g - h * 0.5), (x + lean, top)])
        p.stroke(stem, (2.4, 1.2), leaf, "shadow")
        for k in range(2):
            ly = c.g - h * (0.25 + 0.25 * k)
            side = 1 if (i + k) % 2 else -1
            lm = p.fill(Path.ellipse(x + side * 5 + lean * 0.2, ly, 6, 2.4, rot=-30 * side), leaf, "base")
            p.tone(lm - lm.shift(-0.8, -0.8), leaf, "shadow")
        ft = T(colours[i % len(colours)])
        if "blue" in c.name:
            for k in range(4):
                fm = p.fill(Path.ellipse(x + lean, top + k * 5, 3.0 - k * 0.3, 2.6), ft, "base")
                p.tone(fm - fm.shift(-0.6, -0.6), ft, "shadow")
        else:
            petals = Path()
            for k in range(5):
                a = math.radians(k * 72)
                petals.add(Path.circle(x + lean + math.cos(a) * 3.2, top + math.sin(a) * 3.2, 2.8))
            fm = p.fill(petals, ft, "base", rule="nonzero")
            p.tone(fm - fm.shift(-0.7, -0.7), ft, "shadow")
            p.fill(Path.circle(x + lean, top, 1.6), T("flower.yellow"), "deep-shadow")
    c.L.place(p, reach=1.6)


def p_fern(c):
    """A fern: arching fronds, each a stem with paired leaflets, from a dark heart."""
    p = c.piece()
    t = T("#4f9a58")
    n = 7
    for i in range(n):
        a = math.radians(-160 + i * (140 / (n - 1)))
        ln = c.bh * (0.8 + 0.2 * math.sin(math.pi * i / (n - 1)))
        tip = (c.mx + math.cos(a) * ln * 0.75, c.g + math.sin(a) * ln)
        mid = (c.mx + math.cos(a) * ln * 0.45, c.g + math.sin(a) * ln * 0.95 - 4)
        frond = Path.smooth([(c.mx, c.g - 2), mid, tip])
        tt = t if i % 2 else T("leaf.dark")
        for k in range(1, 8):
            (fx, fy), (tx, ty) = frond.at(k / 8.0)
            s = 6.0 * (1 - k / 9.0)
            for side in (-1, 1):
                lm = p.fill(Path.ellipse(fx - ty * s * side * 0.8, fy + tx * s * side * 0.8, s, s * 0.4,
                                         rot=math.degrees(math.atan2(ty, tx)) + 60 * side), tt, "base")
                p.tone(lm - lm.shift(-0.6, -0.8), tt, "shadow")
        p.stroke(frond, (2.0, 0.4), tt, "deep")
    c.L.place(p, reach=1.6)


def p_grass(c):
    """A tuft of long grass, blades fanning up, dark at their roots."""
    p = c.piece()
    t = T("#7ab858")
    blades = Path()
    for i in range(11):
        a = math.radians(-90 + (i - 5) * 9 + (hash01(i, 1, c.seed) - 0.5) * 8)
        ln = c.bh * (0.6 + 0.4 * hash01(i, 2, c.seed))
        x = c.mx + (i - 5) * 2.2
        blades.add(Path.smooth([(x, c.g), (x + math.cos(a) * ln * 0.5, c.g + math.sin(a) * ln * 0.55), (x + math.cos(a) * ln * 1.1 + (i - 5) * 1.5, c.g + math.sin(a) * ln)]))
    m = p.stroke(blades, (4.2, 0.0), t, "base")
    p.tone(p.below(m, c.g - c.bh * 0.35), t, "shadow")
    p.stroke(blades.translate(-0.8, -1.0), (1.4, 0.0), t, "light")
    c.L.place(p, reach=1.6)


def p_mushrooms(c):
    """A few toadstools (red-capped, or pale), or the Mirewood's gloom mushrooms, dark violet and
    faintly glowing under their caps."""
    p = c.piece()
    gloom = "gloom" in c.name
    cap = T({"mushrooms": "#d84a44", "mushrooms-pale": "#e8dcc8"}.get(c.name, "#5a3a7c"))
    stalk = T("#efe4d4" if not gloom else "#8a7aa8")
    n = 3 if "small" not in c.name else 2
    if c.name in ("mushrooms", "mushrooms-pale"):
        n = 3
    for i in range(n):
        u = (i + 0.5) / n
        x = c.x0 + c.bw * (0.15 + 0.7 * u)
        h = c.bh * (0.95 if i == 1 else 0.62 + 0.2 * hash01(i, 1, c.seed))
        r = c.bw * (0.22 if i == 1 else 0.15) * (1.3 if gloom else 1.0)
        top = c.g - h
        st = Path.rect(x - r * 0.28, top + r * 0.4, r * 0.56, h - r * 0.4, r=r * 0.2)
        sm = p.fill(st, stalk, "base")
        shade_lr(p, sm, stalk, x, r * 0.56)
        p.edge(st, stalk, 1.0)
        capp = Path().move(x - r, top + r * 0.55).quad(x - r * 0.95, top - r * 0.4, x, top - r * 0.5).quad(x + r * 0.95, top - r * 0.4, x + r, top + r * 0.55) \
            .quad(x, top + r * 0.8, x - r, top + r * 0.55).close()
        cm = p.fill(capp, cap, "base")
        p.tone(cm - cm.shift(-2, -2.5), cap, "shadow")
        p.tone(cm & p.shape(Path.ellipse(x - r * 0.35, top - r * 0.15, r * 0.35, r * 0.2)), cap, "light")
        if c.name == "mushrooms":
            for k in range(3):
                p.fill(Path.circle(x - r * 0.4 + k * r * 0.4, top - r * 0.05 + (k % 2) * 3, r * 0.12), T("flower.white"), "base", clip=cm)
        p.edge(capp, cap, 1.1)
        if gloom:
            p.glow(x, top + r * 0.6, r * 2.4, "#b07ae8", 0.25)
            for k in range(3):
                p.fill(Path.circle(x - r * 0.4 + k * r * 0.4, top + r * 0.1, 1.5), T("#d8b8ff"), "light", clip=cm)
    c.L.place(p, reach=1.8)


def p_potted(c):
    """A potted plant: a glazed pot and a leafy plant with a few white lilies."""
    p = c.piece()
    P.pot(None, c.mx, c.g, c.bw * 0.62, c.bh * 0.32, "terracotta", piece=p)
    t = T("leaf")
    top = c.g - c.bh * 0.3
    for i in range(7):
        a = math.radians(-150 + i * 20)
        ln = c.bh * (0.5 + 0.2 * hash01(i, 1, c.seed))
        tip = (c.mx + math.cos(a) * ln * 0.5, top + math.sin(a) * ln)
        lp = Path().move(c.mx, top).quad(c.mx + math.cos(a) * ln * 0.2 - 5, top + math.sin(a) * ln * 0.5, *tip) \
            .quad(c.mx + math.cos(a) * ln * 0.2 + 5, top + math.sin(a) * ln * 0.5, c.mx + 1, top).close()
        m = p.fill(lp, t if i % 2 else T("leaf.dark"), "base")
        p.tone(m - m.shift(-1, -1), t, "shadow")
        p.edge(lp, t, 1.0)
    for k in range(2):
        fx, fy = c.mx + (k - 0.5) * c.bw * 0.3, top - c.bh * 0.42 - k * 6
        lily = Path.polygon([(fx - 5, fy), (fx, fy - 7), (fx + 5, fy), (fx, fy + 3)])
        p.fill(lily, T("flower.white"), "base")
        p.edge(lily, T("flower.white"), 0.9)
    c.L.place(p)


def p_planter(c):
    """A long wooden planter box spilling flowers."""
    p = c.piece()
    h = c.bh * 0.45
    P.bush(None, c.mx, c.g - h + 4, c.bw * 0.86, (c.bh - h) / 1.45, "leaf", c.seed, flowers_on=("flower.pink", "flower.white", "flower.yellow"), piece=p)
    wt = T("wood")
    board(p, c.x0 + 4, c.g - h, c.bw - 8, h, wt, grain=2, seed=c.seed)
    for x in (c.x0 + 4, c.x1 - 12):
        p.fill(Path.rect(x, c.g - h, 8, h), wt, "terminator")
    p.ground_shadow(c.x0, c.x1, c.g + 1, 5, 0.3)
    c.L.place(p)


def p_hay(c):
    """A bale of hay, bound with twine, straws sticking out."""
    p = c.piece()
    t = T("#e2c066")
    h = c.bh * 0.92
    body = Path.rect(c.x0 + 3, c.g - h, c.bw - 6, h, r=8)
    m = p.fill(body, t, "base")
    p.tone(m - m.shift(0, 3), t, "light")
    p.tone(p.below(m, c.g - h * 0.3), t, "shadow")
    for i in range(int(c.bw / 6)):
        x = c.x0 + 6 + i * 6
        p.stroke(Path.polyline([(x, c.g - h + 6), (x + 2, c.g - 6)]), 0.9, t, "terminator", clip=m)
    for x in (c.x0 + c.bw * 0.3, c.x0 + c.bw * 0.7):
        p.stroke(Path.polyline([(x, c.g - h), (x, c.g)]), 2.0, T("rope"), "base", clip=m)
    p.edge(body, t, 1.2)
    p.ground_shadow(c.x0, c.x1, c.g + 1, 5, 0.3)
    c.L.place(p)


# --------------------------------------------------------------------------- #
# Boxes, barrels, sacks and bundles
# --------------------------------------------------------------------------- #

def box3(p, x0, y0, x1, y1, t, depth=8.0, top_t=None, side=True, which="base"):
    """A box seen from the front and a little above: its front face, its top a lit parallelogram
    going back up and right, its right side in shadow. Returns the front's Mask."""
    d = depth
    if side:
        sp = Path.polygon([(x1, y0), (x1 + d * 0.6, y0 - d), (x1 + d * 0.6, y1 - d), (x1, y1)])
        p.fill(sp, t, "shadow")
        p.edge(sp, t, 1.1)
    tp = Path.polygon([(x0, y0), (x0 + d * 0.6, y0 - d), (x1 + d * 0.6, y0 - d), (x1, y0)])
    p.fill(tp, top_t or t, "light")
    p.edge(tp, top_t or t, 1.1)
    fp = Path.rect(x0, y0, x1 - x0, y1 - y0)
    m = p.fill(fp, t, which)
    p.edge(fp, t, 1.3)
    return m


def p_barrel(c):
    """Barrels: plain, water (the lid off and water in it), salt (a heap on top), flowers (planted),
    cargo (darker, a mark on it), and a small cask."""
    p = c.piece()
    n = c.name
    colour = "wood.dark" if n == "cargo-barrel" else "wood"
    top_extra = {"barrel-flowers": 0.36, "barrel-salt": 0.16}.get(n, 0.0)
    w = c.bw / 1.14
    h = c.bh * (1 - top_extra) - 7
    P.barrel(None, c.mx, c.g, w, h, colour, c.seed, piece=p, lid=n not in ("barrel-water", "barrel-salt", "barrel-flowers"))
    top = c.g - h
    if n == "barrel-water":
        wt = T("water")
        m = p.fill(Path.ellipse(c.mx, top, w * 0.42, 5), wt, "base")
        p.stroke(Path.polyline([(c.mx - w * 0.2, top - 1), (c.mx + w * 0.05, top - 1)]), 1.4, wt, "highlight", clip=m)
        p.edge(Path.ellipse(c.mx, top, w * 0.42, 5), T("wood"), 1.6, "deep")
    elif n == "barrel-salt":
        st = T("#f2eef0")
        heap = Path().move(c.mx - w * 0.44, top + 2).quad(c.mx - w * 0.2, top - c.bh * 0.2, c.mx, top - c.bh * 0.18) \
            .quad(c.mx + w * 0.25, top - c.bh * 0.16, c.mx + w * 0.44, top + 2).close()
        m = p.fill(heap, st, "base")
        p.tone(m - m.shift(-2, -3), st, "shadow")
        p.edge(heap, st, 1.1)
    elif n == "barrel-flowers":
        P.bush(None, c.mx, top + 4, w * 0.95, c.bh * top_extra / 1.3, "leaf", c.seed,
               flowers_on=("flower.pink", "flower.white", "flower.yellow"), piece=p)
    elif n == "cargo-barrel":
        for a, b in (((-0.15, 0.55), (0.15, 0.35)), ((-0.15, 0.35), (0.15, 0.55))):
            p.stroke(Path.polyline([(c.mx + a[0] * w, c.g - h * a[1]), (c.mx + b[0] * w, c.g - h * b[1])]), 2.4, T("cloth.cream"), "base")
    c.L.place(p)


def barrel_end(p, cx, cy, r, t, iron):
    """A barrel lying on its side, seen end on: the round head with its boards and its hoop."""
    m = p.fill(Path.circle(cx, cy, r), t, "base")
    for k in range(-2, 3):
        p.stroke(Path.polyline([(cx + k * r * 0.36, cy - r), (cx + k * r * 0.36, cy + r)]), 0.9, t, "deep", clip=m)
    p.tone(m - m.shift(2, 2), t, "light")
    p.tone(m - m.shift(-2.5, -2.5), t, "shadow")
    ring = m - p.shape(Path.circle(cx, cy, r - 3.5))
    p.tone(ring, iron, "base")
    p.fill(Path.circle(cx, cy, r * 0.18), t, "deep-shadow")
    p.edge(Path.circle(cx, cy, r), iron, 1.3)


def p_keg_stack(c):
    """Kegs stacked on their sides, ends to us: two below, one on top."""
    p = c.piece()
    t, iron = T("wood"), T("iron")
    r = min(c.bw / 4.1, c.bh / 3.9)
    for (dx, row) in ((-1, 0), (1, 0), (0, 1)):
        barrel_end(p, c.mx + dx * r * 1.02, c.g - r - row * r * 1.72, r, t, iron)
    p.ground_shadow(c.mx - r * 2, c.mx + r * 2, c.g + 1, 6, 0.3)
    c.L.place(p)


GOODS = {"crate-apples": "apple", "crate-onions": "#c99a7c"}


def p_crate(c):
    """Crates: plain, of apples or onions (heaped in the top), open (its lid leaning on it, straw
    in it), stacked, a cargo crate (roped, stencilled), a crate of books."""
    p = c.piece()
    n = c.name
    if n == "crate-stack":
        w = c.bw * 0.48
        P.crate(None, c.mx - w * 0.55, c.g, w, c.bh * 0.42, c.seed, piece=p)
        P.crate(None, c.mx + w * 0.5, c.g, w * 0.92, c.bh * 0.38, c.seed + 1, piece=p)
        P.crate(None, c.mx - w * 0.05, c.g - c.bh * 0.44, w * 0.95, c.bh * 0.36, c.seed + 2, piece=p)
    elif n == "book-crate":
        w, h = c.bw - 8, c.bh * 0.5
        top = c.g - h
        for i in range(6):
            bt = T(("cloth.red", "cloth.navy", "cloth.teal", "cloth.plum", "wood", "cloth.gold")[i])
            bh = c.bh * (0.32 + 0.12 * hash01(i, 1, c.seed))
            bx = c.mx - w * 0.42 + i * w * 0.14
            bp = Path.rect(bx, top - bh + 10, w * 0.12, bh, r=1)
            p.fill(bp, bt, "base")
            p.fill(Path.rect(bx, top - bh + 16, w * 0.12, 3), T("gold"), "base")
            p.edge(bp, bt, 1.0)
        P.crate(None, c.mx - 2, c.g, w, h, c.seed, piece=p)
    else:
        goods = GOODS.get(n)
        w = c.bw - 7
        h = c.bh - (16 if goods else 10)
        P.crate(None, c.mx - 2, c.g, w, h, c.seed, piece=p, top_goods=goods)
        if n == "cargo-crate":
            p.stroke(Path.polyline([(c.mx - 2 - w / 2, c.g - h * 0.5), (c.mx - 2 + w / 2, c.g - h * 0.5)]), 3.2, T("rope"), "base")
            P._icon(p, "diamond", c.mx - 2, c.g - h * 0.74, h * 0.3)
        if n == "crate-open":
            straw = T("#e2c066")
            for i in range(9):
                sx = c.mx - 2 - w * 0.42 + i * w * 0.1
                p.stroke(Path.polyline([(sx, c.g - h - 1), (sx + 4, c.g - h - 8 - 4 * hash01(i, 1, c.seed))]), (2.0, 0.4), straw, "base")
            lid = Path.polygon([(c.mx + w * 0.3, c.g), (c.mx + w * 0.44, c.g - h * 0.95), (c.mx + w * 0.52, c.g - h * 0.92), (c.mx + w * 0.4, c.g)])
            p.fill(lid, T("wood.light"), "light")
            p.edge(lid, T("wood.light"), 1.1)
    c.L.place(p)


def pillow(p, x0, y0, x1, y1, t, tie=None):
    """A soft lumpy bundle (cloth tied up, a sack lying down), shaded as a pillow by style.shade."""
    import style
    w, h = x1 - x0, y1 - y0
    path = Path.smooth([(x0 + w * 0.05, y1), (x0, y0 + h * 0.5), (x0 + w * 0.15, y0 + h * 0.08), (x0 + w * 0.5, y0),
                        (x1 - w * 0.12, y0 + h * 0.1), (x1, y0 + h * 0.55), (x1 - w * 0.06, y1)], closed=True)
    m = p.shape(path)
    style.shade(m, "cloth", t, edge=P.INNER, rim=False)
    if tie:
        p.stroke(Path.polyline([(x0 + w * 0.5, y0 + 2), (x0 + w * 0.52, y1 - 2)]), 2.2, T(tie), "base", clip=m)
    return m


def p_bundle(c):
    """The refugees' bundles (cloth tied up), a bedroll, a satchel, a coil of rope."""
    p = c.piece()
    n = c.name
    if n == "refugee-bundle":
        pillow(p, c.x0 + 4, c.g - c.bh * 0.85, c.mx + c.bw * 0.15, c.g, T("cloth.plum"), "rope")
        pillow(p, c.mx - c.bw * 0.05, c.g - c.bh * 0.6, c.x1 - 4, c.g, T("canvas"), "rope")
    elif n == "bedroll":
        h = c.bh * 0.9
        t = T("cloth.red")
        body = Path.rect(c.x0 + 4, c.g - h, c.bw - 8 - h * 0.4, h, r=h * 0.45)
        m = p.fill(body, t, "base")
        p.tone(p.below(m, c.g - h * 0.35), t, "shadow")
        for k in range(3):
            p.stroke(Path.polyline([(c.x0 + 20 + k * 24, c.g - h), (c.x0 + 20 + k * 24, c.g)]), 3, T("cloth.cream"), "base", clip=m)
        p.edge(body, t, 1.2)
        end = Path.ellipse(c.x1 - 4 - h * 0.45, c.g - h / 2, h * 0.3, h * 0.5)
        em = p.fill(end, t, "light")
        p.stroke(Path().arc(c.x1 - 4 - h * 0.45, c.g - h / 2, h * 0.15, 0, 300, h * 0.25), 1.2, t, "deep", clip=em)
        p.edge(end, t, 1.1)
    elif n == "satchel":
        t = T("#8a5a3a")
        body = Path.rect(c.x0 + 4, c.g - c.bh * 0.7, c.bw - 8, c.bh * 0.7, r=6)
        m = p.fill(body, t, "base")
        p.tone(p.below(m, c.g - c.bh * 0.25), t, "shadow")
        flap = Path.polygon([(c.x0 + 4, c.g - c.bh * 0.7), (c.x1 - 4, c.g - c.bh * 0.7), (c.x1 - 8, c.g - c.bh * 0.35), (c.x0 + 8, c.g - c.bh * 0.35)])
        p.fill(flap, t, "light")
        p.edge(flap, t, 1.0)
        p.fill(Path.rect(c.mx - 3, c.g - c.bh * 0.42, 6, 6), T("brass"), "base")
        p.stroke(Path.smooth([(c.x0 + 6, c.g - c.bh * 0.6), (c.mx, c.y0 + 2), (c.x1 - 6, c.g - c.bh * 0.6)]), 2.0, t, "deep")
        p.edge(body, t, 1.2)
    else:
        t = T("rope")
        for k in range(4):
            rx = c.bw * (0.45 - k * 0.08)
            ring = Path.ellipse(c.mx, c.g - c.bh * 0.35 - k * 3, rx, c.bh * 0.3 - k * 1.5)
            p.stroke(ring, 5.0, t, "base")
            p.stroke(ring.translate(-0.8, -1.2), 1.4, t, "light")
        p.stroke(Path.smooth([(c.mx + c.bw * 0.3, c.g - c.bh * 0.3), (c.x1 - 2, c.g - 4)]), 4.0, t, "base")
    p.ground_shadow(c.x0 + 2, c.x1 - 2, c.g + 1, 5, 0.3)
    c.L.place(p)


# --------------------------------------------------------------------------- #
# Furniture
# --------------------------------------------------------------------------- #

def table_top(p, x0, x1, y, t, thick=10, depth=10):
    """A table's top seen a little from above: its lit top surface and its front edge."""
    top = Path.polygon([(x0 + depth * 0.3, y - depth), (x1 - depth * 0.3, y - depth), (x1, y), (x0, y)])
    p.fill(top, t, "light")
    p.edge(top, t, 1.1)
    front = Path.rect(x0, y, x1 - x0, thick)
    p.fill(front, t, "base")
    p.edge(front, t, 1.2)


def p_chair(c):
    """A wooden chair with a pink cushion, a three-legged stool, an armchair upholstered in navy."""
    p = c.piece()
    wood = T("wood")
    n = c.name
    if n == "stool":
        top = c.g - c.bh * 0.9
        for lx in (-0.38, 0.38, 0.0):
            leg(p, c.mx + lx * c.bw - 3, top + 6, 6, c.g - top - 6, wood)
        seat = Path.ellipse(c.mx, top + 4, c.bw * 0.48, 7)
        p.fill(seat, wood, "light")
        p.edge(seat, wood, 1.2)
    elif n == "armchair":
        up = T("cloth.navy")
        back = Path.rect(c.x0 + c.bw * 0.12, c.y0 + 4, c.bw * 0.76, c.bh * 0.62, r=14)
        bm = p.fill(back, up, "base")
        p.tone(bm - bm.shift(3, 3), up, "light")
        for k in range(3):
            p.fill(Path.circle(c.mx + (k - 1) * c.bw * 0.2, c.y0 + c.bh * 0.25, 2.0), T("gold"), "base")
        p.edge(back, up, 1.4)
        seat = Path.rect(c.x0 + c.bw * 0.08, c.g - c.bh * 0.42, c.bw * 0.84, c.bh * 0.18, r=8)
        p.fill(seat, up, "light")
        p.edge(seat, up, 1.2)
        for side in (0, 1):
            ax = c.x0 + 2 if side == 0 else c.x1 - c.bw * 0.2 - 2
            arm = Path.rect(ax, c.g - c.bh * 0.5, c.bw * 0.2, c.bh * 0.28, r=8)
            am = p.fill(arm, wood, "base")
            p.tone(am - am.shift(0, 2), wood, "light")
            p.edge(arm, wood, 1.2)
        front = Path.rect(c.x0 + c.bw * 0.1, c.g - c.bh * 0.26, c.bw * 0.8, c.bh * 0.12, r=3)
        p.fill(front, wood, "base")
        p.edge(front, wood, 1.1)
        for lx in (c.x0 + c.bw * 0.14, c.x1 - c.bw * 0.14 - 6):
            leg(p, lx, c.g - c.bh * 0.15, 6, c.bh * 0.15, wood)
    else:
        cush = T("cloth.pink")
        bx = c.mx + c.bw * 0.2
        back = Path.rect(bx - 4, c.y0 + 4, 8, c.bh * 0.62)
        p.fill(back, wood, "base")
        p.edge(back, wood, 1.1)
        for yy in (c.y0 + 10, c.y0 + c.bh * 0.22):
            rail = Path.rect(c.mx - c.bw * 0.3, yy, c.bw * 0.55, 6, r=2)
            p.fill(rail, wood, "light")
            p.edge(rail, wood, 1.0)
        for lx in (c.mx - c.bw * 0.34, c.mx + c.bw * 0.18):
            leg(p, lx, c.g - c.bh * 0.42, 6, c.bh * 0.42, wood)
        seat = Path.rect(c.mx - c.bw * 0.4, c.g - c.bh * 0.46, c.bw * 0.74, 8, r=2)
        p.fill(seat, wood, "base")
        p.edge(seat, wood, 1.1)
        cp = Path.rect(c.mx - c.bw * 0.38, c.g - c.bh * 0.52, c.bw * 0.7, 9, r=4)
        cm = p.fill(cp, cush, "base")
        p.tone(cm - cm.shift(0, 2), cush, "light")
        p.edge(cp, cush, 1.1)
    p.ground_shadow(c.x0 + 2, c.x1 - 2, c.g + 1, 5, 0.3)
    c.L.place(p)


def p_table(c):
    """Tables: a plank table, a round one, a side table with a candle and a book, one with tankards,
    one with wine, a desk with drawers, the royal desk, the war table with its map."""
    p = c.piece()
    n = c.name
    wood = T("wood.dark" if n in ("royal-desk", "war-table", "desk") else "wood")
    y = c.y0 + (max(14.0, c.bh * 0.3) if n in ("tankard-table", "wine-table", "side-table", "round-table") else 14.0)
    if n in ("desk", "royal-desk"):
        y = c.g - c.bh * 0.62
        body = Path.rect(c.x0 + 6, y + 8, c.bw - 12, c.g - y - 8)
        bm = p.fill(body, wood, "base")
        for k in range(2):
            dx = c.x0 + 14 + k * (c.bw - 40) * 0.6
            dr = Path.rect(dx, y + 20, (c.bw - 40) * 0.35, c.bh * 0.14, r=2)
            p.fill(dr, wood, "terminator")
            p.fill(Path.circle(dx + (c.bw - 40) * 0.175, y + 20 + c.bh * 0.07, 2.2), T("brass"), "base")
            p.edge(dr, wood, 1.0)
        if n == "royal-desk":
            p.stroke(Path.polyline([(c.x0 + 6, y + 14), (c.x1 - 6, y + 14)]), 3.0, T("gold"), "base", clip=bm)
            P._icon(p, "sun", c.mx, c.g - c.bh * 0.2, c.bh * 0.22)
        p.edge(body, wood, 1.3)
        table_top(p, c.x0, c.x1, y, wood)
        paper = Path.polygon([(c.mx - 24, y - 4), (c.mx + 6, y - 6), (c.mx + 10, y - 1), (c.mx - 20, y + 1)])
        p.fill(paper, T("cloth.cream"), "base")
        p.edge(paper, T("cloth.cream"), 0.9)
        p.fill(Path.rect(c.x1 - 30, y - 22, 8, 18), T("cloth.cream"), "base")
        p.stroke(Path.polyline([(c.x1 - 26, y - 22), (c.x1 - 26, y - 26)]), 1.0, T("wood.dark"), "base")
    else:
        if n in ("round-table", "wine-table"):
            stem = Path.rect(c.mx - 6, y + 8, 12, c.g - y - 18)
            p.fill(stem, wood, "base")
            p.edge(stem, wood, 1.1)
            foot = Path.polygon([(c.mx - c.bw * 0.3, c.g), (c.mx - 8, c.g - 12), (c.mx + 8, c.g - 12), (c.mx + c.bw * 0.3, c.g)])
            p.fill(foot, wood, "shadow")
            p.edge(foot, wood, 1.1)
            table_top(p, c.x0 + 2, c.x1 - 2, y, wood, 9, 12)
        else:
            for lx in (c.x0 + 8, c.x1 - 16):
                leg(p, lx, y + 8, 8, c.g - y - 8, wood)
            table_top(p, c.x0 + 2, c.x1 - 2, y, wood, 10, 10)
        if n == "wine-table":
            cloth = Path.polygon([(c.mx - c.bw * 0.35, y - 6), (c.mx + c.bw * 0.35, y - 6), (c.mx + c.bw * 0.4, y + 18), (c.mx - c.bw * 0.4, y + 18)])
            p.fill(cloth, T("cloth.cream"), "base")
            p.edge(cloth, T("cloth.cream"), 1.0)
            bt = T("#6a2a4a")
            bot = Path().move(c.mx - 4, y - 34).line(c.mx + 4, y - 34).line(c.mx + 4, y - 22).quad(c.mx + 9, y - 18, c.mx + 9, y - 4) \
                .line(c.mx - 9, y - 4).quad(c.mx - 9, y - 18, c.mx - 4, y - 22).close()
            p.fill(bot, bt, "base")
            p.stroke(Path.polyline([(c.mx - 5, y - 18), (c.mx - 5, y - 8)]), 1.4, bt, "highlight")
            p.edge(bot, bt, 1.0)
            for gx in (c.mx - 22, c.mx + 18):
                p.fill(Path.polygon([(gx - 5, y - 18), (gx + 5, y - 18), (gx + 2, y - 10), (gx - 2, y - 10)]), T("#e8b0c0"), "base")
                p.stroke(Path.polyline([(gx, y - 10), (gx, y - 4)]), 1.2, T("#c8c8d8"), "base")
        elif n == "tankard-table":
            for tx in (c.mx - c.bw * 0.25, c.mx + c.bw * 0.18):
                tk = T("pottery")
                tp = Path.rect(tx - 7, y - 24, 14, 20, r=2)
                p.fill(tp, tk, "base")
                p.fill(Path.rect(tx - 7, y - 26, 14, 5, r=2), T("cloth.cream"), "base")
                p.stroke(Path.circle(tx + 9, y - 14, 4.5), 2.0, tk, "base")
                p.edge(tp, tk, 1.0)
        elif n == "side-table":
            cp = Path.rect(c.mx - c.bw * 0.2, y - 26, 6, 22)
            p.fill(cp, T("cloth.cream"), "base")
            p.edge(cp, T("cloth.cream"), 0.9)
            p.fill(Path.ellipse(c.mx - c.bw * 0.2 + 3, y - 30, 2.4, 4.4), T("#ffcf6a"), "light")
            bk = Path.rect(c.mx, y - 10, 30, 8, r=1)
            p.fill(bk, T("cloth.red"), "base")
            p.edge(bk, T("cloth.red"), 1.0)
    p.ground_shadow(c.x0 + 2, c.x1 - 2, c.g + 1, 6, 0.3)
    c.L.place(p)


def p_bed(c):
    """A bed against the wall (a tall carved headboard, the mattress and pillow, a quilt over the
    front), or a camp cot, canvas on a frame."""
    p = c.piece()
    wood = T("wood")
    if c.name == "cot":
        y = c.g - c.bh * 0.62
        for lx in (c.x0 + 6, c.x1 - 14):
            leg(p, lx, y, 7, c.g - y, wood)
        can = Path.rect(c.x0 + 2, y - 6, c.bw - 4, 14, r=5)
        ct = T("canvas")
        m = p.fill(can, ct, "base")
        p.tone(p.below(m, y + 2), ct, "shadow")
        p.edge(can, ct, 1.1)
        bl = Path.rect(c.mx - c.bw * 0.1, y - 10, c.bw * 0.45, 10, r=4)
        p.fill(bl, T("cloth.plum"), "base")
        p.edge(bl, T("cloth.plum"), 1.0)
    else:
        head = Path().move(c.x0 + 6, c.g - c.bh * 0.35).line(c.x0 + 6, c.y0 + 18).quad(c.mx, c.y0 - 6, c.x1 - 6, c.y0 + 18) \
            .line(c.x1 - 6, c.g - c.bh * 0.35).close()
        hm = p.fill(head, wood, "base")
        p.tone(hm - hm.shift(2, 2), wood, "light")
        p.fill(Path.rect(c.x0 + 18, c.y0 + 26, c.bw - 36, c.bh * 0.25, r=6), wood, "terminator")
        p.edge(head, wood, 1.3)
        mat = Path.rect(c.x0 + 2, c.g - c.bh * 0.45, c.bw - 4, c.bh * 0.12, r=6)
        p.fill(mat, T("cloth.cream"), "light")
        p.edge(mat, T("cloth.cream"), 1.1)
        pil = Path.ellipse(c.mx - c.bw * 0.18, c.g - c.bh * 0.47, c.bw * 0.22, 8)
        p.fill(pil, T("cloth.cream"), "highlight")
        p.edge(pil, T("cloth.cream"), 1.0)
        qt = T("cloth.blue")
        quilt = Path.rect(c.x0 - 2 + c.bw * 0.15, c.g - c.bh * 0.4, c.bw * 0.85 + 2, c.bh * 0.32, r=4)
        qm = p.fill(quilt, qt, "base")
        for k in range(1, 4):
            x = c.x0 + c.bw * 0.15 + k * c.bw * 0.21
            p.stroke(Path.polyline([(x, c.g - c.bh * 0.4), (x, c.g - c.bh * 0.1)]), 1.0, qt, "deep", clip=qm)
        p.tone(qm - qm.shift(0, 2), qt, "light")
        p.edge(quilt, qt, 1.2)
        for lx in (c.x0 + 4, c.x1 - 12):
            leg(p, lx, c.g - c.bh * 0.1, 8, c.bh * 0.1, wood)
    p.ground_shadow(c.x0, c.x1, c.g + 1, 6, 0.3)
    c.L.place(p)


def _shelf_things(p, x0, x1, sy, kind, i0, seed, h):
    cx = x0
    i = i0
    while cx < x1:
        if kind == "relic":
            g = T("gold")
            what = ("orb", "cup", "box", "orb")[i % 4]
            if what == "orb":
                p.fill(Path.circle(cx + 7, sy - 12, 7), T("#7fb8ff"), "base")
                p.fill(Path.circle(cx + 5, sy - 14, 2.4), T("#e8f6ff"), "base")
                p.fill(Path.rect(cx + 2, sy - 7, 10, 3), g, "base")
                cx += 20
            elif what == "cup":
                cup = Path.polygon([(cx, sy - 26), (cx + 16, sy - 26), (cx + 11, sy - 14), (cx + 5, sy - 14)])
                p.fill(cup, g, "base")
                p.fill(Path.rect(cx + 6, sy - 14, 4, 9), g, "shadow")
                p.edge(cup, g, 1.0)
                cx += 22
            else:
                bx = Path.rect(cx, sy - 14, 18, 10, r=2)
                p.fill(bx, T("cloth.red"), "base")
                p.fill(Path.rect(cx, sy - 11, 18, 2), g, "base")
                p.edge(bx, T("cloth.red"), 1.0)
                cx += 24
        elif i % 3 == 2:
            bt = T(("#6fd0b0", "#d86fa0", "#7f9ff0")[i % 3])
            bp = Path.rect(cx, sy - 20, 9, 15, r=3)
            p.fill(bp, bt, "base")
            p.fill(Path.rect(cx + 3, sy - 24, 3, 5), bt, "shadow")
            p.edge(bp, bt, 0.9)
            cx += 14
        else:
            for j in range(3 + i % 3):
                bh = h + 6 * hash01(i, j, seed)
                bt = T(("cloth.red", "cloth.navy", "cloth.teal", "cloth.plum", "wood.light")[(i + j) % 5])
                bp = Path.rect(cx, sy - 5 - bh, 6, bh)
                p.fill(bp, bt, "base")
                p.fill(Path.rect(cx, sy - bh + 1, 6, 2), T("gold"), "base")
                p.edge(bp, bt, 0.8)
                cx += 6.5
            cx += 5
        i += 1


def p_cabinet(c):
    """Tall furniture against the wall: a wardrobe, a bookshelf, the vault's relic shelf, the
    kitchen's utensil rack, the smith's tool rack."""
    p = c.piece()
    n = c.name
    wood = T("wood.dark" if n in ("relic-shelf", "wardrobe") else "wood")
    x0, x1, y0 = c.x0 + 2, c.x1 - 2, c.y0 + 6
    if n in ("utensil-rack", "tool-rack"):
        for px in (x0 + 4, x1 - 10):
            leg(p, px, y0, 6, c.g - y0, wood)
        for yy in (y0 + 6, y0 + c.bh * 0.55):
            rail = Path.rect(x0, yy, x1 - x0, 7, r=2)
            p.fill(rail, wood, "light")
            p.edge(rail, wood, 1.0)
        iron = T("#8d93a8")
        for i in range(4):
            hx = x0 + 16 + i * (x1 - x0 - 32) / 3
            if n == "utensil-rack":
                p.stroke(Path.polyline([(hx, y0 + 12), (hx, y0 + c.bh * 0.35)]), 2.0, iron, "base")
                shape = Path.circle(hx, y0 + c.bh * 0.38, 7) if i % 2 else Path.ellipse(hx, y0 + c.bh * 0.38, 4, 7)
                p.fill(shape, iron, "base")
                p.edge(shape, iron, 1.0)
            else:
                p.stroke(Path.polyline([(hx, y0 + 12), (hx, y0 + c.bh * 0.42)]), 3.0, wood, "base")
                head = (Path.rect(hx - 9, y0 + 12, 18, 8, r=2), Path.polygon([(hx - 5, y0 + 12), (hx + 5, y0 + 12), (hx + 2, y0 + 26), (hx - 2, y0 + 26)]),
                        Path.rect(hx - 3, y0 + c.bh * 0.3, 6, 14), Path.rect(hx - 8, y0 + 12, 16, 6))[i]
                p.fill(head, iron, "base")
                p.edge(head, iron, 1.0)
    else:
        body = Path.rect(x0, y0, x1 - x0, c.g - y0)
        m = p.fill(body, wood, "base")
        p.tone(p.where(m, lambda xx, yy: xx > x1 - 8), wood, "shadow")
        if n == "wardrobe":
            half = (x1 - x0 - 12) / 2
            for k in range(2):
                dx = x0 + 6 + k * half
                door_ = Path.rect(dx + 2, y0 + 12, half - 4, c.g - y0 - 28, r=3)
                p.fill(door_, wood, "terminator")
                p.fill(Path.rect(dx + 8, y0 + 22, half - 16, (c.g - y0) * 0.3, r=3), wood, "base")
                p.edge(door_, wood, 1.0)
            for hx in (c.mx - 6, c.mx + 6):
                p.fill(Path.circle(hx, y0 + (c.g - y0) * 0.5, 2.4), T("brass"), "base")
        else:
            inner = Path.rect(x0 + 8, y0 + 10, x1 - x0 - 16, c.g - y0 - 22)
            p.fill(inner, wood, "deep")
            for k in range(3):
                sy = y0 + 10 + (c.g - y0 - 22) * (k + 1) / 3
                p.fill(Path.rect(x0 + 6, sy - 5, x1 - x0 - 12, 6), wood, "light")
                _shelf_things(p, x0 + 12, x1 - 18, sy - 5, "relic" if n == "relic-shelf" else "books", k, c.seed, (c.g - y0) * 0.14)
        cap = Path.rect(x0 - 3, y0 - 4, x1 - x0 + 6, 10, r=2)
        p.fill(cap, wood, "light")
        p.edge(cap, wood, 1.1)
        p.edge(body, wood, 1.4)
    p.ground_shadow(c.x0, c.x1, c.g + 1, 6, 0.3)
    c.L.place(p)


def p_counter(c):
    """Counters: the shop's (a ledger and a pair of scales on it) and the tavern's long bar
    (tankards and bottles)."""
    p = c.piece()
    wood = T("wood")
    top = c.y0 + c.bh * 0.32
    front = Path.rect(c.x0 + 6, top + 10, c.bw - 12, c.g - top - 10)
    fm = p.fill(front, wood, "base")
    pw = 64
    for i in range(int((c.bw - 12) / pw)):
        pm = p.shape(Path.rect(c.x0 + 14 + i * pw, top + 22, pw - 16, c.g - top - 40, r=3)) & fm
        p.tone(pm, wood, "terminator")
        p.tone(pm - pm.shift(2, 2), wood, "deep-shadow")
    p.edge(front, wood, 1.3)
    table_top(p, c.x0, c.x1, top + 10, wood, 10, 10)
    if c.name == "bar-counter":
        for i in range(5):
            tx = c.x0 + 40 + i * (c.bw - 120) / 4
            tk = T("pottery" if i % 2 else "brass")
            tp = Path.rect(tx - 7, top - 14, 14, 18, r=2)
            p.fill(tp, tk, "base")
            p.fill(Path.rect(tx - 7, top - 16, 14, 4, r=2), T("cloth.cream"), "base")
            p.edge(tp, tk, 1.0)
        for i in range(3):
            bt = T(("#5a8a4a", "#7a2a4a", "#3a5a8a")[i])
            bx = c.x1 - 70 + i * 16
            bp = Path().move(bx - 3, top - 34).line(bx + 3, top - 34).line(bx + 3, top - 24).quad(bx + 7, top - 20, bx + 7, top - 2) \
                .line(bx - 7, top - 2).quad(bx - 7, top - 20, bx - 3, top - 24).close()
            p.fill(bp, bt, "base")
            p.edge(bp, bt, 0.9)
    else:
        led = Path.polygon([(c.mx - 40, top - 2), (c.mx - 4, top - 6), (c.mx - 2, top + 2), (c.mx - 38, top + 4)])
        p.fill(led, T("cloth.red"), "base")
        p.edge(led, T("cloth.red"), 1.0)
        b = T("brass")
        p.stroke(Path.polyline([(c.mx + 30, top - 30), (c.mx + 30, top)]), 2.4, b, "base")
        p.stroke(Path.polyline([(c.mx + 14, top - 28), (c.mx + 46, top - 28)]), 2.0, b, "base")
        for sx in (c.mx + 14, c.mx + 46):
            p.stroke(Path.polyline([(sx, top - 28), (sx, top - 18)]), 1.0, b, "deep")
            p.fill(Path.ellipse(sx, top - 16, 8, 3), b, "light")
    p.ground_shadow(c.x0, c.x1, c.g + 1, 6, 0.3)
    c.L.place(p)


def p_bench(c):
    """A park bench: slatted wood on curled iron legs."""
    p = c.piece()
    wood, iron = T("wood"), T("iron")
    seat_y = c.g - c.bh * 0.5
    for lx in (c.x0 + 12, c.x1 - 18):
        leg(p, lx, seat_y, 6, c.g - seat_y, iron)
    p.stroke(Path.polyline([(c.x0 + 15, c.y0 + 10), (c.x0 + 15, seat_y)]), 4, iron, "base")
    p.stroke(Path.polyline([(c.x1 - 15, c.y0 + 10), (c.x1 - 15, seat_y)]), 4, iron, "base")
    for yy in (c.y0 + 6, seat_y - 6):
        b = Path.rect(c.x0 + 4, yy, c.bw - 8, c.bh * 0.16, r=3)
        m = p.fill(b, wood, "base")
        p.tone(m - m.shift(0, 2), wood, "light")
        p.edge(b, wood, 1.1)
    p.ground_shadow(c.x0, c.x1, c.g + 1, 5, 0.3)
    c.L.place(p)


def p_chalkboard(c):
    """An A-frame chalkboard with the day's prices chalked on it."""
    p = c.piece()
    wood = T("wood")
    for a, b in (((c.x0 + 6, c.g), (c.mx - 2, c.y0 + 4)), ((c.x1 - 6, c.g), (c.mx + 2, c.y0 + 4))):
        p.stroke(Path.polyline([a, b]), 5, wood, "base")
    bd = Path.rect(c.x0 + 10, c.y0 + 10, c.bw - 20, c.bh * 0.62, r=2)
    p.fill(bd, T("#2f3a3e"), "base")
    p.edge(bd, wood, 2.6, "base")
    ch = T("#f0ece0")
    for k in range(4):
        y = c.y0 + 24 + k * 14
        p.stroke(Path.polyline([(c.x0 + 18, y), (c.x0 + 18 + (c.bw - 40) * (0.5 + 0.4 * hash01(k, 1, c.seed)), y)]), 1.6, ch, "base")
    c.L.place(p)


# --------------------------------------------------------------------------- #
# Lights (their flames are the game's TorchSprite, burning at PropLight.Flame)
# --------------------------------------------------------------------------- #

def p_lamp(c):
    """Street lamps: one lantern on a post, an arm with a lantern hanging off it, two lanterns on a
    crossbar, a short lantern post, a little lantern standing on the ground."""
    n = c.name
    p = c.piece()
    iron = T("iron")
    if n == "lantern-small":
        P.lantern_body(p, c.mx, c.y0 + 6, c.bh / 30.0, True)
        c.L.place(p)
        return
    top = c.y0 + 4
    post_top = top + (44 if n in ("lamp", "lantern-post") else 18)
    base = Path.polygon([(c.mx - 14, c.g), (c.mx - 10, c.g - 20), (c.mx - 6, c.g - 26), (c.mx + 6, c.g - 26), (c.mx + 10, c.g - 20), (c.mx + 14, c.g)])
    bm = p.fill(base, iron, "base")
    shade_lr(p, bm, iron, c.mx, 28)
    p.edge(base, iron, 1.2)
    post = Path.rect(c.mx - 4, post_top, 8, c.g - 26 - post_top)
    pm = p.fill(post, iron, "base")
    shade_lr(p, pm, iron, c.mx, 8, 0.25, 0.1)
    for yy in (c.g - 52, post_top + 8):
        p.fill(Path.rect(c.mx - 6, yy, 12, 5, r=2), iron, "base-light")
    p.edge(post, iron, 1.2)
    if n in ("lamp", "lantern-post"):
        P.lantern_body(p, c.mx, top, 1.5 if n == "lamp" else 1.3, True)
    elif n == "lamp-arm":
        ax = c.x1 - 18
        p.stroke(Path.smooth([(c.mx, post_top + 4), (c.mx + (ax - c.mx) * 0.5, post_top - 4), (ax, post_top + 2)]), 3.0, iron, "base")
        p.stroke(Path.circle(c.mx + (ax - c.mx) * 0.35, post_top + 8, 5), 1.8, iron, "base")
        p.stroke(Path.polyline([(ax, post_top + 2), (ax, post_top + 12)]), 1.6, iron, "base")
        P.lantern_body(p, ax, post_top + 12, 1.3, True)
    else:
        p.stroke(Path.polyline([(c.x0 + 14, post_top + 2), (c.x1 - 14, post_top + 2)]), 3.0, iron, "base")
        for lx in (c.x0 + 16, c.x1 - 16):
            P.lantern_body(p, lx, post_top + 4, 1.15, True)
        P._finial(p, c.mx, post_top - 2, T("gold"))
    c.L.place(p)


def p_lantern_hanging(c):
    """A ship's lantern hanging from an iron hook on a post; the light's flame burns in its glass."""
    p = c.piece()
    iron, wood = T("iron"), T("wood")
    fx = c.cx + 3 * K
    fy = c.H - 24 * K
    px = c.x0 + 6
    post = Path.rect(px, c.y0 + 2, 10, c.g - c.y0 - 2)
    p.fill(post, wood, "base")
    p.edge(post, wood, 1.1)
    p.stroke(Path.smooth([(px + 8, c.y0 + 10), (fx - 4, c.y0 + 4), (fx, c.y0 + 14)]), 2.4, iron, "base")
    s = max(0.8, (fy + 18 - (c.y0 + 14)) / 24.0)
    P.lantern_body(p, fx, c.y0 + 14, s, True)
    c.L.place(p)


def p_sconce(c):
    """A torch in an iron (or, in the vault, gilded) bracket on the wall: the handle's cup is where
    the light's flame stands."""
    p = c.piece()
    metal = T("gold" if c.name.startswith("vault") else "iron")
    cup_y = c.H - 14 * K
    plate = Path.rect(c.cx - 7, cup_y + 18, 14, 22, r=3)
    p.fill(plate, metal, "base")
    p.edge(plate, metal, 1.1)
    ht = T("wood")
    handle = Path.polygon([(c.cx - 4, cup_y + 4), (c.cx + 4, cup_y + 4), (c.cx + 2.5, c.g - 4), (c.cx - 2.5, c.g - 4)])
    hm = p.fill(handle, ht, "base")
    shade_lr(p, hm, ht, c.cx, 8, 0.3, 0.1)
    p.edge(handle, ht, 1.0)
    cup = Path.polygon([(c.cx - 9, cup_y - 2), (c.cx + 9, cup_y - 2), (c.cx + 5, cup_y + 8), (c.cx - 5, cup_y + 8)])
    p.fill(cup, metal, "base")
    p.edge(cup, metal, 1.1)
    p.fill(Path.ellipse(c.cx, cup_y - 2, 8, 3), T("#ff9a48"), "light")
    p.glow(c.cx, cup_y - 6, 30, "#ffb060", 0.35)
    c.L.place(p, reach=1.8)


def p_brazier(c):
    """A brazier (an iron bowl on three legs) or the vault's gilded one on a stone pedestal: coals
    glowing in the bowl, whose rim is where the light's flame stands."""
    p = c.piece()
    vault = c.name.startswith("vault")
    metal = T("gold" if vault else "iron")
    rim = c.H - (20 if vault else 21) * K
    if vault:
        ped = Path.polygon([(c.cx - 14, c.g), (c.cx - 8, rim + 30), (c.cx + 8, rim + 30), (c.cx + 14, c.g)])
        m = p.fill(ped, T("stone"), "base")
        shade_lr(p, m, T("stone"), c.cx, 28)
        p.edge(ped, T("stone"), 1.2)
    else:
        for lx in (-1, 0, 1):
            p.stroke(Path.polyline([(c.cx + lx * c.bw * 0.4, c.g), (c.cx + lx * 6, rim + 18)]), 4.0, metal, "base")
    bowl = Path().move(c.cx - c.bw * 0.48, rim).quad(c.cx - c.bw * 0.4, rim + 28, c.cx, rim + 30).quad(c.cx + c.bw * 0.4, rim + 28, c.cx + c.bw * 0.48, rim).close()
    bm = p.fill(bowl, metal, "base")
    shade_lr(p, bm, metal, c.cx, c.bw)
    p.tone(bm - bm.shift(0, 2), metal, "light")
    p.edge(bowl, metal, 1.3)
    flame_bed(p, c.cx, rim + 4, c.bw * 0.8, c.seed)
    c.L.place(p)


def p_fire(c):
    """A campfire (logs crossed in a ring of stones) or the camp's cookfire (a pot hanging over it
    from a tripod); the light's flame stands on the coals."""
    p = c.piece()
    cook = c.name == "cookfire"
    bed_y = c.H - (6 if cook else 4) * K
    st = T("stone.grey")
    w = c.bw * (0.8 if cook else 0.9)
    if cook:
        iron = T("iron")
        top = c.y0 + 6
        for lx in (c.x0 + 6, c.x1 - 6):
            p.stroke(Path.polyline([(lx, c.g), (c.cx, top)]), 3.2, T("wood"), "base")
        p.stroke(Path.polyline([(c.cx, top), (c.cx, top + c.bh * 0.35)]), 1.6, iron, "base")
        pot = Path().move(c.cx - 16, top + c.bh * 0.35).line(c.cx + 16, top + c.bh * 0.35).quad(c.cx + 18, top + c.bh * 0.6, c.cx, top + c.bh * 0.62) \
            .quad(c.cx - 18, top + c.bh * 0.6, c.cx - 16, top + c.bh * 0.35).close()
        pm = p.fill(pot, iron, "base")
        shade_lr(p, pm, iron, c.cx, 32)
        p.edge(pot, iron, 1.2)
    logs = T("bark")
    for a, b in (((c.cx - w * 0.38, bed_y + 6), (c.cx + w * 0.28, bed_y - 6)), ((c.cx + w * 0.38, bed_y + 6), (c.cx - w * 0.28, bed_y - 6))):
        p.stroke(Path.polyline([a, b]), 8, logs, "base")
        p.stroke(Path.polyline([a, b]).translate(-0.8, -2.0), 2.2, logs, "light")
    flame_bed(p, c.cx, bed_y + 4, w * 0.6, c.seed)
    n = 7
    for i in range(n):
        x = c.cx - w / 2 + w * (i + 0.5) / n
        stone_block(p, Path.ellipse(x, c.g - 5, w / n * 0.62, 6), st)
    c.L.place(p, reach=1.8)


def p_candle_stand(c):
    """A floor candelabrum: a brass stem on a spreading foot, three candles whose wicks are where
    the light's flame burns."""
    p = c.piece()
    b = T("brass")
    wick = c.H - 30 * K
    foot = Path.polygon([(c.cx - c.bw * 0.4, c.g), (c.cx - 5, c.g - 14), (c.cx + 5, c.g - 14), (c.cx + c.bw * 0.4, c.g)])
    p.fill(foot, b, "base")
    p.edge(foot, b, 1.1)
    stem = Path.rect(c.cx - 3, wick + 34, 6, c.g - 14 - wick - 34)
    p.fill(stem, b, "base")
    p.edge(stem, b, 1.0)
    for yy in (c.g - 40, wick + 60):
        p.fill(Path.ellipse(c.cx, yy, 7, 4), b, "light")
    p.stroke(Path.smooth([(c.cx - c.bw * 0.34, wick + 26), (c.cx, wick + 40), (c.cx + c.bw * 0.34, wick + 26)]), 3.0, b, "base")
    for k, dx in enumerate((-c.bw * 0.34, 0.0, c.bw * 0.34)):
        top = wick + (6 if k == 1 else 12)
        cp = Path.rect(c.cx + dx - 3.5, top, 7, wick + 26 - top)
        p.fill(cp, T("cloth.cream"), "base")
        p.edge(cp, T("cloth.cream"), 0.9)
        p.stroke(Path.polyline([(c.cx + dx, top), (c.cx + dx, top - 4)]), 1.2, T("wood.dark"), "base")
        if k != 1:
            p.fill(Path.ellipse(c.cx + dx, top - 8, 2.6, 5), T("#ffcf6a"), "light")
            p.glow(c.cx + dx, top - 8, 22, "#ffc070", 0.3)
    c.L.place(p, reach=1.8)


# --------------------------------------------------------------------------- #
# Banners, boards and signs
# --------------------------------------------------------------------------- #

def p_banner_stand(c):
    """A banner on its own frame: two posts with gold finials, a crossbar, and the banner (cream with
    a blue diamond, or rose with a gold one)."""
    p = c.piece()
    wood = T("wood.dark")
    for px in (c.x0 + 6, c.x1 - 12):
        leg(p, px, c.y0 + 14, 7, c.g - c.y0 - 14, wood)
        P._finial(p, px + 3.5, c.y0 + 16, T("gold"))
    bar = Path.rect(c.x0 + 2, c.y0 + 18, c.bw - 4, 6, r=2)
    p.fill(bar, wood, "light")
    p.edge(bar, wood, 1.0)
    colour = "cloth.cream" if c.name == "banner-blue" else "cloth.pink"
    P.banner(c.mx, c.y0 + 24, c.bw * 0.62, c.bh * 0.62, colour, "diamond", pole=False)(p)
    c.L.place(p)


def p_wall_cloth(c):
    """Hung on the wall: the sun banner (navy, a gold sun) or a tapestry (rose, a gold diamond, fringed)."""
    p = c.piece()
    if c.name == "sun-banner":
        P.banner(c.mx, c.y0 + 8, c.bw * 0.8, c.bh - 12, "cloth.navy", "sun")(p)
    else:
        rod = Path.rect(c.x0, c.y0 + 2, c.bw, 6, r=2)
        p.fill(rod, T("wood.dark"), "base")
        t = T("cloth.pink")
        cloth = Path.rect(c.x0 + 6, c.y0 + 8, c.bw - 12, c.bh - 22)
        m = p.fill(cloth, t, "base")
        p.tone(p.where(m, lambda xx, yy: xx > c.x1 - c.bw * 0.3), t, "shadow")
        for k in range(3):
            fx = c.x0 + 6 + (c.bw - 12) * (k + 1) / 4
            p.stroke(Path.polyline([(fx, c.y0 + 10), (fx + 1, c.g - 16)]), (3.0, 1.0), t, "terminator", clip=m)
        p.tone(m - m.inset(5), T("gold"), "base")
        P._icon(p, "diamond", c.mx, c.y0 + (c.bh - 14) * 0.5, c.bw * 0.5)
        for k in range(int((c.bw - 12) / 6)):
            fx = c.x0 + 9 + k * 6
            p.stroke(Path.polyline([(fx, c.g - 14), (fx, c.g - 6)]), 1.6, T("gold"), "base")
        p.edge(cloth, t, 1.2)
        p.edge(rod, T("wood.dark"), 1.0)
    c.L.place(p)


def p_board(c):
    """The quest board (a little roofed board of pinned notices), a plain sign on a post, the
    rangers' rustic sign, signposts with arrows."""
    p = c.piece()
    n = c.name
    wood = T("wood")
    if n == "notice-board":
        for px in (c.x0 + 8, c.x1 - 16):
            leg(p, px, c.y0 + 20, 8, c.g - c.y0 - 20, wood)
        board(p, c.x0 + 4, c.y0 + 26, c.bw - 8, c.bh * 0.5, wood)
        cols = (T("cloth.cream"), T("#f2e2b0"), T("#e8ece0"))
        nw = (c.bw - 40) / 3
        for k in range(7):
            col = k % 3
            nx = c.x0 + 14 + col * (c.bw - 28) / 3 + 4 * hash01(k, 1, c.seed)
            ny = c.y0 + 34 + (k // 3) * 24 + 4 * hash01(k, 2, c.seed)
            note = Path.rect(nx, ny, nw, 20, r=1)
            p.fill(note, cols[col], "base")
            for j in range(2):
                p.stroke(Path.polyline([(nx + 4, ny + 7 + j * 6), (nx + nw - 5, ny + 7 + j * 6)]), 1.0, T("wood.dark"), "base")
            p.fill(Path.circle(nx + nw / 2, ny + 2, 2.2), T("cloth.red"), "base")
            p.edge(note, cols[col], 0.9)
        roof_ = Path.polygon([(c.x0, c.y0 + 28), (c.mx, c.y0 + 4), (c.x1, c.y0 + 28), (c.x1 - 6, c.y0 + 30), (c.mx, c.y0 + 12), (c.x0 + 6, c.y0 + 30)])
        rt = T("roof.blue")
        rm = p.fill(roof_, rt, "base")
        p.tone(p.where(rm, lambda xx, yy: xx > c.mx), rt, "shadow")
        p.edge(roof_, rt, 1.2)
    elif n in ("sign", "ranger-sign"):
        rustic = n == "ranger-sign"
        pt = T("bark" if rustic else "wood")
        leg(p, c.mx - 4, c.y0 + c.bh * 0.3, 8, c.g - c.y0 - c.bh * 0.3, pt)
        bh_ = c.bh * 0.42
        if rustic:
            bp = Path.smooth([(c.x0 + 3, c.y0 + 12), (c.mx, c.y0 + 6), (c.x1 - 3, c.y0 + 14), (c.x1 - 6, c.y0 + bh_), (c.mx, c.y0 + bh_ + 6), (c.x0 + 5, c.y0 + bh_)], closed=True)
            bt = T("wood.light")
            m = p.fill(bp, bt, "base")
            p.tone(m - m.shift(2, 2), bt, "light")
            p.edge(bp, bt, 1.3)
            p.fill(Path.ellipse(c.mx, c.y0 + bh_ * 0.6, 8, 4, rot=-20), T("leaf"), "base")
            p.stroke(Path.polyline([(c.mx - 12, c.y0 + bh_ * 0.6 + 6), (c.mx + 12, c.y0 + bh_ * 0.6 + 6)]), 1.4, T("wood.dark"), "base")
        else:
            board(p, c.x0 + 3, c.y0 + 8, c.bw - 6, bh_, wood)
            for j in range(2):
                p.stroke(Path.polyline([(c.x0 + 12, c.y0 + 20 + j * 10), (c.x1 - 12, c.y0 + 20 + j * 10)]), 1.2, T("wood.dark"), "base")
    else:
        leg(p, c.mx - 5, c.y0 + 8, 10, c.g - c.y0 - 8, T("wood"))
        if n == "signpost":
            stone_block(p, Path.rect(c.mx - 14, c.g - 22, 28, 22, r=2), T("stone"))
        cols = ("cloth.teal", "cloth.blue", "wood.light")
        for k in range(2 if n == "signpost" else 3):
            y = c.y0 + 14 + k * c.bh * 0.17
            ln = c.bw * 0.46
            if k % 2 == 0:
                x0 = c.mx - 6
                ar = Path.polygon([(x0, y), (x0 + ln - 12, y), (x0 + ln, y + 9), (x0 + ln - 12, y + 18), (x0, y + 18)])
            else:
                x0 = c.mx + 6 - ln
                ar = Path.polygon([(x0 + ln, y), (x0 + 12, y), (x0, y + 9), (x0 + 12, y + 18), (x0 + ln, y + 18)])
            at = T(cols[k % 3])
            m = p.fill(ar, at, "base")
            p.tone(m - m.shift(0, 2), at, "light")
            p.edge(ar, at, 1.2)
    p.ground_shadow(c.mx - 18, c.mx + 18, c.g + 1, 5, 0.3)
    c.L.place(p)


# --------------------------------------------------------------------------- #
# The docks and the ship
# --------------------------------------------------------------------------- #

def p_bollard(c):
    """An iron mooring bollard with a rope round its neck."""
    p = c.piece()
    iron = T("#4f5266")
    w = c.bw * 0.86
    body = Path().move(c.mx - w * 0.42, c.g).line(c.mx - w * 0.36, c.y0 + c.bh * 0.35).quad(c.mx - w * 0.5, c.y0 + c.bh * 0.25, c.mx - w * 0.5, c.y0 + c.bh * 0.12) \
        .quad(c.mx, c.y0, c.mx + w * 0.5, c.y0 + c.bh * 0.12).quad(c.mx + w * 0.5, c.y0 + c.bh * 0.25, c.mx + w * 0.36, c.y0 + c.bh * 0.35) \
        .line(c.mx + w * 0.42, c.g).close()
    m = p.fill(body, iron, "base")
    shade_lr(p, m, iron, c.mx, w, 0.25, 0.12)
    p.edge(body, iron, 1.3)
    p.stroke(Path.ellipse(c.mx, c.y0 + c.bh * 0.42, w * 0.42, 5), 3.4, T("rope"), "base")
    p.ground_shadow(c.mx - w * 0.6, c.mx + w * 0.6, c.g + 1, 5, 0.3)
    c.L.place(p)


def post(p, x, top, ground, w, t):
    pp = Path.rect(x - w / 2, top, w, ground - top)
    m = p.fill(pp, t, "base")
    shade_lr(p, m, t, x, w, 0.25, 0.12)
    p.fill(Path.rect(x - w / 2 - 1, top - 3, w + 2, 6, r=2), t, "light")
    p.edge(pp, t, 1.2)


def p_posts_and_line(c):
    """Two posts and what hangs between them: a fishing net drying, a rope barrier, washing on a line."""
    p = c.piece()
    n = c.name
    wood = T("wood")
    top = c.y0 + 6
    xa, xb = c.x0 + 8, c.x1 - 8
    post(p, xa, top, c.g, 9, wood)
    post(p, xb, top, c.g, 9, wood)
    if n == "net-rack":
        p.stroke(Path.polyline([(xa, top + 4), (xb, top + 4)]), 4.0, wood, "base")
        net = Path.polygon([(xa + 4, top + 6), (xb - 4, top + 6), (xb - 14, c.g - c.bh * 0.25), (xa + 18, c.g - c.bh * 0.18)])
        nt = T("#c9b48a")
        nm = p.shape(net)
        mesh = Path()
        for k in range(14):
            x = xa + (xb - xa) * k / 13
            mesh.add(Path.polyline([(x, top + 6), (x + 18, c.g)]))
            mesh.add(Path.polyline([(x, top + 6), (x - 18, c.g)]))
        p.stroke(mesh, 1.4, nt, "base", clip=nm)
        p.stroke(Path.polyline([(xa + 18, c.g - c.bh * 0.18), (xb - 14, c.g - c.bh * 0.25)]), 2.4, nt, "shadow")
        for k in range(3):
            p.fill(Path.circle(xa + 30 + k * 30, c.g - c.bh * 0.2 - k * 2, 4), T("cloth.red"), "base")
    elif n == "rope-posts":
        p.stroke(Path().move(xa, top + 8).quad(c.mx, top + c.bh * 0.55, xb, top + 8), 4.2, T("cloth.red"), "base")
        for x in (xa, xb):
            p.fill(Path.circle(x, top - 2, 6), T("brass"), "base")
    else:
        rope = Path().move(xa, top + 4).quad(c.mx, top + 24, xb, top + 4)
        p.stroke(rope, 1.6, T("rope"), "deep")
        cols = ("cloth.cream", "cloth.pink", "cloth.blue", "flower.white")
        for k in range(4):
            u = (k + 0.6) / 4.4
            (x, y), _ = rope.at(u)
            w = (xb - xa) / 5.6
            h = c.bh * (0.36 + 0.14 * hash01(k, 1, c.seed))
            cloth = Path.polygon([(x - w / 2, y), (x + w / 2, y + 1), (x + w / 2 - 2, y + h), (x - w / 2 + 2, y + h - 3)])
            ct = T(cols[k])
            m = p.fill(cloth, ct, "base")
            p.tone(p.where(m, lambda xx, yy, a=x: xx > a + w * 0.15), ct, "shadow")
            p.edge(cloth, ct, 1.1)
            for pegx in (x - w / 2 + 3, x + w / 2 - 3):
                p.fill(Path.rect(pegx - 1.5, y - 4, 3, 7), T("wood"), "base")
    p.ground_shadow(c.x0, c.x1, c.g + 1, 5, 0.25)
    c.L.place(p)


def p_harbour_bell(c):
    """The harbour bell: a bronze bell hung in a little wooden frame with a pitched roof."""
    p = c.piece()
    wood = T("wood")
    xa, xb = c.x0 + 8, c.x1 - 8
    top = c.y0 + 16
    post(p, xa, top, c.g, 8, wood)
    post(p, xb, top, c.g, 8, wood)
    p.stroke(Path.polyline([(xa, top + 6), (xb, top + 6)]), 5.0, wood, "base")
    roof_ = Path.polygon([(c.x0, top + 4), (c.mx, c.y0), (c.x1, top + 4)])
    rt = T("roof.blue")
    rm = p.fill(roof_, rt, "base")
    p.tone(p.where(rm, lambda xx, yy: xx > c.mx), rt, "shadow")
    p.edge(roof_, rt, 1.2)
    b = T("#c08a3a")
    bell = Path().move(c.mx - 6, top + 12).quad(c.mx - 12, top + 18, c.mx - 13, top + 40).quad(c.mx - 18, top + 44, c.mx - 19, top + 48) \
        .line(c.mx + 19, top + 48).quad(c.mx + 18, top + 44, c.mx + 13, top + 40).quad(c.mx + 12, top + 18, c.mx + 6, top + 12).close()
    m = p.fill(bell, b, "base")
    shade_lr(p, m, b, c.mx, 30, 0.2, 0.15)
    p.tone(m & p.shape(Path.ellipse(c.mx - 6, top + 26, 3, 9)), b, "highlight")
    p.edge(bell, b, 1.3)
    p.stroke(Path.polyline([(c.mx + 4, top + 48), (c.mx + 8, c.g - c.bh * 0.25)]), 1.4, T("rope"), "base")
    p.ground_shadow(c.x0, c.x1, c.g + 1, 5, 0.3)
    c.L.place(p)


def p_ship_wheel(c):
    """The ship's wheel on its pedestal."""
    p = c.piece()
    wood = T("wood")
    ped = Path.polygon([(c.mx - 12, c.g), (c.mx - 8, c.g - c.bh * 0.42), (c.mx + 8, c.g - c.bh * 0.42), (c.mx + 12, c.g)])
    m = p.fill(ped, wood, "base")
    shade_lr(p, m, wood, c.mx, 20)
    p.edge(ped, wood, 1.2)
    cx, cy = c.mx, c.y0 + c.bw * 0.45
    r = c.bw * 0.36
    for k in range(8):
        a = math.radians(k * 45 + 22.5)
        p.stroke(Path.polyline([(cx, cy), (cx + math.cos(a) * (r + 9), cy + math.sin(a) * (r + 9))]), 3.6, wood, "base")
        p.fill(Path.circle(cx + math.cos(a) * (r + 9), cy + math.sin(a) * (r + 9), 3.2), wood, "light")
    ring = p.shape(Path.circle(cx, cy, r)) - p.shape(Path.circle(cx, cy, r - 6))
    p.tone(ring, wood, "base")
    p.tone(ring - ring.shift(1.5, 1.5), wood, "light")
    p.edge_mask(ring, wood, 1.0)
    p.fill(Path.circle(cx, cy, 6), T("brass"), "base")
    c.L.place(p)


def sail_cloth(p, x0, y0, x1, y1, full, emblem=True):
    """A square sail on its yard: bellied full (curving out and lit), or bunched up furled."""
    t = T("cloth.cream")
    if full:
        sp = Path().move(x0, y0).line(x1, y0).quad(x1 + (x1 - x0) * 0.06, (y0 + y1) / 2, x1 - 4, y1).quad((x0 + x1) / 2, y1 + 12, x0 + 4, y1) \
            .quad(x0 - (x1 - x0) * 0.06, (y0 + y1) / 2, x0, y0).close()
        m = p.fill(sp, t, "base")
        p.tone(p.where(m, lambda xx, yy: xx > x0 + (x1 - x0) * 0.72), t, "shadow")
        p.tone(p.where(m, lambda xx, yy: xx < x0 + (x1 - x0) * 0.2), t, "light")
        for k in range(1, 4):
            fx = x0 + (x1 - x0) * k / 4
            p.stroke(Path.polyline([(fx, y0 + 4), (fx + 3, y1 - 2)]), (2.4, 0.6), t, "terminator", clip=m)
        if emblem:
            em = Path.ellipse((x0 + x1) / 2, (y0 + y1) / 2, (x1 - x0) * 0.12, (y1 - y0) * 0.12)
            p.fill(em, T("cloth.navy"), "base")
            p.stroke(Path.polyline([((x0 + x1) / 2 - 8, (y0 + y1) / 2 - 3), ((x0 + x1) / 2, (y0 + y1) / 2 + 5), ((x0 + x1) / 2 + 8, (y0 + y1) / 2 - 3)]),
                     2.4, T("gold"), "base")
            p.edge(em, T("gold"), 1.4, "base")
        p.edge(sp, t, 1.3)
    else:
        n = 6
        for k in range(n):
            bx = x0 + (x1 - x0) * k / n
            b = Path.rect(bx, y0, (x1 - x0) / n + 1, y1 - y0, r=(y1 - y0) * 0.45)
            m = p.fill(b, t, "base")
            p.tone(p.below(m, y0 + (y1 - y0) * 0.6), t, "shadow")
            p.edge(b, t, 1.0)


def p_ship(c):
    """The Gull's Wager, side on, a little from above so her deck shows: a planked hull with her
    navy and gold stripe, the stern castle with lit windows and lanterns, the mast and its yard,
    rigging to the bowsprit and the stern. Seven frames as before: moored with her sail furled,
    sailing in under full sail (a wake at her stern), the sail coming in, moored again."""
    p = c.piece(pad=2)
    f = c.frame
    X = lambda v: v * K  # noqa: E731  (old art pixels to texels)
    wood = T("#c58a52")
    deck = T("#d9a46a")
    dark = T("wood.dark")
    navy, gold = T("cloth.navy"), T("gold")
    full = f in (1, 2)
    half = f in (3, 4, 5)
    if f in (1, 2, 3, 4, 5):
        foam = T("#eef4fa")
        for k in range(9):
            fx = X(242) + k * X(3.2)
            fy = X(100) + (hash01(k, f, c.seed) - 0.5) * X(14)
            r = X(2.2 + 2.0 * hash01(k, 2, c.seed)) * (1.0 - k / 12)
            m = p.fill(Path.circle(fx, fy, r), foam, "base")
            p.tone(m - m.shift(-1.5, -1.5), foam, "shadow")
        p.fill(Path.ellipse(X(244), X(108), X(12), X(5)), foam, "base")
    # rigging behind everything
    rig = T("#3a2a2a")
    p.stroke(Path.polyline([(X(122), X(6)), (X(9), X(52))]), 2.6, rig, "base")
    p.stroke(Path.polyline([(X(122), X(6)), (X(226), X(34))]), 2.6, rig, "base")
    p.stroke(Path.polyline([(X(122), X(14)), (X(100), X(48))]), 1.8, rig, "base")
    p.stroke(Path.polyline([(X(122), X(14)), (X(142), X(48))]), 1.8, rig, "base")
    # the mast and the yard
    mast = Path.rect(X(120), X(6), X(4), X(46))
    mm = p.fill(mast, wood, "base")
    shade_lr(p, mm, wood, X(122), X(4), 0.25, 0.1)
    p.edge(mast, wood, 1.2)
    p.fill(Path.rect(X(117.5), X(8), X(9), X(5), r=4), dark, "base")
    p.stroke(Path.polyline([(X(84), X(24)), (X(160), X(24))]), X(1.8), wood, "base")
    if full:
        sail_cloth(p, X(86), X(25), X(158), X(60), True)
    elif half:
        sail_cloth(p, X(88), X(25), X(156), X(42), True, emblem=False)
    else:
        sail_cloth(p, X(86), X(23), X(158), X(31), False)
    if f != 0 and f != 6:
        pen = Path.polygon([(X(123), X(3)), (X(138), X(1)), (X(134), X(5)), (X(140), X(8)), (X(123), X(8))])
        p.fill(pen, navy, "base")
        p.edge(pen, navy, 1.0)
    # the bowsprit
    p.stroke(Path.polyline([(X(40), X(60)), (X(8), X(51))]), X(2.2), wood, "base")
    p.fill(Path.circle(X(9), X(51), X(1.6)), gold, "base")
    # the hull: bow on the left, stern castle on the right
    hull = Path().move(X(34), X(56)).quad(X(90), X(52), X(150), X(48)).line(X(150), X(37)).line(X(234), X(36)) \
        .line(X(236), X(98)).quad(X(232), X(118), X(212), X(120)).line(X(82), X(120)).quad(X(48), X(114), X(34), X(56)).close()
    hm = p.fill(hull, wood, "base")
    for k in range(9):
        y = X(62 + k * 6.5)
        p.stroke(Path.polyline([(X(30), y - X(6) * (1 - k / 9)), (X(240), y)]), 1.2, wood, "terminator", clip=hm)
    p.tone(p.below(hm, X(104)), wood, "shadow")
    stripe = p.shape(Path.polygon([(X(40), X(88)), (X(240), X(90)), (X(240), X(95)), (X(44), X(93))])) & hm
    p.tone(stripe, navy, "base")
    gstripe = p.shape(Path.polygon([(X(40), X(86)), (X(240), X(88)), (X(240), X(89.5)), (X(40), X(87.5))])) & hm
    p.tone(gstripe, gold, "base")
    for k in range(3):
        p.fill(Path.circle(X(96 + k * 24), X(80), X(1.4)), gold, "base", clip=hm)
    # the deck, seen a little from above, and the rail along the near side
    dp = Path.polygon([(X(36), X(57)), (X(150), X(49)), (X(150), X(54)), (X(44), X(63))])
    p.fill(dp, deck, "light")
    rail = p.shape(Path().move(X(34), X(56)).quad(X(90), X(52), X(150), X(48)).line(X(150), X(51)).quad(X(90), X(55), X(36), X(59)).close())
    p.tone(rail, dark, "base")
    # the stern castle: its wall, windows lit, gold trim, lanterns
    castle = Path.rect(X(150), X(37), X(84), X(30))
    cm = p.fill(castle, wood, "base-light")
    for k in range(4):
        wx = X(166 + k * 16)
        win = Path.rect(wx, X(46), X(7), X(9), r=X(3))
        p.fill(win, T("glass.lit"), "base")
        p.edge(win, dark, 1.4, "base")
        p.glow(wx + X(3.5), X(50), X(9), "#ffcf7a", 0.18)
    p.stroke(Path.polyline([(X(150), X(37)), (X(234), X(36))]), X(1.6), gold, "base")
    p.stroke(Path.polyline([(X(150), X(64)), (X(236), X(64))]), X(1.2), gold, "base")
    p.edge(castle, wood, 1.4)
    del cm
    for lx, ly in ((X(226), X(26)), (X(214), X(58))):
        p.stroke(Path.polyline([(lx, ly + X(4)), (lx, ly + X(9))]), 2.0, dark, "base")
        P.lantern_body(p, lx, ly - X(2), 0.9, True)
    p.edge(hull, wood, 1.6)
    if f in (0, 6):
        for a, b in (((X(40), X(70)), (X(36), X(126))), ((X(232), X(100)), (X(244), X(126)))):
            p.stroke(Path.smooth([a, ((a[0] + b[0]) / 2 - X(3), (a[1] + b[1]) / 2), b]), 1.8, rig, "base")
    c.L.place(p, reach=2.4)


def p_gangplank(c):
    """The Gull's Wager's near rail (drawn in front of whoever's on her deck) and her gangplank:
    the rail curving up to the bow and running straight aft, striped navy and gold; the plank
    hanging down to the pier, drawn in, gone (the rail's gap), and coming down again."""
    p = c.piece(pad=2)
    f = c.frame
    X = lambda v: v * K  # noqa: E731
    wood = T("#c58a52")
    navy, gold = T("cloth.navy"), T("gold")
    gap = f == 2
    plank_end = {0: 143, 1: None, 2: None, 3: 109, 4: 121, 5: 133}[f]
    curve = Path().move(X(36), X(64)).quad(X(60), X(78), X(100), X(87.5))
    straight_x0 = X(102) if gap else X(96)
    for rail in ([curve, X(8)], [Path.polyline([(straight_x0, X(92.5)), (X(184), X(92.5))]), X(9)]):
        path, w = rail
        m = p.stroke(path, w, wood, "base")
        p.stroke(path.translate(0, -w * 0.32), w * 0.25, wood, "light")
        p.stroke(path.translate(0, w * 0.1), w * 0.28, navy, "base")
        p.stroke(path.translate(0, -w * 0.08), w * 0.07, gold, "base")
        p.edge_mask(m, wood, 1.3)
    if gap:
        curve_end = Path.polyline([(X(84), X(87)), (X(86), X(88))])
        p.stroke(curve_end, X(8), wood, "base")
    else:
        p.fill(Path.rect(X(88), X(84), X(14), X(12), r=X(2)), wood, "base")
    if plank_end:
        pl = Path.rect(X(89), X(92), X(11), X(plank_end) - X(92) - X(1), r=X(2))
        m = p.fill(pl, wood, "base")
        p.tone(p.where(m, lambda xx, yy: xx > X(96)), wood, "shadow")
        for k in range(int((plank_end - 96) / 7)):
            y = X(98 + k * 7)
            p.stroke(Path.polyline([(X(89), y), (X(100), y)]), 1.4, wood, "deep", clip=m)
        p.edge(pl, wood, 1.4)
    c.L.place(p, reach=2.2)


def p_mast(c):
    """The Gull's Wager's mainmast standing on her deck: the mast, its top, two yards with their
    sails furled, a pennant, the shrouds down to the deck."""
    p = c.piece()
    wood = T("#c58a52")
    rig = T("#3a2a2a")
    mx = c.mx
    for dx in (-c.bw * 0.4, c.bw * 0.4):
        p.stroke(Path.polyline([(mx, c.y0 + 40), (mx + dx, c.g - 10)]), 2.0, rig, "base")
    mast = Path.rect(mx - 7, c.y0 + 20, 14, c.g - c.y0 - 20)
    m = p.fill(mast, wood, "base")
    shade_lr(p, m, wood, mx, 14, 0.25, 0.1)
    p.edge(mast, wood, 1.3)
    p.fill(Path.rect(mx - 18, c.y0 + 30, 36, 14, r=4), T("wood.dark"), "base")
    for y, half in ((c.y0 + 0.24 * c.bh, c.bw * 0.42), (c.y0 + 0.44 * c.bh, c.bw * 0.48)):
        p.stroke(Path.polyline([(mx - half, y), (mx + half, y)]), 6, wood, "base")
        sail_cloth(p, mx - half + 6, y + 2, mx + half - 6, y + 26, False)
    pen = Path.polygon([(mx + 2, c.y0 + 4), (mx + 40, c.y0 + 2), (mx + 30, c.y0 + 12), (mx + 42, c.y0 + 22), (mx + 2, c.y0 + 20)])
    p.fill(pen, T("cloth.navy"), "base")
    p.edge(pen, T("cloth.navy"), 1.0)
    c.L.place(p)


# --------------------------------------------------------------------------- #
# Market stalls and carts
# --------------------------------------------------------------------------- #

def p_market_stall(c):
    """A market stall the size of the old one: posts, a striped awning with its valance, the stall
    keeper's shelves, a counter of goods (fish on ice; bolts of cloth; bread and jars), crates."""
    p = c.piece()
    n = c.name
    wood = T("wood")
    cols = {"fish-stall": ("cloth.teal", "cloth.cream"), "stall-cloth": ("cloth.plum", "cloth.cream"),
            "stall-teal": ("cloth.teal", "cloth.cream")}[n]
    x0, x1 = c.x0 + 8, c.x1 - 8
    top = c.y0 + 10
    counter = c.g - c.bh * 0.34
    for px in (x0 + 4, x1 - 4):
        post(p, px, top + 10, c.g, 9, wood)
    back = Path.rect(x0 + 10, top + 40, x1 - x0 - 20, counter - top - 40)
    bt = T(cols[1])
    p.fill(back, bt, "shadow")
    for k in range(2):
        sy = top + 40 + (counter - top - 40) * (k + 1) / 3
        p.fill(Path.rect(x0 + 12, sy, x1 - x0 - 24, 5), wood, "base")
        cx = x0 + 18
        i = 0
        while cx < x1 - 26:
            if n == "stall-cloth":
                ct = T(("cloth.pink", "cloth.blue", "cloth.gold", "cloth.teal", "cloth.red")[(i + k) % 5])
                bp = Path.rect(cx, sy - 24, 16, 24, r=3)
                p.fill(bp, ct, "base")
                p.tone(p.shape(bp) - p.shape(bp).shift(2, 0), ct, "shadow")
                p.edge(bp, ct, 1.0)
                cx += 19
            else:
                jt = T(("pottery", "terracotta", "#6fbfa0", "cloth.plum")[(i + k) % 4])
                jp = Path.rect(cx, sy - 18, 12, 18, r=3)
                p.fill(jp, jt, "base")
                p.edge(jp, jt, 1.0)
                cx += 17
            i += 1
    P.awning(x0 - 2, x1 + 2, top + 8, depth=30, colours=cols, valance=9)(p)
    if n == "fish-stall":
        P.lantern_body(p, x0 + 4, top + 44, 0.9, True)
    board_ = Path.rect(x0 - 4, counter - 6, x1 - x0 + 8, 10)
    p.fill(board_, wood, "light")
    p.edge(board_, wood, 1.1)
    goods_y = counter - 6
    if n == "fish-stall":
        ice = Path.rect(x0 + 2, goods_y - 10, x1 - x0 - 4, 12, r=4)
        p.fill(ice, T("#dff0fa"), "base")
        p.edge(ice, T("#a8c8e0"), 1.0)
        for k in range(5):
            fx = x0 + 18 + k * (x1 - x0 - 36) / 4
            fish = Path.ellipse(fx, goods_y - 12, 14, 5, rot=-10 + 8 * k)
            ft = T("fish")
            m = p.fill(fish, ft, "base")
            p.tone(p.below(m, goods_y - 12), ft, "shadow")
            p.fill(Path.polygon([(fx + 12, goods_y - 12), (fx + 20, goods_y - 18), (fx + 20, goods_y - 6)]), ft, "base")
            p.fill(Path.circle(fx - 8, goods_y - 13, 1.4), T("wood.dark"), "base")
            p.edge(fish, ft, 1.0)
    elif n == "stall-cloth":
        for k in range(4):
            bx = x0 + 12 + k * (x1 - x0 - 24) / 4
            bolt = Path.rect(bx, goods_y - 16, (x1 - x0 - 24) / 4 - 4, 16, r=7)
            ct = T(("cloth.red", "cloth.blue", "cloth.pink", "cloth.cream")[k])
            p.fill(bolt, ct, "base")
            p.edge(bolt, ct, 1.0)
    else:
        for k in range(3):
            P.basket(p, x0 + 26 + k * (x1 - x0 - 52) / 2, goods_y, 34, 13, ("bread", "orange", "pear")[k], c.seed + k)
    front = Path.rect(x0, counter + 4, x1 - x0, c.g - counter - 10)
    ft = T(cols[0])
    fm = p.fill(front, ft, "base")
    for k in range(1, 5):
        fx = x0 + (x1 - x0) * k / 5
        p.stroke(Path.polyline([(fx, counter + 6), (fx, c.g - 8)]), (3.0, 1.0), ft, "shadow", clip=fm)
    p.tone(p.above(fm, counter + 10), ft, "deep")
    p.edge(front, ft, 1.2)
    P.crate(None, x1 - 10, c.g, 34, 26, c.seed, piece=p, top_goods="apple" if n != "fish-stall" else None)
    p.ground_shadow(c.x0, c.x1, c.g + 1, 6, 0.3)
    c.L.place(p)


def wheel(p, cx, cy, r, t):
    ring = p.shape(Path.circle(cx, cy, r)) - p.shape(Path.circle(cx, cy, r - 4))
    for k in range(8):
        a = math.radians(k * 45)
        p.stroke(Path.polyline([(cx, cy), (cx + math.cos(a) * (r - 3), cy + math.sin(a) * (r - 3))]), 2.0, t, "base")
    p.tone(ring, t, "base")
    p.edge_mask(ring, t, 1.0)
    p.fill(Path.circle(cx, cy, 3.5), T("iron"), "base")


def p_cart(c):
    """A handcart (a box on one wheel, two shafts, sacks in it) or the flower seller's cart (heaped
    with flowers in boxes)."""
    p = c.piece()
    wood = T("wood")
    flowers = c.name == "flower-cart"
    r = c.bh * (0.32 if flowers else 0.3)
    bed_y = c.g - r * 1.4
    if flowers:
        P.bush(None, c.mx - c.bw * 0.05, bed_y - 6, c.bw * 0.78, c.bh * 0.32, "leaf", c.seed,
               flowers_on=("flower.pink", "flower.white", "flower.yellow", "blossom"), piece=p)
    else:
        P.sacks(None, c.mx - 4, bed_y - 4, 2, c.seed, piece=p)
    box = Path.rect(c.x0 + 8, bed_y - c.bh * 0.15, c.bw * 0.72, c.bh * 0.2, r=2)
    board(p, c.x0 + 8, bed_y - c.bh * 0.15, c.bw * 0.72, c.bh * 0.2, wood, grain=1, seed=c.seed)
    del box
    p.stroke(Path.polyline([(c.x0 + 8 + c.bw * 0.72, bed_y - 2), (c.x1 - 4, c.g - 8)]), 5, wood, "base")
    wheel(p, c.x0 + c.bw * 0.4, c.g - r, r, T("wood.dark"))
    p.ground_shadow(c.x0, c.x1, c.g + 1, 5, 0.3)
    c.L.place(p)


# --------------------------------------------------------------------------- #
# Stone, statues and the vaults
# --------------------------------------------------------------------------- #

def p_urn(c):
    """A clay urn (or a blue glazed one with a gold band): a round belly, a neck, a lip; shaded as
    a turned pot by style.shade."""
    import style
    p = c.piece()
    blue = c.name == "urn-blue"
    t = T("pottery" if blue else "terracotta")
    w = c.bw * 0.95
    h = c.bh - 4
    body = Path.smooth([(c.mx - w * 0.22, c.g), (c.mx - w * 0.48, c.g - h * 0.42), (c.mx - w * 0.3, c.g - h * 0.78), (c.mx - w * 0.2, c.g - h * 0.88),
                        (c.mx - w * 0.3, c.g - h), (c.mx + w * 0.3, c.g - h), (c.mx + w * 0.2, c.g - h * 0.88), (c.mx + w * 0.3, c.g - h * 0.78),
                        (c.mx + w * 0.48, c.g - h * 0.42), (c.mx + w * 0.22, c.g)], closed=True)
    m = p.shape(body)
    style.shade(m, "cloth", t, form="cylinder", edge=P.INNER, rim=False)
    if blue:
        p.stroke(Path.polyline([(c.mx - w * 0.5, c.g - h * 0.45), (c.mx + w * 0.5, c.g - h * 0.45)]), 3.0, T("gold"), "base", clip=m)
        for k in range(3):
            p.fill(Path.circle(c.mx - w * 0.2 + k * w * 0.2, c.g - h * 0.3, 2.4), T("cloth.cream"), "base", clip=m)
    p.ground_shadow(c.mx - w * 0.4, c.mx + w * 0.4, c.g + 1, 5, 0.3)
    c.L.place(p)


def plinth(p, cx, ground, w, h, t):
    path = Path.rect(cx - w / 2, ground - h, w, h, r=2)
    m = stone_block(p, path, t)
    cap = Path.rect(cx - w / 2 - 4, ground - h - 6, w + 8, 8, r=2)
    stone_block(p, cap, t, "base-light")
    return m


def p_pillar(c):
    """A marble pillar: a fluted shaft on a stepped base under a capital, both banded in gold."""
    p = c.piece()
    t = T("#f2eef4")
    g = T("gold")
    w = c.bw * 0.62
    base_h, cap_h = c.bh * 0.08, c.bh * 0.08
    plinth(p, c.mx, c.g, c.bw * 0.9, base_h, t)
    shaft = Path.rect(c.mx - w / 2, c.y0 + cap_h + 6, w, c.g - base_h - 6 - c.y0 - cap_h - 6)
    m = p.fill(shaft, t, "base")
    for k in range(1, 5):
        fx = c.mx - w / 2 + w * k / 5
        p.stroke(Path.polyline([(fx, c.y0 + cap_h + 8), (fx, c.g - base_h - 8)]), 1.4, t, "terminator", clip=m)
    shade_lr(p, m, t, c.mx, w, 0.3, 0.2)
    p.edge(shaft, t, 1.3)
    cap = Path.rect(c.x0 + 2, c.y0 + 2, c.bw - 4, cap_h + 4, r=3)
    stone_block(p, cap, t)
    for y in (c.y0 + cap_h + 4, c.g - base_h - 6):
        p.fill(Path.rect(c.mx - w / 2 - 2, y, w + 4, 4), g, "base")
    p.ground_shadow(c.mx - c.bw * 0.5, c.mx + c.bw * 0.5, c.g + 1, 5, 0.3)
    c.L.place(p)


def p_keep_arch(c):
    """The keep's arch: two piers of white stone and a round arch over the way through, gold on its
    keystone and capitals."""
    p = c.piece()
    st = T("#f2eef4")
    g = T("gold")
    pier = c.bw * 0.2
    span_top = c.y0 + 8
    arch = Path().move(c.x0 + 2, c.g).line(c.x0 + 2, span_top + c.bw * 0.3).quad(c.x0 + 2, span_top, c.mx, span_top) \
        .quad(c.x1 - 2, span_top, c.x1 - 2, span_top + c.bw * 0.3).line(c.x1 - 2, c.g).line(c.x1 - 2 - pier, c.g) \
        .line(c.x1 - 2 - pier, span_top + c.bw * 0.34).quad(c.x1 - 2 - pier, span_top + pier, c.mx, span_top + pier) \
        .quad(c.x0 + 2 + pier, span_top + pier, c.x0 + 2 + pier, span_top + c.bw * 0.34).line(c.x0 + 2 + pier, c.g).close()
    m = p.fill(arch, st, "base")
    p.tone(p.where(m, lambda xx, yy: xx > c.x1 - pier * 0.5), st, "shadow")
    for k in range(1, 8):
        a = math.radians(180 + 180 * k / 8)
        r0, r1 = c.bw / 2 - pier - 2, c.bw / 2 - 2
        cy = span_top + c.bw * 0.32
        p.stroke(Path.polyline([(c.mx + math.cos(a) * r0, cy + math.sin(a) * r0 * 0.75), (c.mx + math.cos(a) * r1, cy + math.sin(a) * r1 * 0.75)]),
                 1.2, st, "shadow", clip=m)
    p.edge(arch, st, 1.5)
    key = Path.polygon([(c.mx - 10, span_top - 2), (c.mx + 10, span_top - 2), (c.mx + 7, span_top + pier + 2), (c.mx - 7, span_top + pier + 2)])
    p.fill(key, g, "base")
    p.edge(key, g, 1.1)
    for px in (c.x0 + 2, c.x1 - 2 - pier):
        p.fill(Path.rect(px, span_top + c.bw * 0.34, pier, 6), g, "base")
    c.L.place(p)


def p_anvil(c):
    """The smith's anvil on its block, a hammer leaning on it."""
    p = c.piece()
    wood = T("wood.dark")
    iron = T("#7d8398")
    blk = Path.rect(c.mx - c.bw * 0.28, c.g - c.bh * 0.45, c.bw * 0.56, c.bh * 0.45, r=3)
    bm = p.fill(blk, wood, "base")
    shade_lr(p, bm, wood, c.mx, c.bw * 0.56)
    p.edge(blk, wood, 1.2)
    y = c.g - c.bh * 0.45
    s = c.bw
    body = Path.polygon([(c.mx - s * 0.5, y - s * 0.32), (c.mx + s * 0.3, y - s * 0.32), (c.mx + s * 0.48, y - s * 0.24), (c.mx + s * 0.18, y - s * 0.16),
                         (c.mx + s * 0.14, y - 6), (c.mx + s * 0.24, y), (c.mx - s * 0.24, y), (c.mx - s * 0.14, y - 6), (c.mx - s * 0.16, y - s * 0.18),
                         (c.mx - s * 0.38, y - s * 0.22)])
    m = p.fill(body, iron, "base")
    p.tone(p.below(m, y - s * 0.22), iron, "shadow")
    p.tone(m - m.shift(0, 2.5), iron, "highlight")
    p.edge(body, iron, 1.3)
    p.stroke(Path.polyline([(c.x1 - 8, c.g), (c.x1 - 18, c.g - c.bh * 0.55)]), 4, T("wood"), "base")
    p.fill(Path.rect(c.x1 - 28, c.g - c.bh * 0.62, 20, 10, r=2), iron, "base")
    p.ground_shadow(c.x0, c.x1, c.g + 1, 5, 0.3)
    c.L.place(p)


def p_throne(c):
    """The sun throne: a tall gold back with a sunburst behind it, a blue cushioned seat, on steps."""
    p = c.piece()
    g = T("gold")
    blue = T("cloth.blue")
    st = T("#f2eef4")
    step = Path.rect(c.x0 + 2, c.g - c.bh * 0.08, c.bw - 4, c.bh * 0.08, r=2)
    stone_block(p, step, st)
    sx, sy = c.mx, c.y0 + c.bh * 0.24
    for k in range(12):
        a = math.radians(k * 30 - 90)
        ray = Path.polygon([(sx + math.cos(a) * c.bw * 0.48, sy + math.sin(a) * c.bw * 0.48), (sx + math.cos(a + 0.2) * c.bw * 0.2, sy + math.sin(a + 0.2) * c.bw * 0.2),
                            (sx + math.cos(a - 0.2) * c.bw * 0.2, sy + math.sin(a - 0.2) * c.bw * 0.2)])
        p.fill(ray, g, "base" if k % 2 else "light")
    back = Path().move(c.mx - c.bw * 0.28, c.g - c.bh * 0.4).line(c.mx - c.bw * 0.28, c.y0 + c.bh * 0.22).quad(c.mx, c.y0 + c.bh * 0.02, c.mx + c.bw * 0.28, c.y0 + c.bh * 0.22) \
        .line(c.mx + c.bw * 0.28, c.g - c.bh * 0.4).close()
    bm = p.fill(back, g, "base")
    inner = p.shape(Path.rect(c.mx - c.bw * 0.2, c.y0 + c.bh * 0.22, c.bw * 0.4, c.bh * 0.36, r=10))
    p.tone(inner, blue, "base")
    for k in range(6):
        p.fill(Path.circle(c.mx + (k % 2 - 0.5) * c.bw * 0.16, c.y0 + c.bh * (0.3 + 0.05 * k), 1.8), g, "light", clip=inner)
    shade_lr(p, bm - inner, g, c.mx, c.bw * 0.56, 0.32, 0.2)
    p.edge(back, g, 1.4)
    seat = Path.rect(c.mx - c.bw * 0.34, c.g - c.bh * 0.44, c.bw * 0.68, c.bh * 0.1, r=6)
    p.fill(seat, blue, "light")
    p.edge(seat, blue, 1.2)
    front = Path.rect(c.mx - c.bw * 0.32, c.g - c.bh * 0.34, c.bw * 0.64, c.bh * 0.26, r=4)
    fm = p.fill(front, g, "base")
    shade_lr(p, fm, g, c.mx, c.bw * 0.64)
    P._icon(p, "sun", c.mx, c.g - c.bh * 0.22, c.bh * 0.12)
    p.edge(front, g, 1.3)
    c.L.place(p)


def glass_case(p, x0, x1, top, bottom):
    """A glass case: its frame, the glass a pale tint with a streak of light across it."""
    gt = T("#cfe6f6")
    case = Path.rect(x0, top, x1 - x0, bottom - top, r=3)
    m = p.fill(case, gt, "base", alpha=0.35)
    p.stroke(Path.polyline([(x0 + 4, bottom - 8), (x0 + (x1 - x0) * 0.5, top + 6)]), 3.0, gt, "highlight", clip=m, alpha=0.8)
    p.edge(case, T("gold"), 1.6, "base")
    p.fill(Path.rect(x0 - 2, top - 5, x1 - x0 + 4, 6, r=2), T("gold"), "base")


def p_pedestal(c):
    """A white stone pedestal with gold bands: the Sunheart's (the gold gem glowing on it), or a relic
    case (a glass case on it, a golden crown inside, or a blue orb)."""
    p = c.piece()
    st = T("#f2eef4")
    g = T("gold")
    n = c.name
    ped_h = c.bh * (0.55 if n == "sunheart-pedestal" else 0.45)
    w = c.bw * 0.7
    body = Path.rect(c.mx - w / 2, c.g - ped_h, w, ped_h)
    m = stone_block(p, body, st)
    p.stroke(Path.polyline([(c.mx - w / 2, c.g - ped_h * 0.75), (c.mx + w / 2, c.g - ped_h * 0.75)]), 3.0, g, "base", clip=m)
    plinth(p, c.mx, c.g, c.bw * 0.9, 10, st)
    top = c.g - ped_h
    stone_block(p, Path.rect(c.mx - c.bw * 0.45, top - 8, c.bw * 0.9, 9, r=2), st, "base-light")
    if n == "sunheart-pedestal":
        gem = Path.polygon([(c.mx, top - 34), (c.mx + 14, top - 22), (c.mx + 9, top - 8), (c.mx - 9, top - 8), (c.mx - 14, top - 22)])
        gt = T("#ffcf4a")
        gm = p.fill(gem, gt, "base")
        p.tone(p.where(gm, lambda xx, yy: xx > c.mx + 2), gt, "shadow")
        p.fill(Path.polygon([(c.mx - 3, top - 30), (c.mx - 10, top - 22), (c.mx - 3, top - 16)]), gt, "highlight")
        p.edge(gem, gt, 1.2)
        p.glow(c.mx, top - 22, 60, "#ffd070", 0.5)
    else:
        x0, x1 = c.mx - c.bw * 0.38, c.mx + c.bw * 0.38
        ctop = c.y0 + 10
        if n == "relic-case-orb":
            om = p.fill(Path.circle(c.mx, top - 24, 14), T("#7fb8ff"), "base")
            p.tone(om - om.shift(-3, -3), T("#7fb8ff"), "shadow")
            p.fill(Path.circle(c.mx - 5, top - 30, 4), T("#e8f6ff"), "base")
            p.glow(c.mx, top - 24, 40, "#9fd0ff", 0.4)
        else:
            crown = Path.polygon([(c.mx - 16, top - 12), (c.mx - 16, top - 30), (c.mx - 8, top - 20), (c.mx, top - 34), (c.mx + 8, top - 20), (c.mx + 16, top - 30), (c.mx + 16, top - 12)])
            p.fill(crown, g, "base")
            p.fill(Path.circle(c.mx, top - 18, 3), T("cloth.red"), "base")
            p.edge(crown, g, 1.1)
        glass_case(p, x0, x1, ctop, top - 6)
    p.ground_shadow(c.x0, c.x1, c.g + 1, 5, 0.3)
    c.L.place(p)


def p_chest(c):
    """A chest, shut then open: banded wood with iron (or, in the vault, gold-trimmed blue), the
    open one with its lid thrown back and gold heaped inside."""
    p = c.piece()
    vault = c.name == "vault-chest"
    wood = T("cloth.navy" if vault else "wood")
    band = T("gold" if vault else "iron")
    w = c.bw - 8
    h = c.bh * (0.52 if c.frame == 0 else 0.48)
    x0 = c.mx - w / 2
    body = Path.rect(x0, c.g - h, w, h, r=3)
    bm = p.fill(body, wood, "base")
    shade_lr(p, bm, wood, c.mx, w, 0.3, 0.25)
    for bx in (x0 + w * 0.15, x0 + w * 0.85 - 6):
        p.fill(Path.rect(bx, c.g - h, 6, h), band, "base", clip=bm)
    p.edge(body, wood, 1.3)
    if c.frame == 0:
        lid = Path().move(x0 - 2, c.g - h).line(x0 - 2, c.g - h - c.bh * 0.18).quad(c.mx, c.y0 + 2, x0 + w + 2, c.g - h - c.bh * 0.18).line(x0 + w + 2, c.g - h).close()
        lm = p.fill(lid, wood, "light")
        for bx in (x0 + w * 0.15, x0 + w * 0.85 - 6):
            p.fill(Path.rect(bx, c.y0, 6, c.g - h - c.y0 + 2), band, "base", clip=lm)
        p.edge(lid, wood, 1.3)
        lock = Path.rect(c.mx - 6, c.g - h - 4, 12, 14, r=2)
        p.fill(lock, band, "light")
        p.fill(Path.circle(c.mx, c.g - h + 3, 2), T("wood.dark"), "base")
        p.edge(lock, band, 1.0)
    else:
        lid = Path.polygon([(x0 - 2, c.g - h - 2), (x0 + w + 2, c.g - h - 2), (x0 + w - 4, c.y0 + 2), (x0 + 4, c.y0 + 2)])
        p.fill(lid, wood, "shadow")
        p.edge(lid, wood, 1.2)
        gold = T("gold")
        heap = p.nothing()
        for i in range(int(w / 8)):
            heap = heap | p.shape(Path.circle(x0 + 6 + i * 8, c.g - h - 2 - 3 * hash01(i, 1, c.seed), 5))
        p.tone(heap, gold, "base")
        p.tone(heap - heap.shift(-1.4, -1.6), gold, "shadow")
        p.edge_mask(heap, gold, 1.0)
        p.glow(c.mx, c.g - h - 4, w * 0.8, "#ffd070", 0.35)
        p.fill(Path.rect(x0 - 2, c.g - h - 2, w + 4, 6, r=2), wood, "light")
    p.ground_shadow(c.x0, c.x1, c.g + 1, 5, 0.3)
    c.L.place(p)


def crystal_cluster(p, cx, ground, w, h, t, seed, shattered=False):
    """Crystal spikes from a common foot, each faceted: a lit face and a shadowed one."""
    spikes = ((0.0, 1.0, 0.24), (-0.28, 0.7, 0.18), (0.26, 0.62, 0.17), (-0.46, 0.4, 0.13), (0.44, 0.36, 0.12))
    whole = p.nothing()
    for i, (dx, hk, wk) in enumerate(spikes):
        if shattered:
            hk *= 0.35 + 0.2 * hash01(i, 1, seed)
        x = cx + dx * w
        top = ground - h * hk
        sw = w * wk
        lean = dx * w * 0.25
        sp = Path.polygon([(x - sw, ground), (x - sw * 0.6 + lean, top + h * hk * 0.25), (x + lean, top), (x + sw * 0.6 + lean, top + h * hk * 0.25), (x + sw, ground)])
        m = p.fill(sp, t, "base")
        face = p.shape(Path.polygon([(x + lean, top), (x + sw * 0.6 + lean, top + h * hk * 0.25), (x + sw, ground), (x + sw * 0.1, ground)]))
        p.tone(m & face, t, "shadow")
        p.tone(m - face - p.shape(sp.translate(2.5, 0)), t, "light")
        p.edge(sp, t, 1.3)
        whole = whole | m
    return whole


def p_crystal(c):
    """The black crystal: a cluster of spikes of black glass with violet glowing in their seams; the
    purified one pale gold and shining; the shattered one broken off short."""
    p = c.piece()
    n = c.name
    t = T({"black-crystal-purified": "#f2e2b0"}.get(n, "#3a2a4a"))
    base_t = T("#2a2232")
    p.fill(Path.ellipse(c.mx, c.g - 4, c.bw * 0.48, 9), base_t, "base")
    crystal_cluster(p, c.mx, c.g - 4, c.bw * 0.5, c.bh * 0.92, t, c.seed, shattered=n.endswith("shattered"))
    if n == "black-crystal":
        for i in range(5):
            x = c.mx + (hash01(i, 1, c.seed) - 0.5) * c.bw * 0.5
            y = c.g - c.bh * (0.2 + 0.5 * hash01(i, 2, c.seed))
            p.stroke(Path.polyline([(x, y), (x + 3, y + 14), (x - 1, y + 26)]), (2.0, 0.4), T("#b07ae8"), "light")
        p.glow(c.mx, c.g - c.bh * 0.4, c.bw * 0.7, "#9a5ad8", 0.4)
    elif n.endswith("purified"):
        p.glow(c.mx, c.g - c.bh * 0.45, c.bw * 0.9, "#fff0b0", 0.45)
    else:
        for i in range(6):
            x = c.mx + (hash01(i, 3, c.seed) - 0.5) * c.bw * 0.9
            sh = Path.polygon([(x - 5, c.g - 2), (x, c.g - 10 - 6 * hash01(i, 4, c.seed)), (x + 5, c.g - 2)])
            p.fill(sh, t, "base")
            p.edge(sh, t, 1.0)
    c.L.place(p)


def p_waystone(c):
    """Waypoints, asleep then awake: a crystal on a carved stone foot (crystal-stone), or a standing
    stone with a rune cut in it; awake, the crystal and the rune glow blue."""
    p = c.piece()
    awake = c.frame == 1
    st = T("stone.grey")
    if c.name == "crystal-stone":
        foot = Path.polygon([(c.x0 + 4, c.g), (c.x0 + 10, c.g - c.bh * 0.3), (c.x1 - 10, c.g - c.bh * 0.3), (c.x1 - 4, c.g)])
        stone_block(p, foot, st)
        ct = T("#9fd8ff" if awake else "#b8c4d4")
        crystal_cluster(p, c.mx, c.g - c.bh * 0.28, c.bw * 0.34, c.bh * 0.66, ct, c.seed)
        if awake:
            p.glow(c.mx, c.g - c.bh * 0.55, c.bw * 1.4, "#7fd0ff", 0.55)
    else:
        stone = Path().move(c.x0 + 6, c.g).line(c.x0 + 8, c.y0 + 16).quad(c.mx, c.y0, c.x1 - 8, c.y0 + 16).line(c.x1 - 6, c.g).close()
        stone_block(p, stone, st)
        rune = Path.polyline([(c.mx, c.y0 + 28), (c.mx, c.y0 + c.bh * 0.6)]).add(Path.polyline([(c.mx - 8, c.y0 + 38), (c.mx, c.y0 + 46), (c.mx + 8, c.y0 + 38)]))
        p.stroke(rune, 3.0, T("#7fd0ff") if awake else st, "light" if awake else "deep-shadow")
        if awake:
            p.glow(c.mx, c.y0 + c.bh * 0.4, c.bw * 1.6, "#7fd0ff", 0.5)
    p.ground_shadow(c.x0, c.x1, c.g + 1, 4, 0.3)
    c.L.place(p)


def p_lever(c):
    """A lever on the wall, up then down: its plate (iron, or the vault's gold) and its handle."""
    p = c.piece()
    metal = T("gold" if c.name.startswith("vault") else "iron")
    plate = Path.rect(c.cx - c.bw * 0.4, c.g - c.bh * 0.55, c.bw * 0.8, c.bh * 0.4, r=3)
    stone_block(p, plate, metal)
    pivot = (c.cx, c.g - c.bh * 0.35)
    a = math.radians(-60 if c.frame == 0 else 60)
    end = (pivot[0] + math.sin(a) * c.bh * 0.55, pivot[1] - math.cos(a) * c.bh * 0.55)
    p.stroke(Path.polyline([pivot, end]), 5, T("wood.dark"), "base")
    p.fill(Path.circle(end[0], end[1], 6), T("cloth.red"), "base")
    p.fill(Path.circle(*pivot, 4), metal, "light")
    c.L.place(p, reach=1.8)


def p_dart_trap(c):
    """A dart trap in the wall: a carved stone panel with three dark slots."""
    p = c.piece()
    st = T("stone.warm")
    panel = Path.rect(c.x0 + 4, c.y0 + 4, c.bw - 8, c.bh - 8, r=2)
    stone_block(p, panel, st)
    for k in range(3):
        p.fill(Path.rect(c.x0 + 14 + k * (c.bw - 28) / 2 - 3, c.mx * 0 + c.y0 + c.bh * 0.4, 6, c.bh * 0.25, r=3), T("#2a2232"), "base")
    c.L.place(p, reach=1.8)


def p_root_wall(c):
    """The roots across the quarry's way, tangled (a wall of thick black-veined roots) then parted
    (drawn back to the sides, the way open between)."""
    p = c.piece()
    t = T("#5a4248")
    vein = T("#9a5ad8")
    parted = c.frame == 1
    for i in range(9):
        side = -1 if i % 2 == 0 else 1
        x_top = c.x0 + c.bw * (0.1 + 0.8 * hash01(i, 1, c.seed))
        if parted:
            x_top = c.mx + side * c.bw * (0.32 + 0.14 * hash01(i, 2, c.seed))
        pts = [(x_top, c.y0 + 4)]
        for k in range(1, 5):
            u = k / 4
            dx = (hash01(i, k + 3, c.seed) - 0.5) * c.bw * (0.15 if parted else 0.5)
            x = x_top + dx if not parted else c.mx + side * c.bw * (0.3 + 0.16 * hash01(i, k, c.seed))
            pts.append((x, c.y0 + 4 + (c.g - c.y0 - 4) * u))
        root = Path.smooth(pts)
        p.stroke(root, (9, 5), t, "base")
        p.stroke(root.translate(-1.5, -1), (2.6, 1.0), t, "light")
        p.stroke(root.translate(1.5, 1), (1.4, 0.4), vein, "base")
    c.L.place(p)


def ground_decal(c, path, t, inner=None):
    p = c.piece()
    m = p.fill(path, t, "base")
    if inner:
        inner(p, m)
    c.L.place(p, reach=1.6)


def p_decal(c):
    """Things lying flat on the floor, seen side-on from a little above: the sun mosaic (a navy disc
    with a gold sun), a pool of tar (black, an oily violet sheen), a sappers' hole (a dark pit with
    broken stones round its lip), bones."""
    n = c.name
    w = c.bw
    if n == "sun-mosaic":
        rx, ry = w * 0.48, min(c.bh * 0.5, w * 0.24)
        cy = c.g - ry
        p = c.piece()
        navy, g = T("cloth.navy"), T("gold")
        m = p.fill(Path.ellipse(c.mx, cy, rx, ry), T("stone"), "base")
        p.fill(Path.ellipse(c.mx, cy, rx * 0.86, ry * 0.86), navy, "base")
        for k in range(16):
            a = math.radians(k * 22.5)
            r1 = 0.8 if k % 2 == 0 else 0.55
            ray = Path.polygon([(c.mx + math.cos(a) * rx * r1, cy + math.sin(a) * ry * r1), (c.mx + math.cos(a + 0.18) * rx * 0.28, cy + math.sin(a + 0.18) * ry * 0.28),
                                (c.mx + math.cos(a - 0.18) * rx * 0.28, cy + math.sin(a - 0.18) * ry * 0.28)])
            p.fill(ray, g, "base" if k % 2 else "light")
        p.fill(Path.ellipse(c.mx, cy, rx * 0.26, ry * 0.26), g, "light")
        p.edge(Path.ellipse(c.mx, cy, rx * 0.86, ry * 0.86), g, 1.6, "base")
        p.edge(Path.ellipse(c.mx, cy, rx, ry), T("stone"), 1.2, "shadow")
        del m
        c.L.place(p, reach=1.4)
    elif n == "tar-pool":
        p = c.piece()
        t = T("#241c2c")
        pts = [(c.mx + math.cos(a) * w * 0.46 * (0.85 + 0.2 * hash01(i, 1, c.seed)), c.g - c.bh * 0.42 + math.sin(a) * c.bh * 0.4 * (0.85 + 0.2 * hash01(i, 2, c.seed)))
               for i, a in enumerate(np.linspace(0, 2 * math.pi, 10, endpoint=False))]
        pool = Path.smooth(pts, closed=True)
        m = p.fill(pool, t, "base")
        p.fill(Path.ellipse(c.mx - w * 0.12, c.g - c.bh * 0.55, w * 0.18, 3), T("#8a5ab8"), "light", clip=m)
        p.fill(Path.ellipse(c.mx + w * 0.16, c.g - c.bh * 0.35, w * 0.1, 2), T("#c8a8e8"), "light", clip=m)
        p.edge(pool, t, 1.4)
        c.L.place(p, reach=1.4)
    elif n == "sapper-hole":
        p = c.piece()
        rx, ry = w * 0.42, c.bh * 0.32
        cy = c.g - c.bh * 0.42
        p.fill(Path.ellipse(c.mx, cy, rx + 10, ry + 8), T("earth"), "base")
        p.fill(Path.ellipse(c.mx, cy + 2, rx, ry), T("#1c1620"), "base")
        p.fill(Path.ellipse(c.mx, cy - ry * 0.4, rx * 0.8, ry * 0.4), T("#3a2e34"), "base")
        for i in range(10):
            a = 2 * math.pi * i / 10
            sx, sy = c.mx + math.cos(a) * (rx + 6), cy + math.sin(a) * (ry + 5)
            stone_block(p, Path.rect(sx - 8, sy - 5, 16, 10, r=3), T("stone.grey"))
        c.L.place(p, reach=1.4)
    elif n == "bones":
        p = c.piece()
        bt = T("#ece4d4")
        for a, b in (((c.x0 + 10, c.g - 6), (c.mx + 4, c.g - 16)), ((c.mx - 6, c.g - 4), (c.x1 - 26, c.g - 10))):
            p.stroke(Path.polyline([a, b]), 5, bt, "base")
            for e in (a, b):
                p.fill(Path.circle(e[0], e[1] - 2, 3.4), bt, "base")
                p.fill(Path.circle(e[0], e[1] + 2, 3.4), bt, "base")
        skull = Path.ellipse(c.x1 - 18, c.g - 12, 13, 11)
        m = p.fill(skull, bt, "base")
        p.tone(m - m.shift(-2, -2), bt, "shadow")
        for ex in (c.x1 - 23, c.x1 - 13):
            p.fill(Path.circle(ex, c.g - 13, 3), T("#2a2232"), "base")
        p.edge(skull, bt, 1.1)
        c.L.place(p, reach=1.6)


def p_courier(c):
    """The courier fallen in the Mirewood, lying face down in a travelling cloak, his satchel by him."""
    p = c.piece()
    cloak = T("#4a4a5a")
    body = Path.smooth([(c.x0 + 8, c.g - 4), (c.x0 + c.bw * 0.15, c.g - c.bh * 0.55), (c.x0 + c.bw * 0.5, c.g - c.bh * 0.62), (c.x1 - c.bw * 0.2, c.g - c.bh * 0.5),
                        (c.x1 - 8, c.g - 4)], closed=True)
    m = p.fill(body, cloak, "base")
    p.tone(p.below(m, c.g - c.bh * 0.3), cloak, "shadow")
    for k in range(3):
        fx = c.x0 + c.bw * (0.3 + 0.18 * k)
        p.stroke(Path.polyline([(fx, c.g - c.bh * 0.5), (fx + 6, c.g - 6)]), (2.4, 0.6), cloak, "deep", clip=m)
    p.edge(body, cloak, 1.3)
    head = Path.circle(c.x0 + c.bw * 0.1, c.g - c.bh * 0.32, c.bh * 0.24)
    p.fill(head, T("#5a3a2a"), "base")
    p.edge(head, T("#5a3a2a"), 1.0)
    for bx in (c.x1 - c.bw * 0.08,):
        boot = Path.rect(bx - 12, c.g - c.bh * 0.36, 16, c.bh * 0.34, r=4)
        p.fill(boot, T("#3a2a26"), "base")
        p.edge(boot, T("#3a2a26"), 1.0)
    sat = Path.rect(c.mx - 8, c.g - c.bh * 0.32, 26, c.bh * 0.3, r=4)
    p.fill(sat, T("#8a5a3a"), "base")
    p.edge(sat, T("#8a5a3a"), 1.0)
    c.L.place(p, reach=1.6)


def p_vault_stair(c):
    """The vault's stair down: a doorway of white stone with gold trim, steps going down into the
    dark; shut, a grille of gilded bars across it."""
    p = c.piece()
    st = T("#f2eef4")
    g = T("gold")
    frame_ = Path.rect(c.x0 + 2, c.y0 + 6, c.bw - 4, c.g - c.y0 - 6)
    stone_block(p, frame_, st)
    op = Path.rect(c.x0 + 16, c.y0 + 22, c.bw - 32, c.g - c.y0 - 22)
    p.fill(op, T("#2a2232"), "base")
    for k in range(5):
        y = c.y0 + 22 + (c.g - c.y0 - 22) * (0.3 + 0.14 * k)
        step = Path.rect(c.x0 + 16, y, c.bw - 32, (c.g - c.y0 - 22) * 0.14, r=1)
        p.fill(step, T("stone.grey", soft=0.6), "base" if k > 2 else "shadow", alpha=0.35 + 0.15 * k)
        p.edge(step, T("stone.grey"), 0.9, "shadow")
    p.stroke(Path.polyline([(c.x0 + 2, c.y0 + 14), (c.x1 - 2, c.y0 + 14)]), 3.0, g, "base")
    P._icon(p, "sun", c.mx, c.y0 + 14, 18)
    if c.name.endswith("shut"):
        for k in range(7):
            x = c.x0 + 20 + k * (c.bw - 40) / 6
            p.stroke(Path.polyline([(x, c.y0 + 24), (x, c.g - 2)]), 4, g, "base")
        for y in (c.y0 + 40, c.g - 30):
            p.stroke(Path.polyline([(c.x0 + 18, y), (c.x1 - 18, y)]), 4, g, "base")
    c.L.place(p)


def p_fence(c):
    """A section of rail fence (two rails between posts, as the fields' are), or one of its posts."""
    p = c.piece()
    wood = T("wood.light")
    top = c.y0 + 4
    if c.name == "fence-wood-post":
        post(p, c.mx, top + 4, c.g, c.bw * 0.8, wood)
    else:
        for y in (top + c.bh * 0.22, top + c.bh * 0.55):
            rail = Path.rect(c.x0, y, c.bw, 9, r=2)
            m = p.fill(rail, wood, "base")
            p.tone(m - m.shift(0, 2), wood, "light")
            p.edge(rail, wood, 1.1)
        for x in (c.x0 + 8, c.x1 - 8):
            post(p, x, top + 4, c.g, 10, wood)
    p.ground_shadow(c.x0, c.x1, c.g + 1, 4, 0.25)
    c.L.place(p)


# --------------------------------------------------------------------------- #
# Foliage, second pass: leafy clumps instead of round ones
# --------------------------------------------------------------------------- #

def clump_path(x, y, r, seed, bumps=8):
    """A clump of leaves: a circle whose edge is scalloped into `bumps` rounded lobes."""
    pts = []
    n = bumps * 2
    ph = hash01(int(x), int(y), seed) * 6.283
    for i in range(n):
        a = 2 * math.pi * i / n + ph
        rr = r * (1.0 if i % 2 == 0 else 0.82) * (0.94 + 0.12 * hash01(i, int(y), seed))
        pts.append((x + math.cos(a) * rr, y + math.sin(a) * rr * 0.94))
    return Path.smooth(pts, closed=True)


def leaf_clumps(p, clumps, seed, line=True):
    """Foliage as leafy clumps, drawn in the order given, each (x, y, r, tones): its body, a crescent
    of shadow on its lower right, a lobed patch of light on its upper left with a few lit leaves,
    and its dark line along its lower right edge. Returns the whole as a Mask."""
    whole = p.nothing()
    for i, (x, y, r, t) in enumerate(clumps):
        path = clump_path(x, y, r, seed + i)
        m = p.fill(path, t, "base")
        cres = m - p.shape(clump_path(x - r * 0.16, y - r * 0.24, r * 1.0, seed + i))
        p.tone(cres, t, "shadow")
        p.tone(cres - p.shape(clump_path(x - r * 0.06, y - r * 0.1, r * 1.0, seed + i)), t, "deep-shadow", alpha=0.8)
        lit = p.shape(clump_path(x - r * 0.3, y - r * 0.34, r * 0.52, seed + i + 7)) & m
        p.tone(lit, t, "light")
        for k in range(3):
            lx = x - r * (0.45 - 0.2 * k) + (hash01(k, i, seed) - 0.5) * r * 0.2
            ly = y - r * (0.5 - 0.12 * k)
            p.fill(Path.ellipse(lx, ly, r * 0.12, r * 0.06, rot=-30 + 20 * k), t, "highlight", clip=lit, alpha=0.8)
        if line:
            ring = p.line_shape(path, 3.2) & m
            p.tone(p.where(ring, lambda xx, yy: (xx - x) * 0.55 + (yy - y) * 0.85 > r * 0.05), t, "line")
        whole = whole | m
    return whole


def crown(p, cx, cy, rx, ry, t, back, seed, flowers=None, flower_n=0, rk=0.24):
    """A tree's crown of leafy clumps inside the ellipse (cx, cy, rx, ry): a few clumps peeking from
    behind in the darker `back` ramp, then rings of clumps from the top down, the lower ones a step
    toward shadow; flowers dotted over it if asked."""
    r = min(rx, ry) * rk
    ex, ey = max(1.0, rx - r * 1.0), max(1.0, ry - r * 1.0)
    low = haze(t, back["base"], 0.42)
    clumps = []
    for i in range(10):
        a = 2 * math.pi * (i + 0.5) / 10 + hash01(i, 1, seed) * 0.4
        clumps.append((-2, cx + math.cos(a) * ex * 0.95, cy + math.sin(a) * ey * 0.9 - ey * 0.06, r * (0.9 + 0.2 * hash01(i, 2, seed)), back))
    for ring, (k, n) in enumerate(((1.0, 18), (0.7, 13), (0.42, 8), (0.14, 3))):
        for i in range(n):
            a = 2 * math.pi * i / n + ring * 0.37 + hash01(i, ring + 3, seed) * 0.3
            jit = 0.9 + 0.18 * hash01(i, ring + 9, seed)
            x, y = cx + math.cos(a) * ex * k * jit, cy + math.sin(a) * ey * k * jit
            tt = low if y > cy + ey * 0.35 else t
            clumps.append((y, x, y, r * (0.95 + 0.25 * hash01(i, ring + 14, seed)), tt))
    clumps.sort(key=lambda c: c[0])
    whole = leaf_clumps(p, [(x, y, rr, tt) for _, x, y, rr, tt in clumps], seed)
    if flowers:
        for i in range(flower_n):
            a = hash01(i, 4, seed) * 6.283
            d = hash01(i, 5, seed) ** 0.5
            fx, fy = cx + math.cos(a) * rx * d * 0.92, cy + math.sin(a) * ry * d * 0.88
            ft = T(flowers[i % len(flowers)])
            petals = Path()
            s = 2.2 + hash01(i, 6, seed) * 1.2
            for k in range(5):
                aa = math.radians(k * 72 + 20 * i)
                petals.add(Path.circle(fx + math.cos(aa) * s, fy + math.sin(aa) * s, s * 0.85))
            fm = p.fill(petals, ft, "base", clip=whole, rule="nonzero")
            p.tone(fm - fm.shift(-0.7, -0.7), ft, "shadow")
            p.fill(Path.circle(fx, fy, s * 0.45), T("flower.yellow"), "base", clip=whole)
    return whole


def p_tree(c):
    """A broadleaf tree in the old one's box: a trunk with roots flaring, forking into limbs that show
    under the crown, and a crown of leafy clumps."""
    leaf, back, flowers = TREE_COLOURS[c.name]
    p = c.piece()
    bark = T("#5a4048" if "blackvein" in c.name else "bark")
    crown_h = c.bh * 0.72
    rx, ry = c.bw * 0.5 - 5, crown_h * 0.5 - 3
    cy = c.y0 + 4 + ry
    tw = max(6.0, c.bw * 0.06)
    trunk(p, c.mx, c.g, cy + ry * 0.3, tw, bark)
    for side, (u, v) in ((-1, (0.48, 0.25)), (1, (0.5, 0.18)), (-1, (0.22, -0.1)), (1, (0.25, -0.2))):
        branch(p, (c.mx, cy + ry * 0.75), (c.mx + side * rx * u, cy + ry * v), tw * 0.8, tw * 0.3, bark)
    whole = crown(p, c.mx, cy, rx, ry, T(leaf), T(back), c.seed, flowers=flowers, flower_n=int(rx * ry / 300) if flowers else 0)
    if "blackvein" in c.name:
        vt = T("#2a1c34")
        for i in range(8):
            sx = c.mx + (hash01(i, 1, c.seed) - 0.5) * rx * 1.4
            sy = cy + (hash01(i, 2, c.seed) - 0.3) * ry * 1.2
            pts = [(sx, sy)]
            for k in range(4):
                pts.append((pts[-1][0] + (hash01(i, k + 3, c.seed) - 0.5) * 18, pts[-1][1] + 10))
            p.stroke(Path.smooth(pts), (2.4, 0.4), vt, "base", clip=whole)
        p.glow(c.mx, cy + ry * 0.3, rx * 0.8, "#9a5ad8", 0.1)
    for k in range(5):
        gx = c.mx + (k - 2) * tw * 0.9
        p.stroke(Path.polyline([(gx, c.g), (gx + (k - 2) * 3, c.g - 9 - 4 * hash01(k, 3, c.seed))]), (3.0, 0.0), T("leaf"), "base")
    p.ground_shadow(c.mx - c.bw * 0.32, c.mx + c.bw * 0.32, c.g + 1, 8, 0.32)
    c.L.place(p)


def leafy_bush(p, x, ground, w, h, t, seed, flowers=None):
    """A bush: a mound of leafy clumps, the lower ones a step darker, flowers dotted over it."""
    clumps = []
    n = max(3, int(w / 26))
    r0 = min(h * 0.42, w / n * 0.8)
    for row, (k, dy) in enumerate(((1.0, 0.0), (0.7, 0.42))):
        m = n if row == 0 else max(2, n - 1)
        for i in range(m):
            u = (i + 0.5) / m
            bump = math.sin(math.pi * u)
            r = r0 * (0.85 + 0.25 * hash01(i, row, seed)) * (0.8 + 0.25 * bump)
            xx = x - w / 2 + r + (w - 2 * r) * u
            yy = ground - r * 0.9 - dy * h * bump
            clumps.append((yy, xx, yy, r))
    clumps.sort(key=lambda c: c[0])
    low = haze(t, t["shadow"], 0.4)
    whole = leaf_clumps(p, [(xx, yy, r, t if yy < ground - h * 0.45 else low) for _, xx, yy, r in clumps], seed)
    if flowers:
        for i in range(int(w * h / 200)):
            fx = x - w / 2 + w * (0.1 + 0.8 * hash01(i, 2, seed))
            fy = ground - h * 0.9 * hash01(i, 3, seed) ** 0.7
            ft = T(flowers[i % len(flowers)])
            fm = p.fill(Path.circle(fx, fy, 3.0), ft, "base", clip=whole)
            p.tone(fm - fm.shift(-0.7, -0.7), ft, "shadow")
    return whole


def p_bush(c):
    colour, flowers = BUSHES[c.name]
    p = c.piece()
    leafy_bush(p, c.mx, c.g, c.bw - 6, c.bh - 6, T(colour), c.seed, flowers)
    p.ground_shadow(c.x0, c.x1, c.g + 1, 6, 0.3)
    c.L.place(p)


def p_cypress(c):
    """A cypress, standing in the ground or (cypress-pot) in a pot: a tall flame of leafy clumps."""
    p = c.piece()
    potted = c.name.endswith("pot")
    t = T("leaf.dark")
    ground = c.g
    if potted:
        P.pot(None, c.mx, c.g, c.bw * 0.7, c.bh * 0.22, "terracotta", piece=p)
        ground = c.g - c.bh * 0.2
    top = c.y0 + 4
    h = ground - top
    w = c.bw * (0.72 if potted else 0.92)
    clumps = []
    n = 13
    for i in range(n):
        u = i / (n - 1)
        cy_ = ground - h * 0.08 - (h * 0.84) * u
        r = w * 0.5 * (1.0 - u * 0.7) * (0.9 + 0.15 * hash01(i, 1, c.seed)) + 3
        clumps.append((c.mx + (hash01(i, 2, c.seed) - 0.5) * w * 0.25, cy_, r, t if i % 3 else haze(t, t["shadow"], 0.3)))
    clumps.reverse()
    leaf_clumps(p, clumps, c.seed)
    if not potted:
        trunk(p, c.mx, c.g, c.g - c.bh * 0.06, 4, T("bark"))
        p.ground_shadow(c.mx - w * 0.5, c.mx + w * 0.5, c.g + 1, 5, 0.3)
    c.L.place(p)


def p_flower_clump(c):
    """A clump of low leaves with flowers thick in it, lying on the ground."""
    colours = {"flowers-mixed": ("flower.pink", "flower.yellow", "flower.white", "flower.violet"),
               "flowers-pink": ("flower.pink", "blossom"), "flowers-white": ("flower.white", "blossom.white")}[c.name]
    p = c.piece()
    whole = leafy_bush(p, c.mx, c.g, c.bw - 4, c.bh - 4, T("leaf"), c.seed)
    for i in range(int(c.bw * c.bh / 60)):
        fx = c.x0 + 6 + (c.bw - 12) * hash01(i, 2, c.seed)
        fy = c.g - (c.bh - 6) * hash01(i, 3, c.seed) ** 0.8
        ft = T(colours[i % len(colours)])
        petals = Path()
        for k in range(5):
            a = math.radians(k * 72 + 30 * hash01(i, 4, c.seed))
            petals.add(Path.circle(fx + math.cos(a) * 2.4, fy + math.sin(a) * 2.4, 2.1))
        fm = p.fill(petals, ft, "base", clip=whole, rule="nonzero")
        p.tone(fm - fm.shift(-0.6, -0.6), ft, "shadow")
        p.fill(Path.circle(fx, fy, 1.1), T("flower.yellow"), "deep-shadow", clip=whole)
    c.L.place(p, reach=1.8)


# --------------------------------------------------------------------------- #
# Stone, gold, sacks, statues, the fountain: second pass
# --------------------------------------------------------------------------- #

def boulder(p, x, ground, w, h, t, seed, moss=False):
    """A boulder: a rounded, faceted stone, its top-left facet lit, its right in shadow, a crack."""
    h = min(h, w * 0.7)
    pts = [(x - w * 0.46, ground)]
    n = 7
    for i in range(n):
        a = math.pi * 1.05 + math.pi * 0.9 * i / (n - 1)
        rr = 0.9 + 0.12 * hash01(i, 1, seed)
        pts.append((x + math.cos(a) * w / 2 * rr, ground - 2 + math.sin(a) * (h - 2) * rr))
    pts.append((x + w * 0.46, ground))
    path = Path.smooth(pts, closed=True, tension=0.5)
    m = p.fill(path, t, "base")
    facet_l = Path.polygon([(x - w * 0.55, ground - h * 0.35), (x - w * 0.25, ground - h * 1.1), (x + w * 0.1, ground - h * 1.1), (x - w * 0.02, ground - h * 0.62), (x - w * 0.3, ground - h * 0.42)])
    facet_r = Path.polygon([(x + w * 0.18, ground - h * 1.1), (x + w * 0.6, ground - h * 0.5), (x + w * 0.55, ground + 2), (x + w * 0.12, ground + 2), (x + w * 0.08, ground - h * 0.55)])
    p.tone(p.shape(facet_l) & m, t, "light")
    p.tone(p.shape(facet_r) & m, t, "shadow")
    p.tone(m - m.shift(-2, -3), t, "deep-shadow", alpha=0.6)
    p.stroke(Path.polyline([(x - w * 0.05, ground - h * 0.7), (x + w * 0.02, ground - h * 0.45), (x - w * 0.04, ground - h * 0.25)]), (1.4, 0.0), t, "deep", clip=m)
    if moss:
        mm = p.shape(clump_path(x - w * 0.12, ground - h * 0.92, w * 0.28, seed)) & m
        mt = T("leaf")
        p.tone(mm, mt, "base")
        p.tone(mm - mm.shift(-1.2, -1.8), mt, "shadow")
    p.edge(path, t, 1.4)
    return m


def p_rocks(c):
    """Boulders heaped together (moss on the mossy ones), or a heap of broken masonry: squared
    blocks and chips tumbled together, dust at the foot."""
    p = c.piece()
    if c.name == "rubble-heap":
        t = T("stone.grey")
        g = T("#b8aca0")
        i = 0
        rows = ((5, 0.0), (4, 0.3), (3, 0.56), (1, 0.8))
        for row, (n, up) in enumerate(rows):
            span = c.bw * (0.92 - 0.2 * row)
            for k in range(n):
                x = c.mx - span / 2 + span * (k + 0.5) / n + (hash01(i, 1, c.seed) - 0.5) * 8
                y = c.g - (c.bh - 6) * up - 2
                bw_ = span / n * (0.95 + 0.2 * hash01(i, 3, c.seed))
                bh_ = (c.bh - 6) * 0.3 * (0.85 + 0.3 * hash01(i, 4, c.seed))
                blk = Path.rect(-bw_ / 2, -bh_, bw_, bh_, r=3).rotate(-14 + 28 * hash01(i, 5, c.seed)).translate(x, y)
                stone_block(p, blk, t if i % 3 else g)
                i += 1
        for k in range(6):
            x = c.x0 + 8 + (c.bw - 16) * hash01(k, 7, c.seed)
            stone_block(p, Path.rect(x - 5, c.g - 7, 10, 6, r=2).rotate(30 * hash01(k, 8, c.seed), x, c.g - 4), g)
        p.fill(Path.ellipse(c.mx, c.g - 3, c.bw * 0.46, 5), T("earth"), "light", alpha=0.6)
    else:
        t = T("stone.grey")
        specs = ((-0.2, 0.62, 0.86), (0.22, 0.56, 0.66), (0.02, 0.38, 0.4))
        for i, (dx, wk, hk) in enumerate(specs):
            boulder(p, c.mx + dx * c.bw, c.g, c.bw * wk, c.bh * hk, t, c.seed + i, moss=c.name == "rocks-mossy" and i != 2)
    p.ground_shadow(c.x0, c.x1, c.g + 1, 5, 0.3)
    c.L.place(p)


def p_gold(c):
    """A heap of gold coins: a lumpy mound covered in coins catching the light, a few spilled at its
    foot, a jewel or two, and sparkles."""
    p = c.piece()
    g = T("gold")
    pts = [(c.x0 + 4, c.g)]
    n = 9
    for i in range(1, n):
        u = i / n
        hump = math.sin(math.pi * u) ** 0.8
        pts.append((c.x0 + 4 + (c.bw - 8) * u, c.g - (c.bh - 8) * hump * (0.88 + 0.16 * hash01(i, 1, c.seed))))
    pts.append((c.x1 - 4, c.g))
    heap = Path.smooth(pts, closed=True, tension=0.4)
    m = p.fill(heap, g, "base")
    p.tone(m - m.shift(-3, -5), g, "shadow")
    rng = int(c.bw * c.bh / 50)
    for i in range(rng):
        x = c.x0 + 8 + (c.bw - 16) * hash01(i, 2, c.seed)
        y = c.y0 + 6 + (c.bh - 10) * hash01(i, 3, c.seed)
        coin = Path.ellipse(x, y, 5.5, 2.8, rot=-15 + 30 * hash01(i, 4, c.seed))
        cm = p.shape(coin) & m
        p.tone(cm, g, "light" if hash01(i, 5, c.seed) < 0.6 else "base-light")
        p.tone(cm - cm.shift(0, 1.4), g, "highlight")
        p.tone(cm - cm.shift(0, -1.0), g, "deep", alpha=0.7)
    for i in range(4):
        x = c.x0 + c.bw * (0.15 + 0.22 * i)
        coin = Path.ellipse(x, c.g - 2, 5.5, 2.4)
        p.fill(coin, g, "light")
        p.edge(coin, g, 0.9)
    for k, col in enumerate(("cloth.red", "#5fb4ec")):
        jx, jy = c.mx + (k - 0.5) * c.bw * 0.3, c.g - c.bh * (0.45 + 0.15 * k)
        jm = p.fill(Path.polygon([(jx, jy - 5), (jx + 5, jy), (jx, jy + 5), (jx - 5, jy)]), T(col), "base", clip=m)
        p.fill(Path.circle(jx - 1.5, jy - 1.5, 1.4), T(col), "highlight", clip=jm)
    p.edge(heap, g, 1.3)
    for i in range(3):
        P._sparkle(c.L, c.x0 + c.bw * (0.22 + 0.28 * i), c.y0 + c.bh * (0.25 + 0.2 * (i % 2)), 10, "#fff6d0", 0.85)
    p.ground_shadow(c.x0, c.x1, c.g + 1, 5, 0.3)
    c.L.place(p)


def sack(p, x, ground, w, h, t, seed, lean=0.0):
    """A grain sack standing: a full bag, tied at the neck with its top flopped over, shaded round."""
    import style
    body = Path.smooth([(x - w * 0.5, ground), (x - w * 0.54, ground - h * 0.3), (x - w * 0.44, ground - h * 0.62), (x - w * 0.18 + lean, ground - h * 0.82),
                        (x - w * 0.1 + lean, ground - h * 0.88), (x + w * 0.1 + lean, ground - h * 0.88), (x + w * 0.2 + lean, ground - h * 0.8),
                        (x + w * 0.46, ground - h * 0.6), (x + w * 0.55, ground - h * 0.28), (x + w * 0.5, ground)], closed=True)
    m = p.shape(body)
    style.shade(m, "cloth", t, edge=P.INNER, rim=False)
    neck_y = ground - h * 0.86
    top = Path.smooth([(x - w * 0.12 + lean, neck_y), (x - w * 0.24 + lean, neck_y - h * 0.08), (x - w * 0.02 + lean, neck_y - h * 0.14), (x + w * 0.3 + lean, neck_y - h * 0.06),
                       (x + w * 0.12 + lean, neck_y)], closed=True)
    tm = p.fill(top, t, "base-light")
    p.edge(top, t, 1.1)
    del tm
    p.stroke(Path.polyline([(x - w * 0.15 + lean, neck_y + 2), (x + w * 0.15 + lean, neck_y + 2)]), 3.0, T("rope"), "base")
    for k in range(3):
        cx_ = x - w * 0.25 + k * w * 0.22
        p.stroke(Path.smooth([(cx_ + lean * 0.5, ground - h * 0.78), (cx_ - 2, ground - h * 0.5), (cx_ + 1, ground - h * 0.2)]), (1.6, 0.2), t, "terminator", clip=m)


def p_sacks(c):
    """Grain sacks: three standing together, or (sack-pile) a heap of them, one lying across the top,
    a little grain spilled."""
    p = c.piece()
    t = T("sack")
    if c.name == "sack-pile":
        w, h = c.bw * 0.5, c.bh * 0.55
        sack(p, c.mx - w * 0.45, c.g, w, h, t, c.seed)
        sack(p, c.mx + w * 0.45, c.g, w, h * 0.92, t, c.seed + 1, lean=3)
        sack(p, c.mx, c.g - h * 0.72, w * 1.05, h * 0.75, t, c.seed + 2, lean=-3)
        grain = T("#e2c066")
        for i in range(9):
            p.fill(Path.ellipse(c.x1 - 18 + (hash01(i, 1, c.seed) - 0.5) * 16, c.g - 2 - 3 * hash01(i, 2, c.seed), 2.4, 1.4), grain, "base")
    else:
        w, h = c.bw * 0.4, c.bh * 0.9
        sack(p, c.mx - w * 0.72, c.g, w, h * 0.92, t, c.seed)
        sack(p, c.mx + w * 0.72, c.g, w, h * 0.86, t, c.seed + 1, lean=2)
        sack(p, c.mx, c.g, w * 1.05, h, t, c.seed + 2, lean=-2)
    p.ground_shadow(c.x0, c.x1, c.g + 1, 5, 0.3)
    c.L.place(p)


def figure_statue(p, cx, ground, h, t, kind):
    """A stone figure on its plinth, shaded round (style.shade): a hooded woman holding a staff with a
    gold finial, her robe falling in folds; or a knight with his helm and plume, a kite shield on
    his arm and his sword point down."""
    import style
    w = h * 0.34
    if kind == "hooded":
        robe = Path.smooth([(cx - w * 0.66, ground), (cx - w * 0.56, ground - h * 0.4), (cx - w * 0.52, ground - h * 0.66), (cx - w * 0.42, ground - h * 0.74),
                            (cx - w * 0.26, ground - h * 0.8), (cx - w * 0.24, ground - h * 0.92), (cx, ground - h), (cx + w * 0.24, ground - h * 0.92),
                            (cx + w * 0.26, ground - h * 0.8), (cx + w * 0.42, ground - h * 0.74), (cx + w * 0.52, ground - h * 0.66), (cx + w * 0.56, ground - h * 0.4),
                            (cx + w * 0.66, ground)], closed=True)
        m = p.shape(robe)
        style.shade(m, "cloth", t, edge=P.INNER, rim=False)
        hood = p.shape(Path.smooth([(cx - w * 0.26, ground - h * 0.8), (cx - w * 0.24, ground - h * 0.92), (cx, ground - h), (cx + w * 0.24, ground - h * 0.92),
                                    (cx + w * 0.26, ground - h * 0.8), (cx, ground - h * 0.76)], closed=True)) & m
        p.tone(hood - hood.shift(-2, -2), t, "shadow")
        for k in range(4):
            fx = cx - w * 0.36 + k * w * 0.24
            p.stroke(Path.smooth([(fx, ground - h * 0.55), (fx - 2, ground - h * 0.3), (fx + 2, ground - 4)]), (2.6, 0.8), t, "shadow", clip=m)
        face = p.shape(Path.ellipse(cx, ground - h * 0.84, w * 0.17, h * 0.07)) & m
        p.tone(face, t, "deep-shadow")
        arms = Path.smooth([(cx - w * 0.36, ground - h * 0.62), (cx - w * 0.05, ground - h * 0.52), (cx + w * 0.3, ground - h * 0.56)])
        p.stroke(arms, 7.0, t, "base")
        p.stroke(arms.translate(0, 2.5), 2.0, t, "shadow")
        sx = cx + w * 0.3
        p.stroke(Path.polyline([(sx, ground - h * 1.05), (sx - 2, ground)]), 4.0, t, "light")
        p.edge(Path.rect(sx - 3, ground - h * 1.05, 4, h * 1.05), t, 0.8)
        orb = Path.circle(sx, ground - h * 1.08, 7)
        p.fill(orb, T("gold"), "base")
        p.fill(Path.circle(sx - 2, ground - h * 1.1, 2.2), T("gold"), "highlight")
        p.edge(orb, T("gold"), 1.0)
    else:
        legs = Path.rect(cx - w * 0.3, ground - h * 0.42, w * 0.6, h * 0.42, r=3)
        body = Path.smooth([(cx - w * 0.36, ground - h * 0.4), (cx - w * 0.5, ground - h * 0.66), (cx - w * 0.36, ground - h * 0.76), (cx, ground - h * 0.79),
                            (cx + w * 0.36, ground - h * 0.76), (cx + w * 0.5, ground - h * 0.66), (cx + w * 0.36, ground - h * 0.4)], closed=True)
        for part in (legs, body):
            style.shade(p.shape(part), "cloth", t, edge=P.INNER, rim=False)
        helm = Path().move(cx - w * 0.22, ground - h * 0.79).line(cx - w * 0.22, ground - h * 0.9).quad(cx, ground - h * 1.0, cx + w * 0.22, ground - h * 0.9) \
            .line(cx + w * 0.22, ground - h * 0.79).close()
        style.shade(p.shape(helm), "cloth", t, edge=P.INNER, rim=False)
        p.stroke(Path.polyline([(cx - w * 0.15, ground - h * 0.86), (cx + w * 0.15, ground - h * 0.86)]), 2.0, t, "deep")
        plume = Path.smooth([(cx, ground - h * 0.97), (cx + w * 0.2, ground - h * 1.04), (cx + w * 0.42, ground - h * 0.96), (cx + w * 0.3, ground - h * 0.92)])
        p.stroke(plume, (6, 2), t, "light")
        shield = Path().move(cx - w * 0.52, ground - h * 0.68).line(cx - w * 0.06, ground - h * 0.68).line(cx - w * 0.06, ground - h * 0.5) \
            .quad(cx - w * 0.08, ground - h * 0.36, cx - w * 0.29, ground - h * 0.3).quad(cx - w * 0.5, ground - h * 0.36, cx - w * 0.52, ground - h * 0.5).close()
        sm = p.fill(shield, t, "base-light")
        p.tone(p.where(sm, lambda xx, yy: xx > cx - w * 0.25), t, "terminator")
        P._icon(p, "sun", cx - w * 0.29, ground - h * 0.53, w * 0.3)
        p.edge(shield, t, 1.3)
        sw = cx + w * 0.22
        p.stroke(Path.polyline([(sw, ground - h * 0.62), (sw, ground - 3)]), 5, T("#c8ccd8"), "light")
        p.stroke(Path.polyline([(sw - 10, ground - h * 0.6), (sw + 10, ground - h * 0.6)]), 4, T("gold"), "base")


def p_statue(c):
    p = c.piece()
    t = T("#e6e2ea")
    ph = c.bh * 0.15
    plinth(p, c.mx, c.g, c.bw * 0.86, ph, t)
    figure_statue(p, c.mx - (c.bw * 0.05 if c.name == "statue" else 0), c.g - ph - 6, c.bh * 0.72, t, "hooded" if c.name == "statue" else "knight")
    if c.name == "statue":
        leafy_bush(p, c.mx - c.bw * 0.3, c.g - 1, c.bw * 0.36, 26, T("leaf"), c.seed, ("flower.pink", "flower.white"))
        leafy_bush(p, c.mx + c.bw * 0.3, c.g - 1, c.bw * 0.32, 24, T("leaf"), c.seed + 1, ("flower.yellow", "flower.white"))
    p.ground_shadow(c.x0, c.x1, c.g + 1, 6, 0.3)
    c.L.place(p)


def p_fountain(c):
    """The festival square's fountain, side on and a little from above: a wide round basin of cream
    stone (its water showing over the rim), a pedestal of two spilling tiers, the crystal at the
    top, posts with gold finials along the rim, flowers round its foot."""
    p = c.piece()
    st = T("stone")
    water = T("#5fb4ec")
    g = T("gold")
    bw = c.bw * 0.94
    rim_y = c.g - c.bh * 0.24
    ry = c.bh * 0.1
    # the basin's far rim and the water inside
    p.fill(Path.ellipse(c.mx, rim_y, bw / 2, ry), st, "shadow")
    wm = p.fill(Path.ellipse(c.mx, rim_y + 2, bw / 2 - 12, ry - 6), water, "base")
    for k in range(5):
        p.stroke(Path.ellipse(c.mx, rim_y + 2, bw / 2 * (0.25 + 0.15 * k), (ry - 6) * (0.25 + 0.15 * k)), 1.2, water, "light", clip=wm, alpha=0.7)
    # the pedestal and its tiers, spilling
    tiers = ((c.g - c.bh * 0.5, bw * 0.44), (c.g - c.bh * 0.7, bw * 0.26))
    col = Path.rect(c.mx - 14, tiers[1][0], 28, rim_y - tiers[1][0])
    stone_block(p, col, st)
    for ty, tw in tiers:
        for side in (-1, 1):
            sheet = Path.polygon([(c.mx + side * tw * 0.46, ty + 4), (c.mx + side * tw * 0.5, ty + 4), (c.mx + side * tw * 0.58, rim_y), (c.mx + side * tw * 0.48, rim_y)])
            sm = p.fill(sheet, water, "light", alpha=0.75)
            p.stroke(Path.polyline([(c.mx + side * tw * 0.49, ty + 6), (c.mx + side * tw * 0.55, rim_y - 4)]), 1.6, water, "highlight", alpha=0.9)
            del sm
        bowl = Path().move(c.mx - tw / 2, ty).quad(c.mx - tw * 0.32, ty + 26, c.mx, ty + 28).quad(c.mx + tw * 0.32, ty + 26, c.mx + tw / 2, ty).close()
        bm = p.fill(bowl, st, "base")
        p.tone(p.where(bm, lambda xx, yy: xx > c.mx + tw * 0.15), st, "shadow")
        p.edge(bowl, st, 1.2)
        top = Path.ellipse(c.mx, ty, tw / 2, 7)
        p.fill(top, st, "light")
        p.fill(Path.ellipse(c.mx, ty + 1, tw / 2 - 6, 4), water, "light")
        p.edge(top, st, 1.0)
    jet_y = tiers[1][0]
    crystal = Path.polygon([(c.mx, jet_y - c.bh * 0.24), (c.mx + 11, jet_y - c.bh * 0.14), (c.mx, jet_y - 4), (c.mx - 11, jet_y - c.bh * 0.14)])
    ct = T("#7fd0ff")
    cm = p.fill(crystal, ct, "base")
    p.tone(p.where(cm, lambda xx, yy: xx > c.mx), ct, "shadow")
    p.fill(Path.polygon([(c.mx - 2, jet_y - c.bh * 0.21), (c.mx - 8, jet_y - c.bh * 0.14), (c.mx - 2, jet_y - c.bh * 0.08)]), ct, "highlight")
    p.edge(crystal, ct, 1.2)
    p.glow(c.mx, jet_y - c.bh * 0.14, 70, "#9fe0ff", 0.45)
    # the basin's near wall and rim
    wall_top = rim_y
    front = Path().move(c.mx - bw / 2, wall_top).quad(c.mx, wall_top + ry * 2.0, c.mx + bw / 2, wall_top).line(c.mx + bw / 2, c.g - 6) \
        .quad(c.mx, c.g + 4, c.mx - bw / 2, c.g - 6).close()
    fm = p.fill(front, st, "base")
    for k in range(1, 9):
        fx = c.mx - bw / 2 + bw * k / 9
        p.stroke(Path.polyline([(fx, wall_top + ry * 1.2), (fx, c.g - 2)]), 1.0, st, "shadow", clip=fm)
    p.stroke(Path().move(c.mx - bw / 2, (wall_top + c.g) / 2 + 4).quad(c.mx, (wall_top + c.g) / 2 + ry * 2.2, c.mx + bw / 2, (wall_top + c.g) / 2 + 4), 1.0, st, "shadow", clip=fm)
    p.tone(p.where(fm, lambda xx, yy: xx > c.mx + bw * 0.3), st, "shadow")
    p.edge(front, st, 1.3)
    rim = Path().move(c.mx - bw / 2 - 4, wall_top - 4).quad(c.mx, wall_top + ry * 2.0 - 4, c.mx + bw / 2 + 4, wall_top - 4).line(c.mx + bw / 2 + 4, wall_top + 6) \
        .quad(c.mx, wall_top + ry * 2.0 + 6, c.mx - bw / 2 - 4, wall_top + 6).close()
    p.fill(rim, st, "light")
    p.edge(rim, st, 1.1)
    for k in range(5):
        u = k / 4
        px = c.mx - bw / 2 + bw * u
        py = wall_top + ry * 2.0 * (1 - (2 * u - 1) ** 2) * 0.5
        post_ = Path.rect(px - 10, py - 34, 20, 36, r=2)
        stone_block(p, post_, st)
        P._finial(p, px, py - 34, g)
    leafy_bush(p, c.mx - bw * 0.3, c.g - 1, bw * 0.32, 30, T("leaf"), c.seed, ("flower.pink", "flower.white"))
    leafy_bush(p, c.mx + bw * 0.3, c.g - 1, bw * 0.3, 28, T("leaf"), c.seed + 1, ("flower.yellow", "blossom"))
    p.ground_shadow(c.x0, c.x1, c.g + 1, 8, 0.3)
    c.L.place(p)


def p_hearth(c):
    """A tavern hearth: a stone chimney breast with a wide arched fire opening (its coals where the
    light's flame stands), a brick arch round it, a mantel with a copper pot on it."""
    p = c.piece()
    st = T("stone.warm")
    fx = c.cx + 1 * K
    bed = c.H - 20 * K
    P.wall(p, c.x0 + 4, c.y0 + 30, c.x1 - 4, c.g, "stone.warm", c.seed, quoins=False)
    p.edge(Path.rect(c.x0 + 4, c.y0 + 30, c.bw - 8, c.g - c.y0 - 30), st, 1.3)
    stone_block(p, Path.rect(c.mx - c.bw * 0.24, c.y0 + 2, c.bw * 0.48, 34), st)
    ow = c.bw * 0.72
    arch = Path().move(fx - ow / 2 - 10, bed + 22).line(fx - ow / 2 - 10, bed - 44).quad(fx, bed - 84, fx + ow / 2 + 10, bed - 44).line(fx + ow / 2 + 10, bed + 22).close()
    stone_block(p, arch, T("brick"))
    op = Path().move(fx - ow / 2, bed + 22).line(fx - ow / 2, bed - 40).quad(fx, bed - 72, fx + ow / 2, bed - 40).line(fx + ow / 2, bed + 22).close()
    dark = T("#3a2a30")
    om = p.fill(op, dark, "base")
    p.tone(om - om.shift(6, 6), dark, "deep")
    p.tone(p.below(om, bed - 20), T("#7a3a2a"), "base", alpha=0.6)
    logs = T("bark")
    for k in range(2):
        lp = Path.rect(fx - ow * 0.32 + k * 10, bed + 2 - k * 6, ow * 0.6, 9, r=4)
        p.fill(lp, logs, "base")
        p.edge(lp, logs, 1.0)
    flame_bed(p, fx, bed + 6, ow * 0.7, c.seed)
    p.glow(fx, bed - 10, ow * 1.3, "#ff9a48", 0.4)
    mantel = Path.rect(c.x0, bed - 92, c.bw, 12, r=2)
    p.fill(mantel, T("wood.dark"), "base")
    p.edge(mantel, T("wood.dark"), 1.2)
    pot = Path().move(c.x1 - 40, bed - 92).line(c.x1 - 14, bed - 92).quad(c.x1 - 10, bed - 104, c.x1 - 22, bed - 112).line(c.x1 - 32, bed - 112) \
        .quad(c.x1 - 44, bed - 104, c.x1 - 40, bed - 92).close()
    pt = T("#c8784a")
    pm = p.fill(pot, pt, "base")
    shade_lr(p, pm, pt, c.x1 - 27, 26)
    p.edge(pot, pt, 1.0)
    c.L.place(p)


def p_war_table(c):
    """The war table: a heavy table seen a little from above, a map of the realm spread over it
    (sea, coast, a river, forests), banners pinned on it."""
    p = c.piece()
    wood = T("wood.dark")
    y = c.y0 + c.bh * 0.42
    for lx in (c.x0 + 10, c.mx - 5, c.x1 - 20):
        leg(p, lx, y + 10, 10, c.g - y - 10, wood)
    top = Path.polygon([(c.x0 + c.bw * 0.08, c.y0 + 4), (c.x1 - c.bw * 0.08, c.y0 + 4), (c.x1, y), (c.x0, y)])
    p.fill(top, wood, "light")
    p.edge(top, wood, 1.2)
    p.fill(Path.rect(c.x0, y, c.bw, 12), wood, "base")
    p.edge(Path.rect(c.x0, y, c.bw, 12), wood, 1.2)
    mp = Path.polygon([(c.x0 + c.bw * 0.14, c.y0 + 10), (c.x1 - c.bw * 0.14, c.y0 + 10), (c.x1 - c.bw * 0.07, y - 6), (c.x0 + c.bw * 0.07, y - 6)])
    mt = T("#ead8b0")
    m = p.fill(mp, mt, "base")
    sea = p.shape(Path.smooth([(c.x0, c.y0), (c.mx - c.bw * 0.1, c.y0), (c.mx - c.bw * 0.2, c.y0 + c.bh * 0.2), (c.x0 + c.bw * 0.2, y), (c.x0, y)], closed=True)) & m
    p.tone(sea, T("#8ab8d8"), "base")
    for k in range(5):
        fx = c.mx + (k - 1) * c.bw * 0.08
        fy = c.y0 + c.bh * (0.14 + 0.05 * (k % 3))
        p.fill(Path.circle(fx, fy, 4), T("leaf.dark"), "base", clip=m)
    p.stroke(Path.smooth([(c.mx - c.bw * 0.05, c.y0 + 12), (c.mx + c.bw * 0.05, c.y0 + c.bh * 0.25), (c.mx + c.bw * 0.2, y - 8)]), 2.0, T("water"), "base", clip=m)
    p.edge(mp, mt, 1.0)
    for k, (fx, fy, col) in enumerate(((0.35, 0.2, "cloth.red"), (0.55, 0.3, "cloth.navy"), (0.68, 0.16, "cloth.red"), (0.45, 0.34, "cloth.navy"))):
        x, yy = c.x0 + c.bw * fx, c.y0 + c.bh * fy
        p.stroke(Path.polyline([(x, yy), (x, yy - 14)]), 1.2, T("wood.dark"), "base")
        flag = Path.polygon([(x, yy - 14), (x + 9, yy - 11), (x, yy - 8)])
        p.fill(flag, T(col), "base")
    p.ground_shadow(c.x0, c.x1, c.g + 1, 6, 0.3)
    c.L.place(p)


def p_booth(c):
    """A tavern booth: a high-backed bench of panelled wood with a red cushion, and its table."""
    p = c.piece()
    wood = T("wood")
    back = Path.rect(c.x0 + 4, c.y0 + 4, c.bw - 8, c.bh * 0.8, r=4)
    bm = p.fill(back, wood, "base")
    for i in range(4):
        pm = p.shape(Path.rect(c.x0 + 12 + i * (c.bw - 24) / 4, c.y0 + 14, (c.bw - 24) / 4 - 8, c.bh * 0.42, r=3)) & bm
        p.tone(pm, wood, "terminator")
        p.tone(pm - pm.shift(2, 2), wood, "deep-shadow")
        p.tone(pm - pm.shift(-2, -2), wood, "light")
    cap = Path.rect(c.x0, c.y0, c.bw, 9, r=2)
    p.fill(cap, wood, "light")
    p.edge(cap, wood, 1.0)
    p.edge(back, wood, 1.3)
    cush = Path.rect(c.x0 + 8, c.g - c.bh * 0.42, c.bw - 16, 14, r=6)
    p.fill(cush, T("cloth.red"), "base")
    p.tone(p.shape(cush) - p.shape(cush).shift(0, 2), T("cloth.red"), "light")
    p.edge(cush, T("cloth.red"), 1.1)
    tw = T("wood.dark")
    table_top(p, c.x0 + c.bw * 0.15, c.x1 - c.bw * 0.15, c.g - c.bh * 0.3, tw, 9, 10)
    leg(p, c.mx - 4, c.g - c.bh * 0.3 + 9, 8, c.bh * 0.3 - 9, tw)
    for tx in (c.mx - c.bw * 0.15, c.mx + c.bw * 0.1):
        tk = T("pottery")
        tp = Path.rect(tx - 6, c.g - c.bh * 0.3 - 22, 12, 16, r=2)
        p.fill(tp, tk, "base")
        p.edge(tp, tk, 1.0)
    p.ground_shadow(c.x0, c.x1, c.g + 1, 6, 0.3)
    c.L.place(p)


PAINTERS_4 = {"war-table": p_war_table, "booth": p_booth, "hearth": p_hearth, "fountain": p_fountain}
for _n in TREE_COLOURS:
    PAINTERS_4[_n] = p_tree
for _n in BUSHES:
    PAINTERS_4[_n] = p_bush
for _n in ("cypress", "cypress-pot"):
    PAINTERS_4[_n] = p_cypress
for _n in ("flowers-mixed", "flowers-pink", "flowers-white"):
    PAINTERS_4[_n] = p_flower_clump
for _n in ("rocks", "rocks-mossy", "rubble-heap"):
    PAINTERS_4[_n] = p_rocks
for _n in ("gold-pile", "gold-pile-small"):
    PAINTERS_4[_n] = p_gold
for _n in ("sacks", "sack-pile"):
    PAINTERS_4[_n] = p_sacks
for _n in ("statue", "guard-statue"):
    PAINTERS_4[_n] = p_statue


PAINTERS_2 = {}
for _n in ("barrel", "barrel-flowers", "barrel-salt", "barrel-water", "cargo-barrel", "cask"):
    PAINTERS_2[_n] = p_barrel
for _n in ("crate", "crate-apples", "crate-onions", "crate-open", "crate-stack", "cargo-crate", "book-crate"):
    PAINTERS_2[_n] = p_crate
for _n in ("sacks", "sack-pile"):
    PAINTERS_2[_n] = p_sacks
for _n in ("refugee-bundle", "bedroll", "satchel", "rope-coil"):
    PAINTERS_2[_n] = p_bundle
for _n in ("chair", "stool", "armchair"):
    PAINTERS_2[_n] = p_chair
for _n in ("table", "round-table", "side-table", "tankard-table", "wine-table", "desk", "royal-desk", "war-table"):
    PAINTERS_2[_n] = p_table
for _n in ("bed", "cot"):
    PAINTERS_2[_n] = p_bed
for _n in ("wardrobe", "bookshelf", "relic-shelf", "utensil-rack", "tool-rack"):
    PAINTERS_2[_n] = p_cabinet
for _n in ("shop-counter", "bar-counter"):
    PAINTERS_2[_n] = p_counter
for _n in ("lamp", "lamp-arm", "lamp-double", "lantern-post", "lantern-small"):
    PAINTERS_2[_n] = p_lamp
for _n in ("sconce", "vault-sconce"):
    PAINTERS_2[_n] = p_sconce
for _n in ("brazier", "vault-brazier"):
    PAINTERS_2[_n] = p_brazier
for _n in ("campfire", "cookfire"):
    PAINTERS_2[_n] = p_fire
for _n in ("banner-blue", "banner-pink"):
    PAINTERS_2[_n] = p_banner_stand
for _n in ("sun-banner", "tapestry"):
    PAINTERS_2[_n] = p_wall_cloth
for _n in ("notice-board", "sign", "ranger-sign", "signpost", "signpost-arrows"):
    PAINTERS_2[_n] = p_board
PAINTERS_2.update({
    "keg-stack": p_keg_stack, "booth": p_booth, "bench": p_bench, "chalkboard": p_chalkboard,
    "lantern-hanging": p_lantern_hanging, "candle-stand": p_candle_stand, "hearth": p_hearth,
})

PAINTERS_3 = {
    "fence-wood": p_fence, "fence-wood-post": p_fence,
    "bollard": p_bollard, "net-rack": p_posts_and_line, "rope-posts": p_posts_and_line, "washing-line": p_posts_and_line,
    "harbour-bell": p_harbour_bell, "ship-wheel": p_ship_wheel, "ship": p_ship, "gangplank": p_gangplank, "mast": p_mast,
    "fish-stall": p_market_stall, "stall-cloth": p_market_stall, "stall-teal": p_market_stall,
    "handcart": p_cart, "flower-cart": p_cart,
    "rocks": p_rocks, "rocks-mossy": p_rocks, "rubble-heap": p_rocks, "urn": p_urn, "urn-blue": p_urn,
    "pillar-marble": p_pillar, "statue": p_statue, "guard-statue": p_statue, "fountain": p_fountain, "keep-arch": p_keep_arch,
    "anvil": p_anvil, "sun-throne": p_throne,
    "sunheart-pedestal": p_pedestal, "relic-case": p_pedestal, "relic-case-orb": p_pedestal,
    "chest": p_chest, "vault-chest": p_chest, "gold-pile": p_gold, "gold-pile-small": p_gold,
    "black-crystal": p_crystal, "black-crystal-purified": p_crystal, "black-crystal-shattered": p_crystal,
    "crystal-stone": p_waystone, "waypoint": p_waystone, "lever": p_lever, "vault-lever": p_lever, "dart-trap": p_dart_trap,
    "root-wall": p_root_wall, "sun-mosaic": p_decal, "tar-pool": p_decal, "sapper-hole": p_decal, "bones": p_decal,
    "courier-body": p_courier, "vault-stair": p_vault_stair, "vault-stair-shut": p_vault_stair,
}


PAINTERS = {}
for _n in TREE_COLOURS:
    PAINTERS[_n] = p_tree
for _n in ("dead-tree", "tree-blackvein-bare"):
    PAINTERS[_n] = p_bare_tree
for _n in ("fir", "fir-deep"):
    PAINTERS[_n] = p_conifer
for _n in ("cypress", "cypress-pot"):
    PAINTERS[_n] = p_cypress
for _n in BUSHES:
    PAINTERS[_n] = p_bush
for _n in ("flowers-mixed", "flowers-pink", "flowers-white"):
    PAINTERS[_n] = p_flower_clump
for _n in ("wildflowers", "wildflowers-blue", "wildflowers-yellow"):
    PAINTERS[_n] = p_wildflowers
for _n in ("mushrooms", "mushrooms-pale", "gloom-mushroom", "gloom-mushroom-small"):
    PAINTERS[_n] = p_mushrooms
PAINTERS.update({
    "topiary": p_topiary, "stump": p_stump, "woodcutter-stump": p_stump, "snag": p_snag, "log": p_log,
    "fallen-tree": p_fallen_tree, "fern": p_fern, "grass-tall": p_grass, "potted-plant": p_potted,
    "planter": p_planter, "hay-bale": p_hay,
})
PAINTERS.update(PAINTERS_2)
PAINTERS.update(PAINTERS_3)
PAINTERS.update(PAINTERS_4)


# --------------------------------------------------------------------------- #
# Painting, the sheet, the command line
# --------------------------------------------------------------------------- #

def paint_prop(name, out=OUT, quiet=False):
    """Paints one prop, every frame, into out/<id>.png. Returns its path, or None if nothing paints it."""
    fn = PAINTERS.get(name)
    if fn is None:
        return None
    olds = old_frames(name)
    h, w = olds[0].shape[:2]
    W, H = w * K, h * K
    frames = []
    for i, old in enumerate(olds):
        c = Frame(name, i, W, H, fit(old), seed_of(name))
        fn(c)
        frames.append(c.L.rgba())
    img = np.concatenate(frames, axis=1)
    path = os.path.join(out, name + ".png")
    write_png(path, img, quiet=quiet)
    return path


def sheet(names, path, out=OUT):
    """props-sheet.png: each prop's old picture (4x nearest) beside its new one, on the rooms' floor colour."""
    tiles = []
    for n in names:
        new = os.path.join(out, n + ".png")
        if not os.path.isfile(new):
            continue
        olds = old_frames(n)
        old = vector.scale_nearest(olds[0], K)
        nw = read_png(new)
        nw = nw[:, :nw.shape[1] // len(olds)]
        if old.shape[1] > 700:
            old, nw = old[:, :], nw[:, :]
        tiles.append((n, old, nw))
    W = 2400
    x = y = 12
    rowh = 0
    pos = []
    for n, old, nw in tiles:
        w = old.shape[1] + nw.shape[1] + 10
        h = old.shape[0]
        if x + w > W:
            x, y, rowh = 12, y + rowh + 24, 0
        pos.append((x, y, old, nw))
        x += w + 26
        rowh = max(rowh, h)
    H = y + rowh + 16
    canvas = np.zeros((H, W, 4))
    canvas[..., :3] = np.array((214, 201, 184)) / 255.0
    canvas[..., 3] = 1.0
    for x, y, old, nw in pos:
        vector.over(canvas, vector.premultiply(old), x, y)
        vector.over(canvas, vector.premultiply(nw), x + old.shape[1] + 10, y)
    write_png(path, vector.unpremultiply(canvas))


def paint(preview=None, names=None, out=OUT):
    """paint_all.py's hook: paints every prop a room uses (or `names`), and with preview (a folder)
    the sheet there. Returns the files written, relative to Content."""
    names = names or used_props()
    written = []
    missing = []
    for n in names:
        path = paint_prop(n, out, quiet=True)
        if path is None:
            missing.append(n)
        else:
            written.append(os.path.relpath(path, roomkit.CONTENT).replace(os.sep, "/"))
    print("  painted %d props into %s" % (len(written), os.path.relpath(out, roomkit.ROOT).replace(os.sep, "/")))
    if missing:
        print("  nothing paints these yet: " + ", ".join(missing))
    if preview:
        sheet(names, os.path.join(preview, "props-sheet.png"), out)
    return written


def assets_lines(out=OUT):
    """paint_all.py's ASSETS.md line for the painted props: ([the files], the line), naming every
    picture in Content/Props/Painted."""
    files = ["Content/Props/Painted/" + n for n in sorted(os.listdir(out)) if n.endswith(".png")] if os.path.isdir(out) else []
    if not files:
        return []
    return [(files, ", ".join(files) + " - created by Caleb Kiatoukaysi for Legend Quest, the floor props in the "
             "illustrated style, each the prop of the same name in Content/Props repainted at four times its size with "
             "the same frames, foot and footprint: trees, bushes, flowers and plants; boxes, barrels, sacks and bundles; "
             "furniture; lamps, torches and candles (the game burns their flames); banners, boards and signs; the docks' "
             "and the ship's pieces; market stalls and carts; stone, statues, the fountain and the vaults' treasures, "
             "painted procedurally by tools/rooms/paint_props.py with tools/rooms/paint.py, released under public domain")]


def main():
    ap = argparse.ArgumentParser(description="Paint the floor props in the painted style, into Content/Props/Painted.")
    ap.add_argument("only", nargs="*", help="prop ids (default: every prop a room uses)")
    ap.add_argument("--out", default=OUT, help="where <id>.png goes (default Content/Props/Painted)")
    ap.add_argument("--sheet", nargs="?", const=SHOTS, help="also write props-sheet.png into this folder (default shots/art/rooms)")
    args = ap.parse_args()
    paint(args.sheet, args.only or None, args.out)


if __name__ == "__main__":
    main()
