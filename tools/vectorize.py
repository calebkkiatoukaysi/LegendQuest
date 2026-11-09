"""Turns a painted picture into vector art: flat cel colours with smooth, crisp edges.

ChatGPT's parts are painted: soft gradients, brush texture and noise, and edges that are only as
sharp as the pixels. This redraws one as the kind of vector art Flash-era games were made of:

  - it finds the line art: what's dark outright (darker than LINE_DARK) or darker than round it by
    LINE_EDGE, in pieces of LINE_SPECK pixels or more. Each piece becomes a dark shape of its own
    colour, holes and all, so an eye's outline doesn't fill in over the eye;
  - under the lines it paints the colours back in from beside them, smooths them without crossing
    an edge (each pixel averaged with the neighbours that are its own colour, a few times over),
    which takes the brush texture out, finds the picture's palette (k-means in L*a*b*, COLOURS of
    them) and flattens every pixel to its nearest, so each tone of the cel shading is one flat
    colour; then splits it into regions (each run of one colour, four-connected) and merges any
    smaller than MIN_REGION into what's round it, which clears the specks;
  - it traces each shape along the pixel edges, smooths the staircase off it (a Gaussian along the
    outline, SIGMA pixels, LINE_SIGMA for a line), simplifies it (Ramer-Douglas-Peucker) and fits
    cubic Beziers through what's left (Catmull-Rom), keeping turns sharper than CORNER as corners;
  - it stacks the colours biggest first, each with its holes filled and grown a pixel into the
    bigger ones round it, so the smaller ones on top cover every seam, and the lines over them all;
  - and it draws the shapes back at any size with exact anti-aliasing (RENDER_ROWS sub-rows a pixel,
    exact across them), or writes them out as SVG.

  python tools/vectorize.py sample           a few faces and armors, painted against vector, in shots/art/vector
  python tools/vectorize.py file <png> [<out.svg>] [--outline N]
                                             any picture on black (or with alpha): an SVG and a PNG beside it

tools/import_parts.py imports the parts through it with --vector, so the game's parts are the vector
ones; every mask and socket is measured off the painted picture as before.
"""
import argparse
import os
import sys
import time

import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
import import_character as ic  # noqa: E402

ROOT = os.path.dirname(HERE)
SHOTS = os.path.join(ROOT, "shots", "art", "vector")

# How many flat colours a picture's drawn in, and how many of its pixels pick them; then up to
# EXTRA_COLOURS more for any that are further than OFF_PALETTE (L*a*b*) from all of them.
COLOURS = 40
SAMPLE = 60000
EXTRA_COLOURS = 12
OFF_PALETTE = 14.0
# The edge-keeping smoothing: how many passes, and how near (L*a*b*) a neighbour has to be to count.
SMOOTH_PASSES = 3
SMOOTH_NEAR = 12.0
# A region smaller than this many pixels is a speck, merged into what's round it.
MIN_REGION = 20
# The line art: darker than LINE_DARK (L*), or LINE_EDGE darker than the mean round it within
# LINE_REACH, in pieces of LINE_SPECK pixels or more, and no thicker than LINE_THICK each way of its
# middle, and no lighter than LINE_LIGHT (L*) in the middle; drawn a little darker than it was, by LINE_INK.
LINE_DARK = 16.0
LINE_EDGE = 14.0
LINE_REACH = 3
LINE_SPECK = 8
LINE_THICK = 3
LINE_LIGHT = 48.0
LINE_INK = 0.9
# The outlines: the staircase smoothed off along SIGMA pixels, simplified to within SIMPLIFY pixels,
# and a turn sharper than CORNER degrees kept as a corner rather than rounded.
SIGMA = 1.1
SIMPLIFY = 0.45
LINE_SIGMA = 0.7
LINE_SIMPLIFY = 0.3
CORNER = 70.0
# Drawing: sub-rows a pixel, and how many straight pieces each curve's drawn in.
RENDER_ROWS = 4
CURVE_STEPS = 6
# Tracing is done on the picture enlarged, so an eye's lines and highlights are big enough to keep
# and the curves come out smooth: as many times as fits PIXEL_BUDGET pixels, at most UPSCALE_MOST.
# Every size above is in the picture's own pixels and grows with it.
PIXEL_BUDGET = 4.0e6
UPSCALE_MOST = 3
# How far (in the picture's pixels) each colour grows under the smaller ones drawn over it.
SEAM_REACH = 1
# A dark line round the whole figure, if asked for: its colour.
OUTLINE_COLOUR = (22, 16, 28)


# --------------------------------------------------------------------------- #
# Colour
# --------------------------------------------------------------------------- #

