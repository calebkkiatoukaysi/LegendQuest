"""
Every clip the person rig plays (03-art-overhaul.md §4, and the art plan's §5 combat clips), written
as poses so they're readable and diffable, and turned into Content/Rig/clips.json.

    python tools/rig/clips.py          writes Content/Characters/Parts/clips.json
    python tools/rig/clips.py --check  each clip's feet against the ground and its joints' bends

clips.json:
    {"format": "LegendQuest rig clips 1",
     "events": ["step", "hit", "release", "pass"],
     "rigs": {"humanoid": {"clips": {name: clip}, "aliases": {name: clip},
                           "combat": {beat: clip or {class: clip}}}, <creature rigs>...},
     "props": [clips a prop plays on its own strip]}

Every clip plays over base.rig.json's restPose, the relaxed three-quarter stance: its channels are
added to the rest pose's, so REST here means "the stance", not straight limbs.

A clip (03 §4.1): {"length", "loop", "ease": "smooth"|"sine"|"linear", "speed", "bones": {bone:
{"rot"|"x"|"y"|"sx"|"sy": [[t, v], ...]}}, "swaps": [[t, slot, part]], "layer": {slot: layer},
"events": [[t, name]], "partner": {"offset": [x, y], "clip": name}, "fx": [[t, effect, socket or
bone]], "face": {"eyes": state, ...}, "expr": expression}. Rotations are degrees clockwise (y is
down, so a negative turn lifts an arm forward), offsets rig px, scales multiply. A clip lists only
the bones it moves; one that doesn't loop holds its last frame until the next clip. Playback blends
0.12 s.

Easing (sample() here is the game's, ClipTrack.At): "smooth", the default, is a cubic through the
keys that never overshoots them, at rest at a key where a channel turns back or holds and moving on
through one it carries on past, as an animator's auto-clamped curves do. So a limb doesn't stop dead
at every key the way it does easing each pair with a sine ("sine", kept for a clip that wants it).

A clip with a "speed" moves the figure: its planted feet are left behind at that many rig px a
second, and the game plays it faster or slower to match how fast the figure is really going. A
clip named "<name>@<base>" is that clip worked out for one base's own proportions (the jog, the walk
and the run, from tools/rig/gait.py); the game plays it on that base's figures and "<name>" on any
other.

Every clip but the locomotion and the fast combat moves gets drag (`drag()`): each forearm, hand and
the head follows the bone it hangs from a beat late, so a limb leads from the shoulder and the hand
trails, the way the successive joints of a real arm break.

Combat timings follow ExploreScreen.CombatCues: wind-ups 0.22 s or less, strikes 0.30 s with `hit`
at 0.12, cast 0.45 s with `release` at 0.25, hurt 0.35 s, die 0.9 s, revive and yield 0.6 s.

The creature rigs' clips are tools/rig/creature_clips.py's: if it's there, its rigs() (a dict of rig
-> {"clips", "aliases", "combat"}) is merged in under "rigs", and its humanoid_extras() (the person
clips only enemies play: {"clips", "aliases"}) into the humanoid rig, a name clash being an error.

Only needs CPython.
"""

import importlib
import json
import math
import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
from vector import ROOT  # noqa: E402
import gait as G  # noqa: E402

OUT = os.path.join(ROOT, "Content", "Characters", "Parts", "clips.json")
EVENTS = ("step", "hit", "release", "pass")
CLASSES = ("one-hand", "two-hand", "polearm", "dagger", "unarmed", "hook")
RANGED = ("bow", "crossbow")
CLIPS = {}
ALIASES = {}
NO_DRAG = set()


# --------------------------------------------------------------------------- #
# Poses: {"bone": rot, "bone.y": offset, ...}; anything not named is at rest
# --------------------------------------------------------------------------- #

def P(*parts, **kw):
    """A pose: merges dicts (later wins) and keyword bones (rot), "bone__y" for other channels."""
    out = {}
    for p in parts:
        out.update(p)
    for k, v in kw.items():
        out[k.replace("__", ".")] = v
    return out


def _leg():
    """The mannequins' leg: (hip height, thigh, shin) in rig px, the near leg's from skeleton.py's
    joints, both bases' averaged."""
    import skeleton
    hip, thigh, shin = [], [], []
    for base, joints in skeleton.JOINTS.items():
        unit = skeleton.RIG_PX[base]
        ground = joints["root"][1]
        hip.append((ground - joints["hip_near"][1]) / unit)
        thigh.append((joints["knee_near"][1] - joints["hip_near"][1]) / unit)
        shin.append((joints["ankle_near"][1] - joints["knee_near"][1]) / unit)
    return sum(hip) / len(hip), sum(thigh) / len(thigh), sum(shin) / len(shin)


HIP, THIGH, SHIN = _leg()
K = HIP / 90.0
"""The figure's size against the one these poses were first written for (hips at 90 px)."""


LEGS = {"near": G.Leg("near"), "far": G.Leg("far")}
"""The two bases' legs averaged, for planting feet by IK."""


def stand_on(hips_x=0.0, hips_y=0.0, near=(0.0, 0.0), far=(0.0, 0.0), roll=(0.0, 0.0)):
    """The legs' channels with the hips moved (rig px) and each foot planted where it stands at rest
    moved by (x, y), flat on the ground or rolled (near, far) degrees: the legs bend to reach."""
    out = {"hips.x": hips_x, "hips.y": hips_y}
    for side, moved, turn in (("near", near, roll[0]), ("far", far, roll[1])):
        leg = LEGS[side]
        hip = (leg.hip[0] + hips_x, leg.hip[1] + hips_y)
        lean, bend, _ = leg.reach(hip, (leg.ankle[0] + moved[0], leg.ankle[1] + moved[1]))
        out.update({k: round(v, 2) for k, v in leg.channels(lean, bend, turn).items()})
    return out


def crouch(a, near=0.0, far=0.0):
    """Knees bent about `a` degrees (the hips dropped as far as two straight legs bent so would
    drop them), both feet staying flat where they stand, or stepped `near` and `far` rig px forward."""
    drop = (THIGH + SHIN) * (1 - math.cos(math.radians(a)))
    return stand_on(0.0, round(drop, 2), (near, 0.0), (far, 0.0))


REST = {}
BREATHE = {"torso.sy": 1.008}
READY = P(crouch(12, near=7.0, far=-4.0), {"torso": 8, "head": -3, "arm_near_up": -30, "arm_near_low": -72,
                                           "arm_far_up": -22, "arm_far_low": -60})
"""The fight stance, over the rest pose: knees bent, the near arm up with the weapon, the far arm
forward in guard, showing in front of the chest."""
def legs_at(hips_x, hips_y, near, far):
    """The hips moved (rig px) and each leg posed: given an ankle to reach, (x, y) from where it
    stands at rest, and a foot roll ({"ankle": (x, y), "foot": deg}), or given outright as a thigh
    lean forward of straight down, a knee bend and a foot roll ({"lean", "bend", "foot"}), all
    absolute degrees."""
    out = {"hips.x": hips_x, "hips.y": hips_y}
    for side, how in (("near", near), ("far", far)):
        leg = LEGS[side]
        if "ankle" in how:
            hip = (leg.hip[0] + hips_x, leg.hip[1] + hips_y)
            lean, bend, _ = leg.reach(hip, (leg.ankle[0] + how["ankle"][0], leg.ankle[1] + how["ankle"][1]))
        else:
            lean, bend = how["lean"], how["bend"]
        out.update({k: round(v, 2) for k, v in leg.channels(lean, bend, how.get("foot", 0.0)).items()})
    return out


def _kneel():
    """Down on the far knee, the near foot planted ahead: the far thigh near upright with its knee's
    ball on the ground, the shin rising back from it to the foot standing on its tucked toes; the
    hips as high as that leaves them, and the near leg folding up to its foot."""
    far = LEGS["far"]
    lean, ball = 8.0, 4.0
    ground = 0.5 * (far.heel[1] + far.toe[1])
    hips_y = (ground - ball) - far.l1 * math.cos(math.radians(lean)) - far.hip[1]
    near = LEGS["near"]
    return legs_at(-2.0, round(hips_y, 2), {"ankle": (26.0 + near.hip[0] - near.ankle[0], 0.0)},
                   {"lean": lean, "bend": 120.0, "foot": 80.0})


def _sit(seat, feet):
    """Sat with the hips `seat` rig px above the ground, each foot planted `feet` rig px ahead of
    where it stands."""
    near = LEGS["near"]
    hips_y = -seat - near.hip[1]
    return legs_at(0.0, round(hips_y, 2), {"ankle": (feet + (near.hip[0] - near.ankle[0]), 0.0)}, {"ankle": (feet, 0.0)})


