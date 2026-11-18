"""
An armor painted whole, on a figure, and cut into the skeleton's pieces by the body under it.

    python tools/dress_pose.py template <base>                     # the figure to paint an outfit on
    python tools/dress_pose.py cut <painted.png> <base> <folder>   # a painted figure into its pieces
    python tools/dress_pose.py preview <base> <out.png> <folder> [<folder> ...] [--moving] [--side-foot]   # pieces posed

A trial of another way to make an armor than tools/body_parts.py's (thirteen pieces painted apart):
the outfit is painted once, whole, on the body stood in a "dressing pose", and the cut takes it
apart. Nothing has to be found in the painting to do it, because the body under it is known.

`template` stands the painted body's pieces (tools/source/characters/bodies/<base>/, less the head)
on the skeleton with the arms held out from the trunk and the legs a little apart (POSE), at the
biggest size that fits 1024 x 1536, on black: tools/source/characters/dress-pose-<man|woman>.png,
and where it put the figure in the .json beside it. In that pose almost nothing is behind anything
else: only the joints, the shoulders and the tops of the thighs.

`cut` takes that picture with an outfit painted on it:

  - every painted pixel is in some bone's place: on the body, the piece seen there; off it (a
    pauldron, a skirt's flare), the nearest piece;
  - the painting's own lines part it into areas (a plate, a strap, a sleeve). A small area goes
    whole to the bone most of it is on, and so does a bigger one that's mostly (WHOLE) on a bone
    drawn over the others it touches, so a pauldron stays one thing on the arm and a boot's cuff on
    the shin; any other is parted where the body's pieces meet (a sleeve at the elbow);
  - what hangs from the trunk over the legs (a skirt, a tabard, a coat's tails: it's painted across
    the gap between the thighs) stays whole on the torso, which is drawn over the legs, so the legs
    swing under it;
  - each piece then carries on under the pieces drawn over it, as far as its body piece does (and
    as much further as the outfit is thick there), in its own colours and with a line round its
    end. At rest that's all hidden, so the pieces put together are the painting again; when a joint
    bends, what shows is more of the same sleeve, with its own round end;
  - and each piece is put back on its bone at rest, on the base's canvas, as body_parts.py's are:
    <folder>/<bone>.png.

`preview` draws each folder's pieces (over the body's own) at rest and in a few of the game's poses;
--moving shows the jog and the run instead, and --side-foot tries the near foot drawn side on (the
far foot's picture at the near ankle), toes the way the figure's going.

Needs CPython, numpy and scipy.

Written by Caleb Kiatoukaysi for Legend Quest.
"""
import json
import os
import sys

import numpy as np
from scipy import ndimage as ndi

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
sys.path.insert(0, os.path.join(HERE, "rig"))
import import_character as ic  # noqa: E402
import skeleton as K  # noqa: E402

