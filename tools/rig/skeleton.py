"""
The skeleton the painted mannequins are animated on: every joint of the two bases
(tools/source/characters/base-male.png and base-female.png), the bones between them, and which
piece of the figure each bone carries. tools/import_parts.py cuts every body and armor into these
pieces, and the game turns them about their joints to play the clips (tools/rig/clips.py).

    python tools/rig/skeleton.py <folder>    # draws each base's skeleton over it into <folder>

The joints are on the bases' 1024 x 1536 canvas, measured by eye on the figures and checked by the
drawings this writes. The figure faces right in three-quarter view: the near arm and the near leg
are the ones on the left of the picture, nearer us, drawn in front of the body; the far ones are on
the right, behind it.

Only needs CPython and numpy.
"""

import os
import sys

import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
from vector import read_png, write_png  # noqa: E402

sys.path.insert(0, os.path.dirname(HERE))
import import_character as ic  # noqa: E402  (its connected pieces)

ROOT = os.path.normpath(os.path.join(HERE, "..", ".."))
CHARS = os.path.join(ROOT, "tools", "source", "characters")

#: The bones, parents first: (bone, parent, the joint it turns about). The joint names are the
#: JOINTS keys. The names are the old cut-out rig's, so its clips (tools/rig/clips.py) play on it.
BONES = (
    ("root", None, "root"),
    ("hips", "root", "hips"),
    ("spine", "hips", "spine"),
    ("chest", "spine", "chest"),
    ("head", "chest", "neck"),
    ("arm_far_up", "chest", "shoulder_far"),
    ("arm_far_low", "arm_far_up", "elbow_far"),
    ("hand_far", "arm_far_low", "wrist_far"),
    ("arm_near_up", "chest", "shoulder_near"),
    ("arm_near_low", "arm_near_up", "elbow_near"),
    ("hand_near", "arm_near_low", "wrist_near"),
    ("leg_far_up", "hips", "hip_far"),
    ("leg_far_low", "leg_far_up", "knee_far"),
    ("foot_far", "leg_far_low", "ankle_far"),
    ("leg_near_up", "hips", "hip_near"),
    ("leg_near_low", "leg_near_up", "knee_near"),
    ("foot_near", "leg_near_low", "ankle_near"),
)

#: Every joint on each base, (x, y) on the canvas: the middle of each painted ball (the jointed
#: mannequins of 2026-11-09), the hips inside the pelvis over each thigh, the trunk's down its middle.
JOINTS = {
    "base-male": {
        "root": (483, 1466), "hips": (540, 700), "spine": (540, 590), "chest": (545, 460), "neck": (548, 325),
        "shoulder_near": (390, 403), "elbow_near": (311, 595), "wrist_near": (292, 758),
        "shoulder_far": (625, 400), "elbow_far": (648, 590), "wrist_far": (712, 752),
        "hip_near": (470, 745), "knee_near": (412, 985), "ankle_near": (313, 1338),
        "hip_far": (597, 752), "knee_far": (597, 988), "ankle_far": (590, 1320),
    },
    "base-female": {
        "root": (500, 1466), "hips": (545, 695), "spine": (540, 600), "chest": (540, 470), "neck": (532, 355),
        "shoulder_near": (428, 414), "elbow_near": (362, 596), "wrist_near": (320, 762),
        "shoulder_far": (612, 415), "elbow_far": (640, 600), "wrist_far": (690, 770),
        "hip_near": (480, 735), "knee_near": (412, 1022), "ankle_near": (325, 1336),
        "hip_far": (600, 738), "knee_far": (590, 998), "ankle_far": (600, 1328),
    },
}

#: Each painted ball's radius on the canvas: an upper arm's top is kept round to it (so it looks the
#: same however far the arm turns), and a lower piece carries its parent round it (so a bend never
#: opens a gap at the joint).
BALL = {
    "base-male": {"shoulder_near": 60, "shoulder_far": 44, "elbow_near": 38, "elbow_far": 34, "wrist_near": 30,
                  "wrist_far": 28, "knee_near": 42, "knee_far": 40, "ankle_near": 34, "ankle_far": 30, "neck": 40},
    "base-female": {"shoulder_near": 44, "shoulder_far": 36, "elbow_near": 28, "elbow_far": 26, "wrist_near": 26,
                    "wrist_far": 24, "knee_near": 32, "knee_far": 34, "ankle_near": 26, "ankle_far": 26, "neck": 34},
}

#: Where each limb ends, for its last bone (the hand's and the foot's tips), on the canvas.
TIPS = {
    "base-male": {"hand_near": (318, 858), "hand_far": (725, 845), "foot_near": (325, 1458), "foot_far": (745, 1415)},
    "base-female": {"hand_near": (322, 858), "hand_far": (715, 848), "foot_near": (340, 1460), "foot_far": (740, 1415)},
}