def lab(rgb):
    """sRGB (0 to 255) to CIE L*a*b* under D65."""
    c = np.asarray(rgb, np.float64) / 255.0
    linear = np.where(c <= 0.04045, c / 12.92, ((c + 0.055) / 1.055) ** 2.4)
    xyz = linear @ np.array([[0.4124, 0.3576, 0.1805], [0.2126, 0.7152, 0.0722], [0.0193, 0.1192, 0.9505]]).T
    xyz /= np.array([0.95047, 1.0, 1.08883])
    f = np.where(xyz > 0.008856, np.cbrt(xyz), 7.787 * xyz + 16.0 / 116.0)
    return np.stack([116.0 * f[..., 1] - 16.0, 500.0 * (f[..., 0] - f[..., 1]), 200.0 * (f[..., 1] - f[..., 2])], axis=-1)


def smooth(colour, figure, passes=SMOOTH_PASSES):
    """The colours averaged with the neighbours (of the eight) that are near them in colour, a few
    passes over: brush texture and noise go, and an edge, whose two sides aren't near, stays."""
    out = colour.astype(np.float64).copy()
    for _ in range(passes):
        L = lab(out)
        total = out.copy()
        count = np.ones(figure.shape)
        for dy in (-1, 0, 1):
            for dx in (-1, 0, 1):
                if dy == 0 and dx == 0:
                    continue
                shifted = np.roll(np.roll(out, dy, axis=0), dx, axis=1)
                there = np.roll(np.roll(figure, dy, axis=0), dx, axis=1)
                near = there & (np.linalg.norm(np.roll(np.roll(L, dy, axis=0), dx, axis=1) - L, axis=2) < SMOOTH_NEAR)
                total += shifted * near[:, :, None]
                count += near
        out = np.where(figure[:, :, None], total / count[:, :, None], out)
    return out


def palette(colour, figure, k, least=MIN_REGION):
    """The picture's k colours (k-means++ in L*a*b*, seeded, on a sample of its pixels), and each
    figure pixel's nearest: (palette as RGB, index image with -1 off the figure)."""
    L = lab(colour)
    points = L[figure]
    rgb = colour[figure]
    rng = np.random.default_rng(0)
    pick = points[rng.choice(len(points), min(SAMPLE, len(points)), replace=False)]
    centres = [pick[rng.integers(len(pick))]]
    for _ in range(1, k):
        d = np.min(((pick[:, None, :] - np.array(centres)[None]) ** 2).sum(axis=2), axis=1)
        if d.sum() <= 0:
            break
        centres.append(pick[rng.choice(len(pick), p=d / d.sum())])
    centres = np.array(centres)
    for _ in range(25):
        nearest = nearest_centre(pick, centres)
        moved = np.array([pick[nearest == j].mean(axis=0) if (nearest == j).any() else centres[j] for j in range(len(centres))])
        if np.allclose(moved, centres, atol=0.05):
            break
        centres = moved
    which = nearest_centre(points, centres)
    # A colour too small to win a centre of its own (an iris, an earring, a scar) gets one: while
    # enough pixels are further than OFF_PALETTE from every colour, the furthest of them and those
    # near it add their mean, up to EXTRA_COLOURS more.
    for _ in range(EXTRA_COLOURS):
        off = np.linalg.norm(points - centres[which], axis=1)
        far = off > OFF_PALETTE
        if far.sum() < least:
            break
        seed = points[np.argmax(np.where(far, off, -1.0))]
        near = far & (np.linalg.norm(points - seed, axis=1) < OFF_PALETTE)
        centres = np.vstack([centres, points[near].mean(axis=0)])
        which = nearest_centre(points, centres)
    flat = np.array([rgb[which == j].mean(axis=0) if (which == j).any() else (0.0, 0.0, 0.0) for j in range(len(centres))])
    index = np.full(figure.shape, -1, np.int64)
    index[figure] = which
    return flat, index


def nearest_centre(points, centres, chunk=200000):
    """Each point's nearest centre, a chunk at a time so the distances fit in memory."""
    out = np.empty(len(points), np.int64)
    cc = (centres ** 2).sum(axis=1)
    for a in range(0, len(points), chunk):
        p = points[a:a + chunk]
        out[a:a + chunk] = np.argmin(cc[None, :] - 2.0 * p @ centres.T, axis=1)
    return out


# --------------------------------------------------------------------------- #
# Regions
# --------------------------------------------------------------------------- #

