# Brief: LegendQuest in detailed pixel art (what ChatGPT paints, and how Claude puts it in the game)

The owner's direction (2026-11-20): move LegendQuest from smooth painted art to **detailed, fantasy pixel art** while
keeping everything else: the side-view rooms, the jointed skeleton and its animations, mix-and-match armour, and the
heroic high-fantasy feel. Add **detailed, animated conversation portraits** with many expressions, keep the player's
custom look in **cutscenes**, and make sure **monsters, enemies and backgrounds all move**.

The work is split between two assistants:

| Who | Does |
| --- | --- |
| **ChatGPT** (image generation) | Paints every picture, from the prompts in this file, on the templates Claude makes. One picture per message. |
| **Claude** (in this repository) | Makes the templates, converts every picture to true pixel art (fixed pixel grid, limited palette, clean outlines), cuts and rigs it, animates it, and wires it into the game, with checks. |

Why the split: an image model paints well but its "pixel art" is not real pixel art (pixel sizes drift, colours
blur, nothing lines up between pictures). Converting painted art automatically only works if the painting was made
for it: a test on the current painterly armour (Brannock, 2026-11-20) came out muddy, because brush texture turns
into noise when it's shrunk. So ChatGPT paints **pixel-ready** art (flat colour areas, hard edges, bold shapes, no
texture), and Claude's converter turns it into clean pixel art.

Read §0 and §1 before painting anything. §2 has the blocks pasted into every prompt. §3 to §7 are the prompt cards by
kind. §8 is Claude's implementation plan. §9 lists every file and where it goes.

---

## 0. Rules

1. **Original content only.** LegendQuest takes the *feel* of AdventureQuest Worlds (side-view rooms, the
   three-quarter character angle, heroic fantasy gear, real-time skills) and nothing it owns. **No prompt ever names a
   game, studio, artist or company**, and no other game's pictures are ever attached to a prompt or traced.
2. **Templates first.** Every part, portrait and creature is painted on a template Claude makes (§8, step 2), so the
   pieces line up. A picture painted on a blank canvas can't be cut. If a template a card asks for doesn't exist yet,
   ask Claude for it.
3. **The size lock still applies.** ChatGPT must return each picture at exactly the template's size and layout. If it
   comes back cropped, zoomed or shifted, use the **Fix the size** and **Realign** follow-ups from
   `Story/13-art-prompts.md` §1.2. Claude checks every picture before cutting it.
4. **Name files exactly as the card says**, and put them where §9 says. Claude's tools find them by name.
5. **Every file is credited.** Claude lists each picture in `ASSETS.md` as "supplied by Caleb Kiatoukaysi, made in
   ChatGPT", with the tool that converted it, as for the current art.
6. **The old art keeps working** until the pixel art replaces it: the game gets a switch (§8, step 1), and both looks
   run side by side until the owner signs the pixel look off.
7. **Game balance doesn't change.** Damage lands on each animation's `hit` and `release` times
   (`Combat/Realtime/MotionTimings.cs`). The pixel look changes how poses are *drawn*, never those times.

---

## 1. The target look, in numbers

| | Value | Why |
| --- | --- | --- |
| **World resolution** | The world (rooms, characters, monsters, effects) is drawn at **800 × 450 art pixels** and blown up ×2, with hard edges, to the 1600 × 900 screen | Big enough for detail; a whole-number scale, so every pixel is a clean square |
| **HUD and text** | Drawn on top at full 1600 × 900, sharp | Readability; the HUD is not pixelated |
| **Standard character** | **98 art pixels** tall, feet to crown (the same on-screen size as now, 196 screen px) | Room for real armour detail: plate edges, trims, a face |
| **Conversation portrait** | **128 × 128 art pixels**, drawn ×2 (256 px) in the dialogue box, ×3 (384 px) in cutscene close-ups | Detailed faces with readable expressions |
| **Animation** | Poses step at **12 frames a second**, like hand-drawn sprites; movement across the floor stays smooth; limbs turn in 64 fixed angle steps with a pixel-art rotation filter | Keeps the pixel look crisp while the skeleton drives everything |
| **Palette** | About 32 colours per character, 48 per room, chosen per picture by the converter; shadows shift toward violet-blue, highlights toward warm gold, by fixed amounts | Consistent, rich colour; the standard pixel-art ramp |
| **Outlines** | One dark pixel, deep brown-violet (`#2a1f3d` mixed with the colour it borders), never pure black; lighter inside the figure | A clean read against detailed rooms |
| **Light** | From above and slightly in front, never from the side, so a right-facing figure can be mirrored to face left | As now (`Story/03-art-overhaul.md` §2.3) |

**More fantasy.** The bright, slightly anime look moves toward a richer high-fantasy look: deeper jewel tones,
burnished gold and steel, cool moonlit shadows against warm torchlight, glowing runes and gems, ornate but readable
gear, and semi-realistic faces with honest age and character. It's still heroic and colourful, not grimdark.

---

## 2. The shared blocks

