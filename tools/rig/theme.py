"""
The rig's colours, from the owner's art guide (tools/source/art-guide.png, "2D Art Style &
Generation Guide"): muted, earthy, worked out in OKLab by the same formulas for every material.

    python tools/rig/theme.py       writes Content/Rig/ramps.png and ramps.json

Where the colours come from:
  - the guide's 5 x 4 Palette swatches (neutrals; reds and purples; blues and greens; soft and
    warm), sampled live as 13 x 13 medians at x 1365, 1400, 1432, 1465, 1499 and y 646, 681, 716,
    751: every costume ramp (cloth, leather, metal, stone) is one of them;
  - skin and hair, sampled off its Head & Face and Hair panels and its characters (GUIDE_SAMPLES,
    each with where it was taken), very light to deep brown and brown, black, blonde, auburn,
    grey and silver;
  - the old style sheet only for glows, which may keep a stronger accent (violet, gold, ember).
Every ramp id the kits and looks used before is still here, pointed at the guide's colours.

A ramp is ten colours: index 0 is the line, 1..9 the nine steps deep .. highlight:

    0 line   1 deep   2 .   3 shadow   4 terminator   5 base   6 .   7 light   8 .   9 highlight

The five named tones sit on the odd indices; the even ones between are their OKLab midpoints
(index 4 is still called the terminator; it's now the plain midpoint). A part shades in two to four
of them, hard-edged: base and shadow, the deep tone in creases, a modest light. A kit's tint mask stores
the index as G = round(index * 255 / 9), fractional where tones meet, and RigTint reads the colour
between the two nearest entries. ramps.png is a row per ramp, ten texels wide, in that order.

    theme.ramp("skin.3")        -> the 9 steps, deep .. highlight, as (r, g, b)
    theme.tones("#5c94d5")      -> {"line", "deep", "shadow", "base", "light", "highlight", "rim",
                                    "terminator", "steps": [10 colours], "group", "kind"}
    theme.tones("cloth.woad", group=5)   # a ramp's tones, painting into tint group 5
    theme.oklab((r, g, b)); theme.srgb((L, a, b)); theme.mix(c1, c2, t); theme.delta_e(c1, c2)
    theme.register("cloth.gull-red", "#9d3d61")   # a ramp of your own, written with the rest:
                                                  # call it at module level, and theme.py and
                                                  # generate_rig_kits.py find it by importing you

Only needs CPython and numpy.
"""

import json
import math
import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
import vector  # noqa: E402

ROOT = vector.ROOT
SOURCE = os.path.join(ROOT, "tools", "source")
OUT = os.path.join(ROOT, "Content", "Rig")

# --------------------------------------------------------------------------- #
# OKLab (Björn Ottosson's)
# --------------------------------------------------------------------------- #


def _lin(c):
    c = c / 255.0
    return c / 12.92 if c <= 0.04045 else ((c + 0.055) / 1.055) ** 2.4


def _gam(c):
    c = c * 12.92 if c <= 0.0031308 else 1.055 * (c ** (1 / 2.4)) - 0.055
    return c * 255.0


def _cbrt(x):
    return math.copysign(abs(x) ** (1.0 / 3.0), x)


def rgb(c):
    """A colour as an (r, g, b) tuple of ints: from '#rrggbb', (r, g, b) or (r, g, b, a)."""
    if isinstance(c, str):
        c = c.lstrip("#")
        return int(c[0:2], 16), int(c[2:4], 16), int(c[4:6], 16)
    return int(round(c[0])), int(round(c[1])), int(round(c[2]))


def hexof(c):
    r, g, b = rgb(c)
    return "#%02x%02x%02x" % (r, g, b)


def oklab(c):
    """sRGB (0..255) -> OKLab (L 0..1, a, b)."""
    c = rgb(c) if isinstance(c, str) else c
    r, g, b = _lin(float(c[0])), _lin(float(c[1])), _lin(float(c[2]))
    l = 0.4122214708 * r + 0.5363325363 * g + 0.0514459929 * b
    m = 0.2119034982 * r + 0.6806995451 * g + 0.1073969566 * b
    s = 0.0883024619 * r + 0.2817188376 * g + 0.6299787005 * b
    l_, m_, s_ = _cbrt(l), _cbrt(m), _cbrt(s)
    return (0.2104542553 * l_ + 0.7936177850 * m_ - 0.0040720468 * s_,
            1.9779984951 * l_ - 2.4285922050 * m_ + 0.4505937099 * s_,
            0.0259040371 * l_ + 0.7827717662 * m_ - 0.8086757660 * s_)


