"""
The mannequins as a cut-out rig: each body painted in pieces, pulled apart, not as a whole figure.

    python tools/body_parts.py template <old-base.png> <base>   # the exploded template, from a figure
    python tools/body_parts.py armor                            # the armor templates, from the body's
    python tools/body_parts.py check <painted.png> <id> [--show <png>]   # is it the template's size and layout?
    python tools/body_parts.py cut <painted.png> <id>           # a painted template back into its pieces

<id> is body-man or body-woman (or the base, base-male or base-female) for a body, or an armor's id
(Story/13 §5: its card's Basis says which armor template it edits).

A cut-out ("paper doll") rig draws a figure as separate pieces on a bone tree and turns the pieces
about their joints: fourteen of them, the upper arm, the forearm, the hand, the thigh, the shin and
the foot (near and far), the torso and the head. The torso is one piece, from the neck to the
shorts. Each piece is whole, going on under the piece in front of it, so it can turn there without
a gap.

`template` makes the sheet ChatGPT paints the pieces on, for one base: the figure given (a
mannequin painted whole) cut into its pieces along its own painted lines, the jaw and each joint's
seam (tools/rig/scissors.py, which knows where those lines are on each picture it cuts), and the
pieces pulled apart along the skeleton (EXPLODE, widened till none touch), on black, at the biggest
scale that fits a 1024 x 1536 picture: tools/source/characters/body-parts-<man|woman>.png, with
where each piece is in tools/source/characters/body-parts-<man|woman>.json.

`cut` takes the template ChatGPT painted and puts each piece back on its bone, at rest, at full size:
one picture per piece, tools/source/characters/bodies/<base>/<bone>.png, each on the base's 1024 x
1536 canvas with its own alpha, and the pieces drawn together in their order on black as
tools/source/characters/<base>.png, the base every other part is painted on. tools/import_parts.py
takes the pieces from there.

ChatGPT redraws a whole picture, and hands it back a little zoomed or shifted, at another size, or
with a piece moved, drawn bigger or smaller, or turned a little. So the cut doesn't hold the painting
to the template; it finds each piece where ChatGPT put it and fits it to its own bone:

  - the whole picture is found on the template first (tools/sheet_fit.py), and put back only when
    it's off by more than WHOLE_FAR (a body) or WHOLE_BULK (an armor, whose outfit is bigger than the
    body under it, which reads as a zoom when it isn't one);
  - each painted shape goes, whole, to the piece whose place it's painted in (`owners`), so a piece
    ChatGPT moved is still all one piece; a shape painted across two places (pieces drawn touching)
    is split between them where they meet, and reported;
  - each piece is fitted to its guide on its own (`fit_piece`): for a body, the size, turn and place
    at which the guide's shape lies best over the painted one; for an armor, which is meant to be
    bigger than the body under it, the least turn and move that has it cover its body piece. A
    forearm ChatGPT drew 6% longer and 4 degrees off still meets the elbow and the wrist where the
    skeleton has them.

What can't be put right is reported to redo: a piece missing, cropped off the picture, redrawn in
another shape (a bent arm) or further off than the fit looks (FIT_SIZES, FIT_TURN, FIT_SHIFT), or
an armor piece that doesn't cover its body piece wherever it's put. `check` does all that without
writing anything and says whether the picture is good to cut.

A single piece can be painted again on its own: paint it on the whole template (the other pieces
as they were) and cut that; the fit finds it wherever it is.

`armor` writes the armor templates, tools/source/characters/armor-parts-<man|woman>.png: the body's
template with the head left out (the head is the face's, the hair's and the helm's), the thirteen
pieces an outfit dresses, in the same places, so an armor is cut exactly as the body is: each painted
piece back to rest as tools/source/characters/armors/<id>/<bone>.png. An outfit can be bigger than
the body (a sleeve, a skirt, a pauldron), so an armor's pieces aren't held to the guides' size; each
is checked to cover its guide instead.

Till the bodies are painted, every other sheet (tools/part_sheet.py) is drawn from the template's
own pieces stood up at rest (`standing`, cut into tools/.cache/stand-in): the same shapes in the
same places as the painted figure's, so a sheet painted on either cuts the same.

Written by Caleb Kiatoukaysi for Legend Quest.
"""
import json
import os
import sys

import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
sys.path.insert(0, os.path.join(HERE, "rig"))
import import_character as ic  # noqa: E402  (PNGs, masks, resampling)
import skeleton as K  # noqa: E402  (the joints and the cut into pieces)
import sheet_fit  # noqa: E402  (a picture found on another, moved and scaled)

ROOT = os.path.dirname(HERE)
CHARS = os.path.join(ROOT, "tools", "source", "characters")
CANVAS = (1024, 1536)
NAMES = {"base-male": "man", "base-female": "woman"}

#: The pieces, each on the bone of its name: the torso is one, from the neck to the shorts.
PIECES = ("head", "torso", "arm_far_up", "arm_far_low", "hand_far", "arm_near_up", "arm_near_low", "hand_near",
          "leg_far_up", "leg_far_low", "foot_far", "leg_near_up", "leg_near_low", "foot_near")
#: Back to front, as the game draws them at rest.
DRAW = tuple(b for b in K.DRAW if b in PIECES)
#: How each piece is pulled from its parent, in gaps (x, y): the head up, the arms out and down,
#: the legs down and apart. A piece moves with its parent and then by its own step.
EXPLODE = {"torso": (0, 0), "head": (0, -1),
           "arm_near_up": (-1.1, 0), "arm_near_low": (-0.3, 1), "hand_near": (0, 1),
           "arm_far_up": (1.1, 0), "arm_far_low": (0.3, 1), "hand_far": (0, 1),
           "leg_near_up": (-0.9, 1), "leg_near_low": (-0.2, 1), "foot_near": (-0.2, 1),
           "leg_far_up": (0.9, 1), "leg_far_low": (0.2, 1), "foot_far": (0.2, 1)}
PARENT = {"torso": None, "head": "torso", "arm_near_up": "torso", "arm_near_low": "arm_near_up",
          "hand_near": "arm_near_low", "arm_far_up": "torso", "arm_far_low": "arm_far_up", "hand_far": "arm_far_low",
          "leg_near_up": "torso", "leg_near_low": "leg_near_up", "foot_near": "leg_near_low",
          "leg_far_up": "torso", "leg_far_low": "leg_far_up", "foot_far": "leg_far_low"}
