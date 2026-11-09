"""
Part sheets: several parts in one ChatGPT picture instead of one picture each.

    python tools/part_sheet.py templates                  # the blank sheets, from the bases
    python tools/part_sheet.py cut <png> <sheet-id> [--force]
    python tools/part_sheet.py list                       # the sheets and their cells, from Story/13 §4.0, §4.9, §5.0 and §6.0

Six kinds, each a 1536x1024 picture of cells with thin grey lines between them, on black:

  - a face sheet (face-sheet-*, Story/13 §4.0): four columns and two rows of 384x512 cells, each the
    same crop of one blank base at full size (its head, neck and shoulders). ChatGPT paints a
    different face, the features only, into each.
  - a hair sheet (hair-sheet-*, §4.9): four columns of 384x1024 cells, each the blank base at full
    size from the head to the hips, room for a long braid. ChatGPT gives each head a hairstyle.
  - an armor sheet (armor-sheet-*, §5.0): four columns of 384x1024 cells, each the whole base at
    ARMOR_SCALE (640 x 960, cropped to the cell round the figure). ChatGPT dresses each figure in a
    different outfit. At that scale a standard adult is still 844 pixels tall, more than the parts
    keep (tools/import_parts.py's PART_SCALE), so nothing's lost against a single edit.
  - a helm sheet (helm-sheet-*, §6.0): four columns of 384x1024 cells, each the blank base at full
    size from the top of the canvas, room over the head for a crown or a plume, to the hips. ChatGPT
    puts a helm, hood, hat or crown on each head.
  - a cape sheet and a weapon sheet (cape-sheet-*, weapon-sheet-*, §6.0): four columns of 384x1024
    cells, each the whole base at GEAR_SCALE, a little smaller than an armor's so there's room
    behind the back for a cloak and over the head for a staff. ChatGPT hangs a cape behind each
    figure, or puts a weapon in each near hand (and, where the item says, something in the far one).
    A standard adult is still 756 pixels tall, more than the parts keep.

`templates` writes the blank sheets, <kind>-sheet-male.png and <kind>-sheet-female.png for each kind,
into tools/source/characters. The face, hair and helm sheets are made from the blank bases
(base-male-blank.png, base-female-blank.png) once they're there, and from the bases till then; run it
again when they come.

`cut` keeps the sheet ChatGPT sent back as tools/source/characters/sheets/<sheet-id>.png, then for
each cell:
  - finds how far ChatGPT shifted it, up to SEARCH pixels each way, by matching edges against the
    template where the part leaves the base alone: the neck and shoulders under a face, the waist
    and arms under a helm, the bald head over an armor, a cape or a weapon. A scaled figure (an
    armor's, a cape's, a weapon's) is matched on its fists or its chest as well (WINDOWS), and on a
    range of scales (SCALES), since ChatGPT sometimes draws the whole figure a little bigger or smaller;
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
    "face": {
        "cols": 4, "rows": 2, "scale": 1.0, "folder": "faces", "section": "§4.0",
        "origin": {"base-male": (330, 36), "base-female": (330, 71)},
        "match": {"base-male": (293 - 36 + 60, 512 - SEARCH - 4, SEARCH + 4, 384 - SEARCH - 4),
                  "base-female": (325 - 71 + 60, 512 - SEARCH - 4, SEARCH + 4, 384 - SEARCH - 4)},
        "poor": 9.0,
    },
    "hair": {
        "cols": 4, "rows": 1, "scale": 1.0, "folder": "hairs", "section": "§4.9",
        "origin": {"base-male": (330, 36), "base-female": (330, 71)},
        # The front of the blank face and the jaw, which a hairstyle leaves alone.
        "match": {"base-male": (142, 277, 230, 345), "base-female": (141, 274, 224, 330)},
        "poor": 9.0,
    },
    "armor": {
        "cols": 4, "rows": 1, "scale": ARMOR_SCALE, "folder": "armors", "section": "§5.0",
        "origin": {"base-male": (128, -32), "base-female": (128, -32)},
        # The bald head, crown to chin, and round it.
        "match": {"base-male": (int(100 * ARMOR_SCALE) + 32, int(293 * ARMOR_SCALE) + 32, int(440 * ARMOR_SCALE) - 128, int(640 * ARMOR_SCALE) - 128),
                  "base-female": (int(135 * ARMOR_SCALE) + 32, int(325 * ARMOR_SCALE) + 32, int(425 * ARMOR_SCALE) - 128, int(625 * ARMOR_SCALE) - 128)},
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
FISTS = {"base-male": ((513, 570, 40, 129), (507, 570, 272, 347)), "base-female": ((513, 570, 56, 138), (507, 570, 272, 337))}
HEAD_MIDDLE = {"base-male": (215.75, 157.0), "base-female": (206.0, 179.0)}
SCALES = [round(0.96 + 0.005 * i, 3) for i in range(17)]
NUDGE = 4

#: The kinds painted on the blank-faced bases: what's on and of the head.
ON_BLANK = ("face", "hair", "helm")

#: The chest, on the canvas (rows, then columns): what a weapon sheet's figure is matched on besides
#: its head, since a hand holds the weapon and a shield or a second blade can cover the far fist.
CHEST = {"base-male": (400, 560, 480, 640), "base-female": (430, 590, 470, 620)}


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


#: What a scaled cell is matched on, the head's window first (the score reported is the head's): an
#: armor's head and fists; a cape leaves the head and both fists alone as well; a weapon the head and
#: the chest. And the head's middle, which ChatGPT's resizing is searched about.
WINDOWS = {"armor": {b: [KINDS["armor"]["match"][b]] + list(FISTS[b]) for b in BASES}}
MIDDLE = {"armor": HEAD_MIDDLE}
for _kind in ("cape", "weapon"):
    WINDOWS[_kind], MIDDLE[_kind] = {}, {}
    for _b in BASES:
        _head = to_cell(_kind, _b, *from_armor(KINDS["armor"]["match"][_b], _b))
        _more = ([to_cell(_kind, _b, *from_armor(f, _b)) for f in FISTS[_b]] if _kind == "cape"
                 else [to_cell(_kind, _b, *CHEST[_b])])
        WINDOWS[_kind][_b] = [_head] + _more
        _ax, _ay = HEAD_MIDDLE[_b]
        _aox, _aoy = KINDS["armor"]["origin"][_b]
        _ox, _oy = KINDS[_kind]["origin"][_b]
        MIDDLE[_kind][_b] = ((_ax + _aox) / ARMOR_SCALE * GEAR_SCALE - _ox, (_ay + _aoy) / ARMOR_SCALE * GEAR_SCALE - _oy)
    KINDS[_kind]["match"] = {b: WINDOWS[_kind][b][0] for b in BASES}

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
    return "%s-sheet-%s" % (kind, base_name.split("-")[1])


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


def base(name, kind=None):
    """A base as RGB; for a face, a hair or a helm, the blank base (its features taken off) if it's there."""
    blank = os.path.join(CHARS, name + "-blank.png")
    if kind in ON_BLANK and os.path.exists(blank):
        return load(blank)
    return load(os.path.join(CHARS, name + ".png"))


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


