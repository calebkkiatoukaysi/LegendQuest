"""
The rig's drawing library: anti-aliased vector shapes into numpy pictures, by 4x4 supersampling.
Every part of every kit is drawn with it (03-art-overhaul.md §12, the art plan's §2.7).

A Canvas is one part's picture at its texel size (density 2: a texel is half a screen pixel). Under
it are four buffers at ss x ss samples per texel:

    cov    float64   coverage of the sample, 0..1
    rgb    float64   its colour (a fixed colour, or the preview colour of a tinted one)
    grp    int16     its tint group (art plan §2.3: 0 keeps the colour, 1 skin, 2 hair ...)
    pos    float64   its ramp index, 0 the line and 1..9 deep to highlight (theme.py)

Drawing writes samples; reading out averages each ss x ss block into a texel. Out come three
pictures: the preview (rgba), the kit's fill (tinted texels as the grey of their ramp index) and its
tint mask (R group, G ramp index as round(index * 255 / 9), A coverage), plus the white `#line`
silhouette, the part dilated by the outer line's width beyond its own edge line (style.py).

    c = Canvas(64, 96)                               # 64x96 texels, 256x384 samples
    body = c.fill(Path.capsule((32, 12), (32, 84), 14, 10), (200, 200, 200, 255), group=1, grey=5)
    c.stroke(Path.polyline([(20, 30), (40, 60)]), (2, 0), (60, 40, 60, 255))   # tapered 2 -> 0
    c.write("x.png")

Paths are in texels, y down. Fills use the even-odd rule unless told `rule="nonzero"`. Strokes
are chains of tapered capsules, so a width can change along the path and end in a point.

Deterministic, as every tool here: float64, elementwise, sums in a fixed order, np.rint before a
cast to uint8, PNGs with filter 0 and zlib level 9, and no `random`: noise is pixels.hash01.

Only needs CPython and numpy.
"""

import math
import os
import struct
import sys
import zlib

import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))
TOOLS = os.path.dirname(HERE)
ROOT = os.path.dirname(TOOLS)
sys.path.insert(0, TOOLS)
from pixels import hash01  # noqa: E402,F401  (re-exported: the only noise there is)

SS = 4
"""Samples per texel along each axis."""

STEPS = 9
"""The ramp index runs 0 (the line) to STEPS (highlight); a mask's G is round(index * 255 / STEPS)."""


# --------------------------------------------------------------------------- #
# PNG files
# --------------------------------------------------------------------------- #

def write_png(path, rgba, quiet=False):
    """Writes an H x W x 4 uint8 array as an 8-bit RGBA PNG: filter 0 on every row, zlib level 9,
    no other chunks. The same bytes pixels.write_png makes, without a list per pixel."""
    a = np.ascontiguousarray(rgba, dtype=np.uint8)
    h, w = a.shape[0], a.shape[1]
    if a.ndim == 2 or a.shape[2] != 4:
        raise ValueError("write_png wants H x W x 4, not " + str(a.shape))
    raw = np.zeros((h, w * 4 + 1), np.uint8)
    raw[:, 1:] = a.reshape(h, w * 4)

    def chunk(tag, data):
        body = tag + data
        return struct.pack(">I", len(data)) + body + struct.pack(">I", zlib.crc32(body) & 0xFFFFFFFF)

    header = struct.pack(">IIBBBBB", w, h, 8, 6, 0, 0, 0)
    blob = (b"\x89PNG\r\n\x1a\n" + chunk(b"IHDR", header)
            + chunk(b"IDAT", zlib.compress(raw.tobytes(), 9)) + chunk(b"IEND", b""))
    folder = os.path.dirname(path)
    if folder:
        os.makedirs(folder, exist_ok=True)
    with open(path, "wb") as f:
        f.write(blob)
    if not quiet:
        try:
            shown = os.path.relpath(path, ROOT).replace(os.sep, "/")
        except ValueError:
            shown = path
        print("  " + shown + "  (" + str(w) + "x" + str(h) + ")")


def read_png(path):
    """Reads an 8-bit, non-interlaced RGB or RGBA PNG into an H x W x 4 uint8 array (RGB gets alpha
    255). Unfilters with numpy, the left-dependent filters (Sub, Average, Paeth) a diagonal at a
    time, so a 2048-square painted sheet takes a second, not a minute."""
    data = open(path, "rb").read()
    if data[:8] != b"\x89PNG\r\n\x1a\n":
        raise ValueError(path + " isn't a PNG")
    pos, idat = 8, []
    w = h = depth = ctype = interlace = None
    while pos < len(data):
        ln, = struct.unpack(">I", data[pos:pos + 4])
        tag, body = data[pos + 4:pos + 8], data[pos + 8:pos + 8 + ln]
        pos += 12 + ln
        if tag == b"IHDR":
            w, h, depth, ctype, _, _, interlace = struct.unpack(">IIBBBBB", body)
        elif tag == b"IDAT":
            idat.append(body)
        elif tag == b"IEND":
            break
    if depth != 8 or interlace != 0 or ctype not in (2, 6):
        raise ValueError(path + " has to be 8-bit RGB or RGBA, not interlaced")
    ch = 3 if ctype == 2 else 4
    raw = np.frombuffer(zlib.decompress(b"".join(idat)), np.uint8).reshape(h, w * ch + 1)
    kinds = raw[:, 0].astype(np.int64)
    filt = raw[:, 1:].reshape(h, w, ch).astype(np.int64)
    out = np.zeros((h + 1, w + 1, ch), np.int64)        # a zero row above and column to the left
    if np.all(kinds == 0):
        out[1:, 1:] = filt
    elif np.all((kinds == 0) | (kinds == 2)):
        for y in range(h):
            out[y + 1, 1:] = (filt[y] + (out[y, 1:] if kinds[y] == 2 else 0)) & 255
    else:
        for k in range(w + h - 1):
            ys = np.arange(max(0, k - w + 1), min(h, k + 1))
            xs = k - ys
            r = filt[ys, xs]
            a = out[ys + 1, xs]
            b = out[ys, xs + 1]
            c = out[ys, xs]
            ft = kinds[ys][:, None]
            p = a + b - c
            pa, pb, pc = np.abs(p - a), np.abs(p - b), np.abs(p - c)
            paeth = np.where((pa <= pb) & (pa <= pc), a, np.where(pb <= pc, b, c))
            pred = np.where(ft == 1, a, np.where(ft == 2, b, np.where(ft == 3, (a + b) >> 1,
                                                                      np.where(ft == 4, paeth, 0))))
            out[ys + 1, xs + 1] = (r + pred) & 255
    img = out[1:, 1:].astype(np.uint8)
    if ch == 3:
        img = np.concatenate([img, np.full((h, w, 1), 255, np.uint8)], axis=2)
    return img


