"""
Combat art for Legend Quest: the ability icons, the condition icons, and the pieces the fight's
screen is built from (the hotbar's slots, the initiative portraits, the action, bonus and reaction
pips, the movement bar, death saves, the Soul-Link sigil, the rings and dots on the ground, and
the things that fly).
Run it from the repo root:

    python tools/generate_combat_art.py

Combat/abilities.png: one 24x24 tile a cell, sixteen to a row, in AbilityId's order, so an
ability's icon is the cell numbered (int)its id. The two icons that aren't abilities (End Turn,
and Attack with a bow in hand) start the fourth row, out of the way of new abilities.
Combat/conditions.png: one 12x12 icon a cell, in ConditionId's order.
Combat/hud.png: everything else, each piece named and placed in HUD below.

Combat/CombatIcons.cs is the game's index of all three. Before it draws a thing this script reads
that file and Enums/AbilityId.cs and Enums/ConditionId.cs, and stops with what's different if the
names or the places don't match its own, so the sheets and the code can't drift apart.

The icons use generate_items.py's kit, so a sword on the hotbar is drawn like a sword in the bag:
a material letter a pixel, shaded at the end from the top left, a dark outline round it. An
ability's tile behind it is the colour of what kind of thing it is (steel blue for weapons and
footwork, red for fire, gold for radiance, green for healing, and so on), lit in bands from just
above the middle, with a dark edge and a bevel like the buttons.

Only needs CPython, and there's nothing random in it, so a rerun writes the same bytes.
"""

import math
import os
import re
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from pixels import OUT, ROOT, lerp_color, new_image, put, write_png  # noqa: E402
from generate_items import (FIRE, GLINT, GOLD, ICE, ICON, INK, RED, ROSE, SUNSTEEL, VIOLET, Icon,  # noqa: E402
                            blade, boot, dagger, elixir_of_focus, glove, healing_potion, inside, paste,
                            plate_armour, pommel, shortbow, wrap)


#: Skin, for hands: the style sheet's skin row.
SKIN = ((103, 62, 57), (156, 96, 76), (204, 136, 104), (236, 184, 156), (250, 216, 194))

#: The extra materials the ability icons use: H for skin, Y for smoke.
SMOKE = ((70, 66, 80), (104, 100, 112), (144, 140, 150), (186, 182, 190), (226, 224, 230))
EXTRA = {"H": SKIN, "Y": SMOKE}


# --------------------------------------------------------------------------- #
# The tiles behind the ability icons
# --------------------------------------------------------------------------- #

#: Each kind of ability's tile, darkest band to lightest.
SCHOOLS = {
    "weapon": ((20, 26, 56), (32, 40, 82), (44, 56, 108), (64, 82, 144)),
    "shadow": ((16, 16, 28), (28, 28, 46), (42, 42, 66), (62, 64, 94)),
    "fire": ((68, 18, 20), (108, 32, 26), (150, 54, 32), (194, 90, 44)),
    "frost": ((14, 40, 84), (22, 64, 120), (34, 96, 158), (62, 138, 196)),
    "radiant": ((86, 50, 24), (126, 80, 34), (168, 114, 48), (212, 158, 72)),
    "heal": ((16, 54, 48), (24, 82, 64), (36, 114, 80), (62, 150, 96)),
    "arcane": ((30, 24, 80), (48, 40, 118), (70, 58, 158), (102, 88, 200)),
    "eldritch": ((38, 12, 50), (62, 20, 80), (90, 32, 110), (126, 52, 146)),
    "nature": ((22, 50, 26), (34, 76, 36), (50, 104, 46), (76, 138, 60)),
    "song": ((68, 18, 50), (104, 32, 76), (142, 50, 104), (184, 80, 136)),
    "foe": ((36, 14, 20), (60, 22, 30), (86, 32, 40), (118, 48, 52)),
    "soul": ((34, 20, 62), (56, 34, 96), (82, 52, 132), (118, 80, 170)),
}

#: Where a tile's light is brightest, and how far out each band reaches from there.
TILE_LIGHT = (11.0, 10.0)
TILE_BANDS = (5.0, 8.5, 12.0)

#: How much the bevel lightens a tile's top and left inside edge, and darkens its bottom and right.
TILE_BEVEL_LIGHT = 0.22
TILE_BEVEL_DARK = 0.35


def tile(school):
    """An ability's tile: banded light from just above the middle, a dark edge with its corners
    cut, and a bevel lit from the top left."""
    bands = SCHOOLS[school]
    n = ICON
    img = new_image(n, n)
    for y in range(n):
        for x in range(n):
            if x in (0, n - 1) and y in (0, n - 1):
                continue
            if x in (0, n - 1) or y in (0, n - 1):
                img[y][x] = list(INK) + [255]
                continue
            d = math.hypot(x + 0.5 - TILE_LIGHT[0], y + 0.5 - TILE_LIGHT[1])
            band = 3 - sum(1 for edge in TILE_BANDS if d > edge)
            c = bands[band]
            if x == 1 or y == 1:
                c = lerp_color(c, (255, 255, 255), TILE_BEVEL_LIGHT)
            elif x == n - 2 or y == n - 2:
                c = lerp_color(c, (0, 0, 0), TILE_BEVEL_DARK)
            img[y][x] = list(c) + [255]
    for x, y in ((1, 1), (n - 2, 1), (1, n - 2), (n - 2, n - 2)):
        img[y][x] = list(INK) + [255]
    return img


def dim(ic):
    """Puts everything drawn so far a shade darker, for the thing behind."""
    ic.grid = [[c.lower() if c and c.isupper() else c for c in row] for row in ic.grid]


def bust(ic, ch, cx, top):
    """Someone's head and shoulders, plainly: a round head over a rounded pair of shoulders."""
    ic.disc(ch, cx, top + 3.0, 3.0)
    ic.fill(ch, lambda px, py: ((px - cx) / 6.0) ** 2 + ((py - top - 12.0) / 5.0) ** 2 <= 1.0 and py <= top + 13.5)


def heart(ic, ch, cx, cy, r):
    """A heart, `r` across each lobe."""
    ic.disc(ch, cx - r * 0.78, cy - r * 0.2, r)
    ic.disc(ch, cx + r * 0.78, cy - r * 0.2, r)
    ic.poly(ch, [(cx - r * 1.72, cy + r * 0.1), (cx + r * 1.72, cy + r * 0.1), (cx, cy + r * 2.1)])


def flame(ic, ch, cx, top, bottom, width):
    """A flame standing up: a teardrop with its point at the top."""
    h = bottom - top
    ic.poly(ch, [(cx, top), (cx + width * 0.45, top + h * 0.3), (cx + width * 0.5, top + h * 0.62),
                 (cx + width * 0.4, top + h * 0.88), (cx, bottom), (cx - width * 0.4, top + h * 0.88),
                 (cx - width * 0.5, top + h * 0.62), (cx - width * 0.45, top + h * 0.3)])


def arc(ic, ch, cx, cy, r, start, end, thick, taper=0.0):
    """A stroke round part of a circle, from `start` to `end` degrees (0 is right, 90 up), thinner
    toward its ends by `taper`."""
    steps = max(4, int(abs(end - start) / 8))
    pts = []
    for i in range(steps + 1):
        t = math.radians(start + (end - start) * i / float(steps))
        pts.append((cx + r * math.cos(t), cy - r * math.sin(t)))
    for i, ((x0, y0), (x1, y1)) in enumerate(zip(pts, pts[1:])):
        u = abs((i + 0.5) / steps - 0.5) * 2
        ic.capsule(ch, x0, y0, x1, y1, thick * (1 - taper * u))
    return pts[-1]


