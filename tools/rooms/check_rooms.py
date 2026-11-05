#!/usr/bin/env python3
"""Checks every zone file in Content/Zones against itself, the other zones and the game's catalogs.

Usage:  python tools/rooms/check_rooms.py [zone ...]      exits 1 on any problem

What it checks (m3-plan §3, the ZoneCatalog validation, plus what a side-view room needs):
- the file parses (tools/rooms/zonefile.py: bad lines, ids unique per zone, floors closed and simple,
  badges known, tile rows the right width, every room with a spawn, every zone with start);
- every exit and door goes to a room and spawn that exist (another zone's too);
- the tile strip's tileset exists and has every tile its legend names (World/TilesetCatalog.cs);
- props and the things' looks are in World/PropCatalog.cs, with the right number of pictures;
- cast ids are in Story/Cast.cs (and aren't only a voice), sheets in Characters/Townsfolk.cs,
  creatures in Characters/Creatures.cs, chest items in Items/ItemCatalog.cs, music in Enums/Track.cs;
- every talk is a conversation in Dialogue (generated or hand-written), every bark a banter;
- gilded-flagon has camp seats, gulls-wager has start;
- each fight's Npc(...) foes in Combat/EncounterCatalog.cs stand in one room;
- people, spawns, seats and things stand on the floor and in nothing; aboard props exist;
- doors fit the room and their badges don't overlap; nothing stands in an edge exit;
- and every spawn, trigger, door, exit, person and thing in a room can be walked to from its first
  spawn, round the blocks and the props that are always there.
Warnings (printed, not failing): npcs closer than 80 px along x, and rooms with fewer than three
life lines.
"""

from __future__ import annotations

import os
import sys
from collections import deque
from typing import Dict, List, Optional, Set, Tuple

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)

import catalogs  # noqa: E402
import zonefile as zf  # noqa: E402

ROOT = catalogs.ROOT
ZONES = os.path.join(ROOT, "Content", "Zones")

FEET_HALF_W, FEET_D = 20, 16
NPC_FOOT = (40, 16)
TALK_REACH = 112
USE_REACH = 110
SIGN_REACH = 150
DOOR_REACH = 80
CELL = 8
CAMP_ZONE, OPENING_ZONE = "gilded-flagon", "gulls-wager"


class Problems:
    def __init__(self):
        self.errors: List[str] = []
        self.warnings: List[str] = []

    def error(self, where: str, msg: str) -> None:
        self.errors.append(f"{where}: {msg}")

    def warn(self, where: str, msg: str) -> None:
        self.warnings.append(f"{where}: {msg}")


def footprint_boxes(pid: Optional[str], x: float, y: float):
    info = catalogs.props().get(pid or "")
    if info is None or info.footprint == (0, 0):
        return []
    w, h = info.footprint[0] * 4, info.footprint[1] * 4
    left, top = x - w / 2, y - h
    if not info.gap:
        return [(left, top, left + w, y)]
    side = (w - info.gap * 4) / 2
    return [(left, top, left + side, y), (x + info.gap * 2, top, x + info.gap * 2 + side, y)]


def unconditional(p: zf.Presence) -> bool:
    return not p.when and not p.unless


