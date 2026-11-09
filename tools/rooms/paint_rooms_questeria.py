"""
Questeria's streets, painted (12-rooms §6): the gate plaza, the Lantern Market, the festival
square, the Sunspire steps and the Thornbridge gate. One recipe a room, each a function of a
paint_room.Room, registered in RECIPES; tools/rooms/paint_all.py runs them.

    python tools/rooms/paint_all.py questeria --preview shots/art/rooms

The city's look, for every room here: cream stone and timber under azure, rose, teal and plum
tiled roofs; window boxes, vines and blossom; gold finials and pink and navy banners; strings of
paper lanterns; the Sunspire and its keep over the roofs in the far layer, across a hazed city and
lavender mountains; a day sky going pink toward the horizon, the sun in the west (the light is
always from the top left). Buildings stand on the floor's back edge with their doors on the zone
file's doors.

Only needs CPython and numpy.
"""

import math
import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
import paint as P  # noqa: E402
import roomkit  # noqa: E402
from paint import Door, House, Path, Piece, Window, hash01, tones  # noqa: E402
from roomkit import CANVAS_H  # noqa: E402


def lantern_market(r):
    """Questeria's Lantern Market (12-rooms §6): a street of cream stone and timber under blue and
    rose roofs, strings of paper lanterns between the eaves, the Gilded Flagon at the west end,
    the bakery and Halden's forge to the east, and in the middle an open square of stalls with
    the Sunspire beyond it. Late afternoon: the sun low in the west, the windows starting to glow."""
    g = r.ground
    seed = roomkit.seed_of("questeria", "lantern-market")
    time = "day"
    haze_c = P.HAZE[time]

    # ---- the sky -------------------------------------------------------------------------- #
    P.sky(r["sky"], time, seed=seed, horizon=560, sun=(360, 150), clouds=4, cloud_band=(70, 290))

    # ---- the far layer: mountains, the city's roofs, the Sunspire ---------------------------- #
    far = r["far"]
    P.mountains(far, 470, 300, haze_c, seed=seed + 1, k=0.55, count=8)
    P.hill(far, [(0, 520), (300, 500), (700, 515), (1100, 490), (1500, 512), (1900, 496), (2080, 505)], "hill", haze_c, k=0.5, seed=seed + 2)
    P.skyline(far, 0, far.w, 610, haze_c, k=0.5, seed=seed + 3, height=(90, 210), spires=0.2)
    P.castle(far, r.x("far", 1600), 588, 0.8, haze_c, k=0.1, seed=seed + 4)
    P.skyline(far, 0, far.w, 650, haze_c, k=0.36, seed=seed + 5, height=(60, 120), spires=0.08)

    # ---- the mid layer, west to east ----------------------------------------------------------- #
    mid = r["mid"]
    P.floor_shade(mid, g, 52, 0.3)

    # the corner house at the west end
    r.house(House(-30, 200, g, floors=(("stone", 222), ("plaster.blue", 155)), roof="hip", roof_colour="roof.teal", roof_h=145,
                  windows=[Window(48, None, 38, 58, "arch", lit=True, box=True, floor=0), Window(122, None, 34, 54, "arch", shutters="wood.pink", floor=0),
                           Window(40, None, 34, 50, "arch", box=True, floor=1), Window(118, None, 34, 50, "arch", lit=True, floor=1)],
                  chimneys=[(40, 22, 50)], vines=(160,), seed=seed + 10))
    # the garden by the quest board: a blossom tree behind a low wall
    P.tree(mid, 262, g - 40, "blossom", 340, seed=seed + 11)
    P.low_wall(mid, 172, 372, g, 56, plants=("flower.pink", "flower.white"), seed=seed + 12)

    # the Gilded Flagon
    fx = r.door_x("flagon")
    r.house(House(fx - 180, 350, g, floors=(("stone.warm", 235), ("timber", 160)), roof="hip", roof_colour="roof.blue", roof_h=175,
                  jetty=10, finials=True, dormers=(fx - 90, fx + 80), chimneys=[(fx - 140, 26, 70), (fx + 130, 26, 50)],
                  door=Door(fx, 86, 160, "double", wood="wood.dark"),
                  windows=[Window(fx - 112, None, 46, 70, "arch", lit=True, box=True, panes=3, floor=0),
                           Window(fx + 112, None, 46, 70, "arch", lit=True, box=True, panes=3, floor=0),
                           Window(fx - 115, None, 38, 56, "arch", lit=True, box=True, floor=1),
                           Window(fx + 115, None, 38, 56, "arch", box=True, floor=1)],
                  extras=[P.hanging_sign(fx + 170, 412, "tankard", board="wood.dark", side=1, shape="shield"),
                          P.wall_lantern(fx - 62, 470, -1), P.wall_lantern(fx + 62, 470, 1),
                          P.banner(fx - 32, 228, 32, 104, "cloth.pink", "diamond"), P.banner(fx + 32, 228, 32, 104, "cloth.navy", "sun")],
                  seed=seed + 20))
    P.barrel(mid, fx + 196, g + 2, seed=seed + 21)
    P.crate(mid, fx + 232, g + 3, 40, 34, seed=seed + 22, top_goods="apple")

    # the rose cottage (a home)
    rx = r.door_x("rose-south")
    r.house(House(rx - 115, 230, g, floors=(("plaster.pink", 215), ("timber", 145)), roof="front", roof_colour="roof.pink", roof_h=140,
                  jetty=6, finials=True, door=Door(rx, 74, 150, "arch", wood="wood.blue"),
                  windows=[Window(rx - 74, None, 30, 48, "arch", box=True, floor=0), Window(rx + 74, None, 30, 48, "arch", lit=True, box=True, floor=0),
                           Window(rx - 52, None, 34, 50, "arch", shutters="wood.blue", floor=1), Window(rx + 52, None, 34, 50, "arch", shutters="wood.blue", lit=True, floor=1)],
                  seed=seed + 30))
    P.pot(mid, rx - 98, g + 2, 26, 26, plant="flowers", seed=seed + 31)

    # a cypress in the gap
    P.tree(mid, (rx + 115 + r.door_x("rose-garden") - 130) / 2, g, "cypress", 330, seed=seed + 35)

    # the blue house by the square
    bx = r.door_x("rose-garden")
    r.house(House(bx - 130, 250, g, floors=(("stone", 220), ("plaster.blue", 145)), roof="gable", roof_colour="roof.blue", roof_h=140,
                  dormers=(bx - 5,), chimneys=[(bx + 80, 24, 60)], door=Door(bx, 76, 152, "arch", wood="wood"),
                  windows=[Window(bx - 80, None, 36, 56, "arch", lit=True, box=True, floor=0), Window(bx + 72, None, 34, 54, "arch", shutters="wood.pink", floor=0),
                           Window(bx - 80, None, 34, 50, "arch", box=True, floor=1), Window(bx + 72, None, 34, 50, "arch", box=True, lit=True, floor=1)],
                  vines=(bx - 122,), seed=seed + 40))

    # the square: railings at the back with the Sunspire beyond, a blossom tree, two stalls at
    # its sides and the lantern pole between them
    sq0, sq1 = bx + 120, r.door_x("cottage-garden") - 80
    P.fence(mid, sq0, sq1, g - 2, 44, kind="iron")
    P.tree(mid, sq0 + 52, g - 4, "blossom", 270, seed=seed + 50)
    pole_x = sq0 + 178
    P.stall(mid, sq0 + 86, g, 150, ("cloth.blue", "cloth.cream"), ("apple", "orange", "pear"), seed=seed + 52)
    P.stall(mid, sq1 - 76, g, 140, ("cloth.pink", "cloth.cream"), ("flower.pink", "flower.yellow", "flower.white"), seed=seed + 53)
    P.lantern_pole(mid, pole_x, g, 380)
    P.sacks(mid, pole_x + 40, g + 4, 2, seed=seed + 55)
    P.crate(mid, pole_x + 96, g + 4, 40, 32, seed=seed + 54, top_goods="orange")

    # the plum cottage
    cx = r.door_x("cottage-garden")
    r.house(House(cx - 80, 200, g, floors=(("stone.warm", 210), ("plaster", 120)), roof="hip", roof_colour="roof.plum", roof_h=135,
                  chimneys=[(cx + 90, 22, 50)], door=Door(cx, 72, 148, "arch", wood="wood"),
                  windows=[Window(cx + 82, None, 30, 48, "arch", box=True, floor=0),
                           Window(cx - 30, None, 32, 46, "arch", box=True, floor=1), Window(cx + 60, None, 32, 46, "arch", lit=True, floor=1)],
                  vines=(cx - 72,), seed=seed + 60))

    # the Rising Loaf, the bakery
    kx = r.door_x("bakery")
    r.house(House(kx - 130, 260, g, floors=(("plaster.yellow", 220), ("timber", 145)), roof="hip", roof_colour="roof.pink", roof_h=158,
                  jetty=8, dormers=(kx,), chimneys=[(kx + 90, 26, 72)], door=Door(kx, 74, 150, "arch", wood="wood.pink"),
                  windows=[Window(kx - 85, None, 56, 66, "square", lit=True, panes=3, floor=0),
                           Window(kx + 85, None, 36, 56, "arch", lit=True, box=True, floor=0),
                           Window(kx - 70, None, 36, 52, "arch", box=True, floor=1), Window(kx + 70, None, 36, 52, "arch", box=True, floor=1)],
                  extras=[P.awning(kx - 120, kx - 48, g - 170, 28, ("cloth.pink", "cloth.cream")),
                          P.hanging_sign(kx - 130, 410, "loaf", board="cloth.cream", side=-1, shape="oval")],
                  seed=seed + 70))
    P.barrel(mid, kx - 30 + 160, g + 2, 34, 44, seed=seed + 71)

    # Halden's forge
    hx = r.door_x("forge")
    r.house(House(hx - 120, 240, g, floors=(("stone.grey", 225), ("timber", 135)), roof="gable", roof_colour="roof.slate", roof_h=135,
                  chimneys=[(hx - 70, 36, 96)], door=Door(hx, 104, 156, "open", wood="wood.dark"),
                  windows=[Window(hx + 86, None, 30, 44, "square", lit=True, floor=0),
                           Window(hx - 60, None, 34, 50, "arch", floor=1), Window(hx + 60, None, 34, 50, "arch", lit=True, floor=1)],
                  extras=[P.hanging_sign(hx - 120, 418, "anvil", board="wood.dark", side=-1, shape="shield")],
                  seed=seed + 80))

    # the awning house
    ax = r.door_x("awning-east")
    r.house(House(ax - 112, 212, g, floors=(("plaster", 212), ("plaster.pink", 138)), roof="front", roof_colour="roof.red", roof_h=135,
                  finials=True, door=Door(ax, 70, 148, "arch", wood="wood.green"),
                  windows=[Window(ax - 66, None, 40, 52, "square", box=True, floor=0),
                           Window(ax - 48, None, 34, 50, "arch", shutters="wood.green", floor=1), Window(ax + 50, None, 34, 50, "arch", shutters="wood.green", lit=True, floor=1)],
                  extras=[P.awning(ax - 96, ax - 36, g - 164, 24, ("cloth.teal", "cloth.cream"))],
                  seed=seed + 90))

    # the tall teal townhouse
    tx = r.door_x("townhouse-market")
    r.house(House(tx - 115, 230, g, floors=(("stone", 212), ("plaster.blue", 125), ("plaster.blue", 110)), roof="hip", roof_colour="roof.teal", roof_h=130,
                  finials=True, chimneys=[(tx - 80, 24, 50)], door=Door(tx, 74, 150, "arch", wood="wood.teal"),
                  windows=[Window(tx - 70, None, 36, 56, "arch", lit=True, box=True, floor=0), Window(tx + 72, None, 34, 54, "arch", floor=0),
                           Window(tx - 70, None, 34, 50, "arch", box=True, floor=1), Window(tx, None, 34, 50, "arch", lit=True, floor=1),
                           Window(tx + 70, None, 34, 50, "arch", box=True, floor=1),
                           Window(tx - 38, None, 32, 46, "arch", floor=2), Window(tx + 38, None, 32, 46, "arch", lit=True, floor=2)],
                  seed=seed + 100))
    P.bench(mid, tx - 130, g + 2, 70)

    # the east end: a well under a blossom tree
    P.tree(mid, 3160, g - 6, "oak", 330, seed=seed + 110)
    P.well(mid, 3138, g + 2, seed=seed + 111)

    # strings of lanterns between the eaves, and across the square from the pole
    strings = [
        [(150, 330), (fx - 172, 300)],
        [(fx + 176, 286), (rx - 108, 330)],
        [(rx + 112, 350), (bx - 126, 320)],
        [(bx + 124, 300), (pole_x, g - 372)],
        [(pole_x, g - 372), (cx - 80, 330)],
        [(cx + 120, 340), (kx - 130, 330)],
        [(kx + 130, 300), (hx - 120, 300)],
        [(hx + 120, 330), (ax - 112, 340)],
        [(ax + 100, 330), (tx - 115, 300)],
    ]
    for i, pts in enumerate(strings):
        P.lantern_string(mid, pts, sag=26 + 8 * (i % 3), seed=seed + 120 + i)

    # ---- the foreground: two lamp posts and a heap of flowers along the bottom ------------------ #
    fore = r["fore"]
    dark = 0.5
    for lx in (102, 1331):
        P.lamp_post(fore, r.x("fore", lx), CANVAS_H + 40, 560, lit=True, dark=dark, reach=2.6)
    P.fore_foliage(fore, r.x("fore", 794), CANVAS_H + 26, 230, 96, seed + 130, dark)
    P.fore_foliage(fore, r.x("fore", 2240), CANVAS_H + 30, 180, 78, seed + 131, dark)
    P.fore_foliage(fore, r.x("fore", 3180), CANVAS_H + 30, 220, 92, seed + 132, dark)
    P.near_shade(fore, 820, 0.22)


