"""
UI art for Legend Quest: the button plates, the panel the forms sit on, the marks on the map, the
"!" markers over people, the mouse cursors, the badges, chevrons and glints that mark a room's ways
out and the things in it, and the key caps the tutorial tips show.
Run it from the repo root:

    python tools/generate_ui.py

The plates and the panel are nine-sliced in the game, so they follow the same rule The Black Box's did: anything that
varies has to be in a corner or vary only along the edge it sits on, and the middle is flat. Same
muted look, with dark iron instead of concrete. Only needs CPython, and it's seeded, so a rerun
writes the same bytes.
"""

import math
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

# Twilight blue enamel with gold studs, the colours of the title's banners, so the menus sit with
# the bright art instead of the old dark iron.
IRON = (44, 54, 96)
IRON_LT = (86, 104, 164)
IRON_DK = (18, 22, 46)
GROOVE = (10, 12, 28)
RIVET = (222, 182, 98)
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
# One frame. A form doesn't react to the cursor, the controls on it do. Same enamel as the buttons, a
# shade darker and hollowed out, with a thin line of gold inside the lip.

PANEL_FRAME = 32

#: Fixed corner of the nine-slice. Has to stay bigger than PANEL_WELL or the well's lip shears.
PANEL_CORNER = 12

#: How far in from the border the recessed well is cut.
PANEL_WELL = 7

#: How far in from the border the bronze line runs. Between the rivets and the well.
PANEL_TRIM = 5

#: How far in from the border the corner rivets sit, just inside the bevel.
PANEL_RIVET = 3

SHELL = (38, 46, 84)
SHELL_LT = (80, 96, 152)
SHELL_DK = (14, 16, 36)
WELL = (20, 24, 50)
WELL_LT = (34, 40, 76)
BRONZE = (206, 162, 82)


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


# --------------------------------------------------------------------------- #
# markers.png -- the "!" over someone with something for you, and the arrows to one off the screen
# --------------------------------------------------------------------------- #
#
# Two rows of 14x14 cells, drawn at 3x out on the map like everything else: gold for someone with
# work or a quest for you, red for an event. In each row, the "!" (8 wide, in the middle of its
# cell: a tall bar that narrows, and a round dot under it), then the arrow that sits at the edge of
# the screen pointing right, then the same arrow pointing down and right. The game turns the arrows a quarter at a time for the other six ways,
# which keeps their pixels square. The "!" is lit from the top left like the rest of the art, with
# a bright streak, a glint and a little sparkle off its shoulder. The arrows are lit down their
# middle instead, so they look the same whichever way they're turned.

MARKER = 14
BANG_WIDTH = 8

#: The "!" itself: '#' is filled, and the dark outline goes round whatever is.
BANG = (
    "........",
    "...##...",
    "..####..",
    "..####..",
    "..####..",
    "..####..",
    "...##...",
    "...##...",
    "........",
    "........",
    "..####..",
    "..####..",
    "...##...",
    "........",
)

#: Deep shadow, shadow, base, light, highlight, glint. The gold is the style sheet's gold row; the
#: red is its crimson pushed brighter, so it reads as a warning next to the roofs' pinks.
MARKER_GOLD = ((167, 101, 58), (207, 140, 69), (238, 183, 87), (250, 216, 139), (253, 240, 204), (255, 253, 244))
MARKER_RED = ((116, 28, 50), (172, 44, 64), (220, 72, 80), (246, 134, 124), (255, 210, 198), (255, 248, 242))

#: Dark outlines, not black: a warm brown round the gold, a plum round the red.
MARKER_GOLD_EDGE = (70, 38, 30)
MARKER_RED_EDGE = (62, 18, 38)

#: The arrow, pointing right, round the middle of its cell: a dart with a notch in its back.
ARROW = ((5.6, 0.0), (-4.2, 5.0), (-1.4, 0.0), (-4.2, -5.0))

#: The diagonal arrow is the same dart turned an eighth, and a shade smaller so it still fits its cell.
ARROW_DIAGONAL_SCALE = 0.86


def _outline(img, ox, oy, mask, edge):
    """Puts the dark outline on every empty pixel of a cell next to (not across a corner from) a filled one."""
    for y in range(MARKER):
        for x in range(MARKER):
            if mask[y][x]:
                continue
            if any(0 <= x + dx < MARKER and 0 <= y + dy < MARKER and mask[y + dy][x + dx]
                   for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1))):
                put(img, ox + x, oy + y, edge + (255,))


def _marker_bang(img, oy, ramp, edge):
    """The "!", lit from the top left: light down its left side, shadow down its right, a bright
    streak and a glint near the top, and a four-pointed sparkle off its right shoulder."""
    left = (MARKER - BANG_WIDTH) // 2
    mask = [[False] * MARKER for _ in range(MARKER)]
    for y, row in enumerate(BANG):
        for x, c in enumerate(row):
            mask[y][left + x] = c == "#"

    for y in range(MARKER):
        filled = [x for x in range(MARKER) if mask[y][x]]
        if not filled:
            continue
        # The top row of the bar or the dot catches the light; the bottom row of each is in shadow.
        first, last = filled[0], filled[-1]
        top = not any(mask[y - 1])
        bottom = y + 1 >= MARKER or not any(mask[y + 1])
        for x in filled:
            shade = 3 if x == first else 1 if x == last else 2
            if top:
                shade = min(shade + 1, 3)
            if bottom:
                shade = max(shade - 1, 0)
            put(img, x, oy + y, tone(ramp[shade], 1.0))

    # The streak down the bar, just in from its lit side as it narrows, the glint at the top of it,
    # and a fleck of the same on the dot.
    for y in range(2, 6):
        first = BANG[y].index("#")
        put(img, left + first + 1, oy + y, tone(ramp[4], 1.0))
    put(img, left + 2, oy + 2, tone(ramp[5], 1.0))
    put(img, left + 3, oy + 11, tone(ramp[4], 1.0))

    _outline(img, 0, oy, mask, edge)

    # The sparkle: a bright middle and four soft points, off the top right of the bar.
    sx, sy = left + BANG_WIDTH + 1, oy + 1
    put(img, sx, sy, tone(ramp[5], 1.0))
    for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
        put(img, sx + dx, sy + dy, ramp[4] + (150,))


