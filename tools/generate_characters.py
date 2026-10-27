"""
Paperdoll sheets for Legend Quest.

    python tools/generate_characters.py

Every sheet is the same layout, so the layers stack: 32x48 frames (the same as The Black Box, so the
cutting scripts still fit), ten columns and four rows, 320x192 in all. The feet are at (16, 46) in
every frame, on every sheet.

    columns: 0 standing, 1-4 walking, 5 wind-up, 6 strike, 7 cast, 8 hurt, 9 downed
    rows:    down, left, right, up (the Direction enum's order)

Everything is drawn off one rig: where the head, shoulders, elbows, hands, hips, knees and feet are
in each frame. A layer is a set of shapes hung off those joints, so a helmet or a sleeve drawn on a
different sheet still lands on the same head or arm. The shapes are The Black Box's: signed
distances, sampled three times a pixel each way, shaded from the same top-left light, with a dark
edge on the shadow side.

Facing right isn't facing left mirrored, the way The Black Box did it. The sword hand is the right
one either way, so it's on the near side facing right and the far side facing left, and a mirror
would swap it.

For now there's the lean body. The paperdoll's other layers (eyes, hair, clothes, armour, weapons)
and the second build come with the paperdoll. The body is painted in the skin keys, the first row
of each ramp, so the palette swap can recolour it later.
"""

import math
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from pixels import OUT, new_image, write_png  # noqa: E402

FRAME_W = 32
FRAME_H = 48
COLUMNS = 10
DIRECTIONS = ("down", "left", "right", "up")

#: The columns, by what's in them.
STAND, WALK, WIND_UP, STRIKE, CAST, HURT, DOWNED = 0, (1, 2, 3, 4), 5, 6, 7, 8, 9

#: Each pixel is sampled SS x SS times to decide which shape covers it.
SS = 3

#: Light direction: up, left and toward the viewer, the same as everything else.
LIGHT = (-0.50, -0.75, 0.90)
_LN = math.sqrt(sum(c * c for c in LIGHT))
LIGHT = tuple(c / _LN for c in LIGHT)

#: The light a flat surface facing the viewer gets. Shading is measured from this, so flat areas land in the middle of a ramp.
NDL_FLAT = LIGHT[2]

#: How far the edge of a shape bends away from the viewer. Bigger is rounder.
EDGE_TILT = 0.85


# --------------------------------------------------------------------------- #
# Ramps, dark to light. SKIN's first row is the key the palette swap looks for.
# --------------------------------------------------------------------------- #

SKIN = ((92, 70, 58), (142, 112, 92), (188, 154, 128), (218, 188, 160))
LINEN = ((86, 78, 66), (124, 114, 96), (160, 150, 128), (188, 178, 156))
BREECHES = ((40, 35, 32), (60, 53, 47), (82, 72, 63), (102, 92, 80))

BODY_RAMPS = dict(skin=SKIN, linen=LINEN, breeches=BREECHES)


# --------------------------------------------------------------------------- #
# Shapes. Each part is a signed distance (positive inside, in art pixels), a material and a
# bounding box, so the rasteriser can skip it for pixels it can't touch. From The Black Box.
# --------------------------------------------------------------------------- #

def sd_line(x, y, x0, y0, x1, y1, width):
    """A capsule: a line of some thickness with round ends."""
    dx, dy = x1 - x0, y1 - y0
    length2 = dx * dx + dy * dy
    t = 0.0 if length2 == 0.0 else max(0.0, min(1.0, ((x - x0) * dx + (y - y0) * dy) / length2))
    return width / 2.0 - math.hypot(x - x0 - t * dx, y - y0 - t * dy)


def sd_poly(x, y, points):
    """Even-odd inside test, and the distance to the nearest edge for the sign."""
    inside = False
    nearest = float("inf")
    n = len(points)
    for i in range(n):
        ax, ay = points[i]
        bx, by = points[(i + 1) % n]
        nearest = min(nearest, -sd_line(x, y, ax, ay, bx, by, 0.0))
        if (ay > y) != (by > y):
            if x < ax + (y - ay) * (bx - ax) / (by - ay):
                inside = not inside
    return nearest if inside else -nearest


class Part:
    __slots__ = ("sdf", "mat", "bbox", "bevel", "tone", "name")

    def __init__(self, sdf, mat, bbox, bevel=2.2, tone=0.0, name=""):
        self.sdf = sdf
        self.mat = mat
        self.bbox = bbox
        self.bevel = bevel
        self.tone = tone
        self.name = name


