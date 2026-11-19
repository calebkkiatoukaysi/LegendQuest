"""
Combat art for Legend Quest: the ability icons, the condition icons, and the pieces the fight's
screen is built from (the hotbar's slots, the initiative portraits, the action, bonus, reaction and
Shift pips, the Focus bar, death saves, the Soul-Link sigil, the rings under people's feet, the
plates at the places in the rows and the chevron over one you can Shift to, a support's banner and
its progress ring, and the things that fly).
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

import json
import math
import os
import re
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import generate_items as items  # noqa: E402
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


def backstab(ic):
    """A dagger come down over the top of the front row, the arc it swung through above it."""
    dagger(ic)
    ic.flip(across=False, down=True)
    end = arc(ic, "P", 12.0, 12.5, 8.5, 160, 28, 0.9, taper=0.6)
    ic.poly("P", [(end[0] - 2.6, end[1] - 1.4), (end[0] + 2.4, end[1] - 1.8), (end[0] + 0.6, end[1] + 2.8)])
    for x, y, a in ((20, 20, 230), (21, 19, 130), (19, 21, 130)):
        ic.glow(x, y, RED[3], a)


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
    ("backstab", "shadow", backstab, None),
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


def marked(ic):
    """A red target: a ring with a dot in it."""
    ic.ring("R", 6.0, 6.0, 2.4, 4.0)
    ic.disc("R", 6.0, 6.0, 1.0)
    ic.set(4, 3, "+")


def taunted(ic):
    """A bold orange "!", the shout that drew them."""
    ic.rect("O", 4, 1, 7, 2)
    ic.rect("O", 5, 3, 6, 6)
    ic.rect("O", 5, 8, 6, 9)
    ic.set(4, 1, "+")


def burning(ic):
    flame(ic, "O", 6.0, 0.8, 10.6, 7.4)
    flame(ic, "^", 6.0, 4.6, 10.0, 3.6)
    ic.set(6, 7, "+")


def drop(ic, ch, cx, top, r):
    """A drop of something, its point up."""
    cy = top + r * 2.1
    ic.disc(ch, cx, cy, r)
    ic.poly(ch, [(cx, top), (cx + r * 0.95, cy - r * 0.3), (cx - r * 0.95, cy - r * 0.3)])
    ic.set(int(cx - r * 0.45), int(cy - r * 0.2), "+")


def bleeding(ic):
    drop(ic, "R", 6.0, 1.2, 3.0)


def frozen(ic):
    """A crystal of ice: three strokes through the middle, a point on each end."""
    for angle in (90, 30, 150):
        t = math.radians(angle)
        dx, dy = 4.6 * math.cos(t), 4.6 * math.sin(t)
        ic.capsule("Q", 6.0 - dx, 6.0 - dy, 6.0 + dx, 6.0 + dy, 0.6)
    ic.disc("+", 6.0, 6.0, 0.8)


def poisoned(ic):
    """A green drop with a bubble rising off it."""
    drop(ic, "N", 5.0, 2.2, 2.7)
    ic.disc("N", 9.4, 2.6, 1.1)


def stunned(ic):
    """Three gold stars going round."""
    for cx, cy in ((3, 6), (8, 3), (8, 8)):
        for dx, dy in ((0, 0), (1, 0), (-1, 0), (0, 1), (0, -1)):
            ic.set(cx + dx, cy + dy, "G")
        ic.set(cx, cy, "+")


def blinded(ic):
    """An eye, struck through."""
    ic.ellipse("P", 6.0, 6.0, 4.8, 2.7)
    ic.rect("o", 5, 5, 6, 6)
    ic.capsule("R", 1.6, 10.2, 10.4, 1.8, 0.6)


def frightened(ic):
    """A skull."""
    ic.stamp(["..PPPPPP..",
              ".PPPPPPPP.",
              ".PPPPPPPP.",
              ".PooPPooP.",
              ".PooPPooP.",
              ".PPPPPPPP.",
              "..PPPoPPP.",
              "...PPPPP..",
              "...PoPoP.."], 1, 1)
    ic.set(3, 2, "+")


def charmed(ic):
    """A pink heart, glinting."""
    ic.stamp([".MMM..MMM.",
              "MMMMMMMMMM",
              "MMMMMMMMMM",
              "MMMMMMMMMM",
              ".MMMMMMMM.",
              "..MMMMMM..",
              "...MMMM...",
              "....MM...."], 1, 2)
    ic.set(3, 3, "+")
    ic.glow(10, 1, GLINT, 230)
    ic.glow(10, 2, ROSE[4], 160)
    ic.glow(9, 1, ROSE[4], 160)


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
    ("marked", marked),
    ("taunted", taunted),
    ("burning", burning),
    ("bleeding", bleeding),
    ("frozen", frozen),
    ("poisoned", poisoned),
    ("stunned", stunned),
    ("blinded", blinded),
    ("frightened", frightened),
    ("charmed", charmed),
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
    ("soul-link-gold", 0, 32, 24, 24, 1),
    ("soul-link-flicker", 24, 32, 24, 24, 1),
    ("soul-link-violet", 48, 32, 24, 24, 1),
    ("soul-link-broken", 72, 32, 24, 24, 1),
    ("soul-link-pulse", 96, 32, 24, 24, 1),
    ("target-foe", 120, 32, 24, 12, 4),
    ("target-ally", 120, 44, 24, 12, 4),
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
    ("focus-track", 108, 88, 8, 8, 1),
    ("focus-fill", 116, 88, 8, 8, 1),
    ("focus-preview", 124, 88, 8, 8, 1),
    ("focus-over", 132, 88, 8, 8, 1),
    ("focus-tick", 140, 88, 2, 8, 1),
    ("shift", 142, 88, 12, 12, 1),
    ("shift-spent", 154, 88, 12, 12, 1),
    ("shift-preview", 166, 88, 12, 12, 1),
    ("shift-chevron", 178, 88, 10, 8, 2),
    ("slot-plate", 0, 100, 28, 10, 5),
    ("support-banner", 140, 100, 12, 16, 1),
    ("progress-ring", 0, 116, 16, 16, 9),
)

#: hud.png's size, in pixels.
HUD_WIDTH = 256
HUD_HEIGHT = 132


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

def swap_arrows(px, py):
    """The Shift's pip: two arrows passing, the top one going right and the bottom one left."""
    return (1.5 <= px <= 7.5 and 2.6 <= py <= 4.6) or inside([(7.0, 0.8), (11.0, 3.6), (7.0, 6.4)], px, py) \
        or (4.5 <= px <= 10.5 and 7.4 <= py <= 9.4) or inside([(5.0, 5.6), (1.0, 8.4), (5.0, 11.2)], px, py)


