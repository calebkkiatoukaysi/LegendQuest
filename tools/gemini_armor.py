"""Paints an armor from Story/13-art-prompts.md §5 with Gemini, and gives it back as a sheet in the template's layout.

    python tools/gemini_armor.py <armor-id> [--tries 4] [--out <folder>] [--keep]
    python tools/gemini_armor.py <armor-id> --prompt      # only print the whole prompt, to paste by hand

What makes it come back right, found by trying it (2026-11-18):

1. A wide template. Gemini draws every piece a little bigger and nearer its neighbours than it was, so on the
   armor template itself (gaps of 30 pixels) the pieces come back touching. It's given the same pieces at
   three-quarter size about their own middles (gaps of 80 pixels and more), and they come back as thirteen
   islands. Each is then put back on its body piece on the real template, as big as its place has room for.
2. The outfit said piece by piece. A text model rewrites the card's outfit as seven lines, one per kind of
   piece, so a robe or a coat is divided at the joints and nothing hangs from the torso over the legs.
3. The prompt below: the task, the layout, each piece, the fit, the look, then the outfit both ways.
4. The repo's own check (tools/body_parts.py check) on what comes back, and another try till it says GOOD.

The key is read from GEMINI_API_KEY; in a Claude cloud session the environment's network secret puts it in.
The picture is written to <out>/<armor-id>.png (and every try beside it); --keep also writes it to
tools/source/characters/sheets/<armor-id>.png, where tools/body_parts.py cut and tools/import_parts.py take it.
"""
import base64
import hashlib
import io
import json
import os
import re
import subprocess
import sys
import time
import urllib.error
import urllib.request

import numpy as np
from PIL import Image
from scipy import ndimage as ndi

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
CHARS = os.path.join(ROOT, "tools", "source", "characters")
STORY = os.path.join(ROOT, "Story", "13-art-prompts.md")
API = "https://generativelanguage.googleapis.com/v1beta/models/%s:generateContent"
PAINTER, WRITER = "gemini-3-pro-image", "gemini-3.8-flash"
#: Darker than this (brightest channel) is the black background, as in tools/body_parts.py.
BLACK = 14
#: The wide template's pieces, against the armor template's.
WIDE = 0.75

TASK = ("TASK. Edit the attached image. It is a parts sheet for a 2D game's cut-out character rig: one body pulled apart into "
    'THIRTEEN SEPARATE PIECES on a pure black background (no head). Repaint every piece IN PLACE so that it wears the '
    'outfit described under OUTFIT. This is an in-place repaint of a parts sheet, not a character illustration: do NOT put '
    'the pieces together into a standing figure, and do not move, turn, resize, add or remove any piece.')
LAYOUT = ('LAYOUT, which must hold exactly. The result has the same canvas and the same thirteen pieces in the same places, at '
    'the same sizes and angles as the attached image: 1 torso (neck stub down to the shorts) at the top centre; an upper '
    'arm, then a forearm, then a closed fist in a column down each side; a thigh, then a shin, then a foot in a column '
    'under each side of the torso. The near arm and leg are on the left, the far arm and leg on the right. Every piece is '
    'an island: pure black (#000000) shows all the way round each one, with a clear black gap between every pair of '
    'neighbours (torso and upper arms, torso and thighs, upper arm and forearm, forearm and fist, thigh and shin, shin and '
    'foot). Nothing reaches across a gap: no strap, cloth, plate, shadow or glow. Nothing is painted where the head would '
    'be, and nothing else is added: no head, helmet, hood, cape, weapon, floor, shadow, text or border.')
EACH = ('EACH PIECE. Dress each piece on its own, as if nothing were in front of it, right up to and over BOTH of its rounded '
    'ends at the joints, so no bare skin shows at an end and it can turn under its neighbour. Paint only on and just round '
    "each piece's own shape: a piece may grow a little wider than the body under it, never longer, and never so far that it "
    'closes a gap. What the outfit hangs over a joint (a skirt, coat tails, tassets, a long cuff) is cut at the gap: the '
    'part above the gap is painted on the upper piece and the part below it on the lower piece. Bare skin, only where the '
    "outfit calls for it, keeps the body's warm tan.")
FIT = ('FIT. Every piece keeps the silhouette it has in the attached image: the same length, the same angle and the same '
    "position, with the outfit adding only a thin layer round it (about a tenth of the piece's width at most). Draw bulky "
    "parts compact: a shoulder guard hugs the shoulder, and a cuff, a boot top, a sleeve end or a skirt hem doesn't flare. "
    'No piece is drawn longer than it is, and the fists stay at the angle they are. The torso piece ends where its shorts '
    'end: whatever the outfit, its clothing stops in a clean hem at the bottom edge of the shorts, and no skirt, sash end, '
    'coat front or tail hangs below it.  If in doubt, draw the piece a little smaller and leave more black round it.')