def _lab_to_linear(L, a, b):
    l_ = L + 0.3963377774 * a + 0.2158037573 * b
    m_ = L - 0.1055613458 * a - 0.0638541728 * b
    s_ = L - 0.0894841775 * a - 1.2914855480 * b
    l, m, s = l_ ** 3, m_ ** 3, s_ ** 3
    return (4.0767416621 * l - 3.3077115913 * m + 0.2309699292 * s,
            -1.2684380046 * l + 2.6097574011 * m - 0.3413193965 * s,
            -0.0041960863 * l - 0.7034186147 * m + 1.7076147010 * s)


def srgb(lab):
    """OKLab -> sRGB ints. Out-of-gamut colours keep their lightness and hue and lose chroma
    (a fixed 24-step bisection, so it's the same every run)."""
    L, a, b = lab
    L = max(0.0, min(1.0, L))
    lin = _lab_to_linear(L, a, b)
    if any(v < -1e-9 or v > 1 + 1e-9 for v in lin):
        lo, hi = 0.0, 1.0
        for _ in range(24):
            k = (lo + hi) / 2
            t = _lab_to_linear(L, a * k, b * k)
            if any(v < -1e-9 or v > 1 + 1e-9 for v in t):
                hi = k
            else:
                lo = k
        lin = _lab_to_linear(L, a * lo, b * lo)
    return tuple(int(math.floor(_gam(max(0.0, min(1.0, v))) + 0.5)) for v in lin)


def mix(c1, c2, t):
    """c1 moved a fraction t toward c2, in OKLab. Takes and gives sRGB."""
    p, q = oklab(rgb(c1)), oklab(rgb(c2))
    return srgb(tuple(p[i] + (q[i] - p[i]) * t for i in range(3)))


def lighten(c, dl):
    """c with its OKLab lightness moved by dl."""
    L, a, b = oklab(rgb(c))
    return srgb((L + dl, a, b))


def lightness(c):
    return oklab(rgb(c))[0]


def delta_e(c1, c2):
    """OKLab distance (0.02 is barely there, 0.05 clearly different, 0.1 another colour)."""
    p, q = oklab(rgb(c1)), oklab(rgb(c2))
    return math.sqrt(sum((p[i] - q[i]) ** 2 for i in range(3)))


# --------------------------------------------------------------------------- #
# The tone formulas: the owner's art guide (tools/source/art-guide.png)
# --------------------------------------------------------------------------- #

INK = "#1b1817"
"""Near-black brown: deep tones lean to this."""
LINE_INK = "#151312"
"""Lines are the material mixed most of the way to this: dark brown to near-black."""
SHADE = "#2d2530"
"""The form shadow leans to this cool dark: a soft fill keeps it gentle."""
SKIN_SHADE = "#5d4237"
"""Skin's shadow is warm, toward red-brown."""
SKIN_INK = "#241a17"
LIGHT_TO = "#f2e6d2"
HIGHLIGHT_TO = "#f7efe2"
METAL_SHINE = "#fbf6ee"
RIM = "#e9dfd2"
"""Kept for older callers; the rim is now each part's own lit tone, thin and neutral (style.py)."""
TERMINATOR = SKIN_SHADE
SKIN_LINE = SKIN_INK
BLUSH = "#b98478"
"""A muted blush, for faces only."""
SKY = ("#323749", "#ae8d8c", "#e1cdbc")
"""Kept for older callers: a metal band, if one is asked for, in the guide's navy, dusty pink and cream."""
LINE_MIN_L = 0.10
"""No line is darker than this OKLab lightness (near-black, never #000)."""
LINE_MAX_L = 0.22
"""No line is lighter than this: lines are dark brown to near-black."""

TONE_NAMES = ("line", "deep", "deep-shadow", "shadow", "terminator", "base", "base-light", "light",
              "light-highlight", "highlight")
