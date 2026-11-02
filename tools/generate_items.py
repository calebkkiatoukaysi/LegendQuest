"""
Item icons for Legend Quest: one 24x24 picture for everything in ItemCatalog, and the frames the
inventory's slots are drawn with.
Run it from the repo root:

    python tools/generate_items.py

Items/icons.png is the icons, sixteen to a row, in ITEMS order: an item's Icon in ItemCatalog is
its place in that list, counting from 0, so the two have to stay in step: before it draws a
thing this script reads Items/ItemCatalog.cs, and stops with what's different if an item's icon
or rarity there isn't its place and rarity here. Items/slots.png is one row of 24x24 cells for
the inventory (see SLOTS).

Every icon is drawn the same way, with the kit at the top of this file (generate_combat_art.py
uses it too, so the ability icons match): each pixel is given a material, a letter standing for a
ramp of five shades, and the shading is worked out at the end from where each pixel sits in its
patch of that material. Lit from the top left like everything else: a pixel with nothing of its
own material above it or to its left catches the light, one with nothing below or to its right
falls into shadow, and a big patch also darkens from its top left corner to its bottom right
one. Then a dark outline goes round the lot, and sparkles go on top. Magic things get a glint
(small for uncommon, bigger and coloured for rare), so you can tell them apart at a glance.

Blades, hafts and bows lie corner to corner, point to the top right, like the icons in every RPG
since the first. They're drawn on the diagonal's own grid: `a` counts along the line from the
bottom left to the top right (x - y), and `k` across it (x + y - 23, so 0 is the line itself, and
less is toward the top left, the lit side). A band of `k` from -1 to 1 is a blade three pixels
across on every row, which no amount of rotating a picture would give you so cleanly.

Only needs CPython, and there's nothing random in it, so a rerun writes the same bytes.
"""

import math
import os
import re
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from pixels import OUT, ROOT, new_image, put, write_png  # noqa: E402


# --------------------------------------------------------------------------- #
# Colours
# --------------------------------------------------------------------------- #
#
# Five shades to a ramp: deep shadow, shadow, base, light, highlight. Picked from the style sheet's
# rows and pushed a little brighter where something has to read against the inventory's navy.

STEEL = ((62, 68, 98), (104, 116, 150), (156, 168, 196), (208, 218, 236), (248, 251, 255))
IRON = ((40, 42, 62), (68, 74, 102), (104, 112, 142), (146, 156, 184), (200, 208, 226))
GOLD = ((126, 70, 46), (176, 108, 58), (226, 162, 76), (246, 204, 114), (253, 240, 204))
WOOD = ((72, 40, 34), (106, 62, 46), (146, 90, 58), (186, 126, 78), (222, 170, 114))
LEATHER = ((84, 46, 40), (128, 76, 54), (170, 110, 70), (206, 150, 96), (236, 196, 144))
DARK_LEATHER = ((44, 28, 34), (72, 46, 46), (104, 68, 58), (140, 98, 76), (182, 142, 108))
FUR = ((60, 52, 58), (94, 84, 86), (134, 122, 116), (176, 166, 152), (218, 210, 194))
LINEN = ((134, 112, 108), (188, 166, 152), (226, 208, 188), (246, 232, 212), (255, 249, 238))
BLUE = ((22, 36, 86), (36, 66, 134), (58, 106, 182), (98, 156, 222), (170, 210, 250))
RED = ((90, 20, 40), (148, 34, 54), (206, 58, 66), (240, 114, 108), (255, 200, 190))
VIOLET = ((46, 28, 86), (82, 54, 146), (128, 96, 202), (180, 152, 238), (234, 222, 255))
GREEN = ((22, 64, 52), (42, 102, 64), (80, 146, 70), (142, 188, 82), (212, 228, 148))
CRYSTAL = ((16, 10, 30), (36, 22, 64), (70, 44, 126), (130, 90, 222), (228, 212, 255))
BONE = ((140, 120, 106), (188, 168, 148), (224, 210, 188), (244, 234, 214), (255, 250, 240))
GLASS = ((64, 102, 138), (108, 156, 188), (162, 208, 228), (210, 240, 248), (250, 255, 255))
FIRE = ((128, 30, 24), (204, 70, 34), (242, 134, 44), (252, 198, 78), (255, 246, 192))
ICE = ((28, 76, 128), (50, 128, 180), (94, 194, 222), (160, 236, 244), (234, 255, 255))
ROSE = ((94, 28, 60), (156, 54, 98), (214, 96, 142), (242, 152, 182), (255, 216, 228))
SUNSTEEL = ((150, 104, 62), (214, 166, 96), (246, 220, 148), (255, 242, 200), (255, 255, 242))

#: Which ramp each material letter stands for. A capital is shaded from the base shade, a small
#: letter from one darker, for the far side of something or a seam in it.
MATERIALS = {
    "S": STEEL, "I": IRON, "G": GOLD, "W": WOOD, "L": LEATHER, "D": DARK_LEATHER, "F": FUR,
    "C": LINEN, "B": BLUE, "R": RED, "V": VIOLET, "N": GREEN, "K": CRYSTAL, "P": BONE,
    "E": GLASS, "O": FIRE, "Q": ICE, "M": ROSE, "U": SUNSTEEL,
}

#: Marks that take the ramp of whatever material is round them, at a fixed shade: a highlight, a
#: light and a deep shadow.
ACCENTS = {"+": 4, "^": 3, "-": 0}

#: The outline, and a seam drawn inside something ("o"). The dialogue icons' ink.
INK = (24, 18, 30)

#: The middle of a glint, nearly white.
GLINT = (255, 252, 240)

#: An uncommon thing's glint: pale gold arms. A rare one's: longer arms, the outer ones blue.
GLINT_UNCOMMON = (255, 232, 160)
GLINT_RARE = (176, 196, 255)

#: An item's rarity, as ItemCatalog's Rarity spells it, in lower case.
COMMON, UNCOMMON, RARE = "common", "uncommon", "rare"

#: How big a patch of one material has to be, both ways, before it darkens across its own corners
#: as well as at its edges.
GRADIENT_SPAN = 6

#: Where on a patch, from 0 at its top left corner to 1 at its bottom right one, it gets a shade
#: lighter, and where a shade darker.
GRADIENT_LIGHT = 0.26
GRADIENT_DARK = 0.74

#: An icon's size, in art pixels.
ICON = 24

#: The diagonal the long things lie along: x + y equals this on its middle line.
AXIS = 23


# --------------------------------------------------------------------------- #
# The drawing kit
# --------------------------------------------------------------------------- #