ROOT = os.path.dirname(HERE)
CHARS = os.path.join(ROOT, "tools", "source", "characters")
CANVAS = (1024, 1536)
NAMES = {"base-male": "man", "base-female": "woman"}
#: The pieces an outfit dresses, back to front.
PIECES = tuple(b for b in K.DRAW if b != "head")
RANK = {b: i for i, b in enumerate(PIECES)}
#: The dressing pose, degrees clockwise on top of the stance: the arms out, the legs a little apart.
POSE = {"arm_near_up": 30.0, "arm_far_up": -40.0, "leg_near_up": 4.0, "leg_far_up": -7.0}
MARGIN = 28
#: A painted line: darker than the lightest round it (within INK_WINDOW) by this share. The areas
#: are parted by the lines a pixel wider (a line's soft edge leaks otherwise).
INK_WINDOW, INK = 9, 0.2
#: An area under SMALL pixels goes whole to the bone most of it is on; a bigger one does when at
#: least WHOLE of it is on a bone drawn over every other it's on (a bone counts with LOSER of it).
SMALL, WHOLE, LOSER = 500, 0.6, 0.06
#: What hangs from the trunk: an area with at least HANG of it (and HANG_LEAST pixels) in the gap
#: between the legs, when at least GAP_FULL of the gap's first GAP_ROWS rows is painted; and the
#: areas beside it of its colour (the same hue within LIKE, lit or in shade: no more than LIKE_TONE
#: times lighter or darker) that end no lower than it does (HEM_SLACK).
HANG, HANG_LEAST, GAP_FULL, GAP_ROWS, LIKE, HEM_SLACK = 0.12, 60, 0.6, 50, 0.08, 14
#: It hangs from the trunk: at least this share of it is in the torso's place (trousers seen
#: between a skirt's panels aren't).
#: Off the body: at least this share of an area is outside the body's outline.
OFF_BODY = 0.4
FROM_TRUNK = 0.03
#: "Beside": within this many pixels (a line and its soft edges lie between two areas).
BESIDE = 12
LIKE_TONE = 2.2
#: A piece carries on under the ones over it as far past its body piece as the outfit is thick
#: (the 90th percentile of how far its own paint is from its body piece), at most this.
THICK_MOST = 26
#: The line round a piece's hidden end.
RIM = 3
#: What's carried on under is painted from the piece's nearest colours, smoothed this far.
FILL_SOFT = 3.0
LEGS = ("leg_near_up", "leg_near_low", "leg_far_up", "leg_far_low")


def paths(base):
    stem = os.path.join(CHARS, "dress-pose-%s" % NAMES[base])
    return stem + ".png", stem + ".json"


def body(base):
    """The painted body's pieces at rest: {bone: (colour 0-255, alpha 0-1)}."""
    out = {}
    for bone in K.DRAW:
        rgba = ic.read_png(os.path.join(CHARS, "bodies", base, bone + ".png"))[0].astype(np.float64)
        out[bone] = (rgba[:, :, :3], rgba[:, :, 3] / 255.0)
    return out


def warp(picture, m, shape):
    """`picture` (h, w[, c]) drawn through the 2x3 matrix `m` (its pixel to the new one's) onto a
    canvas of `shape` (h, w), bilinear, nothing outside it."""
    inv = np.linalg.inv(np.vstack([m, [0, 0, 1]]))
    h, w = shape
    yy, xx = np.mgrid[0:h, 0:w].astype(np.float64) + 0.5
    sx = inv[0, 0] * xx + inv[0, 1] * yy + inv[0, 2] - 0.5
    sy = inv[1, 0] * xx + inv[1, 1] * yy + inv[1, 2] - 0.5
    if picture.ndim == 2:
        return ndi.map_coordinates(picture, [sy, sx], order=1, cval=0.0)
    return np.stack([ndi.map_coordinates(picture[:, :, i], [sy, sx], order=1, cval=0.0) for i in range(picture.shape[2])], axis=2)


def posed(base, scale=1.0, origin=(0.0, 0.0)):
    """Each bone's matrix from the rest canvas to the dressing pose's picture."""
    out = {}
    for bone, m in K.world(base, POSE).items():
        m = m * scale
        m[:, 2] += origin
        out[bone] = m
    return out


def premul(colour, alpha):
    return np.concatenate([colour / 255.0 * alpha[:, :, None], alpha[:, :, None]], axis=2)


def stand(layers, mats, shape):
    """Pieces drawn through their matrices in their order: premultiplied RGBA."""
    out = np.zeros(shape + (4,))
    for bone in K.DRAW:
        if bone in layers and layers[bone] is not None:
            s = np.clip(warp(premul(*layers[bone]), mats[bone], shape), 0.0, 1.0)
            out = s + out * (1.0 - s[:, :, 3:])
    return out


