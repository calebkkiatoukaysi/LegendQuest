"""
The real-time fight's clips (Story/11-combat.md §8, the workstreams' contract §2): the auto attacks
for every kind of weapon, a clip for every class skill, the foes' wind-ups and special moves, and
the reactions (blocks, dodges, hurts, knockdowns, deaths, victories), added to the person rig by
tools/rig/clips.py's combat().

How a blow is built (research: the timing and easing spec, §5, §9 and §10 of the feel notes):

  - the anticipation eases out into a coil (ease "out"), then a short moving hold, a few degrees
    deeper (ease "linear"), so the eye reads what's coming;
  - the strike is the fastest segment: two or three frames at 60 a second through a large angle,
    accelerating into the contact (ease "in");
  - the contact carries on into a follow-through that overshoots it and settles back (ease
    "back"), and the recovery eases home to the fight stance;
  - arcs come from turning the chain: each pose gives the upper arm, the forearm and the held
    thing as directions in the room (A()), the hips and the torso lead the shoulder and the
    shoulder the hand by a frame or two (LEAD), and the head follows late;
  - on the contact frames the hand that holds the weapon is stretched along it (a smear), and a
    `trail` window from the end of the hold to just after the contact tells the game when to draw
    the ribbon;
  - the skirt swings after the hips on a spring (clips.spring_skirt()).

A pose (F()) is the fight stance (clips.READY) with what's given changed: the torso's lean, where
the head looks, the legs (L(): the hips moved and each foot stepped from where it stands in the
stance), and each arm as an upper-arm swing forward of straight down, the elbow's bend and the
direction the held thing points (also as a swing: 0 down, 90 at the foe, 180 up, 270 behind), the
wrist worked out from it and the way that kind of thing is held (HOLD, tools/weapon_alone.py's
angles). A two-handed grip puts the far hand on the near hand's weapon by IK.

Every clip's times are in seconds from its start; FR is one frame at 60 a second.

Only needs CPython.

Written by Caleb Kiatoukaysi for Legend Quest.
"""

import math

import gait as G

FR = 1.0 / 60.0
C = None
"""tools/rig/clips.py, handed in by install(): its clip(), stand_on(), READY and the rest."""

#: Each bone's keys this many seconds before the pose's (after, if negative): the hips and the
#: torso start a blow, the shoulder follows, the hand arrives on the beat, the head comes late.
LEAD = {"torso": 2 * FR, "arm_near_up": 1 * FR, "arm_far_up": 1 * FR, "head": -2 * FR}
#: How each kind of thing is held, degrees clockwise from upright (tools/weapon_alone.py's HOLDS).
HOLD = {"blade": 115.0, "long": 8.0, "bow": 10.0, "bigbow": 6.0, "stick": 8.0, "held": 18.0, "carried": 0.0,
        "shield": 0.0}
#: The smear on a contact frame, by how the thing lies along the hand: (sx, sy).
SMEAR = {"x": (1.42, 0.86), "y": (0.88, 1.36), "f": (1.18, 0.94)}
ARMS = {"near": G.Arm("near"), "far": G.Arm("far")}
WEAPON_KINDS = ("one-hand", "two-hand", "polearm", "dagger", "unarmed", "hook", "staff", "bow", "crossbow")


def _wrap(a):
    return (a + 180.0) % 360.0 - 180.0


class A:
    """An arm: the upper arm's swing forward of straight down, the elbow's bend (forward), and the
    direction of what the hand holds (a swing too) or the wrist's own turn; `hold` the way the thing
    is held, if not the pose's."""

    def __init__(self, swing, bend=0.0, item=None, wrist=0.0, hold=None):
        self.swing, self.bend, self.item, self.wrist, self.hold = swing, bend, item, wrist, hold


def item_offset(side, hold):
    """How far round from the forearm a thing held so points, at rest (degrees of swing)."""
    angle = HOLD[hold]
    if side == "far" and angle > 20:
        angle -= 25.0
    forearm = 90.0 - ARMS[side].low0
    return (180.0 - angle) - forearm


#: The kind of weapon the clips being built are for (kit()): their stance, and how a long thing
#: (a staff, a spear, a bow) is kept pointing when a pose turns the near wrist for a gesture.
_KIT = {"kind": None}
KEEP_UP = {"staff": ("long", 168.0), "polearm": ("long", 150.0), "bow": ("bigbow", 172.0)}


def kit(kind):
    """The clips from here on are for fighters with this kind of weapon (None: no kind)."""
    _KIT["kind"] = kind


def arm_channels(side, spec, carried, hold):
    wrist = spec.wrist
    keep = KEEP_UP.get(_KIT["kind"])
    if side == "near" and spec.item is None and keep and spec.hold is None:
        # A gesture with a staff or a spear in the hand: the hand turns about the grip to keep it
        # upright, as a staff is held while the arm moves, rather than swinging it round with the
        # hand (the fist is round, so the wrist's turn doesn't show; a staff swung down over the
        # body does).
        # Not wrapped: as the arm moves smoothly between poses, so does this, and the staff never
        # spins round the long way between two keys.
        wrist = spec.swing + spec.bend + item_offset(side, keep[0]) - keep[1]
    if spec.item is not None:
        wrist = _wrap(spec.swing + spec.bend + item_offset(side, spec.hold or hold) - spec.item)
    ch = ARMS[side].channels(spec.swing, spec.bend, wrist, carried)
    return {k: round(v, 2) for k, v in ch.items()}


def _local_wrist(side, pose):
    """Where a pose puts an arm's wrist in the torso's frame (rig px from the root at rest)."""
    a = ARMS[side]
    up = pose.get("arm_%s_up" % side, 0.0)
    low = pose.get("arm_%s_low" % side, 0.0)
    l1, l2 = math.dist(a.shoulder, a.elbow), math.dist(a.elbow, a.wrist)
    d1, d2 = math.radians(a.up0 + up), math.radians(a.low0 + up + low)
    e = (a.shoulder[0] + l1 * math.cos(d1), a.shoulder[1] + l1 * math.sin(d1))
    return (e[0] + l2 * math.cos(d2), e[1] + l2 * math.sin(d2))


def ik(side, target):
    """The arm's channels (in the torso's frame) that put its wrist at `target`, the elbow bending
    forward as an elbow does."""
    a = ARMS[side]
    l1, l2 = math.dist(a.shoulder, a.elbow), math.dist(a.elbow, a.wrist)
    s = a.shoulder
    d = min(max(math.dist(s, target), abs(l1 - l2) + 1e-3), l1 + l2 - 1e-3)
    toward = math.degrees(math.atan2(target[1] - s[1], target[0] - s[0]))
    at = math.degrees(math.acos((l1 * l1 + d * d - l2 * l2) / (2 * l1 * d)))
    best = None
    for k in (1, -1):
        up_dir = toward + k * at
        e = (s[0] + l1 * math.cos(math.radians(up_dir)), s[1] + l1 * math.sin(math.radians(up_dir)))
        low_dir = math.degrees(math.atan2(target[1] - e[1], target[0] - e[0]))
        bend = _wrap(up_dir - low_dir)
        score = bend if bend > -2 else -999
        if best is None or score > best[0]:
            best = (score, up_dir, low_dir)
    _, up_dir, low_dir = best
    up = _wrap(up_dir - a.up0)
    return {"arm_%s_up" % side: round(up, 2), "arm_%s_low" % side: round(_wrap(low_dir - a.low0 - up), 2)}


_RY = None


def L(hx=0.0, dy=0.0, near=0.0, far=0.0, lift=(0.0, 0.0), roll=(0.0, 0.0)):
    """The legs: the hips moved `hx` forward and `dy` lower than in the fight stance, each foot
    stepped forward (rig px) and lifted from where it stands in the stance, and rolled (degrees)."""
    return C.stand_on(hx, _RY + dy, near=(7.0 + near, -lift[0]), far=(-4.0 + far, -lift[1]), roll=roll)


def F(base=None, torso=None, look=None, legs=None, near=None, far=None, grip=None, root=None, hold=("blade", "blade"),
      far_to=None, **extra):
    """A pose: the fight stance (or `base`) with the torso leaned `torso` degrees forward, the head
    turned to look `look` degrees down from level (negative up), the legs `legs` (L()), the near
    and far arms given as A()s in the room's directions, the far hand put on the near hand's grip
    `grip` rig px along what it holds (or at `far_to`, a point in the torso's frame), and any other
    channels as keywords ("hand_near__sx")."""
    out = dict(base if base is not None else C.READY)
    if legs is not None:
        for k in list(out):
            if k.startswith(("leg_", "foot_", "hips.")):
                del out[k]
        out.update(legs)
    if root is not None:
        out["root"] = root
    if torso is not None:
        out["torso"] = torso
    carried = out.get("root", 0.0) + out.get("torso", 0.0)
    if look is not None:
        out["head"] = round(look - carried, 2)
    if near is not None:
        out.update(arm_channels("near", near, carried, hold[0]))
    if far is not None:
        out.update(arm_channels("far", far, carried, hold[1]))
    if grip is not None and near is not None:
        w = _local_wrist("near", out)
        item = near.item if near.item is not None else near.swing + near.bend
        d = math.radians(90.0 - item - carried)
        target = (w[0] + grip * math.cos(d), w[1] + grip * math.sin(d))
        out.update(ik("far", target))
        out["hand_far"] = 0.0
    if far_to is not None:
        out.update(ik("far", far_to(out) if callable(far_to) else far_to))
    for k, v in extra.items():
        out[k.replace("__", ".")] = v
    return out


def at_near_wrist(dx=0.0, dy=0.0):
    """A far hand's target: the near wrist, moved (dx, dy) in the torso's frame."""
    return lambda pose: (lambda w: (w[0] + dx, w[1] + dy))(_local_wrist("near", pose))


class B:
    """A beat of a clip: `f` frames after the one before, the pose it reaches, the ease out of it,
    its events (names, or (name, arg)), a smear on its frame ((side, "x"|"y"|"f")) and whether it
    opens or closes a trail window."""

    def __init__(self, f, pose, ease="smooth", ev=(), smear=None, trail=None):
        self.f, self.pose, self.ease, self.ev, self.smear, self.trail = f, pose, ease, ev, smear, trail


def countered(keys):
    """The upper arms that mustn't lead their forearms in a clip of these keys: those that, between
    two keys, swing a long way while the elbow opens or closes against them (a thrust: the shoulder
    comes forward as the elbow straightens, and the hand goes straight out). Led a frame ahead, such
    an upper arm would fling the forearm and what it holds round for that frame and back."""
    out = set()
    for side in ("near", "far"):
        up, low = "arm_%s_up" % side, "arm_%s_low" % side
        for (_, a, *_), (_, b, *_) in zip(keys, keys[1:]):
            du = b.get(up, 0.0) - a.get(up, 0.0)
            dl = b.get(low, 0.0) - a.get(low, 0.0)
            if abs(du) > 30 and du * dl < 0 and abs(du + dl) < 0.6 * max(abs(du), abs(dl)):
                out.add(up)
    return out


def unwound(keys):
    """The keys with each hand's turn taken the shortest way round over the whole clip: a pose
    gives a turn between -180 and 180 (arm_channels()), so a hand going from 170 to -170 would
    otherwise spin the long way round, the weapon with it. Each key's turn may be put a whole turn
    or two either way; the first key stays as it is, and so does the last if it is the first's
    (the clip ends on the stance it started on, where the next clip picks up), and of the rest the
    choice that turns the hand least in all is taken."""
    out = [(k[0], dict(k[1])) + tuple(k[2:]) for k in keys]
    turns = (-720.0, -360.0, 0.0, 360.0, 720.0)
    for ch in ("hand_near", "hand_far"):
        if not any(ch in k[1] for k in out):
            continue
        values = [k[1].get(ch, 0.0) for k in out]
        home = abs(values[-1] - values[0]) < 1e-6
        # Least turning to each key at each choice of whole turns: {offset: (cost, path)}.
        best = {0.0: (0.0, [values[0]])}
        for i, v in enumerate(values[1:], 1):
            fixed = i == len(values) - 1 and home
            step = {}
            for o in ((0.0,) if fixed else turns):
                here = v + o
                cost, path = min(((c + abs(here - p[-1]), p) for c, p in best.values()), key=lambda r: r[0])
                step[o] = (cost, path + [here])
            best = step
        short = min(best.values(), key=lambda r: r[0])[1]
        for k, v, was in zip(out, short, values):
            if abs(v - was) > 1e-6:
                k[1][ch] = round(v, 2)
    return out


def seq(name, beats, start=None, first="out", settle=16, end=None, blend=0.04, loop=False, lead=None, notes=None,
        recover=True, spring=True, fps=60, front=None, tracks=None, length=None, unwind=True):
    """A clip from beats: from `start` (the stance) eased `first`, through each beat, and `settle`
    frames back to `end` (the stance; settle 0 ends on the last beat). Each smear stretches its
    hand over the three frames either side of its beat, and opens a trail window over them unless
    the beats open and close their own; `recover` puts a recover event a little after the last hit
    or release. Each hand is taken the shortest way round over the clip (unwound()) unless
    `unwind` is false, for a blow whose weapon is meant to go the long way round (OVER_THE_TOP)."""
    kind = _KIT["kind"]
    if start is None:
        start = stance(kind) if kind else C.READY
    if end is None and kind:
        end = stance(kind)
    keys = [(0.0, start, first)]
    events, tr, smears, windows, opened = [], {}, [], [], None
    t = 0.0
    for b in beats:
        t += b.f * FR
        keys.append((t, b.pose, b.ease))
        for e in b.ev:
            e = (e,) if isinstance(e, str) else tuple(e)
            # A charge's glow and a weapon's glint start as the beat does (the glow grows through
            # the wind-up; the glint marks the start of the hold); everything else is on its pose.
            early = e[0] == "fx" and len(e) > 1 and str(e[1]).startswith(("glow", "glint"))
            events.append((t - b.f * FR if early else t,) + e)
        if b.smear:
            smears.append((t, b.smear))
        if b.trail == "start":
            opened = t
        elif b.trail == "end" and opened is not None:
            windows.append((opened, t, "weapon"))
            opened = None
    if settle:
        t += settle * FR
        keys.append((t, C.READY if end is None else end, "smooth"))
    total = length or t
    for when, (side, axis) in smears:
        sx, sy = SMEAR[axis]
        for ch, peak in (("sx", sx), ("sy", sy)):
            track = tr.setdefault("hand_%s.%s" % (side, ch), [(0.0, 1.0)])
            a = max(track[-1][0] + 0.002, when - 3 * FR)
            track += [(a, 1.0, "in"), (when, peak, "out"), (min(total, when + 3 * FR), 1.0)]
        if not any(b.trail for b in beats):
            windows.append((max(0.0, when - 3 * FR), min(total, when + 3 * FR), "weapon"))
    tr.update(tracks or {})
    if unwind:
        keys = unwound(keys)
    if lead is None:
        lead = {b: v for b, v in LEAD.items() if b not in countered(keys)}
    blows = [e[0] for e in events if e[1] in ("hit", "release")]
    if recover and blows and not loop:
        events.append((min(total, max(blows) + 0.1), "recover"))
    return C.clip(name, total, keys, loop=loop, events=events, ease="smooth", blend=blend, trail=windows or None,
                  lead=lead, tracks=tr, plant_fps=fps, spring=spring, drag=False,
                  notes=notes, front=front)


# --------------------------------------------------------------------------- #
# The auto attacks: three swings for each kind of weapon
# --------------------------------------------------------------------------- #

OVER_THE_TOP = {("dagger", 3)}
"""The swings whose weapon goes the long way round, kept as their keys say rather than unwound():
the dagger's third is wound back behind the head and brought forward over the top and down, the
blade turning more than half a circle in the strike, which the shortest way round would turn back
the other way, past the face."""


