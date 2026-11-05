"""Reads and writes the zone files in Content/Zones/*.txt: a zone is a chain of side-view rooms.

This is the one parser and serializer every room tool shares (convert_maps.py, check_rooms.py,
check_spawns.py, the room template and placeholder generators). It's pure: it reads text and builds
dataclasses, and writes them back, with no image code and nothing from the game's other catalogs.
Whether a prop, a cast member or a conversation exists is check_rooms.py's job.

Like World/Zones/ZoneLoader.cs, it fails on the first bad line, saying the file and the line:
``Zones/docks.txt line 42: there's no badge called pub.``

The format
==========

Every coordinate is in canvas pixels: a room is ``width`` across (a multiple of 64, from 1600 to
4800) and 900 tall, and the game divides everything by ``RoomScale`` (4) once, on loading. A ``;``
outside double quotes starts a comment. Blank lines are ignored, and every line is trimmed (so a
Windows line ending is fine). The files are UTF-8 and the tools write them with CRLF endings.

A line is ``head = body · field · field ...``. The separator is the middle dot ``·`` (U+00B7), with
a space each side. A field is a bare word (a flag, like ``mark``) or ``key value``, where the value
is the rest of the field; a value in double quotes has them taken off (there's no escaping, so a
quoted value can't hold a ``"``). Each key may appear once on a line, and a key that a kind doesn't
have is an error, as ZoneLine.Done makes it.

The zone header comes first::

    [zone]
    id = docks                    ; the file's name without .txt
    name = Saltwind Docks
    world = 80 190                ; optional: where it is on the world map
    music = Exploring             ; a Track name
    region = coast                ; optional: for random encounters
    danger = 0                    ; optional, a whole number from 0 (default 0)

Then one section per room, left to right (``[room <id>]``). Its settings, which come before its
objects (``width`` has to come before anything with a position)::

    name = The Harbourside
    kind = street                 ; street (default) or interior: no sky, far layer or parallax
    width = 3200                  ; a multiple of 64, 1600 to 4800
    paint = docks/harbourside     ; optional, default <zone>/<room>: Content/Rooms/<paint>/
    dark = yes                    ; optional, default no
    ambient = 0.35                ; a dark room's light level, 0 to 1 (only, and always, when dark)
    tint = 214 186 164            ; optional, red green blue
    floor = 0 600, 3200 600, 3200 860, 0 860   ; the floor band: a simple polygon, closed implicitly
    block = 1500 700 90 40        ; any number: an ellipse feet can't enter, centre x y, radii x y
    fight = 400 2000              ; optional: the x range a fight stands in (default the middle 1600)
    tiles = questeria             ; the tile strip's tileset (World/TilesetCatalog.cs)
    tiletop = 576                 ; a multiple of 64 from 0: the strip's top. Interiors start at 0
    legend = = planks             ; any number: a character, a space, a tile name in the tileset
    row = ====,,,,~~~~            ; ceil((900 - tiletop) / 64) rows, top to bottom, width/64 each
    detail = ....f.....           ; optional: as many as the rows, '.' is an empty cell
    legacy = 18 0 44 36           ; any number: x0 y0 x1 y1 of the old map's tiles (x1, y1 exclusive)
    when protectorate = act3      ; any number: a room state and the flag that turns it on (reserved)

``row`` and ``detail`` characters can be anything but a space, ``;``, ``·`` and ``"``; ``.`` is an
empty cell, which only ``detail`` may have. Every room needs a floor and a tile strip.

Its objects, in any order (the writer groups them by kind, keeping the order within a kind)::

    scenery mid rusty-anvil 1180          ; layer far|mid|fore, a prop, x: the placeholder painter's input
    life gulls = 0 80 3200 200            ; a looping detail in a box, x0 y0 x1 y1. Kinds below
    exit west = pier east · id pier · label "The Pier"
    exit up 1500 = sunspire-steps foot · id stairs-up     ; up and down exits stand at an x
    exit east = locked · id east · why "No way through to the east." · unless a1-c1-02
    door 1180 = rusty-anvil start · inn · "The Rusty Anvil" · id anvil · when a1-c4-03
    door 1180 = locked · inn · "The Rusty Anvil" · id anvil-shut · unless a1-c4-03 · why "Barred."
    spawn start = 760 690 · facing right
    npc gull-1 = 1180 720 · cast red-gull · unless red-gulls-beaten
    npc dock-hand = 2500 700 · sheet farmhand · name "A Dock Hand" · wander 160
    prop ship = 1200 600                  ; the prop's name, then its feet. Optional id, when, unless
    chest glade = 300 740 · gold 15 · items healing-potion,leather-cap
    sign fork = 900 640 · text "THE OLD ROAD"
    lever side-lever = 700 620 · flag side-gate
    waypoint stone = 1400 700
    trigger red-gulls = 980 600 1440 860 · talk a1-c1-01 · mark · when arrived
    scene a1-c4-03 = talk a1-c4-03 · when a1-c4-02 · unless a1-c4-03
    camp seat-1 = 700 690 · facing left
    light corner-candle = 400 500 120 0.8 ; x y reach [strength, default 1]; the id is optional

Targets (exits and doors) are ``<room> <spawn>`` for a room of this zone, ``<zone> <spawn>`` for
another zone (a room id is looked for first), ``back`` (where you came in from) or ``locked`` (which
needs ``why``). Fields by kind:

- exit: ``id`` (default ``<room>-<side>``), ``label`` (default the target's name), ``why``,
  ``travel`` (a flag: opens the world map), ``when``, ``unless``. West and east exits trigger in the
  48 px nearest that end of the room.
- door: then the badge and the quoted name, in that order; then ``id`` (default the target room or
  zone, and required for back and locked), ``why``, ``size w h``, ``when``, ``unless``. The box
  stands on the floor's back edge at x; its size defaults by badge (``DOOR_SIZES``).
- spawn: ``facing`` left|right (default: toward the room's centre).
- npc: at most one of ``cast``, ``sheet``, ``look`` (a seed) and ``creature``; ``name`` (needed with
  sheet and look), ``facing``, ``wander`` px, ``patrol left right``, ``badge``, ``aboard <prop>``,
  ``say "a|b"``, ``talk``, ``when``, ``unless``. Wander, patrol and aboard exclude each other.
- prop: ``id``, ``when``, ``unless``.
- chest: ``look``, ``name``, ``gold``, ``items``; sign: ``look``, ``name``, ``text`` (required);
  lever: ``look``, ``name``, ``flag`` (required); waypoint: ``look``, ``name``. All take when/unless.
- trigger: x0 y0 x1 y1, then ``talk``, ``say``, ``set``, ``bark`` (at least one), ``mark``,
  ``once yes|no`` (default yes), ``when``, ``unless``.
- scene: ``talk`` (required), ``when``, ``unless``. It plays wherever you are in the room.
- camp: ``facing``. The first waiting companion stands at the first seat, and so on.

Ids: spawn names are unique within a zone, and so are the ids of everything else (exits, doors,
npcs, props that have one, things, triggers, scenes, camp seats and lights), in one namespace as
the tile maps kept them. Every room has a spawn, and every zone has ``start``.

Badges: ``BADGES``. Life kinds: ``LIFE_KINDS``. Scenery layers: ``SCENERY_LAYERS``.
"""

from __future__ import annotations

import math
import os
from dataclasses import dataclass, field
from typing import Dict, Iterable, List, Optional, Tuple

SEP = "·"
"""The field separator, a middle dot."""

