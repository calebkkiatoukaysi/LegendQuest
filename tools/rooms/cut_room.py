"""
Cuts the owner's painting of a room into what the game draws (12-rooms 3.5): the background panels,
sky to floor, into one flat mid layer, and the foreground strips, if there are any, into the fore
layer.

    python tools/rooms/cut_room.py                            # every room that has its pictures
    python tools/rooms/cut_room.py docks questeria/lantern-market
    python tools/rooms/cut_room.py docks/pier --sources <folder> --out <folder> --zones <folder>
    python tools/rooms/cut_room.py docks/pier --painted-floor

The pictures are tools/source/rooms/<zone>/<room>/bg-<i>.png, one a panel, and fg-<i>.png for any
panel with a foreground, each 1536 x 1024 (sourcekit.py has the panels' geometry; make_room_template.py
draws the guide each one is painted over). For each room it:

1. crops every picture to its middle 1536 x 864 band and resizes it to 1600 x 900 (Lanczos-3, in
   premultiplied alpha, reading on into the bleed so the band's edges come out clean);
2. lays the panels along the room's canvas and cross-fades each into the one before it across the
   middle 200 pixels of their overlap (a smoothstep), so a seam never shows as a line;
3. writes the background as Content/Rooms/<paint>/mid.png: the whole picture, one texel to a screen
   pixel and opaque, down to the floor band's back edge, from where it fades out over 32 pixels so
   the room's tile strip (World/Tilemap.cs, the course's tilemap) is the floor, as it is in every
   room. --painted-floor keeps the painted floor (and hides the strip under it);
4. keys each foreground off its backdrop (its own transparency, or the flat #FF00FF) and writes them
   as fore.png, as wide as the room; a room with no fg pictures has no fore.png;
5. takes away the room's sky and far layers, and any other spelling of its layers: with no sky and
   no far layer RoomScene draws the room flat, nothing sliding (the foreground too);
6. writes doors.txt, each doorway the door's box in the zone file (the painting puts its doors in
   the boxes the guide marks), and thumb.png, the room with the camera in its middle cut down to
   200 x 113 for the zone map, over its tile strip;
7. when it's writing into Content/Rooms, gives each file its block in Content.mgcb (and takes out
   the blocks of the files it took away) and writes the room's line and its pictures' line in
   ASSETS.md, the pictures credited as supplied by Caleb Kiatoukaysi. Running it again changes
   nothing.

A layer wider than 4096 is written in pieces, mid-0.png, mid-1.png and on (roomkit.layer_files). It
stops on a room, naming the file, when a panel is missing, the wrong size or not a PNG, or there's a
file in the folder that isn't one of its panels; the others go on, and it exits 1 at the end. A room
with no pictures yet is left as it is. Deterministic: the same bytes every run. Only needs CPython
and numpy.
"""

import argparse
import os
import sys

import numpy as np

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import roomkit  # noqa: E402
import sourcekit as sk  # noqa: E402
from roomkit import CANVAS_H, CANVAS_W, CONTENT, THUMB_H, THUMB_W  # noqa: E402

LAYER_NAMES = ("sky", "far", "mid", "fore")
CLEAR = (24, 20, 36)
"""What's behind everything in the thumbnail: the game's clear colour."""


