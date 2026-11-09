"""
The painted rooms' shared machinery: what a recipe paints into (Room), writing a room's layers
into the files RoomScene loads, its thumbnail, and previews of what the game will show. The recipes
themselves are in a module a family (paint_rooms_questeria.py, paint_rooms_docks.py ...), and
tools/rooms/paint_all.py is what runs them; this runs one room at a time:

    python tools/rooms/paint_room.py questeria/lantern-market --preview shots/art/rooms

A recipe is a function of a Room, registered in its family's RECIPES under "zone/room":

    def lantern_market(r):
        P.sky(r["sky"], "day", ...)                 # r["sky"], r["far"], r["mid"], r["fore"]: paint.Layers
        r.house(House(...door=Door(r.door_x("flagon"), ...)))   # a house built on a door
        r.doorway("cellar", (x, y, w, h))           # a doorway painted some other way
    RECIPES = {"questeria/lantern-market": lantern_market}

Every layer is painted at canvas resolution, one texel a screen pixel, as wide as RoomScene wants it
(1600 + parallax x (room width - 1600); an interior's mid layer and foreground are as wide as the
room), 900 tall, and written as Content/Rooms/<zone>/<room>/<layer>.png; RoomScene draws a layer at
the scale its height gives (225 world pixels / its height). A layer wider than the graphics
profile's widest texture (roomkit.MAX_TEXTURE, 4096) is written in pieces, <layer>-0.png,
<layer>-1.png ..., which RoomScene lays side by side. Then doors.txt (where each door's doorway is
painted: the recipe fails if a door in the zone file has none, or one outside its box) and
thumb.png, the room with the camera in its middle cut down to 200 x 113 for the zone map.

The previews (into a folder outside Content) are what the game will show with the camera somewhere
along the room: the sky and far layer at their parallax, the tile strip drawn from the room file and
the zone's tileset in Content/Tiles (a painted one at one to one, an old 16-texel one blown up 4x),
the mid layer, the room's floor props (the painted ones from Content/Props/Painted where there are
any), two of the cast drawn by the rig's renderer at room scale (tools/rig/render.py, 0.5), the
foreground, and a stand-in badge over every doorway. A room wider than the screen gets left, middle
and right views.

Deterministic: the same files every run. Only needs CPython and numpy.
"""

import argparse
import json
import os
import sys

import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
import paint as P  # noqa: E402
import roomkit  # noqa: E402
from paint import Path, Piece, tones  # noqa: E402
from roomkit import CANVAS_H, CANVAS_W, CONTENT, THUMB_H, THUMB_W, load_zone  # noqa: E402
import vector  # noqa: E402
from vector import premultiply, read_png, unpremultiply, write_png  # noqa: E402

OUT = os.path.join(CONTENT, "Rooms")
SHOTS = os.path.join(roomkit.ROOT, "shots", "art", "rooms")
LAYER_NAMES = ("sky", "far", "mid", "fore")


