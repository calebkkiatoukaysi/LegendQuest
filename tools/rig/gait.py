"""
The figures' ways of getting about, worked out rather than keyed by hand: the jog the party moves
at, the walk townsfolk stroll at, and the run (clips.py adds them as "jog", "walk" and "run", and
again for each base's own proportions as "jog@base-male" and so on).

Each foot is planted. While it's on the ground it stays put as the figure moves on over it at the
cycle's speed, landing on its heel, rolling flat and peeling off its toe, and the leg reaches it by
two-bone IK, the knee bending forward. In the air it swings through on curves of thigh, knee and
ankle angle shaped the way a person's are (the heel kicked up behind, the knee driven forward, the
shin reaching out to land), joined smoothly to where the stance leaves off and picks up. The hips
ride at whatever height gives the planted knee its bend through the stance (so they're lowest just
after landing in a walk, mid-stance in a jog, and arc through the air between a jog's steps), the
arms swing against the legs with each forearm and hand a beat behind the arm before it, the torso
gives a little on each landing a beat after the hips, and the head stays level.

The feet run in tracks under the hips (the mannequins stand with their legs apart; a person on the
move doesn't). In the three-quarter view the near leg is drawn longer than the far, so its track is
lowered just enough that both legs carry the hips alike.

    python tools/rig/gait.py        how each cycle's knees, feet and hips come out

Only needs CPython (and skeleton.py's tables).
"""

import math
import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
if HERE not in sys.path:
    sys.path.insert(0, HERE)
import skeleton as S  # noqa: E402

FPS = 30
"""Keys a second the cycles are worked out at; the game eases smoothly between them."""

STRAIGHTEST = 2.0
"""The least a knee bends, in degrees: a leg never locks quite straight, nor bends backward."""

GAME_WALK_SPEED = 56.0
"""How fast the party moves, in world px a second (ExploreScreen.WalkSpeed): the jog's feet are planted at it."""
STROLL_SPEED = 24.0
"""How fast townsfolk stroll (Npc.StrollSpeed): the walk's feet are planted at it."""
FIGURE_HEIGHT = 196.0 / 4.0
"""How tall a standard adult stands, in world px: ImportedArt.RoomHeight over ZoneCatalog.RoomScale."""


def world_speed(px_per_second):
    """A speed in world px a second as rig px a second (a figure is S.RIG_HEIGHT rig px tall)."""
    return px_per_second * S.RIG_HEIGHT / FIGURE_HEIGHT


#: Where each foot's sole meets the ground at rest, (heel, toe) on the canvas, measured off the
#: bases: the far foot points ahead, the near one out of the picture. A foot rolls about its heel as
#: it lands and its toe as it leaves; both roll about the side-on far foot's, so the ankle rises the
#: same whichever foot it is.
SOLE = {
    "base-male": {"near": ((284, 1464), (372, 1464)), "far": ((556, 1406), (744, 1418))},
    "base-female": {"near": ((304, 1462), (374, 1462)), "far": ((572, 1407), (734, 1420))},
}


def _point(base, xy):
    j = S.JOINTS[base]
    unit = S.RIG_PX[base]
    return ((xy[0] - j["root"][0]) / unit, (xy[1] - j["root"][1]) / unit)


def _mean(points):
    return (sum(p[0] for p in points) / len(points), sum(p[1] for p in points) / len(points))


def _angle(a, b):
    return math.atan2(b[1] - a[1], b[0] - a[0])


def _turn(v, degrees):
    """A vector turned clockwise (y down) by some degrees."""
    a = math.radians(degrees)
    c, s = math.cos(a), math.sin(a)
    return (v[0] * c - v[1] * s, v[0] * s + v[1] * c)


def _wrap(degrees):
    """An angle brought into (-180, 180]."""
    return -((-degrees + 180.0) % 360.0 - 180.0)


def _smooth(f):
    f = min(1.0, max(0.0, f))
    return f * f * (3 - 2 * f)