def load_panels(zone, room, folder):
    """The room's pictures, checked: ({i: bg}, {i: fg}), each an H x W x 4 uint8 array. Raises
    SourceError, naming the file, for anything wrong."""
    n = sk.panel_count(room.width)
    found, stray = sk.room_sources(folder, n)
    where = "%s/%s" % (zone.id, room.id)
    if stray:
        raise sk.SourceError("%s: %s in %s %s none of its panels. It's painted in %d panel%s: %s, and fg-<i>.png "
                             "for a panel with a foreground, counting from 1." % (
                                 where, ", ".join(stray), sk.shown(folder), "is" if len(stray) == 1 else "are",
                                 n, "" if n == 1 else "s", sk.panel_names(n)))
    missing = ["bg-%d.png" % i for i in range(1, n + 1) if i not in found["bg"]]
    if missing:
        raise sk.SourceError("%s: %s %s missing from %s (it's %d wide, so it's painted in %d panel%s: %s)." % (
            where, ", ".join(missing), "is" if len(missing) == 1 else "are", sk.shown(folder), room.width, n,
            "" if n == 1 else "s", sk.panel_names(n)))
    pictures = {"bg": {}, "fg": {}}
    for kind in ("bg", "fg"):
        for i, path in sorted(found[kind].items()):
            img = sk.read_png(path)
            if img.shape[:2] != (sk.PANEL_H, sk.PANEL_W):
                raise sk.SourceError("%s is %dx%d; a panel is %dx%d, the landscape picture ChatGPT makes." % (
                    sk.shown(path), img.shape[1], img.shape[0], sk.PANEL_W, sk.PANEL_H))
            pictures[kind][i] = img
    return pictures["bg"], pictures["fg"]


def band(picture, opaque):
    """A picture's 16:9 band resized to the canvas's 1600 x 900, premultiplied float64."""
    p = sk.premultiply(picture) if not opaque else picture.astype(np.float64)
    if opaque:
        p[..., 3] = 255.0
    return sk.resize(p, CANVAS_W, CANVAS_H, box=(0, sk.BLEED, sk.PANEL_W, sk.BAND_H))


def stitch(panels, width):
    """Lays the panels (canvas-sized, premultiplied; None for a panel with nothing) along the room,
    each cross-faded into what's under it across the middle of their overlap."""
    out = np.zeros((CANVAS_H, width, 4))
    covered = 0
    for p, left in zip(panels, sk.panel_lefts(width)):
        if p is None:
            continue
        weight = np.ones(sk.SPAN)
        window = sk.fade_window(left, covered)
        if window is not None:
            start, fade = window
            weight = sk.smoothstep((np.arange(sk.SPAN) + 0.5 - start) / fade)
        part = out[:, left:left + sk.SPAN]
        part[...] = part * (1.0 - weight)[None, :, None] + p * weight[None, :, None]
        covered = left + sk.SPAN
    return out


def floor_alpha(room):
    """The mid layer's opacity: 1 down to the floor band's back edge, fading to 0 FLOOR_FADE below
    it, where the tile strip is the floor."""
    line = sk.floor_line(room)
    ys = np.arange(CANVAS_H)[:, None] + 0.5
    return 1.0 - sk.smoothstep((ys - line[None, :]) / sk.FLOOR_FADE)


def cut(zone, room, bg, fg, painted_floor=False):
    """The room's layers: {"mid": uint8 RGBA, "fore": uint8 RGBA or None}."""
    n = sk.panel_count(room.width)
    mid = stitch([band(bg[i], True) for i in range(1, n + 1)], room.width)
    if not painted_floor:
        mid[..., 3] = 255.0 * floor_alpha(room)
        mid[..., :3] *= mid[..., 3:4] / 255.0
    layers = {"mid": sk.unpremultiply(mid), "fore": None}
    if fg:
        fore = stitch([band(sk.key_out(fg[i]), False) if i in fg else None for i in range(1, n + 1)], room.width)
        layers["fore"] = sk.unpremultiply(fore)
    return layers