"""The ramp's ten entries, by index."""


def _clamp_line(c):
    L, a, b = oklab(rgb(c))
    L2 = min(max(L, LINE_MIN_L), LINE_MAX_L)
    return srgb((L2, a, b)) if L2 != L else rgb(c)


def tone_set(base, kind="cloth"):
    """The ten ramp colours for a base colour, as a list indexed like TONE_NAMES, measured off the
    guide's characters (its shadows sit 0.04 to 0.15 under the base in OKLab lightness, its lines
    at 0.22 to 0.30):

    shadow    mix(base, #2d2530, 0.25), then L - 0.03: hard-edged, gentle, the fill keeping it light
    deep      mix(base, #1c1512, 0.50): creases and the undersides of overlaps
    light     mix(base, #f2e6d2, 0.14): a modest lit plane
    highlight mix(base, #f7efe2, 0.35) (metal: #fbf6ee, 0.60, its glint)
    line      mix(base, #140d0b, 0.92), kept between OKLab L 0.10 and 0.22: dark brown to near-black
    skin      its shadow mix(base, #6a3a2e, 0.40), warm; its line mix(base, #2a1611, 0.88)
    The even entries between are the OKLab midpoints, used where tones meet and in soft glows. No
    tone's chroma rises above the base's toward the lights, and toward the darks it falls faster than
    the lightness (to the 1.6 power), so the shading never saturates; a line's chroma stays under
    0.012, near-neutral.
    """
    base = rgb(base)
    if kind == "skin":
        shadow = lighten(mix(base, SKIN_SHADE, 0.40), -0.02)
    else:
        shadow = lighten(mix(base, SHADE, 0.25), -0.03)
    deep = mix(base, INK, 0.50)
    if lightness(deep) > lightness(shadow) - 0.03:
        L, a, b = oklab(deep)
        deep = srgb((max(0.08, lightness(shadow) - 0.04), a, b))
    light = mix(base, LIGHT_TO, 0.14)
    high = mix(base, METAL_SHINE, 0.60) if kind == "metal" else mix(base, HIGHLIGHT_TO, 0.35)
    mid = lambda p, q: mix(p, q, 0.5)  # noqa: E731
    line = mix(base, SKIN_INK, 0.88) if kind == "skin" else mix(base, LINE_INK, 0.92)
    if lightness(line) > lightness(deep) - 0.02:
        L, a, b = oklab(line)
        line = srgb((lightness(deep) - 0.04, a, b))
    line = _clamp_line(line)
    L, a, b = oklab(line)
    C = math.hypot(a, b)
    if C > 0.012:                              # lines are near-neutral dark brown, as the guide's
        line = srgb((L, a * 0.012 / C, b * 0.012 / C))
    row = [line, deep, mid(deep, shadow), shadow, mid(shadow, base), base, mid(base, light), light,
           mid(light, high), high]
    return [_tame(c, base) for c in row]


def _tame(c, base):
    """Holds a tone's chroma no higher than the base's toward the lights, and falling with its
    lightness toward the darks, so shading never saturates (the guide's tones are muted)."""
    L, a, b = oklab(rgb(c))
    L0, a0, b0 = oklab(rgb(base))
    C, C0 = math.hypot(a, b), math.hypot(a0, b0)
    cap = C0 if L >= L0 else C0 * max(0.0, L / max(L0, 1e-6)) ** 1.6
    if C <= cap or C <= 1e-9:
        return rgb(c)
    k = cap / C
    return srgb((L, a * k, b * k))


# --------------------------------------------------------------------------- #
# Cutting the palette from the supplied art
# --------------------------------------------------------------------------- #

GRID_Y = (66, 133, 199, 266, 335, 406, 474, 542, 609, 677, 746, 800)
GRID_X = (37, 81, 125, 169, 213, 257, 300, 344)
SHORT_ROWS = (7, 10)
"""Rows with seven swatches, not eight."""

NPC_SKIN_X = (28, 67, 106, 145, 184, 224, 263, 301)
NPC_SKIN_Y = (45, 65)
NPC_HAIR = ((15, 53), (64, 103), (114, 153), (164, 206), (219, 258), (270, 312))
NPC_HAIR_Y = (670, 725)
NPC_HAIR_NAMES = ("black", "brown", "blonde", "red", "silver", "purple")

