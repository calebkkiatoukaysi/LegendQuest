"""
The wild, painted (12-rooms §6): the Old Road's farmland and fork, the Mirewood's edge, deep wood
and camp, the Old Quarry under it (its lip, the second cut and the floor), and Thornbridge's camp
gate, tents and river's edge. One recipe a room, each a function of a paint_room.Room, registered
in RECIPES; tools/rooms/paint_all.py runs them.

    python tools/rooms/paint_all.py wild --no-shared --preview shots/art/rooms
    python tools/rooms/paint_room.py road/farmland --preview shots/art/rooms

The look, for every room here, is the Lantern Market's (paint.py's): clean dark outlines, flat
fills cel-shaded in two or three tones lit from the top left, soft gradients only in the sky and
the distance. The meadows outside Questeria are bright and lush at noon: rolling patchwork hills,
hedgerows full of flowers, gold wheat, a windmill and a farmhouse. The Mirewood is the same green
going wrong toward the east under a twilight sky: old trees with flaring roots and moss, mist
along the ground, the bark split by black veins with a violet glow in them; the rangers' camp is a
clearing at night by firelight. Thornbridge is the meadow under Questeria's east wall, full of
patched tents and cooking fires, and the old bridge over the river. The quarry's rooms are dark
(the game darkens them with its light map), painted in readable mid values with their own light
where the zone file puts lights: fungus, crystal, daylight at the mouth.

The parts this adds, built on paint.py's Piece and tones so they match its own: bank() (the back
of a meadow), hedgerow(), wheat_field(), haystack(), windmill(), rail_fence(), washing_line(),
signpost(), field_hills() (patchwork hills, far), big_tree() (oak, old, black-veined, bare, with
roots), fir(), mist(), boulders(), mushrooms(), tent(), campfire(), lean_to(), stone_bridge(),
river(), barrow_mound(), mine_mouth(), city_wall(), and fore_tree() for the foreground. The rock,
crystal, fungus and torch parts the quarry uses are paint_rooms_under.py's.

Long things (a bank, a hedge, a field) are drawn in overlapping chunks and each laid in only over
its own stretch (place_span), so a 3200-wide room costs what its chunks do and no outline shows
where two chunks meet.

Deterministic: every bit of noise is paint.hash01 seeded from the room's name. Only needs CPython
and numpy.
"""

import math
import os
import sys

import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
import paint as P  # noqa: E402
import roomkit  # noqa: E402
from paint import Door, House, Path, Piece, Window, hash01, haze, tones  # noqa: E402
from roomkit import CANVAS_H  # noqa: E402

from paint_rooms_under import CHUNK, OVERLAP, chunks, place_span  # noqa: E402,F401  (the long things' chunks)
import paint_rooms_under as U  # noqa: E402


def seeded(zone, room):
    return roomkit.seed_of(zone, room)


def pick(seq, i, k, seed):
    return seq[int(hash01(i, k, seed) * len(seq)) % len(seq)]


# --------------------------------------------------------------------------- #
# The meadow: banks, hedges, fields, haystacks, fences
# --------------------------------------------------------------------------- #

GRASS = "#74b957"
"""The meadow's near grass in the mid layer: a sunny, lush green."""
WHEAT = "#ecc45e"
HAY = "#e6b955"


def _bumpy(x0, x1, top, amp, step, seed):
    """Points along a gently bumpy top line from x0 to x1 (a step apart, starting a step early)."""
    pts = []
    i = int(math.floor(x0 / step)) - 2
    while i * step <= x1 + 2 * step:
        pts.append((i * step, top + (hash01(i, 1, seed) - 0.5) * 2 * amp))
        i += 1
    return pts


def bank(layer, x0, x1, ground, top, colour=GRASS, seed=0, fade=24, amp=7, step=90, tufts=True, flowers=None):
    """The back of a meadow (or any ground) behind the floor: from a gently bumpy top line at about
    `top` down past the floor's back edge, lit along its rim, with tufts of grass along it and
    flowers if asked; it fades into the floor over `fade` pixels under `ground`."""
    t = tones(colour)
    pts = _bumpy(x0, x1, top, amp, step, seed)
    for xa, xb in chunks(x0, x1):
        cx0, cx1 = xa - OVERLAP, xb + OVERLAP
        near = [q for q in pts if cx0 - 2 * step <= q[0] <= cx1 + 2 * step]
        p = Piece(cx0, top - amp - 24, cx1, ground + fade, ss=2, pad=4)
        body = Path.smooth(near)
        body.line(near[-1][0], ground + fade + 4).line(near[0][0], ground + fade + 4).close()
        m = p.fill(body, t, "base")
        p.tone(m - m.shift(0, 8), t, "light")
        p.tone(m - m.shift(0, 3), t, "base-light")
        p.tone(p.below(m, ground - 14), t, "terminator", alpha=0.55)
        if tufts:
            blades = Path()
            n = int((cx1 - cx0) / 22)
            for i in range(n):
                gx = math.floor(cx0 / 22) * 22 + i * 22 + 11 * hash01(int(cx0 / 22) + i, 3, seed)
                k = int(gx / 22)
                gy = _line_y(near, gx) + 14 + 10 * hash01(k, 5, seed)
                h = 6 + 5 * hash01(k, 4, seed)
                for j in range(3):
                    a = math.radians(-90 + (j - 1) * 26)
                    blades.add(Path.polyline([(gx + (j - 1) * 2, gy), (gx + (j - 1) * 2 + math.cos(a) * h, gy + math.sin(a) * h)]))
            p.stroke(blades, (2.0, 0.0), t, "shadow", clip=m, alpha=0.8)
            p.stroke(blades.translate(-0.6, -0.8), (1.2, 0.0), t, "light", clip=m, alpha=0.8)
        if flowers:
            for i in range(int((cx1 - cx0) / 26)):
                fx = math.floor(cx0 / 26) * 26 + i * 26 + 20 * hash01(int(cx0 / 26) + i, 5, seed)
                k = int(fx / 26)
                fy = _line_y(near, fx) + 10 + (ground - top - 16) * hash01(k, 6, seed)
                c = tones(pick(flowers, k, 7, seed))
                fm = p.fill(Path.circle(fx, fy, 2.6), c, "base", clip=m)
                p.tone(fm - fm.shift(-0.7, -0.7), c, "shadow")
        place_span(layer, p, xa, xb, fade=(ground - 2, ground + fade))


def _line_y(pts, x):
    """The y of a polyline through pts (sorted by x) at x."""
    for (ax, ay), (bx, by) in zip(pts, pts[1:]):
        if ax <= x <= bx:
            u = (x - ax) / max(1e-6, bx - ax)
            uu = u * u * (3 - 2 * u)
            return ay + (by - ay) * uu
    return pts[-1][1] if x > pts[-1][0] else pts[0][1]


def hedgerow(layer, x0, x1, ground, h=70, colour="leaf", seed=0, flowers=("flower.white", "flower.pink"), back="leaf.dark"):
    """A hedgerow from x0 to x1 standing on `ground`: a long heap of round clumps of leaves (the
    ones behind a step darker), lit top left, with flowers in it if asked."""
    t, tb = tones(colour), tones(back)
    clumps = []
    x = x0 + 10
    i = 0
    while x < x1 - 6:
        r = h * (0.36 + 0.16 * hash01(i, 1, seed))
        clumps.append((x, ground - r * 0.95 - h * 0.18 * hash01(i, 2, seed), r, 1 if i % 2 else 0))
        x += r * (1.05 + 0.4 * hash01(i, 3, seed))
        i += 1
    for xa, xb in chunks(x0, x1):
        cx0, cx1 = xa - OVERLAP, xb + OVERLAP
        near = [c for c in clumps if cx0 - c[2] - 4 <= c[0] <= cx1 + c[2] + 4]
        if not near:
            continue
        p = Piece(cx0, ground - h * 1.3, cx1, ground + 10, pad=6)
        backs = sorted([c for c in near if c[3]], key=lambda c: c[1])
        fronts = sorted([c for c in near if not c[3]], key=lambda c: c[1])
        whole = P._clumps(p, backs, t, tb) if backs else p.nothing()
        whole = whole | P._clumps(p, fronts, t)
        foot = p.shape(Path.rect(x0, ground - h * 0.32, x1 - x0, h * 0.32 + 2)) & whole
        p.tone(foot, t, "shadow", alpha=0.6)
        if flowers:
            for (cx, cy, r, d) in near:
                k = int(cx * 7) % 9973
                for j in range(3):
                    if hash01(k, j, seed + 5) < 0.55:
                        a = hash01(k, j + 3, seed + 5) * 6.28
                        fx, fy = cx + math.cos(a) * r * 0.55, cy + math.sin(a) * r * 0.5 - r * 0.15
                        c = tones(pick(flowers, k, j, seed + 7))
                        fm = p.fill(Path.circle(fx, fy, 3.0), c, "base", clip=whole)
                        p.tone(fm - fm.shift(-0.8, -0.8), c, "shadow")
                        p.fill(Path.circle(fx, fy, 0.9), tones("flower.yellow"), "shadow")
        for (cx, cy, r, d) in near:
            if xa <= cx < xb:
                p.ground_shadow(cx - r, cx + r, ground + 1, 6, 0.22)
        place_span(layer, p, xa, xb)


