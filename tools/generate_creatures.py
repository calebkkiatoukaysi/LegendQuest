"""
The creatures: the sea-wraiths that come over the Gull's Wager's rail, the gloomfangs and the
Thornmother out of the Mirewood, the Hollowed that come out of Doomward (footmen, hexers and
sappers), and the constructs that guard the Old Crown Vault.

    python tools/generate_creatures.py [creature ...]

With names (gloomfang, say), only those sheets are written.

Every sheet is the walkers' layout, only bigger frames for the bigger things: ten columns
(standing, four steps, then the wind-up, the strike, the cast, the hurt and the downed frames the
fights will use) and a row for each way it faces (down, left, right, up). Its feet are at the
bottom middle of a frame, on the row two up from the bottom, the same as the people.

Nothing's cut from a picture. Each creature is built out of round parts the way you'd rough it
out on paper (balls for a chest and a skull, tapering tubes for legs, a neck and a tail, flat
plates for armour and crystal), posed for the frame, and shaded like the rest of the game: lit
from the top left, a few shades to a colour, a dark seam where a near part crosses a far one, and
a dark outline all round. Glows and smoke go on after the outline, so they're soft against
whatever the creature's standing on. Facing right is facing left in a mirror, lit from the same
side.

Only needs CPython. Every bit of noise is hash01 on where it is, so a rerun writes the same bytes.
"""

import math
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from pixels import OUT, hash01, lerp_color, new_image, put, write_png  # noqa: E402

DOWN, LEFT, RIGHT, UP = range(4)

#: Standing, four steps, wind-up, strike, cast, hurt and downed.
COLUMNS = 10
STAND, WIND_UP, STRIKE, CAST, HURT, DOWNED = 0, 5, 6, 7, 8, 9

#: How many rows under the soles a frame has: the soles' outline is on the row above these, and
#: the point the game draws a walker from is the row under the soles, as it is for the people.
UNDER_FEET = 2

#: How opaque the parts of a see-through creature that aren't see-through are.
SOLID_ALPHA = 240

#: Where the light comes from: up and to the left, and a good way towards you (made a unit long).
LIGHT = tuple(v / math.sqrt(0.45 ** 2 + 0.6 ** 2 + 0.66 ** 2) for v in (-0.45, -0.6, 0.66))


# --------------------------------------------------------------------------- #
# Drawing a frame
# --------------------------------------------------------------------------- #

def level(light, count):
    """Which of `count` shades, dark to light, a surface turned `light` towards the light gets.
    Facing you square on is the middle-to-light shade, so the lightest is a highlight up on the
    top left and the darkest a crescent down the bottom right."""
    t = (light + 0.45) / 1.5
    return max(0, min(count - 1, int(t * count)))


