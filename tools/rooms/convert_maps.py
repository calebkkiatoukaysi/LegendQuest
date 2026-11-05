#!/usr/bin/env python3
"""Turns the old top-down maps in tools/rooms/legacy/*.map into the side-view zones in Content/Zones/*.txt.

Usage:  python tools/rooms/convert_maps.py            writes all 19 zones
        python tools/rooms/convert_maps.py --check    diffs what it would write against the committed files
        python tools/rooms/convert_maps.py --report   lists every old id and the room it landed in
        python tools/rooms/convert_maps.py docks      just those zones (with any of the above)

Each zone's hand-made part is in plan.py: its rooms (their source tile rectangles in the old map,
widths, floors, blocks and fight ranges), door badges, the locked twins, npc badges, the new script
spawns, and scenery and life. Everything else comes out of the .map by these rules (m3-plan §2):

- an object's x maps linearly from its room's source rectangle onto [64, width - 64], and its row
  onto the band's depth [610, 850] (a rotated room swaps the two);
- triggers span the full depth of the band;
- map-edge exits become edge exits, entrances become doors on the band's back edge;
- facing up or down becomes facing toward the room's centre;
- wander is multiplied by 64 (tiles to canvas px), and so is a light's reach (its art px times 4);
- the tile strip is resampled from the old ground rows through the room's most walkable stretch;
- building props go in scenery mid, front props in scenery fore, props standing on the band in prop
  lines (wall props, and props with a light or a when/unless, stay props on the band's back edge).

It's deterministic: two runs give the same bytes. Never hand-edit a zone file: change plan.py.
"""

from __future__ import annotations

import argparse
import difflib
import math
import os
import sys
from dataclasses import dataclass, field
from typing import Dict, List, Optional, Tuple

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)

import catalogs  # noqa: E402
import plan as P  # noqa: E402
import zonefile as zf  # noqa: E402

ROOT = catalogs.ROOT
ZONES = os.path.join(ROOT, "Content", "Zones")

BAND_TOP, BAND_BOTTOM = 610, 850
FLOOR_TOP, FLOOR_BOTTOM = 600, 860
MARGIN = 64
NPC_SPACING = 80
WALL_PROP_RISE = 32
"""A wall prop hangs this far above the floor's back edge (the old half tile, at 4x)."""
FORE_SPACING = 480
"""Front props kept for the foreground at least this far apart, so it stays light (12-rooms §3.1)."""


# ---------------------------------------------------------------------------------------------
# Reading a .map

@dataclass
class MapObj:
    kind: str
    id: str
    x: float
    y: float
    s: Dict[str, str]
    line: int

    def get(self, key: str, default=None):
        return self.s.get(key, default)

    @property
    def feet(self) -> Tuple[float, float]:
        """Where it stands, in tiles: the middle of the tile it names (an entrance's x, y already is)."""
        if self.kind == "entrance":
            return (self.x, self.y)
        return (self.x + 0.5, self.y + 0.5)

    def area(self) -> Tuple[float, float, float, float]:
        w = float(self.s.get("w", 1))
        h = float(self.s.get("h", 1))
        return (self.x, self.y, self.x + w, self.y + h)

    def presence(self) -> zf.Presence:
        return zf.Presence(self.s.get("when"), self.s.get("unless"))


@dataclass
class MapData:
    id: str
    settings: Dict[str, str]
    legend: Dict[str, str]
    ground: List[str]
    detail: List[str]
    objects: List[MapObj]

    @property
    def width(self) -> int:
        return len(self.ground[0])

    @property
    def height(self) -> int:
        return len(self.ground)

    def tile(self, layer: List[str], x: int, y: int) -> str:
        y = min(max(y, 0), self.height - 1)
        x = min(max(x, 0), self.width - 1)
        return layer[y][x] if layer else "."


def _split(text: str) -> List[str]:
    parts, cur, quoted = [], [], False
    for c in text:
        if c == '"':
            quoted = not quoted
        elif c == " " and not quoted:
            if cur:
                parts.append("".join(cur))
            cur = []
        else:
            cur.append(c)
    if cur:
        parts.append("".join(cur))
    return parts


def read_map(map_id: str) -> MapData:
    path = catalogs.map_file(map_id)
    sections: Dict[str, List[Tuple[int, str]]] = {}
    cur = None
    with open(path, encoding="utf-8") as fh:
        for n, raw in enumerate(fh, 1):
            line = raw.split(";", 1)[0].strip()
            if not line:
                continue
            if line.startswith("["):
                cur = line[1:-1].strip().lower()
                sections[cur] = []
                continue
            sections[cur].append((n, line))
    settings = {}
    for _, t in sections["map"]:
        k, v = t.split("=", 1)
        settings[k.strip().lower()] = v.strip()
    legend = {}
    for _, t in sections["legend"]:
        k, v = t.split("=", 1) if not t.startswith("=") else ("=", t.split("=", 2)[2])
        legend[k.strip()] = v.strip()
    ground = [t for _, t in sections["ground"]]
    detail = [t for _, t in sections.get("detail", [])]
    objects = []
    for n, t in sections.get("objects", []):
        parts = _split(t)
        s = {}
        for p in parts[4:]:
            k, v = p.split("=", 1)
            s[k.lower()] = v
        objects.append(MapObj(parts[0].lower(), parts[1], float(parts[2]), float(parts[3]), s, n))
    return MapData(map_id, settings, legend, ground, detail, objects)


# ---------------------------------------------------------------------------------------------
# Mapping

def _r(v: float) -> int:
    return int(math.floor(v + 0.5))


