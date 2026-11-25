"""
Writes docs/pixel-art-batch-2-for-ChatGPT.md: pixel-art batch 2, everything ChatGPT still has to paint for the
pixel look, as ready-to-paste prompts.

    python tools/pixel/batch_2_prompts.py

Batch 1 (docs/pixel-art-batch-1.md) was the pilot; the portrait series (docs/pixel-art-portraits/) made 423 of the
conversation portraits, which are in the game. Batch 2 is the rest, in the same pixel-ready style:

  Part 1  the portraits still to paint: the rest of the series (from docs/pixel-art-portraits/progress.json's next
          number to the end of jobs.json), run with tools/pixel/portrait_sources.py exactly as before;
  Part 2  the characters in the world, pixelated: every painted source the game's figures are cut from (the two
          bodies, the armours painted whole, the face, hair and extra sheets, the cape, the held items), each
          redrawn in the pixel-ready style as an edit of itself, so every piece stays where the cutting tools
          expect it (docs/pixel-art-brief.md §3);
  Part 3  the rooms: each of the 74 background panels in the game (art/rooms/generated/<zone>/<room>/bg-<i>.png)
          redrawn pixel-ready as an edit of itself, then, for the open-air rooms, split into sky, distance and
          middle-ground layers (brief §7.2), and a foreground for every panel.

The style and avoid blocks are read from docs/pixel-art-brief.md §2.1 and §2.3, so they stay the ones the
portraits and batch 1 were painted with. Everything about a room comes from Story/14-background-prompts.md: its
name, panels, whether it's a street, interior or enclosed, its light, each panel's SCENE line, and the
foreground objects its zone file places (Story/14 §4); a panel §4 doesn't cover gets a suggestion from FOREGROUND
below. Only needs CPython. Written by Caleb Kiatoukaysi for Legend Quest; public domain.
"""

import glob
import json
import os
import re

ROOT = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", ".."))
BOOK = os.path.join(ROOT, "Story", "14-background-prompts.md")
BRIEF = os.path.join(ROOT, "docs", "pixel-art-brief.md")
PORTRAITS = os.path.join(ROOT, "docs", "pixel-art-portraits")
CHARACTERS = "tools/source/characters"
OUT = os.path.join(ROOT, "docs", "pixel-art-batch-2-for-ChatGPT.md")
SOURCES = "art/rooms/generated"
DROP_ROOMS = "tools/source/pixel/rooms"
DROP_PARTS = "tools/source/pixel/repaints"