ROOM_HEIGHT = 900
TILE = 64
"""A tile in the strip, in canvas px: 16 art px at 4x."""
MIN_WIDTH, MAX_WIDTH = 1600, 4800
EDGE_EXIT = 48
"""How near each end of the room an edge exit triggers."""

KINDS = ("street", "interior")
SIDES = ("west", "east", "up", "down")
FACINGS = ("left", "right")
BADGES = ("shop", "anvil", "bottle", "inn", "trainer", "scroll", "shield", "sun", "crown", "door",
          "gate", "stair-up", "stair-down")
SCENERY_LAYERS = ("far", "mid", "fore")
LIFE_KINDS = ("gulls", "clouds", "smoke", "flag", "shimmer", "leaves", "motes", "ash", "candle")
THING_KINDS = ("chest", "sign", "lever", "waypoint")

DOOR_SIZES = {
    "shop": (120, 200), "inn": (120, 200), "door": (120, 200),
    "gate": (300, 300), "crown": (300, 300),
    "stair-up": (160, 120), "stair-down": (160, 120),
}
DEFAULT_DOOR_SIZE = (120, 200)
"""Every badge not in DOOR_SIZES (anvil, bottle, trainer, scroll, shield, sun)."""

ZONE_SETTINGS = ("id", "name", "world", "music", "region", "danger")
ZONE_REQUIRED = ("id", "name", "music")
ROOM_SETTINGS = ("name", "kind", "width", "paint", "dark", "ambient", "tint", "floor", "block", "fight",
                 "tiles", "tiletop", "legend", "row", "detail", "legacy")
ROOM_MULTI = ("block", "legend", "row", "detail", "legacy")
OBJECT_KINDS = ("scenery", "life", "exit", "door", "spawn", "npc", "prop", "chest", "sign", "lever",
                "waypoint", "trigger", "scene", "camp", "light", "when")

EMPTY = "."
BAD_TILE_CHARS = (" ", ";", SEP, '"', "\t")


class ZoneError(Exception):
    """A zone file is wrong. The message says the file and, usually, the line."""


# ---------------------------------------------------------------------------------------------
# The records

@dataclass
class Presence:
    when: Optional[str] = None
    unless: Optional[str] = None


@dataclass
class Target:
    """Where an exit or door goes. kind is room, zone, back or locked."""
    kind: str
    place: Optional[str] = None
    spawn: Optional[str] = None

    def text(self) -> str:
        if self.kind in ("back", "locked"):
            return self.kind
        return f"{self.place} {self.spawn}"


@dataclass
class Block:
    cx: float
    cy: float
    rx: float
    ry: float


@dataclass
class Tiles:
    tileset: str
    top: int
    legend: Dict[str, str]
    rows: List[str]
    detail: List[str] = field(default_factory=list)


@dataclass
class Legacy:
    x0: int
    y0: int
    x1: int
    y1: int


@dataclass
class Scenery:
    layer: str
    prop: str
    x: float
    line: int = 0


@dataclass
class Life:
    kind: str
    x0: float
    y0: float
    x1: float
    y1: float
    line: int = 0


@dataclass
class Exit:
    id: str
    side: str
    target: Target
    x: Optional[float] = None
    label: Optional[str] = None
    why: Optional[str] = None
    travel: bool = False
    presence: Presence = field(default_factory=Presence)
    id_given: bool = True
    line: int = 0


@dataclass
class Door:
    id: str
    x: float
    target: Target
    badge: str
    name: str
    why: Optional[str] = None
    size: Optional[Tuple[float, float]] = None
    presence: Presence = field(default_factory=Presence)
    id_given: bool = True
    line: int = 0

    def box_size(self) -> Tuple[float, float]:
        return self.size or DOOR_SIZES.get(self.badge, DEFAULT_DOOR_SIZE)


@dataclass
class Spawn:
    name: str
    x: float
    y: float
    facing: Optional[str] = None
    line: int = 0


@dataclass
class Npc:
    id: str
    x: float
    y: float
    cast: Optional[str] = None
    sheet: Optional[str] = None
    look: Optional[int] = None
    creature: Optional[str] = None
    name: Optional[str] = None
    facing: Optional[str] = None
    wander: float = 0.0
    patrol: Optional[Tuple[float, float]] = None
    badge: Optional[str] = None
    aboard: Optional[str] = None
    say: List[str] = field(default_factory=list)
    talk: Optional[str] = None
    presence: Presence = field(default_factory=Presence)
    line: int = 0


@dataclass
class Prop:
    prop: str
    x: float
    y: float
    id: Optional[str] = None
    presence: Presence = field(default_factory=Presence)
    line: int = 0


@dataclass
class Thing:
    kind: str
    id: str
    x: float
    y: float
    look: Optional[str] = None
    name: Optional[str] = None
    gold: int = 0
    items: List[str] = field(default_factory=list)
    text: Optional[str] = None
    flag: Optional[str] = None
    presence: Presence = field(default_factory=Presence)
    line: int = 0


@dataclass
class Trigger:
    id: str
    x0: float
    y0: float
    x1: float
    y1: float
    talk: Optional[str] = None
    say: Optional[str] = None
    set: Optional[str] = None
    bark: Optional[str] = None
    mark: bool = False
    once: bool = True
    presence: Presence = field(default_factory=Presence)
    line: int = 0


@dataclass
class Scene:
    id: str
    talk: str
    presence: Presence = field(default_factory=Presence)
    line: int = 0


@dataclass
class Camp:
    id: str
    x: float
    y: float
    facing: Optional[str] = None
    line: int = 0


@dataclass
class Light:
    x: float
    y: float
    reach: float
    strength: float = 1.0
    id: Optional[str] = None
    line: int = 0


@dataclass
class Room:
    id: str
    name: str = ""
    kind: str = "street"
    width: int = 0
    paint: Optional[str] = None
    dark: bool = False
    ambient: Optional[float] = None
    tint: Optional[Tuple[int, int, int]] = None
    floor: List[Tuple[float, float]] = field(default_factory=list)
    blocks: List[Block] = field(default_factory=list)
    fight: Optional[Tuple[float, float]] = None
    tiles: Optional[Tiles] = None
    legacy: List[Legacy] = field(default_factory=list)
    states: Dict[str, str] = field(default_factory=dict)
    scenery: List[Scenery] = field(default_factory=list)
    life: List[Life] = field(default_factory=list)
    exits: List[Exit] = field(default_factory=list)
    doors: List[Door] = field(default_factory=list)
    spawns: List[Spawn] = field(default_factory=list)
    npcs: List[Npc] = field(default_factory=list)
    props: List[Prop] = field(default_factory=list)
    things: List[Thing] = field(default_factory=list)
    triggers: List[Trigger] = field(default_factory=list)
    scenes: List[Scene] = field(default_factory=list)
    camp: List[Camp] = field(default_factory=list)
    lights: List[Light] = field(default_factory=list)
    comments: List[str] = field(default_factory=list)
    line: int = 0

    @property
    def paint_path(self) -> str:
        return self.paint or ""

    def fight_range(self) -> Tuple[float, float]:
        if self.fight:
            return self.fight
        mid = self.width / 2
        half = min(800, self.width / 2)
        return (mid - half, mid + half)

    def spawn(self, name: str) -> Optional[Spawn]:
        for s in self.spawns:
            if s.name == name:
                return s
        return None

    def back_edge_at(self, x: float) -> float:
        return back_edge_at(self.floor, x)

    def front_edge_at(self, x: float) -> float:
        return front_edge_at(self.floor, x)

    def on_floor(self, x: float, y: float) -> bool:
        if not point_in_polygon(self.floor, x, y):
            return False
        for b in self.blocks:
            if ((x - b.cx) / b.rx) ** 2 + ((y - b.cy) / b.ry) ** 2 < 1:
                return False
        return True

    def door_box(self, door: Door) -> Tuple[float, float, float, float]:
        """The door's box, left top width height: standing on the floor's back edge at its x."""
        w, h = door.box_size()
        bottom = self.back_edge_at(door.x)
        return (door.x - w / 2, bottom - h, w, h)

    def ids(self) -> Iterable[Tuple[str, int, str]]:
        """(id, line, kind) of everything with an id, spawns aside."""
        for e in self.exits:
            yield e.id, e.line, "exit"
        for d in self.doors:
            yield d.id, d.line, "door"
        for n in self.npcs:
            yield n.id, n.line, "npc"
        for p in self.props:
            if p.id:
                yield p.id, p.line, "prop"
        for t in self.things:
            yield t.id, t.line, t.kind
        for t in self.triggers:
            yield t.id, t.line, "trigger"
        for s in self.scenes:
            yield s.id, s.line, "scene"
        for c in self.camp:
            yield c.id, c.line, "camp"
        for li in self.lights:
            if li.id:
                yield li.id, li.line, "light"