def to_u8(x):
    """Rounds 0..255 floats to uint8, clamped, the one way everything here rounds."""
    return np.rint(np.clip(x, 0.0, 255.0)).astype(np.uint8)


# --------------------------------------------------------------------------- #
# Paths
# --------------------------------------------------------------------------- #

def _bezier_steps(points, ss):
    length = 0.0
    for (x0, y0), (x1, y1) in zip(points, points[1:]):
        length += math.hypot(x1 - x0, y1 - y0)
    return max(4, min(256, int(math.ceil(length * ss / 3.0))))


class Path:
    """A shape in texels: sub-paths of points, each open or closed. Curves flatten to short lines
    (about 3/4 of a texel at ss 4) as they're added, so a Path is just polylines underneath.

        Path().move(0, 0).line(10, 0).cubic(14, 0, 16, 4, 16, 8).close()
        Path.circle(8, 8, 6); Path.ellipse(8, 8, 6, 4, rot=20); Path.rect(0, 0, 10, 4, r=2)
        Path.capsule((0, 0), (0, 30), 6, 4)       # round ends, radius 6 at the start, 4 at the end
        Path.polygon(pts); Path.polyline(pts); Path.smooth(pts, closed=True)  # Catmull-Rom
    """

    def __init__(self, ss=SS):
        self.subs = []          # [ [ [x, y], ... ], closed ]
        self.ss = ss

    # building -------------------------------------------------------------- #

    def _cur(self):
        if not self.subs or self.subs[-1][1]:
            raise ValueError("draw from a move() first")
        return self.subs[-1][0]

    def move(self, x, y):
        self.subs.append([[(float(x), float(y))], False])
        return self

    def line(self, x, y):
        self._cur().append((float(x), float(y)))
        return self

    def quad(self, cx, cy, x, y):
        pts = self._cur()
        x0, y0 = pts[-1]
        n = _bezier_steps([(x0, y0), (cx, cy), (x, y)], self.ss)
        for i in range(1, n + 1):
            t = i / n
            u = 1.0 - t
            pts.append((u * u * x0 + 2 * u * t * cx + t * t * x, u * u * y0 + 2 * u * t * cy + t * t * y))
        return self

    def cubic(self, c1x, c1y, c2x, c2y, x, y):
        pts = self._cur()
        x0, y0 = pts[-1]
        n = _bezier_steps([(x0, y0), (c1x, c1y), (c2x, c2y), (x, y)], self.ss)
        for i in range(1, n + 1):
            t = i / n
            u = 1.0 - t
            a, b, c, d = u * u * u, 3 * u * u * t, 3 * u * t * t, t * t * t
            pts.append((a * x0 + b * c1x + c * c2x + d * x, a * y0 + b * c1y + c * c2y + d * y))
        return self

    def arc(self, cx, cy, r, a0, a1, ry=None):
        """Adds an arc of the circle (or ellipse, with ry) round (cx, cy) from angle a0 to a1, in
        degrees clockwise from +x (y is down). Starts a sub-path if there isn't an open one."""
        ry = r if ry is None else ry
        n = max(4, min(256, int(math.ceil(abs(a1 - a0) / 360.0 * 2 * math.pi * max(r, ry) * self.ss / 3.0))))
        first = (cx + r * math.cos(math.radians(a0)), cy + ry * math.sin(math.radians(a0)))
        if not self.subs or self.subs[-1][1]:
            self.move(*first)
        else:
            self.line(*first)
        for i in range(1, n + 1):
            a = math.radians(a0 + (a1 - a0) * i / n)
            self._cur().append((cx + r * math.cos(a), cy + ry * math.sin(a)))
        return self

    def close(self):
        if self.subs and not self.subs[-1][1]:
            self.subs[-1][1] = True
        return self

    def add(self, other):
        """Appends another path's sub-paths (an even-odd fill of both makes holes where they overlap)."""
        for pts, closed in other.subs:
            self.subs.append([list(pts), closed])
        return self

    # shapes ---------------------------------------------------------------- #

    @staticmethod
    def polygon(points):
        p = Path().move(*points[0])
        for q in points[1:]:
            p.line(*q)
        return p.close()

    @staticmethod
    def polyline(points):
        p = Path().move(*points[0])
        for q in points[1:]:
            p.line(*q)
        return p

    @staticmethod
    def smooth(points, closed=False, tension=0.5):
        """A Catmull-Rom curve through the points, as cubic Béziers."""
        pts = [tuple(map(float, q)) for q in points]
        n = len(pts)
        p = Path().move(*pts[0])
        last = n if closed else n - 1
        for i in range(last):
            p0 = pts[(i - 1) % n] if (closed or i > 0) else pts[i]
            p1 = pts[i]
            p2 = pts[(i + 1) % n]
            p3 = pts[(i + 2) % n] if (closed or i + 2 < n) else p2
            k = tension / 3.0 * 2.0
            c1 = (p1[0] + (p2[0] - p0[0]) * k / 2, p1[1] + (p2[1] - p0[1]) * k / 2)
            c2 = (p2[0] - (p3[0] - p1[0]) * k / 2, p2[1] - (p3[1] - p1[1]) * k / 2)
            p.cubic(c1[0], c1[1], c2[0], c2[1], p2[0], p2[1])
        return p.close() if closed else p

    @staticmethod
    def circle(cx, cy, r):
        return Path().arc(cx, cy, r, 0, 360).close()

    @staticmethod
    def ellipse(cx, cy, rx, ry, rot=0.0):
        p = Path().arc(0, 0, rx, 0, 360, ry).close()
        return p.rotate(rot).translate(cx, cy)

    @staticmethod
    def rect(x, y, w, h, r=0.0):
        if r <= 0:
            return Path.polygon([(x, y), (x + w, y), (x + w, y + h), (x, y + h)])
        r = min(r, w / 2.0, h / 2.0)
        p = Path()
        p.arc(x + w - r, y + r, r, -90, 0)
        p.arc(x + w - r, y + h - r, r, 0, 90)
        p.arc(x + r, y + h - r, r, 90, 180)
        p.arc(x + r, y + r, r, 180, 270)
        return p.close()

    @staticmethod
    def capsule(a, b, ra, rb=None):
        """A limb: round ends of radius ra at a and rb at b, joined by tangent lines."""
        rb = ra if rb is None else rb
        ax, ay = map(float, a)
        bx, by = map(float, b)
        dx, dy = bx - ax, by - ay
        d = math.hypot(dx, dy)
        if d < 1e-9:
            return Path.circle(ax, ay, max(ra, rb))
        ang = math.degrees(math.atan2(dy, dx))
        s = max(-1.0, min(1.0, (ra - rb) / d))
        off = math.degrees(math.acos(s))     # where the tangents touch, measured from a -> b
        p = Path()
        p.arc(bx, by, rb, ang - off, ang + off)
        p.arc(ax, ay, ra, ang + off, ang + 360 - off)
        return p.close()

    # transforms ------------------------------------------------------------ #

    def map(self, f):
        q = Path(self.ss)
        for pts, closed in self.subs:
            q.subs.append([[f(x, y) for x, y in pts], closed])
        return q

    def translate(self, dx, dy):
        return self.map(lambda x, y: (x + dx, y + dy))

    def scale(self, sx, sy=None, cx=0.0, cy=0.0):
        sy = sx if sy is None else sy
        return self.map(lambda x, y: (cx + (x - cx) * sx, cy + (y - cy) * sy))

    def rotate(self, deg, cx=0.0, cy=0.0):
        c, s = math.cos(math.radians(deg)), math.sin(math.radians(deg))
        return self.map(lambda x, y: (cx + (x - cx) * c - (y - cy) * s, cy + (x - cx) * s + (y - cy) * c))

    def affine(self, m):
        """m = (a, b, c, d, e, f): x' = a x + b y + c, y' = d x + e y + f."""
        a, b, c, d, e, f = m
        return self.map(lambda x, y: (a * x + b * y + c, d * x + e * y + f))

    def mirror(self, cx=0.0):
        return self.map(lambda x, y: (2 * cx - x, y))

    # reading --------------------------------------------------------------- #

    def polylines(self):
        """The sub-paths as (N x 2 float64 array, closed)."""
        return [(np.array(pts, np.float64), closed) for pts, closed in self.subs if len(pts) > 1]

    def bounds(self):
        allp = [q for pts, _ in self.subs for q in pts]
        xs = [q[0] for q in allp]
        ys = [q[1] for q in allp]
        return min(xs), min(ys), max(xs), max(ys)

    def length(self):
        total = 0.0
        for arr, closed in self.polylines():
            if closed:
                arr = np.vstack([arr, arr[:1]])
            total += float(np.sqrt(((arr[1:] - arr[:-1]) ** 2).sum(1)).sum())
        return total

    def at(self, t):
        """The point and unit tangent a fraction t along the first sub-path's length."""
        arr, closed = self.polylines()[0]
        if closed:
            arr = np.vstack([arr, arr[:1]])
        seg = np.sqrt(((arr[1:] - arr[:-1]) ** 2).sum(1))
        cum = np.concatenate([[0.0], np.cumsum(seg)])
        target = max(0.0, min(1.0, t)) * cum[-1]
        i = int(min(len(seg) - 1, max(0, np.searchsorted(cum, target, side="right") - 1)))
        u = 0.0 if seg[i] == 0 else (target - cum[i]) / seg[i]
        p = arr[i] + (arr[i + 1] - arr[i]) * u
        d = arr[i + 1] - arr[i]
        n = math.hypot(d[0], d[1]) or 1.0
        return (float(p[0]), float(p[1])), (float(d[0] / n), float(d[1] / n))

    def resample(self, step):
        """The first sub-path as points every `step` texels along it (ends included)."""
        n = max(2, int(math.ceil(self.length() / max(step, 1e-6))) + 1)
        return [self.at(i / (n - 1))[0] for i in range(n)]


