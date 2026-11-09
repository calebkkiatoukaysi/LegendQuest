"""
Checks that ASSETS.md, Content.mgcb and the files on disk all agree. Run it from the repo root
before every commit:

    python tools/check_assets.py

It fails with a list of problems if:
  - a file under Content/ or tools/source/, or a script in tools/, tools/rooms/ or tools/rig/, isn't named in
    ASSETS.md
  - ASSETS.md names a file that doesn't exist
  - a /build: or /copy: line in Content.mgcb points at a file that doesn't exist
  - a file under Content/ isn't in Content.mgcb
  - a room in Content/Zones/ is missing a painted layer in Content/Rooms/ (its mid layer, foreground,
    thumbnail and doors.txt, and outdoors its sky and far layer; a layer too wide for one texture is
    there as pieces, <layer>-0.png and on), or a folder in Content/Rooms/ is for no room at all (but
    Content/Rooms/Life, the life loops' pictures, is for every room). A room the owner has painted
    (its pictures in tools/source/rooms/<zone>/<room>/) is flat once tools/rooms/cut_room.py has cut
    it: a mid layer, a thumbnail and doors.txt, a foreground only if it has one, and no sky or far
    layer, which it fails on as not cut yet

One exception to the last one: a font file counts as in Content.mgcb when a .spritefont that is in
it names the font. The pipeline reads the TTF through the .spritefont and bakes the letters out of
it, so building or copying the TTF as well would just ship it twice.

Only needs CPython.
"""

import os
import re
import sys

ROOT = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), ".."))
CONTENT = os.path.join(ROOT, "Content")
MGCB = os.path.join(CONTENT, "Content.mgcb")
ASSETS = os.path.join(ROOT, "ASSETS.md")

#: Folders under Content/ the pipeline writes into. Nothing in them is mine.
BUILD_FOLDERS = ("bin", "obj")

#: Root-level files from the MonoGame template that ASSETS.md credits.
TEMPLATE_FILES = ("Icon.ico", "Icon.bmp", "app.manifest")

#: Anything in ASSETS.md that looks like a path to a file under Content/ or tools/.
PATH_PATTERN = re.compile(r"(?:Content|tools)/[A-Za-z0-9_./-]+\.[A-Za-z0-9]+")

FONT_PATTERN = re.compile(r"<FontName>\s*([^<]+?)\s*</FontName>")

#: The folders of scripts ASSETS.md has to name every one of. tools/rig keeps the drawing library
#: (vector.py, style.py, theme.py) the room painters and the parts tools draw with.
SCRIPT_FOLDERS = ("tools", os.path.join("tools", "rooms"), os.path.join("tools", "rig"))

#: What every room's folder in Content/Rooms/ has, and what only a street's (outdoors) has too. A
#: layer (mid, fore, sky, far) wider than the widest texture is there as pieces instead,
#: <layer>-0.png, <layer>-1.png and on (tools/rooms/roomkit.py's layer_files).
ROOM_FILES = ("mid.png", "fore.png", "thumb.png", "doors.txt")
STREET_FILES = ("sky.png", "far.png")
LAYER_FILES = ("mid.png", "fore.png", "sky.png", "far.png")

#: Folders in Content/Rooms/ that are for every room, not one: the life loops' pictures.
SHARED_ROOM_FOLDERS = ("Life",)

#: Where the owner's paintings of the rooms are, <zone>/<room>/bg-1.png and on (tools/rooms/cut_room.py).
ROOM_SOURCES = os.path.join(ROOT, "tools", "source", "rooms")
FLAT_FILES = ("mid.png", "thumb.png", "doors.txt")


def rel(path):
    return os.path.relpath(path, ROOT).replace(os.sep, "/")


def files_under(folder, skip=()):
    """Every file under a folder, as repo-relative paths with forward slashes."""
    found = []
    if not os.path.isdir(folder):
        return found
    for here, dirs, files in os.walk(folder):
        if here == folder:
            dirs[:] = [d for d in dirs if d not in skip]
        dirs[:] = [d for d in dirs if d != "__pycache__"]
        for name in files:
            found.append(rel(os.path.join(here, name)))
    return sorted(found)


def mgcb_entries():
    """The files Content.mgcb builds or copies, as repo-relative paths."""
    entries = []
    with open(MGCB, encoding="utf-8") as f:
        for line in f:
            line = line.strip()
            for key in ("/build:", "/copy:"):
                if line.startswith(key):
                    # A /build: line can say where the output goes after a semicolon. Only the source matters here.
                    source = line[len(key):].split(";")[0].strip()
                    entries.append("Content/" + source.replace("\\", "/"))
    return entries


def fonts_named_by(spritefonts):
    """The font files the given .spritefont files name, as repo-relative paths."""
    fonts = set()
    for sprite in spritefonts:
        with open(os.path.join(ROOT, sprite), encoding="utf-8") as f:
            for name in FONT_PATTERN.findall(f.read()):
                fonts.add(rel(os.path.join(os.path.dirname(os.path.join(ROOT, sprite)), name)))
    return fonts


