"""
The owner's art guide (tools/source/art-guide.png; the alignment notes override the art plan's
"Twilight Cel"): clean dark-brown linework, heavier outside and lighter inside; cel shading in two
to four hard-edged tones; a key light from the upper left with a soft fill and only a thin neutral
rim; subtle texture and wear. Folds (§1.7) and armour details (§1.8) as before, and weave, dirt
and scars. All as functions that paint into a vector.Canvas. Everything paints ramp indices into
a tint group (theme.tones(..., group=g)), so one drawing tints into any colour; a group of 0 bakes
the tones' colours instead.

    t = theme.tones("cloth.woad", group=5)
    body = c.shape(path)
    s = style.shade(body, "cloth", t)          # base, form shadow, light, thin rim, edge line
    style.fold(c, "pipe", (40, 60), 30, 90, t, clip=s.interior)
    style.stitches(c, Path.polyline([(10, 80), (60, 82)]), t, clip=s.interior)

Lines, in texels at density 2 (screen px are half):
    EDGE      1.5   every part's own edge line, inside its fill: the lighter inner line
    OUTLINE   0.3/1.1/1.7  how far the #line silhouette reaches beyond that, up / sides / down, so
                    the heavier outer line is 1.8 on top, 2.6 at the sides and 3.2 underneath
                    (about 2.7 px at the guide's size on a dressed figure, whose plies stack)
    DETAIL    1.2   folds, seams, strands, lashes, tapering to 0

The key light comes from the upper left, mostly from above (LIGHT: +x is the way the character
faces, -y up, +z toward the camera), so a mirrored figure still reads as lit from above. A part's surface is guessed from its outline:
its depth in from the edge, rounded like a cylinder `round` texels in radius.

Only needs CPython and numpy.
"""

import math
import os
import sys

import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
import theme  # noqa: E402
from vector import Mask, Path, hash01  # noqa: E402

TONES = {name: i for i, name in enumerate(theme.TONE_NAMES)}
"""Tone name -> ramp index: line 0, deep 1, shadow 3, terminator 4, base 5, light 7, highlight 9."""

EDGE = 1.5
OUTLINE = {"up": 0.3, "side": 1.1, "down": 1.7}
DETAIL = 1.2
RIM_WIDTH = 1.0

LIGHT = tuple(v / math.sqrt(0.30 ** 2 + 0.85 ** 2 + 0.43 ** 2) for v in (-0.30, -0.85, 0.43))
"""Unit vector toward the key light: the art guide's upper left, mostly from above (-y), a little
from the left (-x, behind a figure facing right), and toward the camera (+z); so a mirrored figure
still reads as lit from above. The fill is in the tones: the shadow is only a gentle step down."""
LINE_WEIGHTS = {"humanoid": 1.0, "quadruped": 0.65, "flier": 0.65, "arthropod": 0.65, "wisp": 0.65}
"""How heavy each rig's lines are against a person's: the guide's people at about 2.7 px, its
creatures at about 2.1. A rig not named here is a creature."""
CREATURE_WEIGHT = 0.65
_weight = [None]


def use_rig(rig="humanoid", scale=1.0):
    """Sets the line weight for what's drawn next: the rig's (LINE_WEIGHTS), divided by the scale
    the look will be drawn at (a Thornmother drawn at 2.2 has her lines drawn 2.2 times thinner, so
    they come out at the rig's weight). shade(), edge_line() and pack.part_from() follow it.

        style.use_rig("quadruped", scale=2.2)
    """
    _weight[0] = LINE_WEIGHTS.get(rig, CREATURE_WEIGHT) / float(scale)


def line_weight():
    """The weight set by use_rig(), or a person's (1.0) if none was."""
    return 1.0 if _weight[0] is None else _weight[0]


def outline_reach(weight=None):
    """OUTLINE at a line weight (the current one by default)."""
    w = line_weight() if weight is None else weight
    return {k: v * w for k, v in OUTLINE.items()}


RIM_FROM = (-0.33, -0.94)
"""The thin rim sits on edges whose outward normal points this way: toward the key light."""


class Material:
    """How a surface takes the light. Thresholds are on n . LIGHT (1 facing it, 0 side-on, below
    0 turned away): darker than `shadow` is form shadow; brighter than `light` is the light tone,
    than `highlight` the highlight. None turns a tone off."""

    def __init__(self, name, shadow, light=None, highlight=None, terminator=False, rim=True,
                 edge=True, band=None, speckle=0.0):
        self.name, self.shadow, self.light, self.highlight = name, shadow, light, highlight
        self.terminator, self.rim, self.edge, self.band, self.speckle = terminator, rim, edge, band, speckle


MATERIALS = {
    "skin": Material("skin", 0.30, light=0.93),
    "cloth": Material("cloth", 0.30, light=0.94),
    "leather": Material("leather", 0.30, light=0.84),
    "metal": Material("metal", 0.32, light=0.80, highlight=0.95),
    "hair": Material("hair", 0.34, light=0.88),
    "fur": Material("fur", 0.36, light=0.90),
    "stone": Material("stone", 0.30, light=0.88, speckle=0.07),
    "glow": Material("glow", None, rim=False, edge=False, band="glow"),
}


class Shading(dict):
    """What shade() painted, as Masks: base, shadow, light, highlight, deep, rim, line, interior
    (inside the edge line, where details go), and `I`, the light on each sample (n . LIGHT)."""

    __getattr__ = dict.__getitem__