def _marker_arrow(img, ox, oy, ramp, edge, turn, scale):
    """The arrow, turned `turn` radians clockwise and scaled, lit along its middle: brightest down
    the spine, a glint just behind the point, darker toward the edges, and the dark outline round it."""
    c = (MARKER - 1) / 2.0
    cos_t, sin_t = math.cos(turn), math.sin(turn)
    points = [(c + scale * (x * cos_t - y * sin_t), c + scale * (x * sin_t + y * cos_t)) for x, y in ARROW]

    mask = [[_inside_poly(x + 0.5, y + 0.5, points) for x in range(MARKER)] for y in range(MARKER)]
    for y in range(MARKER):
        for x in range(MARKER):
            if not mask[y][x]:
                continue
            # Where the pixel is along the arrow (u, toward the point) and out from its spine (v).
            px, py = x + 0.5 - c, y + 0.5 - c
            u = (px * cos_t + py * sin_t) / scale
            v = abs(-px * sin_t + py * cos_t) / scale
            shade = 3 if v < 1.1 else 2 if v < 2.6 else 1
            if 1.0 < u < 2.6 and v < 1.1:
                shade = 4
            put(img, ox + x, oy + y, tone(ramp[shade], 1.0))
    _outline(img, ox, oy, mask, edge)


def build_markers():
    img = new_image(MARKER * 3, MARKER * 2)
    for row, (ramp, edge) in enumerate(((MARKER_GOLD, MARKER_GOLD_EDGE), (MARKER_RED, MARKER_RED_EDGE))):
        oy = row * MARKER
        _marker_bang(img, oy, ramp, edge)
        _marker_arrow(img, MARKER, oy, ramp, edge, 0.0, 1.0)
        _marker_arrow(img, MARKER * 2, oy, ramp, edge, math.pi / 4, ARROW_DIAGONAL_SCALE)
    write_png(os.path.join(OUT, "markers.png"), img)


# --------------------------------------------------------------------------- #
# cursors.png -- the mouse cursor's shapes
# --------------------------------------------------------------------------- #
#
# One row of 16x16 frames, blown up 2x in the game, in CursorShape order: an arrow, a ring for
# walking, a hand for using something, a speech bubble for talking, a crossed circle where there's
# no way, and an arched door with its knob for a door in a room. Each is filled, lit on its upper
# left, and has a dark line all the way round so it shows on grass, stone and the dark alike.

CURSOR = 16
CURSOR_BONE = (226, 216, 200)
CURSOR_TORCH = (232, 150, 58)
CURSOR_EMBER = (214, 64, 42)
CURSOR_INK = (14, 11, 10)


def _inside_poly(x, y, points):
    inside = False
    for (ax, ay), (bx, by) in zip(points, points[1:] + points[:1]):
        if (ay > y) != (by > y) and x < ax + (y - ay) * (bx - ax) / (by - ay):
            inside = not inside
    return inside


def _cursor_frame(img, ox, filled, colour):
    """Fills a frame from a test of each pixel's middle, shades it from the top left, and puts the dark line round it."""
    mask = [[filled(x + 0.5, y + 0.5) for x in range(CURSOR)] for y in range(CURSOR)]
    for y in range(CURSOR):
        for x in range(CURSOR):
            if mask[y][x]:
                lit = (x + y) < 12
                put(img, ox + x, y, tone(colour, 1.08 if lit else 0.86))
    for y in range(CURSOR):
        for x in range(CURSOR):
            if mask[y][x]:
                continue
            near = any(0 <= x + dx < CURSOR and 0 <= y + dy < CURSOR and mask[y + dy][x + dx]
                       for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)))
            if near:
                put(img, ox + x, y, CURSOR_INK + (255,))


def build_cursors():
    img = new_image(CURSOR * 6, CURSOR)

    # The arrow, its tip at (1, 1).
    arrow = [(1.0, 1.0), (1.0, 13.0), (4.2, 10.2), (6.8, 15.0), (9.0, 14.0), (6.6, 9.4), (10.6, 9.4)]
    _cursor_frame(img, 0, lambda x, y: _inside_poly(x, y, arrow), CURSOR_BONE)

    # A ring with a dot in it: walk here.
    def ring(x, y):
        d = ((x - 8) ** 2 + (y - 8) ** 2) ** 0.5
        return 4.6 < d < 6.6 or d < 1.6
    _cursor_frame(img, CURSOR, ring, CURSOR_BONE)

    # A hand, one finger up: use it.
    def hand(x, y):
        finger = 6.0 < x < 8.6 and 1.5 < y < 9.0
        knuckles = 6.0 < x < 12.5 and 7.5 < y < 10.5
        palm = 5.0 < x < 12.5 and 9.5 < y < 14.5
        thumb = 3.0 < x < 6.5 and 9.0 < y < 11.5
        return finger or knuckles or palm or thumb
    _cursor_frame(img, CURSOR * 2, hand, CURSOR_BONE)

    # A speech bubble with three dots in it: talk.
    def bubble(x, y):
        body = ((x - 8) / 6.6) ** 2 + ((y - 6.5) / 4.8) ** 2 < 1.0
        tail = _inside_poly(x, y, [(4.0, 9.0), (8.0, 10.0), (3.0, 14.5)])
        dots = 5.5 < y < 7.5 and any(abs(x - cx) < 1.0 for cx in (5.0, 8.0, 11.0))
        return (body or tail) and not dots
    _cursor_frame(img, CURSOR * 3, bubble, CURSOR_TORCH)

    # A circle crossed through: no way there.
    def no_way(x, y):
        d = ((x - 8) ** 2 + (y - 8) ** 2) ** 0.5
        slash = abs((x - 8) - (y - 8)) < 1.3 and d < 5.5
        return 4.8 < d < 6.8 or slash
    _cursor_frame(img, CURSOR * 4, no_way, CURSOR_EMBER)

    # An arched door, the knob a hole the dark line goes round: go in.
    def door(x, y):
        leaf = 3.6 < x < 12.4 and 6.0 < y < 15.0
        arch = math.hypot(x - 8.0, y - 6.0) < 4.4 and y <= 6.0
        knob = math.hypot(x - 10.4, y - 10.6) < 0.9
        return (leaf or arch) and not knob
    _cursor_frame(img, CURSOR * 5, door, CURSOR_TORCH)

    write_png(os.path.join(OUT, "cursors.png"), img)