def ellipse(c, rx, ry, mat, **kw):
    cx, cy = c
    k = min(rx, ry)

    def sdf(x, y):
        return (1.0 - math.sqrt(((x - cx) / rx) ** 2 + ((y - cy) / ry) ** 2)) * k
    return Part(sdf, mat, (cx - rx, cy - ry, cx + rx, cy + ry), **kw)


def capsule(a, b, width, mat, **kw):
    (x0, y0), (x1, y1) = a, b
    h = width / 2.0
    return Part(lambda x, y: sd_line(x, y, x0, y0, x1, y1, width), mat,
                (min(x0, x1) - h, min(y0, y1) - h, max(x0, x1) + h, max(y0, y1) + h), **kw)


def poly(points, mat, **kw):
    xs = [p[0] for p in points]
    ys = [p[1] for p in points]
    return Part(lambda x, y: sd_poly(x, y, points), mat, (min(xs), min(ys), max(xs), max(ys)), **kw)


def lerp(a, b, t):
    return (a[0] + (b[0] - a[0]) * t, a[1] + (b[1] - a[1]) * t)


# --------------------------------------------------------------------------- #
# The rig: where every joint is in every frame
# --------------------------------------------------------------------------- #
# Joints are named for the character's own sides: r is the right arm or leg, which holds the
# weapon. Facing down that's on the screen's left, facing up it's on the right.

class Build:
    """How a body is put together. Only the lean one so far."""

    def __init__(self, shoulder, waist, hip, arm, thigh):
        self.shoulder = shoulder    # half the width across the shoulders, seen from the front
        self.waist = waist          # half the width at the waist
        self.hip = hip              # how far each hip joint is from the middle
        self.arm = arm              # upper arm thickness (the forearm is a little less)
        self.thigh = thigh          # thigh thickness (the shin is a little less)


LEAN = Build(shoulder=4.5, waist=3.6, hip=2.1, arm=2.6, thigh=3.4)

HEAD_Y = 12.0
SHOULDER_Y = 19.6
ELBOW_Y = 24.8
HAND_Y = 29.6
HIP_Y = 30.0
KNEE_Y = 37.2
ANKLE_Y = 43.6

#: (bob, step, swing) per walking column. Step +1 is the right leg forward; the arms swing the
#: other way. The passing frames are a pixel higher, which is the bounce.
WALK_POSES = {1: (0.0, 1, -1), 2: (-1.0, 0, 0), 3: (0.0, -1, 1), 4: (-1.0, 0, 0)}


def front_rig(column, b):
    """Facing down. The character's right is on the screen's left."""
    cx = FRAME_W / 2.0
    j = dict(head=(cx, HEAD_Y), lean=0.0,
             sh_r=(cx - b.shoulder, SHOULDER_Y), sh_l=(cx + b.shoulder, SHOULDER_Y),
             el_r=(cx - b.shoulder - 0.7, ELBOW_Y), el_l=(cx + b.shoulder + 0.7, ELBOW_Y),
             ha_r=(cx - b.shoulder - 0.9, HAND_Y), ha_l=(cx + b.shoulder + 0.9, HAND_Y),
             hip_r=(cx - b.hip, HIP_Y), hip_l=(cx + b.hip, HIP_Y),
             kn_r=(cx - b.hip, KNEE_Y), kn_l=(cx + b.hip, KNEE_Y),
             an_r=(cx - b.hip, ANKLE_Y), an_l=(cx + b.hip, ANKLE_Y),
             bob=0.0, arms_behind=())

    if column in WALK:
        bob, step, swing = WALK_POSES[column]
        j["bob"] = bob
        # The leg stepping toward the viewer comes down the screen a pixel; the other lifts.
        for side, sign in (("r", 1), ("l", -1)):
            drop = 1.0 if step * sign > 0 else (-1.5 if step * sign < 0 else 0.0)
            ax, ay = j["an_" + side]
            j["an_" + side] = (ax, ay + drop)
            kx, ky = j["kn_" + side]
            j["kn_" + side] = (kx, ky + drop * 0.5)
            # The arm on the other side swings forward (down the screen) as this leg does.
            hx, hy = j["ha_" + side]
            j["ha_" + side] = (hx, hy - swing * sign * 1.0)
    elif column == WIND_UP:
        j.update(head=(cx + 0.3, HEAD_Y), el_r=(cx - 7.2, 17.4), ha_r=(cx - 6.4, 13.0),
                 el_l=(cx + 5.6, 24.2), ha_l=(cx + 4.6, 27.6),
                 an_r=(cx - b.hip - 0.6, ANKLE_Y + 1.0), an_l=(cx + b.hip + 0.4, ANKLE_Y - 1.0))
    elif column == STRIKE:
        j.update(head=(cx - 0.3, HEAD_Y + 0.6), lean=0.6, el_r=(cx - 4.4, 25.2), ha_r=(cx - 2.8, 31.4),
                 el_l=(cx + 5.4, 23.6), ha_l=(cx + 6.6, 27.0),
                 kn_r=(cx - b.hip - 0.4, KNEE_Y + 0.8), an_r=(cx - b.hip - 0.6, ANKLE_Y + 1.8),
                 an_l=(cx + b.hip + 0.4, ANKLE_Y - 1.2))
    elif column == CAST:
        j.update(el_r=(cx - 5.0, 23.8), ha_r=(cx - 2.8, 21.4), el_l=(cx + 5.0, 23.8), ha_l=(cx + 2.8, 21.4))
    elif column == HURT:
        j.update(head=(cx, HEAD_Y - 0.8), el_r=(cx - 6.6, 21.8), ha_r=(cx - 8.4, 25.6),
                 el_l=(cx + 6.6, 21.8), ha_l=(cx + 8.4, 25.6),
                 kn_r=(cx - b.hip - 0.6, KNEE_Y - 0.6), kn_l=(cx + b.hip + 0.6, KNEE_Y - 0.6))
    return j