def rooms():
    """Every room in Content/Zones/, as (its paint folder, whether it's a street, zone/room), read
    straight off the zone files: its [room] line, and its kind and paint settings if it has them."""
    found = []
    folder = os.path.join(CONTENT, "Zones")
    if not os.path.isdir(folder):
        return found
    for name in sorted(os.listdir(folder)):
        if not name.endswith(".txt"):
            continue
        zone = name[:-4]
        room = None
        with open(os.path.join(folder, name), encoding="utf-8") as f:
            for line in f:
                line = line.split(";")[0].strip()
                head = re.match(r"\[room\s+([\w-]+)\]$", line)
                if head or line.startswith("["):
                    if room:
                        found.append(room)
                    room = [zone + "/" + head.group(1), True, zone + "/" + head.group(1)] if head else None
                elif room and re.match(r"kind\s*=\s*interior$", line):
                    room[1] = False
                elif room and line.startswith("paint"):
                    room[0] = line.split("=", 1)[1].strip()
        if room:
            found.append(room)
    return [tuple(r) for r in found]


def layer_there(path):
    """Whether a layer is in Content/Rooms/ whole or in pieces: 2 for both (which is wrong), 1 for
    either, 0 for neither."""
    whole = os.path.isfile(os.path.join(ROOT, path))
    pieces = 0
    if path.endswith(LAYER_FILES):
        stem = path[:-len(".png")]
        while os.path.isfile(os.path.join(ROOT, "%s-%d.png" % (stem, pieces))):
            pieces += 1
    return 2 if whole and pieces else 1 if whole or pieces else 0


def room_problems(content_files):
    """Rooms missing a painted file, and folders in Content/Rooms/ that are for no room."""
    problems = []
    painted = set()
    for paint, street, key in rooms():
        painted.add(paint)
        if os.path.isfile(os.path.join(ROOM_SOURCES, *key.split("/"), "bg-1.png")):
            for name in FLAT_FILES:
                path = "Content/Rooms/" + paint + "/" + name
                there = layer_there(path)
                if there != 1:
                    problems.append("room " + paint + (" has no " if not there else " has both " + path + " and its pieces: ")
                                    + ("" if there else path + " ") + "(run tools/rooms/cut_room.py " + key + ")")
            for name in STREET_FILES:
                if layer_there("Content/Rooms/" + paint + "/" + name):
                    problems.append("room " + paint + " is painted in tools/source/rooms/" + key + "/ but still has its "
                                    + name[:-4] + " layer: it isn't cut yet (run tools/rooms/cut_room.py " + key + ")")
            continue
        for name in ROOM_FILES + (STREET_FILES if street else ()):
            path = "Content/Rooms/" + paint + "/" + name
            whole = os.path.isfile(os.path.join(ROOT, path))
            pieces = 0
            if name in LAYER_FILES:
                stem = path[:-len(".png")]
                while os.path.isfile(os.path.join(ROOT, "%s-%d.png" % (stem, pieces))):
                    pieces += 1
            if whole and pieces:
                problems.append("room " + paint + " has " + path + " and its pieces too (run tools/rooms/paint_all.py)")
            elif not whole and not pieces:
                problems.append("room " + paint + " has no " + path + " (run tools/rooms/paint_all.py, or "
                                "tools/rooms/generate_room_placeholders.py for a room with no recipe)")
    for path in content_files:
        if not path.startswith("Content/Rooms/"):
            continue
        folder = path.rsplit("/", 1)[0][len("Content/Rooms/"):]
        if folder not in painted and folder.split("/")[0] not in SHARED_ROOM_FOLDERS:
            problems.append(path + " is for a room that isn't in Content/Zones")
    return problems


def main():
    problems = []

    with open(ASSETS, encoding="utf-8") as f:
        assets_text = f.read()
    named = {m.rstrip(".,;:") for m in PATH_PATTERN.findall(assets_text)}
    named |= {name for name in TEMPLATE_FILES if re.search(r"(^|\s)" + re.escape(name) + r"\b", assets_text, re.M)}

    content_files = files_under(CONTENT, BUILD_FOLDERS)
    source_files = files_under(os.path.join(ROOT, "tools", "source"))
    scripts = sorted(rel(os.path.join(ROOT, folder, n)) for folder in SCRIPT_FOLDERS if os.path.isdir(os.path.join(ROOT, folder))
                     for n in os.listdir(os.path.join(ROOT, folder)) if n.endswith(".py"))

    for path in content_files + source_files + scripts:
        if path not in named:
            problems.append("ASSETS.md doesn't name " + path)

    for path in sorted(named):
        if not os.path.isfile(os.path.join(ROOT, path)):
            problems.append("ASSETS.md names " + path + ", which doesn't exist")

    entries = mgcb_entries()
    for path in entries:
        if not os.path.isfile(os.path.join(ROOT, path)):
            problems.append("Content.mgcb builds " + path + ", which doesn't exist")

    covered = set(entries) | fonts_named_by([p for p in entries if p.endswith(".spritefont")])
    for path in content_files:
        if path == rel(MGCB):
            continue
        if path not in covered:
            problems.append(path + " isn't in Content.mgcb")

    problems += room_problems(content_files)

    if problems:
        print("check_assets: %d problem%s" % (len(problems), "" if len(problems) == 1 else "s"))
        for p in problems:
            print("  " + p)
        sys.exit(1)

    print("check_assets: ASSETS.md, Content.mgcb and the files agree (%d content files, %d sources, %d scripts)."
          % (len(content_files), len(source_files), len(scripts)))


if __name__ == "__main__":
    main()
