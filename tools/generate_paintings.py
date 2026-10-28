"""
The paintings behind the story's big moments, each in layers like the title.

    python tools/generate_paintings.py

Every layer is the title's canvas, 416x277 art pixels, drawn at 4x, and the game sways them against
each other the way it sways the title (Story/Paintings.cs has the layers and how much each moves).
Skies are ordered dither between a few colours, never a smooth gradient; anything lit is lit from
where the light in that picture is (the moon, the sunrise) and goes lavender in shadow, the same
rules as everything else. Questeria at dawn is my title's city, warmed to sunrise; the rest is drawn
here. The people in them are silhouettes edged with the light behind them, small enough that a
face would be two pixels anyway.

Only needs CPython. Every bit of noise is hash01 on where it is, so a rerun writes the same bytes.
"""

import math
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from pixels import OUT, hash01, new_image, write_png  # noqa: E402
from cut_supplied import TITLE_H, TITLE_SOURCE_BOX, TITLE_W, sample  # noqa: E402

W, H = TITLE_W, TITLE_H

BAYER = ((0, 8, 2, 10), (12, 4, 14, 6), (3, 11, 1, 9), (15, 7, 13, 5))


def lit(x, y, t):
    """Whether an ordered-dither cell is on at a level 0..1."""
    return t * 16 > BAYER[y % 4][x % 4]


def put(img, x, y, c):
    if 0 <= x < W and 0 <= y < H:
        img[y][x] = [c[0], c[1], c[2], 255]


def band(img, top, bottom, stops, x0=0, x1=W):
    """Fills rows with a sky that steps through colours, dithering between each pair, never blending them."""
    for y in range(top, bottom):
        t = (y - top) / max(1, bottom - top - 1) * (len(stops) - 1)
        i = min(len(stops) - 2, int(t))
        f = t - i
        for x in range(x0, x1):
            put(img, x, y, stops[i + 1] if lit(x, y, f) else stops[i])


def disc(img, cx, cy, r, colour_at):
    """A filled circle, each pixel coloured by colour_at(x, y, distance from the middle over r)."""
    for y in range(int(cy - r) - 1, int(cy + r) + 2):
        for x in range(int(cx - r) - 1, int(cx + r) + 2):
            d = math.hypot(x + 0.5 - cx, y + 0.5 - cy) / r
            if d <= 1.0:
                c = colour_at(x, y, d)
                if c:
                    put(img, x, y, c)


def poly(img, points, colour):
    """A filled polygon, even-odd."""
    ys = [p[1] for p in points]
    for y in range(int(min(ys)), int(max(ys)) + 1):
        xs = []
        n = len(points)
        for i in range(n):
            (ax, ay), (bx, by) = points[i], points[(i + 1) % n]
            if (ay <= y + 0.5 < by) or (by <= y + 0.5 < ay):
                xs.append(ax + (y + 0.5 - ay) * (bx - ax) / (by - ay))
        xs.sort()
        for i in range(0, len(xs) - 1, 2):
            for x in range(int(round(xs[i])), int(round(xs[i + 1]))):
                put(img, x, y, colour)


def cloud(img, cx, cy, w, h, body, rim, seed, lit_below=False, shadow=None):
    """A long, flat bank of cloud: a row of lumps, each shaded by how high in it a pixel is, so the
    side the light's on comes out in the rim colour and the far side in shadow, dithered between."""
    shadow = shadow or body
    lumps = []
    for i in range(9):
        lx = cx + (i / 8 - 0.5) * w + (hash01(i, 1, seed) - 0.5) * w * 0.12
        ly = cy + (hash01(i, 2, seed) - 0.5) * h * 0.5
        lr = h * (0.35 + hash01(i, 3, seed) * 0.45) * (1 - abs(i / 8 - 0.5) * 0.8)
        lumps.append((lx, ly, lr))
    for y in range(int(cy - h * 1.5), int(cy + h * 1.5)):
        for x in range(int(cx - w * 0.6), int(cx + w * 0.6)):
            best = None
            for lx, ly, lr in lumps:
                d = ((x - lx) / (lr * 2.2)) ** 2 + ((y - ly) / lr) ** 2
                if d <= 1.0 and (best is None or d < best[0]):
                    best = (d, (y - ly) / lr)
            if best is None:
                continue
            d, v = best
            if d > 0.82 and not lit(x, y, (1.0 - d) / 0.18):
                continue
            # v runs -1 at a lump's top to 1 at its bottom.
            toward = v if lit_below else -v
            if toward > 0.35:
                c = rim
            elif toward > 0.0:
                c = rim if lit(x, y, toward / 0.35) else body
            elif toward > -0.5:
                c = body
            else:
                c = shadow if lit(x, y, (-toward - 0.5) * 2) else body
            put(img, x, y, c)


