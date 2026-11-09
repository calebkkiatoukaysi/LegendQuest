"""
Cuts the character parts made in ChatGPT into the game's sockets and atlases: the two bases and every
face, hair, helm, armor, cape, weapon and off-hand item there is.

    python tools/import_parts.py           # import everything in tools/source/characters
    python tools/import_parts.py --check   # say what's out of date and change nothing (exit 1 if anything is)

Every part is an edit of a base (Story/13-art-prompts.md §2.3), so it lines up with the base pixel
for pixel, on the base's 1024 x 1536 canvas on solid black. This:

  - measures each base's sockets, the points parts hang from: root (the ground between the feet),
    neck (the middle of the jaw seam, where the head sits), crown, chest, back (where a cape hangs),
    and the near and far hands (the near one holds the weapon); and the box round the head that
    the portraits crop;
  - keys each base off its black and splits it at the jaw seam into its body, which goes a little
    way up under the jaw so a smaller head never leaves a gap, and its bald head, the face of
    anyone without one; the body's skin mask goes with it, so the game can tint it to a face's skin,
    and the base's skin tone (its cheeks), which a skin picked on the creation screen is against;
  - draws everyone's head as the mannequin's own: the blank-faced base (base-male-blank.png and
    base-female-blank.png, edits of the bases with the features taken off) once there is one, the
    base's own head till then. Nobody's head changes shape, so a hair or a helm fits every head;
  - cuts each face (tools/source/characters/faces/<id>.png, an edit of the blank base, from
    tools/part_sheet.py or one edit at a time) into its layer: everything above the seam, and below
    it whatever ChatGPT painted that isn't the base's body, a beard falling past the jaw. A face
    painted the old way, head and hair together (faces/head-and-hair/<id>.png, an edit of the base
    with its features), is split: its hair on the scalp becomes the part hair-<x>, and the rest,
    eyebrows and a man's beard with it, stays the face;
  - cuts each hair and helm (hairs/hair-<x>.png, helms/helm-<x>.png, edits of the blank base) into
    what it painted on and round the head. Hair and helm are one slot, what's on the head: a helm
    with hair showing has it painted in; It measures the face's
    skin against the base's on the lower face and the top of the neck, and fades the hair out where
    a sheet's cell cut it off. Its mask has its skin, its hair (what isn't the bald head: hair,
    eyebrows, a beard) and its eyes (the coloured part of where the base's eyes are), with the tone
    the hair and the eyes were painted, so the game can dye them;
  - cuts each armor (tools/source/characters/armors/<id>.png) into its layer: the whole figure from
    the jaw down, like the base's body. Its mask has its skin, where the outfit leaves the base's
    skin showing, so the game tints it to the face's; and its Base and Trim, the two biggest
    materials it's made of by colour (materials), with the tone each was painted, so the game can
    dye them to the colours the player picks;
  - cuts each cape (capes/cape-<x>.png) into what it painted in the black behind and round the
    figure, grown a few pixels in under the body's edge, with its Base and Trim to dye; and each
    weapon and off-hand item (weapons/weapon-<x>.png, weapons/offhand-<x>.png) into the changed
    pieces that reach a hand, leaving out the base's fingers where ChatGPT redrew them;
  - makes each base's two holding hands, the fists drawn over whatever a hand holds so the fingers
    close over the grip: the ones painted for it (hands/hand-<man or woman>.png, Story/13 §3.3), or
    till they come a plain fist of its own (simple_fist); kept as shade, the game colours them as
    the skin or the glove they cover (Characters/Parts/RigComposer.cs's LiftFist);
  - shrinks every layer to PART_SCALE on the canvas's own grid, so a part drawn at its socket lands
    on the pixel it was painted on, records each one's pivot (its socket in its own pixels), and
    packs them into atlas pages, one set per slot;
  - with VECTOR on (or --vector for one run), redraws every layer as vector art first
    (tools/vectorize.py): flat cel colours and smooth lines, drawn back at full size before it's
    shrunk. The masks, skins and sockets are still measured off the painted pictures. Each part's
    shapes are kept in VECTOR_CACHE, by what they were made from, so only a part that's changed is
    traced again;
  - reads the recipes (§7) and writes Content/Characters/Parts/parts.json: the scale, the sockets,
    every part's page, rectangle, socket and pivot, and every recipe;
  - rewrites its own sections of Content/Content.mgcb and ASSETS.md, and takes out what it no
    longer makes;
  - saves previews in shots/art/parts/: the sockets on each base, every face on its base the way
    the portraits crop it, every face's layer alone, and every recipe with a face put together at
    its sockets and morph, the way Characters/Parts/RigComposer.cs does it.

The game puts the characters together from these (Characters/Parts). Only needs CPython and numpy,
and nothing in it is random, so the same sources always give the same bytes.
"""

import argparse
import copy
import glob
import json
import os
import re
import sys

import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
import import_character as ic  # noqa: E402  (its PNG reader and writer, masks, resampling, game data and section writer)

ROOT = os.path.dirname(HERE)
SOURCE = os.path.join(ROOT, "tools", "source", "characters")
PARTS = os.path.join(ROOT, "Content", "Characters", "Parts")
MANIFEST = os.path.join(PARTS, "parts.json")
STORY = os.path.join(ROOT, "Story", "13-art-prompts.md")
PREVIEWS = os.path.join(ROOT, "shots", "art", "parts")

#: The bases' canvas, and how tall the standard adult on it stands from the crown (y 115) to the soles (y 1466).
CANVAS = (1024, 1536)
STANDARD = 1351
#: The scale the parts are kept at: half the canvas, so a standard adult is 675.5 pixels tall,
#: enough for a portrait's well to shrink the face rather than blow it up.
PART_SCALE = 0.5

BASES = ("base-male", "base-female")
#: The jaw seam (§2.3): from under the ear to the chin. Above it is the face's (or the helm's), below it the body's.
SEAM = {"base-male": ((510, 250), (595, 293)), "base-female": ((490, 272), (578, 325))}
#: Where to look for each hand (the near one is the lit one on the left), and the shoulder line.
#: Each base's fists themselves (left, right, top, bottom on the canvas), the near one first: clear of
#: the thigh and the shorts beside them, the wrist at the top. What a hand holds is drawn under its
#: fist (fist_mask; Characters/Parts/RigComposer.cs's LiftFist). tools/part_sheet.py's FIST_BOXES are
#: the same boxes, for the weapon sheets' fists.
FISTS = {"base-male": ((276, 398, 760, 862), (663, 762, 760, 860)),
         "base-female": ((296, 398, 760, 862), (657, 738, 760, 860))}
NEAR_HAND = {"base-male": (270, 770, 410, 860), "base-female": (295, 770, 425, 860)}
FAR_HAND = {"base-male": (640, 760, 760, 860), "base-female": (640, 760, 745, 860)}
SHOULDERS = {"base-male": 330, "base-female": 365}
#: How far above the seam the body goes, under the jaw, in the canvas's pixels.
SKIRT = 10

#: Every socket a base has, and the slot each part hangs from which socket.
SOCKETS = ("root", "neck", "crown", "chest", "back", "hand.near", "hand.far")
SLOT_SOCKET = {"body": "root", "head": "neck", "face": "neck", "armor": "root", "cape": "back", "helm": "neck",
               "weapon": "hand.near", "offhand": "hand.far"}
#: The folders under tools/source/characters a slot's parts are in.
FOLDERS = {"face": "faces", "armor": "armors", "hair": "hairs", "helm": "helms", "cape": "capes", "weapon": "weapons",
           "offhand": "weapons"}

# The faces painted before the head was fixed, head and hair together on the bases with their
# features: each is split into its face and its hair (split_face).
OLD_FACES = os.path.join(SOURCE, "faces", "head-and-hair")
# The blank-faced bases the new faces, hairs and helms are painted on, beside the bases.
BLANK = "%s-blank.png"
# Splitting an old face: on the front of the head (from FRONT of the way across the head box), the
# eyebrows (from BROWS of the head's height above the eyes down to them) and, on a man, a beard
# (from NOSE of the head's height below the eyes down) stay with the face; the rest of the hair goes.
FRONT = 0.42
BROWS = 0.10
NOSE = 0.06
# An old face's hair is made a part when it's at least this many pixels; a hair or helm is the
# changed pieces within HEAD_REACH of the head.
HAIR_LEAST = 400
HEAD_REACH = 6

# Whether the parts are redrawn as vector art (tools/vectorize.py) before they're shrunk, and where
# each one's shapes are kept between runs (out of git: they're made from the pictures here).
VECTOR = False
VECTOR_CACHE = os.path.join(ROOT, "tools", ".cache", "vector")

# §7.1's slots, as the game's EquipmentSlot names them in parts.json, and the part slot each fills.
GEAR_SLOTS = {"body": ("body", "armor"), "cloak": ("cloak", "cape"), "head": ("head", "helm"),
              "main hand": ("mainhand", "weapon"), "off hand": ("offhand", "offhand")}
#: The largest an atlas page is.
PAGE = 2048
PAD = 2

#: Keying: the brightest a pixel can be and still be background, the smallest closed-in black that's
#: a gap in the figure, how far round a pixel to look for the figure's colour at its edge, and the
#: least that colour is taken to be.
BLACK = 10
HOLE = 50
EDGE_REACH = 2
EDGE_FLOOR = 24
#: Below the seam: how different from the (tinted) base a pixel has to be to be the face's, the
#: opening that keeps out the base's outlines ChatGPT moved a pixel or two, how far a kept piece
#: grows back along its thin ends, and the biggest gap between strands that's filled.
CHANGED = 48
THIN = 2
#: The width of a shade of skin, in green and blue against red, when looking for a face's commonest.
CHROMA = 0.03
REGROW = 8
GAP = 400
#: How far up from the bottom of a sheet's cell hair fades out, where the cell cut it off, and how
#: much of its top and sides is left out, where its grey lines can be.
FADE = 28
EDGE = 6
#: How near the head a separate piece (a loose strand, an earring) has to be to stay.
NEAR = 12
#: A held piece (a weapon, an off-hand item) is the changed pieces that come within HAND_REACH of
#: either hand's middle (a fist is about 120 by 100, so this reaches a little past its edges, where a
#: blade or a pommel starts); a cape is grown UNDER pixels in under the body's edge, in its own
#: colours, so the body's soft edge never shows the background through it.
HAND_REACH = 90
UNDER = 4
#: An armor's skin: how far from the base's own pixel it can be and still be all skin, and how much
#: further it fades to none.
SKIN_SAME = 20
SKIN_FADE = 40

# Telling an outfit's materials apart, for its Base and Trim (materials). The outlines split it
# into regions, garment pieces, of at least REGION pixels (gaps in them up to GAP_SEAL * 2 wide
# closed); regions whose colours (L*a*b*, lightness
# counting MATERIAL_LIGHTNESS as much as hue and chroma) are within MATERIAL_SAME are one material,
# and a smaller region within MATERIAL_NEAR of a material's colour goes with it.
REGION = 60
GAP_SEAL = 2
MATERIAL_LIGHTNESS = 0.5
MATERIAL_SAME = 15.0
MATERIAL_NEAR = 22.0
# Lines: darker than LINE_L, or darker than DIM_L and greyer than DIM_C (L* and chroma).
LINE_L = 12.0
DIM_L, DIM_C = 24.0, 7.0
# A dye channel is a material with at least this share of what can be dyed, no darker than this luma.
LEAST_MATERIAL = 0.04
TONE_FLOOR = 0.1
# A face: its skin and hair told apart by which they're nearer (a colour distance like
# MATERIAL_SAME's), the hair no further than HAIR_FAR from its own colours, and in pieces of
# HAIR_SPECK or more. Its eyes: within EYE_REACH of the base's irises (bluer than they're red by
# EYE_BLUE), the whites (lighter than WHITE_L, greyer than WHITE_C) and what's coloured within
# IRIS_REACH of them that's no nearer the skin than IRIS_SKIN.
HAIR_FAR = 30.0
FACE_EDGE, FACE_EDGE_REACH = 10.0, 3
THICK_HAIR = 2
HAIR_SPECK = 40
EYE_BLUE = 25
EYE_REACH = 14
WHITE_L, WHITE_C = 72.0, 18.0
IRIS_REACH = 9
IRIS_SKIN = 18.0

