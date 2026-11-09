"""
Puts one character's picture from ChatGPT into the game, in one go:

    python tools/import_character.py <png> <id> [--build standard|lean|big|stout|small] [--kind person|creature] [--force]
    python tools/import_character.py --remove <id>

The picture is the one Story/13-art-prompts.md asks for: one figure standing in three-quarter view
facing right, on a 1024x1536 canvas (1536x1024 for a wide creature), on a transparent background or
a flat, light one. This:

  - keeps its alpha if it has any. Otherwise it keys the background off: a smooth model of the
    background (so a soft vignette or faint noise doesn't matter) is fitted to the border, whatever
    the border reaches through colours close to it is background, and a soft matte along the
    outline works out each edge pixel's alpha from how far it sits between the background and the
    figure beside it, and takes the background back out of its colour, so there's no pale fringe;
  - trims to the figure and finds the feet (the lowest opaque row) and the head (down to the
    narrowest row of the neck under the hair, about a fifth of the way down a person);
  - scales it so a standard adult is FILE_HEIGHT tall from the top of the head to the feet, times
    the build's share (a creature by its size in Characters/Creatures.cs), with a Mitchell filter on
    premultiplied colour;
  - writes Content/Characters/Art/<id>.png and <id>.json (its size, feet, head box, build, kind and
    the size of the picture it came from), copies the picture to tools/source/characters/<id>.png,
    and rewrites its own sections of ASSETS.md and Content/Content.mgcb from what's imported;
  - saves shots/art/import/<id>-preview.png: the figure in a room at the game's scale facing right
    and mirrored, the cut at full size with its head box and feet marked, and the portrait crop.

The build and the kind come from the game's data when they're left out: a Stoneborn is stout, a
Hearthling or a child small, a Sylvan lean, and anything Characters/Creatures.cs gives a frame of
its own is a creature. The id has to be someone's there: an origin, a companion with an approval
meter, one of the Cast, an enemy, a creature or a townsfolk sheet, or a variant of one
(<id>.<variant>, as the prompts name them). An id that draws with another's picture
(Characters/ImportedArt.cs's SharedPictures) is refused, pointing at the one to import instead.
--remove takes an import out again: its files, the copies a build made of them, and its lines.

Only needs CPython and numpy. Nothing in it is random, so the same picture always comes out as the
same bytes.
"""

import argparse
import difflib
import glob
import json
import os
import re
import struct
import sys
import zlib

import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(HERE)
CONTENT = os.path.join(ROOT, "Content")
ART = os.path.join(CONTENT, "Characters", "Art")
SOURCE = os.path.join(ROOT, "tools", "source", "characters")
PREVIEWS = os.path.join(ROOT, "shots", "art", "import")
ASSETS = os.path.join(ROOT, "ASSETS.md")
MGCB = os.path.join(CONTENT, "Content.mgcb")

#: How tall a standard adult is in the files, head to feet, and in a room on the 1600x900 canvas:
#: ImportedArt.FileHeight and ImportedArt.RoomHeight, which the game scales by.
FILE_HEIGHT = 512
ROOM_HEIGHT = 196

#: Each build's height against a standard adult's.
BUILDS = {"small": 0.72, "stout": 0.82, "standard": 1.0, "lean": 1.04, "big": 1.08}

#: The builds the peoples have, and the townsfolk sheets that are children.
ANCESTRY_BUILDS = {"Stoneborn": "stout", "Hearthling": "small", "Sylvan": "lean"}
CHILD_SHEETS = ("child",)

#: The biggest a texture can be on the content pipeline's Reach profile, each way.
MAX_TEXTURE = 2048

#: The portrait crop, as ImportedFigure cuts it: a box round the middle of the head box, the head
#: box's longer side times FACE_REACH tall, starting FACE_LIFT of that side above it.
FACE_REACH = 1.4
FACE_LIFT = 0.1

#: The room the preview stands the figure in, where its floor is, and where the two figures go.
PREVIEW_ROOM = os.path.join(CONTENT, "Rooms", "docks", "harbourside")
PREVIEW_FLOOR = 740
PREVIEW_AT = (620, 980)
PREVIEW_PANEL = 600

#: The background: how far into the picture its model is fitted from the border, and the colour
#: distances (RGB, 0 to 1 a channel) that count as close to it.
RING = 6
NEAREST = 0.06
FARTHEST = 0.15

#: The smallest gap closed off inside the figure that's keyed out as background, as a share of
#: the figure's box: a hand on a hip leaves one far bigger, and an eye's white is far smaller.
HOLE_SHARE = 0.004

#: The marker lines round this tool's sections of ASSETS.md and Content.mgcb.
A_BEGIN = "<!-- imported characters: written by tools/import_character.py; don't edit between these two lines -->"
A_END = "<!-- end imported characters -->"
A_ANCHOR = "## Tiles and props"
M_BEGIN = "#------------ Characters/Art: written by tools/import_character.py ------------#"
M_END = "#--------------------------- end Characters/Art ----------------------------#"
CREDIT = "supplied by Caleb Kiatoukaysi, made in ChatGPT; cut by tools/import_character.py"
TEXTURE = ("/importer:TextureImporter", "/processor:TextureProcessor", "/processorParam:ColorKeyColor=255,0,255,255",
           "/processorParam:ColorKeyEnabled=False", "/processorParam:GenerateMipmaps=False",
           "/processorParam:PremultiplyAlpha=True", "/processorParam:ResizeToPowerOfTwo=False",
           "/processorParam:MakeSquare=False", "/processorParam:TextureFormat=Color",
           "/processorParam:Version=3.8.5.1")

ID = re.compile(r"[a-z0-9]+(?:-[a-z0-9]+)*$")


def rel(path):
    return os.path.relpath(path, ROOT).replace(os.sep, "/")


def fail(message):
    print("import_character: " + message, file=sys.stderr)
    sys.exit(1)


# --------------------------------------------------------------------------- #
# PNG files
# --------------------------------------------------------------------------- #