def cell_of(kind, base_name):
    """One cell of a kind's template: the base cropped (and for an armor, scaled) as the cell holds it."""
    b = base(base_name, kind)
    cw, ch = cell_size(kind)
    s = KINDS[kind]["scale"]
    ox, oy = KINDS[kind]["origin"][base_name]
    if s == 1.0:
        return b[oy:oy + ch, ox:ox + cw]
    # The box of the base's canvas a cell covers, in the canvas's pixels.
    return resample_rgb(b, (ox / s, oy / s, cw / s, ch / s), (cw, ch))


def template(kind, base_name):
    crop = cell_of(kind, base_name)
    cw, ch = cell_size(kind)
    sheet = np.zeros((SHEET_H, SHEET_W, 3), np.uint8)
    for k in range(KINDS[kind]["cols"] * KINDS[kind]["rows"]):
        x, y = cell_box(kind, k)
        sheet[y:y + ch, x:x + cw] = crop
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
    for m in re.finditer(r"^\| `((?:face|hair|armor|helm|cape|weapon)-sheet-[a-z0-9-]+)` \| `(base-[a-z]+)` \| (P\d) \| ([^|]+) \|", text, re.M):
        out[m.group(1)] = (m.group(2), m.group(3), re.findall(r"`((?:face|hair|armor|helm|cape|weapon|offhand)-[a-z0-9.-]+)`", m.group(4)))
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
    b = base(base_name, kind)
    cw, ch = cell_size(kind)
    s = k_["scale"]
    ox, oy = k_["origin"][base_name]
    lines = line_mask(kind, outer=True)
    near = np.zeros_like(lines)  # pixels near a grey line, or the sheet's edge, aren't the sheet's
    for d in range(-KEEP_OFF_LINES, KEEP_OFF_LINES + 1):
        near |= np.roll(lines, d, axis=0) | np.roll(lines, d, axis=1)
    e_got, e_tpl = edges(got), edges(tpl)
    r0, r1, c0, c1 = k_["match"][base_name]
    folder = os.path.join(CHARS, k_["folder"])
    os.makedirs(folder, exist_ok=True)
    report = []
    for k, part in enumerate(ids):
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
            score, grow, dx, dy = rescale(kind, e_got, e_tpl, x, y, base_name, dx, dy)
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
            ax, ay = MIDDLE[kind][base_name]
            box = (x + ax + (-ox - ax) * grow + dx, y + ay + (-oy - ay) * grow + dy, b.shape[1] * s * grow, b.shape[0] * s * grow)
            out = resample_rgb(got, box, (b.shape[1], b.shape[0]), weight=mine.astype(np.float64))
            if kind in ("cape", "weapon"):
                out = only_changed(out, b, tpl, x, y, ox, oy, s, near)
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
    print("part_sheet: cut %s (%s, %d %ss) into tools/source/characters/%s/" % (sheet_id, base_name, len(ids), kind, k_["folder"]))
    print("\n".join(report))


