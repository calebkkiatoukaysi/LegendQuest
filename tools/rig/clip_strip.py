"""
A clip's frames on a dressed figure, side by side: the contact sheet an animator checks a move by
(its anticipation, its arcs, its overshoot, whether the weapon goes through the body), and, if
asked, the clip as an animated GIF.

    python tools/rig/clip_strip.py <out.png> <clip> [<clip> ...] [options]

    --fps N          frames a second to draw the clip at (30)
    --gif <out.gif>  also writes the clips, one after another, as an animated GIF at that rate
    --base B         the body: base-male (the default) or base-female
    --armor A        an outfit's pieces (a folder in tools/source/characters/armors), over the body
    --weapon W       what the near hand holds (an id in tools/source/characters/weapons-alone),
                     "-" for nothing; by default the clip's own weapon kind's (WEAPONS)
    --offhand O      what the far hand holds (an offhand id, or any item, which goes in the far fist)
    --shrink N       draw at 1/N of the bases' canvas (6)
    --per-row N      frames in a row before it wraps (12)
    --json           sample tools/rig/clips.py's clips as it now builds them, not clips.json's

Each clip is a band: its name and length over it, its frames in order, each labelled with its
frame number and time, the ground drawn under the feet. A frame on or just past an event is
marked by a coloured bar under it (hit red, release orange, telegraph yellow, land brown, recover
green, step grey) and its name; the frames in a `trail` window have a blue line over them. The
band ends with the whole clip's frames drawn over each other faintly (onion skin), so a swing's
arc and where its overshoot carries the weapon show in one picture.

The figure is posed by clips.sample(), the game's easing (per key, contract §2.2), and drawn by
skeleton.render(), whose bones carry their scale to their children as the game's do; the pieces are
tools/source/characters/bodies/<base>, the outfit's over them as tools/dress_pose.py lays them, and
the held things put in the fist as tools/weapon_alone.py puts them.

Needs CPython, numpy, scipy and Pillow.

Written by Caleb Kiatoukaysi for Legend Quest.
"""

import json
import os
import sys

import numpy as np
from PIL import Image, ImageDraw

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
sys.path.insert(0, os.path.dirname(HERE))
import clips as C  # noqa: E402
import skeleton as K  # noqa: E402
import dress_pose as D  # noqa: E402
import weapon_alone as W  # noqa: E402

ROOT = os.path.normpath(os.path.join(HERE, "..", ".."))
CHARS = os.path.join(ROOT, "tools", "source", "characters")
CLIPS_JSON = os.path.join(ROOT, "Content", "Characters", "Parts", "clips.json")

#: What each weapon kind is shown holding: (near hand, far hand), either None.
WEAPONS = {
    "one-hand": ("weapon-longsword", None), "two-hand": ("weapon-greatsword", None),
    "polearm": ("weapon-spear", None), "dagger": ("weapon-dagger", "offhand-dagger"), "unarmed": (None, None),
    "hook": ("weapon-cutlass", "weapon-climber-hook"), "staff": ("weapon-quarterstaff", None),
    "bow": ("weapon-longbow", None), "crossbow": ("weapon-light-crossbow", None),
}
#: A class's skills are shown with what that class fights with.
CLASS_KIT = {
    "fighter": ("weapon-longsword", "offhand-steel-shield"), "rogue": ("weapon-dagger", "offhand-dagger"),
    "cleric": ("weapon-mace", "offhand-wooden-shield"), "wizard": ("weapon-quarterstaff", None),
    "paladin": ("weapon-longsword", "offhand-warden-shield"), "druid": ("weapon-quarterstaff", None),
    "warlock": ("weapon-dagger", None), "bard": ("weapon-rapier", None),
    "watchblade": ("weapon-cudgel", "offhand-steel-shield"), "netcaster": ("weapon-gaff", None),
    "deckhand": ("weapon-cutlass", None), "lodge-ranger": ("weapon-longbow", None),
    "battlemage": ("weapon-quarterstaff", None), "ashwalker": ("weapon-greatsword", None),
    "bulwark": ("weapon-warhammer", "offhand-steel-shield"), "ember-mender": ("weapon-pilgrim-staff", None),
    "lanerunner": ("weapon-kael-knives", "offhand-dagger"), "wayfarer": ("weapon-walking-stick", None),
    "gilded-knight": ("weapon-sunsteel-blade", "offhand-warden-shield"), "hemomancer": ("weapon-cultist-dagger", None),
    "hivekeeper": ("weapon-smoker", None), "glasswright": ("weapon-glass-lamp", None),
    "lamplighter": ("weapon-lantern", None), "crownsworn": ("weapon-longsword", None),
    "hourwarden": ("weapon-staff-ring", None),
}
#: A foe's moves are shown with its weapons.
FOE_KIT = {
    "bosun-grell": ("weapon-cutlass", "weapon-climber-hook"), "kael-duel": ("weapon-kael-rapier", "offhand-dagger"),
    "hollowed-hexer": ("weapon-hexer-staff", None), "hollowed-sapper": ("weapon-sapper-pick", None),
    "sapper-chief": ("weapon-sapper-pick", None), "vault-construct": (None, None),
    "hollowed-footman": ("weapon-hollowed-sword", None), "red-gull-thug": ("weapon-cudgel", None),
}
MARKS = {"hit": (220, 40, 40), "release": (240, 140, 20), "telegraph": (230, 200, 30), "land": (130, 90, 50),
         "recover": (40, 170, 70), "step": (150, 150, 150)}
