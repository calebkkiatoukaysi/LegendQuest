"""
Turns the painted front-facing portraits into the game's pixel-art conversation portraits.

    python tools/pixel/portrait_kit.py build            # every portrait: Content/Portraits, the manifest, the credits
    python tools/pixel/portrait_kit.py build sera kael  # just these (and their variants); the rest are kept
    python tools/pixel/portrait_kit.py review [ids...]  # contact sheets of every frame in shots/pixel/portraits/

The sources are ChatGPT's paintings in tools/source/pixel/portraits/<character>/front-facing/, one picture per
expression, named portrait-<id>.<expression>.png, 1024 x 1024 on flat #00FF00 green (docs/pixel-art-brief.md §4,
docs/pixel-art-portraits/). A subfolder of front-facing/ is a redesign the owner asked for (Valencia's
black-hair-red-eyes/), and its pictures replace the ones of the same name above it.

For each portrait id (a character, or one of their variants such as brannock-sling) it:

  1. keys the green out;
  2. picks one palette of 32 colours from all of that id's expressions together, so a colour doesn't flicker
     when the expression changes;
  3. shrinks each picture to a true 96 x 96 grid by majority colour per cell (never by averaging, which would
     blur), weighting the dark outline colours so one-pixel outlines survive; the paintings' own pixel grid
     drifts from 7 to 10 pixels because they were resized from 1254, so it doesn't try to lock onto it;
  4. writes Content/Portraits/<id>.png, one 96 x 96 row per expression in Dialogue/Expressions.cs's order, and
     Content/Portraits/portraits.json, which rows each sheet has and the speakers who use another's portrait
     (ALIASES: refugee-mother is Hedda).

The game animates them itself (Components/PortraitKit.cs): breathing, a bounce while the line types out, and a
pop when the expression changes. Blink and talking frames drawn from the face by finding the eyes and mouth were
tried and dropped (2026-11-25): the eyes are too varied (lashes, fringes, helms, masks, beards) to find reliably,
and a wrong blink is worse than none. Painted blink and talk frames are the way to add them.

It then rewrites its sections of Content/Content.mgcb and ASSETS.md. Needs numpy, scipy and Pillow.
Written by Caleb Kiatoukaysi for Legend Quest; public domain.
"""

import argparse
import glob
import json
import os
import sys

import numpy as np
from PIL import Image
from scipy import ndimage
from scipy.cluster.vq import kmeans2

ROOT = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", ".."))
SOURCE = os.path.join(ROOT, "tools", "source", "pixel", "portraits")
OUT = os.path.join(ROOT, "Content", "Portraits")
MANIFEST = os.path.join(OUT, "portraits.json")
REVIEW = os.path.join(ROOT, "shots", "pixel", "portraits")
MGCB = os.path.join(ROOT, "Content", "Content.mgcb")
ASSETS = os.path.join(ROOT, "ASSETS.md")

sys.path.insert(0, os.path.join(ROOT, "tools"))
from import_character import rewrite  # noqa: E402  (the same section writer the other importers use)

#: The art-pixel grid a portrait is converted to (docs/pixel-art-brief.md §1).
GRID = 96
#: Colours per portrait id, shared by all its expressions.
COLOURS = 32
#: How much more a dark colour counts in a cell's vote, so thin outlines aren't voted away.
DARK_WEIGHT = 1.6
DARK_LUMA = 70
#: A cell is background when more than this share of its picture pixels are green.
BACKGROUND_SHARE = 0.5
#: Every second picture pixel each way is enough to vote with, and four times quicker.
STRIDE = 2

#: Dialogue/Expressions.cs's order: a sheet's rows follow it, skipping the ones a portrait hasn't.
EXPRESSIONS = ["neutral", "smile", "grin", "laugh", "smirk", "sad", "worried", "afraid", "angry", "cold",
               "tired", "shock", "disgust"]


#: Speakers the scripts call by another id than their portrait's: {speaker id: portrait id}.
ALIASES = {"refugee-mother": "hedda"}

A_BEGIN = "<!-- conversation portraits: written by tools/pixel/portrait_kit.py; don't edit between these two lines -->"
A_END = "<!-- end conversation portraits -->"
M_BEGIN = "#------------ Portraits: written by tools/pixel/portrait_kit.py ------------#"
M_END = "#---------------------------- end Portraits ----------------------------#"

MGCB_TEXTURE = """#begin {path}
/importer:TextureImporter
/processor:TextureProcessor
/processorParam:ColorKeyColor=255,0,255,255
/processorParam:ColorKeyEnabled=False
/processorParam:GenerateMipmaps=False
/processorParam:PremultiplyAlpha=True
/processorParam:ResizeToPowerOfTwo=False
/processorParam:MakeSquare=False
/processorParam:TextureFormat=Color
/processorParam:Version=3.8.5.1
/build:{path}
"""


# --------------------------------------------------------------------------- #
# Finding the sources
# --------------------------------------------------------------------------- #