def regions(index):
    """The four-connected runs of one colour in an index image (-1 is nothing): a region image (-1
    off them) and how many. Runs along each row are joined to the runs of the same colour that
    overlap them in the row above, a union-find over runs."""
    h, w = index.shape
    starts, ends, rows, values = [], [], [], []
    for y in range(h):
        row = index[y]
        cut = np.flatnonzero(row[1:] != row[:-1]) + 1
        s = np.concatenate([[0], cut])
        e = np.concatenate([cut, [w]])
        v = row[s]
        keep = v >= 0
        starts.append(s[keep])
        ends.append(e[keep])
        values.append(v[keep])
        rows.append(np.full(int(keep.sum()), y))
    first = np.concatenate([[0], np.cumsum([len(s) for s in starts])])
    S, E, V = np.concatenate(starts), np.concatenate(ends), np.concatenate(values)
    parent = list(range(len(S)))

    def find(i):
        while parent[i] != i:
            parent[i] = parent[parent[i]]
            i = parent[i]
        return i

    Sl, El, Vl = S.tolist(), E.tolist(), V.tolist()
    for y in range(1, h):
        i, j = first[y - 1], first[y]
        i_end, j_end = first[y], first[y + 1]
        while i < i_end and j < j_end:
            if Sl[i] < El[j] and Sl[j] < El[i] and Vl[i] == Vl[j]:
                a, b = find(i), find(j)
                if a != b:
                    parent[b] = a
            if El[i] < El[j]:
                i += 1
            else:
                j += 1
    roots = np.array([find(i) for i in range(len(S))], np.int64)
    _, ids = np.unique(roots, return_inverse=True)
    out = np.full((h, w), -1, np.int64)
    R = np.concatenate(rows)
    lengths = E - S
    flat_rows = np.repeat(R, lengths)
    flat_cols = np.arange(int(lengths.sum())) - np.repeat(np.cumsum(lengths) - lengths, lengths) + np.repeat(S, lengths)
    out[flat_rows, flat_cols] = np.repeat(ids, lengths)
    return out, int(ids.max()) + 1 if len(ids) else 0


def merge_specks(index, least=MIN_REGION):
    """The index image with every region smaller than `least` pixels handed to the colours round it,
    grown in from its edges a pixel at a time."""
    labels, count = regions(index)
    sizes = np.bincount(labels[labels >= 0], minlength=count)
    small = (labels >= 0) & (sizes[np.maximum(labels, 0)] < least)
    out = index.copy()
    out[small] = -2
    for _ in range(64):
        hole = out == -2
        if not hole.any():
            break
        for dy, dx in ((-1, 0), (1, 0), (0, -1), (0, 1)):
            there = np.roll(np.roll(out, dy, axis=0), dx, axis=1)
            take = hole & (there >= 0) & (out == -2)
            out[take] = there[take]
    out[out == -2] = index[out == -2]
    return out


# --------------------------------------------------------------------------- #
# Outlines
# --------------------------------------------------------------------------- #

def outline_loops(mask, holes=False):
    """The outlines round a mask's pixels along their edges, as corner points (x, y): clockwise (y
    down) round the outside, the other way round a hole. Just the one round the piece holding its
    first pixel, unless `holes`, when it's every one. Where two pixels touch only at a corner it
    turns right, so a piece is four-connected, the same as its region."""
    m = np.pad(mask, 1)
    H, W = m.shape
    ys, xs = np.nonzero(m)
    above = np.zeros_like(m)
    above[1:] = m[:-1]
    below = np.zeros_like(m)
    below[:-1] = m[1:]
    left = np.zeros_like(m)
    left[:, 1:] = m[:, :-1]
    right = np.zeros_like(m)
    right[:, :-1] = m[:, 1:]
    sx, sy, ex, ey = [], [], [], []
    for side, (ax, ay, bx, by) in ((~above, (0, 0, 1, 0)), (~right, (1, 0, 1, 1)), (~below, (1, 1, 0, 1)), (~left, (0, 1, 0, 0))):
        py, px = np.nonzero(m & side)
        sx.append(px + ax)
        sy.append(py + ay)
        ex.append(px + bx)
        ey.append(py + by)
    sx, sy, ex, ey = (np.concatenate(a) for a in (sx, sy, ex, ey))
    key = (sy * (W + 1) + sx).tolist()
    end = (ey * (W + 1) + ex).tolist()
    dxs, dys = (ex - sx).tolist(), (ey - sy).tolist()
    px, py = (sx - 1).tolist(), (sy - 1).tolist()
    out = {}
    for n, k in enumerate(key):
        out.setdefault(k, []).append(n)
    # The first pixel in reading order's top edge is on the outline round its piece.
    first = int(np.flatnonzero((sy == ys.min()) & (sx == xs[ys == ys.min()].min()) & (ey == sy))[0])
    used = set()
    loops = []
    for start in [first] + (list(range(len(key))) if holes else []):
        if start in used:
            continue
        points = []
        n = start
        while n not in used:
            used.add(n)
            points.append((px[n], py[n]))
            nexts = [c for c in out.get(end[n], ()) if c not in used]
            if not nexts:
                break
            if len(nexts) > 1:
                turn = (-dys[n], dxs[n])
                nexts.sort(key=lambda c: 0 if (dxs[c], dys[c]) == turn else 1)
            n = nexts[0]
        loops.append(np.array(points, np.float64))
    return loops


def path(loop, sigma, tolerance):
    """An outline as Beziers: smoothed, simplified and fitted."""
    return curves(simplify(smoothed(loop, sigma), tolerance))