# --------------------------------------------------------------------------- #
# Rasterising, at sample resolution
# --------------------------------------------------------------------------- #

def raster_fill(path, w, h, ss=SS, rule="evenodd"):
    """See _raster_fill; works only on the rows the path covers."""
    H, W = h * ss, w * ss
    subs = path.polylines()
    if not subs:
        return np.zeros((H, W), bool)
    ys = np.concatenate([arr[:, 1] for arr, _ in subs]) * ss
    r0 = int(max(0, math.floor(ys.min() - 1)))
    r1 = int(min(H, math.ceil(ys.max() + 1)))
    out = np.zeros((H, W), bool)
    if r1 <= r0:
        return out
    shifted = path.translate(0, -r0 / ss)
    out[r0:r1] = _raster_fill(shifted, w, (r1 - r0), ss, rule, rows=r1 - r0)
    return out


def _raster_fill(path, w, h, ss=SS, rule="evenodd", rows=None):
    """Which samples of a w x h texel picture (at ss x ss each) are inside the path: an (h*ss) x
    (w*ss) bool array. Sample centres are at ((i + 0.5) / ss) texels; a crossing toggles from the
    first sample whose centre is at or right of it, so shared edges never leave gaps or overlap."""
    H, W = (rows if rows is not None else h * ss), w * ss
    xs0, ys0, xs1, ys1, dirs = [], [], [], [], []
    for arr, _closed in path.polylines():
        a = arr * ss
        b = np.roll(a, -1, axis=0)      # every sub-path is filled as closed
        xs0.append(a[:, 0]); ys0.append(a[:, 1]); xs1.append(b[:, 0]); ys1.append(b[:, 1])
    if not xs0:
        return np.zeros((H, W), bool)
    x0 = np.concatenate(xs0); y0 = np.concatenate(ys0); x1 = np.concatenate(xs1); y1 = np.concatenate(ys1)
    keep = y0 != y1
    x0, y0, x1, y1 = x0[keep], y0[keep], x1[keep], y1[keep]
    up = y1 < y0
    ylo = np.where(up, y1, y0); yhi = np.where(up, y0, y1)
    xlo = np.where(up, x1, x0); xhi = np.where(up, x0, x1)
    wind = np.where(up, -1, 1)
    # rows j with centre c = j + 0.5 in [ylo, yhi)
    j0 = np.clip(np.ceil(ylo - 0.5), 0, H).astype(np.int64)
    j1 = np.clip(np.ceil(yhi - 0.5), 0, H).astype(np.int64)
    span = j1 - j0
    ok = span > 0
    if not np.any(ok):
        return np.zeros((H, W), bool)
    j0, span, xlo, xhi, ylo, yhi, wind = j0[ok], span[ok], xlo[ok], xhi[ok], ylo[ok], yhi[ok], wind[ok]
    idx = np.repeat(np.arange(len(j0)), span)
    starts = np.cumsum(span) - span
    rows = j0[idx] + (np.arange(len(idx)) - starts[idx])
    yc = rows + 0.5
    t = (yc - ylo[idx]) / (yhi[idx] - ylo[idx])
    xc = xlo[idx] + t * (xhi[idx] - xlo[idx])
    cols = np.clip(np.ceil(xc - 0.5), 0, W).astype(np.int64)
    acc = np.zeros((H, W + 1), np.int64)
    np.add.at(acc, (rows, cols), wind[idx] if rule == "nonzero" else 1)
    run = np.cumsum(acc, axis=1)[:, :W]
    return (run != 0) if rule == "nonzero" else ((run & 1) == 1)