def back_rig(column, b):
    """Facing up: the front rig turned round, so the right arm is on the screen's right. Anything
    reaching forward goes up the screen and behind the body instead of toward the viewer."""
    j = mirror(front_rig(column, b))
    cx = FRAME_W / 2.0
    if column == STRIKE:
        j.update(el_r=(cx + 4.8, 22.4), ha_r=(cx + 3.2, 17.0), arms_behind=("r",))
    elif column == CAST:
        j.update(el_r=(cx + 5.2, 23.4), ha_r=(cx + 2.4, 19.6), el_l=(cx - 5.2, 23.4), ha_l=(cx - 2.4, 19.6),
                 arms_behind=("r", "l"))
    return j


def side_rig(column, b):
    """Facing right. The right arm and leg are the near ones."""
    cx = FRAME_W / 2.0
    j = dict(head=(cx + 0.6, HEAD_Y), lean=0.0,
             sh_r=(cx - 0.2, SHOULDER_Y + 0.2), sh_l=(cx - 0.6, SHOULDER_Y + 0.2),
             el_r=(cx - 0.4, ELBOW_Y), el_l=(cx - 0.8, ELBOW_Y),
             ha_r=(cx - 0.1, HAND_Y - 0.2), ha_l=(cx - 0.5, HAND_Y - 0.4),
             hip_r=(cx + 0.2, HIP_Y), hip_l=(cx - 0.6, HIP_Y),
             kn_r=(cx + 0.4, KNEE_Y), kn_l=(cx - 0.4, KNEE_Y),
             an_r=(cx + 0.2, ANKLE_Y), an_l=(cx - 0.8, ANKLE_Y),
             bob=0.0, arms_behind=())

    def stride(lead, trail, reach, lift=0.6):
        """One leg out in front, the other behind with its heel up."""
        for side, sign in ((lead, 1), (trail, -1)):
            hx, hy = j["hip_" + side]
            fx = hx + sign * reach
            fy = ANKLE_Y - (lift if sign < 0 else 0.0)
            j["an_" + side] = (fx, fy)
            j["kn_" + side] = ((hx + fx) / 2.0 + 0.7, (hy + fy) / 2.0)

    if column in WALK:
        bob, step, swing = WALK_POSES[column]
        j["bob"] = bob
        if step:
            stride("r" if step > 0 else "l", "l" if step > 0 else "r", 3.4)
        # The near arm swings against the near leg, the far arm with it.
        for side, sign in (("r", -1), ("l", 1)):
            hx, hy = j["ha_" + side]
            ex, ey = j["el_" + side]
            j["ha_" + side] = (hx + swing * sign * 2.4, hy - abs(swing) * 0.4)
            j["el_" + side] = (ex + swing * sign * 1.0, ey)
    elif column == WIND_UP:
        j.update(head=(cx + 0.1, HEAD_Y + 0.2), el_r=(cx - 3.4, 18.6), ha_r=(cx - 5.4, 14.2),
                 el_l=(cx + 1.2, 24.2), ha_l=(cx + 3.2, 26.4))
        stride("r", "l", 2.8)
    elif column == STRIKE:
        j.update(head=(cx + 1.6, HEAD_Y + 0.6), lean=0.8, el_r=(cx + 4.2, 22.4), ha_r=(cx + 8.4, 24.6),
                 el_l=(cx - 1.8, 24.4), ha_l=(cx - 3.4, 27.6))
        stride("r", "l", 4.6, lift=1.0)
    elif column == CAST:
        j.update(el_r=(cx + 3.0, 22.8), ha_r=(cx + 6.8, 20.6), el_l=(cx + 2.2, 23.4), ha_l=(cx + 5.8, 22.2))
    elif column == HURT:
        j.update(head=(cx - 1.4, HEAD_Y + 0.4), lean=-0.8, el_r=(cx + 2.6, 22.0), ha_r=(cx + 4.8, 20.0),
                 el_l=(cx - 2.8, 22.6), ha_l=(cx - 4.6, 24.6),
                 kn_r=(cx + 1.4, KNEE_Y - 0.4), kn_l=(cx + 0.6, KNEE_Y - 0.4))
    return j


