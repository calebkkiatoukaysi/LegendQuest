"""
Cuts the pixel-ready room paintings (docs/pixel-art-brief.md §7, batch 2) into the game's layered
pixel-art rooms: sky, far, mid and fore, each true pixel art at 800 x 450 art pixels a screen.

    python tools/rooms/cut_pixel_room.py                      # every room that has its pictures
    python tools/rooms/cut_pixel_room.py questeria docks/pier
    python tools/rooms/cut_pixel_room.py docks/pier --out <folder>   # anywhere else, nothing is registered

The pictures are tools/source/pixel/rooms/<zone>/<room>/bg-<i>.png, the whole panel redrawn, and
bg-<i>.sky.png, .far.png, .mid.png and .fg.png, the panel's layers, each isolated on flat #FF00FF by
an edit of the whole (ChatGPT, docs/pixel-art-batch-2-rooms/). They're 1536 x 1024 panels laid
along the room as tools/rooms/sourcekit.py says. For each room it:

1. fits every layer of a panel the way its original painting was fitted (art/rooms/runtime-fit.json:
   the reviewed floor line and door centres), so the painted doorways stay on the doors. The
   redraws were painted over the originals and sit on them exactly (scale 1, offset 0 by edge
   correlation, checked here: a panel that has moved stops the room);
2. keys each layer off its magenta, crops the panel's 16:9 band, resizes it to the canvas (1600 x
   900 a screen) and lays the panels along the room, the mid and far layers joined at a
   minimum-error seam and the sky and foreground cross-faded;
3. makes each layer pixel art: a palette of its own (LAYERS gives its size; k-means on its opaque
   pixels), every pixel snapped to it, then halved to 450 rows by majority colour per 2 x 2 cell
   (dark colours weighted, so outlines survive; never averaged, so nothing blurs), with hard alpha,
   and stray single pixels taken away (tidy);
4. writes Content/Rooms/<paint>/sky.png, far.png, mid.png (with its painted floor, over the tile
   strip) and fore.png, doors.txt, thumb.png and layers.txt, which says "layered" and each
   layer's parallax: RoomScene slides them about the room's middle, so with the camera in the
   middle every layer sits where it was painted and none runs out at either end;
5. gives each file its block in Content.mgcb and writes the room's lines in ASSETS.md.

A room whose panels have no layers (only the whole bg-<i>.png, as thornbridge/camp-gate's while its
middle and foreground are still to come) is cut flat from the whole picture: a mid layer and
nothing sliding. Deterministic. Needs numpy and scipy.
"""

import argparse
import json
import os
import sys

import numpy as np
from scipy.cluster.vq import kmeans2
from scipy.spatial import cKDTree

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
import roomkit  # noqa: E402
import sourcekit as sk  # noqa: E402
from cut_room import door_lines, layer_files_in, stitch, write_layer  # noqa: E402
from fit_backgrounds import fit_grid, warp  # noqa: E402
from roomkit import CANVAS_H, CANVAS_W, THUMB_H, THUMB_W  # noqa: E402

SOURCES = os.path.join(sk.ROOT, "tools", "source", "pixel", "rooms")
ORIGINALS = os.path.join(sk.ROOT, "art", "rooms", "generated")
FIT = os.path.join(sk.ROOT, "art", "rooms", "runtime-fit.json")

#: The source layers, what each is written as, and its palette's size.
LAYERS = (("sky", "sky", 24), ("far", "far", 40), ("mid", "mid", 64), ("fg", "fore", 32))
#: How far each layer slides for every world pixel the camera does (the mid layer 1).
PARALLAX = {"sky": 0.15, "far": 0.5, "fore": 1.0}
#: Rooms whose foreground is left out, and why: ChatGPT narrowed these rooms' foreground trees into
#: tall, thin trunks with their crowns up in the panel's top bleed, which is cropped, so in the room
#: they're bare poles from the top of the screen to the bottom. They want repainting (crowns inside
#: the band, trunks no taller than the screen's lower half) before they go back in.
NO_FOREGROUND = {
    "mirewood/deep": "bare foreground trunks, crowns cropped in the bleed",
    "mirewood/camp": "bare foreground trunks either side of the fire, crowns cropped in the bleed",
    "road/fork": "bare foreground trunks, crowns cropped in the bleed",
}
#: How many canvas pixels make an art pixel: 1600 x 900 canvas, 800 x 450 art.
ART = 2
#: A dark colour counts this much more in a cell's vote, so one-pixel outlines aren't voted away.
DARK_WEIGHT = 1.6
DARK_LUMA = 70
#: Below this alpha a canvas pixel is backdrop.
SOLID = 128
#: Edge correlation a redraw must reach against its original, and how far it may have moved.
SAME_PANEL = 0.55