def smoothed(points, sigma=SIGMA):
    """A closed outline with its staircase smoothed off: each point the Gaussian-weighted mean of
    those along the outline either side of it, `sigma` points each way."""
    n = len(points)
    sigma = min(sigma, n / 6.0)
    if sigma < 0.5:
        return points
    r = int(np.ceil(3 * sigma))
    k = np.exp(-0.5 * (np.arange(-r, r + 1) / sigma) ** 2)
    k /= k.sum()
    wrapped = np.concatenate([points[-r:], points, points[:r]])
    return np.stack([np.convolve(wrapped[:, i], k, mode="valid") for i in range(2)], axis=1)


def simplify(points, tolerance):
    """A closed outline with the points it can do without taken out: Ramer-Douglas-Peucker, from its
    first point and the one furthest from it."""
    if len(points) < 5:
        return points
    far = int(np.argmax(((points - points[0]) ** 2).sum(axis=1)))
    keep = np.zeros(len(points), bool)
    keep[0] = keep[far] = True
    stack = [(0, far), (far, len(points))]
    while stack:
        a, b = stack.pop()
        if b - a < 2:
            continue
        p, q = points[a], points[b % len(points)]
        seg = points[a + 1:b]
        d = q - p
        length = np.hypot(*d)
        if length < 1e-9:
            dist = np.hypot(*(seg - p).T)
        else:
            dist = np.abs(d[0] * (seg[:, 1] - p[1]) - d[1] * (seg[:, 0] - p[0])) / length
        i = int(np.argmax(dist))
        if dist[i] > tolerance:
            m = a + 1 + i
            keep[m] = True
            stack += [(a, m), (m, b)]
    return points[keep]


def curves(points):
    """Cubic Beziers through a closed outline's points (Catmull-Rom), a corner where it turns more
    than CORNER degrees: [(p0, c1, c2, p1), ...]."""
    n = len(points)
    if n < 3:
        return []
    prev, nxt = np.roll(points, 1, axis=0), np.roll(points, -1, axis=0)
    a, b = points - prev, nxt - points
    cos = (a * b).sum(axis=1) / np.maximum(np.hypot(*a.T) * np.hypot(*b.T), 1e-9)
    corner = cos < np.cos(np.radians(CORNER))
    tangent = (nxt - prev) / 6.0
    tangent[corner] = 0.0
    out = []
    for i in range(n):
        j = (i + 1) % n
        out.append((points[i], points[i] + tangent[i], points[j] - tangent[j], points[j]))
    return out


# --------------------------------------------------------------------------- #
# Vectorising
# --------------------------------------------------------------------------- #

class Vector:
    """A picture as shapes: its size, and its shapes back to front, each a colour (RGB, 0 to 255) and
    its outlines, each a closed run of cubic Beziers, filled even-odd so a hole stays a hole."""

    def __init__(self, width, height, shapes):
        self.width, self.height, self.shapes = width, height, shapes


def line_art(colour, figure, k=1):
    """The picture's lines (enlarged k times): what's darker than LINE_DARK, or LINE_EDGE darker than
    the mean round it, in pieces of LINE_SPECK pixels or more."""
    L = lab(colour)[..., 0]
    reach = LINE_REACH * k
    near = ic.box_sum(np.where(figure, L, 0.0), reach) / np.maximum(ic.box_sum(figure.astype(np.float64), reach), 1.0)
    line = figure & ((L < LINE_DARK) | (L < near - LINE_EDGE))
    # A line is thin: where the dark is thicker than LINE_THICK each way it's a dark colour (black
    # hair, navy cloth), and goes with the colours.
    t = LINE_THICK * k
    core = ~ic.grow(~line, t)
    line &= ~ic.grow(core, t)
    pieces, count = ic.components(line)
    if count:
        sizes = np.bincount(pieces.ravel(), minlength=count + 1)
        # Ink is dark itself: a piece only darker than what's round it, but light in the middle (an
        # iris against the white of the eye), is a colour, not a line.
        order = np.argsort(pieces.ravel(), kind="stable")
        flat_l = L.ravel()[order]
        starts = np.concatenate([[0], np.cumsum(sizes)])
        middle = np.array([np.median(flat_l[starts[n]:starts[n + 1]]) if sizes[n] else 0.0 for n in range(count + 1)])
        keep = (sizes >= LINE_SPECK * k * k) & (middle <= LINE_LIGHT)
        keep[0] = False
        line &= keep[pieces]
    return line


def painted_under(colour, figure, line):
    """The colours with the lines painted over from beside them: each line pixel the mean of the
    figure's pixels next to it that aren't lines, working in from the edges."""
    out = colour.astype(np.float64).copy()
    known = figure & ~line
    for _ in range(32):
        todo = line & ~known
        if not todo.any():
            break
        total = np.zeros(out.shape)
        count = np.zeros(figure.shape)
        for dy, dx in ((-1, 0), (1, 0), (0, -1), (0, 1), (-1, -1), (-1, 1), (1, -1), (1, 1)):
            there = np.roll(np.roll(known, dy, axis=0), dx, axis=1)
            total += np.roll(np.roll(out, dy, axis=0), dx, axis=1) * there[:, :, None]
            count += there
        fill = todo & (count > 0)
        out[fill] = total[fill] / count[fill][:, None]
        known = known | fill
    return out


