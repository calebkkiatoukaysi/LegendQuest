"""What the game's C# catalogs hold, read straight out of the source so the room tools can check against it.

Stdlib only, and regular expressions only: these read the tables the way they're written in the repo
(World/TilesetCatalog.cs, World/PropCatalog.cs, Story/Cast.cs, Characters/Townsfolk.cs,
Characters/Creatures.cs, Enums/Track.cs, Story/Banters.cs, Items/ItemCatalog.cs, the Dialogue scripts
and Combat/EncounterCatalog.cs). If one of those files changes shape, the function reading it raises
rather than guessing.
"""

from __future__ import annotations

import glob
import os
import re
from dataclasses import dataclass, field
from functools import lru_cache
from typing import Dict, List, Optional, Set, Tuple

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))


MAP_FOLDER = os.path.join(ROOT, "tools", "rooms", "legacy")
"""Where the old tile maps are: kept with the room tools as the converter's input, since the game doesn't read them."""


def map_file(map_id: str) -> str:
    """The path of an old map's .map file."""
    path = os.path.join(MAP_FOLDER, map_id + ".map")
    if not os.path.exists(path):
        raise FileNotFoundError(f"there's no {map_id}.map in {MAP_FOLDER}")
    return path


def _read(rel: str) -> str:
    with open(os.path.join(ROOT, rel), encoding="utf-8") as fh:
        return fh.read()


@dataclass
class TileInfo:
    name: str
    solid: bool = False
    block: int = 1
    edged: bool = False


@dataclass
class Tileset:
    id: str
    tiles: Dict[str, TileInfo] = field(default_factory=dict)


@lru_cache(maxsize=None)
def tilesets() -> Dict[str, Tileset]:
    """Every tileset, with its tiles by name."""
    text = _read("World/TilesetCatalog.cs")
    out: Dict[str, Tileset] = {}
    parts = re.split(r'\n\s*Id = "([a-z0-9-]+)",', text)
    for i in range(1, len(parts), 2):
        ts = Tileset(parts[i])
        body = parts[i + 1].split("};", 1)[0]
        for m in re.finditer(r'new\(\)\s*\{\s*Name = "([a-z0-9-]+)"([^}]*)\}', body):
            rest = m.group(2)
            blk = re.search(r"Block = (\d+)", rest)
            ts.tiles[m.group(1)] = TileInfo(m.group(1), "Solid = true" in rest, int(blk.group(1)) if blk else 1,
                                            "Edged = true" in rest)
        if not ts.tiles:
            raise RuntimeError(f"TilesetCatalog.cs: no tiles read for {ts.id}")
        out[ts.id] = ts
    if not out:
        raise RuntimeError("TilesetCatalog.cs: no tilesets read")
    return out


@dataclass
class PropInfo:
    id: str
    footprint: Tuple[int, int] = (0, 0)
    gap: int = 0
    placement: str = "Floor"
    frames: int = 1
    light: bool = False


@lru_cache(maxsize=None)
def props() -> Dict[str, PropInfo]:
    """Every prop by id, the roof colours included, with footprint (art px), gap, placement and frames."""
    text = _read("World/PropCatalog.cs")
    out: Dict[str, PropInfo] = {}
    # Each entry is a new() { Id = ..., ... } that may span lines (the ones with a light do).
    for m in re.finditer(r'new\(\)\s*\{\s*Id = "([a-z0-9-]+)"(.*?)\}\s*,\s*(?=\n)', text, re.S):
        rest = m.group(2)
        fp = re.search(r"Footprint = new Point\((\d+), (\d+)\)", rest)
        gap = re.search(r"Gap = (\d+)", rest)
        pl = re.search(r"Placement = PropPlacement\.(\w+)", rest)
        fr = re.search(r"Frames = (\d+)", rest)
        out[m.group(1)] = PropInfo(m.group(1), (int(fp.group(1)), int(fp.group(2))) if fp else (0, 0),
                                   int(gap.group(1)) if gap else 0, pl.group(1) if pl else "Floor",
                                   int(fr.group(1)) if fr else 1, "Light = new PropLight" in rest)
    colours = re.search(r"RoofColours = \{([^}]*)\}", text)
    reroofed = re.search(r"Reroofed =\s*\{(.*?)\};", text, re.S)
    if not colours or not reroofed:
        raise RuntimeError("PropCatalog.cs: couldn't read the roof colours")
    cols = re.findall(r'"([a-z]+)"', colours.group(1))
    for house, drawn in re.findall(r'\("([a-z-]+)", (null|"[a-z]+")\)', reroofed.group(1)):
        base = out.get(house)
        if base is None:
            raise RuntimeError(f"PropCatalog.cs: reroofed {house} isn't in the table")
        drawn = None if drawn == "null" else drawn.strip('"')
        for c in cols:
            if c != drawn:
                out[f"{house}-{c}"] = PropInfo(f"{house}-{c}", base.footprint, base.gap, base.placement, base.frames, base.light)
    if len(out) < 50:
        raise RuntimeError("PropCatalog.cs: too few props read")
    return out


