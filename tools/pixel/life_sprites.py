"""
Turns the pixel-ready life sheets (docs/pixel-art-brief.md §7.4) into the rooms' moving pixel sprites.

    python tools/pixel/life_sprites.py

Today that's the gull: life-1.png, the Gull's Wager's deck sheet (tools/source/pixel/rooms/gulls-wager/deck/,
painted in ChatGPT on the 4 x 3 life-sheet grid), has the same gull in three wing positions in cells 7, 8 and 9,
the same size and in the same place in each. This keys them off their magenta, cuts the box all three share,
and shrinks it to pixel art GULL_WIDTH art pixels across by majority colour (dark colours weighted, so the
outline survives), on one palette for all three frames, with a one-pixel deep brown-violet outline round the
outside. It writes Content/Rooms/Life/gull.png, the three frames side by side, flying right (World/Rooms/
LifeLoops.cs flips it to fly left and beats the wings up, level, down, level), and its line in ASSETS.md.
tools/rooms/paint_life.py paints the rest of the life pictures. Deterministic. Needs numpy and scipy.
"""

import os
import sys

import numpy as np
from scipy.cluster.vq import kmeans2
from scipy.spatial import cKDTree

ROOT = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", ".."))
sys.path.insert(0, os.path.join(ROOT, "tools", "rooms"))
import sourcekit as sk  # noqa: E402

SHEET = os.path.join(ROOT, "tools", "source", "pixel", "rooms", "gulls-wager", "deck", "life-1.png")
OUT = os.path.join(ROOT, "Content", "Rooms", "Life", "gull.png")

#: The sheet's grid, and the cells (counting from 1, left to right, top row first) of the gull's wingbeat.
GRID = (4, 3)
GULL_CELLS = (7, 8, 9)
#: How wide a frame is in art pixels (LifeLoops.GullWidth world pixels, two art pixels to one).
GULL_WIDTH = 32
COLOURS = 12
DARK_WEIGHT = 1.8
DARK_LUMA = 80
OUTLINE = np.array([42, 31, 61], np.float64)
"""#2a1f3d, the brief's deep brown-violet, mixed half and half with the colour it borders."""


def cell(sheet, n):
    """Cell n of the sheet, keyed: premultiplied float RGBA."""
    cols, rows = GRID
    h, w = sheet.shape[0] // rows, sheet.shape[1] // cols
    r, c = divmod(n - 1, cols)
    return sk.key_out(sheet[r * h:(r + 1) * h, c * w:(c + 1) * w])


def shrink(frames, width):
    """The frames (premultiplied, the same size) cut to the box they share and shrunk to `width`
    art pixels across by majority colour on one palette. Returns uint8 RGBA frames."""
    solid = [f[..., 3] >= 128 for f in frames]
    ys, xs = np.nonzero(np.logical_or.reduce(solid))
    top, bottom, left, right = ys.min(), ys.max() + 1, xs.min(), xs.max() + 1
    step = (right - left) / width
    height = int(np.ceil((bottom - top) / step))
    colours = [sk.unpremultiply(f[top:bottom, left:right]) for f in frames]
    pixels = np.concatenate([c[..., :3][s[top:bottom, left:right]] for c, s in zip(colours, solid)]).astype(np.float64)
    rng = np.random.default_rng(1)
    pal, _ = kmeans2(pixels[rng.choice(len(pixels), min(len(pixels), 40000), replace=False)], COLOURS, iter=20,
                     minit="++", seed=rng)
    pal = np.unique(np.rint(np.clip(pal, 0, 255)), axis=0)
    tree = cKDTree(pal)
    luma = pal @ np.array([0.299, 0.587, 0.114])
    weight = np.where(luma < DARK_LUMA, DARK_WEIGHT, 1.0)
    out = []
    for c, s in zip(colours, solid):
        s = s[top:bottom, left:right]
        _, index = tree.query(c[..., :3].reshape(-1, 3).astype(np.float64))
        index = index.reshape(s.shape)
        index[~s] = -1
        art = np.zeros((height, width, 4), np.uint8)
        for y in range(height):
            for x in range(width):
                block = index[int(y * step):max(int(y * step) + 1, int((y + 1) * step)),
                              int(x * step):max(int(x * step) + 1, int((x + 1) * step))].ravel()
                votes = {}
                for k in block:
                    votes[k] = votes.get(k, 0.0) + (1.0 if k < 0 else weight[k])
                best = max(sorted(votes), key=lambda k: votes[k])
                if best >= 0:
                    art[y, x, :3] = pal[best]
                    art[y, x, 3] = 255
        out.append(art)
    return out


def outline(art):
    """A one-pixel outline outside the silhouette, the brief's brown-violet mixed with what it borders."""
    a = art[..., 3] > 0
    p = np.pad(a, 1)
    rgb = np.pad(art[..., :3].astype(np.float64), ((1, 1), (1, 1), (0, 0)))
    edge = ~a & (p[:-2, 1:-1] | p[2:, 1:-1] | p[1:-1, :-2] | p[1:-1, 2:])
    out = np.pad(art, ((1, 1), (1, 1), (0, 0)))
    for y, x in zip(*np.nonzero(edge)):
        near = [rgb[y + 1 + dy, x + 1 + dx] for dy, dx in ((-1, 0), (1, 0), (0, -1), (0, 1)) if p[y + 1 + dy, x + 1 + dx]]
        out[y + 1, x + 1, :3] = np.rint((OUTLINE + np.mean(near, axis=0) * 0.35) / 1.35).astype(np.uint8)
        out[y + 1, x + 1, 3] = 255
    return out[1:-1, 1:-1]


def main():
    sheet = sk.read_png(SHEET)
    frames = shrink([cell(sheet, n) for n in GULL_CELLS], GULL_WIDTH - 2)
    frames = [np.pad(f, ((1, 1), (1, 1), (0, 0))) for f in frames]
    for f in frames:
        f[...] = outline(f)
    sk.write_png(OUT, np.concatenate(frames, axis=1))
    rel = "Content/Rooms/Life/gull.png"
    line = (rel + " - a gull's wingbeat in pixel art, three frames (wings up, level and down) side by side, made by "
            "tools/pixel/life_sprites.py from cells 7 to 9 of tools/source/pixel/rooms/gulls-wager/deck/life-1.png, "
            "the deck's life sheet " + sk.SUPPLIED + ", made in ChatGPT: keyed, shrunk to 32 art pixels across by "
            "majority colour on one 12-colour palette, and outlined")
    sk.sync_assets([("## Rooms", lambda p: p == rel, line, None)])
    print("life_sprites: wrote %s (%d x %d)" % (rel, frames[0].shape[1] * len(frames), frames[0].shape[0]))


if __name__ == "__main__":
    main()