@dataclass
class Zone:
    id: str
    name: str = ""
    world: Optional[Tuple[int, int]] = None
    music: str = ""
    region: Optional[str] = None
    danger: int = 0
    rooms: List[Room] = field(default_factory=list)
    comments: List[str] = field(default_factory=list)
    file: str = ""

    def room(self, room_id: str) -> Optional[Room]:
        for r in self.rooms:
            if r.id == room_id:
                return r
        return None

    def room_of_spawn(self, spawn: str) -> Optional[Room]:
        for r in self.rooms:
            if r.spawn(spawn):
                return r
        return None

    def spawn_names(self) -> List[str]:
        return [s.name for r in self.rooms for s in r.spawns]


# ---------------------------------------------------------------------------------------------
# Geometry, shared by the tools

def _edge_ys(poly: List[Tuple[float, float]], x: float) -> List[float]:
    ys = []
    n = len(poly)
    for i in range(n):
        x0, y0 = poly[i]
        x1, y1 = poly[(i + 1) % n]
        if x0 == x1:
            if x == x0:
                ys.extend([y0, y1])
            continue
        lo, hi = min(x0, x1), max(x0, x1)
        if lo <= x <= hi:
            t = (x - x0) / (x1 - x0)
            ys.append(y0 + t * (y1 - y0))
    return ys


def back_edge_at(poly: List[Tuple[float, float]], x: float) -> float:
    """The top of the floor band at x (the nearest y to the back wall), clamped into the polygon's x range."""
    if not poly:
        return 0.0
    xs = [p[0] for p in poly]
    x = min(max(x, min(xs)), max(xs))
    ys = _edge_ys(poly, x)
    return min(ys) if ys else min(p[1] for p in poly)


def front_edge_at(poly: List[Tuple[float, float]], x: float) -> float:
    """The bottom of the floor band at x."""
    if not poly:
        return 0.0
    xs = [p[0] for p in poly]
    x = min(max(x, min(xs)), max(xs))
    ys = _edge_ys(poly, x)
    return max(ys) if ys else max(p[1] for p in poly)


def point_in_polygon(poly: List[Tuple[float, float]], x: float, y: float) -> bool:
    """Even-odd rule; points on the boundary count as inside."""
    n = len(poly)
    if n < 3:
        return False
    for i in range(n):
        if _on_segment(poly[i], poly[(i + 1) % n], (x, y)):
            return True
    inside = False
    j = n - 1
    for i in range(n):
        xi, yi = poly[i]
        xj, yj = poly[j]
        if (yi > y) != (yj > y):
            cross = xi + (y - yi) * (xj - xi) / (yj - yi)
            if x < cross:
                inside = not inside
        j = i
    return inside


def polygon_area(poly: List[Tuple[float, float]]) -> float:
    a = 0.0
    n = len(poly)
    for i in range(n):
        x0, y0 = poly[i]
        x1, y1 = poly[(i + 1) % n]
        a += x0 * y1 - x1 * y0
    return a / 2


def _orient(a, b, c) -> float:
    return (b[0] - a[0]) * (c[1] - a[1]) - (b[1] - a[1]) * (c[0] - a[0])


def _on_segment(a, b, p) -> bool:
    if abs(_orient(a, b, p)) > 1e-9:
        return False
    return min(a[0], b[0]) - 1e-9 <= p[0] <= max(a[0], b[0]) + 1e-9 and \
        min(a[1], b[1]) - 1e-9 <= p[1] <= max(a[1], b[1]) + 1e-9


def _segments_cross(a, b, c, d) -> bool:
    o1, o2, o3, o4 = _orient(a, b, c), _orient(a, b, d), _orient(c, d, a), _orient(c, d, b)
    if ((o1 > 0) != (o2 > 0)) and ((o3 > 0) != (o4 > 0)) and o1 != 0 and o2 != 0 and o3 != 0 and o4 != 0:
        return True
    return (o1 == 0 and _on_segment(a, b, c)) or (o2 == 0 and _on_segment(a, b, d)) or \
        (o3 == 0 and _on_segment(c, d, a)) or (o4 == 0 and _on_segment(c, d, b))


def polygon_problem(poly: List[Tuple[float, float]]) -> Optional[str]:
    """Why a floor isn't a closed simple polygon, or None if it is."""
    n = len(poly)
    if n < 3:
        return "a floor needs at least three points"
    for i in range(n):
        if poly[i] == poly[(i + 1) % n]:
            return f"point {i + 1} is the same as the next (the floor closes itself; don't repeat the first point)"
    if abs(polygon_area(poly)) < 1:
        return "the floor has no area"
    for i in range(n):
        a, b = poly[i], poly[(i + 1) % n]
        for j in range(i + 1, n):
            if j == i or (j + 1) % n == i or j == (i + 1) % n:
                continue
            c, d = poly[j], poly[(j + 1) % n]
            if _segments_cross(a, b, c, d):
                return f"the floor's edges {i + 1} and {j + 1} cross, so it isn't a simple polygon"
    return None


def strip_rows(top: int) -> int:
    """How many rows a tile strip starting at top has: down to the bottom of the room."""
    return max(1, math.ceil((ROOM_HEIGHT - top) / TILE))


# ---------------------------------------------------------------------------------------------
# Reading

def _strip_comment(text: str) -> str:
    quoted = False
    for i, c in enumerate(text):
        if c == '"':
            quoted = not quoted
        elif c == ";" and not quoted:
            return text[:i]
    return text


def _split_outside_quotes(text: str, sep: str) -> Tuple[List[str], bool]:
    parts, cur, quoted = [], [], False
    for c in text:
        if c == '"':
            quoted = not quoted
            cur.append(c)
        elif c == sep and not quoted:
            parts.append("".join(cur))
            cur = []
        else:
            cur.append(c)
    parts.append("".join(cur))
    return parts, quoted


class _Line:
    """One line being read: where it is, for the errors."""

    def __init__(self, file: str, number: int, text: str):
        self.file = file
        self.number = number
        self.text = text

    def fail(self, message: str) -> ZoneError:
        return ZoneError(f"{self.file} line {self.number}: {message}.")