# --------------------------------------------------------------------------- #
# indicators.png -- what makes a room readable: badges, chevrons, glints, plaques
# --------------------------------------------------------------------------- #
#
# Four rows of 18x18 cells. Row 0 is the badges in BadgeKind order, each a round twilight-navy
# medallion with a rim and an emblem in its colour (gold for trade and the palace, amber for an inn,
# blue for the trainers, the Guild and the Watch, white for temples, homes, gates and stairs), then
# the padlock that's laid over a locked badge's lower right, and the compass that goes beside a way
# out to somewhere else on the world map. Row 1 is the same badges greyed, for a locked door. Row 2
# is the chevrons: east and west for the ends of a room, up and down for a stair; then four frames of
# a glint twinkling over something you can use; then the plaque a badge's name sits on and the soft
# outline round a doorway under the mouse, both nine-sliced, the outline white so the game tints it.
# Row 3 is the chevrons greyed and the plaque greyed, for a way that's locked. The game draws it all
# at 3x like the rest of the UI, and a badge beside a name, or the compass beside a label, at 2x.
# The emblems are lit from the top left like everything else and drop a shadow onto the enamel; the
# chevrons light whichever arm faces the top left, so the west one is lit the same way as the east.

INDICATOR = 18

#: The medallion's rings, by distance from the cell's middle: the dark edge, the rim, the enamel.
BADGE_OUTER = 9.0
BADGE_RIM = 8.0
BADGE_ENAMEL = 6.6

#: The columns of row 0 after the badges, and of row 2: the chevrons, the glint's frames, the plaque, the outline.
PADLOCK_COLUMN = 13
COMPASS_COLUMN = 14
CHEVRON_COLUMNS = ("east", "west", "up", "down")
GLINT_COLUMN = 4
PLAQUE_COLUMN = 8
DOOR_OUTLINE_COLUMN = 9

#: The plaque's and the outline's nine-slices: each is a PLAQUE-square frame in the top left of its cell.
#: Have to match PlaqueFrame and PlaqueCorner in World/Rooms/IndicatorLayer.cs.
PLAQUE = 12
PLAQUE_CORNER = 4

#: Deep shadow, shadow, base, light, highlight. The gold is the markers' gold, the amber the cursor's
#: torchlight, the blue the title's banners lit, the white an ivory with a little lilac in its shadow.
BADGE_GOLD = MARKER_GOLD[:5]
BADGE_AMBER = ((150, 70, 34), (198, 102, 42), (232, 150, 58), (248, 192, 112), (254, 230, 184))
BADGE_BLUE = ((44, 74, 150), (66, 110, 196), (104, 156, 230), (156, 198, 248), (220, 236, 255))
BADGE_WHITE = ((132, 128, 146), (178, 174, 188), (218, 214, 224), (242, 240, 246), (255, 255, 255))
BADGE_GREY = ((70, 70, 78), (96, 96, 104), (126, 126, 134), (152, 152, 158), (178, 178, 184))
BADGE_STEEL = ((54, 58, 78), (92, 98, 124), (140, 148, 172), (190, 196, 214), (228, 232, 242))
BADGE_RED = MARKER_RED[:5]

#: The enamel inside the rim, lit, base and in shadow: the buttons' navy, and a slate for a locked one.
ENAMEL = (IRON_LT, IRON, IRON_DK)
ENAMEL_GREY = ((84, 86, 96), (52, 54, 62), (28, 29, 36))

#: The dark edge round a medallion, a chevron and the padlock.
BADGE_INK = (12, 14, 30)
CHEVRON_EDGE = MARKER_GOLD_EDGE
CHEVRON_EDGE_GREY = (30, 30, 36)

#: The glint's white middle and its gold-white arms.
GLINT_CORE = (255, 252, 240)
GLINT_ARM = (253, 240, 204)


def _cell_mask(inside):
    """Which pixels of a cell a shape fills, testing each pixel's middle."""
    return [[inside(x + 0.5, y + 0.5) for x in range(INDICATOR)] for y in range(INDICATOR)]


def _filled(mask, x, y):
    return 0 <= x < INDICATOR and 0 <= y < INDICATOR and mask[y][x]


