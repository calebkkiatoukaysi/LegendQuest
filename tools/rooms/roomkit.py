"""
The pieces the room tools share (generate_room_placeholders.py, make_room_template.py and
cut_room.py): the layers' sizes and scales, an image kept as one flat run of bytes, the catalogs'
props, tilesets and townsfolk read off the game's own files, and the zone files through
zonefile.py.

This isn't run on its own.

A room is drawn in canvas pixels (1600 to 4800 wide, 900 tall) and every layer is saved at art
resolution, an art pixel being a square of canvas pixels, so the pixels stay square:

    layer   scale   parallax   what it is
    sky     4       0.1        a gradient and two or three clouds (streets only)
    far     2       0.3        the skyline: big props cooled and hazed (streets only)
    mid     3       1.0        the buildings, doors and dressing; an interior's wall
    tiles   4                  the tile strip, drawn by the game from the room file
    fore    4       1.15       what's between you and the room, dark (1.0 indoors)

A layer with parallax p scrolls p times as far as the room does, so it's 1600 + p * (width - 1600)
canvas pixels wide; the game draws it at world x (1 - p) * camera's left edge. Something meant to
stand at room x when the camera's centred on it goes at 800 + p * (x - 800) in the layer
(layer_x below); in the mid layer that's just x.

The images are a bytearray of RGBA rows, not a list per pixel as pixels.py's are, since a preview
is a million and a half pixels. Compositing finds the runs of solid and see-through pixels with a
regular expression over the alpha bytes, so the props' hard edges cost almost nothing. Reading and
writing PNGs is pixels.py's.

Only needs CPython.
"""

import colorsys
import os
import re
import sys
import zlib

HERE = os.path.dirname(os.path.abspath(__file__))
TOOLS = os.path.dirname(HERE)
ROOT = os.path.dirname(TOOLS)
CONTENT = os.path.join(ROOT, "Content")
sys.path.insert(0, TOOLS)
sys.path.insert(0, HERE)
from pixels import read_png, write_png  # noqa: E402
import zonefile  # noqa: E402

CANVAS_W, CANVAS_H = 1600, 900
ROOM_SCALE = 4
"""Canvas pixels to a world unit: the game divides room files' numbers by this."""

#: Each layer's scale (canvas pixels to an art pixel) and parallax.
LAYERS = {
    "sky": (4, 0.1),
    "far": (2, 0.3),
    "mid": (3, 1.0),
    "tiles": (4, 1.0),
    "fore": (4, 1.15),
}

TILE = 16
"""A tile, in art pixels; the strip draws them at 4x, 64 canvas pixels."""

THUMB_W, THUMB_H = 200, 113

FORE_LIMIT = 0.12
"""The most of the floor band the foreground may cover."""


# --------------------------------------------------------------------------- #
# Layers
# --------------------------------------------------------------------------- #

def parallax(room, layer):
    """A layer's parallax in a room: interiors don't have any."""
    if room.kind == "interior":
        return 1.0
    return LAYERS[layer][1]


def layer_canvas_width(room, layer):
    p = parallax(room, layer)
    return int(round(CANVAS_W + p * (room.width - CANVAS_W)))