def thumbnail(room, layers):
    """The room with the camera in its middle, over its tile strip, cut down to 200 x 113."""
    cam = max(0, (room.width - CANVAS_W) // 2)
    view = np.zeros((CANVAS_H, CANVAS_W, 4))
    view[..., :3] = CLEAR
    view[..., 3] = 255.0
    strip = sk.tile_strip(room)
    top = room.tiles.top
    sk.over(view, strip[:CANVAS_H - top, cam:cam + CANVAS_W], 0, top)
    for name in ("mid", "fore"):
        if layers.get(name) is not None:
            sk.over(view, sk.premultiply(layers[name][:, cam:cam + CANVAS_W]))
    return sk.unpremultiply(sk.resize(view, THUMB_W, THUMB_H))


def fore_share(room, fore):
    """How much of the floor band the foreground covers (12-rooms 3.1 keeps it to 12%)."""
    if fore is None:
        return 0.0
    ys = np.arange(CANVAS_H)[:, None] + 0.5
    xs = np.arange(room.width) + 0.5
    back = np.array([room.back_edge_at(x) for x in xs])[None, :]
    front = np.array([room.front_edge_at(x) for x in xs])[None, :]
    inside = (ys >= back) & (ys < front)
    return float((fore[..., 3] >= 128)[inside].mean())


def write_layer(img, folder, name):
    """Writes a layer as <name>.png, or in pieces no wider than the widest texture; returns the
    files' names."""
    files = roomkit.layer_files(name, img.shape[1])
    x = 0
    for f, w in zip(files, roomkit.piece_widths(img.shape[1])):
        sk.write_png(os.path.join(folder, f + ".png"), img[:, x:x + w])
        x += w
    return [f + ".png" for f in files]


def layer_files_in(folder, name):
    """Every spelling of a layer in a folder: <name>.png and <name>-<n>.png."""
    out = []
    for f in sk.listed(folder):
        stem = f[:-4] if f.endswith(".png") else None
        if stem == name or (stem and stem.startswith(name + "-") and stem[len(name) + 1:].isdigit()):
            out.append(f)
    return out


def door_lines(room):
    lines = ["; The painted doorway of each door in %s: id, then x y width height in canvas pixels" % room.paint,
             "; (the game divides by RoomScale). Written by tools/rooms/cut_room.py: the room is the owner's",
             "; painting, made over its guide, so each doorway is the door's box in the zone file."]
    for d in room.doors:
        x, y, w, h = room.door_box(d)
        lines.append("%s %d %d %d %d" % (d.id, round(x), round(y), round(w), round(h)))
    return lines


def write_room(zone, room, layers, out):
    """Writes the room's files into out/<paint>/ and takes away the ones it no longer has. Returns
    (written, removed), file names."""
    folder = os.path.join(out, *room.paint.split("/"))
    os.makedirs(folder, exist_ok=True)
    written = write_layer(layers["mid"], folder, "mid")
    if layers["fore"] is not None:
        written += write_layer(layers["fore"], folder, "fore")
    with open(os.path.join(folder, "doors.txt"), "w", encoding="utf-8", newline="\r\n") as f:
        f.write("\n".join(door_lines(room)) + "\n")
    print("  " + sk.shown(os.path.join(folder, "doors.txt")))
    sk.write_png(os.path.join(folder, "thumb.png"), thumbnail(room, layers))
    written += ["doors.txt", "thumb.png"]
    removed = []
    for name in LAYER_NAMES:
        for f in layer_files_in(folder, name):
            if f not in written:
                os.remove(os.path.join(folder, f))
                removed.append(f)
                print("  removed " + sk.shown(os.path.join(folder, f)))
    return written, removed


def register(zone, room, written, bg, fg, folder):
    """Content.mgcb's blocks for the room's files, and its two lines in ASSETS.md."""
    paint = room.paint
    added, removed = sk.sync_mgcb("Rooms/%s/" % paint)
    outputs = ["Content/Rooms/%s/%s" % (paint, f) for f in written]
    sources = [sk.relative(os.path.join(folder, "bg-%d.png" % i)) for i in sorted(bg)]
    sources += [sk.relative(os.path.join(folder, "fg-%d.png" % i)) for i in sorted(fg)]
    n = len(bg)
    mine = "Content/Rooms/%s/" % paint
    source_folder = sk.relative(folder) + "/"
    fore = (", its foreground strip%s keyed off %s backdrop as fore.png" % ("s" if len(fg) > 1 else "", "their" if len(fg) > 1 else "its")
            if fg else "")
    room_line = ("%s - built by tools/rooms/cut_room.py from the owner's painting of %s in %s (%s): painted flat, "
                 "one picture from the sky to the floor with nothing sliding, %s cut to %s 16:9 band, resized to "
                 "1600x900 and cross-faded into the next across their overlap, as the mid layer at one texel to a "
                 "screen pixel with its floor band left to the tile strip%s; with its 200x113 thumbnail for the zone "
                 "map and doors.txt, each doorway the door's box in the zone file; the picture was %s" % (
                     ", ".join(outputs), room.name, zone.name, source_folder,
                     "its panel" if n == 1 else "its %d panels each" % n, "its" if n == 1 else "their",
                     fore, sk.SUPPLIED))
    source_line = ("%s - %s in %s, painted in ChatGPT over its guide%s from tools/rooms/make_room_template.py: the "
                   "background, sky to floor%s, in %s of 1536x1024, %s; tools/rooms/cut_room.py cuts the room's "
                   "layers from them" % (
                       ", ".join(sources), room.name, zone.name, "" if n == 1 else "s",
                       " and the foreground strip%s" % ("s" if len(fg) > 1 else "") if fg else "",
                       "one panel" if n == 1 else "%d panels" % n, sk.SUPPLIED))
    changed = sk.sync_assets([
        ("## Rooms", lambda p: p.startswith(mine), room_line, None),
        ("## Rooms", lambda p: p.startswith(source_folder), source_line, lambda p: p.startswith(mine)),
    ])
    if added or removed or changed:
        print("  Content.mgcb: %d block%s added, %d taken out; ASSETS.md %s" % (
            len(added), "" if len(added) == 1 else "s", len(removed), "updated" if changed else "as it was"))


def main():
    ap = argparse.ArgumentParser(description="Cut the owner's painting of a room into its flat mid layer, foreground, doors and thumbnail.")
    ap.add_argument("only", nargs="*", help="zones or zone/room (default: every room with its pictures)")
    ap.add_argument("--zones", default=os.path.join(CONTENT, "Zones"), help="the folder of zone files")
    ap.add_argument("--sources", default=sk.SOURCES, help="where <zone>/<room>/bg-<i>.png are (default tools/source/rooms)")
    ap.add_argument("--out", default=sk.ROOMS, help="where <paint>/ goes (default Content/Rooms; anywhere else, nothing is registered)")
    ap.add_argument("--painted-floor", action="store_true", help="keep the painted floor in the mid layer, over the tile strip")
    args = ap.parse_args()
    try:
        sk.check_picks(args.only, args.zones)
    except sk.SourceError as e:
        sys.exit(str(e))
    into_content = os.path.normcase(os.path.abspath(args.out)) == os.path.normcase(os.path.abspath(sk.ROOMS))
    cut_count, waiting, failed = 0, 0, []
    for zone, room in sk.rooms(args.zones):
        if not sk.wanted(zone, room, args.only):
            continue
        folder = sk.room_folder(zone.id, room.id, args.sources)
        named = "%s/%s" % (zone.id, room.id) in args.only
        if not sk.listed(folder) and not named:
            waiting += 1
            continue
        print("Cutting %s/%s (%d wide, %d panel%s)..." % (zone.id, room.id, room.width, sk.panel_count(room.width),
                                                         "" if sk.panel_count(room.width) == 1 else "s"))
        try:
            if not sk.listed(folder):
                raise sk.SourceError("%s/%s has no pictures yet: they go in %s, %s." % (
                    zone.id, room.id, sk.shown(folder), sk.panel_names(sk.panel_count(room.width))))
            bg, fg = load_panels(zone, room, folder)
        except sk.SourceError as e:
            print("  " + str(e))
            failed.append("%s/%s" % (zone.id, room.id))
            continue
        layers = cut(zone, room, bg, fg, args.painted_floor)
        share = fore_share(room, layers["fore"])
        written, _ = write_room(zone, room, layers, args.out)
        if layers["fore"] is not None:
            print("    the foreground covers %.1f%% of the floor band%s" % (
                share * 100, "" if share <= roomkit.FORE_LIMIT else " (over the 12% the rooms keep to)"))
        if into_content:
            register(zone, room, written, bg, fg, folder)
        cut_count += 1
    print("cut_room: %d room%s cut%s%s." % (
        cut_count, "" if cut_count == 1 else "s",
        "; %d with no pictures yet, left as they are" % waiting if waiting else "",
        "; failed: " + ", ".join(failed) if failed else ""))
    if failed:
        sys.exit(1)


if __name__ == "__main__":
    main()
