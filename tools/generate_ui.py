"""
UI art for Legend Quest: the button plates, the panel the forms sit on, and the marks on the map.
Run it from the repo root:

    python tools/generate_ui.py

The plates and the panel are nine-sliced in the game, so they follow the same rule The Black Box's did: anything that
varies has to be in a corner or vary only along the edge it sits on, and the middle is flat. Same
muted look, with dark iron instead of concrete. Only needs CPython, and it's seeded, so a rerun
writes the same bytes.
"""

import os
import random
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from pixels import OUT, new_image, put, tone, write_png  # noqa: E402


# --------------------------------------------------------------------------- #
# button.png -- iron plate with light in a groove round its edge
# --------------------------------------------------------------------------- #
#
# Two bands on the same grid: the plate on top and a white mask of the groove's light under it,
# which the game tints. Rows: 0 idle (4 frames of a slow smoulder), 1 hover (6, the light climbing
# out of the groove), 2 press (4, bevel inverted), 3 disabled (1).

BUTTON_FRAME = 24
BUTTON_CORNER = 8
BUTTON_COLS = 6
BUTTON_ROWS = 4

#: Groove inset from the border. Has to stay < BUTTON_CORNER or the light breaks at the seams.
BUTTON_GROOVE = 3

#: Per frame: (face brightness, inverted bevel, dead, groove alpha, how far light spreads)
BUTTON_FRAMES = {
    0: [  # idle -- a slow, uneven smoulder
        (1.00, False, False, 46, 1.05),
        (1.00, False, False, 64, 1.25),
        (1.00, False, False, 37, 0.95),
        (1.00, False, False, 55, 1.15),
    ],
    1: [  # hover -- light climbing out of the groove and onto the bevel
        (1.02, False, False, 78, 1.35),
        (1.04, False, False, 116, 1.75),
        (1.06, False, False, 152, 2.15),
        (1.08, False, False, 184, 2.55),
        (1.10, False, False, 208, 2.90),
        (1.11, False, False, 220, 3.10),
    ],
    2: [  # press -- the plate goes in, and whatever is behind it flares
        (0.86, True, False, 255, 3.40),
        (0.82, True, False, 238, 3.05),
        (0.84, True, False, 219, 2.70),
        (0.86, True, False, 210, 2.50),
    ],
    3: [  # disabled
        (0.74, False, True, 0, 0.00),
    ],
}

# Dark iron, a little warm so it sits with the torchlight.
IRON = (46, 42, 41)
IRON_LT = (82, 75, 70)
IRON_DK = (17, 15, 15)
GROOVE = (9, 7, 7)
RIVET = (112, 102, 94)
OUTLINE = (0, 0, 0, 255)


def _button_falloff(distance, spread):
    """Alpha multiplier for a pixel `distance` px off the groove."""
    if spread <= 0.0 or distance >= spread:
        return 0.0
    return pow(1.0 - distance / spread, 1.6)


