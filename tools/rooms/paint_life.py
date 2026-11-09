"""
The rooms' life loops, painted (12-rooms §3.2): the pictures World/Rooms/LifeLoops.cs and
Components/TorchSprite.cs draw the moving things in a room with, in the rooms' illustrated style,
into Content/Rooms/Life:

    clouds.png      six clouds side by side, 528 x 240 each (132 x 60 world pixels at 4 texels each)
    gull.png        a gull's two wingbeats, 64 x 40 each, flying right (flipped to fly left)
    flag.png        eight frames of a pennant waving, 64 x 40 each; the pole's top at (6, 8)
    flag-pole.png   a slice of the pole, 6 texels across, stretched down to the ground
    flame.png       a torch's flame flickering, six frames of 48 x 80 (12 x 20 art pixels at 4)
    candle.png      a candle's, the same at half the size: six frames of 24 x 40
    smoke.png, mote.png, ash.png, sparkle.png, leaf.png
                    the particles' pictures, four times the size of the title's (ParticleSystem.Repaint),
                    white so the system's colour tints them

    python tools/rooms/paint_life.py                      # into Content/Rooms/Life
    python tools/rooms/paint_life.py --preview shots/art/rooms   # and life-sheet.png there

paint_all.py runs it as a shared part (paint()). Deterministic. Only needs CPython and numpy.
"""

import argparse
import math
import os
import sys

import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
import paint as P  # noqa: E402
import roomkit  # noqa: E402
from paint import Layer, Path, Piece, hash01, tones  # noqa: E402
from vector import write_png  # noqa: E402

OUT = os.path.join(roomkit.CONTENT, "Rooms", "Life")

CLOUD = (528, 240, 6)
GULL = (64, 40, 2)
FLAG = (64, 40, 8)
FLAG_TOP = (6, 8)
POLE = (6, 4)
FLAME = (48, 80, 6)
CANDLE = (24, 40, 6)


def strip(frames):
    """Frames (each H x W x 4 uint8) side by side."""
    return np.concatenate(frames, axis=1)


def clouds():
    """Six painted clouds, day colours (the room's tint makes them dusk or night), each in its cell
    with room round it to drift."""
    w, h, n = CLOUD
    cells = []
    for i in range(n):
        layer = Layer(w, h)
        size = 0.95 + 0.3 * hash01(i, 1, 901)
        P.cloud(layer, w * 0.40, h * 0.70, size, "day", seed=900 + 17 * i)
        cells.append(layer.rgba())
    return strip(cells)


def gull():
    """A gull, white with grey-tipped wings, side on and flying right: wings up in a V, then down
    in a shallow arch. Each wing is a tapering stroke, the far one a shade darker."""
    w, h, n = GULL
    frames = []
    white = tones("#f4f3ef")
    grey = tones("#8e94aa")
    for f in range(n):
        p = Piece(0, 0, w, h, ss=4, pad=0)
        cx, cy = w * 0.5, h * 0.55
        lift = -1 if f == 0 else 1
        for far in (True, False):
            dx = -3.0 if far else 1.0
            k = 0.85 if far else 1.0
            root = (cx + dx, cy - 0.5)
            elbow = (cx + dx - 8 * k, cy + lift * (9 if f == 0 else 2) * k - (2 if f else 0))
            tip = (cx + dx - 19 * k, cy + lift * (15 if f == 0 else 8) * k)
            wing = Path.smooth([root, elbow, tip])
            t = white
            p.stroke(wing, lambda u: 6.0 * (1 - u) ** 0.7 + 1.0, t, "line")
            p.stroke(wing, lambda u: 4.4 * (1 - u) ** 0.7, t, "shadow" if far else "base")
            tipw = Path.smooth([elbow, tip]) if f == 0 else Path.smooth([((elbow[0] + tip[0]) / 2, (elbow[1] + tip[1]) / 2), tip])
            p.stroke(tipw, lambda u: 3.0 * (1 - u) + 0.6, grey, "shadow" if far else "base")
            if not far:
                body = Path.ellipse(cx + 2, cy + 1, 10.5, 4.4, rot=-6)
                bm = p.fill(body, white, "base")
                p.tone(p.below(bm, cy + 2.4), white, "shadow")
                p.edge(body, white, 1.0, "line")
                head = Path.circle(cx + 11.5, cy - 1.6, 3.6)
                p.fill(head, white, "base")
                p.edge(head, white, 1.0, "line")
                p.fill(Path.polygon([(cx + 14.5, cy - 2.4), (cx + 20, cy - 1.2), (cx + 14.5, cy - 0.2)]), tones("#e9b54c"), "base")
                p.fill(Path.circle(cx + 12.8, cy - 2.6, 0.9), tones("#24182a"), "base")
                tail = Path.polygon([(cx - 7, cy), (cx - 14, cy - 2), (cx - 13, cy + 3), (cx - 7, cy + 3)])
                p.fill(tail, grey, "light")
                p.edge(tail, grey, 1.0)
        L = Layer(w, h)
        L.place(p, reach=0.9)
        frames.append(L.rgba())
    return strip(frames)