class Room:
    """What a recipe paints into: the zone file's room, its layers at canvas resolution (sky, far,
    mid and fore outdoors; mid and fore indoors), and the doorways it painted, by the door's x.

        r["mid"]            the mid layer, a paint.Layer
        r.x("far", 1500)    where something standing at room x 1500 goes in the far layer
        r.ground            the floor's back edge at the room's middle (where buildings stand)
        r.door("flagon"), r.door_x("flagon")
        r.house(House(...)) paints a house into the mid layer and keeps its doorway
        r.doorway(id, (x, y, w, h)) keeps a doorway painted some other way (a stair, an arch)
    """

    def __init__(self, zone, room):
        self.zone, self.room = zone, room
        self.street = room.kind == "street"
        self.layers = {}
        for name in (LAYER_NAMES if self.street else ("mid", "fore")):
            self.layers[name] = P.Layer(roomkit.layer_canvas_width(room, name), CANVAS_H)
        self.doorways = {}
        self.ground = room.back_edge_at(room.width / 2)

    def __getitem__(self, name):
        return self.layers[name]

    def x(self, layer, x):
        """Where something standing at room x goes in a layer (roomkit.layer_x)."""
        return roomkit.layer_x(self.room, layer, x)

    def door(self, door_id):
        for d in self.room.doors:
            if d.id == door_id:
                return d
        raise KeyError("%s/%s has no door %s" % (self.zone.id, self.room.id, door_id))

    def door_x(self, door_id):
        return self.door(door_id).x

    def door_box(self, door_id):
        """The door's box in the zone file, (x, y, w, h): its doorway has to be painted inside it."""
        return self.room.door_box(self.door(door_id))

    def house(self, house):
        """Paints a house into the mid layer and keeps its doorway against the door at its x."""
        doorway = P.building(self["mid"], house)
        if doorway is not None:
            self.doorways[round(house.door.cx)] = tuple(int(round(v)) for v in doorway)
        return doorway

    def doorway(self, door_id, rect):
        """Keeps where a door's doorway was painted, (x, y, w, h) in canvas pixels, for one not
        built by house(): a stair, an arch, a door painted on an interior's wall."""
        self.doorways[round(self.door(door_id).x)] = tuple(int(round(v)) for v in rect)

    def check_doors(self):
        """Every door in the zone file has a doorway painted inside its box."""
        for d, (bx, by, bw, bh) in roomkit.door_boxes(self.room):
            art = self.doorways.get(round(d.x))
            if art is None:
                raise SystemExit("%s/%s: nothing's painted for the door at %d (%s)" % (self.zone.id, self.room.id, d.x, d.id))
            x, y, w, h = art
            if not (bx - 1 <= x and x + w <= bx + bw + 1 and by - 1 <= y and y + h <= by + bh + 1):
                raise SystemExit("%s/%s: the doorway painted for %s, %s, isn't inside its box %s"
                                 % (self.zone.id, self.room.id, d.id, art, (bx, by, bw, bh)))


# --------------------------------------------------------------------------- #
# Writing
# --------------------------------------------------------------------------- #

def write_layer(img, folder, name):
    """Writes one layer (an H x W x 4 uint8 picture) as <name>.png, or as pieces no wider than
    MAX_TEXTURE (<name>-0.png ...), and takes away whichever of the other spelling was there."""
    files = roomkit.layer_files(name, img.shape[1])
    x = 0
    for f, w in zip(files, roomkit.piece_widths(img.shape[1])):
        write_png(os.path.join(folder, f + ".png"), img[:, x:x + w])
        x += w
    stale = [name] if len(files) > 1 else []
    i = 0 if len(files) == 1 else len(files)
    while os.path.isfile(os.path.join(folder, "%s-%d.png" % (name, i))):
        stale.append("%s-%d" % (name, i))
        i += 1
    for f in stale:
        path = os.path.join(folder, f + ".png")
        if os.path.isfile(path):
            os.remove(path)
            print("  removed " + os.path.relpath(path, roomkit.ROOT).replace(os.sep, "/"))
    return files


