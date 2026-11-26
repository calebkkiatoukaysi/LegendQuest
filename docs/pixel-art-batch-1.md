# Pixel art, batch 1: the pilot (ready-to-paste ChatGPT prompts)

This is the first batch of art for the pixel-art look (`docs/pixel-art-brief.md`). It's a **pilot**: one or two of
each kind of picture, so Claude can test the converter on every kind before the big batches. The prompts are filled
in, so nothing needs editing. Send the results to Claude when the batch is done, or a section at a time.

**How to use it.** Each card says what to **attach**, what to **paste** (one block, copied whole), and what to
**save the picture as**. Start a **new chat** where the card says so, and stay in the **same chat** for its follow-ups
(that keeps the face, the grid and the layout the same). One picture per message. If a picture comes back cropped,
zoomed or moved, use **Fix the size** (end of this file) and do it again.

**Where the files go.** Save each picture under `tools/source/pixel/`, in the folder the card names. The templates
are in `tools/source/pixel/templates/`.

| § | What | Pictures | Tests |
| --- | --- | --- | --- |
| 1 | Brannock's front-facing close-up and five expressions | 6 | A whole, unique character portrait |
| 2 | The player's face presets (a man and a woman), five expressions each | 12 | The layered kit's faces |
| 3 | One hairstyle and one armour collar | 2 | The layered kit's layers |
| 4 | Brannock's armour, repainted pixel-ready | 1 | Repainting a rig part |
| 5 | Gloomfang, the whole creature | 1 | A creature's look |
| 6 | The emote marks | 1 | Overlay marks |
| 7 | The Gull's Wager deck, panel 1, its four layers and its life sheet | 6 | A layered, animated room |

---

## 1. Brannock's portrait

Brannock's card is **`docs/pixel-art-portraits/brannock.md`**; every character has one in that folder: front-facing, chest-up,
anime-flavoured pixel close-ups. For the pilot, paint only these six from his card, in one chat: the neutral
(`portrait-brannock.neutral.png`), then `angry`, `disgust`, `smile`, `laugh` and `sad`. Folder:
`tools/source/pixel/portraits/brannock/`.

Send the six to Claude before painting anyone else's, so the prompts can be fixed once, not forty times.

---

## 2. The player's face presets

These are faces for the player and {sb}. They're painted **bald, with no clothes and in plain tan skin**, because the
hair, the armour and the colours are separate layers the player chooses. Folder:
`tools/source/pixel/portrait-kit/faces/`.

### 2.1 The man · `pface-m01.neutral.png`

**New chat.** **Attach:** `templates/portrait-template-male.png`.

```text
STYLE. A pixel-art character portrait for a cosy high-fantasy role-playing game's dialogue box: a close-up with the face looking straight at the viewer, the head square to the camera and centred. Anime-flavoured and charming: large, expressive eyes with a big coloured iris, a dark pupil and one or two bright square highlights; clear, bold eyebrows; a small, simple nose shown with a pixel or two of shadow; a small mouth whose shape changes clearly with the mood. Real low-resolution pixel art: the whole picture is a grid of about 96 by 96 big square pixels, each about 10 to 11 image pixels wide, every one a single flat colour, with no anti-aliasing, blur, gradient or texture inside it. A small palette in ramps of three or four tones: shadows cooler and more violet, highlights warmer and more golden. A one-pixel outline in dark brown-violet, never pure black, around the head and neck, with lighter coloured lines inside. Soft, colourful cel shading, lit from above and slightly in front. Normal proportions, not chibi. Plain, flat background in exactly #00FF00 green, right up to the edges.

AVOID. Smooth, painterly, vector or high-resolution rendering; small or uneven pixels; gradients; photographic texture or noise; glossy 3D-render shading; pure-black outlines. Chibi or super-deformed proportions. A three-quarter view, a profile, a tilted head or eyes looking away. Any hair on the head, any beard, any clothes, any jewellery. Sweat drops, anger marks or blush lines beside the face. Any text, letters, numbers, signature or watermark. Anything copied from an existing game, film, comic or brand.

SIZE. The attached template is exactly 1024 by 1024 pixels. Give it back at exactly 1024 by 1024. Paint over the grey mannequin's head and neck exactly where they are, at exactly their size, facing straight out: the eyes on the dashed line across the middle of the head, either side of the dashed centre line; the chin on the dashed line just under the face. The dashed lines are only guides: paint over them. Don't crop, zoom, re-centre or re-frame it.

PAINT ONLY a head, ears and neck: the head completely bald and smooth, no hair at all (the hair is a separate layer), only the eyebrows; the neck ends at the dotted line. Make the shoulders, everything below the dotted line, and all of the background flat #00FF00 green. The skin is a plain warm tan, the eyes mid brown, the brows mid brown: the game recolours them for each player.

FACE. A young man of about twenty-four: an open, likeable face, a straight nose, bright, steady eyes under straight, expressive brows, a friendly mouth, clean-shaven, a firm jaw. A hero's face.

EXPRESSION. Neutral: calm, eyes open looking straight at the viewer, brows level, mouth closed and relaxed.

I'll save it as pface-m01.neutral.png.
```