def vectorize(colour, alpha, colours=COLOURS, outline=0, upscale=None):
    """A picture (straight colour, 0 to 255, and alpha) as a Vector: its flat colours, then its
    lines. `outline` puts a dark line that many pixels wide round the whole figure first. It's
    traced on the box round the figure enlarged `upscale` times (by default as many as fit
    PIXEL_BUDGET, at most UPSCALE_MOST), and the shapes brought back to the picture's size."""
    h, w = alpha.shape
    ys, xs = np.nonzero(alpha > 0.02)
    x0, y0 = max(0, int(xs.min()) - 4), max(0, int(ys.min()) - 4)
    x1, y1 = min(w, int(xs.max()) + 5), min(h, int(ys.max()) + 5)
    k = upscale or max(1, min(UPSCALE_MOST, int(np.sqrt(PIXEL_BUDGET / ((x1 - x0) * (y1 - y0))))))
    premultiplied = np.concatenate([colour[y0:y1, x0:x1] / 255.0 * alpha[y0:y1, x0:x1, None], alpha[y0:y1, x0:x1, None]], axis=2)
    if k > 1:
        premultiplied = ic.resample(premultiplied, (0, 0, x1 - x0, y1 - y0), ((x1 - x0) * k, (y1 - y0) * k))
    a = np.clip(premultiplied[:, :, 3], 0.0, 1.0)
    big = np.where(a[:, :, None] > 0, np.clip(premultiplied[:, :, :3] / np.maximum(a, 1e-6)[:, :, None] * 255.0, 0, 255), 0.0)
    shapes = traced(big, a, k, colours, outline * k)
    offset = np.array([x0, y0], np.float64)
    moved = [(rgb, [[tuple(p / k + offset for p in seg) for seg in loop] for loop in loops]) for rgb, loops in shapes]
    return Vector(w, h, moved)


def traced(colour, alpha, k, colours, outline):
    """The shapes of a picture enlarged k times, in its pixels: flat colours biggest first, then the
    lines (vectorize)."""
    h, w = alpha.shape
    figure = alpha > 0.5
    line = line_art(colour, figure, k)
    soft = smooth(painted_under(colour, figure, line), figure, SMOOTH_PASSES + k - 1)
    flat, index = palette(soft, figure, colours, MIN_REGION * k * k)
    index = merge_specks(index, MIN_REGION * k * k)
    labels, count = regions(index)
    sizes = np.bincount(labels[labels >= 0], minlength=count)
    order = np.argsort(-sizes, kind="stable")
    rank = np.empty(count, np.int64)
    rank[order] = np.arange(count)
    ranked = np.where(labels >= 0, rank[np.maximum(labels, 0)], np.iinfo(np.int64).max)

    shapes = []
    if outline > 0:
        shapes.append((OUTLINE_COLOUR, [path(outline_loops(ic.grow(figure, outline))[0], SIGMA * k, SIMPLIFY * k)]))

    region_colour = np.zeros(count, np.int64)
    on = labels >= 0
    region_colour[labels[on]] = index[on]
    reach = SEAM_REACH * k
    for r, (y0, y1, x0, x1) in zip(order, boxes(labels, count, order, reach + 1)):
        crop = labels[y0:y1, x0:x1] == r
        # Grown into the bigger regions round it, drawn before it, further than its outline's
        # smoothing can pull it back, so no seam shows between them.
        mine = crop | (ic.grow(crop, reach) & (ranked[y0:y1, x0:x1] < rank[r]))
        # Only what joins the region side by side: a grown pixel touching it at a corner alone
        # would start the outline round itself instead.
        pieces, _ = ic.components(mine)
        mine = pieces == pieces[crop][0]
        shape = path(outline_loops(mine)[0] + np.array([x0, y0]), SIGMA * k, SIMPLIFY * k)
        if shape:
            shapes.append((tuple(int(round(v)) for v in flat[region_colour[r]]), [shape]))

    # The lines on top, each piece its own colour (the darker half of it, inked a touch darker).
    pieces, count = ic.components(line)
    if count:
        L = lab(colour)[..., 0]
        sizes = np.bincount(pieces[pieces > 0], minlength=count + 1)
        for n, (y0, y1, x0, x1) in zip(range(1, count + 1), boxes(pieces, count + 1, range(1, count + 1))):
            crop = pieces[y0:y1, x0:x1] == n
            dark = L[y0:y1, x0:x1][crop]
            ink = colour[y0:y1, x0:x1][crop][dark <= np.median(dark)].mean(axis=0) * LINE_INK
            loops = [path(loop + np.array([x0, y0]), LINE_SIGMA * k, LINE_SIMPLIFY * k) for loop in outline_loops(crop, holes=True)]
            loops = [loop for loop in loops if loop]
            if loops:
                shapes.append((tuple(int(round(v)) for v in ink), loops))
    return shapes