BONE_OF = {b: (parent, joint) for b, parent, joint in BONES}

#: The crown of each base's head (where its figure's height is measured from, to the soles at the
#: root), and from it the canvas pixels in one rig px: the clips (clips.py) are written for a figure
#: 161.5 rig px from soles to crown, so an offset in them is that share of the figure's height.
CROWN = {"base-male": 115, "base-female": 150}
RIG_HEIGHT = 161.5
RIG_PX = {base: (JOINTS[base]["root"][1] - CROWN[base]) / RIG_HEIGHT for base in JOINTS}


def bone_line(base, bone):
    """A bone's two ends on the canvas: its joint, and its first child's joint (or its tip)."""
    joints = JOINTS[base]
    start = joints[BONE_OF[bone][1]]
    child = next((b for b, p, _ in BONES if p == bone and b not in ("head",) and not b.startswith(("arm_", "leg_"))), None)
    ends = {"hips": "spine", "spine": "chest", "chest": "neck", "arm_near_up": "elbow_near", "arm_near_low": "wrist_near",
            "arm_far_up": "elbow_far", "arm_far_low": "wrist_far", "leg_near_up": "knee_near", "leg_near_low": "ankle_near",
            "leg_far_up": "knee_far", "leg_far_low": "ankle_far"}
    if bone in ends:
        return start, joints[ends[bone]]
    if bone in TIPS[base]:
        return start, TIPS[base][bone]
    return start, start if child is None else joints[BONE_OF[child][1]]


# --------------------------------------------------------------------------- #
# Cutting a figure into its pieces
# --------------------------------------------------------------------------- #

#: The pieces, in the groups that are in front of one another, front first: the near arm over
#: everything, then the near leg, the trunk, the far leg, and the far arm behind it all.
GROUPS = (
    ("hand_near", "arm_near_low", "arm_near_up"),
    ("chest", "spine", "hips"),
    ("foot_near", "leg_near_low", "leg_near_up"),
    ("foot_far", "leg_far_low", "leg_far_up"),
    ("hand_far", "arm_far_low", "arm_far_up"),
)
TRUNK = GROUPS[1]
#: Back to front, the order the pieces are drawn in at rest. The head (the face, the hair or helm)
#: goes with the chest; what the hands hold goes under the hand that holds it.
DRAW = ("arm_far_up", "arm_far_low", "hand_far", "leg_far_up", "leg_far_low", "foot_far", "leg_near_up", "leg_near_low",
        "foot_near", "hips", "spine", "chest", "head", "arm_near_up", "arm_near_low", "hand_near")
#: The trunk keeps the figure down to here (y on the canvas), the hem of the shorts: the pelvis is
#: drawn over the tops of both legs, the way the shorts are over the thighs, and a leg's top slides
#: under it as the leg turns.
HEM = {"base-male": ((400, 758), (470, 785), (535, 806), (600, 795), (660, 772)),
       "base-female": ((418, 705), (480, 750), (530, 780), (570, 792), (600, 765), (628, 700))}
#: A limb keeps only the painted areas joined to its own bone without crossing an outline (darker
#: than this share of the figure's average lightness), and then the outline round them, OUTLINE_REACH
#: pixels deep: an arm and the body it touches are parted by the line drawn between them.
OUTLINE_DARK = 0.5
OUTLINE_REACH = 3

#: How far round each bone its piece reaches, on the canvas: the limb's half width and a little.
#: The trunk's is round the line from the hips to the neck.
RADIUS = {
    "base-male": {"arm_near_up": 58, "arm_near_low": 46, "hand_near": 64, "arm_far_up": 50, "arm_far_low": 44, "hand_far": 56,
                  "leg_near_up": 72, "leg_near_low": 54, "foot_near": 66, "leg_far_up": 68, "leg_far_low": 50, "foot_far": 70,
                  "trunk": 122},
    "base-female": {"arm_near_up": 46, "arm_near_low": 36, "hand_near": 56, "arm_far_up": 42, "arm_far_low": 36, "hand_far": 52,
                    "leg_near_up": 64, "leg_near_low": 46, "foot_near": 60, "leg_far_up": 60, "leg_far_low": 44, "foot_far": 64,
                    "trunk": 112},
}
#: A child piece carries a disc of its parent round its joint, this share of the joint's ball (BALL),
#: so a bend never opens a gap there: the disc turns with the child, which is drawn over the parent.
CAP = 1.0


