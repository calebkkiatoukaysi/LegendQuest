"""
Paints every room in every zone file out of the game's own art, until the painted rooms arrive:
the props, buildings and tilesets the top-down maps were made of, stood up side on in their layers,
with more detail than the maps had. No new style, and nothing grey.

    python tools/rooms/generate_room_placeholders.py                     # every room in Content/Zones
    python tools/rooms/generate_room_placeholders.py docks questeria/lantern-market
    python tools/rooms/generate_room_placeholders.py --preview <folder>  # and 1600x900 previews there
    python tools/rooms/generate_room_placeholders.py --zones <folder> --out <folder>   # other files
    python tools/rooms/generate_room_placeholders.py --manifest <folder>  # and the Content.mgcb and ASSETS.md lines

For each room it writes Content/Rooms/<paint>/ (paint is <zone>/<room> unless the room says):

- sky.png, at 4x: a gradient in the title's blue and pink (twilight in the Mirewood, night at sea),
  dithered in bands the way the generators shade, with two or three of the title's clouds in it.
  Questeria's has one of the title's floating islands too. Underground it's the dark.
- far.png, at 2x: the skyline. The palace, the temple, the guildhall, rooftops, trees, the city
  wall, the windmill, the title's city across the water, and the title's mountains behind
  everything outdoors, all cooled toward the sky and half their colour taken out, with a few of
  their windows lit. Underground it's the hall going on behind, darker, with lamps far off.
- mid.png, at 3x: the room's scenery mid lines, each building moved so the door it has is on the
  room file's door line, then the street filled in the way the maps were dressed: trees and garden
  walls between the houses, lamps, banners, planters, stalls, crates and barrels along the front,
  strings of lanterns and washing lines between the eaves. Indoors it's the wall (beams, plaster,
  panelling, posts, windows, shelves and little framed paintings out of the room's tileset, or the
  keep's marble, pillars, gallery and banners, or the crypt's and the vault's arcades, or the
  cave's rock with roots hanging off its roof) and the furniture standing against it. A door with
  no building over it gets a doorway the size of its box (a stair, an opening with its steps).
  The barrow, the vault and the quarry are underground though their rooms are streets: they get
  the wall too, with its arches open onto the far layer. Props drawn from above to lie flat (the
  bridges, the decals) can't stand up side on, so they're left out of the scenery.
- fore.png, at 4x: the scenery fore lines, dark against the room, never covering more than 12% of
  the floor band.
- thumb.png, 200x113: the room with the camera in its middle, for the zone map.

Then the detail passes, on every building and tree before it's laid down: a two-tone contact
shadow under everything, a shadow under the eaves, a rim of light along the roofs' top and left
edges (it's lit from the top left, as everything is; nothing's ever flipped), the bottom rows
darkened where it meets the ground, window boxes under windows that hadn't any, and moss and cracks
in the stone, the moss cut out of the room's own tileset.

Everything random is seeded on the zone and room's names, so a rerun writes the same bytes.

Where everything goes: the mid layer has a building's foot on the floor band's back edge at its
x, so a door line's box (on the same edge) and the doorway painted on the building meet. The far
and sky layers, and the foreground, put something that stands at room x where it'll be when the
camera's centred on it (roomkit.layer_x). With --manifest it writes rooms-mgcb.txt and
rooms-assets.txt into the folder named: every file it painted, in Content.mgcb's and ASSETS.md's
own formats, ready to paste.

With --preview it also writes, into the folder named (never into Content), what the game will show:
each layer at its scale and parallax, the tile strip drawn from the room file at 4x, the room's
floor props at 4x, two townsfolk at 4x facing left and right, and a badge over every door. A room
wider than the screen gets one with the camera at each end too. --boxes outlines the door boxes.

Only needs CPython.
"""

import argparse
import math
import os
import random
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from roomkit import (  # noqa: E402
    CANVAS_H, CANVAS_W, CONTENT, FORE_LIMIT, LAYERS, THUMB_H, THUMB_W, TILE, WALKER_FEET, Img, art, band,
    bayer, door_boxes, hash01, horizon, hsv, layer_size, layer_x, load_zone, lum,
    mix, parallax, prop, prop_exists, seed_of, tileset, walker, zone_files,
)

OUT = os.path.join(CONTENT, "Rooms")

# --------------------------------------------------------------------------- #
# The palette: generate_harbour.py's, which is the style sheet's
# --------------------------------------------------------------------------- #

OUTLINE = (58, 32, 38)
WOOD = ((84, 46, 40), (126, 72, 50), (162, 100, 62), (196, 128, 74), (226, 162, 98), (244, 196, 136))
GRASS = ((50, 94, 66), (82, 130, 62), (118, 160, 64), (150, 184, 72), (188, 208, 102))
PETALS = ((244, 150, 186), (252, 246, 236), (250, 214, 110), (190, 170, 240), (228, 108, 152))
GLASS = ((255, 168, 72), (255, 214, 120), (255, 246, 206))
GOLD = ((132, 80, 34), (196, 138, 50), (236, 188, 82), (254, 234, 156))
NAVY = ((20, 28, 66), (32, 50, 110), (52, 82, 154), (96, 132, 200))
IRON = ((40, 40, 56), (78, 80, 98), (130, 134, 152))
CLOTH = ((236, 226, 214), (228, 108, 152), (96, 132, 200), (150, 184, 72), (250, 214, 110), (190, 170, 240))
ROPE = (82, 58, 56)
SHADOW = (60, 40, 88)
"""Shadows lean lavender, as the tiles' do."""
RIM = (255, 244, 226)
WATER = ((6, 58, 120), (10, 86, 158), (18, 112, 184), (28, 140, 206), (56, 180, 228), (144, 224, 246))

#: The skies, top to horizon, in the title's colours: blue going pink at the bottom by day.
SKIES = {
    "day": ((0.0, (78, 124, 210)), (0.3, (112, 158, 230)), (0.56, (168, 184, 240)),
            (0.8, (226, 184, 224)), (1.0, (252, 214, 206))),
    "twilight": ((0.0, (48, 52, 122)), (0.35, (92, 82, 166)), (0.66, (164, 118, 188)),
                 (0.88, (230, 152, 176)), (1.0, (248, 188, 168))),
    "night": ((0.0, (10, 14, 40)), (0.42, (24, 30, 78)), (0.78, (60, 50, 118)), (1.0, (122, 88, 148))),
    "void": ((0.0, (8, 6, 16)), (1.0, (30, 24, 44))),
}
#: What the far layer is cooled toward, under each sky.
HAZES = {"day": (178, 182, 232), "twilight": (132, 112, 184), "night": (46, 48, 100), "void": (24, 20, 38)}

# --------------------------------------------------------------------------- #
# The buildings: where each one's door is, in art pixels right of its foot (PropCatalog says, or the
# maps' entrances do)
# --------------------------------------------------------------------------- #

DOORS = {
    "cottage": 2, "farmhouse": 2, "warehouse": -3, "rusty-anvil": 19, "house-shop": 23, "palace": -10,
    "gilded-flagon": 45, "house-ivy": -31, "house-rose": 0, "house-awning": -38, "chapel": 0, "bakery": 24,
    "forge": -8, "townhouse": 16, "turret-house": 15, "fishmonger": 1, "watch-post": -3, "inn": 24,
    "guildhall": 3, "temple": 0, "city-gate": 0, "east-gate": 0, "keep-arch": 0, "quarry-mouth": 0,
    "vault-door": 0, "gate": 0, "door": 0, "root-wall": 0, "tent-ragged": 0, "tent-ragged-2": 0,
}
BUILDINGS = set(DOORS) | {"windmill", "guildhall", "city-wall"}

#: Each door prop's painted doorway, in art pixels: how far its middle is right of the door line
#: (DOORS), how wide it is, how far its bottom is up from the prop's foot (a door up steps), and
#: how tall it is. Measured off the pictures; doors.txt is written from these.
DOOR_ART = {
    "bakery": (0, 16, 1, 32), "chapel": (0, 38, 0, 58), "city-gate": (0, 48, 0, 90), "cottage": (0, 25, 1, 40),
    "farmhouse": (0, 25, 1, 40), "door": (0, 22, 2, 27), "gate": (0, 22, 2, 27), "east-gate": (0, 42, 0, 90),
    "fishmonger": (0, 30, 0, 34), "forge": (0, 22, 2, 50), "gilded-flagon": (0, 26, 1, 41), "guildhall": (4, 30, 36, 34),
    "house-awning": (0, 19, 3, 44), "house-ivy": (0, 25, 12, 44), "house-rose": (0, 26, 12, 44),
    "house-shop": (0, 18, 22, 37), "inn": (0, 18, 22, 37), "keep-arch": (0, 39, 0, 52), "palace": (0, 28, 28, 40),
    "quarry-mouth": (0, 44, 0, 44), "rusty-anvil": (0, 26, 1, 35), "temple": (0, 36, 0, 76), "townhouse": (0, 16, 1, 35),
    "turret-house": (0, 26, 1, 40), "vault-door": (0, 29, 1, 31), "warehouse": (0, 25, 1, 35), "watch-post": (0, 16, 1, 37),
    "root-wall": (0, 28, 0, 36), "tent-ragged": (0, 20, 0, 24), "tent-ragged-2": (0, 20, 0, 24),
}
ROOFS = ("blue", "pink", "teal", "plum")
HOUSES = ("house-ivy", "house-rose", "house-awning", "house-shop", "townhouse", "turret-house", "cottage")
TREES = {"oak", "oak-deep", "oak-light", "oak-pale", "oak-amber", "oak-turning", "great-oak", "cherry-tree",
         "cherry-tree-small", "blossom-tree", "cypress", "fir", "fir-deep", "tree-blackvein",
         "tree-blackvein-pale", "tree-blackvein-bare", "dead-tree", "snag"}


#: Props drawn from above to lie on the ground, which only make sense in the game's own floor.
FLAT = {"stone-bridge", "ranger-bridge", "vault-stair", "vault-stair-shut", "sun-inlay", "sun-mosaic", "light-shaft",
        "rowboat", "sapper-hole", "tar-pool", "courier-body", "satchel", "sewer-grate"}


def standable(name):
    """Whether a prop can stand in a side-on layer."""
    return prop_exists(name) and name not in FLAT


def base_name(name):
    """A building's name without its roof colour."""
    for c in ROOFS:
        if name.endswith("-" + c) and name[:-len(c) - 1] in DOORS:
            return name[:-len(c) - 1]
    return name


def roofed(rng, name):
    """A house in a roof colour picked at random, or the colour it's drawn in."""
    c = rng.choice(("",) + ROOFS)
    return name + "-" + c if c and prop_exists(name + "-" + c) else name


# --------------------------------------------------------------------------- #
# The themes: what each zone's rooms are dressed with
# --------------------------------------------------------------------------- #

STREET_THEMES = {
    "city": dict(
        sky="day", far="city", ground=(204, 192, 214), island=True, strings="lanterns", kerb="garden-wall",
        gap=("cherry-tree", "blossom-tree", "oak", "cherry-tree-small", "cypress", "blossom-tree"),
        tall=("lamp", "lamp-double", "lamp-arm", "banner-pink", "banner-blue", "lamp"),
        front=("planter", "planter-long", "planter-pink", "barrel-flowers", "cypress-pot", "potted-plant",
               "flower-box", "bench-iron", "crate-apples", "crate-onions", "barrel-water", "cask", "topiary",
               "bush-flowers", "bush-pink", "chalkboard", "flowers-pink", "flowers-mixed", "planter-stone"),
        wide=("stall-blue", "stall-pink", "stall-teal", "stall-cloth", "flower-cart"), density=0.55),
    "harbour": dict(
        sky="day", far="harbour", ground=(120, 168, 214), island=False, strings="washing", kerb="rope-posts",
        gap=("net-rack", "crate-stack", "sack-pile", "washing-line", "cherry-tree-small", "crate-stack"),
        tall=("lantern-post", "lamp-arm", "lantern-post"),
        front=("cargo-crate", "cargo-barrel", "barrel-salt", "barrel-water", "sacks", "crate-open", "crate-stack",
               "rope-coil", "handcart", "bollard", "sack-pile", "crate", "barrel"),
        wide=("fish-stall", "net-rack"), density=0.6),
    "meadow": dict(
        sky="day", far="meadow", ground=(150, 190, 120), island=False, strings=None, kerb="fence-wood",
        gap=("oak", "oak-light", "great-oak", "oak-pale", "bush", "haystack"),
        tall=("signpost", "scarecrow"),
        front=("hay-bale", "wheat", "wildflowers", "wildflowers-yellow", "wildflowers-blue", "bush", "bush-small",
               "water-pump", "trough", "flowers-white", "rocks"),
        wide=("haystack",), density=0.5),
    "forest": dict(
        sky="twilight", far="forest", ground=(58, 70, 92), island=False, strings=None, kerb=None,
        gap=("tree-blackvein", "oak-turning", "fir", "fir-deep", "tree-blackvein-pale", "oak-deep"),
        tall=(),
        front=("bush-deep", "fern", "grass-tall", "rocks-mossy", "stump", "log", "gloom-mushroom",
               "gloom-mushroom-small", "mushrooms", "fern", "bush-small"),
        wide=("woodcutter-stump",), density=0.7),
    "quarry": dict(
        sky="twilight", far="quarry", ground=(96, 88, 110), island=False, strings=None, kerb=None,
        gap=("tree-blackvein-bare", "tree-blackvein", "fir-deep", "dead-tree", "tree-blackvein-pale", "snag"), tall=(),
        front=("rocks", "rocks-mossy", "rubble-heap", "gloom-mushroom", "gloom-mushroom-small", "fern"),
        wide=("rubble-heap",), density=0.5),
    "camp": dict(
        sky="day", far="camp", ground=(150, 180, 130), island=False, strings="washing", kerb="fence-wood",
        gap=("tent-ragged", "tent-ragged-2", "oak", "tent-ragged"),
        tall=("lantern-post",),
        front=("cot", "refugee-bundle", "crate", "barrel", "sacks", "water-barrel", "crate-open", "bedroll",
               "cookfire", "hay-bale"),
        wide=("healer-station",), density=0.55),
    "deck": dict(
        sky="night", far="sea", ground=(30, 60, 110), island=False, strings=None, kerb=None,
        gap=(), tall=("lantern-hanging",),
        front=("cargo-barrel", "cargo-crate", "rope-coil", "barrel", "crate-stack", "sacks", "cask"),
        wide=(), density=0.45),
    # Underground, though the zone file calls them streets: they get the interiors' walls with
    # their arches open onto the far layer, which is the hall going on behind.
    "crypt-street": dict(
        sky="void", far="deep", wall="crypt", island=False, strings=None, kerb=None, shelves=False,
        hang=("sconce",), front=("urn", "urn-blue", "bones", "urn", "rubble-heap"), density=0.35),
    "vault-street": dict(
        sky="void", far="deep", wall="vault", island=False, strings=None, kerb=None, shelves=False,
        hang=("vault-sconce",), front=("gold-pile-small", "relic-case", "relic-case-orb", "gold-pile", "vault-brazier",
                                       "relic-shelf"), density=0.35),
    "cave-street": dict(
        sky="void", far="deep", wall="quarry", island=False, strings=None, kerb=None, shelves=False,
        hang=(), front=("rocks", "rubble-heap", "gloom-mushroom", "gloom-mushroom-small", "rocks-mossy"), density=0.45),
}

