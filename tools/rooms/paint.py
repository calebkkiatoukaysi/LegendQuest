"""
The rooms' painting library: side-on backgrounds in the game's illustrated style, drawn as vector
shapes with tools/rig/vector.py and shaded the way tools/rig/style.py shades the characters. A room
painter (paint_room.py) builds its layers out of these pieces, and paint_tiles.py draws the floor
tilesets with the same palette.

The look (Story/12-rooms.md §3.3 and the owner's Art Guide): clean dark outlines, heavier round a
thing's silhouette and lighter inside it; flat fills cel-shaded in two or three tones with hard
edges, lit from the top left; soft gradients only in the sky and the distance; readable,
uncluttered shapes; saturated where the eye should go and muted elsewhere. No pixel art: every
edge is anti-aliased at the size it's shown.

Everything is drawn at canvas resolution (a pixel is a canvas pixel; the screen is 1600 x 900).
Each thing is drawn on its own vector.Canvas, SS x SS samples a pixel, in the layer's own
coordinates (a Piece), then laid into its Layer with its outline round it. A Layer is premultiplied
float RGBA; it's written at canvas resolution, or averaged down to the art resolution RoomScene
draws today (sky 4, far 2, mid 3, fore 4 screen pixels to an art pixel).

    sky = paint.Layer(1760, 900)
    paint.sky(sky, "day", seed=3, sun=(260, 170), horizon=620)
    mid = paint.Layer(3201, 900)
    doorway = paint.building(mid, paint.House(x0=400, width=330, ground=600, floors=(("stone", 190),
                             ("timber", 150)), roof="hip", roof_colour="roof.blue", door=paint.Door(560, ...)))
    paint.tree(mid, 1500, 600, kind="blossom", seed=7)
    mid.save("mid.png", art=3)

The pieces, by section:
  colour      ramp(), tones(), haze(), PALETTE
  drawing     Piece (one thing on its own canvas, outlined into a Layer; sub() and part() draw a
              part on a canvas its own size and stamp it back), Layer (a whole layer)
  ground      floor_shade() (the shadow along the floor's back edge), near_shade() (the ground
              darkening toward the viewer, for the foreground)
  sky         sky(): day, dusk, night and twilight; painted clouds, a sun or a moon, stars
  far         mountains(), hill(), skyline(), castle(), forest_line(), sea()
  buildings   building(House(...)) from parts: stone, plaster, brick and timber walls (wall());
              hip, gable, front-gable and cone roofs with rows of tiles; Windows; Doors (arch,
              square, double, open) sized to the zone file's boxes; chimneys, dormers; and extras
              fixed to the front: awning(), hanging_sign(), wall_sign(), banner(), wall_lantern()
  plants      tree() (oak, blossom, cypress), bush(), topiary(), vines(), flowers(), flower_box()
  props       stall(), crate(), barrel(), sacks(), basket(), pot(), bench(), fence() (picket or
              iron railings), low_wall(), lamp_post(), lantern_body(), lantern_string(),
              lantern_pole(), cart(), well(), fore_foliage()
  civic       tower(), gatehouse() and archway() (open, doors, portcullis), stairs(), balustrade(),
              balcony() (a house extra), notice_board(), arcade()
  interiors   back_wall(), beams(), shelf(), fireplace(), table(), counter(), interior_window()

    python tools/rooms/paint.py      paints one of everything into shots/art/rooms/paint-kit.png

Light comes from the top left, always (rooms are never mirrored). Shading is planar on flat things
(the lit face, a hard shadow on the side away from the light, cast shadows down and to the right)
and style.shade()'s pseudo-normals on round ones (barrels, sacks, bushes).

Deterministic: noise is hash01 (pixels.py's), seeded by the caller's seed and a constant per use;
float64 throughout, written by vector.write_png.

Only needs CPython and numpy.
"""

import math
import os
import sys

import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))
TOOLS = os.path.dirname(HERE)
sys.path.insert(0, os.path.join(TOOLS, "rig"))
sys.path.insert(0, TOOLS)
import style  # noqa: E402
import theme  # noqa: E402
import vector  # noqa: E402
from vector import Canvas, Mask, Path, block_mean, hash01, write_png  # noqa: E402

SS = 3
"""Samples per pixel along each axis for everything drawn here (vector.py's default is 4; at canvas
resolution 3 is as smooth and a good deal quicker)."""

LIGHT = (-0.6, -0.8)
"""Toward the light, in the picture's plane: up and to the left. Cast shadows fall the other way."""

INK = "#24182a"
"""The outline round everything on the mid layer and the foreground: a near-black plum."""

OUTER = 2.2
"""How far the outline reaches beyond a thing's silhouette, in pixels, on the mid layer."""

INNER = 1.4
"""An inner edge line's width, inside a part: lighter than the outer line, in the part's own line tone."""

DETAIL = 1.0
"""A detail line: joints, planks, tile rows."""

# --------------------------------------------------------------------------- #
# Colour
# --------------------------------------------------------------------------- #

SHADE_TO = "#3b3270"
"""Shadows lean cool lavender, as the title's do."""
LIGHT_TO = "#fff3dc"
HIGH_TO = "#fffaf0"

PALETTE = {
    # stone, plaster and brick: Questeria's warm cream
    "stone": "#efe2cc", "stone.warm": "#ead2b2", "stone.grey": "#cdc5cc", "stone.dark": "#8f8496",
    "stone.blue": "#b9c4dc", "plaster": "#f5ecdb", "plaster.pink": "#f3d4d0", "plaster.blue": "#d8e3ef",
    "plaster.yellow": "#f6e2b0", "brick": "#c47c64", "slab": "#d9cbb6",
    # roofs: the style sheet's azure and rose, and a plum and a teal for rhythm
    "roof.blue": "#3d8fd4", "roof.teal": "#2c9aa8", "roof.pink": "#d95f8f", "roof.plum": "#8c5cac",
    "roof.slate": "#5f6f98", "roof.red": "#c4544c",
    # wood, metal, glass
    "wood": "#9a5f3a", "wood.dark": "#6b4030", "wood.light": "#c38c5a", "wood.blue": "#3f6fa8",
    "wood.green": "#4c8a6c", "wood.pink": "#c95a84", "wood.teal": "#2f8c94", "iron": "#4b4559",
    "gold": "#e9b54c", "brass": "#c99a48", "glass": "#3e5f92", "glass.lit": "#ffc760",
    # cloth
    "cloth.pink": "#e25d8e", "cloth.cream": "#f7eedf", "cloth.blue": "#3b78c8", "cloth.navy": "#2c4588",
    "cloth.teal": "#2b9ca4", "cloth.gold": "#e9b54c", "cloth.plum": "#8b509f", "cloth.red": "#c9474f",
    "canvas": "#efe4cb", "sack": "#cfb48e", "rope": "#8a6a4e",
    # plants
    "leaf": "#52a052", "leaf.dark": "#2f7c5c", "leaf.yellow": "#8fbf4c", "leaf.blue": "#3f8c78",
    "blossom": "#f49cc2", "blossom.white": "#fbeef2", "bark": "#7c5442",
    "flower.pink": "#ef6fa2", "flower.yellow": "#f7cf54", "flower.white": "#fcf5ec",
    "flower.blue": "#7ba2f2", "flower.violet": "#a87cda", "flower.red": "#e2534f",
    # goods
    "apple": "#d9473f", "orange": "#ef9637", "pear": "#b8c94c", "bread": "#da9c54", "cheese": "#f2cc5a",
    "terracotta": "#c9714d", "fish": "#9fb4c8", "pottery": "#5f86b8",
    # far and ground
    "mountain": "#8d7fc0", "hill": "#6fae6a", "sea": "#3f7fc8", "sand": "#ecd3a2", "earth": "#a87a56",
    "grass": "#68ad55", "water": "#3f8fd0",
}


def _clamp_dark(c, top=0.30):
    L, a, b = theme.oklab(theme.rgb(c))
    return theme.srgb((min(L, top), a, b)) if L > top else theme.rgb(c)


_ramps = {}


def ramp(base, shade=SHADE_TO, light=LIGHT_TO, high=HIGH_TO, line=None, soft=1.0):
    """The ten tones of a colour, as theme.tones() gives them (line, deep .. highlight, by index, in
    `steps`), but for the backgrounds: shadows toward a cool lavender (`shade`), lights toward a
    warm cream, and lines the colour mixed most of the way to INK. `soft` < 1 brings the shadows in
    closer (a calmer surface: the floor). Works with Canvas.tone() and style.shade()."""
    key = (theme.hexof(base), shade, light, high, line, soft)
    if key in _ramps:
        return _ramps[key]
    b = theme.rgb(base)
    shadow = theme.lighten(theme.mix(b, shade, 0.32 * soft), -0.045 * soft)
    deep = theme.lighten(theme.mix(b, shade, 0.58 * soft), -0.11 * soft)
    lit = theme.mix(b, light, 0.24)
    hi = theme.mix(b, high, 0.52)
    ln = theme.rgb(line) if line else _clamp_dark(theme.lighten(theme.mix(b, INK, 0.78), -0.03))
    mid = lambda p, q: theme.mix(p, q, 0.5)  # noqa: E731
    steps = [ln, deep, mid(deep, shadow), shadow, mid(shadow, b), b, mid(b, lit), lit, mid(lit, hi), hi]
    named = {name: tuple(steps[i]) for i, name in enumerate(theme.TONE_NAMES)}
    named.update(steps=[tuple(s) for s in steps], group=0, kind="scenery")
    _ramps[key] = named
    return named


def tones(name, **kw):
    """A PALETTE colour's ramp, or any '#rrggbb' or (r, g, b)'s."""
    return ramp(PALETTE.get(name, name) if isinstance(name, str) else name, **kw)


def haze(t, toward, k):
    """A ramp moved a share k of the way toward a haze colour, every tone: the far layer's aerial
    perspective (lower contrast, cooler)."""
    steps = [theme.mix(s, toward, k) for s in t["steps"]]
    named = {name: tuple(steps[i]) for i, name in enumerate(theme.TONE_NAMES)}
    named.update(steps=[tuple(s) for s in steps], group=0, kind="scenery")
    return named


def rgb(c):
    return theme.rgb(PALETTE.get(c, c) if isinstance(c, str) else c)


def mixc(a, b, t):
    """Two colours mixed in OKLab."""
    return theme.mix(rgb(a), rgb(b), t)


# --------------------------------------------------------------------------- #
# Drawing: a Piece is one thing on its own canvas; a Layer is a whole layer
# --------------------------------------------------------------------------- #

class Piece:
    """One thing (a building, a tree, a stall) drawn on a vector.Canvas that covers the box
    (x0, y0)-(x1, y1) of its layer, plus a margin. Paths are given in the layer's coordinates.
    When it's laid into a Layer, everything painted on it gets the outline round its silhouette;
    glows it asked for go on after, added over everything.

        p = Piece(100, 300, 260, 600)
        wall = p.fill(Path.rect(110, 400, 140, 200), tones("stone"))
        p.edge(Path.rect(110, 400, 140, 200), tones("stone"))
        layer.place(p)
    """

    def __init__(self, x0, y0, x1, y1, ss=SS, pad=8):
        self.ox = int(math.floor(x0)) - pad
        self.oy = int(math.floor(y0)) - pad
        self.w = int(math.ceil(x1)) + pad - self.ox
        self.h = int(math.ceil(y1)) + pad - self.oy
        self.ss = ss
        self.c = Canvas(self.w, self.h, ss)
        self.glows = []
        self.shadows = []
        self.parent = None

    # parts on canvases of their own ------------------------------------------ #

    def sub(self, x0, y0, x1, y1):
        """A Piece for one part of this one, over the box (x0, y0)-(x1, y1) (kept inside this
        piece): drawing a window or a sign on a canvas its own size costs what the window does,
        not what the whole building does. Draw on it, then stamp() it back. What's already
        painted here shows through its covered()."""
        x0, y0 = max(x0, self.ox), max(y0, self.oy)
        x1, y1 = min(x1, self.ox + self.w), min(y1, self.oy + self.h)
        q = Piece(x0, y0, max(x0 + 1, x1), max(y0 + 1, y1), ss=self.ss, pad=0)
        q.w = min(q.w, self.ox + self.w - q.ox)
        q.h = min(q.h, self.oy + self.h - q.oy)
        q.c = Canvas(q.w, q.h, self.ss)
        q.parent = self
        return q

    def stamp(self, q):
        """Lays a part drawn on a sub() back in, with its glows and shadows."""
        self.c.stamp(q.c, q.ox - self.ox, q.oy - self.oy)
        self.glows += q.glows
        self.shadows += q.shadows

    def part(self, box, fn, *args, **kw):
        """Draws fn(sub_piece, *args) on a sub() over box and stamps it back; returns what fn did."""
        q = self.sub(*box)
        out = fn(q, *args, **kw)
        self.stamp(q)
        return out

    # coordinates ------------------------------------------------------------ #

    def P(self, path):
        return path.translate(-self.ox, -self.oy)

    def shape(self, path, rule="evenodd"):
        return self.c.shape(self.P(path), rule)

    def line_shape(self, path, widths):
        return self.c.line_shape(self.P(path), widths)

    def below(self, m, y):
        return m.below(y - self.oy)

    def above(self, m, y):
        return m.above(y - self.oy)

    def where(self, m, f):
        return m.where(lambda x, y: f(x + self.ox, y + self.oy))

    def everything(self):
        return self.c.everything()

    def nothing(self):
        return self.c.nothing()

    def covered(self):
        """Everything painted so far, on this piece and (for a sub()) under it on its parent."""
        own = self.c.cov
        if self.parent is None:
            return Mask(self.c, own.copy())
        par = self.parent.covered().a
        ss = self.ss
        ys, xs = (self.oy - self.parent.oy) * ss, (self.ox - self.parent.ox) * ss
        crop = par[ys:ys + own.shape[0], xs:xs + own.shape[1]]
        out = own.copy()
        out[:crop.shape[0], :crop.shape[1]] = np.maximum(out[:crop.shape[0], :crop.shape[1]], crop)
        return Mask(self.c, out)

    # painting --------------------------------------------------------------- #

    def fill(self, path, t, which="base", clip=None, alpha=1.0, rule="evenodd"):
        """Fills a path in one tone of a ramp; returns its Mask (before clipping)."""
        m = path if isinstance(path, Mask) else self.shape(path, rule)
        self.c.tone(m, t, which, alpha=alpha, clip=clip)
        return m

    def tone(self, m, t, which, clip=None, alpha=1.0):
        self.c.tone(m, t, which, alpha=alpha, clip=clip)
        return m

    def colour(self, m, c, alpha=1.0, clip=None):
        """Paints a region in a fixed colour."""
        self.c.paint(m, tuple(rgb(c)) + (255,), alpha=alpha, clip=clip)
        return m

    def stroke(self, path, width, t, which="line", clip=None, alpha=1.0):
        """A line along a path, `width` pixels (a number, (start, end), or a function of 0..1)."""
        m = self.line_shape(path, width)
        self.c.tone(m, t, which, alpha=alpha, clip=clip)
        return m

    def edge(self, path, t, width=INNER, which="line", clip=None):
        """A part's inner edge line: its outline drawn `width` pixels inside it."""
        shape = self.shape(path)
        ring = self.line_shape(path.close() if not _closed(path) else path, width * 2.0) & shape
        if clip is not None:
            ring = ring & clip
        self.c.tone(ring, t, which)
        return ring

    def edge_mask(self, m, t, width=INNER, which="line"):
        """The inner edge line of any region (a union of shapes): worked out from its distance
        field in its own box, so it costs what the region does, not the canvas."""
        inside = m.inside
        rows = np.nonzero(inside.any(axis=1))[0]
        if len(rows) == 0:
            return self.nothing()
        cols = np.nonzero(inside.any(axis=0))[0]
        pad = int(math.ceil(width * self.ss)) + 3
        H, W = inside.shape
        box = (slice(max(0, rows[0] - pad), min(H, rows[-1] + 1 + pad)),
               slice(max(0, cols[0] - pad), min(W, cols[-1] + 1 + pad)))
        d = vector.depth(inside[box], width * self.ss + 2)
        zone = np.zeros_like(m.a)
        zone[box] = ((d <= width * self.ss) & inside[box]).astype(np.float64)
        ring = Mask(self.c, m.a * zone)
        self.c.tone(ring, t, which)
        return ring

    def cast(self, m, dx, dy, onto, t, which="shadow", alpha=1.0):
        """A hard cast shadow: region m moved (dx, dy), where it falls on `onto` and m isn't."""
        sh = (m.shift(dx, dy) & onto) - m
        self.c.tone(sh, t, which, alpha=alpha)
        return sh

    def glow(self, x, y, r, colour, strength=0.5):
        """Asks for a soft additive glow round (x, y), laid on after the piece (a lit window, a lantern)."""
        self.glows.append((x, y, r, rgb(colour), strength))

    def ground_shadow(self, x0, x1, y, depth=10, strength=0.35):
        """Asks for a soft contact shadow on the floor under a thing, from x0 to x1 at y (laid on
        the layer before the piece)."""
        self.shadows.append((x0, x1, y, depth, strength))

    # reading out ------------------------------------------------------------ #

    def rgba_premultiplied(self, ink=INK, reach=OUTER, ink_alpha=1.0):
        """The piece with its outline: everything painted, grown by `reach` pixels, in ink, under it."""
        img = vector.premultiply(self.c.rgba())
        if reach and reach > 0:
            line, _ = vector.outline_of(self.c.cov >= 0.5, self.ss, reach * 0.8, reach, reach * 1.15, pad=1)
            a = line[1:-1, 1:-1, 3].astype(np.float64) / 255.0 * ink_alpha
            ink_c = np.array(rgb(ink), np.float64) / 255.0
            under = np.zeros_like(img)
            under[..., :3] = ink_c * a[..., None]
            under[..., 3] = a
            img = img + under * (1.0 - img[..., 3:4])
        return img


def _closed(path):
    return bool(path.subs) and all(closed for _, closed in path.subs)


class Layer:
    """A whole layer at canvas resolution: H x W premultiplied float RGBA, clear to start."""

    def __init__(self, w, h, fill=None):
        self.w, self.h = int(w), int(h)
        self.px = np.zeros((self.h, self.w, 4), np.float64)
        if fill is not None:
            c = np.array(rgb(fill), np.float64) / 255.0
            self.px[..., :3] = c
            self.px[..., 3] = 1.0

    def put(self, premult, x, y):
        vector.over(self.px, premult, int(x), int(y))

    def place(self, piece, ink=INK, reach=OUTER, ink_alpha=1.0):
        """Lays a piece in: its contact shadows, then it with its outline, then its glows."""
        for (x0, x1, y, depth, strength) in piece.shadows:
            self.soft_ellipse((x0 + x1) / 2.0, y, (x1 - x0) / 2.0, depth, SHADOW_COLOUR, strength)
        self.put(piece.rgba_premultiplied(ink, reach, ink_alpha), piece.ox, piece.oy)
        for (x, y, r, c, s) in piece.glows:
            self.glow(x, y, r, c, s)

    def _box(self, x0, y0, x1, y1):
        xa, xb = max(0, int(math.floor(x0))), min(self.w, int(math.ceil(x1)))
        ya, yb = max(0, int(math.floor(y0))), min(self.h, int(math.ceil(y1)))
        if xb <= xa or yb <= ya:
            return None
        yy, xx = np.mgrid[ya:yb, xa:xb]
        return (slice(ya, yb), slice(xa, xb)), xx + 0.5, yy + 0.5

    def soft_ellipse(self, cx, cy, rx, ry, colour, strength, hard=0.35):
        """A soft-edged ellipse laid over (a contact shadow, a haze patch): full strength inside
        `hard` of its radius, falling off smoothly to nothing at its edge."""
        got = self._box(cx - rx, cy - ry, cx + rx, cy + ry)
        if got is None:
            return
        box, xx, yy = got
        d = np.sqrt(((xx - cx) / max(rx, 1e-6)) ** 2 + ((yy - cy) / max(ry, 1e-6)) ** 2)
        a = strength * _smooth(np.clip((1.0 - d) / max(1e-6, 1.0 - hard), 0.0, 1.0))
        self._lay(box, a, colour)

    def glow(self, cx, cy, r, colour, strength=0.5):
        """An additive glow: light added over what's there, falling off with the square of the distance."""
        got = self._box(cx - r, cy - r, cx + r, cy + r)
        if got is None:
            return
        box, xx, yy = got
        d = np.sqrt((xx - cx) ** 2 + (yy - cy) ** 2) / max(r, 1e-6)
        a = strength * np.clip(1.0 - d, 0.0, 1.0) ** 2
        c = np.array(rgb(colour), np.float64) / 255.0
        p = self.px[box]
        p[..., :3] = np.minimum(1.0, p[..., :3] + c * a[..., None])
        p[..., 3] = np.minimum(1.0, p[..., 3] + a * 0.6)
        p[..., :3] = np.minimum(p[..., :3], p[..., 3:4])

    def _lay(self, box, a, colour):
        c = np.array(rgb(colour), np.float64) / 255.0
        p = self.px[box]
        src = np.zeros(p.shape)
        src[..., :3] = c * a[..., None]
        src[..., 3] = a
        self.px[box] = src + p * (1.0 - a[..., None])

    def cover(self, a, colour, x=0, y=0):
        """Lays a colour over the layer at coverage `a` (a pixel-resolution array) with its top left at (x, y)."""
        h, w = a.shape
        xa, ya = max(0, x), max(0, y)
        xb, yb = min(self.w, x + w), min(self.h, y + h)
        if xb <= xa or yb <= ya:
            return
        sub = a[ya - y:yb - y, xa - x:xb - x]
        if np.ndim(colour) == 3:
            col = colour[ya - y:yb - y, xa - x:xb - x]
            p = self.px[ya:yb, xa:xb]
            src = np.zeros(p.shape)
            src[..., :3] = col * sub[..., None]
            src[..., 3] = sub
            self.px[ya:yb, xa:xb] = src + p * (1.0 - sub[..., None])
        else:
            self._lay((slice(ya, yb), slice(xa, xb)), sub, colour)

    def coverage(self, path, rule="evenodd", ss=SS):
        """A path's coverage over the whole layer, as a pixel-resolution array (for big flat
        things: hills, the sea, a mountain range), worked out only on the rows it covers."""
        x0, y0, x1, y1 = path.bounds()
        ya, yb = max(0, int(math.floor(y0)) - 1), min(self.h, int(math.ceil(y1)) + 1)
        out = np.zeros((self.h, self.w))
        if yb <= ya:
            return out
        inside = vector.raster_fill(path.translate(0, -ya), self.w, yb - ya, ss, rule)
        out[ya:yb] = block_mean(inside.astype(np.float64), ss)
        return out

    def rgba(self):
        return vector.unpremultiply(self.px)

    def art(self, k):
        """The layer averaged down k times (premultiplied, so edges stay clean): RoomScene's art
        resolution. The layer's size should be a multiple of k."""
        h, w = self.h // k, self.w // k
        return vector.unpremultiply(block_mean(self.px[:h * k, :w * k], k))

    def save(self, path, art=1, quiet=False):
        write_png(path, self.rgba() if art == 1 else self.art(art), quiet=quiet)


def floor_shade(layer, y, depth=46, strength=0.26, x0=0, x1=None):
    """A soft band of shadow on the floor just in front of the back edge (y), falling off over
    `depth` pixels: what the buildings and the street's clutter throw on the ground near them, so
    they stand on it. Painted into the mid layer before anything stands there."""
    x1 = layer.w if x1 is None else x1
    v = np.clip((np.arange(depth, dtype=np.float64) + 0.5) / depth, 0.0, 1.0)
    a = strength * (1.0 - v) ** 1.6
    band = np.repeat(a[:, None], int(x1 - x0), axis=1)
    layer.cover(band, SHADOW_COLOUR, int(x0), int(y))


def near_shade(layer, y0, strength=0.2, y1=None):
    """The ground darkening toward the viewer along the bottom of the picture, from y0 down (for
    the foreground layer: it's even across, so its parallax doesn't show)."""
    y1 = layer.h if y1 is None else y1
    v = np.clip((np.arange(int(y1 - y0), dtype=np.float64) + 0.5) / max(1, y1 - y0), 0.0, 1.0)
    a = strength * v ** 1.4
    layer.cover(np.repeat(a[:, None], layer.w, axis=1), SHADOW_COLOUR, 0, int(y0))


SHADOW_COLOUR = "#2a1f45"
"""Contact shadows on the ground: a deep violet, laid at a share of its strength."""


def _smooth(t):
    return t * t * (3.0 - 2.0 * t)