def mask_badge(ic):
    """A rogue's domino mask in the top left corner, for the Cunning Action halves."""
    ic.stamp([".III...III.",
              "IIoII.IIoII",
              ".IIIIIIIII.",
              "..II...II.."], 2, 2)


# --------------------------------------------------------------------------- #
# Ability icons
# --------------------------------------------------------------------------- #
#
# Each is the ability's own thing where it has one (a sword, a potion, a fire bolt, a lyre) and a
# sign for it where it hasn't (a U-turn for Disengage, afterimages for Dodge, a paw for Pack
# Tactics). Cunning Action's halves are Dash and Disengage with a rogue's mask in the corner.

def attack(ic):
    ic.fill("P", lambda px, py: 9.4 <= math.hypot(px - 16.5, py - 15.5) <= 10.8 + 0.25 * (px - 6)
            and px < 14.5 and py < 13.0 and px + py > 13.0)
    blade(ic, -3, 13)
    ic.diag("+", 8, 11, -1, -1)
    ic.diag("G", -5, -4, -4, 4)
    wrap(ic, -10, -6)
    pommel(ic, -12)


def off_hand_attack(ic):
    blade(ic, -3, 13)
    ic.diag("G", -5, -4, -4, 4)
    wrap(ic, -10, -6)
    pommel(ic, -12)
    dim(ic)
    ic.flip()
    dagger(ic)


def dash(ic):
    boot(ic, 4, 7, "L", "D", "D")
    ic.stamp(["....PPP",
              "..PPPP.",
              ".PPPP..",
              "PPPp...",
              ".p....."], 12, 6)
    for y, x0, x1 in ((14, 16, 20), (16, 17, 21), (18, 16, 19)):
        for x in range(x0, x1 + 1):
            ic.glow(x, y, (220, 232, 255), 200 - (x - x0) * 40)


def disengage(ic):
    end = arc(ic, "P", 12.0, 12.5, 6.0, -35, 190, 1.2)
    ic.poly("P", [(end[0] - 3.4, end[1] - 0.6), (end[0] + 3.4, end[1] - 0.6), (end[0] + 0.4, end[1] + 4.6)])


def dodge(ic):
    bust(ic, "S", 14.0, 4.0)


def dodge_ghosts(img):
    """Dodge's afterimages and the streaks it leaves, laid on the tile under the figure."""
    ghost = Icon()
    bust(ghost, "S", 14.0, 4.0)
    for shift, alpha in ((-4, 150), (-8, 80)):
        for y in range(ICON):
            for x in range(ICON):
                if ghost.grid[y][x] and 1 < x + shift < ICON - 2:
                    put(img, x + shift, y, (170, 200, 246, alpha))
    for y, x0, x1 in ((7, 2, 6), (12, 2, 5), (17, 2, 4)):
        for x in range(x0, x1 + 1):
            put(img, x, y, (230, 240, 255, 90 + 30 * (x - x0)))


#: A small open hand, fingers up and the thumb out to the left, over its sleeve ("Z").
HAND = (".H.H.H..",
        ".H.H.H.H",
        ".H.H.H.H",
        ".HHHHHHH",
        "HHHHHHHH",
        ".HHHHHHH",
        "..HHHHH.",
        "..ZZZZZ.",
        "..ZZZZZ.")


def hand(ic, ox, oy, sleeve, down=False):
    """Lays down a small hand, turned fingers down if `down`."""
    rows = HAND[::-1] if down else HAND
    ic.stamp([row.replace("Z", sleeve) for row in rows], ox, oy)


def help_(ic):
    hand(ic, 12, 2, "B", down=True)
    hand(ic, 4, 13, "M")
    ic.glow(11, 11, GLINT, 255)
    for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
        ic.glow(11 + dx, 11 + dy, (255, 232, 160), 210)


def drink_potion(ic):
    healing_potion(ic)
    for x, y, a in ((16, 3, 200), (18, 1, 140), (15, 1, 110)):
        ic.glow(x, y, (255, 214, 214), a)


def throw_alchemists_fire(ic):
    ic.disc("O", 15.5, 12.5, 3.8)
    ic.fill("E", lambda px, py: (px - 15.5) ** 2 + (py - 12.5) ** 2 <= 3.8 ** 2 and py < 11.0)
    ic.rect("E", 15, 6, 16, 8)
    ic.rect("C", 14, 5, 17, 5)
    flame(ic, "O", 15.5, 0.5, 5.0, 4.0)
    ic.set(15, 3, "+")
    ic.set(14, 11, "+")
    for (x, y, a) in ((10, 15, 230), (8, 17, 190), (6, 18, 150), (4, 19, 110), (9, 16, 120), (7, 18, 90)):
        ic.glow(x, y, FIRE[3], a)


def soul_link_strike(ic):
    ic.ring("G", 12.0, 12.0, 8.0, 9.6)
    for _ in range(2):
        blade(ic, -3, 13, ch="U")
        ic.diag("G", -5, -4, -3, 3)
        wrap(ic, -10, -6, ch="B")
        pommel(ic, -12)
        ic.flip()


def second_wind(ic):
    arc(ic, "Q", 12.0, 12.0, 8.0, 100, 250, 1.3, taper=0.8)
    arc(ic, "Q", 12.0, 12.0, 8.0, -80, 70, 1.3, taper=0.8)
    arc(ic, "P", 12.0, 12.0, 5.4, 150, 230, 0.8, taper=0.6)
    arc(ic, "P", 12.0, 12.0, 5.4, -30, 50, 0.8, taper=0.6)
    heart(ic, "R", 12.0, 10.6, 2.4)
    ic.set(10, 10, "+")


def defence(ic):
    plate_armour(ic)


def sneak_attack(ic):
    dagger(ic)
    ic.flip(across=True, down=True)
    ic.fill("P", lambda px, py: math.hypot(px - 7.0, py - 7.0) <= 4.2 and math.hypot(px - 9.2, py - 5.6) > 3.6)
    for x, y, a in ((4, 20, 220), (3, 19, 120), (5, 21, 120)):
        ic.glow(x, y, RED[3], a)


def cunning_dash(ic):
    dash(ic)
    mask_badge(ic)


def cunning_disengage(ic):
    disengage(ic)
    mask_badge(ic)


def sacred_flame(ic):
    ic.stamp(["......U......",
              "......UU.....",
              ".....UUU.....",
              "..U..UUUU....",
              "..UU.UUUU..U.",
              "..UUUUUUUU.U.",
              "..UUUUUUUUUU.",
              ".UUUUUUPUUUU.",
              ".UUUUUPPPUUUU",
              ".UUUUPPPPPUUU",
              ".UUUUPPPPPUUU",
              ".UUUUPP+PPUUU",
              "..UUUUPPPUUU.",
              "...UUUUUUUU..",
              "....UUUUUU..."], 5, 6)


def sacred_flame_light(img):
    """The column of light Sacred Flame falls through, laid on the tile under the flame."""
    for y in range(1, 14):
        for x in range(8, 16):
            a = int(200 * (1 - abs(x - 11.5) / 4.0) * (1 - y / 16.0))
            if a > 0:
                put(img, x, y, (255, 246, 200, a))


def healing_word(ic):
    ic.ellipse("P", 12.0, 10.0, 8.6, 6.4)
    ic.poly("P", [(6.0, 13.5), (10.5, 15.5), (4.0, 20.5)])
    heart(ic, "R", 12.0, 8.6, 2.2)
    ic.set(10, 8, "+")