A_BEGIN = "<!-- character parts: written by tools/import_parts.py; don't edit between these two lines -->"
A_END = "<!-- end character parts -->"
A_ANCHOR = "## Tiles and props"
M_BEGIN = "#------------ Characters/Parts: written by tools/import_parts.py ------------#"
M_END = "#---------------------------- end Characters/Parts ----------------------------#"
CREDIT = "supplied by Caleb Kiatoukaysi, made in ChatGPT; cut by tools/import_parts.py"


def fail(message):
    print("import_parts: " + message, file=sys.stderr)
    sys.exit(1)


def rel(path):
    return os.path.relpath(path, ROOT).replace(os.sep, "/")


# --------------------------------------------------------------------------- #
# The prompt book: what each part edits, and the recipes
# --------------------------------------------------------------------------- #

def story():
    with open(STORY, encoding="utf-8") as f:
        return f.read().replace("\r\n", "\n")


def basis(text):
    """{part id: the id it edits}, from each entry's **Basis:** line."""
    out = {}
    for m in re.finditer(r"^#### `([a-z0-9.-]+)` ·[^\n]*\n+\*\*Basis:\*\* Edits `([a-z0-9.-]+)`", text, re.M):
        out[m.group(1)] = m.group(2)
    return out


def base_of(part, edits):
    """Which base a part is on: the one it edits, or the one the part it edits is on."""
    seen = set()
    while part not in BASES:
        if part in seen or part not in edits:
            return None
        seen.add(part)
        part = edits[part]
    return part


def table(text, start, end):
    """The rows of the markdown table between two headings, as lists of cells, `—` as None and
    the backticks off."""
    a, b = text.index(start), text.index(end)
    out = []
    for line in text[a:b].split("\n"):
        if line.startswith("| `"):
            cells = [c.strip() for c in line.strip().strip("|").split("|")]
            out.append([None if c in ("—", "-", "") else c.strip("`") for c in cells])
    return out


def gear(text, edits):
    """§7.1: what each item draws on each base, as [{"item", "slot", "parts": {base: part or
    None}}]. A part has to be in the book, in the slot the item's slot fills, and an armor on the
    base it's listed under."""
    out, seen = [], set()
    for cells in table(text, "\n### 7.1 ", "\n### 7.2 "):
        if len(cells) != 4:
            fail("§7.1's row for %s should have 4 cells: item, slot, man, woman" % cells[0])
        item, slot, man, woman = cells
        if slot not in GEAR_SLOTS:
            fail("§7.1 puts %s in the slot %r; it's one of %s" % (item, slot, ", ".join(GEAR_SLOTS)))
        name, part_slot = GEAR_SLOTS[slot]
        if (item, name) in seen:
            fail("§7.1 has %s in the %s twice" % (item, slot))
        seen.add((item, name))
        parts = {}
        for base, part in (("base-male", man), ("base-female", woman)):
            check_gear_part(part, part_slot, base if part_slot == "armor" else None, edits, "§7.1's %s (%s)" % (item, slot))
            parts[base] = part
        out.append({"item": item, "slot": name, "parts": parts})
    return out


def own_gear(text, edits, book):
    """§7.2: the party's own pieces for their starting kit, as {recipe: [{"slot", "item", "part"
    or None}]}."""
    out = {}
    for cells in table(text, "\n### 7.2 ", "\n## 8."):
        if len(cells) != 4:
            fail("§7.2's row for %s should have 4 cells: who, slot, item, part" % cells[0])
        who, slot, item, part = cells
        if who not in book:
            fail("§7.2 gives %s their own gear, but there's no recipe %s in §7" % (who, who))
        if slot not in GEAR_SLOTS:
            fail("§7.2 puts %s's %s in the slot %r; it's one of %s" % (who, item, slot, ", ".join(GEAR_SLOTS)))
        name, part_slot = GEAR_SLOTS[slot]
        rows = out.setdefault(who, [])
        if any(r["slot"] == name and r["item"] == item for r in rows):
            fail("§7.2 has %s's %s in the %s twice" % (who, item, slot))
        face = book[who]["face"]
        check_gear_part(part, part_slot, base_of(face, edits) if part_slot == "armor" and face else None, edits,
                        "§7.2's %s (%s, %s)" % (who, item, slot))
        rows.append({"slot": name, "item": item, "part": part})
    return out


def check_gear_part(part, slot, base, edits, what):
    if part is None:
        return
    if not part.startswith(slot + "-"):
        fail("%s draws %s, which isn't a %s part" % (what, part, slot))
    if part not in edits:
        fail("%s draws %s, which isn't a part in Story/13-art-prompts.md" % (what, part))
    if base and base_of(part, edits) != base:
        fail("%s draws %s, painted on %s, where it has to be on %s" % (what, part, base_of(part, edits), base))


RECIPE_SLOTS = ("face", "armor", "cape", "helm", "weapon")


def recipes(text):
    """{id: recipe} from §7's table, its columns found by their headings: the part in each slot
    (None for an empty one; the Helm column is what's on the head, a hair or a helm) and the morph."""
    start, end = text.index("\n## 7."), text.index("\n### 7.1 ")
    out = {}
    columns = None
    for line in text[start:end].split("\n"):
        cells = [c.strip() for c in line.strip().strip("|").split("|")]
        if line.startswith("| Id |"):
            columns = [c.lower().split(" ")[0] for c in cells]
            continue
        if not line.startswith("| `") or columns is None or len(cells) != len(columns):
            continue
        row = dict(zip(columns, cells))
        morph = re.match(r"([\d.]+)\s*×\s*([\d.]+)", row.get("morph", ""))
        if not morph:
            fail("§7's row for %s has no morph (h × w): %s" % (cells[0], row.get("morph")))
        recipe = {slot: (None if row.get(slot, "—") in ("—", "-", "") else row[slot].strip("`")) for slot in RECIPE_SLOTS}
        recipe["morph"] = [float(morph.group(1)), float(morph.group(2))]
        out[cells[0].strip("`")] = recipe
    if not out:
        fail("no recipes in Story/13-art-prompts.md §7")
    return out


# --------------------------------------------------------------------------- #
# Pictures
# --------------------------------------------------------------------------- #

def load(path):
    """A source picture as H x W x 3 0-255 ints, flattened onto black if it carries alpha."""
    rgba, own_alpha = ic.read_png(path)
    rgb = rgba[:, :, :3].astype(np.int32)
    if own_alpha:
        rgb = (rgb * rgba[:, :, 3:4].astype(np.int32) + 127) // 255
    if rgb.shape[:2] != (CANVAS[1], CANVAS[0]):
        fail("%s is %dx%d, not the bases' %dx%d" % (rel(path), rgb.shape[1], rgb.shape[0], CANVAS[0], CANVAS[1]))
    return rgb


def seam_line(base):
    """The seam's height in every column: the line between its ends, running on level past them."""
    (x0, y0), (x1, y1) = SEAM[base]
    return np.interp(np.arange(CANVAS[0]), [x0, x1], [y0, y1])


def rows():
    return np.arange(CANVAS[1])[:, None]


def erode(mask, steps):
    return ~ic.grow(~mask, steps)


def bbox(mask):
    ys, xs = np.nonzero(mask)
    if len(ys) == 0:
        return None
    return int(xs.min()), int(ys.min()), int(xs.max()) + 1, int(ys.max()) + 1


def key_black(rgb):
    """A picture on solid black as straight colour (0-255) and alpha (0 to 1). Whatever the edge
    reaches through near-black pixels is background, so the figure's own darks (black hair, its
    outline) stay solid. A background pixel beside the figure is the figure's colour there blended
    with black, so its alpha is how much of that colour it holds, and its colour is that colour: the
    black comes back out, and a pale edge with no outline (white hair) keeps no grey fringe."""
    bright = rgb.max(axis=2)
    dark = bright <= BLACK
    labels, _ = ic.components(dark)
    border = np.unique(np.concatenate([labels[0], labels[-1], labels[:, 0], labels[:, -1]]))
    # Background is what the edge reaches, and any closed-in black big enough and thick enough to be
    # a gap in the figure (between an arm and the body) rather than a dark in it: a pupil, a fold of
    # hair, or an outline drawn nearly black, which is long but thin.
    sizes = np.bincount(labels.ravel())
    thick = np.unique(labels[erode(dark, 2)])
    holes = np.intersect1d(np.nonzero(sizes >= HOLE)[0], thick)
    background = np.isin(labels, np.union1d(border[border > 0], holes[holes > 0]))
    figure = ~background
    edge = ic.grow(figure, EDGE_REACH) & background
    colour = rgb.astype(np.float64)
    alpha = figure.astype(np.float64)
    if edge.any():
        beside = ic.local_mean(colour, figure.astype(np.float64), (EDGE_REACH, EDGE_REACH + 2))
        near = beside[edge]
        strength = np.maximum(near.max(axis=1), EDGE_FLOOR)
        a = np.clip(bright[edge] / strength, 0.0, 1.0)
        alpha[edge] = a
        colour[edge] = np.where(a[:, None] > 0, np.clip(rgb[edge] / np.maximum(a, 1e-6)[:, None], 0, 255), 0)
    colour[alpha == 0] = 0
    return colour, alpha


def skin_weight(rgb):
    """How much each pixel is the base's skin (0 to 1): warm, well saturated and not dark. The
    underwear is greyer, the outlines darker and the eyes blue, so none of them are."""
    c = rgb.astype(np.float64)
    hi, lo = c.max(axis=2), c.min(axis=2)
    sat = (hi - lo) / np.maximum(hi, 1.0)
    warm = (c[:, :, 0] >= c[:, :, 1]) & (c[:, :, 1] >= c[:, :, 2])
    return warm * ic.smoothstep(0.33, 0.42, sat) * ic.smoothstep(60.0, 100.0, hi)


class Layer:
    """A part at PART_SCALE, trimmed to what's there: straight RGBA, its top left on the scaled
    canvas, and its mask (the same size) if it has one: red its skin; green and blue its two dye
    channels, an armor's Base and Trim or a face's hair and eyes."""

    def __init__(self, rgba, at, mask=None):
        self.rgba, self.at, self.mask = rgba, at, mask

    @property
    def size(self):
        return self.rgba.shape[1], self.rgba.shape[0]


def shrink(colour, alpha, mask=None):
    """A full-size layer at PART_SCALE. The box it's cut from is whole pixels of the scaled canvas,
    so a layer put back at its `at` is exactly where it was painted; the filter is Mitchell's, on
    premultiplied colour, so the edges neither darken nor fringe."""
    box = bbox(alpha > 0)
    if box is None:
        fail("a layer has nothing in it")
    x0, y0, x1, y1 = box
    w, h = int(round(CANVAS[0] * PART_SCALE)), int(round(CANVAS[1] * PART_SCALE))
    ox0, oy0 = max(0, int(np.floor(x0 * PART_SCALE)) - 2), max(0, int(np.floor(y0 * PART_SCALE)) - 2)
    ox1, oy1 = min(w, int(np.ceil(x1 * PART_SCALE)) + 2), min(h, int(np.ceil(y1 * PART_SCALE)) + 2)
    source = (ox0 / PART_SCALE, oy0 / PART_SCALE, (ox1 - ox0) / PART_SCALE, (oy1 - oy0) / PART_SCALE)
    size = (ox1 - ox0, oy1 - oy0)
    premultiplied = np.concatenate([colour / 255.0 * alpha[:, :, None], alpha[:, :, None]], axis=2)
    small = ic.straight(ic.resample(premultiplied, source, size))
    sx0, sy0, sx1, sy1 = bbox(small[:, :, 3] > 0)
    rgba = small[sy0:sy1, sx0:sx1]
    channels = None
    if mask is not None:
        mask = mask if mask.ndim == 3 else np.stack([mask, np.zeros(mask.shape), np.zeros(mask.shape)], axis=2)
        channels = np.full(rgba.shape, 255, np.uint8)
        for k in range(3):
            if mask[:, :, k].any():
                small_k = ic.resample(np.repeat(mask[:, :, k:k + 1], 4, axis=2), source, size)[sy0:sy1, sx0:sx1, 0]
                channels[:, :, k] = np.rint(np.clip(small_k, 0.0, 1.0) * 255.0)
            else:
                channels[:, :, k] = 0
    return Layer(rgba, (ox0 + sx0, oy0 + sy0), channels)