class Cut:
    """The geometry a base's figures are cut with at a scale of its canvas (the parts' PART_SCALE,
    say): each bone's distance and place along its line for every pixel, worked out once and kept,
    so cutting a body and a hundred armors costs one set of them."""

    _kept = {}

    def __init__(self, base, scale, shape):
        self.base, self.scale, self.shape = base, scale, shape
        h, w = shape
        yy, xx = np.mgrid[0:h, 0:w].astype(np.float32)
        self.dist, self.along = {}, {}
        for bone in [b for g in GROUPS for b in g] + ["trunk"]:
            if bone == "trunk":
                (x0, y0), (x1, y1) = JOINTS[base]["hips"], JOINTS[base]["neck"]
            else:
                (x0, y0), (x1, y1) = bone_line(base, bone)
            x0, y0, x1, y1 = x0 * scale, y0 * scale, x1 * scale, y1 * scale
            dx, dy = x1 - x0, y1 - y0
            t = ((xx - x0) * dx + (yy - y0) * dy) / max(1e-6, dx * dx + dy * dy)
            tc = np.clip(t, 0.0, 1.0)
            self.dist[bone] = np.hypot(xx - (x0 + tc * dx), yy - (y0 + tc * dy))
            self.along[bone] = t
        self.reach = {k: v * scale for k, v in RADIUS[base].items()}
        self.rows = yy
        hem = np.array(HEM[base], np.float64) * scale
        self.hem = np.interp(xx, hem[:, 0], hem[:, 1])

    @classmethod
    def of(cls, base, scale, shape):
        key = (base, scale, shape)
        if key not in cls._kept:
            cls._kept[key] = cls(base, scale, shape)
        return cls._kept[key]

    def joint(self, name):
        x, y = JOINTS[self.base][name]
        return x * self.scale, y * self.scale

    def ball(self, joint):
        """A joint's painted ball's radius at the cut's scale (the trunk's joints: a share of its reach)."""
        return BALL[self.base].get(joint, RADIUS[self.base]["trunk"] * 0.5) * self.scale


#: A limb's first bone (the upper arm, the thigh) reaches only this share of its length back past its
#: joint into the body, so it doesn't carry a piece of the chest or the hips; the joint's cap (CAP)
#: covers the bend instead.
BACK_PAST_JOINT = 0.06
#: A piece is the parts of its reach that touch its own bone (within this share of its reach of the
#: line): a scrap of a thigh beside a fist, or of the chest beside a shoulder, isn't the arm's.
ON_BONE = 0.3
FIRST = ("arm_near_up", "arm_far_up", "leg_near_up", "leg_far_up")
#: What a limb's piece fills in under the pieces in front of it reaches only this share of its reach,
#: so the fill stays inside the limb rather than spreading to its edge.
HIDDEN_SHARE = 0.75
#: The round end of a limb's first bone: the circle about its joint, this share of its reach, that
#: its top (the first END_ZONE of its length) is kept to.
JOINT_ROUND = 1.08
END_ZONE = 0.3
#: A scrap of the shoulder thinner than twice this (in pixels at the cut's scale) goes with the arm.
SPECK = 3
#: How much wider than its reach each trunk piece fills in under a limb: the chest out to the
#: shoulders' round, the hips to the top of the thighs.
TRUNK_FILL = {"chest": 1.6, "spine": 1.6, "hips": 1.3}
#: The fill takes its colours from the piece's own inside, not its outline: pixels at least this
#: share as light as the piece's own pixels are on average.
SEED_LIGHT = 0.45


#: The far arm is painted in shadow: darker than SHADE_DARK of the figure's middle lightness, and
#: warmer (red over green by SHADE_WARM) than the dark underwear. That shade, where it's joined to the
#: far arm's bones, is the far arm's even where the chest's reach covers it.
SHADE_DARK = 0.65
SHADE_WARM = 1.4
#: Inside a limb, an area its painted lines close round (a ball, the forearm between its seams) goes
#: whole to the bone that most of it (this share) is nearest, so the cut follows the painted seams.
SEAM_SHARE = 0.7


def far_arm(cut, figure, colour, luma):
    c = colour.astype(np.float64)
    shade = figure & (luma < SHADE_DARK * np.median(luma[figure])) & (c[:, :, 0] > SHADE_WARM * np.maximum(c[:, :, 1], 1.0))
    k = max(1, int(round(2 * cut.scale)))
    shade = ic.grow(~ic.grow(~shade, k), k)
    shade = ~ic.grow(~ic.grow(shade, k), k)
    pieces, count = ic.components(shade)
    if not count:
        return shade
    arm = GROUPS[-1]
    spine = np.any([cut.dist[b] <= cut.reach[b] * ON_BONE for b in arm], axis=0) & shade
    keep = np.unique(pieces[spine])
    mine = np.isin(pieces, keep[keep > 0])
    return ic.grow(mine, max(1, int(round(OUTLINE_REACH * cut.scale)))) & figure