def cure_wounds(ic):
    heart(ic, "R", 12.0, 9.4, 4.4)
    ic.rect("P", 11, 8, 12, 15)
    ic.rect("P", 8, 10, 15, 11)
    ic.set(7, 6, "+")


def fire_bolt(ic):
    ic.poly("O", [(3.0, 21.0), (8.5, 12.5), (12.0, 8.5), (15.0, 5.0), (19.5, 5.5), (19.5, 9.5), (15.5, 13.0), (11.0, 16.0)])
    ic.disc("O", 16.0, 8.5, 4.2)
    ic.disc("^", 16.0, 8.5, 2.3)
    ic.disc("+", 15.5, 8.0, 1.2)
    ic.line("^", 6, 17, 11, 12)


def ray_of_frost(ic):
    ic.fill("Q", lambda px, py: abs(px - py) <= 1.6 and 2.0 < px < 14.0)
    ic.fill("+", lambda px, py: abs(px - py) <= 0.5 and 3.0 < px < 13.0)
    cx = cy = 15.5
    for angle in range(0, 360, 45):
        t = math.radians(angle)
        ex, ey = cx + 5.4 * math.cos(t), cy + 5.4 * math.sin(t)
        ic.capsule("Q", cx, cy, ex, ey, 0.75)
    ic.disc("P", cx, cy, 1.6)


def magic_missile(ic):
    ahead, across = (0.7071, -0.7071), (0.7071, 0.7071)

    def at(c, f, s):
        return (c[0] + ahead[0] * f + across[0] * s, c[1] + ahead[1] * f + across[1] * s)

    centres = [at((11.0, 12.0), f, s) for f, s in ((-0.5, -6.5), (1.5, 0.0), (-0.5, 6.5))]
    for c in centres:
        ic.poly("V", [at(c, 4.0, 0.0), at(c, -1.6, 2.8), at(c, -0.4, 0.0), at(c, -1.6, -2.8)])
    for c in centres:
        tip = at(c, 2.4, 0.0)
        ic.set(int(tip[0]), int(tip[1]), "+")
        for i in range(1, 5):
            tx, ty = at(c, -0.4 - i * 1.0, 0.0)
            ic.glow(int(tx), int(ty), VIOLET[3], 220 - 45 * i)


def lay_on_hands(ic):
    glove(ic, "H", "H", "B")
    for x, y in ((3, 4), (21, 4), (2, 11), (22, 11), (12, 0), (5, 1), (19, 1)):
        ic.glow(x, y, (255, 236, 160), 200)


def protection(ic):
    bust(ic, "B", 8.0, 4.0)
    ic.poly("I", [(10.0, 6.0), (21.0, 6.0), (21.0, 12.5), (19.4, 16.6), (15.5, 20.6), (11.6, 16.6), (10.0, 12.5)])
    ic.poly("S", [(11.6, 7.6), (19.4, 7.6), (19.4, 12.4), (18.0, 15.6), (15.5, 18.4), (13.0, 15.6), (11.6, 12.4)])
    ic.line("G", 15, 8, 15, 17)
    ic.line("G", 12, 11, 18, 11)
    ic.set(12, 8, "+")


def produce_flame(ic):
    ic.fill("H", lambda px, py: ((px - 12.0) / 8.0) ** 2 + ((py - 15.0) / 5.0) ** 2 <= 1.0 and py >= 15.5)
    ic.capsule("H", 5.0, 16.0, 4.6, 12.8, 1.4)
    ic.capsule("H", 19.0, 16.0, 19.4, 12.8, 1.4)
    flame(ic, "O", 12.0, 2.0, 16.0, 9.0)
    flame(ic, "^", 12.0, 7.0, 15.5, 4.4)
    ic.set(12, 12, "+")


def entangle(ic):
    for phase, x0 in ((0.0, 8.0), (2.6, 15.0)):
        pts = [(x0 + 3.2 * math.sin(y * 0.55 + phase), y) for y in range(20, 3, -1)]
        for (xa, ya), (xb, yb) in zip(pts, pts[1:]):
            ic.capsule("N", xa, ya + 0.5, xb, yb + 0.5, 1.0)
    ic.stamp(["NN.", ".NN"], 3, 9)
    ic.stamp([".NN", "NN."], 18, 12)
    ic.stamp(["NN", ".N"], 5, 15)
    ic.rect("L", 3, 20, 20, 20)
    for x, y in ((6, 6), (16, 9), (9, 13), (14, 16)):
        ic.set(x, y, "W")


def eldritch_blast(ic):
    for off in (-1.2, 1.2):
        ic.capsule("M", 3.0 + off, 20.0 + off, 12.0, 11.0, 0.6, 1.1)
    ic.disc("V", 14.5, 9.5, 4.8)
    ic.disc("M", 14.5, 9.5, 3.0)
    ic.disc("+", 14.0, 9.0, 1.3)
    for x, y, a in ((20, 4, 220), (21, 6, 140), (8, 5, 200), (9, 4, 140), (20, 15, 200), (19, 16, 140)):
        ic.glow(x, y, (244, 170, 255), a)


def hex_(ic):
    ic.fill("P", lambda px, py: abs(py - 11.5) <= 5.2 * (1 - ((px - 12.0) / 9.5) ** 2))
    ic.disc("V", 12.0, 11.5, 3.4)
    ic.rect("o", 11, 10, 12, 12)
    ic.set(10, 10, "+")
    ic.stamp(["V", "V", "v"], 9, 17)
    ic.stamp(["V", "v"], 14, 17)
    for x, y in ((12, 2), (4, 6), (20, 6), (4, 17), (20, 17), (12, 21)):
        ic.glow(x, y, (214, 160, 255), 220)


def vicious_mockery(ic):
    ic.fill("P", lambda px, py: ((px - 12.0) / 8.4) ** 2 + ((py - 11.0) / 8.0) ** 2 <= 1.0)
    ic.stamp(["oo.....oo",
              ".oo...oo."], 7, 7)
    ic.stamp(["o.........o",
              ".oo.....oo.",
              "..ooooooo.."], 6, 13)
    ic.stamp(["PP", "PP"], 9, 14)
    ic.stamp(["PP", "PP"], 13, 14)
    for x, y, a in ((2, 4, 220), (3, 5, 160), (21, 4, 220), (20, 5, 160), (2, 18, 160), (21, 18, 160)):
        ic.glow(x, y, ROSE[4], a)


def bardic_inspiration(ic):
    arc(ic, "G", 8.5, 9.0, 4.2, 100, 270, 1.1)
    arc(ic, "G", 15.5, 9.0, 4.2, -90, 80, 1.1)
    ic.rect("G", 8, 4, 15, 5)
    ic.ellipse("W", 12.0, 17.0, 5.0, 3.2)
    for x in (10, 12, 14):
        ic.line("P", x, 6, x, 15)
    ic.stamp(["..PP",
              "..PP",
              "..P.",
              "PPP.",
              "PP.."], 18, 1)


def multiattack(ic):
    for off in (-5, 0, 5):
        ic.capsule("P", 17.0 + off, 3.0, 12.0 + off, 12.0, 0.4, 1.4)
        ic.capsule("P", 12.0 + off, 12.0, 7.0 + off, 20.0, 1.4, 0.4)
    ic.pattern("P", "R", lambda x, y: (x + y) % 7 == 0)


def pack_tactics(ic):
    ic.ellipse("F", 12.0, 15.5, 4.8, 4.0)
    for cx, cy in ((5.8, 10.0), (9.6, 6.4), (14.4, 6.4), (18.2, 10.0)):
        ic.disc("F", cx, cy, 2.0)