def _shade_mask(img, ox, oy, mask, ramp, marks=None):
    """Fills a mask from a ramp, lit from the top left: a pixel with nothing over it or left of it
    catches the light, one with nothing under it or right of it is in shadow, and both at once is
    the highlight. marks(x, y) can name a tone of its own for a pixel (a band, a seam, a knob)."""
    for y in range(INDICATOR):
        for x in range(INDICATOR):
            if not mask[y][x]:
                continue
            lit = not _filled(mask, x, y - 1) or not _filled(mask, x - 1, y)
            dim = not _filled(mask, x, y + 1) or not _filled(mask, x + 1, y)
            shade = 4 if lit and not _filled(mask, x, y - 1) and not _filled(mask, x - 1, y) else 3 if lit else 1 if dim else 2
            if marks is not None:
                own = marks(x + 0.5, y + 0.5)
                if own is not None:
                    shade = own
            put(img, ox + x, oy + y, tone(ramp[shade], 1.0))


def _outline_mask(img, ox, oy, mask, edge):
    """The dark edge on every empty pixel beside (not across a corner from) a filled one."""
    for y in range(INDICATOR):
        for x in range(INDICATOR):
            if not mask[y][x] and any(_filled(mask, x + dx, y + dy) for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1))):
                put(img, ox + x, oy + y, edge + (255,))


def _medallion(img, ox, oy, ramp, enamel):
    """The round badge behind an emblem: the dark edge, the rim lit round its top left, and the
    enamel, recessed, so its top-left lip is in the rim's shadow and its bottom-right lip catches light."""
    c = INDICATOR / 2.0
    lit_e, base_e, dark_e = enamel
    for y in range(INDICATOR):
        for x in range(INDICATOR):
            dx, dy = x + 0.5 - c, y + 0.5 - c
            d = math.hypot(dx, dy)
            facing = dx + dy
            if d > BADGE_OUTER:
                continue
            if d > BADGE_RIM:
                put(img, ox + x, oy + y, BADGE_INK + (255,))
            elif d > BADGE_ENAMEL:
                shade = 4 if facing < -7.0 else 3 if facing < -2.0 else 2 if facing < 4.0 else 1
                put(img, ox + x, oy + y, tone(ramp[shade], 1.0))
            elif d > BADGE_ENAMEL - 1.0 and facing < -1.5:
                put(img, ox + x, oy + y, tone(dark_e, 1.0))
            elif d > BADGE_ENAMEL - 1.0 and facing > 3.0:
                put(img, ox + x, oy + y, tone(lit_e, 0.85))
            else:
                put(img, ox + x, oy + y, tone(base_e, 1.06 if facing < -3.0 else 1.0))


def _emblem(img, ox, oy, inside, ramp, enamel, marks=None):
    """An emblem on the enamel: its shadow dropped a pixel down and right, then the emblem shaded over it."""
    mask = _cell_mask(inside)
    for y in range(INDICATOR):
        for x in range(INDICATOR):
            if mask[y][x] and not _filled(mask, x + 1, y + 1):
                put(img, ox + x + 1, oy + y + 1, tone(enamel[2], 1.0))
    _shade_mask(img, ox, oy, mask, ramp, marks)


def _purse(x, y):
    body = ((x - 9.0) / 4.6) ** 2 + ((y - 11.2) / 3.6) ** 2 < 1.0
    neck = 7.4 < x < 10.6 and 6.6 < y < 8.6
    frill = _inside_poly(x, y, [(5.0, 3.4), (7.0, 4.6), (9.0, 3.4), (11.0, 4.6), (13.0, 3.4), (10.8, 6.8), (7.2, 6.8)])
    return body or neck or frill


def _purse_marks(x, y):
    """The cord round its neck, and a coin's glint on its belly."""
    if 7.4 < x < 10.6 and 7.0 < y < 8.0:
        return 0
    if math.hypot(x - 9.0, y - 11.5) < 1.2:
        return 4
    return None


def _anvil(x, y):
    face = 4.6 < x < 14.0 and 5.6 < y < 8.2
    horn = _inside_poly(x, y, [(3.4, 5.8), (4.8, 5.6), (4.8, 8.2)])
    waist = 7.4 < x < 11.2 and 8.0 < y < 11.2
    foot = 5.4 < x < 13.2 and 11.0 < y < 13.6
    return face or horn or waist or foot


def _bottle(x, y):
    cork = 7.6 < x < 10.4 and 3.2 < y < 5.0
    neck = 8.0 < x < 10.0 and 4.8 < y < 7.6
    body = math.hypot(x - 9.0, y - 10.9) < 3.9
    return cork or neck or body


def _bottle_marks(x, y):
    """The cork darker than the glass, and a streak of light down the glass."""
    if y < 5.0:
        return 1
    if 6.0 < x < 7.0 and 9.0 < y < 12.0:
        return 4
    return None


def _tankard(x, y):
    body = 4.8 < x < 11.4 and 5.8 < y < 14.0
    handle = 1.0 < math.hypot(x - 11.6, y - 9.6) < 2.6 and x > 11.0
    foam = (4.6 < x < 11.6 and 5.2 < y < 6.6) or any(math.hypot(x - cx, y - cy) < 1.4 for cx, cy in ((6.0, 5.2), (8.2, 4.8), (10.4, 5.2)))
    return body or handle or foam


def _tankard_marks(x, y):
    """The foam, pale, and the two hoops round the body."""
    if y < 6.6 and x < 11.6:
        return 4
    if 4.8 < x < 11.4 and (7.8 < y < 8.6 or 12.0 < y < 12.8):
        return 1
    return None


def _swords(x, y):
    for tx, ty, px, py in ((4.8, 4.8, 12.8, 12.8), (13.2, 4.8, 5.2, 12.8)):
        length = math.hypot(px - tx, py - ty)
        ux, uy = (px - tx) / length, (py - ty) / length
        t = (x - tx) * ux + (y - ty) * uy
        v = abs(-(x - tx) * uy + (y - ty) * ux)
        guard = length * 0.62
        if 0.0 <= t <= guard and v < 0.95 * min(1.0, (t + 0.3) / 1.6):
            return True
        if guard <= t <= guard + 1.1 and v < 2.6:
            return True
        if guard + 1.1 <= t <= length - 0.6 and v < 0.7:
            return True
        if math.hypot(x - px, y - py) < 1.15:
            return True
    return False