Then the five expressions in the same chat, as in §1.2, with this paste (and the table's lines, saving as
`pface-m01.<expression>.png`):

```text
Edit the last picture you made: change ONLY the expression, to the one below. Move only the eyes, eyebrows, nose, mouth and cheeks, and the head no more than the expression needs. The bald head, the skin colour, the size and the position of everything else stay exactly the same, the face keeps looking straight at the viewer, and everything that isn't the head and neck stays flat #00FF00 green. Keep it real pixel art on the same grid of big pixels. Give it back at exactly 1024 by 1024.
EXPRESSION: Angry: brows pulled hard down and together with a deep crease between them, eyes narrowed and glaring, nostrils flared, teeth gritted, the jaw set.
I'll save it as pface-m01.angry.png.
```

| Save as | EXPRESSION line |
| --- | --- |
| `pface-m01.angry.png` | Angry: brows pulled hard down and together with a deep crease between them, eyes narrowed and glaring, nostrils flared, teeth gritted, the jaw set. |
| `pface-m01.disgust.png` | Disgusted: eyes narrowed into a flat, unimpressed look, brows lowered and pulled together, nose wrinkled, the upper lip raised on one side in a sneer, the head drawn slightly back. |
| `pface-m01.smile.png` | Happy: eyes softened, cheeks lifted, a warm closed-mouth smile. |
| `pface-m01.laugh.png` | Laughing: eyes squeezed shut into happy upside-down arcs, brows raised, mouth wide open showing the upper teeth and tongue, cheeks high, the head tipped back a little. |
| `pface-m01.sad.png` | Sad: eyes lowered and glistening with a wet line along the lower lids, the inner ends of the brows raised, the corners of the small mouth turned down, the head dipped a little. |

### 2.2 The woman · `pface-f01.neutral.png`

**New chat.** **Attach:** `templates/portrait-template-female.png`. Paste §2.1's first block with the **FACE** line
changed to the one below and the last line to `I'll save it as pface-f01.neutral.png.`:

```text
FACE. A young woman of about twenty-four: an open, likeable face, a small straight nose, bright, warm eyes with bold upper lashes, straight brows, a friendly mouth, high cheekbones. A hero's face.
```

Then the five expressions as in §2.1, saving as `pface-f01.<expression>.png`.

---

## 3. One hairstyle and one collar

Folder: `tools/source/pixel/portrait-kit/hair/` and `.../collars/`.

### 3.1 Short curls · `phair-04.png`

**New chat.** **Attach:** `templates/portrait-template-male.png`.

```text
STYLE. A layer for an anime-flavoured pixel-art portrait in a cosy high-fantasy role-playing game's dialogue box, seen straight from the front. Real low-resolution pixel art: a grid of about 96 by 96 big square pixels, each about 10 to 11 image pixels wide and one flat colour, with no anti-aliasing, blur or gradient. Hair in big, clean, chunky locks with three tones and a bright highlight band. Shadows cooler and more violet, highlights warmer and more golden. A one-pixel outline in dark brown-violet, never pure black. Lit from above and slightly in front.

AVOID. Blur, gradients, noise, painterly rendering, pure-black outlines. A face, a beard, clothes. Any text or watermark.

SIZE. The attached template is exactly 1024 by 1024 pixels. Give it back at exactly 1024 by 1024, with the hair fitted to the grey mannequin's head exactly where it is, seen straight from the front, symmetrical about the dashed centre line, at exactly the head's size. Don't crop, zoom, re-centre or re-frame it.

PAINT ONLY this hairstyle on the head: short, tight curls close to the head, the hairline natural at the forehead, the temples and around the ears, which show. Paint it a plain mid brown with its shading: the game recolours it for each player. Make EVERYTHING that isn't hair flat #FF00FF magenta: the face, the ears, the neck, the shoulders and the background.

I'll save it as phair-04.png.
```