def sources():
    """{portrait id: {expression: path}}, a redesign's subfolder winning over the pictures above it."""
    found = {}
    for character in sorted(os.listdir(SOURCE)):
        folder = os.path.join(SOURCE, character, "front-facing")
        if not os.path.isdir(folder):
            continue
        # The front-facing folder first, then its subfolders, so a redesign overwrites what it replaces.
        paths = sorted(glob.glob(os.path.join(folder, "*.png")))
        paths += sorted(glob.glob(os.path.join(folder, "*", "*.png")))
        for path in paths:
            name = os.path.basename(path)
            if not name.startswith("portrait-"):
                continue
            stem = name[len("portrait-"):-len(".png")]
            pid, _, expression = stem.rpartition(".")
            if expression not in EXPRESSIONS:
                raise SystemExit("portrait_kit: %s is said with %s, which isn't one of Dialogue/Expressions.cs's"
                                 % (os.path.relpath(path, ROOT), expression))
            found.setdefault(pid, {})[expression] = path
    return found


# --------------------------------------------------------------------------- #
# Converting
# --------------------------------------------------------------------------- #

def green(a):
    """The flat green background, and the green fringe round it."""
    r, g, b = (a[..., i].astype(np.int16) for i in range(3))
    return (g > 140) & (g > r + 70) & (g > b + 70)


def palette(pictures, seed):
    """One palette for every expression of a portrait: k-means on their figures, away from the green's fringe."""
    rng = np.random.default_rng(seed)
    samples = []
    for a in pictures:
        near = ndimage.binary_dilation(green(a), iterations=3)
        px = a[~near].reshape(-1, 3)
        samples.append(px[rng.choice(len(px), min(len(px), 40000), replace=False)])
    data = np.concatenate(samples).astype(np.float64)
    colours, _ = kmeans2(data, COLOURS, minit="++", seed=seed, iter=30)
    return np.clip(np.round(colours), 0, 255)