def write_room(r, out=OUT):
    """Writes the room's layers at canvas resolution, doors.txt, and the thumbnail. Returns the
    files written, relative to Content (for the manifest)."""
    folder = os.path.join(out, *r.room.paint.split("/"))
    written = []
    for name, layer in r.layers.items():
        written += ["%s.png" % f for f in write_layer(layer.rgba(), folder, name)]
    lines = ["; The painted doorway of each door in %s: id, then x y width height in canvas pixels" % r.room.paint,
             "; (the game divides by RoomScale). Written by tools/rooms/paint_room.py; a door that isn't",
             "; here has nothing painted for it, and the game uses its box."]
    for d in r.room.doors:
        art = r.doorways.get(round(d.x))
        if art:
            lines.append("%s %d %d %d %d" % ((d.id,) + art))
    with open(os.path.join(folder, "doors.txt"), "w", encoding="utf-8", newline="\r\n") as f:
        f.write("\n".join(lines) + "\n")
    print("  " + os.path.relpath(os.path.join(folder, "doors.txt"), roomkit.ROOT).replace(os.sep, "/"))
    middle = max(0, (r.room.width - CANVAS_W) // 2)
    shot = compose(r, middle, figures=False, badges=False, props=False)
    write_png(os.path.join(folder, "thumb.png"), resize(shot, THUMB_W, THUMB_H))
    return ["Rooms/%s/%s" % (r.room.paint, f) for f in written + ["doors.txt", "thumb.png"]]


def resize(img, w, h):
    """An area-averaging resize (premultiplied), for the thumbnail."""
    pm = premultiply(img)
    H, W = pm.shape[:2]
    ys = (np.arange(h + 1) * H // h)
    xs = (np.arange(w + 1) * W // w)
    rows = np.add.reduceat(pm, ys[:-1], axis=0) / np.diff(ys)[:, None, None]
    out = np.add.reduceat(rows, xs[:-1], axis=1) / np.diff(xs)[None, :, None]
    return unpremultiply(out)


# --------------------------------------------------------------------------- #
# Previews
# --------------------------------------------------------------------------- #

_strips = {}


def tile_strip(room):
    """The room's tile strip at canvas resolution as Tilemap draws it at time zero: ground, edges,
    then detail, from the zone's sheet in Content/Tiles (a painted sheet at one to one, an old
    16-texel one blown up)."""
    if room.paint in _strips:
        return _strips[room.paint]
    t = room.tiles
    ts = roomkit.tileset(t.tileset)
    sheet = read_png(os.path.join(CONTENT, "Tiles", t.tileset + ".png"))
    size = sheet.shape[1] // roomkit.COLUMNS
    k = 64 // size

    def frame(f):
        r_, c_ = divmod(f, roomkit.COLUMNS)
        tile = sheet[r_ * size:(r_ + 1) * size, c_ * size:(c_ + 1) * size]
        return premultiply(vector.scale_nearest(tile, k) if k > 1 else tile)

    cols, nrows = room.width // 64, len(t.rows)
    out = np.zeros((nrows * 64, cols * 64, 4))
    ground = [[ts.ids.get(t.legend.get(c, ""), 0) for c in row] for row in t.rows]
    for y in range(nrows):
        for x in range(cols):
            if ground[y][x]:
                vector.over(out, frame(ts.frame_of(ground[y][x], x, y)), x * 64, y * 64)
    for y in range(nrows):
        for x in range(cols):
            tid = ground[y][x]
            if not tid or not ts.edged(tid):
                continue
            for side, (dx, dy) in enumerate(((0, -1), (1, 0), (0, 1), (-1, 0))):
                nx, ny = x + dx, y + dy
                if 0 <= nx < cols and 0 <= ny < nrows and ground[ny][nx] and ground[ny][nx] != tid:
                    vector.over(out, frame(ts.edge_frame(tid, side)), x * 64, y * 64)
    for y, row in enumerate(t.detail):
        for x, c in enumerate(row):
            tid = ts.ids.get(t.legend.get(c, ""), 0) if c != "." else 0
            if tid:
                vector.over(out, frame(ts.frame_of(tid, x, y)), x * 64, y * 64)
    _strips[room.paint] = out
    return out


def _layer_view(r, name, cam_left):
    """A layer as the game shows it with the camera's left edge at cam_left: premultiplied, 1600
    wide, at canvas resolution."""
    layer = r.layers[name]
    left = int(round(roomkit.parallax(r.room, name) * cam_left))
    img = layer.px[:, left:left + CANVAS_W]
    out = np.zeros((CANVAS_H, CANVAS_W, 4))
    out[:img.shape[0], :img.shape[1]] = img[:CANVAS_H, :CANVAS_W]
    return out


_figures = {}


def figure(name, facing):
    """One of the cast drawn by the rig's renderer at room scale (0.5): premultiplied, and where
    its feet are in the frame."""
    key = (name, facing)
    if key not in _figures:
        sys.path.insert(0, os.path.join(roomkit.TOOLS, "rig"))
        import render  # noqa: E402
        looks = json.load(open(os.path.join(CONTENT, "Rig", "looks.json"), encoding="utf-8"))["looks"]
        frame = render.render_look(looks[name], None, 0.0, facing, "neutral", 0.5)
        _figures[key] = (premultiply(frame), render.frame_origin(0.5))
    return _figures[key]


_props = {}


def prop_picture(name):
    """A floor prop's first picture at canvas resolution, and the texels a world pixel it was drawn
    at: the painted one in Content/Props/Painted (4), or the old one (1, blown up 4x here)."""
    if name not in _props:
        painted = os.path.join(CONTENT, "Props", "Painted", name + ".png")
        old = os.path.join(CONTENT, "Props", name + ".png")
        path = painted if os.path.isfile(painted) else old
        if not os.path.isfile(path):
            _props[name] = None
        else:
            img = read_png(path)
            frames = roomkit.prop_frames().get(name, 1)
            img = img[:, :img.shape[1] // frames]
            if path == old:
                img = vector.scale_nearest(img, 4)
            _props[name] = premultiply(img)
    return _props[name]


BADGE_COLOURS = {"shop": "#e9b54c", "anvil": "#e9b54c", "bottle": "#e9b54c", "inn": "#ec963c", "trainer": "#5f8fd8",
                 "scroll": "#5f8fd8", "shield": "#5f8fd8", "sun": "#faf6ee", "door": "#faf6ee"}
BADGE_ICONS = {"inn": "tankard", "anvil": "anvil", "shop": "sun", "bottle": "bottle", "door": "diamond", "sun": "sun"}


def badge(canvas, cx, cy, kind):
    """A stand-in for the indicator layer's badge over a doorway, so a preview shows where it floats."""
    p = Piece(cx - 30, cy - 30, cx + 30, cy + 30, ss=3, pad=4)
    t = tones(BADGE_COLOURS.get(kind, "#faf6ee"))
    m = p.fill(Path.circle(cx, cy, 21), t, "base")
    p.tone(p.below(m, cy + 6), t, "shadow")
    p.edge(Path.circle(cx, cy, 21), tones("#2a4a8c"), 2.6, "base")
    P._icon(p, BADGE_ICONS.get(kind, "diamond"), cx, cy, 24)
    vector.over(canvas, p.rgba_premultiplied(reach=1.6), p.ox, p.oy)


DEFAULT_CAST = (("sera", "right", 0.42, 760), ("fennick", "left", 0.62, 712))


def compose(r, cam_left, figures=True, badges=True, props=True, cast=DEFAULT_CAST):
    """The room as the game draws it, 1600 x 900, the camera's left edge at cam_left; straight RGBA.
    `cast` is who stands where: (look, facing, share of the screen across, feet y)."""
    canvas = np.zeros((CANVAS_H, CANVAS_W, 4))
    canvas[..., :3] = np.array((24, 20, 36)) / 255.0
    canvas[..., 3] = 1.0
    for name in ("sky", "far"):
        if name in r.layers:
            vector.over(canvas, _layer_view(r, name, cam_left))
    strip = tile_strip(r.room)
    top = r.room.tiles.top
    vector.over(canvas, strip[:, int(cam_left):int(cam_left) + CANVAS_W][:CANVAS_H - top], 0, top)
    vector.over(canvas, _layer_view(r, "mid", cam_left))
    standing = []
    if props:
        for pr in r.room.props:
            img = prop_picture(pr.prop)
            if img is not None:
                standing.append((pr.y * 1.0, img, pr.x, pr.y))
        for th in r.room.things:
            img = prop_picture(th.look or th.kind)
            if img is not None:
                standing.append((th.y * 1.0, img, th.x, th.y))
    if figures:
        for name, facing, u, y in cast:
            img, (fx, fy) = figure(name, facing)
            standing.append((y, ("figure", img, fx, fy), cam_left + CANVAS_W * u, y))
    for _, img, x, y in sorted(standing, key=lambda s: s[0]):
        sx = x - cam_left
        if isinstance(img, tuple):
            _, pic, fx, fy = img
            sh = P.Layer(CANVAS_W, CANVAS_H)
            sh.soft_ellipse(sx, y + 2, 34, 9, P.SHADOW_COLOUR, 0.45)
            vector.over(canvas, sh.px)
            vector.over(canvas, pic, int(sx - fx), int(y - fy))
        else:
            vector.over(canvas, img, int(round(sx - img.shape[1] / 2)), int(round(y - img.shape[0])))
    vector.over(canvas, _layer_view(r, "fore", cam_left))
    if badges:
        for d, (bx, by, bw, bh) in roomkit.door_boxes(r.room):
            art = r.doorways.get(round(d.x))
            ax, ay, aw, ah = art if art else (bx, by, bw, bh)
            sx = ax + aw / 2 - cam_left
            if -40 < sx < CANVAS_W + 40:
                badge(canvas, sx, ay - 30 - 22, d.badge)
    return unpremultiply(canvas)


def fore_share(r):
    """How much of the floor band the foreground covers, with the camera at each end and the middle."""
    worst = 0.0
    t, b = roomkit.band(r.room)
    for cam in sorted({0, max(0, (r.room.width - CANVAS_W) // 2), max(0, r.room.width - CANVAS_W)}):
        v = _layer_view(r, "fore", cam)
        a = v[int(t):int(b), :, 3]
        worst = max(worst, float((a >= 0.5).mean()))
    return worst


def previews(r, folder):
    """Writes the room's previews: <room>-preview.png with the camera in the middle, and -left and
    -right for a room wider than the screen. Returns their paths."""
    base = os.path.join(folder, "%s-%s" % (r.zone.id, r.room.id))
    out = []
    middle = max(0, (r.room.width - CANVAS_W) // 2)
    write_png(base + "-preview.png", compose(r, middle))
    out.append(base + "-preview.png")
    if r.room.width > CANVAS_W:
        for tag, cam in (("left", 0), ("right", r.room.width - CANVAS_W)):
            write_png("%s-%s.png" % (base, tag), compose(r, cam))
            out.append("%s-%s.png" % (base, tag))
    return out


def paint_room(zone, room, recipe, out=OUT, preview=None):
    """Runs a recipe on a room, checks its doors, writes it, and (with preview, a folder) its
    previews. Returns the Content files it wrote."""
    r = Room(zone, room)
    recipe(r)
    r.check_doors()
    files = write_room(r, out)
    share = fore_share(r)
    print("    the foreground covers %.1f%% of the floor band%s" % (share * 100, "" if share <= roomkit.FORE_LIMIT else "  (over the 12%!)"))
    if preview:
        previews(r, preview)
    return files


def load_room(key):
    """The zone and room a "zone/room" key names."""
    zone_id, room_id = key.split("/")
    zone = load_zone(os.path.join(CONTENT, "Zones", zone_id + ".txt"))
    for room in zone.rooms:
        if room.id == room_id:
            return zone, room
    raise SystemExit("%s has no room %s" % (zone_id, room_id))


def main():
    import paint_all
    ap = argparse.ArgumentParser(description="Paint one room (or a few) by its recipe; paint_all.py paints everything.")
    ap.add_argument("only", nargs="+", help="zone/room to paint")
    ap.add_argument("--out", default=OUT, help="where Rooms/<paint>/ goes (default Content/Rooms)")
    ap.add_argument("--preview", help="write 1600x900 previews into this folder (not in Content)")
    args = ap.parse_args()
    if args.preview and os.path.abspath(args.preview).startswith(os.path.abspath(CONTENT)):
        sys.exit("Previews don't go in Content: name a folder outside it.")
    recipes = paint_all.recipes()
    for key in args.only:
        if key not in recipes:
            sys.exit("No recipe for %s yet." % key)
        zone, room = load_room(key)
        print("Painting %s..." % key)
        paint_room(zone, room, recipes[key], args.out, args.preview)
    print("Done.")


if __name__ == "__main__":
    main()