KNEEL = P(_kneel(), {"torso": 10, "arm_near_up": -10, "arm_far_up": 6})
_AHEAD = LEGS["near"].hip[0] - LEGS["near"].ankle[0] + 26.0
KNEEL_STEP = stand_on(-3.0, 6.0, near=(_AHEAD * 0.55, -9.0), far=(-6.0, -3.0), roll=(0.0, 25.0))
"""Going down on a knee or getting up from one: the near foot in the air, stepping."""
KNEEL_RISE = stand_on(-2.0, 16.0, near=(_AHEAD, 0.0), far=(-14.0, -8.0), roll=(0.0, 40.0))
"""...and down ahead, the hips lowering between the feet, the far foot back on its toes."""
SIT = P(_sit(52.0, 24.0), {"torso": -4, "arm_near_up": -18, "arm_near_low": -30, "arm_far_up": -14, "arm_far_low": -30})
SIT_GROUND = P(_sit(9.0, 40.0), {"torso": 14, "arm_near_up": -40, "arm_near_low": -20, "arm_far_up": -30,
                                 "arm_far_low": -20})
LYING = {"root": -90, "root.y": round(-14 * K, 2), "root.x": round(30 * K, 2), "arm_near_up": -70, "arm_far_up": -100,
         "arm_near_low": -10, "arm_far_low": 10, "leg_near_up": -8, "leg_far_up": 6, "head": -10}
LYING_FRONT = {"root": 90, "root.y": round(-14 * K, 2), "root.x": round(-40 * K, 2), "arm_near_up": -150, "arm_far_up": -160,
               "head": -30}
HUNCH = P(crouch(28), {"torso": 32, "head": -8, "arm_near_up": -30, "arm_near_low": -40,
                       "arm_far_up": -20, "arm_far_low": -50})
ARMS_FORWARD = {"arm_near_up": -55, "arm_near_low": -25, "arm_far_up": -45, "arm_far_low": -30}
PALM_UP = {"arm_near_up": -25, "arm_near_low": -75, "hand_near": -20, "head": 10}
REACH_UP = {"arm_near_up": -150, "arm_near_low": -10, "torso": -6, "head": -12}
POINT = {"arm_near_up": -82, "arm_near_low": -4, "torso": -2, "head": -2}


def channels(poses):
    out = set()
    for _, p in poses:
        for k in p:
            bone, _, ch = k.partition(".")
            out.add((bone, ch or "rot"))
    return sorted(out)


def clip(name, length, keys, loop=False, events=(), swaps=(), layer=None, ease="smooth", partner=None,
         fx=(), face=None, expr=None, notes=None, front=None, speed=None, drag=True):
    """Adds a clip from (time, pose) keys. `drag` False keeps its forearms, hands and head as keyed."""
    if loop and keys[-1][0] < length:
        keys = list(keys) + [(length, keys[0][1])]
    bones = {}
    for bone, ch in channels(keys):
        default = 1.0 if ch in ("sx", "sy") else 0.0
        key = bone if ch == "rot" else bone + "." + ch
        track = [[round(t, 4), round(float(p.get(key, default)), 3)] for t, p in keys]
        bones.setdefault(bone, {})[ch] = track
    c = {"length": round(length, 4), "loop": loop, "ease": ease, "bones": bones}
    if speed:
        c["speed"] = round(speed, 2)
    if not drag:
        NO_DRAG.add(name)
    if events:
        c["events"] = [[round(t, 4), e] for t, e in events]
    if swaps:
        c["swaps"] = [list(s) for s in swaps]
    if layer:
        c["layer"] = layer
    if partner:
        c["partner"] = partner
    if fx:
        c["fx"] = [list(f) for f in fx]
    if face:
        c["face"] = face
    if expr:
        c["expr"] = expr
    if notes:
        c["notes"] = notes
    if front:
        c["front"] = list(front)
    if name in CLIPS:
        raise ValueError("two clips called " + name)
    CLIPS[name] = c
    return c


def alias(clip_name, *names):
    for n in names:
        ALIASES[n] = clip_name


def pose_clip(name, pose, into=0.35, **kw):
    """Moves into a pose and holds it."""
    return clip(name, into, [(0, REST), (into, pose)], **kw)


def there_and_back(name, pose, length, hold=0.4, **kw):
    """Into a pose, held, and back."""
    a = (length - hold) / 2.0
    return clip(name, length, [(0, REST), (a, pose), (a + hold, pose), (length, REST)], **kw)


# --------------------------------------------------------------------------- #
# Playing a clip: the game's easing, here to check and preview by
# --------------------------------------------------------------------------- #

def slopes(times, values, loop):
    """Each key's slope for "smooth": the neighbouring segments' slopes blended (a weighted
    harmonic mean, so the curve never overshoots), nought where the channel turns back or holds, and
    at a clip's ends unless it loops, when its first and last keys (the same pose) share one."""
    n = len(times)
    m = [0.0] * n
    if n < 3:
        return m
    h = [max(1e-6, times[i + 1] - times[i]) for i in range(n - 1)]
    d = [(values[i + 1] - values[i]) / h[i] for i in range(n - 1)]

    def blend(d0, d1, h0, h1):
        if d0 * d1 <= 0:
            return 0.0
        return 3 * (h0 + h1) / ((2 * h1 + h0) / d0 + (h1 + 2 * h0) / d1)
    for i in range(1, n - 1):
        m[i] = blend(d[i - 1], d[i], h[i - 1], h[i])
    if loop:
        m[0] = m[-1] = blend(d[-1], d[0], h[-1], h[0])
    return m


def value_at(track, t, ease, loop, cache=None):
    """A track's value at t, as the game has it: held before its first key and after its last."""
    times = [k[0] for k in track]
    values = [k[1] for k in track]
    if t <= times[0]:
        return values[0]
    if t >= times[-1]:
        return values[-1]
    k = 1
    while times[k] < t:
        k += 1
    h = max(1e-6, times[k] - times[k - 1])
    f = (t - times[k - 1]) / h
    if ease == "linear":
        return values[k - 1] + (values[k] - values[k - 1]) * f
    if ease == "sine":
        f = 0.5 - 0.5 * math.cos(math.pi * f)
        return values[k - 1] + (values[k] - values[k - 1]) * f
    m = cache if cache is not None else slopes(times, values, loop)
    f2, f3 = f * f, f * f * f
    return ((2 * f3 - 3 * f2 + 1) * values[k - 1] + (f3 - 2 * f2 + f) * h * m[k - 1]
            + (-2 * f3 + 3 * f2) * values[k] + (f3 - f2) * h * m[k])


def sample(c, t):
    """A clip's pose at t seconds ({"bone": rot, "bone.y": ...}), as the game plays it: a loop
    wraps round."""
    if c["loop"]:
        t %= c["length"]
    pose = {}
    for bone, chans in c["bones"].items():
        for ch, track in chans.items():
            pose[bone if ch == "rot" else bone + "." + ch] = value_at(track, t, c["ease"], c["loop"])
    return pose


PLANT_FPS = 30


def _feet(pose):
    """Where a pose puts each ankle (rig px from the root, in the root's frame) and how each foot is
    turned (absolute degrees), from its legs' channels."""
    out = {}
    for side, leg in LEGS.items():
        up, low = pose.get("leg_%s_up" % side, 0.0), pose.get("leg_%s_low" % side, 0.0)
        hip = (leg.hip[0] + pose.get("hips.x", 0.0), leg.hip[1] + pose.get("hips.y", 0.0))
        thigh = 90.0 - (leg.thigh0 + up)
        bend = (leg.shin0 - leg.thigh0) + low
        out[side] = leg.ankle_at(hip, thigh, bend) + (G._wrap(up + low + pose.get("foot_%s" % side, 0.0)),)
    return out