# --------------------------------------------------------------------------- #
# Bases: sockets, body and bald head
# --------------------------------------------------------------------------- #

class Base:
    """A base keyed off its black: its colour, alpha and skin, its seam, sockets and head box."""

    def __init__(self, name):
        self.name = name
        self.rgb = load(os.path.join(SOURCE, name + ".png"))
        self.colour, self.alpha = key_black(self.rgb)
        self.skin = skin_weight(self.rgb) * (self.alpha > 0.99)
        self.seam = seam_line(name)
        self.above = rows() < self.seam[None, :]
        self.sockets, self.head = self.measure()
        c = self.rgb.astype(np.int64)
        self.eyes = ic.grow(self.above & (self.alpha > 0.99) & (c[:, :, 2] > c[:, :, 0] + EYE_BLUE) & (c[:, :, 2] > c[:, :, 1]), EYE_REACH)
        l, t, w, h = self.head
        cheeks = (self.skin > 0.9) & (rows() > t + 0.3 * h) & (rows() < SEAM[name][1][1] + 25)
        self.skin_tone = [round(float(v), 1) for v in np.median(self.rgb[cheeks], axis=0)]

    def measure(self):
        """The sockets and the head box, in the canvas's pixels, measured off the figure."""
        solid = self.alpha > 0.5
        sole = int(np.nonzero(solid.any(axis=1))[0].max())

        def middle(x0, y0, x1, y1):
            ys, xs = np.nonzero(solid[y0:y1, x0:x1])
            return float(xs.mean() + x0), float(ys.mean() + y0)

        def low_point(x0, x1):
            """The middle of a foot's sole: the lowest rows of it in those columns."""
            part = solid[:, x0:x1]
            bottom = int(np.nonzero(part.any(axis=1))[0].max())
            xs = np.nonzero(part[bottom - 6:bottom + 1].any(axis=0))[0]
            return (xs.min() + xs.max() + 1) / 2.0 + x0, bottom + 1.0

        span = np.nonzero(solid[sole - 120:sole + 1].any(axis=0))[0]
        split = span[np.argmax(np.diff(span))] + 1 if len(span) > 1 and np.diff(span).max() > 3 else (span.min() + span.max()) // 2
        near_foot, far_foot = low_point(int(span.min()), int(split)), low_point(int(split), int(span.max()) + 1)
        root = ((near_foot[0] + far_foot[0]) / 2.0, sole + 1.0)

        head = solid & self.above
        hx0, hy0, hx1, _ = bbox(head)
        top = np.nonzero(solid[hy0:hy0 + 6])
        crown = (float(top[1].mean()), float(hy0))
        (sx0, sy0), (sx1, sy1) = SEAM[self.name]
        neck = ((sx0 + sx1) / 2.0, (sy0 + sy1) / 2.0)

        shoulder = SHOULDERS[self.name]
        line = np.nonzero(solid[shoulder])[0]
        back = (float(line.min()), float(shoulder))
        chest_y = shoulder + 80
        runs = np.split(np.nonzero(solid[chest_y])[0], np.nonzero(np.diff(np.nonzero(solid[chest_y])[0]) > 1)[0] + 1)
        torso = max(runs, key=len)
        chest = ((torso.min() + torso.max() + 1) / 2.0, float(chest_y))

        sockets = {"root": root, "neck": neck, "crown": crown, "chest": chest, "back": back,
                   "hand.near": middle(*NEAR_HAND[self.name]), "hand.far": middle(*FAR_HAND[self.name])}
        return sockets, (float(hx0), float(hy0), float(hx1 - hx0), float(sy1 - hy0))

    def body(self):
        """Everything below the seam, and SKIRT pixels up under the jaw."""
        under = rows() >= (self.seam - SKIRT)[None, :]
        return shrink(self.colour, self.alpha * under, self.skin * under)

    def bald(self):
        """The bald head: everything above the seam."""
        return shrink(self.colour, self.alpha * self.above)

    def tinted(self, ratio):
        """The base's colour with its skin multiplied towards a face's."""
        scale = 1.0 + self.skin[:, :, None] * (np.asarray(ratio)[None, None, :] - 1.0)
        return np.clip(self.colour * scale, 0, 255)


# --------------------------------------------------------------------------- #
# Faces
# --------------------------------------------------------------------------- #