class _Fields:
    """The `· key value` fields of a line. Taken out as they're read; Done checks nothing's left."""

    def __init__(self, line: _Line, segments: List[str], kind: str):
        self.line = line
        self.kind = kind
        self.values: Dict[str, Optional[str]] = {}
        for seg in segments:
            seg = seg.strip()
            if not seg:
                raise line.fail(f"there's an empty field (two {SEP}s together, or one at the end)")
            if seg.startswith('"'):
                raise line.fail(f"{seg} is a quoted value with no key in front of it")
            parts = seg.split(None, 1)
            key = parts[0].lower()
            value = parts[1].strip() if len(parts) > 1 else None
            if value is not None and value.startswith('"'):
                if not value.endswith('"') or len(value) < 2:
                    raise line.fail(f"{key}'s value starts with a \" but doesn't end with one")
                value = value[1:-1]
                if '"' in value:
                    raise line.fail(f"{key}'s value has a \" inside it, and there's no escaping")
                if value == "":
                    raise line.fail(f"{key} is set to nothing")
            if key in self.values:
                raise line.fail(f"{key} is set twice")
            self.values[key] = value

    def text(self, key: str) -> Optional[str]:
        if key not in self.values:
            return None
        value = self.values.pop(key)
        if value is None:
            raise self.line.fail(f"{key} needs a value")
        return value

    def required(self, key: str) -> str:
        value = self.text(key)
        if value is None:
            raise self.line.fail(f"a {self.kind} has to set {key}")
        return value

    def flag(self, key: str) -> bool:
        if key not in self.values:
            return False
        value = self.values.pop(key)
        if value is not None:
            raise self.line.fail(f"{key} is a flag on its own, with nothing after it")
        return True

    def yes_no(self, key: str, fallback: bool) -> bool:
        value = self.text(key)
        if value is None:
            return fallback
        return _yes_no(self.line, value, key)

    def number(self, key: str, fallback: float) -> float:
        value = self.text(key)
        if value is None:
            return fallback
        n = _number(self.line, value, key)
        if n < 0:
            raise self.line.fail(f"{key} can't be less than 0")
        return n

    def numbers(self, key: str, count: int) -> Optional[Tuple[float, ...]]:
        value = self.text(key)
        if value is None:
            return None
        return _numbers(self.line, value, key, count)

    def presence(self) -> Presence:
        return Presence(self.text("when"), self.text("unless"))

    def facing(self) -> Optional[str]:
        value = self.text("facing")
        if value is None:
            return None
        value = value.lower()
        if value not in FACINGS:
            raise self.line.fail(f"facing has to be left or right, not {value}")
        return value

    def done(self, allowed: str) -> None:
        if self.values:
            extra = ", ".join(self.values)
            raise self.line.fail(f"a {self.kind} has no setting called {extra} (it has {allowed})")


def _yes_no(line: _Line, value: str, key: str) -> bool:
    v = value.lower()
    if v in ("yes", "true"):
        return True
    if v in ("no", "false"):
        return False
    raise line.fail(f"{key} has to be yes or no, not {value}")


def _number(line: _Line, text: str, what: str) -> float:
    try:
        n = float(text)
    except ValueError:
        raise line.fail(f"{what} has to be a number, not {text}") from None
    if math.isnan(n) or math.isinf(n):
        raise line.fail(f"{what} has to be a number, not {text}")
    return n


def _numbers(line: _Line, text: str, what: str, count: int) -> Tuple[float, ...]:
    parts = text.split()
    if len(parts) != count:
        raise line.fail(f"{what} has to be {count} numbers, not {text}")
    return tuple(_number(line, p, what) for p in parts)


def _whole(line: _Line, text: str, what: str, lo: int = 0) -> int:
    try:
        n = int(text)
    except ValueError:
        raise line.fail(f"{what} has to be a whole number, not {text}") from None
    if n < lo:
        raise line.fail(f"{what} has to be {lo} or more, not {text}")
    return n


def _word(line: _Line, text: str, what: str) -> str:
    if not text or any(c.isspace() for c in text) or '"' in text:
        raise line.fail(f"{what} has to be one word, not {text!r}")
    return text


def _quoted(line: _Line, text: str, what: str) -> str:
    text = text.strip()
    if len(text) < 3 or not (text.startswith('"') and text.endswith('"')) or '"' in text[1:-1]:
        raise line.fail(f"{what} has to be in double quotes, like \"The Rusty Anvil\", not {text}")
    return text[1:-1]


def _target(line: _Line, text: str) -> Target:
    words = text.split()
    if words == ["back"]:
        return Target("back")
    if words == ["locked"]:
        return Target("locked")
    if len(words) != 2:
        raise line.fail(f"a way out goes to <room> <spawn>, <zone> <spawn>, back or locked, not {text}")
    return Target("place", words[0], words[1])