def _scroll(x, y):
    sheet = 5.6 < x < 12.4 and 5.0 < y < 13.0
    rolls = 4.6 < x < 13.4 and (4.0 < y < 5.8 or 12.2 < y < 13.8)
    return sheet or rolls


def _scroll_marks(x, y):
    """Two lines of writing, and the rolls' ends in shadow."""
    if 7.0 < x < 11.0 and (8.0 < y < 9.0 or 10.0 < y < 11.0):
        return 1
    if (4.0 < y < 5.8 or 12.2 < y < 13.8) and (x < 6.0 or x > 12.0):
        return 1
    return None


def _shield(x, y):
    return _inside_poly(x, y, [(4.6, 4.4), (13.4, 4.4), (13.4, 9.0), (9.0, 14.2), (4.6, 9.0)])


def _shield_marks(x, y):
    """A pale cross on its face."""
    if 8.0 < x < 10.0 and 5.4 < y < 12.6 or 5.6 < x < 12.4 and 7.0 < y < 9.0:
        return 3
    return None


def _sun(x, y):
    """A round sun with a gap round it, and eight rays tapering out from that: the four straight ones long, the diagonals short."""
    dx, dy = x - 9.0, y - 9.0
    d = math.hypot(dx, dy)
    if d < 3.0:
        return True
    k = round(math.atan2(dy, dx) / (math.pi / 4))
    off = abs(math.atan2(dy, dx) - k * math.pi / 4) * d
    if k % 2 == 0:
        return 3.8 < d < 6.4 and off < 1.3 * (6.4 - d) / 2.6 + 0.2
    return 3.8 < d < 5.6 and off < 0.9 * (5.6 - d) / 1.8 + 0.3


def _sun_marks(x, y):
    """A bright middle."""
    return 4 if math.hypot(x - 8.5, y - 8.5) < 1.2 else None


def _crown(x, y):
    band = 4.6 < x < 13.4 and 10.6 < y < 13.2
    points = _inside_poly(x, y, [(4.6, 10.8), (4.6, 5.8), (6.9, 8.4), (9.0, 4.4), (11.1, 8.4), (13.4, 5.8), (13.4, 10.8)])
    return band or points


def _crown_marks(x, y):
    """Three stones set in the band."""
    if 11.0 < y < 12.0 and any(abs(x - cx) < 0.6 for cx in (6.5, 9.0, 11.5)):
        return 0
    return None


def _door(x, y):
    leaf = 5.8 < x < 12.2 and 7.4 < y < 14.0
    arch = math.hypot(x - 9.0, y - 7.4) < 3.2 and y <= 7.4
    return leaf or arch


def _door_marks(x, y):
    """The seam between its planks, and its knob."""
    if 8.0 < x < 9.0 and y > 5.0:
        return 1
    if 10.0 < x < 11.0 and 10.0 < y < 11.0:
        return 0
    return None


def _gate(x, y):
    outer = (4.2 < x < 13.8 and 8.6 < y < 13.8) or (math.hypot(x - 9.0, y - 8.6) < 4.8 and y <= 8.6)
    opening = (6.2 < x < 11.8 and 9.4 < y < 13.8) or (math.hypot(x - 9.0, y - 9.4) < 2.8 and y <= 9.4)
    bars = abs(x - 7.5) < 0.5 or abs(x - 10.5) < 0.5 or abs(y - 11.5) < 0.5
    return outer and (not opening or bars)


def _stairs_up(x, y):
    """Three steps climbing to the right, and an arrow pointing up clear of them on the left."""
    steps = (7.0 < x < 9.4 and 11.2 < y < 13.8) or (9.4 < x < 11.8 and 8.6 < y < 13.8) or (11.8 < x < 14.0 and 6.0 < y < 13.8)
    arrow = _inside_poly(x, y, [(3.4, 8.0), (5.5, 4.6), (7.6, 8.0)]) or (4.8 < x < 6.2 and 7.8 < y < 10.6)
    return steps or arrow


def _stairs_down(x, y):
    """Three steps going down to the right, and an arrow pointing down clear of them on the right."""
    steps = (4.0 < x < 6.2 and 6.0 < y < 13.8) or (6.2 < x < 8.6 and 8.6 < y < 13.8) or (8.6 < x < 11.0 and 11.2 < y < 13.8)
    arrow = _inside_poly(x, y, [(10.4, 7.6), (14.6, 7.6), (12.5, 11.0)]) or (11.8 < x < 13.2 and 4.4 < y < 7.8)
    return steps or arrow


def _compass(x, y):
    """A four-pointed star, its points out to the rim and its waist wide enough to read as a star and not a cross."""
    dx, dy = abs(x - 9.0), abs(y - 9.0)
    return dx < 2.3 * (1.0 - dy / 6.2) or dy < 2.3 * (1.0 - dx / 6.2)


def _compass_north(x, y):
    """Whether a pixel of the star is its north point, which is red, the way every compass has it."""
    return y < 8.0 and abs(x - 9.0) < 1.5


def _padlock(x, y):
    shackle = 1.5 < math.hypot(x - 13.0, y - 10.6) < 2.7 and y < 10.8
    body = 9.8 < x < 16.2 and 10.6 < y < 16.2
    return shackle or body


def _padlock_marks(x, y):
    """The keyhole."""
    return 0 if 12.0 < x < 14.0 and 12.0 < y < 14.0 else None