def face_layer(base, rgb):
    """A face's layer on its base: (colour, alpha, its skin against the base's, notes)."""
    notes = []
    changed = (rgb != base.rgb).any(axis=2)
    if not changed.any():
        fail("a face is the base itself, unchanged")
    # Work in the box ChatGPT painted: a sheet's cell, or the whole picture for a single edit.
    x0, y0, x1, y1 = bbox(changed)
    x0, y0, x1, y1 = max(0, x0 - 4), max(0, y0 - 4), min(CANVAS[0], x1 + 4), min(CANVAS[1], y1 + 4)
    win = (slice(y0, y1), slice(x0, x1))
    colour, alpha = base.colour.copy(), base.alpha.copy()
    colour[win], alpha[win] = key_black(rgb[win])
    figure = alpha > 0.5
    above = base.above

    # The skin, from the lower face and the top of the neck: ChatGPT paints the face's own skin there,
    # but often leaves the shoulders the base's. Only pixels that are skin in both count, which keeps
    # out the eyes, the mouth, red and dark hair and grey beards.
    l, t, w, h = base.head
    c = rgb.astype(np.float64)
    red = np.maximum(c[:, :, 0], 1.0)
    green, blue = c[:, :, 1] / red, c[:, :, 2] / red
    skin_like = (green > 0.52) & (green < 0.86) & (blue > 0.3) & (blue < 0.8) & (c.max(axis=2) > 0.5 * base.rgb.max(axis=2))
    probe = (rows() > t + 0.3 * h) & (rows() < SEAM[base.name][1][1] + 25) & (base.skin > 0.9) & skin_like & figure
    if probe.sum() >= 300:
        # A beard or a fringe can pass for skin by colour, but there's less of it than of the
        # cheeks: take the commonest shade of skin (by its green and blue against its red) and only
        # the pixels close to it.
        chroma = np.stack([green[probe], blue[probe]], axis=1)
        bins = np.floor(chroma / CHROMA).astype(np.int64)
        keys, counts = np.unique(bins[:, 0] * 1000 + bins[:, 1], return_counts=True)
        mode = np.array([keys[np.argmax(counts)] // 1000, keys[np.argmax(counts)] % 1000]) * CHROMA + CHROMA / 2
        close = np.abs(chroma - mode).max(axis=1) <= CHROMA * 1.5
        picked = rgb[probe][close] / np.maximum(base.rgb[probe][close], 1)
        ratio = np.clip(np.median(picked, axis=0), 0.35, 1.35)
    else:
        ratio = np.ones(3)
        notes.append("too little bare skin to measure it by; left as the base's")

    # Below the seam: what isn't the tinted base (or is where the base has nothing), without the
    # outlines ChatGPT moved, grown back along its thin ends and with the gaps between strands
    # filled, so long as it hangs from the head.
    distance = np.abs(rgb - base.tinted(ratio)).max(axis=2)
    raw = ((distance > CHANGED) | (base.alpha < 0.5)) & ~above & figure & changed
    opened = ic.grow(erode(raw, THIN), THIN) & raw
    labels, _ = ic.components(opened)
    touching = np.unique(labels[ic.grow(above, 3) & opened])
    below = np.isin(labels, touching[touching > 0])
    for _ in range(REGROW):
        below = ic.grow(below, 1) & raw
    holes, count = ic.components(~below & ~above)
    if count:
        sizes = np.bincount(holes.ravel())
        edge = np.unique(np.concatenate([holes[y0], holes[y1 - 1], holes[:, x0], holes[:, x1 - 1]]))
        small = np.nonzero(sizes <= GAP)[0]
        small = small[(small > 0) & ~np.isin(small, edge)]
        below |= np.isin(holes, small)

    keep = above.astype(np.float64)
    keep[below] = 1.0
    # Nothing along the top and sides of what was painted: a sheet's grey lines can be left there.
    margin = np.zeros(keep.shape, bool)
    margin[y0:y0 + EDGE, x0:x1] = margin[y0:y1, x0:x0 + EDGE] = margin[y0:y1, x1 - EDGE:x1] = True
    keep[margin & changed] = 0.0
    # A sheet's cell cut the hair off along its bottom: fade it out up to there.
    bottom = y1 - 4
    if bottom < CANVAS[1] - 8 and below[bottom - 6:bottom].any():
        fade = np.clip((bottom - rows()) / FADE, 0.0, 1.0)
        keep = np.where(below, keep * fade, keep)
        notes.append("hair reaches the bottom of its cell; faded out there")
    # Only the head and what's near it: specks and leftovers elsewhere in the cell go.
    alpha = keep_near_main(alpha * keep)
    skin, hair, eyes, hair_tone, eye_tone = face_dyes(base, rgb, alpha > 0.5, ratio)
    if hair_tone is None:
        notes.append("no hair to dye")
    if eye_tone is None:
        notes.append("no eyes found to dye")
    return colour, alpha, ratio, notes, np.stack([skin, hair, eyes], axis=2), [hair_tone, eye_tone]


def blank_of(base):
    """The blank-faced base the new faces, hairs and helms are painted on: base-<x>-blank.png, an edit
    of the base with only its features taken off, keyed off its black, with the base's own sockets,
    head box and eyes (where the faces' eyes go). The base itself till there's one."""
    path = os.path.join(SOURCE, BLANK % base.name)
    if not os.path.exists(path):
        return base
    blank = copy.copy(base)
    blank.rgb = load(path)
    blank.colour, blank.alpha = key_black(blank.rgb)
    blank.skin = skin_weight(blank.rgb) * (blank.alpha > 0.99)
    return blank


def split_face(base, rgb, alpha, hair, ratio):
    """An old head-and-hair face's alpha split into (face, hair on the scalp): the hair (its mask)
    on the front of the head round the eyebrows, and on a man below the eyes (a beard), stays with
    the face, and the rest is the hair; and so is what the hair's mask missed: everything off the
    bald head's outline (a pointed ear goes with the hair till the face is painted again), and
    whatever above the eyebrows isn't the colour of the face's skin, lit or in shadow."""
    hx, hy, hw, hh = base.head
    across = np.arange(CANVAS[0])[None, :]
    eyes = np.nonzero(base.eyes.any(axis=1))[0]
    top, bottom = (eyes.min(), eyes.max()) if len(eyes) else (hy + 0.45 * hh, hy + 0.6 * hh)
    front = across >= hx + FRONT * hw
    facial = front & (rows() >= top - BROWS * hh) & (rows() <= bottom)
    if base.name == "base-male":
        facial = facial | (front & (rows() >= bottom + NOSE * hh))
    head = ic.grow(base.above & (base.alpha > 0.5), 2)
    skin_rgb = np.clip(np.asarray(base.skin_tone) * ratio, 0, 255)
    L = lab(rgb)
    skin = np.min(np.stack([distance(L, lab(np.clip(skin_rgb * k, 0, 255))) for k in (1.08, 1.0, 0.85, 0.72)]), axis=0) < MATERIAL_SAME
    stray = (alpha > 0) & ~facial & (~head | ((rows() < top - BROWS * hh) & ~skin))
    scalp = (hair | stray) & ~facial
    return alpha * ~scalp, alpha * scalp


def head_piece(base, rgb, hair):
    """A hair's or a helm's layer on its blank base: what it changed, in pieces that reach the head,
    grown back along their thin ends and with the gaps between strands filled: (colour, alpha, mask,
    tones). A hair is all hair to dye; a helm's Base and Trim are its two biggest materials."""
    colour, alpha = key_black(rgb)
    figure = alpha > 0.5
    distance = np.abs(rgb.astype(np.float64) - base.rgb).max(axis=2)
    raw = figure & ((distance > CHANGED) | (base.alpha < 0.5))
    opened = ic.grow(erode(raw, THIN), THIN) & raw
    labels, _ = ic.components(opened)
    touching = np.unique(labels[ic.grow(base.above & (base.alpha > 0.5), HEAD_REACH) & opened])
    kept = np.isin(labels, touching[touching > 0])
    for _ in range(REGROW):
        kept = ic.grow(kept, 1) & raw
    holes, count = ic.components(~kept)
    if count:
        sizes = np.bincount(holes.ravel())
        edge = np.unique(np.concatenate([holes[0], holes[-1], holes[:, 0], holes[:, -1]]))
        small = np.nonzero(sizes <= GAP)[0]
        kept |= np.isin(holes, small[(small > 0) & ~np.isin(small, edge)])
    alpha = keep_near_main(alpha * kept)
    there = alpha > 0.5
    nothing = np.zeros(alpha.shape)
    if hair:
        lit = there & ~outline(lab(rgb))
        hair_tone = tone(rgb, lit) if lit.sum() >= 100 else None
        return colour, alpha, np.stack([nothing, there.astype(np.float64) if hair_tone else nothing, nothing], axis=2), [hair_tone, None]
    (base_mask, trim_mask), tones = armor_dyes(rgb, alpha, nothing)
    return colour, alpha, np.stack([nothing, base_mask, trim_mask], axis=2), tones


def part_ids(slot):
    folder = os.path.join(SOURCE, FOLDERS[slot])
    return sorted(os.path.basename(p)[:-4] for p in glob.glob(os.path.join(folder, slot + "-*.png")))


def armor_layer(base, rgb):
    """An armor's layer on its base: the figure from the jaw down (and SKIRT up under it), keyed off
    the black, and its skin: where it's still the base's skin, as warm as skin and close to the base's
    own pixel there, which keeps a tan coat or leather from being tinted along with the arms."""
    colour, alpha = key_black(rgb)
    under = rows() >= (base.seam - SKIRT)[None, :]
    distance = np.abs(rgb - base.rgb).max(axis=2)
    close = np.clip(1.0 - (distance - SKIN_SAME) / SKIN_FADE, 0.0, 1.0)
    skin = skin_weight(rgb) * close * (base.skin > 0.5) * under
    alpha = keep_near_main(alpha * under)
    (base_mask, trim_mask), tones = armor_dyes(rgb, alpha, skin)
    return colour, alpha, np.stack([skin, base_mask, trim_mask], axis=2), tones


def fist_mask(base, box):
    """A base's fist in its box (FISTS), as alpha: the biggest piece of the figure there, opened so
    the hair-thin edge of an outline running on beside it goes."""
    c0, c1, r0, r1 = box
    inside = np.zeros(base.alpha.shape, bool)
    inside[r0:r1, c0:c1] = True
    labels, count = ic.components((base.alpha > 0.5) & inside)
    if not count:
        fail("%s has no fist in %s" % (base.name, (box,)))
    sizes = np.bincount(labels.ravel())
    sizes[0] = 0
    keep = labels == int(np.argmax(sizes))
    keep &= ic.grow(erode(keep, 2), 2)
    return base.alpha * keep


#: The simple fist drawn over what a hand holds: how much its finger creases are closed and its
#: outline smoothed, how far it reaches past the painted fist (so none of that shows round it), how
#: thick its outline is, and the width of its shadow along the lower right and its light along the
#: upper left. Its shades, as a share of the hand's own colour: lit, shadow, highlight, outline.
FIST_CLOSE = 5
FIST_SMOOTH = 7
FIST_COVER = 2
FIST_LINE = 3
FIST_SHADOW = 16
FIST_LIGHT = 6
FIST_SHADES = (1.0, 0.76, 1.12, 0.22)
#: Its top, the wrist, has no outline and fades over FIST_FADE rows into the arm it covers; and
#: FINGERS short lines (FINGER_LENGTH long) part the fingers along its bottom, at these shares of its width.
FIST_FADE = 10
FINGERS = (0.34, 0.52, 0.70)
FINGER_LENGTH = 28
#: The shades are kept in the mask's colour as a share of this, so a highlight brighter than the hand fits.
FIST_UNIT = 200.0


def simple_fist(base, box):
    """The fist drawn over what a hand holds, as plain as the game's other art is detailed: the
    painted fist's shape with its creases closed, smoothed round and grown a little to cover it, in
    one flat shade with a hard shadow along the lower right, a thin light along the upper left and a
    clean outline, all lit from the upper left. Its colour is grey, the shade as a share of
    FIST_UNIT; the game multiplies it by the colour of the hand it covers, skin or glove. Returns
    (colour, alpha) on the canvas."""
    painted = fist_mask(base, box) > 0.5
    shape = erode(ic.grow(painted, FIST_CLOSE), FIST_CLOSE)
    soft = ic.box_sum(shape.astype(np.float64), FIST_SMOOTH) / float((2 * FIST_SMOOTH + 1) ** 2)
    shape = ic.grow(soft > 0.5, FIST_COVER) | painted

    def inside_after(dx, dy):
        moved = np.zeros_like(shape)
        h, w = shape.shape
        moved[max(0, -dy):h - max(0, dy), max(0, -dx):w - max(0, dx)] = shape[max(0, dy):h - max(0, -dy), max(0, dx):w - max(0, -dx)]
        return moved

    lit, dark, light, ink = FIST_SHADES
    ys, xs = np.nonzero(shape)
    top, left, right = ys.min(), xs.min(), xs.max()
    shade = np.where(shape, lit, 0.0)
    shade[shape & ~inside_after(FIST_SHADOW, FIST_SHADOW)] = dark
    shade[shape & ~inside_after(-FIST_LIGHT, -FIST_LIGHT) & (shade == lit)] = light
    outline = shape & ~erode(shape, FIST_LINE)
    outline[:top + FIST_LINE + 1] = False
    shade[outline] = ink
    # The fingers, parted by short lines up from the bottom edge.
    for share in FINGERS:
        x = int(round(left + share * (right - left)))
        column = np.nonzero(shape[:, x])[0]
        bottom = column.max()
        shade[bottom - FINGER_LENGTH:bottom + 1, x:x + FIST_LINE - 1][shape[bottom - FINGER_LENGTH:bottom + 1, x:x + FIST_LINE - 1]] = ink
    grey = np.clip(shade / (255.0 / FIST_UNIT), 0.0, 1.0) * 255.0
    colour = np.repeat(grey[:, :, None], 3, axis=2)
    alpha = shape.astype(np.float64)
    fade = np.clip((np.arange(shape.shape[0]) - top) / float(FIST_FADE), 0.0, 1.0)
    return colour, alpha * fade[:, None]


#: The holding hands painted in ChatGPT (Story/13 §3.3), one picture per base: hand-male.png and
#: hand-female.png, each an edit of its base with plainer fists. Till one's there, simple_fist stands in.
HANDS = os.path.join(SOURCE, "hands")


def painted_fist(base, box, rgb):
    """A base's holding hand as ChatGPT painted it (§3.3): the fist in its box on the painted
    picture, the biggest piece of the figure there, opened; its shading and lines kept as a share of
    the hand's own colour (the brighter half of the fist, which is how the game measures the hand it
    covers), in FIST_UNITs, so the game can colour it as skin or glove; fading into the arm at the
    wrist. Returns (colour, alpha) as simple_fist does, and how much of the base's own fist it covers."""
    colour, alpha = key_black(rgb)
    c0, c1, r0, r1 = box
    inside = np.zeros(alpha.shape, bool)
    inside[r0:r1, c0:c1] = True
    labels, count = ic.components((alpha > 0.5) & inside)
    if not count:
        fail("%s has no fist in %s" % (os.path.join(HANDS, "hand-" + base.name.split("-", 1)[1] + ".png"), (box,)))
    sizes = np.bincount(labels.ravel())
    sizes[0] = 0
    keep = labels == int(np.argmax(sizes))
    keep &= ic.grow(erode(keep, 2), 2)
    luma = colour @ np.array([0.299, 0.587, 0.114])
    lit = luma[keep]
    reference = max(1.0, float(lit[lit >= lit.mean()].mean()))
    grey = np.clip(luma / reference * FIST_UNIT, 0.0, 255.0) * keep
    top = np.nonzero(keep.any(axis=1))[0].min()
    fade = np.clip((np.arange(alpha.shape[0]) - top) / float(FIST_FADE), 0.0, 1.0)
    own = fist_mask(base, box) > 0.5
    return np.repeat(grey[:, :, None], 3, axis=2), alpha * keep * fade[:, None], float((own & keep).sum()) / max(1, own.sum())


def changed_pieces(base, rgb, where):
    """What a part painted, within `where`: the pixels on the figure that differ from the base,
    opened so the base's outlines ChatGPT moved a pixel or two stay out, then grown back along
    their thin ends. Returns (colour, alpha, the pieces)."""
    colour, alpha = key_black(rgb)
    distance = np.abs(rgb.astype(np.float64) - base.rgb).max(axis=2)
    raw = (alpha > 0.5) & (distance > CHANGED) & where
    kept = ic.grow(erode(raw, THIN), THIN) & raw
    for _ in range(REGROW):
        kept = ic.grow(kept, 1) & raw
    return colour, alpha, kept


def cape_layer(base, rgb):
    """A cape's layer on its base: what it painted in the black behind and round the figure (a cape
    is never over the body), grown UNDER pixels in under the body's edge in its own colours, with
    its Base and Trim to dye: (colour, alpha, mask, tones)."""
    behind = base.alpha < 0.5
    colour, alpha, kept = changed_pieces(base, rgb, behind)
    alpha = keep_near_main(alpha * kept)
    there = alpha > 0.5
    under = ic.grow(there, UNDER) & ~behind & ~there
    if under.any():
        beside = ic.local_mean(colour, there.astype(np.float64), (UNDER + 1, UNDER * 3))
        fill = under & ~np.isnan(beside[:, :, 0])
        colour[fill] = beside[fill]
        alpha[fill] = 1.0
    nothing = np.zeros(alpha.shape)
    (base_mask, trim_mask), tones = armor_dyes(rgb, alpha, nothing)
    return colour, alpha, np.stack([nothing, base_mask, trim_mask], axis=2), tones


def hand_layer(base, rgb):
    """A weapon's or an off-hand item's layer on its base: the changed pieces that reach a hand,
    without the base's own fingers where ChatGPT redrew them (skin painted where the base had skin),
    so a gloved hand never gets a bare fist drawn over it. Nothing on it is dyed: (colour, alpha,
    None, None)."""
    fingers = (skin_weight(rgb) > 0.5) & (base.skin > 0.5)
    colour, alpha, kept = changed_pieces(base, rgb, ~fingers)
    hands = np.zeros(alpha.shape, bool)
    for socket in ("hand.near", "hand.far"):
        x, y = base.sockets[socket]
        hands[max(0, int(y) - HAND_REACH):int(y) + HAND_REACH, max(0, int(x) - HAND_REACH):int(x) + HAND_REACH] = True
    labels, count = ic.components(kept)
    reach = np.unique(labels[hands & kept])
    kept &= np.isin(labels, reach[reach > 0])
    alpha = alpha * kept
    if not (alpha > 0.5).any():
        fail("a weapon or off-hand item has nothing near either hand: did ChatGPT put it in the hand?")
    return colour, alpha, None, None


# --------------------------------------------------------------------------- #
# Dye masks: where a part can be recoloured, and the colour it was painted there
# --------------------------------------------------------------------------- #

def lab(rgb):
    """sRGB (0 to 255) to CIE L*a*b* under D65."""
    c = np.asarray(rgb, np.float64) / 255.0
    linear = np.where(c <= 0.04045, c / 12.92, ((c + 0.055) / 1.055) ** 2.4)
    xyz = linear @ np.array([[0.4124, 0.3576, 0.1805], [0.2126, 0.7152, 0.0722], [0.0193, 0.1192, 0.9505]]).T
    xyz /= np.array([0.95047, 1.0, 1.08883])
    f = np.where(xyz > 0.008856, np.cbrt(xyz), 7.787 * xyz + 16.0 / 116.0)
    return np.stack([116.0 * f[..., 1] - 16.0, 500.0 * (f[..., 0] - f[..., 1]), 200.0 * (f[..., 1] - f[..., 2])], axis=-1)


def luma(rgb):
    """Brightness as the game's dye shades by it (Characters/Parts/RigComposer.cs's Luma), 0 to 1."""
    return (np.asarray(rgb, np.float64) / 255.0) @ np.array([0.299, 0.587, 0.114])


def distance(lab_px, colour):
    """How far pixels (L*a*b*) are from a colour, lightness counting MATERIAL_LIGHTNESS as much."""
    d = lab_px - np.asarray(colour)
    return np.sqrt((d[..., 0] * MATERIAL_LIGHTNESS) ** 2 + d[..., 1] ** 2 + d[..., 2] ** 2)


def outline(lab_px):
    """The drawn lines and the near-black: never dyed, so a recoloured part keeps its linework."""
    chroma = np.hypot(lab_px[..., 1], lab_px[..., 2])
    return (lab_px[..., 0] < LINE_L) | ((lab_px[..., 0] < DIM_L) & (chroma < DIM_C))


def regions(open_):
    """The pieces a picture's outlines split it into: labels (0 off them) and how many. A gap in an
    outline would join a shirt to the trousers it's drawn over, so they're found with the lines
    grown by GAP_SEAL, which closes the gap, and then grown back out to them."""
    labels, count = ic.components(erode(open_, GAP_SEAL))
    for _ in range(GAP_SEAL * 2 + 1):
        near = np.zeros_like(labels)
        near[1:] = np.maximum(near[1:], labels[:-1])
        near[:-1] = np.maximum(near[:-1], labels[1:])
        near[:, 1:] = np.maximum(near[:, 1:], labels[:, :-1])
        near[:, :-1] = np.maximum(near[:, :-1], labels[:, 1:])
        fill = open_ & (labels == 0) & (near > 0)
        if not fill.any():
            break
        labels[fill] = near[fill]
    return labels, count


def materials(rgb, where):
    """What an outfit's made of, biggest first: a mask for each. The outlines split it into regions,
    the pieces of each garment, whose shading has no lines in it; regions of one colour are one
    material (both sleeves, both legs), and the specks a texture leaves go with the material they're
    nearest, if they're near one."""
    L = lab(rgb)
    labels, count = regions(where & ~outline(L))
    if count == 0:
        return []
    sizes = np.bincount(labels.ravel(), minlength=count + 1)
    sums = np.stack([np.bincount(labels.ravel(), weights=L[..., k].ravel(), minlength=count + 1) for k in range(3)], axis=1)
    colours = sums / np.maximum(sizes, 1)[:, None]
    big = [r for r in range(1, count + 1) if sizes[r] >= REGION]
    if not big:
        return []
    # Centroid linkage: the two nearest materials join, their colour the mean of both by area, till
    # the nearest are MATERIAL_SAME apart. Joining only the nearest keeps a run of in-between colours
    # from chaining a coat to its trousers.
    centres = colours[big].copy()
    area = sizes[big].astype(np.float64)
    members = [[r] for r in big]
    gaps = distance(centres[:, None, :], centres[None, :, :])
    np.fill_diagonal(gaps, np.inf)
    while len(big) > 1:
        i, j = np.unravel_index(np.argmin(gaps), gaps.shape)
        if gaps[i, j] >= MATERIAL_SAME:
            break
        i, j = min(i, j), max(i, j)
        centres[i] = (centres[i] * area[i] + centres[j] * area[j]) / (area[i] + area[j])
        area[i] += area[j]
        members[i] += members[j]
        members[j] = []
        area[j] = 0.0
        gaps[j, :] = gaps[:, j] = np.inf
        row = distance(centres[i][None, :], centres)
        row[[k for k in range(len(members)) if not members[k]] + [i]] = np.inf
        gaps[i, :] = gaps[:, i] = row
    owner = np.zeros(count + 1, np.int64)
    roots = []
    for k, rs in enumerate(members):
        if rs:
            roots.append(rs[0])
            owner[rs] = rs[0]
    area = {g: int(sizes[owner == g].sum()) for g in roots}
    centre = {g: centres[[k for k, rs in enumerate(members) if rs and rs[0] == g][0]] for g in roots}
    small = [r for r in range(1, count + 1) if 0 < sizes[r] < REGION]
    if small:
        cs = np.array([centre[g] for g in roots])
        d = distance(colours[small][:, None, :], cs[None, :, :])
        nearest = np.argmin(d, axis=1)
        for k, r in enumerate(small):
            if d[k, nearest[k]] < MATERIAL_NEAR:
                owner[r] = roots[nearest[k]]
    return [owner[labels] == g for g in sorted(roots, key=lambda g: -area[g])]


def tone(rgb, mask):
    """The colour a dye channel was painted: the mean of its pixels, which the game maps onto the
    swatch, shading the rest by how much brighter or darker they are (RigComposer's Dye)."""
    return [round(float(v), 1) for v in rgb[mask].mean(axis=0)]


def armor_dyes(rgb, alpha, skin):
    """An outfit's Base and Trim: its biggest material and its next, each a mask and the tone it
    was painted. A material too small or too dark to shade by isn't one; (None, None) for none."""
    where = (alpha > 0.9) & (skin < 0.3)
    found = materials(rgb, where)
    total = sum(int(m.sum()) for m in found) or 1
    dyes = [m for m in found if m.sum() >= LEAST_MATERIAL * total and luma(rgb[m].mean(axis=0)) >= TONE_FLOOR][:2]
    masks = [d.astype(np.float64) for d in dyes] + [np.zeros(alpha.shape)] * (2 - len(dyes))
    tones = [tone(rgb, d) for d in dyes] + [None] * (2 - len(dyes))
    return masks, tones


def face_dyes(base, rgb, figure, ratio):
    """A face's skin, hair and eyes: (skin mask, hair mask, eye mask, hair tone, eye tone).
    The skin and the hair are told apart by colour: the skin's is the base's cheeks times the face's
    (lit, in shadow and in deep shadow), the hair's what's painted beyond the bald head that isn't
    skin (lit and in shadow); each pixel that isn't the bald head as it was is whichever it's nearer.
    The eyes are the coloured part beside the whites, near where the base's are."""
    L = lab(rgb)
    lines = outline(L)
    skin_rgb = np.clip(np.asarray(base.skin_tone) * ratio, 0, 255)
    # Skin lit, shaded and under a highlight; "strict" leaves out the deepest shadow, which dark hair
    # can pass for.
    shades = (1.08, 1.0, 0.85, 0.72, 0.6)
    near_skin = np.stack([distance(L, lab(np.clip(skin_rgb * k, 0, 255))) for k in shades])
    to_skin, strict = near_skin.min(axis=0), near_skin[:4].min(axis=0)

    # The outlines split the head into pieces; each is skin or hair by its colour as a whole, which
    # a single pixel of blonde hair or of shadowed cheek can't be told by. A piece the bald head
    # already had, unchanged, is skin; the hair's colour is that of the pieces painted beyond the
    # bald head, by area, which an ear or two hardly moves; and every changed piece is whichever of
    # the skin and the hair it's nearer.
    # A face's lines are lighter than an outfit's (blonde hair is drawn in brown), so here they're
    # what's FACE_EDGE darker than round it as well as what's dark outright.
    local = ic.box_sum(L[..., 0], FACE_EDGE_REACH) / float((2 * FACE_EDGE_REACH + 1) ** 2)
    labels, count = regions(figure & ~lines & ~(L[..., 0] < local - FACE_EDGE))
    hair = np.zeros(figure.shape, bool)
    hair_tone = None
    if count:
        changed = np.abs(rgb - base.tinted(ratio)).max(axis=2) > CHANGED
        flat_labels = labels.ravel()
        sizes = np.bincount(flat_labels, minlength=count + 1).astype(np.float64)

        def mean(values):
            return np.bincount(flat_labels, weights=values.ravel(), minlength=count + 1) / np.maximum(sizes, 1)

        out = mean((base.alpha < 0.5).astype(np.float64))
        moved = mean(changed.astype(np.float64))
        colours = np.stack([mean(L[..., k]) for k in range(3)], axis=1)
        piece_skin = np.min(np.stack([distance(colours, lab(np.clip(skin_rgb * k, 0, 255))) for k in shades]), axis=0)
        seeds = (sizes >= HAIR_SPECK) & (out > 0.6)
        seeds[0] = False
        if sizes[seeds].sum() >= 100:
            lit = (colours[seeds] * sizes[seeds][:, None]).sum(axis=0) / sizes[seeds].sum()
            piece_hair = np.min(np.stack([distance(colours, lit + np.array([dl, 0.0, 0.0])) for dl in (-18.0, 0.0, 12.0)]), axis=0)
            kept = (sizes >= HAIR_SPECK) & ((moved > 0.35) | (out > 0.3)) & (piece_hair < piece_skin) & (piece_hair < HAIR_FAR)
            kept[0] = False
            hair = kept[labels] & ~ic.grow(base.eyes, 2)
            # Black hair is mostly darker than a line: the thick dark that touches the hair or is
            # beyond the bald head goes with it (an outline is too thin to be thick).
            dark = figure & lines & (changed | (base.alpha < 0.5)) & ~ic.grow(base.eyes, 2)
            thick = ic.grow(erode(dark, THICK_HAIR), THICK_HAIR) & dark
            pieces, n = ic.components(thick)
            if n:
                reach = np.zeros(n + 1, bool)
                reach[np.unique(pieces[ic.grow(hair, 1) | (base.alpha < 0.3)])] = True
                reach[0] = False
                hair |= reach[pieces]
            hair = ic.grow(hair, 1) & figure & (hair | lines)
            if (hair & ~lines).sum() >= 100:
                hair_tone = tone(rgb, hair)
            else:
                hair[:] = False

    zone = base.eyes & figure
    whites = zone & (L[..., 0] > WHITE_L) & (np.hypot(L[..., 1], L[..., 2]) < WHITE_C)
    iris = zone & ic.grow(whites, IRIS_REACH) & ~whites & ~lines & ~hair & (to_skin > IRIS_SKIN)
    if iris.sum() < 20 or whites.sum() < 6:
        # Eyes narrowed to slits show no whites: what's plainly coloured and not skin, where they are.
        iris = zone & ~lines & ~hair & (to_skin > IRIS_SKIN * 1.5) & (np.hypot(L[..., 1], L[..., 2]) > 10.0) & (L[..., 0] > 20.0)
    eye_tone = tone(rgb, iris) if iris.sum() >= 20 else None
    if eye_tone is None:
        iris[:] = False

    skin = ic.smoothstep(MATERIAL_SAME * 2.0, MATERIAL_SAME, to_skin) * figure * ~hair * ~iris
    return skin, hair.astype(np.float64), iris.astype(np.float64), hair_tone, eye_tone


def keep_near_main(alpha):
    """Alpha with only the biggest piece and what's near it or big itself: specks and leftovers (a
    stray line from a sheet) go."""
    alpha = alpha.copy()
    there = alpha > 0.02
    labels, count = ic.components(there)
    if count > 1:
        sizes = np.bincount(labels.ravel())
        sizes[0] = 0
        main = int(np.argmax(sizes))
        near = np.unique(labels[ic.grow(labels == main, NEAR)])
        kept = np.union1d(near[near > 0], np.nonzero(sizes >= sizes[main] * 0.02)[0])
        alpha[there & ~np.isin(labels, kept)] = 0.0
    return alpha


# --------------------------------------------------------------------------- #
# Atlases
# --------------------------------------------------------------------------- #

def pack(images):
    """Shelf-packs {name: RGBA} into pages no bigger than PAGE: tallest first, then by name, so the
    same images always land in the same places. Returns the pages and {name: (page, x, y, w, h)}."""
    order = sorted(images, key=lambda k: (-images[k].shape[0], -images[k].shape[1], k))
    pages, places = [], {}
    x = y = shelf = 0
    used = []
    for name in order:
        h, w = images[name].shape[:2]
        if w + PAD * 2 > PAGE or h + PAD * 2 > PAGE:
            fail("%s is %dx%d, bigger than an atlas page" % (name, w, h))
        if not used or x + w + PAD * 2 > PAGE:
            x, y, shelf = 0, y + shelf, 0
        if not used or y + h + PAD * 2 > PAGE:
            used.append([])
            x = y = shelf = 0
        used[-1].append((name, x + PAD, y + PAD))
        places[name] = (len(used) - 1, x + PAD, y + PAD, w, h)
        x += w + PAD * 2
        shelf = max(shelf, h + PAD * 2)
    for content in used:
        width = max(px + images[n].shape[1] for n, px, _ in content) + PAD
        height = max(py + images[n].shape[0] for n, _, py in content) + PAD
        page = np.zeros((height, width, 4), np.uint8)
        for n, px, py in content:
            img = images[n]
            page[py:py + img.shape[0], px:px + img.shape[1]] = img
        pages.append(page)
    return pages, places


# --------------------------------------------------------------------------- #
# Putting a character together (the reference for Characters/Parts/RigComposer.cs)
# --------------------------------------------------------------------------- #

#: Back to front. "head" is the base's bald head, drawn only when there's no face.
ORDER = ("cape", "body", "armor", "head", "face", "helm", "weapon", "offhand")


def head_scale(morph):
    """How big the head is drawn on a morphed body: a short body keeps more of its head (a child's
    head is big for them), a tall or broad one keeps it as it is."""
    return min(1.0, morph[0]) ** 0.5


def fit(slot, source, target):
    """How much to scale a part painted on one base's sockets and head box (source) to fit another's
    (target): Characters/Parts/PartCatalog.cs's Fit. Each is {"sockets": {...}, "head": [l, t, w, h]}."""
    if source is target:
        return 1.0, 1.0
    s, t = source["sockets"], target["sockets"]
    if slot in ("head", "face", "helm"):
        k = target["head"][3] / source["head"][3]
        return k, k
    if slot == "cape":
        return ((t["chest"][0] - t["back"][0]) / (s["chest"][0] - s["back"][0]),
                (t["root"][1] - t["back"][1]) / (s["root"][1] - s["back"][1]))
    k = (t["root"][1] - t["crown"][1]) / (s["root"][1] - s["crown"][1])
    return k, k


def transform(slot, morph, sockets):
    """Where a slot's socket goes on the morphed body and how its layer scales: (socket, (sx, sy))."""
    mh, mw = morph
    rx, ry = sockets["root"]
    sx, sy = sockets[SLOT_SOCKET[slot]]
    at = (rx + (sx - rx) * mw, ry + (sy - ry) * mh)
    if slot in ("head", "face", "helm"):
        s = head_scale(morph)
        return at, (s, s)
    if slot in ("weapon", "offhand"):
        return at, (mh, mh)
    return at, (mw, mh)


def dye(rgb, painted, colour):
    """Pixels (0 to 1) recoloured to a swatch, shaded as they were against the tone their channel
    was painted: Characters/Parts/RigComposer.cs's Dye."""
    y = rgb @ np.array([0.299, 0.587, 0.114])
    ref = max(float(np.asarray(painted) @ np.array([0.299, 0.587, 0.114])), 0.02)
    colour = np.asarray(colour, np.float64)
    dark = colour[None, None, :] * (y / ref)[..., None]
    light = colour[None, None, :] + (1.0 - colour[None, None, :]) * ((y - ref) / max(1e-3, 1.0 - ref))[..., None]
    return np.clip(np.where((y <= ref)[..., None], dark, light), 0.0, 1.0)


#: The piece each slot moves with, back to front: Characters/Parts/Sockets.cs's BoneOf.
BONES = ("back", "body", "head", "near", "far")
BONE_OF = {"cape": "back", "body": "body", "armor": "body", "head": "head", "face": "head", "helm": "head",
           "weapon": "near", "offhand": "far"}


def compose(layers, morph, sockets, skin, fits=None, dyes=None, fists=None):
    """A recipe's layers ({slot: (Layer, pivot)}) put together at their sockets: (premultiplied
    float RGBA, where the root is on it). `fits` scales a slot's part further ({slot: (fx, fy)}): one
    painted on the other base, fitted to this one (fit). `dyes` recolours a slot's two dye channels
    ({slot: [(swatch, painted tone) or None, ...]}, both 0 to 255). `fists` ({"near": (Layer, pivot),
    "far": ...}, the base's fist masks) draws what a hand holds under its fist, as the game does."""
    placed = []
    for slot in ORDER:
        if slot not in layers:
            continue
        layer, pivot = layers[slot]
        at, (sx, sy) = transform(slot, morph, sockets)
        fx, fy = (fits or {}).get(slot, (1.0, 1.0))
        sx, sy = sx * fx, sy * fy
        w, h = layer.size
        nw, nh = max(1, int(round(w * sx))), max(1, int(round(h * sy)))
        left, top = int(round(at[0] - pivot[0] * nw / w)), int(round(at[1] - pivot[1] * nh / h))
        rgba = layer.rgba.astype(np.float64) / 255.0
        premultiplied = np.concatenate([rgba[:, :, :3] * rgba[:, :, 3:], rgba[:, :, 3:]], axis=2)
        if layer.mask is not None and skin is not None and slot != "face":
            m = layer.mask[:, :, :1].astype(np.float64) / 255.0
            premultiplied[:, :, :3] *= 1.0 + m * (np.asarray(skin)[None, None, :] - 1.0)
            premultiplied[:, :, :3] = np.minimum(premultiplied[:, :, :3], premultiplied[:, :, 3:])
        for k, pair in enumerate((dyes or {}).get(slot, [])):
            if pair is None or layer.mask is None:
                continue
            swatch, painted = pair
            m = layer.mask[:, :, 1 + k:2 + k].astype(np.float64) / 255.0
            straight = premultiplied[:, :, :3] / np.maximum(premultiplied[:, :, 3:], 1e-6)
            dyed = dye(straight, np.asarray(painted) / 255.0, np.asarray(swatch) / 255.0)
            premultiplied[:, :, :3] = (straight + m * (dyed - straight)) * premultiplied[:, :, 3:]
        if (nw, nh) != (w, h):
            premultiplied = ic.resample(premultiplied, (0, 0, w, h), (nw, nh))
        placed.append((premultiplied, left, top, BONE_OF[slot]))
    x0 = min(l for _, l, _, _ in placed)
    y0 = min(t for _, _, t, _ in placed)
    x1 = max(l + p.shape[1] for p, l, _, _ in placed)
    y1 = max(t + p.shape[0] for p, _, t, _ in placed)
    pieces = {}
    for p, l, t, bone in placed:
        piece = pieces.setdefault(bone, np.zeros((y1 - y0, x1 - x0, 4)))
        region = piece[t - y0:t - y0 + p.shape[0], l - x0:l - x0 + p.shape[1]]
        region[:] = p + region * (1.0 - p[:, :, 3:])
    # What a hand holds goes under its fist: the fist, as the body and armor draw it, is lifted off
    # the body onto the hand's piece (RigComposer.cs's LiftFist).
    for hand, (layer, pivot) in (fists or {}).items():
        if hand not in pieces or "body" not in pieces:
            continue
        at, (sx, sy) = transform("body", morph, sockets)
        w, h = layer.size
        nw, nh = max(1, int(round(w * sx))), max(1, int(round(h * sy)))
        left, top = int(round(at[0] - pivot[0] * nw / w)) - x0, int(round(at[1] - pivot[1] * nh / h)) - y0
        a = layer.rgba[:, :, 3:].astype(np.float64) / 255.0
        grey = layer.rgba[:, :, :1].astype(np.float64) / FIST_UNIT
        if (nw, nh) != (w, h):
            both = ic.resample(np.concatenate([grey * a, grey * a, grey * a, a], axis=2), (0, 0, w, h), (nw, nh))
            a = both[:, :, 3:]
            grey = both[:, :, :1] / np.maximum(a, 1e-6)
        if left < 0 or top < 0 or left + nw > x1 - x0 or top + nh > y1 - y0:
            continue
        body = pieces["body"][top:top + nh, left:left + nw]
        held = pieces[hand][top:top + nh, left:left + nw]
        # The plain fist, coloured as the hand it covers: the brighter half of what's under it.
        there = (a[:, :, 0] > 0.5) & (body[:, :, 3] > 0.5)
        if not there.any():
            continue
        under = body[there][:, :3] / body[there][:, 3:]
        luma = under @ np.array([0.299, 0.587, 0.114])
        colour = under[luma >= luma.mean()].mean(axis=0)
        lifted = np.concatenate([np.clip(colour[None, None, :] * grey, 0.0, 1.0) * a, a], axis=2)
        body *= 1.0 - a
        held[:] = lifted + held * (1.0 - a)
    canvas = np.zeros((y1 - y0, x1 - x0, 4))
    for bone in BONES:
        if bone in pieces:
            canvas = pieces[bone] + canvas * (1.0 - pieces[bone][:, :, 3:])
    root = sockets["root"]
    return canvas, (root[0] - x0, root[1] - y0)


def flat(premultiplied, back):
    """A premultiplied picture over a flat colour, as 8-bit RGB."""
    return np.rint(premultiplied[:, :, :3] * 255.0 + np.asarray(back)[None, None, :] * (1.0 - premultiplied[:, :, 3:])).astype(np.uint8)


# --------------------------------------------------------------------------- #
# Previews
# --------------------------------------------------------------------------- #

def grid(tiles, columns, back):
    th = max(t.shape[0] for t in tiles)
    tw = max(t.shape[1] for t in tiles)
    count = (len(tiles) + columns - 1) // columns
    out = np.full((count * (th + 4) + 4, columns * (tw + 4) + 4, 3), back, np.uint8)
    for k, t in enumerate(tiles):
        r, c = divmod(k, columns)
        y, x = 4 + r * (th + 4) + (th - t.shape[0]), 4 + c * (tw + 4) + (tw - t.shape[1]) // 2
        out[y:y + t.shape[0], x:x + t.shape[1]] = t
    return out


def save_rgb(path, rgb):
    ic.write_png(path, np.dstack([rgb, np.full(rgb.shape[:2], 255, np.uint8)]))


def mark(img, x, y, colour, r=3):
    x, y = int(round(x)), int(round(y))
    img[max(0, y - r):y + r + 1, max(0, x - r):x + r + 1] = colour


def socket_preview(base, body, bald):
    """The base at PART_SCALE with its seam, sockets and head box drawn on it."""
    s = PART_SCALE
    layers = {"body": (body, None), "head": (bald, None)}
    w, h = int(round(CANVAS[0] * s)), int(round(CANVAS[1] * s))
    canvas = np.zeros((h, w, 4))
    for layer, _ in layers.values():
        rgba = layer.rgba.astype(np.float64) / 255.0
        p = np.concatenate([rgba[:, :, :3] * rgba[:, :, 3:], rgba[:, :, 3:]], axis=2)
        x, y = layer.at
        region = canvas[y:y + p.shape[0], x:x + p.shape[1]]
        region[:] = p + region * (1.0 - p[:, :, 3:])
    img = flat(canvas, (40, 44, 56))
    for x in range(w):
        y = int(round(base.seam[min(CANVAS[0] - 1, int(x / s))] * s))
        img[y, x] = (255, 80, 80)
    hx, hy, hw, hh = (v * s for v in base.head)
    img[int(hy), int(hx):int(hx + hw)] = img[int(hy + hh), int(hx):int(hx + hw)] = (90, 200, 255)
    img[int(hy):int(hy + hh), int(hx)] = img[int(hy):int(hy + hh), int(hx + hw)] = (90, 200, 255)
    colours = {"root": (255, 230, 60), "neck": (255, 80, 80), "crown": (90, 200, 255), "chest": (120, 255, 120),
               "back": (255, 140, 255), "hand.near": (255, 160, 40), "hand.far": (200, 120, 40)}
    for name, (x, y) in base.sockets.items():
        mark(img, x * s, y * s, colours[name])
    return img


# --------------------------------------------------------------------------- #
# Writing it all
# --------------------------------------------------------------------------- #

def scaled(point):
    return [round(point[0] * PART_SCALE, 2), round(point[1] * PART_SCALE, 2)]


def as_vector(pid, colour, alpha):
    """A layer's colour and alpha redrawn as vector art at the same size: tools/vectorize.py's shapes
    for it, from VECTOR_CACHE if they're there for this same picture."""
    import vectorize
    drawn = vectorize.render(vectorize.cached(colour, alpha, os.path.join(VECTOR_CACHE, pid + ".svg")))
    return vectorize.straight(drawn)


def build(check, vector=VECTOR):
    """Imports every part, or says what's out of date with `check`. `vector` redraws them as vector
    art: True for all of them, or a set of part ids (a base's id for its body and bald head) for just
    those, to see how they'd look."""
    text = story()
    edits = basis(text)
    book = recipes(text)
    items, own = gear(text, edits), own_gear(text, edits, book)
    known = ic.game_data()

    bases = {name: Base(name) for name in BASES}
    blanks = {name: blank_of(b) for name, b in bases.items()}
    layers = {}         # part id -> (slot, base, Layer, extra fields)
    def drawn(pid, colour, alpha):
        return as_vector(pid, colour, alpha) if vector is True or (vector and pid in vector) else (colour, alpha)

    for name, b in bases.items():
        under = rows() >= (b.seam - SKIRT)[None, :]
        colour, alpha = drawn(name, b.colour, b.alpha)
        layers[name + ".body"] = ("body", name, shrink(colour, alpha * under, b.skin * under), {})
        # Everyone's head is the mannequin's: the blank base's once there is one, tinted to their skin.
        k = blanks[name]
        if k is not b:
            colour, alpha = drawn(name + "-blank", k.colour, k.alpha)
        layers[name + ".head"] = ("head", name, shrink(colour, alpha * k.above, k.skin * k.above), {})
        # Its holding hands: what a hand holds is drawn under the fist that holds it, the one painted
        # for it (painted_fist) or, till there is one, a plain one of the importer's (simple_fist).
        painted = os.path.join(HANDS, "hand-%s.png" % name.split("-", 1)[1])
        hands = load(painted) if os.path.exists(painted) else None
        for hand, box in zip(("near", "far"), FISTS[name]):
            if hands is None:
                colour, alpha = simple_fist(b, box)
            else:
                colour, alpha, covers = painted_fist(b, box, hands)
                if covers < 0.97:
                    print("import_parts: the painted %s fist on %s covers only %d%% of the base's own, which shows round it: "
                          "ask ChatGPT for it a touch bigger" % (hand, name, round(covers * 100)))
            layers["%s.hand.%s" % (name, hand)] = ("body", name, shrink(colour, alpha), {})

    report, previews = [], {name: ([], []) for name in BASES}
    # The faces: the new ones, features on the blank base, and the old head-and-hair ones (split)
    # where there's no new one yet.
    faces = {os.path.basename(p)[:-4]: (p, True) for p in glob.glob(os.path.join(OLD_FACES, "face-*.png"))}
    faces.update({fid: (os.path.join(SOURCE, FOLDERS["face"], fid + ".png"), False) for fid in part_ids("face")})
    for fid, (path, old) in sorted(faces.items()):
        base = base_of(fid, edits)
        if base is None:
            fail("%s isn't a face in Story/13-art-prompts.md §4, or what it edits isn't" % rel(path))
        b = bases[base] if old else blanks[base]
        colour, alpha, ratio, notes, mask, tones = face_layer(b, load(path))
        hair = mask[:, :, 1] > 0.5
        face_alpha, hair_alpha = split_face(b, load(path), alpha, hair, ratio)
        hid = "hair-" + fid[len("face-"):]
        nothing = np.zeros(alpha.shape)
        if hair_alpha.sum() >= HAIR_LEAST and not os.path.exists(os.path.join(SOURCE, FOLDERS["hair"], hid + ".png")):
            hair_colour, hair_alpha = drawn(hid, colour, hair_alpha)
            hair_mask = np.stack([nothing, mask[:, :, 1] * (hair_alpha > 0), nothing], axis=2)
            layers[hid] = ("helm", base, shrink(hair_colour, hair_alpha, hair_mask), {"tones": [tones[0], None]})
            report.append("  %-24s %s, split from %s's head" % (hid, base, fid))
        facial = hair & (face_alpha > 0)
        face_mask = np.stack([mask[:, :, 0], mask[:, :, 1] * facial, mask[:, :, 2]], axis=2)
        face_tones = [tones[0] if facial.sum() >= 50 else None, tones[1]]
        colour, face_alpha = drawn(fid, colour, face_alpha)
        layers[fid] = ("face", base, shrink(colour, face_alpha, face_mask), {"skin": [round(float(v), 3) for v in ratio], "tones": face_tones})
        report.append("  %-24s %s, skin %s%s%s" % (fid, base, " ".join("%.2f" % v for v in ratio),
                                                    ", head and hair, split" if old else "",
                                                    ("; " + "; ".join(notes)) if notes else ""))
    # The hairs and helms painted on the blank base: one slot, what's on the head.
    for slot in ("hair", "helm"):
        for pid in part_ids(slot):
            base = base_of(pid, edits)
            if base is None:
                fail("tools/source/characters/%s/%s.png isn't a %s in Story/13-art-prompts.md, or what it edits isn't" % (FOLDERS[slot], pid, slot))
            colour, alpha, mask, tones = head_piece(blanks[base], load(os.path.join(SOURCE, FOLDERS[slot], pid + ".png")), slot == "hair")
            colour, alpha = drawn(pid, colour, alpha)
            layers[pid] = ("helm", base, shrink(colour, alpha, mask), {"tones": tones})
            report.append("  %-24s %s" % (pid, base))
    for aid in part_ids("armor"):
        base = base_of(aid, edits)
        if base is None:
            fail("tools/source/characters/armors/%s.png isn't an armor in Story/13-art-prompts.md §5, or what it edits isn't" % aid)
        colour, alpha, mask, tones = armor_layer(bases[base], load(os.path.join(SOURCE, FOLDERS["armor"], aid + ".png")))
        colour, alpha = drawn(aid, colour, alpha)
        layers[aid] = ("armor", base, shrink(colour, alpha, mask), {"tones": tones})
        report.append("  %-24s %s, dyes %s" % (aid, base, ", ".join("none" if t is None else "#%02x%02x%02x" % tuple(int(round(v)) for v in t) for t in tones)))
    # The capes, painted in the black behind the figure, and what the hands hold.
    for slot in ("cape", "weapon", "offhand"):
        for pid in part_ids(slot):
            base = base_of(pid, edits)
            if base is None:
                fail("tools/source/characters/%s/%s.png isn't a %s in Story/13-art-prompts.md §6, or what it edits isn't" % (FOLDERS[slot], pid, slot))
            cutter = cape_layer if slot == "cape" else hand_layer
            colour, alpha, mask, tones = cutter(bases[base], load(os.path.join(SOURCE, FOLDERS[slot], pid + ".png")))
            colour, alpha = drawn(pid, colour, alpha)
            layers[pid] = (slot, base, shrink(colour, alpha, mask), {"tones": tones} if tones else {})
            report.append("  %-24s %s" % (pid, base))

    # The atlases, a set of pages per slot; a part's mask goes beside it on its page.
    by_slot = {}
    for pid, (slot, base, layer, _) in layers.items():
        group = "base" if slot in ("body", "head") else slot
        by_slot.setdefault(group, {})[pid] = layer.rgba
        if layer.mask is not None:
            by_slot[group][pid + "#mask"] = layer.mask
    files, page_sizes, where = {}, {}, {}
    for group in sorted(by_slot):
        pages, places = pack(by_slot[group])
        for k, page in enumerate(pages):
            name = "%s-%d" % (group, k)
            files[name + ".png"] = page
            page_sizes[name] = [page.shape[1], page.shape[0]]
        for pid, (k, x, y, w, h) in places.items():
            where[pid] = ("%s-%d" % (group, k), [x, y, w, h])

    manifest = {"version": 5, "scale": PART_SCALE, "standardHeight": STANDARD * PART_SCALE,
                "canvas": [int(round(CANVAS[0] * PART_SCALE)), int(round(CANVAS[1] * PART_SCALE))],
                "sockets": list(SOCKETS), "slotSockets": SLOT_SOCKET, "order": list(ORDER),
                "pages": page_sizes, "bases": {}, "parts": {}, "recipes": book, "gear": items, "own": own}
    for name, b in bases.items():
        manifest["bases"][name] = {"sockets": {k: scaled(v) for k, v in b.sockets.items()},
                                   "head": [round(v * PART_SCALE, 2) for v in b.head],
                                   "body": name + ".body", "bald": name + ".head", "skin": b.skin_tone,
                                   "hands": {"near": name + ".hand.near", "far": name + ".hand.far"}}
    for pid in sorted(layers):
        slot, base, layer, extra = layers[pid]
        socket = SLOT_SOCKET[slot]
        sx, sy = bases[base].sockets[socket]
        page, rect = where[pid]
        entry = {"slot": slot, "base": base, "socket": socket, "page": page, "rect": rect,
                 "pivot": [round(sx * PART_SCALE - layer.at[0], 2), round(sy * PART_SCALE - layer.at[1], 2)]}
        if pid + "#mask" in where:
            entry["maskRect"] = where[pid + "#mask"][1]
        entry.update(extra)
        manifest["parts"][pid] = entry
    data = json.dumps(manifest, indent=1) + "\n"

    # A face and its armor are the body, so they have to be on one base; the game won't start with
    # a recipe that puts them on two. Checked here against what every part edits, made or not, so it's
    # caught before anything's painted for it.
    mixed = []
    for rid, r in sorted(book.items()):
        if r["face"] and r["armor"]:
            fb, ab = base_of(r["face"], edits), base_of(r["armor"], edits)
            if fb and ab and fb != ab:
                mixed.append("%s (%s on %s, %s on %s)" % (rid, r["face"], fb, r["armor"], ab))
    if mixed:
        fail("these recipes in Story/13-art-prompts.md §7 put a face and an armor painted on different bases together; give "
             "each an armor made on its face's base:\n  " + "\n  ".join(mixed))

    stale = [p for p in glob.glob(os.path.join(PARTS, "*.png")) if os.path.basename(p) not in files]
    if check:
        late = [rel(os.path.join(PARTS, n)) for n, img in files.items()
                if not os.path.exists(os.path.join(PARTS, n)) or not np.array_equal(ic.read_png(os.path.join(PARTS, n))[0], img)]
        if not os.path.exists(MANIFEST) or open(MANIFEST, encoding="utf-8").read() != data:
            late.append(rel(MANIFEST))
        late += [rel(p) for p in stale]
        if late:
            print("import_parts: out of date; run python tools/import_parts.py:\n  " + "\n  ".join(late))
            return 1
        print("import_parts: up to date (%d parts on %d pages, %d recipes)" % (len(layers), len(files), len(book)))
        return 0

    os.makedirs(PARTS, exist_ok=True)
    for p in stale:
        os.remove(p)
    for name, img in files.items():
        ic.write_png(os.path.join(PARTS, name), img)
    with open(MANIFEST, "w", encoding="utf-8", newline="\n") as f:
        f.write(data)
    ic.rewrite(ic.MGCB, M_BEGIN, M_END, mgcb_section(sorted(files)), None)
    ic.rewrite(ic.ASSETS, A_BEGIN, A_END, assets_section(sorted(files), layers, where), A_ANCHOR)

    write_previews(bases, layers, manifest, book)
    unknown = sorted({rid.split(".")[0] for rid in book} - set(known) - {"you-male", "you-female"})
    print("import_parts: %d parts (%d faces) on %d pages, %d recipes, into %s"
          % (len(layers), sum(1 for v in layers.values() if v[0] == "face"), len(files), len(book), rel(PARTS)))
    for name, b in bases.items():
        print("  %s sockets: %s" % (name, ", ".join("%s (%.1f, %.1f)" % (k, v[0] * PART_SCALE, v[1] * PART_SCALE) for k, v in b.sockets.items())))
    print("\n".join(report))
    if stale:
        print("  took out " + ", ".join(os.path.basename(p) for p in stale))
    if unknown:
        print("  recipes for ids the game doesn't have yet (they wait till it does): " + ", ".join(unknown))
    print("  previews in " + rel(PREVIEWS))
    return 0


def write_previews(bases, layers, manifest, book):
    os.makedirs(PREVIEWS, exist_ok=True)
    back = (24, 26, 34)
    sheet = [socket_preview(b, layers[n + ".body"][2], layers[n + ".head"][2]) for n, b in bases.items()]
    save_rgb(os.path.join(PREVIEWS, "sockets.png"), grid(sheet, 2, back))

    def parts_of(pid):
        slot, base, layer, _ = layers[pid]
        return layer, manifest["parts"][pid]["pivot"]

    def geometry(name):
        """A base's sockets and head box at PART_SCALE, as fit takes them."""
        return {"sockets": {k: scaled(v) for k, v in bases[name].sockets.items()}, "head": [v * PART_SCALE for v in bases[name].head]}

    faces = {name: ([], []) for name in bases}
    for pid in sorted(p for p in layers if layers[p][0] == "face"):
        slot, base, layer, extra = layers[pid]
        shown = {"body": parts_of(base + ".body"), "head": parts_of(base + ".head"), "face": parts_of(pid)}
        if "hair-" + pid[len("face-"):] in layers:
            shown["helm"] = parts_of("hair-" + pid[len("face-"):])
        canvas, root = compose(shown, (1.0, 1.0), {k: scaled(v) for k, v in bases[base].sockets.items()}, extra["skin"])
        # The portrait's box, as Characters/ImportedFigure.cs takes it, at the canvas's offset.
        hx, hy, hw, hh = (v * PART_SCALE for v in bases[base].head)
        ox, oy = root[0] - bases[base].sockets["root"][0] * PART_SCALE, root[1] - bases[base].sockets["root"][1] * PART_SCALE
        side = max(hw, hh)
        top, mid = hy - side * 0.1 + oy, hx + hw / 2 + ox
        tall = side * 1.4
        crop = canvas[max(0, int(top)):int(top + tall), max(0, int(mid - tall / 2)):int(mid + tall / 2)]
        faces[base][0].append(flat(crop, (70, 76, 92)))
        alone = layer.rgba.astype(np.float64) / 255.0
        faces[base][1].append(flat(np.concatenate([alone[:, :, :3] * alone[:, :, 3:], alone[:, :, 3:]], axis=2), (70, 76, 92)))
    for name, (shown, alone) in faces.items():
        if shown:
            save_rgb(os.path.join(PREVIEWS, "faces-%s.png" % name), grid(shown, 10, back))
            save_rgb(os.path.join(PREVIEWS, "faces-%s-layers.png" % name), grid(alone, 10, back))

    # Every armor on its base, bald, as it comes: what each outfit looks like before anyone wears it.
    for name in bases:
        worn = []
        for pid in sorted(p for p in layers if layers[p][0] == "armor" and layers[p][1] == name):
            canvas, _ = compose({"body": parts_of(name + ".body"), "armor": parts_of(pid), "head": parts_of(name + ".head")},
                                (1.0, 1.0), {k: scaled(v) for k, v in bases[name].sockets.items()}, None)
            worn.append(flat(canvas, (70, 76, 92))[::2, ::2])
        if worn:
            save_rgb(os.path.join(PREVIEWS, "armors-%s.png" % name), grid(worn, 12, back))

    figures = []
    for rid in sorted(book):
        r = book[rid]
        face, armor = r["face"] if r["face"] in layers else None, r["armor"] if r["armor"] in layers else None
        if face is None and armor is None:
            continue
        base = layers[face or armor][1]
        parts = {"body": parts_of(base + ".body"), "head": parts_of(base + ".head")}
        if face:
            parts["face"] = parts_of(face)
        if armor and layers[armor][1] == base:
            parts["armor"] = parts_of(armor)
        head = r.get("helm") if r.get("helm") in layers else ("hair-" + face[len("face-"):]) if face else None
        if head in layers:
            parts["helm"] = parts_of(head)
        # The cape and the weapon, if they're painted, fitted when they're on the other base; and
        # what's held, under the fist.
        fits = {}
        for slot in ("cape", "weapon"):
            if r.get(slot) in layers:
                parts[slot] = parts_of(r[slot])
                if layers[r[slot]][1] != base:
                    fits[slot] = fit(slot, geometry(layers[r[slot]][1]), geometry(base))
        fists = {"near": parts_of(base + ".hand.near"), "far": parts_of(base + ".hand.far")}
        skin = layers[face][3]["skin"] if face else None
        canvas, root = compose(parts, r["morph"], {k: scaled(v) for k, v in bases[base].sockets.items()}, skin, fits=fits, fists=fists)
        img = flat(canvas, (70, 76, 92))
        figures.append(img[::2, ::2])
    if figures:
        save_rgb(os.path.join(PREVIEWS, "recipes.png"), grid(figures, 12, back))
    write_dye_previews(bases, layers, parts_of, back)


# Swatches the dye previews try: an armor's Base and Trim twice over, and a face's hair and eyes.
PREVIEW_DYES = (((150, 28, 40), (222, 176, 70)), ((36, 52, 104), (200, 204, 214)))
PREVIEW_FACE = ((214, 214, 226), (150, 90, 210))


def mask_picture(layer, back):
    """A part's mask over it, greyed: skin orange, the first dye channel magenta, the second cyan."""
    rgba = layer.rgba.astype(np.float64) / 255.0
    grey = (rgba[:, :, :3] @ np.array([0.299, 0.587, 0.114]))[..., None] * np.ones(3) * 0.6
    m = layer.mask.astype(np.float64)[:, :, :3] / 255.0
    paint = grey + m[:, :, :1] * np.array([0.9, 0.5, 0.1]) + m[:, :, 1:2] * np.array([0.95, 0.2, 0.85]) + m[:, :, 2:3] * np.array([0.1, 0.85, 0.95])
    paint = np.clip(paint, 0.0, 1.0)
    return flat(np.concatenate([paint * rgba[:, :, 3:], rgba[:, :, 3:]], axis=2), back)


def write_dye_previews(bases, layers, parts_of, back):
    """dyes-armors-<base>.png: each armor as painted, its mask, and dyed twice; dyes-faces-<base>.png:
    each face as painted, its mask, and its hair and eyes dyed."""
    tile_back = (70, 76, 92)
    for name in bases:
        sockets = {k: scaled(v) for k, v in bases[name].sockets.items()}
        rows_out = []
        for pid in sorted(p for p in layers if layers[p][0] == "armor" and layers[p][1] == name):
            layer, tones = layers[pid][2], layers[pid][3]["tones"]
            parts = {"body": parts_of(name + ".body"), "armor": parts_of(pid), "head": parts_of(name + ".head")}
            shots = [flat(compose(parts, (1.0, 1.0), sockets, None)[0], tile_back)]
            for swatches in PREVIEW_DYES:
                pairs = [(s, t) if t else None for s, t in zip(swatches, tones)]
                shots.append(flat(compose(parts, (1.0, 1.0), sockets, None, dyes={"armor": pairs})[0], tile_back))
            h = max(s.shape[0] for s in shots)
            masked = mask_picture(layer, tile_back)
            shots.insert(1, masked)
            rows_out.append(grid([pad_to(s, h, tile_back)[::2, ::2] for s in shots], 4, tile_back))
        if rows_out:
            save_rgb(os.path.join(PREVIEWS, "dyes-armors-%s.png" % name), grid(rows_out, 3, back))
        faces = []
        for pid in sorted(p for p in layers if layers[p][0] == "face" and layers[p][1] == name):
            layer, extra = layers[pid][2], layers[pid][3]
            pairs = [(s, t) if t else None for s, t in zip(PREVIEW_FACE, extra["tones"])]
            parts = {"face": parts_of(pid)}
            shots = [flat(compose(parts, (1.0, 1.0), sockets, None)[0], tile_back), mask_picture(layer, tile_back),
                     flat(compose(parts, (1.0, 1.0), sockets, None, dyes={"face": pairs})[0], tile_back)]
            h = max(s.shape[0] for s in shots)
            faces.append(grid([pad_to(s, h, tile_back) for s in shots], 3, tile_back))
        if faces:
            save_rgb(os.path.join(PREVIEWS, "dyes-faces-%s.png" % name), grid(faces, 4, back))


def pad_to(img, height, back):
    """A picture made `height` tall by filling under it."""
    if img.shape[0] >= height:
        return img
    out = np.empty((height, img.shape[1], 3), np.uint8)
    out[:] = back
    out[:img.shape[0]] = img
    return out


def mgcb_section(names):
    lines = [M_BEGIN, ""]
    for name in names:
        png = "Characters/Parts/" + name
        lines += ["#begin " + png] + list(ic.TEXTURE) + ["/build:" + png, ""]
    lines += ["#begin Characters/Parts/parts.json", "/copy:Characters/Parts/parts.json", ""]
    return lines + [M_END]


def assets_section(names, layers, where):
    lines = [A_BEGIN, "## Characters (parts)", "",
             "The characters' parts made in ChatGPT from Story/13-art-prompts.md, each an edit of one of the two bases "
             "(tools/source/characters/base-male.png and base-female.png), cut by `tools/import_parts.py` from the pictures "
             "it keeps in tools/source/characters: keyed off the black, a base split at the jaw seam into its body and its "
             "bald head, a face cut to its head and the hair below its jaw, shrunk to half size and packed into atlas pages. "
             "The game puts each character together from their recipe at the base's sockets (Characters/Parts).", "",
             "tools/import_parts.py - written by Caleb Kiatoukaysi for Legend Quest, cuts the parts into layers at the bases' "
             "sockets, packs them into atlas pages and writes parts.json", "",
             "tools/vectorize.py - written by Caleb Kiatoukaysi for Legend Quest, redraws a picture as vector art (flat cel "
             "colours, smooth lines, Bezier outlines) and draws it back at any size; the importer runs the parts through it "
             "when VECTOR is on", "",
             "Content/Characters/Parts/parts.json - the sockets of each base, where every part is on its page and which "
             "socket it hangs from, and every recipe from Story/13-art-prompts.md §7; written by tools/import_parts.py", ""]
    for name in names:
        page = name[:-4]
        held = sorted(pid for pid, (p, _) in where.items() if p == page and not pid.endswith("#mask"))
        masks = sum(1 for pid, (p, _) in where.items() if p == page and pid.endswith("#mask"))
        what = ", ".join(held)
        lines += ["Content/Characters/Parts/%s - an atlas page of %d part%s (%s)%s, cut from tools/source/characters; %s"
                  % (name, len(held), "" if len(held) == 1 else "s", what,
                     " and %d mask%s" % (masks, "" if masks == 1 else "s") if masks else "", CREDIT), ""]
    return lines + [A_END]


def main(argv):
    parser = argparse.ArgumentParser(description="Cuts the character parts from ChatGPT into the game's sockets and atlases.")
    parser.add_argument("--check", action="store_true", help="say what's out of date and change nothing")
    parser.add_argument("--vector", nargs="*", metavar="ID", default=None,
                        help="redraw the parts as vector art this run, as VECTOR does; with ids, just those parts")
    args = parser.parse_args(argv)
    sys.exit(build(args.check, True if VECTOR or args.vector == [] else set(args.vector) if args.vector else False))


if __name__ == "__main__":
    main(sys.argv[1:])