#: Foregrounds for the panels Story/14 §4 doesn't place any on: {room: {panel: objects}}. Suggestions, not
#: zone-file pieces, so the game doesn't stand anything of its own there.
FOREGROUND = {
    "gulls-wager/deck": {
        1: "a coil of thick rope on a low wooden bitt, no taller than a sixth of the picture, at about 10% across",
        2: "a squat wooden water cask with iron hoops, no taller than a sixth of the picture, at about 45% across",
        3: "a coil of thick rope on a low wooden bitt, no taller than a sixth of the picture, at about 30% across",
    },
    "docks/fish-market": {
        1: "a slim wooden pole hung with a drying net and a string of small dried fish, rising out of the top of the "
           "picture, at about 10% across",
        2: "a squat black iron mooring bollard with a loop of rope thrown over it, no taller than a sixth of the "
           "picture, at about 55% across",
        3: "a tall weathered wooden post with a lit lantern hanging from an iron arm about two-thirds of the way up, "
           "at about 90% across",
    },
    "docks/pier": {
        1: "a thick weathered pier piling wound with rope and crusted with barnacles, rising out of the top of the "
           "picture, at about 6% across; a squat black iron mooring bollard, no taller than a sixth of the picture, "
           "at about 70% across",
    },
    "questeria/gate-plaza": {
        2: "a slender black wrought-iron street lamp post, its glass lantern lit warm about two-thirds of the way up "
           "the picture, at about 40% across",
        3: "a low stone planter box overflowing with pink and white flowers, no taller than a sixth of the picture, "
           "at about 70% across",
    },
    "questeria/lantern-market": {
        2: "a string of round paper lanterns in cream, pink and pale blue sagging down from the top edge across the "
           "top-left corner, from the left edge to about 25% across",
        3: "a slender black wrought-iron street lamp post, its glass lantern lit warm about two-thirds of the way up "
           "the picture, at about 85% across",
    },
    "questeria/sunspire-steps": {
        1: "a tall cream stone pillar topped with a gold sun finial, rising out of the top of the picture, at about "
           "5% across; a clipped round topiary bush in a stone urn, no taller than a quarter of the picture, at "
           "about 92% across",
    },
    "mirewood/edge": {
        2: "a clump of tall reeds and ferns, no taller than a fifth of the picture, at about 48% across",
    },
    "gilded-flagon/taproom": {
        1: "the end of a heavy dark oak ceiling beam across the top-left corner, an iron lantern hanging from it on "
           "a short chain, lit warm, at about 12% across",
        2: "the high back of a wooden settle, no taller than a fifth of the picture, at about 85% across",
    },
    "shop/inside": {
        1: "a bundle of candles and dried herbs hanging on a cord from the top edge, at about 8% across; the "
           "rounded side of a wooden barrel, no taller than a sixth of the picture, at about 94% across",
    },
    "forge/inside": {
        1: "a heavy iron chain with a hook hanging from the top edge, at about 10% across; a quenching tub of dark "
           "wood with iron hoops, no taller than a sixth of the picture, at about 90% across",
    },
    "bakery/inside": {
        1: "a rack of plaited loaves and dried herbs hanging from the top edge, at about 10% across; a flour sack "
           "slumped against the bottom edge, no taller than a sixth of the picture, at about 92% across",
    },
    "chapel/inside": {
        1: "a tall brass candle stand with three lit candles, at about 6% across; the carved end of a wooden pew, "
           "no taller than a fifth of the picture, at about 92% across",
    },
    "cottage-a/inside": {
        1: "a bunch of dried herbs hanging from the top edge, at about 8% across; a three-legged wooden stool, no "
           "taller than a sixth of the picture, at about 90% across",
    },
    "cottage-b/inside": {
        1: "a woven basket of red apples, no taller than a sixth of the picture, at about 8% across; a copper pot "
           "and ladle hanging from a beam hook at the top edge, at about 90% across",
    },
    "warehouse/inside": {
        1: "a heavy rope and pulley hook hanging from the top edge, at about 15% across; two stacked wooden crates, "
           "no taller than a fifth of the picture, at about 92% across",
    },
    "barrow/steps": {
        1: "a few tree roots trailing down from the top edge, at about 30% across; a tumbled standing stone carved "
           "with spirals, no taller than a fifth of the picture, at about 92% across",
    },
    "barrow/hall": {
        1: "a few tree roots trailing down from the top edge, at about 20% across",
        2: "a broken standing stone carved with spirals and rings, no taller than a fifth of the picture, at about "
           "50% across",
        3: "a few tree roots trailing down from the top edge, at about 75% across",
    },
    "barrow/side-room": {
        1: "a clay burial urn on a low stone block, no taller than a sixth of the picture, at about 10% across; a "
           "few tree roots trailing down from the top edge, at about 85% across",
    },
    "mirewood-quarry/lip": {
        1: "a rough timber prop post with an iron lantern hook, rising out of the top of the picture, at about 8% "
           "across",
        2: "a jagged rock split by black veins with a faint violet light in the cracks, no taller than a fifth of "
           "the picture, at about 45% across",
        3: "a rough timber prop post, rising out of the top of the picture, at about 90% across",
    },
    "mirewood-quarry/cut": {
        1: "a jagged rock spur split by black veins with a faint violet light, no taller than a fifth of the "
           "picture, at about 6% across; a rusted pick leaning on a stone, no taller than a sixth of the picture, "
           "at about 88% across",
    },
    "mirewood-quarry/floor": {
        1: "a cracked boulder split by black veins with a faint violet light, no taller than a fifth of the "
           "picture, at about 12% across",
        2: "a tall shard of black glassy crystal glowing violet at its tip, no taller than a quarter of the "
           "picture, at about 55% across",
        3: "a cracked boulder split by black veins with a faint violet light, no taller than a fifth of the "
           "picture, at about 88% across",
    },
    "thornbridge/camp-gate": {
        1: "a sharpened palisade stake bound with rope, rising out of the top of the picture, at about 5% across; "
           "a stack of split firewood, no taller than a sixth of the picture, at about 90% across",
    },
    "thornbridge/tents": {
        1: "a tent's guy rope slanting down out of the top-left corner to a wooden peg in the bottom edge at about "
           "8% across",
        2: "a wooden rack of spears, no taller than a quarter of the picture, at about 50% across",
        3: "an iron cooking tripod with a pot over a small fire, no taller than a fifth of the picture, at about 85% "
           "across",
    },
    "old-crown-vault/entrance": {
        1: "a fallen fluted column drum, no taller than a sixth of the picture, at about 10% across",
        2: "the edge of a tall dressed-stone pillar with a gold sun boss, rising out of the top of the picture, at "
           "about 52% across",
        3: "a bronze brazier on three legs with a low flame, no taller than a fifth of the picture, at about 90% "
           "across",
    },
    "old-crown-vault/halls": {
        1: "the edge of a tall dressed-stone pillar, rising out of the top of the picture, at about 6% across",
        2: "a bronze brazier on three legs with a low flame, no taller than a fifth of the picture, at about 50% "
           "across",
        3: "the edge of a tall dressed-stone pillar, rising out of the top of the picture, at about 94% across",
    },
    "old-crown-vault/strongroom": {
        1: "the corner of a stone relic plinth with a glass case on it, no taller than a fifth of the picture, at "
           "about 8% across",
        2: "the edge of a tall dressed-stone pillar with a gold sun boss, rising out of the top of the picture, at "
           "about 50% across",
        3: "the corner of a stone relic plinth with a glass case on it, no taller than a fifth of the picture, at "
           "about 92% across",
    },
    "old-crown-vault/treasury": {
        1: "a spill of gold coins round a small iron-bound chest, no taller than a sixth of the picture, at about 8% "
           "across; the edge of a tall dressed-stone pillar, rising out of the top of the picture, at about 94% "
           "across",
    },
    "sunspire-keep/great-hall": {
        1: "a tall navy banner with a gold sun hanging from the top edge, at about 6% across",
        2: "the edge of a great pale stone column, rising out of the top of the picture, at about 50% across",
        3: "a tall navy banner with a gold sun hanging from the top edge, at about 94% across",
    },
    "rusty-anvil/bar": {
        1: "a heavy dark beam across the top-left corner with an iron candle lantern hanging from it, lit, at about "
           "10% across; the back of a rickety wooden chair, no taller than a fifth of the picture, at about 90% "
           "across",
    },
}


