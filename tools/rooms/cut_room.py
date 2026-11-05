"""
Cuts a painted room into its layers (12-rooms 3.5): a background, sky to floor in one picture, and
a foreground strip on a flat key colour, both as wide as the room and 900 tall, painted over the
room's guide from make_room_template.py.

    python tools/rooms/cut_room.py docks/harbourside background.png foreground.png
    python tools/rooms/cut_room.py docks/harbourside background.png foreground.png --out <folder>
    python tools/rooms/cut_room.py --test       # cuts a picture it makes up, and checks the layers

It writes the same files generate_room_placeholders.py does, into Content/Rooms/<paint>/ unless
--out says otherwise:

- sky.png: the background above the horizon line, carried on down to the roofline in its bottom
  row so the far layer sliding over it never shows a gap; at 4x.
- far.png: between the horizon and the roofline, carried a little way below it; at 2x.
- mid.png: everything below the roofline; at 3x. A --mask picture (anything drawn on it is mid,
  whatever band it's in) keeps a roof that pokes above the roofline with the house under it.
- fore.png: the foreground with its key colour taken out (magenta, or whatever --key says); at 4x.
- thumb.png, through generate_room_placeholders.py's composer.

The bands are the template's: horizon and roofline from the room file (roomkit.paint_bands), or
--horizon and --roofline. An interior is all mid layer. Each layer is the middle stretch of the
painting as wide as the layer is (roomkit.layer_columns), so with the camera in the middle of the
room the layers put back together are the painting exactly, and they slide apart from there.
Every art pixel is the middle colour, by brightness, of the canvas pixels under it, the way
cut_supplied.py samples, so the painting's own colours survive.

This is the first version, enough to cut a one-screen room: in a wide room whatever's above the
roofline moves with the far layer, so a house's roof wants the mask. M13 finishes it.

Only needs CPython, and there's nothing random in it.
"""

import argparse
import os
import sys
import tempfile

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from roomkit import (  # noqa: E402
    CANVAS_H, CONTENT, LAYERS, Img, layer_columns, layer_size, load_zone, paint_bands, zone_files,
)
import zonefile  # noqa: E402

MAGENTA = (255, 0, 255)
KEY_SLACK = 40
"""How far from the key colour, in each channel, still counts as key."""