### 3.2 Brannock's armour at the collar · `pcollar-armor-brannock.png`

**New chat.** **Attach:** `templates/portrait-template-male.png`, and `tools/source/characters/sheets/armor-brannock.png`.

```text
STYLE. A layer for an anime-flavoured pixel-art portrait in a cosy high-fantasy role-playing game's dialogue box, seen straight from the front. Real low-resolution pixel art: a grid of about 96 by 96 big square pixels, each about 10 to 11 image pixels wide and one flat colour, with no anti-aliasing, blur or gradient. Bold, simple shapes for trims, rivets and plate edges; one-pixel highlights on metal. Shadows cooler and more violet, highlights warmer and more golden. A one-pixel outline in dark brown-violet, never pure black. Lit from above and slightly in front.

AVOID. Blur, gradients, noise, painterly rendering, pure-black outlines. A head, a face, hair. Any text or watermark.

SIZE. The first attached picture is the template, exactly 1024 by 1024 pixels. Give it back at exactly 1024 by 1024, the clothes fitted to the grey mannequin's shoulders and chest exactly where they are, seen straight from the front, at exactly their size. Don't crop, zoom, re-centre or re-frame it.

PAINT ONLY what this armour shows at the neck, shoulders and upper chest, from the dotted line down to the bottom edge, matching the second picture's design, colours and trims: a charcoal padded work shirt with its collar, a chain-mail shirt over it, a battered iron breastplate edged in tarnished gold, dented and scratched, and one heavy iron pauldron on his left shoulder (on the right of the picture). Make EVERYTHING else flat #FF00FF magenta: the head, the neck above the dotted line, and the background.

I'll save it as pcollar-armor-brannock.png.
```

---

## 4. Brannock's armour, repainted pixel-ready · `armor-brannock.png`

This replaces his current armour picture, to compare how a repainted part converts against the current one. **New
chat.** **Attach:** `tools/source/characters/armor-parts-man.png`. Save it in `tools/source/pixel/repaints/` (not over
the current one).

```text
STYLE. Original high-fantasy art for a 2D side-view role-playing game, painted to be converted into detailed pixel art, so it must be made of large, clean, flat areas of colour with hard edges, like a master pixel artist's design blown up. Bold, even outlines in deep brown-violet, never pure black, about 12 to 16 pixels thick on the outer silhouette and half that inside. Cel shading with hard-edged shapes only: for each material a base tone, two shadow tones that shift toward cool violet-blue, one highlight tone that shifts toward warm gold, and a thin, bright rim light along the top and back edges; polished metal gets a few crisp, bright highlights. No gradients, no brush texture, no noise, no grain, no soft airbrushing, no hairline scratches; nothing smaller than about one hundredth of the figure's height. Show detail with a few bold shapes instead: plate edges, rivets as single round dots, trims as solid bands, stitching as short dashes, one engraved motif. Rich, saturated high-fantasy colour: deep jewel tones, burnished gold and steel, cool shadows and warm light. Lit from above and slightly in front, never from the left or right.

AVOID. Blurry, soft or painterly rendering; airbrushed gradients; photographic texture, film grain or noise; glossy 3D-render shading; pure-black outlines; tiny hairline detail. Any text, letters, numbers, signature or watermark. Anything copied from an existing game, film, comic or brand.

SIZE. The attached picture is exactly 1024 by 1536 pixels. Give it back at exactly 1024 by 1536, laid out exactly as it is: every piece in exactly the same place, at exactly the same size and the same angle. Don't crop, pad, zoom, stretch, squash, rotate, flip, re-centre or re-frame the picture, add no border or frame, and draw nothing bigger, smaller, longer or shorter than it is: paint over the shapes that are there, and keep the solid black background between them.

Dress the pieces in Brannock's soldier-smith kit: a charcoal padded work shirt with the sleeves pushed up; a chain-mail shirt over it to the hips, its rings shown as a bold, regular pattern of dark and light; a battered iron breastplate with tarnished-gold edging, the dents shown as a few bold highlight-and-shadow shapes; one heavy iron pauldron on the near shoulder; a thick dark-leather apron-like skirt to the knees over dark trousers, like a smith's; a wide belt with a big iron buckle and a battered metal flask; heavy iron-capped boots. Any bare skin keeps the body's warm tan.

I'll save it as armor-brannock.png.
```