BACKGROUND = (236, 232, 222)


def kit_for(name):
    """What a clip is best shown holding, by its name: (near, far)."""
    for prefix, table in (("auto-", WEAPONS), ("telegraph-", WEAPONS), ("victory-", WEAPONS), ("windup-", WEAPONS),
                          ("strike-", WEAPONS), ("ready-", WEAPONS)):
        if name.startswith(prefix):
            kind = name[len(prefix):]
            for k in sorted(table, key=len, reverse=True):
                if kind.startswith(k):
                    return table[k]
    if name.startswith("skill-"):
        rest = name[len("skill-"):]
        for cls in sorted(CLASS_KIT, key=len, reverse=True):
            if rest.startswith(cls + "-"):
                return CLASS_KIT[cls]
    if name.startswith("foe-"):
        rest = name[len("foe-"):]
        for foe in sorted(FOE_KIT, key=len, reverse=True):
            if rest.startswith(foe + "-"):
                return FOE_KIT[foe]
    return WEAPONS["one-hand"]


def shrink(colour, alpha, n):
    """A layer at 1/n size, its pixels averaged premultiplied."""
    h, w = alpha.shape
    h2, w2 = h // n, w // n
    a = alpha[:h2 * n, :w2 * n]
    c = colour[:h2 * n, :w2 * n] * a[:, :, None]
    a = a.reshape(h2, n, w2, n).mean(axis=(1, 3))
    c = c.reshape(h2, n, w2, n, 3).mean(axis=(1, 3)) / np.maximum(a, 1e-6)[:, :, None]
    return c, a


def paper(rgba, light=232.0):
    """The white sheet round a held thing whose picture was never cut out (a few items are still
    drawn on an opaque sheet): the near-white pixels joined to the picture's edge."""
    from scipy import ndimage
    pale = (rgba[:, :, :3] >= light).all(axis=2)
    parts, _ = ndimage.label(pale)
    edge = np.unique(np.concatenate([parts[0], parts[-1], parts[:, 0], parts[:, -1]]))
    return np.isin(parts, edge[edge > 0])


def held_layer(base, item, side):
    """An item in a fist on the base's canvas, as a layer, or None if there's no such item."""
    for name in (item, item + ".far"):
        path = os.path.join(CHARS, "weapons-alone", name + ".png")
        if os.path.exists(path):
            rgba = W.ic.read_png(path)[0].astype(np.float64)
            if (rgba[:, :, 3] > 200).mean() > 0.9:
                rgba[:, :, 3] *= ~paper(rgba)
            note = json.load(open(path[:-4] + ".json", encoding="utf-8"))
            pm = W.held(base, rgba[:, :, :3], rgba[:, :, 3] / 255.0, note["grip"], side, note["kind"])
            return pm[:, :, :3] / np.maximum(pm[:, :, 3:], 1e-6) * 255.0, pm[:, :, 3]
    return None