def by_seams(pieces, count, lit, nearest, bones):
    """`nearest` (each pixel's nearest bone in a group) with each area of `pieces` (its painted lines'
    areas, numbered 1 to count) that's lit and SEAM_SHARE or more nearest one bone given wholly to it."""
    ids = np.where(lit, pieces, 0).ravel()
    tally = np.bincount(ids * bones + nearest.ravel(), minlength=(count + 1) * bones).reshape(count + 1, bones)
    total = tally.sum(axis=1)
    best = tally.argmax(axis=1)
    whole = np.where((total > 0) & (tally.max(axis=1) >= SEAM_SHARE * np.maximum(total, 1)), best, -1)
    whole[0] = -1
    given = whole[np.where(lit, pieces, 0)]
    return np.where(given >= 0, given, nearest)


def labels(cut, figure, colour):
    """Which piece each pixel of a figure (a boolean mask on the cut's canvas) is: {bone: mask}. Each
    group in GROUPS, front first, takes what's in its pieces' reach, that no group in front has taken
    and that's joined to one of its bones; each pixel goes to the nearest of its bones. What no group
    takes goes to the nearest bone of all."""
    taken = np.zeros(figure.shape, bool)
    luma = colour[:, :, :3].astype(np.float64) @ np.array([0.299, 0.587, 0.114])
    lit_all = figure & (luma >= luma[figure].mean() * OUTLINE_DARK)
    far = far_arm(cut, figure, colour, luma)
    out = {}
    for group in GROUPS:
        if group is TRUNK:
            inside = figure & ~taken & (cut.dist["trunk"] <= cut.reach["trunk"]) & (cut.rows <= cut.hem)
        else:
            near = []
            for b in group:
                r = cut.dist[b] <= cut.reach[b]
                if b in FIRST:
                    r &= cut.along[b] >= -BACK_PAST_JOINT
                near.append(r)
            inside = figure & ~taken & np.any(near, axis=0)
            # Only what's joined to the limb's own bones without crossing a line, and its outline.
            lit = inside & lit_all
            pieces, count = ic.components(lit)
            if count:
                spine = np.any([cut.dist[b] <= cut.reach[b] * ON_BONE for b in group], axis=0) & lit
                keep = np.unique(pieces[spine])
                kept = np.isin(pieces, keep[keep > 0])
                inside &= kept | (ic.grow(kept, OUTLINE_REACH) & ~lit)
        if group is not GROUPS[-1]:
            inside &= ~far
        nearest = np.argmin([cut.dist[b] for b in group], axis=0)
        if group is not TRUNK and count:
            nearest = by_seams(pieces, count, lit & inside, nearest, len(group))
        for k, b in enumerate(group):
            out[b] = inside & (nearest == k)
        taken |= inside
    rest = figure & ~taken
    if rest.any():
        names = [b for g in GROUPS for b in g]
        nearest = np.argmin([cut.dist[b] for b in names], axis=0)
        for k, b in enumerate(names):
            out[b] |= rest & (nearest == k)
    # A limb's top end is round, about its joint, so it looks the same however far it turns: what of
    # the shoulder or the hip lies outside the circle stays with the trunk.
    nearest_trunk = np.argmin([cut.dist[t] for t in TRUNK], axis=0)
    h, w = figure.shape
    yy, xx = np.mgrid[0:h, 0:w]
    cx, cy = cut.joint("chest")
    for b in ("arm_near_up", "arm_far_up"):
        jx, jy = cut.joint(BONE_OF[b][1])
        around = np.hypot(xx - jx, yy - jy)
        # What's on the body's side of the arm, or above its joint (the top of the shoulder): the
        # outer edge of the arm below the joint is the arm's own.
        (x0, y0), (x1, y1) = bone_line(cut.base, b)
        dx, dy = x1 - x0, y1 - y0
        side = np.sign((cx / cut.scale - x0) * dy - (cy / cut.scale - y0) * dx)
        inner = ((xx / cut.scale - x0) * dy - (yy / cut.scale - y0) * dx) * side > 0
        outside = out[b] & (cut.along[b] < END_ZONE) & (around > cut.ball(BONE_OF[b][1]) * JOINT_ROUND) & (inner | (cut.along[b] < 0))
        out[b] &= ~outside
        for k, t in enumerate(TRUNK):
            out[t] |= outside & (nearest_trunk == k)
        # What that leaves of the shoulder's outline too thin to stand on its own goes back to the arm.
        zone = around <= cut.reach[b] * 1.6
        for t in TRUNK:
            part = out[t] & zone
            thin = part & ~(ic.grow(~ic.grow(~part, SPECK), SPECK))
            out[t] &= ~thin
            out[b] |= thin
    trim_inner(cut, figure, out, nearest_trunk)
    strays(out, nearest_trunk)
    body_edges(cut, out, nearest_trunk)
    return out


#: How far across the trunk's edge, in pixels at full size, the far arm's inner side is the body's.
FAR_SIDE = 34