---

## 5. Gloomfang · `creature-gloomfang.look.png`

**New chat.** **Attach:** nothing. Folder: `tools/source/pixel/creatures/gloomfang/`.

```text
STYLE. Original high-fantasy art for a 2D side-view role-playing game, painted to be converted into detailed pixel art, so it must be made of large, clean, flat areas of colour with hard edges, like a master pixel artist's design blown up. Bold, even outlines in deep brown-violet, never pure black, about 12 to 16 pixels thick on the outer silhouette and half that inside. Cel shading with hard-edged shapes only: a base tone, two shadow tones that shift toward cool violet-blue, one highlight tone that shifts toward warm gold, and a thin, bright rim light along the top and back edges. No gradients, no brush texture, no noise, no grain, no soft airbrushing; nothing smaller than about one hundredth of the creature's length. Fur drawn as bold, clumped, spiky locks, a few big shapes rather than many strands. Lit from above and slightly in front.

AVOID. Blurry, soft or painterly rendering; airbrushed gradients; photographic texture or noise; glossy 3D-render shading; pure-black outlines. Scenery, a floor, a ground shadow, a frame. Any text or watermark. Anything copied from an existing game, film, comic or brand.

CREATURE. A gloomfang, a forest wolf twisted by dark magic: a gaunt, rangy wolf, long-legged and narrow-hipped, its ribs showing under its coat, standing in a low, stalking stance with its head down and forward, in a three-quarter side view facing the right-hand side of the picture. Fur: dark grey-brown with a cold navy cast and a paler, dusty underside, bristling into a ragged mane along the neck, shoulders and spine. Head: a long, narrow muzzle with the lips drawn back over long yellowed fangs; ears flat back; pale violet glowing eyes with thin trails of black smoke drifting back over the head. Long, dark claws on big feet. A thin, ragged tail held low. Hungry, silent, about to lunge.

OUTPUT. One landscape PNG, exactly 1536 by 1024, on a solid black background, the whole creature centred with its feet near the bottom, filling about 85% of the width, nothing cropped. In the game its shoulder comes to a standard adult's hip, and nose to tail it's a little longer than an adult is tall.

I'll save it as creature-gloomfang.look.png.
```

---

## 6. The emote marks · `emotes-01.png`

**New chat.** **Attach:** `templates/emote-sheet.png`. Folder: `tools/source/pixel/emotes/`.

```text
STYLE. Original fantasy game art painted to be converted into pixel art: bold, simple, flat shapes with hard edges, bold deep brown-violet outlines (never pure black), one shadow tone and one highlight tone per colour, no gradients, no texture, no noise.

AVOID. Blur, gradients, noise, pure-black outlines, any text or letters other than the question and exclamation marks, a frame, a watermark.

SIZE. The attached picture is exactly 1536 by 1024 pixels: a 4 by 3 grid of cells on flat #FF00FF magenta. Give it back at exactly 1536 by 1024, laid out exactly as it is. Paint one mark in each cell, centred, inside the cell's dashed margin, filling about two thirds of the cell; keep the background flat #FF00FF magenta, with the grid lines and dashed margins painted over in the same magenta.

The marks, left to right, top row first: 1 an anger vein mark, four bulging red curves in a cross; 2 a sweat drop, pale blue with a white highlight; 3 a single falling tear, pale blue; 4 a blush, two soft pink ovals each with three short diagonal lines; 5 a four-pointed gold sparkle; 6 three wavy gloom lines falling, violet-grey; 7 a bold question mark, white with a dark outline; 8 a bold exclamation mark, white with a dark outline; 9 a small white puff of sigh breath; 10 a small red heart; 11 a dark, tangled scribble cloud of frustration; 12 three rising sleep marks shaped like the letter Z, pale blue, getting smaller.

I'll save it as emotes-01.png.
```