def template(base):
    pieces = {b: v for b, v in body(base).items() if b != "head"}
    w, h = CANVAS
    # Where the pose puts the figure, on a canvas with room round it; then fitted to the picture.
    pad = 400
    big = stand(pieces, posed(base, 1.0, (pad, pad)), (h + 2 * pad, w + 2 * pad))[:, :, 3] > 0.02
    ys, xs = np.nonzero(big)
    x0, x1, y0, y1 = xs.min() - pad, xs.max() + 1 - pad, ys.min() - pad, ys.max() + 1 - pad
    scale = min(1.0, (w - 2 * MARGIN) / (x1 - x0), (h - 2 * MARGIN) / (y1 - y0))
    origin = ((w - scale * (x0 + x1)) / 2.0, (h - scale * (y0 + y1)) / 2.0)
    figure = stand(pieces, posed(base, scale, origin), (h, w))
    png, js = paths(base)
    ic.write_png(png, np.dstack([np.rint(np.clip(figure[:, :, :3], 0, 1) * 255).astype(np.uint8), np.full((h, w), 255, np.uint8)]))
    json.dump({"base": base, "pose": POSE, "scale": round(scale, 6), "origin": [round(v, 3) for v in origin]},
              open(js, "w", encoding="utf-8"), indent=1)
    print("dress_pose: wrote %s (the %s in the dressing pose, at %.3f of the canvas)" % (os.path.relpath(png, ROOT), NAMES[base], scale))


def luma(colour):
    return colour[:, :, 0] * 0.299 + colour[:, :, 1] * 0.587 + colour[:, :, 2] * 0.114


def key(rgb):
    import import_parts as ip
    return ip.key_black(rgb)


def places(base, data, shape):
    """Where the body is in the dressing pose: each piece's mask, the piece seen at each pixel
    (-1 off the body), the piece each pixel is nearest, and the matrices."""
    mats = posed(base, data["scale"], data["origin"])
    pieces = body(base)
    masks = {b: warp(pieces[b][1], mats[b], shape) > 0.5 for b in PIECES}
    seen = np.full(shape, -1, np.int64)
    for b in PIECES:
        seen[masks[b]] = RANK[b]
    _, (iy, ix) = ndi.distance_transform_edt(seen < 0, return_indices=True)
    return masks, seen, seen[iy, ix], mats


def gap_between_legs(masks):
    """The pixels between the two legs, below where they part, and the row they part at."""
    near = masks["leg_near_up"] | masks["leg_near_low"]
    far = masks["leg_far_up"] | masks["leg_far_low"]
    h, w = near.shape
    xs = np.arange(w)
    right_of_near = np.where(near, xs[None, :], -1).max(axis=1)
    left_of_far = np.where(far, xs[None, :], w).min(axis=1)
    rows = (right_of_near >= 0) & (left_of_far < w) & (left_of_far - right_of_near > 4)
    gap = rows[:, None] & (xs[None, :] > right_of_near[:, None]) & (xs[None, :] < left_of_far[:, None])
    gap &= ~np.any(list(masks.values()), axis=0)
    ys = np.nonzero(gap.any(axis=1))[0]
    return gap, (int(ys.min()) if len(ys) else h)


def like(these, one):
    """Which of `these` colours are `one`'s material: its hue, lit or in shade."""
    tone_a, tone_b = these.sum(axis=1) + 1.0, one.sum() + 1.0
    hue = np.abs(these / tone_a[:, None] - one / tone_b).sum(axis=1)
    return (hue < LIKE) & (tone_a / tone_b < LIKE_TONE) & (tone_b / tone_a < LIKE_TONE)