def swings():
    """{kind: [(wind, coil, contact, follow, frames, smear, step back?), ...]}: each kind's three
    auto-attack swings. frames are (anticipation, hold, strike, follow-through, recovery)."""
    bl = ("blade", "blade")
    lg = ("long", "blade")
    out = {}
    out["one-hand"] = [
        # A forehand chop over the top: the blade goes from behind the head to low in front.
        (F(torso=-8, look=-6, legs=L(-3, 1), near=A(160, 45, item=250), far=A(60, 50), hold=bl),
         F(torso=-10, look=-7, legs=L(-4, 1.5), near=A(168, 48, item=262), far=A(65, 55), hold=bl),
         F(torso=22, look=4, legs=L(8, 4, near=4), near=A(82, 8, item=80), far=A(-15, 40), hold=bl),
         F(torso=26, look=5, legs=L(9, 4.5, near=4), near=A(58, 4, item=44), far=A(-25, 35), hold=bl),
         (6, 2, 3, 8, 16), "x", False),
        # A rising backhand from low behind to high in front.
        (F(torso=14, look=2, legs=L(-1, 4), near=A(-35, 15, item=-70), far=A(70, 60), hold=bl),
         F(torso=16, look=3, legs=L(-1.5, 4.5), near=A(-42, 12, item=-82), far=A(74, 62), hold=bl),
         F(torso=-4, look=-6, legs=L(4, 1, near=3), near=A(110, 6, item=125), far=A(10, 50), hold=bl),
         F(torso=-8, look=-8, legs=L(4.5, 1, near=3), near=A(142, 8, item=168), far=A(0, 45), hold=bl),
         (5, 2, 3, 8, 15), "x", False),
        # A lunging thrust: the elbow drawn back, then the arm and the blade in one line at the foe.
        (F(torso=-6, look=0, legs=L(-5, 1, lift=(3, 0)), near=A(-40, 125, item=88), far=A(75, 30), hold=bl),
         F(torso=-8, look=0, legs=L(-6, 1.5, lift=(4, 0)), near=A(-45, 128, item=90), far=A(78, 30), hold=bl),
         F(torso=14, look=6, legs=L(9, 4, near=14), near=A(86, 2, item=90), far=A(-30, 20), hold=bl),
         F(torso=16, look=7, legs=L(10, 4.5, near=14), near=A(90, 0, item=92), far=A(-35, 20), hold=bl),
         (5, 3, 2, 9, 17), "x", True),
    ]
    out["two-hand"] = [
        (F(torso=-12, look=-8, legs=L(-4, 1), near=A(170, 40, item=245), grip=-8, hold=bl),
         F(torso=-14, look=-9, legs=L(-5, 1.5), near=A(176, 42, item=256), grip=-8, hold=bl),
         F(torso=26, look=6, legs=L(9, 7, near=5), near=A(78, 10, item=70), grip=-8, hold=bl),
         F(torso=30, look=8, legs=L(10, 8, near=5), near=A(55, 5, item=36), grip=-8, hold=bl),
         (8, 3, 3, 9, 18), "x", False),
        (F(torso=12, look=2, legs=L(-2, 4), near=A(-40, 20, item=-75), grip=-8, hold=bl),
         F(torso=14, look=3, legs=L(-2.5, 4.5), near=A(-48, 18, item=-90), grip=-8, hold=bl),
         F(torso=-6, look=-6, legs=L(5, 2, near=4), near=A(105, 10, item=130), grip=-8, hold=bl),
         F(torso=-10, look=-8, legs=L(5.5, 2, near=4), near=A(140, 10, item=170), grip=-8, hold=bl),
         (7, 2, 3, 9, 17), "x", False),
        (F(torso=-6, look=0, legs=L(-5, 1, lift=(3, 0)), near=A(-30, 120, item=92), grip=-8, hold=bl),
         F(torso=-8, look=0, legs=L(-6, 1.5, lift=(4, 0)), near=A(-36, 124, item=94), grip=-8, hold=bl),
         F(torso=16, look=6, legs=L(10, 5, near=15), near=A(82, 4, item=88), grip=-8, hold=bl),
         F(torso=18, look=7, legs=L(11, 5.5, near=15), near=A(88, 2, item=90), grip=-8, hold=bl),
         (6, 3, 2, 9, 18), "x", True),
    ]
    out["polearm"] = [
        (F(torso=-5, legs=L(-5, 1, lift=(2, 0)), near=A(-25, 110, item=92), grip=16, hold=lg),
         F(torso=-7, legs=L(-6, 1.5, lift=(3, 0)), near=A(-32, 114, item=93), grip=16, hold=lg),
         F(torso=12, look=5, legs=L(10, 4, near=14), near=A(80, 6, item=88), grip=16, hold=lg),
         F(torso=14, look=6, legs=L(11, 4.5, near=14), near=A(86, 3, item=90), grip=16, hold=lg),
         (5, 2, 2, 9, 17), "y", True),
        (F(torso=-8, look=-6, legs=L(-3, 1), near=A(150, 40, item=205), grip=16, hold=lg),
         F(torso=-10, look=-7, legs=L(-4, 1.5), near=A(156, 42, item=216), grip=16, hold=lg),
         F(torso=14, look=5, legs=L(5, 3, near=4), near=A(85, 10, item=70), grip=16, hold=lg),
         F(torso=18, look=6, legs=L(6, 3.5, near=4), near=A(65, 8, item=44), grip=16, hold=lg),
         (6, 2, 3, 9, 16), "y", False),
        (F(torso=6, legs=L(-3, 3), near=A(-30, 70, item=-50), grip=16, hold=lg),
         F(torso=8, legs=L(-3.5, 3.5), near=A(-35, 72, item=-62), grip=16, hold=lg),
         F(torso=12, look=8, legs=L(4, 5, near=3), near=A(40, 40, item=55), grip=16, hold=lg),
         F(torso=10, look=6, legs=L(4.5, 5, near=3), near=A(55, 40, item=86), grip=16, hold=lg),
         (6, 2, 3, 8, 16), "y", False),
    ]
    out["staff"] = [
        (F(torso=-10, look=-8, legs=L(-3, 1), near=A(165, 35, item=225), grip=14, hold=lg),
         F(torso=-12, look=-9, legs=L(-4, 1.5), near=A(172, 38, item=238), grip=14, hold=lg),
         F(torso=18, look=5, legs=L(5, 4, near=5), near=A(85, 15, item=60), grip=14, hold=lg),
         F(torso=22, look=6, legs=L(6, 4.5, near=5), near=A(60, 10, item=30), grip=14, hold=lg),
         (7, 3, 3, 9, 17), "y", False),
        (F(torso=10, look=2, legs=L(-2, 3), near=A(-30, 25, item=-60), far=A(70, 50), hold=lg),
         F(torso=12, look=3, legs=L(-2.5, 3.5), near=A(-38, 22, item=-75), far=A(74, 52), hold=lg),
         F(torso=-4, look=-5, legs=L(4, 1, near=3), near=A(105, 10, item=135), far=A(10, 40), hold=lg),
         F(torso=-8, look=-7, legs=L(4.5, 1, near=3), near=A(140, 10, item=175), far=A(0, 40), hold=lg),
         (5, 2, 3, 8, 15), "y", False),
        (F(torso=-5, legs=L(-4, 1), near=A(-20, 100, item=95), grip=14, hold=lg),
         F(torso=-7, legs=L(-5, 1.5), near=A(-26, 104, item=97), grip=14, hold=lg),
         F(torso=10, look=5, legs=L(7, 3, near=10), near=A(78, 6, item=90), grip=14, hold=lg),
         F(torso=12, look=6, legs=L(8, 3.5, near=10), near=A(84, 4, item=92), grip=14, hold=lg),
         (5, 2, 2, 9, 16), "y", True),
    ]
    out["dagger"] = [
        (F(torso=-3, legs=L(-3, 1), near=A(-30, 115, item=95), far=A(50, 60, item=120), hold=bl),
         F(torso=-4, legs=L(-3.5, 1.2), near=A(-34, 118, item=96), far=A(52, 62, item=122), hold=bl),
         F(torso=12, look=6, legs=L(6, 3, near=8), near=A(84, 3, item=92), far=A(0, 50, item=100), hold=bl),
         F(torso=14, look=7, legs=L(6.5, 3.2, near=8), near=A(88, 2, item=93), far=A(-5, 50, item=100), hold=bl),
         (4, 1, 2, 7, 13), "x", True),
        (F(torso=-6, legs=L(-2, 1), near=A(-20, 110, item=150), far=A(140, 40, item=220), hold=bl),
         F(torso=-8, legs=L(-2.5, 1.2), near=A(-22, 112, item=150), far=A(146, 42, item=232), hold=bl),
         F(torso=14, look=6, legs=L(4, 2, near=3), near=A(-20, 110, item=150), far=A(60, 10, item=40), hold=bl),
         F(torso=16, look=7, legs=L(4.5, 2.2, near=3), near=A(-22, 110, item=150), far=A(35, 8, item=10), hold=bl),
         (4, 2, 3, 7, 14), None, False),
        (F(torso=-6, look=-6, legs=L(-2, 1), near=A(155, 30, item=30), far=A(40, 60, item=110), hold=bl),
         F(torso=-8, look=-7, legs=L(-2.5, 1.5), near=A(162, 32, item=25), far=A(42, 62, item=110), hold=bl),
         F(torso=16, look=6, legs=L(4, 3, near=4), near=A(70, 5, item=-10), far=A(0, 50, item=100), hold=bl),
         F(torso=19, look=7, legs=L(4.5, 3.5, near=4), near=A(45, 3, item=-20), far=A(-5, 50, item=100), hold=bl),
         (5, 2, 2, 8, 14), "x", False),
    ]
    up_fist = dict(near=A(20, 110, wrist=-10), far=A(40, 105))
    out["unarmed"] = [
        (F(torso=-2, legs=L(-2, 1), near=A(10, 120, wrist=-10), far=A(40, 100)),
         F(torso=-3, legs=L(-2.5, 1.2), near=A(5, 124, wrist=-10), far=A(40, 102)),
         F(torso=10, look=5, legs=L(5, 2.5, near=6), near=A(88, 4, wrist=-15), far=A(30, 110)),
         F(torso=11, look=5, legs=L(5.5, 2.7, near=6), near=A(91, 2, wrist=-15), far=A(30, 110)),
         (4, 1, 2, 7, 13), "f", True),
        (F(torso=-6, legs=L(-2, 1.5), near=A(30, 100, wrist=-10), far=A(10, 120)),
         F(torso=-8, legs=L(-2.5, 1.8), near=A(32, 102, wrist=-10), far=A(4, 124)),
         F(torso=16, look=5, legs=L(6, 3, near=5), near=A(-10, 115, wrist=-10), far=A(88, 4)),
         F(torso=18, look=6, legs=L(6.5, 3.2, near=5), near=A(-12, 115, wrist=-10), far=A(92, 2)),
         (5, 2, 2, 7, 14), None, True),
        (F(torso=-12, look=2, legs=L(-3, 2, near=12, lift=(30, 0)), **up_fist),
         F(torso=-14, look=2, legs=L(-4, 2, near=14, lift=(36, 0)), **up_fist),
         F(torso=-22, look=6, legs=L(-5, 1, near=82, lift=(46, 0)), near=A(10, 110, wrist=-10), far=A(50, 100)),
         F(torso=-20, look=6, legs=L(-4, 1, near=76, lift=(44, 0)), near=A(10, 110, wrist=-10), far=A(50, 100)),
         (6, 3, 2, 8, 15), None, False),
    ]
    hk = ("blade", "blade")
    out["hook"] = [
        (F(torso=-8, look=-6, legs=L(-3, 1), near=A(-15, 110, item=150), far=A(155, 40, item=240), hold=hk),
         F(torso=-10, look=-7, legs=L(-4, 1.5), near=A(-18, 112, item=150), far=A(162, 42, item=252), hold=hk),
         F(torso=18, look=5, legs=L(5, 3, near=4), near=A(-20, 110, item=150), far=A(70, 10, item=50), hold=hk),
         F(torso=21, look=6, legs=L(6, 3.5, near=4), near=A(-22, 110, item=150), far=A(45, 8, item=15), hold=hk),
         (6, 2, 3, 8, 15), ("far", "x"), False),
        (F(torso=-8, look=-6, legs=L(-3, 1), near=A(160, 45, item=250), far=A(40, 60, item=130), hold=hk),
         F(torso=-10, look=-7, legs=L(-4, 1.5), near=A(166, 48, item=262), far=A(44, 62, item=130), hold=hk),
         F(torso=16, look=4, legs=L(5, 3, near=4), near=A(80, 8, item=68), far=A(-10, 50, item=110), hold=hk),
         F(torso=19, look=5, legs=L(6, 3.5, near=4), near=A(50, 4, item=26), far=A(-15, 50, item=110), hold=hk),
         (5, 2, 3, 7, 14), "x", False),
        (F(torso=12, legs=L(-1, 4), near=A(-10, 110, item=150), far=A(-30, 20, item=-40), hold=hk),
         F(torso=14, legs=L(-1.5, 4.5), near=A(-12, 112, item=150), far=A(-38, 18, item=-52), hold=hk),
         F(torso=-6, look=-6, legs=L(4, 1, near=3), near=A(-20, 110, item=150), far=A(110, 15, item=140), hold=hk),
         F(torso=-9, look=-8, legs=L(4.5, 1, near=3), near=A(-22, 110, item=150), far=A(140, 10, item=176), hold=hk),
         (6, 2, 3, 8, 15), ("far", "x"), False),
    ]
    return out


def bow_poses(kind, aim=0.0):
    """A bow's or a crossbow's poses, aimed `aim` degrees above level: {nock, set, drawn, anchor,
    loosed, follow}."""
    if kind == "bow":
        hold = ("bigbow", "blade")
        arm = lambda extra=0.0: A(88 + aim + extra, 2, item=180 + aim + extra)  # noqa: E731
        t = -2 - aim * 0.5
        quiver = (2.0, -150.0)
        return {
            "nock": F(torso=4, look=2, near=A(40, 60, item=150), far_to=quiver, hold=hold),
            "set": F(torso=t, look=-aim, near=arm(), far_to=at_near_wrist(-3, 0), hold=hold),
            "drawn": F(torso=t - 1, look=-aim, near=arm(), far_to=(13.0, -133.0), hold=hold),
            "anchor": F(torso=t - 1.5, look=-aim, near=arm(), far_to=(11.0, -132.5), hold=hold),
            "loosed": F(torso=t - 3, look=-aim - 2, near=arm(3), far_to=(-1.0, -136.0), hold=hold),
            "follow": F(torso=t - 2, look=-aim - 1, near=arm(1), far_to=(2.0, -134.0), hold=hold),
        }
    hold = ("blade", "blade")
    arm = lambda extra=0.0, swing=40.0, bend=60.0: A(swing + aim, bend, item=90 + aim + extra)  # noqa: E731
    support = at_near_wrist(9, 3)
    return {
        "nock": F(torso=6, look=4, legs=L(-1, 2), near=A(10, 80, item=60), far=A(30, 60), hold=hold),
        "set": F(torso=-2, look=-aim, near=arm(), far_to=support, hold=hold),
        "drawn": F(torso=-3, look=-aim, near=arm(), far_to=support, hold=hold),
        "anchor": F(torso=-3.5, look=-aim, near=arm(1), far_to=support, hold=hold),
        "loosed": F(torso=-8, look=-aim - 4, legs=L(-2, 1), near=arm(16, 46), far_to=support, hold=hold),
        "follow": F(torso=-5, look=-aim - 2, legs=L(-1.5, 1), near=arm(6), far_to=support, hold=hold),
    }


def stance(kind):
    """The fight stance for a kind of weapon: clips.READY's body and legs, with the weapon held
    the way that kind is held on guard (a spear's head at the foe, a greatsword in both hands, the
    fists up, the bow low)."""
    if kind not in _STANCES:
        r = C.READY
        bl, lg = ("blade", "blade"), ("long", "blade")
        poses = {
            "one-hand": r,
            "two-hand": F(near=A(5, 85, item=140), grip=-8, hold=bl),
            "polearm": F(near=A(5, 80, item=72), grip=16, hold=lg),
            "staff": F(near=A(5, 80, item=165), far=A(22, 71), hold=lg),
            "dagger": F(near=A(5, 85, item=105), far=A(25, 75, item=110), hold=bl),
            "unarmed": F(near=A(10, 115, wrist=-10), far=A(35, 105)),
            "hook": F(near=A(5, 85, item=140), far=A(25, 75, item=115), hold=bl),
            "bow": F(near=A(25, 45, item=172, hold="bigbow"), far=A(10, 60)),
            "crossbow": F(near=A(5, 85, item=85), far_to=at_near_wrist(9, 3), hold=bl),
        }
        _STANCES.update(poses)
    return _STANCES[kind]


_STANCES = {}


def autos():
    for kind, three in swings().items():
        for n, (wind, coil, contact, follow, frames, smear, back) in enumerate(three, 1):
            a, h, s, f, r = frames
            side_smear = None if smear is None else (smear if isinstance(smear, tuple) else ("near", smear))
            beats = [B(a, wind, "linear"), B(h, coil, "in", trail="start"),
                     B(s, contact, "back", ev=("hit",), smear=side_smear), B(f, follow, "smooth", trail="end")]
            settle = r
            if back:
                # A lunge steps back to the stance, the foot lifting on the way.
                step = F(base=follow, legs=L(3, 2, near=5, lift=(5, 0)))
                beats.append(B(int(r * 0.55), step, "smooth"))
                settle = r - int(r * 0.55)
            if not side_smear:
                beats[1].trail = beats[3].trail = None
                beats[1].trail, beats[3].trail = "start", "end"
            if kind == "polearm" and n == 1:
                # The thrust lifts the arm through more than a right angle to reach forward, so a
                # ribbon over the whole sweep is a half-disc over the head: it opens at the contact.
                beats[1].trail, beats[2].trail = None, "start"
            seq("auto-%s-%d" % (kind, n), beats, settle=settle, blend=0.04, start=stance(kind), end=stance(kind),
                notes="auto attack %d of 3 with a %s weapon" % (n, kind), unwind=(kind, n) not in OVER_THE_TOP)
    for kind in ("bow", "crossbow"):
        for n, (aim, quick) in enumerate(((0.0, False), (0.0, True), (22.0, False)), 1):
            p = bow_poses(kind, aim)
            beats = []
            if not quick:
                beats.append(B(5, p["nock"], "smooth"))
            beats += [B(4 if quick else 5, p["set"], "out"), B(5 if quick else 6, p["drawn"], "linear"),
                      B(1 if quick else 3, p["anchor"], "in"),
                      B(2, p["loosed"], "back", ev=(("release", "hand_near"),)), B(6, p["follow"], "smooth")]
            seq("auto-%s-%d" % (kind, n), beats, settle=10 if quick else 12, blend=0.05, start=stance(kind), end=stance(kind),
                notes="auto attack %d of 3 with a %s: %s" % (n, kind, ("a quick snap shot" if quick else
                                                                           "aimed high" if aim else "a full draw")))


# --------------------------------------------------------------------------- #
# Wind-ups, reactions, the stance
# --------------------------------------------------------------------------- #

def telegraphs():
    """Each kind's held coil for a foe's big move: its first auto's coil, deeper and lower, held
    with a tremble, a glint on the weapon as it starts."""
    sw = swings()
    for kind in WEAPON_KINDS:
        if kind in sw:
            coil = dict(sw[kind][0][1])
        else:
            coil = dict(bow_poses(kind)["anchor"])
        coil = F(base=coil, legs=L(coil.get("hips.x", 0) - 1, coil.get("hips.y", 0) - _RY + 2.5))
        shake = dict(coil)
        shake["torso"] = coil.get("torso", 0) - 1.5
        shake["torso.sy"] = 1.012
        seq("telegraph-" + kind, [B(18, shake, "smooth"), B(18, coil, "smooth")], start=coil, first="smooth",
            settle=0, loop=True, blend=0.15, recover=False, lead={}, spring=False,
            notes="a foe's wind-up held for as long as its telegraph lasts; auto-%s-1 strikes out of it" % kind)
        CLIPS = C.CLIPS
        CLIPS["telegraph-" + kind]["events"] = [[0.0, "telegraph"], [0.0, "fx", "glint"]]


def breathing():
    """The fight stance breathing: the chest lifting, the knees giving a touch, the hands rising
    and falling a beat behind the chest; `ready` with the sword's guard, and `ready-<kind>` with
    each kind of weapon's, which that kind's autos start and end on."""
    for name, r in [("ready", C.READY)] + [("ready-" + k, stance(k)) for k in WEAPON_KINDS]:
        low = dict(r)
        low.update(L(r.get("hips.x", 0.0), 0.8))
        low["torso"] = r["torso"] + 1.2
        low["arm_near_up"] = r["arm_near_up"] + 2.0
        low["arm_far_up"] = r["arm_far_up"] + 2.0
        low["head"] = r["head"] - 0.6
        low["torso.sy"] = 1.008
        C.clip(name, 1.6, [(0.0, r), (0.8, low), (1.6, r)], loop=True, blend=0.2,
               lead={"arm_near_up": -0.12, "arm_far_up": -0.1, "head": -0.15}, drag=False,
               notes="the fight stance, breathing; " + ("every skill starts and ends on its first frame" if name == "ready"
                                                         else "the %s autos start and end on its first frame" % name[6:]))