class Walk:
    """The room's floor on a grid of CELL px: where feet fit, and what's reachable from the first spawn."""

    def __init__(self, room: zf.Room):
        self.room = room
        self.boxes = []
        for p in room.props:
            if unconditional(p.presence):
                self.boxes.extend(footprint_boxes(p.prop, p.x, p.y))
        for t in room.things:
            if unconditional(t.presence):
                self.boxes.extend(footprint_boxes(t.look or t.kind, t.x, t.y))
        self.npc_boxes = []
        for n in room.npcs:
            if unconditional(n.presence) and not n.aboard and not n.wander and not n.patrol:
                w, h = NPC_FOOT
                self.npc_boxes.append((n.x - w / 2, n.y - h, n.x + w / 2, n.y))
        ys = [p[1] for p in room.floor]
        self.y0, self.y1 = int(min(ys)), int(max(ys))
        self.cols = room.width // CELL + 1
        self.rows = (self.y1 - self.y0) // CELL + 1
        self.ok = [[self.fits(c * CELL, self.y0 + r * CELL, self.boxes + self.npc_boxes) for c in range(self.cols)]
                   for r in range(self.rows)]
        self.seen: Set[Tuple[int, int]] = set()

    def fits(self, x: float, y: float, boxes) -> bool:
        room = self.room
        for px in (x - FEET_HALF_W, x + FEET_HALF_W):
            if not zf.point_in_polygon(room.floor, px, y) or not zf.point_in_polygon(room.floor, px, y - FEET_D):
                return False
        for b in room.blocks:
            if ((x - b.cx) / (b.rx + FEET_HALF_W)) ** 2 + ((y - b.cy) / (b.ry + FEET_D / 2)) ** 2 < 1:
                return False
        for (l, t, r, bb) in boxes:
            if x + FEET_HALF_W > l and x - FEET_HALF_W < r and y > t and y - FEET_D < bb:
                return False
        return True

    def cell(self, x: float, y: float) -> Tuple[int, int]:
        return (int(round((y - self.y0) / CELL)), int(round(x / CELL)))

    def flood(self, x: float, y: float) -> bool:
        start = self.nearest_ok(x, y, 24)
        if start is None:
            return False
        q = deque([start])
        self.seen = {start}
        while q:
            r, c = q.popleft()
            for dr, dc in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                nr, nc = r + dr, c + dc
                if 0 <= nr < self.rows and 0 <= nc < self.cols and (nr, nc) not in self.seen and self.ok[nr][nc]:
                    self.seen.add((nr, nc))
                    q.append((nr, nc))
        return True

    def nearest_ok(self, x: float, y: float, radius: float) -> Optional[Tuple[int, int]]:
        r0, c0 = self.cell(x, y)
        best = None
        span = int(radius // CELL) + 1
        for r in range(r0 - span, r0 + span + 1):
            for c in range(c0 - span, c0 + span + 1):
                if 0 <= r < self.rows and 0 <= c < self.cols and self.ok[r][c]:
                    d = (c * CELL - x) ** 2 + (self.y0 + r * CELL - y) ** 2
                    if d <= radius * radius and (best is None or d < best[0]):
                        best = (d, (r, c))
        return best[1] if best else None

    def reach(self, x: float, y: float, radius: float) -> bool:
        r0, c0 = self.cell(x, y)
        span = int(radius // CELL) + 1
        for r in range(r0 - span, r0 + span + 1):
            for c in range(c0 - span, c0 + span + 1):
                if (r, c) in self.seen:
                    d = (c * CELL - x) ** 2 + (self.y0 + r * CELL - y) ** 2
                    if d <= radius * radius:
                        return True
        return False

    def reach_box(self, x0: float, x1: float, y0: float = None, y1: float = None) -> bool:
        y0 = self.y0 if y0 is None else y0
        y1 = self.y1 if y1 is None else y1
        for (r, c) in self.seen:
            x, y = c * CELL, self.y0 + r * CELL
            if x0 <= x <= x1 and y0 <= y <= y1:
                return True
        return False


def check_zone(z: zf.Zone, zones: Dict[str, zf.Zone], P: Problems) -> None:
    props = catalogs.props()
    where_z = f"Zones/{z.id}.txt"
    if z.music not in catalogs.tracks():
        P.error(where_z, f"there's no song called {z.music}")
    waypoints = [t for r in z.rooms for t in r.things if t.kind == "waypoint"]
    if len(waypoints) > 1:
        P.error(where_z, f"there are {len(waypoints)} waypoints; a zone has one at most")
    if waypoints and z.world is None:
        P.error(where_z, "there's a waypoint in a zone that isn't on the world map (it needs world =)")

    # An old save finds its room by the legacy rectangle its feet were in, so no two can overlap.
    rects = [(lg, r.id) for r in z.rooms for lg in r.legacy]
    for i, (a, ra) in enumerate(rects):
        for b, rb in rects[i + 1:]:
            if a.x0 < b.x1 and b.x0 < a.x1 and a.y0 < b.y1 and b.y0 < a.y1:
                P.error(where_z, f"legacy {a.x0} {a.y0} {a.x1} {a.y1} ({ra}) overlaps legacy {b.x0} {b.y0} {b.x1} {b.y1} ({rb})")

    for r in z.rooms:
        where = f"{where_z} [room {r.id}]"
        problem = zf.polygon_problem(r.floor)
        if problem:
            P.error(where, problem)

        # Ways out.
        for item in list(r.exits) + list(r.doors):
            line = f"{where} line {item.line}"
            t = item.target
            if t.kind == "zone":
                other = zones.get(t.place)
                if other is None:
                    P.error(line, f"{item.id} goes to {t.place}, which isn't a zone")
                elif other.room_of_spawn(t.spawn) is None:
                    P.error(line, f"{item.id} goes to {t.place} {t.spawn}, and {t.place} has no spawn called {t.spawn}")
            elif t.kind == "room":
                target = z.room(t.place)
                if target is None or target.spawn(t.spawn) is None:
                    P.error(line, f"{item.id} goes to {t.place} {t.spawn}, which isn't there")
            elif t.kind == "locked" and not item.why:
                P.error(line, f"{item.id} is locked and doesn't say why")
            if isinstance(item, zf.Door) and item.badge not in zf.BADGES:
                P.error(line, f"there's no badge called {item.badge}")

        # Doors fit, and their badges don't overlap.
        for d in r.doors:
            left, top, w, h = r.door_box(d)
            if left < 0 or left + w > r.width or top < 0:
                P.error(f"{where} line {d.line}", f"door {d.id}'s box ({left:g} {top:g} {w:g} {h:g}) doesn't fit the room")
        doors = sorted(r.doors, key=lambda d: d.x)
        for a, b in zip(doors, doors[1:]):
            if a.x == b.x:
                continue
            need = (max(a.box_size()[0], 160) + max(b.box_size()[0], 160)) / 2 + 40
            if b.x - a.x < need:
                P.error(f"{where} line {b.line}", f"doors {a.id} and {b.id} are {b.x - a.x:g} px apart, and their badges need {need:g}")

        # The tile strip.
        tiles = r.tiles
        names = catalogs.tile_names(tiles.tileset)
        if names is None:
            P.error(where, f"there's no tileset called {tiles.tileset}")
        else:
            for c, name in tiles.legend.items():
                if name not in names:
                    P.error(where, f"the {tiles.tileset} tileset has no tile called {name} (legend '{c}')")
            used = {c for row in tiles.rows + tiles.detail for c in row if c != zf.EMPTY}
            for c in sorted(used - set(tiles.legend)):
                P.error(where, f"'{c}' is in the tile rows but not the legend")
        cols = r.width // zf.TILE
        for row in tiles.rows + tiles.detail:
            if len(row) != cols:
                P.error(where, f"a tile row is {len(row)} wide, and the room is {cols} tiles")

        # Props, scenery and things.
        for s in r.scenery:
            if s.prop not in props:
                P.error(f"{where} line {s.line}", f"there's no prop called {s.prop}")
        for p in r.props:
            if p.prop not in props:
                P.error(f"{where} line {p.line}", f"there's no prop called {p.prop}")
        for t in r.things:
            look = t.look or t.kind
            info = props.get(look)
            line = f"{where} line {t.line}"
            if info is None:
                P.error(line, f"there's no prop called {look} for {t.id} to look like")
            else:
                want = 1 if t.kind == "sign" else 2
                if info.frames != want:
                    P.error(line, f"the {look} prop has {info.frames} pictures, and a {t.kind} needs {want}")
            for item in t.items:
                if item not in catalogs.items():
                    P.error(line, f"there's no item called {item}")
            if t.kind == "sign" and not t.text:
                P.error(line, f"sign {t.id} has no text")
            if t.kind == "lever" and not t.flag:
                P.error(line, f"lever {t.id} has no flag")

        # People.
        cast = catalogs.cast()
        for n in r.npcs:
            line = f"{where} line {n.line}"
            if n.cast is not None:
                member = cast.get(n.cast)
                if member is None:
                    P.error(line, f"there's nobody in the cast called {n.cast}")
                elif not member.standing:
                    P.error(line, f"{n.cast} is only ever a voice, and can't stand in a room")
            if n.sheet is not None and n.sheet not in catalogs.townsfolk():
                P.error(line, f"there's no sheet called {n.sheet}")
            if n.creature is not None and n.creature not in catalogs.creatures():
                P.error(line, f"there's no creature called {n.creature}")
            if n.talk is not None and n.talk not in catalogs.dialogue_ids():
                P.error(line, f"there's no conversation called {n.talk}")
            if n.aboard is not None and not any(p.prop == n.aboard or p.id == n.aboard for p in r.props):
                P.error(line, f"{n.id} is aboard {n.aboard}, but there's no {n.aboard} in this room")
        for t in r.triggers:
            line = f"{where} line {t.line}"
            if t.talk is not None and t.talk not in catalogs.dialogue_ids():
                P.error(line, f"there's no conversation called {t.talk}")
            if t.bark is not None and t.bark not in catalogs.banters():
                P.error(line, f"there's no banter called {t.bark}")
        for s in r.scenes:
            if s.talk not in catalogs.dialogue_ids():
                P.error(f"{where} line {s.line}", f"there's no conversation called {s.talk}")

        # Nothing in an edge exit.
        sides = {e.side for e in r.exits}
        for o in list(r.spawns) + [n for n in r.npcs if not n.aboard] + list(r.things) + list(r.camp):
            if ("west" in sides and o.x < zf.EDGE_EXIT) or ("east" in sides and o.x > r.width - zf.EDGE_EXIT):
                P.error(f"{where} line {o.line}", f"{getattr(o, 'id', None) or o.name} stands in an edge exit")

        # Standing somewhere they could.
        walk = Walk(r)
        everything = walk.boxes
        for o in list(r.spawns) + [n for n in r.npcs if not n.aboard] + list(r.camp):
            own = []
            if isinstance(o, zf.Npc):
                own = [(o.x - NPC_FOOT[0] / 2, o.y - NPC_FOOT[1], o.x + NPC_FOOT[0] / 2, o.y)]
            if not walk.fits(o.x, o.y, everything):
                name = getattr(o, "id", None) or o.name
                P.error(f"{where} line {o.line}", f"{name} at {o.x:g} {o.y:g} isn't standing on clear floor")
        for t in r.things:
            own = footprint_boxes(t.look or t.kind, t.x, t.y)
            others = [b for b in everything if b not in own]
            if not walk.fits(t.x, t.y, [b for b in others]):
                P.error(f"{where} line {t.line}", f"{t.id} at {t.x:g} {t.y:g} isn't on clear floor")

        # Spacing, as a warning.
        people = sorted([n for n in r.npcs if not n.aboard], key=lambda n: n.x)
        for a, b in zip(people, people[1:]):
            # One of the cast in two places at two points in the story (Crane at court, then in the hall)
            # is one person, so never beside themselves.
            same_person = a.cast and a.cast == b.cast and not unconditional(a.presence) and not unconditional(b.presence)
            if b.x - a.x < 80 and not same_person and not (a.presence.when and a.presence.when == b.presence.unless) \
                    and not (a.presence.unless and a.presence.unless == b.presence.when):
                P.warn(where, f"{a.id} and {b.id} are only {b.x - a.x:g} px apart")
        if len(r.life) < 3:
            P.warn(where, f"only {len(r.life)} life lines (12-rooms §3.2 wants three moving things)")

        # Walking.
        first = r.spawns[0]
        if not walk.flood(first.x, first.y):
            P.error(where, f"spawn {first.name} has nowhere to stand")
            continue

        def unreachable(what: str, line: int) -> None:
            P.error(f"{where} line {line}", f"{what} can't be walked to from spawn {first.name}")

        for s in r.spawns:
            if not walk.reach(s.x, s.y, 24):
                unreachable(f"spawn {s.name}", s.line)
        for t in r.triggers:
            if not walk.reach_box(t.x0, t.x1, t.y0, t.y1):
                unreachable(f"trigger {t.id}", t.line)
        for d in r.doors:
            foot = r.back_edge_at(d.x) + FEET_D + 4
            if not walk.reach(d.x, foot, DOOR_REACH):
                unreachable(f"door {d.id}'s foot", d.line)
        for e in r.exits:
            if e.side == "west":
                ok = walk.reach_box(0, zf.EDGE_EXIT)
            elif e.side == "east":
                ok = walk.reach_box(r.width - zf.EDGE_EXIT, r.width)
            else:
                ok = walk.reach_box(e.x - 60, e.x + 60)
            if not ok:
                unreachable(f"exit {e.id}", e.line)
        for n in r.npcs:
            if n.aboard:
                continue
            if not walk.reach(n.x, n.y, TALK_REACH):
                unreachable(f"npc {n.id}", n.line)
        for t in r.things:
            if not walk.reach(t.x, t.y, SIGN_REACH if t.kind == "sign" else USE_REACH):
                unreachable(f"{t.kind} {t.id}", t.line)


def main(argv: List[str]) -> int:
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
    P = Problems()
    zones: Dict[str, zf.Zone] = {}
    for name in sorted(os.listdir(ZONES)):
        if not name.endswith(".txt"):
            continue
        try:
            z = zf.load_zone(os.path.join(ZONES, name))
        except zf.ZoneError as err:
            P.errors.append(str(err))
            continue
        zones[z.id] = z

    wanted = argv or sorted(zones)
    for zid in wanted:
        if zid in zones:
            check_zone(zones[zid], zones, P)
        elif not any(e.startswith(f"Zones/{zid}.txt") for e in P.errors):
            P.errors.append(f"Zones/{zid}.txt: there's no such zone")

    if not argv:
        camp = zones.get(CAMP_ZONE)
        if camp is None or not any(r.camp for r in camp.rooms):
            P.errors.append(f"Zones/{CAMP_ZONE}.txt: the camp zone has no camp seats")
        opening = zones.get(OPENING_ZONE)
        if opening is None or opening.room_of_spawn("start") is None:
            P.errors.append(f"Zones/{OPENING_ZONE}.txt: the opening zone has no spawn called start")
        # Each fight's foes stand in one room.
        where_npc: Dict[str, List[str]] = {}
        for z in zones.values():
            for r in z.rooms:
                for n in r.npcs:
                    where_npc.setdefault(n.id, []).append(f"{z.id}/{r.id}")
        for fight, foes in catalogs.encounter_foes().items():
            rooms = set()
            for foe in foes:
                found = where_npc.get(foe)
                if not found:
                    P.errors.append(f"Combat/EncounterCatalog.cs: {fight}'s foe {foe} isn't in any room")
                    continue
                rooms.update(found if len(found) == 1 else [f for f in found if f])
            if len(rooms) > 1:
                # A shared id (kael is in the vault and at the Flagon) is fine as long as one room has all of them.
                common = None
                for foe in foes:
                    s = set(where_npc.get(foe, []))
                    common = s if common is None else common & s
                if not common:
                    P.errors.append(f"Combat/EncounterCatalog.cs: {fight}'s foes are in different rooms: {', '.join(sorted(rooms))}")
            # And inside the room's fight range, where the grid's laid (m3-plan §6).
            for z in zones.values():
                for r in z.rooms:
                    here = [n for n in r.npcs if n.id in foes]
                    if len(here) != len(foes):
                        continue
                    left, right = r.fight_range()
                    for n in here:
                        if not left <= n.x <= right:
                            P.errors.append(f"Zones/{z.id}.txt [room {r.id}] line {n.line}: {fight}'s foe {n.id} at x "
                                            f"{n.x:g} is outside the room's fight range {left:g} to {right:g}")

    for w in P.warnings:
        print("warning: " + w)
    for e in P.errors:
        print(e)
    rooms = sum(len(z.rooms) for z in zones.values())
    print(f"{len(zones)} zones, {rooms} rooms: {len(P.errors)} problems, {len(P.warnings)} warnings")
    return 1 if P.errors else 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