class Icon:
    """One icon being drawn: a material letter (or a mark) for each pixel, and the sparkles that go
    on over the outline. Nothing is coloured until `render`."""

    def __init__(self, size=ICON, remap=None):
        self.size = size
        self.grid = [[None] * size for _ in range(size)]
        self.materials = dict(MATERIALS)
        if remap:
            self.materials.update(remap)
        self.over = []

    # Laying down material ------------------------------------------------- #

    def set(self, x, y, ch):
        """Gives one pixel a material or a mark. "." clears it."""
        if 0 <= x < self.size and 0 <= y < self.size:
            self.grid[y][x] = None if ch == "." else ch

    def fill(self, ch, test):
        """Gives every pixel whose middle passes `test(px, py)` the material."""
        for y in range(self.size):
            for x in range(self.size):
                if test(x + 0.5, y + 0.5):
                    self.set(x, y, ch)

    def diag(self, ch, a0, a1, k0, k1, axis=AXIS):
        """Fills a band on the diagonal's grid: from a0 to a1 along it, k0 to k1 across it."""
        for y in range(self.size):
            for x in range(self.size):
                a, k = x - y, x + y - axis
                if a0 <= a <= a1 and k0 <= k <= k1:
                    self.set(x, y, ch)

    def diag_where(self, ch, test, axis=AXIS):
        """Fills every pixel whose place on the diagonal's grid passes `test(a, k)`."""
        for y in range(self.size):
            for x in range(self.size):
                if test(x - y, x + y - axis):
                    self.set(x, y, ch)

    def rect(self, ch, x0, y0, x1, y1):
        """Fills a box, corners included."""
        for y in range(y0, y1 + 1):
            for x in range(x0, x1 + 1):
                self.set(x, y, ch)

    def disc(self, ch, cx, cy, r):
        """Fills a round."""
        self.fill(ch, lambda px, py: (px - cx) ** 2 + (py - cy) ** 2 <= r * r)

    def ellipse(self, ch, cx, cy, rx, ry):
        """Fills an oval, rx across and ry down from its middle."""
        self.fill(ch, lambda px, py: ((px - cx) / rx) ** 2 + ((py - cy) / ry) ** 2 <= 1.0)

    def ring(self, ch, cx, cy, r0, r1):
        """Fills the band between two circles."""
        self.fill(ch, lambda px, py: r0 * r0 <= (px - cx) ** 2 + (py - cy) ** 2 <= r1 * r1)

    def poly(self, ch, points):
        """Fills a polygon (even-odd)."""
        self.fill(ch, lambda px, py: inside(points, px, py))

    def capsule(self, ch, x0, y0, x1, y1, r0, r1=None):
        """A stroke from one point to another, r0 thick at the start and r1 at the end."""
        r1 = r0 if r1 is None else r1

        def test(px, py):
            dx, dy = x1 - x0, y1 - y0
            t = max(0.0, min(1.0, ((px - x0) * dx + (py - y0) * dy) / float(dx * dx + dy * dy)))
            r = r0 + (r1 - r0) * t
            return (px - x0 - dx * t) ** 2 + (py - y0 - dy * t) ** 2 <= r * r
        self.fill(ch, test)

    def line(self, ch, x0, y0, x1, y1):
        """A one pixel line, Bresenham's."""
        dx, dy = abs(x1 - x0), -abs(y1 - y0)
        sx, sy = (1 if x0 < x1 else -1), (1 if y0 < y1 else -1)
        err = dx + dy
        while True:
            self.set(x0, y0, ch)
            if x0 == x1 and y0 == y1:
                return
            e2 = 2 * err
            if e2 >= dy:
                err += dy
                x0 += sx
            if e2 <= dx:
                err += dx
                y0 += sy

    def stamp(self, rows, ox=0, oy=0):
        """Lays a little picture written as text over the icon: one character a pixel, "." or a
        space left alone."""
        for y, row in enumerate(rows):
            for x, ch in enumerate(row):
                if ch not in ". ":
                    self.set(ox + x, oy + y, ch)

    def mirror(self):
        """Copies the left half onto the right, for the things that are the same both sides."""
        half = self.size // 2
        for y in range(self.size):
            for x in range(half):
                self.grid[y][self.size - 1 - x] = self.grid[y][x]

    def pattern(self, src, dst, test):
        """Swaps one material for another wherever `test(x, y)` says, for weaves, rings and scales."""
        for y in range(self.size):
            for x in range(self.size):
                if self.grid[y][x] == src and test(x, y):
                    self.grid[y][x] = dst

    def flip(self, across=True, down=False):
        """Turns everything drawn so far over: left for right, and top for bottom if `down`."""
        n = self.size
        self.grid = [[self.grid[n - 1 - y if down else y][n - 1 - x if across else x] for x in range(n)] for y in range(n)]

    # Things over the outline ---------------------------------------------- #

    def glow(self, x, y, colour, alpha):
        """A pixel of light laid over everything, outline included."""
        self.over.append((x, y, colour + (alpha,)))

    def sparkle(self, x, y, rarity):
        """The glint a magic thing has: a bright middle and four arms, longer and bluer for rare."""
        self.glow(x, y, GLINT, 255)
        for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
            if rarity == RARE:
                self.glow(x + dx, y + dy, GLINT, 230)
                self.glow(x + 2 * dx, y + 2 * dy, GLINT_RARE, 190)
            else:
                self.glow(x + dx, y + dy, GLINT_UNCOMMON, 210)
        if rarity == RARE:
            for dx, dy in ((1, 1), (-1, 1), (1, -1), (-1, -1)):
                self.glow(x + dx, y + dy, GLINT_RARE, 90)

    # Colouring ------------------------------------------------------------ #

    def _material_of(self, x, y):
        """The ramp letter a pixel belongs to, a mark resolved to the commonest letter round it."""
        ch = self.grid[y][x]
        if ch is None or ch == "o":
            return None
        if ch not in ACCENTS:
            return ch.upper()
        for reach in (1, 2, 3):
            seen = {}
            for dy in range(-reach, reach + 1):
                for dx in range(-reach, reach + 1):
                    nx, ny = x + dx, y + dy
                    if 0 <= nx < self.size and 0 <= ny < self.size:
                        other = self.grid[ny][nx]
                        if other and other not in ACCENTS and other != "o":
                            seen[other.upper()] = seen.get(other.upper(), 0) + 1
            if seen:
                return max(sorted(seen), key=lambda m: seen[m])
        raise ValueError("A mark at (%d, %d) has no material near it to take a colour from." % (x, y))

    def _patches(self, owner):
        """The bounding box of every connected patch of one material, for each pixel in it."""
        box = [[None] * self.size for _ in range(self.size)]
        for y in range(self.size):
            for x in range(self.size):
                if owner[y][x] is None or box[y][x] is not None:
                    continue
                todo, cells = [(x, y)], []
                box[y][x] = True
                while todo:
                    cx, cy = todo.pop()
                    cells.append((cx, cy))
                    for nx, ny in ((cx + 1, cy), (cx - 1, cy), (cx, cy + 1), (cx, cy - 1)):
                        if 0 <= nx < self.size and 0 <= ny < self.size and box[ny][nx] is None \
                                and owner[ny][nx] == owner[y][x]:
                            box[ny][nx] = True
                            todo.append((nx, ny))
                xs = [c[0] for c in cells]
                ys = [c[1] for c in cells]
                bounds = (min(xs), min(ys), max(xs), max(ys))
                for cx, cy in cells:
                    box[cy][cx] = bounds
        return box

    def render(self):
        """Shades every pixel from its material and where it sits, puts the outline round the lot
        and the sparkles on top, and returns the rows of RGBA."""
        n = self.size
        owner = [[self._material_of(x, y) for x in range(n)] for y in range(n)]
        boxes = self._patches(owner)
        out = new_image(n, n)

        def same(x, y, m):
            return 0 <= x < n and 0 <= y < n and owner[y][x] == m

        for y in range(n):
            for x in range(n):
                ch = self.grid[y][x]
                if ch is None:
                    continue
                if ch == "o":
                    out[y][x] = list(INK) + [255]
                    continue
                m = owner[y][x]
                ramp = self.materials[m]
                if ch in ACCENTS:
                    level = ACCENTS[ch]
                else:
                    level = 2 if ch.isupper() else 1
                    if not same(x - 1, y, m) or not same(x, y - 1, m):
                        level += 1
                    if not same(x + 1, y, m) or not same(x, y + 1, m):
                        level -= 1
                    x0, y0, x1, y1 = boxes[y][x]
                    if x1 - x0 + 1 >= GRADIENT_SPAN and y1 - y0 + 1 >= GRADIENT_SPAN:
                        t = ((x - x0) / float(x1 - x0) + (y - y0) / float(y1 - y0)) / 2.0
                        if t < GRADIENT_LIGHT:
                            level += 1
                        elif t > GRADIENT_DARK:
                            level -= 1
                    level = max(0, min(3, level))
                out[y][x] = list(ramp[level]) + [255]

        filled = [[self.grid[y][x] is not None for x in range(n)] for y in range(n)]
        for y in range(n):
            for x in range(n):
                if filled[y][x]:
                    continue
                if any(0 <= x + dx < n and 0 <= y + dy < n and filled[y + dy][x + dx]
                       for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1))):
                    out[y][x] = list(INK) + [255]

        for x, y, rgba in self.over:
            put(out, x, y, rgba)
        return out


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


