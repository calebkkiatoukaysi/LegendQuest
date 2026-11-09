"""
The docks and the Gull's Wager, painted (12-rooms §6): Saltwind Docks' pier, harbourside and fish
market, and the Wager's main deck at night. One recipe a room, each a function of a
paint_room.Room, registered in RECIPES; tools/rooms/paint_all.py runs them.

    python tools/rooms/paint_all.py docks --no-shared --preview shots/art/rooms

The harbour's look, for every room here: Questeria's cream stone and timber gone a little salty
(sea-blue and teal paint, nets drying on the walls, lobster pots by the doors, a plum-roofed dive
bar), a stone quay along the floor's back edge with the harbour behind it, mooring piles with gulls
on them, boats moored with their sails furled, a crane hoisting crates, fish laid out on ice under
striped awnings; and across the water the far coast's lavender mountains, ships at anchor, a
lighthouse on the headland, and Questeria climbing its hill to the Sunspire. A morning sky with the
sun in the west of the picture (the light is always from the top left).

The floor props (the zone file's `prop` lines) are the game's, not the painting's: the Wager among
them, who sails in with her crew on her deck, so the harbourside paints her berth and not her.

The deck is the Wager two days out, at night: a moon and stars over a long dark swell, her far
bulwark along the floor's back edge with its rail lamps, the fore and mizzen masts and the shrouds
to all three (the mainmast is the game's prop), the forecastle and bowsprit at the left and the
lit stern cabin at the right. The room is dark (the game lays its dark over the mid layer and the
floor), so the deck is painted in moonlit wood, and the lamps glow where the zone file's lights are.

Parts this family adds, on paint.py's primitives (so the line and shading match): far_ship,
headland (with its lighthouse), quay_edge, pile, gull, boat, creels, net_drape, net_rack, fish,
fish_stall, fish_rack, crane, hung_crate, hoist, clock, flagpole, ice_hatch, boarded, bollard,
and for the deck bulwark, mast, shrouds, forecastle, stern_cabin and rail_lamp.

Only needs CPython and numpy.
"""

import math
import os
import sys

import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
import paint as P  # noqa: E402
import roomkit  # noqa: E402
from paint import Door, House, Path, Piece, Window, hash01, tones  # noqa: E402
from roomkit import CANVAS_H  # noqa: E402

QUAY = 576
"""Where the quay's far edge is, in canvas pixels: the top of the floor's tile strip, which shows the
cobbles from here down to the floor band's back edge wherever nothing stands in front of them."""

WATERLINE = 556
"""Where the harbour's water meets a moored boat's hull, just behind the quay's edge."""


# --------------------------------------------------------------------------- #
# The far layer: ships at anchor, the headland and its lighthouse
# --------------------------------------------------------------------------- #

def far_ship(layer, x, water_y, s, haze_c, k=0.45, seed=0, masts=2, sails="furled", bow=-1):
    """A ship at anchor far off: a dark hull with a pale stripe and a raised stern, two or three
    masts with their yards, the sails furled along them (or set, cream, lit on the left), and a
    red pennant, all hazed a share k."""
    hull = P.haze(tones("wood.dark"), haze_c, k)
    stripe = P.haze(tones("cloth.cream"), haze_c, k)
    canvas = P.haze(tones("canvas"), haze_c, k * 0.8)
    L, H, mh = 150 * s, 24 * s, 128 * s
    pc = Piece(x - L * 0.8, water_y - H - mh - 30 * s, x + L * 0.8, water_y + 6 * s, ss=2, pad=4)

    def m(path):
        return path if bow < 0 else path.mirror(x)

    body = m(Path().move(x - L * 0.55, water_y - H * 1.15).quad(x, water_y - H * 0.75, x + L * 0.42, water_y - H * 1.3)
             .line(x + L * 0.44, water_y - H * 0.1).quad(x + L * 0.1, water_y + H * 0.2, x - L * 0.36, water_y)
             .quad(x - L * 0.52, water_y - H * 0.3, x - L * 0.55, water_y - H * 1.15).close())
    stern = m(Path.rect(x + L * 0.2, water_y - H * 1.3 - 10 * s, L * 0.24, 12 * s))
    # masts and yards first, so the hull sits in front of their feet
    xs = [x + (-0.2 + 0.4 * i / max(1, masts - 1)) * L for i in range(masts)] if masts > 1 else [x]
    if bow > 0:
        xs = [2 * x - v for v in xs]
    for i, mx in enumerate(xs):
        top = water_y - H - mh * (1.0 - 0.12 * i)
        pc.fill(Path.rect(mx - 1.4 * s, top, 2.8 * s, water_y - H - top + 2), hull, "base")
        for j, f in enumerate((0.2, 0.5, 0.78)):
            yy = top + (water_y - H - top) * f
            half = L * (0.12 + 0.07 * j)
            if sails == "set" and j < 2:
                y2 = top + (water_y - H - top) * (0.5 if j == 0 else 0.78) - 3 * s
                sail = Path.polygon([(mx - half, yy), (mx + half, yy), (mx + half * 1.1, y2), (mx - half * 1.1, y2)])
                sm = pc.fill(sail, canvas, "base")
                pc.tone(pc.where(sm, lambda xx, yy_, a=mx: xx > a + half * 0.35), canvas, "shadow")
                pc.tone(pc.where(sm, lambda xx, yy_, a=mx: xx < a - half * 0.6), canvas, "light")
            pc.fill(Path.rect(mx - half, yy - 1.2 * s, half * 2, 2.4 * s), hull, "base")
            if sails != "set":
                pc.fill(Path.rect(mx - half * 0.9, yy - 3.6 * s, half * 1.8, 3.4 * s, r=1.5 * s), canvas, "base")
        if i == 0:
            pc.fill(Path.polygon([(mx, top), (mx + bow * -1 * 16 * s, top + 3 * s), (mx, top + 6 * s)]),
                    P.haze(tones("cloth.red"), haze_c, k * 0.7), "base")
    hm = pc.fill(body, hull, "base")
    pc.fill(stern, hull, "base")
    pc.tone(pc.below(hm, water_y - H * 0.45), hull, "shadow")
    band = m(Path().move(x - L * 0.53, water_y - H * 0.95).quad(x, water_y - H * 0.55, x + L * 0.43, water_y - H * 1.08))
    pc.stroke(band, 2.6 * s, stripe, "base", clip=hm)
    pc.tone(pc.above(hm, water_y - H * 1.0), hull, "light")
    for i in range(3):
        wx = x + (-0.1 + 0.12 * i) * L * (1 if bow < 0 else -1)
        pc.fill(Path.rect(wx, water_y - H * 0.88, 3 * s, 3 * s), P.haze(tones("glass.lit"), haze_c, k * 0.6), "base", clip=hm)
    layer.place(pc, ink=P._far_line(hull, haze_c, k), reach=0.8)
    # where she sits in the water: a pale line and a soft dark under her
    layer.soft_ellipse(x, water_y + 1, L * 0.5, 3 * s, "#ffffff", 0.25, hard=0.6)
    return pc


def headland(layer, x0, x1, water_y, top_y, haze_c, k=0.4, seed=0, lighthouse=None):
    """A rocky headland across the water: grey rock lit on its left faces and shadowed on its right,
    a green cap of turf, and (at x `lighthouse`) a white lighthouse banded in red, its lamp lit."""
    rock = P.haze(tones("stone.dark"), haze_c, k)
    turf = P.haze(tones("leaf.blue"), haze_c, k)
    w, h = x1 - x0, water_y - top_y
    pts = [(x0, water_y + 6), (x0 + w * 0.12, top_y + h * 0.55), (x0 + w * 0.3, top_y + h * 0.12), (x0 + w * 0.5, top_y),
           (x0 + w * 0.72, top_y + h * 0.18), (x0 + w * 0.9, top_y + h * 0.62), (x1, water_y + 6)]
    body = Path.smooth(pts)
    body.line(x1, water_y + 12).line(x0, water_y + 12).close()
    pc = Piece(x0 - 4, top_y - (h * 1.7 if lighthouse else 10), x1 + 4, water_y + 14, ss=2, pad=4)
    m = pc.fill(body, rock, "base")
    for i in range(5):
        fx = x0 + w * (0.2 + 0.16 * i + 0.05 * hash01(i, 1, seed + 901))
        pc.fill(Path.polygon([(fx, top_y + h * 0.1), (fx + w * 0.14, water_y + 12), (fx + w * 0.04, water_y + 12)]), rock, "shadow", clip=m)
        pc.stroke(Path.polyline([(fx - w * 0.05, top_y + h * 0.4), (fx - w * 0.08, water_y)]), 1.0, rock, "deep-shadow", clip=m)
    cap = Path.smooth([(x0 + w * 0.08, top_y + h * 0.66), (x0 + w * 0.3, top_y + h * 0.05), (x0 + w * 0.5, top_y - 3), (x0 + w * 0.74, top_y + h * 0.14),
                       (x0 + w * 0.92, top_y + h * 0.66), (x0 + w * 0.7, top_y + h * 0.36), (x0 + w * 0.5, top_y + h * 0.3), (x0 + w * 0.3, top_y + h * 0.42)], closed=True)
    tm = pc.shape(cap) & m
    pc.tone(tm, turf, "base")
    pc.tone(tm - tm.shift(-3, -5), turf, "shadow")
    pc.tone(tm - tm.shift(2, 4), turf, "light")
    if lighthouse is not None:
        lx = lighthouse
        base = top_y + h * 0.08
        th = h * 1.25
        white = P.haze(tones("cloth.cream"), haze_c, k * 0.7)
        red = P.haze(tones("cloth.red"), haze_c, k * 0.6)
        tower = Path.polygon([(lx - 11, base), (lx + 11, base), (lx + 7, base - th), (lx - 7, base - th)])
        tw = pc.fill(tower, white, "base")
        for j in range(3):
            yb = base - th * (0.22 + 0.28 * j)
            pc.fill(Path.rect(lx - 14, yb - th * 0.1, 28, th * 0.1), red, "base", clip=tw)
        pc.tone(pc.where(tw, lambda xx, yy: xx > lx + 2), white, "shadow", alpha=0.8)
        pc.fill(Path.rect(lx - 11, base - th - 3, 22, 4), red, "deep")
        lamp = Path.rect(lx - 6, base - th - 15, 12, 12)
        pc.fill(lamp, P.haze(tones("glass.lit"), haze_c, k * 0.3), "light")
        pc.fill(Path.polygon([(lx - 9, base - th - 15), (lx, base - th - 26), (lx + 9, base - th - 15)]), red, "base")
        pc.glow(lx, base - th - 9, 46, "#fff0c0", 0.35)
    layer.place(pc, ink=P._far_line(rock, haze_c, k), reach=0.8)


def harbour_far(r, seed, haze_c, horizon=486, ships=(), coast=True, city=None, headland_at=None, mole=None):
    """The docks' far layer: the far coast's mountains, the sea from the horizon down, ships at
    anchor ((x, scale, masts, sails, bow), ...), a headland with its lighthouse (x0, x1, lighthouse
    x), a harbour mole running out to a beacon (x0, x1), and, if `city` is an x, Questeria on its
    hill there with the Sunspire over it. Every x is in the far layer's own pixels (r.x("far", room
    x) gives where a room x is), so things can be put where the mid layer leaves the water showing."""
    far = r["far"]
    if coast:
        P.mountains(far, horizon, horizon - 150, haze_c, seed=seed + 1, k=0.6, count=max(3, int(far.w / 300)))
    if city is not None:
        cx = city
        P.hill(far, [(cx - 760, horizon + 4), (cx - 460, horizon - 40), (cx - 160, horizon - 92), (cx + 160, horizon - 120), (cx + 700, horizon - 130)],
               "hill", haze_c, k=0.5, seed=seed + 2, base_y=horizon + 40)
        P.skyline(far, cx - 400, cx + 700, horizon - 70, haze_c, k=0.5, seed=seed + 3, height=(60, 150), spires=0.2, lit=0.08)
        P.castle(far, cx + 230, horizon - 96, 0.55, haze_c, k=0.15, seed=seed + 4)
        P.forest_line(far, cx - 900, cx - 360, horizon - 16, 46, haze_c, k=0.45, seed=seed + 8)
        P.skyline(far, cx - 480, cx + 700, horizon + 8, haze_c, k=0.4, seed=seed + 5, height=(40, 90), spires=0.06, lit=0.05)
    P.sea(far, horizon, CANVAS_H, haze_c, seed=seed + 6)
    if headland_at is not None:
        a, b, lx = headland_at
        headland(far, a, b, horizon + 18, horizon - 50, haze_c, k=0.42, seed=seed + 7, lighthouse=lx)
    if mole is not None:
        x0, x1 = mole
        st = P.haze(tones("stone.warm"), haze_c, 0.4)
        pc = Piece(x0, horizon - 70, x1 + 30, horizon + 40, ss=2, pad=4)
        wm = pc.fill(Path.rect(x0, horizon + 22, x1 - x0, 12), st, "base")
        pc.tone(pc.above(wm, horizon + 25), st, "light")
        for i in range(int((x1 - x0) / 34)):
            pc.stroke(Path.polyline([(x0 + 14 + i * 34, horizon + 25), (x0 + 14 + i * 34, horizon + 34)]), 1.0, st, "shadow", clip=wm)
        bx = x1 - 8
        bt = P.haze(tones("stone"), haze_c, 0.35)
        tower = Path.polygon([(bx - 12, horizon + 24), (bx + 12, horizon + 24), (bx + 8, horizon - 34), (bx - 8, horizon - 34)])
        tm = pc.fill(tower, bt, "base")
        pc.tone(pc.where(tm, lambda xx, yy: xx > bx + 2), bt, "shadow")
        pc.fill(Path.rect(bx - 6, horizon - 46, 12, 12), P.haze(tones("glass.lit"), haze_c, 0.2), "light")
        pc.fill(Path.polygon([(bx - 10, horizon - 46), (bx, horizon - 58), (bx + 10, horizon - 46)]), P.haze(tones("roof.blue"), haze_c, 0.35), "base")
        pc.glow(bx, horizon - 40, 40, "#fff0c0", 0.3)
        far.place(pc, ink=P._far_line(st, haze_c, 0.4), reach=0.8)
    for i, (sx, s, masts, sails, bow) in enumerate(ships):
        far_ship(far, sx, horizon + 26 + 40 * (s - 0.5), s, haze_c, k=0.5 - 0.2 * (s - 0.5), seed=seed + 20 + i,
                 masts=masts, sails=sails, bow=bow)