class _RoomReader:
    def __init__(self, file: str, zone: Zone, room: Room, used_ids: Dict[str, int], used_spawns: Dict[str, int]):
        self.file = file
        self.zone = zone
        self.room = room
        self.ids = used_ids
        self.spawns = used_spawns
        self.settings: Dict[str, int] = {}
        self.objects_started = False
        self.tiles: Dict[str, object] = {}
        self.rows: List[Tuple[int, str]] = []
        self.details: List[Tuple[int, str]] = []
        self.legend: Dict[str, str] = {}

    # The room's width, which a position needs to be checked against.
    def _need_width(self, line: _Line, what: str) -> int:
        if not self.room.width:
            raise line.fail(f"width has to come before {what}")
        return self.room.width

    def _check_x(self, line: _Line, x: float, what: str = "x") -> None:
        w = self._need_width(line, "anything with a position")
        if x < 0 or x > w:
            raise line.fail(f"{what} {_fmt(x)} is outside the room, which is {w} wide")

    def _check_xy(self, line: _Line, x: float, y: float) -> None:
        self._check_x(line, x)
        if y < 0 or y > ROOM_HEIGHT:
            raise line.fail(f"y {_fmt(y)} is outside the room, which is {ROOM_HEIGHT} tall")

    def _claim_id(self, line: _Line, ident: str) -> None:
        _word(line, ident, "an id")
        if ident in self.ids:
            raise line.fail(f"there's already something called {ident} in this zone (line {self.ids[ident]})")
        self.ids[ident] = line.number

    def setting(self, line: _Line, key: str, value: str) -> None:
        r = self.room
        if key not in ROOM_SETTINGS:
            raise line.fail(f"a room has no setting called {key} (they're {', '.join(ROOM_SETTINGS)})")
        if self.objects_started:
            raise line.fail(f"{key} is a room setting, and those come before the room's objects")
        if key not in ROOM_MULTI:
            if key in self.settings:
                raise line.fail(f"{key} is set twice")
            self.settings[key] = line.number
        if value == "":
            raise line.fail(f"{key} is set to nothing")

        if key == "name":
            r.name = value
        elif key == "kind":
            if value not in KINDS:
                raise line.fail(f"kind has to be street or interior, not {value}")
            r.kind = value
        elif key == "width":
            w = _whole(line, value, "width")
            if w % TILE or w < MIN_WIDTH or w > MAX_WIDTH:
                raise line.fail(f"width has to be a multiple of {TILE} from {MIN_WIDTH} to {MAX_WIDTH}, not {value}")
            r.width = w
        elif key == "paint":
            r.paint = _word(line, value, "paint")
        elif key == "dark":
            r.dark = _yes_no(line, value, "dark")
        elif key == "ambient":
            a = _number(line, value, "ambient")
            if a < 0 or a > 1:
                raise line.fail(f"ambient has to be from 0 to 1, not {value}")
            r.ambient = a
        elif key == "tint":
            parts = value.split()
            if len(parts) != 3:
                raise line.fail(f"tint has to be three numbers from 0 to 255, red green blue, not {value}")
            rgb = tuple(_whole(line, p, "each of tint's numbers") for p in parts)
            if any(c > 255 for c in rgb):
                raise line.fail(f"each of tint's numbers has to be from 0 to 255, not {value}")
            r.tint = rgb  # type: ignore[assignment]
        elif key == "floor":
            w = self._need_width(line, "the floor")
            points = []
            for i, pair in enumerate(value.split(",")):
                nums = _numbers(line, pair.strip(), f"floor point {i + 1}", 2)
                if nums[0] < 0 or nums[0] > w or nums[1] < 0 or nums[1] > ROOM_HEIGHT:
                    raise line.fail(f"floor point {i + 1} ({pair.strip()}) is outside the room")
                points.append((nums[0], nums[1]))
            problem = polygon_problem(points)
            if problem:
                raise line.fail(problem)
            r.floor = points
        elif key == "block":
            cx, cy, rx, ry = _numbers(line, value, "block (centre x y, radii x y)", 4)
            self._check_xy(line, cx, cy)
            if rx <= 0 or ry <= 0:
                raise line.fail("a block's radii have to be more than 0")
            r.blocks.append(Block(cx, cy, rx, ry))
        elif key == "fight":
            left, right = _numbers(line, value, "fight (left right)", 2)
            self._check_x(line, left, "fight's left")
            self._check_x(line, right, "fight's right")
            if right <= left:
                raise line.fail("fight's right has to be more than its left")
            r.fight = (left, right)
        elif key == "tiles":
            self.tiles["tileset"] = _word(line, value, "tiles")
        elif key == "tiletop":
            top = _whole(line, value, "tiletop")
            if top % TILE or top >= ROOM_HEIGHT:
                raise line.fail(f"tiletop has to be a multiple of {TILE} above {ROOM_HEIGHT}, not {value}")
            self.tiles["top"] = top
        elif key == "legend":
            if len(value) < 3 or value[1] != " ":
                raise line.fail("a legend line is one character, a space and a tile's name, like legend = = planks")
            c, name = value[0], value[2:].strip()
            if c in BAD_TILE_CHARS or c == EMPTY:
                raise line.fail(f"'{c}' can't stand for a tile ('.' is an empty cell)")
            _word(line, name, "a tile's name")
            if c in self.legend:
                raise line.fail(f"'{c}' is in the legend twice")
            self.legend[c] = name
        elif key in ("row", "detail"):
            w = self._need_width(line, f"a {key}")
            cells = w // TILE
            if len(value) != cells:
                raise line.fail(f"a {key} is {cells} tiles in a room {w} wide, and this one is {len(value)}")
            for i, c in enumerate(value):
                if c == EMPTY:
                    if key == "row":
                        raise line.fail(f"row has an empty cell at column {i + 1}, and every ground cell needs a tile")
                    continue
                if c not in self.legend:
                    raise line.fail(f"'{c}' at column {i + 1} isn't in the room's legend (legend lines come first)")
            (self.rows if key == "row" else self.details).append((line.number, value))
        elif key == "legacy":
            nums = value.split()
            if len(nums) != 4:
                raise line.fail(f"legacy has to be x0 y0 x1 y1 in the old map's tiles, not {value}")
            x0, y0, x1, y1 = (_whole(line, n, "legacy") for n in nums)
            if x1 <= x0 or y1 <= y0:
                raise line.fail("legacy's x1 and y1 have to be more than its x0 and y0")
            r.legacy.append(Legacy(x0, y0, x1, y1))

    def obj(self, line: _Line, head: List[str], body: Optional[str]) -> None:
        r = self.room
        kind = head[0]
        self.objects_started = True

        if kind == "scenery":
            if body is not None or len(head) != 4:
                raise line.fail("a scenery line is scenery <far|mid|fore> <prop> <x>, with no =")
            layer = head[1].lower()
            if layer not in SCENERY_LAYERS:
                raise line.fail(f"scenery's layer has to be far, mid or fore, not {head[1]}")
            x = _number(line, head[3], "scenery's x")
            self._check_x(line, x)
            r.scenery.append(Scenery(layer, head[2], x, line.number))
            return

        if body is None:
            raise line.fail(f"a {kind} line needs an = and what comes after it")
        segments, open_quote = _split_outside_quotes(body, SEP)
        if open_quote:
            raise line.fail("a \" is never closed")
        first = segments[0].strip()
        rest = segments[1:]

        if kind == "when":
            if len(head) != 2 or rest:
                raise line.fail("a room state is when <state> = <flag>")
            state = _word(line, head[1], "a state")
            if state in r.states:
                raise line.fail(f"the state {state} is set twice")
            r.states[state] = _word(line, first, "the state's flag")
            return

        if kind == "life":
            if len(head) != 2 or rest:
                raise line.fail("a life line is life <kind> = x0 y0 x1 y1")
            lk = head[1].lower()
            if lk not in LIFE_KINDS:
                raise line.fail(f"there's no life called {lk} (they're {', '.join(LIFE_KINDS)})")
            x0, y0, x1, y1 = _numbers(line, first, "life's box (x0 y0 x1 y1)", 4)
            self._check_xy(line, x0, y0)
            self._check_xy(line, x1, y1)
            if x1 <= x0 or y1 <= y0:
                raise line.fail("life's x1 and y1 have to be more than its x0 and y0")
            r.life.append(Life(lk, x0, y0, x1, y1, line.number))
            return

        if kind == "exit":
            if len(head) not in (2, 3):
                raise line.fail("an exit is exit west|east = ..., or exit up|down <x> = ...")
            side = head[1].lower()
            if side not in SIDES:
                raise line.fail(f"an exit's side has to be west, east, up or down, not {head[1]}")
            x = None
            if side in ("up", "down"):
                if len(head) != 3:
                    raise line.fail(f"an exit {side} needs an x, like exit {side} 1500 = ...")
                x = _number(line, head[2], "the exit's x")
                self._check_x(line, x)
            elif len(head) != 2:
                raise line.fail(f"an exit {side} is at the room's edge, so it has no x")
            else:
                self._need_width(line, "an exit")
            target = _target(line, first)
            f = _Fields(line, rest, "exit")
            ident = f.text("id")
            given = ident is not None
            ident = ident or f"{r.id}-{side}"
            e = Exit(ident, side, target, x, f.text("label"), f.text("why"), f.flag("travel"), f.presence(),
                     given, line.number)
            f.done("id, label, why, travel, when, unless")
            if target.kind == "locked" and not e.why:
                raise line.fail("a locked exit has to say why, with why \"...\"")
            if target.kind != "locked" and e.why:
                raise line.fail("only a locked exit says why")
            self._claim_id(line, ident)
            r.exits.append(e)
            return

        if kind == "door":
            if len(head) != 2:
                raise line.fail("a door is door <x> = <target> · <badge> · \"<name>\"")
            x = _number(line, head[1], "the door's x")
            self._check_x(line, x)
            target = _target(line, first)
            if len(rest) < 2:
                raise line.fail("a door needs its badge and its name after the target: door 1180 = rusty-anvil start · inn · \"The Rusty Anvil\"")
            badge = rest[0].strip().lower()
            if badge not in BADGES:
                raise line.fail(f"there's no badge called {badge} (they're {', '.join(BADGES)})")
            name = _quoted(line, rest[1], "a door's name")
            f = _Fields(line, rest[2:], "door")
            ident = f.text("id")
            given = ident is not None
            if ident is None:
                if target.kind in ("back", "locked"):
                    raise line.fail(f"a door that goes {target.kind} has to have an id")
                ident = target.place
            size = f.numbers("size", 2)
            if size is not None and (size[0] <= 0 or size[1] <= 0):
                raise line.fail("a door's size has to be more than 0 each way")
            d = Door(ident, x, target, badge, name, f.text("why"), size, f.presence(), given, line.number)  # type: ignore[arg-type]
            f.done("id, why, size, when, unless")
            if target.kind == "locked" and not d.why:
                raise line.fail("a locked door has to say why, with why \"...\"")
            if target.kind != "locked" and d.why:
                raise line.fail("only a locked door says why")
            self._claim_id(line, ident)
            r.doors.append(d)
            return

        # Everything else is <kind> <id> = <position> · fields (a light's id is optional).
        if kind == "light":
            if len(head) > 2:
                raise line.fail("a light is light [id] = x y reach [strength]")
            ident = head[1] if len(head) == 2 else None
        else:
            if len(head) != 2:
                raise line.fail(f"a {kind} line is {kind} <id> = ...")
            ident = head[1]

        if kind == "scene":
            f = _Fields(line, segments, "scene")
            s = Scene(ident, f.required("talk"), f.presence(), line.number)
            f.done("talk, when, unless")
            self._claim_id(line, ident)
            r.scenes.append(s)
            return

        if kind == "trigger":
            x0, y0, x1, y1 = _numbers(line, first, "a trigger's box (x0 y0 x1 y1)", 4)
            self._check_xy(line, x0, y0)
            self._check_xy(line, x1, y1)
            if x1 <= x0 or y1 <= y0:
                raise line.fail("a trigger's x1 and y1 have to be more than its x0 and y0")
            f = _Fields(line, rest, "trigger")
            t = Trigger(ident, x0, y0, x1, y1, f.text("talk"), f.text("say"), f.text("set"), f.text("bark"),
                        f.flag("mark"), f.yes_no("once", True), f.presence(), line.number)
            f.done("talk, say, set, bark, mark, once, when, unless")
            if not (t.talk or t.say or t.set or t.bark):
                raise line.fail("this trigger doesn't do anything (it needs talk, say, set or bark)")
            self._claim_id(line, ident)
            r.triggers.append(t)
            return

        if kind == "light":
            nums = first.split()
            if len(nums) not in (3, 4):
                raise line.fail("a light is x y reach [strength]")
            x, y, reach = (_number(line, n, "a light's numbers") for n in nums[:3])
            strength = _number(line, nums[3], "a light's strength") if len(nums) == 4 else 1.0
            self._check_xy(line, x, y)
            if reach <= 0:
                raise line.fail("a light's reach has to be more than 0")
            if strength <= 0 or strength > 1:
                raise line.fail(f"a light's strength has to be more than 0 and no more than 1, not {_fmt(strength)}")
            if rest:
                raise line.fail("a light has no fields")
            if ident:
                self._claim_id(line, ident)
            r.lights.append(Light(x, y, reach, strength, ident, line.number))
            return

        x, y = _numbers(line, first, f"a {kind}'s place (x y)", 2)
        self._check_xy(line, x, y)
        f = _Fields(line, rest, kind)

        if kind == "spawn":
            _word(line, ident, "a spawn's name")
            s = Spawn(ident, x, y, f.facing(), line.number)
            f.done("facing")
            if ident in self.spawns:
                raise line.fail(f"there's already a spawn called {ident} in this zone (line {self.spawns[ident]})")
            self.spawns[ident] = line.number
            r.spawns.append(s)
            return

        if kind == "camp":
            c = Camp(ident, x, y, f.facing(), line.number)
            f.done("facing")
            self._claim_id(line, ident)
            r.camp.append(c)
            return

        if kind == "prop":
            _word(line, ident, "a prop")
            p = Prop(ident, x, y, f.text("id"), f.presence(), line.number)
            f.done("id, when, unless")
            if p.id:
                self._claim_id(line, p.id)
            r.props.append(p)
            return

        if kind == "npc":
            look_text = f.text("look")
            look = None
            if look_text is not None:
                try:
                    look = int(look_text)
                except ValueError:
                    raise line.fail(f"look has to be a whole number, the seed their look is picked from, not {look_text}") from None
            patrol = f.numbers("patrol", 2)
            badge = f.text("badge")
            say = f.text("say")
            n = Npc(ident, x, y, f.text("cast"), f.text("sheet"), look, f.text("creature"), f.text("name"),
                    f.facing(), f.number("wander", 0.0), patrol, badge, f.text("aboard"),  # type: ignore[arg-type]
                    [], f.text("talk"), f.presence(), line.number)
            f.done("cast, sheet, look, creature, name, facing, wander, patrol, badge, aboard, say, talk, when, unless")
            drawn = sum(v is not None for v in (n.cast, n.sheet, n.look, n.creature))
            if drawn > 1:
                raise line.fail("someone's drawn from one of cast, sheet, look or creature, not two of them")
            if drawn == 0 and n.name is None:
                raise line.fail("someone who isn't in the cast or a creature needs name, and sheet or look")
            if (n.sheet is not None or n.look is not None) and n.name is None:
                raise line.fail("someone drawn from sheet or look needs name too")
            if drawn == 0:
                raise line.fail("someone needs cast, sheet, look or creature for how they look")
            if badge is not None:
                b = badge.lower()
                if b not in BADGES:
                    raise line.fail(f"there's no badge called {badge} (they're {', '.join(BADGES)})")
                n.badge = b
            if patrol is not None:
                self._check_x(line, patrol[0], "patrol's left")
                self._check_x(line, patrol[1], "patrol's right")
                if patrol[1] <= patrol[0]:
                    raise line.fail("patrol's right has to be more than its left")
            moves = (n.wander > 0) + (patrol is not None) + (n.aboard is not None)
            if moves > 1:
                raise line.fail("wander, patrol and aboard exclude each other")
            if say is not None:
                lines = [s.strip() for s in say.split("|")]
                if any(not s for s in lines):
                    raise line.fail("say has an empty line in it (two |s together, or one at an end)")
                n.say = lines
            self._claim_id(line, ident)
            r.npcs.append(n)
            return

        if kind in THING_KINDS:
            t = Thing(kind, ident, x, y, f.text("look"), f.text("name"))
            if kind == "chest":
                gold = f.text("gold")
                t.gold = _whole(line, gold, "gold") if gold is not None else 0
                items = f.text("items")
                if items is not None:
                    t.items = [i.strip() for i in items.split(",")]
                    if any(not i for i in t.items):
                        raise line.fail("items has nothing between two of its commas")
                allowed = "look, name, gold, items, when, unless"
            elif kind == "sign":
                t.text = f.required("text")
                allowed = "look, name, text, when, unless"
            elif kind == "lever":
                t.flag = f.required("flag")
                allowed = "look, name, flag, when, unless"
            else:
                allowed = "look, name, when, unless"
            t.presence = f.presence()
            t.line = line.number
            f.done(allowed)
            self._claim_id(line, ident)
            r.things.append(t)
            return

        raise line.fail(f"there's no kind of object called {kind}")

    def finish(self, end_line: _Line) -> None:
        r = self.room
        where = f"{self.file}: room {r.id}"
        if not r.width:
            raise ZoneError(f"{where} doesn't set width.")
        if not r.name:
            raise ZoneError(f"{where} doesn't set name.")
        if not r.floor:
            raise ZoneError(f"{where} has no floor.")
        if r.dark and r.ambient is None:
            raise ZoneError(f"{where} is dark, so it has to set ambient (0 to 1).")
        if not r.dark and r.ambient is not None:
            raise ZoneError(f"{self.file} line {self.settings['ambient']}: ambient only means something in a dark room.")
        if "tileset" not in self.tiles or "top" not in self.tiles or not self.rows:
            raise ZoneError(f"{where} has no tile strip (it needs tiles, tiletop, legend and rows).")
        top = self.tiles["top"]
        want = strip_rows(top)  # type: ignore[arg-type]
        if len(self.rows) != want:
            raise ZoneError(f"{self.file} line {self.rows[-1][0]}: a strip from tiletop {top} has {want} rows, and this one has {len(self.rows)}.")
        if self.details and len(self.details) != len(self.rows):
            raise ZoneError(f"{self.file} line {self.details[-1][0]}: there are {len(self.details)} detail rows and {len(self.rows)} rows; they have to match.")
        r.tiles = Tiles(self.tiles["tileset"], top, dict(self.legend), [t for _, t in self.rows],  # type: ignore[arg-type]
                        [t for _, t in self.details])
        if not r.spawns:
            raise ZoneError(f"{where} has no spawn, and every room needs one.")
        if r.paint is None:
            r.paint = f"{self.zone.id}/{r.id}"