# --------------------------------------------------------------------------- #
# Reading Story/14
# --------------------------------------------------------------------------- #

def read_book():
    with open(BOOK, encoding="utf-8") as f:
        text = f.read().replace("\r\n", "\n")
    rooms_part = text.split("\n## 3. The rooms", 1)[1].split("\n## 4.", 1)[0]
    fore_part = text.split("\n## 4. Foregrounds", 1)[1].split("\n## 5.", 1)[0]

    rooms, zone_name = [], None
    current = None
    panel = None
    for line in rooms_part.split("\n"):
        m = re.match(r"^### 3\.\d+ `([a-z0-9-]+)` · (.+)$", line)
        if m:
            zone_name = m.group(2).strip()
            continue
        m = re.match(r"^#### `([a-z0-9/-]+)` · (.+?) · #(\d+) · (P\d)$", line)
        if m:
            current = {"id": m.group(1), "name": m.group(2), "order": int(m.group(3)), "zone": zone_name,
                       "scenes": {}}
            rooms.append(current)
            continue
        m = re.match(r"^- \*\*Size:\*\* .*?(\d+) panels?.*?\)\. (Street, enclosed|Street|Interior)[^.]*\. (.+)$", line)
        if m and current:
            current["panels"] = int(m.group(1))
            current["kind"] = {"Street": "open", "Street, enclosed": "enclosed", "Interior": "interior"}[m.group(2)]
            current["light"] = m.group(3).strip()
            continue
        m = re.match(r"^##### `[a-z0-9/-]+` · panel (\d+) of \d+$", line)
        if m:
            panel = int(m.group(1))
            continue
        if line.startswith("SCENE") and current and panel:
            current["scenes"][panel] = re.sub(r"^SCENE(, left to right)?\.\s*", "", line).strip()

    placed = {}
    for block in re.split(r"\n#### ", fore_part)[1:]:
        m = re.match(r"`([a-z0-9/-]+)` · foreground for panel (\d+) of \d+", block)
        draw = re.search(r"^DRAW ONLY THESE, each standing up from the bottom edge: (.+)$", block, re.M)
        if m and draw:
            placed[(m.group(1), int(m.group(2)))] = draw.group(1).rstrip(".")
    return rooms, placed


# --------------------------------------------------------------------------- #
# The shared blocks
# --------------------------------------------------------------------------- #

def read_brief():
    """The pixel-ready style block (§2.1) and its avoid list (§2.3), as one paragraph each."""
    with open(BRIEF, encoding="utf-8") as f:
        text = f.read().replace("\r\n", "\n")

    def quoted(heading):
        part = text.split(heading, 1)[1].split("\n### ", 1)[0]
        lines = [line[2:] if line.startswith("> ") else line[1:] for line in part.split("\n") if line.startswith(">")]
        return " ".join(line.strip() for line in lines if line.strip())

    return quoted("### 2.1 "), quoted("### 2.3 ")


def png_size(path):
    with open(path, "rb") as f:
        head = f.read(24)
    return int.from_bytes(head[16:20], "big"), int.from_bytes(head[20:24], "big")