def plant(name, c):
    """Moves a clip's feet along paths rather than its legs through angles: each ankle goes from
    where one key puts it to where the next does, eased as the clip eases, and the legs reach it by
    IK (the knees bending forward), so a foot that stays put stays put and one that moves doesn't
    swing through the floor on the way, nor a heel or toe dip under it as a foot turns. A clip that
    turns its hips is left alone."""
    bones = c["bones"]
    legs = [b for b in bones if b.startswith(("leg_", "foot_"))]
    if not legs or "rot" in bones.get("hips", {}):
        return False
    times = [k[0] for k in next(iter(bones[legs[0]].values()))]
    keyed = [_feet(sample(c, t)) for t in times]
    series = {}
    for side in LEGS:
        for i in range(3):
            series[(side, i)] = [f[side][i] for f in keyed]
    slope = {k: slopes(times, v, c["loop"]) for k, v in series.items()}
    n = max(2, int(math.ceil(c["length"] * PLANT_FPS)))
    dense = [c["length"] * k / n for k in range(n + 1)]
    tracks = {}
    sole = (LEGS["far"].heel_off, LEGS["far"].toe_off)
    lowest_at_rest = max(o[1] for o in sole)

    def channel(bone, ch, t, rest=0.0):
        return value_at(bones[bone][ch], t, c["ease"], c["loop"]) if ch in bones.get(bone, {}) else rest
    for t in dense:
        hx, hy = channel("hips", "x", t), channel("hips", "y", t)
        upright = abs(channel("root", "rot", t)) < 20.0
        for side, leg in LEGS.items():
            ax, ay, foot = (value_at([[a, b] for a, b in zip(times, series[(side, i)])], t, c["ease"], c["loop"],
                                     slope[(side, i)]) for i in range(3))
            # Never through the floor: a heel or toe that would go under it lifts the ankle.
            if upright:
                under = max(ay + G._turn(o, foot)[1] for o in sole) + channel("root", "y", t) - (leg.ankle[1] + lowest_at_rest)
                if under > 0:
                    ay -= under
            lean, bend, _ = leg.reach((leg.hip[0] + hx, leg.hip[1] + hy), (ax, ay))
            for ch, v in leg.channels(lean, bend, foot).items():
                tracks.setdefault(ch, []).append([round(t, 4), round(v, 3)])
    for ch, track in tracks.items():
        # Kept turning the short way from one frame to the next, never a whole turn round.
        for i in range(1, len(track)):
            while track[i][1] - track[i - 1][1] > 180:
                track[i][1] = round(track[i][1] - 360, 3)
            while track[i][1] - track[i - 1][1] < -180:
                track[i][1] = round(track[i][1] + 360, 3)
        bones.setdefault(ch, {})["rot"] = track
    return True


#: What follows what, and how long after (seconds): each forearm its upper arm, each hand its
#: forearm, the head the torso.
DRAG = (("arm_near_low", "arm_near_up", 0.045), ("hand_near", "arm_near_low", 0.035),
        ("arm_far_low", "arm_far_up", 0.045), ("hand_far", "arm_far_low", 0.035), ("head", "torso", 0.06))
CHAIN = {"hips": "root", "torso": "hips", "head": "torso", "arm_near_up": "torso",
         "arm_near_low": "arm_near_up", "hand_near": "arm_near_low", "arm_far_up": "torso",
         "arm_far_low": "arm_far_up", "hand_far": "arm_far_low"}
DRAG_FPS = 30


def _elbow_rest(side):
    a = G.Arm(side)
    return a.low0 - a.up0


def drag(name, c):
    """Lays drag over a clip: each bone in DRAG turns so that where it points follows, a beat late,
    where it pointed as keyed, against the bone it hangs from as it now is. A limb so leads from the
    shoulder, its forearm and hand trailing and catching up as it stops; the head settles after the
    torso. An elbow is never straightened past the rest arm's, a wrist or the head never turned more
    than 30 degrees from the keyed pose. A clip that doesn't loop is lengthened by the longest beat,
    so the last bone settles before it holds."""
    length, loop = c["length"], c["loop"]

    def rot(pose, bone):
        return pose.get(bone, 0.0)

    def world(pose, bone):
        total = 0.0
        while bone is not None:
            total += rot(pose, bone)
            bone = CHAIN.get(bone)
        return total
    lag_total = {}
    for bone, parent, lag in DRAG:
        lag_total[bone] = lag + lag_total.get(parent, 0.0)
    extra = 0.0 if loop else max(lag_total.values())
    n = max(2, int(math.ceil((length + extra) * DRAG_FPS)))
    times = [min(length + extra, k / float(DRAG_FPS)) for k in range(n + 1)]
    if loop:
        times = [length * k / n for k in range(n + 1)]
    keyed = {}

    def at(t):
        if loop:
            t %= length
        key = round(t, 5)
        if key not in keyed:
            keyed[key] = sample(c, max(0.0, t))
        return keyed[key]
    new = {bone: [] for bone, _, _ in DRAG}
    limit = {"arm_near_low": 2.0 - _elbow_rest("near"), "arm_far_low": 2.0 - _elbow_rest("far")}
    for t in times:
        now = dict(at(t))
        for bone, parent, _ in DRAG:
            before = at(t - lag_total[bone]) if (loop or t - lag_total[bone] > 0) else at(0.0)
            want = world(before, bone)
            value = want - world(now, parent)
            own = rot(at(t), bone)
            if bone in limit:
                value = min(value, max(limit[bone], own))
            else:
                value = min(own + 30.0, max(own - 30.0, value))
            now[bone] = value
            new[bone].append((t, value))
    changed = False
    for bone, track in new.items():
        orig = [rot(at(t), bone) for t, _ in track]
        if max(abs(v - o) for (_, v), o in zip(track, orig)) < 0.3:
            continue
        changed = True
        c["bones"].setdefault(bone, {})["rot"] = [[round(t, 4), round(v, 3)] for t, v in track]
    if changed and not loop:
        c["length"] = round(length + extra, 4)
    return changed


# --------------------------------------------------------------------------- #
# Standing, moving, falling (03 §4.3)
# --------------------------------------------------------------------------- #

def idle():
    """Standing at ease, six seconds round: two slow breaths (the chest filling and the shoulders
    lifting with it), and the weight drifting onto the near leg and back to the far with the feet
    staying put, the torso answering it a beat late and the head later still, the arms hanging and
    swaying after the torso."""
    length, n = 6.0, 60
    keys = []
    for k in range(n):
        t = length * k / n
        wave = lambda lag, turns=1: math.sin(2 * math.pi * turns * (t - lag) / length)  # noqa: E731
        breath = 0.5 - 0.5 * math.cos(4 * math.pi * t / length)
        shift = wave(0.0)
        pose = stand_on(round(1.3 * shift, 3), round(0.45 * shift * shift, 3))
        pose.update({"torso": -0.9 * wave(0.35) - 0.6 * wave(0.6) - 0.8 * breath,
                     "torso.sy": 1.0 + 0.0064 * breath, "head": 0.7 * wave(1.0) + 0.5 * breath - 0.6 * wave(0.3, 2),
                     "arm_near_up": 1.4 * wave(0.9) - 1.0 * breath, "arm_far_up": 1.2 * wave(1.1) - 0.8 * breath,
                     "arm_near_low": -2.0 - 1.2 * wave(1.3), "arm_far_low": -2.0 - 1.0 * wave(1.5)})
        keys.append((t, pose))
    clip("idle", length, keys, loop=True, drag=False,
         notes="breathing twice and easing the weight across and back, the feet planted")


def locomotion():
    """The jog, the walk and the run from gait.py: one on the bases averaged, and one for each
    base's own proportions so its planted feet don't slide."""
    import skeleton
    for name, make in G.CYCLES.items():
        for base in [None] + sorted(skeleton.JOINTS):
            g = make(base)
            clip(name if base is None else name + "@" + base, g.period, g.keys(), loop=True, events=g.events(),
                 speed=g.speed, drag=False, notes=None if base else (make.__doc__ or "").split("\n")[0].strip())
    alias("jog", "jog-in", "hurry")
    alias("walk", "walk-slow", "stroll")
    alias("run", "run-in")
    alias("jog", "leave", "stand-beside")