LOOK = ("LOOK. Match the attached image's own art style exactly: bold, clean deep-brown outlines (never pure black), heavier on "
    "each piece's outer edge; flat cel shading with two or three tones per material, hard-edged shadows and one soft "
    'highlight; light from the upper left. Grounded, well-made costume detail: seams, straps, buckles, rivets, stitching, a '
    'little wear. Original design; no pixel art, no 3D render, no gradients or painterly blur.')
RULES = """You are preparing a brief for an illustrator who paints an outfit onto a 2D cut-out character rig. The body is pulled apart into thirteen separate pieces with black gaps between them: the TORSO (from the neck stub down to the bottom of a pair of shorts), and for each side an UPPER ARM, a FOREARM, a closed FIST, a THIGH, a SHIN and a FOOT. Each piece is painted on its own and the game overlaps them at the joints.

Rewrite the outfit below as exactly seven lines, one per kind of piece, in this order and with these labels: TORSO, EACH UPPER ARM, EACH FOREARM, EACH FIST, EACH THIGH, EACH SHIN, EACH FOOT. Rules:
- Say only what is painted ON that piece. Keep every colour, material, pattern and detail of the outfit, and give each detail to the one piece it sits on.
- Nothing hangs across a gap. A garment that covers a joint is divided there: the part above goes on the upper piece, the part below on the lower piece, each wrapped round its own piece like a sleeve. So a robe, gown, skirt, long coat or long mail coat is: its body on the TORSO, ending at the bottom edge of the shorts; then on EACH THIGH a panel of the same cloth wrapped all the way round that thigh; then, if it is long enough, on EACH SHIN a panel wrapped round that shin, carrying the hem and its trim. Never describe one skirt spanning both legs. A long or wide sleeve is divided at the elbow the same way, and it hugs the arm: say that it does not flare.
- The TORSO line always says what covers the shorts (belt, trouser top, skirt top, tassets drawn short), so no underwear shows, and that the torso's clothing ends in a clean hem level with the bottom edge of the shorts: nothing on it hangs lower, so sash ends, belt ends, tabard tails and coat fronts are tucked in or cut off at that hem.
- Plates and guards go on one piece each: a shoulder guard on the upper arm, an elbow guard on the top of the forearm, a knee guard on the top of the shin, a tasset on the upper front of the thigh.
- If a piece is bare, say "bare skin, the body's warm tan". Fists are gloved or bare and closed.
- Plain words, one or two sentences per line, no preamble and nothing after the seventh line.

OUTFIT: """


def pieces_of(tpl_rgb):
    lab, n = ndi.label(tpl_rgb.max(2) > BLACK)
    out = []
    for i in range(1, n + 1):
        m = lab == i
        if m.sum() < 400: continue
        ys, xs = np.nonzero(m)
        out.append({'mask': m, 'box': (xs.min(), ys.min(), xs.max() + 1, ys.max() + 1),
                    'centre': ((xs.min() + xs.max() + 1) / 2.0, (ys.min() + ys.max() + 1) / 2.0)})
    return out
def make_wide(tpl_path, out_path, s=0.75):
    rgb = np.asarray(Image.open(tpl_path).convert('RGB'))
    H, W = rgb.shape[:2]
    canvas = Image.new('RGB', (W, H), (0, 0, 0)); meta = []
    for p in pieces_of(rgb):
        x0, y0, x1, y1 = p['box']
        crop = rgb[y0:y1, x0:x1].copy(); crop[~p['mask'][y0:y1, x0:x1]] = 0
        im = Image.fromarray(crop).resize((max(1, round((x1 - x0) * s)), max(1, round((y1 - y0) * s))), Image.LANCZOS)
        cx, cy = p['centre']
        px, py = int(round(cx - im.width / 2)), int(round(cy - im.height / 2))
        canvas.paste(im, (px, py))
        meta.append({'centre': [cx, cy], 'box': [int(x0), int(y0), int(x1), int(y1)]})
    canvas.save(out_path)
    json.dump({'s': s, 'size': [W, H], 'pieces': meta}, open(out_path[:-4] + '.json', 'w'))
    return out_path