def fence(text):
    return ["```text", text, "```", ""]


NO = "No people, animals, text, letters, numbers, frame, border or watermark."

ROOM_STYLE = (
    "STYLE. An original background for a side-on 2D fantasy role-playing game's room, the scenery only, painted to "
    "be converted into detailed pixel art, so it must be made of clean, flat areas of colour with hard edges, like a "
    "master pixel artist's design blown up. Bold, even outlines in deep brown-violet, never pure black, on every "
    "building, prop, plank, rock and plant. Cel shading with hard-edged shapes only: for each material a base tone, "
    "two shadow tones that shift toward cool violet-blue, one highlight tone that shifts toward warm gold; the sky in "
    "a few broad, flat bands of colour rather than a smooth gradient. Detail shown with a few bold shapes: plank "
    "seams, roof tiles and cobbles as clear repeated shapes, rivets as dots, leaves as clumped scalloped masses, "
    "signs with pictures only. No brush texture, no noise, no grain, no soft airbrushing, no hairline detail. Rich "
    "high-fantasy colour: deep jewel tones, burnished gold and brass, warm lamplight against cool shadow. The "
    "distance is paler, cooler and simpler. Light from the upper left. The camera is side-on at a standing person's "
    "eye level, looking straight at the scene: the horizon straight and level, nothing vanishing into the distance.")

ROOM_AVOID = (
    "AVOID. People, faces, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, "
    "signatures, watermarks. Frames, borders, vignettes. Perspective: vanishing points, a view from above or below, a "
    "tilted horizon. Anything new on the open floor band. Painterly blur, smooth gradients, airbrushing, glossy "
    "3D-render shading, photorealism, grain, lens flare, dense noise or tiny texture. Pure-black outlines. Anything "
    "copied from an existing game, film, comic or brand.")


# --------------------------------------------------------------------------- #
# Part 1: the portraits
# --------------------------------------------------------------------------- #

def part_portraits():
    with open(os.path.join(PORTRAITS, "jobs.json"), encoding="utf-8") as f:
        jobs = json.load(f)
    with open(os.path.join(PORTRAITS, "progress.json"), encoding="utf-8") as f:
        progress = json.load(f)
    done = set(progress["completed_numbers"])
    left = [j for j in jobs if j["number"] not in done]
    groups = {}
    for job in left:
        groups.setdefault((job["character"], job["title"]), []).append(job)
    out = [
        "## Part 1. The portraits still to paint",
        "",
        f"**{len(left)} pictures, #{left[0]['number']} to #{left[-1]['number']}**, for {len(groups)} characters. "
        f"Pictures #1 to #{left[0]['number'] - 1} are painted and already in the game's dialogue box. The last ten "
        "characters (#627 on) are new: speakers the scripts use who had no card (the Trader, the smith, the Chandler, "
        "the innkeeper, the spice and flower sellers, the ship's lookout, the Red Gull thug, the royal messenger and "
        "guard).",
        "",
        "Carry on exactly as before. Every prompt is already written, in `docs/pixel-art-portraits/jobs.json` (and "
        "readable in `docs/pixel-art-portraits/ALL-PORTRAITS-for-ChatGPT.md` and each character's own file there):",
        "",
        "1. `python tools/pixel/portrait_sources.py ready` prints the next job: its prompt and the picture to attach "
        "(the template for a neutral, the character's own neutral for every expression).",
        "2. Paint it, one picture per message, a **new chat for each character**.",
        "3. `python tools/pixel/portrait_sources.py save <number> <png>` files it, keeps the original and credits it.",
        "4. After each character, `python tools/pixel/portrait_sources.py review <character>` makes its contact sheet; "
        "look at it before going on.",
        "",
        "Same look as the 423 before: front-facing, chunky 96 × 96 pixels on flat `#00FF00`, anime-flavoured faces, "
        "each expression an edit of that character's neutral.",
        "",
        "| # | Character | Pictures |",
        "| --- | --- | --- |",
    ]
    for (character, title), items in groups.items():
        faces = ", ".join(j["expression"] if j["kind"] != "variant" else j["filename"][len("portrait-"):-4]
                          for j in items)
        out.append(f"| {items[0]['number']}–{items[-1]['number']} | {title} (`{character}`) | {faces} |")
    out += ["", "When a character's set is saved, Claude converts it into the game with "
            "`python tools/pixel/portrait_kit.py build <character>`.", ""]
    return out, len(left)


# --------------------------------------------------------------------------- #
# Part 2: the characters in the world
# --------------------------------------------------------------------------- #