def trip(ic):
    ic.rect("P", 10, 2, 13, 10)
    ic.poly("P", [(5.5, 10.5), (18.5, 10.5), (12.0, 17.0)])
    ic.rect("L", 3, 19, 20, 20)
    for x, y in ((5, 17), (4, 16), (18, 17), (19, 16)):
        ic.glow(x, y, (230, 210, 190), 180)


def smoke_bomb(ic):
    for cx, cy, r in ((6.0, 9.0, 3.4), (9.5, 5.5, 3.2), (5.0, 14.5, 2.8), (13.0, 4.0, 2.4)):
        ic.disc("Y", cx, cy, r)
    ic.disc("I", 13.0, 14.5, 5.6)
    ic.set(10, 11, "+")
    ic.set(11, 11, "^")
    ic.line("L", 16, 9, 18, 6)
    for x, y, a in ((19, 5, 255), (20, 4, 180), (18, 4, 140), (20, 6, 140)):
        ic.glow(x, y, FIRE[4], a)


def blasting_charge(ic):
    ic.fill("W", lambda px, py: 6.5 < px < 17.5 and 9.0 < py < 20.5 and not ((px < 7.5 or px > 16.5) and (py < 10 or py > 19.5)))
    ic.rect("I", 7, 11, 16, 11)
    ic.rect("I", 7, 17, 16, 17)
    ic.line("L", 12, 8, 14, 5)
    ic.stamp([".O.",
              "O+O",
              ".O."], 14, 2)
    for x, y, a in ((18, 2, 200), (12, 1, 160), (18, 6, 140)):
        ic.glow(x, y, FIRE[4], a)


def thorn_burst(ic):
    for i in range(8):
        t = math.radians(i * 45 + 22.5)
        reach = 10.0 if i % 2 == 0 else 7.5
        ic.capsule("N", 12.0, 12.0, 12.0 + reach * math.cos(t), 12.0 + reach * math.sin(t), 2.0, 0.3)
    ic.disc("R", 12.0, 12.0, 3.0)
    ic.set(11, 11, "+")


def drink_elixir(ic):
    elixir_of_focus(ic)
    for x, y, a in ((17, 2, 220), (19, 4, 150), (6, 3, 150)):
        ic.glow(x, y, VIOLET[4], a)


def end_turn(ic):
    ic.poly("E", [(7.0, 5.0), (17.0, 5.0), (17.0, 6.5), (13.3, 11.5), (13.3, 12.5), (17.0, 17.5), (17.0, 19.0),
                  (7.0, 19.0), (7.0, 17.5), (10.7, 12.5), (10.7, 11.5), (7.0, 6.5)])
    ic.fill("G", lambda px, py: 8.0 < py < 11.0 and abs(px - 12.0) < (11.0 - py) * 1.1 + 0.6)
    ic.fill("G", lambda px, py: 15.0 < py < 19.0 and abs(px - 12.0) < (py - 14.5) * 1.3)
    ic.line("G", 12, 12, 12, 15)
    ic.rect("W", 5, 3, 18, 4)
    ic.rect("W", 5, 19, 18, 20)
    ic.set(8, 6, "+")


def ranged_attack(ic):
    shortbow(ic)
    ic.line("W", 7, 7, 17, 17)
    ic.stamp(["SS.",
              "SSS",
              ".SS"], 4, 4)
    ic.stamp(["R..",
              ".RR",
              ".RR"], 17, 17)


# --------------------------------------------------------------------------- #
# abilities.png
# --------------------------------------------------------------------------- #

#: Tiles to a row on the sheet.
COLUMNS = 16

#: Every ability's icon: its name, the kind of tile it sits on, how it's drawn, and anything laid
#: on the tile under it. In AbilityId's order, which has to match (the names are its names with a
#: hyphen between the words): an ability's icon is the cell numbered (int)its id.
ABILITIES = (
    ("attack", "weapon", attack, None),
    ("off-hand-attack", "weapon", off_hand_attack, None),
    ("dash", "weapon", dash, None),
    ("disengage", "weapon", disengage, None),
    ("dodge", "weapon", dodge, dodge_ghosts),
    ("help", "radiant", help_, None),
    ("drink-potion", "heal", drink_potion, None),
    ("throw-alchemists-fire", "fire", throw_alchemists_fire, None),
    ("soul-link-strike", "soul", soul_link_strike, None),
    ("second-wind", "heal", second_wind, None),
    ("defence", "weapon", defence, None),
    ("sneak-attack", "shadow", sneak_attack, None),
    ("cunning-dash", "shadow", cunning_dash, None),
    ("cunning-disengage", "shadow", cunning_disengage, None),
    ("sacred-flame", "radiant", sacred_flame, sacred_flame_light),
    ("healing-word", "heal", healing_word, None),
    ("cure-wounds", "heal", cure_wounds, None),
    ("fire-bolt", "fire", fire_bolt, None),
    ("ray-of-frost", "frost", ray_of_frost, None),
    ("magic-missile", "arcane", magic_missile, None),
    ("lay-on-hands", "radiant", lay_on_hands, None),
    ("protection", "weapon", protection, None),
    ("produce-flame", "fire", produce_flame, None),
    ("entangle", "nature", entangle, None),
    ("eldritch-blast", "eldritch", eldritch_blast, None),
    ("hex", "eldritch", hex_, None),
    ("vicious-mockery", "song", vicious_mockery, None),
    ("bardic-inspiration", "song", bardic_inspiration, None),
    ("multiattack", "foe", multiattack, None),
    ("pack-tactics", "foe", pack_tactics, None),
    ("trip", "foe", trip, None),
    ("smoke-bomb", "foe", smoke_bomb, None),
    ("blasting-charge", "foe", blasting_charge, None),
    ("thorn-burst", "foe", thorn_burst, None),
    ("drink-elixir", "arcane", drink_elixir, None),
)

#: The icons that aren't abilities, from the first cell of the fourth row: the End Turn button,
#: and Attack for someone with a bow or a crossbow in hand.
EXTRAS_FROM = COLUMNS * 3
EXTRAS = (
    ("end-turn", "weapon", end_turn, None),
    ("ranged-attack", "weapon", ranged_attack, None),
)


def ability_icon(school, draw, under):
    """One ability's tile with its icon on it."""
    img = tile(school)
    if under:
        under(img)
    ic = Icon(remap=EXTRA)
    draw(ic)
    rendered = ic.render()
    for y in range(ICON):
        for x in range(ICON):
            if rendered[y][x][3]:
                put(img, x, y, tuple(rendered[y][x]))
    return img