def gaps(path):
    a = np.asarray(Image.open(path).convert('RGB')).max(2) > BLACK
    lab, n = ndi.label(a); best = 1e9
    for i in range(1, n + 1):
        d = ndi.distance_transform_edt(lab != i)
        best = min(best, d[(lab > 0) & (lab != i)].min())
    return n, best
def recompose(painted_path, tpl_path, wide_path, out_path, a=(0.0, 0.0), b=1.0, layers=None):
    """Give each piece of a picture painted on the wide template its place on the repo's template again."""
    meta = json.load(open(wide_path[:-4] + '.json')); s = meta['s']; W, H = meta['size']
    tpl = np.asarray(Image.open(tpl_path).convert('RGB'))
    wide = np.asarray(Image.open(wide_path).convert('RGB')).max(2) > BLACK
    im = Image.open(painted_path).convert('RGB')
    k = im.height / float(H)                              # painted pixels per template pixel
    big = np.asarray(im.resize((round(W * k), im.height), Image.LANCZOS))   # the canvas's own shape, at the painted height
    # who owns what, on the wide layout: the nearest wide piece
    wl, n = ndi.label(wide)
    _, (iy, ix) = ndi.distance_transform_edt(wl == 0, return_indices=True)
    own_w = wl[iy, ix]
    own_big = np.asarray(Image.fromarray(own_w.astype('uint8')).resize((big.shape[1], big.shape[0]), Image.NEAREST))
    painted = big.max(2) > BLACK
    # and on the repo's template, where each piece may be painted: nearest piece, less a margin, so the gaps stay black
    tl, tn = ndi.label(tpl.max(2) > BLACK)
    d, (jy, jx) = ndi.distance_transform_edt(tl == 0, return_indices=True)
    own_t = tl[jy, jx]
    edge = ndi.maximum_filter(own_t, 9) != ndi.minimum_filter(own_t, 9)   # 4 px either side of where two places meet
    out = np.zeros((H, W, 3), 'uint8'); report = []
    if layers is not None: return place(layers, own_t, edge, (W, H), out_path, a, b), layers
    layers = []
    for p in meta['pieces']:
        cx, cy = p['centre']
        wi = own_w[int(cy), int(cx)]; ti = tl[int(cy), int(cx)]
        if wi == 0 or ti == 0:   # a centre off its piece (a bent shape): take the nearest
            wi = own_w[int(cy), int(cx)]; ti = own_t[int(cy), int(cx)]
        m = painted & (own_big == wi)
        if m.sum() < 50: report.append('missing piece at %d,%d' % (cx, cy)); continue
        lab, c = ndi.label(m)                                      # the piece and what's joined to it; drop far specks
        sizes = ndi.sum(m, lab, range(1, c + 1)); keep = np.isin(lab, 1 + np.nonzero(sizes >= max(60 * k * k, 0.02 * sizes.max()))[0])
        ys, xs = np.nonzero(keep)
        x0, y0, x1, y1 = xs.min(), ys.min(), xs.max() + 1, ys.max() + 1
        crop = big[y0:y1, x0:x1].copy(); crop[~keep[y0:y1, x0:x1]] = 0
        allowed = (own_t == ti) & ~edge
        allowed[:8] = allowed[-8:] = False; allowed[:, :8] = allowed[:, -8:] = False
        guide = tl == ti; ga = float(guide.sum())
        gx0, gy0, gx1, gy1 = p['box']; gcx, gcy = (gx0 + gx1) / 2.0, (gy0 + gy1) / 2.0
        f0 = 1.0 / (s * k); cands = []
        for step in range(0, 13):                    # as big as it was painted, or as big as its place has room for
            f = f0 * (1.0 - 0.03 * step)
            pim = Image.fromarray(crop).resize((max(1, round((x1 - x0) * f)), max(1, round((y1 - y0) * f))), Image.LANCZOS)
            pa = np.asarray(pim); pm = pa.max(2) > BLACK; h, w = pm.shape
            for dy in (-18, -12, -6, 0, 6, 12, 18):
                for dx in (-18, -12, -6, 0, 6, 12, 18):
                    ox, oy = int(round(gcx - w / 2.0)) + dx, int(round(gcy - h / 2.0)) + dy
                    if ox < 0 or oy < 0 or ox + w > W or oy + h > H: continue
                    cover = (pm & guide[oy:oy + h, ox:ox + w]).sum() / ga
                    cut = (pm & ~allowed[oy:oy + h, ox:ox + w]).sum() / float(max(1, pm.sum()))
                    cands.append((cover, cut, step, abs(dx) + abs(dy), ox, oy, pa, pm))
        if not cands: report.append('no room for the piece at %d,%d' % (cx, cy)); continue
        top = max(c[0] for c in cands)
        ok = [c for c in cands if c[0] >= min(0.93, top - 0.01)]      # it has to cover the body piece it dresses
        low = min(c[1] for c in ok)                                   # and then lose as little as it can to its neighbours
        cover, cut, step, _, ox, oy, pa, pm = min([c for c in ok if c[1] <= low + 0.004], key=lambda c: (c[2], c[3]))
        layers.append((pa, ox, oy, int(ti)))
        report.append((round(1.0 - 0.03 * step, 2), round(float(cut), 3), round(float(cover), 2)))
    place(layers, own_t, edge, (W, H), out_path, a, b)
    return report, layers