_LAYERS = {}


def figure(base, armor, near, far, n):
    """The dressed figure's layers at 1/n size."""
    key = (base, armor, near, far, n)
    if key not in _LAYERS:
        bodies = D.body(base)
        layers = D.layers_of(base, os.path.join(CHARS, "armors", armor), bodies) if armor else dict(bodies)
        for item, side in ((near, "near"), (far, "far")):
            if item:
                got = held_layer(base, item, side)
                if got is None:
                    print("clip_strip: no item called", item)
                else:
                    layers["hand_%s+" % side] = got
        _LAYERS[key] = {k: shrink(c, a, n) for k, (c, a) in layers.items()}
    return _LAYERS[key]


def frames_of(clip, fps):
    """The times a clip is drawn at: every 1/fps from 0 to its end (a loop's last frame is its first)."""
    n = int(round(clip["length"] * fps))
    times = [min(clip["length"], k / float(fps)) for k in range(n + 1)]
    if clip["loop"]:
        times = times[:-1] or [0.0]
    return times


_PREMUL = {}


def premultiplied(layers):
    """Each layer premultiplied (0-1) and cropped to where it's painted: {key: (rgba, x0, y0)}."""
    key = id(layers)
    if key not in _PREMUL:
        out = {}
        for k, (c, a) in layers.items():
            ys, xs = np.nonzero(a > 0.004)
            if len(xs) == 0:
                continue
            y0, y1, x0, x1 = max(0, ys.min() - 1), ys.max() + 2, max(0, xs.min() - 1), xs.max() + 2
            pm = np.concatenate([c[y0:y1, x0:x1] / 255.0 * a[y0:y1, x0:x1, None], a[y0:y1, x0:x1, None]], axis=2)
            out[k] = (pm, x0, y0)
        _PREMUL[key] = out
    return _PREMUL[key]


def render(base, layers, pose, size, origin, scale, front=()):
    """skeleton.render(), the same picture, but each piece worked out only over the box it lands in."""
    w, h = size
    out = np.zeros((h, w, 4))
    pieces = premultiplied(layers)
    transforms = K.world(base, pose, scale)
    order = [b for b in K.DRAW if b not in front]
    at = order.index("head") + 1
    order[at:at] = [b for b in K.DRAW if b in front]
    for bone in order:
        for key in (bone + "+", bone) if bone.startswith("hand") else (bone,):
            if key not in pieces:
                continue
            pm, px, py = pieces[key]
            ph, pw = pm.shape[:2]
            m = np.vstack([transforms[bone], [0, 0, 1]])
            corners = np.array([[px, py, 1], [px + pw, py, 1], [px, py + ph, 1], [px + pw, py + ph, 1]], float).T
            dest = (m @ corners)[:2] + np.array(origin, float)[:, None]
            bx0, by0 = max(0, int(np.floor(dest[0].min())) - 1), max(0, int(np.floor(dest[1].min())) - 1)
            bx1, by1 = min(w, int(np.ceil(dest[0].max())) + 2), min(h, int(np.ceil(dest[1].max())) + 2)
            if bx1 <= bx0 or by1 <= by0:
                continue
            inv = np.linalg.inv(m)
            yy, xx = np.mgrid[by0:by1, bx0:bx1].astype(np.float64)
            xx -= origin[0]
            yy -= origin[1]
            sx = inv[0, 0] * xx + inv[0, 1] * yy + inv[0, 2] - px
            sy = inv[1, 0] * xx + inv[1, 1] * yy + inv[1, 2] - py
            x0, y0 = np.floor(sx).astype(int), np.floor(sy).astype(int)
            fx, fy = (sx - x0)[:, :, None], (sy - y0)[:, :, None]
            ok = ((x0 >= 0) & (y0 >= 0) & (x0 < pw - 1) & (y0 < ph - 1))[:, :, None]
            x0, y0 = np.clip(x0, 0, pw - 2), np.clip(y0, 0, ph - 2)
            sample = (pm[y0, x0] * (1 - fx) * (1 - fy) + pm[y0, x0 + 1] * fx * (1 - fy)
                      + pm[y0 + 1, x0] * (1 - fx) * fy + pm[y0 + 1, x0 + 1] * fx * fy) * ok
            region = out[by0:by1, bx0:bx1]
            out[by0:by1, bx0:bx1] = sample + region * (1.0 - sample[:, :, 3:])
    return out