def _button_plate(img, ox, oy, face_mul, inverted, dead, rng):
    """Draws one frame of the iron plate into the top band."""
    S = BUTTON_FRAME
    C = BUTTON_CORNER

    def shade(rgb, extra=1.0):
        r, g, b = (c * face_mul * extra for c in rgb)
        if dead:
            # Disabled loses its warmth too, so it reads as switched off and not just dark.
            grey = (r + g + b) / 3.0
            r, g, b = (c * 0.35 + grey * 0.65 for c in (r, g, b))
        return tone((r, g, b), 1.0)

    # 1. Face, hammered. The grain has to be constant along whichever axis a region gets
    #    stretched, or the nine-slice smears one row of speckle across the whole button.
    row_grain = [rng.randint(-3, 3) for _ in range(S)]
    col_grain = [rng.randint(-3, 3) for _ in range(S)]
    for y in range(S):
        edge_y = y < C or y >= S - C
        for x in range(S):
            edge_x = x < C or x >= S - C
            if edge_x and edge_y:
                n = rng.randint(-4, 4)
            elif edge_y:
                n = row_grain[y]
            elif edge_x:
                n = col_grain[x]
            else:
                n = 0
            put(img, ox + x, oy + y, shade((IRON[0] + n, IRON[1] + n, IRON[2] + n)))

    hi, lo = (IRON_DK, IRON_LT) if inverted else (IRON_LT, IRON_DK)

    # 2. Hard silhouette, then two rings of bevel. The low side goes down first, so the top-left
    #    corner ends up lit and the bottom-right one doesn't.
    for i in range(S):
        put(img, ox + i, oy, OUTLINE)
        put(img, ox + i, oy + S - 1, OUTLINE)
        put(img, ox, oy + i, OUTLINE)
        put(img, ox + S - 1, oy + i, OUTLINE)

    for i in (2, 1):
        f = 1.0 - (i - 1) * 0.35
        for x in range(i, S - i):
            put(img, ox + x, oy + S - 1 - i, shade(lo, f))
            put(img, ox + x, oy + i, shade(hi, f))
        for y in range(i, S - i):
            put(img, ox + S - 1 - i, oy + y, shade(lo, f * 0.90))
            put(img, ox + i, oy + y, shade(hi, f * 0.85))

    # 3. The groove, cut all the way round at a fixed inset so the nine-slice can stretch it.
    g = BUTTON_GROOVE
    for x in range(g, S - g):
        put(img, ox + x, oy + g, shade(GROOVE))
        put(img, ox + x, oy + S - 1 - g, shade(GROOVE))
    for y in range(g, S - g):
        put(img, ox + g, oy + y, shade(GROOVE))
        put(img, ox + S - 1 - g, oy + y, shade(GROOVE))

    # 4. A round rivet in each corner, inside the groove: lit on top, a shadow under it.
    for bx, by in ((g + 2, g + 2), (S - 1 - g - 3, g + 2), (g + 2, S - 1 - g - 3), (S - 1 - g - 3, S - 1 - g - 3)):
        put(img, ox + bx, oy + by, shade(RIVET, 1.15))
        put(img, ox + bx + 1, oy + by, shade(RIVET, 0.85))
        put(img, ox + bx, oy + by + 1, shade(RIVET, 0.80))
        put(img, ox + bx + 1, oy + by + 1, shade(IRON_DK))


def _button_accent(img, ox, oy, alpha, spread):
    """Draws one frame of the light mask into the bottom band, white so it can be tinted."""
    if alpha <= 0:
        return
    S = BUTTON_FRAME
    for y in range(S):
        for x in range(S):
            # Distance in from the nearest border; the groove is a constant inset so this slices cleanly.
            inset = min(x, S - 1 - x, y, S - 1 - y)
            a = int(alpha * _button_falloff(abs(inset - BUTTON_GROOVE), spread))
            if a > 0:
                put(img, ox + x, oy + y, (255, 255, 255, min(255, a)))


def build_button(seed=3301):
    band = BUTTON_ROWS * BUTTON_FRAME
    img = new_image(BUTTON_COLS * BUTTON_FRAME, band * 2)

    for row, frames in BUTTON_FRAMES.items():
        for col, (face_mul, inverted, dead, alpha, spread) in enumerate(frames):
            # One seed per frame, so the hammering doesn't crawl between frames.
            rng = random.Random(seed + row * 100 + col)
            ox, oy = col * BUTTON_FRAME, row * BUTTON_FRAME
            _button_plate(img, ox, oy, face_mul, inverted, dead, rng)
            _button_accent(img, ox, oy + band, alpha, spread)

    write_png(os.path.join(OUT, "button.png"), img)


# --------------------------------------------------------------------------- #
# panel.png -- the nine-sliceable iron slab every form is built on
# --------------------------------------------------------------------------- #
#
# One frame. A form doesn't react to the cursor, the controls on it do. Same iron as the buttons, a
# shade darker and hollowed out, with a thin line of old bronze inside the lip.

PANEL_FRAME = 32

#: Fixed corner of the nine-slice. Has to stay bigger than PANEL_WELL or the well's lip shears.
PANEL_CORNER = 12