#: What Part 2 repaints: (heading, glob under tools/source/characters, what's in the picture).
PART_GROUPS = [
    ("The two bodies", "sheets/body-*.png",
     "the bare base body, in its pieces (head, torso, upper and lower arms and legs, hands and feet)"),
    ("Armours, painted whole", "whole/*.png",
     "an outfit, worn by the headless body standing arms out and legs apart"),
    ("Faces", "sheets/face-sheet-*.png", "faces, each on its bald head"),
    ("Hair", "sheets/hair-sheet-*.png", "hairstyles, each on its bald head"),
    ("Beards, ears, horns, jewellery and marks", "sheets/extra-sheet-*.png", "extras, each on its head"),
    ("Capes", "capes/*.png", "a cape, alone"),
    ("Held items", "weapons-alone/*.png", "a held item, alone and upright, its grip where it is"),
]


def part_block(style, avoid, what, size):
    w, h = size
    return (style + "\n\n" + avoid + "\n\n"
            f"SIZE. The attached picture is exactly {w} by {h} pixels. Give it back at exactly {w} by {h}, laid out "
            "exactly as it is: every piece, figure and item in exactly the same place, at exactly the same size and "
            "the same angle. Don't crop, pad, zoom, stretch, squash, rotate, flip, re-centre or re-frame it, add no "
            "border or frame, and draw nothing bigger, smaller, longer or shorter than it is. The background is "
            "solid black (#000000) everywhere outside the pieces, including anywhere the attached picture is "
            "transparent.\n\n"
            f"REPAINT. Redraw the attached picture ({what}) in the style above: the same design, the same colours, "
            "materials, trims and details, but rendered as bold, flat, hard-edged shapes with the outlines and cel "
            "shading described. Paint over the shapes that are there; don't add, remove or move anything, don't "
            "change any proportions, and keep any bare skin the warm tan it is (the game tints it to the wearer's "
            "skin).\n\nI'll save it under the same file name as the attached picture.")


def part_characters(style, avoid):
    out = [
        "## Part 2. The characters in the world, pixelated",
        "",
        "Every figure in the rooms and fights is put together from painted pieces on one skeleton (`docs/pixel-art-"
        "brief.md` §3): a body, an armour, a face, a hairstyle, extras, a cape and held items. Each picture below is "
        "redrawn **as an edit of itself** in the same pixel-ready style as the portraits and batch 1's Brannock "
        "armour, so every piece stays exactly where the cutting tools expect it and mix-and-match still works. "
        "Nothing about the skeleton, the animations or the customisation changes.",
        "",
        "**How:** one new chat per group (start a new one every eight pictures or so: long chats drift). For each "
        "file, attach it (from `tools/source/characters/`), paste the group's block, and save what comes back under "
        f"the **same file name** in `{DROP_PARTS}/`, keeping its folder (`whole/armor-sera.png` goes to "
        f"`{DROP_PARTS}/whole/armor-sera.png`). The originals stay where they are. Send Claude a group at a time: "
        "Claude converts them to true pixel art, cuts them with the same tools as now, and shows them in-game beside "
        "the old look before anything is replaced.",
        "",
        "**Check each one:** laid over the original it lines up exactly (no piece moved, grown or shrunk); the "
        "background is solid black; it reads as flat, hard-edged colour with brown-violet outlines, not soft paint.",
        "",
        "**Fix the size:**",
        "",
    ]
    out += fence("That moved or changed size. Do it again at exactly the attached picture's size, with every piece "
                 "exactly where it is in the attached picture, at the same size and angle. Don't crop, zoom or "
                 "re-centre. Everything else stays as it was.")
    out += ["**Fix the look:**", ""]
    out += fence("That's still too soft and detailed. Redo it with bigger, flatter areas of colour and hard edges: a "
                 "base tone, two shadow tones and one highlight per material, bold brown-violet outlines, no "
                 "gradients, texture or tiny detail. Keep every piece exactly where and how big it is.")
    total = 0
    for heading, pattern, what in PART_GROUPS:
        files = sorted(glob.glob(os.path.join(ROOT, CHARACTERS, pattern)))
        if not files:
            continue
        sizes = sorted({png_size(f) for f in files})
        total += len(files)
        names = [os.path.relpath(f, os.path.join(ROOT, CHARACTERS)).replace("\\", "/") for f in files]
        out += [f"### {heading} ({len(files)})", ""]
        for size in sizes:
            these = [n for n, f in zip(names, files) if png_size(f) == size]
            if len(sizes) > 1:
                out += [f"**{size[0]} × {size[1]}:**", ""]
            out += [", ".join(f"`{n}`" for n in these), ""]
            out += fence(part_block(style, avoid, what, size))
    return out, total


# --------------------------------------------------------------------------- #
# Part 3: the rooms
# --------------------------------------------------------------------------- #

