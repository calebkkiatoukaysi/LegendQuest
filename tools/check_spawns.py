#!/usr/bin/env python3
"""Checks every spawn the scripts and the code send people to is in the zone they're sent to (build plan §8).

Usage:  python tools/check_spawns.py         exits 1 if a spawn is missing from a zone that's built
        python tools/check_spawns.py --all   also lists the spawns scripts not in the game yet still need

What it reads:
- Story/04 to 08: every `place ... at <spawn>`, `walk ... to <spawn>` and `camera <spawn>` cue, which
  must be a spawn in the zone the script plays in; and every `travel <zone> <spawn>` effect and
  `travel-actor <who> <zone> [<spawn>]` cue, whose spawn must be in that zone (start if it's left out).
  After a travel-actor, the script's later cues may name spawns in the zone it reached too.
- Every C# `Travel("zone", "spawn")` in the repo (DialogueEffect.Travel, generated or hand-written).

The zone a script plays in is its header's map `x`, or the last map in its section, worked out the
way Story/tools/build_registry.py's lastmap does. A script with neither is placed by what starts it in
the zone files: a trigger or scene whose talk it is, an npc whose talk or with it is, or a fight it's
the Then of. A walk or camera target that's an actor (a cast id, you, soulbound, party, both, a
numbered copy, or an npc in the zone) isn't a spawn.

Only scripts the game has (an Id in Dialogue, generated or hand-written) can fail: a script that
isn't compiled yet plays in rooms its act will add (M6 to M8), so it's skipped and counted, and
--all lists what it'll need. Random encounters (enc-) place by anchors near the party, not spawns.

It warns when the spawn is in a different room from the one the script plays in (the room of its
trigger, scene, npc or fight). Zones not built yet (no Content/Zones/<zone>.txt) are skipped and
counted.
"""

from __future__ import annotations

import glob
import os
import re
import sys
from collections import defaultdict
from typing import Dict, List, Optional, Set, Tuple

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(HERE)
sys.path.insert(0, os.path.join(HERE, "rooms"))

import catalogs  # noqa: E402
import zonefile as zf  # noqa: E402

STORY = os.path.join(ROOT, "Story")
ZONES = os.path.join(ROOT, "Content", "Zones")
PARTY_WORDS = {"you", "soulbound", "sb", "party", "both"}
HEADER = re.compile(r"^#### (?:script |banter )?([a-z][\w-]+)(.*)$")


def script_blocks():
    """(file, line, script id, header, zone from the header or section, [(line, text)]) for every script."""
    files = sorted(glob.glob(os.path.join(STORY, "0[4-8]*.md")))
    for path in files:
        name = os.path.basename(path)
        cur, header, lastmap, inb, skip, buf, start, cur_zone = None, "", "", False, False, [], 0, ""
        with open(path, encoding="utf-8") as fh:
            for n, line in enumerate(fh, 1):
                s = line.rstrip("\n")
                if re.match(r"^#{2,3} ", s) and not inb:
                    lastmap = ""
                m = HEADER.match(s)
                if m and not inb:
                    cur, header, start = m.group(1), m.group(2), n
                    mm = re.search(r"map `([\w-]+)`", header)
                    if mm:
                        lastmap = mm.group(1)
                    zone = mm.group(1) if mm else (lastmap if not cur.startswith("enc-") else "")
                    cur_zone = zone
                if s.startswith("```"):
                    if inb and not skip and cur:
                        yield name, start, cur, header, cur_zone, buf
                    if not inb:
                        skip = s.strip() != "```"
                        buf = []
                    inb = not inb
                    continue
                if inb:
                    buf.append((n, s))


class Index:
    """The built zones: where each spawn, npc and talk is."""

    def __init__(self):
        self.zones: Dict[str, zf.Zone] = {}
        if os.path.isdir(ZONES):
            for f in sorted(os.listdir(ZONES)):
                if f.endswith(".txt"):
                    z = zf.load_zone(os.path.join(ZONES, f))
                    self.zones[z.id] = z
        self.talk_room: Dict[str, Tuple[str, str]] = {}
        self.npc_room: Dict[str, List[Tuple[str, str]]] = defaultdict(list)
        for z in self.zones.values():
            for r in z.rooms:
                for t in r.triggers:
                    if t.talk:
                        self.talk_room.setdefault(t.talk, (z.id, r.id))
                for s in r.scenes:
                    self.talk_room.setdefault(s.talk, (z.id, r.id))
                for n in r.npcs:
                    self.npc_room[n.id].append((z.id, r.id))
                    if n.talk:
                        self.talk_room.setdefault(n.talk, (z.id, r.id))
        then = catalogs.encounter_then()
        foes = catalogs.encounter_foes()
        for fight, script in then.items():
            for foe in foes.get(fight, []):
                for where in self.npc_room.get(foe, []):
                    self.talk_room.setdefault(script, where)
                    break
                break

    def spawn_room(self, zone: str, spawn: str) -> Optional[str]:
        z = self.zones.get(zone)
        r = z.room_of_spawn(spawn) if z else None
        return r.id if r else None


