"""
Cuts the owner's floor textures into the tilesets' sheets. Every room's floor is a tile strip drawn
by World/Tilemap.cs, the course's tilemap, and it stays one: a painted room shows the strip
through its floor band (cut_room.py), so these textures are the floor people walk on.

    python tools/rooms/cut_tiles.py                     # every tileset with a texture, and every list
    python tools/rooms/cut_tiles.py questeria interior
    python tools/rooms/cut_tiles.py questeria --sources <folder> --out <folder>

A texture is tools/source/tiles/<tileset>/<tile>.png: 1024 x 1024 and seamless (it tiles with
itself on all four sides), named after one of the tileset's tiles in World/TilesetCatalog.cs.
Each is resampled (Lanczos-3, premultiplied, wrapping round its edges so it stays seamless) to the
tileset's TileTexels (64), or for a block tile to its whole block (a 2 x 2 block's texture becomes
128 x 128, cut into its four cells), and laid on the sheet in the order Tilemap.FrameOf counts:
for every variant the same picture; for every frame of a moving tile (the water, the tar) the
picture with its middle drifting while a band round its edge stays put, so tiles a frame apart still
meet. A detail tile, laid over the ground (the catalog's "Detail" tiles, or one the rooms only use
in their detail rows), is taken off its backdrop the way a foreground is: its own transparency, or
a flat #FF00FF. An edged tile's four edges, and every tile with no texture, are kept as the sheet
already has them.

It writes:

- Content/Tiles/<tileset>.png, only for a tileset with at least one texture (one with none is left
  byte for byte), with its Content.mgcb block, its ASSETS.md line (which keeps what it said about
  the tiles kept) and the textures' line, supplied by Caleb Kiatoukaysi;
- shots/art/room-templates/tiles-<tileset>.txt for every tileset it's asked about: exactly which
  texture files it needs (the tiles a room's floor uses) and which it can have (the rest of its
  sheet), what each one has to be, and which rooms use it. Those are for the prompts.

It stops, naming the file, on a texture that's the wrong size, not a PNG or for no tile in its
tileset. Deterministic: the same bytes every run. Only needs CPython and numpy.
"""

import argparse
import os
import sys

import numpy as np

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import sourcekit as sk  # noqa: E402
from roomkit import CONTENT  # noqa: E402

DRIFT_BAND = 0.25
"""The share of a moving tile's side, from each edge, over which its drifting middle fades back to
the still picture."""


def textures(tileset, folder):
    """The tileset's textures in a folder: {tile name: path}. Raises SourceError on a file that's no
    tile of it."""
    found = {}
    names = [t["name"] for t in tileset.tiles]
    for f in sk.listed(folder):
        stem = f[:-4] if f.lower().endswith(".png") else None
        if stem not in tileset.ids:
            raise sk.SourceError("%s is no tile of the %s tileset: its tiles are %s, each a .png." % (
                sk.shown(os.path.join(folder, f)), tileset.id, ", ".join(names)))
        found[stem] = os.path.join(folder, f)
    return found


def load(path):
    img = sk.read_png(path)
    if img.shape[:2] != (sk.TILE_PICTURE, sk.TILE_PICTURE):
        raise sk.SourceError("%s is %dx%d; a floor texture is %dx%d, seamless." % (
            sk.shown(path), img.shape[1], img.shape[0], sk.TILE_PICTURE, sk.TILE_PICTURE))
    return img