def render_clip(base, layers, clip, fps, n):
    """A clip's frames, premultiplied RGBA floats on one wide canvas each, and where the ground is."""
    h0, w0 = next(iter(layers.values()))[1].shape
    size = (w0 * 3, int(h0 * 1.3))
    origin = (w0, int(h0 * 0.3))
    front = clip.get("front", ())
    out = []
    for t in frames_of(clip, fps):
        out.append(render(base, layers, C.sample(clip, t), size, origin, 1.0 / n, front))
    ground = origin[1] + K.JOINTS[base]["root"][1] / n
    return out, ground


def band(name, clip, shots, ground, fps, per_row):
    """A clip's contact sheet band (a PIL image)."""
    alpha = np.max([s[:, :, 3] for s in shots], axis=0)
    ys, xs = np.nonzero(alpha > 0.03)
    pad = 6
    y0, y1 = max(0, ys.min() - pad), min(alpha.shape[0], max(ys.max(), int(ground)) + pad + 2)
    x0, x1 = max(0, xs.min() - pad), min(alpha.shape[1], xs.max() + pad)
    cw, ch = x1 - x0, y1 - y0
    times = frames_of(clip, fps)
    events = clip.get("events", [])
    trail = clip.get("trail", [])
    count = len(shots) + 1
    cols = min(per_row, count)
    rows = (count + cols - 1) // cols
    label_h, mark_h, head_h = 12, 12, 16
    cell_h = ch + label_h + mark_h
    img = Image.new("RGB", (cols * cw, head_h + rows * cell_h), BACKGROUND)
    draw = ImageDraw.Draw(img)
    extra = []
    if clip.get("blend") is not None:
        extra.append("blend %.2f" % clip["blend"])
    if clip["loop"]:
        extra.append("loop")
    ev_text = ", ".join("%s %.3f" % (e[1] + ("(" + str(e[2]) + ")" if len(e) > 2 else ""), e[0]) for e in events)
    draw.text((4, 2), "%s  %.3f s  %s   %s" % (name, clip["length"], " ".join(extra), ev_text), fill=(20, 20, 20))
    bg = np.array(BACKGROUND, float) / 255.0

    def paste(rgba, k, faint=None):
        r, c = divmod(k, cols)
        if faint is None:
            crop = rgba[y0:y1, x0:x1]
            pic = crop[:, :, :3] + bg * (1.0 - crop[:, :, 3:])
        else:
            pic = faint
        px, py = c * cw, head_h + r * cell_h
        img.paste(Image.fromarray(np.rint(np.clip(pic, 0, 1) * 255).astype(np.uint8)), (px, py))
        gy = py + int(ground) - y0
        draw.line([(px, gy), (px + cw - 1, gy)], fill=(120, 110, 95))
        draw.line([(px + cw - 1, py), (px + cw - 1, py + cell_h)], fill=(200, 195, 185))
        return px, py

    step = 1.0 / fps
    for k, (shot, t) in enumerate(zip(shots, times)):
        px, py = paste(shot, k)
        draw.text((px + 2, py + ch), "%d  %.3f" % (k, t), fill=(60, 60, 60))
        if any(a - 1e-6 <= t <= b + 1e-6 for a, b, *_ in trail):
            draw.line([(px + 2, py + 1), (px + cw - 3, py + 1)], fill=(40, 90, 220), width=3)
        here = [e for e in events if t - step < e[0] <= t + 1e-6 or (k == 0 and e[0] <= 1e-6)]
        if here:
            # One bar for the frame, in the colour of its weightiest event (a blow before the
            # rest), naming every event on it.
            rank = ["hit", "release", "land", "telegraph", "recover", "step"]
            first = min(here, key=lambda e: rank.index(e[1]) if e[1] in rank else len(rank))
            yb = py + ch + label_h
            draw.rectangle([px + 2, yb, px + cw - 3, yb + mark_h - 2], fill=MARKS.get(first[1], (120, 60, 160)))
            names = ", ".join(e[1] if len(e) < 3 else "%s %s" % (e[1], e[2]) for e in here)
            draw.text((px + 4, yb), names, fill=(255, 255, 255))
    # The onion skin: every frame faintly over the others, the hit frames stronger.
    onion = np.zeros(shots[0].shape[:2] + (3,)) + bg
    hits = [e[0] for e in events if e[1] in ("hit", "release")]
    for shot, t in zip(shots, times):
        strong = any(abs(t - h) < step * 0.75 for h in hits)
        a = shot[:, :, 3:] * (0.55 if strong else 0.18)
        onion = (shot[:, :, :3] / np.maximum(shot[:, :, 3:], 1e-6)) * a + onion * (1.0 - a)
    px, py = paste(None, len(shots), faint=onion[y0:y1, x0:x1])
    draw.text((px + 2, py + ch), "onion", fill=(60, 60, 60))
    return img