def wheat_field(layer, x0, x1, ground, h=56, seed=0, colour=WHEAT):
    """A field of ripe wheat seen side on, from x0 to x1 on `ground`: a band of gold stalks with a
    ragged top of ears, each ear a little plaited ellipse lit on its left, the stalks darkening
    toward their feet."""
    t = tones(colour)
    ears = []
    i = int(x0 // 7)
    while i * 7 < x1:
        ex = i * 7 + 3 * hash01(i, 1, seed)
        if x0 + 4 < ex < x1 - 4:
            ey = ground - h + 4 * math.sin(ex / 37.0) + 5 * hash01(i, 2, seed)
            lean = (hash01(i, 3, seed) - 0.4) * 22
            ears.append((ex, ey, lean, i))
        i += 1
    for xa, xb in chunks(x0, x1):
        cx0, cx1 = xa - OVERLAP, xb + OVERLAP
        p = Piece(max(x0 - 8, cx0), ground - h - 22, min(x1 + 8, cx1), ground + 8, pad=4)
        body = Path()
        near = [e for e in ears if cx0 - 20 <= e[0] <= cx1 + 20]
        top = [(e[0], e[1] + 8) for e in near]
        if len(top) < 2:
            continue
        body = Path.polyline([(top[0][0], ground + 2)] + top + [(top[-1][0], ground + 2)]).close()
        m = p.fill(body, t, "base")
        p.tone(p.below(m, ground - h * 0.45), t, "shadow")
        p.tone(p.below(m, ground - h * 0.18), t, "deep-shadow", alpha=0.7)
        stalks = Path()
        for (ex, ey, lean, k) in near:
            stalks.add(Path.polyline([(ex + lean * 0.3, ey + 8), (ex + lean * 0.05, ground + 2)]))
        p.stroke(stalks, 1.0, t, "terminator", clip=m, alpha=0.8)
        whole = m
        for (ex, ey, lean, k) in near:
            ear = Path.ellipse(ex + lean * 0.3, ey, 2.9, 7.5, rot=lean)
            em = p.fill(ear, t, "base-light")
            p.tone(em - em.shift(-1.1, -0.6), t, "shadow")
            for j in range(3):
                yy = ey - 4 + j * 3.5
                p.stroke(Path.polyline([(ex + lean * 0.3 - 2.4, yy), (ex + lean * 0.3 + 2.4, yy + 1.2)]), 0.7, t, "terminator", clip=em)
            if hash01(k, 9, seed) < 0.5:
                p.stroke(Path.polyline([(ex + lean * 0.3, ey - 7), (ex + lean * 0.5 + 1, ey - 15)]), (0.8, 0.0), t, "terminator")
            whole = whole | em
        p.ground_shadow(xa, xb, ground + 1, 6, 0.25)
        place_span(layer, p, xa, xb)


def haystack(layer, x, ground, w=110, h=96, seed=0, colour=HAY):
    """A round-topped haystack: a domed heap of straw, lit on its top left, shadowed on its right
    and its foot, with strands of straw and a pole out of the top."""
    p = Piece(x - w / 2 - 10, ground - h - 30, x + w / 2 + 10, ground + 10)
    t = tones(colour)
    p.stroke(Path.polyline([(x + 2, ground - h + 6), (x + 4, ground - h - 22)]), 3.2, tones("wood"), "base")
    body = Path().move(x - w / 2, ground).quad(x - w * 0.56, ground - h * 0.55, x - w * 0.22, ground - h * 0.9) \
        .quad(x, ground - h * 1.04, x + w * 0.22, ground - h * 0.9).quad(x + w * 0.56, ground - h * 0.55, x + w / 2, ground).close()
    m = p.fill(body, t, "base")
    p.tone(m - p.shape(Path.ellipse(x - w * 0.12, ground - h * 0.55, w * 0.5, h * 0.62)), t, "shadow")
    p.tone(m & p.shape(Path.ellipse(x - w * 0.2, ground - h * 0.72, w * 0.22, h * 0.2)), t, "light")
    p.tone(p.below(m, ground - h * 0.16), t, "terminator", alpha=0.8)
    strands = Path()
    for i in range(int(w * h / 160)):
        a = hash01(i, 1, seed) * math.pi
        rr = hash01(i, 2, seed) ** 0.6
        sx = x + math.cos(a) * w * 0.42 * rr * (1 if i % 2 else -1)
        sy = ground - h * 0.08 - h * 0.8 * hash01(i, 3, seed)
        ln = 6 + 6 * hash01(i, 4, seed)
        tilt = (sx - x) / w * 1.2
        strands.add(Path.polyline([(sx, sy), (sx + tilt * ln, sy + ln)]))
    p.stroke(strands, 0.9, t, "deep-shadow", clip=m, alpha=0.75)
    # a band of rope round its middle
    band = p.line_shape(Path.smooth([(x - w * 0.52, ground - h * 0.38), (x, ground - h * 0.43), (x + w * 0.52, ground - h * 0.38)]), 3.0) & m
    p.tone(band, tones("rope"), "base")
    p.edge(body, t, 1.3)
    p.ground_shadow(x - w * 0.6, x + w * 0.6, ground + 2, 8, 0.33)
    layer.place(p)
    return p


def rail_fence(layer, x0, x1, ground, h=48, colour="wood.light", seed=0, gaps=()):
    """A country fence of split rails between round-topped posts, a little crooked, grey-gold with
    weather; no rails across the `gaps` ((xa, xb), a path through)."""
    t = tones(colour)
    posts = []
    x = x0
    i = 0
    while x <= x1:
        if not any(a < x < b for a, b in gaps):
            posts.append((x, (hash01(i, 1, seed) - 0.5) * 4))
        x += 64 + 10 * hash01(i, 2, seed)
        i += 1
    for a, b in gaps:
        posts += [(a, 0.0), (b, 0.0)]
    posts.sort()
    for xa, xb in chunks(x0, x1 + 1):
        cx0, cx1 = xa - OVERLAP, xb + OVERLAP
        p = Piece(cx0, ground - h - 12, cx1, ground + 8, pad=4)
        for (ya, yb) in ((ground - h * 0.78, ground - h * 0.74), (ground - h * 0.4, ground - h * 0.44)):
            for (pa, _), (pb, _) in zip(posts, posts[1:]):
                if pb < cx0 - 10 or pa > cx1 + 10:
                    continue
                if any(a - 6 <= (pa + pb) / 2 <= b + 6 for a, b in gaps):
                    continue
                rail = Path.polygon([(pa - 6, ya - 3), (pb + 6, yb - 3), (pb + 6, yb + 4), (pa - 6, ya + 4)])
                rm = p.fill(rail, t, "base")
                p.tone(rm - rm.shift(0, 2), t, "light")
                p.stroke(Path.polyline([(pa + 10, ya + 0.5), (pb - 10, yb + 0.5)]), 0.8, t, "terminator", clip=rm)
                p.edge(rail, t, 1.0)
        for (px, lean) in posts:
            if not (cx0 - 10 <= px <= cx1 + 10):
                continue
            post = Path().move(px - 5, ground + 1).line(px - 5 + lean, ground - h + 3).quad(px + lean, ground - h - 4, px + 5 + lean, ground - h + 3) \
                .line(px + 5, ground + 1).close()
            pm = p.fill(post, t, "base")
            p.tone(p.where(pm, lambda xx, yy, a=px: xx > a + 1.5), t, "shadow")
            p.tone(p.where(pm, lambda xx, yy, a=px: xx < a - 3), t, "light")
            p.edge(post, t, 1.1)
            p.ground_shadow(px - 8, px + 8, ground + 1, 4, 0.28)
        place_span(layer, p, xa, xb)


def washing_line(layer, x0, x1, ground, h=150, seed=0, cloths=("cloth.cream", "cloth.blue", "cloth.pink", "cloth.cream", "cloth.gold", "cloth.teal")):
    """A washing line between two T-posts with sheets, shirts and stockings pegged on it."""
    p = Piece(x0 - 20, ground - h - 16, x1 + 20, ground + 8)
    wood = tones("wood")
    for px in (x0, x1):
        post = Path.rect(px - 4, ground - h, 8, h)
        pm = p.fill(post, wood, "base")
        p.tone(p.where(pm, lambda xx, yy, a=px: xx > a + 1), wood, "shadow")
        p.edge(post, wood, 1.0)
        arm = Path.rect(px - 16, ground - h - 2, 32, 6, r=2)
        p.fill(arm, wood, "light")
        p.edge(arm, wood, 1.0)
    sag = 26
    cord = Path().move(x0, ground - h + 2).quad((x0 + x1) / 2, ground - h + 2 + sag * 2, x1, ground - h + 2)
    p.stroke(cord, 1.3, tones("rope"), "deep")
    n = len(cloths)
    for i, c in enumerate(cloths):
        u0 = (i + 0.12) / n
        u1 = (i + 0.82) / n
        (ax, ay), _ = cord.at(u0)
        (bx, by), _ = cord.at(u1)
        kind = int(hash01(i, 1, seed) * 3)
        t = tones(c)
        if kind == 0:   # a sheet
            ln = 54 + 16 * hash01(i, 2, seed)
            sp = Path.polygon([(ax, ay), (bx, by), (bx + 2, by + ln), ((ax + bx) / 2, by + ln - 5), (ax - 1, ay + ln)])
        elif kind == 1:  # a shirt
            mx = (ax + bx) / 2
            sp = Path.polygon([(ax, ay), (bx, by), (bx + 8, by + 14), (bx - 3, by + 16), (bx - 3, by + 40), (ax + 3, ay + 40),
                               (ax + 3, ay + 16), (ax - 8, ay + 14)])
            del mx
        else:           # a stocking
            bx = ax + 12
            sp = Path.polygon([(ax, ay), (ax + 11, ay), (ax + 11, ay + 34), (ax + 20, ay + 40), (ax + 18, ay + 46), (ax, ay + 40)])
        m = p.fill(sp, t, "base")
        p.tone(p.where(m, lambda xx, yy, a=(ax + bx) / 2: xx > a + 4), t, "shadow")
        p.tone(m - m.shift(-1.6, 0), t, "light", alpha=0.7)
        p.edge(sp, t, 1.1)
        for px_ in (ax + 2, bx - 2):
            p.fill(Path.rect(px_ - 1.4, ay - 4, 2.8, 8, r=1), tones("wood.light"), "base")
    p.ground_shadow(x0 - 10, x0 + 10, ground + 1, 4, 0.28)
    p.ground_shadow(x1 - 10, x1 + 10, ground + 1, 4, 0.28)
    layer.place(p)
    return p


def signpost(layer, x, ground, h=130, arms=((1, "cloth.cream"), (-1, "cloth.cream")), seed=0):
    """A wooden signpost with arrow boards pointing each way (side -1 west, 1 east), the boards'
    lettering only suggested by dashes."""
    p = Piece(x - 90, ground - h - 20, x + 90, ground + 8)
    wood = tones("wood")
    post = Path.rect(x - 5, ground - h, 10, h)
    pm = p.fill(post, wood, "base")
    p.tone(p.where(pm, lambda xx, yy: xx > x + 1.5), wood, "shadow")
    p.edge(post, wood, 1.1)
    p.fill(Path.polygon([(x - 7, ground - h), (x, ground - h - 8), (x + 7, ground - h)]), wood, "light")
    for i, (side, c) in enumerate(arms):
        y = ground - h + 14 + i * 30
        tip = x + side * 72
        board = Path.polygon([(x - side * 10, y), (tip - side * 12, y), (tip, y + 10), (tip - side * 12, y + 20), (x - side * 10, y + 20)])
        bt = tones("wood.light")
        bm = p.fill(board, bt, "base")
        p.tone(bm - bm.shift(0, 2), bt, "light")
        p.tone(p.below(bm, y + 15), bt, "shadow")
        for k in range(3):
            dx = x + side * (4 + k * 15)
            p.stroke(Path.polyline([(dx, y + 10), (dx + side * 9, y + 10)]), 1.6, bt, "deep")
        p.edge(board, bt, 1.1)
        p.fill(Path.circle(x, y + 10, 2), tones("iron"), "base")
    p.ground_shadow(x - 12, x + 12, ground + 1, 5, 0.3)
    layer.place(p)
    return p


def windmill(layer, x, ground, h=330, seed=0, angle=18, cap="roof.blue", wall_colour="plaster"):
    """A tower windmill: a tapering whitewashed tower on a stone foot, a door and little windows, a
    tiled cap with a finial, and four lattice sails on a hub, the sails turned `angle` degrees."""
    bw, tw = h * 0.34, h * 0.24
    sail = h * 0.78
    top = ground - h
    p = Piece(x - sail - 20, top - h * 0.36 - sail * 0.2, x + sail + 20, ground + 12)
    wt = tones(wall_colour)
    body = Path.polygon([(x - bw / 2, ground), (x - tw / 2, top), (x + tw / 2, top), (x + bw / 2, ground)])
    m = p.fill(body, wt, "base")
    p.tone(p.where(m, lambda xx, yy: xx > x + (bw / 2) * 0.28 * (1 - (ground - yy) / h) + (tw / 2) * 0.28 * ((ground - yy) / h)), wt, "shadow")
    p.tone(p.where(m, lambda xx, yy: xx < x - (bw / 2) * 0.62 * (1 - (ground - yy) / h) - (tw / 2) * 0.62 * ((ground - yy) / h)), wt, "light")
    # courses of whitewashed stone, faint
    for i in range(1, int(h / 30)):
        yy = ground - i * 30
        u = (ground - yy) / h
        half = bw / 2 * (1 - u) + tw / 2 * u
        p.stroke(Path.polyline([(x - half, yy), (x + half, yy)]), 0.9, wt, "terminator", clip=m, alpha=0.6)
    # a stone foot
    st = tones("stone.grey")
    foot = Path.polygon([(x - bw / 2 - 6, ground + 1), (x - bw / 2 - 2, ground - 26), (x + bw / 2 + 2, ground - 26), (x + bw / 2 + 6, ground + 1)])
    fm = p.fill(foot, st, "base")
    p.tone(p.where(fm, lambda xx, yy: xx > x + bw * 0.2), st, "shadow")
    for k in range(int(bw / 22) + 1):
        bx = x - bw / 2 + 8 + k * 22
        p.stroke(Path.polyline([(bx, ground - 25), (bx, ground)]), 1.0, st, "shadow", clip=fm)
    p.stroke(Path.polyline([(x - bw / 2, ground - 13), (x + bw / 2, ground - 13)]), 1.0, st, "shadow", clip=fm)
    p.edge(foot, st, 1.2, "shadow")
    # the door and the windows
    dw, dh = bw * 0.28, h * 0.2
    P.door(p, Door(x - bw * 0.06, dw, dh, "arch", wood="wood.blue", surround="stone", step=True, studs=False), ground, wt, seed)
    for k, (wy, wx) in enumerate(((0.45, -0.05), (0.7, 0.02))):
        yy = ground - h * wy
        P.window(p, Window(x + wx * bw, yy, 18, 26, "arch", lit=False, shutters=None, sill=True, panes=2), wt, seed + k)
    p.edge(body, wt, 1.4)
    # a little balcony rail round the cap's foot
    rail_y = top + 4
    wood = tones("wood.dark")
    p.fill(Path.rect(x - tw / 2 - 16, rail_y - 2, tw + 32, 6, r=1.5), wood, "base")
    for k in range(7):
        bx = x - tw / 2 - 12 + k * (tw + 24) / 6
        p.fill(Path.rect(bx - 1.2, rail_y - 16, 2.4, 14), wood, "base")
    p.fill(Path.rect(x - tw / 2 - 16, rail_y - 18, tw + 32, 4, r=1.5), wood, "light")
    # the cap: an onion-ish dome of tiles
    ct = tones(cap)
    cw, ch = tw * 1.45, h * 0.3
    capp = Path().move(x - cw / 2, top + 2).quad(x - cw * 0.6, top - ch * 0.55, x - cw * 0.12, top - ch * 0.86) \
        .quad(x, top - ch * 1.02, x + cw * 0.12, top - ch * 0.86).quad(x + cw * 0.6, top - ch * 0.55, x + cw / 2, top + 2).close()
    cm = p.fill(capp, ct, "base")
    P._roof_tiles(p, cm, ct, x - cw / 2, x + cw / 2, top - ch, top - 2, seed, row=11, tile=13, curve=5, cx=x)
    p.tone(p.where(cm, lambda xx, yy: xx > x + cw * 0.1), ct, "shadow", alpha=0.9)
    p.tone(p.where(cm, lambda xx, yy: xx < x - cw * 0.26), ct, "light", alpha=0.6)
    p.stroke(Path.polyline([(x - cw / 2, top + 1), (x + cw / 2, top + 1)]), 2.2, ct, "deep")
    p.edge(capp, ct, 1.3)
    P._finial(p, x, top - ch * 0.98, tones("gold"), tall=True)
    # the sails, on a hub at the cap's front
    hx, hy = x + cw * 0.08, top - ch * 0.42
    frame = tones("wood.light")
    cloth = tones("canvas")
    for k in range(4):
        a = math.radians(angle + k * 90)
        ux, uy = math.cos(a), math.sin(a)
        vx, vy = -uy, ux
        r0, r1 = 18, sail
        sw = sail * 0.17
        # the lattice on the trailing side of the spar
        q = [(hx + ux * r0 * 1.6, hy + uy * r0 * 1.6), (hx + ux * r1, hy + uy * r1),
             (hx + ux * r1 + vx * sw, hy + uy * r1 + vy * sw), (hx + ux * r0 * 1.6 + vx * sw * 0.8, hy + uy * r0 * 1.6 + vy * sw * 0.8)]
        lm = p.fill(Path.polygon(q), cloth, "base", alpha=1.0)
        p.tone(lm, cloth, "shadow" if k % 2 else "base-light")
        lat = Path()
        for j in range(1, 9):
            u = r0 * 1.6 + (r1 - r0 * 1.6) * j / 9
            lat.add(Path.polyline([(hx + ux * u, hy + uy * u), (hx + ux * u + vx * sw, hy + uy * u + vy * sw)]))
        lat.add(Path.polyline([(hx + ux * r0 * 1.6 + vx * sw * 0.5, hy + uy * r0 * 1.6 + vy * sw * 0.5),
                               (hx + ux * r1 + vx * sw * 0.5, hy + uy * r1 + vy * sw * 0.5)]))
        p.stroke(lat, 1.6, frame, "shadow", clip=lm)
        p.edge(Path.polygon(q), frame, 1.6, "base")
        spar = Path.polygon([(hx - vx * 3, hy - vy * 3), (hx + ux * (r1 + 6) - vx * 2.4, hy + uy * (r1 + 6) - vy * 2.4),
                             (hx + ux * (r1 + 6) + vx * 2.4, hy + uy * (r1 + 6) + vy * 2.4), (hx + vx * 3, hy + vy * 3)])
        sm = p.fill(spar, frame, "base")
        p.edge(spar, frame, 1.0)
        del sm
    hub = Path.circle(hx, hy, 11)
    hm = p.fill(hub, wood, "base")
    p.tone(hm - hm.shift(1.6, 1.6), wood, "light")
    p.fill(Path.circle(hx, hy, 4), tones("iron"), "base")
    p.edge(hub, wood, 1.2)
    p.ground_shadow(x - bw * 0.7, x + bw * 0.7, ground + 2, 9, 0.35)
    layer.place(p)
    return p


# --------------------------------------------------------------------------- #
# The far layer: patchwork hills
# --------------------------------------------------------------------------- #

FIELD_COLOURS = ("#79b85a", "#9ccb5e", "#e3c35e", "#5ea256", "#b9cf6a", "#c9a46a")


def field_hills(layer, points, haze_c, k=0.4, seed=0, colours=FIELD_COLOURS, base="hill", hedges=True, trees=6, rows=3, farm=None):
    """A rolling hill (a smooth line through `points`, down to the layer's foot) laid out in a
    patchwork of fields, green, gold and ploughed, with dark hedgerow lines between them and round
    trees along the hedges, hazed a share k toward haze_c; a farmhouse at x `farm` if asked."""
    t = haze(tones(base), haze_c, k)
    top = min(q[1] for q in points)
    pc = Piece(points[0][0] - 20, top - 50, points[-1][0] + 20, layer.h, ss=2, pad=4)
    line = Path.smooth(points)
    body = Path.smooth(points)
    body.line(points[-1][0] + 20, layer.h + 4).line(points[0][0] - 20, layer.h + 4).close()
    m = pc.fill(body, t, "base")
    hedge_t = haze(tones("leaf.dark"), haze_c, k)
    # fields: slanting bands across, broken into rows
    xs = []
    x = points[0][0] - 60
    i = 0
    while x < points[-1][0] + 60:
        xs.append(x)
        x += 90 + 120 * hash01(i, 1, seed)
        i += 1
    bottom = layer.h
    depth = bottom - top
    row_ys = [top + depth * (0.05 + 0.3 * (j + 1) / rows) for j in range(rows)]
    hedge_lines = Path()
    for j in range(rows + 1):
        y0 = top - 40 if j == 0 else row_ys[j - 1]
        y1 = row_ys[j] if j < rows else bottom + 10
        for i in range(len(xs) - 1):
            sl = (xs[i] - (points[0][0] + points[-1][0]) / 2) * 0.12
            a, b = xs[i] + (hash01(i, j, seed + 3) - 0.5) * 30, xs[i + 1]
            poly = Path.polygon([(a, y0), (b, y0), (b + sl * (y1 - y0) / 100, y1), (a + sl * (y1 - y0) / 100, y1)])
            c = colours[int(hash01(i, j + 7, seed + 5) * len(colours))]
            ft = haze(tones(c), haze_c, k)
            fm = pc.shape(poly) & m
            pc.tone(fm, ft, "base")
            # furrows in the gold and ploughed fields
            if c in ("#e3c35e", "#c9a46a"):
                fur = Path()
                for q in range(int((b - a) / 9)):
                    fx = a + 4 + q * 9
                    fur.add(Path.polyline([(fx, y0), (fx + sl * (y1 - y0) / 100, y1)]))
                pc.stroke(fur, 1.0, ft, "shadow", clip=fm, alpha=0.6)
            if hedges:
                hedge_lines.add(Path.polyline([(a, y0), (a + sl * (y1 - y0) / 100, y1)]))
        if hedges and j > 0:
            hedge_lines.add(Path.polyline([(points[0][0] - 20, y0), (points[-1][0] + 20, y0)]))
    if hedges:
        pc.stroke(hedge_lines, 2.6, hedge_t, "base", clip=m)
    pc.tone(m - m.shift(-4, 8), t, "light", alpha=0.8)
    for i in range(trees):
        u = (i + 0.5 + (hash01(i, 1, seed + 611) - 0.5) * 0.6) / trees
        (px, py), _ = line.at(u)
        P._round_tree_small(pc, px, py + 4, 8 + 7 * hash01(i, 2, seed + 611), hedge_t, seed + i)
    if farm is not None:
        fy = _line_y([(q[0], q[1]) for q in points], farm) + 22
        wall = haze(tones("plaster"), haze_c, k)
        rf = haze(tones("roof.red"), haze_c, k)
        pc.fill(Path.rect(farm - 18, fy - 20, 36, 22), wall, "base")
        pc.fill(Path.rect(farm + 8, fy - 20, 10, 22), wall, "shadow")
        pc.fill(Path.polygon([(farm - 24, fy - 18), (farm - 4, fy - 36), (farm + 14, fy - 36), (farm + 24, fy - 18)]), rf, "base")
        pc.fill(Path.rect(farm - 4, fy - 10, 6, 10), wall, "deep")
    layer.place(pc, ink=P._far_line(t, haze_c, k), reach=0.8)
    return pc


# --------------------------------------------------------------------------- #
# Trees: big old ones with roots, firs, the black-veined Mirewood
# --------------------------------------------------------------------------- #

TREE_KINDS = {
    # crown colour, the crown's back colour, bark
    "oak": ("leaf", "leaf.dark", "bark"),
    "lush": ("#62b252", "#3f8c56", "bark"),
    "old": ("#4f9156", "#2f6b52", "#6f5244"),
    "turning": ("#8aa152", "#5d7a4f", "#6f5244"),
    "blackvein": ("#6f7f62", "#4b5354", "#4a3a44"),
    "blossom": ("blossom", "#e07fae", "bark"),
    "bare": (None, None, "#5a4848"),
    "dusk": ("#4f9662", "#2f6852", "#5f4a48"),
    "night": ("#3f7466", "#274c4a", "#4f4048"),
}

VEIN = "#a46cf0"
"""The glow in the Mirewood's black veins."""


def big_tree(layer, x, ground, height=420, kind="oak", seed=0, crown_w=None, roots=True, moss=0.0, veins=0.0, place=True,
             crown_top=None, lean=0.0, trunk_w=None):
    """A big old tree standing at (x, ground): a thick trunk flaring into roots that grip the
    ground, bark in long split plates, two or three boughs forking into a broad crown of round
    clumps (oak, lush, old, turning, blackvein, blossom) or bare branches (bare). `moss` (0..1)
    greens the trunk's lit side and the roots; `veins` (0..1) splits the bark with black veins
    glowing violet, the Mirewood's sickness. Returns the Piece."""
    leaf_c, back_c, bark_c = TREE_KINDS[kind]
    h = height
    cw = crown_w or h * 0.95
    tw = trunk_w or (h * 0.075 + 12)
    trunk_h = h * 0.46
    top = crown_top if crown_top is not None else ground - h
    p = Piece(x - cw * 0.62 - 40, top - 70, x + cw * 0.62 + 40, ground + 16)
    bark = tones(bark_c)
    # roots: flaring buttresses, either side, gripping the ground
    shape = Path().move(x - tw * 0.55 + lean * 0.3, ground - trunk_h)
    shape.quad(x - tw * 0.6, ground - trunk_h * 0.35, x - tw * 1.0, ground - tw * 0.6)
    if roots:
        shape.quad(x - tw * 1.5, ground - 4, x - tw * 2.4, ground + 2)
        shape.line(x - tw * 1.2, ground + 4)
        shape.quad(x - tw * 0.8, ground - tw * 0.3, x - tw * 0.4, ground + 4)
        shape.line(x + tw * 0.3, ground + 5)
        shape.quad(x + tw * 0.8, ground - tw * 0.3, x + tw * 1.3, ground + 4)
        shape.line(x + tw * 2.5, ground + 3)
        shape.quad(x + tw * 1.6, ground - 4, x + tw * 1.0, ground - tw * 0.6)
    else:
        shape.line(x - tw * 1.1, ground + 2).line(x + tw * 1.1, ground + 2).line(x + tw * 1.0, ground - tw * 0.6)
    shape.quad(x + tw * 0.6, ground - trunk_h * 0.35, x + tw * 0.55 + lean * 0.3, ground - trunk_h).close()
    tm = p.fill(shape, bark, "base")
    p.tone(p.where(tm, lambda xx, yy: xx > x + tw * 0.15 + lean * 0.3 * (ground - yy) / trunk_h), bark, "shadow")
    p.tone(p.where(tm, lambda xx, yy: xx < x - tw * 0.45 + lean * 0.3 * (ground - yy) / trunk_h), bark, "light")
    p.tone(p.below(tm, ground - tw * 0.35), bark, "deep-shadow", alpha=0.55)
    # the bark's long plates
    plates = Path()
    for k in range(int(tw / 6) + 2):
        bx = x - tw * 0.5 + k * tw / (int(tw / 6) + 1) + (hash01(k, 1, seed) - 0.5) * 4
        y0 = ground - trunk_h * (0.95 - 0.25 * hash01(k, 2, seed))
        y1 = ground - tw * 0.5 * hash01(k, 3, seed)
        plates.add(Path.smooth([(bx, y0), (bx + (hash01(k, 4, seed) - 0.5) * 6, (y0 + y1) / 2), (bx + (bx - x) * 0.4, y1)]))
    p.stroke(plates, (1.4, 0.4), bark, "deep", clip=tm)
    # a knot hole
    if hash01(1, 9, seed) < 0.6:
        kx, ky = x + tw * 0.1, ground - trunk_h * 0.55
        km = p.fill(Path.ellipse(kx, ky, tw * 0.16, tw * 0.24), bark, "deep")
        p.edge(Path.ellipse(kx, ky, tw * 0.16 + 2.5, tw * 0.24 + 2.5), bark, 2.0, "shadow")
        del km
    if moss > 0:
        mt = tones("#7fae54")
        mm = p.where(tm, lambda xx, yy: (xx < x - tw * 0.1) & ((yy > ground - trunk_h * 0.9 * moss)))
        blob = p.nothing()
        for k in range(int(10 * moss) + 3):
            bx = x - tw * (0.2 + 0.9 * hash01(k, 5, seed)) - tw * 0.4 * (1 if k % 3 == 0 else 0)
            by = ground - trunk_h * 0.8 * moss * hash01(k, 6, seed)
            blob = blob | p.shape(Path.ellipse(bx, by, tw * 0.28 + 4, tw * 0.18 + 3))
        mm = (mm & blob) | (p.below(tm, ground - tw * 0.55) & blob.shift(0, 0))
        p.tone(mm & tm, mt, "base")
        p.tone((mm & tm) - (mm & tm).shift(-1.5, -2), mt, "shadow", alpha=0.7)
    if veins > 0:
        vt = tones("#2a1e30")
        glow = tones(VEIN)
        vv = Path()
        for k in range(int(3 + 6 * veins)):
            bx = x - tw * 0.8 + 1.6 * tw * hash01(k, 7, seed)
            y = ground - 4
            pts = [(bx * 0.6 + x * 0.4 + (bx - x) * 0.8, y)]
            for j in range(5):
                y -= trunk_h * (0.12 + 0.12 * hash01(k, j + 8, seed))
                pts.append((x + (bx - x) * (0.5 - 0.06 * j) + (hash01(k, j + 20, seed) - 0.5) * tw * 0.4, y))
            vv.add(Path.polyline(pts))
        vm = p.line_shape(vv, (3.2, 1.0)) & tm
        p.tone(vm, vt, "base")
        core = p.line_shape(vv, (1.2, 0.3)) & tm
        p.tone(core, glow, "light")
        for k in range(int(2 + 3 * veins)):
            gx = x - tw * 0.6 + 1.2 * tw * hash01(k, 30, seed)
            p.glow(gx, ground - trunk_h * (0.2 + 0.5 * hash01(k, 31, seed)), 26, VEIN, 0.16)
    p.edge(shape, bark, 1.4)
    # the boughs
    boughs = []
    n = 3 if kind != "bare" else 5
    for k in range(n):
        side = -1 if k % 2 == 0 else 1
        if k == n - 1 and n % 2:
            side = 0.25
        ang = side * (28 + 26 * hash01(k, 1, seed + 3))
        ln = (h - trunk_h) * (0.55 + 0.35 * hash01(k, 2, seed + 3))
        sx, sy = x + lean * 0.3, ground - trunk_h * (0.95 + 0.05 * k)
        mx = sx + math.sin(math.radians(ang)) * ln * 0.5
        my = sy - math.cos(math.radians(ang)) * ln * 0.55
        ex = sx + math.sin(math.radians(ang * 1.25)) * ln
        ey = sy - math.cos(math.radians(ang * 1.25)) * ln
        boughs.append(((sx, sy), (mx, my), (ex, ey)))
        bp = Path.smooth([(sx, sy + 6), (mx, my), (ex, ey)])
        bm = p.line_shape(bp, (tw * 0.62, tw * 0.16))
        p.tone(bm, bark, "base")
        p.tone(bm - bm.shift(2.0, 2.0), bark, "light")
        p.tone(bm - bm.shift(-2.4, -1.6), bark, "shadow")
        if kind == "bare" or kind == "blackvein":
            # twigs off the bough
            for j in range(3):
                u = 0.45 + 0.17 * j
                (tx_, ty_), (dx, dy) = bp.at(u)
                tside = 1 if (j + k) % 2 else -1
                tl = ln * (0.22 + 0.12 * hash01(j, k, seed + 5))
                twig = Path.smooth([(tx_, ty_), (tx_ + dx * tl * 0.4 - dy * tside * tl * 0.5, ty_ + dy * tl * 0.4 + dx * tside * tl * 0.5),
                                    (tx_ + dx * tl * 0.7 - dy * tside * tl * 0.9, ty_ + dy * tl * 0.7 + dx * tside * tl * 0.9 - tl * 0.2)])
                p.tone(p.line_shape(twig, (tw * 0.2, 1.0)), bark, "base")
    if leaf_c is None:
        p.edge_mask(p.covered(), bark, 1.0, "deep")
    else:
        t = tones(leaf_c)
        tb = tones(back_c)
        cx = x + lean * 0.4
        cy = top + (ground - trunk_h - top) * 0.5 + h * 0.04
        rx, ry = cw * 0.5, (ground - trunk_h - top) * 0.52 + h * 0.06
        clumps = []
        cn = 18 if kind != "blackvein" else 13
        for i in range(cn):
            a = 2 * math.pi * i / cn + hash01(i, 1, seed + 7) * 0.5
            d = 0.6 + 0.3 * hash01(i, 2, seed + 7)
            px_, py_ = cx + math.cos(a) * rx * d, cy + math.sin(a) * ry * d * 0.86
            r = min(rx, ry) * (0.3 + 0.14 * hash01(i, 3, seed + 7))
            clumps.append((px_, py_, r, 1))
        clumps.sort(key=lambda c: c[1])
        inner = []
        for i in range(8):
            a = 2 * math.pi * i / 8 + 0.3
            inner.append((cx + math.cos(a) * rx * 0.32, cy + math.sin(a) * ry * 0.3 - ry * 0.06, min(rx, ry) * 0.4, 0))
        inner.sort(key=lambda c: c[1])
        # bough ends poke into the lower clumps; the crown over them
        whole = P._clumps(p, [c for c in clumps if c[1] < cy], t, tb)
        whole = whole | P._clumps(p, inner, t)
        whole = whole | P._clumps(p, [c for c in clumps if c[1] >= cy], t)
        if kind == "blossom":
            for i in range(40):
                a = hash01(i, 4, seed + 775) * 6.283
                d = hash01(i, 5, seed + 775) ** 0.5
                fx, fy = cx + math.cos(a) * rx * d * 0.95, cy + math.sin(a) * ry * d * 0.9
                fc = tones("blossom.white")
                fm = p.fill(Path.circle(fx, fy, 2.6), fc, "base", clip=whole)
                p.tone(fm - fm.shift(-0.6, -0.6), fc, "shadow")
        if kind == "blackvein":
            # dead patches in the crown, and veins of violet light through it
            sick = tones("#7a6a86")
            for i in range(4):
                a = hash01(i, 6, seed) * 6.283
                bx_, by_ = cx + math.cos(a) * rx * 0.55, cy + math.sin(a) * ry * 0.5
                dm = p.shape(Path.circle(bx_, by_, min(rx, ry) * 0.2)) & whole
                p.tone(dm, sick, "base", alpha=0.85)
                p.tone(dm - p.shape(Path.circle(bx_ - 4, by_ - 6, min(rx, ry) * 0.2)), sick, "shadow", alpha=0.85)
                p.tone(dm & p.shape(Path.circle(bx_ - 6, by_ - 8, min(rx, ry) * 0.09)), sick, "light", alpha=0.7)
        if kind == "turning":
            for i in range(14):
                a = hash01(i, 8, seed) * 6.283
                d = hash01(i, 9, seed) ** 0.5
                fx, fy = cx + math.cos(a) * rx * d * 0.85, cy + math.sin(a) * ry * d * 0.8
                lc = tones(pick(("#e09a3c", "#d86c3a", "#c9a43c"), i, 10, seed))
                lm = p.fill(Path.circle(fx, fy, 5 + 4 * hash01(i, 11, seed)), lc, "base", clip=whole)
                p.tone(lm - lm.shift(-1, -1.2), lc, "shadow")
    p.ground_shadow(x - cw * 0.4, x + cw * 0.4, ground + 3, 12, 0.34)
    if place:
        layer.place(p)
    return p


def fir(layer, x, ground, h=300, colour="#3f8466", seed=0, place=True, dead=0.0):
    """A fir: a straight trunk under tiers of drooping boughs, each tier a scalloped skirt lit on
    its upper left, narrowing to a spike."""
    w = h * 0.42
    p = Piece(x - w * 0.6 - 10, ground - h - 12, x + w * 0.6 + 10, ground + 12)
    bark = tones("bark")
    trunk = Path.rect(x - h * 0.025 - 3, ground - h * 0.3, h * 0.05 + 6, h * 0.3 + 2)
    tm = p.fill(trunk, bark, "base")
    p.tone(p.where(tm, lambda xx, yy: xx > x + 1), bark, "shadow")
    p.edge(trunk, bark, 1.1)
    t = tones(colour)
    tiers = 6
    whole = p.nothing()
    for i in range(tiers):
        u = i / (tiers - 1)
        ty = ground - h * 0.18 - (h * 0.82) * u * 0.92
        tw_ = w * 0.5 * (1.0 - u * 0.82) + 6
        th = h * 0.2 * (1.0 - u * 0.4)
        sk = Path().move(x, ty - th)
        sk.quad(x - tw_ * 0.4, ty - th * 0.5, x - tw_, ty + 2)
        n = max(3, int(tw_ / 12))
        for k in range(n * 2):
            sx = x - tw_ + (2 * tw_) * (k + 1) / (n * 2)
            sk.line(sx, ty + (6 if k % 2 == 0 else 0) + 3 * hash01(i, k, seed))
        sk.quad(x + tw_ * 0.4, ty - th * 0.5, x, ty - th).close()
        m = p.fill(sk, t, "base")
        p.tone(p.where(m, lambda xx, yy, a=x: xx > a + 2), t, "shadow")
        p.tone(m - m.shift(3, 4), t, "light", alpha=0.85)
        p.tone(p.below(m, ty - 2), t, "deep-shadow", alpha=0.6)
        p.edge(sk, t, 1.1)
        whole = whole | m
    p.fill(Path.polygon([(x - 4, ground - h * 0.92), (x, ground - h - 4), (x + 4, ground - h * 0.92)]), t, "base")
    p.ground_shadow(x - w * 0.4, x + w * 0.4, ground + 2, 8, 0.3)
    if place:
        layer.place(p)
    return p


def boulders(layer, x, ground, w=90, h=50, colour="stone.grey", seed=0, moss=0.0, place=True, piece=None):
    """A heap of two or three rounded boulders, faceted and lit top left, moss on their tops if asked."""
    p = piece or Piece(x - w * 0.7, ground - h - 10, x + w * 0.7, ground + 10)
    t = tones(colour)
    n = 2 + int(hash01(1, 1, seed) * 2)
    for i in range(n):
        u = (i + 0.5) / n
        bw = w * (0.45 + 0.25 * hash01(i, 2, seed))
        bh = h * (0.6 + 0.4 * hash01(i, 3, seed)) * (1.0 if i % 2 == 0 else 0.75)
        bx = x - w / 2 + w * u
        pts = []
        for k in range(9):
            a = math.pi + math.pi * k / 8
            rr = 0.85 + 0.15 * hash01(i * 9 + k, 4, seed)
            pts.append((bx + math.cos(a) * bw / 2 * rr, ground + math.sin(a) * bh * rr))
        rock = Path.polygon([(bx - bw / 2, ground + 2)] + pts + [(bx + bw / 2, ground + 2)])
        m = p.fill(rock, t, "base")
        p.tone(p.where(m, lambda xx, yy, a=bx: xx > a + bw * 0.12), t, "shadow")
        p.tone(m & p.shape(Path.polygon([(bx - bw * 0.42, ground - bh * 0.3), (bx - bw * 0.15, ground - bh * 0.92), (bx + bw * 0.05, ground - bh * 0.7),
                                         (bx - bw * 0.25, ground - bh * 0.2)])), t, "light")
        p.stroke(Path.polyline([(bx + bw * 0.12, ground - bh * 0.9), (bx + bw * 0.18, ground - bh * 0.4), (bx + bw * 0.1, ground)]), 1.0, t, "deep", clip=m)
        if moss > 0:
            mt = tones("#78a855")
            mm = m - m.shift(0, bh * 0.25 * moss + 3)
            p.tone(mm, mt, "base")
            p.tone(mm - mm.shift(-1, -1.5), mt, "shadow", alpha=0.6)
        p.edge(rock, t, 1.2)
    p.ground_shadow(x - w * 0.6, x + w * 0.6, ground + 2, 7, 0.32)
    if piece is None and place:
        layer.place(p)
    return p


def mushrooms(p, x, ground, n=3, colour="#e2534f", seed=0, glow=None, size=1.0):
    """A little cluster of toadstools on a Piece: caps lit top left with pale spots, stalks under."""
    for i in range(n):
        mx = x + (i - (n - 1) / 2) * 11 * size + (hash01(i, 1, seed) - 0.5) * 5
        mh = (12 + 10 * hash01(i, 2, seed)) * size
        cr = (6 + 4 * hash01(i, 3, seed)) * size
        st = tones("#efe4cb")
        stalk = Path.rect(mx - 2.2 * size, ground - mh, 4.4 * size, mh + 1)
        p.fill(stalk, st, "base")
        p.edge(stalk, st, 0.9)
        ct = tones(colour)
        cap = Path().move(mx - cr, ground - mh + 2).quad(mx - cr, ground - mh - cr * 1.1, mx, ground - mh - cr * 1.05) \
            .quad(mx + cr, ground - mh - cr * 1.1, mx + cr, ground - mh + 2).close()
        cm = p.fill(cap, ct, "base")
        p.tone(p.where(cm, lambda xx, yy, a=mx: xx > a + cr * 0.3), ct, "shadow")
        if glow is None:
            for k in range(3):
                p.fill(Path.circle(mx - cr * 0.4 + k * cr * 0.4, ground - mh - cr * (0.45 + 0.2 * (k % 2)), 1.3 * size), tones("#fcf5ec"), "base", clip=cm)
        p.edge(cap, ct, 1.0)
        if glow:
            p.glow(mx, ground - mh - cr * 0.4, 30 * size, glow, 0.3)


def mist(layer, x0, x1, y, depth=60, strength=0.35, colour="#d9d2f0", seed=0, blobs=14):
    """Mist lying along the ground: soft long drifts of pale violet laid over what's there (the
    one soft thing in the mid layer, as the haze is in the distance)."""
    w = x1 - x0
    for i in range(blobs):
        u = (i + hash01(i, 1, seed)) / blobs
        cx = x0 + w * u
        rx = w / blobs * (0.9 + 0.8 * hash01(i, 2, seed))
        ry = depth * (0.35 + 0.35 * hash01(i, 3, seed))
        cy = y - ry * 0.2 + (hash01(i, 4, seed) - 0.5) * depth * 0.4
        layer.soft_ellipse(cx, cy, rx, ry, colour, strength * (0.6 + 0.4 * hash01(i, 5, seed)), hard=0.15)


# --------------------------------------------------------------------------- #
# The foreground
# --------------------------------------------------------------------------- #

def fore_tree(layer, x, kind="oak", dark=0.55, seed=0, w=480, trunk=60, bottom=CANVAS_H + 40, crown_y=30, lean=0.0):
    """A tree right in front of the camera: its trunk, flaring at the foot below the frame and
    tapering as it rises, a bough out to each side, and a crown of dark clumps hanging in from the
    top of the picture, crisp and dark (12-rooms §3.1). Returns the Piece."""
    leaf_c, back_c, bark_c = TREE_KINDS.get(kind, TREE_KINDS["oak"])
    p = Piece(x - w / 2 - 30, -70, x + w / 2 + 30, bottom + 10)
    bark = haze(tones(bark_c), "#140e1c", dark)
    top_y = crown_y + 70
    mid_y = (bottom + top_y) / 2
    tr = Path.smooth([(x - trunk * 1.25, bottom + 4), (x - trunk * 0.62, bottom - 60), (x - trunk * 0.46 + lean * 0.5, mid_y),
                      (x - trunk * 0.32 + lean, top_y), (x + trunk * 0.32 + lean, top_y), (x + trunk * 0.5 + lean * 0.5, mid_y),
                      (x + trunk * 0.66, bottom - 60), (x + trunk * 1.3, bottom + 4)])
    tr.close()
    tm = p.fill(tr, bark, "base")
    cline = lambda yy: x + lean * (bottom - yy) / max(1.0, bottom - top_y)  # noqa: E731
    p.tone(p.where(tm, lambda xx, yy: xx > cline(yy) + trunk * 0.1), bark, "shadow")
    p.tone(p.where(tm, lambda xx, yy: xx < cline(yy) - trunk * 0.3), bark, "light")
    plates = Path()
    for k in range(6):
        u = (k + 0.5) / 6 - 0.5
        y0 = top_y + 40 + 60 * hash01(k, 1, seed)
        y1 = bottom - 20 * hash01(k, 2, seed)
        plates.add(Path.smooth([(cline(y0) + u * trunk * 0.6, y0), (cline((y0 + y1) / 2) + u * trunk * 0.8, (y0 + y1) / 2),
                                (cline(y1) + u * trunk * 1.4, y1)]))
    p.stroke(plates, (2.2, 0.6), bark, "deep", clip=tm)
    p.stroke(plates.translate(-2.4, 0), (1.0, 0.3), bark, "base-light", clip=tm, alpha=0.7)
    for k in range(2):
        ky = top_y + 160 + 180 * k + 60 * hash01(k, 9, seed)
        kx = cline(ky) + trunk * (0.1 - 0.25 * k)
        p.fill(Path.ellipse(kx, ky, trunk * 0.12, trunk * 0.2), bark, "deep", clip=tm)
        p.stroke(Path.ellipse(kx, ky, trunk * 0.12 + 3, trunk * 0.2 + 3), 1.6, bark, "light", clip=tm, alpha=0.6)
    for side in (-1, 1):
        sx, sy = cline(top_y + 30) + lean * 0.2, top_y + 40
        bp = Path.smooth([(sx, sy), (sx + side * w * 0.18, sy - 50), (sx + side * w * 0.36, sy - 90 - 20 * hash01(side + 2, 3, seed))])
        bm = p.line_shape(bp, (trunk * 0.42, trunk * 0.12))
        p.tone(bm, bark, "base")
        p.tone(bm - bm.shift(2.5, 2.5), bark, "light")
        p.tone(bm - bm.shift(-2.5, -2.5), bark, "shadow")
    if leaf_c:
        t = haze(tones(leaf_c), "#140e1c", dark)
        tb = haze(tones(back_c), "#140e1c", dark)
        clumps = []
        n = max(7, int(w / 44))
        for i in range(n):
            u = (i + 0.5) / n
            r = 44 + 30 * hash01(i, 3, seed)
            clumps.append((x - w / 2 + w * u + (hash01(i, 5, seed) - 0.5) * 20,
                           crown_y - 50 + 60 * math.sin(math.pi * u) + 34 * hash01(i, 4, seed), r, i % 2))
        for i in range(4):
            clumps.append((x - w * 0.3 + w * 0.2 * i, crown_y - 70 + 20 * hash01(i, 6, seed), 50, 1))
        clumps.sort(key=lambda c: c[1])
        whole = P._clumps(p, [c for c in clumps if c[3]], t, tb)
        whole = whole | P._clumps(p, [c for c in clumps if not c[3]], t)
        if kind == "blossom":
            fc = haze(tones("blossom.white"), "#140e1c", dark * 0.6)
            for i in range(36):
                a = hash01(i, 7, seed) * 6.283
                d = hash01(i, 8, seed) ** 0.5
                fx = x + math.cos(a) * w * 0.45 * d
                fy = crown_y - 10 + math.sin(a) * 80 * d
                fm = p.fill(Path.circle(fx, fy, 3.0), fc, "base", clip=whole)
                p.tone(fm - fm.shift(-0.7, -0.7), fc, "shadow")
    layer.place(p, reach=2.6)
    return p


def fore_grass(layer, x, ground, w=160, h=70, dark=0.5, seed=0, colour="leaf.dark", flowers=("flower.pink", "flower.white", "flower.yellow")):
    """A tuft of tall grass and leaves rising into the bottom of the picture, dark, a flower or two."""
    p = Piece(x - w / 2 - 10, ground - h - 10, x + w / 2 + 10, ground + 10)
    t = haze(tones(colour), "#140e1c", dark)
    blades = []
    for i in range(int(w / 7)):
        bx = x - w / 2 + w * hash01(i, 1, seed)
        bh = h * (0.5 + 0.5 * hash01(i, 2, seed)) * (1 - abs(bx - x) / w)
        lean = (hash01(i, 3, seed) - 0.5) * 40 + (bx - x) * 0.25
        blade = Path().move(bx - 4, ground).quad(bx - 2 + lean * 0.3, ground - bh * 0.6, bx + lean, ground - bh) \
            .quad(bx + 2 + lean * 0.3, ground - bh * 0.6, bx + 4, ground).close()
        blades.append(blade)
    for i, b in enumerate(blades):
        m = p.fill(b, t, "base" if i % 3 else "shadow")
        p.tone(m - m.shift(1.5, 0), t, "light", alpha=0.6)
    if flowers:
        for i in range(3):
            fx = x - w * 0.3 + w * 0.6 * hash01(i, 5, seed)
            fy = ground - h * (0.4 + 0.4 * hash01(i, 6, seed))
            c = haze(tones(flowers[i % len(flowers)]), "#140e1c", dark * 0.7)
            for k in range(5):
                a = math.radians(k * 72)
                p.fill(Path.circle(fx + math.cos(a) * 3.2, fy + math.sin(a) * 3.2, 2.8), c, "base")
            p.fill(Path.circle(fx, fy, 1.6), tones("flower.yellow"), "shadow")
    layer.place(p, reach=2.4)
    return p


# --------------------------------------------------------------------------- #
# Recipes
# --------------------------------------------------------------------------- #

def _meadow_sky_far(r, seed, sun_x=None, castle_x=None, woods_from=None, time="day"):
    """The Old Road's sky and far layer: a high noon sun and big clouds, lavender mountains, and
    patchwork hills of fields and hedges; Questeria's spires far off to the west if castle_x."""
    sky, far = r["sky"], r["far"]
    haze_c = P.HAZE[time]
    P.sky(sky, time, seed=seed, horizon=560, sun=(sun_x if sun_x is not None else sky.w * 0.62, 96), clouds=4, cloud_band=(80, 300))
    P.mountains(far, 452, 270, haze_c, seed=seed + 1, k=0.58, count=8)
    if castle_x is not None:
        P.skyline(far, castle_x - 260, castle_x + 260, 520, haze_c, k=0.55, seed=seed + 2, height=(40, 110), spires=0.25)
        P.castle(far, castle_x, 500, 0.5, haze_c, k=0.32, seed=seed + 3)
    W = far.w
    field_hills(far, [(-40, 500), (W * 0.2, 470), (W * 0.42, 492), (W * 0.65, 462), (W * 0.85, 486), (W + 40, 470)],
                haze_c, k=0.5, seed=seed + 4, rows=2, trees=9)
    if woods_from is not None:
        P.forest_line(far, woods_from, W + 20, 540, 70, haze_c, k=0.42, seed=seed + 5)
    field_hills(far, [(-40, 560), (W * 0.15, 540), (W * 0.38, 556), (W * 0.6, 536), (W * 0.82, 552), (W + 40, 540)],
                haze_c, k=0.32, seed=seed + 6, rows=2, trees=7, farm=W * 0.7)


def farmland(r):
    """The Old Road's farmland (12-rooms §6; 03b's meadows, bright and lush at noon): the road out
    of Questeria running east between hedgerows, the farmhouse with its washing line and blossom
    tree, gold wheat behind a rail fence, haystacks, the windmill on its rise, and at the east end
    the road turning back over a little stone bridge toward the fork."""
    g = r.ground
    seed = roomkit.seed_of("road", "farmland")
    _meadow_sky_far(r, seed, sun_x=r.x("sky", 1300), castle_x=r.x("far", -300))

    mid = r["mid"]
    W = mid.w
    bank(mid, 0, W, g, g - 52, seed=seed + 10, flowers=("flower.white", "flower.yellow", "flower.pink"))
    P.floor_shade(mid, g, 46, 0.24)

    # the west end: an oak and the signpost home
    big_tree(mid, 70, g - 4, 470, "lush", seed=seed + 20, crown_w=420)
    signpost(mid, 150, g + 2, 128, arms=((-1, "wood.light"), (1, "wood.light")), seed=seed + 21)
    P.tree(mid, 262, g - 6, "blossom", 300, seed=seed + 22)
    washing_line(mid, 330, 470, g - 6, 140, seed=seed + 23)
    hedgerow(mid, 480, 650, g - 2, 62, seed=seed + 24)

    # the farmhouse, its door on the zone file's
    fx = r.door_x("farmhouse")
    r.house(House(fx - 140, 290, g, floors=(("stone.warm", 205), ("timber", 140)), roof="hip", roof_colour="roof.red", roof_h=150,
                  jetty=8, chimneys=[(fx + 110, 26, 64)], dormers=(fx + 50,),
                  door=Door(fx, 74, 150, "arch", wood="wood.green"),
                  windows=[Window(fx - 82, None, 40, 56, "arch", lit=False, box=True, shutters="wood.green", floor=0),
                           Window(fx + 82, None, 40, 56, "arch", box=True, shutters="wood.green", floor=0),
                           Window(fx - 70, None, 36, 50, "arch", box=True, floor=1), Window(fx + 80, None, 36, 50, "arch", floor=1)],
                  vines=(fx - 128, fx + 140), seed=seed + 30))
    P.barrel(mid, fx + 176, g + 2, 36, 46, seed=seed + 31)
    P.pot(mid, fx - 166, g + 2, 26, 26, plant="flowers", seed=seed + 32)

    # the wheat behind its fence, haystacks, the windmill on its rise
    wx = 1846
    bank(mid, wx - 220, wx + 220, g, g - 96, seed=seed + 44, amp=4, step=120)
    wheat_field(mid, 990, 1640, g - 26, 62, seed=seed + 40)
    rail_fence(mid, 990, 1660, g - 4, 44, seed=seed + 41)
    haystack(mid, 1453, g - 2, 120, 104, seed=seed + 42)
    haystack(mid, 1060, g - 4, 90, 78, seed=seed + 43)
    windmill(mid, wx, g - 30, 330, seed=seed + 45, angle=14)
    P.bush(mid, wx - 150, g - 6, 110, 56, seed=seed + 46, flowers_on=("flower.yellow", "flower.white"))

    # the meadow east of the mill: an oak, flowers, the hedge
    big_tree(mid, 2140, g - 6, 420, "oak", seed=seed + 50, crown_w=380)
    hedgerow(mid, 2230, 2470, g - 2, 66, seed=seed + 51, flowers=("flower.blue", "flower.white", "flower.violet"))
    P.tree(mid, 2560, g - 8, "oak", 300, seed=seed + 52)
    hedgerow(mid, 2600, 2760, g - 2, 58, seed=seed + 53)

    # the east end: the stream under a little stone bridge, and the road turning back toward the fork
    stone_bridge(mid, 2900, g - 6, 360, 118, seed=seed + 60)
    path_back(mid, 2990, 3200, 3080, 3150, g, g - 52, seed=seed + 62)
    big_tree(mid, 3190, g - 6, 470, "lush", seed=seed + 61, crown_w=400)

    # the foreground: a blossom tree at the west end, tufts and flowers along the bottom
    fore = r["fore"]
    fore_tree(fore, r.x("fore", 40), "blossom", 0.5, seed + 70, w=520, trunk=58, lean=-14)
    fore_grass(fore, r.x("fore", 1180), CANVAS_H + 20, 180, 80, 0.5, seed + 71)
    P.fore_foliage(fore, r.x("fore", 2337), CANVAS_H + 30, 200, 84, seed + 72, 0.5)
    fore_grass(fore, r.x("fore", 3080), CANVAS_H + 20, 200, 90, 0.5, seed + 73)
    P.near_shade(fore, 820, 0.18)


def stone_bridge(layer, cx, ground, w=360, h=118, seed=0, colour="stone.warm", water=True):
    """A humpbacked bridge of stone over a stream, side on: one round arch over the water, its
    voussoirs, the soffit in shadow under it, coursed stone, a parapet with a lit coping along its
    curved back, sloping wing walls at its feet, and reeds along the banks."""
    p = Piece(cx - w / 2 - 50, ground - h - 30, cx + w / 2 + 50, ground + 10)
    st = tones(colour)
    top = [(cx - w / 2 - 34, ground + 2), (cx - w / 2 + 6, ground - h * 0.4), (cx - w * 0.24, ground - h * 0.86), (cx, ground - h),
           (cx + w * 0.24, ground - h * 0.86), (cx + w / 2 - 6, ground - h * 0.4), (cx + w / 2 + 34, ground + 2)]
    deck = Path.smooth(top)
    body = Path.smooth(top)
    body.line(cx - w / 2 - 34, ground + 2).close()
    ar = w * 0.27
    spring = ground - 10
    arch = Path().move(cx - ar, ground + 4).line(cx - ar, spring).arc(cx, spring, ar, 180, 360, ar * 0.92).line(cx + ar, ground + 4).close()
    m = P.wall(p, cx - w / 2 - 40, ground - h - 6, cx + w / 2 + 40, ground + 2, colour, seed, quoins=False)
    keep = p.shape(body) - p.shape(arch)
    p.c.erase((m - keep).a)
    bm = p.shape(body) - p.shape(arch)
    p.tone(p.where(bm, lambda xx, yy: xx > cx + w * 0.3), st, "shadow", alpha=0.5)
    if water:
        wt = tones("water")
        inside = p.shape(arch)
        p.tone(inside, wt, "base")
        p.tone(p.above(inside, spring - ar * 0.45), wt, "deep")
        far_bank = p.shape(Path.smooth([(cx - ar - 4, spring - ar * 0.42), (cx - ar * 0.3, spring - ar * 0.52), (cx + ar * 0.4, spring - ar * 0.44),
                                        (cx + ar + 4, spring - ar * 0.5), (cx + ar + 4, spring - ar * 1.2), (cx - ar - 4, spring - ar * 1.2)], closed=True)) & inside
        p.tone(far_bank, tones("#4f8f4c"), "base")
        p.tone(far_bank - far_bank.shift(0, 4), tones("#4f8f4c"), "light")
        for k in range(6):
            yy = spring - ar * 0.36 + k * (ar * 0.4 / 6) + 4
            x0 = cx - ar * 0.8 + ar * 1.2 * hash01(k, 1, seed)
            p.stroke(Path.polyline([(x0, yy), (x0 + 12 + 14 * hash01(k, 2, seed), yy)]), 1.6, wt, "light", clip=inside, alpha=0.85)
        # the soffit: the arch's underside in deep shadow along its top
        p.tone(inside - inside.shift(0, 14), tones("#4a4060"), "base", alpha=0.85)
    vou = Path().move(cx - ar - 15, ground + 4).line(cx - ar - 15, spring).arc(cx, spring, ar + 15, 180, 360, ar * 0.92 + 15) \
        .line(cx + ar + 15, ground + 4).line(cx + ar, ground + 4).line(cx + ar, spring).arc(cx, spring, ar, 360, 180, ar * 0.92).close()
    vm = p.fill(vou, st, "base-light")
    p.tone(p.where(vm, lambda xx, yy: xx > cx + ar * 0.55), st, "base")
    for k in range(1, 14):
        a = math.radians(180 + 180 * k / 14)
        p.stroke(Path.polyline([(cx + math.cos(a) * ar, spring + math.sin(a) * ar * 0.92),
                                (cx + math.cos(a) * (ar + 15), spring + math.sin(a) * (ar * 0.92 + 15))]), 1.1, st, "shadow", clip=vm)
    p.edge(vou, st, 1.1, "terminator")
    # a string course and the parapet's coping along the curved top
    sc = p.line_shape(deck.translate(0, 26), 3.2) & bm
    p.tone(sc, st, "light")
    p.tone(sc.shift(0, 3) & bm, st, "shadow")
    cop = p.line_shape(deck.translate(0, 3), 10)
    cpt = tones("stone.grey")
    p.tone(cop, cpt, "base")
    p.tone(cop - cop.shift(0, 3), cpt, "light")
    p.tone((cop.shift(0, 4) & bm) - cop, st, "shadow", alpha=0.8)
    p.edge_mask(cop, cpt, 1.0, "shadow")
    p.edge(body, st, 1.3)
    reeds = tones("#5f9a4e")
    for side in (-1, 1):
        for k in range(8):
            rx = cx + side * (ar + 6 + k * 6)
            rh = 24 + 22 * hash01(k, side + 3, seed)
            p.stroke(Path.smooth([(rx, ground + 3), (rx + side * 2, ground - rh * 0.6), (rx + side * 6, ground - rh)]), (2.6, 0.4), reeds, "base" if k % 2 else "light")
            if k % 3 == 1:
                p.fill(Path.ellipse(rx + side * 5, ground - rh + 6, 2.6, 7.5), tones("wood"), "base")
    p.ground_shadow(cx - w * 0.6, cx + w * 0.6, ground + 2, 8, 0.3)
    layer.place(p)
    return p


def path_back(layer, near0, near1, far0, far1, ground, top, colour="#d9b98a", seed=0, edge="#5d9a4a", fade=24):
    """A path or road going away from the floor into the back of the meadow: a strip narrowing from
    near0..near1 at the floor's back edge to far0..far1 at `top`, with ruts converging and a few
    stones, its sides lined in the grass's shadow; it fades into the floor's own path below."""
    p = Piece(min(near0, far0) - 10, top - 6, max(near1, far1) + 10, ground + fade, ss=2, pad=4)
    t = tones(colour)
    road = Path.polygon([(far0, top), (far1, top), (near1, ground + fade + 2), (near0, ground + fade + 2)])
    m = p.fill(road, t, "base")
    p.tone(p.above(m, top + (ground - top) * 0.3), t, "terminator", alpha=0.5)
    for u in (0.3, 0.7):
        a = (far0 + (far1 - far0) * u, top)
        b = (near0 + (near1 - near0) * u, ground + fade)
        p.stroke(Path.polyline([a, b]), (1.0, 3.0), t, "shadow", clip=m)
        p.stroke(Path.polyline([(a[0] - 1, a[1]), (b[0] - 3, b[1])]), (0.6, 1.4), t, "light", clip=m, alpha=0.7)
    for i in range(int((near1 - near0) / 30)):
        v = hash01(i, 1, seed)
        yy = top + (ground - top) * v
        xa = far0 + (near0 - far0) * v
        xb = far1 + (near1 - far1) * v
        sx = xa + (xb - xa) * hash01(i, 2, seed)
        sm = p.fill(Path.ellipse(sx, yy, 2 + 3 * v, 1.4 + 2 * v), tones("stone.grey"), "base")
        p.tone(sm - sm.shift(-0.6, -0.6), tones("stone.grey"), "shadow")
    p.edge(road, tones(edge), 2.0, "shadow")
    place_span(layer, p, reach=0, fade=(ground - 2, ground + fade))
    return p


# --------------------------------------------------------------------------- #
# The road's barrow, the Mirewood's woods and the quarry's mouth
# --------------------------------------------------------------------------- #

def barrow_mound(layer, cx, ground, w=520, h=230, seed=0):
    """The barrow seen from the road: a long grassy mound, flowers in its turf and a hawthorn on its
    back, and in its face a portal of three great stones (two uprights and a lintel carved with
    spirals) round a dark doorway, worn steps going down into it. Returns the doorway (x, y, w, h)."""
    p = Piece(cx - w / 2 - 20, ground - h - 160, cx + w / 2 + 20, ground + 12)
    turf = tones("#6aac56")
    earth = tones("#9c7a5a")
    mound = Path.smooth([(cx - w / 2, ground + 4), (cx - w * 0.38, ground - h * 0.55), (cx - w * 0.18, ground - h * 0.92), (cx + w * 0.05, ground - h),
                         (cx + w * 0.26, ground - h * 0.9), (cx + w * 0.42, ground - h * 0.5), (cx + w / 2, ground + 4)])
    mound.close()
    m = p.fill(mound, turf, "base")
    p.tone(m - p.shape(Path.ellipse(cx - w * 0.1, ground - h * 0.38, w * 0.5, h * 0.75)), turf, "shadow")
    p.tone(m - m.shift(0, 10), turf, "light")
    p.tone(p.below(m, ground - 26), turf, "terminator", alpha=0.6)
    # grass strokes and flowers in the turf
    strokes = Path()
    for i in range(70):
        gx = cx - w * 0.45 + w * 0.9 * hash01(i, 1, seed)
        gy = ground - h * 0.95 * hash01(i, 2, seed) ** 0.8
        strokes.add(Path.polyline([(gx, gy), (gx - 2, gy - 7)]))
    p.stroke(strokes, (1.6, 0.0), turf, "shadow", clip=m, alpha=0.7)
    for i in range(36):
        fx = cx - w * 0.45 + w * 0.9 * hash01(i, 3, seed)
        fy = ground - h * 0.9 * hash01(i, 4, seed) ** 0.7
        c = tones(pick(("flower.white", "flower.yellow", "flower.violet", "flower.pink"), i, 5, seed))
        fm = p.fill(Path.circle(fx, fy, 2.6), c, "base", clip=m)
        p.tone(fm - fm.shift(-0.7, -0.7), c, "shadow")
    # the hawthorn on its back
    hx, hy = cx + w * 0.16, ground - h * 0.94
    bark = tones("bark")
    p.stroke(Path.smooth([(hx, hy + 6), (hx + 6, hy - 40), (hx + 22, hy - 70)]), (9, 3), bark, "base")
    p.stroke(Path.smooth([(hx + 4, hy - 30), (hx - 20, hy - 56), (hx - 34, hy - 64)]), (5, 2), bark, "base")
    P._clumps(p, [(hx - 30, hy - 72, 24, 1), (hx + 26, hy - 84, 28, 1), (hx - 4, hy - 90, 30, 0), (hx + 40, hy - 66, 20, 0)],
              tones("leaf"), tones("leaf.dark"))
    for i in range(10):
        a = hash01(i, 6, seed) * 6.28
        p.fill(Path.circle(hx + math.cos(a) * 30, hy - 80 + math.sin(a) * 20, 2.6), tones("blossom.white"), "base")
    # the portal: dark doorway, steps down, uprights and the carved lintel
    dw, dh = 112, 166
    dx0, dy0 = cx - dw / 2, ground - dh
    dark = tones("#3a2e40")
    door = Path().move(dx0, ground + 2).line(dx0, dy0 + 10).quad(cx, dy0 - 6, dx0 + dw, dy0 + 10).line(dx0 + dw, ground + 2).close()
    dm = p.fill(door, dark, "base")
    p.tone(dm - dm.shift(6, 8), dark, "deep")
    for k in range(4):
        sy = ground - 4 - k * 11
        sw = dw * (0.92 - 0.12 * k)
        step = Path.rect(cx - sw / 2, sy - 7, sw, 8, r=1.5)
        st_ = haze(tones("stone.grey"), "#2a2238", 0.25 + 0.17 * k)
        sm = p.fill(step, st_, "base", clip=dm)
        p.tone(sm - sm.shift(0, 2), st_, "light", clip=dm)
    st = tones("#b9b2a8")
    for side in (-1, 1):
        ux = cx + side * (dw / 2 + 20)
        up = Path.smooth([(ux - 24, ground + 4), (ux - 26, ground - dh * 0.5), (ux - 22, ground - dh - 8), (ux + 20, ground - dh - 12),
                          (ux + 26, ground - dh * 0.55), (ux + 23, ground + 4)], closed=True)
        um = p.fill(up, st, "base")
        p.tone(p.where(um, lambda xx, yy, a=ux: xx > a + 6), st, "shadow")
        p.tone(p.where(um, lambda xx, yy, a=ux: xx < a - 14), st, "light")
        for k in range(3):
            p.stroke(Path.polyline([(ux - 10 + k * 9, ground - dh * (0.2 + 0.25 * k)), (ux - 4 + k * 9, ground - dh * (0.35 + 0.25 * k))]), (1.2, 0.0), st, "deep", clip=um)
        moss = p.shape(Path.ellipse(ux - 8, ground - dh * 0.9, 18, 10)) & um
        p.tone(moss, tones("#7fae54"), "base")
        p.edge(up, st, 1.3)
    lin = Path.smooth([(cx - dw / 2 - 52, ground - dh - 4), (cx - dw / 2 - 50, ground - dh - 38), (cx, ground - dh - 46),
                       (cx + dw / 2 + 52, ground - dh - 40), (cx + dw / 2 + 54, ground - dh - 2), (cx, ground - dh + 4)], closed=True)
    lm = p.fill(lin, st, "base")
    p.tone(p.below(lm, ground - dh - 12), st, "shadow")
    p.tone(lm - lm.shift(0, 4), st, "light")
    for k in range(3):
        sx = cx - 44 + k * 44
        sp = Path()
        for j in range(18):
            a = j * 0.6
            rr = 1.5 + j * 0.62
            sp.line(sx + math.cos(a) * rr, ground - dh - 22 + math.sin(a) * rr * 0.8) if j else sp.move(sx + rr, ground - dh - 22)
        p.stroke(sp, 1.6, st, "deep", clip=lm)
    p.cast(lm, 2, 6, dm, dark, "deep")
    p.edge(lin, st, 1.3)
    # kerb stones along the mound's foot
    for k in range(9):
        if abs(k - 4) < 1.5:
            continue
        kx = cx - w * 0.44 + k * w * 0.11
        kb = Path.smooth([(kx - 18, ground + 3), (kx - 16, ground - 22), (kx + 2, ground - 30), (kx + 18, ground - 20), (kx + 18, ground + 3)], closed=True)
        km = p.fill(kb, st, "base")
        p.tone(p.where(km, lambda xx, yy, a=kx: xx > a + 4), st, "shadow")
        p.edge(kb, st, 1.1)
    p.ground_shadow(cx - w * 0.5, cx + w * 0.5, ground + 2, 10, 0.3)
    layer.place(p)
    return (dx0, dy0, dw, dh)


def snag(layer, x, ground, h=240, seed=0, veins=0.0, colour="#5a4848", moss=0.3):
    """A dead tree's broken trunk: a tapering stump snapped off in jagged splinters, a stub of a
    bough, bark split in plates, moss at its foot, and black veins if the Mirewood has it."""
    tw = h * 0.12 + 10
    p = Piece(x - tw * 3, ground - h - 20, x + tw * 3, ground + 14)
    bark = tones(colour)
    top = ground - h
    shape = Path().move(x - tw * 2.0, ground + 4).quad(x - tw * 0.9, ground - 10, x - tw * 0.7, ground - h * 0.3) \
        .line(x - tw * 0.55, top + 20).line(x - tw * 0.35, top - 6).line(x - tw * 0.1, top + 14).line(x + tw * 0.1, top - 16) \
        .line(x + tw * 0.3, top + 10).line(x + tw * 0.5, top + 2).line(x + tw * 0.58, ground - h * 0.3) \
        .quad(x + tw * 0.9, ground - 10, x + tw * 2.1, ground + 4).close()
    m = p.fill(shape, bark, "base")
    p.tone(p.where(m, lambda xx, yy: xx > x + tw * 0.12), bark, "shadow")
    p.tone(p.where(m, lambda xx, yy: xx < x - tw * 0.4), bark, "light")
    wood_in = p.shape(Path.polygon([(x - tw * 0.55, top + 22), (x - tw * 0.35, top - 6), (x - tw * 0.1, top + 14), (x + tw * 0.1, top - 16),
                                    (x + tw * 0.3, top + 10), (x + tw * 0.5, top + 4), (x + tw * 0.5, top + 26), (x - tw * 0.5, top + 30)])) & m
    p.tone(wood_in, tones("#c9a47a"), "base")
    p.tone(wood_in - wood_in.shift(0, -3), tones("#c9a47a"), "shadow")
    plates = Path()
    for k in range(5):
        bx = x - tw * 0.45 + k * tw * 0.22
        plates.add(Path.polyline([(bx, top + 34 + 20 * hash01(k, 1, seed)), (bx + (bx - x) * 0.5, ground - 6)]))
    p.stroke(plates, (1.4, 0.4), bark, "deep", clip=m)
    stub = Path.smooth([(x + tw * 0.4, ground - h * 0.55), (x + tw * 1.2, ground - h * 0.68), (x + tw * 1.8, ground - h * 0.74)])
    sm = p.line_shape(stub, (tw * 0.36, tw * 0.18))
    p.tone(sm, bark, "shadow")
    if moss:
        mm = p.below(m, ground - h * 0.18 * moss - 6) & p.where(m, lambda xx, yy: xx < x + tw * 0.4)
        p.tone(mm, tones("#6f9a50"), "base")
        p.tone(mm - mm.shift(-1.5, -2), tones("#6f9a50"), "shadow", alpha=0.6)
    if veins:
        vv = Path()
        for k in range(int(2 + 4 * veins)):
            bx = x - tw * 0.4 + tw * 0.8 * hash01(k, 3, seed)
            vv.add(Path.polyline([(bx, ground - 4), (bx + 4, ground - h * 0.35), (bx - 3, ground - h * 0.6 * (0.6 + 0.4 * hash01(k, 4, seed)))]))
        p.tone(p.line_shape(vv, (3.0, 0.8)) & m, tones("#2a1e30"), "base")
        p.tone(p.line_shape(vv, (1.1, 0.3)) & m, tones(VEIN), "light")
        p.glow(x, ground - h * 0.3, 40, VEIN, 0.14)
    p.edge(shape, bark, 1.3)
    p.ground_shadow(x - tw * 2, x + tw * 2, ground + 3, 8, 0.32)
    layer.place(p)
    return p


def undergrowth(layer, x0, x1, ground, h=60, colour="#4f8f56", seed=0, ferns=True, dark=0.0, flowers=None):
    """The undergrowth along a wood's floor: ferns' fronds fanning up out of a heap of low clumps,
    from x0 to x1; `dark` pulls it toward the deep wood's gloom."""
    t = tones(colour) if not dark else haze(tones(colour), "#1c1830", dark)
    hedgerow(layer, x0, x1, ground, h * 0.75, colour=colour if not dark else theme_mix(colour, "#1c1830", dark),
             back=theme_mix(colour, "#1c1830", 0.3 + dark * 0.7), seed=seed, flowers=flowers)
    if not ferns:
        return
    n = int((x1 - x0) / 90)
    for i in range(n):
        fx = x0 + (i + 0.5) * (x1 - x0) / n + (hash01(i, 1, seed) - 0.5) * 40
        fern(layer, fx, ground + 2, h * (0.8 + 0.4 * hash01(i, 2, seed)), t, seed + i)


def fern(layer, x, ground, h, t, seed=0):
    """A fern: seven long fronds fanning up out of the ground, each a narrow leaf with a darker
    midrib and notched edges, lit on its upper side."""
    p = Piece(x - h * 1.1, ground - h * 1.1, x + h * 1.1, ground + 6)
    for k in range(7):
        a = math.radians(-162 + k * 24 + (hash01(k, 1, seed) - 0.5) * 10)
        ln = h * (0.7 + 0.3 * math.sin(math.pi * k / 6))
        ex, ey = x + math.cos(a) * ln, ground + math.sin(a) * ln
        mx, my = x + math.cos(a) * ln * 0.55, ground + math.sin(a) * ln * 0.55 - ln * 0.12
        nx, ny = -math.sin(a), math.cos(a)
        wd = ln * 0.13
        pts = [(x, ground)]
        for j in range(1, 8):
            u = j / 8.0
            bx = x + (mx - x) * 2 * u * (1 - u) + (ex - x) * u * u
            by = ground + (my - ground) * 2 * u * (1 - u) + (ey - ground) * u * u
            ww = wd * math.sin(math.pi * min(1.0, u * 1.1)) * (1.0 if j % 2 else 0.7)
            pts.append((bx + nx * ww, by + ny * ww))
        pts.append((ex, ey))
        back = []
        for j in range(7, 0, -1):
            u = j / 8.0
            bx = x + (mx - x) * 2 * u * (1 - u) + (ex - x) * u * u
            by = ground + (my - ground) * 2 * u * (1 - u) + (ey - ground) * u * u
            ww = wd * math.sin(math.pi * min(1.0, u * 1.1)) * (1.0 if j % 2 else 0.7)
            back.append((bx - nx * ww, by - ny * ww))
        frond = Path.polygon(pts + back)
        m = p.fill(frond, t, "base" if k % 2 else "base-light")
        p.tone(m - m.shift(-1.4, -1.8), t, "shadow")
        p.stroke(Path().move(x, ground).quad(mx, my, ex, ey), (1.4, 0.3), t, "terminator", clip=m)
        p.edge(frond, t, 0.9)
    layer.place(p, reach=1.6)
    return p


def theme_mix(a, b, k):
    """Two colours mixed (OKLab) as '#rrggbb'."""
    import theme
    return theme.hexof(P.mixc(a, b, k))


def footbridge(layer, x0, x1, ground, seed=0, sag=16):
    """The rangers' footbridge over a gully at the back of the wood: a dark cleft in the bank with
    a trickle of black water in it, and a bridge of planks hung between posts on rope rails."""
    cx = (x0 + x1) / 2
    p = Piece(x0 - 40, ground - 140, x1 + 40, ground + 10)
    earth = tones("#6b5a50")
    cleft = Path.smooth([(x0 + 10, ground - 70), (x0 + 30, ground - 30), (cx - 30, ground + 4), (cx + 30, ground + 4), (x1 - 30, ground - 30),
                         (x1 - 10, ground - 70)])
    cleft.line(x1 - 10, ground - 90).line(x0 + 10, ground - 90).close()
    cm = p.fill(cleft, earth, "shadow")
    p.tone(p.below(cm, ground - 40), earth, "deep")
    tar = p.shape(Path.ellipse(cx, ground - 6, (x1 - x0) * 0.18, 6)) & cm
    p.tone(tar, tones("#2a1e30"), "base")
    p.stroke(Path.polyline([(cx - 20, ground - 7), (cx + 10, ground - 7)]), 1.2, tones(VEIN), "light", alpha=0.7)
    p.edge(cleft, earth, 1.4, "deep")
    wood = tones("wood")
    deck_y = ground - 88
    for px in (x0 + 6, x1 - 6):
        post = Path.rect(px - 5, deck_y - 44, 10, 64)
        pm = p.fill(post, wood, "base")
        p.tone(p.where(pm, lambda xx, yy, a=px: xx > a + 1), wood, "shadow")
        p.edge(post, wood, 1.1)
        p.fill(Path.circle(px, deck_y - 46, 6), wood, "light")
    deck = Path().move(x0 + 6, deck_y).quad(cx, deck_y + sag * 2, x1 - 6, deck_y)
    n = int((x1 - x0) / 13)
    for k in range(n):
        u = (k + 0.5) / n
        (bx, by), (dx, dy) = deck.at(u)
        plank = Path.polygon([(bx - 5.5, by - 3), (bx + 5.5, by - 3 + dy * 6), (bx + 5.5, by + 6 + dy * 6), (bx - 5.5, by + 6)])
        pm = p.fill(plank, tones("wood.light"), "base" if k % 3 else "terminator")
        p.tone(pm - pm.shift(0, 1.5), tones("wood.light"), "light")
        p.edge(plank, tones("wood.light"), 0.9)
    for off in (40, 22):
        rope = Path().move(x0 + 6, deck_y - off).quad(cx, deck_y - off + sag * 2.2, x1 - 6, deck_y - off)
        p.stroke(rope, 2.0, tones("rope"), "base")
        p.stroke(rope.translate(0, -0.8), 0.8, tones("rope"), "light")
    for k in range(1, 6):
        u = k / 6
        (ax, ay), _ = Path().move(x0 + 6, deck_y - 40).quad(cx, deck_y - 40 + sag * 2.2, x1 - 6, deck_y - 40).at(u)
        (bx, by), _ = deck.at(u)
        p.stroke(Path.polyline([(ax, ay), (bx, by)]), 1.2, tones("rope"), "shadow")
    layer.place(p)
    return p


def mine_mouth(layer, cx, ground, seed=0, w=380, h=260, rock="#8a7f8e", lantern=True):
    """The way into the old quarry: an outcrop of grey rock rising out of the wood's floor, roots
    and moss over it, and in it a timber-framed adit, two posts and a lintel beam over a dark
    opening, with a lantern on a hook and a warning board nailed up. Returns the doorway."""
    p = Piece(cx - w / 2 - 30, ground - h - 30, cx + w / 2 + 30, ground + 12)
    rt = tones(rock)
    pts = [(cx - w / 2, ground + 4)]
    for k in range(13):
        u = k / 12.0
        hx = cx - w / 2 + w * u
        hy = ground - h * (math.sin(math.pi * min(1.0, u * 1.08)) ** 0.7) * (0.82 + 0.18 * hash01(k, 1, seed))
        pts.append((hx + (hash01(k, 2, seed) - 0.5) * 14, hy))
    pts.append((cx + w / 2, ground + 4))
    out = Path.polygon(pts)
    m = p.shape(out)
    U.facets(p, m, rock, seed, cell=58, squash=0.85)
    p.tone(p.where(m, lambda xx, yy: xx > cx + w * 0.3), rt, "shadow", alpha=0.6)
    p.tone(m - m.shift(-4, 8), rt, "light", alpha=0.8)
    moss = m - m.shift(0, 16)
    p.tone(moss & p.where(m, lambda xx, yy: xx < cx + w * 0.2), tones("#6f9a50"), "base")
    # roots trailing over the rock
    bark = tones("#4a3a44")
    for k in range(5):
        rx = cx - w * 0.35 + k * w * 0.18
        root = Path.smooth([(rx, ground - h * 0.86 + 10 * k % 30), (rx + 14, ground - h * 0.6), (rx - 6, ground - h * 0.4), (rx + 10, ground - h * 0.18)])
        p.tone(p.line_shape(root, (5, 1.5)) & m, bark, "base")
    # the adit
    dw, dh = 120, 168
    dx0 = cx - dw / 2
    op = Path.rect(dx0, ground - dh, dw, dh + 2)
    dark = tones("#2c2434")
    dm = p.fill(op, dark, "base")
    p.tone(dm - dm.shift(8, 10), dark, "deep")
    for k in range(3):
        bx = dx0 + 16 + k * 40
        p.stroke(Path.polyline([(bx, ground - dh + 30), (bx + 6, ground)]), 2.0, dark, "light", clip=dm, alpha=0.4)
    wood = tones("wood.dark")
    for px in (dx0 - 6, dx0 + dw - 8):
        post = Path.rect(px, ground - dh - 6, 14, dh + 8)
        pm = p.fill(post, wood, "base")
        p.tone(p.where(pm, lambda xx, yy, a=px: xx > a + 8), wood, "shadow")
        p.tone(p.where(pm, lambda xx, yy, a=px: xx < a + 4), wood, "light")
        p.edge(post, wood, 1.1)
    beam = Path.rect(dx0 - 22, ground - dh - 24, dw + 44, 20, r=2)
    bm = p.fill(beam, wood, "base")
    p.tone(bm - bm.shift(0, 3), wood, "light")
    p.cast(bm, 0, 5, dm, dark, "deep")
    for k in range(4):
        p.stroke(Path.polyline([(dx0 - 10 + k * 40, ground - dh - 18), (dx0 + 20 + k * 40, ground - dh - 15)]), (1.0, 0.0), wood, "deep", clip=bm)
    p.edge(beam, wood, 1.2)
    board = Path.polygon([(dx0 + dw + 14, ground - dh + 30), (dx0 + dw + 64, ground - dh + 24), (dx0 + dw + 66, ground - dh + 54), (dx0 + dw + 16, ground - dh + 60)])
    bt = tones("wood.light")
    p.fill(board, bt, "base")
    p.stroke(Path.polyline([(dx0 + dw + 24, ground - dh + 34), (dx0 + dw + 56, ground - dh + 48)]), 2.6, tones("cloth.red"), "base")
    p.stroke(Path.polyline([(dx0 + dw + 56, ground - dh + 32), (dx0 + dw + 24, ground - dh + 50)]), 2.6, tones("cloth.red"), "base")
    p.edge(board, bt, 1.0)
    if lantern:
        p.stroke(Path.polyline([(dx0 - 4, ground - dh + 4), (dx0 - 22, ground - dh + 4), (dx0 - 22, ground - dh + 12)]), 2.0, tones("iron"), "base")
        P.lantern_body(p, dx0 - 22, ground - dh + 12, 1.0, True)
    p.edge(out, rt, 1.4)
    p.ground_shadow(cx - w * 0.5, cx + w * 0.5, ground + 2, 10, 0.32)
    layer.place(p)
    return (dx0, ground - dh, dw, dh)


# --------------------------------------------------------------------------- #
# Camps: tents, fires, the post at the gate
# --------------------------------------------------------------------------- #

TENT_COLOURS = ("canvas", "#c9b48e", "#9fb4c8", "#d9a07a", "#b9a4c8", "#a9bf8e")


def tent(layer, x, ground, w=150, h=120, colour="canvas", seed=0, patches=True, open_=True, pennant=None, trim=None, piece=None, lit=False):
    """A ridge tent seen from its front corner: the front triangle with its door flap tied back on a
    dark inside, the long side going back in shadow, guy ropes to pegs, a pole's end at the peak,
    patches stitched on if it's seen hard use, a pennant if asked."""
    p = piece or Piece(x - w / 2 - 50, ground - h - 50, x + w / 2 + w * 0.5 + 30, ground + 10)
    t = tones(colour)
    d = w * 0.42
    apex = (x, ground - h)
    back_apex = (x + d, ground - h - d * 0.18)
    side = Path.polygon([apex, back_apex, (x + w / 2 + d, ground - d * 0.18), (x + w / 2, ground + 1)])
    sm = p.fill(side, t, "shadow")
    p.tone(p.shape(Path.polygon([apex, back_apex, (back_apex[0] + 4, back_apex[1] + 14), (apex[0] + 4, apex[1] + 14)])) & sm, t, "base", alpha=0.7)
    for k in range(1, 4):
        u = k / 4
        a = (apex[0] + (back_apex[0] - apex[0]) * u, apex[1] + (back_apex[1] - apex[1]) * u)
        b = (x + w / 2 + d * u, ground + 1 - d * 0.18 * u)
        p.stroke(Path.polyline([a, b]), 1.0, t, "deep-shadow", clip=sm, alpha=0.6)
    p.edge(side, t, 1.2)
    front = Path().move(x - w / 2, ground + 1).quad(x - w * 0.22, ground - h * 0.48, apex[0], apex[1]).quad(x + w * 0.2, ground - h * 0.5, x + w / 2, ground + 1).close()
    fm = p.fill(front, t, "base")
    p.tone(p.where(fm, lambda xx, yy: xx < x - w * 0.28), t, "light", alpha=0.8)
    if trim:
        p.tone(p.below(fm, ground - 12), tones(trim), "base")
    if patches:
        for k in range(2):
            px_ = x - w * 0.3 + w * 0.5 * hash01(k, 1, seed) if k == 0 else x + d * 0.4 + w * 0.2
            py_ = ground - h * (0.25 + 0.3 * hash01(k, 2, seed))
            pc = tones(pick(TENT_COLOURS, k, 3, seed + 1))
            pp = Path.rect(px_ - 9, py_ - 8, 18 + 6 * hash01(k, 4, seed), 15, r=1)
            pm = p.fill(pp, pc, "base" if k == 0 else "shadow", clip=fm if k == 0 else sm)
            dash = Path()
            for j in range(5):
                dash.add(Path.polyline([(px_ - 8 + j * 4, py_ - 9), (px_ - 6.5 + j * 4, py_ - 9)]))
                dash.add(Path.polyline([(px_ - 8 + j * 4, py_ + 8), (px_ - 6.5 + j * 4, py_ + 8)]))
            p.stroke(dash, 0.9, pc, "deep", clip=pm)
            p.edge(pp, pc, 0.9, "deep-shadow")
    if open_:
        door = Path.polygon([(x - w * 0.18, ground + 1), (x, ground - h * 0.78), (x + w * 0.18, ground + 1)])
        inside = tones("#4a3a48") if not lit else tones("#8a5a40")
        dm = p.fill(door, inside, "base")
        p.tone(dm - dm.shift(4, 4), inside, "deep")
        if lit:
            p.glow(x, ground - h * 0.2, w * 0.5, "#ffb060", 0.25)
        flap = Path.polygon([(x, ground - h * 0.78), (x - w * 0.18, ground + 1), (x - w * 0.34, ground - h * 0.12), (x - w * 0.18, ground - h * 0.32)])
        flm = p.fill(flap, t, "base-light")
        p.tone(p.where(flm, lambda xx, yy: xx > x - w * 0.2), t, "shadow")
        p.edge(flap, t, 1.0)
        p.stroke(Path.polyline([(x - w * 0.26, ground - h * 0.2), (x - w * 0.18, ground - h * 0.24)]), 1.6, tones("rope"), "base")
    p.edge(front, t, 1.3)
    wood = tones("wood")
    p.stroke(Path.polyline([(apex[0], apex[1] + 4), (apex[0], apex[1] - 12)]), 3.0, wood, "base")
    if pennant:
        pt = tones(pennant)
        p.stroke(Path.polyline([(apex[0], apex[1] - 10), (apex[0], apex[1] - 30)]), 1.6, wood, "base")
        p.fill(Path.polygon([(apex[0] + 1, apex[1] - 30), (apex[0] + 22, apex[1] - 25), (apex[0] + 1, apex[1] - 20)]), pt, "base")
    rope = tones("rope")
    for (ax, ay), (gx, gy) in (((x - w * 0.42, ground - h * 0.18), (x - w / 2 - 34, ground + 1)), ((apex[0], apex[1] + 2), (x - w / 2 - 44, ground + 1)),
                               ((back_apex[0], back_apex[1] + 2), (x + w / 2 + d + 26, ground - 4))):
        p.stroke(Path.polyline([(ax, ay), (gx, gy)]), 1.1, rope, "deep", alpha=0.85)
        p.fill(Path.rect(gx - 2, gy - 7, 4, 9), wood, "shadow")
    p.ground_shadow(x - w * 0.55, x + w * 0.55 + d, ground + 2, 9, 0.3)
    if piece is None:
        layer.place(p)
    return p


def bell_tent(layer, x, ground, w=230, h=200, colour="cloth.cream", trim="cloth.teal", seed=0, sign="leaf", pennant="cloth.teal"):
    """A big round bell tent (the clinic's): a tall cone on a short wall, scalloped trim round its
    eave, the front laced open on a lit inside, a pennant on its pole, and a board by the door with
    a green sprig painted on it."""
    p = Piece(x - w / 2 - 70, ground - h - 50, x + w / 2 + 70, ground + 10)
    t, tt = tones(colour), tones(trim)
    eave = ground - h * 0.32
    wall = Path.polygon([(x - w / 2, ground + 1), (x - w / 2 + 4, eave), (x + w / 2 - 4, eave), (x + w / 2, ground + 1)])
    wm = p.fill(wall, t, "base")
    p.tone(p.where(wm, lambda xx, yy: xx > x + w * 0.18), t, "shadow")
    p.tone(p.where(wm, lambda xx, yy: xx < x - w * 0.36), t, "light")
    for k in range(1, 8):
        sx = x - w / 2 + w * k / 8
        p.stroke(Path.polyline([(sx, eave), (sx, ground)]), 1.0, t, "terminator", clip=wm, alpha=0.7)
    cone = Path().move(x - w / 2 - 14, eave + 4).quad(x - w * 0.22, eave - h * 0.3, x, ground - h).quad(x + w * 0.22, eave - h * 0.3, x + w / 2 + 14, eave + 4).close()
    cm = p.fill(cone, t, "base-light")
    p.tone(p.where(cm, lambda xx, yy: xx > x + w * 0.08), t, "shadow")
    for k in range(1, 6):
        sx = x - w / 2 - 14 + (w + 28) * k / 6
        p.stroke(Path.polyline([(x, ground - h), (sx, eave + 4)]), 1.0, t, "terminator", clip=cm, alpha=0.6)
    val = Path().move(x - w / 2 - 14, eave)
    n = 14
    for k in range(n):
        ax = x - w / 2 - 14 + (w + 28) * k / n
        bx = x - w / 2 - 14 + (w + 28) * (k + 1) / n
        val.quad((ax + bx) / 2, eave + 18, bx, eave)
    val.line(x + w / 2 + 14, eave - 6).line(x - w / 2 - 14, eave - 6).close()
    vm = p.fill(val, tt, "base")
    p.tone(p.where(vm, lambda xx, yy: xx > x + w * 0.2), tt, "shadow")
    p.edge(val, tt, 1.1)
    p.edge(cone, t, 1.3)
    door = Path.polygon([(x - w * 0.16, ground + 1), (x - w * 0.1, eave + 14), (x + w * 0.1, eave + 14), (x + w * 0.16, ground + 1)])
    inside = tones("#c98a5a")
    dm = p.fill(door, inside, "base")
    p.tone(dm - dm.shift(5, 4), inside, "shadow")
    p.glow(x, ground - 40, w * 0.6, "#ffcf8a", 0.22)
    for side in (-1, 1):
        fl = Path.polygon([(x + side * w * 0.1, eave + 14), (x + side * w * 0.3, eave + 16), (x + side * w * 0.22, ground + 1), (x + side * w * 0.16, ground + 1)])
        p.fill(fl, t, "base-light" if side < 0 else "shadow")
        p.edge(fl, t, 1.0)
    p.edge(wall, t, 1.2)
    wood = tones("wood")
    p.stroke(Path.polyline([(x, ground - h + 2), (x, ground - h - 34)]), 3.0, wood, "base")
    p.fill(Path.polygon([(x + 1, ground - h - 34), (x + 34, ground - h - 27), (x + 1, ground - h - 20)]), tones(pennant), "base")
    if sign:
        sx = x + w / 2 + 34
        p.stroke(Path.polyline([(sx, ground + 1), (sx, ground - 84)]), 4, wood, "base")
        board = Path.rect(sx - 26, ground - 112, 52, 40, r=4)
        bt = tones("cloth.cream")
        p.fill(board, bt, "base")
        leaf = tones("leaf")
        p.stroke(Path.polyline([(sx - 8, ground - 80), (sx + 6, ground - 104)]), 2.0, tones("wood"), "base")
        for k in range(3):
            ly = ground - 86 - k * 7
            for sd in (-1, 1):
                p.fill(Path.ellipse(sx - 4 + k * 4 + sd * 5, ly, 5, 2.6, rot=-40 * sd - 30), leaf, "base")
        p.edge(board, tones("wood.dark"), 1.4, "base")
    p.ground_shadow(x - w * 0.6, x + w * 0.6, ground + 2, 10, 0.32)
    layer.place(p)
    return p


def campfire(layer, x, ground, seed=0, pot=False, size=1.0, lit=True, glow=1.0):
    """A campfire: a ring of stones, logs crossed in it, flames in three tones and their glow; a
    pot on an iron tripod over it if asked (a cooking fire)."""
    s = size
    p = Piece(x - 60 * s, ground - 120 * s, x + 60 * s, ground + 10)
    st = tones("stone.grey")
    logs = tones("bark")
    for k, (a, b) in enumerate(((-34, 22), (-20, -26), (8, 30))):
        lg = Path.capsule((x + a * s, ground - 4 * s - k * 3 * s), (x + b * s, ground - 12 * s - k * 2 * s), 5 * s, 4.5 * s)
        lm = p.fill(lg, logs, "base")
        p.tone(lm - lm.shift(0, 1.6), logs, "light")
        p.edge(lg, logs, 1.0)
        p.fill(Path.circle(x + b * s, ground - 12 * s - k * 2 * s, 3 * s), tones("#d9a46a"), "base")
    if lit:
        fl = tones("#ff9a3c")
        inner = tones("#ffe08a")
        for k, (fx, fh) in enumerate(((-14, 40), (-4, 58), (8, 46), (16, 30))):
            cx_ = x + fx * s
            fh *= s
            flame = Path().move(cx_ - 10 * s, ground - 8 * s).quad(cx_ - 12 * s, ground - 8 * s - fh * 0.6, cx_ + 1, ground - 8 * s - fh) \
                .quad(cx_ + 3 * s, ground - 8 * s - fh * 0.45, cx_ + 10 * s, ground - 8 * s).close()
            p.fill(flame, fl, "base")
            core = Path().move(cx_ - 5 * s, ground - 8 * s).quad(cx_ - 5 * s, ground - 8 * s - fh * 0.4, cx_ + 1, ground - 8 * s - fh * 0.64) \
                .quad(cx_ + 2 * s, ground - 8 * s - fh * 0.3, cx_ + 5 * s, ground - 8 * s).close()
            p.fill(core, inner, "base")
        p.glow(x, ground - 26 * s, 140 * s * glow, "#ff9a4a", 0.42)
        p.glow(x, ground - 22 * s, 50 * s, "#ffe0a0", 0.5)
    for k in range(9):
        a = math.pi + math.pi * (k + 0.5) / 9
        sx = x + math.cos(a) * 40 * s
        sy = ground + 2 + math.sin(a) * 6 * s
        if k in (3, 4, 5):
            sy = ground + 3
        stone = Path.ellipse(sx, sy - 5 * s, 9 * s, 7 * s)
        sm = p.fill(stone, st, "base")
        p.tone(sm - sm.shift(-1.4, -1.6), st, "shadow")
        p.edge(stone, st, 1.0)
    if pot:
        iron = tones("iron")
        for a, b in (((-46, 4), (0, -96)), ((46, 4), (0, -96)), ((10, 8), (0, -96))):
            p.stroke(Path.polyline([(x + a[0] * s, ground + a[1] * s), (x + b[0] * s, ground + b[1] * s)]), 2.6 * s, iron, "base")
        p.stroke(Path.polyline([(x, ground - 94 * s), (x, ground - 66 * s)]), 1.4 * s, iron, "deep")
        potp = Path().move(x - 20 * s, ground - 66 * s).line(x + 20 * s, ground - 66 * s).quad(x + 24 * s, ground - 40 * s, x, ground - 36 * s) \
            .quad(x - 24 * s, ground - 40 * s, x - 20 * s, ground - 66 * s).close()
        pm = p.fill(potp, iron, "base")
        p.tone(p.where(pm, lambda xx, yy: xx > x + 6 * s), iron, "shadow")
        p.tone(p.where(pm, lambda xx, yy: xx < x - 12 * s), iron, "light")
        p.fill(Path.rect(x - 22 * s, ground - 70 * s, 44 * s, 6 * s, r=2), iron, "base-light")
        p.edge(potp, iron, 1.1)
    p.ground_shadow(x - 46 * s, x + 46 * s, ground + 3, 6, 0.3)
    layer.place(p)
    return p


def stump_table(layer, x, ground, seed=0):
    """The rangers' stump table: a great sawn stump, its top ringed, a map pinned to it with a
    dagger, two mugs and a candle."""
    p = Piece(x - 90, ground - 110, x + 90, ground + 12)
    bark = tones("bark")
    w, h = 130, 58
    body = Path().move(x - w / 2 - 12, ground + 3).quad(x - w / 2, ground - 10, x - w / 2 + 2, ground - h).line(x + w / 2 - 2, ground - h) \
        .quad(x + w / 2, ground - 10, x + w / 2 + 14, ground + 3).close()
    m = p.fill(body, bark, "base")
    p.tone(p.where(m, lambda xx, yy: xx > x + w * 0.2), bark, "shadow")
    for k in range(6):
        bx = x - w / 2 + 10 + k * 22
        p.stroke(Path.polyline([(bx, ground - h + 8), (bx + (bx - x) * 0.1, ground)]), (1.4, 0.3), bark, "deep", clip=m)
    top = Path.ellipse(x, ground - h, w / 2, 11)
    wt = tones("#d9b07a")
    tm = p.fill(top, wt, "base")
    for k in range(1, 4):
        p.stroke(Path.ellipse(x + 4, ground - h, w / 2 * k / 4, 11 * k / 4), 0.9, wt, "shadow", clip=tm)
    p.edge(top, bark, 1.6, "base")
    mp = Path.polygon([(x - 36, ground - h - 2), (x + 20, ground - h - 6), (x + 28, ground - h + 4), (x - 30, ground - h + 7)])
    mt = tones("#efe0c0")
    p.fill(mp, mt, "base")
    p.stroke(Path.polyline([(x - 24, ground - h + 2), (x - 4, ground - h - 2), (x + 14, ground - h + 1)]), 1.0, tones("cloth.red"), "base")
    p.edge(mp, mt, 0.9)
    p.stroke(Path.polyline([(x + 6, ground - h), (x + 12, ground - h - 26)]), 2.2, tones("#c9cfe0"), "base")
    p.fill(Path.rect(x + 9, ground - h - 34, 6, 9, r=1), tones("wood.dark"), "base")
    for mx in (x - 46, x + 40):
        mug = Path.rect(mx - 6, ground - h - 16, 12, 15, r=2)
        p.fill(mug, tones("pottery"), "base")
        p.stroke(Path.circle(mx + 8, ground - h - 9, 3.5), 1.4, tones("pottery"), "base")
        p.edge(mug, tones("pottery"), 0.9)
    cx_ = x - 20
    p.fill(Path.rect(cx_ - 3, ground - h - 22, 6, 18), tones("cloth.cream"), "base")
    p.fill(Path.ellipse(cx_, ground - h - 27, 2.6, 5.5), tones("#ffcf6a"), "light")
    p.glow(cx_, ground - h - 27, 60, "#ffc070", 0.35)
    p.ground_shadow(x - w * 0.6, x + w * 0.6, ground + 3, 8, 0.34)
    layer.place(p)
    return p


def weapon_rack(layer, x, ground, seed=0):
    """A rack of bows, spears and a quiver leaning on a rail between two posts."""
    p = Piece(x - 60, ground - 150, x + 60, ground + 10)
    wood = tones("wood")
    for px in (x - 44, x + 44):
        post = Path.rect(px - 4, ground - 100, 8, 100)
        p.fill(post, wood, "base")
        p.edge(post, wood, 1.0)
    rail = Path.rect(x - 52, ground - 96, 104, 7, r=2)
    p.fill(rail, wood, "light")
    p.edge(rail, wood, 1.0)
    for k in range(5):
        sx = x - 36 + k * 18
        if k % 2 == 0:
            p.stroke(Path.polyline([(sx, ground), (sx + 6, ground - 138)]), 2.4, tones("wood.light"), "base")
            p.fill(Path.polygon([(sx + 3, ground - 138), (sx + 6, ground - 152), (sx + 10, ground - 138)]), tones("#c9cfe0"), "base")
        else:
            bow = Path.smooth([(sx, ground - 4), (sx + 10, ground - 60), (sx + 2, ground - 120)])
            p.stroke(bow, (2.8, 2.0), tones("wood.dark"), "base")
            p.stroke(Path.polyline([(sx, ground - 4), (sx + 2, ground - 120)]), 0.8, tones("cloth.cream"), "base")
    p.ground_shadow(x - 50, x + 50, ground + 2, 6, 0.3)
    layer.place(p)
    return p


def lantern_line(layer, points, sag=24, seed=0):
    """Lanterns hung on a cord between trees (the camp's): plain iron lanterns, every one lit."""
    for (ax, ay), (bx, by) in zip(points, points[1:]):
        p = Piece(min(ax, bx) - 20, min(ay, by) - 10, max(ax, bx) + 20, max(ay, by) + sag * 2 + 50)
        cord = Path().move(ax, ay).quad((ax + bx) / 2, (ay + by) / 2 + sag * 2, bx, by)
        p.stroke(cord, 1.4, tones("rope"), "deep")
        n = max(1, int(abs(bx - ax) / 150))
        for i in range(n):
            (lx, ly), _ = cord.at((i + 0.5) / n)
            P.lantern_body(p, lx, ly, 0.9, True)
        layer.place(p, reach=1.4)


def guard_post(layer, x, ground, seed=0):
    """The watch's post at the camp gate: a little timber booth under a striped awning, a notice
    board of pinned papers beside it, a barrel and Questeria's banner on a pole."""
    p = Piece(x - 130, ground - 260, x + 130, ground + 10)
    wood = tones("wood")
    w, h = 130, 150
    booth = Path.rect(x - w / 2, ground - h, w, h)
    P.wall(p, x - w / 2, ground - h, x + w / 2, ground, "timber", seed, quoins=False)
    win = Path.rect(x - w / 2 + 18, ground - h + 30, w - 36, 50)
    inside = tones("#5a4650")
    wm = p.fill(win, inside, "base")
    p.tone(wm - wm.shift(4, 4), inside, "deep")
    ledge = Path.rect(x - w / 2 + 10, ground - h + 78, w - 20, 8, r=2)
    p.fill(ledge, wood, "light")
    p.edge(ledge, wood, 1.0)
    p.edge(win, tones("wood.dark"), 2.0, "base")
    p.edge(booth, tones("wood.dark"), 1.4)
    roof = Path.polygon([(x - w / 2 - 14, ground - h + 4), (x - w / 2 - 6, ground - h - 26), (x + w / 2 + 6, ground - h - 26), (x + w / 2 + 14, ground - h + 4)])
    rt = tones("roof.slate")
    rm = p.fill(roof, rt, "base")
    p.tone(p.where(rm, lambda xx, yy: xx > x + w * 0.3), rt, "shadow")
    P._roof_tiles(p, rm, rt, x - w / 2 - 14, x + w / 2 + 14, ground - h - 26, ground - h + 2, seed, row=10, tile=14)
    p.edge(roof, rt, 1.2)
    aw = P.awning(x - w / 2 + 8, x + w / 2 - 8, ground - h + 22, 22, ("cloth.blue", "cloth.cream"))
    aw(p)
    nb = Path.rect(x + w / 2 + 14, ground - 128, 70, 56, r=3)
    p.stroke(Path.polyline([(x + w / 2 + 22, ground), (x + w / 2 + 22, ground - 72)]), 5, wood, "base")
    p.stroke(Path.polyline([(x + w / 2 + 76, ground), (x + w / 2 + 76, ground - 72)]), 5, wood, "base")
    p.fill(nb, tones("wood.light"), "base")
    for k in range(4):
        px_ = x + w / 2 + 22 + (k % 2) * 30
        py_ = ground - 122 + (k // 2) * 25
        paper = Path.rect(px_, py_, 24, 20, r=1)
        p.fill(paper, tones("cloth.cream"), "base")
        for j in range(3):
            p.stroke(Path.polyline([(px_ + 4, py_ + 5 + j * 5), (px_ + 19, py_ + 5 + j * 5)]), 0.9, tones("wood.dark"), "base", alpha=0.6)
        p.fill(Path.circle(px_ + 12, py_ + 2, 1.8), tones("cloth.red"), "base")
        p.edge(paper, tones("cloth.cream"), 0.8, "shadow")
    p.edge(nb, tones("wood.light"), 1.1)
    P.barrel(None, x - w / 2 - 28, ground + 1, 36, 46, piece=p, seed=seed)
    P.banner(x - w / 2 - 74, ground - 230, 30, 86, "cloth.navy", "sun")(p)
    p.stroke(Path.polyline([(x - w / 2 - 74, ground), (x - w / 2 - 74, ground - 236)]), 4, wood, "base")
    p.ground_shadow(x - w * 0.7, x + w * 0.9, ground + 2, 9, 0.33)
    layer.place(p)
    return p


def city_gate(layer, cx, ground, seed=0, wall_x0=None):
    """Questeria's east gate from outside: a tall arch in the curtain wall between two round towers
    under blue cone roofs, the portcullis drawn up into its slot, a glimpse of the sunlit city
    through it, banners on the towers, and the wall with its battlements running off to the west."""
    mid_w = 230
    wall_h = 300
    x0 = wall_x0 if wall_x0 is not None else cx - 600
    p = Piece(x0 - 10, ground - wall_h - 260, cx + mid_w / 2 + 120, ground + 12)
    st = tones("stone")
    # the curtain wall west of the gate
    P.wall(p, x0, ground - wall_h + 50, cx - mid_w / 2, ground, "stone", seed, quoins=False)
    for k in range(int((cx - mid_w / 2 - x0) / 34) + 1):
        bx = x0 + k * 34
        if k % 2 == 0:
            mer = Path.rect(bx, ground - wall_h + 26, 22, 26)
            p.fill(mer, st, "base")
            p.edge(mer, st, 1.0, "shadow")
    p.stroke(Path.polyline([(x0, ground - wall_h + 52), (cx - mid_w / 2, ground - wall_h + 52)]), 4, st, "light")
    # the gatehouse block
    gx0, gx1 = cx - mid_w / 2, cx + mid_w / 2
    P.wall(p, gx0, ground - wall_h - 30, gx1, ground, "stone.warm", seed + 1, quoins=False)
    for k in range(7):
        bx = gx0 + k * mid_w / 6.5
        if k % 2 == 0:
            mer = Path.rect(bx, ground - wall_h - 56, 24, 28)
            p.fill(mer, tones("stone.warm"), "base")
            p.edge(mer, tones("stone.warm"), 1.0, "shadow")
    aw, ah = 132, 220
    arch = P._opening("arch", cx - aw / 2, ground - ah, aw, ah)
    am = p.fill(arch, tones("#cfe0f0"), "base")
    # the city seen through: a sunlit street, a house front, the sky
    p.tone(p.below(am, ground - ah * 0.45), tones("plaster.yellow"), "base")
    p.tone(p.below(am, ground - ah * 0.45) & p.where(am, lambda xx, yy: xx > cx + 10), tones("plaster.pink"), "base")
    p.tone(p.below(am, ground - 30), tones("slab"), "base")
    p.tone(p.shape(Path.polygon([(cx - aw / 2, ground - ah * 0.45), (cx - 10, ground - ah * 0.62), (cx + 30, ground - ah * 0.45)])) & am, tones("roof.pink"), "base")
    p.tone(am - am.shift(10, 14), tones("#5a5068"), "base", alpha=0.75)
    vou = Path().move(cx - aw / 2 - 18, ground).line(cx - aw / 2 - 18, ground - ah + aw / 2).arc(cx, ground - ah + aw / 2, aw / 2 + 18, 180, 360) \
        .line(cx + aw / 2 + 18, ground).line(cx + aw / 2, ground).line(cx + aw / 2, ground - ah + aw / 2).arc(cx, ground - ah + aw / 2, aw / 2, 360, 180).close()
    vm = p.fill(vou, tones("stone"), "base-light")
    for k in range(1, 12):
        a = math.radians(180 + 180 * k / 12)
        p.stroke(Path.polyline([(cx + math.cos(a) * aw / 2, ground - ah + aw / 2 + math.sin(a) * aw / 2),
                                (cx + math.cos(a) * (aw / 2 + 18), ground - ah + aw / 2 + math.sin(a) * (aw / 2 + 18))]), 1.1, st, "shadow", clip=vm)
    p.edge(vou, st, 1.2, "terminator")
    # the portcullis, up
    iron = tones("iron")
    pg = p.shape(Path.rect(cx - aw / 2, ground - ah, aw, 46)) & am
    for k in range(7):
        bx = cx - aw / 2 + 10 + k * (aw - 20) / 6
        p.tone(p.line_shape(Path.polyline([(bx, ground - ah), (bx, ground - ah + 54)]), 4) & am, iron, "base")
        p.fill(Path.polygon([(bx - 3, ground - ah + 52), (bx, ground - ah + 62), (bx + 3, ground - ah + 52)]), iron, "base", clip=am)
    for k in range(2):
        p.tone(p.line_shape(Path.polyline([(cx - aw / 2, ground - ah + 18 + k * 22), (cx + aw / 2, ground - ah + 18 + k * 22)]), 3.4) & am, iron, "base")
    del pg
    # the towers
    for side in (-1, 1):
        tx = cx + side * (mid_w / 2 + 34)
        tw = 104
        house = House(tx - tw / 2, tw, ground, floors=(("stone", wall_h + 40),), roof="cone", roof_colour="roof.blue", roof_h=170,
                      eave=10, quoins=True, finials=True, seed=seed + 3 + side)
        P.wall(p, tx - tw / 2, ground - wall_h - 40, tx + tw / 2, ground, "stone", seed + 5 + side, quoins=True)
        tm = p.shape(Path.rect(tx - tw / 2, ground - wall_h - 40, tw, wall_h + 40))
        p.tone(p.where(tm, lambda xx, yy, a=tx: xx > a + tw * 0.18), st, "shadow", alpha=0.85)
        p.tone(p.where(tm, lambda xx, yy, a=tx: xx < a - tw * 0.36), st, "light", alpha=0.6)
        rm, _ = P.roof(p, house)
        for k, wy in enumerate((ground - wall_h + 30, ground - wall_h * 0.55)):
            P.window(p, Window(tx, wy, 16, 30, "arch", lit=k == 1, sill=True, panes=1), st, seed + k)
        P.banner(tx, ground - wall_h + 90, 30, 100, "cloth.navy" if side < 0 else "cloth.pink", "sun" if side < 0 else "diamond")(p)
        p.edge(Path.rect(tx - tw / 2, ground - wall_h - 40, tw, wall_h + 40), st, 1.3)
    p.ground_shadow(x0, cx + mid_w / 2 + 100, ground + 2, 10, 0.33)
    layer.place(p)
    return p


def river(layer, x0, x1, y0, y1, seed=0, colour="water", bank="#5f9a4e"):
    """A river running across the back of the room between y0 (the far bank) and y1 (the near
    bank's foot): bands of blue darkening toward the near bank, lines of light lying on it, the far
    bank's grassy lip over it and reeds along both."""
    t = tones(colour)
    bt = tones(bank)
    for xa, xb in chunks(x0, x1):
        cx0, cx1 = xa - OVERLAP, xb + OVERLAP
        p = Piece(cx0, y0 - 30, cx1, y1 + 8, ss=2, pad=4)
        body = Path.rect(cx0, y0, cx1 - cx0, y1 - y0 + 4)
        m = p.fill(body, t, "base")
        p.tone(p.above(m, y0 + (y1 - y0) * 0.35), t, "base-light")
        p.tone(p.below(m, y0 + (y1 - y0) * 0.75), t, "shadow")
        n = int((cx1 - cx0) * (y1 - y0) / 1800)
        for i in range(n):
            k = int(cx0 / 50) * 1000 + i
            lx = cx0 + (cx1 - cx0) * hash01(k, 1, seed)
            ly = y0 + 6 + (y1 - y0 - 10) * hash01(k, 2, seed)
            ln = 10 + 26 * hash01(k, 3, seed)
            p.stroke(Path.polyline([(lx, ly), (lx + ln, ly)]), lambda u: 2.0 * math.sin(math.pi * u), t, "light" if ly < (y0 + y1) / 2 else "base-light")
        lip = Path.smooth([(cx0 - 10, y0 + 4), (cx0 + 200, y0 - 2), (cx0 + 400, y0 + 3), (cx1 + 10, y0 - 1)])
        lip.line(cx1 + 10, y0 - 18).line(cx0 - 10, y0 - 18).close()
        lm = p.fill(lip, bt, "base")
        p.tone(p.below(lm, y0 - 4), bt, "shadow")
        place_span(layer, p, xa, xb, reach=1.2)


def big_bridge(layer, x0, x1, deck_y, water_y, seed=0, arches=3, colour="stone", parapet=40):
    """An old stone bridge of several arches seen side on, its piers standing in the water with
    cutwaters, the arches' soffits in shadow, voussoirs, a string course, a parapet with a lit
    coping and moss creeping up from the water."""
    w = x1 - x0
    p = Piece(x0 - 20, deck_y - parapet - 20, x1 + 20, water_y + 12)
    st = tones(colour)
    span = w / arches
    pier = span * 0.2
    ar = (span - pier) / 2
    spring = water_y - ar * 0.25
    m = P.wall(p, x0, deck_y - parapet, x1, water_y + 2, colour, seed, quoins=False)
    holes = p.nothing()
    archs = []
    for k in range(arches):
        cx = x0 + span * (k + 0.5)
        a = Path().move(cx - ar, water_y + 4).line(cx - ar, spring).arc(cx, spring, ar, 180, 360, ar * 0.9).line(cx + ar, water_y + 4).close()
        archs.append((cx, a))
        holes = holes | p.shape(a)
    p.c.erase((m & holes).a)
    wt = tones("water")
    for cx, a in archs:
        inside = p.shape(a)
        p.tone(inside, tones("#4c6a8c"), "base")
        p.tone(p.below(inside, water_y - ar * 0.2), wt, "base")
        for k in range(4):
            yy = water_y - ar * 0.16 + k * 5
            p.stroke(Path.polyline([(cx - ar * 0.6 + k * 10, yy), (cx - ar * 0.1 + k * 10, yy)]), 1.4, wt, "light", clip=inside, alpha=0.8)
        p.tone(inside - inside.shift(0, 16), tones("#3e3858"), "base", alpha=0.85)
        vou = Path().move(cx - ar - 16, water_y + 4).line(cx - ar - 16, spring).arc(cx, spring, ar + 16, 180, 360, ar * 0.9 + 16) \
            .line(cx + ar + 16, water_y + 4).line(cx + ar, water_y + 4).line(cx + ar, spring).arc(cx, spring, ar, 360, 180, ar * 0.9).close()
        vm = p.fill(vou, st, "base-light")
        for j in range(1, 13):
            ang = math.radians(180 + 180 * j / 13)
            p.stroke(Path.polyline([(cx + math.cos(ang) * ar, spring + math.sin(ang) * ar * 0.9),
                                    (cx + math.cos(ang) * (ar + 16), spring + math.sin(ang) * (ar * 0.9 + 16))]), 1.1, st, "shadow", clip=vm)
        p.edge(vou, st, 1.1, "terminator")
    for k in range(arches + 1):
        px = x0 + span * k
        cw = Path.polygon([(px - pier / 2 - 4, water_y + 4), (px - pier / 2 - 4, water_y - 30), (px, water_y - 44), (px + pier / 2 + 4, water_y - 30), (px + pier / 2 + 4, water_y + 4)])
        cm = p.fill(cw, st, "base")
        p.tone(p.where(cm, lambda xx, yy, a=px: xx > a), st, "shadow")
        p.edge(cw, st, 1.0)
    body = p.shape(Path.rect(x0, deck_y - parapet, w, water_y - deck_y + parapet + 2)) - holes
    moss = p.below(body, water_y - 18) & body
    p.tone(moss, tones("#5f8a58"), "base", alpha=0.85)
    sc = p.shape(Path.rect(x0 - 4, deck_y - 2, w + 8, 7))
    p.tone(sc, st, "light")
    p.tone(sc.shift(0, 7) & body, st, "shadow")
    cop = Path.rect(x0 - 8, deck_y - parapet - 10, w + 16, 12, r=2)
    cpt = tones("stone.grey")
    cpm = p.fill(cop, cpt, "base")
    p.tone(cpm - cpm.shift(0, 3), cpt, "light")
    p.tone((cpm.shift(0, 6) & body) - cpm, st, "shadow")
    p.edge(cop, cpt, 1.1, "shadow")
    p.edge(Path.rect(x0, deck_y - parapet, w, water_y - deck_y + parapet + 2), st, 1.3)
    layer.place(p)
    return p


def fork(r):
    """The fork in the Old Road (12-rooms §6): the meadow going on from the farmland, a stream at the
    west end with its anglers' blossom trees, the barrow's grassy mound with its portal of three
    great stones on the zone file's door, the great oak the young couple walk out to, and east of
    it the wood thickening, darker and older, toward the Mirewood."""
    g = r.ground
    seed = roomkit.seed_of("road", "fork")
    _meadow_sky_far(r, seed, sun_x=r.x("sky", 1100), woods_from=r.x("far", 1900))
    mid = r["mid"]
    W = mid.w
    bank(mid, 0, 2300, g, g - 50, seed=seed + 10, flowers=("flower.white", "flower.yellow", "flower.violet"))
    bank(mid, 2300, W, g, g - 54, colour="#5c9c52", seed=seed + 11, flowers=("flower.white",))
    P.floor_shade(mid, g, 46, 0.26)

    # the west end: an oak over the stream, blossom, hedges full of flowers
    big_tree(mid, 80, g - 4, 450, "lush", seed=seed + 20, crown_w=400, moss=0.2)
    P.tree(mid, 355, g - 6, "blossom", 320, seed=seed + 21)
    hedgerow(mid, 420, 640, g - 2, 60, seed=seed + 22, flowers=("flower.yellow", "flower.white"))
    hedgerow(mid, 700, 900, g - 2, 56, seed=seed + 23, flowers=("flower.blue", "flower.white", "flower.violet"))
    P.tree(mid, 986, g - 6, "blossom", 300, seed=seed + 24)
    fir(mid, 1080, g - 4, 270, seed=seed + 25)

    # the barrow, its portal on the zone file's door
    bx = r.door_x("barrow")
    r.doorway("barrow", barrow_mound(mid, bx, g, 540, 236, seed=seed + 30))
    boulders(mid, bx - 330, g, 90, 46, seed=seed + 31, moss=0.6)
    boulders(mid, bx + 300, g, 80, 40, seed=seed + 32, moss=0.4)
    P.bush(mid, bx + 380, g - 2, 110, 56, seed=seed + 33, flowers_on=("flower.pink", "flower.white"))

    # the great oak
    big_tree(mid, 1954, g - 4, 620, "oak", seed=seed + 40, crown_w=620, trunk_w=74, moss=0.35)
    hedgerow(mid, 2120, 2300, g - 2, 58, seed=seed + 41, flowers=("flower.white", "flower.yellow"))

    # the wood thickening toward the Mirewood
    undergrowth(mid, 2280, W, g - 2, 72, colour="#4f8f56", seed=seed + 50, flowers=("flower.white",))
    big_tree(mid, 2380, g - 6, 470, "old", seed=seed + 51, crown_w=380, moss=0.5)
    fir(mid, 2514, g - 4, 340, colour="#36755c", seed=seed + 52)
    big_tree(mid, 2640, g - 6, 540, "oak", seed=seed + 53, crown_w=420, moss=0.3)
    fir(mid, 2800, g - 4, 310, colour="#36755c", seed=seed + 54)
    big_tree(mid, 2960, g - 6, 560, "old", seed=seed + 55, crown_w=420, moss=0.6)
    big_tree(mid, 3170, g - 6, 520, "old", seed=seed + 56, crown_w=400, moss=0.6)
    mist(mid, 2500, W, g - 8, 50, 0.18, seed=seed + 57)

    fore = r["fore"]
    fore_grass(fore, r.x("fore", 229), CANVAS_H + 20, 200, 90, 0.5, seed + 70)
    P.fore_foliage(fore, r.x("fore", 844), CANVAS_H + 28, 210, 80, seed + 71, 0.5)
    fore_grass(fore, r.x("fore", 1490), CANVAS_H + 20, 180, 80, 0.5, seed + 72, flowers=("flower.pink", "flower.white"))
    fore_grass(fore, r.x("fore", 2025), CANVAS_H + 20, 160, 70, 0.55, seed + 73)
    fore_tree(fore, r.x("fore", 3020), "old", 0.6, seed + 74, w=520, trunk=60, lean=10)
    P.near_shade(fore, 820, 0.18)


def far_wood(layer, base, height, colour, haze_c, k=0.4, seed=0, firs=0.35):
    """A wood seen far off: a row of round crowns and fir spires along `base`, filled down to the
    layer's foot, lit on their upper left rims and shadowed on their lower right, hazed."""
    t = haze(tones(colour), haze_c, k)
    pc = Piece(-20, base - height * 1.9, layer.w + 20, layer.h, ss=2, pad=4)
    whole = pc.shape(Path.rect(-20, base - height * 0.35, layer.w + 40, layer.h - base + height))
    x = -20.0
    i = 0
    while x < layer.w + 40:
        hh = height * (0.6 + 0.6 * hash01(i, 1, seed))
        if hash01(i, 2, seed) < firs:
            w = hh * 0.42
            pts = [(x - w / 2, base)]
            for k_ in range(3):
                yy = base - hh * (k_ + 1) / 3.4
                ww = w / 2 * (1 - k_ / 3.2)
                pts += [(x - ww, yy + hh * 0.08), (x - ww * 0.5, yy)]
            pts.append((x, base - hh * 1.15))
            for k_ in range(2, -1, -1):
                yy = base - hh * (k_ + 1) / 3.4
                ww = w / 2 * (1 - k_ / 3.2)
                pts += [(x + ww * 0.5, yy), (x + ww, yy + hh * 0.08)]
            pts.append((x + w / 2, base))
            whole = whole | pc.shape(Path.polygon(pts))
            x += w * 0.7
        else:
            r_ = hh * 0.45
            whole = whole | pc.shape(Path.circle(x, base - hh * 0.62, r_)) | pc.shape(Path.circle(x + r_ * 0.7, base - hh * 0.45, r_ * 0.75))
            x += r_ * (1.3 + 0.4 * hash01(i, 3, seed))
        i += 1
    pc.tone(whole, t, "base")
    pc.tone(whole - whole.shift(-5, -8), t, "shadow")
    pc.tone(whole - whole.shift(4, 7), t, "light", alpha=0.8)
    layer.place(pc, ink=P._far_line(t, haze_c, k), reach=0.8)


def _wood_sky_far(r, seed, time="twilight", sun=None, moon=None, gloom=0.0, mountains=True):
    """The Mirewood's sky and far layer: a twilight (or night) sky, hills going violet in the
    distance, and the wood itself far off in rows of round crowns, each row nearer and darker,
    mist lying between them."""
    sky, far = r["sky"], r["far"]
    haze_c = P.HAZE[time]
    P.sky(sky, time, seed=seed, horizon=600, sun=sun, moon=moon, clouds=3, cloud_band=(50, 260))
    W = far.w
    if mountains:
        P.mountains(far, 420, 260, haze_c, seed=seed + 1, k=0.6, count=7)
    rows = (("#3f8a6a", 0.5, 440, 80), ("#2f7a5c", 0.38, 492, 100), ("#24684e", 0.26, 548, 110))
    for i, (c, k, base, hgt) in enumerate(rows):
        far_wood(far, base, hgt, c, haze_c, k=min(0.8, k + gloom), seed=seed + 2 + i, firs=0.3 + 0.1 * i)
        mist(far, 0, W, base + 16, 40, 0.16 + 0.04 * i, colour=P.mixc(haze_c, "#ffffff", 0.45), seed=seed + 10 + i, blobs=9)


def edge(r):
    """The Mirewood's edge (12-rooms §6): the road out of the meadows going in under the first
    trees at dusk; bright and green at the west end, a blossom tree and lush oaks, the wood growing
    older, mossier and stranger to the east, the leaves turning, until the last trees have black
    veins in their bark with a violet light in them and mist lies along the ground."""
    g = r.ground
    seed = roomkit.seed_of("mirewood", "edge")
    _wood_sky_far(r, seed, "dusk", sun=(r.x("sky", 300), 330))
    mid = r["mid"]
    W = mid.w
    bank(mid, 0, 1700, g, g - 44, colour="#62a052", seed=seed + 10, flowers=("flower.white", "flower.yellow", "flower.pink"))
    bank(mid, 1700, W, g, g - 46, colour="#557f56", seed=seed + 11)
    P.floor_shade(mid, g, 52, 0.3)
    undergrowth(mid, 0, 1600, g - 2, 64, colour="#5a9a56", seed=seed + 12, flowers=("flower.white", "flower.pink"))
    undergrowth(mid, 1600, W, g - 2, 70, colour="#4a7a58", seed=seed + 13)

    P.tree(mid, 192, g - 6, "blossom", 330, seed=seed + 20)
    big_tree(mid, 420, g - 4, 520, "lush", seed=seed + 21, crown_w=420, moss=0.2)
    big_tree(mid, 900, g - 4, 560, "oak", seed=seed + 22, crown_w=460, moss=0.3)
    P.bush(mid, 1120, g - 2, 120, 60, seed=seed + 23, flowers_on=("flower.white", "flower.yellow"))
    big_tree(mid, 1318, g - 4, 520, "oak", seed=seed + 24, crown_w=420, moss=0.4)
    big_tree(mid, 1740, g - 6, 580, "old", seed=seed + 25, crown_w=440, moss=0.6)
    big_tree(mid, 2138, g - 6, 540, "turning", seed=seed + 26, crown_w=430, moss=0.5)
    big_tree(mid, 2420, g - 6, 560, "old", seed=seed + 27, crown_w=400, moss=0.7, veins=0.25)
    big_tree(mid, 2640, g - 6, 520, "turning", seed=seed + 28, crown_w=400, moss=0.5, veins=0.4)
    big_tree(mid, 2850, g - 6, 560, "blackvein", seed=seed + 29, crown_w=420, moss=0.4, veins=0.8)
    big_tree(mid, 3110, g - 6, 600, "blackvein", seed=seed + 30, crown_w=440, moss=0.3, veins=1.0)
    mist(mid, 1900, W, g - 8, 70, 0.3, seed=seed + 31, blobs=16)

    fore = r["fore"]
    fore_tree(fore, r.x("fore", 154), "lush", 0.55, seed + 40, w=520, trunk=58, lean=-10)
    fore_grass(fore, r.x("fore", 1100), CANVAS_H + 20, 180, 80, 0.55, seed + 41)
    P.fore_foliage(fore, r.x("fore", 2000), CANVAS_H + 30, 200, 80, seed + 42, 0.55)
    fore_grass(fore, r.x("fore", 3085), CANVAS_H + 20, 220, 90, 0.6, seed + 43, colour="#3f5a4a", flowers=None)
    P.near_shade(fore, 820, 0.22)


def deep(r):
    """The Deep Wood (12-rooms §6): old trees with roots like walls and moss up their trunks, black
    veins splitting their bark with a violet light in them, dead snags, the rangers' footbridge
    over a gully of tar, mist along the ground, and the old quarry's timbered mouth in an outcrop
    of grey rock on the zone file's door."""
    g = r.ground
    seed = roomkit.seed_of("mirewood", "deep")
    _wood_sky_far(r, seed, "twilight", gloom=0.08)
    mid = r["mid"]
    W = mid.w
    bank(mid, 0, W, g, g - 46, colour="#4f7a55", seed=seed + 10)
    P.floor_shade(mid, g, 56, 0.34)
    undergrowth(mid, 0, W, g - 2, 66, colour="#467055", seed=seed + 11)

    big_tree(mid, 160, g - 6, 600, "old", seed=seed + 20, crown_w=460, moss=0.8, veins=0.3, trunk_w=60)
    big_tree(mid, 560, g - 4, 460, "bare", seed=seed + 21, crown_w=380, moss=0.3, veins=0.5)
    footbridge(mid, 680, 900, g - 4, seed=seed + 22)
    snag(mid, 980, g - 2, 250, seed=seed + 23, veins=0.6)
    big_tree(mid, 1240, g - 6, 620, "old", seed=seed + 24, crown_w=480, moss=0.9, trunk_w=64)
    big_tree(mid, 1520, g - 6, 580, "blackvein", seed=seed + 25, crown_w=440, moss=0.5, veins=1.0)
    big_tree(mid, 1800, g - 6, 600, "old", seed=seed + 26, crown_w=440, moss=0.8, veins=0.5)
    qx = r.door_x("quarry")
    r.doorway("quarry", mine_mouth(mid, qx, g, seed=seed + 30))
    big_tree(mid, 2480, g - 6, 620, "blackvein", seed=seed + 31, crown_w=460, moss=0.4, veins=1.0)
    big_tree(mid, 2760, g - 6, 560, "old", seed=seed + 32, crown_w=420, moss=0.9, veins=0.6)
    snag(mid, 2994, g - 2, 280, seed=seed + 33, veins=0.9)
    big_tree(mid, 3160, g - 6, 600, "blackvein", seed=seed + 34, crown_w=440, moss=0.5, veins=0.9)
    mist(mid, 0, W, g - 6, 80, 0.32, seed=seed + 35, blobs=26)

    fore = r["fore"]
    fore_tree(fore, r.x("fore", 359), "turning", 0.6, seed + 40, w=500, trunk=56, lean=-8)
    fore_tree(fore, r.x("fore", 1659), "blackvein", 0.6, seed + 41, w=460, trunk=54, lean=12)
    fore_tree(fore, r.x("fore", 2982), "blackvein", 0.62, seed + 42, w=480, trunk=58, lean=-6)
    P.near_shade(fore, 800, 0.26)


def camp(r):
    """The Rangers' Camp (12-rooms §6): a clearing in the Mirewood at night by firelight, the moon
    over the trees, the rangers' tents, the stump table with its map, a rack of bows and spears,
    crates and firewood, and lanterns strung between the trunks; the fire itself is the floor's."""
    g = r.ground
    seed = roomkit.seed_of("mirewood", "camp")
    _wood_sky_far(r, seed, "night", moon=(r.x("sky", 1150), 120), gloom=0.05, mountains=False)
    mid = r["mid"]
    W = mid.w
    bank(mid, 0, W, g, g - 44, colour="#4a7a52", seed=seed + 10)
    P.floor_shade(mid, g, 56, 0.34)
    undergrowth(mid, 0, 260, g - 2, 70, colour="#467055", seed=seed + 11)
    undergrowth(mid, 1340, W, g - 2, 70, colour="#467055", seed=seed + 12)
    big_tree(mid, 70, g - 6, 640, "old", seed=seed + 20, crown_w=500, moss=0.7, trunk_w=62)
    big_tree(mid, 1540, g - 6, 640, "old", seed=seed + 21, crown_w=500, moss=0.7, trunk_w=62)
    big_tree(mid, 800, g - 12, 560, "oak", seed=seed + 22, crown_w=540, moss=0.4)
    tent(mid, 330, g - 2, 170, 128, "#a9bf8e", seed=seed + 30, pennant="leaf", patches=True, lit=True)
    weapon_rack(mid, 540, g, seed=seed + 31)
    P.crate(mid, 620, g + 2, 46, 40, seed=seed + 32)
    P.barrel(mid, 668, g + 2, 36, 46, seed=seed + 33)
    stump_table(mid, 980, g + 2, seed=seed + 34)
    tent(mid, 1180, g - 2, 160, 120, "#c9b48e", seed=seed + 35, patches=True, lit=False)
    P.sacks(mid, 1340, g + 2, 2, seed=seed + 36)
    lantern_line(mid, [(150, 330), (520, 360), (800, 300)], sag=22, seed=seed + 37)
    lantern_line(mid, [(800, 300), (1150, 350), (1460, 320)], sag=24, seed=seed + 38)
    # the fire's light on the clearing (the campfire is a floor prop at 800)
    mid.glow(800, g + 40, 520, "#ff9a4a", 0.22)
    mid.glow(800, g + 60, 240, "#ffc070", 0.18)

    fore = r["fore"]
    fore_tree(fore, r.x("fore", 64), "old", 0.65, seed + 40, w=520, trunk=60, lean=-8)
    P.fore_foliage(fore, r.x("fore", 763), CANVAS_H + 30, 180, 70, seed + 41, 0.6)
    fore_tree(fore, r.x("fore", 1500), "old", 0.65, seed + 42, w=500, trunk=58, lean=10)
    P.near_shade(fore, 800, 0.24)


def _camp_sky_far(r, seed, wall=False):
    """Thornbridge's sky and far layer: a bright day, lavender mountains, the meadows east of the
    city in patchwork, and (if wall) Questeria's wall and its spires far off over the camp."""
    sky, far = r["sky"], r["far"]
    haze_c = P.HAZE["day"]
    P.sky(sky, "day", seed=seed, horizon=560, sun=(sky.w * 0.3, 110), clouds=4, cloud_band=(70, 280))
    W = far.w
    P.mountains(far, 452, 280, haze_c, seed=seed + 1, k=0.58, count=7)
    if wall:
        P.castle(far, W * 0.2, 470, 0.55, haze_c, k=0.3, seed=seed + 2)
        P.skyline(far, 0, W, 500, haze_c, k=0.48, seed=seed + 3, height=(60, 150), spires=0.15)
        city_wall_far(far, 0, W, 520, haze_c, k=0.4, seed=seed + 4)
    field_hills(far, [(-40, 520), (W * 0.25, 500), (W * 0.5, 515), (W * 0.75, 498), (W + 40, 512)], haze_c, k=0.4, seed=seed + 5, rows=2, trees=8)
    field_hills(far, [(-40, 566), (W * 0.2, 552), (W * 0.45, 562), (W * 0.7, 548), (W + 40, 560)], haze_c, k=0.28, seed=seed + 6, rows=1, trees=6)


def city_wall_far(layer, x0, x1, base_y, haze_c, k=0.4, seed=0, h=70):
    """Questeria's wall far off: a long crenellated curtain with round towers under blue cones."""
    st = haze(tones("stone"), haze_c, k)
    rt = haze(tones("roof.blue"), haze_c, k)
    pc = Piece(x0, base_y - h - 90, x1, layer.h, ss=2, pad=4)
    body = Path.rect(x0 - 10, base_y - h, x1 - x0 + 20, layer.h - base_y + h)
    m = pc.fill(body, st, "base")
    for i in range(int((x1 - x0) / 16) + 1):
        if i % 2 == 0:
            pc.fill(Path.rect(x0 + i * 16, base_y - h - 9, 9, 10), st, "base")
    pc.tone(pc.below(m, base_y - h + 8), st, "shadow", alpha=0.5)
    x = x0 + 80
    i = 0
    while x < x1:
        tw = 34 + 10 * hash01(i, 1, seed)
        P._tower(pc, x, base_y + 10, tw, h + 50, st, rt, 46, seed + i, windows=1)
        x += 260 + 140 * hash01(i, 2, seed)
        i += 1
    layer.place(pc, ink=P._far_line(st, haze_c, k), reach=0.8)


def camp_gate(r):
    """Thornbridge's camp gate (12-rooms §6): Questeria's east gate at the west end, its towers under
    blue cones and the sunlit city through the arch, the wall running off west; cypresses by the
    road, Hedda's post with its notice board, and the first of the refugees' tents."""
    g = r.ground
    seed = roomkit.seed_of("thornbridge", "camp-gate")
    _camp_sky_far(r, seed)
    mid = r["mid"]
    W = mid.w
    bank(mid, 0, W, g, g - 48, seed=seed + 10, flowers=("flower.white", "flower.yellow"))
    P.floor_shade(mid, g, 48, 0.28)
    city_gate(mid, 300, g, seed=seed + 20, wall_x0=-20)
    P.tree(mid, 520, g - 4, "cypress", 330, seed=seed + 21)
    P.tree(mid, 566, g - 4, "cypress", 290, seed=seed + 22)
    guard_post(mid, 770, g, seed=seed + 23)
    hedgerow(mid, 930, 1010, g - 2, 50, seed=seed + 24, flowers=("flower.pink",))
    tent(mid, 1090, g - 4, 150, 118, "canvas", seed=seed + 25, pennant="cloth.blue")
    campfire(mid, 1290, g, seed=seed + 26, pot=True, size=0.9)
    tent(mid, 1430, g - 6, 170, 128, "#9fb4c8", seed=seed + 27)
    P.crate(mid, 1240, g + 2, 40, 34, seed=seed + 28)

    fore = r["fore"]
    P.lamp_post(fore, r.x("fore", 470), CANVAS_H + 40, 560, lit=False, dark=0.5, reach=2.6)
    fore_grass(fore, r.x("fore", 1000), CANVAS_H + 20, 180, 80, 0.5, seed + 30)
    P.near_shade(fore, 820, 0.18)


def tents(r):
    """Thornbridge's tents (12-rooms §6): the refugees' camp in the meadow under the city wall, rows
    of patched tents in every colour, cooking fires with pots on tripods, washing lines, crates and
    bundles, the clinic's big bell tent on the zone file's healer station, and blossom at the east
    end toward the river."""
    g = r.ground
    seed = roomkit.seed_of("thornbridge", "tents")
    _camp_sky_far(r, seed, wall=True)
    mid = r["mid"]
    W = mid.w
    bank(mid, 0, W, g, g - 50, seed=seed + 10, flowers=("flower.white", "flower.yellow"))
    P.floor_shade(mid, g, 48, 0.28)
    # the back row of tents, a little smaller, then the front row
    back = [(380, "#c9b48e", 130), (860, "#9fb4c8", 120), (1400, "#d9a07a", 126), (2150, "#b9a4c8", 120), (2640, "canvas", 128), (3120, "#a9bf8e", 120)]
    for i, (x, c, w) in enumerate(back):
        tent(mid, x, g - 26, w, w * 0.78, c, seed=seed + 20 + i, open_=hash01(i, 1, seed) < 0.6)
    big_tree(mid, 140, g - 6, 470, "oak", seed=seed + 30, crown_w=400)
    front = [(525, "canvas", 160, "cloth.pink"), (690, "#a9bf8e", 150, None), (1251, "#c9b48e", 170, "cloth.blue"), (1560, "#9fb4c8", 160, None),
             (2256, "canvas", 170, "cloth.gold"), (2452, "#d9a07a", 150, None), (2780, "#b9a4c8", 160, "cloth.teal"), (3040, "canvas", 160, None)]
    fires = [(1000, True), (1700, True), (2600, True)]
    for i, (x, c, w, pen) in enumerate(front):
        tent(mid, x, g - 4, w, w * 0.78, c, seed=seed + 40 + i, pennant=pen, lit=i % 3 == 0)
    washing_line(mid, 1050, 1180, g - 8, 130, seed=seed + 50, cloths=("cloth.cream", "cloth.blue", "cloth.cream", "cloth.pink"))
    washing_line(mid, 2880, 2990, g - 8, 124, seed=seed + 51, cloths=("cloth.gold", "cloth.cream", "cloth.teal"))
    for i, (x, pot) in enumerate(fires):
        campfire(mid, x, g + 2, seed=seed + 60 + i, pot=pot, size=0.95)
    P.crate(mid, 860, g + 2, 44, 38, seed=seed + 70, top_goods="apple")
    P.sacks(mid, 1355, g + 2, 3, seed=seed + 71)
    P.barrel(mid, 1660, g + 2, 36, 46, seed=seed + 72)
    P.crate(mid, 2360, g + 2, 40, 34, seed=seed + 73)
    P.barrel(mid, 2540, g + 2, 34, 44, seed=seed + 74)
    # the clinic
    hx = 1879
    bell_tent(mid, hx, g - 2, 250, 214, seed=seed + 80)
    P.tree(mid, 2815, g - 30, "blossom", 300, seed=seed + 81)
    P.tree(mid, 2954, g - 6, "blossom", 250, seed=seed + 82)
    P.bush(mid, 3140, g - 2, 110, 56, seed=seed + 83, flowers_on=("flower.blue", "flower.white"))

    fore = r["fore"]
    fore_grass(fore, r.x("fore", 300), CANVAS_H + 20, 180, 80, 0.5, seed + 90)
    P.fore_foliage(fore, r.x("fore", 1300), CANVAS_H + 30, 190, 74, seed + 91, 0.5)
    fore_grass(fore, r.x("fore", 2300), CANVAS_H + 20, 170, 80, 0.5, seed + 92)
    P.fore_foliage(fore, r.x("fore", 3100), CANVAS_H + 30, 200, 80, seed + 93, 0.5)
    P.near_shade(fore, 820, 0.18)


def river_edge(r):
    """Thornbridge's river edge (12-rooms §6): the old bridge itself at the west end, three arches
    of mossy stone standing in the river, the river running across the back of the room under a
    far bank of meadow and trees, reeds along the near bank, blossom and an oak by the water."""
    g = r.ground
    seed = roomkit.seed_of("thornbridge", "river-edge")
    _camp_sky_far(r, seed)
    mid = r["mid"]
    W = mid.w
    bank(mid, 0, W, g - 76, g - 120, colour="#6aac56", seed=seed + 10, fade=6, flowers=("flower.white", "flower.yellow"))
    for i, x in enumerate((900, 1080, 1420)):
        P.tree(mid, x, g - 84, "oak" if i != 1 else "blossom", 190 + 30 * (i % 2), seed=seed + 11 + i)
    hedgerow(mid, 1120, 1380, g - 82, 34, seed=seed + 15, flowers=("flower.white",))
    river(mid, -10, W + 10, g - 78, g + 4, seed=seed + 20)
    big_bridge(mid, -60, 720, g - 200, g - 4, seed=seed + 21, arches=3)
    boulders(mid, 780, g - 2, 100, 40, colour="stone.grey", seed=seed + 22, moss=0.7)
    # reeds and stones along the near bank
    reeds(mid, 0, W, g + 2, seed=seed + 23)
    P.floor_shade(mid, g + 2, 40, 0.24)
    P.tree(mid, 1010, g - 2, "blossom", 300, seed=seed + 24)
    big_tree(mid, 1260, g - 2, 500, "oak", seed=seed + 25, crown_w=440, moss=0.3)

    fore = r["fore"]
    fore_tree(fore, r.x("fore", 120), "lush", 0.55, seed + 30, w=520, trunk=58, lean=-10)
    P.fore_foliage(fore, r.x("fore", 1361), CANVAS_H + 30, 220, 84, seed + 31, 0.5)
    P.near_shade(fore, 820, 0.18)


def reeds(layer, x0, x1, ground, seed=0, colour="#5f9a4e", every=26):
    """Clumps of reeds and bulrushes along a bank, with a stone here and there."""
    t = tones(colour)
    for xa, xb in chunks(x0, x1):
        cx0, cx1 = xa - OVERLAP, xb + OVERLAP
        p = Piece(cx0, ground - 70, cx1, ground + 8, pad=4)
        i0 = int(math.floor(cx0 / every))
        for i in range(i0, int(cx1 / every) + 1):
            if hash01(i, 1, seed) < 0.35:
                continue
            bx = i * every + every * hash01(i, 2, seed)
            for k in range(5):
                rx = bx + (k - 2) * 4
                rh = 22 + 30 * hash01(i * 5 + k, 3, seed)
                lean = (k - 2) * 3 + (hash01(i * 5 + k, 4, seed) - 0.5) * 6
                p.stroke(Path.smooth([(rx, ground + 3), (rx + lean * 0.3, ground - rh * 0.6), (rx + lean, ground - rh)]), (2.8, 0.4), t,
                         "base" if k % 2 else "light")
                if k == 2 and hash01(i, 5, seed) < 0.6:
                    p.fill(Path.ellipse(rx + lean, ground - rh + 6, 2.8, 8), tones("wood"), "base")
            if hash01(i, 6, seed) < 0.25:
                sm = p.fill(Path.ellipse(bx + 10, ground - 3, 10, 6), tones("stone.grey"), "base")
                p.tone(sm - sm.shift(-1.5, -1.5), tones("stone.grey"), "shadow")
        place_span(layer, p, xa, xb)


RECIPES = {
    "road/farmland": farmland,
    "road/fork": fork,
    "mirewood/edge": edge,
    "mirewood/deep": deep,
    "mirewood/camp": camp,
    "thornbridge/camp-gate": camp_gate,
    "thornbridge/tents": tents,
    "thornbridge/river-edge": river_edge,
}