def _widths_along(widths, n, cum):
    total = cum[-1] if cum[-1] > 0 else 1.0
    t = cum / total
    if callable(widths):
        return np.array([float(widths(float(u))) for u in t], np.float64)
    if isinstance(widths, (int, float)):
        return np.full(n, float(widths))
    widths = list(widths)
    if len(widths) == n:
        return np.array(widths, np.float64)
    if len(widths) == 2:
        return widths[0] + (widths[1] - widths[0]) * t
    # several stops spread evenly along the length
    stops = np.linspace(0.0, 1.0, len(widths))
    return np.interp(t, stops, np.array(widths, np.float64))


def raster_stroke(path, widths, w, h, ss=SS):
    """The samples a stroke covers: each sub-path drawn as a chain of capsules whose width follows
    `widths` (a number; (start, end) tapering along the length; one per point; several stops
    spread along the length; or a function of t in 0..1). Widths are full widths, in texels."""
    H, W = h * ss, w * ss
    out = np.zeros((H, W), bool)
    for arr, closed in path.polylines():
        if closed:
            arr = np.vstack([arr, arr[:1]])
        given = len(arr)
        if not (isinstance(widths, (list, tuple)) and len(widths) == given and given > 2):
            # cut long segments into pieces of 3/4 texel at most, so a width that changes along
            # the path (a taper, a swell) is followed even on a two-point line
            pieces = [arr[:1]]
            for a, b in zip(arr[:-1], arr[1:]):
                n = max(1, int(math.ceil(float(np.hypot(*(b - a))) / 0.75)))
                t = (np.arange(1, n + 1, dtype=np.float64) / n)[:, None]
                pieces.append(a + (b - a) * t)
            arr = np.vstack(pieces)
        seg = np.sqrt(((arr[1:] - arr[:-1]) ** 2).sum(1))
        cum = np.concatenate([[0.0], np.cumsum(seg)])
        rad = _widths_along(widths, len(arr), cum) * 0.5 * ss
        pts = arr * ss
        for i in range(len(pts) - 1):
            (ax, ay), (bx, by) = pts[i], pts[i + 1]
            ra, rb = rad[i], rad[i + 1]
            r = max(ra, rb)
            if r <= 0:
                continue
            c0 = int(max(0, math.floor(min(ax, bx) - r - 1)))
            c1 = int(min(W, math.ceil(max(ax, bx) + r + 1)))
            r0 = int(max(0, math.floor(min(ay, by) - r - 1)))
            r1 = int(min(H, math.ceil(max(ay, by) + r + 1)))
            if c1 <= c0 or r1 <= r0:
                continue
            sy, sx = np.mgrid[r0:r1, c0:c1]
            px = sx + 0.5 - ax
            py = sy + 0.5 - ay
            dx, dy = bx - ax, by - ay
            dd = dx * dx + dy * dy
            if dd > 0:
                t = np.clip((px * dx + py * dy) / dd, 0.0, 1.0)
            else:
                t = np.zeros_like(px, dtype=np.float64)
            qx = px - t * dx
            qy = py - t * dy
            rr = ra + (rb - ra) * t
            out[r0:r1, c0:c1] |= (qx * qx + qy * qy) <= rr * rr
    return out


# --------------------------------------------------------------------------- #
# Distance fields
# --------------------------------------------------------------------------- #

def distance(inside, rmax, rx=1.0, ry=1.0):
    """For every cell, the distance to the nearest True cell of `inside` (0 on them), capped at
    rmax, in cells. With rx and ry it's the elliptical distance sqrt((dx/rx)^2 + (dy/ry)^2): the
    reach of an ellipse of those radii. Exact (separable: rows, then columns), bounded by rmax."""
    inside = np.asarray(inside, bool)
    H, W = inside.shape
    Rx = int(math.ceil(rmax * rx)) + 1
    Ry = int(math.ceil(rmax * ry)) + 1
    big = float(Rx + 1)
    # along rows: how far the nearest True cell is, from running indices of the last one seen
    idx = np.arange(W, dtype=np.float64)[None, :]
    last = np.maximum.accumulate(np.where(inside, idx, -1e9), axis=1)
    nxt = np.minimum.accumulate(np.where(inside, idx, 1e9)[:, ::-1], axis=1)[:, ::-1]
    g = np.minimum(np.minimum(idx - last, nxt - idx), big)
    gx = (g / rx) ** 2
    d2 = gx.copy()
    for k in range(1, min(Ry, H - 1) + 1):
        dyk = (k / ry) ** 2
        d2[k:] = np.minimum(d2[k:], gx[:-k] + dyk)
        d2[:-k] = np.minimum(d2[:-k], gx[k:] + dyk)
    return np.minimum(np.sqrt(d2), float(rmax))