def _hermite(keys, x):
    """A cubic through (x, value, slope or None) keys, a slope left None taken from its neighbours
    (or flat at an end)."""
    n = len(keys)
    slopes = []
    for i, (_, _, m) in enumerate(keys):
        if m is not None:
            slopes.append(m)
        elif 0 < i < n - 1:
            slopes.append((keys[i + 1][1] - keys[i - 1][1]) / (keys[i + 1][0] - keys[i - 1][0]))
        else:
            slopes.append(0.0)
    for i in range(n - 1):
        x0, v0, _ = keys[i]
        x1, v1, _ = keys[i + 1]
        if x <= x1 or i == n - 2:
            h = x1 - x0
            f = (x - x0) / h
            f2, f3 = f * f, f * f * f
            return ((2 * f3 - 3 * f2 + 1) * v0 + (f3 - 2 * f2 + f) * h * slopes[i]
                    + (-2 * f3 + 3 * f2) * v1 + (f3 - f2) * h * slopes[i + 1])
    return keys[-1][1]


class Leg:
    """One leg's rest geometry in rig px from the root, on a base (None: both averaged): the hip,
    knee and ankle, the sole's heel and toe, and from them its lengths and rest angles."""

    def __init__(self, side, base=None):
        bases = [base] if base else list(S.JOINTS)
        j = lambda name: _mean([_point(b, S.JOINTS[b][name + "_" + side]) for b in bases])  # noqa: E731
        self.side = side
        self.hip, self.knee, self.ankle = j("hip"), j("knee"), j("ankle")
        self.heel = _mean([_point(b, SOLE[b][side][0]) for b in bases])
        self.toe = _mean([_point(b, SOLE[b][side][1]) for b in bases])
        self.l1 = math.dist(self.hip, self.knee)
        self.l2 = math.dist(self.knee, self.ankle)
        self.thigh0 = math.degrees(_angle(self.hip, self.knee))
        self.shin0 = math.degrees(_angle(self.knee, self.ankle))
        self.heel_off = (self.heel[0] - self.ankle[0], self.heel[1] - self.ankle[1])
        self.toe_off = (self.toe[0] - self.ankle[0], self.toe[1] - self.ankle[1])
        self.home = self.ankle

    def span(self, bend):
        """How far the hip is from the ankle with the knee bent so far (degrees)."""
        return math.sqrt(self.l1 ** 2 + self.l2 ** 2 + 2 * self.l1 * self.l2 * math.cos(math.radians(bend)))

    def reach(self, hip, ankle):
        """Two-bone IK, the knee forward: (thigh, knee bend) in degrees, the thigh's lean forward of
        straight down and the knee's bend from straight, and how far short the leg fell (0 if it
        reached)."""
        d = math.dist(hip, ankle)
        most = self.l1 + self.l2 - 1e-6
        short = max(0.0, d - most)
        d = min(max(d, abs(self.l1 - self.l2) + 1e-6), most)
        toward = _angle(hip, ankle)
        at_hip = math.acos(max(-1.0, min(1.0, (self.l1 ** 2 + d ** 2 - self.l2 ** 2) / (2 * self.l1 * d))))
        thigh = toward - at_hip
        knee = (hip[0] + self.l1 * math.cos(thigh), hip[1] + self.l1 * math.sin(thigh))
        shin = math.atan2(ankle[1] - knee[1], ankle[0] - knee[0])
        # A thigh leans from straight back (-120) round through forward to over the head (240).
        lean = (90.0 - math.degrees(thigh) + 120.0) % 360.0 - 120.0
        return lean, _wrap(math.degrees(shin - thigh)), short

    def ankle_at(self, hip, lean, bend):
        """Forward kinematics: the ankle for the hip, the thigh's lean and the knee's bend."""
        thigh = math.radians(90.0 - lean)
        knee = (hip[0] + self.l1 * math.cos(thigh), hip[1] + self.l1 * math.sin(thigh))
        shin = thigh + math.radians(bend)
        return (knee[0] + self.l2 * math.cos(shin), knee[1] + self.l2 * math.sin(shin))

    def channels(self, lean, bend, foot):
        """The clip channels for a thigh lean, a knee bend and a foot roll (absolute, degrees)."""
        thigh = 90.0 - lean
        up = thigh - self.thigh0
        low = (thigh + bend) - self.shin0 - up
        return {"leg_%s_up" % self.side: up, "leg_%s_low" % self.side: low,
                "foot_%s" % self.side: _wrap(foot - (up + low))}