INTERIOR_THEMES = {
    "tavern": dict(wall="interior", shelves=True, hang=("tapestry", "sconce", "sconce"),
                   front=("keg-stack", "barrel", "cask", "cask", "candle-stand", "barrel-water", "crate", "book-crate")),
    "house": dict(wall="interior", shelves=True, hang=("tapestry", "sconce"),
                  front=("bookshelf", "wardrobe", "potted-plant", "book-crate", "side-table", "chair", "barrel",
                         "utensil-rack", "candle-stand")),
    "bakery": dict(wall="interior", shelves=True, hang=("sconce",),
                   front=("utensil-rack", "sacks", "sack-pile", "crate-apples", "barrel", "cask", "potted-plant")),
    "forge": dict(wall="interior", shelves=False, hang=("sconce",),
                  front=("tool-rack", "barrel-water", "crate", "barrel", "anvil", "tool-rack")),
    "shop": dict(wall="interior", shelves=True, hang=("sconce", "tapestry"),
                 front=("bookshelf", "crate-open", "book-crate", "barrel", "potted-plant", "crate-apples", "sacks")),
    "warehouse": dict(wall="interior", shelves=False, hang=("sconce",),
                      front=("crate-stack", "sack-pile", "cargo-barrel", "cargo-crate", "crate", "barrel-salt", "sacks")),
    "chapel": dict(wall="interior", shelves=False, hang=("tapestry", "sun-banner", "sconce"),
                   front=("candle-stand", "potted-plant", "candle-stand", "urn")),
    "keep": dict(wall="keep", shelves=False, hang=("sun-banner", "tapestry"), density=0.3,
                 front=("brazier", "potted-plant", "urn-blue", "potted-plant")),
    "crypt": dict(wall="crypt", shelves=False, hang=("sconce",), front=("urn", "urn-blue", "bones", "urn")),
    "vault": dict(wall="vault", shelves=False, hang=("vault-sconce",),
                  front=("relic-shelf", "gold-pile-small", "relic-case", "relic-case-orb", "gold-pile")),
    "cave": dict(wall="quarry", shelves=False, hang=(),
                 front=("rocks", "rubble-heap", "gloom-mushroom", "gloom-mushroom-small")),
}

ZONE_THEMES = {
    "questeria": "city", "docks": "harbour", "road": "meadow", "mirewood": "forest", "mirewood-quarry": "quarry",
    "thornbridge": "camp", "gulls-wager": "deck", "rusty-anvil": "tavern", "gilded-flagon": "tavern",
    "sunspire-keep": "keep", "barrow": "crypt", "old-crown-vault": "vault", "bakery": "bakery",
    "chapel": "chapel", "cottage-a": "house", "cottage-b": "house", "forge": "forge", "shop": "shop",
    "warehouse": "warehouse",
}
TILESET_THEMES = {"questeria": "city", "meadow": "meadow", "forest": "forest", "mirewood": "forest",
                  "quarry": "quarry", "ship": "deck", "interior": "house", "keep": "keep", "crypt": "crypt",
                  "vault": "vault"}


def theme_of(zone, room):
    """The theme's name and its settings: by the zone, then by the room's tileset."""
    tiles = room.tiles.tileset if room.tiles else None
    name = ZONE_THEMES.get(zone.id) or TILESET_THEMES.get(tiles, "city")
    if room.kind == "interior":
        if name not in INTERIOR_THEMES:
            name = "cave" if name == "quarry" else "house"
        return name, INTERIOR_THEMES[name]
    if name in ("crypt", "vault"):
        name += "-street"
    elif name == "quarry" and room.dark:
        name = "cave-street"
    elif name not in STREET_THEMES:
        name = {"keep": "city", "cave": "cave-street"}.get(name, "city")
    return name, STREET_THEMES[name]


# --------------------------------------------------------------------------- #
# Colour passes
# --------------------------------------------------------------------------- #

def hazed(img, amount, colour, desat=0.5):
    """A copy cooled toward a colour and with some of its own colour taken out: distance."""
    out = img.copy()

    def f(r, g, b):
        y = lum(r, g, b)
        r, g, b = r + (y - r) * desat, g + (y - g) * desat, b + (y - b) * desat
        return (r + (colour[0] - r) * amount, g + (colour[1] - g) * amount, b + (colour[2] - b) * amount)
    out.recolour(f)
    return out


def darkened(img, k, tint, amount):
    out = img.copy()
    out.recolour(lambda r, g, b: (r * k + (tint[0] - r * k) * amount, g * k + (tint[1] - g * k) * amount,
                                  b * k + (tint[2] - b * k) * amount))
    return out


def ramp(stops, t):
    t = max(0.0, min(1.0, t))
    for (t0, c0), (t1, c1) in zip(stops, stops[1:]):
        if t <= t1:
            return mix(c0, c1, (t - t0) / (t1 - t0) if t1 > t0 else 0)
    return stops[-1][1]


def kind_of(r, g, b):
    """What a pixel of a building looks like it is: roof, wall, glass or other."""
    h, s, v = hsv(r, g, b)
    if s < 0.3 and v > 0.55:
        return "wall"
    if s > 0.3 and v > 0.3 and (165 <= h <= 265 or h >= 285 or h <= 12):
        return "roof"
    return "other"