#: How far in from the border the recessed well is cut.
PANEL_WELL = 7

#: How far in from the border the bronze line runs. Between the rivets and the well.
PANEL_TRIM = 5

#: How far in from the border the corner rivets sit, just inside the bevel.
PANEL_RIVET = 3

SHELL = (34, 31, 30)
SHELL_LT = (66, 60, 56)
SHELL_DK = (13, 11, 11)
WELL = (17, 15, 15)
WELL_LT = (35, 31, 30)
BRONZE = (92, 72, 44)


def build_panel(seed=4472):
    S = PANEL_FRAME
    C = PANEL_CORNER
    rng = random.Random(seed)
    img = new_image(S, S)

    # 1. Face. Same stretch-safe grain rule as the button.
    row_grain = [rng.randint(-2, 2) for _ in range(S)]
    col_grain = [rng.randint(-2, 2) for _ in range(S)]
    for y in range(S):
        edge_y = y < C or y >= S - C
        for x in range(S):
            edge_x = x < C or x >= S - C
            if edge_x and edge_y:
                n = rng.randint(-2, 2)
            elif edge_y:
                n = row_grain[y]
            elif edge_x:
                n = col_grain[x]
            else:
                n = 0
            put(img, x, y, tone((SHELL[0] + n, SHELL[1] + n, SHELL[2] + n), 1.0))

    # 2. Silhouette and the outer bevel, lit from the top-left like everything else.
    for i in range(S):
        put(img, i, 0, OUTLINE)
        put(img, i, S - 1, OUTLINE)
        put(img, 0, i, OUTLINE)
        put(img, S - 1, i, OUTLINE)

    for i in (2, 1):
        f = 1.0 - (i - 1) * 0.40
        for x in range(i, S - i):
            put(img, x, i, tone(SHELL_LT, f))
            put(img, x, S - 1 - i, tone(SHELL_DK, f))
        for y in range(i, S - i):
            put(img, i, y, tone(SHELL_LT, f * 0.85))
            put(img, S - 1 - i, y, tone(SHELL_DK, f * 0.90))

    # 3. The bronze line, lit along the top and left, dull along the bottom and right.
    t = PANEL_TRIM
    for x in range(t, S - t):
        put(img, x, t, tone(BRONZE, 1.15))
        put(img, x, S - 1 - t, tone(BRONZE, 0.70))
    for y in range(t, S - t):
        put(img, t, y, tone(BRONZE, 1.0))
        put(img, S - 1 - t, y, tone(BRONZE, 0.75))

    # 4. The well: the slab hollowed out, so the lighting inverts (far lip lit, near lip in shadow).
    w = PANEL_WELL
    for y in range(w, S - w):
        for x in range(w, S - w):
            put(img, x, y, tone(WELL, 1.0))
    for x in range(w, S - w):
        put(img, x, w, tone(SHELL_DK, 1.0))
        put(img, x, S - 1 - w, tone(WELL_LT, 1.0))
    for y in range(w, S - w):
        put(img, w, y, tone(SHELL_DK, 0.90))
        put(img, S - 1 - w, y, tone(WELL_LT, 0.85))

    # 5. Rivets in the corners of the lip, outside the bronze line.
    b = PANEL_RIVET
    for bx, by in ((b, b), (S - 1 - b - 1, b), (b, S - 1 - b - 1), (S - 1 - b - 1, S - 1 - b - 1)):
        put(img, bx, by, tone(RIVET, 1.1))
        put(img, bx + 1, by, tone(RIVET, 0.8))
        put(img, bx, by + 1, tone(RIVET, 0.75))
        put(img, bx + 1, by + 1, tone(SHELL_DK, 1.0))

    write_png(os.path.join(OUT, "panel.png"), img)


# --------------------------------------------------------------------------- #
# map-marks.png -- the marks on the map screen
# --------------------------------------------------------------------------- #
#
# One row of 12x12 marks, blown up 3x in the game: a place you've been, a place you know of but
# haven't been, the ring round the place you are, a waypoint stone, the brackets round the place
# you've picked to travel to, you on the local map, and one dot of a road.