def depth(inside, rmax):
    """Inside distance: for every True cell, how far it is to the nearest False cell (the edge),
    capped at rmax; 0 outside. The picture's border counts as outside."""
    inside = np.asarray(inside, bool)
    padded = np.zeros((inside.shape[0] + 2, inside.shape[1] + 2), bool)
    padded[1:-1, 1:-1] = ~inside
    d = distance(padded, rmax)[1:-1, 1:-1]
    return np.where(inside, d, 0.0)


def block_mean(a, ss):
    """The mean of every ss x ss block, summed in a fixed order so reruns match to the bit."""
    H, W = a.shape[0], a.shape[1]
    h, w = H // ss, W // ss
    acc = np.zeros((h, w) + a.shape[2:], np.float64)
    for dy in range(ss):
        for dx in range(ss):
            acc = acc + a[dy::ss, dx::ss]
    return acc / float(ss * ss)


def blur(a, r=1, passes=2):
    """A box blur of radius r cells, `passes` times (two is a tent), edges clamped. Fixed order."""
    out = np.asarray(a, np.float64)
    for _ in range(passes):
        for axis in (0, 1):
            pad = [(0, 0), (0, 0)]
            pad[axis] = (r, r)
            p = np.pad(out, pad, mode="edge")
            acc = np.zeros_like(out)
            n = out.shape[axis]
            for k in range(2 * r + 1):
                acc = acc + (p[k:k + n] if axis == 0 else p[:, k:k + n])
            out = acc / float(2 * r + 1)
    return out


def upsample(a, ss):
    """Bilinear: a texel field to sample resolution, sample centres at (i + 0.5) / ss."""
    h, w = a.shape
    ys = (np.arange(h * ss) + 0.5) / ss - 0.5
    xs = (np.arange(w * ss) + 0.5) / ss - 0.5
    y0 = np.clip(np.floor(ys).astype(np.int64), 0, h - 1)
    x0 = np.clip(np.floor(xs).astype(np.int64), 0, w - 1)
    y1 = np.clip(y0 + 1, 0, h - 1)
    x1 = np.clip(x0 + 1, 0, w - 1)
    fy = np.clip(ys - y0, 0.0, 1.0)[:, None]
    fx = np.clip(xs - x0, 0.0, 1.0)[None, :]
    a00 = a[y0][:, x0]; a01 = a[y0][:, x1]; a10 = a[y1][:, x0]; a11 = a[y1][:, x1]
    return (a00 * (1 - fx) + a01 * fx) * (1 - fy) + (a10 * (1 - fx) + a11 * fx) * fy


# --------------------------------------------------------------------------- #
# Masks: regions of a canvas
# --------------------------------------------------------------------------- #

