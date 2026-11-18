"""
Weapons painted on their own, with no hand, and put in the figure's fist here.

    python tools/weapon_alone.py template                                   # the blank sheet to paint four on
    python tools/weapon_alone.py cut <painted.png> <base> <id> [<id> ...]   # a sheet into its items, each in the fist
    python tools/weapon_alone.py preview <base> <out.png> <id> [<id> ...]   # each held, at rest and in a few poses

The owner's call (2026-11-18): ChatGPT, asked to put an item through a fist, redraws the hand with
it. So an item is painted alone, upright, with nothing holding it, and the game does the holding:
it already draws what's held under a plain closed fist (the hand's own piece), which closes over the
grip.

`template` writes tools/source/characters/weapon-alone-sheet.png: 1536 x 1024, black, four columns
parted by thin grey lines, and in each a pair of small grey ticks at the columns' edges, GRIP_ROW
down. An item is painted upright in its column, its business end up, with the middle of its grip
(where the hand goes) level with the ticks: a sword's grip, the middle of a staff, a bow's handle,
the top of a lantern's ring, the middle of a shield.

`cut` takes the painted sheet: each column keyed off the black (the grey lines and ticks left out),
its item found, and its grip worked out from its own shape, by its kind (grip_of): the ticks tell
ChatGPT where to put it, but it's only ever near them. Each is kept
as it was painted (tools/source/characters/weapons-alone/<id>.png, with the grip in <id>.json), and
put in the fist on the base: turned to the angle its kind is held at (HOLDS: a blade low with its
point forward, a staff near upright, a carried thing hanging), made the size its kind is (a long
weapon is painted smaller than life to fit its column), its grip on the hand's socket, drawn under
the hand's own piece: tools/source/characters/weapons/<id>.png, the base with the item in its
fist, which tools/import_parts.py takes as it does any item painted on the base. An id starting
"offhand-" goes in the far hand.

`preview` draws each on the body at rest and in a few of the game's poses, the item moving with the
hand.

Needs CPython, numpy and scipy.

Written by Caleb Kiatoukaysi for Legend Quest.
"""
import json
import os
import re
import sys

import numpy as np
from scipy import ndimage as ndi

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
sys.path.insert(0, os.path.join(HERE, "rig"))
import import_character as ic  # noqa: E402
import skeleton as K  # noqa: E402
import dress_pose as D  # noqa: E402  (the body's pieces, warping)

ROOT = os.path.dirname(HERE)
CHARS = os.path.join(ROOT, "tools", "source", "characters")
SHEET = (1536, 1024)
COLS = 4
CELL = SHEET[0] // COLS
#: How far down a column the middle of an item's grip goes, and the ticks that mark it.
GRIP_ROW = 640
TICK, LINE_GREY = 26, 96
#: How an item's held, by kind: (degrees clockwise from upright, its size against how it's painted).
#: The first kind whose words are in the item's id is its kind.
HOLDS = (
    ("shield", r"shield|buckler", (0.0, 1.0)),
    ("carried", r"lantern|basket|bucket|pail|pot|jug|bag|sack|satchel|tray|book|bottle|tankard|cup|flask|net|fish|bell|censer|cage|kettle", (0.0, 1.0)),
    ("long", r"staff|spear|halberd|pike|glaive|trident|scythe|crook|banner|pole|oar|rake|pitchfork|hoe|broom|longbow|lance|standard", (8.0, 1.45)),
    ("bow", r"bow", (10.0, 1.0)),
    ("blade", r"", (115.0, 1.0)),
)
#: Where each hand holds, by the bone, and the joint and tip its socket's between.
HANDS = {"near": "hand_near", "far": "hand_far"}
#: Smaller than this (pixels) and a painted scrap isn't the item's.
SPECK = 120
#: A handle is the rows no wider than this many times the thinner rows of an item's lower half
#: (their 30th percentile: a pommel's tip is thinner still, and isn't the handle).
GRIP_THIN = 1.4


def kind_of(item):
    for name, words, hold in HOLDS:
        if re.search(words, item):
            return name, hold
    return HOLDS[-1][0], HOLDS[-1][2]


def template_path():
    return os.path.join(CHARS, "weapon-alone-sheet.png")