def standing():
    idle()
    alias("idle", "stand-still", "hold")
    locomotion()
    # A step forward: the weight shifts back, the near foot reaches out and lands, the body follows
    # over it, and the far foot comes up beside it.
    clip("step-forward", 0.7, [(0, REST), (0.12, stand_on(-1.5, 0.6)),
                               (0.3, P(stand_on(2.0, 1.2, near=(9.0, -4.0)), {"arm_near_up": 6, "arm_far_up": -6})),
                               (0.45, stand_on(7.0, 1.0, near=(12.0, 0.0))),
                               (0.7, P(stand_on(12.0, 0.0, near=(12.0, 0.0), far=(12.0, 0.0))))],
         events=[(0.45, "step")])
    # Sitting down: the weight goes back over the heels and the arms come forward to balance, then down.
    clip("sit", 0.8, [(0, REST), (0.3, P(crouch(22), {"torso": 18, "arm_near_up": -20, "arm_far_up": -16})),
                      (0.8, SIT)])
    clip("sit-fire", 1.0, [(0, REST), (0.35, P(crouch(30), {"torso": 22, "arm_near_up": -24, "arm_far_up": -18})),
                           (0.65, P(SIT_GROUND, {"hips.y": SIT_GROUND["hips.y"] - 6, "torso": 18})), (1.0, SIT_GROUND)])
    clip("sit-up", 0.8, [(0, LYING), (0.35, P(LYING, {"root": -60, "head": -20, "torso": 10})), (0.8, SIT_GROUND)])
    # Up off a knee: lean forward over the front foot, push, straighten, the head last.
    clip("stand", 0.85, [(0, KNEEL), (0.22, P(KNEEL, {"torso": 28, "head": 6, "arm_near_up": -10})),
                         (0.45, P(KNEEL_RISE, {"torso": 15})), (0.65, P(KNEEL_STEP, {"torso": 3})), (0.85, REST)])
    alias("stand", "rise")
    # Down onto a knee: the near foot steps out ahead and lands, the far heel lifts as the hips
    # lower between them, the far knee comes down, and it settles.
    clip("kneel", 0.8, [(0, REST), (0.2, P(KNEEL_STEP, {"torso": 3})), (0.4, P(KNEEL_RISE, {"torso": 12})),
                        (0.62, P(KNEEL, {"hips.y": KNEEL["hips.y"] + 1.5, "torso": 13})), (0.8, KNEEL)])
    pose_clip("fall-kneel", P(KNEEL, {"torso": 18, "head": 12}), 0.18, ease="linear")
    pose_clip("kneel-smoking", KNEEL, 0.5, fx=[(0.3, "smoke", "hips")])
    pose_clip("lean", {"torso": -10, "hips.x": -3, "leg_far_up": 10, "leg_near_up": -8,
                       "arm_near_up": -20, "arm_near_low": -60})
    pose_clip("lean-rail", {"torso": 22, "head": -10, "arm_near_up": -60, "arm_near_low": -30,
                            "arm_far_up": -50, "arm_far_low": -40, "leg_far_up": 6})
    clip("turn", 0.15, [(0, REST), (0.075, {"hips.y": 3, "torso.sx": 0.6, "hips.sx": 0.6, "head.sx": 0.7}), (0.15, REST)],
         ease="linear", notes="the runtime flips facing at the halfway point, where the figure is squashed thinnest")
    there_and_back("bow", P({"torso": 40, "head": 18, "arm_near_up": 8, "arm_far_up": 4}), 1.4, 0.5)
    there_and_back("bow-head", {"head": 20, "torso": 4}, 0.9, 0.3)
    pose_clip("look-up", {"head": -24, "torso": -6})
    pose_clip("face-sun", {"head": -32, "torso": -12}, 0.6, face={"eyes": "closed"})
    alias("face-sun", "stand-sun")
    pose_clip("look-back", {"head": -6, "torso": -10, "head.sx": -1.0, "hair_back.sx": -1.0, "hair_back.x": 36}, 0.4,
              notes="the head's parts flip to look behind; hair_back moves across to the other side")
    pose_clip("hide", HUNCH, 0.3)
    clip("leap", 0.55, [(0, REST), (0.15, P(crouch(24), {"arm_near_up": 30, "arm_far_up": 30})),
                        (0.35, {"root.y": -30, "root.x": 20, "leg_near_up": -40, "leg_near_low": 60,
                                "arm_near_up": -120, "arm_far_up": -110}),
                        (0.55, {"root.x": 44})])
    clip("dive", 0.5, [(0, REST), (0.15, P(crouch(20), {"torso": 20})),
                       (0.5, P(LYING_FRONT, {"root.x": 30}))])
    clip("land", 0.35, [(0, {"root.y": -24, "leg_near_up": -20, "leg_near_low": 30}),
                        (0.15, P(crouch(30), {"arm_near_up": -40, "arm_far_up": -30})), (0.35, REST)],
         events=[(0.15, "step")])
    alias("land", "drop-in")
    clip("stagger", 0.7, [(0, REST), (0.15, {"hips.x": -8, "torso": -12, "head": -10, "arm_near_up": -30,
                                             "arm_far_up": 20, "leg_near_up": -14}),
                          (0.4, {"hips.x": -14, "torso": 6, "leg_far_up": 12}), (0.7, {"hips.x": -14})],
         events=[(0.4, "step")])
    clip("fall", 0.6, [(0, REST), (0.2, P(crouch(16), {"torso": -14, "arm_near_up": -50, "arm_far_up": -40})),
                       (0.6, LYING)])
    alias("fall", "slip")
    pose_clip("slump", P(SIT_GROUND, {"torso": 38, "head": 30, "arm_near_up": 6, "arm_far_up": 4}), 0.8)
    clip("slide-to", 0.8, [(0, LYING), (0.8, P(LYING, {"root.x": 60, "arm_near_up": -160, "arm_far_up": -170}))],
         notes="pulled toward the cue's target; the runtime moves the root there")
    pose_clip("hold-throne", P(LYING_FRONT, {"arm_near_up": -170, "arm_near_low": -10}), 0.6)
    there_and_back("scream", {"head": -34, "torso": -10, "arm_near_up": -150, "arm_near_low": -100,
                              "arm_far_up": -140, "arm_far_low": -100}, 1.6, 1.0, expr="shock")
    clip("wake-gasp", 0.35, [(0, LYING), (0.35, P(SIT_GROUND, {"head": -14, "torso": 2}))], expr="shock")
    clip("march-in-place", 0.8, [(0, {"leg_near_up": -30, "leg_near_low": 40, "arm_far_up": -20, "arm_near_up": 10}),
                                 (0.4, {"leg_far_up": -30, "leg_far_low": 40, "arm_near_up": -20, "arm_far_up": 10}),
                                 (0.8, {"leg_near_up": -30, "leg_near_low": 40, "arm_far_up": -20, "arm_near_up": 10})],
         loop=True, events=[(0.2, "step"), (0.6, "step")])
    clip("frozen", 0.01, [(0, REST)], fx=[(0, "frozen", "root")],
         notes="holds whatever pose the actor is in, stops the blink, tints violet; the runtime doesn't blend out of the last pose")
    CHAIN = {"arm_near_up": -150, "arm_near_low": 30, "arm_far_up": -140, "arm_far_low": 30, "head": 14}
    pose_clip("chained", CHAIN, 0.4)
    clip("break-chains", 0.7, [(0, CHAIN), (0.3, P(CHAIN, {"torso": -8})), (0.7, {"arm_near_up": -20, "arm_far_up": 10})],
         fx=[(0.3, "shatter", "hand_near")])
    pose_clip("arm-sling", {"arm_near_up": -10, "arm_near_low": -95}, 0.3,
              swaps=[(0, "arm_near_low", "sling")], notes="the look's kit draws the sling as part 'sling'")


# --------------------------------------------------------------------------- #
# Dancing, clapping, jumping, waving (the owner's, 2026-11-09)
# --------------------------------------------------------------------------- #



def reach(side, tx, ty):
    """The turns of an arm (side "near" or "far") that put its wrist at (tx, ty) on base-male's
    canvas: {arm_<side>_up: deg, arm_<side>_low: deg}, clockwise over the rest pose. Below the
    shoulder the elbow bends down and back; raised above it, the elbow goes out to its own side (the
    near arm's to the left, the far arm's to the right), so a raised arm never crosses the face. Both
    bases are near enough in build that one answer suits them."""
    import math
    import skeleton
    j = skeleton.JOINTS["base-male"]
    sx, sy = j["shoulder_" + side]
    ex, ey = j["elbow_" + side]
    wx, wy = j["wrist_" + side]
    l1, l2 = math.hypot(ex - sx, ey - sy), math.hypot(wx - ex, wy - ey)
    d = min(max(math.hypot(tx - sx, ty - sy), abs(l1 - l2) + 1e-3), l1 + l2 - 1e-3)
    toward = math.atan2(ty - sy, tx - sx)
    bend = math.acos((l1 * l1 + d * d - l2 * l2) / (2 * l1 * d))
    raised = ty < sy - 0.25 * (l1 + l2)
    out = -1.0 if side == "near" else 1.0
    best = None
    for k in (1, -1):
        a = toward + k * bend
        e = (sx + l1 * math.cos(a), sy + l1 * math.sin(a))
        score = out * e[0] if raised else e[1] - 0.5 * e[0]
        if best is None or score > best[0]:
            best = (score, a, e)
    _, a, (nex, ney) = best
    upper = math.degrees(a - math.atan2(ey - sy, ex - sx))
    lower = math.degrees(math.atan2(ty - ney, tx - nex) - math.atan2(wy - ey, wx - ex)) - upper
    wrap = lambda v: (v + 180.0) % 360.0 - 180.0  # noqa: E731
    return {"arm_%s_up" % side: round(wrap(upper), 1), "arm_%s_low" % side: round(wrap(lower), 1)}


