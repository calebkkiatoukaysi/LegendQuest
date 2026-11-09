"""
Every room indoors, painted (12-rooms §6, 03b §1): the Gilded Flagon's taproom, the Rusty Anvil's
bar, Sunspire Keep's great hall, and Questeria's small insides (the bakery, the chapel, the two
cottages, the forge, the Chandler's and the warehouse). One recipe a room, each a function of a
paint_room.Room, registered in RECIPES; tools/rooms/paint_all.py runs them.

    python tools/rooms/paint_all.py interiors --no-shared --preview shots/art/rooms

An interior is `kind = interior`: no sky, no far layer, no parallax. Its mid layer is the back
wall from the ceiling down to the floor's back edge (paint.back_wall: plaster, stone or boards
over a wainscot and a skirting, darkening toward the ceiling), the ceiling's beams, and what's
built into the room or hung on its walls: windows (with curtains), a gallery and its stair, a
chimney breast or a forge's hood, shelves, pictures, sconces, chandeliers, banners. The
furniture standing on the floor (the bar counter, the hearths, the tables, the throne, the
benches, the anvil) is the zone file's props, drawn by the game, so the painting leaves room for
it: a chimney breast rises behind each hearth prop, the back bar's shelves sit over the kegs, the
throne's cloth of estate hangs behind the throne. The foreground is a dark beam across the top
and a post or two, so the room has a near side.

Most of these rooms are dark (the game lays its dark over them and the hearths' and candles'
pools of light cut it), so they're painted at their full colour and let the game dim them; the
great hall is lit, by its windows.

Parts this family adds, on paint.py's primitives (so the line and shading match): window (an
interior window with its view, curtains and a box), picture, sconce, chandelier, hanging_lantern,
plaque, garland, herbs, onions, gallery (a balcony on posts with a balustrade, doors and windows
on its upper floor), stair, chimney_breast, hood, pilaster, vault, rose_window, lancet, canopy,
arms (a shield on crossed swords), tool_board, bellows, horseshoe, ship_wheel, anchor,
mounted_fish, notices, rope_hanks, bread_shelf, wheat, ship_model, sacks_stack, mezzanine,
hoist_chain, and fore_frame for the near side.

Only needs CPython and numpy.
"""

import math
import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
import paint as P  # noqa: E402
import roomkit  # noqa: E402
from paint import Door, Path, Piece, Window, hash01, tones  # noqa: E402
from roomkit import CANVAS_H  # noqa: E402

DARK = "#140e1c"
"""What the foreground is pulled toward: a near-black plum."""


def dim(name, k=0.5):
    return P.haze(tones(name), DARK, k)


# --------------------------------------------------------------------------- #
# Walls and what's built into them
# --------------------------------------------------------------------------- #

def window(layer, cx, top, w=90, h=130, view="day", curtains=None, box=False, kind="arch", panes=3, seed=0, wall="plaster"):
    """A window in an interior wall: paint.window's surround, glass and bars, the view (day: a soft
    light falling in; night: the glass gone deep blue with a star or two), curtains tied back
    either side under a valance if `curtains` is a colour, and a box of flowers on the sill."""
    p = Piece(cx - w - 60, top - 46, cx + w + 60, top + h + (74 if box else 40))
    P.window(p, Window(cx, top, w, h, kind, lit=False, panes=panes, box=box), tones(wall), seed)
    x = cx - w / 2
    glass = P._opening(kind, x, top, w, h)
    if view == "night":
        gm = p.shape(glass)
        p.colour(gm, "#1b2150", alpha=0.62)
        for i in range(3):
            sx = x + w * (0.2 + 0.6 * hash01(i, 1, seed + 2001))
            sy = top + h * (0.15 + 0.4 * hash01(i, 2, seed + 2001))
            p.fill(Path.circle(sx, sy, 1.4), tones("#fff8e6"), "base", clip=gm)
    if curtains:
        ct = tones(curtains)
        for side in (-1, 1):
            ox = cx + side * (w / 2 + 6)
            tie_y = top + h * 0.62
            outer = ox + side * 26
            drape = Path().move(ox - side * 10, top - 16).line(outer, top - 16).quad(outer + side * 4, tie_y - 30, outer - side * 6, tie_y) \
                .quad(outer + side * 6, top + h + 8, outer + side * 2, top + h + 22).line(ox - side * 4, top + h + 22) \
                .quad(ox + side * 2, tie_y + 30, ox + side * 12, tie_y).quad(ox - side * 12, tie_y - 50, ox - side * 10, top - 16).close()
            dm = p.fill(drape, ct, "base")
            for k in range(1, 3):
                fx = ox + side * (6 + k * 7)
                p.stroke(Path.smooth([(fx, top - 14), (fx + side * 2, tie_y - 20), (fx - side * 2, tie_y + 4), (fx + side * 4, top + h + 20)]), (2.2, 1.0), ct, "shadow", clip=dm)
            if side > 0:
                p.tone(p.where(dm, lambda xx, yy, a=ox: xx > a + 14), ct, "shadow")
            else:
                p.tone(p.where(dm, lambda xx, yy, a=ox: xx < a - 20), ct, "light")
            p.fill(Path.ellipse(ox + side * 10, tie_y, 9, 5), tones("gold"), "base")
            p.edge(drape, ct, 1.1)
        rod = Path.rect(cx - w / 2 - 40, top - 22, w + 80, 6, r=3)
        p.fill(rod, tones("wood.dark"), "base")
        for ex in (cx - w / 2 - 42, cx + w / 2 + 42):
            p.fill(Path.circle(ex, top - 19, 5), tones("gold"), "base")
        val = Path().move(cx - w / 2 - 36, top - 20).line(cx + w / 2 + 36, top - 20)
        n = 6
        for i in range(n, 0, -1):
            a = cx - w / 2 - 36 + (w + 72) * i / n
            b = cx - w / 2 - 36 + (w + 72) * (i - 1) / n
            val.quad((a + b) / 2, top + 6, b, top - 20 + (0 if i > 1 else 0))
        vm = p.fill(val.close(), ct, "base-light")
        p.tone(vm - vm.shift(0, -3), ct, "shadow")
        p.edge_mask(vm, ct, 1.0)
    if view == "day":
        p.glow(cx, top + h * 0.5, w * 2.2, "#fff2d0", 0.2)
    layer.place(p)
    return p


def picture(layer, cx, cy, w=90, h=70, subject="sea", frame="gold", seed=0):
    """A framed painting on the wall, hung from a nail by a cord: a seascape, hills, a ship, or a
    portrait; its frame lit on its top left, its shadow on the wall."""
    q = Piece(cx - w / 2, cy - h / 2 - 36, cx + w / 2, cy - h / 2, pad=4)
    q.stroke(Path.polyline([(cx - w * 0.35, cy - h / 2 - 4), (cx, cy - h / 2 - 30), (cx + w * 0.35, cy - h / 2 - 4)]), 1.3, tones("rope"), "deep")
    q.fill(Path.circle(cx, cy - h / 2 - 30, 2.6), tones("iron"), "base")
    layer.place(q, reach=0.6)
    p = Piece(cx - w / 2 - 16, cy - h / 2 - 20, cx + w / 2 + 16, cy + h / 2 + 20)
    p.colour(p.shape(Path.rect(cx - w / 2 - 3, cy - h / 2 - 3, w + 16, h + 16, r=2)), P.SHADOW_COLOUR, alpha=0.28)
    ft = tones(frame)
    outer = Path.rect(cx - w / 2 - 8, cy - h / 2 - 8, w + 16, h + 16, r=2)
    fm = p.fill(outer, ft, "base")
    p.tone(fm - fm.shift(2, 2), ft, "light")
    p.tone(fm - fm.shift(-2, -2), ft, "shadow")
    p.edge(outer, ft, 1.1)
    x0, y0 = cx - w / 2, cy - h / 2
    inner = Path.rect(x0, y0, w, h)
    im = p.shape(inner)
    if subject == "portrait":
        p.tone(im, tones("#3d3150"), "base")
        p.fill(Path.ellipse(cx, y0 + h * 1.02, w * 0.36, h * 0.34), tones("cloth.plum"), "base", clip=im)
        p.fill(Path.ellipse(cx, y0 + h * 0.44, w * 0.15, h * 0.2), tones("#f0c8a8"), "base", clip=im)
        p.fill(Path.ellipse(cx, y0 + h * 0.34, w * 0.17, h * 0.13), tones("#8a5a3a"), "base", clip=im)
    else:
        p.tone(im, tones("#a8c6f0"), "base")
        p.fill(Path.circle(x0 + w * 0.75, y0 + h * 0.28, h * 0.1), tones("#fff6dc"), "base", clip=im)
        if subject in ("sea", "ship"):
            p.fill(Path.rect(x0, y0 + h * 0.58, w, h), tones("sea"), "base", clip=im)
            p.stroke(Path.polyline([(x0 + w * 0.1, y0 + h * 0.72), (x0 + w * 0.4, y0 + h * 0.72)]), 1.2, tones("sea"), "light", clip=im)
            if subject == "ship":
                p.fill(Path.polygon([(cx - w * 0.2, y0 + h * 0.56), (cx + w * 0.2, y0 + h * 0.56), (cx + w * 0.14, y0 + h * 0.66), (cx - w * 0.16, y0 + h * 0.66)]), tones("wood.dark"), "base", clip=im)
                p.fill(Path.polygon([(cx - 2, y0 + h * 0.14), (cx - 2, y0 + h * 0.54), (cx - w * 0.18, y0 + h * 0.5)]), tones("cloth.cream"), "base", clip=im)
                p.fill(Path.polygon([(cx + 2, y0 + h * 0.2), (cx + 2, y0 + h * 0.54), (cx + w * 0.16, y0 + h * 0.52)]), tones("cloth.cream"), "shadow", clip=im)
            else:
                p.fill(Path.polygon([(x0 + w * 0.6, y0 + h * 0.52), (x0 + w * 0.7, y0 + h * 0.3), (x0 + w * 0.72, y0 + h * 0.54)]), tones("cloth.cream"), "base", clip=im)
        else:
            p.fill(Path.smooth([(x0 - 4, y0 + h * 0.62), (x0 + w * 0.35, y0 + h * 0.46), (x0 + w * 0.7, y0 + h * 0.58), (x0 + w + 4, y0 + h * 0.5), (x0 + w + 4, y0 + h + 4), (x0 - 4, y0 + h + 4)], closed=True), tones("hill"), "base", clip=im)
            p.fill(Path.smooth([(x0 - 4, y0 + h * 0.8), (x0 + w * 0.5, y0 + h * 0.66), (x0 + w + 4, y0 + h * 0.78), (x0 + w + 4, y0 + h + 4), (x0 - 4, y0 + h + 4)], closed=True), tones("leaf"), "base", clip=im)
            p.fill(Path.circle(x0 + w * 0.28, y0 + h * 0.5, h * 0.12), tones("leaf.dark"), "base", clip=im)
    p.edge(inner, tones("#3d3150"), 1.2, "deep")
    layer.place(p)