def parse_zone(text: str, file: str = "Zones/zone.txt", expect_id: Optional[str] = None) -> Zone:
    """Reads a zone file's text. file is what the errors call it; expect_id checks the header's id."""
    if text.startswith("﻿"):
        text = text[1:]
    zone = Zone(id="", file=file)
    zone_settings: Dict[str, Tuple[int, str]] = {}
    section: Optional[str] = None
    reader: Optional[_RoomReader] = None
    ids: Dict[str, int] = {}
    spawns: Dict[str, int] = {}
    pending_comments: List[str] = []
    header_done = False
    lines = text.split("\n")

    def finish_header(at: _Line) -> None:
        nonlocal header_done
        for key in ZONE_REQUIRED:
            if key not in zone_settings:
                raise ZoneError(f"{file}: [zone] doesn't set {key}.")
        header_done = True

    for i, raw in enumerate(lines):
        number = i + 1
        stripped = raw.strip()
        if stripped.startswith(";"):
            comment = stripped[1:]
            if comment.startswith(" "):
                comment = comment[1:]
            pending_comments.append(comment)
            continue
        line_text = _strip_comment(raw).strip()
        if not line_text:
            if not stripped:
                continue
            continue
        line = _Line(file, number, line_text)

        if line_text.startswith("["):
            if not line_text.endswith("]"):
                raise line.fail("a section name has no closing ]")
            name = line_text[1:-1].strip()
            if name.lower() == "zone":
                if section is not None:
                    raise line.fail("[zone] has to come first, and once")
                section = "zone"
                zone.comments = pending_comments
                pending_comments = []
                continue
            parts = name.split()
            if len(parts) != 2 or parts[0].lower() != "room":
                raise line.fail(f"there's no section called [{name}] (the sections are [zone] and [room <id>])")
            if section is None:
                raise line.fail("[zone] has to come before the rooms")
            if section == "zone":
                finish_header(line)
            if reader is not None:
                reader.finish(line)
            room_id = _word(line, parts[1], "a room's id")
            if zone.room(room_id):
                raise line.fail(f"there are two rooms called {room_id}")
            room = Room(id=room_id, line=number, comments=pending_comments)
            pending_comments = []
            zone.rooms.append(room)
            reader = _RoomReader(file, zone, room, ids, spawns)
            section = "room"
            continue

        pending_comments = []
        if section is None:
            raise line.fail("this line comes before any section")

        equals_at = _find_outside_quotes(line_text, "=")
        head_text = line_text if equals_at < 0 else line_text[:equals_at]
        body = None if equals_at < 0 else line_text[equals_at + 1:].strip()
        head = head_text.split()
        if not head:
            raise line.fail("a line has to start with a key")
        key = head[0].lower()
        head[0] = key

        if section == "zone":
            if body is None or len(head) != 1:
                raise line.fail("a [zone] line is key = value")
            if key not in ZONE_SETTINGS:
                raise line.fail(f"[zone] has no setting called {key} (they're {', '.join(ZONE_SETTINGS)})")
            if key in zone_settings:
                raise line.fail(f"{key} is set twice")
            if body == "":
                raise line.fail(f"{key} is set to nothing")
            zone_settings[key] = (number, body)
            if key == "id":
                zone.id = _word(line, body, "id")
                if expect_id is not None and zone.id != expect_id:
                    raise line.fail(f"the file says it's {zone.id}, but it's called {expect_id}.txt")
            elif key == "name":
                zone.name = body
            elif key == "world":
                wx, wy = body.split() if len(body.split()) == 2 else (None, None)
                if wx is None:
                    raise line.fail(f"world has to be two whole numbers, like 300 200, not {body}")
                zone.world = (_whole(line, wx, "world"), _whole(line, wy, "world"))
            elif key == "music":
                zone.music = _word(line, body, "music")
            elif key == "region":
                zone.region = _word(line, body, "region")
            elif key == "danger":
                zone.danger = _whole(line, body, "danger")
            continue

        assert reader is not None
        if key in ROOM_SETTINGS:
            if body is None or len(head) != 1:
                raise line.fail(f"{key} is a room setting: {key} = value")
            reader.setting(line, key, body)
        elif key in OBJECT_KINDS:
            reader.obj(line, head, body)
        else:
            raise line.fail(f"a room has no setting or object called {key}")

    if section is None:
        raise ZoneError(f"{file}: there's no [zone] section.")
    if section == "zone":
        finish_header(_Line(file, len(lines), ""))
    if reader is not None:
        reader.finish(_Line(file, len(lines), ""))
    if not zone.rooms:
        raise ZoneError(f"{file}: a zone needs at least one room.")
    if "start" not in spawns:
        raise ZoneError(f"{file}: there's no spawn called start, and every zone needs one.")

    _resolve_targets(zone)
    return zone