_sheet = {}


def _image(name):
    if name not in _sheet:
        _sheet[name] = vector.read_png(os.path.join(SOURCE, name))
    return _sheet[name]


def _median(block):
    import numpy as np
    flat = block.reshape(-1, block.shape[-1])[:, :3].astype(np.float64)
    return tuple(int(v) for v in np.rint(np.median(flat, axis=0)))


def swatch(row, col):
    """The style sheet's swatch at row 1..12, column 1..8."""
    if row in SHORT_ROWS and col == 8:
        raise ValueError("row %d has seven swatches" % row)
    img = _image("style-sheet.png")
    x, y = GRID_X[col - 1], GRID_Y[row - 1]
    return _median(img[y - 8:y + 9, x - 8:x + 9])


def npc_skin(i):
    """npc-customization.png's skin swatch i, 0 (palest) .. 7 (deepest)."""
    img = _image("npc-customization.png")
    x = NPC_SKIN_X[i]
    return _median(img[NPC_SKIN_Y[0]:NPC_SKIN_Y[1], x - 6:x + 7])


def npc_hair(i):
    """npc-customization.png's hair colour i (black, brown, blonde, red, silver, purple): the
    median of the head's lit half, its brightest 40% of pixels against the dark backdrop."""
    import numpy as np
    img = _image("npc-customization.png")
    x0, x1 = NPC_HAIR[i]
    block = img[NPC_HAIR_Y[0]:NPC_HAIR_Y[1], x0:x1 + 1, :3].reshape(-1, 3).astype(np.int64)
    hair = block[np.abs(block - 12).sum(1) > 70]
    luma = hair[:, 0] * 299 + hair[:, 1] * 587 + hair[:, 2] * 114
    order = np.argsort(luma, kind="stable")
    lit = hair[order[int(len(order) * 0.6):]]
    return tuple(int(v) for v in np.rint(np.median(lit.astype(np.float64), axis=0)))


#: The owner's art guide's 5 x 4 palette (tools/source/art-guide.png, its Palette panel): the
#: swatches' centres, sampled live as 13 x 13 medians. Rows: neutrals; reds and purples; blues and
#: greens; soft and warm.
GUIDE = "art-guide.png"
GUIDE_X = (1365, 1400, 1432, 1465, 1499)
GUIDE_Y = (646, 681, 716, 751)
GUIDE_NAMES = (("cream", "tan", "brown", "dark-brown", "near-black"),
               ("burgundy", "wine", "plum", "dark-purple", "black"),
               ("navy", "slate-blue", "teal-grey", "olive", "dark-green"),
               ("dusty-pink", "grey", "khaki", "olive-brown", "mustard"))


def guide_swatch(row, col):
    """The guide's palette swatch at row 1..4, column 1..5."""
    img = _image(GUIDE)
    x, y = GUIDE_X[col - 1], GUIDE_Y[row - 1]
    return _median(img[y - 6:y + 7, x - 6:x + 7])


def guide_swatches():
    """All twenty, as {name: (r, g, b)}."""
    return {GUIDE_NAMES[r][c]: guide_swatch(r + 1, c + 1) for r in range(4) for c in range(5)}


#: Skin and hair, sampled off the guide's Head & Face and Hair panels and its characters (the median
#: of each sample's lit half), fixed here so the ramps don't move if a crop does. Where sampled:
GUIDE_SAMPLES = {
    "skin-very-light": ("#e6b08f", "Head & Face, lower row, third face"),
    "skin-light": ("#dba684", "Head & Face, top row, fourth face"),
    "skin-light-medium": ("#cf9776", "Head & Face, lower row, second face"),
    "skin-medium": ("#c28a69", "Head & Face, lower row, fifth face"),
    "skin-tan": ("#a56e4d", "Head & Face, the bald man"),
    "skin-brown": ("#975e3e", "Kell's arm"),
    "skin-deep": ("#7c4a33", "Kell's face in shadow"),
    "skin-deepest": ("#5e3b2a", "Drainor's face under his hood"),
    "hair-brown": ("#654334", "Head & Face, the brown-haired youth"),
    "hair-black": ("#362c29", "Hair, the long straight style"),
    "hair-blonde": ("#b58a5e", "Kian's braid, lit"),
    "hair-red": ("#8f4934", "Head & Face, the auburn bun, lit"),
    "hair-silver": ("#c0ac9e", "Head & Face, the elder woman"),
    "hair-grey": ("#9b7d68", "Head & Face, the grey-haired man"),
    "hair-ash": ("#b69a82", "Hair, the light tousled style, lit"),
    "hair-auburn": ("#7e3e31", "Head & Face, the auburn bun"),
    "hair-purple-black": ("#2f2b35", "Elara's hair"),
    "metal-steel": ("#736566", "Kell's shoulder guard"),
    "metal-silver": ("#cbbcb6", "Valencia's crown"),
}