def sconce(layer, x, y, lit=True, metal="brass"):
    """A candle sconce on the wall: a backplate, a curled arm, a cup and a candle, lit."""
    p = Piece(x - 30, y - 60, x + 30, y + 40)
    m = tones(metal)
    plate = Path.ellipse(x, y + 10, 9, 18)
    pm = p.fill(plate, m, "base")
    p.tone(pm - pm.shift(-2, -2), m, "shadow")
    p.edge(plate, m, 1.0)
    p.stroke(Path.smooth([(x, y + 18), (x + 8, y + 16), (x + 12, y + 6), (x + 10, y - 2)]), 3.0, m, "base")
    cup = Path.polygon([(x + 2, y - 4), (x + 18, y - 4), (x + 14, y + 2), (x + 6, y + 2)])
    p.fill(cup, m, "light")
    candle = Path.rect(x + 6, y - 28, 8, 24, r=1.5)
    p.fill(candle, tones("cloth.cream"), "base")
    p.edge(candle, tones("cloth.cream"), 0.9)
    if lit:
        p.fill(Path.ellipse(x + 10, y - 35, 3, 6), tones("#ffcf6a"), "light")
        p.fill(Path.ellipse(x + 10, y - 33, 1.4, 3), tones("#fff6d8"), "base")
        p.glow(x + 10, y - 34, 90, "#ffc070", 0.32)
        p.glow(x + 10, y - 34, 26, "#fff0c0", 0.4)
    layer.place(p)


def chandelier(layer, x, top, drop=150, r=80, lit=True, metal="wood.dark"):
    """A wheel chandelier on a chain: the rim seen a little from below, chains to it from a ring,
    candles standing round it, lit."""
    iron = tones("iron")
    hub_y = top + drop - 46
    q = Piece(x - r - 10, top - 4, x + r + 10, top + drop + 4, pad=4)
    for k in range(int((hub_y - top) / 9)):
        q.stroke(Path.ellipse(x, top + k * 9 + 4, 2.6, 4.6), 1.4, iron, "base")
    for dx in (-r * 0.92, 0, r * 0.92):
        q.stroke(Path.polyline([(x, hub_y), (x + dx, top + drop)]), 1.4, iron, "deep")
    q.fill(Path.circle(x, hub_y, 5), iron, "base")
    layer.place(q, reach=0.7)
    p = Piece(x - r - 30, top + drop - 50, x + r + 30, top + drop + 40)
    t = tones(metal)
    rim = Path.ellipse(x, top + drop, r, r * 0.2)
    hole = Path.ellipse(x, top + drop - 2, r - 9, r * 0.2 - 5)
    rm = p.shape(rim) - p.shape(hole)
    p.tone(rm, t, "base")
    p.tone(rm - rm.shift(0, 2.5), t, "light")
    p.tone(p.below(rm, top + drop + r * 0.12), t, "shadow")
    p.edge_mask(rm, t, 1.0)
    n = 7
    for i in range(n):
        a = math.pi * (i + 0.5) / n
        cx = x - math.cos(a) * r * 0.94
        cy = top + drop + math.sin(a) * r * 0.16
        candle = Path.rect(cx - 3.5, cy - 24, 7, 22, r=1.5)
        p.fill(candle, tones("cloth.cream"), "base")
        p.fill(Path.ellipse(cx, cy - 1, 7, 3), tones("brass"), "base")
        p.edge(candle, tones("cloth.cream"), 0.9)
        if lit:
            p.fill(Path.ellipse(cx, cy - 30, 2.6, 5.4), tones("#ffcf6a"), "light")
            p.glow(cx, cy - 30, 40, "#ffd890", 0.3)
    if lit:
        p.glow(x, top + drop - 20, r * 3.0, "#ffb860", 0.22)
    layer.place(p)


def hanging_lantern(layer, x, top, drop=110, s=1.6, lit=True):
    """A lantern hung from the ceiling on a chain."""
    iron = tones("iron")
    q = Piece(x - 10, top - 4, x + 10, top + drop + 4, pad=4)
    for k in range(int(drop / 9)):
        q.stroke(Path.ellipse(x, top + k * 9 + 4, 2.4, 4.4), 1.3, iron, "base")
    layer.place(q, reach=0.7)
    p = Piece(x - 30, top + drop - 10, x + 30, top + drop + 30 * s + 10)
    P.lantern_body(p, x, top + drop, s, lit)
    layer.place(p)


def plaque(layer, cx, top, w, h, icon="tankard", board="wood.dark"):
    """A name board fixed to the wall, gold-rimmed with an icon: paint.wall_sign's, or with one of
    the harbour's icons (an anchor, a fish) from paint_rooms_docks."""
    import paint_rooms_docks as D
    p = Piece(cx - w / 2 - 10, top - 10, cx + w / 2 + 10, top + h + 14)
    p.colour(p.shape(Path.rect(cx - w / 2 + 3, top + 4, w, h, r=4)), P.SHADOW_COLOUR, alpha=0.28)
    if icon in D.ICONS:
        bt = tones(board)
        bp = Path.rect(cx - w / 2, top, w, h, r=4)
        m = p.fill(bp, bt, "base")
        p.tone(m - m.shift(1.6, 1.6), bt, "light")
        ring = m - m.inset(3)
        p.tone(ring, tones("gold"), "base")
        D.ICONS[icon](p, cx, top + h / 2, h * 0.9)
        for sx in (cx - w / 2 + 8, cx + w / 2 - 8):
            p.fill(Path.circle(sx, top + h / 2, 2.0), tones("gold"), "light")
        p.edge(bp, bt, 1.3)
    else:
        P.wall_sign(cx, top, w, h, icon, board)(p)
    layer.place(p)


def garland(layer, x0, x1, y, sag=30, seed=0, flowers=("flower.yellow", "flower.white"), every=180):
    """A garland of leaves (hops, ivy) looped along a wall from x0 to x1, a flower here and there."""
    leaf = tones("leaf")
    leaf2 = tones("leaf.yellow")
    n = max(1, int((x1 - x0) / every))
    for k in range(n):
        a = x0 + (x1 - x0) * k / n
        b = x0 + (x1 - x0) * (k + 1) / n
        p = Piece(a - 12, y - 16, b + 12, y + sag + 24)
        cord = Path().move(a, y).quad((a + b) / 2, y + sag * 2, b, y)
        clump = p.nothing()
        for i in range(int((b - a) / 9) + 1):
            (lx, ly), _ = cord.at(i / max(1, int((b - a) / 9)))
            clump = clump | p.shape(Path.circle(lx, ly + 2, 6 + 2.5 * hash01(i, k, seed + 2011)))
        t = leaf if k % 2 == 0 else leaf2
        p.tone(clump, t, "base")
        p.tone(clump - clump.shift(-1.6, -2.2), t, "shadow")
        p.tone(clump - clump.shift(1.2, 1.6), t, "light")
        p.edge_mask(clump, t, 1.0)
        for i in range(4):
            (lx, ly), _ = cord.at((i + 0.5) / 4)
            c = tones(flowers[(i + k) % len(flowers)])
            fm = p.fill(Path.circle(lx, ly, 3.6), c, "base")
            p.tone(fm - fm.shift(-0.8, -0.8), c, "shadow")
        for ex in (a, b):
            p.fill(Path.circle(ex, y, 3.4), tones("brass"), "base")
        layer.place(p, reach=1.6)