# --------------------------------------------------------------------------- #
# Checking a redraw sits on its original
# --------------------------------------------------------------------------- #

def _edges(rgba, w, h):
    """A picture's edge strength, w x h, normalised: for lining two pictures up."""
    g = rgba[..., :3].astype(np.float64).mean(axis=2)
    g = sk.resize(np.repeat(g[..., None], 4, axis=2), w, h)[..., 0]
    e = np.abs(np.diff(g, axis=1, prepend=g[:, :1])) + np.abs(np.diff(g, axis=0, prepend=g[:1]))
    e -= e.mean()
    return e / (e.std() + 1e-9)


def same_panel(redraw, original):
    """How well the redraw's edges line up with the original's where they are (no shift), and the
    shift, in panel pixels, that would line them up best: (score, dx, dy)."""
    w, h = sk.PANEL_W // 4, sk.PANEL_H // 4
    a, b = _edges(original, w, h), _edges(redraw, w, h)
    c = np.fft.irfft2(np.fft.rfft2(a) * np.conj(np.fft.rfft2(b)), s=(h, w)) / (w * h)
    iy, ix = np.unravel_index(np.argmax(c), c.shape)
    dy = iy if iy < h // 2 else iy - h
    dx = ix if ix < w // 2 else ix - w
    return float(c[0, 0]), dx * 4, dy * 4


# --------------------------------------------------------------------------- #
# Pixel art
# --------------------------------------------------------------------------- #

def palette(rgba, colours):
    """A palette for a layer: k-means on its opaque pixels (every fourth one), seeded the same
    every time; float64 colours x 3."""
    solid = rgba[..., 3] >= SOLID
    pixels = rgba[..., :3][solid][::4].astype(np.float64)
    if len(pixels) == 0:
        return np.zeros((1, 3))
    unique = np.unique(pixels, axis=0)
    if len(unique) <= colours:
        return unique
    rng = np.random.default_rng(1)
    pick = pixels[rng.choice(len(pixels), size=min(len(pixels), 120000), replace=False)]
    centres, _ = kmeans2(pick, colours, iter=12, minit="++", seed=rng)
    return np.unique(np.rint(np.clip(centres, 0, 255)), axis=0)


def pixelate(rgba, colours):
    """A canvas-sized layer (uint8 RGBA) as pixel art at half the size: every pixel snapped to the
    layer's palette, then each 2 x 2 cell the colour most of it is (dark ones counting more), hard
    alpha. Returns uint8 RGBA."""
    h, w = rgba.shape[:2]
    h -= h % ART
    w -= w % ART
    rgba = rgba[:h, :w]
    pal = palette(rgba, colours)
    _, index = cKDTree(pal).query(rgba[..., :3].reshape(-1, 3).astype(np.float64))
    index = index.reshape(h, w).astype(np.int32)
    index[rgba[..., 3] < SOLID] = -1
    luma = pal @ np.array([0.299, 0.587, 0.114])
    weight_of = np.where(luma < DARK_LUMA, DARK_WEIGHT, 1.0)
    cells = [index[dy::ART, dx::ART] for dy in range(ART) for dx in range(ART)]
    weights = [np.where(c < 0, 1.0, weight_of[np.maximum(c, 0)]) for c in cells]
    best = cells[0]
    best_score = np.full(best.shape, -1.0)
    for candidate in cells:
        score = sum(np.where(other == candidate, wt, 0.0) for other, wt in zip(cells, weights))
        better = score > best_score
        best = np.where(better, candidate, best)
        best_score = np.where(better, score, best_score)
    best = tidy(best)
    out = np.zeros(best.shape + (4,), np.uint8)
    solid = best >= 0
    out[..., :3][solid] = pal[best[solid]].astype(np.uint8)
    out[..., 3][solid] = 255
    return out


def tidy(index, passes=2):
    """Takes away stray pixels: one that shares its colour with none of its four neighbours, where at
    least three of them are one colour, becomes that colour. Lines (two neighbours alike) keep."""
    for _ in range(passes):
        p = np.pad(index, 1, mode="edge")
        n = [p[:-2, 1:-1], p[2:, 1:-1], p[1:-1, :-2], p[1:-1, 2:]]
        alone = np.ones(index.shape, bool)
        for side in n:
            alone &= side != index
        fill = np.full(index.shape, -2, np.int32)
        for i in range(4):
            agree = sum((n[i] == n[j]).astype(np.int32) for j in range(4))
            fill = np.where((agree >= 3) & (fill == -2), n[i], fill)
        index = np.where(alone & (fill != -2), fill, index)
    return index