def reactions():
    r = C.READY
    # A block: the guard snaps up across the body, the blow lands on it and pushes the figure back
    # a little; then the guard comes down.
    guard = F(torso=-4, look=-2, legs=L(-2, 1.5), near=A(60, 80, item=180), far=A(70, 50))
    jolt = F(torso=-8, look=-4, legs=L(-5, 2), near=A(58, 84, item=184), far=A(66, 54))
    seq("block", [B(5, guard, "out"), B(2, guard, "linear", ev=("hit",)), B(3, jolt, "back"), B(6, guard, "smooth")],
        settle=10, blend=0.03, notes="a blow caught on the guard (or the shield)")
    # A dodge: a quick sway back and down out of the blow's way, and back.
    away = F(torso=-14, look=-6, legs=L(-12, 5, far=-6), near=A(20, 90, item=150), far=A(50, 70))
    seq("dodge", [B(4, away, "out"), B(6, F(base=away, torso=-12, legs=L(-13, 5.5, far=-6)), "smooth")],
        settle=14, blend=0.03, recover=False, notes="leaning back out of a blow's way")
    # Hurt: thrown back at once (two frames), the head snapping later, and easing back past guard.
    hit = F(torso=-16, look=-14, legs=L(-6, 1), near=A(-5, 70, item=130), far=A(40, 40))
    back = F(torso=4, look=2, legs=L(-2, 1.5))
    seq("hurt", [B(2, hit, "out", ev=("hit",)), B(9, back, "smooth")], first="out", settle=12, blend=0.03,
        notes="a flinch: the hit at its first frame")
    hit2 = F(torso=-24, look=-22, legs=L(-14, 3, far=-6), near=A(30, 40, item=160), far=A(70, 20))
    stagger = F(torso=8, look=4, legs=L(-8, 4, near=-5, far=-6))
    seq("hurt-big", [B(3, hit2, "out", ev=("hit",)), B(8, F(base=hit2, legs=L(-12, 3, near=-5, far=-6, lift=(4, 0))), "smooth"),
                     B(8, stagger, "smooth")], settle=12, blend=0.03, notes="a big hit or a crit: knocked back a step")
    # Knocked down: thrown back off the feet, the back meeting the floor (land), a bounce, still.
    lying = C.LYING
    buckle = F(torso=-26, look=-24, legs=L(-10, 6, far=-8), near=A(50, 30), far=A(90, 20))
    air = dict(lying)
    air.update({"root": -55, "root.y": round(-22 * C.K, 2), "root.x": round(12 * C.K, 2), "arm_near_up": -120,
                "arm_far_up": -140, "leg_near_up": -40, "leg_near_low": 40, "leg_far_up": -20, "leg_far_low": 30,
                "head": -20})
    bounce = C.P(lying, {"root": lying["root"] + 5, "root.y": lying["root.y"] - 3, "head": -22, "arm_near_up": -90,
                         "arm_far_up": -118, "leg_near_up": -18})
    C.clip("knockdown", 0.75, [(0, r, "out"), (3 * FR, buckle, "linear"), (12 * FR, air, "in"),
                               (24 * FR, lying, "out"), (30 * FR, bounce, "smooth"), (0.75, lying)],
           events=[(0.0, "hit"), (24 * FR, "land"), (24 * FR, "fx", "dust")], blend=0.03, drag=False,
           notes="knocked off the feet; getup or downed follows")
    # Getting up: roll to the side, push up onto a knee, stand into the stance.
    side = C.P(lying, {"root": -70, "arm_far_up": -60, "arm_far_low": -40, "head": -30, "leg_near_up": -40,
                       "leg_near_low": 60})
    C.clip("getup", 0.9, [(0, lying), (0.2, side), (0.5, C.P(C.KNEEL, {"torso": 30, "head": -6, "arm_near_up": -20})),
                          (0.72, C.P(C.KNEEL_RISE, {"torso": 14})), (0.9, r)], blend=0.08, drag=False,
           events=[(0.72, "step"), (0.9, "recover")], notes="from knocked down to the fight stance")
    # Downed: lying hurt on the floor, breathing, a hand reaching a little.
    breath = C.P(lying, {"torso.sy": 1.02, "arm_near_up": lying["arm_near_up"] - 8, "arm_near_low": -20, "head": -14})
    # Its blend is long so the old turn-based screen, which plays it straight from standing, still
    # shows a slump to the floor rather than a snap.
    C.clip("downed", 2.4, [(0, lying), (1.2, breath), (2.4, lying)], loop=True, blend=0.35, drag=False,
           notes="down and waiting to be got up, breathing")
    # The fall into it, for the old screen's Downed pose and anyone who wants the fall alone.
    buckle2 = C.P(C.crouch(30), {"torso": -20, "head": 8, "arm_near_up": -40, "arm_far_up": -20})
    bounce2 = C.P(lying, {"root": lying["root"] + 4, "head": -16, "arm_near_up": -84, "arm_far_up": -112})
    C.clip("collapse", 0.95, [(0, r), (0.3, buckle2), (0.72, lying, "in"), (0.82, bounce2), (0.95, lying)],
           events=[(0.72, "land")], blend=0.06, drag=False, notes="the knees go, the body after them, onto the back")
    # Death: the blow, the knees going, a twist down onto the back, a bounce, still.
    reel = F(torso=-18, look=-20, legs=L(-6, 2), near=A(40, 30), far=A(80, 40))
    sag = F(torso=24, look=20, legs=L(-2, 22, near=2, far=-4), near=A(-10, 10), far=A(10, 10))
    C.clip("die", 1.15, [(0, r, "out"), (4 * FR, reel, "smooth"), (0.34, sag, "smooth"), (0.5, F(base=sag, legs=L(-4, 34))),
                         (0.72, lying, "in"), (0.8, bounce2, "smooth"), (0.95, lying)],
           events=[(0.0, "hit"), (0.72, "land"), (0.72, "fx", "dust")], blend=0.03, drag=False,
           notes="the death: the blow, the knees, the floor; the game fades the body after a held beat")
    C.clip("revive", 0.95, [(0, lying), (0.25, side), (0.55, C.P(C.KNEEL, {"torso": 20, "head": -10})),
                            (0.8, F(torso=-6, look=-12, near=A(40, 60), far=A(50, 60))), (0.95, r)],
           events=[(0.0, "fx", "glow-body"), (0.55, "step")], blend=0.1, drag=False,
           notes="got up from downed, on a little hit points")


def victories():
    r = C.READY
    poses = {
        "one-hand": F(torso=-4, look=-12, near=A(165, 10, item=180), far=A(20, 60)),
        "two-hand": F(torso=-4, look=-6, near=A(20, 125, item=235), grip=-8),
        "polearm": F(torso=-2, look=-6, near=A(40, 40, item=180, hold="long"), far=A(-10, 100)),
        "dagger": F(torso=-2, look=-8, near=A(60, 80, item=205), far=A(-10, 90, item=60)),
        "unarmed": F(torso=-6, look=-14, near=A(150, 60, wrist=-10), far=A(10, 100)),
        "hook": F(torso=-4, look=-10, near=A(20, 90, item=140), far=A(160, 10, item=180)),
        "staff": F(torso=-3, look=-10, near=A(160, 10, item=95, hold="long"), grip=18, hold=("long", "blade")),
        "bow": F(torso=-4, look=-12, near=A(160, 10, item=180, hold="bigbow"), far=A(10, 90)),
        "crossbow": F(torso=-2, look=-6, near=A(20, 120, item=165), far=A(-10, 90)),
    }
    for kind, pose in poses.items():
        r = stance(kind)
        dip = F(base=r, legs=L(0, 4), torso=r["torso"] + 6)
        over = dict(pose)
        for k in ("arm_near_up", "arm_far_up"):
            over[k] = pose.get(k, 0.0) - 6
        breath = dict(pose)
        breath["torso.sy"] = 1.008
        seq("victory-" + kind, [B(8, dip, "out"), B(10, over, "back"), B(10, pose, "smooth"), B(44, breath, "smooth")],
            settle=0, start=r, blend=0.1, recover=False, notes="the fight won, with a %s weapon" % kind)


def generic():
    """The cast, the channel, the potion and the Soul-Link Strike."""
    r = C.READY
    gather = F(torso=-4, look=-2, near=A(20, 110, wrist=-20), far=A(25, 105))
    push = F(torso=10, look=3, legs=L(3, 2, near=3), near=A(80, 8, wrist=-50), far=A(70, 15))
    seq("cast", [B(7, gather, "out", ev=(("fx", "glow-hand"),)), B(5, F(base=gather, torso=-6), "in"),
                 B(3, push, "back", ev=("release",)), B(12, F(base=push, torso=8), "smooth")],
        settle=12, blend=0.06, notes="a spell: the hands gather the charge and push it out, the release at 0.25 s as the "
                                     "old fight screen times it")
    hold = F(torso=8, look=2, legs=L(2, 2, near=3), near=A(82, 6, wrist=-50), far=A(72, 12))
    pulse = F(base=hold, torso=10, arm_near_up=hold["arm_near_up"] - 3, torso__sy=1.01)
    C.clip("channel", 1.0, [(0, hold), (0.5, pulse), (1.0, hold)], loop=True, blend=0.1, drag=False,
           events=[(0.0, "fx", "glow-hand")], notes="holding a spell on its target")
    uncork = F(torso=6, look=6, near=A(30, 110, wrist=-20), far=A(40, 90))
    drink = F(torso=-12, look=-30, near=A(70, 120, wrist=-40), far=A(30, 80))
    seq("drink-potion", [B(10, uncork, "out"), B(10, drink, "smooth"), B(16, F(base=drink, look=-34), "smooth", ev=("release",)),
                         B(8, F(base=r, torso=10, look=4), "smooth")], settle=10, blend=0.08,
        notes="a potion from the belt, drunk; the heal at the release")
    # The Soul-Link Strike: both souls glowing, a crouch, a leap and a two-handed blow down.
    crouch = F(torso=24, look=-10, legs=L(-2, 14), near=A(-20, 40, item=240), grip=-8)
    rise = F(torso=-6, look=-12, legs=C.legs_at(4, -10, {"lean": 30, "bend": 60}, {"lean": 10, "bend": 40}),
             near=A(175, 20, item=250), grip=-8, root__y=-34)
    top = F(base=rise, near=A(180, 25, item=262), grip=-8, root__y=-40, root__x=8)
    slam = F(torso=28, look=8, legs=L(10, 14, near=12), near=A(70, 10, item=40), grip=-8)
    seq("soul-link-strike", [B(12, crouch, "out", ev=(("fx", "glow-body"),)), B(10, crouch, "in"),
                             B(10, rise, "out"), B(8, top, "in", trail="start"),
                             B(5, slam, "back", ev=("hit", "land", ("fx", "burst-ground")), smear=("near", "x"), trail=None),
                             B(12, F(base=slam, torso=30), "smooth", trail="end"),
                             B(14, F(base=slam, legs=L(5, 6, near=6, lift=(4, 0))), "smooth")],
        settle=14, blend=0.06, notes="you and the Soul-Bound strike as one")


# --------------------------------------------------------------------------- #
# Skills: archetypes
# --------------------------------------------------------------------------- #

def heavy(name, kind, n, timing=(12, 3, 3, 10, 20), power=1.0, glint=True, notes=None, fx=None, extra_ev=()):
    """A heavy blow: one of a kind's swings with a longer, deeper wind-up and a glint at the hold."""
    wind, coil, contact, follow, _, smear, back = swings()[kind][n - 1]
    a, h, s, f, r = timing
    deeper = F(base=coil, torso=coil.get("torso", 0) - 3 * power, legs=L(coil.get("hips.x", 0) - 1.5 * power,
                                                                         coil.get("hips.y", 0) - _RY + 1.5 * power))
    hard = F(base=contact, legs=L(contact.get("hips.x", 0) + 2 * power, contact.get("hips.y", 0) - _RY + 2 * power,
                                  near=contact_step(contact)))
    side = None if smear is None else (smear if isinstance(smear, tuple) else ("near", smear))
    ev = [("fx", "glint")] if glint else []
    hit_ev = ["hit"] + list(extra_ev) + ([("fx", fx)] if fx else [])
    beats = [B(a, wind, "linear"), B(h, deeper, "in", ev=ev, trail="start"),
             B(s, hard, "back", ev=hit_ev, smear=side), B(f, follow, "smooth", trail="end")]
    settle = r
    if back:
        beats.append(B(int(r * 0.5), F(base=follow, legs=L(3, 2, near=5, lift=(5, 0))), "smooth"))
        settle = r - int(r * 0.5)
    return seq(name, beats, settle=settle, blend=0.04, notes=notes)


def contact_step(pose):
    """How far a pose's near foot is stepped from the stance (rig px), from its legs (an estimate
    good enough to step it a little further)."""
    return 4.0 if pose.get("leg_near_up", 0) < C.READY["leg_near_up"] - 4 else 0.0


def combo(name, parts, notes=None, settle=18, glint=False, start_fx=None, ultimate=False, travel=None, hop_fx=None):
    """Several blows in a row: parts are (kind, swing number, frames to its contact) and each
    blow's contact is a hit; the first has its anticipation and the last its follow-through.
    `travel` (rig px, one for each blow) puts each blow that far forward of where the figure
    stands, as if it went from foe to foe (`hop_fx` on each move), and it comes back after."""
    sw = swings()
    beats = []
    first = True
    for k, (kind, n, gap) in enumerate(parts):
        wind, coil, contact, follow, frames, smear, _ = sw[kind][n - 1]
        if travel:
            x = travel[k]
            coil, contact, follow = (F(base=p, root__x=x) for p in (coil, contact, follow))
        side = None if smear is None else (smear if isinstance(smear, tuple) else ("near", smear))
        ev = [("fx", start_fx)] if (first and start_fx) else []
        if travel and hop_fx and not first:
            ev.append(("fx", hop_fx))
        if first:
            beats += [B(frames[0] + (6 if ultimate else 0), wind, "linear", ev=ev), B(frames[1] + (2 if ultimate else 0), coil, "in")]
        else:
            beats += [B(max(3, gap - frames[2]), coil, "in", ev=ev)]
        beats += [B(frames[2], contact, "back", ev=("hit",), smear=side), B(4, follow, "smooth")]
        first = False
    beats[-1].f = 9
    return seq(name, beats, settle=settle, blend=0.04, notes=notes,
               unwind=not any((kind, n) in OVER_THE_TOP for kind, n, _ in parts))


def hand_cast(name, style="push", side="near", charge=10, hold=3, release_ev="release", notes=None, aim=0.0,
              ultimate=False, fx="glow-hand", follow_frames=10, settle=14, after=None):
    """A spell from one hand: the hand drawn back gathering (a glow), a moving hold, then out at the
    foe in two or three frames, the release on that frame. style: "push" (the palm shoved out),
    "flick" (thrown from the shoulder like a dart), "point" (a finger jabbed), "lash" (a whip
    cracked over the top), "snap" (a fist snapped shut), "sky" (raised, brought down on the foe),
    "sweep" (swept wide and low), "up" (thrown up at the sky), "shoo" (cocked back at the hip and
    flung up and out, shooing something off), "heart" (from the heart, the forearm opened out and up),
    "backhand" (from across the chest, flung out the back of the hand first), "halt" (drawn in to
    the chest, then raised high and flat at the foe, the body leant back from it), "spear" (drawn
    back by the ear, elbow high, and driven out straight), "scoop" (hunched low, swept up from
    behind the knee and flung up at the foe), "blow" (lifted to the lips, then sent out from
    them), "dart" (cocked at the shoulder, elbow down, and thrown out like a dart), "down"
    (straight up at the sky, then pulled down in front at the foe's feet), "brow" (the hand to the
    brow, the head bowed to it, then opened out low)."""
    other = "far" if side == "near" else "near"
    keep = {other: A(30, 100, wrist=-10)}
    styles = {
        "push": (A(10, 120, wrist=-30), A(84 + aim, 6, wrist=-60), -6, 10),
        "flick": (A(130, 80, wrist=20), A(80 + aim, 10, wrist=-40), -8, 12),
        "point": (A(20, 110, wrist=-10), A(88 + aim, 2, wrist=-20), -2, 8),
        "lash": (A(170, 50, wrist=40), A(70 + aim, 6, wrist=-30), -10, 18),
        "snap": (A(60, 60, wrist=0), A(80 + aim, 30, wrist=-20), -4, 10),
        "sky": (A(165, 15, wrist=-30), A(70 + aim, 10, wrist=-50), -10, 14),
        "sweep": (A(-30, 40, wrist=20), A(60, 20, wrist=-40), 6, 4),
        "up": (A(20, 100, wrist=-10), A(150, 15, wrist=-50), -2, -10),
        "shoo": (A(-25, 80, wrist=35), A(112 + aim, 12, wrist=-35), 4, -4),
        "heart": (A(40, 145, wrist=-40), A(72 + aim, 28, wrist=-80), 4, 6),
        "backhand": (A(55, 150, wrist=40), A(102 + aim, 8, wrist=-10), 6, -8),
        "halt": (A(35, 130, wrist=-20), A(128 + aim, 18, wrist=-70), -2, -8),
        "spear": (A(150, 115, wrist=10), A(86 + aim, 0, wrist=-50), -10, 14),
        "scoop": (A(-60, 15, wrist=30), A(72 + aim, 30, wrist=-40), 22, 2),
        "blow": (A(110, 140, wrist=-60), A(82 + aim, 18, wrist=-40), 2, 8),
        "dart": (A(40, 150, wrist=20), A(90 + aim, 4, wrist=-30), -4, 10),
        "down": (A(175, 8, wrist=-30), A(25 + aim, 10, wrist=-60), -12, 14),
        "brow": (A(135, 160, wrist=-60), A(62 + aim, 20, wrist=-50), 10, -4),
    }
    back_arm, out_arm, t0, t1 = styles[style]
    p = {"near": None, "far": None}
    p[side], p[other] = back_arm, keep[other]
    gather = F(torso=t0, look=-2 - aim * 0.5, legs=L(-2, 1.5), near=p["near"], far=p["far"])
    p[side] = A(back_arm.swing + 6, back_arm.bend + 4, wrist=back_arm.wrist)
    deep = F(torso=t0 - 2, look=-3 - aim * 0.5, legs=L(-2.5, 2), near=p["near"], far=p["far"])
    p[side], p[other] = out_arm, A(-10, 90, wrist=-10)
    out = F(torso=t1, look=2 - aim, legs=L(3, 2, near=4), near=p["near"], far=p["far"])
    p[side] = A(out_arm.swing + 4, max(0, out_arm.bend - 4), wrist=out_arm.wrist - 10)
    over = F(torso=t1 + 2, look=2 - aim, legs=L(3.5, 2.2, near=4), near=p["near"], far=p["far"])
    rel = (release_ev, "hand_" + side) if release_ev == "release" and side == "far" else release_ev
    beats = [B(charge, gather, "out", ev=(("fx", fx),) if fx else ()), B(hold, deep, "in"),
             B(3, out, "back", ev=(rel,), smear=(side, "f")), B(follow_frames, over, "smooth")]
    beats[2].smear = None
    if after:
        beats += after(over)
    return seq(name, beats, settle=settle, blend=0.06, notes=notes)