@dataclass
class CastInfo:
    id: str
    name: str
    standing: bool


@lru_cache(maxsize=None)
def cast() -> Dict[str, CastInfo]:
    """The cast by id; standing is False for someone who's only ever a voice (no sheet and no look)."""
    text = _read("Story/Cast.cs")
    table = text.split("CastMember[] Table", 1)[1]
    out: Dict[str, CastInfo] = {}
    for m in re.finditer(r'new\("([a-z0-9-]+)",\s*"([^"]*)",\s*(null|"[^"]*"),\s*(null\)|[^\n]*)', table):
        standing = not (m.group(3) == "null" and m.group(4).startswith("null)"))
        out[m.group(1)] = CastInfo(m.group(1), m.group(2), standing)
    if not out:
        raise RuntimeError("Cast.cs: no cast read")
    return out


@lru_cache(maxsize=None)
def townsfolk() -> Set[str]:
    text = _read("Characters/Townsfolk.cs")
    m = re.search(r"All \{ get; \} = new\[\]\s*\{(.*?)\};", text, re.S)
    if not m:
        raise RuntimeError("Townsfolk.cs: couldn't read the sheets")
    return set(re.findall(r'"([a-z0-9-]+)"', m.group(1)))


@lru_cache(maxsize=None)
def creatures() -> Set[str]:
    text = _read("Characters/Creatures.cs")
    found = set(re.findall(r'new\("([a-z0-9-]+)",\s*"', text))
    if not found:
        raise RuntimeError("Creatures.cs: no creatures read")
    return found


@lru_cache(maxsize=None)
def tracks() -> Set[str]:
    text = _read("Enums/Track.cs")
    body = text.split("enum Track", 1)[1]
    body = re.sub(r"///[^\n]*", "", body)
    return set(re.findall(r"\b([A-Z][A-Za-z]+),", body))


@lru_cache(maxsize=None)
def banters() -> Set[str]:
    return set(re.findall(r'Id = "([a-z0-9-]+)"', _read("Story/Banters.cs")))


@lru_cache(maxsize=None)
def items() -> Set[str]:
    text = _read("Items/ItemCatalog.cs")
    found = set(re.findall(r'Id = "([a-z0-9-]+)"', text))
    # Some ids are constants (Id = HealingPotion), declared as const strings.
    found.update(re.findall(r'const string \w+ = "([a-z0-9-]+)"', text))
    return found


@lru_cache(maxsize=None)
def dialogue_ids() -> Set[str]:
    """Every Id = "..." in the Dialogue scripts, generated or hand-written (node ids too, which is harmless)."""
    ids: Set[str] = set()
    for path in glob.glob(os.path.join(ROOT, "Dialogue", "**", "*.cs"), recursive=True):
        with open(path, encoding="utf-8") as fh:
            ids.update(re.findall(r'\bId = "([a-z0-9-]+)"', fh.read()))
    return ids


@lru_cache(maxsize=None)
def encounter_foes() -> Dict[str, List[str]]:
    """Each encounter's Npc(...) foes, by the npc id on the map."""
    text = _read("Combat/EncounterCatalog.cs")
    out: Dict[str, List[str]] = {}
    for block in re.split(r'\n\s*Id = "', text)[1:]:
        eid = block.split('"', 1)[0]
        body = block.split("\n        },", 1)[0]
        foes = re.findall(r'Npc\("[a-z0-9-]+", "([a-z0-9-]+)"\)', body)
        if foes:
            out[eid] = foes
    return out


@lru_cache(maxsize=None)
def encounter_then() -> Dict[str, str]:
    """Each encounter's Then (the script its win plays), by encounter id."""
    text = _read("Combat/EncounterCatalog.cs")
    out: Dict[str, str] = {}
    for block in re.split(r'\n\s*Id = "', text)[1:]:
        eid = block.split('"', 1)[0]
        body = block.split("\n        },", 1)[0]
        m = re.search(r'Then = "([a-z0-9-]+)"', body)
        if m:
            out[eid] = m.group(1)
    return out


def tile_names(tileset: str) -> Optional[Set[str]]:
    ts = tilesets().get(tileset)
    return set(ts.tiles) if ts else None