def layer_size(room, layer):
    """A layer's size in its own art pixels."""
    s = LAYERS[layer][0]
    return -(-layer_canvas_width(room, layer) // s), CANVAS_H // s


def layer_x(room, layer, x):
    """Where, in a layer's canvas pixels, something that stands at room x goes."""
    p = parallax(room, layer)
    return CANVAS_W / 2 + p * (x - CANVAS_W / 2)


def layer_origin(room, layer, cam_left):
    """Where a layer's left edge is on the room's canvas when the camera's left edge is cam_left."""
    return (1 - parallax(room, layer)) * cam_left


def seed_of(*parts):
    """A seed from names, the same on every run (Python's own hash of a string isn't)."""
    return zlib.crc32("/".join(str(p) for p in parts).encode("utf-8"))


# --------------------------------------------------------------------------- #
# Images
# --------------------------------------------------------------------------- #

_SOLID = re.compile(b"\xff+")
_PARTIAL = re.compile(b"[\x01-\xfe]")


class Img:
    """An RGBA image, its rows one after another in a bytearray."""

    __slots__ = ("w", "h", "buf")

    def __init__(self, w, h, fill=(0, 0, 0, 0), buf=None):
        self.w, self.h = w, h
        self.buf = buf if buf is not None else bytearray(bytes(fill) * (w * h))

    @staticmethod
    def load(path):
        w, h, ch, rows = read_png(path)
        if ch == 4:
            return Img(w, h, buf=bytearray(b"".join(bytes(r) for r in rows)))
        img = Img(w, h)
        for y, r in enumerate(rows):
            line = bytearray(w * 4)
            for c in range(3):
                line[c::4] = r[c::3]
            line[3::4] = b"\xff" * w
            img.buf[y * w * 4:(y + 1) * w * 4] = line
        return img

    def save(self, path):
        stride = self.w * 4
        rows = []
        for y in range(self.h):
            o = y * stride
            rows.append([self.buf[o + i:o + i + 4] for i in range(0, stride, 4)])
        write_png(path, rows)

    def copy(self):
        return Img(self.w, self.h, buf=bytearray(self.buf))

    def get(self, x, y):
        o = (y * self.w + x) * 4
        return tuple(self.buf[o:o + 4])

    def alpha(self, x, y):
        if 0 <= x < self.w and 0 <= y < self.h:
            return self.buf[(y * self.w + x) * 4 + 3]
        return 0

    def set(self, x, y, c):
        if 0 <= x < self.w and 0 <= y < self.h:
            o = (y * self.w + x) * 4
            self.buf[o:o + 4] = bytes((c[0], c[1], c[2], c[3] if len(c) > 3 else 255))

    def blend(self, x, y, c, a=None):
        """Lays a colour over a pixel at an opacity (0..255), as pixels.put does."""
        if not (0 <= x < self.w and 0 <= y < self.h):
            return
        sa = (c[3] if len(c) > 3 else 255) if a is None else a
        if sa <= 0:
            return
        o = (y * self.w + x) * 4
        if sa >= 255:
            self.buf[o:o + 4] = bytes((c[0], c[1], c[2], 255))
            return
        b = self.buf
        da = b[o + 3]
        k = sa / 255.0
        out = sa + da * (1 - k)
        if out <= 0:
            return
        for i in range(3):
            b[o + i] = int(round((c[i] * sa + b[o + i] * da * (1 - k)) / out))
        b[o + 3] = int(round(out))

    def shade(self, x, y, k, tint=None, mix=0.0):
        """Darkens (k < 1) or lightens a drawn pixel, and pulls it toward a tint. Clear pixels stay clear."""
        if not (0 <= x < self.w and 0 <= y < self.h):
            return
        o = (y * self.w + x) * 4
        b = self.buf
        if b[o + 3] == 0:
            return
        for i in range(3):
            v = b[o + i] * k
            if tint is not None:
                v = v * (1 - mix) + tint[i] * mix
            b[o + i] = max(0, min(255, int(round(v))))

    def crop(self, x, y, w, h):
        out = Img(w, h)
        for yy in range(h):
            sy = y + yy
            if not 0 <= sy < self.h:
                continue
            x0, x1 = max(0, x), min(self.w, x + w)
            if x1 <= x0:
                continue
            o = (sy * self.w + x0) * 4
            d = (yy * w + (x0 - x)) * 4
            out.buf[d:d + (x1 - x0) * 4] = self.buf[o:o + (x1 - x0) * 4]
        return out

    def bbox(self):
        """The box round every drawn pixel: x0, y0, x1, y1 (x1 and y1 past the end), or None."""
        x0, y0, x1, y1 = self.w, self.h, -1, -1
        stride = self.w * 4
        for y in range(self.h):
            alphas = self.buf[y * stride + 3:(y + 1) * stride:4]
            if alphas.count(0) == self.w:
                continue
            first = next(i for i, a in enumerate(alphas) if a)
            last = self.w - 1 - next(i for i, a in enumerate(reversed(alphas)) if a)
            x0, x1 = min(x0, first), max(x1, last)
            y0, y1 = min(y0, y), y
        if x1 < 0:
            return None
        return x0, y0, x1 + 1, y1 + 1

    def over(self, src, ox, oy):
        """Composites another image over this one with its top left at (ox, oy)."""
        x0, x1 = max(0, ox), min(self.w, ox + src.w)
        y0, y1 = max(0, oy), min(self.h, oy + src.h)
        if x1 <= x0 or y1 <= y0:
            return
        n = x1 - x0
        sb, db = src.buf, self.buf
        for y in range(y0, y1):
            s0 = ((y - oy) * src.w + (x0 - ox)) * 4
            seg = sb[s0:s0 + n * 4]
            alphas = seg[3::4]
            d0 = (y * self.w + x0) * 4
            for m in _SOLID.finditer(alphas):
                a, b = m.span()
                db[d0 + a * 4:d0 + b * 4] = seg[a * 4:b * 4]
            for m in _PARTIAL.finditer(alphas):
                i = m.start()
                p = seg[i * 4:i * 4 + 4]
                self.blend(x0 + i, y, p)

    def fill(self, x, y, w, h, c):
        line = bytes((c[0], c[1], c[2], c[3] if len(c) > 3 else 255)) * max(0, w)
        for yy in range(max(0, y), min(self.h, y + h)):
            xa, xb = max(0, x), min(self.w, x + w)
            if xb > xa:
                o = (yy * self.w + xa) * 4
                self.buf[o:o + (xb - xa) * 4] = line[:(xb - xa) * 4]

    def scaled(self, k):
        """Every pixel k by k."""
        if k == 1:
            return self.copy()
        out = Img(self.w * k, self.h * k)
        stride = out.w * 4
        for y in range(self.h):
            src = self.buf[y * self.w * 4:(y + 1) * self.w * 4]
            line = bytearray(stride)
            for r in range(k):
                for c in range(4):
                    line[r * 4 + c::k * 4] = src[c::4]
            for r in range(k):
                o = (y * k + r) * stride
                out.buf[o:o + stride] = line
        return out

    def view(self, k, x, y, w, h):
        """The part of this image, scaled k times, that's w by h at (x, y) in scaled pixels."""
        out = Img(w, h)
        stride = w * 4
        ax0 = max(0, x // k)
        ax1 = min(self.w, -(-(x + w) // k))
        if ax1 <= ax0:
            return out
        last_row, last_line = None, None
        for yy in range(h):
            ay = (y + yy) // k
            if not 0 <= ay < self.h:
                continue
            if ay != last_row:
                src = self.buf[(ay * self.w + ax0) * 4:(ay * self.w + ax1) * 4]
                n = ax1 - ax0
                wide = bytearray(n * k * 4)
                for r in range(k):
                    for c in range(4):
                        wide[r * 4 + c::k * 4] = src[c::4]
                start = x - ax0 * k
                d = max(0, -start)
                s = max(0, start)
                take = min(w - d, n * k - s)
                line = bytearray(stride)
                if take > 0:
                    line[d * 4:(d + take) * 4] = wide[s * 4:(s + take) * 4]
                last_row, last_line = ay, line
            out.buf[yy * stride:(yy + 1) * stride] = last_line
        return out

    def recolour(self, fn):
        """Runs every drawn pixel's (r, g, b) through fn."""
        b = self.buf
        cache = {}
        for o in range(0, len(b), 4):
            if b[o + 3] == 0:
                continue
            key = bytes(b[o:o + 3])
            c = cache.get(key)
            if c is None:
                c = cache[key] = bytes(clamp(v) for v in fn(key[0], key[1], key[2]))
            b[o:o + 3] = c

    def multiply_alpha(self, k):
        self.buf[3::4] = bytes(int(a * k) for a in self.buf[3::4])

    def solid_count(self, x0, y0, x1, y1, threshold=128):
        """How many pixels in a box are drawn (at least threshold opaque)."""
        n = 0
        for y in range(max(0, y0), min(self.h, y1)):
            alphas = self.buf[(y * self.w + max(0, x0)) * 4 + 3:(y * self.w + min(self.w, x1)) * 4:4]
            n += sum(1 for a in alphas if a >= threshold)
        return n


def clamp(v):
    return 0 if v < 0 else 255 if v > 255 else int(round(v))


def mix(a, b, t):
    return tuple(clamp(a[i] + (b[i] - a[i]) * t) for i in range(3))


def lum(r, g, b):
    return 0.299 * r + 0.587 * g + 0.114 * b


def hsv(r, g, b):
    """Hue in degrees, saturation and value 0..1."""
    h, s, v = colorsys.rgb_to_hsv(r / 255.0, g / 255.0, b / 255.0)
    return h * 360.0, s, v


def bayer(x, y):
    """The 4x4 ordered-dither threshold at a pixel, 0..1, as the generators dither."""
    m = ((0, 8, 2, 10), (12, 4, 14, 6), (3, 11, 1, 9), (15, 7, 13, 5))
    return (m[y % 4][x % 4] + 0.5) / 16.0


def hash01(x, y, seed):
    """A repeatable 0..1 number for a pixel (pixels.py's)."""
    h = (x * 374761393 + y * 668265263 + seed * 2147483647) & 0xFFFFFFFF
    h = ((h ^ (h >> 13)) * 1274126177) & 0xFFFFFFFF
    return ((h ^ (h >> 16)) & 0xFFFF) / 65535.0


# --------------------------------------------------------------------------- #
# The game's art, by the names the catalogs use
# --------------------------------------------------------------------------- #

_props = {}
_frames = None


def prop_frames():
    """How many pictures each prop's sheet has, off World/PropCatalog.cs; a prop it doesn't name has one."""
    global _frames
    if _frames is None:
        _frames = {}
        with open(os.path.join(ROOT, "World", "PropCatalog.cs"), encoding="utf-8") as f:
            text = f.read()
        for block in re.split(r"new\(\)", text):
            m = re.search(r'Texture = "Props/([\w-]+)"', block)
            n = re.search(r"Frames = (\d+)", block)
            if m:
                _frames[m.group(1)] = int(n.group(1)) if n else 1
    return _frames


def prop_exists(name):
    return os.path.isfile(os.path.join(CONTENT, "Props", name + ".png"))


def prop(name):
    """A prop's first picture: its foot is the middle of its bottom edge, as PropSprite draws it."""
    if name not in _props:
        img = Img.load(os.path.join(CONTENT, "Props", name + ".png"))
        frames = prop_frames().get(name, 1)
        if frames > 1:
            img = img.crop(0, 0, img.w // frames, img.h)
        _props[name] = img
    return _props[name]


_art = {}


def art(rel):
    """Any picture under Content/, read once."""
    if rel not in _art:
        _art[rel] = Img.load(os.path.join(CONTENT, rel))
    return _art[rel]


class Tileset:
    """A tileset's sheet and its tiles, read off World/TilesetCatalog.cs so the frames line up with the game's."""

    def __init__(self, name, tiles):
        self.name = name
        self.sheet = art("Tiles/%s.png" % name)
        self.tiles = tiles  # [(name, variants, frames, block, edged)]
        self.ids = {}
        self.first = []
        frame = 0
        for i, (tname, variants, frames, block, edged) in enumerate(tiles):
            self.ids[tname] = i + 1
            self.first.append(frame)
            frame += variants * frames * block * block + (4 if edged else 0)

    def frame_of(self, tid, x, y):
        """Tilemap.FrameOf at time zero."""
        _, variants, frames, block, _ = self.tiles[tid - 1]
        pick = (((x // block * 73856093) ^ (y // block * 19349663)) & 0xFFFFFFFF) >> 7
        variant = pick % variants
        step = 0 if frames == 1 else (pick // variants) % frames
        return self.first[tid - 1] + (variant * frames + step) * block * block + y % block * block + x % block

    def edge_frame(self, tid, side):
        _, variants, frames, block, _ = self.tiles[tid - 1]
        return self.first[tid - 1] + variants * frames * block * block + side

    def edged(self, tid):
        return self.tiles[tid - 1][4]

    def tile(self, frame):
        cols = self.sheet.w // TILE
        return self.sheet.crop(frame % cols * TILE, frame // cols * TILE, TILE, TILE)

    def tile_named(self, name, variant=0, x=0, y=0):
        """A tile's picture by name: its variant'th variant, the block cell at (x, y)."""
        tid = self.ids[name]
        _, variants, frames, block, _ = self.tiles[tid - 1]
        f = self.first[tid - 1] + (variant % variants * frames) * block * block + y % block * block + x % block
        return self.tile(f)

    def has(self, name):
        return name in self.ids


_tilesets = None


def tileset(name):
    global _tilesets
    if _tilesets is None:
        _tilesets = {}
        with open(os.path.join(ROOT, "World", "TilesetCatalog.cs"), encoding="utf-8") as f:
            text = f.read()
        for chunk in text.split("private static readonly TilesetDefinition")[1:]:
            m = re.search(r'Id = "([\w-]+)"', chunk)
            if not m:
                continue
            tiles = []
            for t in re.finditer(r'new\(\)\s*\{\s*Name = "([\w-]+)"([^}]*)\}', chunk):
                body = t.group(2)

                def num(key, default):
                    n = re.search(key + r" = (\d+)", body)
                    return int(n.group(1)) if n else default
                tiles.append((t.group(1), num("Variants", 1), num("Frames", 1), num("Block", 1),
                              "Edged = true" in body))
            _tilesets[m.group(1)] = tiles
    if name not in _tilesets:
        raise KeyError("TilesetCatalog.cs has no tileset called " + name)
    if not isinstance(_tilesets[name], Tileset):
        _tilesets[name] = Tileset(name, _tilesets[name])
    return _tilesets[name]


def walker(sheet, facing, column=0):
    """A frame of a townsfolk sheet (Content/People) or the paperdoll's body: 32x48, rows down, left, right, up."""
    rows = {"down": 0, "left": 1, "right": 2, "up": 3}
    img = art(sheet if sheet.endswith(".png") else "People/%s.png" % sheet)
    return img.crop(column * 32, rows[facing] * 48, 32, 48)


WALKER_FEET = (16, 46)


# --------------------------------------------------------------------------- #
# Zones and rooms
# --------------------------------------------------------------------------- #

def zone_files(folder=None):
    folder = folder or os.path.join(CONTENT, "Zones")
    if not os.path.isdir(folder):
        return []
    return [os.path.join(folder, n) for n in sorted(os.listdir(folder)) if n.endswith(".txt")]


def load_zone(path):
    return zonefile.load_zone(path)


def door_boxes(room):
    """Every door's box (left, top, width, height) in canvas pixels, one per x: a locked twin shares its door's box."""
    seen = {}
    for d in room.doors:
        key = round(d.x)
        if key not in seen:
            seen[key] = (d, room.door_box(d))
    return [seen[k] for k in sorted(seen)]


def band(room):
    """The floor band's top and bottom: the highest back edge and the lowest front edge."""
    ys = [p[1] for p in room.floor]
    return min(ys), max(ys)


def horizon(room):
    """Where the far layer's ground meets the sky, in canvas pixels: a little above the band."""
    return int(band(room)[0]) - 40


def paint_bands(room):
    """The two lines a painted background is cut along (12-rooms 3.5), in canvas pixels: the sky
    is above the first, the far layer between them, the mid layer below the second. An interior
    is all mid layer, so both are 0."""
    if room.kind == "interior":
        return 0, 0
    top = int(band(room)[0])
    return top - 330, top - 210


def layer_columns(room, layer):
    """Which columns of a flat painting of the whole room a layer takes, so that with the camera in
    the room's middle the layers put back together are the painting exactly: the middle stretch of
    it as wide as the layer (wider than the room for the foreground, which runs off both ends)."""
    lw = layer_canvas_width(room, layer)
    x0 = (room.width - lw) // 2
    return x0, x0 + lw
