"""The hand-made half of the conversion: how each of the 19 maps becomes a zone of side-view rooms.

convert_maps.py reads this and the old maps (*.map) and writes Content/Zones/*.txt. Every fix to a
zone goes here, never into a generated file (m3-plan §2). Coordinates are canvas px (a room is
width x 900; the floor band runs about 600 to 860), and source rectangles are the old map's tiles,
x0 y0 x1 y1 with x1 and y1 exclusive.

For each zone:
- rooms, left to right: the source rectangle each maps x and depth from (`src`), its width, floor,
  blocks and fight range, and the old tiles it covers for migrating saves (`legacy`, default src);
- `exits`, `entrances` and `idoors`: what each old exit, entrance and hand-opened door becomes
  (an edge exit, a door with a badge, or a link between two rooms), and its locked twin;
- `twins`: the old `*-shut` triggers and `blocked`, now the locked twins of a door or exit;
- `links`: the ways between the zone's own rooms (an exit each way and a spawn at each end);
- `assign`, `place`, `span`, `facing`, `fixed`: an old id put in a room other than the one its
  position falls in, put at a canvas x (and y), a trigger's x range, a facing, and npcs and doors the
  spreading leaves where they are;
- `badges`: npc badges (shop, anvil, bottle...);
- `prop_layer`, `prop_place`, `prop_room`: where a prop goes (far, mid, fore, prop, back, drop),
  by prop id or by `id@x,y` for one of several;
- per room, extra `spawns`, `doors`, `exits`, `props`, `triggers`, `scenery` and `life`.
"""

from __future__ import annotations

from dataclasses import dataclass, field
from typing import Dict, List, Optional, Tuple

import zonefile as zf

W16, W24, W32, W48 = 1600, 2432, 3200, 4800


# ---------------------------------------------------------------------------------------------
# The schema

@dataclass
class Twin:
    """A locked twin: the same door or exit, locked, there unless the flag that opens the real one is set."""
    id: str
    of: str
    why: Optional[str] = None
    unless: Optional[str] = None
    label: Optional[str] = None


@dataclass
class ExitSpec:
    """What an old exit becomes: side west, east, up or down (an exit), or door (a door with a badge)."""
    side: str
    room: Optional[str] = None
    x: Optional[int] = None
    badge: str = "door"
    name: Optional[str] = None
    label: Optional[str] = None
    target: Optional[zf.Target] = None
    travel: bool = False
    size: Optional[Tuple[int, int]] = None
    twin: Optional[Twin] = None


@dataclass
class Entrance:
    badge: str = "door"
    name: Optional[str] = None
    size: Optional[Tuple[int, int]] = None


@dataclass
class IDoor:
    """What an old hand-opened door becomes. side door, west, east, or link (a Link makes it)."""
    side: str = "door"
    room: Optional[str] = None
    x: Optional[int] = None
    badge: str = "door"
    name: Optional[str] = None
    label: Optional[str] = None
    target: Optional[zf.Target] = None
    why: Optional[str] = None
    size: Optional[Tuple[int, int]] = None
    twin: Optional[Twin] = None


@dataclass
class Link:
    """A way between two rooms of the zone. edge: a's east edge to b's west edge. stairs: a's up to b's down.
    door: a door in each (id_a in a, id_b in b), arriving at spawn_b and spawn_a."""
    a: str
    b: str
    kind: str = "edge"
    xa: Optional[int] = None
    xb: Optional[int] = None
    ya: Optional[int] = None
    yb: Optional[int] = None
    id_a: Optional[str] = None
    id_b: Optional[str] = None
    label_a: Optional[str] = None
    label_b: Optional[str] = None
    when: Optional[str] = None
    locked_why: Optional[str] = None
    locked_label: Optional[str] = None
    twin_id: Optional[str] = None
    badge_a: str = "door"
    badge_b: str = "door"
    name_a: Optional[str] = None
    name_b: Optional[str] = None
    spawn_a: Optional[str] = None
    spawn_b: Optional[str] = None


@dataclass
class RoomPlan:
    id: str
    name: str
    width: int
    src: Tuple[float, float, float, float]
    kind: str = "street"
    legacy: Optional[List[Tuple[int, int, int, int]]] = None
    floor: Optional[List[Tuple[int, int]]] = None
    blocks: List[Tuple[int, int, int, int]] = field(default_factory=list)
    fight: Optional[Tuple[int, int]] = None
    rotate: bool = False
    flip: bool = False
    stretch: Optional[Tuple[int, int]] = None
    dark: Optional[bool] = None
    ambient: Optional[float] = None
    tint: Optional[Tuple[int, int, int]] = None
    states: Dict[str, str] = field(default_factory=dict)
    priority: int = 0
    spawns: List[tuple] = field(default_factory=list)
    doors: List[zf.Door] = field(default_factory=list)
    exits: List[zf.Exit] = field(default_factory=list)
    props: List[zf.Prop] = field(default_factory=list)
    triggers: List[zf.Trigger] = field(default_factory=list)
    scenery: List[tuple] = field(default_factory=list)
    life: Optional[List[tuple]] = None
    prop_layer: Dict[str, str] = field(default_factory=dict)
    comments: List[str] = field(default_factory=list)
    tile_rows: Dict[int, str] = field(default_factory=dict)
    wall_rows: Optional[Tuple[int, int]] = None
    wall_x: Optional[Tuple[int, int]] = None
    wall_sections: List[Tuple[float, float, int, int]] = field(default_factory=list)