def where(room):
    """The room's name as a sentence says it: "Saltwind Docks: The Pier", or just "The Smithy" when the zone is the room."""
    return room["name"] if room["zone"] == room["name"] else "%s: %s" % (room["zone"], room["name"])


def light(room):
    """The room's light in lower case, for the middle of a sentence."""
    text = re.sub(r";\s*(dark \(ambient [0-9.]+\)|tint [0-9 ]+)", "", room["light"]).rstrip(". ")
    return text[0].lower() + text[1:]


def whole_block(room, panel):
    scene = room["scenes"].get(panel)
    dark = "dark" in room["light"]
    return (ROOM_STYLE + "\n\n"
            f"ROOM. {where(room)}, panel {panel} of {room['panels']}; {light(room)}. The attached picture is this "
            "panel as it is in the game now.\n\n"
            "REDRAW. Redraw the attached picture in the style above. It is the same picture: every building, wall, "
            "door, gate, stair, window, tree, mast and rock, the horizon, the floor line and the open floor band "
            "stay exactly where they are, at exactly their size and shape, with the same colours and the same time "
            "of day. Don't add, remove or move anything; only the way it's drawn changes."
            + ("\n\nWHAT'S IN IT (from the room's first brief, for reference: wherever it and the attached picture "
               f"differ, the attached picture is right). {scene}" if scene else "")
            + ("\n\nLIGHT. The game darkens this room itself and adds its own glow to lamps, candles and fires, so "
               "keep it at clear, readable mid values, as in the attached picture." if dark else "")
            + "\n\nSIZE. Exactly 1536 by 1024 pixels, the whole scene edge to edge, like the attached picture. Don't "
            "crop, zoom, re-centre or re-frame it.\n\n" + ROOM_AVOID + f"\n\nI'll save it as bg-{panel}.png.")


LAYER_KEEP = ("Keep what stays exactly as it is in the attached picture, pixel for pixel: the same size (1536 by 1024), "
              "the same place, the same flat colours and outlines. Don't zoom, crop, re-centre or redraw anything you "
              "keep, and add nothing that isn't in the attached picture.")
MAGENTA = ("Everything you remove becomes perfectly flat #FF00FF magenta: one exact colour, with no shading, "
           "gradient, glow, outline or soft edge spilling into it; the edges of what you keep stay hard.")


def sky_block(room, panel):
    return (f"Edit the attached picture (bg-{panel}.png: {where(room)}, panel {panel} of {room['panels']}; "
            f"{light(room)}). Keep ONLY the sky: its bands of colour, clouds, sun or moon and stars. Take away "
            "everything that stands in front of the sky (buildings, roofs, towers, masts, rigging, trees, hills, the "
            "sea, the ground, the floor) and paint the sky on behind where they were, carrying its bands and clouds "
            "on naturally all the way down to the bottom edge, so the whole picture is sky with no hole, silhouette "
            "or outline of anything removed. " + LAYER_KEEP + " " + NO + f"\nI'll save it as bg-{panel}.sky.png.")


def far_block(room, panel):
    return (f"Edit the attached picture (bg-{panel}.png: {where(room)}, panel {panel} of {room['panels']}). Keep ONLY "
            "the distance: what sits far away on or near the horizon, such as the sea, far hills, the far tree line, "
            "distant towers, spires and rooftops, and the far bank of a river. Remove the sky above it and "
            "everything nearer than it (the buildings, walls, doors, near trees, masts, rails, props and the floor "
            "people walk on). Where something nearer hid part of the distance, carry the distance on naturally "
            "behind it, so it is one unbroken band across the whole width; below the horizon it may stop at the "
            "floor line. " + MAGENTA + " If this panel shows no distance at all, give back a picture that is flat "
            "magenta all over. " + LAYER_KEEP + " " + NO + f"\nI'll save it as bg-{panel}.far.png.")


def mid_block(room, panel):
    return (f"Edit the attached picture (bg-{panel}.png: {where(room)}, panel {panel} of {room['panels']}). Keep ONLY "
            "the middle ground: the buildings, walls, roofs, doors, gates, stairs, near trees, masts, rails, fences, "
            "signs, lamps and props of the scene, and the floor people walk on, down to the bottom edge. Remove the "
            "sky and the distance (the sea, far hills, the far tree line, distant towers and rooftops): every gap "
            "between roofs, chimneys, branches, leaves, rigging and railings where the sky or the distance shows "
            "through. Thin things such as ropes, rigging, railings and twigs stay, with magenta between them. "
            + MAGENTA + " " + LAYER_KEEP + " " + NO + f"\nI'll save it as bg-{panel}.mid.png.")


