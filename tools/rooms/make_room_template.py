"""
Writes the guide a room is painted over (12-rooms 3.5): one picture per room, as wide as the room
and 900 tall, in canvas pixels, out of its room file.

    python tools/rooms/make_room_template.py --out <folder>                  # every room in Content/Zones
    python tools/rooms/make_room_template.py --out <folder> docks questeria/lantern-market
    python tools/rooms/make_room_template.py --out <folder> --zones <folder of zone files>

It marks, in flat colours with labels in a little pixel font:

- the three bands cut_room.py cuts a painted background along: the sky above the horizon line, the
  far layer between it and the roofline, the mid layer below (an interior is all mid layer);
- the floor band, shaded and hatched, where feet can go, with any blocks (the fountain) ringed;
- the tile strip under it, on its 64-pixel grid, which the game draws itself;
- every door's box, the size its badge gives it, with where the badge floats over it;
- the edge exits, and the up and down exits;
- where the foreground may go: anywhere below the band, and no more than 12% of the band itself;
- a figure the size a person stands (128 by 192) at each spawn, for scale.

The guides are a brief for whoever paints, so they go wherever --out says, never in Content.

Only needs CPython, and there's nothing random in it.
"""

import argparse
import math
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from roomkit import (  # noqa: E402
    CANVAS_H, CONTENT, FORE_LIMIT, Img, band, door_boxes, load_zone, paint_bands, zone_files,
)

SKY = (206, 222, 248)
FAR = (226, 218, 242)
MID = (246, 240, 228)
FLOOR = (236, 206, 176)
STRIP = (222, 214, 204)
FORE = (196, 228, 196)
DOOR = (226, 72, 84)
EXIT = (64, 120, 210)
INK = (58, 32, 38)
FIGURE = (120, 96, 160)

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
}


def text(img, x, y, s, colour=INK, size=3):
    """Writes a label in the pixel font, size canvas pixels to a font pixel."""
    for ch in s.upper():
        glyph = FONT.get(ch, FONT[" "])
        for gy, row in enumerate(glyph):
            for gx, bit in enumerate(row):
                if bit == "1":
                    img.fill(x + gx * size, y + gy * size, size, size, colour)
        x += 4 * size
    return x


def dashed(img, y, colour, label):
    for x in range(0, img.w, 24):
        img.fill(x, y - 1, 14, 3, colour)
    text(img, 12, y + 6, label, colour)


def outline(img, x, y, w, h, colour, thick=3):
    img.fill(x, y, w, thick, colour)
    img.fill(x, y + h - thick, w, thick, colour)
    img.fill(x, y, thick, h, colour)
    img.fill(x + w - thick, y, thick, h, colour)


def shade_box(img, x, y, w, h, colour, alpha):
    for yy in range(max(0, y), min(img.h, y + h)):
        for xx in range(max(0, x), min(img.w, x + w)):
            img.blend(xx, yy, colour, alpha)