@dataclass
class ZonePlan:
    id: str
    rooms: List[RoomPlan]
    map: Optional[str] = None
    name: Optional[str] = None
    music: Optional[str] = None
    region: Optional[str] = None
    danger: int = 0
    life: str = "town"
    comments: List[str] = field(default_factory=list)
    exits: Dict[str, ExitSpec] = field(default_factory=dict)
    entrances: Dict[str, Entrance] = field(default_factory=dict)
    idoors: Dict[str, IDoor] = field(default_factory=dict)
    twins: Dict[str, Twin] = field(default_factory=dict)
    links: List[Link] = field(default_factory=list)
    assign: Dict[str, str] = field(default_factory=dict)
    place: Dict[str, tuple] = field(default_factory=dict)
    span: Dict[str, Tuple[int, int]] = field(default_factory=dict)
    facing: Dict[str, str] = field(default_factory=dict)
    fixed: List[str] = field(default_factory=list)
    drop: Dict[str, str] = field(default_factory=dict)
    badges: Dict[str, str] = field(default_factory=dict)
    patrol: Dict[str, Tuple[int, int]] = field(default_factory=dict)
    prop_layer: Dict[str, str] = field(default_factory=dict)
    prop_place: Dict[str, tuple] = field(default_factory=dict)
    prop_room: Dict[str, Optional[str]] = field(default_factory=dict)
    # Where a script's spawn goes if the .map doesn't have it (M2 adds them to the maps): name -> (room, x, y, facing).
    spawn_fallbacks: Dict[str, Tuple[str, int, int, str]] = field(default_factory=dict)


# ---------------------------------------------------------------------------------------------
# What props are

ROOFS = ("blue", "pink", "teal", "plum")
REROOFED = {"house-ivy", "house-awning", "house-rose", "house-shop", "townhouse", "cottage", "turret-house",
            "bakery", "chapel", "forge", "warehouse", "gilded-flagon", "rusty-anvil", "fishmonger"}


def base_prop(pid: str) -> str:
    """A prop without its roof colour."""
    for c in ROOFS:
        if pid.endswith("-" + c) and pid[: -len(c) - 1] in REROOFED:
            return pid[: -len(c) - 1]
    return pid


BUILDINGS = REROOFED | {
    "palace", "temple", "guildhall", "inn", "watch-post", "farmhouse", "windmill", "east-gate", "city-wall",
    "city-gate", "quarry-mouth", "keep-arch", "stall-blue", "stall-pink", "stall-teal", "stall-cloth",
    "tent-ragged", "tent-ragged-2", "garden-wall", "well-roofed", "stone-bridge", "ranger-bridge",
    "great-oak", "healer-station",
}
FAR_PROPS = {"palace", "temple", "guildhall"}
STORY_PROPS = {"ship", "gangplank", "black-crystal", "black-crystal-shattered", "black-crystal-purified", "root-wall",
               "sapper-hole", "fallen-tree", "vault-stair", "vault-stair-shut", "sun-throne", "sunheart-pedestal",
               "courier-body", "satchel", "fountain", "campfire", "cookfire", "relic-shelf", "war-table"}
FORE_OK = {"lamp", "lantern-post", "lamp-arm", "lamp-double", "bollard", "bush", "bush-small", "bush-deep",
           "bush-flowers", "bush-pink", "fern", "grass-tall", "cypress-pot", "planter", "planter-pink",
           "rocks", "rocks-mossy", "fence-wood", "fence-wood-post", "crate", "cargo-crate", "barrel",
           "cargo-barrel", "flower-box", "barrel-flowers", "oak", "oak-light", "oak-deep", "fir", "fir-deep",
           "tree-blackvein", "tree-blackvein-pale", "tree-blackvein-bare", "oak-turning", "blossom-tree",
           "cherry-tree-small", "gloom-mushroom", "candle-stand", "potted-plant", "rope-posts", "topiary"}
NO_DETAIL = {"cobweb", "cobweb-ne"}

