"""
Part sheets: several parts in one ChatGPT picture instead of one picture each.

    python tools/part_sheet.py templates                  # the blank sheets, from the bases
    python tools/part_sheet.py check <png> <sheet-id>     # does it keep the template's size and layout?
    python tools/part_sheet.py fit <png> <sheet-id> [--force] [--check <png>] [--record <json>]   # cut it, wherever ChatGPT put things
    python tools/part_sheet.py cut <png> <sheet-id> [--force]   # cut one that kept to the template exactly
    python tools/part_sheet.py list                       # the sheets and their cells, from Story/13 §4.0, §4.9, §4.10, §5.0 and §6.0

Seven kinds, each a 1536x1024 picture of cells with thin grey lines between them, on black:

  - a face sheet (face-sheet-*, Story/13 §4.0): five columns and two rows of 307x512 cells, each the
    same crop of one blank base at full size (its head, neck and shoulders), ten to a sheet: the two
    sheets are the whole shared set of ten men's and ten women's faces. ChatGPT paints a different
    face, the features only, into each.
  - a hair sheet (hair-sheet-*, §4.9): the hair library, in bulk. Eight columns and two rows of
    192x512 cells, each the blank base at HAIR_SCALE (half size, the size the game keeps its parts
    at, so nothing's lost) from the top of the canvas to the hips: the man along the top row, the
    woman along the bottom. ChatGPT gives each column one hairstyle, on both heads, so every style
    fits either base (the woman's comes out as <id>-f).
  - an extra sheet (extra-sheet-*, §4.10): what goes on or over the face that isn't the face or the
    hair: beards, jewellery, spectacles, scars, a people's ears and horns. Six columns and two rows
    of 256x512 cells, each the blank base at EXTRA_SCALE from the top of the canvas to the chest. A
    sheet for both (base `both`) has the man above and the woman below, one item a column, as a
    hair sheet; one for the man alone (base-male: the beards) has twelve, a different one in each
    cell.
  - an armor sheet (armor-sheet-*, §5.0): four columns of 384x1024 cells, each the whole base at
    ARMOR_SCALE (640 x 960, cropped to the cell round the figure). ChatGPT dresses each figure in a
    different outfit. At that scale a standard adult is still 844 pixels tall, more than the parts
    keep (tools/import_parts.py's PART_SCALE), so nothing's lost against a single edit.
  - a helm sheet (helm-sheet-*, §6.0): four columns of 384x1024 cells, each the blank base at full
    size from the top of the canvas, room over the head for a crown or a plume, to the hips. ChatGPT
    puts a helm, hood, hat or crown on each head.
  - a cape sheet (cape-sheet-*, §6.0): four columns of 384x1024 cells, each the whole base at
    GEAR_SCALE, a little smaller than an armor's so there's room behind the back for a cloak.
    ChatGPT hangs a cape behind each figure. A standard adult is still 756 pixels tall, more than
    the parts keep.
  - a weapon sheet (weapon-sheet-*, §6.0): the cape sheet's cells, but showing only the base's two
    fists, each with a stub of forearm, where they are on the figure (fists_only): with no arm to
    move, ChatGPT can't pose the figure, and the item goes straight through the closed near fist
    (and, where the item says, the far one), the way a 2D game character holds one.

`templates` writes the blank sheets, <kind>-sheet-male.png and <kind>-sheet-female.png for each kind,
into tools/source/characters; a pair sheet's is <kind>-sheet.png (hair-sheet.png, extra-sheet.png),
and the beards' extra-sheet-male.png. Since the cut-out rig (2026-11-11) the bases are the bodies
painted in pieces and stood up at rest by tools/body_parts.py (base-male.png, base-female.png); till
they're painted, the sheets are drawn from the body templates' own pieces stood up the same way
(body_parts.standing), the same shapes in the same places, so a sheet painted on either cuts the
same. Run it again when the bodies come. An armor isn't a sheet of four whole figures any more: it's
painted in pieces on body_parts.py's armor template (armor-parts-man.png, armor-parts-woman.png), so
`templates` makes no armor sheets; the armor kind stays only to cut the old ones.

Every template is 1536 x 1024, a size ChatGPT makes as it is, so it has no reason to crop, pad or
stretch the picture it sends back. It still sometimes does (a 1535-wide sheet, busts drawn bigger,
a figure a little taller): `check` says whether a sheet kept to its template, cell by cell (its
size against the template's, where it is, how well it matched), and whether it's good to cut or
should be redone; `fit` cuts it wherever ChatGPT put things, each cell back at its base's size.

`fit` is `cut` for a sheet ChatGPT didn't keep to the template for: painted at another size (1774 x
887, say), with the busts drawn bigger than the template's, or every figure a little taller. It finds
each cell between the sheet's grey lines and finds the base under it anywhere, at any scale
(tools/sheet_fit.py), matched on FIT_WINDOWS, where the part leaves the base alone; then puts the cell
back on the canvas the same way, keeping a hair's or an extra's pixels only where they differ from
the base drawn the same way (changed_only) and only what's the part's own (own_part). `--check <png>`
writes a picture of each cell put back, with the base's outline over it, to see the fit is exact;
`--record <json>` keeps each cell's fit (scale, place, score, and the sheet's hash), so cutting the
same sheet again uses it instead of fitting again.

`cut` keeps the sheet ChatGPT sent back as tools/source/characters/sheets/<sheet-id>.png, then for
each cell:
  - finds how far ChatGPT shifted it, up to SEARCH pixels each way, by matching edges against the
    template where the part leaves the base alone: the neck and shoulders under a face, the waist
    and arms under a helm, the bald head over an armor or a cape, the fists of a weapon sheet. A
    scaled cell (an armor's, a cape's, a weapon's) is matched on its fists as well (WINDOWS), and on
    a range of scales (SCALES), since ChatGPT sometimes draws the whole figure a little bigger or smaller;
  - puts the shifted cell back where it came from on the base's canvas, leaving out the grey lines:
    a face, a hair or a helm is pasted onto a copy of its base; an armor is resampled (Mitchell, on
    premultiplied colour) back up to full size onto black; a cape or a weapon is resampled too, and
    kept only where it differs from its template (only_changed), the base's own pixels everywhere
    else. Either way the result lines up with the base exactly as a single edit would;
  - saves it as tools/source/characters/<folder>/<id>.png (faces, hairs, armors, helms, capes,
    weapons), and warns when the match
    is poor (ChatGPT moved or rescaled the figure: make that part on its own) or when the part runs
    off the cell (it's cut there).
Then it rewrites its own section of ASSETS.md from what's on disk. tools/import_parts.py takes the
parts from there into the game.
"""
import glob
import os
import re
import shutil
import sys

import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(HERE)
sys.path.insert(0, HERE)
import import_character as ic  # noqa: E402  (its PNG reader and writer, and Mitchell resampling)
import sheet_fit  # noqa: E402  (a cell found on its base at any scale and place)

CHARS = os.path.join(ROOT, "tools", "source", "characters")
SHEETS = os.path.join(CHARS, "sheets")
STORY = os.path.join(ROOT, "Story", "13-art-prompts.md")
ASSETS = os.path.join(ROOT, "ASSETS.md")

SHEET_W, SHEET_H = 1536, 1024
BASES = ("base-female", "base-male")
LINE = (58, 58, 58)
LINE_HALF = 1       # the grey lines are 2 px wide, on the cell boundaries
KEEP_OFF_LINES = 8  # pixels this close to a line aren't the sheet's
SEARCH = 18
#: An armor sheet's cells are the base at this scale, which makes its 1024 x 1536 canvas exactly 640 x 960.
ARMOR_SCALE = 0.625
#: A hair sheet's cells are the base at half size, as the game keeps every part (import_parts.py's
#: PART_SCALE): the canvas from the top to the hips, 384 wide round the head, in 192 x 512.
HAIR_SCALE = 0.5
#: An extra sheet's cells are the base at three quarters: the head, the neck and the chest, 341 x
#: 683 of the canvas in 256 x 512.
EXTRA_SCALE = 0.75
#: The two rows of a pair sheet (a sheet whose base is `both`), top then bottom: the same item on
#: each. The woman's copy of a part is the man's id with -f on the end.
PAIR = ("base-male", "base-female")
#: A cape or weapon sheet's cells are the base a little smaller, 573 x 860, so a cloak sweeping out
#: behind the back or a staff taller than the figure fits in the cell: a cell is the canvas from
#: x 100 to 786, the whole figure with room behind it, and 40 pixels over the head.
GEAR_SCALE = 0.56
GEAR_ORIGIN = (56, -40)
#: On a cape or weapon sheet, how different from the template a pixel has to be to be the part's;
#: everything else is the base's own, so the part comes back as clean as a single edit.
GEAR_CHANGED = 36