MARK = 12
MARK_BONE = (222, 212, 198)
MARK_DIM = (120, 110, 100)
MARK_TORCH = (232, 150, 58)
MARK_STONE = (150, 146, 140)
MARK_RUNE = (170, 214, 236)
MARK_INK = (14, 11, 10)


def _mark_place(img, ox, filled):
    """A lozenge, lit on its upper left. Hollow for a place you only know of."""
    c = 5.5
    for y in range(MARK):
        for x in range(MARK):
            d = abs(x - c) + abs(y - c)
            if d > 4.6:
                continue
            if d > 3.6:
                put(img, ox + x, y, MARK_INK + (255,))
            elif filled:
                lit = (x - c) + (y - c) < -0.5
                put(img, ox + x, y, tone(MARK_BONE, 1.0 if lit else 0.74))
            elif d > 2.6:
                put(img, ox + x, y, tone(MARK_DIM, 1.0))


def _mark_ring(img, ox):
    """A ring round a place, in torchlight, for where you are."""
    for y in range(MARK):
        for x in range(MARK):
            d = ((x - 5.5) ** 2 + (y - 5.5) ** 2) ** 0.5
            if 5.0 < d < 6.0:
                put(img, ox + x, y, tone(MARK_TORCH, 1.1 if x + y < 11 else 0.8))
            elif 4.3 < d <= 5.0:
                put(img, ox + x, y, MARK_INK + (200,))


def _mark_waypoint(img, ox):
    """A standing stone with its mark lit."""
    for y in range(1, 11):
        for x in range(4, 8):
            put(img, ox + x, y, tone(MARK_STONE, 1.15 if x == 4 else 0.7 if x == 7 else 0.95))
    for y in range(3, 9):
        put(img, ox + 5, y, tone(MARK_RUNE, 1.0))
    for x in range(3, 9):
        put(img, ox + x, 0, MARK_INK + (255,))
        put(img, ox + x, 11, MARK_INK + (255,))
    for y in range(1, 11):
        put(img, ox + 3, y, MARK_INK + (255,))
        put(img, ox + 8, y, MARK_INK + (255,))


def _mark_brackets(img, ox):
    """Four corners round a place, for the one picked to travel to."""
    for (x0, y0, sx, sy) in ((0, 0, 1, 1), (11, 0, -1, 1), (0, 11, 1, -1), (11, 11, -1, -1)):
        for i in range(3):
            put(img, ox + x0 + sx * i, y0, tone(MARK_TORCH, 1.1))
            put(img, ox + x0, y0 + sy * i, tone(MARK_TORCH, 1.1))


def _mark_you(img, ox):
    """You, on the local map: a dot of torchlight with a dark edge and a highlight."""
    for y in range(MARK):
        for x in range(MARK):
            d = ((x - 5.5) ** 2 + (y - 5.5) ** 2) ** 0.5
            if d < 2.6:
                put(img, ox + x, y, tone(MARK_TORCH, 1.25 if (x, y) == (4, 4) else 1.0))
            elif d < 3.6:
                put(img, ox + x, y, MARK_INK + (255,))


def _mark_dot(img, ox):
    """One dot of a road between places."""
    for y in (5, 6):
        for x in (5, 6):
            put(img, ox + x, y, tone(MARK_BONE, 0.8 if (x, y) == (6, 6) else 0.95))


def build_map_marks():
    img = new_image(MARK * 7, MARK)
    _mark_place(img, 0, True)
    _mark_place(img, MARK, False)
    _mark_ring(img, MARK * 2)
    _mark_waypoint(img, MARK * 3)
    _mark_brackets(img, MARK * 4)
    _mark_you(img, MARK * 5)
    _mark_dot(img, MARK * 6)
    write_png(os.path.join(OUT, "map-marks.png"), img)


if __name__ == "__main__":
    print("Generating UI for Legend Quest...")
    build_button()
    build_panel()
    build_map_marks()
    print("Done.")