def boxes(labels, count, which, margin=1):
    """The box round each of some labels' pixels, `margin` pixels wider each way, clipped to the
    picture: (top, bottom, left, right) for each, in the order asked."""
    h, w = labels.shape
    flat = labels.ravel()
    on = np.flatnonzero(flat >= 0)
    ys, xs = np.divmod(on, w)
    ids = flat[on]
    top = np.full(count, h)
    bottom = np.full(count, -1)
    left = np.full(count, w)
    right = np.full(count, -1)
    np.minimum.at(top, ids, ys)
    np.maximum.at(bottom, ids, ys)
    np.minimum.at(left, ids, xs)
    np.maximum.at(right, ids, xs)
    for r in which:
        yield max(0, top[r] - margin), min(h, bottom[r] + 1 + margin), max(0, left[r] - margin), min(w, right[r] + 1 + margin)


# --------------------------------------------------------------------------- #
# Drawing and writing
# --------------------------------------------------------------------------- #

def flatten(loop, scale):
    """A closed run of Beziers as a polygon, each curve CURVE_STEPS straight pieces, scaled."""
    p = np.array([seg for seg in loop])  # (n, 4, 2)
    t = np.linspace(0.0, 1.0, CURVE_STEPS, endpoint=False)[None, :, None]
    u = 1.0 - t
    pts = u ** 3 * p[:, None, 0] + 3 * u * u * t * p[:, None, 1] + 3 * u * t * t * p[:, None, 2] + t ** 3 * p[:, None, 3]
    return pts.reshape(-1, 2) * scale


def coverage(polys, height, width):
    """How much of each pixel some closed polygons cover together, 0 to 1, in their box: (coverage,
    top, left). RENDER_ROWS sub-rows a pixel, each crossed exactly; even-odd, so a hole's a hole."""
    x0 = np.concatenate([p[:, 0] for p in polys])
    y0 = np.concatenate([p[:, 1] for p in polys])
    x1 = np.concatenate([np.roll(p[:, 0], -1) for p in polys])
    y1 = np.concatenate([np.roll(p[:, 1], -1) for p in polys])
    s = RENDER_ROWS
    top, bottom = max(0, int(np.floor(y0.min()))), min(height, int(np.ceil(y0.max())))
    left, right = max(0, int(np.floor(x0.min()))), min(width, int(np.ceil(x0.max())) + 1)
    if bottom <= top or right <= left:
        return None, 0, 0
    lo, hi = np.minimum(y0, y1), np.maximum(y0, y1)
    k0 = np.ceil(lo * s - 0.5).astype(np.int64)
    k1 = np.ceil(hi * s - 0.5).astype(np.int64)
    k0, k1 = np.maximum(k0, top * s), np.minimum(k1, bottom * s)
    counts = np.maximum(k1 - k0, 0)
    total = int(counts.sum())
    if total == 0:
        return None, 0, 0
    edge = np.repeat(np.arange(len(x0)), counts)
    k = np.arange(total) - np.repeat(np.cumsum(counts) - counts, counts) + np.repeat(k0, counts)
    yk = (k + 0.5) / s
    t = (yk - y0[edge]) / (y1 - y0)[edge]
    x = x0[edge] + t * (x1 - x0)[edge]
    order = np.lexsort((x, k))
    k, x = k[order], x[order]
    if len(k) % 2:
        k, x = k[:-1], x[:-1]
    xa, xb, row = x[0::2], x[1::2], k[0::2] // s - top
    xa = np.clip(xa - left, 0, right - left)
    xb = np.clip(xb - left, 0, right - left)
    cols = right - left
    cov = np.zeros((bottom - top, cols + 2))
    diff = np.zeros((bottom - top, cols + 2))
    ia, ib = np.floor(xa).astype(np.int64), np.floor(xb).astype(np.int64)
    w = 1.0 / s
    same = ia == ib
    np.add.at(cov, (row[same], ia[same]), (xb - xa)[same] * w)
    d = ~same
    np.add.at(cov, (row[d], ia[d]), (ia[d] + 1 - xa[d]) * w)
    np.add.at(cov, (row[d], ib[d]), (xb[d] - ib[d]) * w)
    np.add.at(diff, (row[d], ia[d] + 1), w)
    np.add.at(diff, (row[d], ib[d]), -w)
    cov += np.cumsum(diff, axis=1)
    return np.clip(cov[:, :cols], 0.0, 1.0), top, left


def render(vector, scale=1.0):
    """A Vector drawn, premultiplied RGBA 0 to 1, at `scale` times its size."""
    width, height = int(round(vector.width * scale)), int(round(vector.height * scale))
    canvas = np.zeros((height, width, 4))
    for rgb, loops in vector.shapes:
        cov, top, left = coverage([flatten(loop, scale) for loop in loops], height, width)
        if cov is None:
            continue
        region = canvas[top:top + cov.shape[0], left:left + cov.shape[1]]
        a = cov[:, :, None]
        paint = np.concatenate([np.asarray(rgb, np.float64) / 255.0, [1.0]])[None, None, :]
        region[:] = paint * a + region * (1.0 - a)
    return canvas