---

## 7. The Gull's Wager deck, panel 1, in layers

The room's first panel, painted pixel-ready, then split into its layers so the room has depth, then its moving things.
**New chat.** Folder: `tools/source/pixel/rooms/gulls-wager/deck/`.

### 7.1 The whole panel · `bg-1.png`

**Attach:** `shots/art/room-templates/gulls-wager-deck-1.png` (Claude made it; ask if it's missing).

```text
STYLE. An original background painting for a side-on 2D fantasy role-playing game, painted to be converted into detailed pixel art, so it must be made of clean, flat areas of colour with hard edges. Bold, even outlines in deep brown-violet, never pure black, on every building, prop and plank. Cel shading with hard-edged shapes: a base tone, two shadow tones that shift toward cool violet-blue, one highlight tone that shifts toward warm gold; the sky in a few broad, flat bands of colour rather than a smooth gradient. Detail shown with bold shapes: plank seams, rivets as dots, coiled rope as a few bold loops, sails as a few big folds. No brush texture, no noise, no grain, no soft airbrushing, no tiny detail. Rich high-fantasy colour: deep navy and violet night, burnished gold and brass, warm lamplight against cool moonlight. The distance is paler, cooler and simpler. The camera is side-on at a standing person's eye level, looking straight at the scene: the horizon straight and level, nothing vanishing into the distance.

ROOM. The Gull's Wager: The Main Deck. This is panel 1 of 3, the left part of a room that scrolls sideways; the scene carries on past its right edge.

LAYOUT, as the attached guide shows it. The floor line is straight and level about 64% of the way down the picture (y 656 of 1024). Below it, down to the bottom of the picture, is the open floor band, with nothing standing on it: plain, even deck planks running left to right. A person standing on the floor would be about 185 pixels tall. The far horizon is a straight, level line about halfway down the picture (y 512 of 1024). The room's left end is closed.

SCENE, left to right. The Gull's Wager, a trim merchant ship, two days out at sea at night. From the left edge to about 20%: the bow, the forecastle's raised deck behind the floor with a short ladder up to it, its rail sweeping up to the left, and the bowsprit angling up out of the top-left corner with its stays. Along the whole back of the floor runs the ship's far bulwark, about waist-high to a person: dark planks between stanchions under a varnished cap rail trimmed in navy and gold, pin rails hung with neatly coiled ropes, and a small brass lamp on a bracket at about 79%, lit. At about 50% the foremast rises from behind the bulwark out of the top of the picture, its sails furled tight on two yards, its shrouds and ratlines slanting down to the rail either side. Keep the right edge, around 95% to 100%, clear of masts. Over the bulwark: a calm, dark sea with long, soft swells to a level horizon, and a full moon high at about 25% across, silver-white, its path of broken light lying on the water under it. Sky: deep navy shading to violet at the horizon in flat bands, scattered stars, two or three long clouds edged in moonlight. Quiet, lonely, a little uneasy; the sea is very big. Paint at clear, readable mid values: the game darkens the room and adds the lamp's glow itself.

OUTPUT. One landscape PNG, exactly 1536 by 1024: the whole scene from sky to floor, edge to edge, painted right through the top and bottom 80 pixels of bleed.

AVOID. People, animals, birds, creatures. Text, letters, numbers, runes, labels, signatures, watermarks. Frames, borders, vignettes. Perspective: vanishing points, a view from above or below, a tilted horizon. Anything on the open floor band. Painterly blur, smooth gradients, airbrushing, glossy 3D-render shading, photorealism, grain, lens flare, dense noise or tiny texture. Pure-black outlines. The layout guide's flat colours, lines or labels. Anything copied from an existing game, film, comic or brand.

I'll save it as bg-1.png.
```

### 7.2 The four layers (same chat, one message each, **attaching `bg-1.png` to every message**)

```text
Edit the attached picture (bg-1.png, the whole panel): keep ONLY the sky: the night sky, the stars, the clouds and the moon, exactly as they are, pixel for pixel. Make everything else flat #FF00FF magenta. Where the ship, the sea and the masts hid part of the sky, continue the sky naturally behind them, down to the horizon. Same size, exactly 1536 by 1024. I'll save it as bg-1.sky.png.
```

```text
Edit the attached picture (bg-1.png, the whole panel): keep ONLY the distant scenery: the sea from the horizon down to the top of the bulwark, with the moon's path of light on it, exactly as it is, pixel for pixel. Make everything else flat #FF00FF magenta, including the sky. Where the bulwark, the masts and the rigging hid part of the sea, continue the sea naturally behind them. Same size, exactly 1536 by 1024. I'll save it as bg-1.far.png.
```

```text
Edit the attached picture (bg-1.png, the whole panel): keep ONLY the ship: the deck, the bulwark, the forecastle and its ladder, the bowsprit, the foremast with its sails and rigging, the coiled ropes and the lamp, exactly as they are, pixel for pixel. Make the sky and the sea flat #FF00FF magenta. Same size, exactly 1536 by 1024. I'll save it as bg-1.mid.png.
```

```text
Edit the attached picture (bg-1.png, the whole panel): is there anything that would stand in front of people walking on the deck, at the very bottom edge (the near rail's top, a coil of rope, the edge of a hatch)? If so, add one or two such things along the bottom edge only, low and dark, covering no more than a tenth of the floor band, and make everything else flat #FF00FF magenta. If not, return a picture that is entirely flat #FF00FF magenta. Same size, exactly 1536 by 1024. I'll save it as bg-1.fg.png.
```

### 7.3 The deck's moving things · `life-1.png`

**New chat.** **Attach:** `templates/life-sheet.png`.

```text
STYLE. Original high-fantasy art for a 2D side-view role-playing game's ship at night, painted to be converted into detailed pixel art: clean, flat areas of colour with hard edges, bold deep brown-violet outlines (never pure black), a base tone, two shadow tones shifting toward violet-blue and one highlight tone shifting toward warm gold, no gradients, no texture, no noise. Lit from above and slightly in front; the lamplight warm, the moonlight cool.

AVOID. Blur, gradients, noise, pure-black outlines, people, any text or watermark, a frame.

SIZE. The attached picture is exactly 1536 by 1024 pixels: a 4 by 3 grid of cells on flat #FF00FF magenta. Give it back at exactly 1536 by 1024, laid out exactly as it is. Paint one item in each cell, alone, centred, inside the cell's dashed margin, uncropped; keep the background flat #FF00FF magenta, with the grid lines and dashed margins painted over in the same magenta. Paint each item at the size it would be on a ship where a person stands about 185 pixels tall.

The items, left to right, top row first: 1 a small brass ship's lamp hanging from a short iron bracket, lit, its glass glowing warm; 2 the same lamp's flame alone, a teardrop of warm yellow and white; 3 a navy-and-gold pennant flag on a short rope, flying out to the right; 4 a furled sail's loose corner of canvas with its tie, hanging; 5 a single coil of rope hanging from a belaying pin; 6 a long, thin strip of the moon's reflected light on dark water, a broken line of silver flecks; 7, 8 and 9 the same small seagull in flight, seen from the side facing right, in three wing positions, wings up, level, and down, the same size and in the same place in each cell; 10 a wisp of grey-blue sea mist, a soft flat shape; 11 a wooden block-and-tackle pulley on a short rope; 12 a small tied bundle of canvas sacks.

I'll save it as life-1.png.
```

---

## Fix the size (use when a picture comes back wrong)

**Attach the template again** (not the picture that came back), and paste:

```text
That came back at a different size or layout from the picture I attached: something is cropped, zoomed, moved, or drawn bigger or smaller than it was. Do the same edit again on the attached picture. It is exactly the size it is: give it back at exactly that size, laid out exactly as it is, every piece, figure and line in exactly the same place, at exactly the same size and angle. Don't crop, pad, zoom, stretch, rotate, flip, re-centre or re-frame it, and add no border. Keep the same flat background colour.
```

## When the batch is done

Tell Claude. Claude will convert every picture to true pixel art, put the portraits in a test viewer animated
(blinking, talking, changing expression), compare the repainted armour with the current one in-game, rig Gloomfang's
look into a parts template for batch 2, and say what worked, what to change in the prompts, and what's next.