ZONE_NAMES = {
    "gulls-wager": "The Gull's Wager", "docks": "Saltwind Docks", "questeria": "Questeria", "road": "The Old Road",
    "barrow": "The Barrow", "mirewood": "The Mirewood", "mirewood-quarry": "The Old Quarry",
    "thornbridge": "Thornbridge", "old-crown-vault": "The Old Crown Vault", "sunspire-keep": "Sunspire Keep",
    "rusty-anvil": "The Rusty Anvil", "gilded-flagon": "The Gilded Flagon", "bakery": "The Bakery",
    "chapel": "The Chapel", "cottage-a": "A Cottage", "cottage-b": "A Cottage", "forge": "The Smithy",
    "shop": "The Chandler's", "warehouse": "A Warehouse",
}


def default_life(zp: ZonePlan, rp: RoomPlan) -> List[tuple]:
    """Three or four moving things for every room (12-rooms §3.2), by the kind of place."""
    w = rp.width
    sky = (0, 40, w, 260)
    roofs = (w // 8, 160, w - w // 8, 420)
    air = (0, 300, w, 860)
    front = (0, 820, w, 900)
    kind = zp.life
    if rp.kind == "interior":
        return [("candle", (0, 200, w, 640)), ("motes", (0, 120, w, 760)), ("smoke", (w // 4, 0, 3 * w // 4, 300))]
    if kind == "sea":
        return [("clouds", sky), ("shimmer", front), ("flag", (w // 3, 40, 2 * w // 3, 240)), ("candle", (0, 380, w, 640))]
    if kind == "coast":
        return [("gulls", sky), ("shimmer", front), ("flag", roofs), ("smoke", roofs)]
    if kind == "town":
        return [("clouds", sky), ("flag", roofs), ("smoke", roofs), ("leaves", air)]
    if kind == "meadow":
        return [("clouds", sky), ("leaves", air), ("motes", air), ("smoke", roofs)]
    if kind == "wood":
        return [("leaves", air), ("motes", air), ("clouds", sky)]
    if kind == "camp":
        return [("smoke", (w // 3, 200, 2 * w // 3, 700)), ("motes", air), ("leaves", air), ("candle", (w // 3, 560, 2 * w // 3, 800))]
    if kind == "dungeon":
        return [("motes", air), ("candle", (0, 300, w, 640)), ("ash", (0, 0, w, 860))]
    return [("clouds", sky), ("motes", air), ("leaves", air)]


def interior(zone_id: str, width: int, src, comments=None, **kw) -> ZonePlan:
    """One of the little insides: a single room, the way out an edge exit back where you came in."""
    exits = kw.pop("exits", {"door": ExitSpec("west")})
    rp = RoomPlan(kw.pop("room_id", "inside"), kw.pop("room_name", None) or ZONE_NAMES[zone_id], width, src, kind="interior",
                  **{k: kw.pop(k) for k in list(kw) if k in RoomPlan.__dataclass_fields__})
    return ZonePlan(zone_id, [rp], exits=exits, region=kw.pop("region", "questeria"), life="interior",
                    comments=comments or [], **kw)


# ---------------------------------------------------------------------------------------------
# The zones

ZONES: Dict[str, ZonePlan] = {}


def zone(zp: ZonePlan) -> None:
    ZONES[zp.id] = zp


# The Gull's Wager: one long deck at night, the bow to the west.
zone(ZonePlan(
    "gulls-wager", region="saltwind", life="sea",
    comments=["The Gull's Wager at night, two days out: one deck, the bow at the left. Generated by",
              "tools/rooms/convert_maps.py from the old gulls-wager.map and tools/rooms/plan.py; don't edit it."],
    rooms=[RoomPlan("deck", "The Main Deck", W32, (9, 9, 36, 17), fight=(200, 2400))],
    place={"bow": (420, 690), "start": (1150, 700), "voyage": (470, None)},
    span={"voyage": (240, 700)},
    facing={"bow": "left"},
    spawn_fallbacks={"bow": ("deck", 420, 690, "left")},
))

# Saltwind Docks: the west boardwalk and its landing stage, the harbourside where the ship ties up,
# and the fish market with the road up to the city at its east end.
zone(ZonePlan(
    "docks", region="saltwind", life="coast",
    comments=["Saltwind Docks: the pier, the harbourside and the fish market, left to right. Generated by",
              "tools/rooms/convert_maps.py from the old docks.map and tools/rooms/plan.py; don't edit it."],
    rooms=[
        RoomPlan("pier", "The Pier", W16, (0, 12.5, 18, 18.5), legacy=[(0, 0, 18, 36)]),
        RoomPlan("harbourside", "The Harbourside", W32, (18, 12.5, 44, 18.5), legacy=[(18, 0, 44, 36)],
                 fight=(400, 2200)),
        RoomPlan("fish-market", "The Fish Market", W32, (44, 12.5, 76, 18.5), legacy=[(44, 0, 76, 36)]),
    ],
    exits={"city": ExitSpec("east", label="Questeria")},
    entrances={"anvil": Entrance("inn"), "chandler": Entrance("shop")},
    twins={"anvil-shut": Twin("anvil-shut", "anvil")},
    drop={"red-gulls-behind": "merged into red-gulls (a trigger spans the band's whole depth now)"},
    links=[Link("pier", "harbourside"), Link("harbourside", "fish-market")],
    badges={},
    prop_layer={"ship": "back", "gangplank": "back", "rowboat": "drop"},
    prop_place={"gangplank@27.5,27.5": (None, 664)},
    place={"start": (560, 780), "city": (2960, 700)},
))

# Questeria: the gate plaza where the harbour road comes in, the festival square with the fountain,
# the Sunspire's steps above it, the Lantern Market, and the road out to Thornbridge.
Q_BAND = {"bench", "bench-iron", "bench-plain", "statue", "crystal-pillar", "lamp", "planter", "flower-cart",
          "table", "water-barrel", "cask", "crate-stack", "sacks", "cargo-barrel", "crate-open", "crate-apples",
          "chalkboard", "barrel-water", "handcart", "water-pump", "washtub", "signpost-arrows", "flowers-pink",
          "flowers-white", "flowers-mixed", "sewer-grate", "gilded-flagon-sign", "lantern-small", "cypress-pot"}
zone(ZonePlan(
    "questeria", region="questeria", life="town",
    comments=["Questeria: the gate plaza, the festival square (the Sunspire's steps above it), the Lantern",
              "Market and the Thornbridge gate. Generated by tools/rooms/convert_maps.py from",
              "the old questeria.map and tools/rooms/plan.py; don't edit it."],
    rooms=[
        RoomPlan("gate-plaza", "The Gate Plaza", W32, (0, 20, 28, 38), legacy=[(0, 0, 10, 58), (10, 21, 28, 58)],
                 priority=1, prop_layer={p: "mid" for p in ("lamp", "bench-iron", "planter-long", "water-barrel", "cask")}),
        RoomPlan("festival-square", "The Festival Square", W32, (28, 21, 56, 40), legacy=[(28, 21, 56, 58)], priority=1,
                 blocks=[(1600, 690, 250, 50)],
                 scenery=[("far", "palace", 1600), ("far", "temple", 700), ("far", "guildhall", 2500)]),
        RoomPlan("sunspire-steps", "The Sunspire Steps", W16, (10, 14, 74, 21), legacy=[(10, 0, 74, 21)]),
        RoomPlan("lantern-market", "The Lantern Market", W32, (56, 24, 96, 40), legacy=[(74, 0, 96, 21), (56, 21, 96, 58)], priority=1),
        RoomPlan("thornbridge-gate", "The Thornbridge Gate", W16, (96, 26, 108, 40), legacy=[(96, 0, 108, 58)], priority=1),
    ],
    exits={
        "road": ExitSpec("door", room="festival-square", x=2300, badge="gate", name="The City Gate"),
        "docks": ExitSpec("west", room="gate-plaza", label="Saltwind Docks"),
        "thornbridge": ExitSpec("east", room="thornbridge-gate", label="Thornbridge"),
    },
    entrances={
        "keep": Entrance("crown"), "vault": Entrance("stair-down"), "chapel": Entrance("sun"),
        "shop-south": Entrance("shop"), "flagon": Entrance("inn"), "forge": Entrance("anvil"),
    },
    twins={"keep-shut": Twin("keep-shut", "keep"), "vault-shut": Twin("vault-shut", "vault")},
    links=[
        Link("gate-plaza", "festival-square"), Link("festival-square", "lantern-market"),
        Link("lantern-market", "thornbridge-gate"),
        Link("festival-square", "sunspire-steps", kind="stairs", xa=1000, xb=200, ya=720, yb=720,
             label_a="The Sunspire Steps", label_b="The Festival Square"),
    ],
    assign={
        "tamsin": "lantern-market", "townhouse-south": "festival-square", "ivy-south": "festival-square",
        "townhouse-market": "lantern-market", "rose-garden": "lantern-market", "turret-east": "thornbridge-gate",
        "farmhand": "thornbridge-gate", "smith": "lantern-market", "watch-b": "festival-square",
    },
    place={
        # The gate plaza, west to east.
        "docks": (300, 700), "turret-south": (500, None), "chapel": (880, None), "ivy-west": (1260, None),
        "shop-south": (1700, None), "rose-west": (2160, None), "awning-south": (2620, None),
        # The festival square: the waypoint crystal, the stair up, the fountain, the gate, two houses.
        "start": (1600, 800), "gate": (2300, 700), "crystal": (620, 700), "ivy-south": (2700, None),
        "townhouse-south": (2980, None), "watch-a": (2840, 660), "watch-b": (2940, 690),
        # The steps: the keep's door and the vault's stair.
        "keep": (760, None), "vault": (1160, None),
        # The market: the board and Fennick, the Flagon, the stalls, the bakery, the forge and its smith.
        "board": (240, 660), "fennick": (330, 700), "tamsin": (460, 760), "messenger": (620, 700),
        "lantern-seller": (1000, 690),
        "flagon": (560, None), "rose-south": (900, None), "rose-garden": (1240, None),
        "cottage-garden": (1860, None), "bakery": (2160, None), "forge": (2460, None), "smith": (2560, 700),
        "awning-east": (2740, None), "townhouse-market": (2980, None),
        # The Thornbridge gate.
        "turret-east": (520, None), "townhouse-east": (1100, None), "farmhand": (760, 760), "east-gate": (1400, 700),
    },
    fixed=["road", "keep", "vault", "flagon", "forge", "fennick", "smith"],
    badges={"trader": "shop", "spice-seller": "bottle", "smith": "anvil"},
    span={"city-gate": (480, 760)},
    prop_layer={"fountain": "prop", "gilded-flagon-sign": "drop", "rowboat": "drop", "sewer-grate": "drop",
                "statue": "prop", "crystal-pillar": "mid", "flower-arch": "mid", "vault-stair": "back",
                "stall-teal": "prop", "stall-cloth": "prop",
                "vault-stair-shut": "back"},
    prop_place={"fountain@42,28": (1600, 700), "statue@32,23": (380, 650), "statue@52,23": (2600, 650),
                "vault-stair@56,12": (1160, None), "vault-stair-shut@56,12": (1160, None)},
))

# The Old Road: the farmland out of the south gate, and the fork where the barrow path goes north and
# the road goes on east to the Mirewood, past the fallen tree.
zone(ZonePlan(
    "road", region="saltwind", danger=10, life="meadow",
    comments=["The Old Road: the farmland and the fork. Generated by tools/rooms/convert_maps.py from",
              "the old road.map and tools/rooms/plan.py; don't edit it."],
    rooms=[
        RoomPlan("farmland", "The Farmland", W32, (0, 12, 25, 21), legacy=[(0, 0, 25, 36)]),
        RoomPlan("fork", "The Fork", W32, (25, 9, 64, 27), legacy=[(25, 0, 64, 36)]),
    ],
    exits={
        "questeria": ExitSpec("west", room="farmland", label="Questeria"),
        "barrow": ExitSpec("door", room="fork", x=1320, badge="gate", name="The Barrow"),
        "mirewood": ExitSpec("east", room="fork", label="The Mirewood"),
    },
    twins={"blocked": Twin("blocked", "mirewood")},
    links=[Link("farmland", "fork")],
    place={"start": (300, 730), "barrow": (1320, 700), "mirewood": (2960, 730), "stone": (1620, 690),
           "fork": (1120, 680), "camp": (1960, 800)},
    prop_layer={"wheat": "mid", "fence-wood-side": "drop", "fallen-tree": "prop", "log": "prop", "stump": "prop"},
    prop_place={"fallen-tree@57,18.5": (2700, 760), "log@56.6,13.9": (2560, 640), "log@57.6,19.6": (2820, 830),
                "stump@54.6,19.8": (2440, 820)},
))

# The barrow: the steps down and the passage, the hall, and the side room behind the stone door.
zone(ZonePlan(
    "barrow", region="saltwind", life="dungeon",
    comments=["The barrow: the steps, the hall and the side room. Generated by tools/rooms/convert_maps.py",
              "from the old barrow.map and tools/rooms/plan.py; don't edit it."],
    rooms=[
        RoomPlan("steps", "The Barrow Steps", W16, (14, 20, 26, 30), legacy=[(12, 20, 28, 30)]),
        RoomPlan("hall", "The Barrow Hall", W32, (2, 12, 32, 20), legacy=[(0, 12, 40, 20)], fight=(400, 2800)),
        RoomPlan("side-room", "The Side Room", W16, (2, 6, 8, 11), legacy=[(0, 0, 12, 12)], priority=-1),
    ],
    exits={"road": ExitSpec("west", room="steps", label="The Old Road")},
    idoors={
        "tomb": IDoor("door", room="hall", x=1800, badge="door", name="Tomb door",
                      why="The tomb door is locked fast, and there's no keyhole in it."),
        "side": IDoor("link", room="hall"),
    },
    links=[
        Link("steps", "hall"),
        Link("hall", "side-room", kind="door", xa=420, xb=260, id_a="side", id_b="side-out", when="side-gate",
             badge_a="gate", badge_b="gate", name_a="Stone door", name_b="Stone door", spawn_a="side-door",
             spawn_b="side-room", twin_id="side-shut",
             locked_why="The stone door won't move by hand. Something else in the hall works it."),
    ],
    assign={"cold": "steps"},
    place={"start": (220, 730), "daylight": (180, 640), "side-lever": (2900, None), "side-chest": (900, 720)},
    span={"cold": (1240, 1480), "bark-barrow-inside": (600, 1000)},
    prop_room={"sarcophagus@19.5,6.5": None, "urn@16,5": None, "urn-blue@23,5": None, "sconce@19,3": None},
))

# The Mirewood: the edge where the road comes in and the gloomfangs are, the deep wood with the bridge
# and the quarry's mouth, and the rangers' camp.
zone(ZonePlan(
    "mirewood", region="mirewood", danger=20, life="wood",
    comments=["The Mirewood: the edge, the deep wood and the camp. Generated by tools/rooms/convert_maps.py",
              "from the old mirewood.map and tools/rooms/plan.py; don't edit it."],
    rooms=[
        RoomPlan("edge", "The Mirewood's Edge", W32, (0, 9, 24, 24), legacy=[(0, 0, 24, 26), (0, 26, 4, 40), (22, 26, 24, 40)], priority=1,
                 fight=(1000, 2800)),
        RoomPlan("deep", "The Deep Wood", W32, (24, 4, 50, 26), legacy=[(24, 0, 50, 40)], priority=1),
        RoomPlan("camp", "The Rangers' Camp", W16, (8, 26, 20, 34), legacy=[(4, 26, 22, 40)]),
    ],
    exits={
        "road": ExitSpec("west", room="edge", label="The Old Road"),
        "quarry": ExitSpec("door", room="deep", x=2120, badge="gate", name="The Old Quarry"),
    },
    links=[Link("edge", "deep"), Link("deep", "camp")],
    place={"start": (300, 760), "road": (300, 760), "quarry": (2120, 690), "camp": (420, 760),
           "glade": (760, 660), "west-sign": (560, 640), "treeline": (2000, 650), "sera": (2000, 650)},
    fixed=["sera"],
    spawn_fallbacks={"treeline": ("edge", 2000, 650, "left"), "fire": ("camp", 1045, 745, "left"),
                     "camp-edge": ("camp", 1413, 700, "left")},
    prop_layer={"campfire": "prop"},
))

# The Old Quarry: the lip (the upper cut, in from the ramp), the second cut past the first roots, and
# the floor past the second, where the courier lies and the Thornmother's den is.
zone(ZonePlan(
    "mirewood-quarry", region="mirewood", life="dungeon",
    comments=["The Old Quarry: the lip, the cut and the floor. Generated by tools/rooms/convert_maps.py from",
              "the old mirewood-quarry.map and tools/rooms/plan.py; don't edit it."],
    rooms=[
        RoomPlan("lip", "The Quarry's Lip", W32, (2, 21, 34, 28), legacy=[(0, 19, 36, 32)],
                 props=[zf.Prop("root-wall", 3020, 740, None, zf.Presence(None, "quarry-roots-1"))]),
        RoomPlan("cut", "The Second Cut", W16, (2, 9, 13, 19), legacy=[(0, 7, 13, 19)],
                 props=[zf.Prop("root-wall", 1420, 740, None, zf.Presence(None, "quarry-roots-2"))]),
        RoomPlan("floor", "The Quarry Floor", W32, (3, 2, 34, 16), legacy=[(0, 0, 36, 7), (13, 7, 36, 19)],
                 fight=(1300, 3136), priority=1),
    ],
    exits={"mirewood": ExitSpec("west", room="lip", label="The Mirewood")},
    idoors={"roots-1": IDoor("link", room="lip"), "roots-2": IDoor("link", room="cut")},
    links=[
        Link("lip", "cut", id_a="roots-1", when="quarry-roots-1", twin_id="roots-1-shut", locked_label="Roots",
             locked_why="A wall of black roots grows across the way, thick as your arm. Sera might know what to do."),
        Link("cut", "floor", id_a="roots-2", when="quarry-roots-2", twin_id="roots-2-shut", locked_label="Roots",
             locked_why="More of the roots, worse than the first. Sera might know what to do."),
    ],
    place={"start": (240, 730), "mouth": (240, 730), "store": (520, 700), "seep": (2620, 680),
           "daylight": (160, 640), "daylight-in": (380, 700)},
    span={"roots-1-sera": (2760, 3100), "roots-2-sera": (1180, 1500), "bark-quarry-roots": (300, 900),
          "bark-quarry-down": (300, 700)},
    spawn_fallbacks={"quarry-mouth": ("floor", 1253, 747, "left")},
))

# Thornbridge: the camp at the east gate, the tents, and the river's edge where the Hollowed come.
zone(ZonePlan(
    "thornbridge", region="kestrel", life="meadow",
    comments=["Thornbridge: the camp gate, the tents and the river's edge. Generated by",
              "tools/rooms/convert_maps.py from the old thornbridge.map and tools/rooms/plan.py; don't edit it."],
    rooms=[
        RoomPlan("camp-gate", "The Camp Gate", W16, (3, 9, 16, 16), legacy=[(0, 0, 16, 16)],
                 scenery=[("mid", "east-gate", 300)]),
        RoomPlan("tents", "The Tents", W32, (2, 12, 24, 30), legacy=[(0, 16, 24, 30), (16, 0, 24, 16)], priority=1),
        RoomPlan("river-edge", "The River's Edge", W16, (24, 6, 40, 24), legacy=[(24, 0, 40, 30)], fight=(200, 1536)),
    ],
    exits={"questeria": ExitSpec("west", room="camp-gate", label="Questeria")},
    links=[Link("camp-gate", "tents"), Link("tents", "river-edge")],
    assign={"refugee-f": "tents"},
    place={"start": (260, 720), "gate": (260, 720)},
    prop_layer={"east-gate": "drop", "stone-bridge": "mid", "cookfire": "prop"},
))

# The Old Crown Vault: the antechamber with the warden's room, the long hall, the relic gallery
# behind the Crown Door, and the treasury.
zone(ZonePlan(
    "old-crown-vault", region="questeria", life="dungeon",
    comments=["The Old Crown Vault: the entrance, the halls, the strongroom and the treasury. Generated by",
              "tools/rooms/convert_maps.py from the old old-crown-vault.map and tools/rooms/plan.py; don't edit it."],
    rooms=[
        RoomPlan("entrance", "The Antechamber", W32, (4, 22, 26, 32), legacy=[(5, 19, 26, 36), (0, 32, 5, 36)], priority=1),
        RoomPlan("halls", "The Long Hall", W32, (0, 12, 37, 19), legacy=[(0, 11, 38, 19), (0, 19, 5, 32)]),
        RoomPlan("strongroom", "The Relic Gallery", W32, (3, 2, 35, 10), legacy=[(0, 0, 38, 11)], fight=(1700, 3136)),
        RoomPlan("treasury", "The Treasury", W16, (26, 23, 37, 32), legacy=[(26, 19, 38, 36)]),
    ],
    exits={"stair-up": ExitSpec("door", room="entrance", x=1500, badge="stair-up", name="Up to the Terrace")},
    idoors={"outer": IDoor("link", room="entrance"), "treasury": IDoor("link", room="entrance"),
            "crown": IDoor("link", room="halls")},
    twins={"crown-shut": Twin("crown-shut", "crown")},
    links=[
        Link("entrance", "halls", kind="door", xa=2160, xb=2480, id_a="outer", id_b="halls-out", badge_a="gate",
             badge_b="gate", name_a="The Vault Doors", name_b="The Vault Doors", spawn_a="entrance-doors",
             spawn_b="halls-doors"),
        Link("entrance", "treasury", kind="door", xa=2900, xb=260, id_a="treasury", id_b="treasury-out",
             badge_a="gate", badge_b="gate", name_a="The Treasury Doors", name_b="The Treasury Doors",
             spawn_a="entrance-treasury", spawn_b="treasury-doors"),
        Link("halls", "strongroom", kind="door", xa=1640, xb=600, id_a="crown", id_b="strongroom-out",
             when="crown-door", badge_a="gate", badge_b="gate", name_a="The Crown Door", name_b="The Crown Door",
             spawn_a="halls-crown", spawn_b="strongroom-door", twin_id="crown-shut"),
    ],
    assign={"tunnel": "halls", "treasury": "entrance"},
    place={"start": (1500, 730), "stair": (1500, 730), "construct-1": (1900, 650), "construct-2": (2420, 650),
           "crown-lever": (780, None), "warden-chest": (1040, 720), "daylight": (1500, 640),
           "construct-3": (1400, 650), "construct-4": (1880, 650)},
    span={"bark-vault-down": (1300, 1700), "bark-vault-constructs": (1760, 2560), "tunnel": (90, 250)},
    fixed=["construct-1", "construct-2", "construct-3", "construct-4"],
    spawn_fallbacks={"relic-shelf": ("strongroom", 2896, 630, "left")},
))

# Sunspire Keep: the throne room and the great hall as one long hall, the throne at the left and the
# door out at the right. It's the old map turned on its side: its rows run left to right.
zone(ZonePlan(
    "sunspire-keep", region="questeria", life="interior",
    comments=["Sunspire Keep: the great hall, the throne at the left and the door at the right. Generated by",
              "tools/rooms/convert_maps.py from the old sunspire-keep.map and tools/rooms/plan.py; don't edit it."],
    rooms=[RoomPlan("great-hall", "The Great Hall", W32, (1, 3, 43, 33), kind="interior", rotate=True,
                    legacy=[(0, 0, 44, 34)], wall_rows=(0, 4), wall_x=(1, 43),
                    wall_sections=[(0, 17, 120, 1700), (17, 34, 2000, 3080)])],
    exits={"door": ExitSpec("door", room="great-hall", x=3000, badge="door", name="Out to the Terrace")},
    place={
        "start": (1000, 760), "door": (2980, 790), "throne": (520, 690), "throne-left": (370, 650),
        "throne-right": (680, 770), "gallery-end": (2860, 700),
        "leon": (520, 690), "elara": (680, 770), "crane": (370, 650), "crane-court": (370, 650),
        "guard-1": (200, 640), "guard-2": (820, 640),
        "courtier-5": (1120, 700), "courtier-1": (1220, 650), "courtier-3": (1320, 820), "courtier-2": (1420, 650),
        "courtier-4": (1520, 800), "guard-3": (1740, 700), "guard-4": (2080, 760),
    },
    fixed=["leon", "elara", "crane", "crane-court", "guard-1", "guard-2"],
    spawn_fallbacks={"throne": ("great-hall", 520, 690, "right"), "throne-left": ("great-hall", 370, 650, "right"),
                     "throne-right": ("great-hall", 680, 770, "right"), "gallery-end": ("great-hall", 2860, 700, "left")},
    facing={"leon": "right", "elara": "right", "crane": "right", "crane-court": "right", "guard-1": "right",
            "guard-2": "right", "start": "left", "gallery-end": "left", "throne": "right",
            "throne-left": "right", "throne-right": "right"},
    prop_layer={"sun-throne": "back", "light-shaft": "drop", "sun-inlay": "drop", "keep-arch": "back",
                "planter-pink": "drop", "cypress-pot": "drop", "guard-statue": "back", "pillar-marble": "back",
                "brazier": "back", "potted-plant": "drop", "book-crate": "drop"},
    prop_place={"sun-throne@21.5,6.6": (520, None)},
))

# The insides. Each is one room; the way out is its west edge, back where you came in.
zone(interior("rusty-anvil", W16, (1, 3, 15, 11), region="saltwind", room_id="bar", room_name="The Bar",
              comments=["The Rusty Anvil, Brannock's bar. Generated by tools/rooms/convert_maps.py; don't edit it."],
              place={"start": (200, 740), "door": (200, 740)},
              spawn_fallbacks={"corner-table": ("bar", 1378, 690, "left")}))
zone(interior("gilded-flagon", W24, (1, 3, 21, 14), room_id="taproom", room_name="The Taproom",
              comments=["The Gilded Flagon's taproom, and the camp. Generated by tools/rooms/convert_maps.py; don't edit it."],
              place={"start": (200, 740)}))
zone(interior("bakery", W16, (1, 3, 15, 10),
              comments=["Inside the Rising Loaf. Generated by tools/rooms/convert_maps.py; don't edit it."],
              place={"start": (200, 740)}))
zone(interior("chapel", W16, (1, 3, 17, 11),
              comments=["Inside the Chapel of the Dawn. Generated by tools/rooms/convert_maps.py; don't edit it."],
              place={"start": (200, 740)}, prop_room={f"bench@{x},7.8": None for x in ("2.1875", "4.8125", "12.1875", "14.8125")}))
zone(interior("cottage-a", W16, (1, 3, 13, 9),
              comments=["Inside a cottage (the one with the bed by the door). Generated by tools/rooms/convert_maps.py; don't edit it."],
              place={"start": (200, 740)}))
zone(interior("cottage-b", W16, (1, 3, 13, 9),
              comments=["Inside a cottage (the one with the hearth). Generated by tools/rooms/convert_maps.py; don't edit it."],
              place={"start": (200, 740)}))
zone(interior("forge", W16, (1, 3, 15, 10),
              comments=["Inside Halden's Forge. Generated by tools/rooms/convert_maps.py; don't edit it."],
              place={"start": (200, 740)}, badges={"smith": "anvil"}))
zone(interior("shop", W16, (1, 3, 15, 9),
              comments=["Inside the Chandler's. Generated by tools/rooms/convert_maps.py; don't edit it."],
              place={"start": (200, 740)}, badges={"shopkeeper": "shop"}))
zone(interior("warehouse", W16, (1, 3, 17, 11), region="saltwind",
              comments=["Inside a warehouse on the docks. Generated by tools/rooms/convert_maps.py; don't edit it."],
              place={"start": (200, 740)}))