class Figure:
    """One frame being drawn: what colour each pixel is and which part put it there, and the glows
    and smoke that go on over the outline. Everything's drawn facing left (or towards you, or away);
    a figure made with `flip` turns it into facing right as it's laid down."""

    def __init__(self, w, h, flip=False, seed=0):
        self.w, self.h, self.flip, self.seed = w, h, flip, seed
        self.shift = 0
        self.colour = {}
        self.owner = {}
        self.parts = 0
        self.over = []
        # How see-through it is: none of it, unless a creature says how opaque each row is. The
        # solid pixels (a wraith's kelp, its eyes) stay nearly opaque whatever.
        self.opacity = None
        self.solid = set()

    @property
    def ground(self):
        """The lowest row anything's drawn on: the outline under the soles goes on the row below."""
        return self.h - UNDER_FEET - 2

    def at(self, x, y):
        """Where a drawn pixel lands: moved across by `shift` (for a long creature side on, to keep
        it off the frame's edges), and mirrored if this frame faces right."""
        x += self.shift
        return (self.w - 1 - x if self.flip else x, y)

    def lay(self, shaped, ramp, bias=0.0, seam=True, rough=0.0):
        """Lays a part down: `shaped` is each pixel's surface (which way it faces, as x, y and
        towards you), shaded off the ramp. A seam goes down the part's sides and bottom wherever
        they're over a part laid before it."""
        self.parts += 1
        mine = []
        for (x, y), (nx, ny, nz) in shaped.items():
            p = self.at(x, y)
            if self.flip:
                nx = -nx
            light = nx * LIGHT[0] + ny * LIGHT[1] + nz * LIGHT[2] + bias
            if rough:
                light += (hash01(p[0], p[1] // 2, self.seed + self.parts) - 0.5) * rough
            self.colour[p] = ramp[level(light, len(ramp))]
            self.owner[p] = self.parts
            mine.append(p)
        if seam:
            self.seam(mine, ramp[0] if seam is True else seam)
        return mine

    def seam(self, pixels, colour):
        """Darkens a part's edge pixels that sit to the side of or above an older part."""
        part = self.parts
        for x, y in pixels:
            if self.owner.get((x, y)) != part:
                continue
            for n in ((x - 1, y), (x + 1, y), (x, y + 1)):
                if n in self.owner and self.owner[n] < part:
                    self.colour[(x, y)] = colour
                    break

    def ball(self, cx, cy, rx, ry, ramp, **kw):
        """An ellipse, shaded as the front half of a ball."""
        shaped = {}
        for y in range(math.floor(cy - ry) - 1, math.ceil(cy + ry) + 1):
            for x in range(math.floor(cx - rx) - 1, math.ceil(cx + rx) + 1):
                nx, ny = (x + 0.5 - cx) / rx, (y + 0.5 - cy) / ry
                d = nx * nx + ny * ny
                if d <= 1.0:
                    shaped[(x, y)] = (nx, ny, math.sqrt(1.0 - d))
        return self.lay(shaped, ramp, **kw)

    def limb(self, points, radii, ramp, **kw):
        """A tube down a line of points, as thick as `radii` at each (tapering between them),
        rounded at both ends and shaded round like an arm."""
        xs = [p[0] for p in points]
        ys = [p[1] for p in points]
        big = max(radii) + 1
        shaped = {}
        for y in range(math.floor(min(ys) - big), math.ceil(max(ys) + big)):
            for x in range(math.floor(min(xs) - big), math.ceil(max(xs) + big)):
                px, py = x + 0.5, y + 0.5
                best = None
                for i in range(len(points) - 1):
                    (ax, ay), (bx, by) = points[i], points[i + 1]
                    dx, dy = bx - ax, by - ay
                    length = dx * dx + dy * dy
                    t = 0.0 if length == 0 else max(0.0, min(1.0, ((px - ax) * dx + (py - ay) * dy) / length))
                    qx, qy = ax + dx * t, ay + dy * t
                    r = radii[i] + (radii[i + 1] - radii[i]) * t
                    d = math.hypot(px - qx, py - qy)
                    if d <= r and (best is None or d / r < best[0]):
                        best = (d / r, (px - qx) / r, (py - qy) / r)
                if best:
                    _, nx, ny = best
                    shaped[(x, y)] = (nx, ny, math.sqrt(max(0.0, 1.0 - nx * nx - ny * ny)))
        return self.lay(shaped, ramp, **kw)

    def plate(self, points, ramp, facing=(0.0, 0.0, 1.0), **kw):
        """A flat polygon, all one shade: the shade a flat face turned `facing` gets."""
        shaped = {}
        xs = [p[0] for p in points]
        ys = [p[1] for p in points]
        for y in range(math.floor(min(ys)), math.ceil(max(ys)) + 1):
            for x in range(math.floor(min(xs)), math.ceil(max(xs)) + 1):
                if inside(points, x + 0.5, y + 0.5):
                    shaped[(x, y)] = facing
        return self.lay(shaped, ramp, **kw)

    def block(self, x0, y0, x1, y1, ramp, bevel=1, **kw):
        """A box with bevelled edges, like a dressed stone: its top and left edges catch the light,
        its right and bottom ones are in shadow, and its face is flat between."""
        shaped = {}
        for y in range(math.floor(y0), math.ceil(y1)):
            for x in range(math.floor(x0), math.ceil(x1)):
                if (x < x0 + bevel or x >= x1 - bevel) and (y < y0 + bevel or y >= y1 - bevel):
                    continue
                if y < y0 + bevel:
                    n = (0.0, -0.8, 0.6)
                elif x < x0 + bevel:
                    n = (-0.8, -0.1, 0.6)
                elif x >= x1 - bevel:
                    n = (0.85, 0.1, 0.5)
                elif y >= y1 - bevel:
                    n = (0.1, 0.9, 0.4)
                else:
                    n = (-0.05, -0.1, 0.99)
                shaped[(x, y)] = n
        return self.lay(shaped, ramp, **kw)

    def dot(self, x, y, rgb):
        """One pixel, a colour of its own, counted as part of the last part laid."""
        p = self.at(round(x - 0.5) if isinstance(x, float) else x, y)
        self.colour[p] = rgb
        self.owner.setdefault(p, self.parts)

    def line(self, x0, y0, x1, y1, rgb):
        """A one-pixel line, end to end."""
        steps = max(abs(x1 - x0), abs(y1 - y0), 1)
        for i in range(int(math.ceil(steps)) + 1):
            t = i / math.ceil(steps)
            self.dot(int(math.floor(x0 + (x1 - x0) * t)), int(math.floor(y0 + (y1 - y0) * t)), rgb)

    def glow(self, x, y, rgba):
        """A pixel that goes on after the outline: a glow, a wisp of smoke, a spark."""
        self.over.append((self.at(x, y), rgba))

    def has(self, x, y):
        """Whether anything's drawn at a pixel (in the frame's own, unmirrored, terms)."""
        return self.at(x, y) in self.colour

    def top_of(self, x):
        """The highest drawn pixel in a column, or None."""
        ys = [y for (px, y) in self.colour if px == self.at(x, 0)[0]]
        return min(ys) if ys else None

    def alpha(self, p):
        """How opaque a pixel of the frame comes out."""
        if self.opacity is None:
            return 255
        return SOLID_ALPHA if p in self.solid else self.opacity(p[1])

    def render(self, outline):
        """The finished frame: every part, an outline round the lot, then the glows over it."""
        img = new_image(self.w, self.h)
        for (x, y), rgb in self.colour.items():
            if 0 <= x < self.w and 0 <= y < self.h:
                img[y][x] = [rgb[0], rgb[1], rgb[2], self.alpha((x, y))]
        for (x, y) in list(self.colour):
            for n in ((x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1)):
                if n not in self.colour and 0 <= n[0] < self.w and 0 <= n[1] < self.h:
                    img[n[1]][n[0]] = [outline[0], outline[1], outline[2], self.alpha((x, y))]
        for (x, y), rgba in self.over:
            put(img, x, y, rgba)
        return img


def inside(points, px, py):
    """Whether a point's inside a polygon (even-odd)."""
    hit = False
    j = len(points) - 1
    for i in range(len(points)):
        (xi, yi), (xj, yj) = points[i], points[j]
        if (yi > py) != (yj > py) and px < (xj - xi) * (py - yi) / (yj - yi) + xi:
            hit = not hit
        j = i
    return hit


def rotate(point, pivot, angle):
    """A point turned round a pivot, by an angle in degrees (positive turns the top towards the
    back, which is to the right, facing left)."""
    a = math.radians(angle)
    dx, dy = point[0] - pivot[0], point[1] - pivot[1]
    return (pivot[0] + dx * math.cos(a) - dy * math.sin(a), pivot[1] + dx * math.sin(a) + dy * math.cos(a))


def shifted(ramp, k):
    """A ramp moved k shades darker, the darkest repeated, for the far side of something."""
    return tuple(ramp[max(0, i - k)] for i in range(len(ramp)))


# --------------------------------------------------------------------------- #
# Sheets
# --------------------------------------------------------------------------- #

def sheet(name, w, h, draw, outline):
    """Draws a creature's whole sheet, frame by frame, and saves it as Creatures/<name>.png.
    `draw(figure, facing, column)` draws one frame; facing right is its facing left mirrored."""
    img = new_image(w * COLUMNS, h * 4)
    for r in range(4):
        for c in range(COLUMNS):
            fig = Figure(w, h, flip=(r == RIGHT), seed=r * 31 + c * 7)
            draw(fig, LEFT if r == RIGHT else r, c)
            frame = fig.render(outline)
            for y in range(h):
                img[r * h + y][c * w:(c + 1) * w] = frame[y]
    write_png(os.path.join(OUT, "Creatures", name + ".png"), img)


# --------------------------------------------------------------------------- #
# Gloomfang
# --------------------------------------------------------------------------- #

#: The gloomfang's coat, nearly black with a cold violet in its light; its belly and muzzle a
#: little warmer and greyer; its fangs and claws; the eyes and the smoke out of them.
GLOOM_FUR = ((24, 22, 38), (44, 42, 64), (68, 66, 96), (100, 98, 134), (142, 140, 178))
GLOOM_PALE = ((40, 36, 52), (66, 62, 80), (98, 94, 112), (136, 132, 148))
FANG = ((180, 170, 170), (236, 230, 220))
GLOOM_EYE = ((176, 120, 255), (236, 214, 255))
SMOKE = (8, 4, 14)
SMOKE_LIT = (52, 22, 80)
MAW = (96, 22, 40)
GLOOM_OUTLINE = (16, 12, 26)

#: How far right a gloomfang side on is moved, so its nose clears the frame's edge.
WOLF_SHIFT = 1


def quad_pose(column):
    """How a four-footed thing stands in a frame: how far its body's dropped (+) and pushed back
    (+) or forward (-), where each foot is from where it stands (forward is -) and how high it's
    lifted, where its head's moved to and how far its muzzle's tipped up, how open its jaws are,
    where its tail's swung, whether its eyes are open, how hard it's smoking or glowing, and how
    far its front end's reared up (in pixels at the shoulder)."""
    pose = {"drop": 0, "back": 0, "feet": {"fl": (0, 0), "fr": (0, 0), "hl": (0, 0), "hr": (0, 0)},
            "head": (0, 0), "tilt": 0, "jaw": 0, "tail": 0, "eyes": True, "fume": 1.0, "rear": 0}
    if 1 <= column <= 4:
        # A trot: the near fore and far hind foot go together, then the other pair.
        i = column - 1
        swing = (-3, 0, 3, 0)[i]
        lift_a, lift_b = (0, 0, 0, 2)[i], (0, 2, 0, 0)[i]
        pose["feet"] = {"fl": (swing, lift_a), "hr": (swing, lift_a), "fr": (-swing, lift_b), "hl": (-swing, lift_b)}
        pose["drop"] = (0, -1, 0, -1)[i]
        pose["tail"] = (1, 0, -1, 0)[i]
    elif column == WIND_UP:
        pose.update(drop=2, back=1, head=(-1, 3), tilt=-8, tail=-2, fume=1.5, rear=1,
                    feet={"fl": (-2, 0), "fr": (-1, 0), "hl": (2, 0), "hr": (3, 0)})
    elif column == STRIKE:
        pose.update(drop=-1, back=0, head=(0, 0), jaw=2, tail=2, fume=1.2, rear=2,
                    feet={"fl": (-4, 4), "fr": (-3, 3), "hl": (3, 0), "hr": (4, 0)})
    elif column == CAST:
        pose.update(drop=1, back=1, head=(1, -1), tilt=38, jaw=1, fume=2.4,
                    feet={"fl": (-1, 0), "fr": (0, 0), "hl": (1, 0), "hr": (2, 0)})
    elif column == HURT:
        pose.update(back=1, head=(1, -1), tilt=12, tail=-1, eyes=False, fume=0.5,
                    feet={"fl": (-2, 0), "fr": (-1, 0), "hl": (1, 0), "hr": (2, 0)})
    return pose


#: How many pixels a thread of smoke runs before it's gone, at its ordinary strength.
SMOKE_LENGTH = 10


def smoke(fig, x, y, drift, amount, phase, rise=1.0, bend=0.0):
    """Black smoke leaking off a point: a thread that runs `rise` pixels up and `drift` across for
    every pixel it goes (and `bend` more across the further it's gone, so it can go straight up
    and then stream back), curling as it goes, thick and dense where it leaves and thinning to
    nothing, with the odd gap in it."""
    length = SMOKE_LENGTH * amount
    dense = {}
    steps = int(length * 2) + 1
    for i in range(steps + 1):
        k = i / 2.0
        t = k / max(1.0, length)
        sway = math.sin(k * 0.8 + phase * math.tau) * 0.8
        px = x + 0.5 + drift * k + bend * k * k + sway * (1 - min(1.0, abs(drift)) * 0.5)
        py = y + 0.5 - k * rise + sway * min(1.0, abs(drift)) * 0.5
        r = 0.9 * (1 - t) + 0.55
        colour = lerp_color(SMOKE_LIT, SMOKE, t * 3)
        for qy in range(int(math.floor(py - r)), int(math.ceil(py + r)) + 1):
            for qx in range(int(math.floor(px - r)), int(math.ceil(px + r)) + 1):
                d = math.hypot(qx + 0.5 - px, qy + 0.5 - py)
                if d > r:
                    continue
                a = 255 * (1 - t ** 1.5 * 0.75) * (1 - 0.3 * d / r)
                if t > 0.4 and hash01(qx, qy, int(phase * 8) + 3) < 0.25:
                    continue
                if a > dense.get((qx, qy), (0, None))[0]:
                    dense[(qx, qy)] = (a, colour)
    for (qx, qy), (a, colour) in sorted(dense.items()):
        fig.glow(qx, qy, tuple(colour) + (int(a),))


def eye_glow(fig, x, y, colours, open_=True, width=2, shut=None):
    """A glowing eye: its bright core and a softer pixel beside it, and a little light spilling
    out round it. Shut, it's a dark line."""
    if not open_:
        for i in range(width):
            fig.dot(x + i, y, shut or GLOOM_OUTLINE)
        return
    fig.dot(x, y, colours[1])
    for i in range(1, width):
        fig.dot(x + i, y, colours[0])
    fig.glow(x - 1, y, colours[0] + (90,))
    fig.glow(x + width, y, colours[0] + (70,))


def ruff(fig, x0, x1, ramp, seed, every=0.55, tall=2):
    """A ragged edge of fur along the top of whatever's drawn between two columns: tufts that lean
    back, so the back line bristles instead of being smooth."""
    tops = {x: fig.top_of(x) for x in range(x0, x1 + 2)}
    for x in range(x0, x1 + 1):
        if tops[x] is None:
            continue
        h = hash01(x, 0, seed)
        if h < every:
            fig.dot(x, tops[x] - 1, ramp[2] if h < every / 2 else ramp[1])
            if h < every / 4 and tall > 1 and tops[x + 1] is not None and tops[x + 1] >= tops[x]:
                fig.dot(x + 1, tops[x] - 1, ramp[1])


def wolf_side(fig, column, pose):
    """A gloomfang side on, facing left: lean, long in the leg, hackles up over its shoulders and
    its head held low in front of them."""
    d, b, rear = pose["drop"], pose["back"], pose["rear"]
    feet = pose["feet"]
    fur, far = GLOOM_FUR, shifted(GLOOM_FUR, 1)
    g = fig.ground + 0.5

    def foreleg(x, foot, ramp, bias=0.0):
        fx, lift = foot
        paw = (x + fx - 0.6, g - 1.2 - lift)
        fig.limb([(x + b, 19 + d - rear), (x + b * 0.5 + fx * 0.5, 23.5 + d * 0.4 - lift * 0.5), paw],
                 [2.2, 1.4, 1.2], ramp, bias=bias)
        fig.ball(paw[0] - 0.8, g - 0.6 - lift, 1.8, 1.0, ramp, bias=bias)

    def hindleg(x, foot, ramp, bias=0.0):
        fx, lift = foot
        fig.limb([(x + b, 17 + d + rear), (x - 1.8 + b * 0.6 + fx * 0.3, 21.5 + d * 0.6),
                  (x + 1.8 + b * 0.3 + fx * 0.7, 24.6 - lift * 0.6), (x + 0.8 + fx, g - 1.2 - lift)],
                 [3.0, 1.7, 1.2, 1.2], ramp, bias=bias)
        fig.ball(x + fx, g - 0.6 - lift, 1.8, 1.0, ramp, bias=bias)

    # The far side first, a shade darker: its legs, and the tail hanging behind.
    foreleg(19, feet["fr"], far, -0.1)
    hindleg(33, feet["hr"], far, -0.1)
    t = pose["tail"]
    fig.limb([(34.5 + b, 13.5 + d + rear * 0.5), (39 + b, 14.5 + d + t * 0.5), (41.8 + b, 18 + d + t),
              (42.2 + b - t * 0.5, 22 + d + t * 1.5)], [1.6, 2.5, 2.2, 1.0], fur, rough=0.25)

    # The body: a deep chest, a tucked waist and lean haunches, bristling along the top.
    fig.limb([(17 + b, 16.6 + d - rear), (30 + b, 16.2 + d + rear * 0.5)], [4.3, 3.5], fur, rough=0.25, seam=False)
    fig.ball(31 + b, 16.3 + d + rear * 0.5, 4.6, 4.4, fur, rough=0.25, seam=False)
    fig.ball(17 + b, 17 + d - rear, 5.2, 5.0, fur, rough=0.25, seam=False)
    ruff(fig, int(13 + b), int(34 + b), fur, 5, every=0.6)

    foreleg(17, feet["fl"], fur)
    hindleg(31, feet["hl"], fur)
    wolf_head_side(fig, column, pose, 15 + b, 15 + d - rear)


def wolf_head_side(fig, column, pose, nx, ny):
    """The head side on, from where its neck leaves the chest at (nx, ny): a long muzzle, tall
    ears, the eye burning and the smoke streaming back off it."""
    hx, hy = pose["head"]
    tilt = pose["tilt"]
    jaw = pose["jaw"]
    fur, far = GLOOM_FUR, shifted(GLOOM_FUR, 1)
    pivot = (nx - 3 + hx, ny + hy)

    def at(x, y):
        return rotate((nx - 5.5 + x + hx, ny - 0.5 + y + hy), pivot, tilt)

    fig.limb([(nx + 2, ny + 0.5), at(1.5, 0.5)], [3.8, 3.2], fur, seam=False, rough=0.25)
    fig.plate([at(1.5, -2.5), at(3.5, -8.0), at(4.8, -2.2)], far, facing=(0.3, -0.2, 0.9))
    sx, sy = at(0, 0)
    fig.ball(sx, sy, 4.2, 3.6, fur, rough=0.2)
    if jaw:
        gape = jaw * 1.2
        fig.limb([at(-2, 2.4), at(-6.2, 3.0 + gape)], [1.4, 1.0], shifted(GLOOM_PALE, 1))
        for i in range(3):
            mx, my = at(-3.5 - i, 2.2 + gape * 0.5)
            fig.dot(int(mx), int(my), MAW)
        fx, fy = at(-6.0, 2.4)
        fig.dot(int(fx), int(fy), FANG[1])
        fx, fy = at(-5.6, 2.4 + gape)
        fig.dot(int(fx), int(fy), FANG[0])
    fig.limb([at(-2, 1.0), at(-6.5, 2.1)], [2.3, 1.5], fur, seam=False, rough=0.2)
    if not jaw:
        fig.limb([at(-2.2, 2.3), at(-6, 2.6)], [1.2, 0.9], shifted(GLOOM_PALE, 1), seam=False)
    fig.plate([at(-1.0, -2.6), at(0.6, -8.2), at(2.6, -2.8)], fur, facing=(-0.5, -0.3, 0.8))
    tx, ty = at(-7.6, 1.4)
    fig.dot(int(tx), int(ty), GLOOM_OUTLINE)
    ex, ey = at(-2.4, -0.6)
    ex, ey = int(ex), int(ey)
    fig.dot(ex + 1, ey - 1, GLOOM_OUTLINE)
    eye_glow(fig, ex, ey, GLOOM_EYE, pose["eyes"])
    if pose["eyes"]:
        # It curls back off the head, or goes straight up off a raised one.
        bend = 0.13 if tilt < 20 else 0.02
        smoke(fig, ex + 1, ey - 2, -0.35 if tilt < 20 else 0.15, pose["fume"], column / 4.0, bend=bend)


def wolf_front(fig, column, pose):
    """A gloomfang coming at you: its head big in front, its chest and forelegs under it, and its
    back and haunches spreading out behind, with the tail to one side."""
    d = pose["drop"]
    hx, hy = pose["head"]
    feet = pose["feet"]
    fur, far = GLOOM_FUR, shifted(GLOOM_FUR, 1)
    g = fig.ground + 0.5
    cx = 24
    for side in (-1, 1):
        fx, lift = feet["hl" if side < 0 else "hr"]
        foot_y = g - 3.6 - lift - max(0, fx) * 0.2
        fig.limb([(cx + side * 5.5, 17 + d), (cx + side * 6.0, foot_y - 0.5)], [2.2, 1.4], far, bias=-0.1)
        fig.ball(cx + side * 6.0, foot_y, 1.7, 1.0, far, bias=-0.1)
    fig.ball(cx, 15.5 + d, 7.6, 5.0, far, rough=0.25)
    ruff(fig, cx - 7, cx + 7, far, 9, every=0.45, tall=1)
    fig.ball(cx, 20.5 + d, 5.0, 4.4, fur, rough=0.25)
    for side in (-1, 1):
        fx, lift = feet["fl" if side < 0 else "fr"]
        x = cx + side * 2.6
        foot_y = g - 0.6 - lift - max(0, fx) * 0.3
        fig.limb([(x, 22 + d), (x, foot_y - 0.6)], [1.8, 1.4], fur)
        fig.ball(x, foot_y, 1.9, 1.0, fur)
    wolf_head_front(fig, column, pose, cx + hx, 12.5 + d + hy)


def wolf_head_front(fig, column, pose, cx, cy):
    """The head facing you: tall ears, ruffs bristling out at the cheeks, the paler muzzle, a
    scowl, and both eyes burning, the smoke coming up off them past the ears."""
    fur, far = GLOOM_FUR, shifted(GLOOM_FUR, 1)
    lift = pose["tilt"] / 12.0
    for side in (-1, 1):
        fig.plate([(cx + side * 1.8, cy - 3.0), (cx + side * 5.0, cy - 9.5 - lift * 0.3), (cx + side * 6.2, cy - 1.8)],
                  fur if side < 0 else far, facing=(side * 0.3, -0.4, 0.85))
        fig.plate([(cx + side * 3.0, cy - 3.6), (cx + side * 4.8, cy - 7.4), (cx + side * 5.2, cy - 3.2)],
                  shifted(GLOOM_PALE, 1), facing=(0.0, 0.0, 1.0), seam=False)
    for side in (-1, 1):
        fig.plate([(cx + side * 4.0, cy - 1.5), (cx + side * 7.6, cy + 2.4), (cx + side * 5.5, cy + 2.6),
                   (cx + side * 7.0, cy + 4.4), (cx + side * 3.5, cy + 4.5)], fur, facing=(side * 0.6, 0.2, 0.75))
    fig.ball(cx, cy, 5.6, 4.8, fur, rough=0.2, bias=0.06)
    my = cy + 3.4 - lift
    fig.ball(cx, my, 3.0, 2.4, GLOOM_PALE)
    nose = int(round(my - 1.5))
    fig.dot(int(cx) - 1, nose, GLOOM_OUTLINE)
    fig.dot(int(cx), nose, GLOOM_OUTLINE)
    if pose["jaw"]:
        rows = 1 if pose["jaw"] == 1 else 2
        for r in range(rows):
            for x in range(int(cx) - 2, int(cx) + 2):
                fig.dot(x, nose + 2 + r, MAW)
        fig.dot(int(cx) - 2, nose + 2, FANG[1])
        fig.dot(int(cx) + 1, nose + 2, FANG[1])
    ey = int(round(cy - 0.5 - lift))
    for side in (-1, 1):
        inner = int(cx) - 3 if side < 0 else int(cx) + 2
        outer = inner - 1 if side < 0 else inner + 1
        if pose["eyes"]:
            fig.dot(outer, ey, GLOOM_EYE[1])
            fig.dot(inner, ey, GLOOM_EYE[0])
            fig.glow(outer + side, ey, GLOOM_EYE[0] + (80,))
            smoke(fig, outer + side, ey - 2, side * 0.45, pose["fume"], column / 4.0 + side * 0.3)
        else:
            fig.dot(outer, ey, GLOOM_OUTLINE)
            fig.dot(inner, ey, GLOOM_OUTLINE)
        fig.dot(inner, ey - 1, GLOOM_OUTLINE)


def wolf_back(fig, column, pose):
    """A gloomfang going away: haunches and tail nearest, its back, the back of its head and its
    ears, and the smoke from its eyes coming up either side of them."""
    d = pose["drop"]
    hx, hy = pose["head"]
    feet = pose["feet"]
    fur, far = GLOOM_FUR, shifted(GLOOM_FUR, 1)
    g = fig.ground + 0.5
    cx = 24
    for side in (-1, 1):
        fx, lift = feet["fl" if side < 0 else "fr"]
        fig.ball(cx + side * 3.0, g - 3.6 - lift - max(0, -fx) * 0.2, 1.7, 1.0, far, bias=-0.1)
    hcx, hcy = cx + hx, max(11.0, 11 + d + hy - pose["tilt"] / 12.0)
    for side in (-1, 1):
        fig.plate([(hcx + side * 1.6, hcy - 2.5), (hcx + side * 4.6, hcy - 8.5), (hcx + side * 5.6, hcy - 1.0)],
                  fur, facing=(side * 0.3, -0.4, 0.85))
    fig.ball(hcx, hcy, 5.0, 4.2, fur, rough=0.2)
    fig.limb([(cx, 13 + d), (cx, 17 + d)], [5.2, 6.4], fur, rough=0.25)
    ruff(fig, cx - 5, cx + 5, fur, 13, every=0.5, tall=1)
    for side in (-1, 1):
        fx, lift = feet["hl" if side < 0 else "hr"]
        x = cx + side * 5.0
        foot_y = g - 0.6 - lift - max(0, -fx) * 0.3
        fig.limb([(x, 20 + d), (x + side * 0.6, 23 + d * 0.5), (x - side * 0.2, foot_y - 0.6)], [3.0, 2.0, 1.5], fur)
        fig.ball(x, foot_y, 1.9, 1.0, fur)
    fig.ball(cx, 20.5 + d, 6.2, 4.2, fur, rough=0.25)
    for x in (cx - 1, cx):
        for y in range(int(12 + d), int(18 + d)):
            if hash01(x, y, 17) < 0.5:
                fig.dot(x, y, fur[1])
    t = pose["tail"]
    fig.limb([(cx, 19.5 + d), (cx + t * 0.6, 23.5 + d), (cx + t * 1.2, g - 1.5)], [2.0, 2.6, 1.3], fur, rough=0.25)
    if pose["eyes"]:
        for side in (-1, 1):
            smoke(fig, int(hcx + side * 3.5), int(hcy - 1), side * 0.45, pose["fume"], column / 4.0 + side * 0.3)


def wolf_downed(fig, column):
    """A gloomfang down: sprawled flat, legs out in front and behind, its head on the ground and
    its ears back, the eyes gone dark and the last of the smoke going up off it."""
    fur, far = GLOOM_FUR, shifted(GLOOM_FUR, 1)
    g = fig.ground + 0.5
    fig.limb([(13.5, 25.5), (6, 26.6)], [1.5, 1.1], far)
    fig.limb([(34, 24.5), (39.5, 26), (44, 27.2)], [1.8, 2.0, 1.0], fur, rough=0.25)
    fig.limb([(16, 23.8), (30, 23.6)], [3.6, 3.2], fur, rough=0.25, seam=False)
    fig.ball(31, 23.4, 4.4, 4.2, fur, rough=0.25, seam=False)
    fig.ball(16.5, 24.2, 4.6, 4.2, fur, rough=0.25, seam=False)
    ruff(fig, 13, 34, fur, 21, every=0.4, tall=1)
    fig.limb([(14, 26.5), (6.5, g - 0.8)], [1.5, 1.2], fur)
    fig.limb([(31, 25.5), (36.5, g - 0.8)], [2.2, 1.2], fur)
    fig.ball(9.5, 23.5, 4.0, 3.4, fur, rough=0.2)
    fig.limb([(7.5, 24.5), (3, 26.0)], [2.2, 1.4], fur, seam=False)
    fig.plate([(10, 20.8), (14.5, 19.5), (12.5, 21.8)], far, facing=(0.2, -0.5, 0.8))
    fig.dot(2, 25, GLOOM_OUTLINE)
    fig.dot(6, 23, GLOOM_OUTLINE)
    fig.dot(7, 23, GLOOM_OUTLINE)
    smoke(fig, 7, 21, 0.5, 0.5, 0.3)


def gloomfang(fig, facing, column):
    """One frame of a gloomfang."""
    if column == DOWNED:
        wolf_downed(fig, column)
        return
    pose = quad_pose(column)
    if facing == LEFT:
        fig.shift = WOLF_SHIFT
        wolf_side(fig, column, pose)
    elif facing == DOWN:
        wolf_front(fig, column, pose)
    else:
        wolf_back(fig, column, pose)


# --------------------------------------------------------------------------- #
# The Hollowed
# --------------------------------------------------------------------------- #

#: Ash-grey skin, gone cold and a little blue in its shadows; the empty, pale eyes; the black of a
#: crack in them, and the outline they're all drawn round with.
ASH = ((66, 62, 74), (98, 94, 104), (130, 126, 132), (160, 156, 158), (190, 186, 184))
HOLLOW_EYE = ((150, 186, 214), (236, 248, 255))
CRACK = (24, 16, 28)
HOLLOW_OUTLINE = (34, 24, 38)

#: Doomward's war-gear: blackened iron, the faded crimson of its cloth, tarnished bronze, worn
#: leather, the drab of the breeches and mail under it, and boots.
DOOM_IRON = ((26, 26, 38), (44, 44, 60), (66, 68, 88), (98, 102, 122), (146, 152, 170))
DOOM_CLOTH = ((60, 22, 36), (96, 34, 48), (134, 52, 60), (168, 82, 78))
DOOM_BRONZE = ((92, 58, 30), (144, 98, 46), (196, 148, 72), (236, 200, 120))
LEATHER = ((56, 36, 30), (88, 58, 42), (122, 84, 58), (156, 116, 80))
DRAB = ((38, 34, 44), (58, 54, 64), (80, 76, 86), (106, 102, 110))
BOOT = ((40, 28, 30), (64, 44, 40), (92, 64, 52))

#: The hexer's robe (a scholar's plum, faded), its staff of blackened wood, and the black crystal
#: on top of it, with the Box's violet light in its edges.
HEX_ROBE = ((42, 26, 50), (66, 40, 72), (94, 60, 96), (126, 86, 120), (158, 118, 148))
STAFF = ((34, 24, 28), (58, 42, 42), (86, 64, 58))
BLACK_CRYSTAL = ((22, 12, 38), (58, 34, 104), (140, 96, 228), (236, 218, 255))
HEX_GLOW = (170, 110, 255)

#: The sapper's lantern light.
LANTERN_LIGHT = ((255, 196, 96), (255, 244, 196))

#: What a Hollowed leaves when it falls: ash, a little darker where it's heaped.
ASH_HEAP = ((76, 72, 82), (108, 104, 110), (142, 138, 140), (178, 174, 172))

#: Where things are on a Hollowed facing you, in a 32x48 frame: the middle of the head and how
#: big it is (the youth's big head, sixteen pixels across), the row the shoulders are on, the
#: belt's row, and the hips the legs hang from. Bob and stoop move all of them.
HEAD_Y, HEAD_RX, HEAD_RY = 14.3, 7.6, 7.0
SHOULDER_Y = 23.2
BELT_Y = 31
HIP_Y = 31.5


def biped_pose(column):
    """How someone on two legs stands in a frame: how far they've bobbed up (-) or down (+), how
    far they lean forward (+), where each foot is (how far forward and how high lifted, near foot
    first), where each hand is (forward and raised, the weapon hand first), which way the weapon
    points (degrees from straight down, forward positive), whether the eyes are open, how hard
    any magic's flaring, and how far the head's tipped back (-)."""
    pose = {"bob": 0, "lean": 0, "feet": [(0, 0), (0, 0)], "hands": [(0, 0), (0, 0)],
            "weapon": 20, "eyes": True, "flare": 0.0, "tip": 0, "carry": column <= 4}
    if 1 <= column <= 4:
        i = column - 1
        stride = (3, 0, -3, 0)[i]
        pose["feet"] = [(stride, (0, 0, 0, 2)[i]), (-stride, (0, 2, 0, 0)[i])]
        pose["hands"] = [(-stride * 0.5, 0), (stride * 0.5, 0)]
        pose["bob"] = (0, -1, 0, -1)[i]
        pose["weapon"] = 20 - stride * 3
    elif column == WIND_UP:
        pose.update(lean=-1, feet=[(2, 0), (-2, 0)], hands=[(-2, 10), (1, 2)], weapon=200)
    elif column == STRIKE:
        pose.update(lean=2, bob=1, feet=[(4, 0), (-3, 0)], hands=[(4, 6), (-1, 1)], weapon=120)
    elif column == CAST:
        pose.update(bob=-1, feet=[(1, 0), (-1, 0)], hands=[(2, 11), (2, 11)], weapon=175, flare=1.0)
    elif column == HURT:
        pose.update(lean=-2, tip=-1, feet=[(-1, 0), (1, 0)], hands=[(-3, 3), (-3, 4)], weapon=-15, eyes=False)
    return pose


def heading(angle):
    """Which way a weapon points, as a step across and down, for an angle from straight down
    (forward, which is left facing left, positive)."""
    a = math.radians(angle)
    return (-math.sin(a), math.cos(a))


def sword(fig, x, y, angle, length=10):
    """A Doomward soldier's short sword in a fist at (x, y): a notched iron blade with a bright
    edge, a bronze guard across it and a pommel the other side of the hand."""
    dx, dy = heading(angle)
    tip = (x + dx * length, y + dy * length)
    fig.limb([(x - dx * 1.8, y - dy * 1.8), (x, y)], [0.75, 0.75], LEATHER, seam=False)
    fig.dot(int(math.floor(x - dx * 2.4)), int(math.floor(y - dy * 2.4)), DOOM_BRONZE[2])
    fig.limb([(x + dx * 1.4, y + dy * 1.4), tip], [1.0, 0.55], DOOM_IRON)
    fig.limb([(x + dx * 1.3 - dy * 2.0, y + dy * 1.3 + dx * 2.0), (x + dx * 1.3 + dy * 2.0, y + dy * 1.3 - dx * 2.0)],
             [0.65, 0.65], DOOM_BRONZE, seam=False)
    nx, ny = x + dx * length * 0.6, y + dy * length * 0.6
    fig.dot(int(math.floor(nx + dy * 0.9)), int(math.floor(ny - dx * 0.9)), CRACK)


def staff(fig, x, y, angle, glow, below=12.5, above=22):
    """The hexer's staff through a fist at (x, y): blackened wood, down to the ground and up past
    the head, a shard of black crystal lashed to its top that glows brighter as it casts. Returns
    where the crystal's point is."""
    dx, dy = heading(angle)
    if dy > 0:
        below = min(below, (fig.ground - 0.5 - y) / dy)
        # Held up high, it's held further down its length, so the crystal stays in the frame.
        above = min(above, (y - 7.5) / dy)
    foot = (x + dx * below, y + dy * below)
    top = (x - dx * above, y - dy * above)
    fig.limb([foot, top], [0.8, 1.0], STAFF, seam=False)
    for i in (2.0, 3.0):
        fig.dot(int(math.floor(top[0] + dx * i)), int(math.floor(top[1] + dy * i)), DOOM_BRONZE[1])
    px, py = -dy, dx
    point = (top[0] - dx * 5.0, top[1] - dy * 5.0)
    fig.plate([(top[0] + px * 1.8, top[1] + py * 1.8 - dy), point, (top[0] - px * 1.8, top[1] - py * 1.8 - dy),
               (top[0] + dx * 0.8, top[1] + dy * 0.8)], BLACK_CRYSTAL[:3], facing=(-0.5, -0.5, 0.7))
    fig.plate([(top[0] + px * 0.2, top[1] + py * 0.2 - dy), point, (top[0] - px * 1.8, top[1] - py * 1.8 - dy)],
              BLACK_CRYSTAL[:3], facing=(0.6, 0.2, 0.75), seam=False)
    cx, cy = int(math.floor(top[0] - dx * 2.5)), int(math.floor(top[1] - dy * 2.5))
    fig.dot(cx, cy, BLACK_CRYSTAL[3] if glow > 0.5 else BLACK_CRYSTAL[2])
    halo = int(50 + 140 * min(1.0, glow))
    for ox, oy in ((-2, 0), (2, 0), (0, -3), (-1, -2), (1, -2), (0, 2)):
        if not fig.has(cx + ox, cy + oy):
            fig.glow(cx + ox, cy + oy, HEX_GLOW + (halo // 2,))
    return point


def pick(fig, x, y, angle, point=1):
    """The sapper's pick in a fist at (x, y): a stout handle, and its iron head across the end,
    one point long and one short (the long one turned the other way round if `point` is -1)."""
    dx, dy = heading(angle)
    end = (x + dx * 8.5, y + dy * 8.5)
    fig.limb([(x - dx * 2, y - dy * 2), end], [0.85, 0.85], LEATHER, seam=False)
    px, py = -dy * point, dx * point
    fig.limb([(end[0] + px * 5.0 - dx * 1.4, end[1] + py * 5.0 - dy * 1.4), (end[0] + px * 2.2, end[1] + py * 2.2), end,
              (end[0] - px * 2.6, end[1] - py * 2.6)], [0.5, 1.0, 1.25, 0.6], DOOM_IRON)


def lantern(fig, x, y, bright):
    """A small brass lantern hanging from a fist at (x, y), lit, and its light spilling round it."""
    fig.dot(x, y + 1, DOOM_BRONZE[0])
    fig.block(x - 1.5, y + 2, x + 2.5, y + 7, DOOM_BRONZE, bevel=1)
    for oy in (3, 4, 5):
        fig.dot(x, y + oy, LANTERN_LIGHT[1] if oy == 4 else LANTERN_LIGHT[0])
        fig.dot(x + 1, y + oy, LANTERN_LIGHT[0])
    fig.dot(x, y + 2, DOOM_BRONZE[3])
    reach = 2 + int(bright)
    for oy in range(-reach - 1, reach + 2):
        for ox in range(-reach, reach + 2):
            d = math.hypot(ox - 0.5, oy * 0.9)
            if d <= reach + 0.5 and not fig.has(x + ox, y + 4 + oy):
                fig.glow(x + ox, y + 4 + oy, LANTERN_LIGHT[0] + (int(110 * (1 - d / (reach + 1))) + 15,))


def sparks(fig, x, y, strength):
    """The Box's power gathering in a hand: violet motes and black wisps round a point."""
    for i in range(int(7 * strength)):
        a = i * 2.4
        r = 1.5 + (i % 3)
        px, py = int(x + math.cos(a) * r), int(y + math.sin(a) * r * 0.8)
        fig.glow(px, py, (HEX_GLOW if i % 2 else BLACK_CRYSTAL[3]) + (210 - i * 10,))
    fig.glow(int(x), int(y), BLACK_CRYSTAL[3] + (240,))


def hollow_eyes(fig, xs, y, open_):
    """Empty eyes: a pixel pale and lit from inside over a dimmer one, no pupil, and a little light
    coming off them; shut (it's hurt), a dark line."""
    for x in xs:
        if open_:
            fig.dot(x, y, HOLLOW_EYE[1])
            fig.dot(x, y + 1, HOLLOW_EYE[0])
        else:
            fig.dot(x, y + 1, CRACK)
    if open_:
        fig.glow(min(xs) - 1, y, HOLLOW_EYE[0] + (70,))
        fig.glow(max(xs) + 1, y, HOLLOW_EYE[0] + (70,))


def face(fig, kit, cx, cy, pose, facing):
    """The head itself: ash-grey and a little sunken, the empty eyes, a slack mouth and a crack
    down one cheek. Facing away, just the back of the skull and a crack."""
    skin = shifted(ASH, 2) if kit["head"] == "hood" else ASH
    if facing == LEFT:
        fig.ball(cx, cy, HEAD_RX - 0.6, HEAD_RY, skin)
        ey = int(cy + 1.5)
        fx = int(cx - HEAD_RX + 1.6)
        fig.dot(fx, ey - 1, skin[0])
        fig.dot(fx + 1, ey - 1, skin[1])
        hollow_eyes(fig, (fx, fx + 1), ey, pose["eyes"])
        fig.dot(fx - 1, ey + 4, skin[1])
        fig.dot(fx, ey + 4, skin[0])
        fig.dot(int(cx + 2), ey, skin[1])
        fig.dot(int(cx + 2), ey + 1, skin[0])
        fig.dot(int(cx + 3), ey + 1, skin[1])
        for x, y in ((fx + 3, ey + 3), (fx + 4, ey + 4), (fx + 5, ey + 4)):
            fig.dot(x, y, CRACK)
        return
    fig.ball(cx, cy, HEAD_RX, HEAD_RY, skin)
    if facing == UP:
        for x, y in ((int(cx) + 2, int(cy) + 2), (int(cx) + 3, int(cy) + 3), (int(cx) + 3, int(cy) + 4)):
            fig.dot(x, y, CRACK)
        return
    ey = int(cy + 1.5)
    left, right = int(cx) - 4, int(cx) + 2
    for x in (left, left + 1, right, right + 1):
        fig.dot(x, ey - 1, skin[1])
    fig.dot(left - 1, ey - 1, skin[0])
    fig.dot(right + 2, ey - 1, skin[0])
    hollow_eyes(fig, (left, left + 1), ey, pose["eyes"])
    hollow_eyes(fig, (right, right + 1), ey, pose["eyes"])
    for x in (left - 1, right + 2):
        fig.dot(x, ey + 2, skin[1])
    fig.dot(int(cx) - 1, ey + 4, skin[0])
    fig.dot(int(cx), ey + 4, skin[0])
    for x, y in ((right + 3, ey - 2), (right + 3, ey - 1), (right + 4, ey), (right + 3, ey + 2), (right + 4, ey + 3)):
        if fig.has(x, y):
            fig.dot(x, y, CRACK)


def kettle_helm(fig, cx, cy, facing):
    """A footman's kettle helm: a dented iron dome with a wide brim over the brow, a bronze rim,
    a crack down the dome and a ragged crimson crest."""
    side = facing == LEFT
    brim = int(cy - HEAD_RY + 4.5)
    dome_cx = cx + (0.6 if side else 0)
    shaped = {}
    for y in range(int(cy - HEAD_RY - 3), brim + 1):
        for x in range(int(cx - 10), int(cx + 10)):
            nx, ny = (x + 0.5 - dome_cx) / 7.2, (y + 0.5 - (brim + 0.5)) / 7.5
            if nx * nx + ny * ny <= 1.0:
                shaped[(x, y)] = (nx, ny, math.sqrt(max(0.0, 1 - nx * nx - ny * ny)))
    fig.lay(shaped, DOOM_IRON)
    x0, x1 = (int(cx - 10), int(cx + 10)) if not side else (int(cx - 10), int(cx + 9))
    for x in range(x0, x1):
        end = min(x - x0, x1 - 1 - x)
        y = brim + (1 if end == 0 else 0)
        fig.dot(x, y, DOOM_IRON[3] if x < cx - 2 else DOOM_IRON[2])
        fig.dot(x, y + 1, DOOM_BRONZE[1] if x < cx else DOOM_BRONZE[0])
    crack = ((2, -7), (2, -6), (3, -5), (3, -4), (4, -3))
    flip = -1 if facing == UP else 1
    for ox, oy in crack:
        x = int(cx) + ox * flip - (1 if flip < 0 else 0)
        fig.dot(x, brim + oy + 1, CRACK)
        fig.dot(x - 1, brim + oy + 1, DOOM_IRON[4])
    top = brim - 7
    for i in range(-2, 3) if not side else range(0, 5):
        x = int(dome_cx) + i
        fig.dot(x, top, DOOM_CLOTH[2] if i < 1 else DOOM_CLOTH[1])
        if hash01(x, 3, 7) < 0.6:
            fig.dot(x + (1 if side else 0), top - 1, DOOM_CLOTH[3] if i < 0 else DOOM_CLOTH[2])


def deep_hood(fig, cx, cy, facing):
    """A hexer's deep hood: a bell of cloth over the head to the shoulders with a point that flops
    back, the face far inside it, dark."""
    side = facing == LEFT
    top = cy - HEAD_RY - 1.5
    bottom = SHOULDER_Y + 2 + (cy - HEAD_Y)

    def half(y):
        t = (y + 0.5 - top) / (bottom - top)
        if t < 0.28:
            return 8.4 * math.sqrt(max(0.0, 1 - ((0.28 - t) / 0.28) ** 2))
        return 8.4 + max(0.0, t - 0.7) * 4

    shaped = {}
    hood_cx = cx + (1.5 if side else 0)
    for y in range(int(top) - 1, int(bottom) + 1):
        w = half(y)
        if w < 0.8:
            continue
        for x in range(int(hood_cx - w - 1), int(hood_cx + w + 1)):
            if abs(x + 0.5 - hood_cx) > w:
                continue
            if facing != UP:
                ox = (x + 0.5 - (cx - (3.0 if side else 0))) / (3.8 if side else 5.0)
                oy = (y + 0.5 - (cy + 2.0)) / 5.2
                if ox * ox + oy * oy < 1.0:
                    continue
            u = (x + 0.5 - hood_cx) / max(1.0, w)
            v = (y + 0.5 - (top + bottom) / 2) / ((bottom - top) / 2)
            shaped[(x, y)] = (u * 0.85, v * 0.5 - 0.25, math.sqrt(max(0.05, 1 - u * u * 0.72)))
    tip = (hood_cx + (6 if side else 0.5), top - 1)
    for i in range(3):
        shaped[(int(tip[0]) + (i if side else 0), int(tip[1]) + i - 1)] = (0.3, -0.7, 0.6)
    fig.lay(shaped, HEX_ROBE)
    if facing != UP:
        # The lining round the face, and the shadow the hood throws inside it.
        for (x, y) in list(shaped):
            for n in ((x - 1, y), (x + 1, y), (x, y + 1), (x, y - 1)):
                if n not in shaped and fig.has(*n) and n[1] > top + 2 and abs(n[0] + 0.5 - cx) < 7:
                    fig.dot(x, y, HEX_ROBE[0])
                    break
    else:
        for y in range(int(top + 3), int(bottom)):
            if hash01(0, y, 31) < 0.8:
                fig.dot(int(hood_cx), y, HEX_ROBE[1])


def leather_cap(fig, cx, cy, facing):
    """A sapper's cap: worn leather close over the skull with flaps over the ears, an iron band
    across the brow with rivets, and a patch stitched on."""
    side = facing == LEFT
    brim = int(cy - HEAD_RY + 4.5)
    shaped = {}
    cap_cx = cx + (0.8 if side else 0)
    for y in range(int(cy - HEAD_RY - 1), brim + 1):
        for x in range(int(cx - 10), int(cx + 10)):
            nx, ny = (x + 0.5 - cap_cx) / (HEAD_RX + 0.6), (y + 0.5 - (brim + 0.5)) / (HEAD_RY - 0.4)
            if nx * nx + ny * ny <= 1.0:
                shaped[(x, y)] = (nx, ny, math.sqrt(max(0.0, 1 - nx * nx - ny * ny)))
    # Ear flaps down the sides of the head (the one at the back, side on).
    flaps = (1,) if side else (-1, 1)
    if facing != UP or True:
        for s in flaps:
            fx = cap_cx + s * (HEAD_RX - 0.4) - (1.5 if side else 0)
            for y in range(brim, brim + 6):
                for x in (int(fx) - 1, int(fx)):
                    shaped[(x, y)] = (s * 0.8, 0.2, 0.55)
    fig.lay(shaped, LEATHER)
    x0, x1 = (int(cx - 8), int(cx + 8)) if not side else (int(cx - 8), int(cx + 6))
    for x in range(x0, x1):
        fig.dot(x, brim, DOOM_IRON[3] if x < cx - 2 else DOOM_IRON[2])
        fig.dot(x, brim - 1, DOOM_IRON[2] if x < cx - 2 else DOOM_IRON[1])
    for x in range(x0 + 1, x1 - 1, 4):
        fig.dot(x, brim - 1, DOOM_IRON[4])
    px = int(cx) + (2 if facing != UP else -4)
    for i in range(3):
        fig.dot(px + i, int(cy - HEAD_RY + 1), LEATHER[3] if i == 1 else LEATHER[0])
        fig.dot(px + i, int(cy - HEAD_RY + 2), LEATHER[0])


def headgear(fig, kit, cx, cy, facing):
    """Whatever's on a Hollowed's head."""
    if kit["head"] == "helm":
        kettle_helm(fig, cx, cy, facing)
    elif kit["head"] == "hood":
        deep_hood(fig, cx, cy, facing)
    else:
        leather_cap(fig, cx, cy, facing)


def hollowed_front(fig, kit, pose, facing):
    """A Hollowed seen from the front, or (with `facing` UP) from behind, standing on (16, ground):
    legs, the body in its gear, the arms and what's in them, then the head."""
    b, g = pose["bob"], fig.ground + 0.5
    cx = 16.0
    away = facing == UP
    stoop = kit.get("hunch", 0)
    robe = kit["body"] == "robe"
    # The weapon's in the figure's right hand: on your left facing you, your right going away.
    wx = -1 if not away else 1
    toward = 1 if not away else -1
    hands = pose["hands"]
    sy = SHOULDER_Y + b + stoop

    # Where each hand goes: down at the side, or up and out.
    hand_at = []
    for i, (fwd, up) in enumerate(hands):
        side = wx if i == 0 else -wx
        if i == 0 and pose["carry"] and kit["weapon"] == "pick":
            fwd, up = fwd, up + 5
        reach = 8.6 if robe and i == 0 else 7.2
        hx = cx + side * min(8.6, reach + max(0.0, up) * 0.3)
        hy = BELT_Y + 1 + b - up * 1.15 + fwd * 0.25 * toward
        hand_at.append((side, hx, hy))

    # Behind the body, going away: the lantern and the head of the pick.
    if away and kit.get("lantern"):
        side, hx, hy = hand_at[1]
        lantern(fig, int(hx), int(hy), kit.get("glow", 0) + pose["flare"] * 2)
    if away:
        weapon_front(fig, kit, pose, hand_at[0], facing)

    # The legs and boots, the forward foot a pixel nearer.
    for i, (fwd, lift) in enumerate(pose["feet"]):
        side = (-1, 1)[i] * wx
        x = cx + side * 2.6
        fy = g - 1.0 - lift - (0.8 if fwd * toward < 0 else 0)
        if not robe:
            fig.limb([(x, HIP_Y + b), (x - side * 0.2, 37 + b * 0.5), (x, fy - 1.2)], [2.4, 2.1, 1.9], DRAB)
            shin = DOOM_IRON if kit["body"] == "armour" else LEATHER
            fig.limb([(x, 38.5 + b * 0.5), (x, fy - 1.2)], [2.05, 1.95], shin, seam=False)
        fig.ball(x + side * 0.3, fy, 2.6, 1.5, BOOT)

    if robe:
        robe_front(fig, cx, b, g, pose, away)
    elif kit["body"] == "armour":
        fig.limb([(cx, 24.5 + b), (cx, 28.5 + b)], [5.0, 4.6], DOOM_IRON)
        tabard = [(cx - 3.8, 22 + b), (cx + 3.8, 22 + b), (cx + 4.6, 36.5 + b), (cx - 4.6, 36.5 + b)]
        fig.plate(tabard, DOOM_CLOTH, facing=(-0.15, 0.1, 0.98))
        shade_plate(fig, cx, 22 + b, 36.5 + b, 3.8, 4.6, DOOM_CLOTH)
        for x in range(int(cx - 4), int(cx + 4)):
            if hash01(x, 2, 23 + int(away)) < 0.45:
                fig.dot(x, int(37 + b), DOOM_CLOTH[0])
        if not away:
            # Doomward's ember on the chest, in bronze.
            for x, y, k in ((15, 25, 3), (16, 25, 2), (14, 26, 2), (15, 26, 3), (16, 26, 2), (17, 26, 1), (15, 27, 1), (16, 27, 0)):
                fig.dot(x, int(y + b), DOOM_BRONZE[k])
        for x in range(int(cx - 5), int(cx + 5)):
            fig.dot(x, int(BELT_Y + b), DOOM_BRONZE[2] if x < cx else DOOM_BRONZE[1])
        if not away:
            fig.dot(int(cx) - 1, int(BELT_Y + b), DOOM_BRONZE[3])
    else:
        fig.limb([(cx, 24.5 + b + stoop), (cx, 28.5 + b)], [5.0, 4.6], LEATHER)
        if not away:
            fig.plate([(cx - 4.2, 27.5 + b), (cx + 4.2, 27.5 + b), (cx + 4.6, 38 + b), (cx - 4.6, 38 + b)],
                      shifted(LEATHER, 1), facing=(-0.1, 0.1, 0.98))
            for y in (int(sy - 0.5), int(sy + 0.5)):
                fig.dot(int(cx) - 1, y, DOOM_CLOTH[2])
                fig.dot(int(cx), y, DOOM_CLOTH[1])
        for x in range(int(cx - 5), int(cx + 5)):
            fig.dot(x, int(BELT_Y + b), LEATHER[0])
        for x in (int(cx) + 2, int(cx) + 3) if not away else (int(cx) - 4, int(cx) - 3):
            fig.dot(x, int(BELT_Y + b) + 1, LEATHER[2])
            fig.dot(x, int(BELT_Y + b) + 2, LEATHER[1])

    # The arms: a sleeve to the elbow, ash-grey forearms, and hands.
    for side, hx, hy in hand_at:
        sx = cx + side * 5.8
        ex, ey = (sx + hx) / 2 + side * 0.9, (sy + hy) / 2
        sleeve = HEX_ROBE if robe else DRAB if kit["body"] == "armour" else DOOM_CLOTH
        if robe:
            fig.limb([(sx, sy), (ex, ey), (hx, hy - 0.5)], [2.1, 2.3, 2.8], sleeve)
            fig.ball(hx, hy + 0.8, 1.6, 1.5, ASH)
            fig.dot(int(hx - 1), int(hy + 2), CRACK)
            fig.dot(int(hx), int(hy + 2), CRACK)
        else:
            fig.limb([(sx, sy), (ex, ey)], [2.1, 1.9], sleeve)
            fig.limb([(ex, ey), (hx, hy)], [1.8, 1.6], DOOM_IRON if kit["body"] == "armour" else ASH, seam=False)
            fig.ball(hx, hy + 0.6, 1.7, 1.7, ASH)
    # Pauldrons: both shoulders on a footman, the pick shoulder on a sapper.
    for side in (-1, 1):
        if kit["body"] == "armour" or (kit["body"] == "jerkin" and side == wx):
            fig.ball(cx + side * 6.0, sy - 0.3, 3.3, 2.6, DOOM_IRON)
            fig.dot(int(cx + side * 6.0) - (1 if side < 0 else 0), int(sy + 1.6), DOOM_BRONZE[1])
            fig.dot(int(cx + side * 6.0) + (0 if side < 0 else -1), int(sy - 1.5), CRACK)

    if not away and not robe:
        weapon_front(fig, kit, pose, hand_at[0], facing)
    face(fig, kit, cx, HEAD_Y + b + stoop + pose["tip"], pose, facing)
    headgear(fig, kit, cx, HEAD_Y + b + stoop + pose["tip"], facing)
    if not away and robe:
        weapon_front(fig, kit, pose, hand_at[0], facing)
    if kit.get("lantern") and not away:
        side, hx, hy = hand_at[1]
        lantern(fig, int(hx), int(hy + 1), kit.get("glow", 0) + pose["flare"] * 2)
    if kit["weapon"] == "staff" and pose["flare"]:
        side, hx, hy = hand_at[1]
        sparks(fig, hx, hy - 1, pose["flare"])


def shade_plate(fig, cx, y0, y1, w0, w1, ramp):
    """Shades a hanging panel (a tabard, a robe) as a gentle curve across, lit on its left."""
    shaped = {}
    for y in range(int(y0), int(y1) + 1):
        t = (y - y0) / max(1.0, y1 - y0)
        w = w0 + (w1 - w0) * t
        for x in range(int(cx - w - 1), int(cx + w + 1)):
            if fig.has(x, y) and abs(x + 0.5 - cx) <= w:
                u = (x + 0.5 - cx) / w
                shaped[(x, y)] = (u * 0.8, -0.1 + t * 0.3, math.sqrt(max(0.05, 1 - u * u * 0.64)))
    fig.lay(shaped, ramp, seam=False)


def robe_front(fig, cx, b, g, pose, away):
    """The hexer's robe, facing you or going away: wide from the shoulders to a ragged hem on the
    ground, folds down it, a crimson stole and a rope belt."""
    (near_fwd, _), (far_fwd, _) = pose["feet"]
    sway = (near_fwd - far_fwd) * 0.12
    w0, w1 = 5.4, 7.8
    pts = [(cx - w0, 21 + b), (cx + w0, 21 + b), (cx + w1 + sway, g + 0.4), (cx - w1 + sway, g + 0.4)]
    fig.plate(pts, HEX_ROBE)
    shade_plate(fig, cx + sway * 0.5, 21 + b, g - 0.6, w0, w1, HEX_ROBE)
    for fold in (-4, -1, 3):
        for y in range(int(33 + b), int(g - 1)):
            x = int(cx + fold + (y - 33) * fold * 0.04 + sway)
            if hash01(fold, y, 5) < 0.85 and fig.has(x, y):
                fig.dot(x, y, HEX_ROBE[1])
    for x in range(int(cx - w1 - 1), int(cx + w1 + 1)):
        if fig.has(x, int(g - 1)):
            fig.dot(x, int(g - 1), HEX_ROBE[0] if hash01(x, 0, 47) < 0.6 else HEX_ROBE[1])
    if not away:
        for s in (-1, 1):
            x0 = int(cx) + (-3 if s < 0 else 1)
            for y in range(int(22 + b), int(39 + b)):
                fig.dot(x0, y, DOOM_CLOTH[2] if s < 0 else DOOM_CLOTH[1])
                fig.dot(x0 + 1, y, DOOM_CLOTH[1] if s < 0 else DOOM_CLOTH[0])
            fig.dot(x0, int(39 + b), DOOM_BRONZE[2])
            fig.dot(x0 + 1, int(39 + b), DOOM_BRONZE[1])
    for x in range(int(cx - 6), int(cx + 6)):
        fig.dot(x, int(BELT_Y + b), LEATHER[2] if (x + int(b)) % 2 else LEATHER[1])


def weapon_front(fig, kit, pose, hand, facing):
    """The weapon in the weapon hand seen square on: down and out at the side at rest, up over the
    head winding up and casting, across the body striking."""
    side, hx, hy = hand
    angle = pose["weapon"]
    out = -side
    kind = kit["weapon"]
    hy += 0.6
    if kind == "sword":
        if angle > 150:
            sword(fig, hx, hy, 180 + 20 * side, 10)
        elif angle > 60:
            sword(fig, hx, hy, 60 * side, 10)
        else:
            sword(fig, hx, hy, (8 + angle * 0.3) * out, 10)
    elif kind == "staff":
        staff(fig, hx, hy, (4 + max(0, angle - 20) * 0.1) * out if angle < 150 else 8 * side,
              kit.get("glow", 0) + pose["flare"])
    else:
        if pose["carry"] or angle > 150:
            pick(fig, hx, hy, 180 + 15 * side, -side)
        elif angle > 60:
            pick(fig, hx, hy, 50 * side, -side)
        else:
            pick(fig, hx, hy - 1, 160 * out, -side)


def hollowed_side(fig, kit, pose):
    """A Hollowed side on, facing left: the far arm and leg first, the body, the near leg, the near
    arm with the weapon in it, then the head."""
    b, g = pose["bob"], fig.ground + 0.5
    lean = pose["lean"]
    stoop = kit.get("hunch", 0)
    robe = kit["body"] == "robe"
    cx = 16.0
    top = cx - lean * 0.6 - stoop * 0.8
    sy = SHOULDER_Y + b + stoop

    def leg(fwd, lift, ramp, bias=0.0):
        foot = cx - fwd * 0.9
        knee = (cx - fwd * 0.55 - lift * 0.5, 37 + b * 0.5 - lift * 0.6)
        fig.limb([(cx - lean * 0.2, HIP_Y + b), knee, (foot, g - 1.2 - lift)], [2.3, 2.0, 1.8], ramp, bias=bias)
        shin = DOOM_IRON if kit["body"] == "armour" else LEATHER
        fig.limb([knee, (foot, g - 1.6 - lift)], [1.9, 1.9], shin, bias=bias, seam=False)
        fig.ball(foot - 0.9, g - 1.0 - lift, 2.8, 1.5, BOOT, bias=bias)

    def arm(fwd, up, sleeve, bias=0.0):
        sx = top + 0.8
        hx = sx - fwd * 0.8 - max(0.0, up) * 0.25
        hy = BELT_Y + 1 + b - up * 1.15
        ex, ey = (sx + hx) / 2 + 0.9 + up * 0.1, (sy + hy) / 2 + 0.3
        if robe:
            fig.limb([(sx, sy), (ex, ey), (hx, hy - 0.4)], [2.0, 2.2, 2.6], sleeve, bias=bias)
            fig.ball(hx - 0.3, hy + 0.8, 1.5, 1.5, ASH, bias=bias)
        else:
            fig.limb([(sx, sy), (ex, ey)], [2.0, 1.8], sleeve, bias=bias)
            fig.limb([(ex, ey), (hx, hy)], [1.7, 1.5], DOOM_IRON if kit["body"] == "armour" else ASH, bias=bias, seam=False)
            fig.ball(hx, hy + 0.6, 1.6, 1.6, ASH, bias=bias)
        return hx, hy + 0.6

    (near_fwd, near_lift), (far_fwd, far_lift) = pose["feet"]
    (w_fwd, w_up), (o_fwd, o_up) = pose["hands"]
    sleeve = HEX_ROBE if robe else DRAB if kit["body"] == "armour" else DOOM_CLOTH
    if kit.get("lantern"):
        lx, ly = top + 0.8 - o_fwd * 0.8 + 1, BELT_Y + 1 + b - o_up * 1.15
        lantern(fig, int(lx), int(ly + 0.6), kit.get("glow", 0) + pose["flare"] * 2)
    arm(o_fwd, o_up, shifted(sleeve, 1), -0.1)
    if not robe:
        leg(far_fwd, far_lift, shifted(DRAB, 1), -0.1)
        leg(near_fwd, near_lift, DRAB)
    else:
        for fwd, lift in ((far_fwd, far_lift), (near_fwd, near_lift)):
            if lift or fwd > 0:
                fig.ball(cx - fwd * 0.9 - 1.2, g - 1.0 - lift, 2.6, 1.4, BOOT)

    if robe:
        sway = (near_fwd - far_fwd) * 0.25
        fig.plate([(top - 3.6, 21 + b), (top + 3.8, 21 + b), (cx + 5.8 + sway, g + 0.4), (cx - 6.4 + sway, g + 0.4)],
                  HEX_ROBE, facing=(-0.2, -0.1, 0.97))
        shade_plate(fig, cx - 0.3 + sway * 0.5, 21 + b, g - 0.6, 3.7, 6.1, HEX_ROBE)
        for y in range(int(30 + b), int(g - 1)):
            fig.dot(int(cx + 1 + (y - 30) * 0.12 + sway * 0.5), y, HEX_ROBE[1])
        for x in range(int(cx - 7), int(cx + 7)):
            if fig.has(x, int(g - 1)):
                fig.dot(x, int(g - 1), HEX_ROBE[0])
        for y in range(int(22 + b), int(39 + b)):
            fig.dot(int(top - 3 + (y - 22) * 0.1), y, DOOM_CLOTH[2])
        for x in range(int(top - 4), int(top + 4)):
            fig.dot(x, int(BELT_Y + b), LEATHER[2] if x % 2 else LEATHER[1])
    elif kit["body"] == "armour":
        fig.limb([(top, 24.5 + b), (cx - lean * 0.3, 28.5 + b)], [4.4, 4.0], DOOM_IRON)
        fig.plate([(top - 3.4, 23 + b), (top - 0.5, 23 + b), (cx - 1.0, 36.5 + b), (cx - 4.6 - max(0, near_fwd) * 0.3, 36.5 + b)],
                  DOOM_CLOTH, facing=(-0.3, 0.1, 0.95))
        for x, y in ((top - 2, 25), (top - 2, 26), (top - 1, 27)):
            fig.dot(int(x), int(y + b), DOOM_BRONZE[2])
        for x, y in ((top + 1, 25), (top + 2, 26), (top + 2, 27), (top + 3, 28)):
            fig.dot(int(x), int(y + b), CRACK)
            fig.dot(int(x) - 1, int(y + b), DOOM_IRON[4])
        for x in range(int(top - 4), int(top + 4)):
            fig.dot(x, int(BELT_Y + b), DOOM_BRONZE[2] if x < top else DOOM_BRONZE[1])
    else:
        fig.limb([(top + 0.5, 24.5 + b + stoop * 0.5), (cx - lean * 0.3, 28.5 + b)], [4.5, 4.1], LEATHER)
        fig.plate([(top - 4.3, 27.5 + b), (top - 1.2, 27.5 + b), (cx - 1.5, 38 + b), (cx - 5.0, 38 + b)],
                  shifted(LEATHER, 1), facing=(-0.4, 0.1, 0.9))
        for x in range(int(top - 4), int(top + 4)):
            fig.dot(x, int(BELT_Y + b), LEATHER[0])
        fig.dot(int(top + 2), int(BELT_Y + b) + 1, LEATHER[2])
        fig.dot(int(top + 3), int(BELT_Y + b) + 1, LEATHER[2])
        fig.dot(int(top + 2), int(BELT_Y + b) + 2, LEATHER[1])
    if kit["body"] in ("armour", "jerkin"):
        fig.ball(top + 0.8, sy - 0.3, 3.1, 2.6, DOOM_IRON)
        fig.dot(int(top + 0.8), int(sy + 1.6), DOOM_BRONZE[1])
        fig.dot(int(top + 1.8), int(sy - 1.5), CRACK)

    angle = pose["weapon"]
    if pose["carry"] and kit["weapon"] == "pick":
        w_fwd, w_up = w_fwd * 0.3 + 1, w_up + 5
    hx, hy = arm(w_fwd, w_up, sleeve)
    if kit["weapon"] == "sword":
        sword(fig, hx, hy, angle, 10)
    elif kit["weapon"] == "staff":
        staff(fig, hx, hy, 4 + max(0, angle - 20) * 0.3 if angle < 150 else -12, kit.get("glow", 0) + pose["flare"])
        if pose["flare"]:
            sparks(fig, hx - 1, hy - 1, pose["flare"])
    else:
        pick(fig, hx, hy, 200 if pose["carry"] else angle + 20 if angle < 150 else 200)
    head_y = HEAD_Y + b + stoop + pose["tip"]
    face(fig, kit, top - 1.0, head_y, pose, LEFT)
    headgear(fig, kit, top - 1.0, head_y, LEFT)


def hollowed_downed(fig, kit):
    """A Hollowed fallen: crumbled to a heap of ash with its gear in it and on it, and a last
    thread of smoke going up."""
    g = fig.ground + 0.5
    cx = 16.0
    if kit["weapon"] == "staff":
        staff(fig, 16, g - 4, 100, 0, below=10, above=6)
    fig.ball(cx, g - 3.2, 11.0, 4.0, ASH_HEAP, rough=0.5)
    fig.ball(cx - 2.5, g - 5.4, 6.8, 3.6, ASH_HEAP, rough=0.5, seam=False)
    if kit["body"] == "robe":
        fig.plate([(cx - 10, g - 0.5), (cx - 6, g - 5), (cx - 1, g - 6.5), (cx + 3, g - 4), (cx + 6, g - 0.5)],
                  HEX_ROBE, facing=(-0.2, -0.6, 0.8))
        fig.plate([(cx - 3, g - 6.5), (cx + 1, g - 9.5), (cx + 3.5, g - 5)], HEX_ROBE, facing=(0.2, -0.5, 0.8))
    elif kit["body"] == "armour":
        fig.ball(cx + 5, g - 2.6, 3.2, 2.2, DOOM_IRON)
        fig.dot(int(cx + 5), int(g - 4), CRACK)
        fig.plate([(cx - 7, g - 1), (cx - 4, g - 4), (cx, g - 3), (cx - 1, g - 0.5)], DOOM_CLOTH, facing=(-0.2, -0.6, 0.8))
    else:
        fig.plate([(cx - 7, g - 1), (cx - 4, g - 4), (cx + 1, g - 3), (cx, g - 0.5)], LEATHER, facing=(-0.2, -0.6, 0.8))
    for i in range(9):
        x = int(cx - 9 + hash01(i, 0, 61) * 18)
        y = int(g - 1 - hash01(i, 1, 61) * 4)
        if fig.has(x, y):
            fig.dot(x, y, ASH_HEAP[3])
    if kit["head"] == "helm":
        shaped = {}
        hx, hy = cx + 2, g - 5.5
        for y in range(int(hy - 5), int(hy + 2)):
            for x in range(int(hx - 6), int(hx + 6)):
                nx, ny = (x + 0.5 - hx) / 5.0, (y + 0.5 - (hy + 1.5)) / 5.5
                if nx * nx + ny * ny <= 1.0:
                    shaped[(x, y)] = (nx, ny, math.sqrt(max(0.0, 1 - nx * nx - ny * ny)))
        fig.lay(shaped, DOOM_IRON)
        for x in range(int(hx - 7), int(hx + 7)):
            fig.dot(x, int(hy + 1), DOOM_IRON[2] if x < hx else DOOM_IRON[1])
            fig.dot(x, int(hy + 2), DOOM_BRONZE[1])
        fig.dot(int(hx + 1), int(hy - 3), CRACK)
        fig.dot(int(hx + 2), int(hy - 2), CRACK)
        sword(fig, cx - 11, g - 2.6, -85, 10)
    elif kit["head"] == "cap":
        fig.ball(cx + 1, g - 5.5, 4.4, 2.8, LEATHER)
        for x in range(int(cx - 3), int(cx + 5)):
            fig.dot(x, int(g - 4), DOOM_IRON[2])
        pick(fig, cx - 6, g - 2.6, -95, -1)
        lantern(fig, int(cx + 7), int(g - 8), 0)
    smoke(fig, int(cx - 2), int(g - 9), 0.3, 0.6, 0.2)


def hollowed(kit):
    """A frame-drawer for one kind of Hollowed, from its kit: what's on its head (helm, hood or
    cap), what it wears (armour, robe or jerkin), what it carries (sword, staff or pick), whether
    it has a lantern, how much it stoops, and how bright what it carries glows."""
    def draw(fig, facing, column):
        if column == DOWNED:
            hollowed_downed(fig, kit)
            return
        pose = biped_pose(column)
        if facing == LEFT:
            hollowed_side(fig, kit, pose)
        else:
            hollowed_front(fig, kit, pose, facing)
    return draw


#: The three Hollowed: a footman in cracked Doomward plate and a kettle helm with a short sword, a
#: hexer in a scholar's robe and a deep hood with a staff of black crystal, and a stooping sapper
#: in leather with a pick and a lantern.
FOOTMAN = {"head": "helm", "body": "armour", "weapon": "sword"}
HEXER = {"head": "hood", "body": "robe", "weapon": "staff", "glow": 0.4}
SAPPER = {"head": "cap", "body": "jerkin", "weapon": "pick", "lantern": True, "hunch": 1, "glow": 0}


# --------------------------------------------------------------------------- #
# The Thornmother
# --------------------------------------------------------------------------- #

#: Her fur, a dark bear-brown with the Mirewood's violet sunk into its shadows; her muzzle, a
#: little paler; her claws; the brambles growing out through her; the black veins under her coat;
#: her eyes; and the outline.
BEAR_FUR = ((16, 10, 20), (28, 19, 29), (44, 30, 40), (64, 44, 52), (90, 64, 66), (126, 96, 86))
BEAR_MUZZLE = ((52, 38, 42), (80, 60, 56), (110, 86, 72), (140, 112, 90))
CLAW = ((150, 140, 128), (214, 206, 188))
THORN = ((44, 20, 26), (92, 44, 42), (140, 80, 60), (220, 196, 156))
VEIN = (104, 58, 168)
CRYSTAL_FACE = ((10, 6, 18), (22, 14, 38), (40, 26, 68))
BEAR_EYE = ((190, 120, 255), (248, 230, 255))
BEAR_OUTLINE = (18, 10, 22)

#: How far left of where she's drawn side on her body goes (right, if it's less than nothing), so
#: she fits the frame from her nose to her rump.
BEAR_SHIFT = -2


def bear_pose(column):
    """How the Thornmother stands in a frame: drop and push back like the wolves, her feet, how
    far her front end's reared up (degrees round her hips), her head and jaw, whether her eyes are
    open, and how bright her crystals burn."""
    pose = {"drop": 0, "back": 0, "feet": {"fl": (0, 0), "fr": (0, 0), "hl": (0, 0), "hr": (0, 0)},
            "rear": 0, "head": (0, 0), "tilt": 0, "jaw": 0.5, "eyes": True, "burn": 0.3, "reach": 0}
    if 1 <= column <= 4:
        # A heavy amble: the near pair together, then the far pair.
        i = column - 1
        swing = (-5, 0, 5, 0)[i]
        lift_a, lift_b = (0, 0, 0, 3)[i], (0, 3, 0, 0)[i]
        pose["feet"] = {"fl": (swing, lift_a), "hl": (swing, lift_a), "fr": (-swing, lift_b), "hr": (-swing, lift_b)}
        pose["drop"] = (1, 0, 1, 0)[i]
        pose["head"] = (0, (1, 0, 1, 0)[i])
    elif column == WIND_UP:
        pose.update(rear=18, back=0, head=(0, -2), tilt=10, jaw=1, burn=0.6, reach=1,
                    feet={"fl": (0, 0), "fr": (0, 0), "hl": (2, 0), "hr": (3, 0)})
    elif column == STRIKE:
        pose.update(rear=-5, back=-1, drop=2, head=(2, 2), tilt=8, jaw=2, burn=0.6,
                    feet={"fl": (-10, 0), "fr": (-7, 0), "hl": (3, 0), "hr": (4, 0)})
    elif column == CAST:
        pose.update(rear=6, back=1, head=(1, -3), tilt=34, jaw=2, burn=1.0,
                    feet={"fl": (-2, 0), "fr": (0, 0), "hl": (1, 0), "hr": (2, 0)})
    elif column == HURT:
        pose.update(rear=8, back=0, head=(2, -2), tilt=18, jaw=1, eyes=False, burn=0.1,
                    feet={"fl": (-1, 3), "fr": (1, 0), "hl": (2, 0), "hr": (2, 0)})
    return pose


def thorn(fig, x, y, angle, length, width=3.0):
    """A bramble thorn growing out through the fur at (x, y): a dark woody spike, lit down its
    left edge, its tip pale. The angle is from straight up, towards the back positive."""
    a = math.radians(angle)
    dx, dy = math.sin(a), -math.cos(a)
    px, py = -dy, dx
    tip = (x + dx * length, y + dy * length)
    base_l = (x - px * width / 2, y - py * width / 2)
    base_r = (x + px * width / 2, y + py * width / 2)
    mid = (x + dx * length * 0.35, y + dy * length * 0.35)
    fig.plate([base_l, tip, mid], THORN[:3], facing=(-0.6, -0.5, 0.6), seam=False)
    fig.plate([mid, tip, base_r], THORN[:3], facing=(0.5, 0.2, 0.8), seam=False)
    for k in (1.0, 1.6):
        fig.dot(int(math.floor(tip[0] - dx * k)), int(math.floor(tip[1] - dy * k)), THORN[3] if k < 1.5 else THORN[2])


def shard(fig, x, y, angle, length, width, burn):
    """A spike of black crystal grown out of her at (x, y): a long faceted point, its lit face a
    dull violet-black and its shadowed face black, a violet edge where the light catches it, and a
    glow off its tip as bright as she's burning."""
    a = math.radians(angle)
    dx, dy = math.sin(a), -math.cos(a)
    px, py = -dy, dx
    tip = (x + dx * length, y + dy * length)
    shoulder = 0.72
    left = (x - px * width / 2, y - py * width / 2)
    right = (x + px * width / 2, y + py * width / 2)
    left_top = (x - px * width / 2 + dx * length * shoulder, y - py * width / 2 + dy * length * shoulder)
    right_top = (x + px * width / 2 + dx * length * shoulder, y + py * width / 2 + dy * length * shoulder)
    ridge = (x + px * width * 0.1, y + py * width * 0.1)
    ridge_top = (ridge[0] + dx * length * 0.8, ridge[1] + dy * length * 0.8)
    fig.plate([left, left_top, tip, ridge_top, ridge], CRYSTAL_FACE, facing=(-0.6, -0.4, 0.7))
    fig.plate([ridge, ridge_top, tip, right_top, right], CRYSTAL_FACE[:2] + (CRYSTAL_FACE[0],), facing=(0.7, 0.2, 0.6), seam=False)
    # The edge the light catches, up the lit side to the point.
    steps = int(length)
    for i in range(1, steps):
        t = i / steps
        ex = left[0] + (left_top[0] - left[0]) * min(1, t / shoulder) + (tip[0] - left_top[0]) * max(0, (t - shoulder) / (1 - shoulder))
        ey = left[1] + (left_top[1] - left[1]) * min(1, t / shoulder) + (tip[1] - left_top[1]) * max(0, (t - shoulder) / (1 - shoulder))
        if fig.has(int(math.floor(ex + px * 0.6)), int(math.floor(ey + py * 0.6))):
            fig.dot(int(math.floor(ex + px * 0.6)), int(math.floor(ey + py * 0.6)), BLACK_CRYSTAL[2] if t > 0.45 else BLACK_CRYSTAL[1])
    tx, ty = int(math.floor(tip[0] - dx)), int(math.floor(tip[1] - dy))
    if burn > 0.2:
        fig.dot(tx, ty, BLACK_CRYSTAL[3])
    halo = int(40 + 180 * burn)
    for ox in range(-3, 4):
        for oy in range(-3, 4):
            d = math.hypot(ox, oy)
            if 0 < d <= 1 + burn * 2.5 and not fig.has(tx + ox, ty + oy):
                fig.glow(tx + ox, ty + oy, HEX_GLOW + (int(halo * (1 - d / (2 + burn * 2.5))),))


def veins(fig, points, seed):
    """Black veins running under her coat along a line of points, branching off it here and
    there, the odd one lit violet from inside."""
    for i in range(len(points) - 1):
        (ax, ay), (bx, by) = points[i], points[i + 1]
        steps = int(max(abs(bx - ax), abs(by - ay))) + 1
        for k in range(steps):
            t = k / steps
            x, y = int(math.floor(ax + (bx - ax) * t)), int(math.floor(ay + (by - ay) * t))
            if fig.has(x, y):
                fig.dot(x, y, VEIN if 0.15 < t < 0.85 or i else BLACK_CRYSTAL[1])
                if hash01(x, y, seed) < 0.1:
                    fig.dot(x, y, BLACK_CRYSTAL[2])
            if hash01(x, y, seed + 1) < 0.12:
                for j in range(1, 3):
                    bx2, by2 = x + j, y + j * (1 if hash01(x, y, seed + 2) < 0.5 else -1)
                    if fig.has(bx2, by2):
                        fig.dot(bx2, by2, BLACK_CRYSTAL[1])


def bear_side(fig, column, pose):
    """The Thornmother side on, facing left: a great humped she-bear, head low and heavy, legs like
    trunks, brambles bristling along her back and black crystal grown out of her shoulders and
    haunch."""
    d, b, rear = pose["drop"], pose["back"] - BEAR_SHIFT, pose["rear"]
    feet = pose["feet"]
    fur, far = BEAR_FUR, shifted(BEAR_FUR, 1)
    g = fig.ground + 0.5
    hip = (70 + b, 58 + d)

    def up(x, y):
        """A point on her front half, turned up round her hips as she rears."""
        return rotate((x + b, y + d), hip, rear)

    def foreleg(x, foot, ramp, bias=0.0):
        fx, lift = foot
        sx, sy = up(x, 56)
        if rear > 12 or pose["reach"]:
            # Reared: the forelegs up and reaching, claws out.
            px, py = sx - 13, sy + 2
            fig.limb([(sx, sy), (sx - 7, sy + 5), (px, py)], [7.0, 5.8, 5.0], ramp, bias=bias, rough=0.2)
            paw = (px - 2.5, py + 0.5)
            fig.ball(paw[0], paw[1], 4.6, 4.0, ramp, bias=bias)
            for i in range(3):
                fig.line(int(paw[0] - 4), int(paw[1] - 2 + i * 2), int(paw[0] - 6), int(paw[1] - 1 + i * 2), CLAW[1])
            return
        footx = x + fx - 1 - BEAR_SHIFT
        fig.limb([(sx, sy), ((sx + footx) / 2 - 0.5, (sy + g) / 2 - lift * 0.4), (footx, g - 6.5 - lift)],
                 [7.2, 6.0, 5.6], ramp, bias=bias, rough=0.2)
        fig.ball(footx - 1.5, g - 2.2 - lift, 6.8, 2.8, ramp, bias=bias)
        for i in range(3):
            fig.dot(int(footx - 7 + i * 2.2), int(g - 1 - lift), CLAW[1] if i else CLAW[0])
            fig.dot(int(footx - 7.5 + i * 2.2), int(g - lift), CLAW[0])

    def hindleg(x, foot, ramp, bias=0.0):
        fx, lift = foot
        footx = x + fx - BEAR_SHIFT
        fig.limb([(x + b, 56 + d), (x + 3 + b * 0.5 + fx * 0.3, 65), (footx + 1, g - 6.8 - lift)],
                 [9.0, 6.8, 6.0], ramp, bias=bias, rough=0.2)
        fig.ball(footx - 1, g - 2.2 - lift, 7.0, 2.8, ramp, bias=bias)
        for i in range(3):
            fig.dot(int(footx - 7 + i * 2.2), int(g - 1 - lift), CLAW[0])

    # The far side first, a shade darker.
    foreleg(39, feet["fr"], far, -0.1)
    hindleg(73, feet["hr"], far, -0.1)

    # Her body: the haunch, the barrel of her, the great hump over her shoulders and her chest.
    fig.ball(70 + b, 47 + d, 15, 14.5, fur, rough=0.2, seam=False)
    sx, sy = up(42, 45)
    fig.limb([(sx, sy), (68 + b, 46 + d)], [15, 14], fur, rough=0.2, seam=False)
    hx, hy = up(42, 39.5)
    fig.ball(hx, hy, 15, 13, fur, rough=0.2, seam=False)
    cx_, cy_ = up(34, 50)
    fig.ball(cx_, cy_, 12, 11.5, fur, rough=0.2, seam=False)
    fig.ball(85 + b, 41 + d, 3.2, 3.2, fur)
    ruff(fig, int(min(hx, sx) - 12), int(84 + b), fur, 77, every=0.5, tall=2)
    veins(fig, [up(36, 42), up(46, 47), (58 + b, 44 + d), (66 + b, 52 + d)], 3)
    veins(fig, [up(30, 50), up(36, 56)], 9)

    # Brambles through her back and flanks, and black crystal out of her hump and haunch.
    for i, (x, y, ang, ln) in enumerate(((54, 32, 25, 12), (60, 33, 38, 13), (66, 34, 48, 10), (77, 35, 58, 12),
                                         (83, 40, 78, 9), (58, 48, 95, 7), (47, 51, 125, 6), (79, 51, 105, 7),
                                         (26, 42, -60, 7), (33, 55, -110, 6))):
        tx, ty = (x + b, y + d) if x > 50 else up(x, y)
        thorn(fig, tx, ty, ang + rear * 0.5, ln, 4.0 if ln > 9 else 3.0)
    for x, y, ang, ln, w in ((36, 30, -18, 17, 7), (44, 29, 8, 21, 8), (51, 32, 30, 12, 5), (30, 35, -40, 9, 4)):
        tx, ty = up(x, y)
        shard(fig, tx, ty, ang - rear * 0.6, ln, w, pose["burn"])
    shard(fig, 70 + b, 34 + d, 20, 11, 5, pose["burn"])
    shard(fig, 75 + b, 36 + d, 48, 7, 4, pose["burn"])

    foreleg(36, feet["fl"], fur)
    hindleg(70, feet["hl"], fur)
    bear_head_side(fig, column, pose, up)


def bear_head_side(fig, column, pose, up):
    """Her head side on: broad and heavy, small round ears, a short thick muzzle, the eye a violet
    coal with a black streak under it, and her jaws open to roar."""
    hx, hy = pose["head"]
    tilt = pose["tilt"]
    jaw = pose["jaw"]
    fur = BEAR_FUR
    nx, ny = up(30, 46)
    pivot = (nx - 6 + hx, ny + hy)

    def at(x, y):
        return rotate((nx - 10 + x + hx, ny - 1 + y + hy), pivot, tilt + pose["rear"] * 0.3)

    fig.limb([(nx + 4, ny), at(2, 1)], [10.5, 9.0], fur, seam=False, rough=0.2)
    ex_, ey_ = at(5.5, -8.5)
    fig.ball(ex_, ey_, 3.2, 3.2, shifted(fur, 1))
    sx, sy = at(0, 0)
    fig.ball(sx, sy, 11.5, 10.0, fur, rough=0.2)
    if jaw >= 1:
        gape = jaw * 2.2
        fig.limb([at(-3, 6), at(-12, 7 + gape)], [3.2, 2.6], shifted(BEAR_MUZZLE, 1))
        for i in range(7):
            mx, my = at(-5 - i, 4.6 + gape * 0.5)
            fig.dot(int(mx), int(my), MAW)
            fig.dot(int(mx), int(my) + 1, MAW if i % 3 else (64, 14, 30))
        for i in range(3):
            tx, ty = at(-7.5 - i * 2.2, 3.6)
            fig.dot(int(tx), int(ty) + 1, FANG[1])
            tx, ty = at(-7.5 - i * 2.2, 5.8 + gape)
            fig.dot(int(tx), int(ty) - 1, FANG[0])
    fig.limb([at(-4, 2.5), at(-14.5, 3.8)], [5.8, 4.4], BEAR_MUZZLE, seam=False)
    if jaw < 1:
        fig.limb([at(-4, 6.4), at(-12, 6.6)], [2.8, 2.2], shifted(BEAR_MUZZLE, 1), seam=False)
        # A snarl: the lip curled back off her fangs.
        for i in range(5):
            mx, my = at(-7 - i * 1.4, 5.2)
            fig.dot(int(mx), int(my), MAW if i % 2 else BEAR_OUTLINE)
        for i in range(3):
            fx_, fy_ = at(-8 - i * 2.2, 5.4)
            fig.dot(int(fx_), int(fy_) + 1, FANG[1])
    tx, ty = at(-18, 2.4)
    fig.ball(tx, ty, 2.2, 1.8, ((20, 12, 20), (36, 24, 32), (70, 56, 64)))
    ex_, ey_ = at(-1.5, -9.2)
    fig.ball(ex_, ey_, 3.4, 3.4, fur)
    fig.ball(ex_, ey_ + 0.6, 1.6, 1.6, shifted(BEAR_MUZZLE, 1), seam=False)
    ex, ey = at(-6, -2.5)
    ex, ey = int(ex), int(ey)
    for i in range(4):
        fig.dot(ex - 1 + i, ey - 1 - (1 if i == 3 else 0), BEAR_OUTLINE)
    if pose["eyes"]:
        fig.dot(ex, ey, BEAR_EYE[1])
        fig.dot(ex + 1, ey, BEAR_EYE[0])
        fig.dot(ex, ey + 1, BEAR_EYE[0])
        fig.glow(ex - 1, ey, BEAR_EYE[0] + (130,))
        fig.glow(ex - 2, ey, BEAR_EYE[0] + (60,))
        fig.glow(ex + 2, ey, BEAR_EYE[0] + (70,))
        for i in range(2, 5):
            fig.dot(ex + (1 if i > 3 else 0), ey + i, BLACK_CRYSTAL[1])
    else:
        fig.dot(ex, ey, BEAR_OUTLINE)
        fig.dot(ex + 1, ey, BEAR_OUTLINE)


def bear_front(fig, column, pose):
    """The Thornmother coming at you: her shoulders and their crystals towering behind a broad
    head, the muzzle and both burning eyes, and her forelegs planted either side of her chest."""
    d, rear = pose["drop"], pose["rear"]
    feet = pose["feet"]
    fur, far = BEAR_FUR, shifted(BEAR_FUR, 1)
    g = fig.ground + 0.5
    cx = 48
    lift = max(0, rear) * 0.4
    lunge = max(0, -rear) * 0.6 + (2 if pose["back"] < 0 else 0)
    for side in (-1, 1):
        fx, lft = feet["hl" if side < 0 else "hr"]
        x = cx + side * 21
        fig.limb([(x, 54 + d), (x + side, g - 7 - lft)], [7, 5.5], far, bias=-0.1, rough=0.2)
        fig.ball(x + side, g - 5 - lft, 6, 2.6, far, bias=-0.1)
    fig.ball(cx, 46 + d - lift * 0.4, 26, 15, far, rough=0.2)
    for x, y, ang, ln, w in ((cx - 13, 37, -28, 14, 7), (cx - 4, 33, -6, 19, 8), (cx + 7, 34, 14, 16, 7),
                             (cx + 16, 39, 38, 10, 5), (cx - 21, 42, -55, 8, 4)):
        shard(fig, x, y + d - lift, ang, ln, w, pose["burn"])
    for x, y, ang, ln in ((cx - 24, 46, -70, 7), (cx + 24, 47, 70, 7), (cx - 18, 36, -35, 6), (cx + 20, 38, 40, 7)):
        thorn(fig, x, y + d - lift * 0.5, ang, ln)
    ruff(fig, cx - 26, cx + 26, far, 31, every=0.35, tall=1)
    fig.ball(cx, 60 + d - lift * 0.5 + lunge * 0.3, 15, 10, fur, rough=0.2)
    veins(fig, [(cx - 6, 54 + d), (cx - 2, 60 + d), (cx - 4, 66 + d)], 13)
    for side in (-1, 1):
        fx, lft = feet["fl" if side < 0 else "fr"]
        x = cx + side * 11
        if rear > 12:
            # Reared: paws up beside her head, claws out.
            px, py = cx + side * 24, 34 - lift * 0.3
            fig.limb([(x, 54 - lift * 0.5), (cx + side * 20, 46 - lift * 0.5), (px, py)], [7, 6, 5], fur, rough=0.2)
            fig.ball(px, py - 1, 5, 4.5, fur)
            for i in range(3):
                fig.line(int(px - 3 + i * 3), int(py - 6), int(px - 3 + i * 3), int(py - 4), CLAW[1])
            continue
        foot_y = g - 3 - lft - (fx > 0) * 1
        fig.limb([(x, 58 + d), (x + side * 0.5, 66), (x, foot_y - 3.5)], [7.4, 6.4, 6.0], fur, rough=0.2)
        fig.ball(x, foot_y + 0.6, 7.2, 3.0, fur)
        for i in range(4):
            fig.dot(int(x - 3 + i * 2), int(foot_y + 3.2), CLAW[1] if i % 2 else CLAW[0])
    bear_head_front(fig, column, pose, cx + pose["head"][0], 42 + d + pose["head"][1] - lift + lunge)


def bear_head_front(fig, column, pose, cx, cy):
    """Her head facing you: broad, round-eared, thorns through her brow, the pale muzzle, a black
    nose, and the eyes burning violet with black running from them."""
    fur = BEAR_FUR
    raise_ = pose["tilt"] / 10.0
    for side in (-1, 1):
        fig.ball(cx + side * 11, cy - 10, 4.6, 4.6, fur)
        fig.ball(cx + side * 11, cy - 9.4, 2.4, 2.4, shifted(BEAR_MUZZLE, 1), seam=False)
    fig.ball(cx, cy, 14.5, 12, fur, rough=0.2)
    for side in (-1, 1):
        fig.ball(cx + side * 10.5, cy + 4.5, 5, 5.5, fur, rough=0.2, seam=False)
    thorn(fig, cx - 6, cy - 8, -25, 6, 2.5)
    thorn(fig, cx + 4, cy - 9.5, 15, 7, 2.5)
    my = cy + 5.5 - raise_
    fig.ball(cx, my, 7.6, 5.8, BEAR_MUZZLE)
    fig.ball(cx, my - 3.2, 2.6, 1.8, ((20, 12, 20), (36, 24, 32), (70, 56, 64)))
    if pose["jaw"] >= 1:
        rows = 2 + int(pose["jaw"])
        for r in range(rows):
            for x in range(int(cx) - 4 + (r == rows - 1), int(cx) + 4 - (r == rows - 1)):
                fig.dot(x, int(my + 1 + r), MAW if r else (64, 14, 30))
        for x in (int(cx) - 4, int(cx) - 2, int(cx) + 1, int(cx) + 3):
            fig.dot(x, int(my + 1), FANG[1])
            fig.dot(x, int(my + rows), FANG[0])
    else:
        # A snarl: lips back off a row of fangs.
        for x in range(int(cx) - 4, int(cx) + 4):
            fig.dot(x, int(my + 1), BEAR_OUTLINE)
            fig.dot(x, int(my + 2), FANG[1] if x % 2 else MAW)
    ey = int(cy - 2 - raise_)
    for side in (-1, 1):
        x = int(cx) + (-6 if side < 0 else 4)
        for i in range(-1, 3):
            fig.dot(x + i, ey - 1 - (1 if (i == -1 and side < 0) or (i == 2 and side > 0) else 0), BEAR_OUTLINE)
        if pose["eyes"]:
            fig.dot(x, ey, BEAR_EYE[1] if side < 0 else BEAR_EYE[0])
            fig.dot(x + 1, ey, BEAR_EYE[0] if side < 0 else BEAR_EYE[1])
            fig.glow(x - 1, ey, BEAR_EYE[0] + (90,))
            fig.glow(x + 2, ey, BEAR_EYE[0] + (90,))
            for i in range(1, 4):
                fig.dot(x + (0 if side < 0 else 1), ey + i, VEIN)
        else:
            fig.dot(x, ey, BEAR_OUTLINE)
            fig.dot(x + 1, ey, BEAR_OUTLINE)


def bear_back(fig, column, pose):
    """The Thornmother going away: her rump nearest with its stub of a tail, her back humping up
    to the shoulders and their crystals, and the back of her head and ears beyond."""
    d, rear = pose["drop"], pose["rear"]
    feet = pose["feet"]
    fur, far = BEAR_FUR, shifted(BEAR_FUR, 1)
    g = fig.ground + 0.5
    cx = 48
    lift = max(0, rear) * 0.4
    for side in (-1, 1):
        fx, lft = feet["fl" if side < 0 else "fr"]
        x = cx + side * 15
        fig.ball(x, g - 6 - lft, 6, 2.6, far, bias=-0.1)
        if rear > 12:
            fig.limb([(cx + side * 16, 40 - lift), (cx + side * 24, 30 - lift)], [6, 5], far, rough=0.2)
            for i in range(3):
                fig.line(int(cx + side * 24 - 3 + i * 3), int(25 - lift), int(cx + side * 24 - 3 + i * 3), int(23 - lift), CLAW[1])
    hcy = 26 + d + pose["head"][1] - lift
    for side in (-1, 1):
        fig.ball(cx + side * 9, hcy - 7, 3.8, 3.8, fur)
    fig.ball(cx + pose["head"][0], hcy, 11, 9, fur, rough=0.25)
    fig.ball(cx, 40 + d - lift * 0.6, 22, 13, fur, rough=0.2)
    for x, y, ang, ln, w in ((cx - 10, 33, -25, 13, 6), (cx + 1, 31, 4, 17, 7), (cx + 11, 34, 30, 12, 6)):
        shard(fig, x, y + d - lift, ang, ln, w, pose["burn"])
    fig.ball(cx, 54 + d, 21, 13, fur, rough=0.2)
    ruff(fig, cx - 22, cx + 22, fur, 37, every=0.4, tall=1)
    for x, y, ang, ln in ((cx - 14, 44, -40, 7), (cx - 6, 45, -10, 8), (cx + 5, 44, 15, 8), (cx + 15, 46, 45, 7),
                          (cx - 19, 54, -75, 6), (cx + 20, 55, 75, 6)):
        thorn(fig, x, y + d, ang, ln)
    veins(fig, [(cx - 2, 42 + d), (cx + 3, 50 + d), (cx + 1, 58 + d)], 17)
    fig.ball(cx, 50 + d, 3.2, 3.0, fur)
    for side in (-1, 1):
        fx, lft = feet["hl" if side < 0 else "hr"]
        x = cx + side * 13
        foot_y = g - 3 - lft - (fx < 0) * 1
        fig.limb([(x, 60 + d), (x + side, 67), (x, foot_y - 3.7)], [8.5, 6.8, 6.2], fur, rough=0.2)
        fig.ball(x, foot_y + 0.6, 7.2, 3.0, fur)


def bear_downed(fig, column):
    """The Thornmother brought down: slumped on her belly with her chin on the ground and her
    forelegs out, her eyes dark, her crystals gone dull and a crack through the biggest."""
    fur, far = BEAR_FUR, shifted(BEAR_FUR, 1)
    g = fig.ground + 0.5
    fig.limb([(30, 70), (14, g - 5.5)], [6, 5], far)
    fig.ball(68, 63, 16, 12.5, fur, rough=0.2, seam=False)
    fig.limb([(40, 64), (66, 63)], [12, 12], fur, rough=0.2, seam=False)
    fig.ball(40, 61, 14, 12, fur, rough=0.2, seam=False)
    ruff(fig, 26, 84, fur, 91, every=0.45, tall=1)
    for x, y, ang, ln, w in ((40, 50, -30, 14, 7), (48, 50, 10, 11, 6), (68, 51, 25, 9, 5)):
        shard(fig, x, y, ang, ln, w, 0.0)
    fig.line(42, 44, 40, 40, VEIN)
    for x, y, ang, ln in ((56, 52, 30, 7), (62, 51, 40, 6), (76, 52, 55, 7), (82, 57, 80, 5)):
        thorn(fig, x, y, ang, ln)
    fig.limb([(34, 71), (18, g - 5)], [6, 5], fur)
    for i in range(3):
        fig.dot(13 + i * 2, int(g - 1), CLAW[0])
    fig.limb([(70, 70), (82, g - 5.5)], [7, 5.5], fur)
    fig.ball(24, 66, 10, 8.5, fur, rough=0.25)
    fig.ball(28, 58, 3, 3, fur)
    fig.limb([(20, 70), (12, 72)], [4.6, 3.6], BEAR_MUZZLE, seam=False)
    fig.ball(9, 71, 1.8, 1.6, ((20, 12, 20), (36, 24, 32), (70, 56, 64)))
    for i in range(3):
        fig.dot(18 + i, 64, BEAR_OUTLINE)


def thornmother(fig, facing, column):
    """One frame of the Thornmother."""
    if column == DOWNED:
        bear_downed(fig, column)
        return
    pose = bear_pose(column)
    if facing == LEFT:
        bear_side(fig, column, pose)
    elif facing == DOWN:
        bear_front(fig, column, pose)
    else:
        bear_back(fig, column, pose)


# --------------------------------------------------------------------------- #
# The vault construct
# --------------------------------------------------------------------------- #

#: Dressed sandstone, warm in the light and cool in its shadows; the gold set into it; the sun's
#: light in its eyes and the heart in its chest; and its outline.
VAULT_STONE = ((50, 44, 60), (76, 66, 80), (106, 92, 98), (138, 120, 114), (170, 150, 132), (204, 186, 158))
VAULT_GOLD = ((112, 70, 26), (178, 124, 40), (230, 182, 74), (255, 234, 150))
SUN_LIGHT = ((255, 210, 110), (255, 246, 206))
VAULT_OUTLINE = (40, 30, 38)


def slab(fig, points, ramp, bevel=1.5, **kw):
    """A dressed stone with flat faces: a polygon whose edges are bevelled, the ones facing up and
    left catching the light and the ones facing down and right in shadow, flat across its face."""
    shaped = {}
    xs = [p[0] for p in points]
    ys = [p[1] for p in points]
    edges = []
    for i in range(len(points)):
        (ax, ay), (bx, by) = points[i], points[(i + 1) % len(points)]
        length = math.hypot(bx - ax, by - ay) or 1.0
        edges.append((ax, ay, bx, by, length))
    # Which way round the points go, so each edge's outward side is known.
    area = sum(points[i][0] * points[(i + 1) % len(points)][1] - points[(i + 1) % len(points)][0] * points[i][1]
               for i in range(len(points)))
    turn = 1 if area > 0 else -1
    for y in range(math.floor(min(ys)), math.ceil(max(ys)) + 1):
        for x in range(math.floor(min(xs)), math.ceil(max(xs)) + 1):
            px, py = x + 0.5, y + 0.5
            if not inside(points, px, py):
                continue
            best = None
            for ax, ay, bx, by, length in edges:
                t = max(0.0, min(1.0, ((px - ax) * (bx - ax) + (py - ay) * (by - ay)) / (length * length)))
                d = math.hypot(px - ax - (bx - ax) * t, py - ay - (by - ay) * t)
                if best is None or d < best[0]:
                    best = (d, (by - ay) / length * -turn, (bx - ax) / length * turn)
            d, nx, ny = best
            if d < bevel:
                shaped[(x, y)] = (nx * 0.8, ny * 0.8, 0.6)
            else:
                shaped[(x, y)] = (0.05, 0.05, 0.99)
    return fig.lay(shaped, ramp, **kw)


def sun(fig, x, y, burn):
    """The gold sun set in the construct's chest: a ring with rays, and the heart in the middle of
    it glowing as bright as it's burning."""
    for ox, oy in ((-2, -1), (-2, 0), (1, -1), (1, 0), (-1, -2), (0, -2), (-1, 1), (0, 1)):
        fig.dot(x + ox, y + oy, VAULT_GOLD[2] if ox < 0 or oy < -1 else VAULT_GOLD[1])
    for ox, oy in ((-1, -4), (0, -4), (-4, -1), (-4, 0), (3, -1), (3, 0), (-1, 3), (0, 3),
                   (-3, -3), (2, -3), (-3, 2), (2, 2)):
        fig.dot(x + ox, y + oy, VAULT_GOLD[3] if ox < 0 and oy < 0 else VAULT_GOLD[1])
    core = SUN_LIGHT[1] if burn > 0.4 else SUN_LIGHT[0]
    for ox, oy in ((-1, -1), (0, -1), (-1, 0), (0, 0)):
        fig.dot(x + ox, y + oy, core if (ox, oy) == (-1, -1) or burn > 0.4 else SUN_LIGHT[0])
    reach = 1 + int(burn * 5)
    for oy in range(-reach - 4, reach + 4):
        for ox in range(-reach - 4, reach + 4):
            d = math.hypot(ox + 0.5, oy + 0.5)
            if 4.5 < d <= 4.5 + reach:
                fig.glow(x + ox, y + oy, SUN_LIGHT[0] + (int(150 * burn * (1 - (d - 4.5) / (reach + 1))),))


def construct_eyes(fig, xs, y, open_, burn):
    """The light behind the visor: gold-white eyes, or a dull ember when it's hurt."""
    for i, x in enumerate(xs):
        fig.dot(x, y, SUN_LIGHT[1] if open_ and i % 2 == 0 else SUN_LIGHT[0] if open_ else VAULT_GOLD[0])
        fig.dot(x, y + 1, SUN_LIGHT[0] if open_ else VAULT_STONE[0])
    if open_:
        for x in (min(xs) - 1, max(xs) + 1):
            fig.glow(x, y, SUN_LIGHT[0] + (int(90 + 90 * burn),))
            fig.glow(x, y + 1, SUN_LIGHT[0] + (int(40 + 60 * burn),))


def construct_head(fig, cx, top, facing, pose, burn):
    """The construct's head: a squared helm of stone, a visor slit with the light behind it, and
    the Old Crown's three gold points on top."""
    side = facing == LEFT
    x0, x1 = (cx - 7, cx + 7) if not side else (cx - 7.5, cx + 5.5)
    slab(fig, [(x0 + 2, top), (x1 - 2, top), (x1, top + 2.5), (x1, top + 12), (x0, top + 12), (x0, top + 2.5)], VAULT_STONE)
    for i, ox in enumerate((-4, 0, 4) if not side else (-3, 1)):
        h = 3 if ox == 0 or (side and i == 1) else 2
        for k in range(h):
            fig.dot(int(cx + ox) - (0 if not side else 1), int(top) - 1 - k, VAULT_GOLD[3] if k == h - 1 else VAULT_GOLD[2])
            fig.dot(int(cx + ox) - (0 if not side else 1) - 1, int(top) - 1 - k, VAULT_GOLD[1] if k < h - 1 else VAULT_GOLD[2])
    for x in range(int(x0 + 1), int(x1)):
        fig.dot(x, int(top) + 1, VAULT_GOLD[2] if x < cx else VAULT_GOLD[1])
    if facing == UP:
        for y in range(int(top) + 3, int(top) + 11):
            fig.dot(int(cx) - 1, y, VAULT_STONE[1])
        return
    vy = int(top) + 6
    if side:
        for x in range(int(x0), int(x0) + 6):
            fig.dot(x, vy, VAULT_OUTLINE)
            fig.dot(x, vy + 1, VAULT_STONE[0])
        construct_eyes(fig, (int(x0) + 1, int(x0) + 2), vy, pose["eyes"], burn)
    else:
        for x in range(int(cx) - 5, int(cx) + 5):
            fig.dot(x, vy, VAULT_OUTLINE)
            fig.dot(x, vy + 1, VAULT_STONE[0])
        construct_eyes(fig, (int(cx) - 4, int(cx) - 3), vy, pose["eyes"], burn)
        construct_eyes(fig, (int(cx) + 2, int(cx) + 3), vy, pose["eyes"], burn)
        fig.line(int(cx) - 1, vy + 3, int(cx) - 1, vy + 4, VAULT_STONE[1])


def gauntlet(fig, x, y, ramp, bias=0.0):
    """A forearm and fist of stone, banded with gold at the wrist, the fist's middle at (x, y)."""
    slab(fig, [(x - 4.5, y - 11), (x + 4.5, y - 11), (x + 4.8, y - 3), (x - 4.8, y - 3)], ramp, bias=bias)
    for k in range(-4, 5):
        fig.dot(int(x + k), int(y - 3), VAULT_GOLD[2] if k < 0 else VAULT_GOLD[1])
    slab(fig, [(x - 4.2, y - 2.5), (x + 4.2, y - 2.5), (x + 4.2, y + 3.5), (x - 4.2, y + 3.5)], ramp, bias=bias)
    for k in (-1.5, 1.5):
        fig.line(int(x + k), int(y - 1), int(x + k), int(y + 2), ramp[1])


def construct_front(fig, pose, facing):
    """A vault construct facing you, or going away: stout legs, a broad chest with the gold sun set
    in it, great round shoulders, arms ending in stone fists, and a small helmed head."""
    b, g = pose["bob"], fig.ground + 0.5
    cx = 24.0
    away = facing == UP
    burn = 0.3 + pose["flare"] * 0.7
    stone, far = VAULT_STONE, shifted(VAULT_STONE, 1)
    toward = 1 if not away else -1
    for i, (fwd, lift) in enumerate(pose["feet"]):
        side = (-1, 1)[i]
        x = cx + side * 6
        fy = g + 0.5 - lift - (1.0 if fwd * toward < 0 else 0)
        slab(fig, [(x - 3.8, 46 + b), (x + 3.8, 46 + b), (x + 3.8, fy - 4), (x - 3.8, fy - 4)], stone)
        slab(fig, [(x - 5, fy - 5), (x + 5, fy - 5), (x + 5.2, fy), (x - 5.2, fy)], stone)
        for k in range(-4, 5):
            fig.dot(int(x + k), int(fy - 5), VAULT_GOLD[2] if k < 0 else VAULT_GOLD[1])
    slab(fig, [(cx - 9, 42 + b), (cx + 9, 42 + b), (cx + 8, 49 + b), (cx - 8, 49 + b)], stone)

    hands = []
    for i, (fwd, up) in enumerate(pose["hands"]):
        side = -1 if i == 0 else 1
        if away:
            side = -side
        inward = min(1.0, max(0.0, (up - 4) / 6))
        hx = cx + side * (15.5 - inward * 9)
        hy = 50 + b - up * 2.2 + fwd * 0.4 * toward
        hands.append((side, hx, hy, up))
    for side, hx, hy, up in hands:
        if up > 6:
            continue
        sx, sy = cx + side * 13, 26 + b
        fig.limb([(sx, sy), (hx, hy - 10)], [3.8, 3.4], far)
        gauntlet(fig, hx, hy, stone)

    chest = [(cx - 13, 21 + b), (cx + 13, 21 + b), (cx + 10.5, 43 + b), (cx - 10.5, 43 + b)]
    slab(fig, chest, stone, bevel=2)
    if not away:
        for y in range(int(23 + b), int(41 + b)):
            t = (y - 21 - b) / 22
            for s in (-1, 1):
                fig.dot(int(cx + s * (10.5 - t * 2.3)) - (1 if s > 0 else 0), y, VAULT_GOLD[1] if s > 0 else VAULT_GOLD[2])
        sun(fig, int(cx), int(31 + b), burn)
        for x in range(int(cx - 9), int(cx + 9)):
            fig.dot(x, int(43 + b), VAULT_GOLD[2] if x < cx else VAULT_GOLD[1])
        # Old cracks, and moss in one.
        for x, y in ((cx + 6, 36), (cx + 7, 37), (cx + 7, 38), (cx + 8, 39)):
            fig.dot(int(x), int(y + b), VAULT_STONE[0])
    else:
        for y in range(int(23 + b), int(42 + b)):
            fig.dot(int(cx) - 1, y, VAULT_GOLD[1])
            fig.dot(int(cx), y, VAULT_GOLD[0])
    for side in (-1, 1):
        fig.ball(cx + side * 13.5, 23 + b, 6.5, 5.8, stone)
        for k in range(-5, 6):
            y = 23 + b + 5.0 * math.sqrt(max(0.0, 1 - (k / 6.5) ** 2))
            fig.dot(int(cx + side * 13.5 + k), int(y), VAULT_GOLD[2] if k < 0 else VAULT_GOLD[1])
    construct_head(fig, cx, 9 + b + pose["tip"], facing, pose, burn)
    for side, hx, hy, up in hands:
        if up <= 6:
            continue
        sx, sy = cx + side * 13, 24 + b
        fig.limb([(sx, sy), (hx, hy + 6)], [3.8, 3.4], stone)
        gauntlet(fig, hx, hy, stone)
    if pose["flare"]:
        for i in range(10):
            a = i * math.tau / 10
            for r in (16, 18, 20):
                fig.glow(int(cx + math.cos(a) * r), int(31 + b + math.sin(a) * r * 0.8), SUN_LIGHT[0] + (int(120 - r * 4),))


def construct_side(fig, pose):
    """A vault construct side on, facing left: the far arm and leg darker behind, the deep body,
    the near leg, the near arm swinging its fist, and the helmed head."""
    b, g = pose["bob"], fig.ground + 0.5
    lean = pose["lean"]
    cx = 25.5
    burn = 0.3 + pose["flare"] * 0.7
    stone, far = VAULT_STONE, shifted(VAULT_STONE, 1)
    top = cx - lean

    def leg(fwd, lift, ramp, bias=0.0):
        x = cx - fwd * 1.1
        fy = g + 0.5 - lift
        slab(fig, [(cx - 4 - lean * 0.3, 46 + b), (cx + 4 - lean * 0.3, 46 + b), (x + 4, fy - 4), (x - 4, fy - 4)], ramp, bias=bias)
        slab(fig, [(x - 6.5, fy - 5), (x + 4.5, fy - 5), (x + 4.5, fy), (x - 7, fy)], ramp, bias=bias)
        for k in range(-6, 4):
            fig.dot(int(x + k), int(fy - 5), VAULT_GOLD[2] if k < -1 else VAULT_GOLD[1])

    def arm(fwd, up, ramp, bias=0.0):
        sx, sy = top + 1, 25 + b
        hx = sx - fwd * 1.3 - max(0.0, up) * 0.4
        hy = 50 + b - up * 2.2
        fig.limb([(sx, sy), (hx + 1, hy - 9)], [3.6, 3.2], ramp, bias=bias)
        gauntlet(fig, hx, hy, ramp, bias=bias)

    (near_fwd, near_lift), (far_fwd, far_lift) = pose["feet"]
    (w_fwd, w_up), (o_fwd, o_up) = pose["hands"]
    arm(o_fwd, o_up, far, -0.1)
    leg(far_fwd, far_lift, far, -0.1)
    slab(fig, [(cx - 7 - lean * 0.3, 42 + b), (cx + 7 - lean * 0.3, 42 + b), (cx + 6.5, 49 + b), (cx - 6.5, 49 + b)], stone)
    leg(near_fwd, near_lift, stone)
    slab(fig, [(top - 10, 21 + b), (top + 10, 21 + b), (cx + 8.5, 43 + b), (cx - 9, 43 + b)], stone, bevel=2)
    for y in range(int(23 + b), int(42 + b)):
        fig.dot(int(top - 9 + (y - 21 - b) * 0.05), y, VAULT_GOLD[2])
    for x, y in ((top + 5, 30), (top + 6, 31), (top + 6, 32), (top + 7, 33)):
        fig.dot(int(x), int(y + b), VAULT_STONE[0])
    for x in range(int(cx - 7), int(cx + 7)):
        fig.dot(x, int(43 + b), VAULT_GOLD[2] if x < cx else VAULT_GOLD[1])
    if pose["flare"]:
        fig.glow(int(top - 10), int(31 + b), SUN_LIGHT[1] + (220,))
        fig.glow(int(top - 11), int(31 + b), SUN_LIGHT[0] + (150,))
    fig.ball(top + 1, 24 + b, 7.5, 6.4, stone)
    for k in range(-6, 7):
        y = 24 + b + 5.6 * math.sqrt(max(0.0, 1 - (k / 7.5) ** 2))
        fig.dot(int(top + 1 + k), int(y), VAULT_GOLD[2] if k < 0 else VAULT_GOLD[1])
    construct_head(fig, top - 1.5, 9 + b + pose["tip"], LEFT, pose, burn)
    arm(w_fwd, w_up, stone)


def construct_downed(fig):
    """A vault construct broken: tumbled into a heap of dressed blocks, its gold bands loose, its
    helmed head on its side at the front with the light gone out of it."""
    g = fig.ground + 0.5
    stone = VAULT_STONE
    for x0, y0, x1, y1 in ((22, g - 16, 36, g - 6), (30, g - 9, 43, g), (6, g - 8, 19, g), (17, g - 11, 31, g)):
        slab(fig, [(x0, y0), (x1, y0), (x1, y1), (x0, y1)], stone, bevel=1.5)
    for x in range(18, 31):
        fig.dot(x, int(g - 11), VAULT_GOLD[2] if x < 24 else VAULT_GOLD[1])
    fig.ball(36, g - 17, 5.5, 4.6, stone)
    construct_head(fig, 11.5, g - 21, DOWN, {"eyes": False}, 0.0)
    for x, y in ((28, g - 15), (29, g - 14), (29, g - 13), (30, g - 12)):
        fig.dot(int(x), int(y), VAULT_STONE[0])
    for i in range(6):
        x = int(4 + hash01(i, 0, 71) * 40)
        fig.dot(x, int(g), VAULT_GOLD[2] if i % 2 else VAULT_STONE[3])


def vault_construct(fig, facing, column):
    """One frame of a vault construct. It moves like the Hollowed (it's on two legs), only slower
    and heavier: everything it does is its fists."""
    if column == DOWNED:
        construct_downed(fig)
        return
    pose = biped_pose(column)
    if column == WIND_UP:
        pose["hands"] = [(0, 12), (0, 12)]
    elif column == STRIKE:
        pose["hands"] = [(5, 1), (5, 1)]
    elif column == CAST:
        pose["hands"] = [(1, 3), (1, 3)]
    if facing == LEFT:
        construct_side(fig, pose)
    else:
        construct_front(fig, pose, facing)


# --------------------------------------------------------------------------- #
# The sea-wraith
# --------------------------------------------------------------------------- #

#: A drowned thing gone sea-green and half see-through, pale and cold in its light; the kelp it's
#: got for hair; its eyes and the cold glow off it; the dark of its mouth and eye sockets; the
#: water it comes apart into; and its outline.
WRAITH = ((22, 74, 84), (40, 116, 118), (68, 160, 152), (118, 206, 190), (190, 244, 228))
KELP = ((20, 42, 34), (34, 70, 46), (56, 104, 58), (92, 140, 72))
WRAITH_EYE = ((170, 255, 240), (250, 255, 255))
WRAITH_GLOW = (150, 255, 230)
DROWNED = (8, 30, 40)
SEAWATER = ((18, 70, 96), (34, 112, 138), (80, 170, 186), (196, 244, 240))
WRAITH_OUTLINE = (6, 30, 40)

#: How opaque the wraith is down its length: its head and shoulders most, thinning through its
#: middle to the rags it trails off into, which you can nearly see through.
WRAITH_TOP_ALPHA = 215
WRAITH_TAIL_ALPHA = 95
WRAITH_FADE_FROM, WRAITH_FADE_TO = 28, 44

#: How much more see-through it goes for the flicker when it's hurt.
WRAITH_FLICKER = 0.5


def wraith_pose(column):
    """How a sea-wraith hangs in a frame. It doesn't walk, it drifts, so its "steps" are its rags
    and hair streaming and its arms trailing, with the body held at the same height; it rears and
    spreads its claws to wind up, lunges claws first to strike, throws its head back and wails to
    cast, flickers half away when it's hurt, and comes apart into water when it's down."""
    pose = {"lean": 0, "rise": 0, "sway": 0.0, "hands": [(0, 0), (0, 0)], "jaw": 0, "eyes": True,
            "wail": 0.0, "flicker": False, "claws": False, "phase": 0.0}
    if 1 <= column <= 4:
        i = column - 1
        pose.update(lean=1, sway=(1.0, 0.0, -1.0, 0.0)[i], phase=i / 4.0, hands=[(-1, 1), (-1, 1)])
    elif column == WIND_UP:
        pose.update(lean=-2, rise=-2, hands=[(-1, 9), (-1, 9)], jaw=1, claws=True, phase=0.6)
    elif column == STRIKE:
        pose.update(lean=3, rise=1, hands=[(5, 4), (4, 3)], jaw=2, claws=True, phase=0.2, sway=-1.0)
    elif column == CAST:
        pose.update(rise=-1, hands=[(1, 7), (1, 7)], jaw=2, wail=1.0, phase=0.4)
    elif column == HURT:
        pose.update(lean=-2, hands=[(-3, 4), (-3, 5)], jaw=1, eyes=False, flicker=True, phase=0.8)
    return pose


def kelp(fig, points, width, phase, seed):
    """A strand of kelp hanging off the wraith along a line of points, waving, with a frond
    widening here and there down it. It's solid, not see-through."""
    wavy = []
    for i, (x, y) in enumerate(points):
        wavy.append((x + math.sin(i * 1.3 + phase * math.tau + seed) * 0.7 * (i > 0), y))
    radii = [width * (1.0 - 0.35 * i / max(1, len(points) - 1)) for i in range(len(points))]
    for i in range(1, len(radii) - 1):
        if hash01(i, seed, 51) < 0.45:
            radii[i] *= 1.35
    fig.solid.update(fig.limb(wavy, radii, KELP, seam=False))


def claws(fig, x, y, toward, spread):
    """Three long pale claws off a hand at (x, y), reaching `toward` (a step across and down)."""
    dx, dy = toward
    for k in (-1, 0, 1):
        sx, sy = -dy * k * spread, dx * k * spread
        for i in range(1, 4):
            fig.dot(int(math.floor(x + sx * (0.5 + i * 0.3) + dx * i)), int(math.floor(y + sy * (0.5 + i * 0.3) + dy * i)),
                    WRAITH[4] if i < 3 else WRAITH[3])


def drips(fig, x0, x1, y, phase):
    """Water running off it: a few drops falling from its rags, and the odd one already on the way
    down."""
    for i in range(4):
        x = int(x0 + (x1 - x0) * hash01(i, 0, 81))
        fall = (phase * 6 + i * 1.7) % 5
        if y + fall < fig.ground + 2:
            fig.glow(x, int(y + fall), SEAWATER[3] + (150 - int(fall * 22),))


def wraith_glow(fig, x, y, strength):
    """The cold pale light it gives off, round a point, out past its outline."""
    reach = 3 + strength * 3
    for oy in range(-int(reach) - 1, int(reach) + 2):
        for ox in range(-int(reach) - 1, int(reach) + 2):
            d = math.hypot(ox, oy * 1.1)
            if d <= reach and not fig.has(x + ox, y + oy):
                fig.glow(x + ox, y + oy, WRAITH_GLOW + (int(105 * (1 - d / (reach + 0.5))),))


def wraith_face(fig, cx, cy, facing, pose):
    """Its drowned face: sunken dark sockets with a cold light for eyes, and a mouth hanging open."""
    open_ = pose["eyes"]
    if facing == LEFT:
        sockets = ((int(cx) - 4, int(cy)),)
        mouth = (int(cx) - 5, int(cy) + 4)
    else:
        sockets = ((int(cx) - 4, int(cy)), (int(cx) + 2, int(cy)))
        mouth = (int(cx) - 1, int(cy) + 4)
    for x, y in sockets:
        for ox in (-1, 0, 1, 2):
            for oy in (0, 1):
                if (ox in (-1, 2)) and oy == 1:
                    continue
                fig.dot(x + ox, y + oy, DROWNED)
        if open_:
            fig.dot(x, y, WRAITH_EYE[1])
            fig.dot(x + 1, y, WRAITH_EYE[0])
            fig.dot(x, y + 1, WRAITH_EYE[0])
            fig.solid.update({fig.at(x, y), fig.at(x + 1, y), fig.at(x, y + 1)})
            fig.glow(x - 2, y, WRAITH_EYE[0] + (110,))
            fig.glow(x + 3, y, WRAITH_EYE[0] + (90,))
            fig.glow(x, y - 1, WRAITH_EYE[0] + (90,))
            fig.glow(x + 1, y - 1, WRAITH_EYE[0] + (60,))
    gape = 1 + pose["jaw"]
    for oy in range(gape):
        for ox in range(2 if facing != LEFT else 1):
            fig.dot(mouth[0] + ox, mouth[1] + oy, DROWNED)
    fig.dot(mouth[0] + (2 if facing != LEFT else 1), mouth[1], WRAITH[1])


def wraith_front(fig, column, pose, facing):
    """A sea-wraith facing you (or, with `facing` UP, going away): kelp hanging round its head and
    down past its shoulders, a thin drowned body, long arms ending in claws, and rags trailing off
    under it where its legs should be."""
    cx = 16.0
    away = facing == UP
    lean = pose["lean"] * (0.5 if not away else -0.3)
    rise = pose["rise"]
    hy = 14 + rise + max(0, lean)
    phase = pose["phase"]
    sway = pose["sway"]
    body, far = WRAITH, shifted(WRAITH, 1)

    # The rags it trails off into, swaying, longest in the middle.
    for k, (ox, length) in enumerate(((-3.6, 9), (-1.2, 12), (1.2, 11), (3.6, 8))):
        top = (cx + ox * 0.8, 29 + rise)
        bend = math.sin(phase * math.tau + k * 1.4) * 1.3 + sway * (0.6 + k * 0.1)
        fig.limb([top, (cx + ox + bend * 0.5, top[1] + length * 0.5), (cx + ox * 1.2 + bend, top[1] + length)],
                 [2.2, 1.5, 0.6], far if k % 2 else body)
    fig.plate([(cx - 5.5, 26 + rise), (cx + 5.5, 26 + rise), (cx + 5.0, 33 + rise), (cx + 2.5, 31 + rise),
               (cx, 35 + rise), (cx - 2.5, 31.5 + rise), (cx - 5.0, 34 + rise)], body, facing=(-0.2, 0.1, 0.95))

    # The kelp hanging down its back and over its shoulders, behind its arms.
    for side in (-1, 1):
        for k, ox in enumerate((6.4, 4.6)):
            top = (cx + side * ox * 0.8, hy - 4)
            end_y = hy + 15 + k * 3 - (2 if away else 0)
            kelp(fig, [top, (cx + side * (ox + 0.6), hy + 2), (cx + side * (ox + 0.4), hy + 8), (cx + side * (ox - 0.2), end_y)],
                 1.3, phase, k + (3 if side > 0 else 0))
    fig.limb([(cx, 22 + rise + lean), (cx, 28 + rise)], [4.4, 3.2], body)

    # Arms: long and thin, hanging or reaching, claws at the ends.
    hands = []
    for i, (fwd, up) in enumerate(pose["hands"]):
        side = -1 if i == 0 else 1
        sx, sy = cx + side * 5.0, 22.5 + rise + lean
        hx = cx + side * (7.0 + max(0, up) * 0.35) - side * max(0, fwd) * 0.5
        hyy = 31 + rise - up * 1.3 + max(0, fwd) * 0.4
        fig.limb([(sx, sy), ((sx + hx) / 2 + side * 1.2, (sy + hyy) / 2), (hx, hyy)], [1.6, 1.3, 1.1], far)
        fig.ball(hx, hyy + 0.5, 1.4, 1.3, body)
        hands.append((side, hx, hyy))

    if not away:
        # Ribs showing through.
        for y in (24, 26):
            for x in range(int(cx) - 2, int(cx) + 2):
                if x != int(cx) - 1 or y != 24:
                    fig.dot(x, int(y + rise + lean), WRAITH[1])

    # The head, a shade darker so its eyes burn out of it, then the kelp over it.
    fig.ball(cx, hy, 6.8, 6.4, body, bias=-0.2)
    fig.ball(cx, hy + 3.6, 4.6, 3.2, body, bias=-0.2, seam=False)
    if not away:
        wraith_face(fig, cx, hy + 0.5, DOWN, pose)
    shaped = {}
    cap_bottom = hy - 3 if not away else hy + 5
    for y in range(int(hy - 8), int(cap_bottom) + 1):
        for x in range(int(cx - 8), int(cx + 8)):
            nx, ny = (x + 0.5 - cx) / 7.3, (y + 0.5 - (hy - 0.5)) / 7.0
            if nx * nx + ny * ny <= 1.0:
                # A ragged fringe: some strands come down further than others.
                if y > hy - 5 and not away and hash01(x, 0, 91) < 0.5 and abs(x + 0.5 - cx) < 4:
                    continue
                shaped[(x, y)] = (nx, ny, math.sqrt(max(0.0, 1 - nx * nx - ny * ny)))
    fig.solid.update(fig.lay(shaped, KELP))
    for side in (-1, 1):
        kelp(fig, [(cx + side * 3.0, hy - 3), (cx + side * 4.2, hy + 1), (cx + side * 4.6, hy + 5)], 0.9, phase, 7 + side)
    if away:
        for k in range(-2, 3):
            kelp(fig, [(cx + k * 2.2, hy + 2), (cx + k * 2.4, hy + 9), (cx + k * 2.2, hy + 14 + abs(k))], 1.2, phase, 11 + k)

    for side, hx, hyy in hands:
        up = hyy < 26
        claws(fig, hx, hyy + 1.0, (side * 0.3, -1.0) if up else (side * 0.2, 1.0), 0.9)
    wraith_glow(fig, int(cx), int(hy), 0.6 + pose["wail"])
    wraith_glow(fig, int(cx), int(hy) + 12, 0.3)
    if pose["wail"]:
        wail(fig, cx, hy + 6)
    drips(fig, cx - 4, cx + 4, 41 + rise, phase)


def wail(fig, x, y):
    """Its wail: rings of cold light spreading out off its open mouth, and spray thrown up."""
    for r in (5, 8, 11):
        for i in range(16):
            a = i * math.tau / 16
            if hash01(i, r, 3) < 0.7:
                fig.glow(int(x + math.cos(a) * r), int(y + math.sin(a) * r * 0.7), WRAITH_GLOW + (int(150 - r * 9),))


def wraith_side(fig, column, pose):
    """A sea-wraith side on, drifting left: leaning into it, the kelp streaming back off its head,
    its rags trailing out behind and below, its near arm reaching ahead with the claws out."""
    cx = 16.0
    lean = pose["lean"]
    rise = pose["rise"]
    phase = pose["phase"]
    sway = pose["sway"]
    body, far = WRAITH, shifted(WRAITH, 1)
    top = cx - lean * 0.7
    hy = 14 + rise

    (w_fwd, w_up), (o_fwd, o_up) = pose["hands"]

    def arm(fwd, up, ramp):
        sx, sy = top + 0.5, 22.5 + rise
        hx = sx - 2 - fwd * 1.1 - max(0, up) * 0.2
        hyy = 30.5 + rise - up * 1.3
        fig.limb([(sx, sy), ((sx + hx) / 2 + 1.0, (sy + hyy) / 2 + 0.5), (hx, hyy)], [1.5, 1.3, 1.1], ramp)
        fig.ball(hx, hyy + 0.4, 1.4, 1.3, ramp)
        return hx, hyy

    arm(o_fwd, o_up, far)
    for k, (oy, length) in enumerate(((0, 11), (1.5, 13), (3, 10))):
        start = (cx + 1 + k * 0.8, 28 + rise + oy * 0.3)
        bend = math.sin(phase * math.tau + k * 1.5) * 1.2 + sway * 0.5
        fig.limb([start, (start[0] + 2.5 + k, start[1] + length * 0.5 + bend * 0.4),
                  (start[0] + 4.5 + k * 1.5 + bend, start[1] + length)], [2.2, 1.4, 0.6], far if k == 1 else body)
    fig.plate([(top - 3.5, 26 + rise), (top + 4.0, 26 + rise), (cx + 6.5, 33 + rise), (cx + 3.5, 31.5 + rise),
               (cx + 1.0, 35 + rise), (cx - 2.5, 31 + rise)], body, facing=(-0.3, 0.1, 0.95))
    fig.limb([(top, 22 + rise), (cx + 0.5, 28 + rise)], [3.4, 2.6], body)
    for k, (ox, length) in enumerate(((3.5, 16), (5.5, 13), (1.8, 18))):
        start = (top + ox * 0.6, hy - 5)
        trail = 2.5 + lean * 0.6
        kelp(fig, [start, (top + ox + trail * 0.5, hy + 1), (top + ox + trail, hy + 7), (top + ox + trail * 1.4, hy - 4 + length)],
             1.3, phase, 21 + k)
    fig.ball(top - 0.5, hy, 6.4, 6.4, body, bias=-0.2)
    fig.ball(top - 2.0, hy + 3.4, 4.2, 3.2, body, bias=-0.2, seam=False)
    wraith_face(fig, top - 0.5, hy + 0.5, LEFT, pose)
    shaped = {}
    for y in range(int(hy - 8), int(hy + 5)):
        for x in range(int(top - 8), int(top + 8)):
            nx, ny = (x + 0.5 - (top + 0.8)) / 6.9, (y + 0.5 - (hy - 0.5)) / 7.0
            if nx * nx + ny * ny > 1.0:
                continue
            # The kelp covers the back of the head down to the jaw, and only the crown in front.
            if x + 0.5 < top - 5.0 + max(0, y - (hy - 5)) * 2.2:
                continue
            shaped[(x, y)] = (nx, ny, math.sqrt(max(0.0, 1 - nx * nx - ny * ny)))
    fig.solid.update(fig.lay(shaped, KELP))
    kelp(fig, [(top - 4.5, hy - 4), (top - 5.8, hy), (top - 5.6, hy + 4)], 0.9, phase, 29)
    hx, hyy = arm(w_fwd, w_up, body)
    up = w_up > 5
    claws(fig, hx, hyy + 0.8, (-0.8, -0.6) if up else (-0.7, 0.7) if w_fwd > 2 else (-0.2, 1.0), 0.9)
    wraith_glow(fig, int(top - 1), int(hy), 0.6 + pose["wail"])
    wraith_glow(fig, int(cx), int(hy) + 12, 0.3)
    if pose["wail"]:
        wail(fig, top - 4, hy + 5)
    drips(fig, cx, cx + 8, 40 + rise, phase)


def wraith_downed(fig, column):
    """A sea-wraith gone: a spreading puddle of seawater on the deck with its kelp lying in it, the
    last of it rising off the water as a wisp of cold light, and a splash still coming down."""
    g = fig.ground + 0.5
    cx = 16.0
    shaped = {}
    for y in range(int(g - 4), int(g) + 1):
        for x in range(1, 31):
            nx, ny = (x + 0.5 - cx) / 12.5, (y + 0.5 - (g - 1.6)) / 2.6
            ripple = 0.12 * math.sin(x * 0.9)
            if nx * nx + ny * ny <= 1.0 + ripple:
                shaped[(x, y)] = (nx * 0.3, -0.5 + ny * 0.3, 0.8)
    fig.lay(shaped, SEAWATER[:3], seam=False)
    for x in range(int(cx) - 8, int(cx) + 7, 3):
        if fig.has(x, int(g - 3)):
            fig.dot(x, int(g - 3), SEAWATER[3])
            fig.dot(x + 1, int(g - 3), SEAWATER[2])
    kelp(fig, [(cx - 9, g - 1.5), (cx - 4, g - 2.5), (cx + 1, g - 1.6)], 1.0, 0.1, 41)
    kelp(fig, [(cx + 2, g - 2.8), (cx + 6, g - 1.6), (cx + 10, g - 2.2)], 0.9, 0.3, 43)
    fig.opacity = lambda y: 170
    for i in range(14):
        t = i / 13
        x = cx + math.sin(t * 5.5) * 2.2 * (1 - t)
        y = g - 5 - t * 22
        fig.glow(int(x), int(y), WRAITH_GLOW + (int(170 * (1 - t)),))
        if t < 0.6:
            fig.glow(int(x) + 1, int(y), WRAITH_GLOW + (int(90 * (1 - t)),))
    for x, y in ((cx - 7, g - 7), (cx + 6, g - 9), (cx - 3, g - 11), (cx + 9, g - 5)):
        fig.glow(int(x), int(y), SEAWATER[3] + (190,))


def sea_wraith(fig, facing, column):
    """One frame of a sea-wraith: see-through, thinning to nearly nothing down its rags."""
    if column == DOWNED:
        wraith_downed(fig, column)
        return
    pose = wraith_pose(column)
    flicker = WRAITH_FLICKER if pose["flicker"] else 1.0

    def opacity(y):
        t = max(0.0, min(1.0, (y - WRAITH_FADE_FROM) / (WRAITH_FADE_TO - WRAITH_FADE_FROM)))
        return int((WRAITH_TOP_ALPHA + (WRAITH_TAIL_ALPHA - WRAITH_TOP_ALPHA) * t) * flicker)
    fig.opacity = opacity
    if facing == LEFT:
        wraith_side(fig, column, pose)
    else:
        wraith_front(fig, column, pose, facing)
    if pose["flicker"]:
        # Hurt, it flickers: the solid parts go see-through too.
        fig.solid = set()


# --------------------------------------------------------------------------- #
# Saving
# --------------------------------------------------------------------------- #

#: Every creature: its id (and sheet name), its frame's width and height, how to draw a frame of
#: it, and the colour of its outline.
CREATURES = (
    ("sea-wraith", 32, 48, sea_wraith, WRAITH_OUTLINE),
    ("gloomfang", 48, 32, gloomfang, GLOOM_OUTLINE),
    ("thornmother", 96, 80, thornmother, BEAR_OUTLINE),
    ("hollowed-footman", 32, 48, hollowed(FOOTMAN), HOLLOW_OUTLINE),
    ("hollowed-hexer", 32, 48, hollowed(HEXER), HOLLOW_OUTLINE),
    ("hollowed-sapper", 32, 48, hollowed(SAPPER), HOLLOW_OUTLINE),
    ("vault-construct", 48, 64, vault_construct, VAULT_OUTLINE),
)


if __name__ == "__main__":
    wanted = sys.argv[1:]
    known = [c[0] for c in CREATURES]
    for name in wanted:
        if name not in known:
            sys.exit("No creature called '%s'." % name)
    print("Generating the creatures for Legend Quest...")
    for name, w, h, draw, outline in CREATURES:
        if not wanted or name in wanted:
            sheet(name, w, h, draw, outline)
    print("Done.")
