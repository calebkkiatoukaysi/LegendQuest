# Background prompts: every room, for ChatGPT

The owner's request (2026-11-06): the art for every room's background, made in ChatGPT the way the characters are (`13-art-prompts.md`), in the clean, outlined fantasy look of the Lantern Market preview (`shots/art/rooms/lantern-market-preview.png`).

This file has a ready-to-paste block for every picture: **74 background panels** for the **37 rooms** of the **19 zones**, **22 optional foreground strips**, and **90 floor tiles**. Each block is complete on its own: the style, that picture's prompt and the avoid list, in one copy.

The rooms' layouts come from their zone files (`Content/Zones/*.txt`): door positions, kinds and names, the ways out, how dark a room is. What's in each scene comes from `12-rooms.md` §6, `03b-world-art.md`, the scripts, and the descriptions in the room-painting scripts (`tools/rooms/paint_rooms_*.py`). Anything worked out here without a source is listed on the room's **Derived** line. No prompt names a game, a studio or a brand; keep it that way when you edit one.

---

## 1. How to use this file

1. **One ChatGPT chat per room.** Make all of a room's panels in the same chat, in order, so the look stays the same from one to the next.
2. **Attach** the picture the entry names: the room's layout guide from `shots/art/room-templates/`. For panel 2 onward, **also attach your picture of the panel before it.**
3. **Paste the block**, all of it, in one go.
4. **Check it.** The doors sit in their boxes. The bottom third is plain, open ground. No people, animals, letters or numbers. The horizon is level. For panel 2 onward, its left side matches the right side of the panel before.
5. **Send the PNG to Claude in chat, with the file name the entry gives** (for example `docks-pier-1.png`). Claude imports it, credits it and checks it.

**Panel 3 of a wide room** repeats most of panel 2, because it's lined up with the room's right end; the game switches from panel 2 to panel 3 about halfway across panel 3. So panel 3's middle must match panel 2, and its right half is what shows. If it won't match, send it anyway and tell Claude.

Foregrounds (§4) are optional extras for some streets: lamp posts, trees and planters that stand in front of everyone. Floor tiles (§5) work the same way: attach nothing, paste the block, send the PNG with its name.

### 1.1 Fixing a picture

Ask for one fix at a time and say what stays: *"Keep everything else exactly the same; only ..."*

- **A door's in the wrong place or the wrong size.** *"Move the inn door so it fills the box labelled INN in the guide exactly, its bottom on the floor line. Keep everything else."* If it still won't move, try once more in a new message with only that instruction; then send the picture anyway and tell Claude which door is off.
- **Things on the floor.** *"Clear the bottom third: plain, even cobbles all the way across, nothing standing on them, no shadows."* Crates, barrels, carts and tables are the game's own pieces; it stands them there itself.
- **Letters or numbers appeared** (on signs, the guide's labels copied in). *"Remove all text: signs show only a picture."*
- **It's in perspective** (a street running away, a tilted horizon). Paste the style block's camera sentence again: *"Side-on at eye level, building fronts flat to us, a straight, level horizon."*
- **The panels don't match at the join.** Attach both pictures and say *"Redraw the left part of this picture to match the right part of the other one exactly."* If they still differ, send both and tell Claude.
- **Too dark or too gloomy** in a dark room. *"Brighter and clearer: mid values, readable colours. The game darkens it itself."*
- **People or animals crept in.** *"Remove every person and animal; keep everything else."*

---

## 2. The shared blocks

Every block in §3 to §5 already contains these. They're here for reference, and to edit: if you change one, ask Claude to update the blocks.

### 2.1 The background style block

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.
```

### 2.2 The background avoid list

```text
AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

### 2.3 The foreground style block and avoid list

```text
STYLE. An original foreground overlay for a side-on 2D fantasy game scene, in the same clean, outlined, vector-like style as the scene behind it: crisp outlines in deep brown to near-black, never pure black, flat fills with two or three cel tones and hard-edged shadow shapes, light from the upper left; but darker and a little more saturated than the scene, because these things stand nearest the camera. Draw only the objects listed below, each standing up from the very bottom edge of the picture at the place given (a share of the picture's width, left to right), cut off by the bottom edge as if the camera stood just behind them. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture at all. Tall things (a lamp post, a trunk) are slim, and together the objects cover no more than an eighth of the bottom third of the picture. Side-on at eye level, no perspective. The first attached picture is the layout guide and the second is the background this strip stands in front of: match its style and light, but don't copy the background itself, or the guide's colours, lines or labels. No people, animals, text, letters, numbers, frame or watermark. One 1536 by 1024 PNG.

AVOID. A background, sky, scenery, ground or floor behind the objects. Any shading, gradient, glow or halo spilling onto the empty area. Objects floating above the bottom edge. Wide bushes or hedges across the bottom. Extra objects not in the list. Perspective. People, animals, text, letters, logos, watermarks. Pixel art, painterly blur, glossy 3D-render shading. Pure-black outlines.
```

### 2.4 The tile style blocks and avoid list

Ground tiles (seamless textures):

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.
```

Overlay tiles (small things the game lays over the ground: flowers, bones, coins):

```text
STYLE. A small overlay for a floor tile in a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on. It is laid over a ground texture, so draw only the thing described below, centred, and keep it at least 64 px away from every edge unless the description says otherwise. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture. No ground under it, no cast shadow beyond a thin contact shadow.
```

Feature tiles (one picture that fills its square: a window, a pressure plate):

```text
STYLE. A square tile for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Flat on, with no perspective. It is one picture that fills the whole square edge to edge, so that it fits against the plain tiles beside it: no border, frame or vignette.
```

```text
AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

### 2.5 How doors look, by badge

The badge is the door's kind in the zone file (`12-rooms.md` §2.2), and it sets the box's size. The blocks name each door's kind and describe its look.

| Badge | Box (canvas px) | Drawn as |
| --- | --- | --- |
| `inn` | 120 × 200 | A broad arched oak door with iron straps, lanterns either side, a hanging sign with a tankard |
| `shop` | 120 × 200 | A glazed or half-glazed door by a bay window of wares, a hanging sign with a picture of what it sells |
| `anvil` | 120 × 200 | A wide planked door in sooty stone, a hanging sign with an anvil, a forge glow in the window |
| `sun` | 120 × 200 | A pointed-arch door in pale stone, a gold sun over it |
| `door` | 120 × 200 | A plain arched wooden door with a step; homes, the warehouse, the tomb |
| `gate` | 300 × 300 | A great door, gateway, portal or arch, as the room says: the City Gate, the barrow's portal, the vault doors |
| `crown` | 300 × 300 | The keep's great door: tall oak double doors with gold fittings under a sun |
| `stair-up`, `stair-down` | 160 × 120 | The foot of a stair climbing away, or the head of steps going down under an arch |

### 2.6 Looks that recur

These descriptions are written into every block that shows them, so the Sunspire and the city look the same from every room. Where the story doesn't describe them, they're derived from the current room paintings.

- **The Sunspire:** the royal keep's great tower on its hill above the city: a slender pale cream-and-gold stone spire with a tall azure cone tipped by a gold sun finial, smaller blue-coned turrets clustered round its foot, pink and navy pennants, a faint gold glow at its tip.
- **Questeria's houses:** cream dressed stone below and timber-framed or plastered above (cream, blush pink or pale blue), under steep roofs of scalloped tiles in azure, rose, teal or plum, with dormers, brick chimneys, gold finials, arched windows with warm glass, window boxes of pink and white flowers, ivy and climbing roses.
- **The Lantern Market's lanterns:** strings of round paper lanterns in cream, pink and pale blue looped between the eaves.
- **The Aurelian sun:** a round gold sun with long straight rays; the royal colours are navy and gold.
- **Black veins:** the rot from the quarry: thin black cracks in bark, stone and leaves with a faint violet light in them.

---

## 3. The rooms

Grouped by zone, in the order the story reaches them. Each room has a heading with its order in the index (§6) and its priority, a few lines on where its design comes from, and then one entry per panel. The game draws its own props, people and floor tiles over every painting; each room's **The game's own pieces** line says what it stands there, so the painting can leave room.

### 3.1 `gulls-wager` · The Gull's Wager

The merchant ship that brings you and {sb} to Fable, two days out, at night. Tiles: `ship`.

#### `gulls-wager/deck` · The Main Deck · #1 · P0

- **Size:** 3200 × 900, 3 panels (canvas x 0–1600, 1400–3000, 1600–3200). Street. Night at sea; dark (ambient 0.35).
- **Doors:** none. **Ways out:** none.
- **Used in:** P-04, P-05.
- **Basis:** `12-rooms.md` §6 (the main deck at sea, rigging, a long swell); `03b-world-art.md` §1 (deck at night, rail, mast, hanging lanterns, sea all round); the room's painting recipe (`paint_rooms_docks.py`, `deck`); the zone file (the bow at the left, the rail lamps, the game's own mainmast and wheel).
- **Derived:** the moon's place over the left of the room, the forecastle's and stern cabin's lengths, the fore and mizzen masts' places, the ensign.
- **The game's own pieces:** The game stands its own mainmast in the middle of the room, its ship's wheel near the stern, and its lanterns, barrels, crates and rope coils on the deck.

##### `gulls-wager/deck` · panel 1 of 3

- **Attach:** `shots/art/room-templates/gulls-wager-deck-1.png`.
- **Send to Claude as:** `gulls-wager-deck-1.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Gull's Wager: The Main Deck. This is panel 1 of 3, the left part of a room that scrolls sideways; the scene carries on past its right edge.

LAYOUT, as the attached guide shows it. The floor line, where every wall, doorstep, tree and rock meets the ground, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even deck planks running left to right. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person. The far horizon is a straight, level line about halfway down the picture (y 512 of 1024), at the eye height of someone standing on the floor; the distance (sea, hills, the far city) sits on it, and above the roofs and trees is sky.

DOORS. There is no door box in this panel, so nothing here should look like a door you could use, apart from the ways out named below; any building's own door stays small, plain and shut.

WAYS OUT. The room's left end is closed: end the scene at the left with a building, a wall, rocks or trees, as the scene says.

SCENE, left to right. The Gull's Wager, a trim merchant ship, two days out at sea at night. From the left edge to about 20%: the bow, the forecastle's raised deck behind the floor with a short ladder up to it, its rail sweeping up to the left, and the bowsprit angling up out of the top-left corner with its stays. Along the whole back of the floor runs the ship's far bulwark, about waist-high to a person: dark planks between stanchions under a varnished cap rail trimmed in navy and gold, pin rails hung with neatly coiled ropes, and a small brass lamp on a bracket at about 79%, lit. At about 50% the foremast rises from behind the bulwark out of the top of the picture, its sails furled tight on two yards, its shrouds and ratlines slanting down to the rail either side. Keep the right edge, around 95% to 100%, clear of masts: the game stands its own mainmast there. Over the bulwark: a calm, dark sea with long soft swells to a level horizon, and a full moon high at about 25% across, silver-white, its path of broken light lying on the water under it. Sky: deep navy shading to violet at the horizon, scattered stars, two or three long clouds edged in moonlight. Mood: quiet, lonely, a little uneasy; the sea is very big.

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from sky to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

##### `gulls-wager/deck` · panel 2 of 3

- **Attach:** `shots/art/room-templates/gulls-wager-deck-2.png`, and also attach your panel 1 picture (`gulls-wager-deck-1.png`).
- **Send to Claude as:** `gulls-wager-deck-2.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Gull's Wager: The Main Deck. This is panel 2 of 3, the middle part of a room that scrolls sideways; the scene carries on past its left and right edges.

CONTINUATION. The second attached picture is panel 1 of this room. This panel carries the scene on to the right of it: the left 12% of this picture (about 192 px) shows exactly the same stretch as the right 12% of panel 1, so redraw that stretch to match it (the same buildings, trees, sky, clouds, colours, horizon, floor line, scale and light) and carry on seamlessly from there. Only the right 1344 px are new.

LAYOUT, as the attached guide shows it. The floor line, where every wall, doorstep, tree and rock meets the ground, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even deck planks running left to right. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person. The far horizon is a straight, level line about halfway down the picture (y 512 of 1024), at the eye height of someone standing on the floor; the distance (sea, hills, the far city) sits on it, and above the roofs and trees is sky.

DOORS. There is no door box in this panel, so nothing here should look like a door you could use, apart from the ways out named below; any building's own door stays small, plain and shut.

SCENE, left to right. The middle of the ship's main deck at night. At about 12% the game stands its own mainmast: keep that spot to sky and bulwark only, though its rigging lines may cross the sky from the left. The far bulwark runs the whole width along the back of the floor: dark planks between stanchions, a varnished cap rail trimmed in navy and gold, pin rails with coiled ropes, and a small brass lamp on a bracket at about 34%, lit. From about 60% to 75% the mizzen mast rises behind the bulwark out of the top of the picture, its sails furled, its shrouds and ratlines slanting down either side. From about 85% the stern cabin begins: its front of painted planks in navy with gold trim, a door-less wall of small square windows glowing warm, a short ladder up to the quarterdeck above it. Over the bulwark: the dark, calm sea with long soft swells to a level horizon, a little moonlight on the crests. Sky: deep navy shading to violet at the horizon, stars, a long moonlit cloud.

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from sky to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

##### `gulls-wager/deck` · panel 3 of 3

- **Attach:** `shots/art/room-templates/gulls-wager-deck-3.png`, and also attach your panel 2 picture (`gulls-wager-deck-2.png`).
- **Send to Claude as:** `gulls-wager-deck-3.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Gull's Wager: The Main Deck. This is panel 3 of 3, the right part of a room that scrolls sideways; the scene carries on past its left edge.

CONTINUATION. The second attached picture is panel 2 of this room. This panel carries the scene on to the right of it: the left 88% of this picture (about 1344 px) shows exactly the same stretch as the right 88% of panel 2, so redraw that stretch to match it (the same buildings, trees, sky, clouds, colours, horizon, floor line, scale and light) and carry on seamlessly from there. Only the right 192 px are new.

LAYOUT, as the attached guide shows it. The floor line, where every wall, doorstep, tree and rock meets the ground, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even deck planks running left to right. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person. The far horizon is a straight, level line about halfway down the picture (y 512 of 1024), at the eye height of someone standing on the floor; the distance (sea, hills, the far city) sits on it, and above the roofs and trees is sky.

DOORS. There is no door box in this panel, so nothing here should look like a door you could use, apart from the ways out named below; any building's own door stays small, plain and shut.

WAYS OUT. The room's right end is closed: end the scene at the right with a building, a wall, rocks or trees, as the scene says.

SCENE, left to right. The stern end of the ship's main deck at night. Keep the left edge to sky and bulwark: the mainmast stands just off the left. The far bulwark runs along the back of the floor from the left to about 70%: dark planks, a navy-and-gold cap rail, pin rails with coiled ropes, a small brass lamp at about 21%, lit. From about 50% to 62% the mizzen mast rises out of the top of the picture, sails furled, shrouds and ratlines either side. From about 72% to the right edge: the stern cabin, its front of navy-painted planks with gold trim, a row of small square windows glowing warm, a ladder up to the quarterdeck, and on the quarterdeck rail at about 89% a big stern lantern, lit, with a navy ensign hanging from its staff above it. Over the bulwark to the left: the dark, calm sea to a level horizon. Sky: deep navy to violet, stars, moonlit cloud edges. The right end of the ship is closed: the stern cabin fills it.

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from sky to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```


### 3.2 `docks` · Saltwind Docks

Questeria's harbour, where the story lands: bright, salty, busy, a little rough. Tiles: `questeria`.

#### `docks/harbourside` · The Harbourside · #2 · P0

- **Size:** 3200 × 900, 3 panels (canvas x 0–1600, 1400–3000, 1600–3200). Street. Bright morning.
- **Doors:** inn *The Rusty Anvil* at x 441; door *A Warehouse* at x 2641. **Ways out:** west to The Pier; east to The Fish Market.
- **Used in:** P-05's end (landing), A1-C1-01 (the Red Gulls).
- **Basis:** `12-rooms.md` §6 (Tomas's net-rack, the customs house, the *Wager*'s berth with a gangway); `03b-world-art.md` §1 and §3 (fish stalls, net racks, the harbour bell); `paint_rooms_docks.py` (`harbourside`); the zone file (the Rusty Anvil's and the warehouse's doors, the ship and its gangplank as the game's props at the berth).
- **Derived:** the exact places of the nets, the customs house and the fishmonger's stall; the far coast's headland and lighthouse.
- **The game's own pieces:** The game moors its own ship, the Gull's Wager, at the berth (about x 1246) with its gangplank, and stands its fish stall, crates, a handcart and bollards on the quay.

##### `docks/harbourside` · panel 1 of 3

- **Attach:** `shots/art/room-templates/docks-harbourside-1.png`.
- **Send to Claude as:** `docks-harbourside-1.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. Saltwind Docks: The Harbourside. This is panel 1 of 3, the left part of a room that scrolls sideways; the scene carries on past its right edge.

LAYOUT, as the attached guide shows it. The floor line, where every wall, doorstep, tree and rock meets the ground, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even quay cobbles, with a plank boardwalk along the front. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person. The far horizon is a straight, level line about halfway down the picture (y 512 of 1024), at the eye height of someone standing on the floor; the distance (sea, hills, the far city) sits on it, and above the roofs and trees is sky.

DOORS. Draw each of these exactly inside its labelled box on the guide, filling the box, its threshold on the floor line (the names are only for you: write nothing): an inn door (The Rusty Anvil), from about 24% to 31% across, its top about 45% down. Any other door in this panel stays small, plain and shut, so these are clearly the ones to use.

WAYS OUT. The left edge is a way out, to The Pier: let the scene and the floor run on off the left edge.

SCENE, left to right. Saltwind Docks' harbourside on a bright, fresh morning. From the left edge to about 12%: a stack of cargo crates and barrels against a wall, and a black iron lamp post. From about 15% to 37%: the Rusty Anvil, a shabby dockside drinking den of soot-stained brick and tarred timber under a sagging plum-coloured tile roof, one upstairs window boarded across with planks, a crooked chimney, and a hanging sign with a rusty anvil on it; its inn door is in the box. From about 40% to 60%: Tomas's net rack, two weathered posts and a crossbeam with green-brown fishing nets hung over it to dry, cork floats on them, lobster pots stacked at its foot. From about 62% to the right edge: the open berth, the quay's edge of dressed coping stones with a squat mooring pile at about 64%, and bright harbour water behind it, open and empty where the game moors its own ship (about 75% to 100%): leave that water clear. Across the water in the distance: the harbour mole with a small beacon tower, two ships at anchor with furled sails, a green headland with a white lighthouse, pale mountains beyond. Sky: clear morning blue with big soft white clouds warming to pink near the horizon, the sun high at the upper left. Mood: busy, salty, cheerful, a little rough.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from sky to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

##### `docks/harbourside` · panel 2 of 3

- **Attach:** `shots/art/room-templates/docks-harbourside-2.png`, and also attach your panel 1 picture (`docks-harbourside-1.png`).
- **Send to Claude as:** `docks-harbourside-2.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. Saltwind Docks: The Harbourside. This is panel 2 of 3, the middle part of a room that scrolls sideways; the scene carries on past its left and right edges.

CONTINUATION. The second attached picture is panel 1 of this room. This panel carries the scene on to the right of it: the left 12% of this picture (about 192 px) shows exactly the same stretch as the right 12% of panel 1, so redraw that stretch to match it (the same buildings, trees, sky, clouds, colours, horizon, floor line, scale and light) and carry on seamlessly from there. Only the right 1344 px are new.

LAYOUT, as the attached guide shows it. The floor line, where every wall, doorstep, tree and rock meets the ground, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even quay cobbles, with a plank boardwalk along the front. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person. The far horizon is a straight, level line about halfway down the picture (y 512 of 1024), at the eye height of someone standing on the floor; the distance (sea, hills, the far city) sits on it, and above the roofs and trees is sky.

DOORS. Draw each of these exactly inside its labelled box on the guide, filling the box, its threshold on the floor line (the names are only for you: write nothing): a door (A Warehouse), from about 74% to 81% across, its top about 45% down. Any other door in this panel stays small, plain and shut, so these are clearly the ones to use.

SCENE, left to right. The middle of the harbourside on a bright morning. From the left edge to about 20%: the end of the berth, the quay's coping stones and open blue harbour water with a mooring pile at about 8% (the game's ship sits just off the left: keep the water plain). From about 25% to 48%: the customs house, a handsome two-storey building of cream dressed stone with a steep azure scalloped-tile roof and a dormer, a big round clock face with a gold rim between the upper windows (no numbers, just tick marks), navy-and-gold sun banners hanging either side of it, and a navy flag with a gold sun flying from a pole on the roof. From about 48% to 60%: a fishmonger's stall with a pink-and-cream striped awning, silver fish hung by their tails and laid on ice, and a pink blossom tree beside it. From about 64% to 92%: a tall teal-painted timber warehouse with a stone base, loft doors high in its gable under a projecting hoist beam with a block and rope, its door in the box. From about 92%: the quay and blue water again. Behind everything: the harbour, the mole with its beacon, ships at anchor, the far headland. Sky: clear morning blue, big soft clouds.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from sky to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

##### `docks/harbourside` · panel 3 of 3

- **Attach:** `shots/art/room-templates/docks-harbourside-3.png`, and also attach your panel 2 picture (`docks-harbourside-2.png`).
- **Send to Claude as:** `docks-harbourside-3.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. Saltwind Docks: The Harbourside. This is panel 3 of 3, the right part of a room that scrolls sideways; the scene carries on past its left edge.

CONTINUATION. The second attached picture is panel 2 of this room. This panel carries the scene on to the right of it: the left 88% of this picture (about 1344 px) shows exactly the same stretch as the right 88% of panel 2, so redraw that stretch to match it (the same buildings, trees, sky, clouds, colours, horizon, floor line, scale and light) and carry on seamlessly from there. Only the right 192 px are new.

LAYOUT, as the attached guide shows it. The floor line, where every wall, doorstep, tree and rock meets the ground, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even quay cobbles, with a plank boardwalk along the front. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person. The far horizon is a straight, level line about halfway down the picture (y 512 of 1024), at the eye height of someone standing on the floor; the distance (sea, hills, the far city) sits on it, and above the roofs and trees is sky.

DOORS. Draw each of these exactly inside its labelled box on the guide, filling the box, its threshold on the floor line (the names are only for you: write nothing): a door (A Warehouse), from about 61% to 69% across, its top about 45% down. Any other door in this panel stays small, plain and shut, so these are clearly the ones to use.

WAYS OUT. The right edge is a way out, to The Fish Market: let the scene and the floor run on off the right edge.

SCENE, left to right. The east end of the harbourside on a bright morning. From the left edge to about 12%: open blue harbour water behind the quay's coping stones. From about 13% to 36%: the customs house, cream dressed stone under a steep azure scalloped-tile roof, a round gold-rimmed clock face between the upper windows (tick marks only), navy-and-gold sun banners, a navy flag with a gold sun on the roof. From about 36% to 48%: a fishmonger's stall with a pink-and-cream striped awning and fish hung by their tails, a pink blossom tree beside it. From about 51% to 78%: the tall teal timber warehouse on a stone base, loft doors high in its gable under a hoist beam with a block and rope, its door in the box. From about 80% to the right edge: the quay's edge and the harbour again, a small fishing boat with a blue-and-white hull and a furled red sail moored at about 90%, the mole and its beacon behind. The scene runs on off the right edge toward the fish market. Sky: clear morning blue with soft clouds.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from sky to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

#### `docks/fish-market` · The Fish Market · #3 · P0

- **Size:** 3200 × 900, 3 panels (canvas x 0–1600, 1400–3000, 1600–3200). Street. Bright morning.
- **Doors:** shop *The Chandler's* at x 1258. **Ways out:** east to Questeria; west to The Harbourside.
- **Used in:** Chapter 1, the way from the docks up to the city; the Chandler's.
- **Basis:** `12-rooms.md` §6 (stalls, ice, a crane hoisting crates); `paint_rooms_docks.py` (`fish_market`); the zone file (the Chandler's door, the watch post and lamps in the painter's list, the exit up to Questeria).
- **Derived:** the crane's and stalls' exact places, the ice store, the stair climbing toward the city.
- **The game's own pieces:** The game stands its own net rack, barrels, crates, a chalkboard and a signpost on the quay.

##### `docks/fish-market` · panel 1 of 3

- **Attach:** `shots/art/room-templates/docks-fish-market-1.png`.
- **Send to Claude as:** `docks-fish-market-1.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. Saltwind Docks: The Fish Market. This is panel 1 of 3, the left part of a room that scrolls sideways; the scene carries on past its right edge.

LAYOUT, as the attached guide shows it. The floor line, where every wall, doorstep, tree and rock meets the ground, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even quay cobbles. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person. The far horizon is a straight, level line about halfway down the picture (y 512 of 1024), at the eye height of someone standing on the floor; the distance (sea, hills, the far city) sits on it, and above the roofs and trees is sky.