def sample(picture, mask, x0, scale, w, h):
    """Cuts a stretch of a picture down to w by h art pixels, scale canvas pixels each, starting at
    canvas column x0 (which can be off either end; that's clear). mask(x, y) says which canvas
    pixels belong to the layer: an art pixel is drawn if most of its canvas pixels do, in the
    middle colour of them by brightness."""
    out = Img(w, h)
    for ty in range(h):
        for tx in range(w):
            mine, total = [], 0
            for y in range(ty * scale, min(picture.h, (ty + 1) * scale)):
                for x in range(x0 + tx * scale, x0 + (tx + 1) * scale):
                    total += 1
                    if 0 <= x < picture.w and mask(x, y):
                        r, g, b, a = picture.get(x, y)
                        if a > 128:
                            mine.append((r + g + b, r, g, b))
            if mine and len(mine) * 2 >= total:
                mine.sort()
                _, r, g, b = mine[len(mine) // 2]
                out.set(tx, ty, (r, g, b))
    return out


def cut(room, background, foreground, horizon=None, roofline=None, mask=None, key=MAGENTA):
    """The layers of a painted room, by name."""
    if background.w != room.width or background.h != CANVAS_H:
        raise SystemExit("The background is %dx%d; room %s is %dx%d." % (background.w, background.h, room.id, room.width, CANVAS_H))
    if foreground is not None and (foreground.w != room.width or foreground.h != CANVAS_H):
        raise SystemExit("The foreground is %dx%d; it has to be the room's size, %dx%d." % (foreground.w, foreground.h, room.width, CANVAS_H))
    h0, r0 = paint_bands(room)
    horizon = h0 if horizon is None else horizon
    roofline = r0 if roofline is None else roofline
    if room.kind == "street" and not 0 < horizon < roofline < CANVAS_H:
        raise SystemExit("The horizon (%d) has to be above the roofline (%d), both inside the room." % (horizon, roofline))

    def is_mid(x, y):
        return y >= roofline or (mask is not None and mask.alpha(x, y) > 128)

    layers = {}
    if room.kind == "street":
        layers["sky"] = band_layer(room, background, "sky", lambda x, y: y < horizon and not is_mid(x, y), roofline)
        layers["far"] = band_layer(room, background, "far",
                                   lambda x, y: horizon <= y < roofline and not is_mid(x, y), roofline + 12)
    x0, _ = layer_columns(room, "mid")
    w, h = layer_size(room, "mid")
    layers["mid"] = sample(background, (lambda x, y: True) if room.kind == "interior" else is_mid, x0, LAYERS["mid"][0], w, h)
    w, h = layer_size(room, "fore")
    x0, _ = layer_columns(room, "fore")
    if foreground is None:
        layers["fore"] = Img(w, h)
    else:
        def not_key(x, y):
            r, g, b, a = foreground.get(x, y)
            return a > 128 and not all(abs(c - k) <= KEY_SLACK for c, k in zip((r, g, b), key))
        layers["fore"] = sample(foreground, not_key, x0, LAYERS["fore"][0], w, h)
    return layers


def band_layer(room, picture, layer, inside, carry_to):
    """A layer cut out of a band of the painting, its bottom row carried down to carry_to (canvas
    pixels) so the layer under nothing never shows through as it slides."""
    scale = LAYERS[layer][0]
    x0, _ = layer_columns(room, layer)
    w, h = layer_size(room, layer)
    img = sample(picture, inside, x0, scale, w, h)
    stop = min(h, -(-carry_to // scale))
    for x in range(w):
        last = None
        for y in range(stop):
            if img.alpha(x, y):
                last = img.get(x, y)
            elif last is not None:
                img.set(x, y, last)
    return img


def save(room, layers, out):
    from generate_room_placeholders import compose, thumbnail, tile_strip
    folder = os.path.join(out, *room.paint.split("/"))
    for name in ("sky", "far", "mid", "fore"):
        if name in layers:
            layers[name].save(os.path.join(folder, name + ".png"))
    middle = max(0, (room.width - 1600) // 2)
    thumbnail(compose(room, layers, tile_strip(room), middle, people=False, badges=False)).save(
        os.path.join(folder, "thumb.png"))


def find_room(zones, which):
    zone_id, _, room_id = which.partition("/")
    for path in zone_files(zones):
        zone = load_zone(path)
        if zone.id == zone_id:
            room = zone.room(room_id) if room_id else zone.rooms[0]
            if room is None:
                raise SystemExit("Zone %s has no room called %s." % (zone_id, room_id))
            return room
    raise SystemExit("There's no zone called %s in %s." % (zone_id, zones))


# --------------------------------------------------------------------------- #
# The test: a made-up room and a made-up painting of it, cut and checked
# --------------------------------------------------------------------------- #

TEST_ZONE = "\n".join([
    "[zone]", "id = test", "name = Test", "music = Exploring",
    "[room street]", "name = A Street", "width = 2560", "floor = 0 600, 2560 600, 2560 860, 0 860",
    "tiles = questeria", "tiletop = 576", "legend = # plaza",
] + ["row = " + "#" * 40] * 6 + ["spawn start = 1200 720"])

SKY_C, FAR_C, MID_C, ROOF_C, FLOOR_C, FORE_C = (120, 160, 230), (170, 150, 200), (230, 214, 190), (60, 130, 200), (200, 180, 150), (40, 30, 60)


def test_pictures(room):
    """A background in flat bands, with a house whose roof pokes above the roofline (and a mask that
    keeps it), and a foreground of two posts on magenta."""
    horizon, roofline = paint_bands(room)
    bg = Img(room.width, CANVAS_H)
    for y in range(CANVAS_H):
        c = SKY_C if y < horizon else FAR_C if y < roofline else MID_C if y < 600 else FLOOR_C
        bg.fill(0, y, room.width, 1, c)
    mask = Img(room.width, CANVAS_H)
    bg.fill(1000, roofline - 60, 400, 60, ROOF_C)
    mask.fill(1000, roofline - 60, 400, 60, ROOF_C)
    fg = Img(room.width, CANVAS_H, MAGENTA + (255,))
    for x in (400, 1900):
        fg.fill(x, 700, 48, 200, FORE_C)
    return bg, fg, mask


def run_test():
    zone = zonefile.parse_zone(TEST_ZONE, "Zones/test.txt", "test")
    room = zone.rooms[0]
    bg, fg, mask = test_pictures(room)
    layers = cut(room, bg, fg, mask=mask)
    horizon, roofline = paint_bands(room)
    problems = []

    def check(what, ok):
        if not ok:
            problems.append(what)

    for name in ("sky", "far", "mid", "fore"):
        check("%s is %s, not %s" % (name, (layers[name].w, layers[name].h), layer_size(room, name)),
              (layers[name].w, layers[name].h) == layer_size(room, name))
    sky, far, mid, fore = layers["sky"], layers["far"], layers["mid"], layers["fore"]
    check("the sky's top isn't the sky", sky.get(5, 5)[:3] == SKY_C)
    check("the sky isn't carried down to the roofline", sky.get(5, (roofline - 8) // 4)[:3] == SKY_C)
    check("the far layer's band isn't the far colour", far.get(5, (horizon + roofline) // 4)[:3] == FAR_C)
    check("the far layer isn't clear above the horizon", far.alpha(5, 10) == 0)
    check("the mid layer isn't clear above the roofline", mid.alpha(10, (roofline - 30) // 3) == 0)
    check("the mid layer's wall isn't the wall", mid.get(10, (roofline + 60) // 3)[:3] == MID_C)
    check("the masked roof didn't go in the mid layer", mid.get(1200 // 3, (roofline - 30) // 3)[:3] == ROOF_C)
    check("the masked roof went in the far layer too",
          far.get((1200 - layer_columns(room, "far")[0]) // 2, (roofline - 30) // 2)[:3] == FAR_C)
    fx0 = layer_columns(room, "fore")[0]
    check("the foreground's key wasn't taken out", fore.alpha((1000 - fx0) // 4, 200) == 0)
    check("the foreground's post is missing", fore.get((424 - fx0) // 4, 200)[:3] == FORE_C)
    if problems:
        print("cut_room: the synthetic test failed:")
        for p in problems:
            print("  " + p)
        sys.exit(1)
    with tempfile.TemporaryDirectory() as tmp:
        save(room, layers, tmp)
        written = sorted(os.listdir(os.path.join(tmp, "test", "street")))
    print("cut_room: the synthetic test passed (%s)." % ", ".join(written))


def main():
    ap = argparse.ArgumentParser(description="Cut a painted room into its layers.")
    ap.add_argument("room", nargs="?", help="zone/room")
    ap.add_argument("background", nargs="?")
    ap.add_argument("foreground", nargs="?")
    ap.add_argument("--zones", default=os.path.join(CONTENT, "Zones"))
    ap.add_argument("--out", default=os.path.join(CONTENT, "Rooms"))
    ap.add_argument("--horizon", type=int)
    ap.add_argument("--roofline", type=int)
    ap.add_argument("--mask", help="a picture as big as the room: whatever's drawn on it is mid layer")
    ap.add_argument("--key", default="255,0,255", help="the foreground's key colour, r,g,b")
    ap.add_argument("--test", action="store_true", help="cut a made-up painting and check the layers")
    args = ap.parse_args()
    if args.test:
        run_test()
        return
    if not args.room or not args.background:
        ap.error("name the room and the background (and the foreground, if there is one)")
    room = find_room(args.zones, args.room)
    background = Img.load(args.background)
    foreground = Img.load(args.foreground) if args.foreground else None
    mask = Img.load(args.mask) if args.mask else None
    key = tuple(int(v) for v in args.key.split(","))
    save(room, cut(room, background, foreground, args.horizon, args.roofline, mask, key), args.out)


if __name__ == "__main__":
    main()
