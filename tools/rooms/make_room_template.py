"""
Writes the layout guides the owner paints each room over in ChatGPT (12-rooms 3.5): one 1536 x 1024
picture a panel, out of the room's zone file, the shape ChatGPT makes its landscapes in.

    python tools/rooms/make_room_template.py                          # every room in Content/Zones
    python tools/rooms/make_room_template.py docks questeria/lantern-market
    python tools/rooms/make_room_template.py --out <folder> --zones <folder of zone files>

A room is painted in panels (sourcekit.py has the geometry): one for a room 1600 wide, else
ceil((width - 1600) / 1400) + 1, each 1600 canvas pixels of the room, overlapping the next by 200,
the last against the room's right end. Each panel's guide is
shots/art/room-templates/<zone>-<room>-<i>.png, i counting from 1 left to right, and the owner's
painting of it is saved as tools/source/rooms/<zone>/<room>/bg-<i>.png (cut_room.py). Each guide
shows, in light outlines and pale washes so a painting can go straight over it:

- the 16:9 band that's used (the middle 1536 x 864) and the bleed above and below it, which is
  cropped: sky above, ground below;
- the horizon outdoors, and the floor band's back edge: where the buildings and their doors stand,
  or indoors where the back wall meets the floor;
- the floor band, shaded and labelled "keep open: plain ground", where the characters walk (the
  game's tile strip is the floor there);
- every door's box from the zone file, outlined and labelled with its badge's kind and its name,
  and the stairs up and down;
- the edge exits, marked with chevrons and where they go;
- the overlap with each neighbouring panel, hatched: "must continue the neighbour panel";
- the panel's number, the room, where the panel is along it, and the file to save the painting as.

The guides are a brief, not game content, so they never go in Content. Deterministic: the same
bytes every run. Only needs CPython and numpy.
"""

import argparse
import os
import sys

import numpy as np

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import roomkit  # noqa: E402
import sourcekit as sk  # noqa: E402
from roomkit import CANVAS_H, CONTENT  # noqa: E402

PAPER = (246, 244, 239)
SKY = (233, 240, 250)
DISTANCE = (239, 241, 234)
WALL = (243, 237, 229)
FLOOR = (241, 229, 207)
GROUND = (235, 223, 202)
BLEED = (229, 229, 229)
BLEED_HATCH = (214, 214, 214)
BLEED_INK = (140, 140, 140)
HORIZON_INK = (112, 152, 212)
FLOOR_INK = (178, 140, 96)
DOOR_INK = (212, 94, 104)
EXIT_INK = (84, 128, 206)
EXIT_WASH = (221, 230, 247)
OVERLAP_INK = (160, 132, 212)
INK = (118, 106, 100)

#: A 3x5 pixel font, enough for the labels: each glyph is five rows of three.
FONT = {
    "A": ("010", "101", "111", "101", "101"), "B": ("110", "101", "110", "101", "110"),
    "C": ("011", "100", "100", "100", "011"), "D": ("110", "101", "101", "101", "110"),
    "E": ("111", "100", "110", "100", "111"), "F": ("111", "100", "110", "100", "100"),
    "G": ("011", "100", "101", "101", "011"), "H": ("101", "101", "111", "101", "101"),
    "I": ("111", "010", "010", "010", "111"), "J": ("001", "001", "001", "101", "010"),
    "K": ("101", "101", "110", "101", "101"), "L": ("100", "100", "100", "100", "111"),
    "M": ("101", "111", "111", "101", "101"), "N": ("110", "101", "101", "101", "101"),
    "O": ("010", "101", "101", "101", "010"), "P": ("110", "101", "110", "100", "100"),
    "Q": ("010", "101", "101", "110", "011"), "R": ("110", "101", "110", "101", "101"),
    "S": ("011", "100", "010", "001", "110"), "T": ("111", "010", "010", "010", "010"),
    "U": ("101", "101", "101", "101", "111"), "V": ("101", "101", "101", "101", "010"),
    "W": ("101", "101", "111", "111", "101"), "X": ("101", "101", "010", "101", "101"),
    "Y": ("101", "101", "010", "010", "010"), "Z": ("111", "001", "010", "100", "111"),
    "0": ("111", "101", "101", "101", "111"), "1": ("010", "110", "010", "010", "111"),
    "2": ("110", "001", "010", "100", "111"), "3": ("110", "001", "010", "001", "110"),
    "4": ("101", "101", "111", "001", "001"), "5": ("111", "100", "110", "001", "110"),
    "6": ("011", "100", "111", "101", "111"), "7": ("111", "001", "010", "010", "010"),
    "8": ("111", "101", "111", "101", "111"), "9": ("111", "101", "111", "001", "110"),
    " ": ("000", "000", "000", "000", "000"), "-": ("000", "000", "111", "000", "000"),
    "/": ("001", "001", "010", "100", "100"), ".": ("000", "000", "000", "000", "010"),
    ":": ("000", "010", "000", "010", "000"), "%": ("101", "001", "010", "100", "101"),
    "'": ("010", "010", "000", "000", "000"), ",": ("000", "000", "000", "010", "100"),
    "(": ("001", "010", "010", "010", "001"), ")": ("100", "010", "010", "010", "100"),
    "<": ("001", "010", "100", "010", "001"), ">": ("100", "010", "001", "010", "100"),
    "!": ("010", "010", "010", "000", "010"), "?": ("110", "001", "010", "000", "010"),
    "&": ("010", "101", "010", "101", "011"), "+": ("000", "010", "111", "010", "000"),
    "=": ("000", "111", "000", "111", "000"), "_": ("000", "000", "000", "000", "111"),
}


