"""
The owner's paintings, and what the tools that read them share: make_room_template.py,
cut_room.py, cut_tiles.py and check_room_sources.py (12-rooms 3.5). This isn't run on its own.

The contract (Story/14-background-prompts.md writes the prompts to the same one)
================================================================================

Panels. ChatGPT makes 1536 x 1024 landscape pictures. A room W canvas pixels wide (1600 to 4800,
900 tall) is painted as N panels: one for W <= 1600, else ceil((W - 1600) / 1400) + 1. Each panel
covers 1600 canvas pixels, consecutive panels overlap by 200, and the last one is right-aligned to
the room's end (so its overlap with the one before it can be wider). Panels count from 1, left to
right. In each picture only the middle 1536 x 864 band (16:9) is used, at 0.96 picture pixels to a
canvas pixel; the top and bottom 80 rows are bleed, sky above and ground below, and are cropped.

    panel_count(W), panel_lefts(W)    how many panels, and where each starts on the canvas
    to_panel(x, y, left)              where a canvas point is on a panel's picture

Sources. tools/source/rooms/<zone>/<room>/bg-<i>.png (every panel): the whole scene, sky to floor.
fg-<i>.png (optional): a foreground strip on a transparent background or a flat #FF00FF. An
interior's are named the same. tools/source/tiles/<tileset>/<tile>.png: a seamless 1024 x 1024
floor texture, named after a tile in World/TilesetCatalog.cs. The templates and the tile lists
for the prompts go in shots/art/room-templates/.

Also here: reading any non-interlaced PNG (and saying plainly when a file isn't one), writing them
with a filter picked per row, a Lanczos-3 resize, the key that takes a picture off its backdrop,
the tilesets read off World/TilesetCatalog.cs (TileTexels and all), a room's tile strip drawn the
way Tilemap draws it at time zero, and keeping ASSETS.md and Content.mgcb in step with what the
cutters write, idempotently.

Deterministic: float64, elementwise, sums in a fixed order, np.rint before a cast to uint8, and
PNGs at zlib level 9. Only needs CPython and numpy.
"""

import os
import re
import struct
import sys
import zlib

import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
import roomkit  # noqa: E402
from roomkit import CANVAS_H, CANVAS_W, CONTENT, ROOT  # noqa: E402

# --------------------------------------------------------------------------- #
# The panels
# --------------------------------------------------------------------------- #

PANEL_W, PANEL_H = 1536, 1024
"""A picture as ChatGPT makes it."""
BLEED = 80
"""Rows cropped off the top and the bottom of a picture."""
BAND_H = PANEL_H - 2 * BLEED
"""The 16:9 band that's used: 1536 x 864."""
SPAN = CANVAS_W
"""Canvas pixels a panel covers."""
OVERLAP = 200
"""Canvas pixels consecutive panels share (more for the last, which is right-aligned)."""
STRIDE = SPAN - OVERLAP
SCALE = PANEL_W / SPAN
"""Picture pixels to a canvas pixel: 0.96, the same both ways."""

SOURCES = os.path.join(ROOT, "tools", "source", "rooms")
TILE_SOURCES = os.path.join(ROOT, "tools", "source", "tiles")
TEMPLATES = os.path.join(ROOT, "shots", "art", "room-templates")
ROOMS = os.path.join(CONTENT, "Rooms")
TILE_PICTURE = 1024
"""A floor texture's side."""

KEY = (255, 0, 255)
"""The flat backdrop a foreground strip or a detail tile can be painted on."""

HORIZON_ABOVE_FLOOR = 150
"""Outdoors, the horizon the templates mark: this far above the floor band's back edge, about the
eye height of someone standing at the back of it, which puts it across the middle of a picture."""

FLOOR_FADE = 32
"""How far below the floor band's back edge a painting fades into the tile strip under it."""


class SourceError(Exception):
    """A source is missing or wrong. The message names the file and says what to do."""