def body_edges(cut, out, nearest_trunk):
    """What the limbs behind the trunk have of the body's own edge goes back to it, so a limb that
    swings away doesn't take a strip of the body with it (the limb fills in what the body then hides
    of it, from its own colours): each leg's top above the shorts' hem, and the far arm's inner side
    along the body, below its shoulder's round, FAR_SIDE across."""
    h, w = nearest_trunk.shape
    yy, xx = np.mgrid[0:h, 0:w]
    body = np.any([out[t] for t in TRUNK], axis=0)
    moved = np.zeros((h, w), bool)
    for b in ("leg_near_up", "leg_far_up"):
        above = out[b] & (cut.rows <= cut.hem)
        out[b] &= ~above
        moved |= above
    jx, jy = cut.joint("shoulder_far")
    clear_of_shoulder = np.hypot(xx - jx, yy - jy) > cut.ball("shoulder_far") * JOINT_ROUND
    side = ic.grow(body, max(1, int(round(FAR_SIDE * cut.scale)))) & clear_of_shoulder & (cut.rows <= cut.hem)
    for b in ("arm_far_up", "arm_far_low"):
        edge = out[b] & side
        out[b] &= ~edge
        moved |= edge
    for k, t in enumerate(TRUNK):
        out[t] |= moved & (nearest_trunk == k)


#: A scrap of a limb's piece apart from the rest of it, smaller than this share of the piece, is the
#: body's: left on the arm it would float off the shoulder as the arm turned.
STRAY = 0.03


def strays(out, nearest_trunk):
    """Gives each limb piece's stray scraps (8-connected, apart from its biggest part and smaller
    than STRAY of it) to the nearest trunk piece."""
    for b in [b for g in GROUPS if g is not TRUNK for b in g]:
        grown = ic.grow(out[b], 1) & ~out[b]
        parts, count = ic.components(out[b] | grown)
        if count <= 1:
            continue
        sizes = np.bincount(parts[out[b]], minlength=count + 1)
        keep = int(np.argmax(sizes))
        small = np.nonzero((sizes > 0) & (sizes < STRAY * sizes.sum()))[0]
        stray = out[b] & np.isin(parts, small[small != keep])
        if not stray.any():
            continue
        out[b] &= ~stray
        for k, t in enumerate(TRUNK):
            out[t] |= stray & (nearest_trunk == k)


#: Under the armpit the near arm is only as wide as it shows lower down: where its inner side lies
#: against the body, what's further in than its width where that side shows (and SIDE_SLACK of its
#: reach to spare) is the body's, so a raised arm doesn't take a strip of the side with it.
SIDE_SLACK = 0.15
#: The trunk fills under the near arm this far (pixels at full size) past its side's line too, so the
#: fold of the armpit isn't left open as the arm swings back.
ARMPIT_SPARE = 30
SIDE_BINS = 40


def trim_inner(cut, figure, out, nearest_trunk):
    b = "arm_near_up"
    (x0, y0), (x1, y1) = bone_line(cut.base, b)
    x0, y0, x1, y1 = x0 * cut.scale, y0 * cut.scale, x1 * cut.scale, y1 * cut.scale
    dx, dy = x1 - x0, y1 - y0
    length = max(1e-6, float(np.hypot(dx, dy)))
    h, w = figure.shape
    yy, xx = np.mgrid[0:h, 0:w]
    cx, cy = cut.joint("chest")
    toward = np.sign((cx - x0) * dy - (cy - y0) * dx)
    off = ((xx - x0) * dy - (yy - y0) * dx) * toward / length
    along = cut.along[b]
    arm = out[b]
    look = max(2.0, 4 * cut.scale)
    marks = []
    for k in range(SIDE_BINS):
        lo, hi = k / SIDE_BINS, (k + 1) / SIDE_BINS
        inbin = (along >= lo) & (along < hi)
        mine = arm & inbin
        if not mine.any():
            continue
        edge = off[mine].max()
        beyond = inbin & (off > edge) & (off <= edge + look)
        free = (figure & beyond).sum() < 0.3 * max(1, beyond.sum())
        marks.append(((lo + hi) / 2, edge, free))
    shown = [(t, e) for t, e, free in marks if free]
    if not shown:
        return
    ts, es = zip(*shown)
    width = np.interp(along, ts, es) + SIDE_SLACK * cut.reach[b]
    jx, jy = cut.joint(BONE_OF[b][1])
    round_end = np.hypot(xx - jx, yy - jy) <= cut.ball(BONE_OF[b][1]) * JOINT_ROUND
    hidden_t = [t for t, e, free in marks if not free]
    if not hidden_t:
        return
    span = (along >= min(hidden_t) - 0.5 / SIDE_BINS) & (along <= max(hidden_t) + 0.5 / SIDE_BINS)
    extra = arm & span & (off > width) & ~round_end
    out[b] = arm & ~extra
    for k, t in enumerate(TRUNK):
        out[t] |= extra & (nearest_trunk == k)