def place(layers, own_t, edge, size, out_path, a, b):
    """The pieces on the template, each kept to its own place; (x - a) / b moves the whole picture."""
    W, H = size; out = np.zeros((H, W, 3), 'uint8'); trims = []
    for pa, ox, oy, ti in layers:
        h, w = pa.shape[:2]
        im = Image.fromarray(pa) if b == 1.0 else Image.fromarray(pa).resize((max(1, round(w / b)), max(1, round(h / b))), Image.LANCZOS)
        layer = Image.new('RGB', (W, H), (0, 0, 0)); layer.paste(im, (int(round((ox - a[0]) / b)), int(round((oy - a[1]) / b))))
        la = np.asarray(layer); lm = la.max(2) > BLACK
        allowed = (own_t == ti) & ~edge
        allowed[:8] = allowed[-8:] = False; allowed[:, :8] = allowed[:, -8:] = False
        trims.append(round(float((lm & ~allowed).sum()) / max(1, lm.sum()), 3))
        lm = lm & allowed; out[lm] = la[lm]
    Image.fromarray(out).save(out_path)
    return trims


def card(armor):
    """An armor's card in Story/13: the template it edits, and its outfit."""
    md = open(STORY, encoding="utf-8").read()
    c = re.search(r"^#### `" + re.escape(armor) + r"` .*?\n\n\*\*Basis:\*\* (.*?)\n\n(.*?)(?=\n#### |\n### |\n## )", md, re.M | re.S)
    if not c:
        sys.exit("gemini_armor: no card for " + armor + " in Story/13-art-prompts.md")
    edits = re.search(r"Edits `([^`]+)`", c.group(1)).group(1)
    if not edits.startswith("armor-parts-"):
        sys.exit("gemini_armor: " + armor + " edits " + edits + ", not an armor template: paint that one first, by hand")
    return edits, re.sub(r"\nI'll save it as .*$", "", c.group(2).strip())


def ask(model, parts, config):
    body = {"contents": [{"parts": parts}], "generationConfig": config}
    req = urllib.request.Request(API % model, data=json.dumps(body).encode(),
                                 headers={"content-type": "application/json", "x-goog-api-key": os.environ.get("GEMINI_API_KEY", "from-the-environment")})
    for turn in range(3):
        try:
            return json.load(urllib.request.urlopen(req, timeout=400))["candidates"][0]["content"]["parts"]
        except urllib.error.HTTPError as e:
            why = "HTTP %d %s" % (e.code, e.read().decode()[:300])
            if e.code in (400, 401, 403, 404):
                break
        except Exception as e:  # a dropped line, or an answer with no picture in it
            why = repr(e)[:300]
        time.sleep(5 * (turn + 1))
    sys.exit("gemini_armor: " + model + " said " + why)


def by_piece(outfit, cache):
    """The outfit rewritten as seven lines, one per kind of piece."""
    path = os.path.join(cache, hashlib.sha1((RULES + outfit).encode()).hexdigest()[:16] + ".txt")
    if os.path.exists(path):
        return open(path, encoding="utf-8").read()
    for turn in range(3):
        text = "".join(p.get("text", "") for p in ask(WRITER, [{"text": RULES + outfit}], {"temperature": 0.2})).strip()
        lines = [l.strip().lstrip("-* ").replace("**", "") for l in text.splitlines() if l.strip()]
        if len(lines) == 7 and lines[0].upper().startswith("TORSO"):
            text = "\n".join("- " + l for l in lines)
            open(path, "w", encoding="utf-8").write(text)
            return text
    sys.exit("gemini_armor: the outfit didn't come back as seven lines:\n" + text)