def owners(colour, alpha, masks, seen, near, shoulder=None, neck=None):
    """The bone each painted pixel goes with (-1 none), and the painting's lines."""
    figure = alpha > 0.5
    light = luma(colour)
    closed = ndi.grey_closing(light, size=(INK_WINDOW, INK_WINDOW))
    ink = figure & ((closed - light) / (closed + 1.0) > INK)
    areas, n = ndi.label(figure & ~ndi.binary_dilation(ink))
    count = len(PIECES)
    there = areas > 0
    votes = np.bincount(areas[there] * count + near[there], minlength=(n + 1) * count).reshape(n + 1, count)
    size = votes.sum(axis=1)
    top = votes.argmax(axis=1)
    share = votes.max(axis=1) / np.maximum(size, 1)
    counts = votes >= np.maximum(60, LOSER * size)[:, None]
    counts[np.arange(n + 1), top] = False
    over = np.where(counts, np.arange(count)[None, :], -1).max(axis=1) < top
    whole = (size < SMALL) | (over & (share >= WHOLE))
    own = np.where(whole[areas], top[areas], near)
    own[~there] = -1
    # On the body, a pixel is never a piece's that's drawn under the piece seen there: every piece
    # covers its own body piece, itself or under what's over it.
    own = np.where(there & (seen >= 0) & (own < seen), seen, own)
    # The whole shoulder: what stands off the body beside the near arm's own (a pauldron's upper
    # plate, its rim), above the shoulder and nearer it than the neck, goes with the arm, which is drawn over
    # the trunk, so the pauldron is one thing and turns with the arm.
    worn, on_shoulder = 0, np.zeros(n + 1, bool)
    if shoulder is not None:
        index = np.arange(1, n + 1)
        ys, xs = np.mgrid[0:areas.shape[0], 0:areas.shape[1]]
        cy = np.concatenate([[0.0], ndi.mean(ys, areas, index)])
        cx = np.concatenate([[0.0], ndi.mean(xs, areas, index)])
        off_body = np.bincount(areas[there & (seen < 0)], minlength=n + 1) >= OFF_BODY * size
        by_shoulder = np.hypot(cx - shoulder[0], cy - shoulder[1]) < np.hypot(cx - neck[0], cy - neck[1])
        arm, trunk = RANK["arm_near_up"], RANK["torso"]
        could = off_body & by_shoulder & (cy <= shoulder[1])
        could[0] = False
        on_arm = (own == arm) & there
        while True:
            beside = np.unique(areas[ndi.binary_dilation(on_arm, iterations=BESIDE) & there & (own == trunk)])
            new = [a for a in beside if could[a]]
            if not new:
                break
            took = np.isin(areas, new) & (own == trunk)
            own[took] = arm
            on_arm |= took
            could[new] = False
            on_shoulder[new] = True
            worn += len(new)
    # What hangs from the trunk over the legs.
    gap, crotch = gap_between_legs(masks)
    first = gap & (np.arange(gap.shape[0])[:, None] < crotch + GAP_ROWS)
    hangs = np.zeros(n + 1, bool)
    if first.any() and (figure & first).sum() >= GAP_FULL * first.sum():
        in_gap = np.bincount(areas[there & gap], minlength=n + 1)
        from_trunk = votes[:, RANK["torso"]] >= np.maximum(40, FROM_TRUNK * size)
        hangs = (in_gap >= np.maximum(HANG_LEAST, HANG * size)) & from_trunk
        hangs[0] = False
        index = np.arange(1, n + 1)
        mean = np.stack([np.concatenate([[0.0], ndi.mean(colour[:, :, i], areas, index)]) for i in range(3)], axis=1)
        bottom = np.concatenate([[0.0], ndi.maximum(np.broadcast_to(np.arange(gap.shape[0])[:, None], gap.shape), areas, index)])
        leggy = np.isin(top, [RANK[b] for b in LEGS] + [RANK["torso"]])
        while hangs.any():
            hem = bottom[hangs].max()
            beside = np.unique(areas[ndi.binary_dilation(hangs[areas], iterations=BESIDE) & there])
            new = [a for a in beside if not hangs[a] and leggy[a] and bottom[a] <= hem + HEM_SLACK
                   and like(mean[np.nonzero(hangs)[0]], mean[a]).any()]
            if not new:
                break
            hangs[new] = True
        # And the scraps among them (a pleat's shade, a rivet): areas mostly inside what hangs, closed up.
        if hangs.any():
            closed = ndi.binary_closing(hangs[areas], iterations=BESIDE) & there
            inside = np.bincount(areas[closed], minlength=n + 1)
            hangs |= leggy & (inside >= 0.6 * size) & (bottom <= bottom[hangs].max() + HEM_SLACK)
            hangs[0] = False
        hangs &= ~on_shoulder
        own[hangs[areas]] = RANK["torso"]
    # The lines and the soft edge go with the area beside them.
    _, (iy, ix) = ndi.distance_transform_edt(~there, return_indices=True)
    own = np.where(alpha > 0, own[iy, ix], -1)
    return own, ink, int(hangs.sum()), worn