#: Where the near arm hides the trunk's side, the trunk's fill reaches only to the side's line: the
#: trunk's own edge, its notches filled from the rows where the side shows above and below, smoothed
#: over FLANK_SMOOTH rows. The line is inked FLANK_INK pixels deep in the figure's outline colour, so a
#: raised arm leaves a painted edge, not a cut one.
FLANK_SMOOTH = 12
FLANK_INK = 3
ARMPIT = 0.5


def near_flank(cut, own):
    """The trunk's near side under the near arm: (line, hidden), where line[y] is the x the trunk
    reaches to on row y (inf on rows with none of the trunk) and hidden[y] says the near arm covers
    the side on that row."""
    trunk = own["chest"] | own["spine"] | own["hips"]
    comp, count = ic.components(trunk)
    if count > 1:
        sizes = np.bincount(comp.ravel())
        sizes[0] = 0
        trunk = comp == sizes.argmax()
    arm = own["arm_near_up"] | own["arm_near_low"] | own["hand_near"]
    h = trunk.shape[0]
    rows = trunk.any(axis=1)
    left = np.where(rows, trunk.argmax(axis=1), 0)
    look = max(1, int(round(4 * cut.scale)))
    ys = np.nonzero(rows)[0]
    hidden = np.zeros(h, bool)
    for y in ys:
        hidden[y] = arm[y, max(0, left[y] - look):left[y]].any()
    line = np.full(h, np.inf)
    line[ys] = left[ys]
    seen = ys[~hidden[ys]]
    if len(seen) >= 2:
        line[ys] = np.minimum(left[ys], np.interp(ys, seen, left[seen]))
    k = max(1, int(round(FLANK_SMOOTH * cut.scale)))
    low = line.copy()
    for y in ys[hidden[ys]]:
        low[y] = line[max(0, y - k):y + k + 1].min()
    out = line.copy()
    for y in ys[hidden[ys]]:
        window = low[max(0, y - k):y + k + 1]
        out[y] = window[np.isfinite(window)].mean()
    return out, hidden


def box_of(mask, pad):
    ys, xs = np.nonzero(mask)
    if len(xs) == 0:
        return None
    h, w = mask.shape
    return max(0, ys.min() - pad), min(h, ys.max() + 1 + pad), max(0, xs.min() - pad), min(w, xs.max() + 1 + pad)


def fill_from(colour, known, todo):
    """Colours for `todo` painted in from `known` beside them, a ring at a time, worked in the box
    round them both."""
    out = colour.astype(np.float64).copy()
    box = box_of(known | todo, 2)
    if box is None:
        return out
    y0, y1, x0, x1 = box
    c, k, t = out[y0:y1, x0:x1], known[y0:y1, x0:x1].copy(), todo[y0:y1, x0:x1]
    while True:
        left = t & ~k
        if not left.any():
            break
        total = np.zeros(c.shape)
        count = np.zeros(k.shape)
        for dy, dx in ((-1, 0), (1, 0), (0, -1), (0, 1), (-1, -1), (-1, 1), (1, -1), (1, 1)):
            there = np.roll(np.roll(k, dy, axis=0), dx, axis=1)
            total += np.roll(np.roll(c, dy, axis=0), dx, axis=1) * there[:, :, None]
            count += there
        ring = left & (count > 0)
        if not ring.any():
            break
        c[ring] = total[ring] / count[ring][:, None]
        k |= ring
    out[y0:y1, x0:x1] = c
    return out


def seeds(colour, own):
    """What a piece's fill takes its colours from: its own pixels that aren't its outline (lit
    enough, SEED_LIGHT), or all of them if none are."""
    luma = colour[:, :, :3].astype(np.float64) @ np.array([0.299, 0.587, 0.114])
    if not own.any():
        return own
    light = own & (luma >= luma[own].mean() * SEED_LIGHT)
    return light if light.any() else own


#: Pixels at the figure's soft edge (some alpha, but less than half) go to the piece beside them,
#: this many pixels deep, so a piece keeps its smoothed outline.
SOFT_EDGE = 2