def main() -> int:
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
    idx = Index()
    cast = set(catalogs.cast())
    errors: List[str] = []
    warnings: List[str] = []
    skipped: Dict[str, int] = defaultdict(int)
    checked = 0
    unplaced: List[str] = []

    def is_actor(word: str, zone: Optional[str]) -> bool:
        if word in PARTY_WORDS or word in cast:
            return True
        base = re.sub(r"-\d+$", "", word)
        if base in cast:
            return True
        z = idx.zones.get(zone or "")
        return bool(z and any(n.id == word for r in z.rooms for n in r.npcs))

    state = {"built": True}

    def check(where: str, zone: str, spawn: str, plays: Optional[Tuple[str, str]], also: List[str]) -> None:
        nonlocal checked
        if zone not in idx.zones:
            skipped[zone] += 1
            return
        if not state["built"]:
            if idx.spawn_room(zone, spawn) is None and not any(o in idx.zones and idx.spawn_room(o, spawn) for o in also):
                later_missing.append(f"{where}: {spawn} isn't in {zone} yet")
            return
        checked += 1
        room = idx.spawn_room(zone, spawn)
        if room is None:
            for other in also:
                if other in idx.zones and idx.spawn_room(other, spawn):
                    return
            errors.append(f"{where}: there's no spawn called {spawn} in {zone}")
            return
        if plays and plays[0] == zone and plays[1] != room:
            warnings.append(f"{where}: {spawn} is in {zone}/{room}, and the script plays in {zone}/{plays[1]}")

    built = catalogs.dialogue_ids()
    later_scripts: Set[str] = set()
    later_missing: List[str] = []
    encounters: Set[str] = set()
    for fname, start, sid, header, zone, lines in script_blocks():
        if sid.startswith("enc-"):
            # A random encounter's spots are anchors placed near the party (08 §2.4), not spawns.
            encounters.add(sid)
            continue
        if sid not in built:
            # Not compiled into the game yet: its rooms come with its act (M6 to M8). Counted, and
            # listed with --all, but not a failure.
            later_scripts.add(sid)
        state["built"] = sid in built
        with_m = re.search(r"with `([\w-]+)`", header)
        plays = idx.talk_room.get(sid)
        if plays is None and with_m:
            npc = with_m.group(1)
            found = idx.npc_room.get(npc, [])
            if zone:
                found = [f for f in found if f[0] == zone] or found
            plays = found[0] if found else None
        if not zone and plays:
            zone = plays[0]
        reached: List[str] = []
        for n, raw in lines:
            where = f"Story/{fname} line {n} ({sid})"
            t = raw.strip()
            for fx in re.findall(r"~ ([^→]+)", t):
                for e in fx.split("·"):
                    p = e.split()
                    if len(p) >= 3 and p[0] == "travel":
                        check(where, p[1], p[2], None, [])
            body = re.sub(r"^#[\w-]+\s+", "", t)
            body = re.sub(r"^\{if [^}]*\}\s*", "", body)
            if not body.startswith("!"):
                continue
            for cue in body[1:].split("·"):
                p = cue.split()
                if not p:
                    continue
                verb = p[0]
                if verb == "travel-actor" and len(p) >= 3:
                    target = p[2]
                    check(where, target, p[3] if len(p) > 3 else "start", None, [])
                    reached.append(target)
                    continue
                spawn = None
                if verb == "place" and "at" in p:
                    spawn = p[p.index("at") + 1] if p.index("at") + 1 < len(p) else None
                elif verb == "walk" and "to" in p:
                    spawn = p[p.index("to") + 1] if p.index("to") + 1 < len(p) else None
                    if spawn and is_actor(spawn, reached[-1] if reached else zone):
                        spawn = None
                elif verb == "camera" and len(p) > 1:
                    spawn = p[1]
                    if spawn == "north":
                        if state["built"] or "--all" in sys.argv:
                            warnings.append(f"{where}: camera north is a draft; it needs a spawn")
                        spawn = None
                    elif is_actor(spawn, reached[-1] if reached else zone):
                        spawn = None
                if spawn is None:
                    continue
                current = reached[-1] if reached else zone
                if not current:
                    if state["built"]:
                        unplaced.append(f"{where}: {verb} {spawn}, and the script has no zone")
                    continue
                check(where, current, spawn, plays if not reached else None, [zone] + reached if zone else reached)

    # The code's own travels.
    for path in glob.glob(os.path.join(ROOT, "**", "*.cs"), recursive=True):
        rel = os.path.relpath(path, ROOT).replace("\\", "/")
        if rel.startswith(("obj/", "bin/")) or "/obj/" in rel or "/bin/" in rel:
            continue
        with open(path, encoding="utf-8") as fh:
            for n, line in enumerate(fh, 1):
                for zone, spawn in re.findall(r'\bTravel\("([\w-]+)", "([\w-]+)"\)', line):
                    check(f"{rel} line {n}", zone, spawn, None, [])

    for w in warnings:
        print("warning: " + w)
    for u in unplaced:
        print(u)
    if "--all" in sys.argv:
        for m in later_missing:
            print("later: " + m)
    for e in errors:
        print(e)
    later = sum(skipped.values())
    print(f"{checked} spawns checked in {len(idx.zones)} built zones: {len(errors)} missing, {len(warnings)} warnings; "
          f"{later} in {len(skipped)} zones not built yet" + (f" ({', '.join(sorted(skipped))})" if skipped else "")
          + (f"; {len(unplaced)} in a script with no zone" if unplaced else "")
          + f"; {len(later_scripts)} scripts not in the game yet and {len(encounters)} random encounters skipped"
          + (f" ({len(later_missing)} of their spawns still to place)" if "--all" in sys.argv else ""))
    return 1 if errors or unplaced else 0


if __name__ == "__main__":
    sys.exit(main())