def lines():
    """Where the template's grey is: the lines between the columns and the ticks at the grip's row."""
    w, h = SHEET
    grey = np.zeros((h, w), bool)
    for k in range(1, COLS):
        grey[:, k * CELL - 1:k * CELL + 1] = True
    for k in range(COLS):
        x0 = k * CELL
        grey[GRIP_ROW - 1:GRIP_ROW + 2, x0 + 4:x0 + 4 + TICK] = True
        grey[GRIP_ROW - 1:GRIP_ROW + 2, x0 + CELL - 4 - TICK:x0 + CELL - 4] = True
    return grey


def template():
    w, h = SHEET
    sheet = np.zeros((h, w, 4), np.uint8)
    sheet[:, :, 3] = 255
    sheet[lines(), :3] = LINE_GREY
    ic.write_png(template_path(), sheet)
    print("weapon_alone: wrote %s (four columns, the grip's ticks %d down)" % (os.path.relpath(template_path(), ROOT), GRIP_ROW))


def socket(base, side):
    """Where a hand holds, on the base's canvas: the middle of its fist, between the wrist and its tip."""
    wrist = np.array(K.JOINTS[base]["wrist_" + side], float)
    tip = np.array(K.TIPS[base]["hand_" + side], float)
    return wrist + 0.55 * (tip - wrist)


def items(painted):
    """The painted sheet's columns: [(colour 0-255, alpha 0-1, where it's solid)], None for an empty one."""
    w, h = SHEET
    got = ic.read_png(painted)[0][:, :, :3]
    if got.shape[:2] != (h, w):
        pm = np.concatenate([got / 255.0, np.ones(got.shape[:2] + (1,))], axis=2)
        got = np.rint(np.clip(ic.resample(pm, (0, 0, got.shape[1], got.shape[0]), (w, h))[:, :, :3], 0, 1) * 255).astype(np.uint8)
    # The template's grey, where it's still grey, is nothing.
    rgb = got.astype(np.int64)
    greyish = (np.abs(rgb[:, :, 0] - rgb[:, :, 1]) < 18) & (np.abs(rgb[:, :, 1] - rgb[:, :, 2]) < 18) & (rgb.max(axis=2) < 170)
    got = got.copy()
    got[ndi.binary_dilation(lines(), iterations=3) & greyish] = 0
    colour, alpha = D.key(got)
    out = []
    for k in range(COLS):
        c, a = colour[:, k * CELL:(k + 1) * CELL], alpha[:, k * CELL:(k + 1) * CELL].copy()
        labels, n = ndi.label(a > 0.5)
        sizes = np.bincount(labels.ravel(), minlength=n + 1)
        keep = np.isin(labels, [i for i in range(1, n + 1) if sizes[i] >= SPECK])
        a *= ndi.binary_dilation(keep, iterations=2)
        solid = a > 0.5
        if not solid.any():
            out.append(None)
            continue
        out.append((c, a, solid))
    return out