def lerp_colour(stops, v):
    """A colour field from stops [(0..1, colour), ...] at v (an array of 0..1)."""
    pos = np.array([s[0] for s in stops], np.float64)
    cols = np.array([rgb(s[1]) for s in stops], np.float64)
    out = np.zeros(np.shape(v) + (3,))
    for i in range(3):
        out[..., i] = np.interp(v, pos, cols[:, i])
    return out


# --------------------------------------------------------------------------- #
# Sky
# --------------------------------------------------------------------------- #

SKIES = {
    "day": [(0.0, "#3d6cc6"), (0.33, "#6b9be2"), (0.6, "#a7bdf0"), (0.8, "#e4bde3"), (0.92, "#fbd3d3"), (1.0, "#fde3cf")],
    "dusk": [(0.0, "#28357f"), (0.3, "#5254a8"), (0.56, "#ac6cb2"), (0.78, "#ee93b0"), (0.92, "#fcb894"), (1.0, "#ffd49c")],
    "night": [(0.0, "#0b1033"), (0.45, "#1a2462"), (0.78, "#373784"), (1.0, "#5f4c92")],
    "twilight": [(0.0, "#2a2a72"), (0.35, "#4b479e"), (0.65, "#8b69b6"), (0.86, "#d48fbf"), (1.0, "#f2b4b6")],
}

#: Each sky's clouds: lit top, body, shadowed underside, and the edge they soften into.
CLOUDS = {
    "day": ("#fffaf3", "#f7e6f0", "#cbb7e2"),
    "dusk": ("#ffe3c4", "#f4a5bb", "#8f70b4"),
    "night": ("#8d8fcf", "#5b5ca6", "#373a7c"),
    "twilight": ("#f7d3e4", "#c79ed4", "#7d69b4"),
}

#: What far things are hazed toward under each sky (12-rooms §3.1: cooler, lower contrast).
HAZE = {"day": "#b9c2ee", "dusk": "#c99ac8", "night": "#3a3c80", "twilight": "#9d86c4"}


def sky(layer, time="day", seed=0, horizon=None, sun=None, moon=None, clouds=3, stars=None, cloud_band=(60, 300)):
    """Paints a whole sky into a Layer: the gradient (soft, top to `horizon`), then a sun or a moon
    with its halo, stars at night and twilight, and `clouds` painted clouds in `cloud_band`.
    `time` is day, dusk, night or twilight."""
    h, w = layer.h, layer.w
    horizon = horizon if horizon is not None else int(h * 0.72)
    v = np.clip(np.arange(h, dtype=np.float64) / max(1, horizon), 0.0, 1.0)
    col = lerp_colour(SKIES[time], v)                                  # h x 3
    # a faint brightening toward the sun's side, so the gradient isn't flat across
    xs = np.arange(w, dtype=np.float64)
    sx = sun[0] if sun else (moon[0] if moon else w * 0.25)
    side = np.exp(-((xs - sx) / (w * 0.7)) ** 2)[None, :, None] * 0.06
    field = col[:, None, :] * (1.0 - side) + 255.0 * side
    layer.px[..., :3] = np.clip(field, 0, 255) / 255.0
    layer.px[..., 3] = 1.0

    if stars or (stars is None and time in ("night", "twilight")):
        n = int(w * h / 9000) if time == "night" else int(w * h / 22000)
        for i in range(n):
            x = hash01(i, 1, seed + 501) * w
            y = hash01(i, 2, seed + 501) ** 1.4 * horizon * 0.85
            r = 0.6 + 1.2 * hash01(i, 3, seed + 501) ** 3
            layer.soft_ellipse(x, y, r * 1.6, r * 1.6, "#fff8e6", 0.55 + 0.4 * hash01(i, 4, seed + 501), hard=0.3)
            if r > 1.4:
                _sparkle(layer, x, y, r * 5.0, "#fff8e6", 0.75)

    if sun:
        x, y = sun
        r = 44 if time == "day" else 56
        halo = "#fff1cf" if time == "day" else "#ffd0a0"
        layer.glow(x, y, r * 5.5, halo, 0.28)
        layer.glow(x, y, r * 2.4, halo, 0.35)
        disc = layer.coverage(Path.circle(x, y, r))
        layer.cover(disc, "#fffaea" if time == "day" else "#fff0c8")
    if moon:
        x, y = moon
        r = 46
        layer.glow(x, y, r * 4.0, "#c8d2ff", 0.22)
        disc = layer.coverage(Path.circle(x, y, r))
        layer.cover(disc, "#eef0ff")
        for i in range(5):
            a = hash01(i, 5, seed + 503) * 6.28
            d = r * 0.55 * hash01(i, 6, seed + 503)
            cr = r * (0.12 + 0.16 * hash01(i, 7, seed + 503))
            m = layer.coverage(Path.circle(x + math.cos(a) * d, y + math.sin(a) * d, cr))
            layer.cover(np.minimum(m, disc) * 0.5, "#c3c6e8")
        layer.cover(np.minimum(disc, 1.0 - layer.coverage(Path.circle(x - r * 0.18, y - r * 0.2, r * 0.98))) * 0.6, "#b4b6dc")

    top, bot = cloud_band
    for i in range(clouds):
        u = (i + 0.25 + 0.5 * hash01(i, 11, seed + 507)) / max(1, clouds)
        cx = u * w
        cy = top + (bot - top) * hash01(i, 12, seed + 507)
        size = 0.7 + 0.6 * hash01(i, 13, seed + 507)
        cloud(layer, cx, cy, size, time, seed + 31 * i)
    return layer


def _sparkle(layer, x, y, r, colour, strength):
    """A four-pointed glint: two thin soft diamonds."""
    for rx, ry in ((r, r * 0.12), (r * 0.12, r)):
        got = layer._box(x - rx, y - ry, x + rx, y + ry)
        if got is None:
            continue
        box, xx, yy = got
        d = np.abs(xx - x) / rx + np.abs(yy - y) / ry
        layer._lay(box, strength * np.clip(1.0 - d, 0, 1) ** 1.5, colour)


def cloud(layer, cx, cy, size=1.0, time="day", seed=0, alpha=1.0):
    """A painted cloud: a flat-bottomed heap of round puffs, lit along its top left, its underside
    in shadow, its tone edges softened a little (the sky's the one place soft gradients go)."""
    lit_c, body_c, shade_c = CLOUDS[time]
    n = 5 + int(hash01(1, 1, seed) * 4)
    width = 230 * size
    puffs = []
    for i in range(n):
        t = (i + 0.5) / n
        bump = math.sin(math.pi * t) ** 0.8
        r = (26 + 30 * bump + 14 * hash01(i, 2, seed)) * size
        x = cx - width / 2 + width * t + (hash01(i, 3, seed) - 0.5) * 18 * size
        y = cy - bump * 34 * size - (hash01(i, 4, seed) - 0.3) * 12 * size
        puffs.append((x, y, r))
    # a second, smaller heap trailing off one side
    for i in range(3):
        r = (14 + 10 * hash01(i, 6, seed)) * size
        x = cx + width / 2 + (16 + 26 * i) * size
        y = cy + 2 * size - 6 * i * size
        puffs.append((x, y, r))
    base_y = cy + 22 * size
    x0 = min(p[0] - p[2] for p in puffs) - 4
    x1 = max(p[0] + p[2] for p in puffs) + 4
    y0 = min(p[1] - p[2] for p in puffs) - 4
    got = layer._box(x0, y0, x1, base_y + 6)
    if got is None:
        return
    box, xx, yy = got

    def union(dx, dy, k):
        a = np.zeros(xx.shape)
        for (px, py, r) in puffs:
            d = np.sqrt((xx - px - dx * r) ** 2 + (yy - py - dy * r) ** 2)
            a = np.maximum(a, np.clip(r * k - d + 0.5, 0.0, 1.0))
        return a

    shape = union(0, 0, 1.0) * np.clip(base_y - yy + 0.5, 0.0, 1.0)
    shadow = np.clip(shape - union(-0.10, -0.30, 1.0), 0.0, 1.0)
    shadow = np.maximum(shadow, shape * np.clip((yy - (base_y - 12 * size)) / 4.0, 0.0, 1.0))
    lit = np.clip(shape - union(0.08, 0.16, 1.0), 0.0, 1.0)
    shadow = vector.blur(shadow, 1, 2)
    lit = vector.blur(lit, 1, 2) * (1.0 - shadow)
    v = np.clip((yy - y0) / max(1.0, base_y - y0), 0, 1)
    body = lerp_colour([(0.0, lit_c), (0.55, body_c), (1.0, body_c)], v)
    col = body * (1 - shadow[..., None]) + np.array(rgb(shade_c)) * shadow[..., None]
    col = col * (1 - lit[..., None]) + np.array(rgb(lit_c)) * lit[..., None]
    a = shape * alpha
    p = layer.px[box]
    src = np.zeros(p.shape)
    src[..., :3] = col / 255.0 * a[..., None]
    src[..., 3] = a
    layer.px[box] = src + p * (1.0 - a[..., None])


# --------------------------------------------------------------------------- #
# The far layer: mountains, a skyline, the castle, forests, the sea
# --------------------------------------------------------------------------- #

def _far_line(t, haze_c, k):
    """A far thing's outline colour: its own line tone, hazed, so the distance still has edges
    but never dark ones."""
    return theme.mix(t["line"], haze_c, min(0.9, k + 0.35))


def mountains(layer, base_y, top_y, haze_c, seed=0, colour="mountain", k=0.45, snow=True, count=None, x0=0, x1=None):
    """A range of mountains along the layer, peaks between top_y and base_y: each peak's left face
    lit and its right in shadow, snow on the tallest, all hazed a share k toward haze_c."""
    x1 = layer.w if x1 is None else x1
    t = haze(tones(colour), haze_c, k)
    n = count or max(3, int((x1 - x0) / 260))
    peaks = []
    for i in range(n + 2):
        x = x0 + (i - 0.5 + 0.6 * hash01(i, 1, seed + 601)) * (x1 - x0) / n
        y = top_y + (base_y - top_y) * (0.15 + 0.75 * hash01(i, 2, seed + 601))
        peaks.append((x, y))
    ridge = []
    for i, (px, py) in enumerate(peaks):
        if i:
            qx, qy = peaks[i - 1]
            mx = (qx + px) / 2 + (hash01(i, 3, seed + 601) - 0.5) * 40
            my = max(qy, py) + (base_y - max(qy, py)) * (0.35 + 0.3 * hash01(i, 4, seed + 601))
            ridge.append((mx - 18, my - 8))
            ridge.append((mx, my))
            ridge.append((mx + 22, my - 6))
        ridge.append((px, py))
    outline = [(x0 - 40, base_y + 40)] + ridge + [(x1 + 40, base_y + 40)]
    body = Path.polygon(outline)
    pc = Piece(x0 - 40, top_y - 10, x1 + 40, base_y + 50, ss=2, pad=4)
    m = pc.fill(body, t, "base")
    # facets: from each peak down its right side, the shadow
    for i, (px, py) in enumerate(peaks):
        drop = base_y + 40 - py
        rx = px + drop * (0.55 + 0.25 * hash01(i, 5, seed + 601))
        lx = px + drop * (0.08 + 0.1 * hash01(i, 6, seed + 601))
        pc.fill(Path.polygon([(px, py), (rx, base_y + 40), (lx, base_y + 40)]), t, "shadow", clip=m)
        # a ridge line down the lit face
        if hash01(i, 7, seed + 601) < 0.7:
            gx = px - drop * 0.35
            pc.fill(Path.polygon([(px - 2, py + 6), (gx, base_y + 40), (gx + 10, base_y + 40)]), t, "base-light", clip=m)
        if snow and py < top_y + (base_y - top_y) * 0.45:
            cap = drop * 0.22
            jag = [(px, py)]
            for j in range(6):
                u = j / 5.0
                jx = px - cap * 0.9 + cap * 1.8 * u
                jy = py + cap * (0.55 + 0.45 * hash01(i * 7 + j, 8, seed + 601)) * (1 - abs(u - 0.5) * 0.6)
                jag.append((jx, jy))
            snow_m = pc.shape(Path.polygon([(px, py - 1)] + jag[1:])) & m
            pc.tone(snow_m, haze(tones("#f6f2ff"), haze_c, k * 0.6), "base")
            pc.tone(snow_m & pc.shape(Path.polygon([(px, py), (rx, base_y + 40), (lx, base_y + 40)])),
                    haze(tones("#f6f2ff"), haze_c, k * 0.6), "shadow")
    pc.edge_mask(m, haze(t, haze_c, 0.0), width=1.0, which="deep-shadow")
    layer.place(pc, ink=_far_line(t, haze_c, k), reach=0.8)
    return pc


def hill(layer, points, colour, haze_c, k=0.3, seed=0, trees=0, tree_colour="leaf", base_y=None):
    """A rolling hill (a smooth line through `points`, filled down to the layer's foot), lit on
    its upper rim, with `trees` small round trees along its crest."""
    t = haze(tones(colour), haze_c, k)
    base_y = base_y or layer.h
    pts = [(points[0][0] - 20, base_y)] + list(points) + [(points[-1][0] + 20, base_y)]
    line = Path.smooth(points)
    body = Path.smooth(points)
    for q in [(points[-1][0] + 20, base_y), (points[0][0] - 20, base_y)]:
        body.line(*q)
    body.close()
    xs = [p[0] for p in pts]
    ys = [p[1] for p in points]
    pc = Piece(min(xs), min(ys) - 60, max(xs), base_y, ss=2, pad=4)
    m = pc.fill(body, t, "base")
    pc.tone(m - m.shift(-6, 10), t, "light")
    pc.tone(pc.below(m, min(ys) + (base_y - min(ys)) * 0.55), t, "terminator", alpha=0.6)
    for i in range(trees):
        u = (i + 0.5 + (hash01(i, 1, seed + 611) - 0.5) * 0.6) / trees
        p, _ = line.at(u)
        _round_tree_small(pc, p[0], p[1] + 4, 9 + 8 * hash01(i, 2, seed + 611), haze(tones(tree_colour), haze_c, k), seed + i)
    layer.place(pc, ink=_far_line(t, haze_c, k), reach=0.8)


def _round_tree_small(pc, x, y, r, t, seed):
    m = pc.shape(Path.circle(x, y - r, r)) | pc.shape(Path.circle(x - r * 0.6, y - r * 0.6, r * 0.7)) | \
        pc.shape(Path.circle(x + r * 0.6, y - r * 0.55, r * 0.75))
    pc.tone(m, t, "base")
    pc.tone(m - m.shift(-r * 0.25, -r * 0.35), t, "shadow")
    pc.tone(m - m.shift(r * 0.2, r * 0.3), t, "light")
    return m


def forest_line(layer, x0, x1, base_y, height, haze_c, k=0.35, seed=0, colour="leaf.dark"):
    """A wood seen far off: a row of round crowns, each lit top left, hazed."""
    t = haze(tones(colour), haze_c, k)
    pc = Piece(x0, base_y - height * 1.6, x1, layer.h, ss=2, pad=4)
    whole = pc.nothing()
    x = x0
    i = 0
    while x < x1 + 20:
        r = height * (0.35 + 0.3 * hash01(i, 1, seed + 621))
        y = base_y - height * (0.5 + 0.5 * hash01(i, 2, seed + 621))
        whole = whole | pc.shape(Path.circle(x, y, r))
        x += r * (1.0 + 0.4 * hash01(i, 3, seed + 621))
        i += 1
    whole = whole | pc.shape(Path.rect(x0 - 10, base_y - height * 0.5, x1 - x0 + 20, layer.h - base_y + height))
    pc.tone(whole, t, "base")
    pc.tone(whole - whole.shift(-5, -8), t, "shadow")
    pc.tone(whole - whole.shift(4, 7), t, "light")
    layer.place(pc, ink=_far_line(t, haze_c, k), reach=0.8)


def sea(layer, y0, y1, haze_c, seed=0, time="day", k=0.2, x0=0, x1=None):
    """The sea from y0 (the horizon) to y1, from x0 to x1 (the whole width by default): a soft
    gradient from hazed at the horizon to deeper below, with lines of light lying on it."""
    top = theme.mix(rgb("sea"), haze_c, 0.65)
    bot = theme.mix(rgb("sea"), haze_c, k)
    h = int(y1 - y0)
    v = np.linspace(0, 1, max(1, h))
    col = lerp_colour([(0, top), (1, bot)], v)
    x1 = layer.w if x1 is None else int(x1)
    x0 = max(0, int(x0))
    p = layer.px[int(y0):int(y0) + h, x0:x1]
    p[..., :3] = col[:, None, :] / 255.0
    p[..., 3] = 1.0
    for i in range(int((x1 - x0) * h / 2600)):
        x = x0 + hash01(i, 1, seed + 631) * (x1 - x0)
        u = hash01(i, 2, seed + 631)
        y = y0 + 3 + u * u * (h - 6)
        ln = (8 + 40 * hash01(i, 3, seed + 631)) * (0.4 + u)
        layer.soft_ellipse(x, y, ln, 0.9 + u, "#ffffff", 0.25 + 0.2 * (1 - u), hard=0.6)


def _tower(pc, x, base, w, h, wall, roof, roof_h, seed, windows=2, finial=True, k_lit=True, cone_w=None):
    """A round tower (far): its wall lit on the left and shadowed on the right, a conical roof, a
    gold finial, and a few windows, some lit."""
    body = Path.rect(x - w / 2, base - h, w, h)
    m = pc.fill(body, wall, "base")
    pc.tone(pc.where(m, lambda xx, yy: xx > x + w * 0.18), wall, "shadow")
    pc.tone(pc.where(m, lambda xx, yy: xx < x - w * 0.3), wall, "light")
    cw = cone_w or w * 1.25
    top = base - h
    cone = Path().move(x - cw / 2, top + 2).quad(x - cw * 0.22, top - roof_h * 0.45, x, top - roof_h) \
        .quad(x + cw * 0.22, top - roof_h * 0.45, x + cw / 2, top + 2).close()
    cm = pc.fill(cone, roof, "base")
    pc.tone(pc.where(cm, lambda xx, yy: xx > x + cw * 0.06), roof, "shadow")
    pc.tone(pc.where(cm, lambda xx, yy: xx < x - cw * 0.2), roof, "light")
    pc.stroke(Path.polyline([(x - cw / 2, top + 2), (x + cw / 2, top + 2)]), 1.6, roof, "deep")
    if finial:
        g = tones("gold")
        pc.fill(Path.rect(x - 0.9, top - roof_h - 9, 1.8, 10), g, "base")
        pc.fill(Path.circle(x, top - roof_h - 3, 2.0), g, "light")
    for j in range(windows):
        wy = base - h * (0.3 + 0.45 * j / max(1, windows - 1)) if windows > 1 else base - h * 0.55
        lit = hash01(j, 1, seed + 641) < 0.45
        win = Path().move(x - 2.4, wy + 5).line(x - 2.4, wy).arc(x, wy, 2.4, 180, 360).line(x + 2.4, wy + 5).close()
        pc.fill(win, tones("glass.lit") if lit else tones("glass"), "base" if lit else "shadow")
    return m | cm


def castle(layer, cx, base_y, scale, haze_c, k=0.2, seed=0, glow=True):
    """The Sunspire and its keep (or any castle, smaller): a central great tower with a tall blue
    spire and the crystal on top, ranks of turrets round it with conical roofs, curtain walls with
    battlements, on a green rock. Saturated more than the rest of the far layer: it's where the
    eye goes. `scale` 1 is about 520 pixels tall."""
    s = scale
    wall = haze(tones("stone"), haze_c, k)
    wall_dim = haze(tones("stone"), haze_c, k + 0.12)
    roof = haze(tones("roof.blue"), haze_c, k * 0.8)
    roof_dim = haze(tones("roof.blue"), haze_c, k + 0.12)
    rock = haze(tones("leaf.blue"), haze_c, k + 0.1)
    cliff = haze(tones("stone.dark"), haze_c, k + 0.05)
    pc = Piece(cx - 330 * s, base_y - 560 * s, cx + 330 * s, base_y + 30, ss=3, pad=6)
    # the rock it stands on
    rock_p = Path.smooth([(cx - 330 * s, base_y + 30), (cx - 260 * s, base_y - 40 * s), (cx - 170 * s, base_y - 92 * s),
                          (cx - 60 * s, base_y - 112 * s), (cx + 70 * s, base_y - 108 * s), (cx + 190 * s, base_y - 80 * s),
                          (cx + 280 * s, base_y - 30 * s), (cx + 330 * s, base_y + 30)])
    rock_p.close()
    rm = pc.fill(rock_p, cliff, "base")
    pc.tone(pc.where(rm, lambda xx, yy: xx > cx + 40 * s), cliff, "shadow")
    for i in range(7):
        x = cx + (-250 + 80 * i + 20 * hash01(i, 1, seed + 651)) * s
        pc.stroke(Path.polyline([(x, base_y - 60 * s), (x + 8 * s, base_y + 10)]), (1.6, 0.4), cliff, "deep-shadow", clip=rm)
    green = pc.shape(Path.smooth([(cx - 300 * s, base_y - 10 * s), (cx - 200 * s, base_y - 78 * s), (cx - 60 * s, base_y - 104 * s),
                                   (cx + 80 * s, base_y - 100 * s), (cx + 210 * s, base_y - 66 * s), (cx + 300 * s, base_y - 8 * s),
                                   (cx + 200 * s, base_y - 40 * s), (cx, base_y - 60 * s), (cx - 200 * s, base_y - 36 * s)], closed=True)) & rm
    pc.tone(green, rock, "base")
    pc.tone(green - green.shift(-4, -6), rock, "shadow")
    # curtain wall with battlements, in two tiers
    for tier, (wy, ww, wh) in enumerate(((base_y - 100 * s, 470 * s, 70 * s), (base_y - 170 * s, 300 * s, 80 * s))):
        t = wall_dim if tier == 0 else wall
        body = Path.rect(cx - ww / 2, wy - wh, ww, wh)
        bm = pc.fill(body, t, "base")
        n = int(ww / (14 * s))
        for j in range(n):
            if j % 2 == 0:
                pc.fill(Path.rect(cx - ww / 2 + j * ww / n, wy - wh - 8 * s, ww / n, 8 * s + 1), t, "base")
        pc.tone(pc.below(bm, wy - wh * 0.18), t, "shadow")
        for j in range(5 + tier * 2):
            ax = cx - ww / 2 + ww * (j + 0.5) / (5 + tier * 2)
            arch = Path().move(ax - 7 * s, wy - 4 * s).line(ax - 7 * s, wy - 22 * s).arc(ax, wy - 22 * s, 7 * s, 180, 360) \
                .line(ax + 7 * s, wy - 4 * s).close()
            lit = hash01(j, tier, seed + 653) < 0.35
            pc.fill(arch, tones("glass.lit") if lit else t, "base" if lit else "deep")
        pc.stroke(Path.polyline([(cx - ww / 2, wy - wh), (cx + ww / 2, wy - wh)]), 1.4, t, "light")
    # turrets, back to front, then the great tower
    spots = [(-200, 150, 34, 150, 70, wall_dim, roof_dim), (190, 160, 34, 140, 66, wall_dim, roof_dim),
             (-120, 200, 40, 190, 90, wall, roof), (118, 196, 40, 178, 84, wall, roof),
             (-235, 96, 30, 110, 56, wall_dim, roof_dim), (240, 98, 30, 104, 54, wall_dim, roof_dim),
             (-58, 250, 48, 230, 104, wall, roof), (64, 246, 46, 214, 100, wall, roof)]
    for i, (dx, up, w, h, rh, wt, rt) in enumerate(spots):
        _tower(pc, cx + dx * s, base_y - (up - 40) * s, w * s, h * s * 0.6, wt, rt, rh * s, seed + i, windows=2)
    # the great tower and the spire
    gt_base = base_y - 160 * s
    gm = _tower(pc, cx, gt_base, 66 * s, 250 * s, wall, roof, 170 * s, seed + 99, windows=3, finial=False, cone_w=86 * s)
    # balconies and gold bands on it
    g = haze(tones("gold"), haze_c, k * 0.5)
    for yy in (gt_base - 120 * s, gt_base - 200 * s):
        pc.fill(Path.rect(cx - 38 * s, yy - 4 * s, 76 * s, 7 * s), wall, "light")
        pc.stroke(Path.polyline([(cx - 38 * s, yy + 3 * s), (cx + 38 * s, yy + 3 * s)]), 1.2, g, "base")
    tip = gt_base - 250 * s - 170 * s
    pc.fill(Path.rect(cx - 1.4 * s, tip - 18 * s, 2.8 * s, 20 * s), g, "base")
    crystal = Path.polygon([(cx, tip - 46 * s), (cx + 9 * s, tip - 28 * s), (cx, tip - 14 * s), (cx - 9 * s, tip - 28 * s)])
    ct = tones("#7fd0ff")
    cm = pc.fill(crystal, ct, "base")
    pc.tone(pc.where(cm, lambda xx, yy: xx > cx), ct, "shadow")
    pc.fill(Path.polygon([(cx - 1, tip - 42 * s), (cx - 6 * s, tip - 28 * s), (cx - 1, tip - 20 * s)]), ct, "highlight")
    if glow:
        pc.glow(cx, tip - 30 * s, 70 * s, "#9fe0ff", 0.55)
        pc.glow(cx, tip - 30 * s, 22 * s, "#e8f8ff", 0.6)
    # banners on the great tower
    for side in (-1, 1):
        bx = cx + side * 20 * s
        bp = Path.polygon([(bx - 6 * s, gt_base - 190 * s), (bx + 6 * s, gt_base - 190 * s), (bx + 6 * s, gt_base - 140 * s),
                           (bx, gt_base - 132 * s), (bx - 6 * s, gt_base - 140 * s)])
        pc.fill(bp, haze(tones("cloth.pink"), haze_c, k * 0.6), "base")
    layer.place(pc, ink=_far_line(wall, haze_c, k * 0.5), reach=1.0)
    return pc


