"""
The pieces every art generator for Legend Quest shares: an RGBA image as rows of pixels, the PNG
writer, and a few colour helpers. The PNG code is copied from The Black Box's generate_assets.py,
so like that one it only needs CPython.

This isn't run on its own. generate_ui.py and generate_world.py import it.
"""

import math
import os
import struct
import zlib

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..")
OUT = os.path.join(ROOT, "Content")

#: 4x4 ordered dither, 0..15. Used to break light falloff into pixel-art bands instead of a smooth gradient.
BAYER = ((0, 8, 2, 10), (12, 4, 14, 6), (3, 11, 1, 9), (15, 7, 13, 5))


def new_image(w, h, fill=(0, 0, 0, 0)):
    return [[list(fill) for _ in range(w)] for _ in range(h)]


def put(img, x, y, rgba):
    """Alpha-composites rgba over the pixel at (x, y); out-of-bounds is ignored."""
    if y < 0 or y >= len(img) or x < 0 or x >= len(img[0]):
        return
    sr, sg, sb, sa = rgba
    if sa <= 0:
        return
    if sa >= 255:
        img[y][x] = [sr, sg, sb, 255]
        return
    dr, dg, db, da = img[y][x]
    a = sa / 255.0
    out_a = sa + da * (1 - a)
    if out_a <= 0:
        img[y][x] = [0, 0, 0, 0]
        return
    img[y][x] = [
        int(round((sr * sa + dr * da * (1 - a)) / out_a)),
        int(round((sg * sa + dg * da * (1 - a)) / out_a)),
        int(round((sb * sa + db * da * (1 - a)) / out_a)),
        int(round(out_a)),
    ]


def write_png(path, img):
    h = len(img)
    w = len(img[0])
    raw = bytearray()
    for row in img:
        raw.append(0)  # filter type 0 (None)
        for px in row:
            raw += bytes(px)

    def chunk(tag, data):
        body = tag + data
        return struct.pack(">I", len(data)) + body + struct.pack(">I", zlib.crc32(body) & 0xFFFFFFFF)

    header = struct.pack(">IIBBBBB", w, h, 8, 6, 0, 0, 0)  # 8-bit RGBA
    blob = (b"\x89PNG\r\n\x1a\n"
            + chunk(b"IHDR", header)
            + chunk(b"IDAT", zlib.compress(bytes(raw), 9))
            + chunk(b"IEND", b""))
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "wb") as f:
        f.write(blob)
    rel = os.path.relpath(path, ROOT).replace(os.sep, "/")
    print("  " + rel + "  (" + str(w) + "x" + str(h) + ")")


def clamp_byte(v):
    return max(0, min(255, int(round(v))))


def lerp_color(a, b, t):
    t = max(0.0, min(1.0, t))
    return tuple(int(round(a[i] + (b[i] - a[i]) * t)) for i in range(len(a)))


def tone(rgb, k):
    """Scales a colour by k and makes it an opaque RGBA."""
    return (clamp_byte(rgb[0] * k), clamp_byte(rgb[1] * k), clamp_byte(rgb[2] * k), 255)


def hash01(x, y, seed):
    """A repeatable 0..1 number for a pixel. Same input, same number, every run."""
    h = (x * 374761393 + y * 668265263 + seed * 2147483647) & 0xFFFFFFFF
    h = ((h ^ (h >> 13)) * 1274126177) & 0xFFFFFFFF
    return ((h ^ (h >> 16)) & 0xFFFF) / 65535.0


def value_noise(x, y, cell, seed):
    """Smooth noise: hash01 on a grid of `cell` pixels, blended between the corners."""
    gx, gy = x / cell, y / cell
    x0, y0 = int(math.floor(gx)), int(math.floor(gy))
    fx, fy = gx - x0, gy - y0
    sx, sy = fx * fx * (3 - 2 * fx), fy * fy * (3 - 2 * fy)
    a = hash01(x0, y0, seed)
    b = hash01(x0 + 1, y0, seed)
    c = hash01(x0, y0 + 1, seed)
    d = hash01(x0 + 1, y0 + 1, seed)
    return (a + (b - a) * sx) + ((c + (d - c) * sx) - (a + (b - a) * sx)) * sy