def through_filter(edge, box, size):
    """An edge map, clipped at 120 as the matching clips it, resampled (box and size as
    ic.resample takes them). The resampler keeps colour within alpha, so it goes through as a 0-to-1
    colour on a solid alpha and comes back out at its own scale."""
    e = np.minimum(edge, 120.0) / 120.0
    picture = np.stack([e, e, e, np.ones_like(e)], axis=2)
    return ic.resample(picture, box, size)[:, :, 0] * 120.0


def only_changed(out, b, tpl, x, y, ox, oy, s, near):
    """A cape's or a weapon's cell, put back at full size (out), kept only where it differs from its
    template cell put back the same way: everywhere else it's the base's own pixel. Scaling the cell
    down and up again softens the whole figure, so without this every outline would read as changed.
    Specks are opened away and what's kept is grown a little, to keep the part's own soft edge."""
    h, w = b.shape[:2]
    cell_w, cell_h = cell_size("cape")
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
    if kind in ("armor", "cape", "weapon", "helm"):
        sides["left"] = (slice(y, y + ch), slice(x + KEEP_OFF_LINES + 1, x + band + 2))
        sides["right"] = (slice(y, y + ch), slice(x + cw - band - 2, x + cw - KEEP_OFF_LINES - 1))
    if kind in ("cape", "weapon", "helm"):
        sides["top"] = (slice(y + KEEP_OFF_LINES + 1, y + band + 2), slice(x, x + cw))
    for side, (rs, cs) in sides.items():
        # The template where this part of the cell came from, before ChatGPT shifted it.
        ys = np.clip(np.arange(rs.start, rs.stop) - dy, 0, SHEET_H - 1)
        xs = np.clip(np.arange(cs.start, cs.stop) - dx, 0, SHEET_W - 1)
        empty = tpl[ys][:, xs].max(axis=2) < 30
        painted = got[rs, cs].max(axis=2) > 60
        if (empty & painted).mean() > 0.02:
            notes.append({"face": "the face", "hair": "the hair", "helm": "the helm", "cape": "the cape",
                          "weapon": "the weapon"}.get(kind, "the outfit") + " runs off the %s of the cell and is cut there" % side)
    return notes


def write_assets(table):
    lines = [A_BEGIN, "## Part sheets", "",
             "Several parts at a time: ChatGPT paints one face, hairstyle, outfit, helm, cape or weapon into each cell of a "
             "sheet made from a base (Story/13-art-prompts.md §4.0, §4.9, §5.0 and §6.0), and `tools/part_sheet.py` puts each "
             "cell back on its base's canvas.", "",
             "tools/part_sheet.py - written by Caleb Kiatoukaysi for Legend Quest, makes the blank face, hair, armor, helm, cape "
             "and weapon sheets from the bases and cuts a painted sheet back into one picture per part, lined up with its base", ""]
    for kind in KINDS:
        for base_name in BASES:
            tid = template_id(kind, base_name)
            what = {"face": "eight copies of its head and shoulders at full size, four across and two down",
                    "hair": "four copies of it from the head to the hips at full size, side by side",
                    "helm": "four copies of it from the top of the canvas, room for a crown, to the hips, at full size, side by side"}.get(
                kind, "four copies of the whole figure at %g scale, side by side" % KINDS[kind]["scale"])
            lines += ["tools/source/characters/%s.png - the blank %s sheet: %s, from tools/source/characters/%s.png, with grey "
                      "lines between them; built by tools/part_sheet.py" % (tid, kind, what, base_file(base_name, kind)), ""]
    for sid in sorted(table):
        if os.path.exists(os.path.join(SHEETS, sid + ".png")):
            kind = kind_of(sid)
            lines += ["tools/source/characters/sheets/%s.png - %s sheet %s (%s), painted on tools/source/characters/%s.png; %s"
                      % (sid, kind, sid, ", ".join(table[sid][2]), template_id(kind, table[sid][0]), CREDIT), ""]
    for sid in sorted(table):
        kind = kind_of(sid)
        folder = KINDS[kind]["folder"]
        for k, part in enumerate(table[sid][2]):
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
    if not argv or argv[0] not in ("templates", "cut", "list"):
        sys.exit(__doc__)
    if argv[0] == "templates":
        for kind in KINDS:
            for base_name in BASES:
                tid = template_id(kind, base_name)
                save(os.path.join(CHARS, tid + ".png"), template(kind, base_name))
                print("part_sheet: wrote tools/source/characters/%s.png" % tid)
        write_assets(sheets_table())
    elif argv[0] == "list":
        for sid, (base_name, pri, ids) in sorted(sheets_table().items()):
            print("%s (%s, %s): %s" % (sid, base_name, pri, ", ".join("%d %s" % (k + 1, f) for k, f in enumerate(ids))))
    else:
        args = [a for a in argv[1:] if not a.startswith("--")]
        if len(args) != 2:
            sys.exit("part_sheet: cut <png> <sheet-id> [--force]")
        cut(args[0], args[1], force="--force" in argv)


if __name__ == "__main__":
    main(sys.argv[1:])