def mirror(j):
    """Flips a rig across the middle of the frame. The joints keep their names, so the right arm is
    still the right arm, just on the other side of the screen."""
    out = {}
    for key, value in j.items():
        if isinstance(value, tuple) and len(value) == 2 and isinstance(value[0], float):
            out[key] = (FRAME_W - value[0], value[1])
        else:
            out[key] = value
    out["lean"] = -j["lean"]
    return out


def rig(direction, column, b):
    if direction == "down":
        return front_rig(column, b)
    if direction == "up":
        return back_rig(column, b)
    j = side_rig(column, b)
    return j if direction == "right" else mirror(j)


# --------------------------------------------------------------------------- #
# The body layer
# --------------------------------------------------------------------------- #

def shifted(p, dy):
    return (p[0], p[1] + dy)


def body_parts(direction, j, b):
    """The skin, the undershirt and the breeches, in the order they're painted for one frame."""
    bob, lean = j["bob"], j["lean"]
    side = direction in ("left", "right")
    near = "r" if direction in ("right", "down") else "l"
    far = "l" if near == "r" else "r"

    def P(name):
        """A joint, moved by the walk's bounce. Everything above the hips bobs; the feet stay planted."""
        x, y = j[name]
        return (x, y + bob) if not name.startswith(("an_", "kn_")) else (x, y)

    parts = []

    def leg(s, tone=0.0):
        hip, knee, ankle = P("hip_" + s), P("kn_" + s), P("an_" + s)
        parts.append(capsule(hip, knee, b.thigh, "skin", bevel=1.8, tone=tone, name="leg"))
        parts.append(capsule(knee, ankle, b.thigh - 0.6, "skin", bevel=1.6, tone=tone, name="leg"))
        if side:
            ahead = 1.2 if direction == "right" else -1.2
            parts.append(ellipse((ankle[0] + ahead, ankle[1] + 1.2), 2.6, 1.3, "skin", bevel=1.0, tone=tone, name="foot"))
        else:
            parts.append(ellipse((ankle[0], ankle[1] + 1.2), 1.9, 1.3, "skin", bevel=1.0, tone=tone, name="foot"))
        # The breeches cover the top of the thigh.
        parts.append(capsule(hip, lerp(hip, knee, 0.55), b.thigh + 0.5, "breeches", bevel=1.6, tone=tone, name="breeches"))

    def arm(s, tone=0.0):
        sh, el, ha = P("sh_" + s), P("el_" + s), P("ha_" + s)
        parts.append(capsule(sh, el, b.arm, "skin", bevel=1.5, tone=tone, name="arm"))
        parts.append(capsule(el, ha, b.arm - 0.4, "skin", bevel=1.4, tone=tone, name="arm"))
        parts.append(ellipse(ha, 1.4, 1.5, "skin", bevel=1.0, tone=tone, name="hand"))
        parts.append(capsule(sh, lerp(sh, el, 0.45), b.arm + 0.6, "linen", bevel=1.4, tone=tone, name="sleeve"))

    cx = FRAME_W / 2.0 + lean
    top, waist, hips = SHOULDER_Y - 1.2 + bob, 27.2 + bob, 31.4 + bob

    if side:
        forward = 1.0 if direction == "right" else -1.0
        # Far arm and far leg first, darker since they're turned away from the light.
        arm(far, tone=-0.25)
        leg(far, tone=-0.25)
        leg(near)
        torso = [(cx - 2.6 * forward, top + 0.2), (cx + 2.2 * forward, top), (cx + 3.0 * forward, top + 2.6),
                 (cx + 2.4 * forward, waist), (cx - 2.2 * forward, waist), (cx - 3.2 * forward, top + 3.0)]
        parts.append(poly([(cx - 2.6, waist - 0.4), (cx + 2.6, waist - 0.4), (cx + 2.8, hips), (cx - 2.8, hips)],
                          "breeches", bevel=1.8, name="breeches"))
        parts.append(poly(torso, "linen", bevel=2.4, name="torso"))
        parts.append(capsule(shifted(j["head"], bob + 3.6), shifted(j["head"], bob + 6.6), 2.6, "skin", bevel=1.2, tone=-0.25, name="neck"))
        arm(near)
        hx, hy = j["head"]
        parts.append(ellipse((hx, hy + bob), 3.3, 4.2, "skin", bevel=2.4, name="head"))
        parts.append(ellipse((hx + 3.2 * forward, hy + 0.8 + bob), 0.9, 0.9, "skin", bevel=0.7, name="head"))   # the nose
        parts.append(ellipse((hx - 0.8 * forward, hy + 0.6 + bob), 0.9, 1.3, "skin", bevel=0.7, tone=-0.1, name="ear"))
        return parts

    # Facing down or up: legs, the breeches across the hips, the shirt, the arms, then the head.
    for s in ("r", "l"):
        leg(s)
    for s in j["arms_behind"]:
        arm(s, tone=-0.2)
    parts.append(poly([(cx - b.waist - 0.4, waist - 0.6), (cx + b.waist + 0.4, waist - 0.6),
                       (cx + b.waist + 0.8, hips), (cx - b.waist - 0.8, hips)], "breeches", bevel=1.8, name="breeches"))
    parts.append(poly([(cx - b.shoulder - 0.6, top), (cx + b.shoulder + 0.6, top), (cx + b.shoulder + 0.8, top + 2.4),
                       (cx + b.waist, waist), (cx - b.waist, waist), (cx - b.shoulder - 0.8, top + 2.4)],
                      "linen", bevel=2.6, name="torso"))
    hx, hy = j["head"]
    parts.append(capsule((hx, hy + 3.4 + bob), (cx, top + 0.6), 2.8, "skin", bevel=1.2, tone=-0.3, name="neck"))
    for s in ("r", "l"):
        if s not in j["arms_behind"]:
            arm(s)
    for ex in (-3.6, 3.6):
        parts.append(ellipse((hx + ex, hy + 0.8 + bob), 0.9, 1.3, "skin", bevel=0.7, tone=-0.1, name="ear"))
    parts.append(ellipse((hx, hy + bob), 3.5, 4.2, "skin", bevel=2.6, name="head"))
    return parts