class Arm:
    """One arm's rest geometry: the shoulder, elbow and wrist, and the rest angles of its two bones."""

    def __init__(self, side, base=None):
        bases = [base] if base else list(S.JOINTS)
        j = lambda name: _mean([_point(b, S.JOINTS[b][name + "_" + side]) for b in bases])  # noqa: E731
        self.side = side
        self.shoulder, self.elbow, self.wrist = j("shoulder"), j("elbow"), j("wrist")
        self.up0 = math.degrees(_angle(self.shoulder, self.elbow))
        self.low0 = math.degrees(_angle(self.elbow, self.wrist))

    def channels(self, swing, bend, wrist, carried):
        """The clip channels for the upper arm's swing forward of straight down, the elbow's bend and
        the wrist's (degrees), under a trunk turned `carried` degrees in all."""
        up_dir = 90.0 - swing
        low_dir = up_dir - bend
        up = up_dir - self.up0 - carried
        low = low_dir - self.low0 - (carried + up)
        return {"arm_%s_up" % self.side: up, "arm_%s_low" % self.side: low, "hand_%s" % self.side: wrist}


class Gait:
    """A locomotion cycle on a base (None: both averaged). Times are shares of the cycle; the near
    foot lands at 0, the far at 0.5.

    period      seconds for two steps
    speed       rig px a second the planted feet are left behind at
    duty        the share of the cycle each foot is on the ground (over 0.5 a walk, under it a run)
    ahead       how much of the planted sweep is ahead of the hip when the foot lands
    knee        the planted knee's bend through the stance, [(share of the stance, degrees)]
    land, flat  the foot's roll as it lands (- toes up), and the share of the stance till it's flat
    peel, off   the share of the stance the heel starts to rise at, and the roll at toe-off
    near_roll   how much of the roll the near foot (pointing out of the picture) shows
    swing       the swing's keys, at shares of the swing: {"lean": [(w, deg)], "bend": [...], "foot": [...]}
    arms        {"swing": (middle, size), "bend": (middle, size), "wrist": (middle, size), "lag": share}:
                the upper arm's swing forward of straight down, the elbow's bend and the wrist's,
                each following the one before it by `lag` of the cycle
    lean        (forward, give): the torso's lean forward, and how much more it leans as the weight
                comes down on each foot
    """

    def __init__(self, base=None, **kw):
        self.near_roll = 0.6
        self.__dict__.update(kw)
        self.base = base
        self.legs = {"near": Leg("near", base), "far": Leg("far", base)}
        self.arms_ = {"near": Arm("near", base), "far": Arm("far", base)}
        self._place_tracks()
        heights = [self.hips(k / 120.0) for k in range(120)]
        self._mid = (max(heights) + min(heights)) / 2.0
        self._half = max(1e-6, (max(heights) - min(heights)) / 2.0)

    # -- where the feet go ------------------------------------------------------------------------ #

    def _place_tracks(self):
        """Each foot's track runs under its hip. The far foot's on the ground it stands on; the
        near foot's is lowered (or raised) till, with the knee bent alike, it carries the hips as
        high as the far one does."""
        far, near = self.legs["far"], self.legs["near"]
        far.home = (far.hip[0], far.ankle[1])
        middle = self.stance_bend(self.duty * 0.5)
        hip_drop = far.home[1] - far.span(middle) - far.hip[1]
        near.home = (near.hip[0], near.hip[1] + hip_drop + near.span(middle))

    def sweep(self):
        return self.speed * self.period * self.duty

    def ground_x(self, leg, p):
        """Where a planted foot's ankle is across (flat), a share p of the cycle after it landed."""
        return leg.home[0] + self.sweep() * self.ahead - self.speed * self.period * p

    def roll(self, p):
        """The planted foot's roll, a share p of the cycle after it landed."""
        s = p / self.duty
        if s < self.flat:
            return self.land * (1 - _smooth(s / self.flat))
        if s < self.peel:
            return 0.0
        f = (s - self.peel) / (1 - self.peel)
        return self.off * f * f

    def planted(self, leg, p):
        """A planted foot's ankle a share p of the cycle after it landed, rolled about the heel or
        toe of a foot seen side-on."""
        side_on = self.legs["far"]
        flat = (self.ground_x(leg, p), leg.home[1])
        roll = self.roll(p)
        if abs(roll) < 1e-9:
            return flat
        off = side_on.toe_off if roll > 0 else side_on.heel_off
        r = _turn(off, roll)
        return (flat[0] + off[0] - r[0], flat[1] + off[1] - r[1])

    # -- the hips ------------------------------------------------------------------------------------ #

    def stance_bend(self, p):
        """How far the planted knee bends, a share p of the cycle after it landed."""
        return _hermite([(s, v, None) for s, v in self.knee], p / self.duty)

    def _carried(self, side, p, bend=None):
        """The hips' offset down that gives a planted leg its knee bend (or the one given)."""
        leg = self.legs[side]
        ankle = self.planted(leg, p)
        d = leg.span(self.stance_bend(p) if bend is None else bend)
        dx = ankle[0] - leg.hip[0]
        return ankle[1] - math.sqrt(max(0.0, d * d - dx * dx)) - leg.hip[1]

    def _carried_both(self, p):
        """The hips' offset down a foot a share p into its stance puts them at: the two legs'
        averaged, so the two steps rise and fall alike, but never so high either can't reach."""
        mean = 0.5 * (self._carried("near", p) + self._carried("far", p))
        return max(mean, self._carried("near", p, STRAIGHTEST), self._carried("far", p, STRAIGHTEST))

    def hips(self, q):
        """The hips' offset down at a share q of the cycle: carried by whichever feet are down (the
        lower they put it, so both reach), or in the air on an arc from toe-off to the landing. It's
        the same for both steps."""
        step = (2.0 * (q % 1.0)) % 1.0
        held = [self._carried_both(p) for p in (step / 2.0, step / 2.0 + 0.5) if p < self.duty]
        if held:
            return max(held)
        # In the air (a jog or a run): the trailing foot left at step share 2 * duty, the next lands at 1.
        eps = 1e-4
        a, a0 = self._carried_both(self.duty - eps), self._carried_both(self.duty - 2 * eps)
        b, b1 = self._carried_both(0.0), self._carried_both(eps)
        start = 2.0 * self.duty
        span = 1.0 - start
        f = (step - start) / span
        # Each end's slope per share of the cycle, as one per share of this arc.
        return _hermite([(0.0, a, (a - a0) / eps * 0.5 * span), (1.0, b, (b1 - b) / eps * 0.5 * span)], f)

    def sink(self, q):
        """How low the hips are against their range, -1 at the top and 1 at the bottom."""
        return (self.hips(q) - self._mid) / self._half

    def phase(self, side, q):
        return q % 1.0 if side == "near" else (q - 0.5) % 1.0

    # -- each leg ---------------------------------------------------------------------------------- #

    def stance(self, side, p, q):
        leg = self.legs[side]
        hip = (leg.hip[0], leg.hip[1] + self.hips(q))
        lean, bend, short = leg.reach(hip, self.planted(leg, p))
        return lean, bend, self.roll(p) * (self.near_roll if side == "near" else 1.0), short

    def leg(self, side, q):
        """(thigh lean, knee bend, foot roll, how far short the leg fell) at a share q of the cycle."""
        p = self.phase(side, q)
        if p < self.duty:
            return self.stance(side, p, q)
        # In the air: from toe-off to the next landing through the swing's keys, leaving and arriving
        # at the stance's own angles and speeds.
        eps = 1e-4
        q_off = q - (p - self.duty)
        q_on = q + (1.0 - p)
        a = self.stance(side, self.duty - eps, q_off - eps)
        a0 = self.stance(side, self.duty - 2 * eps, q_off - 2 * eps)
        b = self.stance(side, 0.0, q_on)
        b1 = self.stance(side, eps, q_on + eps)
        span = 1.0 - self.duty
        w = (p - self.duty) / span
        out = []
        for i, name in enumerate(("lean", "bend", "foot")):
            keys = [(0.0, a[i], (a[i] - a0[i]) / eps * span)]
            keys += [(x, v, None) for x, v in self.swing[name]]
            keys.append((1.0, b[i], (b1[i] - b[i]) / eps * span))
            out.append(_hermite(keys, w))
        return out[0], max(STRAIGHTEST, out[1]), out[2], 0.0

    # -- the whole figure ---------------------------------------------------------------------------- #

    def pose(self, q):
        pose = {"hips.y": self.hips(q)}
        forward, give = self.lean
        # The torso gives a little as the weight comes down on each foot, a beat after the hips and
        # most of it a beat after that; the head stays level, nodding with it later still.
        pose["torso"] = forward + give * (0.4 * self.sink(q - 0.03) + 0.6 * self.sink(q - 0.06))
        pose["head"] = -forward - 1.0 + 0.5 * give * self.sink(q - 0.1)
        carried = pose["torso"]
        sw_mid, sw_size = self.arms["swing"]
        bd_mid, bd_size = self.arms["bend"]
        wr_mid, wr_size = self.arms["wrist"]
        lag = self.arms["lag"]
        for side in ("near", "far"):
            lean, bend, foot, _ = self.leg(side, q)
            pose.update(self.legs[side].channels(lean, bend, foot))
            # Each arm swings against its own leg, furthest forward as that leg pushes off behind;
            # the forearm and the hand follow, each a beat later.
            p = self.phase(side, q) - self.duty
            swing = sw_mid + sw_size * math.cos(2 * math.pi * p)
            elbow = bd_mid + bd_size * math.cos(2 * math.pi * (p - lag))
            wrist = wr_mid + wr_size * math.cos(2 * math.pi * (p - 2 * lag))
            pose.update(self.arms_[side].channels(swing, elbow, wrist, carried))
        return pose

    def keys(self):
        n = max(8, round(self.period * FPS))
        return [(self.period * k / n, self.pose(k / float(n))) for k in range(n)]

    def events(self):
        return [(0.0, "step"), (self.period / 2.0, "step")]

    def report(self, name):
        """How the cycle comes out: each knee's bend planted and swinging, how far short a leg falls,
        how low each swinging foot comes (rig px above its ground), and the hips' range."""
        lines = []
        for side, leg in self.legs.items():
            st, sw, clear, short = [], [], [], 0.0
            for k in range(FPS * 2):
                q = k / (FPS * 2.0)
                lean, bend, foot, s = self.leg(side, q)
                short = max(short, s)
                p = self.phase(side, q)
                (st if p < self.duty else sw).append(bend)
                w = (p - self.duty) / (1.0 - self.duty)
                if 0.1 < w < 0.9:
                    hip = (leg.hip[0], leg.hip[1] + self.hips(q))
                    ankle = leg.ankle_at(hip, lean, bend)
                    clear.append(min(leg.home[1] + o[1] - (ankle[1] + _turn(o, foot)[1]) for o in (leg.heel_off, leg.toe_off)))
            lines.append("  %-18s %-4s knee planted %3.0f-%-3.0f swinging %3.0f-%-3.0f  swinging foot at least %4.1f above the ground%s"
                         % (name, side, min(st), max(st), min(sw), max(sw), min(clear), "  SHORT %.1f" % short if short > 0.05 else ""))
        hy = [self.hips(k / 60.0) for k in range(60)]
        lines.append("  %-18s hips %.1f to %.1f below rest, near foot's track %.1f below its rest"
                     % (name, min(hy), max(hy), self.legs["near"].home[1] - self.legs["near"].ankle[1]))
        return lines


