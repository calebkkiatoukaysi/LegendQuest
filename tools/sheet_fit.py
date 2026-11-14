"""
Fitting the cells of a sheet ChatGPT laid out its own way back onto their base.

ChatGPT doesn't always keep to a sheet's template: it can draw the busts bigger than the template's,
paint the whole sheet at another size (1774 x 887, or 2172 x 724, for 1536 x 1024), or stand every
figure a little taller. part_sheet.py's cut looks for each cell within a few pixels and a few per
cent of where the template put it; this finds it wherever it is. For each cell it looks for the
scale and the place at which the base's own picture lies under the cell, judged only where the part
leaves the base alone (WINDOWS: for a hair, the front of the face and the chest; for an extra, the
forehead and the chest; for an armor, the bald head and the fists):

  - coarsely, at a quarter of the size, over every scale from SCALE_LOW to SCALE_HIGH and every place
    at once, by masked normalised cross-correlation done with FFTs (ncc_map);
  - then at full size round the best, scale by scale in fine steps and place by place, finished to
    a fraction of a pixel by a parabola through the best score and its neighbours (refine).

A fit maps the base's canvas into the sheet: canvas point (X, Y) is sheet point
(x0 + tx + s X, y0 + ty + s Y), where (x0, y0) is the cell's corner. The cells are found between the
sheet's grey dividing lines (cell_boxes), not assumed from the template's sizes.

Written by Caleb Kiatoukaysi for Legend Quest.
"""
import numpy as np

#: The scales searched, base to sheet: from a bust drawn at not quite half size to one larger than life.
SCALE_LOW, SCALE_HIGH, SCALE_STEP = 0.40, 1.60, 0.01
#: The coarse search's shrink, and the fine search's steps round its best: scale, then place.
COARSE = 4
FINE_SCALE, FINE_SCALES = 0.002, 5
FINE_PLACE = 3
#: A cell after the first in its row is searched only this far either side of the scale found so far.
NEAR_SCALE = 0.05
#: How much of a window has to be in the cell for a place to count, as a share of it.
LEAST_SEEN = 0.45
#: The grey lines between the cells: how far from where an even grid puts them to look, how grey
#: they are, and how far either side of one a pixel isn't the cell's.
LINE_LOOK, LINE_GREY, LINE_SPARE = 8, (25, 110), 3


def luma(rgb):
    return rgb[..., :3].astype(np.float64) @ np.array([0.299, 0.587, 0.114])


def _area_axis(a, start, length, n, axis):
    """`a` resampled along one axis by area: output k is the mean of the input over
    [start + k L / n, start + (k + 1) L / n); what's beyond the input counts as nothing."""
    a = np.moveaxis(a, axis, 0)
    size = a.shape[0]
    c = np.concatenate([np.zeros((1,) + a.shape[1:]), np.cumsum(a, axis=0)], axis=0)
    x = np.clip(start + np.arange(n + 1) * (length / n), 0.0, size)
    i = np.minimum(np.floor(x).astype(np.int64), size - 1)
    f = (x - i).reshape((-1,) + (1,) * (a.ndim - 1))
    integral = c[i] + f * (c[i + 1] - c[i])
    return np.moveaxis((integral[1:] - integral[:-1]) / (length / n), 0, axis)


def resize(a, box, size):
    """A box (x, y, w, h) of a 2-D map resampled to size (w, h) by area: exact for shrinking, which
    is all the fit does (the base drawn smaller, as the sheet has it)."""
    x, y, w, h = box
    return _area_axis(_area_axis(a, y, h, size[1], 0), x, w, size[0], 1)


def _fast(n):
    """The next size up that's quick for an FFT (2, 3 and 5 only)."""
    while True:
        m = n
        for p in (2, 3, 5):
            while m % p == 0:
                m //= p
        if m == 1:
            return n
        n += 1


def ncc_map(cell, valid, base, weight, least):
    """Masked normalised cross-correlation at every place at once: score[ty, tx] for the scaled base
    map's corner at (tx, ty) in the cell, tx from -(base width - 1) to cell width - 1 (and the same
    down), counting only the cell's valid pixels and the base's weighted ones, and -1 where less
    than `least` of the weight is in the cell. Returns (score, tx values, ty values)."""
    h, w = cell.shape
    bh, bw = base.shape
    ph, pw = _fast(h + bh - 1), _fast(w + bw - 1)

    def spectrum(a):
        return np.fft.rfft2(a, (ph, pw))

    def corr(fa, fb):
        return np.fft.irfft2(fa * np.conj(fb), (ph, pw))

    fv, fvc, fvcc = spectrum(valid), spectrum(valid * cell), spectrum(valid * cell * cell)
    fm, fme, fmee = spectrum(weight), spectrum(weight * base), spectrum(weight * base * base)
    n = corr(fv, fm)
    sc, se = corr(fvc, fm), corr(fv, fme)
    scc, see, sce = corr(fvcc, fm), corr(fv, fmee), corr(fvc, fme)
    with np.errstate(invalid="ignore", divide="ignore"):
        cov = sce - sc * se / n
        var = np.maximum(scc - sc * sc / n, 1e-9) * np.maximum(see - se * se / n, 1e-9)
        score = cov / np.sqrt(var)
    score[n < least] = -1.0
    score[~np.isfinite(score)] = -1.0
    tys = np.arange(-(bh - 1), h)
    txs = np.arange(-(bw - 1), w)
    return score[np.ix_(tys % ph, txs % pw)], txs, tys