# --------------------------------------------------------------------------- #
# The quay: its edge, piles and gulls, boats, pots and nets
# --------------------------------------------------------------------------- #

def quay_edge(layer, x0, x1, y=QUAY, seed=0):
    """The quay's far edge where the harbour comes up to it: a course of dressed coping stones seen
    from a little above, their tops lit and their joints dark, an iron mooring ring here and there."""
    p = Piece(x0, y - 12, x1, y + 12, ss=3, pad=4)
    st = tones("stone.grey")
    top = Path.rect(x0, y - 7, x1 - x0, 13)
    m = p.fill(top, st, "base")
    p.tone(p.above(m, y - 3), st, "light")
    p.tone(p.below(m, y + 3), st, "shadow")
    x = x0
    i = 0
    while x < x1:
        x += 54 + 30 * hash01(i, 1, seed + 911)
        if x < x1 - 6:
            p.stroke(Path.polyline([(x, y - 7), (x - 2, y + 6)]), 1.1, st, "deep-shadow", clip=m)
            if hash01(i, 2, seed + 911) < 0.3:
                p.stroke(Path.ellipse(x + 14, y - 1, 5, 3), 1.4, tones("iron"), "base")
        i += 1
    layer.place(p)


def gull(p, x, feet, s=1.0, facing=1, seed=0):
    """A gull standing (into a Piece): a white body shaded underneath, a grey wing with a black tip,
    a round white head, a yellow beak and orange legs; facing 1 looks right, -1 left."""
    white = tones("#f7f5f1")
    grey = tones("#a9b3c8")
    beak = tones("#f2b23e")
    dark = tones("#36344a")
    f = facing
    for dx in (-3, 3):
        p.stroke(Path.polyline([(x + dx * s, feet), (x + dx * 0.6 * s, feet - 8 * s)]), 1.7 * s, beak, "shadow")
    tail = Path.polygon([(x - f * 8 * s, feet - 15 * s), (x - f * 21 * s, feet - 19 * s), (x - f * 19 * s, feet - 12 * s), (x - f * 6 * s, feet - 9 * s)])
    p.fill(tail, dark, "base")
    body = Path.ellipse(x, feet - 15 * s, 13 * s, 8.5 * s, rot=-10 * f)
    bm = p.fill(body, white, "base")
    p.tone(p.below(bm, feet - 13 * s), white, "shadow")
    wing = Path.ellipse(x - f * 3 * s, feet - 17 * s, 11 * s, 5.5 * s, rot=-14 * f)
    wm = p.fill(wing, grey, "base")
    p.tone(wm - wm.shift(-1.2 * s, -1.6 * s), grey, "shadow")
    p.fill(Path.ellipse(x - f * 12 * s, feet - 19 * s, 5 * s, 3 * s, rot=-14 * f), dark, "base", clip=wm)
    p.edge(wing, grey, 0.9)
    hx, hy = x + f * 10 * s, feet - 24 * s
    hm = p.fill(Path.circle(hx, hy, 6 * s), white, "base")
    p.tone(hm - hm.shift(-1.5 * s, -1.5 * s), white, "shadow", alpha=0.7)
    p.fill(Path.polygon([(hx + f * 4.5 * s, hy - 1 * s), (hx + f * 12 * s, hy + 1 * s), (hx + f * 4.5 * s, hy + 2.6 * s)]), beak, "base")
    p.fill(Path.circle(hx + f * 2 * s, hy - 1.4 * s, 1.2 * s), dark, "base")
    p.edge(body, white, 1.0, "shadow")


def pile(layer, x, top, bottom=QUAY + 4, w=20, seed=0, gull_facing=0):
    """A mooring pile standing in the harbour behind the quay: a round post lit on its left, rope
    wound round its head, weed and wet along its foot, and a gull on top if gull_facing (1 or -1)."""
    p = Piece(x - w - 26, top - 44, x + w + 26, bottom + 4)
    t = tones("wood")
    body = Path.rect(x - w / 2, top, w, bottom - top)
    m = p.fill(body, t, "base")
    p.tone(p.where(m, lambda xx, yy: xx > x + w * 0.16), t, "shadow")
    p.tone(p.where(m, lambda xx, yy: xx < x - w * 0.3), t, "light")
    for k in range(2):
        gx = x - w * 0.15 + k * w * 0.3
        p.stroke(Path.polyline([(gx, top + 8), (gx + 1, bottom - 10)]), (1.0, 0.2), t, "deep", clip=m)
    wet = tones("leaf.blue")
    weed = Path().move(x - w, bottom)
    for i in range(7):
        weed.line(x - w / 2 + w * i / 6, bottom - 22 - 8 * hash01(i, 1, seed + 921))
    weed.line(x + w, bottom).close()
    p.fill(weed, wet, "shadow", clip=m)
    p.tone(p.below(m, bottom - 10), wet, "deep-shadow", alpha=0.6)
    rope = tones("rope")
    for k in range(3):
        yy = top + 12 + k * 5
        rm = p.fill(Path.rect(x - w / 2 - 1, yy, w + 2, 5, r=2), rope, "base")
        p.tone(rm - rm.shift(0, 1.4), rope, "light")
        p.stroke(Path.polyline([(x - w / 2, yy + 4), (x + w / 2, yy + 1)]), 0.8, rope, "deep", clip=rm)
    cap = Path.ellipse(x, top, w / 2, 3.5)
    p.fill(cap, t, "light")
    p.edge(cap, t, 1.0, "shadow")
    p.edge(body, t, 1.2)
    if gull_facing:
        gull(p, x, top + 1, 1.0, gull_facing, seed)
    layer.place(p)
    return p


def _rigged(p, x, w, h, mh, mast_x, deck_y, top, bow, mx, sail, seed):
    """A boat's mast, boom and furled sail, and her pennant (into her Piece)."""
    wood = tones("wood.light")
    mp = Path.rect(mast_x - 4, top, 8, deck_y - top + 4)
    mm = p.fill(mp, wood, "base")
    p.tone(p.where(mm, lambda xx, yy: xx > mast_x + 1), wood, "shadow")
    p.edge(mp, wood, 1.0)
    p.fill(Path.polygon([(mast_x, top), (mast_x + (-bow) * 26, top + 5), (mast_x, top + 10)]), tones("cloth.red"), "base")
    boom_y = deck_y - mh * 0.16
    boom_end = mx(x + w * 0.5)
    boom = Path.polyline([(mast_x, boom_y), (boom_end, boom_y + 6)])
    p.stroke(boom, 5, tones("wood"), "base")
    st = tones(sail)
    lump = []
    n = 7
    for i in range(n + 1):
        u = i / n
        bxu = mast_x + (boom_end - mast_x) * u
        lump.append((bxu, boom_y - 8 - 6 * math.sin(math.pi * u) - 3 * hash01(i, 1, seed + 931)))
    for i in range(n, -1, -1):
        u = i / n
        lump.append((mast_x + (boom_end - mast_x) * u, boom_y + 4 + 6 * u))
    sm = p.fill(Path.smooth(lump, closed=True), st, "base")
    p.tone(sm - sm.shift(-2, -3), st, "shadow")
    p.tone(sm - sm.shift(1.5, 2), st, "light")
    for i in range(1, 4):
        tx = mast_x + (boom_end - mast_x) * i / 4
        p.stroke(Path.polyline([(tx - 2, boom_y - 14), (tx + 2, boom_y + 8)]), 2.0, tones("rope"), "deep", clip=sm)
    p.edge_mask(sm, st, 1.0)


def boat(layer, x, water_y=WATERLINE, w=220, seed=0, hull="wood.blue", band="cloth.cream", stripe="cloth.red",
         mast=1.0, bow=-1, sail="cloth.cream", moor=None):
    # mast None: a dinghy, oars shipped and no mast
    """A fishing boat moored in the harbour, her hull above the water: planked, painted (`hull`), a
    pale band under the gunwale with a stripe below it, the lower strakes in shadow; her mast
    `mast` x her length tall with the sail furled along the boom, a red pennant, stays fore and aft,
    fenders over the side, and a ripple and her reflection where she sits. `moor`: a room x on the
    quay edge her bow line runs to."""
    h = w * 0.2
    mh = w * 1.3 * (mast or 0.2)
    t = tones(hull)
    p = Piece(x - w * 0.62, water_y - h - mh - 24, x + w * 0.62, water_y + h * 0.9)
    rig = Piece(x - w * 0.62 - (abs(moor - x) if moor is not None else 0), water_y - h - mh - 24,
                x + w * 0.62 + (abs(moor - x) if moor is not None else 0), QUAY + 6, pad=6)

    def m(path):
        return path if bow < 0 else path.mirror(x)

    def mx(v):
        return v if bow < 0 else 2 * x - v

    hull_p = m(Path().move(x - w * 0.52, water_y - h * 1.3).quad(x - w * 0.05, water_y - h * 0.82, x + w * 0.46, water_y - h * 1.08)
               .line(x + w * 0.47, water_y - h * 0.1).quad(x + w * 0.2, water_y + h * 0.5, x - w * 0.36, water_y + h * 0.1)
               .quad(x - w * 0.53, water_y - h * 0.4, x - w * 0.52, water_y - h * 1.3).close())
    sheer = m(Path().move(x - w * 0.52, water_y - h * 1.3).quad(x - w * 0.05, water_y - h * 0.82, x + w * 0.46, water_y - h * 1.08))
    # the rigging, on its own piece with a light line, so the ropes stay thin
    mast_x = mx(x - w * 0.08)
    deck_y = water_y - h * 0.95
    top = deck_y - mh
    ropes = tones("rope")
    if mast:
        rig.stroke(Path.polyline([(mast_x, top + 4), (mx(x - w * 0.52), water_y - h * 1.3)]), 1.3, ropes, "deep")
        rig.stroke(Path.polyline([(mast_x, top + 4), (mx(x + w * 0.45), water_y - h * 1.08)]), 1.3, ropes, "deep")
        for dx in (-0.1, 0.1):
            rig.stroke(Path.polyline([(mast_x, top + mh * 0.25), (mast_x + w * dx, deck_y)]), 1.1, ropes, "deep")
    if moor is not None:
        bx, by = mx(x - w * 0.5), water_y - h * 1.2
        rig.stroke(Path().move(bx, by).quad((bx + moor) / 2, max(by, QUAY - 4) + 18, moor, QUAY - 2), 2.0, ropes, "shadow")
    layer.place(rig, reach=0.5)
    # the mast, the boom and the furled sail (or a dinghy's oars)
    wood = tones("wood.light")
    if not mast:
        for k in range(2):
            oar = Path.capsule((mx(x - w * 0.3 + k * w * 0.1), water_y - h * 1.25), (mx(x + w * 0.3 + k * w * 0.1), water_y - h * 1.0 - 4 * k), 3, 3)
            p.fill(oar, wood, "base")
            p.edge(oar, wood, 0.9)
    else:
        _rigged(p, x, w, h, mh, mast_x, deck_y, top, bow, mx, sail, seed)
    # the hull
    whole = p.shape(hull_p) & p.above(p.everything(), water_y + 1)
    p.tone(whole, t, "base")
    bt = tones(band)
    band_m = whole & p.shape(m(Path().move(x - w * 0.6, water_y - h * 1.4).line(x + w * 0.6, water_y - h * 1.2)
                                .line(x + w * 0.6, water_y - h * 0.82).quad(x - w * 0.05, water_y - h * 0.46, x - w * 0.6, water_y - h * 0.95).close()))
    p.tone(band_m, bt, "base")
    p.tone(band_m - band_m.shift(0, 2), bt, "light")
    p.stroke(m(Path().move(x - w * 0.55, water_y - h * 0.98).quad(x - w * 0.05, water_y - h * 0.5, x + w * 0.5, water_y - h * 0.8)), 3.0, tones(stripe), "base", clip=whole)
    for k in range(1, 4):
        yy = water_y - h * 0.82 + k * h * 0.24
        p.stroke(m(Path().move(x - w * 0.55, yy - h * 0.25).quad(x - w * 0.05, yy + h * 0.1, x + w * 0.5, yy - h * 0.1)), 1.0, t, "deep", clip=whole, alpha=0.8)
    p.tone(p.below(whole, water_y - h * 0.3), t, "shadow")
    p.tone(whole - whole.shift(0, -3), t, "deep-shadow")
    p.stroke(sheer, 3.2, tones("wood.light"), "light")
    p.edge_mask(whole, t, 1.2)
    for k in range(2):
        fx = mx(x - w * 0.15 + k * w * 0.28)
        fy = water_y - h * 0.92 + 2
        p.stroke(Path.polyline([(fx, fy - 6), (fx, fy + 4)]), 1.2, tones("rope"), "deep")
        fm = p.fill(Path.ellipse(fx, fy + 10, 5, 8), tones("sack"), "base")
        p.tone(fm - fm.shift(-1.5, -1.5), tones("sack"), "shadow")
    # the ripple where she sits, and her reflection
    sea = tones("sea")
    refl = p.shape(hull_p.map(lambda xx, yy: (xx, 2 * water_y - yy))) & p.below(p.everything(), water_y + 1)
    p.tone(refl & p.above(p.everything(), min(water_y + h * 0.7, QUAY - 8)), t, "deep-shadow", alpha=0.32)
    for k in range(3):
        ry = water_y + 2 + k * 5
        p.stroke(Path.polyline([(x - w * (0.5 - 0.1 * k), ry), (x + w * (0.45 - 0.1 * k), ry)]), lambda u: 1.8 * math.sin(math.pi * u), sea, "highlight", alpha=0.45)
    layer.place(p)
    return p