def moves():
    # In three-quarter view the hands meet in front of the body, out past the chest, where the far
    # hand (painted in shadow, behind the chest) shows; the near arm reaches across in front.
    apart = P(reach("near", 650, 478), reach("far", 790, 470), {"head": -2})
    together = P(reach("near", 712, 474), reach("far", 728, 470), {"head": 2, "torso": 3})
    clip("clap", 0.5, [(0, apart), (0.25, together), (0.5, apart)], loop=True, events=[(0.25, "clap")],
         notes="the hands meet in front of the body")
    alias("clap", "applaud")

    def groove(side):
        arms = P(reach("near", 320, 130), reach("far", 700, 740)) if side > 0 else             P(reach("near", 360, 740), reach("far", 770, 130))
        return P(crouch(10), arms, {"hips.x": 3 * side, "torso": -9 * side, "head": 6 * side,
                                    "leg_near_up": -12 * side, "leg_near_low": 14, "leg_far_up": 12 * side, "leg_far_low": 14})
    dip = P(crouch(18), reach("near", 600, 470), reach("far", 760, 480))
    clip("dance", 1.2, [(0, groove(1)), (0.3, dip), (0.6, groove(-1)), (0.9, dip), (1.2, groove(1))], loop=True,
         events=[(0.3, "step"), (0.9, "step")], notes="a bouncing two-step, one hand high and the other low, then swapping")
    alias("dance", "celebrate")
    up = P(reach("near", 330, 90), reach("far", 760, 100))
    # Down into the crouch with the arms swung back (anticipation), up through the toes as the arms
    # throw up, the knees tucking at the top, a landing that gives deep and comes back up.
    clip("jump", 1.0, [(0, REST), (0.24, P(crouch(30), reach("near", 300, 720), reach("far", 590, 730), {"torso": 18})),
                       (0.34, P(up, stand_on(0, -6, roll=(16, 30)), {"torso": -2})),
                       (0.5, P(up, {"root.y": -30, "leg_near_up": -34, "leg_near_low": 56, "foot_near": -20,
                                    "leg_far_up": -22, "leg_far_low": 64, "foot_far": -24})),
                       (0.66, P(reach("near", 360, 520), reach("far", 760, 520), {"root.y": -4})),
                       (0.76, P(crouch(26), reach("near", 330, 640), reach("far", 700, 640), {"torso": 10})), (1.0, REST)],
         events=[(0.24, "step"), (0.76, "step")], notes="straight up and down where they stand")
    alias("jump", "hop")
    hello = P(reach("near", 300, 150), {"head": -4})
    side = P(hello, {"arm_near_low": hello["arm_near_low"] - 28})
    clip("wave", 1.3, [(0, REST), (0.3, hello), (0.5, side), (0.7, hello), (0.9, side), (1.3, REST)],
         notes="the near hand up beside the head, the forearm waving")
    alias("wave", "wave-hand", "greet")
    hands_up = P(reach("near", 300, 80), reach("far", 790, 90), {"head": -10})
    clip("cheer", 1.0, [(0, REST), (0.25, P(crouch(10), hands_up)), (0.5, P(hands_up, {"root.y": -10})),
                        (0.75, P(crouch(10), hands_up)), (1.0, REST)], events=[(0.75, "step")],
         notes="both arms up in a V")


# --------------------------------------------------------------------------- #
# Hands and items
# --------------------------------------------------------------------------- #