DOORS. Draw each of these exactly inside its labelled box on the guide, filling the box, its threshold on the floor line (the names are only for you: write nothing): a shop door (The Chandler's), from about 75% to 82% across, its top about 45% down. Any other door in this panel stays small, plain and shut, so these are clearly the ones to use.

WAYS OUT. The left edge is a way out, to The Harbourside: let the scene and the floor run on off the left edge.

SCENE, left to right. Saltwind Docks' fish market on a bright morning. From the left edge to about 5%: barrels against a wall. From about 8% to 32%: a tall, narrow townhouse washed blush pink, cream stone below, a rose scalloped-tile roof with a dormer, window boxes of flowers. From about 35% to 66%: the market on the quay, with the harbour's blue water behind the quay's edge: a fish stall with a blue-and-white striped awning at about 40%, a timber harbour crane at about 52% standing on the quay's edge, its jib reaching out over the water and lifting a net of crates on a rope from a low cargo boat moored behind, and a second fish stall with a pink-and-cream awning at about 62%, fish hung by their tails and laid on ice. From about 68% to 88%: the Chandler's, a ship's chandlery in blue-grey stone with a navy-painted shopfront, a bay window full of brass lanterns, coils of rope and tins, and a hanging sign with a gold anchor; its shop door is in the box. From about 88%: the corner of the ice store. Behind it all: the harbour, masts of moored boats, the far headland. Sky: clear morning blue with soft clouds, the sun high at the upper left. Mood: loud, busy, smelling of salt and fish.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from sky to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

##### `docks/fish-market` · panel 2 of 3

- **Attach:** `shots/art/room-templates/docks-fish-market-2.png`, and also attach your panel 1 picture (`docks-fish-market-1.png`).
- **Send to Claude as:** `docks-fish-market-2.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. Saltwind Docks: The Fish Market. This is panel 2 of 3, the middle part of a room that scrolls sideways; the scene carries on past its left and right edges.

CONTINUATION. The second attached picture is panel 1 of this room. This panel carries the scene on to the right of it: the left 12% of this picture (about 192 px) shows exactly the same stretch as the right 12% of panel 1, so redraw that stretch to match it (the same buildings, trees, sky, clouds, colours, horizon, floor line, scale and light) and carry on seamlessly from there. Only the right 1344 px are new.

LAYOUT, as the attached guide shows it. The floor line, where every wall, doorstep, tree and rock meets the ground, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even quay cobbles. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person. The far horizon is a straight, level line about halfway down the picture (y 512 of 1024), at the eye height of someone standing on the floor; the distance (sea, hills, the far city) sits on it, and above the roofs and trees is sky.

DOORS. There is no door box in this panel, so nothing here should look like a door you could use, apart from the ways out named below; any building's own door stays small, plain and shut.

SCENE, left to right. The east half of the fish market, where the street starts to climb toward the city. From the left edge to about 22%: the ice store, a squat building of pale stone with a wide serving hatch open on a cold blue-white inside, blocks of ice stacked in it, a little frost on the sill. From about 22% to 33%: a rack of fish drying in the wind, posts and rails with pairs of fish hung by their tails. At about 36%: the Watch post, a small square guardhouse of cream stone with a navy-and-gold shield on its front, a lantern and a navy pennant. From about 44% to 60%: a low garden wall of cream stone with a pink blossom tree leaning over it at about 55% and white flowers at its foot. From about 60% to the right edge: the street climbing away to the right behind the floor, a broad flight of pale stone steps rising behind a low balustrade, black iron lamp posts at about 64% and 88%, a pink flowering bush at about 78%. Above and beyond, on its hill in the far distance: Questeria, cream walls and colourful roofs climbing up the slope, crowned by the Sunspire, the royal keep's great tower on its hill above the city: a slender pale cream-and-gold stone spire with a tall azure cone tipped by a gold sun finial, smaller blue-coned turrets clustered round its foot, pink and navy pennants, a faint gold glow at its tip. Sky: clear morning blue with soft clouds.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from sky to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

##### `docks/fish-market` · panel 3 of 3

- **Attach:** `shots/art/room-templates/docks-fish-market-3.png`, and also attach your panel 2 picture (`docks-fish-market-2.png`).
- **Send to Claude as:** `docks-fish-market-3.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. Saltwind Docks: The Fish Market. This is panel 3 of 3, the right part of a room that scrolls sideways; the scene carries on past its left edge.

CONTINUATION. The second attached picture is panel 2 of this room. This panel carries the scene on to the right of it: the left 88% of this picture (about 1344 px) shows exactly the same stretch as the right 88% of panel 2, so redraw that stretch to match it (the same buildings, trees, sky, clouds, colours, horizon, floor line, scale and light) and carry on seamlessly from there. Only the right 192 px are new.

LAYOUT, as the attached guide shows it. The floor line, where every wall, doorstep, tree and rock meets the ground, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even quay cobbles. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person. The far horizon is a straight, level line about halfway down the picture (y 512 of 1024), at the eye height of someone standing on the floor; the distance (sea, hills, the far city) sits on it, and above the roofs and trees is sky.

DOORS. There is no door box in this panel, so nothing here should look like a door you could use, apart from the ways out named below; any building's own door stays small, plain and shut.

WAYS OUT. The right edge is a way out, to Questeria: let the scene and the floor run on off the right edge.

SCENE, left to right. The east end of the fish market, climbing toward the city. From the left edge to about 9%: the ice store's corner, its open hatch with blocks of ice. From about 9% to 21%: a rack of fish drying, posts and rails with pairs of fish. At about 24%: the Watch post, a small square guardhouse of cream stone with a navy-and-gold shield, a lantern and a navy pennant. From about 30% to 50%: a low cream garden wall with a pink blossom tree leaning over it at about 43%. From about 50% to the right edge: a broad flight of pale stone steps climbing away to the right behind a low balustrade, black iron lamp posts at about 51%, 75% and 81%, a pink flowering bush at about 66%, pink flowers at about 93%. High beyond, filling the far right: Questeria on its hill, cream walls and roofs of azure, rose and teal climbing the slope, crowned by the Sunspire, the royal keep's great tower on its hill above the city: a slender pale cream-and-gold stone spire with a tall azure cone tipped by a gold sun finial, smaller blue-coned turrets clustered round its foot, pink and navy pennants, a faint gold glow at its tip. The right edge is the way up into the city. Sky: clear morning blue with soft clouds.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from sky to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

#### `docks/pier` · The Pier · #4 · P0

- **Size:** 1600 × 900, 1 panel (canvas x 0–1600). Street. Bright morning.
- **Doors:** door *A Cottage* at x 401; door *A Warehouse* at x 1030. **Ways out:** east to The Harbourside.
- **Used in:** side room off the harbourside (Netty, Maud).
- **Basis:** `12-rooms.md` §6 (the long pier, boats moored, gulls on every post); `paint_rooms_docks.py` (`pier`); the zone file (Netty's cottage door, the warehouse door, the painter's oak, cottage and warehouse).
- **Derived:** the dinghy, the mole's beacon and the lighthouse.
- **The game's own pieces:** The game stands its own bollards, harbour bell, net rack, washing line, crates, barrels and lamp posts on the quay, and adds the gulls.

##### `docks/pier` · panel 1 of 1

- **Attach:** `shots/art/room-templates/docks-pier-1.png`.
- **Send to Claude as:** `docks-pier-1.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. Saltwind Docks: The Pier. This one picture is the whole room.

LAYOUT, as the attached guide shows it. The floor line, where every wall, doorstep, tree and rock meets the ground, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even quay cobbles and boardwalk planks. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person. The far horizon is a straight, level line about halfway down the picture (y 512 of 1024), at the eye height of someone standing on the floor; the distance (sea, hills, the far city) sits on it, and above the roofs and trees is sky.

DOORS. Draw each of these exactly inside its labelled box on the guide, filling the box, its threshold on the floor line (the names are only for you: write nothing): a door (A Cottage), from about 21% to 29% across, its top about 45% down; a door (A Warehouse), from about 61% to 68% across, its top about 45% down. Any other door in this panel stays small, plain and shut, so these are clearly the ones to use.

WAYS OUT. The room's left end is closed: end the scene at the left with a building, a wall, rocks or trees, as the scene says. The right edge is a way out, to The Harbourside: let the scene and the floor run on off the right edge.

SCENE, left to right. The pier at Saltwind Docks on a bright morning. The left end of the room is closed: start it with a big leafy oak at about 0% to 20%, its trunk just off the left edge and its round crown arching over. From about 15% to 35%: Netty's fisherman's cottage, a small, snug house of teal-painted boards on a stone base under a grey-blue slate roof with a stubby chimney, a fishing net hung along its wall on pegs, lobster pots stacked by the door, a window box of red geraniums; its house door is in the box. From about 36% to 58%: the open harbour behind the quay's edge, a red-and-cream fishing boat with a furled sail and a little rowing dinghy moored to tall wooden mooring posts (leave the posts' tops empty: the game adds the gulls). From about 58% to 80%: the big warehouse, a tall barn-like building of weathered timber on a stone base with a hoist beam and rope at its gable, loft doors open; its door is in the box. From about 80% to the right edge: the harbour running out east to a long stone mole with a small beacon tower, and beyond it a green headland with a white lighthouse. The scene runs on off the right edge to the harbourside. Sky: clear morning blue with soft clouds, the sun high at the upper left. Mood: peaceful, salty, homely.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from sky to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```


### 3.3 `questeria` · Questeria

The capital: cream stone and timber, roofs of azure, rose, teal and plum, flowers everywhere, the Sunspire over it all. The hub of Act 1. Tiles: `questeria`.

#### `questeria/gate-plaza` · The Gate Plaza · #5 · P0

- **Size:** 3200 × 900, 3 panels (canvas x 0–1600, 1400–3000, 1600–3200). Street. Bright midday.
- **Doors:** door *A Turret House* at x 500; sun *The Chapel of the Dawn* at x 880; door *A Cottage* at x 1260; shop *The Chandler's* at x 1700; door *A Cottage* at x 2160; door *A Cottage* at x 2620. **Ways out:** west to Saltwind Docks; east to The Festival Square.
- **Used in:** arriving in the city ({sb}'s first look at it); homes, the Chapel, the Chandler's.
- **Basis:** `12-rooms.md` §6 (the city gate and the Watch's notice board); `paint_rooms_questeria.py` (`gate_plaza`); the zone file (six doors, the painter's houses, chapel, watch post, well and benches).
- **Derived:** the harbour seen through the gate, the bell tower's shape, the arch at the east end.
- **The game's own pieces:** The game stands its own signpost, flower beds, a cypress and a washing line on the cobbles.

##### `questeria/gate-plaza` · panel 1 of 3

- **Attach:** `shots/art/room-templates/questeria-gate-plaza-1.png`.
- **Send to Claude as:** `questeria-gate-plaza-1.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. Questeria: The Gate Plaza. This is panel 1 of 3, the left part of a room that scrolls sideways; the scene carries on past its right edge.

LAYOUT, as the attached guide shows it. The floor line, where every wall, doorstep, tree and rock meets the ground, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even grey cobbles. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person. The far horizon is a straight, level line about halfway down the picture (y 512 of 1024), at the eye height of someone standing on the floor; the distance (sea, hills, the far city) sits on it, and above the roofs and trees is sky.

DOORS. Draw each of these exactly inside its labelled box on the guide, filling the box, its threshold on the floor line (the names are only for you: write nothing): a door (A Turret House), from about 28% to 35% across, its top about 45% down; a temple door (The Chapel of the Dawn), from about 51% to 59% across, its top about 45% down; a door (A Cottage), from about 75% to 82% across, its top about 45% down. Any other door in this panel stays small, plain and shut, so these are clearly the ones to use.

WAYS OUT. The left edge is a way out, to Saltwind Docks: let the scene and the floor run on off the left edge.

SCENE, left to right. Questeria's gate plaza, just inside the city's harbour gate, at bright midday. Questeria's houses are cream dressed stone below and timber-framed or plastered above (cream, blush pink or pale blue), under steep roofs of scalloped tiles in azure, rose, teal or plum, with dormers, brick chimneys, gold finials, arched windows with warm glass, window boxes of pink and white flowers, ivy and climbing roses. From the left edge to about 12%: the inside of the harbour gate, a tall arch in the cream city wall, open, with a glimpse of the harbour, masts and blue water through it. From about 12% to 24%: the Watch's tower, a square cream-stone tower with a navy-and-gold shield and a navy pennant, a notice board under it with pinned papers (blank or with tiny scribble marks only), an iron bench and pink flowers at its foot. From about 25% to 38%: a turret house with a round corner turret under a teal cone roof; its house door is in the box. From about 42% to 68%: the Chapel of the Dawn, small and lovely, pink-washed stone with a pointed-arch door with a gold sun over it (the temple door in the box), a round rose window of rose, gold and blue glass above, and a slim bell tower with a little bell in an open arch; a planter of pink flowers at about 69%. From about 72% to 92%: the ivy house, cream stone almost covered in ivy, a blue roof; its house door is in the box; a black iron street lamp at about 90%. At about 97%: a little roofed well with a water barrel. Far beyond the roofs: the Sunspire, the royal keep's great tower on its hill above the city: a slender pale cream-and-gold stone spire with a tall azure cone tipped by a gold sun finial, smaller blue-coned turrets clustered round its foot, pink and navy pennants, a faint gold glow at its tip, small and pale over the right half. Sky: bright blue with big soft clouds going pink low down, the sun high at the upper left. Mood: homely, sunny, welcoming.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from sky to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

##### `questeria/gate-plaza` · panel 2 of 3

- **Attach:** `shots/art/room-templates/questeria-gate-plaza-2.png`, and also attach your panel 1 picture (`questeria-gate-plaza-1.png`).
- **Send to Claude as:** `questeria-gate-plaza-2.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. Questeria: The Gate Plaza. This is panel 2 of 3, the middle part of a room that scrolls sideways; the scene carries on past its left and right edges.

CONTINUATION. The second attached picture is panel 1 of this room. This panel carries the scene on to the right of it: the left 12% of this picture (about 192 px) shows exactly the same stretch as the right 12% of panel 1, so redraw that stretch to match it (the same buildings, trees, sky, clouds, colours, horizon, floor line, scale and light) and carry on seamlessly from there. Only the right 1344 px are new.

LAYOUT, as the attached guide shows it. The floor line, where every wall, doorstep, tree and rock meets the ground, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even grey cobbles. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person. The far horizon is a straight, level line about halfway down the picture (y 512 of 1024), at the eye height of someone standing on the floor; the distance (sea, hills, the far city) sits on it, and above the roofs and trees is sky.

DOORS. Draw each of these exactly inside its labelled box on the guide, filling the box, its threshold on the floor line (the names are only for you: write nothing): a shop door (The Chandler's), from about 15% to 22% across, its top about 45% down; a door (A Cottage), from about 44% to 51% across, its top about 45% down; a door (A Cottage), from about 72% to 80% across, its top about 45% down. Any other door in this panel stays small, plain and shut, so these are clearly the ones to use.

SCENE, left to right. The middle of Questeria's gate plaza at bright midday. Questeria's houses are cream dressed stone below and timber-framed or plastered above (cream, blush pink or pale blue), under steep roofs of scalloped tiles in azure, rose, teal or plum, with dormers, brick chimneys, gold finials, arched windows with warm glass, window boxes of pink and white flowers, ivy and climbing roses. From the left edge to about 5%: the end of the ivy house and a black iron lamp. From about 6% to 13%: a little roofed well and a water barrel. From about 13% to 26%: the Chandler's, a narrow shop with a navy-painted front, a bay window glowing with rows of candles, and a hanging sign with a candle on it; its shop door is in the box. From about 30% to 60%: the rose house, a broad house of rose-pink plaster over cream stone with climbing roses round its door and windows; its house door is in the box; an iron bench at about 64% and a long planter of flowers at about 69%. From about 70% to 92%: the awning house, cream with a striped rose-and-white awning over a ground-floor window and a plum roof; its house door is in the box; a cask at about 95%. Beyond the roofs: the Sunspire, the royal keep's great tower on its hill above the city: a slender pale cream-and-gold stone spire with a tall azure cone tipped by a gold sun finial, smaller blue-coned turrets clustered round its foot, pink and navy pennants, a faint gold glow at its tip rising pale and tall. Sky: bright blue with soft clouds going pink low down.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from sky to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

##### `questeria/gate-plaza` · panel 3 of 3

- **Attach:** `shots/art/room-templates/questeria-gate-plaza-3.png`, and also attach your panel 2 picture (`questeria-gate-plaza-2.png`).
- **Send to Claude as:** `questeria-gate-plaza-3.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. Questeria: The Gate Plaza. This is panel 3 of 3, the right part of a room that scrolls sideways; the scene carries on past its left edge.

CONTINUATION. The second attached picture is panel 2 of this room. This panel carries the scene on to the right of it: the left 88% of this picture (about 1344 px) shows exactly the same stretch as the right 88% of panel 2, so redraw that stretch to match it (the same buildings, trees, sky, clouds, colours, horizon, floor line, scale and light) and carry on seamlessly from there. Only the right 192 px are new.

LAYOUT, as the attached guide shows it. The floor line, where every wall, doorstep, tree and rock meets the ground, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even grey cobbles. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person. The far horizon is a straight, level line about halfway down the picture (y 512 of 1024), at the eye height of someone standing on the floor; the distance (sea, hills, the far city) sits on it, and above the roofs and trees is sky.

DOORS. Draw each of these exactly inside its labelled box on the guide, filling the box, its threshold on the floor line (the names are only for you: write nothing): a shop door (The Chandler's), from about 2% to 10% across, its top about 45% down; a door (A Cottage), from about 31% to 39% across, its top about 45% down; a door (A Cottage), from about 60% to 68% across, its top about 45% down. Any other door in this panel stays small, plain and shut, so these are clearly the ones to use.

WAYS OUT. The right edge is a way out, to The Festival Square: let the scene and the floor run on off the right edge.

SCENE, left to right. The east end of Questeria's gate plaza at bright midday. Questeria's houses are cream dressed stone below and timber-framed or plastered above (cream, blush pink or pale blue), under steep roofs of scalloped tiles in azure, rose, teal or plum, with dormers, brick chimneys, gold finials, arched windows with warm glass, window boxes of pink and white flowers, ivy and climbing roses. From the left edge to about 12%: the Chandler's navy shopfront, its bay window of candles and its candle sign; its shop door is in the box. From about 25% to 45%: the rose house, rose-pink plaster with climbing roses; its house door is in the box; an iron bench at about 51% and a long planter at about 56%. From about 58% to 80%: the awning house under a plum roof with a rose-and-white striped awning; its house door is in the box; a cask at about 82%. From about 86% to the right edge: a tall open stone archway through to the festival square, sunlight and bunting beyond it, its foot running off the right edge with the floor. Beyond the roofs: the Sunspire, the royal keep's great tower on its hill above the city: a slender pale cream-and-gold stone spire with a tall azure cone tipped by a gold sun finial, smaller blue-coned turrets clustered round its foot, pink and navy pennants, a faint gold glow at its tip. Sky: bright blue with soft clouds going pink low down.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from sky to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

#### `questeria/festival-square` · The Festival Square · #6 · P0

- **Size:** 3200 × 900, 3 panels (canvas x 0–1600, 1400–3000, 1600–3200). Street. Bright midday.
- **Doors:** gate *The City Gate* at x 2300; door *A Cottage* at x 2700; door *A Townhouse* at x 2980. **Ways out:** west to The Gate Plaza; east to The Lantern Market; up at x 1000 to The Sunspire Steps.
- **Used in:** the city's hub; later the square of the Purging Dawn.
- **Basis:** `12-rooms.md` §6 (the square of the Purging Dawn, a fountain, the balcony above); `paint_rooms_questeria.py` (`festival_square`); the zone file (the City Gate, two homes, the way up to the Sunspire Steps at x 1000, the painter's palace, temple, guildhall, crystal pillars, flower arch and watch post).
- **Derived:** the colonnade's columns, the crystal pillars' look, the bunting's colours.
- **The game's own pieces:** The game stands its own fountain in the middle of the square (about x 1600), two statues, lamps, benches, planters and a crystal waystone on the paving.

##### `questeria/festival-square` · panel 1 of 3

- **Attach:** `shots/art/room-templates/questeria-festival-square-1.png`.
- **Send to Claude as:** `questeria-festival-square-1.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. Questeria: The Festival Square. This is panel 1 of 3, the left part of a room that scrolls sideways; the scene carries on past its right edge.

LAYOUT, as the attached guide shows it. The floor line, where every wall, doorstep, tree and rock meets the ground, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even pale cream paving. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person. The far horizon is a straight, level line about halfway down the picture (y 512 of 1024), at the eye height of someone standing on the floor; the distance (sea, hills, the far city) sits on it, and above the roofs and trees is sky.

DOORS. There is no door box in this panel, so nothing here should look like a door you could use, apart from the ways out named below; any building's own door stays small, plain and shut.

WAYS OUT. The left edge is a way out, to The Gate Plaza: let the scene and the floor run on off the left edge. The guide marks a way up, to The Sunspire Steps, at about 62% across.

SCENE, left to right. Questeria's festival square at bright midday, dressed for a festival, with bunting of pink, navy and gold pennants strung across high over the whole square. From the left edge to about 24%: a colonnade of cream stone columns with gold capitals carrying a balustraded walk, shaded arches behind. At about 28%: a flower arch, an arbour of pink climbing roses on a white iron frame. From about 45% to 78%: the broad stair up to the Sunspire Steps, wide pale stone steps rising away up the slope behind the square between low balustrades with urns of flowers, centred on about 62%; the guide marks the way up there. Beside its foot at about 60%: a crystal pillar, a slim cream stone column crowned with a glowing pale-blue crystal. From about 80% to the right edge: the start of the palace front, cream stone with tall arched windows and gold trim, rising to a grand balcony over the middle of the square at the very right edge, gold sun banners hanging from it. In the far distance over the roofs: a domed temple at about 44%, pale and hazy. Sky: bright blue with big soft clouds, pink near the horizon, the sun high at the upper left. Mood: grand, festive, proud.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from sky to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

##### `questeria/festival-square` · panel 2 of 3

- **Attach:** `shots/art/room-templates/questeria-festival-square-2.png`, and also attach your panel 1 picture (`questeria-festival-square-1.png`).
- **Send to Claude as:** `questeria-festival-square-2.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. Questeria: The Festival Square. This is panel 2 of 3, the middle part of a room that scrolls sideways; the scene carries on past its left and right edges.

CONTINUATION. The second attached picture is panel 1 of this room. This panel carries the scene on to the right of it: the left 12% of this picture (about 192 px) shows exactly the same stretch as the right 12% of panel 1, so redraw that stretch to match it (the same buildings, trees, sky, clouds, colours, horizon, floor line, scale and light) and carry on seamlessly from there. Only the right 1344 px are new.

LAYOUT, as the attached guide shows it. The floor line, where every wall, doorstep, tree and rock meets the ground, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even pale cream paving. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person. The far horizon is a straight, level line about halfway down the picture (y 512 of 1024), at the eye height of someone standing on the floor; the distance (sea, hills, the far city) sits on it, and above the roofs and trees is sky.

DOORS. Draw each of these exactly inside its labelled box on the guide, filling the box, its threshold on the floor line (the names are only for you: write nothing): a great door or gateway (The City Gate), from about 47% to 66% across, its top about 36% down; a door (A Cottage), from about 78% to 85% across, its top about 45% down; a door (A Townhouse), from about 95% to 100% across, its top about 45% down (its box runs off the right edge, so only its left part shows here). Any other door in this panel stays small, plain and shut, so these are clearly the ones to use.

SCENE, left to right. The middle of Questeria's festival square at bright midday, bunting of pink, navy and gold pennants strung high across it. From the left edge to about 25%: the palace front, cream stone with tall arched windows and gold trim, its grand balcony centred at about 12% with a carved stone balustrade and long gold sun banners hanging from it (the game's fountain stands in front of it); behind and above it, filling the sky: the Sunspire, the royal keep's great tower on its hill above the city: a slender pale cream-and-gold stone spire with a tall azure cone tipped by a gold sun finial, smaller blue-coned turrets clustered round its foot, pink and navy pennants, a faint gold glow at its tip. At about 37%: a small square watch post of cream stone with a navy-and-gold shield and a lantern. From about 45% to 68%: the City Gate in the city wall, a great round arch between two round towers with blue cone roofs and navy banners, its heavy timber gates folded open; through it the road runs out into sunny green country (the great door in the box is this arch). At about 68%: a crystal pillar with a glowing pale-blue crystal. In the far distance at about 69%: the guildhall's pale towers. From about 74% to 88%: the ivy house, cream stone thick with ivy under a blue roof; its house door is in the box. From about 90% to the right edge: a tall plum-roofed townhouse; its door box runs off the right edge. Sky: bright blue with soft clouds.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from sky to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

##### `questeria/festival-square` · panel 3 of 3

- **Attach:** `shots/art/room-templates/questeria-festival-square-3.png`, and also attach your panel 2 picture (`questeria-festival-square-2.png`).
- **Send to Claude as:** `questeria-festival-square-3.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. Questeria: The Festival Square. This is panel 3 of 3, the right part of a room that scrolls sideways; the scene carries on past its left edge.

CONTINUATION. The second attached picture is panel 2 of this room. This panel carries the scene on to the right of it: the left 88% of this picture (about 1344 px) shows exactly the same stretch as the right 88% of panel 2, so redraw that stretch to match it (the same buildings, trees, sky, clouds, colours, horizon, floor line, scale and light) and carry on seamlessly from there. Only the right 192 px are new.

LAYOUT, as the attached guide shows it. The floor line, where every wall, doorstep, tree and rock meets the ground, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even pale cream paving. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person. The far horizon is a straight, level line about halfway down the picture (y 512 of 1024), at the eye height of someone standing on the floor; the distance (sea, hills, the far city) sits on it, and above the roofs and trees is sky.

DOORS. Draw each of these exactly inside its labelled box on the guide, filling the box, its threshold on the floor line (the names are only for you: write nothing): a great door or gateway (The City Gate), from about 34% to 53% across, its top about 36% down; a door (A Cottage), from about 65% to 72% across, its top about 45% down; a door (A Townhouse), from about 82% to 90% across, its top about 45% down. Any other door in this panel stays small, plain and shut, so these are clearly the ones to use.

WAYS OUT. The right edge is a way out, to The Lantern Market: let the scene and the floor run on off the right edge.

SCENE, left to right. The east side of Questeria's festival square at bright midday, bunting strung high across it. From the left edge to about 12%: the east half of the palace front, cream stone, tall arched windows, gold trim, the end of its balcony with a gold sun banner; the Sunspire, the royal keep's great tower on its hill above the city: a slender pale cream-and-gold stone spire with a tall azure cone tipped by a gold sun finial, smaller blue-coned turrets clustered round its foot, pink and navy pennants, a faint gold glow at its tip rises behind it. At about 24%: a small cream watch post with a navy-and-gold shield and a lantern. From about 33% to 54%: the City Gate, a great round arch in the city wall between two round towers under blue cones with navy banners, the timber gates folded open, the road and green country beyond (the great door in the box is this arch). At about 56%: a crystal pillar crowned with a glowing pale-blue crystal. From about 62% to 76%: the ivy house under a blue roof; its house door is in the box. From about 78% to 94%: a tall, narrow townhouse washed plum and cream with a rose roof; its door is in the box. From about 94% to the right edge: the street running on to the Lantern Market. Sky: bright blue with soft clouds going pink low down.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from sky to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

#### `questeria/lantern-market` · The Lantern Market · #7 · P0

- **Size:** 3200 × 900, 3 panels (canvas x 0–1600, 1400–3000, 1600–3200). Street. Late afternoon, golden.
- **Doors:** inn *The Gilded Flagon* at x 560; door *A Cottage* at x 900; door *A Cottage* at x 1240; door *A Cottage* at x 1860; door *The Rising Loaf* at x 2160; anvil *Halden's Forge* at x 2460; door *A Cottage* at x 2720; door *A Townhouse* at x 2980. **Ways out:** west to The Festival Square; east to The Thornbridge Gate.
- **Used in:** A1-C1-02 (Fennick and the quest board); the Gilded Flagon, Halden's Forge, the Rising Loaf.
- **Basis:** `12-rooms.md` §6 (the market under strings of lanterns); `paint_rooms_questeria.py` (`lantern_market`) and its preview, the look the owner liked; the zone file (eight doors, the painter's houses, stalls and trees).
- **Derived:** the corner house, the garden wall by the quest board, the bunting pole in the square.
- **The game's own pieces:** The game stands its own quest board (about x 240), lamps, a flower cart, a market stall, casks, barrels and a chalkboard on the paving.

##### `questeria/lantern-market` · panel 1 of 3

- **Attach:** `shots/art/room-templates/questeria-lantern-market-1.png`.
- **Send to Claude as:** `questeria-lantern-market-1.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. Questeria: The Lantern Market. This is panel 1 of 3, the left part of a room that scrolls sideways; the scene carries on past its right edge.

LAYOUT, as the attached guide shows it. The floor line, where every wall, doorstep, tree and rock meets the ground, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even pale paving and cobbles. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person. The far horizon is a straight, level line about halfway down the picture (y 512 of 1024), at the eye height of someone standing on the floor; the distance (sea, hills, the far city) sits on it, and above the roofs and trees is sky.

DOORS. Draw each of these exactly inside its labelled box on the guide, filling the box, its threshold on the floor line (the names are only for you: write nothing): an inn door (The Gilded Flagon), from about 31% to 39% across, its top about 45% down; a door (A Cottage), from about 52% to 60% across, its top about 45% down; a door (A Cottage), from about 74% to 81% across, its top about 45% down. Any other door in this panel stays small, plain and shut, so these are clearly the ones to use.

WAYS OUT. The left edge is a way out, to The Festival Square: let the scene and the floor run on off the left edge.

SCENE, left to right. Questeria's Lantern Market in the late afternoon, warm and golden, the sun low at the upper left and the first windows starting to glow; strings of round paper lanterns in cream, pink and pale blue looped between the eaves all along the street. Questeria's houses are cream dressed stone below and timber-framed or plastered above (cream, blush pink or pale blue), under steep roofs of scalloped tiles in azure, rose, teal or plum, with dormers, brick chimneys, gold finials, arched windows with warm glass, window boxes of pink and white flowers, ivy and climbing roses. From the left edge to about 12%: a corner house with a teal roof and lit arched windows. From about 12% to 24%: a low cream garden wall with round green bushes, and a big pink blossom tree behind it (the game puts the quest board in front). From about 24% to 46%: the Gilded Flagon, a big, handsome inn, cream stone below and white timber-framing above under a steep azure scalloped roof with two dormers and tall chimneys, a pink banner and a navy banner with gold stars hanging from the upper floor, lit windows with flower boxes, lanterns on brackets either side of its broad arched oak door, and a hanging sign with a gold tankard; its inn door is in the box. From about 49% to 63%: the rose house, blush pink with a blue arched door; its house door is in the box. From about 63% to 68%: a tall clipped cypress. From about 70% to 86%: a cream house with a blue roof; its house door is in the box. From about 88% to the right edge: the market square opens, a market stall with a blue-and-white striped awning and baskets of fruit, and beyond the square, over the far roofs, the Sunspire, the royal keep's great tower on its hill above the city: a slender pale cream-and-gold stone spire with a tall azure cone tipped by a gold sun finial, smaller blue-coned turrets clustered round its foot, pink and navy pennants, a faint gold glow at its tip. Sky: soft blue going warm pink and gold toward the horizon, big soft clouds. Mood: lively, warm, inviting.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from sky to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

##### `questeria/lantern-market` · panel 2 of 3

- **Attach:** `shots/art/room-templates/questeria-lantern-market-2.png`, and also attach your panel 1 picture (`questeria-lantern-market-1.png`).
- **Send to Claude as:** `questeria-lantern-market-2.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. Questeria: The Lantern Market. This is panel 2 of 3, the middle part of a room that scrolls sideways; the scene carries on past its left and right edges.

CONTINUATION. The second attached picture is panel 1 of this room. This panel carries the scene on to the right of it: the left 12% of this picture (about 192 px) shows exactly the same stretch as the right 12% of panel 1, so redraw that stretch to match it (the same buildings, trees, sky, clouds, colours, horizon, floor line, scale and light) and carry on seamlessly from there. Only the right 1344 px are new.

LAYOUT, as the attached guide shows it. The floor line, where every wall, doorstep, tree and rock meets the ground, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even pale paving and cobbles. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person. The far horizon is a straight, level line about halfway down the picture (y 512 of 1024), at the eye height of someone standing on the floor; the distance (sea, hills, the far city) sits on it, and above the roofs and trees is sky.

DOORS. Draw each of these exactly inside its labelled box on the guide, filling the box, its threshold on the floor line (the names are only for you: write nothing): a door (A Cottage), from about 25% to 32% across, its top about 45% down; a door (The Rising Loaf), from about 44% to 51% across, its top about 45% down; a smithy door (Halden's Forge), from about 62% to 70% across, its top about 45% down; a door (A Cottage), from about 79% to 86% across, its top about 45% down; a door (A Townhouse), from about 95% to 100% across, its top about 45% down (its box runs off the right edge, so only its left part shows here). Any other door in this panel stays small, plain and shut, so these are clearly the ones to use.

SCENE, left to right. The middle of the Lantern Market in the late afternoon, warm and golden; strings of round paper lanterns in cream, pink and pale blue looped between the eaves. Questeria's houses are cream dressed stone below and timber-framed or plastered above (cream, blush pink or pale blue), under steep roofs of scalloped tiles in azure, rose, teal or plum, with dormers, brick chimneys, gold finials, arched windows with warm glass, window boxes of pink and white flowers, ivy and climbing roses. From the left edge to about 24%: the open market square, a stall with a blue-and-white striped awning at about 5% and one with a pink-and-white awning at about 18%, crates, sacks and baskets of fruit and bread under them, a tall lantern pole hung with strings of lanterns; beyond the square, framed by the rooftops, the Sunspire, the royal keep's great tower on its hill above the city: a slender pale cream-and-gold stone spire with a tall azure cone tipped by a gold sun finial, smaller blue-coned turrets clustered round its foot, pink and navy pennants, a faint gold glow at its tip rising tall and pale and catching the gold light. From about 25% to 33%: a small cottage under a plum-purple roof; its house door is in the box; a round green oak at about 35%. From about 38% to 52%: the Rising Loaf, a bakery washed pink and cream with a curved bay window of loaves and a hanging sign with a golden loaf; its door is in the box. A pink cherry tree at about 56%. From about 59% to 75%: Halden's Forge, sooty grey stone under a plum roof, a fat brick chimney trailing smoke, an orange forge glow in its window, a hanging sign with a black anvil; its smithy door is in the box. From about 77% to 92%: a cream house with a pink-and-white striped awning; its house door is in the box. From about 93%: a teal-roofed townhouse begins; its door box runs off the right edge. Sky: soft blue going warm pink and gold toward the horizon.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from sky to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

##### `questeria/lantern-market` · panel 3 of 3

- **Attach:** `shots/art/room-templates/questeria-lantern-market-3.png`, and also attach your panel 2 picture (`questeria-lantern-market-2.png`).
- **Send to Claude as:** `questeria-lantern-market-3.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. Questeria: The Lantern Market. This is panel 3 of 3, the right part of a room that scrolls sideways; the scene carries on past its left edge.

CONTINUATION. The second attached picture is panel 2 of this room. This panel carries the scene on to the right of it: the left 88% of this picture (about 1344 px) shows exactly the same stretch as the right 88% of panel 2, so redraw that stretch to match it (the same buildings, trees, sky, clouds, colours, horizon, floor line, scale and light) and carry on seamlessly from there. Only the right 192 px are new.

LAYOUT, as the attached guide shows it. The floor line, where every wall, doorstep, tree and rock meets the ground, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even pale paving and cobbles. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person. The far horizon is a straight, level line about halfway down the picture (y 512 of 1024), at the eye height of someone standing on the floor; the distance (sea, hills, the far city) sits on it, and above the roofs and trees is sky.

DOORS. Draw each of these exactly inside its labelled box on the guide, filling the box, its threshold on the floor line (the names are only for you: write nothing): a door (A Cottage), from about 12% to 20% across, its top about 45% down; a door (The Rising Loaf), from about 31% to 39% across, its top about 45% down; a smithy door (Halden's Forge), from about 50% to 58% across, its top about 45% down; a door (A Cottage), from about 66% to 74% across, its top about 45% down; a door (A Townhouse), from about 82% to 90% across, its top about 45% down. Any other door in this panel stays small, plain and shut, so these are clearly the ones to use.

WAYS OUT. The right edge is a way out, to The Thornbridge Gate: let the scene and the floor run on off the right edge.

SCENE, left to right. The east end of the Lantern Market in the late afternoon, warm and golden; strings of round paper lanterns in cream, pink and pale blue looped between the eaves. Questeria's houses are cream dressed stone below and timber-framed or plastered above (cream, blush pink or pale blue), under steep roofs of scalloped tiles in azure, rose, teal or plum, with dormers, brick chimneys, gold finials, arched windows with warm glass, window boxes of pink and white flowers, ivy and climbing roses. From the left edge to about 9%: a market stall with a pink-and-white awning, crates and baskets. From about 10% to 20%: a small cottage under a plum-purple roof; its house door is in the box; a round green oak at about 23%. From about 26% to 40%: the Rising Loaf, a pink-and-cream bakery with a bay window of loaves and a golden loaf sign; its door is in the box. A pink cherry tree at about 43%. From about 46% to 62%: Halden's Forge, sooty grey stone under a plum roof, a fat brick chimney with smoke, a forge glow in the window, an anvil sign; its smithy door is in the box. From about 64% to 76%: a cream house with a pink-and-white striped awning; its house door is in the box; a wooden bench at about 79%. From about 80% to 91%: a tall townhouse under a teal roof; its door is in the box. From about 91% to the right edge: a pink blossom tree over a little round well ringed with flowers, and the street running on east toward the Thornbridge gate. Sky: soft blue going warm pink and gold toward the horizon, the Sunspire's tip just showing over the roofs at the left.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from sky to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

#### `questeria/sunspire-steps` · The Sunspire Steps · #8 · P0

- **Size:** 1600 × 900, 1 panel (canvas x 0–1600). Street. Afternoon.
- **Doors:** crown *Sunspire Keep* at x 760; stair-down *The Old Crown Vault* at x 1160. **Ways out:** down at x 200 to The Festival Square.
- **Used in:** A1-C4-02 (down to the vault), A1-C4-03 (up to the keep).
- **Basis:** `12-rooms.md` §6 (the broad steps up to the keep, the Sunspire filling the far layer); `paint_rooms_questeria.py` (`sunspire_steps`); the zone file (the keep's great door, the stair down to the vault, the way down to the square at x 200, the painter's temple, palace and guildhall).
- **Derived:** the temple's and guildhall's facades, the iron gate's arch.
- **The game's own pieces:** The game draws the vault stair's iron gate itself (shut or open), and stands its own banners, cypresses, a topiary and lamps on the terrace.

##### `questeria/sunspire-steps` · panel 1 of 1

- **Attach:** `shots/art/room-templates/questeria-sunspire-steps-1.png`.
- **Send to Claude as:** `questeria-sunspire-steps-1.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. Questeria: The Sunspire Steps. This one picture is the whole room.

LAYOUT, as the attached guide shows it. The floor line, where every wall, doorstep, tree and rock meets the ground, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even pale terrace paving. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person. The far horizon is a straight, level line about halfway down the picture (y 512 of 1024), at the eye height of someone standing on the floor; the distance (sea, hills, the far city) sits on it, and above the roofs and trees is sky.

DOORS. Draw each of these exactly inside its labelled box on the guide, filling the box, its threshold on the floor line (the names are only for you: write nothing): a palace's great door (Sunspire Keep), from about 38% to 57% across, its top about 36% down; the opening for a stair going down (The Old Crown Vault), from about 68% to 78% across, its top about 53% down. Any other door in this panel stays small, plain and shut, so these are clearly the ones to use.

WAYS OUT. The room's left end is closed: end the scene at the left with a building, a wall, rocks or trees, as the scene says. The guide marks a way down, to The Festival Square, at about 12% across. The room's right end is closed: end the scene at the right with a building, a wall, rocks or trees, as the scene says.

SCENE, left to right. The terrace at the top of the Sunspire Steps, in front of the royal keep, on a clear afternoon. The whole sky is filled by the Sunspire, the royal keep's great tower on its hill above the city: a slender pale cream-and-gold stone spire with a tall azure cone tipped by a gold sun finial, smaller blue-coned turrets clustered round its foot, pink and navy pennants, a faint gold glow at its tip, standing huge right behind the keep, its gold tip near the top of the picture. From the left edge to about 18%: the head of the broad stair going down to the festival square, its balustrade turning down out of sight (the guide marks the way down at about 12%). From about 8% to 28%: the temple's front, white stone columns under a pediment with a gold sun, tall bronze doors shut. Along the back of the terrace runs a carved cream balustrade with flower urns. From about 30% to 66%: the front of Sunspire Keep, cream and gold stone, two round towers with tall azure cones and gold finials flanking a great arched entrance with a gold sun blazing over it; the palace's great door (tall oak double doors with gold fittings) fills its box. From about 64% to 80%: a low wall under the terrace with a plain round stone arch in it around the stair-down box (paint only the arch and dark plain stone inside it: the game draws its own iron gate and steps there). From about 76% to the right edge: the guildhall, a grand pale building with a clock-less tower, tall windows and blue banners. Light: clear afternoon from the upper left, the stone glowing warm. Mood: grand, a little forbidding, guarded.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from sky to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

#### `questeria/thornbridge-gate` · The Thornbridge Gate · #9 · P0

- **Size:** 1600 × 900, 1 panel (canvas x 0–1600). Street. Afternoon.
- **Doors:** door *A Turret House* at x 520; door *A Townhouse* at x 1100. **Ways out:** east to Thornbridge; west to The Lantern Market.
- **Used in:** A1-C3 (the way out to Thornbridge); A1-C5-01 (the caravan leaves here).
- **Basis:** `12-rooms.md` §6 (the arch down to Thornbridge); `paint_rooms_questeria.py` (`thornbridge_gate`); the zone file (two homes, the painter's turret house, townhouse, cherry tree and flowers, the exit east to Thornbridge).
- **Derived:** the east gate's towers, the view down to the river and the tents.
- **The game's own pieces:** The game stands its own lamps, a barrel of flowers and a signpost on the cobbles.

##### `questeria/thornbridge-gate` · panel 1 of 1

- **Attach:** `shots/art/room-templates/questeria-thornbridge-gate-1.png`.
- **Send to Claude as:** `questeria-thornbridge-gate-1.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. Questeria: The Thornbridge Gate. This one picture is the whole room.

LAYOUT, as the attached guide shows it. The floor line, where every wall, doorstep, tree and rock meets the ground, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even grey cobbles. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person. The far horizon is a straight, level line about halfway down the picture (y 512 of 1024), at the eye height of someone standing on the floor; the distance (sea, hills, the far city) sits on it, and above the roofs and trees is sky.

DOORS. Draw each of these exactly inside its labelled box on the guide, filling the box, its threshold on the floor line (the names are only for you: write nothing): a door (A Turret House), from about 29% to 36% across, its top about 45% down; a door (A Townhouse), from about 65% to 72% across, its top about 45% down. Any other door in this panel stays small, plain and shut, so these are clearly the ones to use.

WAYS OUT. The left edge is a way out, to The Lantern Market: let the scene and the floor run on off the left edge. The right edge is a way out, to Thornbridge: let the scene and the floor run on off the right edge.

SCENE, left to right. The east end of Questeria at the city's east gate, on a warm afternoon. Questeria's houses are cream dressed stone below and timber-framed or plastered above (cream, blush pink or pale blue), under steep roofs of scalloped tiles in azure, rose, teal or plum, with dormers, brick chimneys, gold finials, arched windows with warm glass, window boxes of pink and white flowers, ivy and climbing roses. From the left edge to about 20%: a garden of white flowers behind a low cream wall, the last of the Lantern Market's houses at the very left. From about 20% to 40%: a turret house with a round corner turret under a teal cone and a blue roof; its house door is in the box. From about 42% to 56%: a little garden with pink flowers and a low wall. From about 56% to 78%: a tall cream townhouse with a rose roof, window boxes and ivy; its house door is in the box; a small pink cherry tree at about 81%. From about 84% to the right edge: the city's east gate, a round arch in the thick cream city wall with a blue-coned tower at its side, the gate open; through it and beyond, down the slope in the distance, the road winding down to a river in a green valley, a patched old stone bridge, and many small, colourful tents of a refugee camp by the water, thin smoke rising from cooking fires. The floor runs on through the right edge and out of the gate. Sky: warm afternoon blue with soft clouds going pink low down, the sun at the upper left. Mood: the city's comfortable edge, with trouble in sight beyond it.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from sky to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```


### 3.4 `gilded-flagon` · The Gilded Flagon

The big, warm inn on the Lantern Market. Tiles: `interior`.

#### `gilded-flagon/taproom` · The Taproom · #10 · P0

- **Size:** 2432 × 900, 2 panels (canvas x 0–1600, 832–2432). Interior. Evening, warm lamplight; dark (ambient 0.55).
- **Doors:** none. **Ways out:** west back out.
- **Used in:** A1-C4-01 (Elara, hooded, in the booth); resting and rumours.
- **Basis:** `12-rooms.md` §6 (a long bar, a fire, a balcony); `03b-world-art.md` §1 (tavern hall, back booth, hearth); `paint_rooms_interiors.py` (`taproom`); the zone file (the hearth, the bar and the tables as the game's props).
- **Derived:** the balcony's room doors and windows, the second chandelier.
- **The game's own pieces:** The game stands its own bar counter, kegs, bookshelf, hearth, booth, tables, chairs and candle stands on the floor, and hangs two tapestries on the lower wall.

##### `gilded-flagon/taproom` · panel 1 of 2

- **Attach:** `shots/art/room-templates/gilded-flagon-taproom-1.png`.
- **Send to Claude as:** `gilded-flagon-taproom-1.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Gilded Flagon: The Taproom. This is panel 1 of 2, the left part of a room that scrolls sideways; the scene carries on past its right edge.

LAYOUT, as the attached guide shows it. This is an interior, with no sky: the back wall, with the ceiling's beams at the top, fills the picture down to the floor line. The floor line, where the back wall and everything against it meets the floor, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even wide honey-brown floorboards. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person.

DOORS. There is no door box in this panel, so nothing here should look like a door you could use, apart from the ways out named below.

WAYS OUT. The left edge is the way back out: let the scene and the floor run on off the left edge.

SCENE, left to right. Inside the Gilded Flagon's taproom, a big, warm, well-loved inn, in the evening. Warm yellow plaster over a dark wood panelled wainscot, dark ceiling beams across the top. From the left edge to about 8%: a curtained window on a dusky blue evening. From about 10% to 45%: the bar's back wall: two long shelves of bottles in every colour, a garland of hops and green leaves looped along the wall above them, and high up at about 19% the inn's gilded board, dark wood with a gold rim and a painted gold tankard (no letters); the game stands its bar, kegs and stools in front. At about 55%: a wheel chandelier of candles hanging from a beam on a chain, lit. From about 48% to 70%: a wooden staircase with a turned banister climbing from the floor line up to the right to a balcony. From about 70% to the right edge: the balcony running along the upper wall behind a turned wooden balustrade, a guest-room door at about 78% and a small window at about 90% behind it, the lower wall under it plain warm panelling. The left edge is the way out to the street. Mood: cosy, golden, bustling.

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from ceiling to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

##### `gilded-flagon/taproom` · panel 2 of 2

- **Attach:** `shots/art/room-templates/gilded-flagon-taproom-2.png`, and also attach your panel 1 picture (`gilded-flagon-taproom-1.png`).
- **Send to Claude as:** `gilded-flagon-taproom-2.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Gilded Flagon: The Taproom. This is panel 2 of 2, the right part of a room that scrolls sideways; the scene carries on past its left edge.

CONTINUATION. The second attached picture is panel 1 of this room. This panel carries the scene on to the right of it: the left 48% of this picture (about 737 px) shows exactly the same stretch as the right 48% of panel 1, so redraw that stretch to match it (the same buildings, trees, sky, clouds, colours, horizon, floor line, scale and light) and carry on seamlessly from there. Only the right 799 px are new.

LAYOUT, as the attached guide shows it. This is an interior, with no sky: the back wall, with the ceiling's beams at the top, fills the picture down to the floor line. The floor line, where the back wall and everything against it meets the floor, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even wide honey-brown floorboards. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person.

DOORS. There is no door box in this panel, so nothing here should look like a door you could use, apart from the ways out named below.

WAYS OUT. The room's right end is closed: end it at the right with its end wall.

SCENE, left to right. The east half of the Gilded Flagon's taproom in the evening. Warm yellow plaster over a dark wood panelled wainscot, dark beams across the ceiling. From the left edge to about 18%: the top of the wooden staircase reaching the balcony, with a wheel chandelier of candles hanging at about 3%. From about 18% to the right edge: the balcony along the upper wall behind a turned wooden balustrade, guest-room doors at about 26%, 76% and 92%, small windows at about 39% and 85%. From about 54% to 66%: a great chimney breast of warm cream stone rising from the floor line to the ceiling (the game stands its own hearth in front of its foot, so keep its lowest part plain stone). At about 80%: a second wheel chandelier, lit. The lower wall under the balcony is plain warm panelling with a few candle sconces (the game hangs its own tapestries there). The right end of the room is closed: a panelled end wall. Mood: cosy, golden, bustling.

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from ceiling to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```


### 3.5 `shop` · The Chandler's

The Chandler's, the ship's chandlery the harbour and the gate plaza both open on. Tiles: `interior`.

#### `shop/inside` · The Chandler's · #11 · P0

- **Size:** 1600 × 900, 1 panel (canvas x 0–1600). Interior. Day; dark (ambient 0.7).
- **Doors:** none. **Ways out:** west back out.
- **Used in:** the Chandler's: buying and selling.
- **Basis:** `12-rooms.md` §2.4 (a shop is a counter, shelves of what it sells, the keeper); `paint_rooms_interiors.py` (`shop`); the zone file (the counter, bookshelves and tool rack as the game's props).
- **Derived:** the window's view of the harbour, the beams' lanterns.
- **The game's own pieces:** The game stands its own counter, bookshelves, tool rack, crates, barrels and candle stands on the floor.

##### `shop/inside` · panel 1 of 1

- **Attach:** `shots/art/room-templates/shop-inside-1.png`.
- **Send to Claude as:** `shop-inside-1.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Chandler's. This one picture is the whole room.

LAYOUT, as the attached guide shows it. This is an interior, with no sky: the back wall, with the ceiling's beams at the top, fills the picture down to the floor line. The floor line, where the back wall and everything against it meets the floor, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even pale grey flagstones. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person.

DOORS. There is no door box in this panel, so nothing here should look like a door you could use, apart from the ways out named below.

WAYS OUT. The left edge is the way back out: let the scene and the floor run on off the left edge. The room's right end is closed: end it at the right with its end wall.

SCENE, left to right. Inside the Chandler's, a ship's chandlery, by day. Soft blue plaster above dark wooden boards, heavy dark beams across the ceiling with two brass ship's lanterns hanging from them on chains at about 30% and 70%. From about 6% to 18%: a window with small panes looking out on the sunny harbour, blue water and masts. From about 20% to 36%: hanks of rope hung on pegs and an iron anchor on the wall. From about 40% to 66%: tall shelves behind where the counter stands, full of jars, brass lanterns, tins, candles, bottles and folded sailcloth; above them at about 53% the shop's board, dark wood with a gold rim and a painted gold anchor (no letters). From about 70% to 80%: a model sailing ship on a bracket high on the wall. From about 82% to 95%: a ship's wheel mounted on the wall, coils of rope below it. The left edge is the way back out to the street. Mood: neat, salty, crammed with useful things.

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from ceiling to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```


### 3.6 `forge` · The Smithy

Halden's Forge on the Lantern Market. Tiles: `interior`.

#### `forge/inside` · The Smithy · #12 · P0

- **Size:** 1600 × 900, 1 panel (canvas x 0–1600). Interior. Day; dark (ambient 0.55).
- **Doors:** none. **Ways out:** west back out.
- **Used in:** Halden's Forge: the smith's shop.
- **Basis:** `paint_rooms_interiors.py` (`forge`); the zone file (the hearth and anvil, tool racks and barrels as the game's props).
- **Derived:** the shield's colours.
- **The game's own pieces:** The game stands its own hearth and anvil in the middle (about x 800), tool racks, a water barrel, a table, crates and a chalkboard on the floor.

##### `forge/inside` · panel 1 of 1

- **Attach:** `shots/art/room-templates/forge-inside-1.png`.
- **Send to Claude as:** `forge-inside-1.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Smithy. This one picture is the whole room.

LAYOUT, as the attached guide shows it. This is an interior, with no sky: the back wall, with the ceiling's beams at the top, fills the picture down to the floor line. The floor line, where the back wall and everything against it meets the floor, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even grey flagstones. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person.

DOORS. There is no door box in this panel, so nothing here should look like a door you could use, apart from the ways out named below.

WAYS OUT. The left edge is the way back out: let the scene and the floor run on off the left edge. The room's right end is closed: end it at the right with its end wall.

SCENE, left to right. Inside Halden's Forge by day: a working smithy of grey stone gone sooty, heavy beams across the ceiling. From about 5% to 16%: a window on the bright day. From about 18% to 34%: a wooden board on the wall hung with hammers, tongs and files. From about 36% to 64%: a great brick hood, wide at the bottom and narrowing up to a flue that goes up through the roof, centred on about 50%, the bricks under it lit orange by the fire's glow (the game stands its own hearth and anvil in front, so keep the bottom of the hood's opening plain dark brick); a big leather bellows hangs on the wall at about 39%. From about 66% to 80%: horseshoes nailed up in an arch, and a round shield on two crossed swords, the smith's trade sign. From about 82% to 96%: another board of tools, chains and a coil of iron bar. The left edge is the way back out to the street. Mood: hot, honest, hard-working.

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from ceiling to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```


### 3.7 `bakery` · The Bakery

The Rising Loaf on the Lantern Market. Tiles: `interior`.

#### `bakery/inside` · The Bakery · #13 · P0

- **Size:** 1600 × 900, 1 panel (canvas x 0–1600). Interior. Sunny morning; dark (ambient 0.65).
- **Doors:** none. **Ways out:** west back out.
- **Used in:** the Rising Loaf (the baker and a customer).
- **Basis:** `paint_rooms_interiors.py` (`bakery`); the zone file (two ovens as hearth props, the counter, shelves).
- **Derived:** the window's flower box.
- **The game's own pieces:** The game stands its own two ovens (about x 285 and 600), a table, counter, bookshelf, sacks and baskets on the floor.

##### `bakery/inside` · panel 1 of 1

- **Attach:** `shots/art/room-templates/bakery-inside-1.png`.
- **Send to Claude as:** `bakery-inside-1.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Bakery. This one picture is the whole room.

LAYOUT, as the attached guide shows it. This is an interior, with no sky: the back wall, with the ceiling's beams at the top, fills the picture down to the floor line. The floor line, where the back wall and everything against it meets the floor, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even pale flagstones and warm floorboards. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person.

DOORS. There is no door box in this panel, so nothing here should look like a door you could use, apart from the ways out named below.

WAYS OUT. The left edge is the way back out: let the scene and the floor run on off the left edge. The room's right end is closed: end it at the right with its end wall.

SCENE, left to right. Inside the Rising Loaf, a little bakery, on a sunny morning. Warm butter-yellow plaster, dark beams across the ceiling. From about 5% to 46%: the ovens' wall, warm red brick, with one long copper-and-brick hood running above both ovens and up to a chimney (the game stands its own two round brick ovens at about 18% and 37%, so keep the wall low down plain brick). From about 50% to 60%: a sunny window with small panes and a box of yellow flowers on its sill, light pouring in. From about 62% to 80%: wooden shelves stacked with round loaves, plaited loaves and buns behind where the counter stands, and above them at about 70% the bakery's board with a painted golden loaf (no letters). From about 82% to 96%: sheaves of wheat tied with ribbon on the wall and strings of onions hanging. The left edge is the way back out to the street. Mood: warm, floury, smelling of bread.

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from ceiling to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```


### 3.8 `chapel` · The Chapel

The Chapel of the Dawn on the gate plaza. Tiles: `interior`.

#### `chapel/inside` · The Chapel · #14 · P0

- **Size:** 1600 × 900, 1 panel (canvas x 0–1600). Interior. Day, coloured window light; dark (ambient 0.6).
- **Doors:** none. **Ways out:** west back out.
- **Used in:** the Chapel of the Dawn (the keeper, worshippers).
- **Basis:** `paint_rooms_interiors.py` (`chapel`); the zone file (the altar table, tapestries and pews as the game's props).
- **Derived:** the lancets' colours.
- **The game's own pieces:** The game stands its own altar table, candle stands, pews, bookshelf and plants, and hangs three tapestries low on the wall.

##### `chapel/inside` · panel 1 of 1

- **Attach:** `shots/art/room-templates/chapel-inside-1.png`.
- **Send to Claude as:** `chapel-inside-1.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Chapel. This one picture is the whole room.

LAYOUT, as the attached guide shows it. This is an interior, with no sky: the back wall, with the ceiling's beams at the top, fills the picture down to the floor line. The floor line, where the back wall and everything against it meets the floor, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even pale warm flagstones. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person.

DOORS. There is no door box in this panel, so nothing here should look like a door you could use, apart from the ways out named below.

WAYS OUT. The left edge is the way back out: let the scene and the floor run on off the left edge. The room's right end is closed: end it at the right with its end wall.

SCENE, left to right. Inside the Chapel of the Dawn by day: a small, beautiful chapel of warm cream stone. Slim wall shafts at about 10%, 35%, 65% and 90% rise to carry pointed arches under a timber roof. High in the middle, centred at about 50%: a great round rose window of stained glass in the colours of dawn (rose, gold, soft blue and lilac) round a gold sun at its centre, glowing, and below it a shallow arched niche in the wall where the altar stands (the game stands its own altar and candles in front of it). Tall pointed lancet windows of stained glass at about 20% and 80%, high up (the game hangs cloths on the wall below them). Candle sconces on the wall shafts, lit. Coloured light falls softly from the windows. The left edge is the way back out to the plaza. Mood: hushed, warm, hopeful.

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from ceiling to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```


### 3.9 `cottage-a` · A Cottage

The homes behind the city's plain doors: the one with the bed by the door. Tiles: `interior`.

#### `cottage-a/inside` · A Cottage · #15 · P0

- **Size:** 1600 × 900, 1 panel (canvas x 0–1600). Interior. Day; dark (ambient 0.65).
- **Doors:** none. **Ways out:** west back out.
- **Used in:** homes (the one with the bed by the door).
- **Basis:** `paint_rooms_interiors.py` (`cottage_a`); the zone file (bed, wardrobe, bookshelf, table as the game's props).
- **Derived:** the windows' places high on the wall.
- **The game's own pieces:** The game stands its own bed, side table, wardrobe, bookshelf, table, chairs and plants on the floor, and hangs a tapestry.

##### `cottage-a/inside` · panel 1 of 1

- **Attach:** `shots/art/room-templates/cottage-a-inside-1.png`.
- **Send to Claude as:** `cottage-a-inside-1.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. A Cottage. This one picture is the whole room.

LAYOUT, as the attached guide shows it. This is an interior, with no sky: the back wall, with the ceiling's beams at the top, fills the picture down to the floor line. The floor line, where the back wall and everything against it meets the floor, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even warm floorboards. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person.

DOORS. There is no door box in this panel, so nothing here should look like a door you could use, apart from the ways out named below.

WAYS OUT. The left edge is the way back out: let the scene and the floor run on off the left edge. The room's right end is closed: end it at the right with its end wall.

SCENE, left to right. Inside a small, snug cottage by day. Cream plaster over a dark wooden wainscot, dark beams across a low ceiling. Two curtained windows high in the wall at about 25% and 60%, small panes, the bright day and green leaves outside, boxes of pink flowers on their sills. A wooden shelf of blue-and-white crockery at about 42%, high up. Bunches of herbs drying from a beam at about 72%. A small framed painting of the sea at about 86%. A candle sconce at about 35%. The lower wall is plain wainscot, because the game stands its own bed, wardrobe and bookshelf against it. The left edge is the way back out. Mood: cosy, tidy, lived-in.

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from ceiling to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```


### 3.10 `cottage-b` · A Cottage

The homes behind the city's plain doors: the one with the hearth. Tiles: `interior`.

#### `cottage-b/inside` · A Cottage · #16 · P0

- **Size:** 1600 × 900, 1 panel (canvas x 0–1600). Interior. Day; dark (ambient 0.65).
- **Doors:** none. **Ways out:** west back out.
- **Used in:** homes (the one with the hearth).
- **Basis:** `paint_rooms_interiors.py` (`cottage_b`); the zone file (the hearth at x 493, the bed at x 1401, the wardrobe).
- **Derived:** the shelf's place.
- **The game's own pieces:** The game stands its own hearth (about x 493), armchair, wardrobe, bed, table and chairs on the floor, and hangs a tapestry.

##### `cottage-b/inside` · panel 1 of 1

- **Attach:** `shots/art/room-templates/cottage-b-inside-1.png`.
- **Send to Claude as:** `cottage-b-inside-1.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. A Cottage. This one picture is the whole room.

LAYOUT, as the attached guide shows it. This is an interior, with no sky: the back wall, with the ceiling's beams at the top, fills the picture down to the floor line. The floor line, where the back wall and everything against it meets the floor, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even warm floorboards and a few flagstones. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person.

DOORS. There is no door box in this panel, so nothing here should look like a door you could use, apart from the ways out named below.

WAYS OUT. The left edge is the way back out: let the scene and the floor run on off the left edge. The room's right end is closed: end it at the right with its end wall.

SCENE, left to right. Inside a small cottage by day, the one with the hearth. Blue-grey plaster over a dark wainscot, dark beams across a low ceiling. A small framed painting of green hills at about 12%. From about 24% to 38%: a brick chimney breast standing out from the wall from the floor line to the ceiling (the game stands its own hearth at its foot, so keep its lowest part plain brick). A curtained window with small panes at about 52%, high up, the day outside. Strings of onions and bunches of herbs hanging from the beams at about 45% and 75%. A wooden shelf with jars and a jug at about 70%, high up. A small square window high at about 90%, over where the bed stands. The left edge is the way back out. Mood: homely, a little worn, warm.

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from ceiling to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```


### 3.11 `warehouse` · A Warehouse

The warehouses on the docks. Tiles: `interior`.

#### `warehouse/inside` · A Warehouse · #17 · P0

- **Size:** 1600 × 900, 1 panel (canvas x 0–1600). Interior. Day, light from high windows; dark (ambient 0.6).
- **Doors:** none. **Ways out:** west back out.
- **Used in:** the docks' warehouses (the tally clerk).
- **Basis:** `paint_rooms_interiors.py` (`warehouse`); the zone file (crates, barrels, sacks and the clerk's desk as the game's props).
- **Derived:** the ladder to the loft.
- **The game's own pieces:** The game stands its own crates, barrels, sacks, the clerk's desk, a candle stand and a chalkboard on the floor.

##### `warehouse/inside` · panel 1 of 1

- **Attach:** `shots/art/room-templates/warehouse-inside-1.png`.
- **Send to Claude as:** `warehouse-inside-1.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. A Warehouse. This one picture is the whole room.

LAYOUT, as the attached guide shows it. This is an interior, with no sky: the back wall, with the ceiling's beams at the top, fills the picture down to the floor line. The floor line, where the back wall and everything against it meets the floor, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even worn grey flagstones. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person.

DOORS. There is no door box in this panel, so nothing here should look like a door you could use, apart from the ways out named below.

WAYS OUT. The left edge is the way back out: let the scene and the floor run on off the left edge. The room's right end is closed: end it at the right with its end wall.

SCENE, left to right. Inside a dockside warehouse by day: tall walls of weathered boards between heavy upright posts at about 0%, 25%, 50%, 75% and 100%, the roof's big beams and rafters across the top. From the left edge to about 45%: a loft running along the upper wall on beams, sacks and crates stored on it, a wooden ladder leaning up to it at about 40%. At about 55%: a hoist's iron block and chain hanging from a roof beam. High windows at about 65% and 85% letting in long shafts of daylight with dust in them. At about 70%: a lantern hanging from a peg over where the clerk's desk stands. The lower walls are plain boards, because the game stacks its own crates and barrels against them. The left edge is the way back out to the quay. Mood: dusty, busy, echoing.

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from ceiling to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```


### 3.12 `road` · The Old Road

The Old Road east from Questeria: farmland, then the fork by the barrow, then the woods. Bright and lush. Tiles: `meadow`.

#### `road/farmland` · The Farmland · #18 · P0

- **Size:** 3200 × 900, 3 panels (canvas x 0–1600, 1400–3000, 1600–3200). Street. Bright noon.
- **Doors:** door *The Farmhouse* at x 817. **Ways out:** west to Questeria; east to The Fork.
- **Used in:** Chapter 1 onward: the road out of Questeria toward the Mirewood.
- **Basis:** `03b-world-art.md` §2 (the meadows); `paint_rooms_wild.py` (`farmland`); the zone file (the farmhouse door, the painter's wheat, haystack, windmill, washing line and bridge).
- **Derived:** the city seen far off at the west end.
- **The game's own pieces:** The game stands its own hay bales, a handcart, rail fences and wildflowers on the grass.

##### `road/farmland` · panel 1 of 3

- **Attach:** `shots/art/room-templates/road-farmland-1.png`.
- **Send to Claude as:** `road-farmland-1.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Old Road: The Farmland. This is panel 1 of 3, the left part of a room that scrolls sideways; the scene carries on past its right edge.

LAYOUT, as the attached guide shows it. The floor line, where every wall, doorstep, tree and rock meets the ground, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even meadow grass with a dirt road. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person. The far horizon is a straight, level line about halfway down the picture (y 512 of 1024), at the eye height of someone standing on the floor; the distance (sea, hills, the far city) sits on it, and above the roofs and trees is sky.

DOORS. Draw each of these exactly inside its labelled box on the guide, filling the box, its threshold on the floor line (the names are only for you: write nothing): a door (The Farmhouse), from about 47% to 55% across, its top about 45% down. Any other door in this panel stays small, plain and shut, so these are clearly the ones to use.

WAYS OUT. The left edge is a way out, to Questeria: let the scene and the floor run on off the left edge.

SCENE, left to right. The Old Road through the farmland outside Questeria, at bright noon. From the left edge to about 10%: a hedgerow, and far off on its hill at the very left the city's cream walls and the Sunspire, the royal keep's great tower on its hill above the city: a slender pale cream-and-gold stone spire with a tall azure cone tipped by a gold sun finial, smaller blue-coned turrets clustered round its foot, pink and navy pennants, a faint gold glow at its tip, small and hazy. From about 10% to 25%: a pink-blossomed cherry tree at about 16% and a washing line between two posts at about 22% with white and blue linen drying. At about 32%: a cottage garden with a flower box. From about 40% to 64%: the farmhouse, whitewashed walls with dark timbers under a warm rose-tiled roof, a stone chimney trailing smoke, flower boxes, a vegetable patch; its house door is in the box. Behind and either side, in the middle distance behind a rail fence: fields of golden wheat from about 34% to the right edge, a barrel at about 80%, a round haystack at about 91%. Far off: rolling green hills, little copses, a soft blue distance. Sky: bright summer blue with big puffy white clouds, the sun high at the upper left. Mood: open, golden, peaceful.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from sky to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

##### `road/farmland` · panel 2 of 3

- **Attach:** `shots/art/room-templates/road-farmland-2.png`, and also attach your panel 1 picture (`road-farmland-1.png`).
- **Send to Claude as:** `road-farmland-2.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Old Road: The Farmland. This is panel 2 of 3, the middle part of a room that scrolls sideways; the scene carries on past its left and right edges.

CONTINUATION. The second attached picture is panel 1 of this room. This panel carries the scene on to the right of it: the left 12% of this picture (about 192 px) shows exactly the same stretch as the right 12% of panel 1, so redraw that stretch to match it (the same buildings, trees, sky, clouds, colours, horizon, floor line, scale and light) and carry on seamlessly from there. Only the right 1344 px are new.

LAYOUT, as the attached guide shows it. The floor line, where every wall, doorstep, tree and rock meets the ground, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even meadow grass with a dirt road. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person. The far horizon is a straight, level line about halfway down the picture (y 512 of 1024), at the eye height of someone standing on the floor; the distance (sea, hills, the far city) sits on it, and above the roofs and trees is sky.

DOORS. There is no door box in this panel, so nothing here should look like a door you could use, apart from the ways out named below; any building's own door stays small, plain and shut.

SCENE, left to right. The Old Road through the farmland at bright noon. From the left edge to about 20%: golden wheat behind a rail fence, a round haystack at about 3%. From about 18% to 40%: a windmill on a little green rise at about 28%, a round whitewashed tower under a conical rose roof, four big lattice sails. From about 45% to 75%: a hedgerow along the road, blue wildflowers at about 56%, a wooden crate by the fence at about 72%. From about 85% to the right edge: the road bending over a little humpbacked bridge of grey stone at about 97%, a brook running under it. Far off: rolling green hills and woods. Sky: bright summer blue with big puffy clouds.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from sky to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

##### `road/farmland` · panel 3 of 3

- **Attach:** `shots/art/room-templates/road-farmland-3.png`, and also attach your panel 2 picture (`road-farmland-2.png`).
- **Send to Claude as:** `road-farmland-3.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Old Road: The Farmland. This is panel 3 of 3, the right part of a room that scrolls sideways; the scene carries on past its left edge.

CONTINUATION. The second attached picture is panel 2 of this room. This panel carries the scene on to the right of it: the left 88% of this picture (about 1344 px) shows exactly the same stretch as the right 88% of panel 2, so redraw that stretch to match it (the same buildings, trees, sky, clouds, colours, horizon, floor line, scale and light) and carry on seamlessly from there. Only the right 192 px are new.

LAYOUT, as the attached guide shows it. The floor line, where every wall, doorstep, tree and rock meets the ground, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even meadow grass with a dirt road. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person. The far horizon is a straight, level line about halfway down the picture (y 512 of 1024), at the eye height of someone standing on the floor; the distance (sea, hills, the far city) sits on it, and above the roofs and trees is sky.

DOORS. There is no door box in this panel, so nothing here should look like a door you could use, apart from the ways out named below; any building's own door stays small, plain and shut.

WAYS OUT. The right edge is a way out, to The Fork: let the scene and the floor run on off the right edge.

SCENE, left to right. The east end of the farmland at bright noon. From the left edge to about 25%: golden wheat behind a rail fence, and the windmill on its rise at about 15%, whitewashed under a rose cone with four lattice sails. From about 30% to 65%: a hedgerow, blue wildflowers at about 43%, a crate by the fence at about 60%. From about 74% to 94%: a little humpbacked bridge of grey stone at about 84%, a sparkling brook under it with reeds. From about 90% to the right edge: the road running on east toward the fork, the trees getting thicker in the distance. Far off: green hills, darker woods to the east. Sky: bright summer blue with big puffy clouds.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from sky to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

#### `road/fork` · The Fork · #19 · P0

- **Size:** 3200 × 900, 3 panels (canvas x 0–1600, 1400–3000, 1600–3200). Street. Bright afternoon, darkening toward the east.
- **Doors:** gate *The Barrow* at x 1320. **Ways out:** east to The Mirewood; west to The Farmland.
- **Used in:** Chapter 1 onward: the fork, the barrow, the way into the Mirewood.
- **Basis:** `paint_rooms_wild.py` (`fork`); the zone file (the barrow's door, the painter's blossom trees, great oak, firs, oaks and rocks; the way east to the Mirewood).
- **Derived:** the stream's course at the back.
- **The game's own pieces:** The game stands its own trees, bushes, a campfire, a fallen tree across the east way, a signpost, a waystone and a chest on the grass.

##### `road/fork` · panel 1 of 3

- **Attach:** `shots/art/room-templates/road-fork-1.png`.
- **Send to Claude as:** `road-fork-1.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Old Road: The Fork. This is panel 1 of 3, the left part of a room that scrolls sideways; the scene carries on past its right edge.

LAYOUT, as the attached guide shows it. The floor line, where every wall, doorstep, tree and rock meets the ground, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even meadow grass. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person. The far horizon is a straight, level line about halfway down the picture (y 512 of 1024), at the eye height of someone standing on the floor; the distance (sea, hills, the far city) sits on it, and above the roofs and trees is sky.

DOORS. Draw each of these exactly inside its labelled box on the guide, filling the box, its threshold on the floor line (the names are only for you: write nothing): a great door or gateway (The Barrow), from about 73% to 92% across, its top about 36% down. Any other door in this panel stays small, plain and shut, so these are clearly the ones to use.

WAYS OUT. The left edge is a way out, to The Farmland: let the scene and the floor run on off the left edge.

SCENE, left to right. The fork in the Old Road on a bright afternoon. From the left edge to about 30%: a little stream winding across the back of the meadow, pink blossom trees leaning over it at about 22%, yellow and blue wildflowers (about 31% and 48%). From about 55% to 70%: a pink cherry tree at about 62%, a dark fir at about 68%, mossy grey rocks at about 69%. From about 70% to the right edge: the barrow, a long, low, grassy burial mound rising behind the meadow, wildflowers on its back, and at its foot its portal: two great grey standing stones and a lintel stone across them, framing a dark stone doorway into the mound, which fills its box (the great door in the box is this portal). Rocks and a small bush at about 96%. Far off: soft green hills under a blue sky. Sky: bright afternoon blue with soft clouds, the sun at the upper left. Mood: open and sunny, with an old mystery in it.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from sky to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

##### `road/fork` · panel 2 of 3

- **Attach:** `shots/art/room-templates/road-fork-2.png`, and also attach your panel 1 picture (`road-fork-1.png`).
- **Send to Claude as:** `road-fork-2.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Old Road: The Fork. This is panel 2 of 3, the middle part of a room that scrolls sideways; the scene carries on past its left and right edges.

CONTINUATION. The second attached picture is panel 1 of this room. This panel carries the scene on to the right of it: the left 12% of this picture (about 192 px) shows exactly the same stretch as the right 12% of panel 1, so redraw that stretch to match it (the same buildings, trees, sky, clouds, colours, horizon, floor line, scale and light) and carry on seamlessly from there. Only the right 1344 px are new.

LAYOUT, as the attached guide shows it. The floor line, where every wall, doorstep, tree and rock meets the ground, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even meadow grass. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person. The far horizon is a straight, level line about halfway down the picture (y 512 of 1024), at the eye height of someone standing on the floor; the distance (sea, hills, the far city) sits on it, and above the roofs and trees is sky.

DOORS. Draw each of these exactly inside its labelled box on the guide, filling the box, its threshold on the floor line (the names are only for you: write nothing): a great door or gateway (The Barrow), from about 0% to 4% across, its top about 36% down (its box runs off the left edge, so only its right part shows here). Any other door in this panel stays small, plain and shut, so these are clearly the ones to use.

SCENE, left to right. The fork in the Old Road, where the meadow starts to turn to woodland. At the very left edge: the end of the barrow's portal (its box runs off the left edge) and the grassy mound. Rocks and a small bush at about 8%. From about 22% to 48%: the great oak, alone on a gentle rise at about 35%, a huge, ancient, spreading oak with a vast round crown, a landmark. From about 55% to the right edge: the wood thickening, oaks at about 70%, 74% and 81%, a dark fir at about 99%, mushrooms among the roots at about 84%, the trees getting taller, darker, older and mossier toward the right. Far off: a dark wooded ridge, the Mirewood. Sky: afternoon blue, a little greyer and cooler toward the right.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from sky to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

##### `road/fork` · panel 3 of 3

- **Attach:** `shots/art/room-templates/road-fork-3.png`, and also attach your panel 2 picture (`road-fork-2.png`).
- **Send to Claude as:** `road-fork-3.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Old Road: The Fork. This is panel 3 of 3, the right part of a room that scrolls sideways; the scene carries on past its left edge.

CONTINUATION. The second attached picture is panel 2 of this room. This panel carries the scene on to the right of it: the left 88% of this picture (about 1344 px) shows exactly the same stretch as the right 88% of panel 2, so redraw that stretch to match it (the same buildings, trees, sky, clouds, colours, horizon, floor line, scale and light) and carry on seamlessly from there. Only the right 192 px are new.

LAYOUT, as the attached guide shows it. The floor line, where every wall, doorstep, tree and rock meets the ground, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even meadow grass. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person. The far horizon is a straight, level line about halfway down the picture (y 512 of 1024), at the eye height of someone standing on the floor; the distance (sea, hills, the far city) sits on it, and above the roofs and trees is sky.

DOORS. There is no door box in this panel, so nothing here should look like a door you could use, apart from the ways out named below; any building's own door stays small, plain and shut.

WAYS OUT. The right edge is a way out, to The Mirewood: let the scene and the floor run on off the right edge.

SCENE, left to right. The east end of the fork, where the road goes into the trees. From the left edge to about 35%: the great oak on its gentle rise at about 22%, huge and spreading. From about 45% to the right edge: the wood closing in, oaks at about 57%, 61% and 69%, a dark fir at about 86%, mushrooms at about 72%, the trunks crowding together, darker, mossier and older, ferns at their feet, until at the right edge the road disappears into the shade of the Mirewood. Far off: a dark wooded ridge. Sky: afternoon blue going greyer and cooler toward the right, the light falling off under the trees.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from sky to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```


### 3.13 `barrow` · The Barrow

An ancient burial mound off the Old Road: grey-violet stone, roots, niches. Underground. Tiles: `crypt`.

#### `barrow/steps` · The Barrow Steps · #20 · P0

- **Size:** 1600 × 900, 1 panel (canvas x 0–1600). Street, enclosed (underground). Underground, daylight at the door; dark (ambient 0.07).
- **Doors:** none. **Ways out:** west to The Old Road; east to The Barrow Hall.
- **Used in:** the barrow side quest (from the fork).
- **Basis:** `paint_rooms_under.py` (`barrow_steps`); the zone file (the way back to the road at the west, the hall to the east, a sconce, the daylight at the door).
- **Derived:** the portal's three stones as seen from inside.
- **The game's own pieces:** The game hangs its own sconce on the wall and adds the dust and candle flicker.

##### `barrow/steps` · panel 1 of 1

- **Attach:** `shots/art/room-templates/barrow-steps-1.png`.
- **Send to Claude as:** `barrow-steps-1.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Barrow: The Barrow Steps. This one picture is the whole room.

LAYOUT, as the attached guide shows it. This place is enclosed, with no open sky: the back wall fills the picture from the top down to the floor line. The floor line, where the back wall and everything against it meets the floor, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even grey-violet flagstones. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person.

DOORS. There is no door box in this panel, so nothing here should look like a door you could use, apart from the ways out named below; any building's own door stays small, plain and shut.

WAYS OUT. The left edge is a way out, to The Old Road: let the scene and the floor run on off the left edge. The right edge is a way out, to The Barrow Hall: let the scene and the floor run on off the right edge.

SCENE, left to right. Inside the barrow, the passage just in from the door. No sky. From the left edge to about 15%: the portal at the west end, a rough doorway of three great stones, bright daylight and green grass and blue sky seen through it, the light spilling a little way in. From about 15% to 85%: a passage of huge grey-violet stones, the walls built of big blocks under massive lintel stones, carved with spirals and rings, tree roots pushing through the cracks overhead and hanging down, small burial niches in the walls holding clay urns and stubby candles. From about 85% to the right edge: an arch at the east end opening on darkness, a few steps going down into the dark toward the hall. Mood: ancient, hushed, a little eerie, but beautiful stonework.

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from the top of the walls to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

#### `barrow/hall` · The Barrow Hall · #21 · P0

- **Size:** 3200 × 900, 3 panels (canvas x 0–1600, 1400–3000, 1600–3200). Street, enclosed (underground). Underground; dark (ambient 0.07).
- **Doors:** gate *Stone door* at x 420; door *Tomb door* at x 1800. **Ways out:** west to The Barrow Steps.
- **Used in:** the barrow side quest (the lever, the side room).
- **Basis:** `12-rooms.md` §6; `paint_rooms_under.py` (`barrow_hall`) and its preview; the zone file (the stone door to the side room, the tomb door, the lever and sconces).
- **Derived:** the sarcophagus bay, the carved end wall.
- **The game's own pieces:** The game hangs its own sconces on the piers, stands its own urns on the floor, and puts the lever on the wall near the east end.

##### `barrow/hall` · panel 1 of 3

- **Attach:** `shots/art/room-templates/barrow-hall-1.png`.
- **Send to Claude as:** `barrow-hall-1.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Barrow: The Barrow Hall. This is panel 1 of 3, the left part of a room that scrolls sideways; the scene carries on past its right edge.

LAYOUT, as the attached guide shows it. This place is enclosed, with no open sky: the back wall fills the picture from the top down to the floor line. The floor line, where the back wall and everything against it meets the floor, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even grey-violet flagstones. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person.

DOORS. Draw each of these exactly inside its labelled box on the guide, filling the box, its threshold on the floor line (the names are only for you: write nothing): a great door or gateway (Stone door), from about 17% to 36% across, its top about 36% down. Any other door in this panel stays small, plain and shut, so these are clearly the ones to use.

WAYS OUT. The left edge is a way out, to The Barrow Steps: let the scene and the floor run on off the left edge.

SCENE, left to right. The barrow's long hall, underground, no sky. Grey-violet stone throughout, built in great blocks. Square stone piers at about 8%, 40%, 70% and 100%, with round arches springing between them under a vaulted ceiling, tree roots hanging down through the vault, cobwebs in the corners of the arches. From the left edge to about 8%: the low arch back to the steps, dark beyond. From about 17% to 36%: the stone door to the side room, a massive slab of stone carved with spirals, set in a heavy round arch, filling its box. From about 42% to 68%: a bay with a big carved spiral roundel high on the wall over a stone sarcophagus set back in a recess. From about 72% to the right edge: walls of small round-topped burial niches in rows, holding clay urns, little candles and a few old bones. Mood: ancient, solemn, still.

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from the top of the walls to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

##### `barrow/hall` · panel 2 of 3

- **Attach:** `shots/art/room-templates/barrow-hall-2.png`, and also attach your panel 1 picture (`barrow-hall-1.png`).
- **Send to Claude as:** `barrow-hall-2.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Barrow: The Barrow Hall. This is panel 2 of 3, the middle part of a room that scrolls sideways; the scene carries on past its left and right edges.

CONTINUATION. The second attached picture is panel 1 of this room. This panel carries the scene on to the right of it: the left 12% of this picture (about 192 px) shows exactly the same stretch as the right 12% of panel 1, so redraw that stretch to match it (the same buildings, trees, sky, clouds, colours, horizon, floor line, scale and light) and carry on seamlessly from there. Only the right 1344 px are new.

LAYOUT, as the attached guide shows it. This place is enclosed, with no open sky: the back wall fills the picture from the top down to the floor line. The floor line, where the back wall and everything against it meets the floor, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even grey-violet flagstones. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person.

DOORS. Draw each of these exactly inside its labelled box on the guide, filling the box, its threshold on the floor line (the names are only for you: write nothing): a door (Tomb door), from about 21% to 29% across, its top about 45% down. Any other door in this panel stays small, plain and shut, so these are clearly the ones to use.

SCENE, left to right. The middle of the barrow's long hall, underground. Grey-violet stone, square piers at about 15%, 50% and 75% with round arches between them under the vault, roots hanging through, cobwebs. From the left edge to about 13%: walls of burial niches with urns and candles. From about 17% to 33%: the tomb door, a smaller stone door in a deep arched recess, carved with a sun, between two carved sun roundels on the wall either side of the recess; its door is in the box. From about 35% to 60%: bays open on the dark of the barrow going on beyond, deep shadow with faint pillars receding in it. From about 65% to the right edge: more walls of burial niches with urns and candles (the game puts its lever on the wall near about 94%: keep that spot plain stone).

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from the top of the walls to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

##### `barrow/hall` · panel 3 of 3

- **Attach:** `shots/art/room-templates/barrow-hall-3.png`, and also attach your panel 2 picture (`barrow-hall-2.png`).
- **Send to Claude as:** `barrow-hall-3.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Barrow: The Barrow Hall. This is panel 3 of 3, the right part of a room that scrolls sideways; the scene carries on past its left edge.

CONTINUATION. The second attached picture is panel 2 of this room. This panel carries the scene on to the right of it: the left 88% of this picture (about 1344 px) shows exactly the same stretch as the right 88% of panel 2, so redraw that stretch to match it (the same buildings, trees, sky, clouds, colours, horizon, floor line, scale and light) and carry on seamlessly from there. Only the right 192 px are new.

LAYOUT, as the attached guide shows it. This place is enclosed, with no open sky: the back wall fills the picture from the top down to the floor line. The floor line, where the back wall and everything against it meets the floor, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even grey-violet flagstones. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person.

DOORS. Draw each of these exactly inside its labelled box on the guide, filling the box, its threshold on the floor line (the names are only for you: write nothing): a door (Tomb door), from about 9% to 16% across, its top about 45% down. Any other door in this panel stays small, plain and shut, so these are clearly the ones to use.

WAYS OUT. The room's right end is closed: end it at the right with solid wall or rock, as the scene says.

SCENE, left to right. The east end of the barrow's long hall, underground. Grey-violet stone, square piers at about 30% and 60% with round arches between them, roots hanging through the vault, cobwebs. From the left edge to about 20%: the tomb door, a small stone door in an arched recess carved with a sun, between two sun roundels; its door is in the box. From about 22% to 50%: bays open on deep darkness, faint pillars in it. From about 55% to 95%: walls of burial niches with urns and candles (keep about 81% plain stone: the game puts its lever there). From about 95% to the right edge: the hall ends in a solid wall of great stones carved with a big spiral. Mood: ancient, solemn, watchful.

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from the top of the walls to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

#### `barrow/side-room` · The Side Room · #22 · P0

- **Size:** 1600 × 900, 1 panel (canvas x 0–1600). Street, enclosed (underground). Underground; dark (ambient 0.07).
- **Doors:** gate *Stone door* at x 260. **Ways out:** none.
- **Used in:** the barrow side quest (the chest).
- **Basis:** `paint_rooms_under.py` (`barrow_side`); the zone file (the stone door back to the hall, a sconce, an urn, the chest).
- **Derived:** the effigy's details.
- **The game's own pieces:** The game stands its own chest in front of the sarcophagus, an urn on the floor, and hangs a sconce.

##### `barrow/side-room` · panel 1 of 1

- **Attach:** `shots/art/room-templates/barrow-side-room-1.png`.
- **Send to Claude as:** `barrow-side-room-1.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Barrow: The Side Room. This one picture is the whole room.

LAYOUT, as the attached guide shows it. This place is enclosed, with no open sky: the back wall fills the picture from the top down to the floor line. The floor line, where the back wall and everything against it meets the floor, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even grey-violet flagstones. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person.

DOORS. Draw each of these exactly inside its labelled box on the guide, filling the box, its threshold on the floor line (the names are only for you: write nothing): a great door or gateway (Stone door), from about 7% to 26% across, its top about 36% down. Any other door in this panel stays small, plain and shut, so these are clearly the ones to use.

WAYS OUT. The room's left end is closed: end it at the left with solid wall or rock, as the scene says. The room's right end is closed: end it at the right with solid wall or rock, as the scene says.

SCENE, left to right. The barrow's side room, a small burial chamber underground, no sky. Grey-violet stone walls of great blocks, a low vaulted ceiling with roots pushing through. From about 7% to 26%: the stone door back to the hall, a massive slab carved with spirals in a heavy round arch, filling its box. From about 30% to 47%: rows of small round-topped burial niches with urns and candles. From about 50% to 75%: set back against the wall on a low stone dais, a stone sarcophagus with a carved knight lying on its lid, hands folded on a sword, and a carved spiral roundel on the wall above it. From about 78% to the right edge: more burial niches. The chamber is closed on both sides. Mood: quiet, reverent, old.

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from the top of the walls to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```


### 3.14 `mirewood` · The Mirewood

The forest going bad: lovely at its edge, black-veined and poisoned deeper in, at dusk. Tiles: `mirewood`.

#### `mirewood/edge` · The Mirewood's Edge · #23 · P0

- **Size:** 3200 × 900, 3 panels (canvas x 0–1600, 1400–3000, 1600–3200). Street. Dusk; dark (ambient 0.62); tint 196 188 226.
- **Doors:** none. **Ways out:** west to The Old Road; east to The Deep Wood.
- **Used in:** A1-C2-01 (the gloomfangs, Sera).
- **Basis:** `12-rooms.md` §6 (the forest's edge, the road); `03b-world-art.md` §1 (black-veined trees, twilight tint); `paint_rooms_wild.py` (`edge`); the zone file (the painter's oaks, blossom tree and black-veined trees, the lavender tint).
- **Derived:** the order of the leaves turning.
- **The game's own pieces:** The game stands its own trees, bushes, flowers, a log, a ranger's sign and a chest on the grass, and tints the room lavender.

##### `mirewood/edge` · panel 1 of 3

- **Attach:** `shots/art/room-templates/mirewood-edge-1.png`.
- **Send to Claude as:** `mirewood-edge-1.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Mirewood: The Mirewood's Edge. This is panel 1 of 3, the left part of a room that scrolls sideways; the scene carries on past its right edge.

LAYOUT, as the attached guide shows it. The floor line, where every wall, doorstep, tree and rock meets the ground, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even forest grass and a packed earth trail. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person. The far horizon is a straight, level line about halfway down the picture (y 512 of 1024), at the eye height of someone standing on the floor; the distance (sea, hills, the far city) sits on it, and above the roofs and trees is sky.

DOORS. There is no door box in this panel, so nothing here should look like a door you could use, apart from the ways out named below; any building's own door stays small, plain and shut.

WAYS OUT. The left edge is a way out, to The Old Road: let the scene and the floor run on off the left edge.

SCENE, left to right. The edge of the Mirewood at dusk, where the road from the meadows goes in under the first trees. From the left edge to about 22%: a pink blossom tree at about 12% and a round green oak at about 18%, still bright and lush, ferns and flowering bushes at their feet. From about 30% to 70%: tall oaks with big round crowns of fresh green leaves at about 54%, 56% and 58%, a fresh light-green oak at about 82% and another oak at the right edge, their trunks mossy at the roots, ferns, bracken and round bushes along their feet, the trail going in under them. Through the gaps between the trunks, far back: more trees, softer and bluer. Sky (showing between and over the crowns): dusk, deep blue at the top going to violet and then warm pink at the horizon, a few pink-lit clouds. Mood: lovely and calm at first, the light starting to go.

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly. The game also washes the whole room in a cool lavender dusk, so keep the colours clean rather than pushing them that way.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from sky to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

##### `mirewood/edge` · panel 2 of 3

- **Attach:** `shots/art/room-templates/mirewood-edge-2.png`, and also attach your panel 1 picture (`mirewood-edge-1.png`).
- **Send to Claude as:** `mirewood-edge-2.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Mirewood: The Mirewood's Edge. This is panel 2 of 3, the middle part of a room that scrolls sideways; the scene carries on past its left and right edges.

CONTINUATION. The second attached picture is panel 1 of this room. This panel carries the scene on to the right of it: the left 12% of this picture (about 192 px) shows exactly the same stretch as the right 12% of panel 1, so redraw that stretch to match it (the same buildings, trees, sky, clouds, colours, horizon, floor line, scale and light) and carry on seamlessly from there. Only the right 1344 px are new.

LAYOUT, as the attached guide shows it. The floor line, where every wall, doorstep, tree and rock meets the ground, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even forest grass and a packed earth trail. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person. The far horizon is a straight, level line about halfway down the picture (y 512 of 1024), at the eye height of someone standing on the floor; the distance (sea, hills, the far city) sits on it, and above the roofs and trees is sky.

DOORS. There is no door box in this panel, so nothing here should look like a door you could use, apart from the ways out named below; any building's own door stays small, plain and shut.

SCENE, left to right. Deeper in at the Mirewood's edge, at dusk. Oaks at about 19% and 26%, still green. From about 40% to 65%: the leaves turning, an oak in amber and gold at about 46%, dark-green older oaks at about 61% and 74%, moss thicker on their trunks, the bushes thinning. From about 82% to the right edge: something wrong: a pale tree at about 88% whose bark is split by thin black veins with a faint violet light glowing in the cracks, an amber oak at about 89%, a first thin mist lying along the ground. Far back between the trunks: the wood darker and bluer. Sky between the crowns: deep blue going violet, a little pink at the horizon. Mood: the wood getting older and stranger.

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly. The game also washes the whole room in a cool lavender dusk, so keep the colours clean rather than pushing them that way.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from sky to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

##### `mirewood/edge` · panel 3 of 3

- **Attach:** `shots/art/room-templates/mirewood-edge-3.png`, and also attach your panel 2 picture (`mirewood-edge-2.png`).
- **Send to Claude as:** `mirewood-edge-3.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Mirewood: The Mirewood's Edge. This is panel 3 of 3, the right part of a room that scrolls sideways; the scene carries on past its left edge.

CONTINUATION. The second attached picture is panel 2 of this room. This panel carries the scene on to the right of it: the left 88% of this picture (about 1344 px) shows exactly the same stretch as the right 88% of panel 2, so redraw that stretch to match it (the same buildings, trees, sky, clouds, colours, horizon, floor line, scale and light) and carry on seamlessly from there. Only the right 192 px are new.

LAYOUT, as the attached guide shows it. The floor line, where every wall, doorstep, tree and rock meets the ground, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even forest grass and a packed earth trail. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person. The far horizon is a straight, level line about halfway down the picture (y 512 of 1024), at the eye height of someone standing on the floor; the distance (sea, hills, the far city) sits on it, and above the roofs and trees is sky.

DOORS. There is no door box in this panel, so nothing here should look like a door you could use, apart from the ways out named below; any building's own door stays small, plain and shut.

WAYS OUT. The right edge is a way out, to The Deep Wood: let the scene and the floor run on off the right edge.

SCENE, left to right. The east end of the Mirewood's edge, at dusk. An oak at about 6% and a green one at about 14%, an amber oak at about 34%, dark old oaks at about 49% and 62%. From about 70% to the right edge: the black-veined trees, a pale one at about 75% and a dark one at about 93%, their bark split by black veins with faint violet light in the cracks, an amber oak at about 77%, mist lying low along the ground, the trail going on east into the deep wood. Sky between the crowns: deep blue to violet, nearly night at the top. Mood: uneasy, quiet, wrong.

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly. The game also washes the whole room in a cool lavender dusk, so keep the colours clean rather than pushing them that way.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from sky to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

#### `mirewood/deep` · The Deep Wood · #24 · P0

- **Size:** 3200 × 900, 3 panels (canvas x 0–1600, 1400–3000, 1600–3200). Street. Dusk going to dark; dark (ambient 0.62); tint 196 188 226.
- **Doors:** gate *The Old Quarry* at x 2120. **Ways out:** west to The Mirewood's Edge; east to The Rangers' Camp.
- **Used in:** A1-C2-01 and A1-C2-02 (the way to the quarry).
- **Basis:** `12-rooms.md` §6 (old trees, moss, mist, black-veined in Act 1); `paint_rooms_wild.py` (`deep`); the zone file (the quarry's door, the painter's footbridge, dead tree, snags and black-veined trees).
- **Derived:** the quarry mouth's timbers and rails.
- **The game's own pieces:** The game stands its own trees, tar pools, gloom mushrooms, logs, a stump, bones, ranger signs and chests on the ground, and tints the room lavender.

##### `mirewood/deep` · panel 1 of 3

- **Attach:** `shots/art/room-templates/mirewood-deep-1.png`.
- **Send to Claude as:** `mirewood-deep-1.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Mirewood: The Deep Wood. This is panel 1 of 3, the left part of a room that scrolls sideways; the scene carries on past its right edge.

LAYOUT, as the attached guide shows it. The floor line, where every wall, doorstep, tree and rock meets the ground, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even dark wilted grass and leaf litter. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person. The far horizon is a straight, level line about halfway down the picture (y 512 of 1024), at the eye height of someone standing on the floor; the distance (sea, hills, the far city) sits on it, and above the roofs and trees is sky.

DOORS. There is no door box in this panel, so nothing here should look like a door you could use, apart from the ways out named below; any building's own door stays small, plain and shut.

WAYS OUT. The left edge is a way out, to The Mirewood's Edge: let the scene and the floor run on off the left edge.

SCENE, left to right. The deep wood of the Mirewood at dusk. Huge old trees with roots like walls and moss climbing their trunks. From the left edge to about 25%: an old oak with amber leaves and enormous roots. At about 35%: a dead, grey, leafless tree. From about 40% to 58%: the rangers' footbridge, a rope-and-plank bridge slung across a gully behind the trail, black tar gleaming at the bottom of the gully; a dead snag at about 54%. From about 62% to the right edge: old mossy trunks, and at about 92% a big tree whose bark is split by black veins with faint violet light glowing in the cracks. Mist lies along the ground. Between the trunks, far back: dark blue-green gloom, more trunks fading into it. Sky (small patches through the canopy): deep blue going violet. Mood: old, damp, poisoned, watched.

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly. The game also washes the whole room in a cool lavender dusk, so keep the colours clean rather than pushing them that way.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from sky to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

##### `mirewood/deep` · panel 2 of 3

- **Attach:** `shots/art/room-templates/mirewood-deep-2.png`, and also attach your panel 1 picture (`mirewood-deep-1.png`).
- **Send to Claude as:** `mirewood-deep-2.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Mirewood: The Deep Wood. This is panel 2 of 3, the middle part of a room that scrolls sideways; the scene carries on past its left and right edges.

CONTINUATION. The second attached picture is panel 1 of this room. This panel carries the scene on to the right of it: the left 12% of this picture (about 192 px) shows exactly the same stretch as the right 12% of panel 1, so redraw that stretch to match it (the same buildings, trees, sky, clouds, colours, horizon, floor line, scale and light) and carry on seamlessly from there. Only the right 1344 px are new.

LAYOUT, as the attached guide shows it. The floor line, where every wall, doorstep, tree and rock meets the ground, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even dark wilted grass and leaf litter. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person. The far horizon is a straight, level line about halfway down the picture (y 512 of 1024), at the eye height of someone standing on the floor; the distance (sea, hills, the far city) sits on it, and above the roofs and trees is sky.

DOORS. Draw each of these exactly inside its labelled box on the guide, filling the box, its threshold on the floor line (the names are only for you: write nothing): a great door or gateway (The Old Quarry), from about 36% to 54% across, its top about 36% down. Any other door in this panel stays small, plain and shut, so these are clearly the ones to use.

SCENE, left to right. The deep wood at dusk, by the old quarry. At about 4%: a big black-veined tree, violet light in its cracks. From about 15% to 34%: old, twisted, mossy trunks with black veins starting in them. From about 36% to 54%: an outcrop of grey rock rising out of the forest floor, and cut into it the old quarry's mouth, a dark opening framed by heavy old timber posts and a lintel beam, iron spikes and a rusty lamp hook in the wood, old cart rails running into the dark (the great door in the box is this mouth). From about 58% to the right edge: twisted old trees, a black-veined one leaning at about 75%, a dead snag at the right edge. Mist low along the ground. Sky through the canopy: violet going dark. Mood: something bad is down there.

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly. The game also washes the whole room in a cool lavender dusk, so keep the colours clean rather than pushing them that way.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from sky to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

##### `mirewood/deep` · panel 3 of 3

- **Attach:** `shots/art/room-templates/mirewood-deep-3.png`, and also attach your panel 2 picture (`mirewood-deep-2.png`).
- **Send to Claude as:** `mirewood-deep-3.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Mirewood: The Deep Wood. This is panel 3 of 3, the right part of a room that scrolls sideways; the scene carries on past its left edge.

CONTINUATION. The second attached picture is panel 2 of this room. This panel carries the scene on to the right of it: the left 88% of this picture (about 1344 px) shows exactly the same stretch as the right 88% of panel 2, so redraw that stretch to match it (the same buildings, trees, sky, clouds, colours, horizon, floor line, scale and light) and carry on seamlessly from there. Only the right 192 px are new.

LAYOUT, as the attached guide shows it. The floor line, where every wall, doorstep, tree and rock meets the ground, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even dark wilted grass and leaf litter. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person. The far horizon is a straight, level line about halfway down the picture (y 512 of 1024), at the eye height of someone standing on the floor; the distance (sea, hills, the far city) sits on it, and above the roofs and trees is sky.

DOORS. Draw each of these exactly inside its labelled box on the guide, filling the box, its threshold on the floor line (the names are only for you: write nothing): a great door or gateway (The Old Quarry), from about 23% to 42% across, its top about 36% down. Any other door in this panel stays small, plain and shut, so these are clearly the ones to use.

WAYS OUT. The right edge is a way out, to The Rangers' Camp: let the scene and the floor run on off the right edge.

SCENE, left to right. The east end of the deep wood, at dusk. From about 23% to 42%: the grey rock outcrop with the old quarry's timbered mouth, a dark opening framed by heavy old posts and a lintel beam, cart rails running into it (the great door in the box is this mouth). From about 45% to 85%: twisted, mossy old trees, a black-veined one leaning at about 62%, violet light in its cracks. A dead snag at about 87%. From about 88% to the right edge: the trail going on east under the trees toward the rangers' camp. Mist low along the ground. Sky through the canopy: violet going dark. Mood: heavy, poisoned, quiet.

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly. The game also washes the whole room in a cool lavender dusk, so keep the colours clean rather than pushing them that way.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from sky to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

#### `mirewood/camp` · The Rangers' Camp · #28 · P0

- **Size:** 1600 × 900, 1 panel (canvas x 0–1600). Street. Night, firelight; dark (ambient 0.62); tint 196 188 226.
- **Doors:** none. **Ways out:** west to The Deep Wood.
- **Used in:** A1-C2-03 (camp in the Mirewood); resting.
- **Basis:** `12-rooms.md` §6 (camp); `paint_rooms_wild.py` (`camp`); the zone file (the campfire as the game's prop, the painter's foreground oaks).
- **Derived:** the tents' colours, the stump table.
- **The game's own pieces:** The game stands its own campfire in the middle (about x 800), logs, bedrolls, crates, a barrel, trees and a waystone on the grass, and tints the room lavender.

##### `mirewood/camp` · panel 1 of 1

- **Attach:** `shots/art/room-templates/mirewood-camp-1.png`.
- **Send to Claude as:** `mirewood-camp-1.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Mirewood: The Rangers' Camp. This one picture is the whole room.

LAYOUT, as the attached guide shows it. The floor line, where every wall, doorstep, tree and rock meets the ground, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even forest grass. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person. The far horizon is a straight, level line about halfway down the picture (y 512 of 1024), at the eye height of someone standing on the floor; the distance (sea, hills, the far city) sits on it, and above the roofs and trees is sky.

DOORS. There is no door box in this panel, so nothing here should look like a door you could use, apart from the ways out named below; any building's own door stays small, plain and shut.

WAYS OUT. The left edge is a way out, to The Deep Wood: let the scene and the floor run on off the left edge. The room's right end is closed: end the scene at the right with a building, a wall, rocks or trees, as the scene says.

SCENE, left to right. The rangers' camp, a clearing in the Mirewood at night, by firelight. From the left edge to about 12%: the dark trunk of a big oak. From about 15% to 35%: a ranger's canvas tent in forest green, its flap tied back, a stack of firewood beside it. From about 38% to 60%: the clearing open at the back, a wide tree stump used as a table with a map weighted down on it at about 45%, and above the treetops a round moon at about 50% among stars. From about 62% to 86%: a second canvas tent in faded tan, a wooden rack of bows and spears at about 68%. From about 88% to the right edge: another dark oak trunk. Small lanterns strung on a cord between the trunks, lit warm. The firelight (from the game's campfire in the middle of the floor) lights the near faces of the tents and trunks warm orange; everything further back is cool moonlit blue. The trees close in on both sides; the left edge is the trail back. Mood: safe for one night, tired, warm.

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly. The game also washes the whole room in a cool lavender dusk, so keep the colours clean rather than pushing them that way.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from sky to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```


### 3.15 `mirewood-quarry` · The Old Quarry

The old quarry under the Mirewood where the rot comes from: cut stone, black roots, a black crystal. Underground. Tiles: `quarry`. No room here has a painting recipe yet, so more of it is derived.

#### `mirewood-quarry/lip` · The Quarry's Lip · #25 · P0

- **Size:** 3200 × 900, 3 panels (canvas x 0–1600, 1400–3000, 1600–3200). Street, enclosed (underground). Underground, daylight at the ramp; dark (ambient 0.1).
- **Doors:** none. **Ways out:** west to The Mirewood; east to The Second Cut.
- **Used in:** A1-C2-02 (the first roots).
- **Basis:** `03b-world-art.md` §1 and §3 (root puzzles, the black crystal, the quarry tileset); `tools/rooms/plan.py` (the lip is the upper cut, in from the ramp); the zone file (the daylight at the west end, the fungus and crystal lights, the store's crates, the root wall at the east exit).
- **Derived:** nearly everything here: no painting recipe exists yet. The gallery, the chasm, the winch, the timber props.
- **The game's own pieces:** The game stands its own crates, barrels, sacks, a log, rocks, gloom mushrooms, bones and chests on the floor, and its own wall of black roots across the east way until Sera parts it.

##### `mirewood-quarry/lip` · panel 1 of 3

- **Attach:** `shots/art/room-templates/mirewood-quarry-lip-1.png`.
- **Send to Claude as:** `mirewood-quarry-lip-1.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Old Quarry: The Quarry's Lip. This is panel 1 of 3, the left part of a room that scrolls sideways; the scene carries on past its right edge.

LAYOUT, as the attached guide shows it. This place is enclosed, with no open sky: the back wall fills the picture from the top down to the floor line. The floor line, where the back wall and everything against it meets the floor, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even worn cut stone. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person.

DOORS. There is no door box in this panel, so nothing here should look like a door you could use, apart from the ways out named below; any building's own door stays small, plain and shut.

WAYS OUT. The left edge is a way out, to The Mirewood: let the scene and the floor run on off the left edge.

SCENE, left to right. The Old Quarry's lip: an old stone quarry cut deep into the rock under the Mirewood, gone dark and roofed over by the forest's roots. From the left edge to about 12%: the ramp in from the woods, a ragged opening in the rock high on the left with daylight and green leaves beyond it, the light falling in on the stone. From about 12% to 35%: the old quarry store, a lean-to of grey timber against the rock with empty shelves and hooks (the game stands crates and barrels in front). From about 35% to the right edge: the upper gallery: a back wall of cut grey stone in big blocky steps with chisel marks, old timber props and beams holding up the rock, iron rings set in the stone, a rusty winch with a rope drum at about 60%, a cluster of softly glowing blue-green fungus on the rock at about 49%, the first black roots creeping down out of the cracks above. No sky except at the ramp. Mood: abandoned, damp, unsettling.

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from the top of the walls to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

##### `mirewood-quarry/lip` · panel 2 of 3

- **Attach:** `shots/art/room-templates/mirewood-quarry-lip-2.png`, and also attach your panel 1 picture (`mirewood-quarry-lip-1.png`).
- **Send to Claude as:** `mirewood-quarry-lip-2.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Old Quarry: The Quarry's Lip. This is panel 2 of 3, the middle part of a room that scrolls sideways; the scene carries on past its left and right edges.

CONTINUATION. The second attached picture is panel 1 of this room. This panel carries the scene on to the right of it: the left 12% of this picture (about 192 px) shows exactly the same stretch as the right 12% of panel 1, so redraw that stretch to match it (the same buildings, trees, sky, clouds, colours, horizon, floor line, scale and light) and carry on seamlessly from there. Only the right 1344 px are new.

LAYOUT, as the attached guide shows it. This place is enclosed, with no open sky: the back wall fills the picture from the top down to the floor line. The floor line, where the back wall and everything against it meets the floor, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even worn cut stone. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person.

DOORS. There is no door box in this panel, so nothing here should look like a door you could use, apart from the ways out named below; any building's own door stays small, plain and shut.

SCENE, left to right. The Old Quarry's lip, the upper gallery. A back wall of cut grey stone in big blocky steps with chisel marks, old timber props and beams, and from about 15% to 55% the wall drops away behind a low broken timber rail into a dark chasm, the far wall of the quarry across it cut in old terraces going down into darkness. At about 31%: violet-black crystal glinting in a crack in the rock. At about 66%: a cluster of glowing blue-green fungus. Black roots grow down out of the rock above, more of them toward the right, and thin black veins run through the stone. No sky. Mood: deep, cold, wrong.

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from the top of the walls to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

##### `mirewood-quarry/lip` · panel 3 of 3

- **Attach:** `shots/art/room-templates/mirewood-quarry-lip-3.png`, and also attach your panel 2 picture (`mirewood-quarry-lip-2.png`).
- **Send to Claude as:** `mirewood-quarry-lip-3.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Old Quarry: The Quarry's Lip. This is panel 3 of 3, the right part of a room that scrolls sideways; the scene carries on past its left edge.

CONTINUATION. The second attached picture is panel 2 of this room. This panel carries the scene on to the right of it: the left 88% of this picture (about 1344 px) shows exactly the same stretch as the right 88% of panel 2, so redraw that stretch to match it (the same buildings, trees, sky, clouds, colours, horizon, floor line, scale and light) and carry on seamlessly from there. Only the right 192 px are new.

LAYOUT, as the attached guide shows it. This place is enclosed, with no open sky: the back wall fills the picture from the top down to the floor line. The floor line, where the back wall and everything against it meets the floor, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even worn cut stone. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person.

DOORS. There is no door box in this panel, so nothing here should look like a door you could use, apart from the ways out named below; any building's own door stays small, plain and shut.

WAYS OUT. The right edge is a way out, to The Second Cut: let the scene and the floor run on off the right edge.

SCENE, left to right. The east end of the quarry's lip. A back wall of cut grey stone with chisel marks and old timber props, a dark chasm behind a broken rail from the left edge to about 40%, the far terraces fading into the dark. At about 50%: a cluster of glowing blue-green fungus. From about 60% to the right edge: thick black roots grow down out of the ceiling rock and over the back wall, knotted and glossy, thin violet light in their cracks. At the right edge: the dark mouth of a passage going on deeper; keep its opening clear of roots, because the game draws its own wall of roots across it. No sky. Mood: choked, menacing.

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from the top of the walls to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

#### `mirewood-quarry/cut` · The Second Cut · #26 · P0

- **Size:** 1600 × 900, 1 panel (canvas x 0–1600). Street, enclosed (underground). Underground; dark (ambient 0.1).
- **Doors:** none. **Ways out:** west to The Quarry's Lip; east to The Quarry Floor.
- **Used in:** A1-C2-02 (the second roots).
- **Basis:** the zone file (root floor all along, fungus and crystal lights, the root wall at the east exit); `tools/rooms/plan.py` (the second cut, past the first roots).
- **Derived:** everything: no painting recipe exists yet.
- **The game's own pieces:** The game stands its own bones and gloom mushrooms on the floor, and its own wall of black roots across the east way.

##### `mirewood-quarry/cut` · panel 1 of 1

- **Attach:** `shots/art/room-templates/mirewood-quarry-cut-1.png`.
- **Send to Claude as:** `mirewood-quarry-cut-1.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Old Quarry: The Second Cut. This one picture is the whole room.

LAYOUT, as the attached guide shows it. This place is enclosed, with no open sky: the back wall fills the picture from the top down to the floor line. The floor line, where the back wall and everything against it meets the floor, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even broken stone overgrown with roots. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person.

DOORS. There is no door box in this panel, so nothing here should look like a door you could use, apart from the ways out named below; any building's own door stays small, plain and shut.

WAYS OUT. The left edge is a way out, to The Quarry's Lip: let the scene and the floor run on off the left edge. The right edge is a way out, to The Quarry Floor: let the scene and the floor run on off the right edge.

SCENE, left to right. The Second Cut in the Old Quarry: a narrow, high slot cut through grey rock, deeper underground. The back wall is close: cut grey stone with chisel marks, black roots crawling down it and over it from above, thick as arms, knotted and glossy, thin violet light in their cracks, black veins running into the stone. Glowing blue-green fungus at about 17% and 83%. At about 67%: a black crystal shard jutting from the rock with a violet edge. Old timber props, half-rotten, at about 30% and 55%. From the left edge: the way back to the lip, a darker opening. At the right edge: a dark passage going on down; keep its opening clear, because the game draws its own wall of roots across it. No sky. Mood: tight, choked, going wrong.

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from the top of the walls to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

#### `mirewood-quarry/floor` · The Quarry Floor · #27 · P0

- **Size:** 3200 × 900, 3 panels (canvas x 0–1600, 1400–3000, 1600–3200). Street, enclosed (underground). Underground, a violet glow; dark (ambient 0.1).
- **Doors:** none. **Ways out:** west to The Second Cut.
- **Used in:** A1-C2-02 (the courier's body, the Thornmother, the black crystal).
- **Basis:** `03b-world-art.md` §1 and §3 (the black crystal spike, the courier's body, the Thornmother's den); the Act 1 script (A1-C2-02); the zone file (the crystal at x 2393, the den's crystal lights, the tar seep at the west end).
- **Derived:** everything about the den's look: no painting recipe exists yet.
- **The game's own pieces:** The game stands its own black crystal spike (about x 2393, in its three states), the courier's body and satchel, bones and gloom mushrooms, and the den's chest; the Thornmother is a creature.

##### `mirewood-quarry/floor` · panel 1 of 3

- **Attach:** `shots/art/room-templates/mirewood-quarry-floor-1.png`.
- **Send to Claude as:** `mirewood-quarry-floor-1.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Old Quarry: The Quarry Floor. This is panel 1 of 3, the left part of a room that scrolls sideways; the scene carries on past its right edge.

LAYOUT, as the attached guide shows it. This place is enclosed, with no open sky: the back wall fills the picture from the top down to the floor line. The floor line, where the back wall and everything against it meets the floor, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even broken stone overgrown with roots. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person.

DOORS. There is no door box in this panel, so nothing here should look like a door you could use, apart from the ways out named below; any building's own door stays small, plain and shut.

WAYS OUT. The left edge is a way out, to The Second Cut: let the scene and the floor run on off the left edge.

SCENE, left to right. The floor of the Old Quarry, deep underground. The quarry's cut walls rise out of the top of the picture, grey stone in old terraces with chisel marks, black roots hanging down from far above like cables. From the left edge to about 12%: the passage in from the cut, a crack at its foot with black tar seeping out. From about 15% to 60%: the quarry's back wall with an abandoned stone sledge and a collapsed timber crane leaning on it, old tools rusting in a niche, glowing blue-green fungus in clusters at about 25% and 70%. From about 70% to the right edge: the roots thicken, black and thorny, and the stone starts to show glossy black veins with a faint violet light. No sky. Mood: a dead end at the bottom of the world.

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from the top of the walls to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

##### `mirewood-quarry/floor` · panel 2 of 3

- **Attach:** `shots/art/room-templates/mirewood-quarry-floor-2.png`, and also attach your panel 1 picture (`mirewood-quarry-floor-1.png`).
- **Send to Claude as:** `mirewood-quarry-floor-2.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Old Quarry: The Quarry Floor. This is panel 2 of 3, the middle part of a room that scrolls sideways; the scene carries on past its left and right edges.

CONTINUATION. The second attached picture is panel 1 of this room. This panel carries the scene on to the right of it: the left 12% of this picture (about 192 px) shows exactly the same stretch as the right 12% of panel 1, so redraw that stretch to match it (the same buildings, trees, sky, clouds, colours, horizon, floor line, scale and light) and carry on seamlessly from there. Only the right 1344 px are new.

LAYOUT, as the attached guide shows it. This place is enclosed, with no open sky: the back wall fills the picture from the top down to the floor line. The floor line, where the back wall and everything against it meets the floor, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even broken stone overgrown with roots. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person.

DOORS. There is no door box in this panel, so nothing here should look like a door you could use, apart from the ways out named below; any building's own door stays small, plain and shut.

SCENE, left to right. The Thornmother's den at the bottom of the Old Quarry. From the left edge to about 30%: the quarry's back wall of grey cut stone going black, thorny black roots crawling over it. From about 30% to 95%: the back opens into a great hollow knotted with black thorny roots, like a nest, and violet-black crystal growing out of the rock in clusters at about 44% and 81%, glinting; at about 62% the hollow's heart, where the game stands its own black crystal spike: paint the space behind it as a dark recess in the roots lit by a faint violet glow, with nothing standing in it. From about 95%: grey stone again, roots hanging. No sky. Mood: a lair, poisoned and pulsing.

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from the top of the walls to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

##### `mirewood-quarry/floor` · panel 3 of 3

- **Attach:** `shots/art/room-templates/mirewood-quarry-floor-3.png`, and also attach your panel 2 picture (`mirewood-quarry-floor-2.png`).
- **Send to Claude as:** `mirewood-quarry-floor-3.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Old Quarry: The Quarry Floor. This is panel 3 of 3, the right part of a room that scrolls sideways; the scene carries on past its left edge.

CONTINUATION. The second attached picture is panel 2 of this room. This panel carries the scene on to the right of it: the left 88% of this picture (about 1344 px) shows exactly the same stretch as the right 88% of panel 2, so redraw that stretch to match it (the same buildings, trees, sky, clouds, colours, horizon, floor line, scale and light) and carry on seamlessly from there. Only the right 192 px are new.

LAYOUT, as the attached guide shows it. This place is enclosed, with no open sky: the back wall fills the picture from the top down to the floor line. The floor line, where the back wall and everything against it meets the floor, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even broken stone overgrown with roots. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person.

DOORS. There is no door box in this panel, so nothing here should look like a door you could use, apart from the ways out named below; any building's own door stays small, plain and shut.

WAYS OUT. The room's right end is closed: end it at the right with solid wall or rock, as the scene says.

SCENE, left to right. The far end of the Thornmother's den. From the left edge to about 75%: the great hollow knotted with black thorny roots, violet-black crystal clusters at about 31% and 68%, and at about 49% the dark recess behind where the game stands its own black crystal spike, lit by a faint violet glow, with nothing standing in it. From about 75% to the right edge: the quarry's grey cut wall closing the end, thick black roots draped over it, a cluster of glowing blue-green fungus at about 90%. The room is closed on the right. No sky. Mood: a lair, poisoned and pulsing.

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from the top of the walls to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```


### 3.16 `thornbridge` · Thornbridge

The refugee camp outside Questeria's east gate, by the river and the old bridge. Tiles: `meadow`.

#### `thornbridge/camp-gate` · The Camp Gate · #29 · P0

- **Size:** 1600 × 900, 1 panel (canvas x 0–1600). Street. Afternoon.
- **Doors:** none. **Ways out:** west to Questeria; east to The Tents.
- **Used in:** A1-C3-01 (the east gate: Brisk, Hedda and Lise).
- **Basis:** `12-rooms.md` §6 (the refugee camp's gate, Hedda's post); `paint_rooms_wild.py` (`camp_gate`); the zone file (the painter's city wall, cypresses and east gate).
- **Derived:** Hedda's post's shape and place.
- **The game's own pieces:** The game stands its own wildflowers on the grass.

##### `thornbridge/camp-gate` · panel 1 of 1

- **Attach:** `shots/art/room-templates/thornbridge-camp-gate-1.png`.
- **Send to Claude as:** `thornbridge-camp-gate-1.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. Thornbridge: The Camp Gate. This one picture is the whole room.

LAYOUT, as the attached guide shows it. The floor line, where every wall, doorstep, tree and rock meets the ground, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even meadow grass with a dirt road. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person. The far horizon is a straight, level line about halfway down the picture (y 512 of 1024), at the eye height of someone standing on the floor; the distance (sea, hills, the far city) sits on it, and above the roofs and trees is sky.

DOORS. There is no door box in this panel, so nothing here should look like a door you could use, apart from the ways out named below; any building's own door stays small, plain and shut.

WAYS OUT. The left edge is a way out, to Questeria: let the scene and the floor run on off the left edge. The right edge is a way out, to The Tents: let the scene and the floor run on off the right edge.

SCENE, left to right. Outside Questeria's east gate, where the refugee camp begins, on a hazy afternoon. From the left edge to about 8%: the cream city wall running off to the left, two tall dark cypresses at about 4% and 6%. From about 8% to 32%: Questeria's east gate, a round arch in the thick cream wall between two round towers under blue cones with navy banners, its gates open, the sunlit city with its colourful roofs seen through it; it's the backdrop to the way back into the city, which is the left edge. The road comes out of the gate along the floor. From about 40% to 56%: Hedda's post, a little lean-to shelter of planks and canvas with a notice board on a post beside it, papers pinned up (blank or with scribble marks only), a water butt. From about 60% to the right edge: open meadow sloping away, and the first of the refugees' tents, patched canvas in faded colours, from about 75%, thin smoke from cooking fires rising, a river valley in the distance. Sky: warm hazy afternoon blue, the sun at the upper left. Mood: the safe city on one side, worried people waiting on the other.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from sky to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

#### `thornbridge/tents` · The Tents · #30 · P0

- **Size:** 3200 × 900, 3 panels (canvas x 0–1600, 1400–3000, 1600–3200). Street. Afternoon, a little smoky.
- **Doors:** none. **Ways out:** west to The Camp Gate; east to The River's Edge.
- **Used in:** A1-C3-02 (Mira, the sick, the supplies).
- **Basis:** `12-rooms.md` §6 (rows of tents, the clinic tent, cooking fires); `paint_rooms_wild.py` (`tents`); the zone file (the painter's tents, the healer station, the city wall and cypresses, blossom at the east end).
- **Derived:** the clinic tent's emblem, the washing lines' places.
- **The game's own pieces:** The game stands its own cooking fires, cots, crates, sacks, bundles and a water barrel on the grass.

##### `thornbridge/tents` · panel 1 of 3

- **Attach:** `shots/art/room-templates/thornbridge-tents-1.png`.
- **Send to Claude as:** `thornbridge-tents-1.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. Thornbridge: The Tents. This is panel 1 of 3, the left part of a room that scrolls sideways; the scene carries on past its right edge.

LAYOUT, as the attached guide shows it. The floor line, where every wall, doorstep, tree and rock meets the ground, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even trodden meadow grass with a dirt road. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person. The far horizon is a straight, level line about halfway down the picture (y 512 of 1024), at the eye height of someone standing on the floor; the distance (sea, hills, the far city) sits on it, and above the roofs and trees is sky.

DOORS. There is no door box in this panel, so nothing here should look like a door you could use, apart from the ways out named below; any building's own door stays small, plain and shut.

WAYS OUT. The left edge is a way out, to The Camp Gate: let the scene and the floor run on off the left edge.

SCENE, left to right. Thornbridge: the refugees' tent camp in the meadow under Questeria's walls, on a hazy afternoon. Behind everything, in the far layer, the cream city wall runs along the top of the slope with its round towers and blue cones. From the left edge to about 28%: trodden meadow, a washing line strung between poles with patched clothes. From about 30% to 42%: a cluster of patched tents in faded red, ochre, blue and green canvas, sagging on their poles, bundles at their doors. From about 45% to 70%: crates, a cart without a horse, washing lines, thin smoke rising from cooking fires (the game stands the fires). From about 74% to the right edge: more patched tents at about 78% and 96%. Sky: warm hazy afternoon blue with smoke drifting across it, the sun at the upper left. Mood: crowded, tired, making do, kind.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from sky to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

##### `thornbridge/tents` · panel 2 of 3

- **Attach:** `shots/art/room-templates/thornbridge-tents-2.png`, and also attach your panel 1 picture (`thornbridge-tents-1.png`).
- **Send to Claude as:** `thornbridge-tents-2.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. Thornbridge: The Tents. This is panel 2 of 3, the middle part of a room that scrolls sideways; the scene carries on past its left and right edges.

CONTINUATION. The second attached picture is panel 1 of this room. This panel carries the scene on to the right of it: the left 12% of this picture (about 192 px) shows exactly the same stretch as the right 12% of panel 1, so redraw that stretch to match it (the same buildings, trees, sky, clouds, colours, horizon, floor line, scale and light) and carry on seamlessly from there. Only the right 1344 px are new.

LAYOUT, as the attached guide shows it. The floor line, where every wall, doorstep, tree and rock meets the ground, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even trodden meadow grass with a dirt road. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person. The far horizon is a straight, level line about halfway down the picture (y 512 of 1024), at the eye height of someone standing on the floor; the distance (sea, hills, the far city) sits on it, and above the roofs and trees is sky.

DOORS. There is no door box in this panel, so nothing here should look like a door you could use, apart from the ways out named below; any building's own door stays small, plain and shut.

SCENE, left to right. The middle of the Thornbridge tent camp, on a hazy afternoon. The city wall runs along the top of the slope in the far layer. At about 8%: a patched tent. From about 22% to 40%: the clinic, a big round bell tent of pale canvas with its door flap tied open, a green leaf painted over the door, a table of jars and bandages beside it. From about 48% to 68%: a stretch of the city wall come down close behind the camp, with two dark cypresses at about 51% and 65%, and a patched tent at about 54% and 66%. From about 80% to the right edge: more tents at about 85% and 94%, and pink blossom trees at about 88% and 97% as the camp reaches toward the river. Smoke from cooking fires. Sky: hazy afternoon blue.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from sky to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

##### `thornbridge/tents` · panel 3 of 3

- **Attach:** `shots/art/room-templates/thornbridge-tents-3.png`, and also attach your panel 2 picture (`thornbridge-tents-2.png`).
- **Send to Claude as:** `thornbridge-tents-3.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. Thornbridge: The Tents. This is panel 3 of 3, the right part of a room that scrolls sideways; the scene carries on past its left edge.

CONTINUATION. The second attached picture is panel 2 of this room. This panel carries the scene on to the right of it: the left 88% of this picture (about 1344 px) shows exactly the same stretch as the right 88% of panel 2, so redraw that stretch to match it (the same buildings, trees, sky, clouds, colours, horizon, floor line, scale and light) and carry on seamlessly from there. Only the right 192 px are new.

LAYOUT, as the attached guide shows it. The floor line, where every wall, doorstep, tree and rock meets the ground, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even trodden meadow grass with a dirt road. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person. The far horizon is a straight, level line about halfway down the picture (y 512 of 1024), at the eye height of someone standing on the floor; the distance (sea, hills, the far city) sits on it, and above the roofs and trees is sky.

DOORS. There is no door box in this panel, so nothing here should look like a door you could use, apart from the ways out named below; any building's own door stays small, plain and shut.

WAYS OUT. The right edge is a way out, to The River's Edge: let the scene and the floor run on off the right edge.

SCENE, left to right. The east end of the Thornbridge tent camp, on a hazy afternoon. From the left edge to about 28%: the clinic, a big round bell tent of pale canvas with a green leaf over its open door, at about 17%. From about 35% to 55%: a stretch of the city wall close behind, dark cypresses at about 38% and 52%, patched tents at about 41% and 53%. From about 65% to the right edge: patched tents at about 72%, 81% and 90%, pink blossom trees at about 76% and 85%, pink and blue wildflowers at about 71% and 92%, the meadow sloping down to the river at the right edge. Smoke from cooking fires. Sky: hazy afternoon blue going warm.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from sky to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

#### `thornbridge/river-edge` · The River's Edge · #31 · P0

- **Size:** 1600 × 900, 1 panel (canvas x 0–1600). Street. Late afternoon.
- **Doors:** none. **Ways out:** west to The Tents.
- **Used in:** A1-C3-02 (the Hollowed raid at the river).
- **Basis:** `12-rooms.md` §6 (the bridge itself and the river below); `paint_rooms_wild.py` (`river_edge`); the zone file (the painter's stone bridge, blossom tree, rocks and oaks).
- **Derived:** the far bank's trees.
- **The game's own pieces:** The game stands its own trees, bushes, flowers and mushrooms on the grass.

##### `thornbridge/river-edge` · panel 1 of 1

- **Attach:** `shots/art/room-templates/thornbridge-river-edge-1.png`.
- **Send to Claude as:** `thornbridge-river-edge-1.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. Thornbridge: The River's Edge. This one picture is the whole room.

LAYOUT, as the attached guide shows it. The floor line, where every wall, doorstep, tree and rock meets the ground, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even meadow grass with a dirt road, reeds at the very bottom. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person. The far horizon is a straight, level line about halfway down the picture (y 512 of 1024), at the eye height of someone standing on the floor; the distance (sea, hills, the far city) sits on it, and above the roofs and trees is sky.

DOORS. There is no door box in this panel, so nothing here should look like a door you could use, apart from the ways out named below; any building's own door stays small, plain and shut.

WAYS OUT. The left edge is a way out, to The Tents: let the scene and the floor run on off the left edge. The room's right end is closed: end the scene at the right with a building, a wall, rocks or trees, as the scene says.

SCENE, left to right. Thornbridge's river edge in the late afternoon. From the left edge to about 35%: the old bridge itself, the thorn bridge the place is named for, three round arches of mossy grey stone striding across the river behind the bank, centred at about 18%, ivy and thorny briars on its parapet. Behind the whole room runs the river, broad and slow and sparkling, across the back of the scene under a far bank of green meadow and soft trees. Grey rocks at the water's edge at about 38%. A pink blossom tree at about 45% leaning over the water. A big oak at about 74% by the water. Reeds along the near bank. The room is closed on the right by trees. Sky: late afternoon blue warming to gold and pink at the horizon, the sun low at the upper left. Mood: lovely, quiet, then dangerous.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from sky to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```


### 3.17 `old-crown-vault` · The Old Crown Vault

The royal vault under the Sunspire: cream stone and gold, broken into by sappers. Underground. Tiles: `vault`.

#### `old-crown-vault/entrance` · The Antechamber · #32 · P0

- **Size:** 3200 × 900, 3 panels (canvas x 0–1600, 1400–3000, 1600–3200). Street, enclosed (underground). Underground, daylight down the stair; dark (ambient 0.12).
- **Doors:** stair-up *Up to the Terrace* at x 1500; gate *The Vault Doors* at x 2160; gate *The Treasury Doors* at x 2900. **Ways out:** none.
- **Used in:** A1-C4-02 (into the vault).
- **Basis:** `12-rooms.md` §6; `03b-world-art.md` §3 (vault doors, relic shelves); `paint_rooms_under.py` (`vault_entrance`) and its preview; the zone file (the stair up to the terrace, the vault doors, the treasury doors, the sappers' hole).
- **Derived:** the bays' relic shelves and roundels.
- **The game's own pieces:** The game stands its own braziers, banners, the sappers' hole and rubble, a gilded lever and a strongbox on the floor, and hangs a sconce.

##### `old-crown-vault/entrance` · panel 1 of 3

- **Attach:** `shots/art/room-templates/old-crown-vault-entrance-1.png`.
- **Send to Claude as:** `old-crown-vault-entrance-1.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Old Crown Vault: The Antechamber. This is panel 1 of 3, the left part of a room that scrolls sideways; the scene carries on past its right edge.

LAYOUT, as the attached guide shows it. This place is enclosed, with no open sky: the back wall fills the picture from the top down to the floor line. The floor line, where the back wall and everything against it meets the floor, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even pale polished paving with gold joints. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person.

DOORS. Draw each of these exactly inside its labelled box on the guide, filling the box, its threshold on the floor line (the names are only for you: write nothing): the foot of a stair going up (Up to the Terrace), from about 89% to 99% across, its top about 53% down. Any other door in this panel stays small, plain and shut, so these are clearly the ones to use.

WAYS OUT. The room's left end is closed: end it at the left with solid wall or rock, as the scene says.

SCENE, left to right. The Old Crown Vault's antechamber, underground beneath the Sunspire, no sky. Cream stone dressed with gold: square piers at about 22%, 55% and 82% with gold bands, round arches between them under a coffered ceiling, gold lines along the cornice. From the left edge to about 20%: the west wall broken through, a ragged hole in the dressed stone into raw brown earth and the dark mouth of a dug tunnel, timber shoring, fallen blocks. From about 25% to 52%: a bay with dark wooden relic shelves on the wall holding gold goblets, little chests, crowns, orbs and old swords. From about 58% to 80%: a bay with a big carved roundel of the Aurelian sun in gold. From about 89% to the right edge: the foot of a stair going up: its lowest steps fill the box and climb away up through a tall arched opening, bright daylight falling down them from above. Mood: hushed, rich, and violated.

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from the top of the walls to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

##### `old-crown-vault/entrance` · panel 2 of 3

- **Attach:** `shots/art/room-templates/old-crown-vault-entrance-2.png`, and also attach your panel 1 picture (`old-crown-vault-entrance-1.png`).
- **Send to Claude as:** `old-crown-vault-entrance-2.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Old Crown Vault: The Antechamber. This is panel 2 of 3, the middle part of a room that scrolls sideways; the scene carries on past its left and right edges.

CONTINUATION. The second attached picture is panel 1 of this room. This panel carries the scene on to the right of it: the left 12% of this picture (about 192 px) shows exactly the same stretch as the right 12% of panel 1, so redraw that stretch to match it (the same buildings, trees, sky, clouds, colours, horizon, floor line, scale and light) and carry on seamlessly from there. Only the right 1344 px are new.

LAYOUT, as the attached guide shows it. This place is enclosed, with no open sky: the back wall fills the picture from the top down to the floor line. The floor line, where the back wall and everything against it meets the floor, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even pale polished paving with gold joints. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person.

DOORS. Draw each of these exactly inside its labelled box on the guide, filling the box, its threshold on the floor line (the names are only for you: write nothing): the foot of a stair going up (Up to the Terrace), from about 1% to 11% across, its top about 53% down; a great door or gateway (The Vault Doors), from about 38% to 57% across, its top about 36% down; a great door or gateway (The Treasury Doors), from about 84% to 100% across, its top about 36% down (its box runs off the right edge, so only its left part shows here). Any other door in this panel stays small, plain and shut, so these are clearly the ones to use.

SCENE, left to right. The middle of the Old Crown Vault's antechamber, underground. Cream stone dressed with gold, square piers with gold bands, round arches under a coffered ceiling. From the left edge to about 11%: the foot of the stair going up, its lowest steps in the box, daylight falling down it. From about 14% to 35%: a bay with a gold sun roundel. From about 38% to 57%: the great vault doors to the long hall, huge double doors of gilded bronze in a round arch, a gold sun spread across both leaves, filling their box. From about 60% to 80%: a bay with a relief of kneeling figures holding up a sun, in pale stone. From about 84% to the right edge: the treasury doors begin (their box runs off the right edge), double doors banded in gold. Mood: hushed, grand.

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from the top of the walls to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

##### `old-crown-vault/entrance` · panel 3 of 3

- **Attach:** `shots/art/room-templates/old-crown-vault-entrance-3.png`, and also attach your panel 2 picture (`old-crown-vault-entrance-2.png`).
- **Send to Claude as:** `old-crown-vault-entrance-3.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Old Crown Vault: The Antechamber. This is panel 3 of 3, the right part of a room that scrolls sideways; the scene carries on past its left edge.

CONTINUATION. The second attached picture is panel 2 of this room. This panel carries the scene on to the right of it: the left 88% of this picture (about 1344 px) shows exactly the same stretch as the right 88% of panel 2, so redraw that stretch to match it (the same buildings, trees, sky, clouds, colours, horizon, floor line, scale and light) and carry on seamlessly from there. Only the right 192 px are new.

LAYOUT, as the attached guide shows it. This place is enclosed, with no open sky: the back wall fills the picture from the top down to the floor line. The floor line, where the back wall and everything against it meets the floor, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even pale polished paving with gold joints. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person.

DOORS. Draw each of these exactly inside its labelled box on the guide, filling the box, its threshold on the floor line (the names are only for you: write nothing): a great door or gateway (The Vault Doors), from about 26% to 44% across, its top about 36% down; a great door or gateway (The Treasury Doors), from about 72% to 91% across, its top about 36% down. Any other door in this panel stays small, plain and shut, so these are clearly the ones to use.

WAYS OUT. The room's right end is closed: end it at the right with solid wall or rock, as the scene says.

SCENE, left to right. The east end of the Old Crown Vault's antechamber, underground. Cream stone dressed with gold, square piers with gold bands, round arches under a coffered ceiling. From the left edge to about 22%: a bay with a gold sun roundel. From about 26% to 44%: the great vault doors, gilded bronze double doors in a round arch with a gold sun across both leaves, filling their box. From about 48% to 68%: a bay with a relief of kneeling figures holding up a sun. From about 72% to 91%: the treasury doors, heavy double doors banded in gold with a relief of a crown over heaped coins, filling their box. From about 93% to the right edge: the end wall, a shallow niche with a gold urn carved in it. Mood: hushed, grand, sealed.

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from the top of the walls to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

#### `old-crown-vault/halls` · The Long Hall · #33 · P0

- **Size:** 3200 × 900, 3 panels (canvas x 0–1600, 1400–3000, 1600–3200). Street, enclosed (underground). Underground; dark (ambient 0.12).
- **Doors:** gate *The Crown Door* at x 1640; gate *The Vault Doors* at x 2480. **Ways out:** none.
- **Used in:** A1-C4-02 (the traps, the Crown Door).
- **Basis:** `paint_rooms_under.py` (`vault_halls`); `03b-world-art.md` §3 (dart traps, pressure plates); the zone file (the Crown Door, the vault doors, the dart traps on the wall, the sappers' earth at the west end).
- **Derived:** the bays' relics.
- **The game's own pieces:** The game hangs its own dart traps and sconces on the wall, stands an urn and rubble on the floor, and draws the pressure plates in the floor.

##### `old-crown-vault/halls` · panel 1 of 3

- **Attach:** `shots/art/room-templates/old-crown-vault-halls-1.png`.
- **Send to Claude as:** `old-crown-vault-halls-1.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Old Crown Vault: The Long Hall. This is panel 1 of 3, the left part of a room that scrolls sideways; the scene carries on past its right edge.

LAYOUT, as the attached guide shows it. This place is enclosed, with no open sky: the back wall fills the picture from the top down to the floor line. The floor line, where the back wall and everything against it meets the floor, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even pale polished paving with gold joints. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person.

DOORS. Draw each of these exactly inside its labelled box on the guide, filling the box, its threshold on the floor line (the names are only for you: write nothing): a great door or gateway (The Crown Door), from about 93% to 100% across, its top about 36% down (its box runs off the right edge, so only its left part shows here). Any other door in this panel stays small, plain and shut, so these are clearly the ones to use.

WAYS OUT. The room's left end is closed: end it at the left with solid wall or rock, as the scene says.

SCENE, left to right. The Old Crown Vault's long hall, underground, no sky: an arcade of gilded bays going on and on. Cream stone dressed with gold, square piers with gold bands every eighth of the way or so, round arches between them, navy-and-gold banners and gold sun roundels high in the bays, relics on shelves (goblets, crowns, little chests). From the left edge to about 15%: a bay where the ceiling has broken and a bank of brown earth has come down, the sappers' tunnel. Keep the wall plain at about 38%, 48% and 74%: the game hangs its dart traps there. At the right edge: the left part of the Crown Door (its box runs off the right edge), gold. Mood: rich, still, dangerous.

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from the top of the walls to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

##### `old-crown-vault/halls` · panel 2 of 3

- **Attach:** `shots/art/room-templates/old-crown-vault-halls-2.png`, and also attach your panel 1 picture (`old-crown-vault-halls-1.png`).
- **Send to Claude as:** `old-crown-vault-halls-2.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Old Crown Vault: The Long Hall. This is panel 2 of 3, the middle part of a room that scrolls sideways; the scene carries on past its left and right edges.

CONTINUATION. The second attached picture is panel 1 of this room. This panel carries the scene on to the right of it: the left 12% of this picture (about 192 px) shows exactly the same stretch as the right 12% of panel 1, so redraw that stretch to match it (the same buildings, trees, sky, clouds, colours, horizon, floor line, scale and light) and carry on seamlessly from there. Only the right 1344 px are new.

LAYOUT, as the attached guide shows it. This place is enclosed, with no open sky: the back wall fills the picture from the top down to the floor line. The floor line, where the back wall and everything against it meets the floor, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even pale polished paving with gold joints. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person.

DOORS. Draw each of these exactly inside its labelled box on the guide, filling the box, its threshold on the floor line (the names are only for you: write nothing): a great door or gateway (The Crown Door), from about 6% to 24% across, its top about 36% down; a great door or gateway (The Vault Doors), from about 58% to 77% across, its top about 36% down. Any other door in this panel stays small, plain and shut, so these are clearly the ones to use.

SCENE, left to right. The middle of the Old Crown Vault's long hall. Cream stone dressed with gold, square piers with gold bands, round arches, banners and sun roundels high in the bays. From about 6% to 24%: the Crown Door, tall double doors of gold with the Old Crown in relief across both leaves, set in a deep arch, filling its box. From about 28% to 55%: gilded bays with relic shelves (keep the wall plain at about 44%: the game hangs a dart trap there). From about 58% to 77%: the vault doors back to the antechamber, gilded bronze double doors with a gold sun, filling their box. From about 80% to the right edge: more gilded bays (keep about 80% plain: the game hangs a dart trap there). Mood: rich, still, dangerous.

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from the top of the walls to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

##### `old-crown-vault/halls` · panel 3 of 3

- **Attach:** `shots/art/room-templates/old-crown-vault-halls-3.png`, and also attach your panel 2 picture (`old-crown-vault-halls-2.png`).
- **Send to Claude as:** `old-crown-vault-halls-3.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Old Crown Vault: The Long Hall. This is panel 3 of 3, the right part of a room that scrolls sideways; the scene carries on past its left edge.

CONTINUATION. The second attached picture is panel 2 of this room. This panel carries the scene on to the right of it: the left 88% of this picture (about 1344 px) shows exactly the same stretch as the right 88% of panel 2, so redraw that stretch to match it (the same buildings, trees, sky, clouds, colours, horizon, floor line, scale and light) and carry on seamlessly from there. Only the right 192 px are new.

LAYOUT, as the attached guide shows it. This place is enclosed, with no open sky: the back wall fills the picture from the top down to the floor line. The floor line, where the back wall and everything against it meets the floor, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even pale polished paving with gold joints. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person.

DOORS. Draw each of these exactly inside its labelled box on the guide, filling the box, its threshold on the floor line (the names are only for you: write nothing): a great door or gateway (The Crown Door), from about 0% to 12% across, its top about 36% down (its box runs off the left edge, so only its right part shows here); a great door or gateway (The Vault Doors), from about 46% to 64% across, its top about 36% down. Any other door in this panel stays small, plain and shut, so these are clearly the ones to use.

WAYS OUT. The room's right end is closed: end it at the right with solid wall or rock, as the scene says.

SCENE, left to right. The east end of the Old Crown Vault's long hall. From the left edge to about 12%: the right part of the Crown Door, gold, with the Old Crown in relief. From about 15% to 42%: gilded bays with relic shelves and sun roundels (keep the wall plain at about 31%: a dart trap). From about 46% to 64%: the vault doors back to the antechamber, gilded bronze double doors with a gold sun, filling their box (keep about 67%, just right of them, plain: a dart trap). From about 68% to the right edge: more bays, and the hall ends in a solid wall with a tall gold relief of the sun. Mood: rich, still, dangerous.

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from the top of the walls to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

#### `old-crown-vault/strongroom` · The Relic Gallery · #34 · P0

- **Size:** 3200 × 900, 3 panels (canvas x 0–1600, 1400–3000, 1600–3200). Street, enclosed (underground). Underground; dark (ambient 0.12).
- **Doors:** gate *The Crown Door* at x 600. **Ways out:** none.
- **Used in:** A1-C4-02 (Kael, the Sunheart's empty pedestal, the sappers).
- **Basis:** `paint_rooms_under.py` (`vault_strongroom`); the zone file (the Crown Door at x 600, the Sunheart's pedestal at x 1600 and the relic cases as the game's props).
- **Derived:** the apse's mosaic.
- **The game's own pieces:** The game stands its own relic shelves, relic cases, urns, braziers, the sun mosaic and the Sunheart's pedestal (about x 1600) on the floor, and hangs sconces.

##### `old-crown-vault/strongroom` · panel 1 of 3

- **Attach:** `shots/art/room-templates/old-crown-vault-strongroom-1.png`.
- **Send to Claude as:** `old-crown-vault-strongroom-1.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Old Crown Vault: The Relic Gallery. This is panel 1 of 3, the left part of a room that scrolls sideways; the scene carries on past its right edge.

LAYOUT, as the attached guide shows it. This place is enclosed, with no open sky: the back wall fills the picture from the top down to the floor line. The floor line, where the back wall and everything against it meets the floor, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even pale polished paving with gold inlay. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person.

DOORS. Draw each of these exactly inside its labelled box on the guide, filling the box, its threshold on the floor line (the names are only for you: write nothing): a great door or gateway (The Crown Door), from about 28% to 47% across, its top about 36% down. Any other door in this panel stays small, plain and shut, so these are clearly the ones to use.

WAYS OUT. The room's left end is closed: end it at the left with solid wall or rock, as the scene says.

SCENE, left to right. The Relic Gallery, the Old Crown Vault's strongroom, underground, no sky. Cream stone and gold, round-topped alcoves in the walls between slim gilded pilasters, each alcove full of treasures on stepped shelves: crowns, goblets, orbs, sceptres, old leather books. From the left edge to about 25%: alcoves. From about 28% to 47%: the Crown Door from this side, tall gold double doors with the Old Crown in relief, filling its box. From about 50% to 85%: more alcoves of relics. From about 85% to the right edge: the edge of a great apse begins, a tall half-dome recess in the wall, its back glowing with gold mosaic. Mood: a treasure house, hushed and golden.

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from the top of the walls to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

##### `old-crown-vault/strongroom` · panel 2 of 3

- **Attach:** `shots/art/room-templates/old-crown-vault-strongroom-2.png`, and also attach your panel 1 picture (`old-crown-vault-strongroom-1.png`).
- **Send to Claude as:** `old-crown-vault-strongroom-2.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Old Crown Vault: The Relic Gallery. This is panel 2 of 3, the middle part of a room that scrolls sideways; the scene carries on past its left and right edges.

CONTINUATION. The second attached picture is panel 1 of this room. This panel carries the scene on to the right of it: the left 12% of this picture (about 192 px) shows exactly the same stretch as the right 12% of panel 1, so redraw that stretch to match it (the same buildings, trees, sky, clouds, colours, horizon, floor line, scale and light) and carry on seamlessly from there. Only the right 1344 px are new.

LAYOUT, as the attached guide shows it. This place is enclosed, with no open sky: the back wall fills the picture from the top down to the floor line. The floor line, where the back wall and everything against it meets the floor, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even pale polished paving with gold inlay. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person.

DOORS. There is no door box in this panel, so nothing here should look like a door you could use, apart from the ways out named below; any building's own door stays small, plain and shut.

SCENE, left to right. The heart of the Relic Gallery, underground. From the left edge to about 30%: a great apse, a tall half-dome recess with slim pillars either side, its back wall a huge gold Aurelian sun in mosaic with long rays spreading across the dome, glowing (the game stands the Sunheart's pedestal in front of its centre, at about 12%: keep the wall behind it clear). From about 30% to the right edge: walls of round-topped alcoves between gilded pilasters, full of crowns, goblets, orbs and books. Mood: the most precious place in the kingdom, and its centre is empty.

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from the top of the walls to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

##### `old-crown-vault/strongroom` · panel 3 of 3

- **Attach:** `shots/art/room-templates/old-crown-vault-strongroom-3.png`, and also attach your panel 2 picture (`old-crown-vault-strongroom-2.png`).
- **Send to Claude as:** `old-crown-vault-strongroom-3.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Old Crown Vault: The Relic Gallery. This is panel 3 of 3, the right part of a room that scrolls sideways; the scene carries on past its left edge.

CONTINUATION. The second attached picture is panel 2 of this room. This panel carries the scene on to the right of it: the left 88% of this picture (about 1344 px) shows exactly the same stretch as the right 88% of panel 2, so redraw that stretch to match it (the same buildings, trees, sky, clouds, colours, horizon, floor line, scale and light) and carry on seamlessly from there. Only the right 192 px are new.

LAYOUT, as the attached guide shows it. This place is enclosed, with no open sky: the back wall fills the picture from the top down to the floor line. The floor line, where the back wall and everything against it meets the floor, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even pale polished paving with gold inlay. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person.

DOORS. There is no door box in this panel, so nothing here should look like a door you could use, apart from the ways out named below; any building's own door stays small, plain and shut.

WAYS OUT. The room's right end is closed: end it at the right with solid wall or rock, as the scene says.

SCENE, left to right. The east end of the Relic Gallery, underground. At the very left edge: the right half of the great apse with its gold sun mosaic (the pedestal stands just off the left). From about 10% to 95%: walls of round-topped alcoves between gilded pilasters, full of treasures. From about 95% to the right edge: the end wall, plain gold-trimmed stone. The room is closed on the right. Mood: hushed and golden.

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from the top of the walls to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

#### `old-crown-vault/treasury` · The Treasury · #35 · P0

- **Size:** 1600 × 900, 1 panel (canvas x 0–1600). Street, enclosed (underground). Underground; dark (ambient 0.12).
- **Doors:** gate *The Treasury Doors* at x 260. **Ways out:** none.
- **Used in:** A1-C4-02 (the treasury's strongboxes).
- **Basis:** `paint_rooms_under.py` (`vault_treasury`); the zone file (the treasury doors, the gold piles and strongboxes as the game's props).
- **Derived:** the shelves of plate.
- **The game's own pieces:** The game stands its own gold piles, braziers, urns, a relic shelf and two strongboxes on the floor, and hangs a sconce.

##### `old-crown-vault/treasury` · panel 1 of 1

- **Attach:** `shots/art/room-templates/old-crown-vault-treasury-1.png`.
- **Send to Claude as:** `old-crown-vault-treasury-1.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Old Crown Vault: The Treasury. This one picture is the whole room.

LAYOUT, as the attached guide shows it. This place is enclosed, with no open sky: the back wall fills the picture from the top down to the floor line. The floor line, where the back wall and everything against it meets the floor, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even pale polished paving with gold joints. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person.

DOORS. Draw each of these exactly inside its labelled box on the guide, filling the box, its threshold on the floor line (the names are only for you: write nothing): a great door or gateway (The Treasury Doors), from about 7% to 26% across, its top about 36% down. Any other door in this panel stays small, plain and shut, so these are clearly the ones to use.

WAYS OUT. The room's left end is closed: end it at the left with solid wall or rock, as the scene says. The room's right end is closed: end it at the right with solid wall or rock, as the scene says.

SCENE, left to right. The Old Crown's treasury, a small gilded chamber underground, no sky. From about 7% to 26%: the treasury doors back to the antechamber, heavy double doors banded in gold with a crown over heaped coins in relief, filling their box. From about 28% to the right edge: deep round-topped alcoves in cream-and-gold walls, heaped with gold coins spilling over their lips, open chests overflowing, crowns and goblets, shelves of gold and silver plate, a gold sun on the back wall at about 65%. The room is closed at both ends. Mood: dazzling, overflowing, a little greedy.

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from the top of the walls to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```


### 3.18 `sunspire-keep` · Sunspire Keep

The royal keep's great hall, where the King holds court. Tiles: `keep`.

#### `sunspire-keep/great-hall` · The Great Hall · #36 · P0

- **Size:** 3200 × 900, 3 panels (canvas x 0–1600, 1400–3000, 1600–3200). Interior. Day, bright windows.
- **Doors:** door *Out to the Terrace* at x 3000. **Ways out:** none.
- **Used in:** A1-C4-03 (the court), A1-C4-04 (after court).
- **Basis:** `12-rooms.md` §6 (the throne, banners, the gallery); `03b-world-art.md` §3 (the sun throne, banners, marble pillars); `paint_rooms_interiors.py` (`great_hall`); the zone file (the throne at x 520, the pillars, the arch, the door out to the terrace).
- **Derived:** the windows' places between the pillars.
- **The game's own pieces:** The game stands its own sun throne (about x 520), marble pillars, guard statues, braziers, a great arch, a war table, a desk and chairs on the floor, and hangs the sun banners and tapestries.

##### `sunspire-keep/great-hall` · panel 1 of 3

- **Attach:** `shots/art/room-templates/sunspire-keep-great-hall-1.png`.
- **Send to Claude as:** `sunspire-keep-great-hall-1.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. Sunspire Keep: The Great Hall. This is panel 1 of 3, the left part of a room that scrolls sideways; the scene carries on past its right edge.

LAYOUT, as the attached guide shows it. This is an interior, with no sky: the back wall, with the ceiling's beams at the top, fills the picture down to the floor line. The floor line, where the back wall and everything against it meets the floor, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even polished cream marble. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person.

DOORS. There is no door box in this panel, so nothing here should look like a door you could use, apart from the ways out named below.

WAYS OUT. The room's left end is closed: end it at the left with its end wall.

SCENE, left to right. The great hall of Sunspire Keep, the throne room, on a bright day. A long hall of cream ashlar stone over a blue-grey dado with a gold rail, fluted pilasters with gold capitals at about 49% and 88% (where the game stands marble pillars), round arches springing between them under a coffered ceiling with gold bosses. From the left edge to about 10%: the hall's west end wall. From about 18% to 46%: behind where the throne stands, a great cloth of estate hanging from high on the wall, deep navy with a gold fringe and a huge gold sun embroidered on it, and above it a round window of stained glass with a gold sun at its centre. Tall arched windows of stained glass in blue, gold and rose at about 62% and 75%, their light falling across the hall. Gold chandeliers hang down the hall. Mood: royal, bright, splendid.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from ceiling to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

##### `sunspire-keep/great-hall` · panel 2 of 3

- **Attach:** `shots/art/room-templates/sunspire-keep-great-hall-2.png`, and also attach your panel 1 picture (`sunspire-keep-great-hall-1.png`).
- **Send to Claude as:** `sunspire-keep-great-hall-2.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. Sunspire Keep: The Great Hall. This is panel 2 of 3, the middle part of a room that scrolls sideways; the scene carries on past its left and right edges.

CONTINUATION. The second attached picture is panel 1 of this room. This panel carries the scene on to the right of it: the left 12% of this picture (about 192 px) shows exactly the same stretch as the right 12% of panel 1, so redraw that stretch to match it (the same buildings, trees, sky, clouds, colours, horizon, floor line, scale and light) and carry on seamlessly from there. Only the right 1344 px are new.

LAYOUT, as the attached guide shows it. This is an interior, with no sky: the back wall, with the ceiling's beams at the top, fills the picture down to the floor line. The floor line, where the back wall and everything against it meets the floor, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even polished cream marble. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person.

DOORS. Draw each of these exactly inside its labelled box on the guide, filling the box, its threshold on the floor line (the names are only for you: write nothing): a door (Out to the Terrace), from about 96% to 100% across, its top about 45% down (its box runs off the right edge, so only its left part shows here). Any other door in this panel stays small, plain and shut, so these are clearly the ones to use.

SCENE, left to right. The middle of Sunspire Keep's great hall on a bright day. Cream ashlar over a blue-grey dado with a gold rail, fluted gold-capped pilasters at about 0% and 57%, round arches under a coffered ceiling, gold chandeliers. From about 15% to 45%: a gallery, a stone balcony with a carved balustrade running across the upper wall over the hall, centred at about 32% (the game stands a great arch under it). Tall arched stained-glass windows in blue, gold and rose at about 70% and 85%. At the right edge: the start of the door out to the terrace (its box runs off the right edge). Mood: royal and busy.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from ceiling to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```

##### `sunspire-keep/great-hall` · panel 3 of 3

- **Attach:** `shots/art/room-templates/sunspire-keep-great-hall-3.png`, and also attach your panel 2 picture (`sunspire-keep-great-hall-2.png`).
- **Send to Claude as:** `sunspire-keep-great-hall-3.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. Sunspire Keep: The Great Hall. This is panel 3 of 3, the right part of a room that scrolls sideways; the scene carries on past its left edge.

CONTINUATION. The second attached picture is panel 2 of this room. This panel carries the scene on to the right of it: the left 88% of this picture (about 1344 px) shows exactly the same stretch as the right 88% of panel 2, so redraw that stretch to match it (the same buildings, trees, sky, clouds, colours, horizon, floor line, scale and light) and carry on seamlessly from there. Only the right 192 px are new.

LAYOUT, as the attached guide shows it. This is an interior, with no sky: the back wall, with the ceiling's beams at the top, fills the picture down to the floor line. The floor line, where the back wall and everything against it meets the floor, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even polished cream marble. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person.

DOORS. Draw each of these exactly inside its labelled box on the guide, filling the box, its threshold on the floor line (the names are only for you: write nothing): a door (Out to the Terrace), from about 84% to 91% across, its top about 45% down. Any other door in this panel stays small, plain and shut, so these are clearly the ones to use.

WAYS OUT. The room's right end is closed: end it at the right with its end wall.

SCENE, left to right. The east end of Sunspire Keep's great hall on a bright day. Cream ashlar over a blue-grey dado with a gold rail, fluted gold-capped pilasters at about 45% and 83%, round arches under a coffered ceiling, gold chandeliers. From about 5% to 30%: the gallery, a stone balcony with a carved balustrade across the upper wall. Tall arched stained-glass windows at about 58% and 70%. From about 84% to 91%: the door out to the terrace, a tall arched double door of oak with gold fittings, sunlight showing round its edges, filling its box. From about 94% to the right edge: the hall's east end wall. Mood: royal, bright.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from ceiling to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```


### 3.19 `rusty-anvil` · The Rusty Anvil

Brannock's bar on the harbourside: low, dark, rough. Tiles: `interior`.

#### `rusty-anvil/bar` · The Bar · #37 · P0

- **Size:** 1600 × 900, 1 panel (canvas x 0–1600). Interior. Night, one candle; dark (ambient 0.45); tint 214 186 164.
- **Doors:** none. **Ways out:** west back out.
- **Used in:** A1-C4-05 (Brannock).
- **Basis:** `03b-world-art.md` §1 (a dim dockside bar, Brannock's corner table); `paint_rooms_interiors.py` (`anvil_bar`); the zone file (the bar counter, Brannock's table at x 1378, the warm tint).
- **Derived:** the junk's places on the walls.
- **The game's own pieces:** The game stands its own bar counter, kegs, stools, tables, barrels and a candle stand on the floor, hangs a sconce and a lantern, and tints the room warm amber.

##### `rusty-anvil/bar` · panel 1 of 1

- **Attach:** `shots/art/room-templates/rusty-anvil-bar-1.png`.
- **Send to Claude as:** `rusty-anvil-bar-1.png`

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style. Crisp, confident outlines in deep brown to near-black, never pure black, on every building, prop, rock and plant: heavier round outer shapes, finer inside them. Flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. Charming, stylised architecture that is still well built: timber frames, dressed stone courses, scalloped roof tiles, window boxes, ironwork, a little moss and wear. Lush, rounded foliage in clumped, scalloped masses. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal, the strongest colour saved for doors, signs, magic, gold and lit windows. The distance is paler, cooler, hazier and simpler. Light always comes from the upper left: lit faces on the left of every form, shadows falling to the right. The camera is side-on at a standing person's eye level, looking straight at the scene: building fronts face us flat, the horizon is straight and level, and nothing vanishes into the distance. It sits behind characters drawn with bold, clean outlines and cel shading, so it shares their line weight and colour feel but is a little calmer, with its detail gathered round doors, signs and windows. The bottom of the scene is the floor the characters walk on: open, flat, plain ground with nothing standing on it. The first attached picture is a layout guide: follow its floor line, its horizon or back-wall line and its labelled boxes exactly, and draw each door, gate or stair inside its box with its threshold on the floor line; but it is only a plan, so don't copy its flat colours, hatching, lines, labels or numbers. No people, animals or creatures, and no text, letters or numbers anywhere: signs carry pictures only. No interface, frame, border or watermark. The picture is 1536 by 1024; its top and bottom 80 px will be cut off, so carry the sky or ceiling up through the top and the ground down through the bottom, and keep everything that matters inside the middle 864 px.

ROOM. The Rusty Anvil: The Bar. This one picture is the whole room.

LAYOUT, as the attached guide shows it. This is an interior, with no sky: the back wall, with the ceiling's beams at the top, fills the picture down to the floor line. The floor line, where the back wall and everything against it meets the floor, is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even dark floorboards and flagstones. A person standing on the floor would be about 185 px tall, so a house door is a little taller than a person.

DOORS. There is no door box in this panel, so nothing here should look like a door you could use, apart from the ways out named below.

WAYS OUT. The left edge is the way back out: let the scene and the floor run on off the left edge. The room's right end is closed: end it at the right with its end wall.

SCENE, left to right. Inside the Rusty Anvil, a low, dim dockside bar, at night. A tall wainscot of black tarred boards under stained, smoky plaster, heavy low beams across the ceiling resting on thick posts at about 20%, 55% and 90%. From the left edge to about 45%: the wall behind the bar: one long shelf of dusty bottles, and above it at about 30% a crooked wooden board with a rusty iron anvil painted on it (no letters). From about 48% to 60%: a small grimy window on the night, dark blue with a blur of moon. From about 60% to 78%: sailors' junk on the wall, a ship's wheel, an old anchor, a fishing net draped from nails. From about 80% to the right edge: the corner, a big prize fish mounted on a board, and papers pinned to the wall (blank, or with scribble marks only). The left edge is the way back out to the harbour. Mood: rough, smoky, quiet, a place where people go to not be found.

LIGHT. The game darkens this room itself and adds its own glow to the lamps, candles, fires and fungus, so paint it at clear, readable mid values: every shape and colour easy to read, no big areas of black, no heavy shadows of your own. Lamps, windows and glowing things may glow softly. The game also washes the whole room in a warm, brownish amber, so keep the colours clean rather than pushing them that way.

OUTPUT. One landscape PNG, 1536 by 1024: the whole scene from ceiling to floor, edge to edge, painted right through the top and bottom 80 px of bleed.

AVOID. People, faces, silhouettes of people, animals, birds, creatures. Text, letters, numbers, runes that read as writing, labels, signatures, logos, watermarks. Interface elements, frames, borders, vignettes. Perspective: vanishing points, a road or floor running away into the distance, a view from above or below, a tilted or curved horizon, fisheye. Anything on the open floor band: props, crates, barrels, puddles, rugs, paths, cast shadows, clutter. Doors, gates or arches moved, resized or added outside the guide's boxes. Pixel art, painterly blur, airbrushed or glossy 3D-render shading, photorealism, film grain, lens flare. Pure-black or uneven outlines. Neon or candy colours, muddy greys, the whole scene in one hue. Dense noise or tiny texture everywhere. The layout guide's flat colours, hatching, lines or labels. Anything copied from an existing game, film, comic or brand.
```


---

## 4. Foregrounds (optional)

A foreground strip is the few things that stand nearest the camera and are drawn over everyone: lamp posts, tree trunks, planters, bollards. Only the streets whose zone files place them have one. Make a room's background panels first, in the same chat, so the style matches; attach the same layout guide as the panel; and send the PNG to Claude with the name given. A room is fine without them.

### `docks/harbourside` · The Harbourside

#### `docks/harbourside` · foreground for panel 1 of 3

- **Attach:** `shots/art/room-templates/docks-harbourside-1.png`, and also your background panel 1 picture (`docks-harbourside-1.png`), so the strip matches it.
- **Send to Claude as:** `docks-harbourside-fg-1.png`

```text
STYLE. An original foreground overlay for a side-on 2D fantasy game scene, in the same clean, outlined, vector-like style as the scene behind it: crisp outlines in deep brown to near-black, never pure black, flat fills with two or three cel tones and hard-edged shadow shapes, light from the upper left; but darker and a little more saturated than the scene, because these things stand nearest the camera. Draw only the objects listed below, each standing up from the very bottom edge of the picture at the place given (a share of the picture's width, left to right), cut off by the bottom edge as if the camera stood just behind them. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture at all. Tall things (a lamp post, a trunk) are slim, and together the objects cover no more than an eighth of the bottom third of the picture. Side-on at eye level, no perspective. The first attached picture is the layout guide and the second is the background this strip stands in front of: match its style and light, but don't copy the background itself, or the guide's colours, lines or labels. No people, animals, text, letters, numbers, frame or watermark. One 1536 by 1024 PNG.

ROOM. Saltwind Docks: The Harbourside. The foreground for panel 1 of 3.

DRAW ONLY THESE, each standing up from the bottom edge: a squat black iron mooring bollard with a loop of rope thrown over it, no taller than a sixth of the picture, at about 34% across.

AVOID. A background, sky, scenery, ground or floor behind the objects. Any shading, gradient, glow or halo spilling onto the empty area. Objects floating above the bottom edge. Wide bushes or hedges across the bottom. Extra objects not in the list. Perspective. People, animals, text, letters, logos, watermarks. Pixel art, painterly blur, glossy 3D-render shading. Pure-black outlines.
```

#### `docks/harbourside` · foreground for panel 2 of 3

- **Attach:** `shots/art/room-templates/docks-harbourside-2.png`, and also your background panel 2 picture (`docks-harbourside-2.png`), so the strip matches it.
- **Send to Claude as:** `docks-harbourside-fg-2.png`

```text
STYLE. An original foreground overlay for a side-on 2D fantasy game scene, in the same clean, outlined, vector-like style as the scene behind it: crisp outlines in deep brown to near-black, never pure black, flat fills with two or three cel tones and hard-edged shadow shapes, light from the upper left; but darker and a little more saturated than the scene, because these things stand nearest the camera. Draw only the objects listed below, each standing up from the very bottom edge of the picture at the place given (a share of the picture's width, left to right), cut off by the bottom edge as if the camera stood just behind them. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture at all. Tall things (a lamp post, a trunk) are slim, and together the objects cover no more than an eighth of the bottom third of the picture. Side-on at eye level, no perspective. The first attached picture is the layout guide and the second is the background this strip stands in front of: match its style and light, but don't copy the background itself, or the guide's colours, lines or labels. No people, animals, text, letters, numbers, frame or watermark. One 1536 by 1024 PNG.

ROOM. Saltwind Docks: The Harbourside. The foreground for panel 2 of 3.

DRAW ONLY THESE, each standing up from the bottom edge: a squat black iron mooring bollard with a loop of rope thrown over it, no taller than a sixth of the picture, at about 38% across.

AVOID. A background, sky, scenery, ground or floor behind the objects. Any shading, gradient, glow or halo spilling onto the empty area. Objects floating above the bottom edge. Wide bushes or hedges across the bottom. Extra objects not in the list. Perspective. People, animals, text, letters, logos, watermarks. Pixel art, painterly blur, glossy 3D-render shading. Pure-black outlines.
```

#### `docks/harbourside` · foreground for panel 3 of 3

- **Attach:** `shots/art/room-templates/docks-harbourside-3.png`, and also your background panel 3 picture (`docks-harbourside-3.png`), so the strip matches it.
- **Send to Claude as:** `docks-harbourside-fg-3.png`

```text
STYLE. An original foreground overlay for a side-on 2D fantasy game scene, in the same clean, outlined, vector-like style as the scene behind it: crisp outlines in deep brown to near-black, never pure black, flat fills with two or three cel tones and hard-edged shadow shapes, light from the upper left; but darker and a little more saturated than the scene, because these things stand nearest the camera. Draw only the objects listed below, each standing up from the very bottom edge of the picture at the place given (a share of the picture's width, left to right), cut off by the bottom edge as if the camera stood just behind them. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture at all. Tall things (a lamp post, a trunk) are slim, and together the objects cover no more than an eighth of the bottom third of the picture. Side-on at eye level, no perspective. The first attached picture is the layout guide and the second is the background this strip stands in front of: match its style and light, but don't copy the background itself, or the guide's colours, lines or labels. No people, animals, text, letters, numbers, frame or watermark. One 1536 by 1024 PNG.

ROOM. Saltwind Docks: The Harbourside. The foreground for panel 3 of 3.

DRAW ONLY THESE, each standing up from the bottom edge: a squat black iron mooring bollard with a loop of rope thrown over it, no taller than a sixth of the picture, at about 26% across.

AVOID. A background, sky, scenery, ground or floor behind the objects. Any shading, gradient, glow or halo spilling onto the empty area. Objects floating above the bottom edge. Wide bushes or hedges across the bottom. Extra objects not in the list. Perspective. People, animals, text, letters, logos, watermarks. Pixel art, painterly blur, glossy 3D-render shading. Pure-black outlines.
```

### `questeria/gate-plaza` · The Gate Plaza

#### `questeria/gate-plaza` · foreground for panel 1 of 3

- **Attach:** `shots/art/room-templates/questeria-gate-plaza-1.png`, and also your background panel 1 picture (`questeria-gate-plaza-1.png`), so the strip matches it.
- **Send to Claude as:** `questeria-gate-plaza-fg-1.png`

```text
STYLE. An original foreground overlay for a side-on 2D fantasy game scene, in the same clean, outlined, vector-like style as the scene behind it: crisp outlines in deep brown to near-black, never pure black, flat fills with two or three cel tones and hard-edged shadow shapes, light from the upper left; but darker and a little more saturated than the scene, because these things stand nearest the camera. Draw only the objects listed below, each standing up from the very bottom edge of the picture at the place given (a share of the picture's width, left to right), cut off by the bottom edge as if the camera stood just behind them. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture at all. Tall things (a lamp post, a trunk) are slim, and together the objects cover no more than an eighth of the bottom third of the picture. Side-on at eye level, no perspective. The first attached picture is the layout guide and the second is the background this strip stands in front of: match its style and light, but don't copy the background itself, or the guide's colours, lines or labels. No people, animals, text, letters, numbers, frame or watermark. One 1536 by 1024 PNG.

ROOM. Questeria: The Gate Plaza. The foreground for panel 1 of 3.

DRAW ONLY THESE, each standing up from the bottom edge: a low stone planter box overflowing with pink and white flowers, no taller than a sixth of the picture, at about 52% across.

AVOID. A background, sky, scenery, ground or floor behind the objects. Any shading, gradient, glow or halo spilling onto the empty area. Objects floating above the bottom edge. Wide bushes or hedges across the bottom. Extra objects not in the list. Perspective. People, animals, text, letters, logos, watermarks. Pixel art, painterly blur, glossy 3D-render shading. Pure-black outlines.
```

### `questeria/festival-square` · The Festival Square

#### `questeria/festival-square` · foreground for panel 1 of 3

- **Attach:** `shots/art/room-templates/questeria-festival-square-1.png`, and also your background panel 1 picture (`questeria-festival-square-1.png`), so the strip matches it.
- **Send to Claude as:** `questeria-festival-square-fg-1.png`

```text
STYLE. An original foreground overlay for a side-on 2D fantasy game scene, in the same clean, outlined, vector-like style as the scene behind it: crisp outlines in deep brown to near-black, never pure black, flat fills with two or three cel tones and hard-edged shadow shapes, light from the upper left; but darker and a little more saturated than the scene, because these things stand nearest the camera. Draw only the objects listed below, each standing up from the very bottom edge of the picture at the place given (a share of the picture's width, left to right), cut off by the bottom edge as if the camera stood just behind them. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture at all. Tall things (a lamp post, a trunk) are slim, and together the objects cover no more than an eighth of the bottom third of the picture. Side-on at eye level, no perspective. The first attached picture is the layout guide and the second is the background this strip stands in front of: match its style and light, but don't copy the background itself, or the guide's colours, lines or labels. No people, animals, text, letters, numbers, frame or watermark. One 1536 by 1024 PNG.

ROOM. Questeria: The Festival Square. The foreground for panel 1 of 3.

DRAW ONLY THESE, each standing up from the bottom edge: a slender black wrought-iron street lamp post, its glass lantern lit warm about two-thirds of the way up the picture, at about 7% across; a clipped round topiary bush in a stone urn, no taller than a quarter of the picture, at about 62% across.

AVOID. A background, sky, scenery, ground or floor behind the objects. Any shading, gradient, glow or halo spilling onto the empty area. Objects floating above the bottom edge. Wide bushes or hedges across the bottom. Extra objects not in the list. Perspective. People, animals, text, letters, logos, watermarks. Pixel art, painterly blur, glossy 3D-render shading. Pure-black outlines.
```

#### `questeria/festival-square` · foreground for panel 2 of 3

- **Attach:** `shots/art/room-templates/questeria-festival-square-2.png`, and also your background panel 2 picture (`questeria-festival-square-2.png`), so the strip matches it.
- **Send to Claude as:** `questeria-festival-square-fg-2.png`

```text
STYLE. An original foreground overlay for a side-on 2D fantasy game scene, in the same clean, outlined, vector-like style as the scene behind it: crisp outlines in deep brown to near-black, never pure black, flat fills with two or three cel tones and hard-edged shadow shapes, light from the upper left; but darker and a little more saturated than the scene, because these things stand nearest the camera. Draw only the objects listed below, each standing up from the very bottom edge of the picture at the place given (a share of the picture's width, left to right), cut off by the bottom edge as if the camera stood just behind them. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture at all. Tall things (a lamp post, a trunk) are slim, and together the objects cover no more than an eighth of the bottom third of the picture. Side-on at eye level, no perspective. The first attached picture is the layout guide and the second is the background this strip stands in front of: match its style and light, but don't copy the background itself, or the guide's colours, lines or labels. No people, animals, text, letters, numbers, frame or watermark. One 1536 by 1024 PNG.

ROOM. Questeria: The Festival Square. The foreground for panel 2 of 3.

DRAW ONLY THESE, each standing up from the bottom edge: a clipped round topiary bush in a stone urn, no taller than a quarter of the picture, at about 57% across.

AVOID. A background, sky, scenery, ground or floor behind the objects. Any shading, gradient, glow or halo spilling onto the empty area. Objects floating above the bottom edge. Wide bushes or hedges across the bottom. Extra objects not in the list. Perspective. People, animals, text, letters, logos, watermarks. Pixel art, painterly blur, glossy 3D-render shading. Pure-black outlines.
```

#### `questeria/festival-square` · foreground for panel 3 of 3

- **Attach:** `shots/art/room-templates/questeria-festival-square-3.png`, and also your background panel 3 picture (`questeria-festival-square-3.png`), so the strip matches it.
- **Send to Claude as:** `questeria-festival-square-fg-3.png`

```text
STYLE. An original foreground overlay for a side-on 2D fantasy game scene, in the same clean, outlined, vector-like style as the scene behind it: crisp outlines in deep brown to near-black, never pure black, flat fills with two or three cel tones and hard-edged shadow shapes, light from the upper left; but darker and a little more saturated than the scene, because these things stand nearest the camera. Draw only the objects listed below, each standing up from the very bottom edge of the picture at the place given (a share of the picture's width, left to right), cut off by the bottom edge as if the camera stood just behind them. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture at all. Tall things (a lamp post, a trunk) are slim, and together the objects cover no more than an eighth of the bottom third of the picture. Side-on at eye level, no perspective. The first attached picture is the layout guide and the second is the background this strip stands in front of: match its style and light, but don't copy the background itself, or the guide's colours, lines or labels. No people, animals, text, letters, numbers, frame or watermark. One 1536 by 1024 PNG.

ROOM. Questeria: The Festival Square. The foreground for panel 3 of 3.

DRAW ONLY THESE, each standing up from the bottom edge: a clipped round topiary bush in a stone urn, no taller than a quarter of the picture, at about 45% across.

AVOID. A background, sky, scenery, ground or floor behind the objects. Any shading, gradient, glow or halo spilling onto the empty area. Objects floating above the bottom edge. Wide bushes or hedges across the bottom. Extra objects not in the list. Perspective. People, animals, text, letters, logos, watermarks. Pixel art, painterly blur, glossy 3D-render shading. Pure-black outlines.
```

### `questeria/lantern-market` · The Lantern Market

#### `questeria/lantern-market` · foreground for panel 1 of 3

- **Attach:** `shots/art/room-templates/questeria-lantern-market-1.png`, and also your background panel 1 picture (`questeria-lantern-market-1.png`), so the strip matches it.
- **Send to Claude as:** `questeria-lantern-market-fg-1.png`

```text
STYLE. An original foreground overlay for a side-on 2D fantasy game scene, in the same clean, outlined, vector-like style as the scene behind it: crisp outlines in deep brown to near-black, never pure black, flat fills with two or three cel tones and hard-edged shadow shapes, light from the upper left; but darker and a little more saturated than the scene, because these things stand nearest the camera. Draw only the objects listed below, each standing up from the very bottom edge of the picture at the place given (a share of the picture's width, left to right), cut off by the bottom edge as if the camera stood just behind them. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture at all. Tall things (a lamp post, a trunk) are slim, and together the objects cover no more than an eighth of the bottom third of the picture. Side-on at eye level, no perspective. The first attached picture is the layout guide and the second is the background this strip stands in front of: match its style and light, but don't copy the background itself, or the guide's colours, lines or labels. No people, animals, text, letters, numbers, frame or watermark. One 1536 by 1024 PNG.

ROOM. Questeria: The Lantern Market. The foreground for panel 1 of 3.

DRAW ONLY THESE, each standing up from the bottom edge: a slender black wrought-iron street lamp post, its glass lantern lit warm about two-thirds of the way up the picture, at about 6% across; a low stone planter box overflowing with pink and white flowers, no taller than a sixth of the picture, at about 50% across; a slender black wrought-iron street lamp post, its glass lantern lit warm about two-thirds of the way up the picture, at about 83% across.

AVOID. A background, sky, scenery, ground or floor behind the objects. Any shading, gradient, glow or halo spilling onto the empty area. Objects floating above the bottom edge. Wide bushes or hedges across the bottom. Extra objects not in the list. Perspective. People, animals, text, letters, logos, watermarks. Pixel art, painterly blur, glossy 3D-render shading. Pure-black outlines.
```

### `questeria/thornbridge-gate` · The Thornbridge Gate

#### `questeria/thornbridge-gate` · foreground for panel 1 of 1

- **Attach:** `shots/art/room-templates/questeria-thornbridge-gate-1.png`, and also your background panel 1 picture (`questeria-thornbridge-gate-1.png`), so the strip matches it.
- **Send to Claude as:** `questeria-thornbridge-gate-fg-1.png`

```text
STYLE. An original foreground overlay for a side-on 2D fantasy game scene, in the same clean, outlined, vector-like style as the scene behind it: crisp outlines in deep brown to near-black, never pure black, flat fills with two or three cel tones and hard-edged shadow shapes, light from the upper left; but darker and a little more saturated than the scene, because these things stand nearest the camera. Draw only the objects listed below, each standing up from the very bottom edge of the picture at the place given (a share of the picture's width, left to right), cut off by the bottom edge as if the camera stood just behind them. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture at all. Tall things (a lamp post, a trunk) are slim, and together the objects cover no more than an eighth of the bottom third of the picture. Side-on at eye level, no perspective. The first attached picture is the layout guide and the second is the background this strip stands in front of: match its style and light, but don't copy the background itself, or the guide's colours, lines or labels. No people, animals, text, letters, numbers, frame or watermark. One 1536 by 1024 PNG.

ROOM. Questeria: The Thornbridge Gate. The foreground for the room.

DRAW ONLY THESE, each standing up from the bottom edge: a slender black wrought-iron street lamp post, its glass lantern lit warm about two-thirds of the way up the picture, at about 16% across.

AVOID. A background, sky, scenery, ground or floor behind the objects. Any shading, gradient, glow or halo spilling onto the empty area. Objects floating above the bottom edge. Wide bushes or hedges across the bottom. Extra objects not in the list. Perspective. People, animals, text, letters, logos, watermarks. Pixel art, painterly blur, glossy 3D-render shading. Pure-black outlines.
```

### `road/farmland` · The Farmland

#### `road/farmland` · foreground for panel 1 of 3

- **Attach:** `shots/art/room-templates/road-farmland-1.png`, and also your background panel 1 picture (`road-farmland-1.png`), so the strip matches it.
- **Send to Claude as:** `road-farmland-fg-1.png`

```text
STYLE. An original foreground overlay for a side-on 2D fantasy game scene, in the same clean, outlined, vector-like style as the scene behind it: crisp outlines in deep brown to near-black, never pure black, flat fills with two or three cel tones and hard-edged shadow shapes, light from the upper left; but darker and a little more saturated than the scene, because these things stand nearest the camera. Draw only the objects listed below, each standing up from the very bottom edge of the picture at the place given (a share of the picture's width, left to right), cut off by the bottom edge as if the camera stood just behind them. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture at all. Tall things (a lamp post, a trunk) are slim, and together the objects cover no more than an eighth of the bottom third of the picture. Side-on at eye level, no perspective. The first attached picture is the layout guide and the second is the background this strip stands in front of: match its style and light, but don't copy the background itself, or the guide's colours, lines or labels. No people, animals, text, letters, numbers, frame or watermark. One 1536 by 1024 PNG.

ROOM. The Old Road: The Farmland. The foreground for panel 1 of 3.

DRAW ONLY THESE, each standing up from the bottom edge: the dark trunk of a blossom tree rising out of the top of the picture, a few boughs of pink blossom across the top, at about 14% across.

AVOID. A background, sky, scenery, ground or floor behind the objects. Any shading, gradient, glow or halo spilling onto the empty area. Objects floating above the bottom edge. Wide bushes or hedges across the bottom. Extra objects not in the list. Perspective. People, animals, text, letters, logos, watermarks. Pixel art, painterly blur, glossy 3D-render shading. Pure-black outlines.
```

#### `road/farmland` · foreground for panel 2 of 3

- **Attach:** `shots/art/room-templates/road-farmland-2.png`, and also your background panel 2 picture (`road-farmland-2.png`), so the strip matches it.
- **Send to Claude as:** `road-farmland-fg-2.png`

```text
STYLE. An original foreground overlay for a side-on 2D fantasy game scene, in the same clean, outlined, vector-like style as the scene behind it: crisp outlines in deep brown to near-black, never pure black, flat fills with two or three cel tones and hard-edged shadow shapes, light from the upper left; but darker and a little more saturated than the scene, because these things stand nearest the camera. Draw only the objects listed below, each standing up from the very bottom edge of the picture at the place given (a share of the picture's width, left to right), cut off by the bottom edge as if the camera stood just behind them. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture at all. Tall things (a lamp post, a trunk) are slim, and together the objects cover no more than an eighth of the bottom third of the picture. Side-on at eye level, no perspective. The first attached picture is the layout guide and the second is the background this strip stands in front of: match its style and light, but don't copy the background itself, or the guide's colours, lines or labels. No people, animals, text, letters, numbers, frame or watermark. One 1536 by 1024 PNG.

ROOM. The Old Road: The Farmland. The foreground for panel 2 of 3.

DRAW ONLY THESE, each standing up from the bottom edge: a young cherry tree, its slim dark trunk rising from the bottom edge to a small round crown of pink blossom about halfway up, at about 59% across.

AVOID. A background, sky, scenery, ground or floor behind the objects. Any shading, gradient, glow or halo spilling onto the empty area. Objects floating above the bottom edge. Wide bushes or hedges across the bottom. Extra objects not in the list. Perspective. People, animals, text, letters, logos, watermarks. Pixel art, painterly blur, glossy 3D-render shading. Pure-black outlines.
```

#### `road/farmland` · foreground for panel 3 of 3

- **Attach:** `shots/art/room-templates/road-farmland-3.png`, and also your background panel 3 picture (`road-farmland-3.png`), so the strip matches it.
- **Send to Claude as:** `road-farmland-fg-3.png`

```text
STYLE. An original foreground overlay for a side-on 2D fantasy game scene, in the same clean, outlined, vector-like style as the scene behind it: crisp outlines in deep brown to near-black, never pure black, flat fills with two or three cel tones and hard-edged shadow shapes, light from the upper left; but darker and a little more saturated than the scene, because these things stand nearest the camera. Draw only the objects listed below, each standing up from the very bottom edge of the picture at the place given (a share of the picture's width, left to right), cut off by the bottom edge as if the camera stood just behind them. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture at all. Tall things (a lamp post, a trunk) are slim, and together the objects cover no more than an eighth of the bottom third of the picture. Side-on at eye level, no perspective. The first attached picture is the layout guide and the second is the background this strip stands in front of: match its style and light, but don't copy the background itself, or the guide's colours, lines or labels. No people, animals, text, letters, numbers, frame or watermark. One 1536 by 1024 PNG.

ROOM. The Old Road: The Farmland. The foreground for panel 3 of 3.

DRAW ONLY THESE, each standing up from the bottom edge: a young cherry tree, its slim dark trunk rising from the bottom edge to a small round crown of pink blossom about halfway up, at about 46% across.

AVOID. A background, sky, scenery, ground or floor behind the objects. Any shading, gradient, glow or halo spilling onto the empty area. Objects floating above the bottom edge. Wide bushes or hedges across the bottom. Extra objects not in the list. Perspective. People, animals, text, letters, logos, watermarks. Pixel art, painterly blur, glossy 3D-render shading. Pure-black outlines.
```

### `road/fork` · The Fork

#### `road/fork` · foreground for panel 1 of 3

- **Attach:** `shots/art/room-templates/road-fork-1.png`, and also your background panel 1 picture (`road-fork-1.png`), so the strip matches it.
- **Send to Claude as:** `road-fork-fg-1.png`

```text
STYLE. An original foreground overlay for a side-on 2D fantasy game scene, in the same clean, outlined, vector-like style as the scene behind it: crisp outlines in deep brown to near-black, never pure black, flat fills with two or three cel tones and hard-edged shadow shapes, light from the upper left; but darker and a little more saturated than the scene, because these things stand nearest the camera. Draw only the objects listed below, each standing up from the very bottom edge of the picture at the place given (a share of the picture's width, left to right), cut off by the bottom edge as if the camera stood just behind them. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture at all. Tall things (a lamp post, a trunk) are slim, and together the objects cover no more than an eighth of the bottom third of the picture. Side-on at eye level, no perspective. The first attached picture is the layout guide and the second is the background this strip stands in front of: match its style and light, but don't copy the background itself, or the guide's colours, lines or labels. No people, animals, text, letters, numbers, frame or watermark. One 1536 by 1024 PNG.

ROOM. The Old Road: The Fork. The foreground for panel 1 of 3.

DRAW ONLY THESE, each standing up from the bottom edge: two or three rounded grey rocks, no taller than a sixth of the picture, at about 14% across; the trunk of an oak rising out of the top of the picture, a few boughs of fresh light-green leaves across the top, at about 53% across; the dark trunk of a blossom tree rising out of the top of the picture, a few boughs of pink blossom across the top, at about 93% across.

AVOID. A background, sky, scenery, ground or floor behind the objects. Any shading, gradient, glow or halo spilling onto the empty area. Objects floating above the bottom edge. Wide bushes or hedges across the bottom. Extra objects not in the list. Perspective. People, animals, text, letters, logos, watermarks. Pixel art, painterly blur, glossy 3D-render shading. Pure-black outlines.
```

#### `road/fork` · foreground for panel 2 of 3

- **Attach:** `shots/art/room-templates/road-fork-2.png`, and also your background panel 2 picture (`road-fork-2.png`), so the strip matches it.
- **Send to Claude as:** `road-fork-fg-2.png`

```text
STYLE. An original foreground overlay for a side-on 2D fantasy game scene, in the same clean, outlined, vector-like style as the scene behind it: crisp outlines in deep brown to near-black, never pure black, flat fills with two or three cel tones and hard-edged shadow shapes, light from the upper left; but darker and a little more saturated than the scene, because these things stand nearest the camera. Draw only the objects listed below, each standing up from the very bottom edge of the picture at the place given (a share of the picture's width, left to right), cut off by the bottom edge as if the camera stood just behind them. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture at all. Tall things (a lamp post, a trunk) are slim, and together the objects cover no more than an eighth of the bottom third of the picture. Side-on at eye level, no perspective. The first attached picture is the layout guide and the second is the background this strip stands in front of: match its style and light, but don't copy the background itself, or the guide's colours, lines or labels. No people, animals, text, letters, numbers, frame or watermark. One 1536 by 1024 PNG.

ROOM. The Old Road: The Fork. The foreground for panel 2 of 3.

DRAW ONLY THESE, each standing up from the bottom edge: the dark trunk of a blossom tree rising out of the top of the picture, a few boughs of pink blossom across the top, at about 6% across; two or three rounded grey rocks, no taller than a sixth of the picture, at about 39% across; the dark trunk of an old oak rising out of the top of the picture, a few boughs of dark green leaves across the top, at about 98% across.

AVOID. A background, sky, scenery, ground or floor behind the objects. Any shading, gradient, glow or halo spilling onto the empty area. Objects floating above the bottom edge. Wide bushes or hedges across the bottom. Extra objects not in the list. Perspective. People, animals, text, letters, logos, watermarks. Pixel art, painterly blur, glossy 3D-render shading. Pure-black outlines.
```

#### `road/fork` · foreground for panel 3 of 3

- **Attach:** `shots/art/room-templates/road-fork-3.png`, and also your background panel 3 picture (`road-fork-3.png`), so the strip matches it.
- **Send to Claude as:** `road-fork-fg-3.png`

```text
STYLE. An original foreground overlay for a side-on 2D fantasy game scene, in the same clean, outlined, vector-like style as the scene behind it: crisp outlines in deep brown to near-black, never pure black, flat fills with two or three cel tones and hard-edged shadow shapes, light from the upper left; but darker and a little more saturated than the scene, because these things stand nearest the camera. Draw only the objects listed below, each standing up from the very bottom edge of the picture at the place given (a share of the picture's width, left to right), cut off by the bottom edge as if the camera stood just behind them. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture at all. Tall things (a lamp post, a trunk) are slim, and together the objects cover no more than an eighth of the bottom third of the picture. Side-on at eye level, no perspective. The first attached picture is the layout guide and the second is the background this strip stands in front of: match its style and light, but don't copy the background itself, or the guide's colours, lines or labels. No people, animals, text, letters, numbers, frame or watermark. One 1536 by 1024 PNG.

ROOM. The Old Road: The Fork. The foreground for panel 3 of 3.

DRAW ONLY THESE, each standing up from the bottom edge: two or three rounded grey rocks, no taller than a sixth of the picture, at about 27% across; the dark trunk of an old oak rising out of the top of the picture, a few boughs of dark green leaves across the top, at about 86% across.

AVOID. A background, sky, scenery, ground or floor behind the objects. Any shading, gradient, glow or halo spilling onto the empty area. Objects floating above the bottom edge. Wide bushes or hedges across the bottom. Extra objects not in the list. Perspective. People, animals, text, letters, logos, watermarks. Pixel art, painterly blur, glossy 3D-render shading. Pure-black outlines.
```

### `mirewood/edge` · The Mirewood's Edge

#### `mirewood/edge` · foreground for panel 1 of 3

- **Attach:** `shots/art/room-templates/mirewood-edge-1.png`, and also your background panel 1 picture (`mirewood-edge-1.png`), so the strip matches it.
- **Send to Claude as:** `mirewood-edge-fg-1.png`

```text
STYLE. An original foreground overlay for a side-on 2D fantasy game scene, in the same clean, outlined, vector-like style as the scene behind it: crisp outlines in deep brown to near-black, never pure black, flat fills with two or three cel tones and hard-edged shadow shapes, light from the upper left; but darker and a little more saturated than the scene, because these things stand nearest the camera. Draw only the objects listed below, each standing up from the very bottom edge of the picture at the place given (a share of the picture's width, left to right), cut off by the bottom edge as if the camera stood just behind them. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture at all. Tall things (a lamp post, a trunk) are slim, and together the objects cover no more than an eighth of the bottom third of the picture. Side-on at eye level, no perspective. The first attached picture is the layout guide and the second is the background this strip stands in front of: match its style and light, but don't copy the background itself, or the guide's colours, lines or labels. No people, animals, text, letters, numbers, frame or watermark. One 1536 by 1024 PNG.

ROOM. The Mirewood: The Mirewood's Edge. The foreground for panel 1 of 3.

DRAW ONLY THESE, each standing up from the bottom edge: the trunk of an oak rising out of the top of the picture, a few boughs of fresh light-green leaves across the top, at about 10% across.

LIGHT. The game darkens this room, so paint them at clear, readable mid values, a little darker than the scene behind.

AVOID. A background, sky, scenery, ground or floor behind the objects. Any shading, gradient, glow or halo spilling onto the empty area. Objects floating above the bottom edge. Wide bushes or hedges across the bottom. Extra objects not in the list. Perspective. People, animals, text, letters, logos, watermarks. Pixel art, painterly blur, glossy 3D-render shading. Pure-black outlines.
```

#### `mirewood/edge` · foreground for panel 3 of 3

- **Attach:** `shots/art/room-templates/mirewood-edge-3.png`, and also your background panel 3 picture (`mirewood-edge-3.png`), so the strip matches it.
- **Send to Claude as:** `mirewood-edge-fg-3.png`

```text
STYLE. An original foreground overlay for a side-on 2D fantasy game scene, in the same clean, outlined, vector-like style as the scene behind it: crisp outlines in deep brown to near-black, never pure black, flat fills with two or three cel tones and hard-edged shadow shapes, light from the upper left; but darker and a little more saturated than the scene, because these things stand nearest the camera. Draw only the objects listed below, each standing up from the very bottom edge of the picture at the place given (a share of the picture's width, left to right), cut off by the bottom edge as if the camera stood just behind them. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture at all. Tall things (a lamp post, a trunk) are slim, and together the objects cover no more than an eighth of the bottom third of the picture. Side-on at eye level, no perspective. The first attached picture is the layout guide and the second is the background this strip stands in front of: match its style and light, but don't copy the background itself, or the guide's colours, lines or labels. No people, animals, text, letters, numbers, frame or watermark. One 1536 by 1024 PNG.

ROOM. The Mirewood: The Mirewood's Edge. The foreground for panel 3 of 3.

DRAW ONLY THESE, each standing up from the bottom edge: a big mossy boulder, no taller than a fifth of the picture, at about 93% across.

LIGHT. The game darkens this room, so paint them at clear, readable mid values, a little darker than the scene behind.

AVOID. A background, sky, scenery, ground or floor behind the objects. Any shading, gradient, glow or halo spilling onto the empty area. Objects floating above the bottom edge. Wide bushes or hedges across the bottom. Extra objects not in the list. Perspective. People, animals, text, letters, logos, watermarks. Pixel art, painterly blur, glossy 3D-render shading. Pure-black outlines.
```

### `mirewood/deep` · The Deep Wood

#### `mirewood/deep` · foreground for panel 1 of 3

- **Attach:** `shots/art/room-templates/mirewood-deep-1.png`, and also your background panel 1 picture (`mirewood-deep-1.png`), so the strip matches it.
- **Send to Claude as:** `mirewood-deep-fg-1.png`

```text
STYLE. An original foreground overlay for a side-on 2D fantasy game scene, in the same clean, outlined, vector-like style as the scene behind it: crisp outlines in deep brown to near-black, never pure black, flat fills with two or three cel tones and hard-edged shadow shapes, light from the upper left; but darker and a little more saturated than the scene, because these things stand nearest the camera. Draw only the objects listed below, each standing up from the very bottom edge of the picture at the place given (a share of the picture's width, left to right), cut off by the bottom edge as if the camera stood just behind them. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture at all. Tall things (a lamp post, a trunk) are slim, and together the objects cover no more than an eighth of the bottom third of the picture. Side-on at eye level, no perspective. The first attached picture is the layout guide and the second is the background this strip stands in front of: match its style and light, but don't copy the background itself, or the guide's colours, lines or labels. No people, animals, text, letters, numbers, frame or watermark. One 1536 by 1024 PNG.

ROOM. The Mirewood: The Deep Wood. The foreground for panel 1 of 3.

DRAW ONLY THESE, each standing up from the bottom edge: the trunk of an oak rising out of the top of the picture, a few boughs of amber and gold leaves across the top, at about 22% across; the trunk of an old tree rising out of the top of the picture, its bark split by black veins with a faint violet light in the cracks, a few dark boughs, at about 63% across.

LIGHT. The game darkens this room, so paint them at clear, readable mid values, a little darker than the scene behind.

AVOID. A background, sky, scenery, ground or floor behind the objects. Any shading, gradient, glow or halo spilling onto the empty area. Objects floating above the bottom edge. Wide bushes or hedges across the bottom. Extra objects not in the list. Perspective. People, animals, text, letters, logos, watermarks. Pixel art, painterly blur, glossy 3D-render shading. Pure-black outlines.
```

#### `mirewood/deep` · foreground for panel 2 of 3

- **Attach:** `shots/art/room-templates/mirewood-deep-2.png`, and also your background panel 2 picture (`mirewood-deep-2.png`), so the strip matches it.
- **Send to Claude as:** `mirewood-deep-fg-2.png`

```text
STYLE. An original foreground overlay for a side-on 2D fantasy game scene, in the same clean, outlined, vector-like style as the scene behind it: crisp outlines in deep brown to near-black, never pure black, flat fills with two or three cel tones and hard-edged shadow shapes, light from the upper left; but darker and a little more saturated than the scene, because these things stand nearest the camera. Draw only the objects listed below, each standing up from the very bottom edge of the picture at the place given (a share of the picture's width, left to right), cut off by the bottom edge as if the camera stood just behind them. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture at all. Tall things (a lamp post, a trunk) are slim, and together the objects cover no more than an eighth of the bottom third of the picture. Side-on at eye level, no perspective. The first attached picture is the layout guide and the second is the background this strip stands in front of: match its style and light, but don't copy the background itself, or the guide's colours, lines or labels. No people, animals, text, letters, numbers, frame or watermark. One 1536 by 1024 PNG.

ROOM. The Mirewood: The Deep Wood. The foreground for panel 2 of 3.

DRAW ONLY THESE, each standing up from the bottom edge: the trunk of an old tree rising out of the top of the picture, its bark split by black veins with a faint violet light in the cracks, a few dark boughs, at about 16% across; a twisted, leafless old trunk rising out of the top of the picture, split by black veins with a faint violet light in the cracks, at about 55% across; the trunk of an old tree rising out of the top of the picture, its bark split by black veins with a faint violet light in the cracks, a few dark boughs, at about 99% across.

LIGHT. The game darkens this room, so paint them at clear, readable mid values, a little darker than the scene behind.

AVOID. A background, sky, scenery, ground or floor behind the objects. Any shading, gradient, glow or halo spilling onto the empty area. Objects floating above the bottom edge. Wide bushes or hedges across the bottom. Extra objects not in the list. Perspective. People, animals, text, letters, logos, watermarks. Pixel art, painterly blur, glossy 3D-render shading. Pure-black outlines.
```

#### `mirewood/deep` · foreground for panel 3 of 3

- **Attach:** `shots/art/room-templates/mirewood-deep-3.png`, and also your background panel 3 picture (`mirewood-deep-3.png`), so the strip matches it.
- **Send to Claude as:** `mirewood-deep-fg-3.png`

```text
STYLE. An original foreground overlay for a side-on 2D fantasy game scene, in the same clean, outlined, vector-like style as the scene behind it: crisp outlines in deep brown to near-black, never pure black, flat fills with two or three cel tones and hard-edged shadow shapes, light from the upper left; but darker and a little more saturated than the scene, because these things stand nearest the camera. Draw only the objects listed below, each standing up from the very bottom edge of the picture at the place given (a share of the picture's width, left to right), cut off by the bottom edge as if the camera stood just behind them. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture at all. Tall things (a lamp post, a trunk) are slim, and together the objects cover no more than an eighth of the bottom third of the picture. Side-on at eye level, no perspective. The first attached picture is the layout guide and the second is the background this strip stands in front of: match its style and light, but don't copy the background itself, or the guide's colours, lines or labels. No people, animals, text, letters, numbers, frame or watermark. One 1536 by 1024 PNG.

ROOM. The Mirewood: The Deep Wood. The foreground for panel 3 of 3.

DRAW ONLY THESE, each standing up from the bottom edge: the trunk of an old tree rising out of the top of the picture, its bark split by black veins with a faint violet light in the cracks, a few dark boughs, at about 4% across; a twisted, leafless old trunk rising out of the top of the picture, split by black veins with a faint violet light in the cracks, at about 43% across; the trunk of an old tree rising out of the top of the picture, its bark split by black veins with a faint violet light in the cracks, a few dark boughs, at about 86% across.

LIGHT. The game darkens this room, so paint them at clear, readable mid values, a little darker than the scene behind.

AVOID. A background, sky, scenery, ground or floor behind the objects. Any shading, gradient, glow or halo spilling onto the empty area. Objects floating above the bottom edge. Wide bushes or hedges across the bottom. Extra objects not in the list. Perspective. People, animals, text, letters, logos, watermarks. Pixel art, painterly blur, glossy 3D-render shading. Pure-black outlines.
```

### `mirewood/camp` · The Rangers' Camp

#### `mirewood/camp` · foreground for panel 1 of 1

- **Attach:** `shots/art/room-templates/mirewood-camp-1.png`, and also your background panel 1 picture (`mirewood-camp-1.png`), so the strip matches it.
- **Send to Claude as:** `mirewood-camp-fg-1.png`

```text
STYLE. An original foreground overlay for a side-on 2D fantasy game scene, in the same clean, outlined, vector-like style as the scene behind it: crisp outlines in deep brown to near-black, never pure black, flat fills with two or three cel tones and hard-edged shadow shapes, light from the upper left; but darker and a little more saturated than the scene, because these things stand nearest the camera. Draw only the objects listed below, each standing up from the very bottom edge of the picture at the place given (a share of the picture's width, left to right), cut off by the bottom edge as if the camera stood just behind them. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture at all. Tall things (a lamp post, a trunk) are slim, and together the objects cover no more than an eighth of the bottom third of the picture. Side-on at eye level, no perspective. The first attached picture is the layout guide and the second is the background this strip stands in front of: match its style and light, but don't copy the background itself, or the guide's colours, lines or labels. No people, animals, text, letters, numbers, frame or watermark. One 1536 by 1024 PNG.

ROOM. The Mirewood: The Rangers' Camp. The foreground for the room.

DRAW ONLY THESE, each standing up from the bottom edge: the dark trunk of an oak rising out of the top of the picture, a few boughs of green leaves across the top, at about 4% across; the trunk of an oak rising out of the top of the picture, a few boughs of fresh light-green leaves across the top, at about 48% across; the dark trunk of an old oak rising out of the top of the picture, a few boughs of dark green leaves across the top, at about 89% across.

LIGHT. The game darkens this room, so paint them at clear, readable mid values, a little darker than the scene behind.

AVOID. A background, sky, scenery, ground or floor behind the objects. Any shading, gradient, glow or halo spilling onto the empty area. Objects floating above the bottom edge. Wide bushes or hedges across the bottom. Extra objects not in the list. Perspective. People, animals, text, letters, logos, watermarks. Pixel art, painterly blur, glossy 3D-render shading. Pure-black outlines.
```

### `thornbridge/river-edge` · The River's Edge

#### `thornbridge/river-edge` · foreground for panel 1 of 1

- **Attach:** `shots/art/room-templates/thornbridge-river-edge-1.png`, and also your background panel 1 picture (`thornbridge-river-edge-1.png`), so the strip matches it.
- **Send to Claude as:** `thornbridge-river-edge-fg-1.png`

```text
STYLE. An original foreground overlay for a side-on 2D fantasy game scene, in the same clean, outlined, vector-like style as the scene behind it: crisp outlines in deep brown to near-black, never pure black, flat fills with two or three cel tones and hard-edged shadow shapes, light from the upper left; but darker and a little more saturated than the scene, because these things stand nearest the camera. Draw only the objects listed below, each standing up from the very bottom edge of the picture at the place given (a share of the picture's width, left to right), cut off by the bottom edge as if the camera stood just behind them. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture at all. Tall things (a lamp post, a trunk) are slim, and together the objects cover no more than an eighth of the bottom third of the picture. Side-on at eye level, no perspective. The first attached picture is the layout guide and the second is the background this strip stands in front of: match its style and light, but don't copy the background itself, or the guide's colours, lines or labels. No people, animals, text, letters, numbers, frame or watermark. One 1536 by 1024 PNG.

ROOM. Thornbridge: The River's Edge. The foreground for the room.

DRAW ONLY THESE, each standing up from the bottom edge: the trunk of an oak rising out of the top of the picture, a few boughs of fresh light-green leaves across the top, at about 10% across; the dark trunk of an oak rising out of the top of the picture, a few boughs of green leaves across the top, at about 85% across.

AVOID. A background, sky, scenery, ground or floor behind the objects. Any shading, gradient, glow or halo spilling onto the empty area. Objects floating above the bottom edge. Wide bushes or hedges across the bottom. Extra objects not in the list. Perspective. People, animals, text, letters, logos, watermarks. Pixel art, painterly blur, glossy 3D-render shading. Pure-black outlines.
```

---

## 5. Floor tiles

The floor band stays a tile map (the course needs one), so the ground under everyone's feet is drawn from tiles, not from the painting. Each tile is a square picture. Attach nothing; paste the block; send the PNG to Claude with the name given. Most are seamless textures; the **overlay** tiles are small things the game lays over the ground (flowers, bones, coins), drawn on an empty background; the **feature** tiles are one picture that fills the square.

These are the tiles the current rooms use, by tileset, from the zone files' legends. The catalog's other tiles (§5.10) aren't used by any room yet.

### 5.1 `questeria`

For the harbour city of Questeria and its docks: warm cream stone, grey cobbles, weathered planks, blue-green water. Used by: `docks/fish-market`, `docks/harbourside`, `docks/pier`, `questeria/festival-square`, `questeria/gate-plaza`, `questeria/lantern-market`, `questeria/sunspire-steps`, `questeria/thornbridge-gate`.

#### `questeria/plaza`

- **Send to Claude as:** `tile-questeria-plaza.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For the harbour city of Questeria and its docks: warm cream stone, grey cobbles, weathered planks, blue-green water. Large pale cream paving slabs laid in a neat running bond, warm stone with faint wear and a few hairline cracks.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `questeria/terrace`

- **Send to Claude as:** `tile-questeria-terrace.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For the harbour city of Questeria and its docks: warm cream stone, grey cobbles, weathered planks, blue-green water. Fine, polished honey-cream flagstones, larger and smoother than street paving, laid square with thin pale joints.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `questeria/cobble`

- **Send to Claude as:** `tile-questeria-cobble.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For the harbour city of Questeria and its docks: warm cream stone, grey cobbles, weathered planks, blue-green water. Rounded cobblestones in soft blue-grey and warm grey, packed tight, each with a small highlight on its upper left.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `questeria/grass`

- **Send to Claude as:** `tile-questeria-grass.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For the harbour city of Questeria and its docks: warm cream stone, grey cobbles, weathered planks, blue-green water. A neat, short city lawn, fresh green with small scalloped tufts in two greens.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `questeria/water`

- **Send to Claude as:** `tile-questeria-water.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For the harbour city of Questeria and its docks: warm cream stone, grey cobbles, weathered planks, blue-green water. Clear blue-green harbour and canal water with soft ripple lines and a few pale glints; one still frame.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `questeria/planks`

- **Send to Claude as:** `tile-questeria-planks.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For the harbour city of Questeria and its docks: warm cream stone, grey cobbles, weathered planks, blue-green water. A boardwalk of weathered honey-brown planks running left to right, dark gaps between them, nail heads, staggered joints.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

### 5.2 `ship`

For the merchant ship the Gull's Wager at night (paint it in clear daylight values; the game darkens it): honey-brown deck planks, navy and gold trim, the deep blue sea. Used by: `gulls-wager/deck`.

#### `ship/deck`

- **Send to Claude as:** `tile-ship-deck.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For the merchant ship the Gull's Wager at night (paint it in clear daylight values; the game darkens it): honey-brown deck planks, navy and gold trim, the deep blue sea. Ship's deck planking: long narrow honey-brown planks running left to right, dark caulked seams, staggered butt joints, a few nail heads.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `ship/sea`

- **Send to Claude as:** `tile-ship-sea.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For the merchant ship the Gull's Wager at night (paint it in clear daylight values; the game darkens it): honey-brown deck planks, navy and gold trim, the deep blue sea. The open sea: deep blue water with long soft swell lines and pale glints; one still frame.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `ship/rail-n`

- **Send to Claude as:** `tile-ship-rail-n.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For the merchant ship the Gull's Wager at night (paint it in clear daylight values; the game darkens it): honey-brown deck planks, navy and gold trim, the deep blue sea. The ship's far bulwark rail seen from above: a broad varnished brown cap rail with a navy-and-gold stripe running left to right across the middle of the square, deck planks either side.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `ship/rail-s`

- **Send to Claude as:** `tile-ship-rail-s.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For the merchant ship the Gull's Wager at night (paint it in clear daylight values; the game darkens it): honey-brown deck planks, navy and gold trim, the deep blue sea. The ship's near bulwark rail seen from above: a broad varnished brown cap rail with a navy-and-gold stripe running left to right across the middle of the square, the sea showing beyond it in the lower part.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

### 5.3 `interior`

For the insides of Questeria's homes, shops and inns: warm oak floorboards, pale flagstones, cream plaster and dark oak panelling. Used by: `bakery/inside`, `chapel/inside`, `cottage-a/inside`, `cottage-b/inside`, `forge/inside`, `gilded-flagon/taproom`, `rusty-anvil/bar`, `shop/inside`, `warehouse/inside`.

#### `interior/floor-wood`

- **Send to Claude as:** `tile-interior-floor-wood.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For the insides of Questeria's homes, shops and inns: warm oak floorboards, pale flagstones, cream plaster and dark oak panelling. Warm honey-brown oak floorboards running left to right, long boards with staggered joints, soft grain lines and a few nail heads.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `interior/floor-stone`

- **Send to Claude as:** `tile-interior-floor-stone.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For the insides of Questeria's homes, shops and inns: warm oak floorboards, pale flagstones, cream plaster and dark oak panelling. Pale warm-grey flagstones of mixed rectangular sizes with thin mortar joints and gentle wear.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `interior/rug`

- **Send to Claude as:** `tile-interior-rug.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For the insides of Questeria's homes, shops and inns: warm oak floorboards, pale flagstones, cream plaster and dark oak panelling. The woven field of a rug: a small repeating diamond pattern in crimson, gold and navy, no border (the game draws the rug's edge).

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `interior/wall-top`

- **Send to Claude as:** `tile-interior-wall-top.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For the insides of Questeria's homes, shops and inns: warm oak floorboards, pale flagstones, cream plaster and dark oak panelling. The top of a plastered wall seen from straight above: a cream plaster cap with a dark oak edge running left to right.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `interior/wall-face`

- **Send to Claude as:** `tile-interior-wall-face.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For the insides of Questeria's homes, shops and inns: warm oak floorboards, pale flagstones, cream plaster and dark oak panelling. A back wall's face: cream lime plaster with faint trowel marks, a dark oak beam running left to right across it.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `interior/wall-base`

- **Send to Claude as:** `tile-interior-wall-base.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For the insides of Questeria's homes, shops and inns: warm oak floorboards, pale flagstones, cream plaster and dark oak panelling. The foot of a wall: dark oak wainscot panelling in tall rectangular panels with a moulded rail along the top.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

### 5.4 `crypt`

For an ancient burial barrow under a grassy mound: big grey-violet stones, moss in the joints. Used by: `barrow/hall`, `barrow/side-room`, `barrow/steps`.

#### `crypt/floor`

- **Send to Claude as:** `tile-crypt-floor.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For an ancient burial barrow under a grassy mound: big grey-violet stones, moss in the joints. Big, worn grey-violet stone flags of mixed sizes, slightly uneven, a little moss in the joints.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `crypt/steps`

- **Send to Claude as:** `tile-crypt-steps.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For an ancient burial barrow under a grassy mound: big grey-violet stones, moss in the joints. Worn grey-violet stone stair treads seen from above, running left to right in bands, each tread lit along its front edge.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `crypt/wall-top`

- **Send to Claude as:** `tile-crypt-wall-top.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For an ancient burial barrow under a grassy mound: big grey-violet stones, moss in the joints. The top of a thick old wall seen from straight above: rough grey-violet capstones with moss in the joints.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `crypt/bones` · overlay

- **Send to Claude as:** `tile-crypt-bones.png`

```text
STYLE. A small overlay for a floor tile in a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on. It is laid over a ground texture, so draw only the thing described below, centred, and keep it at least 64 px away from every edge unless the description says otherwise. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture. No ground under it, no cast shadow beyond a thin contact shadow.

OVERLAY. For an ancient burial barrow under a grassy mound: big grey-violet stones, moss in the joints. A few old, pale, broken bones lying scattered.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. A ground or background under the thing, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

### 5.5 `meadow`

For the sunny meadows and farmland round Questeria and the refugee camp at Thornbridge: fresh green grass, pale earth paths, a clear stream. Used by: `road/farmland`, `road/fork`, `thornbridge/camp-gate`, `thornbridge/river-edge`, `thornbridge/tents`.

#### `meadow/grass`

- **Send to Claude as:** `tile-meadow-grass.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For the sunny meadows and farmland round Questeria and the refugee camp at Thornbridge: fresh green grass, pale earth paths, a clear stream. Bright, sunny meadow grass, fresh green with lighter tufts and tiny scalloped blades.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `meadow/long-grass`

- **Send to Claude as:** `tile-meadow-long-grass.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For the sunny meadows and farmland round Questeria and the refugee camp at Thornbridge: fresh green grass, pale earth paths, a clear stream. Tall meadow grass seen from above, two greens, swaying, with pale seed heads.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `meadow/path`

- **Send to Claude as:** `tile-meadow-path.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For the sunny meadows and farmland round Questeria and the refugee camp at Thornbridge: fresh green grass, pale earth paths, a clear stream. A narrow footpath of pale trodden earth with a few small stones.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `meadow/road`

- **Send to Claude as:** `tile-meadow-road.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For the sunny meadows and farmland round Questeria and the refugee camp at Thornbridge: fresh green grass, pale earth paths, a clear stream. A broad country road of packed light-brown earth and fine gravel.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `meadow/soil`

- **Send to Claude as:** `tile-meadow-soil.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For the sunny meadows and farmland round Questeria and the refugee camp at Thornbridge: fresh green grass, pale earth paths, a clear stream. Ploughed field soil in rich brown furrows running left to right.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `meadow/stream`

- **Send to Claude as:** `tile-meadow-stream.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For the sunny meadows and farmland round Questeria and the refugee camp at Thornbridge: fresh green grass, pale earth paths, a clear stream. A clear, shallow, sparkling blue stream with soft ripple lines and pebbles showing under the water; one still frame.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `meadow/wall`

- **Send to Claude as:** `tile-meadow-wall.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For the sunny meadows and farmland round Questeria and the refugee camp at Thornbridge: fresh green grass, pale earth paths, a clear stream. The top of the city wall seen from straight above: pale cream coping stones running left to right.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `meadow/ruts` · overlay

- **Send to Claude as:** `tile-meadow-ruts.png`

```text
STYLE. A small overlay for a floor tile in a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on. It is laid over a ground texture, so draw only the thing described below, centred, and keep it at least 64 px away from every edge unless the description says otherwise. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture. No ground under it, no cast shadow beyond a thin contact shadow.

OVERLAY. For the sunny meadows and farmland round Questeria and the refugee camp at Thornbridge: fresh green grass, pale earth paths, a clear stream. A pair of cart-wheel ruts running straight across from the left edge to the right edge (these may touch both side edges and must line up across them).

OUTPUT. One square PNG, 1024 by 1024.

AVOID. A ground or background under the thing, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `meadow/reeds` · overlay

- **Send to Claude as:** `tile-meadow-reeds.png`

```text
STYLE. A small overlay for a floor tile in a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on. It is laid over a ground texture, so draw only the thing described below, centred, and keep it at least 64 px away from every edge unless the description says otherwise. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture. No ground under it, no cast shadow beyond a thin contact shadow.

OVERLAY. For the sunny meadows and farmland round Questeria and the refugee camp at Thornbridge: fresh green grass, pale earth paths, a clear stream. A clump of green reeds with a few brown bulrush heads.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. A ground or background under the thing, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `meadow/crops` · overlay

- **Send to Claude as:** `tile-meadow-crops.png`

```text
STYLE. A small overlay for a floor tile in a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on. It is laid over a ground texture, so draw only the thing described below, centred, and keep it at least 64 px away from every edge unless the description says otherwise. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture. No ground under it, no cast shadow beyond a thin contact shadow.

OVERLAY. For the sunny meadows and farmland round Questeria and the refugee camp at Thornbridge: fresh green grass, pale earth paths, a clear stream. A row of young green crop plants running straight across from the left edge to the right edge (these may touch both side edges and must line up across them).

OUTPUT. One square PNG, 1024 by 1024.

AVOID. A ground or background under the thing, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `meadow/petals` · overlay

- **Send to Claude as:** `tile-meadow-petals.png`

```text
STYLE. A small overlay for a floor tile in a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on. It is laid over a ground texture, so draw only the thing described below, centred, and keep it at least 64 px away from every edge unless the description says otherwise. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture. No ground under it, no cast shadow beyond a thin contact shadow.

OVERLAY. For the sunny meadows and farmland round Questeria and the refugee camp at Thornbridge: fresh green grass, pale earth paths, a clear stream. A light scatter of pink blossom petals.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. A ground or background under the thing, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `meadow/flowers` · overlay

- **Send to Claude as:** `tile-meadow-flowers.png`

```text
STYLE. A small overlay for a floor tile in a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on. It is laid over a ground texture, so draw only the thing described below, centred, and keep it at least 64 px away from every edge unless the description says otherwise. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture. No ground under it, no cast shadow beyond a thin contact shadow.

OVERLAY. For the sunny meadows and farmland round Questeria and the refugee camp at Thornbridge: fresh green grass, pale earth paths, a clear stream. A few small wildflowers in pink, white, blue and yellow.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. A ground or background under the thing, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `meadow/clover` · overlay

- **Send to Claude as:** `tile-meadow-clover.png`

```text
STYLE. A small overlay for a floor tile in a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on. It is laid over a ground texture, so draw only the thing described below, centred, and keep it at least 64 px away from every edge unless the description says otherwise. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture. No ground under it, no cast shadow beyond a thin contact shadow.

OVERLAY. For the sunny meadows and farmland round Questeria and the refugee camp at Thornbridge: fresh green grass, pale earth paths, a clear stream. A small patch of clover with tiny white flowers.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. A ground or background under the thing, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `meadow/pebbles` · overlay

- **Send to Claude as:** `tile-meadow-pebbles.png`

```text
STYLE. A small overlay for a floor tile in a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on. It is laid over a ground texture, so draw only the thing described below, centred, and keep it at least 64 px away from every edge unless the description says otherwise. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture. No ground under it, no cast shadow beyond a thin contact shadow.

OVERLAY. For the sunny meadows and farmland round Questeria and the refugee camp at Thornbridge: fresh green grass, pale earth paths, a clear stream. A few small rounded pebbles.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. A ground or background under the thing, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `meadow/tuft` · overlay

- **Send to Claude as:** `tile-meadow-tuft.png`

```text
STYLE. A small overlay for a floor tile in a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on. It is laid over a ground texture, so draw only the thing described below, centred, and keep it at least 64 px away from every edge unless the description says otherwise. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture. No ground under it, no cast shadow beyond a thin contact shadow.

OVERLAY. For the sunny meadows and farmland round Questeria and the refugee camp at Thornbridge: fresh green grass, pale earth paths, a clear stream. Two or three tufts of longer grass.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. A ground or background under the thing, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `meadow/lilies` · overlay

- **Send to Claude as:** `tile-meadow-lilies.png`

```text
STYLE. A small overlay for a floor tile in a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on. It is laid over a ground texture, so draw only the thing described below, centred, and keep it at least 64 px away from every edge unless the description says otherwise. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture. No ground under it, no cast shadow beyond a thin contact shadow.

OVERLAY. For the sunny meadows and farmland round Questeria and the refugee camp at Thornbridge: fresh green grass, pale earth paths, a clear stream. Two water-lily pads, one with a white flower.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. A ground or background under the thing, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

### 5.6 `mirewood`

For the Mirewood, a forest going bad at dusk: lush green at its edge, wilting and rotting deeper in, black veins with a faint violet light, black tar. Used by: `mirewood/camp`, `mirewood/deep`, `mirewood/edge`.

#### `mirewood/grass`

- **Send to Claude as:** `tile-mirewood-grass.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For the Mirewood, a forest going bad at dusk: lush green at its edge, wilting and rotting deeper in, black veins with a faint violet light, black tar. Lush, deep-green forest grass, soft and short, in rounded clumps.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `mirewood/grass-wilt`

- **Send to Claude as:** `tile-mirewood-grass-wilt.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For the Mirewood, a forest going bad at dusk: lush green at its edge, wilting and rotting deeper in, black veins with a faint violet light, black tar. The same forest grass going tired: olive and yellow-green, a little sparse.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `mirewood/grass-rot`

- **Send to Claude as:** `tile-mirewood-grass-rot.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For the Mirewood, a forest going bad at dusk: lush green at its edge, wilting and rotting deeper in, black veins with a faint violet light, black tar. Rotten grass: grey-green and brown, flattened, with thin black threads through it.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `mirewood/leaves`

- **Send to Claude as:** `tile-mirewood-leaves.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For the Mirewood, a forest going bad at dusk: lush green at its edge, wilting and rotting deeper in, black veins with a faint violet light, black tar. A carpet of fallen oak leaves in amber, rust and gold over dark earth.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `mirewood/leaves-blackvein`

- **Send to Claude as:** `tile-mirewood-leaves-blackvein.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For the Mirewood, a forest going bad at dusk: lush green at its edge, wilting and rotting deeper in, black veins with a faint violet light, black tar. Fallen leaves gone dark and brittle, each with fine black veins and a faint violet sheen.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `mirewood/gloom-moss`

- **Send to Claude as:** `tile-mirewood-gloom-moss.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For the Mirewood, a forest going bad at dusk: lush green at its edge, wilting and rotting deeper in, black veins with a faint violet light, black tar. Thick, spongy moss in deep blue-green with tiny pale spore dots.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `mirewood/trail`

- **Send to Claude as:** `tile-mirewood-trail.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For the Mirewood, a forest going bad at dusk: lush green at its edge, wilting and rotting deeper in, black veins with a faint violet light, black tar. A packed earth forest trail, brown, with small stones.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `mirewood/tar-stream`

- **Send to Claude as:** `tile-mirewood-tar-stream.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For the Mirewood, a forest going bad at dusk: lush green at its edge, wilting and rotting deeper in, black veins with a faint violet light, black tar. A slow stream of thick black tar with oily violet and green sheens and slow ripple rings; one still frame.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `mirewood/undergrowth`

- **Send to Claude as:** `tile-mirewood-undergrowth.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For the Mirewood, a forest going bad at dusk: lush green at its edge, wilting and rotting deeper in, black veins with a faint violet light, black tar. Dense green undergrowth of ferns and shrubs seen from above, too thick to walk through.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `mirewood/undergrowth-rot`

- **Send to Claude as:** `tile-mirewood-undergrowth-rot.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For the Mirewood, a forest going bad at dusk: lush green at its edge, wilting and rotting deeper in, black veins with a faint violet light, black tar. The same undergrowth rotting: grey-brown leaves, black stems, a few thorns.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `mirewood/tuft` · overlay

- **Send to Claude as:** `tile-mirewood-tuft.png`

```text
STYLE. A small overlay for a floor tile in a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on. It is laid over a ground texture, so draw only the thing described below, centred, and keep it at least 64 px away from every edge unless the description says otherwise. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture. No ground under it, no cast shadow beyond a thin contact shadow.

OVERLAY. For the Mirewood, a forest going bad at dusk: lush green at its edge, wilting and rotting deeper in, black veins with a faint violet light, black tar. Two or three tufts of forest grass.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. A ground or background under the thing, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `mirewood/flowers` · overlay

- **Send to Claude as:** `tile-mirewood-flowers.png`

```text
STYLE. A small overlay for a floor tile in a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on. It is laid over a ground texture, so draw only the thing described below, centred, and keep it at least 64 px away from every edge unless the description says otherwise. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture. No ground under it, no cast shadow beyond a thin contact shadow.

OVERLAY. For the Mirewood, a forest going bad at dusk: lush green at its edge, wilting and rotting deeper in, black veins with a faint violet light, black tar. A few small white and pink woodland flowers.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. A ground or background under the thing, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `mirewood/fern` · overlay

- **Send to Claude as:** `tile-mirewood-fern.png`

```text
STYLE. A small overlay for a floor tile in a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on. It is laid over a ground texture, so draw only the thing described below, centred, and keep it at least 64 px away from every edge unless the description says otherwise. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture. No ground under it, no cast shadow beyond a thin contact shadow.

OVERLAY. For the Mirewood, a forest going bad at dusk: lush green at its edge, wilting and rotting deeper in, black veins with a faint violet light, black tar. A small fern seen from above.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. A ground or background under the thing, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `mirewood/pebbles` · overlay

- **Send to Claude as:** `tile-mirewood-pebbles.png`

```text
STYLE. A small overlay for a floor tile in a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on. It is laid over a ground texture, so draw only the thing described below, centred, and keep it at least 64 px away from every edge unless the description says otherwise. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture. No ground under it, no cast shadow beyond a thin contact shadow.

OVERLAY. For the Mirewood, a forest going bad at dusk: lush green at its edge, wilting and rotting deeper in, black veins with a faint violet light, black tar. A few small grey pebbles.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. A ground or background under the thing, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `mirewood/roots` · overlay

- **Send to Claude as:** `tile-mirewood-roots.png`

```text
STYLE. A small overlay for a floor tile in a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on. It is laid over a ground texture, so draw only the thing described below, centred, and keep it at least 64 px away from every edge unless the description says otherwise. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture. No ground under it, no cast shadow beyond a thin contact shadow.

OVERLAY. For the Mirewood, a forest going bad at dusk: lush green at its edge, wilting and rotting deeper in, black veins with a faint violet light, black tar. A short tangle of brown surface roots.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. A ground or background under the thing, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `mirewood/litter` · overlay

- **Send to Claude as:** `tile-mirewood-litter.png`

```text
STYLE. A small overlay for a floor tile in a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on. It is laid over a ground texture, so draw only the thing described below, centred, and keep it at least 64 px away from every edge unless the description says otherwise. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture. No ground under it, no cast shadow beyond a thin contact shadow.

OVERLAY. For the Mirewood, a forest going bad at dusk: lush green at its edge, wilting and rotting deeper in, black veins with a faint violet light, black tar. A scatter of twigs and fallen leaves.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. A ground or background under the thing, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `mirewood/veins` · overlay

- **Send to Claude as:** `tile-mirewood-veins.png`

```text
STYLE. A small overlay for a floor tile in a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on. It is laid over a ground texture, so draw only the thing described below, centred, and keep it at least 64 px away from every edge unless the description says otherwise. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture. No ground under it, no cast shadow beyond a thin contact shadow.

OVERLAY. For the Mirewood, a forest going bad at dusk: lush green at its edge, wilting and rotting deeper in, black veins with a faint violet light, black tar. Thin black veins crawling over the ground with a faint violet glow along them.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. A ground or background under the thing, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `mirewood/tuft-dead` · overlay

- **Send to Claude as:** `tile-mirewood-tuft-dead.png`

```text
STYLE. A small overlay for a floor tile in a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on. It is laid over a ground texture, so draw only the thing described below, centred, and keep it at least 64 px away from every edge unless the description says otherwise. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture. No ground under it, no cast shadow beyond a thin contact shadow.

OVERLAY. For the Mirewood, a forest going bad at dusk: lush green at its edge, wilting and rotting deeper in, black veins with a faint violet light, black tar. Two or three dead, grey grass tufts.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. A ground or background under the thing, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `mirewood/toadstools` · overlay

- **Send to Claude as:** `tile-mirewood-toadstools.png`

```text
STYLE. A small overlay for a floor tile in a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on. It is laid over a ground texture, so draw only the thing described below, centred, and keep it at least 64 px away from every edge unless the description says otherwise. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture. No ground under it, no cast shadow beyond a thin contact shadow.

OVERLAY. For the Mirewood, a forest going bad at dusk: lush green at its edge, wilting and rotting deeper in, black veins with a faint violet light, black tar. A small cluster of toadstools with pale violet caps.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. A ground or background under the thing, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `mirewood/tar-spatter` · overlay

- **Send to Claude as:** `tile-mirewood-tar-spatter.png`

```text
STYLE. A small overlay for a floor tile in a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on. It is laid over a ground texture, so draw only the thing described below, centred, and keep it at least 64 px away from every edge unless the description says otherwise. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture. No ground under it, no cast shadow beyond a thin contact shadow.

OVERLAY. For the Mirewood, a forest going bad at dusk: lush green at its edge, wilting and rotting deeper in, black veins with a faint violet light, black tar. A few glossy spatters of black tar.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. A ground or background under the thing, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `mirewood/brambles` · overlay

- **Send to Claude as:** `tile-mirewood-brambles.png`

```text
STYLE. A small overlay for a floor tile in a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on. It is laid over a ground texture, so draw only the thing described below, centred, and keep it at least 64 px away from every edge unless the description says otherwise. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture. No ground under it, no cast shadow beyond a thin contact shadow.

OVERLAY. For the Mirewood, a forest going bad at dusk: lush green at its edge, wilting and rotting deeper in, black veins with a faint violet light, black tar. A knot of dark thorny brambles.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. A ground or background under the thing, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `mirewood/tar-slick` · overlay

- **Send to Claude as:** `tile-mirewood-tar-slick.png`

```text
STYLE. A small overlay for a floor tile in a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on. It is laid over a ground texture, so draw only the thing described below, centred, and keep it at least 64 px away from every edge unless the description says otherwise. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture. No ground under it, no cast shadow beyond a thin contact shadow.

OVERLAY. For the Mirewood, a forest going bad at dusk: lush green at its edge, wilting and rotting deeper in, black veins with a faint violet light, black tar. A small glossy puddle of black tar with an oily violet-green sheen; one still frame.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. A ground or background under the thing, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

### 5.7 `quarry`

For an old stone quarry deep under the Mirewood: grey cut stone with chisel marks, black roots, black tar, glowing blue-green fungus. Used by: `mirewood-quarry/cut`, `mirewood-quarry/floor`, `mirewood-quarry/lip`.

#### `quarry/floor`

- **Send to Claude as:** `tile-quarry-floor.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For an old stone quarry deep under the Mirewood: grey cut stone with chisel marks, black roots, black tar, glowing blue-green fungus. Cut grey quarry stone worn flat, in big squared blocks with chisel marks and stone dust.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `quarry/root-floor`

- **Send to Claude as:** `tile-quarry-root-floor.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For an old stone quarry deep under the Mirewood: grey cut stone with chisel marks, black roots, black tar, glowing blue-green fungus. Broken grey stone floor overgrown by a mat of thick black roots and dark earth.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `quarry/ramp`

- **Send to Claude as:** `tile-quarry-ramp.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For an old stone quarry deep under the Mirewood: grey cut stone with chisel marks, black roots, black tar, glowing blue-green fungus. A ramp of packed gravel and earth with old cart ruts.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `quarry/wall-top`

- **Send to Claude as:** `tile-quarry-wall-top.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For an old stone quarry deep under the Mirewood: grey cut stone with chisel marks, black roots, black tar, glowing blue-green fungus. The top edge of a cut stone wall seen from straight above: squared grey blocks in a row.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `quarry/tar-seep`

- **Send to Claude as:** `tile-quarry-tar-seep.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For an old stone quarry deep under the Mirewood: grey cut stone with chisel marks, black roots, black tar, glowing blue-green fungus. Black tar welling up through cracks in grey stone, glossy, with an oily sheen; one still frame.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `quarry/gravel` · overlay

- **Send to Claude as:** `tile-quarry-gravel.png`

```text
STYLE. A small overlay for a floor tile in a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on. It is laid over a ground texture, so draw only the thing described below, centred, and keep it at least 64 px away from every edge unless the description says otherwise. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture. No ground under it, no cast shadow beyond a thin contact shadow.

OVERLAY. For an old stone quarry deep under the Mirewood: grey cut stone with chisel marks, black roots, black tar, glowing blue-green fungus. A scatter of grey gravel and stone chips.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. A ground or background under the thing, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `quarry/roots` · overlay

- **Send to Claude as:** `tile-quarry-roots.png`

```text
STYLE. A small overlay for a floor tile in a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on. It is laid over a ground texture, so draw only the thing described below, centred, and keep it at least 64 px away from every edge unless the description says otherwise. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture. No ground under it, no cast shadow beyond a thin contact shadow.

OVERLAY. For an old stone quarry deep under the Mirewood: grey cut stone with chisel marks, black roots, black tar, glowing blue-green fungus. Two or three black roots crossing the ground.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. A ground or background under the thing, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `quarry/fungus` · overlay

- **Send to Claude as:** `tile-quarry-fungus.png`

```text
STYLE. A small overlay for a floor tile in a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on. It is laid over a ground texture, so draw only the thing described below, centred, and keep it at least 64 px away from every edge unless the description says otherwise. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture. No ground under it, no cast shadow beyond a thin contact shadow.

OVERLAY. For an old stone quarry deep under the Mirewood: grey cut stone with chisel marks, black roots, black tar, glowing blue-green fungus. A small cluster of softly glowing blue-green fungus.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. A ground or background under the thing, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `quarry/tools` · overlay

- **Send to Claude as:** `tile-quarry-tools.png`

```text
STYLE. A small overlay for a floor tile in a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on. It is laid over a ground texture, so draw only the thing described below, centred, and keep it at least 64 px away from every edge unless the description says otherwise. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture. No ground under it, no cast shadow beyond a thin contact shadow.

OVERLAY. For an old stone quarry deep under the Mirewood: grey cut stone with chisel marks, black roots, black tar, glowing blue-green fungus. An old rusty pick and a broken shovel lying flat.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. A ground or background under the thing, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `quarry/shards` · overlay

- **Send to Claude as:** `tile-quarry-shards.png`

```text
STYLE. A small overlay for a floor tile in a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on. It is laid over a ground texture, so draw only the thing described below, centred, and keep it at least 64 px away from every edge unless the description says otherwise. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture. No ground under it, no cast shadow beyond a thin contact shadow.

OVERLAY. For an old stone quarry deep under the Mirewood: grey cut stone with chisel marks, black roots, black tar, glowing blue-green fungus. A few small black crystal shards with violet edges.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. A ground or background under the thing, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

### 5.8 `vault`

For the Old Crown Vault under the royal keep: pale cream polished stone with gold let into the joints, navy and gold inlay, and the brown earth the sappers dug through. Used by: `old-crown-vault/entrance`, `old-crown-vault/halls`, `old-crown-vault/strongroom`, `old-crown-vault/treasury`.

#### `vault/floor`

- **Send to Claude as:** `tile-vault-floor.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For the Old Crown Vault under the royal keep: pale cream polished stone with gold let into the joints, navy and gold inlay, and the brown earth the sappers dug through. Pale cream polished stone floor slabs with thin gold lines in the joints.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `vault/inlay`

- **Send to Claude as:** `tile-vault-inlay.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For the Old Crown Vault under the royal keep: pale cream polished stone with gold let into the joints, navy and gold inlay, and the brown earth the sappers dug through. A field of inlaid stone: a repeating pattern of small gold suns on deep navy, no border (the game draws its gold edge).

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `vault/tunnel`

- **Send to Claude as:** `tile-vault-tunnel.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For the Old Crown Vault under the royal keep: pale cream polished stone with gold let into the joints, navy and gold inlay, and the brown earth the sappers dug through. The dug floor of a rough tunnel: packed brown earth with stones and pick marks.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `vault/steps`

- **Send to Claude as:** `tile-vault-steps.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For the Old Crown Vault under the royal keep: pale cream polished stone with gold let into the joints, navy and gold inlay, and the brown earth the sappers dug through. Pale cream stone stair treads edged with gold, seen from above, running left to right in bands.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `vault/wall-top`

- **Send to Claude as:** `tile-vault-wall-top.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For the Old Crown Vault under the royal keep: pale cream polished stone with gold let into the joints, navy and gold inlay, and the brown earth the sappers dug through. The top of a dressed cream stone wall seen from straight above, a thin gold line along it.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `vault/earth`

- **Send to Claude as:** `tile-vault-earth.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For the Old Crown Vault under the royal keep: pale cream polished stone with gold let into the joints, navy and gold inlay, and the brown earth the sappers dug through. Loose, dug brown earth with stones and a few broken pieces of pale slab.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `vault/earth-top`

- **Send to Claude as:** `tile-vault-earth-top.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For the Old Crown Vault under the royal keep: pale cream polished stone with gold let into the joints, navy and gold inlay, and the brown earth the sappers dug through. A bank of heaped brown earth seen from above, darker toward the bottom.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `vault/pressure-plate` · feature

- **Send to Claude as:** `tile-vault-pressure-plate.png`

```text
STYLE. A square tile for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Flat on, with no perspective. It is one picture that fills the whole square edge to edge, so that it fits against the plain tiles beside it: no border, frame or vignette.

TILE. For the Old Crown Vault under the royal keep: pale cream polished stone with gold let into the joints, navy and gold inlay, and the brown earth the sappers dug through. A square pressure plate set into a pale cream polished floor: a slightly raised slab with a gold rim and a small gold sun mark, the floor round it filling the corners.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `vault/debris` · overlay

- **Send to Claude as:** `tile-vault-debris.png`

```text
STYLE. A small overlay for a floor tile in a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on. It is laid over a ground texture, so draw only the thing described below, centred, and keep it at least 64 px away from every edge unless the description says otherwise. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture. No ground under it, no cast shadow beyond a thin contact shadow.

OVERLAY. For the Old Crown Vault under the royal keep: pale cream polished stone with gold let into the joints, navy and gold inlay, and the brown earth the sappers dug through. Broken fragments of pale slab and a little stone dust.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. A ground or background under the thing, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `vault/coins` · overlay

- **Send to Claude as:** `tile-vault-coins.png`

```text
STYLE. A small overlay for a floor tile in a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on. It is laid over a ground texture, so draw only the thing described below, centred, and keep it at least 64 px away from every edge unless the description says otherwise. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture. No ground under it, no cast shadow beyond a thin contact shadow.

OVERLAY. For the Old Crown Vault under the royal keep: pale cream polished stone with gold let into the joints, navy and gold inlay, and the brown earth the sappers dug through. A small scatter of gold coins.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. A ground or background under the thing, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `vault/darts` · overlay

- **Send to Claude as:** `tile-vault-darts.png`

```text
STYLE. A small overlay for a floor tile in a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on. It is laid over a ground texture, so draw only the thing described below, centred, and keep it at least 64 px away from every edge unless the description says otherwise. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture. No ground under it, no cast shadow beyond a thin contact shadow.

OVERLAY. For the Old Crown Vault under the royal keep: pale cream polished stone with gold let into the joints, navy and gold inlay, and the brown earth the sappers dug through. A few small spent darts with brass heads.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. A ground or background under the thing, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `vault/bones` · overlay

- **Send to Claude as:** `tile-vault-bones.png`

```text
STYLE. A small overlay for a floor tile in a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on. It is laid over a ground texture, so draw only the thing described below, centred, and keep it at least 64 px away from every edge unless the description says otherwise. Everything else is empty: a transparent background, or, if transparency isn't available, perfectly flat magenta #FF00FF with no shading, gradient or texture. No ground under it, no cast shadow beyond a thin contact shadow.

OVERLAY. For the Old Crown Vault under the royal keep: pale cream polished stone with gold let into the joints, navy and gold inlay, and the brown earth the sappers dug through. A few old, pale bones.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. A ground or background under the thing, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

### 5.9 `keep`

For Sunspire Keep, the royal palace: polished cream marble, crimson and gold carpet, cream ashlar walls over a blue-grey dado. Used by: `sunspire-keep/great-hall`.

#### `keep/marble`

- **Send to Claude as:** `tile-keep-marble.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For Sunspire Keep, the royal palace: polished cream marble, crimson and gold carpet, cream ashlar walls over a blue-grey dado. Polished cream marble floor squares with soft grey veining, the squares meeting across the edges.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `keep/carpet`

- **Send to Claude as:** `tile-keep-carpet.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For Sunspire Keep, the royal palace: polished cream marble, crimson and gold carpet, cream ashlar walls over a blue-grey dado. The field of a royal carpet: deep crimson with a small repeating gold sun motif, no border (the game draws its edge).

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `keep/dais`

- **Send to Claude as:** `tile-keep-dais.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For Sunspire Keep, the royal palace: polished cream marble, crimson and gold carpet, cream ashlar walls over a blue-grey dado. The top of the throne dais: pale gold-veined marble.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `keep/steps`

- **Send to Claude as:** `tile-keep-steps.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For Sunspire Keep, the royal palace: polished cream marble, crimson and gold carpet, cream ashlar walls over a blue-grey dado. Marble stair treads with gold nosing, seen from above, running left to right in bands.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `keep/wall-top`

- **Send to Claude as:** `tile-keep-wall-top.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For Sunspire Keep, the royal palace: polished cream marble, crimson and gold carpet, cream ashlar walls over a blue-grey dado. The top of a cream ashlar wall seen from straight above.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `keep/wall-high`

- **Send to Claude as:** `tile-keep-wall-high.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For Sunspire Keep, the royal palace: polished cream marble, crimson and gold carpet, cream ashlar walls over a blue-grey dado. The upper part of a palace wall: cream ashlar under a moulded cornice running left to right.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `keep/wall-face`

- **Send to Claude as:** `tile-keep-wall-face.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For Sunspire Keep, the royal palace: polished cream marble, crimson and gold carpet, cream ashlar walls over a blue-grey dado. The middle of a palace wall: cream ashlar blocks in neat courses.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `keep/wall-base`

- **Send to Claude as:** `tile-keep-wall-base.png`

```text
STYLE. A square floor tile texture for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, a little finer than on buildings; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Seen from straight above, flat on, with no perspective and no shadow cast from outside the square. It must tile seamlessly: whatever leaves one edge comes back in at the opposite edge, so a grid of copies shows no seams, and no single feature stands out enough to make the repeat obvious. Calm and medium-low in contrast, so characters standing on it stand out. The texture fills the whole square edge to edge: no border, frame, vignette, or light spot in the middle.

TILE. For Sunspire Keep, the royal palace: polished cream marble, crimson and gold carpet, cream ashlar walls over a blue-grey dado. The foot of a palace wall: a blue-grey dado with a gold rail along its top.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. Visible seams or a tile grid, borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `keep/window-high` · feature

- **Send to Claude as:** `tile-keep-window-high.png`

```text
STYLE. A square tile for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Flat on, with no perspective. It is one picture that fills the whole square edge to edge, so that it fits against the plain tiles beside it: no border, frame or vignette.

TILE. For Sunspire Keep, the royal palace: polished cream marble, crimson and gold carpet, cream ashlar walls over a blue-grey dado. The arched top of a tall stained-glass window set in cream stone: the stone arch fills the square, the glass inside it blue, gold and rose.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

#### `keep/window` · feature

- **Send to Claude as:** `tile-keep-window.png`

```text
STYLE. A square tile for a side-on 2D fantasy role-playing game, hand-drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones and hard-edged shadow shapes; a bright, mystical fantasy palette; light from the upper left. Flat on, with no perspective. It is one picture that fills the whole square edge to edge, so that it fits against the plain tiles beside it: no border, frame or vignette.

TILE. For Sunspire Keep, the royal palace: polished cream marble, crimson and gold carpet, cream ashlar walls over a blue-grey dado. The lower part of a tall stained-glass window set in cream stone: the glass fills the middle of the square, panes of blue, gold and rose, a stone sill along the bottom.

OUTPUT. One square PNG, 1024 by 1024.

AVOID. borders, frames, vignettes, a light spot in the middle. Perspective, a horizon, a side view. Text, letters, numbers, logos, watermarks. People, animals, footprints. Pixel art, photographic texture, painterly blur, glossy 3D-render shading. Pure-black outlines. Busy, high-contrast noise.
```

### 5.10 Catalog tiles no room uses yet

`World/TilesetCatalog.cs` lists these too. No room's tile strip uses them yet, so they need no picture until one does; when one does, Claude adds its block here. The `forest` tileset is used by no zone at all.

| Tileset | Tiles |
| --- | --- |
| `questeria` | `sand` |
| `ship` | `hull`, `hatch`, `bow`, `stern`, `bowsprit`, `lantern` |
| `crypt` | `floor-worn`, `floor-shadow`, `wall-face`, `wall-niche`, `wall-carving`, `wall-base`, `rubble`, `cobweb`, `cobweb-ne` |
| `meadow` | `ford`, `hedge`, `stepping` |
| `mirewood` | `cliff-top`, `cliff-face`, `cliff-base` |
| `quarry` | `floor-shadow`, `black-vein-floor`, `wall-face`, `wall-roots`, `wall-crystal`, `wall-base`, `rubble` |
| `vault` | `floor-shadow`, `floor-cracked`, `sun-crest`, `pit`, `wall-face`, `wall-crest`, `wall-niche`, `wall-base`, `earth-face`, `rubble`, `cobweb`, `cobweb-ne` |
| `keep` | `marble-gold` |
| `forest` | `grass`, `leaves`, `road`, `undergrowth`, `moss`, `tuft`, `fern`, `flowers`, `pebbles`, `roots`, `ruts`, `litter` |

---

## 6. Index

Every room is **P0**: the Prologue to Act 1 Chapter 4 uses all of them. So the order column is the priority: the order the story first reaches each room, with each side room (a shop, a home, the pier, the barrow) placed in the chapter where it first opens. Make them top to bottom.

| # | Room | Name | Chapter | Width | Panels | Send as | Foreground | Priority |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | `gulls-wager/deck` | The Main Deck | Prologue | 3200 | 3 | `gulls-wager-deck-1.png`, `gulls-wager-deck-2.png`, `gulls-wager-deck-3.png` | none | P0 |
| 2 | `docks/harbourside` | The Harbourside | Chapter 1 | 3200 | 3 | `docks-harbourside-1.png`, `docks-harbourside-2.png`, `docks-harbourside-3.png` | `docks-harbourside-fg-1.png`, `docks-harbourside-fg-2.png`, `docks-harbourside-fg-3.png` | P0 |
| 3 | `docks/fish-market` | The Fish Market | Chapter 1 | 3200 | 3 | `docks-fish-market-1.png`, `docks-fish-market-2.png`, `docks-fish-market-3.png` | none | P0 |
| 4 | `docks/pier` | The Pier | Chapter 1 | 1600 | 1 | `docks-pier-1.png` | none | P0 |
| 5 | `questeria/gate-plaza` | The Gate Plaza | Chapter 1 | 3200 | 3 | `questeria-gate-plaza-1.png`, `questeria-gate-plaza-2.png`, `questeria-gate-plaza-3.png` | `questeria-gate-plaza-fg-1.png` | P0 |
| 6 | `questeria/festival-square` | The Festival Square | Chapter 1 | 3200 | 3 | `questeria-festival-square-1.png`, `questeria-festival-square-2.png`, `questeria-festival-square-3.png` | `questeria-festival-square-fg-1.png`, `questeria-festival-square-fg-2.png`, `questeria-festival-square-fg-3.png` | P0 |
| 7 | `questeria/lantern-market` | The Lantern Market | Chapter 1 | 3200 | 3 | `questeria-lantern-market-1.png`, `questeria-lantern-market-2.png`, `questeria-lantern-market-3.png` | `questeria-lantern-market-fg-1.png` | P0 |
| 8 | `questeria/sunspire-steps` | The Sunspire Steps | Chapter 1 (reachable); Chapter 4 (the keep and the vault) | 1600 | 1 | `questeria-sunspire-steps-1.png` | none | P0 |
| 9 | `questeria/thornbridge-gate` | The Thornbridge Gate | Chapter 1 (reachable); Chapter 3 | 1600 | 1 | `questeria-thornbridge-gate-1.png` | `questeria-thornbridge-gate-fg-1.png` | P0 |
| 10 | `gilded-flagon/taproom` | The Taproom | Chapter 1 (reachable); Chapter 4 | 2432 | 2 | `gilded-flagon-taproom-1.png`, `gilded-flagon-taproom-2.png` | none | P0 |
| 11 | `shop/inside` | The Chandler's | Chapter 1 | 1600 | 1 | `shop-inside-1.png` | none | P0 |
| 12 | `forge/inside` | The Smithy | Chapter 1 | 1600 | 1 | `forge-inside-1.png` | none | P0 |
| 13 | `bakery/inside` | The Bakery | Chapter 1 | 1600 | 1 | `bakery-inside-1.png` | none | P0 |
| 14 | `chapel/inside` | The Chapel | Chapter 1 | 1600 | 1 | `chapel-inside-1.png` | none | P0 |
| 15 | `cottage-a/inside` | A Cottage | Chapter 1 | 1600 | 1 | `cottage-a-inside-1.png` | none | P0 |
| 16 | `cottage-b/inside` | A Cottage | Chapter 1 | 1600 | 1 | `cottage-b-inside-1.png` | none | P0 |
| 17 | `warehouse/inside` | A Warehouse | Chapter 1 | 1600 | 1 | `warehouse-inside-1.png` | none | P0 |
| 18 | `road/farmland` | The Farmland | Chapter 1 | 3200 | 3 | `road-farmland-1.png`, `road-farmland-2.png`, `road-farmland-3.png` | `road-farmland-fg-1.png`, `road-farmland-fg-2.png`, `road-farmland-fg-3.png` | P0 |
| 19 | `road/fork` | The Fork | Chapter 1 | 3200 | 3 | `road-fork-1.png`, `road-fork-2.png`, `road-fork-3.png` | `road-fork-fg-1.png`, `road-fork-fg-2.png`, `road-fork-fg-3.png` | P0 |
| 20 | `barrow/steps` | The Barrow Steps | Chapter 1 | 1600 | 1 | `barrow-steps-1.png` | none | P0 |
| 21 | `barrow/hall` | The Barrow Hall | Chapter 1 | 3200 | 3 | `barrow-hall-1.png`, `barrow-hall-2.png`, `barrow-hall-3.png` | none | P0 |
| 22 | `barrow/side-room` | The Side Room | Chapter 1 | 1600 | 1 | `barrow-side-room-1.png` | none | P0 |
| 23 | `mirewood/edge` | The Mirewood's Edge | Chapter 2 | 3200 | 3 | `mirewood-edge-1.png`, `mirewood-edge-2.png`, `mirewood-edge-3.png` | `mirewood-edge-fg-1.png`, `mirewood-edge-fg-3.png` | P0 |
| 24 | `mirewood/deep` | The Deep Wood | Chapter 2 | 3200 | 3 | `mirewood-deep-1.png`, `mirewood-deep-2.png`, `mirewood-deep-3.png` | `mirewood-deep-fg-1.png`, `mirewood-deep-fg-2.png`, `mirewood-deep-fg-3.png` | P0 |
| 25 | `mirewood-quarry/lip` | The Quarry's Lip | Chapter 2 | 3200 | 3 | `mirewood-quarry-lip-1.png`, `mirewood-quarry-lip-2.png`, `mirewood-quarry-lip-3.png` | none | P0 |
| 26 | `mirewood-quarry/cut` | The Second Cut | Chapter 2 | 1600 | 1 | `mirewood-quarry-cut-1.png` | none | P0 |
| 27 | `mirewood-quarry/floor` | The Quarry Floor | Chapter 2 | 3200 | 3 | `mirewood-quarry-floor-1.png`, `mirewood-quarry-floor-2.png`, `mirewood-quarry-floor-3.png` | none | P0 |
| 28 | `mirewood/camp` | The Rangers' Camp | Chapter 2 | 1600 | 1 | `mirewood-camp-1.png` | `mirewood-camp-fg-1.png` | P0 |
| 29 | `thornbridge/camp-gate` | The Camp Gate | Chapter 3 | 1600 | 1 | `thornbridge-camp-gate-1.png` | none | P0 |
| 30 | `thornbridge/tents` | The Tents | Chapter 3 | 3200 | 3 | `thornbridge-tents-1.png`, `thornbridge-tents-2.png`, `thornbridge-tents-3.png` | none | P0 |
| 31 | `thornbridge/river-edge` | The River's Edge | Chapter 3 | 1600 | 1 | `thornbridge-river-edge-1.png` | `thornbridge-river-edge-fg-1.png` | P0 |
| 32 | `old-crown-vault/entrance` | The Antechamber | Chapter 4 | 3200 | 3 | `old-crown-vault-entrance-1.png`, `old-crown-vault-entrance-2.png`, `old-crown-vault-entrance-3.png` | none | P0 |
| 33 | `old-crown-vault/halls` | The Long Hall | Chapter 4 | 3200 | 3 | `old-crown-vault-halls-1.png`, `old-crown-vault-halls-2.png`, `old-crown-vault-halls-3.png` | none | P0 |
| 34 | `old-crown-vault/strongroom` | The Relic Gallery | Chapter 4 | 3200 | 3 | `old-crown-vault-strongroom-1.png`, `old-crown-vault-strongroom-2.png`, `old-crown-vault-strongroom-3.png` | none | P0 |
| 35 | `old-crown-vault/treasury` | The Treasury | Chapter 4 | 1600 | 1 | `old-crown-vault-treasury-1.png` | none | P0 |
| 36 | `sunspire-keep/great-hall` | The Great Hall | Chapter 4 | 3200 | 3 | `sunspire-keep-great-hall-1.png`, `sunspire-keep-great-hall-2.png`, `sunspire-keep-great-hall-3.png` | none | P0 |
| 37 | `rusty-anvil/bar` | The Bar | Chapter 4 | 1600 | 1 | `rusty-anvil-bar-1.png` | none | P0 |

**Totals:** 19 zones, 37 rooms, 74 background panels (18 one-panel rooms, 1 two-panel room, 18 three-panel rooms), 22 optional foregrounds, 90 floor tiles in 9 tilesets.

---

## 7. For Claude: importing what the owner sends

The owner sends each PNG in chat under the name its entry gives. Put it where the import tools look, credit it, and import it:

| Sent as | Save as |
| --- | --- |
| `<zone>-<room>-<i>.png` | `tools/source/rooms/<zone>/<room>/bg-<i>.png` |
| `<zone>-<room>-fg-<i>.png` | `tools/source/rooms/<zone>/<room>/fg-<i>.png` |
| `tile-<tileset>-<tile>.png` | `tools/source/tiles/<tileset>/<tile>.png` |

Zone and room ids both contain hyphens, so split the name by the zone list (§6), not on the first hyphen: `mirewood-quarry-lip-1.png` is `mirewood-quarry/lip`, panel 1; `gulls-wager-deck-2.png` is `gulls-wager/deck`.

Then:

```text
python tools/rooms/cut_room.py <zone>/<room>
python tools/rooms/cut_tiles.py <tileset>
python tools/rooms/check_room_sources.py
```

and add one line per file to `ASSETS.md` in the style of the other supplied pictures (`tools/check_assets.py` fails on any file under `tools/source/` it doesn't name), for example: `tools/source/rooms/docks/pier/bg-1.png - the Pier, background panel 1; made with ChatGPT from Story/14-background-prompts.md, supplied by Caleb Kiatoukaysi for Legend Quest`.

**The panels.** ChatGPT makes 1536 × 1024 pictures. A room W canvas px wide (1600 to 4800, 900 tall) is N panels: N = 1 for W ≤ 1600, else ceil((W − 1600) / 1400) + 1. Each covers 1600 canvas px; consecutive panels overlap by 200; the last is right-aligned, so a 3200 room's panels start at 0, 1400 and 1600, and its panel 3 overlaps panel 2 by 1400 px (87.5%) with only 200 px new, and the 2432 taproom's panel 2 starts at 832 and overlaps panel 1 by 768. `sourcekit.fade_window` cross-fades each panel into the one before across the middle 200 px of their overlap: room x 1400 to 1600 between panels 1 and 2 of a 3200 room, x 2200 to 2400 between panels 2 and 3, and x 1116 to 1316 in the taproom. Only the centre 1536 × 864 band of each picture is used (16:9, 0.96 image px to a canvas px); the top and bottom 80 px are bleed. So canvas y 600, the floor's back edge, is image y 656; the template's horizon (150 px above the floor's back edge, y 450 in most rooms) is image y 512; a 120 × 200 door is 115 × 192 image px.

**If the owner's chat can't reproduce a long overlap,** the last panel can be made from the one before: shift that picture left by the panel offset in image px (192 for a 3200 room's panel 3), and ask ChatGPT to paint only the empty strip at the right.