def creels(layer, x, ground, n=3, seed=0):
    """Lobster pots stacked by a door: round-topped cages of withies over a wooden base, netting
    inside, a float tied to one."""
    p = Piece(x - 30 * n, ground - 110, x + 30 * n, ground + 10)
    wt = tones("wood.light")
    net = tones("#7f8f7a")
    spots = [(x - 26 * (n - 1) / 2 + 26 * i + (hash01(i, 1, seed + 941) - 0.5) * 4, ground, 1.0) for i in range(n)]
    if n >= 3:
        spots.append((x - 12, ground - 40, 0.9))
    for (cx, gy, s) in spots:
        w, hh = 46 * s, 40 * s
        dome = Path().move(cx - w / 2, gy).line(cx - w / 2, gy - hh * 0.55).quad(cx - w / 2, gy - hh, cx, gy - hh) \
            .quad(cx + w / 2, gy - hh, cx + w / 2, gy - hh * 0.55).line(cx + w / 2, gy).close()
        dm = p.fill(dome, net, "shadow")
        for k in range(-3, 4):
            p.stroke(Path.polyline([(cx + k * w * 0.13, gy - hh), (cx + k * w * 0.13 + 8, gy)]), 0.8, net, "deep", clip=dm)
            p.stroke(Path.polyline([(cx + k * w * 0.13, gy - hh), (cx + k * w * 0.13 - 8, gy)]), 0.8, net, "deep", clip=dm)
        for k in (-0.3, 0.0, 0.3):
            p.stroke(Path.smooth([(cx + k * w * 1.2, gy), (cx + k * w * 1.1, gy - hh * 0.75), (cx + k * w * 0.4, gy - hh + 1)]), 2.6, wt, "base", clip=dm)
        p.stroke(Path.polyline([(cx - w / 2, gy - hh * 0.5), (cx + w / 2, gy - hh * 0.5)]), 2.2, wt, "base", clip=dm)
        p.fill(Path.rect(cx - w / 2 - 2, gy - 7, w + 4, 7, r=1.5), tones("wood"), "base")
        p.tone(p.where(dm, lambda xx, yy, a=cx: xx > a + w * 0.2), net, "deep-shadow", alpha=0.5)
        p.edge(dome, wt, 1.1)
    fx, fy = x + 26 * (n - 1) / 2 + 18, ground - 18
    fm = p.fill(Path.circle(fx, fy, 8), tones("orange"), "base")
    p.tone(fm - fm.shift(-2, -2), tones("orange"), "shadow")
    p.ground_shadow(x - 28 * n / 2 - 10, x + 28 * n / 2 + 16, ground + 1, 6, 0.3)
    layer.place(p)
    return p


def _mesh(p, region, t, step=9.0, width=0.9, which="deep"):
    """A net's diamond mesh over a region of a piece."""
    bx0, by0 = p.ox, p.oy
    bx1, by1 = p.ox + p.w, p.oy + p.h
    lines = Path()
    span = (by1 - by0)
    k = bx0 - span
    while k < bx1 + span:
        lines.add(Path.polyline([(k, by0), (k + span, by1)]))
        lines.add(Path.polyline([(k + span, by0), (k, by1)]))
        k += step
    p.stroke(lines, width, t, which, clip=region)


def net_drape(x0, x1, y, depth=70, seed=0, colour="#7f927c"):
    """A fishing net hung on pegs along a wall from x0 to x1 at height y, sagging between them: a
    see-through mesh, its lower edge ragged, cork floats along it. A house extra."""

    def draw(p, house=None):
        t = tones(colour)
        top = Path().move(x0, y)
        pegs = 3
        bottom = []
        for i in range(pegs * 4 + 1):
            u = i / (pegs * 4)
            bottom.append((x0 + (x1 - x0) * u, y + depth * (0.55 + 0.45 * abs(math.sin(math.pi * u * pegs))) * (0.85 + 0.25 * hash01(i, 1, seed + 951))))
        for i in range(1, pegs + 1):
            top.quad(x0 + (x1 - x0) * (i - 0.5) / pegs, y + 18, x0 + (x1 - x0) * i / pegs, y)
        for q in reversed(bottom):
            top.line(*q)
        region = p.shape(top.close())
        p.tone(region, t, "shadow", alpha=0.45)
        _mesh(p, region, t, 8.0, 1.0, "deep")
        p.cast(region, 3, 4, p.covered() - region, tones("stone"), "shadow", alpha=0.25)
        for i in range(pegs + 1):
            px = x0 + (x1 - x0) * i / pegs
            p.fill(Path.circle(px, y, 3.2), tones("wood.dark"), "base")
        cork = tones("#d79a55")
        for i in range(1, pegs * 4, 2):
            cx, cy = bottom[i]
            cm = p.fill(Path.ellipse(cx, cy - 3, 6, 4.5), cork, "base")
            p.tone(cm - cm.shift(-1.2, -1.2), cork, "shadow")
            p.edge(Path.ellipse(cx, cy - 3, 6, 4.5), cork, 0.9)

    draw.box = (x0 - 12, y - 10, x1 + 12, y + depth * 1.2 + 16)
    return draw


def net_rack(layer, x0, x1, ground, h=190, seed=0, gull_at=None):
    """Tomas's kind of net rack: two posts and a cross beam with nets hung over it to dry, their
    floats along the bottom, a basket of tackle at one foot."""
    wood = tones("wood")
    frame = Piece(x0 - 14, ground - h - 50, x1 + 14, ground + 10)
    for px in (x0, x1):
        post = Path.rect(px - 6, ground - h, 12, h)
        pm = frame.fill(post, wood, "base")
        frame.tone(frame.where(pm, lambda xx, yy, a=px: xx > a + 2), wood, "shadow")
        frame.edge(post, wood, 1.1)
    beam = Path.rect(x0 - 12, ground - h - 4, x1 - x0 + 24, 12, r=2)
    bm = frame.fill(beam, wood, "base")
    frame.tone(bm - bm.shift(0, 2.5), wood, "light")
    frame.edge(beam, wood, 1.1)
    if gull_at is not None:
        gull(frame, gull_at, ground - h - 4, 1.0, -1, seed)
    frame.ground_shadow(x0 - 10, x1 + 10, ground + 1, 6, 0.3)
    layer.place(frame)
    netp = Piece(x0, ground - h - 4, x1, ground - 10, pad=10)
    t = tones("#7f927c")
    for j, (a, b, d) in enumerate(((0.04, 0.55, 0.8), (0.42, 0.97, 0.66))):
        nx0, nx1 = x0 + (x1 - x0) * a, x0 + (x1 - x0) * b
        bottom = []
        for i in range(9):
            u = i / 8
            bottom.append((nx0 + (nx1 - nx0) * u + (hash01(i, j, seed + 953) - 0.5) * 8,
                           ground - h + h * d * (0.86 + 0.18 * hash01(i, j + 3, seed + 953)) - 12 * math.sin(math.pi * u)))
        path = Path().move(nx0, ground - h + 2).line(nx1, ground - h + 2)
        for q in reversed(bottom):
            path.line(*q)
        region = netp.shape(path.close())
        netp.tone(region, t, "base" if j == 0 else "base-light", alpha=0.3)
        _mesh(netp, region, t, 8.0, 1.0, "deep")
        cork = tones("#d79a55")
        for q in bottom[1:-1:2]:
            cm = netp.fill(Path.ellipse(q[0], q[1] - 3, 6, 4.5), cork, "base")
            netp.tone(cm - cm.shift(-1.2, -1.2), cork, "shadow")
            netp.edge(Path.ellipse(q[0], q[1] - 3, 6, 4.5), cork, 0.9)
    layer.place(netp, reach=0.6)


# --------------------------------------------------------------------------- #
# The market: fish, stalls, racks, a crane, hoists
# --------------------------------------------------------------------------- #

FISH_COLOURS = ("fish", "#c9d3e2", "#e8a598", "#9fc0b4", "#b8b0d8")


def fish(p, cx, cy, L, angle=0.0, colour="fish", seed=0):
    """A fish (into a Piece), L long, laid at an angle: a body darker along its back and pale at its
    belly, a forked tail, a fin, and an eye with a glint."""
    t = tones(colour)
    ca, sa = math.cos(math.radians(angle)), math.sin(math.radians(angle))

    def at(u, v):
        return (cx + u * ca - v * sa, cy + u * sa + v * ca)
    body = Path.ellipse(cx, cy, L * 0.42, L * 0.15, rot=angle)
    tail = Path.polygon([at(-L * 0.36, 0), at(-L * 0.56, -L * 0.15), at(-L * 0.5, 0), at(-L * 0.56, L * 0.15)])
    p.fill(tail, t, "shadow")
    bm = p.fill(body, t, "base")
    back = p.shape(Path.ellipse(*at(0, -L * 0.07), L * 0.4, L * 0.08, rot=angle))
    p.tone(bm & back, t, "shadow")
    p.tone(bm & p.shape(Path.ellipse(*at(-L * 0.02, L * 0.08), L * 0.3, L * 0.05, rot=angle)), t, "light")
    p.fill(Path.polygon([at(-L * 0.05, -L * 0.13), at(L * 0.08, -L * 0.2), at(L * 0.12, -L * 0.12)]), t, "shadow")
    ex, ey = at(L * 0.3, -L * 0.03)
    p.fill(Path.circle(ex, ey, max(1.2, L * 0.035)), tones("#26222e"), "base")
    p.fill(Path.circle(ex - 0.5, ey - 0.5, max(0.5, L * 0.012)), tones("#ffffff"), "base")
    p.edge(body, t, 0.9)
    return bm


def _fish_icon(p, cx, cy, s):
    fish(p, cx, cy, s * 1.05, -10, "#9fc6e0")


def _anchor_icon(p, cx, cy, s):
    g = tones("gold")
    p.stroke(Path.polyline([(cx, cy - s * 0.32), (cx, cy + s * 0.34)]), s * 0.1, g, "base")
    p.stroke(Path.polyline([(cx - s * 0.2, cy - s * 0.18), (cx + s * 0.2, cy - s * 0.18)]), s * 0.08, g, "base")
    p.stroke(Path().arc(cx, cy + s * 0.02, s * 0.34, 20, 160), s * 0.09, g, "base")
    p.stroke(Path.circle(cx, cy - s * 0.4, s * 0.08), s * 0.05, g, "base")
    for side in (-1, 1):
        a = math.radians(90 - side * 70)
        tx, ty = cx + math.cos(a) * s * 0.34, cy + s * 0.02 + math.sin(a) * s * 0.34
        p.fill(Path.polygon([(tx - side * s * 0.02, ty - s * 0.14), (tx + side * s * 0.1, ty + s * 0.02), (tx - side * s * 0.06, ty + s * 0.04)]), g, "base")


ICONS = {"fish": _fish_icon, "anchor": _anchor_icon}