def build_abilities():
    count = EXTRAS_FROM + len(EXTRAS)
    rows = (count + COLUMNS - 1) // COLUMNS
    img = new_image(COLUMNS * ICON, rows * ICON)
    cells = list(enumerate(ABILITIES)) + [(EXTRAS_FROM + i, e) for i, e in enumerate(EXTRAS)]
    for i, (_, school, draw, under) in cells:
        paste(img, (i % COLUMNS) * ICON, (i // COLUMNS) * ICON, ability_icon(school, draw, under))
    write_png(os.path.join(OUT, "Combat", "abilities.png"), img)


# --------------------------------------------------------------------------- #
# conditions.png
# --------------------------------------------------------------------------- #
#
# 12x12, small enough to line up under a name on the party strip and the initiative bar, so each is
# one plain shape in one colour, like the dialogue icons.

CONDITION = 12


def downed(ic):
    heart(ic, "R", 6.0, 5.0, 2.4)
    ic.stamp(["o.", ".o", "o.", ".o", "o."], 5, 3)


def prone(ic):
    ic.disc("P", 2.8, 6.5, 1.8)
    ic.capsule("B", 5.0, 6.5, 9.6, 6.5, 1.6)
    ic.rect("L", 1, 9, 10, 9)


def slowed(ic):
    ic.stamp([".....LLL....",
              "....LLLLL...",
              "...LLlllLL..",
              "...LlLLLlL..",
              "...LlLlLlL..",
              "...LLlLLL...",
              ".P..LLLL....",
              ".PPPPPPPPP..",
              "..PPPPPPPPP."], 0, 1)


def disengaged(ic):
    end = arc(ic, "P", 6.5, 6.5, 3.2, -40, 190, 0.8)
    ic.poly("P", [(end[0] - 2.2, end[1] - 0.4), (end[0] + 2.2, end[1] - 0.4), (end[0] + 0.2, end[1] + 2.8)])


def dodging(ic):
    ic.poly("Q", [(10.5, 1.2), (8.6, 6.8), (4.6, 8.8), (6.2, 4.0)])
    ic.line("P", 2, 10, 6, 6)


def restrained(ic):
    ic.ring("S", 4.2, 7.6, 1.3, 2.9)
    ic.ring("S", 7.8, 4.4, 1.3, 2.9)
    ic.fill("s", lambda px, py: 1.3 <= math.hypot(px - 4.2, py - 7.6) <= 2.9 and px > 5.0 and py < 7.0)


def hexed(ic):
    ic.fill("P", lambda px, py: abs(py - 6.0) <= 3.4 * (1 - ((px - 6.0) / 5.4) ** 2))
    ic.disc("V", 6.0, 6.0, 2.2)
    ic.set(6, 6, "o")
    ic.set(5, 5, "+")


def inspired(ic):
    ic.disc("G", 4.0, 8.4, 2.1)
    ic.rect("G", 5, 2, 5, 8)
    ic.capsule("G", 5.5, 2.5, 9.0, 5.0, 0.8)
    ic.glow(10, 1, GLINT, 230)
    ic.glow(10, 2, (255, 232, 160), 160)
    ic.glow(9, 1, (255, 232, 160), 160)


def stable(ic):
    heart(ic, "N", 6.0, 5.0, 2.4)
    ic.rect("P", 5, 3, 6, 8)
    ic.rect("P", 3, 5, 8, 6)


def chevrons(ic, ch, up):
    """Two chevrons, pointing up for an advantage, down for a disadvantage."""
    for oy in (1.0, 5.0):
        pts = [(6.0, oy), (10.5, oy + 4.5), (8.3, oy + 4.5), (6.0, oy + 2.2), (3.7, oy + 4.5), (1.5, oy + 4.5)]
        if not up:
            pts = [(x, 11.0 - y) for x, y in pts]
        ic.poly(ch, pts)


def helped(ic):
    chevrons(ic, "G", True)


def mocked(ic):
    chevrons(ic, "M", False)


#: Every condition's icon, in ConditionId's order, which has to match.
CONDITIONS = (
    ("downed", downed),
    ("prone", prone),
    ("slowed", slowed),
    ("disengaged", disengaged),
    ("dodging", dodging),
    ("restrained", restrained),
    ("hexed", hexed),
    ("inspired", inspired),
    ("stable", stable),
    ("helped", helped),
    ("mocked", mocked),
)


def build_conditions():
    img = new_image(len(CONDITIONS) * CONDITION, CONDITION)
    for i, (_, draw) in enumerate(CONDITIONS):
        ic = Icon(size=CONDITION, remap=EXTRA)
        draw(ic)
        paste(img, i * CONDITION, 0, ic.render())
    write_png(os.path.join(OUT, "Combat", "conditions.png"), img)


# --------------------------------------------------------------------------- #
# hud.png
# --------------------------------------------------------------------------- #

#: Every piece of hud.png: its name, where its first frame is, its size, and how many frames it
#: has, side by side to the right. CombatIcons.cs has the same table, and this script checks it.
HUD = (
    ("hotbar-normal", 0, 0, 30, 30, 1),
    ("hotbar-hover", 30, 0, 30, 30, 1),
    ("hotbar-disabled", 60, 0, 30, 30, 1),
    ("hotbar-selected", 90, 0, 30, 30, 1),
    ("portrait-party", 120, 0, 30, 32, 1),
    ("portrait-foe", 150, 0, 30, 32, 1),
    ("portrait-current", 180, 0, 30, 32, 1),
    ("death-save-empty", 210, 0, 10, 10, 1),
    ("death-save-success", 220, 0, 10, 10, 1),
    ("death-save-failure", 230, 0, 10, 10, 1),
    ("path-dot", 210, 10, 6, 6, 1),
    ("path-dot-far", 216, 10, 6, 6, 1),
    ("soul-link-gold", 0, 32, 24, 24, 1),
    ("soul-link-flicker", 24, 32, 24, 24, 1),
    ("soul-link-violet", 48, 32, 24, 24, 1),
    ("soul-link-broken", 72, 32, 24, 24, 1),
    ("soul-link-pulse", 96, 32, 24, 24, 1),
    ("target-foe", 120, 32, 24, 12, 4),
    ("target-ally", 120, 44, 24, 12, 4),
    ("path-end", 216, 32, 16, 16, 1),
    ("path-end-far", 232, 32, 16, 16, 1),
    ("arrow", 0, 56, 16, 16, 1),
    ("fire-bolt", 16, 56, 16, 16, 4),
    ("frost-ray", 80, 56, 16, 16, 4),
    ("radiant-mote", 144, 56, 16, 16, 4),
    ("eldritch-blast", 0, 72, 16, 16, 4),
    ("magic-missile", 64, 72, 16, 16, 4),
    ("flask", 128, 72, 16, 16, 4),
    ("action", 0, 88, 12, 12, 1),
    ("action-spent", 12, 88, 12, 12, 1),
    ("action-preview", 24, 88, 12, 12, 1),
    ("bonus", 36, 88, 12, 12, 1),
    ("bonus-spent", 48, 88, 12, 12, 1),
    ("bonus-preview", 60, 88, 12, 12, 1),
    ("reaction", 72, 88, 12, 12, 1),
    ("reaction-spent", 84, 88, 12, 12, 1),
    ("reaction-preview", 96, 88, 12, 12, 1),
    ("move-track", 108, 88, 8, 8, 1),
    ("move-fill", 116, 88, 8, 8, 1),
    ("move-preview", 124, 88, 8, 8, 1),
    ("move-over", 132, 88, 8, 8, 1),
    ("move-tick", 140, 88, 2, 8, 1),
)

#: hud.png's size, in pixels.
HUD_WIDTH = 256
HUD_HEIGHT = 100


# Frames: the hotbar's slots and the initiative portraits ------------------- #
#
# Like the buttons: a black edge, a ring of bevel lit from the top left, a ring of trim inside it,
# and a well in the middle the icon or the face sits in (24x24 or 24x26, three pixels in). Each
# state's colours: bevel lit and dark, trim lit and dark, the well's top and bottom.

#: How far in from a frame's edge its well is, and the black of the edge itself (the buttons').
FRAME_INSET = 3
FRAME_EDGE = (0, 0, 0)

#: Each frame's colours, by the name of its piece.
FRAMES = {
    "hotbar-normal": ((86, 104, 164), (18, 22, 46), (206, 162, 82), (144, 108, 56), (20, 24, 50), (30, 36, 70)),
    "hotbar-hover": ((116, 136, 200), (28, 34, 68), (250, 216, 139), (206, 150, 80), (26, 32, 64), (40, 48, 90)),
    "hotbar-disabled": ((74, 76, 90), (20, 20, 28), (112, 108, 104), (72, 70, 70), (14, 14, 22), (20, 20, 30)),
    "hotbar-selected": ((250, 216, 139), (167, 101, 58), (255, 248, 222), (238, 184, 88), (34, 40, 80), (50, 58, 104)),
    "portrait-party": ((86, 104, 164), (18, 22, 46), (206, 162, 82), (144, 108, 56), (40, 48, 92), (20, 24, 50)),
    "portrait-foe": ((176, 64, 76), (54, 14, 26), (220, 96, 92), (140, 44, 54), (64, 26, 42), (30, 12, 24)),
    "portrait-current": ((250, 216, 139), (167, 101, 58), (255, 248, 222), (238, 184, 88), (52, 62, 112), (24, 28, 58)),
}


def frame(img, ox, oy, w, h, colours, glints):
    """One frame: the edge with its corners cut, the bevel, the trim and the well."""
    bevel_lit, bevel_dark, trim_lit, trim_dark, well_top, well_bottom = colours
    for y in range(h):
        for x in range(w):
            ring = min(x, y, w - 1 - x, h - 1 - y)
            if ring == 0:
                if (x in (0, w - 1)) and (y in (0, h - 1)):
                    continue
                c = FRAME_EDGE
            elif ring == 1:
                c = bevel_lit if (x == 1 or y == 1) and not (x == w - 2 or y == h - 2) else bevel_dark
            elif ring == 2:
                c = trim_lit if (x == 2 or y == 2) and not (x == w - 3 or y == h - 3) else trim_dark
            else:
                c = lerp_color(well_top, well_bottom, (y - FRAME_INSET) / float(h - 2 * FRAME_INSET - 1))
            img[oy + y][ox + x] = list(c) + [255]
    if glints:
        for x, y in ((2, 2), (w - 3, 2), (2, h - 3), (w - 3, h - 3)):
            img[oy + y][ox + x] = list(GLINT) + [255]


# Pips: the action, the bonus action and the reaction ---------------------- #

#: A pip's size, in art pixels.
PIP = 12

#: Each pip's shape, and its ramp letter: a gold circle for the action, a teal triangle for the
#: bonus action, a violet diamond for the reaction.
PIPS = (
    ("action", "G", lambda px, py: math.hypot(px - 6.0, py - 6.0) <= 4.6),
    ("bonus", "Q", lambda px, py: inside([(6.0, 1.2), (11.0, 10.4), (1.0, 10.4)], px, py)),
    ("reaction", "V", lambda px, py: inside([(6.0, 0.8), (11.2, 6.0), (6.0, 11.2), (0.8, 6.0)], px, py)),
)

#: A spent pip's hollow, and how far a preview pip is lifted toward white.
PIP_SPENT = (22, 26, 54)
PIP_PREVIEW = 0.55


def pip(img, ox, oy, shape, ch, state):
    """One pip: full (shaded in its colour), spent (a dark hollow with a dim rim), or preview (the
    pale ghost of it, for what the move you're pointing at would spend)."""
    ic = Icon(size=PIP, remap=EXTRA)
    ic.fill(ch, shape)
    rendered = ic.render()
    ramp = ic.materials[ch]
    for y in range(PIP):
        for x in range(PIP):
            r, g, b, a = rendered[y][x]
            if not a:
                continue
            if state != "full" and (r, g, b) != INK:
                rim = any(not shape(x + dx + 0.5, y + dy + 0.5) for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)))
                if state == "spent":
                    c = ramp[1] if rim else PIP_SPENT
                else:
                    c = ramp[3] if rim else lerp_color(ramp[3], (255, 255, 255), PIP_PREVIEW)
                r, g, b = c
            img[oy + y][ox + x] = [r, g, b, 255]