#: The badges, in BadgeKind order: the emblem, its marks, and its colour.
BADGES = (
    (_purse, _purse_marks, BADGE_GOLD),          # Shop
    (_anvil, None, BADGE_GOLD),                  # Anvil
    (_bottle, _bottle_marks, BADGE_GOLD),        # Bottle
    (_tankard, _tankard_marks, BADGE_AMBER),     # Inn
    (_swords, None, BADGE_BLUE),                 # Trainer
    (_scroll, _scroll_marks, BADGE_BLUE),        # Scroll
    (_shield, _shield_marks, BADGE_BLUE),        # Shield
    (_sun, _sun_marks, BADGE_WHITE),             # Sun
    (_crown, _crown_marks, BADGE_GOLD),          # Crown
    (_door, _door_marks, BADGE_WHITE),           # Door
    (_gate, None, BADGE_WHITE),                  # Gate
    (_stairs_up, None, BADGE_WHITE),             # StairUp
    (_stairs_down, None, BADGE_WHITE),           # StairDown
)

#: Each chevron: its outline, and which of its arms faces the top left and catches the light.
CHEVRONS = {
    "east": ([(4.0, 2.0), (9.0, 2.0), (15.0, 9.0), (9.0, 16.0), (4.0, 16.0), (10.0, 9.0)], lambda x, y: y < 9.0),
    "west": ([(14.0, 2.0), (9.0, 2.0), (3.0, 9.0), (9.0, 16.0), (14.0, 16.0), (8.0, 9.0)], lambda x, y: y < 9.0),
    "up": ([(2.0, 15.0), (2.0, 10.0), (9.0, 3.0), (16.0, 10.0), (16.0, 15.0), (9.0, 8.0)], lambda x, y: x < 9.0),
    "down": ([(2.0, 3.0), (9.0, 10.0), (16.0, 3.0), (16.0, 8.0), (9.0, 15.0), (2.0, 8.0)], lambda x, y: x < 9.0),
}

#: The glint's frames, from a speck to the full star and back down: how far each arm reaches, and
#: how strong each pixel out along it is.
GLINT = (
    (1, (110,)),
    (2, (230, 120)),
    (4, (250, 220, 150, 70)),
    (2, (150, 60)),
)


def _badge(img, ox, oy, emblem, marks, ramp, enamel):
    _medallion(img, ox, oy, ramp, enamel)
    _emblem(img, ox, oy, emblem, ramp, enamel, marks)


def _compass_badge(img, ox, oy):
    """The compass: a gold medallion with a four-pointed star, north in red."""
    _medallion(img, ox, oy, BADGE_GOLD, ENAMEL)
    mask = _cell_mask(_compass)
    for y in range(INDICATOR):
        for x in range(INDICATOR):
            if mask[y][x] and not _filled(mask, x + 1, y + 1):
                put(img, ox + x + 1, oy + y + 1, tone(IRON_DK, 1.0))
    _shade_mask(img, ox, oy, mask, BADGE_GOLD)
    for y in range(INDICATOR):
        for x in range(INDICATOR):
            if mask[y][x] and _compass_north(x + 0.5, y + 0.5):
                put(img, ox + x, oy + y, tone(BADGE_RED[3 if x < 9 else 2], 1.0))


def _padlock_cell(img, ox, oy):
    """The padlock, in the bottom right of its cell so it sits on a badge's rim, steel with a dark edge."""
    mask = _cell_mask(_padlock)
    _shade_mask(img, ox, oy, mask, BADGE_STEEL, _padlock_marks)
    _outline_mask(img, ox, oy, mask, BADGE_INK)


def _chevron(img, ox, oy, points, lit_arm, ramp, edge):
    """One chevron, its arm toward the top left lit and the other a shade down, light along its top
    edges and shadow along its bottom ones, and the dark edge round it."""
    mask = _cell_mask(lambda x, y: _inside_poly(x, y, points))
    for y in range(INDICATOR):
        for x in range(INDICATOR):
            if not mask[y][x]:
                continue
            shade = 3 if lit_arm(x + 0.5, y + 0.5) else 2
            if not _filled(mask, x, y - 1):
                shade = min(shade + 1, 4)
            elif not _filled(mask, x, y + 1):
                shade = max(shade - 1, 1)
            put(img, ox + x, oy + y, tone(ramp[shade], 1.0))
    _outline_mask(img, ox, oy, mask, edge)


def _glint(img, ox, oy, reach, strengths):
    """One frame of the glint: a white middle and four arms fading out from it, and at its fullest
    a softer pixel on each diagonal."""
    c = INDICATOR // 2 - 1
    put(img, ox + c, oy + c, GLINT_CORE + (255,))
    for i in range(1, reach + 1):
        a = strengths[i - 1]
        for dx, dy in ((i, 0), (-i, 0), (0, i), (0, -i)):
            put(img, ox + c + dx, oy + c + dy, (GLINT_ARM if i > 1 else GLINT_CORE) + (a,))
    if reach >= 4:
        for dx, dy in ((1, 1), (-1, 1), (1, -1), (-1, -1)):
            put(img, ox + c + dx, oy + c + dy, GLINT_ARM + (120,))


def _plaque(img, ox, oy, trim, enamel):
    """The plaque a name sits on: a dark edge with its corners cut round, a line of trim lit along
    its top and left, a bevel inside that, and flat enamel in the middle. Every row of its top and
    bottom bands is one colour across the middle, and every column of its sides one colour down it,
    so it nine-slices."""
    S = PLAQUE
    lit_e, base_e, dark_e = enamel
    for y in range(S):
        for x in range(S):
            inset = min(x, y, S - 1 - x, S - 1 - y)
            corner = (x in (0, S - 1)) and (y in (0, S - 1))
            if corner:
                continue
            if inset == 0:
                colour = tone(BADGE_INK, 1.0)
            elif inset == 1:
                lit = y == 1 or x == 1 and y < S - 2
                colour = tone(trim, 1.15 if y == 1 else 1.0 if lit else 0.72)
            elif inset == 2:
                colour = tone(lit_e, 0.9) if y == 2 or x == 2 and y < S - 3 else tone(dark_e, 1.0)
            else:
                colour = tone(base_e, 1.0)
            put(img, ox + x, oy + y, colour)