def svg(vector):
    """A Vector as an SVG document."""
    out = ['<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 %d %d" width="%d" height="%d">'
           % (vector.width, vector.height, vector.width, vector.height)]
    for rgb, loops in vector.shapes:
        d = []
        for loop in loops:
            p0 = loop[0][0]
            d.append("M%.2f %.2f" % (p0[0], p0[1]))
            for _, c1, c2, p1 in loop:
                d.append("C%.2f %.2f %.2f %.2f %.2f %.2f" % (c1[0], c1[1], c2[0], c2[1], p1[0], p1[1]))
            d.append("Z")
        out.append('<path fill="#%02x%02x%02x" fill-rule="evenodd" d="%s"/>' % (rgb[0], rgb[1], rgb[2], "".join(d)))
    out.append("</svg>")
    return "\n".join(out) + "\n"


def read_svg(text):
    """A Vector back from an SVG this wrote (svg): its size and its shapes."""
    import re
    size = re.search(r'viewBox="0 0 (\d+) (\d+)"', text)
    shapes = []
    for fill, d in re.findall(r'<path fill="#([0-9a-f]{6})" fill-rule="evenodd" d="([^"]+)"', text):
        rgb = tuple(int(fill[i:i + 2], 16) for i in (0, 2, 4))
        loops = []
        for part in d.split("M")[1:]:
            numbers = np.array([float(v) for v in re.findall(r"-?\d+(?:\.\d+)?", part)])
            start, rest = numbers[:2], numbers[2:].reshape(-1, 6)
            loop, p0 = [], start
            for c in rest:
                loop.append((p0, c[0:2], c[2:4], c[4:6]))
                p0 = c[4:6]
            loops.append(loop)
        shapes.append((rgb, loops))
    return Vector(int(size.group(1)), int(size.group(2)), shapes)


def fingerprint(colour, alpha):
    """What a picture's vector depends on, as a hash: its figure, its colours, and this file's settings."""
    import hashlib
    h = hashlib.sha1()
    h.update(np.packbits(alpha > 0.5).tobytes())
    h.update(np.rint(np.where((alpha > 0.5)[:, :, None], colour, 0)).astype(np.uint8).tobytes())
    h.update(repr((COLOURS, SAMPLE, EXTRA_COLOURS, OFF_PALETTE, SMOOTH_PASSES, SMOOTH_NEAR, MIN_REGION, LINE_DARK, LINE_EDGE, LINE_REACH, LINE_SPECK, LINE_THICK, LINE_LIGHT, LINE_INK,
                   SIGMA, SIMPLIFY, LINE_SIGMA, LINE_SIMPLIFY, CORNER, PIXEL_BUDGET, UPSCALE_MOST, SEAM_REACH, colour.shape)).encode())
    return h.hexdigest()


def cached(colour, alpha, path):
    """A picture's Vector, read from its SVG at `path` if that was made from this same picture with
    these same settings, or made (vectorize) and written there."""
    key = fingerprint(colour, alpha)
    if os.path.exists(path):
        with open(path, encoding="utf-8") as f:
            text = f.read()
        if ("<!-- source %s -->" % key) in text:
            return read_svg(text)
    vector = vectorize(colour, alpha)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    head, rest = svg(vector).split("\n", 1)
    text = head + "\n<!-- source %s -->\n" % key + rest
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write(text)
    # What's drawn is what was written, its numbers rounded as the SVG has them, so a part drawn
    # from the cache next time comes out the same to the last bit.
    return read_svg(text)


def straight(premultiplied):
    """Premultiplied RGBA (0 to 1) as straight colour (0 to 255) and alpha."""
    a = premultiplied[:, :, 3]
    colour = premultiplied[:, :, :3] / np.maximum(a, 1e-6)[:, :, None] * 255.0
    return np.where(a[:, :, None] > 0, np.clip(colour, 0, 255), 0.0), a


def on_black(premultiplied):
    """Premultiplied RGBA (0 to 1) over black, as 8-bit RGBA."""
    rgb = np.rint(np.clip(premultiplied[:, :, :3], 0, 1) * 255).astype(np.uint8)
    return np.dstack([rgb, np.full(rgb.shape[:2], 255, np.uint8)])


# --------------------------------------------------------------------------- #
# Samples
# --------------------------------------------------------------------------- #

SAMPLE_FACES = ("face-player-1", "face-player-5", "face-brannock", "face-odelle", "face-sera", "face-tomas")
SAMPLE_ARMORS = ("armor-traveller", "armor-watch", "armor-grell", "armor-plate", "armor-sera", "armor-mage-robes-f")