def paste(img, ox, oy, rows):
    """Copies a rendered icon into a sheet."""
    for y, row in enumerate(rows):
        for x, px in enumerate(row):
            if px[3]:
                img[oy + y][ox + x] = list(px)


# The diagonal's grid ------------------------------------------------------- #

def continuous(a, k, axis=AXIS):
    """Where a place on the diagonal's grid is, as a point (it needn't be a pixel's middle)."""
    return (a + k + axis) / 2.0 + 0.5, (k - a + axis) / 2.0 + 0.5


def xy(a, k, axis=AXIS):
    """The pixel at a place on the diagonal's grid."""
    return (a + k + axis) // 2, (k - a + axis) // 2


def centre(a, k, axis=AXIS):
    """The middle of the pixel at a place on the diagonal's grid, for discs and strokes."""
    x, y = xy(a, k, axis)
    return x + 0.5, y + 0.5


def stroke(ic, ch, points, r):
    """A stroke through places on the diagonal's grid."""
    pts = [continuous(a, k) for a, k in points]
    for (x0, y0), (x1, y1) in zip(pts, pts[1:]):
        ic.capsule(ch, x0, y0, x1, y1, r)


# --------------------------------------------------------------------------- #
# Weapons
# --------------------------------------------------------------------------- #
#
# Corner to corner, point to the top right. A blade three pixels across, two for the rapier and
# four for the greatsword; told apart by length, guard and grip as much as by width. Hafts are
# wood, two across, with the head at the top right.

def wrap(ic, a0, a1, k0=-1, k1=0, ch="D"):
    """A grip wound with leather: every other turn a shade darker."""
    ic.diag_where(ch, lambda a, k: a0 <= a <= a1 and k0 <= k <= k1)
    ic.diag_where(ch.lower(), lambda a, k: a0 <= a <= a1 and k0 <= k <= k1 and (a - a0) % 4 in (2, 3))


def pommel(ic, a, ch="G", r=1):
    """A round knob at the end of a grip."""
    ic.diag_where(ch, lambda aa, k: abs(aa - a) + abs(k) <= r + 1 and abs(k) <= r)


def blade(ic, a0, a1, k0=-1, k1=1, ch="S"):
    """A straight blade along the diagonal, its point the last pixel."""
    ic.diag(ch, a0, a1, k0, k1)


def dagger(ic):
    blade(ic, -2, 13)
    ic.diag("+", 8, 11, -1, -1)
    ic.diag("G", -4, -3, -3, 3)
    wrap(ic, -9, -5)
    pommel(ic, -11)


def shortsword(ic):
    blade(ic, -3, 15)
    ic.diag("+", 10, 13, -1, -1)
    ic.diag("G", -5, -4, -4, 4)
    ic.diag("G", -6, -6, -4, -3)
    ic.diag("G", -6, -6, 3, 4)
    wrap(ic, -11, -6)
    pommel(ic, -13)


def longsword(ic):
    blade(ic, -5, 17)
    ic.diag("s", -3, 11, 0, 0)
    ic.diag("+", 13, 16, -1, -1)
    ic.diag("G", -7, -6, -5, 5)
    ic.diag("G", -8, -8, -5, -4)
    ic.diag("G", -8, -8, 4, 5)
    wrap(ic, -14, -8)
    pommel(ic, -16)
    ic.set(*xy(-16, 0), "R")


def greatsword(ic):
    ic.diag_where("S", lambda a, k: -3 <= a <= 17 and (-2 <= k <= 1 if a <= 13 else -1 <= k <= 1))
    ic.diag("s", -1, 12, 0, 0)
    ic.diag("+", 12, 16, -1, -1)
    ic.diag("+", 3, 9, -2, -2)
    ic.diag("I", -6, -4, -6, 5)
    ic.diag("G", -6, -6, -6, 5)
    wrap(ic, -15, -7, ch="L")
    pommel(ic, -17, ch="I")


def rapier(ic):
    blade(ic, -4, 18, -1, 0)
    ic.diag("+", 12, 17, -1, -1)
    ic.diag("G", -6, -5, -5, 5)
    ic.ring("G", *centre(-3, 3), 1.0, 2.2)
    stroke(ic, "G", [(-6, 5), (-9, 6), (-12, 5), (-14, 3)], 0.7)
    wrap(ic, -13, -7)
    pommel(ic, -15, r=1)


def scimitar(ic):
    ic.poly("S", [(8.5, 14.5), (11, 11), (13.5, 8), (16, 5.5), (19, 3.2), (21.6, 2.2),
                  (21, 4.8), (19.6, 7.6), (17.4, 10.6), (14.6, 13.2), (11.6, 15.6)])
    ic.line("+", 12, 10, 18, 4)
    ic.diag("G", -6, -5, -4, 3)
    ic.set(*xy(-7, -4), "G")
    wrap(ic, -12, -7)
    pommel(ic, -14)


def handaxe(ic):
    ic.diag("W", -13, 11, -1, 0)
    wrap(ic, -13, -7, ch="L")
    ic.diag_where("I", lambda a, k: -8 <= k <= -1 and 6 - 0.55 * (-k - 1) <= a <= 10 + 0.45 * (-k - 1))
    ic.diag_where("S", lambda a, k: -9 <= k <= -7 and 6 - 0.55 * (-k - 1) <= a <= 10 + 0.45 * (-k - 1))
    ic.diag("^", 3, 13, -9, -9)
    ic.diag("I", 6, 10, 1, 2)


def battleaxe(ic):
    ic.diag("W", -19, 14, -1, 0)
    wrap(ic, -19, -12, ch="L")
    for side in (-1, 1):
        def head(a, k, side=side):
            d = k * side
            return 1 <= d <= 9 and 7 - 0.7 * (d - 1) <= a <= 12 + 0.6 * (d - 1)
        ic.diag_where("I", head)
        ic.diag_where("S", lambda a, k, h=head, side=side: h(a, k) and k * side >= 7)
    ic.diag("^", 3, 16, -9, -9)
    ic.diag("G", 8, 10, -1, 0)
    ic.diag("S", 14, 17, -1, 0)


def warhammer(ic):
    ic.diag("W", -18, 10, -1, 0)
    wrap(ic, -18, -11, ch="L")
    ic.diag("I", 7, 12, -7, 3)
    ic.diag("S", 7, 12, -8, -7)
    ic.diag_where("I", lambda a, k: 4 <= k <= 8 and abs(a - 9.5) <= (8 - k) * 0.7 + 0.6)
    ic.diag("G", 6, 6, -7, 3)
    ic.diag("I", 13, 15, -1, 0)


def mace(ic):
    ic.diag("I", -17, 6, -1, 0)
    wrap(ic, -17, -9)
    pommel(ic, -18, ch="I", r=1)
    cx, cy = centre(10, 0)
    for dx, dy in ((0, -4), (4, 0), (0, 4), (-4, 0), (3, -3), (3, 3), (-3, 3), (-3, -3)):
        ic.capsule("S", cx, cy, cx + dx * 1.1, cy + dy * 1.1, 1.1, 0.5)
    ic.disc("I", cx, cy, 3.0)
    ic.diag("G", 4, 6, -2, 1)