def cut(painted, base, out_dir, quiet=False):
    """A figure painted on the dressing pose into its pieces at rest: <out_dir>/<bone>.png."""
    png, js = paths(base)
    data = json.load(open(js, encoding="utf-8"))
    w, h = CANVAS
    got = ic.read_png(painted)[0][:, :, :3]
    if got.shape[:2] != (h, w):
        pm = np.concatenate([got / 255.0, np.ones(got.shape[:2] + (1,))], axis=2)
        got = np.rint(np.clip(ic.resample(pm, (0, 0, got.shape[1], got.shape[0]), (w, h))[:, :, :3], 0, 1) * 255)
    colour, alpha = key(got.astype(np.uint8))
    masks, seen, near, mats = places(base, data, (h, w))
    spot = lambda joint: mats["torso"] @ np.array([K.JOINTS[base][joint][0], K.JOINTS[base][joint][1], 1.0])  # noqa: E731
    own, ink, hanging, worn = owners(colour, alpha, masks, seen, near, spot("shoulder_near"), spot("neck"))
    figure = alpha > 0.5
    dark = np.median(colour[ink], axis=0) if ink.any() else np.array([40.0, 25.0, 20.0])
    os.makedirs(out_dir, exist_ok=True)
    report = []
    for bone in PIECES:
        mine = own == RANK[bone]
        solid = mine & figure
        a = np.where(mine, alpha, 0.0)
        c = colour.copy()
        under = np.zeros((h, w), bool)
        off = ndi.distance_transform_edt(~masks[bone])
        outside = off[solid & ~masks[bone]]
        thick = min(THICK_MOST, float(np.percentile(outside, 90))) if len(outside) > 50 else 0.0
        under = (off <= thick) & figure & ~mine & (own > RANK[bone])
        # Its colours: its own, or (all of it hidden: a thigh under a skirt) the next piece's along.
        seeds = np.zeros((h, w), bool)
        for other in [bone] + [b for b, p, _ in K.BONES if p == bone] + [K.BONE_OF[bone][0]]:
            if other in RANK:
                seeds = (own == RANK[other]) & figure & ~ink
                if seeds.sum() >= 40:
                    break
        if under.any() and seeds.any():
            _, (iy, ix) = ndi.distance_transform_edt(~seeds, return_indices=True)
            fill = colour[iy, ix] * under[:, :, None]
            weight = np.maximum(ndi.gaussian_filter(under.astype(np.float64), FILL_SOFT), 1e-6)
            fill = np.stack([ndi.gaussian_filter(fill[:, :, i], FILL_SOFT) for i in range(3)], axis=2) / weight[:, :, None]
            c = np.where(under[:, :, None], fill, c)
            rim = under & ~ndi.binary_erosion(mine | under, iterations=RIM)
            c[rim] = dark
            a = np.where(under, 1.0, a)
        else:
            under[:] = False
        rest = np.clip(warp(premul(c, a), np.linalg.inv(np.vstack([mats[bone], [0, 0, 1]]))[:2], (h, w)), 0.0, 1.0)
        ic.write_png(os.path.join(out_dir, bone + ".png"), ic.straight(rest))
        report.append("  %-13s %6d painted, %6d carried on under the pieces over it" % (bone, int(solid.sum()), int(under.sum())))
    if not quiet:
        print("dress_pose: cut %s into %s%s" % (painted, os.path.relpath(out_dir, ROOT),
                                                 ("; %d areas hang from the trunk over the legs" % hanging if hanging else "")
                                               + ("; %d areas off the near shoulder go with the arm" % worn if worn else "")))
        print("\n".join(report))