def sample(only=None, colours=COLOURS, outline=0, name=""):
    """Each sample part's layer, as tools/import_parts.py cuts it, painted beside vector: at full
    size, then at the game's part size. Their SVGs go beside them."""
    import import_parts as ip
    os.makedirs(SHOTS, exist_ok=True)
    edits = ip.basis(ip.story())
    bases = {}
    back = np.array([70, 76, 92], np.float64) / 255.0

    def tile(premultiplied, box):
        x0, y0, x1, y1 = box
        crop = premultiplied[y0:y1, x0:x1]
        return np.clip(crop[:, :, :3] + back * (1.0 - crop[:, :, 3:]), 0, 1)

    def painted(colour, alpha):
        return np.concatenate([colour / 255.0 * alpha[:, :, None], alpha[:, :, None]], axis=2)

    for kind, ids in (("faces", SAMPLE_FACES), ("armors", SAMPLE_ARMORS)):
        rows = []
        for pid in ids:
            if only and pid not in only:
                continue
            path = os.path.join(ip.SOURCE, kind, pid + ".png")
            if not os.path.exists(path):
                continue
            base = ip.base_of(pid, edits)
            b = bases.setdefault(base, ip.Base(base))
            rgb = ip.load(path)
            if kind == "faces":
                colour, alpha = ip.face_layer(b, rgb)[:2]
            else:
                colour, alpha = ip.armor_layer(b, rgb)[:2]
            started = time.time()
            vector = vectorize(colour, alpha, colours, outline)
            drawn = render(vector)
            with open(os.path.join(SHOTS, pid + name + ".svg"), "w", encoding="utf-8", newline="\n") as f:
                f.write(svg(vector))
            print("  %-22s %4d shapes, %.0fs" % (pid, len(vector.shapes), time.time() - started))
            ys, xs = np.nonzero(alpha > 0)
            box = (max(0, xs.min() - 8), max(0, ys.min() - 8), xs.max() + 9, ys.max() + 9)
            full = [tile(painted(colour, alpha), box), tile(drawn, box)]
            small_box = tuple(int(v * ip.PART_SCALE) for v in box)
            half = ic.resample(painted(colour, alpha), (0, 0, alpha.shape[1], alpha.shape[0]),
                               (int(alpha.shape[1] * ip.PART_SCALE), int(alpha.shape[0] * ip.PART_SCALE)))
            game = [tile(half, small_box), tile(render(vector, ip.PART_SCALE), small_box)]
            tiles = full + ([upscale(g, 2) for g in game] if kind == "faces" else game)
            tall = max(t.shape[0] for t in tiles)
            rows.append(np.concatenate([np.pad(t, ((4, 4 + tall - t.shape[0]), (4, 4), (0, 0))) for t in tiles], axis=1))
        if rows:
            width = max(r.shape[1] for r in rows)
            sheet = np.concatenate([np.pad(r, ((0, 0), (0, width - r.shape[1]), (0, 0))) for r in rows], axis=0)
            out = np.rint(sheet * 255).astype(np.uint8)
            ic.write_png(os.path.join(SHOTS, "compare-%s%s.png" % (kind, name)), np.dstack([out, np.full(out.shape[:2], 255, np.uint8)]))
            print("wrote", os.path.relpath(os.path.join(SHOTS, "compare-%s%s.png" % (kind, name)), ROOT))


def upscale(img, k):
    return np.repeat(np.repeat(img, k, axis=0), k, axis=1)


def main(argv):
    parser = argparse.ArgumentParser(description=__doc__.split("\n")[0])
    sub = parser.add_subparsers(dest="command", required=True)
    some = sub.add_parser("sample")
    some.add_argument("only", nargs="*")
    some.add_argument("--colours", type=int, default=COLOURS)
    some.add_argument("--outline", type=int, default=0)
    some.add_argument("--name", default="", help="added to the sheets' names")
    one = sub.add_parser("file")
    one.add_argument("png")
    one.add_argument("svg", nargs="?")
    one.add_argument("--outline", type=int, default=0)
    one.add_argument("--colours", type=int, default=COLOURS)
    args = parser.parse_args(argv)
    if args.command == "sample":
        sample(set(args.only) or None, args.colours, args.outline, args.name)
        return 0
    image = ic.read_png(args.png)
    image = image[0] if isinstance(image, tuple) else image
    rgb = image[:, :, :3].astype(np.float64)
    if image.shape[2] == 4 and (image[:, :, 3] < 255).any():
        alpha = image[:, :, 3] / 255.0
    else:
        import import_parts as ip
        rgb, alpha = ip.key_black(image[:, :, :3])
    vector = vectorize(rgb, alpha, args.colours, args.outline)
    out = args.svg or os.path.splitext(args.png)[0] + ".vector.svg"
    with open(out, "w", encoding="utf-8", newline="\n") as f:
        f.write(svg(vector))
    ic.write_png(os.path.splitext(out)[0] + ".png", on_black(render(vector)))
    print("wrote", out, "and its PNG:", len(vector.shapes), "shapes")
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