#: Every named colour, as where it's cut from: ("g", row, col) a guide swatch; ("gs", name) a guide
#: sample above; ("s", row, col) the old style sheet's swatch, for glows only (magic may keep a
#: stronger accent). Every id the kits and looks already use is still here, pointed at the guide.
SOURCES = {
    # skin.0 .. skin.7, very light to deep brown
    "skin.0": ("gs", "skin-very-light"), "skin.1": ("gs", "skin-light"), "skin.2": ("gs", "skin-light-medium"),
    "skin.3": ("gs", "skin-medium"), "skin.4": ("gs", "skin-tan"), "skin.5": ("gs", "skin-brown"),
    "skin.6": ("gs", "skin-deep"), "skin.7": ("gs", "skin-deepest"),
    # hair: the old ten ids, then the guide's own
    "hair.chestnut": ("gs", "hair-brown"), "hair.black": ("gs", "hair-black"), "hair.gold": ("gs", "hair-blonde"),
    "hair.copper": ("gs", "hair-red"), "hair.silver": ("gs", "hair-silver"), "hair.midnight": ("gs", "hair-purple-black"),
    "hair.rose": ("g", 2, 2), "hair.lavender": ("g", 2, 4), "hair.plum": ("g", 2, 3),
    "hair.platinum": ("gs", "hair-ash"), "hair.grey": ("gs", "hair-grey"), "hair.auburn": ("gs", "hair-auburn"),
    "hair.brown": ("gs", "hair-brown"), "hair.blonde": ("gs", "hair-blonde"), "hair.red": ("gs", "hair-red"),
    # eyes
    "eyes.blue": ("g", 3, 2), "eyes.brown": ("g", 1, 3), "eyes.green": ("g", 3, 4), "eyes.amber": ("g", 4, 5),
    "eyes.grey": ("g", 4, 2), "eyes.violet": ("g", 2, 4), "eyes.teal": ("g", 3, 3), "eyes.hazel": ("g", 4, 4),
    "eyes.dark": ("g", 1, 4),
    # cloth: the old dye names, each re-pointed at its nearest guide swatch, then the swatches by name
    "cloth.cream": ("g", 1, 1), "cloth.white": ("g", 1, 1), "cloth.sky": ("g", 3, 2), "cloth.blush": ("g", 4, 1),
    "cloth.oat": ("g", 1, 2), "cloth.woad": ("g", 3, 1), "cloth.teal": ("g", 3, 3), "cloth.forest": ("g", 3, 5),
    "cloth.crimson": ("g", 2, 1), "cloth.plum": ("g", 2, 3), "cloth.ochre": ("g", 4, 4), "cloth.charcoal": ("g", 2, 5),
    "cloth.rose": ("g", 2, 2), "cloth.bark": ("g", 1, 4), "cloth.navy": ("g", 3, 1), "cloth.olive": ("g", 3, 4),
    "cloth.sand": ("g", 1, 2), "cloth.wine": ("g", 2, 2), "cloth.lavender": ("g", 2, 4), "cloth.grey": ("g", 4, 2),
    "cloth.slate": ("g", 3, 2), "cloth.moss": ("g", 4, 4), "cloth.parchment": ("g", 1, 1), "cloth.cobalt": ("g", 3, 2),
    "cloth.tan": ("g", 1, 2), "cloth.brown": ("g", 1, 3), "cloth.dark-brown": ("g", 1, 4), "cloth.near-black": ("g", 1, 5),
    "cloth.burgundy": ("g", 2, 1), "cloth.dark-purple": ("g", 2, 4), "cloth.slate-blue": ("g", 3, 2),
    "cloth.teal-grey": ("g", 3, 3), "cloth.dark-green": ("g", 3, 5), "cloth.dusty-pink": ("g", 4, 1),
    "cloth.khaki": ("g", 4, 3), "cloth.olive-brown": ("g", 4, 4), "cloth.mustard": ("g", 4, 5),
    # metal and leather
    "metal.iron": ("gs", "metal-steel"), "metal.blackened": ("g", 1, 5), "metal.bronze": ("g", 1, 2),
    "metal.gold": ("g", 4, 5), "metal.sunsteel": ("gs", "metal-silver"), "metal.silver": ("gs", "metal-silver"),
    "leather.dark": ("g", 1, 4), "leather.black": ("g", 1, 5), "leather.tan": ("g", 1, 2),
    "leather.red": ("g", 2, 1), "leather.brown": ("g", 1, 3),
    # stone, and the glows (magic may keep a stronger accent: violet, ember, gold)
    "stone.dressed": ("g", 1, 1), "stone.grey": ("g", 4, 2), "stone.dark": ("g", 4, 3),
    "glow.gold": ("s", 9, 5), "glow.violet": ("s", 8, 7), "glow.sky": ("g", 3, 3), "glow.pink": ("g", 4, 1),
    "glow.ember": ("hex", "#d9662b"),
}