# The movement bar ---------------------------------------------------------- #
#
# Three-sliced: a cap three pixels wide each end and two in the middle that stretch, the middle the
# same down every column. The track goes down first at the bar's full length, then the fill as far
# as there's movement left, the preview over the part the path you're pointing at would use, and the
# over piece instead when that's more than there is. A tick marks every tile.

#: A movement piece's size, and the track's, the fill's, the preview's and the over piece's
#: colours, top to bottom.
MOVE = 8
MOVE_TRACK = ((8, 10, 24), (14, 16, 36), (34, 40, 76))
MOVE_FILL = ((253, 240, 204), (246, 204, 114), (226, 162, 76), (176, 108, 58))
MOVE_PREVIEW = ((255, 255, 246), (255, 248, 222), (250, 232, 184), (220, 196, 140))
MOVE_OVER = ((255, 200, 190), (240, 114, 108), (206, 58, 66), (148, 34, 54))


def move_piece(img, ox, oy, ramp=None):
    """The track (ramp None) or one of the fills, which sit inside the track's edge."""
    for y in range(MOVE):
        for x in range(MOVE):
            corner = x in (0, MOVE - 1) and y in (0, MOVE - 1)
            edge = x in (0, MOVE - 1) or y in (0, MOVE - 1)
            if corner:
                continue
            if ramp is None:
                if edge:
                    c = INK
                else:
                    c = MOVE_TRACK[0] if y == 1 else MOVE_TRACK[2] if y == MOVE - 2 else MOVE_TRACK[1]
            else:
                if edge or (x in (1, MOVE - 2) and y in (1, MOVE - 2)):
                    continue
                c = ramp[0] if y == 1 else ramp[1] if y <= 3 else ramp[2] if y <= 5 else ramp[3]
            img[oy + y][ox + x] = list(c) + [255]


def move_tick(img, ox, oy):
    """A notch across the bar: a dark line with a glint of the fill beside it."""
    for y in range(1, MOVE - 1):
        img[oy + y][ox] = list(MOVE_TRACK[0]) + [255]
        img[oy + y][ox + 1] = list(MOVE_FILL[0]) + [140]


# Death saves --------------------------------------------------------------- #

#: A death save's size, in art pixels.
SAVE = 10


def death_save(img, ox, oy, kind):
    """A socket for a death save: empty, a bright gem for a success, a red one crossed through for
    a failure."""
    if kind == "empty":
        for y in range(SAVE):
            for x in range(SAVE):
                d = math.hypot(x + 0.5 - 5.0, y + 0.5 - 5.0)
                if d <= 3.6:
                    c = PIP_SPENT if x + y < 9 else (52, 62, 108)
                    img[oy + y][ox + x] = list(c) + [255]
                elif d <= 4.6:
                    img[oy + y][ox + x] = list(INK) + [255]
        return
    ic = Icon(size=SAVE)
    ic.disc("U" if kind == "success" else "R", 5.0, 5.0, 3.6)
    if kind == "success":
        ic.set(3, 3, "+")
        ic.set(4, 3, "+")
    else:
        ic.stamp(["o..o", ".oo.", ".oo.", "o..o"], 3, 3)
    paste(img, ox, oy, ic.render())


# The Soul-Link sigil ------------------------------------------------------- #
#
# STORY.md 2.6: two interlocking hands in a circle, in five states. Gold while the bond is whole; gold
# with a violet edge, the one frame of flicker that shows it going wrong; violet; grey and broken;
# and grey with gold coming back through the cracks.

#: The sigil's grey, for when the bond is broken.
GREY = ((54, 54, 66), (86, 86, 100), (124, 124, 136), (166, 166, 176), (212, 212, 218))