def quarterstaff(ic):
    ic.diag("W", -18, 18, -1, 1)
    ic.diag("w", -12, 10, 1, 1)
    ic.diag("G", -18, -15, -1, 1)
    ic.diag("G", 15, 18, -1, 1)
    wrap(ic, -6, -2, -1, 1, ch="L")
    wrap(ic, 4, 8, -1, 1, ch="L")


def spear(ic):
    ic.diag("W", -19, 6, -1, 0)
    ic.diag("I", -19, -17, -1, 0)
    ic.diag("G", 5, 8, -1, 1)
    widths = {9: 1, 10: 2, 11: 2, 12: 3, 13: 3, 14: 3, 15: 2, 16: 2, 17: 1, 18: 1, 19: 0}
    ic.diag_where("S", lambda a, k: a in widths and abs(k) <= widths[a])
    ic.diag("^", 10, 18, 0, 0)
    ic.stamp(["RR.",
              "rRR",
              ".rR",
              "..r"], *xy(5, 4))


def bow(ic, span, depth, thick, ch="W", tips=1.4):
    """A bow corner to corner, its string on the diagonal and its back bowed to the top left."""
    points = []
    for i in range(span * 2 + 1):
        a = i - span
        k = -depth * (1 - (a / float(span)) ** 2)
        if abs(a) > span - 3:
            k += tips * (abs(a) - (span - 3)) / 3.0
        points.append(continuous(a, k))
    for (x0, y0), (x1, y1) in zip(points, points[1:]):
        t = abs((x0 - points[span][0])) / float(span)
        ic.capsule(ch, x0, y0, x1, y1, thick * (1.0 - 0.35 * t))


def shortbow(ic):
    bow(ic, 12, 6.5, 1.15)
    ic.line("P", *xy(-12, 0), *xy(12, 0))
    wrap(ic, -2, 2, -7, -5, ch="L")


def longbow(ic):
    bow(ic, 17, 6.0, 1.0)
    ic.line("P", *xy(-17, 0), *xy(17, 0))
    wrap(ic, -2, 2, -7, -5, ch="L")


def light_crossbow(ic):
    ic.diag("W", -17, 6, -1, 1)
    ic.diag("W", -17, -11, -2, 2)
    ic.diag("w", -17, -11, 2, 2)
    stroke(ic, "S", [(1, -10), (5, -6), (7, 0), (5, 6), (1, 10)], 0.8)
    ic.line("P", *xy(1, -10), *xy(-3, 0))
    ic.line("P", *xy(1, 10), *xy(-3, 0))
    ic.diag("I", 7, 9, -1, 1)
    ic.ring("I", *centre(11, 0), 0.8, 1.9)
    ic.diag("I", -8, -6, 2, 3)


def ember_dagger(ic):
    blade(ic, -2, 13, ch="O")
    ic.diag("+", 3, 11, -1, -1)
    ic.diag("o", 1, 8, 0, 0)
    ic.diag("I", -4, -3, -3, 3)
    ic.diag("G", -4, -3, -3, -3)
    ic.diag("G", -4, -3, 3, 3)
    ic.set(*xy(-4, 1), "R")
    wrap(ic, -9, -5)
    pommel(ic, -11, ch="I")
    for a, k, alpha in ((4, -3, 110), (8, 3, 110), (12, -2, 150), (14, 1, 150), (15, 0, 90), (6, -3, 70)):
        ic.glow(*xy(a, k), FIRE[3], alpha)


def thornwood_bow(ic):
    bow(ic, 12, 6.5, 1.15, ch="N")
    ic.line("P", *xy(-12, 0), *xy(12, 0))
    for a in (-9, -5, 5, 9):
        x, y = continuous(a, -6.5 * (1 - (a / 12.0) ** 2) - 2.2)
        ic.set(int(x), int(y), "n")
    wrap(ic, -2, 2, -7, -5, ch="L")
    ic.stamp(["N.", "NN"], *xy(-3, -9))


def sunsteel_blade(ic):
    blade(ic, -5, 17, ch="U")
    ic.diag("u", -3, 11, 0, 0)
    ic.diag("+", 9, 16, -1, -1)
    ic.diag("G", -7, -6, -5, 5)
    ic.diag("G", -8, -8, -5, -4)
    ic.diag("G", -8, -8, 4, 5)
    ic.disc("G", *centre(-7, 0), 1.6)
    ic.set(*xy(-7, 0), "O")
    wrap(ic, -14, -9, ch="B")
    pommel(ic, -16)
    for a, k in ((2, -2), (6, 2), (10, -2), (14, 2), (16, -2)):
        ic.glow(*xy(a, k), SUNSTEEL[3], 110)


# --------------------------------------------------------------------------- #
# Armour
# --------------------------------------------------------------------------- #
#
# Seen from the front, the left half written out and mirrored. Told apart by what they're made of:
# a plain tunic, quilting, leather, studs, shaggy hide, rings, scales, plates and a robe.

def body(ic, half):
    """Lays down the left half of something worn on the body, written as text, and mirrors it."""
    ic.stamp(half)
    ic.mirror()


def travelling_clothes(ic):
    body(ic, ["",
              "",
              "",
              ".......BBB..",
              "......BBBBC.",
              "....BBBBBBBC",
              "...BBBBBBBBB",
              "..BBBBBBBBBB",
              "..BBBBBBBBBB",
              "..BBB.BBBBBB",
              "..bbb.BBBBBB",
              "......BBBBBB",
              "......BBBBBB",
              "......LLLLLL",
              "......LLLLLG",
              "......BBBBBB",
              "......BBBBBB",
              ".....BBBBBBB",
              ".....BBBBBBB",
              ".....bbbbbbb"])


def padded_armour(ic):
    body(ic, ["",
              "",
              "........cc..",
              ".......CCCCC",
              "......CCCCCC",
              "....CCCCCCCC",
              "...CCCCCCCCC",
              "..CCCCCCCCCC",
              "..CCCCCCCCCC",
              "..CCC.CCCCCC",
              "..CCC.CCCCCC",
              "..CCC.CCCCCC",
              "..ccc.CCCCCC",
              "......CCCCCC",
              "......CCCCCC",
              "......CCCCCC",
              "......CCCCCC",
              ".....CCCCCCC",
              ".....CCCCCCC",
              ".....ccccccc"])
    ic.pattern("C", "c", lambda x, y: y >= 5 and ((x + y) % 4 == 0 or (x - y) % 4 == 0))
    for y in range(4, 19, 2):
        ic.set(11, y, "o")
        ic.set(12, y + 1, "o")


def leather_armour(ic):
    body(ic, ["",
              "",
              "",
              "......LL....",
              ".....LLLL...",
              "...LLLLLLLLL",
              "..LLLLLLLLLL",
              "..LLLlLLLLLL",
              "..LLL.LLLLLL",
              "...l..LLLLLL",
              "......LLLLLL",
              "......LLLLLL",
              "......LLLLLL",
              "......DDDDDD",
              "......DDDDDG",
              "......LLLLLL",
              "......LLLLLL",
              "......lLLlLL",
              "......lLLlLL",
              "......l..l.."])
    for y in range(6, 13):
        ic.set(8, y, "l")
        ic.set(15, y, "l")


def studded_leather(ic):
    leather_armour(ic)
    ic.pattern("L", "D", lambda x, y: True)
    ic.pattern("l", "d", lambda x, y: True)
    for y in range(6, 18, 3):
        for x in range(6, 18, 3):
            if ic.grid[y][x] in ("D", "d") and ic.grid[y][x + 1] in ("D", "d"):
                ic.set(x, y, "S")
    ic.stamp(["SS", "Ss"], 3, 6)
    ic.stamp(["SS", "sS"], 19, 6)


