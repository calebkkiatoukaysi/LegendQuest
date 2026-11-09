"""
Paints the world in the illustrated style, everything or a part of it: the floor tilesets, the
floor props, the rooms' life (clouds, gulls, flags, flames, smoke, leaves ...), and every room by
its family's recipes; then puts every file it wrote into Content/Content.mgcb.

    python tools/rooms/paint_all.py                          # everything
    python tools/rooms/paint_all.py docks interiors          # families (and the shared parts they use)
    python tools/rooms/paint_all.py mirewood barrow/hall     # zones, or zone/room
    python tools/rooms/paint_all.py --shared                 # only the tilesets, props and life
    python tools/rooms/paint_all.py docks --no-shared --preview shots/art/rooms
    python tools/rooms/paint_all.py --list                   # who paints what
    python tools/rooms/paint_all.py --shared --parts none --assets   # only Content.mgcb and ASSETS.md

The families, one module of recipes each (FAMILIES): a module has RECIPES = {"zone/room": recipe},
a recipe being a function of a paint_room.Room (paint_room.py says what one can do). A family's
module that isn't written yet is skipped with a note, so the families can arrive one at a time.

The shared parts (SHARED), each a module with a paint() that writes its files and returns them,
relative to Content:
    tiles   paint_tiles.py   every tileset in Content/Tiles, 64 texels a tile
    props   paint_props.py   the floor props in Content/Props/Painted, 4 texels a world pixel
    life    paint_life.py    the rooms' life loops' pictures in Content/Rooms/Life

Content.mgcb: every file under Content/Tiles, Content/Props/Painted and Content/Rooms gets a block
(a texture for a .png, /copy for a .txt), and a block for a file that's gone (a layer now written
in pieces, or one no longer in pieces) is taken out. Nothing else in it is touched.

ASSETS.md (--assets): a line for every painted room (its files, and what it shows: its recipe's
docstring's first paragraph), the shared parts' lines (each module's assets_lines()), and one line
for the painting scripts, each replacing the line that already names the same files.

Deterministic: every part is. Only needs CPython and numpy.
"""

import argparse
import importlib
import os
import sys
import time

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
import roomkit  # noqa: E402

CONTENT = roomkit.CONTENT
MGCB = os.path.join(CONTENT, "Content.mgcb")

#: The families of rooms: name, the module of their recipes, and what they are.
FAMILIES = (
    ("questeria", "paint_rooms_questeria", "Questeria's streets"),
    ("docks", "paint_rooms_docks", "the docks and the Gull's Wager"),
    ("interiors", "paint_rooms_interiors", "every room indoors"),
    ("wild", "paint_rooms_wild", "the road, the Mirewood and its quarry, Thornbridge"),
    ("under", "paint_rooms_under", "the barrow and the Old Crown Vault"),
)

#: The parts every family shares, painted before the rooms.
SHARED = (
    ("tiles", "paint_tiles", "the floor tilesets"),
    ("props", "paint_props", "the floor props"),
    ("life", "paint_life", "the rooms' life loops"),
)

#: The folders under Content whose files this keeps in Content.mgcb.
MANAGED = ("Tiles", os.path.join("Props", "Painted"), "Rooms")


def _module(name):
    if not os.path.isfile(os.path.join(HERE, name + ".py")):
        return None
    return importlib.import_module(name)


def families():
    """{family: (module or None, what it is)}."""
    return {name: (_module(mod), what) for name, mod, what in FAMILIES}


def recipes(only=None):
    """Every registered recipe, {"zone/room": (family, recipe)} if only is "family", else
    {"zone/room": recipe}. Two families painting the same room is an error."""
    found, owner = {}, {}
    for name, mod, _ in FAMILIES:
        m = _module(mod)
        if m is None:
            continue
        for key, fn in getattr(m, "RECIPES", {}).items():
            if key in owner:
                raise SystemExit("%s is painted by two families, %s and %s" % (key, owner[key], name))
            found[key], owner[key] = fn, name
    if only == "family":
        return {k: (owner[k], fn) for k, fn in found.items()}
    return found