def hands():
    there_and_back("point", POINT, 1.4, 0.8)
    there_and_back("raise-hand", {"arm_near_up": -120, "arm_near_low": -30, "hand_near": -20}, 1.2, 0.6)
    there_and_back("stop", {"arm_near_up": -75, "arm_near_low": -20, "hand_near": -70, "torso": -3}, 1.0, 0.5)
    pose_clip("show-palm", PALM_UP, 0.4, swaps=[(0.2, "hand_near", "open")])
    alias("show-palm", "look-palm")
    there_and_back("close-fist", {"arm_near_up": -20, "arm_near_low": -80, "torso": 3}, 1.0, 0.5,
                   swaps=[(0.25, "hand_near", "fist")])
    reach_out = {"arm_near_up": -62, "arm_near_low": -28, "hand_near": -20, "torso": 9, "head": 6,
                 "leg_near_up": -10}
    clip("interact", 0.8, [(0, REST), (0.3, reach_out), (0.5, P(reach_out, {"arm_near_low": -36, "hand_near": 10})),
                           (0.8, REST)], loop=True, events=[(0.4, "pass")],
         notes="using what's in front: a door, a lever, a counter, a chest (the art guide's Interact cycle)")
    pose_clip("offer-hand", {"arm_near_up": -55, "arm_near_low": -15, "hand_near": -40, "torso": 4},
              swaps=[(0.2, "hand_near", "open")])
    there_and_back("reach", P(REACH_UP, {"leg_far_up": 8}), 1.2, 0.5)
    alias("reach", "reach-crown", "touch-crown")
    clip("crown-self", 1.6, [(0, REST), (0.5, P(REACH_UP, {"arm_far_up": -150, "arm_far_low": -10})),
                             (0.9, {"arm_near_up": -160, "arm_near_low": -70, "arm_far_up": -160, "arm_far_low": -70}),
                             (1.6, REST)])
    clip("give-item", 1.0, [(0, REST), (0.4, {"arm_near_up": -60, "arm_near_low": -20, "torso": 6}),
                            (0.7, {"arm_near_up": -60, "arm_near_low": -20, "torso": 6}), (1.0, REST)],
         events=[(0.55, "pass")], partner={"offset": [34, 0], "clip": "receive-item"})
    clip("receive-item", 1.0, [(0, REST), (0.45, {"arm_near_up": -55, "arm_near_low": -25}),
                               (0.7, {"arm_near_up": -45, "arm_near_low": -50}), (1.0, {"arm_near_low": -30})],
         events=[(0.55, "pass")])
    alias("give-item", "hand-over")
    clip("pick-up", 0.9, [(0, REST), (0.4, P(crouch(34), {"torso": 50, "arm_near_up": -30})),
                          (0.9, {"arm_near_up": -10, "arm_near_low": -40})], events=[(0.4, "pass")])
    clip("snatch", 0.35, [(0, REST), (0.15, {"arm_near_up": -70, "arm_near_low": -10, "torso": 10}),
                          (0.35, {"arm_near_up": -10, "arm_near_low": -70})], events=[(0.15, "pass")], ease="linear")
    alias("pick-up", "grab-item")
    clip("place-item", 1.0, [(0, {"arm_near_low": -40}), (0.5, P(crouch(16), {"torso": 18, "arm_near_up": -50})),
                             (1.0, REST)], events=[(0.5, "pass")])
    alias("place-item", "lift-into")
    pose_clip("lift-item", {"arm_near_up": -95, "arm_near_low": -40, "head": -8}, 0.5)
    alias("lift-item", "raise-item", "lift-ledger", "hold-out")
    pose_clip("lift-both", {"arm_near_up": -100, "arm_near_low": -30, "arm_far_up": -95, "arm_far_low": -30, "head": -10}, 0.5)
    clip("throw", 0.6, [(0, REST), (0.25, {"arm_near_up": 40, "arm_near_low": -90, "torso": -10, "leg_near_up": -10}),
                        (0.35, {"arm_near_up": -110, "arm_near_low": -10, "torso": 12}), (0.6, {"arm_near_up": -30})],
         events=[(0.33, "release")])
    alias("throw", "throw-fire", "burn-book")
    clip("uncork", 1.2, [(0, REST), (0.4, {"arm_near_up": -20, "arm_near_low": -90, "arm_far_up": -20, "arm_far_low": -95}),
                         (0.7, {"arm_near_up": -20, "arm_near_low": -100, "arm_far_up": -10, "arm_far_low": -80}),
                         (1.2, {"arm_near_up": -20, "arm_near_low": -90})])
    alias("uncork", "fill-vial", "cut-palm")
    clip("untie-cord", 1.2, [(0, REST), (0.4, {"arm_near_up": -40, "arm_near_low": -120, "arm_far_up": -40, "arm_far_low": -120, "head": 12}),
                             (0.9, {"arm_near_up": -40, "arm_near_low": -110, "arm_far_up": -40, "arm_far_low": -110, "head": 12}),
                             (1.2, {"arm_near_up": -30, "arm_near_low": -60})], events=[(0.9, "pass")])
    clip("drink-flask", 1.4, [(0, REST), (0.4, {"arm_near_up": -60, "arm_near_low": -110, "head": -20}),
                              (1.0, {"arm_near_up": -65, "arm_near_low": -115, "head": -28}), (1.4, {"arm_near_low": -40})])
    alias("drink-flask", "pour-wine")
    read_pose = {"arm_near_up": -20, "arm_near_low": -85, "arm_far_up": -15, "arm_far_low": -80, "head": 16, "torso": 4}
    clip("read", 2.0, [(0, read_pose), (1.0, P(read_pose, {"head": 20})), (2.0, read_pose)], loop=True)
    alias("read", "read-roll", "inspect", "look-at-shrine")
    sign_pose = P(crouch(6), {"torso": 28, "head": 20, "arm_near_up": -30, "arm_near_low": -60})
    clip("sign", 0.8, [(0, sign_pose), (0.4, P(sign_pose, {"arm_near_low": -70, "hand_near": 10})), (0.8, sign_pose)], loop=True)
    alias("sign", "writing")
    clip("bind-book", 1.2, [(0, read_pose), (0.6, P(read_pose, {"arm_near_low": -100, "arm_far_low": -60})), (1.2, read_pose)],
         loop=True)
    mend_pose = P(SIT_GROUND, {"head": 22, "arm_near_up": -20, "arm_near_low": -80, "arm_far_up": -25, "arm_far_low": -85})
    clip("mend", 0.9, [(0, mend_pose), (0.45, P(mend_pose, {"arm_near_low": -95, "hand_near": 14})), (0.9, mend_pose)], loop=True)
    alias("mend", "whittle", "clean-blade", "scratch-shield", "stuff-satchel")
    clip("flip-coin", 1.0, [(0, {"arm_near_up": -20, "arm_near_low": -70}), (0.2, {"arm_near_up": -30, "arm_near_low": -100}),
                            (0.7, {"arm_near_up": -20, "arm_near_low": -70, "head": -14}), (1.0, {"arm_near_up": -20, "arm_near_low": -70})],
         events=[(0.2, "release"), (0.7, "pass")], fx=[(0.2, "coin-arc", "hand_near")])
    clip("open-parcel", 1.4, [(0, read_pose), (0.7, P(read_pose, {"arm_near_up": -40, "arm_near_low": -50, "arm_far_low": -100})),
                              (1.4, read_pose)])
    clip("unlock", 1.2, [(0, REST), (0.4, {"arm_near_up": -70, "arm_near_low": -20, "torso": 6}),
                         (0.8, {"arm_near_up": -70, "arm_near_low": -20, "hand_near": 60, "torso": 6}), (1.2, REST)])
    clip("pickpocket", 0.8, [(0, P(crouch(10), {"torso": 10})), (0.35, P(crouch(10), {"torso": 12, "arm_near_up": -40, "arm_near_low": -10})),
                             (0.8, P(crouch(10), {"torso": 10, "arm_near_low": -60}))], events=[(0.4, "pass")],
         partner={"offset": [26, 0], "clip": "idle"})
    clip("hood-back", 1.0, [(0, REST), (0.45, {"arm_near_up": -150, "arm_near_low": -80, "arm_far_up": -150, "arm_far_low": -80}),
                            (1.0, REST)], swaps=[(0.5, "headwear", "hood-down")])
    there_and_back("touch-burn", {"arm_near_up": -40, "arm_near_low": -120, "head": 10, "torso": 6}, 1.4, 0.7)
    alias("touch-burn", "cough", "breathe-in")
    heal_pose = P(ARMS_FORWARD, {"hand_near": -30, "torso": 4})
    clip("heal", 1.2, [(0, REST), (0.4, heal_pose), (1.2, heal_pose)], fx=[(0.3, "glow-gold", "hand_front"), (0.3, "glow-gold", "hand_back")])
    clip("drain", 1.2, [(0, heal_pose), (0.6, P(heal_pose, {"torso": -4})), (1.2, heal_pose)], loop=True,
         fx=[(0, "particles-red-in", "hand_front")])
    chant_pose = {"arm_near_up": -40, "arm_near_low": -70, "arm_far_up": -35, "arm_far_low": -70, "head": 12}
    clip("chant", 2.0, [(0, chant_pose), (1.0, P(chant_pose, {"head": 18, "torso": 4})), (2.0, chant_pose)], loop=True)
    pose_clip("address-crowd", {"arm_near_up": -110, "arm_near_low": -20, "arm_far_up": -100, "arm_far_low": -20,
                                "head": -12, "torso": -5}, 0.6)
    alias("address-crowd", "climb-cart")
    hit_wall = {"arm_near_up": -100, "arm_near_low": -10, "arm_far_up": -95, "arm_far_low": -10, "torso": 12}
    clip("pound-wall", 0.6, [(0, P(hit_wall, {"arm_near_up": -130, "arm_far_up": -125})), (0.3, hit_wall),
                             (0.6, P(hit_wall, {"arm_near_up": -130, "arm_far_up": -125}))], loop=True, events=[(0.3, "hit")])
    pose_clip("bar-door", P(crouch(14), {"torso": 26, "arm_near_up": -70, "arm_near_low": -70,
                                         "arm_far_up": -60, "arm_far_low": -60, "leg_far_up": 20}), 0.4)
    alias("bar-door", "set-shield", "shield-cover")
    pose_clip("blade-at-throat", {"arm_near_up": -70, "arm_near_low": -30, "torso": 6, "head": 8}, 0.5,
              partner={"offset": [24, 0], "clip": "kneel"})
    clip("pommel-strike", 0.5, [(0, REST), (0.2, {"arm_near_up": -150, "arm_near_low": -20}),
                                (0.32, {"arm_near_up": -40, "arm_near_low": -10, "torso": 14}), (0.5, {"arm_near_up": -30})],
         events=[(0.32, "hit")])
    alias("pommel-strike", "strike")
    clip("draw", 0.6, [(0, REST), (0.25, {"arm_near_up": -160, "arm_near_low": -60, "head": -6}),
                       (0.6, {"arm_near_up": -50, "arm_near_low": -40})], events=[(0.3, "pass")],
         notes="moves the weapon from back or belt to hand_front at pass")
    pose_clip("lower-weapon", {"arm_near_up": 6, "arm_near_low": -10, "head": 6})
    alias("lower-weapon", "lower-knives", "bow-lowered")
    clip("dub", 2.4, [(0, {"arm_near_up": -60, "arm_near_low": -20}), (0.6, {"arm_near_up": -100, "arm_near_low": -20}),
                      (1.0, {"arm_near_up": -80, "arm_near_low": -10}), (1.4, {"arm_near_up": -100, "arm_near_low": -20}),
                      (1.8, {"arm_near_up": -70, "arm_near_low": -10}), (2.4, {"arm_near_up": -20})],
         events=[(1.0, "hit"), (1.8, "hit")], partner={"offset": [40, 0], "clip": "kneel"})
    clip("burn", 1.0, [(0, REST), (0.2, {"torso": -14, "head": -16, "arm_near_up": -60, "arm_far_up": -50, "hips.x": -6}),
                       (1.0, P(crouch(20), {"torso": 20, "arm_near_low": -100, "arm_far_low": -100}))],
         fx=[(0, "fire", "torso")], expr="afraid")
    clip("ash", 1.6, [(0, REST), (0.9, LYING), (1.6, LYING)], fx=[(0.9, "ash", "root")],
         notes="die, then the parts dissolve upward as ash (or toward the target for consumed)")
    alias("ash", "consumed")


# --------------------------------------------------------------------------- #
# Two actors
# --------------------------------------------------------------------------- #

def two_actors():
    shake = {"arm_near_up": -45, "arm_near_low": -30}
    clip("shake-hands", 1.4, [(0, REST), (0.4, shake), (0.6, P(shake, {"arm_near_low": -40})), (0.8, shake),
                              (1.0, P(shake, {"arm_near_low": -40})), (1.4, REST)],
         partner={"offset": [36, 0], "clip": "shake-hands"})
    clip("take-hand", 0.8, [(0, REST), (0.6, {"arm_near_up": -40, "arm_near_low": -20})],
         partner={"offset": [36, 0], "clip": "take-hand"}, swaps=[(0.5, "hand_near", "open")])
    alias("take-hand", "catch-hand", "touch-fingers", "touch-hand")
    clip("clasp-hands", 1.0, [(0, REST), (0.5, {"arm_near_up": -70, "arm_near_low": -50, "torso": 4})],
         partner={"offset": [40, 0], "clip": "clasp-hands"}, fx=[(0.5, "sigil-glow", "hand_near")])
    clip("hand-on-shoulder", 0.8, [(0, REST), (0.6, {"arm_near_up": -95, "arm_near_low": -10})],
         partner={"offset": [30, 0], "clip": "idle"})
    alias("hand-on-shoulder", "hand-on-bark")
    hug = {"arm_near_up": -80, "arm_near_low": -70, "arm_far_up": -70, "arm_far_low": -80, "torso": 8, "head": 10}
    clip("hug", 1.2, [(0, REST), (0.5, hug), (1.2, hug)], partner={"offset": [22, 0], "clip": "hug"})
    clip("grab-arm", 0.4, [(0, REST), (0.2, {"arm_near_up": -70, "arm_near_low": -10, "torso": 8}),
                           (0.4, {"arm_near_up": -55, "arm_near_low": -30})], ease="linear",
         partner={"offset": [30, 0], "clip": "grabbed"}, swaps=[(0.2, "hand_near", "fist")])
    clip("grabbed", 0.4, [(0, REST), (0.4, {"torso": -8, "head": -10, "hips.x": -4, "arm_near_up": -30})], expr="shock")
    alias("grab-arm", "grab-wrist")
    clip("tackle", 0.6, [(0, REST), (0.15, P(crouch(20), {"torso": 30})), (0.6, P(LYING_FRONT, {"root.x": 50}))],
         partner={"offset": [20, 0], "clip": "tackled"}, events=[(0.3, "hit")])
    clip("tackled", 0.6, [(0, REST), (0.3, {"torso": -20, "hips.x": -10}), (0.6, P(LYING, {"root.x": 10}))])
    clip("cloak-around", 1.4, [(0, REST), (0.7, {"arm_near_up": -100, "arm_near_low": -60, "cape_1": -40, "cape_2": -30,
                                                 "cape_3": -20}), (1.4, {"arm_near_up": -70, "arm_near_low": -80, "cape_1": -30})],
         partner={"offset": [18, 0], "clip": "idle"}, fx=[(0.9, "fade-both", "root")])
    clip("let-go", 1.0, [(0, {"arm_near_up": -40, "arm_near_low": -20}), (1.0, REST)],
         partner={"offset": [36, 0], "clip": "let-go-b"})
    clip("let-go-b", 1.0, [(0, {"arm_near_up": -40, "arm_near_low": -20}), (1.0, P(LYING, {"root.x": 30}))],
         notes="the far actor slides away")
    clip("hesitate", 1.4, [(0, REST), (0.35, {"hips.x": 8, "leg_near_up": -16}), (0.7, {"hips.x": 8}),
                           (1.05, {"hips.x": 0, "leg_far_up": 12}), (1.4, REST)], events=[(0.35, "step"), (1.05, "step")])