def sign(x, y, icon="fish", board="wood.dark", side=1, w=58, h=46, shape="shield", trim="gold"):
    """A hanging sign on a wrought-iron bracket, as paint.hanging_sign but with the harbour's icons
    (a fish, an anchor) or paint's own. A house extra."""
    if icon not in ICONS:
        return P.hanging_sign(x, y, icon, board, side, w, h, shape, trim)

    def draw(p, house=None):
        iron = tones("iron")
        arm = 70 * side
        p.stroke(Path.polyline([(x, y), (x + arm, y)]), 3.6, iron, "base")
        p.stroke(Path.smooth([(x, y + 22), (x + arm * 0.25, y + 18), (x + arm * 0.45, y + 6), (x + arm * 0.5, y)]), 2.4, iron, "base")
        p.stroke(Path.circle(x + arm * 0.22, y + 9, 5), 1.8, iron, "base")
        p.fill(Path.rect(x - 4 * side if side > 0 else x, y - 6, 4, 30), iron, "shadow")
        bx = x + arm * 0.62
        for dx in (-w * 0.32, w * 0.32):
            p.stroke(Path.polyline([(bx + dx, y), (bx + dx, y + 10)]), 1.4, iron, "deep")
        top = y + 10
        if shape == "oval":
            bp = Path.ellipse(bx, top + h / 2, w / 2, h / 2)
        elif shape == "shield":
            bp = Path().move(bx - w / 2, top).line(bx + w / 2, top).line(bx + w / 2, top + h * 0.55) \
                .quad(bx + w / 2, top + h * 0.9, bx, top + h).quad(bx - w / 2, top + h * 0.9, bx - w / 2, top + h * 0.55).close()
        else:
            bp = Path.rect(bx - w / 2, top, w, h, r=6)
        bt = tones(board)
        bm = p.fill(bp, bt, "base")
        p.tone(bm - bm.shift(2.5, 2.5), bt, "light")
        p.tone(p.where(bm, lambda xx, yy: xx > bx + w * 0.3), bt, "shadow")
        ring = bm - bm.inset(3.2)
        p.tone(ring, tones(trim), "base")
        p.tone(ring - ring.shift(1, 1), tones(trim), "light")
        ICONS[icon](p, bx, top + h * 0.46, min(w, h) * 0.62)
        p.edge(bp, bt, 1.4)

    draw.box = (min(x, x + 70 * side) - w / 2 - 10, y - 10, max(x, x + 70 * side) + w / 2 + 10, y + h + 20)
    return draw


def fish_stall(layer, x, ground, w=180, colours=("cloth.pink", "cloth.cream"), seed=0, hanging=True, crab=True):
    """A fishmonger's stall: two posts under a striped awning, fish hung by their tails from a rail
    beneath it, a sloping tray of crushed ice with the catch laid out on it (and a crab), a planked
    front with a painted fish on a board, and a scale on a post."""
    p = Piece(x - w / 2 - 50, ground - 240, x + w / 2 + 50, ground + 12)
    wood = tones("wood")
    top_y = ground - 205
    for px in (x - w / 2 + 6, x + w / 2 - 6):
        post = Path.rect(px - 4, top_y + 10, 8, ground - top_y - 10)
        pm = p.fill(post, wood, "base")
        p.tone(p.where(pm, lambda xx, yy, a=px: xx > a + 1), wood, "shadow")
        p.edge(post, wood, 1.1)
    back = Path.rect(x - w / 2 + 10, top_y + 26, w - 20, ground - top_y - 80)
    bt = tones(colours[1])
    bm = p.fill(back, bt, "shadow")
    for k in range(5):
        fx = x - w / 2 + 18 + k * (w - 36) / 4
        p.stroke(Path.polyline([(fx, top_y + 28), (fx + 2, ground - 56)]), (2.0, 0.6), bt, "deep-shadow", clip=bm)
    if hanging:
        rail_y = top_y + 50
        p.fill(Path.rect(x - w / 2 + 8, rail_y - 2, w - 16, 4), wood, "deep")
        for i in range(int((w - 40) / 26)):
            hx = x - w / 2 + 28 + i * 26
            p.stroke(Path.polyline([(hx, rail_y), (hx, rail_y + 8)]), 1.0, tones("iron"), "base")
            fish(p, hx, rail_y + 8 + 22, 44 + 6 * hash01(i, 1, seed + 961), 90 - 6 + 12 * hash01(i, 2, seed + 961),
                 FISH_COLOURS[int(hash01(i, 3, seed + 961) * len(FISH_COLOURS))])
    P.awning(x - w / 2 - 4, x + w / 2 + 4, top_y + 6, depth=30, colours=colours, valance=9)(p)
    counter_y = ground - 72
    tray = Path.polygon([(x - w / 2 - 4, counter_y - 4), (x + w / 2 + 4, counter_y - 4), (x + w / 2 + 8, counter_y + 10), (x - w / 2 - 8, counter_y + 10)])
    p.fill(tray, wood, "light")
    ice = tones("#e3f0f7")
    heap = p.nothing()
    for i in range(int(w / 10)):
        heap = heap | p.shape(Path.circle(x - w / 2 + 6 + i * 10, counter_y - 6 - 3 * hash01(i, 4, seed + 961), 7))
    heap = heap - p.below(p.everything(), counter_y + 2)
    p.tone(heap, ice, "base")
    p.tone(heap - heap.shift(-1.6, -2), ice, "shadow")
    for i in range(int(w / 9)):
        p.fill(Path.circle(x - w / 2 + 6 + i * 9 + 3, counter_y - 9 - 2 * hash01(i, 5, seed + 961), 1.4), ice, "highlight")
    p.edge_mask(heap, ice, 0.9, "shadow")
    for i in range(int((w - 30) / 30)):
        fx = x - w / 2 + 30 + i * 30
        fish(p, fx, counter_y - 12 - 2 * hash01(i, 6, seed + 961), 40, -22 + 10 * hash01(i, 7, seed + 961),
             FISH_COLOURS[int(hash01(i, 8, seed + 961) * len(FISH_COLOURS))])
    if crab:
        cxr, cyr = x + w / 2 - 22, counter_y - 12
        ct = tones("#e2603f")
        for side in (-1, 1):
            for k in range(3):
                p.stroke(Path.polyline([(cxr + side * 6, cyr + 2 + k * 2), (cxr + side * 15, cyr + 6 + k * 3)]), 1.6, ct, "shadow")
            p.fill(Path.circle(cxr + side * 13, cyr - 6, 4), ct, "base")
        cm = p.fill(Path.ellipse(cxr, cyr, 10, 7), ct, "base")
        p.tone(cm - cm.shift(-2, -2), ct, "shadow")
        p.fill(Path.circle(cxr - 3, cyr - 7, 1.4), tones("#26222e"), "base")
        p.fill(Path.circle(cxr + 3, cyr - 7, 1.4), tones("#26222e"), "base")
    front = Path.rect(x - w / 2, counter_y + 10, w, ground - counter_y - 18)
    fm = p.fill(front, wood, "base")
    for k in range(1, 4):
        p.stroke(Path.polyline([(x - w / 2, counter_y + 10 + k * (ground - counter_y - 18) / 4), (x + w / 2, counter_y + 10 + k * (ground - counter_y - 18) / 4)]), 1.0, wood, "deep", clip=fm)
    p.tone(p.above(fm, counter_y + 16), wood, "deep")
    board = Path.rect(x - 34, counter_y + 22, 68, 30, r=4)
    p.fill(board, tones("cloth.cream"), "base")
    p.edge(board, tones("wood.dark"), 1.6, "base")
    fish(p, x, counter_y + 37, 46, 0, "#7fb0d8")
    p.edge(front, wood, 1.2)
    p.ground_shadow(x - w / 2 - 10, x + w / 2 + 10, ground + 2, 9, 0.35)
    layer.place(p)
    return p


def fish_rack(layer, x0, x1, ground, h=170, seed=0):
    """A rack of fish drying in the wind: posts and two rails, pairs of fish hung by their tails."""
    p = Piece(x0 - 12, ground - h - 30, x1 + 12, ground + 10)
    wood = tones("wood.light")
    n = max(2, int((x1 - x0) / 90) + 1)
    for i in range(n):
        px = x0 + (x1 - x0) * i / (n - 1)
        post = Path.rect(px - 5, ground - h, 10, h)
        pm = p.fill(post, wood, "base")
        p.tone(p.where(pm, lambda xx, yy, a=px: xx > a + 1.5), wood, "shadow")
        p.edge(post, wood, 1.0)
    for j, ry in enumerate((ground - h + 8, ground - h * 0.52)):
        rail = Path.rect(x0 - 8, ry - 3, x1 - x0 + 16, 6, r=2)
        p.fill(rail, wood, "base")
        p.edge(rail, wood, 1.0)
        k = 0
        fx = x0 + 14 + 8 * j
        while fx < x1 - 10:
            p.stroke(Path.polyline([(fx, ry), (fx, ry + 6)]), 1.0, tones("rope"), "deep")
            fish(p, fx, ry + 26, 40, 92 + 8 * (hash01(k, j, seed + 971) - 0.5), FISH_COLOURS[int(hash01(k, j + 2, seed + 971) * 3)])
            fx += 22 + 6 * hash01(k, j + 4, seed + 971)
            k += 1
    p.ground_shadow(x0 - 6, x1 + 6, ground + 1, 6, 0.3)
    layer.place(p)
    return p


def hung_crate(p, x, top, w=40, h=34, seed=0):
    """A crate hanging on a hook (into a Piece): paint.crate without its contact shadow."""
    P.crate(None, x, top + h, w, h, seed, piece=p)
    p.shadows.pop()


def crane(layer, x, ground, h=380, reach=230, side=-1, seed=0, load=2):
    """A harbour crane on the quay's edge: a timber post on a stone plinth, a jib slanting up and
    out over the water with its brace, a sheave at its tip, the rope down to a cargo net of crates
    (`load` of them), and a spoked winch wheel at the foot."""
    wood = tones("wood")
    dark = tones("wood.dark")
    iron = tones("iron")
    tip = (x + side * reach, ground - h * 0.98)
    lift = ground - h * 0.42
    rig = Piece(min(x, tip[0]) - 40, tip[1] - 20, max(x, tip[0]) + 40, ground, pad=6)
    rig.stroke(Path.polyline([(x, ground - h), (tip[0], tip[1])]), 1.6, tones("rope"), "deep")
    rig.stroke(Path.polyline([(tip[0], tip[1] + 6), (tip[0], lift - 22)]), 1.8, tones("rope"), "shadow")
    rig.stroke(Path.polyline([(tip[0] - side * 4, tip[1] + 6), (x + side * 12, ground - 62)]), 1.6, tones("rope"), "shadow")
    layer.place(rig, reach=0.5)
    p = Piece(min(x, tip[0]) - 60, tip[1] - 20, max(x, tip[0]) + 60, ground + 10)
    st = tones("stone.grey")
    plinth = Path.rect(x - 40, ground - 26, 80, 26, r=2)
    pm = p.fill(plinth, st, "base")
    p.tone(pm - pm.shift(0, 2.5), st, "light")
    p.tone(p.where(pm, lambda xx, yy: xx > x + 20), st, "shadow")
    p.edge(plinth, st, 1.2, "shadow")
    post = Path.rect(x - 10, ground - h, 20, h - 24)
    om = p.fill(post, wood, "base")
    p.tone(p.where(om, lambda xx, yy: xx > x + 3), wood, "shadow")
    p.tone(p.where(om, lambda xx, yy: xx < x - 6), wood, "light")
    p.edge(post, wood, 1.2)
    cap = Path.rect(x - 14, ground - h - 8, 28, 10, r=2)
    p.fill(cap, dark, "base")
    p.edge(cap, dark, 1.0)
    foot = (x + side * 8, ground - h * 0.3)
    jib = Path.capsule(foot, tip, 8, 6)
    jm = p.fill(jib, wood, "base")
    p.tone(jm - jm.shift(1.6, 2.4), wood, "light")
    p.tone(jm - jm.shift(-1.4, -2.0), wood, "shadow")
    p.edge(jib, wood, 1.1)
    mid = ((foot[0] + tip[0]) / 2, (foot[1] + tip[1]) / 2)
    brace = Path.capsule((x + side * 6, ground - h * 0.78), mid, 5, 4)
    p.fill(brace, wood, "shadow")
    p.edge(brace, wood, 1.0)
    for (bx, by) in (foot, mid, (x, ground - h * 0.78)):
        style_bolt = Path.circle(bx, by, 3)
        p.fill(style_bolt, iron, "base")
    sheave = Path.circle(tip[0], tip[1] + 2, 9)
    p.fill(sheave, iron, "base")
    p.fill(Path.circle(tip[0] - 2, tip[1], 3), iron, "light")
    p.edge(sheave, iron, 1.0)
    # the winch
    wx, wy, wr = x - side * 30, ground - 64, 32
    ring = p.shape(Path.circle(wx, wy, wr)) - p.shape(Path.circle(wx, wy, wr - 6))
    p.tone(ring, dark, "base")
    p.tone(ring - ring.shift(1.5, 1.5), dark, "light")
    for k in range(8):
        a = math.radians(k * 45 + 10)
        p.stroke(Path.polyline([(wx, wy), (wx + math.cos(a) * (wr - 4), wy + math.sin(a) * (wr - 4))]), 3.0, dark, "base")
    p.fill(Path.circle(wx, wy, 7), iron, "base")
    p.edge_mask(ring, dark, 1.0)
    # the load: a hook and a net of crates
    hook = Path.smooth([(tip[0], lift - 24), (tip[0], lift - 14), (tip[0] + 5, lift - 9), (tip[0] + 1, lift - 5)])
    p.stroke(hook, 2.6, iron, "base")
    for i in range(load):
        hung_crate(p, tip[0] - 22 * (load - 1) + 44 * i, lift - 2 + 4 * (i % 2), 42, 36, seed + i)
    net = p.shape(Path.polygon([(tip[0], lift - 10), (tip[0] - 24 * load - 6, lift + 40), (tip[0] + 24 * load + 6, lift + 40)])) \
        & p.above(p.everything(), lift + 42)
    _mesh(p, net & p.covered(), tones("rope"), 10.0, 1.2, "deep")
    p.stroke(Path.polyline([(tip[0], lift - 10), (tip[0] - 24 * load, lift + 4)]), 1.6, tones("rope"), "shadow")
    p.stroke(Path.polyline([(tip[0], lift - 10), (tip[0] + 24 * load, lift + 4)]), 1.6, tones("rope"), "shadow")
    p.ground_shadow(x - 50, x + 50, ground + 2, 7, 0.35)
    layer.place(p)
    return p