def skyline(layer, x0, x1, base_y, haze_c, k=0.4, seed=0, height=(60, 150), roofs=("roof.blue", "roof.pink", "roof.teal", "roof.blue"),
            spires=0.18, lit=0.12, walls=("stone", "plaster", "stone.warm", "plaster.pink")):
    """A row of rooftops seen across the city: houses of varying width and height with gabled,
    hipped and conical roofs, now and then a tower with a spire, a chimney, a few windows lit. Each
    has a narrow side wall on its right in shadow, so the row has depth; all hazed by k."""
    pc = Piece(x0, base_y - height[1] - 140, x1, layer.h, ss=2, pad=4)
    x = x0 - 20
    i = 0
    while x < x1 + 20:
        w = 34 + 60 * hash01(i, 1, seed + 661)
        h = height[0] + (height[1] - height[0]) * hash01(i, 2, seed + 661)
        wall = haze(tones(walls[int(hash01(i, 3, seed + 661) * len(walls))]), haze_c, k)
        roof = haze(tones(roofs[int(hash01(i, 4, seed + 661) * len(roofs))]), haze_c, k)
        top = base_y - h
        if hash01(i, 5, seed + 661) < spires:
            _tower(pc, x + w * 0.35, base_y, w * 0.45, h + 40, wall, roof, 50 + 30 * hash01(i, 6, seed + 661), seed + i)
        else:
            body = Path.rect(x, top, w, h + (layer.h - base_y))
            m = pc.fill(body, wall, "base")
            side = w * 0.18
            pc.fill(Path.rect(x + w - side, top, side, h + (layer.h - base_y)), wall, "shadow")
            kind = hash01(i, 7, seed + 661)
            rh = w * (0.45 + 0.35 * hash01(i, 8, seed + 661))
            if kind < 0.45:
                rp = Path.polygon([(x - 4, top + 2), (x + w * 0.5, top - rh), (x + w + 4, top + 2)])
            elif kind < 0.8:
                rp = Path.polygon([(x - 4, top + 2), (x + w * 0.22, top - rh * 0.8), (x + w * 0.78, top - rh * 0.8), (x + w + 4, top + 2)])
            else:
                rp = Path.polygon([(x - 3, top + 2), (x + w * 0.5, top - rh * 1.5), (x + w + 3, top + 2)])
            rm = pc.fill(rp, roof, "base")
            pc.tone(pc.where(rm, lambda xx, yy, a=x + w * 0.62: xx > a), roof, "shadow")
            if hash01(i, 9, seed + 661) < 0.4:
                cx_ = x + w * (0.2 + 0.5 * hash01(i, 10, seed + 661))
                pc.fill(Path.rect(cx_, top - rh * 0.7, 6, rh * 0.5), wall, "terminator")
            rows = int(h / 26)
            for r in range(rows):
                for c in range(max(1, int(w / 22))):
                    wx = x + 7 + c * 20
                    wy = top + 12 + r * 26
                    if wx + 6 > x + w - side:
                        continue
                    on = hash01(i * 31 + c, r, seed + 663) < lit
                    pc.fill(Path.rect(wx, wy, 5, 8, r=2), tones("glass.lit") if on else wall, "base" if on else "shadow")
                    if on:
                        pc.glow(wx + 2.5, wy + 4, 14, "#ffd890", 0.18)
        x += w * (0.85 + 0.2 * hash01(i, 11, seed + 661))
        i += 1
    layer.place(pc, ink=_far_line(tones("stone"), haze_c, k), reach=0.7)
    return pc


# --------------------------------------------------------------------------- #
# Buildings from parts
# --------------------------------------------------------------------------- #

class Door:
    """A building's door: its middle at cx (the zone file's door x), `w` x `h` pixels of doorway,
    `kind` arch (rounded top), square, double (two arched leaves) or open (a forge's, standing open),
    in `wood`, with a `surround` of stone voussoirs or a timber frame, and a step."""

    def __init__(self, cx, w=72, h=132, kind="arch", wood="wood", surround="stone", step=True, studs=True, lamp=None):
        self.cx, self.w, self.h, self.kind, self.wood = cx, w, h, kind, wood
        self.surround, self.step, self.studs, self.lamp = surround, step, studs, lamp


class Window:
    """A window: its middle at cx, its top at `top` (from the floor's top if `floor` is given
    instead), `w` x `h`, `kind` arch, round, square or shop, `lit` (warm glass) or not, with
    optional `shutters` (a colour), a flower `box`, and `panes` across."""

    def __init__(self, cx, top=None, w=36, h=52, kind="arch", lit=False, shutters=None, box=False, panes=2, floor=None, sill=True):
        self.cx, self.top, self.w, self.h, self.kind, self.lit = cx, top, w, h, kind, lit
        self.shutters, self.box, self.panes, self.floor, self.sill = shutters, box, panes, floor, sill


class House:
    """What a building's made of, ground floor first.

    x0, width   its front, in the layer's pixels; ground is where it stands (the floor's back edge)
    floors      ((material, height), ...): material stone, plaster, timber or brick, optionally
                with a colour after a dot ("plaster.pink", "stone.warm")
    roof        hip, gable (the ridge along the street), front (a gable end to the street) or cone,
                in roof_colour, roof_h tall, overhanging by `eave`
    door        a Door; windows: Windows; dormers: x positions of dormer windows on the roof
    chimneys    ((x, width, height), ...), standing behind the roof
    extras      callables f(piece, house) run last (signs, awnings, banners, lanterns)
    jetty       the upper floors stand out this many pixels over the ground floor, on a beam
    plinth      the height of the darker stone course along the foot
    """

    def __init__(self, x0, width, ground, floors=(("stone", 180),), roof="hip", roof_colour="roof.blue", roof_h=None,
                 eave=14, door=None, windows=(), dormers=(), chimneys=(), extras=(), jetty=0, plinth=14, seed=0,
                 finials=False, quoins=True, gable_window=True, vines=()):
        self.x0, self.width, self.ground = x0, width, ground
        self.floors, self.roof, self.roof_colour = floors, roof, roof_colour
        self.roof_h = roof_h if roof_h is not None else width * (0.55 if roof != "front" else 0.62)
        self.eave, self.door, self.windows, self.dormers = eave, door, windows, dormers
        self.chimneys, self.extras, self.jetty, self.plinth, self.seed = chimneys, extras, jetty, plinth, seed
        self.finials, self.quoins, self.gable_window, self.vines = finials, quoins, gable_window, vines

    @property
    def x1(self):
        return self.x0 + self.width

    def floor_box(self, i):
        """Floor i's (x0, top, x1, bottom), the jetty taken into account."""
        y = self.ground
        for j, (_, h) in enumerate(self.floors):
            top = y - h
            if j == i:
                out = self.jetty if j > 0 else 0
                return self.x0 - out, top, self.x1 + out, y
            y = top
        raise IndexError(i)

    @property
    def wall_top(self):
        return self.ground - sum(h for _, h in self.floors)


def _material(spec):
    kind, _, colour = spec.partition(".")
    default = {"stone": "stone", "plaster": "plaster", "timber": "plaster", "brick": "brick"}[kind]
    return kind, (kind + "." + colour if colour and (kind + "." + colour) in PALETTE else (colour if colour in PALETTE else default))