#: Each kind of sheet: its grid, its cells' crop of the base, what to match a cell on, and where its parts go.
#:   origin: where a cell's top left is on the base's canvas at the sheet's scale (x, y; y can be negative: padding)
#:   match:  the rows and columns of a cell (top, bottom, left, right) the part leaves the base alone in
KINDS = {
    # Ten to a sheet, five across: a cell is 307 wide, the head in its middle.
    "face": {
        "cols": 5, "rows": 2, "scale": 1.0, "folder": "faces", "section": "§4.0",
        "origin": {"base-male": (375, 36), "base-female": (370, 71)},
        "match": {"base-male": (304 - 36 + 60, 512 - SEARCH - 4, SEARCH + 4, 307 - SEARCH - 4),
                  "base-female": (330 - 71 + 60, 512 - SEARCH - 4, SEARCH + 4, 307 - SEARCH - 4)},
        "poor": 9.0,
    },
    # Eight styles, each on both heads; matched on the face and the near fist (WINDOWS, below).
    "hair": {
        "cols": 8, "rows": 2, "scale": HAIR_SCALE, "folder": "hairs", "section": "§4.9",
        "origin": {"base-male": (168, 0), "base-female": (166, 0)}, "poor": 9.0,
    },
    # Six items on both heads, or twelve on one; matched on the near shoulder (WINDOWS, below).
    "extra": {
        "cols": 6, "rows": 2, "scale": EXTRA_SCALE, "folder": "extras", "section": "§4.10",
        "origin": {"base-male": (268, 0), "base-female": (265, 0)}, "poor": 9.0,
    },
    "armor": {
        "cols": 4, "rows": 1, "scale": ARMOR_SCALE, "folder": "armors", "section": "§5.0",
        "origin": {"base-male": (128, -32), "base-female": (128, -32)},
        # The bald head, crown to chin, and round it.
        "match": {"base-male": (int(100 * ARMOR_SCALE) + 32, int(304 * ARMOR_SCALE) + 32, int(436 * ARMOR_SCALE) - 128, int(610 * ARMOR_SCALE) - 128),
                  "base-female": (int(135 * ARMOR_SCALE) + 32, int(330 * ARMOR_SCALE) + 32, int(430 * ARMOR_SCALE) - 128, int(597 * ARMOR_SCALE) - 128)},
        "poor": 9.0,
    },
    "helm": {
        "cols": 4, "rows": 1, "scale": 1.0, "folder": "helms", "section": "§6.0",
        # From the top of the canvas, so a tall crown or a plume has room over the head.
        "origin": {"base-male": (330, 0), "base-female": (330, 0)},
        # The waist and the arms, canvas rows 500 to 800, which no helm reaches (a hood's drape and a
        # mail coif stop at the shoulders).
        "match": {"base-male": (500, 800, SEARCH + 4, 384 - SEARCH - 4),
                  "base-female": (500, 800, SEARCH + 4, 384 - SEARCH - 4)},
        "poor": 9.0,
    },
    # Their windows are filled in below, from the armor's head and fists.
    "cape": {
        "cols": 4, "rows": 1, "scale": GEAR_SCALE, "folder": "capes", "section": "§6.0",
        "origin": {"base-male": GEAR_ORIGIN, "base-female": GEAR_ORIGIN}, "poor": 9.0,
    },
    "weapon": {
        "cols": 4, "rows": 1, "scale": GEAR_SCALE, "folder": "weapons", "section": "§6.0",
        "origin": {"base-male": GEAR_ORIGIN, "base-female": GEAR_ORIGIN}, "poor": 9.0,
    },
}

#: An armor cell is also matched on its two fists, which every outfit keeps in place (gloves or bare),
#: so a figure ChatGPT drew a little bigger or smaller is found and scaled back: the scale is searched
#: about the head's middle, in SCALES, round the best shift at full size.
FISTS = {"base-male": ((510, 573, 34, 103), (507, 569, 297, 350)), "base-female": ((512, 574, 43, 100), (518, 573, 293, 340))}
HEAD_MIDDLE = {"base-male": (198.25, 162.0), "base-female": (193.25, 180.75)}
SCALES = [round(0.96 + 0.005 * i, 3) for i in range(17)]
NUDGE = 4

#: The kinds painted on the blank-faced bases: what's on and of the head.
ON_BLANK = ("face", "hair", "helm", "extra")

def to_cell(kind, base_name, r0, r1, c0, c1):
    """A window of the base's canvas (rows r0 to r1, columns c0 to c1) in a kind's cell."""
    s = KINDS[kind]["scale"]
    ox, oy = KINDS[kind]["origin"][base_name]
    return int(r0 * s - oy), int(r1 * s - oy), int(c0 * s - ox), int(c1 * s - ox)


def from_armor(window, base_name):
    """An armor cell's window (rows, then columns) on the base's canvas."""
    r0, r1, c0, c1 = window
    ox, oy = KINDS["armor"]["origin"][base_name]
    return (r0 + oy) / ARMOR_SCALE, (r1 + oy) / ARMOR_SCALE, (c0 + ox) / ARMOR_SCALE, (c1 + ox) / ARMOR_SCALE


#: What a scaled cell is matched on, the score's window first: an armor's head and fists; a cape
#: leaves the head and both fists alone as well; a weapon sheet has only the fists. And the point
#: ChatGPT's resizing is searched about: the head's middle, or between a weapon sheet's fists.
WINDOWS = {"armor": {b: [KINDS["armor"]["match"][b]] + list(FISTS[b]) for b in BASES}}
MIDDLE = {"armor": HEAD_MIDDLE}
for _kind in ("cape", "weapon"):
    WINDOWS[_kind], MIDDLE[_kind] = {}, {}
    for _b in BASES:
        _fists = [to_cell(_kind, _b, *from_armor(f, _b)) for f in FISTS[_b]]
        _ox, _oy = KINDS[_kind]["origin"][_b]
        if _kind == "cape":
            # The head, then both fists, about the head's middle, as an armor.
            WINDOWS[_kind][_b] = [to_cell(_kind, _b, *from_armor(KINDS["armor"]["match"][_b], _b))] + _fists
            _ax, _ay = HEAD_MIDDLE[_b]
            _aox, _aoy = KINDS["armor"]["origin"][_b]
            MIDDLE[_kind][_b] = ((_ax + _aox) / ARMOR_SCALE * GEAR_SCALE - _ox, (_ay + _aoy) / ARMOR_SCALE * GEAR_SCALE - _oy)
        else:
            # A weapon sheet has only the fists: the far one first (an item in the near fist leaves it
            # alone, so it's the score), then the near one, about the point between them.
            WINDOWS[_kind][_b] = [_fists[1], _fists[0]]
            MIDDLE[_kind][_b] = (sum(w[2] + w[3] for w in _fists) / 4.0, sum(w[0] + w[1] for w in _fists) / 4.0)
    KINDS[_kind]["match"] = {b: WINDOWS[_kind][b][0] for b in BASES}


def _head_middle(kind, b):
    """The head's middle in a scaled kind's cell (from the armor cell's HEAD_MIDDLE)."""
    hx, hy = HEAD_MIDDLE[b]
    aox, aoy = KINDS["armor"]["origin"][b]
    ox, oy = KINDS[kind]["origin"][b]
    s = KINDS[kind]["scale"]
    return ((hx + aox) / ARMOR_SCALE * s - ox, (hy + aoy) / ARMOR_SCALE * s - oy)


#: A hairstyle leaves the front of the face and the jaw alone (rows, columns on the canvas), and the
#: front of the body below (long hair falls down the back); an extra leaves the near shoulder alone.
FACE_FRONT = {"base-male": (170, 295, 520, 600), "base-female": (200, 325, 515, 587)}
BODY_FRONT = {"base-male": (520, 640, 520, 600), "base-female": (560, 660, 510, 590)}
NEAR_SHOULDER = {"base-male": (380, 450, 385, 440), "base-female": (390, 455, 400, 460)}
WINDOWS["hair"] = {b: [to_cell("hair", b, *FACE_FRONT[b]), to_cell("hair", b, *BODY_FRONT[b])] for b in BASES}
WINDOWS["extra"] = {b: [to_cell("extra", b, *NEAR_SHOULDER[b])] for b in BASES}
for _kind in ("hair", "extra"):
    MIDDLE[_kind] = {b: _head_middle(_kind, b) for b in BASES}
    KINDS[_kind]["match"] = {b: WINDOWS[_kind][b][0] for b in BASES}


def cells(kind, base_name, ids):
    """Each cell of a sheet in order: (its number from 0, its base, its part's id). A pair sheet's
    (base `both`) top row is the man's copy of each item and its bottom row the woman's (-f)."""
    if base_name != "both":
        return [(k, base_name, pid) for k, pid in enumerate(ids)]
    cols = KINDS[kind]["cols"]
    return [(row * cols + k, b, pid + ("-f" if row else "")) for row, b in enumerate(PAIR) for k, pid in enumerate(ids)]

A_BEGIN = "<!-- part sheets: written by tools/part_sheet.py; don't edit between these two lines -->"
A_END = "<!-- end part sheets -->"
OLD_SECTIONS = (("<!-- face sheets: written by tools/face_sheet.py; don't edit between these two lines -->", "<!-- end face sheets -->"),)
A_ANCHOR = "## Tiles and props"
CREDIT = "supplied by Caleb Kiatoukaysi, made in ChatGPT"


def kind_of(sheet_id):
    for kind in KINDS:
        if sheet_id.startswith(kind + "-sheet-"):
            return kind
    sys.exit("part_sheet: %s isn't a sheet id (%s)" % (sheet_id, ", ".join(k + "-sheet-*" for k in KINDS)))


def cell_size(kind):
    k = KINDS[kind]
    return SHEET_W // k["cols"], SHEET_H // k["rows"]


def template_id(kind, base_name):
    return "%s-sheet" % kind if base_name == "both" else "%s-sheet-%s" % (kind, base_name.split("-")[1])


#: The templates each kind has: a pair sheet's for the hair, a pair sheet's and the man's for the
#: extras (the beards are his alone), and one for each base for the rest.
TEMPLATES = {"hair": ("both",), "extra": ("both", "base-male")}


def load(path):
    """A picture as H x W x 3 uint8, flattened onto black if it carries alpha."""
    rgba, own_alpha = ic.read_png(path)
    rgb = rgba[:, :, :3].astype(np.float64)
    if own_alpha:
        rgb *= rgba[:, :, 3:4] / 255.0
    return np.rint(rgb).astype(np.uint8)


def save(path, rgb):
    ic.write_png(path, np.dstack([rgb, np.full(rgb.shape[:2], 255, np.uint8)]))


def resample_rgb(rgb, box, size, weight=None):
    """A box of an RGB picture (x, y, w, h in its pixels) resampled to size (w, h) with Mitchell's
    filter, on premultiplied colour, where `weight` (0 to 1, the same size as rgb) says how much of
    each pixel counts; what nothing counts towards comes out black. Returns RGB uint8."""
    a = np.ones(rgb.shape[:2]) if weight is None else weight
    premultiplied = np.concatenate([rgb / 255.0 * a[:, :, None], a[:, :, None]], axis=2)
    out = ic.resample(premultiplied, box, size)
    return np.rint(np.clip(out[:, :, :3], 0.0, 1.0) * 255.0).astype(np.uint8)


_STANDING = {}