def loft(x, top, w=70, h=86, beam_y=None, drop=150, side=1):
    """A warehouse's loft: a pair of planked doors high in the gable, the hoist beam's end over them
    with its sheave, and the rope down to a crate hanging beside them. A house extra."""
    beam_y = top - 30 if beam_y is None else beam_y

    def draw(p, house=None):
        dark = tones("wood.dark")
        wood = tones("wood")
        iron = tones("iron")
        frame = Path.rect(x - w / 2 - 7, top - 7, w + 14, h + 10)
        fm = p.fill(frame, dark, "base")
        p.cast(fm, 3, 4, p.covered() - fm, tones("plaster"), "shadow", alpha=0.7)
        p.edge(frame, dark, 1.1)
        leaf = Path.rect(x - w / 2, top, w, h)
        lm = p.fill(leaf, wood, "base")
        for k in range(1, 6):
            p.stroke(Path.polyline([(x - w / 2 + w * k / 6, top), (x - w / 2 + w * k / 6, top + h)]), 1.0, wood, "deep", clip=lm)
        p.stroke(Path.polyline([(x, top), (x, top + h)]), 2.2, wood, "line", clip=lm)
        for yy in (top + h * 0.25, top + h * 0.7):
            p.fill(Path.rect(x - w / 2, yy, w, 4), iron, "base", clip=lm)
        p.tone(lm - lm.shift(4, 4), wood, "deep", alpha=0.8)
        end = Path.rect(x - 12, beam_y - 12, 24, 24, r=2)
        em = p.fill(end, dark, "base")
        p.tone(em - em.shift(2, 2), dark, "light")
        for k in range(3):
            p.stroke(Path.circle(x, beam_y, 3 + k * 3), 0.9, dark, "deep", clip=em)
        p.cast(em, 4, 5, p.covered() - em, tones("plaster"), "shadow", alpha=0.7)
        p.edge(end, dark, 1.2)
        p.stroke(Path.polyline([(x, beam_y + 12), (x, beam_y + 20)]), 2.0, iron, "base")
        p.fill(Path.circle(x, beam_y + 26, 7), iron, "base")
        rx = x + 7 * side
        cx = x + side * (w / 2 + 34)
        p.stroke(Path().move(rx, beam_y + 26).quad(rx + side * 10, beam_y + 30, cx, beam_y + 40).line(cx, beam_y + drop - 8), 1.6, tones("rope"), "shadow")
        hung_crate(p, cx, beam_y + drop, 40, 34, 3)

    draw.box = (x - w / 2 - abs(w) - 60, beam_y - 20, x + w / 2 + 90, beam_y + drop + 50)
    return draw


def clock(x, y, r=26):
    """A clock on a building's front, round, cream-faced in a gold rim, hands at ten past ten. A house extra."""

    def draw(p, house=None):
        g = tones("gold")
        face = tones("cloth.cream")
        rim = Path.circle(x, y, r + 5)
        rm = p.fill(rim, g, "base")
        p.tone(rm - rm.shift(1.5, 1.5), g, "light")
        p.cast(rm, 3, 4, p.covered() - rm, tones("stone"), "shadow")
        p.fill(Path.circle(x, y, r), face, "base")
        p.tone(p.shape(Path.circle(x, y, r)) - p.shape(Path.circle(x - 3, y - 3, r)), face, "shadow")
        for k in range(12):
            a = math.radians(k * 30)
            p.fill(Path.circle(x + math.cos(a) * r * 0.8, y + math.sin(a) * r * 0.8, 1.6), tones("wood.dark"), "base")
        p.stroke(Path.polyline([(x, y), (x - r * 0.4, y - r * 0.32)]), 2.4, tones("wood.dark"), "base")
        p.stroke(Path.polyline([(x, y), (x + r * 0.55, y - r * 0.42)]), 1.8, tones("wood.dark"), "base")
        p.fill(Path.circle(x, y, 2.4), g, "base")
        p.edge(rim, g, 1.2)

    draw.box = (x - r - 14, y - r - 14, x + r + 18, y + r + 18)
    return draw


def flagpole(x, base, h=120, colour="cloth.navy", emblem="sun", side=1):
    """A flagpole standing up from a roof at (x, base) with a flag flying from it. A house extra."""

    def draw(p, house=None):
        wood = tones("wood.light")
        p.fill(Path.rect(x - 2.5, base - h, 5, h), wood, "base")
        P._finial(p, x, base - h, tones("gold"))
        t = tones(colour)
        fy = base - h + 6
        fw, fh = 80, 50
        flag = Path().move(x, fy).quad(x + side * fw * 0.5, fy - 10, x + side * fw, fy + 4) \
            .line(x + side * fw * 0.86, fy + fh * 0.5 + 2).line(x + side * fw, fy + fh) \
            .quad(x + side * fw * 0.5, fy + fh - 12, x, fy + fh).close()
        m = p.fill(flag, t, "base")
        p.tone(m & p.shape(Path.ellipse(x + side * fw * 0.6, fy + fh * 0.55, fw * 0.16, fh * 0.6)), t, "shadow")
        p.tone(m & p.shape(Path.ellipse(x + side * fw * 0.25, fy + fh * 0.4, fw * 0.1, fh * 0.5)), t, "light")
        P._icon(p, emblem, x + side * fw * 0.4, fy + fh * 0.48, 24)
        p.edge(flag, t, 1.2)

    draw.box = (x - 90, base - h - 30, x + 90, base + 10)
    return draw


def ice_hatch(x0, x1, top, bottom):
    """The ice store's serving hatch, as a house extra: an opening into the cold dark, blocks of ice
    stacked inside lit pale blue, a counter sill, and a board over it with a fish."""

    def draw(p, house=None):
        inside = tones("#2d3a5c")
        op = Path.rect(x0, top, x1 - x0, bottom - top)
        st = tones("stone.grey")
        sur = Path.rect(x0 - 10, top - 10, x1 - x0 + 20, bottom - top + 14)
        sm = p.fill(sur, st, "base-light")
        p.cast(sm, 3, 3.5, p.covered() - sm, tones("stone.grey"), "shadow")
        p.edge(sur, st, 1.2, "terminator")
        om = p.fill(op, inside, "base")
        ice = tones("#bfe3f2")
        bw, bh = 34, 26
        for row in range(3):
            n = int((x1 - x0 - 10) / (bw + 3))
            for i in range(n - (row % 2)):
                bx = x0 + 6 + i * (bw + 3) + (bw / 2 if row % 2 else 0)
                by = bottom - 10 - (row + 1) * (bh + 2)
                if by < top + 8:
                    continue
                blk = Path.rect(bx, by, bw, bh, r=2)
                m = p.fill(blk, ice, "base", clip=om)
                p.tone(m - m.shift(-2, -2), ice, "shadow")
                p.tone(m - m.shift(2, 2), ice, "highlight")
        p.tone(om - om.shift(6, 6), inside, "deep")
        sill = Path.rect(x0 - 12, bottom - 4, x1 - x0 + 24, 10, r=2)
        slm = p.fill(sill, tones("wood"), "base")
        p.tone(slm - slm.shift(0, 2), tones("wood"), "light")
        p.edge(sill, tones("wood"), 1.1)
        cx = (x0 + x1) / 2
        board = Path.rect(cx - 50, top - 46, 100, 30, r=4)
        p.fill(board, tones("cloth.blue"), "base")
        p.edge(board, tones("wood.dark"), 1.6, "base")
        fish(p, cx, top - 31, 48, 0, "#d8eef8")

    draw.box = (x0 - 20, top - 56, x1 + 20, bottom + 14)
    return draw


def boarded(x, top, w, h):
    """Planks nailed across a window (the Rusty Anvil's upstairs). A house extra."""

    def draw(p, house=None):
        t = tones("wood.light")
        for k, (a, b) in enumerate(((0.18, 0.3), (0.55, 0.62), (0.2, 0.86))):
            plank = Path.polygon([(x - w * 0.62, top + h * a), (x + w * 0.62, top + h * b), (x + w * 0.62, top + h * b + 11), (x - w * 0.62, top + h * a + 11)])
            m = p.fill(plank, t, "base" if k != 1 else "terminator")
            p.tone(m - m.shift(0, 2), t, "light")
            p.cast(m, 2, 3, p.covered() - m, tones("brick"), "shadow", alpha=0.7)
            for nx in (x - w * 0.5, x + w * 0.5):
                p.fill(Path.circle(nx, top + h * (a + (b - a) * (nx - x + w * 0.62) / (w * 1.24)) + 5.5, 1.3), tones("iron"), "base")
            p.edge(plank, t, 1.0)

    draw.box = (x - w - 10, top - 10, x + w + 10, top + h + 20)
    return draw


def bollard(layer, x, ground, s=1.0, dark=0.5, seed=0):
    """An iron bollard in the foreground, dark and crisp, a rope's loop thrown over it."""
    p = Piece(x - 70 * s, ground - 150 * s, x + 90 * s, ground + 10)
    t = P.haze(tones("iron"), "#140e1c", dark)
    rope = P.haze(tones("rope"), "#140e1c", dark)
    body = Path().move(x - 30 * s, ground).line(x - 26 * s, ground - 70 * s).quad(x - 40 * s, ground - 92 * s, x - 34 * s, ground - 104 * s) \
        .quad(x, ground - 118 * s, x + 34 * s, ground - 104 * s).quad(x + 40 * s, ground - 92 * s, x + 26 * s, ground - 70 * s) \
        .line(x + 30 * s, ground).close()
    m = p.fill(body, t, "base")
    p.tone(p.where(m, lambda xx, yy: xx > x + 10 * s), t, "shadow")
    p.tone(p.where(m, lambda xx, yy: xx < x - 18 * s), t, "light")
    band = Path.rect(x - 32 * s, ground - 60 * s, 64 * s, 9 * s, r=3)
    p.fill(band, t, "base-light")
    loop = Path().move(x - 28 * s, ground - 78 * s).quad(x, ground - 64 * s, x + 28 * s, ground - 80 * s)
    p.stroke(loop, 9 * s, rope, "base")
    p.stroke(Path().move(x + 26 * s, ground - 78 * s).quad(x + 60 * s, ground - 50 * s, x + 84 * s, ground + 4), 9 * s, rope, "base")
    p.edge(body, t, 1.4)
    layer.place(p, reach=2.6)


def rope_coil(p, x, ground, r=26, colour="rope", dark=0.0):
    """A coil of rope lying on the ground (into a Piece): rings seen a little from above; `dark`
    pulls it toward a silhouette, for the foreground."""
    t = tones(colour) if dark <= 0 else P.haze(tones(colour), "#140e1c", dark)
    outer = Path.ellipse(x, ground - r * 0.35, r, r * 0.4)
    m = p.fill(outer, t, "base")
    for k in range(1, 4):
        p.stroke(Path.ellipse(x, ground - r * 0.35 - k * 1.5, r * (1 - k * 0.2), r * 0.4 * (1 - k * 0.2)), 1.6, t, "deep", clip=m)
    p.tone(m - m.shift(-2, -2), t, "shadow")
    p.fill(Path.ellipse(x, ground - r * 0.4, r * 0.25, r * 0.1), t, "deep-shadow")
    p.edge(outer, t, 1.1)


# --------------------------------------------------------------------------- #
# The Wager's deck
# --------------------------------------------------------------------------- #

NIGHT = "#2b3166"
"""What the deck's wood is cooled toward under the moon."""


def moonlit(name, k=0.22):
    return P.haze(tones(name), NIGHT, k)


def sheer_y(x, ground, width, low=520, bow=380, stern=430):
    """The bulwark's top along the deck: low amidships, sweeping up to the bow (left) and the stern."""
    u = np.asarray(x, np.float64) / max(1.0, width)
    lift_b = np.maximum(0.0, 1.0 - u / 0.22) ** 2
    lift_s = np.maximum(0.0, (u - 0.82) / 0.18) ** 2
    y = low - (low - bow) * lift_b - (low - stern) * lift_s
    return float(y) if np.ndim(y) == 0 else y