class Mask:
    """A region of a Canvas, as a sample-resolution float array (1 inside, 0 outside, between for
    soft edges). Made by Canvas.shape / fill / stroke; combined with | & - ~; moved and grown in
    texels. Painting with one only touches its samples.

        body = c.shape(path); arm = c.shape(arm_path)
        c.tone(body - arm, tones, "shadow")              # the body where the arm isn't
        under = arm.shift(0, 3) & body                   # the arm's cast shadow, 3 texels down
        rim_zone = body.inset(3) - body.inset(6)         # a band 3..6 texels in from the edge
    """

    def __init__(self, canvas, a):
        self.canvas = canvas
        self.a = np.asarray(a, np.float64)

    def _other(self, o):
        return o.a if isinstance(o, Mask) else np.asarray(o, np.float64)

    def __or__(self, o):
        return Mask(self.canvas, np.maximum(self.a, self._other(o)))

    def __and__(self, o):
        return Mask(self.canvas, np.minimum(self.a, self._other(o)))

    def __sub__(self, o):
        return Mask(self.canvas, np.minimum(self.a, 1.0 - self._other(o)))

    def __invert__(self):
        return Mask(self.canvas, 1.0 - self.a)

    def scaled(self, k):
        """The same region at k strength (k * coverage)."""
        return Mask(self.canvas, self.a * k)

    @property
    def inside(self):
        """The samples at least half in, as bools."""
        return self.a >= 0.5

    def any(self):
        return bool(np.any(self.a > 0))

    def area(self):
        """In texels."""
        return float(self.a.sum()) / (self.canvas.ss ** 2)

    def bounds(self):
        """(x0, y0, x1, y1) in texels of the samples at least half in, or None."""
        ys, xs = np.nonzero(self.inside)
        if len(xs) == 0:
            return None
        ss = self.canvas.ss
        return xs.min() / ss, ys.min() / ss, (xs.max() + 1) / ss, (ys.max() + 1) / ss

    def shift(self, dx, dy):
        """Moved by (dx, dy) texels (rounded to samples); what moves in from the border is empty."""
        ss = self.canvas.ss
        sx, sy = int(round(dx * ss)), int(round(dy * ss))
        out = np.zeros_like(self.a)
        H, W = self.a.shape
        ys0, ys1 = max(0, sy), min(H, H + sy)
        xs0, xs1 = max(0, sx), min(W, W + sx)
        if ys1 > ys0 and xs1 > xs0:
            out[ys0:ys1, xs0:xs1] = self.a[ys0 - sy:ys1 - sy, xs0 - sx:xs1 - sx]
        return Mask(self.canvas, out)

    def grow(self, r, ry=None):
        """Dilated by r texels (ry vertically if given)."""
        ss = self.canvas.ss
        ry = r if ry is None else ry
        if r <= 0 and ry <= 0:
            return Mask(self.canvas, self.a.copy())
        big = max(r, ry)
        box = self._box(int(math.ceil(big * ss)) + 2)
        out = self.a.copy()
        if box is None:
            return Mask(self.canvas, out)
        d = distance(self.inside[box], big * ss + 2, rx=max(r, 1e-3) / big, ry=max(ry, 1e-3) / big)
        out[box] = np.maximum(out[box], (d <= big * ss).astype(np.float64))
        return Mask(self.canvas, out)

    def _box(self, pad):
        """The slices round what's inside, padded by `pad` samples, or None if nothing is."""
        ins = self.inside
        rows = np.nonzero(ins.any(axis=1))[0]
        if len(rows) == 0:
            return None
        cols = np.nonzero(ins.any(axis=0))[0]
        H, W = ins.shape
        return (slice(max(0, rows[0] - pad), min(H, rows[-1] + 1 + pad)),
                slice(max(0, cols[0] - pad), min(W, cols[-1] + 1 + pad)))

    def inset(self, r):
        """Eroded by r texels: what's at least r in from the edge."""
        if r <= 0:
            return Mask(self.canvas, self.a.copy())
        ss = self.canvas.ss
        out = np.zeros_like(self.a)
        box = self._box(1)
        if box is None:
            return Mask(self.canvas, out)
        d = depth(self.inside[box], r * ss + 1)
        out[box] = (d > r * ss).astype(np.float64)
        return Mask(self.canvas, out)

    def depth(self, rmax, res=2, smooth=0):
        """Inside distance, in texels, capped at rmax, at sample resolution. Worked out at `res`
        cells per texel and brought up bilinearly, which is plenty for shading; `smooth` blurs it
        that many cells first (shading uses 2, so normals don't step)."""
        ss = self.canvas.ss
        step = max(1, ss // res)
        cov = block_mean(self.a, step) if step > 1 else self.a
        d = depth(cov >= 0.5, rmax * res + 1) / res
        if smooth:
            d = blur(d, int(smooth), 2)
        if step > 1:
            d = upsample(d, step)
        return np.where(self.inside, np.minimum(d, rmax), 0.0)

    def band(self, r0, r1):
        """The samples between r0 and r1 texels in from the edge."""
        d = self.depth(r1 + 1, res=self.canvas.ss)
        return Mask(self.canvas, ((d > r0) & (d <= r1) & self.inside).astype(np.float64))

    def above(self, y):
        """The part of this region above y (texels)."""
        ss = self.canvas.ss
        rows = (np.arange(self.a.shape[0]) + 0.5) / ss
        return Mask(self.canvas, self.a * (rows < y)[:, None])

    def below(self, y):
        ss = self.canvas.ss
        rows = (np.arange(self.a.shape[0]) + 0.5) / ss
        return Mask(self.canvas, self.a * (rows >= y)[:, None])

    def where(self, f):
        """The part where f(x, y) is true, x and y being sample-centre arrays in texels."""
        ss = self.canvas.ss
        H, W = self.a.shape
        y, x = np.mgrid[0:H, 0:W]
        keep = f((x + 0.5) / ss, (y + 0.5) / ss)
        return Mask(self.canvas, self.a * np.asarray(keep, np.float64))


# --------------------------------------------------------------------------- #
# Canvas
# --------------------------------------------------------------------------- #

def grey_of(pos):
    """The preview grey of a ramp index: the same value the kit's fill and mask store."""
    return np.rint(np.asarray(pos, np.float64) * 255.0 / STEPS)


class Canvas:
    """One part's picture, w x h texels, drawn at ss x ss samples per texel.

        c = Canvas(80, 120)
        m = c.fill(path, (182, 170, 190, 255), group=1, grey=5)   # painted, tinted skin, ramp index 5
        c.stroke(path, (2, 0), (90, 60, 90, 255))                # a tapered line in a fixed colour
        c.paint(m.shift(0, 3) & m, ...)                          # anything paints a Mask
        c.rgba(); c.fill_image(); c.mask(); c.outline(); c.write("part.png")

    `rgba` is a colour (0..255, alpha optional). `group` is the tint group, 0 for a fixed colour.
    `grey` is the ramp index, 0 the line .. 9 highlight (a tone name works too: style.TONES); for a
    tinted group the preview colour defaults to the index's grey if rgba is None.
    """

    def __init__(self, w, h, ss=SS):
        self.w, self.h, self.ss = int(w), int(h), int(ss)
        H, W = self.h * self.ss, self.w * self.ss
        self.cov = np.zeros((H, W), np.float64)
        self.rgb = np.zeros((H, W, 3), np.float64)
        self.grp = np.zeros((H, W), np.int16)
        self.pos = np.zeros((H, W), np.float64)

    # regions ---------------------------------------------------------------- #

    def shape(self, path, rule="evenodd"):
        """The Mask a path fills, without painting it."""
        if isinstance(path, Mask):
            return path
        return Mask(self, raster_fill(path, self.w, self.h, self.ss, rule).astype(np.float64))

    def line_shape(self, path, widths):
        """The Mask a stroke covers, without painting it."""
        return Mask(self, raster_stroke(path, widths, self.w, self.h, self.ss).astype(np.float64))

    def everything(self):
        return Mask(self, np.ones_like(self.cov))

    def covered(self):
        """Everything painted so far, as a Mask."""
        return Mask(self, self.cov.copy())

    def nothing(self):
        return Mask(self, np.zeros_like(self.cov))

    # painting --------------------------------------------------------------- #

    def paint(self, mask, rgba=None, group=0, grey=None, alpha=1.0, clip=None):
        """Paints a region. Over what's there: colour and coverage composite; a sample takes the
        new group if the paint is at least half opaque there; the ramp index blends within a group."""
        if isinstance(grey, str):
            from style import TONES  # noqa: avoids a circular import at load time
            grey = TONES[grey]
        if grey is None:
            grey = 5.0
        if rgba is None:
            g = float(grey_of(grey))
            rgba = (g, g, g, 255)
        rgba = tuple(rgba) + ((255,) if len(rgba) == 3 else ())
        a = mask.a if isinstance(mask, Mask) else np.asarray(mask, np.float64)
        if clip is not None:
            a = np.minimum(a, clip.a if isinstance(clip, Mask) else clip)
        a = np.clip(a * (rgba[3] / 255.0) * alpha, 0.0, 1.0)
        self._composite(a, np.array(rgba[:3], np.float64), group, float(grey))
        return Mask(self, a)

    def _composite(self, a, col, group, pos):
        """Puts paint of coverage `a` (sample-resolution) over the canvas, inside its bounding box
        only. `col` is a colour or a per-sample colour field, `pos` a ramp index or a field."""
        rows = np.nonzero(a.any(axis=1))[0]
        if len(rows) == 0:
            return
        cols = np.nonzero(a.any(axis=0))[0]
        box = (slice(rows[0], rows[-1] + 1), slice(cols[0], cols[-1] + 1))
        a = a[box]
        cov, rgb, grp, cur = self.cov[box], self.rgb[box], self.grp[box], self.pos[box]
        if np.ndim(col) == 3:
            col = col[box]
        if np.ndim(pos) == 2:
            pos = pos[box]
        touched = a > 0
        out_cov = a + cov * (1.0 - a)
        safe = np.where(out_cov > 0, out_cov, 1.0)
        mixw = np.where(out_cov > 0, a / safe, 0.0)[..., None]
        self.rgb[box] = np.where(touched[..., None], col * mixw + rgb * (1.0 - mixw), rgb)
        same = (grp == group) & (cov > 0)
        take = (a >= 0.5) | (cov <= 0)
        newpos = np.where(same, pos * a + cur * (1.0 - a), np.where(take, pos, cur))
        self.pos[box] = np.where(touched, newpos, cur)
        self.grp[box] = np.where(touched & take, np.int16(group), grp).astype(np.int16)
        self.cov[box] = np.where(touched, out_cov, cov)

    def fill(self, path, rgba=None, group=0, grey=None, clip=None, rule="evenodd", alpha=1.0):
        """Fills a path; returns its Mask (before clipping)."""
        m = self.shape(path, rule)
        self.paint(m, rgba, group, grey, alpha, clip)
        return m

    def stroke(self, path, widths, rgba=None, group=0, grey=None, clip=None, alpha=1.0):
        """Strokes a path with a width that can vary along it; returns its Mask (before clipping)."""
        m = self.line_shape(path, widths)
        self.paint(m, rgba, group, grey, alpha, clip)
        return m

    def tone(self, mask, tones, which, alpha=1.0, clip=None):
        """Paints a region in one tone of a theme.tones() dict: its colour, its group, its index."""
        from style import TONES  # noqa
        idx = TONES[which] if isinstance(which, str) else float(which)
        return self.paint(mask, tones_colour(tones, idx), tones.get("group", 0), idx, alpha, clip)

    def gradient(self, mask, tones, field, alpha=1.0, clip=None):
        """Paints a region with a ramp index that varies per sample (`field`, a sample-resolution
        array of indices 0..9): the soft gradients §1.3 allows (iris, sky band, glows)."""
        a = mask.a if isinstance(mask, Mask) else mask
        if clip is not None:
            a = np.minimum(a, clip.a)
        field = np.broadcast_to(np.asarray(field, np.float64), self.cov.shape)
        steps = np.array(tones["steps"], np.float64)
        lo = np.clip(np.floor(field).astype(np.int64), 0, STEPS)
        hi = np.clip(lo + 1, 0, STEPS)
        f = (np.clip(field, 0, STEPS) - lo)[..., None]
        col = steps[lo] * (1 - f) + steps[hi] * f
        group = tones.get("group", 0)
        a = np.clip(a * alpha, 0.0, 1.0)
        self._composite(a, col, group, np.asarray(field, np.float64))

    def erase(self, mask):
        """Takes coverage away (a hole, a notch)."""
        a = np.clip(mask.a if isinstance(mask, Mask) else mask, 0, 1)
        self.cov = self.cov * (1.0 - a)

    def stamp(self, other, x, y):
        """Draws another canvas onto this one at (x, y) texels (same ss), sample for sample."""
        ss = self.ss
        sx, sy = int(round(x * ss)), int(round(y * ss))
        H, W = self.cov.shape
        h, w = other.cov.shape
        ys0, xs0 = max(0, sy), max(0, sx)
        ys1, xs1 = min(H, sy + h), min(W, sx + w)
        if ys1 <= ys0 or xs1 <= xs0:
            return
        src = (slice(ys0 - sy, ys1 - sy), slice(xs0 - sx, xs1 - sx))
        dst = (slice(ys0, ys1), slice(xs0, xs1))
        a = other.cov[src]
        touched = a > 0
        out_cov = a + self.cov[dst] * (1 - a)
        safe = np.where(out_cov > 0, out_cov, 1.0)
        mixw = np.where(out_cov > 0, a / safe, 0.0)[..., None]
        self.rgb[dst] = np.where(touched[..., None], other.rgb[src] * mixw + self.rgb[dst] * (1 - mixw), self.rgb[dst])
        take = touched & ((a >= 0.5) | (self.cov[dst] <= 0))
        self.grp[dst] = np.where(take, other.grp[src], self.grp[dst])
        self.pos[dst] = np.where(take, other.pos[src], self.pos[dst])
        self.cov[dst] = np.where(touched, out_cov, self.cov[dst])

    # reading out ------------------------------------------------------------ #

    def _resolve(self):
        """Per texel: coverage, colour, majority group and that group's ramp index."""
        ss = self.ss
        A = block_mean(self.cov, ss)
        cw = block_mean(self.rgb * self.cov[..., None], ss)
        safe = np.where(A > 0, A, 1.0)[..., None]
        rgb = np.where(A[..., None] > 0, cw / safe, 0.0)
        groups = [int(g) for g in np.unique(self.grp[self.cov > 0])] or [0]
        best = np.full(A.shape, -1.0)
        grp = np.zeros(A.shape, np.int64)
        pos = np.zeros(A.shape, np.float64)
        for g in groups:
            m = (self.grp == g) * self.cov
            share = block_mean(m, ss)
            pw = block_mean(self.pos * m, ss)
            p = np.where(share > 0, pw / np.where(share > 0, share, 1.0), 0.0)
            win = share > best
            best = np.where(win, share, best)
            grp = np.where(win, g, grp)
            pos = np.where(win, p, pos)
        return A, rgb, grp, pos

    def rgba(self):
        """The preview: every sample's colour, as an H x W x 4 uint8 array (straight alpha)."""
        A, rgb, _, _ = self._resolve()
        return np.concatenate([to_u8(rgb), to_u8(A * 255.0)[..., None]], axis=2)

    def fill_image(self):
        """The kit's fill picture: fixed-colour texels as painted, tinted texels as the grey of
        their ramp index."""
        A, rgb, grp, pos = self._resolve()
        g = grey_of(pos)[..., None]
        col = np.where((grp > 0)[..., None], np.repeat(g, 3, axis=2), rgb)
        return np.concatenate([to_u8(col), to_u8(A * 255.0)[..., None]], axis=2)

    def mask(self):
        """The kit's tint mask: R the group, G the ramp index (round(index * 255 / 9)), B 0,
        A the coverage."""
        A, _, grp, pos = self._resolve()
        out = np.zeros(A.shape + (4,), np.uint8)
        out[..., 0] = np.clip(grp, 0, 255).astype(np.uint8)
        out[..., 1] = to_u8(grey_of(pos))
        out[..., 3] = to_u8(A * 255.0)
        return out

    def outline(self, up=0.0, side=2.0, down=3.0, pad=None):
        """The `#line` silhouette: everything painted, grown by `up` texels at the top, `side` at
        the sides and `down` underneath (an egg-shaped reach: style.py's line widths beyond a
        part's own edge line). White, alpha the coverage. Returns (H x W x 4 uint8, pad), the
        picture being `pad` texels bigger than the canvas on every side."""
        return outline_of(self.cov >= 0.5, self.ss, up, side, down, pad)

    def write(self, path, what="rgba"):
        """Writes the preview (or "fill", "mask", "line") as a PNG."""
        img = {"rgba": self.rgba, "fill": self.fill_image, "mask": self.mask}.get(what)
        write_png(path, img() if img else self.outline()[0])


def outline_of(inside, ss, up=0.0, side=2.0, down=3.0, pad=None):
    """Canvas.outline for any sample-resolution bool array (ss samples per texel)."""
    pad = int(math.ceil(max(up, side, down))) + 1 if pad is None else pad
    P = pad * ss
    grid = np.zeros((inside.shape[0] + 2 * P, inside.shape[1] + 2 * P), bool)
    grid[P:-P, P:-P] = inside
    ry = (up + down) / 2.0
    cy = (down - up) / 2.0
    rx = max(side, 1e-3)
    ryv = max(ry, 1e-3)
    big = max(rx, ryv)
    shifted = np.zeros_like(grid)
    k = int(round(cy * ss))
    if k > 0:
        shifted[k:] = grid[:-k]
    elif k < 0:
        shifted[:k] = grid[-k:]
    else:
        shifted = grid
    d = distance(shifted, big * ss + 1, rx=rx / big, ry=ryv / big)
    grown = (d <= big * ss) | grid
    A = block_mean(grown.astype(np.float64), ss)
    out = np.zeros(A.shape + (4,), np.uint8)
    out[..., :3] = 255
    out[..., 3] = to_u8(A * 255.0)
    return out, pad


def tones_colour(tones, idx):
    """The colour at a (possibly fractional) ramp index of a theme.tones() dict."""
    steps = tones["steps"]
    lo = int(max(0, min(STEPS, math.floor(idx))))
    hi = min(STEPS, lo + 1)
    f = max(0.0, min(1.0, idx - lo))
    a, b = steps[lo], steps[hi]
    return tuple(a[i] + (b[i] - a[i]) * f for i in range(3)) + (255,)


# --------------------------------------------------------------------------- #
# Whole pictures: compositing and resampling (render.py and preview.py use these)
# --------------------------------------------------------------------------- #

def premultiply(rgba_u8):
    """uint8 straight RGBA -> float64 premultiplied, 0..1."""
    f = rgba_u8.astype(np.float64) / 255.0
    f[..., :3] *= f[..., 3:4]
    return f


def unpremultiply(p):
    """float64 premultiplied 0..1 -> uint8 straight RGBA."""
    a = p[..., 3:4]
    rgb = np.where(a > 0, p[..., :3] / np.where(a > 0, a, 1.0), 0.0)
    return np.concatenate([to_u8(rgb * 255.0), to_u8(a * 255.0)], axis=2)


def over(dst, src, x=0, y=0):
    """Composites premultiplied float src onto premultiplied float dst at (x, y), in place."""
    H, W = dst.shape[:2]
    h, w = src.shape[:2]
    x0, y0 = max(0, x), max(0, y)
    x1, y1 = min(W, x + w), min(H, y + h)
    if x1 <= x0 or y1 <= y0:
        return dst
    s = src[y0 - y:y1 - y, x0 - x:x1 - x]
    d = dst[y0:y1, x0:x1]
    dst[y0:y1, x0:x1] = s + d * (1.0 - s[..., 3:4])
    return dst


def sample_affine(src, inv, out_w, out_h, ox=0, oy=0):
    """Draws a premultiplied float picture through an affine map, bilinear with clamp-to-clear
    (LinearClamp over a part with a clear border, as the game does). `inv` maps an output pixel
    centre (x + 0.5, y + 0.5), offset by (ox, oy), back to source texel coordinates:
    (a, b, c, d, e, f) with u = a x + b y + c, v = d x + e y + f. Returns out_h x out_w x 4."""
    a, b, c, d, e, f = inv
    yy, xx = np.mgrid[0:out_h, 0:out_w]
    X = xx + ox + 0.5
    Y = yy + oy + 0.5
    u = a * X + b * Y + c - 0.5
    v = d * X + e * Y + f - 0.5
    h, w = src.shape[:2]
    pad = np.zeros((h + 2, w + 2, src.shape[2]), np.float64)
    pad[1:-1, 1:-1] = src
    u = u + 1.0
    v = v + 1.0
    u0 = np.floor(u).astype(np.int64)
    v0 = np.floor(v).astype(np.int64)
    fu = (u - u0)[..., None]
    fv = (v - v0)[..., None]
    valid = (u0 >= 0) & (v0 >= 0) & (u0 + 1 < w + 2) & (v0 + 1 < h + 2)
    u0c = np.clip(u0, 0, w)
    v0c = np.clip(v0, 0, h)
    p00 = pad[v0c, u0c]
    p01 = pad[v0c, u0c + 1]
    p10 = pad[v0c + 1, u0c]
    p11 = pad[v0c + 1, u0c + 1]
    out = (p00 * (1 - fu) + p01 * fu) * (1 - fv) + (p10 * (1 - fu) + p11 * fu) * fv
    return np.where(valid[..., None], out, 0.0)


def scale_nearest(img, k):
    """An integer nearest-neighbour upscale (the rooms' art pixels to canvas pixels)."""
    return np.repeat(np.repeat(img, k, axis=0), k, axis=1)