def _door_outline(img, ox, oy):
    """The soft line round a doorway under the mouse: white, strongest a pixel in from the edge and
    fading either side of that, round at the corners, and clear in the middle."""
    S = PLAQUE
    radius = 3.5
    for y in range(S):
        for x in range(S):
            px, py = x + 0.5, y + 0.5
            dx = max(0.0, abs(px - S / 2.0) - (S / 2.0 - radius))
            dy = max(0.0, abs(py - S / 2.0) - (S / 2.0 - radius))
            inset = radius - math.hypot(dx, dy)
            if inset < 0.0:
                continue
            a = 70 if inset < 1.0 else 210 if inset < 2.0 else 90 if inset < 3.0 else 25 if inset < 4.0 else 0
            put(img, ox + x, oy + y, (255, 255, 255, a))


def build_indicators():
    img = new_image(INDICATOR * 15, INDICATOR * 4)
    for column, (emblem, marks, ramp) in enumerate(BADGES):
        _badge(img, column * INDICATOR, 0, emblem, marks, ramp, ENAMEL)
        _badge(img, column * INDICATOR, INDICATOR, emblem, marks, BADGE_GREY, ENAMEL_GREY)
    _padlock_cell(img, PADLOCK_COLUMN * INDICATOR, 0)
    _compass_badge(img, COMPASS_COLUMN * INDICATOR, 0)

    for column, name in enumerate(CHEVRON_COLUMNS):
        points, lit_arm = CHEVRONS[name]
        _chevron(img, column * INDICATOR, INDICATOR * 2, points, lit_arm, BADGE_GOLD, CHEVRON_EDGE)
        _chevron(img, column * INDICATOR, INDICATOR * 3, points, lit_arm, BADGE_GREY, CHEVRON_EDGE_GREY)

    for frame, (reach, strengths) in enumerate(GLINT):
        _glint(img, (GLINT_COLUMN + frame) * INDICATOR, INDICATOR * 2, reach, strengths)

    _plaque(img, PLAQUE_COLUMN * INDICATOR, INDICATOR * 2, RIVET, ENAMEL)
    _plaque(img, PLAQUE_COLUMN * INDICATOR, INDICATOR * 3, BADGE_GREY[3], ENAMEL_GREY)
    _door_outline(img, DOOR_OUTLINE_COLUMN * INDICATOR, INDICATOR * 2)

    write_png(os.path.join(OUT, "indicators.png"), img)

# --------------------------------------------------------------------------- #
# Ivory and ink, for the conversation icons and the key caps
# --------------------------------------------------------------------------- #
# The d20's ivory, darkest to lightest, and its ink edge. The d20 itself is a 3D model now
# (Components/DieModel.cs), and paints its faces in the same ivory as it loads.

DIE_IVORY = ((112, 96, 110), (168, 150, 150), (214, 200, 188), (244, 236, 222), (255, 252, 244))
DIE_EDGE = (42, 30, 50)


# --------------------------------------------------------------------------- #
# The icons beside options in a conversation
# --------------------------------------------------------------------------- #
# 12x12, in DialogueScreen's order: a die for a check, a heart for a flirt, a lock for something
# that needs a flag, and the three voices: a shield for honorable, a coin for pragmatic, a quill for
# witty.

ICON = 12
ICON_GOLD = ((150, 104, 40), (222, 182, 98), (252, 230, 160))
ICON_ROSE = ((128, 34, 60), (214, 76, 104), (250, 160, 180))
ICON_STEEL = ((70, 74, 96), (140, 148, 172), (214, 220, 232))
ICON_INK = (24, 18, 30)


def _icon(img, ox, inside, ramp):
    """One icon: whatever's inside, shaded top left to bottom right along its ramp, with a dark edge."""
    for y in range(ICON):
        for x in range(ICON):
            if inside(x + 0.5, y + 0.5):
                tone = 2 if x + y < 9 else 1 if x + y < 15 else 0
                img[y][ox + x] = list(ramp[tone]) + [255]
    for y in range(ICON):
        for x in range(ICON):
            if img[y][ox + x][3]:
                continue
            if any(0 <= x + dx < ICON and 0 <= y + dy < ICON and img[y + dy][ox + x + dx][3] and img[y + dy][ox + x + dx][:3] != list(ICON_INK)
                   for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1))):
                img[y][ox + x] = list(ICON_INK) + [255]


def build_dialogue_icons():
    img = new_image(ICON * 6, ICON)

    def die(x, y):
        return _inside_poly(x, y, [(6, 1.2), (10.5, 3.6), (10.5, 8.4), (6, 10.8), (1.5, 8.4), (1.5, 3.6)])
    _icon(img, 0, die, DIE_IVORY[2:])

    def heart(x, y):
        return ((x - 4.2) ** 2 + (y - 4.6) ** 2 < 6.0 or (x - 7.8) ** 2 + (y - 4.6) ** 2 < 6.0
                or _inside_poly(x, y, [(1.7, 5.4), (10.3, 5.4), (6, 10.6)]))
    _icon(img, ICON, heart, ICON_ROSE)

    def lock(x, y):
        shackle = 3.2 < ((x - 6) ** 2 + (y - 5) ** 2) ** 0.5 < 4.6 and y < 5.5
        body = 2.5 < x < 9.5 and 5.0 < y < 10.6
        return shackle or body
    _icon(img, ICON * 2, lock, ICON_STEEL)

    def shield(x, y):
        return _inside_poly(x, y, [(2, 1.5), (10, 1.5), (10, 6), (6, 10.8), (2, 6)])
    _icon(img, ICON * 3, shield, ICON_STEEL)

    def coin(x, y):
        return ((x - 6) / 4.4) ** 2 + ((y - 6) / 4.6) ** 2 < 1.0
    _icon(img, ICON * 4, coin, ICON_GOLD)

    def quill(x, y):
        vane = _inside_poly(x, y, [(10.5, 1.2), (8.2, 6.5), (5.0, 8.4), (7.0, 4.0)])
        shaft = abs((x - 2.0) - (10.0 - y)) < 0.8 and 2.0 < x < 6.5
        return vane or shaft
    _icon(img, ICON * 5, quill, DIE_IVORY[2:])

    write_png(os.path.join(OUT, "dialogue-icons.png"), img)