KIND_OF_FAMILY = {"skin": "skin", "hair": "hair", "eyes": "eyes", "cloth": "cloth", "metal": "metal",
                  "leather": "leather", "stone": "stone", "glow": "glow", "fur": "hair"}

#: 03-art-overhaul.md §7.3's reserved hues that have a colour. A generic NPC's palette within
#: OKLab 0.05 of one is rejected (check_looks.py). The Watch's navy and brass, the Lodge's moss and
#: Harrow's heron green are reserved by name only.
RESERVED = {
    "violet": "#7a3fb0", "sun-gold": "#e8b84a", "wine-red": "#7d1f2e", "guild-cobalt": "#2c4fa3",
    "ember": "#d9662b",
}
RESERVED_DISTANCE = 0.05

_bases = {}
_extra = {}


def _cut(src):
    if src[0] == "g":
        return guide_swatch(src[1], src[2])
    if src[0] == "gs":
        return rgb(GUIDE_SAMPLES[src[1]][0])
    if src[0] == "hex":
        return rgb(src[1])
    if src[0] == "s":
        return swatch(src[1], src[2])
    if src[0] == "skin":
        return npc_skin(src[1])
    if src[0] == "hair":
        return npc_hair(src[1])
    raise ValueError(src)


def base(rid):
    """A ramp's base colour."""
    if rid in _extra:
        return _extra[rid][0]
    if rid not in _bases:
        if rid not in SOURCES:
            raise KeyError("no ramp " + rid + "; theme.register() makes one")
        _bases[rid] = _cut(SOURCES[rid])
    return _bases[rid]


def kind_of(rid):
    if rid in _extra:
        return _extra[rid][1]
    return KIND_OF_FAMILY.get(rid.split(".")[0], "cloth")


def register(rid, base_colour, kind=None):
    """Adds a ramp of your own (an enemy's colours, a notable's reserved hue). Its id is
    family.name; the family sets how it's built (metal gets the sky band, skin the terminator)
    unless `kind` says. It's written to ramps.png and ramps.json with the rest."""
    _extra[rid] = (rgb(base_colour), kind or KIND_OF_FAMILY.get(rid.split(".")[0], "cloth"))
    return rid


def ids():
    """Every ramp id, in ramps.png's row order: the cut ones, then the registered ones."""
    return list(SOURCES) + [r for r in sorted(_extra) if r not in SOURCES]


def ramp(rid):
    """The nine steps of a ramp, deep .. highlight, as (r, g, b) tuples."""
    return tone_set(base(rid), kind_of(rid))[1:]


def ramp_row(rid):
    """The ten colours of ramps.png's row: the line, then the nine steps."""
    return tone_set(base(rid), kind_of(rid))