def _resolve_targets(zone: Zone) -> None:
    """Sorts each place target into room (this zone's) or zone (another), and checks a room target's spawn."""
    for room in zone.rooms:
        for item in list(room.exits) + list(room.doors):
            t = item.target
            if t.kind != "place":
                continue
            target_room = zone.room(t.place)  # type: ignore[arg-type]
            if target_room is not None:
                t.kind = "room"
                if target_room.spawn(t.spawn) is None:  # type: ignore[arg-type]
                    raise ZoneError(f"{zone.file} line {item.line}: room {t.place} has no spawn called {t.spawn}.")
            else:
                t.kind = "zone"
            if not item.id_given and isinstance(item, Door):
                pass


def _find_outside_quotes(text: str, ch: str) -> int:
    quoted = False
    for i, c in enumerate(text):
        if c == '"':
            quoted = not quoted
        elif c == ch and not quoted:
            return i
    return -1


def load_zone(path: str) -> Zone:
    """Reads one zone file from disk. Errors call it Zones/<name>.txt, as the game does."""
    base = os.path.basename(path)
    zone_id = base[:-4] if base.endswith(".txt") else base
    with open(path, encoding="utf-8", newline="") as fh:
        text = fh.read()
    return parse_zone(text, f"Zones/{base}", zone_id)


def load_all(folder: str) -> Dict[str, Zone]:
    """Every zone in a folder, by id, in file-name order. Fails on the first bad file."""
    zones: Dict[str, Zone] = {}
    for name in sorted(os.listdir(folder)):
        if name.endswith(".txt"):
            z = load_zone(os.path.join(folder, name))
            zones[z.id] = z
    return zones


# ---------------------------------------------------------------------------------------------
# Writing

def _fmt(n: float) -> str:
    if float(n).is_integer():
        return str(int(n))
    s = f"{n:.3f}".rstrip("0").rstrip(".")
    return s


def _q(text: str) -> str:
    if '"' in text:
        raise ZoneError(f"can't write {text!r}: a quoted value can't hold a \"")
    return f'"{text}"'