# --------------------------------------------------------------------------- #
# The cycles
# --------------------------------------------------------------------------- #

def jog(base=None):
    """The party's pace: an upright, steady jog, a short flight between steps, the elbows bent near
    square and pumping, the torso leaning a little into it."""
    return Gait(base, period=0.7, speed=world_speed(GAME_WALK_SPEED), duty=0.38, ahead=0.4,
                knee=[(0.0, 18.0), (0.45, 40.0), (1.0, 16.0)], land=-8.0, flat=0.22, peel=0.55, off=28.0,
                swing={"lean": [(0.25, -10.0), (0.55, 16.0), (0.78, 28.0)],
                       "bend": [(0.18, 52.0), (0.42, 92.0), (0.66, 70.0), (0.86, 34.0)],
                       "foot": [(0.3, 22.0), (0.62, 6.0), (0.85, -4.0)]},
                arms={"swing": (-10.0, 24.0), "bend": (80.0, 10.0), "wrist": (-6.0, 6.0), "lag": 0.06},
                lean=(6.0, 1.2))


def walk(base=None):
    """A relaxed walk for strolling townsfolk: heel to toe, the planted leg near straight, the arms
    hanging and swinging easily."""
    return Gait(base, period=1.1, speed=world_speed(STROLL_SPEED), duty=0.62, ahead=0.5,
                knee=[(0.0, 4.0), (0.2, 15.0), (0.5, 6.0), (0.78, 9.0), (1.0, 36.0)],
                land=-14.0, flat=0.2, peel=0.6, off=26.0,
                swing={"lean": [(0.3, 4.0), (0.62, 20.0), (0.86, 19.0)],
                       "bend": [(0.3, 60.0), (0.6, 38.0), (0.85, 10.0)],
                       "foot": [(0.3, 12.0), (0.6, 0.0), (0.88, -10.0)]},
                arms={"swing": (-4.0, 13.0), "bend": (16.0, 8.0), "wrist": (-2.0, 4.0), "lag": 0.07},
                lean=(2.0, 0.6))