def pieces(base, colour, alpha, scale=1.0, prior=None, with_labels=False):
    """A figure (colour 0-255 and alpha 0-1, on its base's canvas at `scale`) cut into its pieces:
    {bone: (colour, alpha)}, each the figure's size. A piece is what labels() gives it, with what the
    pieces in front of it hide within its reach filled in from its own colours (so a limb that moves
    away leaves no hole), and a child's cap over its joint (CAP)."""
    figure = alpha > 0.5
    cut = Cut.of(base, scale, figure.shape)
    own = labels(cut, figure, colour)
    if prior is not None:
        # Where the base's body is, its pieces; past it (a pauldron, a skirt), the figure's own cut.
        known = np.any(list(prior.values()), axis=0)
        own = {b: (prior[b] & figure) | (own[b] & ~known) for b in own}
    # The soft edge, to the piece beside it.
    soft = (alpha > 0) & ~figure
    claimed = np.any(list(own.values()), axis=0)
    for _ in range(SOFT_EDGE):
        for b in list(own):
            ring = ic.grow(own[b], 1) & soft & ~claimed
            own[b] = own[b] | ring
            claimed |= ring
    figure = figure | claimed
    front = np.zeros(figure.shape, bool)
    nearest_trunk = np.argmin([cut.dist[t] for t in TRUNK], axis=0)
    out = {}
    h, w = figure.shape
    yy, xx = np.mgrid[0:h, 0:w]
    line, side_hidden = near_flank(cut, own)
    jx, jy = cut.joint("shoulder_near")
    shoulder = np.hypot(xx - jx, yy - jy) <= cut.ball("shoulder_near") * JOINT_ROUND
    # The side's line meets the shoulder's round in a curve, as the armpit does: the corner between
    # them filled, ARMPIT of the round's reach.
    within = (xx >= line[:, None] - 0.5 - ARMPIT_SPARE * cut.scale) | shoulder
    bend = max(1, int(round(cut.reach["arm_near_up"] * ARMPIT)))
    within = ~ic.grow(~ic.grow(within, bend), bend)
    for group in GROUPS:
        group_own = np.any([own[b] for b in group], axis=0)
        for b in group:
            key = "trunk" if group is TRUNK else b
            hidden = front & figure & (cut.dist[key] <= cut.reach[key])
            if group is TRUNK:
                # The trunk fills what an arm hides a little wider than it reaches, so the shoulder
                # the arm leaves is whole (to TRUNK_FILL of its reach), and out to its side's line.
                hidden = front & figure & (cut.dist[key] <= cut.reach[key] * TRUNK_FILL[b]) & within
                hidden &= nearest_trunk == TRUNK.index(b)
            else:
                # Round at the joint too (no cut across it), so it looks the same however far it turns.
                hidden &= cut.dist[key] <= cut.reach[key] * HIDDEN_SHARE
            c = fill_from(colour, seeds(colour, own[b]), hidden) if hidden.any() and own[b].any() else colour.astype(np.float64)
            a = np.where(own[b], alpha, 0.0) + hidden * (1.0 if own[b].any() else 0.0)
            out[b] = (c, np.clip(a, 0.0, 1.0))
        front |= group_own
    # The side's line inked where the arm hid it (below the shoulder's round, which the arm keeps).
    body = np.any([out[t][1] > 0.5 for t in TRUNK], axis=0)
    ink_px = max(1, int(round(FLANK_INK * cut.scale)))
    edge = body & ic.grow(~body, ink_px) & side_hidden[:, None] & (xx <= line[:, None] + ink_px + 1) & ~shoulder
    if edge.any():
        luma = colour[:, :, :3].astype(np.float64) @ np.array([0.299, 0.587, 0.114])
        dark = figure & (luma < luma[figure].mean() * OUTLINE_DARK)
        ink = np.median(colour[dark], axis=0) if dark.any() else np.zeros(colour.shape[2])
        for t in TRUNK:
            c, a = out[t]
            mine = edge & (a > 0.5)
            inked = c.copy()
            inked[mine, :3] = ink[:3]
            out[t] = (inked, a)
    # Each child's cap: the pixels round its joint, as they're painted, carried by the child.
    for bone, parent, joint in BONES:
        # The shoulders and the hips turn far, and the trunk's fill covers what they leave: no cap.
        if parent is None or bone == "head" or bone in FIRST or bone not in out or parent not in out:
            continue
        jx, jy = cut.joint(joint)
        disc = (np.hypot(xx - jx, yy - jy) <= CAP * cut.ball(joint)) & figure
        disc &= own[parent] | own[bone]
        c, a = out[bone]
        out[bone] = (np.where(disc[:, :, None], colour, c), np.where(disc, np.maximum(a, alpha), a))
    # The trunk's pieces overlap a little where they meet, the one drawn over carrying a strip of the
    # one under, so the seam doesn't open into a crack as they turn against each other.
    seam = max(1, int(round(TRUNK_OVERLAP * cut.scale)))
    for k, t in enumerate(TRUNK):
        solid = out[t][1] > 0.5
        for u in TRUNK[k + 1:]:
            cu, au = out[u]
            strip = ic.grow(solid, seam) & (au > 0.5) & ~solid
            c, a = out[t]
            out[t] = (np.where(strip[:, :, None], cu, c), np.where(strip, au, a))
    return (out, own) if with_labels else out


#: How far (pixels at full size) each trunk piece reaches over the one drawn under it.
TRUNK_OVERLAP = 3


# --------------------------------------------------------------------------- #
# Posing it (the reference for the game's)
# --------------------------------------------------------------------------- #