# --------------------------------------------------------------------------- #
# A room
# --------------------------------------------------------------------------- #

def panel_layers(folder, i):
    """A panel's pictures: {"whole": rgba, "sky": rgba, ...}, the layers only if all four are there."""
    out = {"whole": sk.read_png(os.path.join(folder, "bg-%d.png" % i))}
    names = [s for s, _, _ in LAYERS]
    paths = {s: os.path.join(folder, "bg-%d.%s.png" % (i, s)) for s in names}
    if all(os.path.isfile(p) for p in paths.values()):
        for s, p in paths.items():
            out[s] = sk.read_png(p)
    for name, picture in out.items():
        if picture.shape[:2] != (sk.PANEL_H, sk.PANEL_W):
            raise sk.SourceError("%s is %dx%d; a panel is %dx%d." % (
                sk.shown(paths.get(name, os.path.join(folder, "bg-%d.png" % i))), picture.shape[1], picture.shape[0],
                sk.PANEL_W, sk.PANEL_H))
    return out


def canvas_band(premultiplied):
    """A panel's 16:9 band, resized to the canvas's 1600 x 900, premultiplied."""
    return sk.resize(premultiplied, CANVAS_W, CANVAS_H, box=(0, sk.BLEED, sk.PANEL_W, sk.BAND_H))


def cut(zone, room, folder, fits):
    """The room's pixel-art layers ({name: uint8 RGBA or None}) and whether it's flat."""
    n = sk.panel_count(room.width)
    panels = [panel_layers(folder, i) for i in range(1, n + 1)]
    flat = any(len(p) == 1 for p in panels)
    grids = []
    for i, p in enumerate(panels, 1):
        key = "%s/%s/bg-%d" % (zone.id, room.id, i)
        original = os.path.join(ORIGINALS, zone.id, room.id, "bg-%d.png" % i)
        if key in fits and os.path.isfile(original):
            score, dx, dy = same_panel(p["whole"], sk.read_png(original))
            if score < SAME_PANEL or (dx, dy) != (0, 0):
                raise sk.SourceError("%s: the redraw doesn't sit on its original (edge match %.2f, best shift %d, %d); "
                                     "fit it again before cutting" % (key, score, dx, dy))
            grids.append(fit_grid(zone, room, i, fits[key]))
        else:
            grids.append(None)

    def layer(source, opaque, seam):
        pieces = []
        for p, grid in zip(panels, grids):
            picture = p[source]
            pm = sk.premultiply(picture) if not opaque else picture.astype(np.float64)
            if opaque:
                pm[..., 3] = 255.0
            else:
                pm = sk.key_out(picture)
            if grid is not None:
                pm = warp(pm, *grid)
            pieces.append(canvas_band(pm))
        return sk.unpremultiply(stitch(pieces, room.width, fitted=seam))

    colours = {s: c for s, _, c in LAYERS}
    if flat:
        return {"mid": pixelate(layer("whole", True, True), colours["mid"]), "sky": None, "far": None, "fore": None}, True
    out = {
        "sky": pixelate(layer("sky", True, False), colours["sky"]),
        "far": pixelate(layer("far", False, True), colours["far"]),
        "mid": pixelate(layer("mid", False, True), colours["mid"]),
        "fore": None if "%s/%s" % (zone.id, room.id) in NO_FOREGROUND else pixelate(layer("fg", False, False), colours["fg"]),
    }
    if out["fore"] is None or not (out["fore"][..., 3] > 0).any() or "%s/%s" % (zone.id, room.id) in NO_FOREGROUND:
        out["fore"] = None
    return out, False