def run(base=None):
    """A cutscene's run: a longer flight, a deeper lean, the knees and arms driving harder."""
    return Gait(base, period=0.56, speed=world_speed(GAME_WALK_SPEED * 1.6), duty=0.31, ahead=0.4,
                knee=[(0.0, 20.0), (0.45, 46.0), (1.0, 18.0)], land=-6.0, flat=0.2, peel=0.5, off=34.0,
                swing={"lean": [(0.22, -16.0), (0.55, 24.0), (0.78, 40.0)],
                       "bend": [(0.2, 70.0), (0.42, 118.0), (0.66, 92.0), (0.86, 40.0)],
                       "foot": [(0.3, 30.0), (0.62, 8.0), (0.85, -4.0)]},
                arms={"swing": (-12.0, 34.0), "bend": (84.0, 14.0), "wrist": (-8.0, 8.0), "lag": 0.06},
                lean=(11.0, 1.6))


CYCLES = {"jog": jog, "walk": walk, "run": run}
"""Each cycle by its clip's name; clips.py makes one on the bases averaged, and one for each base."""


def main():
    for name, make in CYCLES.items():
        for base in [None] + sorted(S.JOINTS):
            for line in make(base).report(name + ("@" + base if base else "")):
                print(line)


if __name__ == "__main__":
    main()