def flag():
    """A pink pennant with a gold hem on a gold-knobbed pole, in eight frames of one wave running
    along it, so it loops."""
    w, h, n = FLAG
    frames = []
    cloth = tones("cloth.pink")
    hem = tones("gold")
    tx, ty = FLAG_TOP
    for f in range(n):
        p = Piece(0, 0, w, h, ss=4, pad=0)
        phase = 2 * math.pi * f / n
        length = w - tx - 6
        top, bot = [], []
        for i in range(17):
            u = i / 16.0
            x = tx + 1 + length * u
            wave = math.sin(phase - u * 5.0) * (1.5 + 4.0 * u)
            depth = 22 * (1 - u) + 1.5
            top.append((x, ty + 1 + wave))
            bot.append((x, ty + 1 + depth + wave * 1.15))
        shape = Path.polygon(top + bot[::-1])
        m = p.fill(shape, cloth, "base")
        # the folds: where the wave turns away from the light it's in shadow
        for i in range(16):
            u = (i + 0.5) / 16.0
            slope = math.cos(phase - u * 5.0)
            if slope > 0.35:
                x0 = tx + 1 + length * i / 16.0
                p.fill(Path.rect(x0, 0, length / 16.0 + 0.6, h), cloth, "shadow", clip=m, alpha=0.9)
            elif slope < -0.55:
                x0 = tx + 1 + length * i / 16.0
                p.fill(Path.rect(x0, 0, length / 16.0 + 0.6, h), cloth, "light", clip=m, alpha=0.8)
        p.stroke(Path.polyline(top), 2.4, hem, "base", clip=m)
        p.edge(shape, cloth, 1.2)
        knob = Path.circle(tx, ty - 3.5, 3.6)
        p.fill(Path.rect(tx - 1.6, ty - 2, 3.2, h - ty + 2), tones("wood.dark"), "base")
        km = p.fill(knob, hem, "base")
        p.tone(km - km.shift(1, 1), hem, "light")
        p.edge(knob, hem, 1.0)
        L = Layer(w, h)
        L.place(p, reach=1.2)
        frames.append(L.rgba())
    return strip(frames)


def flag_pole():
    """A slice of the pole, stretched down to the ground: dark wood, lit on its left."""
    w, h = POLE
    t = tones("wood.dark")
    img = np.zeros((h, w, 4), np.uint8)
    cols = [t["line"], t["light"], t["base"], t["base"], t["shadow"], t["line"]]
    for x in range(w):
        img[:, x, :3] = cols[x]
        img[:, x, 3] = 255
    return img


