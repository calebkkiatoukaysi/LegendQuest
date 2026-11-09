"""
Says which rooms and floor tiles the owner has painted, and which are still to do (12-rooms 3.5):

    python tools/rooms/check_room_sources.py            # every room and tileset
    python tools/rooms/check_room_sources.py --missing  # only what's still to paint

For each room in Content/Zones: how many panels it's painted in, which of
tools/source/rooms/<zone>/<room>/bg-<i>.png it has (and fg-<i>.png), and whether cut_room.py has cut
them yet (its folder in Content/Rooms has no sky or far layer, and its mid layer is newer than every
picture). For each tileset in World/TilesetCatalog.cs: which of tools/source/tiles/<tileset>/<tile>.png
it has, the tiles a room's floor uses first (cut_tiles.py's tiles-<tileset>.txt says what each one
has to be).

It only reads the pictures' headers, so it's quick. Nothing missing is a problem (the rooms keep the
art they have until their pictures come); what is: a picture that's the wrong size or not a PNG, a
file that's no panel or no tile, a folder for a room or a tileset that doesn't exist. It exits 1 if
there's one of those, after listing them.

Only needs CPython and numpy (sourcekit.py's).
"""

import argparse
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import sourcekit as sk  # noqa: E402
from roomkit import CONTENT  # noqa: E402


def size_problem(path, want):
    """What's wrong with a picture's size or kind, or None."""
    try:
        w, h = sk.png_size(path)
    except sk.SourceError as e:
        return str(e)
    if (w, h) != want:
        return "%s is %dx%d; it should be %dx%d." % (sk.shown(path), w, h, want[0], want[1])
    return None


def cut_state(room, found):
    """Whether the room's been cut from its pictures: "cut", or what's to do."""
    folder = os.path.join(sk.ROOMS, *room.paint.split("/"))
    names = sk.listed(folder)
    if any(n.startswith(("sky", "far")) for n in names):
        return "not cut yet (run cut_room.py)"
    mids = [os.path.join(folder, n) for n in names if n.startswith("mid")]
    if not mids:
        return "not cut yet (run cut_room.py)"
    pictures = [p for kind in found.values() for p in kind.values()]
    if pictures and min(os.path.getmtime(m) for m in mids) < max(os.path.getmtime(p) for p in pictures):
        return "a picture's newer than the cut (run cut_room.py)"
    return "cut"


def main():
    ap = argparse.ArgumentParser(description="Report which rooms and tiles have the owner's pictures, and which are still to paint.")
    ap.add_argument("--missing", action="store_true", help="only list what's still to paint")
    ap.add_argument("--sources", default=sk.SOURCES)
    ap.add_argument("--tiles", default=sk.TILE_SOURCES)
    ap.add_argument("--zones", default=os.path.join(CONTENT, "Zones"))
    args = ap.parse_args()
    problems = []

    print("Rooms (tools/source/rooms/<zone>/<room>/bg-<i>.png, a panel each; templates in shots/art/room-templates):")
    rooms_done = panels_done = panels_all = 0
    known = {}
    for zone, room in sk.rooms(args.zones):
        known.setdefault(zone.id, set()).add(room.id)
        n = sk.panel_count(room.width)
        panels_all += n
        folder = sk.room_folder(zone.id, room.id, args.sources)
        found, stray = sk.room_sources(folder, n)
        for name in stray:
            problems.append("%s is none of %s/%s's panels (%s, and fg-<i>.png, counting from 1)" % (
                sk.shown(os.path.join(folder, name)), zone.id, room.id, sk.panel_names(n)))
        for kind in ("bg", "fg"):
            for path in found[kind].values():
                p = size_problem(path, (sk.PANEL_W, sk.PANEL_H))
                if p:
                    problems.append(p)
        have = sorted(found["bg"])
        panels_done += len(have)
        missing = ["bg-%d" % i for i in range(1, n + 1) if i not in found["bg"]]
        if not missing:
            rooms_done += 1
        if args.missing and not missing:
            continue
        fg = (", fg %s" % " ".join(str(i) for i in sorted(found["fg"]))) if found["fg"] else ""
        if not have:
            state = "nothing yet"
        elif missing:
            state = "has %s, missing %s" % (" ".join("bg-%d" % i for i in have), " ".join(missing))
        else:
            state = "all %d panel%s%s, %s" % (n, "" if n == 1 else "s", fg, cut_state(room, found))
        print("  %-34s %4d wide, %d panel%s  %s" % ("%s/%s" % (zone.id, room.id), room.width, n, " " if n == 1 else "s", state))
    if os.path.isdir(args.sources):
        for z in sk.listed_dirs(args.sources):
            if z not in known:
                problems.append("%s is for a zone that isn't in Content/Zones" % sk.shown(os.path.join(args.sources, z)))
                continue
            for r in sk.listed_dirs(os.path.join(args.sources, z)):
                if r not in known[z]:
                    problems.append("%s is for a room %s hasn't got" % (sk.shown(os.path.join(args.sources, z, r)), z))
            for f in sk.listed(os.path.join(args.sources, z)):
                problems.append("%s should be in a room's folder, %s/<room>/" % (sk.shown(os.path.join(args.sources, z, f)), z))

    print()
    print("Floor tiles (tools/source/tiles/<tileset>/<tile>.png, 1024x1024 and seamless; "
          "shots/art/room-templates/tiles-<tileset>.txt says what each is):")
    usage = sk.tile_usage(args.zones)
    catalog = sk.tilesets()
    tiles_done = tiles_needed = 0
    for tid, tileset in catalog.items():
        folder = os.path.join(args.tiles, tid)
        have = {}
        for f in sk.listed(folder):
            stem = f[:-4] if f.lower().endswith(".png") else None
            if stem not in tileset.ids:
                problems.append("%s is no tile of the %s tileset" % (sk.shown(os.path.join(folder, f)), tid))
                continue
            have[stem] = os.path.join(folder, f)
            p = size_problem(have[stem], (sk.TILE_PICTURE, sk.TILE_PICTURE))
            if p:
                problems.append(p)
        used = [t["name"] for t in tileset.tiles if t["name"] in usage.get(tid, {})]
        rest = [t["name"] for t in tileset.tiles if t["name"] not in usage.get(tid, {})]
        tiles_needed += len(used)
        tiles_done += sum(1 for n in used if n in have)
        need = [n for n in used if n not in have]
        if args.missing and not need:
            continue
        line = "  %-10s needed %d/%d" % (tid, len(used) - len(need), len(used))
        if need:
            line += ", missing: " + ", ".join(need)
        extra = [n for n in rest if n in have]
        if extra:
            line += "; optional ones there: " + ", ".join(extra)
        print(line)
    if os.path.isdir(args.tiles):
        for d in sk.listed_dirs(args.tiles):
            if d not in catalog:
                problems.append("%s is for a tileset World/TilesetCatalog.cs hasn't got" % sk.shown(os.path.join(args.tiles, d)))
        for f in sk.listed(args.tiles):
            problems.append("%s should be in a tileset's folder, tools/source/tiles/<tileset>/" % sk.shown(os.path.join(args.tiles, f)))

    print()
    print("check_room_sources: %d of %d rooms painted (%d of %d panels), %d of %d floor tiles the rooms use; %d problem%s." % (
        rooms_done, len([1 for _ in sk.rooms(args.zones)]), panels_done, panels_all, tiles_done, tiles_needed,
        len(problems), "" if len(problems) == 1 else "s"))
    for p in problems:
        print("  " + p)
    if problems:
        sys.exit(1)


if __name__ == "__main__":
    main()