def fg_block(room, panel, objects):
    return ("Using the attached picture (bg-" + str(panel) + ".png) only as the reference for style, light, colours and "
            "scale (don't copy the scene itself), paint a NEW foreground overlay for it, the same size, 1536 by 1024 "
            f"pixels. Room: {where(room)}, panel {panel} of {room['panels']}; {light(room)}.\n"
            "DRAW ONLY THESE, the things nearest the camera, in front of where people walk: " + objects + ". "
            "Each one stands up from the very bottom edge of the picture (cut off by it, as if the camera stood just "
            "behind it) or, if it hangs, comes down from the very top edge, at the place given as a share of the "
            "picture's width, left to right. Draw it at the attached scene's scale (a person standing on its floor "
            "is about 185 pixels tall), in the same pixel-ready style: flat colours, hard edges, bold brown-violet "
            "outlines, a base tone with two violet-blue shadow tones and one gold highlight; a little darker and "
            "more saturated than the scene, because it's nearer. Nothing may stand in front of any door, gate or "
            "stair in the attached picture: if a place given is in front of one, move that thing sideways to the "
            "nearest clear spot. Keep them slim: together they cover no more than an eighth of the bottom third. "
            "Everything else is perfectly flat #FF00FF magenta, with no shading, gradient, glow or halo spilling "
            "into it, and no ground, floor, sky or scenery behind the objects. Side-on, no perspective. " + NO +
            f"\nI'll save it as bg-{panel}.fg.png.")


def card(title, attach, block):
    return [title, "", "- **Attach:** " + attach, ""] + fence(block)


def part_rooms():
    rooms, placed = read_book()
    pictures = 0
    out = [
        "## Part 3. The rooms: backgrounds and foregrounds",
        "",
        "Every room in the game is one flat painting today (the 74 panels in `art/rooms/generated/`, fitted into the "
        "37 rooms): nothing slides as the camera moves, and nothing stands in front of the characters. For each "
        "panel, in one chat per room:",
        "",
        "1. **The panel, pixel-ready** (`bg-<i>.png`): attach the panel as it is in the game "
        f"(`{SOURCES}/<zone>/<room>/bg-<i>.png`) and redraw it in the pixel-ready style, keeping everything exactly "
        "where it is, so the room's doors and floor still fit the game.",
        "2. **Its layers** (open-air rooms only): attach **your new `bg-<i>.png`** to each message and isolate the sky "
        "(`bg-<i>.sky.png`), the distance (`bg-<i>.far.png`) and the middle ground (`bg-<i>.mid.png`), so the sky and "
        "the distance drift slower than the street (`Story/12-rooms.md` §3.1). Interiors and underground rooms keep "
        "their back wall as the middle ground and skip this.",
        "3. **Its foreground** (`bg-<i>.fg.png`): the slim things nearest the camera that people walk behind.",
        "",
        f"Save every picture in `{DROP_ROOMS}/<zone>/<room>/` under the name its block ends with, and send Claude a "
        "room at a time. Claude keys the magenta, checks the layers against the panel, fits them with the room's "
        "existing measurements (`art/rooms/runtime-fit.json`), converts them to true pixel art and gives the game "
        "back its parallax.",
        "",
        "**Panels that overlap.** A wide room's panels overlap (panel 3 repeats most of panel 2). For panels 2 and 3, "
        "also attach your finished picture of the same kind for the panel before, and add: *\"Where this panel shows "
        "the same stretch as the second attached picture, make it match that one exactly.\"*",
        "",
        "**The deck** was painted pixel-ready once already, in batch 1, but from a new layout rather than the panel "
        "in the game; it's redone here from the game's panels so all three match. Batch 1's stays as the pilot.",
        "",
        "**Check each picture:** 1536 × 1024 and lined up with the panel (doors, horizon, floor line, rooftops); flat "
        "colour and hard edges; the sky layer all sky; the distance and middle ground together give back the whole "
        "panel; magenta exactly `#FF00FF`; the foreground only the listed things, none in front of a door; no people, "
        "animals or text anywhere.",
        "",
        "**Fixes** (paste in the same chat):",
        "",
    ]
    out += fence("That moved or changed size. Do it again at exactly 1536 by 1024 pixels, with everything exactly where "
                 "it is in the attached picture: don't zoom, crop or re-centre. Everything else stays as it was.")
    out += fence("Some of what I asked you to remove is still there (or a ghost of it): remove it completely and fill "
                 "the gap as asked. Keep everything else exactly the same.")
    out += fence("The magenta isn't flat: make every removed area exactly #FF00FF, one colour, with no gradient, glow or "
                 "soft edge, and keep the edges of what's left hard.")
    out += fence("That's still too soft and detailed. Redo it with bigger, flatter areas of colour and hard edges, bold "
                 "brown-violet outlines and the sky in flat bands. Keep everything exactly where and how big it is.")

    for room in rooms:
        kind = {"open": "open air", "enclosed": "underground", "interior": "interior"}[room["kind"]]
        out += ["---", "", f"### `{room['id']}` · {where(room)}", "",
                f"{room['panels']} panel{'s' if room['panels'] > 1 else ''}, {kind}; {light(room)}. "
                f"Folder: `{DROP_ROOMS}/{room['id']}/`.", ""]
        for panel in range(1, room["panels"] + 1):
            source = f"`{SOURCES}/{room['id']}/bg-{panel}.png` (the game's panel)"
            mine = f"your `bg-{panel}.png`"
            if room["panels"] > 1:
                out += [f"#### Panel {panel} of {room['panels']}", ""]
            out += card(f"**The panel, pixel-ready** · `bg-{panel}.png`", source, whole_block(room, panel))
            pictures += 1
            if room["kind"] == "open":
                out += card(f"**Sky** · `bg-{panel}.sky.png`", mine, sky_block(room, panel))
                out += card(f"**Distance** · `bg-{panel}.far.png`", mine, far_block(room, panel))
                out += card(f"**Middle ground** · `bg-{panel}.mid.png`", mine, mid_block(room, panel))
                pictures += 3
            objects = placed.get((room["id"], panel))
            note = "placed by the room's zone file"
            if objects is None:
                objects = FOREGROUND.get(room["id"], {}).get(panel)
                note = "a suggestion; change it freely"
            if objects:
                out += card(f"**Foreground** · `bg-{panel}.fg.png` ({note})", mine, fg_block(room, panel, objects))
                pictures += 1
    return out, pictures, len(rooms), sum(r["panels"] for r in rooms)