#: The clasp at the top of the sigil, the front hand's fingers ("a", a shade darker) curled over
#: the back hand ("b"), its thumb over the top.
SIGIL_CLASP = ("...AAA....",
               "..AAAAAb..",
               ".AAAAaAbb.",
               ".AAAaAabbb",
               ".AAaAaAbbb",
               "..AAaAabb.",
               "..AAA.bb..")


def sigil_hands(ic, ch, apart):
    """Two forearms rising from the circle to a clasp at the top, hand in hand, the open space
    between them. Pulled apart, each hand is left reaching for the other."""
    back, front = ch.lower(), ch
    if apart:
        ic.capsule(back, 18.5, 19.5, 15.0, 10.5, 1.5)
        ic.ellipse(back, 15.4, 8.6, 2.2, 2.4)
        ic.capsule(front, 5.5, 19.5, 9.0, 10.5, 1.5)
        ic.ellipse(front, 8.6, 8.6, 2.2, 2.4)
        return
    ic.capsule(back, 18.5, 19.5, 14.2, 11.5, 1.5)
    ic.capsule(front, 5.5, 19.5, 9.8, 11.5, 1.5)
    ic.stamp([row.replace("A", front).replace("a", back).replace("b", back) for row in SIGIL_CLASP], 7, 5)


def sigil(state):
    """The sigil in one of its five states."""
    ic = Icon(remap={"Z": GREY})
    whole = state in ("gold", "flicker", "violet")
    ring = {"gold": "G", "flicker": "G", "violet": "V"}.get(state, "Z")
    ic.ring(ring, 12.0, 12.0, 8.4, 10.2)
    sigil_hands(ic, ring, apart=not whole)
    if not whole:
        for y, x in ((1, 15), (2, 14), (3, 15), (4, 14), (19, 9), (20, 8), (21, 9), (22, 8)):
            ic.set(x, y, "o")
    if state == "flicker":
        for y in range(ICON):
            for x in range(ICON):
                if ic.grid[y][x] == "G" and math.hypot(x + 0.5 - 12.0, y + 0.5 - 12.0) > 9.4:
                    ic.grid[y][x] = "V"
    rendered = ic.render()
    over = []
    if state == "flicker":
        over = [(x, y, (190, 140, 255), 130) for x, y in ((1, 12), (22, 12), (12, 1), (12, 22), (4, 4), (19, 19))]
    if state == "pulse":
        for y in range(ICON):
            for x in range(ICON):
                if ic.grid[y][x] == "o":
                    rendered[y][x] = list(GOLD[3]) + [255]
        for y in range(5, 18):
            for x in (11, 12):
                put(rendered, x, y, GOLD[3] + (110,))
        over = [(x, y, GOLD[4], 150) for x, y in ((1, 12), (22, 12), (12, 1), (12, 22))]
    for x, y, c, a in over:
        put(rendered, x, y, c + (a,))
    return rendered


# Things on the ground ------------------------------------------------------ #

#: A target ring's size and frames, and its colours (dark, mid, bright) under a foe and an ally.
#: The path's dots and ends take the same colours: an ally's in reach, a foe's too far.
RING_WIDTH = 24
RING_HEIGHT = 12
RING_FRAMES = 4
RING_FOE = ((120, 24, 40), (206, 58, 66), (255, 160, 150))
RING_ALLY = ((150, 96, 40), (238, 184, 88), (255, 244, 200))