class RoomMap:
    """One room being built: its plan, the zonefile room, and how old tiles map onto it."""

    def __init__(self, zp: "P.ZonePlan", rp: "P.RoomPlan", m: MapData):
        self.zp = zp
        self.rp = rp
        self.m = m
        sx0, sy0, sx1, sy1 = rp.src
        self.sx0, self.sy0, self.sx1, self.sy1 = sx0, sy0, sx1, sy1
        self.W = rp.width
        dark = rp.dark if rp.dark is not None else m.settings.get("dark", "no") == "yes"
        ambient = None
        if dark:
            ambient = rp.ambient if rp.ambient is not None else float(m.settings.get("ambient", "1"))
        tint = rp.tint
        if tint is None and "tint" in m.settings:
            tint = tuple(int(c) for c in m.settings["tint"].split())
        floor = rp.floor or [(0, FLOOR_TOP), (self.W, FLOOR_TOP), (self.W, FLOOR_BOTTOM), (0, FLOOR_BOTTOM)]
        self.room = zf.Room(id=rp.id, name=rp.name, kind=rp.kind, width=self.W, paint=None, dark=dark,
                            ambient=ambient, tint=tint, floor=list(floor),
                            blocks=[zf.Block(*b) for b in rp.blocks], fight=rp.fight,
                            legacy=[zf.Legacy(*(int(v) for v in r)) for r in
                                    (rp.legacy or ([(0, 0, m.width, m.height)] if len(zp.rooms) == 1 else [rp.src]))],
                            states=dict(rp.states))
        self.room.paint = f"{zp.id}/{rp.id}"
        self.fore_xs: List[float] = []

    # Old tiles to canvas px.
    def map_x(self, ox: float, oy: float) -> float:
        u, lo, hi = (oy, self.sy0, self.sy1) if self.rp.rotate else (ox, self.sx0, self.sx1)
        t = (u - lo) / (hi - lo)
        if self.rp.flip:
            t = 1 - t
        t = min(max(t, 0.0), 1.0)
        return MARGIN + t * (self.W - 2 * MARGIN)

    def map_y(self, ox: float, oy: float) -> float:
        v, lo, hi = (ox, self.sx0, self.sx1) if self.rp.rotate else (oy, self.sy0, self.sy1)
        t = (v - lo) / (hi - lo)
        t = min(max(t, 0.0), 1.0)
        return BAND_TOP + t * (BAND_BOTTOM - BAND_TOP)

    def place(self, ox: float, oy: float) -> Tuple[int, int]:
        return _r(self.map_x(ox, oy)), _r(self.map_y(ox, oy))

    def behind(self, oy_or_x: Tuple[float, float]) -> int:
        """-1 if a point's behind the band (north of the source rows), 1 if in front, 0 on it."""
        ox, oy = oy_or_x
        v, lo, hi = (ox, self.sx0, self.sx1) if self.rp.rotate else (oy, self.sy0, self.sy1)
        if v < lo:
            return -1
        if v > hi:
            return 1
        return 0

    def back_edge(self, x: float) -> int:
        return _r(zf.back_edge_at(self.room.floor, x))

    def front_edge(self, x: float) -> int:
        return _r(zf.front_edge_at(self.room.floor, x))

    def toward_centre(self, x: float) -> str:
        return "right" if x < self.W / 2 else "left"

    def facing(self, old: Optional[str], x: float) -> str:
        if self.rp.rotate:
            return {"down": "right", "up": "left"}.get(old or "", self.toward_centre(x))
        if old in ("left", "right"):
            return old
        return self.toward_centre(x)

    def contains(self, ox: float, oy: float) -> bool:
        for (x0, y0, x1, y1) in (self.rp.legacy or [self.rp.src]):
            if x0 <= ox < x1 and y0 <= oy < y1:
                return True
        return False


# ---------------------------------------------------------------------------------------------
# Converting one zone