def _parabola(left, mid, right):
    """Where a parabola through three equally spaced scores peaks, from -0.5 to 0.5 of a step."""
    d = left - 2.0 * mid + right
    return 0.0 if d >= 0 else float(np.clip(0.5 * (left - right) / d, -0.5, 0.5))


def coarse(cell, valid, base, weight, scales=None):
    """The best (score, s, tx, ty) over every scale (or `scales`, a (low, high) range) and place, at
    1/COARSE size, in full-size units."""
    small = (cell.shape[1] // COARSE, cell.shape[0] // COARSE)
    c = resize(cell, (0, 0, small[0] * COARSE, small[1] * COARSE), small)
    v = resize(valid, (0, 0, small[0] * COARSE, small[1] * COARSE), small) > 0.999
    total = weight.sum()
    best = (-2.0, 1.0, 0.0, 0.0)
    low, high = scales or (SCALE_LOW, SCALE_HIGH)
    for s in np.arange(low, high + 1e-9, SCALE_STEP):
        k = s / COARSE
        size = (max(2, int(round(base.shape[1] * k))), max(2, int(round(base.shape[0] * k))))
        box = (0, 0, size[0] / k, size[1] / k)
        b, m = resize(base, box, size), resize(weight, box, size)
        score, txs, tys = ncc_map(c, v.astype(np.float64), b, m, LEAST_SEEN * total * k * k)
        j = np.unravel_index(np.argmax(score), score.shape)
        if score[j] > best[0]:
            best = (float(score[j]), float(s), float(txs[j[1]] * COARSE), float(tys[j[0]] * COARSE))
    return best


def _scaled_points(base, weight, s):
    """The base map at scale s, as its weighted points: (ys, xs, values, weights)."""
    size = (int(round(base.shape[1] * s)), int(round(base.shape[0] * s)))
    box = (0, 0, size[0] / s, size[1] / s)
    b, m = resize(base, box, size), resize(weight, box, size)
    ys, xs = np.nonzero(m > 0.05)
    return ys, xs, b[ys, xs], m[ys, xs]


def _scores(cell, valid, pts, places, least):
    """The masked NCC of the cell against the base's points at each integer place (tx, ty)."""
    ys, xs, vals, wts = pts
    h, w = cell.shape
    out = np.full(len(places), -1.0)
    for i, (tx, ty) in enumerate(places):
        py, px = ys + ty, xs + tx
        inside = (py >= 0) & (py < h) & (px >= 0) & (px < w)
        py, px = py[inside], px[inside]
        k = valid[py, px] > 0.5
        wt = wts[inside][k]
        n = wt.sum()
        if n < least:
            continue
        a, b = cell[py[k], px[k]], vals[inside][k]
        ma, mb = (wt * a).sum() / n, (wt * b).sum() / n
        da, db = a - ma, b - mb
        den = np.sqrt((wt * da * da).sum() * (wt * db * db).sum())
        if den > 0:
            out[i] = (wt * da * db).sum() / den
    return out


def refine(cell, valid, base, weight, start, steps=FINE_SCALES):
    """The coarse fit (score, s, tx, ty) made exact: at full size, FINE_SCALES steps of FINE_SCALE
    either side, FINE_PLACE pixels either side of where each scale puts the window's middle, then a
    parabola through the best and its neighbours in scale, across and down. Returns (score, s, tx, ty)."""
    _, s0, tx0, ty0 = start
    ys, xs = np.nonzero(weight > 0.05)
    anchor = np.array([xs.mean(), ys.mean()])      # the windows' middle, on the canvas
    fixed = np.array([tx0, ty0]) + s0 * anchor     # ... and where the coarse fit puts it in the cell
    total = weight.sum()
    offsets = [(dx, dy) for dy in range(-FINE_PLACE, FINE_PLACE + 1) for dx in range(-FINE_PLACE, FINE_PLACE + 1)]
    rows = []
    for k in range(-steps, steps + 1):
        s = s0 + k * FINE_SCALE
        pts = _scaled_points(base, weight, s)
        centre = np.rint(fixed - s * anchor).astype(int)
        places = [(centre[0] + dx, centre[1] + dy) for dx, dy in offsets]
        scores = _scores(cell, valid, pts, places, LEAST_SEEN * total * s * s)
        j = int(np.argmax(scores))
        grid = scores.reshape(2 * FINE_PLACE + 1, 2 * FINE_PLACE + 1)
        gy, gx = divmod(j, 2 * FINE_PLACE + 1)
        fx = _parabola(grid[gy, gx - 1], grid[gy, gx], grid[gy, gx + 1]) if 0 < gx < 2 * FINE_PLACE else 0.0
        fy = _parabola(grid[gy - 1, gx], grid[gy, gx], grid[gy + 1, gx]) if 0 < gy < 2 * FINE_PLACE else 0.0
        tx, ty = places[j][0] + fx, places[j][1] + fy
        rows.append((float(scores[j]), s, s, tx, ty))
    best = int(np.argmax([r[0] for r in rows]))
    score, s, sx, tx, ty = rows[best]
    if 0 < best < len(rows) - 1:
        f = _parabola(rows[best - 1][0], score, rows[best + 1][0])
        # Moving the scale by f steps keeps the windows' middle where it is.
        ds = f * FINE_SCALE
        tx -= ds * anchor[0]
        ty -= ds * anchor[1]
        sx += ds
    return score, sx, tx, ty


def refine_all(cell, valid, base, weight, start, tries=4):
    """refine, again round the end of its range when the best is there (the coarse step was off by
    more than the fine range), up to `tries` times."""
    best = refine(cell, valid, base, weight, start)
    for _ in range(tries):
        low, high = start[1] - FINE_SCALES * FINE_SCALE, start[1] + FINE_SCALES * FINE_SCALE
        if low + FINE_SCALE * 0.5 < best[1] < high - FINE_SCALE * 0.5:
            break
        start = best
        again = refine(cell, valid, base, weight, start)
        if again[0] <= best[0]:
            break
        best = again
    return best


def fit(cell_rgb, valid, base_rgb, weight, near=None, span=NEAR_SCALE, strict=False, fixed=False):
    """Where the base lies under one cell: (score, s, tx, ty), canvas (X, Y) at cell (tx + s X, ty + s Y).
    `near`, a scale found for the cells before it in the row, narrows the search to `span` round it;
    `strict` keeps the fine search inside that too (not following the score past it); `fixed` holds
    the scale at `near` and finds only the place."""
    cell, base = luma(cell_rgb), luma(base_rgb)
    v = valid.astype(np.float64)
    if fixed:
        start = coarse(cell, v, base, weight, (near, near))
        start = (start[0], near, start[2], start[3])
        return refine(cell, v, base, weight, start, steps=0)
    scales = (near - span, near + span) if near else None
    start = coarse(cell, v, base, weight, scales)
    return refine(cell, v, base, weight, start) if strict else refine_all(cell, v, base, weight, start)


def footprint(shape, cell_size, s, tx, ty, spare):
    """The canvas pixels a fitted cell covers, `spare` pixels in from its edges (a bool canvas)."""
    h, w = shape
    cw, ch = cell_size
    xs = (np.arange(w) * s + tx)
    ys = (np.arange(h) * s + ty)
    inside_x = (xs >= spare) & (xs <= cw - 1 - spare)
    inside_y = (ys >= spare) & (ys <= ch - 1 - spare)
    return inside_y[:, None] & inside_x[None, :]


def cell_boxes(rgb, cols, rows):
    """Each cell of a sheet, left to right then top to bottom, as (x0, y0, x1, y1) between its grey
    dividing lines, found near where an even grid puts them (or the even grid's, where there's
    none), less LINE_SPARE either side."""
    h, w = rgb.shape[:2]
    lum = luma(rgb)
    chroma = rgb[..., :3].max(axis=2).astype(np.int16) - rgb[..., :3].min(axis=2)
    grey = (lum > LINE_GREY[0]) & (lum < LINE_GREY[1]) & (chroma < 18)

    def lines(count, length, along):
        cuts = [0]
        for k in range(1, count):
            guess = int(round(k * length / count))
            lo, hi = max(1, guess - LINE_LOOK), min(length - 1, guess + LINE_LOOK + 1)
            share = grey[:, lo:hi].mean(axis=0) if along == "x" else grey[lo:hi, :].mean(axis=1)
            cuts.append(lo + int(np.argmax(share)) if share.max() > 0.5 else guess)
        cuts.append(length)
        return cuts

    xs, ys = lines(cols, w, "x"), lines(rows, h, "y")
    boxes = []
    for r in range(rows):
        for c in range(cols):
            x0 = xs[c] + (LINE_SPARE if c > 0 else 1)
            x1 = xs[c + 1] - (LINE_SPARE if c < cols - 1 else 1)
            y0 = ys[r] + (LINE_SPARE if r > 0 else 1)
            y1 = ys[r + 1] - (LINE_SPARE if r < rows - 1 else 1)
            boxes.append((x0, y0, x1, y1))
    return boxes