def downed_parts(direction, b):
    """Lying on their back, head toward one side of the frame: the downed column. Facing down and
    left the head's on the left; facing right and up, on the right."""
    flip = direction in ("right", "up")

    def X(x):
        return FRAME_W - x if flip else x

    ground = 42.0
    parts = []
    # Legs, knees a little up.
    for dy, tone in ((-1.2, -0.25), (1.0, 0.0)):
        hip, knee, ankle = (X(19.0), ground + dy), (X(23.6), ground - 1.6 + dy), (X(28.4), ground + 0.6 + dy)
        parts.append(capsule(hip, knee, b.thigh, "skin", bevel=1.6, tone=tone))
        parts.append(capsule(knee, ankle, b.thigh - 0.6, "skin", bevel=1.4, tone=tone))
        parts.append(capsule(hip, lerp(hip, knee, 0.55), b.thigh + 0.5, "breeches", bevel=1.4, tone=tone))
    parts.append(poly([(X(17.0), ground - 3.0), (X(20.4), ground - 3.0), (X(20.4), ground + 2.6), (X(17.0), ground + 2.6)],
                      "breeches", bevel=1.6))
    parts.append(poly([(X(9.4), ground - 3.6), (X(17.6), ground - 3.2), (X(17.6), ground + 2.8), (X(9.4), ground + 3.0)],
                      "linen", bevel=2.2))
    # One arm across the body, one out along the ground.
    parts.append(capsule((X(10.4), ground + 2.6), (X(15.0), ground + 4.2), b.arm - 0.2, "skin", bevel=1.2))
    parts.append(capsule((X(11.0), ground - 3.0), (X(15.4), ground - 0.8), b.arm - 0.2, "skin", bevel=1.2, tone=-0.1))
    parts.append(ellipse((X(15.8), ground - 0.4), 1.3, 1.3, "skin", bevel=1.0))
    parts.append(ellipse((X(6.2), ground - 0.4), 3.6, 3.2, "skin", bevel=2.2))
    return parts