def shrink(a, colours):
    """The picture as GRID x GRID cells, each its majority colour (dark ones weighted), or clear."""
    size = a.shape[0]
    sample = a[::STRIDE, ::STRIDE]
    flat = sample.reshape(-1, 3).astype(np.float64)
    labels = np.empty(len(flat), np.int32)
    for s in range(0, len(flat), 65536):
        d = ((flat[s:s + 65536, None, :] - colours[None]) ** 2).sum(axis=2)
        labels[s:s + 65536] = d.argmin(axis=1)
    labels = labels.reshape(sample.shape[:2])
    clear = len(colours)
    labels[green(sample)] = clear

    luma = colours @ np.array([0.3, 0.59, 0.11])
    weight = np.where(luma < DARK_LUMA, DARK_WEIGHT, 1.0)
    edges = np.linspace(0, size // STRIDE, GRID + 1).round().astype(int)
    out = np.zeros((GRID, GRID, 4), np.uint8)
    for y in range(GRID):
        for x in range(GRID):
            cell = labels[edges[y]:edges[y + 1], edges[x]:edges[x + 1]].ravel()
            counts = np.bincount(cell, minlength=clear + 1).astype(np.float64)
            if counts[clear] > counts.sum() * BACKGROUND_SHARE:
                continue
            votes = counts[:clear] * weight
            out[y, x, :3] = colours[votes.argmax()]
            out[y, x, 3] = 255
    return tidy(out)


def tidy(out):
    """Drops lone opaque cells floating in the clear, and fills lone clear holes inside the figure."""
    solid = out[..., 3] > 0
    neighbours = ndimage.convolve(solid.astype(np.int8), np.ones((3, 3), np.int8), mode="constant") - solid
    out[solid & (neighbours <= 1)] = 0
    hole = ~solid & (neighbours >= 7)
    for y, x in zip(*np.nonzero(hole)):
        around = out[max(0, y - 1):y + 2, max(0, x - 1):x + 2].reshape(-1, 4)
        around = around[around[:, 3] > 0]
        values, counts = np.unique(around, axis=0, return_counts=True)
        out[y, x] = values[counts.argmax()]
    return out


# --------------------------------------------------------------------------- #
# Building
# --------------------------------------------------------------------------- #

def build_one(pid, pictures, seed):
    expressions = [e for e in EXPRESSIONS if e in pictures]
    arrays = [np.asarray(Image.open(pictures[e]).convert("RGB")) for e in expressions]
    for e, a in zip(expressions, arrays):
        if a.shape[:2] != (1024, 1024):
            raise SystemExit("portrait_kit: %s is %dx%d, not 1024x1024" % (pictures[e], a.shape[1], a.shape[0]))
    colours = palette(arrays, seed)
    sheet = np.zeros((GRID * len(expressions), GRID, 4), np.uint8)
    for row, a in enumerate(arrays):
        sheet[row * GRID:(row + 1) * GRID] = shrink(a, colours)
    return expressions, sheet


def build(only):
    found = sources()
    chosen = found if not only else {pid: v for pid, v in found.items()
                                     if pid in only or pid.split("-")[0] in only}
    if only and not chosen:
        raise SystemExit("portrait_kit: no portraits called " + ", ".join(only))
    os.makedirs(OUT, exist_ok=True)
    manifest = {"grid": GRID, "portraits": {}}
    if os.path.isfile(MANIFEST):
        with open(MANIFEST, encoding="utf-8") as f:
            manifest["portraits"] = {k: v for k, v in json.load(f)["portraits"].items() if k in found}
    for pid in sorted(chosen):
        expressions, sheet = build_one(pid, chosen[pid], seed=7)
        Image.fromarray(sheet, "RGBA").save(os.path.join(OUT, pid + ".png"), optimize=True)
        manifest["portraits"][pid] = expressions
        print("%-22s %2d expressions" % (pid, len(expressions)))
    for stale in glob.glob(os.path.join(OUT, "*.png")):
        if os.path.basename(stale)[:-4] not in manifest["portraits"]:
            os.remove(stale)
    manifest["portraits"] = dict(sorted(manifest["portraits"].items()))
    manifest["aliases"] = dict(sorted(ALIASES.items()))
    with open(MANIFEST, "w", encoding="utf-8", newline="\n") as f:
        json.dump(manifest, f, indent=1)
        f.write("\n")
    register(sorted(manifest["portraits"]), found)


def register(ids, found):
    lines = [M_BEGIN, ""]
    for pid in ids:
        lines += MGCB_TEXTURE.format(path="Portraits/%s.png" % pid).rstrip("\n").split("\n") + [""]
    lines += ["#begin Portraits/portraits.json", "/copy:Portraits/portraits.json", "", M_END]
    rewrite(MGCB, M_BEGIN, M_END, lines, None)

    a = [A_BEGIN, "## Conversation portraits", "",
         "The pixel-art faces in the dialogue box, converted by tools/pixel/portrait_kit.py from the front-facing "
         "portraits supplied by Caleb Kiatoukaysi, made in ChatGPT (their sources and prompts are credited under "
         "tools/source/pixel/portraits above and recorded in docs/pixel-art-portraits/): keyed, given one 32-colour "
         "palette per portrait, shrunk to a 96 x 96 grid by majority colour. Released for Legend Quest by its owner.", ""]
    for pid in ids:
        expressions = [e for e in EXPRESSIONS if e in found.get(pid, {})]
        a.append("Content/Portraits/%s.png - %s's conversation portrait, one 96 x 96 face per expression: %s."
                 % (pid, pid, ", ".join(expressions)))
    a += ["", "Content/Portraits/portraits.json - which expressions each portrait sheet has, in row order; "
          "written by tools/pixel/portrait_kit.py.", "",
          "tools/pixel/portrait_kit.py - written by Caleb Kiatoukaysi for Legend Quest, converts the painted "
          "portraits above into the game's pixel-art portrait sheets; public domain.", A_END]
    rewrite(ASSETS, A_BEGIN, A_END, a, None)


# --------------------------------------------------------------------------- #
# Review
# --------------------------------------------------------------------------- #

def review(only):
    """A sheet per portrait with every expression side by side at x2, and one of every neutral (lineup.png)."""
    with open(MANIFEST, encoding="utf-8") as f:
        manifest = json.load(f)
    os.makedirs(REVIEW, exist_ok=True)
    scale, gap, ground = 2, 4, (40, 36, 52, 255)
    cell = GRID * scale
    neutrals = []
    for pid, expressions in manifest["portraits"].items():
        if only and pid not in only and pid.split("-")[0] not in only:
            continue
        sheet = Image.open(os.path.join(OUT, pid + ".png")).convert("RGBA")
        out = Image.new("RGBA", (len(expressions) * (cell + gap) - gap, cell), ground)
        for r in range(len(expressions)):
            face = sheet.crop((0, r * GRID, GRID, (r + 1) * GRID)).resize((cell, cell), Image.NEAREST)
            out.alpha_composite(face, (r * (cell + gap), 0))
            if r == 0:
                neutrals.append(face)
        out.save(os.path.join(REVIEW, pid + ".png"))
    per_row = 10
    lineup = Image.new("RGBA", (per_row * (cell + gap) - gap, -(-len(neutrals) // per_row) * (cell + gap) - gap), ground)
    for i, face in enumerate(neutrals):
        lineup.alpha_composite(face, ((i % per_row) * (cell + gap), (i // per_row) * (cell + gap)))
    lineup.save(os.path.join(REVIEW, "lineup.png"))
    print("review sheets in " + os.path.relpath(REVIEW, ROOT))


def main():
    parser = argparse.ArgumentParser(description=__doc__.split("\n\n")[0])
    parser.add_argument("mode", choices=["build", "review"])
    parser.add_argument("ids", nargs="*")
    args = parser.parse_args()
    if args.mode == "build":
        build(set(args.ids))
    else:
        review(set(args.ids))


if __name__ == "__main__":
    main()