def grip_of(item, solid):
    """Where the hand goes on an item painted upright, by its kind, from its own shape (ChatGPT puts
    the grip near the ticks, not on them): a carried thing's ring at its top; the middle of a shield
    or a bow; a long weapon a little above its middle; and for the rest (a sword, an axe, a mace) the
    middle of the longest thin stretch of its lower half, which is its handle."""
    kind = kind_of(item)[0]
    rows = np.nonzero(solid.any(axis=1))[0]
    top, bottom = int(rows[0]), int(rows[-1])
    xs = np.arange(solid.shape[1])
    left = np.where(solid, xs[None, :], solid.shape[1]).min(axis=1)
    right = np.where(solid, xs[None, :], -1).max(axis=1)
    width = (right - left + 1).astype(float)
    middle = lambda y: (left[y] + right[y]) / 2.0  # noqa: E731
    if kind == "carried":
        y = top + min(14, (bottom - top) // 8)
    elif kind in ("shield", "bow"):
        y = (top + bottom) // 2
    elif kind == "long":
        y = int(top + 0.45 * (bottom - top))
    else:
        lo = int(top + 0.5 * (bottom - top))
        thin = width[lo:bottom + 1] <= GRIP_THIN * np.percentile(width[lo:bottom + 1], 30)
        runs, n = ndi.label(thin)
        best = max(range(1, n + 1), key=lambda i: (runs == i).sum())
        where = np.nonzero(runs == best)[0]
        y = lo + int(where.mean())
    return float(middle(y)), float(y)


def held(base, colour, alpha, grip, item):
    """An item on the base's canvas, in the fist: (colour, alpha) under the hand, and which hand."""
    side = "far" if item.startswith("offhand-") else "near"
    _, (angle, size) = kind_of(item)
    if side == "far":
        angle = -angle if angle > 20 else angle
    t = np.radians(angle)
    c, s = np.cos(t) * size, np.sin(t) * size
    at = socket(base, side)
    m = np.array([[c, -s, 0.0], [s, c, 0.0]])
    m[:, 2] = at - m[:, :2] @ np.array(grip)
    w, h = D.CANVAS
    pm = np.clip(D.warp(D.premul(colour, alpha), m, (h, w)), 0.0, 1.0)
    return pm, side


def cut(painted, base, ids):
    bodies = D.body(base)
    w, h = D.CANVAS
    alone_dir, held_dir = os.path.join(CHARS, "weapons-alone"), os.path.join(CHARS, "weapons")
    os.makedirs(alone_dir, exist_ok=True)
    os.makedirs(held_dir, exist_ok=True)
    found = items(painted)
    for k, item in enumerate(ids):
        if k >= COLS or found[k] is None:
            print("  %-28s nothing painted in column %d: redo" % (item, k + 1))
            continue
        colour, alpha, solid = found[k]
        grip = grip_of(item, solid)
        ic.write_png(os.path.join(alone_dir, item + ".png"), ic.straight(D.premul(colour, alpha)))
        json.dump({"grip": [round(grip[0], 1), round(grip[1], 1)], "kind": kind_of(item)[0]},
                  open(os.path.join(alone_dir, item + ".json"), "w", encoding="utf-8"), indent=1)
        pm, side = held(base, colour, alpha, grip, item)
        # The base stood up from its pieces, the item under the hand that holds it.
        figure = np.zeros((h, w, 4))
        for bone in K.DRAW:
            if bone == HANDS[side]:
                figure = pm + figure * (1.0 - pm[:, :, 3:])
            layer = D.premul(*bodies[bone])
            figure = layer + figure * (1.0 - layer[:, :, 3:])
        rgb = np.rint(np.clip(figure[:, :, :3], 0, 1) * 255).astype(np.uint8)
        ic.write_png(os.path.join(held_dir, item + ".png"), np.dstack([rgb, np.full((h, w), 255, np.uint8)]))
        print("  %-28s a %s, in the %s hand; its grip at %.0f, %.0f" % (item, kind_of(item)[0], side, grip[0], grip[1]))


#: The poses a preview shows.
SHOTS = ((None, 0.0), ("idle", 1.0), ("jog@%s", 0.18), ("windup-one-hand", 0.2), ("strike-one-hand", 0.12), ("block", 0.3))


def preview(base, out, ids):
    import clips as C
    doc = json.load(open(os.path.join(ROOT, "Content", "Characters", "Parts", "clips.json"), encoding="utf-8"))
    known = doc["rigs"]["humanoid"]["clips"]
    bodies = D.body(base)
    w, h = D.CANVAS
    rows = []
    for item in ids:
        rgba = ic.read_png(os.path.join(CHARS, "weapons-alone", item + ".png"))[0].astype(np.float64)
        grip = json.load(open(os.path.join(CHARS, "weapons-alone", item + ".json"), encoding="utf-8"))["grip"]
        pm, side = held(base, rgba[:, :, :3], rgba[:, :, 3] / 255.0, grip, item)
        layers = dict(bodies)
        layers[HANDS[side] + "+"] = (pm[:, :, :3] / np.maximum(pm[:, :, 3:], 1e-6) * 255.0, pm[:, :, 3])
        row = []
        for name, t in SHOTS:
            clip = known.get(name % base if name and "%s" in name else name) if name else None
            pose = C.sample(clip, t) if clip else {}
            row.append(K.render(base, layers, pose, size=(w + 400, h), origin=(200, 0))[::2, ::2, :3])
        rows.append(np.concatenate(row, axis=1))
    sheet = np.concatenate(rows, axis=0)
    ic.write_png(out, np.dstack([np.rint(np.clip(sheet, 0, 1) * 255).astype(np.uint8), np.full(sheet.shape[:2], 255, np.uint8)]))
    print("weapon_alone: wrote", out)


def main(argv):
    if argv == ["template"]:
        template()
    elif len(argv) >= 4 and argv[0] == "cut":
        cut(argv[1], argv[2], argv[3:])
    elif len(argv) >= 4 and argv[0] == "preview":
        preview(argv[1], argv[2], argv[3:])
    else:
        sys.exit(__doc__)


if __name__ == "__main__":
    main(sys.argv[1:])