def panel_count(width):
    """How many panels a room `width` canvas pixels wide is painted in."""
    if width <= SPAN:
        return 1
    return -(-(int(width) - SPAN) // STRIDE) + 1


def panel_lefts(width):
    """Where each panel starts on the canvas, left to right: every STRIDE, and the last one against
    the room's right end."""
    n = panel_count(width)
    return [i * STRIDE for i in range(n - 1)] + [max(0, int(width) - SPAN)]


def neighbours(width, i):
    """The canvas stretches panel i (from 0) shares with the panel before it and the one after it,
    each (x0, x1, the other panel's index), or None at an end."""
    lefts = panel_lefts(width)
    before = (lefts[i], lefts[i - 1] + SPAN, i - 1) if i > 0 else None
    after = (lefts[i + 1], lefts[i] + SPAN, i + 1) if i + 1 < len(lefts) else None
    return before, after


def fade_window(left, covered):
    """Where a panel starting at canvas `left` cross-fades into what's laid down to `covered`: the
    middle OVERLAP of their overlap, (start, width) in the panel's own canvas columns; None if they
    don't overlap."""
    ov = covered - left
    if ov <= 0:
        return None
    fade = min(ov, OVERLAP)
    return (ov - fade) / 2.0, float(fade)


def to_panel(x, y, left):
    """A canvas point on the picture of the panel that starts at canvas `left`."""
    return (x - left) * SCALE, BLEED + y * SCALE


def horizon(room):
    """Outdoors, where the templates put the horizon, in canvas pixels; None indoors."""
    if room.kind == "interior":
        return None
    return roomkit.band(room)[0] - HORIZON_ABOVE_FLOOR


def floor_line(room):
    """Each canvas column's line below which a painting gives way to the tile strip: the floor's
    back edge, and never above the strip's top (there'd be nothing under it)."""
    xs = np.arange(room.width) + 0.5
    return np.array([max(room.back_edge_at(x), room.tiles.top) for x in xs])


# --------------------------------------------------------------------------- #
# Where the sources are
# --------------------------------------------------------------------------- #

PANEL_NAME = re.compile(r"^(bg|fg)-([1-9][0-9]*)\.png$")
IGNORED = ("thumbs.db", "desktop.ini")


def shown(path):
    """A path as the messages show it: from the repo root, or whole if it's elsewhere."""
    try:
        r = os.path.relpath(path, ROOT)
    except ValueError:
        return path.replace(os.sep, "/")
    return (path if r.startswith("..") else r).replace(os.sep, "/")


def room_folder(zone_id, room_id, root=None):
    return os.path.join(root or SOURCES, zone_id, room_id)


def listed(folder):
    """The files in a folder that count: not hidden, not Windows' own."""
    if not os.path.isdir(folder):
        return []
    return [n for n in sorted(os.listdir(folder))
            if not n.startswith(".") and n.lower() not in IGNORED and os.path.isfile(os.path.join(folder, n))]


def listed_dirs(folder):
    """The folders in a folder, not hidden."""
    if not os.path.isdir(folder):
        return []
    return [n for n in sorted(os.listdir(folder)) if not n.startswith(".") and os.path.isdir(os.path.join(folder, n))]


def room_sources(folder, panels):
    """What's in a room's source folder: ({"bg": {i: path}, "fg": {i: path}}, [names that aren't a
    panel of this room])."""
    found = {"bg": {}, "fg": {}}
    stray = []
    for name in listed(folder):
        m = PANEL_NAME.match(name)
        if m and int(m.group(2)) <= panels:
            found[m.group(1)][int(m.group(2))] = os.path.join(folder, name)
        else:
            stray.append(name)
    return found, stray


def has_sources(zone_id, room_id, root=None):
    """Whether the owner has started painting a room: anything in its source folder."""
    return bool(listed(room_folder(zone_id, room_id, root)))


def panel_names(panels):
    return "bg-1.png" if panels == 1 else "bg-1.png to bg-%d.png" % panels


# --------------------------------------------------------------------------- #
# PNG files
# --------------------------------------------------------------------------- #

PNG_SIGNATURE = b"\x89PNG\r\n\x1a\n"


def _what_is(data):
    if data[:3] == b"\xff\xd8\xff":
        return "a JPEG"
    if data[:4] == b"RIFF" and data[8:12] == b"WEBP":
        return "a WebP picture"
    if data[:6] in (b"GIF87a", b"GIF89a"):
        return "a GIF"
    return "not a PNG"


def png_size(path):
    """A PNG's width and height off its header, without reading the rest."""
    with open(path, "rb") as f:
        data = f.read(33)
    if data[:8] != PNG_SIGNATURE or data[12:16] != b"IHDR":
        raise SourceError("%s is %s: save it as a PNG." % (shown(path), _what_is(data)))
    return struct.unpack(">II", data[16:24])


def read_png(path):
    """Any non-interlaced PNG (grey, RGB, palette, with or without alpha, 1 to 16 bits) as an
    H x W x 4 uint8 RGBA array. Raises SourceError, naming the file, for anything else."""
    with open(path, "rb") as f:
        data = f.read()
    if data[:8] != PNG_SIGNATURE:
        raise SourceError("%s is %s: save it as a PNG." % (shown(path), _what_is(data)))
    ihdr, plte, trns, idat = None, None, None, []
    pos = 8
    while pos + 8 <= len(data):
        n, = struct.unpack(">I", data[pos:pos + 4])
        tag, body = data[pos + 4:pos + 8], data[pos + 8:pos + 8 + n]
        pos += 12 + n
        if tag == b"IHDR":
            ihdr = struct.unpack(">IIBBBBB", body)
        elif tag == b"PLTE":
            plte = np.frombuffer(body, np.uint8).reshape(-1, 3)
        elif tag == b"tRNS":
            trns = body
        elif tag == b"IDAT":
            idat.append(body)
        elif tag == b"IEND":
            break
    if ihdr is None:
        raise SourceError("%s has no header: it isn't a whole PNG." % shown(path))
    w, h, depth, ctype, _, _, interlace = ihdr
    depths = {0: (1, 2, 4, 8, 16), 2: (8, 16), 3: (1, 2, 4, 8), 4: (8, 16), 6: (8, 16)}
    if ctype not in depths or depth not in depths[ctype]:
        raise SourceError("%s is a kind of PNG this can't read (colour type %d, %d bits)." % (shown(path), ctype, depth))
    if interlace:
        raise SourceError("%s is interlaced: save it again without interlacing." % shown(path))
    channels = {0: 1, 2: 3, 3: 1, 4: 2, 6: 4}[ctype]
    bits = depth * channels
    stride = (w * bits + 7) // 8
    try:
        raw = zlib.decompress(b"".join(idat))
    except zlib.error:
        raise SourceError("%s is damaged (its picture data won't unpack)." % shown(path))
    if len(raw) < h * (stride + 1):
        raise SourceError("%s is damaged (it's cut short)." % shown(path))
    raw = np.frombuffer(raw, np.uint8)[:h * (stride + 1)].reshape(h, stride + 1)
    if raw[:, 0].max(initial=0) > 4:
        raise SourceError("%s is damaged (a row has no filter PNG knows)." % shown(path))
    rows = _unfilter(raw[:, 0], raw[:, 1:], max(1, bits // 8))
    if depth == 16:
        s = rows.reshape(h, w, channels, 2)[..., 0]
    elif depth == 8:
        s = rows.reshape(h, w, channels)
    else:
        b = np.unpackbits(rows, axis=1)[:, :w * depth].reshape(h, w, depth).astype(np.int64)
        s = (b * (1 << np.arange(depth - 1, -1, -1))).sum(axis=2).astype(np.uint8)[..., None]
    out = np.zeros((h, w, 4), np.uint8)
    out[..., 3] = 255
    if ctype == 3:
        if plte is None or s.max(initial=0) >= len(plte):
            raise SourceError("%s is damaged (its palette's missing or too short)." % shown(path))
        out[..., :3] = plte[s[..., 0]]
        alpha = np.full(len(plte), 255, np.uint8)
        if trns:
            t = np.frombuffer(trns, np.uint8)[:len(plte)]
            alpha[:len(t)] = t
        out[..., 3] = alpha[s[..., 0]]
    elif ctype in (0, 4):
        g = s[..., 0]
        if depth < 8:
            g = (g.astype(np.int64) * 255 // ((1 << depth) - 1)).astype(np.uint8)
        out[..., 0] = out[..., 1] = out[..., 2] = g
        if ctype == 4:
            out[..., 3] = s[..., 1]
        elif trns and depth <= 8:
            out[..., 3] = np.where(s[..., 0] == struct.unpack(">H", trns[:2])[0], 0, 255)
    else:
        out[..., :3] = s[..., :3]
        if ctype == 6:
            out[..., 3] = s[..., 3]
        elif trns and depth == 8:
            key = np.array(struct.unpack(">HHH", trns[:6]))
            out[..., 3] = np.where(np.all(s[..., :3] == key, axis=2), 0, 255)
    return out


def _unfilter(kinds, filtered, bpp):
    """Undoes PNG's row filters on the raw bytes, a pixel's bpp bytes at a time. Rows of None, Sub and
    Up go a row at a time; Average and Paeth (which lean on the pixel to the left as well as the row
    above) a diagonal at a time, which numpy does at speed."""
    h, stride = filtered.shape
    units = stride // bpp
    f = filtered.reshape(h, units, bpp).astype(np.int64)
    out = np.zeros((h + 1, units + 1, bpp), np.int64)   # a zero row above and column to the left
    kinds = kinds.astype(np.int64)
    if np.all(kinds <= 2):
        for y in range(h):
            k = kinds[y]
            if k == 0:
                out[y + 1, 1:] = f[y]
            elif k == 1:
                out[y + 1, 1:] = np.cumsum(f[y], axis=0) & 255
            else:
                out[y + 1, 1:] = (f[y] + out[y, 1:]) & 255
    else:
        for k in range(units + h - 1):
            ys = np.arange(max(0, k - units + 1), min(h, k + 1))
            xs = k - ys
            r = f[ys, xs]
            a, b, c = out[ys + 1, xs], out[ys, xs + 1], out[ys, xs]
            ft = kinds[ys][:, None]
            p = a + b - c
            pa, pb, pc = np.abs(p - a), np.abs(p - b), np.abs(p - c)
            paeth = np.where((pa <= pb) & (pa <= pc), a, np.where(pb <= pc, b, c))
            pred = np.where(ft == 1, a, np.where(ft == 2, b, np.where(ft == 3, (a + b) >> 1, np.where(ft == 4, paeth, 0))))
            out[ys + 1, xs + 1] = (r + pred) & 255
    return out[1:, 1:].reshape(h, units * bpp).astype(np.uint8)


def write_png(path, rgba, quiet=False):
    """Writes an H x W x 4 uint8 array as an 8-bit RGBA PNG, each row with whichever of PNG's five
    filters leaves the smallest bytes (the usual heuristic, ties to the lowest), zlib level 9."""
    a = np.ascontiguousarray(rgba, dtype=np.uint8)
    h, w = a.shape[:2]
    if a.ndim != 3 or a.shape[2] != 4:
        raise ValueError("write_png wants H x W x 4, not " + str(a.shape))
    x = a.reshape(h, w * 4).astype(np.int16)
    left = np.zeros_like(x)
    left[:, 4:] = x[:, :-4]
    up = np.zeros_like(x)
    up[1:] = x[:-1]
    ul = np.zeros_like(x)
    ul[1:, 4:] = x[:-1, :-4]
    p = left + up - ul
    pa, pb, pc = np.abs(p - left), np.abs(p - up), np.abs(p - ul)
    paeth = np.where((pa <= pb) & (pa <= pc), left, np.where(pb <= pc, up, ul))
    best, best_score = None, None
    for kind, pred in enumerate((0, left, up, (left + up) >> 1, paeth)):
        cand = ((x - pred) & 255).astype(np.uint8)
        signed = cand.astype(np.int16)
        score = np.minimum(signed, 256 - signed).sum(axis=1)
        if best is None:
            best, best_score, choice = cand, score, np.zeros(h, np.uint8)
        else:
            better = score < best_score
            best = np.where(better[:, None], cand, best)
            best_score = np.where(better, score, best_score)
            choice = np.where(better, kind, choice).astype(np.uint8)
    raw = np.zeros((h, w * 4 + 1), np.uint8)
    raw[:, 0] = choice
    raw[:, 1:] = best

    def chunk(tag, body):
        return struct.pack(">I", len(body)) + tag + body + struct.pack(">I", zlib.crc32(tag + body) & 0xFFFFFFFF)

    blob = (PNG_SIGNATURE + chunk(b"IHDR", struct.pack(">IIBBBBB", w, h, 8, 6, 0, 0, 0))
            + chunk(b"IDAT", zlib.compress(raw.tobytes(), 9)) + chunk(b"IEND", b""))
    folder = os.path.dirname(path)
    if folder:
        os.makedirs(folder, exist_ok=True)
    with open(path, "wb") as f:
        f.write(blob)
    if not quiet:
        print("  %s  (%dx%d)" % (shown(path), w, h))


# --------------------------------------------------------------------------- #
# Colour, resizing and keying
# --------------------------------------------------------------------------- #

def premultiply(rgba):
    """uint8 (or 0..255 float) straight RGBA to float64 premultiplied, 0..255."""
    p = np.asarray(rgba, np.float64).copy()
    p[..., :3] *= p[..., 3:4] / 255.0
    return p


CLEAR_BELOW = 4.0
"""An alpha (of 255) under which a pixel is clear: what a resize's ringing leaves round a hard edge,
whose colour means nothing."""


def unpremultiply(p):
    """float64 premultiplied, 0..255, to uint8 straight RGBA. A colour can't be more than its alpha,
    and a pixel all but clear is clear."""
    a = np.clip(p[..., 3:4], 0.0, 255.0)
    a = np.where(a < CLEAR_BELOW, 0.0, a)
    rgb = np.where(a > 0, np.clip(p[..., :3], 0.0, a) * 255.0 / np.maximum(a, 1e-9), 0.0)
    return np.rint(np.clip(np.concatenate([rgb, a], axis=2), 0.0, 255.0)).astype(np.uint8)


def over(dst, src, x=0, y=0):
    """Lays premultiplied src over premultiplied dst in place, src's top left at (x, y), clipped."""
    h, w = src.shape[:2]
    x0, y0 = max(0, x), max(0, y)
    x1, y1 = min(dst.shape[1], x + w), min(dst.shape[0], y + h)
    if x1 <= x0 or y1 <= y0:
        return
    s = src[y0 - y:y1 - y, x0 - x:x1 - x]
    d = dst[y0:y1, x0:x1]
    d[...] = s + d * (1.0 - s[..., 3:4] / 255.0)


def _lanczos3(x):
    x = np.abs(x)
    out = np.zeros_like(x)
    inside = x < 3.0
    xi = x[inside]
    with np.errstate(divide="ignore", invalid="ignore"):
        v = 3.0 * np.sin(np.pi * xi) * np.sin(np.pi * xi / 3.0) / (np.pi * np.pi * xi * xi)
    out[inside] = np.where(xi < 1e-9, 1.0, v)
    return out


def _taps(n_out, start, length, n_total, wrap):
    """The source pixels and weights for each of n_out pixels resampled from source pixels
    start..start+length: Lanczos-3, widened when shrinking so it averages, reaching past the stretch
    into the pixels round it (clamped at the picture's ends, or wrapping round for a seamless one)."""
    scale = n_out / float(length)
    stretch = max(1.0, 1.0 / scale)
    support = 3.0 * stretch
    centre = start + (np.arange(n_out) + 0.5) / scale - 0.5
    first = np.ceil(centre - support).astype(np.int64)
    count = int(np.ceil(2 * support)) + 1
    idx = first[:, None] + np.arange(count)[None, :]
    weight = _lanczos3((idx - centre[:, None]) / stretch)
    weight /= weight.sum(axis=1, keepdims=True)
    idx = np.mod(idx, n_total) if wrap else np.clip(idx, 0, n_total - 1)
    return idx, weight


def resize(img, w, h, box=None, wrap=False):
    """Resamples a float64 picture (premultiplied, H x W x C) to w x h: the whole of it, or the part
    in box = (x, y, width, height) reading on into what's round the box. Separable Lanczos-3."""
    bx, by, bw, bh = box or (0, 0, img.shape[1], img.shape[0])
    ix, wx = _taps(w, bx, bw, img.shape[1], wrap)
    iy, wy = _taps(h, by, bh, img.shape[0], wrap)
    rows = np.unique(np.clip(iy, 0, img.shape[0] - 1))
    src = img[rows]
    where = np.searchsorted(rows, iy)
    tmp = np.zeros((src.shape[0], w, img.shape[2]))
    for k in range(ix.shape[1]):
        tmp += wx[:, k][None, :, None] * src[:, ix[:, k], :]
    out = np.zeros((h, w, img.shape[2]))
    for k in range(iy.shape[1]):
        out += wy[:, k][:, None, None] * tmp[where[:, k]]
    return out


SOLID_SPILL, KEY_SPILL = 128.0, 216.0
"""How magenta a pixel has to be (the least of its red and blue over its green) to start being keyed
out, and to be gone: the backdrop's own noise is well over KEY_SPILL, a painted pink well under."""
KEY_SHARE = 0.02
"""A picture's keyed by colour only if this much of it is the backdrop: one drawn on a transparent
background, with some pink in it, keeps its pink."""


KEY_EDGE = 2
"""How far from the backdrop, in pixels, a pixel can be a blend of it: there every bit of magenta
counts, so the picture's anti-aliased edge comes off clean."""


def _near(mask, r):
    """Every pixel within r (a square) of one in the mask."""
    h, w = mask.shape
    pad = np.zeros((h + 2 * r, w + 2 * r), bool)
    pad[r:r + h, r:r + w] = mask
    out = np.zeros_like(mask)
    for dy in range(2 * r + 1):
        for dx in range(2 * r + 1):
            out |= pad[dy:dy + h, dx:dx + w]
    return out


def key_out(rgba):
    """Takes a picture off its backdrop: its own transparency, and, if it was painted on the flat
    #FF00FF, the backdrop. Next to the backdrop a pixel's share of magenta says how much of it is the
    backdrop, and that's taken back out of its colour; further in, only what's nearly the backdrop's
    own colour goes, so a pink in the picture keeps. Returns float64 premultiplied RGBA, 0..255."""
    f = np.asarray(rgba, np.float64)
    rgb = f[..., :3]
    alpha = f[..., 3] / 255.0
    spill = np.minimum(rgb[..., 0], rgb[..., 2]) - rgb[..., 1]
    backdrop = spill >= KEY_SPILL
    if backdrop.mean() >= KEY_SHARE:
        keep = np.clip((KEY_SPILL - spill) / (KEY_SPILL - SOLID_SPILL), 0.0, 1.0)
        edge_keep = np.clip((KEY_SPILL - spill) / KEY_SPILL, 0.0, 1.0)
        keep = np.where(_near(backdrop, KEY_EDGE), np.minimum(keep, edge_keep), keep)
        t = keep[..., None]
        edge = (keep > 0) & (keep < 1)
        unmixed = np.clip((rgb - (1.0 - t) * np.array(KEY, np.float64)) / np.maximum(t, 1e-6), 0.0, 255.0)
        excess = np.maximum(np.minimum(unmixed[..., 0], unmixed[..., 2]) - unmixed[..., 1], 0.0)[..., None]
        unmixed = unmixed - excess * np.array([1.0, 0.0, 1.0])
        rgb = np.where(edge[..., None], unmixed, rgb)
        alpha = np.minimum(alpha, keep)
    out = np.concatenate([rgb * alpha[..., None], (alpha * 255.0)[..., None]], axis=2)
    return out


def smoothstep(t):
    t = np.clip(t, 0.0, 1.0)
    return t * t * (3.0 - 2.0 * t)


# --------------------------------------------------------------------------- #
# The tilesets, off World/TilesetCatalog.cs
# --------------------------------------------------------------------------- #

COLUMNS = roomkit.COLUMNS
"""Tiles across a sheet."""
TILE_CANVAS = 64
"""A tile's side in canvas pixels: 16 world pixels at RoomScale 4."""


class Tileset:
    """A tileset as TilesetCatalog has it: its id, sheet, TileTexels, what its summary says, and its
    tiles in sheet order, each a dict of name, variants, frames, block, edged, solid and section (the
    // comment over it in the catalog: ground, detail or walls, when it has one)."""

    def __init__(self, ident, texture, texels, summary, tiles):
        self.id, self.texture, self.texels, self.summary, self.tiles = ident, texture, texels, summary, tiles
        self.ids = {t["name"]: i + 1 for i, t in enumerate(tiles)}
        self.firsts = []
        frame = 0
        for t in tiles:
            self.firsts.append(frame)
            frame += self.frames_in(t)
        self.frame_count = frame

    @staticmethod
    def frames_in(t):
        return t["variants"] * t["frames"] * t["block"] * t["block"] + (4 if t["edged"] else 0)

    def tile(self, name):
        return self.tiles[self.ids[name] - 1]

    def cell(self, name, variant, frame, cx, cy):
        """The sheet frame of one cell of one frame of one variant of a tile, as Tilemap.FrameOf counts."""
        t = self.tile(name)
        b = t["block"]
        return self.firsts[self.ids[name] - 1] + (variant * t["frames"] + frame) * b * b + cy * b + cx

    def frame_of(self, tid, x, y):
        """Tilemap.FrameOf at time zero, for the cell at column x, row y."""
        t = self.tiles[tid - 1]
        b = t["block"]
        pick = (((x // b * 73856093) ^ (y // b * 19349663)) & 0xFFFFFFFF) >> 7
        variant = pick % t["variants"]
        step = 0 if t["frames"] == 1 else (pick // t["variants"]) % t["frames"]
        return self.firsts[tid - 1] + (variant * t["frames"] + step) * b * b + y % b * b + x % b

    def edge_frame(self, tid, side):
        t = self.tiles[tid - 1]
        return self.firsts[tid - 1] + t["variants"] * t["frames"] * t["block"] * t["block"] + side

    @property
    def sheet_path(self):
        return os.path.join(CONTENT, *self.texture.split("/")) + ".png"


_catalog = None


def tilesets():
    """Every tileset in World/TilesetCatalog.cs, by id, in the catalog's order."""
    global _catalog
    if _catalog is not None:
        return _catalog
    with open(os.path.join(ROOT, "World", "TilesetCatalog.cs"), encoding="utf-8") as f:
        text = f.read()
    consts = {n: int(v) for n, v in re.findall(r"const int (\w+) = (\d+);", text)}
    lines = text.split("\n")
    out = {}
    for i, line in enumerate(lines):
        m = re.search(r"private static readonly TilesetDefinition (\w+) = new\(\)", line)
        if not m:
            continue
        doc, j = [], i - 1
        while j >= 0 and lines[j].strip().startswith("///"):
            doc.insert(0, lines[j].strip()[3:].strip())
            j -= 1
        summary = re.search(r"<summary>(.*?)</summary>", " ".join(doc))
        body, j = [], i + 1
        while j < len(lines) and lines[j].rstrip() != "    };":
            body.append(lines[j])
            j += 1
        body = "\n".join(body)
        ident = re.search(r'Id = "([\w-]+)"', body).group(1)
        texture = re.search(r'Texture = "([\w/-]+)"', body).group(1)
        tm = re.search(r"TileTexels = (\w+)", body)
        texels = 16 if tm is None else int(tm.group(1)) if tm.group(1).isdigit() else consts[tm.group(1)]
        tiles, section, after_comment = [], None, False
        for t in re.finditer(r'(//[^\n]*)|new\(\)\s*\{\s*Name = "([\w-]+)"([^}]*)\}', body):
            if t.group(1):
                word = t.group(1)[2:].strip().lower()
                if not after_comment:   # a comment's first line names the section; the rest go on with it
                    section = ("detail" if word.startswith("detail") else
                               "walls" if word.startswith(("walls", "the rock")) else "ground")
                after_comment = True
                continue
            after_comment = False
            rest = t.group(3)

            def num(key, default):
                n = re.search(key + r" = (\d+)", rest)
                return int(n.group(1)) if n else default
            tiles.append({"name": t.group(2), "variants": num("Variants", 1), "frames": num("Frames", 1),
                          "block": num("Block", 1), "edged": "Edged = true" in rest, "solid": "Solid = true" in rest,
                          "section": section})
        out[ident] = Tileset(ident, texture, texels, summary.group(1).strip() if summary else "", tiles)
    _catalog = out
    return out


def tile_usage(zones=None):
    """Which rooms use each tile, and how: {tileset: {tile: {"ground": [rooms], "detail": [rooms]}}}."""
    used = {}
    for path in roomkit.zone_files(zones):
        zone = roomkit.load_zone(path)
        for room in zone.rooms:
            t = room.tiles
            key = "%s/%s" % (zone.id, room.id)
            for kind, rows in (("ground", t.rows), ("detail", t.detail)):
                for ch in sorted({c for row in rows for c in row if c != "."}):
                    name = t.legend.get(ch)
                    if name is None:
                        continue
                    where = used.setdefault(t.tileset, {}).setdefault(name, {"ground": [], "detail": []})[kind]
                    if key not in where:
                        where.append(key)
    return used


def tile_kind(tileset, t, usage):
    """What a tile is for a painter: "detail" (laid over the ground, so on a transparent backdrop),
    "walls" or "ground"."""
    if t["section"]:
        return t["section"]
    u = usage.get(tileset.id, {}).get(t["name"])
    if u and u["detail"] and not u["ground"]:
        return "detail"
    return "walls" if t["name"].startswith("wall") else "ground"


_sheets = {}


def sheet(tileset):
    """A tileset's sheet as it is in Content/Tiles, read once."""
    if tileset.id not in _sheets:
        _sheets[tileset.id] = read_png(tileset.sheet_path)
    return _sheets[tileset.id]


def tile_strip(room):
    """A room's tile strip as Tilemap draws it at time zero (ground, edges, then detail), premultiplied
    float64 at canvas resolution: its rows x 64 tall, the room's width across."""
    t = room.tiles
    ts = tilesets()[t.tileset]
    img = sheet(ts)
    k = ts.texels
    cols = img.shape[1] // k
    cache = {}

    def frame(i):
        if i not in cache:
            r, c = divmod(i, cols)
            tile = img[r * k:(r + 1) * k, c * k:(c + 1) * k]
            if k != TILE_CANVAS:
                tile = np.repeat(np.repeat(tile, TILE_CANVAS // k, axis=0), TILE_CANVAS // k, axis=1)
            cache[i] = premultiply(tile)
        return cache[i]

    width = room.width // TILE_CANVAS
    rows = len(t.rows)
    out = np.zeros((rows * TILE_CANVAS, width * TILE_CANVAS, 4))
    ground = [[ts.ids.get(t.legend.get(c, ""), 0) for c in row] for row in t.rows]
    for y in range(rows):
        for x in range(width):
            if ground[y][x]:
                over(out, frame(ts.frame_of(ground[y][x], x, y)), x * TILE_CANVAS, y * TILE_CANVAS)
    for y in range(rows):
        for x in range(width):
            tid = ground[y][x]
            if not tid or not ts.tiles[tid - 1]["edged"]:
                continue
            for side, (dx, dy) in enumerate(((0, -1), (1, 0), (0, 1), (-1, 0))):
                nx, ny = x + dx, y + dy
                if 0 <= nx < width and 0 <= ny < rows and ground[ny][nx] and ground[ny][nx] != tid:
                    over(out, frame(ts.edge_frame(tid, side)), x * TILE_CANVAS, y * TILE_CANVAS)
    for y, row in enumerate(t.detail):
        for x, c in enumerate(row):
            tid = ts.ids.get(t.legend.get(c, ""), 0) if c != "." else 0
            if tid:
                over(out, frame(ts.frame_of(tid, x, y)), x * TILE_CANVAS, y * TILE_CANVAS)
    return out


# --------------------------------------------------------------------------- #
# Zones
# --------------------------------------------------------------------------- #

def rooms(zones=None):
    """Every (zone, room) in the zone files, in file order."""
    for path in roomkit.zone_files(zones):
        zone = roomkit.load_zone(path)
        for room in zone.rooms:
            yield zone, room


def wanted(zone, room, picks):
    return not picks or zone.id in picks or "%s/%s" % (zone.id, room.id) in picks


def check_picks(picks, zones=None):
    """Fails on a pick that's no zone and no zone/room."""
    known = set()
    for zone, room in rooms(zones):
        known |= {zone.id, "%s/%s" % (zone.id, room.id)}
    bad = [p for p in picks if p not in known]
    if bad:
        raise SourceError("No zone or room called %s (they're zone or zone/room, as in Content/Zones)." % ", ".join(bad))


# --------------------------------------------------------------------------- #
# ASSETS.md and Content.mgcb
# --------------------------------------------------------------------------- #

ASSETS = os.path.join(ROOT, "ASSETS.md")
MGCB = os.path.join(CONTENT, "Content.mgcb")
SUPPLIED = "supplied by Caleb Kiatoukaysi for Legend Quest"

TEXTURE_BLOCK = (
    "#begin {0}",
    "/importer:TextureImporter",
    "/processor:TextureProcessor",
    "/processorParam:ColorKeyColor=255,0,255,255",
    "/processorParam:ColorKeyEnabled=False",
    "/processorParam:GenerateMipmaps=False",
    "/processorParam:PremultiplyAlpha=True",
    "/processorParam:ResizeToPowerOfTwo=False",
    "/processorParam:MakeSquare=False",
    "/processorParam:TextureFormat=Color",
    "/processorParam:Version=3.8.5.1",
    "/build:{0}",
)


def _read_lines(path):
    with open(path, "rb") as f:
        return f.read().decode("utf-8").replace("\r\n", "\n").split("\n")


def _write_lines(path, lines, before):
    if lines == before:
        return False
    with open(path, "wb") as f:
        f.write("\r\n".join(lines).encode("utf-8"))
    return True


def named_in(line):
    """The files an ASSETS.md line is about: the paths before its " - ", or none."""
    if not line or line.startswith("#") or " - " not in line:
        return []
    return [p.strip() for p in line.split(" - ", 1)[0].split(",")]


def find_line(lines, match):
    for i, line in enumerate(lines):
        if any(match(p) for p in named_in(line)):
            return i
    return None


def put_line(lines, line, section, match, after=None):
    """Puts a line in ASSETS.md's lines: in place of the line about any file `match` says yes to, or
    else after the line `after` says yes to, or else at the end of `section`. A blank line goes
    either side, as the file has them."""
    i = find_line(lines, match)
    if i is not None:
        lines[i] = line
        return lines
    j = find_line(lines, after) if after else None
    if j is not None:
        lines[j + 1:j + 1] = ["", line]
        return lines
    heads = [k for k, l in enumerate(lines) if l.startswith("## ")]
    at = None
    for n, k in enumerate(heads):
        if lines[k].strip() == section:
            at = heads[n + 1] if n + 1 < len(heads) else len(lines)
    if at is None:
        lines += ["", section, ""]
        at = len(lines)
    while at > 0 and lines[at - 1].strip() == "":
        at -= 1
    lines[at:at] = ["", line]
    return lines


def sync_assets(entries, path=ASSETS):
    """Writes ASSETS.md's lines: each entry (section, match, line, after) as put_line does. Returns
    whether anything changed."""
    before = _read_lines(path)
    lines = list(before)
    for section, match, line, after in entries:
        lines = put_line(lines, line, section, match, after)
    return _write_lines(path, lines, before)


def _blocks(lines):
    """Content.mgcb's blocks: (name, first line, last line), each from its #begin to the line before
    the next blank line or #begin."""
    out = []
    i = 0
    while i < len(lines):
        if lines[i].startswith("#begin "):
            j = i + 1
            while j < len(lines) and lines[j].strip() and not lines[j].startswith("#begin "):
                j += 1
            out.append((lines[i][len("#begin "):].strip(), i, j - 1))
            i = j
        else:
            i += 1
    return out


def sync_mgcb(prefix, path=MGCB):
    """Makes Content.mgcb build every .png and copy every .txt under a part of Content (prefix, from
    Content with forward slashes: "Rooms/docks/pier/" for a folder, "Tiles/questeria.png" for one
    file), and takes out the blocks under it for files that are gone. Nothing else in it changes. A
    new block goes after the last one in its folder (or its folder's folder, or at the end).
    Returns (added, removed)."""
    root = os.path.join(CONTENT, *prefix.rstrip("/").split("/"))
    if prefix.endswith("/"):
        present = sorted("%s%s" % (prefix, n) for n in listed(root) if n.endswith((".png", ".txt")))
    else:
        present = [prefix] if os.path.isfile(root) else []
    before = _read_lines(path)
    lines = list(before)
    removed = []
    for name, first, last in reversed(_blocks(lines)):
        if name.startswith(prefix) and name not in present:
            end = last + 1
            while end < len(lines) and not lines[end].strip():
                end += 1
            del lines[first:end]
            removed.append(name)
    added = []
    for rel in present:
        blocks = _blocks(lines)
        if any(name == rel for name, _, _ in blocks):
            continue
        block = [l.format(rel) for l in TEXTURE_BLOCK] if rel.endswith(".png") else ["#begin " + rel, "/copy:" + rel]
        folder = rel.rsplit("/", 1)[0] + "/"
        parent = folder.rstrip("/").rsplit("/", 1)[0] + "/"
        at = None
        for want in (folder, parent):
            for name, _, last in blocks:
                if name.startswith(want):
                    at = last
            if at is not None:
                break
        if at is None:
            while lines and not lines[-1].strip():
                lines.pop()
            lines += [""] + block + [""]
        else:
            lines[at + 1:at + 1] = [""] + block
        added.append(rel)
    _write_lines(path, lines, before)
    return added, sorted(removed)


def relative(path):
    """A file's path for ASSETS.md: from the repo root, forward slashes."""
    return shown(path)