def main(argv):
    if len(argv) < 2:
        sys.exit(__doc__)
    opts = {"--fps": "30", "--base": "base-male", "--shrink": "6", "--per-row": "12"}
    names, out, flags = [], None, set()
    i = 0
    while i < len(argv):
        a = argv[i]
        if a in ("--json",):
            flags.add(a)
        elif a.startswith("--"):
            opts[a] = argv[i + 1]
            i += 1
        elif out is None:
            out = a
        else:
            names.append(a)
        i += 1
    fps, n, per_row, base = int(opts["--fps"]), int(opts["--shrink"]), int(opts["--per-row"]), opts["--base"]
    if "--json" in flags:
        doc = C.build()
    else:
        doc = json.load(open(CLIPS_JSON, encoding="utf-8"))
    rig = doc["rigs"]["humanoid"]
    known, aliases = rig["clips"], rig.get("aliases", {})
    bands, gif = [], []
    for name in names:
        real = aliases.get(name, name)
        if real not in known:
            print("clip_strip: no clip called", name)
            continue
        clip = known[real]
        near, far = kit_for(name)
        if "--weapon" in opts:
            near = None if opts["--weapon"] == "-" else opts["--weapon"]
        if "--offhand" in opts:
            far = None if opts["--offhand"] == "-" else opts["--offhand"]
        layers = figure(base, opts.get("--armor"), near, far, n)
        shots, ground = render_clip(base, layers, clip, fps, n)
        bands.append(band(name, clip, shots, ground, fps, per_row))
        gif.append((shots, ground))
    if not bands:
        sys.exit(1)
    width = max(b.width for b in bands)
    sheet = Image.new("RGB", (width, sum(b.height for b in bands) + 4 * (len(bands) - 1)), (90, 90, 90))
    y = 0
    for b in bands:
        sheet.paste(b, (0, y))
        y += b.height + 4
    sheet.save(out)
    print("clip_strip: wrote", out)
    if "--gif" in opts:
        bg = np.array(BACKGROUND, float) / 255.0
        alpha = np.max([s[:, :, 3] for shots, _ in gif for s in shots], axis=0)
        ys, xs = np.nonzero(alpha > 0.03)
        y0, y1, x0, x1 = max(0, ys.min() - 6), ys.max() + 8, max(0, xs.min() - 6), xs.max() + 6
        pics = []
        for shots, ground in gif:
            for s in shots:
                pic = s[:, :, :3] + bg * (1.0 - s[:, :, 3:])
                pic[int(ground), :] = (0.47, 0.43, 0.37)
                pics.append(Image.fromarray(np.rint(np.clip(pic[y0:y1, x0:x1], 0, 1) * 255).astype(np.uint8)))
            pics.extend([pics[-1]] * int(fps * 0.3))
        pics[0].save(opts["--gif"], save_all=True, append_images=pics[1:], duration=int(round(1000.0 / fps)), loop=0)
        print("clip_strip: wrote", opts["--gif"])


if __name__ == "__main__":
    main(sys.argv[1:])