def base(name, kind=None):
    """A base as RGB; for a face, a hair or a helm, the blank base (its features taken off) if it's
    there. Till the body's painted, the body template's own pieces stood up (body_parts.standing)."""
    blank = os.path.join(CHARS, name + "-blank.png")
    if kind in ON_BLANK and os.path.exists(blank):
        return load(blank)
    own = os.path.join(CHARS, name + ".png")
    if os.path.exists(own):
        return load(own)
    if name not in _STANDING:
        import body_parts
        _STANDING[name] = body_parts.standing(name)
    return _STANDING[name].copy()


def base_file(name, kind):
    """The base picture a kind's sheets are made from, as tools/source/characters/<this>.png."""
    blank = name + "-blank"
    return blank if kind in ON_BLANK and os.path.exists(os.path.join(CHARS, blank + ".png")) else name


def cell_box(kind, k):
    cw, ch = cell_size(kind)
    cols = KINDS[kind]["cols"]
    return (k % cols) * cw, (k // cols) * ch


def line_mask(kind, outer=False):
    """The grey lines between the cells. With outer, the sheet's own edges too, where ChatGPT
    sometimes draws a frame round the whole picture."""
    k = KINDS[kind]
    cw, ch = cell_size(kind)
    m = np.zeros((SHEET_H, SHEET_W), bool)
    if outer:
        m[:LINE_HALF * 2, :] = m[-LINE_HALF * 2:, :] = True
        m[:, :LINE_HALF * 2] = m[:, -LINE_HALF * 2:] = True
    for c in range(1, k["cols"]):
        m[:, c * cw - LINE_HALF:c * cw + LINE_HALF] = True
    for r in range(1, k["rows"]):
        m[r * ch - LINE_HALF:r * ch + LINE_HALF, :] = True
    return m


#: A weapon sheet's cells show only the base's two fists, each with a stub of forearm (FOREARM
#: pixels of it, on the canvas), where they are on the base: with no arm or body in the picture,
#: ChatGPT can't move them into a pose, and puts the item straight through the closed fist.
FOREARM = 60
#: Each fist's box on the canvas (left, right, top, bottom), the near fist's first: the fist itself
#: and the forearm above it, clear of the thigh and the shorts beside it, measured on the bases.
FIST_BOXES = {"base-male": ((256, 373, 752, 869), (676, 769, 748, 863)),
              "base-female": ((270, 368, 756, 872), (669, 753, 766, 869))}


def fists_only(b, base_name):
    """A base with everything but its two fists, and a stub of each forearm, blacked out: in each
    fist's box, the biggest piece of the figure."""
    figure = b.max(axis=2) > 12
    keep = np.zeros(b.shape[:2], bool)
    for c0, c1, r0, r1 in FIST_BOXES[base_name]:
        box = np.zeros(b.shape[:2], bool)
        box[r0 - FOREARM:r1, c0:c1] = True
        labels, count = ic.components(figure & box)
        if count:
            sizes = np.bincount(labels.ravel())
            sizes[0] = 0
            keep |= labels == int(np.argmax(sizes))
    # Opened, so the hair-thin edge of an outline that runs on beside a fist goes.
    keep &= ic.grow(~ic.grow(~keep, 2), 2)
    return np.where(keep[:, :, None], b, 0).astype(np.uint8)


def cell_of(kind, base_name):
    """One cell of a kind's template: the base cropped (and for an armor, a cape or a weapon, scaled)
    as the cell holds it; for a weapon, only its fists."""
    b = base(base_name, kind)
    if kind == "weapon":
        b = fists_only(b, base_name)
    cw, ch = cell_size(kind)
    s = KINDS[kind]["scale"]
    ox, oy = KINDS[kind]["origin"][base_name]
    if s == 1.0:
        return b[oy:oy + ch, ox:ox + cw]
    # The box of the base's canvas a cell covers, in the canvas's pixels.
    return resample_rgb(b, (ox / s, oy / s, cw / s, ch / s), (cw, ch))


def template(kind, base_name):
    cw, ch = cell_size(kind)
    cols = KINDS[kind]["cols"]
    crops = {b: cell_of(kind, b) for b in (PAIR if base_name == "both" else (base_name,))}
    sheet = np.zeros((SHEET_H, SHEET_W, 3), np.uint8)
    for k in range(cols * KINDS[kind]["rows"]):
        x, y = cell_box(kind, k)
        sheet[y:y + ch, x:x + cw] = crops[PAIR[k // cols] if base_name == "both" else base_name]
    sheet[line_mask(kind)] = LINE
    return sheet


def edges(rgb):
    g = rgb.astype(np.float64) @ np.array([0.299, 0.587, 0.114])
    gx = np.zeros_like(g)
    gy = np.zeros_like(g)
    gx[:, 1:-1] = g[:, 2:] - g[:, :-2]
    gy[1:-1, :] = g[2:, :] - g[:-2, :]
    return np.hypot(gx, gy)


def sheets_table():
    """{sheet id: (base, priority, [part ids in cell order])} from Story/13 §4.0, §4.9, §5.0 and §6.0."""
    text = open(STORY, encoding="utf-8").read()
    out = {}
    for m in re.finditer(r"^\| `((?:face|hair|armor|helm|cape|weapon|extra)-sheet-[a-z0-9-]+)` \| `(base-[a-z]+|both)` \| (P\d) \| ([^|]+) \|", text, re.M):
        out[m.group(1)] = (m.group(2), m.group(3), re.findall(r"`((?:face|hair|armor|helm|cape|weapon|offhand|beard|jewel|skin|ears|horns)-[a-z0-9.-]+)`", m.group(4)))
    if not out:
        sys.exit("part_sheet: no sheets in Story/13-art-prompts.md §4.0, §4.9, §5.0 or §6.0")
    return out


def cut(png, sheet_id, force=False):
    table = sheets_table()
    if sheet_id not in table:
        sys.exit("part_sheet: %s isn't a sheet in Story/13 (python tools/part_sheet.py list)" % sheet_id)
    kind = kind_of(sheet_id)
    k_ = KINDS[kind]
    base_name, _, ids = table[sheet_id]
    got = load(png)
    if got.shape[:2] != (SHEET_H, SHEET_W):
        print("part_sheet: %s is %dx%d; scaling it to %dx%d" % (png, got.shape[1], got.shape[0], SHEET_W, SHEET_H))
        got = resample_rgb(got, (0, 0, got.shape[1], got.shape[0]), (SHEET_W, SHEET_H))
    tpl = template(kind, base_name)
    cw, ch = cell_size(kind)
    s = k_["scale"]
    lines = line_mask(kind, outer=True)
    near = np.zeros_like(lines)  # pixels near a grey line, or the sheet's edge, aren't the sheet's
    for d in range(-KEEP_OFF_LINES, KEEP_OFF_LINES + 1):
        near |= np.roll(lines, d, axis=0) | np.roll(lines, d, axis=1)
    e_got, e_tpl = edges(got), edges(tpl)
    folder = os.path.join(CHARS, k_["folder"])
    os.makedirs(folder, exist_ok=True)
    report = []
    for k, cell_base, part in cells(kind, base_name, ids):
        b = base(cell_base, kind)
        ox, oy = k_["origin"][cell_base]
        r0, r1, c0, c1 = k_["match"][cell_base]
        x, y = cell_box(kind, k)
        ref = e_tpl[y + r0:y + r1, x + c0:x + c1]
        best = None
        for dy in range(-SEARCH, SEARCH + 1):
            for dx in range(-SEARCH, SEARCH + 1):
                if y + r0 + dy < 0 or y + r1 + dy > SHEET_H or x + c0 + dx < 0 or x + c1 + dx > SHEET_W:
                    continue
                win = e_got[y + r0 + dy:y + r1 + dy, x + c0 + dx:x + c1 + dx]
                score = np.abs(np.minimum(win, 120) - np.minimum(ref, 120)).mean()
                if best is None or score < best[0]:
                    best = (score, dx, dy)
        score, dx, dy = best
        grow = 1.0
        if s != 1.0:
            score, grow, dx, dy = rescale(kind, e_got, e_tpl, x, y, cell_base, dx, dy)
        # What of the sheet counts: inside this cell, clear of the grey lines.
        mine = np.zeros((SHEET_H, SHEET_W), bool)
        mine[y:y + ch, x:x + cw] = True
        mine &= ~near
        if k_["scale"] == 1.0:
            out = b.copy()
            ry, rx = np.mgrid[0:ch, 0:cw]
            sy, sx = y + ry + dy, x + rx + dx
            ok = (sy >= 0) & (sy < SHEET_H) & (sx >= 0) & (sx < SHEET_W)
            ok &= mine[np.clip(sy, 0, SHEET_H - 1), np.clip(sx, 0, SHEET_W - 1)]
            out[oy + ry[ok], ox + rx[ok]] = got[sy[ok], sx[ok]]
        else:
            # The whole canvas, from the cell it was scaled into: canvas (X, Y) is the template cell's
            # t = (X * s - ox, Y * s - oy), which ChatGPT drew at (x, y) + a + (t - a) * grow + (dx, dy).
            ax, ay = MIDDLE[kind][cell_base]
            box = (x + ax + (-ox - ax) * grow + dx, y + ay + (-oy - ay) * grow + dy, b.shape[1] * s * grow, b.shape[0] * s * grow)
            out = resample_rgb(got, box, (b.shape[1], b.shape[0]), weight=mine.astype(np.float64))
            if kind in ("cape", "weapon", "hair", "extra"):
                out = only_changed(kind, out, b, tpl, x, y, ox, oy, s, near)
        notes = spills(kind, got, tpl, mine, x, y, dx, dy)
        dest = os.path.join(folder, part + ".png")
        if os.path.exists(dest) and not force:
            report.append("  cell %d %s: kept the one already there (--force to replace it)" % (k + 1, part))
            continue
        save(dest, out)
        if score > k_["poor"]:
            notes.insert(0, "POOR MATCH (%.1f): ChatGPT moved or resized this figure; check it, or make it on its own" % score)
        scaled_note = ", scaled %.3f" % (1.0 / grow) if grow != 1.0 else ""
        report.append("  cell %d %s: shifted %+d, %+d%s, match %.1f%s" % (k + 1, part, dx, dy, scaled_note, score,
                                                                        ("; " + "; ".join(notes)) if notes else ""))
    os.makedirs(SHEETS, exist_ok=True)
    kept = os.path.join(SHEETS, sheet_id + ".png")
    if os.path.abspath(png) != os.path.abspath(kept):
        shutil.copyfile(png, kept)
    write_assets(table)
    print("part_sheet: cut %s (%s, %d %ss) into tools/source/characters/%s/" % (sheet_id, base_name, len(cells(kind, base_name, ids)), kind, k_["folder"]))
    print("\n".join(report))


def through_filter(edge, box, size):
    """An edge map, clipped at 120 as the matching clips it, resampled (box and size as
    ic.resample takes them). The resampler keeps colour within alpha, so it goes through as a 0-to-1
    colour on a solid alpha and comes back out at its own scale."""
    e = np.minimum(edge, 120.0) / 120.0
    picture = np.stack([e, e, e, np.ones_like(e)], axis=2)
    return ic.resample(picture, box, size)[:, :, 0] * 120.0


def only_changed(kind, out, b, tpl, x, y, ox, oy, s, near):
    """A cape's or a weapon's cell, put back at full size (out), kept only where it differs from its
    template cell put back the same way: everywhere else it's the base's own pixel. Scaling the cell
    down and up again softens the whole figure, so without this every outline would read as changed.
    Specks are opened away and what's kept is grown a little, to keep the part's own soft edge."""
    h, w = b.shape[:2]
    cell_w, cell_h = cell_size(kind)
    mine = np.zeros((SHEET_H, SHEET_W), bool)
    mine[y:y + cell_h, x:x + cell_w] = True
    mine &= ~near
    ref = resample_rgb(tpl, (x - ox, y - oy, w * s, h * s), (w, h), weight=mine.astype(np.float64))
    changed = np.abs(out.astype(np.int16) - ref.astype(np.int16)).max(axis=2) > GEAR_CHANGED
    changed = ic.grow(~ic.grow(~changed, 1), 1)
    changed = ic.grow(changed, 2)
    return np.where(changed[:, :, None], out, b)


def rescale(kind, e_got, e_tpl, x, y, base_name, dx0, dy0):
    """(score, grow, dx, dy) for a scaled cell (an armor's, a cape's or a weapon's): how much bigger
    ChatGPT drew the figure than the template (about the head's middle) and the shift, matching the
    kind's WINDOWS. The sheet's edges are resampled into the template's geometry at each scale and
    matched within NUDGE of the shift found at full size. The score is the head's match at the best
    of them."""
    cw, ch = cell_size(kind)
    ax, ay = MIDDLE[kind][base_name]
    windows = WINDOWS[kind][base_name]
    pad = NUDGE + 1
    # Mitchell's filter softens a little even at full size, so the template goes through it too.
    soft = through_filter(e_tpl, (x - pad, y - pad, cw + 2 * pad, ch + 2 * pad), (cw + 2 * pad, ch + 2 * pad))
    ref = [soft[r0 + pad:r1 + pad, c0 + pad:c1 + pad] for r0, r1, c0, c1 in windows]
    best = None
    for grow in SCALES:
        # The template cell's (tx, ty) is the sheet's (x + ax + (tx - ax) * grow, y + ay + (ty - ay) * grow).
        box = (x + ax * (1 - grow) - pad * grow, y + ay * (1 - grow) - pad * grow, (cw + 2 * pad) * grow, (ch + 2 * pad) * grow)
        grid = through_filter(e_got, box, (cw + 2 * pad, ch + 2 * pad))
        for dy in range(dy0 - NUDGE, dy0 + NUDGE + 1):
            for dx in range(dx0 - NUDGE, dx0 + NUDGE + 1):
                total = count = 0.0
                for (r0, r1, c0, c1), r in zip(windows, ref):
                    oy, ox = r0 + pad + dy, c0 + pad + dx
                    if oy < 0 or ox < 0 or oy + (r1 - r0) > grid.shape[0] or ox + (c1 - c0) > grid.shape[1]:
                        total = None
                        break
                    total += np.abs(grid[oy:oy + r1 - r0, ox:ox + c1 - c0] - r).sum()
                    count += r.size
                if total is None:
                    continue
                score = total / count
                if best is None or score < best[0] - 1e-9:
                    best = (score, grow, dx, dy, grid)
    # The score reported is the head's alone: an outfit's gloves change the fists, but nothing should
    # change the bald head, so it says how well the figure lines up.
    _, grow, dx, dy, grid = best
    r0, r1, c0, c1 = windows[0]
    head = np.abs(grid[r0 + pad + dy:r1 + pad + dy, c0 + pad + dx:c1 + pad + dx] - ref[0]).mean()
    return head, grow, dx, dy


def spills(kind, got, tpl, mine, x, y, dx, dy):
    """Where the part runs off its cell and is cut: new, bright pixels along the cell's edges where
    the template is black."""
    cw, ch = cell_size(kind)
    notes = []
    band = KEEP_OFF_LINES + 2
    sides = {"bottom": (slice(y + ch - band - 2, y + ch - KEEP_OFF_LINES - 1), slice(x, x + cw))}
    if kind in ("armor", "cape", "weapon", "helm", "hair", "extra"):
        sides["left"] = (slice(y, y + ch), slice(x + KEEP_OFF_LINES + 1, x + band + 2))
        sides["right"] = (slice(y, y + ch), slice(x + cw - band - 2, x + cw - KEEP_OFF_LINES - 1))
    if kind in ("cape", "weapon", "helm", "hair", "extra"):
        sides["top"] = (slice(y + KEEP_OFF_LINES + 1, y + band + 2), slice(x, x + cw))
    for side, (rs, cs) in sides.items():
        # The template where this part of the cell came from, before ChatGPT shifted it.
        ys = np.clip(np.arange(rs.start, rs.stop) - dy, 0, SHEET_H - 1)
        xs = np.clip(np.arange(cs.start, cs.stop) - dx, 0, SHEET_W - 1)
        empty = tpl[ys][:, xs].max(axis=2) < 30
        painted = got[rs, cs].max(axis=2) > 60
        if (empty & painted).mean() > 0.02:
            notes.append({"face": "the face", "hair": "the hair", "helm": "the helm", "cape": "the cape",
                          "weapon": "the weapon", "extra": "the extra"}.get(kind, "the outfit") + " runs off the %s of the cell and is cut there" % side)
    return notes


#: What a fitted cell is matched on: windows of the base's canvas the part leaves alone, by kind.
#: head: the bald head and round it; near_fist, far_fist: the hands; face_front: the face's profile,
#: forehead to chin (a fringe may cover some); forehead: the brow and the front of the face above
#: the nose (a horn, an ear or a beard is elsewhere); chest: the chest and the front of the body
#: under the shoulders (long hair or a long beard covers some).
FIT_WINDOWS = {"hair": ("face_front", "chest"), "extra": ("forehead", "chest"), "face": ("chest",),
               "helm": ("chest",), "armor": ("head", "near_fist", "far_fist"),
               "cape": ("head", "near_fist", "far_fist"), "weapon": ("head",)}
#: A fitted cell scoring under this (normalised cross-correlation, 1 the best) is reported as poor.
FIT_POOR = 0.75
#: `check`: a cell drawn more than CHECK_SIZE bigger or smaller than the template's, or CHECK_PLACE
#: template pixels from its place, is put back by the fit; more than CHECK_REDO_SIZE, it's redone.
CHECK_SIZE, CHECK_PLACE, CHECK_REDO_SIZE = 0.02, 8.0, 0.15
#: A cell whose scale is further than this (a share of it) from its row's is fitted again near the row's.
FIT_ODD = 0.025
#: Where a cell differs from the base drawn at its first fit by more than this (lightness), grown by
#: FIT_COVER, its part covers the base (occluded).
FIT_OCCLUDED, FIT_COVER = 40.0, 4
#: A cell led off at first that's found again further than this from its row's scale has its scale
#: held at the row's (only its place found).
FIT_SAME = 0.012
#: How far in from a fitted cell's edges its pixels count, in the cell's own pixels.
FIT_SPARE = 3
#: A head part fitted from a sheet keeps only itself (own_part, which says how these are used).
FIT_LINE, FIT_SPECK, FIT_THICK = 35.0, 8, 2
#: Within FIT_EDGE of the base's lines and edges, a fitted pixel is changed only if its colour isn't
#: within FIT_DRIFT pixels in the base drawn the same way (changed_only).
FIT_EDGE, FIT_DRIFT = 6, 3
#: A part's soft edge is what round it is at least FIT_SOFT darker than the base; skin is the part's
#: only within FIT_BODY of its own colours.
FIT_SOFT, FIT_BODY = 8.0, 4
#: Skin is at least FIT_SKIN_LUM bright and its red over its blue by FIT_WARM; a run of ChatGPT's
#: head outline drawn off the base's lies within FIT_GAP of its edge (head_part).
FIT_SKIN_LUM, FIT_WARM, FIT_GAP = 115.0, 40, 8
#: A pixel of ChatGPT's bigger head is the part's if the part's body is within FIT_REACH of it, off the head.
FIT_REACH = 10
#: A hair's pale highlight looks like skin. Off the head, a patch of it (FIT_SHINE pixels or more)
#: whose edge (FIT_SHINE_EDGE pixels round it) is FIT_SHINE_RING the hair's own, with almost none
#: of it (FIT_SHINE_OPEN) on the black or the bare head, is the hair's shine, kept as painted, not
#: ChatGPT's head drawn bigger.
FIT_SHINE, FIT_SHINE_EDGE, FIT_SHINE_RING, FIT_SHINE_OPEN = 15, 2, 0.8, 0.05
#: A run of line along the base's edge this long or longer, with no body beside it, is the head's outline drawn off.
FIT_LONG = 24
#: A thin piece with no solid body, FIT_ALONG_SHARE of it within FIT_ALONG_LINE of the base's
#: outline, is that outline drawn a little off.
FIT_ALONG_LINE, FIT_ALONG_SHARE = 4, 0.7
#: A hairstyle's face and ear, with nothing of its own on them: below FIT_EYES of the head box down,
#: in front of FIT_EAR of it across (the back of the ear).
FIT_EYES, FIT_EAR = 0.5, 0.08
#: ... and its jaw and chin, below FIT_JAW of the head box down; jewellery's, marks', ears' and horns'
#: too, in front of FIT_CHEEK of it across (an earring hangs behind that).
FIT_JAW, FIT_CHEEK = 0.75, 0.4
#: The skin's own hue, and the browns its outline blends through: green over red, then blue over red,
#: from and to (silver is greyer, gold yellower).
FIT_HUE = (0.45, 0.76, 0.2, 0.62)
#: Darker than this (its brightest channel) is the sheet's black background.
FIT_BLACK = 16


def fit_windows(kind, base_name, names=None):
    """The weight a fitted cell is matched with over the base's canvas: 1 in the kind's FIT_WINDOWS
    (or the windows `names`)."""
    import import_parts as ip
    b = ip.Base(base_name)
    hx, hy, hw, hh = b.head
    shoulders = ip.SHOULDERS[base_name]
    near, far = ip.NEAR_HAND[base_name], ip.FAR_HAND[base_name]
    boxes = {"head": (hy - 10, hy + hh + 6, hx - 10, hx + hw + 10),
             "near_fist": (near[1], near[3], near[0], near[2]),
             "far_fist": (far[1], far[3], far[0], far[2]),
             "face_front": FACE_FRONT[base_name],
             "forehead": (hy + 0.12 * hh, hy + 0.55 * hh, hx + 0.45 * hw, hx + hw + 14),
             "chest": (shoulders - 20, shoulders + 200, hx + 0.1 * hw, hx + hw + 50)}
    weight = np.zeros(base(base_name).shape[:2])
    for name in names or FIT_WINDOWS[kind]:
        r0, r1, c0, c1 = (int(round(v)) for v in boxes[name])
        weight[max(0, r0):r1, max(0, c0):c1] = 1.0
    return weight


def occluded(cell, b, s, tx, ty):
    """Where on the base's canvas a cell's part covers the base, by its first fit (s, tx, ty): where
    the cell differs from the base drawn there by more than FIT_OCCLUDED, grown by FIT_COVER."""
    ch, cw = cell.shape[:2]
    drawn = resample_rgb(b, (-tx / s, -ty / s, cw / s, ch / s), (cw, ch))
    differs = np.abs(sheet_fit.luma(drawn) - sheet_fit.luma(cell)) > FIT_OCCLUDED
    differs = ic.grow(differs, FIT_COVER)
    h, w = b.shape[:2]
    cx = np.rint(tx + s * np.arange(w)).astype(int)
    cy = np.rint(ty + s * np.arange(h)).astype(int)
    ix, iy = np.nonzero((cx >= 0) & (cx < cw))[0], np.nonzero((cy >= 0) & (cy < ch))[0]
    out = np.zeros((h, w), bool)
    out[np.ix_(iy, ix)] = differs[np.ix_(cy[iy], cx[ix])]
    return out


def changed_only(out, b, cell_size_, s, tx, ty):
    """A fitted cell put back on the canvas (out), kept only where it differs from the base drawn
    into the cell the same way and put back again (ref: so the softening cancels), and only inside
    what the cell covers (FIT_SPARE in from its edges, where the resampling runs out); the base's own
    pixel everywhere else. Returns (the part on the canvas, what was kept).

    ChatGPT redraws the base as it paints, a pixel or a few off along its outlines and the edge of
    its figure: within FIT_EDGE of those, a pixel counts as changed only if no pixel of ref within
    FIT_DRIFT has its colour. Elsewhere any difference past GEAR_CHANGED counts, and so does anything
    painted on the base's black however dark (a part's outline is near black, and dropped there it
    would be keyed out with the background and leave the part's edge ragged). Specks are opened
    away and what's kept grown a little, for its soft edge."""
    h, w = b.shape[:2]
    cw, ch = cell_size_
    drawn = resample_rgb(b, (-tx / s, -ty / s, cw / s, ch / s), (cw, ch))
    ref = resample_rgb(drawn, (tx, ty, w * s, h * s), (w, h))
    covered = sheet_fit.footprint((h, w), cell_size_, s, tx, ty, FIT_SPARE)
    o, r = out.astype(np.int16), ref.astype(np.int16)
    plain = np.abs(o - r).max(axis=2)
    lum = r.astype(np.float64) @ np.array([0.299, 0.587, 0.114])
    figure = r.max(axis=2) >= FIT_BLACK
    lines = (lum < FIT_LINE) & figure
    near = ic.grow(lines | (ic.grow(figure, 1) & ic.grow(~figure, 1)), FIT_EDGE)
    drift = plain.copy()
    pad = np.pad(r, ((FIT_DRIFT, FIT_DRIFT), (FIT_DRIFT, FIT_DRIFT), (0, 0)), mode="edge")
    for dy in range(-FIT_DRIFT, FIT_DRIFT + 1):
        for dx in range(-FIT_DRIFT, FIT_DRIFT + 1):
            if dy * dy + dx * dx > FIT_DRIFT * FIT_DRIFT:
                continue
            moved = pad[FIT_DRIFT + dy:FIT_DRIFT + dy + h, FIT_DRIFT + dx:FIT_DRIFT + dx + w]
            drift = np.minimum(drift, np.abs(o - moved).max(axis=2))
    difference = np.where(near, drift, plain)
    on_black = ~ic.grow(figure, FIT_DRIFT) & (out.max(axis=2) >= FIT_BLACK)
    changed = ((difference > GEAR_CHANGED) | on_black) & covered
    changed = ic.grow(~ic.grow(~changed, 1), 1)
    # The part's soft edge: what's round it, so long as it's darker than the base there (an
    # outline blending into what's under it), not lighter (ChatGPT's rim light on the skin).
    darker = (out.astype(np.float64) @ np.array([0.299, 0.587, 0.114])) < lum - FIT_SOFT
    changed = changed | (ic.grow(changed, 2) & covered & (plain > GEAR_CHANGED // 2) & (darker | on_black))
    return np.where(changed[:, :, None], out, b), changed


def fit_cut(png, sheet_id, force=False, check=None, record=None, write=True):
    """`cut` for a sheet laid out ChatGPT's own way: each cell found on its base (sheet_fit). With
    write False, nothing's written: how each cell sits against the template is reported, and the
    verdict on the sheet returned (GOOD: cut it; REDO: ask ChatGPT for it again)."""
    import hashlib
    import json
    digest = hashlib.sha256(open(png, "rb").read()).hexdigest()
    records = json.load(open(record, encoding="utf-8")) if record and os.path.exists(record) else {}
    known = records.get(sheet_id, {}).get("cells", {}) if records.get(sheet_id, {}).get("sha256") == digest else {}
    table = sheets_table()
    if sheet_id not in table:
        sys.exit("part_sheet: %s isn't a sheet in Story/13 (python tools/part_sheet.py list)" % sheet_id)
    kind = kind_of(sheet_id)
    k_ = KINDS[kind]
    base_name, _, ids = table[sheet_id]
    got = load(png)
    redo, fixed = [], []
    if got.shape[:2] != (SHEET_H, SHEET_W):
        fixed.append("it came back %dx%d, not %dx%d" % (got.shape[1], got.shape[0], SHEET_W, SHEET_H))
    boxes = sheet_fit.cell_boxes(got, k_["cols"], k_["rows"])
    folder = os.path.join(CHARS, k_["folder"])
    if write:
        os.makedirs(folder, exist_ok=True)
    report, shown, fits = [], [], []
    windows = {}
    placed = []
    for k, cell_base, part in cells(kind, base_name, ids):
        x0, y0, x1, y1 = boxes[k]
        b = base(cell_base, kind)
        if cell_base not in windows:
            windows[cell_base] = fit_windows(kind, cell_base)
        cell = got[y0:y1, x0:x1]
        found = [f[4] for f in fits if f[1] == cell_base]
        if str(k) in known:
            score, s, tx, ty = known[str(k)]
        else:
            score, s, tx, ty = sheet_fit.fit(cell, np.ones(cell.shape[:2]), b, windows[cell_base],
                                             near=float(np.median(found)) if found else None)
        fits.append((k, cell_base, part, score, s, tx, ty))
    # Each cell found again, matched on its windows less what its part covers there (occluded), so
    # the part doesn't pull the fit: within FIT_ODD of its first scale, or of its row's where the
    # first is far from the rest or matched poorly (long hair or a braid over the chest led it off;
    # a row's cells are the same bust at much the same size).
    firsts = list(fits)
    for i, (k, cell_base, part, score, s, tx, ty) in enumerate(firsts):
        if str(k) in known:
            continue
        middle = float(np.median([f[4] for f in firsts if f[1] == cell_base]))
        centre = middle if abs(s - middle) > FIT_ODD * middle or score < FIT_POOR else s
        x0, y0, x1, y1 = boxes[k]
        cell = got[y0:y1, x0:x1]
        b = base(cell_base, kind)
        weight = windows[cell_base] * ~occluded(cell, b, s, tx, ty)
        again = sheet_fit.fit(cell, np.ones(cell.shape[:2]), b, weight, near=centre,
                              span=FIT_ODD * centre, strict=True)
        if abs(again[1] - middle) > (FIT_SAME if centre == middle else FIT_ODD) * middle:
            # Still far from its row (FIT_SAME, for one led off at first): what's left uncovered (a
            # bob hides the face's profile) can't tell the scale, so it's the row's, and only the
            # place is found.
            again = sheet_fit.fit(cell, np.ones(cell.shape[:2]), b, weight, near=middle, fixed=True)
        fits[i] = (k, cell_base, part) + tuple(again)
    for k, cell_base, part, score, s, tx, ty in fits:
        x0, y0, x1, y1 = boxes[k]
        b = base(cell_base, kind)
        h, w = b.shape[:2]
        mine = np.zeros(got.shape[:2])
        mine[y0:y1, x0:x1] = 1.0
        out = resample_rgb(got, (x0 + tx, y0 + ty, w * s, h * s), (w, h), weight=mine)
        ref, covered = drawn_back(b, (x1 - x0, y1 - y0), s, tx, ty)
        placed.append((k, cell_base, part, b, out, ref, covered, (x0, y0, x1, y1), score, s, tx, ty))
    for k, cell_base, part, b, out, ref, covered, (x0, y0, x1, y1), score, s, tx, ty in placed:
        if kind in ("hair", "extra"):
            out, kept = head_part(out, b, ref, covered, cell_base, part)
        elif kind in ("cape", "weapon"):
            out, kept = changed_only(out, b, (x1 - x0, y1 - y0), s, tx, ty)
        else:
            kept = out.max(axis=2) > 60
        notes = fit_spills(kept, sheet_fit.footprint(kept.shape, (x1 - x0, y1 - y0), s, tx, ty, FIT_SPARE + 3), kind)
        if score < FIT_POOR:
            notes.insert(0, "POOR FIT (%.2f): check it, or make it on its own" % score)
            redo.append("cell %d (%s) isn't the template's figure (fit %.2f)" % (k + 1, part, score))
        # Against the template: the cell's figure at the template's scale (the sheet's own size
        # allowed for) and in its place, in the template's pixels.
        rx, ry = got.shape[1] / float(SHEET_W), got.shape[0] / float(SHEET_H)
        ratio = float(np.sqrt(rx * ry))
        ox, oy = k_["origin"][cell_base]
        cw_, ch_ = cell_size(kind)
        cx_, cy_ = cell_box(kind, k)
        # The canvas point at the template cell's middle: where the template has it on the sheet,
        # and where ChatGPT drew it.
        mx, my = (cw_ / 2.0 + ox) / k_["scale"], (ch_ / 2.0 + oy) / k_["scale"]
        moved = float(np.hypot(x0 + tx + s * mx - (cx_ + cw_ / 2.0) * rx, y0 + ty + s * my - (cy_ + ch_ / 2.0) * ry)) / ratio
        grown = s / (k_["scale"] * ratio) - 1.0
        notes.append("as drawn %+.1f%% in size, %.0f pixels from its place" % (grown * 100, moved))
        if abs(grown) > CHECK_REDO_SIZE:
            redo.append("cell %d (%s) was drawn %+.0f%% in size" % (k + 1, part, grown * 100))
        elif abs(grown) > CHECK_SIZE or moved > CHECK_PLACE:
            fixed.append("cell %d drawn %+.1f%% in size, %.0f pixels off" % (k + 1, grown * 100, moved))
        own = [n for n in notes if "runs off" in n and (kind not in ("face", "helm") or "top" in n)]
        if own:
            redo.append("cell %d (%s) %s" % (k + 1, part, own[0]))
        if not write:
            report.append("  cell %d %s: scale %.4f, at %+.1f, %+.1f, fit %.3f%s" % (k + 1, part, s, tx, ty, score,
                                                                                ("; " + "; ".join(notes)) if notes else ""))
            continue
        dest = os.path.join(folder, part + ".png")
        if os.path.exists(dest) and not force:
            report.append("  cell %d %s: kept the one already there (--force to replace it)" % (k + 1, part))
        else:
            save(dest, out)
            report.append("  cell %d %s: scale %.4f, at %+.1f, %+.1f, fit %.3f%s" % (k + 1, part, s, tx, ty, score,
                                                                                ("; " + "; ".join(notes)) if notes else ""))
        if check:
            shown.append(fit_picture(out, b, kind))
    # A row's cells are laid out alike: one far from the rest has likely been found in the wrong place.
    for row in sorted({f[1] for f in fits}):
        scales = np.array([f[4] for f in fits if f[1] == row])
        middle = float(np.median(scales))
        for f in fits:
            if f[1] == row and abs(f[4] - middle) > FIT_ODD * middle:
                report.append("  cell %d %s: ODD SCALE %.3f against its row's %.3f; check it" % (f[0] + 1, f[2], f[4], middle))
                redo.append("cell %d (%s) is at a different size from the rest of its row" % (f[0] + 1, f[2]))
    verdict = ("REDO: " + "; ".join(redo)) if redo else ("GOOD: " + ("; ".join(fixed) + ", which the fit puts back" if fixed else "the template's size and layout"))
    if not write:
        print("part_sheet: checked %s (%s, %d %ss, %dx%d) against its template"
              % (sheet_id, base_name, len(fits), kind, got.shape[1], got.shape[0]))
        print("\n".join(report))
        print(verdict)
        return verdict
    if record:
        records[sheet_id] = {"sha256": digest, "size": [got.shape[1], got.shape[0]],
                             "cells": {str(f[0]): [round(f[3], 4), round(f[4], 6), round(f[5], 3), round(f[6], 3)] for f in fits}}
        with open(record, "w", encoding="utf-8", newline="\n") as f:
            json.dump(records, f, indent=1)
    os.makedirs(SHEETS, exist_ok=True)
    kept = os.path.join(SHEETS, sheet_id + ".png")
    if os.path.abspath(png) != os.path.abspath(kept):
        shutil.copyfile(png, kept)
    write_assets(table)
    if check and shown:
        tall = max(p.shape[0] for p in shown)
        save(check, np.concatenate([np.pad(p, ((0, tall - p.shape[0]), (0, 4), (0, 0))) for p in shown], axis=1))
    print("part_sheet: fitted %s (%s, %d %ss, %dx%d) into tools/source/characters/%s/"
          % (sheet_id, base_name, len(fits), kind, got.shape[1], got.shape[0], k_["folder"]))
    print("\n".join(report))
    print(verdict)
    return verdict


def drawn_back(b, cell_size_, s, tx, ty):
    """The base drawn into a fitted cell and put back the same way (the same softening), and what
    the cell covers (FIT_SPARE in from its edges)."""
    h, w = b.shape[:2]
    cw, ch = cell_size_
    drawn = resample_rgb(b, (-tx / s, -ty / s, cw / s, ch / s), (cw, ch))
    return resample_rgb(drawn, (tx, ty, w * s, h * s), (w, h)), sheet_fit.footprint((h, w), cell_size_, s, tx, ty, FIT_SPARE)


def _drift(a, b, reach):
    """Each pixel's nearest colour match (largest channel difference) in b within `reach` pixels."""
    h, w = a.shape[:2]
    x, y = a.astype(np.int16), np.pad(b.astype(np.int16), ((reach, reach), (reach, reach), (0, 0)), mode="edge")
    best = np.full((h, w), 999, np.int16)
    for dy in range(-reach, reach + 1):
        for dx in range(-reach, reach + 1):
            if dy * dy + dx * dx <= reach * reach:
                best = np.minimum(best, np.abs(x - y[reach + dy:reach + dy + h, reach + dx:reach + dx + w]).max(axis=2))
    return best


def shine(labels, count, sizes, own, open_):
    """Which of the pale pieces (labels, 1 to count) are a hair's shine: FIT_SHINE pixels or more,
    their edge (FIT_SHINE_EDGE round them) at least FIT_SHINE_RING the hair's own (`own`), at most
    FIT_SHINE_OPEN on the black or the bare head (`open_`). A bool per label."""
    out = np.zeros(count + 1, bool)
    h, w = labels.shape
    r = FIT_SHINE_EDGE
    for lab_ in np.nonzero(sizes >= FIT_SHINE)[0]:
        if lab_ == 0:
            continue
        ys, xs = np.nonzero(labels == lab_)
        y0, y1, x0, x1 = max(0, ys.min() - r), min(h, ys.max() + r + 1), max(0, xs.min() - r), min(w, xs.max() + r + 1)
        piece = labels[y0:y1, x0:x1] == lab_
        ring = ic.grow(piece, r) & ~piece & (labels[y0:y1, x0:x1] != lab_)
        if not ring.any():
            continue
        out[lab_] = (own[y0:y1, x0:x1][ring].mean() >= FIT_SHINE_RING
                     and open_[y0:y1, x0:x1][ring].mean() <= FIT_SHINE_OPEN)
    return out


def head_part(out, b, ref, covered, base_name, part):
    """A fitted hair or extra, sorted pixel by pixel against the base drawn the same way (ref):
    (the part on the canvas, kept). A pixel is new where it differs from ref past GEAR_CHANGED, or is
    painted at all on ref's black (an outline is near black). Then, of what's new:

      - body, the part's own colours (not black, not a line darker than FIT_LINE, not skin: warm
        and at least FIT_SKIN_LUM bright, red over green over blue by FIT_WARM, as the base's skin
        is lit or shining and every hair, beard and item is painted darker or another colour), is
        kept, all of it. A skin mark (skin-*) is skin by nature, so for one nothing counts as skin;
      - a line or skin within FIT_EDGE of ref's lines and edges, whose colour is in ref within
        FIT_DRIFT pixels, and further than FIT_BODY from body, is the base drawn a few pixels off:
        it goes (a hair's own dark strands touch its body, so they stay);
      - skin on the base's head that touches the base's bare skin is the base showing (a scalp, its
        shine, a face): it goes; skin the part closes round (a highlight) stays;
      - skin off the base's head is ChatGPT's head drawn bigger (for anything but a pair of ears,
        whose skin it is): each pixel of it with the part's solid body (FIT_THICK; an outline's
        soft edge is thin) just beyond it, off the head (within FIT_REACH), is the gap between the
        face and the part, and is filled with the part's own nearest colour, so the part meets the
        face with no gap and no pale strip; the rest of it goes, and so does a run of line along
        the base's edge, FIT_LONG or longer, with no body within FIT_BODY (the head's outline,
        drawn off);
      - a hairstyle has nothing on the face below the eyes or on the ear (FIT_EYES, FIT_EAR), or
        just off them, but solid hair coming down from above the eyes (a fringe, hair over the ear,
        the far side of a bob): ChatGPT shades the jaw and the ear in the hair's own hue, so only
        where it is tells; below FIT_JAW, the jaw and chin, only solid hair of the hair's colour
        from above, and none of ChatGPT's wider head filled in;
      - jewellery, marks, ears and horns have nothing on the jaw and chin in front of FIT_CHEEK
        but metal and stones (a lip ring): not the skin's hue (FIT_HUE), a line or skin;
      - specks under FIT_SPECK go, pieces with fewer than FIT_SPECK pixels of body (a run of
        outline, a shadow ChatGPT drew darker; a wisp of hair is thin, but the hair's colour), and
        thin pieces with no solid body lying along the base's outline (FIT_ALONG_SHARE of them
        within FIT_ALONG_LINE), and every pixel within FIT_ALONG_LINE of the outline and further than
        FIT_BODY from solid body, as such a run can join the part: the outline drawn off, its soft
        edge the part's colour;
        under the jaw seam only solid pieces (FIT_THICK) with body in
        them that grow from the head stay (a beard, long hair, an earring), not a neck or a chest;
      - round what's kept, its soft edge: what's darker than the base there, or on the black, and
        not skin.
    The base's own pixel is everywhere else, and only what the cell covers counts."""
    import import_parts as ip
    h, w = b.shape[:2]
    weights = np.array([0.299, 0.587, 0.114])
    o, r = out.astype(np.int16), ref.astype(np.int16)
    plain = np.abs(o - r).max(axis=2)
    lum_o, lum_r = out.astype(np.float64) @ weights, ref.astype(np.float64) @ weights
    figure = ref.max(axis=2) >= FIT_BLACK
    black = out.max(axis=2) < FIT_BLACK
    line = (lum_o < FIT_LINE) & ~black
    rgb = out.astype(np.int16)
    skin = ((lum_o >= FIT_SKIN_LUM) & (rgb[..., 0] >= rgb[..., 1]) & (rgb[..., 1] >= rgb[..., 2])
            & (rgb[..., 0] - rgb[..., 2] >= FIT_WARM) & ~line & ~black)
    if part.startswith("skin-"):
        skin[:] = False
    on_black = ~ic.grow(figure, FIT_DRIFT) & ~black
    new = ((plain > GEAR_CHANGED) | on_black) & covered
    body = new & ~black & ~line & ~skin
    by_body = ic.grow(body, FIT_BODY)
    near = ic.grow(((lum_r < FIT_LINE) & figure) | (ic.grow(figure, 1) & ic.grow(~figure, 1)), FIT_EDGE)
    offset = new & near & (_drift(out, ref, FIT_DRIFT) <= GEAR_CHANGED) & ~by_body & (line | skin | black)
    kept = new & ~offset
    kept = ic.grow(~ic.grow(~kept, 1), 1) & covered
    paint = out.copy()
    # Skin and stray outline in what's kept: the base showing, a highlight, ChatGPT's head drawn
    # bigger than the base's (along its edge: filled with the part where the part runs beside it,
    # gone where the background does), or an ear.
    bare = figure & ~kept
    band = ic.grow(figure, FIT_GAP) & ic.grow(~figure, FIT_GAP)
    # The part's solid body: an outline's soft edge is a part's colour too, but thin.
    solid_body = ic.grow(~ic.grow(~(body & kept), FIT_THICK), FIT_THICK) & body & kept
    beside = ic.grow(solid_body, FIT_BODY)
    strip = np.zeros((h, w), bool)
    labels, count = ic.components(kept & skin)
    if count:
        sizes = np.maximum(np.bincount(labels.ravel(), minlength=count + 1), 1)
        on_head = np.bincount(labels.ravel(), weights=figure.ravel().astype(np.float64), minlength=count + 1) > 0.5 * sizes
        touches = np.zeros(count + 1, bool)
        touches[np.unique(labels[ic.grow(bare, 1) & (labels > 0)])] = True
        # The base showing: skin on its head that touches its bare skin.
        showing = touches & on_head
        # ChatGPT's head drawn bigger, off the base's (by any amount: a bald dome drawn taller than
        # the base's; not by touching it, as its inner edge, the base's own drawn a pixel or two
        # off, has gone already): the part's where its body lies just beyond, off the head
        # (FIT_REACH), and gone everywhere else. A pair of ears is the one part that is skin off the head.
        bigger = ~on_head & (not part.startswith("ears-"))
        if part.startswith("hair-"):
            # A hair's shine: pale, but the hair all round it and nothing open beside it.
            bigger &= ~shine(labels, count, sizes, kept & ~skin, black | bare)
        showing[0] = bigger[0] = False
        bigger_px = bigger[labels]
        strip = bigger_px & ic.grow(solid_body & ~figure, FIT_REACH)
        kept &= ~showing[labels] & ~(bigger_px & ~strip)
    # ChatGPT's head outline, drawn off the base's: long runs of line along its edge with no body
    # beside them (a strap crossing the edge is short, so it stays).
    runs, n = ic.components(kept & line & band & ~beside)
    if n:
        ys_, xs_ = np.nonzero(runs)
        ids = runs[ys_, xs_]
        top, bottom = np.full(n + 1, 1 << 30), np.full(n + 1, -1)
        left, right = np.full(n + 1, 1 << 30), np.full(n + 1, -1)
        np.minimum.at(top, ids, ys_)
        np.maximum.at(bottom, ids, ys_)
        np.minimum.at(left, ids, xs_)
        np.maximum.at(right, ids, xs_)
        long_ = np.maximum(bottom - top, right - left) >= FIT_LONG
        long_[0] = False
        kept &= ~long_[runs]
    if strip.any():
        source = solid_body & kept & ~strip
        colour = paint.astype(np.float64)
        have, todo = source.copy(), strip.copy()
        for _ in range(64):
            if not todo.any():
                break
            mean = ic.local_mean(np.where(have[..., None], colour, 0.0), have.astype(np.float64), (1,))
            step = todo & ~np.isnan(mean[..., 0])
            if not step.any():
                break
            colour[step] = mean[step]
            have |= step
            todo &= ~step
        paint = np.rint(colour).astype(np.uint8)
        kept &= ~todo
    # Specks, and pieces with none of the part's own colour in them (a run of outline, a shadow
    # ChatGPT drew darker than the base's): a wisp of hair is thin but is the hair's colour.
    # And thin pieces with no solid body that lie along the base's outline nearly all their length:
    # its outline drawn a little off, whose soft edge is a part's colour (a wisp of hair runs off
    # across the scalp, so it stays).
    outline_near = ic.grow(((lum_r < FIT_LINE) & figure) | (ic.grow(figure, 1) & ic.grow(~figure, 1)), FIT_ALONG_LINE)
    solid_now = ic.grow(~ic.grow(~(body & kept), FIT_THICK), FIT_THICK) & body & kept
    # Pixel by pixel too, as such a run often joins the part (along the profile up to the hair):
    # what's thin, away from the part's solid body, and on the base's outline.
    kept &= ~(outline_near & ~ic.grow(solid_now, FIT_BODY))
    labels, count = ic.components(kept)
    if count:
        def per(mask):
            return np.bincount(labels.ravel(), weights=mask.ravel().astype(np.float64), minlength=count + 1)

        sizes = np.maximum(np.bincount(labels.ravel(), minlength=count + 1), 1)
        coloured, solid_px, along = per(body), per(solid_now), per(outline_near) / sizes
        small = (sizes < FIT_SPECK) | (coloured < FIT_SPECK) | ((solid_px < FIT_SPECK) & (along >= FIT_ALONG_SHARE))
        small[0] = False
        kept &= ~small[labels]
    geometry = ip.Base(base_name)
    above = geometry.above
    if part.startswith("hair-"):
        # A hairstyle has nothing of its own on the face below the eyes or on the ear (nor just off
        # them, where ChatGPT draws the jaw a little wider): only hair coming down from above the
        # eyes, solid (a fringe, hair over the ear, the far side of a bob). ChatGPT shades the jaw
        # and the ear in the hair's own hue, so a shadow there can't be told from hair by its
        # colour, only by where it is.
        hx, hy, hw, hh = geometry.head
        eyes = hy + FIT_EYES * hh
        across, down = np.arange(w)[None, :], np.arange(h)[:, None]
        face = (down > eyes) & (across > hx + FIT_EAR * hw) & ic.grow(figure & above, FIT_GAP * 2)
        # The hair's mass: anything kept that's thick and not skin (its shadowed side can be as
        # dark as a line, but a line is thin).
        hairy = (body | strip | line) & kept & ~(skin & ~strip)
        mass = ic.grow(~ic.grow(~hairy, FIT_THICK), FIT_THICK) & hairy
        labels, count = ic.components(mass)
        from_above = np.zeros(count + 1, bool)
        from_above[np.unique(labels[(down < eyes) & (labels > 0)])] = True
        from_above[0] = False
        hanging_hair = ic.grow(from_above[labels], FIT_BODY) & kept
        kept &= ~(face & ~hanging_hair)
        # The jaw and chin, below the mouth: hair hangs past them, never on them, so nothing of
        # ChatGPT's wider head is filled there, and only solid hair of the hair's own colour (not
        # a jaw's shadow, as dark as a line) that comes from above stays.
        jaw = face & (down > hy + FIT_JAW * hh)
        coloured = ic.grow(~ic.grow(~(body & kept), FIT_THICK), FIT_THICK) & body & kept
        labels, count = ic.components(coloured)
        from_above = np.zeros(count + 1, bool)
        from_above[np.unique(labels[(down < eyes) & (labels > 0)])] = True
        from_above[0] = False
        kept &= ~(jaw & ~(ic.grow(from_above[labels], FIT_BODY) & ~strip))
    if part.startswith(("jewel-", "skin-", "ears-", "horns-")):
        # On the jaw and the chin in front of the ear (an earring hangs behind it), jewellery, marks,
        # ears and horns have only metal and stones (a lip ring, a stud): what's there in the skin's
        # own hue (ChatGPT's jaw drawn a little off, its outline blended into the skin), a line or
        # skin goes, but a ring's outline beside its metal stays.
        hx, hy, hw, hh = geometry.head
        across, down = np.arange(w)[None, :], np.arange(h)[:, None]
        jaw = (down > hy + FIT_JAW * hh) & (across > hx + FIT_CHEEK * hw) & ic.grow(figure & above, FIT_GAP * 2)
        c = out.astype(np.float64)
        red = np.maximum(c[..., 0], 1.0)
        hued = ((c[..., 0] >= c[..., 1]) & (c[..., 1] >= c[..., 2]) & (c[..., 1] / red >= FIT_HUE[0]) & (c[..., 1] / red <= FIT_HUE[1])
                & (c[..., 2] / red >= FIT_HUE[2]) & (c[..., 2] / red <= FIT_HUE[3]))
        metal = kept & ~hued & ~line & ~skin & ~black
        kept &= ~(jaw & (hued | skin | (line & ~ic.grow(metal, 2))))
    head = ic.grow(above & figure, 4)
    solid = ic.grow(~ic.grow(~kept, FIT_THICK), FIT_THICK) & kept
    labels, count = ic.components(solid)
    reach = np.zeros(count + 1, bool)
    reach[np.unique(labels[head & solid])] = True
    reach &= np.bincount(labels.ravel(), weights=(body & solid).ravel().astype(np.float64), minlength=count + 1) >= FIT_SPECK
    reach[0] = False
    hanging = reach[labels]
    for _ in range(FIT_THICK + 1):
        hanging = ic.grow(hanging, 1) & kept
    kept = (kept & above) | hanging
    darker = lum_o < lum_r - FIT_SOFT
    kept = kept | (ic.grow(kept, 2) & covered & (plain > GEAR_CHANGED // 2) & (darker | on_black) & ~black & ~skin)
    return np.where(kept[:, :, None], paint, b), kept


def fit_spills(kept, covered, kind):
    """Where the part itself runs off its fitted cell and is cut there: what was kept, on the edge of
    what the cell covers (a bust's own arm and chest always run off it, but they're not kept)."""
    edge = ic.grow(~covered, 1) & covered
    rows, cols = np.nonzero(edge & kept)
    if len(rows) < 12:
        return []
    ys, xs = np.nonzero(covered)
    sides = []
    for side, near in (("top", rows <= ys.min() + 1), ("bottom", rows >= ys.max() - 1),
                       ("left", cols <= xs.min() + 1), ("right", cols >= xs.max() - 1)):
        if near.sum() >= 12:
            sides.append(side)
    return ["runs off the %s of its cell and is cut there" % side for side in sides]


def fit_picture(out, b, kind):
    """A fitted part put back, cropped round what it shows, with the base's outline over it in cyan:
    where they don't line up, the cyan shows beside the painted outline."""
    lum = out.astype(np.float64) @ np.array([0.299, 0.587, 0.114])
    base_lum = b.astype(np.float64) @ np.array([0.299, 0.587, 0.114])
    lines = (base_lum < 45) & (ic.grow(base_lum > 45, 2))
    shown = out.copy()
    shown[lines] = (0, 255, 255)
    rows, cols = np.nonzero(lum > 20)
    if not len(rows):
        return shown[::2, ::2]
    top = max(0, rows.min() - 10)
    bottom = min(out.shape[0], (rows.max() + 10) if kind in ("armor", "cape", "weapon") else top + 700)
    return shown[top:bottom:2, max(0, cols.min() - 10):cols.max() + 10:2]


def drawn_from(base_name, kind):
    """What a template's cells were drawn from, for ASSETS.md: the base, or till the body's painted,
    its template's own pieces stood up (tools/body_parts.py's stand-in)."""
    if os.path.exists(os.path.join(CHARS, base_file(base_name, kind) + ".png")):
        return "tools/source/characters/%s.png" % base_file(base_name, kind)
    return ("the pieces of tools/source/characters/body-parts-%s.png stood up at rest by tools/body_parts.py"
            % ("man" if base_name == "base-male" else "woman"))


def write_assets(table):
    lines = [A_BEGIN, "## Part sheets", "",
             "Several parts at a time: ChatGPT paints one face, hairstyle, outfit, helm, cape or weapon into each cell of a "
             "sheet made from a base (Story/13-art-prompts.md §4.0, §4.9, §5.0 and §6.0), and `tools/part_sheet.py` puts each "
             "cell back on its base's canvas.", "",
             "tools/part_sheet.py - written by Caleb Kiatoukaysi for Legend Quest, makes the blank face, hair, armor, helm, cape "
             "and weapon sheets from the bases and cuts a painted sheet back into one picture per part, lined up with its base", "",
             "tools/sheet_fit.py - written by Caleb Kiatoukaysi for Legend Quest, finds a base under each cell of a sheet ChatGPT "
             "laid out its own way, at any scale and place (masked cross-correlation, coarse then fine), for part_sheet.py fit", ""]
    for kind in KINDS:
        for base_name in TEMPLATES.get(kind, BASES):
            tid = template_id(kind, base_name)
            what = {"face": "ten copies of its head and shoulders at full size, five across and two down",
                    "hair": "the blank man eight times along the top and the blank woman eight times along the bottom, each "
                            "from the top of the head to the hips at half size",
                    "extra": ("the blank man six times along the top and the blank woman six times along the bottom"
                              if base_name == "both" else "twelve copies of the blank man, six across and two down")
                             + ", each the head, neck and chest at %g scale" % EXTRA_SCALE,
                    "helm": "four copies of it from the top of the canvas, room for a crown, to the hips, at full size, side by side",
                    "weapon": "four copies of its two fists, each with a stub of forearm, where they are on the figure, at %g scale, "
                              "side by side, the rest of the figure left out" % GEAR_SCALE}.get(
                kind, "four copies of the whole figure at %g scale, side by side" % KINDS[kind]["scale"])
            if not os.path.exists(os.path.join(CHARS, tid + ".png")):
                continue
            source = " and ".join(drawn_from(b, kind) for b in (PAIR if base_name == "both" else (base_name,)))
            lines += ["tools/source/characters/%s.png - the blank %s sheet: %s, from %s, with grey "
                      "lines between them; built by tools/part_sheet.py" % (tid, kind, what, source), ""]
    for sid in sorted(table):
        if os.path.exists(os.path.join(SHEETS, sid + ".png")):
            kind = kind_of(sid)
            lines += ["tools/source/characters/sheets/%s.png - %s sheet %s (%s), painted on tools/source/characters/%s.png; %s"
                      % (sid, kind, sid, ", ".join(table[sid][2]), template_id(kind, table[sid][0]), CREDIT), ""]
    for sid in sorted(table):
        kind = kind_of(sid)
        folder = KINDS[kind]["folder"]
        for k, _, part in cells(kind, table[sid][0], table[sid][2]):
            if os.path.exists(os.path.join(CHARS, folder, part + ".png")):
                lines += ["tools/source/characters/%s/%s.png - %s on its base's canvas, cut by tools/part_sheet.py from cell %d "
                          "of tools/source/characters/sheets/%s.png; %s" % (folder, part, part, k + 1, sid, CREDIT), ""]
    # The faces painted before the head was fixed, heads and hair together, and their sheets.
    for path in sorted(glob.glob(os.path.join(SHEETS, "head-and-hair", "*.png"))):
        lines += ["tools/source/characters/sheets/head-and-hair/%s - a face sheet painted the old way, eight heads with their "
                  "hair on the face sheet as it was then; %s" % (os.path.basename(path), CREDIT), ""]
    for path in sorted(glob.glob(os.path.join(CHARS, "faces", "head-and-hair", "*.png"))):
        part = os.path.basename(path)[:-4]
        lines += ["tools/source/characters/faces/head-and-hair/%s.png - %s's head and hair on its base's canvas, painted the old "
                  "way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; "
                  "tools/import_parts.py splits it into its face and its hair; %s" % (part, part, CREDIT), ""]
    lines.append(A_END)
    raw = open(ASSETS, "rb").read().decode("utf-8")
    nl = "\r\n" if "\r\n" in raw else "\n"
    text = raw.replace("\r\n", "\n").split("\n")
    for begin, end in OLD_SECTIONS:
        if begin in text and end in text:
            i, j = text.index(begin), text.index(end)
            if j + 1 < len(text) and text[j + 1] == "":
                j += 1
            text = text[:i] + text[j + 1:]
    if A_BEGIN in text and A_END in text:
        i, j = text.index(A_BEGIN), text.index(A_END)
        text = text[:i] + lines + text[j + 1:]
    elif A_ANCHOR in text:
        i = text.index(A_ANCHOR)
        text = text[:i] + lines + [""] + text[i:]
    else:
        text = text + [""] + lines
    open(ASSETS, "wb").write(nl.join(text).encode("utf-8"))


def main(argv):
    if not argv or argv[0] not in ("templates", "cut", "fit", "list", "check"):
        sys.exit(__doc__)
    if argv[0] == "templates":
        for kind in KINDS:
            if kind == "armor":
                continue  # painted in pieces now: tools/body_parts.py armor
            for base_name in TEMPLATES.get(kind, BASES):
                tid = template_id(kind, base_name)
                save(os.path.join(CHARS, tid + ".png"), template(kind, base_name))
                print("part_sheet: wrote tools/source/characters/%s.png" % tid)
        write_assets(sheets_table())
    elif argv[0] == "list":
        for sid, (base_name, pri, ids) in sorted(sheets_table().items()):
            print("%s (%s, %s): %s" % (sid, base_name, pri, ", ".join("%d %s" % (k + 1, f) for k, _, f in cells(kind_of(sid), base_name, ids))))
    elif argv[0] == "check":
        args = [a for a in argv[1:] if not a.startswith("--")]
        if len(args) != 2:
            sys.exit("part_sheet: check <png> <sheet-id>")
        verdict = fit_cut(args[0], args[1], write=False)
        sys.exit(1 if verdict.startswith("REDO") else 0)
    elif argv[0] == "fit":
        check = argv[argv.index("--check") + 1] if "--check" in argv else None
        record = argv[argv.index("--record") + 1] if "--record" in argv else None
        args = [a for a in argv[1:] if not a.startswith("--") and a not in (check, record)]
        if len(args) != 2:
            sys.exit("part_sheet: fit <png> <sheet-id> [--force] [--check <png>] [--record <json>]")
        fit_cut(args[0], args[1], force="--force" in argv, check=check, record=record)
    else:
        args = [a for a in argv[1:] if not a.startswith("--")]
        if len(args) != 2:
            sys.exit("part_sheet: cut <png> <sheet-id> [--force]")
        cut(args[0], args[1], force="--force" in argv)


if __name__ == "__main__":
    main(sys.argv[1:])