# --------------------------------------------------------------------------- #
# Rasteriser (The Black Box's, cut down to what the body needs)
# --------------------------------------------------------------------------- #

def render_frame(parts, ramps):
    """Draws one frame and returns its rows of RGBA."""
    W, H = FRAME_W, FRAME_H
    candidates = [[[] for _ in range(W)] for _ in range(H)]
    for i, p in enumerate(parts):
        x0, y0, x1, y1 = p.bbox
        for py in range(max(0, int(math.floor(y0)) - 1), min(H, int(math.ceil(y1)) + 1)):
            for px in range(max(0, int(math.floor(x0)) - 1), min(W, int(math.ceil(x1)) + 1)):
                candidates[py][px].append(i)

    # Which part owns each pixel: the last one painted over most of its samples.
    owner = [[-1] * W for _ in range(H)]
    half = (SS * SS) / 2.0
    for py in range(H):
        for px in range(W):
            cands = candidates[py][px]
            if not cands:
                continue
            counts = {}
            covered = 0
            for sy in range(SS):
                y = py + (sy + 0.5) / SS
                for sx in range(SS):
                    x = px + (sx + 0.5) / SS
                    top = -1
                    for i in cands:
                        if parts[i].sdf(x, y) > 0.0:
                            top = i
                    if top >= 0:
                        counts[top] = counts.get(top, 0) + 1
                        covered += 1
            if covered >= half:
                owner[py][px] = max(counts, key=lambda k: (counts[k], k))

    # Shade each pixel off its part's surface, bent away from the viewer near its edge.
    index = [[0] * W for _ in range(H)]
    for py in range(H):
        for px in range(W):
            i = owner[py][px]
            if i < 0:
                continue
            p = parts[i]
            n = len(ramps[p.mat])
            x, y = px + 0.5, py + 0.5
            d = p.sdf(x, y)
            e = 0.35
            gx = p.sdf(x + e, y) - p.sdf(x - e, y)
            gy = p.sdf(x, y + e) - p.sdf(x, y - e)
            gl = math.hypot(gx, gy) or 1.0
            ox, oy = -gx / gl, -gy / gl
            tilt = EDGE_TILT * max(0.0, min(1.0, 1.0 - max(d, 0.0) / p.bevel))
            nx, ny = ox * tilt, oy * tilt
            nz = math.sqrt(max(0.0, 1.0 - nx * nx - ny * ny))
            ndl = nx * LIGHT[0] + ny * LIGHT[1] + nz * LIGHT[2]
            shade = 0.55 + 1.1 * (ndl - NDL_FLAT) + p.tone
            index[py][px] = max(0, min(n - 1, int(math.floor(shade * n))))

    # Selective outline: the silhouette goes darkest on the shadow side, one step down on the lit side.
    out = new_image(W, H)
    for py in range(H):
        for px in range(W):
            i = owner[py][px]
            if i < 0:
                continue
            k = index[py][px]
            if px == W - 1 or owner[py][px + 1] < 0 or py == H - 1 or owner[py + 1][px] < 0:
                k = 0
            elif px == 0 or owner[py][px - 1] < 0 or py == 0 or owner[py - 1][px] < 0:
                k = max(0, k - 1)
            c = ramps[parts[i].mat][k]
            out[py][px] = [c[0], c[1], c[2], 255]
    return out


def build_sheet(name, layer, ramps, b):
    """Every frame of one layer, ten across and a row per direction."""
    sheet = new_image(FRAME_W * COLUMNS, FRAME_H * len(DIRECTIONS))
    for d, direction in enumerate(DIRECTIONS):
        for c in range(COLUMNS):
            parts = downed_parts(direction, b) if c == DOWNED else layer(direction, rig(direction, c, b), b)
            frame = render_frame(parts, ramps)
            for y in range(FRAME_H):
                sheet[d * FRAME_H + y][c * FRAME_W:(c + 1) * FRAME_W] = frame[y]
    write_png(os.path.join(OUT, "Paperdoll", name + ".png"), sheet)


if __name__ == "__main__":
    print("Generating character sheets for Legend Quest...")
    build_sheet("body-lean", body_parts, BODY_RAMPS, LEAN)
    print("Done.")