class Converter:
    def __init__(self, zp: "P.ZonePlan"):
        self.zp = zp
        self.m = read_map(zp.map or zp.id)
        self.rooms = [RoomMap(zp, rp, self.m) for rp in zp.rooms]
        self.by_id = {r.rp.id: r for r in self.rooms}
        self.lookup = sorted(self.rooms, key=lambda r: (r.rp.priority, zp.rooms.index(r.rp)))
        self.report: List[Tuple[str, str, str]] = []
        self.props = catalogs.props()
        self.tilesets = catalogs.tilesets()

    def note(self, kind: str, ident: str, where: str) -> None:
        self.report.append((kind, ident, where))

    def room_for(self, o: MapObj, point: Tuple[float, float]) -> Optional[RoomMap]:
        if o.id in self.zp.assign and o.kind != "prop":
            return self.by_id[self.zp.assign[o.id]]
        for r in self.lookup:
            if r.contains(*point):
                return r
        # A zone of one room has everything in it, the walls and the doorway too.
        return self.rooms[0] if len(self.rooms) == 1 else None

    def placed(self, o: MapObj, r: RoomMap, point: Tuple[float, float]) -> Tuple[int, int]:
        x, y = r.place(*point)
        over = self.zp.place.get(o.id) if o.kind != "prop" else None
        if over is not None:
            x = over[0] if over[0] is not None else x
            y = over[1] if len(over) > 1 and over[1] is not None else y
        return x, y

    # -- the conversion -------------------------------------------------------------------
    def run(self) -> zf.Zone:
        m, zp = self.m, self.zp
        world = None
        if "world" in m.settings:
            wx, wy = m.settings["world"].split()
            world = (int(wx), int(wy))
        zone = zf.Zone(id=zp.id, name=zp.name or m.settings["name"], world=world,
                       music=zp.music or m.settings["music"], region=zp.region, danger=zp.danger,
                       comments=list(zp.comments))

        self.ties = self.tie_buildings()
        tied = {id(b) for b, _ in self.ties.values()}
        for o in m.objects:
            if o.kind != "prop" and o.id in zp.drop:
                self.note(o.kind, o.id, f"dropped: {zp.drop[o.id]}")
                continue
            if o.kind == "prop" and id(o) in tied:
                continue
            handler = getattr(self, "do_" + o.kind)
            handler(o)

        for r in self.rooms:
            self.add_plan_objects(r)
        have = {o.id for o in m.objects if o.kind == "spawn"}
        for name, (room, x, y, facing) in zp.spawn_fallbacks.items():
            if name not in have:
                self.by_id[room].room.spawns.append(zf.Spawn(name, x, y, facing))
                self.note("spawn", name, f"{room} (from plan.py: the .map doesn't have it)")
        self.add_links()
        for t in self.pending_twins:
            owner = self.owner_room(t.of)
            if owner is None:
                raise SystemExit(f"{self.zp.id}: the twin {t.id} is of {t.of}, which isn't anywhere")
            self.add_twin(self.by_id[owner], t.of, t)
        for r in self.rooms:
            self.spread_npcs(r)
            self.spread_doors(r)
            self.place_buildings(r)
            self.keep_clear_of_edges(r)
            self.settle(r)
            self.build_strip(r)
            self.add_life(r)
            r.room.comments = list(r.rp.comments)
        zone.rooms = [r.room for r in self.rooms]
        self.check_ids(zone)
        return zone

    def check_ids(self, zone: zf.Zone) -> None:
        seen: Dict[str, str] = {}
        spawns: Dict[str, str] = {}
        for r in zone.rooms:
            for ident, _, kind in r.ids():
                if ident in seen:
                    raise SystemExit(f"{zone.id}: two things are called {ident} ({seen[ident]} and {kind} in {r.id})")
                seen[ident] = f"{kind} in {r.id}"
            for s in r.spawns:
                if s.name in spawns:
                    raise SystemExit(f"{zone.id}: two spawns are called {s.name} ({spawns[s.name]} and {r.id})")
                spawns[s.name] = r.id

    # -- each kind ------------------------------------------------------------------------
    def do_spawn(self, o: MapObj) -> None:
        r = self.room_for(o, o.feet)
        if r is None:
            raise SystemExit(f"{self.zp.id}: spawn {o.id} at {o.x} {o.y} is in no room")
        x, y = self.placed(o, r, o.feet)
        facing = self.zp.facing.get(o.id) or r.facing(o.get("facing"), x)
        r.room.spawns.append(zf.Spawn(o.id, x, y, facing))
        self.note("spawn", o.id, r.rp.id)

    def target_of(self, o: MapObj) -> zf.Target:
        to = o.get("to")
        if to == "back":
            return zf.Target("back")
        return zf.Target("zone", to, o.get("at", "start"))

    def do_exit(self, o: MapObj) -> None:
        spec = self.zp.exits.get(o.id)
        if spec is None:
            raise SystemExit(f"{self.zp.id}: plan.py doesn't say what exit {o.id} becomes")
        x0, y0, x1, y1 = o.area()
        centre = ((x0 + x1) / 2, (y0 + y1) / 2)
        r = self.by_id[spec.room] if spec.room else self.room_for(o, centre)
        target = spec.target or self.target_of(o)
        presence = o.presence()
        if spec.side in ("west", "east"):
            r.room.exits.append(zf.Exit(o.id, spec.side, target, None, spec.label, None, spec.travel, presence))
        elif spec.side in ("up", "down"):
            x = spec.x if spec.x is not None else _r(r.map_x(*centre))
            r.room.exits.append(zf.Exit(o.id, spec.side, target, x, spec.label, None, spec.travel, presence))
        else:
            x = spec.x if spec.x is not None else _r(r.map_x(*centre))
            r.room.doors.append(zf.Door(o.id, x, target, spec.badge, spec.name or self.name_of(target),
                                        None, spec.size, presence))
        self.note("exit", o.id, f"{r.rp.id} ({'door' if spec.side == 'door' else spec.side + ' exit'})")
        if spec.twin:
            self.add_twin(r, o.id, spec.twin)

    def name_of(self, target: zf.Target) -> str:
        if target.kind == "back":
            return "The Way Out"
        return P.ZONE_NAMES.get(target.place, target.place)

    def do_entrance(self, o: MapObj) -> None:
        r = self.room_for(o, o.feet)
        if r is None:
            raise SystemExit(f"{self.zp.id}: entrance {o.id} is in no room")
        x, _ = self.placed(o, r, o.feet)
        spec = self.zp.entrances.get(o.id, P.Entrance())
        name = spec.name or o.get("name") or self.name_of(self.target_of(o))
        r.room.doors.append(zf.Door(o.id, x, self.target_of(o), spec.badge, name, None, spec.size, o.presence()))
        self.note("entrance", o.id, f"{r.rp.id} (door)")

    def add_twin(self, r: RoomMap, of: str, twin: "P.Twin") -> None:
        """A locked twin of an exit or door: the same place, locked, with when and unless the other way round."""
        for d in r.room.doors:
            if d.id == of:
                r.room.doors.append(zf.Door(twin.id, d.x, zf.Target("locked"), d.badge, d.name, twin.why, d.size,
                                            zf.Presence(None, twin.unless)))
                return
        for e in r.room.exits:
            if e.id == of:
                r.room.exits.append(zf.Exit(twin.id, e.side, zf.Target("locked"), e.x, twin.label or e.label, twin.why,
                                            False, zf.Presence(None, twin.unless)))
                return
        raise SystemExit(f"{self.zp.id}: the twin {twin.id} is of {of}, which isn't in room {r.rp.id}")

    def do_trigger(self, o: MapObj) -> None:
        twin = self.zp.twins.get(o.id)
        if twin is not None:
            # The old *-shut trigger is now the locked twin of a door or exit. Its line is the why.
            if twin.why is None:
                twin.why = o.get("say")
            if twin.unless is None:
                twin.unless = o.get("unless")
            self.note("trigger", o.id, f"locked twin of {twin.of} (in {self.find_owner(twin.of)})")
            self.pending_twins.append(twin)
            return
        x0, y0, x1, y1 = o.area()
        centre = ((x0 + x1) / 2, (y0 + y1) / 2)
        r = self.room_for(o, centre)
        if r is None:
            raise SystemExit(f"{self.zp.id}: trigger {o.id} is in no room")
        over = self.zp.place.get(o.id)
        if self.zp.span.get(o.id):
            a, b = self.zp.span[o.id]
        else:
            if r.rp.rotate:
                a, b = sorted((r.map_x(x0, y0), r.map_x(x0, y1)))
            else:
                a, b = sorted((r.map_x(x0, y0), r.map_x(x1, y0)))
            if over is not None and over[0] is not None:
                half = (b - a) / 2
                a, b = over[0] - half, over[0] + half
        a, b = _r(max(a, 0)), _r(min(b, r.W))
        if b - a < 64:
            mid = (a + b) / 2
            a, b = _r(max(0, mid - 32)), _r(min(r.W, mid + 32))
        top = min(p[1] for p in r.room.floor)
        bottom = max(p[1] for p in r.room.floor)
        r.room.triggers.append(zf.Trigger(o.id, a, top, b, bottom, o.get("talk"), o.get("say"), o.get("set"),
                                          o.get("bark"), o.get("mark") == "yes", o.get("once", "yes") != "no",
                                          o.presence()))
        self.note("trigger", o.id, r.rp.id)

    def find_owner(self, ident: str) -> str:
        if ident in self.zp.exits and self.zp.exits[ident].room:
            return self.zp.exits[ident].room
        if ident in self.zp.idoors and self.zp.idoors[ident].room:
            return self.zp.idoors[ident].room
        for o in self.m.objects:
            if o.id == ident and o.kind != "prop":
                r = self.room_for(o, o.feet)
                return r.rp.id if r else "?"
        return "?"

    def do_scene(self, o: MapObj) -> None:
        r = self.room_for(o, o.feet)
        if r is None:
            raise SystemExit(f"{self.zp.id}: scene {o.id} is in no room")
        r.room.scenes.append(zf.Scene(o.id, o.get("talk"), o.presence()))
        self.note("scene", o.id, r.rp.id)

    def do_thing(self, o: MapObj) -> None:
        r = self.room_for(o, o.feet)
        if r is None:
            raise SystemExit(f"{self.zp.id}: {o.kind} {o.id} is in no room")
        x, y = self.placed(o, r, o.feet)
        t = zf.Thing(o.kind, o.id, x, y, o.get("look"), o.get("name"))
        if o.kind == "chest":
            t.gold = int(o.get("gold", 0))
            t.items = [i.strip() for i in o.get("items", "").split(",") if i.strip()]
        t.text = o.get("text")
        t.flag = o.get("flag")
        t.presence = o.presence()
        r.room.things.append(t)
        self.note(o.kind, o.id, r.rp.id)

    do_chest = do_sign = do_lever = do_waypoint = do_thing

    def do_door(self, o: MapObj) -> None:
        """An old door you opened by hand (or a flag worked) becomes a room door or an edge exit."""
        spec = self.zp.idoors.get(o.id)
        if spec is None:
            raise SystemExit(f"{self.zp.id}: plan.py doesn't say what door {o.id} becomes")
        r = self.by_id[spec.room] if spec.room else self.room_for(o, o.feet)
        if spec.side == "link":
            self.note("door", o.id, f"{r.rp.id} (a way to another room, from plan.py's links)")
            return
        flag = o.get("flag")
        presence = zf.Presence(flag, None) if flag else o.presence()
        target = spec.target if spec.target is not None else zf.Target("locked")
        why = spec.why if target.kind == "locked" else None
        if spec.side in ("west", "east"):
            r.room.exits.append(zf.Exit(o.id, spec.side, target, None, spec.label or o.get("name"), why, False, presence))
        else:
            x = spec.x if spec.x is not None else _r(r.map_x(*o.feet))
            r.room.doors.append(zf.Door(o.id, x, target, spec.badge, spec.name or o.get("name", "A Door"), why,
                                        spec.size, presence))
        self.note("door", o.id, f"{r.rp.id} ({'door' if spec.side == 'door' else spec.side + ' exit'})")
        if spec.twin:
            if spec.twin.unless is None:
                spec.twin.unless = flag
            self.add_twin(r, o.id, spec.twin)

    def do_npc(self, o: MapObj) -> None:
        r = self.room_for(o, o.feet)
        if r is None:
            raise SystemExit(f"{self.zp.id}: npc {o.id} at {o.x} {o.y} is in no room")
        x, y = self.placed(o, r, o.feet)
        aboard = o.get("aboard")
        if aboard:
            ship = next((p for p in self.m.objects if p.kind == "prop" and p.id == aboard), None)
            sx, _ = r.place(*ship.feet)
            dx = (o.feet[0] - ship.feet[0]) * 64
            x, y = _r(sx + dx), r.back_edge(sx) + 4
        look = o.get("look")
        wander = float(o.get("wander", 0)) * 64
        n = zf.Npc(o.id, x, y, o.get("cast"), o.get("sheet"), int(look) if look else None, o.get("creature"),
                   o.get("name"), self.zp.facing.get(o.id) or r.facing(o.get("facing"), x), wander, None,
                   self.zp.badges.get(o.id), aboard, [s.strip() for s in o.get("say", "").split("|") if s.strip()],
                   o.get("talk"), o.presence())
        if o.id in self.zp.patrol:
            n.patrol = self.zp.patrol[o.id]
            n.wander = 0
        r.room.npcs.append(n)
        self.note("npc", o.id, r.rp.id)

    def do_light(self, o: MapObj) -> None:
        r = self.room_for(o, o.feet)
        if r is None:
            self.note("light", o.id, "dropped: in no room")
            return
        x, y = self.placed(o, r, o.feet)
        reach = float(o.get("reach")) * 64
        strength = float(o.get("strength", 1))
        r.room.lights.append(zf.Light(x, y, reach, strength, o.id))
        self.note("light", o.id, r.rp.id)

    def do_camp(self, o: MapObj) -> None:
        r = self.room_for(o, o.feet)
        x, y = self.placed(o, r, o.feet)
        r.room.camp.append(zf.Camp(o.id, x, y, self.zp.facing.get(o.id) or r.facing(o.get("facing"), x)))
        self.note("camp", o.id, r.rp.id)

    def do_prop(self, o: MapObj) -> None:
        base = P.base_prop(o.id)
        r = None
        key = f"{o.id}@{o.x:g},{o.y:g}"
        if key in self.zp.prop_room:
            r = self.by_id[self.zp.prop_room[key]] if self.zp.prop_room[key] else None
            if r is None:
                return
        else:
            for cand in self.lookup:
                if cand.contains(*o.feet):
                    r = cand
                    break
            if r is None and len(self.rooms) == 1:
                r = self.rooms[0]
        if r is None:
            return
        x = _r(r.map_x(*o.feet))
        info = self.props.get(o.id)
        if info is None:
            raise SystemExit(f"{self.zp.id}: there's no prop called {o.id}")
        side = r.behind(o.feet)
        presence = o.presence()
        layer = self.zp.prop_layer.get(key) or self.zp.prop_layer.get(o.id) or r.rp.prop_layer.get(o.id)
        if layer is None:
            if base in P.BUILDINGS and not presence.when and not presence.unless:
                layer = "mid"
            elif o.id in P.FAR_PROPS:
                layer = "far" if side < 0 else "mid"
            elif info.placement == "Wall" or info.light or presence.when or presence.unless or o.id in P.STORY_PROPS:
                layer = "prop" if side <= 0 or r.rp.kind == "interior" else "fore"
                if layer == "prop" and side < 0:
                    layer = "back"
            elif side < 0:
                layer = "mid"
            elif side > 0:
                layer = "fore"
            else:
                layer = "prop"
            if r.rp.kind == "interior" and layer in ("mid", "fore"):
                layer = "back" if side <= 0 else "fore"
        if layer == "drop":
            return
        if layer in ("far", "mid", "fore"):
            if layer == "fore":
                if base not in P.FORE_OK and base not in P.BUILDINGS:
                    return
                elif base in P.BUILDINGS:
                    layer = "mid"
                elif any(abs(x - fx) < FORE_SPACING for fx in r.fore_xs):
                    return
                else:
                    r.fore_xs.append(x)
            if any(s.prop == o.id and abs(s.x - x) < 8 and s.layer == layer for s in r.room.scenery):
                return
            r.room.scenery.append(zf.Scenery(layer, o.id, x))
            return
        # A prop line: on the band (prop), on the back edge (back), or hung on the wall (wall).
        if info.placement == "Wall" and r.rp.wall_sections:
            # A turned room's back wall runs along the old map's x: each stretch of wall takes its props
            # from one part of the old map, spread by where they were along it.
            ox, oy = o.feet
            for (y0, y1, nx0, nx1) in r.rp.wall_sections:
                if y0 <= oy < y1:
                    t = (ox - r.sx0) / (r.sx1 - r.sx0)
                    x = _r(nx0 + min(max(t, 0.0), 1.0) * (nx1 - nx0))
                    break
        if layer == "back" or (layer == "prop" and info.placement == "Wall"):
            y = r.back_edge(x) - (WALL_PROP_RISE if info.placement == "Wall" else 0)
        else:
            _, y = r.place(*o.feet)
        over = self.zp.prop_place.get(key)
        if over is not None:
            x = over[0] if over[0] is not None else x
            y = over[1] if len(over) > 1 and over[1] is not None else y
        if any(p.prop == o.id and abs(p.x - x) < 24 and abs(p.y - y) < 4 and p.presence == presence
               for p in r.room.props):
            return
        r.room.props.append(zf.Prop(o.id, x, y, None, presence))

    # -- what the plan adds ---------------------------------------------------------------
    pending_twins: List["P.Twin"]

    def add_plan_objects(self, r: RoomMap) -> None:
        rp = r.rp
        for s in rp.spawns:
            r.room.spawns.append(zf.Spawn(s[0], s[1], s[2], s[3] if len(s) > 3 else r.toward_centre(s[1])))
            self.note("spawn", s[0], f"{rp.id} (new)")
        for d in rp.doors:
            r.room.doors.append(d)
            self.note("door", d.id, f"{rp.id} (new)")
        for e in rp.exits:
            r.room.exits.append(e)
            self.note("exit", e.id, f"{rp.id} (new)")
        for p in rp.props:
            r.room.props.append(p)
        for t in rp.triggers:
            r.room.triggers.append(t)
        for s in rp.scenery:
            r.room.scenery.append(zf.Scenery(*s))

    def owner_room(self, ident: str) -> Optional[str]:
        for r in self.rooms:
            if any(d.id == ident for d in r.room.doors) or any(e.id == ident for e in r.room.exits):
                return r.rp.id
        return None

    def add_links(self) -> None:
        """Each link between two rooms of the zone: an exit each way, and a spawn at each end to arrive at."""
        for link in self.zp.links:
            a, b = self.by_id[link.a], self.by_id[link.b]
            if link.kind == "edge":
                a_spawn, b_spawn = f"{link.a}-east", f"{link.b}-west"
                a.room.spawns.append(zf.Spawn(a_spawn, a.W - 160, link.ya or 730, "left"))
                b.room.spawns.append(zf.Spawn(b_spawn, 160, link.yb or 730, "right"))
                a.room.exits.append(zf.Exit(link.id_a or f"{link.a}-east", "east", zf.Target("room", link.b, b_spawn),
                                            None, link.label_a, None, False, zf.Presence(link.when, None)))
                b.room.exits.append(zf.Exit(link.id_b or f"{link.b}-west", "west", zf.Target("room", link.a, a_spawn),
                                            None, link.label_b, None, False, zf.Presence()))
                if link.locked_why:
                    a.room.exits.append(zf.Exit(link.twin_id or f"{link.a}-east-shut", "east", zf.Target("locked"), None,
                                                link.locked_label or link.label_a, link.locked_why, False,
                                                zf.Presence(None, link.when)))
            elif link.kind == "stairs":
                a_spawn, b_spawn = f"{link.a}-up", f"{link.b}-down"
                a.room.spawns.append(zf.Spawn(a_spawn, link.xa, link.ya or 700, a.toward_centre(link.xa)))
                b.room.spawns.append(zf.Spawn(b_spawn, link.xb, link.yb or 700, b.toward_centre(link.xb)))
                a.room.exits.append(zf.Exit(link.id_a or f"{link.a}-up", "up", zf.Target("room", link.b, b_spawn),
                                            link.xa, link.label_a, None, False, zf.Presence()))
                b.room.exits.append(zf.Exit(link.id_b or f"{link.b}-down", "down", zf.Target("room", link.a, a_spawn),
                                            link.xb, link.label_b, None, False, zf.Presence()))
            else:  # a door each way
                a_spawn, b_spawn = link.spawn_a or f"{link.a}-{link.id_a}", link.spawn_b or f"{link.b}-{link.id_b}"
                a.room.spawns.append(zf.Spawn(a_spawn, link.xa, a.back_edge(link.xa) + 70, a.toward_centre(link.xa)))
                b.room.spawns.append(zf.Spawn(b_spawn, link.xb, b.back_edge(link.xb) + 70, b.toward_centre(link.xb)))
                existing = next((d for d in a.room.doors if d.id == link.id_a), None)
                if existing is not None:
                    existing.target = zf.Target("room", link.b, b_spawn)
                    existing.x = link.xa
                else:
                    a.room.doors.append(zf.Door(link.id_a, link.xa, zf.Target("room", link.b, b_spawn), link.badge_a,
                                                link.name_a, None, None, zf.Presence(link.when, None)))
                b.room.doors.append(zf.Door(link.id_b, link.xb, zf.Target("room", link.a, a_spawn), link.badge_b,
                                            link.name_b, None, None, zf.Presence()))
                if link.twin_id and link.locked_why:
                    a.room.doors.append(zf.Door(link.twin_id, link.xa, zf.Target("locked"), link.badge_a, link.name_a,
                                                link.locked_why, None, zf.Presence(None, link.when)))

    # -- tidying --------------------------------------------------------------------------
    def spread_npcs(self, r: RoomMap) -> None:
        """Pushes npcs apart along x, NPC_SPACING or more where the room allows, keeping their order."""
        fixed = set(self.zp.place) | set(self.zp.fixed)
        movable = [n for n in r.room.npcs if not n.aboard] + list(r.room.camp)
        if len(movable) < 2:
            return
        movable.sort(key=lambda n: (n.x, n.y, n.id))
        lo, hi = MARGIN + 16, r.W - MARGIN - 16
        xs = [min(max(n.x, lo), hi) for n in movable]
        for _ in range(60):
            moved = False
            for i in range(1, len(xs)):
                gap = xs[i] - xs[i - 1]
                if gap < NPC_SPACING and not _apart(movable[i - 1], movable[i]):
                    push = (NPC_SPACING - gap) / 2
                    a_fixed = movable[i - 1].id in fixed
                    b_fixed = movable[i].id in fixed
                    if a_fixed and b_fixed:
                        continue
                    if a_fixed:
                        xs[i] += 2 * push
                    elif b_fixed:
                        xs[i - 1] -= 2 * push
                    else:
                        xs[i - 1] -= push
                        xs[i] += push
                    moved = True
            for i in range(len(xs)):
                xs[i] = min(max(xs[i], lo), hi)
            if not moved:
                break
        for n, x in zip(movable, xs):
            n.x = _r(x)

    def spread_doors(self, r: RoomMap) -> None:
        """Pushes doors apart so their badges don't overlap. Twins (same x) move together."""
        groups: Dict[float, List[zf.Door]] = {}
        for d in r.room.doors:
            groups.setdefault(d.x, []).append(d)
        keys = sorted(groups)
        if len(keys) < 2:
            return
        fixed = {d.id for d in r.room.doors if d.id in self.zp.fixed}
        xs = list(keys)

        def width(i):
            return max(max(d.box_size()[0] for d in groups[keys[i]]), 200)

        for _ in range(80):
            moved = False
            for i in range(1, len(xs)):
                need = (width(i - 1) + width(i)) / 2 + 80
                gap = xs[i] - xs[i - 1]
                if gap < need:
                    push = (need - gap) / 2
                    af = any(d.id in fixed for d in groups[keys[i - 1]])
                    bf = any(d.id in fixed for d in groups[keys[i]])
                    if af and bf:
                        continue
                    if af:
                        xs[i] += 2 * push
                    elif bf:
                        xs[i - 1] -= 2 * push
                    else:
                        xs[i - 1] -= push
                        xs[i] += push
                    moved = True
            for i in range(len(xs)):
                w = width(i)
                xs[i] = min(max(xs[i], w / 2 + 120), r.W - w / 2 - 120)
            if not moved:
                break
        for k, x in zip(keys, xs):
            for d in groups[k]:
                d.x = _r(x)

    def keep_clear_of_edges(self, r: RoomMap) -> None:
        """Nothing stands in an edge exit's 48 px: spawns, npcs, things and camp seats come in to 120 px."""
        sides = {e.side for e in r.room.exits}
        lo = 120 if "west" in sides else MARGIN
        hi = r.W - (120 if "east" in sides else MARGIN)
        for group in (r.room.spawns, r.room.npcs, r.room.things, r.room.camp):
            for o in group:
                if getattr(o, "aboard", None):
                    continue
                o.x = min(max(o.x, lo), hi)
        for t in r.room.triggers:
            if "west" in sides and t.x0 < zf.EDGE_EXIT + 16:
                t.x0 = zf.EDGE_EXIT + 16
            if "east" in sides and t.x1 > r.W - zf.EDGE_EXIT - 16:
                t.x1 = r.W - zf.EDGE_EXIT - 16
            if t.x1 - t.x0 < 64:
                t.x0 = max(0, t.x1 - 64)

    def tie_buildings(self) -> Dict[str, Tuple[MapObj, float]]:
        """Each door's building: the building prop whose front the old entrance (or door-exit) is in,
        and how far right of the building's foot the door is, in old tiles."""
        ties: Dict[str, Tuple[MapObj, float]] = {}
        buildings = [o for o in self.m.objects if o.kind == "prop" and P.base_prop(o.id) in P.BUILDINGS
                     and o.id not in P.FAR_PROPS - {"palace"}]
        for o in self.m.objects:
            if o.kind == "entrance":
                ex, ey = o.feet
            elif o.kind == "exit" and self.zp.exits.get(o.id) and self.zp.exits[o.id].side == "door":
                x0, y0, x1, y1 = o.area()
                ex, ey = (x0 + x1) / 2, (y0 + y1) / 2
            else:
                continue
            best = None
            for b in buildings:
                info = self.props[b.id]
                bx, by = b.feet
                dx, dy = ex - bx, ey - by
                if abs(dx) * 16 <= info.footprint[0] / 2 + 8 and -2.0 <= dy <= 1.6:
                    if best is None or abs(dx) + abs(dy) < best[1]:
                        best = (b, abs(dx) + abs(dy), dx)
            if best is not None:
                ties[o.id] = (best[0], best[2])
        return ties

    def place_buildings(self, r: RoomMap) -> None:
        """Puts each door's building behind its door (the mid layer's at 3x, so 48 px an old tile), then
        moves any other mid scenery out of the doorways."""
        for d in r.room.doors:
            tie = self.ties.get(d.id)
            if tie is None:
                continue
            b, dx = tie
            r.room.scenery.append(zf.Scenery("mid", b.id, _r(d.x - dx * 48)))
            self.note("prop", b.id, f"{r.rp.id} (scenery mid, behind door {d.id})")
        holes = sorted({(d.x - d.box_size()[0] / 2 - 8, d.x + d.box_size()[0] / 2 + 8) for d in r.room.doors})
        tied_names = {(self.ties[d.id][0].id) for d in r.room.doors if d.id in self.ties}
        for s in r.room.scenery:
            if s.layer != "mid" or (s.prop in tied_names and any(abs(s.x - (d.x - self.ties[d.id][1] * 48)) < 2
                                                                   for d in r.room.doors if d.id in self.ties)):
                continue
            info = self.props.get(s.prop)
            half = (info.footprint[0] if info and info.footprint[0] else 16) * 1.5
            if P.base_prop(s.prop) not in P.BUILDINGS:
                half = min(half, 40)

            def clash(x):
                return any(x + half > a and x - half < b for a, b in holes)

            if not clash(s.x):
                continue
            for step in range(1, 200):
                for x in (s.x + step * 16, s.x - step * 16):
                    if half <= x <= r.W - half and not clash(x):
                        s.x = _r(x)
                        break
                else:
                    continue
                break

    def obstacles(self, r: RoomMap) -> List[Tuple[float, float, float, float]]:
        """Every prop line's footprint (and every thing's), in canvas px: left, top, right, bottom."""
        boxes = []
        for p in r.room.props:
            boxes.extend(footprints(self.props.get(p.prop), p.x, p.y))
        for t in r.room.things:
            boxes.extend(footprints(self.props.get(t.look or t.kind), t.x, t.y))
        return boxes

    def settle(self, r: RoomMap) -> None:
        """Moves anyone (and any spawn, seat or thing) standing in a footprint, a block or off the floor to the nearest free spot."""
        props_only = []
        for p in r.room.props:
            props_only.extend(footprints(self.props.get(p.prop), p.x, p.y))
        people = [n for n in r.room.npcs if not n.aboard] + list(r.room.spawns) + list(r.room.camp)
        for group in (list(r.room.things), people):
            boxes = props_only if group and isinstance(group[0], zf.Thing) else self.obstacles(r)
            for o in group:
                own = footprints(self.props.get(o.look or o.kind), o.x, o.y) if isinstance(o, zf.Thing) else []
                others = [b for b in boxes if b not in own]
                if free(r.room, others, o.x, o.y):
                    continue
                spot = nearest_free(r.room, others, o.x, o.y)
                if spot is None:
                    who = getattr(o, 'id', None) or getattr(o, 'name', '?')
                    raise SystemExit(f"{self.zp.id}/{r.rp.id}: nowhere free near {o.x} {o.y} for {who}")
                o.x, o.y = spot

    # -- the tile strip -------------------------------------------------------------------
    def build_strip(self, r: RoomMap) -> None:
        m = self.m
        ts = self.tilesets[m.settings["tileset"]]
        legend = m.legend
        rev = {name: c for c, name in legend.items()}
        rp = r.rp
        interior = rp.kind == "interior"
        top = 0 if interior else 576
        nrows = zf.strip_rows(top)
        cols = r.W // zf.TILE

        def solid(c: str) -> bool:
            info = ts.tiles.get(legend.get(c, ""))
            return info is None or info.solid

        def block(c: str) -> int:
            info = ts.tiles.get(legend.get(c, ""))
            return info.block if info else 1

        # Which old column (and, rotated, which old row) each new column reads.
        def old_u(c: int) -> int:
            cx = c * zf.TILE + zf.TILE / 2
            t = min(max((cx - MARGIN) / (r.W - 2 * MARGIN), 0.0), 1.0)
            if rp.flip:
                t = 1 - t
            lo, hi = (r.sy0, r.sy1) if rp.rotate else (r.sx0, r.sx1)
            return int(min(max(math.floor(lo + t * (hi - lo)), lo), hi - 1))

        def cell(oy: int, ox: int, layer=None) -> str:
            return m.tile(layer if layer is not None else m.ground, ox, oy)

        # The stretch: the run of old rows (rotated: columns) that's most walkable across the room.
        lo_v, hi_v = (r.sx0, r.sx1) if rp.rotate else (r.sy0, r.sy1)
        us = [old_u(c) for c in range(cols)]
        if rp.stretch:
            ry0, ry1 = rp.stretch
        else:
            def frac(v):
                cells = [cell(u, v) if rp.rotate else cell(v, u) for u in sorted(set(us))]
                return sum(not solid(c) for c in cells) / len(cells)
            vs = list(range(int(lo_v), int(math.ceil(hi_v))))
            vs = [v for v in vs if 0 <= v < (m.width if rp.rotate else m.height)]
            good = [v for v in vs if frac(v) >= 0.5]
            best = None
            run: List[int] = []
            for v in vs + [None]:
                if v is not None and v in good and (not run or v == run[-1] + 1):
                    run.append(v)
                    continue
                if run and (best is None or len(run) > len(best)):
                    best = run
                run = [v] if v is not None and v in good else []
            if best is None:
                best = [max(vs, key=frac)]
            ry0, ry1 = best[0], best[-1] + 1

        def at(v: int, u: int, layer=None) -> str:
            return cell(u, v, layer) if rp.rotate else cell(v, u, layer)

        band_rows = [k for k in range(nrows) if top + k * zf.TILE < FLOOR_BOTTOM and top + (k + 1) * zf.TILE > FLOOR_TOP]
        above = [k for k in range(nrows) if top + (k + 1) * zf.TILE <= FLOOR_TOP]
        below = [k for k in range(nrows) if top + k * zf.TILE >= FLOOR_BOTTOM]
        common = {}
        for v in range(ry0, ry1):
            for u in sorted(set(us)):
                c = at(v, u)
                if not solid(c):
                    common[c] = common.get(c, 0) + 1
        fallback = max(sorted(common), key=lambda c: common[c]) if common else next(c for c in legend if not solid(c))

        rows: List[str] = [""] * nrows
        detail: List[str] = ["." * cols] * nrows
        nb = len(band_rows)
        for i, k in enumerate(band_rows):
            v = ry0 + int(math.floor((i + 0.5) / nb * (ry1 - ry0)))
            row, drow = [], []
            for c in range(cols):
                u = us[c]
                t = at(v, u)
                if solid(t) or block(t) > 1:
                    t = next((at(v2, u) for v2 in sorted(range(ry0, ry1), key=lambda v2: (abs(v2 - v), v2))
                              if not solid(at(v2, u)) and block(at(v2, u)) == 1), fallback)
                row.append(t)
                d = at(v, u, m.detail) if m.detail else "."
                if d != "." and (solid(d) or block(d) > 1 or legend.get(d) in P.NO_DETAIL):
                    d = "."
                drow.append(d)
            rows[k] = "".join(row)
            detail[k] = "".join(drow)
        for j, k in enumerate(below):
            v = min(ry1 + j, (m.width if rp.rotate else m.height) - 1)
            row = []
            for c in range(cols):
                t = at(v, us[c])
                if block(t) > 1:
                    t = rows[band_rows[-1]][c]
                row.append(t)
            rows[k] = "".join(row)
        if above:
            wall = self.wall_rows(r, len(above), cols, rev)
            for j, k in enumerate(above):
                rows[k] = wall[j]
        if rp.tile_rows:
            for k, pattern in rp.tile_rows.items():
                rows[k] = (pattern * (cols // len(pattern) + 1))[:cols]
        used = []
        for row in rows + detail:
            for ch in row:
                if ch != "." and ch not in used:
                    used.append(ch)
        # A character the zone format keeps for itself (a quote, a semicolon) gets a free one instead.
        swap: Dict[str, str] = {}
        for ch in used:
            if ch in zf.BAD_TILE_CHARS:
                swap[ch] = next(c for c in "'`^qQjJ" if c not in legend and c not in swap.values())
        rows = ["".join(swap.get(ch, ch) for ch in row) for row in rows]
        detail = ["".join(swap.get(ch, ch) for ch in row) for row in detail]
        r.room.tiles = zf.Tiles(m.settings["tileset"], top, {swap.get(c, c): legend[c] for c in legend if c in used},
                                rows, detail if any(set(d) != {"."} for d in detail) else [])

    def wall_rows(self, r: RoomMap, n: int, cols: int, rev: Dict[str, str]) -> List[str]:
        """An interior's back wall: the old north wall's rows, stretched to n rows and resampled across."""
        m, rp = self.m, r.rp
        first = rp.wall_rows or (0, int(rp.src[1]))
        old = list(range(first[0], first[1]))
        x0, x1 = (rp.wall_x or (rp.src[0], rp.src[2]))
        out = []
        for j in range(n):
            if j == 0:
                oy = old[0]
            elif j == n - 1:
                oy = old[-1]
            else:
                mids = old[1:-1] or old
                oy = mids[min(int((j - 1) / max(1, n - 2) * len(mids)), len(mids) - 1)]
            row = []
            for c in range(cols):
                cx = c * zf.TILE + zf.TILE / 2
                t = min(max((cx - MARGIN) / (r.W - 2 * MARGIN), 0.0), 1.0)
                ox = int(min(max(math.floor(x0 + t * (x1 - x0)), x0), x1 - 1))
                row.append(m.tile(m.ground, ox, oy))
            out.append("".join(row))
        return out

    def add_life(self, r: RoomMap) -> None:
        for kind, box in (r.rp.life or P.default_life(self.zp, r.rp)):
            x0, y0, x1, y1 = box
            r.room.life.append(zf.Life(kind, x0, y0, x1, y1))


FEET_HALF_W, FEET_D = 20, 16
"""The walker's feet box at 4x: 10 by 4 art px."""


def footprints(info, x: float, y: float) -> List[Tuple[float, float, float, float]]:
    """A prop's footprint at 4x, standing at x, y: one box, or two either side of an arch's gap."""
    if info is None or info.footprint == (0, 0):
        return []
    w, h = info.footprint[0] * 4, info.footprint[1] * 4
    left, top = x - w / 2, y - h
    if not info.gap:
        return [(left, top, left + w, y)]
    side = (w - info.gap * 4) / 2
    return [(left, top, left + side, y), (x + info.gap * 2, top, x + info.gap * 2 + side, y)]


def free(room: zf.Room, boxes, x: float, y: float) -> bool:
    """Whether feet at x, y fit: on the floor, out of the blocks, and clear of every box."""
    for px in (x - FEET_HALF_W, x + FEET_HALF_W):
        if not zf.point_in_polygon(room.floor, px, y) or not zf.point_in_polygon(room.floor, px, y - FEET_D):
            return False
    for b in room.blocks:
        if ((x - b.cx) / (b.rx + FEET_HALF_W)) ** 2 + ((y - b.cy) / (b.ry + FEET_D / 2)) ** 2 < 1:
            return False
    for (l, t, rr, bb) in boxes:
        if x + FEET_HALF_W > l and x - FEET_HALF_W < rr and y > t and y - FEET_D < bb:
            return False
    return True


def nearest_free(room: zf.Room, boxes, x: float, y: float) -> Optional[Tuple[int, int]]:
    """The nearest spot to x, y where feet fit: up and down the band first, then further along it."""
    for dx in [0] + [s * k * 24 for k in range(1, 30) for s in (1, -1)]:
        cx = x + dx
        if cx < MARGIN or cx > room.width - MARGIN:
            continue
        top = zf.back_edge_at(room.floor, cx) + FEET_D + 2
        bottom = zf.front_edge_at(room.floor, cx) - 2
        ys = sorted(range(int(top), int(bottom) + 1, 4), key=lambda v: (abs(v - y), v))
        for cy in ys:
            if free(room, boxes, cx, cy):
                return _r(cx), _r(cy)
    return None


def _apart(a, b) -> bool:
    """Two npcs never on the map together (one when a flag, the other unless it), so they can share a spot."""
    pa, pb = getattr(a, "presence", zf.Presence()), getattr(b, "presence", zf.Presence())
    return bool((pa.when and pa.when == pb.unless) or (pa.unless and pa.unless == pb.when))


def convert(zone_id: str) -> Tuple[zf.Zone, List[Tuple[str, str, str]]]:
    zp = P.ZONES[zone_id]
    c = Converter(zp)
    c.pending_twins = []
    zone = c.run()
    # Round-trip through the parser, so what's written is exactly what the game will read.
    text = zf.write_zone(zone)
    zf.parse_zone(text, f"Zones/{zone_id}.txt", zone_id)
    return zone, c.report


def main() -> int:
    ap = argparse.ArgumentParser(description=__doc__.split("\n\n")[0])
    ap.add_argument("zones", nargs="*")
    ap.add_argument("--check", action="store_true", help="diff against the committed zone files; exit 1 if any differ")
    ap.add_argument("--report", action="store_true", help="list every old id and the room it landed in")
    args = ap.parse_args()
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
    ids = args.zones or list(P.ZONES)
    bad = 0
    for zid in ids:
        try:
            zone, report = convert(zid)
        except zf.ZoneError as err:
            print(f"{zid}: the converter wrote a file the parser refuses: {err}")
            return 1
        text = zf.write_zone(zone)
        path = os.path.join(ZONES, zid + ".txt")
        if args.check:
            old = open(path, encoding="utf-8", newline="").read() if os.path.exists(path) else ""
            if old != text:
                bad += 1
                diff = difflib.unified_diff(old.splitlines(), text.splitlines(), f"committed/{zid}.txt", f"generated/{zid}.txt", lineterm="", n=1)
                print("\n".join(list(diff)[:60]))
        elif not args.report:
            os.makedirs(ZONES, exist_ok=True)
            with open(path, "w", encoding="utf-8", newline="") as fh:
                fh.write(text)
        if args.report:
            print(f"== {zid}")
            for kind, ident, where in report:
                print(f"  {kind:9} {ident:32} {where}")
        if not args.check and not args.report:
            print(f"{zid}: {len(zone.rooms)} rooms ({', '.join(r.id for r in zone.rooms)})")
    if args.check:
        print(f"{len(ids) - bad} of {len(ids)} zones match" + ("" if not bad else f"; {bad} differ"))
    return 1 if bad else 0


if __name__ == "__main__":
    sys.exit(main())