def bulwark(layer, x0, x1, ground, width, seed=0):
    """The ship's far bulwark along the deck's back edge: planks between stanchions, its cap rail
    lit along the top, a band of navy and gold, scuppers along the foot, a pin rail with belaying
    pins and coiled falls hanging from them, and its sheer rising to the bow and the stern."""
    wood = moonlit("wood")
    cap_t = moonlit("wood.light")
    navy = moonlit("cloth.navy", 0.1)
    gold = moonlit("gold", 0.12)
    for a in range(int(x0), int(x1), 800):
        b = min(int(x1), a + 800)
        p = Piece(a, 300, b, ground + 8, pad=6)
        pts = [(x, sheer_y(x, ground, width)) for x in range(a - 20, b + 21, 20)]
        top = Path.polyline(pts)
        body = Path.polyline(pts).line(b + 20, ground).line(a - 20, ground).close()
        m = p.fill(body, wood, "base")
        for k in range(1, 7):
            plank = Path.polyline([(x, y + (ground - y) * k / 7) for x, y in pts])
            p.stroke(plank, 1.1, wood, "deep", clip=m)
        p.tone(p.where(m, lambda xx, yy: yy > ground - 40), wood, "shadow")
        band = Path.polyline([(x, y + 22) for x, y in pts]).line(b + 20, pts[-1][1] + 40).line(a - 20, pts[0][1] + 40)
        band = Path.polygon([(x, y + 22) for x, y in pts] + [(x, y + 40) for x, y in reversed(pts)])
        bm = p.fill(band, navy, "base", clip=m)
        p.stroke(Path.polyline([(x, y + 22) for x, y in pts]), 2.2, gold, "base", clip=m)
        p.stroke(Path.polyline([(x, y + 40) for x, y in pts]), 2.2, gold, "base", clip=m)
        del bm
        for sx in range(a - a % 160 + 80, b, 160):
            if sx < a:
                continue
            sy = sheer_y(sx, ground, width)
            st = Path.rect(sx - 7, sy, 14, ground - sy)
            sm = p.fill(st, wood, "base-light")
            p.tone(p.where(sm, lambda xx, yy, c=sx: xx > c + 2), wood, "shadow")
            p.edge(st, wood, 1.0)
            if (sx // 160) % 2 == 0:
                sc = Path.rect(sx + 30, ground - 14, 26, 9, r=3)
                p.fill(sc, wood, "deep")
        cap = Path.polygon([(x, y - 6) for x, y in pts] + [(x, y + 5) for x, y in reversed(pts)])
        cm = p.fill(cap, cap_t, "base")
        p.tone(p.where(cm, lambda xx, yy: yy < sheer_y(xx, ground, width) - 1), cap_t, "light")
        p.edge(cap, cap_t, 1.1)
        p.stroke(top, 1.0, cap_t, "highlight", alpha=0.6)
        layer.place(p)


def pin_rail(layer, x0, x1, y, seed=0):
    """A pin rail along the bulwark: a rail with belaying pins and the falls coiled and hung on them."""
    p = Piece(x0 - 10, y - 20, x1 + 10, y + 90)
    wood = moonlit("wood.light")
    rope = moonlit("rope", 0.08)
    rail = Path.rect(x0, y, x1 - x0, 10, r=2)
    rm = p.fill(rail, wood, "base")
    p.tone(rm - rm.shift(0, 2), wood, "light")
    p.edge(rail, wood, 1.0)
    n = int((x1 - x0) / 34)
    for i in range(n):
        px = x0 + 17 + i * 34
        pin = Path.rect(px - 2.5, y - 12, 5, 28, r=2)
        p.fill(pin, wood, "base")
        p.edge(pin, wood, 0.9)
        if hash01(i, 1, seed + 981) < 0.7:
            L = 34 + 26 * hash01(i, 2, seed + 981)
            coil = Path().move(px - 9, y + 12).quad(px - 14, y + 12 + L, px, y + 16 + L).quad(px + 14, y + 12 + L, px + 9, y + 12).close()
            cm = p.fill(coil, rope, "base")
            p.tone(cm - p.shape(coil.translate(-3, -3)), rope, "shadow")
            for k in range(3):
                p.stroke(coil.translate(0, -3 - k * 4).scale(0.9 - k * 0.12, 0.9 - k * 0.12, px, y + 12), 1.0, rope, "deep", clip=cm)
            p.edge(coil, rope, 0.9)
    layer.place(p)


def rail_lamp(layer, x, y, s=1.4):
    """A lamp on a bracket on the bulwark, lit."""
    p = Piece(x - 40, y - 30, x + 40, y + 60)
    iron = moonlit("iron")
    p.stroke(Path.smooth([(x - 14, y + 20), (x - 8, y), (x, y - 8), (x + 2, y - 6)]), 2.6, iron, "base")
    p.fill(Path.rect(x - 16, y + 10, 5, 30), iron, "shadow")
    P.lantern_body(p, x + 2, y - 4, s, True)
    p.glow(x + 2, y + 18, 140, "#ffb860", 0.25)
    layer.place(p)


def mast(layer, x, ground, h=900, yards=(0.32, 0.58), sails="furled", top_mark=True, seed=0):
    """A mast standing from the deck out of the top of the picture: a round spar lit on its left
    with iron bands, its yards across with the sails furled on them and gaskets round, a fighting top
    (a crow's nest) if top_mark, and a pennant."""
    wood = moonlit("wood.light")
    canvas = moonlit("canvas", 0.18)
    iron = moonlit("iron")
    top = ground - h
    p = Piece(x - 260, max(-20, top), x + 260, ground + 10)
    body = Path.polygon([(x - 13, ground), (x - 9, top), (x + 9, top), (x + 13, ground)])
    m = p.fill(body, wood, "base")
    p.tone(p.where(m, lambda xx, yy: xx > x + 3), wood, "shadow")
    p.tone(p.where(m, lambda xx, yy: xx < x - 7), wood, "light")
    for k in range(6):
        by = ground - 60 - k * 120
        if by < top:
            break
        p.fill(Path.rect(x - 14, by, 28, 6, r=2), iron, "base", clip=None)
    p.edge(body, wood, 1.2)
    for j, f in enumerate(yards):
        yy = ground - h * f
        half = 200 - 46 * j
        yard = Path.rect(x - half, yy - 5, half * 2, 10, r=4)
        ym = p.fill(yard, wood, "base")
        p.tone(ym - ym.shift(0, 2), wood, "light")
        p.edge(yard, wood, 1.0)
        lump = []
        n = 10
        for i in range(n + 1):
            u = i / n
            lump.append((x - half * 0.94 + half * 1.88 * u, yy + 4 + 12 * math.sin(math.pi * u) ** 0.6 + 4 * hash01(i, j, seed + 991)))
        for i in range(n, -1, -1):
            lump.append((x - half * 0.94 + half * 1.88 * i / n, yy + 2))
        sm = p.fill(Path.smooth(lump, closed=True), canvas, "base")
        p.tone(sm - sm.shift(-2, -3), canvas, "shadow")
        p.tone(sm & p.above(p.everything(), yy + 7), canvas, "light", alpha=0.6)
        for i in range(1, 7):
            gx = x - half * 0.94 + half * 1.88 * i / 7
            p.stroke(Path.polyline([(gx - 2, yy + 1), (gx + 2, yy + 20)]), 2.0, moonlit("rope"), "deep", clip=sm)
        p.edge_mask(sm, canvas, 1.0)
    if top_mark:
        ty = ground - h * 0.8
        nest = Path.polygon([(x - 34, ty), (x + 34, ty), (x + 26, ty + 26), (x - 26, ty + 26)])
        nm = p.fill(nest, wood, "base")
        p.tone(p.where(nm, lambda xx, yy: xx > x + 10), wood, "shadow")
        for k in range(1, 3):
            p.stroke(Path.polyline([(x - 34 + k * 2, ty + k * 8), (x + 34 - k * 2, ty + k * 8)]), 1.0, wood, "deep", clip=nm)
        p.edge(nest, wood, 1.1)
    if top >= -10:
        p.fill(Path.polygon([(x, top + 4), (x + 60, top + 12), (x, top + 20)]), moonlit("cloth.red", 0.1), "base")
    layer.place(p)


def shrouds(layer, x, ground, top_y, spread=150, width=3200, ratlines=True):
    """A mast's far shrouds: lines from chain plates on the bulwark up to the masthead, with ratlines
    across them, dark against the night."""
    rope = moonlit("rope", 0.35)
    a, b = x - spread, x + spread
    p = Piece(a - 10, top_y - 10, b + 10, ground, pad=4)
    feet = []
    for k in range(4):
        fx = a + (b - a) * (k / 3.0) * 0.36 if k < 2 else b - (b - a) * ((3 - k) / 3.0) * 0.36
        feet.append(fx)
    for fx in feet:
        fy = sheer_y(fx, ground, width) + 4
        p.stroke(Path.polyline([(fx, fy), (x + (fx - x) * 0.06, top_y)]), 1.6, rope, "deep")
    if ratlines:
        for side in (feet[:2], feet[2:]):
            f0, f1 = side
            y = sheer_y(f0, ground, width) - 24
            while y > top_y + 60:
                u = (y - top_y) / (sheer_y(f0, ground, width) - top_y)
                xa = x + (f0 - x) * (0.06 + 0.94 * u)
                xb = x + (f1 - x) * (0.06 + 0.94 * u)
                p.stroke(Path.polyline([(xa, y), (xb, y + 1)]), 1.0, rope, "deep", alpha=0.85)
                y -= 26
    layer.place(p, reach=0.4)


def stay(layer, a, b, sag=30, width=1.8):
    """A stay or a line hung between two points, sagging a little."""
    x0, x1 = min(a[0], b[0]), max(a[0], b[0])
    y0, y1 = min(a[1], b[1]), max(a[1], b[1])
    p = Piece(x0 - 6, y0 - 6, x1 + 6, y1 + sag + 6, pad=4)
    p.stroke(Path().move(*a).quad((a[0] + b[0]) / 2, (a[1] + b[1]) / 2 + sag, *b), width, moonlit("rope", 0.35), "deep")
    layer.place(p, reach=0.4)


def forecastle(layer, x1, ground, width, seed=0):
    """The bow: the forecastle's raised deck from the room's left edge to x1, its bulkhead of planks
    facing aft with a ladder up it, a rail along its top, and the bowsprit running out over the bow
    with its jib furled."""
    wood = moonlit("wood")
    light = moonlit("wood.light")
    fy = ground - 120
    p = Piece(-30, 100, x1 + 30, ground + 8)
    face = Path.rect(-30, fy, x1 + 30, ground - fy)
    m = p.fill(face, wood, "base-light")
    for k in range(1, 6):
        p.stroke(Path.polyline([(-30, fy + k * 20), (x1, fy + k * 20)]), 1.1, wood, "deep", clip=m)
    p.tone(p.where(m, lambda xx, yy: xx > x1 - 18), wood, "shadow")
    p.tone(p.below(m, ground - 26), wood, "shadow")
    lip = Path.rect(-30, fy - 8, x1 + 38, 12, r=2)
    lm = p.fill(lip, light, "base")
    p.tone(lm - lm.shift(0, 2), light, "light")
    p.edge(lip, light, 1.0)
    p.edge(face, wood, 1.2)
    # the ladder
    lx = x1 - 70
    for side in (-1, 1):
        p.fill(Path.rect(lx + side * 22 - 3, fy - 30, 7, ground - fy + 30), light, "base")
    for k in range(5):
        p.fill(Path.rect(lx - 22, fy - 10 + k * 26, 44, 5, r=1.5), light, "terminator")
    # the forecastle rail: posts and a cap
    for k in range(int(x1 / 46) + 1):
        px = 12 + k * 46
        if px > x1 - 6:
            break
        p.fill(Path.rect(px - 4, fy - 56, 8, 50), wood, "base")
    cap = Path.rect(-30, fy - 62, x1 + 28, 10, r=3)
    p.fill(cap, light, "base")
    p.edge(cap, light, 1.0)
    # the bowsprit
    bs = Path.capsule((40, fy - 40), (-40, fy - 190), 11, 8)
    p.fill(bs, light, "base")
    p.edge(bs, light, 1.1)
    p.ground_shadow(-20, x1 + 20, ground + 2, 10, 0.4)
    layer.place(p)
    return fy


def stern_cabin(layer, x0, ground, width, seed=0, lamp=None):
    """The stern: the cabin from x0 to the room's right edge, its front planked and painted, a row
    of lit windows with small panes, steps up to the quarterdeck, a carved rail along its top, the
    big stern lantern on its post (at x `lamp`) and the ensign flying aft."""
    wood = moonlit("wood")
    light = moonlit("wood.light")
    navy = moonlit("cloth.navy", 0.1)
    gold = moonlit("gold", 0.12)
    x1 = width + 30
    top = ground - 230
    p = Piece(x0 - 40, top - 300, x1, ground + 8)
    face = Path.rect(x0, top, x1 - x0, ground - top)
    m = p.fill(face, wood, "base")
    for k in range(1, 11):
        p.stroke(Path.polyline([(x0, top + k * 21), (x1, top + k * 21)]), 1.0, wood, "deep", clip=m)
    band = Path.rect(x0, top + 14, x1 - x0, 20)
    p.fill(band, navy, "base", clip=m)
    p.stroke(Path.polyline([(x0, top + 14), (x1, top + 14)]), 2, gold, "base")
    p.stroke(Path.polyline([(x0, top + 34), (x1, top + 34)]), 2, gold, "base")
    p.tone(p.where(m, lambda xx, yy: xx < x0 + 14), wood, "light")
    p.tone(p.below(m, ground - 24), wood, "shadow")
    p.edge(face, wood, 1.3)
    for i in range(int((x1 - x0 - 120) / 110)):
        wx = x0 + 120 + i * 110
        P.window(p, Window(wx, top + 66, 52, 62, "square", lit=True, panes=3, sill=True), wood, seed + i)
    # the steps up, against the cabin's front
    for k in range(6):
        sx = x0 - 30 + k * 0
        step = Path.rect(x0 - 36 + k * 6, ground - 26 - k * 34, 70 - k * 6, 10, r=2)
        sm = p.fill(step, light, "base")
        p.tone(sm - sm.shift(0, 2), light, "light")
        p.edge(step, light, 1.0)
        del sx
    p.fill(Path.rect(x0 - 40, top - 4, 10, ground - top + 4), light, "base")
    # the quarterdeck rail: turned balusters under a cap
    rail_y = top - 4
    for k in range(int((x1 - x0) / 30) + 1):
        bx = x0 + 10 + k * 30
        bal = Path().move(bx - 4, rail_y).quad(bx - 9, rail_y - 18, bx - 3, rail_y - 34).line(bx + 3, rail_y - 34).quad(bx + 9, rail_y - 18, bx + 4, rail_y).close()
        bm = p.fill(bal, light, "base")
        p.tone(p.where(bm, lambda xx, yy, c=bx: xx > c + 1), light, "shadow")
    cap = Path.rect(x0 - 6, rail_y - 44, x1 - x0 + 6, 11, r=3)
    cm = p.fill(cap, light, "base")
    p.tone(cm - cm.shift(0, 2), light, "light")
    p.edge(cap, light, 1.0)
    if lamp is not None:
        post = Path.rect(lamp - 5, rail_y - 150, 10, 110)
        p.fill(post, wood, "base")
        p.edge(post, wood, 1.0)
        P.lantern_body(p, lamp, rail_y - 196, 2.4, True)
        p.glow(lamp, rail_y - 160, 260, "#ffb860", 0.28)
    # the ensign on its staff, flying aft
    fx = width - 96
    p.fill(Path.rect(fx - 3, rail_y - 280, 6, 240), light, "base")
    t = moonlit("cloth.navy", 0.05)
    fy = rail_y - 276
    flag = Path().move(fx, fy).quad(fx + 40, fy - 8, fx + 90, fy + 6).line(fx + 80, fy + 30).line(fx + 92, fy + 56) \
        .quad(fx + 40, fy + 46, fx, fy + 54).close()
    fm = p.fill(flag, t, "base")
    p.tone(fm & p.shape(Path.ellipse(fx + 56, fy + 28, 14, 30)), t, "shadow")
    P._icon(p, "sun", fx + 36, fy + 26, 26)
    p.edge(flag, t, 1.1)
    p.ground_shadow(x0 - 40, x1, ground + 2, 10, 0.4)
    layer.place(p)


# --------------------------------------------------------------------------- #
# Recipes
# --------------------------------------------------------------------------- #

def pier(r):
    """The Pier (12-rooms §6): the long pier, boats moored, gulls on every post. Netty's teal
    fisherman's cottage at the west end under an oak, nets drying on its wall and lobster pots by
    its door; then the open harbour behind the quay's edge with a fishing boat and a dinghy tied
    up and gulls on the piles; the big warehouse with its hoist; and the harbour running out east
    to the mole and its beacon, the lighthouse on the headland beyond."""
    g = r.ground
    seed = roomkit.seed_of("docks", "pier")
    time = "day"
    haze_c = P.HAZE[time]

    P.sky(r["sky"], time, seed=seed, horizon=520, sun=(300, 140), clouds=4, cloud_band=(60, 280))
    harbour_far(r, seed, haze_c, horizon=486,
                ships=((620, 0.55, 2, "set", 1), (790, 0.7, 3, "furled", -1)),
                headland_at=(1400, 1700, 1560), mole=(1140, 1330))

    mid = r["mid"]
    P.floor_shade(mid, g, 52, 0.3)

    # the open water behind the quay, between the cottage and the warehouse and east of it
    boat(mid, 720, WATERLINE, 230, seed=seed + 3, hull="wood.blue", band="cloth.cream", stripe="cloth.red", bow=-1, moor=560)
    boat(mid, 1390, WATERLINE + 2, 150, seed=seed + 4, hull="wood.green", band="cloth.cream", stripe="gold", mast=None, bow=1, moor=1560)
    pile(mid, 540, 470, seed=seed + 5, gull_facing=1)
    pile(mid, 880, 492, seed=seed + 6, gull_facing=-1)
    pile(mid, 1210, 482, seed=seed + 7, gull_facing=1)
    pile(mid, 1560, 470, seed=seed + 8, gull_facing=0)
    quay_edge(mid, 500, 920, seed=seed + 1)
    quay_edge(mid, 1160, 1600, seed=seed + 2)

    # the oak behind the cottage, and Netty's cottage
    P.tree(mid, 150, g - 6, "oak", 360, seed=seed + 10)
    cx = r.door_x("netty-cottage")
    r.house(House(cx - 112, 228, g, floors=(("stone.blue", 208), ("plaster", 132)), roof="hip", roof_colour="roof.teal", roof_h=140,
                  chimneys=[(cx + 70, 24, 54)], door=Door(cx, 72, 148, "arch", wood="wood.teal"),
                  windows=[Window(cx - 70, None, 36, 54, "arch", lit=True, box=True, floor=0),
                           Window(cx - 54, None, 34, 48, "arch", shutters="wood.teal", floor=1),
                           Window(cx + 54, None, 34, 48, "arch", shutters="wood.teal", lit=True, floor=1)],
                  extras=[net_drape(cx + 44, cx + 104, g - 186, 64, seed=seed + 11)],
                  vines=(cx - 104,), seed=seed + 12))
    creels(mid, cx + 82, g + 2, 3, seed=seed + 13)

    # the warehouse
    wx = r.door_x("warehouse-west")
    r.house(House(wx - 135, 270, g, floors=(("stone.grey", 210), ("timber", 170)), roof="front", roof_colour="roof.slate", roof_h=150,
                  door=Door(wx, 100, 176, "double", wood="wood.dark"), gable_window=False,
                  windows=[Window(wx - 92, None, 34, 50, "square", floor=0), Window(wx + 94, None, 34, 50, "square", lit=True, floor=0),
                           Window(wx - 84, None, 32, 46, "arch", floor=1)],
                  extras=[loft(wx, g - 350, 66, 84, g - 404, 120), P.wall_lantern(wx - 66, g - 150, -1)],
                  seed=seed + 20))

    # the foreground: a mooring post at the west edge, a coil of rope at the east
    fore = r["fore"]
    bollard(fore, 60, CANVAS_H + 30, 1.0)
    fp = Piece(1440, CANVAS_H - 70, 1600, CANVAS_H + 20)
    rope_coil(fp, 1520, CANVAS_H + 14, 64, "rope", dark=0.5)
    fore.place(fp, reach=2.6)
    P.near_shade(fore, 820, 0.2)


def steps(layer, x0, x1, ground, n=6, rise=16):
    """Stone steps going up and away between two walls: each tread lit, each riser in shadow,
    narrowing a little as they climb."""
    p = Piece(x0 - 10, ground - n * rise - 20, x1 + 10, ground + 6)
    st = tones("stone.warm")
    for k in range(n):
        inset = k * 2.0
        y = ground - (k + 1) * rise
        riser = Path.rect(x0 + inset, y + 5, x1 - x0 - inset * 2, rise - 5)
        tread = Path.rect(x0 + inset - 2, y, x1 - x0 - inset * 2 + 4, 6, r=1.5)
        p.fill(riser, st, "shadow")
        p.edge(riser, st, 0.9, "deep-shadow")
        p.fill(tread, st, "light")
        p.edge(tread, st, 0.9, "shadow")
    layer.place(p)


def harbourside(r):
    """The Harbourside (12-rooms §6): Tomas's net rack, the customs house, the Wager's berth with a
    gangway. West to east: a stack of cargo by a lamp, the Rusty Anvil (a shabby brick and timber
    dive bar under a plum roof, one window boarded, its anvil sign out); Tomas's nets drying; the
    long berth, open water behind the quay's edge with a pile at each end, where the Wager (the
    game's prop) ties up; the customs house with its clock and flag; a fishmonger's stall and a
    blossom tree; the teal warehouse with its loft and hoist; and the harbour again at the east end
    with a boat moored. Across the water the mole and its beacon, ships at anchor, and the headland."""
    g = r.ground
    seed = roomkit.seed_of("docks", "harbourside")
    time = "day"
    haze_c = P.HAZE[time]

    P.sky(r["sky"], time, seed=seed, horizon=520, sun=(330, 150), clouds=5, cloud_band=(60, 280))
    fx = lambda x: r.x("far", x)  # noqa: E731
    harbour_far(r, seed, haze_c, horizon=486,
                ships=((fx(150), 0.6, 2, "furled", 1), (fx(1250), 0.8, 3, "furled", -1), (fx(2330), 0.55, 2, "set", -1)),
                headland_at=(fx(2900), fx(3500), fx(3200)), mole=(fx(800), fx(1500)))

    mid = r["mid"]
    P.floor_shade(mid, g, 52, 0.3)

    # the water: the berth, and the east end
    pile(mid, 740, 460, seed=seed + 1, gull_facing=1)
    pile(mid, 1790, 470, seed=seed + 2, gull_facing=-1)
    boat(mid, 3060, WATERLINE, 210, seed=seed + 3, hull="wood.teal", band="cloth.cream", stripe="gold", bow=1, moor=2900)
    pile(mid, 2930, 476, seed=seed + 4, gull_facing=1)
    quay_edge(mid, 560, 1830, seed=seed + 5)
    quay_edge(mid, 2820, 3200, seed=seed + 6)

    # the west end: cargo by a lamp
    P.lamp_post(mid, 176, g, 300)
    P.crate(mid, 40, g + 2, 50, 44, seed=seed + 7)
    P.crate(mid, 46, g - 42, 42, 36, seed=seed + 8)
    P.crate(mid, 98, g + 3, 44, 40, seed=seed + 9, top_goods="orange")
    P.barrel(mid, -6, g + 2, seed=seed + 10)

    # the Rusty Anvil
    ax = r.door_x("anvil")
    up = g - 220 - 150 + (150 - 48) * 0.42
    r.house(House(ax - 205, 320, g, floors=(("brick", 220), ("timber.sack", 150)), roof="gable", roof_colour="roof.plum", roof_h=130,
                  jetty=8, chimneys=[(ax - 150, 26, 60)], door=Door(ax, 80, 158, "square", wood="wood.dark", surround="timber"),
                  windows=[Window(ax - 128, None, 52, 62, "square", lit=True, panes=3, floor=0), Window(ax + 80, None, 34, 50, "square", lit=True, floor=0),
                           Window(ax - 118, up, 34, 48, "arch", floor=1), Window(ax + 40, None, 34, 48, "arch", lit=True, floor=1)],
                  extras=[boarded(ax - 118, up, 34, 48), sign(ax - 205, 404, "anvil", board="wood.dark", side=-1, shape="shield", trim="brass"),
                          P.wall_lantern(ax - 56, g - 150, -1)],
                  seed=seed + 20))
    P.barrel(mid, ax + 140, g + 2, 38, 48, seed=seed + 21)

    # Tomas's nets
    net_rack(mid, 600, 720, g, 176, seed=seed + 25)

    # the customs house
    cx = 1995
    r.house(House(cx - 165, 330, g, floors=(("stone", 236), ("stone.warm", 164)), roof="hip", roof_colour="roof.blue", roof_h=150,
                  finials=True, dormers=(cx,), chimneys=[(cx - 120, 26, 60)],
                  windows=[Window(cx - 110, None, 50, 92, "arch", lit=True, panes=3, floor=0), Window(cx, None, 50, 92, "arch", panes=3, floor=0),
                           Window(cx + 110, None, 50, 92, "arch", lit=True, panes=3, floor=0),
                           Window(cx - 110, None, 38, 56, "arch", box=True, floor=1), Window(cx + 110, None, 38, 56, "arch", lit=True, box=True, floor=1)],
                  extras=[clock(cx, g - 236 - 82, 26), P.banner(cx - 52, g - 236 - 150, 28, 96, "cloth.navy", "sun"),
                          P.banner(cx + 52, g - 236 - 150, 28, 96, "cloth.navy", "sun"),
                          flagpole(cx + 96, 92, 76, "cloth.navy", "sun")],
                  seed=seed + 30))

    # the fishmonger and the blossom tree
    P.tree(mid, 2390, g - 30, "blossom", 300, seed=seed + 40)
    fish_stall(mid, 2250, g, 170, ("cloth.pink", "cloth.cream"), seed=seed + 41)
    P.low_wall(mid, 2330, 2470, g, 48, plants=("flower.pink", "flower.white"), seed=seed + 42)

    # the teal warehouse
    wx = r.door_x("warehouse-middle")
    r.house(House(wx - 150, 300, g, floors=(("stone.grey", 212), ("timber", 176)), roof="front", roof_colour="roof.teal", roof_h=160,
                  finials=True, gable_window=False, door=Door(wx, 104, 178, "double", wood="wood.dark"),
                  windows=[Window(wx - 100, None, 36, 54, "square", lit=True, floor=0), Window(wx + 100, None, 36, 54, "square", floor=0),
                           Window(wx - 96, None, 32, 46, "arch", floor=1)],
                  extras=[loft(wx, g - 212 - 140, 66, 84, g - 212 - 194, 126), P.wall_lantern(wx + 70, g - 160, 1)],
                  seed=seed + 50))
    creels(mid, wx + 196, g + 2, 3, seed=seed + 51)
    P.lamp_post(mid, 2840, g, 290)

    # the foreground: the two bollards, and the near edge darkening
    fore = r["fore"]
    for bx in (537, 2014):
        bollard(fore, r.x("fore", bx), CANVAS_H + 30, 1.0)
    P.near_shade(fore, 820, 0.2)


def fish_market(r):
    """The Fish Market (12-rooms §6): stalls, ice, a crane hoisting crates. West to east: the pink
    townhouse; the market on the quay, two fish stalls either side of a crane lifting crates out of
    a lighter; the Chandler's (the shop: an anchor on its sign); the ice store with blocks of ice at
    its hatch; a rack of fish drying; the Watch post; then the street climbs east toward the city,
    a blossom tree over the wall, lamps either side of the steps up, and Questeria on its hill
    across the far layer with the Sunspire."""
    g = r.ground
    seed = roomkit.seed_of("docks", "fish-market")
    time = "day"
    haze_c = P.HAZE[time]

    P.sky(r["sky"], time, seed=seed, horizon=520, sun=(280, 150), clouds=5, cloud_band=(60, 280))
    fx = lambda x: r.x("far", x)  # noqa: E731
    harbour_far(r, seed, haze_c, horizon=486, city=fx(2950),
                ships=((fx(-40), 0.6, 2, "set", 1), (fx(780), 0.75, 3, "furled", -1), (fx(1760), 0.5, 2, "furled", 1)),
                mole=(fx(380), fx(1000)))

    mid = r["mid"]
    P.floor_shade(mid, g, 52, 0.3)

    # the water: the west end and the market quay
    pile(mid, 170, 466, seed=seed + 1, gull_facing=1)
    boat(mid, 720, WATERLINE + 2, 220, seed=seed + 2, hull="wood.dark", band="wood.light", stripe="cloth.blue", mast=None, bow=-1, moor=560)
    pile(mid, 1050, 470, seed=seed + 3, gull_facing=-1)
    quay_edge(mid, 0, 240, seed=seed + 4)
    quay_edge(mid, 470, 1110, seed=seed + 5)

    # the pink townhouse
    tx = 352
    r.house(House(tx - 120, 240, g, floors=(("plaster.pink", 214), ("plaster.pink", 140), ("timber", 120)), roof="front", roof_colour="roof.red", roof_h=130,
                  jetty=6, finials=True,
                  windows=[Window(tx - 50, None, 70, 70, "square", lit=True, panes=3, floor=0), Window(tx + 72, None, 36, 56, "arch", box=True, floor=0),
                           Window(tx - 56, None, 34, 50, "arch", shutters="wood.green", box=True, floor=1), Window(tx + 56, None, 34, 50, "arch", shutters="wood.green", lit=True, floor=1),
                           Window(tx - 40, None, 32, 46, "arch", floor=2), Window(tx + 40, None, 32, 46, "arch", lit=True, floor=2)],
                  extras=[P.awning(tx - 96, tx - 4, g - 172, 28, ("cloth.teal", "cloth.cream"))],
                  vines=(tx + 108,), seed=seed + 10))

    # the market on the quay: two stalls and the crane between them
    fish_stall(mid, 560, g, 160, ("cloth.blue", "cloth.cream"), seed=seed + 20)
    crane(mid, 800, g, 390, 150, -1, seed=seed + 21, load=2)
    fish_stall(mid, 960, g, 170, ("cloth.pink", "cloth.cream"), seed=seed + 22, crab=False)

    # the Chandler's
    sx = r.door_x("chandler")
    r.house(House(sx - 150, 300, g, floors=(("stone.warm", 224), ("timber", 150)), roof="hip", roof_colour="roof.slate", roof_h=150,
                  jetty=8, dormers=(sx - 20,), chimneys=[(sx + 110, 26, 60)], door=Door(sx, 76, 152, "arch", wood="wood.blue"),
                  windows=[Window(sx - 92, None, 62, 74, "square", lit=True, panes=3, floor=0), Window(sx + 96, None, 36, 56, "arch", lit=True, box=True, floor=0),
                           Window(sx - 80, None, 36, 52, "arch", box=True, floor=1), Window(sx + 80, None, 36, 52, "arch", lit=True, box=True, floor=1)],
                  extras=[P.awning(sx - 132, sx - 52, g - 182, 28, ("cloth.blue", "cloth.cream")),
                          sign(sx + 150, 404, "anchor", board="cloth.navy", side=1, shape="oval"),
                          P.wall_lantern(sx - 50, g - 150, -1)],
                  seed=seed + 30))

    # the ice store
    ix0, ix1 = 1440, 1650
    r.house(House(ix0, ix1 - ix0, g, floors=(("stone.grey", 200),), roof="gable", roof_colour="roof.slate", roof_h=110,
                  extras=[ice_hatch(ix0 + 40, ix1 - 40, g - 150, g - 64)], seed=seed + 40))

    # fish drying, and the Watch post
    fish_rack(mid, 1690, 1880, g, 176, seed=seed + 45)
    wx = 1984
    r.house(House(wx - 72, 144, g, floors=(("stone.grey", 160), ("plaster.blue", 96)), roof="cone", roof_colour="roof.blue", roof_h=140,
                  windows=[Window(wx, None, 70, 52, "square", lit=True, panes=2, floor=0), Window(wx, None, 30, 40, "arch", floor=1)],
                  extras=[P.banner(wx - 50, g - 150, 26, 80, "cloth.navy", "diamond", pole=False),
                          P.banner(wx + 50, g - 150, 26, 80, "cloth.navy", "diamond", pole=False)],
                  seed=seed + 50))

    # the climb toward the city: a wall with a blossom tree over it, lamps by the steps
    P.tree(mid, 2282, g - 40, "blossom", 280, seed=seed + 60)
    P.low_wall(mid, 2120, 2600, g, 64, plants=("flower.white", "flower.pink"), seed=seed + 61)
    P.lamp_post(mid, 2416, g, 300)
    P.bush(mid, 2656, g, 130, 70, seed=seed + 62, flowers_on=("flower.pink", "blossom"))
    P.tree(mid, 3140, g - 10, "oak", 330, seed=seed + 63)
    steps(mid, 2820, 2876, g)
    P.low_wall(mid, 2700, 2806, g, 52, seed=seed + 64)
    P.low_wall(mid, 2890, 3200, g, 52, seed=seed + 65)
    for lx in (2800, 2896):
        P.lamp_post(mid, lx, g, 280)
    P.bush(mid, 3088, g, 110, 56, seed=seed + 66, flowers_on=("flower.pink", "flower.yellow"))

    # the foreground
    fore = r["fore"]
    bollard(fore, r.x("fore", 640), CANVAS_H + 30, 1.0)
    bollard(fore, r.x("fore", 2050), CANVAS_H + 30, 0.9)
    P.fore_foliage(fore, r.x("fore", 2980), CANVAS_H + 30, 220, 90, seed + 70, 0.5)
    P.near_shade(fore, 820, 0.2)


def swell(layer, y0, y1, seed=0, colour="#141a44", crest="#8d95d8"):
    """A long swell on the night sea from y0 to y1: broad soft troughs in a darker blue, each with
    a thin pale crest, longer and bolder nearer."""
    w = layer.w
    for i in range(int(w * (y1 - y0) / 9000)):
        u = hash01(i, 1, seed + 1001)
        y = y0 + 6 + u * u * (y1 - y0 - 12)
        x = hash01(i, 2, seed + 1001) * w
        ln = (60 + 160 * hash01(i, 3, seed + 1001)) * (0.4 + u)
        layer.soft_ellipse(x, y + 3, ln, 2.5 + 4 * u, colour, 0.35, hard=0.5)
        layer.soft_ellipse(x - ln * 0.1, y, ln * 0.7, 0.8 + u, crest, 0.3 + 0.2 * u, hard=0.6)


def moon_path(layer, x, y0, y1, seed=0, colour="#f2eedc"):
    """The moon's light lying on the sea below it: short bright dashes in a column, wider and more
    broken nearer."""
    for i in range(int((y1 - y0) / 3)):
        u = hash01(i, 1, seed + 1011)
        y = y0 + 2 + u * (y1 - y0 - 4)
        spread = 18 + 120 * u
        dx = (hash01(i, 2, seed + 1011) - 0.5) * 2 * spread
        ln = 6 + 30 * u * hash01(i, 3, seed + 1011)
        layer.soft_ellipse(x + dx, y, ln, 0.8 + 1.2 * u, colour, 0.55 * (1 - abs(dx) / spread * 0.7), hard=0.6)


def deck(r):
    """The Main Deck of the Gull's Wager, two days out at night (12-rooms §6; 03b §1: deck at
    night, rail, mast, hanging lanterns, sea all round). A moon and stars over a long dark swell
    with the moon's path on it; the far bulwark along the deck's back edge, navy and gold, its pin
    rails hung with coiled falls and a lamp lit at each of the zone's rail lights; the forecastle
    and the bowsprit at the left (the bow), the fore and mizzen masts with their sails furled and
    the shrouds and stays of all three (the mainmast is the game's prop), and the stern cabin at the
    right with its windows lit, the big stern lantern and the ensign. The room is dark: the wood is
    moonlit, and the game's dark and the lamps' pools do the rest."""
    g = r.ground
    W = r.room.width
    seed = roomkit.seed_of("gulls-wager", "deck")
    time = "night"
    haze_c = P.HAZE[time]
    horizon = 430

    P.sky(r["sky"], time, seed=seed, horizon=horizon + 40, moon=(1250, 290), clouds=4, cloud_band=(50, 180))
    far = r["far"]
    P.sea(far, horizon, CANVAS_H, haze_c, seed=seed + 1, time=time, k=0.35)
    swell(far, horizon, CANVAS_H - 200, seed=seed + 2)
    moon_path(far, 1410, horizon + 2, CANVAS_H - 260, seed=seed + 3)

    mid = r["mid"]
    lights = {li.id: li for li in r.room.lights}
    # the rigging behind everything: the shrouds and stays of all three masts
    shrouds(mid, 700, g, 40, 150, W)
    shrouds(mid, 1600, g, 20, 180, W)
    shrouds(mid, 2520, g, 60, 140, W)
    stay(mid, (700, 30), (-40, 300), sag=30)
    stay(mid, (1600, 30), (700, 330), sag=40)
    stay(mid, (2520, 60), (1600, 330), sag=40)
    stay(mid, (1600, 120), (2520, 360), sag=30, width=1.4)
    # the bulwark, its pin rails, lamps
    bulwark(mid, 0, W, g, W, seed=seed + 10)
    for a, b in ((820, 1100), (1760, 2040), (2190, 2420)):
        pin_rail(mid, a, b, sheer_y((a + b) / 2, g, W) + 26, seed=seed + a)
    for lid in ("rail-lamp-1", "rail-lamp-2"):
        li = lights.get(lid)
        if li is not None:
            rail_lamp(mid, li.x, sheer_y(li.x, g, W) - 6)
    # the masts the painting carries
    mast(mid, 700, g, 960, yards=(0.3, 0.56), top_mark=True, seed=seed + 20)
    mast(mid, 2520, g, 900, yards=(0.34, 0.6), top_mark=False, seed=seed + 21)
    # the bow and the stern
    forecastle(mid, 470, g, W, seed=seed + 30)
    stern = lights.get("stern-lamp")
    stern_cabin(mid, 2730, g, W, seed=seed + 31, lamp=stern.x if stern is not None else 3022)
    P.floor_shade(mid, g, 46, 0.3)

    # the foreground: the near shrouds coming down to the near rail, dark, and a coil of rope
    fore = r["fore"]
    dark = 0.6
    for x0 in (r.x("fore", 360), r.x("fore", 2860)):
        p = Piece(x0 - 20, -10, x0 + 340, CANVAS_H + 10, pad=4)
        t = P.haze(tones("rope"), "#140e1c", dark)
        feet = [x0 + k * 26 for k in range(4)]
        for fx_ in feet:
            p.stroke(Path.polyline([(fx_, CANVAS_H + 10), (x0 + 300, -10)]), 3.2, t, "deep")
        y = CANVAS_H - 30
        while y > 40:
            u = (CANVAS_H + 10 - y) / (CANVAS_H + 20)
            xa = feet[0] + (x0 + 300 - feet[0]) * u
            xb = feet[-1] + (x0 + 300 - feet[-1]) * u
            p.stroke(Path.polyline([(xa, y), (xb, y + 1)]), 2.2, t, "deep")
            y -= 40
        fore.place(p, reach=1.2)
    cp = Piece(r.x("fore", 1180) - 90, CANVAS_H - 70, r.x("fore", 1180) + 90, CANVAS_H + 20)
    rope_coil(cp, r.x("fore", 1180), CANVAS_H + 16, 70, "rope", dark=0.6)
    fore.place(cp, reach=2.6)


RECIPES = {
    "docks/pier": pier,
    "docks/harbourside": harbourside,
    "docks/fish-market": fish_market,
    "gulls-wager/deck": deck,
}