def tones(col, group=0, kind=None):
    """The named tones of a colour or a ramp, as a dict: line, deep, shadow, base, light,
    highlight, rim, terminator, blush; `steps` (all ten, by index); the tint `group` to paint
    into; the `kind` it was built as. `col` is a ramp id ("skin.3"), '#rrggbb' or (r, g, b)."""
    if isinstance(col, str) and not col.startswith("#"):
        row = ramp_row(col)
        k = kind_of(col)
    else:
        k = kind or "cloth"
        row = tone_set(col, k)
    named = {name: row[i] for i, name in enumerate(TONE_NAMES)}
    named.update({"rim": rgb(RIM), "blush": rgb(BLUSH), "steps": [tuple(c) for c in row],
                  "group": group, "kind": k})
    return named


def grey_tones(group):
    """Tones that paint a tint group in its grey ramp positions (what a kit's fill stores)."""
    row = [(round(i * 255 / 9),) * 3 for i in range(10)]
    named = {name: row[i] for i, name in enumerate(TONE_NAMES)}
    named.update({"rim": rgb(RIM), "blush": rgb(BLUSH), "steps": row, "group": group, "kind": "grey"})
    return named


def reserved_clash(col):
    """The reserved hue a colour is within RESERVED_DISTANCE of, or None."""
    for name, hexc in RESERVED.items():
        if delta_e(col, hexc) < RESERVED_DISTANCE:
            return name
    return None


# --------------------------------------------------------------------------- #
# Writing ramps.png and ramps.json
# --------------------------------------------------------------------------- #

def _write_text(path, text):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "w", encoding="utf-8", newline="\r\n") as f:
        f.write(text)
    print("  " + os.path.relpath(path, ROOT).replace(os.sep, "/"))


def _where(src):
    if not src:
        return "registered"
    if src[0] == "g":
        return "art-guide palette r%dc%d (%s)" % (src[1], src[2], GUIDE_NAMES[src[1] - 1][src[2] - 1])
    if src[0] == "gs":
        return "art-guide: " + GUIDE_SAMPLES[src[1]][1]
    if src[0] == "s":
        return "style-sheet r%dc%d" % (src[1], src[2])
    return src[0] + " " + str(src[1])


def write(out=OUT):
    import numpy as np
    order = ids()
    img = np.zeros((len(order), 10, 4), np.uint8)
    table = {}
    for y, rid in enumerate(order):
        row = ramp_row(rid)
        for x, c in enumerate(row):
            img[y, x] = (c[0], c[1], c[2], 255)
        src = SOURCES.get(rid)
        table[rid] = {
            "row": y,
            "kind": kind_of(rid),
            "base": hexof(base(rid)),
            "from": _where(src),
            "line": hexof(row[0]),
            "steps": [hexof(c) for c in row[1:]],
        }
    vector.write_png(os.path.join(out, "ramps.png"), img)
    doc = {
        "format": "LegendQuest rig ramps 1",
        "texture": "ramps.png",
        "columns": list(TONE_NAMES),
        "maskG": "index = G * 9 / 255; colour = lerp(row[floor(index)], row[ceil(index)], fraction)",
        "rim": hexof(RIM),
        "ink": hexof(INK),
        "reserved": RESERVED,
        "ramps": table,
    }
    _write_text(os.path.join(out, "ramps.json"), json.dumps(doc, indent=1) + "\n")
    return order


def collect():
    """Imports every tools/rig module that registers ramps (one whose source says theme.register),
    so ramps.png has them however it's written. Register at import time, at module level."""
    import importlib
    for name in sorted(os.listdir(HERE)):
        if not name.endswith(".py") or name in ("theme.py", "generate_rig_kits.py"):
            continue
        if "theme.register(" in open(os.path.join(HERE, name), encoding="utf-8").read():
            importlib.import_module(name[:-3])


def main():
    print("ramps:")
    collect()
    order = write()
    for fam in ("skin", "hair", "eyes", "cloth", "metal", "leather", "stone", "glow"):
        n = sum(1 for r in order if r.startswith(fam + "."))
        print("  %-8s %d" % (fam, n))


if __name__ == "__main__":
    # the generators collect() imports say `import theme`: make that this module, not a second copy
    sys.modules.setdefault("theme", sys.modules["__main__"])
    main()