def unfilter(kinds, filt, bpp):
    """Undoes the PNG row filters on an H x stride array of bytes, bpp bytes a pixel. The filters
    that look left (Sub, Average, Paeth) are undone a diagonal at a time, so it's numpy's speed."""
    h, stride = filt.shape
    if np.all(kinds == 0):
        return filt.copy()
    units = stride // bpp
    f = filt[:, :units * bpp].reshape(h, units, bpp).astype(np.int64)
    out = np.zeros((h + 1, units + 1, bpp), np.int64)
    if np.all((kinds == 0) | (kinds == 2)):
        for y in range(h):
            out[y + 1, 1:] = (f[y] + (out[y, 1:] if kinds[y] == 2 else 0)) & 255
    else:
        for k in range(units + h - 1):
            ys = np.arange(max(0, k - units + 1), min(h, k + 1))
            xs = k - ys
            a, b, c = out[ys + 1, xs], out[ys, xs + 1], out[ys, xs]
            p = a + b - c
            pa, pb, pc = np.abs(p - a), np.abs(p - b), np.abs(p - c)
            paeth = np.where((pa <= pb) & (pa <= pc), a, np.where(pb <= pc, b, c))
            ft = kinds[ys][:, None]
            pred = np.where(ft == 1, a, np.where(ft == 2, b, np.where(ft == 3, (a + b) >> 1, np.where(ft == 4, paeth, 0))))
            out[ys + 1, xs + 1] = (f[ys, xs] + pred) & 255
    rows = np.zeros((h, stride), np.uint8)
    rows[:, :units * bpp] = out[1:, 1:].reshape(h, units * bpp)
    return rows