# --------------------------------------------------------------------------- #
# Writing the file
# --------------------------------------------------------------------------- #

def main():
    style, avoid = read_brief()
    portraits, n_portraits = part_portraits()
    characters, n_parts = part_characters(style, avoid)
    rooms, n_rooms, room_count, panel_count = part_rooms()
    total = n_portraits + n_parts + n_rooms
    out = [
        "# Pixel art, batch 2: everything left to paint (ready-to-paste ChatGPT prompts)",
        "",
        "<!-- Generated by tools/pixel/batch_2_prompts.py from docs/pixel-art-brief.md, docs/pixel-art-portraits/ "
        "and Story/14-background-prompts.md. Don't edit by hand: change those (or the script) and run it again. -->",
        "",
        "LegendQuest is moving to detailed fantasy pixel art (`docs/pixel-art-brief.md`). The conversation portraits "
        "came first and are in the game. This batch is the rest, **all in the same pixel-ready look as the portraits "
        "and batch 1**: flat areas of colour with hard edges, bold deep brown-violet outlines (never black), a base "
        "tone with two violet-blue shadow tones and one warm-gold highlight per material, no gradients, texture or "
        "tiny detail. Claude converts every picture to true pixel art on a fixed grid, so paint big and clean.",
        "",
        f"**{total} pictures:** {n_portraits} portraits (Part 1), {n_parts} character pieces (Part 2) and {n_rooms} "
        f"room pictures for {room_count} rooms and {panel_count} panels (Part 3).",
        "",
        "**Rules for every picture.** One picture per message. Original work only: no prompt names a game, studio or "
        "artist, and no other game's pictures are attached or copied. Return each picture at exactly the size its "
        "card says, laid out like what's attached; use the fix-ups when one comes back moved, cropped or soft. Save "
        "each under the name and folder the card gives: Claude's tools find them by name. Claude credits every "
        "picture in `ASSETS.md` as supplied by Caleb Kiatoukaysi, made in ChatGPT.",
        "",
        "**Order.** Part 1 first (it's already under way), then Part 3's open-air rooms in the order listed (the "
        "story reaches them in that order), then Part 2, bodies first, then the party's and the Prologue's armours. "
        "The game keeps its current look for anything not yet painted.",
        "",
        "**The pixel-ready style block and avoid list**, which every Part 2 block already contains "
        "(`docs/pixel-art-brief.md` §2.1 and §2.3):",
        "",
    ]
    out += fence(style)
    out += fence(avoid)
    out += portraits + characters + rooms
    while out and out[-1] == "":
        out.pop()
    with open(OUT, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(out) + "\n")
    print("%s: %d pictures (%d portraits, %d character pieces, %d room pictures)" % (
        os.path.relpath(OUT, ROOT), total, n_portraits, n_parts, n_rooms))


if __name__ == "__main__":
    main()