def target_ring(img, ox, oy, colours):
    """The ring under whoever you're aiming at, four frames of its bright dashes going round."""
    dark, mid, bright = colours
    for f in range(RING_FRAMES):
        for y in range(RING_HEIGHT):
            for x in range(RING_WIDTH):
                u = (x + 0.5 - RING_WIDTH / 2.0) / (RING_WIDTH / 2.0 - 0.5)
                v = (y + 0.5 - RING_HEIGHT / 2.0) / (RING_HEIGHT / 2.0 - 0.5)
                d = u * u + v * v
                if not 0.62 <= d <= 1.0:
                    continue
                angle = math.degrees(math.atan2(v, u)) % 360
                dash = int((angle + f * 90.0 / RING_FRAMES) // 45) % 2 == 0
                c = bright if dash and d > 0.78 else mid if dash else dark
                img[oy + y][ox + f * RING_WIDTH + x] = list(c) + [255]


def path_dot(img, ox, oy, colours):
    """One dot of the path you're pointing along, a dark edge round it."""
    dark, mid, bright = colours
    for y in range(6):
        for x in range(6):
            if x in (0, 5) and y in (0, 5):
                continue
            if x in (0, 5) or y in (0, 5):
                c = INK
            else:
                c = bright if (x, y) == (1, 1) else mid if x + y < 6 else dark
            img[oy + y][ox + x] = list(c) + [255]


def path_end(img, ox, oy, colours, far):
    """Where the path ends: a ring with a dot in it, or crossed through if it's too far, with a
    soft dark edge so it shows on any ground."""
    dark, mid, bright = colours
    for y in range(16):
        for x in range(16):
            dx, dy = x + 0.5 - 8.0, y + 0.5 - 8.0
            d = math.hypot(dx, dy)
            off_cross = abs(abs(dx) - abs(dy))
            mark = off_cross < 0.8 and d < 4.2 if far else d < 1.6
            edge = off_cross < 1.8 and d < 4.6 if far else d < 2.6
            if 4.6 < d < 6.4 or mark:
                img[oy + y][ox + x] = list(bright if x + y < 14 else mid) + [255]
            elif 3.6 < d < 7.4 or edge:
                img[oy + y][ox + x] = list(dark) + [200]


# Things that fly ------------------------------------------------------------ #
#
# 16x16, flying right, their middle the middle of the frame; the game turns them to where they're
# going. Fire flickers, frost glitters, light pulses, the blast crackles and the flask tumbles.

#: A frame of a thing in flight, in art pixels.
SHOT = 16


def arrow(ic, f):
    ic.stamp([".RR.............",
              "..RR.......SS...",
              "WWWWWWWWWWWSSS..",
              "..RR.......SS...",
              ".RR............."], 0, 6)
    ic.set(13, 8, "+")


def fire_shot(ic, f):
    sway = (0, 1, 0, -1)[f]
    ic.poly("O", [(1.0, 8.0 + sway), (6.0, 6.0), (11.0, 4.8), (14.5, 8.0), (11.0, 11.2), (6.0, 10.0)])
    ic.disc("O", 11.0, 8.0, 3.2)
    ic.disc("^", 11.0, 8.0, 1.8)
    ic.set(11, 7, "+")
    for x, y in ((3 + f, 5), (5 - f % 2, 11), (2, 9 - f % 2)):
        ic.glow(x, y, FIRE[3], 170)


def frost_shot(ic, f):
    ic.poly("Q", [(15.0, 8.0), (9.0, 5.6), (2.0, 8.0), (9.0, 10.4)])
    ic.line("+", 4, 8, 13, 8)
    for i, (x, y) in enumerate(((2, 5), (5, 11), (1, 10), (6, 4))):
        if (i + f) % 2 == 0:
            ic.glow(x, y, ICE[4], 230)


def radiant_shot(ic, f):
    halo = (4.2, 4.8, 5.4, 4.8)[f]
    for y in range(SHOT):
        for x in range(SHOT):
            d = math.hypot(x + 0.5 - 8.0, y + 0.5 - 8.0)
            if 3.4 < d <= halo:
                ic.glow(x, y, SUNSTEEL[3], int(160 * (1 - (d - 3.4) / 2.2)))
    ic.disc("U", 8.0, 8.0, 2.8)
    ic.set(7, 7, "+")


def eldritch_shot(ic, f):
    ic.poly("M", [(1.0, 8.0), (6.0, 6.6), (9.0, 8.0), (6.0, 9.4)])
    ic.disc("V", 10.0, 8.0, 3.4)
    ic.disc("M", 10.0, 8.0, 1.8)
    ic.set(9, 7, "+")
    sparks = (((14, 4), (6, 4)), ((15, 11), (7, 12)), ((14, 12), (5, 5)), ((15, 5), (8, 11)))[f]
    for x, y in sparks:
        ic.glow(x, y, (244, 170, 255), 230)


def missile_shot(ic, f):
    ic.poly("V", [(15.0, 8.0), (8.0, 4.4), (10.0, 8.0), (8.0, 11.6)])
    ic.set(13, 8, "+")
    for i in range(1, 6):
        ic.glow(9 - i, 8 + ((i + f) % 3 == 0) - ((i + f) % 3 == 1), VIOLET[3], 220 - 38 * i)


def flask_shot(ic, f):
    ic.disc("O", 8.0, 9.5, 3.4)
    ic.fill("E", lambda px, py: (px - 8.0) ** 2 + (py - 9.5) ** 2 <= 3.4 ** 2 and py < 8.5)
    ic.rect("E", 7, 4, 8, 6)
    ic.rect("C", 7, 3, 8, 3)
    ic.set(7, 1, "O")
    ic.set(8, 2, "O")
    ic.set(7, 2, "+")
    for _ in range(f):
        ic.grid = [[ic.grid[SHOT - 1 - x][y] for x in range(SHOT)] for y in range(SHOT)]


#: Each thing in flight, by the name of its piece, and how a frame of it is drawn.
SHOTS = (
    ("arrow", arrow),
    ("fire-bolt", fire_shot),
    ("frost-ray", frost_shot),
    ("radiant-mote", radiant_shot),
    ("eldritch-blast", eldritch_shot),
    ("magic-missile", missile_shot),
    ("flask", flask_shot),
)


def build_hud():
    img = new_image(HUD_WIDTH, HUD_HEIGHT)
    at = {name: (x, y, w, h, frames) for name, x, y, w, h, frames in HUD}

    for name, colours in FRAMES.items():
        x, y, w, h, _ = at[name]
        frame(img, x, y, w, h, colours, glints=name in ("hotbar-selected", "portrait-current"))

    for name, ch, shape in PIPS:
        for suffix, state in (("", "full"), ("-spent", "spent"), ("-preview", "preview")):
            x, y, _, _, _ = at[name + suffix]
            pip(img, x, y, shape, ch, state)

    for name, ramp in (("move-track", None), ("move-fill", MOVE_FILL), ("move-preview", MOVE_PREVIEW), ("move-over", MOVE_OVER)):
        x, y, _, _, _ = at[name]
        move_piece(img, x, y, ramp)
    move_tick(img, *at["move-tick"][:2])

    for kind in ("empty", "success", "failure"):
        x, y, _, _, _ = at["death-save-" + kind]
        death_save(img, x, y, kind)

    for state in ("gold", "flicker", "violet", "broken", "pulse"):
        x, y, _, _, _ = at["soul-link-" + state]
        paste(img, x, y, sigil(state))

    target_ring(img, *at["target-foe"][:2], RING_FOE)
    target_ring(img, *at["target-ally"][:2], RING_ALLY)
    path_dot(img, *at["path-dot"][:2], RING_ALLY)
    path_dot(img, *at["path-dot-far"][:2], RING_FOE)
    path_end(img, *at["path-end"][:2], RING_ALLY, far=False)
    path_end(img, *at["path-end-far"][:2], RING_FOE, far=True)

    for name, draw in SHOTS:
        x, y, w, h, frames = at[name]
        for f in range(frames):
            ic = Icon(size=SHOT, remap=EXTRA)
            draw(ic, f)
            paste(img, x + f * w, y, ic.render())

    write_png(os.path.join(OUT, "Combat", "hud.png"), img)


# --------------------------------------------------------------------------- #
# Checking the index
# --------------------------------------------------------------------------- #

INDEX = os.path.join(ROOT, "Combat", "CombatIcons.cs")
ABILITY_ENUM = os.path.join(ROOT, "Enums", "AbilityId.cs")
CONDITION_ENUM = os.path.join(ROOT, "Enums", "ConditionId.cs")

#: A row of CombatIcons' hud table: ("name", new Rectangle(x, y, w, h), frames).
PIECE = re.compile(r'\("([a-z0-9-]+)",\s*new Rectangle\((\d+),\s*(\d+),\s*(\d+),\s*(\d+)\),\s*(\d+)\)')


def hyphenated(name):
    """An enum member's name the way the sheets spell it: OffHandAttack is off-hand-attack."""
    return re.sub(r"(?<!^)(?=[A-Z])", "-", name).lower()


def enum_names(path):
    """The members of the enum in a file, in order."""
    with open(path, encoding="utf-8") as f:
        text = f.read()
    body = text[text.index("{", text.index(" enum ")) + 1:text.rindex("}")]
    body = re.sub(r"//[^\n]*", "", body)
    return [m.split("=")[0].strip() for m in body.split(",") if m.strip()]


def quoted(text, field):
    """The names in one of CombatIcons' string tables."""
    m = re.search(r"\b" + field + r"\s*=\s*\{(.*?)\};", text, re.S)
    return re.findall(r'"([^"]+)"', m.group(1)) if m else []


def check_index():
    """Stops with what's different if CombatIcons.cs or the enums don't match the tables here."""
    problems = []

    def same(what, ours, theirs):
        for i in range(max(len(ours), len(theirs))):
            mine = ours[i] if i < len(ours) else "nothing"
            other = theirs[i] if i < len(theirs) else "nothing"
            if mine != other:
                problems.append("%s has %s at %d, where this script has %s" % (what, other, i, mine))
                return

    with open(INDEX, encoding="utf-8") as f:
        text = f.read()
    abilities = [name for name, _, _, _ in ABILITIES]
    conditions = [name for name, _ in CONDITIONS]
    same("AbilityId", abilities, [hyphenated(n) for n in enum_names(ABILITY_ENUM)])
    same("CombatIcons.Abilities", abilities, quoted(text, "Abilities"))
    same("CombatIcons.Extras", [name for name, _, _, _ in EXTRAS], quoted(text, "Extras"))
    same("ConditionId", conditions, [hyphenated(n) for n in enum_names(CONDITION_ENUM)])
    same("CombatIcons.Conditions", conditions, quoted(text, "Conditions"))
    same("CombatIcons.Pieces", [tuple(row) for row in HUD],
         [(n, int(x), int(y), int(w), int(h), int(fr)) for n, x, y, w, h, fr in PIECE.findall(text)])

    constants = {k: int(v) for k, v in re.findall(r"const int (\w+) = (\d+);", text)}
    for name, value in (("AbilitySize", ICON), ("AbilityColumns", COLUMNS), ("ExtrasFrom", EXTRAS_FROM),
                        ("ConditionSize", CONDITION), ("FrameInset", FRAME_INSET),
                        ("HudWidth", HUD_WIDTH), ("HudHeight", HUD_HEIGHT)):
        if constants.get(name) != value:
            problems.append("CombatIcons.%s is %s, where this script has %d" % (name, constants.get(name), value))

    if problems:
        sys.exit("generate_combat_art: the sheets and the code don't match, so nothing was written.\n  "
                 + "\n  ".join(problems))


if __name__ == "__main__":
    print("Generating combat art for Legend Quest...")
    check_index()
    build_abilities()
    build_conditions()
    build_hud()
    print("Done.")