#: The smallest gap between pieces on the template, in its pixels, and the first gap tried and its step (canvas pixels).
LEAST_GAP, FIRST_GAP, GAP_STEP = 22, 60, 10
#: The template's margin round the pieces.
MARGIN = 24
#: A painted shape belongs to the piece whose place it's painted in: each pixel of the template is
#: that of the guide nearest it, up to this far (template pixels).
REACH = 40
#: A shape with fewer pixels than this in no piece's place is a speck, and dropped.
SPECK = 60
#: A shape painted across two pieces' places is two pieces drawn touching when the lesser has at
#: least this share of it (and SPECK pixels); less, and it's one piece reaching a little.
JOINED = 0.08
#: Darker than this (brightest channel) is the black background.
BLACK = 14
#: How far a piece may be from its guide and still be fitted to it: its size, its turn (degrees)
#: and its move (template pixels). A fit at the edge of these is reported to redo.
FIT_SIZES, FIT_TURN, FIT_SHIFT = (0.80, 1.25), 15.0, 80
#: The fit looks coarsely first, at 1/FIT_COARSE size, every COARSE_SIZE and COARSE_TURN degrees,
#: then at full size every FINE_SIZE and FINE_TURN degrees round the best, FINE_STEPS either side.
FIT_COARSE, COARSE_SIZE, COARSE_TURN = 4, 0.03, 2.0
FINE_SIZE, FINE_TURN, FINE_STEPS = 0.004, 0.25, 6
#: A body's piece is put right when its fit is off by more than SETTLE_SIZE in size, SETTLE_TURN
#: degrees or SETTLE_PLACE template pixels; closer, it's left as painted. One with under
#: SETTLE_LEAST of its guide's area is missing or cut short, and one whose shape overlaps its
#: guide's less than SHAPE_POOR (the share of the two together they have in common) at the best fit
#: was redrawn in another shape.
SETTLE_SIZE, SETTLE_TURN, SETTLE_PLACE = 0.01, 0.4, 1.0
SETTLE_LEAST, SHAPE_POOR = 0.5, 0.85
#: The whole picture against the template: off by more than WHOLE_FAR template pixels anywhere on
#: the figure (WHOLE_BULK for an armor) it was zoomed or moved, which the cut puts right; matched
#: worse than WHOLE_POOR (an armor, whose shape is its own, WHOLE_POOR_ARMOR) it isn't the template
#: at all.
WHOLE_FAR, WHOLE_BULK, WHOLE_POOR, WHOLE_POOR_ARMOR = 6.0, 16.0, 0.6, 0.3
#: The whole picture is found at 1/WHOLE_COARSE size: it's only where each piece's fit starts.
WHOLE_COARSE = 4
#: An armor's piece has to cover this much of its guide (the body piece it dresses), and no more
#: than SPILL_MOST of what's painted may be in no piece's place (it'd be lost). Its fit is among
#: the turns and moves whose cover is within COVER_SLACK of the best, with the guide grown by each
#: of ARMOR_BULK (template pixels: a close sleeve to a thick coat) to find the outfit's own.
COVER_LEAST, SPILL_MOST, COVER_SLACK = 0.85, 0.02, 0.015
ARMOR_BULK = (0, 3, 6, 10, 15, 21)
#: Where the figure stood up from the template's own pieces is kept, till the painted body comes.
STAND_IN = os.path.join(ROOT, "tools", ".cache", "stand-in")
STORY = os.path.join(ROOT, "Story", "13-art-prompts.md")
WHO = {"body-man": "base-male", "body-woman": "base-female"}


def load(path):
    return ic.read_png(path)[0][:, :, :3]


def save(path, rgb):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    ic.write_png(path, np.dstack([rgb.astype(np.uint8), np.full(rgb.shape[:2], 255, np.uint8)]))


def paths(base):
    name = NAMES[base]
    return (os.path.join(CHARS, "body-parts-%s.png" % name), os.path.join(CHARS, "body-parts-%s.json" % name))


def armor_path(base):
    return os.path.join(CHARS, "armor-parts-%s.png" % NAMES[base])


def armor_template(base):
    """The thirteen pieces an outfit dresses, in the body template's places: once the body's painted
    and cut (bodies/<base>/), its own pieces pulled apart again (painted_layout), so an outfit is
    painted over the body it's worn on and its bare skin is that body's; till then the body's
    template with its head left out."""
    if os.path.isdir(os.path.join(CHARS, "bodies", base)):
        save(armor_path(base), painted_layout(base))
        print("body_parts: wrote %s (the painted %s's pieces, less the head, in the template's places)"
              % (os.path.relpath(armor_path(base), ROOT), NAMES[base]))
        return
    png, js = paths(base)
    tpl = load(png)
    data = json.load(open(js, encoding="utf-8"))
    _, alpha = key(tpl)
    head = ic.grow(guide_on_template(alpha, data, "head"), 3)
    out = tpl.copy()
    out[head] = 0
    save(armor_path(base), out)
    print("body_parts: wrote %s (body-parts-%s.png less the head)" % (os.path.relpath(armor_path(base), ROOT), NAMES[base]))


def painted_layout(base, head=False):
    """The painted body's pieces (bodies/<base>/<bone>.png, each on its bone at rest) pulled apart
    again just as the template has them, on black, with or without the head: RGB 0-255."""
    _, js = paths(base)
    data = json.load(open(js, encoding="utf-8"))
    w, h = CANVAS
    sheet = np.zeros((h, w, 3))
    for bone in DRAW:
        if bone == "head" and not head:
            continue
        rgba = ic.read_png(os.path.join(CHARS, "bodies", base, bone + ".png"))[0].astype(np.float64)
        alpha = rgba[:, :, 3] / 255.0
        c, a = drawn(rgba[:, :, :3], alpha, data["pieces"][bone]["pull"], data["scale"], data["origin"])
        sheet = c + sheet * (1.0 - a[:, :, None])
    return np.rint(np.clip(sheet, 0, 1) * 255)