def thumbnail(room, layers):
    """The room with the camera in its middle (where every layer sits as painted), cut down to 200 x 113."""
    cam = max(0, (room.width - CANVAS_W) // 2) // ART
    view = np.zeros((CANVAS_H // ART, CANVAS_W // ART, 4))
    view[..., :3] = (24, 20, 36)
    view[..., 3] = 255.0
    for name in ("sky", "far", "mid", "fore"):
        if layers.get(name) is not None:
            sk.over(view, sk.premultiply(layers[name][:, cam:cam + CANVAS_W // ART]))
    return sk.unpremultiply(sk.resize(view, THUMB_W, THUMB_H))


def layer_lines(layers, flat):
    lines = ["; Written by tools/rooms/cut_pixel_room.py: pixel-art layers, 450 texels to the screen's height,",
             "; each sliding by its parallax about the room's middle."]
    if flat:
        return lines + ["flat", "foreground no"]
    slides = " ".join("%s %g" % (name, PARALLAX[name]) for name in ("sky", "far", "fore") if layers.get(name) is not None)
    return lines + ["layered", "parallax " + slides, "foreground " + ("yes" if layers.get("fore") is not None else "no")]


def write_room(zone, room, layers, flat, out):
    """Writes the room's files into out/<paint>/ and takes away the ones it no longer has."""
    folder = os.path.join(out, *room.paint.split("/"))
    os.makedirs(folder, exist_ok=True)
    written = []
    for name in ("sky", "far", "mid", "fore"):
        if layers.get(name) is not None:
            written += write_layer(layers[name], folder, name)
    with open(os.path.join(folder, "doors.txt"), "w", encoding="utf-8", newline="\r\n") as f:
        f.write("\n".join(line.replace("tools/rooms/cut_room.py", "tools/rooms/cut_pixel_room.py")
                          for line in door_lines(room)) + "\n")
    sk.write_png(os.path.join(folder, "thumb.png"), thumbnail(room, layers))
    with open(os.path.join(folder, "layers.txt"), "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(layer_lines(layers, flat)) + "\n")
    written += ["doors.txt", "thumb.png", "layers.txt"]
    for name in ("sky", "far", "mid", "fore"):
        for f in layer_files_in(folder, name):
            if f not in written:
                os.remove(os.path.join(folder, f))
                print("  removed " + sk.shown(os.path.join(folder, f)))
    return written


def register(zone, room, written, folder, flat, n):
    """Content.mgcb's blocks for the room's files, and its two lines in ASSETS.md."""
    paint = room.paint
    sk.sync_mgcb("Rooms/%s/" % paint)
    mine = "Content/Rooms/%s/" % paint
    outputs = ["Content/Rooms/%s/%s" % (paint, f) for f in written]
    source_folder = sk.relative(folder) + "/"
    how = ("cut flat from the whole redraw (its layers are still to be painted): the mid layer, nothing sliding"
           if flat else "its sky, far, middle and foreground layers keyed off their magenta, each its own layer, "
           "sliding by its parallax about the room's middle")
    room_line = ("%s - built by tools/rooms/cut_pixel_room.py from the pixel-ready redraw of %s in %s (%s): %s; "
                 "%s fitted to the doors and floor as the original painting was (art/rooms/runtime-fit.json), "
                 "cut to %s 16:9 band, joined along the room, and made pixel art at 800 x 450 art pixels a screen "
                 "(a palette per layer, majority colour per cell, hard edges), with its painted floor; its 200x113 "
                 "thumbnail for the zone map and doors.txt; the pictures were %s" % (
                     ", ".join(outputs), room.name, zone.name, source_folder, how,
                     "its panel" if n == 1 else "its %d panels each" % n, "its" if n == 1 else "their", sk.SUPPLIED))
    sk.sync_assets([("## Rooms", lambda p: p.startswith(mine), room_line, None)])


def main():
    ap = argparse.ArgumentParser(description="Cut the pixel-ready room paintings into layered pixel-art rooms.")
    ap.add_argument("only", nargs="*", help="zones or zone/room (default: every room with its pictures)")
    ap.add_argument("--zones", default=os.path.join(sk.CONTENT, "Zones"))
    ap.add_argument("--sources", default=SOURCES)
    ap.add_argument("--out", default=sk.ROOMS)
    args = ap.parse_args()
    fits = json.load(open(FIT, encoding="utf-8"))["panels"]
    sk.check_picks(args.only, args.zones)
    into_content = os.path.normcase(os.path.abspath(args.out)) == os.path.normcase(os.path.abspath(sk.ROOMS))
    done, failed = 0, []
    for zone, room in sk.rooms(args.zones):
        if not sk.wanted(zone, room, args.only):
            continue
        folder = sk.room_folder(zone.id, room.id, args.sources)
        n = sk.panel_count(room.width)
        if not all(os.path.isfile(os.path.join(folder, "bg-%d.png" % i)) for i in range(1, n + 1)):
            continue
        print("Cutting %s/%s (%d wide, %d panel%s)..." % (zone.id, room.id, room.width, n, "" if n == 1 else "s"))
        try:
            layers, flat = cut(zone, room, folder, fits)
        except sk.SourceError as e:
            print("  " + str(e))
            failed.append("%s/%s" % (zone.id, room.id))
            continue
        written = write_room(zone, room, layers, flat, args.out)
        if into_content:
            register(zone, room, written, folder, flat, n)
        print("  %s" % ", ".join(written))
        done += 1
    print("cut_pixel_room: %d room%s cut%s." % (done, "" if done == 1 else "s",
                                                 "; failed: " + ", ".join(failed) if failed else ""))
    if failed:
        sys.exit(1)


if __name__ == "__main__":
    main()