def prompt(outfit, pieces):
    return "\n\n".join([TASK, LAYOUT, EACH, FIT, LOOK,
                        "OUTFIT, as a whole (for its look, colours and materials): " + outfit,
                        "OUTFIT, piece by piece (this is what to paint on each piece; where the two differ, this wins):\n" + pieces])


def whole(armor, png):
    """The repo's check, and where it found the picture on the template: its scale and its move."""
    p = subprocess.run([sys.executable, os.path.join(ROOT, "tools", "body_parts.py"), "check", os.path.abspath(png), armor],
                       capture_output=True, text=True, cwd=ROOT)
    lines = (p.stdout + p.stderr).strip().splitlines()
    m = re.search(r"found at scale ([\d.]+), ([-\d.]+), ([-\d.]+)", lines[0]) if lines else None
    return (lines[-1] if lines else "no output"), (tuple(float(x) for x in m.groups()) if m else (1.0, 0.0, 0.0))


def fit(armor, raw, tpl, wide, out):
    """A picture painted on the wide template, on the armor template: placed, then moved till the check finds it there."""
    report, layers = recompose(raw, tpl, wide, out)
    a, b = [0.0, 0.0], 1.0
    for turn in range(4):
        verdict, (s, tx, ty) = whole(armor, out)
        if verdict.startswith("GOOD") or (abs(s - 1) < 0.003 and abs(tx) < 1.5 and abs(ty) < 1.5):
            break
        a, b = [a[0] + b * tx, a[1] + b * ty], b * s
        recompose(raw, tpl, wide, out, a, b, layers)
    return verdict, [r for r in report if isinstance(r, str)]


def main(argv):
    flags = [a for a in argv if a.startswith("--")]
    args = [a for a in argv if not a.startswith("--")]
    tries, out = 4, os.path.join(ROOT, "tools", ".cache", "gemini-armor")
    if "--tries" in argv:
        tries = int(argv[argv.index("--tries") + 1]); args.remove(argv[argv.index("--tries") + 1])
    if "--out" in argv:
        out = argv[argv.index("--out") + 1]; args.remove(out)
    if len(args) != 1 or set(flags) - {"--tries", "--out", "--keep", "--prompt"}:
        sys.exit(__doc__)
    armor = args[0]
    os.makedirs(out, exist_ok=True)
    edits, outfit = card(armor)
    tpl = os.path.join(CHARS, edits + ".png")
    wide = make_wide(tpl, os.path.join(out, "wide-" + edits + ".png"), WIDE)
    text = prompt(outfit, by_piece(outfit, out))
    if "--prompt" in flags:
        print("[attach %s]\n\n%s" % (wide, text))
        return
    with open(wide, "rb") as f:
        parts = [{"inline_data": {"mime_type": "image/png", "data": base64.b64encode(f.read()).decode()}}, {"text": text}]
    config = {"responseModalities": ["IMAGE"], "imageConfig": {"aspectRatio": "2:3", "imageSize": "2K"}}
    best = None
    for turn in range(1, tries + 1):
        raw, fitted = os.path.join(out, "%s.try%d.raw.png" % (armor, turn)), os.path.join(out, "%s.try%d.png" % (armor, turn))
        data = [p.get("inlineData") or p.get("inline_data") for p in ask(PAINTER, parts, config)]
        data = [d for d in data if d]
        if not data:
            print("gemini_armor: try %d came back with no picture" % turn)
            continue
        Image.open(io.BytesIO(base64.b64decode(data[0]["data"]))).convert("RGB").save(raw)
        verdict, missing = fit(armor, raw, tpl, wide, fitted)
        if missing:
            verdict = "REDO: " + "; ".join(missing)
        print("gemini_armor: try %d: %s" % (turn, verdict[:200]))
        score = (not verdict.startswith("GOOD"), verdict.count(";"))
        if best is None or score < best[0]:
            best = (score, fitted, verdict)
        if verdict.startswith("GOOD"):
            break
    if best is None:
        sys.exit("gemini_armor: nothing came back")
    final = os.path.join(out, armor + ".png")
    Image.open(best[1]).save(final)
    print("gemini_armor: %s is %s" % (final, "GOOD" if best[2].startswith("GOOD") else "the best of %d tries, but not good: %s" % (tries, best[2][:200])))
    if "--keep" in flags and best[2].startswith("GOOD"):
        Image.open(final).save(os.path.join(CHARS, "sheets", armor + ".png"))
        print("gemini_armor: kept as tools/source/characters/sheets/%s.png" % armor)
    sys.exit(0 if best[2].startswith("GOOD") else 1)


if __name__ == "__main__":
    main(sys.argv[1:])