def standing(base):
    """The base figure at rest, as every other sheet shows it: the painted body stood up from its
    pieces (tools/source/characters/<base>.png, which `cut` writes) once it's there; till then the
    template's own pieces stood up the same way, in tools/.cache/stand-in."""
    own = os.path.join(CHARS, base + ".png")
    if os.path.exists(own):
        return load(own)
    png, js = paths(base)
    out_dir = os.path.join(STAND_IN, base)
    target = os.path.join(out_dir, base + ".png")
    newest = max(os.path.getmtime(p) for p in (png, js, os.path.abspath(__file__)))
    if not os.path.exists(target) or os.path.getmtime(target) < newest:
        cut(png, base, out_dir, quiet=True)
    return load(target)


def who_is(who):
    """(base, armor id or None) for a cut's <id>: a body's (body-man, body-woman or the base's own
    name), or an armor's, whose base is the armor template its Story/13 card edits."""
    import re
    if who in NAMES:
        return who, None
    if who in WHO:
        return WHO[who], None
    text = open(STORY, encoding="utf-8").read()
    m = re.search(r"^#### `%s` · [^\n]*\n\n\*\*Basis:\*\* Edits `armor-parts-(man|woman)`" % re.escape(who), text, re.M)
    if not m:
        sys.exit("body_parts: %s isn't body-man, body-woman or an armor whose Story/13 card edits armor-parts-man or "
                 "armor-parts-woman" % who)
    return ("base-male" if m.group(1) == "man" else "base-female"), who


def key(rgb):
    """A picture on black as colour and alpha: the black round the figure is nothing."""
    import import_parts as ip
    return ip.key_black(rgb)


def guide_pieces(base, rgb):
    """The figure given cut into its pieces on its canvas: {bone: (colour 0-255, alpha 0-1)}. The
    cut is tools/rig/scissors.py's, along the figure's own painted lines (the jaw, each joint's
    seam): every piece whole, with what the pieces in front of it hide filled in in its own skin
    and outlined, so it turns under them without a gap."""
    import scissors
    cutting = scissors.Cutting(base, rgb)
    if cutting.problems:
        sys.exit("body_parts: %s can't be cut: %s" % (base, "; ".join(cutting.problems)))
    return {b: (c.astype(np.float64), a.astype(np.float64)) for b, (c, a) in cutting.pieces().items()}


def offsets(gap):
    """Each piece's pull from where it is at rest, in canvas pixels, for a gap."""
    out = {}
    for bone in PIECES:
        x = y = 0.0
        b = bone
        while b is not None:
            dx, dy = EXPLODE[b]
            x, y = x + dx * gap, y + dy * gap
            b = PARENT[b]
        out[bone] = (x, y)
    return out


def layout(pieces):
    """The gap and scale the pieces are pulled apart by and drawn at, and where the canvas's corner
    goes on the template: (gap, scale, origin, offsets). The gap's widened till no two pieces come
    within LEAST_GAP of each other on the template."""
    boxes = {b: ic_bbox(a > 0.5) for b, (_, a) in pieces.items()}
    gap = FIRST_GAP
    while True:
        off = offsets(gap)
        placed = {b: (x0 + off[b][0], y0 + off[b][1], x1 + off[b][0], y1 + off[b][1]) for b, (x0, y0, x1, y1) in boxes.items()}
        x0 = min(p[0] for p in placed.values())
        y0 = min(p[1] for p in placed.values())
        x1 = max(p[2] for p in placed.values())
        y1 = max(p[3] for p in placed.values())
        scale = min((CANVAS[0] - 2 * MARGIN) / (x1 - x0), (CANVAS[1] - 2 * MARGIN) / (y1 - y0))
        origin = (MARGIN - x0 * scale + ((CANVAS[0] - 2 * MARGIN) - (x1 - x0) * scale) / 2, MARGIN - y0 * scale)
        masks = {b: drawn_mask(a, off[b], scale, origin) for b, (_, a) in pieces.items()}
        reach = max(1, int(LEAST_GAP / 2))
        grown = {b: ic.grow(m, reach) for b, m in masks.items()}
        clash = any((grown[p] & grown[q]).any() for i, p in enumerate(PIECES) for q in PIECES[i + 1:])
        if not clash:
            return gap, scale, origin, off
        gap += GAP_STEP


def ic_bbox(mask):
    ys, xs = np.nonzero(mask)
    return xs.min(), ys.min(), xs.max() + 1, ys.max() + 1


def drawn_mask(alpha, off, scale, origin):
    """A piece's mask drawn on the template (canvas (X, Y) at origin + scale (X + off))."""
    w, h = CANVAS
    box = (-(origin[0] / scale) - off[0], -(origin[1] / scale) - off[1], w / scale, h / scale)
    a = ic.resample(np.repeat(alpha[:, :, None], 4, axis=2), box, (w, h))[:, :, 0]
    return a > 0.5


def drawn(colour, alpha, off, scale, origin):
    """A piece drawn on the template: (premultiplied RGB 0-1, alpha)."""
    w, h = CANVAS
    box = (-(origin[0] / scale) - off[0], -(origin[1] / scale) - off[1], w / scale, h / scale)
    pm = np.concatenate([colour / 255.0 * alpha[:, :, None], alpha[:, :, None]], axis=2)
    out = ic.resample(pm, box, (w, h))
    return out[:, :, :3], out[:, :, 3]


def template(old, base):
    pieces = guide_pieces(base, load(old))
    gap, scale, origin, off = layout(pieces)
    w, h = CANVAS
    sheet = np.zeros((h, w, 3))
    centres = {}
    for bone in DRAW:
        c, a = drawn(*pieces[bone], off[bone], scale, origin)
        sheet = c + sheet * (1.0 - a[:, :, None])
        ys, xs = np.nonzero(a > 0.5)
        # The piece's middle, moved onto the piece if that's off it (a bent arm's middle can be beside it).
        mx, my = xs.mean(), ys.mean()
        k = np.argmin((xs - mx) ** 2 + (ys - my) ** 2)
        centres[bone] = (float(xs[k]), float(ys[k]))
    png, js = paths(base)
    save(png, np.rint(np.clip(sheet, 0, 1) * 255))
    data = {"base": base, "scale": round(scale, 6), "origin": [round(origin[0], 3), round(origin[1], 3)], "gap": gap,
            "pieces": {b: {"pull": [round(off[b][0], 3), round(off[b][1], 3)], "centre": [round(v, 1) for v in centres[b]]}
                       for b in PIECES}, "draw": list(DRAW)}
    with open(js, "w", encoding="utf-8", newline="\n") as f:
        json.dump(data, f, indent=1)
        f.write("\n")
    print("body_parts: wrote %s and %s: %d pieces, gap %d, scale %.3f (a standard adult %d pixels tall on it)"
          % (os.path.relpath(png, ROOT), os.path.relpath(js, ROOT), len(PIECES), gap, scale,
             (K.JOINTS[base]["root"][1] - K.CROWN[base]) * scale))