def _presence(p: Presence) -> List[str]:
    out = []
    if p.when:
        out.append(f"when {p.when}")
    if p.unless:
        out.append(f"unless {p.unless}")
    return out


def _join(head: str, body: str, fields: List[str]) -> str:
    return f" {SEP} ".join([f"{head} = {body}"] + fields)


def write_room(zone: Zone, r: Room) -> List[str]:
    out: List[str] = []
    for c in r.comments:
        out.append(f"; {c}" if c else ";")
    out.append(f"[room {r.id}]")
    out.append(f"name = {r.name}")
    out.append(f"kind = {r.kind}")
    out.append(f"width = {r.width}")
    if r.paint and r.paint != f"{zone.id}/{r.id}":
        out.append(f"paint = {r.paint}")
    if r.dark:
        out.append("dark = yes")
        out.append(f"ambient = {_fmt(r.ambient if r.ambient is not None else 1)}")
    if r.tint:
        out.append("tint = " + " ".join(str(c) for c in r.tint))
    out.append("floor = " + ", ".join(f"{_fmt(x)} {_fmt(y)}" for x, y in r.floor))
    for b in r.blocks:
        out.append(f"block = {_fmt(b.cx)} {_fmt(b.cy)} {_fmt(b.rx)} {_fmt(b.ry)}")
    if r.fight:
        out.append(f"fight = {_fmt(r.fight[0])} {_fmt(r.fight[1])}")
    for lg in r.legacy:
        out.append(f"legacy = {lg.x0} {lg.y0} {lg.x1} {lg.y1}")
    if r.tiles:
        t = r.tiles
        out.append(f"tiles = {t.tileset}")
        out.append(f"tiletop = {t.top}")
        for c, name in t.legend.items():
            out.append(f"legend = {c} {name}")
        for row in t.rows:
            out.append(f"row = {row}")
        for row in t.detail:
            out.append(f"detail = {row}")
    for state, flag in r.states.items():
        out.append(f"when {state} = {flag}")

    for s in r.scenery:
        out.append(f"scenery {s.layer} {s.prop} {_fmt(s.x)}")
    for li in r.life:
        out.append(f"life {li.kind} = {_fmt(li.x0)} {_fmt(li.y0)} {_fmt(li.x1)} {_fmt(li.y1)}")
    for e in r.exits:
        head = f"exit {e.side}" if e.x is None else f"exit {e.side} {_fmt(e.x)}"
        fields = [f"id {e.id}"]
        if e.label:
            fields.append(f"label {_q(e.label)}")
        if e.why:
            fields.append(f"why {_q(e.why)}")
        if e.travel:
            fields.append("travel")
        out.append(_join(head, e.target.text(), fields + _presence(e.presence)))
    for d in r.doors:
        fields = [d.badge, _q(d.name), f"id {d.id}"]
        if d.why:
            fields.append(f"why {_q(d.why)}")
        if d.size:
            fields.append(f"size {_fmt(d.size[0])} {_fmt(d.size[1])}")
        out.append(_join(f"door {_fmt(d.x)}", d.target.text(), fields + _presence(d.presence)))
    for s in r.spawns:
        fields = [f"facing {s.facing}"] if s.facing else []
        out.append(_join(f"spawn {s.name}", f"{_fmt(s.x)} {_fmt(s.y)}", fields))
    for n in r.npcs:
        fields = []
        for key in ("cast", "sheet", "look", "creature"):
            v = getattr(n, key)
            if v is not None:
                fields.append(f"{key} {v}")
        if n.name:
            fields.append(f"name {_q(n.name)}")
        if n.facing:
            fields.append(f"facing {n.facing}")
        if n.wander:
            fields.append(f"wander {_fmt(n.wander)}")
        if n.patrol:
            fields.append(f"patrol {_fmt(n.patrol[0])} {_fmt(n.patrol[1])}")
        if n.badge:
            fields.append(f"badge {n.badge}")
        if n.aboard:
            fields.append(f"aboard {n.aboard}")
        if n.say:
            fields.append(f"say {_q('|'.join(n.say))}")
        if n.talk:
            fields.append(f"talk {n.talk}")
        out.append(_join(f"npc {n.id}", f"{_fmt(n.x)} {_fmt(n.y)}", fields + _presence(n.presence)))
    for p in r.props:
        fields = [f"id {p.id}"] if p.id else []
        out.append(_join(f"prop {p.prop}", f"{_fmt(p.x)} {_fmt(p.y)}", fields + _presence(p.presence)))
    for t in r.things:
        fields = []
        if t.look:
            fields.append(f"look {t.look}")
        if t.name:
            fields.append(f"name {_q(t.name)}")
        if t.kind == "chest":
            if t.gold:
                fields.append(f"gold {t.gold}")
            if t.items:
                fields.append("items " + ",".join(t.items))
        if t.kind == "sign" and t.text:
            fields.append(f"text {_q(t.text)}")
        if t.kind == "lever" and t.flag:
            fields.append(f"flag {t.flag}")
        out.append(_join(f"{t.kind} {t.id}", f"{_fmt(t.x)} {_fmt(t.y)}", fields + _presence(t.presence)))
    for t in r.triggers:
        fields = []
        if t.talk:
            fields.append(f"talk {t.talk}")
        if t.say:
            fields.append(f"say {_q(t.say)}")
        if t.set:
            fields.append(f"set {t.set}")
        if t.bark:
            fields.append(f"bark {t.bark}")
        if t.mark:
            fields.append("mark")
        if not t.once:
            fields.append("once no")
        out.append(_join(f"trigger {t.id}", f"{_fmt(t.x0)} {_fmt(t.y0)} {_fmt(t.x1)} {_fmt(t.y1)}",
                         fields + _presence(t.presence)))
    for s in r.scenes:
        out.append(f" {SEP} ".join([f"scene {s.id} = talk {s.talk}"] + _presence(s.presence)))
    for c in r.camp:
        fields = [f"facing {c.facing}"] if c.facing else []
        out.append(_join(f"camp {c.id}", f"{_fmt(c.x)} {_fmt(c.y)}", fields))
    for li in r.lights:
        head = f"light {li.id}" if li.id else "light"
        nums = f"{_fmt(li.x)} {_fmt(li.y)} {_fmt(li.reach)}"
        if li.strength != 1:
            nums += f" {_fmt(li.strength)}"
        out.append(f"{head} = {nums}")
    return out


def write_zone(zone: Zone) -> str:
    """The zone as file text, CRLF line endings, ending with a line break. Deterministic."""
    out: List[str] = []
    for c in zone.comments:
        out.append(f"; {c}" if c else ";")
    out.append("[zone]")
    out.append(f"id = {zone.id}")
    out.append(f"name = {zone.name}")
    if zone.world:
        out.append(f"world = {zone.world[0]} {zone.world[1]}")
    out.append(f"music = {zone.music}")
    if zone.region:
        out.append(f"region = {zone.region}")
    out.append(f"danger = {zone.danger}")
    for r in zone.rooms:
        out.append("")
        out.extend(write_room(zone, r))
    return "\r\n".join(out) + "\r\n"


def save_zone(zone: Zone, path: str) -> None:
    with open(path, "w", encoding="utf-8", newline="") as fh:
        fh.write(write_zone(zone))


if __name__ == "__main__":
    import sys

    for p in sys.argv[1:]:
        try:
            z = load_zone(p)
        except ZoneError as err:
            print(err)
            sys.exit(1)
        print(f"{z.id}: {len(z.rooms)} rooms, " + ", ".join(f"{r.id} {r.width}" for r in z.rooms))
