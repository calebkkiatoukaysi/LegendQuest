"""
The scissors a painted mannequin is cut into its pieces with: every cut runs exactly along a line
the picture itself draws (the jawline, the round of a shoulder, a joint's seam), found to a fraction
of a pixel, so each piece keeps its own outline whole and the pieces put back together are the
picture again.

    python tools/rig/scissors.py <figure.png> <base> <folder>    # cuts it; writes the pieces and a check

A seam is a few points clicked near a drawn line (SEAMS). Between them the cut follows the line's
ink (a live wire: the cheapest path over the picture, where ink is cheap and bare skin dear), is
smoothed, and is then moved to the ink's far edge, so the piece in front keeps the whole line.

Only needs CPython, numpy and scipy.
"""

import heapq
import os
import sys

import numpy as np
from scipy import ndimage as ndi

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.dirname(HERE))
sys.path.insert(0, HERE)

#: A drawn line is at most this wide (the closing's window finds darks narrower than it).
INK_WINDOW = 9
#: How much darker than the skin beside it a line is before it counts (a share of that skin's
#: lightness), and the share at which it is wholly ink.
INK_FROM, INK_FULL = 0.12, 0.50
#: What a step of the wire costs on ink, on bare skin, and over the background.
STEP_INK, STEP_SKIN, STEP_OUT = 1.0, 40.0, 6.0
#: How far a clicked point may move to sit on the ink, and how far round its points a seam looks.
SNAP, WINDOW = 6, 36
#: The cut is this far past the ink's edge (so no pixel on the far side of it has any of the line
#: in it), and a seam's ends run on this far, so that seams that meet still cross once each is
#: moved to its own ink's edge; an end out by the black runs on until it is off the figure (at
#: most so far), and an end deeper in the figure than MEETS is one that meets another seam.
PAST_INK, RUN_ON, RUN_ON_MOST, MEETS = 0.8, 8, 30, 12
#: A line with no measurable ink (an edge between light and dark) is taken to be this wide.
NOMINAL_INK = 3.0


def luma(colour):
    return colour[:, :, 0] * 0.299 + colour[:, :, 1] * 0.587 + colour[:, :, 2] * 0.114


def ridge(colour):
    """How much darker each pixel is than the lightest it could be without its thin darks: a line
    drawn on skin stands out; shading, and the outline against the black round the figure, don't."""
    light = luma(colour)
    closed = ndi.grey_closing(light, size=(INK_WINDOW, INK_WINDOW))
    return closed - light, closed


def inkness(colour, alpha):
    """0 to 1: how much each pixel is a drawn line."""
    dark, closed = ridge(colour)
    ink = np.clip((dark / (closed + 1.0) - INK_FROM) / (INK_FULL - INK_FROM), 0.0, 1.0)
    return ink * (alpha > 0.5)


def edginess(colour, alpha):
    """0 to 1: how sharply the lightness changes (the line between a lit piece and one in shadow)."""
    light = ndi.gaussian_filter(luma(colour), 1.2)
    gy, gx = np.gradient(light)
    return np.clip(np.hypot(gx, gy) / 30.0, 0.0, 1.0) * (alpha > 0.5)