def _flame(w, h, f, n, seed, small=False):
    """One frame of a flame on a frame w x h: a teardrop of orange round a yellow heart, its tip
    swaying and its height breathing from frame to frame."""
    p = Piece(0, 0, w, h, ss=4, pad=0)
    k = w / 48.0
    ph = 2 * math.pi * f / n
    sway = math.sin(ph) * 4.0 * k + math.sin(2 * ph + 1.3) * 1.5 * k
    tall = (54 + 6 * math.sin(ph * 2 + 0.7) + 3 * hash01(f, 1, seed)) * k
    base_y = h - 2 * k
    cx = w / 2

    def drop(scale, dy=0.0):
        hh = tall * scale
        ww = 15 * k * scale
        return Path().move(cx - ww, base_y - ww * 0.9 - dy).cubic(cx - ww, base_y - hh * 0.55 - dy, cx + sway * 0.4, base_y - hh * 0.8 - dy,
                                                                  cx + sway, base_y - hh - dy) \
            .cubic(cx + sway * 0.5 + ww * 0.6, base_y - hh * 0.7 - dy, cx + ww * 1.05, base_y - hh * 0.45 - dy, cx + ww, base_y - ww * 0.9 - dy) \
            .arc(cx, base_y - ww * 0.9 - dy, ww, 0, 180).close()

    outer = tones("#ff7a3a")
    mid = tones("#ffb04a")
    heart = tones("#fff1b8")
    p.fill(drop(1.0), outer, "base")
    p.fill(drop(0.72, 2 * k), mid, "base")
    p.fill(drop(0.42, 3 * k), heart, "base")
    if not small:
        for i in range(2):
            ex = cx + (hash01(f, i + 2, seed) - 0.5) * 14 * k + sway * 0.8
            ey = base_y - tall * (1.05 + 0.15 * i) - 4 * k * ((f + i) % 3)
            p.fill(Path.circle(ex, ey, 1.6 * k), mid, "light")
    L = Layer(w, h)
    L.place(p, reach=0)
    return L.rgba()


def flame():
    w, h, n = FLAME
    return strip([_flame(w, h, f, n, 911) for f in range(n)])


def candle():
    w, h, n = CANDLE
    return strip([_flame(w, h, f, n, 913, small=True) for f in range(n)])


def _radial(size, core, falloff=2.0):
    """A soft white disc: full in the middle `core` of its radius, falling off to nothing."""
    y, x = np.mgrid[0:size, 0:size]
    d = np.sqrt((x + 0.5 - size / 2) ** 2 + (y + 0.5 - size / 2) ** 2) / (size / 2)
    a = np.clip((1 - d) / max(1e-6, 1 - core), 0, 1) ** falloff
    img = np.zeros((size, size, 4), np.uint8)
    img[..., :3] = 255
    img[..., 3] = np.rint(np.clip(a, 0, 1) * 255).astype(np.uint8)
    return img


def smoke():
    """A puff of smoke: three soft lobes, white (the room's smoke colour tints it)."""
    s = 32
    L = Layer(s, s)
    for (x, y, r) in ((13, 18, 9), (20, 15, 8), (16, 11, 7)):
        L.soft_ellipse(x, y, r, r, "#ffffff", 0.75, hard=0.45)
    return L.rgba()


def mote():
    return _radial(32, 0.25, 1.6)


def ash():
    """A flake of ash: a small ragged leaf of grey-white with a hard edge."""
    s = 32
    p = Piece(0, 0, s, s, ss=4, pad=0)
    t = tones("#f2f0f4")
    shape = Path.smooth([(9, 15), (14, 9), (21, 10), (24, 16), (19, 22), (12, 21)], closed=True)
    m = p.fill(shape, t, "base")
    p.tone(m - m.shift(-1, -1.2), t, "shadow")
    L = Layer(s, s)
    L.place(p, reach=0)
    return L.rgba()


def sparkle():
    """A glint: a four-pointed star with a soft glow round it, white."""
    s = 32
    L = Layer(s, s)
    L.soft_ellipse(16, 16, 9, 9, "#ffffff", 0.35, hard=0.1)
    P._sparkle(L, 16, 16, 15, "#ffffff", 1.0)
    L.soft_ellipse(16, 16, 2.5, 2.5, "#ffffff", 1.0, hard=0.5)
    return L.rgba()