def silhouette(rgb):
    """A picture's figure as white on black (what isn't the background), for matching an armor,
    whose colours aren't the template's, on its shape."""
    _, a = key(rgb)
    return np.repeat((a * 255.0)[:, :, None], 3, axis=2)


def sample(picture, xs, ys, step=1.0):
    """A premultiplied H x W x C picture read at points (xs, ys) in its continuous coordinates
    (pixel k spans k to k + 1), through the Mitchell filter as tools/import_character.py's resample
    uses it, stretched to `step` (the picture's pixels per pixel read) when that's over one. Off the
    picture is nothing."""
    h, w = picture.shape[:2]
    cx, cy = xs - 0.5, ys - 0.5
    k = max(1.0, step)
    reach = int(np.ceil(2.0 * k))
    fx, fy = np.floor(cx).astype(np.int64), np.floor(cy).astype(np.int64)
    taps = range(-reach + 1, reach + 1)
    wx = [ic.mitchell((fx + i - cx) / k) for i in taps]
    wy = [ic.mitchell((fy + j - cy) / k) for j in taps]
    total = sum(wx) * sum(wy)
    out = np.zeros(xs.shape + picture.shape[2:])
    for j, ty in zip(taps, wy):
        yy = fy + j
        inside = (yy >= 0) & (yy < h)
        yc = np.clip(yy, 0, h - 1)
        for i, tx in zip(taps, wx):
            xx = fx + i
            weight = ty * tx * (inside & (xx >= 0) & (xx < w)) / total
            out += weight[..., None] * picture[yc, np.clip(xx, 0, w - 1)]
    alpha = np.clip(out[..., 3], 0.0, 1.0)
    out[..., :3] = np.clip(out[..., :3], 0.0, alpha[..., None])
    out[..., 3] = alpha
    return out


def bilinear(a, xs, ys):
    """A 2-D map read at points (xs, ys) in its continuous coordinates, linearly; off it is 0."""
    h, w = a.shape
    cx, cy = xs - 0.5, ys - 0.5
    x0, y0 = np.floor(cx).astype(np.int64), np.floor(cy).astype(np.int64)
    fx, fy = cx - x0, cy - y0

    def at(yy, xx):
        inside = (yy >= 0) & (yy < h) & (xx >= 0) & (xx < w)
        return np.where(inside, a[np.clip(yy, 0, h - 1), np.clip(xx, 0, w - 1)], 0.0)

    return ((1 - fy) * ((1 - fx) * at(y0, x0) + fx * at(y0, x0 + 1))
            + fy * ((1 - fx) * at(y0 + 1, x0) + fx * at(y0 + 1, x0 + 1)))


def similar(centre, size, turn, move):
    """The map of a guide's point t to where it's painted, centre + move + size R(turn) (t - centre),
    as (A, b): t goes to A t + b."""
    a = np.radians(turn)
    A = size * np.array([[np.cos(a), -np.sin(a)], [np.sin(a), np.cos(a)]])
    return A, np.asarray(centre, float) + np.asarray(move, float) - A @ np.asarray(centre, float)