def template(room):
    """The guide for one room."""
    w = room.width
    img = Img(w, CANVAS_H, MID + (255,))
    horizon, roofline = paint_bands(room)
    top, bottom = band(room)
    if room.kind == "street":
        img.fill(0, 0, w, horizon, SKY)
        img.fill(0, horizon, w, roofline - horizon, FAR)
    # The tile strip on its grid.
    t = room.tiles
    img.fill(0, t.top, w, CANVAS_H - t.top, STRIP)
    for x in range(0, w, 64):
        img.fill(x, t.top, 1, CANVAS_H - t.top, (200, 190, 182))
    for y in range(t.top, CANVAS_H, 64):
        img.fill(0, y, w, 1, (200, 190, 182))
    # The floor band, shaded and hatched, and the blocks in it.
    for x in range(w):
        y0, y1 = int(room.back_edge_at(x)), int(room.front_edge_at(x))
        for y in range(y0, y1):
            img.set(x, y, FLOOR if (x + y) % 16 > 1 else (214, 176, 140))
        img.fill(x, y0 - 1, 1, 3, INK)
        img.fill(x, y1 - 1, 1, 3, INK)
    for b in room.blocks:
        for i in range(720):
            a = i / 720 * 2 * math.pi
            img.fill(int(b.cx + b.rx * math.cos(a)) - 1, int(b.cy + b.ry * math.sin(a)) - 1, 3, 3, INK)
    # Where the foreground may go: below the band freely, and at most 12% of the band.
    for x in range(w):
        y1 = int(room.front_edge_at(x))
        for y in range(y1 + 2, CANVAS_H):
            if (x - y) % 12 < 3:
                img.set(x, y, FORE)
    text(img, 12, CANVAS_H - 26, "FOREGROUND HERE, AND AT MOST %d%% OF THE FLOOR BAND" % int(FORE_LIMIT * 100), (60, 120, 60))
    text(img, 12, int(top) + 10, "FLOOR BAND: FEET GO HERE. THE TILE STRIP UNDER IT IS DRAWN BY THE GAME")
    if room.kind == "street":
        dashed(img, horizon, EXIT, "HORIZON: SKY ABOVE (PARALLAX 0.1)")
        dashed(img, roofline, (120, 80, 170), "ROOFLINE: FAR LAYER ABOVE (0.3), MID BELOW (1.0)")
    else:
        text(img, 12, 12, "INTERIOR: ALL ONE LAYER, NO PARALLAX")
    # Doors, with their badges.
    for d, (bx, by, bw, bh) in door_boxes(room):
        shade_box(img, int(bx), int(by), int(bw), int(bh), DOOR, 60)
        outline(img, int(bx), int(by), int(bw), int(bh), DOOR)
        cx, cy = int(bx + bw / 2), int(by) - 30 - 22
        for yy in range(-22, 23):
            for xx in range(-22, 23):
                if xx * xx + yy * yy <= 22 * 22:
                    img.set(cx + xx, cy + yy, DOOR if xx * xx + yy * yy > 18 * 18 else (255, 236, 236))
        text(img, int(bx) + 6, int(by) + 8, d.badge, DOOR, 2)
        text(img, int(bx) + 6, int(by) + 22, d.name.replace("’", "'")[:18], DOOR, 2)
    # Exits.
    for e in room.exits:
        if e.side in ("west", "east"):
            x = 0 if e.side == "west" else w - 48
            shade_box(img, x, int(top), 48, int(bottom - top), EXIT, 70)
            text(img, x + 4, int(bottom) - 20, e.side[0].upper(), EXIT, 4)
        else:
            x = int(e.x)
            y = int(room.back_edge_at(x))
            outline(img, x - 80, y - 120, 160, 120, EXIT)
            text(img, x - 72, y - 112, "STAIRS " + e.side.upper(), EXIT, 2)
    # A person, for scale, at each spawn.
    for s in room.spawns:
        x, y = int(s.x), int(s.y)
        outline(img, x - 64, y - 192, 128, 192, FIGURE, 2)
        img.fill(x - 20, y - 180, 40, 40, FIGURE)
        img.fill(x - 28, y - 132, 56, 90, FIGURE)
        img.fill(x - 24, y - 42, 18, 42, FIGURE)
        img.fill(x + 6, y - 42, 18, 42, FIGURE)
        text(img, x - 60, y + 6, s.name[:14], FIGURE, 2)
    text(img, w - 12 - 4 * 3 * len(room.paint), 12 if room.kind == "street" else 30, room.paint.upper())
    return img


def main():
    ap = argparse.ArgumentParser(description="Write the painting guide for each room.")
    ap.add_argument("only", nargs="*", help="zones or zone/room (default: all)")
    ap.add_argument("--zones", default=os.path.join(CONTENT, "Zones"))
    ap.add_argument("--out", required=True, help="where the guides go (not in Content)")
    args = ap.parse_args()
    if os.path.abspath(args.out).startswith(os.path.abspath(CONTENT)):
        sys.exit("The guides are a brief for painting, not game content: name a folder outside Content.")
    files = zone_files(args.zones)
    if not files:
        sys.exit("No zone files in %s." % args.zones)
    for path in files:
        zone = load_zone(path)
        for room in zone.rooms:
            if args.only and zone.id not in args.only and "%s/%s" % (zone.id, room.id) not in args.only:
                continue
            template(room).save(os.path.join(args.out, "%s-%s.png" % (zone.id, room.id)))


if __name__ == "__main__":
    main()