def wire(cost, start, end, box):
    """The cheapest 8-connected path from `start` to `end` (x, y) inside `box` (x0, y0, x1, y1)."""
    x0, y0, x1, y1 = box
    w, h = x1 - x0, y1 - y0
    c = cost[y0:y1, x0:x1]
    sx, sy, ex, ey = start[0] - x0, start[1] - y0, end[0] - x0, end[1] - y0
    best = np.full((h, w), np.inf)
    came = np.full((h, w), -1, np.int32)
    best[sy, sx] = 0.0
    heap = [(0.0, sx, sy)]
    steps = ((1, 0, 1.0), (-1, 0, 1.0), (0, 1, 1.0), (0, -1, 1.0), (1, 1, 1.4142), (1, -1, 1.4142), (-1, 1, 1.4142), (-1, -1, 1.4142))
    while heap:
        d, x, y = heapq.heappop(heap)
        if d > best[y, x]:
            continue
        if x == ex and y == ey:
            break
        for dx, dy, length in steps:
            nx, ny = x + dx, y + dy
            if 0 <= nx < w and 0 <= ny < h:
                nd = d + length * 0.5 * (c[y, x] + c[ny, nx])
                if nd < best[ny, nx]:
                    best[ny, nx] = nd
                    came[ny, nx] = y * w + x
                    heapq.heappush(heap, (nd, nx, ny))
    path = [(ex, ey)]
    while (path[-1][0], path[-1][1]) != (sx, sy):
        k = came[path[-1][1], path[-1][0]]
        if k < 0:
            raise ValueError("no path from %s to %s" % (start, end))
        path.append((k % w, k // w))
    return [(x + x0, y + y0) for x, y in reversed(path)]


def by_length(points, step=1.0):
    """A line's points again, `step` apart along it."""
    points = np.asarray(points, np.float64)
    seg = np.hypot(*np.diff(points, axis=0).T)
    keep = np.concatenate([[True], seg > 1e-9])
    points, seg = points[keep], seg[seg > 1e-9]
    at = np.concatenate([[0.0], np.cumsum(seg)])
    want = np.linspace(0.0, at[-1], max(2, int(round(at[-1] / step)) + 1))
    return np.stack([np.interp(want, at, points[:, 0]), np.interp(want, at, points[:, 1])], axis=1)


def smooth(points, sigma):
    """A line smoothed along itself with its two ends held."""
    if len(points) < 5 or sigma <= 0:
        return points
    out = ndi.gaussian_filter1d(points, sigma, axis=0, mode="nearest")
    blend = np.clip(np.minimum(np.arange(len(points)), np.arange(len(points))[::-1]) / (2.0 * sigma), 0.0, 1.0)[:, None]
    return points * (1 - blend) + out * blend


def unfolded(moved, line, by):
    """A line moved sideways, without the points that folded over each other inside a sharp turn:
    those that ended nearer the line they came from than they were moved."""
    from scipy.spatial import cKDTree
    near = cKDTree(by_length(line, 0.25)).query(moved)[0]
    keep = near >= by - 0.25
    keep[0] = keep[-1] = True
    return moved[keep]


def circle(points):
    """The circle nearest a line's points: its middle, its radius, how far off it they are (rms)."""
    x, y = points[:, 0], points[:, 1]
    (a, b, c), *_ = np.linalg.lstsq(np.stack([x, y, np.ones(len(x))], axis=1), x * x + y * y, rcond=None)
    cx, cy = a / 2.0, b / 2.0
    r = float(np.sqrt(c + cx * cx + cy * cy))
    return (float(cx), float(cy)), r, float(np.sqrt(np.mean((np.hypot(x - cx, y - cy) - r) ** 2)))


def normals(points):
    """Each point's unit tangent and its normal (the tangent turned a quarter clockwise on screen)."""
    t = np.gradient(points, axis=0)
    t /= np.maximum(np.hypot(t[:, 0], t[:, 1]), 1e-9)[:, None]
    return t, np.stack([-t[:, 1], t[:, 0]], axis=1)


def sample(field, points):
    return ndi.map_coordinates(field, [points[:, 1], points[:, 0]], order=1, mode="nearest")


def onto_ink(points, dark, reach=4.0, step=0.5, over=0.5):
    """A line moved onto the middle of the ink it runs along, and that ink's width at each point
    (nan where there is none to measure): the middle is the centre of the darkness across the line."""
    _, n = normals(points)
    offs = np.arange(-reach, reach + step / 2, step)
    across = np.stack([sample(dark, points + n * o) for o in offs], axis=1)
    peak = across.max(axis=1)
    weight = np.clip(across - over * peak[:, None], 0.0, None)
    # Only the run of darkness the line is on: stop at the first gap either side of the peak.
    top = across.argmax(axis=1)
    for i in range(len(points)):
        row = weight[i]
        k = top[i]
        while k + 1 < len(offs) and row[k + 1] > 0:
            k += 1
        row[k + 1:] = 0
        k = top[i]
        while k - 1 >= 0 and row[k - 1] > 0:
            k -= 1
        row[:k] = 0
    total = weight.sum(axis=1)
    found = (peak > 12.0) & (total > 0)
    shift = np.where(found, (weight * offs).sum(axis=1) / np.maximum(total, 1e-9), 0.0)
    width = np.where(found, (across * (weight > 0)).sum(axis=1) * step / np.maximum(peak, 1e-9), np.nan)
    return shift, width, found


class Seam:
    """One cut: the piece in front of it, the pieces behind, which of them the drawn line belongs
    to ("front", or "back" where a joint's ball is on the piece behind), and the clicked points
    (x, y) or (x, y, "c") for a corner the line turns sharply at. Points marked (x, y, "o") before
    the first or after the last are not on the line: they are where the cut goes on its way out of
    the figure, exactly through each (no snapping, no stepping aside). `edge` is for a line that
    is partly no ink at all, only where a lit piece ends against one in shadow."""

    def __init__(self, name, front, back, points, ink="front", edge=False):
        self.name, self.front, self.back, self.ink, self.edge = name, front, tuple(back), ink, edge
        out = [len(p) > 2 and p[2] == "o" for p in points]
        first, last = out.index(False), len(out) - out[::-1].index(False)
        self.enter = [(float(p[0]), float(p[1])) for p in points[:first]]
        self.leave = [(float(p[0]), float(p[1])) for p in points[last:]]
        points = points[first:last]
        assert not any(out[first:last]), "%s: a way out in the middle of the line" % name
        self.points = [(p[0], p[1]) for p in points]
        self.corner = [len(p) > 2 for p in points]

    def owner(self):
        return self.front if self.ink == "front" else self.back[0]

    def trace(self, cut, inside):
        """Finds the line on a Cutting's picture: `snapped` (the clicked points on the ink),
        `centre` (the ink's middle), `width` (the ink's), then `curve`, the cut itself along the
        ink's far edge. `inside` is a point of the piece that owns the line."""
        ink, cost = cut.ink, cut.cost
        if self.edge:
            ink = np.maximum(ink, cut.edge)
            cost = np.where(cut.inner, STEP_INK + (STEP_SKIN - STEP_INK) * (1.0 - ink) ** 2, STEP_OUT)
        h, w = cost.shape
        snapped = []
        for x, y in self.points:
            if cut.inner[y, x]:
                y0, x0 = max(0, y - SNAP), max(0, x - SNAP)
                near = ink[y0:y + SNAP + 1, x0:x + SNAP + 1]
                yy, xx = np.mgrid[y0:y0 + near.shape[0], x0:x0 + near.shape[1]]
                score = near - 0.03 * np.hypot(xx - x, yy - y)
                if near.max() >= 0.3:
                    k = np.unravel_index(score.argmax(), score.shape)
                    x, y = int(xx[k]), int(yy[k])
            snapped.append((x, y))
        self.snapped = np.array(snapped, np.float64)
        xs, ys = self.snapped[:, 0], self.snapped[:, 1]
        box = (max(0, int(xs.min()) - WINDOW), max(0, int(ys.min()) - WINDOW),
               min(w, int(xs.max()) + WINDOW + 1), min(h, int(ys.max()) + WINDOW + 1))
        runs, run = [], []
        for k in range(len(snapped) - 1):
            leg = wire(cost, snapped[k], snapped[k + 1], box)
            run += leg if not run else leg[1:]
            if self.corner[k + 1]:
                runs.append(run)
                run = []
        runs.append(run)
        centre, width, found = [], [], []
        for run in runs:
            pts = smooth(by_length(run), 2.5)
            shift, wide, ok = onto_ink(pts, cut.dark)
            if self.edge:
                # Where there's no ink, the line is where the lightness changes fastest: no width.
                e_shift, _, e_ok = onto_ink(pts, cut.steep, over=0.75)
                # ...and ink further off the wire than the edge is, is another line's that ends here.
                bare = e_ok & (~ok | (np.abs(e_shift) + 1.0 < np.abs(shift)))
                shift, wide, ok = np.where(bare, e_shift, shift), np.where(bare, 0.0, wide), ok | e_ok
            shift = ndi.gaussian_filter1d(np.where(ok, shift, 0.0), 2.0, mode="nearest")
            pts = smooth(pts + normals(pts)[1] * shift[:, None], 1.5)
            centre.append(pts if not centre else pts[1:])
            width.append(wide if not width else wide[1:])
            found.append(ok if not found else ok[1:])
        self.centre = np.concatenate(centre)
        wide, self.found = np.concatenate(width), np.concatenate(found)
        usual = np.nanmedian(wide) if np.isfinite(wide).any() else NOMINAL_INK
        wide = np.clip(np.where(np.isfinite(wide), wide, usual), 0.0 if self.edge else 1.5, 6.0)
        # Where another line joins it the ink is a blot: the line is as wide there as either side.
        wide = ndi.median_filter(wide, JOINED, mode="nearest")
        self.width = ndi.gaussian_filter1d(wide, 4.0, mode="nearest")
        t, n = normals(self.centre)
        toward = np.asarray(inside, np.float64) - self.centre
        toward /= np.maximum(np.hypot(toward[:, 0], toward[:, 1]), 1e-9)[:, None]
        self.owner_side = 1.0 if (toward * n).sum() > 0 else -1.0     # which way the owner is, along n
        by = self.width / 2.0 + PAST_INK
        curve = smooth(unfolded(self.centre - n * (self.owner_side * by)[:, None], self.centre, by), 1.0)
        before, after = self.run_on(cut, curve, True, self.enter), self.run_on(cut, curve, False, self.leave)
        self.body = (len(before), len(before) + len(curve))      # the cut itself, between its two tails
        self.curve = np.concatenate([before, curve, after])
        self.front_side = self.owner_side if self.ink == "front" else -self.owner_side
        return self

    @staticmethod
    def run_on(cut, curve, start, through=()):
        """The cut carried on past one end, through the points it is led out by and then straight:
        RUN_ON pixels where it meets another seam inside the figure, and otherwise on until it is
        off the figure (an end clicked a little short of the black still parts its pieces)."""
        k = min(6, len(curve) - 1)
        end, before = (curve[0], curve[k]) if start else (curve[-1], curve[-1 - k])
        tail = []
        for point in (through[::-1] if start else through):
            point = np.array(point, np.float64)
            steps = max(1, int(np.ceil(np.hypot(*(point - end)))))
            tail += [end + (point - end) * (step / steps) for step in range(1, steps + 1)]
            before, end = end, point
        along = (end - before) / max(np.hypot(*(end - before)), 1e-9)
        h, w = cut.figure.shape
        ex, ey = min(w - 1, max(0, int(round(end[0])))), min(h - 1, max(0, int(round(end[1]))))
        off = 0
        for step in range(1, (RUN_ON if cut.depth[ey, ex] > MEETS else RUN_ON_MOST) + 1):
            p = end + along * step
            x, y = int(round(p[0])), int(round(p[1]))
            inside = 0 <= x < w and 0 <= y < h and cut.figure[y, x]
            off = 0 if inside else off + 1
            tail.append(p)
            if step >= RUN_ON and off >= 2:
                break
        tail = np.array(tail)
        return tail[::-1] if start else tail

    def distance(self, shape, margin=8):
        """(box, signed distance, nearest part) from the cut over its own window: the distance is
        positive on the front piece's side; the part is the index of the nearest stretch of it."""
        h, w = shape
        c = self.curve
        x0, y0 = max(0, int(c[:, 0].min()) - margin), max(0, int(c[:, 1].min()) - margin)
        x1, y1 = min(w, int(c[:, 0].max()) + margin + 1), min(h, int(c[:, 1].max()) + margin + 1)
        yy, xx = np.mgrid[y0:y1, x0:x1].astype(np.float64)
        best = np.full(xx.shape, np.inf)
        sign = np.zeros(xx.shape)
        part = np.zeros(xx.shape, np.int32)
        for k, ((ax, ay), (bx, by)) in enumerate(zip(c[:-1], c[1:])):
            dx, dy = bx - ax, by - ay
            u = np.clip(((xx - ax) * dx + (yy - ay) * dy) / max(dx * dx + dy * dy, 1e-12), 0.0, 1.0)
            px, py = xx - (ax + u * dx), yy - (ay + u * dy)
            d = np.hypot(px, py)
            closer = d < best
            best[closer] = d[closer]
            sign[closer] = np.sign(-dy * px + dx * py)[closer]      # along the normal (-ty, tx)
            part[closer] = k
        return (x0, y0, x1, y1), best * sign * self.front_side, part


#: The pieces, back to front as they're drawn at rest (the skeleton's order).
DRAW = ("arm_far_up", "arm_far_low", "hand_far", "leg_far_up", "leg_far_low", "foot_far", "leg_near_up", "leg_near_low",
        "foot_near", "torso", "head", "arm_near_up", "arm_near_low", "hand_near")

#: Every base's seams: name, the piece in front, the pieces behind, the points clicked along the
#: drawn line (ends in the black beside the figure), and whose the line is where it isn't the
#: front piece's: at a knee and at the far elbow the joint's ball is on the piece behind.
SEAMS = {
    "base-male": (
        ("jaw", "head", ("torso",), [(459, 249), (476, 252), (488, 253, "c"), (500, 274), (528, 292), (545, 298), (551, 306)]),
        ("shoulder_near", "arm_near_up", ("torso",), [(386, 343.2, "o"), (392.5, 343.5, "o"), (405, 347), (412, 348), (438, 366), (446, 400), (440, 430), (432, 443, "c"),
                                                      (418, 452), (412, 475), (409, 492), (406.6, 499.6, "o"), (406.7, 504, "o")]),
        ("far", "torso", ("arm_far_up", "arm_far_low"), [(590, 343), (600, 360), (615, 372), (628, 400), (633, 430), (630, 455),
                                                         (624, 478), (618, 510), (613, 530), (611, 555), (612, 580), (615, 600),
                                                         (618, 620), (620, 640), (621, 648), (628, 660),
                                                     (631.5, 663.5, "o"), (633.5, 669.5, "o"), (636.5, 675, "o")], "front", True),
        ("hem_near", "torso", ("leg_near_up",), [(379, 772), (385, 771), (420, 768), (455, 771), (490, 784), (515, 798), (535, 809)]),
        ("hem_far", "torso", ("leg_far_up",), [(535, 809), (570, 806), (610, 798), (640, 785), (655, 773), (661, 770)]),
        ("thigh", "leg_near_up", ("leg_far_up",), [(535, 809), (533, 820), (531, 838), (527, 849), (526, 857)]),
        ("elbow_near", "arm_near_low", ("arm_near_up",), [(272, 574), (290, 560), (310, 557), (335, 568), (346, 585), (355, 596)]),
        ("wrist_near", "hand_near", ("arm_near_low",), [(254, 757), (280, 742), (297, 739), (318, 748), (327, 754)]),
        ("knee_near", "leg_near_low", ("leg_near_up",), [(368, 970), (385, 1000), (417, 1021), (440, 1000), (447, 980), (462, 992)], "back"),
        ("knee_far", "leg_far_low", ("leg_far_up",), [(552, 993), (570, 1015), (607, 1027), (637, 1020), (648, 1012)], "back"),
        ("ankle_near", "foot_near", ("leg_near_low",), [(277, 1344), (300, 1330), (320, 1327), (345, 1337), (352, 1340)]),
        ("ankle_far", "foot_far", ("leg_far_low",), [(558, 1314), (575, 1329), (600, 1312), (620, 1306), (627, 1303)]),
        ("elbow_far", "arm_far_low", ("arm_far_up",), [(612, 584), (617, 588), (628, 593), (640, 595), (655, 588), (662, 575), (668, 567)], "back"),
        ("wrist_far", "hand_far", ("arm_far_low",), [(686, 762), (697, 757), (715, 745), (730, 745), (742, 751)]),
    ),
    # Her far arm shows twice, above the bust and below it, so the torso is cut from it twice; and
    # her near knee's ball is on the shin, in front of the thigh.
    "base-female": (
        ("jaw", "head", ("torso",), [(474, 291), (482, 286, "c"), (490, 299), (500, 307), (510, 314), (520, 319), (530, 323),
                                     (535, 326), (536.6, 331, "o"), (537, 336, "o")]),
        ("shoulder_near", "arm_near_up", ("torso",), [(440, 368), (443, 375), (450, 378), (459, 387), (464, 400), (467, 412),
                                                      (467, 425), (466, 440), (462, 450), (456, 454, "c"), (451, 460),
                                                      (449, 466), (447, 476), (445, 483), (443.5, 489, "o"), (444, 499, "o")]),
        ("far_top", "torso", ("arm_far_up",), [(570, 380), (574, 388), (580, 395), (590, 405), (594, 415), (600, 427),
                                               (612, 437), (620, 444), (625, 448)]),
        ("far", "torso", ("arm_far_up", "arm_far_low"), [(631, 508), (622, 514), (611, 520, "c"), (609, 545), (601, 565),
                                                         (599, 582), (600, 600), (604, 617), (610, 635), (616, 650),
                                                         (620, 670), (622, 678), (624, 688), (625, 690),
                                                         (627.3, 691, "o"), (628.6, 692.4, "o"), (629.4, 694, "o"), (630.3, 695.8, "o"),
                                                         (631, 698, "o"), (632.2, 701.5, "o"), (634, 705, "o"), (636, 708.5, "o")],
         "front", True),
        ("hem_near", "torso", ("leg_near_up",), [(416, 722), (421, 718), (429, 708), (443, 700), (459, 697), (475, 697),
                                                 (487, 700), (499, 708), (511, 720), (521, 734), (529, 750), (534, 764),
                                                 (536, 780), (536, 789)]),
        ("hem_far", "torso", ("leg_far_up",), [(536, 789), (545, 787), (550, 785), (560, 780), (570, 770), (580, 760),
                                               (590, 750), (600, 738), (610, 724), (620, 712), (628, 704), (632, 704.5, "o"), (635, 708, "o")]),
        ("thigh", "leg_near_up", ("leg_far_up",), [(536, 789), (533, 802), (530, 816), (527, 828), (524, 838), (523, 846)]),
        ("elbow_near", "arm_near_low", ("arm_near_up",), [(334, 581), (340, 577), (350, 573), (360, 571), (368, 574), (376, 581),
                                                          (382, 591), (385, 598), (390, 602)]),
        ("wrist_near", "hand_near", ("arm_near_low",), [(287, 765), (292, 762), (300, 756), (309, 754), (319, 754), (327, 757),
                                                        (334, 762), (339, 768)]),
        ("knee_near", "leg_near_low", ("leg_near_up",), [(374, 997.9, "o"), (384, 1002), (386, 1007, "c"), (395, 998), (405, 991), (417, 989),
                                                         (429, 992), (438, 1000), (441, 1010), (440, 1018), (447.5, 1023.5, "o")]),
        ("knee_far", "leg_far_low", ("leg_far_up",), [(553, 1002), (559, 1006), (564, 1013), (572, 1020), (580, 1025),
                                                      (590, 1029), (600, 1031), (610, 1031), (620, 1028), (626, 1025),
                                                      (633, 1021)], "back"),
        ("ankle_near", "foot_near", ("leg_near_low",), [(297, 1344), (302, 1341), (307, 1336), (315, 1331), (325, 1328),
                                                        (335, 1328), (343, 1331), (349, 1336), (352, 1340), (357, 1343)]),
        ("ankle_far", "foot_far", ("leg_far_low",), [(575, 1324), (580, 1326), (584, 1333), (590, 1336), (598, 1333),
                                                     (604, 1326), (612, 1320), (620, 1318), (629, 1317), (634, 1318)]),
        ("elbow_far", "arm_far_low", ("arm_far_up",), [(602, 601), (605, 607), (609, 611), (617, 616), (627, 616), (636, 610),
                                                       (640, 603), (642, 596), (647, 594)], "back"),
        ("wrist_far", "hand_far", ("arm_far_low",), [(675, 777), (682, 776), (688, 775), (695, 765), (704, 760), (718, 760),
                                                     (724, 759)]),
    ),
}

#: A point well inside each piece that isn't found from its bone (the bone's middle).
SEEDS = {
    "base-male": {"head": (540, 220), "torso": (520, 560)},
    "base-female": {"head": (520, 200), "torso": (520, 560), "arm_far_up": (625, 560)},
}
#: Where else a piece shows, apart from where its seed is: her far shoulder, above the bust.
ALSO = {
    "base-female": {"arm_far_up": [(606, 415)]},
}


def seeds(base):
    import skeleton as K
    out = {}
    for bone in DRAW:
        if bone in SEEDS[base]:
            out[bone] = SEEDS[base][bone]
        else:
            (x0, y0), (x1, y1) = K.bone_line(base, bone)
            out[bone] = (int(round((x0 + x1) / 2)), int(round((y0 + y1) / 2)))
    return out


class Cutting:
    """A figure cut: `seams` (each traced), `piece` (which piece every pixel of the figure is, an
    index into DRAW, -1 off it), and from them the pieces themselves (`pieces()`)."""

    def __init__(self, base, rgb):
        import import_parts as ip
        self.base = base
        self.colour, self.alpha = ip.key_black(rgb)
        self.figure = self.alpha > 0
        self.inner = ndi.distance_transform_edt(self.alpha > 0.5) > 3
        self.depth = ndi.distance_transform_edt(self.figure)
        self.dark, _ = ridge(self.colour)
        self.ink = inkness(self.colour, self.alpha) * self.inner
        self.cost = np.where(self.inner, STEP_INK + (STEP_SKIN - STEP_INK) * (1.0 - self.ink) ** 2, STEP_OUT)
        self.edge = edginess(self.colour, self.alpha) * self.inner
        gy, gx = np.gradient(ndi.gaussian_filter(luma(self.colour), 1.0))
        self.steep = np.hypot(gx, gy) * self.inner
        self.seeds = seeds(base)
        self.seams = {}
        for spec in SEAMS[base]:
            seam = Seam(*spec)
            self.seams[seam.name] = seam.trace(self, self.seeds[seam.owner()])
        self.part()

    def part(self):
        h, w = self.alpha.shape
        barrier = np.zeros((h, w), bool)
        self.sd = {}
        for name, seam in self.seams.items():
            box, sd, part = seam.distance((h, w))
            self.sd[name] = (box, sd, part)
            x0, y0, x1, y1 = box
            barrier[y0:y1, x0:x1] |= np.abs(sd) <= 0.71
        lab, _ = ndi.label(self.figure & ~barrier)
        first = np.full((h, w), -1, np.int16)
        owner, self.problems = {}, []
        for k, bone in enumerate(DRAW):
            x, y = self.seeds[bone]
            if lab[y, x] == 0:
                self.problems.append("the seed of %s (%d, %d) isn't on the figure" % (bone, x, y))
                continue
            if lab[y, x] in owner:
                self.problems.append("no cut parts %s from %s" % (bone, owner[lab[y, x]]))
                continue
            owner[lab[y, x]] = bone
            first[lab == lab[y, x]] = k
        for bone, points in ALSO.get(self.base, {}).items():
            for x, y in points:
                if lab[y, x] == 0 or lab[y, x] in owner:
                    self.problems.append("%s isn't cut off at (%d, %d): that's %s" % (bone, x, y, owner.get(lab[y, x], "no piece")))
                    continue
                owner[lab[y, x]] = bone
                first[lab == lab[y, x]] = DRAW.index(bone)
        # A pixel the cut runs through is its side's: whatever piece is nearest it on that side.
        piece = first.copy()
        for name in self.seams:
            (x0, y0, x1, y1), sd, _ = self.sd[name]
            was, now = first[y0:y1, x0:x1], piece[y0:y1, x0:x1]
            band = (np.abs(sd) <= 0.71) & self.figure[y0:y1, x0:x1] & (now < 0)
            for side in (sd > 0, sd <= 0):
                known = (was >= 0) & side
                if known.any() and (band & side).any():
                    iy, ix = ndi.distance_transform_edt(~known, return_distances=False, return_indices=True)
                    sel = band & side
                    now[sel] = was[iy[sel], ix[sel]]
        left = self.figure & (piece < 0)
        self.stray = int(left.sum())
        self.largest_stray = 0
        if left.any():
            sizes = np.bincount(lab[left])
            self.largest_stray = int(sizes[1:].max()) if len(sizes) > 1 else 0
            iy, ix = ndi.distance_transform_edt(piece < 0, return_distances=False, return_indices=True)
            piece[left] = piece[iy[left], ix[left]]
        # A pixel right by a cut is on its own side of it, whatever another cut that ends there
        # made of it: the piece in front on one side, the piece behind on the other.
        self.sided = 0
        for name, seam in self.seams.items():
            (x0, y0, x1, y1), sd, part = self.sd[name]
            now = piece[y0:y1, x0:x1]
            by = (part >= seam.body[0]) & (part < seam.body[1] - 1) & self.figure[y0:y1, x0:x1] & (np.abs(sd) < 2.0)
            front, backs = DRAW.index(seam.front), [DRAW.index(b) for b in seam.back]
            wrong = by & (sd > 0) & np.isin(now, backs)
            now[wrong] = front
            self.sided += int(wrong.sum())
            wrong, right = by & (sd <= 0) & (now == front), np.isin(now, backs) & (sd <= 0)
            if wrong.any() and right.any():
                iy, ix = ndi.distance_transform_edt(~right, return_distances=False, return_indices=True)
                now[wrong] = now[iy[wrong], ix[wrong]]
                self.sided += int(wrong.sum())
        self.piece = piece


#: Where a piece goes on under the pieces in front of it, so that turning either never opens a
#: hole: ("capsule", (x0, y0), r0, (x1, y1), r1), ("disc", (x, y), r), or ("ball", seam, inset), a
#: disc about that seam's joint that stops `inset` short of the seam (it stays behind the joint's
#: ball however far that turns). Only what the pieces in front hide at rest is kept; it is filled
#: with the piece's own skin and given the piece's own outline.
EXTEND = {
    "base-male": {
        "torso": [("capsule", (506, 318), 37, (504, 262), 33), ("capsule", (425, 380), 38, (428, 470), 24)],
        "arm_far_up": [("capsule", (622, 395), 42, (632, 560), 30)],
        "arm_far_low": [("capsule", (630, 600), 26, (660, 660), 26), ("ball", "wrist_far", 0.5)],
        "leg_near_up": [("capsule", (462, 735), 70, (455, 800), 76)],
        "leg_far_up": [("capsule", (590, 745), 68, (585, 880), 62)],
        "arm_near_up": [("ball", "elbow_near", 0.5)],
        "arm_near_low": [("ball", "wrist_near", 0.5)],
        "leg_near_low": [("ball", "ankle_near", 0.5)],
        "leg_far_low": [("ball", "ankle_far", 0.5)],
    },
    "base-female": {
        "torso": [("capsule", (506, 340), 28, (507, 290), 26), ("capsule", (448, 400), 30, (440, 470), 18)],
        "arm_far_up": [("capsule", (606, 410), 30, (626, 560), 24)],
        "arm_far_low": [("capsule", (626, 606), 22, (660, 700), 28), ("ball", "wrist_far", 0.5)],
        "leg_near_up": [("capsule", (472, 720), 52, (470, 790), 64), ("ball", "knee_near", 0.5)],
        "leg_far_up": [("capsule", (588, 725), 56, (582, 860), 60)],
        "arm_near_up": [("ball", "elbow_near", 0.5)],
        "arm_near_low": [("ball", "wrist_near", 0.5)],
        "leg_near_low": [("ball", "ankle_near", 0.5)],
        "leg_far_low": [("ball", "ankle_far", 0.5)],
    },
}
#: A piece behind a cut goes on at least this far under the piece in front (its skin, then its own
#: outline), the outline it is given is this wide, and a line's soft edge is this deep.
BAND, OUTLINE_WIDE, EDGE = 4.5, 2.5, 1.3
#: A pixel a piece covers less of than this is left whole to the piece under it.
LEAST = 0.02
#: The last faint trace of a line reaches this far past its cut: there the piece behind is painted
#: with its own skin, and the trace goes with the line.
TRACE = 3.0
#: What a piece goes on under others by is never a thorn: nowhere narrower than this across...
THORN = 9
#: A blot where lines join is shorter than half this along the line.
JOINED = 25
#: ...and it is joined to the piece: a gap between the two narrower than twice this is filled.
BRIDGE = 9
#: A picture has a pale ring along its lines. Past a cut, skin lighter than the skin beside it by
#: less than the first of these (0-255) is that ring, and is painted out; lighter by more than the
#: second, it is the piece behind's own light, and is left as it is.
HALO = (12.0, 36.0)
#: Nearer the figure's edge than this a cut is among the outlines of the pieces it parts, and what
#: is beside it there is theirs, not a trace of its own line.
SHALLOW = 2.0
#: A figure's outline is no deeper than this from its edge.
RIM = 5.0
#: What a piece is painted with beside a cut is one of its own pixels past the cut's reach: the
#: one most like what is there, of those no more than this further off than the nearest.
LOOK = 2.5
#: Two colours nearer than this in a channel say nothing of how much of each a pixel is.
APART = 24.0
OUTLINE = (52, 30, 20)


def shape_cover(shape, cut):
    """How much of each pixel one of EXTEND's shapes covers (0 to 1), on a Cutting's canvas."""
    import skeleton as K
    h, w = cut.alpha.shape
    yy, xx = np.mgrid[0:h, 0:w].astype(np.float32)
    if shape[0] == "ball":
        # About the joint, and no further than the piece in front reaches from it, so that piece
        # hides it whichever way it turns.
        x, y = K.JOINTS[cut.base][shape[1]]
        reach = ndi.distance_transform_edt(cut.piece == DRAW.index(cut.seams[shape[1]].front))
        r = reach[int(round(y)), int(round(x))] - EDGE - shape[2]
        d = np.hypot(xx - x, yy - y) - r
    elif shape[0] == "disc":
        d = np.hypot(xx - shape[1][0], yy - shape[1][1]) - shape[2]
    else:
        (x0, y0), r0, (x1, y1), r1 = shape[1:]
        dx, dy = x1 - x0, y1 - y0
        t = np.clip(((xx - x0) * dx + (yy - y0) * dy) / (dx * dx + dy * dy), 0.0, 1.0)
        d = np.hypot(xx - (x0 + t * dx), yy - (y0 + t * dy)) - (r0 + t * (r1 - r0))
    return np.clip(0.5 - d, 0.0, 1.0)


def nearest(mask):
    """For every pixel, the (rows, columns) of the nearest pixel of a mask."""
    return ndi.distance_transform_edt(~mask, return_distances=False, return_indices=True)


def likest(colour, these, source):
    """For each pixel of `these` (a mask), the colour of the pixel of `source` (a mask) most like
    it, of those no more than LOOK further from it than the nearest."""
    h, w = these.shape
    ys, xs = np.nonzero(these)
    iy, ix = nearest(source)
    ny, nx = iy[ys, xs], ix[ys, xs]
    reach = np.minimum(np.hypot(ny - ys, nx - xs) + LOOK, 12.0)
    own = colour[ys, xs]
    best = colour[ny, nx].copy()
    least = np.abs(best - own).max(axis=-1)
    r = int(np.ceil(reach.max()))
    for dy in range(-r, r + 1):
        for dx in range(-r, r + 1):
            cy, cx = ys + dy, xs + dx
            ok = (np.hypot(dx, dy) <= reach) & (cy >= 0) & (cy < h) & (cx >= 0) & (cx < w)
            cy, cx = np.clip(cy, 0, h - 1), np.clip(cx, 0, w - 1)
            there = colour[cy, cx]
            off = np.abs(there - own).max(axis=-1)
            better = ok & source[cy, cx] & (off < least)
            best[better], least[better] = there[better], off[better]
    return best


def least_cover(colour, under, lo, hi, apart=APART):
    """The least a piece can cover a pixel that is `colour` where what's under it is `under`, if
    the piece's own colour there is one found near it: between `lo` and `hi`, channel by channel."""
    up = np.where(hi - under > apart, (colour - under) / np.maximum(hi - under, apart), 0.0)
    down = np.where(under - lo > apart, (under - colour) / np.maximum(under - lo, apart), 0.0)
    return np.clip(np.maximum(up, down).max(axis=-1), 0.0, 1.0)


def cover(colour, under, lo, hi, least):
    """How much of a pixel a piece covers at its soft edge: what its colours say (least_cover), no
    less than `least` (how far inside the cut it is), and enough for its own colour to be a colour
    at all (0 to 255), so that laid back over what's under it, it is the picture exactly."""
    return np.maximum(np.maximum(least_cover(colour, under, lo, hi), least_cover(colour, under, 0.0, 255.0, 1e-3)), least)


def unmixed(colour, a, under):
    """The colour that, laid `a` thick over `under`, gives `colour`."""
    return np.clip((colour - (1.0 - a)[:, None] * under) / a[:, None], 0.0, 255.0)


def cut_pieces(self):
    """The pieces: {bone: (colour 0-255 float32 (h, w, 3), alpha 0-1 float32 (h, w))}, full canvas.
    Drawn back to front where they are (DRAW), they are the picture again (`check`)."""
    h, w = self.alpha.shape
    orig, alpha0 = self.colour.astype(np.float32), self.alpha.astype(np.float32)
    solid = alpha0 >= 0.999
    deep = np.clip(ndi.distance_transform_edt(solid) - 1.0, 0.0, 1.0).astype(np.float32)
    index = {b: k for k, b in enumerate(DRAW)}
    near_cut = np.zeros((h, w), bool)
    for (x0, y0, x1, y1), sd, _ in self.sd.values():
        near_cut[y0:y1, x0:x1] |= np.abs(sd) < TRACE
    clean = {b: (self.piece == k) & solid & (self.ink < 0.1) & self.inner & ~near_cut for b, k in index.items()}
    lo = ndi.minimum_filter(np.where(self.figure[:, :, None], orig, 255.0), size=(5, 5, 1))
    hi = ndi.maximum_filter(np.where(self.figure[:, :, None], orig, 0.0), size=(5, 5, 1))
    lo_wide = ndi.minimum_filter(lo, size=(5, 5, 1))        # the line's own dark, from as far off as its trace
    alpha = {b: np.where(self.piece == k, alpha0, 0.0).astype(np.float32) for b, k in index.items()}
    colour = {b: orig for b in DRAW}                 # copied the first time a piece's is changed

    def own(bone):
        if colour[bone] is orig:
            colour[bone] = orig.copy()
        return colour[bone]

    # 1. Each piece behind goes on under the pieces in front: its shapes, and a band along each cut.
    more = {b: np.zeros((h, w), np.float32) for b in DRAW}
    traced = {b: np.zeros((h, w), bool) for b in DRAW}       # a piece's own pixels with a trace of a line in front
    covered = {}                                             # where the pieces in front hide a piece at rest
    for bone in DRAW:
        hidden = np.isin(self.piece, [k for k in index.values() if k > index[bone]]) & solid
        mine = (self.piece == index[bone]) & solid       # no gap between what is hidden and the piece itself
        inside = np.clip(ndi.distance_transform_edt(hidden | mine) - 1.0, 0.0, 1.0) * hidden
        covered[bone] = inside > 0.5
        for shape in EXTEND[self.base].get(bone, ()):
            more[bone] = np.maximum(more[bone], shape_cover(shape, self) * inside).astype(np.float32)
    under = {}
    for name, seam in self.seams.items():
        if seam.ink != "front":
            continue
        (x0, y0, x1, y1), sd, part = self.sd[name]
        first, last = seam.body
        body = (part >= first) & (part < last - 1)
        sub = self.piece[y0:y1, x0:x1]
        band = (sub == index[seam.front]) & body & (sd < BAND + 0.5)
        iy, ix = nearest(np.isin(sub, [index[b] for b in seam.back]))
        whose = np.where(band, sub[iy, ix], -1)
        beside = ndi.distance_transform_edt(sub != index[seam.front]) <= TRACE + 1.5
        under[name] = whose
        for b in seam.back:
            m = whose == index[b]
            there = more[b][y0:y1, x0:x1]
            there[m] = np.maximum(there[m], np.clip(BAND + 0.5 - sd[m], 0.0, 1.0) * deep[y0:y1, x0:x1][m])
            traced[b][y0:y1, x0:x1] |= ((sub == index[b]) & body & beside & solid[y0:y1, x0:x1]
                                        & (sd < 0) & (sd > -TRACE) & (self.depth[y0:y1, x0:x1] > SHALLOW))
    for bone in DRAW:
        mine = self.piece == index[bone]
        ext = more[bone] * ~mine
        if traced[bone].any():
            # Up to the cut a piece is what it is just past it, its own lines and all: each pixel
            # takes the one of the piece's own there that is most like it, so only what nothing of
            # the piece's own accounts for is left to be the line's. Its skin is never painted with
            # its outline: a strip too narrow to reach past the cut takes its skin from along it.
            past = mine & solid & ~near_cut
            for these, source in ((traced[bone] & (self.depth <= RIM), past),
                                  (traced[bone] & (self.depth > RIM), past & (self.depth > RIM))):
                if these.any() and source.any():
                    own(bone)[these] = likest(orig, these, source)
        if not ext.any():
            continue
        yy, xx = np.mgrid[-BRIDGE:BRIDGE + 1, -BRIDGE:BRIDGE + 1]
        joined = ndi.binary_closing(mine | (ext > 0.5), np.hypot(xx, yy) <= BRIDGE) & covered[bone]
        ext = np.maximum(ext, joined.astype(np.float32))
        yy, xx = np.mgrid[0:THORN, 0:THORN] - THORN // 2
        opened = ndi.binary_opening(mine | (ext > 0.5), np.hypot(xx, yy) <= THORN / 2.0)
        ext = np.minimum(ext, np.clip(ndi.gaussian_filter(opened.astype(np.float32), 0.8) * 2.0 - 0.5, 0.0, 1.0))
        on = ext > 0
        iy, ix = nearest(clean[bone])
        fill = np.zeros((h, w, 3), np.float32)
        fill[on] = orig[iy[on], ix[on]]
        fill[clean[bone]] = orig[clean[bone]]
        known = (on | clean[bone]).astype(np.float32)
        soft = ndi.gaussian_filter(fill, (3.0, 3.0, 0)) / np.maximum(ndi.gaussian_filter(known, 3.0), 1e-4)[:, :, None]
        depth = ndi.distance_transform_edt(np.maximum(alpha[bone], ext) > 0.5)
        line = (np.clip(OUTLINE_WIDE + 0.5 - depth, 0.0, 1.0) * (ext > 0))[:, :, None]
        inked = mine & (self.ink > 0.85) & solid
        ink = np.median(orig[inked], axis=0) if inked.sum() > 50 else np.array(OUTLINE, np.float32)
        own(bone)[on] = (soft * (1.0 - line) + ink * line)[on]
        alpha[bone] = np.maximum(alpha[bone], ext)

    # 2. A line's own edge: the piece that owns it keeps its ink, thinning to nothing at the cut.
    taken = np.zeros((h, w), bool)                           # a pixel is one line's edge, never two
    self.retouched = np.zeros((h, w), bool)                  # where the pale ring by a line was painted out
    for name, seam in self.seams.items():
        (x0, y0, x1, y1), sd, part = self.sd[name]
        first, last = seam.body
        win = (slice(y0, y1), slice(x0, x1))
        body = (part >= first) & (part < last - 1) & solid[win]
        sub, o = self.piece[win], orig[win]
        if seam.ink == "front":
            for b in seam.back:
                inside = ((sub == index[seam.front]) & (sd >= 0) & (sd < EDGE) & (under[name] == index[b])
                          & (alpha[b][win] >= 0.999))
                # Where a third piece lies between the two (three meet), the pixel stays whole.
                between = np.zeros(sub.shape, bool)
                for other in DRAW[index[b] + 1:index[seam.front]]:
                    between |= alpha[other][win] > 0
                edge = (inside | traced[b][win]) & body & ~taken[win] & ~between
                if not edge.any():
                    continue
                was, below, mine = o[edge], colour[b][win][edge], inside[edge]
                dark, light = np.where(mine[:, None], lo[win][edge], lo_wide[win][edge]), hi[win][edge]
                # Past the cut, what is darker than the skin there is the last of the line and goes
                # with it, what is much lighter is the piece behind's own light and stays on it, and
                # the pale ring between the two is painted out.
                glow = np.clip(((was - below).max(axis=-1) - HALO[0]) / (HALO[1] - HALO[0]), 0.0, 1.0)[:, None]
                # ...but where no line is drawn (a bare edge) there is no trace or ring past the cut:
                # only the soft edge of the piece in front, where the colours say it is there.
                edgeless = (seam.width[np.clip(part - first, 0, len(seam.width) - 1)] < 0.5)[edge] & ~mine
                mine = mine | (edgeless & (sd[edge] > -EDGE) & (least_cover(was, below, dark, light) >= LEAST))
                seen = np.where((mine | edgeless)[:, None], was, np.minimum(was, below) + np.maximum(was - below, 0.0) * glow)
                a = cover(seen, below, dark, light, np.clip(sd[edge] - 0.3, 0.0, 1.0))
                self.retouched[win][edge] = np.abs(was - seen).max(axis=-1) > 0.5
                keep = (a >= LEAST) & (mine | ((glow[:, 0] <= 0.0) & ~edgeless))
                ys, xs = np.nonzero(edge)
                # ...but only what is joined to that piece: a speck apart stays where it is.
                whole = sub == index[seam.front]
                grown = whole.copy()
                grown[ys[keep], xs[keep]] = True
                lab, _ = ndi.label(grown, np.ones((3, 3), int))
                keep &= np.isin(lab, np.unique(lab[whole]))[ys, xs]
                # A trace thins away from the cut: full for a third of its reach, nothing at the end.
                # What is darker than that allows is the piece behind's own shading, and stays on it.
                line = unmixed(seen, np.maximum(a, LEAST), below)
                thin = np.where(mine, a, np.minimum(a, np.clip(1.5 * (1.0 + sd[edge] / TRACE), 0.0, 1.0)))
                keep &= thin >= LEAST
                less = keep & (thin < a)
                cf, cb, af = own(seam.front)[win], own(b)[win], alpha[seam.front][win]
                cf[ys[keep], xs[keep]] = line[keep]
                af[ys, xs] = np.where(keep, thin, 0.0)
                cb[ys[less], xs[less]] = ((seen[less] - thin[less][:, None] * line[less])
                                          / (1.0 - thin[less])[:, None])
                cb[ys[~keep], xs[~keep]] = seen[~keep]
                taken[win] |= edge
        else:
            # The line is the piece behind's (a joint's ball): the piece in front lies over its outer
            # edge, and thins away over the line itself, so both edges are soft and nothing shows.
            b = seam.back[0]
            beside = (ndi.distance_transform_edt(sub != index[seam.front]) <= EDGE + 2.0) & ~taken[win]
            edge = (sub == index[b]) & body & (sd < 0) & (sd > -EDGE) & beside
            fringe = (sub == index[b]) & body & (sd <= -EDGE) & (sd > -EDGE - 1.0) & beside
            if not edge.any():
                continue
            iy, ix = nearest(clean[seam.front][win])
            was, skin = o[edge], o[iy[edge], ix[edge]]
            a = cover(was, skin, lo[win][edge], hi[win][edge], np.clip(-sd[edge] - 0.3, 0.0, 1.0))
            keep = a >= LEAST
            ys, xs = np.nonzero(edge)
            cf, cb = own(seam.front)[win], own(b)[win]
            af, ab = alpha[seam.front][win], alpha[b][win]
            cb[ys[keep], xs[keep]] = unmixed(was[keep], a[keep], skin[keep])
            ab[ys, xs] = np.where(keep, a, 0.0)
            cf[edge], af[edge] = was, 1.0
            cf[fringe], af[fringe] = o[fringe], np.clip(sd[fringe] + EDGE + 1.0, 0.0, 1.0)
            taken[win] |= edge | fringe
    return {b: (colour[b], alpha[b]) for b in DRAW}


def assembled(pieces):
    """The pieces drawn back to front where they are: (colour times alpha, alpha)."""
    first = pieces[DRAW[0]][1]
    rgb, a = np.zeros(first.shape + (3,), np.float32), np.zeros(first.shape, np.float32)
    for bone in DRAW:
        c, pa = pieces[bone]
        rgb = c * pa[:, :, None] + rgb * (1.0 - pa[:, :, None])
        a = pa + a * (1.0 - pa)
    return rgb, a


def check(self, pieces):
    """How far the pieces, put back where they were, are from the picture: the worst pixel's colour
    (0-255) and alpha (0-1) everywhere but in the pale ring painted out along the cuts, then how many
    pixels that ring was and the most one of them was changed (0-255)."""
    rgb, a = assembled(pieces)
    off = np.abs(rgb - self.colour * self.alpha[:, :, None]).max(axis=2)
    ring = self.retouched
    return (float(off[~ring].max()), float(np.abs(a - self.alpha).max()), int(ring.sum()),
            float(off[ring].max()) if ring.any() else 0.0)


Cutting.pieces, Cutting.check = cut_pieces, check


def main(argv):
    import import_character as ic
    if len(argv) != 3:
        print(__doc__)
        return 2
    src, base, folder = argv[0], argv[1], os.path.abspath(argv[2])
    cut = Cutting(base, ic.read_png(src)[0][:, :, :3])
    for problem in cut.problems:
        print("%s: %s" % (base, problem))
    if cut.problems:
        return 1
    pieces = cut.pieces()
    os.makedirs(folder, exist_ok=True)
    for bone, (colour, alpha) in pieces.items():
        rgba = np.dstack([np.clip(colour, 0, 255) * (alpha[:, :, None] > 0), alpha * 255.0])
        ic.write_png(os.path.join(folder, bone + ".png"), (rgba + 0.5).astype(np.uint8))
    print("%s: %d pieces in %s; put back together they are within %.2f of the picture (alpha %.3f), but "
          "for the pale ring painted out along the cuts (%d pixels, by at most %.1f)"
          % ((base, len(pieces), folder) + cut.check(pieces)))
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