def _sky_and_city(r, seed, castle_x, castle_scale=0.8, castle_base=588, sea_to=None, roofs_k=0.5, time="day", city_to=None):
    """The sky and far layer every Questeria street shares: the day sky with the sun in the west,
    lavender mountains, green hills, two ranks of the city's roofs, and the Sunspire standing over
    room x castle_x (none if castle_x is None). `sea_to` (a far-layer x) puts the harbour's sea at
    the west end, up to there; `city_to` (a far-layer x) ends the roofs there, for the country
    outside the walls."""
    haze_c = P.HAZE[time]
    P.sky(r["sky"], time, seed=seed, horizon=560, sun=(360, 150), clouds=4, cloud_band=(70, 290))
    far = r["far"]
    P.mountains(far, 470, 300, haze_c, seed=seed + 1, k=0.55, count=max(3, far.w // 260))
    P.hill(far, [(0, 520), (far.w * 0.15, 500), (far.w * 0.35, 515), (far.w * 0.55, 490), (far.w * 0.75, 512), (far.w, 500)],
           "hill", haze_c, k=0.5, seed=seed + 2)
    x0 = 0
    if sea_to:
        P.sea(far, 505, far.h, haze_c, seed=seed + 6, x0=0, x1=sea_to + 60)
        x0 = sea_to
    x1 = far.w if city_to is None else city_to
    P.skyline(far, x0, x1, 610, haze_c, k=roofs_k, seed=seed + 3, height=(90, 210), spires=0.2)
    if castle_x is not None:
        P.castle(far, r.x("far", castle_x), castle_base, castle_scale, haze_c, k=0.1, seed=seed + 4)
    P.skyline(far, x0, x1, 650, haze_c, k=roofs_k - 0.14, seed=seed + 5, height=(60, 120), spires=0.08)


def _fore(r, seed, lamps=(), bushes=(), dark=0.5):
    """The foreground every Questeria street shares: dark lamp posts at room x `lamps`, heaps of
    leaves and flowers rising into the bottom at room x `bushes` ((x, width, height)), and the
    ground darkening toward the viewer."""
    fore = r["fore"]
    for lx in lamps:
        P.lamp_post(fore, r.x("fore", lx), CANVAS_H + 40, 560, lit=True, dark=dark, reach=2.6)
    for i, (bxx, bw, bh) in enumerate(bushes):
        P.fore_foliage(fore, r.x("fore", bxx), CANVAS_H + 28, bw, bh, seed + 130 + i, dark)
    P.near_shade(fore, 820, 0.22)


def gate_plaza(r):
    """The gate plaza (12-rooms §6): the city gate at the west end with the harbour beyond it, the
    Watch's notice board under its tower, a turret house, the Chapel of the Dawn with its rose
    window and bell tower, the ivy house, the Chandler's with candles in its window, the rose house
    and the awning house, and the arch through to the festival square at the east end."""
    g = r.ground
    seed = roomkit.seed_of("questeria", "gate-plaza")
    _sky_and_city(r, seed, 2900, 0.7, 590, sea_to=r.x("far", 700))
    mid = r["mid"]
    P.floor_shade(mid, g, 52, 0.3)

    # the city gate, open onto the road to the docks, and the Watch's board
    P.gatehouse(mid, 40, 360, g, 90, 150, 230, h=380, roof_colour="roof.teal", opening="open", tower_w=90, seed=seed + 10)
    P.notice_board(mid, 300, g + 2, 96, seed=seed + 11)

    # the turret house
    tx = r.door_x("turret-south")
    P.tower(mid, tx + 128, g, 62, 430, "roof.blue", 90, seed=seed + 12, banner="cloth.navy")
    r.house(House(tx - 60, 160, g, floors=(("stone", 222), ("plaster.pink", 140)), roof="hip", roof_colour="roof.pink", roof_h=120,
                  door=Door(tx, 74, 150, "arch", wood="wood.blue"),
                  windows=[Window(tx + 50, None, 30, 48, "arch", lit=True, box=True, floor=0),
                           Window(tx - 30, None, 32, 48, "arch", box=True, floor=1), Window(tx + 40, None, 32, 48, "arch", floor=1)],
                  seed=seed + 13))

    # the Chapel of the Dawn: a tall gable, a rose window, lancets of stained glass, a bell tower
    cx = r.door_x("chapel")
    P.tower(mid, cx + 205, g, 66, 480, "roof.blue", 120, seed=seed + 20, slits=3, banner="cloth.pink")
    r.house(House(cx - 175, 345, g, floors=(("stone.warm", 270), ("stone.warm", 110)), roof="front", roof_colour="roof.blue", roof_h=200,
                  finials=True, gable_window=False, door=Door(cx, 94, 174, "double", wood="wood.dark"),
                  windows=[Window(cx - 110, None, 34, 120, "arch", lit=True, panes=1, floor=0, sill=True),
                           Window(cx + 110, None, 34, 120, "arch", lit=True, panes=1, floor=0, sill=True)],
                  extras=[_rose_window(cx, g - 270 - 70, 46), P.banner(cx - 150, g - 262, 28, 84, "cloth.pink", "sun"),
                          P.banner(cx + 150, g - 262, 28, 84, "cloth.pink", "sun")],
                  quoins=True, seed=seed + 21))

    # the ivy house
    ix = r.door_x("ivy-west")
    r.house(House(ix - 120, 230, g, floors=(("stone", 220), ("plaster", 140)), roof="hip", roof_colour="roof.blue", roof_h=140,
                  chimneys=[(ix + 70, 24, 56)], door=Door(ix, 74, 150, "arch", wood="wood.green"),
                  windows=[Window(ix - 72, None, 34, 54, "arch", box=True, floor=0), Window(ix + 72, None, 34, 54, "arch", lit=True, floor=0),
                           Window(ix - 60, None, 34, 50, "arch", shutters="wood.green", floor=1), Window(ix + 60, None, 34, 50, "arch", lit=True, floor=1)],
                  vines=(ix - 110, ix - 40, ix + 104), seed=seed + 30))

    # a garden: a blossom tree over a low wall
    sx = r.door_x("shop-south")
    gx0, gx1 = ix + 116, sx - 134
    P.tree(mid, (gx0 + gx1) / 2, g - 30, "blossom", 300, seed=seed + 35)
    P.low_wall(mid, gx0 + 6, gx1 - 6, g, 54, plants=("flower.pink", "flower.white", "flower.yellow"), seed=seed + 36)

    # the Chandler's
    r.house(House(sx - 130, 260, g, floors=(("plaster.yellow", 222), ("timber", 148)), roof="gable", roof_colour="roof.teal", roof_h=140,
                  jetty=8, dormers=(sx + 20,), chimneys=[(sx - 90, 24, 60)], door=Door(sx, 74, 150, "arch", wood="wood.teal"),
                  windows=[Window(sx - 78, None, 60, 64, "square", lit=True, panes=3, floor=0), Window(sx + 84, None, 34, 54, "arch", lit=True, box=True, floor=0),
                           Window(sx - 66, None, 36, 52, "arch", box=True, floor=1), Window(sx + 66, None, 36, 52, "arch", floor=1)],
                  extras=[P.awning(sx - 116, sx - 40, g - 172, 28, ("cloth.teal", "cloth.cream")),
                          P.hanging_sign(sx + 130, 412, "candle", board="cloth.navy", side=1, shape="shield")],
                  seed=seed + 40))

    # an oak and a cart in the yard between
    rx = r.door_x("rose-west")
    P.tree(mid, (sx + 130 + rx - 110) / 2 + 10, g - 2, "oak", 320, seed=seed + 45)
    P.cart(mid, (sx + 130 + rx - 110) / 2 - 10, g + 6, 130, seed=seed + 46)

    # the rose house
    r.house(House(rx - 110, 220, g, floors=(("plaster.pink", 215), ("timber", 145)), roof="front", roof_colour="roof.pink", roof_h=140,
                  jetty=6, finials=True, door=Door(rx, 74, 150, "arch", wood="wood.blue"),
                  windows=[Window(rx - 70, None, 30, 48, "arch", box=True, floor=0), Window(rx + 70, None, 30, 48, "arch", lit=True, box=True, floor=0),
                           Window(rx - 50, None, 34, 50, "arch", shutters="wood.blue", floor=1), Window(rx + 50, None, 34, 50, "arch", shutters="wood.blue", floor=1)],
                  seed=seed + 50))

    # a fruit stall under a string of lanterns
    ax = r.door_x("awning-south")
    stall_x = (rx + 110 + ax - 110) / 2
    P.tree(mid, stall_x + 70, g - 20, "cypress", 300, seed=seed + 55)
    P.stall(mid, stall_x - 20, g, 160, ("cloth.pink", "cloth.cream"), ("apple", "pear", "orange"), seed=seed + 56)

    # the awning house
    r.house(House(ax - 110, 220, g, floors=(("plaster", 212), ("plaster.blue", 138)), roof="hip", roof_colour="roof.plum", roof_h=130,
                  chimneys=[(ax + 70, 22, 50)], door=Door(ax, 70, 148, "arch", wood="wood.pink"),
                  windows=[Window(ax - 64, None, 40, 52, "square", box=True, floor=0), Window(ax + 66, None, 30, 48, "arch", lit=True, floor=0),
                           Window(ax - 48, None, 34, 50, "arch", shutters="wood.pink", floor=1), Window(ax + 50, None, 34, 50, "arch", lit=True, floor=1)],
                  extras=[P.awning(ax - 94, ax - 34, g - 164, 24, ("cloth.pink", "cloth.cream"))],
                  seed=seed + 60))

    # a tall townhouse, and the arch through to the festival square
    r_end = r.room.width
    P.tree(mid, ax + 150, g - 10, "blossom", 260, seed=seed + 65)
    house_x = ax + 190
    r_mid = House(house_x, 210, g, floors=(("stone", 212), ("plaster.blue", 125), ("plaster.blue", 110)), roof="hip", roof_colour="roof.teal",
                  roof_h=130, finials=True, chimneys=[(house_x + 170, 24, 50)],
                  windows=[Window(house_x + 50, None, 34, 54, "arch", lit=True, box=True, floor=0), Window(house_x + 150, None, 34, 54, "arch", floor=0),
                           Window(house_x + 50, None, 34, 50, "arch", box=True, floor=1), Window(house_x + 150, None, 34, 50, "arch", lit=True, floor=1),
                           Window(house_x + 100, None, 32, 46, "arch", floor=2)],
                  seed=seed + 70)
    P.building(mid, r_mid)
    P.gatehouse(mid, house_x + 230, r_end + 40, g, house_x + 270, 140, 220, h=320, roof_colour="roof.blue", opening="open", towers=False,
                seed=seed + 75)

    # lanterns between the eaves
    strings = [[(tx + 100, 330), (cx - 175, 300)], [(cx + 240, 260), (ix - 120, 300)], [(ix + 110, 300), (sx - 130, 300)],
               [(sx + 130, 320), (rx - 110, 320)], [(rx + 110, 330), (ax - 110, 330)], [(ax + 110, 320), (house_x, 300)]]
    for i, pts in enumerate(strings):
        P.lantern_string(mid, pts, sag=26 + 8 * (i % 3), seed=seed + 80 + i)

    _fore(r, seed, lamps=(620, 2000), bushes=((380, 220, 92), (1460, 190, 80), (2860, 220, 90)))


def _rose_window(cx, cy, r):
    """A chapel's rose window, as a house extra: a ring of stone tracery round petals of coloured
    glass, lit from inside."""

    def draw(p, house=None):
        st = tones("stone")
        ring = Path.circle(cx, cy, r + 9)
        p.fill(ring, st, "base-light")
        p.edge(ring, st, 1.4, "terminator")
        glass = p.fill(Path.circle(cx, cy, r), tones("#3e4f8e"), "base")
        cols = ("#e25d8e", "#5f8fd8", "#e9b54c", "#6fd0b0")
        for k in range(8):
            a = math.radians(k * 45)
            petal = Path.ellipse(cx + math.cos(a) * r * 0.55, cy + math.sin(a) * r * 0.55, r * 0.36, r * 0.2, rot=k * 45)
            p.fill(petal, tones(cols[k % 4]), "light", clip=glass)
        p.fill(Path.circle(cx, cy, r * 0.25), tones("#e9b54c"), "light")
        lead = tones("wood.dark")
        for k in range(8):
            a = math.radians(k * 45 + 22.5)
            p.stroke(Path.polyline([(cx + math.cos(a) * r * 0.26, cy + math.sin(a) * r * 0.26), (cx + math.cos(a) * r, cy + math.sin(a) * r)]),
                     2.2, st, "base-light", clip=glass)
        p.stroke(Path.circle(cx, cy, r * 0.26), 2.2, st, "base-light")
        p.edge(Path.circle(cx, cy, r), lead, 1.6, "base")
        p.glow(cx, cy, r * 2.2, "#ffd890", 0.18)
    draw.box = (cx - r - 14, cy - r - 14, cx + r + 14, cy + r + 14)
    return draw


def festival_square(r):
    """The festival square, the square of the Purging Dawn (12-rooms §6): a colonnade on the west,
    the broad stair up to the Sunspire steps, the palace front with its balcony over the fountain
    and the sun banners hanging from it, the City Gate out to the road, the ivy house and a tall
    townhouse; bunting strung across, the Sunspire high behind."""
    g = r.ground
    seed = roomkit.seed_of("questeria", "festival-square")
    _sky_and_city(r, seed, 1000, 0.85, 590)
    mid = r["mid"]
    P.floor_shade(mid, g, 52, 0.3)

    # the colonnade on the west, and a blossom tree by it
    P.arcade(mid, 20, 560, g, 230, 4, seed=seed + 10, roof_colour="roof.blue")
    P.tree(mid, 660, g - 6, "blossom", 300, seed=seed + 11)

    # the stair up to the Sunspire steps
    up = next(e for e in r.room.exits if e.side == "up")
    P.balustrade(mid, 730, up.x - 150, g, 56, seed=seed + 12)
    P.stairs(mid, up.x, g, 240, 200, 10, seed=seed + 13)

    # the palace front with the balcony, sun banners and an arcade at its foot
    px0, px1 = up.x + 170, 2050
    pw = px1 - px0
    house = House(px0, pw, g, floors=(("stone", 236), ("stone.warm", 170)), roof="hip", roof_colour="roof.blue", roof_h=170, finials=True,
                  dormers=(px0 + pw * 0.25, px0 + pw * 0.75), chimneys=[(px0 + 60, 28, 50), (px1 - 60, 28, 50)],
                  windows=[Window(px0 + pw * k / 6, None, 44, 96, "arch", lit=(k % 2 == 0), panes=2, floor=0, box=False) for k in (1, 2, 4, 5)]
                  + [Window(px0 + pw * k / 6, None, 40, 76, "arch", lit=(k % 2 == 1), floor=1, box=True) for k in (1, 2, 4, 5)]
                  + [Window(px0 + pw / 2, None, 62, 110, "arch", lit=True, panes=3, floor=1, sill=False)],
                  extras=[P.balcony(px0 + pw / 2, g - 236 - 2, 190), P.banner(px0 + pw / 2 - 78, g - 236 - 150, 34, 140, "cloth.pink", "sun"),
                          P.banner(px0 + pw / 2 + 78, g - 236 - 150, 34, 140, "cloth.navy", "sun"),
                          _sun_disc(px0 + pw / 2, g - 236 - 170 - 50, 34)],
                  quoins=True, seed=seed + 20)
    P.building(mid, house)
    P.bush(mid, px0 + pw / 2, g, 150, 50, "leaf", seed=seed + 21, flowers_on=("flower.yellow", "flower.white"))

    # the City Gate, out to the road
    gx = r.door_x("road")
    bx, by, bw, bh = r.door_box("road")
    doorway = P.gatehouse(mid, gx - 170, gx + 170, g, gx - 92, 184, 262, h=410, roof_colour="roof.teal", opening="portcullis", tower_w=88,
                          banners=("cloth.navy", "cloth.pink"), seed=seed + 30)
    r.doorway("road", doorway)

    # the ivy house and the townhouse
    ix = r.door_x("ivy-south")
    r.house(House(ix - 100, 200, g, floors=(("stone", 220), ("plaster", 140)), roof="hip", roof_colour="roof.pink", roof_h=130,
                  door=Door(ix, 72, 150, "arch", wood="wood.green"),
                  windows=[Window(ix + 64, None, 30, 48, "arch", lit=True, box=True, floor=0),
                           Window(ix - 50, None, 34, 50, "arch", shutters="wood.green", floor=1), Window(ix + 50, None, 34, 50, "arch", floor=1)],
                  vines=(ix - 92, ix + 92), seed=seed + 40))
    tx = r.door_x("townhouse-south")
    r.house(House(tx - 110, 220, g, floors=(("stone", 212), ("plaster.blue", 125), ("plaster.blue", 110)), roof="hip", roof_colour="roof.teal",
                  roof_h=130, finials=True, chimneys=[(tx + 80, 24, 50)], door=Door(tx, 74, 150, "arch", wood="wood.teal"),
                  windows=[Window(tx - 66, None, 34, 54, "arch", lit=True, box=True, floor=0), Window(tx + 66, None, 34, 54, "arch", floor=0),
                           Window(tx - 60, None, 34, 50, "arch", box=True, floor=1), Window(tx + 60, None, 34, 50, "arch", lit=True, floor=1),
                           Window(tx, None, 32, 46, "arch", floor=2)],
                  seed=seed + 50))
    P.tree(mid, r.room.width - 40, g - 6, "oak", 320, seed=seed + 55)

    # bunting across the square
    bunt = ("#e25d8e", "#e9b54c", "#5f8fd8", "#fbf4ea")
    for i, pts in enumerate([[(300, 300), (up.x - 130, 360)], [(px0 + 40, 250), (px0 + pw / 2 - 110, 330)],
                             [(px0 + pw / 2 + 110, 330), (px1 - 40, 250)], [(px1 + 10, 300), (gx - 200, 330)], [(gx + 210, 330), (ix - 100, 300)]]):
        P.lantern_string(mid, pts, sag=34, every=40, colours=bunt, seed=seed + 60 + i)

    _fore(r, seed, lamps=(), bushes=((640, 230, 96), (1600, 200, 70), (2620, 220, 92)))


def _sun_disc(cx, cy, r):
    """The Aurelian sun in gold on a blue roundel, high on the palace front: a house extra."""

    def draw(p, house=None):
        ring = Path.circle(cx, cy, r + 6)
        p.fill(ring, tones("gold"), "base")
        p.edge(ring, tones("gold"), 1.4)
        p.fill(Path.circle(cx, cy, r), tones("cloth.navy"), "base")
        P._icon(p, "sun", cx, cy, r * 1.8)
    draw.box = (cx - r - 14, cy - r - 14, cx + r + 14, cy + r + 14)
    return draw


def sunspire_steps(r):
    """The Sunspire steps (12-rooms §6): the terrace at the top of the stair, the keep's great door
    under its towers with the sun over it, a balustrade along the terrace with the way down to the
    square at its west end, the old iron gate to the Old Crown Vault low in the terrace wall, and the
    Sunspire itself filling the far layer."""
    g = r.ground
    seed = roomkit.seed_of("questeria", "sunspire-steps")
    _sky_and_city(r, seed, 800, 0.92, 600, roofs_k=0.42)
    mid = r["mid"]
    P.floor_shade(mid, g, 52, 0.3)

    # the balustrade along the terrace, open at the stair down
    down = next(e for e in r.room.exits if e.side == "down")
    kx = r.door_x("keep")
    P.balustrade(mid, 0, kx - 240, g, 58, seed=seed + 10, gap=(down.x - 70, down.x + 70))
    P.balustrade(mid, kx + 240, r.room.width, g, 58, seed=seed + 11)

    # the keep's great door
    doorway = P.gatehouse(mid, kx - 190, kx + 190, g, kx - 92, 184, 254, h=430, roof_colour="roof.blue", opening="doors", tower_w=96,
                          gold=True, sign="sun", banners=("cloth.pink", "cloth.navy"), seed=seed + 20)
    r.doorway("keep", doorway)

    # the vault's gate, low in the terrace wall
    vx = r.door_x("vault")
    bx, by, bw, bh = r.door_box("vault")
    vp = Piece(vx - 110, g - 160, vx + 110, g + 10)
    q = vp.sub(vx - 100, g - 150, vx + 100, g + 4)
    P.wall(q, vx - 96, g - 140, vx + 96, g, "stone.grey", seed + 30, quoins=True)
    vrect = P.archway(q, vx - 44, g, 88, 104, tones("stone.grey"), opening="portcullis")
    vp.stamp(q)
    vp.edge(Path.rect(vx - 96, g - 140, 192, 140), tones("stone.grey"), 1.4)
    vp.ground_shadow(vx - 100, vx + 100, g + 2, 7, 0.32)
    mid.place(vp)
    r.doorway("vault", vrect)

    # lantern poles either side of the door
    for side in (-1, 1):
        P.lantern_pole(mid, kx + side * 300, g, 330)

    _fore(r, seed, lamps=(1500,), bushes=((80, 200, 90), (1000, 180, 70)))


def thornbridge_gate(r):
    """The Thornbridge gate (12-rooms §6): the last of the Lantern Market's houses, a turret house
    and a townhouse with a garden between, and the city's east gate, its arch open on the road down
    to Thornbridge and the refugees' tents by the river below."""
    g = r.ground
    seed = roomkit.seed_of("questeria", "thornbridge-gate")
    _sky_and_city(r, seed, None, city_to=1240)
    far = r["far"]
    _camp_far(far, 1440, 540, seed + 7)  # a one-screen room: the far layer doesn't slide
    mid = r["mid"]
    P.floor_shade(mid, g, 52, 0.3)

    # the end of the market street on the west
    r_end = r.room.width
    P.building(mid, House(-40, 200, g, floors=(("stone", 220), ("timber", 150)), roof="hip", roof_colour="roof.blue", roof_h=140, jetty=8,
                          windows=[Window(40, None, 34, 54, "arch", lit=True, box=True, floor=0), Window(120, None, 34, 54, "arch", floor=0),
                                   Window(40, None, 34, 50, "arch", box=True, floor=1), Window(120, None, 34, 50, "arch", lit=True, floor=1)],
                          chimneys=[(120, 24, 50)], seed=seed + 10))

    # the turret house
    tx = r.door_x("turret-east")
    P.tower(mid, tx - 126, g, 64, 430, "roof.teal", 92, seed=seed + 20, banner="cloth.pink")
    r.house(House(tx - 92, 200, g, floors=(("stone", 222), ("plaster.blue", 140)), roof="hip", roof_colour="roof.teal", roof_h=125,
                  chimneys=[(tx + 70, 22, 50)], door=Door(tx, 74, 150, "arch", wood="wood.blue"),
                  windows=[Window(tx + 64, None, 30, 48, "arch", lit=True, box=True, floor=0),
                           Window(tx - 40, None, 34, 50, "arch", shutters="wood.blue", floor=1), Window(tx + 50, None, 34, 50, "arch", lit=True, floor=1)],
                  seed=seed + 21))

    # a garden between
    hx = r.door_x("townhouse-east")
    P.tree(mid, (tx + 108 + hx - 110) / 2, g - 30, "blossom", 300, seed=seed + 30)
    P.low_wall(mid, tx + 116, hx - 118, g, 54, plants=("flower.pink", "flower.white"), seed=seed + 31)

    # the townhouse
    r.house(House(hx - 110, 220, g, floors=(("plaster.pink", 214), ("timber", 140)), roof="front", roof_colour="roof.pink", roof_h=140,
                  jetty=6, finials=True, door=Door(hx, 74, 150, "arch", wood="wood.teal"),
                  windows=[Window(hx - 66, None, 30, 48, "arch", box=True, floor=0), Window(hx + 66, None, 30, 48, "arch", lit=True, box=True, floor=0),
                           Window(hx - 50, None, 34, 50, "arch", shutters="wood.teal", floor=1), Window(hx + 50, None, 34, 50, "arch", lit=True, floor=1)],
                  seed=seed + 40))

    # the east gate, open on the road to Thornbridge
    P.gatehouse(mid, hx + 160, r_end + 120, g, hx + 230, 190, 270, h=420, roof_colour="roof.blue", opening="open", tower_w=92,
                banners=("cloth.navy", "cloth.pink"), seed=seed + 50)

    P.lantern_string(mid, [(160, 320), (tx - 160, 300)], sag=30, seed=seed + 60)
    P.lantern_string(mid, [(tx + 108, 320), (hx - 110, 320)], sag=34, seed=seed + 61)
    _fore(r, seed, lamps=(), bushes=((160, 220, 90), (900, 180, 74)))


def _camp_far(layer, cx, base, seed):
    """Thornbridge seen from the city's east gate: the road going down, the river, and the camp's
    tents and smoke on the meadow, far off and hazed."""
    haze_c = P.HAZE["day"]
    P.hill(layer, [(cx - 300, base - 70), (cx - 140, base - 48), (cx, base - 40), (cx + 200, base - 52), (cx + 420, base - 66)], "grass", haze_c, k=0.32,
           seed=seed, trees=6, tree_colour="leaf.dark")
    p = Piece(cx - 300, base - 120, cx + 300, base + 80, ss=2, pad=4)
    river = Path.smooth([(cx - 300, base + 40), (cx - 120, base + 22), (cx + 120, base + 34), (cx + 300, base + 18)])
    p.stroke(river, 16, P.haze(tones("water"), haze_c, 0.35), "base")
    p.stroke(river.translate(0, -3), 3, P.haze(tones("water"), haze_c, 0.2), "light")
    cols = ("cloth.cream", "canvas", "cloth.teal", "sack", "cloth.pink")
    for i in range(16):
        x = cx - 160 + 340 * hash01(i, 1, seed)
        y = base - 4 + 20 * hash01(i, 2, seed)
        s = 16 + 12 * hash01(i, 3, seed)
        t = P.haze(tones(cols[i % len(cols)]), haze_c, 0.35)
        tent = Path.polygon([(x - s, y), (x, y - s * 0.9), (x + s, y)])
        m = p.fill(tent, t, "base")
        p.tone(p.where(m, lambda xx, yy, c=x: xx > c), t, "shadow")
    layer.place(p, ink=P._far_line(tones("stone"), haze_c, 0.35), reach=0.7)
    for i in range(3):
        layer.soft_ellipse(cx - 120 + 110 * i, base - 40 - 16 * i, 18, 34, "#e6e2ee", 0.35, hard=0.2)


RECIPES = {
    "questeria/gate-plaza": gate_plaza,
    "questeria/lantern-market": lantern_market,
    "questeria/festival-square": festival_square,
    "questeria/sunspire-steps": sunspire_steps,
    "questeria/thornbridge-gate": thornbridge_gate,
}