Paste the right style block first, then the avoid list, then the size lock (`Story/13-art-prompts.md` §2.3, tall or
wide to match the template), then the card. **These blocks replace `Story/13`'s and `Story/14`'s style blocks for the
pixel look.** Their old avoid list says "Pixel art"; don't use it with these cards.

### 2.1 Pixel-ready style block (characters' parts, creatures, rooms, props)

> STYLE. Original high-fantasy art for a 2D side-view role-playing game, painted to be converted into detailed pixel
> art, so it must be made of large, clean, flat areas of colour with hard edges, like a master pixel artist's design
> blown up. Bold, even outlines in deep brown-violet, never pure black, about 12 to 16 pixels thick on the outer
> silhouette and half that inside. Cel shading with hard-edged shapes only: for each material a base tone, two shadow
> tones that shift toward cool violet-blue, one highlight tone that shifts toward warm gold, and a thin, bright rim
> light along the top and back edges; polished metal gets a few crisp, bright highlights. No gradients, no brush
> texture, no noise, no grain, no soft airbrushing, no hairline scratches; nothing smaller than about one hundredth
> of the figure's height. Show detail with a few bold shapes instead: plate edges, rivets as single round dots, trims as
> solid bands, stitching as short dashes, one engraved or glowing motif. Rich, saturated high-fantasy colour: deep jewel
> tones, burnished gold and steel, cool shadows and warm light, with gems, runes and magic glowing. Heroic but
> believable proportions: an adult is about five and a quarter heads tall, with real hands and fingers; not chibi.
> Lit from above and slightly in front of the subject, never from the left or right.

### 2.2 Pixel portrait style block (conversation portraits only)

> STYLE. A detailed, semi-realistic fantasy pixel-art portrait for a role-playing game's conversation window, head and
> shoulders. Real pixel art: the picture is a grid of 128 by 128 art pixels, each drawn as an 8 by 8 block of one flat
> colour, with no anti-aliasing, blur or gradient inside a block. About 32 colours in hue-shifted ramps: shadows cooler
> and more violet, highlights warmer and more golden. A selective dark outline in deep brown-violet, never pure black.
> Deliberate single-pixel highlights in the eyes, on the lips, on metal and on gems; dithering only on soft cloth or
> fur. Expressive, semi-realistic faces with real bone structure, honest age and character, eyes that are not
> oversized, and hair in clean, clumped locks with two or three tones. High-fantasy costume detail at the collar and
> shoulders. Three-quarter view facing the right-hand side of the picture, both eyes showing, lit from above and
> slightly in front. Plain, flat background in the exact colour I name.

### 2.3 Avoid list (paste after either style block)

> AVOID. Blurry, soft or painterly rendering; airbrushed gradients; photographic texture, film grain or noise;
> glossy 3D-render shading; pure-black outlines; tiny hairline detail. Chibi or super-deformed proportions, huge anime
> eyes. A front view, a flat profile or a back view unless I ask. Scenery, a floor, a ground shadow, a frame or a border,
> unless I ask. Any text, letters, numbers, signature or watermark. Extra or missing fingers. Anything copied from an
> existing game, film, comic or brand.

### 2.4 Backgrounds

Every card says which flat background to use. **Characters' parts and creatures:** solid black, as the current
templates. **Portraits:** flat `#00FF00` green (it never appears in a face, so it keys out cleanly). **Life sheets,
foregrounds and isolated room layers:** flat `#FF00FF` magenta.

---

## 3. Characters (the skeleton and mix-and-match armour)

**Nothing about the skeleton or customisation changes.** Every person is still the shared base body wearing an armour
painted in 13 pieces (one per bone), a cape, a helm or hairstyle, extras and held items, with the head's features from
a face. The game still puts the pieces on the skeleton and plays the same clips. Only the art inside each piece
becomes pixel art. Armour from one set and a helm from another still mix freely, because every piece is cut to the
same bones.

### 3.1 How

1. **First, nothing is repainted.** Claude converts every existing part to pixel art (§8, step 3), and the owner
   looks at the result in-game. Parts that convert cleanly (plain cloth, simple leather) stay as they are.
2. **Parts that come out muddy are repainted** in the pixel-ready style, on the **same templates and the same cards**
   as before (`Story/13-art-prompts.md` §3 to §6), with one change: paste §2.1 and §2.3 of this file instead of
   Story/13's style block and avoid list. Return the picture under the same file name; Claude re-converts it.
3. Claude's converter flags the muddiest parts (§8, step 3) and writes the repaint list, worst first, to
   `shots/pixel/repaint-list.md`.

### 3.2 Card: repaint a part in the pixel-ready style · `<part-id>.png`