def hide_armour(ic):
    body(ic, ["",
              "",
              "......P.P...",
              ".....PPPPPP.",
              "...FPPPPPPP.",
              "..FFFFPFFFFF",
              "..FFFFFFFFFF",
              "..FFFFFFFFFF",
              "..FFFFFFFFFF",
              "..fFf.FFFFFF",
              "...f..FFFFFF",
              "......FFFFFF",
              "......FFFFFF",
              "......LLLLLL",
              "......FFFFFF",
              "......FFFFFF",
              ".....FFFFFFF",
              ".....FFFfFFF",
              ".....fFf..fF",
              ".......f...."])
    ic.pattern("F", "f", lambda x, y: (x * 7 + y * 3) % 11 == 0)
    ic.stamp(["o", "o", "o"], 9, 7)
    ic.stamp(["o", "o", "o"], 14, 8)


def mail(x, y):
    """The rings of mail: a lit ring and a dark gap, every row offset from the last."""
    return (x + (y // 2)) % 2 == 1 and y % 2 == 0 or (x + (y // 2)) % 2 == 0 and y % 2 == 1


def chain_shirt(ic):
    body(ic, ["",
              "",
              "",
              ".......LLL..",
              "......SSSLL.",
              "....SSSSSSSL",
              "...SSSSSSSSS",
              "..SSSSSSSSSS",
              "..SSSSSSSSSS",
              "..SSS.SSSSSS",
              "..sss.SSSSSS",
              "......SSSSSS",
              "......SSSSSS",
              "......SSSSSS",
              "......SSSSSS",
              "......SSSSSS",
              "......SSSSSS",
              "......ssssss"])
    ic.pattern("S", "s", lambda x, y: y >= 5 and mail(x, y))


def scale_mail(ic):
    body(ic, ["",
              "",
              "",
              ".......LLL..",
              "......GGGLL.",
              "....GGGGGGGL",
              "...GGGGGGGGG",
              "..GGGGGGGGGG",
              "..GGGGGGGGGG",
              "..GGG.GGGGGG",
              "..GGG.GGGGGG",
              "..ggg.GGGGGG",
              "......GGGGGG",
              "......GGGGGG",
              "......GGGGGG",
              "......GGGGGG",
              "......GGGGGG",
              ".....GGGGGGG",
              ".....GGGGGGG",
              ".....ggggggg"])

    def scale_edge(x, y):
        row = y // 3
        u = (x + (2 if row % 2 else 0)) % 4
        v = y % 3
        return v == 2 and u in (1, 2) or v == 1 and u in (0, 3) or v == 0 and u == 0
    ic.pattern("G", "g", lambda x, y: y >= 5 and scale_edge(x, y))


def chain_mail(ic):
    body(ic, ["",
              "",
              "........II..",
              "......IIIIII",
              ".....IIIIIII",
              "...IIIIIBBBB",
              "..IIIIIIBBBB",
              "..IIIIIIBBBB",
              "..IIIIIIBBBB",
              "..III.IIBBBB",
              "..III.IIBBBB",
              "..III.IIBBBB",
              "..iii.IIBBBB",
              "......GGGGGG",
              "......IIBBBB",
              "......IIBBBB",
              ".....IIIBBBB",
              ".....IIIBBBB",
              ".....IIIBBBB",
              ".....iii.bbb"])
    ic.pattern("I", "i", lambda x, y: y >= 4 and mail(x, y))
    ic.stamp(["GG", "GG"], 11, 8)


def plate_armour(ic):
    body(ic, ["",
              "",
              "",
              "........SSSS",
              "...SSSoSSSSS",
              "..SSSSSoSSSS",
              "..SSSSSoSSSS",
              "..SSSSSoSSSS",
              "...sssssSSSS",
              "....s.SSSSSS",
              "......SSSSSS",
              "......SSSSSS",
              "......SSSSSS",
              "......GGGGGG",
              "......SSSSSS",
              "......ssssss",
              "......SSSSSS",
              "......ssssss",
              "......SSS.SS",
              "......sss.ss"])
    for y in range(4, 13):
        ic.set(11, y, "^")
    ic.set(4, 5, "+")
    ic.set(3, 6, "^")


def mage_robes(ic):
    body(ic, ["",
              "",
              "......VVV...",
              "......VVVGG.",
              ".....VVVVVGG",
              "....VVVVVVVG",
              "...VVVVVVVVG",
              "...VVVVVVVVG",
              "..VVVVVVVVVG",
              "..VVV.VVVVVG",
              "..VVV.VVVVVG",
              "..GGG.VVVVVG",
              "......VVVVVG",
              "......GGGGGG",
              "......VVVVVG",
              ".....VVVVVVG",
              ".....VVVVVVG",
              "....VVVVVVVG",
              "....VVVVVVVG",
              "....GGGGGGGG"])
    ic.stamp(["^", "+", "^"], 7, 8)
    ic.stamp([".^.", "^+^", ".^."], 15, 7)


# --------------------------------------------------------------------------- #
# Shields
# --------------------------------------------------------------------------- #

HEATER = [(4.0, 3.0), (20.0, 3.0), (20.0, 11.0), (18.6, 15.0), (15.6, 18.4), (12.0, 21.0),
          (8.4, 18.4), (5.4, 15.0), (4.0, 11.0)]


def heater(ic, rim, field, inset=1.6):
    """A heater shield: the rim, and the field inside it."""
    ic.poly(rim, HEATER)
    cx = 12.0
    inner = [(cx + (x - cx) * (1 - inset / 8.0), 3.0 + (y - 3.0) * (1 - inset / 18.0) + inset * 0.9) for x, y in HEATER]
    ic.poly(field, inner)


def wooden_shield(ic):
    ic.disc("I", 12.0, 12.0, 9.4)
    ic.disc("W", 12.0, 12.0, 8.0)
    ic.pattern("W", "w", lambda x, y: x in (7, 11, 15) and 4 <= y <= 19)
    ic.disc("I", 12.0, 12.0, 2.8)
    ic.set(11, 11, "+")
    for x, y in ((12, 3), (12, 20), (3, 12), (20, 12), (6, 6), (17, 6), (6, 17), (17, 17)):
        ic.set(x, y, "^")


def steel_shield(ic):
    heater(ic, "I", "S")
    ic.line("s", 12, 5, 12, 19)
    ic.line("^", 11, 5, 11, 19)
    for x, y in ((6, 5), (17, 5), (6, 11), (17, 11)):
        ic.set(x, y, "-")
    ic.set(7, 6, "+")


def warden_shield(ic):
    heater(ic, "G", "B")
    ic.stamp(["...G...",
              "..GGG..",
              ".GGGGG.",
              "GGG+GGG",
              ".GGGGG.",
              "..GGG..",
              "...G..."], 9, 7)
    ic.stamp(["G", "G", "G"], 12, 14)
    ic.set(7, 6, "+")


# --------------------------------------------------------------------------- #
# Heads, hands and feet
# --------------------------------------------------------------------------- #

def leather_cap(ic):
    ic.stamp(["........LLLLLLLL........",
              "......LLLLLLLLLLLL......",
              ".....LLLLLLLLLLLLLL.....",
              "....LLLLLLLLLLLLLLLL....",
              "....LLLLLLLLLLLLLLLL....",
              "...LLLLLLLLLLLLLLLLLL...",
              "...LLLLLLLLLLLLLLLLLL...",
              "...DDDDDDDDDDDDDDDDDD...",
              "...DDDDDDDDDDDDDDDDDD...",
              "...LL..............LL...",
              "...LL..............LL...",
              "...LL..............LL...",
              "....L..............L...."], 0, 5)
    ic.line("l", 12, 5, 12, 11)
    ic.line("l", 8, 7, 7, 11)
    ic.line("l", 16, 7, 17, 11)
    ic.set(11, 4, "D")
    ic.set(12, 4, "D")


def iron_helm(ic):
    ic.stamp([".........SSSSSS.........",
              ".......SSSSSSSSSS.......",
              "......SSSSSSSSSSSS......",
              ".....SSSSSSSSSSSSSS.....",
              ".....SSSSSSSSSSSSSS.....",
              "....SSSSSSSSSSSSSSSS....",
              "....GGGGGGGGGGGGGGGG....",
              "....SooooooSSooooooS....",
              "....SSSSSSSSSSSSSSSS....",
              "....SSSSSSSSSSSSSSSS....",
              ".....SSSSSoSSoSSSSS.....",
              ".....SSSSSoSSoSSSSS.....",
              "......SSSSSSSSSSSS......",
              ".......IIIIIIIIII.......",
              "......IIIIIIIIIIII......",
              "......iiiiiiiiiiii......"], 0, 3)
    ic.line("^", 11, 3, 11, 8)
    ic.line("s", 12, 3, 12, 8)
    ic.line("s", 11, 11, 11, 15)
    ic.line("^", 12, 11, 12, 15)
    ic.set(8, 5, "+")
    ic.set(7, 6, "^")


def circlet_of_focus(ic):
    ic.fill("G", lambda px, py: ((px - 12.0) / 9.6) ** 2 + ((py - 13.0) / 5.0) ** 2 <= 1.0
            and ((px - 12.0) / 8.0) ** 2 + ((py - 13.4) / 3.4) ** 2 > 1.0)
    ic.stamp(["..G..",
              ".GVG.",
              "GVVVG",
              ".GVG.",
              "..G.."], 10, 5)
    ic.set(11, 6, "+")
    ic.stamp(["G", "V"], 5, 9)
    ic.stamp(["G", "V"], 18, 9)


def glove(ic, fingers, band, cuff):
    """A glove upright, its back to you: three fingers and a little one, a thumb out to the left,
    a band over the knuckles and a flared cuff."""
    ic.stamp(["........XX.XX.XX........",
              "........XX.XX.XX.XX.....",
              "........XX.XX.XX.XX.....",
              "........xx.xx.xx.XX.....",
              "........XX.XX.XX.xx.....",
              "...XX...XXXXXXXXXXX.....",
              "...XXX..YYYYYYYYYYY.....",
              "....XXX.XXXXXXXXXXX.....",
              ".....XXXXXXXXXXXXXX.....",
              "......XXXXXXXXXXXXX.....",
              ".......XXXXXXXXXXXX.....",
              "........XXXXXXXXXX......",
              "........ZZZZZZZZZZ......",
              ".......ZZZZZZZZZZZZ.....",
              "......ZZZZZZZZZZZZZZ....",
              "......zzzzzzzzzzzzzz...."], 0, 3)
    for y in range(ic.size):
        for x in range(ic.size):
            ch = ic.grid[y][x]
            if ch in ("X", "x"):
                ic.grid[y][x] = fingers if ch == "X" else fingers.lower()
            elif ch == "Y":
                ic.grid[y][x] = band
            elif ch in ("Z", "z"):
                ic.grid[y][x] = cuff if ch == "Z" else cuff.lower()


def leather_gloves(ic):
    glove(ic, "L", "D", "D")
    for x in (11, 14):
        ic.set(x, 12, "l")
        ic.set(x, 13, "l")


def gauntlets_of_might(ic):
    glove(ic, "S", "G", "I")
    ic.stamp(["RR", "RR"], 12, 12)
    ic.set(12, 12, "+")
    ic.set(9, 11, "^")


def boot(ic, ox, oy, leg, cuff, sole):
    """One boot side on, toe to the left: a turned-down cuff, the shaft, the foot and the sole,
    with a gap under the arch before the heel."""
    ic.stamp(["....ZZZZZZZ",
              "....ZZZZZZZ",
              ".....XXXXX.",
              ".....XXXXX.",
              ".....XXXXX.",
              ".....XXXXX.",
              ".....XXXXX.",
              "....XXXXXX.",
              "..XXXXXXXX.",
              ".XXXXXXXXXX",
              "XXXXXXXXXXX",
              "YYYYYY..YYY"], ox, oy)
    for y in range(ic.size):
        for x in range(ic.size):
            ch = ic.grid[y][x]
            if ch == "X":
                ic.grid[y][x] = leg
            elif ch == "Z":
                ic.grid[y][x] = cuff
            elif ch == "Y":
                ic.grid[y][x] = sole


def leather_boots(ic):
    boot(ic, 10, 6, "l", "d", "d")
    boot(ic, 3, 9, "L", "D", "D")
    ic.line("l", 6, 18, 12, 18)


def iron_boots(ic):
    boot(ic, 10, 6, "s", "i", "i")
    boot(ic, 3, 9, "S", "I", "I")
    for y in (13, 16):
        ic.line("s", 8, y, 12, y)
    ic.line("s", 5, 17, 5, 19)
    ic.line("s", 7, 17, 7, 19)
    ic.set(9, 12, "+")


def boots_of_striding(ic):
    boot(ic, 10, 6, "l", "g", "d")
    boot(ic, 3, 9, "L", "G", "D")
    ic.stamp(["....PP",
              "..PPPP",
              ".PPPP.",
              "PPPp..",
              ".p...."], 13, 11)
    ic.set(17, 11, "+")


# --------------------------------------------------------------------------- #
# Cloaks, the amulet and the ring
# --------------------------------------------------------------------------- #

def cloak(ic, outside, lining, clasp, trim=None):
    """A cloak hung up by its clasp, the hood up and empty, open down the front on its lining."""
    ic.stamp([".........XXXXXX.........",
              "........XXXXXXXX........",
              ".......XXX----XXX.......",
              ".......XX------XX.......",
              ".......XX------XX.......",
              ".......XXX----XXX.......",
              "......XXXXXCCXXXXX......",
              ".....XXXXXXYYXXXXXX.....",
              ".....XXXXXXYYXXXXXX.....",
              "....XXXXXXYYYYXXXXXX....",
              "....XXXXXXYYYYXXXXXX....",
              "....XXXXXYYYYYYXXXXX....",
              "...XXXXXXYYYYYYXXXXXX...",
              "...XXXXXYYYYYYYYXXXXX...",
              "...XXXXXYYYYYYYYXXXXX...",
              "..XXXXXXYYYYYYYYXXXXXX..",
              "..XXXXXYYYYYYYYYYXXXXX..",
              "..xXXxXyYyYyYyYyYxXXxX.."], 0, 3)
    for y in range(ic.size):
        for x in range(ic.size):
            ch = ic.grid[y][x]
            if ch in ("X", "x"):
                ic.grid[y][x] = outside if ch == "X" else outside.lower()
            elif ch in ("Y", "y"):
                ic.grid[y][x] = lining if ch == "Y" else lining.lower()
            elif ch == "C":
                ic.grid[y][x] = clasp
    for y in range(13, 20):
        ic.set(6, y, outside.lower())
        ic.set(17, y, outside.lower())
    if trim:
        for y in range(10, 21):
            for x in range(ic.size):
                if ic.grid[y][x] == outside and x < ic.size - 1 and ic.grid[y][x + 1] in (lining, lining.lower()):
                    ic.grid[y][x] = trim
                if ic.grid[y][x] == outside and x > 0 and ic.grid[y][x - 1] in (lining, lining.lower()):
                    ic.grid[y][x] = trim


def travelling_cloak(ic):
    cloak(ic, "N", "D", "G")


def cloak_of_protection(ic):
    cloak(ic, "B", "V", "S", trim="G")
    ic.stamp(["SS"], 11, 9)
    ic.set(11, 9, "+")


def amulet_of_vigour(ic):
    for i in range(9):
        t = i / 8.0
        x = 5 + 7 * t
        y = 3 + 9 * math.sin(t * math.pi / 2)
        ic.set(int(round(x)), int(round(y)), "G")
        ic.set(23 - int(round(x)), int(round(y)), "G")
    ic.stamp(["....GGG....",
              "...GGGGG...",
              "..GRRGRRG..",
              ".GRRRRRRRG.",
              ".GRRRRRRRG.",
              "..GRRRRRG..",
              "...GRRRG...",
              "....GRG....",
              ".....G....."], 7, 11)
    ic.set(9, 14, "+")
    ic.set(10, 14, "^")


def ring_of_protection(ic):
    ic.fill("G", lambda px, py: ((px - 12.0) / 7.5) ** 2 + ((py - 15.0) / 5.5) ** 2 <= 1.0
            and ((px - 12.0) / 5.4) ** 2 + ((py - 15.4) / 3.6) ** 2 > 1.0)
    ic.stamp(["..GGG..",
              ".GBBBG.",
              "GBBBBBG",
              "GBBBBBG",
              ".GBBBG.",
              "..GGG.."], 9, 4)
    ic.set(11, 6, "+")
    ic.set(10, 7, "^")


# --------------------------------------------------------------------------- #
# Potions and the rest
# --------------------------------------------------------------------------- #

def flask(ic, liquid, level=13.0, cork="W"):
    """A round flask: glass, the liquid to `level`, a neck and a cork."""
    ic.disc("E", 12.0, 14.5, 6.6)
    ic.rect("E", 10, 5, 13, 9)
    ic.fill(liquid, lambda px, py: (px - 12.0) ** 2 + (py - 14.5) ** 2 <= 6.6 ** 2 and py >= level)
    ic.rect(cork, 10, 3, 13, 5)
    ic.rect("G", 9, 6, 14, 6)
    ic.stamp(["+", "^", "^"], 8, 11)


def healing_potion(ic):
    flask(ic, "R")


def greater_healing_potion(ic):
    ic.fill("E", lambda px, py: ((px - 12.0) / 7.0) ** 2 + ((py - 15.0) / 6.4) ** 2 <= 1.0)
    ic.rect("E", 10, 4, 13, 8)
    ic.rect("E", 9, 8, 14, 9)
    ic.fill("R", lambda px, py: ((px - 12.0) / 7.0) ** 2 + ((py - 15.0) / 6.4) ** 2 <= 1.0 and py >= 11.0)
    ic.rect("G", 9, 2, 14, 3)
    ic.rect("G", 10, 1, 13, 1)
    ic.rect("G", 9, 7, 14, 7)
    ic.stamp(["+", "^", "^"], 7, 12)


def alchemists_fire(ic):
    flask(ic, "O", level=12.0, cork="C")
    ic.stamp(["..O..",
              ".OOO.",
              ".OO+O",
              "OO+OO",
              ".O+O."], 9, 0)


def elixir_of_focus(ic):
    ic.rect("E", 9, 5, 14, 19)
    ic.rect("V", 9, 9, 14, 19)
    ic.rect("G", 8, 2, 15, 4)
    ic.rect("G", 9, 20, 14, 20)
    ic.stamp(["+", "^", "^", "^"], 10, 6)
    ic.set(12, 12, "+")
    ic.set(11, 15, "^")
    ic.set(13, 17, "+")


def wolf_pelt(ic):
    ic.stamp([".........FF..FF.........",
              "........FFFFFFFF........",
              "........FFFFFFFF........",
              ".........FFFFFF.........",
              "...FF....FFFFFF....FF...",
              "....FFFFFFFFFFFFFFFF....",
              ".....FFFFFFFFFFFFFF.....",
              "......FFFFFFFFFFFF......",
              "......FFFFFFFFFFFF......",
              "......FFFFFFFFFFFF......",
              "......FFFFFFFFFFFF......",
              "......FFFFFFFFFFFF......",
              ".....FFFFFFFFFFFFFF.....",
              "....FFFFFFFFFFFFFFFF....",
              "...FF..FFFFFFFFFF..FF...",
              ".........FFFFFF.........",
              "..........FFFF..........",
              "...........FF...........",
              "...........FF..........."], 0, 2)
    ic.pattern("F", "f", lambda x, y: x in (11, 12) and 6 <= y <= 19)
    ic.set(10, 4, "o")
    ic.set(13, 4, "o")


def black_crystal_shard(ic):
    ic.poly("K", [(11.0, 1.5), (15.5, 7.0), (17.0, 15.0), (13.5, 21.5), (8.5, 19.5), (6.5, 12.0), (8.0, 6.0)])
    ic.poly("k", [(11.0, 1.5), (15.5, 7.0), (17.0, 15.0), (13.5, 21.5), (12.0, 12.0)])
    ic.line("^", 11, 3, 9, 12)
    ic.line("+", 10, 5, 10, 7)
    for x, y, a in ((5, 12, 120), (6, 8, 90), (18, 14, 120), (17, 8, 90), (12, 22, 80), (7, 18, 100)):
        ic.glow(x, y, CRYSTAL[3], a)


# --------------------------------------------------------------------------- #
# icons.png
# --------------------------------------------------------------------------- #

#: Icons to a row on the sheet.
COLUMNS = 16

#: Every item: its id, how it's drawn, its rarity, and where its glint goes if it has one. In
#: ItemCatalog's order, which has to match: an item's Icon is its place here, counting from 0.
#: New items go on the end, never in the middle, or every icon after them moves.
ITEMS = (
    ("dagger", dagger, COMMON, None),
    ("shortsword", shortsword, COMMON, None),
    ("longsword", longsword, COMMON, None),
    ("rapier", rapier, COMMON, None),
    ("scimitar", scimitar, COMMON, None),
    ("handaxe", handaxe, COMMON, None),
    ("battleaxe", battleaxe, COMMON, None),
    ("greatsword", greatsword, COMMON, None),
    ("warhammer", warhammer, COMMON, None),
    ("mace", mace, COMMON, None),
    ("quarterstaff", quarterstaff, COMMON, None),
    ("spear", spear, COMMON, None),
    ("shortbow", shortbow, COMMON, None),
    ("longbow", longbow, COMMON, None),
    ("light-crossbow", light_crossbow, COMMON, None),
    ("ember-dagger", ember_dagger, UNCOMMON, (16, 6)),
    ("thornwood-bow", thornwood_bow, UNCOMMON, (15, 4)),
    ("sunsteel-blade", sunsteel_blade, RARE, (17, 5)),
    ("travelling-clothes", travelling_clothes, COMMON, None),
    ("padded-armour", padded_armour, COMMON, None),
    ("leather-armour", leather_armour, COMMON, None),
    ("studded-leather", studded_leather, COMMON, None),
    ("hide-armour", hide_armour, COMMON, None),
    ("chain-shirt", chain_shirt, COMMON, None),
    ("scale-mail", scale_mail, COMMON, None),
    ("chain-mail", chain_mail, COMMON, None),
    ("plate-armour", plate_armour, COMMON, None),
    ("mage-robes", mage_robes, COMMON, None),
    ("wooden-shield", wooden_shield, COMMON, None),
    ("steel-shield", steel_shield, COMMON, None),
    ("warden-shield", warden_shield, UNCOMMON, (17, 5)),
    ("leather-cap", leather_cap, COMMON, None),
    ("iron-helm", iron_helm, COMMON, None),
    ("circlet-of-focus", circlet_of_focus, UNCOMMON, (13, 6)),
    ("leather-gloves", leather_gloves, COMMON, None),
    ("gauntlets-of-might", gauntlets_of_might, UNCOMMON, (16, 9)),
    ("leather-boots", leather_boots, COMMON, None),
    ("iron-boots", iron_boots, COMMON, None),
    ("boots-of-striding", boots_of_striding, UNCOMMON, (18, 7)),
    ("travelling-cloak", travelling_cloak, COMMON, None),
    ("cloak-of-protection", cloak_of_protection, UNCOMMON, (16, 9)),
    ("amulet-of-vigour", amulet_of_vigour, UNCOMMON, (14, 13)),
    ("ring-of-protection", ring_of_protection, RARE, (13, 6)),
    ("healing-potion", healing_potion, COMMON, None),
    ("greater-healing-potion", greater_healing_potion, UNCOMMON, (15, 14)),
    ("alchemists-fire", alchemists_fire, COMMON, None),
    ("elixir-of-focus", elixir_of_focus, UNCOMMON, (13, 8)),
    ("wolf-pelt", wolf_pelt, COMMON, None),
    ("black-crystal-shard", black_crystal_shard, COMMON, None),
)


def item_icon(draw, rarity, glint):
    """One item's icon, rendered."""
    ic = Icon()
    draw(ic)
    if rarity != COMMON:
        ic.sparkle(glint[0], glint[1], rarity)
    return ic.render()


def build_icons():
    rows = (len(ITEMS) + COLUMNS - 1) // COLUMNS
    img = new_image(COLUMNS * ICON, rows * ICON)
    for i, (_, draw, rarity, glint) in enumerate(ITEMS):
        paste(img, (i % COLUMNS) * ICON, (i // COLUMNS) * ICON, item_icon(draw, rarity, glint))
    write_png(os.path.join(OUT, "Items", "icons.png"), img)


# --------------------------------------------------------------------------- #
# slots.png
# --------------------------------------------------------------------------- #
#
# One row of 24x24 cells for the inventory and the character sheet. The first three are the slot
# itself, a well sunk into the panel: empty, under the mouse, and picked. Then, in EquipmentSlot's
# order, the faint outline an empty equipment slot shows of what goes in it, and last the rims an
# uncommon and a rare thing's slot gets. The outlines and rims are drawn over a slot and under
# the item, so a slot is: the well, then the outline if it's empty or the rim if what's in it is
# magic, then the icon.

#: The panel's colours (generate_ui.py's SHELL_DK, WELL, WELL_LT and BRONZE), and a brighter well
#: so an icon's outline still shows against it.
SLOT_EDGE = (14, 16, 36)
SLOT_WELL = (30, 36, 70)
SLOT_SHADOW = (20, 24, 50)
SLOT_LIP = (52, 62, 108)
SLOT_BRONZE = (206, 162, 82)
SLOT_GOLD = (246, 204, 114)

#: The faint outline of what goes in an empty slot: two shades of the well, lit and unlit.
SILHOUETTE = ((54, 64, 112), (70, 82, 136))

#: An uncommon thing's rim and a rare one's: green and blue, like every RPG's.
RIM_UNCOMMON = (142, 196, 92)
RIM_RARE = (110, 168, 240)

#: What goes in each equipment slot, in EquipmentSlot's order: the icon its outline is cut from.
SLOT_OUTLINES = (
    ("main-hand", longsword),
    ("off-hand", steel_shield),
    ("body", chain_shirt),
    ("head", iron_helm),
    ("hands", leather_gloves),
    ("feet", leather_boots),
    ("cloak", travelling_cloak),
    ("amulet", amulet_of_vigour),
    ("ring", ring_of_protection),
)

#: The cells, in order. Has to match what the inventory screen reads.
SLOTS = ("slot", "slot-hover", "slot-selected") + tuple(name for name, _ in SLOT_OUTLINES) + ("rim-uncommon", "rim-rare")


def _slot_well(img, ox):
    """The well: a hard dark edge, sunk so its top and left inner sides are in shadow and its
    bottom and right lips catch the light."""
    n = ICON
    for y in range(n):
        for x in range(n):
            if x in (0, n - 1) or y in (0, n - 1):
                c = SLOT_EDGE
            elif x == 1 or y == 1:
                c = SLOT_SHADOW
            elif x == n - 2 or y == n - 2:
                c = SLOT_LIP
            else:
                c = SLOT_WELL
            img[y][ox + x] = list(c) + [255]


def _slot_edge(img, ox, colour, alpha, corners):
    """A rim of light just inside the hard edge, and brighter corners."""
    n = ICON
    for i in range(1, n - 1):
        for x, y in ((i, 1), (i, n - 2), (1, i), (n - 2, i)):
            put(img, ox + x, y, colour + (alpha,))
    for x, y in ((1, 1), (n - 2, 1), (1, n - 2), (n - 2, n - 2)):
        put(img, ox + x, y, corners + (255,))


def _slot_outline(img, ox, draw):
    """The faint outline of an icon: its shape in two dark shades, no outline, no sparkle."""
    rows = Icon()
    draw(rows)
    rendered = rows.render()
    for y in range(ICON):
        for x in range(ICON):
            r, g, b, a = rendered[y][x]
            if not a or (r, g, b) == INK:
                continue
            lit = (r * 299 + g * 587 + b * 114) / 1000.0 > 150
            img[y][ox + x] = list(SILHOUETTE[1 if lit else 0]) + [255]


def build_slots():
    img = new_image(len(SLOTS) * ICON, ICON)
    _slot_well(img, 0)
    _slot_well(img, ICON)
    _slot_edge(img, ICON, SLOT_BRONZE, 150, SLOT_BRONZE)
    _slot_well(img, ICON * 2)
    _slot_edge(img, ICON * 2, SLOT_GOLD, 255, GLINT)
    for i, (_, draw) in enumerate(SLOT_OUTLINES):
        _slot_outline(img, ICON * (3 + i), draw)
    for i, colour in enumerate((RIM_UNCOMMON, RIM_RARE)):
        _slot_edge(img, ICON * (3 + len(SLOT_OUTLINES) + i), colour, 170, colour)
    write_png(os.path.join(OUT, "Items", "slots.png"), img)


# --------------------------------------------------------------------------- #
# Checking the catalogue
# --------------------------------------------------------------------------- #

CATALOG = os.path.join(ROOT, "Items", "ItemCatalog.cs")


def check_catalog():
    """Stops with what's different if ItemCatalog's items, icons or rarities don't match ITEMS."""
    with open(CATALOG, encoding="utf-8") as f:
        text = f.read()
    names = dict(re.findall(r'const string (\w+) = "([^"]+)";', text))
    found = []
    for block in re.split(r"\bnew\(\)", text)[1:]:
        item = re.search(r'\bId = (?:"([^"]+)"|(\w+))', block)
        icon = re.search(r"\bIcon = (\d+)", block)
        if item and icon:
            rarity = re.search(r"\bRarity = Rarity\.(\w+)", block)
            found.append((item.group(1) or names.get(item.group(2), item.group(2)), int(icon.group(1)),
                          rarity.group(1).lower() if rarity else COMMON))

    problems = []
    if len(found) != len(ITEMS):
        problems.append("ItemCatalog has %d items with icons, and this script draws %d" % (len(found), len(ITEMS)))
    for item_id, icon, rarity in found:
        if not 0 <= icon < len(ITEMS):
            problems.append("%s has icon %d, which isn't drawn" % (item_id, icon))
        elif ITEMS[icon][0] != item_id:
            problems.append("%s has icon %d, which is %s's here" % (item_id, icon, ITEMS[icon][0]))
        elif ITEMS[icon][2] != rarity:
            problems.append("%s is %s in ItemCatalog and %s here" % (item_id, rarity, ITEMS[icon][2]))
    if problems:
        sys.exit("generate_items: the icons and ItemCatalog don't match, so nothing was written.\n  "
                 + "\n  ".join(problems))


if __name__ == "__main__":
    print("Generating item icons for Legend Quest...")
    check_catalog()
    build_icons()
    build_slots()
    print("Done.")