def outline_dark(img, colour):
    """A dark line round everything drawn, on the outside, below and right (the light's from the top left)."""
    marks = []
    for y in range(H):
        for x in range(W):
            if img[y][x][3]:
                continue
            if any(0 <= x - dx < W and 0 <= y - dy < H and img[y - dy][x - dx][3] for dx, dy in ((1, 0), (0, 1), (-1, 0), (0, -1))):
                marks.append((x, y))
    for x, y in marks:
        put(img, x, y, colour)


def figure(img, foot_x, foot_y, height, colour, rim, rim_side, pose="stand", reach=0):
    """A person as a silhouette: head, shoulders, body, legs, an arm reaching out by `reach`, edged
    on one side with the light behind them."""
    head_r = height * 0.13
    head_y = foot_y - height + head_r
    shoulder_y = foot_y - height * 0.72
    hip_y = foot_y - height * 0.38
    half = height * 0.15
    cells = set()

    def add_poly(points):
        ys = [p[1] for p in points]
        for y in range(int(min(ys)), int(max(ys)) + 1):
            xs = []
            n = len(points)
            for i in range(n):
                (ax, ay), (bx, by) = points[i], points[(i + 1) % n]
                if (ay <= y + 0.5 < by) or (by <= y + 0.5 < ay):
                    xs.append(ax + (y + 0.5 - ay) * (bx - ax) / (by - ay))
            xs.sort()
            for i in range(0, len(xs) - 1, 2):
                for x in range(int(round(xs[i])), int(round(xs[i + 1]))):
                    cells.add((x, y))

    for y in range(int(head_y - head_r) - 1, int(head_y + head_r) + 2):
        for x in range(int(foot_x - head_r) - 1, int(foot_x + head_r) + 2):
            if math.hypot(x + 0.5 - foot_x, y + 0.5 - head_y) <= head_r:
                cells.add((x, y))
    add_poly([(foot_x - half * 0.5, head_y + head_r * 0.6), (foot_x + half * 0.5, head_y + head_r * 0.6),
              (foot_x + half, shoulder_y), (foot_x + half * 0.8, hip_y), (foot_x - half * 0.8, hip_y), (foot_x - half, shoulder_y)])
    if pose == "kneel":
        add_poly([(foot_x - half * 0.8, hip_y), (foot_x + half * 0.9, hip_y), (foot_x + half * 1.8, foot_y), (foot_x - half * 0.9, foot_y)])
    else:
        add_poly([(foot_x - half * 0.8, hip_y), (foot_x - half * 0.05, hip_y), (foot_x - half * 0.2, foot_y), (foot_x - half * 0.9, foot_y)])
        add_poly([(foot_x + half * 0.05, hip_y), (foot_x + half * 0.8, hip_y), (foot_x + half * 0.9, foot_y), (foot_x + half * 0.2, foot_y)])
    if reach:
        hand_x = foot_x + reach
        add_poly([(foot_x, shoulder_y + 1), (foot_x, shoulder_y + 3), (hand_x, shoulder_y + height * 0.12), (hand_x, shoulder_y + height * 0.12 - 2)])
    for x, y in cells:
        put(img, x, y, colour)
    for x, y in cells:
        if (x + rim_side, y) not in cells or (x, y - 1) not in cells and rim_side > 0:
            put(img, x, y, rim)
    return cells