def picked(picks):
    """The rooms a list of picks names: families, zones or zone/room keys. Empty is every one."""
    every = recipes("family")
    if not picks:
        return sorted(every)
    fams = {name for name, _, _ in FAMILIES}
    keys = []
    for p in picks:
        if p in fams:
            keys += [k for k, (fam, _) in every.items() if fam == p]
        elif "/" in p:
            if p not in every:
                raise SystemExit("No recipe paints %s yet." % p)
            keys.append(p)
        else:
            got = [k for k in every if k.split("/")[0] == p]
            if not got:
                raise SystemExit("%s is neither a family nor a zone anything paints." % p)
            keys += got
    return sorted(set(keys))


# --------------------------------------------------------------------------- #
# Content.mgcb
# --------------------------------------------------------------------------- #

TEXTURE_BLOCK = (
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
)


def managed_files():
    """Every file under the managed folders, relative to Content with forward slashes."""
    out = []
    for folder in MANAGED:
        root = os.path.join(CONTENT, folder)
        for dirpath, _, names in os.walk(root):
            for n in names:
                if n.endswith((".png", ".txt")):
                    rel = os.path.relpath(os.path.join(dirpath, n), CONTENT).replace(os.sep, "/")
                    out.append(rel)
    return sorted(set(out))


def sync_mgcb(path=MGCB, quiet=False):
    """Puts a block in Content.mgcb for every file under the managed folders that hasn't one, next
    to its folder's others (or at the end), and takes out the blocks of managed files that are
    gone. Returns (added, removed)."""
    with open(path, "rb") as f:
        text = f.read().decode("utf-8")
    lines = text.replace("\r\n", "\n").split("\n")
    # the blocks: each from its #begin to the blank line after it
    blocks, i, head = [], 0, []
    while i < len(lines) and not lines[i].startswith("#begin "):
        head.append(lines[i])
        i += 1
    while i < len(lines):
        if lines[i].startswith("#begin "):
            j = i + 1
            while j < len(lines) and lines[j].strip() != "" and not lines[j].startswith("#begin "):
                j += 1
            blocks.append([lines[i][len("#begin "):].strip(), lines[i:j]])
            while j < len(lines) and lines[j].strip() == "":
                j += 1
            i = j
        else:
            i += 1
    have = {b[0] for b in blocks}
    files = managed_files()
    on_disk = set(files)
    prefixes = tuple(m.replace(os.sep, "/") + "/" for m in MANAGED)
    removed = [b[0] for b in blocks if b[0].startswith(prefixes) and b[0] not in on_disk]
    blocks = [b for b in blocks if b[0] not in removed]
    added = []
    for rel in files:
        if rel in have:
            continue
        block = [l.format(rel) for l in TEXTURE_BLOCK] if rel.endswith(".png") else ["#begin " + rel, "/copy:" + rel]
        folder = rel.rsplit("/", 1)[0] + "/"
        at = None
        for k, (name, _) in enumerate(blocks):
            if name.startswith(folder):
                at = k
        if at is None:
            parent = folder.rsplit("/", 2)[0] + "/"
            for k, (name, _) in enumerate(blocks):
                if name.startswith(parent):
                    at = k
        blocks.insert(len(blocks) if at is None else at + 1, [rel, block])
        added.append(rel)
    if not added and not removed:
        return added, removed
    out = head + []
    for _, b in blocks:
        out += b + [""]
    with open(path, "wb") as f:
        f.write("\r\n".join(out).encode("utf-8"))
    if not quiet:
        print("  Content.mgcb: %d blocks added, %d taken out" % (len(added), len(removed)))
    return added, removed


# --------------------------------------------------------------------------- #
# ASSETS.md
# --------------------------------------------------------------------------- #

ASSETS = os.path.join(roomkit.ROOT, "ASSETS.md")
CREDIT = "created by Caleb Kiatoukaysi for Legend Quest"
LICENCE = "released under public domain"

#: Each shared part's ASSETS.md section.
SHARED_SECTIONS = {"tiles": "## Tiles and props", "props": "## Tiles and props", "life": "## Rooms"}