def read_png(path):
    """Reads a PNG (any colour type, 1 to 16 bits, not interlaced) into an H x W x 4 uint8 array,
    with whether it carries alpha of its own (an alpha channel or a tRNS chunk)."""
    with open(path, "rb") as f:
        data = f.read()
    if data[:8] != b"\x89PNG\r\n\x1a\n":
        fail(path + " isn't a PNG")
    pos, idat, palette, trns = 8, [], None, None
    w = h = depth = ctype = interlace = None
    while pos + 8 <= len(data):
        ln, = struct.unpack(">I", data[pos:pos + 4])
        tag, body = data[pos + 4:pos + 8], data[pos + 8:pos + 8 + ln]
        pos += 12 + ln
        if tag == b"IHDR":
            w, h, depth, ctype, _, _, interlace = struct.unpack(">IIBBBBB", body)
        elif tag == b"PLTE":
            palette = np.frombuffer(body, np.uint8).reshape(-1, 3)
        elif tag == b"tRNS":
            trns = body
        elif tag == b"IDAT":
            idat.append(body)
        elif tag == b"IEND":
            break
    channels = {0: 1, 2: 3, 3: 1, 4: 2, 6: 4}.get(ctype)
    if channels is None or interlace or depth not in (1, 2, 4, 8, 16) or depth < 8 and ctype not in (0, 3) \
            or depth == 16 and ctype == 3:
        fail(path + " is a kind of PNG this can't read (interlaced, or an odd bit depth); save it again as 8-bit RGBA")
    bpp = max(1, channels * depth // 8)
    stride = (w * channels * depth + 7) // 8
    raw = np.frombuffer(zlib.decompress(b"".join(idat)), np.uint8)[:h * (stride + 1)].reshape(h, stride + 1)
    rows = unfilter(raw[:, 0].astype(np.int64), raw[:, 1:], bpp)

    if depth == 16:
        samples = rows.reshape(h, w * channels, 2)[:, :, 0]
    elif depth == 8:
        samples = rows[:, :w * channels]
    else:
        bits = np.unpackbits(rows, axis=1)[:, :w * depth].reshape(h, w, depth).astype(np.int64)
        samples = (bits * (1 << np.arange(depth - 1, -1, -1))).sum(axis=2)
        if ctype == 0:
            samples = samples * 255 // ((1 << depth) - 1)
    samples = np.asarray(samples).reshape(h, w, channels).astype(np.uint8)

    rgba = np.full((h, w, 4), 255, np.uint8)
    own_alpha = ctype in (4, 6) or trns is not None
    if ctype == 3:
        index = samples[:, :, 0].astype(np.int64)
        rgba[:, :, :3] = palette[np.clip(index, 0, len(palette) - 1)]
        if trns is not None:
            alpha = np.full(256, 255, np.uint8)
            alpha[:len(trns)] = np.frombuffer(trns, np.uint8)
            rgba[:, :, 3] = alpha[index]
    elif ctype in (0, 4):
        rgba[:, :, :3] = samples[:, :, :1]
        if ctype == 4:
            rgba[:, :, 3] = samples[:, :, 1]
    else:
        rgba[:, :, :3] = samples[:, :, :3]
        if ctype == 6:
            rgba[:, :, 3] = samples[:, :, 3]
    if trns is not None and ctype in (0, 2):
        key = np.array(struct.unpack(">" + "H" * (len(trns) // 2), trns)) >> (8 if depth == 16 else 0)
        shown = samples[:, :, :len(key)].astype(np.int64)
        rgba[np.all(shown == key, axis=2), 3] = 0
    return rgba, own_alpha


def write_png(path, rgba):
    """Writes an H x W x 4 uint8 array as an 8-bit RGBA PNG: filter 0 on every row, zlib level 9,
    no other chunks, so the same pixels are always the same bytes."""
    a = np.ascontiguousarray(rgba, dtype=np.uint8)
    h, w = a.shape[:2]
    raw = np.zeros((h, w * 4 + 1), np.uint8)
    raw[:, 1:] = a.reshape(h, w * 4)

    def chunk(tag, body):
        return struct.pack(">I", len(body)) + tag + body + struct.pack(">I", zlib.crc32(tag + body) & 0xFFFFFFFF)

    blob = (b"\x89PNG\r\n\x1a\n" + chunk(b"IHDR", struct.pack(">IIBBBBB", w, h, 8, 6, 0, 0, 0))
            + chunk(b"IDAT", zlib.compress(raw.tobytes(), 9)) + chunk(b"IEND", b""))
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "wb") as f:
        f.write(blob)


# --------------------------------------------------------------------------- #
# The game's data
# --------------------------------------------------------------------------- #

def source_text(path):
    with open(os.path.join(ROOT, path), encoding="utf-8-sig") as f:
        return f.read()


def blocks(text, pattern):
    """Each match of a pattern, with the text from it to the next match: one table row and what's
    written in it."""
    found = list(re.finditer(pattern, text))
    for i, m in enumerate(found):
        yield m, text[m.start():found[i + 1].start() if i + 1 < len(found) else len(text)]


def ancestry_build(block):
    m = re.search(r"Ancestry = Ancestry\.(\w+)", block)
    return ANCESTRY_BUILDS.get(m.group(1)) if m else None


def shared_pictures():
    """ImportedArt.SharedPictures: the ids that draw with another id's picture."""
    text = source_text("Characters/ImportedArt.cs")
    table = re.search(r"SharedPictures = new\(\)\s*\{(.*?)\};", text, re.S)
    return dict(re.findall(r'\["([^"]+)"\]\s*=\s*"([^"]+)"', table.group(1))) if table else {}


def game_data():
    """Everyone the game knows by id, read off its tables, as {id: {"what", "name", "build",
    "height"}}: what they are, what they're called, the build their look gives them (or None), and
    for a creature in a frame of its own, its height against a person's (or None)."""
    known = {}

    def add(id_, what, name, build=None, height=None):
        entry = known.setdefault(id_, {"what": what, "name": name, "build": None, "height": None})
        entry["build"] = entry["build"] or build
        entry["height"] = entry["height"] or height

    person = float(re.search(r"Figure: new Vector2\(([\d.]+)f, ([\d.]+)f\)", source_text("Characters/WalkerShape.cs")).group(2))
    creatures = {}
    text = source_text("Characters/Creatures.cs")
    for m in re.finditer(r'new\("([^"]+)",\s*"([^"]+)",\s*(WalkerShape\.Person|Standing\(\s*\d+,\s*\d+,\s*figure:\s*new\([\d.]+f,\s*([\d.]+)f\))', text):
        creatures[m.group(1)] = float(m.group(4)) / person if m.group(4) else None
        add(m.group(1), "creature", m.group(2), height=creatures[m.group(1)])

    text = source_text("Characters/Origins.cs")
    consts = dict(re.findall(r'const string (\w+) = "([^"]+)";', text))
    for m, block in blocks(text, r'Id = (\w+|"[^"]+"), Name = ("[^"]*"|string\.Empty)'):
        id_ = m.group(1).strip('"') if m.group(1).startswith('"') else consts.get(m.group(1))
        name = "" if m.group(2) == "string.Empty" else m.group(2).strip('"')
        add(id_, "origin", name or "the Soul-Bound", ancestry_build(block))

    cast = {}
    for m, block in blocks(source_text("Story/Cast.cs"), r'new\("([^"]+)",\s*"([^"]+)",\s*(null|"([^"]+)")'):
        build = "small" if m.group(4) in CHILD_SHEETS else ancestry_build(block)
        cast[m.group(1)] = build
        add(m.group(1), "cast", m.group(2), build)

    for m, block in blocks(source_text("Combat/EnemyCatalog.cs"), r'Id = "([^"]+)", Name = "([^"]+)"'):
        drawn = re.search(r'(Creature|Cast) = "([^"]+)"', block)
        build = height = None
        if drawn and drawn.group(1) == "Cast":
            build = cast.get(drawn.group(2))
        elif drawn:
            height = creatures.get(drawn.group(2))
        add(m.group(1), "enemy", m.group(2), build, height)
    for companion in re.findall(r'new\("affinity-[^"]+",\s*-?\d+,\s*-?\d+,\s*-?\d+,\s*"([^"]+)"\)', source_text("Story/Meters.cs")):
        add(companion, "companion", companion.capitalize())

    sheets = re.search(r"All \{ get; \} = new\[\]\s*\{(.*?)\};", source_text("Characters/Townsfolk.cs"), re.S)
    for sheet in re.findall(r'"([^"]+)"', sheets.group(1)):
        add(sheet, "townsfolk sheet", "the " + sheet.replace("-", " "), "small" if sheet in CHILD_SHEETS else None)
    for zone in sorted(glob.glob(os.path.join(CONTENT, "Zones", "*.txt"))):
        with open(zone, encoding="utf-8") as f:
            for sheet in re.findall(r"\bsheet ([a-z0-9-]+)", f.read()):
                add(sheet, "townsfolk sheet", "the " + sheet.replace("-", " "))
    return known


def close_matches(wanted, ids):
    near = difflib.get_close_matches(wanted, ids, n=8, cutoff=0.6)
    near += [i for i in sorted(ids) if (wanted in i or i in wanted) and i not in near]
    return near[:10]


def resolve(id_, known, shared):
    """Who an id is in the game's data, checked: its entry, and its variant's name if it's one."""
    base, _, variant = id_.partition(".")
    if not ID.match(base) or variant and not ID.match(variant):
        fail("%s isn't an id: ids are lower case words joined by hyphens, with a variant after a dot (red-gull.b)" % id_)
    if base in shared:
        fail("%s has no picture of its own: it draws with %s's (Story/13-art-prompts.md says no separate prompt). "
             "Import %s instead." % (base, shared[base], shared[base] + ("." + variant if variant else "")))
    if base not in known:
        near = close_matches(base, list(known))
        fail("nobody in the game is called %s.%s" % (base, " Did you mean: " + ", ".join(near) + "?" if near else ""))
    return known[base], variant


def named(id_, known):
    """How ASSETS.md names whoever an imported id is."""
    base, _, variant = id_.partition(".")
    name = known.get(base, {}).get("name", base)
    return name + (" (" + variant.replace("-", " ") + ")" if variant else "")


# --------------------------------------------------------------------------- #
# Masks
# --------------------------------------------------------------------------- #

def components(mask):
    """The 4-connected pieces of a boolean mask: an int32 image (0 off the mask, 1 and up a piece)
    and how many pieces. Runs along each row are joined to the overlapping runs of the row above,
    so it's a union-find over runs, not over pixels."""
    h, w = mask.shape
    padded = np.zeros((h, w + 2), np.int8)
    padded[:, 1:-1] = mask
    step = np.diff(padded, axis=1)
    rows, starts = np.nonzero(step == 1)
    _, ends = np.nonzero(step == -1)
    rows, starts, ends = rows.tolist(), starts.tolist(), ends.tolist()
    parent = list(range(len(rows)))

    def find(i):
        while parent[i] != i:
            parent[i] = parent[parent[i]]
            i = parent[i]
        return i

    first = np.searchsorted(np.array(rows, np.int64), np.arange(h + 1)).tolist()
    for y in range(1, h):
        i, j = first[y - 1], first[y]
        while i < first[y] and j < first[y + 1]:
            if starts[i] < ends[j] and starts[j] < ends[i]:
                a, b = find(i), find(j)
                if a != b:
                    parent[max(a, b)] = min(a, b)
            if ends[i] < ends[j]:
                i += 1
            elif ends[j] < ends[i]:
                j += 1
            else:
                i, j = i + 1, j + 1
    labels, number = {}, np.zeros((h, w), np.int32)
    for k in range(len(rows)):
        root = find(k)
        if root not in labels:
            labels[root] = len(labels) + 1
        number[rows[k], starts[k]:ends[k]] = labels[root]
    return number, len(labels)


def grow(mask, steps=1):
    """A mask grown by a pixel each step, diagonals too."""
    out = mask.copy()
    for _ in range(steps):
        tall = out.copy()
        tall[1:] |= out[:-1]
        tall[:-1] |= out[1:]
        out = tall.copy()
        out[:, 1:] |= tall[:, :-1]
        out[:, :-1] |= tall[:, 1:]
    return out


def box_sum(a, r):
    """The sum over the (2r + 1)-square window round every pixel, nothing counted off the edge."""
    pad = [(r + 1, r), (r + 1, r)] + [(0, 0)] * (a.ndim - 2)
    s = np.pad(a.astype(np.float64), pad).cumsum(axis=0).cumsum(axis=1)
    n = 2 * r + 1
    return s[n:, n:] - s[:-n, n:] - s[n:, :-n] + s[:-n, :-n]


def local_mean(colour, weight, reaches):
    """The weighted mean colour round every pixel, from the first window in `reaches` that has any
    weight in it, and where none has, NaN."""
    mean = np.full(colour.shape, np.nan)
    for r in reaches:
        total = box_sum(weight, r)
        sums = box_sum(colour * weight[:, :, None], r)
        fill = np.isnan(mean[:, :, 0]) & (total > 1e-9)
        mean[fill] = sums[fill] / total[fill][:, None]
    return mean


def smoothstep(lo, hi, x):
    t = np.clip((x - lo) / (hi - lo), 0.0, 1.0)
    return t * t * (3.0 - 2.0 * t)


def luminance(rgb):
    return rgb @ np.array([0.2126, 0.7152, 0.0722])


# --------------------------------------------------------------------------- #
# Keying the background off
# --------------------------------------------------------------------------- #

def fit_background(rgb, where, step):
    """A quadratic surface a channel fitted to the pixels where `where` is set (every step-th way),
    fitted twice, the second time without what the first got badly wrong (a figure touching the
    border). Returns the model of the whole picture and how noisy the background is round it."""
    h, w = where.shape
    ys, xs = np.nonzero(where[::step, ::step])
    ys, xs = ys * step, xs * step
    u, v = xs / max(1, w - 1) * 2.0 - 1.0, ys / max(1, h - 1) * 2.0 - 1.0
    basis = np.stack([np.ones_like(u), u, v, u * u, u * v, v * v], axis=1)
    samples = rgb[ys, xs]
    keep = np.ones(len(ys), bool)
    for _ in range(2):
        coef = np.linalg.lstsq(basis[keep], samples[keep], rcond=None)[0]
        miss = np.sqrt(((basis @ coef - samples) ** 2).sum(axis=1))
        spread = 1.4826 * np.median(np.abs(miss - np.median(miss))) + np.median(miss)
        keep = miss <= max(3.0 * spread, 0.02)
    gu = np.linspace(-1.0, 1.0, w)[None, :]
    gv = np.linspace(-1.0, 1.0, h)[:, None]
    model = np.empty((h, w, 3))
    for c in range(3):
        k = coef[:, c]
        model[:, :, c] = k[0] + k[1] * gu + k[2] * gv + k[3] * gu * gu + k[4] * gu * gv + k[5] * gv * gv
    return np.clip(model, 0.0, 1.0), spread


def holes(passable, near, rgb, reach, figure):
    """The pieces of background the border can't reach (between an arm and the body): closed off
    by a dark outline, background-coloured all through, and at least HOLE_SHARE of the figure's
    box, so an eye's white or a white collar inside the lines is never taken for one."""
    labels, count = components(passable)
    found = np.zeros_like(passable)
    edge = np.unique(np.concatenate([labels[0], labels[-1], labels[:, 0], labels[:, -1]]))
    area = np.bincount(labels.ravel(), minlength=count + 1)
    close = np.bincount(labels.ravel(), weights=(near < reach * 0.6).ravel(), minlength=count + 1)
    light = luminance(rgb)
    rows, cols = np.nonzero(figure.any(axis=1))[0], np.nonzero(figure.any(axis=0))[0]
    smallest = max(200, HOLE_SHARE * (rows[-1] - rows[0] + 1) * (cols[-1] - cols[0] + 1)) if len(rows) else np.inf
    for k in np.nonzero((area >= smallest) & (close >= 0.95 * np.maximum(area, 1)))[0]:
        if k == 0 or k in edge:
            continue
        ys, xs = np.nonzero(labels == k)
        y0, y1, x0, x1 = max(0, ys.min() - 3), ys.max() + 4, max(0, xs.min() - 3), xs.max() + 4
        piece = labels[y0:y1, x0:x1] == k
        ring = grow(piece, 2) & ~piece
        if np.median(light[y0:y1, x0:x1][ring]) < 0.45:
            found[y0:y1, x0:x1] |= piece
    return found


def key_out(rgba):
    """Takes a flat, light background off a picture that has no alpha of its own. Returns the
    straight colour (H x W x 3, 0 to 1), the alpha, and a word on what the background was."""
    rgb = rgba[:, :, :3].astype(np.float64) / 255.0
    h, w = rgb.shape[:2]
    ring = np.zeros((h, w), bool)
    ring[:RING], ring[-RING:], ring[:, :RING], ring[:, -RING:] = True, True, True, True
    model, spread = fit_background(rgb, ring, 2)
    reach = float(np.clip(NEAREST + 4.0 * spread, NEAREST, FARTHEST))

    background = ring
    for _ in range(2):
        near = np.sqrt(((rgb - model) ** 2).sum(axis=2))
        passable = near < reach
        labels, _ = components(passable)
        edge = np.unique(np.concatenate([labels[0], labels[-1], labels[:, 0], labels[:, -1]]))
        background = np.isin(labels, edge[edge > 0])
        model, _ = fit_background(rgb, background, 3)
    near = np.sqrt(((rgb - model) ** 2).sum(axis=2))
    gaps = holes(near < reach, near, rgb, reach, ~background)
    background |= gaps
    inside = ~background

    # The soft matte: each pixel either side of the outline is a mix of the background behind it
    # and the figure's colour just inside the edge, and its alpha is how far along that line it is.
    edge_in = inside & grow(background, 4) & ~grow(background, 1)
    figure = local_mean(rgb, edge_in.astype(np.float64), (3, 6))
    figure = np.where(np.isnan(figure), local_mean(rgb, inside.astype(np.float64), (6, 12)), figure)
    figure = np.where(np.isnan(figure), rgb, figure)
    band = (inside & grow(background, 2)) | (background & grow(inside, 1))
    gap = figure - model
    span = (gap ** 2).sum(axis=2)
    along = ((rgb - model) * gap).sum(axis=2) / np.maximum(span, 1e-9)
    alpha = inside.astype(np.float64)
    mixed = band & (span > 0.08 ** 2)
    alpha[mixed] = np.clip(along[mixed], 0.0, 1.0)
    alpha[band & background & (alpha < 0.04)] = 0.0
    alpha[band & inside & (alpha > 0.97)] = 1.0

    # The colour with the background taken back out, eased to the figure's own edge colour where
    # there's too little of it to trust: that's what keeps a pale fringe off.
    solved = np.clip((rgb - (1.0 - alpha[:, :, None]) * model) / np.maximum(alpha, 1e-3)[:, :, None], 0.0, 1.0)
    trust = smoothstep(0.35, 0.8, alpha)[:, :, None]
    colour = np.where(band[:, :, None], trust * solved + (1.0 - trust) * figure, rgb)
    tone = luminance(model).mean()
    word = "%s background keyed off" % ("light" if tone >= 0.55 else "dark (not the light one the prompts ask for)")
    pieces = components(gaps)[1]
    if pieces:
        word += ", with %d enclosed gap%s (check the preview)" % (pieces, "" if pieces == 1 else "s")
    if spread > 0.05:
        word += ", but it isn't flat: check the preview"
    return colour, alpha, word


def defringe(rgba):
    """Keeps a picture's own alpha, and only takes the pale halo off its soft edge pixels where
    they're much lighter than the figure just inside them."""
    rgb = rgba[:, :, :3].astype(np.float64) / 255.0
    alpha = rgba[:, :, 3].astype(np.float64) / 255.0
    solid = alpha >= 0.95
    figure = local_mean(rgb, solid.astype(np.float64), (3, 6))
    soft = (alpha > 0.0) & ~solid & ~np.isnan(figure[:, :, 0])
    pale = soft & (luminance(rgb) > luminance(np.nan_to_num(figure)) + 0.25)
    trust = smoothstep(0.6, 0.95, alpha)[:, :, None]
    rgb = np.where(pale[:, :, None], trust * rgb + (1.0 - trust) * np.nan_to_num(figure), rgb)
    return rgb, alpha, "its own alpha kept"


def clean_specks(alpha):
    """Drops the specks the key left lying about: pieces too small and faint to be part of anything."""
    labels, count = components(alpha > 0.02)
    if count <= 1:
        return alpha
    area = np.bincount(labels.ravel(), minlength=count + 1)
    weight = np.bincount(labels.ravel(), weights=alpha.ravel(), minlength=count + 1)
    biggest = int(np.argmax(area[1:])) + 1
    drop = (area < 16) & (weight < 12)
    drop[0], drop[biggest] = False, False
    return np.where(drop[labels], 0.0, alpha)


# --------------------------------------------------------------------------- #
# Measuring the figure
# --------------------------------------------------------------------------- #

def run_at(row, x, reach):
    """The opaque run along a row through column x (or the nearest one within reach of it), as
    (start, end), or None."""
    w = len(row)
    x = int(np.clip(x, 0, w - 1))
    if not row[x]:
        lo, hi = max(0, x - reach), min(w, x + reach + 1)
        hits = np.nonzero(row[lo:hi])[0]
        if len(hits) == 0:
            return None
        x = lo + int(hits[np.argmin(np.abs(hits + lo - x))])
    start = x
    while start > 0 and row[start - 1]:
        start -= 1
    end = x + 1
    while end < w and row[end]:
        end += 1
    return start, end


def person_head(solid, top, feet):
    """A standing person's head: (head top, its box (x, y, w, h)), or None if it can't be found.
    The middle of the head is the column with the most figure in the top fifth; the top is the
    first row where the figure through it is a head's width, not a bow's or a staff's; and the box
    goes down to the narrowest row of the neck, a tenth to three tenths of the way down."""
    tall = feet - top
    band = solid[top:top + max(1, int(0.22 * tall))].sum(axis=0).astype(np.float64)
    smooth = max(1, int(0.03 * tall))
    centre = int(np.argmax(np.convolve(band, np.ones(2 * smooth + 1), "same")))
    reach = max(2, int(0.03 * tall))

    head_top = None
    for y in range(top, top + int(0.3 * tall)):
        run = run_at(solid[y], centre, reach)
        if run and run[1] - run[0] >= 0.04 * tall:
            head_top = y
            break
    if head_top is None:
        return None
    body = feet - head_top
    widths, spans, x = [], [], centre
    for y in range(head_top, head_top + int(0.3 * body)):
        run = run_at(solid[y], x, reach)
        if run is None:
            widths.append(0)
            spans.append((x, x))
            continue
        widths.append(run[1] - run[0])
        spans.append(run)
        x = int(np.clip((run[0] + run[1]) // 2, x - 2, x + 2))
    widths = np.array(widths, np.float64)
    smooth = np.convolve(widths, np.ones(5) / 5.0, "same")
    lo, hi = int(0.1 * body), int(0.3 * body)
    neck = lo + int(np.argmin(smooth[lo:hi]))
    if smooth[neck] >= 0.85 * np.median(widths[int(0.06 * body):neck + 1]):
        neck = int(0.19 * body)
    left = min(s for s, _ in spans[:neck + 1])
    right = max(e for _, e in spans[:neck + 1])
    left, right = max(left, int(centre - 0.15 * body)), min(right, int(centre + 0.15 * body))
    return head_top, (left, head_top, right - left, neck)


def creature_head(solid, box):
    """A creature's head, facing right: the figure in the front third of it, over its feet."""
    x, y, w, h = box
    front = solid[y:y + int(0.75 * h), x + int(0.62 * w):x + w]
    ys, xs = np.nonzero(front)
    if len(ys) == 0:
        return x, y, w, h
    return x + int(0.62 * w) + int(xs.min()), y + int(ys.min()), int(xs.max() - xs.min()) + 1, int(ys.max() - ys.min()) + 1


def measure(alpha, creature):
    """The figure's box (x, y, w, h), its feet (x, the row under the lowest opaque one), the top of
    its head, and its head box, all in the picture's pixels."""
    seen = alpha >= 0.03
    solid = alpha >= 0.5
    rows, cols = np.nonzero(seen.any(axis=1))[0], np.nonzero(seen.any(axis=0))[0]
    if len(rows) == 0 or not solid.any():
        fail("there's no figure left once the background's off: is it the right picture?")
    box = (int(cols[0]), int(rows[0]), int(cols[-1] - cols[0] + 1), int(rows[-1] - rows[0] + 1))
    feet_row = int(np.nonzero(solid.any(axis=1))[0][-1])
    tall = feet_row + 1 - box[1]
    band = alpha[max(box[1], feet_row + 1 - max(2, int(0.05 * tall))):feet_row + 1]
    weight = band.sum(axis=0)
    feet_x = float((weight * np.arange(len(weight))).sum() / max(weight.sum(), 1e-9)) + 0.5

    found = None if creature and box[2] > box[3] else person_head(solid, box[1], feet_row + 1)
    if found:
        head_top, head = found
    else:
        head_top, head = box[1], creature_head(solid, box)
    return box, (feet_x, feet_row + 1), head_top, head


# --------------------------------------------------------------------------- #
# Resampling
# --------------------------------------------------------------------------- #

def mitchell(x):
    """The Mitchell-Netravali filter, B = C = 1/3: sharp, with hardly any ringing at the outline."""
    x = np.abs(x)
    b = c = 1.0 / 3.0
    near = ((12 - 9 * b - 6 * c) * x ** 3 + (-18 + 12 * b + 6 * c) * x ** 2 + (6 - 2 * b)) / 6.0
    far = ((-b - 6 * c) * x ** 3 + (6 * b + 30 * c) * x ** 2 + (-12 * b - 48 * c) * x + (8 * b + 24 * c)) / 6.0
    return np.where(x < 1, near, np.where(x < 2, far, 0.0))


def taps(origin, length, count, size):
    """Which source pixels each of `count` new pixels across `length` source pixels from `origin`
    takes, and how much of each: the filter stretched to the step when it's shrinking. Off the
    picture's `size` a tap's weight is kept but it reads nothing, as if it were transparent."""
    scale = count / length
    support = 2.0 * max(1.0, 1.0 / scale)
    centres = origin + (np.arange(count) + 0.5) / scale - 0.5
    first = np.floor(centres - support).astype(np.int64) + 1
    index = first[:, None] + np.arange(int(np.ceil(2 * support)) + 1)[None, :]
    weight = mitchell((index - centres[:, None]) * min(1.0, scale))
    weight /= weight.sum(axis=1, keepdims=True)
    outside = (index < 0) | (index >= size)
    return np.clip(index, 0, size - 1), np.where(outside, 0.0, weight)


def resample(premultiplied, box, size):
    """A box of a premultiplied H x W x 4 picture (x, y, w, h, in pixels) resampled to size (w, h).
    Tap by tap in a fixed order, so it's the same to the last bit every run."""
    x, y, w, h = box
    index, weight = taps(y, h, size[1], premultiplied.shape[0])
    tall = np.zeros((size[1], premultiplied.shape[1], 4))
    for k in range(index.shape[1]):
        tall += weight[:, k, None, None] * premultiplied[index[:, k]]
    index, weight = taps(x, w, size[0], premultiplied.shape[1])
    out = np.zeros((size[1], size[0], 4))
    for k in range(index.shape[1]):
        out += weight[None, :, k, None] * tall[:, index[:, k]]
    alpha = np.clip(out[:, :, 3], 0.0, 1.0)
    out[:, :, :3] = np.clip(out[:, :, :3], 0.0, alpha[:, :, None])
    out[:, :, 3] = alpha
    return out


def straight(premultiplied):
    """A premultiplied 0-to-1 picture as straight 8-bit RGBA, with no colour under nothing."""
    alpha = premultiplied[:, :, 3]
    rgb = premultiplied[:, :, :3] / np.maximum(alpha, 1e-9)[:, :, None]
    out = np.rint(np.clip(np.concatenate([rgb, alpha[:, :, None]], axis=2), 0.0, 1.0) * 255.0).astype(np.uint8)
    out[out[:, :, 3] == 0] = 0
    return out


# --------------------------------------------------------------------------- #
# The preview
# --------------------------------------------------------------------------- #

def over(canvas, picture, x, y, opacity=1.0):
    """Lays a straight 8-bit RGBA picture over an H x W x 3 0-to-1 canvas, its top left at (x, y)."""
    h, w = picture.shape[:2]
    x0, y0, x1, y1 = max(0, x), max(0, y), min(canvas.shape[1], x + w), min(canvas.shape[0], y + h)
    if x0 >= x1 or y0 >= y1:
        return
    piece = picture[y0 - y:y1 - y, x0 - x:x1 - x].astype(np.float64) / 255.0
    a = piece[:, :, 3:] * opacity
    canvas[y0:y1, x0:x1] = canvas[y0:y1, x0:x1] * (1.0 - a) + piece[:, :, :3] * a


def room_backdrop(width, height):
    """The room the preview's figures stand in: its sky, far and mid layers, the left screen of it."""
    canvas = np.zeros((height, width, 3))
    canvas[:] = np.linspace(0.55, 0.85, height)[:, None, None] * np.array([0.75, 0.82, 1.0])
    for layer in ("sky", "far", "mid"):
        path = os.path.join(PREVIEW_ROOM, layer + ".png")
        if os.path.isfile(path):
            over(canvas, read_png(path)[0], 0, 0)
    return canvas


def shadow(canvas, x, y, width):
    """The squashed soft shadow the game draws under the feet."""
    rx, ry = width / 2.0, width * 0.15
    ys, xs = np.mgrid[0:canvas.shape[0], 0:canvas.shape[1]]
    t = np.clip(1.0 - ((xs - x) / rx) ** 2 - ((ys - y) / ry) ** 2, 0.0, 1.0) ** 1.5 * 0.5
    canvas *= 1.0 - t[:, :, None]


def portrait_box(head):
    """The portrait's square on the picture, as ImportedFigure cuts it for a square well."""
    x, y, w, h = head
    side = max(w, h) * FACE_REACH
    return x + w / 2.0 - side / 2.0, y - max(w, h) * FACE_LIFT, side, side


def rectangle(canvas, box, colour, thick=2):
    x, y, w, h = (int(round(v)) for v in box)
    for t in range(thick):
        canvas[max(0, y + t), max(0, x):max(0, x + w)] = colour
        canvas[max(0, y + h - 1 - t), max(0, x):max(0, x + w)] = colour
        canvas[max(0, y):max(0, y + h), max(0, x + t)] = colour
        canvas[max(0, y):max(0, y + h), max(0, x + w - 1 - t)] = colour


def preview(path, cut, premultiplied, info, creature):
    """Writes the preview: the room with the figure at the game's scale facing right and mirrored,
    and a panel with the cut at full size on a checkerboard (its head box gold, its feet red), the
    dialogue's portrait at its size and at twice it."""
    width, height = 1600 + PREVIEW_PANEL, 900
    canvas = np.zeros((height, width, 3))
    canvas[:, :1600] = room_backdrop(1600, height)
    canvas[:, 1600:] = (0.09, 0.08, 0.11)

    w, h = info["size"]
    room = (max(1, round(w * ROOM_HEIGHT / FILE_HEIGHT)), max(1, round(h * ROOM_HEIGHT / FILE_HEIGHT)))
    small = straight(resample(premultiplied, (0, 0, w, h), room))
    fx, fy = info["feet"][0] * room[0] / w, info["feet"][1] * room[1] / h
    spread = room[0] * 0.85 if creature else fy * 0.306
    for at, picture, x in ((PREVIEW_AT[0], small, fx), (PREVIEW_AT[1], small[:, ::-1], room[0] - fx)):
        shadow(canvas, at, PREVIEW_FLOOR, spread)
        over(canvas, picture, int(round(at - x)), int(round(PREVIEW_FLOOR - fy)))

    fit = min(1.0, (PREVIEW_PANEL - 40) / w, 560 / h)
    shown = (max(1, round(w * fit)), max(1, round(h * fit)))
    left, top = 1600 + (PREVIEW_PANEL - shown[0]) // 2, 20
    ys, xs = np.mgrid[0:shown[1], 0:shown[0]]
    canvas[top:top + shown[1], left:left + shown[0]] = np.where((((xs // 16) + (ys // 16)) % 2 == 0)[:, :, None], 0.80, 0.62)
    over(canvas, cut if fit == 1.0 else straight(resample(premultiplied, (0, 0, w, h), shown)), left, top)
    hx, hy, hw, hh = info["head"]
    rectangle(canvas, (left + hx * fit, top + hy * fit, hw * fit, hh * fit), (0.95, 0.78, 0.30))
    feet = (left + int(round(info["feet"][0] * fit)), top + int(round(info["feet"][1] * fit)) - 1)
    canvas[max(0, feet[1] - 8):feet[1] + 9, feet[0] - 1:feet[0] + 2] = (0.9, 0.2, 0.2)
    canvas[feet[1] - 1:feet[1] + 2, feet[0] - 8:feet[0] + 9] = (0.9, 0.2, 0.2)

    face = portrait_box(info["head"])
    for size, x in ((120, 1600 + 60), (240, 1600 + 220)):
        y = height - 20 - size
        canvas[y:y + size, x:x + size] = (0.20, 0.18, 0.24)
        over(canvas, straight(resample(premultiplied, face, (size, size))), x, y)
    write_png(path, np.concatenate([np.rint(np.clip(canvas, 0, 1) * 255).astype(np.uint8),
                                    np.full((height, width, 1), 255, np.uint8)], axis=2))


# --------------------------------------------------------------------------- #
# ASSETS.md and Content.mgcb
# --------------------------------------------------------------------------- #

def imported():
    """Every id imported so far, by its .json in Content/Characters/Art."""
    return sorted(os.path.basename(p)[:-len(".json")] for p in glob.glob(os.path.join(ART, "*.json")))


def assets_section(ids, known):
    if not ids:
        return []
    lines = [A_BEGIN, "## Characters (imported art)", "",
             "The characters' pictures made in ChatGPT from Story/13-art-prompts.md, each cut by `tools/import_character.py` "
             "from the picture it keeps in tools/source/characters: keyed off its background, trimmed to the figure, scaled "
             "by its build and saved with its feet and its head box beside it. The game draws one in place of the paperdoll "
             "or the sheet for whoever has its id (Characters/ImportedArt.cs).", ""]
    for id_ in ids:
        with open(os.path.join(ART, id_ + ".json"), encoding="utf-8") as f:
            info = json.load(f)
        lines += ["Content/Characters/Art/%s.png, Content/Characters/Art/%s.json - %s, standing in three-quarter view facing "
                  "right (%s %s), cut from tools/source/characters/%s.png; %s"
                  % (id_, id_, named(id_, known), info["build"], info["kind"], id_, CREDIT), ""]
    return lines + [A_END]


def mgcb_section(ids):
    if not ids:
        return []
    lines = [M_BEGIN, ""]
    for id_ in ids:
        png, data = "Characters/Art/%s.png" % id_, "Characters/Art/%s.json" % id_
        lines += ["#begin " + png] + list(TEXTURE) + ["/build:" + png, "", "#begin " + data, "/copy:" + data, ""]
    return lines + [M_END]


def splice(text, begin, end, section, anchor):
    """The text with the lines between the markers (and the markers) replaced by the section, or
    the section put before the anchor (or at the end) if the markers aren't there yet. An empty
    section takes the markers out."""
    lines = text.split("\n")
    if begin in lines and end in lines:
        i, j = lines.index(begin), lines.index(end)
        after = lines[j + 1:]
        if not section and after and after[0] == "":
            after = after[1:]
        return "\n".join(lines[:i] + section + after)
    if not section:
        return text
    if anchor and anchor in lines:
        k = lines.index(anchor)
        return "\n".join(lines[:k] + section + [""] + lines[k:])
    while lines and lines[-1] == "":
        lines.pop()
    return "\n".join(lines + [""] + section + [""])


def rewrite(path, begin, end, section, anchor):
    with open(path, "rb") as f:
        raw = f.read().decode("utf-8")
    crlf = "\r\n" in raw
    text = splice(raw.replace("\r\n", "\n"), begin, end, section, anchor)
    out = text.replace("\n", "\r\n") if crlf else text
    if out != raw:
        with open(path, "wb") as f:
            f.write(out.encode("utf-8"))


def register(known):
    ids = imported()
    rewrite(ASSETS, A_BEGIN, A_END, assets_section(ids, known), A_ANCHOR)
    rewrite(MGCB, M_BEGIN, M_END, mgcb_section(ids), None)


# --------------------------------------------------------------------------- #
# Importing
# --------------------------------------------------------------------------- #

def import_character(png, id_, build, kind, force):
    known, shared = game_data(), shared_pictures()
    entry, variant = resolve(id_, known, shared)
    out_png, out_json = os.path.join(ART, id_ + ".png"), os.path.join(ART, id_ + ".json")
    source = os.path.join(SOURCE, id_ + ".png")
    if not os.path.isfile(png):
        fail("there's no picture at " + png)
    if os.path.isfile(out_json) and not force:
        fail("%s is imported already; --force replaces it" % id_)
    with open(png, "rb") as f:
        original = f.read()
    same_file = os.path.isfile(source) and os.path.samefile(png, source)
    if os.path.isfile(source) and not same_file and not force:
        with open(source, "rb") as f:
            if f.read() != original:
                fail("%s holds a different picture already; --force replaces it" % rel(source))

    creature = kind == "creature" if kind else entry["height"] is not None
    if build:
        factor, sized = BUILDS[build], "--build " + build
    elif creature and entry["height"]:
        build, factor, sized = "standard", entry["height"], "its size in Characters/Creatures.cs"
    else:
        build = entry["build"] or "standard"
        factor, sized = BUILDS[build], "its look" if entry["build"] else "the standard build"

    rgba, own_alpha = read_png(png)
    used = own_alpha and (rgba[:, :, 3] < 250).mean() > 0.005
    colour, alpha, keyed = defringe(rgba) if used else key_out(rgba)
    alpha = clean_specks(alpha)
    box, feet, head_top, head = measure(alpha, creature)

    target = FILE_HEIGHT * factor
    scale = target / max(1.0, feet[1] - head_top)
    size = (max(1, int(round(box[2] * scale))), max(1, int(round(box[3] * scale))))
    if max(size) > MAX_TEXTURE:
        fail("%s would come out %dx%d, more than one texture (%d) holds; check its --build or its size" % (id_, size[0], size[1], MAX_TEXTURE))
    premultiplied = np.concatenate([colour * alpha[:, :, None], alpha[:, :, None]], axis=2)
    cut_premultiplied = resample(premultiplied, box, size)
    cut = straight(cut_premultiplied)
    sx, sy = size[0] / box[2], size[1] / box[3]

    def across(x):
        return int(round((x - box[0]) * sx))

    def down(y):
        return int(round((y - box[1]) * sy))

    info = {
        "id": id_,
        "build": build,
        "kind": "creature" if creature else "person",
        "size": [size[0], size[1]],
        "feet": [round((feet[0] - box[0]) * sx, 2), down(feet[1])],
        "head": [across(head[0]), down(head[1]), max(1, across(head[0] + head[2]) - across(head[0])),
                 max(1, down(head[1] + head[3]) - down(head[1]))],
        "source": [int(rgba.shape[1]), int(rgba.shape[0])],
    }

    write_png(out_png, cut)
    with open(out_json, "wb") as f:
        f.write(("{\r\n" + ",\r\n".join('  "%s": %s' % (k, json.dumps(v)) for k, v in info.items()) + "\r\n}\r\n").encode("utf-8"))
    if not same_file:
        os.makedirs(SOURCE, exist_ok=True)
        with open(source, "wb") as f:
            f.write(original)
    register(known)
    shot = os.path.join(PREVIEWS, id_ + "-preview.png")
    preview(shot, cut, cut_premultiplied, info, creature)

    also = sorted(k for k, v in shared.items() if v == id_)
    print("import_character: %s (%s, %s%s) -> %s %dx%d, %s %s sized by %s, feet (%g, %d), head %s, %s%s; registered in ASSETS.md and Content.mgcb"
          % (id_, named(id_, known), entry["what"], ", a variant nothing draws yet" if variant else "", rel(out_png), size[0], size[1],
             build, info["kind"], sized, info["feet"][0], info["feet"][1], tuple(info["head"]), keyed,
             "; also draws " + ", ".join(also) if also else ""))
    print("  preview: " + rel(shot))


def remove(id_):
    """Takes an import out: its picture and .json, the copies a build made of them, its source
    copy, its preview and its lines."""
    gone = [p for p in (os.path.join(ART, id_ + ".png"), os.path.join(ART, id_ + ".json"),
                        os.path.join(SOURCE, id_ + ".png"), os.path.join(PREVIEWS, id_ + "-preview.png")) if os.path.isfile(p)]
    for pattern in ("bin/**/Content/Characters/Art/%s.*", "Content/bin/**/Characters/Art/%s.*", "Content/obj/**/Characters/Art/%s.*"):
        gone += [p for p in glob.glob(os.path.join(ROOT, pattern % id_), recursive=True) if os.path.isfile(p)]
    if not gone:
        fail("%s isn't imported" % id_)
    for path in gone:
        os.remove(path)
    register(game_data())
    print("import_character: removed %s (%d files) and its lines in ASSETS.md and Content.mgcb" % (id_, len(gone)))


def main(argv):
    parser = argparse.ArgumentParser(description="Puts a character's picture from ChatGPT into the game.")
    parser.add_argument("png", nargs="?", help="the picture")
    parser.add_argument("id", nargs="?", help="whose it is: an origin, cast, enemy, creature or townsfolk sheet id")
    parser.add_argument("--build", choices=sorted(BUILDS), help="how tall they stand (from their look if left out)")
    parser.add_argument("--kind", choices=("person", "creature"), help="a person or a creature (from the game's data if left out)")
    parser.add_argument("--force", action="store_true", help="replace an import that's there already")
    parser.add_argument("--remove", metavar="ID", help="take an import out again")
    args = parser.parse_args(argv)
    if args.remove:
        remove(args.remove)
    elif args.png and args.id:
        import_character(args.png, args.id, args.build, args.kind, args.force)
    else:
        parser.error("give a picture and an id, or --remove and an id")


if __name__ == "__main__":
    main(sys.argv[1:])