def block_mean(a, k):
    h, w = a.shape
    return a[:h - h % k, :w - w % k].reshape(h // k, k, w // k, k).mean(axis=(1, 3))


def _parabola(left, mid, right):
    d = left - 2.0 * mid + right
    return 0.0 if d >= 0 else float(np.clip(0.5 * (left - right) / d, -0.5, 0.5))


def fit_piece(guide, painted, centre, armor):
    """How a piece is painted against its guide, both soft masks (0 to 1) on the template's grid:
    (size, turn in degrees, (dx, dy), score, cover, at_edge), the guide's point t painted at
    centre + d + size R(turn) (t - centre), `centre` being the guide's middle.

    A body piece's fit is the size, turn and move at which the two shapes overlap most (the score:
    what they have in common over what either covers). An armor piece is meant to be bigger than
    the body piece it dresses, so it's never resized; its fit is the turn and move at which it
    overlaps most with its guide grown by the outfit's bulk (ARMOR_BULK: how much is found with
    the fit), of those at which it covers the guide as well as it can (within COVER_SLACK): a
    sleeve's fitted to the arm in it, and a pauldron or a skirt's flare, which the arm or the leg
    doesn't fill, is held by the parts of the piece that fit closely, at the shoulder and the
    elbow, the waist. `cover` is how much of the guide the piece covers at its fit. `at_edge` is a
    fit at the edge of what's looked through (FIT_SIZES, FIT_TURN, FIT_SHIFT): it may be further off.

    It looks coarsely first, at 1/FIT_COARSE size, through every size (or bulk), turn and move;
    then at full size in a box round where that puts the guide, a turn at a time, a size at a time,
    and the neighbours of the best, finished by a parabola through them."""
    from scipy.ndimage import distance_transform_edt
    h, w = guide.shape
    S, k = FIT_SHIFT, FIT_COARSE
    c = np.asarray(centre, float)
    gy, gx = np.nonzero(guide > 0.5)
    radius = float(np.sqrt(((gx + 0.5 - c[0]) ** 2 + (gy + 0.5 - c[1]) ** 2).max()))
    painted_area, whole = painted.sum(), guide.sum()
    gx0, gy0, gx1, gy1 = ic_bbox(guide > 0.02)
    px0, py0, px1, py1 = ic_bbox(painted > 0.02)
    # The guide grown by each bulk, softly (its edge half a pixel wide), in a box round it.
    reach = max(ARMOR_BULK) + 3 if armor else 0
    bx0, by0, bx1, by1 = max(0, gx0 - reach), max(0, gy0 - reach), min(w, gx1 + reach), min(h, gy1 + reach)
    grown = {0: guide}
    if armor:
        away = distance_transform_edt(guide[by0:by1, bx0:bx1] < 0.5)

    def bulked(r):
        if r not in grown:
            out = guide.copy()
            out[by0:by1, bx0:bx1] = np.maximum(guide[by0:by1, bx0:bx1], np.clip(r + 0.5 - away, 0.0, 1.0))
            grown[r] = out
        return grown[r]

    def correlate(gt, fp, shape, shift):
        """What a rendered guide and the painted piece have in common at every move up to `shift`
        either way: (dy, dx)."""
        common = np.fft.irfft2(fp * np.conj(np.fft.rfft2(gt, shape)), shape)
        index = np.arange(-shift, shift + 1)
        return np.maximum(common[np.ix_(index % shape[0], index % shape[1])], 0.0)

    def tables(maps, fp, shape, shift, xs, ys, cc, move, shapes, turns, unit):
        """The overlap of every shape (a (size, bulk)) and turn against every move within `shift`
        of `move`, and for an armor the cover of the guide at every turn and move: (overlap
        (shape, turn, dy, dx), cover (turn, dy, dx) or None). `maps` gives a bulk's guide on the
        grid the box is in; `unit` is how many template pixels a pixel of it is."""
        overlap = np.zeros((len(shapes), len(turns), 2 * shift + 1, 2 * shift + 1))
        cover = np.zeros((len(turns), 2 * shift + 1, 2 * shift + 1)) if armor else None
        for ti, t in enumerate(turns):
            for si, (s, r) in enumerate(shapes):
                A, b = similar(cc, s, t, move)
                inv = np.linalg.inv(A)
                sx, sy = inv[0, 0] * (xs - b[0]) + inv[0, 1] * (ys - b[1]), inv[1, 0] * (xs - b[0]) + inv[1, 1] * (ys - b[1])
                gt = bilinear(maps(r), sx, sy)
                common = correlate(gt, fp, shape, shift) * unit * unit
                overlap[si, ti] = common / np.maximum(gt.sum() * unit * unit + painted_area - common, 1e-9)
                if armor and si == 0:
                    base = gt if r == 0 else bilinear(maps(0), sx, sy)
                    cover[ti] = correlate(base, fp, shape, shift) * unit * unit / max(whole, 1e-9)
        return overlap, cover

    def choose(overlap, cover):
        """(si, ti, iy, ix): the most overlap, of the moves that cover the guide as well as any."""
        if cover is None:
            return np.unravel_index(np.argmax(overlap), overlap.shape)
        ok = cover >= cover.max() - COVER_SLACK
        return np.unravel_index(np.argmax(np.where(ok[None], overlap, -1.0)), overlap.shape)

    # Coarsely, at 1/k size, every size (or bulk) and turn against every move, in a box big enough for all.
    spare = int(0.3 * max(gx1 - gx0, gy1 - gy0)) + S + reach
    x0, y0 = max(0, min(gx0, px0) - spare), max(0, min(gy0, py0) - spare)
    x1, y1 = min(w, max(gx1, px1) + spare), min(h, max(gy1, py1) + spare)
    x1, y1 = x0 + (x1 - x0) // k * k, y0 + (y1 - y0) // k * k
    small = {}

    def coarse_map(r):
        if r not in small:
            small[r] = block_mean(bulked(r)[y0:y1, x0:x1], k)
        return small[r]

    p = block_mean(painted[y0:y1, x0:x1], k)
    n = S // k
    shape = (sp_fast(p.shape[0] + n), sp_fast(p.shape[1] + n))
    ys, xs = np.mgrid[0:p.shape[0], 0:p.shape[1]] + 0.5
    shapes = ([(1.0, r) for r in ARMOR_BULK] if armor
              else [(s, 0) for s in np.arange(FIT_SIZES[0], FIT_SIZES[1] + 1e-9, COARSE_SIZE)])
    turns = list(np.arange(-FIT_TURN, FIT_TURN + 1e-9, COARSE_TURN))
    overlap, cover = tables(coarse_map, np.fft.rfft2(p, shape), shape, n, xs, ys, (c - (x0, y0)) / k, (0.0, 0.0), shapes, turns, k)
    si, ti, iy, ix = choose(overlap, cover)
    edge = (not armor and si in (0, len(shapes) - 1)) or ti in (0, len(turns) - 1) or n in (abs(iy - n), abs(ix - n))
    (s0, r0), t0, d0 = shapes[si], turns[ti], (float((ix - n) * k), float((iy - n) * k))

    # Then at full size, in a box round where the coarse fit puts the guide, moves within m of it.
    m = 2 * k + 2
    A, b = similar(c, s0 * (1 + FINE_STEPS * FINE_SIZE), t0, d0)
    turned = [A @ q + b for q in np.array([[gx0, gy0], [gx1, gy0], [gx0, gy1], [gx1, gy1]], float)]
    spare = m + int(radius * np.radians(FINE_STEPS * FINE_TURN)) + reach + 4
    x0 = max(0, int(min(min(q[0] for q in turned), px0)) - spare)
    y0 = max(0, int(min(min(q[1] for q in turned), py0)) - spare)
    x1 = min(w, int(max(max(q[0] for q in turned), px1)) + spare)
    y1 = min(h, int(max(max(q[1] for q in turned), py1)) + spare)
    p = painted[y0:y1, x0:x1]
    shape = (sp_fast(p.shape[0] + m), sp_fast(p.shape[1] + m))
    fp = np.fft.rfft2(p, shape)
    ys, xs = np.mgrid[y0:y1, x0:x1] + 0.5
    moves_x, moves_y = d0[0] + np.arange(-m, m + 1), d0[1] + np.arange(-m, m + 1)

    def fine(shapes, turns):
        return tables(bulked, fp, shape, m, xs, ys, c, d0, shapes, turns, 1)

    steps = np.arange(-FINE_STEPS, FINE_STEPS + 1)
    turns = list(t0 + steps * FINE_TURN)
    overlap, cover = fine([(s0, r0)], turns)
    turn = turns[choose(overlap, cover)[1]]
    shapes = ([(1.0, r) for r in range(max(0, r0 - 2), r0 + 3)] if armor else [(s, 0) for s in s0 + steps * FINE_SIZE])
    overlap, cover = fine(shapes, [turn])
    s1, r1 = shapes[choose(overlap, cover)[0]]
    # The neighbours of the best in size (not an armor's) and turn, moved till the best is in their middle.
    for _ in range(4):
        shapes = [(s1, r1)] if armor else [(s1 - FINE_SIZE, 0), (s1, 0), (s1 + FINE_SIZE, 0)]
        turns = [turn - FINE_TURN, turn, turn + FINE_TURN]
        overlap, cover = fine(shapes, turns)
        si, ti, iy, ix = choose(overlap, cover)
        middle = 0 if armor else 1
        if (si, ti) == (middle, 1):
            break
        (s1, r1), turn = shapes[si], turns[ti]
    best = float(overlap[si, ti, iy, ix])
    size, dx, dy = s1, float(moves_x[ix]), float(moves_y[iy])
    if ti == 1:
        if not armor and si == 1:
            size += _parabola(overlap[0, 1, iy, ix], best, overlap[2, 1, iy, ix]) * FINE_SIZE
        turn += _parabola(overlap[si, 0, iy, ix], best, overlap[si, 2, iy, ix]) * FINE_TURN
        if 0 < ix < 2 * m:
            dx += _parabola(overlap[si, ti, iy, ix - 1], best, overlap[si, ti, iy, ix + 1])
        if 0 < iy < 2 * m:
            dy += _parabola(overlap[si, ti, iy - 1, ix], best, overlap[si, ti, iy + 1, ix])
    covered = float(cover[ti, iy, ix]) if armor else best
    return float(size), float(turn), (dx, dy), best, covered, bool(edge)


def sp_fast(n):
    return sheet_fit._fast(n)


def owners(alpha, owner_t, back, count):
    """Which piece each painted pixel is (-1 none): every shape painted (alpha over a half) goes
    whole to the piece whose place most of it is in (`owner_t`, the template's pixels each the
    nearest guide's, read where `back` maps the painting's pixels to it); a shape painted across two
    places, the lesser at least JOINED of it, is split between them where they meet. The soft edge
    round each shape goes with it. Returns (owner, the pairs of pieces painted joined, how many
    pixels are painted in no piece's place)."""
    h, w = alpha.shape
    iy, ix = back
    inside = (iy >= 0) & (iy < owner_t.shape[0]) & (ix >= 0) & (ix < owner_t.shape[1])
    place = np.where(inside, owner_t[np.clip(iy, 0, owner_t.shape[0] - 1), np.clip(ix, 0, owner_t.shape[1] - 1)], -1)
    labels, n = ic.components(alpha > 0.5)
    votes = np.zeros((n + 1, count), np.int64)
    there = (labels > 0) & (place >= 0)
    np.add.at(votes, (labels[there], place[there]), 1)
    sizes = np.bincount(labels.ravel(), minlength=n + 1)
    whole = np.full(n + 1, -1, np.int64)
    split, joined, lost = np.zeros(n + 1, bool), set(), 0
    for lab in range(1, n + 1):
        v = votes[lab]
        total = v.sum()
        if total == 0:
            if sizes[lab] >= SPECK:
                lost += int(sizes[lab])
            continue
        order = np.argsort(-v)
        whole[lab] = order[0]
        if count > 1 and v[order[1]] >= max(SPECK, JOINED * total):
            split[lab] = True
            joined.add(tuple(sorted((int(order[0]), int(order[1])))))
    owner = whole[labels]
    cut_here = split[labels]
    owner[cut_here] = np.where(place[cut_here] >= 0, place[cut_here], owner[cut_here])
    owner[labels == 0] = -1
    # The soft edge (alpha under a half) goes with the shape beside it.
    soft = (alpha > 0) & (owner < 0) & (labels == 0)
    for _ in range(3):
        near = np.full((h, w), -1, np.int64)
        near[1:] = np.maximum(near[1:], owner[:-1])
        near[:-1] = np.maximum(near[:-1], owner[1:])
        near[:, 1:] = np.maximum(near[:, 1:], owner[:, :-1])
        near[:, :-1] = np.maximum(near[:, :-1], owner[:, 1:])
        fill = soft & (owner < 0) & (near >= 0)
        if not fill.any():
            break
        owner[fill] = near[fill]
    return owner, sorted(joined), lost


def cut(painted, who, out_dir=None, write=True, quiet=False, show=None):
    """A painted template back into its pieces at rest, each fitted to its own bone, and for a body
    the base drawn from them. With write False, nothing's written: the verdict on the picture is
    returned (and printed)."""
    base, armor = who_is(who)
    png, js = paths(base)
    tpl = load(armor_path(base) if armor else png)
    data = json.load(open(js, encoding="utf-8"))
    scale, origin = data["scale"], data["origin"]
    got = load(painted)
    w, h = CANVAS
    redo, fixed = [], []
    if got.shape[:2] != (h, w):
        fixed.append("it came back %dx%d, not %dx%d: scaled back" % (got.shape[1], got.shape[0], w, h))
        got = ic_resample_rgb(got, (0, 0, got.shape[1], got.shape[0]), (w, h))
    exact = np.array_equal(got, tpl)
    _, tpl_alpha = key(tpl)
    pieces = [b for b in PIECES if not (armor and b == "head")]
    guides = {bone: guide_on_template(tpl_alpha, data, bone) for bone in pieces}
    # The whole picture on the template, a body matched on the guides' own pixels, an armor on its
    # shape (its colours are the outfit's): template point t is painted at G t = (tx, ty) + s t.
    # Only where to start each piece's own fit, so it's found at 1/WHOLE_COARSE size.
    s, tx, ty, score = 1.0, 0.0, 0.0, 1.0
    if not exact:
        k = WHOLE_COARSE

        def shrunk(rgb):
            return np.stack([block_mean(rgb[:, :, i].astype(np.float64), k) for i in range(3)], axis=2)

        if armor:
            weight = block_mean(ic.grow(tpl_alpha > 0.5, 8).astype(np.float64), k)
            score, s, tx, ty = sheet_fit.fit(shrunk(silhouette(got)), np.ones((h // k, w // k)), shrunk(silhouette(tpl)), weight, near=1.0, span=0.08)
        else:
            weight = block_mean((tpl_alpha > 0.5).astype(np.float64), k)
            score, s, tx, ty = sheet_fit.fit(shrunk(got), np.ones((h // k, w // k)), shrunk(tpl), weight, near=1.0, span=0.08)
        tx, ty = tx * k, ty * k
    fx0, fy0, fx1, fy1 = ic_bbox(tpl_alpha > 0.5)
    drift = max(np.hypot(tx + (s - 1) * x, ty + (s - 1) * y) for x in (fx0, fx1) for y in (fy0, fy1))
    if score < (WHOLE_POOR_ARMOR if armor else WHOLE_POOR):
        redo.append("it doesn't match the template (%.2f): not the attached picture, or redrawn in another layout" % score)
    if drift > (WHOLE_BULK if armor else WHOLE_FAR):
        fixed.append("the whole picture was zoomed %.1f%% and moved %+.0f, %+.0f: put back" % ((s - 1.0) * 100, tx, ty))
    else:
        s, tx, ty = 1.0, 0.0, 0.0
    colour, alpha = key(got)
    # Each template pixel is the nearest guide's, up to REACH from it; each painted pixel is the
    # template's under it.
    owner_t = np.full((h, w), -1, int)
    dist = np.full((h, w), REACH + 1, int)
    for k, bone in enumerate(pieces):
        m = guides[bone].copy()
        for d in range(0, REACH + 1):
            newer = m & (dist > d)
            owner_t[newer] = k
            dist[newer] = d
            m = ic.grow(m, 1)
    back = (np.floor((np.arange(h) + 0.5 - ty) / s).astype(np.int64)[:, None] + np.zeros((1, w), np.int64),
            np.floor((np.arange(w) + 0.5 - tx) / s).astype(np.int64)[None, :] + np.zeros((h, 1), np.int64))
    owner, joined, lost = owners(alpha, owner_t, back, len(pieces))
    for a, b in joined:
        redo.append("the %s and the %s are painted touching: cut apart where they meet, but ask for the black gap back"
                    % (pieces[a].replace("_", " "), pieces[b].replace("_", " ")))
    painted_px = (alpha > 0.5).sum()
    if lost > SPILL_MOST * max(1, painted_px):
        redo.append("%.0f%% of it is painted where no piece is (between them, or where the head was): it'd be lost"
                    % (lost * 100.0 / max(1, painted_px)))
    if out_dir is None:
        out_dir = os.path.join(CHARS, "armors", armor) if armor else os.path.join(CHARS, "bodies", base)
    if write:
        os.makedirs(out_dir, exist_ok=True)
    report, rests = [], {}
    gys, gxs = (np.arange(h) + 0.5) * s + ty, (np.arange(w) + 0.5) * s + tx
    for k, bone in enumerate(pieces):
        name = bone.replace("_", " ")
        mine = alpha * (owner == k)
        g = tpl_alpha * ic.grow(guides[bone], 2)
        area = g.sum()
        if mine.sum() < (0.2 if armor else SETTLE_LEAST) * area:
            redo.append("the %s is missing or cut short (%.0f%% of its area)" % (name, mine.sum() * 100.0 / max(area, 1)))
            report.append("  %-13s %6d pixels; missing" % (bone, int((mine > 0.5).sum())))
            continue
        if (mine[:2] > 0.5).any() or (mine[-2:] > 0.5).any() or (mine[:, :2] > 0.5).any() or (mine[:, -2:] > 0.5).any():
            redo.append("the %s runs off the edge of the picture: it was cropped" % name)
        # The painted piece on the template's grid, where the whole picture's fit puts it.
        if s == 1.0 and tx == 0.0 and ty == 0.0:
            on_t = mine
        else:
            py, px = np.nonzero(mine > 0.02)
            X0, X1 = max(0, int((px.min() - tx) / s) - 3), min(w, int((px.max() + 1 - tx) / s) + 4)
            Y0, Y1 = max(0, int((py.min() - ty) / s) - 3), min(h, int((py.max() + 1 - ty) / s) + 4)
            on_t = np.zeros((h, w))
            on_t[Y0:Y1, X0:X1] = bilinear(mine, gxs[None, X0:X1], gys[Y0:Y1, None])
        gy, gx = np.nonzero(g > 0.5)
        centre = (float(gx.mean()) + 0.5, float(gy.mean()) + 0.5)
        if exact:
            size, turn, move, fit_score, edge = 1.0, 0.0, (0.0, 0.0), 1.0, False
        else:
            size, turn, move, fit_score, _, edge = fit_piece(g, on_t, centre, bool(armor))
        settled = abs(size - 1.0) <= SETTLE_SIZE and abs(turn) <= SETTLE_TURN and np.hypot(*move) <= SETTLE_PLACE
        if settled:
            A, b = similar(centre, 1.0, 0.0, (0.0, 0.0))
        else:
            A, b = similar(centre, size, turn, move)
        what = ("" if armor else "%+.1f%% in size, " % ((size - 1) * 100)) + "turned %+.1f degrees, moved %.1f pixels" % (turn, np.hypot(*move))
        if armor:
            X0, Y0, X1, Y1 = ic_bbox(g > 0)
            ys, xs = np.mgrid[Y0:Y1, X0:X1] + 0.5
            cover = (g[Y0:Y1, X0:X1] * bilinear(on_t, A[0, 0] * xs + A[0, 1] * ys + b[0], A[1, 0] * xs + A[1, 1] * ys + b[1])).sum() / max(area, 1e-9)
            note = "covers %3.0f%% of its body piece, shape %.3f" % (cover * 100, fit_score)
            if cover < COVER_LEAST:
                redo.append("the %s doesn't cover its body piece (%.0f%%): shrunk, moved too far or left out" % (name, cover * 100))
        else:
            note = "shape %.3f" % fit_score
            if fit_score < SHAPE_POOR:
                redo.append("the %s was redrawn in another shape (it overlaps its guide %.0f%% at best): bent, or not the piece"
                            % (name, fit_score * 100))
        if edge:
            redo.append("the %s is %s: further off than the cut looks, so it may be further still" % (name, what))
        elif not settled:
            fixed.append("the %s (%s)" % (name, what))
            note += "; fitted to its bone: " + what
        # Back to rest at full size: canvas point X is template point t0 = origin + scale (X + pull)
        # on the guide, painted at G (A t0 + b).
        pull = data["pieces"][bone]["pull"]
        py, px = np.nonzero(mine > 0.02)
        corners = np.array([[px.min(), py.min()], [px.max() + 1, py.min()], [px.min(), py.max() + 1], [px.max() + 1, py.max() + 1]], float)
        inv = np.linalg.inv(A)
        on_guide = ((corners - (tx, ty)) / s - b) @ inv.T
        rest = on_guide / scale - np.asarray(origin) / scale - np.asarray(pull)
        bx0, by0 = (int(max(0, np.floor(v) - 4)) for v in rest.min(axis=0))
        bx1, by1 = int(min(w, np.ceil(rest[:, 0].max()) + 4)), int(min(h, np.ceil(rest[:, 1].max()) + 4))
        X, Y = np.meshgrid(np.arange(bx0, bx1) + 0.5, np.arange(by0, by1) + 0.5)
        t0x, t0y = origin[0] + scale * (X + pull[0]), origin[1] + scale * (Y + pull[1])
        tx1 = A[0, 0] * t0x + A[0, 1] * t0y + b[0]
        ty1 = A[1, 0] * t0x + A[1, 1] * t0y + b[1]
        pm = np.concatenate([colour / 255.0 * mine[:, :, None], mine[:, :, None]], axis=2)
        crop = sample(pm, tx + s * tx1, ty + s * ty1, step=s * scale * (1.0 if settled else size))
        rests[bone] = (bx0, by0, crop)
        if write:
            full = np.zeros((h, w, 4), np.uint8)
            full[by0:by1, bx0:bx1] = ic.straight(crop)
            ic.write_png(os.path.join(out_dir, bone + ".png"), full)
        report.append("  %-13s %6d pixels; %s" % (bone, int((mine > 0.5).sum()), note))
    figure = np.zeros((h, w, 3))
    for bone in DRAW:
        if bone in rests:
            x0, y0, crop = rests[bone]
            ch, cw = crop.shape[:2]
            under = figure[y0:y0 + ch, x0:x0 + cw]
            figure[y0:y0 + ch, x0:x0 + cw] = crop[:, :, :3] + under * (1.0 - crop[:, :, 3:4])
    figure = np.rint(np.clip(figure, 0, 1) * 255)
    target = None
    if write and not armor:
        target = os.path.join(CHARS, base + ".png") if out_dir == os.path.join(CHARS, "bodies", base) else os.path.join(out_dir, base + ".png")
        save(target, figure)
    if show:
        save(show, review(figure, base))
    verdict = ("REDO: " + "; ".join(redo)) if redo else ("GOOD: " + ("; ".join(fixed) if fixed else "the template's size and layout, as it was"))
    if not quiet:
        if write:
            print("body_parts: cut %s (found at scale %.4f, %+.1f, %+.1f, match %.3f) into %s%s"
                  % (painted, s, tx, ty, score, os.path.relpath(out_dir, ROOT), (" and " + os.path.relpath(target, ROOT)) if target else ""))
        else:
            print("body_parts: %s against %s (found at scale %.4f, %+.1f, %+.1f, match %.3f)"
                  % (painted, os.path.basename(armor_path(base) if armor else png), s, tx, ty, score))
        print("\n".join(report))
        print(verdict)
    return verdict


def review(figure, base):
    """The figure stood up from a cut, beside the template's own stood up, each piece's outline
    from the template over the cut one: to see every piece sits where the skeleton has it."""
    ref = standing_stand_in(base)
    _, a = key(ref)
    edge = (a > 0.5) & ic.grow(a <= 0.5, 2)
    over = figure.copy()
    over[edge] = (90, 220, 255)
    return np.concatenate([over, np.zeros((figure.shape[0], 8, 3)), ref], axis=1)


def standing_stand_in(base):
    """The template's own pieces stood up at rest (the stand-in), whatever's painted."""
    target = os.path.join(STAND_IN, base, base + ".png")
    if not os.path.exists(target):
        cut(paths(base)[0], base, os.path.join(STAND_IN, base), quiet=True)
    return load(target)


def guide_on_template(tpl_alpha, data, bone):
    """Where a piece's guide is on the template: the template's own figure under the piece's centre
    (as the template recorded it), or the nearest to it."""
    labels, count = ic.components(tpl_alpha > 0.5)
    cx, cy = (int(round(v)) for v in data["pieces"][bone]["centre"])
    for reach in (0, 4, 12, 30):
        win = labels[max(0, cy - reach):cy + reach + 1, max(0, cx - reach):cx + reach + 1]
        ids, n = np.unique(win[win > 0], return_counts=True)
        if len(ids):
            return labels == ids[np.argmax(n)]
    return np.zeros(tpl_alpha.shape, bool)


def ic_resample_rgb(rgb, box, size):
    pm = np.concatenate([rgb / 255.0, np.ones(rgb.shape[:2] + (1,))], axis=2)
    out = ic.resample(pm, box, size)
    return np.rint(np.clip(out[:, :, :3], 0, 1) * 255).astype(np.uint8)


def main(argv):
    show = argv[argv.index("--show") + 1] if "--show" in argv else None
    args = [a for a in argv if a not in ("--show", show)]
    if len(args) == 3 and args[0] == "template":
        template(args[1], args[2])
    elif args == ["armor"]:
        for base in NAMES:
            armor_template(base)
    elif len(args) == 3 and args[0] == "check":
        verdict = cut(args[1], args[2], write=False, show=show)
        sys.exit(1 if verdict.startswith("REDO") else 0)
    elif len(args) in (3, 4) and args[0] == "cut":
        verdict = cut(args[1], args[2], args[3] if len(args) == 4 else None, show=show)
        if verdict.startswith("REDO"):
            print("body_parts: cut anyway; redo the picture before it goes in the game")
    else:
        sys.exit(__doc__)


if __name__ == "__main__":
    main(sys.argv[1:])