SCRIPTS_LINE = (
    "{files} - written by Caleb Kiatoukaysi for Legend Quest, the room painters, in the illustrated style: "
    "paint.py is the drawing library (skies, the far layer's mountains, roofs and castles, houses built from "
    "walls, roofs, windows and doors, towers, gates and stairs, trees, props, interiors) on tools/rig/vector.py "
    "and style.py; paint_room.py is what a room's recipe paints into, and writes the room, its thumbnail and "
    "doors.txt, with previews; paint_all.py runs everything and keeps Content.mgcb and this file up to date; "
    "paint_tiles.py paints every floor tileset, paint_props.py the floor props and paint_life.py the rooms' "
    "life loops; and each paint_rooms_(family).py has a family's recipes{families}. They need CPython and "
    "numpy, and every one is seeded, so a rerun gives the same files, " + LICENCE)


def _docline(fn):
    """A recipe's description for ASSETS.md: its docstring's first paragraph on one line, without
    the section reference."""
    import re
    doc = (fn.__doc__ or "").strip().split("\n\n")[0]
    doc = " ".join(doc.split())
    doc = re.sub(r"\s*\(12-rooms [^)]*\)", "", doc)
    return doc.rstrip(".")


def paint_room_load(key):
    import paint_room
    return paint_room.load_room(key)


def room_line(key, family, fn):
    """The ASSETS.md line for a painted room: its files on disk, what it shows, who painted it."""
    zone, room = paint_room_load(key)
    folder = os.path.join(CONTENT, "Rooms", *room.paint.split("/"))
    files = []
    for name in ("sky", "far", "mid", "fore"):
        if os.path.isfile(os.path.join(folder, name + ".png")):
            files.append(name + ".png")
        i = 0
        while os.path.isfile(os.path.join(folder, "%s-%d.png" % (name, i))):
            files.append("%s-%d.png" % (name, i))
            i += 1
    files += ["thumb.png", "doors.txt"]
    paths = ", ".join("Content/Rooms/%s/%s" % (room.paint, f) for f in files)
    module = dict((n, m) for n, m, _ in FAMILIES)[family]
    return ("%s - %s, %s in %s, painted procedurally by tools/rooms/%s.py with tools/rooms/paint.py: %s. Its layers "
            "are one texel to a screen pixel (RoomScene draws each at the scale its height gives), with its "
            "200x113 thumbnail for the zone map and doors.txt, where each door's doorway is painted, %s"
            % (paths, CREDIT, room.name, zone.name, module, _docline(fn), LICENCE))


def _put(lines, line, section, keys):
    """Replaces the line naming any of `keys` (Content or tools paths) before its " - ", or adds the
    line at the end of `section`. Returns the lines."""
    keys = set(keys)
    for i, l in enumerate(lines):
        if not l or l.startswith("#") or " - " not in l:
            continue
        named = {q.strip() for q in l.split(" - ", 1)[0].split(",")}
        if named & keys:
            lines[i] = line
            return lines
    heads = [i for i, l in enumerate(lines) if l.startswith("## ")]
    at = None
    for k, i in enumerate(heads):
        if lines[i].strip() == section:
            at = heads[k + 1] if k + 1 < len(heads) else len(lines)
    if at is None:
        lines += ["", section, ""]
        at = len(lines)
    while at > 0 and lines[at - 1].strip() == "":
        at -= 1
    lines[at:at] = ["", line]
    return lines