def world(base, pose, scale=1.0):
    """Each bone's transform for a pose ({"bone": degrees, "bone.x"/".y": rig px, "bone.sx"/".sy":
    scale}), as 2x3 matrices taking a rest-pose canvas point to where it's drawn: a bone turns about
    its joint (clockwise, y down), moves by its offset, and carries its children."""
    unit = RIG_PX[base] * scale
    out = {}
    for bone, parent, joint in BONES:
        jx, jy = JOINTS[base][joint][0] * scale, JOINTS[base][joint][1] * scale
        a = np.radians(pose.get(bone, 0.0))
        c, s = np.cos(a), np.sin(a)
        sx, sy = pose.get(bone + ".sx", 1.0), pose.get(bone + ".sy", 1.0)
        ox, oy = pose.get(bone + ".x", 0.0) * unit, pose.get(bone + ".y", 0.0) * unit
        # About the joint: translate to it, scale, turn, translate back, then the offset.
        local = np.array([[c * sx, -s * sy, 0.0], [s * sx, c * sy, 0.0]])
        local[:, 2] = np.array([jx, jy]) - local[:, :2] @ np.array([jx, jy]) + np.array([ox, oy])
        if parent is not None:
            p = out[parent]
            local = np.hstack([p[:, :2] @ local[:, :2], (p[:, :2] @ local[:, 2] + p[:, 2])[:, None]])
        out[bone] = local
    return out


def render(base, layers, pose, size=None, origin=(0, 0), scale=1.0, front=()):
    """The pieces ({bone: (colour, alpha)} on the canvas, in DRAW order, plus any held under a hand
    as "<bone>+") posed: premultiplied RGBA on a canvas of `size` (w, h), the rest canvas's (0, 0)
    at `origin`."""
    h0, w0 = next(iter(layers.values()))[1].shape
    w, h = size or (w0, h0)
    out = np.zeros((h, w, 4))
    transforms = world(base, pose, scale)
    yy, xx = np.mgrid[0:h, 0:w].astype(np.float64)
    # A clip can bring pieces in front of the chest (its `front`): they're drawn after the head.
    order = [b for b in DRAW if b not in front]
    at = order.index("head") + 1
    order[at:at] = [b for b in DRAW if b in front]
    for bone in order:
        for key in (bone + "+", bone) if bone.startswith("hand") else (bone,):
            if key not in layers:
                continue
            colour, alpha = layers[key]
            m = transforms[bone]
            inv = np.linalg.inv(np.vstack([m, [0, 0, 1]]))
            sx = inv[0, 0] * (xx - origin[0]) + inv[0, 1] * (yy - origin[1]) + inv[0, 2]
            sy = inv[1, 0] * (xx - origin[0]) + inv[1, 1] * (yy - origin[1]) + inv[1, 2]
            x0, y0 = np.floor(sx).astype(int), np.floor(sy).astype(int)
            fx, fy = sx - x0, sy - y0
            ok = (x0 >= 0) & (y0 >= 0) & (x0 < w0 - 1) & (y0 < h0 - 1)
            x0c, y0c = np.clip(x0, 0, w0 - 2), np.clip(y0, 0, h0 - 2)
            premul = np.concatenate([colour / 255.0 * alpha[:, :, None], alpha[:, :, None]], axis=2)
            sample = (premul[y0c, x0c] * ((1 - fx) * (1 - fy))[:, :, None] + premul[y0c, x0c + 1] * (fx * (1 - fy))[:, :, None]
                      + premul[y0c + 1, x0c] * ((1 - fx) * fy)[:, :, None] + premul[y0c + 1, x0c + 1] * (fx * fy)[:, :, None])
            sample *= ok[:, :, None]
            out = sample + out * (1.0 - sample[:, :, 3:])
    return out


def draw(base, out):
    """The base with its skeleton over it: the bones in yellow, the joints in red."""
    img = read_png(os.path.join(CHARS, base + ".png"))[:, :, :3].astype(np.float64) * 0.6
    h, w = img.shape[:2]
    yy, xx = np.mgrid[0:h, 0:w]
    for bone, _, _ in BONES:
        (x0, y0), (x1, y1) = bone_line(base, bone)
        dx, dy = x1 - x0, y1 - y0
        length = max(1e-6, dx * dx + dy * dy)
        t = np.clip(((xx - x0) * dx + (yy - y0) * dy) / length, 0, 1)
        d = np.hypot(xx - (x0 + t * dx), yy - (y0 + t * dy))
        img[d < 3] = (240, 220, 60)
    for name, (x, y) in JOINTS[base].items():
        img[np.hypot(xx - x, yy - y) < 8] = (230, 50, 50)
    write_png(os.path.join(out, "skeleton-" + base + ".png"), np.dstack([img.astype(np.uint8), np.full((h, w), 255, np.uint8)]))


if __name__ == "__main__":
    folder = sys.argv[1] if len(sys.argv) > 1 else os.path.join(ROOT, "shots", "art", "rig")
    os.makedirs(folder, exist_ok=True)
    for base in JOINTS:
        draw(base, folder)
    print("skeleton: drawn into", folder)