def drift(cell, step, frames):
    """Frame `step` of a moving tile made from one picture: its middle rolled along by step/frames
    of the tile (and half that down), faded back to the picture over DRIFT_BAND of the side from
    each edge, so every frame meets its neighbours the way the picture does."""
    if step == 0:
        return cell
    n = cell.shape[0]
    rolled = np.roll(cell, (step * n // (2 * frames), step * n // frames), axis=(0, 1))
    d = np.minimum(np.arange(n), n - 1 - np.arange(n)) / (DRIFT_BAND * n)
    w = sk.smoothstep(d)
    m = (w[:, None] * w[None, :])[..., None]
    return cell * (1.0 - m) + rolled * m


def cut_sheet(tileset, found, usage):
    """The tileset's sheet with its textures laid on: uint8 RGBA."""
    old = sk.read_png(tileset.sheet_path)
    k = tileset.texels
    if old.shape[1] != sk.COLUMNS * k or old.shape[0] % k:
        raise sk.SourceError("%s is %dx%d, not %d tiles of %d texels across: it isn't the sheet TilesetCatalog says." % (
            sk.shown(tileset.sheet_path), old.shape[1], old.shape[0], sk.COLUMNS, k))
    rows = -(-tileset.frame_count // sk.COLUMNS)
    sheet = np.zeros((max(rows * k, old.shape[0]), sk.COLUMNS * k, 4), np.uint8)
    sheet[:old.shape[0]] = old
    for name, path in sorted(found.items()):
        t = tileset.tile(name)
        b = t["block"]
        img = load(path)
        detail = sk.tile_kind(tileset, t, usage) == "detail"
        src = sk.key_out(img) if detail else sk.premultiply(np.concatenate(
            [img[..., :3], np.full(img.shape[:2] + (1,), 255, np.uint8)], axis=2))
        whole = sk.resize(src, b * k, b * k, wrap=True)
        for f in range(t["frames"]):
            picture = sk.unpremultiply(drift(whole, f, t["frames"]))
            for v in range(t["variants"]):
                for cy in range(b):
                    for cx in range(b):
                        i = tileset.cell(name, v, f, cx, cy)
                        r, c = divmod(i, sk.COLUMNS)
                        sheet[r * k:(r + 1) * k, c * k:(c + 1) * k] = picture[cy * k:(cy + 1) * k, cx * k:(cx + 1) * k]
    return sheet


def describe(tileset, t, kind):
    """What a tile's texture has to be, for the list."""
    parts = []
    if kind == "detail":
        parts.append("laid over the ground: the thing itself on a transparent background (or a flat #FF00FF), seen from straight above")
    elif kind == "walls":
        parts.append("a wall seen from the front (or its top from above), opaque and seamless")
    else:
        parts.append("ground seen from straight above, opaque and seamless on all four sides")
    if t["block"] > 1:
        parts.append("one picture for a %d x %d block of tiles (%d x %d texels on the sheet)" % (
            t["block"], t["block"], t["block"] * tileset.texels, t["block"] * tileset.texels))
    if t["frames"] > 1:
        parts.append("moves: its %d frames are made from the one picture" % t["frames"])
    if t["edged"]:
        parts.append("its edges, where it meets other ground, stay as the sheet has them")
    if t["solid"]:
        parts.append("nobody walks on it")
    return "; ".join(parts)


def tile_list(tileset, usage, folder):
    """The text of tiles-<tileset>.txt."""
    used = usage.get(tileset.id, {})
    need = [t for t in tileset.tiles if t["name"] in used]
    can = [t for t in tileset.tiles if t["name"] not in used]
    rel = sk.relative(folder)
    lines = [
        "The %s tileset's floor textures (World/TilesetCatalog.cs: %s)" % (tileset.id, tileset.summary),
        "",
        "Save each one as %s/<file>, a 1024 x 1024 PNG that tiles seamlessly with itself on every side," % rel,
        "lit from the top left, calmer and lower in contrast than the rooms' paintings so the characters read",
        "against it. Then run: python tools/rooms/cut_tiles.py %s" % tileset.id,
        "Each becomes one tile of %d x %d texels (16 world pixels; 64 screen pixels at the game's scale), so" % (tileset.texels, tileset.texels),
        "keep its pattern big and simple: a cobble, plank or slab a quarter to a whole of the picture across.",
        "A tile with no texture keeps the one the sheet has now.",
        "",
        "Needed: %d file%s (a room's floor uses each)" % (len(need), "" if len(need) == 1 else "s"),
        "",
    ]

    def entry(t):
        kind = sk.tile_kind(tileset, t, usage)
        out = ["%s.png" % t["name"], "    %s" % describe(tileset, t, kind)]
        u = used.get(t["name"])
        if u:
            for how in ("ground", "detail"):
                if u[how]:
                    out.append("    %s in: %s" % ("the ground" if how == "ground" else "the detail over it",
                                                  ", ".join(sorted(u[how]))))
        return out

    for t in need:
        lines += entry(t)
    lines += ["", "Optional: %d file%s (on the sheet, but no room's floor uses it yet)" % (len(can), "" if len(can) == 1 else "s"), ""]
    for t in can:
        lines += entry(t)
    return lines


def register(tileset, found, folder):
    """Content.mgcb's block for the sheet, and its two lines in ASSETS.md."""
    sheet = "Content/%s.png" % tileset.texture
    sk.sync_mgcb("%s.png" % tileset.texture)
    lines = sk._read_lines(sk.ASSETS)
    i = sk.find_line(lines, lambda p: p == sheet)
    before = ""
    if i is not None:
        said = lines[i].split(" - ", 1)[1]
        marker = "; every other tile as it was: "
        before = said.split(marker, 1)[1] if marker in said else said
    names = sorted(found)
    sources = [sk.relative(found[n]) for n in names]
    rel = sk.relative(folder)
    sheet_line = ("%s - built by tools/rooms/cut_tiles.py: the %s tileset's %s tile%s cut from the owner's seamless "
                  "textures in %s/, each resampled to %d texels a tile (a block's to its whole block, a moving tile's "
                  "frames drifting from it) in the order TilesetCatalog lists; the pictures were %s; every other tile "
                  "as it was: %s" % (sheet, tileset.id, ", ".join(names), "" if len(names) == 1 else "s", rel,
                                     tileset.texels, sk.SUPPLIED, before or "kept from the sheet"))
    source_line = ("%s - floor textures for the %s tileset, each 1024x1024 and seamless, made in ChatGPT, %s; "
                   "tools/rooms/cut_tiles.py cuts them into %s" % (", ".join(sources), tileset.id, sk.SUPPLIED, sheet))
    sk.sync_assets([
        ("## Tiles and props", lambda p: p == sheet, sheet_line, None),
        ("## Tiles and props", lambda p: p.startswith(rel + "/"), source_line, lambda p: p == sheet),
    ])


def main():
    ap = argparse.ArgumentParser(description="Cut the owner's floor textures into the tilesets' sheets, and list what each needs.")
    ap.add_argument("only", nargs="*", help="tilesets (default: every one in World/TilesetCatalog.cs)")
    ap.add_argument("--sources", default=sk.TILE_SOURCES, help="where <tileset>/<tile>.png are (default tools/source/tiles)")
    ap.add_argument("--out", default=os.path.join(CONTENT, "Tiles"), help="where <tileset>.png goes (default Content/Tiles; anywhere else, nothing is registered)")
    ap.add_argument("--lists", default=sk.TEMPLATES, help="where tiles-<tileset>.txt go (default shots/art/room-templates)")
    args = ap.parse_args()
    catalog = sk.tilesets()
    bad = [t for t in args.only if t not in catalog]
    if bad:
        sys.exit("No tileset called %s; World/TilesetCatalog.cs has %s." % (", ".join(bad), ", ".join(catalog)))
    usage = sk.tile_usage()
    into_content = os.path.normcase(os.path.abspath(args.out)) == os.path.normcase(os.path.abspath(os.path.join(CONTENT, "Tiles")))
    os.makedirs(args.lists, exist_ok=True)
    cut, failed = [], []
    for tid, tileset in catalog.items():
        if args.only and tid not in args.only:
            continue
        folder = os.path.join(args.sources, tid)
        path = os.path.join(args.lists, "tiles-%s.txt" % tid)
        with open(path, "w", encoding="utf-8", newline="\r\n") as f:
            f.write("\n".join(tile_list(tileset, usage, os.path.join(sk.TILE_SOURCES, tid))) + "\n")
        print("  " + sk.shown(path))
        try:
            found = textures(tileset, folder)
            if not found:
                continue
            print("Cutting the %s tileset: %s..." % (tid, ", ".join(sorted(found))))
            sheet = cut_sheet(tileset, found, usage)
        except sk.SourceError as e:
            print("  " + str(e))
            failed.append(tid)
            continue
        sk.write_png(os.path.join(args.out, os.path.basename(tileset.texture) + ".png"), sheet)
        if into_content:
            register(tileset, found, folder)
        cut.append(tid)
    print("cut_tiles: %d tileset%s cut%s." % (len(cut), "" if len(cut) == 1 else "s",
                                               "; failed: " + ", ".join(failed) if failed else ""))
    if failed:
        sys.exit(1)


if __name__ == "__main__":
    main()