> [§2.1 style block] [§2.3 avoid list] [Size (tall) or Size (wide) from Story/13 §2.3]
>
> Repaint the attached template exactly as the part card below describes, in the style above. Keep every piece,
> figure and grey line exactly where it is, at exactly its size and angle; paint over the shapes that are there.
> Keep the costume's design, colours and details from the card, but render them as bold, flat, hard-edged shapes.
>
> [The part's description, copied from its card in Story/13-art-prompts.md]
>
> I'll save it as `<part-id>.png`.

### 3.3 Faces on the figure in the world

At 98 pixels tall a figure's face is about 12 pixels across, so it shows eyes, brows and mouth as a few pixels each.
Claude makes these small faces from the existing face parts and from the portraits' expressions (§4). The world
figure shows the same expression as the line being spoken. **Disgust is new** (§4.2), so each of the twenty shared
faces needs a disgust edit using Story/13's **Expressions** follow-up (§1.2), with this description:
"disgust (eyes narrowed, brows lowered and pulled together, nose wrinkled, the upper lip raised on one side)".

---

## 4. Conversation portraits (detailed, with expressions, animated)

### 4.1 What a portrait is

A **portrait kit** is a set of 128 × 128 pixel busts (top of the head to mid-chest) of one character: one picture per
expression, all lined up exactly, so the game can swap between them and animate them. They're painted on the
**portrait template** (`portrait-template-male.png`, `portrait-template-female.png`, 1024 × 1024; Claude makes them).
The template shows the bald mannequin's head and shoulders in grey, with guide lines for the eye line, the chin, the
collar line and the safe area. Speakers on the left of the dialogue box face right; on the right they're mirrored.

### 4.2 The expressions

The scripts already use twelve expressions (`Story/03-art-overhaul.md` §5.2, `Dialogue/Expressions.cs`). The owner's
list (anger, disgust, happy, laughing, sadness) maps onto them, with **disgust** added as the thirteenth:

| Expression | Owner's word | What the face does (this wording goes in the prompts) |
| --- | --- | --- |
| `neutral` | | Calm: eyes open, brows level, mouth closed and relaxed |
| `smile` | **happy** | Warm: eyes softened, cheeks lifted, a gentle closed-mouth smile |
| `grin` | happy (big) | Delighted: brows raised, eyes half closed with crinkles at the corners, a wide grin showing the upper teeth |
| `laugh` | **laughing** | Laughing: eyes squeezed shut in happy curves, brows raised, mouth wide open showing teeth and tongue, cheeks high |
| `smirk` | | Knowing: eyes narrowed, one brow raised, one corner of the mouth pulled up |
| `sad` | **sadness** | Sad: eyes lowered and wet along the lower lids, the inner ends of the brows raised, the corners of the mouth turned down |
| `worried` | | Worried for someone else: eyes open, brows knit and tilted up in the middle, mouth flat and tight |
| `afraid` | | Afraid for yourself: eyes wide with the whites showing, brows tilted up, mouth open, the face pale |
| `angry` | **anger** | Angry: brows pulled hard down and together with a deep crease between them, eyes narrowed and glaring, nostrils flared, teeth gritted, the jaw set |
| `cold` | | Cold: eyes narrowed and still, brows level, mouth a flat line, no warmth at all |
| `tired` | | Tired: eyes half closed with heavy lids, brows level, mouth slack, faint shadows under the eyes |
| `shock` | | Shocked: eyes wide, brows raised high, mouth dropped open |
| `disgust` | **disgust** (new) | Disgusted: eyes narrowed, brows lowered and pulled together, nose wrinkled, the upper lip raised on one side, the head drawn slightly back |

**Who gets which** (keeps the number of pictures sensible):

| Tier | Who | Expressions |
| --- | --- | --- |
| Main | The player's and {sb}'s face presets (§4.6), the companions and the major characters of `Story/03` §7.4 (Sera, Mira, Kael, Brannock, Valencia, Elara, Maleficio, Varkhul, Severin, Crane, Thessaly, Vorlaith) | All 13 |
| Named | Every other named speaker (Corra, Tomas, Grell, Gannet, Hester, Tobin and the rest) | `neutral`, `smile`, `sad`, `angry`, `shock`, `disgust` (others fall back to the nearest) |
| Generic | Unnamed townsfolk, guards, cultists | Built from the shared kit (§4.6), `neutral`, `smile` and `angry` |

A missing expression falls back down a chain (`grin` → `smile` → `neutral`, `afraid` → `worried` → `sad`, and so on),
so a kit works before it's finished.

### 4.3 How a portrait animates

Each expression is **one painted picture**. Claude's tools derive every moving part from it, so ChatGPT only paints
the expressions:

| Motion | How it's made | What it does in the game |
| --- | --- | --- |
| **Blink** | Claude finds the eyes (by comparing with the template's eye line) and draws closed and half-closed lids from the picture's own skin and lash colours | Every 3 to 6 s: half, closed, half over 0.12 s. None in `cold` and `shock`; a slow blink in `sad` and `tired` |
| **Talking** | Claude finds the mouth and draws two open shapes (part open, open) in the picture's own lip, teeth and mouth colours, following the expression's mouth (a smile talks smiling, a frown talks frowning) | While a line types, the mouth cycles at about 10 a second and closes on punctuation |
| **Breathing** | Claude splits the bust at the template's collar line into head and body | The body rises a pixel every 3 s or so, with the head following a frame later |
| **Changing expression** | | The swap is hidden behind a blink, with a one-pixel dip of the head, so faces never pop |
| **Per-expression motion** | | `laugh`: the head bobs a pixel, twice. `angry`: a 1-pixel shake as it lands. `shock`: the head jumps up 2 pixels and settles. `afraid`: a faint tremble. `sad`: the head sinks a pixel. `disgust`: the head draws back a pixel. `cold`: perfectly still |
| **Emote marks** | One painted sheet (§4.7), animated by Claude | Small marks by the head: anger vein (pulses), sweat drop (slides), tears (drip), blush, sparkle, gloom lines, a question or exclamation mark, a sigh puff |

If a derived blink or talking mouth looks wrong on a face (an unusual face such as Vorlaith's glass, a veil, a beard
over the mouth), ChatGPT paints that frame as an edit (§4.5), and the painted one wins.

### 4.4 Card: a character's portrait · `portrait-<character>.neutral.png`

Attach `portrait-template-male.png` or `portrait-template-female.png`.

> [§2.2 pixel portrait style block, background #00FF00] [§2.3 avoid list]
>
> SIZE. The attached picture is exactly 1024 × 1024 pixels. Give it back at exactly 1024 × 1024. Paint the character
> over the grey mannequin exactly where it is: the eyes on the eye line, the chin on the chin line, the shoulders
> filling the bottom edge, the top of the head (or hair, or hat) inside the safe area. Don't crop, zoom, re-centre or
> re-frame it.
>
> Paint this character with a neutral expression (calm: eyes open, brows level, mouth closed and relaxed):
> [the character's description: age, build, people (Sylvan, Emberkin, Stoneborn, Wildkin or human), skin, hair, eyes,
> scars and marks, and what shows at the collar and shoulders. Take it from `Story/03-art-overhaul.md` §7.4 and the
> character's card in `Story/13-art-prompts.md` §7, so the portrait matches the figure in the world.]
>
> I'll save it as `portrait-<character>.neutral.png`.

### 4.5 Follow-ups for portraits (in the same chat)

**Expression** · `portrait-<character>.<expression>.png`

> Edit the attached portrait: change ONLY the expression, to the one I name below. Move only the eyes, eyebrows, nose,
> mouth and cheeks, and the head no more than the expression needs. The hair, the clothes, the colours, the pixel grid,
> the size and the position of everything else stay exactly the same. Keep it real pixel art on the same 128 by 128
> grid, on the same flat #00FF00 background. Give it back at exactly 1024 × 1024.
> The expression: [the row's description from §4.2]. I'll save it as `portrait-<character>.<expression>.png`.

**Blink override** (only if Claude asks) · `portrait-<character>.<expression>.blink.png`

> Edit the attached portrait: close the eyes gently, as in the middle of a blink, and change nothing else, pixel for
> pixel. Same size, grid and background. I'll save it as `portrait-<character>.<expression>.blink.png`.

**Talk override** (only if Claude asks) · `portrait-<character>.<expression>.talk.png`

> Edit the attached portrait: open the mouth as if in the middle of saying "ah", keeping the expression's mouth shape
> (smiling, frowning, gritted), and change nothing else, pixel for pixel. Same size, grid and background. I'll save it
> as `portrait-<character>.<expression>.talk.png`.

**Variant look** (a character whose look changes, e.g. `sister-vale` to `valencia-unhooded`, `kael-shadow`, Brannock
without his beard clasps) · `portrait-<variant>.neutral.png`

> Edit the attached portrait: [the change]. Keep the face, the expression, the pixel grid, the size and the position
> exactly the same. I'll save it as `portrait-<variant>.neutral.png`.

Then do the expressions on the variant as above. A variant only needs the expressions its scenes use; Claude lists them.

### 4.6 Customised characters (the player and {sb}): the layered portrait kit

The player and {sb} are both made on the creation screen, so their portraits can't be single paintings. They're built
from **layers** painted on the same portrait template, so every choice the player makes shows in conversations and
cutscenes:

| Layer (back to front) | What's painted | How many |
| --- | --- | --- |
| `hair-back` | Hair that falls behind the head and shoulders | One per hairstyle (the hair library of `Story/13` §4.9) |
| `collar` | What the armour shows at the neck and shoulders | One per armour (`armor-…`), so changing armour changes the portrait |
| `face` | The head and neck with the face's features, **one picture per expression** | 6 face presets per body (12 in all), all 13 expressions each |
| `hair-front` | Hair over the forehead, ears and face | One per hairstyle |
| `extras` | Beards, earrings, scars, freckles, ears and horns of the peoples | One per extra (`Story/13` §4.10) |
| `helm` | Helms, hats and hoods; it hides `hair-front` and may hide `hair-back` | One per helm |

**Colours.** Face presets are painted in the base's warm tan skin and hair is painted in plain mid-brown. Claude's
converter maps those onto **swappable colour ramps**, so the skin tone, hair colour and eye colour chosen at creation
recolour every layer at once. Armour colours come from the same dyes as the figure in the world.

**The same kit makes the generic NPCs:** a seeded mix of face, hair, collar, helm and colours gives every townsperson
a portrait, so nobody speaks with a blank box.

Cards for the kit (all on the portrait template, background `#00FF00` for faces and `#FF00FF` for every other layer):

**Face preset** · `pface-<m|f><nn>.<expression>.png`

> [§2.2] [§2.3] [the size lock of §4.4]
>
> Paint only a head and neck over the grey mannequin, exactly where it is: no hair at all (the head is bald, smooth),
> no clothes (the neck ends at the collar line), warm tan skin. [The face's description: age, face shape, nose, eyes,
> brows, lips, any lines.] Neutral expression. I'll save it as `pface-<m|f><nn>.neutral.png`.

Then the **Expression** follow-up (§4.5) for each of the 13.

**Hairstyle** · `phair-<id>.png`

> [§2.2] [§2.3] [the size lock of §4.4]
>
> On the attached template, paint only this hairstyle on the bald mannequin's head, in plain mid-brown with its
> shading, and make everything that isn't hair flat #FF00FF magenta, including the face and the neck. [The hairstyle's
> description from Story/13 §4.9.] I'll save it as `phair-<id>.png`.

Claude splits each hairstyle into `hair-back` and `hair-front` at the head's outline.

**Collar** · `pcollar-<armor-id>.png`, attach the template and a picture of the armour

> [§2.2] [§2.3] [the size lock of §4.4]
>
> On the attached template, paint only what this armour shows at the neck, shoulders and upper chest, matching the
> second picture's design, colours and trims, and make everything else (the head, the neck above the collar line, the
> background) flat #FF00FF magenta. I'll save it as `pcollar-<armor-id>.png`.

**Helm** · `phelm-<id>.png` and **Extra** · `pextra-<id>.png`: as the hairstyle card, painting only that helm, hat or
hood (with the head's top hidden under it), or only that beard, jewel, scar or people's ears or horns.

### 4.7 Card: emote marks · `emotes-01.png`

Attach `emote-sheet.png` (1536 × 1024, a grid of 12 cells; Claude makes it).

> [§2.1] [§2.3] [Size (wide) from Story/13 §2.3]
>
> Paint one small fantasy emote mark in each cell of the attached grid, centred, on flat #FF00FF magenta, each a bold,
> simple shape that reads at 16 pixels across: 1 an anger vein mark in red, 2 a sweat drop in pale blue, 3 a single
> tear, 4 a blush of two pink ovals with three short lines, 5 a four-pointed gold sparkle, 6 three wavy gloom lines in
> violet-grey, 7 a question mark, 8 an exclamation mark, 9 a puff of sigh breath, 10 a small red heart, 11 a dark
> scribble cloud of frustration, 12 three rising "z" marks for sleep. Keep each inside its cell with a margin.
> I'll save it as `emotes-01.png`.

---

## 5. Cutscenes

### 5.1 Customisation carries through every cutscene

- **In the room**, cutscenes play on the same skeleton figures as the rest of the game, so the player and {sb} always
  wear exactly what they've equipped, in the colours they chose, with the same animations (`Story/03` §4.3).
- **In conversations**, the layered kit (§4.6) builds their portraits from their current look and armour.
- **In painted story scenes** (backdrops), any picture that shows the player or {sb} leaves a gap where Claude draws
  their actual figure in the right pose (as `Story/03` §10 already says), so a story painting never shows the wrong
  character.

### 5.2 What's new for cutscenes

| Feature | What it does |
| --- | --- |
| **Close-ups** | A cue puts a speaker's portrait large (×3) in the middle of the screen with letterbox bars, for big moments (a confession, a betrayal, a death). It uses the same kit, so the player's close-up is their own face |
| **Emotes in the world** | A cue puts an emote mark (§4.7) over a figure's head in the room, animated |
| **Faces in the world** | The figure's small face takes the line's expression, as the portrait does |
| **Animated story paintings** | Each backdrop is painted in 3 to 5 layers that drift at different speeds, with moving details (rain, sea, smoke, flicker, drifting embers), made as in §7 |

### 5.3 Card: a story painting in layers · `backdrop-<id>.<layer>.png`

Story paintings are listed in `Story/03-art-overhaul.md` §10 and `Story/15-asset-prompts.md`. Paint the whole scene
first, then isolate its layers with edits in the same chat (this keeps the layers matching):

> [§2.1 style block, but for a full scene: replace "a 2D side-view role-playing game" with "a story scene in a 2D
> role-playing game"] [§2.3 avoid list, without "Scenery"]
>
> Paint this scene as a wide 1536 × 1024 picture: [the scene's description from Story/03 §10]. [If the player or {sb}
> is in it: Leave the place where <who> stands as a clear, empty space of plain ground or sky, about <size> tall, at
> <where>; their figure is added later.] I'll save it as `backdrop-<id>.whole.png`.

Then, one per layer, from back to front:

> Edit the attached picture: keep ONLY the [sky / distant scenery / middle ground / near foreground / figures], exactly
> as they are, pixel for pixel, and make everything else flat #FF00FF magenta. Where the removed things hid part of
> this layer, continue this layer's scenery naturally behind them. Same size. I'll save it as
> `backdrop-<id>.<sky|far|mid|near|figures>.png`.

---

## 6. Monsters and enemies (all animated)

### 6.1 How each kind moves

| Kind | Who | How it's animated |
| --- | --- | --- |
| **Humanoid foes** | The Hollowed, cultists, drowned, wights, constructs, the Unmade, Protectorate soldiers, bandits | The person skeleton with an armour kit, exactly as the party. They already have every clip; repainting their armours (§3) is all they need |
| **Rigged creatures** | Gloomfang, rot-boar, ash-hound, rockjaw, the Thornmother (quadruped); dire bat (flier); glass scorpion (arthropod) | **New creature skeletons** (`Story/03` §9). Each creature is painted in pieces on its skeleton's template, like the body, and Claude animates it with clips: idle (breathing, tail and ear motion), walk, run, two attacks, a special, the telegraph wind-up, hurt, stagger, death, and an entrance. Bosses add a phase change and an enraged idle |
| **Spirits and things without a body** | Sea-wraiths, the Hungering, wisps, Vorlaith's light | Layered pieces that wobble, stretch and drift, with colour cycling on their glow and particle trails |
| **Huge things** | The leviathan, the Thornmother at full size | Rigged at scale, or as a moving shadow and parts across the room |

Every monster also gets, from Claude: a pixel hit flash (brightened inside its outline, not a white blob), a dithered
dissolve on death, glowing eyes and runes that pulse by colour cycling, and its attacks' effects drawn as pixel art.

### 6.2 Card: a creature's look · `creature-<id>.look.png`

Paint the look first, so the pieces have something to match.

> [§2.1] [§2.3] [Size (wide) from Story/13 §2.3]
>
> Paint this creature whole, in a side three-quarter view facing the right-hand side of the picture, in a relaxed,
> ready stance, on solid black, filling about two thirds of the picture's height: [the creature's description from
> Story/03 §9 or the encounter's notes, its size next to a standard adult, its colours, and what makes its silhouette
> unmistakable]. I'll save it as `creature-<id>.look.png`.

### 6.3 Card: the creature in pieces · `creature-<id>.parts.png`

Attach the skeleton's parts template (`creature-parts-<quadruped|flier|arthropod>.png`, 1536 × 1024; Claude makes it:
the skeleton's pieces pulled apart, each a grey shape with its joints marked) **and** the look picture from §6.2.

> [§2.1] [§2.3] [Size (wide) from Story/13 §2.3]
>
> Paint the creature in the second picture onto the first picture's grey pieces, one piece per grey shape, exactly
> where each shape is and at exactly its size and angle: the body, the neck, the head, the jaw, each leg's upper and
> lower part and foot, and each part of the tail [adjust to the template's labels]. Each piece is that part of the
> creature alone, painted all the way to the edge of its grey shape, including the part that will be hidden under the
> next piece at the joint, so nothing gaps when it bends. Keep the solid black background between the pieces.
> I'll save it as `creature-<id>.parts.png`.

### 6.4 Card: a monster's extras · `creature-<id>.fx.png`

For anything that isn't a body part: a breath weapon, a thrown spine, a summoned glyph, a boss's second-phase look.

> [§2.1] [§2.3] [Size (wide)]
>
> On flat #FF00FF magenta, in the four cells of a 2 × 2 grid, paint: [1 to 4 items, each described]. Each one alone,
> centred in its cell, with a margin. I'll save it as `creature-<id>.fx.png`.

---

## 7. Backgrounds (rooms, all animated)

### 7.1 What changes

Rooms are painted the same way as now (`Story/12-rooms.md` §3.5, `Story/14-background-prompts.md`), in 1536 × 1024
panels on Claude's room guides, with three changes:

1. **The pixel-ready style block** (§2.1, with "a 2D side-view role-playing game's room background, the scenery only,
   no people") replaces Story/14's style block.
2. **Each panel is split into layers**, so the room has depth when the camera moves: `sky`, `far`, `mid` (buildings,
   walls, doors, the floor) and `fg` (things in front of the people). Each is isolated from the whole panel by an edit,
   as in §5.3. Today's rooms are one flat layer; this brings back the parallax of `Story/12` §3.1.
3. **Each room gets a life sheet** of things that move (§7.3).

At 800 art pixels per screen, a panel shrinks to about half its size, so rooms keep a lot of detail: cobbles, roof
tiles, shop signs and posters stay readable.

### 7.2 Card: isolate a room layer · `bg-<i>.<sky|far|mid|fg>.png`

Paint the panel first with Story/14's card for it and §2.1 above (saved as `bg-<i>.png`), then, in the same chat:

> Edit the attached picture: keep ONLY the [sky and clouds / distant scenery (hills, the sea, far towers and rooftops)
> / the buildings, walls, doors, props and floor of the middle ground / the things nearest to us that stand in front of
> where people walk], exactly as they are, pixel for pixel, and make everything else flat #FF00FF magenta. Where the
> removed things hid part of this layer, continue this layer naturally behind them. Same size, same layout.
> I'll save it as `bg-<i>.<layer>.png`.

### 7.3 How rooms move

Every room has at least three moving things (`Story/12` §3.2). Claude animates them with these methods. Only the last
needs extra painting:

| Method | Used for | Made by |
| --- | --- | --- |
| **Parallax** | Sky, far and foreground drift against the middle as the camera moves | The layers of §7.2 |
| **Colour cycling** | Water shimmer, waterfalls, lava, glowing runes, lit windows, magic circles, candle-lit glass | Claude marks the areas in the room's `life.json` and cycles their colours, the classic pixel-art way; nothing extra to paint |
| **Sway and ripple** | Flags, banners, hanging signs, lanterns, washing lines, leaves, grass, cloaks on hooks, reeds | A cut-out from the life sheet, bent a pixel at a time by Claude |
| **Flicker and glow** | Torches, candles, braziers, hearths, forges | A cut-out flame plus Claude's flicker and soft light (the current torch glow) |
| **Particles** | Rain, snow, ash, embers, sea spray, dust motes, falling leaves, fireflies, chimney smoke | Claude's particle systems, drawn as pixel sprites |
| **Short loops** | Birds flapping, a cat's tail, a waterwheel turning, a fishing float bobbing | A 3- or 4-frame strip on the life sheet, or a cut-out Claude turns or bobs |

### 7.4 Card: a room's life sheet · `life-<n>.png`

Attach `life-sheet.png` (1536 × 1024, a 4 × 3 grid; Claude makes it). Claude lists each room's needs in
`shots/pixel/life-needs.md`.

> [§2.1, with "a 2D side-view role-playing game's room details"] [§2.3] [Size (wide)]
>
> On flat #FF00FF magenta, paint one item in each cell of the attached grid, alone, centred, uncropped, with a margin,
> at the size it would be in this room next to a person 98 pixels tall: [cell 1: …, cell 2: …, up to 12]. For an item
> that loops (a bird, a waterwheel), paint its frames one per cell in a row, left to right, the same size and in the
> same place in each cell, changing only the moving part. I'll save it as `life-<n>.png`.

---

## 8. Claude's side: building it into the game

In order. Each step ends with checks, and nothing is committed without `python tools/check_assets.py` passing.
Commits go on a feature branch under the owner's identity with no co-author lines; no pushing or pull requests unless
the owner asks.

### Step 1: The pixel look switch and renderer

- `ArtSettings.Look = Painted | Pixel`, read from `settings.json` and toggled on the options screen's debug page. The
  proof runs (`--proof`) run in both until the owner signs pixel off.
- In pixel mode, `Screens/ExploreScreen.cs` and `ExploreScreen.CombatDraw.cs` draw the world (room layers, figures,
  props, particles, trails, flying things) into an 800 × 450 target with the camera halved and every position snapped
  to whole art pixels, then blow it up ×2 with point sampling onto `ScreenCanvas`. The HUD, text, popups and panels
  draw over it at full resolution, as now.
- `Characters/WalkerSprite.cs` samples clip poses at 12 frames a second in pixel mode (positions on the floor stay
  smooth). Combat timing is read from `clips.json` as before and doesn't change.
- Limb pieces turn in 64 steps, using rotations computed once per piece with a pixel-art rotation filter (upscale,
  turn, pick the majority colour), so turning limbs don't shimmer.
- **Check:** the build is clean (0 warnings), `--validate` and `--simulate selftest` pass, and `--simulate all 200` gives
  the same results in both looks. Proof shots of the Arena and Boss stages in both looks.

### Step 2: Templates

- `tools/pixel/templates.py` writes `portrait-template-male.png` and `-female.png` (head and shoulders of the
  mannequin with the eye line, chin line, collar line and safe area), `emote-sheet.png`, `life-sheet.png`, and the
  creature parts templates (`creature-parts-quadruped.png`, `-flier.png`, `-arthropod.png`) from the new creature
  skeletons in `tools/rig/skeleton.py`.
- **Check:** each template is exactly a size ChatGPT makes, and a test cut of the bare template gives the right pieces.

### Step 3: The converter

- `tools/pixel/pixelate.py` with modes `parts`, `portrait`, `room`, `life`, `creature`, `check`. It keys the background,
  flattens painted texture, picks a palette per picture with the fixed violet-shadow and gold-highlight shift, shrinks
  by majority colour per block (not averaging), snaps pictures that are already pixel art to their own grid, removes
  stray pixels, and adds the selective outline. Rooms get ordered dithering in the sky only.
- It reserves colour ramps for skin, hair and eyes (and the existing dyes) so they can be recoloured.
- It writes a repaint list, worst first, by a "mud" score (how many colour clusters are smaller than 3 pixels).
- `tools/import_parts.py` gets a `--pixel` path that builds pixel atlases next to the current ones.
- **Check:** the converted P0 cast side by side, standing and in six combat clips, in `shots/pixel/lineup.png`; the
  owner judges it.

### Step 4: Portraits

- `tools/pixel/portrait_kit.py` builds each kit: keys the green, snaps to the 128 grid, finds the eyes and mouth against
  the template, draws the blink and talk frames, splits head from body at the collar line, and builds the layered kit's
  ramps. Painted overrides win.
- C#: a `PortraitKit` that draws a speaker's portrait from their kit at the line's expression, with the blink, talking,
  breathing, expression change and per-expression motion of §4.3, and emote marks. `Screens/DialogueScreen.cs`'s
  portrait grows from 120 to 256 pixels. The layered kit builds the player's, {sb}'s and generic NPCs' portraits from
  their look and armour.
- Add `disgust` to `Dialogue/Expressions.cs`, the script check and `Story/03` §5.2, so scripts can use `(disgust)`.
- **Check:** every expression of the P0 kits, filmed talking and blinking, in `shots/pixel/portraits/`; a dialogue
  scene with the player's portrait changing armour.

### Step 5: Cutscenes

- Cues `closeup <actor> <expression>`, `closeup off` and `emote <actor> <mark>`, documented in `Story/03` §4.4.
- Layered backdrops with drift and moving details, and the player's and {sb}'s figures drawn into their gaps.
- **Check:** the Prologue's story scenes play with a customised player in both looks.

### Step 6: Creatures

- The quadruped, flier and arthropod skeletons (Python and C#), the creature clips in `tools/rig/combat_clips.py`
  (clips the fights need, each with `hit` and `release` at the times the current sheet poses use, so balance holds),
  and `python tools/rig/clips.py --check` extended to them. Then Gloomfang, the Thornmother and the rest replace their
  sheets.
- **Check:** every creature's clips on contact sheets (`tools/rig/clip_strip.py`) and in the Arena proof;
  `--simulate all 200` unchanged.

### Step 7: Rooms

- `tools/rooms/cut_room.py` cuts the layered panels and converts them; each room gets `life.json` (colour cycles, sways,
  flickers, particles, loops) read by `World/Rooms/LifeLoops.cs`.
- **Check:** every P0 room in `shots/pixel/rooms/` and a short capture of each moving thing; `tools/rooms/check_rooms.py`
  passes.

### Step 8: Docs

Update `README.md`, `Story/03-art-overhaul.md` (§1, §2, §5, §9, §10), `Story/12-rooms.md` §3, and point
`Story/13`/`14`/`15`'s cards to this file's style blocks once the owner signs the look off.

---

## 9. Where every picture goes

| Picture | File | Folder |
| --- | --- | --- |
| A repainted part | the same name as now | `tools/source/pixel/repaints/` (Claude moves it into place once it's approved) |
| The templates (Claude makes them) | `portrait-template-<male\|female>.png`, `emote-sheet.png`, `life-sheet.png`, `creature-parts-<rig>.png` | `tools/source/pixel/templates/` |
| A character's portrait | `portrait-<character>.<expression>[.blink\|.talk].png` | `tools/source/pixel/portraits/<character>/` |
| A face preset | `pface-<m\|f><nn>.<expression>.png` | `tools/source/pixel/portrait-kit/faces/` |
| A hairstyle, collar, helm, extra | `phair-<id>.png`, `pcollar-<armor-id>.png`, `phelm-<id>.png`, `pextra-<id>.png` | `tools/source/pixel/portrait-kit/<hair\|collars\|helms\|extras>/` |
| Emote marks | `emotes-01.png` | `tools/source/pixel/emotes/` |
| A story painting | `backdrop-<id>.<whole\|sky\|far\|mid\|near\|figures>.png` | `tools/source/pixel/backdrops/<id>/` |
| A creature | `creature-<id>.<look\|parts\|fx>.png` | `tools/source/pixel/creatures/<id>/` |
| A room panel and its layers | `bg-<i>.png`, `bg-<i>.<sky\|far\|mid\|fg>.png` | `tools/source/pixel/rooms/<zone>/<room>/` |
| A room's life sheet | `life-<n>.png` | `tools/source/pixel/rooms/<zone>/<room>/` |

---

## 10. The order of work

**Phase 0 (Claude, before any painting):** Steps 1 to 3: the switch, the renderer, the templates and the converter on
the existing art. The owner looks at the converted game and decides which parts need repainting.

**Phase 1 (ChatGPT, then Claude): the opening.** Everything the Prologue to Act 1 Chapter 4 needs (P0):

1. Six face presets per body, all 13 expressions (the player and {sb}), and the hairstyles, collars and helms the
   creation screen offers.
2. Portraits for the P0 cast: Brannock, Sera, Mira, Kael, Corra, Tomas, Grell and the Prologue's speakers.
3. The emote sheet.
4. Repaints of the P0 armours the converter flags, worst first.
5. Gloomfang and the Thornmother, look and pieces.
6. The Gull's Wager and Saltwind Docks rooms in layers, with their life sheets.

**Phase 2:** the rest of Act 1 (P1). **Phase 3:** Acts 2 and 3 (P2), with the story paintings in the order the
scripts reach them.

Send pictures to Claude in batches (a character's whole portrait kit, a room's panels and layers): Claude converts a
batch, shows the result in-game, and says what (if anything) to repaint before the next.