class Sheet:
    """A guide being drawn: RGB, float64, 0..255, 1536 x 1024."""

    def __init__(self):
        self.px = np.zeros((sk.PANEL_H, sk.PANEL_W, 3))
        self.px[...] = PAPER

    def fill(self, x0, y0, x1, y1, colour, alpha=1.0):
        x0, y0 = max(0, int(round(x0))), max(0, int(round(y0)))
        x1, y1 = min(sk.PANEL_W, int(round(x1))), min(sk.PANEL_H, int(round(y1)))
        if x1 > x0 and y1 > y0:
            area = self.px[y0:y1, x0:x1]
            area[...] = area * (1 - alpha) + np.array(colour, np.float64) * alpha

    def hatch(self, x0, y0, x1, y1, colour, alpha=1.0, spacing=18, width=2, slope=1):
        """Diagonal lines over a box, `slope` 1 or -1."""
        x0, y0 = max(0, int(round(x0))), max(0, int(round(y0)))
        x1, y1 = min(sk.PANEL_W, int(round(x1))), min(sk.PANEL_H, int(round(y1)))
        if x1 <= x0 or y1 <= y0:
            return
        ys, xs = np.mgrid[y0:y1, x0:x1]
        on = ((xs + slope * ys) % spacing) < width
        area = self.px[y0:y1, x0:x1]
        area[on] = area[on] * (1 - alpha) + np.array(colour, np.float64) * alpha

    def hline(self, y, x0, x1, colour, width=2, dash=0):
        y = int(round(y))
        if dash:
            for x in range(int(x0), int(x1), dash * 2):
                self.fill(x, y - width // 2, min(x + dash, x1), y - width // 2 + width, colour)
        else:
            self.fill(x0, y - width // 2, x1, y - width // 2 + width, colour)

    def vline(self, x, y0, y1, colour, width=2, dash=0):
        x = int(round(x))
        if dash:
            for y in range(int(y0), int(y1), dash * 2):
                self.fill(x - width // 2, y, x - width // 2 + width, min(y + dash, y1), colour)
        else:
            self.fill(x - width // 2, y0, x - width // 2 + width, y1, colour)

    def box(self, x0, y0, x1, y1, colour, width=2):
        self.hline(y0 + width // 2, x0, x1, colour, width)
        self.hline(y1 - width + width // 2, x0, x1, colour, width)
        self.vline(x0 + width // 2, y0, y1, colour, width)
        self.vline(x1 - width + width // 2, y0, y1, colour, width)

    def stroke(self, points, colour, width=3):
        """A polyline, stamped a square at a time."""
        for (ax, ay), (bx, by) in zip(points, points[1:]):
            n = int(max(abs(bx - ax), abs(by - ay))) + 1
            for t in np.linspace(0.0, 1.0, n):
                x, y = ax + (bx - ax) * t, ay + (by - ay) * t
                self.fill(x - width / 2, y - width / 2, x + width / 2, y + width / 2, colour)

    def text(self, x, y, s, colour=INK, size=3):
        """Writes a label in the pixel font, size pixels to a font pixel; returns where it ends."""
        for ch in s.upper().replace("’", "'"):
            glyph = FONT.get(ch, FONT[" "])
            for gy, row in enumerate(glyph):
                for gx, bit in enumerate(row):
                    if bit == "1":
                        self.fill(x + gx * size, y + gy * size, x + (gx + 1) * size, y + (gy + 1) * size, colour)
            x += 4 * size
        return x

    def rgba(self):
        out = np.zeros((sk.PANEL_H, sk.PANEL_W, 4), np.uint8)
        out[..., :3] = np.rint(np.clip(self.px, 0, 255)).astype(np.uint8)
        out[..., 3] = 255
        return out


def text_width(s, size):
    return len(s) * 4 * size - size


def wrap(words, size, width):
    """Lines of words no wider than width pixels at a font size."""
    lines, line = [], ""
    for w in words.split():
        trial = (line + " " + w).strip()
        if line and text_width(trial, size) > width:
            lines.append(line)
            line = w
        else:
            line = trial
    if line:
        lines.append(line)
    return lines


def label_in(sheet, lines, x0, x1, y, colour, size):
    """Lines of text centred between x0 and x1 from y down, each kept on the picture."""
    for line in lines:
        w = text_width(line, size)
        x = min(max((x0 + x1 - w) / 2, 6), sk.PANEL_W - 6 - w)
        sheet.text(int(round(x)), int(round(y)), line, colour, size)
        y += 7 * size
    return y


def place_names(zones=None):
    """Every zone's and room's name, for the exits' labels: {zone id: name, "zone/room": name}."""
    names = {}
    for zone, room in sk.rooms(zones):
        names[zone.id] = zone.name
        names["%s/%s" % (zone.id, room.id)] = room.name
    return names


def exit_label(zone, e, names):
    """Where an exit goes, as its label in the game says it."""
    t = e.target
    if t.kind == "locked":
        return "locked"
    if t.kind == "back":
        return "back out"
    if e.label:
        return e.label
    if t.kind == "room":
        return names.get("%s/%s" % (zone.id, t.place), t.place)
    return names.get(t.place, t.place)


def template(zone, room, i, names=None):
    """The guide for panel i (from 0) of a room."""
    names = names or {}
    lefts = sk.panel_lefts(room.width)
    n = len(lefts)
    left = lefts[i]
    s = sk.SCALE
    sheet = Sheet()

    def X(x):
        return (x - left) * s

    def Y(y):
        return sk.BLEED + y * s

    top, bottom = roomkit.band(room)
    horizon = sk.horizon(room)
    xs = np.arange(sk.PANEL_W)
    canvas_x = left + (xs + 0.5) / s
    back = np.array([room.back_edge_at(x) for x in canvas_x])
    front = np.array([room.front_edge_at(x) for x in canvas_x])

    # The washes: sky and distance outdoors (a wall indoors), the floor band, the ground below it.
    for col in range(sk.PANEL_W):
        yb, yf = Y(back[col]), Y(front[col])
        if horizon is not None:
            sheet.fill(col, sk.BLEED, col + 1, Y(horizon), SKY)
            sheet.fill(col, Y(horizon), col + 1, yb, DISTANCE)
        else:
            sheet.fill(col, sk.BLEED, col + 1, yb, WALL)
        sheet.fill(col, yb, col + 1, yf, FLOOR)
        sheet.fill(col, yf, col + 1, sk.PANEL_H - sk.BLEED, GROUND)
    for b in room.blocks:
        cx, cy = X(b.cx), Y(b.cy)
        pts = [(cx + b.rx * s * np.cos(a), cy + b.ry * s * np.sin(a)) for a in np.linspace(0, 2 * np.pi, 97)]
        sheet.stroke(pts, FLOOR_INK, 2)
        label_in(sheet, wrap("THE GAME DRAWS WHAT STANDS HERE", 2, 2 * b.rx * s - 20), cx - b.rx * s, cx + b.rx * s,
                 cy - 10, FLOOR_INK, 2)

    # The bleed, cropped off.
    for y0, y1 in ((0, sk.BLEED), (sk.PANEL_H - sk.BLEED, sk.PANEL_H)):
        sheet.fill(0, y0, sk.PANEL_W, y1, BLEED)
        sheet.hatch(0, y0, sk.PANEL_W, y1, BLEED_HATCH)
    sheet.hline(sk.BLEED, 0, sk.PANEL_W, BLEED_INK, 2, dash=10)
    sheet.hline(sk.PANEL_H - sk.BLEED, 0, sk.PANEL_W, BLEED_INK, 2, dash=10)

    # The lines: the horizon, the back of the floor, its front.
    if horizon is not None:
        sheet.hline(Y(horizon), 0, sk.PANEL_W, HORIZON_INK, 2, dash=14)
        sheet.text(10, int(Y(horizon)) - 22, "HORIZON", HORIZON_INK, 3)
    for col in range(sk.PANEL_W):
        sheet.fill(col, Y(back[col]) - 1, col + 1, Y(back[col]) + 1, FLOOR_INK)
    for col in range(0, sk.PANEL_W, 20):
        sheet.fill(col, Y(front[col]) - 1, col + 10, Y(front[col]) + 1, FLOOR_INK)
    back_words = ("THE BACK WALL MEETS THE FLOOR ON THIS LINE" if room.kind == "interior"
                  else "BACK OF THE FLOOR: BUILDINGS AND DOORS STAND ON THIS LINE")
    sheet.text(60, int(Y(back[60])) + 8, back_words, FLOOR_INK, 2)
    sheet.text(60, int(Y(front[60])) + 8, "FRONT OF THE FLOOR", FLOOR_INK, 2)
    if horizon is not None:
        sheet.text(sk.PANEL_W - 14 - text_width("SKY", 3), int(Y(horizon)) - 22, "SKY", INK, 3)
        words = "DISTANCE: FAR-OFF LAND, SEA OR ROOFS"
        sheet.text(sk.PANEL_W - 14 - text_width(words, 2), int(Y(horizon)) + 10, words, INK, 2)
    else:
        sheet.text(sk.PANEL_W - 14 - text_width("BACK WALL", 3), int(Y(back[-1])) - 22, "BACK WALL", INK, 3)

    # The floor band's label, in the middle of what this panel shows of it.
    mid_y = Y((top + bottom) / 2)
    label_in(sheet, ["KEEP OPEN: PLAIN GROUND"], 0, sk.PANEL_W, mid_y - 26, FLOOR_INK, 5)
    label_in(sheet, ["(WHERE THE CHARACTERS WALK)"], 0, sk.PANEL_W, mid_y + 16, FLOOR_INK, 3)

    # The overlaps with the panels either side.
    before, after = sk.neighbours(room.width, i)
    for part, slope in ((before, 1), (after, -1)):
        if part is None:
            continue
        x0, x1, other = part
        a, b = X(x0), X(x1)
        sheet.hatch(a, sk.BLEED, b, sk.PANEL_H - sk.BLEED, OVERLAP_INK, 0.45, spacing=22, width=2, slope=slope)
        for edge in (a, b):
            if 2 < edge < sk.PANEL_W - 2:
                sheet.vline(edge, sk.BLEED, sk.PANEL_H - sk.BLEED, OVERLAP_INK, 3, dash=12)
        words = "MUST CONTINUE THE NEIGHBOUR PANEL: PANEL %d" % (other + 1)
        y = label_in(sheet, wrap(words, 3, b - a - 16), a, b, sk.BLEED + 14, OVERLAP_INK, 3)
        label_in(sheet, wrap("MATCH IT EXACTLY ACROSS THIS STRIP", 2, b - a - 16), a, b, y + 2, OVERLAP_INK, 2)

    # The doors, and the stairs.
    boxes = [(d.badge.upper(), d.name, room.door_box(d), DOOR_INK) for d, _ in roomkit.door_boxes(room)]
    for e in room.exits:
        if e.side in ("up", "down"):
            y = room.back_edge_at(e.x)
            boxes.append(("STAIRS " + e.side.upper(), exit_label(zone, e, names), (e.x - 80, y - 120, 160, 120), EXIT_INK))
    for kind, name, (bx, by, bw, bh), colour in boxes:
        a, b = X(bx), X(bx + bw)
        if b < 0 or a > sk.PANEL_W:
            continue
        sheet.fill(a, Y(by), b, Y(by + bh), colour, 0.10)
        sheet.box(a, Y(by), b, Y(by + bh), colour, 3)
        va, vb = max(a, 0), min(b, sk.PANEL_W)
        if vb - va < 40:
            continue
        for size in (3, 2):
            lines = [kind] + wrap(name, size, vb - va - 10)
            if all(text_width(line, size) <= vb - va - 8 for line in lines):
                break
        label_in(sheet, lines, va, vb, Y(by) + 10, colour, size)

    # The ways out at the room's ends.
    sides = {}
    for e in room.exits:
        if e.side in ("west", "east") and (e.side not in sides or sides[e.side].target.kind == "locked"):
            sides[e.side] = e
    for side, e in sorted(sides.items()):
        x0 = 0 if side == "west" else room.width - 48
        a, b = X(x0), X(x0 + 48)
        if b < 0 or a > sk.PANEL_W:
            continue
        sheet.fill(a, Y(top), b, Y(bottom), EXIT_WASH)
        sheet.box(a, Y(top), b, Y(bottom), EXIT_INK, 2)
        cy = Y((top + bottom) / 2)
        for k in range(3):
            tip = (a + 8 + k * 12) if side == "west" else (b - 8 - k * 12)
            back_x = tip + 14 if side == "west" else tip - 14
            sheet.stroke([(back_x, cy - 16), (tip, cy), (back_x, cy + 16)], EXIT_INK, 4)
        words = "WAY OUT %s: %s" % (side.upper(), exit_label(zone, e, names))
        w = text_width(words, 3)
        tx = b + 10 if side == "west" else a - 10 - w
        sheet.text(int(tx), int(cy) + 40, words, EXIT_INK, 3)

    # What it is, and where it goes.
    title = "PANEL %d OF %d   %s/%s: %s   CANVAS X %d TO %d OF %d" % (
        i + 1, n, zone.id, room.id, room.name, left, left + sk.SPAN, room.width)
    sheet.text(14, 14, title, INK, 4 if text_width(title, 4) <= sk.PANEL_W - 28 else 3)
    sheet.text(14, 50, "BLEED, CROPPED OFF: KEEP PAINTING THE %s UP TO THE EDGE. ONLY THE 16:9 BAND BETWEEN "
               "THE DASHED LINES IS USED." % ("CEILING" if room.kind == "interior" else "SKY"), BLEED_INK, 2)
    sheet.text(14, sk.PANEL_H - sk.BLEED + 14, "BLEED, CROPPED OFF: KEEP PAINTING THE GROUND DOWN TO THE EDGE.", BLEED_INK, 2)
    save = "SAVE AS TOOLS/SOURCE/ROOMS/%s/%s/BG-%d.PNG   (A FOREGROUND STRIP, IF ANY: FG-%d.PNG)" % (zone.id, room.id, i + 1, i + 1)
    sheet.text(14, sk.PANEL_H - 40, save, INK, 3 if text_width(save, 3) <= sk.PANEL_W - 28 else 2)
    return sheet.rgba()


def main():
    ap = argparse.ArgumentParser(description="Write the 1536x1024 guide for every panel of every room.")
    ap.add_argument("only", nargs="*", help="zones or zone/room (default: all)")
    ap.add_argument("--zones", default=os.path.join(CONTENT, "Zones"))
    ap.add_argument("--out", default=sk.TEMPLATES, help="where the guides go (default shots/art/room-templates)")
    args = ap.parse_args()
    if os.path.abspath(args.out).startswith(os.path.abspath(CONTENT)):
        sys.exit("The guides are a brief for painting, not game content: name a folder outside Content.")
    try:
        sk.check_picks(args.only, args.zones)
    except sk.SourceError as e:
        sys.exit(str(e))
    count = 0
    names = place_names(args.zones)
    for zone, room in sk.rooms(args.zones):
        if not sk.wanted(zone, room, args.only):
            continue
        for i in range(sk.panel_count(room.width)):
            sk.write_png(os.path.join(args.out, "%s-%s-%d.png" % (zone.id, room.id, i + 1)), template(zone, room, i, names))
            count += 1
    print("make_room_template: %d guide%s in %s." % (count, "" if count == 1 else "s", sk.shown(args.out)))


if __name__ == "__main__":
    main()