# --------------------------------------------------------------------------- #
# Combat (art plan §5)
# --------------------------------------------------------------------------- #

WIND = {
    "one-hand": {"arm_near_up": -150, "arm_near_low": -40, "torso": -5, "head": -4},
    "two-hand": {"arm_near_up": -165, "arm_near_low": -30, "arm_far_up": -160, "arm_far_low": -30, "torso": -18},
    "polearm": {"arm_near_up": 10, "arm_near_low": -80, "arm_far_up": -40, "arm_far_low": -60, "torso": -7, "hips.x": -4},
    "dagger": {"arm_near_up": 20, "arm_near_low": -110, "torso": -3},
    "unarmed": {"arm_near_up": 25, "arm_near_low": -120, "torso": -7, "hips.x": -3},
    "hook": {"arm_far_up": -130, "arm_far_low": -50, "torso": -7},
}
BLOW = {
    "one-hand": {"arm_near_up": -40, "arm_near_low": -6, "torso": 20, "hips.x": 6, "leg_near_up": -24},
    "two-hand": {"arm_near_up": -30, "arm_near_low": -4, "arm_far_up": -36, "arm_far_low": -6, "torso": 26, "hips.x": 6},
    "polearm": {"arm_near_up": -80, "arm_near_low": -4, "arm_far_up": -70, "arm_far_low": -10, "torso": 11, "hips.x": 10, "leg_near_up": -26},
    "dagger": {"arm_near_up": -82, "arm_near_low": -2, "torso": 13, "hips.x": 8, "leg_near_up": -20},
    "unarmed": {"arm_near_up": -84, "arm_near_low": -2, "torso": 15, "hips.x": 6, "leg_near_up": -18},
    "hook": {"arm_far_up": -50, "arm_far_low": -10, "torso": 19, "hips.x": 6, "leg_near_up": -18},
}
VICTORY = {
    "one-hand": {"arm_near_up": -160, "arm_near_low": -10, "head": -10},
    "two-hand": {"arm_near_up": -20, "arm_near_low": -20, "arm_far_up": -30, "arm_far_low": -40, "torso": -4},
    "polearm": {"arm_near_up": -20, "arm_near_low": -90, "head": -8},
    "dagger": {"arm_near_up": -40, "arm_near_low": -110, "hand_near": 40, "head": -6},
    "unarmed": {"arm_near_up": -150, "arm_near_low": -60, "head": -12},
    "hook": {"arm_far_up": -140, "arm_far_low": -30, "head": -8},
}


def follow(pose, more=0.18):
    """A blow carried on past its mark: every turn in it a share further."""
    return {k: (v * (1 + more) if not k.endswith((".x", ".y", ".sx", ".sy")) else v) for k, v in pose.items()}


def combat():
    # The fight stance breathes and bounces lightly on the knees, the feet planted, the chest and
    # the guard a beat behind the hips.
    low = P(crouch(15, near=7.0, far=-4.0), {k: v for k, v in READY.items() if not k.startswith(("leg_", "foot_", "hips"))})
    clip("ready", 1.2, [(0, READY), (0.6, P(low, {"torso": READY["torso"] + 1.0, "torso.sy": 1.006,
                                                  "arm_near_up": READY["arm_near_up"] + 2, "arm_far_up": READY["arm_far_up"] + 2}))],
         loop=True)
    for k in CLASSES:
        # The wind-up eases back into the coil and settles there; the strike snaps through the
        # mark at the hit, carries on past it, and comes back to guard.
        wound = P(READY, WIND[k])
        clip("windup-" + k, 0.22, [(0, READY), (0.16, P(READY, follow(WIND[k], 0.06))), (0.22, wound)], drag=False)
        clip("strike-" + k, 0.30, [(0, wound), (0.05, P(READY, {key: 0.75 * v + 0.25 * BLOW[k].get(key, 0.0) for key, v in WIND[k].items()})),
                                   (0.12, P(READY, BLOW[k])), (0.19, P(READY, follow(BLOW[k]))), (0.30, READY)],
             events=[(0.12, "hit")], drag=False)
        clip("victory-" + k, 1.0, [(0, READY), (0.3, P(VICTORY[k], {"hips.y": -1.5})), (0.5, VICTORY[k]), (1.0, VICTORY[k])])
    bow = {"arm_near_up": -84, "arm_near_low": 0, "arm_far_up": -86, "arm_far_low": -150, "torso": -4, "head": -2}
    for k in RANGED:
        clip("draw-" + k, 0.5, [(0, READY), (0.25, {"arm_far_up": -150, "arm_far_low": -40}), (0.5, bow)],
             layer={"arm_near_up": 6, "arm_near_low": 6, "hand_near": 6} if k == "bow" else None,
             notes="takes an arrow from the back socket's quiver")
        clip("loose-" + k, 0.3, [(0, bow), (0.06, P(bow, {"arm_far_low": -170, "arm_far_up": -100})), (0.3, READY)],
             events=[(0.06, "release")])
    cast = P(READY, ARMS_FORWARD, {"hand_near": -40, "torso": 9})
    clip("cast", 0.45, [(0, READY), (0.2, P(READY, {"arm_near_up": -20, "arm_near_low": -100, "arm_far_up": -20, "arm_far_low": -100, "torso": -3})),
                        (0.25, cast), (0.45, cast)], events=[(0.25, "release")], fx=[(0.25, "glow", "hand_front")])
    clip("channel", 1.0, [(0, cast), (0.5, P(cast, {"torso": 6, "arm_near_low": -32})), (1.0, cast)], loop=True,
         fx=[(0, "glow", "hand_front")])
    clip("throw-combat", 0.5, [(0, READY), (0.2, P(READY, {"arm_near_up": 50, "arm_near_low": -90, "torso": -7})),
                               (0.3, P(READY, {"arm_near_up": -110, "arm_near_low": -10, "torso": 17})), (0.5, READY)],
         events=[(0.28, "release")])
    clip("block", 0.4, [(0, READY), (0.12, P(READY, {"arm_far_up": -70, "arm_far_low": -80, "arm_near_up": -60, "arm_near_low": -90,
                                                    "torso": -3, "hips.x": -3})), (0.4, READY)], events=[(0.12, "hit")])
    clip("dodge", 0.4, [(0, READY), (0.15, P(crouch(22), {"hips.x": -16, "torso": 18, "head": 10})), (0.4, READY)])
    # A flinch: thrown back at once, the head snapping a beat after the chest, then easing forward
    # past guard a touch before it settles.
    hit = P(READY, {"hips.x": -6, "torso": -20, "head": -18, "arm_near_up": -20, "arm_far_up": -30})
    clip("hurt", 0.4, [(0, READY), (0.06, hit), (0.2, P(READY, {"hips.x": -3, "torso": 5, "head": 2})), (0.4, READY)],
         expr="angry", drag=False)
    clip("hurt-big", 0.45, [(0, READY), (0.1, P(crouch(10), {"hips.x": -14, "torso": -26, "head": -24, "arm_near_up": -50, "arm_far_up": -60})),
                            (0.3, P(READY, {"hips.x": -7, "torso": 7})), (0.45, P(READY, {"hips.x": -6}))], expr="shock", drag=False)
    clip("catch", 0.35, [(0, READY), (0.08, P(READY, {"hips.x": -4, "torso": -5, "arm_near_up": -70, "arm_far_up": -65})), (0.35, READY)])
    # Falling: the knees go first, the torso tips back after them, the arms fling up late, a small
    # bounce as the back meets the floor.
    buckle = P(crouch(30), {"torso": -20, "head": 8, "arm_near_up": -40, "arm_far_up": -20})
    bounce = P(LYING, {"root": LYING["root"] + 4, "head": -16, "arm_near_up": -84, "arm_far_up": -112})
    clip("downed", 0.95, [(0, READY), (0.3, buckle), (0.72, LYING), (0.82, bounce), (0.95, LYING)])
    clip("die", 1.0, [(0, READY), (0.3, buckle), (0.72, LYING), (0.82, bounce), (0.95, LYING), (1.0, LYING)],
         fx=[(0.9, "fade", "root")])
    clip("revive", 0.6, [(0, LYING), (0.3, KNEEL), (0.6, READY)])
    clip("yield", 0.6, [(0, READY), (0.6, P(KNEEL, {"arm_near_up": 8, "arm_near_low": 0, "head": 18, "torso": 14}))])


