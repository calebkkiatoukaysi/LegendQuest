"""
The pieces every art generator for Legend Quest shares: an RGBA image as rows of pixels, the PNG
writer and reader, and a few colour helpers. The PNG code is copied from The Black Box's
generate_assets.py, so like that one it only needs CPython.

This isn't run on its own. The generate_ scripts and cut_supplied.py import it.
"""

import os
import struct
import zlib

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..")
OUT = os.path.join(ROOT, "Content")


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


def read_png(path):
    """Reads an 8-bit, non-interlaced RGB or RGBA PNG. Returns its width, height, how many bytes a
    pixel takes (3 or 4), and its rows, each one bytes in a bytearray.

    The Black Box's read_png, except a row stays as bytes instead of becoming a list per pixel. The
    pictures I make with ChatGPT are a million and a half pixels each, and a list per pixel ran out
    of patience long before it ran out of memory."""
    data = open(path, "rb").read()
    assert data[:8] == b"\x89PNG\r\n\x1a\n", path + " isn't a PNG"
    pos, idat = 8, b""
    while pos < len(data):
        ln, = struct.unpack(">I", data[pos:pos + 4])
        tag, body = data[pos + 4:pos + 8], data[pos + 8:pos + 8 + ln]
        pos += 12 + ln
        if tag == b"IHDR":
            w, h, depth, ctype, _, _, interlace = struct.unpack(">IIBBBBB", body)
        elif tag == b"IDAT":
            idat += body
        elif tag == b"IEND":
            break
    assert depth == 8 and interlace == 0 and ctype in (2, 6), path + " has to be 8-bit RGB or RGBA, not interlaced"
    ch = 3 if ctype == 2 else 4
    raw = zlib.decompress(idat)
    stride = w * ch
    rows, prev, p = [], bytearray(stride), 0
    for _ in range(h):
        f, line = raw[p], bytearray(raw[p + 1:p + 1 + stride])
        p += 1 + stride
        if f == 1:
            for i in range(ch, stride):
                line[i] = (line[i] + line[i - ch]) & 255
        elif f == 2:
            for i in range(stride):
                line[i] = (line[i] + prev[i]) & 255
        elif f == 3:
            for i in range(stride):
                line[i] = (line[i] + (((line[i - ch] if i >= ch else 0) + prev[i]) >> 1)) & 255
        elif f == 4:
            for i in range(stride):
                a = line[i - ch] if i >= ch else 0
                b = prev[i]
                c = prev[i - ch] if i >= ch else 0
                pa, pb, pc = abs(b - c), abs(a - c), abs(a + b - 2 * c)
                line[i] = (line[i] + (a if pa <= pb and pa <= pc else (b if pb <= pc else c))) & 255
        rows.append(line)
        prev = line
    return w, h, ch, rows


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