def _normals(mask, depth_field, radius):
    """Pseudo-normals from a depth-in-from-the-edge field: a cylinder of `radius` texels at the
    edge, flat beyond it. Returns (nx, ny, nz) at sample resolution."""
    ss = mask.canvas.ss
    u = np.clip(depth_field / max(radius, 1e-6), 0.0, 1.0)
    w = 1.0 - u
    slope = w / np.sqrt(np.maximum(1.0 - w * w, 1e-4))
    gy, gx = np.gradient(depth_field)               # only the direction is used
    g = np.sqrt(gx * gx + gy * gy)
    gx = np.where(g > 1e-9, gx / np.where(g > 1e-9, g, 1.0), 0.0)
    gy = np.where(g > 1e-9, gy / np.where(g > 1e-9, g, 1.0), 0.0)
    nx, ny, nz = -slope * gx, -slope * gy, np.ones_like(slope)
    n = np.sqrt(nx * nx + ny * ny + nz * nz)
    return nx / n, ny / n, nz / n, gx, gy


def _outward(depth_field):
    gy, gx = np.gradient(depth_field)
    g = np.sqrt(gx * gx + gy * gy)
    safe = np.where(g > 1e-9, g, 1.0)
    return np.where(g > 1e-9, -gx / safe, 0.0), np.where(g > 1e-9, -gy / safe, 0.0)


def _cylinder(mask, axis, bias, smooth_texels=2.0):
    """Across-the-form normals of a cylinder standing along `axis` ("y": rows, a limb or torso
    upright; "x": columns, a foot): per row, where the shape starts and ends; nx runs -1 .. 1
    across it, `bias` moving the nearest point toward the front."""
    from vector import blur
    a = mask.inside if axis == "y" else mask.inside.T
    H, W = a.shape
    idx = np.arange(W, dtype=np.float64)
    has = a.any(axis=1)
    left = np.where(has, np.argmax(a, axis=1), 0).astype(np.float64)
    right = np.where(has, W - 1 - np.argmax(a[:, ::-1], axis=1), 0).astype(np.float64)
    # rows without any of the shape borrow their neighbours' extents, then everything is smoothed
    rows = np.arange(H)
    if has.any():
        left = np.interp(rows, rows[has], left[has])
        right = np.interp(rows, rows[has], right[has])
    r = int(max(1, smooth_texels * mask.canvas.ss))
    left = blur(left[:, None], r, 2)[:, 0]
    right = blur(right[:, None], r, 2)[:, 0]
    centre = (left + right + 1.0) / 2.0
    half = np.maximum((right - left + 1.0) / 2.0, 1.0)
    centre = centre + bias * half
    u = np.clip((idx[None, :] + 0.5 - centre[:, None]) / half[:, None], -1.0, 1.0)
    return u if axis == "y" else u.T


def _ends(mask, axis, round, smooth_texels=1.0):
    """Along-the-form normals near a cylinder's two ends: per column (for an upright one), where
    the shape starts and stops, and how far into the `round`-texel zone at each end a sample is:
    -1 at the top edge rising to 0, 0 to +1 at the bottom edge."""
    from vector import blur
    ss = mask.canvas.ss
    a = mask.inside if axis == "y" else mask.inside.T          # rows along the axis
    a = a.T                                                     # columns become rows
    H, W = a.shape
    idx = np.arange(W, dtype=np.float64)
    has = a.any(axis=1)
    top = np.where(has, np.argmax(a, axis=1), 0).astype(np.float64)
    bot = np.where(has, W - 1 - np.argmax(a[:, ::-1], axis=1), 0).astype(np.float64)
    rows = np.arange(H)
    if has.any():
        top = np.interp(rows, rows[has], top[has])
        bot = np.interp(rows, rows[has], bot[has])
    r = int(max(1, smooth_texels * ss))
    top = blur(top[:, None], r, 2)[:, 0]
    bot = blur(bot[:, None], r, 2)[:, 0]
    R = max(round * ss, 1.0)
    t = np.clip((idx[None, :] + 0.5 - top[:, None]) / R, 0.0, 1.0)
    b = np.clip((bot[:, None] + 1.0 - (idx[None, :] + 0.5)) / R, 0.0, 1.0)
    v = np.where(t < 1.0, -(1.0 - t) ** 1.5, 0.0) + np.where(b < 1.0, (1.0 - b) ** 1.5, 0.0)
    v = v.T
    return v if axis == "y" else v.T