def layers_of(base, folder, bodies, side_foot=False):
    """A folder's pieces at rest, each over the body's own; the head is the body's. With
    `side_foot`, the near foot is the far foot's picture (the one drawn side on, toes the way the
    figure faces) moved to the near ankle, a little bigger for being nearer."""
    out = {}
    for bone in K.DRAW:
        c, a = bodies[bone]
        path = os.path.join(folder, bone + ".png")
        if bone != "head" and os.path.exists(path):
            rgba = ic.read_png(path)[0].astype(np.float64)
            ca, aa = rgba[:, :, :3], rgba[:, :, 3] / 255.0
            both = aa + a * (1.0 - aa)
            c = (ca * aa[:, :, None] + c * (a * (1.0 - aa))[:, :, None]) / np.maximum(both, 1e-6)[:, :, None]
            a = both
        out[bone] = (c, a)
    if side_foot:
        (nx, ny), (fx, fy) = K.JOINTS[base]["ankle_near"], K.JOINTS[base]["ankle_far"]
        m = np.array([[SIDE_FOOT, 0.0, nx - SIDE_FOOT * fx], [0.0, SIDE_FOOT, ny - SIDE_FOOT * fy]])
        c, a = out["foot_far"]
        pm = np.clip(warp(premul(c, a), m, a.shape), 0.0, 1.0)
        out["foot_near"] = (pm[:, :, :3] / np.maximum(pm[:, :, 3:], 1e-6) * 255.0, pm[:, :, 3])
    return out


#: The side-on near foot's size against the far foot's.
SIDE_FOOT = 1.08
#: The poses a preview shows: (the label, the clip, the time in it), or a pose itself.
SHOTS = (("rest", None, 0.0), ("walk", "walk@%s", 0.25), ("walk", "walk@%s", 0.8), ("run", "run@%s", 0.2),
         ("strike", "strike-one-hand", 0.12), ("cast", "cast", 0.25), ("kneel", "kneel", 0.8), ("cheer", "cheer", 0.5))

#: The jog and the run, for looking at the lean and the feet.
MOVING = (("jog", "jog@%s", 0.0), ("jog", "jog@%s", 0.18), ("jog", "jog@%s", 0.35), ("jog", "jog@%s", 0.53),
          ("run", "run@%s", 0.0), ("run", "run@%s", 0.15), ("run", "run@%s", 0.3), ("run", "run@%s", 0.45))


def preview(base, out, folders, shots=SHOTS, side_foot=False):
    import clips as C
    doc = json.load(open(os.path.join(ROOT, "Content", "Characters", "Parts", "clips.json"), encoding="utf-8"))
    known = doc["rigs"]["humanoid"]["clips"]
    bodies = body(base)
    w, h = CANVAS
    rows = []
    for folder in folders:
        layers = layers_of(base, folder, bodies, side_foot)
        row = []
        for _, name, t in shots:
            pose = C.sample(known[name % base if name and "%s" in name else name], t) if name else {}
            shot = K.render(base, layers, pose, size=(w + 240, h), origin=(120, 0))
            row.append(shot[::2, ::2, :3])
        rows.append(np.concatenate(row, axis=1))
    sheet = np.concatenate(rows, axis=0)
    ic.write_png(out, np.dstack([np.rint(np.clip(sheet, 0, 1) * 255).astype(np.uint8),
                                 np.full(sheet.shape[:2], 255, np.uint8)]))
    print("dress_pose: wrote", out)


def main(argv):
    if len(argv) == 2 and argv[0] == "template":
        template(argv[1])
    elif len(argv) == 4 and argv[0] == "cut":
        cut(argv[1], argv[2], argv[3])
    elif len(argv) >= 4 and argv[0] == "preview":
        side = "--side-foot" in argv
        moving = "--moving" in argv
        preview(argv[1], argv[2], [a for a in argv[3:] if not a.startswith("--")], MOVING if moving else SHOTS, side)
    else:
        sys.exit(__doc__)


if __name__ == "__main__":
    main(sys.argv[1:])