COMBAT_MAP = {
    "ready": "ready",
    "windup": {k: "windup-" + k for k in CLASSES},
    "strike": {k: "strike-" + k for k in CLASSES},
    "draw": {k: "draw-" + k for k in RANGED},
    "loose": {k: "loose-" + k for k in RANGED},
    "cast": "cast", "channel": "channel", "throw": "throw-combat", "block": "block", "dodge": "dodge",
    "hurt": "hurt", "hurt-big": "hurt-big", "downed": "downed", "die": "die", "revive": "revive",
    "yield": "yield", "walk": "walk", "run": "run",
    "victory": {k: "victory-" + k for k in CLASSES},
}

PROPS = ("crystal-break", "split", "shrink", "pull", "float-free", "lean-toward", "spin", "cross", "land")
"""03 §4.3's props and things that aren't people: they play on the prop's own strip (Burr's
`land` is the flier rig's)."""


def humanoid():
    CLIPS.clear()
    ALIASES.clear()
    NO_DRAG.clear()
    standing()
    moves()
    hands()
    two_actors()
    combat()
    for name, c in CLIPS.items():
        if c.get("speed") or name == "idle":
            continue
        plant(name, c)
        if name not in NO_DRAG:
            drag(name, c)
    for a, target in ALIASES.items():
        if a in CLIPS:
            raise ValueError("alias %s is also a clip" % a)
        if target not in CLIPS:
            raise ValueError("alias %s -> %s, which isn't a clip" % (a, target))
    return {"clips": dict(sorted(CLIPS.items())), "aliases": dict(sorted(ALIASES.items())), "combat": COMBAT_MAP}


def _creature_module():
    if not os.path.exists(os.path.join(HERE, "creature_clips.py")):
        return None
    return importlib.import_module("creature_clips")


def creature_rigs():
    """The enemy artist's creature clips, if creature_clips.py is there."""
    mod = _creature_module()
    fn = getattr(mod, "rigs", None) if mod else None
    return fn() if fn else {}


def humanoid_extras():
    """The person-rig clips only enemies play (creature_clips.humanoid_extras()), if any."""
    mod = _creature_module()
    fn = getattr(mod, "humanoid_extras", None) if mod else None
    return fn() if fn else {"clips": {}, "aliases": {}}


def build():
    person = humanoid()
    extra = humanoid_extras()
    for kind in ("clips", "aliases"):
        for name, v in extra.get(kind, {}).items():
            if name in person["clips"] or name in person["aliases"]:
                raise ValueError("creature_clips.humanoid_extras() has %s, which clips.py has already" % name)
            person[kind][name] = v
        person[kind] = dict(sorted(person[kind].items()))
    rigs = {"humanoid": person}
    for name, data in sorted(creature_rigs().items()):
        if name == "humanoid":
            raise ValueError("creature_clips.py can't redefine the humanoid rig")
        rigs[name] = data
    return {"format": "LegendQuest rig clips 1", "events": list(EVENTS), "rigs": rigs, "props": list(PROPS)}


def check():
    """Each clip played through on the bases averaged, at DRAG_FPS: how far a foot goes below the
    ground (rig px), how far an elbow or a knee bends the wrong way (degrees), and, for a clip that
    moves the figure along, how far a planted foot slides. Prints the ones that do any of it."""
    legs = {s: G.Leg(s) for s in ("near", "far")}
    arms = {s: G.Arm(s) for s in ("near", "far")}
    doc = humanoid()
    worst = []
    for name, c in doc["clips"].items():
        if "@" in name:
            continue
        n = max(2, int(c["length"] * DRAG_FPS))
        sink, bend, slide = 0.0, 0.0, 0.0
        planted = {}
        for k in range(n + 1):
            t = c["length"] * k / n
            pose = sample(c, t)
            lying = abs(pose.get("root", 0.0)) > 20
            for side, leg in legs.items():
                up, low = pose.get("leg_%s_up" % side, 0.0), pose.get("leg_%s_low" % side, 0.0)
                knee = (leg.shin0 - leg.thigh0) + low
                bend = max(bend, -knee)
                # The foot's sole against the ground, the root's own turn and offsets aside.
                hips = (pose.get("hips.x", 0.0), pose.get("hips.y", 0.0))
                if lying or abs(pose.get("hips", 0.0)) > 0.5:
                    continue
                thigh = leg.thigh0 + up
                shin = thigh + (leg.shin0 - leg.thigh0) + low
                hip = (leg.hip[0] + hips[0], leg.hip[1] + hips[1])
                kx = hip[0] + leg.l1 * math.cos(math.radians(thigh))
                ky = hip[1] + leg.l1 * math.sin(math.radians(thigh))
                ax = kx + leg.l2 * math.cos(math.radians(shin))
                ay = ky + leg.l2 * math.sin(math.radians(shin)) + pose.get("root.y", 0.0)
                foot = up + low + pose.get("foot_%s" % side, 0.0)
                lowest = max(ay + G._turn(o, foot)[1] - o[1] for o in (legs["far"].heel_off, legs["far"].toe_off))
                sink = max(sink, lowest - leg.ankle[1])
                if c.get("speed") and lowest - leg.ankle[1] > -0.3:
                    planted.setdefault(side, []).append(ax + c["speed"] * t)
            carried = sum(pose.get(b, 0.0) for b in ("root", "hips", "torso"))
            for side, arm in arms.items():
                # An elbow can only be judged when the arm hangs below the shoulder: raised, which
                # way it bends in the picture depends on how the arm's turned.
                if 15 < arm.up0 + carried + pose.get("arm_%s_up" % side, 0.0) < 165:
                    bend = max(bend, (arm.low0 - arm.up0) + pose.get("arm_%s_low" % side, 0.0))
        for xs in planted.values():
            slide = max(slide, max(xs) - min(xs)) if len(xs) < 4 else slide
        if sink > (2.5 if c.get("speed") else 1.5) or bend > 6.0:
            worst.append((name, sink, bend))
    for name, sink, bend in sorted(worst, key=lambda w: -max(w[1], w[2] / 4)):
        print("  %-22s foot %4.1f below the ground, a joint %5.1f degrees the wrong way" % (name, sink, bend))
    print("  %d clips, %d with a foot under the ground or a joint bent backward" % (len(doc["clips"]), len(worst)))


def dumps(doc):
    """clips.json's text: indented down to each track, a track's keys on its one line."""
    import re
    text = json.dumps(doc, indent=1)
    text = re.sub(r"\[\s+(-?[0-9.e+-]+),\s+(-?[0-9.e+-]+)\s+\]", r"[\1, \2]", text)
    keys = r"\[\s+((?:\[-?[0-9.e+-]+, -?[0-9.e+-]+\],?\s*)+)\]"
    return re.sub(keys, lambda m: "[" + ", ".join(re.findall(r"\[[^\[\]]+\]", m.group(1))) + "]", text) + "\n"


def main():
    if "--check" in sys.argv[1:]:
        check()
        return
    doc = build()
    os.makedirs(os.path.dirname(OUT), exist_ok=True)
    with open(OUT, "w", encoding="utf-8", newline="\r\n") as f:
        f.write(dumps(doc))
    print("  " + os.path.relpath(OUT, ROOT).replace(os.sep, "/"))
    for rig, d in doc["rigs"].items():
        print("  %-10s %3d clips, %3d aliases" % (rig, len(d["clips"]), len(d.get("aliases", {}))))


if __name__ == "__main__":
    main()