def two_hand_cast(name, style="push", charge=14, hold=4, release_ev="release", notes=None, ultimate=False,
                  fx="glow-hands", settle=16, extra=None, hits=1, gap=10):
    """A spell from both hands: "push" (gathered at the chest, shoved out), "fan" (thumbs together,
    fingers fanned at the foe), "overhead" (raised over the head, hurled forward), "sky" (both
    raised high to the sky and held), "ground" (raised, then driven down at the floor), "spread"
    (gathered in, then flung wide), "wave" (palms shoved out with a stamp), "clock" (both straight
    up at twelve, then flung to three and six like a clock's hands), "offer" (cupped low at the
    belly, then lifted up and opened out, giving), "tide" (both low behind, then heaved up and
    over at the foe like a wave), "hug" (hugged in to the chest, hunched, then flung up high),
    "jaws" (held out together like shut jaws, then torn open, one up and one down)."""
    look = -2
    if style == "push":
        g = F(torso=-6, look=-2, legs=L(-2, 2), near=A(10, 120, wrist=-30), far=A(15, 115, wrist=-30))
        o = F(torso=12, look=3, legs=L(4, 3, near=5), near=A(82, 6, wrist=-60), far=A(76, 10, wrist=-60))
    elif style == "fan":
        g = F(torso=-4, look=-2, legs=L(-2, 2), near=A(30, 100, wrist=-20), far=A(35, 100, wrist=-20))
        o = F(torso=10, look=4, legs=L(4, 3, near=5), near=A(78, 10, wrist=-80), far=A(84, 8, wrist=-70))
    elif style == "overhead":
        g = F(torso=-14, look=-16, legs=L(-4, 2), near=A(170, 30, wrist=-20), far=A(165, 35, wrist=-20))
        o = F(torso=16, look=4, legs=L(6, 4, near=6), near=A(85, 8, wrist=-60), far=A(80, 10, wrist=-60))
    elif style == "sky":
        g = F(torso=4, look=8, legs=L(0, 4), near=A(20, 100, wrist=-20), far=A(20, 100, wrist=-20))
        o = F(torso=-10, look=-24, legs=L(-1, 0), near=A(165, 10, wrist=-40), far=A(160, 10, wrist=-40))
    elif style == "ground":
        g = F(torso=-10, look=-10, legs=L(-2, 1), near=A(150, 40, wrist=-20), far=A(145, 40, wrist=-20))
        o = F(torso=30, look=14, legs=L(2, 16, near=4), near=A(40, 4, wrist=-70), far=A(35, 4, wrist=-70))
    elif style == "spread":
        g = F(torso=12, look=8, legs=L(0, 6), near=A(10, 130, wrist=-20), far=A(10, 130, wrist=-20))
        o = F(torso=-12, look=-14, legs=L(-1, 0), near=A(120, 10, wrist=-60), far=A(60, 10, wrist=-60))
    elif style == "clock":
        g = F(torso=-6, look=-16, legs=L(-1, 2), near=A(172, 6, wrist=-20), far=A(168, 8, wrist=-20))
        o = F(torso=6, look=0, legs=L(3, 7, near=6, far=-6), near=A(92, 4, wrist=-60), far=A(-2, 4, wrist=-20))
    elif style == "offer":
        g = F(torso=12, look=14, legs=L(-1, 5), near=A(25, 118, wrist=-40), far=A(30, 116, wrist=-40))
        o = F(torso=-6, look=-14, legs=L(1, 1, near=3), near=A(122, 28, wrist=-75), far=A(116, 30, wrist=-75))
    elif style == "tide":
        g = F(torso=14, look=6, legs=L(-3, 8), near=A(-45, 30, wrist=10), far=A(-40, 30, wrist=10))
        o = F(torso=-6, look=-8, legs=L(4, 2, near=6), near=A(110, 10, wrist=-60), far=A(104, 12, wrist=-60))
    elif style == "hug":
        g = F(torso=16, look=10, legs=L(0, 6), near=A(70, 135, wrist=-30), far=A(65, 135, wrist=-30))
        o = F(torso=-10, look=-22, legs=L(-1, 0), near=A(150, 25, wrist=-50), far=A(135, 25, wrist=-50))
    elif style == "jaws":
        g = F(torso=6, look=2, legs=L(-1, 3), near=A(85, 25, wrist=-40), far=A(80, 25, wrist=-40))
        o = F(torso=8, look=4, legs=L(3, 4, near=5), near=A(140, 15, wrist=-40), far=A(25, 15, wrist=-40))
    elif style == "wave":
        g = F(torso=-8, look=-4, legs=L(-3, 2, lift=(6, 0)), near=A(20, 120, wrist=-30), far=A(25, 115, wrist=-30))
        o = F(torso=14, look=4, legs=L(5, 6, near=8), near=A(86, 4, wrist=-70), far=A(80, 6, wrist=-70))
    else:
        raise ValueError(style)
    deep = dict(g)
    deep["torso"] = g["torso"] - 2
    for k in ("arm_near_up", "arm_far_up"):
        deep[k] = g[k] + (3 if style != "ground" else -3)
    over = dict(o)
    over["torso"] = o["torso"] + 2
    beats = [B(charge, g, "out", ev=(("fx", fx),) if fx else ()), B(hold, deep, "in"), B(3, o, "back", ev=(release_ev,)),
             B(10, over, "smooth")]
    for k in range(hits - 1):
        beats += [B(gap // 2, deep, "in"), B(gap - gap // 2, o, "back", ev=(release_ev,))]
    if extra:
        beats += extra(o)
    return seq(name, beats, settle=settle, blend=0.06, notes=notes)


def item_up(name, side="near", item=180, hold=None, shout=True, notes=None, release_at=1, kneel=False, settle=16,
            charge=10, linger=18, fx="burst-self", grip=None, style="high"):
    """A buff called out: what's in the hand raised to the sky (a sword, a lantern, a fist), the
    chest out, the head back: the release when it's at the top. style: "high" (straight up at
    the sky), "level" (from low at the hip up to the sky in front, leant in at the foe), "low"
    (lifted to the chest, then set low before the ally, crouched over it), "face" (lifted before
    the face, the head bowed to it), "knelt" (raised high from a kneel, the body kept crouched
    low, the other hand to the heart)."""
    hold = hold or ("blade", "blade")
    dip = F(torso=10, look=6, legs=L(0, 6 if not kneel else 10), near=A(10, 100, item=120) if side == "near" else None,
            far=A(10, 100) if side == "far" else None, hold=hold)
    arm = A(170, 6, item=item)
    up = F(torso=-8, look=-18, legs=L(-1, 0), near=arm if side == "near" else A(30, 60, item=150),
           far=arm if side == "far" else A(40, 50), hold=hold, grip=grip)
    if style == "level":
        dip = F(torso=4, look=4, legs=L(-2, 5), near=A(-30, 40, item=150), far=A(30, 80), hold=hold)
        up = F(torso=8, look=-10, legs=L(3, 3, near=5), near=A(128, 4, item=item - 50), far=A(-20, 50), hold=hold)
    elif style == "low":
        dip = F(torso=-2, look=0, legs=L(0, 3), near=A(100, 70, item=item), far=A(40, 100), hold=hold)
        up = F(torso=20, look=12, legs=L(1, 12, near=4), near=A(55, 8, item=item), far=A(70, 40), hold=hold)
    elif style == "face":
        dip = F(torso=8, look=4, legs=L(0, 4), near=A(-10, 30, item=item), far=A(30, 100), hold=hold)
        up = F(torso=6, look=18, legs=L(0, 3), near=A(55, 115, item=item), far=A(45, 125), hold=hold)
    elif style == "knelt":
        up = F(torso=-4, look=-16, legs=L(-2, 16, near=8, far=-6), near=arm, far=A(40, 130), hold=hold)
    elif style != "high":
        raise ValueError(style)
    over = dict(up)
    over["arm_%s_up" % side] = up["arm_%s_up" % side] - 5
    over["torso.sy"] = 1.012
    beats = [B(charge, dip, "out"), B(8, up, "back", ev=("release", ("fx", fx)) if fx else ("release",)),
             B(linger, over, "smooth")]
    return seq(name, beats, settle=settle, blend=0.06, notes=notes)


def guard_up(name, style="shield", notes=None, crouch=6, settle=14, linger=14, release="release"):
    """A defensive set: "shield" (the shield brought up and planted in front), "wall" (down behind
    it, braced), "brace" (both fists clenched, the body tensed and set), "cross" (the forearms
    crossed, then thrown open), "cloak" (ducked behind the far arm raised across the face, then
    that arm swept down and round like a cloak thrown about)."""
    if style == "shield":
        set_ = F(torso=4, look=0, legs=L(-1, crouch, near=4), near=A(20, 100, item=140), far=A(80, 20))
        jolt = F(base=set_, torso=6, legs=L(0, crouch + 1, near=4))
    elif style == "wall":
        set_ = F(torso=14, look=-4, legs=L(-2, crouch + 8, near=8, far=-6), near=A(40, 60, item=110), far=A(75, 15))
        jolt = F(base=set_, torso=16, legs=L(-1, crouch + 9, near=8, far=-6))
    elif style == "brace":
        set_ = F(torso=12, look=4, legs=L(-1, crouch + 4, near=4, far=-4), near=A(20, 120, wrist=-20), far=A(25, 120))
        jolt = F(base=set_, torso=14, torso__sy=0.97, legs=L(-1, crouch + 6, near=4, far=-4))
    elif style == "cloak":
        set_ = F(torso=14, look=14, legs=L(-1, crouch + 3), near=A(30, 110, wrist=-20), far=A(140, 120))
        jolt = F(torso=-4, look=-6, legs=L(1, 2, near=3), near=A(20, 100, wrist=-20), far=A(-30, 30))
    elif style == "cross":
        set_ = F(torso=10, look=6, legs=L(0, crouch + 2), near=A(60, 110, wrist=-30), far=A(65, 105))
        jolt = F(torso=-8, look=-10, legs=L(0, 1), near=A(110, 20, wrist=-40), far=A(30, 20))
    else:
        raise ValueError(style)
    beats = [B(8, set_, "out"), B(4, jolt, "back", ev=(release, ("fx", "burst-self"))), B(linger, jolt, "smooth")]
    return seq(name, beats, settle=settle, blend=0.05, notes=notes)


def reach_out(name, style="palm", notes=None, settle=14, charge=10, linger=10, fx="glow-hand"):
    """A heal or a gift reached out to an ally: "palm" (the open hand held out), "both" (both
    hands, low and gentle), "toss" (an underhand toss of something), "chest" (the hand to the heart,
    then out), "circle" (the hand turning a slow circle backwards), "cut" (the near hand drawn
    across the far one before the chest, then the far arm thrust out to the ally), "open" (both
    hands to the heart, then opened out low to either side)."""
    if style == "palm":
        g = F(torso=-2, look=0, near=A(20, 100, wrist=-30), far=A(30, 90))
        o = F(torso=8, look=4, legs=L(2, 2, near=3), near=A(80, 15, wrist=-70), far=A(10, 80))
    elif style == "both":
        g = F(torso=4, look=4, near=A(10, 100, wrist=-30), far=A(15, 100, wrist=-30))
        o = F(torso=14, look=10, legs=L(2, 6, near=4), near=A(60, 30, wrist=-70), far=A(55, 30, wrist=-70))
    elif style == "toss":
        g = F(torso=6, look=4, legs=L(-1, 4), near=A(-40, 30, wrist=10), far=A(40, 70))
        o = F(torso=-4, look=-6, legs=L(2, 2, near=4), near=A(110, 10, wrist=-40), far=A(30, 70))
    elif style == "chest":
        g = F(torso=6, look=8, near=A(40, 140, wrist=-40), far=A(30, 120))
        o = F(torso=-2, look=-4, legs=L(2, 2, near=3), near=A(90, 10, wrist=-70), far=A(20, 80))
    elif style == "cut":
        g = F(torso=6, look=12, legs=L(-1, 2), near=A(40, 110, item=95), far=A(60, 90, wrist=-60))
        o = F(torso=10, look=2, legs=L(3, 3, near=5), near=A(0, 80, item=110), far=A(88, 4, wrist=-70))
    elif style == "open":
        g = F(torso=6, look=8, near=A(40, 140, wrist=-40), far=A(35, 135, wrist=-40))
        o = F(torso=-6, look=-6, legs=L(0, 2), near=A(60, 8, wrist=-70), far=A(-30, 8, wrist=-70))
    elif style == "circle":
        g = F(torso=0, look=0, near=A(120, 30, wrist=-20), far=A(30, 90))
        o = F(torso=6, look=2, near=A(70, 20, wrist=-60), far=A(30, 90))
    else:
        raise ValueError(style)
    beats = [B(charge, g, "out", ev=(("fx", fx),) if fx else ()), B(5, o, "back", ev=("release",)), B(linger, o, "smooth")]
    if style == "circle":
        mid = F(torso=2, look=0, near=A(40, 60, wrist=-40), far=A(30, 90))
        low = F(torso=4, look=2, near=A(60, 10, wrist=-40), far=A(30, 90))
        beats = [B(6, g, "out", ev=(("fx", fx),)), B(6, mid, "linear"), B(6, low, "linear"), B(6, o, "back", ev=("release",)),
                 B(8, o, "smooth")]
    return seq(name, beats, settle=settle, blend=0.06, notes=notes)


def throw(name, style="overhand", notes=None, settle=14, release_arg=None, frames=None):
    """Something thrown: "overhand" (wound back behind the head and hurled), "net" (both hands,
    swung round and flung wide), "lob" (underhand, high), "drop" (dashed down at the feet), "flip"
    (flicked up off the thumb), "spike" (the free hand's, up behind the head and hurled down at the
    floor in front, the blade kept levelled at the foe). `frames` (anticipation, hold, throw, follow-through) stretches the
    style's own."""
    rel = ("release",) if release_arg is None else (("release", release_arg),)
    given = frames
    if style == "overhand":
        w = F(torso=-12, look=-4, legs=L(-4, 2, lift=(5, 0)), near=A(150, 90, wrist=20), far=A(80, 20))
        c = F(base=w, torso=-14, near=A(160, 95, wrist=25))
        o = F(torso=18, look=4, legs=L(6, 4, near=8), near=A(90, 10, wrist=-50), far=A(-10, 40))
        f = F(torso=22, look=6, legs=L(6.5, 4.5, near=8), near=A(40, 10, wrist=-40), far=A(-15, 40))
        frames = (8, 3, 3, 8)
    elif style == "net":
        w = F(torso=-10, look=-4, legs=L(-3, 3), near=A(-40, 40, wrist=0), far=A(-30, 40, wrist=0))
        c = F(base=w, torso=-12, near=A(-50, 40), far=A(-40, 40))
        o = F(torso=10, look=-4, legs=L(5, 3, near=6), near=A(120, 10, wrist=-40), far=A(110, 15, wrist=-40))
        f = F(torso=8, look=-6, legs=L(5, 3, near=6), near=A(100, 10, wrist=-30), far=A(90, 15, wrist=-30))
        frames = (10, 3, 4, 10)
    elif style == "lob":
        w = F(torso=10, look=0, legs=L(-2, 6), near=A(-40, 30, wrist=10), far=A(60, 40))
        c = F(base=w, near=A(-48, 32, wrist=10))
        o = F(torso=-6, look=-10, legs=L(3, 1, near=4), near=A(120, 10, wrist=-40), far=A(30, 50))
        f = F(torso=-8, look=-12, legs=L(3, 1, near=4), near=A(135, 10, wrist=-40), far=A(30, 50))
        frames = (8, 3, 4, 8)
    elif style == "drop":
        w = F(torso=-6, look=-4, legs=L(0, 1), near=A(130, 50, wrist=0), far=A(40, 60))
        c = F(base=w, near=A(140, 50))
        o = F(torso=26, look=20, legs=L(-2, 12), near=A(10, 10, wrist=-40), far=A(60, 60))
        f = F(base=o, torso=28)
        frames = (6, 2, 3, 6)
    elif style == "spike":
        w = F(torso=-10, look=-6, legs=L(-2, 1, lift=(6, 0)), near=A(86, 6, item=90), far=A(165, 70, wrist=20))
        c = F(base=w, torso=-12, far=A(172, 76, wrist=25))
        o = F(torso=20, look=26, legs=L(3, 9, near=7), near=A(92, 6, item=94), far=A(25, 12, wrist=-40))
        f = F(base=o, torso=22, look=28)
        frames = (6, 2, 3, 6)
    elif style == "flip":
        w = F(torso=4, look=6, legs=L(0, 3), near=A(30, 90, wrist=-10), far=A(30, 80))
        c = F(base=w, near=A(26, 94, wrist=-10))
        o = F(torso=-6, look=-24, legs=L(0, 0), near=A(70, 70, wrist=-50), far=A(30, 80))
        f = F(base=o, look=-30)
        frames = (8, 3, 3, 16)
    else:
        raise ValueError(style)
    a, h, s, ff = frames if given is None else given
    return seq(name, [B(a, w, "linear"), B(h, c, "in"), B(s, o, "back", ev=rel), B(ff, f, "smooth")], settle=settle,
               blend=0.05, notes=notes)


def leap_slam(name, kind="two-hand", notes=None, height=34, forward=10, hits=1, glow=None, settle=18, smear="x",
              extra_ev=()):
    """A leap and a blow down out of it onto the foe (or the ground): crouch, spring up with the
    weapon raised high, the blow and the landing on the same frame, a dust burst."""
    hold = ("long", "blade") if kind in ("staff", "polearm") else ("blade", "blade")
    two = kind in ("two-hand", "staff", "polearm")
    grip = (14 if kind in ("staff", "polearm") else -8) if two else None
    far = None if two else A(70, 50)
    crouch = F(torso=26, look=-12, legs=L(-2, 14), near=A(-20, 40, item=-60), far=far, grip=grip, hold=hold)
    rise = F(torso=-8, look=-14, legs=C.legs_at(4, -10, {"lean": 30, "bend": 60}, {"lean": 10, "bend": 40}),
             near=A(172, 20, item=248), far=None if two else A(110, 30), grip=grip, hold=hold, root__y=-height * 0.8)
    top = F(base=rise, near=A(178, 25, item=260), grip=grip, hold=hold, root__y=-height, root__x=forward * 0.6,
            torso=-10)
    slam = F(torso=30, look=10, legs=L(10, 15, near=12), near=A(65, 10, item=30), far=None if two else A(-20, 40), grip=grip,
             hold=hold, root__x=forward)
    rest = F(base=slam, torso=32, legs=L(10, 16, near=12), root__x=forward)
    ev0 = (("fx", glow),) if glow else ()
    beats = [B(10, crouch, "out", ev=ev0), B(4, crouch, "in"), B(9, rise, "out"), B(6, top, "in", trail="start"),
             B(4, slam, "back", ev=("hit", "land", ("fx", "burst-ground")) + tuple(extra_ev), smear=("near", smear)),
             B(10, rest, "smooth", trail="end"),
             B(10, F(base=C.READY, legs=L(4, 4, near=6, lift=(5, 0)), torso=12, root__x=forward * 0.4), "smooth")]
    for k in range(hits - 1):
        beats += [B(6, F(base=top, root__y=-8, root__x=forward), "out"), B(4, slam, "back", ev=("hit",), smear=("near", smear))]
    return seq(name, beats, settle=settle, blend=0.05, notes=notes)


def spin(name, kind="two-hand", turns=1, hits=None, notes=None, settle=18, glow=None, final=None):
    """A spinning blow: the weapon swept round level at the foe, the figure turning right round
    (the root's x scale through nought to -1 and back, which in a side view is a turn), a hit on
    each pass in front; `final` adds a last two-handed blow down."""
    hold = ("long", "blade") if kind in ("staff", "polearm") else ("blade", "blade")
    two = kind in ("two-hand", "staff", "polearm")
    grip = (14 if kind in ("staff", "polearm") else -8) if two else None
    far = None if two else A(90, 10)
    wind = F(torso=-6, look=-4, legs=L(-3, 6, far=-4), near=A(-50, 60, item=-90), far=None if two else A(40, 60),
             grip=grip, hold=hold)
    level = F(torso=4, look=0, legs=L(2, 7, near=4), near=A(95, 4, item=95), far=far, grip=grip, hold=hold)
    beats = [B(10, wind, "out", ev=(("fx", glow),) if glow else ()), B(3, F(base=wind, torso=-8), "in", trail="start")]
    tracks = {"root.sx": [(0.0, 1.0)]}
    t = 13 * FR
    hits = hits or turns
    for k in range(turns):
        beats.append(B(4, level, "linear", ev=("hit",) if k < hits else ()))
        # A turn is 12 frames: the blade in front on the 4th, then away (sx to -1) and back round (to 1).
        t += 4 * FR
        # Thin as it turns side on, a moment turned away (mirrored), and round to the front again.
        tracks["root.sx"] += [(t, 1.0, "in"), (t + 3 * FR, -1.0, "hold"), (t + 5 * FR, -1.0, "out"), (t + 8 * FR, 1.0, "linear")]
        beats.append(B(8, level, "linear"))
        t += 8 * FR
    beats[-1].trail = "end"
    if final:
        top = F(torso=-10, look=-10, legs=L(0, 3), near=A(175, 30, item=250), grip=grip if two else None, far=far, hold=hold)
        down = F(torso=26, look=8, legs=L(8, 12, near=10), near=A(65, 8, item=30), grip=grip if two else None,
                 far=None if two else A(-20, 40), hold=hold)
        beats += [B(6, top, "in", ev=(("fx", "glint"),)), B(3, down, "back", ev=("hit", ("fx", "burst-ground")),
                                                            smear=("near", "x" if hold[0] == "blade" else "y")),
                  B(10, F(base=down, torso=28), "smooth")]
    return seq(name, beats, settle=settle, blend=0.04, notes=notes, tracks=tracks)


def dash_strike(name, kind="dagger", n=1, notes=None, dash=18, settle=18, glow=None, hits=1, low=False):
    """A dash in and a blow: the figure gone forward in a crouch (root moved forward), the blow,
    and back to where it stood. `low` crouches the blow at the knees."""
    wind, coil, contact, follow, frames, smear, _ = swings()[kind][n - 1]
    side = None if smear is None else (smear if isinstance(smear, tuple) else ("near", smear))
    crouch = F(base=coil, torso=coil.get("torso", 0) + 16, legs=L(-2, 10))
    lunge = F(base=contact, root__x=dash, legs=L(6, 8 if not low else 16, near=10))
    if low:
        lunge["torso"] = lunge.get("torso", 0) + 18
    beats = [B(8, crouch, "out", ev=(("fx", glow),) if glow else ()), B(2, crouch, "in", trail="start"),
             B(5, lunge, "back", ev=("hit",), smear=side)]
    for k in range(hits - 1):
        beats += [B(4, F(base=coil, root__x=dash, legs=L(2, 8, near=10)), "in"), B(3, lunge, "back", ev=("hit",), smear=side)]
    beats += [B(8, F(base=follow, root__x=dash, legs=L(6, 6, near=10)), "smooth", trail="end"),
              B(10, F(base=C.READY, root__x=dash * 0.3, legs=L(0, 4, near=4, lift=(6, 0))), "smooth")]
    return seq(name, beats, settle=settle, blend=0.04, notes=notes, unwind=(kind, n) not in OVER_THE_TOP)


def rope_swing(name, notes=None):
    """Swinging across on a rope: a hand up on the rope, the feet off the floor and tucked, carried
    forward in an arc, a slash at the bottom of the swing, dropped back to the floor."""
    grab = F(torso=-4, look=-10, legs=L(-2, 6), near=A(40, 60, item=120), far=A(170, 10))
    swing_back = F(torso=-12, look=-10, legs=C.legs_at(-6, -4, {"lean": 40, "bend": 80}, {"lean": 30, "bend": 90}),
                   near=A(140, 50, item=240), far=A(175, 5), root__y=-18, root__x=-6)
    bottom = F(torso=14, look=4, legs=C.legs_at(6, -4, {"lean": 50, "bend": 30}, {"lean": 30, "bend": 60}),
               near=A(70, 6, item=50), far=A(165, 5), root__y=-12, root__x=20)
    high = F(base=bottom, near=A(40, 4, item=10), root__y=-20, root__x=28, torso=18)
    land = F(torso=20, look=4, legs=L(6, 14, near=10), near=A(30, 40, item=80), far=A(60, 30), root__x=24)
    beats = [B(8, grab, "out"), B(8, swing_back, "smooth"), B(8, bottom, "in", ev=("hit",), smear=("near", "x")),
             B(8, high, "out"), B(8, land, "in", ev=("land", ("fx", "dust"))),
             B(12, F(base=C.READY, root__x=10, legs=L(0, 4, near=4, lift=(5, 0))), "smooth")]
    return seq(name, beats, settle=16, blend=0.05, notes=notes)


def leap_away(name, notes=None, release_at="top"):
    """Up and away (onto the rooftops): a crouch, a spring up and back, a moment in the air, down
    light on the toes and back into the stance."""
    crouch = F(torso=20, look=-8, legs=L(0, 12), near=A(-30, 60, item=60), far=A(-20, 60))
    up = F(torso=-10, look=-6, legs=C.legs_at(-4, -6, {"lean": 50, "bend": 100}, {"lean": 20, "bend": 110}),
           near=A(120, 40, item=150), far=A(130, 30), root__y=-40, root__x=-16)
    land = F(torso=18, look=0, legs=L(-2, 12, far=-6), near=A(20, 80, item=120), far=A(50, 60), root__x=-10)
    beats = [B(8, crouch, "out"), B(3, crouch, "in"), B(10, up, "out", ev=("release", ("fx", "smoke"))), B(8, up, "in"),
             B(8, land, "back", ev=("land",)), B(10, F(base=C.READY, root__x=-4, legs=L(0, 3, far=-3, lift=(0, 4))), "smooth")]
    return seq(name, beats, settle=14, blend=0.05, notes=notes)


def shield_bash(name, notes=None, heavy_=False, settle=16):
    """The shield driven into the foe with a step: the far arm drawn in, then shoved out."""
    w = F(torso=-6, look=-2, legs=L(-4, 3, lift=(4, 0)), near=A(-20, 100, item=140), far=A(20, 120))
    c = F(base=w, torso=-8, legs=L(-5, 3.5, lift=(5, 0)))
    o = F(torso=20, look=4, legs=L(10, 6, near=14), near=A(-30, 90, item=120), far=A(88, 10))
    f = F(torso=22, look=4, legs=L(11, 6.5, near=14), near=A(-32, 90, item=120), far=A(92, 6))
    a, h = (12, 3) if heavy_ else (6, 2)
    beats = [B(a, w, "linear"), B(h, c, "in"), B(3, o, "back", ev=("hit",)), B(9, f, "smooth"),
             B(8, F(base=f, legs=L(4, 3, near=6, lift=(5, 0))), "smooth")]
    return seq(name, beats, settle=settle, blend=0.04, notes=notes)


def haul(name, kind="polearm", notes=None):
    """A hook or a gaff thrown out at the foe and hauled back hand over hand: the hit when it
    catches, the yank after."""
    hold = ("long", "blade") if kind == "polearm" else ("blade", "blade")
    w = F(torso=-8, look=-4, legs=L(-3, 2), near=A(150, 60, item=200), far=A(60, 50), hold=hold)
    o = F(torso=14, look=4, legs=L(6, 4, near=8), near=A(90, 6, item=95), far=A(80, 10), hold=hold)
    yank = F(torso=-16, look=-4, legs=L(-8, 6, far=-8), near=A(10, 110, item=120), far=A(30, 110), hold=hold)
    beats = [B(8, w, "out"), B(2, w, "in"), B(4, o, "back", ev=(("release", "hand_near"),)), B(8, o, "smooth", ev=("hit",)),
             B(6, yank, "back", ev=(("fx", "dust"),)), B(8, yank, "smooth")]
    return seq(name, beats, settle=16, blend=0.05, notes=notes)


def signal(name, notes=None, hits=3, gap=9, item=180):
    """An order given with the weapon raised and dropped (Fire!), then braced while the blows land
    from off the screen, one hit each."""
    up = F(torso=-6, look=-14, legs=L(-1, 2), near=A(170, 10, item=item), far=A(30, 60))
    down = F(torso=14, look=4, legs=L(3, 6, near=5), near=A(80, 10, item=70), far=A(30, 60))
    brace = F(torso=16, look=-4, legs=L(-1, 8, near=4, far=-4), near=A(30, 100, item=120), far=A(70, 100))
    beats = [B(10, up, "out", ev=(("fx", "glint"),)), B(8, up, "linear"), B(4, down, "back"), B(12, brace, "smooth")]
    for k in range(hits):
        beats.append(B(gap if k else 4, F(base=brace, torso=brace["torso"] + (2 if k % 2 else -2)), "smooth",
                       ev=("hit", ("fx", "burst-ground"))))
    return seq(name, beats, settle=18, blend=0.06, notes=notes)


def transform(name, notes=None):
    """Taking a beast's shape: hunched in, the change gathering, then up and out with the arms
    flung wide and clawed, a roar."""
    hunch = F(torso=32, look=14, legs=L(-2, 16), near=A(20, 130, wrist=-30), far=A(20, 130, wrist=-30))
    deep = F(base=hunch, torso=36, legs=L(-2, 18), torso__sy=0.96)
    roar = F(torso=-14, look=-24, legs=L(2, 4, near=6, far=-4), near=A(130, 40, wrist=-60), far=A(110, 40, wrist=-60),
             torso__sx=1.06, torso__sy=1.04)
    settle_ = F(base=roar, torso=6, look=-4, torso__sx=1.04, torso__sy=1.03, legs=L(2, 8, near=6, far=-4))
    beats = [B(14, hunch, "out", ev=(("fx", "glow-body"),)), B(16, deep, "linear"),
             B(6, roar, "back", ev=("release", ("fx", "burst-self"))), B(18, F(base=roar, torso=-16), "smooth"),
             B(14, settle_, "smooth")]
    return seq(name, beats, settle=14, blend=0.06, notes=notes)


def rummage(name, notes=None, drink=False):
    """A hand into the bag at the hip, rummaging, and something brought out (and drunk)."""
    into = F(torso=18, look=20, legs=L(0, 4), near=A(-20, 30, wrist=20), far=A(30, 90))
    dig = F(base=into, near=A(-10, 40, wrist=40), torso=20)
    out = F(torso=-2, look=-8, near=A(90, 80, wrist=-30), far=A(30, 90))
    beats = [B(8, into, "out"), B(5, dig, "smooth"), B(5, into, "smooth"), B(5, dig, "smooth"), B(8, out, "back", ev=("release",)),
             B(12, out, "smooth")]
    if drink:
        beats[-1:] = [B(10, F(torso=-12, look=-30, near=A(70, 120, wrist=-40), far=A(30, 80)), "smooth"), B(10, out, "smooth")]
    return seq(name, beats, settle=12, blend=0.06, notes=notes)


def snatch(name, notes=None, many=1, settle=14):
    """A quick hand into the foe's pocket and back: in, the hit when it closes, out."""
    beats = []
    for k in range(many):
        crouch = F(torso=14, look=4, legs=L(-1, 6), near=A(-10, 60, item=170), far=A(30, 80), root__x=4 * k)
        grab = F(torso=22, look=6, legs=L(8, 8, near=10), near=A(-10, 60, item=170), far=A(80, 10), root__x=4 * k + 6)
        back = F(torso=8, look=0, legs=L(0, 4, near=4), near=A(-10, 60, item=170), far=A(10, 110), root__x=4 * k)
        beats += [B(6 if k == 0 else 3, crouch, "out"), B(3, grab, "back", ev=("hit",)), B(5, back, "smooth")]
    return seq(name, beats, settle=settle, blend=0.04, notes=notes)


def kick(name, notes=None, settle=16):
    """A kick of the near leg (the unarmed auto's third, heavier)."""
    return heavy(name, "unarmed", 3, timing=(10, 3, 2, 10, 18), notes=notes)


def whistle(name, notes=None):
    """Two fingers to the mouth and a whistle, then an arm waved on: help comes."""
    mouth = F(torso=-4, look=-8, near=A(70, 130, wrist=-40), far=A(30, 70))
    blow = F(base=mouth, torso=-8, look=-12, torso__sy=1.01)
    wave = F(torso=6, look=0, legs=L(2, 2, near=3), near=A(120, 30, wrist=-30), far=A(30, 70))
    wave2 = F(base=wave, near=A(80, 20, wrist=-40))
    beats = [B(10, mouth, "out"), B(14, blow, "smooth", ev=("release",)), B(8, wave, "smooth"), B(8, wave2, "smooth"),
             B(8, wave, "smooth")]
    return seq(name, beats, settle=14, blend=0.06, notes=notes)


def point_at(name, notes=None, item=None, hold=None, taunt=False, fx="glow-hand"):
    """A foe singled out: the weapon or a finger levelled at it, held a beat (a taunt adds a
    beckon)."""
    hold = hold or ("blade", "blade")
    near = A(88, 4, item=item) if item is not None else A(88, 2, wrist=-10)
    up = F(torso=-2, look=-2, legs=L(-1, 2), near=A(30, 100, item=item) if item is not None else A(30, 100), far=A(30, 80),
           hold=hold)
    lev = F(torso=8, look=2, legs=L(3, 2, near=4), near=near, far=A(-10, 70), hold=hold)
    beats = [B(8, up, "out"), B(4, lev, "back", ev=("release", ("fx", fx)) if fx else ("release",)), B(12, lev, "smooth")]
    if taunt:
        beck = F(base=lev, far=A(60, 100, wrist=-40), torso=-4, look=-4)
        beats += [B(6, beck, "smooth"), B(6, F(base=lev, far=A(60, 60, wrist=-20), torso=-4), "smooth"), B(6, beck, "smooth")]
    return seq(name, beats, settle=14, blend=0.06, notes=notes)


def ring_bell(name, notes=None, rings=3, item=180):
    """Something held up high and rung (or shaken) side to side: the release on the first ring."""
    up = F(torso=-6, look=-16, legs=L(0, 3), near=A(165, 10, item=item, hold="carried"), far=A(30, 60))
    left = F(base=up, near=A(150, 20, item=item - 20, hold="carried"), torso=-4)
    right = F(base=up, near=A(178, 5, item=item + 20, hold="carried"), torso=-8)
    beats = [B(12, up, "out", ev=(("fx", "glow-hand"),))]
    for k in range(rings):
        beats += [B(6, left, "back", ev=("release",) if k == 0 else (("fx", "ring"),)), B(6, right, "back")]
    return seq(name, beats, settle=16, blend=0.06, notes=notes)


def war_cry(name, side="near", item=180, hold=None, notes=None, grip=None, kneel=False):
    """An ultimate called out: down into a deep crouch (or a kneel) gathering it, a held tremble,
    then a spring up off the floor with what's in the hand thrust at the sky, the other arm flung
    wide and the head thrown back (the release), down onto planted feet with a stamp (land), and
    the pose held while it rings out."""
    hold = hold or ("blade", "blade")
    other = "far" if side == "near" else "near"
    arms = {side: A(-10, 70, item=60), other: A(10, 60)}
    if kneel:
        low = F(base=C.KNEEL, torso=26, look=12, near=arms["near"], far=arms["far"], hold=hold)
    else:
        low = F(torso=28, look=10, legs=L(-2, 18, near=2, far=-4), near=arms["near"], far=arms["far"], hold=hold)
    coil = F(base=low, torso=low["torso"] + 3, torso__sy=0.97)
    arms = {side: A(176, 4, item=item), other: A(100, 12, wrist=-40)}
    if grip is not None:
        arms[other] = None
    burst = F(torso=-14, look=-26, legs=C.legs_at(1, -6, {"lean": 8, "bend": 30}, {"lean": -6, "bend": 30}),
              near=arms["near"], far=arms["far"], hold=hold, grip=grip, root__y=-14, torso__sx=1.03, torso__sy=1.03)
    planted = F(base=burst, legs=L(1, 7, near=8, far=-6), root__y=0.0, torso=-10, look=-22, torso__sx=1.0, torso__sy=1.0)
    ring = F(base=planted, torso=-12, look=-24, torso__sy=1.012)
    for k in ("arm_near_up", "arm_far_up"):
        ring[k] = planted.get(k, 0.0) - 4
    beats = [B(14, low, "out", ev=(("fx", "glow-body"),)), B(10, coil, "linear"),
             B(5, burst, "back", ev=("release", ("fx", "burst-self"))), B(7, planted, "in"),
             B(4, planted, "smooth", ev=("land", ("fx", "burst-ground"))), B(20, ring, "smooth")]
    return seq(name, beats, settle=16, blend=0.06, notes=notes)


def shield_slam(name, notes=None):
    """The shield heaved up overhead and slammed down rim first into the floor in front, the body
    down behind it, braced (the release on the slam), and held."""
    up = F(torso=-10, look=-14, legs=L(-2, 3), near=A(30, 90, item=140), far=A(165, 20))
    coil = F(base=up, torso=-12, far=A(172, 24), torso__sy=1.01)
    down = F(torso=18, look=-4, legs=L(-2, 16, near=10, far=-6), near=A(40, 60, item=110), far=A(70, 10))
    braced = F(base=down, torso=20, legs=L(-2, 17, near=10, far=-6))
    beats = [B(12, up, "out", ev=(("fx", "glow-body"),)), B(6, coil, "in"),
             B(4, down, "back", ev=("release", "land", ("fx", "burst-ground"))), B(30, braced, "smooth")]
    return seq(name, beats, settle=20, blend=0.05, notes=notes)


# --------------------------------------------------------------------------- #
# Skills with a move of their own: each says what it does, where an archetype would have made two
# different skills look alike (clips.py --check's likeness test). Each keeps the archetype's
# frames to its release and its length, which the fight times its effect by.
# --------------------------------------------------------------------------- #

def shield_rap(name, notes=None):
    """Raise Shield: the sword's flat rapped twice on the shield to draw every eye (the release on
    the first rap), then the shield brought up high, the Warden peering over its rim."""
    back = F(torso=-6, look=-6, legs=L(-2, 3), near=A(150, 70, item=215), far=A(55, 70))
    deep = F(base=back, torso=-8, near=A(158, 72, item=228), far=A(55, 70))
    rap = F(torso=8, look=0, legs=L(1, 5, near=2), near=A(70, 70, item=118), far=A(60, 64))
    lift = F(base=rap, torso=4, near=A(118, 72, item=200))
    up = F(torso=-4, look=-8, legs=L(-1, 6, near=4, far=-2), near=A(25, 120, item=165), far=A(118, 42))
    held = F(base=up, torso=-5, look=-9, torso__sy=1.01)
    beats = [B(7, back, "out"), B(2, deep, "in"), B(3, rap, "back", ev=("release", ("fx", "burst-self"))),
             B(4, lift, "out"), B(3, rap, "in", ev=(("fx", "spark"),)), B(8, up, "back"), B(6, held, "smooth")]
    return seq(name, beats, settle=7, blend=0.05, notes=notes)


def plant_line(name, notes=None):
    """Hold the Line: the near foot lifted and stamped down into a wide, low stance (the release
    and the land on the stamp), the shield thrust out square and the cudgel laid over its rim at
    the front row, held there daring them."""
    lift = F(torso=-6, look=-4, legs=L(-3, 2, near=4, lift=(20, 0)), near=A(60, 115, item=175), far=A(40, 80))
    stamp = F(torso=16, look=-4, legs=L(5, 13, near=16, far=-8), near=A(100, 50, item=96), far=A(88, 12))
    sink = F(base=stamp, torso=18, legs=L(5, 14.5, near=16, far=-8))
    dare = F(base=stamp, torso=15, look=-7, legs=L(5, 13.5, near=16, far=-8), near=A(102, 46, item=92))
    beats = [B(8, lift, "out"), B(4, stamp, "back", ev=("release", "land", ("fx", "dust"), ("fx", "burst-self"))),
             B(5, sink, "out"), B(9, dare, "smooth")]
    return seq(name, beats, settle=14, blend=0.05, notes=notes)


def kneel_vigil(name, notes=None):
    """Vigil: down on one knee, the sword planted point down before it and the shield lifted high
    and out over the ally (the release as the knee meets the floor), the head bowed; and up."""
    step = F(legs=C.KNEEL_STEP, torso=12, look=6, near=A(40, 60, item=60), far=A(80, 50))
    down = F(base=C.KNEEL, torso=8, look=16, near=A(72, 40, item=2), far=A(140, 30))
    bowed = F(base=down, torso=10, look=20, far=A(144, 28), torso__sy=1.01)
    rise = F(legs=C.KNEEL_RISE, torso=10, look=6, near=A(40, 60, item=60), far=A(90, 40))
    beats = [B(6, step, "out"), B(6, down, "back", ev=("release", ("fx", "burst-self"))), B(12, bowed, "smooth"),
             B(8, rise, "smooth")]
    return seq(name, beats, settle=8, blend=0.05, notes=notes)


def rally_call(name, notes=None):
    """Rally: the blade swept low behind, then up through a great arc to the sky with a step on and
    the free arm thrown up to lift them (the release at the top), and pumped twice: up, all of you."""
    low = F(torso=16, look=4, legs=L(-2, 6), near=A(-40, 20, item=-75), far=A(20, 60))
    up = F(torso=-10, look=-20, legs=L(4, 2, near=8), near=A(172, 6, item=182), far=A(130, 30, wrist=-50))
    pump = F(base=up, torso=-3, look=-14, near=A(125, 62, item=140), far=A(100, 60, wrist=-50))
    top = F(base=up, torso=-12, look=-22, torso__sy=1.012)
    beats = [B(12, low, "out"), B(8, up, "back", ev=("release", ("fx", "burst-self"))), B(6, pump, "out"),
             B(6, top, "back"), B(12, F(base=top, torso=-11), "smooth")]
    return seq(name, beats, settle=16, blend=0.06, notes=notes)


def salute_call(name, notes=None):
    """Call the Order: heels together and the blade brought up before the face in a knight's salute
    (the release), then swept out and down to the side, the shield low at the side: the Order is
    called, and comes."""
    draw = F(torso=4, look=2, legs=L(0, 3, near=-6, far=3), near=A(10, 60, item=60), far=A(15, 30))
    salute = F(torso=-5, look=-3, legs=L(0, 0.5, near=-6, far=3), near=A(35, 140, item=180), far=A(10, 25))
    sweep = F(torso=2, look=0, legs=L(1, 1.5, near=-6, far=3), near=A(70, 18, item=28), far=A(10, 25))
    held = F(base=sweep, torso=0, look=-2, torso__sy=1.01)
    beats = [B(12, draw, "out"), B(8, salute, "back", ev=("release", ("fx", "glint"))), B(10, sweep, "back"),
             B(14, held, "smooth")]
    return seq(name, beats, settle=16, blend=0.06, notes=notes)


def swig_and_toss(name, notes=None):
    """Rum Ration: the flask to the lips and the head thrown back for a swig, then tossed on to the
    ally underhand (the release), and the mouth wiped on the back of the hand."""
    swig = F(torso=-14, look=-36, legs=L(-2, 2), near=A(15, 40, item=120), far=A(105, 150, wrist=-40))
    deep = F(base=swig, torso=-17, look=-42)
    toss = F(torso=6, look=-4, legs=L(2, 3, near=4), near=A(15, 40, item=120), far=A(105, 10, wrist=-40))
    wipe = F(torso=2, look=-4, legs=L(1, 3, near=3), near=A(15, 40, item=120), far=A(60, 140, wrist=-10))
    beats = [B(6, swig, "out", ev=(("fx", "glow-hand"),)), B(4, deep, "smooth"), B(5, toss, "back", ev=("release",)),
             B(6, F(base=toss, far=A(112, 8, wrist=-50)), "smooth"), B(4, wipe, "smooth")]
    return seq(name, beats, settle=14, blend=0.06, notes=notes)


#: The hivekeeper's smoker held low and out of the way while the free hand works.
SMOKER_LOW = A(5, 45, item=140, hold="held")


def smear_salve(name, notes=None):
    """Honey Salve: the hand dipped down into the pot at the hip, then the arm reached out to the
    ally and drawn down in a stroke, smearing it on (the release as the hand meets them)."""
    dip = F(torso=16, look=22, legs=L(-1, 4), near=SMOKER_LOW, far=A(-25, 50, wrist=30))
    scoop = F(base=dip, torso=18, far=A(-18, 60, wrist=-10))
    reach = F(torso=10, look=4, legs=L(3, 4, near=5), near=SMOKER_LOW, far=A(82, 15, wrist=-75))
    stroke = F(torso=14, look=10, legs=L(3.5, 5, near=5), near=SMOKER_LOW, far=A(55, 25, wrist=-85))
    beats = [B(6, dip, "out", ev=(("fx", "glow-hand"),)), B(4, scoop, "smooth"), B(5, reach, "back", ev=("release",)),
             B(6, stroke, "linear"), B(4, F(base=stroke, far=A(50, 30, wrist=-80)), "smooth")]
    return seq(name, beats, settle=14, blend=0.06, notes=notes)


#: The staff's hand while the free hand makes a gesture: low at the side, the staff kept upright
#: (arm_channels()).
STAFF_KEPT = A(25, 95)


def quicken(name, notes=None):
    """Haste: the free arm winds a big circle before the chest like a clock's key, up, out, down
    and in, and flicks it at the ally (the release), then rolls on round, over and over, while the
    staff ticks on the floor: faster, faster."""
    a = F(torso=-4, look=-8, legs=L(-1, 2), near=STAFF_KEPT, far=A(150, 70, wrist=-20))
    b = F(base=a, torso=-2, look=-4, far=A(112, 12, wrist=-40))
    c = F(base=a, torso=2, look=2, far=A(48, 20, wrist=-10))
    d = F(base=a, torso=2, look=0, far=A(20, 120, wrist=-10))
    flick = F(torso=6, look=2, legs=L(2, 2, near=3), near=STAFF_KEPT, far=A(100, 6, wrist=-70))
    roll_in = F(base=flick, torso=2, look=-4, legs=L(1.5, 3, near=3), near=A(34, 112), far=A(145, 75, wrist=-30))
    roll_out = F(base=flick, torso=8, look=4, near=A(22, 86), far=A(55, 15, wrist=-65))
    beats = [B(3, a, "out", ev=(("fx", "glow-hand"),)), B(2, b, "linear"), B(2, c, "linear"), B(1, d, "linear"),
             B(4, flick, "back", ev=("release",)), B(4, roll_in, "smooth"), B(4, roll_out, "smooth"),
             B(4, roll_in, "smooth"), B(4, roll_out, "smooth")]
    return seq(name, beats, settle=10, blend=0.06, notes=notes)


def examine(name, notes=None):
    """Read the Wound: leant in with the head down, the hand at the chin, studying; a short jab at
    the weak place (the release), and the hand back at the chin; then the head tilted one way and
    the other, reading it."""
    lean = F(torso=24, look=34, legs=L(4, 5, near=5), near=STAFF_KEPT, far=A(62, 150, wrist=-30))
    jab = F(torso=28, look=26, legs=L(5, 6, near=5), near=STAFF_KEPT, far=A(40, 62, wrist=-25))
    tilt = F(base=lean, torso=25, look=38)
    tilt2 = F(base=lean, torso=22, look=24)
    beats = [B(8, lean, "out"), B(4, jab, "back", ev=("release",)), B(8, tilt, "smooth"), B(6, tilt2, "smooth")]
    return seq(name, beats, settle=12, blend=0.06, notes=notes)


def curse(name, notes=None):
    """A hex: hunched, the hand drawn back to the face, thrust out at the foe (the release), then
    wrenched back in to the chest and twisted there, as if tearing something out of it."""
    draw = F(torso=20, look=-8, legs=L(-2, 6), near=STAFF_KEPT, far=A(40, 150, wrist=30))
    thrust = F(torso=24, look=-4, legs=L(3, 7, near=5), near=STAFF_KEPT, far=A(92, 18, wrist=40))
    wrench = F(torso=14, look=-10, legs=L(-2, 8), near=STAFF_KEPT, far=A(30, 140, wrist=-50))
    back = F(base=wrench, torso=18, far=A(40, 128, wrist=35))
    beats = [B(8, draw, "out"), B(4, thrust, "back", ev=("release", ("fx", "glow-hand"))), B(6, wrench, "out"),
             B(6, back, "back")]
    return seq(name, beats, settle=14, blend=0.06, notes=notes)


def shake_hive(name, notes=None, fx="smoke"):
    """Angry Hive: the smoker hoisted overhead and shaken hard, side to side, then thrust down at
    the row as the bees pour out of it (the hit), the arm carried on past and back."""
    hold = ("held", "blade")
    up = F(torso=-8, look=-16, legs=L(-1, 3), near=A(160, 25, item=180), far=A(40, 70), hold=hold)
    left = F(base=up, torso=-6, near=A(148, 35, item=145), hold=hold)
    right = F(base=up, torso=-10, near=A(172, 15, item=215), hold=hold)
    thrust = F(torso=16, look=4, legs=L(5, 5, near=7), near=A(96, 10, item=100), far=A(-10, 50), hold=hold)
    past = F(base=thrust, torso=18, near=A(80, 10, item=78), hold=hold)
    beats = [B(5, up, "out"), B(3, left, "back"), B(3, right, "back"), B(2, left, "in"),
             B(3, thrust, "back", ev=("hit", ("fx", fx))), B(10, past, "smooth")]
    return seq(name, beats, settle=16, blend=0.04, notes=notes)


def high_guard(name, notes=None):
    """Riposte: the weight rocked back and the blade lifted into a high hanging guard, the point
    down at the foe (the release as it sets, a glint), and the point beckoning: miss me."""
    rock = F(torso=-8, look=-2, legs=L(-6, 6, far=-4), near=A(140, 60, item=110), far=A(70, 40))
    guard = F(torso=-6, look=0, legs=L(-5, 7, far=-4), near=A(152, 40, item=62), far=A(75, 30))
    beck = F(base=guard, near=A(150, 44, item=84))
    lower = F(torso=0, look=0, legs=L(-2, 4), near=A(95, 60, item=125), far=A(70, 40))
    beats = [B(8, rock, "out"), B(4, guard, "back", ev=("release", ("fx", "glint"))), B(6, beck, "smooth"),
             B(6, guard, "smooth"), B(7, lower, "smooth")]
    return seq(name, beats, settle=7, blend=0.06, notes=notes)


def order_point(name, notes=None):
    """Command: the blade at the shoulder, chin up, the free hand drawn in and thrown out straight,
    pointing the ally on (the release), and held: go."""
    draw = F(torso=-4, look=-4, legs=L(-1, 2), near=A(20, 125, item=178), far=A(30, 132, wrist=-10))
    point = F(torso=8, look=0, legs=L(3, 3, near=5), near=A(20, 125, item=178), far=A(96, 2, wrist=-12))
    held = F(base=point, torso=4, look=-4)
    beats = [B(8, draw, "out"), B(4, point, "back", ev=("release", ("fx", "glow-hand"))), B(12, held, "smooth")]
    return seq(name, beats, settle=14, blend=0.06, notes=notes)


def lantern_out(name, notes=None):
    """Reveal: the lantern swung up and thrust high out into the dark (the release), the free hand
    shading the eyes, peering, and the light swept across the foes."""
    hold = ("carried", "blade")
    low = F(torso=8, look=4, legs=L(-2, 4), near=A(-10, 60, item=180), far=A(30, 80), hold=hold)
    out = F(torso=10, look=-6, legs=L(4, 4, near=6), near=A(148, 6, item=180), far=A(55, 145, wrist=-60), hold=hold)
    left = F(base=out, near=A(136, 12, item=180), torso=12, hold=hold)
    right = F(base=out, near=A(158, 4, item=180), torso=9, hold=hold)
    beats = [B(8, low, "out"), B(4, out, "back", ev=("release", ("fx", "glow-hand"))), B(6, left, "smooth"),
             B(6, right, "smooth")]
    return seq(name, beats, settle=14, blend=0.06, notes=notes)


def fencer_bound(name, notes=None):
    """A fencer's bound back out of reach: down, then off both feet back and low with the blade
    kept levelled at the foe and the free arm up behind for balance (the release as it leaves the
    floor), down on the back foot, and the guard taken again."""
    crouch = F(torso=6, look=2, legs=L(-2, 9), near=A(84, 8, item=90), far=A(200, 50))
    bound = F(torso=-6, look=0, legs=C.legs_at(-2, -4, {"lean": 30, "bend": 50}, {"lean": -10, "bend": 60}),
              near=A(88, 4, item=92), far=A(215, 40), root__y=-16, root__x=-24)
    land = F(torso=4, look=2, legs=L(-4, 11, far=-8), near=A(86, 8, item=90), far=A(205, 50), root__x=-22)
    beats = [B(8, crouch, "out"), B(3, crouch, "in"), B(10, bound, "out", ev=("release", ("fx", "smoke"))),
             B(8, F(base=bound, root__y=-10, root__x=-26), "in"), B(8, land, "back", ev=("land",)),
             B(10, F(base=C.READY, root__x=-8, legs=L(-1, 4, far=-4)), "smooth")]
    return seq(name, beats, settle=14, blend=0.05, notes=notes)


def edge_wipe(name, notes=None):
    """Poisoned Edge: the dagger held up level before the chest and the off hand drawn along its
    edge from the hilt to the point, wiping the venom on (the release at the point), and the blade
    turned to the light."""
    level = A(55, 85, item=95)
    hilt = F(torso=6, look=14, legs=L(-1, 3), near=level, far_to=at_near_wrist(2, -2))
    point = F(torso=7, look=12, legs=L(-1, 3), near=level, far_to=at_near_wrist(20, -4))
    turn = F(base=point, near=A(62, 80, item=125), far=A(30, 90), look=4)
    beats = [B(10, hilt, "out", ev=(("fx", "glow-weapon"),)), B(5, point, "back", ev=("release",)),
             B(10, turn, "smooth")]
    return seq(name, beats, settle=14, blend=0.06, notes=notes)


def sigil(name, notes=None):
    """Ward: the free hand traces a sigil in the air, low across the body, out and up, and sets it
    up over the ally, the arm high and the head back (the release)."""
    up = F(torso=8, look=6, legs=L(-1, 3), near=STAFF_KEPT, far=A(50, 110, wrist=-20))
    across = F(base=up, torso=2, look=-2, far=A(96, 30, wrist=-40))
    set_ = F(torso=-8, look=-20, legs=L(0, 1, near=3), near=STAFF_KEPT, far=A(158, 14, wrist=-75))
    beats = [B(5, up, "out", ev=(("fx", "glow-hand"),)), B(5, across, "linear"), B(5, set_, "back", ev=("release",)),
             B(10, F(base=set_, torso=5), "smooth")]
    return seq(name, beats, settle=14, blend=0.06, notes=notes)


def press_burn(name, notes=None):
    """Cauterise: the hand drawn back glowing, pressed down on the wound with the weight behind
    it, crouched over it (the release), pressed harder, and snatched up off it, hot, to the
    shoulder."""
    draw = F(torso=-4, look=0, legs=L(-1, 2), near=STAFF_KEPT, far=A(25, 120, wrist=-40))
    press = F(torso=26, look=16, legs=L(4, 10, near=6), near=STAFF_KEPT, far=A(50, 8, wrist=-85))
    harder = F(base=press, torso=30, legs=L(5, 12, near=6), far=A(46, 4, wrist=-88))
    off = F(torso=-4, look=-2, legs=L(0, 3, near=3), near=STAFF_KEPT, far=A(120, 140, wrist=-20))
    beats = [B(12, draw, "out", ev=(("fx", "glow-hand"),)), B(5, press, "back", ev=("release",)),
             B(8, harder, "smooth"), B(6, off, "out")]
    return seq(name, beats, settle=14, blend=0.06, notes=notes)


def rub_hands(name, notes=None):
    """Warm Hands: the hands rubbed together before the chest, hunched over them, then the arms
    flung wide, one forward and one back, the chest open and the warmth given off them (the
    release)."""
    together = F(torso=12, look=14, legs=L(0, 5), near=A(52, 108, wrist=-30), far=A(48, 112, wrist=-30))
    rub = F(base=together, near=A(48, 114, wrist=-20), far=A(56, 102, wrist=-40))
    spread = F(torso=-8, look=-8, legs=L(1, 3, near=3), near=A(112, 8, wrist=-75), far=A(-55, 10, wrist=-75))
    beats = [B(5, together, "out", ev=(("fx", "glow-hands"),)), B(5, rub, "smooth"), B(5, spread, "back", ev=("release",)),
             B(10, F(base=spread, torso=-5), "smooth")]
    return seq(name, beats, settle=14, blend=0.06, notes=notes)


def seize(name, notes=None):
    """Arrest: a step in with the cudgel raised, the foe's wrist seized high (the hit), and twisted
    down and round behind it, the cudgel held over it."""
    step = F(torso=8, look=0, legs=L(3, 4, near=8), near=A(150, 50, item=205), far=A(60, 40))
    grab = F(torso=10, look=-2, legs=L(6, 5, near=10), near=A(155, 45, item=210), far=A(96, 6, wrist=-20))
    twist = F(torso=20, look=12, legs=L(5, 9, near=10), near=A(140, 60, item=190), far=A(35, 50, wrist=60))
    beats = [B(6, step, "out"), B(3, grab, "back", ev=("hit",)), B(5, twist, "out")]
    return seq(name, beats, settle=14, blend=0.04, notes=notes)


def mocking_bow(name, notes=None):
    """Vicious Mockery: the blade swept out wide and down into a deep, mocking bow, the free hand
    on the heart (the release), held, then up with the head cocked one way and the other."""
    sweep = F(torso=-4, look=-6, legs=L(-1, 2, near=4), near=A(120, 20, item=150), far=A(30, 90))
    bow = F(torso=34, look=26, legs=L(-3, 6, near=10, far=-6), near=A(70, 10, item=40), far=A(30, 150, wrist=-40))
    up = F(torso=-6, look=-12, legs=L(-1, 3, near=6), near=A(40, 60, item=120), far=A(-15, 40))
    cock = F(base=up, look=-4, torso=-4)
    beats = [B(8, sweep, "out"), B(4, bow, "back", ev=("release",)), B(12, F(base=bow, torso=36), "smooth"),
             B(6, up, "out"), B(6, cock, "smooth"), B(6, up, "smooth")]
    return seq(name, beats, settle=14, blend=0.06, notes=notes)


def pommel(name, notes=None):
    """Belaying Pin: the sword turned in the hand and hoisted, the blade back over the shoulder,
    and the pommel clubbed down on the foe's head (the hit), the weight dropping into it."""
    hoist = F(torso=-8, look=-8, legs=L(-3, 2), near=A(150, 90, item=250), far=A(60, 50))
    coil = F(base=hoist, torso=-10, near=A(158, 92, item=258))
    clout = F(torso=22, look=6, legs=L(6, 6, near=6), near=A(78, 40, item=170), far=A(-10, 40))
    past = F(base=clout, torso=25, legs=L(6.5, 7, near=6), near=A(66, 46, item=160))
    beats = [B(7, hoist, "linear"), B(2, coil, "in", ev=(("fx", "glint"),)), B(3, clout, "back", ev=("hit",)),
             B(8, past, "smooth")]
    return seq(name, beats, settle=14, blend=0.04, notes=notes)


def second_wind(name, notes=None):
    """Second Wind: bent right over, leant on the sword with its point on the floor and the other
    hand on the knee, then up with a great breath in, the chest swelling and the head back (the
    release), and let out, the guard taken again."""
    # The hand comes down close in front first, so the blade drops point first rather than
    # sweeping out level like a cut.
    drop = F(torso=30, look=16, legs=L(-2, 9), near=A(20, 70, item=70), far=A(20, 20))
    bent = F(torso=48, look=24, legs=L(-2, 14), near=A(58, 32, item=10), far=A(14, 12))
    heave = F(base=bent, torso=50, look=26, torso__sy=0.97)
    breath = F(torso=-10, look=-14, legs=L(-1, 2), near=A(30, 70, item=120), far=A(40, 60), torso__sy=1.035,
               torso__sx=1.02)
    out = F(base=C.READY, torso=10, look=0, torso__sy=0.99)
    beats = [B(3, drop, "out"), B(3, bent, "smooth"), B(2, heave, "smooth"),
             B(4, breath, "back", ev=("release", ("fx", "burst-self"))), B(8, breath, "smooth"),
             B(6, out, "smooth")]
    return seq(name, beats, settle=14, blend=0.05, notes=notes)


def flourish(name, notes=None):
    """Inspiration: the blade drawn up before the face, whirled over forward and pointed at the
    ally (the release), and a nod to them."""
    up = F(torso=-2, look=-2, legs=L(-1, 2), near=A(30, 130, item=172), far=A(-10, 30))
    whirl = F(torso=2, look=0, legs=L(0, 2), near=A(66, 72, item=50), far=A(-10, 30))
    point = F(torso=6, look=2, legs=L(2, 3, near=4), near=A(92, 8, item=96), far=A(-15, 30))
    nod = F(base=point, torso=8, look=12)
    beats = [B(8, up, "out"), B(4, whirl, "linear"), B(4, point, "back", ev=("release",)), B(10, nod, "smooth")]
    return seq(name, beats, settle=12, blend=0.06, notes=notes)


def hymn(name, notes=None):
    """Battle Hymn: the head bowed and the free hand on the heart, a breath drawn in, then the chest
    up and the head back, the free arm opened wide and the blade lifted (the release on the first
    note), beating the hymn's time with it like a baton, swaying, while it rings out."""
    bow = F(torso=12, look=16, legs=L(-1, 5), near=A(30, 20, item=50), far=A(40, 145, wrist=-40))
    breath = F(base=bow, torso=8, look=10, torso__sy=1.02)
    sing = F(torso=-12, look=-26, legs=L(0, -1.5), near=A(152, 18, item=152), far=A(100, 10, wrist=-70))
    sway = F(base=sing, torso=-8, look=-22, near=A(136, 30, item=128))
    sway2 = F(base=sing, torso=-14, look=-27, near=A(164, 12, item=176), torso__sy=1.012)
    beats = [B(14, bow, "out", ev=(("fx", "glow-body"),)), B(10, breath, "linear"),
             B(5, sing, "back", ev=("release", ("fx", "burst-self"))), B(7, sway, "smooth"), B(10, sway2, "smooth"),
             B(10, sway, "smooth")]
    return seq(name, beats, settle=20, blend=0.06, notes=notes)


# --------------------------------------------------------------------------- #
# Every class's skills (skill-ids.md; the weapon autos play auto-<kind>-n)
# --------------------------------------------------------------------------- #

def skills():
    S = "skill-"
    # Swordhand
    kit("one-hand")
    spin(S + "fighter-cleave", "one-hand", turns=1, notes="Cleave: one wide sweep round through the foe and its neighbours")
    second_wind(S + "fighter-second-wind", notes="Second Wind: bent over, leant on the sword, a great breath in, back on the feet")
    high_guard(S + "fighter-riposte", notes="Riposte: rocked back into a high hanging guard, the point beckoning the miss")
    war_cry(S + "fighter-action-surge", item=185, notes="Action Surge: down, gathering, and up with the blade at the sky, again")
    # Shadowstep
    kit("dagger")
    dash_strike(S + "rogue-backstab", "dagger", 3, notes="Backstab: in low behind the foe, the dagger driven down")
    throw(S + "rogue-smoke-step", "drop", notes="Smoke Step: a pellet dashed at the feet, gone in the smoke")
    edge_wipe(S + "rogue-poisoned-edge", notes="Poisoned Edge: the off hand drawn along the blade's edge, wiping on the venom")
    combo(S + "rogue-shadow-dance", [("dagger", 1, 8), ("dagger", 2, 12), ("dagger", 3, 14)], start_fx="smoke", ultimate=True,
          travel=(14, 26, 8), hop_fx="smoke",
          notes="Shadow Dance: three backstabs out of the smoke, each on another foe")
    # Lightbearer
    kit("one-hand")
    hand_cast(S + "cleric-sacred-flame", "sky", side="far", notes="Sacred Flame: the hand raised and brought down, fire from above")
    reach_out(S + "cleric-cure-wounds", "palm", notes="Cure Wounds: the hand laid out toward the ally")
    item_up(S + "cleric-bless", item=180, notes="Bless: the mace raised high over the allies")
    two_hand_cast(S + "cleric-guiding-bolt", "push", notes="Guiding Bolt: light gathered at the chest and driven out")
    two_hand_cast(S + "cleric-dawnlight", "sky", charge=30, hold=8, settle=24, release_ev="hit",
                  notes="Dawnlight: both arms raised to the sky, the dawn breaks over everyone")
    # Arcanist
    kit("staff")
    hand_cast(S + "wizard-fire-bolt", "flick", side="far", notes="Fire Bolt: a mote of fire flicked at the foe")
    hand_cast(S + "wizard-magic-missile", "dart", side="far", charge=8, hold=2,
              after=lambda o: [B(4, F(base=o, arm_far_up=o["arm_far_up"] + 30, arm_far_low=o["arm_far_low"] - 30), "in"),
                               B(3, o, "back", ev=(("release", "hand_far"),)),
                               B(4, F(base=o, arm_far_up=o["arm_far_up"] + 30, arm_far_low=o["arm_far_low"] - 30), "in"),
                               B(3, o, "back", ev=(("release", "hand_far"),))],
              notes="Magic Missile: the hand cocked at the shoulder and three darts thrown, one after another")
    two_hand_cast(S + "wizard-burning-hands", "fan", charge=10, notes="Burning Hands: thumbs together, fingers fanned, fire")
    guard_up(S + "wizard-shield", "cross", crouch=2, linger=8, settle=10, notes="Shield: a ward thrown up against the blow")
    two_hand_cast(S + "wizard-fireball", "overhead", charge=34, hold=8, settle=24,
                  notes="Fireball: fire gathered over the head and hurled")
    # Warden
    kit("one-hand")
    heavy(S + "paladin-divine-smite", "one-hand", 1, timing=(8, 3, 3, 10, 16), fx="glow-weapon",
          notes="Divine Smite: the blade bright with radiance, brought down")
    reach_out(S + "paladin-lay-on-hands", "both", charge=12, linger=14, notes="Lay on Hands: both hands on the ally")
    shield_rap(S + "paladin-raise-shield", notes="Raise Shield: the sword rapped on the shield, every foe's eye on the Warden")
    item_up(S + "paladin-unbroken-oath", item=180, kneel=True, charge=26, linger=28, settle=20, style="knelt",
            notes="Unbroken Oath: from a kneel the blade raised high, still crouched low, the shield to the heart, the oath held over everyone")
    # Wildcaller
    kit("staff")
    hand_cast(S + "druid-thorn-whip", "lash", notes="Thorn Whip: the vine cracked out and the foe hauled in")
    two_hand_cast(S + "druid-entangle", "ground", notes="Entangle: the hands driven down, the roots rise")
    hand_cast(S + "druid-healing-word", "brow", side="far", charge=6, hold=2, follow_frames=6, settle=10, fx=None,
              notes="Healing Word: the hand to the brow, a word, and the hand opened out to the ally")
    hand_cast(S + "druid-moonbeam", "down", side="far", charge=12, hold=4,
              notes="Moonbeam: the arm straight up at the sky, then the light pulled down on the foe")
    transform(S + "druid-wild-shape-bear", notes="Wild Shape: Bear: hunched, gathering, and up as the bear")
    # Hexblade
    kit("dagger")
    hand_cast(S + "warlock-eldritch-blast", "spear", side="far",
              notes="Eldritch Blast: the hand drawn back by the ear and driven out, the beam along the arm")
    point_at(S + "warlock-hex", notes="Hex: the finger levelled and the curse laid", fx="glow-hand")
    hand_cast(S + "warlock-hellish-rebuke", "backhand", side="far", charge=4, hold=1, follow_frames=6, settle=10,
              notes="Hellish Rebuke: a backhand flung at the striker, and it burns")
    guard_up(S + "warlock-armour-of-shadows", "cloak",
             notes="Armour of Shadows: ducked behind the arm, then the shadows swept round like a cloak")
    two_hand_cast(S + "warlock-void-maw", "jaws", charge=30, hold=10, settle=24,
                  notes="Void Maw: the hands held out together like shut jaws, then torn open over every foe")
    # Skald
    kit("one-hand")
    mocking_bow(S + "bard-vicious-mockery", notes="Vicious Mockery: a deep, mocking bow, and the head cocked")
    flourish(S + "bard-inspiration", notes="Inspiration: a flourish of the blade, pointed at the ally, and a nod")
    hand_cast(S + "bard-healing-word", "heart", side="far", charge=6, hold=2, follow_frames=6, settle=10, fx=None,
              notes="Healing Word: a sung word, the hand from the heart opened out to the ally")
    two_hand_cast(S + "bard-thunderwave", "wave", charge=10, notes="Thunderwave: a stamp and both palms shoved out")
    hymn(S + "bard-battle-hymn", notes="Battle Hymn: a hand on the heart, then the head back and the hymn sung out, swaying")
    # Watchblade
    kit("one-hand")
    plant_line(S + "watchblade-hold-the-line", notes="Hold the Line: stamped down wide behind the shield, the front row dared")
    seize(S + "watchblade-arrest", notes="Arrest: in with the cudgel raised, the wrist seized and twisted down")
    whistle(S + "watchblade-whistle-for-the-watch", notes="Whistle for the Watch: a whistle and a wave; a Watchman comes")
    shield_slam(S + "watchblade-shield-wall", notes="Shield Wall: the shield heaved up and slammed down, everyone behind it")
    # Netcaster
    kit("polearm")
    heavy(S + "netcaster-gaff", "polearm", 1, timing=(5, 2, 2, 9, 16), glint=False, notes="Gaff: the hook jabbed out at the foe")
    throw(S + "netcaster-cast-the-net", "net", notes="Cast the Net: swung round and flung wide over the row")
    reach_out(S + "netcaster-mend", "palm", notes="Mend: a hand to the ally's hurt")
    haul(S + "netcaster-haul-in", notes="Haul In: the gaff out, caught, hauled in")
    two_hand_cast(S + "netcaster-high-tide", "spread", charge=24, hold=8, release_ev="hit", settle=22,
                  notes="High Tide: the sea raised with both arms and brought down on every foe")
    # Deckhand
    kit("one-hand")
    pommel(S + "deckhand-belaying-pin", notes="Belaying Pin: the pommel clubbed down on the foe's head")
    swig_and_toss(S + "deckhand-rum-ration", notes="Rum Ration: a swig from the flask, and the flask tossed to the ally")
    rope_swing(S + "deckhand-swing-across", notes="Swing Across: over on a rope, a cut on the way")
    signal(S + "deckhand-broadside", notes="Broadside: the order given, the ship's guns land from off the screen")
    # Lodge Ranger
    kit("bow")
    point_at(S + "lodge-ranger-hunters-mark", notes="Hunter's Mark: the quarry pointed out and marked")
    hand_cast(S + "lodge-ranger-hawk-strike", "up", side="far", fx=None, notes="Hawk Strike: the hawk thrown up off the arm")
    p = bow_poses("bow")
    seq(S + "lodge-ranger-volley", [B(5, p["nock"]), B(4, p["set"], "out"), B(5, p["drawn"], "linear"), B(2, p["anchor"], "in"),
                                    B(2, p["loosed"], "back", ev=("release",)), B(4, p["set"], "out"),
                                    B(4, p["drawn"], "in"), B(2, p["loosed"], "back", ev=("release",)), B(4, p["set"], "out"),
                                    B(4, p["drawn"], "in"), B(2, p["loosed"], "back", ev=("release",)),
                                    B(6, p["follow"], "smooth")], settle=12, blend=0.05,
        notes="Volley: three arrows loosed as fast as they're drawn")
    p = bow_poses("bow", 45.0)
    seq(S + "lodge-ranger-rain-of-arrows", [B(8, p["nock"]), B(10, p["set"], "out", ev=(("fx", "glint"),)),
                                            B(14, p["drawn"], "linear"), B(8, p["anchor"], "in"),
                                            B(2, p["loosed"], "back", ev=("release",)), B(16, p["follow"], "smooth")],
        settle=18, blend=0.06, notes="Rain of Arrows: drawn long and loosed high, to fall on every foe")
    # Guild Battlemage
    kit("staff")
    hand_cast(S + "battlemage-force-bolt", "push", side="far", charge=8, notes="Force Bolt: a shove of force from the palm")
    sigil(S + "battlemage-ward", notes="Ward: a sigil traced in the air and set over the ally")
    hand_cast(S + "battlemage-counterspell", "snap", side="far", charge=4, hold=1, follow_frames=6, settle=10, fx=None,
              notes="Counterspell: the fist shut on the foe's spell")
    heavy(S + "battlemage-chain-lightning", "staff", 3, timing=(12, 3, 2, 10, 16), fx="glow-weapon",
          notes="Chain Lightning: the staff driven at the foe, the lightning jumping on")
    leap_slam(S + "battlemage-spire-lance", "staff", height=24, forward=6, glow="glow-weapon",
              notes="Spire Lance: up with the staff and down, the lance of force along the line")
    # Ashwalker
    kit("two-hand")
    hand_cast(S + "ashwalker-ash-cloud", "sweep", side="far", fx="smoke", notes="Ash Cloud: ash flung low across the front row")
    guard_up(S + "ashwalker-endure", "brace", notes="Endure: set and braced against what's coming")
    heavy(S + "ashwalker-burn-the-fields", "two-hand", 2, timing=(14, 3, 3, 10, 18), fx="burst-ground",
          notes="Burn the Fields: the blade dragged up out of the ground in a line of fire")
    spin(S + "ashwalker-last-harvest", "two-hand", turns=2, final=True, glow="glow-weapon",
         notes="Last Harvest: the blade reaped round twice and brought down")
    # Ironvein Bulwark
    kit("one-hand")
    shield_bash(S + "bulwark-shield-bash", notes="Shield Bash: the shield driven into the foe")
    seq(S + "bulwark-cover", [B(4, F(torso=12, look=0, legs=L(8, 6, near=12), near=A(10, 100, item=140), far=A(85, 15), root__x=10),
                                 "out", ev=("release",)), B(10, F(torso=10, legs=L(8, 6, near=12), near=A(10, 100, item=140),
                                                                    far=A(85, 15), root__x=10), "smooth")],
        settle=14, blend=0.03, notes="Cover: a step across with the shield up to take the blow")
    guard_up(S + "bulwark-hold", "wall", notes="Hold: the shield set, the back row behind it")
    guard_up(S + "bulwark-stone-skin", "brace", crouch=8, notes="Stone Skin: braced, the skin gone to stone")
    war_cry(S + "bulwark-nobody-left-behind", side="far", item=180, hold=("blade", "shield"), kneel=True,
            notes="Nobody Left Behind: from a kneel by the fallen, up with the shield raised over them, every one of them up")
    # Ember Mender
    kit("staff")
    hand_cast(S + "ember-mender-cinder", "blow", side="far", charge=8,
              notes="Cinder: the hand lifted to the lips, and the ember sent off it at the foe")
    press_burn(S + "ember-mender-cauterise", notes="Cauterise: the hot hand pressed down on the wound, crouched over it, and snatched up off it")
    examine(S + "ember-mender-read-the-wound", notes="Read the Wound: leant in, studying, and the weak place jabbed at")
    rub_hands(S + "ember-mender-warm-hands", notes="Warm Hands: the hands rubbed together and spread warm over the row")
    two_hand_cast(S + "ember-mender-rekindle", "offer", charge=28, hold=8, settle=22,
                  notes="Rekindle: cupped hands lifted from the belly and opened, the fire given back to everyone")
    # Lanerunner
    kit("dagger")
    combo(S + "lanerunner-twin-cuts", [("dagger", 1, 6), ("dagger", 2, 9)], settle=14,
          notes="Twin Cuts: a stab and the off-hand's slash")
    snatch(S + "lanerunner-pick-pocket", notes="Pick Pocket: in and out of the foe's pocket")
    leap_away(S + "lanerunner-rooftops", notes="Rooftops: up and away out of reach, and down light")
    dash_strike(S + "lanerunner-low-blow", "dagger", 1, low=True, notes="Low Blow: a stab under the guard")
    seq_pocket(S + "lanerunner-everyones-pocket")
    # Wayfarer
    kit("staff")
    heavy(S + "wayfarer-walking-stick", "staff", 1, timing=(6, 2, 3, 8, 14), glint=False,
          notes="Walking Stick: a whack over the top with the stick")
    throw(S + "wayfarer-throw-anything", "overhand", notes="Throw Anything: whatever comes out of the bag, hurled")
    seq(S + "wayfarer-duchess-kicks", [B(8, F(torso=-4, look=-8, near=A(70, 130, wrist=-40), far=A(30, 70)), "out"),
                                       B(10, F(torso=-6, look=-10, near=A(70, 130, wrist=-40), far=A(30, 70)), "smooth"),
                                       B(6, F(torso=8, look=2, near=A(88, 4, wrist=-10), far=A(-10, 60)), "back"),
                                       B(10, F(torso=8, look=2, near=A(88, 4, wrist=-10), far=A(-10, 60)), "smooth", ev=("hit",)),
                                       B(10, F(torso=-6, look=-6, near=A(40, 120, wrist=-20), far=A(20, 90)), "smooth")],
        settle=14, blend=0.06, notes="Duchess Kicks: a whistle and a point; Duchess, off the screen, does the rest")
    rummage(S + "wayfarer-rummage", notes="Rummage: a hand in the bag and a potion out")
    throw(S + "wayfarer-lucky-stone", "flip", settle=22, frames=(14, 6, 3, 26), notes="Lucky Stone: the stone flipped high for luck")
    # Gilded Knight
    kit("one-hand")
    item_up(S + "gilded-knight-sunburst", item=190, charge=10, linger=10, settle=14, style="level",
            notes="Sunburst: the blade swept up from the hip to the sky before the foe, and the sun breaks off it")
    kneel_vigil(S + "gilded-knight-vigil", notes="Vigil: on one knee, the sword planted, the shield held out over the ally")
    salute_call(S + "gilded-knight-call-the-order", notes="Call the Order: a knight's salute, and the blade swept out: the Order comes")
    leap_slam(S + "gilded-knight-gilded-dawn", "one-hand", height=30, glow="glow-weapon",
              notes="Gilded Dawn: up into the light and down, the dawn over every foe")
    # Hemomancer
    kit("dagger")
    hand_cast(S + "hemomancer-blood-lash", "lash", side="far", notes="Blood Lash: a lash of blood cracked out")
    hand_cast(S + "hemomancer-drain", "push", side="far", charge=10, follow_frames=4,
              after=lambda o: [B(10, F(base=o, torso=-6, arm_far_low=o["arm_far_low"] - 50, arm_far_up=o["arm_far_up"] + 25), "out"),
                               B(10, F(base=o, torso=-8, arm_far_low=o["arm_far_low"] - 60, arm_far_up=o["arm_far_up"] + 30), "smooth")],
              notes="Drain: the hand out, clutching, and the life pulled back into it")
    reach_out(S + "hemomancer-blood-shield", "cut",
              notes="Blood Shield: the dagger drawn across the other hand, and that arm thrust out to the ally")
    reach_out(S + "hemomancer-given-freely", "chest", charge=14, linger=14, notes="Given Freely: from the heart to the ally")
    two_hand_cast(S + "hemomancer-crimson-tide", "tide", charge=30, hold=8, release_ev="hit", settle=24,
                  notes="Crimson Tide: both hands low behind, then heaved up and over, a tide of blood over every foe")
    # Hivekeeper
    kit("one-hand")
    hand_cast(S + "hivekeeper-swarm", "shoo", side="far", charge=8, fx=None, notes="Swarm: the bees shooed off at the foe")
    smear_salve(S + "hivekeeper-honey-salve", notes="Honey Salve: the hand dipped in the pot and the salve smeared on the ally")
    throw(S + "hivekeeper-smoke-pot", "lob", notes="Smoke Pot: lobbed into the row")
    shake_hive(S + "hivekeeper-angry-hive", notes="Angry Hive: the smoker shaken hard overhead and the hive let loose on the row")
    two_hand_cast(S + "hivekeeper-the-whole-hive", "hug", charge=26, hold=8, settle=24,
                  notes="The Whole Hive: hunched over, the arms hugged in, then flung up high, every bee out")
    # Glasswright
    kit("one-hand")
    hand_cast(S + "glasswright-prism-ray", "point", side="far", charge=10,
              after=lambda o: [B(6, F(base=o, torso=o["torso"] - 2, arm_far_up=o["arm_far_up"] + 20), "in"),
                               B(3, F(base=o, arm_far_up=o["arm_far_up"] - 10), "back", ev=(("release", "hand_far"),))],
              notes="Prism Ray: a ray from the fingertips, and its echo split off to another foe")
    # Mirror Image is a spell, not a blow: the free hand drawn up across the face, glowing, then swept
    # out and down as the images split off, the sword kept at guard and no trail.
    veil = F(torso=-4, look=-6, legs=L(-2, 2), far=A(125, 95, wrist=-30))
    split = F(torso=3, look=4, legs=L(1, 3, near=2), far=A(55, 15, wrist=-60))
    seq(S + "glasswright-mirror-image", [B(10, veil, "out", ev=(("fx", "glow-hand"),)), B(3, F(base=veil, torso=-6), "in"),
                                         B(4, split, "back", ev=(("release", "hand_far"), ("fx", "burst-self"))),
                                         B(8, F(base=split, torso=4), "smooth")],
        settle=14, blend=0.06, notes="Mirror Image: the hand across the face, swept away, and three of you")
    hand_cast(S + "glasswright-refract", "sweep", side="far", charge=3, hold=1, follow_frames=6, settle=10, fx=None,
              notes="Refract: a sweep of the hand and the shot turned aside")
    two_hand_cast(S + "glasswright-glasshouse", "spread", charge=12, notes="Glasshouse: the hands drawn in and the glass flung open")
    reach_out(S + "glasswright-given-in-love", "open", charge=28, linger=26, settle=20,
              notes="Given in Love: both hands to the heart, then opened out low to every ally")
    # Lamplighter
    kit("one-hand")
    heavy(S + "lamplighter-lantern-swing", "one-hand", 2, timing=(6, 2, 3, 8, 14), glint=False,
          notes="Lantern Swing: the lantern swung up into the foe")
    item_up(S + "lamplighter-sanctuary", item=180, hold=("carried", "blade"), charge=10, linger=14, style="low",
            notes="Sanctuary: the lantern lifted, then set low before the ally, crouched over it")
    item_up(S + "lamplighter-vespers", item=180, hold=("carried", "blade"), charge=14, linger=24, style="face",
            notes="Vespers: the lantern lifted before the bowed head, the evening prayer")
    lantern_out(S + "lamplighter-reveal", notes="Reveal: the lantern thrust high into the dark, the eyes shaded, peering")
    ring_bell(S + "lamplighter-the-bell", rings=4, notes="The Bell: rung high, and every ally comes clear")
    # Crownsworn
    kit("one-hand")
    order_point(S + "crownsworn-command", notes="Command: the blade at the shoulder and the ally pointed on")
    point_at(S + "crownsworn-told-rudely", item=90, taunt=True, fx=None, notes="Told Rudely: the blade levelled and a beckon")
    rally_call(S + "crownsworn-rally", notes="Rally: the blade swept up from low to the sky and pumped, the call to rise")
    combo(S + "crownsworn-my-fathers-sword", [("one-hand", 2, 7), ("one-hand", 3, 8), ("one-hand", 2, 8),
                                              ("two-hand", 1, 14)], start_fx="glow-weapon", ultimate=True, settle=14,
          notes="My Father's Sword: three cuts and the blade brought down")
    # Hourwarden
    kit("staff")
    seq_second_hand(S + "hourwarden-second-hand")
    reach_out(S + "hourwarden-rewind", "circle", notes="Rewind: the hand turned back round the clock")
    quicken(S + "hourwarden-haste", notes="Haste: a hand wound like a clock's key and flicked at the ally, rolling on")
    hand_cast(S + "hourwarden-stop", "halt", side="far", charge=10, hold=4, follow_frames=18,
              notes="Stop: the arm drawn in, then raised high and held flat at the foe, leant back from it")
    two_hand_cast(S + "hourwarden-bleed-the-clock", "clock", charge=30, hold=10, settle=24,
                  notes="Bleed the Clock: the arms up at twelve, then flung to three and six like a clock's hands")


def seq_pocket(name):
    """Everyone's Pocket: a run of quick snatches along the foes, then the twin cuts."""
    beats = []
    for k in range(3):
        crouch = F(torso=14, look=4, legs=L(-1, 6), near=A(-10, 60, item=170), far=A(30, 80), root__x=6 * k)
        grab = F(torso=22, look=6, legs=L(6, 8, near=8), near=A(-10, 60, item=170), far=A(80, 10), root__x=6 * k + 5)
        beats += [B(5 if k == 0 else 3, crouch, "out"), B(3, grab, "back", ev=(("fx", "coin"),))]
    sw = swings()["dagger"]
    for n, gap in ((1, 4), (2, 8)):
        wind, coil, contact, follow, frames, smear, _ = sw[n - 1]
        beats += [B(gap, F(base=coil, root__x=12), "in"),
                  B(frames[2], F(base=contact, root__x=12), "back", ev=("hit",), smear=("near", "x") if smear else None)]
    beats += [B(10, F(base=sw[1][3], root__x=12), "smooth"), B(10, F(base=C.READY, root__x=4, legs=L(0, 3, lift=(4, 0))), "smooth")]
    seq(name, beats, settle=14, blend=0.04, notes="Everyone's Pocket: a hand in every pocket, then the twin cuts")


def seq_second_hand(name):
    """Second Hand: the staff swung, and the blow landing a beat behind the swing, as if the
    swing's echo struck."""
    wind, coil, contact, follow, frames, smear, _ = swings()["staff"][1]
    beats = [B(6, wind, "linear"), B(2, coil, "in", trail="start"), B(3, contact, "back", smear=("near", "y")),
             B(6, follow, "smooth", trail="end"), B(4, follow, "smooth", ev=("hit", ("fx", "echo")))]
    seq(name, beats, settle=15, blend=0.04, notes="Second Hand: the swing, and its echo landing a beat behind it")


# --------------------------------------------------------------------------- #
# Foes' special moves
# --------------------------------------------------------------------------- #

def foes():
    kit(None)
    hk = ("blade", "blade")
    # Bosun Grell's hook thrown on its chain and the foe pulled in.
    w = F(torso=-8, look=-4, legs=L(-3, 2), near=A(-15, 110, item=150), far=A(150, 60, item=220), hold=hk)
    o = F(torso=14, look=4, legs=L(6, 4, near=8), near=A(-20, 110, item=150), far=A(92, 4, item=95), hold=hk)
    yank = F(torso=-16, look=-4, legs=L(-8, 6, far=-8), near=A(-20, 110, item=150), far=A(10, 120, item=140), hold=hk)
    seq("foe-bosun-grell-hook-pull", [B(10, w, "out"), B(3, w, "in"), B(4, o, "back", ev=(("release", "hand_far"),)),
                                      B(8, o, "smooth", ev=("hit",)), B(6, yank, "back"), B(8, yank, "smooth")],
        settle=18, blend=0.05, notes="the hook thrown out on its chain, caught, and the foe hauled in")
    combo("foe-bosun-grell-multiattack", [("hook", 1, 8), ("hook", 2, 14)], notes="the hook, then the cutlass")
    # Kael: a feint (a lunge pulled back), then the real stab.
    sw = swings()["one-hand"][2]
    wind, coil, contact, follow = sw[0], sw[1], sw[2], sw[3]
    half = F(base=contact, legs=L(4, 3, near=6), torso=6)
    seq("foe-kael-duel-feint", [B(5, wind, "linear"), B(2, coil, "in"), B(4, half, "out"), B(5, coil, "in", trail="start"),
                                B(2, contact, "back", ev=("hit",), smear=("near", "x")), B(9, follow, "smooth", trail="end"),
                                B(9, F(base=follow, legs=L(3, 2, near=5, lift=(5, 0))), "smooth")],
        settle=10, blend=0.04, notes="a lunge that stops short, and the stab that doesn't")
    combo("foe-kael-duel-multiattack", [("dagger", 1, 7), ("one-hand", 3, 10)], notes="a quick cut, then the rapier's lunge")
    throw("foe-kael-duel-smoke-bomb", "spike", notes="a smoke bomb hurled down at the floor, the rapier kept on the foe")
    fencer_bound("foe-kael-duel-cunning-disengage", notes="a fencer's bound back out of reach, the rapier still levelled")
    # The Hollowed Hexer's bolt of ash from the staff, and its hex.
    kit("staff")
    hand_cast("foe-hollowed-hexer-ashen-bolt", "scoop", side="far", charge=14, hold=4,
              notes="hunched low, ash swept up from behind the knee and flung at the foe")
    curse("foe-hollowed-hexer-hex", notes="the curse laid with the hand thrust out, then wrenched back in to the chest")
    kit(None)
    # The sappers light a charge and lob it.
    # One ability for both sappers, so one clip: the chief's is an alias of it (clips.py's
    # likeness() reads two clips as two moves).
    for foe in ("hollowed-sapper",):
        light = F(torso=14, look=16, legs=L(0, 6), near=A(-10, 60, item=170), far=A(30, 110, wrist=-20))
        seq("foe-%s-blasting-charge" % foe,
            [B(10, light, "out", ev=(("fx", "spark"),)), B(12, F(base=light, torso=16), "smooth"),
             B(8, F(torso=10, look=0, legs=L(-2, 6), near=A(-10, 60, item=170), far=A(-40, 30, wrist=10)), "linear"),
             B(3, F(torso=10, look=0, legs=L(-2, 6), near=A(-10, 60, item=170), far=A(-46, 32, wrist=10)), "in"),
             B(4, F(torso=-6, look=-10, legs=L(3, 1, near=4), near=A(-10, 60, item=170), far=A(120, 10, wrist=-40)), "back",
               ev=(("release", "hand_far"),)),
             B(8, F(torso=-8, look=-12, legs=L(3, 1, near=4), near=A(-10, 60, item=170), far=A(135, 10, wrist=-40)), "smooth")],
            settle=16, blend=0.05, notes="a blasting charge lit and lobbed over the front row")
    C.alias("foe-hollowed-sapper-blasting-charge", "foe-sapper-chief-blasting-charge")
    heavy("foe-sapper-chief-lantern", "one-hand", 3, timing=(10, 3, 3, 10, 16), fx="spark",
          notes="the lantern rammed into the foe")
    heavy("foe-vault-construct-gilded-slam", "unarmed", 2, timing=(14, 4, 3, 10, 20), notes="a slow, heavy slam of the fist")
    # The Hollowed footman's telegraphed chop: the rusted blade taken up in both hands and brought
    # down over the top, harder than its auto.
    heavy("foe-hollowed-footman-overhead-chop", "two-hand", 1, timing=(12, 4, 3, 10, 20), power=1.4,
          notes="the rusted blade raised in both hands and chopped down")
    seq("foe-vault-construct-trip", [B(10, F(torso=10, look=6, legs=L(-2, 8), near=A(-20, 40), far=A(-10, 40)), "out"),
                                     B(3, F(torso=12, legs=L(-3, 9), near=A(-30, 40), far=A(-20, 40)), "in"),
                                     B(4, F(torso=30, look=20, legs=L(6, 16, near=8), near=A(40, 10), far=A(30, 10)), "back",
                                       ev=("hit", ("fx", "burst-ground"))),
                                     B(10, F(torso=32, look=20, legs=L(6, 16, near=8), near=A(40, 10), far=A(30, 10)), "smooth")],
        settle=18, blend=0.04, notes="a sweep at the feet")


# --------------------------------------------------------------------------- #

def install(clips_module):
    """Adds every fighting clip to clips.py's library."""
    global C, _RY
    C = clips_module
    _RY = C.READY["hips.y"]
    breathing()
    autos()
    telegraphs()
    reactions()
    victories()
    generic()
    skills()
    foes()


def combat_map():
    """clips.json's combat map: the old screen's moves (windup, strike, ...) and the new ones."""
    m = {}
    m["auto"] = {k: "auto-%s-1" % k for k in WEAPON_KINDS}
    m["auto-2"] = {k: "auto-%s-2" % k for k in WEAPON_KINDS}
    m["auto-3"] = {k: "auto-%s-3" % k for k in WEAPON_KINDS}
    m["telegraph"] = {k: "telegraph-" + k for k in WEAPON_KINDS}
    m["victory"] = {k: "victory-" + k for k in WEAPON_KINDS}
    for move in ("knockdown", "getup", "collapse", "drink-potion", "soul-link-strike"):
        m[move] = move
    return m