def glass_windows(img, max_w=14, max_h=20):
    """The windows: small patches of dark blue glass with wall or frame all round them. Each is
    (x0, y0, x1, y1, pixels)."""
    w, h, b = img.w, img.h, img.buf
    glass = bytearray(w * h)
    for i in range(w * h):
        o = i * 4
        if b[o + 3] < 200:
            continue
        r, g, bb = b[o], b[o + 1], b[o + 2]
        if bb > r + 25 and bb >= g - 12 and lum(r, g, bb) < 150:
            glass[i] = 1
    seen = bytearray(w * h)
    found = []
    for start in range(w * h):
        if not glass[start] or seen[start]:
            continue
        stack, pix = [start], []
        seen[start] = 1
        while stack:
            i = stack.pop()
            pix.append(i)
            x, y = i % w, i // w
            for j in ((i - 1) if x > 0 else -1, (i + 1) if x < w - 1 else -1, i - w, i + w):
                if 0 <= j < w * h and glass[j] and not seen[j]:
                    seen[j] = 1
                    stack.append(j)
        xs = [i % w for i in pix]
        ys = [i // w for i in pix]
        x0, x1, y0, y1 = min(xs), max(xs) + 1, min(ys), max(ys) + 1
        if len(pix) < 4 or x1 - x0 > max_w or y1 - y0 > max_h or x1 - x0 < 2 or y1 - y0 < 3:
            continue
        ring = solid = 0
        for x in range(x0 - 1, x1 + 1):
            for y in (y0 - 1, y1):
                ring += 1
                if 0 <= x < w and 0 <= y < h and b[(y * w + x) * 4 + 3] > 200 and not glass[y * w + x]:
                    solid += 1
        for y in range(y0, y1):
            for x in (x0 - 1, x1):
                ring += 1
                if 0 <= x < w and b[(y * w + x) * 4 + 3] > 200 and not glass[y * w + x]:
                    solid += 1
        if solid >= 0.8 * ring:
            found.append((x0, y0, x1, y1, pix))
    return found


def light_windows(img, rng, share):
    """Lights some of a building's windows warm, darkest glass deepest."""
    for x0, y0, x1, y1, pix in glass_windows(img):
        if rng.random() > share:
            continue
        for i in pix:
            o = i * 4
            v = lum(img.buf[o], img.buf[o + 1], img.buf[o + 2])
            c = GLASS[0] if v < 70 else GLASS[1] if v < 115 else GLASS[2]
            img.buf[o:o + 3] = bytes(c)


# --------------------------------------------------------------------------- #
# The detail passes
# --------------------------------------------------------------------------- #

class Kit:
    """What the detail passes take from the room's tileset: moss cut out of it (or out of
    Questeria's, if it has no green), and the colours of its cracks."""

    def __init__(self, room):
        own = room.tiles.tileset if room.tiles else "questeria"
        self.stamps, self.crack = [], ((96, 84, 104), (236, 226, 214))
        for name in (own, "questeria"):
            try:
                ts = tileset(name)
            except KeyError:
                continue
            if not self.stamps:
                self.stamps = moss_stamps(ts.sheet)
        try:
            ts = tileset(own)
            stone = [n for n in ("plaza", "floor", "wall-face", "cobble", "marble") if ts.has(n)]
            if stone:
                self.crack = crack_colours(ts.tile_named(stone[0]))
        except KeyError:
            pass
        if not self.stamps:
            self.stamps = [[(0, 0, GRASS[1]), (1, 0, GRASS[2]), (0, 1, GRASS[0])]]


def is_green(r, g, b):
    return g > r + 12 and g > b + 4 and hsv(r, g, b)[1] > 0.25


def moss_stamps(sheet):
    """Little patches of the tileset's own greens, 5x4 at most, kept where they're green."""
    stamps = []
    for ty in range(0, sheet.h, 16):
        for tx in range(0, sheet.w, 16):
            for oy, ox in ((2, 2), (8, 6), (5, 10)):
                cells = []
                for y in range(4):
                    for x in range(5):
                        px, py = tx + ox + x, ty + oy + y
                        if px >= sheet.w or py >= sheet.h:
                            continue
                        r, g, b, a = sheet.get(px, py)
                        if a > 200 and is_green(r, g, b):
                            cells.append((x, y, (r, g, b)))
                if 7 <= len(cells) <= 17:
                    stamps.append(cells)
            if len(stamps) > 40:
                return stamps
    return stamps


def crack_colours(tile):
    """A stone tile's darkest colour, for a crack, and its lightest, for the crack's lit edge."""
    px = [tile.get(x, y)[:3] for y in range(TILE) for x in range(TILE)]
    return min(px, key=lambda c: lum(*c)), max(px, key=lambda c: lum(*c))


def detail(img, kit, rng, roofs=True, boxes=True, moss=True):
    """The detail passes on one building, in place."""
    w, h, b = img.w, img.h, img.buf
    kinds = [None] * (w * h)
    for i in range(w * h):
        o = i * 4
        if b[o + 3] > 128:
            kinds[i] = kind_of(b[o], b[o + 1], b[o + 2])

    if roofs:
        # The shadow under the eaves: the first wall below a run of roof, in each column.
        for x in range(w):
            run = 0
            for y in range(int(h * 0.7)):
                k = kinds[y * w + x]
                if k == "roof":
                    run += 1
                elif k == "wall" and run >= 4 and all(kinds[(y + d) * w + x] == "wall" for d in range(1, 4) if y + d < h):
                    for d, f in enumerate((0.66, 0.76, 0.87, 0.95)):
                        if y + d < h and kinds[(y + d) * w + x] is not None:
                            img.shade(x, y + d, f, SHADOW, 0.14)
                    break
                elif k is None:
                    run = 0
        # The rim of light on the roofs' top and left edges, and a softer second pixel inside it.
        rim = set()
        for y in range(h):
            for x in range(w):
                if kinds[y * w + x] == "roof" and (img.alpha(x, y - 1) < 128 or img.alpha(x - 1, y) < 128):
                    rim.add((x, y))
        for x, y in sorted(rim):
            img.shade(x, y, 1.0, RIM, 0.5)
        for x, y in sorted(rim):
            for nx, ny in ((x + 1, y), (x, y + 1)):
                if 0 <= nx < w and 0 <= ny < h and kinds[ny * w + nx] == "roof" and (nx, ny) not in rim:
                    img.shade(nx, ny, 1.0, RIM, 0.18)

    # Cracks in big stretches of plain wall, lit on their lower edge.
    if moss:
        for _ in range(max(1, w // 40)):
            x, y = rng.randrange(3, max(4, w - 3)), rng.randrange(int(h * 0.3), max(int(h * 0.3) + 1, h - 8))
            if not all(kinds[(y + dy) * w + x + dx] == "wall" for dy in range(-3, 6) for dx in range(-3, 4)
                       if 0 <= x + dx < w and 0 <= y + dy < h):
                continue
            for step in range(rng.randrange(4, 8)):
                img.set(x, y, kit.crack[0])
                if x + 1 < w and kinds[y * w + x + 1] == "wall":
                    img.shade(x + 1, y, 1.0, kit.crack[1], 0.5)
                y += 1
                x += rng.choice((-1, 0, 0, 1))
                if not (0 <= x < w and 0 <= y < h) or kinds[y * w + x] != "wall":
                    break

    # A notice or two pasted up on the wall, low down where people would read them.
    if boxes:
        for _ in range(rng.randrange(0, 3)):
            x, y = rng.randrange(3, max(4, w - 8)), rng.randrange(int(h * 0.55), max(int(h * 0.55) + 1, h - 14))
            if all(kinds[(y + dy) * w + x + dx] == "wall" for dy in range(-1, 8) for dx in range(-1, 7)
                   if 0 <= x + dx < w and 0 <= y + dy < h):
                poster(img, x, y, rng)

    # Window boxes under the windows that hadn't any.
    if boxes:
        windows = glass_windows(img)
        glass = set(i for win in windows for i in win[4])
        for x0, y0, x1, y1, _ in windows:
            if y1 > h * 0.8 or x1 - x0 < 4 or rng.random() > 0.6:
                continue
            # The box goes on the first row of plain wall under the window's sill, so long as
            # there's no more glass between (a window with a bar across it is one window).
            row = None
            for y in range(y1, min(h - 6, y1 + 7)):
                if any(y * w + x in glass for x in range(x0, x1)):
                    break
                if sum(1 for x in range(x0, x1) if kinds[y * w + x] == "wall") >= 0.7 * (x1 - x0):
                    row = y
                    break
            if row is None or row == y1:
                continue
            below = [img.get(x, y) for x in range(x0 - 1, x1 + 1) for y in range(row, min(h, row + 6))]
            if any(a > 128 and (is_green(r, g, bb) or (r > 200 and bb > 140 and g < 190)) for r, g, bb, a in below):
                continue
            window_box(img, x0 - 1, x1 + 1, row + 1, rng)

    # Moss along the foot of the walls and in the corners, keyed onto stone.
    if moss and kit.stamps:
        for _ in range(max(2, w // 16)):
            x = rng.randrange(0, w)
            y = h - 1 - int(abs(rng.gauss(0, h * 0.06)))
            stamp = rng.choice(kit.stamps)
            for dx, dy, c in stamp:
                px, py = x + dx, y - 3 + dy
                if 0 <= px < w and 0 <= py < h and kinds[py * w + px] in ("wall", "other") and \
                        lum(*img.get(px, py)[:3]) > 90:
                    img.set(px, py, c)

    # Where it meets the ground: the bottom rows darker, two tones.
    for x in range(w):
        y = h - 1
        while y >= h - 4 and img.alpha(x, y) < 128:
            y -= 1
        if y < h - 4:
            continue
        for d, f in enumerate((0.7, 0.82, 0.92)):
            img.shade(x, y - d, f, SHADOW, 0.1)


def poster(img, x, y, rng):
    """A notice pasted on a wall: paper, a few lines of writing, a pin, and its shadow."""
    paper = rng.choice(((244, 236, 214), (236, 226, 196), (250, 244, 230)))
    ink = (96, 84, 104)
    for dy in range(6):
        for dx in range(5):
            c = paper
            if dy in (1, 3) and 0 < dx < 4 or dy == 4 and 0 < dx < 3:
                c = ink
            img.set(x + dx, y + dy, c)
    for dx in range(1, 6):
        img.shade(x + dx, y + 6, 0.8, SHADOW, 0.2)
    for dy in range(1, 7):
        img.shade(x + 5, y + dy, 0.8, SHADOW, 0.2)
    img.set(x + 2, y, PETALS[4])


def rim_light(img, amount=0.28):
    """A rim of light along a picture's top and left edges, where the light comes from."""
    edge = [(x, y) for y in range(img.h) for x in range(img.w)
            if img.alpha(x, y) > 128 and (img.alpha(x, y - 1) < 128 or img.alpha(x - 1, y) < 128)]
    for x, y in edge:
        img.shade(x, y, 1.0, RIM, amount)


def window_box(img, x0, x1, y, rng):
    """A flower box: a wooden trough with greens and petals spilling over it, and a trail of ivy."""
    for x in range(x0, x1):
        img.set(x, y + 1, WOOD[4] if x == x0 + 1 else WOOD[3])
        img.set(x, y + 2, WOOD[2])
        img.set(x, y + 3, OUTLINE)
        top = GRASS[rng.randrange(1, 4)] if rng.random() < 0.6 else rng.choice(PETALS)
        img.set(x, y, top)
        if rng.random() < 0.35:
            img.set(x, y - 1, rng.choice(PETALS) if rng.random() < 0.5 else GRASS[3])
    img.set(x0 - 1, y + 1, OUTLINE)
    img.set(x0 - 1, y + 2, OUTLINE)
    img.set(x1, y + 1, OUTLINE)
    img.set(x1, y + 2, OUTLINE)
    for _ in range(rng.randrange(1, 3)):
        x = rng.randrange(x0, x1)
        for d in range(rng.randrange(1, 4)):
            img.set(x, y + 4 + d, GRASS[1] if d % 2 else GRASS[2])


def contact_shadow(img, cx, by, width):
    """A two-tone shadow on the ground where something stands: the object covers its top half."""
    rx = max(3.0, width * 0.55)
    ry = max(1.5, width * 0.07)
    for tone, (k, a) in enumerate(((1.0, 70), (0.72, 120))):
        r_x, r_y = rx * k, ry * (1.0 if tone == 0 else 0.62)
        for dy in range(-int(r_y), int(r_y) + 1):
            half = r_x * math.sqrt(max(0.0, 1 - (dy / (r_y + 0.5)) ** 2))
            for x in range(int(cx - half), int(cx + half) + 1):
                img.blend(x, by + dy, SHADOW, a if tone == 0 else a - 70)


def place(img, src, foot_x, foot_y, shadow=True):
    """Lays a picture down standing on its foot (the middle of its bottom edge), with its shadow."""
    x = int(round(foot_x)) - src.w // 2
    if shadow:
        box = src.bbox()
        width = (box[2] - box[0]) if box else src.w
        contact_shadow(img, foot_x, int(foot_y), width)
    img.over(src, x, int(round(foot_y)) - src.h)


# --------------------------------------------------------------------------- #
# The sky
# --------------------------------------------------------------------------- #

def build_sky(room, theme, rng):
    w, h = layer_size(room, "sky")
    stops = SKIES[theme["sky"]]
    hz = horizon(room) / LAYERS["sky"][0]
    img = Img(w, h)
    levels = 14
    cols = [ramp(stops, i / (levels - 1)) for i in range(levels)]
    for y in range(h):
        t = min(1.0, y / hz) * (levels - 1)
        lo = int(t)
        frac = t - lo
        a, b = cols[lo], cols[min(levels - 1, lo + 1)]
        line = bytearray(w * 4)
        pa, pb = bytes(a + (255,)), bytes(b + (255,))
        for x in range(w):
            line[x * 4:x * 4 + 4] = pb if frac > bayer(x, y) else pa
        img.buf[y * w * 4:(y + 1) * w * 4] = line
    seed = seed_of(room.paint, "stars")
    if theme["sky"] in ("night", "twilight"):
        for y in range(int(hz * 0.75)):
            for x in range(w):
                v = hash01(x, y, seed)
                if v > 0.996:
                    img.set(x, y, (255, 246, 226))
                elif v > 0.990:
                    img.blend(x, y, (220, 214, 255), 120)
    # The title's clouds, two or three, and in Questeria one of its floating islands.
    sheet = art("Title/clouds.png")
    cells = [sheet.crop(i * 132, 0, 132, 60) for i in range(6)]
    count = 2 if room.width <= CANVAS_W else 3
    for i in range(count):
        cloud = cells[rng.randrange(6)]
        if theme["sky"] != "day":
            cloud = darkened(cloud, 0.62 if theme["sky"] == "night" else 0.8, (92, 76, 150), 0.35)
        x = int(w * (i + rng.uniform(0.15, 0.6)) / count) - cloud.w // 2
        y = rng.randrange(4, max(5, int(hz * 0.55)))
        img.over(cloud, x, y)
    if theme.get("island") and rng.random() < 0.6:
        isle = art("Title/island.png")
        img.over(isle, rng.randrange(20, max(21, w - isle.w - 20)), rng.randrange(14, max(15, int(hz * 0.35))))
    return img


# --------------------------------------------------------------------------- #
# The far layer
# --------------------------------------------------------------------------- #

_mountains = {}


def mountains(width):
    """The title's mountains, off tools/source/title-far.png, cut to a width (cached by it). They're
    sampled the way cut_supplied.py samples, so they keep the picture's own colours."""
    if width not in _mountains:
        import cut_supplied
        box = (0, 440, 1536, 330)
        height = int(round(330 * width / 1536))
        rows = cut_supplied.sample("title-far.png", box, (width, height), dark=10)
        img = Img(width, height)
        for y, line in enumerate(rows):
            for x, px in enumerate(line):
                if px[3]:
                    img.set(x, y, px)
        # Only the range itself: the bottoms of the floating islands above it hang into the box.
        keep = bytearray(width * height)
        stack = [(height - 1) * width + x for x in range(width) if img.alpha(x, height - 1)]
        for i in stack:
            keep[i] = 1
        while stack:
            i = stack.pop()
            x, y = i % width, i // width
            for nx, ny in ((x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1)):
                j = ny * width + nx
                if 0 <= nx < width and 0 <= ny < height and not keep[j] and img.alpha(nx, ny):
                    keep[j] = 1
                    stack.append(j)
        for i in range(width * height):
            if not keep[i]:
                img.buf[i * 4:i * 4 + 4] = bytes(4)
        _mountains[width] = img
    return _mountains[width]


def far_prop(name, haze, amount, rng, lit=0.3, desat=0.5):
    img = prop(name).copy()
    if lit:
        light_windows(img, rng, lit)
    out = hazed(img, amount, haze, desat)
    if lit:
        # Lit windows glow through the haze.
        for x0, y0, x1, y1, pix in glass_windows(img):
            for i in pix:
                o = i * 4
                if tuple(img.buf[o:o + 3]) in GLASS:
                    out.buf[o:o + 3] = img.buf[o:o + 3]
    return out


def far_row(img, names, base, haze, amount, rng, x0=-30, x1=None, gap=(-12, 18), lit=0.3, jitter=0):
    """A row of props along the far layer, edge to edge, overlapping a little."""
    x = x0
    x1 = img.w + 30 if x1 is None else x1
    while x < x1:
        name = rng.choice(names)
        if base_name(name) in HOUSES:
            name = roofed(rng, base_name(name))
        p = far_prop(name, haze, amount, rng, lit if base_name(name) in BUILDINGS or base_name(name) in HOUSES else 0)
        img.over(p, x, base - p.h + rng.randrange(0, jitter + 1))
        x += p.w + rng.randrange(*gap)


def ground_band(img, top, colour, haze, amount):
    c = mix(colour, haze, amount)
    dark = mix(c, SHADOW, 0.25)
    for y in range(top, img.h):
        cc = dark if (y - top) % 9 == 8 else c
        img.fill(0, y, img.w, 1, cc)


def sea(img, top, haze, amount, seed, night=False):
    """The sea from the horizon down, out of the harbour's water: palest and haziest at the
    horizon, deeper nearer, in bands, with glints and a bright line where it meets the sky."""
    ramp_ = [mix(c, haze, amount * (1 - i / 6.0)) for i, c in enumerate((WATER[5], WATER[4], WATER[3], WATER[2]))]
    if night:
        ramp_ = [mix(c, (14, 20, 54), 0.6) for c in ramp_]
    for y in range(top, img.h):
        t = min(1.0, (y - top) / 70.0) * (len(ramp_) - 1)
        lo = int(t)
        a, b = ramp_[lo], ramp_[min(len(ramp_) - 1, lo + 1)]
        pa, pb = bytes(a + (255,)), bytes(b + (255,))
        line = bytearray(img.w * 4)
        for x in range(img.w):
            line[x * 4:x * 4 + 4] = pb if t - lo > bayer(x, y) else pa
        img.buf[y * img.w * 4:(y + 1) * img.w * 4] = line
        for x in range(img.w):
            v = hash01(x // 3, y, seed)
            if v > 0.988:
                img.set(x, y, mix(ramp_[0], (255, 255, 255), 0.55))
            elif v < 0.015:
                img.set(x, y, mix(a, SHADOW, 0.25))
    img.fill(0, top, img.w, 1, mix(ramp_[0], (255, 255, 255), 0.4))


def build_far(room, zone, theme, rng):
    w, h = layer_size(room, "far")
    img = Img(w, h)
    sc = LAYERS["far"][0]
    hz = int(horizon(room) / sc)
    haze = HAZES[theme["sky"]]
    kind = theme["far"]
    lit = 0.35 if theme["sky"] == "day" else 0.6

    def scenery_far(base):
        for s in room.scenery:
            if s.layer == "far" and standable(s.prop):
                p = far_prop(s.prop, haze, 0.32, rng, lit)
                img.over(p, int(layer_x(room, "far", s.x) / sc) - p.w // 2, base - p.h)

    if kind != "sea":
        m = hazed(mountains(min(w, 1200) if w > 600 else 600), 0.35 if kind != "forest" else 0.2, haze, 0.25)
        top = hz - int(m.h * (0.62 if kind in ("forest", "quarry", "meadow", "camp") else 0.45))
        for x in range(0, w, m.w):
            img.over(m, x - (rng.randrange(0, 60) if x else 0), top)

    if kind == "city":
        ground_band(img, hz - 4, theme["ground"], haze, 0.45)
        landmarks = ["palace", "temple", "guildhall"]
        rng.shuffle(landmarks)
        if room.id == "sunspire-steps":
            landmarks = ["palace"]
        if any(s.layer == "far" for s in room.scenery):
            landmarks = []
        far_row(img, HOUSES + ("chapel", "cypress", "cherry-tree-small"), hz - 22, haze, 0.55, rng, gap=(-30, 2))
        spots = [int(w * (i + 0.5) / len(landmarks)) for i in range(len(landmarks))]
        for name, x in zip(landmarks, spots):
            if room.width <= CANVAS_W and name != landmarks[0]:
                continue
            p = far_prop(name, haze, 0.42, rng, lit)
            img.over(p, x - p.w // 2 + rng.randrange(-40, 41), hz - 6 - p.h)
        far_row(img, HOUSES + ("cypress", "oak", "blossom-tree", "cherry-tree"), hz + 4, haze, 0.38, rng,
                gap=(-26, 10), jitter=3)
    elif kind == "harbour":
        sea_top = hz - 34
        sea(img, sea_top, haze, 0.45, seed_of(room.paint, "sea"))
        city = hazed(art("Title/city.png").crop(0, 0, 416, 196), 0.28, haze, 0.25)
        left = rng.choice((True, False))
        cx = -60 if left else w - city.w + 60
        img.over(city, cx, sea_top + 4 - city.h)
        for _ in range(2 + w // 400):
            boat = far_prop("rowboat", haze, 0.4, rng, 0)
            img.over(boat, rng.randrange(0, w), rng.randrange(sea_top + 6, sea_top + 40))
    elif kind == "meadow":
        ground_band(img, hz - 10, theme["ground"], haze, 0.4)
        far_row(img, ("oak", "oak-light", "oak-pale", "bush", "great-oak"), hz - 6, haze, 0.5, rng, gap=(-20, 30))
        if room.width > CANVAS_W or rng.random() < 0.7:
            mill = far_prop("windmill", haze, 0.35, rng, 0)
            img.over(mill, rng.randrange(w // 4, 3 * w // 4), hz + 6 - mill.h)
        wall = far_prop("city-wall", haze, 0.45, rng, 0)
        palace = far_prop("palace", haze, 0.5, rng, lit)
        img.over(palace, 30, hz - 16 - palace.h)
        for x in range(-20, w // 3, wall.w - 4):
            img.over(wall, x, hz - wall.h + 2)
        far_row(img, ("farmhouse", "haystack", "oak", "hay-bale", "oak-amber"), hz + 8, haze, 0.3, rng, gap=(30, 160))
    elif kind == "forest":
        ground_band(img, hz - 20, theme["ground"], haze, 0.3)
        far_row(img, ("fir-deep", "fir", "tree-blackvein-pale", "fir-deep"), hz - 18, haze, 0.55, rng, gap=(-14, 0))
        far_row(img, ("tree-blackvein", "oak-turning", "fir", "tree-blackvein-pale", "fir-deep"), hz + 2, haze, 0.38,
                rng, gap=(-30, -6), jitter=4)
    elif kind == "quarry":
        ground_band(img, hz - 10, theme["ground"], haze, 0.3)
        cliff(img, hz, haze, rng)
        far_row(img, ("fir-deep", "tree-blackvein-bare", "fir"), hz - 60, haze, 0.5, rng, gap=(10, 80))
    elif kind == "camp":
        ground_band(img, hz - 10, theme["ground"], haze, 0.4)
        palace = far_prop("palace", haze, 0.5, rng, lit)
        img.over(palace, w // 2 - palace.w // 2 + rng.randrange(-80, 81), hz - 30 - palace.h)
        far_row(img, HOUSES, hz - 26, haze, 0.52, rng, gap=(-30, -6))
        wall = far_prop("city-wall", haze, 0.38, rng, 0)
        gate = far_prop("east-gate", haze, 0.36, rng, 0)
        for x in range(-20, w + 20, wall.w - 4):
            img.over(wall, x, hz - wall.h + 4)
        img.over(gate, rng.randrange(0, max(1, w - gate.w)), hz + 6 - gate.h)
        far_row(img, ("tent-ragged", "tent-ragged-2", "oak", "tent-ragged"), hz + 14, haze, 0.3, rng, gap=(20, 120))
    elif kind == "sea":
        sea(img, hz - 50, haze, 0.25, seed_of(room.paint, "sea"), night=theme["sky"] == "night")
    elif kind == "deep":
        deep_hall(img, theme, seed_of(room.paint, "deep"), room.ambient if room.dark else 1.0)
    scenery_far(hz + 8)
    return img


def deep_hall(img, theme, seed, ambient):
    """The far side of an underground hall, seen through its arches: the same stone, darker and
    further, with a few lamps burning a long way off. The game leaves the far layer out of a dark
    room's dark (so a painted night sky keeps its light), so the hall's painted as dark as the room
    is (its ambient light, and a little of the lamps'), and only the lamps aren't."""
    try:
        ts = tileset(theme["wall"])
    except KeyError:
        return
    tid = ts.ids.get("wall-face")
    if not tid:
        return
    faces = [ts.tile_named("wall-face", v) for v in range(ts.tiles[tid - 1][1])]
    tiled(img, faces, 0, 0, img.w, img.h, seed)
    damp(img, 0, 0, img.w, img.h, seed + 1, 0.3)
    for y in range(img.h):
        k = (0.42 + 0.3 * min(1.0, y / (img.h * 0.7))) * min(1.0, ambient + 0.25)
        for x in range(img.w):
            img.shade(x, y, k, (16, 12, 28), 0.35)
    x = int(hash01(1, 1, seed) * 60) + 30
    while x < img.w:
        y = int(img.h * 0.45 + hash01(x, 2, seed) * 60)
        for r in range(9, 0, -1):
            for dy in range(-r, r + 1):
                for dx in range(-r, r + 1):
                    if dx * dx + dy * dy <= r * r:
                        img.blend(x + dx, y + dy, GLASS[0], 14)
        for dy in range(3):
            img.set(x, y + dy - 1, GLASS[2 - dy])
        x += 120 + int(hash01(x, 3, seed) * 160)


def cliff(img, hz, haze, rng):
    """A quarry's rock face, out of the quarry tileset's wall, ragged along its top."""
    try:
        ts = tileset("quarry")
    except KeyError:
        return
    faces = [hazed(ts.tile_named("wall-face", v), 0.35, haze, 0.3) for v in range(3)]
    top = [hz - 50 - int(18 * math.sin(x / 37.0) + 10 * math.sin(x / 13.0 + 1)) for x in range(img.w)]
    for x in range(img.w):
        for y in range(top[x], img.h):
            t = faces[(x // TILE + y // TILE) % 3]
            img.set(x, y, t.get(x % TILE, y % TILE))
        img.set(x, top[x], mix(faces[0].get(0, 0)[:3], RIM, 0.4))
    damp(img, 0, min(top), img.w, img.h, seed_of("cliff", img.w), 0.25)


# --------------------------------------------------------------------------- #
# The mid layer, outside
# --------------------------------------------------------------------------- #

class Mid:
    """The mid layer as it's built: its image, where its ground is, and what's been put where."""

    def __init__(self, room, kit, rng):
        self.room, self.kit, self.rng = room, kit, rng
        self.w, self.h = layer_size(room, "mid")
        self.img = Img(self.w, self.h)
        self.sc = LAYERS["mid"][0]
        self.base = [int(round(room.back_edge_at(x * self.sc) / self.sc)) for x in range(self.w)]
        self.keep_clear = []   # (x0, x1) the doorways, in art pixels
        for d, (bx, by, bw, bh) in door_boxes(room):
            self.keep_clear.append((bx / self.sc - 4, (bx + bw) / self.sc + 4))
        self.taken = []        # (x0, x1) along the front
        self.doors = {}        # door line x (canvas, rounded) -> its painted doorway (x, y, w, h) in canvas pixels
        self.props = []        # (x0, x1) where the room's own props stand against the wall
        self.tall = []         # (x0, x1) where they reach high up it, or hang on it
        for pr in room.props:
            if prop_exists(pr.prop) and pr.y < room.back_edge_at(pr.x) + 100:
                half = prop(pr.prop).w * 4 / 2
                span = ((pr.x - half) / self.sc, (pr.x + half) / self.sc)
                self.props.append(span)
                if pr.y - prop(pr.prop).h * 4 < room.back_edge_at(pr.x) - 180 or pr.y <= room.back_edge_at(pr.x):
                    self.tall.append(span)

    def base_at(self, x):
        return self.base[max(0, min(self.w - 1, int(x)))]

    def painted(self, door, x0, y0, w, h):
        """Notes where a door's doorway was painted, given in art pixels."""
        self.doors[round(door.x)] = tuple(int(round(v * self.sc)) for v in (x0, y0, w, h))

    def painted_prop(self, door, name, foot, base):
        """Notes the doorway of a door prop standing on its foot (art pixels) on the ground at base."""
        dc, w, lift, h = DOOR_ART[base_name(name)]
        centre = int(round(foot)) + DOORS[base_name(name)] + dc + 0.5
        self.painted(door, centre - w / 2, base - lift - h, w, h)

    def clear(self, x0, x1, taken=True, props=False):
        if any(a < x1 and x0 < b for a, b in self.keep_clear):
            return False
        if props and any(a < x1 and x0 < b for a, b in (self.tall if props == "tall" else self.props)):
            return False
        return not taken or not any(a < x1 - 2 and x0 + 2 < b for a, b in self.taken)


def door_for(mid, room, name, foot_canvas):
    """Which door line a building's door should be on: the nearest one its picture covers."""
    off = DOORS.get(base_name(name))
    if off is None:
        return None
    half = prop(name).w * mid.sc / 2
    best = None
    for d, box in door_boxes(room):
        dist = abs(d.x - (foot_canvas + off * mid.sc))
        if abs(d.x - foot_canvas) <= half + 30 and (best is None or dist < best[0]):
            best = (dist, d)
    return best[1] if best else None


def build_street_mid(room, zone, theme_name, theme, kit, rng):
    m = Mid(room, kit, rng)
    img = m.img

    # The scenery mid lines: buildings onto their doors, everything else where it says.
    placed, used, mounds = [], set(), []
    for s in room.scenery:
        if s.layer != "mid" or not standable(s.prop):
            continue
        foot = s.x
        d = door_for(m, room, s.prop, s.x)
        if d is not None and id(d) not in used:
            used.add(id(d))
            foot = d.x - DOORS[base_name(s.prop)] * m.sc
        else:
            d = None
        placed.append((s.prop, foot / m.sc, d))
    # A door no building stands over gets a doorway of its own, unless the room's own props have
    # one there (a stair, a gate) or it's a stair, which the indicator marks on the floor.
    for d, box in door_boxes(room):
        if id(d) in used or d.badge in ("stair-up", "stair-down"):
            continue
        if any(abs(foot * m.sc - d.x) < prop(n).w * m.sc / 2 for n, foot, _ in placed):
            continue
        if any(abs(pr.x - d.x) < 120 and (base_name(pr.prop) in DOORS or "stair" in pr.prop) for pr in room.props):
            continue
        if theme_name in ("meadow", "camp") and d.badge in ("gate", "crown"):
            mounds.append((d.x / m.sc, d))
            continue
        name = {"forest": "quarry-mouth", "quarry": "quarry-mouth", "harbour": "door"}.get(
            theme_name, "keep-arch" if d.badge in ("gate", "crown") else "door")
        placed.append((name, d.x / m.sc, d))

    spans = []
    for x, d in mounds:
        spans.append((x - 60, x + 60, "mound", x, d))
    for name, foot, d in placed:
        p = prop(name)
        box = p.bbox() or (0, 0, p.w, p.h)
        x0 = foot - p.w // 2 + box[0]
        spans.append((x0, x0 + box[2] - box[0], name, foot, d))
    spans.sort(key=lambda s: s[:4])

    # Behind them: whatever fills the gaps, standing a little further back.
    gaps, at = [], -40
    for x0, x1, _, _, _ in spans:
        if x0 > at:
            gaps.append((at, x0))
        at = max(at, x1)
    gaps.append((at, m.w + 40))
    fill_gaps(m, theme_name, theme, gaps, spans)

    # The buildings and the trees, with their detail.
    for x0, x1, name, foot, d in spans:
        if name == "mound":
            mound_door(m, foot)
            m.painted_prop(d, "door", foot, m.base_at(foot) + 1)
            continue
        p = prop(name).copy()
        is_tree = base_name(name) in TREES
        detail(p, kit, rng, roofs=not is_tree, boxes=base_name(name) in BUILDINGS, moss=not is_tree)
        if is_tree:
            rim_light(p)
        place(img, p, foot, m.base_at(foot))
        if d is not None and base_name(name) in DOOR_ART:
            m.painted_prop(d, name, foot, m.base_at(foot))

    berths(m)
    strings(m, theme, [s[:4] for s in spans])
    dress_front(m, theme_name, theme)
    return img, m.doors


def berths(m):
    """Where a ship's moored at the back of the floor: the harbour's water under her and a stone
    coping along the edge of the quay in front, so her hull sits in the water at its waterline
    instead of ending flat on the cobbles. Nothing's stacked along the quay there."""
    img = m.img
    for pr in m.room.props:
        if pr.prop != "ship" or pr.y > m.room.back_edge_at(pr.x) + 40:
            continue
        ship = prop("ship")
        cols = [x for x in range(ship.w) if any(ship.alpha(x, y) > 128 for y in range(ship.h))]
        lowest = max(y for y in range(ship.h) for x in cols[::4] if ship.alpha(x, y) > 128)
        left = (pr.x + (cols[0] - ship.w // 2) * 4) / m.sc - 4
        right = (pr.x + (cols[-1] + 1 - ship.w // 2) * 4) / m.sc + 4
        hull = (pr.y - (ship.h - 1 - lowest) * 4) / m.sc
        seed = seed_of(m.room.paint, "berth")
        for x in range(int(left), int(right)):
            base = m.base_at(x)
            top = int(hull) - 7
            for y in range(top, base - 2):
                t = (y - top) / max(1, base - 2 - top)
                c = WATER[4] if t < 0.3 else WATER[3] if t < 0.7 else WATER[2]
                if hash01(x // 3, y, seed) > 0.93:
                    c = WATER[5]
                img.set(x, y, c)
            # The waterline along her hull: foam where it laps, and her shadow on the water under it.
            for y, c, a in ((int(hull), WATER[5], 255), (int(hull) + 1, (240, 250, 255), 140),
                            (int(hull) + 2, SHADOW, 90), (int(hull) + 3, SHADOW, 50)):
                if hash01(x, y, seed + 1) > 0.25:
                    img.blend(x, y, c, a)
            for y, c in ((base - 2, (236, 226, 214)), (base - 1, (198, 186, 194)), (base, (150, 140, 164)), (base + 1, OUTLINE)):
                img.set(x, y, c)
            if x % 11 == 0:
                img.set(x, base - 1, (150, 140, 164))
                img.set(x, base, OUTLINE)
        m.taken.append((left, right))


def mound_door(m, cx):
    """A barrow's way in: a hump of the meadow's mossy rocks with a bush on top, and the barrow's
    stone door set in the middle of it."""
    img, rng = m.img, m.rng
    base = m.base_at(cx)
    for dx, lift, name in ((-44, 0, "rocks-mossy"), (40, 0, "rocks-mossy"), (-24, 14, "rocks"), (22, 12, "rocks-mossy"),
                           (0, 26, "rocks-mossy"), (-8, 40, "bush-deep"), (30, 30, "bush-small")):
        p = prop(name).copy()
        rim_light(p, 0.2)
        place(img, p, cx + dx, base - lift, shadow=lift == 0)
    door = prop("door").copy()
    detail(door, m.kit, rng, roofs=False, boxes=False, moss=True)
    place(img, door, cx, base + 1)


def fill_gaps(m, theme_name, theme, gaps, spans):
    img, rng = m.img, m.rng
    if theme_name in ("forest", "quarry"):
        # The wood: two rows of trees all the way along, the back one deeper and a little hazed.
        # At the quarry it's thinner, and dead.
        names = list(theme["gap"])
        if m.room.id in ("edge",):
            names = ["oak-deep", "oak-light", "fir", "fir-deep", "oak-turning", "tree-blackvein-pale"]
        thin = 1.0 if theme_name == "forest" else 1.8
        for row, (lift, haze_k) in enumerate(((14, 0.22), (4, 0.0))):
            x = -40 + rng.randrange(0, 40)
            while x < m.w + 40:
                name = rng.choice(names)
                p = prop(name).copy()
                detail(p, m.kit, rng, roofs=False, boxes=False, moss=False)
                rim_light(p, 0.22)
                if haze_k:
                    p = hazed(p, haze_k, HAZES["twilight"], 0.2)
                if m.clear(x, x + p.w, taken=False) or row == 0 or p.w < 40:
                    place(img, p, x + p.w // 2, m.base_at(x + p.w // 2) - lift, shadow=row == 1)
                x += int(p.w * rng.uniform(0.45, 0.8) * thin)
        return
    if theme_name == "deck":
        deck_mid(m, theme)
        return
    if theme_name == "camp":
        wall = prop("city-wall")
        for x in range(-20, m.w + 20, wall.w - 4):
            img.over(wall, x, m.base_at(x) - 18 - wall.h)
    for a, b in gaps:
        width = b - a
        if width < 20:
            continue
        # Very wide gaps get a house or two standing back behind the garden wall, partly hidden.
        if theme_name == "city" and width > 260:
            x = a + 30
            while x < b - 90:
                name = roofed(rng, rng.choice(HOUSES))
                p = hazed(prop(name), 0.1, HAZES["day"], 0.1)
                detail(p, m.kit, rng, boxes=True)
                place(img, p, x + p.w // 2, m.base_at(x) - 12, shadow=False)
                x += p.w + rng.randrange(30, 90)
        # Trees and the like, behind.
        x = a + rng.randrange(-10, 10)
        while x < b - 10 and theme["gap"]:
            name = rng.choice(theme["gap"])
            p = prop(name).copy()
            detail(p, m.kit, rng, roofs=base_name(name) in BUILDINGS, boxes=False, moss=False)
            if base_name(name) in TREES:
                rim_light(p)
            foot = x + p.w // 2
            if foot > b + 20:
                break
            place(img, p, foot, m.base_at(foot) - (6 if base_name(name) in TREES else 2))
            x += int(p.w * rng.uniform(0.6, 1.0)) + rng.randrange(4, 24)
        # A low wall or fence along the back of the gap.
        kerb = theme.get("kerb")
        if kerb and prop_exists(kerb):
            k = prop(kerb)
            x = a
            while x < b:
                place(img, k, x + k.w // 2, m.base_at(x + k.w // 2) - 1, shadow=False)
                x += k.w - 2


def deck_mid(m, theme):
    """The Gull's Wager's far rail all along the back of the deck, her masts and their rigging, and
    lanterns hung on the rail."""
    img, rng = m.img, m.rng
    try:
        ts = tileset("ship")
        rail = ts.tile_named("rail-n", 0)
    except KeyError:
        rail = None
    for x in range(m.w):
        b = m.base_at(x)
        if rail is not None:
            for y in range(TILE):
                c = rail.get(x % TILE, y)
                if c[3] > 128:
                    img.set(x, b - TILE + y, c)
    masts = []
    standing = [pr.x / m.sc for pr in m.room.props if pr.prop == "mast"]
    x = 260 + rng.randrange(0, 80)
    while x < m.w - 120:
        if not any(abs(x - s) < 140 for s in standing):
            masts.append(x)
        x += 340 + rng.randrange(0, 120)
    for x in masts:
        p = prop("mast")
        top = m.base_at(x) - p.h
        for side in (-1, 1):
            for k in range(3):
                rope(img, x + side * 2, top + 30, x + side * (60 + k * 22), m.base_at(x) - TILE, 3 + k)
        place(img, p, x, m.base_at(x) + 2)
    x = 90
    while x < m.w - 40:
        if m.clear(x - 6, x + 6, taken=False):
            place(img, prop("lantern-hanging"), x, m.base_at(x) - 6, shadow=False)
        x += 150 + rng.randrange(0, 60)


def rope(img, x0, y0, x1, y1, sag):
    """A line between two points that sags in the middle."""
    n = max(abs(x1 - x0), abs(y1 - y0), 1)
    last = None
    for i in range(n + 1):
        t = i / n
        x = int(round(x0 + (x1 - x0) * t))
        y = int(round(y0 + (y1 - y0) * t + sag * 4 * t * (1 - t)))
        if (x, y) != last:
            img.set(x, y, ROPE)
            last = (x, y)


def strings(m, theme, spans):
    """Strings of lanterns, or washing lines, from one building's eaves to the next."""
    kind = theme.get("strings")
    if not kind:
        return
    img, rng = m.img, m.rng
    buildings = [s for s in spans if base_name(s[2]) in BUILDINGS or base_name(s[2]) in HOUSES]
    for (ax0, ax1, an, af), (bx0, bx1, bn, bf) in zip(buildings, buildings[1:]):
        gap = bx0 - ax1
        if kind == "washing" and not 8 <= gap <= 180:
            continue
        if kind == "lanterns" and gap > 260:
            continue
        ya = m.base_at(af) - int(prop(an).h * rng.uniform(0.42, 0.58))
        yb = m.base_at(bf) - int(prop(bn).h * rng.uniform(0.42, 0.58))
        xa, xb = int(ax1) - 6, int(bx0) + 6
        if xb - xa < 10:
            xa, xb = int(ax1) - 14, int(bx0) + 14
        sag = 4 + (xb - xa) * 0.1
        rope(img, xa, ya, xb, yb, sag)
        n = max(2, (xb - xa) // (11 if kind == "lanterns" else 8))
        for i in range(1, n):
            t = i / n
            x = int(xa + (xb - xa) * t)
            y = int(ya + (yb - ya) * t + sag * 4 * t * (1 - t))
            if kind == "lanterns":
                paper_lantern(img, x, y + 1, rng)
            elif rng.random() < 0.8:
                cloth(img, x, y + 1, rng)


def paper_lantern(img, x, y, rng):
    """A little lit lantern hanging off a string: the harbour's lantern glass, in a paper colour."""
    shell = rng.choice(((244, 150, 186), (250, 214, 110), (255, 168, 72), (190, 170, 240)))
    img.set(x, y, ROPE)
    for dy, row in enumerate(("o.o", "aba", "abc", "aba", ".o.")):
        for dx, ch in enumerate(row):
            c = {"o": OUTLINE, "a": mix(shell, OUTLINE, 0.25), "b": GLASS[2], "c": GLASS[1]}.get(ch)
            if c:
                img.set(x - 1 + dx, y + 1 + dy, c)
    img.blend(x - 2, y + 3, GLASS[1], 70)
    img.blend(x + 2, y + 3, GLASS[1], 70)


def cloth(img, x, y, rng):
    """A piece of washing pegged on a line."""
    c = rng.choice(CLOTH)
    w, h = rng.randrange(4, 7), rng.randrange(5, 9)
    for dy in range(h):
        for dx in range(w):
            edge = dx == 0 or dx == w - 1 or dy == h - 1
            col = OUTLINE if edge else (mix(c, SHADOW, 0.3) if dx == w - 2 else c)
            img.set(x - w // 2 + dx, y + dy, col)
    img.set(x - w // 2 + 1, y - 1, WOOD[1])


def dress_front(m, theme_name, theme):
    """Along the front of the buildings: the tall things at intervals, a stall or two, then the
    small things in between, never in front of a door."""
    img, rng = m.img, m.rng
    front = [n for n in theme["front"] if prop_exists(n)]
    tall = [n for n in theme["tall"] if prop_exists(n)]
    wide = [n for n in theme.get("wide", ()) if prop_exists(n)]
    items = []
    x = rng.randrange(30, 80)
    while x < m.w - 20 and tall:
        name = rng.choice(tall)
        p = prop(name)
        if m.clear(x, x + p.w):
            items.append((x + p.w // 2, name))
            m.taken.append((x, x + p.w))
        x += rng.randrange(110, 190)
    for _ in range(max(1, m.w // 400) if wide else 0):
        name = rng.choice(wide)
        p = prop(name)
        for _try in range(12):
            x = rng.randrange(10, max(11, m.w - p.w - 10))
            if m.clear(x, x + p.w):
                items.append((x + p.w // 2, name))
                m.taken.append((x, x + p.w))
                break
    x = rng.randrange(4, 20)
    while x < m.w and front:
        name = rng.choice(front)
        p = prop(name)
        if rng.random() < theme["density"] and m.clear(x, x + p.w, props=m.room.kind == "interior" or "wall" in theme):
            items.append((x + p.w // 2, name))
            m.taken.append((x, x + p.w))
            x += p.w + rng.randrange(0, 6)
        else:
            x += rng.randrange(6, 22)
    for foot, name in sorted(items):
        p = prop(name).copy()
        if base_name(name) in TREES:
            detail(p, m.kit, rng, roofs=False, boxes=False, moss=False)
        place(img, p, foot, m.base_at(foot) + 2)


# --------------------------------------------------------------------------- #
# The mid layer, inside
# --------------------------------------------------------------------------- #

def build_interior_mid(room, zone, theme_name, theme, kit, rng):
    """An interior's mid layer, or an underground street's (with its arches open)."""
    m = Mid(room, kit, rng)
    img = m.img
    style = theme["wall"]
    try:
        ts = tileset(style if style != "quarry" else "quarry")
    except KeyError:
        ts = tileset("interior")
        style = "interior"
    wall = Img(m.w, m.h)
    if style == "interior":
        bays = timber_wall(m, wall, ts)
    else:
        bays = stone_wall(m, wall, ts, style)
    # Only above the floor's back edge: the strip's floor shows below it.
    for x in range(m.w):
        for y in range(m.base_at(x), m.h):
            o = (y * m.w + x) * 4
            wall.buf[o:o + 4] = b"\x00\x00\x00\x00"
    # An open arch shows the hall behind in shadow, from its edge in.
    if room.kind == "street":
        for y in range(m.h):
            for x in range(m.w):
                if wall.alpha(x, y) == 0 and y < m.base_at(x) and (wall.alpha(x - 1, y) or wall.alpha(x, y - 1)):
                    for d, a in ((0, 150), (1, 90), (2, 40)):
                        if wall.alpha(x + d, y) == 0:
                            wall.blend(x + d, y, SHADOW, a)
    img.over(wall, 0, 0)

    # Doors: each door line's doorway, the size of its box, unless the scenery has a door there.
    doorish = [s.x for s in room.scenery if s.layer == "mid" and base_name(s.prop) in DOORS]
    for d, (bx, by, bw, bh) in door_boxes(room):
        if any(abs(x - d.x) < 60 for x in doorish):
            continue
        m.painted(d, *doorway(img, bx / m.sc, by / m.sc, bw / m.sc, bh / m.sc, style, rng, wall, d.badge))

    # Things on the wall, one bay at a time: under the gallery in the keep, between the windows and
    # the rail anywhere else.
    hang = [n for n in theme["hang"] if prop_exists(n)]
    if style == "keep":
        gy = gallery_y(m)
        for a, b in bays:
            cx = (a + b) // 2
            if not m.clear(a, b, taken=False):
                continue
            if bays.index((a, b)) % 3 != 2:
                for bx in (cx - 34, cx + 14):
                    img.over(prop("sun-banner"), bx, gy + 14)
            else:
                img.over(prop("tapestry"), cx - 17, gy + 16)
            if b - a >= 120:
                for sx in (a + 2, b - 12):
                    img.over(prop("vault-sconce"), sx, gy + 40)
        bays = []
    elif room.kind == "street" and style in ("crypt", "vault"):
        # The arches are open, so there's no wall in them to hang anything on: the lamps go on the
        # pilasters between them.
        for (_, b0), (a1, _) in zip(bays, bays[1:]):
            px = (b0 + a1) // 2
            if hang and m.clear(px - 6, px + 6, taken=False):
                lamp = prop(hang[0])
                img.over(lamp, px - lamp.w // 2, m.base_at(px) - 92)
        bays = []
    for a, b in bays:
        mid_x = (a + b) // 2
        if not m.clear(a, b, taken=False) or not m.clear(mid_x - 20, mid_x + 20, taken=False, props="tall"):
            continue
        if theme.get("shelves") and rng.random() < 0.4 and style == "interior":
            shelf_block(img, ts, mid_x, m.base_at(mid_x) - 104, rng)
        elif style == "interior" and rng.random() < 0.35:
            framed_picture(img, mid_x, m.base_at(mid_x) - 108, rng)
        elif hang and rng.random() < 0.8:
            name = rng.choice(hang)
            p = prop(name)
            y = m.base_at(mid_x) - (118 if name in ("tapestry", "sun-banner") else 92)
            img.over(p, mid_x - p.w // 2, y)
            if name in ("tapestry", "sun-banner"):
                for sx in (mid_x - p.w // 2 - 12, mid_x + p.w // 2 + 4):
                    if "sconce" in theme["hang"] and m.clear(sx, sx + 10, taken=False):
                        img.over(prop("sconce"), sx, y + 10)

    # The scenery mid lines: the furniture standing against the wall.
    for s in room.scenery:
        if s.layer == "mid" and standable(s.prop):
            p = prop(s.prop)
            foot = s.x / m.sc
            for d, _ in door_boxes(room):
                if base_name(s.prop) in DOOR_ART and abs(s.x - d.x) < 60:
                    m.painted_prop(d, s.prop, foot, m.base_at(foot) + 1)
            if style == "keep" and base_name(s.prop) == "sun-throne":
                dais(img, foot, m.base_at(foot), p.w + 30)
            place(img, p, foot, m.base_at(foot) + 1)
            m.taken.append((foot - p.w / 2, foot + p.w / 2))
    dress_front(m, theme_name, dict(front=theme["front"], tall=(), wide=(), density=theme.get("density", 0.4), wall=style))
    return img, m.doors


PICTURES = (("Title/sky.png", "Title/city.png"),
            ("Paintings/red-moon-sky.png", "Paintings/red-moon-moon.png", "Paintings/red-moon-headland.png",
             "Paintings/red-moon-shore.png"),
            ("Paintings/questeria-dawn-sky.png", "Paintings/questeria-dawn-clouds.png", "Paintings/questeria-dawn-city.png"),
            ("Paintings/pier-memory-sky.png", "Paintings/pier-memory-sea.png", "Paintings/pier-memory-pier.png"))
_pictures = {}


def little_picture(layers, w, h):
    """One of the title's or the story's paintings, its layers put together and cut down to w by h
    by averaging, for a picture on a wall."""
    key = (layers, w, h)
    if key not in _pictures:
        whole = None
        for rel in layers:
            if not os.path.isfile(os.path.join(CONTENT, rel)):
                continue
            if whole is None:
                whole = art(rel).copy()
            else:
                whole.over(art(rel), 0, 0)
        out = Img(w, h)
        if whole is not None:
            for ty in range(h):
                for tx in range(w):
                    r = g = b = n = 0
                    for y in range(ty * whole.h // h, (ty + 1) * whole.h // h):
                        for x in range(tx * whole.w // w, (tx + 1) * whole.w // w):
                            c = whole.get(x, y)
                            if c[3]:
                                r, g, b, n = r + c[0], g + c[1], b + c[2], n + 1
                    if n:
                        out.set(tx, ty, (r // n, g // n, b // n))
        _pictures[key] = out
    return _pictures[key]


def framed_picture(img, cx, y, rng):
    """A little painting in a gilt or wooden frame, hung off a nail."""
    w, h = rng.choice(((26, 17), (20, 14), (16, 20)))
    pic = little_picture(rng.choice(PICTURES), w, h)
    frame = rng.choice((GOLD, WOOD[1:5]))
    x0 = cx - w // 2
    for k, c in enumerate((OUTLINE, frame[3], frame[2])):
        for xx in range(x0 - 3 + k, x0 + w + 3 - k):
            img.set(xx, y - 3 + k, frame[3] if k and xx == x0 - 3 + k else c)
            img.set(xx, y + h + 2 - k, OUTLINE if k == 0 else frame[1])
        for yy in range(y - 3 + k, y + h + 3 - k):
            img.set(x0 - 3 + k, yy, c)
            img.set(x0 + w + 2 - k, yy, OUTLINE if k == 0 else frame[1])
    img.over(pic, x0, y)
    rope(img, x0 + 2, y - 3, cx, y - 9, 0)
    rope(img, cx, y - 9, x0 + w - 3, y - 3, 0)
    img.set(cx, y - 10, IRON[2])
    for xx in range(x0 - 2, x0 + w + 3):
        img.shade(xx, y + h + 3, 0.82, SHADOW, 0.15)


def tiled(dst, src_tiles, x0, y0, x1, y1, seed):
    """Fills a box with tiles at their own size, a variant picked per cell."""
    for y in range(max(0, y0), min(dst.h, y1)):
        for x in range(max(0, x0), min(dst.w, x1)):
            cx, cy = (x - x0) // TILE, (y - y0) // TILE
            t = src_tiles[int(hash01(cx, cy, seed) * len(src_tiles)) % len(src_tiles)]
            c = t.get((x - x0) % TILE, (y - y0) % TILE)
            if c[3]:
                dst.set(x, y, c)


def beam(img, x0, x1, y0, y1, lit_top=True):
    """A timber beam across, shaded like the props' wood: lit along the top, outlined."""
    for y in range(y0, y1):
        t = (y - y0) / max(1, y1 - y0 - 1)
        c = WOOD[3] if (y == y0 + 1 and lit_top) else WOOD[2] if t < 0.6 else WOOD[1]
        for x in range(x0, x1):
            img.set(x, y, c)
    for x in range(x0, x1):
        img.set(x, y0, OUTLINE)
        img.set(x, y1 - 1, OUTLINE)


def post(img, x, y0, y1, wide=8):
    for y in range(y0, y1):
        for dx in range(wide):
            c = OUTLINE if dx in (0, wide - 1) else WOOD[3] if dx == 1 else WOOD[1] if dx == wide - 2 else WOOD[2]
            img.set(x + dx, y, c)
    # The knee braces under the ceiling beam.
    for side in (-1, 1):
        for i in range(12):
            bx = x + (wide - 1 if side > 0 else 0) + side * i
            for t in range(3):
                img.set(bx, y0 + 12 - i + t, OUTLINE if t in (0, 2) else WOOD[2])


def timber_wall(m, wall, ts):
    """A plastered wall under a beamed ceiling, panelled to the rail, posts between the bays, out of
    the interior tileset's plaster, window, shelf and panelling. Returns the bays (x0, x1)."""
    rng = m.rng
    plaster = ts.tile_named("wall-face", 0)
    panels = [ts.tile_named("wall-base", v) for v in range(2)] if ts.has("wall-base") else [plaster]
    seed = seed_of(m.room.paint, "wall")
    top_beam = 12
    low = min(m.base)
    rail = low - 50
    plaster_fill(wall, plaster, 0, top_beam, m.w, rail, seed)
    # The shadow the ceiling throws.
    for y, k in ((top_beam, 0.72), (top_beam + 1, 0.82), (top_beam + 2, 0.9), (top_beam + 3, 0.96)):
        for x in range(m.w):
            wall.shade(x, y, k, SHADOW, 0.12)
    tiled(wall, panels, 0, rail + 4, m.w, m.h, seed + 1)
    beam(wall, 0, m.w, 0, top_beam)
    beam(wall, 0, m.w, rail, rail + 5)
    for x in range(m.w):
        b = m.base_at(x)
        for y, c in ((b - 4, OUTLINE), (b - 3, WOOD[1]), (b - 2, WOOD[0]), (b - 1, OUTLINE)):
            wall.set(x, y, c)
    # Joist ends along the ceiling.
    for x in range(14, m.w, 34):
        for y in range(0, top_beam + 3):
            for dx in range(6):
                c = OUTLINE if dx in (0, 5) or y == top_beam + 2 else WOOD[3] if dx == 1 else WOOD[2]
                wall.set(x + dx, y, c)
    # Posts, and the bays between them.
    xs, x = [], rng.randrange(60, 110)
    while x < m.w - 40:
        if m.clear(x - 2, x + 10, taken=False):
            xs.append(x)
        x += rng.randrange(150, 200)
    for x in xs:
        post(wall, x, top_beam, rail)
    edges = [0] + [x + 8 for x in xs] + [m.w]
    starts = [0] + xs + [m.w]
    bays = [(edges[i] + 4, starts[i + 1] - 4) for i in range(len(xs) + 1)]
    window = ts.tile_named("wall-face", 1)
    for a, b in bays:
        if b - a < 50:
            continue
        for wx in ((a + b) // 2 - 40, (a + b) // 2 + 8) if b - a > 140 else ((a + b) // 2 - 16,):
            if m.clear(wx - 4, wx + 36, taken=False) and rng.random() < 0.75:
                big_window(wall, window, wx, top_beam + 26, rng)
    return bays


def plaster_fill(img, tile, x0, y0, x1, y1, seed):
    """Plaster in the tileset's plaster colours: its flat colour and its speckle on the tile's own
    four-pixel pitch, without the beam and post the tile carries along its top and left edge, and
    with the odd damp patch and worn spot so it doesn't read as a grid."""
    flat, speck = tile.get(1, 3)[:3], tile.get(4, 4)[:3]
    damp, worn = mix(flat, SHADOW, 0.12), mix(flat, (255, 255, 255), 0.25)
    for y in range(y0, y1):
        for x in range(x0, x1):
            c = flat
            if (x % 4 == 0 and y % 4 == 0) or (x % 4 == 2 and y % 4 == 2):
                c = speck
            n = hash01(x // 7, y // 5, seed) + 0.35 * hash01(x // 3, y // 3, seed + 1)
            if n > 1.18:
                c = mix(c, damp[:3], 0.6)
            elif n < 0.1:
                c = worn
            img.set(x, y, c)


def big_window(img, tile, x, y, rng):
    """A window two tiles by two, out of the tileset's window, with a frame, a sill and a flower box."""
    for ty in range(2):
        for tx in range(2):
            img.over(tile, x + tx * TILE, y + ty * TILE)
    for i in range(-1, 2 * TILE + 1):
        img.set(x + i, y - 1, OUTLINE)
        img.set(x + i, y - 2, WOOD[2])
    for dx in range(-3, 2 * TILE + 3):
        img.set(x + dx, y + 2 * TILE, WOOD[4] if dx == -3 else WOOD[3])
        img.set(x + dx, y + 2 * TILE + 1, WOOD[1])
        img.set(x + dx, y + 2 * TILE + 2, OUTLINE)
    if rng.random() < 0.6:
        window_box(img, x + 2, x + 2 * TILE - 2, y + 2 * TILE + 3, rng)


def shelf_block(img, ts, cx, y, rng):
    """A run of the tileset's shelves, three wide and two high, with a frame round it."""
    if not ts.has("wall-face"):
        return
    shelf = ts.tile_named("wall-face", 2)
    x = cx - 24
    for ty in range(2):
        for tx in range(3):
            img.over(shelf, x + tx * TILE, y + ty * TILE)
    for i in range(-1, 3 * TILE + 1):
        img.set(x + i, y - 1, OUTLINE)
        img.set(x + i, y + 2 * TILE, OUTLINE)
    for j in range(-1, 2 * TILE + 1):
        img.set(x - 1, y + j, OUTLINE)
        img.set(x + 3 * TILE, y + j, OUTLINE)


def stone_wall(m, wall, ts, style):
    """A wall out of a stone tileset: the keep's marble with pillars and tall windows, the crypt's
    and the vault's carved stone, the quarry's rock. Returns the bays between the pillars or niches."""
    rng = m.rng
    seed = seed_of(m.room.paint, "wall")
    low = min(m.base)

    def tiles(*names):
        out = []
        for n in names:
            if ts.has(n):
                tid = ts.ids[n]
                out += [ts.tile_named(n, v) for v in range(ts.tiles[tid - 1][1])]
        return out

    if style == "keep":
        high, face, base_t, topt = tiles("wall-high"), tiles("wall-face"), tiles("wall-base"), tiles("wall-top")
        tiled(wall, topt, 0, 0, m.w, TILE, seed)
        tiled(wall, high, 0, TILE, m.w, TILE * 3, seed + 1)
        tiled(wall, face, 0, TILE * 3, m.w, low - TILE, seed + 2)
        tiled(wall, base_t, 0, low - TILE, m.w, m.h, seed + 3)
        gallery(wall, gallery_y(m), m.w)
        for x in range(m.w):
            wall.set(x, TILE, GOLD[2])
            wall.set(x, TILE + 1, GOLD[0])
            for k, f in enumerate((0.74, 0.84, 0.93)):
                wall.shade(x, TILE + 2 + k, f, SHADOW, 0.1)
        xs, x = [], rng.randrange(70, 120)
        while x < m.w - 40:
            if m.clear(x - 14, x + 14, taken=False):
                xs.append(x)
            x += 210
        win_hi, win = tiles("window-high"), tiles("window")
        bays = []
        edges = [0] + xs + [m.w]
        for a, b in zip(edges, edges[1:]):
            bays.append((a + 16, b - 16))
            cx = (a + b) // 2
            if win_hi and win and b - a > 70 and m.clear(cx - 20, cx + 20, taken=False):
                top = TILE * 2 + 8
                rows = max(1, (gallery_y(m) - 10 - top) // TILE)
                for wx in (cx - TILE - 1, cx + 1):
                    for j in range(rows):
                        wall.over(win_hi[0] if j < rows - 1 else win[0], wx, top + TILE * j)
                arch(wall, cx - TILE - 3, cx + TILE + 3, top - 3)
        for x in xs:
            tall_pillar(wall, x, TILE + 2, low)
        return [(a, b) for a, b in bays]
    names = {"crypt": ("wall-face", "wall-niche", "wall-carving", "wall-base"),
             "vault": ("wall-face", "wall-crest", "wall-niche", "wall-base"),
             "quarry": ("wall-face", "wall-roots", "wall-crystal", "wall-base")}[style]
    face, extras, base_t = tiles(names[0]), tiles(names[1], names[2]), tiles(names[3])
    tiled(wall, face, 0, 0, m.w, low - TILE, seed)
    tiled(wall, base_t or face, 0, low - TILE, m.w, m.h, seed + 1)
    damp(wall, 0, 0, m.w, low, seed + 5, 0.45 if style == "quarry" else 0.14)
    if style in ("crypt", "vault"):
        bays = arcade(wall, m, face, style, rng, opening=m.room.kind == "street")
        for a, b in bays:
            if extras and b - a > 60 and rng.random() < 0.7:
                wall.over(rng.choice(extras), (a + b) // 2 - TILE // 2, low - TILE * rng.randrange(4, 6))
    else:
        bays, x = [], 40
        while x < m.w - 60:
            for row in (rng.randrange(3, 5), rng.randrange(6, 9)):
                if extras and m.clear(x - 4, x + TILE + 4, taken=False) and rng.random() < 0.55:
                    wall.over(rng.choice(extras), x + rng.randrange(0, 60), low - TILE * row)
            bays.append((x, x + 120))
            x += 140 + rng.randrange(0, 40)
    # The light falls off toward the ceiling, which nothing lights.
    for y in range(low):
        k = 0.5 + 0.5 * min(1.0, y / (low * 0.6))
        if k < 1.0:
            for x in range(m.w):
                wall.shade(x, y, k, SHADOW, 0.25 * (1 - k))
    if style == "quarry":
        cave_ceiling(wall, m, seed)
        cave_details(wall, m, ts, seed)
    else:
        for x in range(m.w):
            for k, f in enumerate((0.6, 0.74, 0.86, 0.94)):
                wall.shade(x, k, f, SHADOW, 0.2)
    return bays


def value_noise(x, y, cell, seed):
    """Smooth repeatable noise, 0..1: hash01 on a grid, blended between the corners."""
    gx, gy = x / cell, y / cell
    x0, y0 = int(gx), int(gy)
    fx, fy = gx - x0, gy - y0
    fx, fy = fx * fx * (3 - 2 * fx), fy * fy * (3 - 2 * fy)
    a, b = hash01(x0, y0, seed), hash01(x0 + 1, y0, seed)
    c, d = hash01(x0, y0 + 1, seed), hash01(x0 + 1, y0 + 1, seed)
    return (a + (b - a) * fx) * (1 - fy) + (c + (d - c) * fx) * fy


def damp(img, x0, y0, x1, y1, seed, depth):
    """Soft darker patches over a stone wall, damp and soot, so its courses don't read as a grid."""
    for y in range(y0, y1):
        for x in range(x0, x1):
            n = value_noise(x, y, 26, seed) * 0.7 + value_noise(x, y, 9, seed + 1) * 0.3
            if n > 0.5:
                img.shade(x, y, 1 - depth * (n - 0.5) * 2, SHADOW, 0.1)


def stone_ramp(tiles_):
    """A stone tileset's darkest, middle and lightest colours, for the masonry drawn to match it."""
    px = sorted((c for t in tiles_ for y in range(TILE) for x in range(TILE) for c in [t.get(x, y)] if c[3]),
                key=lambda c: lum(*c[:3]))
    return tuple(px[int(len(px) * q)][:3] for q in (0.12, 0.5, 0.88))


def arcade(wall, m, face, style, rng, opening=False):
    """Pilasters and round arches along a crypt's or vault's wall, under a cornice, each arch's
    recess in shadow, in the wall's own stone (and gold, in the vault); or, in a room with a far
    layer, open above a low wall onto it. Returns the recesses."""
    dark, mid, light = stone_ramp(face)
    trim = GOLD if style == "vault" else (light, light, mid, light)
    low = min(m.base)
    top, spring = 22, 64
    for x in range(m.w):
        for k, c in enumerate((OUTLINE, light, mid, mid, dark, OUTLINE)):
            wall.set(x, top - 6 + k, trim[2] if style == "vault" and k == 1 else c)
        for k, f in enumerate((0.66, 0.8, 0.92)):
            wall.shade(x, top + k, f, SHADOW, 0.12)
    xs, x = [], rng.randrange(40, 90)
    while x < m.w - 20:
        if m.clear(x - 10, x + 10, taken=False):
            xs.append(x)
        x += rng.randrange(170, 220)
    recesses = []
    edges = [-10] + xs + [m.w + 10]
    for a, b in zip(edges, edges[1:]):
        a0, b0 = a + 8, b - 8
        if b0 - a0 < 30:
            continue
        cx, half = (a0 + b0) / 2, (b0 - a0) / 2
        rise = min(half * 0.55, spring - top - 10)
        for x in range(int(a0), int(b0)):
            dx = (x - cx) / half
            arc = spring - rise * math.sqrt(max(0.0, 1 - dx * dx))
            for y in range(int(arc) + 3, low - TILE):
                if opening and y < low - TILE * 3:
                    wall.set(x, y, (0, 0, 0, 0))
                elif opening and y == low - TILE * 3:
                    wall.set(x, y, light)
                else:
                    wall.shade(x, y, 0.8, SHADOW, 0.16)
            for k, c in enumerate((OUTLINE, light, trim[2], mid, OUTLINE)):
                wall.set(x, int(arc) - 2 + k, c)
        recesses.append((int(a0) + 6, int(b0) - 6))
    for x in xs:
        for y in range(top, low - TILE + 4):
            for dx in range(-7, 8):
                c = OUTLINE if abs(dx) == 7 else light if dx == -6 else dark if dx >= 5 else mid
                wall.set(x + dx, y, c)
        for y0, wide in ((top, 9), (low - TILE - 4, 9)):
            for y in range(y0, y0 + 5):
                for dx in range(-wide, wide + 1):
                    c = OUTLINE if abs(dx) == wide or y in (y0, y0 + 4) else trim[2] if style == "vault" and y == y0 + 1 else light if dx < 0 else mid
                    wall.set(x + dx, y, c)
    return recesses


def cave_details(wall, m, ts, seed):
    """Off the quarry tileset's detail tiles: roots hanging from the roof, fungus glowing low on the
    wall, and crystal shards caught in the rock."""
    def stamp(name, x, y, variant):
        if ts.has(name):
            wall.over(ts.tile_named(name, variant), x, y)
    x = 6
    while x < m.w - 16:
        top = 0
        while top < m.h and wall.get(x + 8, top)[:3] in ((30, 24, 40), (52, 44, 66)):
            top += 1
        stamp("roots", x, top - 2, int(hash01(x, 1, seed) * 2))
        if hash01(x, 2, seed) > 0.5:
            stamp("roots", x + 3, top + 12, int(hash01(x, 3, seed) * 2))
        x += 26 + int(hash01(x, 4, seed) * 50)
    low = min(m.base)
    x = 20
    while x < m.w - 16:
        y = low - TILE - 10 - int(hash01(x, 5, seed) * 24)
        for r in range(10, 0, -2):
            for dy in range(-r, r + 1):
                for dx in range(-r, r + 1):
                    if dx * dx + dy * dy <= r * r:
                        wall.blend(x + 8 + dx, y + 8 + dy, (120, 230, 220), 10)
        stamp("fungus", x, y, int(hash01(x, 6, seed) * 2))
        x += 60 + int(hash01(x, 7, seed) * 110)
    for i in range(m.w // 70):
        sx, sy = int(hash01(i, 8, seed) * (m.w - 16)), int(30 + hash01(i, 9, seed) * (low - 90))
        stamp("shards", sx, sy, i % 2)


def cave_ceiling(wall, m, seed):
    """A cave's roof: dark rock coming down raggedly from the top, with an edge of rim light where
    the lamps below catch it, and stalactites hanging off it."""
    rock, dark = (52, 44, 66), (30, 24, 40)
    edge = []
    for x in range(m.w):
        e = int(14 + 9 * math.sin(x / 23.0) + 5 * math.sin(x / 8.0 + 1.3) + 4 * hash01(x // 3, 0, seed))
        edge.append(e)
        for y in range(0, e):
            wall.set(x, y, dark if y < e - 4 else rock)
        wall.set(x, e, OUTLINE)
        wall.set(x, e - 1, mix(rock, RIM, 0.25))
    x = 12
    while x < m.w - 12:
        n = hash01(x, 7, seed)
        width, length = 3 + int(n * 4), 6 + int(n * 14)
        top = edge[x]
        for d in range(length):
            half = max(0, int(width * (1 - d / length) / 2 + 0.5))
            for dx in range(-half, half + 1):
                c = mix(rock, RIM, 0.2) if dx == -half else OUTLINE if dx == half else rock
                wall.set(x + dx, top + d, c)
        x += 22 + int(hash01(x, 3, seed) * 40)


def gallery_y(m):
    """Where the keep's gallery runs along the wall: a little under halfway down it."""
    return min(m.base) - 96


def gallery(img, y, w):
    """The gallery along the keep's wall: a marble ledge on a shadow, and a balustrade with a gold rail."""
    marble = ((250, 246, 250), (232, 224, 238), (206, 196, 216), (170, 158, 186))
    for x in range(w):
        img.set(x, y - 9, OUTLINE)
        img.set(x, y - 8, GOLD[2])
        img.set(x, y - 7, marble[1])
        img.set(x, y - 6, marble[3])
        for k, c in enumerate((marble[0], marble[1], marble[2], marble[3], OUTLINE)):
            img.set(x, y + k, c)
        for k, f in enumerate((0.62, 0.74, 0.86, 0.94)):
            img.shade(x, y + 5 + k, f, SHADOW, 0.15)
        if x % 5 in (1, 2):
            for yy in range(y - 5, y):
                img.set(x, yy, marble[1] if x % 5 == 1 else marble[2])
            img.set(x, y - 3, marble[0] if x % 5 == 1 else marble[2])
        elif x % 5 == 3:
            for yy in range(y - 5, y):
                img.shade(x, yy, 0.8, SHADOW, 0.2)


def arch(img, x0, x1, y):
    """A pointed gold arch over a pair of windows."""
    mid = (x0 + x1) / 2
    half = (x1 - x0) / 2
    for x in range(x0, x1 + 1):
        dy = int(round(6 * (1 - abs(x - mid) / half)))
        img.set(x, y - dy, GOLD[2])
        img.set(x, y - dy + 1, GOLD[0])


def tall_pillar(img, x, y0, y1):
    """The keep's marble pillar run up from floor to cornice: its own capital and base, and its
    shaft repeated between them."""
    p = prop("pillar-marble")
    box = p.bbox() or (0, 0, p.w, p.h)
    cap_h, foot_h = 12, 14
    left = x - p.w // 2
    img.over(p.crop(0, box[1], p.w, cap_h), left, y0)
    shaft = p.crop(0, box[1] + cap_h + 6, p.w, 10)
    y = y0 + cap_h
    while y < y1 - foot_h:
        img.over(shaft.crop(0, 0, p.w, min(10, y1 - foot_h - y)), left, y)
        y += 10
    img.over(p.crop(0, box[3] - foot_h, p.w, foot_h), left, y1 - foot_h)
    for yy in range(y0 + cap_h, y1 - foot_h):
        img.shade(left + p.w - 2, yy, 0.85, SHADOW, 0.1)


def dais(img, cx, by, width):
    """A gold-edged step for the throne to stand on."""
    x0 = int(cx - width / 2)
    for y in range(by - 6, by + 2):
        for x in range(x0, x0 + int(width)):
            c = GOLD[3] if y == by - 6 else GOLD[2] if y == by - 5 else (236, 228, 240) if y < by else GOLD[1]
            img.set(x, y, c)
    for y in range(by - 6, by + 2):
        img.set(x0, y, OUTLINE)
        img.set(x0 + int(width) - 1, y, OUTLINE)


def stair_opening(img, x, y, w, h, badge, wall):
    """A stair going up or down out of a room: a dark opening in a stone frame with its steps lit
    one above the other, going up into it or down out of sight. Returns its box, in art pixels."""
    x0, y0, x1, y1 = int(x + 2), int(y), int(x + w - 2), int(y + h)
    stone = (196, 188, 204)
    if wall is not None:
        cs = [wall.get(int((x0 + x1) / 2) + dx, max(0, y0 - 6)) for dx in range(-8, 9)]
        stone = max((c[:3] for c in cs if c[3]), key=lambda c: lum(*c), default=stone)
    for yy in range(y0, y1):
        for xx in range(x0, x1):
            edge = xx in (x0, x0 + 1, x1 - 2, x1 - 1) or yy in (y0, y0 + 1)
            img.set(xx, yy, OUTLINE if xx in (x0, x1 - 1) or yy == y0 else stone if edge else (24, 18, 34))
    steps = max(3, (y1 - y0) // 6)
    for i in range(steps):
        sy = (y1 - 2 - i * 5) if badge == "stair-up" else (y0 + 8 + i * 5)
        width = (x1 - x0 - 6) - (i * 2 if badge == "stair-up" else -i * 0)
        if sy <= y0 + 3 or sy >= y1 - 1:
            continue
        shade = mix(stone, (24, 18, 34), (i / steps) * (0.6 if badge == "stair-up" else 0.9))
        for xx in range(x0 + 3, x0 + 3 + width):
            img.set(xx, sy, shade)
            img.set(xx, sy + 1, mix(shade, OUTLINE, 0.5))
    return x0, y0, x1 - x0, y1 - y0


def doorway(img, x, y, w, h, style, rng, wall=None, badge="door"):
    """A door the size of its box: planks and iron bands, in a wooden frame indoors and a stone one
    underground (the wall's own lightest stone), or the keep's blue and gold doors. Returns the
    doorway's box, frame and all, in art pixels."""
    x0, y0, x1, y1 = int(x + w * 0.12), int(y + h * 0.18), int(x + w * 0.88), int(y + h)
    mid = (x0 + x1) / 2
    half = (x1 - x0) / 2
    if badge in ("stair-up", "stair-down"):
        return stair_opening(img, x, y, w, h, badge, wall)
    keep = style in ("keep", "vault")
    body = NAVY if keep else WOOD
    trim = GOLD if keep else IRON
    frame = (236, 228, 240) if keep else WOOD[2]
    if style in ("crypt", "vault", "quarry") and wall is not None:
        # The frame's the wall's own lightest stone.
        stones = [wall.get(int(mid) + dx, max(0, y0 - 8)) for dx in range(-8, 9)]
        frame = max((c[:3] for c in stones if c[3]), key=lambda c: lum(*c), default=(150, 146, 160))
    for yy in range(y0 - 4, y1):
        for xx in range(x0 - 4, x1 + 4):
            ry = y0 + half * 0.6
            if yy < ry:
                dx = (xx - mid) / (half + 4)
                if dx * dx + ((yy - ry) / (half * 0.6 + 4)) ** 2 > 1:
                    continue
            inside = x0 <= xx < x1 and yy >= y0 and (yy >= ry or ((xx - mid) / half) ** 2 + ((yy - ry) / (half * 0.6)) ** 2 <= 1)
            if not inside:
                img.set(xx, yy, OUTLINE if xx in (x0 - 4, x1 + 3) else frame)
                continue
            plank = int((xx - x0) / max(1, (x1 - x0) / 4))
            c = body[2] if plank % 2 else body[1]
            if xx == x0 or (xx - x0) % max(1, (x1 - x0) // 4) == 0:
                c = OUTLINE
            if keep and int(mid) - 1 <= xx <= int(mid):
                c = GOLD[2]
            if yy in (int(y0 + (y1 - y0) * 0.3), int(y0 + (y1 - y0) * 0.75)):
                c = trim[1]
            img.set(xx, yy, c)
    ring = (int(mid) + (3 if not keep else 4), int(y0 + (y1 - y0) * 0.55))
    for dx, dy in ((0, 0), (1, 0), (0, 1), (1, 1), (-1, 1), (2, 1), (0, 2), (1, 2)):
        img.set(ring[0] + dx, ring[1] + dy, GOLD[2] if keep else IRON[2])
    for xx in range(x0 - 5, x1 + 5):
        img.set(xx, y1 - 1, (230, 220, 210) if keep else WOOD[1])
        img.set(xx, y1 - 2, OUTLINE)
    return x0 - 4, y0 - 4, x1 - x0 + 8, y1 - y0 + 4


# --------------------------------------------------------------------------- #
# The foreground
# --------------------------------------------------------------------------- #

def build_fore(room, theme, rng):
    w, h = layer_size(room, "fore")
    sc = LAYERS["fore"][0]
    lines = [s for s in room.scenery if s.layer == "fore" and standable(s.prop)]
    top, bottom = band(room)
    rows = (int(top // sc), int(math.ceil(bottom / sc)))
    dark_tint = (26, 20, 46) if room.kind == "street" else (34, 22, 30)
    rim = (150, 140, 200) if theme.get("sky", "day") == "day" else (110, 96, 170)
    while True:
        img = Img(w, h)
        for s in lines:
            p = prop(s.prop)
            d = darkened(p, 0.36, dark_tint, 0.42)
            # A thin rim of the sky's light on the top-left edges, so it stays crisp against the room.
            for y in range(d.h):
                for x in range(d.w):
                    if d.alpha(x, y) > 128 and (d.alpha(x, y - 1) < 128 or d.alpha(x - 1, y) < 128):
                        d.shade(x, y, 1.0, rim, 0.45)
            sink = max(4, int(p.h * 0.18))
            cx = layer_x(room, "fore", s.x) / sc
            img.over(d, int(round(cx)) - d.w // 2, h + sink - d.h)
        covered = img.solid_count(0, rows[0], w, rows[1])
        share = covered / float(w * max(1, rows[1] - rows[0]))
        if share <= FORE_LIMIT or not lines:
            return img, share
        lines.remove(max(lines, key=lambda s: prop(s.prop).w * prop(s.prop).h))


# --------------------------------------------------------------------------- #
# Composing: the tile strip, previews and thumbnails
# --------------------------------------------------------------------------- #

def tile_strip(room):
    """The room's tile strip at art resolution, drawn the way Tilemap draws it at time zero: the
    ground, the edges where one kind meets another, then the detail."""
    t = room.tiles
    ts = tileset(t.tileset)
    cols, nrows = room.width // 64, len(t.rows)
    img = Img(cols * TILE, nrows * TILE)
    ground = [[ts.ids.get(t.legend.get(c, ""), 0) for c in row] for row in t.rows]
    for y in range(nrows):
        for x in range(cols):
            if ground[y][x]:
                img.over(ts.tile(ts.frame_of(ground[y][x], x, y)), x * TILE, y * TILE)
    for y in range(nrows):
        for x in range(cols):
            tid = ground[y][x]
            if not tid or not ts.edged(tid):
                continue
            for side, (dx, dy) in enumerate(((0, -1), (1, 0), (0, 1), (-1, 0))):
                nx, ny = x + dx, y + dy
                if 0 <= nx < cols and 0 <= ny < nrows and ground[ny][nx] and ground[ny][nx] != tid:
                    img.over(ts.tile(ts.edge_frame(tid, side)), x * TILE, y * TILE)
    for y, row in enumerate(t.detail):
        for x, c in enumerate(row):
            tid = ts.ids.get(t.legend.get(c, ""), 0) if c != "." else 0
            if tid:
                img.over(ts.tile(ts.frame_of(tid, x, y)), x * TILE, y * TILE)
    return img


def compose(room, layers, strip, cam_left, people=True, badges=True, boxes=False, doors=None):
    """The room as the game draws it, 1600x900, with the camera's left edge at cam_left. A door's
    badge floats over its painted doorway (doors, from the mid layer), or over its box if nothing's
    painted for it; boxes outlines both, the box dashed and the doorway solid."""
    canvas = Img(CANVAS_W, CANVAS_H, (24, 20, 36, 255))
    for name in ("sky", "far"):
        if name in layers:
            s = LAYERS[name][0]
            canvas.over(layers[name].view(s, int(round(parallax(room, name) * cam_left)), 0, CANVAS_W, CANVAS_H), 0, 0)
    top = room.tiles.top
    canvas.over(strip.view(4, int(cam_left), 0, CANVAS_W, CANVAS_H - top), 0, top)
    canvas.over(layers["mid"].view(3, int(cam_left), 0, CANVAS_W, CANVAS_H), 0, 0)

    # What stands on the floor: the room's props and two townsfolk, back to front.
    standing = []
    for p in room.props:
        if prop_exists(p.prop):
            standing.append((p.y, prop(p.prop).scaled(4), p.x))
    if people:
        t, b = band(room)
        y = int(t + (b - t) * 0.55)
        sheets = sorted(n[:-4] for n in os.listdir(os.path.join(CONTENT, "People")) if n.endswith(".png"))
        r = random.Random(seed_of(room.paint, "people"))
        a, c = r.sample(sheets, 2)
        standing.append((y, walker(a, "right").scaled(4), cam_left + 640))
        standing.append((y + 24, walker(c, "left").scaled(4), cam_left + 980))
    for y, img, x in sorted(standing, key=lambda s: s[0]):
        sx = int(round(x - cam_left))
        if img.h == 48 * 4 and img.w == 32 * 4:
            for dy in range(-10, 11):
                half = 32 * math.sqrt(max(0.0, 1 - (dy / 10.5) ** 2))
                for dx in range(int(-half), int(half) + 1):
                    canvas.blend(sx + dx, int(y) + dy, (0, 0, 0), 128)
            canvas.over(img, sx - WALKER_FEET[0] * 4, int(y) - WALKER_FEET[1] * 4)
        else:
            canvas.over(img, sx - img.w // 2, int(y) - img.h)

    if "fore" in layers:
        canvas.over(layers["fore"].view(4, int(round(parallax(room, "fore") * cam_left)), 0, CANVAS_W, CANVAS_H), 0, 0)

    if badges or boxes:
        for d, (bx, by, bw, bh) in door_boxes(room):
            sx = int(bx - cam_left)
            art = (doors or {}).get(round(d.x))
            if boxes:
                for i in range(int(bw)):
                    for yy in (int(by), int(by + bh) - 1):
                        canvas.blend(sx + i, yy, (255, 255, 255), 200 if (i // 6) % 2 == 0 else 60)
                for j in range(int(bh)):
                    for xx in (sx, sx + int(bw) - 1):
                        canvas.blend(xx, int(by) + j, (255, 255, 255), 200 if (j // 6) % 2 == 0 else 60)
                if art:
                    ax = int(art[0] - cam_left)
                    canvas.fill(ax, art[1], art[2], 2, GOLD[2])
                    canvas.fill(ax, art[1] + art[3] - 2, art[2], 2, GOLD[2])
                    canvas.fill(ax, art[1], 2, art[3], GOLD[2])
                    canvas.fill(ax + art[2] - 2, art[1], 2, art[3], GOLD[2])
            if badges:
                if art:
                    badge(canvas, int(art[0] + art[2] / 2 - cam_left), art[1] - 30 - 22, d.badge)
                else:
                    badge(canvas, sx + int(bw / 2), int(by) - 30 - 22, d.badge)
    return canvas


BADGE_COLOURS = {"shop": GOLD[2], "anvil": GOLD[2], "bottle": GOLD[2], "inn": (236, 150, 60), "trainer": NAVY[3],
                 "scroll": NAVY[3], "shield": NAVY[3], "sun": (250, 248, 240), "door": (250, 248, 240)}


def badge(canvas, cx, cy, kind):
    """A stand-in for the indicator layer's badge, so a preview shows where each door's is."""
    c = BADGE_COLOURS.get(kind, (250, 248, 240))
    for dy in range(-22, 23):
        for dx in range(-22, 23):
            d = math.hypot(dx, dy)
            if d <= 22:
                canvas.set(cx + dx, cy + dy, OUTLINE if d > 19 else mix(c, (255, 255, 255), 0.35) if dy < -8 else c)
    for dy in range(-6, 7):
        for dx in range(-6, 7):
            if abs(dx) + abs(dy) <= 6:
                canvas.set(cx + dx, cy + dy, OUTLINE)


def thumbnail(canvas):
    """The composed room cut down to 200x113, averaging each patch of pixels."""
    out = Img(THUMB_W, THUMB_H)
    b, w = canvas.buf, canvas.w
    for ty in range(THUMB_H):
        y0, y1 = ty * CANVAS_H // THUMB_H, (ty + 1) * CANVAS_H // THUMB_H
        for tx in range(THUMB_W):
            x0, x1 = tx * CANVAS_W // THUMB_W, (tx + 1) * CANVAS_W // THUMB_W
            r = g = bb = n = 0
            for y in range(y0, y1, 2):
                o = (y * w + x0) * 4
                for x in range(x0, x1, 2):
                    r += b[o]
                    g += b[o + 1]
                    bb += b[o + 2]
                    n += 1
                    o += 8
            out.set(tx, ty, (r // n, g // n, bb // n))
    return out


# --------------------------------------------------------------------------- #
# Rooms
# --------------------------------------------------------------------------- #

def build_room(zone, room):
    """Every layer of a room ({name: Img}), its doors' painted doorways, and the foreground's share of the band."""
    theme_name, theme = theme_of(zone, room)
    kit = Kit(room)
    layers = {}
    if room.kind == "street":
        layers["sky"] = build_sky(room, theme, random.Random(seed_of(room.paint, "sky")))
        layers["far"] = build_far(room, zone, theme, random.Random(seed_of(room.paint, "far")))
        build = build_interior_mid if "wall" in theme else build_street_mid
    else:
        build = build_interior_mid
    layers["mid"], doors = build(room, zone, theme_name, theme, kit, random.Random(seed_of(room.paint, "mid")))
    layers["fore"], share = build_fore(room, theme, random.Random(seed_of(room.paint, "fore")))
    return theme_name, layers, doors, share


def door_lines(room, doors):
    """doors.txt: each door's painted doorway, in canvas pixels. A locked twin shares its door's."""
    lines = ["; The painted doorway of each door in %s: id, then x y width height in canvas pixels" % room.paint,
             "; (the game divides by RoomScale). Written by tools/rooms/generate_room_placeholders.py; a door",
             "; that isn't here has nothing painted for it, and the game uses its box."]
    for d in room.doors:
        box = doors.get(round(d.x))
        if box:
            lines.append("%s %d %d %d %d" % ((d.id,) + box))
    return lines


MGCB_BLOCK = (
    "#begin {0}",
    "/importer:TextureImporter",
    "/processor:TextureProcessor",
    "/processorParam:ColorKeyColor=255,0,255,255",
    "/processorParam:ColorKeyEnabled=False",
    "/processorParam:GenerateMipmaps=False",
    "/processorParam:PremultiplyAlpha=True",
    "/processorParam:ResizeToPowerOfTwo=False",
    "/processorParam:MakeSquare=False",
    "/processorParam:TextureFormat=Color",
    "/processorParam:Version=3.8.5.1",
    "/build:{0}",
    "",
)

#: What each layer is, for ASSETS.md.
LAYER_WORDS = {
    "sky": "its sky at 4x",
    "far": "its far layer at 2x (the skyline, cooled and hazed)",
    "mid": "its mid layer at 3x (the buildings on their doors, or the wall, and the dressing)",
    "fore": "its foreground at 4x",
    "thumb": "its 200x113 thumbnail for the zone map",
    "doors": "and doors.txt, where each door's doorway is painted",
}


def manifest(folder, rooms):
    """Writes what Content.mgcb and ASSETS.md need for every file painted: rooms-mgcb.txt, the
    blocks in Content.mgcb's own format, and rooms-assets.txt, a paragraph a room in ASSETS.md's."""
    mgcb, assets = [], []
    for zone, room, names in rooms:
        paths = ["Rooms/%s/%s.png" % (room.paint, n) for n in names] + ["Rooms/%s/doors.txt" % room.paint]
        for rel in paths[:-1]:
            mgcb += [line.format(rel) for line in MGCB_BLOCK]
        mgcb += ["#begin " + paths[-1], "/copy:" + paths[-1], ""]
        assets.append("%s - created by Caleb Kiatoukaysi for Legend Quest, %s in %s: %s, put together "
                      "procedurally by tools/rooms/generate_room_placeholders.py from the game's own props, tiles and "
                      "title pictures, released under public domain" % (
                          ", ".join("Content/" + rel for rel in paths), room.name, zone.name,
                          ", ".join(LAYER_WORDS[n] for n in names + ["doors"])))
        assets.append("")
    os.makedirs(folder, exist_ok=True)
    for name, lines in (("rooms-mgcb.txt", mgcb), ("rooms-assets.txt", assets)):
        with open(os.path.join(folder, name), "w", encoding="utf-8", newline="\r\n") as f:
            f.write("\n".join(lines))
        print("  " + os.path.join(folder, name))


def wanted(zone, room, picks):
    return not picks or zone.id in picks or "%s/%s" % (zone.id, room.id) in picks


def main():
    ap = argparse.ArgumentParser(description="Paint every room's layers out of the game's own art.")
    ap.add_argument("only", nargs="*", help="zones or zone/room to paint (default: all)")
    ap.add_argument("--zones", default=os.path.join(CONTENT, "Zones"), help="the folder of zone files")
    ap.add_argument("--out", default=OUT, help="where Rooms/<paint>/ goes (default Content/Rooms)")
    ap.add_argument("--preview", help="also write 1600x900 previews into this folder (not in Content)")
    ap.add_argument("--boxes", action="store_true", help="outline the door boxes in the previews")
    ap.add_argument("--manifest", help="write the Content.mgcb blocks and ASSETS.md lines for the rooms into this folder")
    args = ap.parse_args()

    if args.preview and os.path.abspath(args.preview).startswith(os.path.abspath(CONTENT)):
        sys.exit("Previews don't go in Content: name a folder outside it.")
    files = zone_files(args.zones)
    if not files:
        sys.exit("No zone files in %s." % args.zones)
    print("Painting the rooms for Legend Quest...")
    painted = []
    for path in files:
        zone = load_zone(path)
        for room in zone.rooms:
            if not wanted(zone, room, args.only):
                continue
            theme_name, layers, doors, share = build_room(zone, room)
            folder = os.path.join(args.out, *room.paint.split("/"))
            for name in ("sky", "far", "mid", "fore"):
                if name in layers:
                    layers[name].save(os.path.join(folder, name + ".png"))
            with open(os.path.join(folder, "doors.txt"), "w", encoding="utf-8", newline="\r\n") as f:
                f.write("\n".join(door_lines(room, doors)) + "\n")
            strip = tile_strip(room)
            middle = max(0, (room.width - CANVAS_W) // 2)
            thumbnail(compose(room, layers, strip, middle, people=False, badges=False)).save(
                os.path.join(folder, "thumb.png"))
            print("    %s/%s: %s, the foreground covers %.1f%% of the band" % (zone.id, room.id, theme_name, share * 100))
            painted.append((zone, room, [n for n in ("sky", "far", "mid", "fore") if n in layers] + ["thumb"]))

            if args.preview:
                base = os.path.join(args.preview, "%s-%s" % (zone.id, room.id))
                compose(room, layers, strip, middle, boxes=args.boxes, doors=doors).save(base + ".png")
                if room.width > CANVAS_W:
                    compose(room, layers, strip, 0, boxes=args.boxes, doors=doors).save(base + "-left.png")
                    compose(room, layers, strip, room.width - CANVAS_W, boxes=args.boxes, doors=doors).save(base + "-right.png")
    if args.manifest:
        manifest(args.manifest, painted)
    print("Done.")


if __name__ == "__main__":
    main()