def frieze(layer, x0, x1, y, colours=("flower.pink", "flower.blue"), leaf="leaf", step=56, seed=0):
    """A stencilled frieze painted along the top of a wall: two fine lines, a wandering vine between
    them, and a little flower with its leaves every `step` pixels, flat paint with no outline."""
    lt = tones(leaf)
    for a in range(int(x0), int(x1), 400):
        b = min(int(x1), a + 400)
        p = Piece(a, y - 16, b, y + 16, pad=2)
        for yy in (y - 13, y + 13):
            p.stroke(Path.polyline([(a, yy), (b, yy)]), 1.6, lt, "shadow")
        vine = Path.smooth([(x, y + 5 * math.sin((x - x0) / step * math.pi)) for x in range(a - 12, b + 14, 7)])
        p.stroke(vine, 1.5, lt, "base")
        k = int((a - x0) // step)
        x = x0 + k * step + step / 2
        while x < b + step:
            if a - 10 <= x <= b + 10:
                for side in (-1, 1):
                    p.fill(Path.ellipse(x + side * 8, y + 1, 5, 2.4, rot=side * 25), lt, "base")
                c = tones(colours[k % len(colours)])
                for j in range(5):
                    an = math.radians(j * 72 - 90)
                    p.fill(Path.circle(x + math.cos(an) * 3.4, y - 2 + math.sin(an) * 3.4, 2.6), c, "base")
                p.fill(Path.circle(x, y - 2, 1.6), tones("flower.yellow"), "base")
            x += step
            k += 1
        layer.place(p, reach=0)


def herbs(layer, x, top, n=3, seed=0, kind="herbs"):
    """Bundles of herbs (or strings of onions and garlic) hung from a beam to dry."""
    p = Piece(x - 20 * n - 20, top - 4, x + 20 * n + 20, top + 120)
    for i in range(n):
        hx = x + (i - (n - 1) / 2) * 30
        L = 30 + 30 * hash01(i, 1, seed + 2021)
        p.stroke(Path.polyline([(hx, top), (hx, top + L)]), 1.2, tones("rope"), "deep")
        if kind == "onions":
            for k in range(4):
                c = tones("#e2b06a" if (i + k) % 2 else "#f2ead8")
                bm = p.fill(Path.ellipse(hx + (k % 2 - 0.5) * 8, top + L + 8 + k * 13, 8, 7.5), c, "base")
                p.tone(bm - bm.shift(-1.6, -1.6), c, "shadow")
                p.edge(Path.ellipse(hx + (k % 2 - 0.5) * 8, top + L + 8 + k * 13, 8, 7.5), c, 0.9)
        else:
            t = tones(("leaf", "leaf.yellow", "leaf.blue")[i % 3])
            ty = top + L
            for k in range(-2, 3):
                ex, ey = hx + k * 7, ty + 46 - 5 * abs(k)
                p.stroke(Path.polyline([(hx, ty), (ex, ey)]), 1.4, t, "shadow")
                for j in range(4):
                    u = 0.32 + j * 0.2
                    lx, ly = hx + (ex - hx) * u, ty + (ey - ty) * u
                    side = 1 if (j + k) % 2 else -1
                    lm = p.fill(Path.ellipse(lx + side * 3.5, ly, 4.6, 2.4, rot=side * 55), t, "base")
                    p.tone(lm - lm.shift(-0.8, -0.8), t, "shadow")
                if hash01(i, k, seed + 2023) < 0.3:
                    fm = p.fill(Path.circle(ex, ey + 2, 2.6), tones(("flower.violet", "flower.yellow", "flower.white")[i % 3]), "base")
                    del fm
            p.fill(Path.rect(hx - 5, ty - 3, 10, 6, r=1.5), tones("cloth.red"), "base")
    layer.place(p)


def chimney_breast(layer, cx, w, floor_y, top=0, material="stone.warm", seed=0):
    """A chimney breast standing out from the wall, floor to ceiling, behind a hearth (the game's
    prop stands in front of it): coursed stone or brick, its right side in shadow, its left lit."""
    p = Piece(cx - w / 2 - 6, top, cx + w / 2 + 14, floor_y + 6)
    m = P.wall(p, cx - w / 2, top, cx + w / 2, floor_y, material, seed, quoins=False)
    t = tones(material if material in P.PALETTE else "stone")
    p.tone(p.where(m, lambda xx, yy: xx > cx + w / 2 - 16), t, "shadow")
    p.tone(p.where(m, lambda xx, yy: xx < cx - w / 2 + 8), t, "light")
    side = Path.rect(cx + w / 2, top, 10, floor_y - top)
    p.colour(p.shape(side), P.SHADOW_COLOUR, alpha=0.3)
    soot = Path.smooth([(cx - w * 0.25, floor_y - 300), (cx - w * 0.1, floor_y - 420), (cx + w * 0.12, floor_y - 440), (cx + w * 0.26, floor_y - 300)], closed=True)
    p.fill(soot, t, "terminator", alpha=0.45, clip=m)
    p.edge(Path.rect(cx - w / 2, top, w, floor_y - top), t, 1.3)
    layer.place(p)


def hood(layer, cx, bottom, w_bottom, w_flue, flare=170, top=0, material="brick", seed=0, glow=True):
    """A forge's (or an oven's) hood: a wide brick hood over the hearth narrowing to its flue and
    up through the ceiling, an iron lip along its foot, soot on it, the fire's glow under it."""
    p = Piece(cx - w_bottom / 2 - 20, top, cx + w_bottom / 2 + 20, bottom + 30)
    shape = Path.polygon([(cx - w_bottom / 2, bottom), (cx - w_flue / 2, bottom - flare), (cx - w_flue / 2, top),
                          (cx + w_flue / 2, top), (cx + w_flue / 2, bottom - flare), (cx + w_bottom / 2, bottom)])
    m = P.wall(p, cx - w_bottom / 2, top, cx + w_bottom / 2, bottom, material, seed, quoins=False)
    p.c.erase((m - p.shape(shape)).a)
    m = p.shape(shape)
    t = tones(material if material in P.PALETTE else "brick")
    p.tone(p.where(m, lambda xx, yy: xx > cx + w_flue * 0.3), t, "shadow", alpha=0.8)
    p.fill(Path.polygon([(cx - w_bottom / 2, bottom), (cx - w_flue / 2, bottom - flare), (cx - w_flue / 2 + 10, bottom - flare), (cx - w_bottom / 2 + 16, bottom)]), t, "light", clip=m, alpha=0.6)
    soot = Path.smooth([(cx - w_bottom * 0.3, bottom), (cx - w_flue * 0.4, bottom - flare * 0.8), (cx, bottom - flare * 1.3), (cx + w_flue * 0.4, bottom - flare * 0.8), (cx + w_bottom * 0.3, bottom)], closed=True)
    p.fill(soot, tones("#3a2a30"), "base", alpha=0.35, clip=m)
    lip = Path.rect(cx - w_bottom / 2 - 10, bottom - 4, w_bottom + 20, 14, r=2)
    iron = tones("iron")
    lm = p.fill(lip, iron, "base")
    p.tone(lm - lm.shift(0, 2), iron, "light")
    for k in range(int(w_bottom / 30)):
        p.fill(Path.circle(cx - w_bottom / 2 + 14 + k * 30, bottom + 3, 1.8), iron, "highlight")
    p.edge(lip, iron, 1.1)
    p.edge(shape, t, 1.3)
    if glow:
        p.glow(cx, bottom + 40, w_bottom * 0.9, "#ff8a3a", 0.3)
    layer.place(p)


def pilaster(layer, x, w, top, floor_y, material="stone", seed=0, trim="gold"):
    """A pilaster standing out of the wall from the floor to the ceiling: a moulded base, a fluted
    shaft lit on its left, a capital with a gilt band."""
    p = Piece(x - w / 2 - 14, top, x + w / 2 + 14, floor_y + 6)
    t = tones(material)
    shaft = Path.rect(x - w / 2, top + 30, w, floor_y - top - 66)
    m = p.fill(shaft, t, "base")
    for k in range(1, 5):
        fx = x - w / 2 + w * k / 5
        p.stroke(Path.polyline([(fx, top + 40), (fx, floor_y - 46)]), 2.2, t, "shadow", clip=m)
        p.stroke(Path.polyline([(fx - 2.5, top + 40), (fx - 2.5, floor_y - 46)]), 1.0, t, "light", clip=m, alpha=0.7)
    p.tone(p.where(m, lambda xx, yy: xx > x + w / 2 - 8), t, "terminator")
    p.tone(p.where(m, lambda xx, yy: xx < x - w / 2 + 5), t, "light")
    p.edge(shaft, t, 1.2)
    for (y0, h, ex) in ((floor_y - 40, 40, 10), (floor_y - 52, 12, 5)):
        b = Path.rect(x - w / 2 - ex, y0, w + ex * 2, h, r=2)
        bm = p.fill(b, t, "base-light")
        p.tone(bm - bm.shift(0, 2.5), t, "light")
        p.tone(p.where(bm, lambda xx, yy: xx > x + w / 2), t, "shadow")
        p.edge(b, t, 1.1)
    cap = Path.rect(x - w / 2 - 12, top + 8, w + 24, 26, r=2)
    cm = p.fill(cap, t, "base-light")
    p.tone(cm - cm.shift(0, 3), t, "light")
    p.tone(p.where(cm, lambda xx, yy: xx > x + w / 2 + 2), t, "shadow")
    p.fill(Path.rect(x - w / 2 - 12, top + 26, w + 24, 5), tones(trim), "base")
    p.edge(cap, t, 1.2)
    layer.place(p)


def vault(layer, xs, spring, crown, material="stone.warm", pointed=False, seed=0, trim=None):
    """The ceiling's arches spanning from pilaster to pilaster at xs: a band of voussoirs from the
    springing line up to the crown and down, the spandrels above them in shadow."""
    t = tones(material)
    for i in range(len(xs) - 1):
        a, b = xs[i], xs[i + 1]
        p = Piece(a - 4, -4, b + 4, spring + 8, pad=4)
        mx = (a + b) / 2
        band = 26

        def arch(off):
            if pointed:
                return Path().move(a + off, spring).quad(a + off, crown + off * 0.6, mx, crown + off).quad(b - off, crown + off * 0.6, b - off, spring)
            return Path().move(a + off, spring).cubic(a + off, crown + off - (spring - crown) * 0.1, b - off, crown + off - (spring - crown) * 0.1, b - off, spring)
        outer, inner = arch(0), arch(band)
        span = Path()
        for q in outer.subs[0][0]:
            span = span.line(*q) if span.subs else span.move(*q)
        for q in reversed(inner.subs[0][0]):
            span.line(*q)
        span.close()
        spandrel = Path().move(a, -4).line(b, -4).line(b, spring)
        for q in reversed(outer.subs[0][0]):
            spandrel.line(*q)
        spandrel.close()
        sm = p.fill(spandrel, t, "shadow")
        p.tone(sm, t, "terminator", alpha=0.5)
        m = p.fill(span, t, "base-light")
        pts = outer.subs[0][0]
        ipts = inner.subs[0][0]
        n = 11
        for k in range(1, n):
            q0 = pts[int(len(pts) * k / n)]
            q1 = ipts[int(len(ipts) * k / n)]
            p.stroke(Path.polyline([q0, q1]), 1.1, t, "shadow", clip=m)
        p.tone(m - m.shift(0, 2.5), t, "light")
        if trim:
            p.stroke(inner, 2.2, tones(trim), "base")
        p.edge(span, t, 1.1)
        layer.place(p, reach=1.6)


def rose_window(layer, cx, cy, r, colours=("cloth.blue", "cloth.pink", "gold", "#7fd0ff"), seed=0, petals=12, glow=0.3):
    """A round window of stained glass: a stone ring, petals of coloured glass round a gold sun at
    the middle, the leading and the tracery dark, the light coming through it."""
    p = Piece(cx - r - 24, cy - r - 24, cx + r + 24, cy + r + 24)
    st = tones("stone")
    ring = Path.circle(cx, cy, r + 16)
    rm = p.fill(ring, st, "base-light")
    p.tone(rm - rm.shift(-2.5, -2.5), st, "shadow")
    for k in range(petals * 2):
        a = math.radians(k * 180 / petals)
        p.stroke(Path.polyline([(cx + math.cos(a) * r, cy + math.sin(a) * r), (cx + math.cos(a) * (r + 16), cy + math.sin(a) * (r + 16))]), 1.0, st, "shadow", clip=rm)
    p.edge(ring, st, 1.3, "terminator")
    disc = p.shape(Path.circle(cx, cy, r))
    p.tone(disc, tones("#2f3f80"), "base")
    for k in range(petals):
        a = 2 * math.pi * k / petals
        pc = Path.ellipse(cx + math.cos(a) * r * 0.58, cy + math.sin(a) * r * 0.58, r * 0.36, r * 0.17, rot=math.degrees(a))
        c = tones(colours[k % len(colours)])
        pm = p.fill(pc, c, "light", clip=disc)
        p.tone(pm - pm.shift(-2, -2), c, "base")
    for k in range(petals):
        a = 2 * math.pi * (k + 0.5) / petals
        c = tones(colours[(k + 1) % len(colours)])
        p.fill(Path.circle(cx + math.cos(a) * r * 0.86, cy + math.sin(a) * r * 0.86, r * 0.1), c, "light", clip=disc)
    lead = tones("#2a2236")
    for k in range(petals):
        a = 2 * math.pi * (k + 0.5) / petals
        p.stroke(Path.polyline([(cx + math.cos(a) * r * 0.3, cy + math.sin(a) * r * 0.3), (cx + math.cos(a) * r, cy + math.sin(a) * r)]), 2.6, lead, "base", clip=disc)
    p.stroke(Path.circle(cx, cy, r * 0.74), 2.4, lead, "base", clip=disc)
    p.stroke(Path.circle(cx, cy, r * 0.3), 3.0, lead, "base", clip=disc)
    sun = Path.circle(cx, cy, r * 0.26)
    p.fill(sun, tones("gold"), "light")
    P._icon(p, "sun", cx, cy, r * 0.46)
    p.edge(Path.circle(cx, cy, r), lead, 2.4, "base")
    if glow:
        p.glow(cx, cy, r * 2.6, "#fff0d0", glow)
    layer.place(p)


def _lancet_path(x, top, w, h):
    return Path().move(x, top + h).line(x, top + w * 0.75).quad(x, top + w * 0.1, x + w / 2, top) \
        .quad(x + w, top + w * 0.1, x + w, top + w * 0.75).line(x + w, top + h).close()


def lancet(layer, cx, top, w=70, h=230, colours=("cloth.blue", "#7fd0ff", "gold", "cloth.pink"), seed=0, glow=0.22):
    """A tall pointed window of stained glass: a stone surround, a lattice of diamond panes in
    colours, a gold sun near its head, the light through it."""
    x = cx - w / 2
    p = Piece(x - 26, top - 30, x + w + 26, top + h + 24)
    st = tones("stone")
    sur = _lancet_path(x - 12, top - 14, w + 24, h + 18)
    sm = p.fill(sur, st, "base-light")
    p.tone(sm - sm.shift(-2.5, -2.5), st, "shadow")
    p.edge(sur, st, 1.3, "terminator")
    glass = _lancet_path(x, top, w, h)
    gm = p.shape(glass)
    p.tone(gm, tones("#3a4c92"), "base")
    step = w / 3.0
    k = 0
    for row in range(int(h / step * 2) + 2):
        for col in range(5):
            dx = x + (col - 0.5 + (row % 2) * 0.5) * step
            dy = top + row * step / 2
            d = Path.polygon([(dx, dy - step / 2), (dx + step / 2, dy), (dx, dy + step / 2), (dx - step / 2, dy)])
            c = tones(colours[int(hash01(row, col, seed + 2031) * len(colours))])
            p.fill(d, c, "light" if hash01(row, col + 9, seed + 2031) < 0.5 else "base", clip=gm, alpha=0.9)
            k += 1
    lead = tones("#2a2236")
    lines = Path()
    span = h + w
    xx = x - span
    while xx < x + w + span:
        lines.add(Path.polyline([(xx, top), (xx + span, top + span)]))
        lines.add(Path.polyline([(xx + span, top), (xx, top + span)]))
        xx += step
    p.stroke(lines, 1.8, lead, "base", clip=gm)
    sun = Path.circle(cx, top + w * 0.62, w * 0.22)
    p.fill(sun, tones("gold"), "light", clip=gm)
    p.stroke(sun, 2.0, lead, "base")
    p.stroke(Path.polyline([(x + 4, top + h - 6), (x + w * 0.6, top + w * 0.5)]), 3.0, tones("#ffffff"), "base", clip=gm, alpha=0.18)
    p.edge(glass, lead, 2.4, "base")
    sill = Path.rect(x - 14, top + h, w + 28, 9, r=2)
    p.fill(sill, st, "light")
    p.edge(sill, st, 1.0, "shadow")
    if glow:
        p.glow(cx, top + h * 0.5, w * 2.6, "#fff0d0", glow)
    layer.place(p)


def canopy(layer, cx, top, w, floor_y, colour="cloth.navy", trim="gold", emblem="sun"):
    """A cloth of estate behind a throne: a tester with a scalloped, fringed valance at the top and
    the cloth hanging from it to the floor in folds, gold-bordered, a great gold sun on it."""
    p = Piece(cx - w / 2 - 40, top - 20, cx + w / 2 + 40, floor_y + 8)
    t = tones(colour)
    g = tones(trim)
    cloth = Path.rect(cx - w / 2, top + 40, w, floor_y - top - 40)
    m = p.fill(cloth, t, "base")
    n = 6
    for k in range(n):
        fx = cx - w / 2 + w * (k + 0.5) / n
        p.fill(Path.rect(fx + 8, top + 40, w / n * 0.3, floor_y - top - 40), t, "shadow", clip=m)
        p.fill(Path.rect(fx - w / n * 0.35, top + 40, 6, floor_y - top - 40), t, "light", clip=m, alpha=0.6)
    border = m - m.inset(8)
    p.tone(border, g, "base")
    p.tone(border - border.shift(1.5, 1.5), g, "light")
    big = w * 0.62
    sy = top + 40 + big * 0.62
    for k in range(16):
        a = math.radians(k * 22.5)
        L = big * (0.5 if k % 2 == 0 else 0.38)
        p.fill(Path.polygon([(cx + math.cos(a) * L, sy + math.sin(a) * L), (cx + math.cos(a + 0.2) * big * 0.2, sy + math.sin(a + 0.2) * big * 0.2),
                             (cx + math.cos(a - 0.2) * big * 0.2, sy + math.sin(a - 0.2) * big * 0.2)]), g, "base" if k % 2 == 0 else "light", clip=m)
    sm = p.fill(Path.circle(cx, sy, big * 0.22), g, "light")
    p.tone(sm - sm.shift(-2, -2), g, "base")
    p.edge(Path.circle(cx, sy, big * 0.22), g, 1.2)
    p.edge(cloth, t, 1.3)
    tester = Path.rect(cx - w / 2 - 30, top, w + 60, 44, r=3)
    tm = p.fill(tester, t, "base")
    p.tone(p.above(tm, top + 10), t, "light")
    p.fill(Path.rect(cx - w / 2 - 30, top, w + 60, 8), g, "base")
    val = Path().move(cx - w / 2 - 30, top + 40)
    k = 10
    for i in range(k):
        a_ = cx - w / 2 - 30 + (w + 60) * i / k
        b_ = cx - w / 2 - 30 + (w + 60) * (i + 1) / k
        val.quad((a_ + b_) / 2, top + 74, b_, top + 40)
    val.close()
    vm = p.fill(val, t, "base")
    p.tone(p.below(vm, top + 54), t, "shadow")
    fringe = p.shape(val) - p.shape(val.translate(0, -7))
    p.tone(fringe, g, "base")
    p.edge(tester, t, 1.2)
    for ex in (cx - w / 2 - 30, cx + w / 2 + 30):
        P._finial(p, ex, top, g)
    layer.place(p)


def arms(layer, x, y, s=1.0, colour="cloth.navy", emblem="sun"):
    """A heater shield on two crossed swords, hung on the wall."""
    p = Piece(x - 90 * s, y - 80 * s, x + 90 * s, y + 90 * s)
    steel = tones("#c9cfe0")
    hilt = tones("brass")
    for side in (-1, 1):
        a = (x - side * 70 * s, y + 70 * s)
        b = (x + side * 64 * s, y - 64 * s)
        blade = Path.capsule(a, b, 4.5 * s, 3 * s)
        bm = p.fill(blade, steel, "base")
        p.tone(bm - bm.shift(1.2, 1.2), steel, "light")
        p.edge(blade, steel, 1.0)
        gx, gy = x - side * 52 * s, y + 52 * s
        guard = Path.capsule((gx - 14 * s, gy - 14 * s * side * -1), (gx + 14 * s, gy + 14 * s * side * -1), 3.5 * s)
        p.fill(guard, hilt, "base")
        grip = Path.capsule((gx, gy), (x - side * 72 * s, y + 72 * s), 4 * s)
        p.fill(grip, tones("wood.dark"), "base")
        p.fill(Path.circle(x - side * 76 * s, y + 76 * s, 5.5 * s), hilt, "light")
    t = tones(colour)
    sh = Path().move(x - 42 * s, y - 50 * s).line(x + 42 * s, y - 50 * s).line(x + 42 * s, y + 4 * s) \
        .quad(x + 40 * s, y + 46 * s, x, y + 66 * s).quad(x - 40 * s, y + 46 * s, x - 42 * s, y + 4 * s).close()
    m = p.fill(sh, t, "base")
    p.tone(p.where(m, lambda xx, yy: xx > x + 10 * s), t, "shadow")
    rim = m - m.inset(5 * s)
    p.tone(rim, tones("gold"), "base")
    p.tone(rim - rim.shift(1.5, 1.5), tones("gold"), "light")
    P._icon(p, emblem, x, y + 4 * s, 44 * s)
    p.edge(sh, t, 1.3)
    layer.place(p)


def tool_board(layer, x0, x1, y, seed=0):
    """A board of pegs on the wall with the smith's tools hung on it: hammers, tongs, a file, a
    poker, each iron with a wooden handle."""
    p = Piece(x0 - 10, y - 20, x1 + 10, y + 130)
    wood = tones("wood")
    iron = tones("iron")
    board = Path.rect(x0, y, x1 - x0, 16, r=2)
    bm = p.fill(board, wood, "base")
    p.tone(bm - bm.shift(0, 2), wood, "light")
    p.edge(board, wood, 1.0)
    x = x0 + 18
    i = 0
    while x < x1 - 16:
        kind = ("hammer", "tongs", "hammer", "file", "tongs", "poker")[i % 6]
        p.fill(Path.circle(x, y + 8, 3), tones("wood.dark"), "base")
        if kind == "hammer":
            L = 70 + 14 * hash01(i, 1, seed + 2041)
            hd = Path.rect(x - 4, y + 10, 8, L, r=2)
            p.fill(hd, wood, "base")
            p.edge(hd, wood, 0.9)
            head = Path.rect(x - 16, y + 10 + L - 4, 32, 18, r=3)
            hm = p.fill(head, iron, "base")
            p.tone(hm - hm.shift(0, 2), iron, "light")
            p.edge(head, iron, 1.0)
            x += 40
        elif kind == "tongs":
            for side in (-1, 1):
                p.stroke(Path.smooth([(x, y + 10), (x + side * 4, y + 40), (x + side * 3, y + 80), (x + side * 9, y + 104)]), 3.2, iron, "base")
            p.fill(Path.circle(x, y + 30, 3.4), iron, "light")
            x += 30
        elif kind == "file":
            f = Path.rect(x - 4, y + 12, 8, 80, r=2)
            fm = p.fill(f, iron, "base-light")
            for k in range(8):
                p.stroke(Path.polyline([(x - 4, y + 40 + k * 6), (x + 4, y + 37 + k * 6)]), 0.8, iron, "shadow", clip=fm)
            p.fill(Path.rect(x - 5, y + 10, 10, 26, r=2), wood, "base")
            p.edge(f, iron, 0.9)
            x += 26
        else:
            p.stroke(Path.polyline([(x, y + 10), (x, y + 110)]), 4.0, iron, "base")
            p.stroke(Path.polyline([(x, y + 110), (x + 10, y + 118)]), 4.0, iron, "base")
            p.stroke(Path.circle(x, y + 16, 6), 2.4, iron, "base")
            x += 30
        i += 1
    layer.place(p)


def bellows(layer, x, y, s=1.0):
    """A great pair of bellows hung on the wall: leather between two boards, an iron nozzle."""
    p = Piece(x - 70 * s, y - 60 * s, x + 90 * s, y + 70 * s)
    wood = tones("wood")
    leather = tones("#8a5a44")
    body = Path.smooth([(x - 50 * s, y - 30 * s), (x + 10 * s, y - 40 * s), (x + 50 * s, y - 6 * s), (x + 50 * s, y + 6 * s), (x + 10 * s, y + 40 * s), (x - 50 * s, y + 30 * s)], closed=True)
    m = p.fill(body, leather, "base")
    for k in range(1, 4):
        p.stroke(Path.smooth([(x - 46 * s, y - 30 * s + k * 15 * s), (x, y - 26 * s + k * 13 * s), (x + 46 * s, y - 2 * s + k * 1.5 * s)]), 1.4, leather, "deep", clip=m)
    p.tone(p.below(m, y + 6 * s), leather, "shadow")
    for sgn in (-1, 1):
        board = Path.polygon([(x - 60 * s, y + sgn * 30 * s), (x + 10 * s, y + sgn * 44 * s), (x + 44 * s, y + sgn * 8 * s), (x - 60 * s, y + sgn * 22 * s)])
        bm = p.fill(board, wood, "base")
        p.tone(bm - bm.shift(1.5, 1.5), wood, "light")
        p.edge(board, wood, 1.0)
    noz = Path.polygon([(x + 44 * s, y - 6 * s), (x + 84 * s, y - 2 * s), (x + 84 * s, y + 2 * s), (x + 44 * s, y + 6 * s)])
    p.fill(noz, tones("iron"), "base")
    p.edge(noz, tones("iron"), 1.0)
    p.fill(Path.rect(x - 68 * s, y - 6 * s, 14 * s, 12 * s, r=3), wood, "base")
    layer.place(p)


def horseshoes(layer, x, y, n=5, r=80):
    """Horseshoes nailed up in an arch over the forge, for luck."""
    p = Piece(x - r - 30, y - r - 30, x + r + 30, y + 30)
    iron = tones("iron")
    for i in range(n):
        a = math.pi + math.pi * (i + 0.5) / n
        hx, hy = x + math.cos(a) * r, y + math.sin(a) * r * 0.6
        shoe = Path().arc(hx, hy, 11, -200, 20)
        p.stroke(shoe, 5.0, iron, "base")
        p.stroke(Path().arc(hx - 1, hy - 1, 11, -170, -60), 1.4, iron, "light")
    layer.place(p)


def ship_wheel(layer, x, y, r=56, colour="wood"):
    """A ship's wheel hung on the wall: rim, eight spokes running out to handles, a brass hub."""
    p = Piece(x - r - 30, y - r - 30, x + r + 30, y + r + 30)
    t = tones(colour)
    for k in range(8):
        a = math.radians(k * 45 + 10)
        p.stroke(Path.polyline([(x, y), (x + math.cos(a) * (r + 18), y + math.sin(a) * (r + 18))]), 5.0, t, "base")
        p.fill(Path.circle(x + math.cos(a) * (r + 20), y + math.sin(a) * (r + 20), 5.5), t, "light")
    ring = p.shape(Path.circle(x, y, r)) - p.shape(Path.circle(x, y, r - 10))
    p.tone(ring, t, "base")
    p.tone(ring - ring.shift(2, 2), t, "light")
    p.tone(ring - ring.shift(-2, -2), t, "shadow")
    p.edge_mask(ring, t, 1.0)
    hub = Path.circle(x, y, 13)
    p.fill(hub, tones("brass"), "base")
    p.fill(Path.circle(x - 3, y - 3, 4), tones("brass"), "highlight")
    p.edge(hub, tones("brass"), 1.0)
    layer.place(p)


def anchor(layer, x, y, s=1.0):
    """An old iron anchor hung on the wall, a rope through its ring."""
    p = Piece(x - 80 * s, y - 100 * s, x + 80 * s, y + 90 * s)
    iron = tones("iron")
    shank = Path.rect(x - 7 * s, y - 70 * s, 14 * s, 140 * s, r=4)
    p.fill(shank, iron, "base")
    stock = Path.rect(x - 50 * s, y - 56 * s, 100 * s, 12 * s, r=5)
    p.fill(stock, tones("wood.dark"), "base")
    arm = Path().arc(x, y + 20 * s, 56 * s, 20, 160)
    p.stroke(arm, 12 * s, iron, "base")
    for side in (-1, 1):
        a = math.radians(90 - side * 70)
        tx, ty = x + math.cos(a) * 56 * s, y + 20 * s + math.sin(a) * 56 * s
        p.fill(Path.polygon([(tx - side * 4 * s, ty - 22 * s), (tx + side * 14 * s, ty + 2 * s), (tx - side * 8 * s, ty + 8 * s)]), iron, "base")
    p.stroke(Path.circle(x, y - 82 * s, 10 * s), 4.0 * s, iron, "base")
    p.stroke(Path.smooth([(x - 30 * s, y - 100 * s), (x, y - 86 * s), (x + 34 * s, y - 100 * s)]), 4.0, tones("rope"), "base")
    for m_ in (shank, stock):
        p.edge(m_, iron, 1.1)
    layer.place(p)


def mounted_fish(layer, x, y, L=120):
    """A prize fish mounted on an oval board."""
    p = Piece(x - L * 0.8, y - L * 0.5, x + L * 0.8, y + L * 0.5)
    board = Path.ellipse(x, y, L * 0.7, L * 0.34)
    bt = tones("wood.dark")
    bm = p.fill(board, bt, "base")
    p.tone(bm - bm.shift(2, 2), bt, "light")
    p.edge(board, bt, 1.2)
    import paint_rooms_docks as D
    D.fish(p, x, y, L, -6, "#8fb7a8")
    layer.place(p)


def notices(layer, x, y, n=3, seed=0):
    """Notices pinned to the wall: curling paper with a drawing or lines of writing, a red seal."""
    p = Piece(x - 70, y - 60, x + 70 + 40 * n, y + 90)
    paper = tones("#f2e7cf")
    for i in range(n):
        px = x + i * 52 + (hash01(i, 1, seed + 2051) - 0.5) * 10
        py = y + (hash01(i, 2, seed + 2051) - 0.5) * 20
        w, h = 46, 60
        sheet = Path.polygon([(px - w / 2, py - h / 2), (px + w / 2, py - h / 2 + 3), (px + w / 2 - 2, py + h / 2), (px - w / 2 + 2, py + h / 2 - 2)])
        sm = p.fill(sheet, paper, "base")
        p.tone(sm - sm.shift(-1.5, -1.5), paper, "shadow")
        if i % 2 == 0:
            p.fill(Path.ellipse(px, py - 6, 10, 12), tones("#c9a888"), "base", clip=sm)
            p.fill(Path.ellipse(px, py - 10, 5, 6), tones("#8a6a50"), "base", clip=sm)
        for k in range(4):
            yy = py + (8 if i % 2 == 0 else -18) + k * 6
            p.stroke(Path.polyline([(px - 16, yy), (px + 10 + 6 * hash01(i, k, seed + 2053), yy)]), 1.0, tones("#5a4a5a"), "base", clip=sm)
        p.fill(Path.circle(px, py - h / 2 + 4, 3), tones("cloth.red"), "base")
        p.edge(sheet, paper, 0.9, "shadow")
    layer.place(p)


def rope_hanks(layer, x0, x1, y, seed=0):
    """Hanks of rope hung on pegs along the wall: each a coil of long loops, bound at the top."""
    p = Piece(x0 - 40, y - 20, x1 + 40, y + 150)
    i = 0
    x = x0
    while x <= x1:
        t = tones(("rope", "sack", "#b88a5a")[i % 3])
        L = 96 + 24 * hash01(i, 1, seed + 2061)
        for k in range(4):
            loop = Path.ellipse(x + (k - 1.5) * 2.5, y + 8 + L / 2 + k * 2, 20 - k * 1.5, L / 2)
            p.stroke(loop, 4.6, t, "base" if k % 2 else "base-light")
            p.stroke(loop, 1.0, t, "deep")
        p.fill(Path.rect(x - 9, y - 6, 18, 12, r=3), tones("wood.dark"), "base")
        bind = Path.rect(x - 14, y + 12, 28, 14, r=3)
        bm = p.fill(bind, t, "terminator")
        for k in range(4):
            p.stroke(Path.polyline([(x - 14, y + 14 + k * 3.4), (x + 14, y + 12 + k * 3.4)]), 0.9, t, "deep", clip=bm)
        p.edge(bind, t, 1.0)
        x += 66
        i += 1
    layer.place(p)


def bread_shelf(layer, x, y, w=260, seed=0):
    """A shelf of bread on two brackets: round loaves scored across, long loaves, rolls."""
    p = Piece(x - w / 2 - 10, y - 60, x + w / 2 + 10, y + 40)
    t = tones("wood")
    board = Path.rect(x - w / 2, y, w, 9)
    bm = p.fill(board, t, "base")
    p.tone(bm - bm.shift(0, 2), t, "light")
    p.edge(board, t, 1.1)
    for bx in (x - w / 2 + 16, x + w / 2 - 24):
        br = Path.polygon([(bx, y + 9), (bx + 8, y + 9), (bx + 8, y + 32), (bx + 2, y + 32)])
        p.fill(br, t, "shadow")
        p.edge(br, t, 1.0)
    bt = tones("bread")
    cx = x - w / 2 + 16
    i = 0
    while cx < x + w / 2 - 20:
        kind = int(hash01(i, 1, seed + 2071) * 3)
        if kind == 0:
            r = 22
            loaf = Path().move(cx, y).quad(cx, y - r * 1.2, cx + r, y - r * 1.2).quad(cx + 2 * r, y - r * 1.2, cx + 2 * r, y).close()
            m = p.fill(loaf, bt, "base")
            p.tone(m - m.shift(-2.4, -3), bt, "shadow")
            p.tone(m & p.shape(Path.ellipse(cx + r * 0.7, y - r * 0.9, r * 0.5, r * 0.25)), bt, "light")
            for k in range(3):
                p.stroke(Path.polyline([(cx + r * 0.5 + k * r * 0.4, y - r * 0.5), (cx + r * 0.75 + k * r * 0.4, y - r * 1.05)]), 1.4, bt, "deep-shadow", clip=m)
            p.edge(loaf, bt, 1.1)
            cx += 2 * r + 8
        elif kind == 1:
            L = 62
            loaf = Path.ellipse(cx + L / 2, y - 9, L / 2, 9, rot=-4)
            m = p.fill(loaf, tones("#d29350"), "base")
            p.tone(m - m.shift(-1.6, -2), tones("#d29350"), "shadow")
            for k in range(4):
                p.stroke(Path.polyline([(cx + 10 + k * 12, y - 6), (cx + 16 + k * 12, y - 14)]), 1.2, tones("#d29350"), "deep-shadow", clip=m)
            p.edge(loaf, tones("#d29350"), 1.0)
            cx += L + 8
        else:
            for k in range(3):
                rm = p.fill(Path.ellipse(cx + 10 + k * 18, y - 8, 9, 8), bt, "base-light")
                p.tone(rm - rm.shift(-1.4, -1.6), bt, "shadow")
                p.edge(Path.ellipse(cx + 10 + k * 18, y - 8, 9, 8), bt, 0.9)
            cx += 62
        i += 1
    layer.place(p)


def wheat(layer, x, y, s=1.0):
    """A sheaf of wheat tied with a ribbon, hung on the wall."""
    p = Piece(x - 60 * s, y - 80 * s, x + 60 * s, y + 80 * s)
    t = tones("#e6bf5c")
    tie = y + 12 * s
    ears = []
    for k in range(-5, 6):
        tx, ty = x + k * 7.5 * s, y - 44 * s + abs(k) * 3.5 * s
        bx, by = x + k * 4 * s, y + 66 * s - abs(k) * 2 * s
        p.stroke(Path.polyline([(bx, by), (x + k * 0.9 * s, tie), (tx, ty + 12 * s)]), 1.8 * s, t, "shadow")
        ears.append((tx, ty, k))
    for (tx, ty, k) in sorted(ears, key=lambda e: abs(e[2]), reverse=True):
        ear = Path.ellipse(tx, ty, 4.6 * s, 13 * s, rot=k * 5)
        em = p.fill(ear, t, "base")
        p.tone(em - em.shift(-1.2, -1.2), t, "shadow")
        p.tone(em - em.shift(1.0, 1.0), t, "light")
        for j in range(3):
            p.stroke(Path.polyline([(tx - 3 * s, ty - 6 * s + j * 6 * s), (tx + 3 * s, ty - 9 * s + j * 6 * s)]), 0.8, t, "deep", clip=em)
        p.stroke(Path.polyline([(tx, ty - 12 * s), (tx + k * 1.2 * s, ty - 26 * s)]), 0.8, t, "shadow")
    p.fill(Path.rect(x - 10 * s, tie - 5 * s, 20 * s, 10 * s, r=3), tones("cloth.red"), "base")
    p.fill(Path.polygon([(x, tie), (x - 18 * s, tie + 22 * s), (x - 9 * s, tie + 25 * s)]), tones("cloth.red"), "shadow")
    p.fill(Path.polygon([(x, tie), (x + 16 * s, tie + 24 * s), (x + 7 * s, tie + 26 * s)]), tones("cloth.red"), "base")
    layer.place(p, reach=1.6)


def ship_model(layer, x, shelf_y, L=110):
    """A model ship on a bracket shelf: a little hull, three masts and set sails."""
    p = Piece(x - L, shelf_y - L * 1.2, x + L, shelf_y + 30)
    t = tones("wood")
    board = Path.rect(x - L * 0.7, shelf_y, L * 1.4, 8)
    p.fill(board, t, "base")
    p.edge(board, t, 1.0)
    for bx in (x - L * 0.5, x + L * 0.5 - 8):
        p.fill(Path.polygon([(bx, shelf_y + 8), (bx + 8, shelf_y + 8), (bx + 8, shelf_y + 26), (bx + 2, shelf_y + 26)]), t, "shadow")
    hull = Path().move(x - L * 0.5, shelf_y - L * 0.22).quad(x, shelf_y - L * 0.12, x + L * 0.42, shelf_y - L * 0.26).line(x + L * 0.4, shelf_y - L * 0.06) \
        .quad(x, shelf_y + 2, x - L * 0.36, shelf_y - L * 0.04).close()
    canvas = tones("cloth.cream")
    for k, mx in enumerate((x - L * 0.22, x + L * 0.02, x + L * 0.24)):
        h = L * (0.62 if k == 1 else 0.5)
        p.fill(Path.rect(mx - 1.5, shelf_y - L * 0.2 - h, 3, h), tones("wood.dark"), "base")
        for j in range(2):
            sy = shelf_y - L * 0.2 - h * (0.92 - 0.42 * j)
            sail = Path.polygon([(mx - 13, sy), (mx + 13, sy), (mx + 15, sy + h * 0.36), (mx - 15, sy + h * 0.36)])
            sm = p.fill(sail, canvas, "base")
            p.tone(p.where(sm, lambda xx, yy, a=mx: xx > a + 5), canvas, "shadow")
            p.edge(sail, canvas, 0.9)
    m = p.fill(hull, t, "base")
    p.stroke(Path().move(x - L * 0.48, shelf_y - L * 0.17).quad(x, shelf_y - L * 0.07, x + L * 0.41, shelf_y - L * 0.2), 2.4, tones("cloth.blue"), "base", clip=m)
    p.tone(p.below(m, shelf_y - L * 0.08), t, "shadow")
    p.edge(hull, t, 1.0)
    layer.place(p)


def sacks_stack(layer, x, ground, rows=2, n=4, seed=0):
    """A stack of grain sacks, rows of them heaped up (for a loft)."""
    for r_ in range(rows):
        P.sacks(layer, x + (r_ % 2) * 15, ground - r_ * 34, n - r_, seed=seed + r_)


def gallery(layer, x0, x1, floor_y, ground, posts=(), doors=(), windows=(), wall="plaster", material="wood", seed=0, rail=58, gaps=()):
    """A gallery along the wall: the upper floor's doors and windows behind it, its floor a heavy
    beam with the joists' ends along it, a balustrade of turned balusters under a rail on top, and
    posts with brackets down to the floor at `posts`. `gaps` are (a, b) stretches it doesn't cover
    (where a chimney breast goes up through it)."""
    wall_t = tones(wall)
    up = Piece(x0, floor_y - 200, x1, floor_y + 4)
    for dx in doors:
        P.door(up, Door(dx, 66, 136, "arch", wood="wood", surround="timber", step=False), floor_y, wall_t, seed + int(dx))
    for wx in windows:
        P.window(up, Window(wx, floor_y - 150, 46, 70, "arch", lit=True, panes=2), wall_t, seed + int(wx))
    layer.place(up)
    t = tones(material)
    p = Piece(x0 - 10, floor_y - rail - 16, x1 + 10, ground + 6)
    spans = []
    a = x0
    for (g0, g1) in sorted(gaps):
        spans.append((a, g0))
        a = g1
    spans.append((a, x1))
    for (a, b) in spans:
        under = Path.rect(a, floor_y + 30, b - a, 40)
        p.colour(p.shape(under), P.SHADOW_COLOUR, alpha=0.22)
        beam = Path.rect(a, floor_y, b - a, 30)
        bm = p.fill(beam, t, "base")
        p.tone(bm - bm.shift(0, 3), t, "light")
        p.tone(p.below(bm, floor_y + 22), t, "shadow")
        for k in range(int((b - a) / 40)):
            jx = a + 20 + k * 40
            jm = p.fill(Path.rect(jx - 7, floor_y + 7, 14, 14, r=1.5), t, "terminator")
            p.tone(jm - jm.shift(-1.5, -1.5), t, "deep-shadow")
        p.edge(beam, t, 1.2)
        for k in range(int((b - a) / 22) + 1):
            bx = a + 8 + k * 22
            if bx > b - 6:
                break
            yb, yt = floor_y - 4, floor_y - rail + 10
            bal = Path().move(bx - 4, yb).quad(bx - 9, (yb + yt) / 2 + 6, bx - 3, (yb + yt) / 2).quad(bx - 7, yt + 6, bx - 3, yt) \
                .line(bx + 3, yt).quad(bx + 7, yt + 6, bx + 3, (yb + yt) / 2).quad(bx + 9, (yb + yt) / 2 + 6, bx + 4, yb).close()
            m = p.fill(bal, t, "base-light")
            p.tone(p.where(m, lambda xx, yy, c=bx: xx > c + 1), t, "shadow")
            p.edge(bal, t, 0.9)
        cap = Path.rect(a - 4, floor_y - rail, b - a + 8, 12, r=3)
        cm = p.fill(cap, t, "base")
        p.tone(cm - cm.shift(0, 2.5), t, "light")
        p.edge(cap, t, 1.1)
        sill = Path.rect(a, floor_y - 8, b - a, 8)
        p.fill(sill, t, "terminator")
    for px in posts:
        post = Path.rect(px - 10, floor_y + 30, 20, ground - floor_y - 30)
        pm = p.fill(post, t, "base")
        p.tone(p.where(pm, lambda xx, yy, c=px: xx > c + 4), t, "shadow")
        p.tone(p.where(pm, lambda xx, yy, c=px: xx < c - 6), t, "light")
        p.edge(post, t, 1.2)
        for side in (-1, 1):
            br = Path.polygon([(px + side * 10, floor_y + 30), (px + side * 44, floor_y + 30), (px + side * 10, floor_y + 66)])
            p.fill(br, t, "base" if side < 0 else "shadow")
            p.edge(br, t, 1.0)
        p.ground_shadow(px - 16, px + 16, ground + 1, 5, 0.3)
    layer.place(p)


def stair(layer, x0, x1, bottom, top, material="wood", seed=0, rail=70):
    """A stair rising along the wall from (x0, bottom) to (x1, top): the treads' nosings lit, the
    risers in shadow, the stringer under them, the dark space below, balusters and a handrail, and
    newel posts at both ends."""
    t = tones(material)
    n = max(4, int((bottom - top) / 26))
    rise = (bottom - top) / n
    run = (x1 - x0) / n
    p = Piece(x0 - 20, top - rail - 30, x1 + 20, bottom + 6)
    under = Path.polygon([(x0, bottom), (x1, top + 20), (x1, bottom)])
    p.colour(p.shape(under), P.SHADOW_COLOUR, alpha=0.32)
    saw = Path().move(x0, bottom)
    for k in range(n):
        saw.line(x0 + k * run, bottom - (k + 1) * rise)
        saw.line(x0 + (k + 1) * run, bottom - (k + 1) * rise)
    saw.line(x1, top + 34).line(x0 + 30, bottom).close()
    m = p.fill(saw, t, "shadow")
    for k in range(n):
        tread = Path.rect(x0 + k * run - 3, bottom - (k + 1) * rise - 2, run + 4, 7, r=1.5)
        tm = p.fill(tread, t, "light")
        p.edge(tread, t, 0.9)
        del tm
    stringer = Path.polygon([(x0 + 6, bottom), (x1, top + 6), (x1, top + 36), (x0 + 40, bottom)])
    sm = p.fill(stringer, t, "base")
    p.tone(sm - sm.shift(-1.5, 1.5), t, "light")
    p.edge(stringer, t, 1.1)
    del m
    for k in range(0, n, 1):
        bx = x0 + (k + 0.5) * run
        by = bottom - (k + 1) * rise
        p.fill(Path.rect(bx - 3, by - rail + 8, 6, rail - 8), t, "base-light")
    hand = Path.capsule((x0 + 10, bottom - rise - rail), (x1, top - rail + 4), 5)
    hm = p.fill(hand, t, "base")
    p.tone(hm - hm.shift(0, 2), t, "light")
    p.edge(hand, t, 1.0)
    for (nx, ny) in ((x0 + 10, bottom), (x1, top + 4)):
        newel = Path.rect(nx - 9, ny - rail - 18, 18, rail + 18)
        nm = p.fill(newel, t, "base")
        p.tone(p.where(nm, lambda xx, yy, c=nx: xx > c + 3), t, "shadow")
        p.fill(Path.circle(nx, ny - rail - 22, 9), t, "light")
        p.edge(newel, t, 1.1)
    layer.place(p)


def mezzanine(layer, x0, x1, floor_y, ground, posts=(), seed=0):
    """A warehouse's loft floor: planks on a beam, a plain rail, posts to the floor, and sacks and
    crates stored up on it."""
    for i, sx in enumerate(range(int(x0) + 70, int(x1) - 60, 150)):
        if hash01(i, 1, seed + 2081) < 0.5:
            sacks_stack(layer, sx, floor_y - 4, 2, 3, seed=seed + i)
        else:
            P.crate(layer, sx - 20, floor_y - 2, 54, 46, seed=seed + i)
            P.crate(layer, sx + 34, floor_y - 2, 46, 40, seed=seed + i + 7)
            P.crate(layer, sx + 4, floor_y - 48, 44, 38, seed=seed + i + 9)
    t = tones("wood")
    p = Piece(x0 - 10, floor_y - 80, x1 + 10, ground + 6)
    beam = Path.rect(x0, floor_y, x1 - x0, 28)
    bm = p.fill(beam, t, "base")
    p.tone(bm - bm.shift(0, 3), t, "light")
    for k in range(int((x1 - x0) / 60)):
        p.stroke(Path.polyline([(x0 + 30 + k * 60, floor_y + 4), (x0 + 30 + k * 60, floor_y + 26)]), 1.0, t, "deep", clip=bm)
    p.edge(beam, t, 1.2)
    for yy in (floor_y - 64, floor_y - 34):
        rail = Path.rect(x0, yy, x1 - x0, 8, r=2)
        rm = p.fill(rail, t, "base")
        p.tone(rm - rm.shift(0, 2), t, "light")
        p.edge(rail, t, 1.0)
    for px in list(range(int(x0) + 6, int(x1), 120)) + [int(x1) - 8]:
        p.fill(Path.rect(px - 5, floor_y - 70, 10, 70), t, "base")
    for px in posts:
        post = Path.rect(px - 12, floor_y + 28, 24, ground - floor_y - 28)
        pm = p.fill(post, t, "base")
        p.tone(p.where(pm, lambda xx, yy, c=px: xx > c + 5), t, "shadow")
        p.edge(post, t, 1.2)
        for side in (-1, 1):
            br = Path.capsule((px + side * 6, floor_y + 70), (px + side * 50, floor_y + 32), 5)
            p.fill(br, t, "base" if side < 0 else "shadow")
            p.edge(br, t, 1.0)
        p.ground_shadow(px - 18, px + 18, ground + 1, 5, 0.3)
    layer.place(p)


def hoist_chain(layer, x, top, drop, hook=True):
    """A pulley block on the ceiling beam and its chain down to a hook."""
    p = Piece(x - 40, top - 4, x + 40, top + drop + 40)
    iron = tones("iron")
    block = Path.rect(x - 14, top, 28, 40, r=6)
    bm = p.fill(block, tones("wood.dark"), "base")
    p.tone(bm - bm.shift(-2, -2), tones("wood.dark"), "shadow")
    p.fill(Path.circle(x, top + 20, 9), iron, "base")
    p.edge(block, tones("wood.dark"), 1.0)
    for side in (-6, 6):
        for k in range(int((drop - 40) / 10)):
            p.stroke(Path.ellipse(x + side, top + 44 + k * 10, 2.4, 4.6), 1.2, iron, "base")
    if hook:
        p.stroke(Path.smooth([(x + 6, top + drop), (x + 6, top + drop + 14), (x - 4, top + drop + 22), (x - 10, top + drop + 12)]), 4.0, iron, "base")
    layer.place(p)


def fore_frame(r, posts=(), beam=True, dark=0.55, drape=None):
    """The near side of an interior in the foreground: a dark beam across the top, dark posts at
    `posts`, and (drape, a colour) swags of curtain hanging across the top corners."""
    fore = r["fore"]
    W = fore.w
    if beam:
        p = Piece(-10, -10, W + 10, 50, pad=4)
        t = dim("wood.dark", dark)
        bm = p.fill(Path.rect(-10, -10, W + 20, 46), t, "base")
        p.tone(p.below(bm, 26), t, "shadow")
        p.tone(p.above(bm, 4), t, "light", alpha=0.6)
        for k in range(int(W / 180)):
            p.stroke(Path.polyline([(80 + k * 180, 8), (130 + k * 180, 12)]), (1.4, 0.0), t, "deep")
        fore.place(p, reach=2.4)
    for px in posts:
        p = Piece(px - 40, -10, px + 40, CANVAS_H + 10, pad=4)
        t = dim("wood.dark", dark)
        post = Path.rect(px - 26, -10, 52, CANVAS_H + 20)
        pm = p.fill(post, t, "base")
        p.tone(p.where(pm, lambda xx, yy, c=px: xx > c + 8), t, "shadow")
        p.tone(p.where(pm, lambda xx, yy, c=px: xx < c - 18), t, "light", alpha=0.7)
        for side in (-1, 1):
            br = Path.polygon([(px + side * 26, 36), (px + side * 80, 36), (px + side * 26, 96)])
            p.fill(br, t, "base" if side < 0 else "shadow")
        fore.place(p, reach=2.4)
    if drape:
        t = dim(drape, dark * 0.7)
        for side in (-1, 1):
            x0 = 0 if side < 0 else W
            p = Piece(min(x0, x0 - side * 260) - 10, -10, max(x0, x0 - side * 260) + 10, 420)
            sw = Path().move(x0, -10).line(x0 - side * 250, -10).quad(x0 - side * 170, 90, x0 - side * 60, 120) \
                .quad(x0 - side * 30, 260, x0 - side * 50, 400).line(x0, 400).close()
            m = p.fill(sw, t, "base")
            for k in range(3):
                p.stroke(Path().move(x0 - side * (40 + k * 60), -6).quad(x0 - side * (40 + k * 45), 120, x0 - side * (14 + k * 6), 380), (3.0, 1.0), t, "shadow", clip=m)
            p.fill(Path.ellipse(x0 - side * 52, 126, 14, 8), dim("gold", dark * 0.6), "base")
            fore.place(p, reach=2.4)


# --------------------------------------------------------------------------- #
# Recipes
# --------------------------------------------------------------------------- #

def taproom(r):
    """The Gilded Flagon's taproom (12-rooms §6: interior, a long bar, a fire, a balcony): warm
    yellow plaster over a panelled wainscot under dark beams. The bar end at the left: the inn's
    gilded board over it, two long shelves of bottles over the kegs, a garland of hops, a wheel
    chandelier; a curtained window at the far left. Then the stair up to the balcony, which runs
    east along the wall either side of the great stone chimney breast (the hearth is the game's
    prop, in front of it), the rooms' doors and a window behind its balustrade."""
    g, W = r.ground, r.room.width
    seed = roomkit.seed_of("gilded-flagon", "taproom")
    mid = r["mid"]
    P.back_wall(mid, 0, W, 0, g, "plaster", "yellow", "wood", seed, dado=0.3)
    chimney_breast(mid, 1792, 236, g, 0, "stone.warm", seed + 1)
    window(mid, 120, 170, 84, 130, view="night", curtains="cloth.red", box=True, seed=seed + 2)
    plaque(mid, 720, 92, 330, 64, "tankard")
    for (sx, sy, items) in ((560, 300, ("bottle", "jar", "bottle", "plate")), (880, 300, ("bottle", "bottle", "jar")),
                            (560, 392, ("bottle", "jar", "bottle")), (880, 392, ("jar", "bottle", "plate", "bottle"))):
        P.shelf(mid, sx, sy, 270, items, seed=seed + sx + sy)
    garland(mid, 380, 1080, 194, 20, seed=seed + 3, every=175)
    sconce(mid, 380, 330)
    sconce(mid, 1070, 330)
    picture(mid, 1640, 420, 60, 80, "portrait", seed=seed + 4)
    stair(mid, 1170, 1460, g, 300, "wood", seed=seed + 5)
    gallery(mid, 1460, W, 300, g, posts=(1990, 2410), doors=(1580, 2140), windows=(2340,), wall="plaster.yellow",
            gaps=((1674, 1910),), seed=seed + 6)
    P.beams(mid, 0, W, 0, posts=(), h=30, seed=seed + 7)
    chandelier(mid, 300, 30, 150, 80)
    chandelier(mid, 1310, 30, 130, 76)
    fore_frame(r, posts=(40,))


def anvil_bar(r):
    """The Rusty Anvil (03b §1: a dim dockside bar, Brannock's corner table), kept as it was
    built: low and dark, tall wainscot of tarred boards under stained plaster, heavy low beams on
    posts, the bar at the left with a crooked board over it and a shelf of bottles, a grimy
    window on the night, sailors' junk on the walls (a wheel, an anchor, a prize fish, a net),
    notices pinned up, and one candle sconce lit."""
    g, W = r.ground, r.room.width
    seed = roomkit.seed_of("rusty-anvil", "bar")
    mid = r["mid"]
    P.back_wall(mid, 0, W, 0, g, "plaster", "sack", "wood.dark", seed, dado=0.46)
    plaque(mid, 485, 150, 240, 56, "anvil", board="wood.dark")
    P.shelf(mid, 485, 300, 300, ("bottle", "bottle", "jar", "bottle"), seed=seed + 1)
    window(mid, 1060, 190, 76, 96, view="night", kind="square", panes=2, seed=seed + 2, wall="sack")
    ship_wheel(mid, 1250, 270, 52)
    anchor(mid, 1480, 300, 0.9)
    mounted_fish(mid, 830, 200, 110)
    notices(mid, 640, 430, 2, seed=seed + 3)
    sconce(mid, 970, 380)
    herbs(mid, 160, 40, 3, seed=seed + 4, kind="onions")
    P.beams(mid, 0, W, 0, posts=((90, g), (920, g)), h=40, seed=seed + 5)
    import paint_rooms_docks as D
    netp = Piece(1300, 30, W, 280)
    D.net_drape(1320, W + 20, 40, 150, seed=seed + 6)(netp)
    mid.place(netp, reach=0.8)
    fore_frame(r, posts=(1580,), dark=0.6)


def great_hall(r):
    """Sunspire Keep's great hall (12-rooms §6: the throne, banners, the gallery): a long hall of
    cream ashlar over a blue-grey dado, fluted pilasters where the marble pillars stand, round
    arches springing between them under the ceiling, tall windows of stained glass. The throne at
    the left stands before a cloth of estate with a great gold sun under the sun window; a gallery
    runs along the middle of the hall over the arch; the door out to the terrace is at the east
    end. Lit by its windows (the room isn't dark); gold chandeliers hang down the hall."""
    g, W = r.ground, r.room.width
    seed = roomkit.seed_of("sunspire-keep", "great-hall")
    mid = r["mid"]
    P.back_wall(mid, 0, W, 0, g, "stone", None, "stone.blue", seed, dado=0.18)
    xs = (-20, 791, 1405, 2317, 2931, W + 20)
    vault(mid, xs, 120, 20, "stone", seed=seed + 1, trim="gold")
    rose_window(mid, 520, 150, 74, seed=seed + 2)
    canopy(mid, 520, 250, 300, g, "cloth.navy", "gold")
    for (cx, top) in ((180, 190), (1000, 150), (1200, 150), (2520, 150), (2730, 150)):
        lancet(mid, cx, top, 76, 210, seed=seed + int(cx))
    gallery(mid, 1480, 2240, 300, g, posts=(), doors=(1700, 2040), windows=(1870,), wall="stone", material="stone", seed=seed + 3)
    for x in xs[1:-1]:
        pilaster(mid, x, 64, 110, g, "stone", seed=seed + int(x))
    wall_t = tones("stone")
    dp = Piece(2900, 380, 3100, g + 8)
    rect = P.door(dp, Door(r.door_x("door"), 100, 190, "double", wood="wood.dark", surround="stone"), g, wall_t, seed + 4)
    mid.place(dp)
    r.doorway("door", rect)
    arms(mid, 3000, 330, 0.8)
    chandelier(mid, 1100, 0, 210, 96, metal="gold")
    chandelier(mid, 2620, 0, 210, 96, metal="gold")
    fore_frame(r, beam=False, posts=(1180, 2700), dark=0.5, drape="cloth.navy")


def chapel(r):
    """The Chapel of the Dawn (inside): warm stone, wall shafts carrying pointed arches under the
    roof, a rose window of the dawn's colours over the altar's niche (the altar and its cloth are
    the game's props), tall lancets either side over the hangings, sconces lit."""
    g, W = r.ground, r.room.width
    seed = roomkit.seed_of("chapel", "inside")
    mid = r["mid"]
    P.back_wall(mid, 0, W, 0, g, "stone", "warm", "stone.grey", seed, dado=0.16)
    xs = (-10, 480, 1120, W + 10)
    vault(mid, xs, 150, 24, "stone.warm", pointed=True, seed=seed + 1)
    niche = Piece(640, 300, 960, g + 4)
    st = tones("stone.warm")
    npath = _lancet_path(670, 330, 260, g - 330)
    nm = niche.fill(npath, st, "shadow")
    niche.tone(nm - nm.shift(8, 8), st, "deep-shadow")
    niche.edge(npath, st, 1.4, "terminator")
    sur = _lancet_path(656, 316, 288, g - 316)
    niche.tone(niche.shape(sur) - nm, st, "base-light")
    niche.edge(sur, st, 1.2, "shadow")
    mid.place(niche)
    rose_window(mid, 800, 190, 104, colours=("cloth.pink", "gold", "#ffb070", "cloth.blue"), seed=seed + 2, petals=12)
    for cx in (220, 1380):
        lancet(mid, cx, 150, 70, 220, colours=("cloth.pink", "gold", "#7fd0ff", "cloth.blue"), seed=seed + int(cx))
    for cx in (330, 1270):
        lancet(mid, cx, 170, 54, 190, colours=("#ffb070", "cloth.pink", "gold"), seed=seed + int(cx) + 1, glow=0.12)
    for x in xs[1:-1]:
        pilaster(mid, x, 44, 140, g, "stone.warm", seed=seed + int(x), trim="stone.grey")
    for sx in (560, 1040):
        sconce(mid, sx, 360)
    fore_frame(r, beam=False, posts=(20, 1580), dark=0.55)


def cottage_a(r):
    """A cottage (the one with the bed by the door): cream plaster over a wainscot, dark beams,
    two curtained windows on the day with flowers in their boxes, a shelf of crockery and a
    picture of the sea, herbs drying from a beam, a sconce."""
    g, W = r.ground, r.room.width
    seed = roomkit.seed_of("cottage-a", "inside")
    mid = r["mid"]
    P.back_wall(mid, 0, W, 0, g, "plaster", None, "wood", seed, dado=0.32)
    frieze(mid, 0, W, 64, ("flower.pink", "flower.blue"), seed=seed + 9)
    window(mid, 300, 170, 96, 130, view="day", curtains="cloth.pink", box=True, seed=seed + 1)
    window(mid, 1160, 170, 96, 130, view="day", curtains="cloth.pink", box=True, seed=seed + 2)
    P.shelf(mid, 1010, 330, 200, ("plate", "jar", "plate"), seed=seed + 3)
    picture(mid, 600, 300, 96, 70, "sea", seed=seed + 4)
    picture(mid, 1450, 330, 60, 80, "portrait", frame="wood", seed=seed + 5)
    P.beams(mid, 0, W, 0, posts=((80, g), (820, g)), h=30, seed=seed + 6)
    herbs(mid, 480, 30, 3, seed=seed + 7)
    sconce(mid, 760, 340)
    fore_frame(r, posts=(1560,))


def cottage_b(r):
    """A cottage (the one with the hearth): blue-grey plaster, a brick chimney breast behind the
    hearth (the game's prop), a curtained window and a small one over the bed, a picture of hills,
    a shelf, onions and herbs hung from the beams."""
    g, W = r.ground, r.room.width
    seed = roomkit.seed_of("cottage-b", "inside")
    mid = r["mid"]
    P.back_wall(mid, 0, W, 0, g, "plaster", "blue", "wood", seed, dado=0.3)
    frieze(mid, 0, W, 64, ("flower.yellow", "flower.white"), leaf="leaf.blue", seed=seed + 9)
    chimney_breast(mid, 493, 220, g, 0, "brick", seed + 1)
    window(mid, 760, 170, 96, 130, view="day", curtains="cloth.teal", box=True, seed=seed + 2)
    import paint_rooms_docks as D
    cp = Piece(250, 180, 340, 270)
    D.clock(292, 226, 24)(cp)
    mid.place(cp)
    window(mid, 1400, 200, 64, 84, view="day", kind="square", panes=2, seed=seed + 3)
    picture(mid, 1050, 300, 100, 70, "hills", seed=seed + 4)
    P.shelf(mid, 220, 360, 180, ("jar", "plate", "jar"), seed=seed + 5)
    P.beams(mid, 0, W, 0, posts=((1180, g),), h=30, seed=seed + 6)
    herbs(mid, 260, 30, 2, seed=seed + 7, kind="onions")
    herbs(mid, 960, 30, 3, seed=seed + 8)
    fore_frame(r, posts=(30,))


def bakery(r):
    """The Rising Loaf (inside): warm yellow plaster, the ovens' wall of brick under one long hood
    (the two ovens are the game's props), shelves of loaves over the counter, the bakery's board
    with its loaf, sheaves of wheat, a sunny window, strings of onions."""
    g, W = r.ground, r.room.width
    seed = roomkit.seed_of("bakery", "inside")
    mid = r["mid"]
    P.back_wall(mid, 0, W, 0, g, "plaster", "yellow", "wood", seed, dado=0.28)
    frieze(mid, 0, W, 64, ("flower.red", "flower.white"), leaf="leaf.yellow", seed=seed + 9)
    bw = Piece(150, 260, 740, g + 6)
    m = P.wall(bw, 160, 300, 730, g, "brick", seed + 1, quoins=False)
    bw.edge(Path.rect(160, 300, 570, g - 300), tones("brick"), 1.3)
    del m
    mid.place(bw)
    hood(mid, 445, 300, 600, 150, 120, 0, "brick", seed + 2, glow=False)
    plaque(mid, 1115, 110, 230, 60, "loaf", board="cloth.cream")
    bread_shelf(mid, 1000, 300, 230, seed=seed + 3)
    bread_shelf(mid, 1260, 300, 230, seed=seed + 4)
    bread_shelf(mid, 1000, 400, 230, seed=seed + 5)
    window(mid, 1450, 170, 90, 120, view="day", curtains="cloth.cream", box=True, seed=seed + 6)
    wheat(mid, 790, 250)
    herbs(mid, 820, 30, 2, seed=seed + 7, kind="onions")
    P.beams(mid, 0, W, 0, posts=((860, g),), h=30, seed=seed + 8)
    fore_frame(r, posts=(1570,))


def forge(r):
    """Halden's forge (inside): grey stone gone sooty, a great brick hood over the hearth (the
    hearth and the anvil are the game's props) up through the roof with its glow, the bellows hung
    by it, boards of tools either side, horseshoes nailed in an arch, a shield on crossed swords
    for the trade, a window on the day."""
    g, W = r.ground, r.room.width
    seed = roomkit.seed_of("forge", "inside")
    mid = r["mid"]
    P.back_wall(mid, 0, W, 0, g, "stone", "grey", "wood.dark", seed, dado=0.2)
    hood(mid, 800, 330, 330, 150, 150, 0, "brick", seed + 1, glow=True)
    tool_board(mid, 400, 610, 250, seed=seed + 2)
    bellows(mid, 600, 460, 1.0)
    tool_board(mid, 960, 1170, 250, seed=seed + 3)
    horseshoes(mid, 200, 150, 5, 70)
    arms(mid, 1430, 300, 0.9, "cloth.red", "diamond")
    window(mid, 200, 176, 90, 120, view="day", kind="arch", seed=seed + 4, wall="stone.grey")
    P.beams(mid, 0, W, 0, posts=((340, g), (1270, g)), h=34, seed=seed + 5)
    hoist_chain(mid, 1110, 34, 170)
    fore_frame(r, posts=(1570,), dark=0.6)


def shop(r):
    """The Chandler's (inside): a ship's chandlery in blue plaster over dark boards, the shop's
    board with its anchor, shelves of jars, lanterns and tins behind where the counter stands, a
    model ship up on a bracket, hanks of rope on pegs, an anchor and a wheel on the walls,
    lanterns hung from the beams, and a window on the harbour."""
    g, W = r.ground, r.room.width
    seed = roomkit.seed_of("shop", "inside")
    mid = r["mid"]
    P.back_wall(mid, 0, W, 0, g, "plaster", "blue", "wood.dark", seed, dado=0.34)
    plaque(mid, 853, 110, 240, 60, "anchor", board="cloth.navy")
    P.shelf(mid, 853, 300, 300, ("jar", "bottle", "jar", "plate"), seed=seed + 1)
    P.shelf(mid, 853, 400, 300, ("bottle", "jar", "book"), seed=seed + 2)
    ship_model(mid, 1160, 280, 120)
    rope_hanks(mid, 480, 620, 240, seed=seed + 3)
    picture(mid, 400, 130, 90, 64, "ship", seed=seed + 6)
    anchor(mid, 1480, 320, 0.8)
    window(mid, 200, 170, 96, 130, view="day", curtains="cloth.blue", seed=seed + 4, wall="plaster.blue")
    P.beams(mid, 0, W, 0, posts=((1000, g),), h=30, seed=seed + 5)
    for lx in (560, 1340):
        hanging_lantern(mid, lx, 30, 100, 1.6, True)
    fore_frame(r, posts=(1570,))


def warehouse(r):
    """A warehouse on the docks (inside): tall walls of boards between heavy posts under the roof's
    beams, a loft along the left with sacks and crates stored on it, a hoist's block and chain from
    the beam, high windows on the day, a lantern hung by the clerk's desk (the crates, barrels and
    the desk on the floor are the game's props)."""
    g, W = r.ground, r.room.width
    seed = roomkit.seed_of("warehouse", "inside")
    mid = r["mid"]
    P.back_wall(mid, 0, W, 0, g, "boards", None, "wood", seed, dado=0.0)
    for cx in (1060, 1300, 1520):
        window(mid, cx, 110, 70, 80, view="day", kind="square", panes=2, seed=seed + cx, wall="wood")
    mezzanine(mid, 0, 720, 300, g, posts=(700,), seed=seed + 1)
    P.beams(mid, 720, W, 300, posts=((1180, g),), h=22, seed=seed + 9)
    P.beams(mid, 0, W, 0, posts=((1180, g),), h=36, seed=seed + 2)
    hoist_chain(mid, 880, 36, 300)
    hanging_lantern(mid, 1420, 36, 150, 1.6, True)
    notices(mid, 1420, 360, 2, seed=seed + 3)
    fore_frame(r, posts=(30,))


RECIPES = {
    "gilded-flagon/taproom": taproom,
    "rusty-anvil/bar": anvil_bar,
    "sunspire-keep/great-hall": great_hall,
    "chapel/inside": chapel,
    "cottage-a/inside": cottage_a,
    "cottage-b/inside": cottage_b,
    "bakery/inside": bakery,
    "forge/inside": forge,
    "shop/inside": shop,
    "warehouse/inside": warehouse,
}