def save(name, layer, img):
    write_png(os.path.join(OUT, "Paintings", name + "-" + layer + ".png"), img)


# --------------------------------------------------------------------------- #
# P-05: Questeria out of the sea at dawn, from the Gull's Wager's rail
# --------------------------------------------------------------------------- #

DAWN_SKY = ((40, 44, 98), (78, 70, 140), (150, 98, 160), (226, 132, 150), (250, 184, 140), (255, 226, 160))
SEA = ((26, 40, 80), (42, 70, 120), (70, 104, 156), (120, 140, 180))
GLINT = ((255, 214, 150), (255, 242, 200))
WOOD = ((54, 34, 36), (92, 60, 50), (138, 94, 66), (184, 134, 88))


def questeria_dawn():
    sky = new_image(W, H)
    band(sky, 0, 190, DAWN_SKY)
    # The sun just under the city's shoulder, and a few clouds lit gold underneath.
    disc(sky, 300, 176, 22, lambda x, y, d: (255, 244, 210) if d < 0.75 else (255, 220, 150) if lit(x, y, 1.6 - d) else None)
    for i, (cx, cy, w, h) in enumerate(((70, 44, 120, 9), (210, 22, 160, 8), (350, 60, 110, 8), (130, 104, 90, 6), (390, 118, 70, 5))):
        cloud(sky, cx, cy, w, h, (126, 88, 146), (250, 196, 156), 100 + i, lit_below=True, shadow=(86, 66, 120))
    save("questeria-dawn", "sky", sky)

    # My title's city, warmed: every colour pulled toward the sunrise, the shadows kept blue.
    city = sample("title-city.png", TITLE_SOURCE_BOX, (W, H), key=True, holes=True)
    for y in range(H):
        for x in range(W):
            r, g, b, a = city[y][x]
            if not a:
                continue
            luma = (r * 3 + g * 6 + b) / 2550
            warm = 0.35 * luma
            city[y][x] = [min(255, int(r + (255 - r) * warm * 0.9)), min(255, int(g + (214 - g) * warm * 0.6)), int(b * (1 - warm * 0.5)), 255]
    # The city only goes down to the sea; everything under the waterline is the sea's.
    for y in range(206, H):
        for x in range(W):
            city[y][x] = [0, 0, 0, 0]
    save("questeria-dawn", "city", city)

    sea = new_image(W, H)
    band(sea, 206, H, (SEA[2], SEA[1], SEA[1], SEA[0]))
    for y in range(206, H):
        depth = (y - 206) / (H - 206)
        stroke = 3 + int(depth * 14)
        for x in range(0, W, stroke * 2):
            if hash01(x // stroke, y, 7) > 0.55:
                shift = int(hash01(x, y, 8) * stroke)
                for k in range(stroke):
                    put(sea, x + shift + k, y, SEA[3] if k < stroke // 2 else SEA[2])
        # The sun's road: short gold strokes, narrow at the horizon and wider towards you.
        half = 6 + depth * 46
        for x in range(int(300 - half), int(300 + half)):
            n = hash01(x // 2, y, 9)
            if n > 0.62 - depth * 0.2 and (y % 2 == 0 or n > 0.85):
                put(sea, x, y, GLINT[1] if n > 0.9 else GLINT[0])
    save("questeria-dawn", "sea", sea)

    rail = new_image(W, H)
    # The ship's rail across the bottom: a thick top rail, posts, and the deck's edge below them,
    # all lit from the sunrise in front.
    for x in range(W):
        for y in range(236, 246):
            put(rail, x, y, WOOD[3] if y == 236 else WOOD[2] if y < 241 else WOOD[1] if y < 245 else WOOD[0])
        for y in range(262, H):
            put(rail, x, y, WOOD[2] if y == 262 else WOOD[1] if (x + 3 * (y // 5)) % 37 else WOOD[0])
    for px in range(10, W, 46):
        for y in range(246, 262):
            for x in range(px, px + 7):
                put(rail, x, y, WOOD[3] if x == px else WOOD[2] if x < px + 4 else WOOD[1])
    # A coil of rope on the rail, and a lantern hung from a post.
    disc(rail, 322, 233, 8, lambda x, y, d: (214, 186, 132) if int(d * 8) % 2 else (150, 120, 84))
    disc(rail, 102, 226, 5, lambda x, y, d: (255, 236, 170) if d < 0.5 else (230, 170, 80))
    for y in range(214, 222):
        put(rail, 102, y, WOOD[0])
    outline_dark(rail, (30, 18, 26))
    save("questeria-dawn", "rail", rail)


# --------------------------------------------------------------------------- #
# P-03: the oath under the red moon
# --------------------------------------------------------------------------- #

NIGHT = ((10, 6, 22), (26, 12, 40), (52, 18, 52), (96, 30, 58), (138, 46, 62))
MOON = ((150, 34, 40), (198, 56, 50), (232, 96, 72), (252, 150, 112))

#: The moon's craters: where they are and how big, in art pixels.
CRATERS = ((180, 80, 9), (232, 70, 6), (246, 120, 11), (196, 136, 7), (160, 112, 5), (222, 156, 4), (270, 92, 5))
ROCK = ((14, 8, 20), (30, 18, 34))
OATH = ((255, 210, 120), (255, 246, 210))


def red_moon():
    sky = new_image(W, H)
    band(sky, 0, H, NIGHT)
    for i in range(70):
        x, y = int(hash01(i, 1, 21) * W), int(hash01(i, 2, 21) * 150)
        put(sky, x, y, (240, 220, 230) if hash01(i, 3, 21) > 0.7 else (160, 120, 160))
    save("red-moon", "sky", sky)

    moon = new_image(W, H)

    def moon_colour(x, y, d):
        crater = any(math.hypot(x - cx2, y - cy2) < r2 for cx2, cy2, r2 in CRATERS) and d < 0.9
        shade = 3 - int(d * 2.2) - (1 if (x - 208) + (y - 104) > 30 else 0)
        c = MOON[max(0, min(3, shade - (1 if crater else 0)))]
        return c
    disc(moon, 208, 104, 78, moon_colour)
    # A thin glow round its edge.
    disc(moon, 208, 104, 84, lambda x, y, d: (90, 26, 44) if d > 0.93 and lit(x, y, 0.5) else None)
    save("red-moon", "moon", moon)

    front = new_image(W, H)
    # The rocks on the shore they stand on, black against the moon, with its red along their tops.
    poly(front, [(0, 222), (90, 212), (150, 218), (210, 206), (300, 214), (416, 204), (416, 277), (0, 277)], ROCK[0])
    for x in range(W):
        for y in range(196, 230):
            if front[y][x][3] and (y == 0 or not front[y - 1][x][3]):
                put(front, x, y, (110, 36, 50))
    # The two of them, sixteen, facing each other, palms together between them.
    rim = (230, 92, 80)
    figure(front, 184, 210, 46, ROCK[1], rim, 1, reach=14)
    figure(front, 232, 208, 48, ROCK[1], rim, -1, reach=-14)
    # The light where their hands meet: the Soul-Bond's first glow.
    disc(front, 208, 178, 7, lambda x, y, d: OATH[1] if d < 0.4 else OATH[0] if lit(x, y, 1.3 - d) else None)
    save("red-moon", "figures", front)


# --------------------------------------------------------------------------- #
# P-03: the pier in the storm where you met
# --------------------------------------------------------------------------- #

STORM = ((20, 24, 36), (34, 40, 56), (52, 60, 78), (78, 88, 104))
WAVE = ((16, 28, 40), (28, 46, 60), (54, 78, 92), (150, 176, 186))
PLANK = ((34, 24, 26), (64, 46, 40), (100, 76, 58))


def pier_memory():
    sky = new_image(W, H)
    band(sky, 0, 160, (STORM[0], STORM[1], STORM[1], STORM[2]))
    for i, (cx, cy, w, h) in enumerate(((100, 28, 320, 24), (330, 18, 300, 22), (210, 78, 380, 20), (60, 122, 240, 13), (370, 128, 220, 13))):
        cloud(sky, cx, cy, w, h, STORM[1], STORM[3], 200 + i, shadow=STORM[0])
    # A crack of lightning out at sea, forking once.
    x, fork = 300, None
    for y in range(30, 150):
        x += round((hash01(y, 1, 41) - 0.45) * 3)
        put(sky, x, y, (236, 240, 255))
        put(sky, x + 1, y, (150, 164, 214))
        if y == 90:
            fork = x
    for y in range(90, 132):
        fork -= 1 if hash01(y, 3, 41) > 0.3 else 0
        put(sky, fork, y, (200, 210, 250))
    save("pier-memory", "sky", sky)

    sea = new_image(W, H)
    band(sea, 150, H, (WAVE[2], WAVE[1], WAVE[0], WAVE[0]))
    for y in range(150, H):
        depth = (y - 150) / (H - 150)
        for x in range(W):
            crest = math.sin(x * (0.11 - depth * 0.06) + y * 0.45) + 0.6 * math.sin(x * 0.04 - y * 0.25)
            if crest > 1.35 - depth * 0.2:
                put(sea, x, y, WAVE[3] if crest > 1.5 else WAVE[2])
    # The boat, over: its keel up, the planks of its hull, and the sea breaking white along it.
    poly(sea, [(234, 186), (346, 180), (330, 202), (250, 206)], PLANK[1])
    poly(sea, [(240, 182), (342, 177), (346, 180), (234, 186)], PLANK[2])
    for x in range(244, 336, 10):
        for y in range(185, 204):
            if y % 5 == 0:
                put(sea, x + (y // 5) % 3, y, PLANK[0])
    outline_dark(sea, (10, 12, 20))
    for x in range(226, 354):
        if hash01(x, 4, 51) > 0.35:
            put(sea, x, 207 + int(hash01(x, 5, 51) * 3), WAVE[3])
    save("pier-memory", "sea", sea)

    pier = new_image(W, H)
    # The pier comes in from the bottom left, big in front, its boards going away from you.
    poly(pier, [(0, 214), (196, 196), (204, 202), (0, 238)], PLANK[1])
    for i in range(12):
        t = i / 12
        x0, y0 = t * 196, 214 - t * 18
        poly(pier, [(x0, y0), (x0 + 3, y0 - 0.3), (x0 + 3, y0 + 24 - t * 18), (x0, y0 + 24 - t * 18)], PLANK[0])
    for x in range(0, 200):
        put(pier, x, int(214 - x * 18 / 196), PLANK[2])
    for px, top, bottom in ((14, 236, 277), (70, 226, 270), (122, 216, 262), (170, 206, 252)):
        poly(pier, [(px, top), (px + 7, top - 1), (px + 7, bottom), (px, bottom)], PLANK[0])
    outline_dark(pier, (10, 10, 16))
    # The two of them at eight: one flat on the boards reaching down, one in the water reaching up.
    rim = (176, 192, 220)
    figure(pier, 182, 200, 40, (22, 20, 28), rim, 1, pose="kneel", reach=16)
    figure(pier, 212, 236, 34, (22, 20, 28), rim, -1, reach=-12)
    for x in range(196, 232):
        if hash01(x, 3, 51) > 0.3:
            put(pier, x, 222 + (x % 3), WAVE[3])
    # Rain, slanting, over the front of everything.
    for i in range(300):
        rx, ry = int(hash01(i, 1, 61) * W), int(hash01(i, 2, 61) * H)
        for k in range(6):
            put(pier, rx - k // 2, ry + k, (150, 166, 190) if k < 3 else (96, 110, 140))
    save("pier-memory", "pier", pier)


if __name__ == "__main__":
    print("Painting the story's paintings for Legend Quest...")
    questeria_dawn()
    red_moon()
    pier_memory()
    print("Done.")