def wall(p, x0, y0, x1, y1, spec, seed=0, quoins=True, beams="wood.dark"):
    """A wall from (x0, y0) to (x1, y1) in a material: `stone` (ashlar courses, staggered joints,
    a few blocks a shade off, quoins at the corners), `plaster` (smooth, a patch or two fallen
    away to the stones under it, a hairline crack, damp along the foot), `timber` (plaster infill
    between posts, rails and braces, each beam lit on its top and left and casting a thin shadow
    right and down) or `brick`. Returns the wall's Mask."""
    kind, colour = _material(spec)
    t = tones(colour)
    rect = Path.rect(x0, y0, x1 - x0, y1 - y0)
    m = p.fill(rect, t, "base")
    w, h = x1 - x0, y1 - y0
    if kind in ("stone", "brick"):
        ch = 22 if kind == "stone" else 9
        bw = 46 if kind == "stone" else 22
        rows = int(math.ceil(h / ch))
        joints = Path()
        for r in range(rows):
            y = y1 - (r + 1) * ch
            if y > y0:
                joints.add(Path.polyline([(x0, y), (x1, y)]))
            off = (bw / 2) * (r % 2)
            x = x0 - off
            c = 0
            while x < x1:
                bwj = bw * (0.75 + 0.5 * hash01(c, r, seed + 701)) if kind == "stone" else bw
                nx = x + bwj
                if x0 < nx < x1:
                    joints.add(Path.polyline([(nx, max(y0, y)), (nx, min(y1, y + ch))]))
                tint = hash01(c, r, seed + 703)
                if tint < 0.16 or tint > 0.9:
                    blk = Path.rect(max(x0, x) + 1, max(y0, y) + 1, min(x1, nx) - max(x0, x) - 2, min(y1, y + ch) - max(y0, y) - 2)
                    if min(x1, nx) - max(x0, x) > 4 and min(y1, y + ch) - max(y0, y) > 4:
                        p.fill(blk, t, "base-light" if tint > 0.9 else "terminator", alpha=0.75)
                x = nx
                c += 1
        p.stroke(joints, DETAIL * (1.1 if kind == "stone" else 0.9), t, "shadow", clip=m, alpha=0.9)
        if kind == "stone":
            # a lit lip along the top of every course, below its joint
            lips = Path()
            for r in range(rows):
                y = y1 - (r + 1) * ch + 1.6
                if y > y0 + 2:
                    lips.add(Path.polyline([(x0, y), (x1, y)]))
            p.stroke(lips, 1.0, t, "light", clip=m, alpha=0.6)
    elif kind == "plaster" or kind == "timber":
        n = (1 if hash01(1, 1, seed + 711) < 0.45 else 0) if kind == "plaster" else 0
        for i in range(n):
            px = x0 + w * (0.15 + 0.7 * hash01(i, 2, seed + 711))
            py = y0 + h * (0.2 + 0.6 * hash01(i, 3, seed + 711))
            pr = 12 + 10 * hash01(i, 4, seed + 711)
            patch = Path.smooth([(px - pr, py), (px - pr * 0.4, py - pr * 0.6), (px + pr * 0.5, py - pr * 0.5),
                                 (px + pr, py + pr * 0.1), (px + pr * 0.3, py + pr * 0.6), (px - pr * 0.6, py + pr * 0.5)], closed=True)
            pm = p.shape(patch) & m
            st = tones("stone.warm")
            p.tone(pm, st, "terminator")
            for j in range(6):
                sx = px - pr * 0.9 + (j % 3) * pr * 0.62 + (j // 3) * pr * 0.3
                sy = py - pr * 0.42 + (j // 3) * pr * 0.46
                b = p.shape(Path.rect(sx, sy, pr * 0.56, pr * 0.4, r=1.5)) & pm
                p.tone(b, st, "base")
                p.tone(b - b.shift(-1.2, -1.2), st, "shadow")
                p.tone(b - b.shift(1.0, 1.0), st, "light")
            p.edge_mask(pm, t, 1.1, "shadow")
        if kind == "plaster":
            cx = x0 + w * (0.2 + 0.6 * hash01(5, 5, seed + 711))
            crack = Path.polyline([(cx, y0 + h * 0.15), (cx + 4, y0 + h * 0.22), (cx + 1, y0 + h * 0.3), (cx + 6, y0 + h * 0.36)])
            p.stroke(crack, (1.0, 0.0), t, "deep-shadow", clip=m)
        # damp along the foot
        damp = Path().move(x0, y1)
        for i in range(9):
            damp.line(x0 + w * i / 8, y1 - 10 - 7 * hash01(i, 6, seed + 711))
        damp.line(x1, y1).close()
        p.fill(damp, t, "terminator", clip=m, alpha=0.55)
    if kind == "timber":
        b = tones(beams)
        bw = 9
        posts = [x0, x1 - bw]
        n = max(1, int(round(w / 74)))
        for i in range(1, n):
            posts.append(x0 + w * i / n - bw / 2)
        rails = [y0, y1 - bw, (y0 + y1) / 2 - bw / 2]
        frame = p.nothing()
        for x in posts:
            frame = frame | p.shape(Path.rect(x, y0, bw, h))
        for y in rails:
            frame = frame | p.shape(Path.rect(x0, y, w, bw))
        xs = sorted(posts)
        for i in range(len(xs) - 1):
            a, bb = xs[i] + bw, xs[i + 1]
            if bb - a < 24:
                continue
            for half in (0, 1):
                top = y0 + bw if half == 0 else (y0 + y1) / 2 + bw / 2
                bot = (y0 + y1) / 2 - bw / 2 if half == 0 else y1 - bw
                if (i + half) % 2 == 0:
                    brace = Path.polyline([(a, bot), (bb, top)])
                else:
                    brace = Path.polyline([(a, top), (bb, bot)])
                frame = frame | (p.line_shape(brace, bw * 0.85) & p.shape(Path.rect(a, top, bb - a, bot - top)))
        frame = frame & m
        p.cast(frame, 2.2, 2.6, m, t, "shadow")
        p.tone(frame, b, "base")
        p.tone(frame - frame.shift(1.6, 1.6), b, "light")
        p.tone(frame - frame.shift(-1.4, -1.4), b, "shadow")
        p.edge_mask(frame, b, 1.0)
        for x in posts:
            for y in rails:
                if hash01(int(x), int(y), seed + 713) < 0.5:
                    p.fill(Path.circle(x + bw / 2, y + bw / 2, 1.2), b, "deep")
    if quoins and kind in ("stone", "plaster"):
        q = tones("stone" if kind == "plaster" else colour)
        qm = p.nothing()
        y = y1
        r = 0
        while y > y0 + 4:
            hgt = 20
            for side in (0, 1):
                long_ = (r + side) % 2 == 0
                qw = 26 if long_ else 15
                x = x0 if side == 0 else x1 - qw
                blk = Path.rect(x, max(y0, y - hgt), qw, min(hgt, y - y0) - 1.5)
                qm = qm | p.shape(blk)
            y -= hgt
            r += 1
        qm = qm & m
        p.tone(qm, q, "base-light")
        p.tone(qm - qm.shift(-1.4, -1.4), q, "terminator")
        p.edge_mask(qm, q, 1.0, "shadow")
    return m


def _roof_tiles(p, rm, t, x0, x1, y_top, y_bot, seed, row=15.0, tile=19.0, curve=None, cx=None):
    """Rows of round-ended tiles over a roof region: each row's scalloped lower edge in the deep
    tone, the row above's overlap shadow under it, and a lit lip along each tile's top. `curve`
    (for a cone) bends the rows into arcs about cx."""
    n = int((y_bot - y_top) / row) + 1
    lines, shadows, lips = Path(), Path(), Path()
    for r in range(n):
        y = y_bot - r * row
        off = (tile / 2) * (r % 2)
        pts = []
        x = x0 - tile - off
        while x <= x1 + tile:
            for k in range(5):
                a = math.pi * k / 4
                px = x + tile / 2 - math.cos(a) * tile / 2
                py = y + math.sin(a) * tile * 0.30
                if curve:
                    u = max(-1.0, min(1.0, (px - cx) / max(1.0, (x1 - x0) / 2)))
                    py += curve * (1 - u * u)
                pts.append((px, py))
            x += tile
        line = Path.polyline(pts)
        lines.add(line)
        shadows.add(line.translate(0, 2.6))
        lips.add(line.translate(0.6, -row + 2.6))
    p.stroke(shadows, 3.4, t, "shadow", clip=rm, alpha=0.8)
    p.stroke(lips, 1.3, t, "light", clip=rm, alpha=0.75)
    p.stroke(lines, DETAIL * 1.15, t, "deep", clip=rm)


def roof(p, house):
    """A house's roof over its top floor, by kind: hip (a trapezoid flaring at the eave, its left
    end a lit sliver and its right in shadow), gable (the same, steeper ends), front (the gable end
    to the street: two sloping tiled verges over a triangle of wall with a round window), cone (a
    turret's). Tiled in rows; a gold finial on the peaks if the house has finials. Returns the
    roof's Mask and its silhouette path."""
    t = tones(house.roof_colour)
    x0, x1 = house.x0 - house.eave - house.jetty, house.x1 + house.eave + house.jetty
    ye = house.wall_top + 6
    rh = house.roof_h
    ridge = ye - rh
    kind = house.roof
    g = tones("gold")
    if kind in ("hip", "gable"):
        inset = rh * (0.42 if kind == "hip" else 0.16)
        flare = 6
        path = Path().move(x0 - flare, ye).quad(x0 + inset * 0.25, ye - rh * 0.2, x0 + inset, ridge) \
            .line(x1 - inset, ridge).quad(x1 - inset * 0.25, ye - rh * 0.2, x1 + flare, ye).close()
        rm = p.fill(path, t, "base")
        _roof_tiles(p, rm, t, x0, x1, ridge, ye - 4, house.seed)
        # the hip ends: lit on the left, shadow on the right
        left = Path.polygon([(x0 - flare, ye), (x0 + inset, ridge), (x0 + inset + rh * 0.12, ridge), (x0 + rh * 0.06, ye)])
        right = Path.polygon([(x1 + flare, ye), (x1 - inset, ridge), (x1 - inset - rh * 0.14, ridge), (x1 - rh * 0.09, ye)])
        p.tone(p.shape(left) & rm, t, "light", alpha=0.85)
        p.tone(p.shape(right) & rm, t, "shadow", alpha=0.95)
        p.stroke(Path.polyline([(x0 + inset + rh * 0.12, ridge + 1), (x0 + rh * 0.06, ye - 1)]), 1.2, t, "deep", clip=rm)
        p.stroke(Path.polyline([(x1 - inset - rh * 0.14, ridge + 1), (x1 - rh * 0.09, ye - 1)]), 1.2, t, "deep", clip=rm)
        # the ridge cap
        cap = Path.rect(x0 + inset - 3, ridge - 4, (x1 - inset) - (x0 + inset) + 6, 7, r=3)
        cm = p.fill(cap, t, "terminator")
        p.tone(cm - cm.shift(0, 2), t, "light")
        p.edge(cap, t, 1.1)
        if house.finials:
            for fx in (x0 + inset, x1 - inset):
                _finial(p, fx, ridge - 3, g)
        p.edge(path, t)
        return rm, path
    if kind == "front":
        cx = (house.x0 + house.x1) / 2
        apex = ridge
        verge = 20
        half = (x1 - x0) / 2
        # the gable wall
        wall_mat, wall_col = _material(house.floors[-1][0])
        gw = Path.polygon([(house.x0 - house.jetty, ye - 4), (cx, apex + verge * 0.9), (house.x1 + house.jetty, ye - 4)])
        gm = wall(p, house.x0 - house.jetty, apex, house.x1 + house.jetty, ye - 4, house.floors[-1][0], house.seed + 5, quoins=False)
        gm_clip = p.shape(gw)
        p.c.erase((gm - gm_clip).a)
        if house.gable_window:
            r = min(22, half * 0.28)
            wy = (apex + ye) / 2 + r * 0.4
            _round_window(p, cx, wy, r, house.seed)
        # the two verges, flaring out at the eave
        lv = Path.polygon([(x0 - 6, ye + 2), (cx, apex - 4), (cx, apex + verge), (x0 + verge * 0.9, ye + 2)])
        rv = Path.polygon([(x1 + 6, ye + 2), (cx, apex - 4), (cx, apex + verge), (x1 - verge * 0.9, ye + 2)])
        lm = p.fill(lv, t, "base")
        rmk = p.fill(rv, t, "shadow")
        for m_, side in ((lm, -1), (rmk, 1)):
            for i in range(int(half / 13)):
                u = (i + 0.5) / int(half / 13)
                ax = cx + side * u * half
                ay = apex + (ye - apex) * u
                p.stroke(Path.polyline([(ax, ay - 3), (ax + side * verge * 0.2, ay + verge * 0.8)]), 1.0, t, "deep", clip=m_)
        p.tone(lm - lm.shift(1.5, 1.5), t, "light")
        p.edge(lv, t)
        p.edge(rv, t)
        # the shadow the verges throw on the gable wall
        p.cast(lm | rmk, 3, 5, p.shape(gw), tones(wall_col), "shadow")
        if house.finials:
            _finial(p, cx, apex - 4, g)
        path = Path.polygon([(x0 - 6, ye + 2), (cx, apex - 4), (x1 + 6, ye + 2)])
        return lm | rmk | gm_clip, path
    if kind == "cone":
        cx = (house.x0 + house.x1) / 2
        w = x1 - x0 + 8
        path = Path().move(cx - w / 2, ye).quad(cx - w * 0.2, ye - rh * 0.5, cx, ridge).quad(cx + w * 0.2, ye - rh * 0.5, cx + w / 2, ye).close()
        rm = p.fill(path, t, "base")
        _roof_tiles(p, rm, t, cx - w / 2, cx + w / 2, ridge, ye - 3, house.seed, row=12, tile=13, curve=6, cx=cx)
        p.tone(p.where(rm, lambda xx, yy: xx > cx + w * 0.08), t, "shadow", alpha=0.9)
        p.tone(p.where(rm, lambda xx, yy: xx < cx - w * 0.28), t, "light", alpha=0.6)
        p.edge(path, t)
        _finial(p, cx, ridge, g, tall=True)
        return rm, path
    raise ValueError("no roof kind " + kind)


def _finial(p, x, y, g, tall=False):
    h = 26 if tall else 16
    p.fill(Path.rect(x - 1.6, y - h, 3.2, h + 2), g, "base")
    p.fill(Path.circle(x, y - h * 0.45, 3.4), g, "base")
    p.fill(Path.polygon([(x, y - h - 10), (x + 3.4, y - h), (x - 3.4, y - h)]), g, "light")
    p.fill(Path.circle(x - 1.0, y - h * 0.45 - 1.0, 1.2), g, "highlight")


def _round_window(p, cx, cy, r, seed, lit=False):
    st = tones("stone")
    ring = p.fill(Path.circle(cx, cy, r + 5), st, "base-light")
    p.edge(Path.circle(cx, cy, r + 5), st, 1.2, "shadow")
    gt = tones("glass.lit") if lit else tones("glass")
    gm = p.fill(Path.circle(cx, cy, r), gt, "base")
    p.tone(gm - gm.shift(3, 3), gt, "deep")
    if not lit:
        p.stroke(Path.polyline([(cx - r * 0.5, cy + r * 0.1), (cx + r * 0.1, cy - r * 0.55)]), 2.2, gt, "light", clip=gm)
    wt = tones("wood.dark")
    for a in range(0, 180, 45):
        dx, dy = math.cos(math.radians(a)) * r, math.sin(math.radians(a)) * r
        p.stroke(Path.polyline([(cx - dx, cy - dy), (cx + dx, cy + dy)]), 2.0, wt, "base", clip=gm)
    p.edge(Path.circle(cx, cy, r), wt, 1.6, "base")
    return ring


def _opening(kind, x, y, w, h):
    """The outline of an opening: arch (round top), round, square or shop, top left at (x, y)."""
    if kind in ("arch", "double"):
        r = w / 2
        return Path().move(x, y + h).line(x, y + r).arc(x + r, y + r, r, 180, 360).line(x + w, y + h).close()
    if kind == "flat-arch":
        return Path().move(x, y + h).line(x, y + 8).quad(x + w / 2, y - 6, x + w, y + 8).line(x + w, y + h).close()
    return Path.rect(x, y, w, h)


def window(p, wnd, wall_t, seed=0):
    """A window into a wall: a stone surround (a keystone on an arch), the recess in shadow along
    its top and left, the glass (warm and glowing when lit, else a cool blue with a streak of sky
    in it), glazing bars, a sill throwing its shadow down, and shutters and a flower box if asked."""
    x = wnd.cx - wnd.w / 2
    y = wnd.top
    w, h = wnd.w, wnd.h
    kind = wnd.kind
    st = tones("stone")
    if wnd.shutters:
        sh = tones(wnd.shutters)
        for side in (-1, 1):
            sx = x - w * 0.55 - 4 if side < 0 else x + w + 4
            sp = Path.rect(sx, y + (w / 2 if kind == "arch" else 0) - 2, w * 0.55, h - (w / 2 if kind == "arch" else 0) + 4)
            sm = p.fill(sp, sh, "base")
            for k in range(1, 3):
                p.stroke(Path.polyline([(sx + 2, y + h * k / 3 + 4), (sx + w * 0.55 - 2, y + h * k / 3 + 4)]), 1.0, sh, "deep", clip=sm)
            p.tone(sm - sm.shift(1.5, 1.5), sh, "light")
            p.cast(sm, 2.5, 3, p.everything(), wall_t, "shadow")
            p.edge(sp, sh, 1.2)
    sur = _opening(kind, x - 6, y - 6, w + 12, h + 8)
    sm = p.fill(sur, st, "base-light")
    p.cast(sm, 2.5, 3.0, p.everything() - sm, wall_t, "shadow")
    if kind == "arch":
        p.fill(Path.polygon([(x + w / 2 - 5, y - 9), (x + w / 2 + 5, y - 9), (x + w / 2 + 3.5, y + 5), (x + w / 2 - 3.5, y + 5)]), st, "base")
        p.edge(Path.polygon([(x + w / 2 - 5, y - 9), (x + w / 2 + 5, y - 9), (x + w / 2 + 3.5, y + 5), (x + w / 2 - 3.5, y + 5)]), st, 1.0, "shadow")
    p.edge(sur, st, 1.2, "terminator")
    op = _opening(kind, x, y, w, h)
    gt = tones("glass.lit") if wnd.lit else tones("glass")
    gm = p.fill(op, gt, "base")
    if wnd.lit:
        p.tone(p.below(gm, y + h * 0.55), gt, "light")
        p.glow(x + w / 2, y + h * 0.55, max(w, h) * 1.1, "#ffcf7a", 0.22)
    else:
        streak = Path.polygon([(x + w * 0.15, y + h * 0.75), (x + w * 0.45, y + h * 0.18), (x + w * 0.62, y + h * 0.18), (x + w * 0.32, y + h * 0.75)])
        p.fill(streak, gt, "light", clip=gm, alpha=0.8)
        p.fill(streak.translate(w * 0.32, 0), gt, "base-light", clip=gm, alpha=0.6)
    p.tone(gm - gm.shift(4, 4.5), gt, "deep" if not wnd.lit else "terminator")
    wt = tones("wood.dark") if not wnd.lit else tones("wood")
    bars = Path()
    for k in range(1, wnd.panes):
        bx = x + w * k / wnd.panes
        bars.add(Path.polyline([(bx, y), (bx, y + h)]))
    if h > 30:
        bars.add(Path.polyline([(x, y + h * (0.55 if kind == "arch" else 0.5)), (x + w, y + h * (0.55 if kind == "arch" else 0.5))]))
    p.stroke(bars, 2.4, wt, "base", clip=gm)
    p.edge(op, wt, 2.0, "base")
    p.edge(op, wt, 0.9, "line")
    if wnd.sill:
        sill = Path.rect(x - 9, y + h + 1, w + 18, 6, r=1.5)
        slm = p.fill(sill, st, "light")
        p.tone(p.below(slm, y + h + 5), st, "terminator")
        p.cast(slm, 0, 5, p.everything() - slm - sm, wall_t, "shadow")
        p.edge(sill, st, 1.0, "shadow")
    if wnd.box:
        flower_box(p, x + w / 2, y + h + 7, w + 14, seed)


def flower_box(p, cx, top, w, seed, colours=("flower.pink", "flower.white", "flower.yellow", "flower.violet")):
    """A wooden box of flowers under a window, with leaves and a trail of ivy hanging over it."""
    wt = tones("wood")
    box = Path.rect(cx - w / 2, top, w, 13, r=1.5)
    leaf = tones("leaf")
    # trailing ivy first, so the box sits over it
    for i in range(3):
        sx = cx - w / 2 + 6 + (w - 12) * hash01(i, 1, seed + 731)
        ln = 18 + 22 * hash01(i, 2, seed + 731)
        pts = [(sx + 3 * math.sin(k * 1.3 + i), top + 8 + ln * k / 5) for k in range(6)]
        p.stroke(Path.smooth(pts), 1.2, leaf, "shadow")
        for k in range(1, 6):
            lx, ly = pts[k]
            lm = p.fill(Path.ellipse(lx + (2.5 if k % 2 else -2.5), ly, 3.4, 2.2, rot=30 if k % 2 else -30), leaf, "base")
            p.tone(lm - lm.shift(-0.8, -1), leaf, "shadow")
    bm = p.fill(box, wt, "base")
    p.stroke(Path.polyline([(cx - w / 2, top + 6.5), (cx + w / 2, top + 6.5)]), 1.0, wt, "deep", clip=bm)
    p.tone(bm - bm.shift(0, 1.6), wt, "light")
    p.edge(box, wt, 1.2)
    # the leaves and flowers heaped on top
    clump = p.nothing()
    n = int(w / 7)
    for i in range(n):
        lx = cx - w / 2 + 3 + (w - 6) * (i + 0.5) / n
        clump = clump | p.shape(Path.circle(lx, top - 2 - 3 * hash01(i, 3, seed + 731), 5 + 2 * hash01(i, 4, seed + 731)))
    clump = clump - p.shape(Path.rect(cx - w, top + 2, w * 2, 40))
    p.tone(clump, leaf, "base")
    p.tone(clump - clump.shift(-1.5, -2.2), leaf, "shadow")
    p.edge_mask(clump, leaf, 1.0)
    flowers(p, cx - w / 2 + 4, cx + w / 2 - 4, top - 4, seed, colours, size=2.8, rows=1)


def flowers(p, x0, x1, y, seed, colours=("flower.pink", "flower.white", "flower.yellow"), size=3.0, rows=1, spread=6):
    """Little five-petalled flowers along a line, in clusters, each with a darker heart."""
    n = int((x1 - x0) / (size * 2.4)) * rows
    for i in range(n):
        fx = x0 + (x1 - x0) * hash01(i, 1, seed + 741)
        fy = y + spread * (hash01(i, 2, seed + 741) - 0.5) * rows
        c = tones(colours[int(hash01(i, 3, seed + 741) * len(colours))])
        r = size * (0.8 + 0.4 * hash01(i, 4, seed + 741))
        petals = Path()
        for k in range(5):
            a = k * 72 + 90 * hash01(i, 5, seed + 741)
            petals.add(Path.circle(fx + math.cos(math.radians(a)) * r * 0.55, fy + math.sin(math.radians(a)) * r * 0.55, r * 0.5))
        fm = p.fill(petals, c, "base", rule="nonzero")
        p.tone(fm - fm.shift(-0.6, -0.6), c, "shadow", alpha=0.8)
        p.fill(Path.circle(fx, fy, r * 0.28), tones("flower.yellow") if colours[0] != "flower.yellow" else tones("wood"), "deep-shadow")


def door(p, d, ground, wall_t, seed=0):
    """A door in a wall, standing on the ground: the step, the stone voussoirs or timber frame round
    it, the leaf (or two) of vertical planks with iron straps and studs, and the recess's shadow
    on its top and left. Returns the painted doorway, (x, y, w, h) in the layer's pixels."""
    if d.kind == "open":
        return _open_door(p, d, ground, wall_t)
    x = d.cx - d.w / 2
    y = ground - d.h
    w, h = d.w, d.h
    st = tones("stone")
    wood = tones(d.wood)
    iron = tones("iron")
    kind = d.kind
    if d.step:
        step = Path.rect(x - 10, ground - 6, w + 20, 10, r=1.5)
        stm = p.fill(step, st, "base")
        p.tone(stm - stm.shift(0, 2.2), st, "light")
        p.tone(p.below(stm, ground), st, "shadow")
        p.edge(step, st, 1.1, "shadow")
        h -= 6
    if d.surround == "stone":
        sur = _opening("arch" if kind in ("arch", "double") else "square", x - 12, y - 12, w + 24, h + 12)
        sm = p.fill(sur, st, "base-light")
        p.cast(sm, 3, 3.5, p.everything() - sm, wall_t, "shadow")
        if kind in ("arch", "double"):
            r = w / 2 + 12
            cx, cy = d.cx, y - 12 + r
            n = 9
            for i in range(1, n):
                a = math.radians(180 + 180 * i / n)
                p.stroke(Path.polyline([(cx + math.cos(a) * (w / 2), cy + math.sin(a) * (w / 2)),
                                        (cx + math.cos(a) * r, cy + math.sin(a) * r)]), 1.1, st, "shadow", clip=sm)
            key = Path.polygon([(cx - 7, y - 15), (cx + 7, y - 15), (cx + 5, y + 2), (cx - 5, y + 2)])
            km = p.fill(key, st, "light")
            p.edge(key, st, 1.0, "shadow")
            del km
        for j in range(int(h / 26)):
            yy = y + h - (j + 1) * 26 + (w / 2 if kind in ("arch", "double") else 0) * 0
            if yy < y + w / 2 and kind in ("arch", "double"):
                continue
            p.stroke(Path.polyline([(x - 12, yy), (x, yy)]), 1.0, st, "shadow", clip=sm)
            p.stroke(Path.polyline([(x + w, yy), (x + w + 12, yy)]), 1.0, st, "shadow", clip=sm)
        p.edge(sur, st, 1.3, "terminator")
    else:
        fw = tones("wood.dark")
        sur = _opening("arch" if kind in ("arch", "double") else "square", x - 8, y - 8, w + 16, h + 8)
        sm = p.fill(sur, fw, "base")
        p.tone(sm - sm.shift(1.6, 1.6), fw, "light")
        p.cast(sm, 3, 3.5, p.everything() - sm, wall_t, "shadow")
        p.edge(sur, fw, 1.2)
    op = _opening("arch" if kind in ("arch", "double") else kind, x, y, w, h)
    om = p.fill(op, wood, "base")
    planks = Path()
    n = max(3, int(w / 12))
    for i in range(1, n):
        if kind == "double" and i == n // 2:
            continue
        px = x + w * i / n
        planks.add(Path.polyline([(px, y), (px, y + h)]))
    p.stroke(planks, 1.1, wood, "deep", clip=om)
    for i in range(n):
        px = x + w * (i + 0.18) / n
        p.stroke(Path.polyline([(px, y + 4), (px, y + h - 4)]), 1.0, wood, "light", clip=om, alpha=0.5)
    if kind == "double":
        p.stroke(Path.polyline([(d.cx, y), (d.cx, y + h)]), 2.4, wood, "line", clip=om)
    for k, yy in enumerate((y + h * 0.3, y + h * 0.72)):
        strap = Path.rect(x, yy, w * (0.75 if kind != "double" else 1.0), 5)
        stm = p.fill(strap, iron, "base", clip=om)
        p.tone(stm - stm.shift(0, 1.4), iron, "light", clip=om)
        if d.studs:
            for i in range(int(w / 12)):
                style.rivet(p.c, x + 6 + i * 12 - p.ox, yy + 2.5 - p.oy, iron, r=1.3, clip=om)
    hx = d.cx + w * (0.28 if kind != "double" else 0.08)
    ring_y = y + h * 0.52
    p.stroke(Path.circle(hx, ring_y + 4, 3.6), 1.4, tones("brass"), "base")
    p.fill(Path.circle(hx, ring_y, 2.2), tones("brass"), "light")
    # the recess: the surround's shadow on the leaf's top and left
    p.tone(om - om.shift(5, 5.5), wood, "deep", alpha=0.9)
    p.edge(op, wood, 1.6)
    return (x, y, w, h + (6 if d.step else 0))


def _open_door(p, d, ground, wall_t):
    """An open doorway (a forge's, a stable's): a wide stone opening on a dark, warm inside with a
    fire glowing at the back and an anvil before it, its two leaves folded back against the jambs."""
    x, y, w, h = d.cx - d.w / 2, ground - d.h, d.w, d.h
    st = tones("stone.grey")
    sur = Path.rect(x - 14, y - 14, w + 28, h + 14)
    sm = p.fill(sur, st, "base-light")
    p.cast(sm, 3, 3.5, p.everything() - sm, wall_t, "shadow")
    lintel = Path.rect(x - 18, y - 18, w + 36, 16, r=2)
    lm = p.fill(lintel, st, "light")
    p.edge(lintel, st, 1.2, "shadow")
    del lm
    p.edge(sur, st, 1.3, "terminator")
    inside = tones("#4a3038")
    op = Path.rect(x, y, w, h)
    om = p.fill(op, inside, "base")
    p.tone(p.below(om, ground - 22), inside, "light")
    p.glow(x + w * 0.3, ground - 40, w * 0.9, "#ff8a3a", 0.55)
    hood = Path.polygon([(x + w * 0.08, y + h * 0.18), (x + w * 0.58, y + h * 0.18), (x + w * 0.52, ground - 70), (x + w * 0.14, ground - 70)])
    p.fill(hood, tones("#5a4650"), "base")
    p.edge(hood, tones("#5a4650"), 1.0)
    hearth = Path.rect(x + w * 0.1, ground - 52, w * 0.46, 34, r=3)
    p.fill(hearth, tones("stone.dark"), "base")
    p.edge(hearth, tones("stone.dark"), 1.0)
    coals = Path.rect(x + w * 0.14, ground - 58, w * 0.38, 8, r=3)
    p.fill(coals, tones("#e2553a"), "base")
    for k, (fx, fh) in enumerate(((0.2, 26), (0.3, 36), (0.4, 30), (0.48, 20))):
        cx_ = x + w * fx
        flame = Path().move(cx_ - 7, ground - 56).quad(cx_ - 8, ground - 56 - fh * 0.6, cx_ + 1, ground - 56 - fh)             .quad(cx_ + 2, ground - 56 - fh * 0.45, cx_ + 7, ground - 56).close()
        p.fill(flame, tones("#ff8f3a"), "base")
        inner = Path().move(cx_ - 3.5, ground - 56).quad(cx_ - 4, ground - 56 - fh * 0.4, cx_ + 0.5, ground - 56 - fh * 0.62)             .quad(cx_ + 1, ground - 56 - fh * 0.3, cx_ + 3.5, ground - 56).close()
        p.fill(inner, tones("#ffe08a"), "base")
    anvil = Path.polygon([(x + w * 0.6, ground - 34), (x + w * 0.92, ground - 34), (x + w * 0.86, ground - 26), (x + w * 0.8, ground - 26),
                          (x + w * 0.82, ground - 8), (x + w * 0.66, ground - 8), (x + w * 0.7, ground - 26), (x + w * 0.64, ground - 26)])
    p.fill(anvil, tones("#3a3444"), "base")
    p.tone(om - om.shift(6, 6), inside, "deep")
    wood = tones(d.wood)
    for side in (-1, 1):
        lx = x - 2 if side < 0 else x + w - 12
        leaf = Path.rect(lx, y + 4, 14, h - 4)
        m = p.fill(leaf, wood, "base")
        p.tone(m - m.shift(-2 * side, 0), wood, "light" if side < 0 else "shadow")
        for yy in (y + h * 0.25, y + h * 0.7):
            p.fill(Path.rect(lx, yy, 14, 4), tones("iron"), "base")
        p.edge(leaf, wood, 1.1)
    p.edge(op, inside, 1.6)
    return (x, y, w, h)


def _eave_shadow(p, house, walls, wt_top):
    """The roof's overhang shadow along the top of the wall, with a wavy lower edge where the tile
    ends are, and the jetty's under each upper floor."""
    y = house.wall_top
    sh = Path().move(house.x0 - house.jetty - 2, y - 2)
    sh.line(house.x1 + house.jetty + 2, y - 2)
    n = int(house.width / 12) + 2
    for i in range(n, -1, -1):
        sh.line(house.x0 - house.jetty + house.width * i / max(1, n - 2) + (house.jetty * 2) * 0, y + 13 + (3 if i % 2 else 0))
    sh.close()
    p.tone(p.shape(sh) & walls, wt_top, "shadow")


def building(layer, house):
    """Paints a house into a layer, ground floor up: its walls, the beams between floors, the
    windows and the door, the roof's shadow on the wall, chimneys and the roof, dormers, and the
    extras (signs, awnings, banners, lanterns, vines); then its outline and contact shadow. Each
    part is drawn on a canvas of its own size (Piece.sub) and stamped in.
    Returns the painted doorway (x, y, w, h), or None if it has no door."""
    tallest = max([ch for (_, _, ch) in house.chimneys] + [0])
    top = house.wall_top - house.roof_h - tallest - 60
    p = Piece(house.x0 - house.eave - house.jetty - 90, top, house.x1 + house.eave + house.jetty + 90, house.ground + 14)
    wall_tones = []
    for i, (spec, h) in enumerate(house.floors):
        fx0, fy0, fx1, fy1 = house.floor_box(i)
        p.part((fx0 - 2, fy0 - 2, fx1 + 2, fy1 + 2), wall, fx0, fy0, fx1, fy1, spec, house.seed + 13 * i, quoins=house.quoins)
        wall_tones.append(tones(_material(spec)[1]))

    def plinth(q):
        pt = tones("stone.grey")
        pl = Path.rect(house.x0 - 2, house.ground - house.plinth, house.width + 4, house.plinth + 4)
        plm = q.fill(pl, pt, "base")
        q.tone(plm - plm.shift(0, 2), pt, "light")
        for i in range(int(house.width / 40) + 1):
            bx = house.x0 + 20 + i * 40 + (hash01(i, 1, house.seed + 751) - 0.5) * 10
            q.stroke(Path.polyline([(bx, house.ground - house.plinth + 2), (bx, house.ground + 2)]), 1.0, pt, "shadow", clip=plm)
        q.edge(pl, pt, 1.2, "shadow")
    p.part((house.x0 - 6, house.ground - house.plinth - 4, house.x1 + 6, house.ground + 8), plinth)

    beam = tones("wood.dark")
    for i in range(1, len(house.floors)):
        fx0, fy0, fx1, fy1 = house.floor_box(i)
        lower = house.floor_box(i - 1)

        def floor_beam(q, i=i, fx0=fx0, fx1=fx1, fy1=fy1, lower=lower):
            walls = q.covered()
            if house.jetty:
                # the jetty: a shadow under it on the floor below, and brackets
                q.tone(q.shape(Path.rect(lower[0], fy1 + 4, lower[2] - lower[0], 14)) & walls, wall_tones[i - 1], "shadow")
                for bx in (lower[0] + 4, lower[2] - 12):
                    br = Path.polygon([(bx, fy1 + 4), (bx + 8, fy1 + 4), (bx + 8, fy1 + 22), (bx + 4, fy1 + 22)])
                    q.fill(br, beam, "base")
                    q.edge(br, beam, 1.0)
            bp = Path.rect(fx0 - 3, fy1 - 4, fx1 - fx0 + 6, 9)
            bm = q.fill(bp, beam, "base")
            q.tone(bm - bm.shift(0, 2), beam, "light")
            q.cast(bm, 0, 4, walls - bm, wall_tones[i - 1], "shadow")
            q.edge(bp, beam, 1.2)
        p.part((fx0 - 8, fy1 - 8, fx1 + 8, fy1 + 26), floor_beam)

    for wnd in house.windows:
        fl = wnd.floor if wnd.floor is not None else 0
        if wnd.top is None:
            fx0, fy0, fx1, fy1 = house.floor_box(fl)
            wnd.top = fy0 + (fy1 - fy0 - wnd.h) * 0.42
        box = (wnd.cx - wnd.w * 1.1 - 16, wnd.top - 20, wnd.cx + wnd.w * 1.1 + 16, wnd.top + wnd.h + (72 if wnd.box else 18))
        p.part(box, window, wnd, wall_tones[min(fl, len(wall_tones) - 1)], house.seed + int(wnd.cx))
    doorway = None
    if house.door is not None:
        d = house.door
        doorway = p.part((d.cx - d.w / 2 - 34, house.ground - d.h - 34, d.cx + d.w / 2 + 34, house.ground + 8),
                         door, d, house.ground, wall_tones[0], house.seed)
    p.part((house.x0 - house.jetty - 4, house.wall_top - 4, house.x1 + house.jetty + 4, house.wall_top + 22),
           lambda q: _eave_shadow(q, house, q.covered(), wall_tones[-1]))
    for vx in house.vines:
        p.part((vx - 30, house.wall_top + 10, vx + 30, house.ground), vines, vx, house.ground - 4, house.wall_top + 24, house.seed + int(vx))

    def roof_part(q):
        for (cx, cw, ch) in house.chimneys:
            chimney(q, cx, house.wall_top - house.roof_h * 0.45, cw, ch + house.roof_h * 0.45, house.seed)
        rm, _ = roof(q, house)
        for dx in house.dormers:
            dormer(q, dx, house, rm)
    p.part((house.x0 - house.eave - house.jetty - 24, top, house.x1 + house.eave + house.jetty + 24, house.wall_top + 24), roof_part)
    for f in house.extras:
        box = getattr(f, "box", None)
        if box is None:
            f(p, house)
        else:
            p.part(box, f, house)
    p.ground_shadow(house.x0 - 20, house.x1 + 20, house.ground + 3, 9, 0.35)
    layer.place(p)
    return doorway


def chimney(p, cx, base, w, h, seed=0, material="brick"):
    """A chimney: a stack lit on its left and shadowed on its right, a cap slab, a pot."""
    t = tones(material if material in PALETTE else "brick")
    top = base - h
    body = Path.rect(cx - w / 2, top, w, h)
    m = p.fill(body, t, "base")
    for i in range(int(h / 9)):
        yy = top + 10 + i * 9
        p.stroke(Path.polyline([(cx - w / 2, yy), (cx + w / 2, yy)]), 0.9, t, "shadow", clip=m)
        off = (w / 3) * (i % 2)
        p.stroke(Path.polyline([(cx - w / 2 + off + w / 3, yy - 9), (cx - w / 2 + off + w / 3, yy)]), 0.9, t, "shadow", clip=m)
    p.tone(p.where(m, lambda xx, yy: xx > cx + w * 0.18), t, "shadow")
    p.tone(p.where(m, lambda xx, yy: xx < cx - w * 0.36), t, "light")
    cap = Path.rect(cx - w / 2 - 4, top - 6, w + 8, 7, r=1.5)
    st = tones("stone.grey")
    cm = p.fill(cap, st, "base")
    p.tone(cm - cm.shift(0, 2), st, "light")
    p.edge(cap, st, 1.1, "shadow")
    pot = Path.rect(cx - w * 0.22, top - 16, w * 0.44, 11, r=2)
    tt = tones("terracotta")
    pm = p.fill(pot, tt, "base")
    p.tone(p.where(pm, lambda xx, yy: xx > cx + 1), tt, "shadow")
    p.edge(pot, tt, 1.0)
    p.edge(body, t, 1.2)


def dormer(p, cx, house, roof_mask):
    """A dormer window standing out of the roof: a small front gable of the wall's material."""
    t = tones(house.roof_colour)
    ye = house.wall_top
    base = ye - house.roof_h * 0.18
    w, h = 46, 40
    wall_t = tones(_material(house.floors[-1][0])[1])
    body = Path.rect(cx - w / 2, base - h, w, h + 4)
    bm = p.fill(body, wall_t, "base")
    p.tone(p.where(bm, lambda xx, yy: xx > cx + w * 0.3), wall_t, "shadow")
    p.edge(body, wall_t, 1.2)
    window(p, Window(cx, base - h + 9, 20, 26, "arch", lit=hash01(int(cx), 3, house.seed) < 0.5, sill=False), wall_t, house.seed)
    hood = Path.polygon([(cx - w / 2 - 8, base - h + 4), (cx, base - h - 26), (cx + w / 2 + 8, base - h + 4),
                         (cx + w / 2 - 2, base - h + 4), (cx, base - h - 14), (cx - w / 2 + 2, base - h + 4)])
    hm = p.fill(hood, t, "base")
    p.tone(p.where(hm, lambda xx, yy: xx > cx), t, "shadow")
    p.tone(hm - hm.shift(1.2, 1.4), t, "light")
    p.edge(hood, t, 1.3)
    p.cast(p.shape(Path.rect(cx - w / 2, base - h, w, h + 4)) | hm, 5, 4, roof_mask - bm - hm, t, "shadow")


# --- extras: things fixed to a building's front ---------------------------- #

def awning(x0, x1, y, depth=26, colours=("cloth.pink", "cloth.cream"), stripes=None, valance=10):
    """An awning over a shop window, from x0 to x1 at height y, as an extra (f(piece, house)):
    stripes running down a sloping cloth to a scalloped valance, lit above and shadowed under, and
    its shadow on the wall below."""

    def draw(p, house=None):
        n = stripes or max(4, int((x1 - x0) / 18))
        out = 8
        top_l, top_r = (x0, y), (x1, y)
        bot_l, bot_r = (x0 - out, y + depth), (x1 + out, y + depth)
        cloth = Path.polygon([top_l, top_r, bot_r, bot_l])
        wall_m = p.covered()
        shadow_under = p.shape(Path.polygon([(x0 - out + 4, y + depth), (x1 + out - 4, y + depth), (x1 + 4, y + depth + 30), (x0 + 8, y + depth + 30)])) & wall_m
        p.colour(shadow_under, SHADE_TO, alpha=0.32)
        cm = p.fill(cloth, tones(colours[1]), "base")
        for i in range(n):
            if i % 2:
                continue
            u0, u1 = i / n, (i + 1) / n
            sp = Path.polygon([(x0 + (x1 - x0) * u0, y), (x0 + (x1 - x0) * u1, y),
                               (bot_l[0] + (bot_r[0] - bot_l[0]) * u1, y + depth), (bot_l[0] + (bot_r[0] - bot_l[0]) * u0, y + depth)])
            p.fill(sp, tones(colours[0]), "base", clip=cm)
        p.tone(p.above(cm, y + 5), tones(colours[1]), "shadow", alpha=0.35)
        p.tone(cm - cm.shift(0, 2), tones(colours[1]), "light", alpha=0.5)
        # the valance: scallops
        val = Path().move(bot_l[0], y + depth)
        k = n * 2
        for i in range(k):
            ax = bot_l[0] + (bot_r[0] - bot_l[0]) * i / k
            bx = bot_l[0] + (bot_r[0] - bot_l[0]) * (i + 1) / k
            val.quad((ax + bx) / 2, y + depth + valance * 2.0, bx, y + depth)
        val.close()
        vm = p.shape(val)
        for i in range(k):
            ax = bot_l[0] + (bot_r[0] - bot_l[0]) * i / k
            c = colours[0] if (i // 2) % 2 == 0 else colours[1]
            p.tone(vm & p.shape(Path.rect(ax, y + depth - 1, (bot_r[0] - bot_l[0]) / k + 0.5, valance * 2)), tones(c), "base")
        p.tone(vm.shift(0, 0) & p.below(vm, y + depth + valance * 0.6), tones(colours[1]), "shadow", alpha=0.45)
        p.edge_mask(vm, tones(colours[0]), 1.1)
        p.edge(cloth, tones(colours[0]), 1.2)
        rod = Path.rect(x0 - 3, y - 3, x1 - x0 + 6, 5, r=2)
        p.fill(rod, tones("wood.dark"), "base")
        p.edge(rod, tones("wood.dark"), 1.0)
    draw.box = (x0 - 14, y - 8, x1 + 14, y + depth + valance * 2 + 36)
    return draw


def hanging_sign(x, y, icon="tankard", board="wood", side=1, w=58, h=46, shape="shield", trim="gold"):
    """A hanging sign on a wrought-iron bracket out from the wall at (x, y), the board `w` x `h`
    hanging under it with an icon painted on (tankard, loaf, anvil, bottle, flower, sun, diamond,
    needle, candle), as an extra."""

    def draw(p, house=None):
        iron = tones("iron")
        arm = 70 * side
        bar = Path.polyline([(x, y), (x + arm, y)])
        p.stroke(bar, 3.6, iron, "base")
        p.stroke(Path.smooth([(x, y + 22), (x + arm * 0.25, y + 18), (x + arm * 0.45, y + 6), (x + arm * 0.5, y)]), 2.4, iron, "base")
        p.stroke(Path.circle(x + arm * 0.22, y + 9, 5), 1.8, iron, "base")
        p.fill(Path.rect(x - 4 * side if side > 0 else x, y - 6, 4, 30), iron, "shadow")
        bx = x + arm * 0.62
        p.stroke(Path.polyline([(bx - w * 0.32, y), (bx - w * 0.32, y + 10)]), 1.4, iron, "deep")
        p.stroke(Path.polyline([(bx + w * 0.32, y), (bx + w * 0.32, y + 10)]), 1.4, iron, "deep")
        top = y + 10
        if shape == "shield":
            bp = Path().move(bx - w / 2, top).line(bx + w / 2, top).line(bx + w / 2, top + h * 0.55) \
                .quad(bx + w / 2, top + h * 0.9, bx, top + h).quad(bx - w / 2, top + h * 0.9, bx - w / 2, top + h * 0.55).close()
        elif shape == "oval":
            bp = Path.ellipse(bx, top + h / 2, w / 2, h / 2)
        else:
            bp = Path.rect(bx - w / 2, top, w, h, r=6)
        bt = tones(board)
        bm = p.fill(bp, bt, "base")
        p.tone(bm - bm.shift(2.5, 2.5), bt, "light")
        p.tone(p.where(bm, lambda xx, yy: xx > bx + w * 0.3), bt, "shadow")
        inner = p.shape(bp)
        ring = inner - inner.inset(3.2)
        p.tone(ring, tones(trim), "base")
        p.tone(ring - ring.shift(1, 1), tones(trim), "light")
        _icon(p, icon, bx, top + h * 0.46, min(w, h) * 0.62)
        p.edge(bp, bt, 1.4)

    draw.box = (min(x, x + 70 * side) - w / 2 - 10, y - 10, max(x, x + 70 * side) + w / 2 + 10, y + h + 20)
    return draw


def _icon(p, icon, cx, cy, s):
    """The picture on a sign or a banner, about s pixels across, in gold or its own colours."""
    g = tones("gold")
    if icon == "tankard":
        body = Path.rect(cx - s * 0.32, cy - s * 0.3, s * 0.5, s * 0.62, r=s * 0.06)
        m = p.fill(body, g, "base")
        p.stroke(Path.smooth([(cx + s * 0.18, cy - s * 0.18), (cx + s * 0.42, cy - s * 0.12), (cx + s * 0.4, cy + s * 0.14), (cx + s * 0.18, cy + s * 0.18)]), s * 0.09, g, "base")
        foam = Path.circle(cx - s * 0.22, cy - s * 0.32, s * 0.12).add(Path.circle(cx - s * 0.06, cy - s * 0.36, s * 0.14)).add(Path.circle(cx + s * 0.1, cy - s * 0.31, s * 0.11))
        p.fill(foam, tones("cloth.cream"), "base", rule="nonzero")
        p.tone(p.where(m, lambda xx, yy: xx > cx + s * 0.02), g, "shadow")
        for k in range(2):
            p.stroke(Path.polyline([(cx - s * 0.32, cy - s * 0.05 + k * s * 0.26), (cx + s * 0.18, cy - s * 0.05 + k * s * 0.26)]), s * 0.05, g, "deep", clip=m)
    elif icon == "loaf":
        body = Path.ellipse(cx, cy, s * 0.46, s * 0.26)
        bt = tones("bread")
        m = p.fill(body, bt, "base")
        p.tone(m - m.shift(-s * 0.06, -s * 0.08), bt, "shadow")
        for k in range(3):
            x = cx - s * 0.22 + k * s * 0.2
            p.stroke(Path.polyline([(x - s * 0.06, cy + s * 0.06), (x + s * 0.06, cy - s * 0.14)]), s * 0.06, bt, "deep-shadow", clip=m)
        p.tone(m - m.shift(s * 0.04, s * 0.05), bt, "light")
    elif icon == "anvil":
        body = Path.polygon([(cx - s * 0.5, cy - s * 0.2), (cx + s * 0.32, cy - s * 0.2), (cx + s * 0.5, cy - s * 0.1), (cx + s * 0.18, cy - s * 0.02),
                             (cx + s * 0.12, cy + s * 0.14), (cx + s * 0.26, cy + s * 0.3), (cx - s * 0.26, cy + s * 0.3), (cx - s * 0.12, cy + s * 0.14),
                             (cx - s * 0.16, cy - s * 0.04), (cx - s * 0.36, cy - s * 0.08)])
        it = tones("#8d93a8")
        m = p.fill(body, it, "base")
        p.tone(p.below(m, cy), it, "shadow")
        p.tone(m - m.shift(0, s * 0.05), it, "highlight")
    elif icon == "bottle":
        body = Path().move(cx - s * 0.08, cy - s * 0.42).line(cx + s * 0.08, cy - s * 0.42).line(cx + s * 0.08, cy - s * 0.16) \
            .quad(cx + s * 0.34, cy - s * 0.05, cx + s * 0.3, cy + s * 0.2).quad(cx + s * 0.24, cy + s * 0.4, cx, cy + s * 0.4) \
            .quad(cx - s * 0.24, cy + s * 0.4, cx - s * 0.3, cy + s * 0.2).quad(cx - s * 0.34, cy - s * 0.05, cx - s * 0.08, cy - s * 0.16).close()
        ct = tones("#6fd0b0")
        m = p.fill(body, ct, "base")
        p.tone(p.above(m, cy + s * 0.02), tones("#d8f2ec"), "base")
        p.stroke(Path.polyline([(cx - s * 0.16, cy - s * 0.02), (cx - s * 0.18, cy + s * 0.22)]), s * 0.06, ct, "highlight", clip=m)
        p.fill(Path.rect(cx - s * 0.1, cy - s * 0.5, s * 0.2, s * 0.1), tones("wood"), "base")
    elif icon == "flower":
        for k in range(5):
            a = math.radians(k * 72 - 90)
            pm = p.fill(Path.circle(cx + math.cos(a) * s * 0.2, cy + math.sin(a) * s * 0.2, s * 0.17), tones("flower.pink"), "base")
            p.tone(pm - pm.shift(-1, -1), tones("flower.pink"), "shadow")
        p.fill(Path.circle(cx, cy, s * 0.12), g, "base")
    elif icon in ("sun", "diamond"):
        if icon == "sun":
            for k in range(8):
                a = math.radians(k * 45)
                p.fill(Path.polygon([(cx + math.cos(a) * s * 0.46, cy + math.sin(a) * s * 0.46),
                                     (cx + math.cos(a + 0.35) * s * 0.2, cy + math.sin(a + 0.35) * s * 0.2),
                                     (cx + math.cos(a - 0.35) * s * 0.2, cy + math.sin(a - 0.35) * s * 0.2)]), g, "base")
            m = p.fill(Path.circle(cx, cy, s * 0.22), g, "light")
        else:
            m = p.fill(Path.polygon([(cx, cy - s * 0.45), (cx + s * 0.3, cy), (cx, cy + s * 0.45), (cx - s * 0.3, cy)]), g, "base")
            p.fill(Path.polygon([(cx, cy - s * 0.22), (cx + s * 0.14, cy), (cx, cy + s * 0.22), (cx - s * 0.14, cy)]), g, "light")
        p.tone(p.where(m, lambda xx, yy: xx > cx), g, "shadow", alpha=0.6)
    elif icon == "needle":
        p.stroke(Path.smooth([(cx - s * 0.36, cy + s * 0.3), (cx, cy), (cx + s * 0.34, cy - s * 0.32)]), s * 0.06, tones("#c9cfe0"), "light")
        p.stroke(Path.smooth([(cx + s * 0.3, cy - s * 0.28), (cx + s * 0.1, cy + s * 0.2), (cx - s * 0.2, cy + s * 0.05), (cx - s * 0.3, cy - s * 0.25)]), s * 0.05, tones("cloth.pink"), "base")
    elif icon == "candle":
        wax = tones("cloth.cream")
        body = Path.rect(cx - s * 0.12, cy - s * 0.12, s * 0.24, s * 0.5, r=s * 0.04)
        m = p.fill(body, wax, "base")
        p.tone(p.where(m, lambda xx, yy: xx > cx + s * 0.02), wax, "shadow")
        p.fill(Path.ellipse(cx, cy + s * 0.38, s * 0.3, s * 0.08), g, "base")
        flame = Path().move(cx, cy - s * 0.48).quad(cx + s * 0.12, cy - s * 0.26, cx, cy - s * 0.16).quad(cx - s * 0.12, cy - s * 0.26, cx, cy - s * 0.48).close()
        p.fill(flame, tones("#ffb04a"), "base")
        p.fill(Path.ellipse(cx, cy - s * 0.25, s * 0.04, s * 0.08), tones("#fff1b8"), "base")
    else:
        p.fill(Path.circle(cx, cy, s * 0.3), g, "base")


def banner(x, y, w=30, h=90, colour="cloth.pink", emblem="diamond", trim="gold", pole=True, tail="point"):
    """A hanging banner on a short pole out from the wall, its top at (x, y): the cloth falling in
    two soft folds, gold trim down its sides, an emblem, a pointed or swallow tail. An extra."""

    def draw(p, house=None):
        t = tones(colour)
        g = tones(trim)
        if pole:
            rod = Path.rect(x - w / 2 - 6, y - 4, w + 12, 5, r=2)
            p.fill(rod, tones("wood.dark"), "base")
            for ex in (x - w / 2 - 6, x + w / 2 + 6):
                p.fill(Path.circle(ex, y - 1.5, 3.4), g, "base")
            p.edge(rod, tones("wood.dark"), 1.0)
        if tail == "point":
            bp = Path.polygon([(x - w / 2, y), (x + w / 2, y), (x + w / 2, y + h * 0.82), (x, y + h), (x - w / 2, y + h * 0.82)])
        else:
            bp = Path.polygon([(x - w / 2, y), (x + w / 2, y), (x + w / 2, y + h), (x, y + h * 0.8), (x - w / 2, y + h)])
        m = p.fill(bp, t, "base")
        p.tone(p.where(m, lambda xx, yy: xx > x + w * 0.18), t, "shadow")
        p.tone(p.where(m, lambda xx, yy: (xx > x - w * 0.12) & (xx < x - w * 0.02)), t, "terminator", alpha=0.7)
        p.tone(p.where(m, lambda xx, yy: xx < x - w * 0.34), t, "light")
        border = m - m.inset(3.0)
        p.tone(border, g, "base")
        p.tone(border - border.shift(1, 1), g, "light")
        _icon(p, emblem, x, y + h * 0.42, w * 0.7)
        p.edge(bp, t, 1.3)
        p.cast(m, 4, 5, p.covered() - m, tones("stone"), "shadow", alpha=0.6)

    draw.box = (x - w / 2 - 12, y - 10, x + w / 2 + 12, y + h + 14)
    return draw


def wall_lantern(x, y, side=1, lit=True):
    """A lantern on a curled iron bracket out from the wall at (x, y), glowing if lit. An extra."""

    def draw(p, house=None):
        iron = tones("iron")
        p.stroke(Path.smooth([(x, y), (x + 10 * side, y - 4), (x + 18 * side, y - 2), (x + 20 * side, y + 6)]), 2.4, iron, "base")
        p.fill(Path.rect(x - 2, y - 7, 4, 14), iron, "shadow")
        lantern_body(p, x + 20 * side, y + 6, 1.0, lit)

    draw.box = (x - 36, y - 12, x + 36, y + 44)
    return draw


def lantern_body(p, cx, top, s=1.0, lit=True):
    """A small iron-framed lantern hanging from (cx, top): cap, glass, base; its glow if lit."""
    iron = tones("iron")
    cap = Path.polygon([(cx - 8 * s, top + 8 * s), (cx, top), (cx + 8 * s, top + 8 * s)])
    p.fill(cap, iron, "base")
    gl = Path.polygon([(cx - 7 * s, top + 8 * s), (cx + 7 * s, top + 8 * s), (cx + 5.5 * s, top + 24 * s), (cx - 5.5 * s, top + 24 * s)])
    gt = tones("glass.lit") if lit else tones("glass")
    gm = p.fill(gl, gt, "light" if lit else "base")
    if lit:
        p.fill(Path.ellipse(cx, top + 16 * s, 2.4 * s, 4 * s), tones("#fff6d8"), "base")
        p.glow(cx, top + 16 * s, 46 * s, "#ffc86a", 0.42)
        p.glow(cx, top + 16 * s, 16 * s, "#fff0c0", 0.4)
    p.stroke(Path.polyline([(cx, top + 8 * s), (cx, top + 24 * s)]), 1.2 * s, iron, "base", clip=gm)
    base = Path.rect(cx - 7 * s, top + 23 * s, 14 * s, 4 * s, r=1)
    p.fill(base, iron, "base")
    p.edge(gl, iron, 1.4 * s, "base")
    p.fill(Path.circle(cx, top - 1.5 * s, 2 * s), iron, "light")


def wall_sign(x, y, w, h, icon, board="wood.dark"):
    """A board fixed flat to the wall over a door, with a gold rim and an icon: a house extra."""

    def draw(p, house=None):
        bt = tones(board)
        bp = Path.rect(x - w / 2, y, w, h, r=4)
        m = p.fill(bp, bt, "base")
        p.cast(m, 3, 4, p.covered() - m, tones("stone.grey"), "shadow")
        p.tone(m - m.shift(1.6, 1.6), bt, "light")
        ring = m - m.inset(3)
        p.tone(ring, tones("gold"), "base")
        _icon(p, icon, x, y + h / 2, h * 0.9)
        for sx in (x - w / 2 + 8, x + w / 2 - 8):
            p.fill(Path.circle(sx, y + h / 2, 2.0), tones("gold"), "light")
        p.edge(bp, bt, 1.3)
    draw.box = (x - w / 2 - 8, y - 8, x + w / 2 + 8, y + h + 12)
    return draw


def vines(p, x, bottom, top, seed=0, flowers_on=True):
    """Ivy climbing a wall from (x, bottom) up toward top: a wandering stem, leaves in pairs lit on
    their top left, and here and there a pink or white flower."""
    leaf = tones("leaf")
    leaf2 = tones("leaf.dark")
    pts = []
    n = max(4, int((bottom - top) / 26))
    for i in range(n + 1):
        u = i / n
        pts.append((x + 9 * math.sin(u * 5.0 + seed) + 4 * (hash01(i, 1, seed + 761) - 0.5), bottom - (bottom - top) * u))
    stem = Path.smooth(pts)
    p.stroke(stem, (2.6, 0.8), tones("bark"), "shadow")
    for i in range(int((bottom - top) / 9)):
        (lx, ly), (tx, ty) = stem.at(i * 9 / max(1.0, bottom - top) * 0.98)
        side = 1 if i % 2 else -1
        t = leaf if hash01(i, 2, seed + 761) < 0.6 else leaf2
        cx, cy = lx + side * 6, ly - 2
        lm = p.fill(Path.ellipse(cx, cy, 5.6, 3.6, rot=-35 * side), t, "base")
        p.tone(lm - lm.shift(-1.2, -1.2), t, "shadow")
        p.edge(Path.ellipse(cx, cy, 5.6, 3.6, rot=-35 * side), t, 0.9)
        if flowers_on and hash01(i, 3, seed + 761) < 0.18:
            c = tones("flower.pink" if hash01(i, 4, seed + 761) < 0.6 else "flower.white")
            fm = p.fill(Path.circle(cx + side * 3, cy - 3, 3.2), c, "base")
            p.tone(fm - fm.shift(-0.8, -0.8), c, "shadow")
            p.fill(Path.circle(cx + side * 3, cy - 3, 1.0), tones("flower.yellow"), "base")


# --------------------------------------------------------------------------- #
# Plants
# --------------------------------------------------------------------------- #

def _clumps(p, clumps, t, back=None, line=True, light_k=1.0):
    """Foliage as round clumps, back to front: each clump in the base tone with its lower right a
    crescent of shadow and its upper left a patch of light (the ones further back a step darker),
    and the dark line along its lower edge, so the crown reads as heaped clumps."""
    whole = p.nothing()
    for i, (x, y, r, depth) in enumerate(clumps):
        cm = p.shape(Path.circle(x, y, r))
        tt = back if (back is not None and depth > 0) else t
        p.tone(cm, tt, "base")
        p.tone(cm - p.shape(Path.circle(x - r * 0.16, y - r * 0.24, r * 1.02)), tt, "shadow")
        p.tone(cm & p.shape(Path.circle(x - r * 0.32, y - r * 0.36, r * 0.52 * light_k)), tt, "light")
        p.tone(cm & p.shape(Path.circle(x - r * 0.4, y - r * 0.44, r * 0.2 * light_k)), tt, "highlight", alpha=0.7)
        if line:
            arc = Path().arc(x, y, r - 0.6, -15, 165)
            p.stroke(arc, lambda u: 1.5 * math.sin(math.pi * u) ** 0.6, tt, "line")
        whole = whole | cm
    return whole


def tree(layer, x, ground, kind="oak", height=300, seed=0, place=True):
    """A tree standing at (x, ground): a trunk forking into branches, a crown of clumps (oak, green;
    blossom, pink; cypress, a tall dark flame), and its contact shadow. Returns the Piece."""
    h = height
    w = h * (0.8 if kind != "cypress" else 0.28)
    p = Piece(x - w * 0.75, ground - h - 20, x + w * 0.75, ground + 14)
    bark = tones("bark")
    trunk_h = h * (0.42 if kind != "cypress" else 0.16)
    tw = h * 0.045 + 5
    trunk = Path().move(x - tw * 1.5, ground).quad(x - tw * 0.6, ground - trunk_h * 0.3, x - tw * 0.7, ground - trunk_h) \
        .line(x + tw * 0.7, ground - trunk_h).quad(x + tw * 0.6, ground - trunk_h * 0.3, x + tw * 1.6, ground).close()
    tm = p.fill(trunk, bark, "base")
    p.tone(p.where(tm, lambda xx, yy: xx > x + tw * 0.15), bark, "shadow")
    p.tone(p.where(tm, lambda xx, yy: xx < x - tw * 0.45), bark, "light")
    for k in range(3):
        bx = x - tw * 0.4 + k * tw * 0.4
        p.stroke(Path.polyline([(bx, ground - trunk_h * 0.9), (bx + 1, ground - trunk_h * 0.2)]), (1.1, 0.0), bark, "deep", clip=tm)
    if kind != "cypress":
        for side in (-1, 1):
            br = Path.smooth([(x, ground - trunk_h * 0.9), (x + side * w * 0.18, ground - trunk_h * 1.2), (x + side * w * 0.3, ground - trunk_h * 1.45)])
            p.stroke(br, (tw * 0.9, tw * 0.35), bark, "base")
    p.edge(trunk, bark, 1.3)
    if kind == "cypress":
        t = tones("leaf.dark")
        clumps = []
        n = 9
        for i in range(n):
            u = i / (n - 1)
            cy = ground - trunk_h - (h - trunk_h) * u * 0.9
            r = w * 0.5 * (1.0 - u * 0.75) + 4
            clumps.append((x + (hash01(i, 1, seed + 771) - 0.5) * 6, cy, r, 0))
        whole = _clumps(p, clumps, t)
        tipm = p.fill(Path.polygon([(x - 7, ground - h * 0.86), (x, ground - h), (x + 7, ground - h * 0.86)]), t, "base")
        whole = whole | tipm
    else:
        t = tones("leaf" if kind == "oak" else "blossom")
        back = tones("leaf.dark" if kind == "oak" else "#e07fae")
        cx, cy = x, ground - trunk_h - (h - trunk_h) * 0.48
        rx, ry = w * 0.5, (h - trunk_h) * 0.5
        clumps = []
        n = 14 if kind == "oak" else 16
        for i in range(n):
            a = 2 * math.pi * i / n + hash01(i, 1, seed + 773) * 0.5
            d = 0.62 + 0.25 * hash01(i, 2, seed + 773)
            px, py = cx + math.cos(a) * rx * d, cy + math.sin(a) * ry * d * 0.85
            r = min(rx, ry) * (0.36 + 0.14 * hash01(i, 3, seed + 773))
            clumps.append((px, py, r, 1 if math.sin(a) < -0.1 else 1))
        clumps.sort(key=lambda c: c[1])
        inner = []
        for i in range(7):
            a = 2 * math.pi * i / 7 + 0.4
            inner.append((cx + math.cos(a) * rx * 0.3, cy + math.sin(a) * ry * 0.3 - ry * 0.05, min(rx, ry) * 0.42, 0))
        inner.sort(key=lambda c: c[1])
        whole = _clumps(p, [c for c in clumps if c[1] < cy], t, back)
        whole = whole | _clumps(p, inner, t)
        whole = whole | _clumps(p, [c for c in clumps if c[1] >= cy], t)
        if kind == "blossom":
            for i in range(26):
                a = hash01(i, 4, seed + 775) * 6.283
                d = hash01(i, 5, seed + 775) ** 0.5
                fx, fy = cx + math.cos(a) * rx * d * 0.95, cy + math.sin(a) * ry * d * 0.9
                fc = tones("blossom.white")
                fm = p.fill(Path.circle(fx, fy, 2.4), fc, "base", clip=whole)
                p.tone(fm - fm.shift(-0.6, -0.6), fc, "shadow")
    p.ground_shadow(x - w * 0.42, x + w * 0.42, ground + 2, 10, 0.32)
    if place:
        layer.place(p)
    return p


def bush(layer, x, ground, w=90, h=60, colour="leaf", seed=0, flowers_on=None, place=True, piece=None):
    """A low bush of round clumps standing on the ground, flowering if asked (a list of colours)."""
    p = piece or Piece(x - w / 2 - 10, ground - h - 10, x + w / 2 + 10, ground + 12)
    t = tones(colour)
    clumps = []
    n = max(3, int(w / 22))
    for i in range(n):
        u = (i + 0.5) / n
        bump = math.sin(math.pi * u)
        r = h * (0.32 + 0.22 * bump) * (0.85 + 0.3 * hash01(i, 1, seed + 781))
        clumps.append((x - w / 2 + w * u, ground - r * 0.85 - bump * h * 0.25, r, 0))
    clumps.sort(key=lambda c: -c[2])
    whole = _clumps(p, clumps, t)
    if flowers_on:
        for i in range(int(w * h / 260)):
            fx = x - w / 2 + w * hash01(i, 2, seed + 783)
            fy = ground - h * 0.95 * hash01(i, 3, seed + 783) ** 0.7
            c = tones(flowers_on[int(hash01(i, 4, seed + 783) * len(flowers_on))])
            fm = p.fill(Path.circle(fx, fy, 2.8), c, "base", clip=whole)
            p.tone(fm - fm.shift(-0.7, -0.7), c, "shadow")
    p.ground_shadow(x - w * 0.55, x + w * 0.55, ground + 1, 7, 0.3)
    if place and piece is None:
        layer.place(p)
    return p


def topiary(layer, x, ground, h=120, seed=0, piece=None):
    """A clipped ball of box on a stem in a terracotta pot."""
    p = piece or Piece(x - 40, ground - h - 10, x + 40, ground + 10)
    pot(None, x, ground, 34, 30, "terracotta", piece=p)
    p.stroke(Path.polyline([(x, ground - 28), (x, ground - h * 0.55)]), 4, tones("bark"), "base")
    r = h * 0.26
    _clumps(p, [(x - r * 0.4, ground - h + r * 1.1, r * 0.75, 0), (x + r * 0.45, ground - h + r * 1.15, r * 0.7, 0),
                (x, ground - h + r * 0.9, r * 0.85, 0)], tones("leaf.dark"))
    if piece is None:
        layer.place(p)
    return p


# --------------------------------------------------------------------------- #
# Props
# --------------------------------------------------------------------------- #

def pot(layer, x, ground, w=30, h=28, colour="terracotta", piece=None, plant=None, seed=0):
    """A flowerpot: a tapered body with a rolled rim, lit left and shadowed right; a plant in it
    if asked ("flowers" or "fern")."""
    p = piece or Piece(x - w, ground - h - 50, x + w, ground + 10)
    t = tones(colour)
    body = Path.polygon([(x - w / 2, ground - h + 6), (x + w / 2, ground - h + 6), (x + w * 0.36, ground), (x - w * 0.36, ground)])
    m = p.fill(body, t, "base")
    p.tone(p.where(m, lambda xx, yy: xx > x + w * 0.12), t, "shadow")
    p.tone(p.where(m, lambda xx, yy: xx < x - w * 0.3), t, "light")
    rim = Path.rect(x - w / 2 - 3, ground - h, w + 6, 8, r=2)
    rm = p.fill(rim, t, "base-light")
    p.tone(p.where(rm, lambda xx, yy: xx > x + w * 0.2), t, "terminator")
    p.edge(body, t, 1.2)
    p.edge(rim, t, 1.2)
    if plant:
        bush(None, x, ground - h + 2, w * 1.3, w * 0.9, "leaf", seed, flowers_on=("flower.pink", "flower.white") if plant == "flowers" else None, piece=p)
    p.ground_shadow(x - w * 0.6, x + w * 0.6, ground + 1, 5, 0.3)
    if piece is None and layer is not None:
        layer.place(p)
    return p


def crate(layer, x, ground, w=46, h=40, seed=0, piece=None, top_goods=None):
    """A wooden crate seen a little from above: its front of planks with a cross brace and corner
    posts, its top a lit sliver, and goods heaped in it if asked (apple, orange, pear, bread)."""
    p = piece or Piece(x - w, ground - h - 40, x + w, ground + 10)
    t = tones("wood.light")
    d = 8
    front = Path.rect(x - w / 2, ground - h, w, h)
    topf = Path.polygon([(x - w / 2, ground - h), (x - w / 2 + d * 0.6, ground - h - d), (x + w / 2 + d * 0.6, ground - h - d), (x + w / 2, ground - h)])
    side = Path.polygon([(x + w / 2, ground - h), (x + w / 2 + d * 0.6, ground - h - d), (x + w / 2 + d * 0.6, ground - d), (x + w / 2, ground)])
    sm = p.fill(side, t, "shadow")
    if top_goods:
        g = tones(top_goods)
        heap = p.nothing()
        for i in range(int(w / 9)):
            gx = x - w / 2 + 6 + i * 9 + d * 0.3
            heap = heap | p.shape(Path.circle(gx, ground - h - d * 0.5 - 2 * hash01(i, 1, seed), 5.4))
        p.tone(heap, g, "base")
        p.tone(heap - heap.shift(-1.4, -1.6), g, "shadow")
        for i in range(int(w / 9)):
            gx = x - w / 2 + 6 + i * 9 + d * 0.3
            p.fill(Path.circle(gx - 1.6, ground - h - d * 0.5 - 2 * hash01(i, 1, seed) - 1.8, 1.2), g, "highlight")
        p.edge_mask(heap, g, 1.0)
    else:
        tm = p.fill(topf, t, "light")
        p.edge(topf, t, 1.1)
        del tm
    fm = p.fill(front, t, "base")
    for i in range(1, 3):
        p.stroke(Path.polyline([(x - w / 2, ground - h + h * i / 3), (x + w / 2, ground - h + h * i / 3)]), 1.0, t, "deep", clip=fm)
    brace = Path.polyline([(x - w / 2 + 5, ground - 5), (x + w / 2 - 5, ground - h + 5)])
    bm = p.line_shape(brace, 5) & fm
    p.cast(bm, 1.5, 2, fm, t, "shadow")
    p.tone(bm, t, "base-light")
    for px in (x - w / 2, x + w / 2 - 6):
        pm = p.fill(Path.rect(px, ground - h, 6, h), t, "terminator")
        del pm
    p.edge(front, t, 1.3)
    p.edge(side, t, 1.1)
    del sm
    p.ground_shadow(x - w * 0.6, x + w * 0.7, ground + 1, 6, 0.32)
    if piece is None and layer is not None:
        layer.place(p)
    return p


def barrel(layer, x, ground, w=40, h=52, colour="wood", seed=0, piece=None, lid=True):
    """A barrel: a bulging body of staves shaded as a cylinder (style.shade's), three iron hoops,
    and its lid an ellipse lit on top."""
    p = piece or Piece(x - w, ground - h - 20, x + w, ground + 10)
    t = tones(colour)
    body = Path().move(x - w * 0.42, ground).quad(x - w * 0.56, ground - h / 2, x - w * 0.42, ground - h) \
        .line(x + w * 0.42, ground - h).quad(x + w * 0.56, ground - h / 2, x + w * 0.42, ground).close()
    m = p.shape(body)
    style.shade(m, "cloth", t, form="cylinder", edge=INNER, rim=False)
    for k in range(-2, 3):
        sx = x + k * w * 0.18
        p.stroke(Path.smooth([(sx * 0.98 + x * 0.02, ground - h + 2), (sx + k * w * 0.04, ground - h / 2), (sx * 0.98 + x * 0.02, ground - 2)]), 0.9, t, "deep", clip=m)
    iron = tones("iron")
    for yy in (ground - h * 0.16, ground - h * 0.5, ground - h * 0.84):
        hp = p.line_shape(Path.polyline([(x - w * 0.6, yy), (x + w * 0.6, yy)]), 4.2) & m
        p.tone(hp, iron, "base")
        p.tone(hp - hp.shift(0, 1.4), iron, "light")
        p.tone(p.where(hp, lambda xx, yy_: xx > x + w * 0.2), iron, "shadow")
    if lid:
        top = Path.ellipse(x, ground - h, w * 0.42, 5)
        lm = p.fill(top, t, "light")
        p.stroke(Path.polyline([(x - w * 0.2, ground - h), (x + w * 0.25, ground - h)]), 0.9, t, "shadow", clip=lm)
        p.edge(top, t, 1.1)
    p.ground_shadow(x - w * 0.6, x + w * 0.6, ground + 1, 6, 0.32)
    if piece is None and layer is not None:
        layer.place(p)
    return p


def sacks(layer, x, ground, n=3, seed=0, piece=None):
    """A heap of grain sacks, each a lumpy pillow tied at the neck, shaded by style.shade."""
    p = piece or Piece(x - 30 * n, ground - 70, x + 30 * n, ground + 10)
    t = tones("sack")
    for i in range(n):
        sx = x + (i - (n - 1) / 2) * 30 + (hash01(i, 1, seed + 791) - 0.5) * 6
        h = 40 + 10 * hash01(i, 2, seed + 791)
        body = Path.smooth([(sx - 16, ground), (sx - 18, ground - h * 0.5), (sx - 8, ground - h * 0.92), (sx - 3, ground - h),
                            (sx + 4, ground - h), (sx + 9, ground - h * 0.9), (sx + 18, ground - h * 0.45), (sx + 16, ground)], closed=True)
        m = p.shape(body)
        style.shade(m, "cloth", t, edge=INNER, rim=False)
        p.stroke(Path.polyline([(sx - 6, ground - h * 0.82), (sx + 6, ground - h * 0.84)]), 2.2, tones("rope"), "base")
    p.ground_shadow(x - 18 * n, x + 18 * n, ground + 1, 6, 0.3)
    if piece is None and layer is not None:
        layer.place(p)
    return p


def basket(p, x, ground, w=40, h=20, goods="apple", seed=0):
    """A wicker basket of goods on a counter or the ground (into a Piece)."""
    t = tones("wood.light")
    g = tones(goods)
    heap = p.nothing()
    for i in range(int(w / 8)):
        heap = heap | p.shape(Path.circle(x - w / 2 + 5 + i * 8, ground - h - 1 - 3 * hash01(i, 1, seed + 795), 5))
    for i in range(int(w / 10)):
        heap = heap | p.shape(Path.circle(x - w / 2 + 9 + i * 10, ground - h - 6, 5))
    p.tone(heap, g, "base")
    p.tone(heap - heap.shift(-1.3, -1.5), g, "shadow")
    p.edge_mask(heap, g, 0.9)
    body = Path.polygon([(x - w / 2, ground - h), (x + w / 2, ground - h), (x + w * 0.42, ground), (x - w * 0.42, ground)])
    m = p.fill(body, t, "base")
    for k in range(1, 4):
        p.stroke(Path.polyline([(x - w / 2, ground - h + h * k / 4), (x + w / 2, ground - h + h * k / 4)]), 0.9, t, "deep", clip=m)
    for k in range(int(w / 6)):
        p.stroke(Path.polyline([(x - w / 2 + k * 6, ground - h), (x - w / 2 + k * 6 + 2, ground)]), 0.8, t, "shadow", clip=m)
    p.tone(p.where(m, lambda xx, yy: xx > x + w * 0.25), t, "shadow", alpha=0.6)
    p.edge(body, t, 1.1)


def stall(layer, x, ground, w=170, colours=("cloth.blue", "cloth.cream"), goods=("apple", "orange", "pear"), seed=0, sign=None):
    """A market stall: two posts, a striped awning with a scalloped valance, a counter draped in
    cloth, baskets of goods on it and crates beside it."""
    p = Piece(x - w / 2 - 50, ground - 230, x + w / 2 + 50, ground + 12)
    wood = tones("wood")
    top_y = ground - 200
    for px in (x - w / 2 + 6, x + w / 2 - 6):
        post = Path.rect(px - 4, top_y + 10, 8, ground - top_y - 10)
        pm = p.fill(post, wood, "base")
        p.tone(p.where(pm, lambda xx, yy, a=px: xx > a + 1), wood, "shadow")
        p.edge(post, wood, 1.1)
    # the back cloth, in shadow under the awning
    back = Path.rect(x - w / 2 + 10, top_y + 26, w - 20, ground - top_y - 70)
    bt = tones(colours[1])
    bm = p.fill(back, bt, "shadow")
    for k in range(5):
        fx = x - w / 2 + 18 + k * (w - 36) / 4
        p.stroke(Path.polyline([(fx, top_y + 28), (fx + 2, ground - 46)]), (2.0, 0.6), bt, "deep-shadow", clip=bm)
    # shelves of goods on the back cloth
    shelf = Path.rect(x - w / 2 + 14, top_y + 92, w - 28, 5)
    p.fill(shelf, wood, "base")
    for i in range(int((w - 40) / 22)):
        jx = x - w / 2 + 26 + i * 22
        jar = Path.rect(jx - 6, top_y + 74, 12, 18, r=3)
        jt = tones(("pottery", "terracotta", "#6fbfa0", "cloth.plum")[int(hash01(i, 1, seed + 801) * 4)])
        jm = p.fill(jar, jt, "base")
        p.tone(p.where(jm, lambda xx, yy, a=jx: xx > a + 1), jt, "shadow")
        p.edge(jar, jt, 1.0)
    # the awning
    aw = awning(x - w / 2 - 4, x + w / 2 + 4, top_y + 6, depth=30, colours=colours, valance=9)
    aw(p)
    # the counter: a board on trestles with a cloth hanging in front
    counter_y = ground - 62
    board = Path.rect(x - w / 2 - 2, counter_y - 6, w + 4, 8)
    p.fill(board, wood, "light")
    p.edge(board, wood, 1.1)
    for i, gname in enumerate(goods):
        bx = x - w / 2 + 28 + i * (w - 56) / max(1, len(goods) - 1)
        basket(p, bx, counter_y - 6, 38, 14, gname, seed + i)
    cloth = Path().move(x - w / 2, counter_y + 2).line(x + w / 2, counter_y + 2).line(x + w / 2, ground - 10)
    n = 7
    for i in range(n, -1, -1):
        cx = x - w / 2 + w * i / n
        cloth.line(cx, ground - 10 + (5 if i % 2 else 0))
    cloth.close()
    ct = tones(colours[0])
    cm = p.fill(cloth, ct, "base")
    for k in range(1, n):
        fx = x - w / 2 + w * k / n
        p.stroke(Path.polyline([(fx, counter_y + 4), (fx, ground - 12)]), (3.0, 1.0), ct, "shadow", clip=cm)
    p.tone(p.above(cm, counter_y + 10), ct, "deep")
    trim = cm & p.shape(Path.rect(x - w, counter_y + 10, w * 2, 6))
    p.tone(trim, tones("gold"), "base")
    p.edge(cloth, ct, 1.2)
    if sign:
        board_ = Path.rect(x - 30, top_y - 14, 60, 20, r=4)
        p.fill(board_, tones("cloth.cream"), "base")
        p.edge(board_, tones("wood.dark"), 1.6, "base")
        _icon(p, sign, x, top_y - 4, 16)
    p.ground_shadow(x - w / 2 - 10, x + w / 2 + 10, ground + 2, 9, 0.35)
    layer.place(p)
    return p


def bench(layer, x, ground, w=90, seed=0):
    """A wooden bench with iron legs."""
    p = Piece(x - w / 2 - 6, ground - 50, x + w / 2 + 6, ground + 10)
    wood, iron = tones("wood"), tones("iron")
    for lx in (x - w / 2 + 10, x + w / 2 - 14):
        leg = Path.rect(lx, ground - 30, 5, 30)
        p.fill(leg, iron, "base")
        p.edge(leg, iron, 1.0)
    back = Path.rect(x - w / 2, ground - 48, w, 7, r=2)
    seat = Path.rect(x - w / 2 - 4, ground - 32, w + 8, 7, r=2)
    for part in (back, seat):
        m = p.fill(part, wood, "base")
        p.tone(m - m.shift(0, 2), wood, "light")
        p.edge(part, wood, 1.1)
    p.ground_shadow(x - w * 0.55, x + w * 0.55, ground + 1, 5, 0.3)
    layer.place(p)
    return p


def fence(layer, x0, x1, ground, h=46, kind="picket", colour="wood.light", seed=0):
    """A picket fence (pointed pales on two rails) or wrought-iron railings with gold finials."""
    p = Piece(x0 - 6, ground - h - 16, x1 + 6, ground + 8)
    if kind == "picket":
        t = tones(colour)
        for y in (ground - h * 0.7, ground - h * 0.3):
            rail = Path.rect(x0, y, x1 - x0, 5)
            p.fill(rail, t, "shadow")
        x = x0
        while x < x1:
            pale = Path.polygon([(x, ground), (x, ground - h + 6), (x + 4, ground - h), (x + 8, ground - h + 6), (x + 8, ground)])
            m = p.fill(pale, t, "base")
            p.tone(p.where(m, lambda xx, yy, a=x: xx > a + 5), t, "shadow")
            p.edge(pale, t, 1.0)
            x += 13
    else:
        t = tones("iron")
        g = tones("gold")
        for y in (ground - h + 4, ground - 8):
            p.fill(Path.rect(x0, y, x1 - x0, 3.5), t, "base")
        x = x0
        while x <= x1:
            p.fill(Path.rect(x - 1.4, ground - h, 2.8, h), t, "base")
            p.fill(Path.polygon([(x - 3.4, ground - h), (x, ground - h - 9), (x + 3.4, ground - h)]), g, "base")
            x += 11
    p.ground_shadow(x0, x1, ground + 1, 4, 0.25)
    layer.place(p)
    return p


def low_wall(layer, x0, x1, ground, h=50, colour="stone", seed=0, coping=True, plants=None):
    """A low garden wall of stone with a coping, plants spilling over it if asked."""
    p = Piece(x0 - 10, ground - h - 50, x1 + 10, ground + 10)
    m = wall(p, x0, ground - h, x1, ground, "stone." + colour.split(".")[-1] if "." in colour else "stone", seed, quoins=False)
    del m
    if coping:
        st = tones("stone.grey")
        cp = Path.rect(x0 - 5, ground - h - 8, x1 - x0 + 10, 10, r=2)
        cm = p.fill(cp, st, "base")
        p.tone(cm - cm.shift(0, 2.5), st, "light")
        p.cast(cm, 0, 5, p.covered() - cm, tones(colour), "shadow")
        p.edge(cp, st, 1.2, "shadow")
    if plants:
        bush(None, (x0 + x1) / 2, ground - h - 4, (x1 - x0) * 0.8, 34, "leaf", seed, flowers_on=plants, piece=p)
    p.edge(Path.rect(x0, ground - h, x1 - x0, h), tones(colour), 1.3)
    p.ground_shadow(x0 - 6, x1 + 6, ground + 2, 7, 0.32)
    layer.place(p)
    return p


def lamp_post(layer, x, ground, h=300, lit=True, seed=0, ink=INK, reach=OUTER, dark=0.0):
    """A street lamp: a fluted iron post on a stepped base, a curled crown, a lantern with its glow.
    `dark` (0..1) pulls it toward a silhouette, for the foreground."""
    p = Piece(x - 40, ground - h - 40, x + 40, ground + 10)
    t = tones("iron") if dark <= 0 else haze(tones("iron"), "#140e1c", dark)
    base = Path.polygon([(x - 16, ground), (x - 12, ground - 22), (x - 7, ground - 30), (x + 7, ground - 30), (x + 12, ground - 22), (x + 16, ground)])
    m = p.fill(base, t, "base")
    p.tone(p.where(m, lambda xx, yy: xx > x + 2), t, "shadow")
    post = Path.rect(x - 4, ground - h + 30, 8, h - 58)
    pm = p.fill(post, t, "base")
    p.tone(p.where(pm, lambda xx, yy: xx > x + 1), t, "shadow")
    p.tone(p.where(pm, lambda xx, yy: xx < x - 2), t, "light")
    for yy in (ground - 60, ground - h * 0.55, ground - h + 40):
        ring = Path.rect(x - 6, yy, 12, 5, r=2)
        p.fill(ring, t, "base-light")
    for side in (-1, 1):
        p.stroke(Path.smooth([(x, ground - h + 40), (x + side * 10, ground - h + 34), (x + side * 13, ground - h + 24), (x + side * 8, ground - h + 22)]), 2.2, t, "base")
    p.edge(base, t, 1.2)
    p.edge(post, t, 1.2)
    lantern_body(p, x, ground - h - 4, 1.6, lit)
    if dark <= 0:
        p.ground_shadow(x - 20, x + 20, ground + 1, 5, 0.32)
    layer.place(p, ink=ink, reach=reach)
    return p


def lantern_string(layer, points, sag=40, every=46, colours=("#ff9fb8", "#ffd27a", "#9fd2ff", "#ffffff"), seed=0, lit=True):
    """A string of paper lanterns hung between points (the Lantern Market's): a sagging cord, and
    round paper lanterns along it, each with ribs, a cap, its light glowing through, and a halo."""
    for (ax, ay), (bx, by) in zip(points, points[1:]):
        x0, x1 = min(ax, bx), max(ax, bx)
        p = Piece(x0 - 20, min(ay, by) - 10, x1 + 20, max(ay, by) + sag + 50)
        cord = Path().move(ax, ay).quad((ax + bx) / 2, (ay + by) / 2 + sag * 2, bx, by)
        p.stroke(cord, 1.4, tones("rope"), "deep")
        n = max(1, int(abs(bx - ax) / every))
        for i in range(n):
            u = (i + 0.5) / n
            (lx, ly), _ = cord.at(u)
            c = colours[int(hash01(i, int(ax), seed + 811) * len(colours))]
            t = tones(c)
            r = 8.5 + 2.5 * hash01(i, 2, seed + 811)
            p.stroke(Path.polyline([(lx, ly), (lx, ly + 5)]), 1.0, tones("rope"), "deep")
            body = Path.ellipse(lx, ly + 5 + r, r * 0.9, r)
            m = p.fill(body, t, "light" if lit else "base")
            p.tone(p.where(m, lambda xx, yy, a=lx: xx > a + r * 0.35), t, "base")
            if lit:
                p.tone(m & p.shape(Path.ellipse(lx - r * 0.15, ly + 5 + r * 0.9, r * 0.38, r * 0.5)), t, "highlight")
            for k in (-0.45, 0.0, 0.45):
                p.stroke(Path.smooth([(lx + k * r * 0.5, ly + 6), (lx + k * r * 1.05, ly + 5 + r), (lx + k * r * 0.5, ly + 4 + 2 * r)]), 0.8, t, "terminator", clip=m)
            p.fill(Path.rect(lx - r * 0.45, ly + 3, r * 0.9, 3.5, r=1), tones("wood.dark"), "base")
            p.fill(Path.rect(lx - r * 0.35, ly + 4 + 2 * r, r * 0.7, 3, r=1), tones("wood.dark"), "base")
            p.stroke(Path.polyline([(lx, ly + 7 + 2 * r), (lx, ly + 13 + 2 * r)]), 1.2, tones("cloth.red"), "base")
            p.edge(body, t, 1.1, "deep")
            if lit:
                p.glow(lx, ly + 5 + r, r * 4.2, rgb(c), 0.32)
        layer.place(p, reach=1.4)


def cart(layer, x, ground, w=150, seed=0, load="crates"):
    """A handcart: a plank bed on a spoked wheel, its shafts down to the ground, loaded."""
    p = Piece(x - w / 2 - 30, ground - 130, x + w / 2 + 30, ground + 10)
    wood = tones("wood")
    bed_y = ground - 48
    if load == "crates":
        crate(None, x - w * 0.18, bed_y - 6, 44, 34, seed, piece=p, top_goods="apple")
        crate(None, x + w * 0.2, bed_y - 6, 40, 30, seed + 1, piece=p, top_goods="orange")
    bed = Path.rect(x - w / 2, bed_y - 8, w, 14, r=2)
    m = p.fill(bed, wood, "base")
    p.tone(m - m.shift(0, 2), wood, "light")
    p.edge(bed, wood, 1.2)
    p.stroke(Path.polyline([(x - w / 2, bed_y), (x - w / 2 - 34, ground - 4)]), 5, wood, "base")
    wx, wy, r = x + w * 0.12, ground - 26, 26
    wm = p.shape(Path.circle(wx, wy, r)) - p.shape(Path.circle(wx, wy, r - 5))
    p.tone(wm, tones("wood.dark"), "base")
    for k in range(8):
        a = math.radians(k * 45)
        p.stroke(Path.polyline([(wx, wy), (wx + math.cos(a) * (r - 4), wy + math.sin(a) * (r - 4))]), 2.2, tones("wood.dark"), "base")
    p.fill(Path.circle(wx, wy, 4.5), tones("iron"), "base")
    p.edge_mask(wm, tones("wood.dark"), 1.0)
    p.ground_shadow(x - w * 0.6, x + w * 0.5, ground + 1, 7, 0.32)
    layer.place(p)
    return p


def well(layer, x, ground, seed=0):
    """A stone well with a little roof, a windlass and a bucket."""
    p = Piece(x - 70, ground - 200, x + 70, ground + 10)
    st = tones("stone")
    body = Path.rect(x - 46, ground - 52, 92, 52, r=4)
    wall(p, x - 46, ground - 52, x + 46, ground, "stone", seed, quoins=False)
    p.edge(body, st, 1.3)
    rim = Path.rect(x - 52, ground - 60, 104, 10, r=3)
    rm = p.fill(rim, tones("stone.grey"), "base")
    p.tone(rm - rm.shift(0, 2.4), tones("stone.grey"), "light")
    p.edge(rim, tones("stone.grey"), 1.1, "shadow")
    wood = tones("wood")
    for px in (x - 42, x + 36):
        post = Path.rect(px, ground - 150, 7, 92)
        p.fill(post, wood, "base")
        p.edge(post, wood, 1.1)
    roof_p = Path.polygon([(x - 64, ground - 140), (x, ground - 190), (x + 64, ground - 140), (x + 56, ground - 134), (x, ground - 176), (x - 56, ground - 134)])
    rt = tones("roof.blue")
    rmk = p.fill(roof_p, rt, "base")
    p.tone(p.where(rmk, lambda xx, yy: xx > x), rt, "shadow")
    p.edge(roof_p, rt, 1.3)
    p.stroke(Path.polyline([(x - 38, ground - 112), (x + 38, ground - 112)]), 4, wood, "deep")
    p.stroke(Path.polyline([(x + 4, ground - 112), (x + 4, ground - 84)]), 1.2, tones("rope"), "base")
    bucket = Path.polygon([(x - 6, ground - 84), (x + 14, ground - 84), (x + 12, ground - 68), (x - 4, ground - 68)])
    p.fill(bucket, wood, "base")
    p.edge(bucket, wood, 1.0)
    p.ground_shadow(x - 60, x + 60, ground + 2, 8, 0.33)
    layer.place(p)
    return p


def lantern_pole(layer, x, ground, h=380):
    """The square's tall lantern pole: a painted wooden mast on a stone foot, a gold finial, and a
    ring of lanterns at its head that the strings run out from."""
    p = Piece(x - 40, ground - h - 40, x + 40, ground + 10)
    wood = tones("wood.blue")
    st = tones("stone.grey")
    foot = Path.polygon([(x - 22, ground + 2), (x - 18, ground - 26), (x + 18, ground - 26), (x + 22, ground + 2)])
    m = p.fill(foot, st, "base")
    p.tone(p.where(m, lambda xx, yy: xx > x + 6), st, "shadow")
    p.edge(foot, st, 1.2, "shadow")
    mast = Path.rect(x - 5, ground - h, 10, h - 24)
    mm = p.fill(mast, wood, "base")
    p.tone(p.where(mm, lambda xx, yy: xx > x + 1.5), wood, "shadow")
    p.tone(p.where(mm, lambda xx, yy: xx < x - 2.5), wood, "light")
    for yy in (ground - 120, ground - h + 40):
        p.fill(Path.rect(x - 7, yy, 14, 6, r=2), tones("gold"), "base")
    p.edge(mast, wood, 1.2)
    _finial(p, x, ground - h, tones("gold"), tall=True)
    lantern_body(p, x - 18, ground - h + 22, 0.9, True)
    lantern_body(p, x + 18, ground - h + 22, 0.9, True)
    p.stroke(Path.polyline([(x - 20, ground - h + 22), (x + 20, ground - h + 22)]), 2.0, tones("iron"), "base")
    p.ground_shadow(x - 26, x + 26, ground + 2, 5, 0.32)
    layer.place(p)


def fore_foliage(layer, x, ground, w, h, seed=0, dark=0.5):
    """A dark heap of leaves and flowers rising into the bottom of the picture, in front of everyone."""
    p = Piece(x - w / 2 - 10, ground - h - 10, x + w / 2 + 10, ground + 10)
    t = haze(tones("leaf.dark"), "#140e1c", dark)
    clumps = []
    n = max(3, int(w / 34))
    for i in range(n):
        u = (i + 0.5) / n
        bump = math.sin(math.pi * u)
        rr = h * (0.3 + 0.25 * bump) * (0.85 + 0.3 * hash01(i, 1, seed))
        clumps.append((x - w / 2 + w * u, ground - rr * 0.6 - bump * h * 0.32, rr, 0))
    clumps.sort(key=lambda c: -c[2])
    whole = _clumps(p, clumps, t)
    for i in range(int(w / 12)):
        fx = x - w / 2 + w * hash01(i, 2, seed)
        fy = ground - h * 0.9 * hash01(i, 3, seed) ** 0.6
        c = haze(tones(("flower.pink", "flower.white", "flower.violet")[i % 3]), "#140e1c", dark * 0.8)
        fm = p.fill(Path.circle(fx, fy, 3.4), c, "base", clip=whole)
        p.tone(fm - fm.shift(-0.8, -0.8), c, "shadow")
    layer.place(p, reach=2.6)


# --------------------------------------------------------------------------- #
# Civic: towers, gatehouses, arches, stairs, balustrades, balconies, notice boards
# --------------------------------------------------------------------------- #

def _cylinder_shade(p, x0, x1, y0, y1, t, clip=None):
    """Shades a region as the side of a round tower: a lit band on the left, the shadow side on
    the right, with a darker terminator between, hard-edged."""
    w = x1 - x0
    region = p.shape(Path.rect(x0, y0, w, y1 - y0))
    if clip is not None:
        region = region & clip
    p.tone(p.where(region, lambda xx, yy: xx > x0 + w * 0.62), t, "shadow", alpha=0.85)
    p.tone(p.where(region, lambda xx, yy: (xx > x0 + w * 0.56) & (xx <= x0 + w * 0.62)), t, "terminator", alpha=0.7)
    p.tone(p.where(region, lambda xx, yy: xx < x0 + w * 0.16), t, "light", alpha=0.7)


def tower(layer, cx, ground, w=90, h=320, roof_colour="roof.blue", roof_h=None, material="stone", seed=0,
          slits=3, crown="cone", banner=None, piece=None):
    """A round tower standing on the ground: courses of stone shaded as a cylinder (lit left,
    shadowed right), arrow slits, a corbelled rim, and a conical tiled roof with a gold finial (or
    battlements, crown="battlements"). A pennant on its finial if `banner` is a colour."""
    roof_h = roof_h if roof_h is not None else w * 1.35
    top = ground - h
    p = piece or Piece(cx - w, top - roof_h - 50, cx + w, ground + 10)
    kind, colour = _material(material)
    t = tones(colour)
    body = Path.rect(cx - w / 2, top, w, h)
    q = p.sub(cx - w / 2 - 2, top - 2, cx + w / 2 + 2, ground + 2)
    wall(q, cx - w / 2, top, cx + w / 2, ground, material, seed, quoins=False)
    _cylinder_shade(q, cx - w / 2, cx + w / 2, top, ground, t)
    for i in range(slits):
        sy = top + 40 + (h - 120) * (i + 0.5) / max(1, slits)
        sl = Path.rect(cx - 4, sy, 8, 30, r=3)
        q.fill(Path.rect(cx - 8, sy - 4, 16, 38, r=4), t, "base-light")
        q.fill(sl, tones("#2c2440"), "base")
        q.edge(Path.rect(cx - 8, sy - 4, 16, 38, r=4), t, 1.0, "shadow")
    p.stamp(q)
    p.edge(body, t, INNER)
    rim = Path.rect(cx - w / 2 - 8, top - 10, w + 16, 14, r=2)
    rm = p.fill(rim, t, "base-light")
    for i in range(int((w + 16) / 14)):
        bx = cx - w / 2 - 8 + 7 + i * 14
        p.fill(Path.polygon([(bx - 4, top + 4), (bx + 4, top + 4), (bx, top + 12)]), t, "shadow")
    p.cast(rm, 0, 5, p.shape(body), t, "shadow")
    p.edge(rim, t, 1.2, "shadow")
    if crown == "battlements":
        for i in range(int((w + 16) / 18) + 1):
            if i % 2 == 0:
                mx = cx - w / 2 - 8 + i * 18
                merl = Path.rect(mx, top - 30, 13, 22)
                p.fill(merl, t, "base")
                p.tone(p.where(p.shape(merl), lambda xx, yy: xx > cx + w * 0.12), t, "shadow")
                p.edge(merl, t, 1.2)
    else:
        house = House(cx - w / 2, w, top + 4, floors=((material, 0.001),), roof="cone", roof_colour=roof_colour, roof_h=roof_h, eave=10, seed=seed)
        roof(p, house)
        if banner:
            tipx, tipy = cx, top + 4 - roof_h - 30
            pen = Path.polygon([(tipx + 1, tipy), (tipx + 34, tipy + 6), (tipx + 1, tipy + 13)])
            p.fill(pen, tones(banner), "base")
            p.edge(pen, tones(banner), 1.0)
    p.ground_shadow(cx - w * 0.6, cx + w * 0.6, ground + 2, 8, 0.33)
    if piece is None:
        layer.place(p)
    return p


def archway(p, x, ground, w, h, t, depth=16, keystone=True, opening="open", wood="wood.dark", gold=False):
    """A big round-headed arch into a wall (on a Piece): voussoirs round it, a keystone, the reveal
    in shadow down its left and across its top, and inside it `opening`: "open" (cut away, so what's
    behind shows through), "doors" (two tall studded leaves), "portcullis" (an iron grille over the
    dark) or "dark". Returns the opening's rect (x, y, w, h)."""
    r = w / 2
    top = ground - h
    outer = Path().move(x - 16, ground).line(x - 16, top + r).arc(x + r, top + r, r + 16, 180, 360).line(x + w + 16, ground).close()
    om = p.fill(outer, t, "base-light")
    n = 11
    for i in range(1, n):
        a = math.radians(180 + 180 * i / n)
        p.stroke(Path.polyline([(x + r + math.cos(a) * r, top + r + math.sin(a) * r),
                                (x + r + math.cos(a) * (r + 16), top + r + math.sin(a) * (r + 16))]), 1.2, t, "shadow", clip=om)
    if keystone:
        key = Path.polygon([(x + r - 10, top - 20), (x + r + 10, top - 20), (x + r + 7, top + 6), (x + r - 7, top + 6)])
        p.fill(key, tones("gold") if gold else t, "light")
        p.edge(key, tones("gold") if gold else t, 1.1, "shadow")
    p.edge(outer, t, 1.4, "terminator")
    hole = _opening("arch", x, top, w, h)
    reveal = p.shape(hole)
    if opening == "open":
        p.c.erase(reveal.a)
        band = reveal - reveal.shift(depth * 0.55, depth * 0.45)
        p.tone(band, t, "shadow")
        p.edge_mask(band, t, 1.0, "deep")
    else:
        if opening == "doors":
            wt = tones(wood)
            m = p.fill(hole, wt, "base")
            for k in range(1, int(w / 14)):
                p.stroke(Path.polyline([(x + k * 14, top), (x + k * 14, ground)]), 1.1, wt, "deep", clip=m)
            p.stroke(Path.polyline([(x + r, top), (x + r, ground)]), 3.0, wt, "line", clip=m)
            iron = tones("iron")
            for yy in (top + h * 0.35, top + h * 0.72):
                band = p.shape(Path.rect(x, yy, w, 7)) & m
                p.tone(band, iron, "base")
                for i in range(int(w / 16)):
                    style.rivet(p.c, x + 8 + i * 16 - p.ox, yy + 3.5 - p.oy, iron, r=1.6, clip=m)
            for hx in (x + r - 12, x + r + 12):
                p.stroke(Path.circle(hx, top + h * 0.55, 5), 1.8, tones("brass"), "base")
            if gold:
                g = tones("gold")
                p.stroke(Path.polyline([(x + r, top + 6), (x + r, ground)]), 2.0, g, "base", clip=m)
                _icon(p, "sun", x + r, top + r * 0.75, r * 0.7)
            p.tone(m - m.shift(depth * 0.45, depth * 0.45), wt, "deep")
        else:
            dark = tones("#2a2236")
            m = p.fill(hole, dark, "base")
            p.tone(p.below(m, ground - h * 0.25), dark, "light", alpha=0.6)
            if opening == "portcullis":
                iron = tones("iron")
                for k in range(1, int(w / 16)):
                    p.stroke(Path.polyline([(x + k * 16, top), (x + k * 16, ground - h * 0.42)]), 3.0, iron, "base", clip=m)
                for k in range(1, int(h * 0.58 / 18)):
                    p.stroke(Path.polyline([(x, top + k * 18), (x + w, top + k * 18)]), 2.6, iron, "base", clip=m)
                for k in range(1, int(w / 16)):
                    p.fill(Path.polygon([(x + k * 16 - 3, ground - h * 0.42), (x + k * 16 + 3, ground - h * 0.42), (x + k * 16, ground - h * 0.42 + 9)]), iron, "base")
        p.edge(hole, t, 1.4, "deep")
    return (x, top, w, h)


def gatehouse(layer, x0, x1, ground, arch_x, arch_w, arch_h, h=None, roof_colour="roof.blue", seed=0, opening="open",
              towers=True, tower_w=96, banners=("cloth.pink", "cloth.navy"), material="stone", gold=False, sign=None):
    """A gatehouse from x0 to x1: a block of dressed stone with battlements, a great arch (archway)
    at arch_x, banners either side, and round towers with conical roofs at its ends (towers=True).
    `opening` says what's in the arch. Returns the arch's opening, (x, y, w, h)."""
    h = h if h is not None else arch_h + 150
    top = ground - h
    p = Piece(x0 - tower_w, top - tower_w * 1.6 - 90, x1 + tower_w, ground + 14)
    kind, colour = _material(material)
    t = tones(colour)
    q = p.sub(x0 - 2, top - 40, x1 + 2, ground + 2)
    wall(q, x0, top, x1, ground, material, seed, quoins=False)
    for i in range(int((x1 - x0) / 26) + 1):
        if i % 2 == 0:
            mx = x0 + i * 26
            merl = Path.rect(mx, top - 26, 18, 28)
            q.fill(merl, t, "base")
            q.edge(merl, t, 1.2)
    cornice = Path.rect(x0 - 6, top - 4, x1 - x0 + 12, 12, r=2)
    cm = q.fill(cornice, t, "base-light")
    q.cast(cm, 0, 6, q.covered() - cm, t, "shadow")
    q.edge(cornice, t, 1.1, "shadow")
    opening_rect = archway(q, arch_x, ground, arch_w, arch_h, t, opening=opening, gold=gold)
    p.stamp(q)
    p.edge(Path.rect(x0, top, x1 - x0, h), t, INNER)
    if banners:
        for k, side in enumerate((-1, 1)):
            bx = arch_x + arch_w / 2 + side * (arch_w / 2 + 52)
            if x0 + 24 < bx < x1 - 24:
                f = banner(bx, top + 30, 34, min(130, h * 0.42), banners[k % len(banners)], "sun" if gold else "diamond")
                p.part(f.box, f)
    if sign:
        p.part((arch_x - 10, top + 6, arch_x + arch_w + 10, top + 60), lambda q2: _gate_plaque(q2, arch_x + arch_w / 2, top + 26, sign))
    if towers:
        for cx in (x0 - tower_w * 0.25, x1 + tower_w * 0.25):
            tower(None, cx, ground, tower_w, h + 50, roof_colour, tower_w * 1.4, material, seed + int(cx), slits=2, piece=p, banner="cloth.pink")
    p.ground_shadow(x0 - tower_w * 0.6, x1 + tower_w * 0.6, ground + 3, 10, 0.36)
    layer.place(p)
    return opening_rect


def _gate_plaque(p, cx, cy, icon):
    g = tones("gold")
    board = Path.rect(cx - 46, cy - 16, 92, 32, r=5)
    p.fill(board, tones("cloth.navy"), "base")
    p.edge(board, g, 2.4, "base")
    _icon(p, icon, cx, cy, 26)


def balustrade(layer, x0, x1, ground, h=56, colour="stone", posts=140, urns=True, seed=0, gap=None, piece=None):
    """A stone balustrade: a plinth, a row of turned balusters, a top rail, and a post every
    `posts` pixels with an urn of flowers on it. `gap` (x0, x1) leaves an opening (a stair head)."""
    t = tones(colour)
    p = piece or Piece(x0 - 20, ground - h - 50, x1 + 20, ground + 10)
    spans = [(x0, x1)] if not gap else [(x0, gap[0]), (gap[1], x1)]
    for a, b in spans:
        if b - a < 10:
            continue
        plinth = Path.rect(a, ground - 12, b - a, 12)
        p.fill(plinth, t, "shadow")
        p.edge(plinth, t, 1.0)
        bx = a + 10
        while bx < b - 8:
            bal = Path.smooth([(bx - 4, ground - 12), (bx - 6, ground - h * 0.35), (bx - 3, ground - h * 0.62), (bx - 4.5, ground - h + 14),
                               (bx + 4.5, ground - h + 14), (bx + 3, ground - h * 0.62), (bx + 6, ground - h * 0.35), (bx + 4, ground - 12)], closed=True)
            m = p.fill(bal, t, "base")
            p.tone(p.where(m, lambda xx, yy, c=bx: xx > c + 1.2), t, "shadow")
            p.edge(bal, t, 1.0)
            bx += 15
        rail = Path.rect(a - 4, ground - h, b - a + 8, 14, r=2)
        rm = p.fill(rail, t, "base-light")
        p.tone(rm - rm.shift(0, 2.5), t, "light")
        p.edge(rail, t, 1.2, "shadow")
        n = max(1, int(round((b - a) / posts)))
        for k in range(n + 1):
            x = a + (b - a) * k / n
            post = Path.rect(x - 11, ground - h - 6, 22, h + 6)
            pm = p.fill(post, t, "base")
            p.tone(p.where(pm, lambda xx, yy, c=x: xx > c + 4), t, "shadow")
            cap = Path.rect(x - 14, ground - h - 12, 28, 9, r=2)
            p.fill(cap, t, "light")
            p.edge(post, t, 1.2)
            p.edge(cap, t, 1.0, "shadow")
            if urns and hash01(int(x), 1, seed + 841) < 0.8:
                pot(None, x, ground - h - 12, 22, 18, "stone.warm", piece=p, plant="flowers", seed=seed + int(x))
    p.ground_shadow(x0, x1, ground + 1, 6, 0.28)
    if piece is None:
        layer.place(p)
    return p


def stairs(layer, x, ground, w=230, rise=170, steps=9, colour="stone", seed=0, rails=True):
    """A broad flight of steps going up from the floor's back edge into the distance, as a stair to
    another room is painted: each step's lit tread and shadowed riser, narrowing a little as they
    rise, balustraded walls up both sides, and a landing at the top. Returns the stair's foot box."""
    t = tones(colour)
    p = Piece(x - w / 2 - 70, ground - rise - 120, x + w / 2 + 70, ground + 12)
    sh = rise / steps
    for i in range(steps):
        k = 1.0 - 0.18 * i / steps
        y = ground - i * sh
        half = w / 2 * k
        riser = Path.rect(x - half, y - sh * 0.62, half * 2, sh * 0.62)
        tread = Path.rect(x - half, y - sh, half * 2, sh * 0.38)
        p.fill(riser, t, "shadow")
        p.fill(tread, t, "light")
        p.stroke(Path.polyline([(x - half, y - sh * 0.62), (x + half, y - sh * 0.62)]), 1.2, t, "deep")
        for j in range(int(half * 2 / 60)):
            jx = x - half + 30 + j * 60 + (hash01(i, j, seed + 851) - 0.5) * 20
            p.stroke(Path.polyline([(jx, y - sh * 0.6), (jx, y)]), 1.0, t, "terminator")
    landing = Path.rect(x - w / 2 * 0.82, ground - rise - 16, w * 0.82, 16)
    p.fill(landing, t, "base-light")
    p.edge(Path.rect(x - w / 2, ground - rise - 16, w, rise + 16), t, 1.4)
    if rails:
        for side in (-1, 1):
            wx = x + side * (w / 2 + 14)
            wallp = Path.polygon([(wx - 16, ground), (wx + 16, ground), (wx + 16 - side * 22, ground - rise - 40), (wx - 16 - side * 22, ground - rise - 40)])
            m = p.fill(wallp, t, "base")
            p.tone(p.where(m, lambda xx, yy, c=wx: xx > c + 4), t, "shadow")
            p.edge(wallp, t, 1.3)
            cap = Path.polygon([(wx - 20, ground - 54), (wx + 20, ground - 54), (wx + 20 - side * 22, ground - rise - 52), (wx - 20 - side * 22, ground - rise - 52)])
            p.fill(cap, t, "light")
            p.edge(cap, t, 1.1, "shadow")
            post = Path.rect(wx - 18, ground - 74, 36, 74)
            p.fill(post, t, "base-light")
            p.edge(post, t, 1.2)
            pot(None, wx, ground - 74, 24, 20, "stone.warm", piece=p, plant="flowers", seed=seed + side)
    p.ground_shadow(x - w / 2 - 30, x + w / 2 + 30, ground + 2, 8, 0.3)
    layer.place(p)
    return (x - w / 2, ground - rise, w, rise)


def balcony(x, y, w=150, colour="stone", gold=True):
    """A balcony on corbels across a building's front at height y, with a balustrade: a house extra."""

    def draw(p, house=None):
        t = tones(colour)
        slab = Path.rect(x - w / 2 - 6, y, w + 12, 12, r=2)
        sm = p.fill(slab, t, "base-light")
        p.cast(sm, 0, 8, p.covered() - sm, t, "shadow")
        p.edge(slab, t, 1.2, "shadow")
        for i in range(5):
            cx = x - w / 2 + 10 + i * (w - 20) / 4
            cor = Path().move(cx - 7, y + 12).line(cx + 7, y + 12).quad(cx + 7, y + 34, cx, y + 38).quad(cx - 3, y + 26, cx - 7, y + 12).close()
            p.fill(cor, t, "base")
            p.edge(cor, t, 1.0)
        balustrade(None, x - w / 2, x + w / 2, y, 44, colour, posts=w / 2, urns=False, piece=p)
        if gold:
            p.stroke(Path.polyline([(x - w / 2 - 4, y - 44), (x + w / 2 + 4, y - 44)]), 2.0, tones("gold"), "base")
    draw.box = (x - w / 2 - 30, y - 70, x + w / 2 + 30, y + 50)
    return draw


def notice_board(layer, x, ground, w=110, seed=0, emblem="shield"):
    """The Watch's notice board: a board on two posts under a little tiled roof, notices pinned to
    it (one with a red seal), and the Watch's blue shield on its gable."""
    p = Piece(x - w / 2 - 20, ground - 200, x + w / 2 + 20, ground + 10)
    wood = tones("wood")
    for px in (x - w / 2 + 8, x + w / 2 - 8):
        post = Path.rect(px - 5, ground - 170, 10, 170)
        m = p.fill(post, wood, "base")
        p.tone(p.where(m, lambda xx, yy, c=px: xx > c + 1), wood, "shadow")
        p.edge(post, wood, 1.1)
    board = Path.rect(x - w / 2, ground - 160, w, 96, r=2)
    bm = p.fill(board, tones("wood.light"), "base")
    p.tone(bm - bm.inset(5), wood, "base")
    p.edge(board, wood, 1.3)
    for i in range(6):
        nx = x - w / 2 + 14 + (i % 3) * (w - 28) / 3 + 6 * hash01(i, 1, seed + 861)
        ny = ground - 150 + (i // 3) * 42 + 4 * hash01(i, 2, seed + 861)
        nw, nh = 22 + 6 * hash01(i, 3, seed + 861), 30
        paper = Path.polygon([(nx, ny), (nx + nw, ny + 1), (nx + nw - 1, ny + nh), (nx + 1, ny + nh - 1)])
        pt = tones("cloth.cream")
        p.fill(paper, pt, "base")
        for k in range(3):
            p.stroke(Path.polyline([(nx + 4, ny + 8 + k * 7), (nx + nw - 5, ny + 8 + k * 7)]), 1.0, pt, "terminator")
        p.edge(paper, pt, 0.9, "shadow")
        p.fill(Path.circle(nx + nw / 2, ny + 2, 1.8), tones("cloth.red") if i == 1 else tones("iron"), "base")
    rf = Path.polygon([(x - w / 2 - 16, ground - 158), (x, ground - 196), (x + w / 2 + 16, ground - 158), (x + w / 2 + 8, ground - 152), (x, ground - 184), (x - w / 2 - 8, ground - 152)])
    rt = tones("roof.blue")
    rm = p.fill(rf, rt, "base")
    p.tone(p.where(rm, lambda xx, yy: xx > x), rt, "shadow")
    p.edge(rf, rt, 1.3)
    sh = Path().move(x - 9, ground - 186).line(x + 9, ground - 186).line(x + 9, ground - 176).quad(x + 9, ground - 168, x, ground - 164).quad(x - 9, ground - 168, x - 9, ground - 176).close()
    p.fill(sh, tones("cloth.navy"), "base")
    p.edge(sh, tones("gold"), 1.4, "base")
    p.ground_shadow(x - w / 2 - 6, x + w / 2 + 6, ground + 2, 6, 0.3)
    layer.place(p)
    return p


def arcade(layer, x0, x1, ground, h=200, arches=4, colour="stone", seed=0, roof_colour="roof.blue"):
    """A colonnade: a row of round arches on columns under a tiled lean-to roof, the walk behind
    it in shadow with a lit window or two."""
    t = tones(colour)
    p = Piece(x0 - 20, ground - h - 90, x1 + 20, ground + 10)
    back = Path.rect(x0, ground - h, x1 - x0, h)
    bm = p.fill(back, t, "shadow")
    p.tone(p.below(bm, ground - 30), t, "deep-shadow")
    aw = (x1 - x0) / arches
    holes = []
    for i in range(arches):
        ax = x0 + i * aw + 16
        holes.append(_opening("arch", ax, ground - h + 46, aw - 32, h - 46))
        if hash01(i, 1, seed + 871) < 0.5:
            window(p, Window(ax + (aw - 32) / 2, ground - h + 100, 26, 40, "arch", lit=True, sill=False), t, seed + i)
    q = p.sub(x0 - 2, ground - h - 2, x1 + 2, ground + 2)
    wall(q, x0, ground - h, x1, ground, colour if colour.startswith("stone") else "stone", seed, quoins=False)
    for hole in holes:
        hm = q.shape(hole)
        q.c.erase(hm.a)
        q.tone(hm.grow(4) - hm, t, "base-light")
        q.edge_mask(hm.grow(4) - hm, t, 1.0, "terminator")
    p.stamp(q)
    lean = Path.polygon([(x0 - 14, ground - h + 6), (x1 + 14, ground - h + 6), (x1 + 4, ground - h - 50), (x0 - 4, ground - h - 50)])
    rt = tones(roof_colour)
    rm = p.fill(lean, rt, "base")
    _roof_tiles(p, rm, rt, x0 - 14, x1 + 14, ground - h - 50, ground - h + 2, seed)
    p.edge(lean, rt, 1.3)
    p.ground_shadow(x0 - 10, x1 + 10, ground + 2, 8, 0.33)
    layer.place(p)
    return p


# --------------------------------------------------------------------------- #
# Interiors
# --------------------------------------------------------------------------- #

def back_wall(layer, x0, x1, top, floor_y, kind="plaster", colour=None, panel="wood", seed=0, dado=0.34):
    """An interior's back wall from top to the floor line: plaster, stone or wood boards above, a
    wainscot of framed wood panels for the lower `dado` share, a skirting board, and a soft
    darkening toward the ceiling."""
    p = Piece(x0, top, x1, floor_y + 6, ss=2, pad=2)
    spec = {"plaster": "plaster", "stone": "stone", "timber": "timber", "boards": "plaster"}[kind]
    if colour:
        spec = spec + "." + colour
    m = wall(p, x0, top, x1, floor_y, spec, seed, quoins=False)
    if kind == "boards":
        wt = tones(panel)
        p.tone(m, wt, "base")
        for i in range(int((x1 - x0) / 26) + 1):
            bx = x0 + i * 26
            p.stroke(Path.polyline([(bx, top), (bx, floor_y)]), 1.2, wt, "deep", clip=m)
            p.stroke(Path.polyline([(bx + 2, top), (bx + 2, floor_y)]), 0.9, wt, "light", clip=m, alpha=0.6)
    wt = tones(panel)
    dy = floor_y - (floor_y - top) * dado
    if dado > 0:
        dm = p.fill(Path.rect(x0, dy, x1 - x0, floor_y - dy), wt, "base")
        pw = 90
        for i in range(int((x1 - x0) / pw) + 1):
            px = x0 + i * pw + 10
            pn = Path.rect(px, dy + 14, pw - 20, floor_y - dy - 34, r=3)
            pm = p.shape(pn) & dm
            p.tone(pm, wt, "terminator")
            p.tone(pm - pm.shift(2.5, 2.5), wt, "deep-shadow")
            p.tone(pm - pm.shift(-2, -2), wt, "light")
        rail = Path.rect(x0, dy - 4, x1 - x0, 9)
        rm = p.fill(rail, wt, "base-light")
        p.tone(rm - rm.shift(0, 2), wt, "highlight")
        p.cast(rm, 0, 5, m - rm, tones(_material(spec)[1]), "shadow")
        p.edge(rail, wt, 1.0)
    sk = Path.rect(x0, floor_y - 14, x1 - x0, 14)
    p.fill(sk, tones("wood.dark"), "base")
    p.edge(sk, tones("wood.dark"), 1.0)
    layer.place(p, reach=0)
    v = np.clip((np.arange(layer.h) - top) / max(1.0, (floor_y - top) * 0.5), 0, 1)
    shade_a = (1.0 - v) * 0.28
    a = np.repeat(shade_a[:, None], max(0, int(x1 - x0)), axis=1) * (np.arange(layer.h)[:, None] >= top) * (np.arange(layer.h)[:, None] < floor_y)
    layer.cover(a, "#2a2040", int(x0), 0)


def beams(layer, x0, x1, y, posts=(), h=26, seed=0):
    """Ceiling beams: a heavy beam along the top of the room, and posts down to the floor with
    brackets, each lit on its top and left edges."""
    p = Piece(x0, y - 10, x1, layer.h, ss=2, pad=4)
    t = tones("wood.dark")
    bm = p.fill(Path.rect(x0, y, x1 - x0, h), t, "base")
    p.tone(bm - bm.shift(0, 3), t, "light")
    for i in range(int((x1 - x0) / 60)):
        p.stroke(Path.polyline([(x0 + i * 60 + 20, y + 8), (x0 + i * 60 + 48, y + 10)]), (1.0, 0.0), t, "deep", clip=bm)
    p.edge(Path.rect(x0, y, x1 - x0, h), t, 1.4)
    for (px, foot) in posts:
        post = Path.rect(px - 9, y + h - 2, 18, foot - y - h + 2)
        pm = p.fill(post, t, "base")
        p.tone(p.where(pm, lambda xx, yy, a=px: xx > a + 3), t, "shadow")
        p.tone(p.where(pm, lambda xx, yy, a=px: xx < a - 6), t, "light")
        p.edge(post, t, 1.3)
        for side in (-1, 1):
            br = Path.polygon([(px + side * 9, y + h), (px + side * 40, y + h), (px + side * 9, y + h + 34)])
            p.fill(br, t, "base" if side < 0 else "shadow")
            p.edge(br, t, 1.2)
    layer.place(p)


def shelf(layer, x, y, w=160, items=("jar", "bottle", "book", "plate"), seed=0):
    """A wall shelf on two brackets, with things on it: jars, bottles, books, plates, a candle."""
    p = Piece(x - w / 2 - 10, y - 70, x + w / 2 + 10, y + 40)
    t = tones("wood")
    board = Path.rect(x - w / 2, y, w, 8)
    bm = p.fill(board, t, "base")
    p.tone(bm - bm.shift(0, 2), t, "light")
    p.edge(board, t, 1.1)
    for bx in (x - w / 2 + 14, x + w / 2 - 22):
        br = Path.polygon([(bx, y + 8), (bx + 8, y + 8), (bx + 8, y + 30), (bx + 2, y + 30)])
        p.fill(br, t, "shadow")
        p.edge(br, t, 1.0)
    cx = x - w / 2 + 12
    i = 0
    while cx < x + w / 2 - 14:
        kind = items[int(hash01(i, 1, seed + 821) * len(items))]
        if kind == "jar":
            jt = tones(("pottery", "terracotta", "#7fbf9f")[i % 3])
            jp = Path.rect(cx, y - 24, 16, 24, r=4)
            jm = p.fill(jp, jt, "base")
            p.tone(p.where(jm, lambda xx, yy, a=cx: xx > a + 10), jt, "shadow")
            p.fill(Path.rect(cx + 2, y - 28, 12, 5, r=1.5), tones("wood.dark"), "base")
            p.edge(jp, jt, 1.0)
            cx += 22
        elif kind == "bottle":
            bt = tones(("#6fd0b0", "#d86fa0", "#7f9ff0")[i % 3])
            bp = Path().move(cx + 4, y - 30).line(cx + 8, y - 30).line(cx + 8, y - 20).quad(cx + 13, y - 16, cx + 12, y).line(cx, y) \
                .quad(cx - 1, y - 16, cx + 4, y - 20).close()
            bm2 = p.fill(bp, bt, "base")
            p.tone(p.where(bm2, lambda xx, yy, a=cx: xx > a + 7), bt, "shadow")
            p.stroke(Path.polyline([(cx + 3, y - 14), (cx + 3, y - 4)]), 1.2, bt, "highlight", clip=bm2)
            p.edge(bp, bt, 1.0)
            cx += 18
        elif kind == "book":
            for k in range(3 + int(hash01(i, 2, seed + 821) * 3)):
                bh = 24 + 8 * hash01(i, k, seed + 823)
                bk = tones(("cloth.red", "cloth.navy", "cloth.teal", "cloth.plum", "wood")[int(hash01(i, k, seed + 825) * 5)])
                bp = Path.rect(cx, y - bh, 7, bh)
                p.fill(bp, bk, "base")
                p.fill(Path.rect(cx, y - bh + 4, 7, 2), tones("gold"), "base")
                p.edge(bp, bk, 0.9)
                cx += 7.5
            cx += 6
        elif kind == "plate":
            pt = tones("#e8eef8")
            pp = Path.ellipse(cx + 12, y - 14, 12, 13)
            pm = p.fill(pp, pt, "base")
            p.tone(pm - p.shape(Path.ellipse(cx + 12, y - 14, 8, 9)), pt, "shadow")
            p.edge(pp, pt, 1.0)
            cx += 28
        else:
            cx += 16
        i += 1
    layer.place(p)
    return p


def fireplace(layer, cx, floor_y, w=220, h=210, seed=0, lit=True):
    """A stone fireplace: jambs and a lintel of big blocks, a wooden mantel with things on it, the
    firebox dark inside, logs and a fire burning with its glow, and soot up the stone."""
    p = Piece(cx - w / 2 - 20, floor_y - h - 40, cx + w / 2 + 20, floor_y + 10)
    st = tones("stone.warm")
    outer = Path.rect(cx - w / 2, floor_y - h, w, h)
    wall(p, cx - w / 2, floor_y - h, cx + w / 2, floor_y, "stone.warm", seed, quoins=False)
    p.edge(outer, st, 1.4)
    ow, oh = w * 0.58, h * 0.56
    op = _opening("flat-arch", cx - ow / 2, floor_y - oh, ow, oh)
    dark = tones("#3a2a30")
    om = p.fill(op, dark, "base")
    p.tone(om - om.shift(6, 6), dark, "deep")
    p.edge(op, st, 2.0, "shadow")
    soot = Path.smooth([(cx - ow * 0.3, floor_y - oh), (cx - ow * 0.15, floor_y - oh - 30), (cx + ow * 0.1, floor_y - oh - 46), (cx + ow * 0.3, floor_y - oh)], closed=True)
    p.fill(soot, dark, "shadow", alpha=0.35)
    mantel = Path.rect(cx - w / 2 - 14, floor_y - h * 0.7, w + 28, 14, r=2)
    mm = p.fill(mantel, tones("wood.dark"), "base")
    p.tone(mm - mm.shift(0, 2.5), tones("wood.dark"), "light")
    p.edge(mantel, tones("wood.dark"), 1.2)
    if lit:
        logs = tones("bark")
        for k in range(3):
            lp = Path.rect(cx - ow * 0.32 + k * 8, floor_y - 18 - k * 6, ow * 0.55, 10, r=5)
            p.fill(lp, logs, "base")
            p.edge(lp, logs, 1.0)
        fl = tones("#ffb04a")
        for k in range(5):
            fx = cx - ow * 0.25 + k * ow * 0.12
            fh = 30 + 22 * hash01(k, 1, seed + 831)
            fp = Path().move(fx - 9, floor_y - 18).quad(fx - 10, floor_y - 18 - fh * 0.6, fx + 2, floor_y - 18 - fh) \
                .quad(fx + 4, floor_y - 18 - fh * 0.5, fx + 9, floor_y - 18).close()
            fm = p.fill(fp, fl, "base")
            p.tone(fm & p.shape(Path.ellipse(fx, floor_y - 24, 5, fh * 0.35)), fl, "highlight")
        p.glow(cx, floor_y - 40, w * 1.4, "#ffa050", 0.4)
        p.glow(cx, floor_y - 40, w * 0.5, "#ffd080", 0.45)
    layer.place(p)
    return p


def table(layer, x, ground, w=150, h=60, cloth=None, seed=0, things=("tankard", "plate", "candle")):
    """A trestle table, a cloth on it if asked, and things on top (tankards, plates, a candle)."""
    p = Piece(x - w / 2 - 10, ground - h - 50, x + w / 2 + 10, ground + 10)
    t = tones("wood")
    for lx in (x - w / 2 + 14, x + w / 2 - 22):
        leg = Path.polygon([(lx, ground - h + 8), (lx + 8, ground - h + 8), (lx + 12, ground), (lx - 4, ground)])
        p.fill(leg, t, "shadow")
        p.edge(leg, t, 1.0)
    topp = Path.rect(x - w / 2, ground - h, w, 10, r=2)
    tm = p.fill(topp, t, "base")
    p.tone(tm - tm.shift(0, 2.5), t, "light")
    p.edge(topp, t, 1.2)
    if cloth:
        ct = tones(cloth)
        cp = Path.polygon([(x - w * 0.3, ground - h), (x + w * 0.3, ground - h), (x + w * 0.24, ground - h + 30), (x - w * 0.24, ground - h + 30)])
        p.fill(cp, ct, "base")
        p.edge(cp, ct, 1.0)
    for i, th in enumerate(things):
        tx = x - w / 2 + 24 + i * (w - 48) / max(1, len(things) - 1)
        if th == "tankard":
            tk = tones("pottery")
            tp = Path.rect(tx - 7, ground - h - 18, 14, 18, r=2)
            p.fill(tp, tk, "base")
            p.stroke(Path.circle(tx + 9, ground - h - 10, 4), 1.6, tk, "base")
            p.edge(tp, tk, 1.0)
        elif th == "plate":
            pt = tones("#e8eef8")
            pp = Path.ellipse(tx, ground - h - 2, 14, 3.5)
            p.fill(pp, pt, "base")
            p.edge(pp, pt, 0.9)
            p.fill(Path.ellipse(tx, ground - h - 6, 7, 4), tones("bread"), "base")
        elif th == "candle":
            cp = Path.rect(tx - 3, ground - h - 22, 6, 22)
            p.fill(cp, tones("cloth.cream"), "base")
            p.edge(cp, tones("cloth.cream"), 0.9)
            p.fill(Path.ellipse(tx, ground - h - 27, 2.4, 5), tones("#ffcf6a"), "light")
            p.glow(tx, ground - h - 27, 50, "#ffc070", 0.35)
    p.ground_shadow(x - w * 0.55, x + w * 0.55, ground + 1, 7, 0.3)
    layer.place(p)
    return p


def counter(layer, x0, x1, ground, h=90, colour="wood", seed=0):
    """A shop or bar counter: a panelled front with a lit top board."""
    p = Piece(x0 - 8, ground - h - 10, x1 + 8, ground + 8)
    t = tones(colour)
    front = Path.rect(x0, ground - h + 10, x1 - x0, h - 10)
    fm = p.fill(front, t, "base")
    for i in range(int((x1 - x0) / 70) + 1):
        pn = Path.rect(x0 + i * 70 + 10, ground - h + 22, 50, h - 42, r=3)
        pm = p.shape(pn) & fm
        p.tone(pm, t, "terminator")
        p.tone(pm - pm.shift(2, 2), t, "deep-shadow")
    topb = Path.rect(x0 - 6, ground - h, x1 - x0 + 12, 12, r=2)
    tm = p.fill(topb, t, "light")
    p.tone(tm - tm.shift(0, 2), t, "highlight")
    p.cast(tm, 0, 6, fm, t, "shadow")
    p.edge(front, t, 1.3)
    p.edge(topb, t, 1.2)
    layer.place(p)
    return p


def interior_window(layer, cx, top, w=90, h=130, seed=0, view="day"):
    """A window in an interior wall, with the day (or night) outside it and light falling in."""
    p = Piece(cx - w, top - 30, cx + w, top + h + 30)
    wnd = Window(cx, top, w, h, "arch", lit=False, panes=3)
    window(p, wnd, tones("plaster"), seed)
    if view == "day":
        p.glow(cx, top + h * 0.5, w * 2.4, "#fff2d0", 0.18)
    layer.place(p)
    return p


# --------------------------------------------------------------------------- #
# The kit sheet: every piece once, for review
# --------------------------------------------------------------------------- #

def kit_sheet(path):
    """Paints one of everything into a sheet: the four skies, the far pieces, houses of every roof
    and wall, the plants, the props, and an interior wall with its furniture."""
    W, H = 2400, 2200
    sheet = Layer(W, H, fill="#2a2440")
    # the four skies with their far layers
    for i, (time, extra) in enumerate((("day", {"sun": (90, 70)}), ("dusk", {"sun": (430, 230)}),
                                       ("night", {"moon": (480, 80)}), ("twilight", {}))):
        s = Layer(580, 420)
        sky(s, time, seed=i, horizon=300, clouds=2, cloud_band=(40, 160), **extra)
        hz = HAZE[time]
        mountains(s, 300, 170, hz, seed=i, k=0.5, count=3)
        if i == 0:
            castle(s, 300, 400, 0.55, hz, k=0.15, seed=i)
        elif i == 1:
            sea(s, 300, 420, hz, seed=i, time=time)
        elif i == 2:
            skyline(s, 0, 580, 400, hz, k=0.45, seed=i, height=(50, 120), lit=0.4)
        else:
            forest_line(s, 0, 580, 380, 60, hz, k=0.4, seed=i)
            hill(s, [(0, 400), (200, 380), (400, 395), (580, 385)], "hill", hz, k=0.3, seed=i, trees=5)
        sheet.put(s.px, 20 + i * 595, 20)
    # houses: every roof, every wall
    g = 1000
    floor_shade(sheet, g, 40, 0.3, 0, W)
    building(sheet, House(40, 220, g, floors=(("stone", 190), ("plaster", 140)), roof="hip", roof_colour="roof.blue",
                          door=Door(150, 72, 132), windows=[Window(80, None, 34, 50, floor=0, box=True), Window(110, None, 34, 50, floor=1, shutters="wood.blue")],
                          chimneys=[(220, 24, 50)], seed=1))
    building(sheet, House(320, 200, g, floors=(("brick", 190), ("timber", 140)), roof="front", roof_colour="roof.pink", finials=True,
                          door=Door(420, 70, 128, "square", "wood.green", surround="timber"), jetty=8,
                          windows=[Window(360, None, 40, 50, "square", lit=True, floor=0)], seed=2,
                          extras=[awning(340, 400, g - 150, 24, ("cloth.teal", "cloth.cream"))]))
    building(sheet, House(580, 160, g, floors=(("stone.grey", 210),), roof="cone", roof_colour="roof.teal", roof_h=170,
                          door=Door(660, 100, 140, "open"), seed=3))
    building(sheet, House(800, 240, g, floors=(("plaster.yellow", 200), ("plaster.pink", 140)), roof="gable", roof_colour="roof.plum",
                          dormers=(920,), door=Door(920, 84, 140, "double", "wood.dark"),
                          windows=[Window(850, None, 40, 60, lit=True, floor=0, box=True), Window(990, None, 40, 60, floor=0)],
                          extras=[hanging_sign(1040, g - 190, "bottle", side=1, shape="oval"), wall_lantern(870, g - 130, -1),
                                  banner(920, g - 330, 30, 90, "cloth.navy", "sun"), wall_sign(920, g - 196, 110, 30, "needle")], seed=4))
    tree(sheet, 1230, g, "oak", 300, seed=5)
    tree(sheet, 1420, g, "blossom", 280, seed=6)
    tree(sheet, 1550, g, "cypress", 300, seed=7)
    bush(sheet, 1680, g, 120, 60, seed=8, flowers_on=("flower.pink", "flower.yellow"))
    topiary(sheet, 1800, g, 120, seed=9)
    lamp_post(sheet, 1880, g, 280)
    well(sheet, 2000, g, seed=10)
    lantern_pole(sheet, 2150, g, 320)
    lantern_string(sheet, [(1880, 730), (2150, 700), (2380, 740)], seed=11)
    # props
    g2 = 1400
    floor_shade(sheet, g2, 40, 0.3, 0, W)
    stall(sheet, 120, g2, 170, seed=12, sign="sun")
    crate(sheet, 280, g2, top_goods="apple")
    crate(sheet, 340, g2, 40, 36)
    barrel(sheet, 410, g2)
    sacks(sheet, 500, g2, 3)
    pot(sheet, 590, g2, plant="flowers")
    bench(sheet, 690, g2)
    fence(sheet, 760, 900, g2, kind="picket")
    fence(sheet, 920, 1060, g2, kind="iron")
    low_wall(sheet, 1080, 1260, g2, plants=("flower.white", "flower.pink"))
    cart(sheet, 1380, g2)
    fore_foliage(sheet, 1600, g2, 200, 80, seed=13, dark=0.5)
    lamp_post(sheet, 1760, g2, 260, dark=0.5)
    # an interior: the back wall, beams, a fireplace, shelves, a window, a table and a counter
    top, fl = 1480, 2160
    back_wall(sheet, 20, 2380, top, fl, "plaster", panel="wood", seed=14)
    interior_window(sheet, 300, top + 120, seed=15)
    fireplace(sheet, 700, fl, seed=16)
    shelf(sheet, 1100, top + 250, 220, seed=17)
    shelf(sheet, 1100, top + 360, 220, items=("book",), seed=18)
    beams(sheet, 20, 2380, top, posts=((520, fl), (1500, fl)), seed=19)
    table(sheet, 1300, fl, cloth="cloth.cream", seed=20)
    counter(sheet, 1700, 2100, fl, seed=21)
    sheet.save(path)


if __name__ == "__main__":
    import argparse
    ap = argparse.ArgumentParser(description="paint.py is a library; run it to paint the kit sheet for review.")
    ap.add_argument("--kit", default=os.path.join(vector.ROOT, "shots", "art", "rooms", "paint-kit.png"),
                    help="where the kit sheet goes (default shots/art/rooms/paint-kit.png)")
    a = ap.parse_args()
    kit_sheet(a.kit)