#: Each pip's shape, and its ramp letter: a gold circle for the action, a teal triangle for the
#: bonus action, a violet diamond for the reaction, and two green arrows passing for the Shift.
PIPS = (
    ("action", "G", lambda px, py: math.hypot(px - 6.0, py - 6.0) <= 4.6),
    ("bonus", "Q", lambda px, py: inside([(6.0, 1.2), (11.0, 10.4), (1.0, 10.4)], px, py)),
    ("reaction", "V", lambda px, py: inside([(6.0, 0.8), (11.2, 6.0), (6.0, 11.2), (0.8, 6.0)], px, py)),
    ("shift", "N", swap_arrows),
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


# The Focus bar ------------------------------------------------------------- #
#
# Three-sliced: a cap three pixels wide each end and two in the middle that stretch, the middle the
# same down every column. The track goes down first at the bar's full length (100 Focus), then the
# fill as far as there's Focus, the preview over the part the skill you're pointing at would spend,
# and the over piece instead when that's more than there is. A tick marks every ten.

#: A Focus piece's size, and the track's, the fill's, the preview's and the over piece's colours,
#: top to bottom. Focus is a cool blue, so it can't be taken for health.
BAR = 8
FOCUS_TRACK = ((8, 10, 24), (14, 16, 36), (34, 40, 76))
FOCUS_FILL = ((214, 236, 255), (120, 186, 250), (62, 128, 222), (36, 78, 164))
FOCUS_PREVIEW = ((255, 255, 255), (232, 244, 255), (196, 224, 250), (150, 190, 236))
FOCUS_OVER = ((255, 200, 190), (240, 114, 108), (206, 58, 66), (148, 34, 54))


def bar_piece(img, ox, oy, ramp=None):
    """The track (ramp None) or one of the fills, which sit inside the track's edge."""
    for y in range(BAR):
        for x in range(BAR):
            corner = x in (0, BAR - 1) and y in (0, BAR - 1)
            edge = x in (0, BAR - 1) or y in (0, BAR - 1)
            if corner:
                continue
            if ramp is None:
                if edge:
                    c = INK
                else:
                    c = FOCUS_TRACK[0] if y == 1 else FOCUS_TRACK[2] if y == BAR - 2 else FOCUS_TRACK[1]
            else:
                if edge or (x in (1, BAR - 2) and y in (1, BAR - 2)):
                    continue
                c = ramp[0] if y == 1 else ramp[1] if y <= 3 else ramp[2] if y <= 5 else ramp[3]
            img[oy + y][ox + x] = list(c) + [255]


def bar_tick(img, ox, oy):
    """A notch across the bar: a dark line with a glint of the fill beside it."""
    for y in range(1, BAR - 1):
        img[oy + y][ox] = list(FOCUS_TRACK[0]) + [255]
        img[oy + y][ox + 1] = list(FOCUS_FILL[0]) + [140]


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
# The story's sigil: two interlocking hands in a circle, in five states. Gold while the bond is whole; gold
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
#: The slot plates take the same colours for a foe's slot and an ally's you can reach.
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


# The slot plates ----------------------------------------------------------- #
#
# A flat oval on the floor at every place in the rows, seen from the side the way the floor is: a
# rim with a soft dark edge outside it so it shows on any ground, and the floor showing through the
# middle. Its five frames are what the slot is to the turn: plain, a foe you can reach, an ally you
# can reach, caught in what you're aiming (filled), and somewhere you can Shift to.

#: A plate's size and its frames' colours: the rim's dark and bright, the middle, and the middle's
#: opacity.
PLATE_WIDTH = 28
PLATE_HEIGHT = 10
PLATE_EMBER = ((150, 50, 20), (252, 198, 78), (242, 134, 44))
PLATES = (
    ((70, 64, 76), (232, 224, 206), (232, 224, 206), 34),
    (RING_FOE[0], RING_FOE[2], RING_FOE[1], 64),
    (RING_ALLY[0], RING_ALLY[2], RING_ALLY[1], 64),
    (PLATE_EMBER[0], PLATE_EMBER[1], PLATE_EMBER[2], 150),
    ((28, 76, 128), (160, 236, 244), (94, 194, 222), 56),
)


def slot_plate(img, ox, oy):
    """The plate's five frames side by side."""
    for f, (dark, bright, middle, opacity) in enumerate(PLATES):
        for y in range(PLATE_HEIGHT):
            for x in range(PLATE_WIDTH):
                u = (x + 0.5 - PLATE_WIDTH / 2.0) / (PLATE_WIDTH / 2.0 - 0.5)
                v = (y + 0.5 - PLATE_HEIGHT / 2.0) / (PLATE_HEIGHT / 2.0 - 0.5)
                d = u * u + v * v
                if d > 1.0:
                    continue
                if d > 0.80:
                    c, a = dark, 190
                elif d > 0.56:
                    c, a = (bright if v < 0.2 else lerp_color(bright, dark, 0.35)), 255
                else:
                    c, a = middle, opacity
                img[oy + y][ox + f * PLATE_WIDTH + x] = list(c) + [a]


# The Shift chevron ---------------------------------------------------------- #

#: The chevron over a slot you can Shift to, pointing down at its plate: lit, and bright for the one
#: under the mouse. "o" is the outline, "#" the chevron, "+" its lit edge.
CHEVRON = ("ooo....ooo",
           "o+#o..o+#o",
           ".o+#oo+#o.",
           "..o+##+o..",
           "...o##o...",
           "....oo....",
           "..........",
           "..........")
CHEVRON_COLOURS = (((160, 236, 244), (94, 194, 222)), ((255, 255, 255), (234, 255, 255)))


def shift_chevron(img, ox, oy):
    """The chevron's two frames: plain and under the mouse."""
    for f, (lit, body) in enumerate(CHEVRON_COLOURS):
        for y, row in enumerate(CHEVRON):
            for x, ch in enumerate(row):
                c = {"o": INK, "#": body, "+": lit}.get(ch)
                if c:
                    img[oy + y][ox + f * 10 + x] = list(c) + [255]


# A support's banner ---------------------------------------------------------- #
#
# The ribbon a support's name and what it does are written on, behind the rows: three-sliced, four
# pixels at each end and four in the middle that stretch. Navy cloth with a gold edge top and
# bottom, square at its left end and cut in a swallowtail at its right.

BANNER_WIDTH = 12
BANNER_HEIGHT = 16
BANNER_CLOTH = ((40, 48, 92), (20, 24, 50))
BANNER_TRIM = ((250, 216, 139), (167, 101, 58))


def support_banner(img, ox, oy):
    w, h = BANNER_WIDTH, BANNER_HEIGHT
    for y in range(h):
        for x in range(w):
            notch = x >= w - 4 and abs(y + 0.5 - h / 2.0) < (x - (w - 5)) * 1.2
            if notch:
                continue
            edge_notch = x >= w - 5 and abs(y + 0.5 - h / 2.0) < (x - (w - 6)) * 1.2
            if y in (0, h - 1) or x == 0 or edge_notch:
                c = INK
            elif y == 1 or (x == 1 and y < h - 2):
                c = BANNER_TRIM[0]
            elif y == h - 2:
                c = BANNER_TRIM[1]
            else:
                c = lerp_color(BANNER_CLOTH[0], BANNER_CLOTH[1], (y - 2) / float(h - 5))
            img[oy + y][ox + x] = list(c) + [255]


# A support's progress ring ------------------------------------------------------ #
#
# How far along something a support is holding out for is (the crystal's charge), as a ring that
# fills clockwise from the top: frame k is k eighths of it.

RING = 16
PROGRESS_FRAMES = 9
PROGRESS_TRACK = (34, 40, 76)
PROGRESS_FILL = ((180, 152, 238), (234, 222, 255))


def progress_ring(img, ox, oy):
    for f in range(PROGRESS_FRAMES):
        for y in range(RING):
            for x in range(RING):
                dx, dy = x + 0.5 - RING / 2.0, y + 0.5 - RING / 2.0
                d = math.hypot(dx, dy)
                if d > 7.6 or d < 4.4:
                    continue
                if d > 6.8 or d < 5.0:
                    c = INK
                else:
                    turn = (math.degrees(math.atan2(dx, -dy)) % 360) / 360.0
                    c = (PROGRESS_FILL[1] if dx + dy < 0 else PROGRESS_FILL[0]) if turn < f / 8.0 else PROGRESS_TRACK
                img[oy + y][ox + f * RING + x] = list(c) + [255]


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

    for name, ramp in (("focus-track", None), ("focus-fill", FOCUS_FILL), ("focus-preview", FOCUS_PREVIEW), ("focus-over", FOCUS_OVER)):
        x, y, _, _, _ = at[name]
        bar_piece(img, x, y, ramp)
    bar_tick(img, *at["focus-tick"][:2])

    for kind in ("empty", "success", "failure"):
        x, y, _, _, _ = at["death-save-" + kind]
        death_save(img, x, y, kind)

    for state in ("gold", "flicker", "violet", "broken", "pulse"):
        x, y, _, _, _ = at["soul-link-" + state]
        paste(img, x, y, sigil(state))

    target_ring(img, *at["target-foe"][:2], RING_FOE)
    target_ring(img, *at["target-ally"][:2], RING_ALLY)
    slot_plate(img, *at["slot-plate"][:2])
    shift_chevron(img, *at["shift-chevron"][:2])
    support_banner(img, *at["support-banner"][:2])
    progress_ring(img, *at["progress-ring"][:2])

    for name, draw in SHOTS:
        x, y, w, h, frames = at[name]
        for f in range(frames):
            ic = Icon(size=SHOT, remap=EXTRA)
            draw(ic, f)
            paste(img, x + f * w, y, ic.render())

    write_png(os.path.join(OUT, "Combat", "hud.png"), img)


# --------------------------------------------------------------------------- #
# The real-time fight's icons: every class skill, every aura, and the foes' moves
# --------------------------------------------------------------------------- #
#
# The real-time hotbar, the aura rows and the nameplates find their icons by id ("fighter-cleave",
# "burning"), not by an enum's order, so these sheets are indexed by a JSON file written beside
# them (Combat/icons.json), which Components/Realtime/HudIcons reads. Every icon is 24x24, the
# same tile and the same kit as the old ability sheet, drawn at twice its size on the hotbar and at
# its own size in an aura row.
#
# 175 skills have to stay apart at a glance, so each is built from two things: its school, which
# is the tile's colour (steel blue for weapons, red for fire, gold for radiance, teal for the sea,
# brass for time, crimson for blood and so on), and its glyph, the thing drawn on it (a sword, a
# burst, a shield, a hand, a drop, a star, a chain, an arrow, a skull, a leaf, a bell, a bee, a
# glass, a clock), with a small mark in a corner where two of one class would otherwise match (a
# plus for a heal, an arrow for a buff, pips for more than one). Passives keep their school's tile
# with a gold rim inside the edge, like a medal, so they read as always on.

#: The extra materials the real-time icons use, on top of EXTRA: A for the sea's teal, J for
#: poison's acid green, T for brass.
SEA = ((14, 60, 70), (22, 104, 112), (42, 150, 150), (104, 204, 196), (200, 244, 236))
POISON = ((40, 62, 16), (78, 112, 24), (128, 170, 36), (182, 220, 70), (232, 250, 170))
BRASS = ((92, 60, 30), (146, 100, 44), (196, 150, 64), (230, 198, 110), (252, 238, 190))
RT_EXTRA = dict(EXTRA, A=SEA, J=POISON, T=BRASS)

#: The real-time schools' tiles, darkest band to lightest, on top of SCHOOLS.
RT_SCHOOLS = dict(SCHOOLS, **{
    "blood": ((48, 8, 16), (78, 14, 26), (110, 22, 36), (146, 38, 50)),
    "sea": ((10, 40, 52), (14, 62, 76), (22, 88, 100), (40, 120, 128)),
    "time": ((52, 36, 18), (82, 58, 28), (114, 82, 40), (152, 114, 58)),
    "glass": ((20, 52, 72), (30, 80, 104), (48, 112, 138), (84, 152, 176)),
    "poison": ((26, 42, 14), (42, 66, 20), (62, 94, 26), (90, 128, 38)),
    "stone": ((34, 32, 34), (54, 52, 54), (78, 74, 74), (108, 102, 98)),
    "storm": ((22, 26, 60), (34, 42, 92), (50, 62, 126), (76, 92, 164)),
    "honey": ((72, 40, 10), (110, 64, 14), (150, 94, 22), (192, 132, 40)),
    "buff": ((14, 48, 30), (22, 74, 44), (32, 102, 58), (52, 136, 78)),
    "debuff": ((60, 14, 18), (92, 22, 28), (126, 34, 38), (160, 54, 54)),
})

#: A passive's rim: the gold drawn inside the tile's edge in place of the bevel, lit at the top left.
PASSIVE_RIM = ((255, 226, 140), (198, 140, 60))

#: How the corners of an aura's tile are cut: one more pixel than a skill's, so the two kinds of
#: icon read apart when they sit side by side.
AURA_CORNER = 2


# The glyphs ---------------------------------------------------------------- #

def star(ic, ch, cx, cy, outer, inner, points=5, turn=-90.0):
    """A star of `points` points, `outer` to its tips and `inner` to the notches between."""
    pts = []
    for i in range(points * 2):
        r = outer if i % 2 == 0 else inner
        t = math.radians(turn + i * 180.0 / points)
        pts.append((cx + r * math.cos(t), cy + r * math.sin(t)))
    ic.poly(ch, pts)


def sun(ic, cx, cy, r, rays=8, reach=3.2, core="U", ray="G"):
    """A sun: its rays, then its disc over them, lit at the top left."""
    for i in range(rays):
        t = math.radians(i * 360.0 / rays - 90)
        ic.capsule(ray, cx + (r - 0.5) * math.cos(t), cy + (r - 0.5) * math.sin(t),
                   cx + (r + reach) * math.cos(t), cy + (r + reach) * math.sin(t), 1.0, 0.4)
    ic.disc(core, cx, cy, r)
    ic.set(int(cx - r * 0.45), int(cy - r * 0.45), "+")


def crescent(ic, ch, cx, cy, r, dx=2.6, dy=-1.6):
    """A crescent moon: a disc with a disc's worth bitten out of it."""
    ic.fill(ch, lambda px, py: (px - cx) ** 2 + (py - cy) ** 2 <= r * r
            and (px - cx - dx) ** 2 + (py - cy - dy) ** 2 > (r * 0.86) ** 2)


def leaf(ic, ch, cx, cy, length, width, angle, vein=True):
    """A leaf `length` long, pointed both ends, turned `angle` degrees (0 points right)."""
    t = math.radians(angle)
    ux, uy = math.cos(t), math.sin(t)

    def test(px, py):
        a = (px - cx) * ux + (py - cy) * uy
        b = -(px - cx) * uy + (py - cy) * ux
        half = length / 2.0
        return abs(a) <= half and abs(b) <= width / 2.0 * (1 - (a / half) ** 2)
    ic.fill(ch, test)
    if vein:
        ic.capsule(ch.lower(), cx - ux * length * 0.4, cy - uy * length * 0.4, cx + ux * length * 0.3, cy + uy * length * 0.3, 0.35)


def arrow(ic, ch, x0, y0, x1, y1, thick=0.8, head=3.2):
    """An arrow drawn as a sign: a shaft from one point and a solid head at the other."""
    dx, dy = x1 - x0, y1 - y0
    d = math.hypot(dx, dy)
    ux, uy = dx / d, dy / d
    bx, by = x1 - ux * head, y1 - uy * head
    ic.capsule(ch, x0, y0, bx, by, thick)
    ic.poly(ch, [(x1 + ux * 0.6, y1 + uy * 0.6), (bx - uy * head * 0.75, by + ux * head * 0.75),
                 (bx + uy * head * 0.75, by - ux * head * 0.75)])


def turning_arrow(ic, ch, cx, cy, r, start, end, thick=1.0, head=3.0):
    """An arrow round part of a circle, from `start` to `end` degrees, its head at the end."""
    tip = arc(ic, ch, cx, cy, r, start, end, thick)
    way = 1 if end > start else -1
    t = math.radians(end)
    tx, ty = -math.sin(t) * way, -math.cos(t) * way
    ic.poly(ch, [(tip[0] + tx * head, tip[1] + ty * head),
                 (tip[0] - ty * head * 0.8, tip[1] + tx * head * 0.8),
                 (tip[0] + ty * head * 0.8, tip[1] - tx * head * 0.8)])


def shot(ic, ch, x0, y0, x1, y1, fletch="R"):
    """An arrow as fired: a wooden shaft, a steel head at the far end and fletching at the near."""
    dx, dy = x1 - x0, y1 - y0
    d = math.hypot(dx, dy)
    ux, uy = dx / d, dy / d
    ic.capsule("W", x0, y0, x1 - ux * 2.0, y1 - uy * 2.0, 0.55)
    ic.poly("S", [(x1 + ux * 0.8, y1 + uy * 0.8), (x1 - ux * 2.6 - uy * 1.6, y1 - uy * 2.6 + ux * 1.6),
                  (x1 - ux * 2.6 + uy * 1.6, y1 - uy * 2.6 - ux * 1.6)])
    for s in (-1, 1):
        ic.capsule(fletch, x0 + ux * 0.5, y0 + uy * 0.5, x0 + ux * 2.6 - uy * s * 1.4, y0 + uy * 2.6 + ux * s * 1.4, 0.5)


def bolt(ic, body, core, x0, y0, x1, y1, r=3.6):
    """A thing flying, its trail from one point and its head at the other: the fire bolt's shape in any colours."""
    dx, dy = x1 - x0, y1 - y0
    d = math.hypot(dx, dy)
    nx, ny = -dy / d, dx / d
    ic.poly(body, [(x0, y0), (x1 + nx * r * 0.9, y1 + ny * r * 0.9), (x1 - nx * r * 0.9, y1 - ny * r * 0.9)])
    ic.disc(body, x1, y1, r)
    ic.disc(core, x1, y1, r * 0.55)
    ic.disc("+", x1 - 0.4, y1 - 0.4, r * 0.28)


def beam(ic, body, x0, y0, x1, y1, r=1.6):
    """A straight beam with a bright core, and a burst where it lands."""
    ic.capsule(body, x0, y0, x1, y1, r * 0.6, r)
    ic.capsule("+", x0, y0, x1, y1, 0.2, 0.5)
    star(ic, body, x1, y1, r * 2.6, r * 1.1, 6)
    ic.disc("+", x1, y1, 0.9)


def zigzag(ic, ch, pts, thick=0.9):
    """A stroke through points: lightning, a crack, a path."""
    for (x0, y0), (x1, y1) in zip(pts, pts[1:]):
        ic.capsule(ch, x0, y0, x1, y1, thick)


def waves(ic, ch, cx, cy, radii, start=-50, end=50, thick=0.8):
    """Arcs going out from a point: a sound, a shove of air."""
    for r in radii:
        arc(ic, ch, cx, cy, r, start, end, thick, taper=0.5)


def spiral(ic, ch, cx, cy, turns=2.2, reach=8.5, thick=1.0):
    """A spiral in to its middle."""
    steps = int(turns * 24)
    pts = []
    for i in range(steps + 1):
        u = i / float(steps)
        t = u * turns * 2 * math.pi
        r = reach * (1 - u) + 0.6
        pts.append((cx + r * math.cos(t), cy + r * math.sin(t)))
    for i, ((x0, y0), (x1, y1)) in enumerate(zip(pts, pts[1:])):
        ic.capsule(ch, x0, y0, x1, y1, thick * (1 - 0.5 * i / steps))


def shield(ic, rim, field, cx=12.0, top=3.5, w=7.5, h=17.0):
    """A heater shield, `w` from its middle to each side and `h` tall."""
    pts = [(cx - w, top), (cx + w, top), (cx + w, top + h * 0.45), (cx + w * 0.8, top + h * 0.7),
           (cx + w * 0.4, top + h * 0.9), (cx, top + h), (cx - w * 0.4, top + h * 0.9),
           (cx - w * 0.8, top + h * 0.7), (cx - w, top + h * 0.45)]
    ic.poly(rim, pts)
    s = 0.78
    inner = [(cx + (x - cx) * s, top + 1.4 + (y - top) * s) for x, y in pts]
    ic.poly(field, inner)


def coin(ic, cx, cy, r=3.0, ch="G"):
    """A coin, face on, its rim a shade darker."""
    ic.disc(ch.lower(), cx, cy, r)
    ic.disc(ch, cx - 0.3, cy - 0.3, r - 0.9)
    ic.set(int(cx - r * 0.4), int(cy - r * 0.4), "+")


def coin_stack(ic, cx, bottom, count, ch="G"):
    """Coins stacked edge on."""
    for i in range(count):
        y = bottom - i * 2.0
        ic.ellipse(ch.lower(), cx, y, 4.2, 1.6)
        ic.ellipse(ch, cx, y - 0.8, 4.2, 1.3)
    ic.set(int(cx - 2), int(bottom - count * 2.0 + 1), "+")


def bell(ic, cx=12.0, top=3.0, ch="G"):
    """A bell, its crown at `top`, with the clapper under its lip."""
    ic.ring(ch, cx, top + 1.2, 0.8, 1.9)
    ic.fill(ch, lambda px, py: top + 2.5 <= py <= top + 13.5 and abs(px - cx) <= 3.4 + (py - top - 2.5) ** 2 / 26.0)
    ic.rect(ch.lower(), int(cx - 7), int(top + 13), int(cx + 6), int(top + 14))
    ic.disc("I", cx, top + 16.0, 1.6)
    ic.set(int(cx - 2), int(top + 5), "+")
    ic.set(int(cx - 2), int(top + 6), "^")


def lantern(ic, cx=12.0, top=2.0, light="O"):
    """A lantern: a ring to hang it by, a cap, glass round a flame, and a foot."""
    ic.ring("I", cx, top + 1.4, 0.8, 1.8)
    ic.poly("I", [(cx - 4.5, top + 5.5), (cx + 4.5, top + 5.5), (cx + 2.5, top + 3.0), (cx - 2.5, top + 3.0)])
    ic.rect("E", int(cx - 4), int(top + 6), int(cx + 3), int(top + 14))
    ic.rect("I", int(cx - 5), int(top + 15), int(cx + 4), int(top + 16))
    for x in (int(cx - 5), int(cx + 4)):
        ic.rect("I", x, int(top + 6), x, int(top + 14))
    flame(ic, light, cx - 0.5, top + 7.0, top + 14.0, 4.0)
    ic.set(int(cx - 1), int(top + 11), "+")


def clock(ic, cx=12.0, cy=12.0, r=8.5, hands=((0, 5.0), (120, 3.6)), rim="T", face="P"):
    """A clock face: a brass rim, its four marks, and hands at (degrees from twelve, length)."""
    ic.disc(rim, cx, cy, r)
    ic.disc(face, cx, cy, r - 1.6)
    for a in (0, 90, 180, 270):
        t = math.radians(a - 90)
        ic.set(int(cx + (r - 2.6) * math.cos(t)), int(cy + (r - 2.6) * math.sin(t)), "o")
    for a, length in hands:
        t = math.radians(a - 90)
        ic.capsule("I", cx, cy, cx + length * math.cos(t), cy + length * math.sin(t), 0.55)
    ic.disc("I", cx, cy, 0.9)


def hourglass(ic, sand_top=0.5, sand_bottom=0.5, frame="W"):
    """The hourglass of the old End Turn button, with as much sand above and below as asked."""
    ic.poly("E", [(7.0, 5.0), (17.0, 5.0), (17.0, 6.5), (13.3, 11.5), (13.3, 12.5), (17.0, 17.5), (17.0, 19.0),
                  (7.0, 19.0), (7.0, 17.5), (10.7, 12.5), (10.7, 11.5), (7.0, 6.5)])
    top = 11.0 - 3.0 * sand_top
    bottom = 19.0 - 4.0 * sand_bottom
    if sand_top > 0:
        ic.fill("G", lambda px, py: top < py < 11.0 and abs(px - 12.0) < (11.0 - py) * 1.1 + 0.6)
    if sand_bottom > 0:
        ic.fill("G", lambda px, py: bottom < py < 19.0 and abs(px - 12.0) < (py - 14.5) * 1.3)
        ic.line("G", 12, 12, 12, 15)
    ic.rect(frame, 5, 3, 18, 4)
    ic.rect(frame, 5, 19, 18, 20)
    ic.set(8, 6, "+")


def eye(ic, cx=12.0, cy=12.0, rx=8.0, ry=4.6, iris="B"):
    """An eye: the white, the iris and the pupil, glinting."""
    ic.fill("P", lambda px, py: abs(py - cy) <= ry * (1 - ((px - cx) / rx) ** 2))
    ic.disc(iris, cx, cy, ry * 0.78)
    ic.disc("o", cx, cy, ry * 0.32)
    ic.set(int(cx - 1), int(cy - 2), "+")


def skull(ic, ox=6, oy=4, ch="P"):
    """A skull, 12 wide."""
    ic.stamp(["...XXXXXX...",
              "..XXXXXXXX..",
              ".XXXXXXXXXX.",
              ".XXXXXXXXXX.",
              ".XoooXXoooX.",
              ".XoooXXoooX.",
              ".XXXXooXXXX.",
              "..XXXXXXXX..",
              "...XoXoXX...",
              "...XXXXXX..."], ox, oy)
    for y in range(ic.size):
        for x in range(ic.size):
            if ic.grid[y][x] == "X":
                ic.grid[y][x] = ch
    ic.set(ox + 3, oy + 1, "+")


def bee(ic, cx, cy, big=False):
    """A bee side on, facing right: pale wings up, a gold body banded black, a dark head."""
    rx, ry = (3.6, 2.5) if big else (2.6, 1.9)
    ic.ellipse("P", cx - rx * 0.35, cy - ry - 0.9, rx * 0.45, ry * 0.6)
    ic.ellipse("p", cx + rx * 0.2, cy - ry - 0.6, rx * 0.4, ry * 0.55)
    ic.ellipse("G", cx, cy, rx, ry)
    for dx in ((-1.4, 0.6) if big else (-0.6,)):
        ic.rect("o", int(cx + dx), int(cy - ry + 0.6), int(cx + dx), int(cy + ry - 0.6))
    ic.disc("I", cx + rx, cy - 0.3, ry * 0.62)
    ic.set(int(cx - rx), int(cy), "o")


def hive(ic, cx=12.0, bottom=20.0):
    """A straw hive in bands, its door dark at the foot."""
    for i, (w, h) in enumerate(((7.4, 3.0), (6.8, 3.0), (5.6, 3.0), (3.8, 2.6))):
        y = bottom - 1.5 - i * 3.2
        ic.ellipse("G" if i % 2 == 0 else "g", cx, y, w, h * 0.62)
    ic.ellipse("o", cx, bottom - 1.2, 1.6, 1.4)
    ic.set(int(cx - 3), int(bottom - 11), "+")


def cloud(ic, ch, cx, cy, s=1.0):
    """A puff of cloud or smoke."""
    for dx, dy, r in ((-3.6, 1.0, 3.0), (0.0, -1.4, 3.8), (3.8, 0.8, 3.0), (0.4, 2.0, 3.0)):
        ic.disc(ch, cx + dx * s, cy + dy * s, r * s)
    ic.set(int(cx - 2 * s), int(cy - 3 * s), "+")


def fist(ic, ox, oy, ch="H", cuff="L"):
    """A fist, knuckles to the right."""
    ic.stamp(["..XXXXXX..",
              ".XXXXXXXX.",
              "XXXXXXXXXX",
              "XXXXXXXXXX",
              "XXXXXXXXX.",
              ".XXXXXXX..",
              ".ZZZZZZ...",
              ".ZZZZZZ..."], ox, oy)
    for y in range(ic.size):
        for x in range(ic.size):
            if ic.grid[y][x] == "X":
                ic.grid[y][x] = ch
            elif ic.grid[y][x] == "Z":
                ic.grid[y][x] = cuff
    for dy in (1, 3):
        ic.set(ox + 6, oy + dy, ch.lower())


def note(ic, ch, x, y, flag=True):
    """A music note, its head at (x, y)."""
    ic.ellipse(ch, x, y, 1.8, 1.4)
    ic.rect(ch, int(x + 1), int(y - 6), int(x + 1), int(y))
    if flag:
        ic.capsule(ch, x + 1.5, y - 6.0, x + 3.6, y - 3.6, 0.6)


def chain(ic, ch, x0, y0, x1, y1, links=4):
    """Links of chain from one point to another, each turned the other way from the last."""
    for i in range(links):
        u = (i + 0.5) / links
        cx, cy = x0 + (x1 - x0) * u, y0 + (y1 - y0) * u
        if i % 2 == 0:
            ic.ring(ch, cx, cy, 0.9, 2.0)
        else:
            ic.ellipse(ch, cx, cy, 1.0, 1.0)
            ic.set(int(cx), int(cy), "o")


def net(ic, ch="C"):
    """A net cast wide: a fan of cords and the rings across them."""
    for a in range(200, 341, 28):
        t = math.radians(a)
        ic.capsule(ch, 12.0, 5.0, 12.0 + 13 * math.cos(t) * -1, 5.0 - 13 * math.sin(t), 0.45)
    for r in (6.0, 10.0, 14.0):
        arc(ic, ch, 12.0, 5.0, r, -160, -20, 0.5)
    ic.disc("I", 12.0, 5.0, 1.4)


def hook(ic, ch="S", cx=14.0, cy=8.0, r=3.4):
    """A hook's curve opening to the left, its point up."""
    arc(ic, ch, cx, cy, r, 100, -150, 1.0)
    ic.poly(ch, [(cx - r - 1.2, cy - 2.6), (cx - r + 1.2, cy - 0.4), (cx - r - 1.6, cy + 0.6)])


def wave(ic, ch="A", foam="P", top=4.0):
    """A wave rolling in from the right, curling over a hollow, foam along its lip."""
    rows = ("..........FFFF......",
            "........FFXXXXFF....",
            "......FFXXXXXXXXF...",
            ".....FXXXXxxxXXXXF..",
            "....FXXXx....xXXXX..",
            "...FXXXx......xXXX..",
            "...XXXx...FF...XXX..",
            "..XXXX...FXXF..XXX..",
            "..XXXx...XXxX.XXXX..",
            ".XXXXx....xx.XXXXx..",
            ".XXXXXx.....XXXXXx..",
            "XXXXXXXXx.xXXXXXxx..",
            "XXXXXXXXXXXXXXXxx...",
            "xXXXXXXXXXXXXXxxFF.F",
            "xxXXXXXXXXXXXxxXXXXX",
            "xxxxXXXXXXXXXXXXXXXX",
            "xxxxxxxxxxxxxxxxxxxx")
    ic.stamp([r.replace("X", ch).replace("x", ch.lower()).replace("F", foam) for r in rows], 2, int(top))


def flag(ic, field="B", trim="G", pole="W"):
    """A banner on its pole, swallow-tailed."""
    ic.rect(pole, 5, 2, 6, 21)
    ic.disc(trim, 5.5, 2.0, 1.3)
    ic.poly(field, [(7.0, 3.5), (19.0, 3.5), (19.0, 15.5), (15.5, 12.5), (12.0, 15.5), (7.0, 13.5)])
    ic.rect(trim, 7, 3, 19, 3)


def crown(ic, cx=12.0, base=16.0, ch="G"):
    """A crown: a band and three points with a stone in each."""
    ic.rect(ch, int(cx - 7), int(base - 3), int(cx + 6), int(base))
    ic.poly(ch, [(cx - 7.5, base - 3), (cx - 7.5, base - 9), (cx - 4.0, base - 5), (cx, base - 10),
                 (cx + 4.0, base - 5), (cx + 7.5, base - 9), (cx + 7.5, base - 3)])
    for x, y in ((cx - 7, base - 9), (cx, base - 10), (cx + 7, base - 9)):
        ic.disc("R", x, y, 1.1)
    ic.rect(ch.lower(), int(cx - 7), int(base - 1), int(cx + 6), int(base - 1))
    ic.set(int(cx - 5), int(base - 3), "+")


def candle(ic, cx, bottom, height, light="O"):
    """A candle, its flame on top."""
    ic.rect("P", int(cx - 1), int(bottom - height), int(cx + 1), int(bottom))
    flame(ic, light, cx + 0.5, bottom - height - 4.2, bottom - height + 0.4, 2.6)
    ic.set(int(cx - 1), int(bottom - height + 1), "+")


def door(ic, ch="W", cx=12.0):
    """An arched door with its iron bands."""
    ic.fill(ch, lambda px, py: abs(px - cx) <= 6.0 and (py >= 9.0 or (px - cx) ** 2 + (py - 9.0) ** 2 <= 36.0) and py <= 21.0)
    for x in (int(cx - 3), int(cx), int(cx + 3)):
        ic.rect(ch.lower(), x, 5, x, 21)
    ic.rect("I", int(cx - 6), 10, int(cx + 5), 10)
    ic.rect("I", int(cx - 6), 17, int(cx + 5), 17)


def book(ic, cover="R"):
    """A book shut, its spine at the left, pages showing at the edge."""
    ic.rect(cover, 6, 4, 17, 20)
    ic.rect("P", 16, 5, 17, 19)
    ic.rect(cover.lower(), 6, 4, 7, 20)
    ic.rect("G", 9, 7, 14, 8)


def scroll(ic, ch="P"):
    """A scroll, rolled at both ends."""
    ic.rect(ch, 7, 5, 17, 19)
    ic.ellipse(ch.lower(), 12.0, 4.8, 6.2, 1.6)
    ic.ellipse(ch.lower(), 12.0, 19.2, 6.2, 1.6)
    for y in (8, 11, 14):
        ic.rect("o", 9, y, 15, y)


def stone(ic, cx=12.0, cy=13.0, ch="I"):
    """A smooth pebble with a hole worn through it."""
    ic.ellipse(ch, cx, cy, 7.0, 5.6)
    ic.disc("o", cx + 1.0, cy - 0.4, 1.5)
    ic.set(int(cx - 4), int(cy - 3), "+")


def mirror(ic, frame="G", glass="E", cx=12.0, cy=10.0):
    """A hand mirror: an oval of glass in a frame, and its handle."""
    ic.rect(frame, int(cx - 1), int(cy + 6), int(cx), int(cy + 11))
    ic.ellipse(frame, cx, cy, 5.6, 6.6)
    ic.ellipse(glass, cx, cy, 4.2, 5.2)
    ic.line("+", int(cx - 2), int(cy - 3), int(cx), int(cy - 5))


def prism(ic):
    """A glass prism splitting a white ray into colours."""
    ic.poly("E", [(12.0, 4.0), (18.5, 17.0), (5.5, 17.0)])
    ic.capsule("P", 1.5, 14.0, 9.0, 11.5, 0.6)
    for i, ch in enumerate(("R", "O", "G", "N", "B", "V")):
        ic.capsule(ch, 15.0, 11.0 + i * 0.3, 22.0, 6.5 + i * 2.2, 0.45)
    ic.set(11, 7, "+")


def flower(ic, cx=12.0, cy=10.0, petal="M", heart_ch="G"):
    """A flower: five petals round a heart, and its stem."""
    ic.capsule("N", cx, cy + 3.0, cx - 1.0, 21.0, 0.7)
    leaf(ic, "N", cx + 3.0, 17.0, 5.0, 2.6, -30, vein=False)
    for i in range(5):
        t = math.radians(i * 72 - 90)
        ic.disc(petal, cx + 3.2 * math.cos(t), cy + 3.2 * math.sin(t), 2.4)
    ic.disc(heart_ch, cx, cy, 1.8)


def tower(ic, ch="P", roof="V"):
    """A tall tower, battlemented, a lit window and a door."""
    ic.rect(ch, 8, 7, 15, 21)
    for x in (7, 10, 13, 16):
        ic.rect(ch, x, 4, x + 1, 6)
    ic.rect(ch, 7, 6, 16, 7)
    ic.rect("O", 11, 10, 12, 12)
    ic.fill("o", lambda px, py: abs(px - 12.0) <= 1.6 and (py >= 18.5 or (px - 12.0) ** 2 + (py - 18.5) ** 2 <= 2.6) and py <= 21.0)
    for y in (14,):
        ic.rect(ch.lower(), 8, y, 15, y)
    star(ic, roof, 12.0, 2.0, 2.6, 1.0, 4, -90)
    ic.set(9, 8, "+")


def wall(ic, ch="P", top=10):
    """A wall of stone, coursed."""
    ic.rect(ch, 3, top, 20, 20)
    for row, y in enumerate(range(top + 3, 21, 3)):
        ic.rect("o", 3, y, 20, y)
    for row, y in enumerate(range(top, 21, 3)):
        for x in range(4 + (row % 2) * 3, 20, 6):
            ic.rect("o", x, y, x, min(20, y + 2))


def scythe(ic):
    """A scythe: a long snath with a grip, and its blade sweeping out from the top."""
    ic.capsule("W", 6.0, 21.0, 11.0, 3.0, 0.8)
    ic.capsule("W", 6.4, 13.0, 9.8, 14.4, 0.6)

    def blade_test(px, py):
        a = math.degrees(math.atan2(py - 11.0, px - 11.0))
        if not -92.0 <= a <= -10.0:
            return False
        u = (a + 92.0) / 82.0
        r = math.hypot(px - 11.0, py - 11.0)
        return 8.2 - 2.4 * u <= r <= 8.2 + 0.4 * (1 - u) and r >= 6.2 + 1.6 * u
    ic.fill("S", blade_test)
    ic.line("+", 12, 3, 15, 4)


def wheat(ic, cx, bottom, ch="G"):
    """A stalk of wheat."""
    ic.capsule(ch.lower(), cx, bottom, cx, bottom - 10.0, 0.4)
    for i in range(4):
        y = bottom - 9.0 + i * 1.6
        ic.ellipse(ch, cx - 1.0, y, 0.9, 0.8)
        ic.ellipse(ch, cx + 1.0, y + 0.8, 0.9, 0.8)
    ic.ellipse(ch, cx, bottom - 10.4, 0.8, 1.0)


def tree(ic):
    """A broad old tree."""
    ic.rect("W", 11, 12, 13, 21)
    ic.capsule("W", 12.0, 16.0, 8.0, 12.0, 0.6)
    for cx, cy, r in ((8.0, 9.0, 4.0), (16.0, 9.0, 4.0), (12.0, 6.0, 4.8), (12.0, 11.0, 3.6)):
        ic.disc("N", cx, cy, r)
    ic.rect("L", 4, 21, 19, 21)
    ic.set(9, 4, "+")


def footprint(ic, cx, cy, ch="P"):
    """A boot's print: its sole and, apart from it, its heel."""
    ic.ellipse(ch, cx, cy - 1.6, 1.8, 2.4)
    ic.ellipse(ch, cx, cy + 2.4, 1.3, 1.2)


def horseshoe(ic, ch="S"):
    """A horseshoe, open end down, its nail holes dark."""
    ic.fill(ch, lambda px, py: 4.4 <= math.hypot(px - 12.0, py - 10.5) <= 7.0 and py < 12.0)
    ic.fill(ch, lambda px, py: 4.4 <= abs(px - 12.0) <= 7.0 and 10.5 <= py <= 19.0)
    for x, y in ((6, 9), (8, 5), (15, 5), (17, 9), (6, 14), (17, 14)):
        ic.set(x, y, "o")


def compass(ic):
    """A compass rose."""
    ic.ring("G", 12.0, 12.0, 6.6, 8.2)
    star(ic, "P", 12.0, 12.0, 7.0, 1.6, 4, -90)
    ic.poly("R", [(12.0, 4.6), (13.5, 12.0), (10.5, 12.0)])
    ic.disc("G", 12.0, 12.0, 1.0)


def scales(ic, ch="G"):
    """A balance: its post, its beam and two pans."""
    ic.rect(ch, 11, 4, 12, 19)
    ic.rect(ch, 8, 19, 15, 20)
    ic.rect(ch, 4, 6, 19, 6)
    for cx in (5.5, 18.5):
        ic.line(ch.lower(), int(cx), 7, int(cx - 2), 12)
        ic.line(ch.lower(), int(cx), 7, int(cx + 2), 12)
        ic.ellipse(ch, cx, 13.0, 3.4, 1.4)
    ic.disc("+", 11.5, 4.0, 1.0)


def teacup(ic):
    """A cup on its saucer, steam rising."""
    ic.ellipse("P", 12.0, 19.0, 8.0, 1.6)
    ic.fill("P", lambda px, py: 11.0 <= py <= 18.0 and abs(px - 11.0) <= 5.5 - (py - 11.0) * 0.25)
    ic.ring("P", 17.6, 13.6, 1.0, 2.2)
    ic.rect("R", 7, 13, 15, 13)
    for x in (9, 13):
        ic.glow(x, 8, (240, 240, 250), 160)
        ic.glow(x + 1, 6, (240, 240, 250), 110)
        ic.glow(x, 4, (240, 240, 250), 70)


def quill(ic, ch="P"):
    """A quill pen, nib down at the left."""
    leaf(ic, ch, 14.0, 9.0, 15.0, 5.4, -45)
    ic.capsule("I", 4.0, 20.0, 9.0, 15.0, 0.6)
    ic.disc("o", 4.0, 20.5, 0.8)


def knot(ic, ch="C"):
    """A loop of rope tied off, its two ends hanging."""
    ic.ring(ch, 12.0, 9.0, 3.0, 5.0)
    ic.capsule(ch, 9.0, 13.0, 5.0, 21.0, 1.1)
    ic.capsule(ch, 15.0, 13.0, 19.0, 21.0, 1.1)
    ic.capsule(ch.lower(), 9.6, 12.6, 14.4, 15.0, 1.1)
    ic.pattern(ch, ch.lower(), lambda x, y: (x + y) % 4 == 0)


def cannon(ic):
    """A ship's cannon on its carriage, fire at its mouth."""
    ic.capsule("I", 4.0, 15.0, 15.0, 10.0, 2.6, 2.0)
    ic.disc("o", 15.6, 9.7, 1.2)
    ic.rect("W", 4, 16, 13, 18)
    ic.disc("W", 6.0, 19.0, 2.2)
    ic.disc("W", 12.0, 19.0, 2.2)
    star(ic, "O", 19.0, 7.5, 4.6, 1.8, 7)
    ic.disc("+", 19.0, 7.5, 1.2)


def club(ic, ch="W"):
    """A cudgel, thick at the head, iron-banded."""
    ic.capsule(ch, 5.0, 19.0, 15.0, 9.0, 1.0, 2.6)
    ic.disc(ch, 16.0, 8.0, 3.2)
    ic.capsule("I", 13.2, 8.0, 16.0, 10.8, 0.6)
    ic.disc("G", 4.6, 19.4, 1.2)


def pin(ic, ch="W"):
    """A belaying pin: a turned wooden handle and its shaft."""
    ic.capsule(ch, 6.0, 18.0, 13.0, 11.0, 1.0)
    ic.capsule(ch, 12.5, 11.5, 18.0, 6.0, 2.0, 2.4)
    ic.disc(ch.lower(), 13.0, 11.0, 2.4)
    ic.set(15, 7, "+")


def stick(ic, ch="W"):
    """A walking stick with a crook at the top."""
    ic.capsule(ch, 7.0, 21.0, 13.0, 6.0, 0.9)
    arc(ic, ch, 15.6, 6.6, 2.8, 180, -20, 0.9)


def whistle(ic):
    """A tin whistle, its sound going out."""
    ic.capsule("S", 4.0, 15.0, 12.0, 11.0, 2.0)
    ic.disc("S", 12.5, 11.5, 2.8)
    ic.disc("o", 12.5, 11.5, 1.0)
    ic.ring("S", 4.0, 18.5, 0.6, 1.5)
    waves(ic, "P", 13.0, 10.0, (5.0, 7.5), -60, 30)


def horn(ic):
    """A war horn curving up to its bell, bound in gold, notes coming from it."""
    steps = 14
    for i in range(steps):
        u = i / float(steps - 1)
        x = 3.5 + 11.0 * u
        y = 19.0 - 7.0 * u - 3.0 * u * u
        ic.disc("P", x, y, 0.8 + 3.4 * u * u)
    ic.ring("G", 14.5, 9.0, 2.6, 3.6)
    ic.rect("G", 7, 16, 8, 18)
    note(ic, "G", 19.0, 8.0)
    note(ic, "G", 20.0, 16.0, flag=False)


def mask(ic, ch="I"):
    """A domino mask, its eyeholes dark."""
    ic.ellipse(ch, 12.0, 11.0, 9.0, 3.6)
    ic.disc("o", 8.0, 10.6, 1.8)
    ic.disc("o", 16.0, 10.6, 1.8)
    ic.capsule(ch, 3.0, 9.0, 1.5, 6.0, 0.6)
    ic.set(5, 9, "+")


def bag(ic, ch="L", tie="G"):
    """A purse drawn shut at its neck."""
    ic.ellipse(ch, 12.0, 15.0, 6.6, 5.4)
    ic.poly(ch, [(9.0, 10.5), (15.0, 10.5), (16.5, 6.0), (7.5, 6.0)])
    ic.rect(tie, 9, 9, 14, 10)
    ic.set(8, 13, "+")


def key(ic, ch="G"):
    """A key: a ring bow, its shank and two teeth."""
    ic.ring(ch, 7.5, 8.5, 1.4, 3.2)
    ic.capsule(ch, 10.0, 11.0, 18.0, 19.0, 0.8)
    ic.stamp(["X.", "XX"], 15, 18)
    ic.stamp(["X", "X"], 17, 15)
    for y in range(ic.size):
        for x in range(ic.size):
            if ic.grid[y][x] == "X":
                ic.grid[y][x] = ch


def shackle(ic, ch="I"):
    """Manacles: two cuffs on a short chain."""
    ic.ring(ch, 6.8, 14.0, 2.0, 3.6)
    ic.ring(ch, 17.2, 14.0, 2.0, 3.6)
    chain(ic, ch, 9.8, 12.0, 14.2, 12.0, 3)
    ic.set(5, 11, "+")


def paw(ic, ch="F", claws=True):
    """A bear's paw with its claws out."""
    ic.ellipse(ch, 12.0, 15.0, 5.2, 4.4)
    for cx, cy in ((5.8, 9.4), (9.6, 6.6), (14.4, 6.6), (18.2, 9.4)):
        ic.disc(ch, cx, cy, 2.1)
        if claws:
            ic.capsule("P", cx, cy - 2.0, cx + (cx - 12.0) * 0.12, cy - 4.0, 0.5)


def bird(ic, ch="L"):
    """A hawk from the front, wings raised to stoop, its beak gold."""
    ic.stamp(["X..................X",
              "XX................XX",
              "XXX..............XXX",
              ".XXXX....XX....XXXX.",
              "..XXXXX.XXXX.XXXXX..",
              "...XXXXXXXXXXXXXX...",
              "....xXXXXXXXXXXx....",
              "......xXXXXXXx......",
              "........XXXX........",
              "........xXXx........",
              ".......GG..GG......."], 2, 5)
    for y in range(ic.size):
        for x in range(ic.size):
            if ic.grid[y][x] == "X":
                ic.grid[y][x] = ch
            elif ic.grid[y][x] == "x":
                ic.grid[y][x] = ch.lower()
    ic.set(12, 8, "G")
    ic.set(11, 8, "o")


def hand_offering(ic, ch="H"):
    """An open hand, palm up, held out."""
    ic.fill(ch, lambda px, py: ((px - 12.0) / 8.0) ** 2 + ((py - 16.0) / 4.0) ** 2 <= 1.0 and py >= 16.0)
    ic.capsule(ch, 4.6, 16.2, 4.0, 13.0, 1.3)
    ic.capsule(ch, 19.4, 16.2, 20.0, 13.0, 1.3)
    ic.rect("L", 9, 20, 15, 21)


def palm(ic, ch="H", sleeve="B"):
    """A raised open hand, stop."""
    hand(ic, 8, 6, sleeve)
    ic.stamp([".H.H.H..",
              ".H.H.H.H"], 8, 4)


def pointing(ic, ch="H", sleeve="B"):
    """A hand pointing to the right."""
    ic.rect(ch, 6, 10, 12, 15)
    ic.rect(ch, 12, 10, 20, 11)
    ic.rect(ch, 11, 13, 13, 15)
    ic.rect(ch, 5, 8, 7, 11)
    ic.rect(sleeve, 2, 10, 5, 15)


def mouth(ic):
    """A shouting mouth in a speech bubble."""
    ic.ellipse("P", 12.0, 10.0, 8.6, 6.4)
    ic.poly("P", [(15.0, 14.0), (19.5, 20.0), (11.0, 15.5)])
    ic.rect("R", 11, 4, 13, 10)
    ic.rect("R", 11, 12, 13, 13)


def drop_big(ic, ch, cx=12.0, top=3.0, r=5.2):
    """A large drop, its point up."""
    drop(ic, ch, cx, top, r)


def snowflake(ic, ch="Q", cx=12.0, cy=12.0, r=8.0):
    """A snowflake of six arms, each with a pair of twigs."""
    for a in range(0, 360, 60):
        t = math.radians(a)
        ux, uy = math.cos(t), math.sin(t)
        ic.capsule(ch, cx, cy, cx + ux * r, cy + uy * r, 0.6)
        for along in (0.55,):
            bx, by = cx + ux * r * along, cy + uy * r * along
            for s in (-1, 1):
                tt = t + s * math.radians(45)
                ic.capsule(ch, bx, by, bx + math.cos(tt) * 2.4, by + math.sin(tt) * 2.4, 0.45)
    ic.disc("+", cx, cy, 1.0)


def rune_ring(ic, ch="V", cx=12.0, cy=12.0, r=8.6):
    """A ring of runes: a circle with marks round it and a star in it."""
    ic.ring(ch, cx, cy, r - 1.3, r)
    for a in range(0, 360, 45):
        t = math.radians(a + 22.5)
        ic.set(int(cx + (r - 2.6) * math.cos(t)), int(cy + (r - 2.6) * math.sin(t)), ch)
    star(ic, ch.lower(), cx, cy, r - 3.2, (r - 3.2) * 0.42, 5)


def hexagon(ic, ch, cx=12.0, cy=12.0, r=9.0, inner=None):
    """A hexagon, flat at its top and bottom, hollow to `inner` if given."""
    pts = [(cx + r * math.cos(math.radians(a)), cy + r * math.sin(math.radians(a))) for a in range(0, 360, 60)]
    ic.poly(ch, pts)
    if inner:
        pts = [(cx + inner * math.cos(math.radians(a)), cy + inner * math.sin(math.radians(a))) for a in range(0, 360, 60)]
        ic.poly(".", pts)


def burst(ic, ch, cx=12.0, cy=12.0, outer=9.0, inner=4.0, points=8, core=None):
    """A burst: a many pointed star, with a bright core if given."""
    star(ic, ch, cx, cy, outer, inner, points, -90 + 180.0 / points / 2)
    if core:
        ic.disc(core, cx, cy, inner * 0.7)
        ic.disc("+", cx - 0.5, cy - 0.5, inner * 0.3)


def ghost(img, draw, shift, alpha, colour=(170, 200, 246)):
    """Lays a faded copy of a glyph on the tile, shifted across: an afterimage, an echo."""
    g = Icon(remap=RT_EXTRA)
    draw(g)
    for y in range(ICON):
        for x in range(ICON):
            if g.grid[y][x] and 1 < x + shift < ICON - 2:
                put(img, x + shift, y, colour + (alpha,))


# Corner marks ------------------------------------------------------------- #
#
# Each about five pixels square, in a corner the glyph leaves free, to tell apart two icons that
# would otherwise look alike: a plus for healing, an arrow for a lift or a fall, a star, a flame, a
# drop, a snowflake, a spark, and pips for how many.

MARKS = {
    "plus": (".XX.", "XXXX", "XXXX", ".XX."),
    "up": ("..X..", ".XXX.", "XXXXX", ".XXX.", ".XXX."),
    "down": (".XXX.", ".XXX.", "XXXXX", ".XXX.", "..X.."),
    "star": ("..X..", ".XXX.", "XXXXX", ".X.X."),
    "spark": (".X.", "XXX", ".X."),
    "two": ("XX.XX", "XX.XX"),
    "three": ("XX.XX.XX", "XX.XX.XX"),
    "ring": (".XXX.", "XX.XX", "X...X", "XX.XX", ".XXX."),
    "bang": ("XX", "XX", "XX", "..", "XX"),
}


def mark(ic, kind, ch, corner="tr"):
    """Lays a corner mark in a material: top right by default, or "tl", "bl", "br"."""
    rows = MARKS[kind]
    w, h = len(rows[0]), len(rows)
    x = 3 if corner[1] == "l" else ICON - 3 - w
    y = 3 if corner[0] == "t" else ICON - 3 - h
    ic.stamp([r.replace("X", ch) for r in rows], x, y)


def both(*draws):
    """One glyph drawn over another, in order."""
    def draw(ic):
        for d in draws:
            d(ic)
    return draw


def marked_with(draw, kind, ch, corner="tr"):
    """A glyph with a corner mark."""
    return both(draw, lambda ic: mark(ic, kind, ch, corner))


# The skills' glyphs ------------------------------------------------------- #
#
# One function a skill where it needs its own, the shared archetypes where it doesn't. The table
# below names every skill's school and glyph.

def item_glyph(name):
    """One of the bag's weapons or pieces of armour, as generate_items.py draws it."""
    return getattr(items, name)


def cleave(ic):
    item_glyph("battleaxe")(ic)
    arc(ic, "P", 12.0, 13.0, 10.0, 200, 120, 0.6, taper=0.7)


def riposte(ic):
    item_glyph("rapier")(ic)
    turning_arrow(ic, "P", 7.5, 7.0, 4.0, 330, 120, 0.8, 2.4)


def fighting_style(ic):
    item_glyph("longsword")(ic)
    mark(ic, "up", "N", "tl")


def action_surge(ic):
    burst(ic, "O", 12.0, 12.0, 10.5, 4.6, 10, core="^")
    for oy in (13.0, 7.0):
        ic.poly("P", [(12.0, oy - 4.0), (17.0, oy + 1.0), (14.5, oy + 1.0), (12.0, oy - 1.5), (9.5, oy + 1.0), (7.0, oy + 1.0)])


def indomitable(ic):
    shield(ic, "S", "B")
    ic.capsule("G", 12.0, 7.0, 12.0, 17.0, 0.9)
    ic.capsule("G", 8.5, 10.5, 15.5, 10.5, 0.9)


def smoke_step(ic):
    cloud(ic, "Y", 8.0, 9.0, 1.0)
    boot(ic, 9, 9, "D", "I", "I")


def poisoned_edge(ic):
    item_glyph("dagger")(ic)
    for x, y in ((14, 11), (10, 15)):
        drop(ic, "J", x, y, 1.4)


def shadow_dance(ic):
    item_glyph("dagger")(ic)
    dim(ic)
    ic.flip()
    item_glyph("dagger")(ic)
    mark(ic, "three", "V", "tl")


def bless(ic):
    ic.ring("U", 12.0, 7.0, 3.0, 4.3)
    hand(ic, 3, 12, "C")
    ic.flip()
    hand(ic, 3, 12, "C")
    for x, y in ((12, 1), (5, 4), (19, 4)):
        ic.glow(x, y, (255, 236, 160), 220)


def guiding_bolt(ic):
    bolt(ic, "U", "G", 3.0, 21.0, 15.0, 9.0, 4.0)
    star(ic, "P", 19.0, 5.0, 3.4, 1.3, 4, -90)


def dawnlight(ic):
    ic.fill("U", lambda px, py: (px - 12.0) ** 2 + (py - 16.0) ** 2 <= 36.0 and py <= 16.0)
    for a in range(-170, 0, 22):
        t = math.radians(a)
        ic.capsule("G", 12.0 + 7.0 * math.cos(t), 16.0 + 7.0 * math.sin(t), 12.0 + 10.5 * math.cos(t), 16.0 + 10.5 * math.sin(t), 0.9, 0.4)
    ic.rect("P", 3, 17, 20, 18)
    ic.rect("p", 5, 20, 18, 20)
    ic.set(9, 12, "+")


def disciple_of_life(ic):
    heart(ic, "R", 11.0, 10.0, 4.0)
    ic.set(8, 7, "+")
    mark(ic, "plus", "N", "br")


def blessed_healer(ic):
    heart(ic, "R", 8.0, 8.0, 2.8)
    heart(ic, "M", 15.5, 13.0, 2.8)
    arc(ic, "U", 12.0, 11.0, 9.0, 200, 160, 0.6)


def burning_hands(ic):
    hand(ic, 2, 13, "C")
    for cx, top, bottom, w in ((9.0, 3.0, 13.0, 5.0), (14.5, 5.0, 15.0, 5.4), (19.0, 10.0, 18.0, 4.4)):
        flame(ic, "O", cx, top, bottom, w)
        flame(ic, "^", cx, top + (bottom - top) * 0.45, bottom - 0.4, w * 0.45)


def arcane_shield(ic):
    hexagon(ic, "V", 12.0, 12.0, 9.4)
    hexagon(ic, "B", 12.0, 12.0, 7.4)
    hexagon(ic, "v", 12.0, 12.0, 4.2)
    ic.line("+", 6, 9, 9, 5)


def arcane_recovery(ic):
    turning_arrow(ic, "V", 12.0, 12.0, 7.6, 60, 330, 1.2, 3.2)
    star(ic, "P", 12.0, 12.0, 3.6, 1.5, 4, -90)


def fireball(ic):
    burst(ic, "O", 12.0, 12.0, 10.5, 6.0, 12)
    ic.disc("O", 12.0, 12.0, 6.4)
    ic.disc("^", 11.5, 11.5, 4.2)
    ic.disc("+", 10.5, 10.5, 1.8)


def empowered(ic):
    item_glyph("quarterstaff")(ic)
    star(ic, "V", 17.0, 6.0, 4.6, 1.8, 4, -90)
    ic.set(17, 6, "+")


def mace_strike(ic):
    item_glyph("mace")(ic)


def divine_smite(ic):
    item_glyph("longsword")(ic)
    ic.flip(across=False, down=True)
    burst(ic, "U", 16.0, 16.0, 6.0, 2.4, 8, core="P")


def raise_shield(ic):
    item_glyph("warden_shield")(ic)
    mark(ic, "up", "N", "tl")


def unbroken_oath(ic):
    ic.ring("U", 12.0, 12.0, 9.0, 10.4)
    shield(ic, "G", "B", 12.0, 4.5, 6.2, 15.0)
    star(ic, "U", 12.0, 10.6, 3.2, 1.3, 4, -90)


def radiant_strikes(ic):
    item_glyph("mace")(ic)
    for x, y in ((5, 5), (8, 3), (3, 9)):
        ic.glow(x, y, (255, 236, 160), 230)
    mark(ic, "star", "U", "tl")


def thorn_whip(ic):
    pts = [(4.0, 20.0), (7.0, 13.0), (12.0, 9.5), (17.0, 9.0), (20.0, 5.0)]
    zigzag(ic, "N", pts, 0.9)
    for x, y in ((6, 15), (10, 10), (15, 9), (18, 7)):
        ic.set(x + 1, y - 1, "n")
        ic.set(x - 1, y + 1, "n")
    leaf(ic, "N", 20.0, 4.0, 4.0, 2.2, -45, vein=False)


def leaf_heal(ic):
    leaf(ic, "N", 12.0, 12.0, 17.0, 9.0, -50)
    heart(ic, "R", 12.0, 11.2, 1.8)


def moonbeam(ic):
    crescent(ic, "P", 9.0, 7.5, 5.0, 2.6, -1.4)
    ic.fill("Q", lambda px, py: py > 9.0 and abs(px - 13.5) <= 1.4 + (py - 9.0) * 0.22 and py < 21.0)
    ic.fill("+", lambda px, py: py > 10.0 and abs(px - 13.5) <= 0.5 and py < 20.0)
    ic.ellipse("Q", 13.5, 20.5, 5.0, 1.2)


def natural_recovery(ic):
    leaf(ic, "N", 12.0, 12.0, 11.0, 6.0, -45)
    turning_arrow(ic, "P", 12.0, 12.0, 8.6, 110, 400, 0.8, 2.6)


def bear(ic):
    paw(ic, "F")


def natures_ward(ic):
    shield(ic, "W", "N")
    leaf(ic, "n", 12.0, 11.0, 8.0, 4.0, -90, vein=False)


def hellish_rebuke(ic):
    flame(ic, "O", 12.0, 2.0, 20.0, 12.0)
    flame(ic, "^", 12.0, 8.0, 19.0, 6.0)
    turning_arrow(ic, "V", 12.0, 13.0, 9.0, 300, 210, 0.8, 2.6)


def armour_of_shadows(ic):
    item_glyph("cloak")(ic, "V", "K", "G")


def agonising_blast(ic):
    ic.disc("V", 12.0, 12.0, 5.0)
    burst(ic, "M", 12.0, 12.0, 9.5, 4.2, 8)
    ic.disc("V", 12.0, 12.0, 4.0)
    ic.disc("+", 11.0, 11.0, 1.4)


def void_maw(ic):
    spiral(ic, "V", 12.0, 12.0, 2.4, 9.0, 1.2)
    ic.disc("o", 12.0, 12.0, 1.8)


def devils_sight(ic):
    eye(ic, 12.0, 13.0, 8.0, 4.4, "R")
    ic.poly("V", [(5.0, 9.0), (3.0, 2.0), (8.0, 7.6)])
    ic.poly("V", [(19.0, 9.0), (21.0, 2.0), (16.0, 7.6)])


def thunderwave(ic):
    ic.disc("P", 6.0, 12.0, 2.2)
    waves(ic, "B", 6.0, 12.0, (5.5, 9.0, 12.5), -55, 55, 1.0)


def song_of_rest(ic):
    crescent(ic, "U", 15.5, 7.5, 4.6, 2.2, -1.6)
    note(ic, "P", 6.0, 17.0)
    note(ic, "P", 12.0, 19.0)


def countercharm(ic):
    shield(ic, "G", "M", 12.0, 3.5, 7.0, 17.0)
    note(ic, "P", 11.0, 14.0)


def hold_the_line(ic):
    for cx in (6.5, 17.5):
        shield(ic, "s", "b", cx, 5.0, 4.4, 13.0)
    shield(ic, "S", "B", 12.0, 6.0, 4.8, 14.0)
    ic.rect("G", 2, 20, 21, 21)


def shield_wall(ic):
    wall(ic, "P", 12)
    shield(ic, "S", "B", 12.0, 2.5, 5.0, 13.0)


def keeper_of_the_peace(ic):
    key(ic)
    mark(ic, "star", "G", "tr")


def gaff(ic):
    ic.capsule("W", 4.0, 21.0, 14.0, 9.0, 0.9)
    hook(ic, "S", 15.0, 7.0, 3.6)


def haul_in(ic):
    hook(ic, "S", 17.0, 8.0, 3.2)
    ic.capsule("C", 13.4, 8.4, 4.0, 14.0, 0.5)
    arrow(ic, "P", 18.0, 17.0, 6.0, 17.0, 0.8, 3.0)


def weather_eye(ic):
    eye(ic, 12.0, 14.0, 8.0, 4.2, "A")
    cloud(ic, "Y", 12.0, 5.0, 0.7)


def high_tide(ic):
    wave(ic, "A", "P", 4.0)


def mend(ic):
    knot(ic, "C")
    mark(ic, "plus", "N", "tr")


def belaying_pin(ic):
    pin(ic)
    for x, y in ((4, 6), (7, 3), (3, 10)):
        ic.stamp([".X.", "XXX", ".X."], x, y)
        for yy in range(y, y + 3):
            for xx in range(x, x + 3):
                if ic.grid[yy][xx] == "X":
                    ic.grid[yy][xx] = "G"


def rum_ration(ic):
    ic.rect("W", 10, 3, 13, 6)
    ic.fill("L", lambda px, py: 7.0 <= py <= 20.0 and abs(px - 11.5) <= (2.0 if py < 10.0 else 5.0))
    ic.rect("C", 8, 13, 15, 16)
    ic.set(8, 11, "+")
    mark(ic, "plus", "N", "tr")


def swing_across(ic):
    ic.capsule("C", 13.0, 1.0, 7.0, 12.0, 0.5)
    ic.disc("H", 6.5, 12.5, 1.8)
    ic.capsule("B", 6.5, 14.0, 7.5, 19.0, 1.4)
    ic.capsule("B", 7.5, 19.0, 5.0, 21.0, 0.6)
    turning_arrow(ic, "P", 13.0, 1.0, 13.0, 235, 330, 0.8, 2.8)


def sea_legs(ic):
    boot(ic, 5, 3, "D", "A", "I")
    ic.fill("A", lambda px, py: py >= 18.0 + 1.4 * math.sin(px * 0.8) and py <= 21.0)


def broadside(ic):
    cannon(ic)


def captains_share(ic):
    for x, y in ((7.0, 17.5), (12.0, 17.5), (17.0, 17.5), (9.5, 13.5), (14.5, 13.5), (12.0, 9.5)):
        coin(ic, x, y, 2.9)
    coin(ic, 17.5, 6.0, 2.4)


def ranger_shot(ic):
    item_glyph("longbow")(ic)


def hunters_mark(ic):
    ic.ring("R", 12.0, 12.0, 6.0, 7.6)
    ic.ring("R", 12.0, 12.0, 2.2, 3.4)
    for x0, y0, x1, y1 in ((12, 2, 12, 6), (12, 18, 12, 22), (2, 12, 6, 12), (18, 12, 22, 12)):
        ic.line("P", x0, y0, x1, y1)


def volley(ic):
    for off in (-5.0, 0.0, 5.0):
        shot(ic, "W", 4.0 + off, 20.0 - off * 0.2 + 0.0, 18.0 + off * 0.4, 6.0 - off * 0.2 + off)


def count_the_ground(ic):
    footprint(ic, 7.0, 17.0)
    footprint(ic, 12.0, 11.0, "p")
    footprint(ic, 17.0, 6.0)


def rain_of_arrows(ic):
    for x, y in ((4, 3), (10, 6), (16, 2), (7, 12), (14, 11), (19, 9)):
        shot(ic, "W", x - 1.5, y - 1.0, x + 2.5, y + 7.0)
    ic.rect("L", 3, 21, 20, 21)


def old_growth(ic):
    tree(ic)


def force_bolt(ic):
    bolt(ic, "V", "P", 3.0, 12.0, 15.0, 12.0, 4.2)
    for y in (6, 18):
        arrow(ic, "P", 15.0, y, 21.0, y, 0.5, 2.0)


def ward(ic):
    rune_ring(ic, "B")


def counterspell(ic):
    rune_ring(ic, "V")
    ic.capsule("R", 5.0, 19.0, 19.0, 5.0, 1.2)


def chain_lightning(ic):
    zigzag(ic, "U", [(3.0, 5.0), (8.0, 10.0), (6.0, 12.0), (12.0, 16.0)], 0.9)
    zigzag(ic, "U", [(12.0, 16.0), (15.0, 10.0), (17.0, 12.0), (20.0, 5.0)], 0.7)
    for x, y in ((3, 5), (12, 16), (20, 5)):
        ic.disc("P", x, y, 1.6)


def spire_training(ic):
    tower(ic, "P", "V")


def spire_lance(ic):
    beam(ic, "V", 2.0, 21.0, 16.0, 8.0, 2.2)


def archmages_patience(ic):
    book(ic, "V")
    star(ic, "G", 11.5, 15.0, 3.0, 1.2, 5)


def ember_edge(ic):
    item_glyph("shortsword")(ic)
    for x, y, a in ((16, 9, 230), (13, 12, 200), (18, 5, 180), (11, 14, 150)):
        ic.glow(x, y, FIRE[3], a)
        ic.glow(x + 1, y - 1, FIRE[2], a // 2)


def ash_cloud(ic):
    cloud(ic, "Y", 12.0, 11.0, 1.4)
    for x, y in ((6, 18), (11, 20), (16, 17), (18, 20)):
        ic.glow(x, y, FIRE[3], 220)


def endure(ic):
    shield(ic, "I", "S", 12.0, 3.5, 7.2, 17.0)
    flame(ic, "O", 12.0, 7.0, 16.0, 5.0)


def burn_the_fields(ic):
    for cx in (6.0, 11.0, 16.0):
        wheat(ic, cx + 0.5, 21.0)
    flame(ic, "O", 13.0, 8.0, 21.0, 8.0)
    flame(ic, "^", 13.0, 13.0, 20.5, 4.0)


def doomward_born(ic):
    ic.ring("I", 12.0, 12.0, 7.6, 9.2)
    flame(ic, "O", 12.0, 4.0, 18.0, 9.0)
    flame(ic, "^", 12.0, 9.0, 17.5, 4.2)


def last_harvest(ic):
    scythe(ic)
    flame(ic, "O", 17.0, 10.0, 20.0, 5.0)


def nothing_left_to_lose(ic):
    heart(ic, "R", 12.0, 9.0, 4.2)
    zigzag(ic, "o", [(11.0, 5.0), (13.0, 9.0), (10.5, 12.0), (12.5, 16.0)], 0.4)
    flame(ic, "O", 18.0, 14.0, 21.0, 4.0)


def shield_bash(ic):
    item_glyph("steel_shield")(ic)
    for y in (6, 11, 16):
        ic.line("P", 1, y, 3, y)


def cover(ic):
    bust(ic, "B", 15.0, 6.0)
    shield(ic, "S", "I", 9.0, 5.0, 5.4, 15.0)


def hold(ic):
    wall(ic, "P", 14)
    palm(ic)


def stone_skin(ic):
    item_glyph("plate_armour")(ic)
    ic.pattern("S", "P", lambda x, y: (x * 3 + y * 5) % 7 < 3)


def forty_one_names(ic):
    ic.poly("P", [(6.0, 21.0), (6.0, 6.0), (12.0, 2.0), (18.0, 6.0), (18.0, 21.0)])
    for y in range(8, 20, 2):
        ic.rect("o", 8, y, 15 - (y % 3), y)


def nobody_left_behind(ic):
    bust(ic, "B", 9.0, 7.0)
    arrow(ic, "U", 18.0, 21.0, 18.0, 3.0, 0.9, 3.4)


def staying_behind(ic):
    shield(ic, "S", "I")
    heart(ic, "R", 12.0, 9.8, 2.6)
    ic.set(10, 9, "+")


def cinder(ic):
    for x, y, r in ((12.0, 12.0, 3.2), (6.0, 7.0, 1.6), (17.0, 17.0, 1.4), (17.5, 6.0, 1.2), (6.0, 17.0, 1.0)):
        ic.disc("O", x, y, r)
        ic.disc("+", x - 0.4, y - 0.4, r * 0.4)
    bolt(ic, "O", "^", 3.0, 21.0, 12.0, 12.0, 3.2)


def cauterise(ic):
    item_glyph("dagger")(ic)
    ic.pattern("S", "O", lambda x, y: x + y > 26)
    mark(ic, "plus", "N", "tl")


def read_the_wound(ic):
    drop(ic, "R", 9.0, 5.0, 4.2)
    ic.ring("G", 14.0, 13.0, 3.6, 5.0)
    ic.disc("E", 14.0, 13.0, 3.6)
    ic.capsule("W", 17.5, 16.5, 21.0, 20.0, 1.0)


def warm_hands(ic):
    hand_offering(ic)
    flame(ic, "O", 12.0, 3.0, 15.0, 7.0)
    heart(ic, "R", 12.0, 9.6, 1.8)


def index_of_remedies(ic):
    book(ic, "N")
    mark(ic, "plus", "P", "tr")
    ic.rect("P", 10, 11, 12, 15)
    ic.rect("P", 9, 12, 13, 14)


def rekindle(ic):
    flame(ic, "O", 12.0, 1.0, 21.0, 15.0)
    flame(ic, "^", 12.0, 7.0, 20.0, 8.0)
    heart(ic, "R", 12.0, 13.0, 2.6)


def notes_in_the_margin(ic):
    scroll(ic)
    ic.poly("G", [(17.0, 3.0), (21.0, 1.0), (18.0, 6.0)])
    ic.capsule("I", 12.0, 13.0, 17.5, 4.0, 0.5)


def twin_cuts(ic):
    item_glyph("dagger")(ic)
    dim(ic)
    ic.flip()
    item_glyph("dagger")(ic)


def pick_pocket(ic):
    bag(ic, "L", "G")
    hand(ic, 12, 0, "I", down=True)
    coin(ic, 6.0, 5.0, 2.6)


def rooftops(ic):
    ic.poly("R", [(2.0, 21.0), (2.0, 16.0), (8.0, 11.0), (14.0, 16.0), (14.0, 21.0)])
    ic.poly("r", [(12.0, 21.0), (12.0, 18.0), (17.0, 14.0), (22.0, 18.0), (22.0, 21.0)])
    turning_arrow(ic, "P", 13.0, 12.0, 6.0, 170, 20, 0.8, 2.6)


def low_blow(ic):
    fist(ic, 6, 5)
    arrow(ic, "P", 16.0, 14.0, 16.0, 21.0, 0.7, 2.6)


def lane_rules(ic):
    ic.rect("W", 11, 4, 12, 21)
    ic.poly("P", [(4.0, 5.0), (16.0, 5.0), (19.0, 7.5), (16.0, 10.0), (4.0, 10.0)])
    ic.poly("p", [(20.0, 11.0), (8.0, 11.0), (5.0, 13.5), (8.0, 16.0), (20.0, 16.0)])


def everyones_pocket(ic):
    bag(ic, "L", "G")
    for x, y in ((4, 6), (19, 5), (5, 18), (19, 18)):
        coin(ic, x, y, 2.2)


def name_on_the_door(ic):
    door(ic, "W")
    ic.rect("G", 9, 12, 14, 14)


def walking_stick(ic):
    stick(ic)


def throw_anything(ic):
    boot(ic, 8, 5, "L", "D", "D")
    for y in (8, 12, 16):
        ic.line("P", 2, y, 5, y)


def duchess_kicks(ic):
    horseshoe(ic)
    burst(ic, "U", 12.0, 19.0, 4.0, 1.6, 6)


def rummage(ic):
    bag(ic, "L", "R")
    ic.disc("R", 17.5, 6.0, 2.6)
    ic.rect("E", 17, 2, 18, 3)


def haggle(ic):
    scales(ic, "G")


def lucky_stone(ic):
    stone(ic, 12.0, 13.0, "I")
    ic.sparkle(17, 7, "rare")


def seen_it_all(ic):
    compass(ic)


def morning_strike(ic):
    item_glyph("sunsteel_blade")(ic)


def sunburst(ic):
    sun(ic, 12.0, 12.0, 4.6, 12, 4.0)


def vigil(ic):
    ic.ring("U", 12.0, 9.0, 6.0, 6.8)
    candle(ic, 12.0, 21.0, 9.0, "U")


def call_the_order(ic):
    flag(ic, "B", "G")
    sun(ic, 13.0, 9.0, 2.2, 8, 1.6)


def where_the_dark_gathers(ic):
    ic.disc("U", 12.0, 12.0, 8.0)
    ic.fill("V", lambda px, py: (px - 12.0) ** 2 + (py - 12.0) ** 2 <= 64.0 and px > 12.0)
    ic.set(8, 8, "+")


def gilded_dawn(ic):
    dawnlight(ic)
    item_glyph("greatsword")(ic)


def sorry_somewhere_useful(ic):
    hand_offering(ic, "S")
    heart(ic, "R", 12.0, 9.6, 3.0)


def blood_lash(ic):
    pts = [(3.0, 20.0), (7.0, 12.0), (12.0, 10.0), (16.0, 12.0), (20.0, 5.0)]
    zigzag(ic, "R", pts, 1.0)
    drop(ic, "R", 19.0, 13.0, 1.8)


def drain(ic):
    drop(ic, "R", 12.0, 5.0, 5.0)
    turning_arrow(ic, "P", 12.0, 13.0, 9.0, 300, 130, 0.8, 2.6)


def blood_shield(ic):
    shield(ic, "R", "r")
    drop(ic, "M", 12.0, 7.0, 2.4)


def given_freely(ic):
    hand_offering(ic)
    drop(ic, "R", 12.0, 4.0, 3.6)


def the_old_enemy(ic):
    ic.disc("U", 12.0, 12.0, 8.0)
    crescent(ic, "B", 12.0, 12.0, 8.0, -3.0, 0.0)
    ic.fill("B", lambda px, py: (px - 12.0) ** 2 + (py - 12.0) ** 2 <= 64.0 and px > 13.0)


def crimson_tide(ic):
    wave(ic, "R", "M", 4.0)


def forty_years_of_manners(ic):
    teacup(ic)


def swarm(ic):
    for x, y in ((6.0, 8.0), (14.0, 5.0), (17.0, 13.0), (9.0, 15.0), (13.0, 19.0)):
        bee(ic, x, y)


def honey_salve(ic):
    ic.fill("G", lambda px, py: 8.0 <= py <= 20.0 and abs(px - 12.0) <= 6.0)
    ic.rect("C", 5, 6, 18, 8)
    ic.fill("G", lambda px, py: 8.0 <= py <= 12.0 and abs(px - 8.0) <= 1.2)
    ic.set(8, 13, "+")
    mark(ic, "plus", "N", "tr")


def smoke_pot(ic):
    ic.fill("I", lambda px, py: 12.0 <= py <= 20.0 and abs(px - 12.0) <= 6.0 - (py - 12.0) * 0.2)
    ic.rect("I", 9, 9, 14, 11)
    cloud(ic, "Y", 12.0, 5.0, 0.9)


def angry_hive(ic):
    hive(ic, 10.0, 21.0)
    bee(ic, 18.0, 7.0)
    bee(ic, 19.0, 14.0)
    mark(ic, "bang", "R", "tl")


def thirty_summers(ic):
    sun(ic, 12.0, 10.0, 4.0, 8, 3.0)
    bee(ic, 16.0, 17.0, big=True)


def the_whole_hive(ic):
    hive(ic, 12.0, 21.0)
    for x, y in ((4.0, 6.0), (10.0, 3.0), (17.0, 4.0), (20.0, 10.0), (4.0, 12.0)):
        bee(ic, x, y)


def they_know_me(ic):
    hand_offering(ic)
    bee(ic, 12.0, 11.0, big=True)


def prism_ray(ic):
    prism(ic)


def mirror_image(ic):
    bust(ic, "E", 12.0, 5.0)


def mirror_image_under(img):
    ghost(img, lambda g: bust(g, "S", 12.0, 5.0), -6, 170, (220, 236, 255))
    ghost(img, lambda g: bust(g, "S", 12.0, 5.0), 6, 170, (220, 236, 255))


def refract(ic):
    ic.rect("E", 10, 3, 13, 20)
    ic.capsule("P", 2.0, 18.0, 10.0, 12.0, 0.6)
    ic.capsule("U", 13.0, 12.0, 21.0, 4.0, 0.6)


def glasshouse(ic):
    ic.fill("E", lambda px, py: (px - 12.0) ** 2 + (py - 18.0) ** 2 <= 100.0 and py <= 19.0)
    for a in (-150, -120, -90, -60, -30):
        t = math.radians(a)
        ic.capsule("S", 12.0, 18.0, 12.0 + 10.0 * math.cos(t), 18.0 + 10.0 * math.sin(t), 0.4)
    arc(ic, "S", 12.0, 18.0, 6.0, 180, 0, 0.4)
    ic.rect("S", 2, 19, 21, 20)
    ic.set(8, 11, "+")


def night_flowers(ic):
    flower(ic, 11.0, 11.0, "P", "G")
    crescent(ic, "U", 18.0, 5.0, 3.0, -1.6, -1.0)


def given_in_love(ic):
    heart(ic, "E", 12.0, 9.6, 5.0)
    for x, y in ((3, 4), (20, 4), (12, 1)):
        ic.glow(x, y, (255, 236, 160), 220)
    ic.set(8, 7, "+")


def choose_it_yourself(ic):
    chain(ic, "S", 3.0, 18.0, 10.0, 11.0, 3)
    chain(ic, "S", 14.0, 9.0, 21.0, 2.0, 3)
    ic.glow(12, 10, GLINT, 255)
    for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
        ic.glow(12 + dx, 10 + dy, (255, 232, 160), 200)


def lantern_swing(ic):
    lantern(ic, 13.0, 4.0)
    ic.capsule("W", 3.0, 5.0, 12.0, 3.0, 0.6)
    arc(ic, "P", 3.0, 4.0, 15.0, -10, -50, 0.5)


def sanctuary(ic):
    ic.fill("U", lambda px, py: abs(px - 12.0) <= 8.0 and (py >= 10.0 or (px - 12.0) ** 2 + (py - 10.0) ** 2 <= 64.0) and py <= 21.0)
    ic.fill("B", lambda px, py: abs(px - 12.0) <= 5.0 and (py >= 11.0 or (px - 12.0) ** 2 + (py - 11.0) ** 2 <= 25.0) and py <= 21.0)
    candle(ic, 12.0, 20.0, 5.0, "U")


def vespers(ic):
    candle(ic, 6.0, 20.0, 7.0)
    candle(ic, 12.0, 20.0, 10.0)
    candle(ic, 18.0, 20.0, 6.0)


def reveal(ic):
    eye(ic, 12.0, 13.0, 8.4, 4.8, "G")
    for a in (-150, -120, -90, -60, -30):
        t = math.radians(a)
        ic.capsule("U", 12.0 + 7.0 * math.cos(t), 11.0 + 7.0 * math.sin(t), 12.0 + 10.0 * math.cos(t), 11.0 + 10.0 * math.sin(t), 0.6)


def everyone_who_knocks(ic):
    door(ic, "W")
    ic.ring("G", 15.0, 13.0, 1.0, 2.2)


def the_bell(ic):
    bell(ic)
    waves(ic, "P", 12.0, 11.0, (9.5,), 160, 200, 0.6)
    waves(ic, "P", 12.0, 11.0, (9.5,), -20, 20, 0.6)


def bees_guard_the_honey(ic):
    shield(ic, "G", "g")
    bee(ic, 12.0, 12.0, big=True)


def riposte_strike(ic):
    item_glyph("rapier")(ic)
    mark(ic, "up", "S", "tl")


def command(ic):
    pointing(ic, "S", "R")
    crown(ic, 12.0, 7.0)


def told_rudely(ic):
    mouth(ic)


def rally(ic):
    flag(ic, "R", "G")
    mark(ic, "plus", "N", "br")


def no_procedure(ic):
    scroll(ic)
    ic.capsule("R", 5.0, 19.0, 19.0, 5.0, 1.0)


def my_fathers_sword(ic):
    item_glyph("greatsword")(ic)
    crown(ic, 7.0, 9.0)


def somebody_allowed_to_say_no(ic):
    crown(ic, 12.0, 14.0)
    ic.capsule("R", 4.0, 20.0, 20.0, 4.0, 1.0)


def second_hand(ic):
    clock(ic, hands=((60, 6.0),))


def rewind(ic):
    clock(ic, 12.0, 12.0, 7.0, hands=((0, 4.0), (270, 3.0)))
    turning_arrow(ic, "P", 12.0, 12.0, 9.6, 60, 210, 0.8, 2.6)


def haste_glyph(ic):
    hourglass(ic, 0.3, 0.8, "T")
    for oy in (7, 13):
        ic.poly("U", [(18.0, oy), (22.0, oy + 3.0), (18.0, oy + 6.0), (19.6, oy + 3.0)])


def stop(ic):
    clock(ic, rim="Q", face="P", hands=((0, 5.0), (90, 3.6)))
    ic.rect("I", 18, 15, 19, 21)
    ic.rect("I", 21, 15, 22, 21)


def echo_strike(ic):
    item_glyph("quarterstaff")(ic)


def echo_strike_under(img):
    ghost(img, lambda g: item_glyph("quarterstaff")(g), -4, 170, (250, 220, 150))
    ghost(img, lambda g: item_glyph("quarterstaff")(g), -8, 100, (250, 220, 150))


def bleed_the_clock(ic):
    clock(ic, 12.0, 10.0, 7.6, hands=((300, 4.2), (30, 3.0)))
    for x, y in ((8, 17), (14, 18)):
        drop(ic, "T", x, y, 1.6)


def stolen_minute(ic):
    hourglass(ic, 0.6, 0.4, "T")
    mark(ic, "spark", "P", "tr")


def staff_strike(ic):
    item_glyph("quarterstaff")(ic)


def soul_link(ic):
    soul_link_strike(ic)


def telegraph(ic):
    ic.poly("O", [(12.0, 2.0), (21.5, 20.0), (2.5, 20.0)])
    ic.rect("o", 11, 8, 12, 14)
    ic.rect("o", 11, 16, 12, 17)


# The foes' moves, for the icon a nameplate shows over a foe winding one up.

def hook_pull(ic):
    hook(ic, "S", 15.0, 8.0, 3.6)
    ic.capsule("C", 11.4, 8.4, 3.0, 12.0, 0.5)
    arrow(ic, "P", 20.0, 18.0, 6.0, 18.0, 0.8, 3.0)


def feint(ic):
    item_glyph("rapier")(ic)
    mark(ic, "bang", "G", "tl")


def uprooting_charge(ic):
    for y in (8, 13, 18):
        arrow(ic, "N", 3.0, y, 20.0, y, 0.7, 3.0)
    ic.pattern("N", "W", lambda x, y: x < 8)


def ashen_bolt(ic):
    bolt(ic, "V", "Y", 3.0, 21.0, 15.0, 9.0, 4.0)


def bite(ic):
    ic.poly("P", [(4.0, 6.0), (20.0, 6.0), (17.0, 10.0), (15.0, 7.5), (13.0, 10.0), (11.0, 7.5), (9.0, 10.0), (7.0, 7.5)])
    ic.poly("P", [(4.0, 18.0), (20.0, 18.0), (17.0, 14.0), (15.0, 16.5), (13.0, 14.0), (11.0, 16.5), (9.0, 14.0), (7.0, 16.5)])


def claws(ic):
    for off in (-5, 0, 5):
        ic.capsule("P", 17.0 + off, 3.0, 12.0 + off, 12.0, 0.4, 1.4)
        ic.capsule("P", 12.0 + off, 12.0, 7.0 + off, 20.0, 1.4, 0.4)


def roar(ic):
    bite(ic)
    waves(ic, "P", 2.0, 12.0, (19.0, 21.5), -25, 25, 0.6)


# Auras ------------------------------------------------------------------- #

def a_stunned(ic):
    ic.ellipse("Y", 12.0, 16.0, 8.4, 2.4)
    ic.ellipse(".", 12.0, 16.0, 7.0, 1.2)
    for cx, cy in ((5.6, 12.0), (12.0, 7.0), (18.4, 12.0)):
        star(ic, "G", cx, cy, 4.2, 1.7, 5)
    ic.set(12, 6, "+")


def a_frozen(ic):
    snowflake(ic)


def a_restrained(ic):
    entangle(ic)


def a_prone(ic):
    ic.disc("P", 5.0, 15.0, 2.6)
    ic.capsule("B", 8.0, 15.5, 18.0, 15.5, 2.4)
    ic.capsule("L", 18.0, 16.0, 21.0, 16.0, 1.2)
    ic.rect("W", 2, 19, 21, 20)
    arrow(ic, "P", 12.0, 3.0, 12.0, 11.0, 0.7, 2.6)


def a_blinded(ic):
    eye(ic, 12.0, 12.0, 8.6, 5.0, "B")
    ic.capsule("R", 4.0, 20.0, 20.0, 4.0, 1.0)


def a_poisoned(ic):
    drop(ic, "J", 10.0, 3.0, 5.0)
    ic.disc("J", 18.0, 6.0, 1.8)
    ic.disc("J", 17.0, 17.0, 1.2)


def a_frightened(ic):
    skull(ic, 6, 6, "P")


def a_charmed(ic):
    heart(ic, "M", 12.0, 9.0, 4.2)
    ic.set(8, 7, "+")
    for x, y in ((20, 3), (4, 18)):
        ic.glow(x, y, ROSE[4], 220)


def a_taunted(ic):
    mouth(ic)


def a_marked(ic):
    hunters_mark(ic)


def a_slowed(ic):
    ic.fill("G", lambda px, py: 4.0 <= math.hypot(px - 13.0, py - 11.0) <= 6.4 or math.hypot(px - 13.0, py - 11.0) < 1.6)
    spiral(ic, "g", 13.0, 11.0, 1.5, 5.6, 0.6)
    ic.capsule("P", 3.0, 18.0, 20.0, 18.0, 1.6)
    ic.capsule("P", 3.5, 17.0, 3.5, 13.0, 0.8)
    ic.disc("P", 3.5, 12.0, 1.2)


def a_burning(ic):
    flame(ic, "O", 12.0, 2.0, 21.0, 13.0)
    flame(ic, "^", 12.0, 9.0, 20.5, 6.6)
    ic.set(12, 15, "+")


def a_bleeding(ic):
    drop(ic, "R", 12.0, 2.0, 6.0)


def a_concentration(ic):
    ic.ring("V", 12.0, 12.0, 7.0, 8.6)
    ic.ring("V", 12.0, 12.0, 3.0, 4.2)
    ic.disc("P", 12.0, 12.0, 1.6)


def a_hasted(ic):
    for ox in (3.0, 10.0):
        ic.poly("U", [(ox, 5.0), (ox + 6.0, 12.0), (ox, 19.0), (ox + 3.0, 12.0)])
    ic.rect("P", 17, 10, 21, 13)


def a_shielded(ic):
    ic.ring("Q", 12.0, 12.0, 7.6, 9.2)
    shield(ic, "S", "B", 12.0, 6.0, 4.4, 12.0)


def a_blessed(ic):
    ic.ring("U", 12.0, 6.0, 3.0, 4.4)
    sun(ic, 12.0, 15.0, 3.4, 8, 2.8)


def a_inspired(ic):
    note(ic, "G", 8.0, 17.0)
    note(ic, "G", 15.0, 14.0)
    mark(ic, "spark", "P", "tr")


def a_hexed(ic):
    hex_(ic)


def a_warded(ic):
    rune_ring(ic, "B")


def a_regenerating(ic):
    turning_arrow(ic, "N", 12.0, 12.0, 8.4, 60, 330, 1.2, 3.0)
    ic.rect("P", 11, 8, 12, 15)
    ic.rect("P", 8, 11, 15, 12)


def a_buff(ic):
    arrow(ic, "N", 12.0, 21.0, 12.0, 3.0, 2.4, 6.0)


def a_debuff(ic):
    arrow(ic, "R", 12.0, 3.0, 12.0, 21.0, 2.4, 6.0)


def a_downed(ic):
    heart(ic, "R", 12.0, 9.0, 4.4)
    zigzag(ic, "o", [(11.0, 5.0), (13.5, 9.0), (10.5, 12.0), (12.5, 17.0)], 0.5)


def a_dodging(ic):
    dodge(ic)


def a_resistant(ic):
    item_glyph("plate_armour")(ic)
    ic.pattern("S", "P", lambda x, y: (x * 3 + y * 5) % 7 < 3)


def a_sanctuary(ic):
    sanctuary(ic)


def a_mirror_images(ic):
    mirror(ic)


def a_wild_shape(ic):
    paw(ic, "F")


def a_helped(ic):
    help_(ic)


def a_cover(ic):
    cover(ic)


def a_riposte(ic):
    riposte(ic)


#: Every aura's icon: its id, the school of its tile, and its glyph. The design's conditions
#: (11-combat.md §3.6, §4.4) first, then the buff and debuff anything without its own icon falls
#: back on, then the ones the class kits put on people.
AURAS = (
    ("stunned", "foe", a_stunned),
    ("frozen", "frost", a_frozen),
    ("restrained", "nature", a_restrained),
    ("prone", "foe", a_prone),
    ("blinded", "shadow", a_blinded),
    ("poisoned", "poison", a_poisoned),
    ("frightened", "eldritch", a_frightened),
    ("charmed", "song", a_charmed),
    ("taunted", "fire", a_taunted),
    ("marked", "foe", a_marked),
    ("slowed", "time", a_slowed),
    ("burning", "fire", a_burning),
    ("bleeding", "blood", a_bleeding),
    ("concentration", "arcane", a_concentration),
    ("hasted", "time", a_hasted),
    ("shielded", "frost", a_shielded),
    ("blessed", "radiant", a_blessed),
    ("inspired", "song", a_inspired),
    ("hexed", "eldritch", a_hexed),
    ("warded", "arcane", a_warded),
    ("regenerating", "heal", a_regenerating),
    ("buff", "buff", a_buff),
    ("debuff", "debuff", a_debuff),
    ("downed", "blood", a_downed),
    ("dodging", "weapon", a_dodging),
    ("resistant", "stone", a_resistant),
    ("sanctuary", "radiant", a_sanctuary),
    ("mirror-images", "glass", a_mirror_images),
    ("wild-shape", "nature", a_wild_shape),
    ("helped", "radiant", a_helped),
    ("covered", "stone", a_cover),
    ("riposte", "weapon", a_riposte),
)


#: Every class skill's icon, in 10-classes.md's order: its id, its school, its glyph, and anything
#: laid on the tile under it. The ids are `<class code>-<the skill's name in kebab case>`; this
#: script reads Story/10-classes.md and stops if a skill there has no icon here, or this has one
#: for a skill that isn't there.
SKILLS = (
    ("fighter-strike", "weapon", item_glyph("longsword"), None),
    ("fighter-cleave", "weapon", cleave, None),
    ("fighter-second-wind", "heal", second_wind, None),
    ("fighter-riposte", "weapon", riposte, None),
    ("fighter-fighting-style", "weapon", fighting_style, None),
    ("fighter-action-surge", "fire", action_surge, None),
    ("fighter-indomitable", "weapon", indomitable, None),

    ("rogue-strike", "shadow", item_glyph("dagger"), None),
    ("rogue-backstab", "shadow", backstab, None),
    ("rogue-smoke-step", "shadow", smoke_step, None),
    ("rogue-poisoned-edge", "poison", poisoned_edge, None),
    ("rogue-cunning", "shadow", mask, None),
    ("rogue-shadow-dance", "shadow", shadow_dance, None),
    ("rogue-evasion", "shadow", dodge, dodge_ghosts),

    ("cleric-sacred-flame", "radiant", sacred_flame, sacred_flame_light),
    ("cleric-cure-wounds", "heal", cure_wounds, None),
    ("cleric-bless", "radiant", bless, None),
    ("cleric-guiding-bolt", "radiant", guiding_bolt, None),
    ("cleric-disciple-of-life", "heal", disciple_of_life, None),
    ("cleric-dawnlight", "radiant", dawnlight, None),
    ("cleric-blessed-healer", "heal", blessed_healer, None),

    ("wizard-fire-bolt", "fire", fire_bolt, None),
    ("wizard-magic-missile", "arcane", magic_missile, None),
    ("wizard-burning-hands", "fire", burning_hands, None),
    ("wizard-shield", "arcane", arcane_shield, None),
    ("wizard-arcane-recovery", "arcane", arcane_recovery, None),
    ("wizard-fireball", "fire", fireball, None),
    ("wizard-empowered", "arcane", empowered, None),

    ("paladin-strike", "weapon", mace_strike, None),
    ("paladin-divine-smite", "radiant", divine_smite, None),
    ("paladin-lay-on-hands", "radiant", lay_on_hands, None),
    ("paladin-raise-shield", "weapon", raise_shield, None),
    ("paladin-aura-of-protection", "radiant", protection, None),
    ("paladin-unbroken-oath", "radiant", unbroken_oath, None),
    ("paladin-radiant-strikes", "radiant", radiant_strikes, None),

    ("druid-thorn-whip", "nature", thorn_whip, None),
    ("druid-entangle", "nature", entangle, None),
    ("druid-healing-word", "heal", leaf_heal, None),
    ("druid-moonbeam", "frost", moonbeam, None),
    ("druid-natural-recovery", "nature", natural_recovery, None),
    ("druid-wild-shape-bear", "nature", bear, None),
    ("druid-natures-ward", "nature", natures_ward, None),

    ("warlock-eldritch-blast", "eldritch", eldritch_blast, None),
    ("warlock-hex", "eldritch", hex_, None),
    ("warlock-hellish-rebuke", "fire", hellish_rebuke, None),
    ("warlock-armour-of-shadows", "eldritch", armour_of_shadows, None),
    ("warlock-agonising-blast", "eldritch", agonising_blast, None),
    ("warlock-void-maw", "shadow", void_maw, None),
    ("warlock-devils-sight", "eldritch", devils_sight, None),

    ("bard-vicious-mockery", "song", vicious_mockery, None),
    ("bard-inspiration", "song", bardic_inspiration, None),
    ("bard-healing-word", "heal", healing_word, None),
    ("bard-thunderwave", "storm", thunderwave, None),
    ("bard-song-of-rest", "song", song_of_rest, None),
    ("bard-battle-hymn", "song", horn, None),
    ("bard-countercharm", "song", countercharm, None),

    ("watchblade-cudgel-blow", "weapon", club, None),
    ("watchblade-hold-the-line", "stone", hold_the_line, None),
    ("watchblade-arrest", "weapon", shackle, None),
    ("watchblade-whistle-for-the-watch", "weapon", whistle, None),
    ("watchblade-twenty-years-on-the-beat", "weapon", item_glyph("iron_helm"), None),
    ("watchblade-shield-wall", "stone", shield_wall, None),
    ("watchblade-keeper-of-the-peace", "weapon", keeper_of_the_peace, None),

    ("netcaster-gaff", "sea", gaff, None),
    ("netcaster-cast-the-net", "sea", net, None),
    ("netcaster-mend", "heal", mend, None),
    ("netcaster-haul-in", "sea", haul_in, None),
    ("netcaster-weather-eye", "sea", weather_eye, None),
    ("netcaster-high-tide", "sea", high_tide, None),
    ("netcaster-knots-hold", "sea", knot, None),

    ("deckhand-cutlass", "weapon", item_glyph("scimitar"), None),
    ("deckhand-belaying-pin", "weapon", belaying_pin, None),
    ("deckhand-rum-ration", "heal", rum_ration, None),
    ("deckhand-swing-across", "sea", swing_across, None),
    ("deckhand-sea-legs", "sea", sea_legs, None),
    ("deckhand-broadside", "fire", broadside, None),
    ("deckhand-captains-share", "radiant", captains_share, None),

    ("lodge-ranger-shot", "nature", ranger_shot, None),
    ("lodge-ranger-hunters-mark", "nature", hunters_mark, None),
    ("lodge-ranger-hawk-strike", "nature", bird, None),
    ("lodge-ranger-volley", "nature", volley, None),
    ("lodge-ranger-count-the-ground", "nature", count_the_ground, None),
    ("lodge-ranger-rain-of-arrows", "nature", rain_of_arrows, None),
    ("lodge-ranger-old-growth", "nature", old_growth, None),

    ("battlemage-force-bolt", "arcane", force_bolt, None),
    ("battlemage-ward", "arcane", ward, None),
    ("battlemage-counterspell", "arcane", counterspell, None),
    ("battlemage-chain-lightning", "storm", chain_lightning, None),
    ("battlemage-spire-training", "arcane", spire_training, None),
    ("battlemage-spire-lance", "arcane", spire_lance, None),
    ("battlemage-archmages-patience", "arcane", archmages_patience, None),

    ("ashwalker-ember-edge", "fire", ember_edge, None),
    ("ashwalker-ash-cloud", "stone", ash_cloud, None),
    ("ashwalker-endure", "stone", endure, None),
    ("ashwalker-burn-the-fields", "fire", burn_the_fields, None),
    ("ashwalker-doomward-born", "fire", doomward_born, None),
    ("ashwalker-last-harvest", "fire", last_harvest, None),
    ("ashwalker-nothing-left-to-lose", "blood", nothing_left_to_lose, None),

    ("bulwark-shield-bash", "stone", shield_bash, None),
    ("bulwark-cover", "stone", cover, None),
    ("bulwark-hold", "stone", hold, None),
    ("bulwark-stone-skin", "stone", stone_skin, None),
    ("bulwark-forty-one-names", "stone", forty_one_names, None),
    ("bulwark-nobody-left-behind", "radiant", nobody_left_behind, None),
    ("bulwark-staying-behind", "stone", staying_behind, None),

    ("ember-mender-cinder", "fire", cinder, None),
    ("ember-mender-cauterise", "heal", cauterise, None),
    ("ember-mender-read-the-wound", "heal", read_the_wound, None),
    ("ember-mender-warm-hands", "heal", warm_hands, None),
    ("ember-mender-index-of-remedies", "heal", index_of_remedies, None),
    ("ember-mender-rekindle", "fire", rekindle, None),
    ("ember-mender-notes-in-the-margin", "heal", notes_in_the_margin, None),

    ("lanerunner-twin-cuts", "shadow", twin_cuts, None),
    ("lanerunner-pick-pocket", "shadow", pick_pocket, None),
    ("lanerunner-rooftops", "shadow", rooftops, None),
    ("lanerunner-low-blow", "shadow", low_blow, None),
    ("lanerunner-lane-rules", "shadow", lane_rules, None),
    ("lanerunner-everyones-pocket", "shadow", everyones_pocket, None),
    ("lanerunner-name-on-the-door", "shadow", name_on_the_door, None),

    ("wayfarer-walking-stick", "weapon", walking_stick, None),
    ("wayfarer-throw-anything", "weapon", throw_anything, None),
    ("wayfarer-duchess-kicks", "weapon", duchess_kicks, None),
    ("wayfarer-rummage", "heal", rummage, None),
    ("wayfarer-haggle", "radiant", haggle, None),
    ("wayfarer-lucky-stone", "radiant", lucky_stone, None),
    ("wayfarer-seen-it-all", "weapon", seen_it_all, None),

    ("gilded-knight-morning-strike", "radiant", morning_strike, None),
    ("gilded-knight-sunburst", "radiant", sunburst, None),
    ("gilded-knight-vigil", "radiant", vigil, None),
    ("gilded-knight-call-the-order", "radiant", call_the_order, None),
    ("gilded-knight-where-the-dark-gathers", "radiant", where_the_dark_gathers, None),
    ("gilded-knight-gilded-dawn", "radiant", gilded_dawn, None),
    ("gilded-knight-sorry-somewhere-useful", "radiant", sorry_somewhere_useful, None),

    ("hemomancer-blood-lash", "blood", blood_lash, None),
    ("hemomancer-drain", "blood", drain, None),
    ("hemomancer-blood-shield", "blood", blood_shield, None),
    ("hemomancer-given-freely", "blood", given_freely, None),
    ("hemomancer-the-old-enemy", "blood", the_old_enemy, None),
    ("hemomancer-crimson-tide", "blood", crimson_tide, None),
    ("hemomancer-forty-years-of-manners", "blood", forty_years_of_manners, None),

    ("hivekeeper-swarm", "honey", swarm, None),
    ("hivekeeper-honey-salve", "heal", honey_salve, None),
    ("hivekeeper-smoke-pot", "stone", smoke_pot, None),
    ("hivekeeper-angry-hive", "honey", angry_hive, None),
    ("hivekeeper-thirty-summers", "honey", thirty_summers, None),
    ("hivekeeper-the-whole-hive", "honey", the_whole_hive, None),
    ("hivekeeper-they-know-me", "honey", they_know_me, None),

    ("glasswright-prism-ray", "glass", prism_ray, None),
    ("glasswright-mirror-image", "glass", mirror_image, mirror_image_under),
    ("glasswright-refract", "glass", refract, None),
    ("glasswright-glasshouse", "glass", glasshouse, None),
    ("glasswright-night-flowers", "glass", night_flowers, None),
    ("glasswright-given-in-love", "glass", given_in_love, None),
    ("glasswright-choose-it-yourself", "glass", choose_it_yourself, None),

    ("lamplighter-lantern-swing", "radiant", lantern_swing, None),
    ("lamplighter-sanctuary", "radiant", sanctuary, None),
    ("lamplighter-vespers", "heal", vespers, None),
    ("lamplighter-reveal", "radiant", reveal, None),
    ("lamplighter-everyone-who-knocks", "radiant", everyone_who_knocks, None),
    ("lamplighter-the-bell", "radiant", the_bell, None),
    ("lamplighter-bees-guard-the-honey", "honey", bees_guard_the_honey, None),

    ("crownsworn-riposte-strike", "weapon", riposte_strike, None),
    ("crownsworn-command", "radiant", command, None),
    ("crownsworn-told-rudely", "song", told_rudely, None),
    ("crownsworn-rally", "heal", rally, None),
    ("crownsworn-no-procedure", "weapon", no_procedure, None),
    ("crownsworn-my-fathers-sword", "radiant", my_fathers_sword, None),
    ("crownsworn-somebody-allowed-to-say-no", "weapon", somebody_allowed_to_say_no, None),

    ("hourwarden-second-hand", "time", second_hand, None),
    ("hourwarden-rewind", "time", rewind, None),
    ("hourwarden-haste", "time", haste_glyph, None),
    ("hourwarden-stop", "time", stop, None),
    ("hourwarden-echo-strike", "time", echo_strike, echo_strike_under),
    ("hourwarden-bleed-the-clock", "time", bleed_the_clock, None),
    ("hourwarden-stolen-minute", "time", stolen_minute, None),
)

#: The icons after the class skills on the same sheet: the bar's own (the potion, the elixir, the
#: Soul-Link Strike, Help), the warning a foe's nameplate shows when it has no icon of its own, and
#: the foes' telegraphed moves (11-combat.md §6), as `foe-<move>`.
SKILL_EXTRAS = (
    ("drink-potion", "heal", drink_potion, None),
    ("drink-elixir", "arcane", drink_elixir, None),
    ("soul-link-strike", "soul", soul_link_strike, None),
    ("help", "radiant", help_, None),
    ("auto-attack", "weapon", attack, None),
    ("auto-ranged", "weapon", ranged_attack, None),
    ("telegraph", "foe", telegraph, None),
    ("foe-hook-pull", "foe", hook_pull, None),
    ("foe-trip", "foe", trip, None),
    ("foe-thorn-burst", "foe", thorn_burst, None),
    ("foe-uprooting-charge", "foe", uprooting_charge, None),
    ("foe-feint", "foe", feint, None),
    ("foe-smoke-bomb", "foe", smoke_bomb, None),
    ("foe-ashen-bolt", "foe", ashen_bolt, None),
    ("foe-blasting-charge", "foe", blasting_charge, None),
    ("foe-bite", "foe", bite, None),
    ("foe-claws", "foe", claws, None),
    ("foe-cleave", "foe", cleave, None),
    ("foe-roar", "foe", roar, None),
    ("foe-multiattack", "foe", multiattack, None),
)


def rt_tile(school, passive=False, corner=1):
    """A real-time icon's tile: the old tile in the school's colours, its corners cut `corner`
    pixels deep, and for a passive a gold rim in place of the bevel."""
    bands = RT_SCHOOLS[school]
    n = ICON
    img = new_image(n, n)
    for y in range(n):
        for x in range(n):
            ex, ey = min(x, n - 1 - x), min(y, n - 1 - y)
            if ex + ey < corner:
                continue
            if ex == 0 or ey == 0 or ex + ey == corner:
                img[y][x] = list(INK) + [255]
                continue
            d = math.hypot(x + 0.5 - TILE_LIGHT[0], y + 0.5 - TILE_LIGHT[1])
            band = 3 - sum(1 for edge in TILE_BANDS if d > edge)
            c = bands[band]
            rim = ex == 1 or ey == 1 or ex + ey == corner + 1
            if rim and passive:
                c = PASSIVE_RIM[0] if x + y < n - 1 else PASSIVE_RIM[1]
            elif x == 1 or y == 1:
                c = lerp_color(c, (255, 255, 255), TILE_BEVEL_LIGHT)
            elif x == n - 2 or y == n - 2:
                c = lerp_color(c, (0, 0, 0), TILE_BEVEL_DARK)
            img[y][x] = list(c) + [255]
    if corner == 1:
        for x, y in ((1, 1), (n - 2, 1), (1, n - 2), (n - 2, n - 2)):
            img[y][x] = list(INK) + [255]
    return img


def rt_icon(school, draw, under, passive=False, corner=1):
    """One real-time icon: its tile, anything under the glyph, and the glyph."""
    img = rt_tile(school, passive, corner)
    if under:
        under(img)
    ic = Icon(remap=RT_EXTRA)
    draw(ic)
    rendered = ic.render()
    for y in range(ICON):
        for x in range(ICON):
            if rendered[y][x][3]:
                put(img, x, y, tuple(rendered[y][x]))
    return img


#: The real-time sheets' columns, and where the class kits are.
RT_COLUMNS = 16
CLASSES_DOC = os.path.join(ROOT, "Story", "10-classes.md")

#: A class's heading in 10-classes.md, with its code in backticks, and a row of its kit.
CLASS_HEADING = re.compile(r"^#{3,4} .*?\(`([a-z-]+)`\)")
KIT_ROW = re.compile(r"^\| *(?:1|2|3|4|5|10) *\| *([^|]+?) *\| *([ABRP]) *\|")


def kebab(name):
    """A skill's name as its id spells it: Nature's Ward is natures-ward."""
    return re.sub(r"[^a-z0-9]+", "-", name.lower().replace("'", "").replace("’", "")).strip("-")


def class_kits():
    """Every skill in 10-classes.md, as (id, kind), in its order."""
    skills, code = [], None
    with open(CLASSES_DOC, encoding="utf-8") as f:
        for line in f:
            heading = CLASS_HEADING.match(line)
            if heading:
                code = heading.group(1)
                continue
            row = KIT_ROW.match(line)
            if row and code:
                skills.append(("%s-%s" % (code, kebab(row.group(1))), row.group(2)))
    return skills


def check_kits():
    """Stops with what's different if the skills here aren't 10-classes.md's, one for one; returns
    each skill's kind."""
    kits = class_kits()
    ours = [name for name, _, _, _ in SKILLS]
    theirs = [name for name, _ in kits]
    problems = ["%s has no icon" % n for n in theirs if n not in ours]
    problems += ["%s has an icon but isn't in 10-classes.md" % n for n in ours if n not in theirs]
    names = ours + [n for n, _, _, _ in SKILL_EXTRAS] + [n for n, _, _ in AURAS]
    problems += ["two icons are called %s" % n for n in sorted(set(n for n in names if names.count(n) > 1))]
    if problems:
        sys.exit("generate_combat_art: the real-time icons and Story/10-classes.md don't match, so nothing was written.\n  "
                 + "\n  ".join(problems))
    return dict(kits)


def sheet(cells):
    """Lays rendered icons on a sheet RT_COLUMNS wide, a row at a time."""
    rows = (len(cells) + RT_COLUMNS - 1) // RT_COLUMNS
    img = new_image(RT_COLUMNS * ICON, rows * ICON)
    for i, cell in enumerate(cells):
        paste(img, (i % RT_COLUMNS) * ICON, (i // RT_COLUMNS) * ICON, cell)
    return img


def build_realtime_icons():
    """Combat/skills.png, Combat/auras.png and Combat/icons.json, their index."""
    kinds = check_kits()
    skills = [rt_icon(school, draw, under, kinds.get(name) == "P") for name, school, draw, under in SKILLS + SKILL_EXTRAS]
    auras = [rt_icon(school, draw, None, corner=AURA_CORNER) for _, school, draw in AURAS]
    write_png(os.path.join(OUT, "Combat", "skills.png"), sheet(skills))
    write_png(os.path.join(OUT, "Combat", "auras.png"), sheet(auras))

    index = {
        "format": "LegendQuest hud icons 1",
        "size": ICON,
        "columns": RT_COLUMNS,
        "sheets": [
            {"asset": "Combat/skills", "icons": [name for name, _, _, _ in SKILLS + SKILL_EXTRAS]},
            {"asset": "Combat/auras", "icons": [name for name, _, _ in AURAS]},
        ],
        "passives": [name for name, _, _, _ in SKILLS if kinds.get(name) == "P"],
    }
    path = os.path.join(OUT, "Combat", "icons.json")
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        json.dump(index, f, indent=1)
        f.write("\n")
    print("  Content/Combat/icons.json  (%d skill icons, %d aura icons)" % (len(SKILLS + SKILL_EXTRAS), len(AURAS)))


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
    build_realtime_icons()
    print("Done.")