def leaf():
    """A falling petal, pale (the system's tints colour it): a rounded drop with a notch and a
    crease, lit on its top left."""
    w, h = 20, 16
    p = Piece(0, 0, w, h, ss=4, pad=0)
    t = tones("#fff4f8")
    shape = Path().move(3, 9).cubic(4, 3, 11, 1.5, 15, 3).quad(13.5, 5, 17, 6.5).cubic(17, 11, 10, 14.5, 3, 9).close()
    m = p.fill(shape, t, "base")
    p.tone(m - m.shift(-1.2, -1.4), tones("#f6c8da"), "base")
    p.stroke(Path.smooth([(5, 9), (10, 7.5), (14, 5.5)]), (0.9, 0.2), tones("#f6c8da"), "shadow")
    p.edge(shape, tones("#f0b8cf"), 0.8, "shadow")
    L = Layer(w, h)
    L.place(p, reach=0)
    return L.rgba()


PICTURES = (
    ("clouds", clouds), ("gull", gull), ("flag", flag), ("flag-pole", flag_pole), ("flame", flame), ("candle", candle),
    ("smoke", smoke), ("mote", mote), ("ash", ash), ("sparkle", sparkle), ("leaf", leaf),
)


def review(images, path):
    """Every picture on a dusk-blue ground, the small ones blown up 4x, for looking at."""
    rows = []
    for name, img in images:
        k = 4 if img.shape[1] <= 64 and name not in ("flag-pole",) else 1
        if name in ("gull", "flag", "flame", "candle"):
            k = 2
        big = np.repeat(np.repeat(img, k, 0), k, 1) if k > 1 else img
        rows.append(big)
    W = max(r.shape[1] for r in rows) + 40
    H = sum(r.shape[0] + 20 for r in rows) + 20
    out = np.zeros((H, W, 4), np.uint8)
    out[..., :3] = (60, 64, 120)
    out[..., 3] = 255
    y = 20
    for r in rows:
        a = r[..., 3:4].astype(np.float64) / 255.0
        region = out[y:y + r.shape[0], 20:20 + r.shape[1], :3].astype(np.float64)
        out[y:y + r.shape[0], 20:20 + r.shape[1], :3] = np.rint(r[..., :3] * a + region * (1 - a)).astype(np.uint8)
        y += r.shape[0] + 20
    write_png(path, out)


def paint(preview=None, out=OUT):
    """Writes every picture into Content/Rooms/Life (and life-sheet.png into preview). Returns the
    files, relative to Content."""
    images = []
    files = []
    for name, fn in PICTURES:
        img = fn()
        write_png(os.path.join(out, name + ".png"), img)
        images.append((name, img))
        files.append("Rooms/Life/%s.png" % name)
    if preview:
        review(images, os.path.join(preview, "life-sheet.png"))
    return files


def assets_lines():
    """paint_all.py's ASSETS.md line for these pictures: ([the files], the line)."""
    files = ["Content/Rooms/Life/%s.png" % n for n, _ in PICTURES]
    return [(files, ", ".join(files) + " - created by Caleb Kiatoukaysi for Legend Quest, the rooms' life loops in the "
             "illustrated style: six drifting clouds, a gull's two wingbeats, a pennant waving and its pole, a torch's "
             "and a candle's flames flickering, and the smoke, motes, ash, sparkles and petals the rooms' particles are "
             "drawn with, painted procedurally by tools/rooms/paint_life.py with tools/rooms/paint.py, released under "
             "public domain")]


def main():
    ap = argparse.ArgumentParser(description="Paint the rooms' life loops into Content/Rooms/Life.")
    ap.add_argument("--preview", help="also write life-sheet.png into this folder")
    a = ap.parse_args()
    paint(a.preview)


if __name__ == "__main__":
    main()