def shade(mask, material, tones, light=LIGHT, round=None, open=None, edge=None, rim=True,
          deep=(), cast=(), band=None, seed=0, form="pillow", bias=0.0, norim=None):
    """Cel-shades a region of its canvas, all in one go:
      base; hard-edged form shadow on the side away from the light; the light tone on rounded
      tops; a highlight (leather, metal); the material's band (metal's sky band, hair's broken
      ring highlight, a glow's soft centre); speckle (stone); `cast` regions in shadow and `deep`
      regions in the deep tone (pass Masks: what overlaps it, moved down a few texels; creases);
      the skin terminator; the rim on the top-back edge; and the edge line (EDGE texels inside).
    `round` is the surface's radius in texels (default: the region's own half-thickness).
    `open` is a Mask whose boundary doesn't count as an edge: at a joint, the part this one is
    drawn over, near the joint (style.joint() makes it), so no line, rim or rounding is drawn
    there and the joint shows no seam.
    `band` overrides the material's band: for "sky", (top, bottom) in texels of the plate.
    `form` is how the surface is guessed: "pillow" (default) rounds every edge alike, for heads,
    hair, hats; "cylinder" rounds side to side row by row, for anything standing upright (limbs,
    torso, sleeves, skirts), its ends rounded over `round` texels; "cylinder-x" lies along x (a
    foot). `bias` (-1 .. 1) moves a cylinder's nearest line toward the front (+).
    `norim` is a Mask where no rim is drawn: the end of a part that tucks under another, which
    only shows when the joint bends.

        s = style.shade(c.shape(Path.capsule((20, 10), (22, 70), 11, 8)), "skin", theme.tones("skin.3", 1))
    """
    mat = MATERIALS[material] if isinstance(material, str) else material
    if edge is None:
        edge = EDGE * line_weight()
    c = mask.canvas
    ss = c.ss
    whole = mask | open if open is not None else mask
    inside = mask.inside
    own = mask.depth(41.0)
    if round is None:
        round = float(np.clip(own.max() if own.size else 4.0, 3.0, 40.0))
    # smooth, so the normals don't step; a big surface can be worked out coarser
    res = ss if round <= 14 else max(1, ss // 2)
    full_depth = whole.depth(round + 2.0, res=res, smooth=3 if res == ss else 4)
    near = whole.depth(edge + RIM_WIDTH + 2.0, res=ss)    # exact near the edge, for line and rim
    nx, ny, nz, gx, gy = _normals(mask, np.minimum(full_depth, round), round)
    if form in ("cylinder", "cylinder-x"):
        axis = "y" if form == "cylinder" else "x"
        u = _cylinder(whole, axis, bias)
        v = _ends(whole, axis, round)
        if form == "cylinder":
            nx, ny = u * np.sqrt(np.maximum(0.0, 1.0 - v * v)), v
        else:
            nx, ny = v, u * np.sqrt(np.maximum(0.0, 1.0 - v * v))
        nz = np.sqrt(np.maximum(0.04, 1.0 - nx * nx - ny * ny))
        n = np.sqrt(nx * nx + ny * ny + nz * nz)
        nx, ny, nz = nx / n, ny / n, nz / n
    I = nx * light[0] + ny * light[1] + nz * light[2]
    I = np.where(inside, I, 0.0)
    m = mask.a
    out = Shading(I=I)

    def region(test):
        return Mask(c, m * test)

    c.tone(mask, tones, "base")
    out["base"] = mask
    if mat.band == "glow":
        u = np.clip(own / max(round, 1e-6), 0.0, 1.0)
        c.gradient(mask, tones, 5.0 + 4.0 * np.sqrt(u))
        out["interior"] = mask
        out["shadow"] = out["light"] = out["highlight"] = out["rim"] = out["line"] = c.nothing()
        out["deep"] = c.nothing()
        return out

    shadow = region(I < mat.shadow) if mat.shadow is not None else c.nothing()
    lightm = region(I > mat.light) if mat.light is not None else c.nothing()
    high = region(I > mat.highlight) if mat.highlight is not None else c.nothing()
    b = mask.bounds()
    y0, y1 = (b[1], b[3]) if b else (0.0, 1.0)

    kind = band if isinstance(band, str) else mat.band
    if kind == "sky" or (isinstance(band, tuple) and mat.band == "sky"):
        top, bot = band if isinstance(band, tuple) else (y0, y1)
        ys = (np.arange(m.shape[0]) + 0.5) / ss
        v = ((ys - top) / max(bot - top, 1e-6))[:, None] * np.ones((1, m.shape[1]))
        # bright sky above the band, a hard drop to navy at 40%, easing back to the base below
        field = np.where(v < 0.40, 8.6 - 1.8 * (v / 0.40),
                         np.where(v < 0.47, 3.0, 3.0 + 2.0 * np.clip((v - 0.47) / 0.30, 0.0, 1.0)))
        field = np.where(I < mat.shadow, np.minimum(field, 3.0), field)
        c.gradient(mask, tones, field)
    else:
        if shadow.any():
            c.tone(shadow, tones, "shadow")
        if lightm.any():
            c.tone(lightm - shadow, tones, "light")
        if kind == "ring":
            # the "angel ring": a latitude band of the dome, facing the light, broken into lenses
            xs = (np.arange(m.shape[1]) + 0.5) / ss
            ph1, ph2 = hash01(1, 2, seed + 11) * 6.283, hash01(3, 4, seed + 11) * 6.283
            noise = 0.5 + 0.32 * np.sin(xs / 3.1 + ph1) + 0.18 * np.sin(xs / 1.7 + ph2)
            up = -ny
            strength = (1.0 - np.abs(up - 0.53) / 0.14) * noise[None, :] * np.clip((I - 0.40) / 0.25, 0.0, 1.0)
            c.tone(region(strength > 0.30), tones, "light-highlight")
            c.tone(region(strength > 0.62), tones, "highlight")
    if high.any():
        c.tone(high - shadow, tones, "highlight")
    out["shadow"], out["light"], out["highlight"] = shadow, lightm, high

    if mat.speckle > 0:
        H, W = m.shape
        tx = np.arange(W) // (2 * ss)
        ty = np.arange(H) // (2 * ss)
        grid = np.array([[hash01(int(x), int(y), seed + 31) for x in range(c.w // 2 + 1)] for y in range(c.h // 2 + 1)])
        cell = grid[ty][:, tx]
        c.tone(region(cell < mat.speckle * 0.5), tones, "deep-shadow")
        c.tone(region((cell > 1 - mat.speckle * 0.5) & (I > mat.shadow)), tones, "base-light")

    for cm in cast:
        c.tone(mask & cm, tones, "shadow")
    dm = c.nothing()
    for d in deep:
        dm = dm | (mask & d)
    if dm.any():
        c.tone(dm, tones, "deep")
    out["deep"] = dm

    if mat.terminator and shadow.any():
        term = shadow.grow(1.0) - shadow
        c.tone(term & mask, tones, "terminator")

    rim_m = c.nothing()
    if rim and mat.rim:
        ox, oy = _outward(full_depth)
        facing = ox * RIM_FROM[0] + oy * RIM_FROM[1]
        width = RIM_WIDTH * np.clip((facing - 0.78) / 0.14, 0.0, 1.0)
        start = edge if (edge and mat.edge) else 0.0
        zone = (near > start) & (near <= start + width) & inside
        if norim is not None:
            zone = zone & ~norim.inside
        rim_m = region(zone)
        if rim_m.any():
            c.tone(rim_m, tones, "base-light")
    out["rim"] = rim_m

    line_m = c.nothing()
    if edge and mat.edge:
        line_m = region((near <= edge) & inside)
        c.tone(line_m, tones, "line")
    out["line"] = line_m
    out["interior"] = region(near > edge) if (edge and mat.edge) else mask
    return out


def joint(under, centre, radius, inset=EDGE):
    """The `open` region for a part drawn over another at a joint: the part under it, inset by the
    edge line, within `radius` texels of the joint's centre. Further from the joint, where the part
    crosses the other (an arm over the torso), its line stays.

        open = style.joint(c.shape(upper_arm), (24, 60), 12)
    """
    c = under.canvas
    near = c.shape(Path.circle(centre[0], centre[1], radius))
    return under.inset(inset) & near


def edge_line(mask, tones, width=None, open=None):
    """Draws (or redraws, after details) a region's edge line, `width` texels inside it.

        style.edge_line(body, t, open=chest.inset(3))
    """
    width = EDGE * line_weight() if width is None else width
    whole = mask | open if open is not None else mask
    d = whole.depth(width + 2, res=mask.canvas.ss)
    m = Mask(mask.canvas, mask.a * ((d <= width) & mask.inside))
    mask.canvas.tone(m, tones, "line")
    return m


def rim(mask, width=RIM_WIDTH, inset=EDGE, open=None, tones=None):
    """Paints the thin, low-contrast rim along a region's lit edge (toward the key light), `inset`
    texels in: the part's own base-light tone with `tones`, else a neutral light at low strength.

        style.rim(hood_mask, tones=t)
    """
    c = mask.canvas
    whole = mask | open if open is not None else mask
    d = whole.depth(inset + width + 4)
    ox, oy = _outward(d)
    facing = ox * RIM_FROM[0] + oy * RIM_FROM[1]
    w = width * np.clip((facing - 0.78) / 0.14, 0.0, 1.0)
    zone = Mask(c, mask.a * ((d > inset) & (d <= inset + w) & mask.inside))
    if tones is not None:
        c.tone(zone, tones, "base-light")
    else:
        c.paint(zone, theme.rgb(theme.RIM) + (255,), group=0, grey=TONES["base-light"], alpha=0.35)
    return zone


# --------------------------------------------------------------------------- #
# Strokes and small shapes
# --------------------------------------------------------------------------- #

def _dir(angle):
    a = math.radians(angle)
    return math.cos(a), math.sin(a)


def line(c, path, tones, width=DETAIL, tone="line", clip=None, taper=True):
    """A detail line, tapering from `width` to nothing (or both ends with taper="both").

        style.line(c, Path.polyline([(10, 10), (14, 30)]), t, clip=s.interior)
    """
    if taper == "both":
        widths = lambda u: width * math.sin(math.pi * max(0.0, min(1.0, u))) ** 0.6  # noqa: E731
    elif taper:
        widths = (width, 0.0)
    else:
        widths = width
    m = c.line_shape(path, widths)
    c.tone(m, tones, tone, clip=clip)
    return m


def wedge(a, b, width, bulge=0.5):
    """A spindle from a to b, `width` at its widest, `bulge` of the way along (0 .. 1)."""
    ax, ay = a
    bx, by = b
    dx, dy = bx - ax, by - ay
    n = math.hypot(dx, dy) or 1.0
    px, py = -dy / n, dx / n
    mx, my = ax + dx * bulge, ay + dy * bulge
    h = width / 2.0
    return (Path().move(ax, ay)
            .quad(mx + px * h * 1.6 - dx * 0.15, my + py * h * 1.6 - dy * 0.15, bx, by)
            .quad(mx - px * h * 1.6 - dx * 0.15, my - py * h * 1.6 - dy * 0.15, ax, ay).close())


# --------------------------------------------------------------------------- #
# Folds (§1.7): a shadow wedge, a detail line on its shadow side, a light sliver on its ridge
# --------------------------------------------------------------------------- #

def _fold_one(c, a, b, width, tones, clip, bulge=0.6, light_side=1):
    ax, ay = a
    bx, by = b
    dx, dy = bx - ax, by - ay
    n = math.hypot(dx, dy) or 1.0
    px, py = -dy / n, dx / n
    # which side of the fold faces the light: the ridge is on that side
    if px * LIGHT[0] + py * LIGHT[1] < 0:
        px, py = -px, -py
    px, py = px * light_side, py * light_side
    shape = wedge(a, b, width, bulge)
    c.tone(c.shape(shape), tones, "shadow", clip=clip)
    off = width * 0.45
    seam = Path.smooth([(ax - px * off * 0.3, ay - py * off * 0.3),
                        (ax + dx * bulge - px * off, ay + dy * bulge - py * off),
                        (bx, by)])
    c.tone(c.line_shape(seam, (DETAIL, 0.0)), tones, "line", clip=clip)
    ridge = Path.smooth([(ax + px * off * 0.6, ay + py * off * 0.6),
                         (ax + dx * bulge + px * (off + 1.4), ay + dy * bulge + py * (off + 1.4)),
                         (ax + dx * 0.95 + px * off * 0.5, ay + dy * 0.95 + py * off * 0.5)])
    c.tone(c.line_shape(ridge, lambda u: 1.4 * math.sin(math.pi * u)), tones, "light", clip=clip)


def fold(c, kind, at, length, angle, tones, clip=None, width=None, seed=0):
    """One cloth fold of the five kinds (§1.7), from `at`, `length` texels long, pointing `angle`
    degrees (0 is +x, 90 straight down):
      pipe     hanging cloth: a long fold widening downward          style.fold(c, "pipe", (40, 60), 36, 90, t, clip)
      zigzag   compression at elbows, knees, under the belt          style.fold(c, "zigzag", (30, 50), 14, 90, t, clip)
      tension  a diagonal pull, shoulder to armpit, crotch to knee   style.fold(c, "tension", (12, 8), 30, 60, t, clip)
      spiral   round a pushed-up sleeve                              style.fold(c, "spiral", (20, 40), 16, 90, t, clip)
      drop     at a hem: the deep inside and a stitch line           style.fold(c, "drop", (30, 96), 10, 90, t, clip)
    """
    ux, uy = _dir(angle)
    px, py = -uy, ux
    x, y = at
    w = width if width is not None else max(3.0, length * 0.16)
    if kind == "pipe":
        _fold_one(c, (x, y), (x + ux * length, y + uy * length), w, tones, clip, bulge=0.72)
    elif kind == "tension":
        _fold_one(c, (x, y), (x + ux * length, y + uy * length), w * 0.8, tones, clip, bulge=0.25)
    elif kind == "zigzag":
        n = 4
        step = length / n
        span = max(6.0, length * 0.42)
        pts = [(x + ux * step * i + px * span * (0.5 if i % 2 == 0 else -0.5),
                y + uy * step * i + py * span * (0.5 if i % 2 == 0 else -0.5)) for i in range(n + 1)]
        for a, b in zip(pts, pts[1:]):
            _fold_one(c, a, b, max(2.4, step * 0.36), tones, clip, bulge=0.5)
    elif kind == "spiral":
        for i in range(3):
            sx, sy = x + ux * length * i / 3.0, y + uy * length * i / 3.0
            a = (sx - px * w * 1.6, sy - py * w * 1.6)
            mid = (sx + ux * w * 0.9, sy + uy * w * 0.9)
            b = (sx + px * w * 1.6 + ux * w * 0.4, sy + py * w * 1.6 + uy * w * 0.4)
            p = Path.smooth([a, mid, b])
            c.tone(c.line_shape(p, lambda u: 2.6 * math.sin(math.pi * u) ** 0.7), tones, "shadow", clip=clip)
            c.tone(c.line_shape(p.translate(ux * 1.6, uy * 1.6), lambda u: 1.2 * math.sin(math.pi * u)),
                   tones, "light", clip=clip)
            c.tone(c.line_shape(p.translate(-ux * 1.0, -uy * 1.0), (DETAIL * 0.8, 0.0)), tones, "line", clip=clip)
    elif kind == "drop":
        w = width if width is not None else length * 0.75
        tip = (x - ux * length, y - uy * length)
        a = (x - px * w * 0.9, y - py * w * 0.9)
        b = (x + px * w * 0.9, y + py * w * 0.9)
        c.tone(c.shape(Path.polygon([a, tip, b])), tones, "deep", clip=clip)
        c.tone(c.line_shape(Path.polyline([tip, (x + px * w * 0.2, y + py * w * 0.2)]), (DETAIL, 0.0)),
               tones, "line", clip=clip)
        c.tone(c.line_shape(Path.polyline([b, tip]), lambda u: 1.3 * math.sin(math.pi * u)), tones, "light", clip=clip)
        stitches(c, Path.polyline([(x - px * w * 2.2 - ux * 3, y - py * w * 2.2 - uy * 3),
                                   (x + px * w * 2.2 - ux * 3, y + py * w * 2.2 - uy * 3)]),
                 tones, clip=clip, tone="deep", width=1.2)
    else:
        raise ValueError("no fold kind " + kind)


# --------------------------------------------------------------------------- #
# Construction details (§1.8)
# --------------------------------------------------------------------------- #

def seam(c, path, tones, clip=None, width=DETAIL):
    """A sewn seam: a deep line with a light ridge beside it on the lit side.

        style.seam(c, Path.polyline([(30, 10), (32, 90)]), t, clip=s.interior)
    """
    c.tone(c.line_shape(path, width), tones, "deep", clip=clip)
    ox, oy = LIGHT[0] * 1.6, LIGHT[1] * 1.6
    c.tone(c.line_shape(path.translate(ox, oy), lambda u: 1.0 * math.sin(math.pi * u) ** 0.3), tones,
           "light", clip=clip)


def stitches(c, path, tones, on=3.0, off=2.0, width=1.4, tone="deep", clip=None):
    """Stitch dashes along a path, `on` texels drawn and `off` skipped (leather: 3 on 2 off).

        style.stitches(c, Path.polyline([(8, 20), (8, 70)]), t, clip=s.interior)
    """
    total = path.length()
    t = 0.0
    m = c.nothing()
    while t < total:
        a = path.at(t / total)[0]
        b = path.at(min(total, t + on) / total)[0]
        m = m | c.line_shape(Path.polyline([a, b]), width)
        t += on + off
    c.tone(m, tones, tone, clip=clip)
    return m


def rivet(c, x, y, tones, r=1.5, clip=None):
    """A 3-texel rivet: a dome with a deep under-edge and a glint on its lit top.

        style.rivet(c, 20, 30, theme.tones("metal.iron", 8))
    """
    c.tone(c.shape(Path.circle(x, y + 0.4, r + 0.5)), tones, "deep", clip=clip)
    c.tone(c.shape(Path.circle(x, y, r)), tones, "base-light", clip=clip)
    c.tone(c.shape(Path.circle(x + 0.45 * r, y - 0.5 * r, r * 0.45)), tones, "highlight", clip=clip)


def studs(c, path, tones, spacing=8.0, r=1.5, clip=None):
    """Rivets or studs every `spacing` texels along a path (studded leather: 3 texels, 8 apart).

        style.studs(c, Path.polyline([(10, 20), (60, 20)]), theme.tones("metal.iron", 8))
    """
    for p in path.resample(spacing):
        rivet(c, p[0], p[1], tones, r, clip)


def quilting(c, region, tones, size=10.0, angle=45.0, clip=None):
    """Padded quilting: a diamond grid of sewn lines `size` texels apart, each diamond puffed
    (light on its upper front, shadow along its lower edge).

        style.quilting(c, s.interior, t)
    """
    b = region.bounds()
    if b is None:
        return
    x0, y0, x1, y1 = b
    span = (x1 - x0) + (y1 - y0) + 2 * size
    clipm = region if clip is None else (region & clip)
    for sgn in (1, -1):
        a = math.radians(angle * sgn)
        ux, uy = math.cos(a), math.sin(a)
        k = -span
        while k < span:
            cx, cy = (x0 + x1) / 2 - uy * k, (y0 + y1) / 2 + ux * k
            p = Path.polyline([(cx - ux * span, cy - uy * span), (cx + ux * span, cy + uy * span)])
            c.tone(c.line_shape(p.translate(0, 1.2), 2.2), tones, "shadow", clip=clipm)
            c.tone(c.line_shape(p, 1.2), tones, "deep", clip=clipm)
            c.tone(c.line_shape(p.translate(0, -1.3), 1.0), tones, "base-light", clip=clipm)
            k += size


def chain(c, region, tones, size=4.0, shading=None, clip=None, seed=0):
    """Mail: offset rows of `size`-texel arcs on a dark ground, with dense highlight specks on the
    lit side (pass the Shading from shade() for where that is).

        style.chain(c, s.interior, theme.tones("metal.iron", 8), shading=s)
    """
    b = region.bounds()
    if b is None:
        return
    x0, y0, x1, y1 = b
    clipm = region if clip is None else (region & clip)
    c.tone(clipm, tones, "shadow")
    ss = c.ss
    lit = shading["I"] if shading is not None else None
    r = size / 2.0 - 0.15
    links = Path()
    specks = Path()
    row = 0
    y = y0 + size * 0.3
    while y < y1 + size:
        x = x0 + (size / 2.0 if row % 2 else 0.0)
        col = 0
        ring = Path()
        while x < x1 + size:
            ring.add(Path.circle(x, y, r)).add(Path.circle(x, y, r - 1.3))
            sx, sy = x + size * 0.18, y + size * 0.42
            xi, yi = int(sx * ss), int(sy * ss)
            bright = 0.6
            if lit is not None and 0 <= yi < lit.shape[0] and 0 <= xi < lit.shape[1]:
                bright = lit[yi, xi]
            if hash01(col, row, seed + 41) < 0.25 + 0.7 * max(0.0, bright - 0.3):
                specks.add(Path.circle(sx, sy, 0.7))
            x += size
            col += 1
        arcs = c.shape(ring).below(y + 0.2)
        c.tone(arcs.shift(0, 0.8) - arcs, tones, "deep", clip=clipm)
        c.tone(arcs, tones, "base", clip=clipm)
        y += size * 0.75
        row += 1
    if specks.subs:
        c.tone(c.shape(specks), tones, "highlight", clip=clipm)


def scales(c, region, tones, w=8.0, h=10.0, clip=None):
    """Scale armour: rows of w x h scales, each row half a scale over, every scale with a light
    top and the deep tone under the row above.

        style.scales(c, s.interior, theme.tones("metal.bronze", 8))
    """
    b = region.bounds()
    if b is None:
        return
    x0, y0, x1, y1 = b
    clipm = region if clip is None else (region & clip)
    rows = []
    y = y0 - h * 0.3
    r = 0
    while y < y1 + h:
        rows.append((y, r))
        y += h * 0.62
        r += 1
    for y, r in reversed(rows):           # lower rows first: each row up hangs over the next
        x = x0 - (w / 2.0 if r % 2 else 0.0)
        body, shine, rims = Path(), Path(), Path()
        while x < x1 + w:
            body.add(Path().move(x + 0.3, y).line(x + w - 0.3, y).line(x + w - 0.3, y + h * 0.55)
                     .quad(x + w - 0.3, y + h, x + w / 2.0, y + h).quad(x + 0.3, y + h, x + 0.3, y + h * 0.55).close())
            shine.add(Path.ellipse(x + w * 0.58, y + h * 0.42, w * 0.22, h * 0.12))
            rims.add(Path.polyline([(x + 0.4, y + h * 0.55)]).quad(x + 0.4, y + h, x + w / 2.0, y + h)
                     .quad(x + w - 0.4, y + h, x + w - 0.4, y + h * 0.55))
            x += w
        m = c.shape(body, rule="nonzero")
        c.tone(m.shift(0, 1.5) - m, tones, "deep", clip=clipm)
        c.tone(m, tones, "base", clip=clipm)
        c.tone(m.below(y + h * 0.7), tones, "shadow", clip=clipm)
        c.tone(c.shape(shine, rule="nonzero"), tones, "highlight", clip=clipm)
        c.tone(c.line_shape(rims, 1.1), tones, "line", clip=clipm)


def trim(c, path, tones, width=6.0, motif="diamond", spacing=12.0, motif_tones=None, dots=None, clip=None):
    """A trim band along a path: the band, a fine line at each edge, a motif every `spacing`
    texels ("diamond", "dot", "cross" or None) and, with `dots` (tones), gold-thread dots.

        style.trim(c, Path.polyline([(4, 90), (60, 92)]), theme.tones("cloth.ochre", 10), motif="diamond")
    """
    band = c.line_shape(path, width)
    c.tone(band, tones, "base", clip=clip)
    edges = c.line_shape(path, width + 1.2) - c.line_shape(path, width - 1.2)
    c.tone(edges, tones, "line", clip=clip)
    mt = motif_tones or tones
    if motif:
        total = path.length()
        n = max(1, int(total // spacing))
        for i in range(n):
            (px, py), (tx, ty) = path.at((i + 0.5) / n)
            k = width * 0.28
            if motif == "diamond":
                shape = Path.polygon([(px + tx * k, py + ty * k), (px - ty * k, py + tx * k),
                                      (px - tx * k, py - ty * k), (px + ty * k, py - tx * k)])
            elif motif == "dot":
                shape = Path.circle(px, py, k * 0.8)
            else:
                shape = Path.rect(px - k, py - 0.6, 2 * k, 1.2).add(Path.rect(px - 0.6, py - k, 1.2, 2 * k))
            c.tone(c.shape(shape, rule="nonzero"), mt, "light", clip=clip)
    if dots is not None:
        for p in path.resample(spacing / 3.0):
            c.tone(c.shape(Path.circle(p[0], p[1] - width * 0.36, 0.55)), dots, "highlight", clip=clip)
    return band


def scuffs(c, region, tones, n=3, length=5.0, seed=0, clip=None):
    """Wear: n short light scratches with a deep nick, placed by hash01 inside a region.

        style.scuffs(c, s.interior, t, n=3, seed=7)
    """
    b = region.bounds()
    if b is None:
        return
    x0, y0, x1, y1 = b
    clipm = region if clip is None else (region & clip)
    for i in range(n):
        x = x0 + (x1 - x0) * (0.15 + 0.7 * hash01(i, 1, seed + 53))
        y = y0 + (y1 - y0) * (0.15 + 0.7 * hash01(i, 2, seed + 53))
        a = math.radians(-20 + 50 * hash01(i, 3, seed + 53))
        dx, dy = math.cos(a) * length / 2, math.sin(a) * length / 2
        p = Path.polyline([(x - dx, y - dy), (x + dx, y + dy)])
        c.tone(c.line_shape(p, lambda u: 1.3 * math.sin(math.pi * u)), tones, "light", clip=clipm)
        c.tone(c.line_shape(p.translate(0.3, 0.9), lambda u: 0.9 * math.sin(math.pi * u)), tones, "deep-shadow", clip=clipm)


def rolled_rim(c, path, tones, clip=None):
    """Plate's 2-texel rolled rim along an edge: a highlight bead with the deep tone under it.

        style.rolled_rim(c, Path.polyline([(4, 20), (40, 18)]), theme.tones("metal.iron", 8))
    """
    c.tone(c.line_shape(path.translate(0, 1.2), 2.4), tones, "deep", clip=clip)
    c.tone(c.line_shape(path, 2.0), tones, "light", clip=clip)
    c.tone(c.line_shape(path.translate(0.3, -0.4), 0.9), tones, "highlight", clip=clip)


def glint(c, x, y, tones, size=3.0, clip=None):
    """A 3-texel glint on metal: a small four-pointed star in the highlight.

        style.glint(c, 30, 18, theme.tones("metal.gold", 8))
    """
    k = size / 2.0
    star = Path.polygon([(x, y - k * 1.3), (x + 0.4, y - 0.4), (x + k * 1.3, y), (x + 0.4, y + 0.4),
                         (x, y + k * 1.3), (x - 0.4, y + 0.4), (x - k * 1.3, y), (x - 0.4, y - 0.4)])
    c.tone(c.shape(star), tones, "highlight", clip=clip)


def lacing(c, path, tones, spacing=6.0, width=3.0, clip=None):
    """X-lacing along a seam (hide): crossed cords every `spacing` texels.

        style.lacing(c, Path.polyline([(30, 10), (30, 60)]), theme.tones("leather.tan", 9))
    """
    pts = path.resample(spacing)
    for (ax, ay), (bx, by) in zip(pts, pts[1:]):
        dx, dy = bx - ax, by - ay
        n = math.hypot(dx, dy) or 1.0
        px, py = -dy / n * width, dx / n * width
        for p in (Path.polyline([(ax - px, ay - py), (bx + px, by + py)]),
                  Path.polyline([(ax + px, ay + py), (bx - px, by - py)])):
            c.tone(c.line_shape(p, 1.6), tones, "line", clip=clip)
            c.tone(c.line_shape(p, 0.8), tones, "light", clip=clip)


def patch(c, x, y, w, h, tones, clip=None, seed=0):
    """A sewn-on patch (a Common item's): a slightly skewed square with stitch dashes round it.

        style.patch(c, 24, 50, 10, 9, theme.tones("cloth.oat", 4))
    """
    k = (hash01(1, 1, seed + 61) - 0.5) * 1.6
    pts = [(x, y + k), (x + w, y - k), (x + w + k * 0.5, y + h), (x - k * 0.3, y + h + k * 0.4)]
    m = c.shape(Path.polygon(pts))
    c.tone(m, tones, "base-light", clip=clip)
    c.tone(m.below(y + h * 0.6), tones, "base", clip=clip)
    c.tone(m.grow(0.8) - m, tones, "line", clip=clip)
    stitches(c, Path.polygon([(px + (1.4 if px < x + w / 2 else -1.4), py + (1.4 if py < y + h / 2 else -1.4))
                              for px, py in pts]), tones, on=1.6, off=1.4, width=0.9, tone="deep", clip=clip)


def weave(c, region, tones, spacing=3.0, seed=0, clip=None):
    """Cloth texture: a faint broken weave of short dashes, a step lighter and a step darker than
    what's under them, `spacing` texels apart. Subtle at gameplay scale, there up close.

        style.weave(c, s.interior, t)
    """
    b = region.bounds()
    if b is None:
        return
    x0, y0, x1, y1 = b
    clipm = region if clip is None else (region & clip)
    lit, dark = Path(), Path()
    row = 0
    y = y0 + spacing / 2.0
    while y < y1:
        x = x0 + (spacing / 2.0 if row % 2 else 0.0)
        col = 0
        while x < x1:
            h = hash01(col, row, seed + 81)
            if h < 0.30:
                lit.add(Path.rect(x, y, spacing * 0.55, 0.6))
            elif h > 0.72:
                dark.add(Path.rect(x + 0.3, y + 0.5, 0.6, spacing * 0.5))
            x += spacing
            col += 1
        y += spacing
        row += 1
    if lit.subs:
        c.tone(c.shape(lit, rule="nonzero"), tones, "base-light", clip=clipm, alpha=0.55)
    if dark.subs:
        c.tone(c.shape(dark, rule="nonzero"), tones, "terminator", clip=clipm, alpha=0.55)


def dirt(c, region, tones, amount=0.12, seed=0, low=True, clip=None):
    """Grime: irregular blotches a shade darker, thickest toward the bottom of a region (hems,
    knees, boots) with `low`. `amount` is about the share of the region they cover.

        style.dirt(c, s.interior, t, amount=0.15, seed=4)
    """
    b = region.bounds()
    if b is None:
        return
    x0, y0, x1, y1 = b
    clipm = region if clip is None else (region & clip)
    spots = Path()
    n = int(max(1, amount * (x1 - x0) * (y1 - y0) / 9.0))
    for i in range(n):
        v = hash01(i, 7, seed + 91)
        y = y0 + (y1 - y0) * (1.0 - v * v if low else v)
        x = x0 + (x1 - x0) * hash01(i, 8, seed + 91)
        r = 0.8 + 2.0 * hash01(i, 9, seed + 91)
        spots.add(Path.ellipse(x, y, r * (1.0 + hash01(i, 10, seed + 91)), r * 0.7, rot=40 * hash01(i, 11, seed + 91)))
    c.tone(c.shape(spots, rule="nonzero"), tones, "terminator", clip=clipm, alpha=0.7)


def scar(c, path, tones, width=1.6, stitched=False, clip=None):
    """A scar: a thin pale line with a darker edge under it, and stitch ticks across if `stitched`.

        style.scar(c, Path.polyline([(30, 20), (36, 34)]), skin_tones)
    """
    c.tone(c.line_shape(path.translate(0.4, 0.6), lambda u: width * math.sin(math.pi * u) ** 0.5), tones, "shadow", clip=clip)
    c.tone(c.line_shape(path, lambda u: width * 0.7 * math.sin(math.pi * u) ** 0.5), tones, "light", clip=clip)
    if stitched:
        for i, p in enumerate(path.resample(3.0)[1:-1]):
            (px, py), (tx, ty) = path.at((i + 1) / max(1, len(path.resample(3.0)) - 1))
            c.tone(c.line_shape(Path.polyline([(px - ty * 1.5, py + tx * 1.5), (px + ty * 1.5, py - tx * 1.5)]), 0.7),
                   tones, "deep", clip=clip)


def fray(c, path, tones, depth=3.0, spacing=3.0, seed=0, clip=None):
    """A frayed hem: small notches eaten out of an edge, with loose threads.

        style.fray(c, Path.polyline([(4, 98), (60, 99)]), t, seed=3)
    """
    for i, p in enumerate(path.resample(spacing)):
        h = depth * (0.4 + 0.6 * hash01(i, 5, seed + 71))
        if hash01(i, 6, seed + 71) < 0.55:
            c.erase(c.shape(Path.polygon([(p[0] - 1.1, p[1] + 1), (p[0], p[1] - h), (p[0] + 1.1, p[1] + 1)])))
        else:
            c.tone(c.line_shape(Path.polyline([(p[0], p[1] - 1), (p[0] + 0.6, p[1] + h * 0.6)]), (1.0, 0.0)),
                   tones, "deep", clip=clip)