def sync_assets(path=ASSETS):
    """Writes ASSETS.md's lines for every painted room, the shared parts' files and the painting
    scripts: an existing line naming the same files is replaced, a new one goes at the end of its
    section."""
    with open(path, "rb") as f:
        lines = f.read().decode("utf-8").replace("\r\n", "\n").split("\n")
    for key, (family, fn) in sorted(recipes("family").items()):
        zone, room = paint_room_load(key)
        first = ["Content/Rooms/%s/%s" % (room.paint, f) for f in ("sky.png", "mid.png", "mid-0.png", "doors.txt", "thumb.png")]
        lines = _put(lines, room_line(key, family, fn), "## Rooms", first)
    for name, mod, _ in SHARED:
        m = _module(mod)
        if m is None or not hasattr(m, "assets_lines"):
            continue
        for keys, line in m.assets_lines():
            lines = _put(lines, line, SHARED_SECTIONS[name], keys)
    scripts = sorted(n for n in os.listdir(HERE) if n.startswith("paint") and n.endswith(".py"))
    files = ", ".join("tools/rooms/" + n for n in scripts)
    what = dict((a, c) for a, _, c in FAMILIES)
    fams = [(n, mod) for n, mod, _ in FAMILIES if _module(mod) is not None]
    families_txt = (" (" + "; ".join("%s.py, %s" % (mod, what[n]) for n, mod in fams) + ")") if fams else ""
    lines = _put(lines, SCRIPTS_LINE.format(files=files, families=families_txt), "## The scripts", ["tools/rooms/" + n for n in scripts])
    with open(path, "wb") as f:
        f.write("\r\n".join(lines).encode("utf-8"))
    print("  ASSETS.md: the painted rooms', the shared parts' and the scripts' lines written")


# --------------------------------------------------------------------------- #
# Running
# --------------------------------------------------------------------------- #

def paint_shared(names, preview=None):
    """Paints the shared parts named (tiles, props, life) that are written."""
    for name, mod, what in SHARED:
        if name not in names:
            continue
        m = _module(mod)
        if m is None or not hasattr(m, "paint"):
            print("  (%s: %s.py isn't ready yet)" % (what, mod))
            continue
        t = time.time()
        print("Painting %s..." % what)
        m.paint(preview=preview)
        print("  %s took %.0f s" % (what, time.time() - t))


def main():
    ap = argparse.ArgumentParser(description="Paint the world in the illustrated style: tilesets, props, life and rooms.")
    ap.add_argument("picks", nargs="*", help="families, zones or zone/room (default: everything)")
    ap.add_argument("--preview", help="write previews into this folder (not in Content)")
    ap.add_argument("--shared", action="store_true", help="only the shared parts (tiles, props, life)")
    ap.add_argument("--no-shared", action="store_true", help="only the rooms")
    ap.add_argument("--parts", default="tiles,props,life", help="which shared parts (default tiles,props,life)")
    ap.add_argument("--no-mgcb", action="store_true", help="don't touch Content.mgcb")
    ap.add_argument("--assets", action="store_true", help="also write ASSETS.md's lines for what's painted (the lead runs this)")
    ap.add_argument("--list", action="store_true", help="say who paints what, and stop")
    args = ap.parse_args()
    if args.preview and os.path.abspath(args.preview).startswith(os.path.abspath(CONTENT)):
        sys.exit("Previews don't go in Content: name a folder outside it.")
    if args.list:
        every = recipes("family")
        for name, mod, what in FAMILIES:
            m = _module(mod)
            keys = sorted(k for k, (fam, _) in every.items() if fam == name)
            print("%-10s %-28s %s: %s" % (name, mod + ".py", what, ", ".join(keys) if m else "(not written yet)"))
        painted = {k for k in every}
        missing = []
        for f in roomkit.zone_files():
            z = roomkit.load_zone(f)
            missing += ["%s/%s" % (z.id, r.id) for r in z.rooms if "%s/%s" % (z.id, r.id) not in painted]
        print("still placeholders: " + (", ".join(missing) if missing else "none"))
        return
    if not args.no_shared:
        paint_shared(set(args.parts.split(",")), args.preview)
    if not args.shared:
        import paint_room
        every = recipes()
        import sourcekit
        for key in picked(args.picks):
            if sourcekit.has_sources(*key.split("/")):
                print("Leaving %s: it's the owner's painting (tools/source/rooms/%s), which cut_room.py cuts" % (key, key))
                continue
            zone, room = paint_room.load_room(key)
            print("Painting %s..." % key)
            t = time.time()
            paint_room.paint_room(zone, room, every[key], preview=args.preview)
            print("  %s took %.0f s" % (key, time.time() - t))
    if not args.no_mgcb:
        sync_mgcb()
    if args.assets:
        sync_assets()
    print("Done.")


if __name__ == "__main__":
    main()