# --------------------------------------------------------------------------- #
# keycaps.png -- the keys and the mouse the tutorial tips and HOW TO PLAY show
# --------------------------------------------------------------------------- #
# One row of 14x14 frames, drawn at 3x: a key cap, nine-sliced in the game so it's as wide as its
# label (E, TAB, SPACE), then the mouse three ways: its left button lit for a click, its right
# button lit, and its wheel lit. Ivory like the d20, so the dark letters KeyCaps writes on it read,
# lit from the top left like everything else, with a deeper lip along the bottom so it looks like a
# key you could press. The cap keeps to the nine-slice rule: every row of its top and bottom bands
# is one colour across the middle, every column of its side bands one colour down it, and the
# middle is flat.

KEYCAP = 14

#: Fixed corner of the cap's nine-slice. Has to match CornerSize in Components/KeyCaps.cs, and
#: stay deeper than the lip so the lip is all in the bottom band.
KEYCAP_CORNER = 5

#: The rows of the lip along the bottom, deepest last: lit, then shadow, then deep shadow.
KEYCAP_LIP = (10, 11, 12)

#: The lit button and wheel are the cursor's torchlight.
KEY_LIT = ((150, 82, 30), (232, 150, 58), (250, 196, 120))


def _keycap(img):
    """The cap: a dark rounded edge, the top face with a highlight along its top, a lit left side,
    a shadowed right side, and the lip."""
    S = KEYCAP
    rounded = ((0, 0), (S - 1, 0), (0, S - 1), (S - 1, S - 1))
    for y in range(S):
        for x in range(S):
            if (x, y) in rounded:
                continue
            edge = x in (0, S - 1) or y in (0, S - 1) or (x in (1, S - 2) and y in (1, S - 2))
            if edge:
                img[y][x] = list(DIE_EDGE) + [255]
            elif y in KEYCAP_LIP:
                img[y][x] = list(DIE_IVORY[2 - KEYCAP_LIP.index(y)]) + [255]
            elif x == 1:
                img[y][x] = list(DIE_IVORY[2]) + [255]
            elif x == S - 2:
                img[y][x] = list(DIE_IVORY[1]) + [255]
            elif y == 1:
                img[y][x] = list(DIE_IVORY[4]) + [255]
            else:
                img[y][x] = list(DIE_IVORY[3]) + [255]


def _mouse(img, ox, lit):
    """The mouse, its cable at the top: a rounded ivory body, the two buttons split by a groove
    with the wheel in it, and whichever of 'left', 'right' or 'wheel' is `lit` in torchlight."""
    S = KEYCAP
    left, right, top, bottom, split = 3, 11, 2, 12, 6

    def inside(x, y):
        if not (left <= x <= right and top <= y <= bottom):
            return False
        # Round the four corners off by two pixels.
        dx = min(x - left, right - x)
        dy = min(y - top, bottom - y)
        return dx + dy >= 2

    mask = [[inside(x, y) for x in range(S)] for y in range(S)]
    middle = (left + right) // 2
    for y in range(S):
        for x in range(S):
            if not mask[y][x]:
                continue
            on_button = y < split and x != middle
            button = "left" if x < middle else "right"
            if on_button and lit == button:
                shade = KEY_LIT[2] if x == left + 1 or y == top else KEY_LIT[1]
            elif x == middle and y < split or y == split:
                shade = DIE_IVORY[1]
            elif x == right or y == bottom:
                shade = DIE_IVORY[2]
            else:
                shade = DIE_IVORY[4] if (x <= left + 1 or y <= top + 1) and x + y < middle + top + 2 else DIE_IVORY[3]
            img[y][ox + x] = list(shade) + [255]

    # The wheel, in the groove between the buttons.
    for y in range(top + 1, split - 1):
        img[y][ox + middle] = list(KEY_LIT[1] if lit == "wheel" else DIE_IVORY[0]) + [255]

    # The dark edge round the body, then the cable up out of the top of it.
    for y in range(S):
        for x in range(S):
            if mask[y][x]:
                continue
            if any(0 <= x + dx < S and 0 <= y + dy < S and mask[y + dy][x + dx]
                   for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1))):
                img[y][ox + x] = list(DIE_EDGE) + [255]
    img[0][ox + middle] = list(DIE_EDGE) + [255]


def build_keycaps():
    img = new_image(KEYCAP * 4, KEYCAP)
    _keycap(img)
    _mouse(img, KEYCAP, "left")
    _mouse(img, KEYCAP * 2, "right")
    _mouse(img, KEYCAP * 3, "wheel")
    write_png(os.path.join(OUT, "keycaps.png"), img)


if __name__ == "__main__":
    print("Generating UI for Legend Quest...")
    build_button()
    build_panel()
    build_map_marks()
    build_markers()
    build_cursors()
    build_indicators()
    build_dialogue_icons()
    build_keycaps()
    print("Done.")
